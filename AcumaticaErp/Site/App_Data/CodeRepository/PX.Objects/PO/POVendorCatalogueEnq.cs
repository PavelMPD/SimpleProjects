using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.IN;
using System.Collections;
using PX.TM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.RQ;
using PX.Objects.GL;

namespace PX.Objects.PO
{
	[System.SerializableAttribute()]
	public partial class POVendorInventoryEnqFilter : IBqlTable
	{
		#region CurrentOwnerID
		public abstract class currentOwnerID : PX.Data.IBqlField
		{
		}

		[PXDBGuid]
		[CR.CRCurrentOwnerID]
		public virtual Guid? CurrentOwnerID { get; set; }
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
		#region PendingBasePriceDate
		public abstract class pendingBasePriceDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _PendingBasePriceDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Max. Pending Date")]
		[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck=PXPersistingCheck.Nothing)]
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
		[SubItemRawExt(typeof(POVendorInventoryEnqFilter.inventoryID), DisplayName = "Subitem")]
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
		#region VendorID
		public abstract class vendorID : PX.Data.IBqlField
		{
		}
		protected Int32? _VendorID;
		[VendorNonEmployeeActive(Visibility = PXUIVisibility.SelectorVisible,
			DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true, DisplayName = "Vendor")]		
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
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected String _Descr;
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description Contains", Visibility = PXUIVisibility.SelectorVisible)]
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
	}

	public class POVendorInventoryProjection : TM.OwnedFilter.ProjectionAttribute
	{
		public POVendorInventoryProjection()
			: base(typeof(POVendorInventoryEnqFilter),
			BqlCommand.Compose(
		typeof(Select2<,,>),
		typeof(POVendorInventory),	
		typeof(InnerJoin<InventoryItem,
									On<InventoryItem.inventoryID, Equal<POVendorInventory.inventoryID>>,
					 InnerJoin<Vendor, 
					        On<Vendor.bAccountID, Equal<POVendorInventory.vendorID>>,
					 LeftJoin<Location, 
					        On<Location.bAccountID, Equal<POVendorInventory.vendorID>,
					       And<Location.locationID, Equal<POVendorInventory.vendorLocationID>>>>>>),
    TM.OwnedFilter.ProjectionAttribute.ComposeWhere(
		typeof(POVendorInventoryEnqFilter), 
		typeof(InventoryItem.productWorkgroupID), 
		typeof(InventoryItem.productManagerID))))
		{
		}
	}

	[Serializable]
	[POVendorInventoryProjection]
	public partial class POVendorInventoryExt : POVendorInventory
	{
		#region Selected
		public abstract class selected : PX.Data.IBqlField
		{
		}
		protected Boolean? _Selected = false;
		[PXBool()]
		[PXUIField(DisplayName = "Selected")]
		public virtual Boolean? Selected
		{
			get
			{
				return this._Selected;
			}
			set
			{
				this._Selected = value;
			}
		}
		#endregion		
		#region AcctName
		public abstract class acctName : PX.Data.IBqlField
		{
		}
		protected string _AcctName;
		[PXDBString(60, IsUnicode = true, BqlField=typeof(Vendor.acctName))]
		[PXDefault()]
		[PXUIField(DisplayName = "Vendor Name", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String AcctName
		{
			get
			{
				return this._AcctName;
			}
			set
			{
				this._AcctName = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected String _Descr;
		[PXDBString(255, IsUnicode = true, BqlField=typeof(InventoryItem.descr))]
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
		#region PurchaseUnit
		public new abstract class purchaseUnit : PX.Data.IBqlField
		{
		}
        [INUnit(null, typeof(InventoryItem.baseUnit), DisplayName = "Purchase Unit", Visibility = PXUIVisibility.Visible, BqlField = typeof(POVendorInventory.purchaseUnit))]
		public override String PurchaseUnit
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
		#region ItemClassID
		public abstract class itemClassID : PX.Data.IBqlField
		{
		}
		protected String _ItemClassID;
		[PXDBString(10, IsUnicode = true, BqlField=typeof(InventoryItem.itemClassID))]
		[PXUIField(DisplayName = "Item Class ID", Visibility = PXUIVisibility.SelectorVisible)]
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
		[PXDBString(10, IsUnicode = true, BqlField=typeof(Vendor.vendorClassID))]
		[PXDefault(typeof(Search<APSetup.dfltVendorClassID>))]
		[PXSelector(typeof(Search2<VendorClass.vendorClassID, LeftJoin<EPEmployeeClass, On<EPEmployeeClass.vendorClassID, Equal<VendorClass.vendorClassID>>>, Where<EPEmployeeClass.vendorClassID, IsNull>>), DescriptionField = typeof(VendorClass.descr), CacheGlobal = true)]
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
	}

	[PX.Objects.GL.TableAndChartDashboardType]
	public class POVendorCatalogueEnq : PXGraph<POVendorCatalogueEnq>
	{
		//Cache initialize
		public PXFilter<Vendor> _vendor;

		public PXFilter<POVendorInventoryEnqFilter> VendorCatalogueFilter;
		public PXCancel<POVendorInventoryEnqFilter> Cancel;
		public Processing<POVendorInventoryExt> VendorCatalogue;		

		#region Setups
		public PXSetup<POSetup> posetup;
		#endregion

		public PXAction<POVendorInventoryEnqFilter> ViewInventoryItem;
		[PXUIField(DisplayName = "View Inventory Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton()]
		public virtual IEnumerable viewInventoryItem(PXAdapter adapter)
		{
			if (VendorCatalogue.Current != null && VendorCatalogue.Current.InventoryID.HasValue)
			{
				InventoryItemMaint graph = PXGraph.CreateInstance<InventoryItemMaint>();
				InventoryItem invItem = graph.Item.Search<InventoryItem.inventoryID>(VendorCatalogue.Current.InventoryID);
				if (invItem != null)
				{
					graph.Item.Current = invItem;
					throw new PXRedirectRequiredException(graph, true, "View Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		public PXAction<POVendorInventoryEnqFilter> ViewVendorCatalogue;
		[PXUIField(DisplayName = "View Vendor Inventory", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton()]
		public virtual IEnumerable viewVendorCatalogue(PXAdapter adapter)
		{
			if (VendorCatalogue.Current != null && VendorCatalogue.Current.InventoryID.HasValue)
			{
				POVendorCatalogueMaint graph = PXGraph.CreateInstance<POVendorCatalogueMaint>();
				VendorLocation baccount = PXSelect<VendorLocation,
					Where<VendorLocation.bAccountID, Equal<Required<VendorLocation.bAccountID>>,
					  And<VendorLocation.locationID, Equal<Required<VendorLocation.locationID>>>>>
						.Select(graph, VendorCatalogue.Current.VendorID, VendorCatalogue.Current.VendorLocationID);

				if (baccount != null)
				{
					graph.BAccount.Current = baccount;
					throw new PXRedirectRequiredException(graph, true, "View Vendor Catalogue") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		public POVendorCatalogueEnq() 
		{			
			VendorCatalogue.Cache.AllowDelete = false;
			VendorCatalogue.Cache.AllowInsert = false;

			VendorCatalogue.View.WhereAndCurrent<POVendorInventoryEnqFilter>();
			PXUIFieldAttribute.SetRequired<POVendorInventoryExt.vendorInventoryID>(VendorCatalogue.Cache, false);
			PXUIFieldAttribute.SetRequired<POVendorInventoryExt.vendorID>(VendorCatalogue.Cache, false);			

			VendorCatalogue.SetSelected<POVendorInventoryExt.selected>();
			VendorCatalogue.SetProcessDelegate<POVendorInventoryUpdatePriceProcess>(UpdatePrice);
			VendorCatalogue.SetProcessCaption(SO.Messages.Process);
			VendorCatalogue.SetProcessAllCaption(SO.Messages.ProcessAll);
		}		

		protected static void UpdatePrice(POVendorInventoryUpdatePriceProcess graph, POVendorInventoryExt item)
		{
			graph.UpdatePrice(item);
		}

		public class Processing<Type> : PXFilteredProcessing<POVendorInventoryExt, POVendorInventoryEnqFilter,
			Where<POVendorInventoryExt.pendingDate, IsNotNull,
			And<POVendorInventoryExt.pendingDate, LessEqual<Current<POVendorInventoryEnqFilter.pendingBasePriceDate>>>>>
		where Type : POVendorInventoryExt
		{
			public Processing(PXGraph graph)
				:base(graph)
			{
				this._OuterView.WhereAndCurrent<POVendorInventoryEnqFilter>();
			}
		}
	}

	public class POVendorInventoryUpdatePriceProcess : PXGraph<POVendorInventoryUpdatePriceProcess>
	{		
		public virtual void UpdatePrice(POVendorInventoryExt item)
		{
			using (PXConnectionScope cs = new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					DateTime updateTime = DateTime.Now;
					item.LastPrice = item.EffPrice;
					item.EffPrice = item.PendingPrice;
					item.EffDate = item.PendingDate;
					item.PendingPrice = 0m;
					item.PendingDate = null;

					PXDatabase.Update<POVendorInventory>(
									new PXDataFieldAssign(typeof(POVendorInventory.lastPrice).Name, PXDbType.Decimal, item.LastPrice),
									new PXDataFieldAssign(typeof(POVendorInventory.effPrice).Name, PXDbType.Decimal, item.EffPrice),
									new PXDataFieldAssign(typeof(POVendorInventory.effDate).Name, PXDbType.DateTime, item.EffDate),
									new PXDataFieldAssign(typeof(POVendorInventory.pendingPrice).Name, PXDbType.Decimal, 0m),
									new PXDataFieldAssign(typeof(POVendorInventory.pendingDate).Name, PXDbType.DateTime, null),
									new PXDataFieldAssign(typeof(POVendorInventory.lastModifiedDateTime).Name, PXDbType.DateTime, updateTime),
									new PXDataFieldRestrict(typeof(POVendorInventory.recordID).Name, PXDbType.Int, item.RecordID)
									);

					PXDatabase.Update<APSalesPrice>(
									new PXDataFieldAssign(typeof(APSalesPrice.lastPrice).Name, PXDbType.Decimal, item.LastPrice),
									new PXDataFieldAssign(typeof(APSalesPrice.salesPrice).Name, PXDbType.Decimal, item.EffPrice),
									new PXDataFieldAssign(typeof(APSalesPrice.lastDate).Name, PXDbType.DateTime, item.EffDate),
									new PXDataFieldAssign(typeof(APSalesPrice.pendingPrice).Name, PXDbType.Decimal, 0),
									new PXDataFieldAssign(typeof(APSalesPrice.effectiveDate).Name, PXDbType.DateTime, null),
									new PXDataFieldAssign(typeof(APSalesPrice.lastModifiedDateTime).Name, PXDbType.DateTime, updateTime),

									new PXDataFieldRestrict(typeof(APSalesPrice.inventoryID).Name, PXDbType.Int, item.InventoryID),
									new PXDataFieldRestrict(typeof(APSalesPrice.vendorID).Name, PXDbType.Int, item.VendorID),
									new PXDataFieldRestrict(typeof(APSalesPrice.vendorLocationID).Name, PXDbType.Int, item.VendorLocationID),
									new PXDataFieldRestrict(typeof(APSalesPrice.curyID).Name, PXDbType.NVarChar, item.CuryID),
									new PXDataFieldRestrict(typeof(APSalesPrice.uOM).Name, PXDbType.NVarChar, item.PurchaseUnit),
									new PXDataFieldRestrict(typeof(APSalesPrice.subItemID).Name, PXDbType.Int, item.SubItemID),
									new PXDataFieldRestrict(typeof(APSalesPrice.breakQty).Name, PXDbType.Decimal, 0),
									new PXDataFieldRestrict(typeof(APSalesPrice.pendingBreakQty).Name, PXDbType.Decimal, 0),
									new PXDataFieldRestrict(typeof(APSalesPrice.isPromotionalPrice).Name, PXDbType.Bit, false)
									);

					ts.Complete();
				}
			}


		}
	}
}
