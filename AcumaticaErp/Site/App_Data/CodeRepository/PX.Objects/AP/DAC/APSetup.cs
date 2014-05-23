namespace PX.Objects.AP
{
	using System;
	using PX.Data;
	using PX.Objects.CS;
	using PX.Objects.EP.Standalone;
	
	[System.SerializableAttribute()]
    [PXPrimaryGraph(typeof(APSetupMaint))]
    [PXCacheName(Messages.APSetup)]
	public partial class APSetup : PX.Data.IBqlTable
	{
		#region BatchNumberingID
		public abstract class batchNumberingID : PX.Data.IBqlField
		{
		}
		protected String _BatchNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("BATCH")]
		[PXSelector(typeof(Numbering.numberingID))]
		[PXUIField(DisplayName = "Batch Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String BatchNumberingID
		{
			get
			{
				return this._BatchNumberingID;
			}
			set
			{
				this._BatchNumberingID = value;
			}
		}
		#endregion
		#region DfltVendorClassID
		public abstract class dfltVendorClassID : PX.Data.IBqlField
		{
		}
		protected String _DfltVendorClassID;
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Search2<VendorClass.vendorClassID, LeftJoin<EPEmployeeClass, On<EPEmployeeClass.vendorClassID, Equal<VendorClass.vendorClassID>>>, Where<EPEmployeeClass.vendorClassID, IsNull>>))]
		[PXUIField(DisplayName = "Default Vendor Class ID", Visibility=PXUIVisibility.Visible)]
		public virtual String DfltVendorClassID
		{
			get
			{
				return this._DfltVendorClassID;
			}
			set
			{
				this._DfltVendorClassID = value;
			}
		}
		#endregion
		#region PerRetainTran
		public abstract class perRetainTran : PX.Data.IBqlField
		{
		}
		protected Int16? _PerRetainTran;
		[PXDBShort()]
		[PXDefault((short)99)]
		[PXUIField(DisplayName = "Keep Transactions for", Visibility = PXUIVisibility.Visible)]
		public virtual Int16? PerRetainTran
		{
			get
			{
				return this._PerRetainTran;
			}
			set
			{
				this._PerRetainTran = value;
			}
		}
		#endregion
		#region PerRetainHist
		public abstract class perRetainHist : PX.Data.IBqlField
		{
		}
		protected Int16? _PerRetainHist;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Periods to Retain History", Visibility = PXUIVisibility.Invisible)]
		public virtual Int16? PerRetainHist
		{
			get
			{
				return this._PerRetainHist;
			}
			set
			{
				this._PerRetainHist = value;
			}
		}
		#endregion
		#region InvoiceNumberingID
		public abstract class invoiceNumberingID : PX.Data.IBqlField
		{
		}
		protected String _InvoiceNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("APBILL")]
		[PXSelector(typeof(Numbering.numberingID))]
		[PXUIField(DisplayName = "Bill Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String InvoiceNumberingID
		{
			get
			{
				return this._InvoiceNumberingID;
			}
			set
			{
				this._InvoiceNumberingID = value;
			}
		}
		#endregion
		#region PastDue00
		public abstract class pastDue00 : PX.Data.IBqlField
		{
		}
		protected Int16? _PastDue00;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Aging Period 1", Visibility = PXUIVisibility.Visible)]
		public virtual Int16? PastDue00
		{
			get
			{
				return this._PastDue00;
			}
			set
			{
				this._PastDue00 = value;
			}
		}
		#endregion
		#region PastDue01
		public abstract class pastDue01 : PX.Data.IBqlField
		{
		}
		protected Int16? _PastDue01;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Aging Period 2", Visibility = PXUIVisibility.Visible)]
		public virtual Int16? PastDue01
		{
			get
			{
				return this._PastDue01;
			}
			set
			{
				this._PastDue01 = value;
			}
		}
		#endregion
		#region PastDue02
		public abstract class pastDue02 : PX.Data.IBqlField
		{
		}
		protected Int16? _PastDue02;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Aging Period 3", Visibility = PXUIVisibility.Visible)]
		public virtual Int16? PastDue02
		{
			get
			{
				return this._PastDue02;
			}
			set
			{
				this._PastDue02 = value;
			}
		}
		#endregion
		#region CheckNumberingID
		public abstract class checkNumberingID : PX.Data.IBqlField
		{
		}
		protected String _CheckNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("APPAYMENT")]
		[PXSelector(typeof(Numbering.numberingID))]
		[PXUIField(DisplayName = "Payment Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String CheckNumberingID
		{
			get
			{
				return this._CheckNumberingID;
			}
			set
			{
				this._CheckNumberingID = value;
			}
		}
		#endregion
        #region CreditAdjNumberingID
        public abstract class creditAdjNumberingID : PX.Data.IBqlField
        {
        }
        protected String _CreditAdjNumberingID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault("APBILL")]
        [PXSelector(typeof(Numbering.numberingID))]
        [PXUIField(DisplayName = "Credit Adjustment Numbering Sequence", Visibility = PXUIVisibility.Visible)]
        public virtual String CreditAdjNumberingID
        {
            get
            {
                return this._CreditAdjNumberingID;
            }
            set
            {
                this._CreditAdjNumberingID = value;
            }
        }
        #endregion
        #region DebitAdjNumberingID
        public abstract class debitAdjNumberingID : PX.Data.IBqlField
        {
        }
        protected String _DebitAdjNumberingID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault("APBILL")]
        [PXSelector(typeof(Numbering.numberingID))]
        [PXUIField(DisplayName = "Debit Adjustment Numbering Sequence", Visibility = PXUIVisibility.Visible)]
        public virtual String DebitAdjNumberingID
        {
            get
            {
                return this._DebitAdjNumberingID;
            }
            set
            {
                this._DebitAdjNumberingID = value;
            }
        }
        #endregion
		#region DefaultTranDesc
		public abstract class defaultTranDesc : PX.Data.IBqlField
		{
		}
		protected String _DefaultTranDesc;
		[PXDBString(1, IsFixed = true)]
		[PXDefault("C")]
		[PXStringList(new string[] {"C", "I", "N", "U"}, new string[] {"Combination ID and Name", "Vendor ID", "Vendor Name", "User Entered Description"})]
		[PXUIField(DisplayName = "Default Transaction Description", Visibility = PXUIVisibility.Invisible)]
		public virtual String DefaultTranDesc
		{
			get
			{
				return this._DefaultTranDesc;
			}
			set
			{
				this._DefaultTranDesc = value;
			}
		}
		#endregion
		#region ExpenseSubMask
		public abstract class expenseSubMask : PX.Data.IBqlField
		{
		}
		protected String _ExpenseSubMask;
		[PXDefault()]
		[SubAccountMask(DisplayName = "Combine Expense Sub. from")]
		public virtual String ExpenseSubMask
		{
			get
			{
				return this._ExpenseSubMask;
			}
			set
			{
				this._ExpenseSubMask = value;
			}
		}
		#endregion
		#region AutoPost
		public abstract class autoPost : PX.Data.IBqlField
		{
		}
		protected bool? _AutoPost;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Automatically Post on Release", Visibility = PXUIVisibility.Visible)]
		public virtual bool? AutoPost
		{
			get
			{
				return this._AutoPost;
			}
			set
			{
				this._AutoPost = value;
			}
		}
		#endregion
		#region TransactionPosting
		public abstract class transactionPosting : PX.Data.IBqlField
		{
		}
		protected String _TransactionPosting;
		[PXDBString(1, IsFixed = true)]
		[PXDefault("D")]
		[PXUIField(DisplayName = "Transaction Posting", Visibility = PXUIVisibility.Invisible)]
		[PXStringList(new string[] {"S","D"}, new string[] {"Summary", "Detail"})]
		public virtual String TransactionPosting
		{
			get
			{
				return this._TransactionPosting;
			}
			set
			{
				this._TransactionPosting = value;
			}
		}
		#endregion
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
		#region SummaryPost
		public abstract class summaryPost : PX.Data.IBqlField
		{
		}
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Post Summary on Updating GL", Visibility = PXUIVisibility.Visible)]
		public virtual bool? SummaryPost
		{
			get
			{
				return (this._TransactionPosting == "S");
			}
			set
			{
				this._TransactionPosting = (value == true) ? "S" : "D";
			}
		}
		#endregion
		#region RequireApprovePayments
		public abstract class requireApprovePayments : PX.Data.IBqlField
		{
		}
		protected bool? _RequireApprovePayments;
		[PXDBBool()]
		[PXDefault(true)]
        [PXUIField(DisplayName = "Require Approval of Bills Prior to Payment", Visibility = PXUIVisibility.Visible)]
		public virtual bool? RequireApprovePayments
		{
			get
			{
				return this._RequireApprovePayments;
			}
			set
			{
				this._RequireApprovePayments = value;
			}
		}
		#endregion
		#region RequireControlTotal
		public abstract class requireControlTotal : PX.Data.IBqlField
		{
		}
		protected Boolean? _RequireControlTotal;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Validate Document Totals on Entry")]
		public virtual Boolean? RequireControlTotal
		{
			get
			{
				return this._RequireControlTotal;
			}
			set
			{
				this._RequireControlTotal = value;
			}
		}
		#endregion
		#region HoldEntry
		public abstract class holdEntry : PX.Data.IBqlField
		{
		}
		protected Boolean? _HoldEntry;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Hold Documents on Entry")]
		public virtual Boolean? HoldEntry
		{
			get
			{
				return this._HoldEntry;
			}
			set
			{
				this._HoldEntry = value;
			}
		}
		#endregion
		#region EarlyChecks
		public abstract class earlyChecks : PX.Data.IBqlField
		{
		}
		protected Boolean? _EarlyChecks;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Enable Early Checks")]
		public virtual Boolean? EarlyChecks
		{
			get
			{
				return this._EarlyChecks;
			}
			set
			{
				this._EarlyChecks = value;
			}
		}
		#endregion
		#region RequireVendorRef
		public abstract class requireVendorRef : PX.Data.IBqlField
		{
		}
		protected Boolean? _RequireVendorRef;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Require Vendor Reference")]
		public virtual Boolean? RequireVendorRef
		{
			get
			{
				return this._RequireVendorRef;
			}
			set
			{
				this._RequireVendorRef = value;
			}
		}
		#endregion
		#region PaymentLeadTime
		public abstract class paymentLeadTime : PX.Data.IBqlField
		{
		}
		protected Int16? _PaymentLeadTime;
		[PXDBShort()]
		[PXDefault((short)7)]
		[PXUIField(DisplayName = "Payment Lead Time")]
		public virtual Int16? PaymentLeadTime
		{
			get
			{
				return this._PaymentLeadTime;
			}
			set
			{
				this._PaymentLeadTime = value;
			}
		}
		#endregion
        #region InvoicePrecision
        public abstract class invoicePrecision : PX.Data.IBqlField
        {
        }
        protected decimal? _InvoicePrecision;
        [PXDBDecimalString(2)]
        [PXDecimalList(new string[] { "0.05","0.1", "0.5", "1.0", "10", "100" }, new string[] {"0.05", "0.1", "0.5", "1.0", "10", "100" })]
        [PXDefault(TypeCode.Decimal, "0.1")]
        [PXUIField(DisplayName = "Rounding Precision")]
        public virtual decimal? InvoicePrecision
        {
            get
            {
                return this._InvoicePrecision;
            }
            set
            {
                this._InvoicePrecision = value;
            }
        }
        #endregion
        #region InvoiceRounding
        public abstract class invoiceRounding : PX.Data.IBqlField
        {
        }
        protected String _InvoiceRounding;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(RoundingType.Currency)]
        [PXUIField(DisplayName = "Rounding Rule for Bills")]
        [PXStringList(new string[] { RoundingType.Currency, RoundingType.Mathematical, RoundingType.Ceil, RoundingType.Floor }, new string[] { "Use Currency Precision", "Nearest", "Up", "Down" })]
        public virtual String InvoiceRounding
        {
            get
            {
                return this._InvoiceRounding;
            }
            set
            {
                this._InvoiceRounding = value;
            }
        }
        #endregion
        #region RaiseErrorOnDoubleInvoiceNbr
        public abstract class raiseErrorOnDoubleInvoiceNbr : IBqlField{}
        [PXDBBool]
        [PXDefault(false)]
		[PXUIField(DisplayName = "Raise an Error on Duplicate Vendor Reference Number")]
        public virtual Boolean? RaiseErrorOnDoubleInvoiceNbr { get; set; }
        #endregion

	}
}
