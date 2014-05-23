namespace PX.Objects.IN
{
	using System;
	using PX.Data;
	using PX.Objects.CS;
	using PX.Objects.GL;
	
	[System.SerializableAttribute()]
    [PXCacheName(Messages.INTranSplit)]
	public partial class INTranSplit : PX.Data.IBqlTable, ILSDetail
	{
		#region Selected
		public abstract class selected : IBqlField
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
		#region DocType
		public abstract class docType : PX.Data.IBqlField
		{
		}
		protected String _DocType;
		[PXDBString(1, IsFixed = true, IsKey = true)]
		[PXDefault(typeof(INTran.docType))]
		[PXParent(typeof(Select<INRegister, Where<INRegister.docType, Equal<Current<INTranSplit.docType>>, And<INRegister.refNbr, Equal<Current<INTranSplit.refNbr>>>>>))]
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
        #region OrigModule
        public abstract class origModule : PX.Data.IBqlField
        {
        }
        protected String _OrigModule;
        [PXString(2)]       
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
		#region TranType
		public abstract class tranType : PX.Data.IBqlField
		{
		}
		protected String _TranType;
		[PXDBString(3, IsFixed = true)]
		[PXDefault(typeof(INTran.tranType))]
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
		[PXDBDefault(typeof(INRegister.refNbr))]
		[PXParent(typeof(Select<INTran, Where<INTran.docType, Equal<Current<INTranSplit.docType>>, And<INTran.refNbr, Equal<Current<INTranSplit.refNbr>>, And<INTran.lineNbr,Equal<Current<INTranSplit.lineNbr>>>>>>))]
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
		[PXDefault(typeof(INTran.lineNbr))]
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
		#region POLineType
		public abstract class pOLineType : PX.Data.IBqlField
		{
		}
		protected String _POLineType;
		[PXDBString(2)]
		public virtual String POLineType
		{
			get
			{
				return this._POLineType;
			}
			set
			{
				this._POLineType = value;
			}
		}
		#endregion
		#region TransferType
		public abstract class transferType : PX.Data.IBqlField
		{
		}
		protected String _TransferType;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(typeof(INRegister.transferType), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String TransferType
		{
			get
			{
				return this._TransferType;
			}
			set
			{
				this._TransferType = value;
			}
		}
		#endregion
		#region ToSiteID
		public abstract class toSiteID : PX.Data.IBqlField
		{
		}
		protected Int32? _ToSiteID;
        [PXDBInt()]
		public virtual Int32? ToSiteID
		{
			get
			{
				return this._ToSiteID;
			}
			set
			{
				this._ToSiteID = value;
			}
		}
		#endregion
		#region ToLocationID
		public abstract class toLocationID : PX.Data.IBqlField
		{
		}
		protected Int32? _ToLocationID;
        [PXDBInt()]
		public virtual Int32? ToLocationID
		{
			get
			{
				return this._ToLocationID;
			}
			set
			{
				this._ToLocationID = value;
			}
		}
		#endregion
		#region SplitLineNbr
		public abstract class splitLineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _SplitLineNbr;
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(INRegister.lineCntr))]
		public virtual Int32? SplitLineNbr
		{
			get
			{
				return this._SplitLineNbr;
			}
			set
			{
				this._SplitLineNbr = value;
			}
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _TranDate;
		[PXDBDate()]
		[PXDBDefault(typeof(INRegister.tranDate))]
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
		#region InvtMult
		public abstract class invtMult : PX.Data.IBqlField
		{
		}
		protected Int16? _InvtMult;
		[PXDBShort()]
		[PXDefault(typeof(INTran.invtMult))]
		public virtual Int16? InvtMult
		{
			get
			{
				return this._InvtMult;
			}
			set
			{
				this._InvtMult = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[StockItem(Visible = false)]
		[PXDefault(typeof(INTran.inventoryID))]
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
		#region IsStockItem
		public bool? IsStockItem
		{
			get
			{
				return true;
			}
			set { }
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.IBqlField
		{
		}
		protected Int32? _SubItemID;
		[IN.SubItem(typeof(INTranSplit.inventoryID),
			typeof(LeftJoin<INSiteStatus,
				On<INSiteStatus.subItemID, Equal<INSubItem.subItemID>,
				And<INSiteStatus.inventoryID, Equal<Optional<INTranSplit.inventoryID>>,
				And<INSiteStatus.siteID, Equal<Optional<INTranSplit.siteID>>>>>>))]
		[PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
			Where<InventoryItem.inventoryID, Equal<Current<INTranSplit.inventoryID>>,
			And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>))]
		[PXFormula(typeof(Default<INTranSplit.inventoryID>))]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region CostSubItemID
		public abstract class costSubItemID : PX.Data.IBqlField
		{
		}
		protected Int32? _CostSubItemID;
		[PXDBInt()]
		public virtual Int32? CostSubItemID
		{
			get
			{
				return this._CostSubItemID;
			}
			set
			{
				this._CostSubItemID = value;
			}
		}
		#endregion
		#region CostSiteID
		public abstract class costSiteID : PX.Data.IBqlField
		{
		}
		protected Int32? _CostSiteID;
		[PXDBInt()]
		public virtual Int32? CostSiteID
		{
			get
			{
				return this._CostSiteID;
			}
			set
			{
				this._CostSiteID = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.IBqlField
		{
		}
		protected Int32? _SiteID;
		[IN.Site()]
		[PXDefault(typeof(INTran.siteID))]
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
		#region LocationID
		public abstract class locationID : PX.Data.IBqlField
		{
		}
		protected Int32? _LocationID;
		[IN.LocationAvail(typeof(INTranSplit.inventoryID), typeof(INTranSplit.subItemID), typeof(INTranSplit.siteID), typeof(INTranSplit.tranType), typeof(INTranSplit.invtMult))]
		[PXDefault()]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.IBqlField
		{
		}
		protected String _LotSerialNbr;
		[INLotSerialNbr(typeof(INTranSplit.inventoryID), typeof(INTranSplit.subItemID), typeof(INTranSplit.locationID), typeof(INTran.lotSerialNbr))]
		public virtual String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region LotSerClassID
		public abstract class lotSerClassID : PX.Data.IBqlField
		{
		}
		protected String _LotSerClassID;
		[PXString(10, IsUnicode = true)]
		public virtual String LotSerClassID
		{
			get
			{
				return this._LotSerClassID;
			}
			set
			{
				this._LotSerClassID = value;
			}
		}
		#endregion
		#region AssignedNbr
		public abstract class assignedNbr : PX.Data.IBqlField
		{
		}
		protected String _AssignedNbr;
		[PXString(30, IsUnicode = true)]
		public virtual String AssignedNbr
		{
			get
			{
				return this._AssignedNbr;
			}
			set
			{
				this._AssignedNbr = value;
			}
		}
		#endregion
		#region ExpireDate
		public abstract class expireDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _ExpireDate;
		[INExpireDate()]
		public virtual DateTime? ExpireDate
		{
			get
			{
				return this._ExpireDate;
			}
			set
			{
				this._ExpireDate = value;
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
		#region UOM
		public abstract class uOM : PX.Data.IBqlField
		{
		}
		protected String _UOM;
		[INUnit(typeof(INTranSplit.inventoryID), DisplayName="UOM", Enabled=false)]
		[PXDefault(typeof(INTran.uOM))]
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
		[PXDBQuantity(typeof(INTranSplit.uOM), typeof(INTranSplit.baseQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity")]
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
		#region BaseQty
		public abstract class baseQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _BaseQty;
		[PXDBQuantity()]
		public virtual Decimal? BaseQty
		{
			get
			{
				return this._BaseQty;
			}
			set
			{
				this._BaseQty = value;
			}
		}
		#endregion
     
		#region PlanID
		public abstract class planID : PX.Data.IBqlField
		{
		}
		protected Int64? _PlanID;
		[PXDBLong()]
		[INTranSplitPlanID(typeof(INRegister.noteID), typeof(INRegister.hold), typeof(INRegister.transferType))]
		public virtual Int64? PlanID
		{
			get
			{
				return this._PlanID;
			}
			set
			{
				this._PlanID = value;
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
	}

	[PXProjection(typeof(Select5<INTranCost,
		InnerJoin<INTranSplit, 
            On<INTranSplit.tranType, Equal<INTranCost.tranType>, 
            And<INTranSplit.refNbr, Equal<INTranCost.refNbr>, 
            And<INTranSplit.lineNbr, Equal<INTranCost.lineNbr>, 
            And<INTranSplit.costSubItemID, Equal<INTranCost.costSubItemID>, 
            And<INTranSplit.costSiteID, Equal<INTranCost.costSiteID>, 
			And<Where<INTranCost.lotSerialNbr, IsNull, Or<INTranSplit.lotSerialNbr, Equal<INTranCost.lotSerialNbr>>>>>>>>>,
		InnerJoin<INTran, On<INTran.tranType, Equal<INTranSplit.tranType>, And<INTran.refNbr, Equal<INTranSplit.refNbr>, And<INTran.lineNbr, Equal<INTranSplit.lineNbr>>>>>>,
		Aggregate<
			GroupBy<INTranSplit.tranType, 
			GroupBy<INTranSplit.refNbr, 
			GroupBy<INTranSplit.lineNbr, 
			GroupBy<INTranSplit.splitLineNbr,
			GroupBy<INTran.finPeriodID, 
			GroupBy<INTran.tranPeriodID, 
			GroupBy<INTranSplit.inventoryID, 
			GroupBy<INTranSplit.costSiteID, 
			GroupBy<INTranSplit.costSubItemID, 
			Sum<INTranCost.qty, 
			Sum<INTranCost.tranCost>>>>>>>>>>>>>))]
    [Serializable]
	public partial class INTranDetail : PX.Data.IBqlTable
	{
		#region TranType
		public abstract class tranType : PX.Data.IBqlField
		{
		}
		protected String _TranType;
		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(INTranSplit.tranType))]
		[INTranType.List()]
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
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(INTranSplit.refNbr))]
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
		[PXDBInt(IsKey = true, BqlField = typeof(INTranSplit.lineNbr))]
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
		#region SplitLineNbr
		public abstract class splitLineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _SplitLineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(INTranSplit.splitLineNbr))]
		public virtual Int32? SplitLineNbr
		{
			get
			{
				return this._SplitLineNbr;
			}
			set
			{
				this._SplitLineNbr = value;
			}
		}
		#endregion 
		#region TranDate
		public abstract class tranDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _TranDate;
		[PXDBDate(BqlField = typeof(INTranCost.tranDate))]
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
		[GL.FinPeriodID(BqlField = typeof(INTran.finPeriodID))]
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
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.IBqlField
		{
		}
		protected String _TranPeriodID;
		[GL.FinPeriodID(BqlField = typeof(INTran.tranPeriodID))]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[StockItem(BqlField = typeof(INTranCost.inventoryID))]
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
		#region CostSubItemID
		public abstract class costSubItemID : PX.Data.IBqlField
		{
		}
		protected Int32? _CostSubItemID;
		[SubItem(BqlField = typeof(INTranCost.costSubItemID))]
		public virtual Int32? CostSubItemID
		{
			get
			{
				return this._CostSubItemID;
			}
			set
			{
				this._CostSubItemID = value;
			}
		}
		#endregion
		#region CostSiteID
		public abstract class costSiteID : PX.Data.IBqlField
		{
		}
		protected Int32? _CostSiteID;
		[PXDBInt(BqlField = typeof(INTranCost.costSiteID))]
		public virtual Int32? CostSiteID
		{
			get
			{
				return this._CostSiteID;
			}
			set
			{
				this._CostSiteID = value;
			}
		}
		#endregion
		#region InvtMult
		public abstract class invtMult : PX.Data.IBqlField
		{
		}
		protected Int16? _InvtMult;
		[PXDBShort(BqlField = typeof(INTranCost.invtMult))]
		public virtual Int16? InvtMult
		{
			get
			{
				return this._InvtMult;
			}
			set
			{
				this._InvtMult = value;
			}
		}
		#endregion
		#region SumQty
		public abstract class sumQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _SumQty;
		[PXDBQuantity(BqlField = typeof(INTranCost.qty))]
		public virtual Decimal? SumQty
		{
			get
			{
				return this._SumQty;
			}
			set
			{
				this._SumQty = value;
			}
		}
		#endregion
		#region SumTranCost
		public abstract class sumTranCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _SumTranCost;
		[CM.PXDBBaseCury(BqlField = typeof(INTranCost.tranCost))]
		public virtual Decimal? SumTranCost
		{
			get
			{
				return this._SumTranCost;
			}
			set
			{
				this._SumTranCost = value;
			}
		}
		#endregion
		////INTranSplit
		#region SubItemID
		public abstract class subItemID : PX.Data.IBqlField
		{
		}
		protected Int32? _SubItemID;
		[SubItem(BqlField = typeof(INTranSplit.subItemID))]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.IBqlField
		{
		}
		protected Int32? _SiteID;
		[IN.Site(BqlField = typeof(INTranSplit.siteID))]
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
		#region LocationID
		public abstract class locationID : PX.Data.IBqlField
		{
		}
		protected Int32? _LocationID;
		[IN.Location(BqlField = typeof(INTranSplit.locationID))]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.IBqlField
		{
		}
		protected String _LotSerialNbr;
		[PXDBString(100, IsUnicode = true, BqlField = typeof(INTranSplit.lotSerialNbr))]
		public virtual String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.IBqlField
		{
		}
		protected String _UOM;
		[PXDBString(6, IsUnicode = true, BqlField = typeof(INTranSplit.uOM))]
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
		[PXDBQuantity(BqlField = typeof(INTranSplit.qty))]
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
		#region BaseQty
		public abstract class baseQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _BaseQty;
		[PXDBQuantity(BqlField = typeof(INTranSplit.baseQty))]
		public virtual Decimal? BaseQty
		{
			get
			{
				return this._BaseQty;
			}
			set
			{
				this._BaseQty = value;
			}
		}
		#endregion
		#region TranCost
		public abstract class tranCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranCost;
		[CM.PXBaseCury()]
		public virtual Decimal? TranCost
		{
			[PXDependsOnFields(typeof(tranType),typeof(sumTranCost),typeof(sumQty),typeof(baseQty))]
			get
			{
				if (TranType == INTranType.Adjustment || TranType == INTranType.StandardCostAdjustment || TranType == INTranType.NegativeCostAdjustment)
				{
					return SumTranCost;
				}
				else if (SumQty == 0m)
				{
					return 0m;
				}
				else
				{
					return BaseQty * SumTranCost / SumQty;
				}
			}
			set
			{
				this._TranCost = value;
			}
		}
		#endregion
	}

	[PXProjection(typeof(Select5<INTranSplit,
		InnerJoin<INTranCost,
				On<INTranCost.tranType,Equal<INTranSplit.tranType>,
					And<INTranCost.refNbr,Equal<INTranSplit.refNbr>,
					And<INTranCost.lineNbr,Equal<INTranSplit.lineNbr>,
					And<INTranCost.costSiteID, Equal<INTranSplit.costSiteID>,
					And<INTranCost.costSubItemID, Equal<INTranSplit.costSubItemID>,
					And<Where<INTranCost.lotSerialNbr, IsNull, Or<INTranCost.lotSerialNbr, Equal<INTranSplit.lotSerialNbr>>>>>>>>>>,
		Aggregate<
			GroupBy<INTranSplit.docType,
			GroupBy<INTranSplit.refNbr,
			GroupBy<INTranSplit.lineNbr,
			GroupBy<INTranSplit.splitLineNbr,
			Max<INTranSplit.baseQty,
			Sum<INTranCost.qty,
			Sum<INTranCost.tranCost>>>>>>>>>))]
    [Serializable]
	public partial class INTranSplitCost : PX.Data.IBqlTable  
	{
		#region DocType
		public abstract class docType : PX.Data.IBqlField
		{
		}
		protected String _DocType;
		[PXDBString(1, IsFixed = true, BqlField = typeof(INTranSplit.docType))]
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
		#region TranType
		public abstract class tranType : PX.Data.IBqlField
		{
		}
		protected String _TranType;
		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(INTranSplit.tranType))]
		[INTranType.List()]
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
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(INTranSplit.refNbr))]
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
		[PXDBInt(IsKey = true, BqlField = typeof(INTranSplit.lineNbr))]
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
		#region SplitLineNbr
		public abstract class splitLineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _SplitLineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(INTranSplit.splitLineNbr))]
		public virtual Int32? SplitLineNbr
		{
			get
			{
				return this._SplitLineNbr;
			}
			set
			{
				this._SplitLineNbr = value;
			}
		}
		#endregion

		#region TranSplitQty
		public abstract class tranSplitQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranSplitQty;
		[PXDBQuantity(BqlField = typeof(INTranSplit.baseQty))]
		public virtual Decimal? TranSplitQty
		{
			get
			{
				return this._TranSplitQty;
			}
			set
			{
				this._TranSplitQty = value;
			}
		}
		#endregion
		#region TranCostQty 
		// 1. should be equal to TranSplitQty ;
		// 2. assume that INTranCost.InvtMult = INTranSplit.InvtMult
		public abstract class tranCostQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranCostQty;
		[PXDBQuantity(BqlField = typeof(INTranCost.qty))]
		public virtual Decimal? TranCostQty
		{
			get
			{
				return this._TranCostQty;
			}
			set
			{
				this._TranCostQty = value;
			}
		}
		#endregion 
		#region TranCostCost
		public abstract class tranCostCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranCostCost;
		[CM.PXDBBaseCury(BqlField = typeof(INTranCost.tranCost))]
		public virtual Decimal? TranCostCost
		{
			get
			{
				return this._TranCostCost;
			}
			set
			{
				this._TranCostCost = value;
			}
		}
		#endregion
	}


	[PXProjection(typeof(Select5<INCostStatus,
		InnerJoin<INCostSubItemXRef, On<INCostSubItemXRef.costSubItemID, Equal<INCostStatus.costSubItemID>>,
		CrossJoin<INSetup>>, 
		Aggregate<GroupBy<INCostStatus.inventoryID, GroupBy<INCostStatus.costSiteID, GroupBy<INCostSubItemXRef.subItemID, Sum<INCostStatus.qtyOnHand, Sum<INCostStatus.totalCost>>>>>>>))]
    [Serializable]
	public partial class INSiteCostStatus : IBqlTable 
	{
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[PXDBInt(IsKey = true, BqlField = typeof(INCostStatus.inventoryID))]
		[PXDefault()]
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
		#region SubItemID
		public abstract class subItemID : PX.Data.IBqlField
		{
		}
		protected Int32? _SubItemID;
		[PXDBInt(IsKey = true, BqlField = typeof(INCostSubItemXRef.subItemID))]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.IBqlField
		{
		}
		protected Int32? _SiteID;
		[PXDBInt(IsKey = true, BqlField = typeof(INCostStatus.costSiteID))]
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
		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyOnHand;
		[PXDBQuantity(BqlField=typeof(INCostStatus.qtyOnHand))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? QtyOnHand
		{
			get
			{
				return this._QtyOnHand;
			}
			set
			{
				this._QtyOnHand = value;
			}
		}
		#endregion
		#region TotalCost
		public abstract class totalCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _TotalCost;
		[CM.PXDBBaseCury(BqlField=typeof(INCostStatus.totalCost))]
		public virtual Decimal? TotalCost
		{
			get
			{
				return this._TotalCost;
			}
			set
			{
				this._TotalCost = value;
			}
		}
		#endregion
		#region DecPlPrcCst
		public abstract class decPlPrcCst : PX.Data.IBqlField
		{
		}
		protected Int16? _DecPlPrcCst;
		[PXDBShort(BqlField=typeof(INSetup.decPlPrcCst))]
		public virtual Int16? DecPlPrcCst
		{
			get
			{
				return this._DecPlPrcCst;
			}
			set
			{
				this._DecPlPrcCst = value;
			}
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _UnitCost;
		[PXPriceCost()]
		public virtual Decimal? UnitCost
		{
			[PXDependsOnFields(typeof(qtyOnHand),typeof(totalCost),typeof(decPlPrcCst))]
			get
			{
				return (this.QtyOnHand != 0m && this.QtyOnHand != null && this.TotalCost != null) ? Math.Round((decimal)this.TotalCost / (decimal)this.QtyOnHand, (int)this.DecPlPrcCst, MidpointRounding.AwayFromZero) : 0m;
			}
			set
			{
			}
		}
		#endregion
	}

	[PXProjection(typeof(Select5<INCostStatus,
		InnerJoin<INLocation, 
			On<INLocation.locationID, Equal<INCostStatus.costSiteID>>,
		InnerJoin<INCostSubItemXRef, On<INCostSubItemXRef.costSubItemID, Equal<INCostStatus.costSubItemID>>,
		CrossJoin<INSetup>>>,
		Aggregate<GroupBy<INCostStatus.inventoryID, GroupBy<INCostSubItemXRef.subItemID, GroupBy<INCostStatus.costSiteID, Sum<INCostStatus.qtyOnHand, Sum<INCostStatus.totalCost>>>>>>>))]
    [Serializable]
	public partial class INLocationCostStatus : IBqlTable
	{
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[PXDBInt(IsKey = true, BqlField = typeof(INCostStatus.inventoryID))]
		[PXDefault()]
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
		#region SubItemID
		public abstract class subItemID : PX.Data.IBqlField
		{
		}
		protected Int32? _SubItemID;
		[PXDBInt(IsKey = true, BqlField = typeof(INCostSubItemXRef.subItemID))]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.IBqlField
		{
		}
		protected Int32? _LocationID;
		[PXDBInt(IsKey = true, BqlField = typeof(INCostStatus.costSiteID))]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyOnHand;
		[PXDBQuantity(BqlField = typeof(INCostStatus.qtyOnHand))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? QtyOnHand
		{
			get
			{
				return this._QtyOnHand;
			}
			set
			{
				this._QtyOnHand = value;
			}
		}
		#endregion
		#region TotalCost
		public abstract class totalCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _TotalCost;
		[CM.PXDBBaseCury(BqlField = typeof(INCostStatus.totalCost))]
		public virtual Decimal? TotalCost
		{
			get
			{
				return this._TotalCost;
			}
			set
			{
				this._TotalCost = value;
			}
		}
		#endregion
		#region DecPlPrcCst
		public abstract class decPlPrcCst : PX.Data.IBqlField
		{
		}
		protected Int16? _DecPlPrcCst;
		[PXDBShort(BqlField = typeof(INSetup.decPlPrcCst))]
		public virtual Int16? DecPlPrcCst
		{
			get
			{
				return this._DecPlPrcCst;
			}
			set
			{
				this._DecPlPrcCst = value;
			}
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _UnitCost;
		[PXPriceCost()]
		public virtual Decimal? UnitCost
		{
			[PXDependsOnFields(typeof(qtyOnHand), typeof(totalCost), typeof(decPlPrcCst))]
			get
			{
				return (this.QtyOnHand != 0m && this.QtyOnHand != null && this.TotalCost != null) ? Math.Round((decimal)this.TotalCost / (decimal)this.QtyOnHand, (int)this.DecPlPrcCst, MidpointRounding.AwayFromZero) : 0m;
			}
			set
			{
			}
		}
		#endregion
	}

	[PXProjection(typeof(Select5<INCostStatus,
		InnerJoin<INCostSubItemXRef, On<INCostSubItemXRef.costSubItemID, Equal<INCostStatus.costSubItemID>>,
		CrossJoin<INSetup>>,
		Aggregate<GroupBy<INCostStatus.inventoryID, GroupBy<INCostSubItemXRef.subItemID, GroupBy<INCostStatus.costSiteID, GroupBy<INCostStatus.lotSerialNbr, Sum<INCostStatus.qtyOnHand, Sum<INCostStatus.totalCost>>>>>>>>))]
    [Serializable]
	public partial class INLotSerialCostStatus : IBqlTable
	{ 
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[PXDBInt(IsKey = true, BqlField = typeof(INCostStatus.inventoryID))]
		[PXDefault()]
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
		#region SubItemID
		public abstract class subItemID : PX.Data.IBqlField
		{
		}
		protected Int32? _SubItemID;
		[PXDBInt(IsKey = true, BqlField = typeof(INCostSubItemXRef.subItemID))]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.IBqlField
		{
		}
		protected Int32? _SiteID;
		[PXDBInt(IsKey = true, BqlField = typeof(INCostStatus.costSiteID))]
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
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.IBqlField
		{
		}
		protected String _LotSerialNbr;
		[PXDBString(100, IsUnicode = true, IsKey = true, BqlField = typeof(INCostStatus.lotSerialNbr))]
		[PXDefault()]
		public virtual String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyOnHand;
		[PXDBQuantity(BqlField=typeof(INCostStatus.qtyOnHand))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? QtyOnHand
		{
			get
			{
				return this._QtyOnHand;
			}
			set
			{
				this._QtyOnHand = value;
			}
		}
		#endregion
		#region TotalCost
		public abstract class totalCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _TotalCost;
		[CM.PXDBBaseCury(BqlField=typeof(INCostStatus.totalCost))]
		public virtual Decimal? TotalCost
		{
			get
			{
				return this._TotalCost;
			}
			set
			{
				this._TotalCost = value;
			}
		}
		#endregion
		#region DecPlPrcCst
		public abstract class decPlPrcCst : PX.Data.IBqlField
		{
		}
		protected Int16? _DecPlPrcCst;
		[PXDBShort(BqlField = typeof(INSetup.decPlPrcCst))]
		public virtual Int16? DecPlPrcCst
		{
			get
			{
				return this._DecPlPrcCst;
			}
			set
			{
				this._DecPlPrcCst = value;
			}
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _UnitCost;
		[PXPriceCost()]
		public virtual Decimal? UnitCost
		{
			[PXDependsOnFields(typeof(qtyOnHand),typeof(totalCost), typeof(decPlPrcCst))]
			get
			{
				return (this.QtyOnHand != 0m && this.QtyOnHand != null && this.TotalCost != null) ? Math.Round((decimal)this.TotalCost / (decimal)this.QtyOnHand, (int)this.DecPlPrcCst, MidpointRounding.AwayFromZero) : 0m;
			}
			set
			{
			}
		}
		#endregion
	}
}
