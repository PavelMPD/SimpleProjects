using System;
using System.Collections.Generic;
using PX.Data;
using PX.TM;
using PX.Objects.GL;
using PX.Objects.AP;
using PX.Objects.RQ;
using PX.Objects.EP.Standalone;
using PX.Objects.CS;
using PX.Objects.CR;
using System.Collections;
using PX.Objects.PO;

namespace PX.Objects.IN
{
	[System.SerializableAttribute]
	public partial class INReplenishmentFilter : IBqlTable
	{
		#region CurrentOwnerID
		public abstract class currentOwnerID : PX.Data.IBqlField
		{
		}

		[PXDBGuid]
		[CRCurrentOwnerID]
		public virtual Guid? CurrentOwnerID { get; set; }
		#endregion						
		#region MyOwner
		public abstract class myOwner : PX.Data.IBqlField
		{
		}
		protected Boolean? _MyOwner;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Me")]
		public virtual Boolean? MyOwner
		{
			get
			{
				return _MyOwner;
			}
			set
			{
				_MyOwner = value;
			}
		}
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.IBqlField
		{
		}
		protected Guid? _OwnerID;
		[PXDBGuid]
		[PXUIField(DisplayName = "Product Manager")]
		[PX.TM.PXSubordinateOwnerSelector]
		public virtual Guid? OwnerID
		{
			get
			{
				return (_MyOwner == true) ? CurrentOwnerID : _OwnerID;
			}
			set
			{
				_OwnerID = value;
			}
		}
		#endregion
		#region WorkGroupID
		public abstract class workGroupID : PX.Data.IBqlField
		{
		}
		protected Int32? _WorkGroupID;
		[PXDBInt]
		[PXUIField(DisplayName = "Product  Workgroup")]
		[PXSelector(typeof(Search<EPCompanyTree.workGroupID,
			Where<EPCompanyTree.workGroupID, Owned<Current<AccessInfo.userID>>>>),
		 SubstituteKey = typeof(EPCompanyTree.description))]
		public virtual Int32? WorkGroupID
		{
			get
			{
				return (_MyWorkGroup == true) ? null : _WorkGroupID;
			}
			set
			{
				_WorkGroupID = value;
			}
		}
		#endregion
		#region MyWorkGroup
		public abstract class myWorkGroup : PX.Data.IBqlField
		{
		}
		protected Boolean? _MyWorkGroup;
		[PXDefault(false)]
		[PXDBBool]
		[PXUIField(DisplayName = "My", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? MyWorkGroup
		{
			get
			{
				return _MyWorkGroup;
			}
			set
			{
				_MyWorkGroup = value;
			}
		}
		#endregion
		#region FilterSet
		public abstract class filterSet : PX.Data.IBqlField
		{
		}
		[PXDefault(false)]
		[PXDBBool]
        public virtual Boolean? FilterSet
		{
			get
			{
				return
					this.OwnerID != null ||
					this.WorkGroupID != null ||
					this.MyWorkGroup == true;
			}
		}
		#endregion
		#region ReplenishmentSiteID
		public abstract class replenishmentSiteID : PX.Data.IBqlField
		{
		}
		protected Int32? _ReplenishmentSiteID;
		[IN.Site(DisplayName = "Warehouse")]
		[PXDefault]
		public virtual Int32? ReplenishmentSiteID
		{
			get
			{
				return this._ReplenishmentSiteID;
			}
			set
			{
				this._ReplenishmentSiteID = value;
			}
		}
		#endregion
		#region PurchaseDate
		public abstract class purchaseDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _PurchaseDate;
		[PXDBDate]
		[PXUIField(DisplayName = "Purchase Date")]
		[PXDefault(typeof(AccessInfo.businessDate))]		
		public virtual DateTime? PurchaseDate
		{
			get
			{
				return this._PurchaseDate;
			}
			set
			{
				this._PurchaseDate = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[StockItem(Filterable = true)]
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
		#region SubItemCD
		public abstract class subItemCD : PX.Data.IBqlField
		{
		}
		protected String _SubItemCD;
		[SubItemRawExt(typeof(INReplenishmentFilter.inventoryID), DisplayName = "Subitem")]
		//[SubItemRaw(DisplayName = "Subitem")]
		public virtual String SubItemCD
		{
			get
			{
				return this._SubItemCD;
			}
			set
			{
				this._SubItemCD = value;
			}
		}
		#endregion
		#region SubItemCD Wildcard
		public abstract class subItemCDWildcard : PX.Data.IBqlField { };
		[PXDBString(30, IsUnicode = true)]
		public virtual String SubItemCDWildcard
		{
			get
			{
				//return SubItemCDUtils.CreateSubItemCDWildcard(this._SubItemCD);
				return SubCDUtils.CreateSubCDWildcard(this._SubItemCD, SubItemAttribute.DimensionName);
			}
		}
		#endregion
		#region PreferredVendorID
		public abstract class preferredVendorID : PX.Data.IBqlField
		{
		}
		protected Int32? _PreferredVendorID;
		[VendorNonEmployeeActive(Visibility = PXUIVisibility.SelectorVisible,
			DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true, DisplayName = "Vendor")]
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
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected String _Descr;
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region DescrWildcard
		public abstract class descrWildcard : PX.Data.IBqlField
		{
		}
		[PXDBString(255, IsUnicode = true)]
		public virtual String DescrWildcard
		{
			get
			{
				return this._Descr == null ? null : "%" + this._Descr + "%";
			}
		}
		#endregion
		#region OnlySuggested
		public abstract class onlySuggested : PX.Data.IBqlField
		{
		}
		protected Boolean? _OnlySuggested;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Only Suggested Items")]
		public virtual Boolean? OnlySuggested
		{
			get
			{
				return _OnlySuggested;
			}
			set
			{
				_OnlySuggested = value;
			}
		}
		#endregion
	}

	public class INReplenishmentProjection : TM.OwnedFilter.ProjectionAttribute
	{
		public INReplenishmentProjection()
			: base(typeof(INReplenishmentFilter),
			BqlCommand.Compose(
		typeof(Select2<,,>),
		typeof(IN.S.INItemSite),
		typeof(CrossJoin<FeaturesSet,
					 InnerJoin<InventoryItem,
									On<InventoryItem.inventoryID, Equal<IN.S.INItemSite.inventoryID>,
								  And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
									And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>>,						     					 	
					 LeftJoin<Vendor,
									On<Vendor.bAccountID, Equal<IN.S.INItemSite.preferredVendorID>>,			     
					 LeftJoin<INItemSiteReplenishment,
							  On<INItemSiteReplenishment.inventoryID, Equal<INItemSite.inventoryID>,
			         And<INItemSiteReplenishment.siteID, Equal<INItemSite.siteID>,							 
							 And<INItemSiteReplenishment.itemStatus, NotEqual<INItemStatus.inactive>,							 
							 And<FeaturesSet.subItem, Equal<boolTrue>>>>>,
			     InnerJoin<INSubItem,
			            On<INSubItem.subItemID, Equal<INItemSiteReplenishment.subItemID>,
									Or<Where<FeaturesSet.subItem, Equal<boolFalse>,
										    And<INSubItem.subItemCD, Equal<INSubItem.Zero>>>>>, 
			     LeftJoin<INSiteStatus,
									On<INSiteStatus.inventoryID, Equal<IN.S.INItemSite.inventoryID>,
								 And<INSiteStatus.siteID, Equal<IN.S.INItemSite.siteID>,
			           And<INSiteStatus.subItemID, Equal<INSubItem.subItemID>>>>,
					 InnerJoin<INItemClass,
								  On<INItemClass.itemClassID, Equal<InventoryItem.itemClassID>>>>>>>>>),
		TM.OwnedFilter.ProjectionAttribute.ComposeWhere(
		typeof(INReplenishmentFilter),
		typeof(InventoryItem.productWorkgroupID),
		typeof(InventoryItem.productManagerID))))
		{
		}
	}

	[Serializable]
	[INReplenishmentProjection]
	public partial class INReplenishmentItem : IN.S.INItemSite
	{		
		#region SubItemID
		public abstract class subItemID : PX.Data.IBqlField
		{
		}
		protected Int32? _SubItemID;
		[SubItem(IsKey = true, BqlField=typeof(INSubItem.subItemID))]
		[PXDefault]
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
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected String _Descr;
		[PXDBString(255, IsUnicode = true, BqlField = typeof(InventoryItem.descr))]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region ReplenishmentSourceSiteID
		public new abstract class replenishmentSourceSiteID : PX.Data.IBqlField
		{
		}
		[IN.Site(DisplayName = "Source Warehouse", DescriptionField = typeof(INSite.descr))]
		public override Int32? ReplenishmentSourceSiteID
		{
			get
			{
				return this._ReplenishmentSourceSiteID;
			}
			set
			{
				this._ReplenishmentSourceSiteID = value;
			}
		}
		#endregion
		#region PreferredVendorID
		public new abstract class preferredVendorID : PX.Data.IBqlField
		{
		}		
		[AP.VendorActive(DisplayName = "Preferred Vendor ID", Required = false, DescriptionField = typeof(Vendor.acctName), BqlField=typeof(INItemSite.preferredVendorID))]
		public override Int32? PreferredVendorID
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
		public new abstract class preferredVendorLocationID : PX.Data.IBqlField
		{
		}		
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<INReplenishmentItem.preferredVendorID>>>),
			DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Preferred Location", BqlField=typeof(INItemSite.preferredVendorLocationID))]		
		[PXDefault(typeof(Search<Vendor.defLocationID, Where<Vendor.bAccountID, Equal<Current<INReplenishmentItem.preferredVendorID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<INReplenishmentItem.preferredVendorID>))]
		public override Int32? PreferredVendorLocationID
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
		#region PreferredVendorName
		public abstract class preferredVendorName : PX.Data.IBqlField
		{
		}
		protected string _PreferredVendorName;
		[PXDBString(60, IsUnicode = true, BqlField = typeof(Vendor.acctName))]
		[PXDefault(typeof(Search<BAccountR.acctName, Where<BAccountR.bAccountID, Equal<Current<INReplenishmentItem.preferredVendorID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Preferred Vendor Name", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(Default<INReplenishmentItem.preferredVendorID>))]
		public virtual String PreferredVendorName
		{
			get
			{
				return this._PreferredVendorName;
			}
			set
			{
				this._PreferredVendorName = value;
			}
		}
		#endregion
		#region DefaultVendorLocationID
		public abstract class defaultVendorLocationID : PX.Data.IBqlField
		{
		}
		protected int? _DefaultVendorLocationID;
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<INReplenishmentItem.preferredVendorID>>>),
			DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Default Location", BqlField = typeof(Vendor.defLocationID))]
		[PXDefault(typeof(Search<Vendor.defLocationID, Where<Vendor.bAccountID, Equal<Current<INReplenishmentItem.preferredVendorID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<INReplenishmentItem.preferredVendorID>))]
		public virtual Int32? DefaultVendorLocationID
		{
			get
			{
				return this._DefaultVendorLocationID;
			}
			set
			{
				this._DefaultVendorLocationID = value;
			}
		}
		#endregion
		#region ItemClassID
		public abstract class itemClassID : PX.Data.IBqlField
		{
		}
		protected String _ItemClassID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(InventoryItem.itemClassID))]
		[PXUIField(DisplayName = "Item Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<INItemClass.itemClassID, Where<INItemClass.stkItem, Equal<boolTrue>>>), DescriptionField = typeof(INItemClass.descr))]
		[PXDefault(typeof(Search<INSetup.dfltItemClassID>))]
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
		#region VendorClassID
		public abstract class vendorClassID : PX.Data.IBqlField
		{
		}
		protected String _VendorClassID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(Vendor.vendorClassID))]
		[PXDefault(typeof(Search<Vendor.vendorClassID, Where<Vendor.bAccountID, Equal<Current<INReplenishmentItem.preferredVendorID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search2<VendorClass.vendorClassID, LeftJoin<EPEmployeeClass, On<EPEmployeeClass.vendorClassID, Equal<VendorClass.vendorClassID>>>, Where<EPEmployeeClass.vendorClassID, IsNull>>), DescriptionField = typeof(VendorClass.descr), CacheGlobal = true)]
		[PXFormula(typeof(Default<INReplenishmentItem.preferredVendorID>))]
		[PXUIField(DisplayName = "Vendor Class")]
		public virtual String VendorClassID
		{
			get
			{
				return this._VendorClassID;
			}
			set
			{
				this._VendorClassID = value;
			}
		}
		#endregion
		#region SafetyStock
		public new abstract class safetyStock : PX.Data.IBqlField
		{
		}		
		//[PXDBQuantity(BqlField=typeof(INItemSite.safetyStock))]
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<INItemSiteReplenishment.safetyStock, IsNotNull>, INItemSiteReplenishment.safetyStock>, INItemSite.safetyStock>), typeof(decimal))]
		[PXUIField(DisplayName = "Safety Stock")]
		public override Decimal? SafetyStock
		{
			get
			{
				return this._SafetyStock;
			}
			set
			{
				this._SafetyStock = value;
			}
		}
		#endregion
		#region MinQty
		public new abstract class minQty : PX.Data.IBqlField
		{
		}
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<INItemSiteReplenishment.minQty, IsNotNull>, INItemSiteReplenishment.minQty>, INItemSite.minQty>), typeof(decimal))]
		[PXUIField(DisplayName = "Reorder Point")]
		public override Decimal? MinQty
		{
			get
			{
				return this._MinQty;
			}
			set
			{
				this._MinQty = value;
			}
		}
		#endregion
		#region MaxQty
		public new abstract class maxQty : PX.Data.IBqlField
		{
		}
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<INItemSiteReplenishment.maxQty, IsNotNull>, INItemSiteReplenishment.maxQty>, INItemSite.maxQty>), typeof(decimal))]
		[PXUIField(DisplayName = "Max. Qty.")]
		public override Decimal? MaxQty
		{
			get
			{
				return this._MaxQty;
			}
			set
			{
				this._MaxQty = value;
			}
		}
		#endregion

		#region TransferERQ
		public new abstract class transferERQ : PX.Data.IBqlField
		{
		}
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<INItemSiteReplenishment.transferERQ, IsNotNull>, INItemSiteReplenishment.transferERQ>, INItemSite.transferERQ>), typeof(decimal))]
		[PXUIField(DisplayName = "Transfer ERQ")]
		public override Decimal? TransferERQ
		{
			get
			{
				return this._TransferERQ;
			}
			set
			{
				this._TransferERQ = value;
			}
		}
		#endregion
		
		#region LaunchDate
		public new abstract class launchDate : PX.Data.IBqlField
		{
		}
		
		[PXDBDate(BqlField=typeof(INItemSite.launchDate))]
		[PXUIField(DisplayName = "Launch Date")]
		public override DateTime? LaunchDate
		{
			get
			{
				return this._LaunchDate;
			}
			set
			{
				this._LaunchDate = value;
			}
		}
		#endregion		
		#region TerminationDate
		public new abstract class terminationDate : PX.Data.IBqlField
		{
		}
		[PXDBDate(BqlField=typeof(INItemSite.terminationDate))]
		[PXUIField(DisplayName = "Termination Date")]
		public override DateTime? TerminationDate
		{
			get
			{
				return this._TerminationDate;
			}
			set
			{
				this._TerminationDate = value;
			}
		}
		#endregion
		#region ReplenishmentPolicyID
		public new abstract class replenishmentPolicyID : PX.Data.IBqlField
		{
		}		
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa", BqlField=typeof(INItemSite.replenishmentPolicyID))]
		[PXUIField(DisplayName = "Replenishment Policy ID")]
		[PXSelector(typeof(Search<INReplenishmentPolicy.replenishmentPolicyID>), DescriptionField = typeof(INReplenishmentPolicy.descr))]
		public override String ReplenishmentPolicyID
		{
			get
			{
				return this._ReplenishmentPolicyID;
			}
			set
			{
				this._ReplenishmentPolicyID = value;
			}
		}
		#endregion
		#region ReplenishmentSource
		public new abstract class replenishmentSource : PX.Data.IBqlField
		{
		}
		
		[PXDBString(1, IsFixed = true, BqlField=(typeof(INItemSite.replenishmentSource)))]
		[PXUIField(DisplayName = "Replenishment Source")]		
		[INReplenishmentSource.INPlanList]
		[PXDefault(INReplenishmentSource.Purchased, PersistingCheck = PXPersistingCheck.Nothing)]
		public override string ReplenishmentSource
		{
			get
			{
				return this._ReplenishmentSource;
			}
			set
			{
				this._ReplenishmentSource = value;
			}
		}
		#endregion
		#region InclQtySOBackOrdered
		public abstract class inclQtySOBackOrdered : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtySOBackOrdered;
		[PXDBBool(BqlField = typeof(INItemClass.inclQtySOBackOrdered))]
		[PXUIField(DisplayName = "Deduct Qty. On Back Orders")]
		public virtual Boolean? InclQtySOBackOrdered
		{
			get
			{
				return this._InclQtySOBackOrdered;
			}
			set
			{
				this._InclQtySOBackOrdered = value;
			}
		}
		#endregion
		#region InclQtySOBooked
		public abstract class inclQtySOBooked : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtySOBooked;
		[PXDBBool(BqlField = typeof(INItemClass.inclQtySOBooked))]
		[PXUIField(DisplayName = "Deduct Qty. on Customer Orders")]
		public virtual Boolean? InclQtySOBooked
		{
			get
			{
				return this._InclQtySOBooked;
			}
			set
			{
				this._InclQtySOBooked = value;
			}
		}
		#endregion
		#region InclQtySOShipped
		public abstract class inclQtySOShipped : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtySOShipped;
		[PXDBBool(BqlField = typeof(INItemClass.inclQtySOShipped))]
		[PXUIField(DisplayName = "Deduct Qty. Shipped")]
		public virtual Boolean? InclQtySOShipped
		{
			get
			{
				return this._InclQtySOShipped;
			}
			set
			{
				this._InclQtySOShipped = value;
			}
		}
		#endregion
		#region InclQtySOShipping
		public abstract class inclQtySOShipping : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtySOShipping;
		[PXDBBool(BqlField = typeof(INItemClass.inclQtySOShipping))]
		[PXUIField(DisplayName = "Deduct Qty. Shipping")]
		public virtual Boolean? InclQtySOShipping
		{
			get
			{
				return this._InclQtySOShipping;
			}
			set
			{
				this._InclQtySOShipping = value;
			}
		}
		#endregion
		#region InclQtyINIssues
		public abstract class inclQtyINIssues : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyINIssues;
		[PXDBBool(BqlField = typeof(INItemClass.inclQtyINIssues))]
		[PXUIField(DisplayName = "Deduct Qty. On Issues")]
		public virtual Boolean? InclQtyINIssues
		{
			get
			{
				return this._InclQtyINIssues;
			}
			set
			{
				this._InclQtyINIssues = value;
			}
		}
		#endregion		
		#region InclQtyINAssemblyDemand
		public abstract class inclQtyINAssemblyDemand : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyINAssemblyDemand;
		[PXDBBool(BqlField = typeof(INItemClass.inclQtyINAssemblyDemand))]
		[PXUIField(DisplayName = "Deduct Qty. of Kit Assembly Demand")]
		public virtual Boolean? InclQtyINAssemblyDemand
		{
			get
			{
				return this._InclQtyINAssemblyDemand;
			}
			set
			{
				this._InclQtyINAssemblyDemand = value;
			}
		}
		#endregion
		#region QtyProcess
		public abstract class qtyProcess : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyProcess;
		[PXQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. To Process")]
		public virtual Decimal? QtyProcess
		{
			get
			{
				return this._QtyProcess;
			}
			set
			{
				this._QtyProcess = value;
			}
		}
		#endregion
		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyOnHand;
		[PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<INSiteStatus.qtyOnHand, IsNotNull>, INSiteStatus.qtyOnHand>, decimal0>), typeof(decimal))]
		[PXDefault(TypeCode.Decimal, "0.0")]		
		[PXUIField(DisplayName="Qty. On Hand")]
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
		#region QtyPOPrepared
		public abstract class qtyPOPrepared : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyPOPrepared;
		[PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<INSiteStatus.qtyPOPrepared, IsNotNull>, INSiteStatus.qtyPOPrepared>, decimal0>), typeof(decimal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. PO Prepared", Visibility = PXUIVisibility.Visible, Visible =  false)]
		public virtual Decimal? QtyPOPrepared
		{
			get
			{
				return this._QtyPOPrepared;
			}
			set
			{
				this._QtyPOPrepared = value;
			}
		}
		#endregion
		#region QtyPOOrders
		public abstract class qtyPOOrders : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyPOOrders;
		[PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<INSiteStatus.qtyPOOrders, IsNotNull>, INSiteStatus.qtyPOOrders>, decimal0>), typeof(decimal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. PO Orders", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Decimal? QtyPOOrders
		{
			get
			{
				return this._QtyPOOrders;
			}
			set
			{
				this._QtyPOOrders = value;
			}
		}
		#endregion
		#region QtyPOReceipts
		public abstract class qtyPOReceipts : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyPOReceipts;
		[PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<INSiteStatus.qtyPOReceipts, IsNotNull>, INSiteStatus.qtyPOReceipts>, decimal0>), typeof(decimal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. PO Receipts", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Decimal? QtyPOReceipts
		{
			get
			{
				return this._QtyPOReceipts;
			}
			set
			{
				this._QtyPOReceipts = value;
			}
		}
		#endregion
		#region QtyInTransit
		public abstract class qtyInTransit : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyInTransit;
		[PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<INSiteStatus.qtyInTransit, IsNotNull>, INSiteStatus.qtyInTransit>, decimal0>), typeof(decimal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. IN Transit", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Decimal? QtyInTransit
		{
			get
			{
				return this._QtyInTransit;
			}
			set
			{
				this._QtyInTransit = value;
			}
		}
		#endregion
		#region QtyINReceipts
		public abstract class qtyINReceipts : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyINReceipts;
		[PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<INSiteStatus.qtyINReceipts, IsNotNull>, INSiteStatus.qtyINReceipts>, decimal0>), typeof(decimal))]
		[PXDefault(TypeCode.Decimal, "0.0")]		
		[PXUIField(DisplayName = "Qty. IN Receipts", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Decimal? QtyINReceipts
		{
			get
			{
				return this._QtyINReceipts;
			}
			set
			{
				this._QtyINReceipts = value;
			}
		}
		#endregion		
		#region QtyINAssemblySupply
		public abstract class qtyINAssemblySupply : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyINAssemblySupply;
		[PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<INSiteStatus.qtyINAssemblySupply, IsNotNull>, INSiteStatus.qtyINAssemblySupply>, decimal0>), typeof(decimal))]
		[PXDefault(TypeCode.Decimal, "0.0")]		
		[PXUIField(DisplayName = "Qty. IN Assembly Supply", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Decimal? QtyINAssemblySupply
		{
			get
			{
				return this._QtyINAssemblySupply;
			}
			set
			{
				this._QtyINAssemblySupply = value;
			}
		}
		#endregion
		#region QtySOBackOrdered
		public abstract class qtySOBackOrdered : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtySOBackOrdered;
		[PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<INSiteStatus.qtySOBackOrdered, IsNotNull>, INSiteStatus.qtySOBackOrdered>, decimal0>), typeof(decimal))]
		[PXDefault(TypeCode.Decimal, "0.0")]		
		[PXUIField(DisplayName = "Qty. SO Back Ordered", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Decimal? QtySOBackOrdered
		{
			get
			{
				return this._QtySOBackOrdered;
			}
			set
			{
				this._QtySOBackOrdered = value;
			}
		}
		#endregion
		#region QtySOBooked
		public abstract class qtySOBooked : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtySOBooked;
		[PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<INSiteStatus.qtySOBooked, IsNotNull>, INSiteStatus.qtySOBooked>, decimal0>), typeof(decimal))]
		[PXDefault(TypeCode.Decimal, "0.0")]		
		[PXUIField(DisplayName = "Qty. SO Booked", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Decimal? QtySOBooked
		{
			get
			{
				return this._QtySOBooked;
			}
			set
			{
				this._QtySOBooked = value;
			}
		}
		#endregion
		#region QtySOShipped
		public abstract class qtySOShipped : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtySOShipped;
		[PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<INSiteStatus.qtySOShipped, IsNotNull>, INSiteStatus.qtySOShipped>, decimal0>), typeof(decimal))]
		[PXDefault(TypeCode.Decimal, "0.0")]		
		[PXUIField(DisplayName = "Qty. SO Shipped", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Decimal? QtySOShipped
		{
			get
			{
				return this._QtySOShipped;
			}
			set
			{
				this._QtySOShipped = value;
			}
		}
		#endregion
		#region QtySOShipping
		public abstract class qtySOShipping : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtySOShipping;
		[PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<INSiteStatus.qtySOShipping, IsNotNull>, INSiteStatus.qtySOShipping>, decimal0>), typeof(decimal))]
		[PXDefault(TypeCode.Decimal, "0.0")]	
		[PXUIField(DisplayName = "Qty. SO Shipping", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Decimal? QtySOShipping
		{
			get
			{
				return this._QtySOShipping;
			}
			set
			{
				this._QtySOShipping = value;
			}
		}
		#endregion
		#region QtyINIssues
		public abstract class qtyINIssues : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyINIssues;
		[PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<INSiteStatus.qtyINIssues, IsNotNull>, INSiteStatus.qtyINIssues>, decimal0>), typeof(decimal))]
		[PXDefault(TypeCode.Decimal, "0.0")]	
		[PXUIField(DisplayName = "Qty. IN Issues", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Decimal? QtyINIssues
		{
			get
			{
				return this._QtyINIssues;
			}
			set
			{
				this._QtyINIssues = value;
			}
		}
		#endregion
		#region QtyINAssemblyDemand
		public abstract class qtyINAssemblyDemand : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyINAssemblyDemand;
		[PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<INSiteStatus.qtyINAssemblyDemand, IsNotNull>, INSiteStatus.qtyINAssemblyDemand>, decimal0>), typeof(decimal))]
		[PXDefault(TypeCode.Decimal, "0.0")]	
		[PXUIField(DisplayName = "Qty. IN Assembly Demand", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Decimal? QtyINAssemblyDemand
		{
			get
			{
				return this._QtyINAssemblyDemand;
			}
			set
			{
				this._QtyINAssemblyDemand = value;
			}
		}
		#endregion		
		#region QtyINReplaned
		public abstract class qtyINReplaned : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyINReplaned;
		[PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<INSiteStatus.qtyINReplaned, IsNotNull>, INSiteStatus.qtyINReplaned>, decimal0>), typeof(decimal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Replaned")]
		public virtual Decimal? QtyINReplaned
		{
			get
			{
				return this._QtyINReplaned;
			}
			set
			{
				this._QtyINReplaned = value;
			}
		}
		#endregion		
		#region QtyReplenishment
		public abstract class qtyReplenishment : PX.Data.IBqlField
		{
		}
		
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. on Supply")]
		public virtual Decimal? QtyReplenishment
		{
			[PXDependsOnFields(typeof(qtyPOPrepared),typeof(qtyPOOrders),typeof(qtyPOReceipts),typeof(qtyInTransit),typeof(qtyINReceipts),typeof(qtyINAssemblySupply))]
			get
			{
				return _QtyPOPrepared + _QtyPOOrders + _QtyPOReceipts + _QtyInTransit + _QtyINReceipts + _QtyINAssemblySupply;
			}
		}
		#endregion	
		#region QtyHardDemand
		public abstract class qtyHardDemand : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyHardDemand;
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. on Demand")]
		public virtual Decimal? QtyHardDemand
		{
			[PXDependsOnFields(typeof(qtySOShipping),typeof(qtySOShipped),typeof(qtySOBackOrdered))]
			get
			{
				return
					_QtySOShipping + _QtySOShipped + _QtySOBackOrdered;
			}
		}
		#endregion	
		#region QtyDemand
		public abstract class qtyDemand : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyDemand;
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. on Demand")]
		public virtual Decimal? QtyDemand
		{
			[PXDependsOnFields(typeof(inclQtySOBooked),typeof(qtySOBooked),typeof(inclQtySOShipping),typeof(qtySOShipping),typeof(inclQtySOShipped),typeof(qtySOShipped),typeof(inclQtySOBackOrdered),typeof(qtySOBackOrdered),typeof(inclQtyINIssues),typeof(qtyINIssues),typeof(inclQtyINAssemblyDemand),typeof(qtyINAssemblyDemand))]
			get
			{
				return 
					(_InclQtySOBooked == true ? _QtySOBooked : 0m ) +
					(_InclQtySOShipping == true ? _QtySOShipping : 0m ) +
					(_InclQtySOShipped == true ? _QtySOShipped : 0m ) +
					(_InclQtySOBackOrdered == true ? _QtySOBackOrdered : 0m) +
					(_InclQtyINIssues == true ? _QtyINIssues : 0m ) +
					(_InclQtyINAssemblyDemand == true ? _QtyINAssemblyDemand : 0m );
			}
		}
		#endregion	
		#region DefaultSubItemID
		public abstract class defaultSubItemID : PX.Data.IBqlField
		{
		}
		protected Int32? _DefaultSubItemID;
		[SubItem(BqlField = typeof(InventoryItem.defaultSubItemID))]
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
	}

	[PX.Objects.GL.TableAndChartDashboardType]
	public class INReplenishmentCreate: PXGraph<INReplenishmentCreate>
	{
		//Cache initialize
		public PXFilter<Vendor> _vendor;

		public PXFilter<INReplenishmentFilter> Filter;
		public PXCancel<INReplenishmentFilter> Cancel;
		[PXFilterable]
		public Processing<INReplenishmentItem> Records;

		public PXAction<INReplenishmentFilter> ViewInventoryItem;
		[PXUIField(DisplayName = "View Stock Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable viewInventoryItem(PXAdapter adapter)
		{
			if (Records.Current != null && Records.Current.InventoryID.HasValue)
			{
				InventoryItemMaint graph = PXGraph.CreateInstance<InventoryItemMaint>();
				InventoryItem invItem = graph.Item.Search<InventoryItem.inventoryID>(Records.Current.InventoryID);
				if (invItem != null)
				{
					graph.Item.Current = invItem;
					throw new PXRedirectRequiredException(graph, true, "View Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		public PXAction<INReplenishmentFilter> ViewVendorCatalogue;
		[PXUIField(DisplayName = "View Vendor Inventory", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable viewVendorCatalogue(PXAdapter adapter)
		{
			if (Records.Current != null && Records.Current.InventoryID.HasValue)
			{
				POVendorCatalogueMaint graph = PXGraph.CreateInstance<POVendorCatalogueMaint>();
				VendorLocation baccount = PXSelect<VendorLocation,
					Where<VendorLocation.bAccountID, Equal<Required<VendorLocation.bAccountID>>,
						And<VendorLocation.locationID, Equal<Required<VendorLocation.locationID>>>>>
						.Select(graph, Records.Current.PreferredVendorID, Records.Current.PreferredVendorLocationID);

				if (baccount != null)
				{
					graph.BAccount.Current = baccount;
					throw new PXRedirectRequiredException(graph, true, "View Vendor Catalogue") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}
		public class Processing<Type> :
		PXFilteredProcessing<INReplenishmentItem, INReplenishmentFilter,
		Where<INReplenishmentItem.siteID, Equal<Current<INReplenishmentFilter.replenishmentSiteID>>,						
						And2<
						Where<INReplenishmentItem.launchDate, IsNull,
						Or<INReplenishmentItem.launchDate, LessEqual<Current<INReplenishmentFilter.purchaseDate>>>>,
						And<Where<INReplenishmentItem.terminationDate, IsNull,
						Or<INReplenishmentItem.terminationDate, GreaterEqual<Current<INReplenishmentFilter.purchaseDate>>>>>
						>>>
		where Type : INReplenishmentItem
		{
			private readonly PXView inventalView;
			public Processing(PXGraph graph)
				: base(graph)
			{
				this._OuterView = new PXView(graph, false, new Select<INReplenishmentItem>(), (PXSelectDelegate)handler);				
				inventalView = new PXView(graph, true, BqlCommand.CreateInstance(
					typeof (
						Select<INReplenishmentItem,
						Where<INReplenishmentItem.siteID, Equal<Current<INReplenishmentFilter.replenishmentSiteID>>,
						And2<
						Where<INReplenishmentItem.launchDate, IsNull,
						Or<INReplenishmentItem.launchDate, LessEqual<Current<INReplenishmentFilter.purchaseDate>>>>,
						And<Where<INReplenishmentItem.terminationDate, IsNull,
						Or<INReplenishmentItem.terminationDate, GreaterEqual<Current<INReplenishmentFilter.purchaseDate>>>>>
						>>>)));

				inventalView.WhereAndCurrent<INReplenishmentFilter>();
			}

			public virtual IEnumerable handler()
			{
				INReplenishmentFilter filter = (INReplenishmentFilter)this._Graph.Caches[typeof (INReplenishmentFilter)].Current;
				foreach (INReplenishmentItem item in inventalView.SelectMulti())
				{
					if (filter.OnlySuggested == false || item.QtyProcess > Decimal.Zero)
						yield return inventalView.Cache.Locate(item) ?? item;
				}
			}
		}

		public INReplenishmentCreate()
		{
			INReplenishmentFilter filter = Filter.Current;
			Records.SetProcessDelegate(delegate(List<INReplenishmentItem> list)
			{
				ReplenishmentCreateProc(list, filter);
			});
		}

		protected virtual void INReplenishmentFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled(this.Records.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<INReplenishmentItem.selected>(this.Records.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<INReplenishmentItem.qtyProcess>(this.Records.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<INReplenishmentItem.replenishmentSource>(this.Records.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<INReplenishmentItem.replenishmentSourceSiteID>(this.Records.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<INReplenishmentItem.preferredVendorID>(this.Records.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<INReplenishmentItem.preferredVendorLocationID>(this.Records.Cache, null, true);
		}
		protected virtual void INReplenishmentItem_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			INReplenishmentItem rec = (INReplenishmentItem)e.Row;
			if (rec != null)
				rec.QtyProcess = RecalcQty(rec);
		}
		protected virtual void INReplenishmentItem_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INReplenishmentItem rec = (INReplenishmentItem)e.Row;
			if (rec == null) return;
			PXUIFieldAttribute.SetEnabled<INReplenishmentItem.replenishmentSourceSiteID>(sender, rec, rec.ReplenishmentSource == INReplenishmentSource.Transfer);
			PXUIFieldAttribute.SetEnabled<INReplenishmentItem.preferredVendorID>(sender, rec, rec.ReplenishmentSource == INReplenishmentSource.Purchased);
			PXUIFieldAttribute.SetEnabled<INReplenishmentItem.preferredVendorLocationID>(sender, rec, rec.ReplenishmentSource == INReplenishmentSource.Purchased);
			if (rec.QtyINReplaned > 0 && rec.Selected == true && rec.QtyProcess == 0)
				sender.RaiseExceptionHandling<INReplenishmentItem.qtyProcess>(rec, rec.QtyProcess,
					new PXSetPropertyException(Messages.ReplenihmentPlanDeleted, PXErrorLevel.Warning));
		}
		protected virtual void INReplenishmentItem_QtyProcess_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INReplenishmentItem rec = (INReplenishmentItem)e.Row;
			if (rec != null )			
				rec.Selected = rec.QtyProcess > 0;			
		}
		protected virtual void INReplenishmentItem_ReplenishmentSource_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INReplenishmentItem rec = (INReplenishmentItem)e.Row;
			if (rec != null)
			{
				Decimal? qty = RecalcQty(rec);
				rec.QtyProcess = qty > 0 ? qty : rec.QtyProcess;
			}
		}
		protected virtual void INReplenishmentItem_PreferredVendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INReplenishmentItem rec = (INReplenishmentItem)e.Row;
			if(rec != null)
			{				
			}
		}
		protected virtual void INReplenishmentItem_PreferredVendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INReplenishmentItem rec = (INReplenishmentItem)e.Row;
			if (rec != null)
		{
				Decimal? qty = RecalcQty(rec);
				rec.QtyProcess = qty > 0 ? qty : rec.QtyProcess;
			}
		}

		protected decimal? RecalcQty(INReplenishmentItem rec)
			{

			Decimal qty = OnRecalcQty(rec);

			if (qty > 0 && rec.ReplenishmentSource != INReplenishmentSource.Transfer)			
				qty = OnRoundQtyByVendor(rec, qty);
			
			return qty > 0? qty : 0;
		}

		protected virtual decimal OnRecalcQty(INReplenishmentItem rec)
		{
			decimal effectiveOnHand = ((rec.QtyOnHand ?? Decimal.Zero)
										+ (rec.QtyReplenishment ?? Decimal.Zero)
										+ (rec.QtyINReplaned ?? Decimal.Zero)
										- (rec.QtyHardDemand ?? Decimal.Zero));
			if (effectiveOnHand < (rec.MinQty??Decimal.Zero))
			{
				if (rec.ReplenishmentSource != INReplenishmentSource.Transfer ||
					rec.ReplenishmentMethod == INReplenishmentMethod.MinMax)
				{
					return (rec.MaxQty ?? Decimal.Zero) -
								 effectiveOnHand;
				}
				else if (rec.ReplenishmentSource == INReplenishmentSource.Transfer &&
					rec.ReplenishmentMethod == INReplenishmentMethod.FixedReorder) 
				{
					return rec.TransferERQ ?? Decimal.Zero;
				}
				return 1m;	//Any demand will changed by vendor ERQ settings.			
			}
			return 0m;
		}

		protected virtual decimal OnRoundQtyByVendor(INReplenishmentItem rec, decimal qty)
		{
			POVendorInventory vendorSettings =
					FetchVendorSettings(this, rec) ?? new POVendorInventory();

			if (rec.ReplenishmentMethod == INReplenishmentMethod.FixedReorder)
						qty = vendorSettings.ERQ.GetValueOrDefault();
					else
					{
						if (vendorSettings.LotSize > 0)
						{
							Decimal size = vendorSettings.LotSize.GetValueOrDefault();
							qty = decimal.Ceiling(qty / size) * size;
						}
						if (qty < vendorSettings.MinOrdQty.GetValueOrDefault())
							qty = vendorSettings.MinOrdQty.GetValueOrDefault();

						Decimal maxOrderQty = vendorSettings.MaxOrdQty ?? Decimal.Zero; //By default, in DB this value is 0, rather then null. So 0 is considered as "not set" for this value
						if (maxOrderQty > 0 && qty > maxOrderQty)
							qty = maxOrderQty;		
						
					}					
			return qty;
		}
		

		protected static void ReplenishmentCreateProc(List<INReplenishmentItem> list, INReplenishmentFilter filter)
		{
			INReplenishmentMaint graph = PXGraph.CreateInstance<INReplenishmentMaint>();						
			int index = 0;
			bool updated = false;
			foreach (INReplenishmentItem rec in list)
			{
				index +=1;
				if (rec.Selected == true || rec.QtyProcess > 0)
					foreach (INItemPlan e in
					PXSelect<INItemPlan,
					Where<INItemPlan.planType, Equal<Required<INItemPlan.planType>>,
					And<INItemPlan.inventoryID, Equal<Required<INItemPlan.inventoryID>>,
					And<INItemPlan.subItemID, Equal<Required<INItemPlan.subItemID>>,
					And<INItemPlan.siteID, Equal<Required<INItemPlan.siteID>>,
						And<INItemPlan.supplyPlanID, IsNull>>>>>>.Select(graph, INPlanConstants.Plan90, rec.InventoryID, rec.SubItemID, rec.SiteID))
						graph.Plans.Delete(e);

				if (rec.QtyProcess > 0)
				{

					INItemPlan plan = new INItemPlan();
					plan.InventoryID = rec.InventoryID;
					plan.SubItemID = rec.SubItemID;
					plan.SiteID = rec.SiteID;
					plan.SourceSiteID = rec.ReplenishmentSource == INReplenishmentSource.Transfer  ? rec.ReplenishmentSourceSiteID : null;
					plan.VendorID = rec.ReplenishmentSource == INReplenishmentSource.Purchased ? rec.PreferredVendorID : null;
					plan.VendorLocationID = rec.ReplenishmentSource == INReplenishmentSource.Purchased
					                        	? rec.PreferredVendorLocationID
					                        	: null;
					plan.PlanQty = rec.QtyProcess;
					plan.PlanDate = filter.PurchaseDate;

					plan.FixedSource =
						rec.ReplenishmentSource == INReplenishmentSource.Transfer 
							? INReplenishmentSource.Transfer
							: INReplenishmentSource.Purchased;

					plan.PlanType = INPlanConstants.Plan90;

					plan.Hold = false;
					graph.Plans.Update(plan);
					updated = true;
				}
			}
			if (updated)
			{				
				INReplenishmentOrder order = new INReplenishmentOrder();
				order.OrderDate = filter.PurchaseDate;
				order.SiteID = filter.ReplenishmentSiteID;
				order.VendorID = filter.PreferredVendorID;								
				order = graph.Document.Update(order);
			}
				graph.Save.Press();
		}

		private static POVendorInventory FetchVendorSettings(PXGraph graph, INReplenishmentItem r)
		{
			var view = 
			new	PXSelect<POVendorInventory,
			Where<POVendorInventory.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
			And<POVendorInventory.subItemID, Equal<Required<POVendorInventory.subItemID>>,
			And<POVendorInventory.vendorID, Equal<Required<POVendorInventory.vendorID>>,
			And<Where2<Where<Required<POVendorInventory.vendorLocationID>, IsNull, And<POVendorInventory.vendorLocationID, IsNull>>,
					    Or<POVendorInventory.vendorLocationID, Equal<Required<POVendorInventory.vendorLocationID>>>>>>>>>(graph);

			return
				(POVendorInventory)view.SelectWindowed(0, 1, r.InventoryID, r.SubItemID, r.PreferredVendorID, r.PreferredVendorLocationID, r.PreferredVendorLocationID) ??
				(POVendorInventory)view.SelectWindowed(0, 1, r.InventoryID, r.DefaultSubItemID, r.PreferredVendorID, r.PreferredVendorLocationID, r.PreferredVendorLocationID) ??
				(POVendorInventory)view.SelectWindowed(0, 1, r.InventoryID, r.SubItemID, r.PreferredVendorID, null, null) ??
				(POVendorInventory)view.SelectWindowed(0, 1, r.InventoryID, r.DefaultSubItemID, r.PreferredVendorID, null, null);
		}
	}
}

