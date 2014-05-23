namespace PX.Objects.AP
{
	using System;
	using PX.Data;
	using PX.Objects.GL;
	using PX.Objects.CM;
	using PX.Objects.CS;
	using PX.Objects.TX;
	
	[System.SerializableAttribute()]
	[PXPrimaryGraph(
		new Type[] { typeof(APPaymentEntry)},
		new Type[] { typeof(Select<APPayment, 
			Where<APPayment.docType, Equal<Current<APAdjust.adjgDocType>>,
			And<APPayment.refNbr, Equal<Current<APAdjust.adjgRefNbr>>>>>)
		})]
	[PXCacheName(Messages.APAdjust)]    
	public partial class APAdjust : PX.Data.IBqlTable, IAdjustment
	{
		#region Selected
		public abstract class selected : PX.Data.IBqlField
		{
		}
		protected bool? _Selected = false;
		[PXBool()]
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
		#region SeparateCheck
		public abstract class separateCheck : PX.Data.IBqlField
		{
		}
		protected Boolean? _SeparateCheck;
		[PXBool()]
		[PXUIField(DisplayName = "Pay Separately", Visibility = PXUIVisibility.Visible)]
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
		#region VendorID
		public abstract class vendorID : PX.Data.IBqlField
		{
		}
		protected Int32? _VendorID;
		[Vendor(Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXDBDefault(typeof(APPayment.vendorID))]
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
		#region AdjgDocType
		public abstract class adjgDocType : PX.Data.IBqlField
		{
		}
		protected String _AdjgDocType;
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
		[PXDBDefault(typeof(APPayment.docType))]
		[PXUIField(DisplayName = "AdjgDocType", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual String AdjgDocType
		{
			get
			{
				return this._AdjgDocType;
			}
			set
			{
				this._AdjgDocType = value;
			}
		}
		#endregion
		#region PrintAdjgDocType
		public abstract class printAdjgDocType : PX.Data.IBqlField
		{
		}
		[PXString(3, IsFixed = true)]
		[APDocType.PrintList()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.Visible, Enabled = true)]
		public virtual String PrintAdjgDocType
		{
			get
			{
				return this._AdjgDocType;
			}
			set
			{
			}
		}
		#endregion
		#region AdjgRefNbr
		public abstract class adjgRefNbr : PX.Data.IBqlField
		{
		}
		protected String _AdjgRefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(APPayment.refNbr))]
		[PXUIField(DisplayName = "AdjgRefNbr", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(Select<APPayment,Where<APPayment.docType,Equal<Current<APAdjust.adjgDocType>>, And<APPayment.refNbr,Equal<Current<APAdjust.adjgRefNbr>>, And<APPayment.lineCntr,Equal<Current<APAdjust.adjNbr>>>>>>))]
		public virtual String AdjgRefNbr
		{
			get
			{
				return this._AdjgRefNbr;
			}
			set
			{
				this._AdjgRefNbr = value;
			}
		}
		#endregion
		#region AdjgBranchID
		public abstract class adjgBranchID : PX.Data.IBqlField
		{
		}
		protected Int32? _AdjgBranchID;
		[Branch(typeof(APPayment.branchID))]
		public virtual Int32? AdjgBranchID
		{
			get
			{
				return this._AdjgBranchID;
			}
			set
			{
				this._AdjgBranchID = value;
			}
		}
		#endregion
		#region AdjdCuryInfoID
		public abstract class adjdCuryInfoID : PX.Data.IBqlField
		{
		}
		protected Int64? _AdjdCuryInfoID;
		[PXDBLong()]
		[PXDefault()]
		[CurrencyInfo(ModuleCode = "AP", CuryIDField = "AdjdCuryID", Enabled = false)]
		public virtual Int64? AdjdCuryInfoID
		{
			get
			{
				return this._AdjdCuryInfoID;
			}
			set
			{
				this._AdjdCuryInfoID = value;
			}
		}
		#endregion
		#region AdjdDocType
		public abstract class adjdDocType : PX.Data.IBqlField
		{
		}
		protected String _AdjdDocType;
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
		[PXDefault(APDocType.Invoice)]
		[PXUIField(DisplayName = "Document Type", Visibility=PXUIVisibility.Visible)]
		[APInvoiceType.AdjdList()]
		public virtual String AdjdDocType
		{
			get
			{
				return this._AdjdDocType;
			}
			set
			{
				this._AdjdDocType = value;
			}
		}
		#endregion
		#region PrintAdjdDocType
		public abstract class printAdjdDocType : PX.Data.IBqlField
		{
		}
		[PXString(3, IsFixed = true)]
		[APDocType.PrintList()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.Visible, Enabled = true)]
		public virtual String PrintAdjdDocType
		{
			get
			{
				return this._AdjdDocType;
			}
			set
			{
			}
		}
		#endregion
		#region AdjdRefNbr
		[PXHidden()]
        [Serializable]
		public partial class APRegister : AP.APRegister
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

		[PXHidden()]
		[PXProjection(typeof(Select2<APRegister, LeftJoin<Standalone.APInvoice, On<Standalone.APInvoice.docType, Equal<APRegister.docType>, And<Standalone.APInvoice.refNbr, Equal<APRegister.refNbr>>>>>))]
        [Serializable]
		public partial class APInvoice : APRegister
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
			#region CustomerID
			public new abstract class vendorID : PX.Data.IBqlField
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
			#region DocDate
			public new abstract class docDate : PX.Data.IBqlField
			{
			}
			#endregion
			#region FinPeriodID
			public new abstract class finPeriodID : PX.Data.IBqlField
			{
			}
			#endregion
			#region DueDate
			public abstract class dueDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _DueDate;
			[PXDBDate(BqlField = typeof(Standalone.APInvoice.dueDate))]
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
			#region InvoiceNbr
			public abstract class invoiceNbr : PX.Data.IBqlField
			{
			}
			protected String _InvoiceNbr;
			[PXDBString(40, IsUnicode = true, BqlField = typeof(Standalone.APInvoice.invoiceNbr))]
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
		}
		public abstract class adjdRefNbr : PX.Data.IBqlField
		{
		}
		protected String _AdjdRefNbr;
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible)]
		[APInvoiceType.AdjdRefNbr(typeof(Search2<APInvoice.refNbr, 
			LeftJoin<APAdjust, On<APAdjust.adjdDocType, Equal<APInvoice.docType>, And<APAdjust.adjdRefNbr, Equal<APInvoice.refNbr>, And<APAdjust.released, Equal<boolFalse>, And<Where<APAdjust.adjgDocType, NotEqual<Current<APPayment.docType>>, Or<APAdjust.adjgRefNbr, NotEqual<Current<APPayment.refNbr>>>>>>>>, 
			LeftJoin<APPayment, On<APPayment.docType, Equal<APInvoice.docType>,
				And<APPayment.refNbr, Equal<APInvoice.refNbr>, And<
				Where<APPayment.docType, Equal<APDocType.prepayment>, Or<APPayment.docType, Equal<APDocType.debitAdj>>>>>>>>,
			Where<APInvoice.vendorID, Equal<Optional<APPayment.vendorID>>, And<APInvoice.docType, Equal<Optional<APAdjust.adjdDocType>>, And2<Where<APInvoice.released, Equal<True>, Or<APInvoice.prebooked, Equal<True>>>, And<APInvoice.openDoc, Equal<boolTrue>, And<APAdjust.adjgRefNbr, IsNull,
				And2<Where<APPayment.refNbr, IsNull, And<Current<APPayment.docType>, NotEqual<APDocType.refund>, Or<APPayment.refNbr, IsNotNull, And<Current<APPayment.docType>, Equal<APDocType.refund>, Or<APPayment.docType, Equal<APDocType.debitAdj>, And<Current<APPayment.docType>, Equal<APDocType.check>, Or<APPayment.docType, Equal<APDocType.debitAdj>, And<Current<APPayment.docType>, Equal<APDocType.voidCheck>>>>>>>>>,
				And<Where<APInvoice.docDate, LessEqual<Current<APPayment.adjDate>>, And<APInvoice.finPeriodID, LessEqual<Current<APPayment.adjFinPeriodID>>, Or<Current<APPayment.adjFinPeriodID>, IsNull, Or<Current<APPayment.docType>, Equal<APDocType.check>, And<Current<APSetup.earlyChecks>, Equal<True>, Or<Current<APPayment.docType>, Equal<APDocType.voidCheck>, And<Current<APSetup.earlyChecks>, Equal<True>>>>>>>>
				>>>>>>>>), Filterable = true)]
		public virtual String AdjdRefNbr
		{
			get
			{
				return this._AdjdRefNbr;
			}
			set
			{
				this._AdjdRefNbr = value;
			}
		}
		#endregion
		#region AdjdBranchID
		public abstract class adjdBranchID : PX.Data.IBqlField
		{
		}
		protected Int32? _AdjdBranchID;
		[Branch(null)]
		public virtual Int32? AdjdBranchID
		{
			get
			{
				return this._AdjdBranchID;
			}
			set
			{
				this._AdjdBranchID = value;
			}
		}
		#endregion
		#region AdjNbr
		public abstract class adjNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _AdjNbr;
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Adjustment Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXDefault(typeof(APPayment.lineCntr))]
		public virtual Int32? AdjNbr
		{
			get
			{
				return this._AdjNbr;
			}
			set
			{
				this._AdjNbr = value;
			}
		}
		#endregion
		#region StubNbr
		public abstract class stubNbr : PX.Data.IBqlField
		{
		}
		protected String _StubNbr;
		[PXDBString(15, IsUnicode = true)]
		public virtual String StubNbr
		{
			get
			{
				return this._StubNbr;
			}
			set
			{
				this._StubNbr = value;
			}
		}
		#endregion
		#region AdjBatchNbr
		public abstract class adjBatchNbr : PX.Data.IBqlField
		{
		}
		protected String _AdjBatchNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName="Batch Number", Visibility=PXUIVisibility.Visible, Visible=true, Enabled=false)]
		public virtual String AdjBatchNbr
		{
			get
			{
				return this._AdjBatchNbr;
			}
			set
			{
				this._AdjBatchNbr = value;
			}
		}
		#endregion
		#region VoidAdjNbr
		public abstract class voidAdjNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _VoidAdjNbr;
		[PXDBInt()]
		public virtual Int32? VoidAdjNbr
		{
			get
			{
				return this._VoidAdjNbr;
			}
			set
			{
				this._VoidAdjNbr = value;
			}
		}
		#endregion
		#region AdjdOrigCuryInfoID
		public abstract class adjdOrigCuryInfoID : PX.Data.IBqlField
		{
		}
		protected Int64? _AdjdOrigCuryInfoID;
		[PXDBLong()]
		[PXDefault()]
		[CurrencyInfo(ModuleCode = "AP", CuryIDField = "AdjdOrigCuryID")]
		public virtual Int64? AdjdOrigCuryInfoID
		{
			get
			{
				return this._AdjdOrigCuryInfoID;
			}
			set
			{
				this._AdjdOrigCuryInfoID = value;
			}
		}
		#endregion
		#region AdjgCuryInfoID
		public abstract class adjgCuryInfoID : PX.Data.IBqlField
		{
		}
		protected Int64? _AdjgCuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(typeof(APPayment.curyInfoID), CuryIDField = "AdjgCuryID")]
		public virtual Int64? AdjgCuryInfoID
		{
			get
			{
				return this._AdjgCuryInfoID;
			}
			set
			{
				this._AdjgCuryInfoID = value;
			}
		}
		#endregion
		#region AdjgDocDate
		public abstract class adjgDocDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _AdjgDocDate;
		[PXDBDate()]
		[PXDBDefault(typeof(APPayment.adjDate))]
		public virtual DateTime? AdjgDocDate
		{
			get
			{
				return this._AdjgDocDate;
			}
			set
			{
				this._AdjgDocDate = value;
			}
		}
		#endregion
		#region AdjgFinPeriodID
		public abstract class adjgFinPeriodID : PX.Data.IBqlField
		{
		}
		protected String _AdjgFinPeriodID;
		[GL.FinPeriodID()]
		[PXDBDefault(typeof(APPayment.adjFinPeriodID))]
		[PXUIField(DisplayName = "Application Period", Enabled = false)]
		public virtual String AdjgFinPeriodID
		{
			get
			{
				return this._AdjgFinPeriodID;
			}
			set
			{
				this._AdjgFinPeriodID = value;
			}
		}
		#endregion
		#region AdjgTranPeriodID
		public abstract class adjgTranPeriodID : PX.Data.IBqlField
		{
		}
		protected String _AdjgTranPeriodID;
		[GL.FinPeriodID()]
		[PXDBDefault(typeof(APPayment.adjTranPeriodID))]
		public virtual String AdjgTranPeriodID
		{
			get
			{
				return this._AdjgTranPeriodID;
			}
			set
			{
				this._AdjgTranPeriodID = value;
			}
		}
		#endregion
		#region AdjdDocDate
		public abstract class adjdDocDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _AdjdDocDate;
		[PXDBDate()]
		[PXDefault()]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.Visible, Enabled=false)]
		public virtual DateTime? AdjdDocDate
		{
			get
			{
				return this._AdjdDocDate;
			}
			set
			{
				this._AdjdDocDate = value;
			}
		}
		#endregion
		#region AdjdFinPeriodID
		public abstract class adjdFinPeriodID : PX.Data.IBqlField
		{
		}
		protected String _AdjdFinPeriodID;
		[FinPeriodID(typeof(APAdjust.adjdDocDate))]
		[PXUIField(DisplayName = "Post Period", Enabled = false)]
		public virtual String AdjdFinPeriodID
		{
			get
			{
				return this._AdjdFinPeriodID;
			}
			set
			{
				this._AdjdFinPeriodID = value;
			}
		}
		#endregion
		#region AdjdClosedFinPeriodID
		public abstract class adjdClosedFinPeriodID : PX.Data.IBqlField
		{
		}
		protected String _AdjdClosedFinPeriodID;
		[PXDBScalar(typeof(Search<APRegister.closedFinPeriodID, Where<APRegister.docType, Equal<APAdjust.adjdDocType>, And<APRegister.refNbr, Equal<APAdjust.adjdRefNbr>>>>))]
		[PXString()]
		public virtual String AdjdClosedFinPeriodID
		{
			get
			{
				return this._AdjdClosedFinPeriodID;
			}
			set
			{
				this._AdjdClosedFinPeriodID = value;
			}
		}
		#endregion
		#region AdjdTranPeriodID
		public abstract class adjdTranPeriodID : PX.Data.IBqlField
		{
		}
		protected String _AdjdTranPeriodID;
		[FinPeriodID(typeof(APAdjust.adjdDocDate))]
		public virtual String AdjdTranPeriodID
		{
			get
			{
				return this._AdjdTranPeriodID;
			}
			set
			{
				this._AdjdTranPeriodID = value;
			}
		}
		#endregion
		#region CuryAdjgDiscAmt
		public abstract class curyAdjgDiscAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryAdjgDiscAmt;
		[PXDBCurrency(typeof(APAdjust.adjgCuryInfoID), typeof(APAdjust.adjDiscAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cash Discount Taken", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryAdjgDiscAmt
		{
			get
			{
				return this._CuryAdjgDiscAmt;
			}
			set
			{
				this._CuryAdjgDiscAmt = value;
			}
		}
		#endregion
		#region CuryAdjgWhTaxAmt
		public abstract class curyAdjgWhTaxAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryAdjgWhTaxAmt;
		[PXDBCurrency(typeof(APAdjust.adjgCuryInfoID), typeof(APAdjust.adjWhTaxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "With. Tax", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryAdjgWhTaxAmt
		{
			get
			{
				return this._CuryAdjgWhTaxAmt;
			}
			set
			{
				this._CuryAdjgWhTaxAmt = value;
			}
		}
		#endregion
		#region CuryAdjgAmt
		public abstract class curyAdjgAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryAdjgAmt;
		[PXDBCurrency(typeof(APAdjust.adjgCuryInfoID), typeof(APAdjust.adjAmt), BaseCalc=false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount Paid", Visibility = PXUIVisibility.Visible)]
		//[PXFormula(null, typeof(SumCalc<APPayment.curyApplAmt>))]
        [PXUnboundFormula(typeof(Mult<APAdjust.adjgBalSign, APAdjust.curyAdjgAmt>), typeof(SumCalc<APPayment.curyApplAmt>))]
		public virtual Decimal? CuryAdjgAmt
		{
			get
			{
				return this._CuryAdjgAmt;
			}
			set
			{
				this._CuryAdjgAmt = value;
			}
		}
		#endregion
		#region AdjDiscAmt
		public abstract class adjDiscAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _AdjDiscAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? AdjDiscAmt
		{
			get
			{
				return this._AdjDiscAmt;
			}
			set
			{
				this._AdjDiscAmt = value;
			}
		}
		#endregion
		#region CuryAdjdDiscAmt
		public abstract class curyAdjdDiscAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryAdjdDiscAmt;
		[PXDBDecimal(4)]
		//[PXDBCurrency(typeof(APAdjust.adjdCuryInfoID), typeof(APAdjust.adjDiscAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryAdjdDiscAmt
		{
			get
			{
				return this._CuryAdjdDiscAmt;
			}
			set
			{
				this._CuryAdjdDiscAmt = value;
			}
		}
		#endregion
		#region AdjWhTaxAmt
		public abstract class adjWhTaxAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _AdjWhTaxAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? AdjWhTaxAmt
		{
			get
			{
				return this._AdjWhTaxAmt;
			}
			set
			{
				this._AdjWhTaxAmt = value;
			}
		}
		#endregion
		#region CuryAdjdWhTaxAmt
		public abstract class curyAdjdWhTaxAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryAdjdWhTaxAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryAdjdWhTaxAmt
		{
			get
			{
				return this._CuryAdjdWhTaxAmt;
			}
			set
			{
				this._CuryAdjdWhTaxAmt = value;
			}
		}
		#endregion
		#region AdjAmt
		public abstract class adjAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _AdjAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? AdjAmt
		{
			get
			{
				return this._AdjAmt;
			}
			set
			{
				this._AdjAmt = value;
			}
		}
		#endregion
		#region CuryAdjdAmt
		public abstract class curyAdjdAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryAdjdAmt;
		[PXDBDecimal(4)]
		//[PXDBCurrency(typeof(APAdjust.adjdCuryInfoID), typeof(APAdjust.adjAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryAdjdAmt
		{
			get
			{
				return this._CuryAdjdAmt;
			}
			set
			{
				this._CuryAdjdAmt = value;
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
			}
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.IBqlField
		{
		}
		protected Boolean? _Hold;
		[PXDBBool()]
		[PXDefault(false)]
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
			}
		}
		#endregion
		#region AdjdAPAcct
		public abstract class adjdAPAcct : PX.Data.IBqlField
		{
		}
		protected Int32? _AdjdAPAcct;
		[Account(SuppressCurrencyValidation = true)]
		[PXDefault()]
		public virtual Int32? AdjdAPAcct
		{
			get
			{
				return this._AdjdAPAcct;
			}
			set
			{
				this._AdjdAPAcct = value;
			}
		}
		#endregion
		#region AdjdAPSub
		public abstract class adjdAPSub : PX.Data.IBqlField
		{
		}
		protected Int32? _AdjdAPSub;
		[SubAccount()]
		[PXDefault()]
		public virtual Int32? AdjdAPSub
		{
			get
			{
				return this._AdjdAPSub;
			}
			set
			{
				this._AdjdAPSub = value;
			}
		}
		#endregion
		#region AdjdWhTaxAcctID
		public abstract class adjdWhTaxAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _AdjdWhTaxAcctID;
		[Account()]
		[PXDefault(typeof(Search2<APTaxTran.accountID, InnerJoin<Tax, On<Tax.taxID, Equal<APTaxTran.taxID>>>, Where<APTaxTran.tranType, Equal<Current<APAdjust.adjdDocType>>, And<APTaxTran.refNbr, Equal<Current<APAdjust.adjdRefNbr>>, And<Tax.taxType, Equal<CSTaxType.withholding>>>>, OrderBy<Asc<APTaxTran.taxID>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? AdjdWhTaxAcctID
		{
			get
			{
				return this._AdjdWhTaxAcctID;
			}
			set
			{
				this._AdjdWhTaxAcctID = value;
			}
		}
		#endregion
		#region AdjdWhTaxSubID
		public abstract class adjdWhTaxSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _AdjdWhTaxSubID;
		[SubAccount()]
		[PXDefault(typeof(Search2<APTaxTran.subID, InnerJoin<Tax, On<Tax.taxID, Equal<APTaxTran.taxID>>>, Where<APTaxTran.tranType, Equal<Current<APAdjust.adjdDocType>>, And<APTaxTran.refNbr, Equal<Current<APAdjust.adjdRefNbr>>, And<Tax.taxType, Equal<CSTaxType.withholding>>>>, OrderBy<Asc<APTaxTran.taxID>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? AdjdWhTaxSubID
		{
			get
			{
				return this._AdjdWhTaxSubID;
			}
			set
			{
				this._AdjdWhTaxSubID = value;
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
		#region AdjdCuryRate
		public abstract class adjdCuryRate : PX.Data.IBqlField
		{
		}
		protected Decimal? _AdjdCuryRate;
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDecimal(6)]
		[PXUIField(DisplayName = "Cross Rate", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? AdjdCuryRate
		{
			get
			{
				return this._AdjdCuryRate;
			}
			set
			{
				this._AdjdCuryRate = value;
			}
		}
		#endregion
		#region CuryDocBal
		public abstract class curyDocBal : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryDocBal;
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXCurrency(typeof(APAdjust.adjgCuryInfoID), typeof(APAdjust.docBal), BaseCalc=false)]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
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
		[PXDecimal(4)]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
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
		#region CuryDiscBal
		public abstract class curyDiscBal : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryDiscBal;
		[PXCurrency(typeof(APAdjust.adjgCuryInfoID), typeof(APAdjust.discBal), BaseCalc=false)]
		[PXUnboundDefault()]
		[PXUIField(DisplayName = "Cash Discount Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
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
		[PXDecimal(4)]
		[PXUnboundDefault()]
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
		#region CuryWhTaxBal
		public abstract class curyWhTaxBal : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryWhTaxBal;
		[PXCurrency(typeof(APAdjust.adjgCuryInfoID), typeof(APAdjust.whTaxBal), BaseCalc = false)]
		[PXUnboundDefault()]
		[PXUIField(DisplayName = "With. Tax Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
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
		[PXDecimal(4)]
		[PXUnboundDefault()]
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
		#region VoidAppl
		public abstract class voidAppl : PX.Data.IBqlField
		{
		}
		[PXBool()]
		[PXUIField(DisplayName = "Void Application", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual Boolean? VoidAppl
		{
			[PXDependsOnFields(typeof(adjgDocType))]
			get
			{
				return APPaymentType.VoidAppl(this._AdjgDocType);
			}
			set
			{
				if ((bool) value)
				{
					this._AdjgDocType = APDocType.VoidCheck;
					this.Voided = true;
				}
			}
		}
		#endregion
		#region ReverseGainLoss
		public abstract class reverseGainLoss : PX.Data.IBqlField
		{
		}
        [PXDependsOnFields(typeof(adjgDocType))]
        public virtual Boolean? ReverseGainLoss
		{

            get
            {
                //return (APPaymentType.DrCr(this._AdjgDocType) == "D");
                return (this.AdjgTBSign == -1m);
            }
			set
			{
			}
		}
		#endregion
        #region AdjgBalSign
        public abstract class adjgBalSign : PX.Data.IBqlField
        {
        }
        [PXDependsOnFields(typeof(adjgDocType), typeof(adjdDocType))]
        public virtual decimal? AdjgBalSign
        {
            get
            {
				return this.AdjgDocType == APDocType.Check && this.AdjdDocType == APDocType.DebitAdj || this.AdjgDocType == APDocType.VoidCheck && this.AdjdDocType == APDocType.DebitAdj ? -1m : 1m;
            }
            set { }
        }
        #endregion
		#region AdjgGLSign
		public abstract class adjgGLSign : PX.Data.IBqlField
		{
		}
		[PXDependsOnFields(typeof(adjgDocType), typeof(adjdDocType))]
		public virtual decimal? AdjgGLSign
		{
			get
			{
				return this.AdjgDocType == APDocType.Check && this.AdjdDocType == APDocType.Prepayment || this.AdjgDocType == APDocType.VoidCheck && this.AdjdDocType == APDocType.Prepayment ? 1m : APDocType.SignAmount(this.AdjdDocType);
			}
			set { }
		}
		#endregion
        #region AdjgTBSign
        public abstract class adjgTBSign : PX.Data.IBqlField
        {
        }
        [PXDependsOnFields(typeof(adjgDocType), typeof(adjdDocType))]
        public virtual decimal? AdjgTBSign
        {
            get
            {
                return this.AdjgDocType == APDocType.Check && this.AdjdDocType == APDocType.Prepayment || this.AdjgDocType == APDocType.VoidCheck && this.AdjdDocType == APDocType.Prepayment ? 1m : APDocType.SignBalance(this.AdjdDocType);
            }
            set { }
        }
        #endregion
        #region AdjdTBSign
        public abstract class adjdTBSign : PX.Data.IBqlField
        {
        }
        [PXDependsOnFields(typeof(adjgDocType), typeof(adjdDocType))]
        public virtual decimal? AdjdTBSign
        {
            get
            {
				return this.AdjgDocType == APDocType.Check && this.AdjdDocType == APDocType.DebitAdj || this.AdjgDocType == APDocType.VoidCheck && this.AdjdDocType == APDocType.DebitAdj ? 1m : APDocType.SignBalance(this.AdjgDocType);
            }
            set { }
        }
        #endregion
		#region SignedRGOLAmt
		public virtual Decimal? SignedRGOLAmt
		{
			[PXDependsOnFields(typeof(reverseGainLoss),typeof(rGOLAmt))]
			get
			{
				return ((bool)this.ReverseGainLoss ? -this._RGOLAmt : this._RGOLAmt);
			}			
		}
		#endregion

        #region AdjdBalSign
        public abstract class adjdBalSign : PX.Data.IBqlField
        {
        }
        [PXDependsOnFields(typeof(adjgDocType), typeof(adjdDocType))]
        public virtual decimal? AdjdBalSign
        {
            get
            {                                
                return this.AdjgDocType == APDocType.Check && this.AdjdDocType == APDocType.DebitAdj || this.AdjgDocType == APDocType.VoidCheck && this.AdjdDocType == APDocType.DebitAdj ? -1m : 1m;
            }
            set { }
        }
        #endregion
	}

    [Serializable]
    public partial class APAdjust2 : APAdjust
    {
        public new abstract class adjgRefNbr : IBqlField { }
        public new abstract class adjgDocType : IBqlField { }
        public new abstract class released : IBqlField { }
        public new abstract class voided : IBqlField { }
    }
}
