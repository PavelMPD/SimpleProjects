namespace PX.Objects.AR
{
	using System;
	using PX.Data;
	using PX.Objects.CM;
	using PX.Objects.GL;
	using PX.Objects.CS;
	using PX.Objects.SO;

	[PXPrimaryGraph(
		new Type[] { typeof(ARPaymentEntry) },
		new Type[] { typeof(Select<ARPayment, 
			Where<ARPayment.docType, Equal<Current<ARAdjust.adjgDocType>>,
			And<ARPayment.refNbr, Equal<Current<ARAdjust.adjgRefNbr>>>>>)
		})]
	[System.SerializableAttribute()]
	[PXCacheName(Messages.ARAdjust)]       
	public partial class ARAdjust : PX.Data.IBqlTable, IAdjustment
	{
		#region CustomerID
		public abstract class customerID : PX.Data.IBqlField
		{
		}
		protected Int32? _CustomerID;
		[PXDBInt]
		[PXDBDefault(typeof(ARPayment.customerID))]
		[PXUIField(DisplayName = "CustomerID", Visibility = PXUIVisibility.Visible, Visible = false)]
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
		#region AdjgDocType
		public abstract class adjgDocType : PX.Data.IBqlField
		{
		}
		protected String _AdjgDocType;
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
		[PXDBDefault(typeof(ARPayment.docType))]
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
		[ARDocType.PrintList()]
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
		[PXDBDefault(typeof(ARPayment.refNbr))]
		[PXUIField(DisplayName = "AdjgRefNbr", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(Select<ARPayment, Where<ARPayment.docType, Equal<Current<ARAdjust.adjgDocType>>, And<ARPayment.refNbr, Equal<Current<ARAdjust.adjgRefNbr>>, And<ARPayment.lineCntr, Equal<Current<ARAdjust.adjNbr>>>>>>))]
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
		[Branch(typeof(ARPayment.branchID))]
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
		[CurrencyInfo(ModuleCode = "AR", CuryIDField = "AdjdCuryID", Enabled = false)]
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
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask="")]
		[PXDefault(ARDocType.Invoice)]
		[PXUIField(DisplayName = "Document Type", Visibility = PXUIVisibility.Visible)]
		[ARInvoiceType.AdjdList()]
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
		[ARDocType.PrintList()]
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
		public partial class ARRegister : AR.ARRegister
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
			#region BranchID
			public new abstract class branchID : PX.Data.IBqlField
			{
			}
			#endregion
		}

		[PXHidden()]
		[PXProjection(typeof(Select2<ARRegister, LeftJoin<Standalone.ARInvoice, On<Standalone.ARInvoice.docType, Equal<ARRegister.docType>, And<Standalone.ARInvoice.refNbr, Equal<ARRegister.refNbr>>>>>))]
        [Serializable]
		public partial class ARInvoice : ARRegister
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
			public new abstract class customerID : PX.Data.IBqlField
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
			public new abstract class dueDate : PX.Data.IBqlField
			{
			}
			#endregion
			#region InvoiceNbr
			public abstract class invoiceNbr : PX.Data.IBqlField
			{
			}
			protected String _InvoiceNbr;
			[PXDBString(40, IsUnicode = true, BqlField = typeof(Standalone.ARInvoice.invoiceNbr))]
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
		[ARInvoiceType.AdjdRefNbr(typeof(Search2<ARInvoice.refNbr, LeftJoin<ARAdjust, On<ARAdjust.adjdDocType, Equal<ARInvoice.docType>, And<ARAdjust.adjdRefNbr, Equal<ARInvoice.refNbr>, And<ARAdjust.released, Equal<boolFalse>, And<ARAdjust.voided, Equal<boolFalse>, 
            And<Where<ARAdjust.adjgDocType, NotEqual<Current<ARPayment.docType>>, Or<ARAdjust.adjgRefNbr, NotEqual<Current<ARPayment.refNbr>>>>>>>>>>, 
			Where<ARInvoice.customerID, Equal<Optional<ARPayment.customerID>>, And<ARInvoice.docType, Equal<Optional<ARAdjust.adjdDocType>>, And<ARInvoice.released, Equal<boolTrue>, And<ARInvoice.openDoc, Equal<boolTrue>, And<ARAdjust.adjgRefNbr, IsNull, 
			And<ARInvoice.docDate, LessEqual<Current<ARPayment.adjDate>>, And<Where<ARInvoice.finPeriodID, LessEqual<Current<ARPayment.adjFinPeriodID>>, Or<Current<ARPayment.adjFinPeriodID>, IsNull>>>>>>>>>>), Filterable = true)]
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
		#region AdjdOrderType
		public abstract class adjdOrderType : PX.Data.IBqlField
		{
		}
		protected String _AdjdOrderType;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Order Type")]
		[PXSelector(typeof(Search<SOOrderType.orderType>))]
		public virtual String AdjdOrderType
		{
			get
			{
				return this._AdjdOrderType;
			}
			set
			{
				this._AdjdOrderType = value;
			}
		}
		#endregion
		#region AdjdOrderNbr
		public abstract class adjdOrderNbr : PX.Data.IBqlField
		{
		}
		protected String _AdjdOrderNbr;
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Order Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<SOOrder.orderNbr,
			Where<SOOrder.orderType, Equal<Optional<ARAdjust.adjdOrderType>>>>), Filterable = true)]
		[PXParent(typeof(Select<SOAdjust, Where<SOAdjust.adjdOrderType, Equal<Current<ARAdjust.adjdOrderType>>, And<SOAdjust.adjdOrderNbr, Equal<Current<ARAdjust.adjdOrderNbr>>,
			And<SOAdjust.adjgDocType, Equal<Current<ARAdjust.adjgDocType>>, And<SOAdjust.adjgRefNbr, Equal<Current<ARAdjust.adjgRefNbr>>>>>>>), LeaveChildren = true)]
		public virtual String AdjdOrderNbr
		{
			get
			{
				return this._AdjdOrderNbr;
			}
			set
			{
				this._AdjdOrderNbr = value;
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
		[PXDefault(typeof(ARPayment.lineCntr))]
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
		#region AdjBatchNbr
		public abstract class adjBatchNbr : PX.Data.IBqlField
		{
		}
		protected String _AdjBatchNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
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
		[CurrencyInfo(ModuleCode = "AR", CuryIDField = "AdjdOrigCuryID")]
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
		[CurrencyInfo(typeof(ARPayment.curyInfoID), CuryIDField = "AdjgCuryID")]
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
		[PXDBDefault(typeof(ARPayment.adjDate))]
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
		[PXDBDefault(typeof(ARPayment.adjFinPeriodID))]
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
		[PXDBDefault(typeof(ARPayment.adjTranPeriodID))]
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
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.Visible, Enabled = false)]
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
		[FinPeriodID(typeof(ARAdjust.adjdDocDate))]
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
		#region AdjdTranPeriodID
		public abstract class adjdTranPeriodID : PX.Data.IBqlField
		{
		}
		protected String _AdjdTranPeriodID;
		[FinPeriodID(typeof(ARAdjust.adjdDocDate))]
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
		#region AdjdClosedFinPeriodID
		public abstract class adjdClosedFinPeriodID : PX.Data.IBqlField
		{
		}
		protected String _AdjdClosedFinPeriodID;
		[PXDBScalar(typeof(Search<ARRegister.closedFinPeriodID, Where<ARRegister.docType, Equal<ARAdjust.adjdDocType>, And<ARRegister.refNbr, Equal<ARAdjust.adjdRefNbr>>>>))]
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
		#region CuryAdjgDiscAmt
		public abstract class curyAdjgDiscAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryAdjgDiscAmt;
		[PXDBCurrency(typeof(ARAdjust.adjgCuryInfoID), typeof(ARAdjust.adjDiscAmt))]
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
		#region CuryAdjgWOAmt
		public abstract class curyAdjgWOAmt : IBqlField { }
		protected Decimal? _CuryAdjgWOAmt;
		[PXDBCurrency(typeof(ARAdjust.adjgCuryInfoID), typeof(ARAdjust.adjWOAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Balance Write-Off", Visibility = PXUIVisibility.Visible)]
		[PXFormula(null, typeof(SumCalc<ARPayment.curyWOAmt>))]
		public virtual Decimal? CuryAdjgWOAmt
		{
			get
			{
				return this._CuryAdjgWOAmt;
			}
			set
			{
				this._CuryAdjgWOAmt = value;
			}
		}
		public virtual Decimal? CuryAdjgWhTaxAmt
		{
			get
			{
				return this._CuryAdjgWOAmt;
			}
			set
			{
				this._CuryAdjgWOAmt = value;
			}
		}
		#endregion
		#region CuryAdjgAmt
		public abstract class curyAdjgAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryAdjgAmt;
		[PXDBCurrency(typeof(ARAdjust.adjgCuryInfoID), typeof(ARAdjust.adjAmt), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount Paid", Visibility = PXUIVisibility.Visible)]
		[PXUnboundFormula(typeof(Mult<ARAdjust.adjgBalSign, ARAdjust.curyAdjgAmt>), typeof(SumCalc<ARPayment.curyApplAmt>))]
		[PXFormula(null, typeof(SumCalc<SOAdjust.curyAdjgBilledAmt>))]
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
		//[PXDBCurrency(typeof(ARAdjust.adjdCuryInfoID), typeof(ARAdjust.adjDiscAmt))]
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
		#region AdjWOAmt
		public abstract class adjWOAmt : IBqlField { }
		protected Decimal? _AdjWOAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? AdjWOAmt
		{
			get
			{
				return this._AdjWOAmt;
			}
			set
			{
				this._AdjWOAmt = value;
			}
		}
		public virtual Decimal? AdjWhTaxAmt
		{
			get
			{
				return this._AdjWOAmt;
			}
			set
			{
				this._AdjWOAmt = value;
			}
		}
		#endregion
		#region CuryAdjdWOAmt
		public abstract class curyAdjdWOAmt : IBqlField { }
		protected Decimal? _CuryAdjdWOAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryAdjdWOAmt
		{
			get
			{
				return this._CuryAdjdWOAmt;
			}
			set
			{
				this._CuryAdjdWOAmt = value;
			}
		}
		public virtual Decimal? CuryAdjdWhTaxAmt
		{
			get
			{
				return this._CuryAdjdWOAmt;
			}
			set
			{
				this._CuryAdjdWOAmt = value;
			}
		}
		#endregion
		#region AdjAmt
		public abstract class adjAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _AdjAmt;
		[PXDBDecimal(4)]
		[PXFormula(null, typeof(SumCalc<SOAdjust.adjBilledAmt>))]
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
		//[PXDBCurrency(typeof(ARAdjust.adjdCuryInfoID), typeof(ARAdjust.adjAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(null, typeof(SumCalc<SOAdjust.curyAdjdBilledAmt>))]
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
		#region AdjdARAcct
		public abstract class adjdARAcct : PX.Data.IBqlField
		{
		}
		protected Int32? _AdjdARAcct;
		[Account()]
		[PXDefault()]
		public virtual Int32? AdjdARAcct
		{
			get
			{
				return this._AdjdARAcct;
			}
			set
			{
				this._AdjdARAcct = value;
			}
		}
		#endregion
		#region AdjdARSub
		public abstract class adjdARSub : PX.Data.IBqlField
		{
		}
		protected Int32? _AdjdARSub;
		[SubAccount()]
		[PXDefault()]
		public virtual Int32? AdjdARSub
		{
			get
			{
				return this._AdjdARSub;
			}
			set
			{
				this._AdjdARSub = value;
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
		#region CuryDocBal
		public abstract class curyDocBal : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryDocBal;
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXCurrency(typeof(ARAdjust.adjgCuryInfoID), typeof(ARAdjust.docBal), BaseCalc = false)]
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
		public virtual Decimal? CuryPayDocBal
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
		public virtual Decimal? PayDocBal
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
		[PXCurrency(typeof(ARAdjust.adjgCuryInfoID), typeof(ARAdjust.discBal), BaseCalc = false)]
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
		public virtual Decimal? CuryPayDiscBal
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
		public virtual Decimal? PayDiscBal
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
		#region CuryWOBal
		public abstract class curyWOBal : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryWOBal;
		[PXCurrency(typeof(ARAdjust.adjgCuryInfoID), typeof(ARAdjust.wOBal), BaseCalc = false)]
		[PXUnboundDefault()]
		[PXUIField(DisplayName = "Write-Off Limit", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? CuryWOBal
		{
			get
			{
				return this._CuryWOBal;
			}
			set
			{
				this._CuryWOBal = value;
			}
		}
		public virtual Decimal? CuryWhTaxBal
		{
			get
			{
				return this._CuryWOBal;
			}
			set
			{
				this._CuryWOBal = value;
			}
		}

		#endregion
		#region WOBal
		public abstract class wOBal : PX.Data.IBqlField
		{
		}
		protected Decimal? _WOBal;
		[PXDecimal(4)]
		[PXUnboundDefault()]
		public virtual Decimal? WOBal
		{
			get
			{
				return this._WOBal;
			}
			set
			{
				this._WOBal = value;
			}
		}
		public virtual Decimal? WhTaxBal
		{
			get
			{
				return this._WOBal;
			}
			set
			{
				this._WOBal = value;
			}
		}
		#endregion
		#region WriteOffReasonCode
		public abstract class writeOffReasonCode : PX.Data.IBqlField
		{
		}
		protected String _WriteOffReasonCode;
		[PXFormula(typeof(Switch<Case<Where<ARAdjust.adjdDocType, NotEqual<ARDocType.creditMemo>>, Current<ARSetup.balanceWriteOff>>>))]
		[PXDBString(ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where2<Where<ReasonCode.usage, Equal<ReasonCodeUsages.creditWriteOff>, And<Current<ARAdjust.adjdDocType>, Equal<ARDocType.creditMemo>>>,
			Or<Where<ReasonCode.usage, Equal<ReasonCodeUsages.balanceWriteOff>, And<Current<ARAdjust.adjdDocType>, NotEqual<ARDocType.creditMemo>>>>>>))]
		[PXUIField(DisplayName = "Write-Off Reason Code", Visibility = PXUIVisibility.Visible)]
		public virtual String WriteOffReasonCode
		{
			get
			{
				return this._WriteOffReasonCode;
			}
			set
			{
				this._WriteOffReasonCode = value;
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
				return (this._AdjgDocType == ARDocType.VoidPayment);
			}
			set
			{
				if ((bool)value)
				{
					this._AdjgDocType = ARDocType.VoidPayment;
					this.Voided = true;
				}
			}
		}
		#endregion
		#region ReverseGainLoss
		public abstract class reverseGainLoss : PX.Data.IBqlField
		{
		}
		[PXBool()]
        [PXDependsOnFields(typeof(adjgDocType))]
        public virtual Boolean? ReverseGainLoss
		{
			get
			{
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
				return this.AdjgDocType == ARDocType.Payment && this.AdjdDocType == ARDocType.CreditMemo || this.AdjgDocType == ARDocType.VoidPayment && this.AdjdDocType == ARDocType.CreditMemo ? -1m : 1m;
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
				return ARDocType.SignAmount(this.AdjdDocType);
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
                return ARDocType.SignBalance(this.AdjdDocType);
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
				return this.AdjgDocType == ARDocType.Payment && this.AdjdDocType == ARDocType.CreditMemo || this.AdjgDocType == ARDocType.VoidPayment && this.AdjdDocType == ARDocType.CreditMemo ? 1m : ARDocType.SignBalance(this.AdjgDocType);
            }
            set { }
        }
        #endregion
		#region CuryWhTaxBal
		public virtual Decimal? CuryPayWhTaxBal
		{
			get
			{
				return 0m;
			}
			set
			{
				;
			}
		}
		#endregion
		#region WhTaxBal
		public virtual Decimal? PayWhTaxBal
		{
			get
			{
				return 0m;
			}
			set
			{
				;
			}
		}
		#endregion
		#region SignedRGOLAmt
		public virtual Decimal? SignedRGOLAmt
		{
			[PXDependsOnFields(typeof(reverseGainLoss),typeof(rGOLAmt))]
			get
			{
				return ((bool) this.ReverseGainLoss ? -this._RGOLAmt : this._RGOLAmt);
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
                return this.AdjgDocType == ARDocType.Payment && this.AdjdDocType == ARDocType.CreditMemo || this.AdjgDocType == ARDocType.VoidPayment && this.AdjdDocType == ARDocType.CreditMemo ? -1m : 1m;
            }
            set { }
        }
        #endregion
	}

    [Serializable]
    public partial class ARAdjust2 : ARAdjust
    {
        public new abstract class adjgRefNbr : IBqlField { }
        public new abstract class adjgDocType : IBqlField { }
        public new abstract class released : IBqlField { }
        public new abstract class voided : IBqlField { }
    }
}
