using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.BQLConstants;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.TX;

namespace PX.Objects.CA
{
	[System.SerializableAttribute()]
    [PX.Objects.GL.TableAndChartDashboardType]
	public class CATranEnq : PXGraph<CATranEnq>
	{
		#region Internal Type definition
        //Alias for CATran
        [Serializable]
		public partial class CATranExt : CATran
		{
			#region DepositType
			public abstract class depositType : PX.Data.IBqlField
			{
			}
			protected String _DepositType;
			[PXString(3, IsFixed = true)]
			public virtual String DepositType
			{
				get
				{
					return this._DepositType;
				}
				set
				{
					this._DepositType = value;
				}
			}
			#endregion
			#region DepositNbr
			public abstract class depositNbr : PX.Data.IBqlField
			{
			}
			protected String _DepositNbr;
			[PXString(15, IsUnicode = true)]
			[PXUIField(DisplayName = "CA Deposit Nbr.", Enabled = false)]
			public virtual String DepositNbr
			{
				get
				{
					return this._DepositNbr;
				}
				set
				{
					this._DepositNbr = value;
				}
			}
			#endregion                
		} 
		#endregion
		
        #region Buttons
			#region Cancel
			public PXSave<CAEnqFilter> Save;
	        
			[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
			[PXCancelButton]
			protected virtual IEnumerable Cancel(PXAdapter adapter)
			{
				CATranListRecords.Cache.Clear();
				Caches[typeof(CADailySummary)].Clear();
				TimeStamp = null;
				//CATranListRecords.SetProcessEnabled(true);
				PXLongOperation.ClearStatus(this.UID);
				return adapter.Get();
			}
			#endregion 
		
			public PXAction<CAEnqFilter> cancel;

			#region Button Release
            public PXAction<CAEnqFilter> Release;
				[PXUIField(DisplayName = "Release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
				[PXProcessButton]
				public virtual IEnumerable release(PXAdapter adapter)
				{
					CASetup			setup = casetup.Current;
					CAEnqFilter    filter = Filter.Current;
					
                    List<CATran> tranList = new List<CATran>();

					foreach (CATran transToRelease in PXSelect<CATran, Where2<Where<Required<CAEnqFilter.includeUnreleased>, Equal<boolTrue>,
			                                                       Or<CATran.released, Equal<boolTrue>>>,
		                                                    And<CATran.cashAccountID, Equal<Required<CAEnqFilter.accountID>>,
		                                                    And<CATran.tranDate, Between<Required<CAEnqFilter.startDate>, Required<CAEnqFilter.endDate>>>>>,
                                         OrderBy<Asc<CATran.tranDate, Asc<CATran.extRefNbr, Asc<CATran.tranID>>>>>.Select(this, filter.IncludeUnreleased, filter.AccountID, filter.StartDate, filter.EndDate)) 
					{
						if (transToRelease.Selected == true)
						{
							tranList.Add(transToRelease);
						}
					}
					Save.Press();
                    if (tranList.Count == 0)
					{
						throw new PXException(Messages.DocumentStatusInvalid);
					}
					else
					{
                        PXLongOperation.StartOperation(this, delegate() { CATrxRelease.GroupReleaseTransaction(tranList, setup.ReleaseAP == true, setup.ReleaseAR == true, true); });
					}
					return adapter.Get();
				}
			#endregion
			
			#region Button Clear
            public PXAction<CAEnqFilter> Clearence;
				[PXUIField(DisplayName = "Clear", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
				[PXProcessButton]
				public virtual IEnumerable clearence(PXAdapter adapter)
				{
					CAEnqFilter    filter = Filter.Current;
					CATranExt newrow = null;
					foreach (CATranExt transToClear in PXSelect<CATranExt, Where2<Where<Required<CAEnqFilter.includeUnreleased>, Equal<boolTrue>,
																   Or<CATranExt.released, Equal<boolTrue>>>,
															And<CATranExt.cashAccountID, Equal<Required<CAEnqFilter.accountID>>,
															And<CATranExt.tranDate, Between<Required<CAEnqFilter.startDate>, Required<CAEnqFilter.endDate>>>>>,
										 OrderBy<Asc<CATranExt.tranDate, Asc<CATranExt.extRefNbr, Asc<CATranExt.tranID>>>>>.Select(this, filter.IncludeUnreleased, filter.AccountID, filter.StartDate, filter.EndDate)) 
					{
						if (transToClear.Reconciled != true)
						{
							newrow = PXCache<CATranExt>.CreateCopy(transToClear);
							newrow.Cleared = true;
							CATranListRecords.Cache.Update(newrow);
						}
					}
					Save.Press();
					return adapter.Get();
				}
			#endregion

			#region viewDoc
			public PXAction<CAEnqFilter> viewDoc;
			[PXUIField(DisplayName = "View Document", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
			[PXLookupButton]
			public virtual IEnumerable ViewDoc(PXAdapter adapter)
			{
				CATran.Redirect(Filter.Cache, CATranListRecords.Current);
				return Filter.Select();
			}

			public PXAction<CAEnqFilter> doubleClick;
			[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
			[PXLookupButton]
			public virtual IEnumerable DoubleClick(PXAdapter adapter)
			{

				CAEnqFilter filterCur = Filter.Current;

				if (filterCur.ShowSummary == true)
				{
					CATran tran = (CATran)(CATranListRecords.Current);
					filterCur.LastStartDate = filterCur.StartDate;
					filterCur.LastEndDate   = filterCur.EndDate;
					filterCur.StartDate     = tran.TranDate;
					filterCur.EndDate       = tran.TranDate;
					filterCur.ShowSummary   = false;

					CATranListRecords.Cache.Clear();
					Caches[typeof(CADailySummary)].Clear();
				}

				return adapter.Get();
			}
			#endregion
		
			#region addDet
			public PXAction<CAEnqFilter> AddDet;
			[PXUIField(DisplayName = "Create Transaction", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = false)]
			[PXProcessButton]
			public virtual IEnumerable addDet(PXAdapter adapter)
			{
				using (new PXTimeStampScope(this.TimeStamp))
				{
					CATran catran = null;
					catran = (catran ?? AddTrxFilter.AddAPTransaction(this, AddFilter, currencyinfo));
					catran = (catran ?? AddTrxFilter.AddARTransaction(this, AddFilter, currencyinfo));
					catran = (catran ?? AddTrxFilter.AddCATransaction(this, AddFilter, currencyinfo));
					if (catran != null)
					{
						CATranExt copy = new CATranExt();
						PXCache<CATran>.RestoreCopy(copy, catran);
						CATranListRecords.Update(copy);
						Save.Press();
					}
				}
				AddTrxFilter.Clear(this, AddFilter);

				CATranListRecords.Cache.Clear();
				Caches[typeof(CADailySummary)].Clear();

				Filter.Current.BegBal = null;

				return adapter.Get();
			}
			#endregion
        #endregion

		#region Selects
        public PXFilter<CAEnqFilter> Filter;
		public PXFilter<AddTrxFilter> AddFilter;
		public PXSelectReadonly<CADailySummary, 
			Where<CADailySummary.cashAccountID, Equal<Current<CAEnqFilter.accountID>>, 
			And<CADailySummary.tranDate, Between<Current<CAEnqFilter.startDate>, Current<CAEnqFilter.endDate>>>>>
			CATranListSummarized;
        //public PXFilteredProcessing<CATran, CAEnqFilter,
		//	Where<boolTrue, Equal<boolTrue>>,
		//	OrderBy<Asc<CATran.tranDate, Asc<CATran.extRefNbr, Asc<CATran.tranID>>>>> CATranListRecords;
		[PXFilterable]
		public PXSelectJoinOrderBy<CATranExt, LeftJoin<BAccountR,On<BAccountR.bAccountID,Equal<CATranExt.referenceID>>>, OrderBy<Asc<CATranExt.tranDate>>> CATranListRecords;
		public PXSelect<CurrencyInfo> currencyinfo;
		public ToggleCurrency<CAEnqFilter> CurrencyView;
		public PXSelect<CAAdj, Where<CAAdj.adjTranType, Equal<CAAPARTranType.cAAdjustment>, And<CAAdj.adjRefNbr,Equal<Required<CAAdj.adjRefNbr>>>>> caadj_adjRefNbr;
		public PXSelect<CASplit, Where<CASplit.adjTranType, Equal<CAAPARTranType.cAAdjustment>, And<CASplit.adjRefNbr, Equal<Required<CASplit.adjRefNbr>>>>> casplit_adjRefNbr;
        public PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Current<CAEnqFilter.accountID>>>> cashaccount;
	
		public PXSetup<CASetup> casetup;
		public CMSetupSelect cmsetup;
        #endregion

        #region Functions

        public override void Persist()
        {
            base.Persist();
            Caches[typeof(CM.CurrencyInfo)].SetStatus(Caches[typeof(CM.CurrencyInfo)].Current, PXEntryStatus.Inserted);
        }       

        public CATranEnq()
		{
			
 			CASetup setup = casetup.Current;
			Views["_AddTrxFilter.curyInfoID_CurrencyInfo.CuryInfoID_"] = new PXView(this, false, new Select<AddTrxFilter>(), new PXSelectDelegate(AddFilter.Get));
		}

				public override void Clear()
				{
					AddFilter.Current.TranDate = null;
					AddFilter.Current.FinPeriodID = null;
					AddFilter.Current.CuryInfoID = null;
					base.Clear();
				}

        public virtual void GetRange(DateTime date, string Range, out DateTime RangeStart, out DateTime RangeEnd)
		{
			switch (Range)
			{ 
				case "W":
					RangeStart = date.AddDays(-1 * (PXDateTime.DayOfWeekOrdinal(date.DayOfWeek) - 1));
					RangeEnd   = date.AddDays(7 - PXDateTime.DayOfWeekOrdinal(date.DayOfWeek));
					return;
				case "M":
					RangeStart = new DateTime(date.Year, date.Month, 1);
					RangeEnd   = new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
					return;
				case "P":
					RangeStart = FinPeriodIDAttribute.PeriodStartDate(this, FinPeriodIDAttribute.PeriodFromDate(this, date));
					RangeEnd   = FinPeriodIDAttribute.PeriodEndDate(this, FinPeriodIDAttribute.PeriodFromDate(this, date));
					return;
				case "D":
				default:
					RangeStart = date;
					RangeEnd   = date;
					return;
			}
		}

		#endregion
		
		#region Execute Select

		protected virtual IEnumerable filter()
		{
			PXCache cache = Caches[typeof(CAEnqFilter)];
			if (cache != null)
			{
				CAEnqFilter filter = cache.Current as CAEnqFilter;
				if (filter != null)
				{
					if(filter.StartDate == null || filter.EndDate == null)
					{
						DateTime startDate;
						DateTime endDate;

						GetRange((DateTime)this.Accessinfo.BusinessDate, casetup.Current.DateRangeDefault, out startDate, out endDate);
						filter.StartDate = startDate;	
						filter.EndDate	 = endDate;
					}

					if (filter.AccountID != null && filter.StartDate != null)
					{
						CADailySummary begBal = PXSelectGroupBy<CADailySummary,
													Where<CADailySummary.cashAccountID, Equal<Required<CAEnqFilter.accountID>>,
													And<CADailySummary.tranDate, Less<Required<CAEnqFilter.startDate>>>>,
													Aggregate<Sum<CADailySummary.amtReleasedClearedCr,
														Sum<CADailySummary.amtReleasedClearedDr,
														Sum<CADailySummary.amtReleasedUnclearedCr,
														Sum<CADailySummary.amtReleasedUnclearedDr,
														Sum<CADailySummary.amtUnreleasedClearedCr,
														Sum<CADailySummary.amtUnreleasedClearedDr,
														Sum<CADailySummary.amtUnreleasedUnclearedCr,
														Sum<CADailySummary.amtUnreleasedUnclearedDr,
														GroupBy<CADailySummary.cashAccountID>>>>>>>>>>>
													.Select(this, filter.AccountID, filter.StartDate);

						if ((begBal == null) || (begBal.CashAccountID == null))
						{
							filter.BegBal        = (decimal)0.0;
							filter.BegClearedBal = (decimal)0.0;
						}
						else
						{
							filter.BegBal = begBal.AmtReleasedClearedDr -
															begBal.AmtReleasedClearedCr +
															begBal.AmtReleasedUnclearedDr -
															begBal.AmtReleasedUnclearedCr;

							filter.BegClearedBal = begBal.AmtReleasedClearedDr -
													 begBal.AmtReleasedClearedCr;

							if (filter.IncludeUnreleased == true)
							{
								filter.BegBal += begBal.AmtUnreleasedClearedDr -
												 begBal.AmtUnreleasedClearedCr +
												 begBal.AmtUnreleasedUnclearedDr -
												 begBal.AmtUnreleasedUnclearedCr;

								filter.BegClearedBal += begBal.AmtUnreleasedClearedDr -
																		begBal.AmtUnreleasedClearedCr;
							}
						}
						CADailySummary turnover = PXSelectGroupBy<CADailySummary,
															Where<CADailySummary.cashAccountID, Equal<Required<CAEnqFilter.accountID>>,
															And<CADailySummary.tranDate,
																			Between<Required<CAEnqFilter.startDate>, Required<CAEnqFilter.startDate>>>>,
														Aggregate<Sum<CADailySummary.amtReleasedClearedCr,
																			Sum<CADailySummary.amtReleasedClearedDr,
																			Sum<CADailySummary.amtReleasedUnclearedCr,
																			Sum<CADailySummary.amtReleasedUnclearedDr,
																			Sum<CADailySummary.amtUnreleasedClearedCr,
																			Sum<CADailySummary.amtUnreleasedClearedDr,
																			Sum<CADailySummary.amtUnreleasedUnclearedCr,
																			Sum<CADailySummary.amtUnreleasedUnclearedDr,
																			GroupBy<CADailySummary.cashAccountID>>>>>>>>>>>.
																			Select(this, filter.AccountID, filter.StartDate, filter.EndDate);

						if ((turnover == null) || (turnover.CashAccountID == null))
						{
							filter.CreditTotal        = (decimal)0.0;
							filter.CreditClearedTotal = (decimal)0.0;
							filter.DebitTotal         = (decimal)0.0;
							filter.DebitClearedTotal  = (decimal)0.0;
						}
						else
						{
							filter.CreditTotal        = turnover.AmtReleasedClearedCr + turnover.AmtReleasedUnclearedCr;
							filter.CreditClearedTotal = turnover.AmtReleasedClearedCr;
							filter.DebitTotal         = turnover.AmtReleasedClearedDr + turnover.AmtReleasedUnclearedDr;
							filter.DebitClearedTotal  = turnover.AmtReleasedClearedDr;

							if (filter.IncludeUnreleased == true)
							{
								filter.CreditTotal        += turnover.AmtUnreleasedClearedCr + turnover.AmtUnreleasedUnclearedCr;
								filter.CreditClearedTotal += turnover.AmtUnreleasedClearedCr;
								filter.DebitTotal         += turnover.AmtUnreleasedClearedDr + turnover.AmtUnreleasedUnclearedDr;
								filter.DebitClearedTotal  += turnover.AmtUnreleasedClearedDr;
							}
						}

						foreach (CADailySummary delta in Caches[typeof(CADailySummary)].Inserted)
						{
							filter.CreditTotal += delta.AmtReleasedClearedCr;
							filter.CreditTotal += delta.AmtReleasedUnclearedCr;
							filter.CreditClearedTotal += delta.AmtReleasedClearedCr;
							filter.DebitTotal += delta.AmtReleasedClearedDr;
							filter.DebitTotal += delta.AmtReleasedUnclearedDr;
							filter.DebitClearedTotal += delta.AmtReleasedClearedDr;
							if (filter.IncludeUnreleased == true)
							{
								filter.CreditTotal += delta.AmtUnreleasedClearedCr;
								filter.CreditTotal += delta.AmtUnreleasedUnclearedCr;
								filter.CreditClearedTotal += delta.AmtUnreleasedClearedCr;
								filter.DebitTotal += delta.AmtUnreleasedClearedDr;
								filter.DebitTotal += delta.AmtUnreleasedUnclearedDr;
								filter.DebitClearedTotal += delta.AmtUnreleasedClearedDr;
							}
						}

						filter.EndBal        = filter.BegBal + filter.DebitTotal - filter.CreditTotal;
						filter.EndClearedBal = filter.BegClearedBal + filter.DebitClearedTotal - filter.CreditClearedTotal;
					}
				}
			}
			yield return cache.Current;
			cache.IsDirty = false;
		}

		public virtual IEnumerable cATranListRecords()
		{
			CAEnqFilter filter = Filter.Current;
			//List<CATranExt> result = new List<CATranExt>();
			List<PXResult<CATranExt, BAccountR>> result = new List<PXResult<CATranExt, BAccountR>>();
            if (filter != null && filter.ShowSummary == true)
			{
				long? id = 0;				
                foreach (CADailySummary daily in CATranListSummarized.Select())
				{
					CATranExt tran = new CATranExt();
					id++;
					tran.TranID = id;
					tran.CashAccountID  = daily.CashAccountID;
					tran.TranDate       = daily.TranDate;

                    tran.CuryDebitAmt   = daily.AmtReleasedClearedDr + daily.AmtReleasedUnclearedDr;  //??
                    tran.CuryCreditAmt  = daily.AmtReleasedClearedCr + daily.AmtReleasedUnclearedCr; //??

					if (Filter.Current.IncludeUnreleased == true)
					{
                        tran.CuryDebitAmt  += daily.AmtUnreleasedClearedDr + daily.AmtUnreleasedUnclearedDr;  //??
                        tran.CuryCreditAmt += daily.AmtUnreleasedClearedCr + daily.AmtUnreleasedUnclearedCr; //??
					}
                    tran.DayDesc = TM.EPCalendarFilter.CalendarTypeAttribute.GetDayName(((DateTime)tran.TranDate).DayOfWeek);
					result.Add(new PXResult<CATranExt, BAccountR>(tran,new BAccountR()));
				}
			}
			else
			{
				//List<CAMessage> listMessages = PXLongOperation.GetCustomInfo(this.UID) as List<CAMessage>;
                Dictionary<long, CAMessage> listMessages = PXLongOperation.GetCustomInfo(this.UID) as Dictionary<long,CAMessage>;
                foreach (PXResult<CATranExt,ARPayment, BAccountR> iRes in PXSelectJoin<CATranExt,LeftJoin<ARPayment, On<ARPayment.docType,Equal<CATranExt.origTranType>,
											And<ARPayment.refNbr,Equal<CATran.origRefNbr>>>,
												LeftJoin<BAccountR,On<BAccountR.bAccountID,Equal<CATranExt.referenceID>>>>,
											
										Where2<Where<Required<CAEnqFilter.includeUnreleased>, Equal<boolTrue>,
			                                                       Or<CATran.released, Equal<boolTrue>>>,
		                                                    And<CATranExt.cashAccountID, Equal<Required<CAEnqFilter.accountID>>,
		                                                    And<CATranExt.tranDate, Between<Required<CAEnqFilter.startDate>, Required<CAEnqFilter.endDate>>>>>,
                                         OrderBy<Asc<CATranExt.tranDate, Asc<CATranExt.extRefNbr, Asc<CATranExt.tranID>>>>>.Select(this, filter.IncludeUnreleased, filter.AccountID, filter.StartDate, filter.EndDate)) 
				{

					CATranExt tran = iRes;
					ARPayment payment = iRes;
					BAccountR baccount = iRes;
                    tran.DayDesc = TM.EPCalendarFilter.CalendarTypeAttribute.GetDayName(((DateTime)tran.TranDate).DayOfWeek);
					tran.DepositNbr = payment.DepositNbr;
					tran.DepositType = payment.DepositType;
                    if (listMessages != null)
                    {
                        CAMessage message;
                        if (listMessages.TryGetValue(tran.TranID.Value, out message))
                        {
                            if (message != null && message.Key == tran.TranID)
                            {
                                CATranListRecords.Cache.RaiseExceptionHandling<CATran.origRefNbr>(tran, tran.OrigRefNbr, new PXSetPropertyException(message.Message, message.ErrorLevel));
                            }
                        }
                        //for(int i=0; i<listMessages.Count; i++)
                        //{
                        //    CAMessage message = (CAMessage)listMessages[i];
                        //    if (message.Key == tran.TranID)
                        //    {
                        //        CATranListRecords.Cache.RaiseExceptionHandling<CATran.origRefNbr>(tran, tran.OrigRefNbr, new PXSetPropertyException(message.Message, message.ErrorLevel));	
                        //    }
                        //}
                    }
					result.Add(new PXResult<CATranExt,BAccountR>(tran, baccount));
				}
			}

			decimal curBalance = 0;
			if (filter != null && filter.BegBal != null)
				curBalance = (decimal)filter.BegBal;
			foreach (PXResult<CATranExt,BAccountR> it in PXView.Sort(result))
			{
				CATran tran = it;
				tran.BegBal = curBalance;
				tran.EndBal = tran.BegBal + tran.CuryDebitAmt - tran.CuryCreditAmt;
				curBalance = (decimal)tran.EndBal;
			}

			return result;
		}
        
		#endregion

        #region CurrencyInfo Events
        protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (cmsetup.Current.MCActivated != true)
			{
				CashAccount cashAcc = cashaccount.Current;
                if (cashAcc != null && !string.IsNullOrEmpty(cashAcc.CuryRateTypeID))
				{
                    e.NewValue = cashAcc.CuryRateTypeID;
					e.Cancel   = true;
				}
			}
		}
        #endregion

        #region CAEnqFilter Events
		
        protected virtual void CAEnqFilter_ShowSummary_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            CAEnqFilter filter = e.Row as CAEnqFilter;

            if (filter.ShowSummary == true)
            {
                if (filter.LastEndDate != null)
                {
                    filter.StartDate	 = filter.LastStartDate;
                    filter.EndDate		 = filter.LastEndDate;
                    filter.LastStartDate = null;
                    filter.LastEndDate   = null;
                }
            }
        }
		
		protected virtual void CAEnqFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
            CAEnqFilter filter = (CAEnqFilter) e.Row;
            if (filter == null) return;

			bool ShowSummaryNotChecked        = filter.ShowSummary != true;

			bool CashAccountNeedReconcilation = (cashaccount.Current != null) && (bool)cashaccount.Current.Reconcile;

			PXCache tranCache = CATranListRecords.Cache;
			tranCache.AllowInsert = false;
			tranCache.AllowUpdate = (ShowSummaryNotChecked);
			tranCache.AllowDelete = (ShowSummaryNotChecked);
			
			PXUIFieldAttribute.SetVisible<CATran.selected>		(tranCache, null, ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATran.hold>          (tranCache, null, false);
			PXUIFieldAttribute.SetVisible<CATran.status>        (tranCache, null, ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATran.origModule>    (tranCache, null, ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATran.origRefNbr>    (tranCache, null, ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATran.origTranType>  (tranCache, null, ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATran.extRefNbr>     (tranCache, null, ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATran.tranDesc>      (tranCache, null, ShowSummaryNotChecked);
			
			PXUIFieldAttribute.SetVisible<CATran.referenceName>	(tranCache, null, ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATran.reconciled>    (tranCache, null, ShowSummaryNotChecked && CashAccountNeedReconcilation);
			PXUIFieldAttribute.SetVisible<CATran.clearDate>     (tranCache, null, ShowSummaryNotChecked && CashAccountNeedReconcilation);
			PXUIFieldAttribute.SetVisible<CATran.cleared>       (tranCache, null, ShowSummaryNotChecked && CashAccountNeedReconcilation);
			PXUIFieldAttribute.SetVisible<CATran.dayDesc>       (tranCache, null, !ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATranExt.dayDesc>(tranCache, null, !ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATranExt.depositNbr>(tranCache, null, ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATran.referenceID>(tranCache, null, ShowSummaryNotChecked);
			//PXUIFieldAttribute.SetVisible<CATran.referenceID_BAccountR_AcctName>(tranCache, null, ShowSummaryNotChecked);
			PXCache bAcctCache = this.Caches[typeof(BAccountR)];
			PXUIFieldAttribute.SetVisible<BAccountR.acctName>(bAcctCache, null, ShowSummaryNotChecked);
			
			Clearence.SetEnabled(CashAccountNeedReconcilation);
			AddFilter.Cache.RaiseRowSelected(AddFilter.Current);

			bool operationExists = PXLongOperation.Exists(UID);
			Save.SetEnabled(!operationExists);
			Release.SetEnabled(!operationExists);
			Clearence.SetEnabled(!operationExists);
			
		}
 
        protected virtual void CAEnqFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            CAEnqFilter filter = e.Row as CAEnqFilter;
            AddTrxFilter addFilter = AddFilter.Current;
            CurrencyInfo   curInfo = currencyinfo.Current;

            Account acct = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, filter.AccountID);

            if (acct != null)
            {
                sender.SetValueExt<CAEnqFilter.curyID>(filter, acct.CuryID);
            }

            CATranListRecords.Cache.Clear();
            Caches[typeof(CADailySummary)].Clear();
        }

        
		protected virtual void CAEnqFilter_AccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{

            CAEnqFilter filter = e.Row as CAEnqFilter;
            cashaccount.Current    = (CashAccount)PXSelectorAttribute.Select<CAEnqFilter.accountID>(sender, filter);
			sender.SetDefaultExt<CAEnqFilter.curyID>(filter);
			AddFilter.Cache.SetValueExt<AddTrxFilter.cashAccountID>(AddFilter.Current, filter.AccountID);
			
			if (filter != null && filter.ShowSummary != true)
			{
				bool needReselect = false;
				foreach (CATranExt tran in PXSelect<CATranExt, Where2<Where<Required<CAEnqFilter.includeUnreleased>, Equal<boolTrue>,
			                                                       Or<CATran.released, Equal<boolTrue>>>,
		                                                    And<CATranExt.cashAccountID, Equal<Required<CAEnqFilter.accountID>>,
		                                                    And<CATranExt.tranDate, Between<Required<CAEnqFilter.startDate>, Required<CAEnqFilter.endDate>>>>>,
                                         OrderBy<Asc<CATranExt.tranDate, Asc<CATranExt.extRefNbr, Asc<CATranExt.tranID>>>>>.Select(this, filter.IncludeUnreleased, filter.AccountID, filter.StartDate, filter.EndDate))
				{
					if (tran.Selected == true)
					{
						tran.Selected = false;
						CATranListRecords.Update(tran);
						needReselect = true;
					}
				}
				if (needReselect == true)
					Save.Press();
			}
		}

        #endregion

        #region CATran Events
		protected virtual void CATranExt_ClearDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if ((bool)((CATran)e.Row).Cleared && (e.NewValue == null))
			{
                throw new PXSetPropertyException(Messages.ClearedDateNotAvailable);
			}
		}

		protected virtual void CATranExt_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CATran catran = (CATran)e.Row;

			if (catran == null) return;

			bool cashAccountNeedReconcilation = (cashaccount.Current != null) && (bool)cashaccount.Current.Reconcile;
			bool isClearEnable				  = catran.Reconciled != true;

            PXUIFieldAttribute.SetEnabled(cache, catran, false);
            PXUIFieldAttribute.SetEnabled<CATran.selected>		(cache, catran, true);
			PXUIFieldAttribute.SetEnabled<CATran.reconciled>(cache, catran, false);
            PXUIFieldAttribute.SetEnabled<CATran.cleared>   (cache, catran, isClearEnable && cashAccountNeedReconcilation);
            PXUIFieldAttribute.SetEnabled<CATran.clearDate> (cache, catran, isClearEnable && cashAccountNeedReconcilation && (catran.Cleared == true));
			
		}

		protected virtual void CATranExt_Cleared_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
            CATran caTran = (CATran)e.Row;
            if (caTran.Cleared == true)
			{
                caTran.ClearDate = this.Accessinfo.BusinessDate;

                if (Filter.Current.IncludeUnreleased == true || caTran.Released == true)
				{
                    Filter.Current.DebitClearedTotal  += caTran.CuryDebitAmt;
                    Filter.Current.CreditClearedTotal += caTran.CuryCreditAmt;
                    Filter.Current.EndClearedBal      += (caTran.CuryDebitAmt - caTran.CuryCreditAmt);
				}
			}
			else 
			{
				caTran.ClearDate = null;

				if (Filter.Current.IncludeUnreleased == true || caTran.Released == true)
				{
					Filter.Current.DebitClearedTotal    -= caTran.CuryDebitAmt;
					Filter.Current.CreditClearedTotal   -= caTran.CuryCreditAmt;
					Filter.Current.EndClearedBal        -= (caTran.CuryDebitAmt - caTran.CuryCreditAmt);
				}
			}
		}

		protected virtual void CATranExt_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			CATran tran = (CATran)e.Row;

            if (tran.Released     == true || 
                tran.OrigModule   != BatchModule.CA || 
                tran.OrigTranType != CAAPARTranType.CAAdjustment)
			{
				throw new PXException(ErrorMessages.CantDeleteRecord);
			}

            CAAdj adj = caadj_adjRefNbr.Select(tran.OrigRefNbr);
			if (adj != null)
			{
				caadj_adjRefNbr.Delete(adj);
			}
            foreach (CASplit split in casplit_adjRefNbr.Select(tran.OrigRefNbr))
			{
				casplit_adjRefNbr.Delete(split);
			}
		}
        #endregion

	}
	
	[Serializable]
	public partial class CAEnqFilter : PX.Data.IBqlTable
	{
		#region AccountID
		public abstract class accountID : PX.Data.IBqlField
		{
		}
		protected Int32? _AccountID;
		[PXDefault()]
		[CashAccount(DisplayName = "Cash Account", DescriptionField = typeof(CashAccount.descr))]
		public virtual Int32? AccountID
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
		#region CuryID
		public abstract class curyID : PX.Data.IBqlField
		{
		}
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Enabled = false)]
		//[PXDefault(typeof(Search<CashAccount.curyID, Where<CashAccount.accountID, Equal<Current<CAEnqFilter.accountID>>>>))]
		[PXSelector(typeof(CM.Currency.curyID))]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _StartDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _EndDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]
		public virtual DateTime? EndDate
		{
			get
			{
				return this._EndDate;
			}
			set
			{
				this._EndDate = value;
			}
		}
		#endregion
		
		#region LastStartDate
		public abstract class lastStartDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _LastStartDate;
		[PXDBDate()]
		[PXDefault()]
		[PXUIField(DisplayName = "Last Start Date", Visible = false)]
		public virtual DateTime? LastStartDate
		{
			get
			{
				return this._LastStartDate;
			}
			set
			{
				this._LastStartDate = value;
			}
		}
		#endregion
		#region LastEndDate
		public abstract class lastEndDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _LastEndDate;
		[PXDBDate()]
		[PXDefault()]
		[PXUIField(DisplayName = "End Date", Visible = false)]
		public virtual DateTime? LastEndDate
		{
			get
			{
				return this._LastEndDate;
			}
			set
			{
				this._LastEndDate = value;
			}
		}
		#endregion
		
		#region ShowSummary
		public abstract class showSummary : PX.Data.IBqlField
		{
		}
		protected Boolean? _ShowSummary;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Show Summary")]
		public virtual Boolean? ShowSummary
		{
			get
			{
				return this._ShowSummary;
			}
			set
			{
				this._ShowSummary = value;
			}
		}
		#endregion
		#region IncludeUnreleased
		public abstract class includeUnreleased : PX.Data.IBqlField
		{
		}
		protected Boolean? _IncludeUnreleased;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Include Unreleased")]
		public virtual Boolean? IncludeUnreleased
		{
			get
			{
				return this._IncludeUnreleased;
			}
			set
			{
				this._IncludeUnreleased = value;
			}
		}
		#endregion
		#region BegBal
		public abstract class begBal : PX.Data.IBqlField
		{
		}
		protected Decimal? _BegBal;
		[PXDecimal(typeof(Search<CM.Currency.decimalPlaces,
												Where<Currency.curyID, Equal<Current<CAEnqFilter.curyID>>>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Beginning Balance", Enabled = false)]
		public virtual Decimal? BegBal
		{
			get
			{
				return this._BegBal;
			}
			set
			{
				this._BegBal = value;
			}
		}
		#endregion
		#region CreditTotal
		public abstract class creditTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _CreditTotal;
		[PXDecimal(typeof(Search<CM.Currency.decimalPlaces,
											Where<Currency.curyID, Equal<Current<CAEnqFilter.curyID>>>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Acct. Credit Total", Enabled = false)]
		public virtual Decimal? CreditTotal
		{
			get
			{
				return this._CreditTotal;
			}
			set
			{
				this._CreditTotal = value;
			}
		}
		#endregion
		#region DebitTotal
		public abstract class debitTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _DebitTotal;
		[PXDecimal(typeof(Search<CM.Currency.decimalPlaces,
										Where<Currency.curyID, Equal<Current<CAEnqFilter.curyID>>>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Acct. Debit Total", Enabled = false)]
		public virtual Decimal? DebitTotal
		{
			get
			{
				return this._DebitTotal;
			}
			set
			{
				this._DebitTotal = value;
			}
		}
		#endregion
		#region EndBal
		public abstract class endBal : PX.Data.IBqlField
		{
		}
		protected Decimal? _EndBal;
		[PXDecimal(typeof(Search<CM.Currency.decimalPlaces,
									Where<Currency.curyID, Equal<Current<CAEnqFilter.curyID>>>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ending Balance", Enabled = false)]
		public virtual Decimal? EndBal
		{
			get
			{
				return this._EndBal;
			}
			set
			{
				this._EndBal = value;
			}
		}
		#endregion
		#region BegClearedBal
		public abstract class begClearedBal : PX.Data.IBqlField
		{
		}
		protected Decimal? _BegClearedBal;
		[PXDecimal(typeof(Search<CM.Currency.decimalPlaces,
									Where<Currency.curyID, Equal<Current<CAEnqFilter.curyID>>>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Beginning Balance", Enabled = false)]
		public virtual Decimal? BegClearedBal
		{
			get
			{
				return this._BegClearedBal;
			}
			set
			{
				this._BegClearedBal = value;
			}
		}
		#endregion
		#region CreditClearedTotal
		public abstract class creditClearedTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _CreditClearedTotal;
		[PXDecimal(typeof(Search<CM.Currency.decimalPlaces,
									Where<Currency.curyID, Equal<Current<CAEnqFilter.curyID>>>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Acct. Credit Total", Enabled = false)]
		public virtual Decimal? CreditClearedTotal
		{
			get
			{
				return this._CreditClearedTotal;
			}
			set
			{
				this._CreditClearedTotal = value;
			}
		}
		#endregion
		#region DebitClearedTotal
		public abstract class debitClearedTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _DebitClearedTotal;
		[PXDecimal(typeof(Search<CM.Currency.decimalPlaces,
									Where<Currency.curyID, Equal<Current<CAEnqFilter.curyID>>>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Acct. Debit Total", Enabled = false)]
		public virtual Decimal? DebitClearedTotal
		{
			get
			{
				return this._DebitClearedTotal;
			}
			set
			{
				this._DebitClearedTotal = value;
			}
		}
		#endregion
		#region EndClearedBal
		public abstract class endClearedBal : PX.Data.IBqlField
		{
		}
		protected Decimal? _EndClearedBal;
		[PXDecimal(typeof(Search<CM.Currency.decimalPlaces,
									Where<Currency.curyID, Equal<Current<CAEnqFilter.curyID>>>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ending Balance", Enabled = false)]
		public virtual Decimal? EndClearedBal
		{
			get
			{
				return this._EndClearedBal;
			}
			set
			{
				this._EndClearedBal = value;
			}
		}
		#endregion
	}
	
    [Serializable]
	public partial class AddTrxFilter : PX.Data.IBqlTable
	{
		#region OnlyExpense
		public abstract class onlyExpense : PX.Data.IBqlField
		{
		}
		protected Boolean? _OnlyExpense;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? OnlyExpense
		{
			get
			{
				return this._OnlyExpense;
			}
			set
			{
				this._OnlyExpense = value;
			}
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _TranDate;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Doc. Date", Required = true)]
		[AddFilter()]
		public virtual DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		protected String _FinPeriodID;
		[OpenPeriod(typeof(AddTrxFilter.tranDate))]
		[PXUIField(DisplayName = "Fin. Period", Required = true)]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _CashAccountID;
		[PXDefault()]
		[GL.CashAccount(DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(CashAccount.descr))]
		public virtual Int32? CashAccountID
		{
			get
			{
				return this._CashAccountID;
			}
			set
			{
				this._CashAccountID = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.IBqlField
		{
		}
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXSelector(typeof(Currency.curyID))]
		[PXDefault(typeof(Search<CashAccount.curyID,Where<CashAccount.cashAccountID,Equal<Current<AddTrxFilter.cashAccountID>>>>))]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.IBqlField
		{
		}
		protected Int32? _BranchID;
		[PXDBInt()]
		[PXDefault(typeof(Search<CashAccount.branchID, Where<CashAccount.cashAccountID, Equal<Current<AddTrxFilter.cashAccountID>>>>))]
		public virtual Int32? BranchID
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
		#region UpdateNextNumber
		public abstract class updateNextNumber : PX.Data.IBqlField
		{
		}
		protected Boolean? _UpdateNextNumber;

		[PXBool()]
		[PXUIField(DisplayName = "Update Next Number", Visibility = PXUIVisibility.Invisible)]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? UpdateNextNumber
		{
			get
			{
				return this._UpdateNextNumber;
			}
			set
			{
				this._UpdateNextNumber = value;
			}
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.IBqlField
		{
		}
		protected String _ExtRefNbr;
		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "Document Ref.", Required = true)]
		[AP.PaymentRef(typeof(AddTrxFilter.cashAccountID), typeof(AddTrxFilter.paymentMethodID), null, typeof(AddTrxFilter.updateNextNumber))]
		public virtual String ExtRefNbr
		{
			get
			{
				return this._ExtRefNbr;
			}
			set
			{
				this._ExtRefNbr = value;
			}
		}
		#endregion
		#region EntryTypeID
		public abstract class entryTypeID : PX.Data.IBqlField
		{
		}
		protected String _EntryTypeID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault()]
		[PXSelector(typeof(Search2<CAEntryType.entryTypeId,
							  InnerJoin<CashAccountETDetail, On<CashAccountETDetail.entryTypeID, Equal<CAEntryType.entryTypeId>>>,
							  Where<CashAccountETDetail.accountID, Equal<Current<AddTrxFilter.cashAccountID>>,
								And<Where<Current<AddTrxFilter.onlyExpense>, Equal<boolFalse>, Or<CAEntryType.module, Equal<BatchModule.moduleCA>>>>>>),
					  DescriptionField = typeof(CAEntryType.descr))]
		[PXUIField(DisplayName = "Entry Type", Required = true)]
		public virtual String EntryTypeID
		{
			get
			{
				return this._EntryTypeID;
			}
			set
			{
				this._EntryTypeID = value;
			}
		}
		#endregion
		#region OrigModule
		public abstract class origModule : PX.Data.IBqlField
		{
		}
		protected String _OrigModule;
		[PXDBString(2, IsFixed = true)]
		[PXDefault(typeof(Search<CAEntryType.module, Where<CAEntryType.entryTypeId, Equal<Current<AddTrxFilter.entryTypeID>>>>))]
		[PXUIField(DisplayName = "Module", Enabled = false)]
		public virtual String OrigModule
		{
			get
			{
				return this._OrigModule;
			}
			set
			{
				this._OrigModule = value;
			}
		}
		#endregion
		#region DrCr
		public abstract class drCr : PX.Data.IBqlField
		{
		}
		protected String _DrCr;
		[PXDefault("D",typeof(Search<CAEntryType.drCr, Where<CAEntryType.entryTypeId, Equal<Current<AddTrxFilter.entryTypeID>>>>))]
		[PXDBString(1, IsFixed = true)]
		[CADrCr.List()]
		[PXUIField(DisplayName = "Disb. / Receipt", Enabled = false)]
		public virtual String DrCr
		{
			get
			{
				return this._DrCr;
			}
			set
			{
				this._DrCr = value;
			}
		}
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.IBqlField
		{
		}
		protected String _TaxZoneID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone")]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
		[PXDefault(typeof(Search<CashAccountETDetail.taxZoneID, Where<CashAccountETDetail.accountID, Equal<Current<AddTrxFilter.cashAccountID>>, And<CashAccountETDetail.entryTypeID, Equal<Current<AddTrxFilter.entryTypeID>>>>>))]
		public virtual String TaxZoneID
		{
			get
			{
				return this._TaxZoneID;
			}
			set
			{
				this._TaxZoneID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected String _Descr;
		[PXDefault(typeof(Search<CAEntryType.descr, Where<CAEntryType.entryTypeId, Equal<Current<AddTrxFilter.entryTypeID>>>>))]
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.IBqlField
		{
		}
		protected Int32? _AccountID;
		[PXDefault(typeof(Coalesce<Search<CashAccountETDetail.offsetAccountID, Where<CashAccountETDetail.entryTypeID, Equal<Current<AddTrxFilter.entryTypeID>>,
							And<CashAccountETDetail.accountID,Equal<Current<AddTrxFilter.cashAccountID>>>>>,
						Search<CAEntryType.accountID, Where<CAEntryType.entryTypeId,Equal<Current<AddTrxFilter.entryTypeID>>>>>))]
		[Account(typeof(AddTrxFilter.branchID), typeof(Search2<Account.accountID,LeftJoin<CashAccount,On<CashAccount.accountID,Equal<Account.accountID>>,
													InnerJoin<CAEntryType,On<CAEntryType.entryTypeId, Equal<Current<AddTrxFilter.entryTypeID>>>>>,													
												Where<CAEntryType.useToReclassifyPayments,Equal<False>,
													Or<Where<CashAccount.cashAccountID,IsNotNull,
														And<CashAccount.curyID,Equal<Current<AddTrxFilter.curyID>>,
														And<CashAccount.cashAccountID,NotEqual<Current<AddTrxFilter.cashAccountID>>>>>>>>), DisplayName = "Offset Account", DescriptionField = typeof(Account.description))]
		public virtual Int32? AccountID
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
		public abstract class subID : PX.Data.IBqlField
		{
		}
		protected Int32? _SubID;
		[SubAccount(typeof(AddTrxFilter.accountID), DisplayName = "Offset Subaccount", DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Coalesce<Search<CashAccountETDetail.offsetSubID, Where<CashAccountETDetail.entryTypeID, Equal<Current<AddTrxFilter.entryTypeID>>,
							And<CashAccountETDetail.accountID,Equal<Current<AddTrxFilter.cashAccountID>>>>>,
				Search<CAEntryType.subID, Where<CAEntryType.entryTypeId, Equal<Current<AddTrxFilter.entryTypeID>>>>>))]
		public virtual Int32? SubID
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
		#region ReferenceID
		public abstract class referenceID : PX.Data.IBqlField
		{
		}
		protected Int32? _ReferenceID;
		[PXDBInt()]
		[PXDefault(typeof(Search<CAEntryType.referenceID, Where<CAEntryType.entryTypeId, Equal<Current<AddTrxFilter.entryTypeID>>>>))]
		[PXVendorCustomerSelector(typeof(AddTrxFilter.origModule))]
		[PXUIField(DisplayName = "Business Account", Visibility = PXUIVisibility.Visible)]
		public virtual Int32? ReferenceID
		{
			get
			{
				return this._ReferenceID;
			}
			set
			{
				this._ReferenceID = value;
			}
		}
		#endregion
		#region CuryTranAmt
		public abstract class curyTranAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTranAmt;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", Required = true)]
		[PXDBCurrency(typeof(AddTrxFilter.curyInfoID), typeof(AddTrxFilter.tranAmt))]//, BaseCalc=false)]
		public virtual Decimal? CuryTranAmt
		{
			get
			{
				return this._CuryTranAmt;
			}
			set
			{
				this._CuryTranAmt = value;
			}
		}
		#endregion
		#region TranAmt
		public abstract class tranAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranAmt;
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranAmt
		{
			get
			{
				return this._TranAmt;
			}
			set
			{
				this._TranAmt = value;
			}
		}
		#endregion
		#region Cleared
		public abstract class cleared : PX.Data.IBqlField
		{
		}
		protected Boolean? _Cleared;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Cleared")]
		public virtual Boolean? Cleared
		{
			get
			{
				return this._Cleared;
			}
			set
			{
				this._Cleared = value;
			}
		}
		#endregion
		#region ClearDate
		public abstract class clearDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _ClearDate;
		[PXDBDate(MaxValue = "06/06/2079", MinValue = "01/01/1900")]
		[PXUIField(DisplayName = "Date Cleared")]
		public virtual DateTime? ClearDate
		{
			get
			{
				return this._ClearDate;
			}
			set
			{
				this._ClearDate = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(ModuleCode = "CA")]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.IBqlField
		{
		}
		protected Int32? _LocationID;
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<AddTrxFilter.referenceID>>, And<Location.isActive, Equal<boolTrue>>>), DescriptionField = typeof(Location.descr))]
		[PXUIField(DisplayName = "Location ID")]
		[PXDefault(typeof(Search<BAccountR.defLocationID, Where<BAccountR.bAccountID,Equal<Current<AddTrxFilter.referenceID>>>>))]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
#if false
        #region PaymentMethodID
        public abstract class paymentTypeID : PX.Data.IBqlField
        {
        }
        protected String _PaymentTypeID;
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Payment Type ID")]
        [PXDefault(typeof(Search<Location.paymentMethodID, Where<Location.bAccountID, Equal<Current<AddTrxFilter.referenceID>>, And<Location.locationID, Equal<Current<AddTrxFilter.locationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search2<PaymentType.paymentTypeID, InnerJoin<CashAccountDetail, On<CashAccountDetail.paymentTypeID, Equal<PaymentType.paymentTypeID>>>, Where<CashAccountDetail.accountID, Equal<Current<AddTrxFilter.cashAccountID>>>>))]
        public virtual String PaymentTypeID
        {
            get
            {
                return this._PaymentTypeID;
            }
            set
            {
                this._PaymentTypeID = value;
            }
        }
        #endregion
#endif
        #region PaymentMethodID
        public abstract class paymentMethodID : PX.Data.IBqlField
        {
        }
        protected String _PaymentMethodID;
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Payment Method")]
        [PXDefault(typeof(Coalesce<
                            Coalesce<
                                Search2<Customer.defPaymentMethodID,InnerJoin<PaymentMethodAccount,
                                       On<PaymentMethodAccount.paymentMethodID,Equal<Customer.defPaymentMethodID>,
                                        And<PaymentMethodAccount.useForAR,Equal<True>,
                                        And<PaymentMethodAccount.cashAccountID,Equal<Current<AddTrxFilter.cashAccountID>>>>>>,
                                       Where<Current<AddTrxFilter.origModule>, Equal<GL.BatchModule.moduleAR>,
                                        And<Customer.bAccountID, Equal<Current<AddTrxFilter.referenceID>>>>>,
                                Search<PaymentMethodAccount.paymentMethodID,
                                        Where<Current<AddTrxFilter.origModule>, Equal<GL.BatchModule.moduleAR>,
                                            And<PaymentMethodAccount.useForAR, Equal<True>,
                                            And<PaymentMethodAccount.cashAccountID, Equal<Current<AddTrxFilter.cashAccountID>>>>>,
                                            OrderBy<Desc<PaymentMethodAccount.aRIsDefault>>>>,
                             Coalesce<
                                    Search2<Location.paymentMethodID,InnerJoin<PaymentMethodAccount,
                                        On<PaymentMethodAccount.paymentMethodID,Equal<Location.paymentMethodID>,
                                        And<PaymentMethodAccount.useForAP,Equal<True>,
                                        And<PaymentMethodAccount.cashAccountID,Equal<Current<AddTrxFilter.cashAccountID>>>>>>,
                                    Where<Current<AddTrxFilter.origModule>, Equal<GL.BatchModule.moduleAP>,
                                        And<Location.bAccountID, Equal<Current<AddTrxFilter.referenceID>>,
                                        And<Location.locationID, Equal<Current<AddTrxFilter.locationID>>>>>>,
                                    Search<PaymentMethodAccount.paymentMethodID,
                                        Where<Current<AddTrxFilter.origModule>, Equal<GL.BatchModule.moduleAP>,
                                        And<PaymentMethodAccount.useForAP,Equal<True>,
                                        And<PaymentMethodAccount.cashAccountID, Equal<Current<AddTrxFilter.cashAccountID>>>>>, 
                                        OrderBy<Desc<PaymentMethodAccount.aPIsDefault>>>>>),
                                        PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search2<PaymentMethod.paymentMethodID,
                            InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.paymentMethodID, Equal<PaymentMethod.paymentMethodID>,
                                And<Where2<Where<PaymentMethodAccount.useForAR, Equal<True>,
                                    And<Current<AddTrxFilter.origModule>, Equal<GL.BatchModule.moduleAR>>>,
                                Or<Where<PaymentMethodAccount.useForAP, Equal<True>,
                                    And<Current<AddTrxFilter.origModule>, Equal<GL.BatchModule.moduleAP>>>>>>>>,
                                Where<PaymentMethodAccount.cashAccountID, Equal<Current<AddTrxFilter.cashAccountID>>>>))]
        public virtual String PaymentMethodID
        {
            get
            {
                return this._PaymentMethodID;
            }
            set
            {
                this._PaymentMethodID = value;
            }
        }
        #endregion
        #region PMInstanceID
        public abstract class pMInstanceID : PX.Data.IBqlField
		{
		}
		protected Int32? _PMInstanceID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Card/Account No")]
        [PXDefault(typeof(Coalesce<
                            Search2<Customer.defPMInstanceID, InnerJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>,
                                And<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>>>>,
                                Where<Current<AddTrxFilter.origModule>,Equal<GL.BatchModule.moduleAR>, 
                                And<Customer.bAccountID, Equal<Current<AddTrxFilter.referenceID>>,
								And<CustomerPaymentMethod.isActive, Equal<True>,
                                And<CustomerPaymentMethod.paymentMethodID, Equal<Current<AddTrxFilter.paymentMethodID>>>>>>>,
                            Search<CustomerPaymentMethod.pMInstanceID,
                                Where<Current<AddTrxFilter.origModule>,Equal<GL.BatchModule.moduleAR>, 
                                    And<CustomerPaymentMethod.bAccountID, Equal<Current<AddTrxFilter.referenceID>>,
                                    And<CustomerPaymentMethod.paymentMethodID, Equal<Current<AddTrxFilter.paymentMethodID>>,
									And<CustomerPaymentMethod.isActive, Equal<True>>>>>,
								OrderBy<Desc<CustomerPaymentMethod.expirationDate,
								Desc<CustomerPaymentMethod.pMInstanceID>>>>>),
                                    PersistingCheck = PXPersistingCheck.Nothing)]
        
		/*[PXDefault(typeof(Search2<Customer.defPMInstanceID,InnerJoin< 
                            Where<Current<AddTrxFilter.origModule>,Equal<GL.BatchModule.moduleAR>, 
                                And<Customer.bAccountID,Equal<Current<AddTrxFilter.referenceID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]*/
		[PXSelector(typeof(Search2<CustomerPaymentMethod.pMInstanceID, 
								InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.paymentMethodID, 
                                    Equal<CustomerPaymentMethod.paymentMethodID>,
                                And<PaymentMethodAccount.useForAR,Equal<True>>>>, 
								Where<CustomerPaymentMethod.bAccountID, Equal<Current<AddTrxFilter.referenceID>>,
                                  And<CustomerPaymentMethod.paymentMethodID, Equal<Current<AddTrxFilter.paymentMethodID>>,   
								  And<CustomerPaymentMethod.isActive, Equal<boolTrue>, 
                                  And<PaymentMethodAccount.cashAccountID,Equal<Current<AddTrxFilter.cashAccountID>>>>>>>), 
								  DescriptionField = typeof(CustomerPaymentMethod.descr))]
		public virtual Int32? PMInstanceID
		{
			get
			{
				return this._PMInstanceID;
			}
			set
			{
				this._PMInstanceID = value;
			}
		}
		#endregion
	
		#region Status
		public abstract class status : PX.Data.IBqlField
		{ 
		}
		[PXString(11, IsFixed=true)]
		[PXUIField(DisplayName="Status", Enabled=false)]
		[GL.BatchStatus.List()]
		public virtual string Status
		{
			get
			{
				if (this._Hold == true)
				{
					return GL.BatchStatus.Hold;
				}
				else
				{
					return GL.BatchStatus.Balanced;
				}
			}
			set
			{ 
			}
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.IBqlField
		{
		}
		protected Boolean? _Hold;
		[PXDBBool()]
		[PXDefault(typeof(Search<CASetup.holdEntry>))]
		[PXUIField(DisplayName = "Hold")]
		public virtual Boolean? Hold
		{
			get
			{
				return this._Hold;
			}
			set
			{
				this._Hold = value;
			}
		}
		#endregion
		#region Methods
		public static CATran AddAPTransaction(PXGraph graph, PXFilter<AddTrxFilter> AddFilter, PXSelectBase<CurrencyInfo> currencyinfo)
		{
			AddTrxFilter parameters = AddFilter.Current;
			CurrencyInfo defcurrencyinfo = currencyinfo.Current;

			if (parameters.OrigModule == GL.BatchModule.AP)
			{
				if (parameters.CashAccountID == null)
				{
					AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.cashAccountID>(parameters,
						null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.cashAccountID).Name));

					throw new PXRowPersistingException(typeof(AddTrxFilter.cashAccountID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.cashAccountID).Name);
				}

				if (string.IsNullOrEmpty(parameters.EntryTypeID))
				{
					AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.entryTypeID>(parameters,
						null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.entryTypeID).Name));

					throw new PXRowPersistingException(typeof(AddTrxFilter.entryTypeID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.entryTypeID).Name);
				}

				if (parameters.ReferenceID == null)
				{
					AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.referenceID>(parameters,
						null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.referenceID).Name));

					throw new PXRowPersistingException(typeof(AddTrxFilter.referenceID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.referenceID).Name);
				}

				if (parameters.LocationID == null)
				{
					AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.locationID>(parameters,
						null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.locationID).Name));

					throw new PXRowPersistingException(typeof(AddTrxFilter.locationID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.locationID).Name);
				}

				if (string.IsNullOrEmpty(parameters.PaymentMethodID))
				{
					AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.paymentMethodID>(parameters,
						null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.paymentMethodID).Name));

					throw new PXRowPersistingException(typeof(AddTrxFilter.paymentMethodID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.paymentMethodID).Name);
				}


				PXFieldState state = (PXFieldState)AddFilter.Cache.GetStateExt(parameters, typeof(AddTrxFilter.extRefNbr).Name);

				if ((string.IsNullOrEmpty(parameters.ExtRefNbr)) && (state != null) && (state.Enabled == true))
				{
					AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.extRefNbr>(parameters,
						null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.extRefNbr).Name));

					throw new PXRowPersistingException(typeof(AddTrxFilter.extRefNbr).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.extRefNbr).Name);
				}
				APPaymentEntry te = PXGraph.CreateInstance<APPaymentEntry>();

				Vendor vend = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(te, parameters.ReferenceID);

				if (vend == null)
				{
					AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.referenceID>(parameters,
						null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.referenceID).Name));

					throw new PXRowPersistingException(typeof(AddTrxFilter.referenceID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.referenceID).Name);
				}

				te.Document.View.Answer = WebDialogResult.No;

				APPayment doc = new APPayment();

				if (parameters.DrCr == CADrCr.CACredit)
				{
					doc.DocType = APDocType.Prepayment;
				}
				else
				{
					doc.DocType = APDocType.Refund;
				}

				doc = PXCache<APPayment>.CreateCopy(te.Document.Insert(doc));

				doc.VendorID		 = parameters.ReferenceID;
				doc.VendorLocationID = parameters.LocationID;
				doc.CashAccountID	 = parameters.CashAccountID;
				doc.PaymentMethodID	 = parameters.PaymentMethodID;
				doc.CuryID			 = parameters.CuryID;
				doc.CuryOrigDocAmt	 = parameters.CuryTranAmt;
				doc.DocDesc			 = parameters.Descr;
				doc.Cleared			 = parameters.Cleared;
				doc.AdjDate			 = parameters.TranDate;
				doc.FinPeriodID		 = parameters.FinPeriodID;
				doc.ExtRefNbr		 = parameters.ExtRefNbr;
				doc.Hold			 = true;
				doc = PXCache<APPayment>.CreateCopy(te.Document.Update(doc));

				foreach (CurrencyInfo info in PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APPayment.curyInfoID>>>>.Select(te))
				{
					CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(defcurrencyinfo);
					new_info.CuryInfoID = info.CuryInfoID;
					te.currencyinfo.Cache.Update(new_info);
				}

				te.Save.Press();

				return (CATran)PXSelect<CATran, Where<CATran.tranID, Equal<Current<APPayment.cATranID>>>>.Select(te);
			}
			return null;
		}

		public static CATran AddARTransaction(PXGraph graph, PXFilter<AddTrxFilter> AddFilter, PXSelectBase<CurrencyInfo> currencyinfo)
		{
			AddTrxFilter parameters = AddFilter.Current;
			CurrencyInfo defcurrencyinfo = currencyinfo.Current;

			if (parameters.OrigModule == GL.BatchModule.AR)
			{
				if (parameters.CashAccountID == null)
				{
					AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.cashAccountID>(parameters,
						null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.cashAccountID).Name));

					throw new PXRowPersistingException(typeof(AddTrxFilter.cashAccountID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.cashAccountID).Name);
				}

				if (string.IsNullOrEmpty(parameters.EntryTypeID))
				{
					AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.entryTypeID>(parameters,
						null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.entryTypeID).Name));

					throw new PXRowPersistingException(typeof(AddTrxFilter.entryTypeID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.entryTypeID).Name);
				}

				if (parameters.ReferenceID == null)
				{
					AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.referenceID>(parameters,
						null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.referenceID).Name));

					throw new PXRowPersistingException(typeof(AddTrxFilter.referenceID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.referenceID).Name);
				}

				if (parameters.LocationID == null)
				{
					AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.locationID>(parameters,
						null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.locationID).Name));

					throw new PXRowPersistingException(typeof(AddTrxFilter.locationID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.locationID).Name);
				}

                if (String.IsNullOrEmpty(parameters.PaymentMethodID))
                {
                    AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.paymentMethodID>(parameters,
                        null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.paymentMethodID).Name));

                    throw new PXRowPersistingException(typeof(AddTrxFilter.paymentMethodID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.paymentMethodID).Name);
                }

				if (parameters.PMInstanceID == null)
				{
                    bool isPMInstanceRequired = false;
                    if (String.IsNullOrEmpty(parameters.PaymentMethodID) == false)
                    {
                        PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(graph, parameters.PaymentMethodID);
                        isPMInstanceRequired = (pm.IsAccountNumberRequired == true);                        
                    }
                    if (isPMInstanceRequired)
                    {
                        AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.pMInstanceID>(parameters,
                            null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.pMInstanceID).Name));
                        throw new PXRowPersistingException(typeof(AddTrxFilter.pMInstanceID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.pMInstanceID).Name);
                    }
				}
				ARPaymentEntry te = PXGraph.CreateInstance<ARPaymentEntry>();
				Customer cust = PXSelect<Customer,
												 Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.
												 Select(te, parameters.ReferenceID);

				if (cust == null)
				{
					AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.referenceID>(parameters,
						null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.referenceID).Name));

					throw new PXRowPersistingException(typeof(AddTrxFilter.referenceID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.referenceID).Name);
				}
				
				ARPayment doc = new ARPayment();

				if (parameters.DrCr == CADrCr.CACredit)
				{
					doc.DocType = ARDocType.Refund;
				}
				else
				{
					doc.DocType = ARDocType.Payment;
				}

				doc = PXCache<ARPayment>.CreateCopy(te.Document.Insert(doc));

				doc.CustomerID			= parameters.ReferenceID;
				doc.CustomerLocationID  = parameters.LocationID;
                doc.PaymentMethodID     = parameters.PaymentMethodID;
				doc.PMInstanceID		= parameters.PMInstanceID;
				doc.CashAccountID		= parameters.CashAccountID;
				doc.CuryOrigDocAmt		= parameters.CuryTranAmt;
				doc.DocDesc				= parameters.Descr;
				doc.Cleared				= parameters.Cleared;
				doc.AdjDate				= parameters.TranDate;
				doc.FinPeriodID			= parameters.FinPeriodID;
				doc.ExtRefNbr			= parameters.ExtRefNbr;
				doc.Hold				= true;
				doc = PXCache<ARPayment>.CreateCopy(te.Document.Update(doc));

				foreach (CurrencyInfo info in PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<ARPayment.curyInfoID>>>>.Select(te))
				{
					CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(defcurrencyinfo);
					new_info.CuryInfoID = info.CuryInfoID;
					te.currencyinfo.Cache.Update(new_info);
				}

				te.Save.Press();

				return (CATran)PXSelect<CATran, Where<CATran.tranID, Equal<Current<ARPayment.cATranID>>>>.Select(te);
			}
			return null;
		}

		public static CATran AddCATransaction(PXGraph graph, PXFilter<AddTrxFilter> AddFilter, PXSelectBase<CurrencyInfo> currencyinfo)
		{
			return AddCATransaction(graph, AddFilter, currencyinfo, false);
		}

		public static CATran AddCATransaction(CashTransferEntry graph, PXFilter<AddTrxFilter> AddFilter, PXSelectBase<CurrencyInfo> currencyinfo)
		{
			return AddCATransaction(graph, AddFilter, currencyinfo, true);
		}

		public static CATran AddCATransaction(PXGraph graph, PXFilter<AddTrxFilter> AddFilter, PXSelectBase<CurrencyInfo> currencyinfo, bool IsTransferExpense)
		{
			AddTrxFilter parameters = AddFilter.Current;

			if (parameters.OrigModule == GL.BatchModule.CA)
			{
				if (parameters.CashAccountID == null)
				{
					AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.cashAccountID>(parameters,
						null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.cashAccountID).Name));

					throw new PXRowPersistingException(typeof(AddTrxFilter.cashAccountID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.cashAccountID).Name);
				} 
				
				if (string.IsNullOrEmpty(parameters.EntryTypeID))
				{
					AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.entryTypeID>(parameters,
						null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.entryTypeID).Name));

					throw new PXRowPersistingException(typeof(AddTrxFilter.entryTypeID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.entryTypeID).Name);
				}

				if (string.IsNullOrEmpty(parameters.ExtRefNbr))
				{
					AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.extRefNbr>(parameters,
						null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.extRefNbr).Name));

					throw new PXRowPersistingException(typeof(AddTrxFilter.extRefNbr).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.extRefNbr).Name);
				}

				if (parameters.AccountID == null)
				{
					AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.accountID>(parameters,
						null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.accountID).Name));

					throw new PXRowPersistingException(typeof(AddTrxFilter.accountID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.accountID).Name);
				}

				if (parameters.SubID == null)
				{
					AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.subID>(parameters,
						null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.subID).Name));

					throw new PXRowPersistingException(typeof(AddTrxFilter.subID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.subID).Name);
				}

				CATranEntry te = PXGraph.CreateInstance<CATranEntry>();
				CurrencyInfo info = new CurrencyInfo();
				
				CashAccount cashacct = (CashAccount)PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(graph, parameters.CashAccountID);
				if ((cashacct != null) && (cashacct.CuryRateTypeID != null))
				{
					info.CuryID = cashacct.CuryID;
					currencyinfo.Cache.RaiseFieldUpdated<CurrencyInfo.curyID>(info, null);
                    
                    CurrencyInfo filterInfo = currencyinfo.Search<CurrencyInfo.curyInfoID>(parameters.CuryInfoID);
                    info.CuryRateTypeID = filterInfo.CuryRateTypeID;
					currencyinfo.Cache.RaiseFieldUpdated<CurrencyInfo.curyRateTypeID>(info, null);
                    info.SetCuryEffDate(te.currencyinfo.Cache, parameters.TranDate);
					info = te.currencyinfo.Insert(info);
				}
				
				CAAdj adj = new CAAdj();
				adj.AdjTranType = (IsTransferExpense ? CATranType.CATransferExp : CATranType.CAAdjustment);
				if (IsTransferExpense)
				{
					adj.TransferNbr = (graph as CashTransferEntry).Transfer.Current.TransferNbr;
				}
				adj.CashAccountID	= parameters.CashAccountID;
				adj.CuryID			= parameters.CuryID;
				adj.CuryInfoID		= info.CuryInfoID;
				adj.DrCr			= parameters.DrCr;
				adj.ExtRefNbr		= parameters.ExtRefNbr;
				adj.Released		= false;
				adj.Cleared			= parameters.Cleared;
				adj.TranDate		= parameters.TranDate;
				adj.TranDesc		= parameters.Descr;
				adj.EntryTypeID		= parameters.EntryTypeID;
				adj.CuryControlAmt	= parameters.CuryTranAmt;
				adj.Hold		    = parameters.Hold;
				adj.TaxZoneID = parameters.TaxZoneID;
				adj = te.CAAdjRecords.Insert(adj);

				CASplit split		= new CASplit();
				split.AdjTranType	= adj.AdjTranType;
				split.AccountID		= parameters.AccountID;
				split.CuryInfoID	= info.CuryInfoID;
				split.Qty			= (decimal)1.0;
				split.CuryUnitPrice = parameters.CuryTranAmt;
				split.CuryTranAmt	= parameters.CuryTranAmt;
				split.TranDesc		= parameters.Descr;
				split.SubID			= parameters.SubID;
				te.CASplitRecords.Insert(split);

				te.Save.Press();
				adj = (CAAdj) te.Caches[typeof(CAAdj)].Current;
				return (CATran)PXSelect<CATran, Where<CATran.tranID, Equal<Required<CAAdj.tranID>>>>.Select(te, adj.TranID);
			}
			return null;
		}

		public static void ClearAll(PXGraph graph, PXFilter<AddTrxFilter> AddFilter)
		{
			Clear(graph, AddFilter);
			AddTrxFilter parameters = AddFilter.Current;

			parameters.CashAccountID = null;
		}

		public static void Clear(PXGraph graph, PXFilter<AddTrxFilter> AddFilter)
		{
			AddTrxFilter parameters = AddFilter.Current;

			parameters.AccountID		= null;
			parameters.SubID			= null;
			parameters.CuryTranAmt		= (decimal)0.0;
			parameters.Descr			= null;
			parameters.EntryTypeID		= null;
			parameters.ExtRefNbr		= null;
			parameters.OrigModule		= null;
			parameters.ReferenceID		= null;
			parameters.LocationID		= null;
			parameters.Cleared			= null;
			parameters.Hold				= null;
			parameters.PaymentMethodID	= null;
			parameters.PMInstanceID		= null;
			parameters.PaymentMethodID	= null;
			AddFilter.Cache.SetDefaultExt<AddTrxFilter.tranDate>(parameters);
		}
		#endregion
		#region Descriptor
		public class AddFilterAttribute : PXEventSubscriberAttribute, IPXRowSelectedSubscriber
		{
			public AddFilterAttribute()
				: base()
			{
			}

			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);
				sender.Graph.FieldUpdated.AddHandler<AddTrxFilter.entryTypeID>	(AddTrxFilter_EntryTypeId_FieldUpdated);
				sender.Graph.FieldUpdated.AddHandler<AddTrxFilter.drCr>			(AddTrxFilter_DrCr_FieldUpdated);
				sender.Graph.FieldUpdated.AddHandler<AddTrxFilter.referenceID>  (AddTrxFilter_ReferenceID_FieldUpdated);
				sender.Graph.FieldUpdated.AddHandler<AddTrxFilter.tranDate>		(AddTrxFilter_TranDate_FieldUpdated);
				sender.Graph.FieldUpdated.AddHandler<AddTrxFilter.cashAccountID>(AddTrxFilter_CashAccountID_FieldUpdated);
                sender.Graph.FieldUpdated.AddHandler<AddTrxFilter.paymentMethodID>(AddTrxFilter_PaymentMethodID_FieldUpdated);
			}

			public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				AddTrxFilter row = (AddTrxFilter)e.Row;
				if (row != null)
				{
					CASetup casetup = (CASetup)PXSelect<CASetup>.Select(sender.Graph);

					PXUIFieldAttribute.SetEnabled<AddTrxFilter.curyID>(sender, row, false);
					PXUIFieldAttribute.SetVisible<AddTrxFilter.accountID>(sender, row, (row.OrigModule == GL.BatchModule.CA));
					PXUIFieldAttribute.SetVisible<AddTrxFilter.subID>(sender, row, (row.OrigModule == GL.BatchModule.CA));
					PXUIFieldAttribute.SetVisible<AddTrxFilter.referenceID>(sender, row, (row.OrigModule == GL.BatchModule.AP || row.OrigModule == GL.BatchModule.AR));
					PXUIFieldAttribute.SetVisible<AddTrxFilter.locationID>(sender, row, (row.OrigModule == GL.BatchModule.AP || row.OrigModule == GL.BatchModule.AR));
					PXUIFieldAttribute.SetVisible<AddTrxFilter.paymentMethodID>(sender, row, (row.OrigModule == GL.BatchModule.AP || row.OrigModule == GL.BatchModule.AR));
					PXUIFieldAttribute.SetVisible<AddTrxFilter.pMInstanceID>(sender, row, (row.OrigModule == GL.BatchModule.AR));
					PXUIFieldAttribute.SetVisible<AddTrxFilter.extRefNbr>(sender, row, true);
					PXUIFieldAttribute.SetVisible<AddTrxFilter.drCr>(sender, row, false);
					PXUIFieldAttribute.SetEnabled<AddTrxFilter.cashAccountID>(sender, row, false);
					PXUIFieldAttribute.SetVisible<AddTrxFilter.origModule>(sender, row, false);
					PXUIFieldAttribute.SetVisible<AddTrxFilter.hold>(sender, row, (row.OrigModule == GL.BatchModule.CA) && (casetup.HoldEntry == true));
					PXUIFieldAttribute.SetVisible<AddTrxFilter.taxZoneID>(sender, row, (row.OrigModule == GL.BatchModule.CA && row.DrCr == "C"));

					CashAccount acct = (CashAccount)PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(sender.Graph, row.CashAccountID);
					bool needReconcilation = (acct != null) && (acct.Reconcile == true);

					PXUIFieldAttribute.SetEnabled<AddTrxFilter.cleared>(sender, row, needReconcilation);
					PXUIFieldAttribute.SetEnabled<AddTrxFilter.clearDate>(sender, row, needReconcilation && (row.Cleared == true));

					if (row.OrigModule == GL.BatchModule.AP)
					{
						PaymentMethodAccount cDet = PXSelect<PaymentMethodAccount,
													Where<PaymentMethodAccount.cashAccountID, Equal<Current<AddTrxFilter.cashAccountID>>,
													And<PaymentMethodAccount.paymentMethodID, Equal<Required<AddTrxFilter.paymentMethodID>>,
													And<PaymentMethodAccount.useForAP, Equal<True>>>>>.Select(sender.Graph);
						if ((cDet != null) && (cDet.APAutoNextNbr == true))
						{
							PXUIFieldAttribute.SetEnabled<AddTrxFilter.extRefNbr>(sender, row, false);
						}
						else
						{
							PXUIFieldAttribute.SetEnabled<AddTrxFilter.extRefNbr>(sender, row, true);
						}
					}
					if (row.OrigModule == GL.BatchModule.AR)
					{
						bool isInstanceRequired = false;
						if (String.IsNullOrEmpty(row.PaymentMethodID) == false)
						{
							PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(sender.Graph, row.PaymentMethodID);
							isInstanceRequired = (pm!= null && pm.IsAccountNumberRequired == true);
						}
						PXUIFieldAttribute.SetEnabled<AddTrxFilter.pMInstanceID>(sender, row, isInstanceRequired);
					}
					if (row.OrigModule == GL.BatchModule.CA) 
					{
						bool isCashAcct = false;
						if( String.IsNullOrEmpty(row.EntryTypeID)==false)
						{
							if (row.AccountID.HasValue)
							{
								CashAccount offsetCashAcct = (CashAccount)PXSelectReadonly<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(sender.Graph, row.AccountID);
								isCashAcct = offsetCashAcct != null && offsetCashAcct.AccountID.HasValue;								
							}
							else
							{
								CAEntryType entryType = PXSelectReadonly<CAEntryType, Where<CAEntryType.entryTypeId, Equal<Required<CAEntryType.entryTypeId>>>>.Select(sender.Graph, row.EntryTypeID);
								if (entryType != null && entryType.UseToReclassifyPayments == true)
								{
									CashAccount availableAccount = PXSelect<CashAccount, Where<CashAccount.cashAccountID, NotEqual<Required<CashAccount.cashAccountID>>,
																		And<CashAccount.curyID, Equal<Required<CashAccount.curyID>>>>>.Select(sender.Graph, row.CashAccountID, row.CuryID);
									if (availableAccount == null)
									{
										sender.RaiseExceptionHandling<AddTrxFilter.accountID>(row, null, new PXSetPropertyKeepPreviousException(Messages.EntryTypeRequiresCashAccountButNoOneIsConfigured, PXErrorLevel.Warning, row.CuryID));
										sender.RaiseExceptionHandling<AddTrxFilter.entryTypeID>(row, null, new PXSetPropertyException(Messages.EntryTypeRequiresCashAccountButNoOneIsConfigured, row.CuryID, PXErrorLevel.Warning));
									}
									else
									{
										sender.RaiseExceptionHandling<AddTrxFilter.accountID>(row, null, null);
										sender.RaiseExceptionHandling<AddTrxFilter.entryTypeID>(row, null, null);
									}
								}
								else 
								{
									sender.RaiseExceptionHandling<AddTrxFilter.accountID>(row, null, null);
									sender.RaiseExceptionHandling<AddTrxFilter.entryTypeID>(row, null, null);
								}
							}
						}
						PXUIFieldAttribute.SetEnabled<AddTrxFilter.subID>(sender, row, isCashAcct);
					}
				}
			}

			public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
			{
				AddTrxFilter row = (AddTrxFilter)e.Row;
				sender.Current = row;
				if (row.DrCr == "D")
				{
					sender.SetValueExt<AddTrxFilter.taxZoneID>(row, null);
				}
			}

			public virtual void AddTrxFilter_EntryTypeId_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				AddTrxFilter row = (AddTrxFilter)e.Row;
				sender.SetDefaultExt<AddTrxFilter.origModule>(row);
				sender.SetDefaultExt<AddTrxFilter.drCr>(row);
				sender.RaiseExceptionHandling<AddTrxFilter.accountID>(row, null, null);
				sender.RaiseExceptionHandling<AddTrxFilter.entryTypeID>(row, null, null);

				sender.SetDefaultExt<AddTrxFilter.descr>(row);
				sender.SetDefaultExt<AddTrxFilter.accountID>  (row);
				sender.SetDefaultExt<AddTrxFilter.subID>	  (row);
				sender.SetDefaultExt<AddTrxFilter.taxZoneID>(row);			
				sender.SetDefaultExt<AddTrxFilter.referenceID>(row);
				sender.SetValue<AddTrxFilter.locationID>	(row, null);
				sender.SetValue<AddTrxFilter.paymentMethodID> (row, null);
				sender.SetValue<AddTrxFilter.pMInstanceID>	(row, null);

			}

			public virtual void AddTrxFilter_DrCr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				AddTrxFilter row = e.Row as AddTrxFilter;

				if (row.ExtRefNbr != null)
				{
					row.ExtRefNbr = null;
				}
			}

			public virtual void AddTrxFilter_TranDate_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
			{
				AddTrxFilter row = (AddTrxFilter)e.Row;
				if (row == null) return;

				CashAccount acct = (CashAccount)PXSelect<CashAccount,Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(sender.Graph, row.CashAccountID);
				if ((acct != null) && (acct.Reconcile != true))
				{
					row.ClearDate = row.TranDate;
				}
			}

			public virtual void AddTrxFilter_ReferenceID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				AddTrxFilter row = e.Row as AddTrxFilter;
				string error = null;
				switch(row.OrigModule)
				{
					case GL.BatchModule.AP:
				
						sender.SetDefaultExt<AddTrxFilter.locationID>   (row);
						sender.SetDefaultExt<AddTrxFilter.paymentMethodID>(row);

						error = PXUIFieldAttribute.GetError<AddTrxFilter.paymentMethodID>(sender, row);
						if (string.IsNullOrEmpty(error) == false)
						{
							sender.SetValue<AddTrxFilter.paymentMethodID>(row, null);
							PXUIFieldAttribute.SetError<AddTrxFilter.paymentMethodID>(sender, row, null, null);
						}

						sender.SetValue<AddTrxFilter.pMInstanceID>(row, null);
						break;

					case GL.BatchModule.AR:
					
						sender.SetDefaultExt<AddTrxFilter.locationID>  (row);
						//sender.SetValue<AddTrxFilter.paymentMethodID>    (row, null);
                        sender.SetDefaultExt<AddTrxFilter.paymentMethodID>(row);
                        error = PXUIFieldAttribute.GetError<AddTrxFilter.paymentMethodID>(sender, row);
                        if (string.IsNullOrEmpty(error) == false)
                        {
                            sender.SetValue<AddTrxFilter.paymentMethodID>(row, null);
                            PXUIFieldAttribute.SetError<AddTrxFilter.paymentMethodID>(sender, row, null, null);
                        }
						sender.SetDefaultExt<AddTrxFilter.pMInstanceID>(row);

						error = PXUIFieldAttribute.GetError<AddTrxFilter.pMInstanceID>(sender, row);
						if (string.IsNullOrEmpty(error) == false)
						{
							sender.SetValue<AddTrxFilter.pMInstanceID>(row, null);
							PXUIFieldAttribute.SetError<AddTrxFilter.pMInstanceID>(sender, row, null, null);
						}
					break;
					default:
						sender.SetValue<AddTrxFilter.locationID>   (row, null);
						sender.SetValue<AddTrxFilter.paymentMethodID>(row, null);
						sender.SetValue<AddTrxFilter.pMInstanceID> (row, null);
					break;
				}
			}

            public virtual void AddTrxFilter_PaymentMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
            {
                AddTrxFilter row = e.Row as AddTrxFilter;
                string error = null;
                if (row.OrigModule == GL.BatchModule.AR)
                {
                    sender.SetDefaultExt<AddTrxFilter.pMInstanceID>(row);
                    error = PXUIFieldAttribute.GetError<AddTrxFilter.pMInstanceID>(sender, row);
                    if (string.IsNullOrEmpty(error) == false)
                    {
                        sender.SetValue<AddTrxFilter.pMInstanceID>(row, null);
                        PXUIFieldAttribute.SetError<AddTrxFilter.pMInstanceID>(sender, row, null, null);
                    }
                }
            }

			public virtual void AddTrxFilter_TranDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				AddTrxFilter row = e.Row as AddTrxFilter;
				PXCache currencycache = sender.Graph.Caches[typeof(CurrencyInfo)];

				CurrencyInfoAttribute.SetEffectiveDate<AddTrxFilter.tranDate>(sender, e);
			}

			public virtual void AddTrxFilter_CashAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				AddTrxFilter row = e.Row as AddTrxFilter;
				CashAccount cashacct = (CashAccount)PXSelectorAttribute.Select<AddTrxFilter.cashAccountID>(sender, row);
				sender.SetDefaultExt<AddTrxFilter.curyID>(row);

                PXView view = sender.Graph.Views["_" + typeof(AddTrxFilter).Name + "_CurrencyInfo_"];
                CurrencyInfo info = view.SelectSingle(row.CuryInfoID) as CurrencyInfo;
                view.Cache.SetValueExt<CurrencyInfo.curyRateTypeID>(info, cashacct.CuryRateTypeID);

				sender.SetDefaultExt<AddTrxFilter.branchID>(row);
				sender.SetDefaultExt<AddTrxFilter.entryTypeID>(row);
				if (cashacct != null)
				{
					row.Cleared   = false;
					row.ClearDate = null;

					if (cashacct.Reconcile != true)
					{
						row.Cleared   = true;
						row.ClearDate = row.TranDate;
					}
				}
			}
		}
		#endregion
	}
	
    [Serializable]
	public partial class CashAccountFilter : PX.Data.IBqlTable
	{
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _CashAccountID;
		[PXDefault()]
		[GL.CashAccount(DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(CashAccount.descr))]
		public virtual Int32? CashAccountID
		{
			get
			{
				return this._CashAccountID;
			}
			set
			{
				this._CashAccountID = value;
			}
		}
		#endregion
	}
}
