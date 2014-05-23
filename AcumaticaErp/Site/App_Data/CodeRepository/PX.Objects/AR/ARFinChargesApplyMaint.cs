using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.CR;

namespace PX.Objects.AR
{
	[TableAndChartDashboardType]
    [Serializable]
	public class ARFinChargesApplyMaint : PXGraph<ARFinChargesApplyMaint>
	{
		#region Internal Types

		[System.SerializableAttribute()]
		public partial class ARFinChargesApplyParameters : IBqlTable
		{
			#region StatementCycle
			public abstract class statementCycle : PX.Data.IBqlField
			{
			}
			protected String _StatementCycle;
			[PXDBString(10, IsUnicode = true)]
			[PXDefault(typeof(ARStatementCycle.statementCycleId))]
			[PXUIField(DisplayName = "Statement Cycle", Visibility = PXUIVisibility.SelectorVisible)]
			[PXSelector(typeof(ARStatementCycle.statementCycleId), DescriptionField = typeof(ARStatementCycle.descr))]
			public virtual String StatementCycle
			{
				get
				{
					return this._StatementCycle;
				}
				set
				{
					this._StatementCycle = value;
				}
			}
			#endregion
			#region CustomerClassID
			public abstract class customerClassID : PX.Data.IBqlField
			{
			}
			protected String _CustomerClassID;
			[PXDBString(10, IsUnicode = true)]
			[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
			[PXSelector(typeof(CustomerClass.customerClassID), DescriptionField = typeof(CustomerClass.descr))]
			[PXUIField(DisplayName = "Customer Class")]

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
			#region CustomerID
			public abstract class customerID : PX.Data.IBqlField
			{
			}
			protected Int32? _CustomerID;
			[PXDBInt()]
			[PXUIField(DisplayName = "Customer", Visibility = PXUIVisibility.SelectorVisible)]
			[CustomerActive(DescriptionField = typeof(Customer.acctName))]
			public virtual Int32? CustomerID
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
			#region FinChargeDate
			public abstract class finChargeDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _FinChargeDate;
			[PXDate()]
			[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Null)]
			[PXUIField(DisplayName = "Overdue Charge Date", Visibility = PXUIVisibility.Visible, Required = true)]
			public virtual DateTime? FinChargeDate
			{
				get
				{
					return this._FinChargeDate;
				}
				set
				{
					this._FinChargeDate = value;
				}
			}
			#endregion
			#region FinPeriodID
			public abstract class finPeriodID : PX.Data.IBqlField
			{
			}
			protected String _FinPeriodID;
			[FinPeriodSelector(typeof(ARFinChargesApplyParameters.finChargeDate))]
			[PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.Visible)]
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

		}

        [Serializable]
		public partial class ARFinChargesDetails : IBqlTable
		{
			#region Selected
			public abstract class selected : IBqlField
			{
			}
			protected bool? _Selected = false;
			[PXBool]
			[PXDefault(true)]
			[PXUIField(DisplayName = "Selected")]
			public bool? Selected
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
			#region DocType
			public abstract class docType : PX.Data.IBqlField
			{
			}
			protected String _DocType;
			[PXString(3, IsKey = true, IsFixed = true)]
			[PXDefault()]
			[ARInvoiceType.List()]
			[PXUIField(DisplayName = "Document Type", Enabled = false)]
			public virtual String DocType
			{
				get
				{
					return this._DocType;
				}
				set
				{
					this._DocType = value;
				}
			}
			#endregion
			#region RefNbr
			public abstract class refNbr : PX.Data.IBqlField
			{
			}
			protected String _RefNbr;
			[PXDBString(15, IsUnicode = true, IsKey = true)]
			[PXDefault()]
			[PXUIField(DisplayName = "Document Number", Enabled = false)]
			//[PXSelector(typeof(Search<ARRegister.refNbr, Where<ARRegister.docType, Equal<Optional<ARRegister.docType>>>>))]
			public virtual String RefNbr
			{
				get
				{
					return this._RefNbr;
				}
				set
				{
					this._RefNbr = value;
				}
			}
			#endregion
			#region DocDate
			public abstract class docDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _DocDate;
			[PXDate()]
			[PXUIField(DisplayName = "Document Date", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual DateTime? DocDate
			{
				get
				{
					return this._DocDate;
				}
				set
				{
					this._DocDate = value;
				}
			}
			#endregion
			#region CustomerID
			public abstract class customerID : PX.Data.IBqlField
			{
			}
			protected Int32? _CustomerID;
			[CustomerActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), Enabled = false)]
			[PXDefault()]
			public virtual Int32? CustomerID
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
			#region CuryID
			public abstract class curyID : PX.Data.IBqlField
			{
			}
			protected String _CuryID;
			[PXString(5, IsUnicode = true, InputMask = ">LLLLL")]
			[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			[PXSelector(typeof(Currency.curyID))]
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
			#region CuryOrigDocAmt
			public abstract class curyOrigDocAmt : PX.Data.IBqlField
			{
			}
			protected Decimal? _CuryOrigDocAmt;
			[PXDecimal(4)]
			[PXUIField(DisplayName = "Orig. Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual Decimal? CuryOrigDocAmt
			{
				get
				{
					return this._CuryOrigDocAmt;
				}
				set
				{
					this._CuryOrigDocAmt = value;
				}
			}
			#endregion
			#region CuryDocBal
			public abstract class curyDocBal : PX.Data.IBqlField
			{
			}
			protected Decimal? _CuryDocBal;
			[PXDecimal(4)]
			[PXUIField(DisplayName = "Open Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual Decimal? CuryDocBal
			{
				get
				{
					return this._CuryDocBal;
				}
				set
				{
					this._CuryDocBal = value;
				}
			}
			#endregion
			#region OverdueDays
			public abstract class overdueDays : PX.Data.IBqlField
			{
			}
			protected Int16? _OverdueDays;
			[PXShort()]
			[PXDefault((short)0)]
			[PXUIField(DisplayName = "Overdue Days", Enabled = false)]
			public virtual Int16? OverdueDays
			{
				get
				{
					return this._OverdueDays;
				}
				set
				{
					this._OverdueDays = value;
				}
			}
			#endregion
			#region FinChargeID
			public abstract class finChargeID : PX.Data.IBqlField
			{
			}
			protected String _FinChargeID;
			[PXString(10, IsUnicode = true)]
			[PXDefault()]
			[PXUIField(DisplayName = "Overdue Charge ID", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual String FinChargeID
			{
				get
				{
					return this._FinChargeID;
				}
				set
				{
					this._FinChargeID = value;
				}
			}
			#endregion
			#region TermsID
			public string TermsID;
			#endregion
			#region FinChargeCuryID
			public abstract class finChargeCuryID : PX.Data.IBqlField
			{
			}
			protected String _FinChargeCuryID;
			[PXString(5, IsUnicode = true, InputMask = ">LLLLL")]
			[PXUIField(DisplayName = "Charge Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			[PXDefault(typeof(Search<Company.baseCuryID>))]
			[PXSelector(typeof(Currency.curyID))]
			public virtual String FinChargeCuryID
			{
				get
				{
					return this._FinChargeCuryID;
				}
				set
				{
					this._FinChargeCuryID = value;
				}
			}
			#endregion
			#region FinChargeAmt
			public abstract class finChargeAmt : PX.Data.IBqlField
			{
			}
			protected Decimal? _FinChargeAmt;
			[PXDBCury(typeof(ARFinChargesDetails.curyID))]
			[PXUIField(DisplayName = "Charge Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual Decimal? FinChargeAmt
			{
				get
				{
					return this._FinChargeAmt;
				}
				set
				{
					this._FinChargeAmt = value;
				}
			}
			#endregion
			#region ARAccountID
			public abstract class aRAccountID : PX.Data.IBqlField
			{
			}
			protected Int32? _ARAccountID;
			[PXDefault]
			[Account(DisplayName = "AR Account", Visibility = PXUIVisibility.Visible)]
			public virtual Int32? ARAccountID
			{
				get
				{
					return this._ARAccountID;
				}
				set
				{
					this._ARAccountID = value;
				}
			}
			#endregion
			#region ARSubID
			public abstract class aRSubID : PX.Data.IBqlField
			{
			}
			protected Int32? _ARSubID;
			[PXDefault]
			[SubAccount(DisplayName = "AR Sub.", Visibility = PXUIVisibility.Visible)]
			public virtual Int32? ARSubID
			{
				get
				{
					return this._ARSubID;
				}
				set
				{
					this._ARSubID = value;
				}
			}
			#endregion
			#region CuryRate
			public abstract class curyRate : PX.Data.IBqlField
			{
			}
			protected Decimal? _CuryRate;
			[PXDBDecimal(6)]
			[PXDefault(TypeCode.Decimal, "1.0")]
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
			protected String _CuryMultDiv;
			[PXDBString(1, IsFixed = true)]
			[PXDefault("M")]
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
			#region CuryRateTypeID
			public abstract class curyRateTypeID : PX.Data.IBqlField
			{
			}
			protected String _CuryRateTypeID;
			[PXDBString(6, IsUnicode = true)]
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
			#region SampleCuryRate
			public abstract class sampleCuryRate : PX.Data.IBqlField
			{
			}
			[PXDBDecimal(6)]
			[PXUIField(DisplayName = "Currency Rate")]
			public virtual Decimal? SampleCuryRate
			{
				[PXDependsOnFields(typeof(curyMultDiv),typeof(curyRate))]
				get
				{
					return (this._CuryMultDiv == "M") ? this._CuryRate : 1 / this._CuryRate ;
				}
				set
				{
				}
			}
			#endregion
			#region HasUnreleasedCharges
			public abstract class hasUnreleasedCharges : PX.Data.IBqlField
			{
			}
			protected bool? _HasUnreleasedCharges;
			[PXDBBool()]
			[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual bool? HasUnreleasedCharges
			{
				get
				{
					return this._HasUnreleasedCharges;
				}
				set
				{
					this._HasUnreleasedCharges = value;
				}
			}
			#endregion
			#region LastFCDocType
			public abstract class lastFCDocType : PX.Data.IBqlField
			{
			}
			protected String _LastFCDocType;
			[PXString(3, IsFixed = true)]
			[PXDefault()]
			public virtual String LastFCDocType
			{
				get
				{
					return this._LastFCDocType;
				}
				set
				{
					this._LastFCDocType = value;
				}
			}
			#endregion
			#region LastFCRefNbr
			public abstract class lastFCRefNbr : PX.Data.IBqlField
			{
			}
			protected String _LastFCRefNbr;
			[PXDBString(15, IsUnicode = true)]
			[PXDefault()]
			[PXUIField(DisplayName = "Last Fin. Charge", Enabled = false)]
			public virtual String LastFCRefNbr
			{
				get
				{
					return this._LastFCRefNbr;
				}
				set
				{
					this._LastFCRefNbr = value;
				}
			}
			#endregion
			#region FinChargeDate
			public abstract class finChargeDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _FinChargeDate;
			[PXDate()]
			[PXUIField(DisplayName = "Overdue Charge Date", Visibility = PXUIVisibility.Visible, Required = true)]
			public virtual DateTime? FinChargeDate
			{
				get
				{
					return this._FinChargeDate;
				}
				set
				{
					this._FinChargeDate = value;
				}
			}
			#endregion
			#region FinPeriodID
			public abstract class finPeriodID : PX.Data.IBqlField
			{
			}
			protected String _FinPeriodID;
			[FinPeriodID()]
			[PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.Visible)]
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


		}

		protected class ARDocKey : AP.Pair<string, string>
		{
			public ARDocKey(string aFirst, string aSecond) : base(aFirst, aSecond) { }
		}
		#endregion

		#region Ctor + public members
		public ARFinChargesApplyMaint()
		{
			ARSetup setup = ARSetup.Current;
			ARFinChargesApplyParameters filter = Filter.Current;
			ARFinChargeRecords.SetProcessDelegate(
				delegate(List<ARFinChargesDetails> list)
				{
					CreateFCDoc(list, filter);
				}
			);			
		}

		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}
		public PXAction<ARFinChargesApplyParameters> cancel;
		public PXAction<ARFinChargesApplyParameters> calculate;

		public PXFilter<ARFinChargesApplyParameters> Filter;
		[PXFilterable]
		public PXFilteredProcessing<ARFinChargesDetails, ARFinChargesApplyParameters> ARFinChargeRecords;
		public PXSetup<ARSetup> ARSetup;

		#region Sub-screen Navigation Buttons
		public PXAction<ARFinChargesApplyParameters> viewDocument;

        [PXUIField(DisplayName = Messages.ViewDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			ARFinChargesDetails row = this.ARFinChargeRecords.Current;
			if (row != null)
			{
				ARInvoice doc = PXSelect<ARInvoice>.Search<ARInvoice.docType, ARInvoice.refNbr>(this, row.DocType, row.RefNbr);
				if (doc != null)
				{
					ARInvoiceEntry graph = PXGraph.CreateInstance<ARInvoiceEntry>();
					graph.Document.Current = doc;
					throw new PXRedirectRequiredException(graph, true, "Document"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
				}
			}
			return Filter.Select();
		}

		public PXAction<ARFinChargesApplyParameters> viewLastFinCharge;

		[PXUIField(DisplayName = Messages.ViewLastCharge, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewLastFinCharge(PXAdapter adapter)
		{
			ARFinChargesDetails row = this.ARFinChargeRecords.Current;
			if (row != null)
			{
				if (!string.IsNullOrEmpty(row.LastFCRefNbr))
				{
					ARInvoice doc = PXSelect<ARInvoice>.Search<ARInvoice.docType, ARInvoice.refNbr>(this, row.LastFCDocType, row.LastFCRefNbr);
					if (doc != null)
					{
						ARInvoiceEntry graph = PXGraph.CreateInstance<ARInvoiceEntry>();
						graph.Document.Current = doc;
						throw new PXRedirectRequiredException(graph, true, "Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
					}
				}
			}
			return Filter.Select();
		}

		#endregion
		#endregion

		#region Delegates
		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
		[PXCancelButton]
		protected virtual IEnumerable Cancel(PXAdapter adapter)
		{
			ARFinChargeRecords.Cache.Clear();
			TimeStamp = null;
			PXLongOperation.ClearStatus(UID);
			return adapter.Get();
		}

		protected virtual IEnumerable aRFinChargeRecords()
		{
			return ARFinChargeRecords.Cache.Inserted;
		}

		[PXUIField(DisplayName = Messages.Calculate, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable Calculate(PXAdapter adapter)
		{
			ARFinChargesApplyParameters filter = Filter.Current as ARFinChargesApplyParameters;
			Company company = PXSelect<Company>.Select(this);
			ARSetup ARSetup = PXSelect<ARSetup>.Select(this);

			ARFinChargeRecords.Cache.Clear();

			PXSelectBase<Customer> cmd = new PXSelectJoin<Customer, 
						InnerJoin<Location, On<Location.bAccountID, Equal<Customer.bAccountID>, And<Location.locationID, Equal<Customer.defLocationID>>>, 
						InnerJoin<CustomerClass, On<CustomerClass.customerClassID, Equal<Customer.customerClassID>>,
						InnerJoin<ARStatementCycle, On<ARStatementCycle.statementCycleId, Equal<Customer.statementCycleId>>>>>,
						Where<Customer.finChargeApply, Equal<BQLConstants.BitOn>,
						And<Match<Current<AccessInfo.userName>>>>>(this);

			if (filter.CustomerID != null)
			{
				cmd.WhereAnd<Where<Customer.bAccountID, Equal<Current<ARFinChargesApplyParameters.customerID>>>>();
			}

			if (filter.CustomerClassID != null)
			{
				cmd.WhereAnd<Where<Customer.customerClassID, Equal<Current<ARFinChargesApplyParameters.customerClassID>>>>();
			}

			if (filter.StatementCycle != null)
			{
				cmd.WhereAnd<Where<Customer.statementCycleId, Equal<Current<ARFinChargesApplyParameters.statementCycle>>>>();
			}

			foreach (PXResult<Customer, Location, CustomerClass, ARStatementCycle> it in cmd.Select())
			{
				Customer cust = (Customer)it;
				Location defLocation = (Location)it;
				CustomerClass cClass = (CustomerClass)it;
				ARStatementCycle sCycle = (ARStatementCycle)it;

				ARFinCharge ARfc = null;
				if (ARSetup.DefFinChargeFromCycle ?? false)
				{
					if (!(sCycle.FinChargeApply ?? false))
						continue; //Fin. charge is not applicable
					ARfc = PXSelect<ARFinCharge, Where<ARFinCharge.finChargeID, Equal<Required<ARFinCharge.finChargeID>>>>.
									 Select(this, sCycle.FinChargeID);
				}
				else
				{
					if (!(cClass.FinChargeApply ?? false))
						continue; //Fin. charge is not applicable
					ARfc = PXSelect<ARFinCharge, Where<ARFinCharge.finChargeID, Equal<Required<ARFinCharge.finChargeID>>>>.
									 Select(this, cClass.FinChargeID);
				}

				if ((ARfc == null) || (ARfc.FinChargeID == null))
					continue;

				//bool hasUnreleased = CheckForUnreleasedCharges(this, cust.BAccountID.Value);
				bool hasOpenPayments = CheckForOpenPayments(this, cust.BAccountID);


				Dictionary<ARDocKey, ARInvoice> result = new Dictionary<ARDocKey, ARInvoice>();
				Dictionary<ARDocKey, ARInvoice> writeOffs = new Dictionary<ARDocKey, ARInvoice>();
				// then we take invoices and 
				foreach (PXResult<ARInvoice, ARAdjust> iDoc in PXSelectJoin<ARInvoice,
												InnerJoin<ARAdjust, 
													On<ARInvoice.docType, Equal<ARAdjust.adjdDocType>,
													And<ARInvoice.refNbr, Equal<ARAdjust.adjdRefNbr>>>>,
												Where<ARInvoice.dueDate, Less<Required<ARInvoice.dueDate>>,
												And<ARAdjust.adjgDocDate, GreaterEqual<Required<ARAdjust.adjgDocDate>>,
												And<ARInvoice.released, Equal<boolTrue>,
												And<ARInvoice.openDoc, Equal<boolFalse>>>>>>.Select(this, filter.FinChargeDate))
				{
					ARInvoice inv = iDoc;
					ARAdjust adj = iDoc;
					if ((inv.DocType == ARDocType.Invoice) || ((inv.DocType == ARDocType.DebitMemo) ||
						  (ARSetup.FinChargeOnCharge == true) && (inv.DocType == ARDocType.FinCharge))
						 )
					{

						ARDocKey docKey = new ARDocKey(inv.DocType, inv.RefNbr);
						if (inv.LastFinChargeDate >= filter.FinChargeDate) continue;
						if (writeOffs.ContainsKey(docKey))
							continue; //Ignore write offs
						ARInvoice doc = null;
						if (result.ContainsKey(docKey))
						{
							doc = result[docKey];
							if (adj.AdjgDocType == ARDocType.SmallBalanceWO)
							{
								writeOffs[docKey] = inv;
								result.Remove(docKey); // Closed Invoices, having write offs - are not subject for charging
								continue;
							}
						}
						else
						{
							doc = PXCache<ARInvoice>.CreateCopy(inv);
							doc.CuryDocBal = doc.CuryOrigDocAmt;
							doc.DocBal = doc.OrigDocAmt;
							result[docKey] = doc;
						}
						doc.DocBal -= adj.AdjAmt + adj.AdjDiscAmt;
						doc.CuryDocBal -= adj.CuryAdjdAmt + adj.CuryAdjdDiscAmt;
					}
				}

				foreach (ARInvoice inv in PXSelect<ARInvoice, Where<ARInvoice.released, Equal<boolTrue>,
															 And<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>,
															 And<ARInvoice.dueDate, Less<Required<ARInvoice.dueDate>>,
															 And<ARRegister.openDoc, Equal<boolTrue>>>>>>.
												 Select(this, cust.BAccountID, filter.FinChargeDate))
				{
					if ((inv.DocType == ARDocType.Invoice) || ((inv.DocType == ARDocType.DebitMemo) ||
						  (ARSetup.FinChargeOnCharge == true) && (inv.DocType == ARDocType.FinCharge))
						 )
					{
						if (inv.LastFinChargeDate >= filter.FinChargeDate) continue; 
						ARDocKey docKey = new ARDocKey(inv.DocType, inv.RefNbr);
						ARInvoice doc = result.ContainsKey(docKey) ? result[docKey] : PXCache<ARInvoice>.CreateCopy(inv);
						result[docKey] = doc;


						ARAdjust adj = null;
						adj = PXSelectGroupBy<ARAdjust, Where<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>,
															And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>,
															And<ARAdjust.adjgDocDate, Greater<Required<ARAdjust.adjgDocDate>>,
															And<ARAdjust.released, Equal<boolTrue>>>>>,
															Aggregate<Sum<ARAdjust.adjAmt, Sum<ARAdjust.curyAdjdAmt, Sum<ARAdjust.rGOLAmt>>>>>.
															Select(this, inv.DocType, inv.RefNbr, filter.FinChargeDate);
						if ((adj != null) && (adj.AdjgRefNbr != null))
						{
							doc.DocBal += (decimal)adj.AdjAmt;
							doc.CuryDocBal += (decimal)adj.CuryAdjdAmt;
						}
					}
				}

				foreach (ARInvoice inv in result.Values)
				{
					decimal docBalance = (decimal)inv.DocBal;
					decimal curdocBalance = (decimal)inv.CuryDocBal;

					CurrencyRate rate;
					CurrencyInfo invinfo = PXSelect<CurrencyInfo,
												Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.
											 Select(this, inv.CuryInfoID);

					if (invinfo.CuryID == invinfo.BaseCuryID || ARfc.BaseCurFlag == true)
					{
						rate = new CurrencyRate();
						rate.CuryRate = 1m;
						rate.CuryMultDiv = "M";
					}
					else if ((rate = PXSelect<CurrencyRate,
									Where<CurrencyRate.fromCuryID, Equal<Required<CurrencyRate.fromCuryID>>,
									And<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>,
									And<CurrencyRate.curyRateType, Equal<Required<CurrencyRate.curyRateType>>,
									And<CurrencyRate.curyEffDate, LessEqual<Required<CurrencyRate.curyEffDate>>>>>>,
									OrderBy<Desc<CurrencyRate.curyEffDate>>>.
									Select(this,
												 invinfo.CuryID,
												 invinfo.BaseCuryID,
												 cust.CuryRateTypeID,
												 filter.FinChargeDate)) == null)
					{
						throw new PXException(PX.Objects.CM.Messages.RateIsNotDefinedForThisDate, cust.CuryRateTypeID, invinfo.CuryID);
					}
					
					if (inv.CuryDocBal > 0m)
					{
						ARFinChargesDetails finCharge = new ARFinChargesDetails();
						Copy(finCharge, inv);
						finCharge.Selected = true;
						finCharge.FinChargeID = ARfc.FinChargeID;
						finCharge.TermsID = ARfc.TermsID;
						finCharge.ARAccountID = defLocation.ARAccountID;
						finCharge.ARSubID = defLocation.ARSubID;

						TimeSpan diff = (TimeSpan)(filter.FinChargeDate - (inv.LastFinChargeDate ?? inv.DueDate));
						finCharge.OverdueDays = (short?)(diff.Days > 0 ? diff.Days : 0);

						if (ARfc.BaseCurFlag == true)
						{
							finCharge.FinChargeCuryID = invinfo.BaseCuryID;
							finCharge.CuryRate = 1m;
							finCharge.CuryMultDiv = "M";
							finCharge.CuryRateTypeID = cust.CuryRateTypeID;

							CalcFCAmount(finCharge, ARfc, (decimal)inv.DocBal);
							//Rounding to the correct precision - for display 
							finCharge.FinChargeAmt = PXCurrencyAttribute.Round(this.Caches[typeof(ARInvoice)], inv, (decimal)finCharge.FinChargeAmt, CMPrecision.BASECURY); 
						}
						else
						{
							finCharge.FinChargeCuryID = invinfo.CuryID;
							finCharge.CuryRate = rate.CuryRate;
							finCharge.CuryMultDiv = rate.CuryMultDiv;
							finCharge.CuryRateTypeID = cust.CuryRateTypeID;

							CalcFCAmount(finCharge, ARfc, (decimal)inv.CuryDocBal);
							//Rounding to the correct precision - for display 
							finCharge.FinChargeAmt = PXCurrencyAttribute.Round(this.Caches[typeof(ARInvoice)], inv, (decimal)finCharge.FinChargeAmt, CMPrecision.TRANCURY); 
						}

						if (finCharge.FinChargeAmt <= Decimal.Zero)
							continue; 

						finCharge.LastFCRefNbr = FindUnreleasedChargeForDoc(this, inv.DocType, inv.RefNbr);
						finCharge.LastFCDocType = ARDocType.FinCharge;
						finCharge.FinChargeDate = filter.FinChargeDate;
						finCharge.FinPeriodID = filter.FinPeriodID;

						bool hasUnreleased = !string.IsNullOrEmpty(finCharge.LastFCRefNbr);
						finCharge.HasUnreleasedCharges = hasUnreleased;
						finCharge.Selected = !(hasUnreleased || hasOpenPayments);
						finCharge = this.ARFinChargeRecords.Insert(finCharge);
						if (hasUnreleased)
						{
							this.ARFinChargeRecords.Cache.RaiseExceptionHandling<ARFinChargesDetails.refNbr>(finCharge, finCharge.RefNbr, new PXSetPropertyException(Messages.ERR_UnreleasedFinChargesForDocument, PXErrorLevel.RowError));
						}
						else
						{
							if (hasOpenPayments)
							{
								this.ARFinChargeRecords.Cache.RaiseExceptionHandling<ARFinChargesDetails.refNbr>(finCharge, finCharge.RefNbr, new PXSetPropertyException(Messages.WRN_FinChargeCustomerHasOpenPayments, PXErrorLevel.RowInfo));
							}
						}
					}
				}
			}

			return adapter.Get();
		}
		#endregion

		#region Processing Functions
		private static void CreateFCDoc(List<ARFinChargesDetails> list, ARFinChargesApplyParameters filter)
		{
			ARInvoiceEntry ie = PXGraph.CreateInstance<ARInvoiceEntry>();
			ie.ARSetup.Current.RequireControlTotal = false;
			Customer cust = null;
			ARFinCharge arFC = null;
			bool failed = false;
			list.Sort(Compare);
			//CurrencyInfo new_info = null;
			Dictionary<string, DateTime> updatedCycles = new Dictionary<string, DateTime>();
			for (int i = 0; i < list.Count; i++)
			{
				ARFinChargesDetails fcDetail = list[i];
				if (!fcDetail.FinChargeDate.HasValue || string.IsNullOrEmpty(fcDetail.FinPeriodID))
				{
                    throw new PXException(Messages.OverdueChargeDateAndFinPeriodAreRequired); //Stop processing other docs - they all have the same date
				}
				try
				{
					if (fcDetail.HasUnreleasedCharges ?? false)
					{
						throw new PXException(Messages.ERR_UnreleasedFinChargesForDocument);
					}


					if (arFC == null || arFC.FinChargeID != fcDetail.FinChargeID)
						arFC = PXSelect<ARFinCharge, Where<ARFinCharge.finChargeID, Equal<Required<ARFinCharge.finChargeID>>>>.
															   Select(ie, fcDetail.FinChargeID);
					if (ie.Document.Current == null)
					{
						if (cust == null || cust.BAccountID != fcDetail.CustomerID)
						{
							cust = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(ie, fcDetail.CustomerID);
						}
						//CurrencyInfo curinfo = new CurrencyInfo();
						//curinfo.CuryEffDate = filter.FinChargeDate;
						//curinfo.CuryID = fcDetail.FinChargeCuryID;
						//curinfo.CuryRateTypeID = fcDetail.CuryRateTypeID;
						//curinfo.CuryRate = fcDetail.CuryRate;
						//curinfo.CuryMultDiv = fcDetail.CuryMultDiv;
						//curinfo = ie.currencyinfo.Insert(curinfo);
						//new_info = PXCache<CurrencyInfo>.CreateCopy(curinfo);

						ARInvoice inv = new ARInvoice();
						inv.DocType = ARDocType.FinCharge;

						inv = PXCache<ARInvoice>.CreateCopy(ie.Document.Insert(inv));
						inv.ARAccountID = fcDetail.ARAccountID;
						inv.ARSubID = fcDetail.ARSubID;
						inv.CustomerID = fcDetail.CustomerID;
						inv = ie.Document.Update(inv);
						inv.CuryID = fcDetail.FinChargeCuryID;
						inv.DocDate = filter.FinChargeDate;
						inv.DocDesc = Messages.FinCharge;
						//inv.Hold = true;
						inv.FinPeriodID = filter.FinPeriodID;
						inv.TermsID = arFC.TermsID;
						inv = ie.Document.Update(inv);
					}
					ARTran det = new ARTran();

					det.AccountID = arFC.FinChargeAccountID;
					det.SubID = arFC.FinChargeSubID;

					det.TranType = ARDocType.FinCharge;
					det.TranDate = filter.FinChargeDate;
					det.CuryTranAmt = fcDetail.FinChargeAmt;
					det.CuryUnitPrice = fcDetail.FinChargeAmt;
					det.TranDesc = String.Format(Messages.FinChargeDocumentFormat, fcDetail.DocType, fcDetail.RefNbr);
					det.LineType = "";
					det.FinPeriodID = filter.FinPeriodID;
					det.Qty = (decimal)1.0;
					det.Released = false;
					det.DrCr = "C";
					det.TranClass = "";
					det.TaxCategoryID = arFC.TaxCategoryID;
					det.Commissionable = false;
					det = ie.Transactions.Insert(det);
					if (string.IsNullOrEmpty(arFC.TaxCategoryID))
					{
						//In this case special update is required, to prevent normal document's default
						det = PXCache<ARTran>.CreateCopy(det);
						det.TaxCategoryID = arFC.TaxCategoryID;
						det = ie.Transactions.Update(det);
					}

					ARFinChargeTran fcTran = new ARFinChargeTran();
					fcTran.TranType = det.TranType;
					fcTran.RefNbr = det.RefNbr;
					fcTran.LineNbr = det.LineNbr;
					fcTran.OrigDocType = fcDetail.DocType;
					fcTran.OrigRefNbr = fcDetail.RefNbr;
					fcTran.FinChargeID = arFC.FinChargeID;
					fcTran = ie.finChargeTrans.Insert(fcTran);

					if ((i == list.Count - 1) || (Compare(fcDetail, list[i + 1]) != 0))
					{
						//CurrencyInfo info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<ARInvoice.curyInfoID>>>>.Select(ie);
						//new_info.CuryInfoID = info.CuryInfoID;
						//ie.currencyinfo.Cache.Update(new_info); //Update currency
						ARInvoice inv = ie.Document.Current;
						inv.CuryOrigDocAmt = inv.CuryDocBal;
						ie.Save.Press();
						if (!updatedCycles.ContainsKey(cust.StatementCycleId)) 
						{
							updatedCycles[cust.StatementCycleId] = fcDetail.FinChargeDate.Value;
						}				
						ie.Clear();
				
					}
				}
				catch (Exception e)
				{
					ie.Clear();
					PXProcessing<ARFinChargesDetails>.SetError(i, e);
					failed = true;
				}
			}
			ARStatementMaint cycleMaintGraph = PXGraph.CreateInstance<ARStatementMaint>();
			foreach (KeyValuePair<string, DateTime> iCycle in updatedCycles) 
			{
				ARStatementCycle row = cycleMaintGraph.ARStatementCycleRecord.Search<ARStatementCycle.statementCycleId>(iCycle.Key);
				cycleMaintGraph.ARStatementCycleRecord.Current = row;
				if(!(row.LastFinChrgDate.HasValue && row.LastFinChrgDate >= iCycle.Value))
					row.LastFinChrgDate = iCycle.Value;
				row = cycleMaintGraph.ARStatementCycleRecord.Update(row);
				cycleMaintGraph.Save.Press();
			}
			if (failed)
			{
				throw new PXException(ErrorMessages.SeveralItemsFailed);
			}
		}

		private static bool CheckForUnreleasedCharges(PXGraph aGraph, int aCustomerID)
		{
			ARRegister doc = PXSelect<ARRegister,
							 Where<ARRegister.docType, Equal<ARDocType.finCharge>,
							 And<ARRegister.released, Equal<BQLConstants.BitOff>,
							 And<ARRegister.customerID, Equal<Required<ARRegister.customerID>>>>>>.SelectWindowed(aGraph, 0, 1, aCustomerID);
			return (doc != null);
		}

		private static string FindUnreleasedChargeForDoc(PXGraph aGraph, string aDocType, string aRefNbr)
		{
			ARInvoice doc = (ARInvoice)PXSelectJoin<ARInvoice, InnerJoin<ARFinChargeTran,
							 On<ARInvoice.docType, Equal<ARFinChargeTran.tranType>,
								And<ARInvoice.refNbr, Equal<ARFinChargeTran.refNbr>>>>,
							 Where<ARFinChargeTran.origDocType, Equal<Required<ARFinChargeTran.origDocType>>,
								And<ARFinChargeTran.origRefNbr, Equal<Required<ARFinChargeTran.origRefNbr>>,
								And<ARInvoice.released, Equal<boolFalse>>>>>.SelectWindowed(aGraph, 0, 1, aDocType, aRefNbr);
			if (doc != null && !string.IsNullOrEmpty(doc.RefNbr))
				return doc.RefNbr;
			return null;
		}

		private static bool CheckForOpenPayments(PXGraph aGraph, int? aCustomerID)
		{
			ARRegister doc = PXSelect<ARPayment, Where<ARPayment.customerID, Equal<Required<ARPayment.customerID>>,
				And<ARPayment.openDoc, Equal<boolTrue>>>>.SelectWindowed(aGraph, 0, 1, aCustomerID);
			return (doc != null);
		}

		static void CalcFCAmount(ARFinChargesDetails aDest, ARFinCharge aDef, decimal aDocBalance )
		{
			decimal amtFC = Decimal.Zero;
			decimal sampleCuryRate = (decimal)aDest.SampleCuryRate;
			if ((bool)aDef.PercentFlag == true)
			{
				amtFC = (decimal)(aDocBalance * aDest.OverdueDays * aDef.FinChargePercent / (365 * 100));
			}

			//Fixed values in ARFinCharge definition are defined in BaseCurrency - so they need to be converted to currency of current Fin. charge.
			if ((amtFC - (aDef.MinFinChargeAmount * sampleCuryRate)) < Decimal.Zero)
			{
				if ((bool)aDef.MinFinChargeFlag == true)
				{
					amtFC = (decimal)aDef.MinFinChargeAmount;
				}
				else
				{
					amtFC = Decimal.Zero;
				}
			}
			aDest.FinChargeAmt = amtFC;
		} 
		#endregion

		#region Events
		protected virtual void ARFinChargesApplyParameters_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			ARFinChargesApplyParameters row = (ARFinChargesApplyParameters)e.Row;
			bool defaultFromCycle = this.ARSetup.Current.DefFinChargeFromCycle ?? false;
			PXUIFieldAttribute.SetRequired<ARFinChargesApplyParameters.finPeriodID>(cache, true);
			PXUIFieldAttribute.SetRequired<ARFinChargesApplyParameters.statementCycle>(cache, defaultFromCycle);
			PXDefaultAttribute.SetPersistingCheck<ARFinChargesApplyParameters.statementCycle>(cache, row, defaultFromCycle ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXUIFieldAttribute.SetVisible<ARFinChargesApplyParameters.customerClassID>(cache, e.Row, !defaultFromCycle);
			PXUIFieldAttribute.SetVisible<ARFinChargesApplyParameters.customerID>(cache, e.Row, !defaultFromCycle);
			bool hasDocs = (this.ARFinChargeRecords.Select().Count > 0);
			this.ARFinChargeRecords.SetProcessAllEnabled(hasDocs);
			this.ARFinChargeRecords.SetProcessEnabled(hasDocs);
			
		}

		protected virtual void ARFinChargesApplyParameters_StatementCycle_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			ARFinChargesApplyParameters row = (ARFinChargesApplyParameters)e.Row;
			bool defaultFromCycle = this.ARSetup.Current.DefFinChargeFromCycle ?? false;
			if (defaultFromCycle)
			{
				ARStatementCycle cycleDef = PXSelect<ARStatementCycle>.Search<ARStatementCycle.statementCycleId>(this, row.StatementCycle);
				if (cycleDef != null)
				{
					row.FinChargeDate = ARStatementProcess.FindNextStatementDate(this.Accessinfo.BusinessDate.Value, cycleDef);
					cache.SetDefaultExt<ARFinChargesApplyParameters.finPeriodID>(e.Row);
				}
			}

		}

		protected virtual void ARFinChargesApplyParameters_FinChargeDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			cache.SetDefaultExt<ARFinChargesApplyParameters.finPeriodID>(e.Row);
		}
		#endregion

		#region Utility
		static int Compare(ARFinChargesDetails aT0, ARFinChargesDetails aT1)
		{
			if (aT0.CustomerID != aT1.CustomerID) return aT0.CustomerID.Value.CompareTo(aT1.CustomerID.Value);
			if (aT0.ARAccountID != aT1.ARAccountID) return aT0.ARAccountID.Value.CompareTo(aT1.ARAccountID.Value);
			if (aT0.ARSubID != aT1.ARSubID) return aT0.ARSubID.Value.CompareTo(aT1.ARSubID.Value);
			return String.Compare(aT0.FinChargeCuryID, aT1.FinChargeCuryID);
		}

		static void Copy(ARFinChargesDetails aDest, ARInvoice aSrc) 
		{
			aDest.CustomerID = aSrc.CustomerID;
			aDest.DocType = aSrc.DocType;
			aDest.RefNbr = aSrc.RefNbr;
			aDest.DocDate = aSrc.DocDate;
			aDest.CuryID = aSrc.CuryID;
			aDest.CuryOrigDocAmt = aSrc.CuryOrigDocAmt;
			aDest.CuryDocBal = aSrc.CuryDocBal;			
		}

		
		#endregion
	}
}

