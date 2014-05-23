using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.TX;
using PX.Objects.CR;
using PX.Objects.CA;

namespace PX.Objects.AP
{
	public class APInvoiceType : APDocType
	{
		/// <summary>
		/// Specialized selector for APInvoice RefNbr.<br/>
		/// By default, defines the following set of columns for the selector:<br/>
		/// APInvoice.refNbr,APInvoice.docDate, APInvoice.finPeriodID,<br/>
		/// APInvoice.vendorID, APInvoice.vendorID_Vendor_acctName,<br/>
		/// APInvoice.vendorLocationID, APInvoice.curyID, APInvoice.curyOrigDocAmt,<br/>
		/// APInvoice.curyDocBal,APInvoice.status, APInvoice.dueDate, APInvoice.invoiceNbr<br/>
		/// </summary>
		public class RefNbrAttribute : PXSelectorAttribute
		{
			public RefNbrAttribute(Type SearchType)
				: base(SearchType,
				typeof(APInvoice.refNbr), 
				typeof(APInvoice.docDate),
				typeof(APInvoice.finPeriodID),
				typeof(APInvoice.vendorID),
				typeof(APInvoice.vendorID_Vendor_acctName),
				typeof(APInvoice.vendorLocationID),
				typeof(APInvoice.invoiceNbr),
				typeof(APInvoice.curyID),
				typeof(APInvoice.curyOrigDocAmt),
				typeof(APInvoice.curyDocBal),
				typeof(APInvoice.status),
				typeof(APInvoice.dueDate))
			{
			}
		}

		public class AdjdRefNbrAttribute : PXSelectorAttribute
		{
			public AdjdRefNbrAttribute(Type SearchType)
				: base(SearchType,
				typeof(APRegister.refNbr),
				typeof(APRegister.docDate),
				typeof(APRegister.finPeriodID),
				typeof(APRegister.vendorLocationID),
				typeof(APRegister.curyID),
				typeof(APRegister.curyOrigDocAmt),
				typeof(APRegister.curyDocBal),
				typeof(APRegister.status),
				typeof(APAdjust.APInvoice.dueDate),
				typeof(APAdjust.APInvoice.invoiceNbr),
				typeof(APRegister.docDesc))
			{
			}
		}

		/// <summary>
		/// Specialized for APInvoices version of the <see cref="AutoNumberAttribute"/><br/>
		/// It defines how the new numbers are generated for the AP Invoice. <br/>
		/// References APInvoice.docType and APInvoice.docDate fields of the document,<br/>
		/// and also define a link between  numbering ID's defined in AP Setup and APInvoice types:<br/>
		/// namely APSetup.invoiceNumberingID, APSetup.adjustmentNumberingID,<br/>
		/// APSetup.adjustmentNumberingID, APSetup.invoiceNumberingID <br/>
		/// </summary>
		public class NumberingAttribute : AutoNumberAttribute
		{
			public NumberingAttribute()
				: base(typeof(APInvoice.docType), typeof(APInvoice.docDate),
					new string[] { Invoice, CreditAdj, DebitAdj, Prepayment },
					new Type[] { typeof(APSetup.invoiceNumberingID), typeof(APSetup.creditAdjNumberingID), typeof(APSetup.debitAdjNumberingID), typeof(APSetup.invoiceNumberingID) }) { ; }
		}

		public new class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Invoice, CreditAdj, DebitAdj, Prepayment },
				new string[] { Messages.Invoice, Messages.CreditAdj, Messages.DebitAdj, Messages.Prepayment }) { }
		}

		public class TaxInvoiceListAttribute : PXStringListAttribute
		{
			public TaxInvoiceListAttribute()
				: base(
				new string[] { Invoice, CreditAdj, DebitAdj },
				new string[] { Messages.Invoice, Messages.CreditAdj, Messages.DebitAdj }) { }
		}

		public class AdjdListAttribute : PXStringListAttribute
		{
			public AdjdListAttribute()
				: base(
				new string[] { Invoice, CreditAdj},
				new string[] { Messages.Invoice, Messages.CreditAdj }) { }
		}
		
		/// <summary>
		/// String list with DocType, suitable for the APInvoice document.<br/>
		/// Used in the DocType selector of APInvoices. <br/>
		/// </summary>
		public class InvoiceListAttribute : PXStringListAttribute
		{
			public InvoiceListAttribute()
				: base(
				new string[] { Invoice, CreditAdj, Prepayment },
				new string[] { Messages.Invoice, Messages.CreditAdj, Messages.Prepayment }) { }
		}

		public static string DrCr(string DocType)
		{
			switch (DocType)
			{
				case Invoice:
				case CreditAdj:
				case Prepayment:
				case QuickCheck:
					return "D";
				case DebitAdj:
				case VoidQuickCheck:
					return "C";
				default:
					return null;
			}
		}
	}

	public class APPaymentBy 
	{
		public const int DueDate = 0;
		public const int DiscountDate = 1;

		public class List : PXIntListAttribute
		{
			public List()
				: base(
					new[] {DueDate, DiscountDate},
					new[] {Messages.DueDate, Messages.DiscountDate})
			{
			}
		}

		public class dueDate : Constant<int>
		{
			public dueDate() : base(DueDate)
			{
			}
		}
		public class discountDate : Constant<int>
		{
			public discountDate() : base(DiscountDate)
			{
			}
		}

	}
	[System.SerializableAttribute()]
	[PXTable()]
	[PXSubstitute(GraphType = typeof(APInvoiceEntry))]
	[PXSubstitute(GraphType = typeof(TX.TXInvoiceEntry))]
	[PXPrimaryGraph(typeof(APInvoiceEntry))]
	[PXCacheName(Messages.APInvoice)]
	public partial class APInvoice : APRegister, IInvoice
	{
		#region Selected
		public new abstract class selected : PX.Data.IBqlField
		{
		}
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.IBqlField
		{
		}
		[GL.Branch(typeof(Coalesce<
			Search<Location.vBranchID, Where<Location.bAccountID, Equal<Current<APRegister.vendorID>>, And<Location.locationID, Equal<Current<APRegister.vendorLocationID>>>>>,
			Search<Branch.branchID, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>), IsDetail = false)]
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
		#region DocType
		public new abstract class docType : PX.Data.IBqlField
		{
		}
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[APInvoiceType.List()]
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
		[APInvoiceType.RefNbr(typeof(Search2<APInvoice.refNbr, 
			InnerJoin<Vendor, On<APInvoice.vendorID, Equal<Vendor.bAccountID>>>,
			Where<APInvoice.docType,Equal<Optional<APInvoice.docType>>, 
			And2<Where<APInvoice.origModule, NotEqual<BatchModule.moduleTX>, Or<APInvoice.released, Equal<True>>>,
			And<Match<Vendor, Current<AccessInfo.userName>>>>>, OrderBy<Desc<APInvoice.refNbr>>>), Filterable = true)]
		[APInvoiceType.Numbering()]
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
		#region VendorID
		public new abstract class vendorID : PX.Data.IBqlField
		{
		}
		#endregion
		#region VendorLocationID
		public new abstract class vendorLocationID : PX.Data.IBqlField
		{
		}
		#endregion
		#region APAccountID
		public new abstract class aPAccountID : PX.Data.IBqlField
		{
		}
		#endregion
		#region APSubID
		public new abstract class aPSubID : PX.Data.IBqlField
		{
		}
		#endregion
		#region TermsID
		public abstract class termsID : PX.Data.IBqlField
		{
		}
		protected String _TermsID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<Vendor.termsID, Where<Vendor.bAccountID, Equal<Current<APInvoice.vendorID>>, And<Current<APInvoice.docType>, NotEqual<APDocType.debitAdj>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, Or<Terms.visibleTo,Equal<TermsVisibleTo.vendor>>>>),DescriptionField=typeof(Terms.descr),Filterable=true)]
		[Terms(typeof(APInvoice.docDate), typeof(APInvoice.dueDate), typeof(APInvoice.discDate), typeof(APInvoice.curyOrigDocAmt), typeof(APInvoice.curyOrigDiscAmt))]
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
		public abstract class dueDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _DueDate;
		[PXDBDate()]
		[PXUIField(DisplayName="Due Date", Visibility=PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DueDate
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
		[PXUIField(DisplayName="Cash Discount Date", Visibility=PXUIVisibility.SelectorVisible)]
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
		[PXDBString(40, IsUnicode=true)]
		[PXUIField(DisplayName="Vendor Ref.", Visibility=PXUIVisibility.SelectorVisible)]
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
		[PXUIField(DisplayName="Vendor Ref. Date", Visibility=PXUIVisibility.Invisible)]
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
		[PXDefault(typeof(Search<Location.vTaxZoneID,Where<Location.bAccountID,Equal<Current<APInvoice.vendorID>>, And<Location.locationID,Equal<Current<APInvoice.vendorLocationID >>>>>), PersistingCheck=PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Vendor Tax Zone", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TaxZone.taxZoneID),DescriptionField=typeof(TaxZone.descr),Filterable =true)]
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
		#region CuryTaxTotal
		public abstract class curyTaxTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTaxTotal;
		[PXDBCurrency(typeof(APInvoice.curyInfoID), typeof(APInvoice.taxTotal))]
		[PXUIField(DisplayName="Tax Total",Visibility=PXUIVisibility.Visible, Enabled=false)]
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
		[PXDBCurrency(typeof(APInvoice.curyInfoID), typeof(APInvoice.lineTotal))]
		[PXUIField(DisplayName="Detail Total",Visibility=PXUIVisibility.Visible, Enabled=false)]
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

		#region CuryVatExemptTotal
		public abstract class curyVatExemptTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryVatExemptTotal;
		[PXDBCurrency(typeof(APInvoice.curyInfoID), typeof(APInvoice.vatExemptTotal))]
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
		[PXDBCurrency(typeof(APInvoice.curyInfoID), typeof(APInvoice.vatTaxableTotal))]
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
		#region CuryOrigWhTaxAmt
		public new abstract class curyOrigWhTaxAmt : PX.Data.IBqlField
		{
		}
		#endregion
		#region OrigWhTaxAmt
		public new abstract class origWhTaxAmt : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryWhTaxBal
		public new abstract class curyWhTaxBal : PX.Data.IBqlField
		{
		}
		#endregion
		#region WhTaxBal
		public new abstract class whTaxBal : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryTaxWheld
		public new abstract class curyTaxWheld : PX.Data.IBqlField
		{
		}
		#endregion
		#region TaxWheld
		public new abstract class taxWheld : PX.Data.IBqlField
		{
		}
		#endregion
		#region DrCr
		public abstract class drCr : PX.Data.IBqlField
		{
		}
		protected string _DrCr;
		[PXString(1,IsFixed = true)]
		public string DrCr
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return APInvoiceType.DrCr(this._DocType);
			}
			set
			{
			}
		}
		#endregion
		#region SeparateCheck
		public abstract class separateCheck : PX.Data.IBqlField
		{
		}
		protected Boolean? _SeparateCheck;
		[PXDBBool()]
		[PXUIField(DisplayName = "Pay Separately", Visibility = PXUIVisibility.Visible)]
		[PXDefault(typeof(Search<Location.separateCheck,Where<Location.bAccountID,Equal<Current<APInvoice.vendorID>>, And<Location.locationID, Equal<Current<APInvoice.vendorLocationID>>>>>))]
		public virtual Boolean? SeparateCheck
		{
			get
			{
				return this._SeparateCheck;
			}
			set
			{
				this._SeparateCheck = value;
				this.SetStatus();
			}
		}
		#endregion
		#region PaySel
		public abstract class paySel : IBqlField
		{
		}
		protected bool? _PaySel;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public bool? PaySel
		{
			get
			{
				return _PaySel;
			}
			set
			{
				_PaySel = value;
			}
		}
		#endregion
		#region PayLocationID
		public abstract class payLocationID : PX.Data.IBqlField
		{
		}
		protected Int32? _PayLocationID;
		[LocationID(
			typeof(Where<Location.bAccountID, Equal<Current<APRegister.vendorID>>,
				And<Location.isActive, Equal<True>, And<MatchWithBranch<Location.vBranchID>>>>),
			DescriptionField = typeof(Location.descr),
			Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Coalesce<
			Search2<Vendor.defLocationID,
			InnerJoin<Location,
				On<Location.locationID, Equal<Vendor.defLocationID>,
				And<Location.bAccountID, Equal<Vendor.bAccountID>>>>,
			Where<Vendor.bAccountID, Equal<Current<APRegister.vendorID>>,
				And<Location.isActive, Equal<True>, And<MatchWithBranch<Location.vBranchID>>>>>,
			Search<Location.locationID,
			Where<Location.bAccountID, Equal<Current<APRegister.vendorID>>,
			And<Location.isActive, Equal<True>, And<MatchWithBranch<Location.vBranchID>>>>>>))]
		public virtual Int32? PayLocationID
		{
			get
			{
				return this._PayLocationID;
			}
			set
			{
				this._PayLocationID = value;
			}
		}
		#endregion
		#region PayDate
		public abstract class payDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _PayDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Pay Date", Visibility = PXUIVisibility.Visible)]
		[PXDefault()]
		[PXFormula(typeof(SO.DateMinusDaysNotLessThenDate<
			Switch<
				Case<Where<Selector<APInvoice.vendorLocationID, Location.vPaymentByType>, Equal<APPaymentBy.discountDate>, 
							 And<APInvoice.discDate, IsNotNull>>, APInvoice.discDate>, 
				APInvoice.dueDate>,
			IsNull<Selector<APInvoice.vendorLocationID, Location.vPaymentLeadTime>, decimal0>, APInvoice.docDate>))]
		public virtual DateTime? PayDate
		{
			get
			{
				return this._PayDate;
			}
			set
			{
				this._PayDate = value;
			}
		}
		#endregion
		#region PayTypeID
		public abstract class payTypeID : PX.Data.IBqlField
		{
		}
		protected String _PayTypeID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Method", Visibility = PXUIVisibility.Visible)]

		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
						  Where<PaymentMethod.useForAP, Equal<True>,
							And<PaymentMethod.isActive, Equal<boolTrue>>>>), DescriptionField = typeof(PaymentMethod.descr))]
		[PXDefault(typeof(Search<Location.paymentMethodID, Where<Location.bAccountID, Equal<Current<APInvoice.vendorID>>, And<Location.locationID, Equal<Current<APInvoice.payLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String PayTypeID
		{
			get
			{
				return this._PayTypeID;
			}
			set
			{
				this._PayTypeID = value;
			}
		}
		#endregion
		#region PayAccountID
		public abstract class payAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _PayAccountID;
		[PXDefault(typeof(Coalesce<Search2<Location.cashAccountID,
										InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<Location.cashAccountID>,
											And<PaymentMethodAccount.paymentMethodID, Equal<Location.vPaymentMethodID>,
											And<PaymentMethodAccount.useForAP, Equal<True>>>>>,
										Where<Location.bAccountID, Equal<Current<APInvoice.vendorID>>,
										And<Location.locationID, Equal<Current<APInvoice.payLocationID>>,
										And<Location.vPaymentMethodID, Equal<Current<APInvoice.payTypeID>>>>>>,
								   Search2<PaymentMethodAccount.cashAccountID, InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>>,
										Where<PaymentMethodAccount.paymentMethodID, Equal<Current<APInvoice.payTypeID>>,
											And<CashAccount.branchID, Equal<Current<APInvoice.branchID>>,
											And<PaymentMethodAccount.useForAP, Equal<True>,
											And<PaymentMethodAccount.aPIsDefault, Equal<True>>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[CashAccount(typeof(APInvoice.branchID), typeof(Search2<CashAccount.cashAccountID,
						InnerJoin<PaymentMethodAccount,
							On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
						Where2<Match<Current<AccessInfo.userName>>,
							And<CashAccount.clearingAccount, Equal<False>,
							And<PaymentMethodAccount.paymentMethodID, Equal<Current<APInvoice.payTypeID>>,
							And<PaymentMethodAccount.useForAP, Equal<True>>>>>>), DisplayName = "Cash Account",
										Visibility = PXUIVisibility.Visible, DescriptionField = typeof(CashAccount.descr))]
		[PXFormula(typeof(Validate<APInvoice.curyID>))]
		public virtual Int32? PayAccountID
		{
			get
			{
				return this._PayAccountID;
			}
			set
			{
				this._PayAccountID = value;
			}
		}
		#endregion
		#region PrebookAcctID
		public abstract class prebookAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _PrebookAcctID;
		
		[PXDefault(typeof(Select<Vendor, Where<Vendor.bAccountID, Equal<Current<APInvoice.vendorID>>>>), SourceField = typeof(Vendor.prebookAcctID), PersistingCheck = PXPersistingCheck.Nothing)]
        [Account(DisplayName = "Reclassification Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		public virtual Int32? PrebookAcctID
		{
			get
			{
				return this._PrebookAcctID;
			}
			set
			{
				this._PrebookAcctID = value;
			}
		}
		#endregion
		#region PrebookSubID
		public abstract class prebookSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _PrebookSubID;

		[PXDefault(typeof(Select<Vendor, Where<Vendor.bAccountID, Equal<Current<APInvoice.vendorID>>>>), SourceField = typeof(Vendor.prebookSubID), PersistingCheck = PXPersistingCheck.Nothing)]
        [SubAccount(typeof(APInvoice.prebookAcctID), DisplayName = "Reclassification Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? PrebookSubID
		{
			get
			{
				return this._PrebookSubID;
			}
			set
			{
				this._PrebookSubID = value;
			}
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
		#region Printed
		public new abstract class printed : PX.Data.IBqlField
		{
		}
		#endregion
		#region Voided
		public new abstract class voided : PX.Data.IBqlField
		{
		}
		#endregion		
		#region Prebooked
		public new abstract class prebooked : PX.Data.IBqlField
		{
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
		#region BatchNbr
		public new abstract class batchNbr : PX.Data.IBqlField
		{
		}
		#endregion
		#region PrebookBatchNbr
		public new abstract class prebookBatchNbr : PX.Data.IBqlField
		{
		}
		#endregion
		#region VoidBatchNbr
		public new abstract class voidBatchNbr : PX.Data.IBqlField
		{
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
		#region DocDesc
		public new abstract class docDesc : PX.Data.IBqlField
		{
		}
		#endregion
		#region VendorID_Vendor_acctName
		public abstract class vendorID_Vendor_acctName : PX.Data.IBqlField
		{
		}
		#endregion
		#region EstPayDate
		public abstract class estPayDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _EstPayDate;
		[PXDBCalced(typeof(Switch<Case<Where<APInvoice.paySel,Equal<boolTrue>>, APInvoice.payDate>, APInvoice.dueDate>), typeof(DateTime))]
		public virtual DateTime? EstPayDate
		{
			get
			{
				return this._EstPayDate;
			}
			set
			{
				this._EstPayDate = value;
			}
		}
		#endregion

		#region LCEnabled
		public abstract class lCEnabled : PX.Data.IBqlField
		{
		}
		protected Boolean? _LCEnabled = false;
		[PXBool()]
		[PXUIField(Visible = false)]
		public virtual Boolean? LCEnabled
		{
			get
			{
				return this._LCEnabled;
			}
			set
			{
				this._LCEnabled = value;
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
	}
}

namespace PX.Objects.AP.Standalone
{
	[Serializable()]
	[PXHidden(ServiceVisible = false)]
	public partial class APInvoice : PX.Data.IBqlTable
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
		#region DueDate
		public abstract class dueDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _DueDate;
		[PXDBDate()]
		public virtual DateTime? DueDate
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
		[PXUIField(DisplayName = "Vendor Ref. Date", Visibility = PXUIVisibility.Invisible)]
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
		#region SeparateCheck
		public abstract class separateCheck : PX.Data.IBqlField
		{
		}
		protected Boolean? _SeparateCheck;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? SeparateCheck
		{
			get
			{
				return this._SeparateCheck;
			}
			set
			{
				this._SeparateCheck = value;
			}
		}
		#endregion
		#region PaySel
		public abstract class paySel : IBqlField
		{
		}
		protected bool? _PaySel = false;
		[PXDBBool]
		[PXDefault(false)]
		public bool? PaySel
		{
			get
			{
				return _PaySel;
			}
			set
			{
				_PaySel = value;
			}
		}
		#endregion
		#region PayDate
		public abstract class payDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _PayDate;
		[PXDBDate()]
		public virtual DateTime? PayDate
		{
			get
			{
				return this._PayDate;
			}
			set
			{
				this._PayDate = value;
			}
		}
		#endregion
		#region PayTypeID
		public abstract class payTypeID : PX.Data.IBqlField
		{
		}
		protected String _PayTypeID;
		[PXDBString(10, IsUnicode = true)]
		public virtual String PayTypeID
		{
			get
			{
				return this._PayTypeID;
			}
			set
			{
				this._PayTypeID = value;
			}
		}
		#endregion
		#region PayAccountID
		public abstract class payAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _PayAccountID;
		[PXDBInt()]
		public virtual Int32? PayAccountID
		{
			get
			{
				return this._PayAccountID;
			}
			set
			{
				this._PayAccountID = value;
			}
		}
		#endregion
		#region PayLocationID
		public abstract class payLocationID : PX.Data.IBqlField
		{
		}
		protected Int32? _PayLocationID;
		[PXDBInt()]
		public virtual Int32? PayLocationID
		{
			get
			{
				return this._PayLocationID;
			}
			set
			{
				this._PayLocationID = value;
			}
		}
		#endregion
		#region PrebookAcctID
		public abstract class prebookAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _PrebookAcctID;

		[PXDBInt()]
		public virtual Int32? PrebookAcctID
		{
			get
			{
				return this._PrebookAcctID;
			}
			set
			{
				this._PrebookAcctID = value;
			}
		}
		#endregion
		#region PrebookSubID
		public abstract class prebookSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _PrebookSubID;

		[PXDBInt()]
		public virtual Int32? PrebookSubID
		{
			get
			{
				return this._PrebookSubID;
			}
			set
			{
				this._PrebookSubID = value;
			}
		}
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
