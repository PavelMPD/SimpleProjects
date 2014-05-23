using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.TX;
using PX.Objects.CR;
using PX.TM;
using PX.SM;
using PX.Objects.EP;
using SOInvoice = PX.Objects.SO.SOInvoice;
using SOInvoiceEntry = PX.Objects.SO.SOInvoiceEntry;
using PX.Objects.PM;
using PX.Objects.CA;


namespace PX.Objects.AR
{
	public class ARInvoiceType : ARDocType
	{
        /// <summary>
        /// Specialized selector for ARInvoice RefNbr.<br/>
        /// By default, defines the following set of columns for the selector:<br/>
        /// ARInvoice.refNbr,ARInvoice.docDate, ARInvoice.finPeriodID,<br/>
        /// ARInvoice.customerID, ARInvoice.customerID_Customer_acctName,<br/>
        /// ARInvoice.customerLocationID, ARInvoice.curyID, ARInvoice.curyOrigDocAmt,<br/>
        /// ARInvoice.curyDocBal,ARInvoice.status, ARInvoice.dueDate, ARInvoice.invoiceNbr<br/> 
        /// </summary>
		public class RefNbrAttribute : PXSelectorAttribute
		{
            /// <summary>
            /// Ctor
            /// </summary>
            /// <param name="SearchType">Must be IBqlSearch, returning ARInvoice.refNbr</param>
			public RefNbrAttribute(Type SearchType)
				: base(SearchType,
                typeof(ARInvoice.refNbr),
                typeof(ARInvoice.invoiceNbr),
				typeof(ARInvoice.docDate),
				typeof(ARInvoice.finPeriodID),
				typeof(ARInvoice.customerID),
				typeof(ARInvoice.customerID_Customer_acctName),
				typeof(ARInvoice.customerLocationID),
				typeof(ARInvoice.curyID),
				typeof(ARInvoice.curyOrigDocAmt),
				typeof(ARInvoice.curyDocBal),
				typeof(ARInvoice.status),
				typeof(ARInvoice.dueDate))
			{
			}
		}

        /// <summary>
        /// Specialized selector for ARInvoice RefNbr.<br/>
        /// By default, defines the following set of columns for the selector:<br/>
        /// ARInvoice.refNbr,ARInvoice.docDate, ARInvoice.finPeriodID,<br/>
        /// ARInvoice.customerID, ARInvoice.customerID_Customer_acctName,<br/>
        /// ARInvoice.customerLocationID, ARInvoice.curyID, ARInvoice.curyOrigDocAmt,<br/>
        /// ARInvoice.curyDocBal,ARInvoice.status, ARInvoice.dueDate, ARInvoice.invoiceNbr<br/>
        /// </summary>		
		public class AdjdRefNbrAttribute : PXSelectorAttribute
		{
            /// <summary>
            /// Ctor
            /// </summary>
            /// <param name="SearchType">Must be IBqlSearch, returning ARInvoice.refNbr</param>
			public AdjdRefNbrAttribute(Type SearchType)
				: base(SearchType,
				typeof(ARRegister.refNbr),
                typeof(ARRegister.docDate),
                typeof(ARRegister.finPeriodID),
                typeof(ARRegister.customerLocationID),
                typeof(ARRegister.curyID),
                typeof(ARRegister.curyOrigDocAmt),
                typeof(ARRegister.curyDocBal),
                typeof(ARRegister.status),
				typeof(ARRegister.dueDate),
                typeof(ARAdjust.ARInvoice.invoiceNbr),
                typeof(ARRegister.docDesc))
			{
			}
		}

        /// <summary>
        /// Specialized for ARInvoices version of the <see cref="AutoNumberAttribute"/><br/>
        /// It defines how the new numbers are generated for the AR Invoice. <br/>
        /// References ARInvoice.docType and ARInvoice.docDate fields of the document,<br/>
        /// and also define a link between  numbering ID's defined in AR Setup and ARInvoice types:<br/>
        /// namely ARSetup.invoiceNumberingID - for ARInvoice, 
        /// ARSetup.adjustmentNumberingID - for ARDebitMemo and ARCreditMemo<br/>        
        /// ARSetup.finChargeNumberingID - for FinCharges <br/>
        /// ARSetup.paymentNumberingID - for CashSale and CashReturn <br/>        
        /// </summary>
		public class NumberingAttribute : AutoNumberAttribute
		{
			public NumberingAttribute()
				: base(typeof(ARInvoice.docType), typeof(ARInvoice.docDate),
					new string[] { Invoice, DebitMemo, CreditMemo, FinCharge, SmallCreditWO, CashSale, CashReturn },
					new Type[] { typeof(ARSetup.invoiceNumberingID), typeof(ARSetup.debitAdjNumberingID), typeof(ARSetup.creditAdjNumberingID), typeof(ARSetup.finChargeNumberingID), null, typeof(ARSetup.paymentNumberingID), typeof(ARSetup.paymentNumberingID) }) { ; }
		}

		public new class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Invoice, DebitMemo, CreditMemo, FinCharge, SmallCreditWO },
				new string[] { Messages.Invoice, Messages.DebitMemo, Messages.CreditMemo, Messages.FinCharge, Messages.SmallCreditWO }) { }
		}

		public class AdjdListAttribute : PXStringListAttribute
		{
			public AdjdListAttribute()
				: base(
				new string[] { Invoice, DebitMemo, FinCharge, SmallCreditWO },
				new string[] { Messages.Invoice, Messages.DebitMemo, Messages.FinCharge, Messages.SmallCreditWO }) { }
		}

		public static string DrCr(string DocType)
		{
			switch (DocType)
			{
				case Invoice:
				case DebitMemo:
				case FinCharge:
				case SmallCreditWO:
				case CashSale:
					return "C";
				case CreditMemo:
				case CashReturn:
					return "D";
				default:
					return null;
			}
		}
	}

    [Serializable]
	public partial class ARChildInvoice : ARInvoice
	{
		#region DocType
		public new abstract class docType : PX.Data.IBqlField
		{
		}
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.IBqlField
		{
		}
		#endregion
	}

	[System.SerializableAttribute()]
	[PXTable()]
	[PXSubstitute(GraphType = typeof(ARInvoiceEntry))]
    [CRCacheIndependentPrimaryGraphList(new Type[] {
		typeof(SO.SOInvoiceEntry),
		typeof(ARInvoiceEntry)
	},
        new Type[] {
		typeof(Select<ARInvoice, 
			Where<ARInvoice.docType, Equal<Current<ARInvoice.docType>>, 
				And<ARInvoice.refNbr, Equal<Current<ARInvoice.refNbr>>,
                And<ARInvoice.origModule, Equal<GL.BatchModule.moduleSO>,
				And<ARInvoice.released, Equal<boolFalse>>>>>>),
		typeof(Select<ARInvoice, 
			Where<ARInvoice.docType, Equal<Current<ARInvoice.docType>>, 
			And<ARInvoice.refNbr, Equal<Current<ARInvoice.refNbr>>>>>)
		})]
	[PXCacheName(Messages.ARInvoice)]
	[PXEMailSource]
	public partial class ARInvoice : ARRegister, IInvoice
	{
		#region Selected
		public new abstract class selected : IBqlField
		{
		}
		#endregion
		#region DocType
		public new abstract class docType : PX.Data.IBqlField
		{
		}
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[ARInvoiceType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
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
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[ARInvoiceType.RefNbr(typeof(Search2<ARInvoice.refNbr,
			InnerJoin<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>>,
			Where<ARInvoice.docType, Equal<Optional<ARInvoice.docType>>,
			And2<Where<ARInvoice.origModule, Equal<BatchModule.moduleAR>, Or<ARInvoice.released, Equal<True>>>,
			And<Match<Customer, Current<AccessInfo.userName>>>>>, OrderBy<Desc<ARInvoice.refNbr>>>), Filterable = true)]
		[ARInvoiceType.Numbering()]
		[ARInvoiceNbr()]
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
        #region OrigModule
        public new abstract class origModule : PX.Data.IBqlField
        {
        }
        #endregion
        #region CustomerID
        public new abstract class customerID : PX.Data.IBqlField
		{
		}
		[CustomerCredit(typeof(ARInvoice.hold), typeof(ARInvoice.released), Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), Filterable = true, TabOrder = 2)]
		[PXUIField(DisplayName = "Customer ID", TabOrder = 2)]
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
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.IBqlField
		{
		}
		[Branch(typeof(Coalesce<
			Search<Location.cBranchID, Where<Location.bAccountID, Equal<Current<ARRegister.customerID>>, And<Location.locationID, Equal<Current<ARRegister.customerLocationID>>>>>,
			Search<GL.Branch.branchID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>), IsDetail = false)]
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
		#region CuryID
		public new abstract class curyID : PX.Data.IBqlField
		{
		}
		#endregion
		#region BillAddressID
		public abstract class billAddressID : PX.Data.IBqlField
		{
		}
		protected Int32? _BillAddressID;
		[PXDBInt()]
		[ARAddress(typeof(Select2<Customer,
			InnerJoin<Location, On<Location.bAccountID, Equal<Customer.bAccountID>, And<Location.locationID, Equal<Customer.defLocationID>>>,
			InnerJoin<Address, On<Address.bAccountID, Equal<Customer.bAccountID>, And<Address.addressID, Equal<Customer.defBillAddressID>>>,
			LeftJoin<ARAddress, On<ARAddress.customerID, Equal<Address.bAccountID>, And<ARAddress.customerAddressID, Equal<Address.addressID>, And<ARAddress.revisionID, Equal<Address.revisionID>, And<ARAddress.isDefaultBillAddress, Equal<boolTrue>>>>>>>>,
			Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>>>))]
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
		[PXDBInt()]
		[ARContact(typeof(Select2<Customer,
			InnerJoin<Location, On<Location.bAccountID, Equal<Customer.bAccountID>, And<Location.locationID, Equal<Customer.defLocationID>>>,
			InnerJoin<Contact, On<Contact.bAccountID, Equal<Customer.bAccountID>, And<Contact.contactID, Equal<Customer.defBillContactID>>>,
			LeftJoin<ARContact, On<ARContact.customerID, Equal<Contact.bAccountID>, And<ARContact.customerContactID, Equal<Contact.contactID>, And<ARContact.revisionID, Equal<Contact.revisionID>, And<ARContact.isDefaultContact, Equal<boolTrue>>>>>>>>,
			Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>>>))]
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
		#region ARAccountID
		public new abstract class aRAccountID : PX.Data.IBqlField
		{
		}
		#endregion
		#region ARSubID
		public new abstract class aRSubID : PX.Data.IBqlField
		{
		}
		#endregion
		#region TermsID
		public abstract class termsID : PX.Data.IBqlField
		{
		}
		protected String _TermsID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<Customer.termsID, Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>, And<Current<ARInvoice.docType>, NotEqual<ARDocType.creditMemo>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, Or<Terms.visibleTo, Equal<TermsVisibleTo.customer>>>>), DescriptionField = typeof(Terms.descr), Filterable = true)]
		[Terms(typeof(ARInvoice.docDate), typeof(ARInvoice.dueDate), typeof(ARInvoice.discDate), typeof(ARInvoice.curyOrigDocAmt), typeof(ARInvoice.curyOrigDiscAmt))]
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
		#region DueDate
		public new abstract class dueDate : PX.Data.IBqlField
		{
		}
		[PXDBDate()]
		[PXUIField(DisplayName = "Due Date", Visibility = PXUIVisibility.SelectorVisible)]
		public override DateTime? DueDate
		{
			get
			{
				return this._DueDate;
			}
			set
			{
				this._DueDate = value;
			}
		}
		#endregion
		#region DiscDate
		public abstract class discDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _DiscDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Cash Discount Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DiscDate
		{
			get
			{
				return this._DiscDate;
			}
			set
			{
				this._DiscDate = value;
			}
		}
		#endregion
		#region InvoiceNbr
		public abstract class invoiceNbr : PX.Data.IBqlField
		{
		}
		protected String _InvoiceNbr;
		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "Customer Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
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
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = "Customer Ref. Date")]
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
		[PXDBString(10, IsUnicode = true)]
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
		#region AvalaraCustomerUsageType
		public abstract class avalaraCustomerUsageType : PX.Data.IBqlField
		{
		}
		protected String _AvalaraCustomerUsageType;
		[PXDefault(typeof(Search<Location.cAvalaraCustomerUsageType, Where<Location.locationID, Equal<Current<ARInvoice.customerLocationID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Customer Usage Type")]
		[TX.TXAvalaraCustomerUsageType.List]
		public virtual String AvalaraCustomerUsageType
		{
			get
			{
				return this._AvalaraCustomerUsageType;
			}
			set
			{
				this._AvalaraCustomerUsageType = value;
			}
		}
		#endregion
		#region DocDate
		public new abstract class docDate : PX.Data.IBqlField
		{
		}
		#endregion
		#region MasterRefNbr
		public abstract class masterRefNbr : PX.Data.IBqlField
		{
		}
		protected String _MasterRefNbr;
		[PXDBString(15, IsUnicode = true)]
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
		#region InstallmentCntr
		public abstract class installmentCntr : PX.Data.IBqlField
		{
		}
		protected short? _InstallmentCntr;
		[PXDBShort()]
		public virtual short? InstallmentCntr
		{
			get
			{
				return this._InstallmentCntr;
			}
			set
			{
				this._InstallmentCntr = value;
			}
		}
		#endregion
		#region InstallmentNbr
		public abstract class installmentNbr : PX.Data.IBqlField
		{
		}
		protected Int16? _InstallmentNbr;
		[PXDBShort()]
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
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.taxTotal))]
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
		[PXDBDecimal(4)]
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
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.lineTotal))]
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
		#region LineTotal
		public abstract class lineTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _LineTotal;
		[PXDBDecimal(4)]
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

        #region CuryVatExemptTotal
        public abstract class curyVatExemptTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryVatExemptTotal;
        [PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.vatExemptTotal))]
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

        #region VatExemptTaxTotal
        public abstract class vatExemptTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _VatExemptTotal;
        [PXDBDecimal(4)]
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
        [PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.vatTaxableTotal))]
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
        [PXDBDecimal(4)]
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


		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryOrigDocAmt
		public new abstract class curyOrigDocAmt : PX.Data.IBqlField
		{
		}
		#endregion
		#region OrigDocAmt
		public new abstract class origDocAmt : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryDocBal
		public new abstract class curyDocBal : PX.Data.IBqlField
		{
		}
		#endregion
		#region DocBal
		public new abstract class docBal : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryDiscBal
		public new abstract class curyDiscBal : PX.Data.IBqlField
		{
		}
		#endregion
		#region DiscBal
		public new abstract class discBal : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryOrigDiscAmt
		public new abstract class curyOrigDiscAmt : PX.Data.IBqlField
		{
		}
		#endregion
		#region OrigDiscAmt
		public new abstract class origDiscAmt : PX.Data.IBqlField
		{
		}
		#endregion
		#region DrCr
		public abstract class drCr : PX.Data.IBqlField
		{
		}
		protected string _DrCr;
		[PXString(1, IsFixed = true)]
		public virtual string DrCr
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return ARInvoiceType.DrCr(this._DocType);
			}
			set
			{
			}
		}
		#endregion

        #region DiscTot
        public new abstract class discTot : PX.Data.IBqlField
        {
        }
        #endregion
        #region CuryDiscTot
        public new abstract class curyDiscTot : PX.Data.IBqlField
        {
        }
        #endregion

		#region Released
		public new abstract class released : PX.Data.IBqlField
		{
		}
		#endregion
		#region OpenDoc
		public new abstract class openDoc : PX.Data.IBqlField
		{
		}
		#endregion
		#region Hold
		public new abstract class hold : PX.Data.IBqlField
		{
		}
		#endregion
		#region BatchNbr
		public new abstract class batchNbr : PX.Data.IBqlField
		{
		}
		#endregion
		#region CommnPct
		public abstract class commnPct : PX.Data.IBqlField
		{
		}
		protected Decimal? _CommnPct;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Commission %", Enabled = false)]
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
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.commnAmt))]
		//[PXFormula(typeof(Mult<ARInvoice.curyCommnblAmt, Div<ARInvoice.commnPct, decimal100>>))]
		[PXUIField(DisplayName = "Commission Amt.", Enabled = false)]
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
		[PXDBDecimal(4)]
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
		#region LastFinChargeDate
		public abstract class lastFinChargeDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _LastFinChargeDate;
		[PXDate()]
		[PXDBScalar(typeof(Search<ARFinChargeTran.docDate,
		 Where<ARFinChargeTran.origDocType, Equal<ARInvoice.docType>,
			And<ARFinChargeTran.origRefNbr, Equal<ARInvoice.refNbr>>>,
			OrderBy<Desc<ARFinChargeTran.docDate>>>))]
		[PXUIField(DisplayName = "Last Fin. Charge Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? LastFinChargeDate
		{
			get
			{
				return this._LastFinChargeDate;
			}
			set
			{
				this._LastFinChargeDate = value;
			}
		}
		#endregion
		#region CuryCommnblAmt
		public abstract class curyCommnblAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryCommnblAmt;
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.commnblAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Commissionable", Enabled = false)]
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
		[PXDBDecimal(4)]
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
		#region CuryWhTaxBal
		public abstract class curyWhTaxBal : PX.Data.IBqlField
		{
		}
		[PXDecimal(4)]
		[PXFormula(typeof(decimal0))]
		public virtual Decimal? CuryWhTaxBal
		{
			get;
			set;
		}

		#endregion
		#region WhTaxBal
		public abstract class whTaxBal : PX.Data.IBqlField
		{
		}
		[PXDecimal(4)]
		[PXFormula(typeof(decimal0))]
		public virtual Decimal? WhTaxBal
		{
			get;
			set;
		}
		#endregion
		#region ScheduleID
		public new abstract class scheduleID : IBqlField
		{
		}
		#endregion
		#region Scheduled
		public new abstract class scheduled : IBqlField
		{
		}
		#endregion
		#region CreatedByID
		public new abstract class createdByID : PX.Data.IBqlField
		{
		}
		#endregion
		#region LastModifiedByID
		public new abstract class lastModifiedByID : PX.Data.IBqlField
		{
		}
		#endregion
		#region DontPrint
		public abstract class dontPrint : PX.Data.IBqlField
		{
		}
		protected Boolean? _DontPrint;
		[PXDBBool()]
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
		#region Printed
		public abstract class printed : PX.Data.IBqlField
		{
		}
		protected Boolean? _Printed;
		[PXDBBool()]
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
		#region DontEmail
		public abstract class dontEmail : PX.Data.IBqlField
		{
		}
		protected Boolean? _DontEmail;
		[PXDBBool()]
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
		#region Emailed
		public abstract class emailed : PX.Data.IBqlField
		{
		}
		protected Boolean? _Emailed;
		[PXDBBool()]
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
		#region Voided
		public new abstract class voided : PX.Data.IBqlField
		{
		}
		#endregion
		#region PrintInvoice
		public abstract class printInvoice : PX.Data.IBqlField
		{
		}
        [PXBool()]
        [PXDBCalced(typeof(Switch<Case<Where<dontPrint, Equal<False>, And<printed, Equal<False>>>, True>, False>), typeof(Boolean))]
        public virtual Boolean? PrintInvoice
		{
			[PXDependsOnFields(typeof(dontPrint),typeof(printed))]
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
        [PXDBCalced(typeof(Switch<Case<Where<dontEmail, Equal<False>, And<emailed, Equal<False>>>, True>, False>), typeof(Boolean))]
        public virtual Boolean? EmailInvoice
		{
			[PXDependsOnFields(typeof(dontEmail),typeof(emailed))]
			get
			{
				return (this._DontEmail != true && (this._Emailed == null || (bool)this._Emailed == false));
			}			
		}
		#endregion
		#region CreditHold
		public abstract class creditHold : PX.Data.IBqlField
		{
		}
		protected Boolean? _CreditHold;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Credit Hold")]
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
		[PXDBBool()]
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
		[PXDBDecimal(4)]
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

        #region SkipDiscounts
        public abstract class skipDiscounts : IBqlField
        {
        }
        protected bool? _SkipDiscounts = false;
        [PXDBBool]
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
		/*
		#region PrintedFlag
		public abstract class printedFlag : PX.Data.IBqlField
		{
		}
		protected short? _PrintedFlag;
		[PXDBCalced(typeof(Switch<Case<Where<ARInvoice.printed, Equal<boolTrue>>, boolTrue>, boolFalse>), typeof(short))]
		public virtual short? PrintedFlag
		{
			get
			{
				return this._PrintedFlag;
			}
			set
			{
				this._PrintedFlag = value;
			}
		}
		#endregion
		#region EmailedFlag
		public abstract class emailedFlag : PX.Data.IBqlField
		{
		}
		protected short? _EmailedFlag;
		[PXDBCalced(typeof(Switch<Case<Where<ARInvoice.emailed, Equal<boolTrue>>, boolTrue>, boolFalse>), typeof(short))]
		public virtual short? EmailedFlag
		{
			get
			{
				return this._EmailedFlag;
			}
			set
			{
				this._EmailedFlag = value;
			}
		}
		#endregion
		*/
		#region ProjectID
		public abstract class projectID : PX.Data.IBqlField
		{
		}
		protected Int32? _ProjectID;
		[ProjectDefault(BatchModule.AR, typeof(Search<Location.cDefProjectID, Where<Location.bAccountID, Equal<Current<ARInvoice.customerID>>, And<Location.locationID, Equal<Current<ARInvoice.customerLocationID>>>>>))]
		[PM.ActiveProjectForModule(BatchModule.AR, typeof(ARInvoice.customerID), true, true)]
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
		#region CustomerID_Customer_acctName
		public abstract class customerID_Customer_acctName : PX.Data.IBqlField
		{
		}
		#endregion
        #region PaymentMethodID
        public abstract class paymentMethodID : PX.Data.IBqlField
        {
        }
        protected string _PaymentMethodID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(typeof(Coalesce<Search2<CustomerPaymentMethod.paymentMethodID,
											InnerJoin<Customer, On<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>,
												And<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>,
												And<CustomerPaymentMethod.isActive,Equal<True>>>>,
											InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>,
												And<PaymentMethod.useForAR,Equal<True>,
												And<PaymentMethod.isActive,Equal<True>>>>>>,
											Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>>>,
                                   Search2<Customer.defPaymentMethodID, InnerJoin<PaymentMethod,On<PaymentMethod.paymentMethodID,Equal<Customer.defPaymentMethodID>,
												And<PaymentMethod.useForAR,Equal<True>,
												And<PaymentMethod.isActive,Equal<True>>>>>,
                                         Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search5<PaymentMethod.paymentMethodID, LeftJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.paymentMethodID, Equal<PaymentMethod.paymentMethodID>,
                                    And<CustomerPaymentMethod.bAccountID, Equal<Current<ARInvoice.customerID>>>>>,
                                Where<PaymentMethod.isActive, Equal<boolTrue>,
                                And<PaymentMethod.useForAR, Equal<boolTrue>,
                                And<Where<PaymentMethod.aRIsOnePerCustomer, Equal<True>,
                                    Or<Where<CustomerPaymentMethod.pMInstanceID, IsNotNull>>>>>>, Aggregate<GroupBy<PaymentMethod.paymentMethodID>>>), DescriptionField = typeof(PaymentMethod.descr))]
        [PXUIFieldAttribute(DisplayName = "Payment Method")]        
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
		protected int? _PMInstanceID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Card/Account No")]
        [PXDefault(typeof(Coalesce<
                        Search2<Customer.defPMInstanceID, InnerJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>,
                                And<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>>>>,
                                Where<Customer.bAccountID, Equal<Current2<ARInvoice.customerID>>,
								And<CustomerPaymentMethod.isActive, Equal<True>,
                                And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<ARInvoice.paymentMethodID>>>>>>,
                        Search<CustomerPaymentMethod.pMInstanceID,
                                Where<CustomerPaymentMethod.bAccountID, Equal<Current2<ARInvoice.customerID>>,
                                    And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<ARInvoice.paymentMethodID>>,
									And<CustomerPaymentMethod.isActive, Equal<True>>>>,
								OrderBy<Desc<CustomerPaymentMethod.expirationDate,
								Desc<CustomerPaymentMethod.pMInstanceID>>>>>)
                        , PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search<CustomerPaymentMethod.pMInstanceID, Where<CustomerPaymentMethod.bAccountID, Equal<Current2<ARInvoice.customerID>>,
            And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<ARInvoice.paymentMethodID>>,
            And<Where<CustomerPaymentMethod.isActive, Equal<boolTrue>, Or<CustomerPaymentMethod.pMInstanceID,
                    Equal<Current<ARInvoice.pMInstanceID>>>>>>>>), DescriptionField = typeof(CustomerPaymentMethod.descr))]
    	public virtual int? PMInstanceID
		{
			get
			{
				return _PMInstanceID;
			}
			set
			{
				_PMInstanceID = value;
			}
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
										Where<CustomerPaymentMethod.bAccountID, Equal<Current<ARInvoice.customerID>>,
											And<CustomerPaymentMethod.pMInstanceID, Equal<Current2<ARInvoice.pMInstanceID>>>>>,
                            Search2<CashAccount.cashAccountID,
                                InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
                                    And<PaymentMethodAccount.useForAR, Equal<True>,
                                    And<PaymentMethodAccount.aRIsDefault, Equal<True>,
                                    And<PaymentMethodAccount.paymentMethodID, Equal<Current2<ARInvoice.paymentMethodID>>>>>>>,
                                    Where<CashAccount.branchID,Equal<Current<ARInvoice.branchID>>,
										And<Match<Current<AccessInfo.userName>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [CashAccount(typeof(ARInvoice.branchID), typeof(Search2<CashAccount.cashAccountID,
                InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
                    And<PaymentMethodAccount.paymentMethodID, Equal<Current<ARInvoice.paymentMethodID>>,
                    And<PaymentMethodAccount.useForAR, Equal<True>>>>>, Where<Match<Current<AccessInfo.userName>>>>), DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(CashAccount.descr))]
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
		#region IsCCPayment
		public abstract class isCCPayment : PX.Data.IBqlField
		{
		}

		protected bool? _IsCCPayment = false;
		[PXBool()]
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
		#region Status
		public new abstract class status : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault(ARDocStatus.Hold)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[ARDocStatus.List()]
		[SetStatus()]
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
		#region Methods
        /// <summary>
        /// This attribute is intended for the status syncronization in the ARInvoice<br/>
        /// Namely, it sets a corresponeded string to the Status field, depending <br/>
        /// upon Voided, Released, CreditHold, Hold, Sheduled,Released, OpenDoc, PrintInvoice,EmailInvoice<br/>
        /// of the ARInvoice<br/>
        /// [SetStatus()]
        /// </summary>
		protected class SetStatusAttribute : PXEventSubscriberAttribute, IPXRowSelectingSubscriber, IPXRowSelectedSubscriber, IPXRowInsertingSubscriber
		{
			protected class Definition : IPrefetchable
			{
				public Boolean? _PrintBeforeRelease;
				public Boolean? _EmailBeforeRelease;
				void IPrefetchable.Prefetch()
				{
					using (PXDataRecord rec =
						PXDatabase.SelectSingle<ARSetup>(
						new PXDataField("PrintBeforeRelease"),
						new PXDataField("EmailBeforeRelease")))
					{
						_PrintBeforeRelease = rec != null ? rec.GetBoolean(0) : false;
						_EmailBeforeRelease = rec != null ? rec.GetBoolean(1) : false;
					}
				}
			}

			protected Definition _Definition;

			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);
				_Definition = PXDatabase.GetSlot<Definition>(typeof(SetStatusAttribute).FullName, typeof(ARSetup));
				sender.Graph.FieldUpdating.AddHandler<ARInvoice.hold>((cache, e) => 
				{
                    PXBoolAttribute.ConvertValue(e);
					HoldUpdating(cache, e);
				});
				sender.Graph.FieldUpdating.AddHandler<ARInvoice.printInvoice>((cache, e) =>
				{
                    PXBoolAttribute.ConvertValue(e);
					PrintInvoiceUpdating(cache, e);
				});
				sender.Graph.FieldUpdating.AddHandler<ARInvoice.emailInvoice>((cache, e) =>
				{
                    PXBoolAttribute.ConvertValue(e);
					EmailInvoiceUpdating(cache, e);
				});
			}

			protected virtual void StatusSet(ARInvoice item, bool? HoldVal, bool? toPrint, bool? toEmail)
			{
				if (item.Voided != null && (bool)item.Voided)
				{
					item.StatusSet(ARDocStatus.Voided);
				}
				else if (item.CreditHold == true && item.IsCCPayment == false)
				{
					item.StatusSet(ARDocStatus.CreditHold);
				}
				else if (item.CreditHold == true && item.IsCCPayment == true)
				{
					item.StatusSet(ARDocStatus.CCHold);
				}
				else if (HoldVal != null && (bool)HoldVal)
				{
					item.StatusSet(ARDocStatus.Hold);
				}
				else if (item.Scheduled != null && (bool)item.Scheduled)
				{
					item.StatusSet(ARDocStatus.Scheduled);
				}
				else if (item.Released != null && !(bool)item.Released)
				{
					if ((item.DocType == ARDocType.Invoice || item.DocType == ARDocType.CreditMemo || item.DocType == ARDocType.DebitMemo))
					{
						if (_Definition != null && _Definition._PrintBeforeRelease == true && toPrint == true)
							item.StatusSet(ARDocStatus.PendingPrint);
						else if (_Definition != null && _Definition._EmailBeforeRelease == true && toEmail == true)
							item.StatusSet(ARDocStatus.PendingEmail);
						else
							item.StatusSet(ARDocStatus.Balanced);
					}
					else
						item.StatusSet(ARDocStatus.Balanced);
				}
				else if (item.OpenDoc != null && (bool)item.OpenDoc)
				{
					item.StatusSet(ARDocStatus.Open);
				}
				else if (item.OpenDoc != null)
				{
					item.StatusSet(ARDocStatus.Closed);
				}
			}

			public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
			{
				ARInvoice item = (ARInvoice)e.Row;
				if (item != null)
				{
				StatusSet(item, item.Hold, item.PrintInvoice, item.EmailInvoice);
			}
			}

			public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
			{
				ARInvoice item = (ARInvoice)e.Row;
				StatusSet(item, item.Hold, item.PrintInvoice, item.EmailInvoice);
			}

			public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				ARInvoice item = e.Row as ARInvoice;
				if (item != null)
				{
					StatusSet(item, item.Hold, item.PrintInvoice, item.EmailInvoice);
				}
			}

			protected virtual void HoldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
			{
				ARInvoice item = e.Row as ARInvoice;
				if (item != null)
				{
					StatusSet(item, (bool?)e.NewValue, item.PrintInvoice, item.EmailInvoice);
				}
			}

			protected virtual void PrintInvoiceUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
			{
				ARInvoice item = e.Row as ARInvoice;
				if (item != null)
				{
					StatusSet(item, item.Hold, (bool?)e.NewValue, item.EmailInvoice);
				}
			}

			protected virtual void EmailInvoiceUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
			{
				ARInvoice item = e.Row as ARInvoice;
				if (item != null)
				{
					StatusSet(item, item.Hold, item.PrintInvoice, (bool?)e.NewValue);
				}
			}
		}

		protected virtual void StatusSet(string Status)
		{
			this._Status = Status;
		}

		protected override void SetStatus()
		{
		}
		#endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.IBqlField
		{
		}
		protected int? _WorkgroupID;
		[PXDBInt]
		[PXDefault(typeof(Customer.workgroupID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCompanyTreeSelector]
		[PXUIField(DisplayName = "Workgroup", Visibility = PXUIVisibility.Visible)]
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
		[PXDBGuid()]
		[PXDefault(typeof(Customer.ownerID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXOwnerSelector(typeof(ARInvoice.workgroupID))]
		[PXUIField(DisplayName = "Owner", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region SalesPersonID
		public new abstract class salesPersonID : PX.Data.IBqlField
		{
		}
		#endregion
		#region NoteID
		public new abstract class noteID : PX.Data.IBqlField
		{
		}
		[PXNote(DescriptionField = typeof(ARInvoice.refNbr))]
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
		[PXDBLong()]
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
		#region Hidden
		public abstract class hidden : PX.Data.IBqlField
		{
		}
		protected Boolean? _Hidden = false;
		[PXBool()]
		public virtual Boolean? Hidden
		{
			get
			{
				return this._Hidden;
			}
			set
			{
				this._Hidden = value;
			}
		}
		#endregion
		#region HiddenOrderType
		public abstract class hiddenOrderType : PX.Data.IBqlField
		{
		}
		protected string _HiddenOrderType;
		[PXString()]
		public virtual string HiddenOrderType
		{
			get
			{
				return this._HiddenOrderType;
			}
			set
			{
				this._HiddenOrderType = value;
			}
		}
		#endregion		
		#region HiddenOrderNbr
		public abstract class hiddenOrderNbr : PX.Data.IBqlField
		{
		}
		protected string _HiddenOrderNbr;
		[PXString()]
		public virtual string HiddenOrderNbr
		{
			get
			{
				return this._HiddenOrderNbr;
			}
			set
			{
				this._HiddenOrderNbr = value;
			}
		}
		#endregion		
		#region IsTaxValid
		public new abstract class isTaxValid : PX.Data.IBqlField
		{
		}
		#endregion
		#region IsTaxPosted
		public new abstract class isTaxPosted : PX.Data.IBqlField
		{
		}
		#endregion
		#region IsTaxSaved
		public new abstract class isTaxSaved : PX.Data.IBqlField
		{
		}
		#endregion
		#region ApplyPaymentWhenTaxAvailable
		public abstract class applyPaymentWhenTaxAvailable : PX.Data.IBqlField
		{
		}
		[PXBool()]
		public virtual bool? ApplyPaymentWhenTaxAvailable { get; set; }

		#endregion

        #region IsRUTROTDeductible
        public abstract class isRUTROTDeductible : IBqlField { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "ROT and RUT deductible document", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual bool? IsRUTROTDeductible { get; set; }
        #endregion
        #region RUTROTType
        public abstract class rUTROTType : IBqlField { }

        [PXDBString(1)]
        [RUTROTTypes.List]
        [PXDefault(RUTROTTypes.RUT, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Deduction type", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual string RUTROTType { get; set; }
        #endregion

        #region CuryRUTROTPersonalAllowance
        public abstract class curyRUTROTPersonalAllowance : IBqlField { }

        [PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.rUTROTPersonalAllowance))]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Search<GL.Branch.rUTROTPersonalAllowanceLimit, Where<GL.Branch.branchID, Equal<Current<ARInvoice.branchID>>>>))]
        [PXUIField(DisplayName = "Personal Allowance", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual Decimal? CuryRUTROTPersonalAllowance { get; set; }
        #endregion

        #region RUTROTPersonalAllowance
        public abstract class rUTROTPersonalAllowance : IBqlField { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? RUTROTPersonalAllowance { get; set; }
        #endregion

        #region RUTROTDeductionPct
        public abstract class rUTROTDeductionPct : IBqlField { }

        [PXDBDecimal(2)]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Search<GL.Branch.rUTROTDeductionPct, Where<GL.Branch.branchID, Equal<Current<ARInvoice.branchID>>>>))]
        [PXUIField(DisplayName = "Deduction %", Visible = false, FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual Decimal? RUTROTDeductionPct { get; set; }
        #endregion

        #region RUTROTAutoDistribution
        public abstract class rUTROTAutoDistribution : IBqlField { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Distribute Automatically", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual bool? RUTROTAutoDistribution { get; set; }
        #endregion

        #region IsRUTROTClaimed
        public abstract class isRUTROTClaimed : IBqlField { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "ROT and RUT was claimed", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual bool? IsRUTROTClaimed { get; set; }
        #endregion

        #region CuryRUTROTAllowedAmt
        public abstract class curyRUTROTAllowedAmt : IBqlField { }

        [PXCurrency(typeof(ARInvoice.curyInfoID), typeof(rUTROTAllowedAmt))]
        [PXDefault(TypeCode.Decimal, "0.0",PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual decimal? CuryRUTROTAllowedAmt { get; set; }
        #endregion

        #region RUTROTAllowedAmt
        public abstract class rUTROTAllowedAmt : IBqlField { }

        [PXBaseCury]
        public virtual decimal? RUTROTAllowedAmt { get; set; }
        #endregion

        #region CuryRUTROTDistributedAmt
        public abstract class curyRUTROTDistributedAmt : IBqlField { }

        [PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(rUTROTDistributedAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Distributed Amount", FieldClass = AR.RUTROTMessages.FieldClass)]        
        public virtual decimal? CuryRUTROTDistributedAmt { get; set; }
        #endregion

        #region RUTROTDistributedAmt
        public abstract class rUTROTDistributedAmt : IBqlField { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? RUTROTDistributedAmt { get; set; }
        #endregion

        #region CuryRUTROTTotalAmt
        public abstract class curyRUTROTTotalAmt : IBqlField { }

        private decimal? _CuryRUTROTTotalAmt;

        [PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.rUTROTTotalAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Deductible Amount", FieldClass = AR.RUTROTMessages.FieldClass)]
        [PXFormula(typeof(Validate<ARInvoice.curyRUTROTAllowedAmt>))]
        public virtual Decimal? CuryRUTROTTotalAmt
        {
            get { return _CuryRUTROTTotalAmt; }
            set { _CuryRUTROTTotalAmt = value; }
        }
        #endregion

        #region RUTROTTotalAmt
        public abstract class rUTROTTotalAmt : IBqlField { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? RUTROTTotalAmt { get; set; }
        #endregion

        #region CuryRUTROTUndistributedAmt
        public abstract class curyRUTROTUndistributedAmt : IBqlField { }

        [PXCurrency(typeof(ARInvoice.curyInfoID), typeof(rUTROTUndistributedAmt))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(IsNull<Sub<ARInvoice.curyRUTROTTotalAmt, ARInvoice.curyRUTROTDistributedAmt>, decimal0>))]
        [PXUIField(DisplayName = "Undistributed Amount", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual decimal? CuryRUTROTUndistributedAmt { get; set; }
        #endregion

        #region RUTROTUndistributedAmt
        public abstract class rUTROTUndistributedAmt : IBqlField { }

        [PXBaseCury]
        public virtual decimal? RUTROTUndistributedAmt { get; set; }
        #endregion

        #region RUTROTDistributionLineCntr
        public abstract class rUTROTDistributionLineCntr : PX.Data.IBqlField
        {
        }

        [PXDBInt()]
        [PXDefault(0)]
        public virtual Int32? RUTROTDistributionLineCntr { get; set; }
        #endregion

        #region RUTROTClaimDate
        public abstract class rUTROTClaimDate : IBqlField { }

        [PXDBDate]
        [PXUIField(DisplayName = "Export Date", FieldClass = AR.RUTROTMessages.FieldClass)]
        public DateTime? RUTROTClaimDate { get; set; }
        #endregion

        #region RUTROTClaimFileName
        public abstract class rUTROTClaimFileName : IBqlField { }

        [PXDBString(40)]
        [PXUIField(DisplayName = "Export File", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual string RUTROTClaimFileName { get; set; }
        #endregion

        #region ROTEstateAppartment
        public abstract class rOTEstateAppartment : IBqlField { }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBString(40)]
        [PXUIField(DisplayName = "Real estate / appartment", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual string ROTEstateAppartment { get; set; }
        #endregion

        #region ROTOrganizationNbr
        public abstract class rOTOrganizationNbr : IBqlField { }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBString(20)]
        [PXUIField(DisplayName = "Organization nbr.", FieldClass = AR.RUTROTMessages.FieldClass)]
        [DynamicValueValidation(typeof(Search<GL.Branch.rUTROTOrgNbrValidRegEx,
                                            Where<Current<ARInvoice.isRUTROTDeductible>, Equal<boolTrue>,
                                            And<Current<ARInvoice.rUTROTType>, Equal<RUTROTTypes.rot>,
                                            And<GL.Branch.branchID, Equal<Current<ARInvoice.branchID>>>>>>))]
        public virtual string ROTOrganizationNbr { get; set; }
        #endregion
        #region RUTROTExportRefNbr
        public abstract class rUTROTExportRefNbr : IBqlField { }

        [PXDBInt]
        [PXUIField(DisplayName = "Export Ref Nbr.", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual int? RUTROTExportRefNbr { get; set; }
        #endregion
	}

	[Serializable()]
	public partial class ARInvoiceNbr : PX.Data.IBqlTable
	{
		#region DocType
		public abstract class docType : PX.Data.IBqlField
		{
		}
		protected String _DocType;
		[PXDBString(3, IsKey = true)]
		[PXDefault()]
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
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXDefault()]
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
		#region RefNoteID
		public abstract class refNoteID : PX.Data.IBqlField
		{
		}
		protected Int64? _RefNoteID;
		[PXDBLong()]
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
	}

    [Serializable]
	public partial class ARInvoiceAR622000 : IBqlTable
	{
		#region RefNbr
		[ARInvoiceType.RefNbr(typeof(Search2<ARInvoice.refNbr,
			InnerJoin<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>>,
			Where<ARInvoice.docType, Equal<Optional<ARInvoice.docType>>,
			And<ARInvoice.released, Equal<True>,
			And<Match<Customer, Current<AccessInfo.userName>>>>>, OrderBy<Desc<ARInvoice.refNbr>>>), Filterable = true)]
		public String RefNbr { get; set; }
		#endregion
	}

    [Serializable]
	public partial class ARInvoiceAR610500 : IBqlTable
	{
		#region RefNbr
		[ARInvoiceType.RefNbr(typeof(Search2<ARInvoice.refNbr,
			InnerJoin<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>>,
			Where<
				ARInvoice.docType, Equal<Optional<ARInvoice.docType>>,
				And2<
					Where<
						ARInvoice.hold, Equal<False>
						, And<ARInvoice.scheduled, Equal<False>, And<ARInvoice.voided, Equal<False>>>
						>
					, And<Match<Customer, Current<AccessInfo.userName>>>
					>
				>
			, OrderBy<Desc<ARInvoice.refNbr>>>), Filterable = true)]
		public String RefNbr { get; set; }
		#endregion
	}


    public class RUTROTTypes
    {
        public const string RUT = "U";
        public const string ROT = "O";

        public class rut : Constant<string>
        {
            public rut() : base(RUT) { }
        }

        public class rot : Constant<string>
        {
            public rot() : base(ROT) { }
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(new string[] { RUT, ROT }, new string[] { RUTROTMessages.RUTType, RUTROTMessages.ROTType })
            {
            }
        }
    }
}

namespace PX.Objects.AR.Standalone
{
	[Serializable()]
	[PXHidden(ServiceVisible = false)]
	public partial class ARInvoice : PX.Data.IBqlTable
	{
		#region DocType
		public abstract class docType : PX.Data.IBqlField
		{
		}
		protected string _DocType;
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
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
		protected string _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
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
		#region BillAddressID
		public abstract class billAddressID : PX.Data.IBqlField
		{
		}
		protected Int32? _BillAddressID;
		[PXDBInt()]
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
		[PXDBInt()]
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
		#region TermsID
		public abstract class termsID : PX.Data.IBqlField
		{
		}
		protected String _TermsID;
		[PXDBString(10, IsUnicode = true)]
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
		#region DiscDate
		public abstract class discDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _DiscDate;
		[PXDBDate()]
		public virtual DateTime? DiscDate
		{
			get
			{
				return this._DiscDate;
			}
			set
			{
				this._DiscDate = value;
			}
		}
		#endregion
		#region InvoiceNbr
		public abstract class invoiceNbr : PX.Data.IBqlField
		{
		}
		protected String _InvoiceNbr;
		[PXDBString(40, IsUnicode = true)]
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
		[PXDBDate()]
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
		[PXDBString(10, IsUnicode = true)]
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
		[PXDBString(15, IsUnicode = true)]
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
		[PXDBShort()]
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
		[PXDBDecimal(4)]
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
		[PXDBDecimal(4)]
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
		[PXDBDecimal(4)]
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
		#region LineTotal
		public abstract class lineTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _LineTotal;
		[PXDBDecimal(4)]
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

        #region CuryVatTaxableTotal
        public abstract class curyVatTaxableTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryVatTaxableTotal;
        [PXDBDecimal(4)]
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
        [PXDBDecimal(4)]
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

        #region CuryVatExemptTotal
        public abstract class curyVatExemptTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryVatExemptTotal;
        [PXDBDecimal(4)]
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
        [PXDBDecimal(4)]
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

		#region CommnPct
		public abstract class commnPct : PX.Data.IBqlField
		{
		}
		protected Decimal? _CommnPct;
		[PXDBDecimal(6)]
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
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal(4)]
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
		[PXDBDecimal(4)]
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
		[PXDBDecimal(4)]
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
		[PXDBDecimal(4)]
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
		[PXDBBool()]
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
		#region Printed
		public abstract class printed : PX.Data.IBqlField
		{
		}
		protected Boolean? _Printed;
		[PXDBBool()]
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
		#region DontEmail
		public abstract class dontEmail : PX.Data.IBqlField
		{
		}
		protected Boolean? _DontEmail;
		[PXDBBool()]
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
		#region Emailed
		public abstract class emailed : PX.Data.IBqlField
		{
		}
		protected Boolean? _Emailed;
		[PXDBBool()]
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
		#region CreditHold
		public abstract class creditHold : PX.Data.IBqlField
		{
		}
		protected Boolean? _CreditHold;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Credit Hold")]
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
		[PXDBBool()]
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
		[PXDBDecimal(4)]
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
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.IBqlField
		{
		}
		protected int? _WorkgroupID;
		[PXDBInt]
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
		[PXDBGuid()]
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
		#region RefNoteID
		public abstract class refNoteID : PX.Data.IBqlField
		{
		}
		protected Int64? _RefNoteID;
		[PXDBLong()]
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
		#region ProjectID
		public abstract class projectID : PX.Data.IBqlField
		{
		}
		protected Int32? _ProjectID;
		[PXDBInt()]
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

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "ROT and RUT deductible document", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual bool? IsRUTROTDeductible { get; set; }
        #endregion
        #region RUTROTType
        public abstract class rUTROTType : IBqlField { }

        [PXDBString(1)]
        [RUTROTTypes.List]
        [PXDefault(RUTROTTypes.RUT, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Deduction type", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual string RUTROTType { get; set; }
        #endregion
        #region CuryRUTROTTotalAmt
        public abstract class curyRUTROTTotalAmt : IBqlField { }

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Deductible Amount", FieldClass = AR.RUTROTMessages.FieldClass)]        
        public virtual Decimal? CuryRUTROTTotalAmt { get; set; }
        #endregion
        #region RUTROTTotalAmt
        public abstract class rUTROTTotalAmt : IBqlField { }

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? RUTROTTotalAmt { get; set; }
        #endregion
        #region CuryRUTROTPersonalAllowance
        public abstract class curyRUTROTPersonalAllowance : IBqlField { }

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Personal Allowance", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual Decimal? CuryRUTROTPersonalAllowance { get; set; }
        #endregion
        #region RUTROTPersonalAllowance
        public abstract class rUTROTPersonalAllowance : IBqlField { }

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? RUTROTPersonalAllowance { get; set; }
        #endregion
        #region RUTROTDeductionPct
        public abstract class rUTROTDeductionPct : IBqlField { }

        [PXDBDecimal(2)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Deduction %", Visible = false, FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual Decimal? RUTROTDeductionPct { get; set; }
        #endregion
        #region RUTROTAutoDistribution
        public abstract class rUTROTAutoDistribution : IBqlField { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Distribute Automatically", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual bool? RUTROTAutoDistribution { get; set; }
        #endregion
        #region IsRUTROTClaimed
        public abstract class isRUTROTClaimed : IBqlField { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "ROT and RUT was claimed", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual bool? IsRUTROTClaimed { get; set; }
        #endregion
        #region CuryRUTROTDistributedAmt
        public abstract class curyRUTROTDistributedAmt : IBqlField { }

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? CuryRUTROTDistributedAmt { get; set; }
        #endregion
        #region RUTROTDistributedAmt
        public abstract class rUTROTDistributedAmt : IBqlField { }

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? RUTROTDistributedAmt { get; set; }
        #endregion            
        #region RUTROTDistributionLineCntr
        public abstract class rUTROTDistributionLineCntr : IBqlField{}

        [PXDBInt()]
        [PXDefault(0)]
        public virtual Int32? RUTROTDistributionLineCntr { get; set; }
        #endregion
        #region SkipDiscounts
        public abstract class skipDiscounts : IBqlField
        {
        }
        protected bool? _SkipDiscounts = false;
        [PXDBBool()]
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
	}
}
