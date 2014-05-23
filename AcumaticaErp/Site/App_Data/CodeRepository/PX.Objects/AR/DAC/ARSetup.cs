namespace PX.Objects.AR
{
	using System;
	using PX.Data;
	using PX.Objects.CS;
    using System.Globalization;

	[System.SerializableAttribute()]
    [PXPrimaryGraph(typeof(ARSetupMaint))]
    [PXCacheName(Messages.ARSetup)]
    public partial class ARSetup : PX.Data.IBqlTable
	{
		#region BatchNumberingID
		public abstract class batchNumberingID : PX.Data.IBqlField
		{
		}
		protected String _BatchNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("BATCH")]
		[PXSelector(typeof(Numbering.numberingID))]
		[PXUIField(DisplayName = "GL Batch Numbering Sequence", Visibility = PXUIVisibility.Visible)]
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
		#region DfltCustomerClassID
		public abstract class dfltCustomerClassID : PX.Data.IBqlField
		{
		}
		protected String _DfltCustomerClassID;
		[PXDBString(10, IsUnicode = true)]
		//[PXDefault("DEFAULT")]
		[PXUIField(DisplayName = "Default Customer Class ID", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(CustomerClass.customerClassID), CacheGlobal = true)]
		public virtual String DfltCustomerClassID
		{
			get
			{
				return this._DfltCustomerClassID;
			}
			set
			{
				this._DfltCustomerClassID = value;
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
		[PXDefault("ARINVOICE")]
		[PXSelector(typeof(Numbering.numberingID))]
		[PXUIField(DisplayName = "Invoice Numbering Sequence", Visibility = PXUIVisibility.Visible)]
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
		#region PaymentNumberingID
		public abstract class paymentNumberingID : PX.Data.IBqlField
		{
		}
		protected String _PaymentNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("ARPAYMENT")]
		[PXSelector(typeof(Numbering.numberingID))]
		[PXUIField(DisplayName = "Payment Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String PaymentNumberingID
		{
			get
			{
				return this._PaymentNumberingID;
			}
			set
			{
				this._PaymentNumberingID = value;
			}
		}
		#endregion
		#region CreditAdjNumberingID
		public abstract class creditAdjNumberingID : PX.Data.IBqlField
		{
		}
		protected String _CreditAdjNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("ARINVOICE")]
		[PXSelector(typeof(Numbering.numberingID))]
		[PXUIField(DisplayName = "Credit Adj. Numbering Sequence", Visibility = PXUIVisibility.Visible)]
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
		[PXDefault("ARINVOICE")]
		[PXSelector(typeof(Numbering.numberingID))]
		[PXUIField(DisplayName = "Debit Adj. Numbering Sequence", Visibility = PXUIVisibility.Visible)]
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
		#region WriteOffNumberingID
		public abstract class writeOffNumberingID : PX.Data.IBqlField
		{
		}
		protected String _WriteOffNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("ARINVOICE")]
		[PXSelector(typeof(Numbering.numberingID))]
		[PXUIField(DisplayName = "Write Off Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String WriteOffNumberingID
		{
			get
			{
				return this._WriteOffNumberingID;
			}
			set
			{
				this._WriteOffNumberingID = value;
			}
		}
		#endregion
		#region UsageNumberingID
		public abstract class usageNumberingID : PX.Data.IBqlField
		{
		}
		[PXDefault("PMTRAN")]
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Numbering.numberingID))]
		[PXUIField(DisplayName = "Usage Transaction Numbering Sequence")]
		public virtual String UsageNumberingID
		{
			get;
			set;
		}
		#endregion
		#region DefaultTranDesc
		public abstract class defaultTranDesc : PX.Data.IBqlField
		{
		}
		protected String _DefaultTranDesc;
		[PXDBString(1, IsFixed = true)]
		[PXDefault("C")]
		[PXStringList(new string[] { "C", "I", "N", "U" }, new string[] { "Combination ID and Name", "Vendor ID", "Vendor Name", "User Entered Description" })]
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
		#region SalesSubMask
		public abstract class salesSubMask : PX.Data.IBqlField
		{
		}
		protected String _SalesSubMask;
		[PXDefault()]
		[SubAccountMask(DisplayName = "Combine Sales Sub. from")]
		public virtual String SalesSubMask
		{
			get
			{
				return this._SalesSubMask;
			}
			set
			{
				this._SalesSubMask = value;
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
		[PXStringList(new string[] { "S", "D" }, new string[] { "Summary", "Detail" })]
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
		#region FinChargeNumberingID
		public abstract class finChargeNumberingID : PX.Data.IBqlField
		{
		}
		protected String _FinChargeNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("ARINVOICE")]
		[PXSelector(typeof(Numbering.numberingID))]
		[PXUIField(DisplayName = "Overdue Charges Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String FinChargeNumberingID
		{
			get
			{
				return this._FinChargeNumberingID;
			}
			set
			{
				this._FinChargeNumberingID = value;
			}
		}
		#endregion
		#region FinChargeOnCharge
		public abstract class finChargeOnCharge : PX.Data.IBqlField
		{
		}
		protected Boolean? _FinChargeOnCharge;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Calculate on Overdue Charges Documents", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? FinChargeOnCharge
		{
			get
			{
				return this._FinChargeOnCharge;
			}
			set
			{
				this._FinChargeOnCharge = value;
			}
		}
		#endregion
		#region AgeCredits
		public abstract class ageCredits : PX.Data.IBqlField
		{
		}
		protected Boolean? _AgeCredits;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Age Credits", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? AgeCredits
		{
			get
			{
				return this._AgeCredits;
			}
			set
			{
				this._AgeCredits = value;
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
		#region ZeroPost
		public abstract class zeroPost : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Post All Transactions on Updating GL")]
		public virtual bool? ZeroPost
		{
			get;
			set;
		}
		#endregion
		#region AutoApplyPayments
		public abstract class autoApplyPayments : PX.Data.IBqlField
		{
		}
		protected Boolean? _AutoApplyPayments;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Auto-Apply Payments")]
		public virtual Boolean? AutoApplyPayments
		{
			get
			{
				return this._AutoApplyPayments;
			}
			set
			{
				this._AutoApplyPayments = value;
			}
		}
		#endregion

		#region AdjustContractStartDate
		public abstract class adjustContractStartDate : PX.Data.IBqlField
		{
		}
		protected Boolean? _AdjustContractStartDate;
		[Obsolete()]
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Adjust Contract Start Date on Activation")]
		public virtual Boolean? AdjustContractStartDate
		{
			get
			{
				return this._AdjustContractStartDate;
			}
			set
			{
				this._AdjustContractStartDate = value;
			}
		}
		#endregion

	
		#region CreditCheckError
		public abstract class creditCheckError : PX.Data.IBqlField
		{
		}
		protected Boolean? _CreditCheckError;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Hold Document on Failed Credit Check", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? CreditCheckError
		{
			get
			{
				return this._CreditCheckError;
			}
			set
			{
				this._CreditCheckError = value;
			}
		}
		#endregion

		#region SPCommnCalcType
		public abstract class sPCommnCalcType : PX.Data.IBqlField
		{
		}
		protected String _SPCommnCalcType;
		[PXDBString(1)]
		[PXDefault(SPCommnCalcTypes.ByInvoice)]
		[PXUIField(DisplayName = "Salesperson Commission by", Visibility = PXUIVisibility.Visible)]
		[SPCommnCalcTypes.List()]
		public virtual String SPCommnCalcType
		{
			get
			{
				return this._SPCommnCalcType;
			}
			set
			{
				this._SPCommnCalcType = value;
			}
		}
		#endregion
		#region SPCommnPeriodType
		public abstract class sPCommnPeriodType : PX.Data.IBqlField
		{
		}
		protected String _SPCommnPeriodType;
		[PXDBString(1)]
		[PXDefault(SPCommnPeriodTypes.Monthly)]
		[PXUIField(DisplayName = "Commission Period Type")]
		[SPCommnPeriodTypes.List()]
		public virtual String SPCommnPeriodType
		{
			get
			{
				return this._SPCommnPeriodType;
			}
			set
			{
				this._SPCommnPeriodType = value;
			}
		}
		#endregion
		#region DefFinChargeFromCycle
		public abstract class defFinChargeFromCycle : PX.Data.IBqlField
		{
		}
		protected Boolean? _DefFinChargeFromCycle;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Set Default Overdue Charges by Statement Cycle")]
		public virtual Boolean? DefFinChargeFromCycle
		{
			get
			{
				return this._DefFinChargeFromCycle;
			}
			set
			{
				this._DefFinChargeFromCycle = value;
			}
		}
		#endregion
		#region FinChargeFirst
		public abstract class finChargeFirst : PX.Data.IBqlField
		{
		}
		protected Boolean? _FinChargeFirst;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Apply Payments to Overdue Charges First")]
		public virtual Boolean? FinChargeFirst
		{
			get
			{
				return this._FinChargeFirst;
			}
			set
			{
				this._FinChargeFirst = value;
			}
		}
		#endregion
		#region PrintBeforeRelease
		public abstract class printBeforeRelease : PX.Data.IBqlField
		{
		}
		protected Boolean? _PrintBeforeRelease;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Invoice/Memo Printing Before Release")]
		public virtual Boolean? PrintBeforeRelease
		{
			get
			{
				return this._PrintBeforeRelease;
			}
			set
			{
				this._PrintBeforeRelease = value;
			}
		}
		#endregion
		#region EmailBeforeRelease
		public abstract class emailBeforeRelease : PX.Data.IBqlField
		{
		}
		protected Boolean? _EmailBeforeRelease;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Invoice/Memo Emailing Before Release")]
		public virtual Boolean? EmailBeforeRelease
		{
			get
			{
				return this._EmailBeforeRelease;
			}
			set
			{
				this._EmailBeforeRelease = value;
			}
		}
		#endregion

		#region IntegratedCCProcessing
		public abstract class integratedCCProcessing : PX.Data.IBqlField
		{
		}
		protected Boolean? _IntegratedCCProcessing;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Integrated CC Processing", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? IntegratedCCProcessing
		{
			get
			{
				return this._IntegratedCCProcessing;
			}
			set
			{
				this._IntegratedCCProcessing = value;
			}
		}
		#endregion

		#region PaymentAutoProcessingTermsID
		public abstract class paymentAutoProcessingTermsID : PX.Data.IBqlField
		{
		}
		protected String _PaymentAutoProcessingTermsID;
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Terms to Process", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.customer>, Or<Terms.visibleTo,Equal<TermsVisibleTo.all>>>>), DescriptionField = typeof(Terms.descr), CacheGlobal = true)]
		public virtual String PaymentAutoProcessingTermsID
		{
			get
			{
				return this._PaymentAutoProcessingTermsID;
			}
			set
			{
				this._PaymentAutoProcessingTermsID = value;
			}
		}
		#endregion

        #region ConsolidatedStatement
		public abstract class consolidatedStatement : PX.Data.IBqlField
		{
		}
		protected Boolean? _ConsolidatedStatement;
		[PXDBBool()]
		[PXDefault(false)]
        [PXUIField(DisplayName = "Consolidate Statements for all Branches")]
		public virtual Boolean? ConsolidatedStatement
		{
			get
			{
				return this._ConsolidatedStatement;
			}
			set
			{
				this._ConsolidatedStatement = value;
			}
		}
		#endregion
        #region StatementBranchID
		public abstract class statementBranchID : PX.Data.IBqlField
		{
		}
		protected Int32? _StatementBranchID;
		[GL.Branch(DisplayName="Statement from Branch", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? StatementBranchID
		{
			get
			{
				return this._StatementBranchID;
			}
			set
			{
				this._StatementBranchID = value;
			}
		}
		#endregion
        #region ConsolidatedDunningLetter
		public abstract class consolidatedDunningLetter : PX.Data.IBqlField
		{
		}
		protected Boolean? _ConsolidatedDunningLetter;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Consolidate Dunning Letters for all Branches")]
		public virtual Boolean? ConsolidatedDunningLetter
		{
			get
			{
				return this._ConsolidatedDunningLetter;
			}
			set
			{
				this._ConsolidatedDunningLetter = value;
			}
		}
		#endregion
        #region DunningLetterBranchID
		public abstract class dunningLetterBranchID : PX.Data.IBqlField
		{
		}
		protected Int32? _DunningLetterBranchID;
		[GL.Branch(DisplayName ="Dunning Letter from Branch", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? DunningLetterBranchID
		{
			get
			{
				return this._DunningLetterBranchID;
			}
			set
			{
				this._DunningLetterBranchID = value;
			}
		}
		#endregion

        #region InvoicePrecision
        public abstract class invoicePrecision : PX.Data.IBqlField
        {
        }
        protected decimal? _InvoicePrecision;
        [PXDBDecimalString(2)]
        [PXDecimalList(new string[] { "0.05", "0.1", "0.5", "1.0", "10", "100" }, new string[] { "0.05", "0.1", "0.5", "1.0", "10", "100" })]
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
        [PXUIField(DisplayName = "Rounding Rule for Invoices")]
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

		#region BalanceWriteOff
		public abstract class balanceWriteOff : PX.Data.IBqlField
		{
		}
		protected String _BalanceWriteOff;
		[PXDBString(ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where<ReasonCode.usage, Equal<ReasonCodeUsages.balanceWriteOff>>>))]
		[PXUIField(DisplayName = "Balance Write-Off Reason Code", Visibility = PXUIVisibility.Visible)]
		public virtual String BalanceWriteOff
		{
			get
			{
				return this._BalanceWriteOff;
			}
			set
			{
				this._BalanceWriteOff = value;
			}
		}
		#endregion
		#region CreditWriteOff
		public abstract class creditWriteOff : PX.Data.IBqlField
		{
		}
		protected String _CreditWriteOff;
		[PXDBString(ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where<ReasonCode.usage, Equal<ReasonCodeUsages.creditWriteOff>>>))]
		[PXUIField(DisplayName = "Credit Write-Off Reason Code", Visibility = PXUIVisibility.Visible)]
		public virtual String CreditWriteOff
		{
			get
			{
				return this._CreditWriteOff;
			}
			set
			{
				this._CreditWriteOff = value;
			}
		}
		#endregion

        #region DefaultRateTypeID
        public abstract class defaultRateTypeID : PX.Data.IBqlField
        {
        }
        protected String _DefaultRateTypeID;
        [PXDBString(6, IsUnicode = true)]
        [PXSelector(typeof(PX.Objects.CM.CurrencyRateType.curyRateTypeID))]
        [PXUIField(DisplayName = "Default Rate Type")]
        public virtual String DefaultRateTypeID
        {
            get
            {
                return this._DefaultRateTypeID;
            }
            set
            {
                this._DefaultRateTypeID = value;
            }
        }
        #endregion
        #region AlwaysFromBaseCury
        public abstract class alwaysFromBaseCury : PX.Data.IBqlField
        {
	}
        protected Boolean? _AlwaysFromBaseCury;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Always Calculate Price from Base Currency Price")]
        public virtual Boolean? AlwaysFromBaseCury
        {
            get
            {
                return this._AlwaysFromBaseCury;
            }
            set
            {
                this._AlwaysFromBaseCury = value;
            }
        }
        #endregion
        #region LineDiscountTarget
        public abstract class lineDiscountTarget : PX.Data.IBqlField
        {
        }
        protected String _LineDiscountTarget;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(LineDiscountTargetType.ExtendedPrice)]
        [LineDiscountTargetType.List()]
        [PXUIField(DisplayName = "Apply Line Discount to", Visibility = PXUIVisibility.Visible, Required = true)]
        public virtual String LineDiscountTarget
        {
            get
            {
                return this._LineDiscountTarget;
            }
            set
            {
                this._LineDiscountTarget = value;
            }
        }
        #endregion
        #region IgnoreDiscountsIfPriceDefined
        public abstract class ignoreDiscountsIfPriceDefined : PX.Data.IBqlField
        {
        }
        protected Boolean? _IgnoreDiscountsIfPriceDefined;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Ignore Configured Discounts When Customer-Specific Price is Defined")]
        public virtual Boolean? IgnoreDiscountsIfPriceDefined
        {
            get
            {
                return this._IgnoreDiscountsIfPriceDefined;
            }
            set
            {
                this._IgnoreDiscountsIfPriceDefined = value;
            }
        }
        #endregion
	}

	public class SPCommnCalcTypes
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { ByInvoice, ByPayment },
				new string[] { Messages.ByInvoice, Messages.ByPayment }) { }
		}

		public const string ByInvoice = "I";
		public const string ByPayment = "P";

		public class byInvoice : Constant<string>
		{
			public byInvoice() : base(ByInvoice) { ;}
		}

		public class byPayment : Constant<string>
		{
			public byPayment() : base(ByPayment) { ;}
		}
	}

	public class SPCommnPeriodTypes
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Monthly, Quarterly, Yearly, FiscalPeriod },
				new string[] { Messages.Monthly, Messages.Quarterly, Messages.Yearly, Messages.FiscalPeriod }) { }
		}

		public const string Monthly = "M";
		public const string Quarterly = "Q";
		public const string Yearly = "Y";
		public const string FiscalPeriod = "F";

		public class monthly : Constant<string>
		{
			public monthly() : base(Monthly) { ;}
		}
		public class quarterly : Constant<string>
		{
			public quarterly() : base(Quarterly) { ;}
		}
		public class yearly : Constant<string>
		{
			public yearly() : base(Yearly) { ;}
		}
		public class fiscalPeriod : Constant<string>
		{
			public fiscalPeriod() : base(FiscalPeriod) { ;}
		}
	}

    public static class LineDiscountTargetType
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { ExtendedPrice, SalesPrice },
                new string[] { Messages.ExtendedPrice, Messages.SalesPrice }) { ; }
        }
        public const string ExtendedPrice = "E";
        public const string SalesPrice = "S";
	}	
}
