namespace PX.Objects.IN
{
	using System;
	using PX.Data;
	using PX.Objects.GL;
	using PX.Objects.CS;
	using PX.Objects.TX;
	using PX.TM;
	using PX.Objects.EP;
	using PX.Objects.DR;
	using PX.Objects.CR;
	using PX.Objects.AP;

	[System.SerializableAttribute()]
	[PXPrimaryGraph(new Type[] {
					typeof(NonStockItemMaint),
					typeof(InventoryItemMaint)},
				new Type[] {
                    typeof(Where<InventoryItem.stkItem, Equal<False>>),
                    typeof(Where<InventoryItem.stkItem, Equal<True>>)
					})]
	[PXCacheName(Messages.InventoryItem)]
	public partial class InventoryItem : PX.Data.IBqlTable, PX.SM.IIncludable, ILotSerNumVal, IAttributeSupport
	{
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[PXDBIdentity()]
		[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible, Visible = false)]
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
		#region InventoryCD
		public abstract class inventoryCD : PX.Data.IBqlField
		{
		}
		protected String _InventoryCD;
		[PXDefault()]
		[InventoryRaw(IsKey = true, DisplayName = "Inventory ID")]
		[PX.Data.EP.PXFieldDescription]
		public virtual String InventoryCD
		{
			get
			{
				return this._InventoryCD;
			}
			set
			{
				this._InventoryCD = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected String _Descr;
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PX.Data.EP.PXFieldDescription]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region ItemClassID
		public abstract class itemClassID : PX.Data.IBqlField
		{
		}
		protected String _ItemClassID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Item Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<INItemClass.itemClassID, Where<INItemClass.stkItem, Equal<boolTrue>>>), DescriptionField = typeof(INItemClass.descr))]
		//[PXDefault(typeof(Search<INSetup.dfltItemClassID>))]
		public virtual String ItemClassID
		{
			get
			{
				return this._ItemClassID;
			}
			set
			{
				this._ItemClassID = value;
			}
		}
		#endregion
		#region ItemStatus
		public abstract class itemStatus : PX.Data.IBqlField
		{
		}
		protected String _ItemStatus;
		[PXDBString(2, IsFixed = true)]
		[PXDefault("AC")]
		[PXUIField(DisplayName = "Item Status", Visibility = PXUIVisibility.SelectorVisible)]
		[InventoryItemStatus.List]
		public virtual String ItemStatus
		{
			get
			{
				return this._ItemStatus;
			}
			set
			{
				this._ItemStatus = value;
			}
		}
		#endregion
		#region ItemType
		public abstract class itemType : PX.Data.IBqlField
		{
		}
		protected String _ItemType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INItemTypes.FinishedGood, typeof(Search<INItemClass.itemType, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>))]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible)]
		[INItemTypes.List()]
		public virtual String ItemType
		{
			get
			{
				return this._ItemType;
			}
			set
			{
				this._ItemType = value;
			}
		}
		#endregion
		#region ValMethod
		public abstract class valMethod : PX.Data.IBqlField
		{
		}
		protected String _ValMethod;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INValMethod.Standard, typeof(Search<INItemClass.valMethod, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>))]
		[PXUIField(DisplayName = "Valuation Method")]
		[INValMethod.List()]
		public virtual String ValMethod
		{
			get
			{
				return this._ValMethod;
			}
			set
			{
				this._ValMethod = value;
			}
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.IBqlField
		{
		}
		protected String _TaxCategoryID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<INItemClass.taxCategoryID, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>))]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
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
		#region BaseUnit
		public abstract class baseUnit : PX.Data.IBqlField
		{
		}
		protected String _BaseUnit;
		[PXDefault(typeof(Search<INItemClass.baseUnit, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>))]
		[INUnit(DisplayName = "Base Unit", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String BaseUnit
		{
			get
			{
				return this._BaseUnit;
			}
			set
			{
				this._BaseUnit = value;
			}
		}
		#endregion
		#region SalesUnit
		public abstract class salesUnit : PX.Data.IBqlField
		{
		}
		protected String _SalesUnit;
		[PXDefault(typeof(Search<INItemClass.salesUnit, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>))]
		[PXFormula(typeof(Switch<Case<Where<FeatureInstalled<FeaturesSet.multipleUnitMeasure>>, Current<salesUnit>>, baseUnit>))]
		[INUnit(null, typeof(InventoryItem.baseUnit), DisplayName = "Sales Unit", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String SalesUnit
		{
			get
			{
				return this._SalesUnit;
			}
			set
			{
				this._SalesUnit = value;
			}
		}
		#endregion
		#region PurchaseUnit
		public abstract class purchaseUnit : PX.Data.IBqlField
		{
		}
		protected String _PurchaseUnit;
		[PXDefault(typeof(Search<INItemClass.purchaseUnit, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>))]
		[PXFormula(typeof(Switch<Case<Where<FeatureInstalled<FeaturesSet.multipleUnitMeasure>>, Current<purchaseUnit>>, baseUnit>))]
		[INUnit(null, typeof(InventoryItem.baseUnit), DisplayName = "Purchase Unit", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String PurchaseUnit
		{
			get
			{
				return this._PurchaseUnit;
			}
			set
			{
				this._PurchaseUnit = value;
			}
		}
		#endregion
		#region Commisionable
		public abstract class commisionable : PX.Data.IBqlField
		{
		}
		protected Boolean? _Commisionable;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Subject to Commission", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? Commisionable
		{
			get
			{
				return this._Commisionable;
			}
			set
			{
				this._Commisionable = value;
			}
		}
		#endregion
		#region ReasonCodeSubID
		public abstract class reasonCodeSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _ReasonCodeSubID;
		[PXDefault(typeof(Search<INPostClass.reasonCodeSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(DisplayName = "Reason Code Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? ReasonCodeSubID
		{
			get
			{
				return this._ReasonCodeSubID;
			}
			set
			{
				this._ReasonCodeSubID = value;
			}
		}
		#endregion
		#region SalesAcctID
		public abstract class salesAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _SalesAcctID;
		[Account(DisplayName = "Sales Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXDefault(typeof(Search<INPostClass.salesAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? SalesAcctID
		{
			get
			{
				return this._SalesAcctID;
			}
			set
			{
				this._SalesAcctID = value;
			}
		}
		#endregion
		#region SalesSubID
		public abstract class salesSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _SalesSubID;
		[SubAccount(typeof(InventoryItem.salesAcctID), DisplayName = "Sales Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.salesSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? SalesSubID
		{
			get
			{
				return this._SalesSubID;
			}
			set
			{
				this._SalesSubID = value;
			}
		}
		#endregion
		#region InvtAcctID
		public abstract class invtAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _InvtAcctID;
		[Account(DisplayName = "Inventory Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXDefault(typeof(Search<INPostClass.invtAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? InvtAcctID
		{
			get
			{
				return this._InvtAcctID;
			}
			set
			{
				this._InvtAcctID = value;
			}
		}
		#endregion
		#region InvtSubID
		public abstract class invtSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _InvtSubID;
		[SubAccount(typeof(InventoryItem.invtAcctID), DisplayName = "Inventory Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.invtSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? InvtSubID
		{
			get
			{
				return this._InvtSubID;
			}
			set
			{
				this._InvtSubID = value;
			}
		}
		#endregion
		#region COGSAcctID
		public abstract class cOGSAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _COGSAcctID;
		[Account(DisplayName = "COGS Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXDefault(typeof(Search<INPostClass.cOGSAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? COGSAcctID
		{
			get
			{
				return this._COGSAcctID;
			}
			set
			{
				this._COGSAcctID = value;
			}
		}
		#endregion
		#region COGSSubID
		public abstract class cOGSSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _COGSSubID;
		[SubAccount(typeof(InventoryItem.cOGSAcctID), DisplayName = "COGS Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.cOGSSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? COGSSubID
		{
			get
			{
				return this._COGSSubID;
			}
			set
			{
				this._COGSSubID = value;
			}
		}
		#endregion
		#region DiscAcctID
		public abstract class discAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _DiscAcctID;
		[Account(DisplayName = "Discount Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXDefault(typeof(Search<INPostClass.discAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? DiscAcctID
		{
			get
			{
				return this._DiscAcctID;
			}
			set
			{
				this._DiscAcctID = value;
			}
		}
		#endregion
		#region DiscSubID
		public abstract class discSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _DiscSubID;
		[SubAccount(typeof(InventoryItem.discAcctID), DisplayName = "Discount Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.discSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? DiscSubID
		{
			get
			{
				return this._DiscSubID;
			}
			set
			{
				this._DiscSubID = value;
			}
		}
		#endregion
		#region StdCstRevAcctID
		public abstract class stdCstRevAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _StdCstRevAcctID;
		[Account(DisplayName = "Standard Cost Revaluation Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXDefault(typeof(Search<INPostClass.stdCstRevAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? StdCstRevAcctID
		{
			get
			{
				return this._StdCstRevAcctID;
			}
			set
			{
				this._StdCstRevAcctID = value;
			}
		}
		#endregion
		#region StdCstRevSubID
		public abstract class stdCstRevSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _StdCstRevSubID;
		[SubAccount(typeof(InventoryItem.stdCstRevAcctID), DisplayName = "Standard Cost Revaluation Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.stdCstRevSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? StdCstRevSubID
		{
			get
			{
				return this._StdCstRevSubID;
			}
			set
			{
				this._StdCstRevSubID = value;
			}
		}
		#endregion
		#region StdCstVarAcctID
		public abstract class stdCstVarAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _StdCstVarAcctID;
		[Account(DisplayName = "Standard Cost Variance Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXDefault(typeof(Search<INPostClass.stdCstVarAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? StdCstVarAcctID
		{
			get
			{
				return this._StdCstVarAcctID;
			}
			set
			{
				this._StdCstVarAcctID = value;
			}
		}
		#endregion
		#region StdCstVarSubID
		public abstract class stdCstVarSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _StdCstVarSubID;
		[SubAccount(typeof(InventoryItem.stdCstVarAcctID), DisplayName = "Standard Cost Variance Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.stdCstVarSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? StdCstVarSubID
		{
			get
			{
				return this._StdCstVarSubID;
			}
			set
			{
				this._StdCstVarSubID = value;
			}
		}
		#endregion
		#region PPVAcctID
		public abstract class pPVAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _PPVAcctID;
		[Account(DisplayName = "Purchase Price Variance Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXDefault(typeof(Search<INPostClass.pPVAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? PPVAcctID
		{
			get
			{
				return this._PPVAcctID;
			}
			set
			{
				this._PPVAcctID = value;
			}
		}
		#endregion
		#region PPVSubID
		public abstract class pPVSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _PPVSubID;
		[SubAccount(typeof(InventoryItem.pPVAcctID), DisplayName = "Purchase Price Variance Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.pPVSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? PPVSubID
		{
			get
			{
				return this._PPVSubID;
			}
			set
			{
				this._PPVSubID = value;
			}
		}
		#endregion
		#region POAccrualAcctID
		public abstract class pOAccrualAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _POAccrualAcctID;
		[Account(DisplayName = "PO Accrual Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXDefault(typeof(Search<INPostClass.pOAccrualAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? POAccrualAcctID
		{
			get
			{
				return this._POAccrualAcctID;
			}
			set
			{
				this._POAccrualAcctID = value;
			}
		}
		#endregion
		#region POAccrualSubID
		public abstract class pOAccrualSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _POAccrualSubID;
		[SubAccount(typeof(InventoryItem.pOAccrualAcctID), DisplayName = "PO Accrual Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.pOAccrualSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? POAccrualSubID
		{
			get
			{
				return this._POAccrualSubID;
			}
			set
			{
				this._POAccrualSubID = value;
			}
		}
		#endregion
		#region LCVarianceAcctID
		public abstract class lCVarianceAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _LCVarianceAcctID;
		[Account(DisplayName = "Landed Cost Variance Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXDefault(typeof(Search<INPostClass.lCVarianceAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? LCVarianceAcctID
		{
			get
			{
				return this._LCVarianceAcctID;
			}
			set
			{
				this._LCVarianceAcctID = value;
			}
		}
		#endregion
		#region LCVarianceSubID
		public abstract class lCVarianceSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _LCVarianceSubID;
		[SubAccount(typeof(InventoryItem.lCVarianceAcctID), DisplayName = "Landed Cost Variance Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.lCVarianceSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? LCVarianceSubID
		{
			get
			{
				return this._LCVarianceSubID;
			}
			set
			{
				this._LCVarianceSubID = value;
			}
		}
		#endregion
		#region LastSiteID
		public abstract class lastSiteID : PX.Data.IBqlField
		{
		}
		protected Int32? _LastSiteID;
		[PXDBInt()]
		public virtual Int32? LastSiteID
		{
			get
			{
				return this._LastSiteID;
			}
			set
			{
				this._LastSiteID = value;
			}
		}
		#endregion
		#region LastStdCost
		public abstract class lastStdCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _LastStdCost;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last Cost", Enabled=false)]
		public virtual Decimal? LastStdCost
		{
			get
			{
				return this._LastStdCost;
			}
			set
			{
				this._LastStdCost = value;
			}
		}
		#endregion
		#region PendingStdCost
		public abstract class pendingStdCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _PendingStdCost;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Pending Cost")]
		public virtual Decimal? PendingStdCost
		{
			get
			{
				return this._PendingStdCost;
			}
			set
			{
				this._PendingStdCost = value;
			}
		}
		#endregion
		#region PendingStdCostDate
		public abstract class pendingStdCostDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _PendingStdCostDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Pending Cost Date")]
        [PXFormula(typeof(Switch<Case<Where<InventoryItem.pendingStdCost, NotEqual<CS.decimal0>>, Current<AccessInfo.businessDate>>, InventoryItem.pendingStdCostDate>))]
		public virtual DateTime? PendingStdCostDate
		{
			get
			{
				return this._PendingStdCostDate;
			}
			set
			{
				this._PendingStdCostDate = value;
			}
		}
		#endregion
		#region StdCost
		public abstract class stdCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _StdCost;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Current Cost", Enabled = false)]
		public virtual Decimal? StdCost
		{
			get
			{
				return this._StdCost;
			}
			set
			{
				this._StdCost = value;
			}
		}
		#endregion
		#region StdCostDate
		public abstract class stdCostDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _StdCostDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Effective Date", Enabled = false)]
		public virtual DateTime? StdCostDate
		{
			get
			{
				return this._StdCostDate;
			}
			set
			{
				this._StdCostDate = value;
			}
		}
		#endregion
		#region LastBasePrice
		public abstract class lastBasePrice : PX.Data.IBqlField
		{
		}
		protected Decimal? _LastBasePrice;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last Price", Enabled = false)]
		public virtual Decimal? LastBasePrice
		{
			get
			{
				return this._LastBasePrice;
			}
			set
			{
				this._LastBasePrice = value;
			}
		}
		#endregion
		#region PendingBasePrice
		public abstract class pendingBasePrice : PX.Data.IBqlField
		{
		}
		protected Decimal? _PendingBasePrice;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Pending Price")]
		public virtual Decimal? PendingBasePrice
		{
			get
			{
				return this._PendingBasePrice;
			}
			set
			{
				this._PendingBasePrice = value;
			}
		}
		#endregion
		#region PendingBasePriceDate
		public abstract class pendingBasePriceDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _PendingBasePriceDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Pending Price Date", Enabled = true)]
        [PXFormula(typeof(Switch<Case<Where<InventoryItem.pendingBasePrice, NotEqual<CS.decimal0>, And<Current<InventoryItem.pendingBasePriceDate>, IsNull>>, Current<AccessInfo.businessDate>>, InventoryItem.pendingBasePriceDate>))]
		public virtual DateTime? PendingBasePriceDate
		{
			get
			{
				return this._PendingBasePriceDate;
			}
			set
			{
				this._PendingBasePriceDate = value;
			}
		}
		#endregion
		#region BasePrice
		public abstract class basePrice : PX.Data.IBqlField
		{
		}
		protected Decimal? _BasePrice;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Current Price", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? BasePrice
		{
			get
			{
				return this._BasePrice;
			}
			set
			{
				this._BasePrice = value;
			}
		}
		#endregion
		#region BasePriceDate
		public abstract class basePriceDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _BasePriceDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Effective Date", Enabled = false)]
		public virtual DateTime? BasePriceDate
		{
			get
			{
				return this._BasePriceDate;
			}
			set
			{
				this._BasePriceDate = value;
			}
		}
		#endregion	
		#region BaseWeight
		public abstract class baseWeight : PX.Data.IBqlField
		{
		}
		protected Decimal? _BaseWeight;

		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseWeight
		{
			get
			{
				return this._BaseWeight;
			}
			set
			{
				this._BaseWeight = value;
			}
		}
		#endregion
		#region BaseVolume
		public abstract class baseVolume : PX.Data.IBqlField
		{
		}
		protected Decimal? _BaseVolume;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Volume")]
		public virtual Decimal? BaseVolume
		{
			get
			{
				return this._BaseVolume;
			}
			set
			{
				this._BaseVolume = value;
			}
		}
		#endregion
		#region BaseItemWeight
		public abstract class baseItemWeight : PX.Data.IBqlField
		{
		}
		protected Decimal? _BaseItemWeight;
		[PXDBQuantity(6, typeof(InventoryItem.weightUOM), typeof(InventoryItem.baseWeight), HandleEmptyKey = true)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Weight")]
		public virtual Decimal? BaseItemWeight
		{
			get
			{
				return this._BaseItemWeight;
			}
			set
			{
				this._BaseItemWeight = value;
			}
		}
		#endregion
		#region BaseItemVolume
		public abstract class baseItemVolume : PX.Data.IBqlField
		{
		}
		protected Decimal? _BaseItemVolume;
		[PXDBQuantity(6,typeof(InventoryItem.volumeUOM), typeof(InventoryItem.baseVolume), HandleEmptyKey = true)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Volume")]
		public virtual Decimal? BaseItemVolume
		{
			get
			{
				return this._BaseItemVolume;
			}
			set
			{
				this._BaseItemVolume = value;
			}
		}
		#endregion
		#region WeightUOM
		public abstract class weightUOM : PX.Data.IBqlField
		{
		}
		protected String _WeightUOM;
		[INUnit(null, typeof(INSetup.weightUOM), DisplayName = "Weight UOM")]
		public virtual String WeightUOM
		{
			get
			{
				return this._WeightUOM;
			}
			set
			{
				this._WeightUOM = value;
			}
		}
		#endregion
		#region VolumeUOM
		public abstract class volumeUOM : PX.Data.IBqlField
		{
		}
		protected String _VolumeUOM;
		[INUnit(null, typeof(INSetup.volumeUOM), DisplayName = "Volume UOM")]
		public virtual String VolumeUOM
		{
			get
			{
				return this._VolumeUOM;
			}
			set
			{
				this._VolumeUOM = value;
			}
		}
		#endregion
		#region PackSeparately
		public abstract class packSeparately : PX.Data.IBqlField
		{
		}
		protected Boolean? _PackSeparately;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Pack Separately", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? PackSeparately
		{
			get
			{
				return this._PackSeparately;
			}
			set
			{
				this._PackSeparately = value;
			}
		}
		#endregion
		#region PackageOption
		public abstract class packageOption : PX.Data.IBqlField
		{
		}
		protected String _PackageOption;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INPackageOption.Manual)]
		[PXUIField(DisplayName = "Packaging Option")]
		[INPackageOption.List]
		public virtual String PackageOption
		{
			get
			{
				return this._PackageOption;
			}
			set
			{
				this._PackageOption = value;
			}
		}
		#endregion
		#region PreferredVendorID
		public abstract class preferredVendorID : PX.Data.IBqlField
		{
		}
		protected Int32? _PreferredVendorID;
		[AP.VendorNonEmployeeActive(DisplayName = "Preferred Vendor", Required = false, DescriptionField = typeof(AP.Vendor.acctName))]
		public virtual Int32? PreferredVendorID
		{
			get
			{
				return this._PreferredVendorID;
			}
			set
			{
				this._PreferredVendorID = value;
			}
		}
		#endregion
		#region PreferredVendorLocationID
		public abstract class preferredVendorLocationID : PX.Data.IBqlField
		{
		}
		protected Int32? _PreferredVendorLocationID;
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<InventoryItem.preferredVendorID>>>),
			DescriptionField = typeof(Location.descr), DisplayName = "Preferred Location")]
		public virtual Int32? PreferredVendorLocationID
		{
			get
			{
				return this._PreferredVendorLocationID;
			}
			set
			{
				this._PreferredVendorLocationID = value;
			}
		}
		#endregion
		#region DefaultSubItemID
		public abstract class defaultSubItemID : PX.Data.IBqlField
		{
		}
		protected Int32? _DefaultSubItemID;
		[IN.SubItem(typeof(InventoryItem.inventoryID), DisplayName="Default Subitem")]
		public virtual Int32? DefaultSubItemID
		{
			get
			{
				return this._DefaultSubItemID;
			}
			set
			{
				this._DefaultSubItemID = value;
			}
		}
		#endregion
		#region DefaultSubItemOnEntry
		public abstract class defaultSubItemOnEntry : PX.Data.IBqlField
		{
		}
		protected Boolean? _DefaultSubItemOnEntry;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use On Entry")]
		public virtual Boolean? DefaultSubItemOnEntry
		{
			get
			{
				return this._DefaultSubItemOnEntry;
			}
			set
			{
				this._DefaultSubItemOnEntry = value;
			}
		}
		#endregion
		#region DfltSiteID
		public abstract class dfltSiteID : PX.Data.IBqlField
		{
		}
		protected Int32? _DfltSiteID;
		[IN.Site(DisplayName = "Default Warehouse", DescriptionField = typeof(INSite.descr))]
		[PXDefault(typeof(Search<INItemClass.dfltSiteID, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? DfltSiteID
		{
			get
			{
				return this._DfltSiteID;
			}
			set
			{
				this._DfltSiteID = value;
			}
		}
		#endregion
		#region DfltShipLocationID
		public abstract class dfltShipLocationID : PX.Data.IBqlField
		{
		}
		protected Int32? _DfltShipLocationID;
		[IN.Location(typeof(InventoryItem.dfltSiteID), DisplayName = "Default Issue From", KeepEntry = false, DescriptionField = typeof(INLocation.descr))]
		public virtual Int32? DfltShipLocationID
		{
			get
			{
				return this._DfltShipLocationID;
			}
			set
			{
				this._DfltShipLocationID = value;
			}
		}
		#endregion
		#region DfltReceiptLocationID
		public abstract class dfltReceiptLocationID : PX.Data.IBqlField
		{
		}
		protected Int32? _DfltReceiptLocationID;
		[IN.Location(typeof(InventoryItem.dfltSiteID), DisplayName = "Default Receipt To", KeepEntry = false, DescriptionField = typeof(INLocation.descr))]
		public virtual Int32? DfltReceiptLocationID
		{
			get
			{
				return this._DfltReceiptLocationID;
			}
			set
			{
				this._DfltReceiptLocationID = value;
			}
		}
		#endregion
		#region ProductWorkgroupID
		public abstract class productWorkgroupID : PX.Data.IBqlField
		{
		}
		protected Int32? _ProductWorkgroupID;
		[PXDBInt()]
		[PXWorkgroupSelector]
		[PXUIField(DisplayName = "Product Workgroup")]
		public virtual Int32? ProductWorkgroupID
		{
			get
			{
				return this._ProductWorkgroupID;
			}
			set
			{
				this._ProductWorkgroupID = value;
			}
		}
		#endregion
		#region ProductManagerID
		public abstract class productManagerID : PX.Data.IBqlField
		{
		}
		protected Guid? _ProductManagerID;
		[PXDBGuid()]
		[PXOwnerSelector(typeof(InventoryItem.productWorkgroupID))]
		[PXUIField(DisplayName = "Product Manager")]
		public virtual Guid? ProductManagerID
		{
			get
			{
				return this._ProductManagerID;
			}
			set
			{
				this._ProductManagerID = value;
			}
		}
		#endregion
		#region PriceWorkgroupID
		public abstract class priceWorkgroupID : PX.Data.IBqlField
		{
		}
		protected Int32? _PriceWorkgroupID;
		[PXDBInt()]
		[PXWorkgroupSelector]
		[PXUIField(DisplayName = "Price Workgroup")]
		public virtual Int32? PriceWorkgroupID
		{
			get
			{
				return this._PriceWorkgroupID;
			}
			set
			{
				this._PriceWorkgroupID = value;
			}
		}
		#endregion
		#region PriceManagerID
		public abstract class priceManagerID : PX.Data.IBqlField
		{
		}
		protected Guid? _PriceManagerID;
		[PXDBGuid()]
		[PXOwnerSelector(typeof(InventoryItem.priceWorkgroupID))]
		[PXUIField(DisplayName = "Price Manager")]
		public virtual Guid? PriceManagerID
		{
			get
			{
				return this._PriceManagerID;
			}
			set
			{
				this._PriceManagerID = value;
			}
		}
		#endregion
		#region StkItem
		public abstract class stkItem : PX.Data.IBqlField
		{
		}
		protected Boolean? _StkItem;
		[PXDBBool()]
		[PXDefault(true)]
        [PXUIField(DisplayName = "Stock Item")]
		public virtual Boolean? StkItem
		{
			get
			{
				return this._StkItem;
			}
			set
			{
				this._StkItem = value;
			}
		}
		#endregion
		#region NegQty
		public abstract class negQty : PX.Data.IBqlField
		{
		}
		protected bool? _NegQty;
		[PXBool()]
		[PXDBScalar(typeof(Search<INItemClass.negQty, Where<INItemClass.itemClassID, Equal<InventoryItem.itemClassID>>>))]
		public virtual bool? NegQty
		{
			get
			{
				return this._NegQty;
			}
			set
			{
				this._NegQty = value;
			}
		}
		#endregion
		#region LotSerClassID
		public abstract class lotSerClassID : PX.Data.IBqlField
		{
		}
		protected String _LotSerClassID;
		[PXDBString(10, IsUnicode = true)]
		//[PXSelector(typeof(INLotSerClass.lotSerClassID), DescriptionField = typeof(INLotSerClass.descr))]
		[PXUIField(DisplayName = "Lot/Serial Class")]
		//[PXDefault(typeof(Search<INItemClass.lotSerClassID, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>))]
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
		#region LotSerNumSharedVal
		public abstract class lotSerNumSharedVal : PX.Data.IBqlField
		{
		}
		protected String _LotSerNumSharedVal;
		[PXString]
		public virtual String LotSerNumSharedVal
		{
			get
			{
				return this._LotSerNumSharedVal;
			}
			set
			{
				this._LotSerNumSharedVal = value;
			}
		}
		#endregion
		#region LotSerNumShared
		public abstract class lotSerNumShared : PX.Data.IBqlField
		{
		}
		protected Boolean? _LotSerNumShared;
		[PXBool]
		public virtual Boolean? LotSerNumShared
		{
			get
			{
				return this._LotSerNumShared;
			}
			set
			{
				this._LotSerNumShared = value;
			}
		}
		#endregion
		#region LotSerNumVal
		public abstract class lotSerNumVal : PX.Data.IBqlField
		{
		}
		protected String _LotSerNumVal;
		[PXDBString(30, IsUnicode = true, InputMask = "999999999999999999999999999999")]
		[PXDefault(typeof(Search<INLotSerClass.lotSerNumVal, Where<INLotSerClass.lotSerClassID, Equal<Current<InventoryItem.lotSerClassID>>, And<INLotSerClass.lotSerNumShared, Equal<boolTrue>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Auto-Incremental Value")]
		public virtual String LotSerNumVal
		{
			get
			{
				return this._LotSerNumVal;
			}
			set
			{
				this._LotSerNumVal = value;
			}
		}
		#endregion
		#region LotSerNumberResult
		public abstract class lotSerNumberResult : PX.Data.IBqlField
		{
		}
		protected String _LotSerNumberResult;
		[PXString(30, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Result of Generation Lot/Serial Number", Enabled = false)]
		public virtual String LotSerNumberResult
		{
			get
			{
				return this._LotSerNumberResult;
			}
			set
			{
				this._LotSerNumberResult = value;
			}
		}
		#endregion
		#region PostClassID
		public abstract class postClassID : PX.Data.IBqlField
		{
		}
		protected String _PostClassID;
		[PXDBString(10, IsUnicode = true)]
		//[PXSelector(typeof(INPostClass.postClassID), DescriptionField = typeof(INPostClass.descr))]
		[PXUIField(DisplayName = "Posting Class")]
		//[PXDefault(typeof(Search<INItemClass.postClassID, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>))]
		public virtual String PostClassID
		{
			get
			{
				return this._PostClassID;
			}
			set
			{
				this._PostClassID = value;
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
		[PXSelector(typeof(Search<DRDeferredCode.deferredCodeID>))]
		[PXDefault(typeof(Search<INItemClass.deferredCode, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region PriceClassID
		public abstract class priceClassID : PX.Data.IBqlField
		{
		}
		protected String _PriceClassID;
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(INPriceClass.priceClassID), DescriptionField = typeof(INPriceClass.description))]
		[PXUIField(DisplayName = "Price Class", Visibility = PXUIVisibility.Visible)]
		[PXDefault(typeof(Search<INItemClass.priceClassID, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String PriceClassID
		{
			get
			{
				return this._PriceClassID;
			}
			set
			{
				this._PriceClassID = value;
			}
		}
		#endregion
		#region IsSplitted
		public abstract class isSplitted : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsSplitted;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Split into Components")]
		public virtual Boolean? IsSplitted
		{
			get
			{
				return this._IsSplitted;
			}
			set
			{
				this._IsSplitted = value;
			}
		}
		#endregion
		#region UseParentSubID
		public abstract class useParentSubID : PX.Data.IBqlField
		{
		}
		protected Boolean? _UseParentSubID;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Component Subaccounts", FieldClass = SubAccountAttribute.DimensionName)]
		public virtual Boolean? UseParentSubID
		{
			get
			{
				return this._UseParentSubID;
			}
			set
			{
				this._UseParentSubID = value;
			}
		}
		#endregion
		#region TotalPercentage
		public abstract class totalPercentage : PX.Data.IBqlField
		{
		}
		protected Decimal? _TotalPercentage;
		[PXDecimal()]
		[PXUIField(DisplayName = "Total Percentage", Enabled = false)]
		public virtual Decimal? TotalPercentage
		{
			get
			{
				return this._TotalPercentage;
			}
			set
			{
				this._TotalPercentage = value;
			}
		}
		#endregion
		#region KitItem
		public abstract class kitItem : PX.Data.IBqlField
		{
		}
		protected Boolean? _KitItem;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is a Kit", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? KitItem
		{
			get
			{
				return this._KitItem;
			}
			set
			{
				this._KitItem = value;
			}
		}
		#endregion
		#region MinGrossProfitPct
		public abstract class minGrossProfitPct : PX.Data.IBqlField
		{
		}
		protected Decimal? _MinGrossProfitPct;
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<INItemClass.minGrossProfitPct, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>))]
		[PXDBDecimal(6, MinValue = 0, MaxValue = 100)]
		[PXUIField(DisplayName = "Min. Markup %", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? MinGrossProfitPct
		{
			get
			{
				return this._MinGrossProfitPct;
			}
			set
			{
				this._MinGrossProfitPct = value;
			}
		}
		#endregion
		#region NonStockReceipt
		public abstract class nonStockReceipt : PX.Data.IBqlField
		{
		}
		protected Boolean? _NonStockReceipt;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? NonStockReceipt
		{
			get
			{
				return this._NonStockReceipt;
			}
			set
			{
				this._NonStockReceipt = value;
			}
		}
		#endregion
		#region NonStockShip
		public abstract class nonStockShip : PX.Data.IBqlField
		{
		}
		protected Boolean? _NonStockShip;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? NonStockShip
		{
			get
			{
				return this._NonStockShip;
			}
			set
			{
				this._NonStockShip = value;
			}
		}
		#endregion
		
		#region ABCCodeID
		public abstract class aBCCodeID : PX.Data.IBqlField
		{
		}
		protected String _ABCCodeID;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "ABC Code")]
		[PXSelector(typeof(INABCCode.aBCCodeID), DescriptionField = typeof(INABCCode.descr))]
		public virtual String ABCCodeID
		{
			get
			{
				return this._ABCCodeID;
			}
			set
			{
				this._ABCCodeID = value;
			}
		}
		#endregion
		#region ABCCodeIsFixed
		public abstract class aBCCodeIsFixed : PX.Data.IBqlField
		{
		}
		protected Boolean? _ABCCodeIsFixed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Fixed ABC Code")]
		public virtual Boolean? ABCCodeIsFixed
		{
			get
			{
				return this._ABCCodeIsFixed;
			}
			set
			{
				this._ABCCodeIsFixed = value;
			}
		}
		#endregion
		#region MovementClassID
		public abstract class movementClassID : PX.Data.IBqlField
		{
		}
		protected String _MovementClassID;
		[PXDBString(1)]
		[PXUIField(DisplayName = "Movement Class")]
		[PXSelector(typeof(INMovementClass.movementClassID), DescriptionField = typeof(INMovementClass.descr))]
		public virtual String MovementClassID
		{
			get
			{
				return this._MovementClassID;
			}
			set
			{
				this._MovementClassID = value;
			}
		}
		#endregion
		#region MovementClassIsFixed
		public abstract class movementClassIsFixed : PX.Data.IBqlField
		{
		}
		protected Boolean? _MovementClassIsFixed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Fixed Movement Class")]
		public virtual Boolean? MovementClassIsFixed
		{
			get
			{
				return this._MovementClassIsFixed;
			}
			set
			{
				this._MovementClassIsFixed = value;
			}
		}
		#endregion

		#region MarkupPct
		public abstract class markupPct : PX.Data.IBqlField
		{
		}
		protected Decimal? _MarkupPct;
		[PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<INItemClass.markupPct, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>))]
		[PXUIField(DisplayName = "Markup %")]
		public virtual Decimal? MarkupPct
		{
			get
			{
				return this._MarkupPct;
			}
			set
			{
				this._MarkupPct = value;
			}
		}
		#endregion
		#region RecPrice
		public abstract class recPrice : PX.Data.IBqlField
		{
		}
		protected Decimal? _RecPrice;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "MSRP")]
		public virtual Decimal? RecPrice
		{
			get
			{
				return this._RecPrice;
			}
			set
			{
				this._RecPrice = value;
			}
		}
		#endregion
		#region ImageUrl
		public abstract class imageUrl : PX.Data.IBqlField
		{
		}
		protected String _ImageUrl;
		[PXDBString(255)]
		public virtual String ImageUrl
		{
			get
			{
				return this._ImageUrl;
			}
			set
			{
				this._ImageUrl = value;
			}
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.IBqlField
		{
		}
		protected Int64? _NoteID;
		[PXNote(typeof(InventoryItem.descr),
			typeof(InventoryItem.productWorkgroupID),
			typeof(InventoryItem.itemStatus),
			DescriptionField = typeof(InventoryItem.inventoryCD),
			Selector = typeof(InventoryItem.inventoryCD))]
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
		#region GroupMask
		public abstract class groupMask : IBqlField
		{
		}
		protected Byte[] _GroupMask;
		[PXDBGroupMask()]
		public virtual Byte[] GroupMask
		{
			get
			{
				return this._GroupMask;
			}
			set
			{
				this._GroupMask = value;
			}
		}
		#endregion
		#region CycleID
		public abstract class cycleID : PX.Data.IBqlField
		{
		}
		protected String _CycleID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "PI Cycle")]
		[PXSelector(typeof(INPICycle.cycleID), DescriptionField = typeof(INPICycle.descr))]
		public virtual String CycleID
		{
			get
			{
				return this._CycleID;
			}
			set
			{
				this._CycleID = value;
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

		#region Attributes
		public abstract class attributes : IBqlField
		{
		}
		protected string[] _Attributes;
		[CRAttributesField(typeof(CSAnswerType.inventoryAnswerType), typeof(InventoryItem.inventoryID), typeof(InventoryItem.itemClassID))]
		public virtual string[] Attributes { get; set; }

		public virtual int? ID
			{
			get { return InventoryID; }
			}
		public virtual string EntityType
			{
			get { return CSAnswerType.InventoryItem; }
			}
		public virtual string ClassID
		{
			get { return ItemClassID; }
		}

		#endregion
		#region Included
		public abstract class included : PX.Data.IBqlField
		{
		}
		protected bool? _Included;
		[PXBool]
		[PXUIField(DisplayName = "Included")]
		[PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? Included
		{
			get
			{
				return this._Included;
			}
			set
			{
				this._Included = value;
			}
		}
		#endregion


        #region IsRUTROTDeductible
        public abstract class isRUTROTDeductible : IBqlField { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "ROT or RUT Deductible Item", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual bool? IsRUTROTDeductible { get; set; }
        #endregion

	}

	public class InventoryItemStatus
	{
		public const string Active = "AC";
		public const string NoSales = "NS";
		public const string NoPurchases = "NP";
		public const string NoRequest = "NR";
		public const string Inactive = "IN";
		public const string MarkedForDeletion = "DE";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Active, NoSales, NoPurchases, NoRequest, Inactive, MarkedForDeletion },
				new string[] { Messages.Active, Messages.NoSales, Messages.NoPurchases, Messages.NoRequest, Messages.Inactive, Messages.ToDelete }
				)
			{ }
		}

		public class SubItemListAttribute : PXStringListAttribute
		{
			public SubItemListAttribute()
				: base(
				new string[] { Active, NoSales, NoPurchases, NoRequest, Inactive},
				new string[] { Messages.Active, Messages.NoSales, Messages.NoPurchases, Messages.NoRequest, Messages.Inactive }
				)
			{ }
		}

		public class active : Constant<string> { public active() : base(Active) { } }
		public class noSales : Constant<string> { public noSales() : base(NoSales) { } }
		public class noPurchases : Constant<string> { public noPurchases() : base(NoPurchases) { } }
		public class noRequest : Constant<string> { public noRequest() : base(NoRequest) { } }
		public class inactive : Constant<string> { public inactive() : base(Inactive) { } }
		public class markedForDeletion : Constant<string> { public markedForDeletion() : base(MarkedForDeletion) { } }
	}


	#region Attributes
	public class INItemTypes
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

		public class StockListAttribute : CustomListAttribute
		{
			public StockListAttribute()
				: base(
					new string[] { FinishedGood, Component, SubAssembly },
					new string[] { Messages.FinishedGood, Messages.Component, Messages.SubAssembly }) { ; }
		}

		public class NonStockListAttribute : CustomListAttribute
		{
			public NonStockListAttribute()
				: base(
					new string[] { NonStockItem, LaborItem, ServiceItem, ChargeItem, ExpenseItem },
					new string[] { Messages.NonStockItem, Messages.LaborItem, Messages.ServiceItem, Messages.ChargeItem, Messages.ExpenseItem }) { ; }
		}

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
					new string[] { FinishedGood, Component, SubAssembly, NonStockItem, LaborItem, ServiceItem, ChargeItem, ExpenseItem },
					new string[] { Messages.FinishedGood, Messages.Component, Messages.SubAssembly, Messages.NonStockItem, Messages.LaborItem, Messages.ServiceItem, Messages.ChargeItem, Messages.ExpenseItem }) { ; }
		}

		public const string NonStockItem = "N";
		public const string LaborItem = "L";
		public const string ServiceItem = "S";
		public const string ChargeItem = "C";
		public const string ExpenseItem = "E";

		public const string FinishedGood = "F";
		public const string Component = "M";
		public const string SubAssembly = "A";


		public class nonStockItem : Constant<string>
		{
			public nonStockItem() : base(NonStockItem) { ;}
		}

		public class laborItem : Constant<string>
		{
			public laborItem() : base(LaborItem) { ;}
		}

		public class serviceItem : Constant<string>
		{
			public serviceItem() : base(ServiceItem) { ;}
		}

		public class chargeItem : Constant<string>
		{
			public chargeItem() : base(ChargeItem) { ;}
		}

		public class expenseItem : Constant<string>
		{
			public expenseItem() : base(ExpenseItem) { ;}
		}

		public class finishedGood : Constant<string>
		{
			public finishedGood() : base(FinishedGood) { ;}
		}

		public class component : Constant<string>
		{
			public component() : base(Component) { ;}
		}

		public class subAssembly : Constant<string>
		{
			public subAssembly() : base(SubAssembly) { ;}
		}


	}

	public class INValMethod
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
					new string[] { Standard, Average, FIFO, Specific },
					new string[] { Messages.Standard, Messages.Average, Messages.FIFO, Messages.Specific }) { ; }
		}

		public const string Standard = "T";
		public const string Average = "A";
		public const string FIFO = "F";
		public const string Specific = "S";

		public class standard : Constant<string>
		{
			public standard() : base(Standard) { ;}
		}

		public class average : Constant<string>
		{
			public average() : base(Average) { ;}
		}

		public class fIFO : Constant<string>
		{
			public fIFO() : base(FIFO) { ;}
		}

		public class specific : Constant<string>
		{
			public specific() : base(Specific) { ;}
		}
	}

	public class INItemStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(new string[] { Active, NoSales, NoPurchases, Inactive, ToDelete }, new string[] { Messages.Active, Messages.NoSales, Messages.NoPurchases, Messages.Inactive, Messages.ToDelete })
			{
			}
		}

		public const string Active = "AC";
		public const string Inactive = "IN";
		public const string NoSales = "NS";
		public const string NoPurchases = "NP";
		public const string ToDelete = "DE";

		public class active : Constant<string>
		{
			public active() : base(Active) { ;}
		}

		public class inactive : Constant<string>
		{
			public inactive() : base(Inactive) { ; }
		}

		public class noSales : Constant<string>
		{
			public noSales() : base(NoSales) { ; }
		}

		public class noPurchases : Constant<string>
		{
			public noPurchases() : base(NoPurchases) { ; }
		}

		public class toDelete : Constant<string>
		{
			public toDelete() : base(ToDelete) { ; }
		}
	}

	public class INPackageOption
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
					new string[] { Manual, Weight, Quantity, WeightAndVolume },
					new string[] { Messages.Manual, Messages.Weight, Messages.Quantity, Messages.WeightAndVolume }) { ; }
		}
		public const string Manual = "N";
		public const string Weight = "W";
		public const string Quantity = "Q";
		public const string WeightAndVolume = "V";

		public class weight : Constant<string>
		{
			public weight() : base(Weight) { ; }
		}
	}

	#endregion


}
