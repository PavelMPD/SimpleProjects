using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.CR;
using APQuickCheck = PX.Objects.AP.Standalone.APQuickCheck;

namespace PX.Objects.AP
{
	public class APDocType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Invoice, CreditAdj, DebitAdj, Check, VoidCheck, Prepayment, Refund, QuickCheck, VoidQuickCheck },
				new string[] { Messages.Invoice, Messages.CreditAdj, Messages.DebitAdj, Messages.Check, Messages.VoidCheck, Messages.Prepayment, Messages.Refund, Messages.QuickCheck, Messages.VoidQuickCheck }) { }
		}

        /// <summary>
        /// Defines a Selector of the AP Document types with shorter description.<br/>
        /// In the screens displayed as combo-box.<br/>
        /// Mostly used in the reports.<br/>
        /// </summary>
		public class PrintListAttribute : PXStringListAttribute
		{
			public PrintListAttribute()
				: base(
				new string[] { Invoice, CreditAdj, DebitAdj, Check, VoidCheck, Prepayment, Refund, QuickCheck, VoidQuickCheck },
				new string[] { Messages.PrintInvoice, Messages.PrintCreditAdj, Messages.PrintDebitAdj, Messages.PrintCheck, Messages.PrintVoidCheck, Messages.PrintPrepayment, Messages.PrintRefund, Messages.PrintQuickCheck, Messages.PrintVoidQuickCheck }) { }
		}

		public const string Invoice = "INV";
		public const string CreditAdj = "ACR";
		public const string DebitAdj = "ADR";
		public const string Check = "CHK";
		public const string VoidCheck = "VCK";
		public const string Prepayment = "PPM";
		public const string Refund = "REF";
		public const string QuickCheck = "QCK";
		public const string VoidQuickCheck = "VQC";

		public class invoice : Constant<string>
		{
			public invoice() : base(Invoice) { ;}
		}

		public class creditAdj : Constant<string>
		{
			public creditAdj() : base(CreditAdj) { ;}
		}

		public class debitAdj : Constant<string>
		{
			public debitAdj() : base(DebitAdj) { ;}
		}

		public class check : Constant<string>
		{
			public check() : base(Check) { ;}
		}

		public class voidCheck : Constant<string>
		{
			public voidCheck() : base(VoidCheck) { ;}
		}

		public class prepayment : Constant<string>
		{
			public prepayment() : base(Prepayment) { ;}
		}

		public class refund : Constant<string>
		{
			public refund() : base(Refund) { ;}
		}

		public class quickCheck : Constant<string>
		{
			public quickCheck() : base(QuickCheck) { ;}
		}

		public class voidQuickCheck : Constant<string>
		{
			public voidQuickCheck() : base(VoidQuickCheck) { ;}
		}

		public static string DocClass(string DocType)
		{
			switch (DocType)
			{
				case Invoice:
				case CreditAdj:
				case DebitAdj:
				case QuickCheck:
				case VoidQuickCheck:
					return "N";
				case Check:
				case VoidCheck:
				case Refund:
					return "P";
				case Prepayment:
					return "U";
				default:
					return null;
			}
		}

		public static bool? Payable(string DocType)
		{
			switch (DocType)
			{
				case Invoice:
				case CreditAdj:
					return true;
				case Check:
				case DebitAdj:
				case VoidCheck:
				case Prepayment:
				case Refund:
				case QuickCheck:
				case VoidQuickCheck:
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
				case CreditAdj:
				case QuickCheck:
					return 0;
				case Prepayment:
					return 1;
				case DebitAdj:
					return 2;
				case Check:
					return 3;
				case VoidCheck:
				case VoidQuickCheck:
					return 4;
				case Refund:
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
				case CreditAdj:
					return 1m;
				case DebitAdj:
				case Check:
				case VoidCheck:
					return -1m;
				case Prepayment:
					return -1m;
				case QuickCheck:
				case VoidQuickCheck:
					return 0m;
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
                case CreditAdj:
                case QuickCheck:
                    return 1m;
                case DebitAdj:
                case Check:
                case VoidCheck:
                case VoidQuickCheck:
                    return -1m;
                case Prepayment:
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
					case CreditAdj:
					case QuickCheck:
						return "D";
					case DebitAdj:
					case VoidQuickCheck:
						return "C";
					//Payment Types
					case Check:
					case Prepayment:
					case VoidCheck:
						return "D";
					case Refund:
						return "C";
					default:
						return "D";
				}
			}
	}

	public class APDocStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Hold, Balanced, Voided, Scheduled, Open, Closed, Printed, Prebooked },
				new string[] { Messages.Hold, Messages.Balanced, Messages.Voided, Messages.Scheduled, Messages.Open, Messages.Closed, Messages.Printed,Messages.Prebooked }) { ; }
		}

		public const string Hold = "H";
		public const string Balanced = "B";
		public const string Voided = "V";
		public const string Scheduled = "S";
		public const string Open = "N";
		public const string Closed = "C";
		public const string Printed = "P";
		public const string Prebooked = "K";

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

		public class printed : Constant<string>
		{
			public printed() : base(Printed) { ;}
		}

		public class prebooked : Constant<string>
		{
			public prebooked() : base(Prebooked) { ;}
		}
	}

    [Serializable]
	public partial class APAROrd : PX.Data.IBqlTable
	{
		#region Ord
		public abstract class ord : PX.Data.IBqlField
		{
		}
		protected Int16? _Ord;
		[PXDBShort(IsKey = true)]
		public virtual Int16? Ord
		{
			get
			{
				return this._Ord;
			}
			set
			{
				this._Ord = value;
			}
		}
		#endregion
	}

	[PXCacheName(Messages.Document)]
	[System.SerializableAttribute()]
	[PXPrimaryGraph(new Type[] {
		typeof(APQuickCheckEntry),
		typeof(TX.TXInvoiceEntry),
		typeof(APInvoiceEntry), 
		typeof(APPaymentEntry)
	},
		new Type[] {
		typeof(Select<APQuickCheck, 
			Where<APQuickCheck.docType, Equal<Current<APRegister.docType>>, 
			And<APQuickCheck.refNbr, Equal<Current<APRegister.refNbr>>>>>),
		typeof(Select<APInvoice, 
			Where<APInvoice.docType, Equal<Current<APRegister.docType>>, 
			And<APInvoice.refNbr, Equal<Current<APRegister.refNbr>>,
			And<Where<APInvoice.released, Equal<False>, And<APInvoice.origModule, Equal<GL.BatchModule.moduleTX>>>>>>>),
		typeof(Select<APInvoice, 
			Where<APInvoice.docType, Equal<Current<APRegister.docType>>, 
			And<APInvoice.refNbr, Equal<Current<APRegister.refNbr>>>>>),
		typeof(Select<APPayment, 
			Where<APPayment.docType, Equal<Current<APRegister.docType>>, 
			And<APPayment.refNbr, Equal<Current<APRegister.refNbr>>>>>)
		})]
	public partial class APRegister : PX.Data.IBqlTable
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
		#region Hidden
		public abstract class hidden : PX.Data.IBqlField
		{
		}
		protected bool? _Hidden = false;
		[PXBool]
		[PXDefault(false)]
		public virtual bool? Hidden
		{
			get
			{
				return _Hidden;
			}
			set
			{
				_Hidden = value;
			}
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.IBqlField
		{
		}
		protected Int32? _BranchID;
		[GL.Branch()]
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
        #region Passed
        public virtual bool? Passed
        {
            get;
            set;
        }
        #endregion
		#region DocType
		public abstract class docType : PX.Data.IBqlField
		{
		}
		protected String _DocType;
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[APDocType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		[PXFieldDescription]
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
		[APDocType.PrintList()]
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
		[PXSelector(typeof(Search<APRegister.refNbr, Where<APRegister.docType, Equal<Optional<APRegister.docType>>>>), Filterable = true)]
		[PXFieldDescription]
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
		[PXDefault(GL.BatchModule.AP)]
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
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.IBqlField
		{
		}
		protected String _TranPeriodID;
		[FinPeriodID(typeof(APRegister.docDate))]
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
		[APOpenPeriod(typeof(APRegister.docDate))]
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
		#region VendorID
		public abstract class vendorID : PX.Data.IBqlField
		{
		}
		protected Int32? _VendorID;
		[VendorActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true)]
		[PXDefault()]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region VendorLocationID
		public abstract class vendorLocationID : PX.Data.IBqlField
		{
		}
		protected Int32? _VendorLocationID;
		[LocationID(
			typeof(Where<Location.bAccountID, Equal<Optional<APRegister.vendorID>>,
				And<Location.isActive, Equal<boolTrue>,
				And<MatchWithBranch<Location.vBranchID>>>>),
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
		public virtual Int32? VendorLocationID
		{
			get
			{
				return this._VendorLocationID;
			}
			set
			{
				this._VendorLocationID = value;
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
		#region APAccountID
		public abstract class aPAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _APAccountID;
		[PXDefault]
		[Account(typeof(APRegister.branchID), typeof(Search<Account.accountID,
					Where2<Match<Current<AccessInfo.userName>>,
						 And<Account.active, Equal<True>,
                         And<Account.isCashAccount, Equal<False>, 
						 And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
						  Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>>), DisplayName = "AP Account")]
		public virtual Int32? APAccountID
		{
			get
			{
				return this._APAccountID;
			}
			set
			{
				this._APAccountID = value;
			}
		}
		#endregion
		#region APSubID
		public abstract class aPSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _APSubID;
		[PXDefault]
		[SubAccount(typeof(APRegister.aPAccountID), DescriptionField = typeof(Sub.description), DisplayName = "AP Subaccount", Visibility = PXUIVisibility.Visible)]
		public virtual Int32? APSubID
		{
			get
			{
				return this._APSubID;
			}
			set
			{
				this._APSubID = value;
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
		[CurrencyInfo(ModuleCode = "AP")]
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
		[PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.origDocAmt))]
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
		[PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.docBal), BaseCalc = false)]
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
        [PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.discTot))]
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
		#region CuryOrigDiscAmt
		public abstract class curyOrigDiscAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryOrigDiscAmt;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.origDiscAmt))]
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
		[PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.discTaken))]
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
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.discBal), BaseCalc = false)]
		[PXUIField(DisplayName = "Cash Discount Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
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
		#region CuryOrigWhTaxAmt
		public abstract class curyOrigWhTaxAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryOrigWhTaxAmt;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.origWhTaxAmt))]
		[PXUIField(DisplayName = "With. Tax", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? CuryOrigWhTaxAmt
		{
			get
			{
				return this._CuryOrigWhTaxAmt;
			}
			set
			{
				this._CuryOrigWhTaxAmt = value;
			}
		}
		#endregion
		#region OrigWhTaxAmt
		public abstract class origWhTaxAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _OrigWhTaxAmt;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OrigWhTaxAmt
		{
			get
			{
				return this._OrigWhTaxAmt;
			}
			set
			{
				this._OrigWhTaxAmt = value;
			}
		}
		#endregion
		#region CuryWhTaxBal
		public abstract class curyWhTaxBal : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryWhTaxBal;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.whTaxBal), BaseCalc = false)]
		public virtual Decimal? CuryWhTaxBal
		{
			get
			{
				return this._CuryWhTaxBal;
			}
			set
			{
				this._CuryWhTaxBal = value;
			}
		}
		#endregion
		#region WhTaxBal
		public abstract class whTaxBal : PX.Data.IBqlField
		{
		}
		protected Decimal? _WhTaxBal;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? WhTaxBal
		{
			get
			{
				return this._WhTaxBal;
			}
			set
			{
				this._WhTaxBal = value;
			}
		}
		#endregion
		#region CuryTaxWheld
		public abstract class curyTaxWheld : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTaxWheld;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.taxWheld))]
		public virtual Decimal? CuryTaxWheld
		{
			get
			{
				return this._CuryTaxWheld;
			}
			set
			{
				this._CuryTaxWheld = value;
			}
		}
		#endregion
		#region TaxWheld
		public abstract class taxWheld : PX.Data.IBqlField
		{
		}
		protected Decimal? _TaxWheld;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TaxWheld
		{
			get
			{
				return this._TaxWheld;
			}
			set
			{
				this._TaxWheld = value;
			}
		}
		#endregion
        #region CuryChargeAmt
        public abstract class curyChargeAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryChargeAmt;
        [PXCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.chargeAmt))]
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
		[PXDBString(60, IsUnicode = true)]
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
				return APDocType.DocClass(this._DocType);
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
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleAP>>>))]
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
		#region PrebookBatchNbr
		public abstract class prebookBatchNbr : PX.Data.IBqlField
		{
		}
		protected String _PrebookBatchNbr;
		[PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Pre-Releasing Batch Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXSelector(typeof(Batch.batchNbr))]
		public virtual String PrebookBatchNbr
		{
			get
			{
				return this._PrebookBatchNbr;
			}
			set
			{
				this._PrebookBatchNbr = value;
			}
		}
		#endregion
		#region VoidBatchNbr
		public abstract class voidBatchNbr : PX.Data.IBqlField
		{
		}
		protected String _VoidBatchNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Void Batch Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXSelector(typeof(Batch.batchNbr))]
		public virtual String VoidBatchNbr
		{
			get
			{
				return this._VoidBatchNbr;
			}
			set
			{
				this._VoidBatchNbr = value;
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
		[PXDefault(APDocStatus.Hold)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[APDocStatus.List()]
		public virtual String Status
		{
			[PXDependsOnFields(typeof(printed), typeof(voided), typeof(scheduled), typeof(released), typeof(hold), typeof(openDoc),typeof(prebooked))]
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
		[PXDefault(true, typeof(APSetup.holdEntry))]
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
		[PXUIField(DisplayName="Void", Visible=false)]
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
		#region Printed
		public abstract class printed : PX.Data.IBqlField
		{
		}
		protected Boolean? _Printed;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Printed
		{
			get
			{
				return this._Printed;
			}
			set
			{
				this._Printed = value;
				this.SetStatus();
			}
		}
		#endregion
		#region Prebooked
		public abstract class prebooked : PX.Data.IBqlField
		{
		}
		protected Boolean? _Prebooked;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Prebooked
		{
			get
			{
				return this._Prebooked;
			}
			set
			{
				this._Prebooked = value;
				this.SetStatus();
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.IBqlField
		{
		}
		protected Int64? _NoteID;
		[PXNote(DescriptionField = typeof(APRegister.refNbr))]
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
		[PXDBDecimal(4)]
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
        public abstract class curyRoundDiff : IBqlField {}
        [PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.roundDiff), BaseCalc = false)]
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
				return APDocType.Payable(this._DocType);
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
				return (APDocType.Payable(this._DocType) == false);
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
				return APDocType.SortOrder(this._DocType);
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
				return APDocType.SignBalance(this._DocType);
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
				return APDocType.SignAmount(this._DocType);
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
				this._Status = APDocStatus.Voided;
			}
			else if (this._Hold != null && (bool)this._Hold)
			{
				this._Status = APDocStatus.Hold;
			}
			else if (this._Scheduled != null && (bool)this._Scheduled)
			{
				this._Status = APDocStatus.Scheduled;
			}
			else if (this._Released != null && !(bool)this._Released)
			{
				if (this._Printed != null && (bool)this._Printed && this._DocType == APDocType.Check)
				{
					this._Status = APDocStatus.Printed;
				}
				else if (this.Prebooked != null && this.Prebooked == true)
				{
					this._Status = APDocStatus.Prebooked;
				}
				else
				{
					this._Status = APDocStatus.Balanced;
				}
			}
			else if (this._OpenDoc != null && (bool)this._OpenDoc)
			{
				this._Status = APDocStatus.Open;
			}
			else if (this._OpenDoc != null)
			{
				this._Status = APDocStatus.Closed;
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
		[APDocType.List()]
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
		#region Released
		public abstract class releasedOrPrebooked : PX.Data.IBqlField
		{
		}
		
		[PXBool()]		
		public virtual Boolean? ReleasedOrPrebooked
		{

			[PXDependsOnFields(typeof(released),typeof(prebooked))]
			get
			{
				return ((this.Released==true) || (this.Prebooked == true));
			}
			set
			{
				
			}
		}
		#endregion

		internal string WarningMessage { get; set; }
	}

	[PXProjection(typeof(Select2<APRegister,
		InnerJoin<Vendor, On<Vendor.bAccountID, Equal<APRegister.vendorID>>>>))]
	[PXBreakInheritance]
    [Serializable]
	public partial class APRegisterAccess : Vendor
	{
		#region DocType
		public abstract class docType : PX.Data.IBqlField
		{
		}
		protected String _DocType;
		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(APRegister.docType))]
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
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(APRegister.refNbr))]
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
		[PXDBBool(BqlField = typeof(APRegister.scheduled))]
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
		[PXDBString(10, IsUnicode = true, BqlField = typeof(APRegister.scheduleID))]
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
