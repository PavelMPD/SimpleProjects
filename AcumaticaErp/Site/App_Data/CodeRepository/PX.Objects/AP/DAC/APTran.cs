using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.FA;
using PX.Objects.PO;

namespace PX.Objects.AP
{
	using System;
	using PX.Data;
	using PX.Objects.GL;
	using PX.Objects.CM;
	using PX.Objects.CS;
	using PX.Objects.IN;
	using PX.Objects.TX;
	using PX.Objects.DR;
	using PX.Objects.PM;
		
	[System.SerializableAttribute()]
	[PXCacheName(Messages.APTran)]
	public partial class APTran : PX.Data.IBqlTable
	{
		#region Selected
		public abstract class selected : IBqlField
		{
		}
		protected bool? _Selected = false;
		[PXBool]
		[PXUIField(DisplayName = "Selected")]
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
		#region BranchID
		public abstract class branchID : PX.Data.IBqlField
		{
		}
		protected Int32? _BranchID;
		[Branch(typeof(APRegister.branchID))]
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
		#region TranType
		public abstract class tranType : PX.Data.IBqlField
		{
		}
		protected String _TranType;
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(APRegister.docType))]
		[PXUIField(DisplayName="Tran. Type",Visibility=PXUIVisibility.Visible,Visible=false)]
		public virtual String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.IBqlField
		{
		}
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(APRegister.refNbr))]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(Select<APRegister, Where<APRegister.docType, Equal<Current<APTran.tranType>>, And<APRegister.refNbr, Equal<Current<APTran.refNbr>>>>>))]
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
		#region LineNbr
		public abstract class lineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXLineNbr(typeof(APRegister.lineCntr))]
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
		#region TranID
		public abstract class tranID : PX.Data.IBqlField
		{
		}
		protected Int32? _TranID;
		[PXDBIdentity()]
		public virtual Int32? TranID
		{
			get
			{
				return this._TranID;
			}
			set
			{
				this._TranID = value;
			}
		}
		#endregion

		#region VendorID
		public abstract class vendorID : PX.Data.IBqlField
		{
		}
		protected Int32? _VendorID;
		[Vendor()]
		[PXDBDefault(typeof(APRegister.vendorID))]
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
		#region BatchNbr
		public abstract class batchNbr : PX.Data.IBqlField
		{
		}
		protected String _BatchNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXDefault("")]
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
		#region LineType
		public abstract class lineType : PX.Data.IBqlField
		{
		}
		protected String _LineType;
		[PXDBString(2, IsFixed = true)]
		[PXDefault("")]
		public virtual String LineType
		{
			get
			{
				return this._LineType;
			}
			set
			{
				this._LineType = value;
			}
		}
		#endregion
		#region POOrderType
		public abstract class pOOrderType : PX.Data.IBqlField
		{
		}
		protected String _POOrderType;
		[PXDBString(2, IsFixed = true)]
		[PO.POOrderType.List()]
		[PXUIField(DisplayName = "PO Type", Enabled = false)]
		public virtual String POOrderType
		{
			get
			{
				return this._POOrderType;
			}
			set
			{
				this._POOrderType = value;
			}
		}
		#endregion
		#region PONbr
		public abstract class pONbr : PX.Data.IBqlField
		{
		}
		protected String _PONbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "PO Number", Enabled = false)]
		[PXSelector(typeof(Search<POOrder.orderNbr, Where<POOrder.orderType, Equal<Optional<APTran.pOOrderType>>>>))]
		public virtual String PONbr
		{
			get
			{
				return this._PONbr;
			}
			set
			{
				this._PONbr = value;
			}
		}
		#endregion
		#region POLineNbr
		public abstract class pOLineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _POLineNbr;
		[PXDBInt()]
		[PXParent(typeof(Select<PO.POLineAP, Where<PO.POLineAP.orderType, Equal<Current<APTran.pOOrderType>>,
													And<PO.POLineAP.orderNbr, Equal<Current<APTran.pONbr>>,
													And<PO.POLineAP.lineNbr, Equal<Current<APTran.pOLineNbr>>,
													And<Current<APTran.receiptNbr>,IsNull>>>>>))]
		[PXUIField(DisplayName = "PO Line", Enabled = false, Visible = false)]
		public virtual Int32? POLineNbr
		{
			get
			{
				return this._POLineNbr;
			}
			set
			{
				this._POLineNbr = value;
			}
		}
		#endregion
		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.IBqlField
		{
		}
		protected String _ReceiptNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "PO Receipt Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<POReceipt.receiptNbr>))]
		public virtual String ReceiptNbr
		{
			get
			{
				return this._ReceiptNbr;
			}
			set
			{
				this._ReceiptNbr = value;
			}
		}
		#endregion
		#region ReceiptLineNbr
		public abstract class receiptLineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _ReceiptLineNbr;
		[PXDBInt()]
		[PXParent(typeof(Select<PO.POReceiptLineR, Where<PO.POReceiptLineR.receiptNbr, Equal<Current<APTran.receiptNbr>>,
														And<PO.POReceiptLineR.lineNbr, Equal<Current<APTran.receiptLineNbr>>>>>))]
		[PXUIField(DisplayName = "PO Receipt Line", Enabled = false, Visible = false)]
		public virtual Int32? ReceiptLineNbr
		{
			get
			{
				return this._ReceiptLineNbr;
			}
			set
			{
				this._ReceiptLineNbr = value;
			}
		}
		#endregion
		#region LCTranID
		public abstract class lCTranID : PX.Data.IBqlField
		{
		}
		protected Int32? _LCTranID;
		[PXDBInt()]
		//[PXDBLiteDefault(typeof(APLandedCostTran.lCTranID),PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Enabled = false, Visible = false)]
		public virtual Int32? LCTranID
		{
			get
			{
				return this._LCTranID;
			}
			set
			{
				this._LCTranID = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		//[NonStockItem(Filterable = true)]
		[APTranInventoryItem(Filterable = true)]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.IBqlField
		{
		}
		protected Int32? _AccountID;
		[Account(typeof(APTran.branchID), DisplayName = "Account", Visibility = PXUIVisibility.Visible, Filterable=false, DescriptionField = typeof(Account.description))]
		[PXDefault(typeof(Search<InventoryItem.cOGSAcctID, Where<InventoryItem.inventoryID, Equal<Current<APTran.inventoryID>>>>))]
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
		[SubAccount(typeof(APTran.accountID), typeof(APTran.branchID), DisplayName = "Subaccount", Visibility = PXUIVisibility.Visible,Filterable = true)]
		[PXDefault(typeof(Search<InventoryItem.cOGSSubID, Where<InventoryItem.inventoryID, Equal<Current<APTran.inventoryID>>>>))]
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
		#region ProjectID
		public abstract class projectID : PX.Data.IBqlField
		{
		}
		protected Int32? _ProjectID;
		[ProjectDefault(BatchModule.AP, typeof(Search<Location.vDefProjectID, Where<Location.bAccountID, Equal<Current<APInvoice.vendorID>>, And<Location.locationID, Equal<Current<APInvoice.vendorLocationID>>>>>), typeof(APTran.accountID))]
		[ActiveProjectForModule(BatchModule.AP, false)]
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
		[ActiveProjectTask(typeof(APTran.projectID), BatchModule.AP, DisplayName = "Project Task")]
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
		#region Box1099
		public abstract class box1099 : PX.Data.IBqlField
		{
		}
		protected Int16? _Box1099;
		[PXDBShort()]
		[PXIntList(new int[] { 0 }, new string[] { "undefined" })]
		[PXDefault(typeof(Coalesce<Search<Account.box1099,Where<Account.accountID,Equal<Current<APTran.accountID>>>>, Search<Vendor.box1099, Where<Vendor.bAccountID, Equal<Current<APTran.vendorID>>>>>), PersistingCheck=PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName="1099 Box", Visibility=PXUIVisibility.Visible)]
		public virtual Int16? Box1099
		{
			get
			{
				return this._Box1099;
			}
			set
			{
				this._Box1099 = value;
			}
		}
		#endregion
        #region TaxID
        public abstract class taxID : PX.Data.IBqlField
        {
        }
        protected String _TaxID;
        [PXDBString(Tax.taxID.Length, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax ID", Visible = false)]
        [PXSelector(typeof(Tax.taxID))]
        public virtual String TaxID
        {
            get
            {
                return this._TaxID;
            }
            set
            {
                this._TaxID = value;
            }
        }
        #endregion
		#region DeferredCode
		public abstract class deferredCode : PX.Data.IBqlField
		{
		}
		protected String _DeferredCode;
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Deferral Code")]
		[PXSelector(typeof(Search<DRDeferredCode.deferredCodeID, 
			Where<DRDeferredCode.accountType, Equal<DeferredAccountType.expense>,
			And<DRDeferredCode.method, NotEqual<DeferredMethodType.cashReceipt>>>>))]
		[PXDefault(typeof(Search2<InventoryItem.deferredCode, InnerJoin<DRDeferredCode, On<DRDeferredCode.deferredCodeID, Equal<InventoryItem.deferredCode>>>, Where<DRDeferredCode.accountType, Equal<DeferredAccountType.expense>, And<InventoryItem.inventoryID, Equal<Current<APTran.inventoryID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String DeferredCode
		{
			get
			{
				return this._DeferredCode;
			}
			set
			{
				this._DeferredCode = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(typeof(APRegister.curyInfoID))]
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
		#region SiteID
		public abstract class siteID : PX.Data.IBqlField
		{
		}
		protected Int32? _SiteID;
		[PXDBInt()]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.IBqlField
		{
		}
		protected String _UOM;
		[PXDefault(typeof(Search<InventoryItem.purchaseUnit, Where<InventoryItem.inventoryID, Equal<Current<APTran.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[INUnit(typeof(APTran.inventoryID))]
		public virtual String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
			}
		}
		#endregion
		#region Qty
		public abstract class qty : PX.Data.IBqlField
		{
		}
		protected Decimal? _Qty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUnboundFormula(typeof(Switch<Case<Where<APTran.drCr, Equal<DrCr.debit>>, APTran.qty>, Minus<APTran.qty>>), typeof(SumCalc<PO.POReceiptLineR.signedVoucheredQty>))]
		[PXUnboundFormula(typeof(Switch<Case<Where<APTran.drCr, Equal<DrCr.debit>>, APTran.qty>, Minus<APTran.qty>>), typeof(SumCalc<PO.POLineAP.voucheredQty>))]
		[PXUnboundFormula(typeof(Switch<Case<Where<APTran.drCr, Equal<DrCr.debit>>, APTran.qty>, Minus<APTran.qty>>), typeof(SumCalc<PO.POLineAP.receivedQty>))]		
		[PXUIField(DisplayName = "Quantity", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? Qty
		{
			get
			{
				return this._Qty;
			}
			set
			{
				this._Qty = value;
			}
		}
		#endregion
		#region CuryUnitCost
		public abstract class curyUnitCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryUnitCost;
        [PXDBCurrency(typeof(Search<INSetup.decPlPrcCst>), typeof(APTran.curyInfoID), typeof(APTran.unitCost))]
		[PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryUnitCost
		{
			get
			{
				return this._CuryUnitCost;
			}
			set
			{
				this._CuryUnitCost = value;
			}
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _UnitCost;
		[PXDBDecimal(6)]
		[PXDefault(typeof(Search<INItemCost.lastCost, Where<INItemCost.inventoryID, Equal<Current<APTran.inventoryID>>>>))]
		public virtual Decimal? UnitCost
		{
			get
			{
				return this._UnitCost;
			}
			set
			{
				this._UnitCost = value;
			}
		}
		#endregion
        #region ManualDisc
        public abstract class manualDisc : PX.Data.IBqlField
        {
        }
        protected Boolean? _ManualDisc;
        [SO.ManualDiscountMode(typeof(APTran.curyDiscAmt), typeof(APTran.curyTranAmt), typeof(APTran.discPct))]
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual Discount", Visibility = PXUIVisibility.Visible)]
        public virtual Boolean? ManualDisc
        {
            get
            {
                return this._ManualDisc;
            }
            set
            {
                this._ManualDisc = value;
            }
        }
        #endregion
        #region CuryLineAmt
        public abstract class curyLineAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryLineAmt;
        [PXDBCurrency(typeof(APTran.curyInfoID), typeof(APTran.lineAmt))]
        [PXUIField(DisplayName = "Ext. Cost")]
        [PXFormula(typeof(Mult<APTran.qty, APTran.curyUnitCost>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryLineAmt
        {
            get
            {
                return this._CuryLineAmt;
            }
            set
            {
                this._CuryLineAmt = value;
            }
        }
        #endregion
        #region LineAmt
        public abstract class lineAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _LineAmt;
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? LineAmt
        {
            get
            {
                return this._LineAmt;
            }
            set
            {
                this._LineAmt = value;
            }
        }
        #endregion
        #region CuryDiscCost
        public abstract class curyDiscCost : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryDiscCost;
        [PXDBPriceCostCalced(typeof(Switch<Case<Where<APTran.qty, Equal<decimal0>>, decimal0>, Div<Sub<APTran.curyTranAmt, APTran.curyDiscAmt>, APTran.qty>>), typeof(Decimal))]
        [PXFormula(typeof(Div<Sub<APTran.curyTranAmt, APTran.curyDiscAmt>, NullIf<APTran.qty, decimal0>>))]
        [PXCurrency(typeof(Search<INSetup.decPlPrcCst>), typeof(APTran.curyInfoID), typeof(APTran.discCost))]
        [PXUIField(DisplayName = "Disc. Unit Cost", Enabled = false, Visible = false)]
        public virtual Decimal? CuryDiscCost
        {
            get
            {
                return this._CuryDiscCost;
            }
            set
            {
                this._CuryDiscCost = value;
            }
        }
        #endregion
        #region DiscCost
        public abstract class discCost : PX.Data.IBqlField
        {
        }
        protected Decimal? _DiscCost;
        [PXDBPriceCostCalced(typeof(Switch<Case<Where<APTran.qty, Equal<decimal0>>, decimal0>, Div<APTran.lineAmt, APTran.qty>>), typeof(Decimal))]
        [PXFormula(typeof(Div<Row<APTran.lineAmt, APTran.curyLineAmt>, NullIf<APTran.qty, decimal0>>))]
        public virtual Decimal? DiscCost
        {
            get
            {
                return this._DiscCost;
            }
            set
            {
                this._DiscCost = value;
            }
        }
        #endregion
		#region CuryTranAmt
		public abstract class curyTranAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTranAmt;
		[PXDBCurrency(typeof(APTran.curyInfoID), typeof(APTran.tranAmt))]
        [PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXFormula(typeof(Sub<APTran.curyLineAmt, APTran.curyDiscAmt>))]
        [PXUnboundFormula(typeof(Switch<Case<Where<APTran.drCr, Equal<DrCr.debit>>, APTran.curyTranAmt>, Minus<APTran.curyTranAmt>>), typeof(SumCalc<PO.POReceiptLineR.signedCuryVoucheredCost>))]
        [PXUnboundFormula(typeof(Switch<Case<Where<APTran.drCr, Equal<DrCr.debit>>, APTran.curyTranAmt>, Minus<APTran.curyTranAmt>>), typeof(SumCalc<PO.POLineAP.curyVoucheredCost>))]
        [PXUnboundFormula(typeof(Switch<Case<Where<APTran.drCr, Equal<DrCr.debit>>, APTran.curyTranAmt>, Minus<APTran.curyTranAmt>>), typeof(SumCalc<PO.POLineAP.curyReceivedCost>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranAmt
		{
			get
			{
				return this._CuryTranAmt;
			}
			set
			{
				this._CuryTranAmt = value;
			}
		}
		#endregion
		#region TranAmt
		public abstract class tranAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranAmt;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		//[PXFormula(null, typeof(SumCalc<PO.POReceiptLineR1.voucheredCost>))]
		public virtual Decimal? TranAmt
		{
			get
			{
				return this._TranAmt;
			}
			set
			{
				this._TranAmt = value;
			}
		}
		#endregion
		#region CuryTaxableAmt
		public abstract class curyTaxableAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTaxableAmt;
		[PXDBCurrency(typeof(APTran.curyInfoID), typeof(APTran.taxableAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Net Amount", Enabled = false)]
        public virtual Decimal? CuryTaxableAmt
		{
			get
			{
				return this._CuryTaxableAmt;
			}
			set
			{
				this._CuryTaxableAmt = value;
			}
		}
		#endregion
		#region TaxableAmt
		public abstract class taxableAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _TaxableAmt;
		[PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? TaxableAmt
		{
			get
			{
				return this._TaxableAmt;
			}
			set
			{
				this._TaxableAmt = value;
			}
		}
		#endregion
        #region CuryTaxAmt
        public abstract class curyTaxAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryTaxAmt;
        [PXDBCurrency(typeof(APTran.curyInfoID), typeof(APTran.taxAmt))]
        [PXUIField(DisplayName = "VAT", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryTaxAmt
        {
            get
            {
                return this._CuryTaxAmt;
            }
            set
            {
                this._CuryTaxAmt = value;
            }
        }
        #endregion
        #region TaxAmt
        public abstract class taxAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _TaxAmt;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? TaxAmt
        {
            get
            {
                return this._TaxAmt;
            }
            set
            {
                this._TaxAmt = value;
            }
        }
        #endregion
        #region GroupDiscountRate
        public abstract class groupDiscountRate : PX.Data.IBqlField
        {
        }
        protected Decimal? _GroupDiscountRate;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "1.0")]
        public virtual Decimal? GroupDiscountRate
        {
            get
            {
                return this._GroupDiscountRate;
            }
            set
            {
                this._GroupDiscountRate = value;
            }
        }
        #endregion
		#region TranClass
		public abstract class tranClass : PX.Data.IBqlField
		{
		}
		protected String _TranClass;
		[PXDBString(1, IsFixed = true)]
		[PXDefault("")]
		public virtual String TranClass
		{
			get
			{
				return this._TranClass;
			}
			set
			{
				this._TranClass = value;
			}
		}
		#endregion
		#region DrCr
		public abstract class drCr : PX.Data.IBqlField
		{
		}
		protected String _DrCr;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(APInvoice.drCr))]
		public virtual String DrCr
		{
			get
			{
				return this._DrCr;
			}
			set
			{
				this._DrCr = value;
			}
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _TranDate;
		[PXDBDate()]
		[PXDBDefault(typeof(APRegister.docDate))]
		public virtual DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		protected String _FinPeriodID;
		[GL.FinPeriodID()]
		[PXDBDefault(typeof(APRegister.finPeriodID))]
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
		#region TranDesc
		public abstract class tranDesc : PX.Data.IBqlField
		{
		}
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName="Transaction Descr.", Visibility=PXUIVisibility.Visible)]
		public virtual String TranDesc
		{
			get
			{
				return this._TranDesc;
			}
			set
			{
				this._TranDesc = value;
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
		#region POPPVAmt
		public abstract class pOPPVAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _POPPVAmt;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? POPPVAmt
		{
			get
			{
				return this._POPPVAmt;
			}
			set
			{
				this._POPPVAmt = value;
			}
		}
		#endregion		
        #region NonBillable
        public abstract class nonBillable : PX.Data.IBqlField
        {
        }
        protected Boolean? _NonBillable;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Non Billable")]
        public virtual Boolean? NonBillable
        {
            get
            {
                return this._NonBillable;
            }
            set
            {
                this._NonBillable = value;
            }
        }
        #endregion
		#region DefScheduleID
		public abstract class defScheduleID : IBqlField
		{
		}
		protected int? _DefScheduleID;
		[PXDBInt]
		[PXUIField(DisplayName = "Deferral Schedule")]
		[PXSelector(typeof(Search<DR.DRSchedule.scheduleID, 
			Where<DR.DRSchedule.bAccountID, Equal<Current<APInvoice.vendorID>>,
			And <DR.DRSchedule.docType, Equal<APInvoiceType.invoice>,
			Or<DR.DRSchedule.docType, Equal<APInvoiceType.creditAdj>>>>>))]
		public virtual int? DefScheduleID
		{
			get
			{
				return this._DefScheduleID;
			}
			set
			{
				this._DefScheduleID = value;
			}
		}
		#endregion
		#region Date
		public abstract class date : PX.Data.IBqlField
		{
		}
		protected DateTime? _Date;
		/// <summary>
		/// Expense Date. When an Expense Claim is released the Expense date is recorded. 
		/// </summary>
		[PXDBDate()]
		[PXUIField(DisplayName="Date", Visible=false)]
		public virtual DateTime? Date
		{
			get
			{
				return this._Date;
			}
			set
			{
				this._Date = value;
			}
		}
		#endregion

        #region LandedCostCodeID
        public abstract class landedCostCodeID : PX.Data.IBqlField
        {
        }
        protected String _LandedCostCodeID;
        [PXDBString(15, IsUnicode = true, IsFixed = false)]
        [PXUIField(DisplayName = "Landed Cost Code")]
        [PXSelector(typeof(Search<PO.LandedCostCode.landedCostCodeID>))]
        public virtual String LandedCostCodeID
        {
            get
            {
                return this._LandedCostCodeID;
            }
            set
            {
                this._LandedCostCodeID = value;
            }
        }
        #endregion

        #region DiscPct
        public abstract class discPct : PX.Data.IBqlField
        {
        }
        protected Decimal? _DiscPct;
        [PXDBDecimal(6, MinValue = -100, MaxValue = 100)]
        [PXUIField(DisplayName = "Discount Percent")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? DiscPct
        {
            get
            {
                return this._DiscPct;
            }
            set
            {
                this._DiscPct = value;
            }
        }
        #endregion
        #region CuryDiscAmt
        public abstract class curyDiscAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryDiscAmt;
        [PXDBCurrency(typeof(APTran.curyInfoID), typeof(APTran.discAmt))]
        [PXUIField(DisplayName = "Discount Amount")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryDiscAmt
        {
            get
            {
                return this._CuryDiscAmt;
            }
            set
            {
                this._CuryDiscAmt = value;
            }
        }
        #endregion
        #region DiscAmt
        public abstract class discAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _DiscAmt;
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? DiscAmt
        {
            get
            {
                return this._DiscAmt;
            }
            set
            {
                this._DiscAmt = value;
            }
        }
        #endregion
        #region DiscountID
        public abstract class discountID : PX.Data.IBqlField
        {
        }
        protected String _DiscountID;
        [PXDBString(10, IsUnicode = true)]
        [PXSelector(typeof(Search<APDiscount.discountID, Where<APDiscount.bAccountID, Equal<Current<APTran.vendorID>>, And<APDiscount.type, Equal<SO.DiscountType.LineDiscount>>>>))]
        [PXUIField(DisplayName = "Discount Code", Visible = true, Enabled = true)]
        public virtual String DiscountID
        {
            get
            {
                return this._DiscountID;
            }
            set
            {
                this._DiscountID = value;
            }
        }
        #endregion
        #region DiscountSequenceID
        public abstract class discountSequenceID : PX.Data.IBqlField
        {
        }
        protected String _DiscountSequenceID;
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Discount Sequence", Visible = false)]
        public virtual String DiscountSequenceID
        {
            get
            {
                return this._DiscountSequenceID;
            }
            set
            {
                this._DiscountSequenceID = value;
            }
        }
        #endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.IBqlField
		{
		}
		protected String _TaxCategoryID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		[PXDefault(typeof(Search<InventoryItem.taxCategoryID,
			Where<InventoryItem.inventoryID, Equal<Current<APTran.inventoryID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String TaxCategoryID
		{
			get
			{
				return this._TaxCategoryID;
			}
			set
			{
				this._TaxCategoryID = value;
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

		#region ClassID
		public abstract class classID : IBqlField
		{
		}
		protected Int32? _ClassID;
		[PXInt]
		[PXSelector(typeof(Search2<FixedAsset.assetID,
			LeftJoin<FABookSettings, On<FixedAsset.assetID, Equal<FABookSettings.assetID>>,
			LeftJoin<FABook, On<FABookSettings.bookID, Equal<FABook.bookID>>>>,
			Where<FixedAsset.recordType, Equal<FARecordType.classType>,
			And<FABook.updateGL, Equal<True>,
			And<FABookSettings.depreciate, Equal<True>>>>>),
					SubstituteKey = typeof(FixedAsset.assetCD),
					DescriptionField = typeof(FixedAsset.description))]
		[PXUIField(DisplayName = "Asset Class", Visibility = PXUIVisibility.Visible)]
		public virtual Int32? ClassID
		{
			get
			{
				return _ClassID;
			}
			set
			{
				_ClassID = value;
			}
		}
		#endregion
		#region Custodian
		public abstract class custodian : IBqlField
		{
		}
		protected Guid? _Custodian;
		[PXGuid]
		[PXSelector(typeof(EPEmployee.userID), SubstituteKey = typeof(EPEmployee.acctCD), DescriptionField = typeof(EPEmployee.acctName))]
		[PXUIField(DisplayName = "Custodian")]
		public virtual Guid? Custodian
		{
			get
			{
				return _Custodian;
			}
			set
			{
				_Custodian = value;
			}
		}
		#endregion


		protected decimal Sign
		{
			get { return this.DrCr == "D" ? Decimal.One : Decimal.MinusOne; }
		}

		#region SignedQty
		public abstract class signedQty : PX.Data.IBqlField
		{
		}
		[PXQuantity()]
		public virtual Decimal? SignedQty
		{
			[PXDependsOnFields(typeof(qty))]
			get
			{
				return this._Qty * this.Sign;
			}
		}
		#endregion
		#region SignedCuryTranAmt
		public abstract class signedCuryTranAmt : PX.Data.IBqlField
		{
		}
		[PXDecimal()]
		public virtual Decimal? SignedCuryTranAmt
		{
			[PXDependsOnFields(typeof(curyTranAmt))]
			get
			{
				return (this._CuryTranAmt * this.Sign);
			}
		}
		#endregion
		#region SignedTranAmt
		public abstract class signedTranAmt : PX.Data.IBqlField
		{
		}
		[PXBaseCury()]
		public virtual Decimal? SignedTranAmt
		{
			[PXDependsOnFields(typeof(tranAmt))]
			get
			{
				return (this._TranAmt* this.Sign);
			}
			
		}
		#endregion		
	}

    /// <summary>
    /// Provides a selector for the Inventory Items, which may be put into the AP Invoice line <br/>
    /// The list is filtered by the user access rights and Inventory Item status - inactive <br/>
    /// and marked to delete items are not shown. If the Purchase order or PO Receipt is specified <br/>
    /// - all the items are shown, restriction is made in other place. <br/>
    /// May be used only on DAC derived from APTran. <br/>
    /// <example>
    /// [APTranInventoryItem(Filterable = true)]
    /// </example>
    /// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	public class APTranInventoryItemAttribute : InventoryAttribute
	{
		public APTranInventoryItemAttribute()
			: base(typeof(Search<InventoryItem.inventoryID, Where2<Match<Current<AccessInfo.userName>>, And2<
						Where<InventoryItem.stkItem, Equal<False>, 
								And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
								And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>, 
								And<Current<APTran.receiptNbr>, IsNull, And<Current<APTran.pONbr>, IsNull>>>>>,
						Or<Current<APTran.pONbr>, IsNotNull, 
						Or<Current<APTran.receiptNbr>, IsNotNull>>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))
		{
		}
	}
}
