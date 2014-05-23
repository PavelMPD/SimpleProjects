namespace PX.Objects.DR
{
	using System;
	using PX.Data;
	using PX.Objects.IN;
	using PX.Objects.GL;
	using PX.Objects.AR;
	using PX.Objects.AP;
	using PX.Objects.CR;
	using PX.Objects.CM;
	using PX.Objects.CS;
	using PX.Objects.CT;
	using System.Diagnostics;
	using System.Collections;

	[System.SerializableAttribute()]
	[DebuggerDisplay("SheduleID={ScheduleID} ComponentID={ComponentID} TotalAmt={TotalAmt} DefAmt={DefAmt}")]
	[PXPrimaryGraph(typeof(ScheduleMaint))]
	public partial class DRScheduleDetail : PX.Data.IBqlTable
	{
		public const int EmptyComponentID = 0;
		public const int EmptyBAccountID = 0;

		#region ScheduleID
		public abstract class scheduleID : PX.Data.IBqlField
		{
		}
		protected Int32? _ScheduleID;
		[PXDBLiteDefault(typeof(DRSchedule.scheduleID))]
		[PXSelector(typeof(DRSchedule.scheduleID), DirtyRead=true)]
		[PXParent(typeof(Select<DRSchedule, Where<DRSchedule.scheduleID, Equal<Current<DRScheduleDetail.scheduleID>>>>))]
		[PXDBInt(IsKey=true)]
		[PXUIField(DisplayName = "Schedule ID")]
		public virtual Int32? ScheduleID
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
		#region ComponentID
		public abstract class componentID : PX.Data.IBqlField
		{
		}
		protected Int32? _ComponentID;

		[PXDBInt(IsKey=true)]
		[PXUIField(DisplayName = "Component ID", Visibility = PXUIVisibility.Visible)]
		[DRComponentSelector(SubstituteKey=typeof(InventoryItem.inventoryCD))]
		public virtual Int32? ComponentID
		{
			get
			{
				return this._ComponentID;
			}
			set
			{
				this._ComponentID = value;
			}
		}

		#endregion

		#region Module
		public abstract class module : PX.Data.IBqlField
		{
		}
		protected String _Module;
		[PXDefault(BatchModule.AR)]
		[PXDBString(2, IsFixed = true)]
		[PXStringList(new string[] { BatchModule.AR, BatchModule.AP }, new string[] { BatchModule.AR, BatchModule.AP })]
		[PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.Invisible)]
		public virtual String Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;

				switch (value)
				{
					case BatchModule.AP:
						_BAccountType = CR.BAccountType.VendorType;
						DefCodeType = DeferredAccountType.Expense;
						break;
					default:
						_BAccountType = CR.BAccountType.CustomerType;
						DefCodeType = DeferredAccountType.Income;
						break;
				}

			}
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.IBqlField
		{
		}
		protected String _DocType;
		[PXDefault(ARDocType.Invoice)]
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Doc. Type", Visibility = PXUIVisibility.Invisible)]
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
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[DRDocumentSelector(typeof(DRScheduleDetail.module), typeof(DRScheduleDetail.docType))]
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

		#region Status
		public abstract class status : PX.Data.IBqlField
		{
		}
		protected String _Status;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(DRScheduleStatus.Open)]
		[DRScheduleStatus.List]
		[PXUIField(DisplayName = "Status", Enabled = false, Visibility=PXUIVisibility.SelectorVisible)]
		public virtual String Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		#endregion
		#region TotalAmt
		public abstract class totalAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _TotalAmt;
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Amount")]
		public virtual Decimal? TotalAmt
		{
			get
			{
				return this._TotalAmt;
			}
			set
			{
				this._TotalAmt = value;
			}
		}
		#endregion
		#region DefAmt
		public abstract class defAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _DefAmt;
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Deferred Amount", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? DefAmt
		{
			get
			{
				return this._DefAmt;
			}
			set
			{
				this._DefAmt = value;
			}
		}
		#endregion
		#region DefCode
		public abstract class defCode : PX.Data.IBqlField
		{
		}
		protected String _DefCode;
		[PXSelector(typeof(Search<DRDeferredCode.deferredCodeID, Where<DRDeferredCode.accountType, Equal<Current<DRScheduleDetail.defCodeType>>>>))]
		[PXDBString(10, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Deferral Code", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String DefCode
		{
			get
			{
				return this._DefCode;
			}
			set
			{
				this._DefCode = value;
			}
		}
		#endregion
		#region DefAcctID
		public abstract class defAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _DefAcctID;
		[PXDefault]
		[Account(DisplayName = "Deferral Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		public virtual Int32? DefAcctID
		{
			get
			{
				return this._DefAcctID;
			}
			set
			{
				this._DefAcctID = value;
			}
		}
		#endregion
		#region DefSubID
		public abstract class defSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _DefSubID;
		[PXDefault]
        [SubAccount(typeof(DRScheduleDetail.defAcctID), DisplayName = "Deferral Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? DefSubID
		{
			get
			{
				return this._DefSubID;
			}
			set
			{
				this._DefSubID = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.IBqlField
		{
		}
		protected Int32? _AccountID;
		[PXDefault]
		[Account(DisplayName = "Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
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
		[PXDefault]
		[SubAccount(typeof(DRScheduleDetail.accountID), DisplayName = "Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
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
		#region IsOpen
		public abstract class isOpen : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsOpen;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "IsOpen")]
		public virtual Boolean? IsOpen
		{
			get
			{
				return this._IsOpen;
			}
			set
			{
				this._IsOpen = value;
			}
		}
		#endregion
		
		#region LineNbr
		public abstract class lineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _LineNbr;
		[PXDBInt()]
		[PXUIField(DisplayName = "Line Nbr.", Enabled=false)]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region DefTotal
		public abstract class defTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _DefTotal;
		[PXBaseCury]
		[PXUIField(DisplayName = "Line Total", Enabled = false)]
		public virtual Decimal? DefTotal
		{
			get
			{
				return this._DefTotal;
			}
			set
			{
				this._DefTotal = value;
			}
		}
		#endregion
		#region LineCntr
		public abstract class lineCntr : PX.Data.IBqlField
		{
		}
		protected Int32? _LineCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? LineCntr
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
		#region DocDate
		public abstract class docDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _DocDate;
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = "Doc. Date", Enabled = false)]
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
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		protected String _FinPeriodID;
		[PXDefault]
		[OpenPeriod(typeof(DRScheduleDetail.docDate))]
		[PXUIField(DisplayName = "Post Period", Enabled=false)]
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

		#region LastRecFinPeriodID
		public abstract class lastRecFinPeriodID : PX.Data.IBqlField
		{
		}
		protected String _LastRecFinPeriodID;
		[FinPeriodID]
		[PXUIField(DisplayName = "Last Recognition Period", Enabled = false)]
		public virtual String LastRecFinPeriodID
		{
			get
			{
				return this._LastRecFinPeriodID;
			}
			set
			{
				this._LastRecFinPeriodID = value;
			}
		}
		#endregion
		#region CloseFinPeriodID
		public abstract class closeFinPeriodID : PX.Data.IBqlField
		{
		}
		protected String _CloseFinPeriodID;
		[FinPeriodID]
		[PXUIField(DisplayName = "Close Period", Enabled = false)]
		public virtual String CloseFinPeriodID
		{
			get
			{
				return this._CloseFinPeriodID;
			}
			set
			{
				this._CloseFinPeriodID = value;
			}
		}
		#endregion

		#region CreditLineNbr
		public abstract class creditLineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _CreditLineNbr;
		[PXDBInt()]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Credit Line Nbr.", Visibility=PXUIVisibility.Invisible, Enabled = false)]
		public virtual Int32? CreditLineNbr
		{
			get
			{
				return this._CreditLineNbr;
			}
			set
			{
				this._CreditLineNbr = value;
			}
		}
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _BAccountID;
		[PXDBInt()]
		[PXDefault]
		[PXUIField(DisplayName="Customer/Vendor", Enabled=false)]
		[PXSelector(typeof(Search<BAccountR.bAccountID, Where<BAccountR.type, Equal<Current<DRScheduleDetail.bAccountType>>, Or<BAccountR.type, Equal<CR.BAccountType.combinedType>>>>), new Type[] { typeof(BAccountR.acctCD), typeof(BAccountR.acctName), typeof(BAccountR.type) }, SubstituteKey = typeof(BAccountR.acctCD))]
		public virtual Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		#region IsCustom
		public abstract class isCustom : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsCustom;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is Custom", Enabled=false)]
		public virtual Boolean? IsCustom
		{
			get
			{
				return this._IsCustom;
			}
			set
			{
				this._IsCustom = value;
			}
		}
		#endregion

		#region DocumentType
		public abstract class documentType : PX.Data.IBqlField
		{
		}
		protected String _DocumentType;
		[PXString(3, IsFixed = true)]
		[DRScheduleDocumentType.List]
		[PXUIField(DisplayName = "Doc. Type", Visibility=PXUIVisibility.SelectorVisible, Enabled = false, Required=true)]
		public virtual String DocumentType
		{
			get
			{
				return this._DocumentType;
			}
			set
			{
				this._DocumentType = value;
			}
		}
		#endregion

		#region BAccountType
		public abstract class bAccountType : PX.Data.IBqlField
		{
		}
		protected String _BAccountType;
		[PXDefault(CR.BAccountType.CustomerType, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXString(2, IsFixed = true)]
		[PXStringList(	new string[] { CR.BAccountType.VendorType, CR.BAccountType.CustomerType },
				new string[] { CR.Messages.VendorType, CR.Messages.CustomerType })]
		public virtual String BAccountType
		{
			get
			{
				return this._BAccountType;
			}
			set
			{
				this._BAccountType = value;
			}
		}
		#endregion

		#region DefCodeType
		public abstract class defCodeType : PX.Data.IBqlField
		{
		}
		protected string _DefCodeType;
		[PXString(1)]
		[PXDefault(DeferredAccountType.Income)]
		[DeferredAccountType.List()]
		public virtual string DefCodeType
		{
			get
			{
				return this._DefCodeType;
			}
			set
			{
				this._DefCodeType = value;
			}
		}
		#endregion


		#region Selected
		public abstract class selected : IBqlField
		{
		}
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Visible)]
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

		#region ProjectID
		public abstract class projectID : PX.Data.IBqlField
		{
		}
		protected Int32? _ProjectID;
		[PXDBScalar(typeof(Search2<ARTran.projectID, InnerJoin<DRSchedule, On<ARTran.tranType, Equal<DRSchedule.docType>, And<ARTran.refNbr, Equal<DRSchedule.refNbr>, And<ARTran.lineNbr, Equal<DRSchedule.lineNbr>>>>>, Where<DRSchedule.scheduleID, Equal<DRScheduleDetail.scheduleID>>>))]
		[PXDimensionSelector("CONTRACT", typeof(Search<Contract.contractID>), typeof(Contract.contractCD))]
		[PXUIField(DisplayName = "Contract ID", Visible = false, Visibility = PXUIVisibility.Invisible)]
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
		#region TaskID
		public abstract class taskID : PX.Data.IBqlField
		{
		}
		protected Int32? _TaskID;
		[PXDBScalar(typeof(Search2<ARTran.taskID, InnerJoin<DRSchedule, On<ARTran.tranType, Equal<DRSchedule.docType>, And<ARTran.refNbr, Equal<DRSchedule.refNbr>, And<ARTran.lineNbr, Equal<DRSchedule.lineNbr>>>>>, Where<DRSchedule.scheduleID, Equal<DRScheduleDetail.scheduleID>>>))]
		[PXSelector(typeof(Search<ContractTask.taskID, Where<ContractTask.contractID, Equal<Optional<ARTran.projectID>>>>), SubstituteKey = typeof(ContractTask.taskCD))]
		[PXUIField(DisplayName = "Task ID", Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual Int32? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion

		#region System Columns
		#region tstamp
		public abstract class Tstamp : PX.Data.IBqlField
		{
		}
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
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
		#region CreatedByID
		public abstract class createdByID : PX.Data.IBqlField
		{
		}
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
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
		public abstract class createdByScreenID : PX.Data.IBqlField
		{
		}
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
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
		public abstract class createdDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
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
		public abstract class lastModifiedByID : PX.Data.IBqlField
		{
		}
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
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
		public abstract class lastModifiedByScreenID : PX.Data.IBqlField
		{
		}
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
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
		public abstract class lastModifiedDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
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
		#endregion
	}

	public static class DRScheduleStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Open, Closed, Draft },
				new string[] { Messages.Open, Messages.Closed, Messages.Draft }) { ; }
		}
		public const string Open = "O";
		public const string Closed = "C";
		public const string Draft = "D";


		public class OpenStatus : Constant<string>
		{
			public OpenStatus() : base(DRScheduleStatus.Open) { ;}
		}
		public class ClosedStatus : Constant<string>
		{
			public ClosedStatus() : base(DRScheduleStatus.Closed) { ;}
		}
		public class DraftStatus : Constant<string>
		{
			public DraftStatus() : base(DRScheduleStatus.Draft) { ;}
		}
	}

	public static class DRScheduleDocumentType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Invoice, ARDocType.CreditMemo, ARDocType.DebitMemo, Bill, APDocType.CreditAdj, APDocType.DebitAdj },
				new string[] { AR.Messages.Invoice, AR.Messages.CreditMemo, AR.Messages.DebitMemo, AP.Messages.Invoice, AP.Messages.CreditAdj, AP.Messages.DebitAdj }) { ; }
		}
		public const string Invoice = "ARI";
		public const string Bill = "API";

		public static string ExtractModule(string documentType)
		{
			switch (documentType)
			{
				case Invoice:
				case ARDocType.CreditMemo:
				case ARDocType.DebitMemo:
					return BatchModule.AR;
				case Bill:
				case APDocType.CreditAdj:
				case APDocType.DebitAdj:
					return BatchModule.AP;
				default:
					throw new PXException("Invalid DocType");
			}
		}

		public static string ExtractDocType(string documentType)
		{
			switch (documentType)
			{
				case Invoice:
					return ARDocType.Invoice;
				case Bill:
					return APDocType.Invoice;
					
				default:
					return documentType;
			}
		}

		public static string BuildDocumentType(string module, string docType)
		{
			switch (docType)
			{
				case APDocType.Invoice:	//case ARDocType.Invoice:
					if (module == BatchModule.AR)
						return Invoice;
					else
						return Bill;
				default:
					return docType;
			}
		}
	}
}
