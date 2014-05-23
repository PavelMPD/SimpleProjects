using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CR;
using ARCashSale = PX.Objects.AR.Standalone.ARCashSale;
using SOInvoice = PX.Objects.SO.SOInvoice;

namespace PX.Objects.AR
{
	public class ARDocType
	{
		public class CustomListAttribute : PXStringListAttribute
		{
			public string[] AllowedValues
			{
				get
				{
					return _AllowedValues;
				}
			}

			public string[] AllowedLabels
			{
				get
				{
					return _AllowedLabels;
				}
			}

			public CustomListAttribute(string[] AllowedValues, string[] AllowedLabels)
				: base(AllowedValues, AllowedLabels)
			{
			}
		}

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Invoice, DebitMemo, CreditMemo, Payment, VoidPayment, Prepayment, Refund, FinCharge, SmallBalanceWO, SmallCreditWO, CashSale, CashReturn },
				new string[] { Messages.Invoice, Messages.DebitMemo, Messages.CreditMemo, Messages.Payment, Messages.VoidPayment, Messages.Prepayment, Messages.Refund, Messages.FinCharge, Messages.SmallBalanceWO, Messages.SmallCreditWO, Messages.CashSale, Messages.CashReturn }) { }
		}

        /// <summary>
        /// Defines a Selector of the AR Document types with shorter description.<br/>
        /// In the screens displayed as combo-box.<br/>
        /// Mostly used in the reports.<br/>
        /// </summary>
		public class PrintListAttribute : PXStringListAttribute
		{
			public PrintListAttribute()
				: base(
				new string[] { Invoice, DebitMemo, CreditMemo, Payment, VoidPayment, Prepayment, Refund, FinCharge, SmallBalanceWO, SmallCreditWO, CashSale, CashReturn },
				new string[] { Messages.PrintInvoice, Messages.PrintDebitMemo, Messages.PrintCreditMemo, Messages.PrintPayment, Messages.PrintVoidPayment, Messages.PrintPrepayment, Messages.PrintRefund, Messages.PrintFinCharge, Messages.PrintSmallBalanceWO, Messages.PrintSmallCreditWO, Messages.PrintCashSale, Messages.PrintCashReturn }) { }
		}

		public class SOListAttribute  : PXStringListAttribute
		{
			public SOListAttribute()
				: base(
				new string[] { Invoice, DebitMemo, CreditMemo, CashSale, CashReturn, NoUpdate },
				new string[] { Messages.Invoice, Messages.DebitMemo, Messages.CreditMemo, Messages.CashSale, Messages.CashReturn, Messages.NoUpdate}) { }
		}

        /// <summary>
        /// Defines a list of the AR Document types, which may be used in the SO module<br/>
        /// </summary>
		public class SOEntryListAttribute : CustomListAttribute
		{
			public SOEntryListAttribute()
				: base(
				new string[] { Invoice, DebitMemo, CreditMemo, CashSale, CashReturn },
				new string[] { Messages.Invoice, Messages.DebitMemo, Messages.CreditMemo, Messages.CashSale, Messages.CashReturn }) { }
		}

		public const string Invoice = "INV";
		public const string DebitMemo = "DRM";
		public const string CreditMemo = "CRM";
		public const string Payment = "PMT";
		public const string VoidPayment = "RPM";
		public const string Prepayment = "PPM";
		public const string Refund = "REF";
		public const string FinCharge = "FCH";
		public const string SmallBalanceWO = "SMB";
		public const string SmallCreditWO = "SMC";
		public const string CashSale = "CSL";
		public const string CashReturn = "RCS";
		public const string Undefined = "UND";
		public const string NoUpdate = Undefined;

		public class invoice : Constant<string>
		{
			public invoice() : base(Invoice) { ;}
		}

		public class debitMemo : Constant<string>
		{
			public debitMemo() : base(DebitMemo) { ;}
		}

		public class creditMemo : Constant<string>
		{
			public creditMemo() : base(CreditMemo) { ;}
		}

		public class payment : Constant<string>
		{
			public payment() : base(Payment) { ;}
		}

		public class voidPayment : Constant<string>
		{
			public voidPayment() : base(VoidPayment) { ;}
		}
		public class prepayment : Constant<string>
		{
			public prepayment() : base(Prepayment) { ;}
		}
		public class refund : Constant<string>
		{
			public refund() : base(Refund) { ;}
		}

		public class finCharge : Constant<string>
		{
			public finCharge() : base(FinCharge) { ;}
		}

		public class smallBalanceWO : Constant<string>
		{
			public smallBalanceWO() : base(SmallBalanceWO) { ;}
		}

		public class smallCreditWO : Constant<string>
		{
			public smallCreditWO() : base(SmallCreditWO) { ;}
		}
				
		public class undefined : Constant<string> 
		{
			public undefined() : base(Undefined) { ;}
		}

		public class noUpdate : Constant<string>
		{
			public noUpdate() : base(NoUpdate) { ;}
		}

		public class cashSale : Constant<string>
		{
			public cashSale() : base(CashSale) { ;}
		}

		public class cashReturn : Constant<string>
		{
			public cashReturn() : base(CashReturn) { ;}
		}

		public static bool? Payable(string DocType)
		{
			switch (DocType)
			{
				case Invoice:
				case DebitMemo:
				case FinCharge:
				case SmallCreditWO: 
					return true;
				case Payment:
				case Prepayment:
				case CreditMemo:
				case VoidPayment:
				case Refund:
				case SmallBalanceWO:
				case CashSale:
				case CashReturn:
					return false;
				default:
					return null;
			}
		}

		public static Int16? SortOrder(string DocType)
		{
			switch (DocType)
			{
				case Invoice:
				case DebitMemo:
				case FinCharge:
				case CashSale:
					return 0;
				case Prepayment:
					return 1;
				case CreditMemo:
					return 2;
				case Payment:
				case SmallBalanceWO:
					return 3;
				case SmallCreditWO:
				case Refund:
					return 4;
				case VoidPayment:
				case CashReturn:
					return 5;
				default:
					return null;
			}
		}

		public static Decimal? SignBalance(string DocType)
		{
			switch (DocType)
			{
				case Refund:
				case Invoice:
				case DebitMemo:
				case FinCharge:
				case SmallCreditWO: 
					return 1m;
				case CreditMemo:
				case Payment :
				case Prepayment:
				case VoidPayment:
				case SmallBalanceWO:
					return -1m;
				case CashSale:
				case CashReturn:
					return 0;
				default:
					return null;
			}
		}
		
		public static Decimal? SignAmount(string DocType)
		{
			switch (DocType)
			{
				case Refund:
				case Invoice:
				case DebitMemo:
				case FinCharge:
				case SmallCreditWO: 
				case CashSale:
					return 1m;
				case CreditMemo:
				case Payment :
				case Prepayment:
				case VoidPayment:
				case SmallBalanceWO:
				case CashReturn:
					return -1m;
				default:
					return null;
			}
		}

		public static string TaxDrCr(string DocType)
		{
			switch (DocType)
			{
			  //Invoice Types
				case Invoice:
				case DebitMemo:
				case FinCharge:
				case CashSale:
					return "C";
				case CreditMemo:
				case CashReturn:
					return "D";
				default:
					return "C";
			}
		}

		public static string DocClass(string DocType)
		{
			switch (DocType) 
			{
				case Invoice:
				case DebitMemo:
				case CreditMemo:
				case FinCharge:
				case CashSale:
				case CashReturn:
					return "N";
				case Payment:
				case VoidPayment:
				case Refund:
					return "P";
				case SmallBalanceWO:
				case SmallCreditWO:
				case Prepayment:
					return "U";
				default:
					return null;
			}
		}
	}

	public class ARDocStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { CreditHold, CCHold, Hold, Balanced, Voided, Scheduled, Open, Closed, PendingPrint, PendingEmail },
				new string[] { Messages.CreditHold, Messages.CCHold, Messages.Hold, Messages.Balanced, Messages.Voided, Messages.Scheduled, Messages.Open, Messages.Closed, Messages.PendingPrint, Messages.PendingEmail }) { ; }
		}

		public const string Hold = "H";
		public const string Balanced = "B";
		public const string Voided = "V";
		public const string Scheduled = "S";
		public const string Open = "N";
		public const string Closed = "C";
		public const string PendingPrint = "P";
		public const string PendingEmail = "E";
		public const string CreditHold = "R";
		public const string CCHold = "W";

		public class hold : Constant<string>
		{
			public hold() : base(Hold) { ;}
		}

		public class balanced : Constant<string>
		{
			public balanced() : base(Balanced) { ;}
		}

		public class voided : Constant<string>
		{
			public voided() : base(Voided) { ;}
		}

		public class scheduled : Constant<string>
		{
			public scheduled() : base(Scheduled) { ;}
		}

		public class open : Constant<string>
		{
			public open() : base(Open) { ;}
		}

		public class closed : Constant<string>
		{
			public closed() : base(Closed) { ;}
		}

		public class pendingPrint : Constant<string>
		{
			public pendingPrint() : base(PendingPrint) { ;}
		}

		public class pendingEmail : Constant<string>
		{
			public pendingEmail() : base(PendingEmail) { ;}
		}

		public class cCHold : Constant<string>
		{
			public cCHold() : base(CCHold) { ;}
		}

		public class creditHold : Constant<string>
		{
			public creditHold() : base(CreditHold) { ;}
		}
	}

	[PXPrimaryGraph(new Type[] {
		typeof(SO.SOInvoiceEntry),
		typeof(ARCashSaleEntry),
		typeof(ARInvoiceEntry), 
		typeof(ARPaymentEntry)
	},
		new Type[] {
		typeof(Select2<ARInvoice, InnerJoin<SOInvoice, On<SOInvoice.docType, Equal<ARInvoice.docType>, And<SOInvoice.refNbr, Equal<ARInvoice.refNbr>>>>, 
			Where<ARInvoice.docType, Equal<Current<ARRegister.docType>>, 
				And<ARInvoice.refNbr, Equal<Current<ARRegister.refNbr>>,
				And<ARInvoice.released, Equal<boolFalse>>>>>),
		typeof(Select<ARCashSale, 
			Where<ARCashSale.docType, Equal<Current<ARRegister.docType>>, 
			And<ARCashSale.refNbr, Equal<Current<ARRegister.refNbr>>>>>),
		typeof(Select<ARInvoice, 
			Where<ARInvoice.docType, Equal<Current<ARRegister.docType>>, 
			And<ARInvoice.refNbr, Equal<Current<ARRegister.refNbr>>>>>),
		typeof(Select<ARPayment, 
			Where<ARPayment.docType, Equal<Current<ARRegister.docType>>, 
			And<ARPayment.refNbr, Equal<Current<ARRegister.refNbr>>>>>)
		})]
	[System.SerializableAttribute()]
	[ARRegisterCacheName(Messages.ARDocument)]	
	public partial class ARRegister : PX.Data.IBqlTable
	{
		#region Selected
		public abstract class selected : IBqlField
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
		#region BranchID
		public abstract class branchID : PX.Data.IBqlField
		{
		}
		protected Int32? _BranchID;
		[Branch()]
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
		#region DocType
		public abstract class docType : PX.Data.IBqlField
		{
		}
		protected String _DocType;
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[ARDocType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
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
		#region PrintDocType
		public abstract class printDocType : PX.Data.IBqlField
		{
		}
		[PXString(3, IsFixed = true)]
		[ARDocType.PrintList()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.Visible, Enabled = true)]
		public virtual String PrintDocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.IBqlField
		{
		}
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[PXSelector(typeof(Search<ARRegister.refNbr, Where<ARRegister.docType, Equal<Optional<ARRegister.docType>>>>),Filterable=true)]
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
		#region OrigModule
		public abstract class origModule : PX.Data.IBqlField
		{
		}
		protected String _OrigModule;
		[PXDBString(2, IsFixed = true)]
		[PXDefault(GL.BatchModule.AR)]
		[PXUIField(DisplayName = "Source", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[GL.BatchModule.FullList()]
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
		#region DocDate
		public abstract class docDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _DocDate;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region OrigDocDate
		public abstract class origDocDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _OrigDocDate;
		[PXDBDate()]
		public virtual DateTime? OrigDocDate
		{
			get
			{
				return this._OrigDocDate;
			}
			set
			{
				this._OrigDocDate = value;
			}
		}
		#endregion
        #region DueDate
        public abstract class dueDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _DueDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Due Date", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.IBqlField
		{
		}
		protected String _TranPeriodID;
		[FinPeriodID(typeof(ARRegister.docDate))]
		public virtual String TranPeriodID
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
		public abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		protected String _FinPeriodID;
		[AROpenPeriod(typeof(ARRegister.docDate))]
        [PXDefault()]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region CustomerID
		public abstract class customerID : PX.Data.IBqlField
		{
		}
		protected Int32? _CustomerID;
		[CustomerActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName),Filterable=true, TabOrder=2)]
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
		#region CustomerLocationID
		public abstract class customerLocationID : PX.Data.IBqlField
		{
		}
		protected Int32? _CustomerLocationID;
		[LocationID(typeof(Where<Location.bAccountID, Equal<Optional<ARRegister.customerID>>,
			And<Location.isActive, Equal<True>,
			And<MatchWithBranch<Location.cBranchID>>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, TabOrder = 3)]
		[PXDefault(typeof(Coalesce<
			Search2<BAccountR.defLocationID,
			InnerJoin<Location, On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>>,
			Where<BAccountR.bAccountID, Equal<Current<ARRegister.customerID>>,
				And<Location.isActive, Equal<True>,	And<MatchWithBranch<Location.cBranchID>>>>>,
			Search<Location.locationID, 
			Where<Location.bAccountID, Equal<Current<ARRegister.customerID>>, 
			And<Location.isActive, Equal<True>, And<MatchWithBranch<Location.cBranchID>>>>>>))]
		public virtual Int32? CustomerLocationID
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
		#region CuryID
		public abstract class curyID : PX.Data.IBqlField
		{
		}
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Search<Company.baseCuryID>))]
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
		#region ARAccountID
		public abstract class aRAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _ARAccountID;
		[PXDefault]
        [Account(typeof(ARRegister.branchID), typeof(Search<Account.accountID,
                    Where2<Match<Current<AccessInfo.userName>>,
                         And<Account.active, Equal<True>,
                         And<Account.isCashAccount, Equal<False>,
                         And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
                          Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>>), DisplayName = "AR Account")]
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
		[SubAccount(typeof(ARRegister.aRAccountID), DescriptionField = typeof(Sub.description), DisplayName = "AR Subaccount", Visibility = PXUIVisibility.Visible)]
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
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(ModuleCode = "AR")]
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
		#region CuryOrigDocAmt
		public abstract class curyOrigDocAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryOrigDocAmt;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.origDocAmt))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region OrigDocAmt
		public abstract class origDocAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _OrigDocAmt;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OrigDocAmt
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
		public abstract class curyDocBal : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryDocBal;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.docBal), BaseCalc=false)]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
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
		#region DocBal
		public abstract class docBal : PX.Data.IBqlField
		{
		}
		protected Decimal? _DocBal;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DocBal
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
		public abstract class curyOrigDiscAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryOrigDiscAmt;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.origDiscAmt))]
		[PXUIField(DisplayName = "Cash Discount", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? CuryOrigDiscAmt
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
		public abstract class origDiscAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _OrigDiscAmt;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OrigDiscAmt
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
		public abstract class curyDiscTaken : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryDiscTaken;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.discTaken))]
		public virtual Decimal? CuryDiscTaken
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
		public abstract class discTaken : PX.Data.IBqlField
		{
		}
		protected Decimal? _DiscTaken;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DiscTaken
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
		public abstract class curyDiscBal : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryDiscBal;
        [PXUIField(DisplayName = "Cash Discount Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.discBal), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryDiscBal
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
		public abstract class discBal : PX.Data.IBqlField
		{
		}
		protected Decimal? _DiscBal;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DiscBal
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

        #region DiscTot
        public abstract class discTot : PX.Data.IBqlField
        {
        }
        protected Decimal? _DiscTot;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? DiscTot
        {
            get
            {
                return this._DiscTot;
            }
            set
            {
                this._DiscTot = value;
            }
        }
        #endregion
        #region CuryDiscTot
        public abstract class curyDiscTot : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryDiscTot;
        [PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.discTot))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Discount Total", Enabled = true)]
        public virtual Decimal? CuryDiscTot
        {
            get
            {
                return this._CuryDiscTot;
            }
            set
            {
                this._CuryDiscTot = value;
            }
        }
        #endregion

        #region CuryChargeAmt
        public abstract class curyChargeAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryChargeAmt;
        [PXCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.chargeAmt))]
        [PXUIField(DisplayName = "Finance Charges", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public virtual Decimal? CuryChargeAmt
        {
            get
            {
                return this._CuryChargeAmt;
            }
            set
            {
                this._CuryChargeAmt = value;
            }
        }
        #endregion
        #region ChargeAmt
        public abstract class chargeAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _ChargeAmt;
        [PXDecimal(4)]
        public virtual Decimal? ChargeAmt
        {
            get
            {
                return this._ChargeAmt;
            }
            set
            {
                this._ChargeAmt = value;
            }
        }
        #endregion
		#region DocDesc
		public abstract class docDesc : PX.Data.IBqlField
		{
		}
		protected String _DocDesc;
		[PXDBString(150, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String DocDesc
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
		#region DocClass
		public abstract class docClass : PX.Data.IBqlField
		{
		}
		[PXString(1, IsFixed = true)]
		public virtual string DocClass
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return ARDocType.DocClass(_DocType);
			}
			set
			{
			}
		}
		#endregion
		#region BatchNbr
		public abstract class batchNbr : PX.Data.IBqlField
		{
		}
		protected String _BatchNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleAR>>>))]
		public virtual String BatchNbr
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
		public abstract class batchSeq : PX.Data.IBqlField
		{
		}
		protected Int16? _BatchSeq;
		[PXDBShort()]
		[PXDefault((short)0)]
		public virtual Int16? BatchSeq
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
		public abstract class status : PX.Data.IBqlField
		{
		}
		protected String _Status;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(ARDocStatus.Hold)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[ARDocStatus.List()]
		public virtual String Status
		{
            [PXDependsOnFields(typeof(voided), typeof(scheduled), typeof(released), typeof(hold), typeof(openDoc))]
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
		public abstract class released : PX.Data.IBqlField
		{
		}
		protected Boolean? _Released;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Released
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
		public abstract class openDoc : PX.Data.IBqlField
		{
		}
		protected Boolean? _OpenDoc;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? OpenDoc
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
		public abstract class hold : PX.Data.IBqlField
		{
		}
		protected Boolean? _Hold;
		[PXDBBool()]
		[PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.Visible)]
		[PXDefault(true, typeof(ARSetup.holdEntry))]
		public virtual Boolean? Hold
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
		public abstract class scheduled : PX.Data.IBqlField
		{
		}
		protected Boolean? _Scheduled;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Scheduled
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
		public abstract class voided : PX.Data.IBqlField
		{
		}
		protected Boolean? _Voided;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Voided
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
		public abstract class noteID : PX.Data.IBqlField
		{
		}
		protected Int64? _NoteID;
		[PXNote()]
		public virtual Int64? NoteID
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
		#region ClosedFinPeriodID
		public abstract class closedFinPeriodID : PX.Data.IBqlField
		{
		}
		protected String _ClosedFinPeriodID;
		[FinPeriodID()]
		[PXUIField(DisplayName = "Closed Period", Visibility = PXUIVisibility.Invisible)]
		public virtual String ClosedFinPeriodID
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
		public abstract class closedTranPeriodID : PX.Data.IBqlField
		{
		}
		protected String _ClosedTranPeriodID;
		[FinPeriodID()]
		[PXUIField(DisplayName = "Closed Period", Visibility = PXUIVisibility.Invisible)]
		public virtual String ClosedTranPeriodID
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
		public abstract class rGOLAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _RGOLAmt;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? RGOLAmt
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
        #region CuryRoundDiff
        public abstract class curyRoundDiff : IBqlField { }
        [PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.roundDiff), BaseCalc = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Rounding Diff.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public decimal? CuryRoundDiff
        {
            get;
            set;
        }
        #endregion
        #region RoundDiff
        public abstract class roundDiff : IBqlField { }
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public decimal? RoundDiff
        {
            get;
            set;
        }
        #endregion
		#region Payable
		public virtual Boolean? Payable
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return ARDocType.Payable(this._DocType);
			}
			set
			{
			}
		}
		#endregion
		#region Paying
		public virtual Boolean? Paying
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return (ARDocType.Payable(this._DocType) == false);
			}
			set
			{
			}
		}
		#endregion
		#region SortOrder
		public virtual Int16? SortOrder
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return ARDocType.SortOrder(this._DocType);
			}
			set
			{
			}
		}
		#endregion
		#region SignBalance
		public virtual Decimal? SignBalance
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return ARDocType.SignBalance(this._DocType);
			}
			set
			{
			}
		}
		#endregion
		#region SignAmount
		public virtual Decimal? SignAmount
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return ARDocType.SignAmount(this._DocType);
			}
			set
			{
			}
		}
		#endregion
		#region Methods
		protected virtual void SetStatus()
		{
			if (this._Voided != null && (bool)this._Voided)
			{
				this._Status = ARDocStatus.Voided;
			}
			else if (this._Hold != null && (bool)this._Hold)
			{
				this._Status = ARDocStatus.Hold;
			}
			else if (this._Scheduled != null && (bool)this._Scheduled)
			{
				this._Status = ARDocStatus.Scheduled;
			}
			else if (this._Released != null && !(bool)this._Released)
			{
				this._Status = ARDocStatus.Balanced;
			}
			else if (this._OpenDoc != null && (bool)this._OpenDoc)
			{
				this._Status = ARDocStatus.Open;
			}
			else if (this._OpenDoc != null)
			{
				this._Status = ARDocStatus.Closed;
			}

		}
		#endregion
		#region ScheduleID
		public abstract class scheduleID : IBqlField
		{
		}
		protected string _ScheduleID;
		[PXDBString(10, IsUnicode = true)]
		public virtual string ScheduleID
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
		public abstract class impRefNbr : PX.Data.IBqlField
		{
		}
		protected String _ImpRefNbr;
		[PXDBString(15, IsUnicode = true)]
		public virtual String ImpRefNbr
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
		public abstract class statementDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _StatementDate;
		[PXDBDate()]
		public virtual DateTime? StatementDate
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
		public abstract class salesPersonID : PX.Data.IBqlField
		{
		}
		protected Int32? _SalesPersonID;
		[SalesPerson()]
		[PXDefault(typeof(Search<CustDefSalesPeople.salesPersonID, Where<CustDefSalesPeople.bAccountID, Equal<Current<ARRegister.customerID>>, And<CustDefSalesPeople.locationID, Equal<Current<ARRegister.customerLocationID>>, And<CustDefSalesPeople.isDefault, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? SalesPersonID
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
		#region IsTaxValid
		public abstract class isTaxValid : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Tax is up to date", Enabled = false)]
		public virtual Boolean? IsTaxValid
		{
			get;
			set;
		}
		#endregion
		#region IsTaxPosted
		public abstract class isTaxPosted : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Tax is posted/commited to the external Tax Engine(Avalara)", Enabled = false)]
		public virtual Boolean? IsTaxPosted
		{
			get;
			set;
		}
		#endregion
		#region IsTaxSaved
		public abstract class isTaxSaved : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Tax is saved in external Tax Engine(Avalara)", Enabled = false)]
		public virtual Boolean? IsTaxSaved
		{
			get;
			set;
		}
		#endregion
		#region OrigDocType
		public abstract class origDocType : PX.Data.IBqlField
		{
		}
		protected String _OrigDocType;
		[PXDBString(3, IsFixed = true)]
		[ARDocType.List()]
		[PXUIField(DisplayName = "Orig. Doc. Type")]
		public virtual String OrigDocType
		{
			get
			{
				return this._OrigDocType;
			}
			set
			{
				this._OrigDocType = value;
			}
		}
		#endregion
		#region OrigRefNbr
		public abstract class origRefNbr : PX.Data.IBqlField
		{
		}
		protected String _OrigRefNbr;
		[PXDBString(15, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Orig. Ref. Nbr.")]
		public virtual String OrigRefNbr
		{
			get
			{
				return this._OrigRefNbr;
			}
			set
			{
				this._OrigRefNbr = value;
			}
		}
		#endregion

		internal string WarningMessage { get; set; }
	}

	[PXProjection(typeof(Select2<ARRegister,
		InnerJoin<Customer, On<Customer.bAccountID, Equal<ARRegister.customerID>>>>))]
	[PXBreakInheritance]
    [Serializable]
	public partial class ARRegisterAccess : Customer
	{
		#region DocType
		public abstract class docType : PX.Data.IBqlField
		{
		}
		protected String _DocType;
		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(ARRegister.docType))]
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
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(ARRegister.refNbr))]
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
		#region Scheduled
		public abstract class scheduled : PX.Data.IBqlField
		{
		}
		protected Boolean? _Scheduled;
		[PXDBBool(BqlField = typeof(ARRegister.scheduled))]
		public virtual Boolean? Scheduled
		{
			get
			{
				return this._Scheduled;
			}
			set
			{
				this._Scheduled = value;
			}
		}
		#endregion
		#region ScheduleID
		public abstract class scheduleID : IBqlField
		{
		}
		protected string _ScheduleID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(ARRegister.scheduleID))]
		public virtual string ScheduleID
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
	}
}
