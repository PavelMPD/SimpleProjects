using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.TX;
using PX.Objects.CR;
using PX.Objects.CA;
using PX.TM;
using PX.SM;
using PX.Objects.EP;
using SOInvoice = PX.Objects.SO.SOInvoice;
using SOInvoiceEntry = PX.Objects.SO.SOInvoiceEntry;

namespace PX.Objects.AR.Standalone
{
	[PXCacheName(Messages.ARCashSale)]
	[PXProjection(typeof(Select2<ARRegister, InnerJoin<ARInvoice, On<ARInvoice.docType, Equal<ARRegister.docType>, And<ARInvoice.refNbr, Equal<ARRegister.refNbr>>>, InnerJoin<ARPayment, On<ARPayment.docType, Equal<ARRegister.docType>, And<ARPayment.refNbr, Equal<ARRegister.refNbr>>>>>, Where<ARRegister.docType, Equal<ARDocType.cashSale>, Or<ARRegister.docType, Equal<ARDocType.cashReturn>>>>), Persistent = true)]
	[Serializable()]
	[PXSubstitute(GraphType = typeof(ARCashSaleEntry))]
	[PXPrimaryGraph(typeof(ARCashSaleEntry))]
	public partial class ARCashSale : ARRegister, ICCPayment
	{
		//ARRegister
		#region DocType
		public new abstract class docType : PX.Data.IBqlField
		{
		}
		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(ARRegister.docType))]
		[PXDefault()]
		[ARCashSaleType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true)]
		[PXFieldDescription]
		public override String DocType
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
		public new abstract class refNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC", IsUnicode = true, BqlField = typeof(ARRegister.refNbr))]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[ARCashSaleType.RefNbr(typeof(Search2<ARCashSale.refNbr,
			LeftJoin<SOInvoice, On<SOInvoice.docType, Equal<ARCashSale.docType>, And<SOInvoice.refNbr, Equal<ARCashSale.refNbr>>>,
			InnerJoin<Customer, On<ARCashSale.customerID, Equal<Customer.bAccountID>>>>,
			Where<ARCashSale.docType, Equal<Current<ARCashSale.docType>>,
			And2<Where<SOInvoice.refNbr, IsNull, Or<ARCashSale.released, Equal<boolTrue>>>,
			And<Match<Customer, Current<AccessInfo.userName>>>>>, OrderBy<Desc<ARCashSale.refNbr>>>), Filterable = true)]
		[ARCashSaleType.Numbering()]
		[PXFieldDescription]
		public override String RefNbr
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
		#region CustomerID
		public new abstract class customerID : PX.Data.IBqlField
		{
		}
		[CustomerActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), Filterable = true, BqlField = typeof(ARRegister.customerID))]
		[PXDefault()]
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
		#region CustomerLocationID
		public new abstract class customerLocationID : PX.Data.IBqlField
		{
		}
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<ARCashSale.customerID>>,
			And<Location.isActive, Equal<True>,
			And<MatchWithBranch<Location.cBranchID>>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, BqlField = typeof(ARRegister.customerLocationID))]
		[PXDefault(typeof(Coalesce<Search2<BAccountR.defLocationID,
			InnerJoin<Location, On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>>,
			Where<BAccountR.bAccountID, Equal<Current<ARCashSale.customerID>>,
				And<Location.isActive, Equal<True>,
				And<MatchWithBranch<Location.cBranchID>>>>>,
			Search<Location.locationID,
			Where<Location.bAccountID, Equal<Current<ARCashSale.customerID>>,
			And<Location.isActive, Equal<True>, And<MatchWithBranch<Location.cBranchID>>>>>>))]
		public override Int32? CustomerLocationID
		{
			get
			{
				return this._CustomerLocationID;
			}
			set
			{
				this._CustomerLocationID = value;
			}
		}
		#endregion
		#region ARAccountID
		public new abstract class aRAccountID : PX.Data.IBqlField
		{
		}
		[PXDefault]
        [Account(typeof(ARCashSale.branchID), typeof(Search<Account.accountID,
                    Where2<Match<Current<AccessInfo.userName>>,
                         And<Account.active, Equal<True>,
                         And<Account.isCashAccount, Equal<False>,
                         And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
                          Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>>), DisplayName = "AR Account", BqlField = typeof(ARRegister.aRAccountID))]
        public override Int32? ARAccountID
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
		public new abstract class aRSubID : PX.Data.IBqlField
		{
		}
		[PXDefault]
		[SubAccount(typeof(ARCashSale.aRAccountID), DisplayName = "AR Sub.", Visibility = PXUIVisibility.Visible, BqlField = typeof(ARRegister.aRSubID))]
		public override Int32? ARSubID
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
		#region TermsID
		public abstract class termsID : PX.Data.IBqlField
		{
		}
		protected String _TermsID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(ARInvoice.termsID))]
		[PXDefault(typeof(Search<Customer.termsID, Where<Customer.bAccountID, Equal<Current<ARCashSale.customerID>>>>))]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, Or<Terms.visibleTo, Equal<TermsVisibleTo.customer>>>>), DescriptionField = typeof(Terms.descr), Filterable = true)]
		[Terms(typeof(ARCashSale.docDate), null, null, typeof(ARCashSale.curyDocBal), typeof(ARCashSale.curyOrigDiscAmt))]
		public virtual String TermsID
		{
			get
			{
				return this._TermsID;
			}
			set
			{
				this._TermsID = value;
			}
		}
		#endregion
		#region LineCntr
		public new abstract class lineCntr : PX.Data.IBqlField
		{
		}
		[PXDBInt(BqlField = typeof(ARRegister.lineCntr))]
		[PXDefault(0)]
		public override Int32? LineCntr
		{
			get
			{
				return this._LineCntr;
			}
			set
			{
				this._LineCntr = value;
			}
		}
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		[PXDBLong(BqlField = typeof(ARRegister.curyInfoID))]
		[CurrencyInfo(ModuleCode = "AR")]
		public override Int64? CuryInfoID
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
		#region CuryOrigDocAmt
		public new abstract class curyOrigDocAmt : PX.Data.IBqlField
		{
		}
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARCashSale.curyInfoID), typeof(ARCashSale.origDocAmt), BqlField = typeof(ARRegister.curyOrigDocAmt))]
		[PXUIField(DisplayName = "Payment Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public override Decimal? CuryOrigDocAmt
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
		#region OrigDocAmt
		public new abstract class origDocAmt : PX.Data.IBqlField
		{
		}
		[PXDBBaseCury(BqlField = typeof(ARRegister.origDocAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? OrigDocAmt
		{
			get
			{
				return this._OrigDocAmt;
			}
			set
			{
				this._OrigDocAmt = value;
			}
		}
		#endregion
		#region CuryDocBal
		public new abstract class curyDocBal : PX.Data.IBqlField
		{
		}
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARCashSale.curyInfoID), typeof(ARCashSale.docBal), BaseCalc = false, BqlField = typeof(ARRegister.curyDocBal))]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public override Decimal? CuryDocBal
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
		#region DocBal
		public new abstract class docBal : PX.Data.IBqlField
		{
		}
		[PXDBBaseCury(BqlField = typeof(ARRegister.docBal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? DocBal
		{
			get
			{
				return this._DocBal;
			}
			set
			{
				this._DocBal = value;
			}
		}
		#endregion
		#region CuryOrigDiscAmt
		public new abstract class curyOrigDiscAmt : PX.Data.IBqlField
		{
		}
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARCashSale.curyInfoID), typeof(ARCashSale.origDiscAmt), BqlField = typeof(ARRegister.curyOrigDiscAmt))]
		[PXUIField(DisplayName = "Cash Discount Taken", Visibility = PXUIVisibility.SelectorVisible)]
		public override Decimal? CuryOrigDiscAmt
		{
			get
			{
				return this._CuryOrigDiscAmt;
			}
			set
			{
				this._CuryOrigDiscAmt = value;
			}
		}
		#endregion
		#region OrigDiscAmt
		public new abstract class origDiscAmt : PX.Data.IBqlField
		{
		}
		[PXDBBaseCury(BqlField = typeof(ARRegister.origDiscAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? OrigDiscAmt
		{
			get
			{
				return this._OrigDiscAmt;
			}
			set
			{
				this._OrigDiscAmt = value;
			}
		}
		#endregion
		#region CuryDiscTaken
		public new abstract class curyDiscTaken : PX.Data.IBqlField
		{
		}
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARCashSale.curyInfoID), typeof(ARCashSale.discTaken), BqlField = typeof(ARRegister.curyDiscTaken))]
		public override Decimal? CuryDiscTaken
		{
			get
			{
				return this._CuryDiscTaken;
			}
			set
			{
				this._CuryDiscTaken = value;
			}
		}
		#endregion
		#region DiscTaken
		public new abstract class discTaken : PX.Data.IBqlField
		{
		}
		[PXDBBaseCury(BqlField = typeof(ARRegister.discTaken))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? DiscTaken
		{
			get
			{
				return this._DiscTaken;
			}
			set
			{
				this._DiscTaken = value;
			}
		}
		#endregion
		#region CuryDiscBal
		public new abstract class curyDiscBal : PX.Data.IBqlField
		{
		}
		[PXUIField(DisplayName = "Cash Discount Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDBCurrency(typeof(ARCashSale.curyInfoID), typeof(ARCashSale.discBal), BaseCalc = false, BqlField = typeof(ARRegister.curyDiscBal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? CuryDiscBal
		{
			get
			{
				return this._CuryDiscBal;
			}
			set
			{
				this._CuryDiscBal = value;
			}
		}
		#endregion
		#region DiscBal
		public new abstract class discBal : PX.Data.IBqlField
		{
		}
		[PXDBBaseCury(BqlField = typeof(ARRegister.discBal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? DiscBal
		{
			get
			{
				return this._DiscBal;
			}
			set
			{
				this._DiscBal = value;
			}
		}
		#endregion
		#region DocDesc
		public new abstract class docDesc : PX.Data.IBqlField
		{
		}
		[PXDBString(150, IsUnicode = true, BqlField = typeof(ARRegister.docDesc))]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public override String DocDesc
		{
			get
			{
				return this._DocDesc;
			}
			set
			{
				this._DocDesc = value;
			}
		}
		#endregion
		#region CreatedByID
		public new abstract class createdByID : PX.Data.IBqlField
		{
		}
		[PXDBCreatedByID(BqlField = typeof(ARRegister.createdByID))]
		public override Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public new abstract class createdByScreenID : PX.Data.IBqlField
		{
		}
		[PXDBCreatedByScreenID(BqlField = typeof(ARRegister.createdByScreenID))]
		public override String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public new abstract class createdDateTime : PX.Data.IBqlField
		{
		}
		[PXDBCreatedDateTime(BqlField = typeof(ARRegister.createdDateTime))]
		public override DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public new abstract class lastModifiedByID : PX.Data.IBqlField
		{
		}
		[PXDBLastModifiedByID(BqlField = typeof(ARRegister.lastModifiedByID))]
		public override Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public new abstract class lastModifiedByScreenID : PX.Data.IBqlField
		{
		}
		[PXDBLastModifiedByScreenID(BqlField = typeof(ARRegister.lastModifiedByScreenID))]
		public override String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public new abstract class lastModifiedDateTime : PX.Data.IBqlField
		{
		}
		[PXDBLastModifiedDateTime(BqlField = typeof(ARRegister.lastModifiedDateTime))]
		public override DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#region tstamp
		public new abstract class Tstamp : PX.Data.IBqlField
		{
		}
		[PXDBTimestamp(BqlField = typeof(ARRegister.Tstamp))]
		public override Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region BatchNbr
		public new abstract class batchNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true, BqlField = typeof(ARRegister.batchNbr))]
		[PXUIField(DisplayName = "Batch Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXSelector(typeof(Batch.batchNbr))]
		public override String BatchNbr
		{
			get
			{
				return this._BatchNbr;
			}
			set
			{
				this._BatchNbr = value;
			}
		}
		#endregion
		#region BatchSeq
		public new abstract class batchSeq : PX.Data.IBqlField
		{
		}
		[PXDBShort(BqlField = typeof(ARRegister.batchSeq))]
		[PXDefault((short)0)]
		public override Int16? BatchSeq
		{
			get
			{
				return this._BatchSeq;
			}
			set
			{
				this._BatchSeq = value;
			}
		}
		#endregion
		#region Status
		public new abstract class status : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true, BqlField = typeof(ARRegister.status))]
		[PXDefault(ARDocStatus.Hold)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[ARDocStatus.List()]
		public override String Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				//this._Status = value;
			}
		}
		#endregion
		#region Released
		public new abstract class released : PX.Data.IBqlField
		{
		}
		[PXDBBool(BqlField = typeof(ARRegister.released))]
		[PXDefault(false)]
		public override Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
				this.SetStatus();
			}
		}
		#endregion
		#region OpenDoc
		public new abstract class openDoc : PX.Data.IBqlField
		{
		}
		[PXDBBool(BqlField = typeof(ARRegister.openDoc))]
		[PXDefault(true)]
		public override Boolean? OpenDoc
		{
			get
			{
				return this._OpenDoc;
			}
			set
			{
				this._OpenDoc = value;
				this.SetStatus();
			}
		}
		#endregion
		#region Hold
		public new abstract class hold : PX.Data.IBqlField
		{
		}
		[PXDBBool(BqlField = typeof(ARRegister.hold))]
		[PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.Visible)]
		[PXDefault(true, typeof(Search<ARSetup.holdEntry>))]
		public override Boolean? Hold
		{
			get
			{
				return this._Hold;
			}
			set
			{
				this._Hold = value;
				this.SetStatus();
			}
		}
		#endregion
		#region Scheduled
		public new abstract class scheduled : PX.Data.IBqlField
		{
		}
		[PXDBBool(BqlField = typeof(ARRegister.scheduled))]
		[PXDefault(false)]
		public override Boolean? Scheduled
		{
			get
			{
				return this._Scheduled;
			}
			set
			{
				this._Scheduled = value;
				this.SetStatus();
			}
		}
		#endregion
		#region Voided
		public new abstract class voided : PX.Data.IBqlField
		{
		}
		[PXDBBool(BqlField = typeof(ARRegister.voided))]
		[PXDefault(false)]
		public override Boolean? Voided
		{
			get
			{
				return this._Voided;
			}
			set
			{
				this._Voided = value;
				this.SetStatus();
			}
		}
		#endregion
		#region NoteID
		public new abstract class noteID : PX.Data.IBqlField
		{
		}
		[PXNote(BqlField = typeof(ARRegister.noteID), DescriptionField = typeof(ARCashSale.refNbr))]
		public override Int64? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region RefNoteID
		public abstract class refNoteID : PX.Data.IBqlField
		{
		}
		protected Int64? _RefNoteID;
		[PXDBLong(BqlField = typeof(ARInvoice.refNoteID))]
		public virtual Int64? RefNoteID
		{
			get
			{
				return this._RefNoteID;
			}
			set
			{
				this._RefNoteID = value;
			}
		}
		#endregion
		#region ClosedFinPeriodID
		public new abstract class closedFinPeriodID : PX.Data.IBqlField
		{
		}
		[FinPeriodID(BqlField = typeof(ARRegister.closedFinPeriodID))]
		[PXUIField(DisplayName = "Closed Period", Visibility = PXUIVisibility.Invisible)]
		public override String ClosedFinPeriodID
		{
			get
			{
				return this._ClosedFinPeriodID;
			}
			set
			{
				this._ClosedFinPeriodID = value;
			}
		}
		#endregion
		#region ClosedTranPeriodID
		public new abstract class closedTranPeriodID : PX.Data.IBqlField
		{
		}
		[FinPeriodID(BqlField = typeof(ARRegister.closedTranPeriodID))]
		[PXUIField(DisplayName = "Closed Period", Visibility = PXUIVisibility.Invisible)]
		public override String ClosedTranPeriodID
		{
			get
			{
				return this._ClosedTranPeriodID;
			}
			set
			{
				this._ClosedTranPeriodID = value;
			}
		}
		#endregion
		#region RGOLAmt
		public new abstract class rGOLAmt : PX.Data.IBqlField
		{
		}
		[PXDBBaseCury(BqlField = typeof(ARRegister.rGOLAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? RGOLAmt
		{
			get
			{
				return this._RGOLAmt;
			}
			set
			{
				this._RGOLAmt = value;
			}
		}
		#endregion
		#region ScheduleID
		public new abstract class scheduleID : IBqlField
		{
		}
		[PXDBString(10, IsUnicode = true, BqlField = typeof(ARRegister.scheduleID))]
		public override string ScheduleID
		{
			get
			{
				return this._ScheduleID;
			}
			set
			{
				this._ScheduleID = value;
			}
		}
		#endregion
		#region ImpRefNbr
		public new abstract class impRefNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true, BqlField = typeof(ARRegister.impRefNbr))]
		public override String ImpRefNbr
		{
			get
			{
				return this._ImpRefNbr;
			}
			set
			{
				this._ImpRefNbr = value;
			}
		}
		#endregion
		#region StatementDate
		public new abstract class statementDate : PX.Data.IBqlField
		{
		}
		[PXDBDate(BqlField = typeof(ARRegister.statementDate))]
		public override DateTime? StatementDate
		{
			get
			{
				return this._StatementDate;
			}
			set
			{
				this._StatementDate = value;
			}
		}
		#endregion
		#region SalesPersonID
		public new abstract class salesPersonID : PX.Data.IBqlField
		{
		}
		[SalesPerson(BqlField = typeof(ARRegister.salesPersonID))]
		[PXDefault(typeof(Search<CustDefSalesPeople.salesPersonID, Where<CustDefSalesPeople.bAccountID, Equal<Current<ARRegister.customerID>>, And<CustDefSalesPeople.locationID, Equal<Current<ARRegister.customerLocationID>>, And<CustDefSalesPeople.isDefault, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public override Int32? SalesPersonID
		{
			get
			{
				return this._SalesPersonID;
			}
			set
			{
				this._SalesPersonID = value;
			}
		}
		#endregion
		#region OrigDocType
		public new abstract class origDocType : PX.Data.IBqlField
		{
		}
		#endregion
		#region OrigRefNbr
		public new abstract class origRefNbr : PX.Data.IBqlField
		{
		}
		#endregion
		//ARInvoice
		#region ARInvoiceDocType
		public abstract class aRInvoiceDocType : PX.Data.IBqlField
		{
		}
		[PXDBString(3, IsFixed = true, BqlField = typeof(ARInvoice.docType))]
		[PXDefault()]
		[PXRestriction()]
		public virtual String ARInvoiceDocType
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
		#region ARInvoiceRefNbr
		public abstract class aRInvoiceRefNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true, InputMask = "", BqlField = typeof(ARInvoice.refNbr))]
		[PXDefault()]
		[PXRestriction()]
		public virtual String ARInvoiceRefNbr
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
		#region BillAddressID
		public abstract class billAddressID : PX.Data.IBqlField
		{
		}
		protected Int32? _BillAddressID;
		[PXDBInt(BqlField = typeof(ARInvoice.billAddressID))]
		[ARAddress(typeof(Select2<Customer,
			InnerJoin<Location, On<Location.bAccountID, Equal<Customer.bAccountID>, And<Location.locationID, Equal<Customer.defLocationID>>>,
			InnerJoin<Address, On<Address.bAccountID, Equal<Customer.bAccountID>, And<Address.addressID, Equal<Customer.defBillAddressID>>>,
			LeftJoin<ARAddress, On<ARAddress.customerID, Equal<Address.bAccountID>, And<ARAddress.customerAddressID, Equal<Address.addressID>, And<ARAddress.revisionID, Equal<Address.revisionID>, And<ARAddress.isDefaultBillAddress, Equal<boolTrue>>>>>>>>,
			Where<Customer.bAccountID, Equal<Current<ARCashSale.customerID>>>>))]
		public virtual Int32? BillAddressID
		{
			get
			{
				return this._BillAddressID;
			}
			set
			{
				this._BillAddressID = value;
			}
		}
		#endregion 
		#region BillContactID
		public abstract class billContactID : PX.Data.IBqlField
		{
		}
		protected Int32? _BillContactID;
		[PXDBInt(BqlField = typeof(ARInvoice.billContactID))]
		[ARContact(typeof(Select2<Customer,
			InnerJoin<Location, On<Location.bAccountID, Equal<Customer.bAccountID>, And<Location.locationID, Equal<Customer.defLocationID>>>,
			InnerJoin<Contact, On<Contact.bAccountID, Equal<Customer.bAccountID>, And<Contact.contactID, Equal<Customer.defBillContactID>>>,
			LeftJoin<ARContact, On<ARContact.customerID, Equal<Contact.bAccountID>, And<ARContact.customerContactID, Equal<Contact.contactID>, And<ARContact.revisionID, Equal<Contact.revisionID>, And<ARContact.isDefaultContact, Equal<boolTrue>>>>>>>>,
			Where<Customer.bAccountID, Equal<Current<ARCashSale.customerID>>>>))]
		public virtual Int32? BillContactID
		{
			get
			{
				return this._BillContactID;
			}
			set
			{
				this._BillContactID = value;
			}
		}
		#endregion
		#region InvoiceNbr
		public abstract class invoiceNbr : PX.Data.IBqlField
		{
		}
		protected String _InvoiceNbr;
		[PXDBString(40, IsUnicode = true, BqlField = typeof(ARInvoice.invoiceNbr))]
		public virtual String InvoiceNbr
		{
			get
			{
				return this._InvoiceNbr;
			}
			set
			{
				this._InvoiceNbr = value;
			}
		}
		#endregion
		#region InvoiceDate
		public abstract class invoiceDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _InvoiceDate;
		[PXDBDate(BqlField = typeof(ARInvoice.invoiceDate))]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		public virtual DateTime? InvoiceDate
		{
			get
			{
				return this._InvoiceDate;
			}
			set
			{
				this._InvoiceDate = value;
			}
		}
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.IBqlField
		{
		}
		protected String _TaxZoneID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(ARInvoice.taxZoneID))]
		[PXDefault(typeof(Search<Location.cTaxZoneID, Where<Location.bAccountID, Equal<Current<ARCashSale.customerID>>, And<Location.locationID, Equal<Current<ARCashSale.customerLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Customer Tax Zone", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
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
		#region MasterRefNbr
		public abstract class masterRefNbr : PX.Data.IBqlField
		{
		}
		protected String _MasterRefNbr;
		[PXDBString(15, IsUnicode = true, BqlField = typeof(ARInvoice.masterRefNbr))]
		public virtual String MasterRefNbr
		{
			get
			{
				return this._MasterRefNbr;
			}
			set
			{
				this._MasterRefNbr = value;
			}
		}
		#endregion
		#region InstallmentNbr
		public abstract class installmentNbr : PX.Data.IBqlField
		{
		}
		protected Int16? _InstallmentNbr;
		[PXDBShort(BqlField = typeof(ARInvoice.installmentNbr))]
		public virtual Int16? InstallmentNbr
		{
			get
			{
				return this._InstallmentNbr;
			}
			set
			{
				this._InstallmentNbr = value;
			}
		}
		#endregion
		#region CuryTaxTotal
		public abstract class curyTaxTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTaxTotal;
		[PXDBCurrency(typeof(ARCashSale.curyInfoID), typeof(ARCashSale.taxTotal), BqlField = typeof(ARInvoice.curyTaxTotal))]
		[PXUIField(DisplayName = "Tax Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTaxTotal
		{
			get
			{
				return this._CuryTaxTotal;
			}
			set
			{
				this._CuryTaxTotal = value;
			}
		}
		#endregion
		#region TaxTotal
		public abstract class taxTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _TaxTotal;
		[PXDBDecimal(4, BqlField = typeof(ARInvoice.taxTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TaxTotal
		{
			get
			{
				return this._TaxTotal;
			}
			set
			{
				this._TaxTotal = value;
			}
		}
		#endregion
		#region CuryLineTotal
		public abstract class curyLineTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryLineTotal;
		[PXDBCurrency(typeof(ARCashSale.curyInfoID), typeof(ARCashSale.lineTotal), BqlField = typeof(ARInvoice.curyLineTotal))]
		[PXUIField(DisplayName = "Detail Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryLineTotal
		{
			get
			{
				return this._CuryLineTotal;
			}
			set
			{
				this._CuryLineTotal = value;
			}
		}
		#endregion
                
        #region CuryVatExemptTotal
        public abstract class curyVatExemptTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryVatExemptTotal;
        [PXDBCurrency(typeof(ARCashSale.curyInfoID), typeof(ARCashSale.vatExemptTotal), BqlField = typeof(ARInvoice.curyVatExemptTotal))]
        [PXUIField(DisplayName = "VAT Exempt Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryVatExemptTotal
        {
            get
            {
                return this._CuryVatExemptTotal;
            }
            set
            {
                this._CuryVatExemptTotal = value;
            }
        }
        #endregion

        #region VatExemptTotal
        public abstract class vatExemptTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _VatExemptTotal;
        [PXDBDecimal(4, BqlField = typeof(ARInvoice.vatExemptTotal))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? VatExemptTotal
        {
            get
            {
                return this._VatExemptTotal;
            }
            set
            {
                this._VatExemptTotal = value;
            }
        }
        #endregion
        
        #region CuryVatTaxableTotal
        public abstract class curyVatTaxableTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryVatTaxableTotal;
        [PXDBCurrency(typeof(ARCashSale.curyInfoID), typeof(ARCashSale.vatTaxableTotal), BqlField = typeof(ARInvoice.curyVatTaxableTotal))]
        [PXUIField(DisplayName = "VAT Taxable Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryVatTaxableTotal
        {
            get
            {
                return this._CuryVatTaxableTotal;
            }
            set
            {
                this._CuryVatTaxableTotal = value;
            }
        }
        #endregion

        #region VatTaxableTotal
        public abstract class vatTaxableTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _VatTaxableTotal;
        [PXDBDecimal(4, BqlField = typeof(ARInvoice.vatTaxableTotal))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? VatTaxableTotal
        {
            get
            {
                return this._VatTaxableTotal;
            }
            set
            {
                this._VatTaxableTotal = value;
            }
        }
        #endregion

		#region LineTotal
		public abstract class lineTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _LineTotal;
		[PXDBDecimal(4, BqlField = typeof(ARInvoice.lineTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LineTotal
		{
			get
			{
				return this._LineTotal;
			}
			set
			{
				this._LineTotal = value;
			}
		}
		#endregion
		#region CommnPct
		public abstract class commnPct : PX.Data.IBqlField
		{
		}
		protected Decimal? _CommnPct;
		[PXDBDecimal(6, BqlField = typeof(ARInvoice.commnPct))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CommnPct
		{
			get
			{
				return this._CommnPct;
			}
			set
			{
				this._CommnPct = value;
			}
		}
		#endregion
		#region CuryCommnAmt
		public abstract class curyCommnAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryCommnAmt;
		[PXDBCurrency(typeof(ARCashSale.curyInfoID), typeof(ARCashSale.commnAmt), BqlField = typeof(ARInvoice.curyCommnAmt))]
		[PXUIField(DisplayName = "Commission Amt.", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryCommnAmt
		{
			get
			{
				return this._CuryCommnAmt;
			}
			set
			{
				this._CuryCommnAmt = value;
			}
		}
		#endregion
		#region CommnAmt
		public abstract class commnAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CommnAmt;
		[PXDBDecimal(4, BqlField = typeof(ARInvoice.commnAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CommnAmt
		{
			get
			{
				return this._CommnAmt;
			}
			set
			{
				this._CommnAmt = value;
			}
		}
		#endregion
		#region CuryCommnblAmt
		public abstract class curyCommnblAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryCommnblAmt;
		[PXDBCurrency(typeof(ARCashSale.curyInfoID), typeof(ARCashSale.commnblAmt), BqlField = typeof(ARInvoice.curyCommnblAmt))]
		[PXUIField(DisplayName = "Total Commissionable", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryCommnblAmt
		{
			get
			{
				return this._CuryCommnblAmt;
			}
			set
			{
				this._CuryCommnblAmt = value;
			}
		}
		#endregion
		#region CommnblAmt
		public abstract class commnblAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CommnblAmt;
		[PXDBDecimal(4, BqlField = typeof(ARInvoice.commnblAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CommnblAmt
		{
			get
			{
				return this._CommnblAmt;
			}
			set
			{
				this._CommnblAmt = value;
			}
		}
		#endregion
		#region DontPrint
		public abstract class dontPrint : PX.Data.IBqlField
		{
		}
		protected Boolean? _DontPrint;
		[PXDBBool(BqlField = typeof(ARInvoice.dontPrint))]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Don't Print")]
		public virtual Boolean? DontPrint
		{
			get
			{
				return this._DontPrint;
			}
			set
			{
				this._DontPrint = value;
			}
		}
		#endregion		
		#region DontEmail
		public abstract class dontEmail : PX.Data.IBqlField
		{
		}
		protected Boolean? _DontEmail;
		[PXDBBool(BqlField = typeof(ARInvoice.dontEmail))]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Don't Email")]
		public virtual Boolean? DontEmail
		{
			get
			{
				return this._DontEmail;
			}
			set
			{
				this._DontEmail = value;
			}
		}
		#endregion		
		#region Printed
		public abstract class printed : PX.Data.IBqlField
		{
		}
		protected Boolean? _Printed;
		[PXDBBool(BqlField = typeof(ARInvoice.printed))]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Printed")]
		public virtual Boolean? Printed
		{
			get
			{
				return this._Printed;
			}
			set
			{
				this._Printed = value;
			}
		}
		#endregion
		#region CreditHold
		public abstract class creditHold : PX.Data.IBqlField
		{
		}
		protected Boolean? _CreditHold;
		[PXDBBool(BqlField = typeof(ARInvoice.creditHold))]
		[PXDefault(false)]
		public virtual Boolean? CreditHold
		{
			get
			{
				return this._CreditHold;
			}
			set
			{
				this._CreditHold = value;
			}
		}
		#endregion
		#region ApprovedCredit
		public abstract class approvedCredit : PX.Data.IBqlField
		{
		}
		protected Boolean? _ApprovedCredit;
		[PXDBBool(BqlField = typeof(ARInvoice.approvedCredit))]
		[PXDefault(false)]
		public virtual Boolean? ApprovedCredit
		{
			get
			{
				return this._ApprovedCredit;
			}
			set
			{
				this._ApprovedCredit = value;
			}
		}
		#endregion
		#region ApprovedCreditAmt
		public abstract class approvedCreditAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _ApprovedCreditAmt;
		[PXDBDecimal(4, BqlField = typeof(ARInvoice.approvedCreditAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ApprovedCreditAmt
		{
			get
			{
				return this._ApprovedCreditAmt;
			}
			set
			{
				this._ApprovedCreditAmt = value;
			}
		}
		#endregion
		#region Emailed
		public abstract class emailed : PX.Data.IBqlField
		{
		}
		protected Boolean? _Emailed;
		[PXDBBool(BqlField = typeof(ARInvoice.emailed))]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Emailed")]
		public virtual Boolean? Emailed
		{
			get
			{
				return this._Emailed;
			}
			set
			{
				this._Emailed = value;
			}
		}
		#endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.IBqlField
		{
		}
		protected int? _WorkgroupID;
		[PXCompanyTreeSelector]
		[PXDefault(typeof(Customer.workgroupID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBInt(BqlField = typeof(ARInvoice.workgroupID))]
		[PXUIField(DisplayName = "Workgroup ID")]
		public virtual int? WorkgroupID
		{
			get
			{
				return this._WorkgroupID;
			}
			set
			{
				this._WorkgroupID = value;
			}
		}
		#endregion
		#region OwnerID
		public abstract class ownerID : IBqlField { }
		protected Guid? _OwnerID;
		[PXDefault(typeof(Customer.ownerID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBGuid(BqlField = typeof(ARInvoice.ownerID))]
		[PXUIField(DisplayName = "Owner ID")]
		[PXOwnerSelector(typeof(ARCashSale.workgroupID))]
		public virtual Guid? OwnerID
		{
			get
			{
				return this._OwnerID;
			}
			set
			{
				this._OwnerID = value;
			}
		}
		#endregion
		#region PrintInvoice
		public abstract class printInvoice : PX.Data.IBqlField
		{
		}
		[PXBool()]
		public virtual Boolean? PrintInvoice
		{
			[PXDependsOnFields(typeof(dontPrint), typeof(printed))]
			get
			{
				return this._DontPrint != true && (this._Printed == null || this._Printed == false);
			}
		}
		#endregion
		#region EmailInvoice
		public abstract class emailInvoice : PX.Data.IBqlField
		{
		}
		[PXBool()]
		public virtual Boolean? EmailInvoice
		{
			[PXDependsOnFields(typeof(dontEmail), typeof(emailed))]
			get
			{
				return (this._DontEmail != true && (this._Emailed == null || (bool)this._Emailed == false));
			}
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.IBqlField
		{
		}
		protected Int32? _ProjectID;
		[ProjectDefault(BatchModule.AR,
			typeof(Search<Location.cDefProjectID, Where<Location.bAccountID, Equal<Current<ARCashSale.customerID>>,And<Location.locationID, Equal<Current<ARCashSale.customerLocationID>>>>>))]
		[ActiveProjectForModule(BatchModule.AR, typeof(ARCashSale.customerID), true, true, BqlField = typeof(ARInvoice.projectID))]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
        #region IsRUTROTDeductible
        public abstract class isRUTROTDeductible : IBqlField { }

        [PXDBBool(BqlField = typeof(ARInvoice.isRUTROTDeductible))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "ROT and RUT deductible document", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual bool? IsRUTROTDeductible { get; set; }
        #endregion
        #region RUTROTType
        public abstract class rUTROTType : IBqlField { }

        [PXDBString(1, BqlField = typeof(ARInvoice.rUTROTType))]
        [RUTROTTypes.List]
        [PXDefault(RUTROTTypes.RUT, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Deduction type", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual string RUTROTType { get; set; }
        #endregion
        #region CuryRUTROTTotalAmt
        public abstract class curyRUTROTTotalAmt : IBqlField { }

        [PXDBDecimal(4, BqlField = typeof(ARInvoice.curyRUTROTTotalAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Deductible Amount", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual Decimal? CuryRUTROTTotalAmt { get; set; }
        #endregion
        #region RUTROTTotalAmt
        public abstract class rUTROTTotalAmt : IBqlField { }

        [PXDBDecimal(4, BqlField = typeof(ARInvoice.rUTROTTotalAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? RUTROTTotalAmt { get; set; }
        #endregion
        #region CuryRUTROTPersonalAllowance
        public abstract class curyRUTROTPersonalAllowance : IBqlField { }

        [PXDBDecimal(4, BqlField = typeof(ARInvoice.curyRUTROTPersonalAllowance))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Personal Allowance", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual Decimal? CuryRUTROTPersonalAllowance { get; set; }
        #endregion
        #region RUTROTPersonalAllowance
        public abstract class rUTROTPersonalAllowance : IBqlField { }

        [PXDBDecimal(4, BqlField = typeof(ARInvoice.rUTROTPersonalAllowance))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? RUTROTPersonalAllowance { get; set; }
        #endregion
        #region RUTROTDeductionPct
        public abstract class rUTROTDeductionPct : IBqlField { }

        [PXDBDecimal(2, BqlField = typeof(ARInvoice.rUTROTDeductionPct))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Deduction %", Visible = false, FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual Decimal? RUTROTDeductionPct { get; set; }
        #endregion
        #region RUTROTAutoDistribution
        public abstract class rUTROTAutoDistribution : IBqlField { }

        [PXDBBool(BqlField = typeof(ARInvoice.rUTROTAutoDistribution))]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Distribute Automatically", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual bool? RUTROTAutoDistribution { get; set; }
        #endregion
        #region IsRUTROTClaimed
        public abstract class isRUTROTClaimed : IBqlField { }

        [PXDBBool(BqlField = typeof(ARInvoice.isRUTROTClaimed))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "ROT and RUT was claimed", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual bool? IsRUTROTClaimed { get; set; }
        #endregion
        #region CuryRUTROTDistributedAmt
        public abstract class curyRUTROTDistributedAmt : IBqlField { }

        [PXDBDecimal(4, BqlField = typeof(ARInvoice.curyRUTROTDistributedAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? CuryRUTROTDistributedAmt { get; set; }
        #endregion
        #region RUTROTDistributedAmt
        public abstract class rUTROTDistributedAmt : IBqlField { }

        [PXDBDecimal(4, BqlField = typeof(ARInvoice.rUTROTDistributedAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? RUTROTDistributedAmt { get; set; }
        #endregion
        #region RUTROTDistributionLineCntr
        public abstract class rUTROTDistributionLineCntr : IBqlField { }

        [PXDBInt(BqlField = typeof(ARInvoice.rUTROTDistributionLineCntr))]
        [PXDefault(0)]
        public virtual Int32? RUTROTDistributionLineCntr { get; set; }
        #endregion
        #region SkipDiscounts
        public abstract class skipDiscounts : IBqlField
        {
        }
        protected bool? _SkipDiscounts = false;
        [PXDBBool(BqlField = typeof(ARInvoice.skipDiscounts))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Skip Group and Document Discounts")]
        public virtual bool? SkipDiscounts
        {
            get
            {
                return _SkipDiscounts;
            }
            set
            {
                _SkipDiscounts = value;
            }
        }
        #endregion

		//ARPayment
		#region ARPaymentDocType
		public abstract class aRPaymentDocType : PX.Data.IBqlField
		{
		}
		[PXDBString(3, IsFixed = true, BqlField = typeof(ARPayment.docType))]
		[PXDefault()]
		[PXRestriction()]
		public virtual String ARPaymentDocType
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
		#region ARPaymentRefNbr
		public abstract class aRPaymentRefNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true, InputMask = "", BqlField = typeof(ARPayment.refNbr))]
		[PXDefault()]
		[PXRestriction()]
		public virtual String ARPaymentRefNbr
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
		#region BranchID
		public new abstract class branchID : PX.Data.IBqlField
		{
		}
		[Branch(typeof(Coalesce<
			Search<Location.cBranchID, Where<Location.bAccountID, Equal<Current<ARCashSale.customerID>>, And<Location.locationID, Equal<Current<ARCashSale.customerLocationID>>>>>,
			Search<GL.Branch.branchID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>), IsDetail = false, BqlField = typeof(ARRegister.branchID))]
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
        #region PaymentMethodID
        public abstract class paymentMethodID : PX.Data.IBqlField
        {
        }
        protected String _PaymentMethodID;        
        [PXDBString(10, IsUnicode = true, BqlField = typeof(ARPayment.paymentMethodID))]
        [PXDefault(typeof(Coalesce<Search2<CustomerPaymentMethod.paymentMethodID, InnerJoin<Customer, On<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>>>,
                                        Where<Customer.bAccountID, Equal<Current<ARCashSale.customerID>>,
                                              And<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>>>>,
                                   Search<Customer.defPaymentMethodID,
                                         Where<Customer.bAccountID, Equal<Current<ARCashSale.customerID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]                   

        [PXSelector(typeof(Search5<PaymentMethod.paymentMethodID, LeftJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.paymentMethodID, Equal<PaymentMethod.paymentMethodID>,
                                    And<CustomerPaymentMethod.bAccountID, Equal<Current<ARCashSale.customerID>>>>>,
                                        Where<PaymentMethod.isActive, Equal<boolTrue>,
                                        And<PaymentMethod.useForAR, Equal<boolTrue>,
                                        And<Where<PaymentMethod.aRIsOnePerCustomer, Equal<True>,
                                           Or<Where<CustomerPaymentMethod.pMInstanceID, IsNotNull>>>>>>, Aggregate<GroupBy<PaymentMethod.paymentMethodID,GroupBy<PaymentMethod.useForAR,GroupBy<PaymentMethod.useForAP>>>>>), DescriptionField = typeof(PaymentMethod.descr))]
        [PXUIField(DisplayName = "Payment Method")]
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
		[PXDBInt(BqlField = typeof(ARPayment.pMInstanceID))]
		[PXUIField(DisplayName = "Card/Account No")]
        [PXDefault(typeof(Coalesce<
                        Search2<Customer.defPMInstanceID, InnerJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>,
                                And<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>>>>,
                                Where<Customer.bAccountID, Equal<Current2<ARCashSale.customerID>>,
									And<CustomerPaymentMethod.isActive,Equal<True>,
									And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<ARCashSale.paymentMethodID>>>>>>,
                        Search<CustomerPaymentMethod.pMInstanceID,
                                Where<CustomerPaymentMethod.bAccountID, Equal<Current2<ARCashSale.customerID>>,
                                    And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<ARCashSale.paymentMethodID>>,
                                    And<CustomerPaymentMethod.isActive, Equal<True>>>>,
								OrderBy<Desc<CustomerPaymentMethod.expirationDate, 
									Desc<CustomerPaymentMethod.pMInstanceID>>>>>)
                        , PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search<CustomerPaymentMethod.pMInstanceID, Where<CustomerPaymentMethod.bAccountID, Equal<Current2<ARCashSale.customerID>>,
            And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<ARCashSale.paymentMethodID>>,
            And<Where<CustomerPaymentMethod.isActive, Equal<boolTrue>, Or<CustomerPaymentMethod.pMInstanceID,
                    Equal<Current<ARCashSale.pMInstanceID>>>>>>>>), DescriptionField = typeof(CustomerPaymentMethod.descr))]

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
		#region PMInstanceID_CustomerPaymentMethod_descr
		public abstract class pMInstanceID_CustomerPaymentMethod_descr : PX.Data.IBqlField
		{
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _CashAccountID;
        [PXDefault(typeof(Coalesce<Search2<CustomerPaymentMethod.cashAccountID,
										InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CustomerPaymentMethod.cashAccountID>,
											And<PaymentMethodAccount.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>,
											And<PaymentMethodAccount.useForAR, Equal<True>>>>>,
										Where<CustomerPaymentMethod.bAccountID, Equal<Current<ARCashSale.customerID>>,
											And<CustomerPaymentMethod.pMInstanceID, Equal<Current2<ARCashSale.pMInstanceID>>>>>,
                            Search2<CashAccount.cashAccountID,
                                InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
                                    And<PaymentMethodAccount.useForAR, Equal<True>,
									And<PaymentMethodAccount.aRIsDefault,Equal<True>,
                                    And<PaymentMethodAccount.paymentMethodID, Equal<Current2<ARCashSale.paymentMethodID>>>>>>>,
									Where<CashAccount.branchID, Equal<Current<ARCashSale.branchID>>, 
										And<Match<Current<AccessInfo.userName>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [CashAccount(typeof(ARCashSale.branchID), typeof(Search2<CashAccount.cashAccountID, 
                    InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>, 
                    And<PaymentMethodAccount.paymentMethodID, Equal<Current2<ARCashSale.paymentMethodID>>,
                    And<PaymentMethodAccount.useForAR, Equal<True>>>>>, Where<Match<Current<AccessInfo.userName>>>>), DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(CashAccount.descr), BqlField = typeof(ARPayment.cashAccountID), SuppressCurrencyValidation = true)]
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
		public new abstract class curyID : PX.Data.IBqlField
		{
		}
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(ARRegister.curyID))]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Search<Company.baseCuryID>))]
		[PXSelector(typeof(Currency.curyID))]
		public override String CuryID
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
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.IBqlField
		{
		}
		protected String _ExtRefNbr;
		[PXDBString(40, IsUnicode = true, BqlField = typeof(ARPayment.extRefNbr))]
		[PXUIField(DisplayName = "Payment Ref.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault()]
        [PaymentRef(typeof(PX.Objects.AR.Standalone.ARCashSale.cashAccountID), typeof(PX.Objects.AR.Standalone.ARCashSale.paymentMethodID), typeof(PX.Objects.AR.ARPayment.updateNextNumber))]
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
		#region AdjDate
		public abstract class adjDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _AdjDate;
		[PXDBDate(BqlField = typeof(ARPayment.adjDate))]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? AdjDate
		{
			get
			{
				return this._AdjDate;
			}
			set
			{
				this._AdjDate = value;
			}
		}
		#endregion
		#region AdjFinPeriodID
		public abstract class adjFinPeriodID : PX.Data.IBqlField
		{
		}
		protected String _AdjFinPeriodID;
		[AROpenPeriod(typeof(ARCashSale.adjDate), BqlField = typeof(ARPayment.adjFinPeriodID))]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String AdjFinPeriodID
		{
			get
			{
				return this._AdjFinPeriodID;
			}
			set
			{
				this._AdjFinPeriodID = value;
			}
		}
		#endregion
		#region AdjTranPeriodID
		public abstract class adjTranPeriodID : PX.Data.IBqlField
		{
		}
		protected String _AdjTranPeriodID;
		[FinPeriodID(typeof(ARCashSale.adjDate), BqlField = typeof(ARPayment.adjTranPeriodID))]
		public virtual String AdjTranPeriodID
		{
			get
			{
				return this._AdjTranPeriodID;
			}
			set
			{
				this._AdjTranPeriodID = value;
			}
		}
		#endregion
		#region Cleared
		public abstract class cleared : PX.Data.IBqlField
		{
		}
		protected Boolean? _Cleared;
		[PXDBBool(BqlField = typeof(ARPayment.cleared))]
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
		[PXDBDate(BqlField = typeof(ARPayment.clearDate))]
		[PXUIField(DisplayName = "Clear Date")]
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
		#region CATranID
		public abstract class cATranID : PX.Data.IBqlField
		{
		}
		protected Int64? _CATranID;
		[PXDBLong(BqlField = typeof(ARPayment.cATranID))]
		[ARCashSaleCashTranID()]
		public virtual Int64? CATranID
		{
			get
			{
				return this._CATranID;
			}
			set
			{
				this._CATranID = value;
			}
		}
		#endregion
		#region ARDepositAsBatch
		public abstract class depositAsBatch : PX.Data.IBqlField
		{
		}
		protected Boolean? _DepositAsBatch;
		[PXDBBool(BqlField = typeof(ARPayment.depositAsBatch))]		
		[PXUIField(DisplayName = "Batch Deposit", Enabled = false)]
		[PXDefault(typeof(Search<CashAccount.clearingAccount, Where<CashAccount.cashAccountID, Equal<Current<ARCashSale.cashAccountID>>>>))]
		public virtual Boolean? DepositAsBatch
		{
			get
			{
				return this._DepositAsBatch;
			}
			set
			{
				this._DepositAsBatch = value;
			}
		}
		#endregion

        #region ChargeCntr
        public abstract class chargeCntr : PX.Data.IBqlField
        {
        }
        protected Int32? _ChargeCntr;
        [PXDBInt(BqlField = typeof(ARPayment.chargeCntr))]
        [PXDefault(0)]
        public virtual Int32? ChargeCntr
        {
            get
            {
                return this._ChargeCntr;
            }
            set
            {
                this._ChargeCntr = value;
            }
        }
        #endregion

        #region DepositAfter
        public abstract class depositAfter : PX.Data.IBqlField
		{
		}
		protected DateTime? _DepositAfter;
		[PXDBDate(BqlField = typeof(ARPayment.depositAfter))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Deposit After", Enabled = false, Visible = false)]
		public virtual DateTime? DepositAfter
		{
			get
			{
				return this._DepositAfter;
			}
			set
			{
				this._DepositAfter = value;
			}
		}
		#endregion
		#region Deposited
		public abstract class deposited : PX.Data.IBqlField
		{
		}
		protected Boolean? _Deposited;
		[PXDBBool(BqlField = typeof(ARPayment.deposited))]
		[PXUIField(DisplayName = "Deposited", Enabled = false)]
		[PXDefault(false)]
		public virtual Boolean? Deposited
		{
			get
			{
				return this._Deposited;
			}
			set
			{
				this._Deposited = value;
			}
		}
		#endregion
		#region DepositDate
		public abstract class depositDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _DepositDate;
		[PXDBDate(BqlField = typeof(ARPayment.depositDate))]		
		[PXUIField(DisplayName = "Batch Deposit Date", Enabled = false)]
		public virtual DateTime? DepositDate
		{
			get
			{
				return this._DepositDate;
			}
			set
			{
				this._DepositDate = value;
			}
		}
		#endregion
		
		#region DepositType
		public abstract class depositType : PX.Data.IBqlField
		{
		}
		protected String _DepositType;
		[PXUIField(Enabled = false)]
		[PXDBString(3, IsFixed = true, BqlField = typeof(ARPayment.depositType))]
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
		[PXDBString(15, IsUnicode = true, BqlField = typeof(ARPayment.depositNbr))]
		[PXUIField(DisplayName = "Batch Deposit Nbr.", Enabled = false)]
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

		#region PaymentProjectID
		public abstract class paymentProjectID : PX.Data.IBqlField
		{
		}
		protected Int32? _PaymentProjectID;
		[ProjectDefault(BatchModule.AR)]
		[ActiveProjectForModule(BatchModule.AR, false, BqlField = typeof(ARPayment.projectID))]
		public virtual Int32? PaymentProjectID
		{
			get
			{
				return this._PaymentProjectID;
			}
			set
			{
				this._PaymentProjectID = value;
			}
		}
		#endregion
		#region PaymentTaskID
		public abstract class paymentTaskID : PX.Data.IBqlField
		{
		}
		protected Int32? _PaymentTaskID;
		[ActiveProjectTask(typeof(ARCashSale.paymentProjectID), BatchModule.AR, DisplayName = "Project Task", BqlField = typeof(ARPayment.taskID))]
		public virtual Int32? PaymentTaskID
		{
			get
			{
				return this._PaymentTaskID;
			}
			set
			{
				this._PaymentTaskID = value;
			}
		}
		#endregion

        #region CuryConsolidateChargeTotal
        public abstract class curyConsolidateChargeTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryConsolidateChargeTotal;
        [PXDBCurrency(typeof(ARCashSale.curyInfoID), typeof(ARCashSale.consolidateChargeTotal), BqlField=typeof(ARPayment.curyConsolidateChargeTotal))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Deducted Charges", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public virtual Decimal? CuryConsolidateChargeTotal
        {
            get
            {
                return this._CuryConsolidateChargeTotal;
            }
            set
            {
                this._CuryConsolidateChargeTotal = value;
            }
        }
        #endregion
        #region ConsolidateChargeTotal
        public abstract class consolidateChargeTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _ConsolidateChargeTotal;
        [PXDBDecimal(4, BqlField=typeof(ARPayment.consolidateChargeTotal))]
        public virtual Decimal? ConsolidateChargeTotal
        {
            get
            {
                return this._ConsolidateChargeTotal;
            }
            set
            {
                this._ConsolidateChargeTotal = value;
            }
        }
        #endregion
		#region RefTranExtNbr
		public abstract class refTranExtNbr : PX.Data.IBqlField
		{
		}
		protected String _RefTranExtNbr;
		[PXDBString(50, IsUnicode = true, BqlField = typeof(ARPayment.refTranExtNbr))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<CCProcTran.pCTranNumber, Where<CCProcTran.pMInstanceID, Equal<Current<ARCashSale.pMInstanceID>>,
								And<CCProcTran.procStatus, Equal<CCProcStatus.finalized>,
								And<CCProcTran.tranStatus, Equal<CCTranStatusCode.approved>,
								And<Where<CCProcTran.tranType, Equal<CCTranTypeCode.authorizeAndCapture>,
								Or<CCProcTran.tranType, Equal<CCTranTypeCode.priorAuthorizedCapture>>>>>>>, OrderBy<Desc<CCProcTran.tranNbr>>>))]
		[PXUIField(DisplayName = "Orig. PC Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String RefTranExtNbr
		{
			get
			{
				return this._RefTranExtNbr;
			}
			set
			{
				this._RefTranExtNbr = value;
			}
		}
		#endregion
        #region IsRUTROTPayment
        public abstract class isRUTROTPayment : IBqlField { }

        [PXDBBool(BqlField = typeof(ARPayment.isRUTROTPayment))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "ROT or RUT payment", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual bool? IsRUTROTPayment { get; set; }
        #endregion

        //ARRegister
		#region DocDate
		public new abstract class docDate : PX.Data.IBqlField
		{
		}
		[PXDBDate(BqlField = typeof(ARRegister.docDate))]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public override DateTime? DocDate
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
		#region TranPeriodID
		public new abstract class tranPeriodID : PX.Data.IBqlField
		{
		}
		[FinPeriodID(typeof(ARCashSale.docDate), BqlField = typeof(ARRegister.tranPeriodID))]
		public override String TranPeriodID
		{
			get
			{
				return this._TranPeriodID;
			}
			set
			{
				this._TranPeriodID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		[FinPeriodID(typeof(ARCashSale.docDate), BqlField = typeof(ARRegister.finPeriodID))]
        [PXDefault()]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
		public override String FinPeriodID
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
		#region CustomerID_Customer_acctName
		public abstract class customerID_Customer_acctName : PX.Data.IBqlField
		{
		}
		#endregion
		#region VoidAppl
		public abstract class voidAppl : PX.Data.IBqlField
		{
		}
		[PXBool()]
		[PXDefault(false)]
		public virtual Boolean? VoidAppl
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return (this._DocType == ARPaymentType.CashReturn);
			}
			set
			{
				if ((bool)value)
				{
					this._DocType = ARPaymentType.CashReturn;
				}
			}
		}
		#endregion
		#region IsCCPayment
		public abstract class isCCPayment : PX.Data.IBqlField
		{
		}

		protected bool? _IsCCPayment;
		[PXBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Visible = false, Enabled = false)]
		public virtual bool? IsCCPayment
		{
			get
			{
				return this._IsCCPayment;
			}
			set
			{
				this._IsCCPayment = value;
			}
		}
		#endregion
		#region CCPaymentStateDescr
		public abstract class cCPaymentStateDescr : PX.Data.IBqlField
		{
		}
		protected String _CCPaymentStateDescr;
		[PXString(255)]
		[PXUIField(DisplayName = "Processing Status", Enabled = false)]
		public virtual String CCPaymentStateDescr
		{
			get
			{
				return this._CCPaymentStateDescr;
			}
			set
			{
				this._CCPaymentStateDescr = value;
			}
		}
		#endregion

		#region ICCPayment Members

		string ICCPayment.OrigDocType
		{
			get
			{
				return null;
			}
		}

		string ICCPayment.OrigRefNbr
		{
			get
			{
				return null;
			}

		}

		#endregion
	}

	public class ARCashSaleType : ARDocType
	{
        /// <summary>
        /// Specialized selector for ARCashSale RefNbr.<br/>
        /// By default, defines the following set of columns for the selector:<br/>
        /// ARCashSale.refNbr,ARCashSale.docDate, ARCashSale.finPeriodID,<br/>
        /// ARCashSale.customerID, ARCashSale.customerID_Customer_acctName,<br/>
        /// ARCashSale.customerLocationID, ARCashSale.curyID, ARCashSale.curyOrigDocAmt,<br/>
        /// ARCashSale.curyDocBal,ARCashSale.status, ARCashSale.dueDate, ARCashSale.invoiceNbr<br/>
        /// </summary>
		public class RefNbrAttribute : PXSelectorAttribute
		{
            /// <summary>
            /// Ctor
            /// </summary>
            /// <param name="SearchType">Must be IBqlSearch, returning ARCashSale.refNbr</param>
			public RefNbrAttribute(Type SearchType)
				: base(SearchType,
                typeof(ARCashSale.refNbr),
                typeof(ARCashSale.extRefNbr),
				typeof(ARCashSale.docDate),
				typeof(ARCashSale.finPeriodID),
				typeof(ARCashSale.customerID),
				typeof(ARCashSale.customerID_Customer_acctName),
				typeof(ARCashSale.customerLocationID),
				typeof(ARCashSale.curyID),
				typeof(ARCashSale.curyOrigDocAmt),
				typeof(ARCashSale.curyDocBal),
				typeof(ARCashSale.status),
				typeof(ARCashSale.cashAccountID),
				typeof(ARCashSale.pMInstanceID_CustomerPaymentMethod_descr))
			{
			}
		}

        /// <summary>
        /// Specialized for AR CashSales version of the of the <see cref="AutoNumberAttribute"/> <br/>
        /// It defines how the new numbers are generated for the AR CashSales. <br/>
        /// References ARInvoice.docType and ARInvoice.docDate fields of the document,<br/>
        /// and also define a link between  numbering ID's defined in AR Setup and ARInvoice types:<br/>
        /// namely ARSetup.paymentNumberingID - for AR CashSale and CashReturn 
        /// </summary>		
		public class NumberingAttribute : AutoNumberAttribute
		{
			public NumberingAttribute()
				: base(typeof(ARCashSale.docType), typeof(ARCashSale.docDate),
					new string[] { CashSale, CashReturn },
					new Type[] { typeof(ARSetup.paymentNumberingID), typeof(ARSetup.paymentNumberingID) }) { ; }
		}

		public new class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { CashSale, CashReturn },
				new string[] { Messages.CashSale, Messages.CashReturn }) { ; }
		}

		public static bool VoidAppl(string DocType)
		{
			return (DocType == VoidPayment);
		}
	}
}
