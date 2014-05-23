using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.AR.Overrides.ARDocumentRelease;

namespace PX.Objects.CM
{
	[TableAndChartDashboardType]
	public class RevalueARAccounts : PXGraph<RevalueARAccounts>
	{
		public PXCancel<RevalueFilter> Cancel;
		public PXFilter<RevalueFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessing<RevaluedARHistory, RevalueFilter, Where<boolTrue, Equal<boolTrue>>, OrderBy<Asc<RevaluedARHistory.accountID, Asc<RevaluedARHistory.subID, Asc<RevaluedARHistory.customerID>>>>> ARAccountList;
		public PXSelect<CurrencyInfo> currencyinfo;
		public PXSetup<ARSetup> arsetup;
		public PXSetup<CMSetup> cmsetup;
		public PXSetup<Company> company;

		public RevalueARAccounts()
		{
			object setup = arsetup.Current;
			setup = cmsetup.Current;
			if (cmsetup.Current.MCActivated != true) 
				throw new Exception(Messages.MultiCurrencyNotActivated);
			ARAccountList.SetProcessCaption(Messages.Revalue);
			ARAccountList.SetProcessAllVisible(false);

			PXUIFieldAttribute.SetEnabled<RevaluedARHistory.finPtdRevalued>(ARAccountList.Cache, null, true);
		}

		protected virtual void RevalueFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			RevalueFilter filter = (RevalueFilter)e.Row;
			if (filter != null)
			{
				ARAccountList.SetProcessDelegate(
					delegate(List<RevaluedARHistory> list)
					{
						Revalue(filter, list);
					}
				);
			}
		}

		protected virtual void RevalueFilter_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (((RevalueFilter)e.Row).CuryEffDate != null)
			{
				((RevalueFilter)e.Row).CuryEffDate = ((DateTime)((RevalueFilter)e.Row).CuryEffDate).AddDays(-1);
			}
		}

		protected virtual void RevalueFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.SetDefaultExt<RevalueFilter.lastARFinPeriodID>(e.Row);
		}

		protected virtual void RevalueFilter_FinPeriodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<RevalueFilter.curyEffDate>(e.Row);

			if (((RevalueFilter)e.Row).CuryEffDate != null)
			{
				((RevalueFilter)e.Row).CuryEffDate = ((DateTime)((RevalueFilter)e.Row).CuryEffDate).AddDays(-1);
			}
			ARAccountList.Cache.Clear();
		}

		protected virtual void RevalueFilter_CuryEffDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARAccountList.Cache.Clear();
		}

		protected virtual void RevalueFilter_CuryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARAccountList.Cache.Clear();
		}

		protected virtual void RevalueFilter_TotalRevalued_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				decimal val = 0m;
				foreach (RevaluedARHistory res in ARAccountList.Cache.Updated)
				{
					if ((bool)res.Selected)
					{
						val += (decimal)res.FinPtdRevalued;
					}
				}
				e.ReturnValue = val;
				e.Cancel = true;
			}
		}

		public virtual IEnumerable araccountlist()
		{
			foreach (PXResult<ARHistoryByPeriod, RevaluedARHistory, Account, Customer> res in PXSelectJoin<ARHistoryByPeriod,
				InnerJoin<RevaluedARHistory, 
				On<RevaluedARHistory.customerID, Equal<ARHistoryByPeriod.customerID>, 
				And<RevaluedARHistory.branchID, Equal<ARHistoryByPeriod.branchID>, 
				And<RevaluedARHistory.accountID, Equal<ARHistoryByPeriod.accountID>, 
				And<RevaluedARHistory.subID, Equal<ARHistoryByPeriod.subID>, 
				And<RevaluedARHistory.curyID, Equal<ARHistoryByPeriod.curyID>, 
				And<RevaluedARHistory.finPeriodID, Equal<ARHistoryByPeriod.lastActivityPeriod>>>>>>>,
				LeftJoin<Account, On<Account.accountID, Equal<ARHistoryByPeriod.accountID>, And<Account.curyID, IsNotNull>>,
				InnerJoin<Customer, On<Customer.bAccountID, Equal<ARHistoryByPeriod.customerID>>>>>,
				Where<ARHistoryByPeriod.curyID, Equal<Current<RevalueFilter.curyID>>,
					And<ARHistoryByPeriod.finPeriodID, Equal<Current<RevalueFilter.finPeriodID>>,
					And<Account.accountID, IsNull,
					And<RevaluedARHistory.curyFinYtdBalance, NotEqual<decimal0>>
					>>>>.Select(this))
			{
				ARHistoryByPeriod histbyper = res;
				RevaluedARHistory hist = PXCache<RevaluedARHistory>.CreateCopy(res);
				RevaluedARHistory existing;
                Customer cust = res;

				if ((existing = ARAccountList.Locate(hist)) != null)
				{
					yield return existing;
					continue;
				}
				else
				{
					ARAccountList.Cache.SetStatus(hist, PXEntryStatus.Held);
				}

				hist.CustomerClassID = cust.CustomerClassID;
				hist.CuryRateTypeID = cmsetup.Current.ARRateTypeReval ?? cust.CuryRateTypeID;

				if (string.IsNullOrEmpty(hist.CuryRateTypeID))
				{
					ARAccountList.Cache.RaiseExceptionHandling<RevaluedGLHistory.curyRateTypeID>(hist, null, new PXSetPropertyException(Messages.RateTypeNotFound));
				}
				else
				{
					CurrencyRateByDate curyrate = PXSelect<CurrencyRateByDate,
						Where<CurrencyRateByDate.fromCuryID, Equal<Current<RevalueFilter.curyID>>,
					And<CurrencyRateByDate.toCuryID, Equal<Current<Company.baseCuryID>>,
					And<CurrencyRateByDate.curyRateType, Equal<Required<Customer.curyRateTypeID>>,
					And2<Where<CurrencyRateByDate.curyEffDate, LessEqual<Current<RevalueFilter.curyEffDate>>, Or<CurrencyRateByDate.curyEffDate, IsNull>>, And<Where<CurrencyRateByDate.nextEffDate, Greater<Current<RevalueFilter.curyEffDate>>, Or<CurrencyRateByDate.nextEffDate, IsNull>>>>>>>>
					.Select(this, hist.CuryRateTypeID);

					if (curyrate == null || curyrate.CuryMultDiv == null)
					{
						hist.CuryMultDiv = "M";
						hist.CuryRate = 1m;
						ARAccountList.Cache.RaiseExceptionHandling<RevaluedARHistory.curyRate>(hist, 1m, new PXSetPropertyException(Messages.RateNotFound, PXErrorLevel.RowWarning));
					}
					else
					{
						hist.CuryRate = curyrate.CuryRate;
						hist.CuryMultDiv = curyrate.CuryMultDiv;
					}

					CurrencyInfo info = new CurrencyInfo();
					info.BaseCuryID = company.Current.BaseCuryID;
					info.CuryID = hist.CuryID;
					info.CuryMultDiv = hist.CuryMultDiv;
					info.CuryRate = hist.CuryRate;

					//hist.CuryFinYtdBalance -= hist.CuryFinYtdDeposits;
					//hist.FinYtdBalance -= hist.FinYtdDeposits;

					decimal baseval;
					PXCurrencyAttribute.CuryConvBase(currencyinfo.Cache, info, (decimal)hist.CuryFinYtdBalance, out baseval);
					hist.FinYtdRevalued = baseval;
					hist.FinPrevRevalued = string.Equals(histbyper.FinPeriodID, histbyper.LastActivityPeriod) ? hist.FinPtdRevalued : 0m;
					hist.FinPtdRevalued = hist.FinYtdRevalued - hist.FinPrevRevalued - hist.FinYtdBalance;
				}
				yield return hist;
			}
		}

		public static void Revalue(RevalueFilter filter, List<RevaluedARHistory> list)
		{
			JournalEntry je = PXGraph.CreateInstance<JournalEntry>();
			PostGraph pg = PXGraph.CreateInstance<PostGraph>();
			PXCache cache = je.Caches[typeof(CuryARHist)];
			PXCache basecache = je.Caches[typeof(ARHist)];
			je.Views.Caches.Add(typeof(CuryARHist));
			je.Views.Caches.Add(typeof(ARHist));
			
			string extRefNbrNumbering = je.CMSetup.Current.ExtRefNbrNumberingID;			
			if (string.IsNullOrEmpty(extRefNbrNumbering) == false)
			{
				RevaluationRefNbrHelper helper = new RevaluationRefNbrHelper(extRefNbrNumbering);
				je.RowPersisting.AddHandler<GLTran>(helper.OnRowPersisting);				
			}

			DocumentList<Batch> created = new DocumentList<Batch>(je);

			Currency currency = PXSelect<Currency, Where<Currency.curyID, Equal<Required<Currency.curyID>>>>.Select(je, filter.CuryID);

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				foreach (RevaluedARHistory hist in list)
				{
					if (hist.FinPtdRevalued == 0m)
					{
						continue;
					}

                    if (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted)
					{
						je.Save.Press();

						if (created.Find(je.BatchModule.Current) == null)
						{
							created.Add(je.BatchModule.Current);
						}
					}

					Batch cmbatch = created.Find<Batch.branchID>(hist.BranchID) ?? new Batch();
					if (cmbatch.BatchNbr == null)
					{
						je.Clear();

						CurrencyInfo info = new CurrencyInfo();
						info.CuryID = hist.CuryID;
						info.CuryEffDate = filter.CuryEffDate;
						info = je.currencyinfo.Insert(info) ?? info;

						cmbatch = new Batch();
						cmbatch.BranchID = hist.BranchID;
						cmbatch.Module = "CM";
						cmbatch.Status = "U";
						cmbatch.AutoReverse = true;
						cmbatch.Released = true;
						cmbatch.Hold = false;
						cmbatch.DateEntered = filter.CuryEffDate;
						cmbatch.FinPeriodID = filter.FinPeriodID;
						cmbatch.TranPeriodID = filter.FinPeriodID;
						cmbatch.CuryID = hist.CuryID;
						cmbatch.CuryInfoID = info.CuryInfoID;
						cmbatch.DebitTotal = 0m;
						cmbatch.CreditTotal = 0m;
						cmbatch.Description = filter.Description;
						je.BatchModule.Insert(cmbatch);

						CurrencyInfo b_info = je.currencyinfo.Select();
						if (b_info != null)
						{
							b_info.CuryID = hist.CuryID;
							b_info.CuryEffDate = filter.CuryEffDate;
							b_info.CuryRate = 1m;
							b_info.CuryMultDiv = "M";
							je.currencyinfo.Update(b_info);
						}
					}
					else
					{
						je.BatchModule.Current = je.BatchModule.Search<Batch.batchNbr>(cmbatch.BatchNbr, cmbatch.Module);
					}

					{
						GLTran tran = new GLTran();
						tran.SummPost = false;
						tran.AccountID = currency.ARProvAcctID ?? hist.AccountID;
						tran.SubID = currency.ARProvSubID ?? hist.SubID;
						tran.CuryDebitAmt = 0m;
						tran.CuryCreditAmt = 0m;

						tran.DebitAmt = (hist.FinPtdRevalued < 0m) ? 0m : hist.FinPtdRevalued;
						tran.CreditAmt = (hist.FinPtdRevalued < 0m) ? -1m * hist.FinPtdRevalued : 0m;

						tran.TranType = "REV";
						tran.TranClass = "A";
						tran.RefNbr = string.Empty;
						tran.TranDesc = string.Empty;
						tran.TranPeriodID = filter.FinPeriodID;
						tran.FinPeriodID = filter.FinPeriodID;
						tran.TranDate = filter.CuryEffDate;
						tran.CuryInfoID = null;
						tran.Released = true;
						tran.ReferenceID = hist.CustomerID;

						je.GLTranModuleBatNbr.Insert(tran);
					}

					foreach (GLTran tran in je.GLTranModuleBatNbr.SearchAll<Asc<GLTran.tranClass>>(new object[] { "G" }))
					{
						je.GLTranModuleBatNbr.Delete(tran);
					}

                    CustomerClass custclass = PXSelectReadonly<CustomerClass, Where<CustomerClass.customerClassID, Equal<Required<CustomerClass.customerClassID>>>>.Select(je, hist.CustomerClassID);

                    if (custclass == null)
                    {
                        custclass = new CustomerClass();
                    }

                    if (custclass.UnrealizedGainAcctID == null)
                    {
                        custclass.UnrealizedGainSubID = null;
                    }

                    if (custclass.UnrealizedLossAcctID == null)
                    {
                        custclass.UnrealizedLossSubID = null;
                    }

					{
						GLTran tran = new GLTran();
						tran.SummPost = true;
						tran.CuryDebitAmt = 0m;
						tran.CuryCreditAmt = 0m;

						if (je.BatchModule.Current.DebitTotal > je.BatchModule.Current.CreditTotal)
						{
							tran.AccountID = custclass.UnrealizedGainAcctID ?? currency.UnrealizedGainAcctID;
                            tran.SubID = custclass.UnrealizedGainSubID ?? GainLossSubAccountMaskAttribute.GetSubID<Currency.unrealizedGainSubID>(je, hist.BranchID, currency);
							tran.DebitAmt = 0m;
							tran.CreditAmt = (je.BatchModule.Current.DebitTotal - je.BatchModule.Current.CreditTotal);
						}
						else
						{
                            tran.AccountID = custclass.UnrealizedLossAcctID ?? currency.UnrealizedLossAcctID;
                            tran.SubID = custclass.UnrealizedLossSubID ?? GainLossSubAccountMaskAttribute.GetSubID<Currency.unrealizedLossSubID>(je, hist.BranchID, currency);
							tran.DebitAmt = (je.BatchModule.Current.CreditTotal - je.BatchModule.Current.DebitTotal);
							tran.CreditAmt = 0m;
						}

						tran.TranType = "REV";
						tran.TranClass = "G";
						tran.RefNbr = string.Empty;
						tran.TranDesc = string.Empty;
						tran.Released = true;
						tran.ReferenceID = null;

						je.GLTranModuleBatNbr.Insert(tran);
					}

					{
						CuryARHist arhist = new CuryARHist();
						arhist.BranchID = hist.BranchID;
						arhist.AccountID = hist.AccountID;
						arhist.SubID = hist.SubID;
						arhist.FinPeriodID = filter.FinPeriodID;
						arhist.CustomerID = hist.CustomerID;
						arhist.CuryID = hist.CuryID;

						arhist = (CuryARHist)cache.Insert(arhist);
						arhist.FinPtdRevalued += hist.FinPtdRevalued;
					}

					{
						ARHist arhist = new ARHist();
						arhist.BranchID = hist.BranchID;
						arhist.AccountID = hist.AccountID;
						arhist.SubID = hist.SubID;
						arhist.FinPeriodID = filter.FinPeriodID;
						arhist.CustomerID = hist.CustomerID;

						arhist = (ARHist)basecache.Insert(arhist);
						arhist.FinPtdRevalued += hist.FinPtdRevalued;
					}
				}

				if (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted)
				{
					je.Save.Press();

					if (created.Find(je.BatchModule.Current) == null)
					{
						created.Add(je.BatchModule.Current);
					}
				}

				ts.Complete();
			}

			CMSetup cmsetup = PXSelect<CMSetup>.Select(je);

			for (int i = 0; i < created.Count; i++)
			{
				if (cmsetup.AutoPostOption == true)
				{
					pg.Clear();
					pg.PostBatchProc(created[i]);
				}
			}

            if (created.Count > 0)
            {
                je.BatchModule.Current = created[created.Count - 1];
                throw new PXRedirectRequiredException(je, "Preview");
            }
		}
	}

	public class RevaluationRefNbrHelper
	{
		private Dictionary<string, string> _batchKeys;
		private string extRefNbrNumbering;		

		public RevaluationRefNbrHelper(string aExtRefNbrNumbering)  
		{
			this._batchKeys = new Dictionary<string, string>();
			this.extRefNbrNumbering = aExtRefNbrNumbering;
		}
		public void OnRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			GLTran tran = (GLTran)e.Row;
			if (tran != null && ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete))
			{
				PXCache batchCache = sender.Graph.Caches[typeof(Batch)];
				Batch batch = (Batch)batchCache.Current;
				if (batch != null && string.IsNullOrEmpty(batch.BatchNbr) == false)
				{
					string batchNbr = batch.BatchNbr;
					if (string.IsNullOrEmpty(tran.RefNbr))
					{
						string extRefNbr;
						if (!_batchKeys.TryGetValue(batchNbr, out extRefNbr))
						{
							extRefNbr = AutoNumberAttribute.GetNextNumber(sender, tran, extRefNbrNumbering, tran.TranDate);
							_batchKeys.Add(batchNbr, extRefNbr);
						}
                        tran.RefNbr = extRefNbr;

                        PXDBLiteDefaultAttribute.SetDefaultForInsert<GLTran.refNbr>(sender, tran, false);
					}
				}
			}
		}
	}


    [Serializable]
	[PXBreakInheritance()]
	public partial class RevaluedARHistory : CuryARHistory
	{
		#region Selected
		public abstract class selected : PX.Data.IBqlField
		{
		}
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region CuryRateTypeID
		public abstract class curyRateTypeID : PX.Data.IBqlField
		{
		}
		protected String _CuryRateTypeID;
		[PXString(6, IsUnicode = true)]
		[PXUIField(DisplayName = "Currency Rate Type")]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		public virtual String CuryRateTypeID
		{
			get
			{
				return this._CuryRateTypeID;
			}
			set
			{
				this._CuryRateTypeID = value;
			}
		}
		#endregion
		#region CuryRate
		public abstract class curyRate : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryRate = 1m;
		[PXDecimal(6)]
		[PXUIField(DisplayName = "Currency Rate")]
		public virtual Decimal? CuryRate
		{
			get
			{
				return this._CuryRate;
			}
			set
			{
				this._CuryRate = value;
			}
		}
		#endregion
		#region CuryMultDiv
		public abstract class curyMultDiv : PX.Data.IBqlField
		{
		}
		protected String _CuryMultDiv = "M";
		[PXString(1, IsFixed = true)]
		public virtual String CuryMultDiv
		{
			get
			{
				return this._CuryMultDiv;
			}
			set
			{
				this._CuryMultDiv = value;
			}
		}
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.IBqlField
		{
		}
		[Account(IsKey = true, DescriptionField = typeof(Account.description))]
		public override Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.IBqlField
		{
		}
		[SubAccount(IsKey = true, DescriptionField = typeof(Sub.description))]
		public override Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.IBqlField
		{
		}
		[Branch(IsKey = true, IsDetail = true)]
		public override Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region CustomerID
		public new abstract class customerID : PX.Data.IBqlField
		{
		}
		[Customer(IsKey = true, DescriptionField = typeof(Customer.acctName))]
		public override Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
        #region CustomerClassID
        public abstract class customerClassID : PX.Data.IBqlField
        {
        }
        protected String _CustomerClassID;
        [PXString(10, IsUnicode = true)]
        [PXSelector(typeof(CustomerClass.customerClassID), DescriptionField = typeof(CustomerClass.descr), CacheGlobal = true)]
        [PXUIField(DisplayName = "Customer Class", Enabled = false)]
        public virtual String CustomerClassID
        {
            get
            {
                return this._CustomerClassID;
            }
            set
            {
                this._CustomerClassID = value;
            }
        }
        #endregion
		#region CuryID
		public new abstract class curyID : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryFinYtdBalance
		public new abstract class curyFinYtdBalance : PX.Data.IBqlField
		{
		}
		[PXDBCury(typeof(RevaluedARHistory.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Foreign Currency Balance", Enabled = false)]
		public override Decimal? CuryFinYtdBalance
		{
			get
			{
				return this._CuryFinYtdBalance;
			}
			set
			{
				this._CuryFinYtdBalance = value;
			}
		}
		#endregion
		#region FinYtdBalance
		public new abstract class finYtdBalance : PX.Data.IBqlField
		{
		}
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Balance", Enabled = false)]
		public override Decimal? FinYtdBalance
		{
			get
			{
				return this._FinYtdBalance;
			}
			set
			{
				this._FinYtdBalance = value;
			}
		}
		#endregion
		#region FinPrevRevalued
		public abstract class finPrevRevalued : PX.Data.IBqlField
		{
		}
		protected Decimal? _FinPrevRevalued;
		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "PTD gain or loss", Enabled = false)]
		public virtual Decimal? FinPrevRevalued
		{
			get
			{
				return this._FinPrevRevalued;
			}
			set
			{
				this._FinPrevRevalued = value;
			}
		}
		#endregion
		#region FinYtdRevalued
		public abstract class finYtdRevalued : PX.Data.IBqlField
		{
		}
		protected Decimal? _FinYtdRevalued;
		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revalued Balance", Enabled = false)]
		[PXFormula(typeof(Add<Add<RevaluedARHistory.finYtdBalance, RevaluedARHistory.finPrevRevalued>, RevaluedARHistory.finPtdRevalued>))]
		public virtual Decimal? FinYtdRevalued
		{
			get
			{
				return this._FinYtdRevalued;
			}
			set
			{
				this._FinYtdRevalued = value;
			}
		}
		#endregion
		#region FinPtdRevalued
		public new abstract class finPtdRevalued : PX.Data.IBqlField
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Difference", Enabled = true)]
		public override Decimal? FinPtdRevalued
		{
			get
			{
				return this._FinPtdRevalued;
			}
			set
			{
				this._FinPtdRevalued = value;
			}
		}
		#endregion
	}
}
