using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.SO;
using PX.Objects.GL;
using System.Collections;
using PX.Objects.DR;
using PX.Objects.PO;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.SM;
using PX.TM;
using PX.Objects.TX;
using ItemStats = PX.Objects.IN.Overrides.INDocumentRelease.ItemStats;
using PX.Objects.AR;

namespace PX.Objects.IN
{
	[PXProjection(typeof(Select4<INSiteStatus, Where<boolTrue, Equal<boolTrue>>, Aggregate<GroupBy<INSiteStatus.inventoryID, GroupBy<INSiteStatus.siteID, Sum<INSiteStatus.qtyOnHand, Sum<INSiteStatus.qtyAvail, Sum<INSiteStatus.qtyNotAvail>>>>>>>))]
    [Serializable]
	public partial class INSiteStatusSummary : INSiteStatus
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region SiteID
		public new abstract class siteID : PX.Data.IBqlField
		{
		}
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.IBqlField
		{
		}
		#endregion
	}

	[PXProjection(typeof(Select4<INLocationStatus, Where<boolTrue, Equal<boolTrue>>, Aggregate<GroupBy<INLocationStatus.inventoryID, GroupBy<INLocationStatus.siteID, GroupBy<INLocationStatus.locationID, Sum<INLocationStatus.qtyOnHand, Sum<INLocationStatus.qtyAvail>>>>>>>))]
    [Serializable]
	public partial class INLocationStatusSummary : INLocationStatus
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		#endregion
		#region SiteID
		public new abstract class siteID : PX.Data.IBqlField
		{
		}
		#endregion
		#region LocationID
		public new abstract class locationID : PX.Data.IBqlField
		{
		}
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.IBqlField
		{
		}
		#endregion
	}

	public class InventoryItemMaint : PXGraph<InventoryItemMaint>
	{
		#region Cache Attached
		[PXDBInt()]
		[PXDBDefault(typeof(InventoryItem.inventoryID))]
		[PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<ARSalesPrice.inventoryID>>>>))]
		public virtual void ARSalesPrice_InventoryID_CacheAttached(PXCache sender) { }

		[PXDBString(255)]
        [PXUIField(DisplayName = "Specific Type")]
		[PXStringList(new string[] { "PX.Objects.CS.SegmentValue", "PX.Objects.IN.InventoryItem" },
			new string[] { "Subitem", "Inventory Item Restriction" })]
		protected virtual void RelationGroup_SpecificType_CacheAttached(PXCache sender)
		{
		}

        [PXDefault()]
		[InventoryRaw(typeof(Where<InventoryItem.stkItem, Equal<True>>), IsKey = true, DisplayName = "Inventory ID", DescriptionField = typeof(InventoryItem.descr), Filterable = true)]
		[PX.Data.EP.PXFieldDescription]
        protected virtual void InventoryItem_InventoryCD_CacheAttached(PXCache sender)
        {
        }	
	
		[PXDBString(1, IsFixed = true, BqlField = typeof(InventoryItem.itemType))]
		[PXDefault(INItemTypes.FinishedGood, typeof(Search<INItemClass.itemType, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>))]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible)]
		[INItemTypes.StockList()]
		protected virtual void InventoryItem_ItemType_CacheAttached(PXCache sender)
		{			
		}		

		[PXDBScalar(typeof(Search<INLotSerClass.lotSerNumVal, Where<INLotSerClass.lotSerClassID, Equal<InventoryItem.lotSerClassID>>>))]		
		protected virtual void InventoryItem_LotSerNumSharedVal_CacheAttached(PXCache sender)
		{
		}

		[PXDBScalar(typeof(Search<INLotSerClass.lotSerNumShared, Where<INLotSerClass.lotSerClassID, Equal<InventoryItem.lotSerClassID>>>))]
		protected virtual void InventoryItem_LotSerNumShared_CacheAttached(PXCache sender)
		{
		}

		[StockItem(IsKey = true, DirtyRead = true)]
		[PXDefault()]
		protected virtual void SiteStatus_InventoryID_CacheAttached(PXCache sender)
		{
		}

		[PXDBDefault(typeof(InventoryItem.inventoryID))]
		[Inventory()]
		[PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<APSalesPrice.inventoryID>>>>))]
		public virtual void APSalesPrice_InventoryID_CacheAttached(PXCache sender) { }

		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<APSalesPrice.vendorID>>>),
			DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location")]
		[PXParent(typeof(Select<Location, Where<Location.bAccountID, Equal<Current<APSalesPrice.vendorID>>,
												And<Location.locationID, Equal<Current<APSalesPrice.vendorLocationID>>>>>))]
		[PXFormula(typeof(Selector<APSalesPrice.vendorID, Vendor.defLocationID>))]
		public virtual void APSalesPrice_VendorLocationID_CacheAttached(PXCache sender) { }

		[SubItem(typeof(APSalesPrice.inventoryID), DisplayName = "Subitem")]
		[PXDefault(PersistingCheck=PXPersistingCheck.NullOrBlank)]
		public virtual void APSalesPrice_SubItemID_CacheAttached(PXCache sender) { }

		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<POVendorInventory.vendorID>>>),
			DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location")]
		[PXFormula(typeof(Selector<POVendorInventory.vendorID, Vendor.defLocationID>))]
		[PXParent(typeof(Select<Location, Where<Location.bAccountID, Equal<Current<POVendorInventory.vendorID>>,
												And<Location.locationID, Equal<Current<POVendorInventory.vendorLocationID>>>>>))]
		protected virtual void POVendorInventory_VendorLocationID_CacheAttached(PXCache sender)
		{
		}

		#region INItemSite
		[StockItem(IsKey = true, DirtyRead = true, CacheGlobal = false, TabOrder = 1)]
		[PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<INItemSite.inventoryID>>>>))]
		[PXDefault()]
		protected virtual void INItemSite_InventoryID_CacheAttached(PXCache sender)
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[ItemSite]
		[PXUIField(DisplayName = "Warehouse", Enabled = false, TabOrder = 2)]
		protected virtual void INItemSite_SiteID_CacheAttached(PXCache sender)
		{
		}		
		#endregion
		#region INItemCategory
		[PXDBInt(IsKey = true)]
		[PXSelector(typeof(INCategory.categoryID), SubstituteKey = typeof(INCategory.categoryCD), DescriptionField = typeof(INCategory.description))]
		[PXUIField(DisplayName = "Category ID")]
		protected virtual void INItemCategory_CategoryID_CacheAttached(PXCache sender)
		{
		}
		[StockItem(IsKey = true)]
		[PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<INItemCategory.inventoryID>>>>))]
		[PXDBLiteDefault(typeof(InventoryItem.inventoryID))]
		protected virtual void INItemCategory_InventoryID_CacheAttached(PXCache sender)
		{
		}

		#endregion
		#region INItemXRef	
		[PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<INItemXRef.inventoryID>>>))]
		[Inventory(Filterable = true, DirtyRead = true, Enabled = false)]
		[PXDBDefault(typeof(InventoryItem.inventoryID), DefaultForInsert = true, DefaultForUpdate = false)]		
		protected virtual void INItemXRef_InventoryID_CacheAttached(PXCache sender)
		{
		}
		#endregion
        #region LotSerClassID
        [PXDBString(10, IsUnicode = true)]
        [PXSelector(typeof(INLotSerClass.lotSerClassID), DescriptionField = typeof(INLotSerClass.descr))]
        [PXUIField(DisplayName = "Lot/Serial Class")]
        [PXDefault(typeof(Search<INItemClass.lotSerClassID, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>))]
        protected virtual void InventoryItem_LotSerClassID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region PostClassID
        [PXDBString(10, IsUnicode = true)]
        [PXSelector(typeof(INPostClass.postClassID), DescriptionField = typeof(INPostClass.descr))]
        [PXUIField(DisplayName = "Posting Class")]
        [PXDefault(typeof(Search<INItemClass.postClassID, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>))]
        protected virtual void InventoryItem_PostClassID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region ItemClassID
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Item Class", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<INItemClass.itemClassID, Where<INItemClass.stkItem, Equal<boolTrue>>>), DescriptionField = typeof(INItemClass.descr))]
        [PXDefault(typeof(Search<INSetup.dfltItemClassID>))]
        protected virtual void InventoryItem_ItemClassID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

		[PXHidden]
		public PXSelect<Location> location; // it's needed to let Location lookup
        public PXSelect<BAccount> baccount; // it's needed to let Customer lookup (Cross-reference tab) to work properly in case AlternateType = [Customer Part Number] 
		public PXSelect<AP.Vendor> vendor;
		public PXSelect<AR.Customer> customer;
		public PXSetup<INSetup> insetup;
		public CMSetupSelect cmsetup;
		public PXSetupOptional<TX.TXAvalaraSetup> avalaraSetup; 
		public PXSelect<InventoryItem,			
			Where<InventoryItem.stkItem, Equal<boolTrue>,
			And<Match<Current<AccessInfo.userName>>>>>
			Item;

		public INSubItemSegmentValueList SegmentValues;

		public PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<InventoryItem.inventoryID>>>> ItemSettings;
		public PXSelect<INItemCost, Where<INItemCost.inventoryID, Equal<Current<InventoryItem.inventoryID>>>> ItemCosts;
		public INUnitSelect<INUnit, InventoryItem.inventoryID, InventoryItem.itemClassID, InventoryItem.salesUnit, InventoryItem.purchaseUnit, InventoryItem.baseUnit, InventoryItem.lotSerClassID> itemunits;

		[PXCopyPasteHiddenView()]
		public PXSelectJoin<INItemSite, 
                            InnerJoin<INSite, On<INSite.siteID, Equal<INItemSite.siteID>>, 
                            LeftJoin<INSiteStatusSummary, On<INSiteStatusSummary.inventoryID, Equal<INItemSite.inventoryID>, 
                                And<INSiteStatusSummary.siteID, Equal<INItemSite.siteID>>>>>, 
                            Where<INItemSite.inventoryID, Equal<Current<InventoryItem.inventoryID>>, 
                                And<Match<INSite, Current<AccessInfo.userName>>>>> itemsiterecords;
		public PXSelect<INItemXRef, Where<INItemXRef.inventoryID, Equal<Current<InventoryItem.inventoryID>>>> itemxrefrecords;
		public PXSetup<INPostClass, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>> postclass;
		public PXSetup<INLotSerClass, Where<INLotSerClass.lotSerClassID, Equal<Current<InventoryItem.lotSerClassID>>>> lotserclass;
		public PXSelect<INComponent, Where<INComponent.inventoryID, Equal<Current<InventoryItem.inventoryID>>>> Components;
		public PXSelect<ARSalesPrice> SalesPrice;
		public PXSelect<INItemClass, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>> ItemClass;

		public PXSelect<INItemRep, Where<INItemRep.inventoryID, Equal<Current<InventoryItem.inventoryID>>>> replenishment;
		public PXSelect<INSubItemRep, 
					 Where<INSubItemRep.inventoryID, Equal<Current<INItemRep.inventoryID>>,
					 And<INSubItemRep.replenishmentClassID, Equal<Current<INItemRep.replenishmentClassID>>>>> subreplenishment;

		public PXSelect<INItemSiteReplenishment, Where<INItemSiteReplenishment.inventoryID, Equal<Current<InventoryItem.inventoryID>>>> itemsitereplenihments;
		
		public POVendorInventorySelect<POVendorInventory, 
			LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<POVendorInventory.inventoryID>>,
			LeftJoin<Vendor, On<Vendor.bAccountID, Equal<POVendorInventory.vendorID>>,
			LeftJoin<Location, On<Location.bAccountID, Equal<POVendorInventory.vendorID>,
						And<Location.locationID, Equal<POVendorInventory.vendorLocationID>>>>>>,
			Where<POVendorInventory.inventoryID, Equal<Current<InventoryItem.inventoryID>>>,
			InventoryItem> VendorItems;

		public PXSelect<ARSalesPrice,
			Where<ARSalesPrice.inventoryID, Equal<Current<InventoryItem.inventoryID>>,
				And<ARSalesPrice.isCustClassPrice, Equal<boolTrue>>>,
			OrderBy<Asc<ARSalesPrice.custPriceClassID,
					Asc<ARSalesPrice.uOM,
					Asc<ARSalesPrice.curyID>>>>> ARSalesPrices;

		public PXSelectJoin<ARSalesPriceEx,
			LeftJoin<Customer, On<Customer.bAccountID, Equal<ARSalesPriceEx.customerID>>>,
			Where<ARSalesPriceEx.inventoryID, Equal<Current<InventoryItem.inventoryID>>,
			 And<ARSalesPriceEx.isCustClassPrice, Equal<boolFalse>>>,
			OrderBy<Asc<Customer.acctCD,
					Asc<ARSalesPriceEx.uOM,
					Asc<ARSalesPriceEx.curyID>>>>> CustomerSalesPrice;

		public PXSelectJoin<APSalesPrice,
			LeftJoin<Vendor, On<Vendor.bAccountID, Equal<APSalesPrice.vendorID>>>,
			Where<APSalesPrice.inventoryID, Equal<Current<InventoryItem.inventoryID>>>,
			OrderBy<Asc<Vendor.acctCD,
					Asc<APSalesPrice.uOM,
					Asc<APSalesPrice.curyID>>>>> VendorSalesPrice;

		public PXSelectJoin<INItemCategory,
	InnerJoin<INCategory, On<INCategory.categoryID, Equal<INItemCategory.categoryID>>>,
	Where<INItemCategory.inventoryID, Equal<Current<InventoryItem.inventoryID>>>,
	OrderBy<Asc<INCategory.categoryCD>>> Categories;

		public PXSelect<INItemBoxEx, Where<INItemBoxEx.inventoryID, Equal<Current<InventoryItem.inventoryID>>>> Boxes;

		public PXSelect<PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus> sitestatus;

		public PXSelect<ItemStats> itemstats;
		
		[PXViewName(CR.Messages.Answers)]
		public CRAttributeList<InventoryItem> Answers;
		

		public PXSelect<PX.SM.RelationGroup> Groups;
		protected System.Collections.IEnumerable groups()
		{
			foreach (PX.SM.RelationGroup group in PXSelect<PX.SM.RelationGroup>.Select(this))
			{
				if((group.SpecificModule == null || group.SpecificModule == typeof(InventoryItem).Namespace) 
					&& (group.SpecificType == null || group.SpecificType == typeof(SegmentValue).FullName || group.SpecificType == typeof(InventoryItem).FullName)
					|| (Item.Current != null && PX.SM.UserAccess.IsIncluded(Item.Current.GroupMask, group)))
				{
					Groups.Current = group;
					yield return group;
				}
			}
		}

		public PXSetup<Company> Company;

		public PXFilter<MassUpdateFilter> MassUpdateSettings;

		public bool ItemDoesNotHaveTransactions
		{
			get
			{
				return (INSiteStatus)PXSelect<INSiteStatus, Where<INSiteStatus.inventoryID, Equal<Current<InventoryItem.inventoryID>>, And<Where<INSiteStatus.qtyOnHand, NotEqual<decimal0>, Or<INSiteStatus.qtyAvail, NotEqual<decimal0>>>>>>.SelectWindowed(this, 0, 1) == null;
			}
		}

        public bool ItemDoesNotHavePlans
        {
            get
            {
                return (INItemPlan)PXSelect<INItemPlan, Where<INItemPlan.inventoryID, Equal<Current<InventoryItem.inventoryID>>>>.SelectWindowed(this, 0, 1) == null;
            }
        }

		public InventoryItemMaint()
		{
			INSetup record = insetup.Current;

			PXUIFieldAttribute.SetVisible<Vendor.curyID>(this.Caches[typeof(Vendor)], null,
				cmsetup.Current.MCActivated == true);
			PXUIFieldAttribute.SetVisible<INUnit.toUnit>(itemunits.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<INUnit.toUnit>(itemunits.Cache, null, false);

			PXUIFieldAttribute.SetVisible<INUnit.sampleToUnit>(itemunits.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<INUnit.sampleToUnit>(itemunits.Cache, null, false);

			PXUIFieldAttribute.SetVisible<InventoryItem.pPVAcctID>(Item.Cache, null, true);
			PXUIFieldAttribute.SetVisible<InventoryItem.pPVSubID>(Item.Cache, null, true);

			PXUIFieldAttribute.SetVisible<InventoryItem.discAcctID>(Item.Cache, null, false);
			PXUIFieldAttribute.SetVisible<InventoryItem.discSubID>(Item.Cache, null, false);

			itemsiterecords.Cache.AllowInsert = false;
			itemsiterecords.Cache.AllowDelete = false;

			PXUIFieldAttribute.SetEnabled(itemsiterecords.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<INItemSite.isDefault>(itemsiterecords.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<INItemSite.siteStatus>(itemsiterecords.Cache, null, true);

			PXUIFieldAttribute.SetEnabled(Groups.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<PX.SM.RelationGroup.included>(Groups.Cache, null, true);

			PXDBDefaultAttribute.SetDefaultForInsert<INItemXRef.inventoryID>(itemxrefrecords.Cache, null, true);
			FieldDefaulting.AddHandler<BAccountR.type>((sender, e) => { if (e.Row != null) e.NewValue = BAccountType.VendorType; });

			action.AddMenuAction(ChangeID);
		}

		#region Buttons Definition		

		[PXCancelButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select)]
		protected virtual System.Collections.IEnumerable Cancel(PXAdapter a)
		{
			foreach (InventoryItem e in (new PXCancel<InventoryItem>(this, "Cancel")).Press(a))
			{				
				if (Item.Cache.GetStatus(e) == PXEntryStatus.Inserted)
				{
					InventoryItem e1 = PXSelect<InventoryItem,
						Where<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>, And<InventoryItem.stkItem, Equal<False>>>>
						.Select(this, e.InventoryCD);
					if (e1 != null)
					{
						Item.Cache.RaiseExceptionHandling<InventoryItem.inventoryCD>(e, e.InventoryCD, 
							new PXSetPropertyException(Messages.NonStockItemExists));
					}
				}
				yield return e;
			}
		}
		public PXSave<InventoryItem> Save;
		public PXAction<InventoryItem> cancel;
		public PXInsert<InventoryItem> Insert;
		public PXCopyPasteAction<InventoryItem> Edit; 
		public PXDelete<InventoryItem> Delete;
		public PXFirst<InventoryItem> First;
		public PXPrevious<InventoryItem> Prev;
		public PXNext<InventoryItem> Next;
		public PXLast<InventoryItem> Last;		

		public PXChangeID<InventoryItem, InventoryItem.inventoryCD> ChangeID;

		public PXAction<InventoryItem> updateCustPriceClass;
		[PXUIField(DisplayName = "Update Prices", Enabled = false)]
		[PXButton]
		public virtual IEnumerable UpdateCustPriceClass(PXAdapter adapter)
		{
			if (MassUpdateSettings.AskExt() == WebDialogResult.OK)
			{
				foreach (ARSalesPrice sp in PXSelect<ARSalesPrice, Where<ARSalesPrice.isCustClassPrice, Equal<boolTrue>, And<ARSalesPrice.inventoryID, Equal<Current<InventoryItem.inventoryID>>, And<ARSalesPrice.isPromotionalPrice, Equal<boolFalse>, And<ARSalesPrice.effectiveDate, IsNotNull, And<ARSalesPrice.effectiveDate, LessEqual<Required<MassUpdateFilter.effectiveDate>>>>>>>>.Select(this, MassUpdateSettings.Current.EffectiveDate))
				{
					ARSalesPrice copy = PXCache<ARSalesPrice>.CreateCopy(sp);
					copy.LastPrice = sp.SalesPrice;
					copy.LastBreakQty = sp.BreakQty;
					copy.LastDate = sp.EffectiveDate;
					copy.SalesPrice = sp.PendingPrice;
					copy.BreakQty = sp.PendingBreakQty;
					copy.EffectiveDate = null;
					copy.PendingPrice = 0;
					copy.PendingBreakQty = 0;
					copy.LastTaxID = copy.TaxID;
					copy.TaxID = copy.PendingTaxID;
					copy.PendingTaxID = null;
					this.ARSalesPrices.Update(copy);
				}
				ARSalesPrices.View.RequestRefresh();
			}
			return adapter.Get();
		}

		public PXAction<InventoryItem> updateCustomerPrice;
		[PXUIField(DisplayName = "Update Prices", MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXButton]
		public virtual IEnumerable UpdateCustomerPrice(PXAdapter adapter)
		{
			if (MassUpdateSettings.AskExt() == WebDialogResult.OK)
			{
				foreach (ARSalesPriceEx sp in PXSelect<ARSalesPriceEx, Where<ARSalesPriceEx.isCustClassPrice, Equal<boolFalse>, And<ARSalesPriceEx.inventoryID, Equal<Current<InventoryItem.inventoryID>>, And<ARSalesPriceEx.isPromotionalPrice, Equal<boolFalse>, And<ARSalesPriceEx.effectiveDate, IsNotNull, And<ARSalesPriceEx.effectiveDate, LessEqual<Required<MassUpdateFilter.effectiveDate>>>>>>>>.Select(this, MassUpdateSettings.Current.EffectiveDate))
				{
					ARSalesPriceEx copy = PXCache<ARSalesPriceEx>.CreateCopy(sp);
					copy.LastPrice = sp.SalesPrice;
					copy.SalesPrice = sp.PendingPrice;
					copy.LastBreakQty = sp.BreakQty;
					copy.BreakQty = sp.PendingBreakQty;
					copy.LastDate = sp.EffectiveDate;
					copy.EffectiveDate = null;
					copy.PendingPrice = 0;
					copy.PendingBreakQty = 0;
					copy.LastTaxID = copy.TaxID;
					copy.TaxID = copy.PendingTaxID;
					copy.PendingTaxID = null;

					this.CustomerSalesPrice.Update(copy);
				}
				CustomerSalesPrice.View.RequestRefresh();
			}
			return adapter.Get();
		}

		public PXAction<InventoryItem> updateAPVendorPrice;
		[PXUIField(DisplayName = "Update Prices", MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXButton]
		public virtual IEnumerable UpdateAPVendorPrice(PXAdapter adapter)
		{
			if (MassUpdateSettings.AskExt() == WebDialogResult.OK)
			{
				foreach (APSalesPrice sp in PXSelect<APSalesPrice, Where<APSalesPrice.inventoryID, Equal<Current<InventoryItem.inventoryID>>, And<APSalesPrice.isPromotionalPrice, Equal<boolFalse>, And<APSalesPrice.effectiveDate, IsNotNull, And<APSalesPrice.effectiveDate, LessEqual<Required<MassUpdateFilter.effectiveDate>>>>>>>.Select(this, MassUpdateSettings.Current.EffectiveDate))
				{
					APSalesPrice copy = PXCache<APSalesPrice>.CreateCopy(sp);
					copy.LastPrice = sp.SalesPrice;
					copy.SalesPrice = sp.PendingPrice;
					copy.LastBreakQty = sp.BreakQty;
					copy.BreakQty = sp.PendingBreakQty;
					copy.LastDate = sp.EffectiveDate;
					copy.EffectiveDate = null;
					copy.PendingPrice = 0;
					copy.PendingBreakQty = 0;

					VendorSalesPrice.Update(copy);
				}
				VendorSalesPrice.View.RequestRefresh();
			}
			return adapter.Get();
		}
		#endregion

		#region InventoryItem Event Handlers

		protected virtual void InventoryItem_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null && ((InventoryItem)e.Row).LotSerNumShared == true)
			{
				sender.SetValue<InventoryItem.lotSerNumVal>(e.Row, sender.GetValue<InventoryItem.lotSerNumSharedVal>(e.Row));
			}
		}

		protected virtual void InventoryItem_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			InventoryItem row = e.Row as InventoryItem;
			if (row == null) return;

			PXUIFieldAttribute.SetEnabled<InventoryItem.lotSerNumVal>(sender, e.Row,
				(lotserclass.Current != null && lotserclass.Current.LotSerNumShared == false && lotserclass.Current.LotSerTrack != INLotSerTrack.NotNumbered));
			
			if (lotserclass.Current != null && lotserclass.Current.LotSerTrack == INLotSerTrack.NotNumbered && e.Row != null)
				((InventoryItem) e.Row).LotSerNumVal = null;

			PXUIFieldAttribute.SetEnabled<InventoryItem.cOGSSubID>(sender, e.Row, (postclass.Current != null && postclass.Current.COGSSubFromSales == false));
			PXUIFieldAttribute.SetEnabled<InventoryItem.stdCstVarAcctID>(sender, e.Row, e.Row != null && ((InventoryItem)e.Row).ValMethod == INValMethod.Standard);
			PXUIFieldAttribute.SetEnabled<InventoryItem.stdCstVarSubID>(sender, e.Row, e.Row != null && ((InventoryItem)e.Row).ValMethod == INValMethod.Standard);
			PXUIFieldAttribute.SetEnabled<InventoryItem.stdCstRevAcctID>(sender, e.Row, e.Row != null && ((InventoryItem)e.Row).ValMethod == INValMethod.Standard);
			PXUIFieldAttribute.SetEnabled<InventoryItem.stdCstRevSubID>(sender, e.Row, e.Row != null && ((InventoryItem)e.Row).ValMethod == INValMethod.Standard);
			PXUIFieldAttribute.SetEnabled<InventoryItem.pendingStdCost>(sender, e.Row, e.Row != null && ((InventoryItem)e.Row).ValMethod == INValMethod.Standard);
			PXUIFieldAttribute.SetEnabled<InventoryItem.pendingStdCostDate>(sender, e.Row, e.Row != null && ((InventoryItem)e.Row).ValMethod == INValMethod.Standard);
			PXUIFieldAttribute.SetVisible<InventoryItem.defaultSubItemOnEntry>(sender, null, insetup.Current.UseInventorySubItem == true);			
			PXUIFieldAttribute.SetEnabled<POVendorInventory.isDefault>(this.VendorItems.Cache, null, true);
			INAcctSubDefault.Required(sender, e);
			PXUIFieldAttribute.SetEnabled<InventoryItem.baseUnit>(sender, e.Row, ItemDoesNotHaveTransactions);

			Item.Cache.AllowDelete = ItemDoesNotHaveTransactions;

			//Multiple Components are not supported for CashReceipt Deferred Revenue:
			DRDeferredCode dc = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Current<InventoryItem.deferredCode>>>>.Select(this);
			PXUIFieldAttribute.SetEnabled<InventoryItem.isSplitted>(sender, e.Row, e.Row != null && dc != null && dc.Method != DeferredMethodType.CashReceipt);
            PXUIFieldAttribute.SetEnabled<InventoryItem.defaultSubItemID>(sender, e.Row, insetup.Current.UseInventorySubItem == true);

			//Initial State for Components:
			Components.Cache.AllowDelete = false;
			Components.Cache.AllowInsert = false;
			Components.Cache.AllowUpdate = false;

            if(e.Row != null)
			    if (((InventoryItem)e.Row).IsSplitted == true)
			    {
				    Components.Cache.AllowDelete = true;
				    Components.Cache.AllowInsert = true;
				    Components.Cache.AllowUpdate = true;
				    ((InventoryItem)e.Row).TotalPercentage = SumComponentsPercentage();
				    PXUIFieldAttribute.SetEnabled<InventoryItem.useParentSubID>(sender, e.Row, true);
			    }
			    else
			    {
				    ((InventoryItem)e.Row).TotalPercentage = 100;
				    ((InventoryItem)e.Row).UseParentSubID = false;
				    PXUIFieldAttribute.SetEnabled<InventoryItem.useParentSubID>(sender, e.Row, false);
			    }

			Boxes.Cache.AllowInsert = row.PackageOption != INPackageOption.Manual && PXAccess.FeatureInstalled<FeaturesSet.autoPackaging>();
			Boxes.Cache.AllowUpdate = row.PackageOption != INPackageOption.Manual && PXAccess.FeatureInstalled<FeaturesSet.autoPackaging>();
			Boxes.Cache.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.autoPackaging>();
				
			if (row.PackageOption == INPackageOption.Quantity)
			{
				PXUIFieldAttribute.SetEnabled<InventoryItem.packSeparately>(Item.Cache, Item.Current, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.qty>(Boxes.Cache, null, true);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.uOM>(Boxes.Cache, null, true);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxQty>(Boxes.Cache, null, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxWeight>(Boxes.Cache, null, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxVolume>(Boxes.Cache, null, false);
			}
			else if (row.PackageOption == INPackageOption.Weight)
			{
				PXUIFieldAttribute.SetEnabled<InventoryItem.packSeparately>(Item.Cache, Item.Current, true);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.qty>(Boxes.Cache, null, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.uOM>(Boxes.Cache, null, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxQty>(Boxes.Cache, null, true);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxWeight>(Boxes.Cache, null, true);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxVolume>(Boxes.Cache, null, false);
			}
			else if (row.PackageOption == INPackageOption.WeightAndVolume)
			{
				PXUIFieldAttribute.SetEnabled<InventoryItem.packSeparately>(Item.Cache, Item.Current, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.qty>(Boxes.Cache, null, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.uOM>(Boxes.Cache, null, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxQty>(Boxes.Cache, null, true);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxWeight>(Boxes.Cache, null, true);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxVolume>(Boxes.Cache, null, true);

			}
			else if (row.PackageOption == INPackageOption.Manual)
			{
				Boxes.Cache.AllowSelect = false;
				PXUIFieldAttribute.SetEnabled<InventoryItem.packSeparately>(Item.Cache, Item.Current, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.qty>(Boxes.Cache, null, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.uOM>(Boxes.Cache, null, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxQty>(Boxes.Cache, null, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxWeight>(Boxes.Cache, null, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxVolume>(Boxes.Cache, null, false);

			}

			if (PXAccess.FeatureInstalled<FeaturesSet.autoPackaging>())
				ValidatePackaging(row);

			this.ARSalesPrices.Cache.AllowInsert =
			this.ARSalesPrices.Cache.AllowUpdate =
			this.ARSalesPrices.Cache.AllowDelete = row.InventoryID > 0;

			this.CustomerSalesPrice.Cache.AllowInsert =
			this.CustomerSalesPrice.Cache.AllowUpdate =
			this.CustomerSalesPrice.Cache.AllowDelete = row.InventoryID > 0;

			this.VendorSalesPrice.Cache.AllowInsert =
			this.VendorSalesPrice.Cache.AllowUpdate =
			this.VendorSalesPrice.Cache.AllowDelete = row.InventoryID > 0;

			this.VendorItems.Cache.AllowInsert =
			this.VendorItems.Cache.AllowUpdate =
			this.VendorItems.Cache.AllowDelete = row.InventoryID > 0;
		}

		protected virtual void InventoryItem_LotSerClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<InventoryItem.lotSerNumVal>(e.Row);
			sender.SetValuePending<InventoryItem.lotSerNumVal>(e.Row, PXCache.NotSetValue);
		}

		protected virtual void InventoryItem_LotSerClassID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INItemLotSerial status =
				PXSelect<INItemLotSerial,
				Where<INItemLotSerial.inventoryID, Equal<Required<INItemLotSerial.inventoryID>>,					
					And<INItemLotSerial.qtyOnHand, NotEqual<decimal0>>>>
					.SelectWindowed(this, 0, 1, ((InventoryItem)e.Row).InventoryID);

			INSiteStatus sitestatus =
				PXSelect<INSiteStatus,
				Where<INSiteStatus.inventoryID, Equal<Required<INSiteStatus.inventoryID>>,
				  And<Where<INSiteStatus.qtyOnHand, NotEqual<decimal0>,
						  Or<INSiteStatus.qtyINReceipts, NotEqual<decimal0>,
						  Or<INSiteStatus.qtyInTransit, NotEqual<decimal0>,
						  Or<INSiteStatus.qtyINIssues, NotEqual<decimal0>,
						  Or<INSiteStatus.qtyINAssemblyDemand, NotEqual<decimal0>,
						  Or<INSiteStatus.qtyINAssemblySupply, NotEqual<decimal0>>>>>>>>>>
				.SelectWindowed(this, 0, 1, ((InventoryItem)e.Row).InventoryID);
			if (status != null || sitestatus != null)
			{
				INLotSerClass oldClass = (INLotSerClass) PXSelectorAttribute.Select<InventoryItem.lotSerClassID>(sender, e.Row);
				INLotSerClass newClass =
					(INLotSerClass) PXSelectorAttribute.Select<InventoryItem.lotSerClassID>(sender, e.Row, e.NewValue);
				if (oldClass.LotSerTrack != newClass.LotSerTrack ||
				    oldClass.LotSerTrackExpiration != newClass.LotSerTrackExpiration ||
				    oldClass.LotSerAssign != newClass.LotSerAssign)
				{
					throw new PXSetPropertyException(Messages.ItemLotSerClassVerifying);
				}
			}
		
		}

        protected virtual void InventoryItem_DefaultSubItemID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
	        if (this.IsImport)
		        e.Cancel = true;
        }

		protected virtual void InventoryItem_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			InventoryItem item = (InventoryItem)e.Row;

			foreach (PXResult<INItemSite, INSite, INSiteStatusSummary> res in itemsiterecords.Select())
			{
			    INItemSite itemsite = res;
			    INSite site = res;
			    INPostClass pclass = postclass.Current;
				bool IsUpdateFlag = false;
				bool IsInserted = itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted;
                sender.RaiseRowSelected(e.Row);

				if (IsInserted)
				{
					if (site != null && site.OverrideInvtAccSub == true)
					{
						itemsite.InvtAcctID = site.InvtAcctID;
						itemsite.InvtSubID = site.InvtSubID;
						IsUpdateFlag = true;
					}
					else if (pclass != null)
					{
						itemsite.InvtAcctID = INReleaseProcess.GetAccountDefaults<INPostClass.invtAcctID>(this, item, site, pclass);
						itemsite.InvtSubID = INReleaseProcess.GetAccountDefaults<INPostClass.invtSubID>(this, item, site, pclass);
						IsUpdateFlag = true;
					}
				}
				else if (pclass != null)
				{
					InventoryItem olditem = (InventoryItem)e.OldRow;
					HashSet<char> maskset = new HashSet<char>(pclass.InvtSubMask.ToCharArray());
					if (pclass.InvtAcctDefault == INAcctSubDefault.MaskItem
						&& maskset.Count == 1 && maskset.Contains(INAcctSubDefault.MaskItem[0])
						&& olditem.InvtAcctID == itemsite.InvtAcctID
						&& olditem.InvtSubID == itemsite.InvtSubID)
					{
						itemsite.InvtAcctID = item.InvtAcctID;
						itemsite.InvtSubID = item.InvtSubID;
						IsUpdateFlag = true;
					}
				}

				if (string.Equals(((InventoryItem)e.Row).ValMethod, ((InventoryItem)e.OldRow).ValMethod) == false || itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
				{
					itemsite.ValMethod = item.ValMethod;
					IsUpdateFlag = true;
				}

				if (itemsite.ValMethod == INValMethod.Standard && (itemsite.StdCostOverride == false || itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted))
				{
					itemsite.PendingStdCost = item.PendingStdCost;
					itemsite.PendingStdCostDate = item.PendingStdCostDate;
					itemsite.StdCost = item.StdCost;
					itemsite.StdCostDate = item.StdCostDate;
					itemsite.LastStdCost = item.LastStdCost;

					IsUpdateFlag = true;
				}

				if (itemsite.BasePriceOverride == false || itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
				{
					itemsite.PendingBasePrice = item.PendingBasePrice;
					itemsite.PendingBasePriceDate = item.PendingBasePriceDate;
					itemsite.BasePrice = item.BasePrice;
					itemsite.BasePriceDate = item.BasePriceDate;
					itemsite.LastBasePrice = item.LastBasePrice;

					IsUpdateFlag = true;
				}

				if (itemsite.MarkupPctOverride == false || itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
				{
					itemsite.MarkupPct = item.MarkupPct;

					IsUpdateFlag = true;
				}

				if (itemsite.RecPriceOverride == false || itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
				{
					itemsite.RecPrice = item.RecPrice;

					IsUpdateFlag = true;
				}

				if (itemsite.ABCCodeOverride == false || itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
				{
					itemsite.ABCCodeID = item.ABCCodeID;
					itemsite.ABCCodeIsFixed = item.ABCCodeIsFixed;

					IsUpdateFlag = true;
				}

				if (itemsite.MovementClassOverride == false || itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
				{
					itemsite.MovementClassID = item.MovementClassID;
					itemsite.MovementClassIsFixed = item.MovementClassIsFixed;

					IsUpdateFlag = true;
				}
				if (itemsite.PreferredVendorOverride == false || itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
				{
					itemsite.PreferredVendorID = item.PreferredVendorID;
					itemsite.PreferredVendorLocationID = item.PreferredVendorLocationID;
					IsUpdateFlag = true;
				}

                if (string.Equals(((InventoryItem)e.Row).SalesUnit, ((InventoryItem)e.OldRow).SalesUnit) == false || itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
                {
                    itemsite.DfltSalesUnit = item.SalesUnit;
                    IsUpdateFlag = true;
                }

                if (string.Equals(((InventoryItem)e.Row).PurchaseUnit, ((InventoryItem)e.OldRow).PurchaseUnit) == false || itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
                {
                    itemsite.DfltPurchaseUnit = item.PurchaseUnit;
                    IsUpdateFlag = true;
                }

				if (INItemSiteMaint.DefaultItemReplenishment(this, itemsite))
					IsUpdateFlag = true;

				if (IsUpdateFlag && itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Notchanged)
				{
					itemsiterecords.Cache.SetStatus(itemsite, PXEntryStatus.Updated);
				}
			}

			if (lotserclass.Current != null && lotserclass.Current.LotSerNumShared == true && string.Equals(item.LotSerNumVal, lotserclass.Current.LotSerNumVal) == false)
			{
				item.LotSerNumVal = lotserclass.Current.LotSerNumVal;
			}

			if (!sender.ObjectsEqual<InventoryItem.pendingBasePrice, InventoryItem.pendingBasePriceDate>(e.Row, e.OldRow) ||
				!sender.ObjectsEqual<InventoryItem.salesUnit>(e.Row, e.OldRow) && SalesPriceUpdateUnit != SalesPriceUpdateUnitType.BaseUnit ||
				!sender.ObjectsEqual<InventoryItem.baseUnit>(e.Row, e.OldRow) && SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit)
			{
				ARSalesPrice sp = PXSelect<ARSalesPrice,
					Where<ARSalesPrice.custPriceClassID, Equal<ARPriceClass.emptyPriceClass>,
					And<ARSalesPrice.inventoryID, Equal<Required<InventoryItem.inventoryID>>,
					And<ARSalesPrice.uOM, Equal<Required<InventoryItem.baseUnit>>,
					And<ARSalesPrice.curyID, Equal<Required<GL.Company.baseCuryID>>,
					And<ARSalesPrice.breakQty, Equal<decimal0>,
					And<ARSalesPrice.isPromotionalPrice, Equal<boolFalse>>>>>>>>.Select(this, item.InventoryID, SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit ? item.BaseUnit : item.SalesUnit, Company.Current.BaseCuryID);

				if (sp != null)
				{
					ARSalesPrice copy = PXCache<ARSalesPrice>.CreateCopy(sp);
					copy.PendingPrice = SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit ? item.PendingBasePrice : INUnitAttribute.ConvertToBase(SalesPrice.Cache, item.InventoryID, item.SalesUnit, item.PendingBasePrice ?? 0m, INPrecision.UNITCOST);
					copy.EffectiveDate = item.PendingBasePriceDate;
					copy.SalesPrice = SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit ? item.BasePrice : INUnitAttribute.ConvertToBase(SalesPrice.Cache, item.InventoryID, item.SalesUnit, item.BasePrice ?? 0m, INPrecision.UNITCOST);
					copy.LastDate = item.BasePriceDate;
					copy.LastPrice = SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit ? item.LastBasePrice : INUnitAttribute.ConvertToBase(SalesPrice.Cache, item.InventoryID, item.SalesUnit, item.LastBasePrice ?? 0m, INPrecision.UNITCOST);

					sender.Graph.RowUpdated.RemoveHandler<ARSalesPrice>(ARSalesPrice_RowUpdated);
					try
					{
						SalesPrice.Update(copy);
					}
					finally
					{
						sender.Graph.RowUpdated.AddHandler<ARSalesPrice>(ARSalesPrice_RowUpdated);
					}
				}
			}
		}

		protected virtual void INItemCost_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INItemCost row = e.Row as INItemCost;
			if (row == null) return;
			bool lastCostEnabled = !(Item.Current.ValMethod == INValMethod.Standard ||
			                         Item.Current.ValMethod == INValMethod.Specific) &&
			                       itemsiterecords.Select(row.InventoryID) != null;
			PXUIFieldAttribute.SetEnabled<INItemCost.lastCost>(sender, e.Row, lastCostEnabled);
		}

		protected virtual void INItemCost_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			INItemCost row = (INItemCost)e.Row;			
			if (row != null && row.LastCost != 0m && row.LastCost != null)
			{
				UdateLastCost(row);
			}
		}
		protected virtual void INItemCost_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			INItemCost row = (INItemCost)e.Row;
			INItemCost oldRow = (INItemCost)e.OldRow;
			if (row != null && oldRow != null && row.LastCost != oldRow.LastCost &&
				  row.LastCost != null)
			{
				UdateLastCost(row);
			}
		}
		private void UdateLastCost(INItemCost row)
		{
			foreach (ItemStats stats in itemstats.Cache.Inserted)
			{
				itemstats.Cache.Delete(stats);
			}
			foreach (INItemSite itemsite in itemsiterecords.Select(row.InventoryID))
			{
				ItemStats stats = new ItemStats();
				stats.InventoryID = itemsite.InventoryID;
				stats.SiteID = itemsite.SiteID;
				stats = itemstats.Insert(stats);
				stats.LastCost = row.LastCost;
				stats.LastCostDate = this.Accessinfo.BusinessDate;
			}
		}
		protected virtual void INSubItemRep_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			UpdateSubItemSiteReplenishment(e.Row, PXDBOperation.Insert);
		}
		protected virtual void INSubItemRep_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			UpdateSubItemSiteReplenishment(e.Row, PXDBOperation.Update);
		}
		protected virtual void INSubItemRep_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			UpdateSubItemSiteReplenishment(e.Row, PXDBOperation.Delete);
		}
		private void UpdateSubItemSiteReplenishment(object item, PXDBOperation operation)
		{
			INSubItemRep row = item as INSubItemRep;
			if(row == null || row.InventoryID == null || row.SubItemID == null ) return;

			foreach(INItemSite itemsite in
				PXSelect<INItemSite, 
					Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
						And<INItemSite.subItemOverride, Equal<boolFalse>>>,
				OrderBy<Asc<INItemSite.inventoryID>>>.Select(this,row.InventoryID))
			{
				if(itemsite.ReplenishmentClassID != row.ReplenishmentClassID) continue;
				PXCache source = this.Caches[typeof (INItemSiteReplenishment)];
				INItemSiteReplenishment r = PXSelect<INItemSiteReplenishment,
					Where<INItemSiteReplenishment.inventoryID, Equal<Required<INItemSiteReplenishment.inventoryID>>,
						And<INItemSiteReplenishment.siteID, Equal<Required<INItemSiteReplenishment.siteID>>,
						And<INItemSiteReplenishment.subItemID, Equal<Required<INItemSiteReplenishment.subItemID>>>>>>
					.SelectWindowed(this, 0,1, row.InventoryID, itemsite.SiteID, row.SubItemID);

				if (r == null)
				{
					if(operation == PXDBOperation.Delete) continue;				

					r = new INItemSiteReplenishment();
					operation = PXDBOperation.Insert;
					r.InventoryID = row.InventoryID;
					r.SiteID = itemsite.SiteID;
					r.SubItemID = row.SubItemID;
				}
				else
					r = PXCache<INItemSiteReplenishment>.CreateCopy(r);
				
				r.SafetyStock = row.SafetyStock;				
				r.MinQty = row.MinQty;
				r.MaxQty = row.MaxQty;
				r.TransferERQ = row.TransferERQ;
				r.ItemStatus = row.ItemStatus;

				switch(operation)
				{
					case PXDBOperation.Insert:
						source.Insert(r);
						break;
					case PXDBOperation.Update:
						source.Update(r);
						break;
					case PXDBOperation.Delete:
						source.Delete(r);
						break;
				}				
			}
			
		}		

		protected virtual void ItemStats_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && ((ItemStats)e.Row).InventoryID < 0 && this.Item.Current != null)
			{
				int? _KeyToAbort = (int?)Item.Cache.GetValue<InventoryItem.inventoryID>(Item.Current);
				if (!_persisted.ContainsKey(_KeyToAbort))
				{
					_persisted.Add(_KeyToAbort, ((ItemStats)e.Row).InventoryID);
				}
				((ItemStats)e.Row).InventoryID = _KeyToAbort;
				sender.Normalize();
			}
		}

		Dictionary<int?, int?> _persisted = new Dictionary<int?, int?>();

		protected virtual void ItemStats_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Aborted && (e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				int? _KeyToAbort;
				if (_persisted.TryGetValue(((ItemStats)e.Row).InventoryID, out _KeyToAbort))
				{
					((ItemStats)e.Row).InventoryID = _KeyToAbort;
				}
			}
		}

		protected virtual void InventoryItem_LotSerNumVal_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
			{
				if (lotserclass.Current != null && (bool)lotserclass.Current.LotSerNumShared)
				{
					e.FieldName = string.Empty;
					e.Cancel = true;
				}
			}
		}

		protected virtual void InventoryItem_LotSerNumVal_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null)
			{
				INLotSerClass lsclass = (INLotSerClass)lotserclass.View.SelectSingleBound(new object[] { e.Row });
				InventoryItem item = (InventoryItem)e.Row;

				if (lsclass != null && lsclass.LotSerNumShared == false)
				{
					string itemLotSerNumVal = sender.GetValuePending<InventoryItem.lotSerNumVal>(e.Row) as string ?? item.LotSerNumVal;

					if (string.IsNullOrEmpty(lsclass.LotSerNumVal) && string.IsNullOrEmpty(itemLotSerNumVal))
					{
						e.NewValue = "000000";
					}
					else
					{
						int currentNum;
						if (int.TryParse(itemLotSerNumVal ?? "0", out currentNum))
						{
							string result = currentNum.ToString(lsclass.LotSerNumVal ?? "000000");
							int checkNum;
							if (int.TryParse(result, out checkNum) && checkNum == currentNum)
							{
								e.NewValue = result;
							}
							else
							{
								e.NewValue = itemLotSerNumVal;
							}
						}
					}

					e.Cancel = true;
				}
			}
		}

		protected virtual void InventoryItem_DfltSiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INItemSite itemsite = (INItemSite)PXSelect<INItemSite, Where<INItemSite.inventoryID, Equal<Current<InventoryItem.inventoryID>>, And<INItemSite.siteID, Equal<Current<InventoryItem.dfltSiteID>>>>>.Select(this);

			INSite site = (INSite)PXSelect<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>.Select(this, ((InventoryItem)e.Row).DfltSiteID);

			if (itemsite != null)
			{
				itemsite = PXCache<INItemSite>.CreateCopy(itemsite);
				itemsite.IsDefault = true;
				itemsiterecords.Update(itemsite);

				//DfltSiteID should follow locations in DAC
				((InventoryItem)e.Row).DfltShipLocationID = itemsite.DfltShipLocationID;
				((InventoryItem)e.Row).DfltReceiptLocationID = itemsite.DfltReceiptLocationID;
			}
			else if (site != null)
			{
				itemsite = new INItemSite();
				itemsite.InventoryID = ((InventoryItem)e.Row).InventoryID;
				itemsite.SiteID = ((InventoryItem)e.Row).DfltSiteID;
				IN.INItemSiteMaint.DefaultItemSiteByItem(this, itemsite, (InventoryItem)e.Row, site, postclass.Current);												
				itemsite.IsDefault = true;				
				itemsite.StdCostOverride = false;
				itemsite.DfltReceiptLocationID = site.ReceiptLocationID;
				itemsite.DfltShipLocationID = site.ShipLocationID;
				itemsiterecords.Insert(itemsite);

				//default item locations in this case too
				((InventoryItem)e.Row).DfltShipLocationID = itemsite.DfltShipLocationID; // already set from site
				((InventoryItem)e.Row).DfltReceiptLocationID = itemsite.DfltReceiptLocationID;
			}
		}

		protected virtual void INItemSite_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void InventoryItem_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			INAcctSubDefault.Required(sender, e);
			InventoryItem row = e.Row as InventoryItem;

			if (row.IsSplitted == true)
			{
				if (string.IsNullOrEmpty(row.DeferredCode))
				{
					if (sender.RaiseExceptionHandling<InventoryItem.deferredCode>(e.Row, row.DeferredCode, new PXSetPropertyException(Data.ErrorMessages.FieldIsEmpty, typeof(InventoryItem.deferredCode).Name)))
					{
						throw new PXRowPersistingException(typeof(InventoryItem.deferredCode).Name, row.DeferredCode, Data.ErrorMessages.FieldIsEmpty, typeof(InventoryItem.deferredCode).Name);
					}
				}

				if (row.TotalPercentage != 100)
				{
					if (sender.RaiseExceptionHandling<InventoryItem.totalPercentage>(e.Row, row.TotalPercentage, new PXSetPropertyException(Messages.SumOfAllComponentsMustBeHundred)))
					{
						throw new PXRowPersistingException(typeof(InventoryItem.totalPercentage).Name, row.TotalPercentage, Messages.SumOfAllComponentsMustBeHundred);
					}
				}
			}

			if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
			{
				if (((InventoryItem)e.Row).ValMethod == INValMethod.Specific && lotserclass.Current != null && (lotserclass.Current.LotSerTrack == INLotSerTrack.NotNumbered || lotserclass.Current.LotSerAssign != INLotSerAssign.WhenReceived))
				{
					if (sender.RaiseExceptionHandling<InventoryItem.valMethod>(e.Row, INValMethod.Specific, new PXSetPropertyException(Messages.SpecificOnlyNumbered)))
					{
						throw new PXRowPersistingException(typeof(InventoryItem.valMethod).Name, INValMethod.Specific, Messages.SpecificOnlyNumbered, typeof(InventoryItem.valMethod).Name);
					}
				}
			}

			if (e.Operation == PXDBOperation.Delete)
			{
				PXDatabase.Delete<INSiteStatus>(
					new PXDataFieldRestrict("InventoryID", PXDbType.Int, ((InventoryItem)e.Row).InventoryID),
					new PXDataFieldRestrict("QtyOnHand", PXDbType.Decimal, 8, 0m, PXComp.EQ),
					new PXDataFieldRestrict("QtyAvail", PXDbType.Decimal, 8, 0m, PXComp.EQ)
					);

				PXDatabase.Delete<INLocationStatus>(
					new PXDataFieldRestrict("InventoryID", PXDbType.Int, ((InventoryItem)e.Row).InventoryID),
					new PXDataFieldRestrict("QtyOnHand", PXDbType.Decimal, 8, 0m, PXComp.EQ),
					new PXDataFieldRestrict("QtyAvail", PXDbType.Decimal, 8, 0m, PXComp.EQ)
					);

				PXDatabase.Delete<INLotSerialStatus>(
					new PXDataFieldRestrict("InventoryID", PXDbType.Int, ((InventoryItem)e.Row).InventoryID),
					new PXDataFieldRestrict("QtyOnHand", PXDbType.Decimal, 8, 0m, PXComp.EQ),
					new PXDataFieldRestrict("QtyAvail", PXDbType.Decimal, 8, 0m, PXComp.EQ)
					);

				PXDatabase.Delete<INCostStatus>(
					new PXDataFieldRestrict("InventoryID", PXDbType.Int, ((InventoryItem)e.Row).InventoryID),
					new PXDataFieldRestrict("QtyOnHand", PXDbType.Decimal, 8, 0m, PXComp.EQ)
					);

				PXDatabase.Delete<INItemCostHist>(
					new PXDataFieldRestrict("InventoryID", PXDbType.Int, ((InventoryItem)e.Row).InventoryID),
					new PXDataFieldRestrict("FinYtdQty", PXDbType.Decimal, 8, 0m, PXComp.EQ),
					new PXDataFieldRestrict("FinYtdCost", PXDbType.Decimal, 8, 0m, PXComp.EQ)
					);

				PXDatabase.Delete<INItemSiteHist>(
					new PXDataFieldRestrict("InventoryID", PXDbType.Int, ((InventoryItem)e.Row).InventoryID),
					new PXDataFieldRestrict("FinYtdQty", PXDbType.Decimal, 8, 0m, PXComp.EQ)
					);

				PXDatabase.Delete<CSAnswers>(
					new PXDataFieldRestrict("EntityID", PXDbType.Int, ((InventoryItem)e.Row).InventoryID),
					new PXDataFieldRestrict("EntityType", PXDbType.Char, 2, ((InventoryItem)e.Row).EntityType, PXComp.EQ)
					);
			}

			if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
			{
				INLotSerClass cls = lotserclass.Current;
				if (cls != null && cls.LotSerTrack != INLotSerTrack.NotNumbered && row.LotSerNumShared == false && string.IsNullOrEmpty(row.LotSerNumVal))
				{
					sender.RaiseExceptionHandling<InventoryItem.lotSerNumVal>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof (InventoryItem.lotSerNumVal).Name));
				}
			}

		}


		protected virtual void ARSalesPrice_UOM_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		private bool AlwaysFromBaseCurrency
		{
			get
			{
				bool alwaysFromBase = false;

				ARSetup arsetup = PXSelect<ARSetup>.Select(this);
				if (arsetup != null)
				{
					alwaysFromBase = arsetup.AlwaysFromBaseCury == true;
				}

				return alwaysFromBase;
			}
		}

		protected virtual void ARSalesPrice_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARSalesPrice row = (ARSalesPrice)e.Row;
			if (row == null) return;
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.effectiveDate>(sender, row, row.IsPromotionalPrice == false);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.pendingBreakQty>(sender, row, row.IsPromotionalPrice == false && !(row.CustPriceClassID==ARPriceClass.EmptyPriceClass && row.BreakQty == 0 && row.PendingBreakQty == 0 && ARSalesPrices.Cache.GetStatus(row)!=PXEntryStatus.Inserted ));
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.uOM>(sender, row, row.IsPromotionalPrice == true || (row.IsPromotionalPrice == false && !(row.CustPriceClassID == ARPriceClass.EmptyPriceClass && row.BreakQty == 0 && row.PendingBreakQty == 0 && ARSalesPrices.Cache.GetStatus(row) != PXEntryStatus.Inserted)));
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.custPriceClassID>(sender, row, row.IsPromotionalPrice == true || (row.IsPromotionalPrice == false && !(row.CustPriceClassID == ARPriceClass.EmptyPriceClass && row.BreakQty == 0 && row.PendingBreakQty == 0 && ARSalesPrices.Cache.GetStatus(row) != PXEntryStatus.Inserted)));		
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.pendingPrice>(sender, row, row.IsPromotionalPrice == false);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.pendingTaxID>(sender, row, row.IsPromotionalPrice == false);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.breakQty>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.salesPrice>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.taxID>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.lastDate>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.expirationDate>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.isPromotionalPrice>(sender, row, ARSalesPrices.Cache.GetStatus(row) == PXEntryStatus.Inserted);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.curyID>(sender, e.Row, AlwaysFromBaseCurrency == false && row.CustPriceClassID != ARPriceClass.EmptyPriceClass);
		}
		protected virtual void ARSalesPriceEx_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARSalesPriceEx row = (ARSalesPriceEx)e.Row;
			if (row == null) return;
			PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.effectiveDate>(sender, row, row.IsPromotionalPrice == false);
			PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.pendingBreakQty>(sender, row, row.IsPromotionalPrice == false);
			PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.pendingPrice>(sender, row, row.IsPromotionalPrice == false);
			PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.pendingTaxID>(sender, row, row.IsPromotionalPrice == false);
			PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.breakQty>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.salesPrice>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.taxID>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.lastDate>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.expirationDate>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.isPromotionalPrice>(sender, row, CustomerSalesPrice.Cache.GetStatus(row) == PXEntryStatus.Inserted);
			Customer cust = (Customer)PXParentAttribute.SelectParent(sender, row, typeof(Customer));
			if (cust != null)
				PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.curyID>(sender, row, AlwaysFromBaseCurrency == false && (cust.CuryID == null || cust.AllowOverrideCury == true));
		}
		protected virtual void APSalesPrice_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			APSalesPrice row = (APSalesPrice)e.Row;
			if (row == null) return;
			PXUIFieldAttribute.SetEnabled<APSalesPrice.effectiveDate>(sender, row, row.IsPromotionalPrice == false);
			PXUIFieldAttribute.SetEnabled<APSalesPrice.pendingBreakQty>(sender, row, row.IsPromotionalPrice == false);
			PXUIFieldAttribute.SetEnabled<APSalesPrice.pendingPrice>(sender, row, row.IsPromotionalPrice == false);
			PXUIFieldAttribute.SetEnabled<APSalesPrice.breakQty>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<APSalesPrice.salesPrice>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<APSalesPrice.lastDate>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<APSalesPrice.expirationDate>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<APSalesPrice.isPromotionalPrice>(sender, row, VendorSalesPrice.Cache.GetStatus(row) == PXEntryStatus.Inserted);
			Vendor vend = (Vendor)PXParentAttribute.SelectParent(sender, row, typeof(Vendor));
			if (vend != null)
			{
				PXUIFieldAttribute.SetEnabled<APSalesPrice.curyID>(sender, row, vend.CuryID == null || vend.AllowOverrideCury == true);
			}
		}

		protected virtual void ARSalesPrice_IsPromotionalPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARSalesPrice row = (ARSalesPrice)e.Row;
			if (row.IsPromotionalPrice == true)
			{
				row.PendingBreakQty = 0;
				row.PendingPrice = 0;
				row.EffectiveDate = null;
				row.LastDate = Accessinfo.BusinessDate;
			}
			else
			{
				row.BreakQty = 0;
				row.SalesPrice = 0;
				row.LastDate = null;
				row.ExpirationDate = null;
				row.EffectiveDate = Accessinfo.BusinessDate;
			}
		}

		protected virtual void ARSalesPrice_CustPriceClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARSalesPrice row = (ARSalesPrice)e.Row;
			if (row.CustPriceClassID == ARPriceClass.EmptyPriceClass)
			{
				sender.SetValueExt<ARSalesPrice.isPromotionalPrice>(row, false);
			}
		}

		protected virtual void ARSalesPrice_PendingPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARSalesPrice row = e.Row as ARSalesPrice;
			if (row != null)
			{
				if (row.EffectiveDate == null && row.IsPromotionalPrice == false && row.PendingPrice != 0m)
					sender.SetDefaultExt<ARSalesPrice.effectiveDate>(e.Row);
			}
		}

		protected virtual void ARSalesPrice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARSalesPrice row = e.Row as ARSalesPrice;
			if (!sender.ObjectsEqual<ARSalesPrice.custPriceClassID, ARSalesPrice.curyID, ARSalesPrice.uOM, ARSalesPrice.pendingBreakQty, ARSalesPrice.pendingPrice, ARSalesPrice.effectiveDate>(e.Row, e.OldRow))
			{
				string uom = SalesPriceUpdateUnit == SalesPriceUpdateUnitType.SalesUnit ? Item.Current.SalesUnit : Item.Current.BaseUnit;
				if (sender.GetStatus(row) != PXEntryStatus.Inserted && row.CustPriceClassID == ARPriceClass.EmptyPriceClass && row.UOM == uom && row.CuryID == Company.Current.BaseCuryID && row.PendingBreakQty == 0 && row.BreakQty == 0 && row.IsPromotionalPrice!=true)
				{
					InventoryItem copy = PXCache<InventoryItem>.CreateCopy(Item.Current);

					copy.PendingBasePriceDate = row.EffectiveDate;
					copy.PendingBasePrice = SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit ? row.PendingPrice ?? 0m : INUnitAttribute.ConvertFromBase(SalesPrice.Cache, Item.Current.InventoryID, Item.Current.SalesUnit, row.PendingPrice ?? 0m, INPrecision.UNITCOST);
					copy.BasePrice = SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit ? row.SalesPrice ?? 0m : INUnitAttribute.ConvertFromBase(SalesPrice.Cache, Item.Current.InventoryID, Item.Current.SalesUnit, row.SalesPrice ?? 0m, INPrecision.UNITCOST);
					copy.LastBasePrice = SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit ? row.LastPrice ?? 0m : INUnitAttribute.ConvertFromBase(SalesPrice.Cache, Item.Current.InventoryID, Item.Current.SalesUnit, row.LastPrice ?? 0m, INPrecision.UNITCOST);
					copy.BasePriceDate = row.LastDate;

					sender.Graph.RowUpdated.RemoveHandler<InventoryItem>(InventoryItem_RowUpdated);
					try
					{
						Item.Update(copy);
					}
					finally
					{
						sender.Graph.RowUpdated.AddHandler<InventoryItem>(InventoryItem_RowUpdated);
					}
				}
			}
		}

		protected virtual void ARSalesPriceEx_IsPromotionalPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARSalesPriceEx row = (ARSalesPriceEx)e.Row;
			if (row.IsPromotionalPrice == true)
			{
				row.PendingBreakQty = 0;
				row.PendingPrice = 0;
				row.EffectiveDate = null;
				row.LastDate = Accessinfo.BusinessDate;
			}
			else
			{
				row.BreakQty = 0;
				row.SalesPrice = 0;
				row.LastDate = null;
				row.ExpirationDate = null;
				row.EffectiveDate = Accessinfo.BusinessDate;
			}
		}

		protected virtual void ARSalesPriceEx_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARSalesPriceEx row = (ARSalesPriceEx)e.Row;

			if (AlwaysFromBaseCurrency == true)
			{
				row.CuryID = Company.Current.BaseCuryID;
			}
			else
			{
				Customer cust = (Customer)PXParentAttribute.SelectParent(sender, row, typeof(Customer));
				if (cust != null)
				{
					if (cust.CuryID != null)
						row.CuryID = cust.CuryID;
					else
						row.CuryID = Company.Current.BaseCuryID;
				}
			}
		}

		protected virtual void ARSalesPriceEx_PendingPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARSalesPriceEx row = e.Row as ARSalesPriceEx;
			if (row != null)
			{
				if (row.EffectiveDate == null && row.IsPromotionalPrice == false && row.PendingPrice != 0m)
					sender.SetDefaultExt<ARSalesPriceEx.effectiveDate>(e.Row);
			}
		}

		protected virtual void APSalesPrice_IsPromotionalPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APSalesPrice row = (APSalesPrice)e.Row;
			if (row.IsPromotionalPrice == true)
			{
				row.PendingBreakQty = 0;
				row.PendingPrice = 0;
				row.EffectiveDate = null;
				row.LastDate = Accessinfo.BusinessDate;
			}
			else
			{
				row.BreakQty = 0;
				row.SalesPrice = 0;
				row.LastDate = null;
				row.ExpirationDate = null;
				row.EffectiveDate = Accessinfo.BusinessDate;
			}
		}

		protected virtual void APSalesPrice_PendingPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APSalesPrice row = e.Row as APSalesPrice;
			if (row != null)
			{
				if (row.EffectiveDate == null && row.IsPromotionalPrice == false && row.PendingPrice != 0m)
					sender.SetDefaultExt<APSalesPrice.effectiveDate>(e.Row);
			}
		}

		protected virtual void ARSalesPrice_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			ARSalesPrice row = (ARSalesPrice)e.Row;
			row.IsCustClassPrice = true;
			row.CuryID = Company.Current.BaseCuryID;
		}

		protected virtual void ARSalesPrice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ARSalesPrice row = (ARSalesPrice)e.Row;
			if (row.CustPriceClassID == null)
				sender.RaiseExceptionHandling<ARSalesPrice.custPriceClassID>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPrice.custPriceClassID).Name));
			if (row.IsPromotionalPrice == true && row.ExpirationDate == null)
				sender.RaiseExceptionHandling<ARSalesPrice.expirationDate>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPrice.expirationDate).Name));
			if (row.IsPromotionalPrice == true && row.LastDate == null)
				sender.RaiseExceptionHandling<ARSalesPrice.lastDate>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPrice.lastDate).Name));
			if (row.IsPromotionalPrice == true && row.ExpirationDate < row.LastDate)
				sender.RaiseExceptionHandling<ARSalesPrice.lastDate>(row, row.LastDate, new PXSetPropertyException(AR.Messages.LastDateExpirationDate, PXErrorLevel.RowError));
			if (row.CuryID == null)
				sender.RaiseExceptionHandling<ARSalesPrice.curyID>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPrice.curyID).Name));
		}

		protected virtual void ARSalesPriceEx_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			ARSalesPriceEx row = (ARSalesPriceEx)e.Row;
			row.IsCustClassPrice = false;
		}

		protected virtual void ARSalesPriceEx_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ARSalesPriceEx row = (ARSalesPriceEx)e.Row;
			if (row.CustomerID == null)
				sender.RaiseExceptionHandling<ARSalesPriceEx.customerID>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPriceEx.customerID).Name));
			if (row.IsPromotionalPrice == true && row.LastDate == null)
				sender.RaiseExceptionHandling<ARSalesPriceEx.expirationDate>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPriceEx.lastDate).Name));
			if (row.IsPromotionalPrice == true && row.ExpirationDate == null)
				sender.RaiseExceptionHandling<ARSalesPriceEx.expirationDate>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPriceEx.expirationDate).Name));
			if (row.IsPromotionalPrice == true && row.ExpirationDate < row.LastDate)
				sender.RaiseExceptionHandling<ARSalesPriceEx.lastDate>(row, row.LastDate, new PXSetPropertyException(AR.Messages.LastDateExpirationDate, PXErrorLevel.RowError));
			if (row.CuryID == null)
				sender.RaiseExceptionHandling<ARSalesPriceEx.curyID>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPriceEx.curyID).Name));
		}

		protected virtual void APSalesPrice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			APSalesPrice price = e.Row as APSalesPrice;

			if (price.BreakQty == 0 && price.PendingBreakQty == 0 && price.IsPromotionalPrice != true && (!sender.ObjectsEqual<APSalesPrice.pendingPrice>(e.Row, e.OldRow) ||
				!sender.ObjectsEqual<APSalesPrice.effectiveDate>(e.Row, e.OldRow) ||
				!sender.ObjectsEqual<APSalesPrice.salesPrice>(e.Row, e.OldRow)))
			{
				POVendorInventory vi = PXSelect<POVendorInventory,
					Where<POVendorInventory.inventoryID, Equal<Required<APSalesPrice.inventoryID>>,
					And<POVendorInventory.vendorID, Equal<Required<APSalesPrice.vendorID>>,
					And<POVendorInventory.vendorLocationID, Equal<Required<APSalesPrice.vendorLocationID>>,
					And<POVendorInventory.purchaseUnit, Equal<Required<APSalesPrice.uOM>>,
					And<POVendorInventory.curyID, Equal<Required<APSalesPrice.curyID>>,
					And<POVendorInventory.subItemID, Equal<Required<APSalesPrice.subItemID>>>>>>>>>.Select(this, price.InventoryID, price.VendorID, price.VendorLocationID, price.UOM, price.CuryID, price.SubItemID);

				if (vi != null)
				{
					POVendorInventory copy = PXCache<POVendorInventory>.CreateCopy(vi);
					copy.PendingPrice = price.PendingPrice;
					copy.PendingDate = price.EffectiveDate;
					copy.EffPrice = price.SalesPrice;
					copy.EffDate = price.LastDate;
					copy.LastPrice = price.LastPrice;

					sender.Graph.RowUpdated.RemoveHandler<POVendorInventory>(POVendorInventory_RowUpdated);
					try
					{
						VendorItems.Update(copy);
					}
					finally
					{
						sender.Graph.RowUpdated.AddHandler<POVendorInventory>(POVendorInventory_RowUpdated);
					}
				}
			}
		}

		protected virtual void APSalesPrice_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			APSalesPrice price = e.Row as APSalesPrice;
			POVendorInventory vendor = getPOVendorInventory(price);
			if (vendor != null)
				VendorItems.Delete(vendor);
		}

		protected virtual void APSalesPrice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			APSalesPrice row = (APSalesPrice)e.Row;
			if (row.IsPromotionalPrice == true && row.LastDate == null)
				sender.RaiseExceptionHandling<APSalesPrice.lastDate>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APSalesPrice.lastDate).Name));
			if (row.IsPromotionalPrice == true && row.ExpirationDate == null)
				sender.RaiseExceptionHandling<APSalesPrice.expirationDate>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APSalesPrice.expirationDate).Name));
			if (row.IsPromotionalPrice == true && row.ExpirationDate < row.LastDate)
				sender.RaiseExceptionHandling<APSalesPrice.lastDate>(row, row.LastDate, new PXSetPropertyException(AR.Messages.LastDateExpirationDate, PXErrorLevel.RowError));
			if (row.CuryID == null)
				sender.RaiseExceptionHandling<APSalesPrice.curyID>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APSalesPrice.curyID).Name));
			if (row.SubItemID == null)
				sender.RaiseExceptionHandling<APSalesPrice.subItemID>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APSalesPrice.subItemID).Name));
		}

		protected virtual void APSalesPrice_VendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APSalesPrice row = (APSalesPrice)e.Row;
			Vendor vend = (Vendor)PXParentAttribute.SelectParent(sender, row, typeof(Vendor));
			if (vend != null)
			{
				if (vend.CuryID != null)
					row.CuryID = vend.CuryID;
				else
					row.CuryID = Company.Current.BaseCuryID;
			}
		}

		protected virtual void ARSalesPrice_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			ARSalesPrice row = e.Row as ARSalesPrice;
			string uOM = SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit ? ItemSettings.Current.BaseUnit : ItemSettings.Current.SalesUnit;
			if (row.IsPromotionalPrice == false && row.CustPriceClassID == ARPriceClass.EmptyPriceClass && row.BreakQty == 0 && row.PendingBreakQty == 0 && row.UOM == uOM)
			{
				e.Cancel=true;
				throw new Exception(Messages.BaseSalesPriceDelete);
			}
		}

		private void InsertSalesPrice(PXCache cache, InventoryItem item, string uom)
		{
			ARSalesPrice price = PXSelect<ARSalesPrice,
													Where<ARSalesPrice.inventoryID, Equal<Required<InventoryItem.inventoryID>>,
															And2<Where<ARSalesPrice.custPriceClassID, IsNotNull, And<ARSalesPrice.custPriceClassID, Equal<ARPriceClass.emptyPriceClass>>>,
															And<ARSalesPrice.uOM, Equal<Required<ARSalesPrice.uOM>>,
															And<ARSalesPrice.isPromotionalPrice, Equal<boolFalse>,
															And<ARSalesPrice.pendingBreakQty, Equal<decimal0>,
															And<ARSalesPrice.breakQty, Equal<decimal0>,
															And<ARSalesPrice.curyID, Equal<Required<GL.Company.baseCuryID>>>>>>>>>>.SelectSingleBound(this, null, item.InventoryID, uom, Company.Current.BaseCuryID);
			if (price == null)
			{
				ARSalesPrice sp = new ARSalesPrice();
				sp.CustPriceClassID = AR.ARPriceClass.EmptyPriceClass;
				sp.InventoryID = item.InventoryID;
				sp.CuryID = Company.Current.BaseCuryID;
				sp.UOM = uom;

				sp.EffectiveDate = item.PendingBasePriceDate;
				sp.LastDate = item.BasePriceDate;

				if (uom != item.BaseUnit)
				{
					sp.SalesPrice = INUnitAttribute.ConvertToBase(cache, item.InventoryID, uom, item.BasePrice ?? 0, INPrecision.UNITCOST);
					sp.PendingPrice = INUnitAttribute.ConvertToBase(cache, item.InventoryID, uom, item.PendingBasePrice ?? 0, INPrecision.UNITCOST);
				}
				else
				{
					sp.SalesPrice = item.BasePrice;
					sp.PendingPrice = item.PendingBasePrice;
				}

				if (sp.PendingPrice <= 0m)
				{
					sp.PendingPrice = 0;
					sp.EffectiveDate = null;
				}

				SalesPrice.Insert(sp);
			}
		}

		private void InsertPOVendorInventory(APSalesPrice price)
		{
			POVendorInventory inventory = getPOVendorInventory(price);
			if (inventory == null)
			{
				POVendorInventory vi = new POVendorInventory();
				vi.InventoryID = price.InventoryID;
				vi.VendorID = price.VendorID;
				vi.VendorLocationID = price.VendorLocationID;
				vi.PurchaseUnit = price.UOM;
				vi.SubItemID = price.SubItemID;
				vi.CuryID = price.CuryID;
				vi.PendingDate = price.EffectiveDate;
				vi.PendingPrice = price.PendingPrice;
				vi.EffDate = price.LastDate;
				vi.EffPrice = price.SalesPrice;
				vi.LastPrice = price.LastPrice;
				VendorItems.Insert(vi);
			}
		}

		private POVendorInventory getPOVendorInventory(APSalesPrice price)
		{
			return PXSelect<POVendorInventory,
										Where<POVendorInventory.inventoryID, Equal<Required<APSalesPrice.inventoryID>>,
											And<POVendorInventory.vendorID, Equal<Required<APSalesPrice.vendorID>>,
											And<POVendorInventory.vendorLocationID, Equal<Required<APSalesPrice.vendorLocationID>>,
											And<POVendorInventory.purchaseUnit, Equal<Required<APSalesPrice.uOM>>,
											And<POVendorInventory.curyID, Equal<Required<APSalesPrice.curyID>>,
											And<POVendorInventory.subItemID, Equal<Required<APSalesPrice.subItemID>>>>>>>>>.Select(this, price.InventoryID, price.VendorID, price.VendorLocationID, price.UOM, price.CuryID, price.SubItemID);
		}

		private void InsertAPVendorPrice(POVendorInventory inventory)
		{
			APSalesPrice price = getAPSalesPrice(inventory);
			if (price == null)
			{
				APSalesPrice vp = new APSalesPrice();
				vp.InventoryID = inventory.InventoryID;
				vp.VendorID = inventory.VendorID;
				vp.VendorLocationID = inventory.VendorLocationID;
				vp.UOM = inventory.PurchaseUnit;
				vp.SubItemID = inventory.SubItemID;
				vp.CuryID = inventory.CuryID;
				vp.EffectiveDate = inventory.PendingDate;
				vp.PendingPrice = inventory.PendingPrice;
				vp.LastDate = inventory.EffDate;
				vp.SalesPrice = inventory.EffPrice;
				vp.LastPrice = inventory.LastPrice;
				vp.IsPromotionalPrice = false;
				vp.PendingBreakQty = 0;
				vp.BreakQty = 0;
				vp.LastBreakQty = 0;
				VendorSalesPrice.Insert(vp);
			}
		}

		private APSalesPrice getAPSalesPrice(POVendorInventory inventory)
		{
			return PXSelect<APSalesPrice,
										Where<APSalesPrice.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
											And<APSalesPrice.vendorID, Equal<Required<POVendorInventory.vendorID>>,
											And<APSalesPrice.vendorLocationID, Equal<Required<POVendorInventory.vendorLocationID>>,
											And<APSalesPrice.uOM, Equal<Required<POVendorInventory.purchaseUnit>>,
											And<APSalesPrice.curyID, Equal<Required<POVendorInventory.curyID>>,
											And<APSalesPrice.subItemID, Equal<Required<POVendorInventory.subItemID>>,
											And<APSalesPrice.isPromotionalPrice, Equal<boolFalse>,
											And<APSalesPrice.pendingBreakQty, Equal<decimal0>,
											And<APSalesPrice.breakQty, Equal<decimal0>>>>>>>>>>>.Select(this, inventory.InventoryID, inventory.VendorID, inventory.VendorLocationID, inventory.PurchaseUnit, inventory.CuryID, inventory.SubItemID);
		}

		protected virtual void InventoryItem_DfltReceiptLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INItemSite itemsite = (INItemSite)PXSelect<INItemSite, Where<INItemSite.inventoryID, Equal<Current<InventoryItem.inventoryID>>, And<INItemSite.siteID, Equal<Current<InventoryItem.dfltSiteID>>>>>.Select(this);

			if (itemsite != null)
			{
				itemsite.DfltReceiptLocationID = ((InventoryItem)e.Row).DfltReceiptLocationID;
				if (itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Notchanged)
				{
					itemsiterecords.Cache.SetStatus(itemsite, PXEntryStatus.Updated);
				}
			}
		}

		protected virtual void InventoryItem_DfltShipLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INItemSite itemsite = (INItemSite)PXSelect<INItemSite, Where<INItemSite.inventoryID, Equal<Current<InventoryItem.inventoryID>>, And<INItemSite.siteID, Equal<Current<InventoryItem.dfltSiteID>>>>>.Select(this);

			if (itemsite != null)
			{
				itemsite.DfltShipLocationID = ((InventoryItem)e.Row).DfltShipLocationID;
				if (itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Notchanged)
				{
					itemsiterecords.Cache.SetStatus(itemsite, PXEntryStatus.Updated);
				}
			}
		}
		
		protected virtual void InventoryItem_DefaultSubItemID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			AddVendorDetail(sender, (InventoryItem)e.Row);
		}
		
		protected virtual void InventoryItem_PreferredVendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			AddVendorDetail(sender, (InventoryItem)e.Row);
		}

		private POVendorInventory AddVendorDetail(PXCache sender, InventoryItem row)
		{
			if (row == null || row.PreferredVendorID == null || row.DefaultSubItemID == null)
			{
				return null;
			}

			POVendorInventory item = 
				PXSelect<POVendorInventory,
			Where<POVendorInventory.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
			And<POVendorInventory.subItemID, Equal<Required<InventoryItem.defaultSubItemID>>,
			And<POVendorInventory.vendorID, Equal<Required<POVendorInventory.vendorID>>,
            And<Where<POVendorInventory.vendorLocationID, Equal<Required<InventoryItem.preferredVendorLocationID>>,				        
						 Or<POVendorInventory.vendorLocationID,IsNull>>>>>>>
			.SelectWindowed(this,0,1,row.InventoryID, row.DefaultSubItemID, row.PreferredVendorID, row.PreferredVendorLocationID);
			if (item == null)
			{
				item = new POVendorInventory();
				item.InventoryID = row.InventoryID;
				item.SubItemID = row.DefaultSubItemID;
				item.PurchaseUnit = row.PurchaseUnit;
				item.VendorID = row.PreferredVendorID;
				item.VendorLocationID = row.PreferredVendorLocationID;				
				item = (POVendorInventory)VendorItems.Cache.Insert(item);				
			}
			return item;
		}

		protected virtual void InventoryItem_ItemClassID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			this.doResetDefaultsOnItemClassChange = false;
				InventoryItem row = (InventoryItem)e.Row;
				INItemClass ic = (INItemClass)PXSelectorAttribute.Select<INItemClass.itemClassID>(cache, row, e.NewValue);

				if (ic != null)
				{
					this.doResetDefaultsOnItemClassChange = true;
					if (e.ExternalCall && cache.GetStatus(row) != PXEntryStatus.Inserted)
					{
						if (Item.Ask(AR.Messages.Warning, Messages.ItemClassChangeWarning, MessageButtons.YesNo) == WebDialogResult.No)
						{
							this.doResetDefaultsOnItemClassChange = false;
						}
					}
				}
			}
		protected virtual void InventoryItem_ItemClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
				if (doResetDefaultsOnItemClassChange)
				{
					sender.SetDefaultExt<InventoryItem.postClassID>(e.Row);
					sender.SetDefaultExt<InventoryItem.priceClassID>(e.Row);
					sender.SetDefaultExt<InventoryItem.priceWorkgroupID>(e.Row);
					sender.SetDefaultExt<InventoryItem.priceManagerID>(e.Row);
					sender.SetDefaultExt<InventoryItem.markupPct>(e.Row);
					sender.SetDefaultExt<InventoryItem.minGrossProfitPct>(e.Row);
					

					INItemClass ic = ItemClass.Select();
					if (ic != null)
					{
						sender.SetValue<InventoryItem.priceWorkgroupID>(e.Row, ic.PriceWorkgroupID);
						sender.SetValue<InventoryItem.priceManagerID>(e.Row, ic.PriceManagerID);
					}					

				sender.SetDefaultExt<InventoryItem.lotSerClassID>(e.Row);

				//sales and purchase units must be cleared not to be added to item unit conversions on base unit change.
				sender.SetValueExt<InventoryItem.baseUnit>(e.Row, null);
				sender.SetValue<InventoryItem.salesUnit>(e.Row, null);
				sender.SetValue<InventoryItem.purchaseUnit>(e.Row, null);
				sender.SetDefaultExt<InventoryItem.baseUnit>(e.Row);
				sender.SetDefaultExt<InventoryItem.salesUnit>(e.Row);
				sender.SetDefaultExt<InventoryItem.purchaseUnit>(e.Row);
				sender.SetDefaultExt<InventoryItem.dfltSiteID>(e.Row);
				sender.SetDefaultExt<InventoryItem.valMethod>(e.Row);

					sender.SetDefaultExt<InventoryItem.taxCategoryID>(e.Row);
					sender.SetDefaultExt<InventoryItem.itemType>(e.Row);
				}
		
			AppendGroupMask(((InventoryItem)e.Row).ItemClassID, sender.GetStatus(e.Row) == PXEntryStatus.Inserted);

			if ((InventoryItem)e.Row != null && ((InventoryItem)e.Row).ItemClassID != null && e.ExternalCall)
			{
				Answers.Cache.Clear();
			}

			if (sender.GetStatus(e.Row) == PXEntryStatus.Inserted)
			{
				foreach (INItemRep r in this.replenishment
					.Select(sender.GetValue<InventoryItem.inventoryID>(e.Row)))
					this.replenishment.Delete(r);

				foreach (INItemClassRep r in PXSelect<INItemClassRep,
					Where<INItemClassRep.itemClassID, Equal<Required<INItemClassRep.itemClassID>>>>
					.Select(this, sender.GetValue<InventoryItem.itemClassID>(e.Row)))
				{
					INItemRep ri = new INItemRep();
					ri.ReplenishmentClassID = r.ReplenishmentClassID;
					ri.ReplenishmentMethod = r.ReplenishmentMethod;
					ri.ReplenishmentPolicyID = r.ReplenishmentPolicyID;
					ri.LaunchDate = r.LaunchDate;
					ri.TerminationDate = r.TerminationDate;
					this.replenishment.Insert(ri);
				}
			}
		}

		protected virtual void InventoryItem_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			((InventoryItem)e.Row).TotalPercentage = 100;
			foreach (INItemClassRep r in PXSelect<INItemClassRep,
					Where<INItemClassRep.itemClassID, Equal<Required<INItemClassRep.itemClassID>>>>
					.Select(this, ((InventoryItem)e.Row).ItemClassID))
			{
				INItemRep ri = new INItemRep();
				ri.ReplenishmentClassID = r.ReplenishmentClassID;
				ri.ReplenishmentMethod = r.ReplenishmentMethod;
				ri.ReplenishmentPolicyID = r.ReplenishmentPolicyID;
				ri.LaunchDate = r.LaunchDate;
				ri.TerminationDate = r.TerminationDate;
				this.replenishment.Insert(ri);
			}
			INItemClass ic = ItemClass.Select();
			if (((InventoryItem)e.Row).InventoryCD != null &&
				((InventoryItem)e.Row).ItemClassID != null &&
				((InventoryItem)e.Row).DfltSiteID == ic.DfltSiteID)
				sender.SetDefaultExt<InventoryItem.dfltSiteID>(e.Row);
				
			AppendGroupMask(((InventoryItem)e.Row).ItemClassID, true);
			_JustInserted = true;
		}

		protected virtual void InventoryItem_PostClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<InventoryItem.invtAcctID>(e.Row);
			sender.SetDefaultExt<InventoryItem.invtSubID>(e.Row);
			sender.SetDefaultExt<InventoryItem.salesAcctID>(e.Row);
			sender.SetDefaultExt<InventoryItem.salesSubID>(e.Row);
			sender.SetDefaultExt<InventoryItem.cOGSAcctID>(e.Row);
			sender.SetDefaultExt<InventoryItem.cOGSSubID>(e.Row);
			sender.SetDefaultExt<InventoryItem.discAcctID>(e.Row);
			sender.SetDefaultExt<InventoryItem.discSubID>(e.Row);
			sender.SetDefaultExt<InventoryItem.stdCstVarAcctID>(e.Row);
			sender.SetDefaultExt<InventoryItem.stdCstVarSubID>(e.Row);
			sender.SetDefaultExt<InventoryItem.stdCstRevAcctID>(e.Row);
			sender.SetDefaultExt<InventoryItem.stdCstRevSubID>(e.Row);
			sender.SetDefaultExt<InventoryItem.pPVAcctID>(e.Row);
			sender.SetDefaultExt<InventoryItem.pPVSubID>(e.Row);
			sender.SetDefaultExt<InventoryItem.pOAccrualAcctID>(e.Row);
			sender.SetDefaultExt<InventoryItem.pOAccrualSubID>(e.Row);

			sender.SetDefaultExt<InventoryItem.reasonCodeSubID>(e.Row);
			sender.SetDefaultExt<InventoryItem.lCVarianceAcctID>(e.Row);
			sender.SetDefaultExt<InventoryItem.lCVarianceSubID>(e.Row);		
		}

		protected virtual void InventoryItem_PurchaseUnit_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			InventoryItem row = (InventoryItem)e.Row;
			if (row == null || string.Compare(row.PurchaseUnit, (string)e.OldValue, true) == 0) return;

			foreach (POVendorInventory item in
				PXSelectJoin<POVendorInventory,
					LeftJoin<POVendorInventoryU, 
								On<POVendorInventoryU.inventoryID, Equal<POVendorInventory.inventoryID>,
								And<POVendorInventoryU.subItemID, Equal<POVendorInventory.subItemID>,
								And<POVendorInventoryU.purchaseUnit, Equal<Required<POVendorInventoryU.purchaseUnit>>,
								And<POVendorInventoryU.vendorID, Equal<POVendorInventory.vendorID>,
								And<Where<POVendorInventoryU.vendorLocationID, Equal<POVendorInventory.vendorLocationID>,
											 Or<Where<POVendorInventoryU.vendorLocationID, IsNull, And<POVendorInventory.vendorLocationID, IsNull>>>>>>>>>>, 
				Where<POVendorInventory.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
					And<POVendorInventory.purchaseUnit, Equal<Required<POVendorInventory.purchaseUnit>>,
					And<POVendorInventoryU.recordID, IsNull>>>>
					.Select(this, row.PurchaseUnit, row.InventoryID, e.OldValue))
			{
				POVendorInventory upd = PXCache<POVendorInventory>.CreateCopy(item);
				if (item.EffPrice != null)
					upd.EffPrice = POItemCostManager.ConvertUOM(this, row, (string)e.OldValue, item.EffPrice.Value, row.PurchaseUnit);
				if (item.LastPrice != null)
					upd.LastPrice = POItemCostManager.ConvertUOM(this, row, (string)e.OldValue, item.LastPrice.Value, row.PurchaseUnit);
				if (item.PendingPrice != null)
					upd.PendingPrice = POItemCostManager.ConvertUOM(this, row, (string)e.OldValue, item.PendingPrice.Value, row.PurchaseUnit);
				upd.PurchaseUnit = row.PurchaseUnit;
				this.VendorItems.Update(upd);
			}
		}
		protected virtual void Vendor_CuryID_FieldSelecting(PXCache sedner, PXFieldSelectingEventArgs e)
		{
			if (e.ReturnValue == null)
				e.ReturnValue = this.Company.Current.BaseCuryID;
		}

		protected virtual void POVendorInventory_IsDefault_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			POVendorInventory current = e.Row as POVendorInventory;
			if ((POVendorInventory) this.VendorItems.SelectWindowed(0, 1, current.InventoryID) == null)
			{
				e.NewValue = true;
				e.Cancel = true;
			}
		}

		protected virtual void POVendorInventory_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			POVendorInventory current = e.Row as POVendorInventory;								
            if (current.VendorID != null && current.SubItemID != null)
                if ((current.IsDefault == true && (this.Item.Current.PreferredVendorID != current.VendorID || this.Item.Current.PreferredVendorLocationID != current.VendorLocationID) ||
                    (current.IsDefault != true && this.Item.Current.PreferredVendorID == current.VendorID && this.Item.Current.PreferredVendorLocationID == current.VendorLocationID)))
			{
                    InventoryItem upd = this.Item.Current;
				upd.PreferredVendorID = current.IsDefault == true ? current.VendorID : null;
				upd.PreferredVendorLocationID = current.IsDefault == true ? current.VendorLocationID : null;
                    if (this.Item.Cache.GetStatus(upd) == PXEntryStatus.Notchanged)
                        this.Item.Cache.SetStatus(upd, PXEntryStatus.Updated);
                    this.VendorItems.View.RequestRefresh();
			}
		}

		protected virtual void POVendorInventory_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			POVendorInventory current = e.Row as POVendorInventory;
			POVendorInventory old = e.OldRow as POVendorInventory;
			if (current == null || old == null) return;

			if(current.VendorID != null && current.SubItemID != null)
				if ((current.IsDefault == true && (this.Item.Current.PreferredVendorID != current.VendorID ||this.Item.Current.PreferredVendorLocationID != current.VendorLocationID) ||
						(current.IsDefault != true &&  this.Item.Current.PreferredVendorID == current.VendorID && this.Item.Current.PreferredVendorLocationID == current.VendorLocationID)))
				{
                    InventoryItem upd =this.Item.Current;
					upd.PreferredVendorID = current.IsDefault == true ? current.VendorID : null;
					upd.PreferredVendorLocationID = current.IsDefault == true ? current.VendorLocationID : null;
					if(this.Item.Cache.GetStatus(upd) == PXEntryStatus.Notchanged)
                        this.Item.Cache.SetStatus(upd, PXEntryStatus.Updated);
					this.VendorItems.View.RequestRefresh();
				}

			if (!cache.ObjectsEqual<POVendorInventory.pendingDate>(e.Row, e.OldRow) ||
				!cache.ObjectsEqual<POVendorInventory.pendingPrice>(e.Row, e.OldRow) ||
				!cache.ObjectsEqual<POVendorInventory.effPrice>(e.Row, e.OldRow))
			{
				APSalesPrice sp = PXSelect<APSalesPrice,
					Where<APSalesPrice.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
					And<APSalesPrice.vendorID, Equal<Required<POVendorInventory.vendorID>>,
					And<APSalesPrice.vendorLocationID, Equal<Required<POVendorInventory.vendorLocationID>>,
					And<APSalesPrice.uOM, Equal<Required<POVendorInventory.purchaseUnit>>,
					And<APSalesPrice.curyID, Equal<Required<POVendorInventory.curyID>>,
					And<APSalesPrice.subItemID, Equal<Required<POVendorInventory.subItemID>>,
					And<APSalesPrice.breakQty, Equal<decimal0>,
					And<APSalesPrice.pendingBreakQty, Equal<decimal0>,
					And<APSalesPrice.isPromotionalPrice, Equal<boolFalse>>>>>>>>>>>.Select(this, current.InventoryID, current.VendorID, current.VendorLocationID, current.PurchaseUnit, current.CuryID, current.SubItemID);

				if (sp != null)
				{
					APSalesPrice copy = PXCache<APSalesPrice>.CreateCopy(sp);
					copy.PendingPrice = current.PendingPrice;
					copy.EffectiveDate = current.PendingDate;
					copy.SalesPrice = current.EffPrice;
					copy.LastDate = current.EffDate;
					copy.LastPrice = current.LastPrice;

					cache.Graph.RowUpdated.RemoveHandler<APSalesPrice>(APSalesPrice_RowUpdated);
					try
					{
						VendorSalesPrice.Update(copy);
					}
					finally
					{
						cache.Graph.RowUpdated.AddHandler<APSalesPrice>(APSalesPrice_RowUpdated);
					}
				}
			}
		}

        protected virtual void POVendorInventory_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            InventoryItem upd = PXCache<InventoryItem>.CreateCopy(this.Item.Current);
            POVendorInventory vendor = e.Row as POVendorInventory;
            object isdefault = cache.GetValueExt<POVendorInventory.isDefault>(e.Row);
            if (isdefault is PXFieldState)
            {
                isdefault = ((PXFieldState)isdefault).Value;
            }
			if ((bool?)isdefault == true)
			{
				upd.PreferredVendorID = null;
				upd.PreferredVendorLocationID = null;
				this.Item.Update(upd);
			}
			APSalesPrice sp = getAPSalesPrice(vendor);
			if (sp != null)
			{
				VendorSalesPrice.Delete(sp);
			}	
		}

		protected virtual void InventoryItem_ValMethod_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (((InventoryItem)e.Row).ValMethod != null && string.Equals(((InventoryItem)e.Row).ValMethod, (string)e.NewValue) == false)
			{
				INCostStatus coststatus = (INCostStatus)PXSelect<INCostStatus, Where<INCostStatus.inventoryID, Equal<Current<InventoryItem.inventoryID>>, And<INCostStatus.qtyOnHand, NotEqual<decimal0>>>>.Select(this);

				if (coststatus != null)
				{
					if (((InventoryItem)e.Row).ValMethod == INValMethod.Specific ||
							(string)e.NewValue == INValMethod.Specific ||
							(string)e.NewValue == INValMethod.Standard ||
                            ((InventoryItem)e.Row).ValMethod == INValMethod.FIFO)
					{
                        List<PXEventSubscriberAttribute> attrlist = sender.GetAttributesReadonly(e.Row, "ValMethod");
                        PXStringListAttribute listattr = (PXStringListAttribute)attrlist.Find(
                            (PXEventSubscriberAttribute attr) => { return attr is INValMethod.ListAttribute; });
                        string oldval, newval;
                        listattr.ValueLabelDic.TryGetValue(((InventoryItem)e.Row).ValMethod, out oldval);
                        listattr.ValueLabelDic.TryGetValue((string)e.NewValue, out newval);
						throw new PXSetPropertyException(string.Format(Messages.ValMethodCannotBeChanged, oldval, newval));
					}					
					sender.RaiseExceptionHandling<InventoryItem.valMethod>
							(e.Row, e.NewValue, new PXSetPropertyException(Messages.ValMethodChanged, PXErrorLevel.Warning));
				}
			}
		}

		protected virtual void InventoryItem_IsSplitted_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			InventoryItem row = e.Row as InventoryItem;
			if (row != null)
			{
				if (row.IsSplitted == false)
				{
					foreach (INComponent c in Components.Select())
					{
						Components.Delete(c);
					}

					row.TotalPercentage = 100;
				}
				else
					row.TotalPercentage = 0;
			}
		}

		protected virtual void InventoryItem_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			this.RowDeleting.RemoveHandler<ARSalesPrice>(ARSalesPrice_RowDeleting);
		}

		protected virtual void InventoryItem_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (e.Row == null) { return; }

			InventoryItem ii_rec = (InventoryItem)e.Row;

			// deleting only inventory-specific uoms
			foreach (INUnit inunit_rec in PXSelect<INUnit, Where<INUnit.unitType, Equal<short1>, And<INUnit.inventoryID, Equal<Required<INUnit.inventoryID>>>>>.Select(this, ii_rec.InventoryID))
			{
				itemunits.Delete(inunit_rec);
			}
		}

		protected virtual void InventoryItem_PackageOption_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			InventoryItem row = e.Row as InventoryItem;
			if (row == null) return;

			if (e.NewValue.ToString() == INPackageOption.Quantity &&  Boxes.Select().Count == 0)
			{
				sender.RaiseExceptionHandling<InventoryItem.packageOption>(row, e.NewValue,
				                                   new PXSetPropertyException(Messages.BoxesRequired, PXErrorLevel.Warning));
			}
			
		}

		protected virtual void InventoryItem_PackageOption_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			InventoryItem row = e.Row as InventoryItem;
			if (row == null) return;

			if (row.PackageOption == INPackageOption.Quantity)
			{
				row.PackSeparately = true;
			}
			else if (row.PackageOption == INPackageOption.Manual)
			{
				row.PackSeparately = false;

				foreach (INItemBoxEx box in Boxes.Select())
				{
					Boxes.Delete(box);
				}
			}
			else if (row.PackageOption == INPackageOption.WeightAndVolume)
			{
				row.PackSeparately = false;
			}
		}

		#endregion

		#region INItemXRef Event Handlers

		protected virtual void INItemXRef_InventoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Item.Current != null)
			{
				e.NewValue = Item.Current.InventoryID;
				e.Cancel = true;
			}
		}

		protected virtual void INItemXRef_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void INItemXRef_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				if (((INItemXRef)e.Row).AlternateType != INAlternateType.VPN && ((INItemXRef)e.Row).AlternateType != INAlternateType.CPN)
				{
					((INItemXRef)e.Row).BAccountID = (int)0;
				}
				((INItemXRef)e.Row).InventoryID = Item.Current.InventoryID;
				sender.Normalize();
			}
		}
		
		protected virtual void INItemXRef_BAccountID_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null && (int?)e.ReturnValue == 0 && ((INItemXRef)e.Row).AlternateType != INAlternateType.VPN && ((INItemXRef)e.Row).AlternateType != INAlternateType.CPN)
			{
				e.ReturnValue = null;
				e.Cancel = true;
			}
		}
		
		protected virtual void INItemXRef_BAccountID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row != null && e.NewValue == null && ((INItemXRef)e.Row).AlternateType != INAlternateType.VPN && ((INItemXRef)e.Row).AlternateType != INAlternateType.CPN)
			{
				e.NewValue = (int)0;
				e.Cancel = true;
			}
		}
		
		protected virtual void INItemXRef_BAccountID_ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e)
		{
			if (e.Row != null && ((INItemXRef)e.Row).BAccountID == null && e.NewValue is int && ((int)e.NewValue) == 0)
			{
				((INItemXRef)e.Row).BAccountID = 0;
				e.Cancel = true;
			}
		}

		protected virtual void INItemXRef_BAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (((INItemXRef)e.Row).AlternateType != INAlternateType.VPN && ((INItemXRef)e.Row).AlternateType != INAlternateType.CPN)
			{
				e.Cancel = true;
			}		
		}		

		#endregion

		#region INItemSite Event Handlers

		protected virtual void INItemSite_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if ((bool)((INItemSite)e.Row).IsDefault && ((INItemSite)e.NewRow).IsDefault == false)
			{
				((INItemSite)e.NewRow).IsDefault = true;
			}
		}

		protected virtual void INItemSite_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if ((bool)((INItemSite)e.Row).IsDefault)
			{
				SetSiteDefault(sender, e);
			}

			INItemSite row = e.Row as INItemSite;

			if (row != null && insetup.Current.UseInventorySubItem != true)
			{
				PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus sitem = new Overrides.INDocumentRelease.SiteStatus();
				sitem.InventoryID = row.InventoryID;
				sitem.SiteID = row.SiteID;
				sitestatus.Insert(sitem);
			}
		}

		protected virtual void INItemSite_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (((INItemSite)e.OldRow).IsDefault == false && (bool)((INItemSite)e.Row).IsDefault)
			{
				SetSiteDefault(sender, e);
			}
		}

		protected virtual void INItemSite_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INItemSite row = (INItemSite)e.Row;
			if (e.Row != null)
			{
				bool isTransfer = (row != null) && INReplenishmentSource.IsTransfer(row.ReplenishmentSource);
				if (isTransfer && row.ReplenishmentSourceSiteID == row.SiteID)
				{
					sender.RaiseExceptionHandling<INItemSite.replenishmentSourceSiteID>(e.Row, row.ReplenishmentSourceSiteID, new PXSetPropertyException(Messages.ReplenishmentSourceSiteMustBeDifferentFromCurrenSite, PXErrorLevel.Warning));
				}
				else
				{
					sender.RaiseExceptionHandling<INItemSite.replenishmentSourceSiteID>(e.Row, row.ReplenishmentSourceSiteID, null);
				}
			}
		}


		#endregion

		#region RelationGroup Event Handlers

		protected virtual void RelationGroup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PX.SM.RelationGroup group = e.Row as PX.SM.RelationGroup;
			if (Item.Current != null && group != null && Groups.Cache.GetStatus(group) == PXEntryStatus.Notchanged)
			{
				group.Included = PX.SM.UserAccess.IsIncluded(Item.Current.GroupMask, group);
			}
		}

		protected virtual void RelationGroup_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		#endregion

		#region INComponent Event Handles

		protected virtual void INComponent_Percentage_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			INComponent row = e.Row as INComponent;
			if (row != null && row.AmtOption == INAmountOption.Percentage)
			{
				row.Percentage = GetRemainingPercentage();
			}
		}

		protected virtual void INComponent_ComponentID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INComponent row = e.Row as INComponent;
			if (row != null)
			{
				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, row.ComponentID);
				if (item != null)
				{
					row.SalesAcctID = item.SalesAcctID;
					row.SalesSubID = item.SalesSubID;
					row.UOM = item.SalesUnit;
					row.DeferredCode = item.DeferredCode;
				}
			}
		}

		protected virtual void INComponent_AmtOption_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INComponent row = e.Row as INComponent;
			if (row != null)
			{
				if (row.AmtOption == INAmountOption.Percentage)
				{
					row.FixedAmt = null;
					row.Percentage = GetRemainingPercentage();
				}
				else
					row.Percentage = 0;
			}
		}

		#endregion
		
		#region INItemRep Event Handler
		protected virtual void INItemRep_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INItemRep row = e.Row as INItemRep;
			if (row != null)
			{
				bool isTransfer = INReplenishmentSource.IsTransfer(row.ReplenishmentSource);
				PXUIFieldAttribute.SetEnabled<INItemRep.replenishmentSourceSiteID>(sender, e.Row, isTransfer);
				PXUIFieldAttribute.SetEnabled<INItemRep.transferERQ>(sender, e.Row, isTransfer && row.ReplenishmentMethod == INReplenishmentMethod.FixedReorder);
				PXUIFieldAttribute.SetEnabled<INSubItemRep.transferERQ>(this.subreplenishment.Cache, null, isTransfer && row.ReplenishmentMethod == INReplenishmentMethod.FixedReorder);
			}
			this.subreplenishment.Cache.AllowInsert =
					e.Row != null && (string.IsNullOrEmpty(row.ReplenishmentClassID) == false) && insetup.Current.UseInventorySubItem == true;
		}

		protected virtual void INItemRep_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			INItemRep r = e.Row as INItemRep;
			if(r != null && r.ReplenishmentClassID != null)
				UpdateItemSiteReplenishment(r);
		}

		protected virtual void INItemRep_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			INItemRep r = e.Row as INItemRep;
			if (r == null) return;
			if (INReplenishmentSource.IsTransfer(r.ReplenishmentSource) == false)
			{
				r.ReplenishmentSourceSiteID = null;
			}
			UpdateItemSiteReplenishment(r);

		}
		protected virtual void INItemRep_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			INItemRep r = e.Row as INItemRep;
			if (r == null) return;
			INItemRep def = new INItemRep();
			def.ReplenishmentClassID = r.ReplenishmentClassID;
			UpdateItemSiteReplenishment(def);
		}
		private void UpdateItemSiteReplenishment(INItemRep r)
		{
			foreach (PXResult<INItemSite, INSite> rec in itemsiterecords.Select())
			{
				INItemSite itemsite = rec;
				INSite site = rec;
				if (itemsite.ReplenishmentPolicyOverride == true ||
					(itemsite.ReplenishmentClassID == site.ReplenishmentClassID &&
					 itemsite.ReplenishmentClassID != r.ReplenishmentClassID)) continue;

				bool IsUpdateFlag = false;
				if (itemsite.ReplenishmentPolicyOverride == false)
				{
					itemsite.ReplenishmentClassID = r.ReplenishmentClassID;
					itemsite.ReplenishmentPolicyID = r.ReplenishmentPolicyID;
					itemsite.ReplenishmentSource = r.ReplenishmentSource;
					itemsite.ReplenishmentSourceSiteID = r.ReplenishmentSourceSiteID;
					itemsite.ReplenishmentMethod = r.ReplenishmentMethod;
					IsUpdateFlag = true;
				}
				if (itemsite.MaxShelfLifeOverride == false || itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
				{ itemsite.MaxShelfLife = r.MaxShelfLife; IsUpdateFlag = true; }
				if (itemsite.LaunchDateOverride == false || itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
				{ itemsite.LaunchDate = r.LaunchDate; IsUpdateFlag = true; }
				if (itemsite.TerminationDateOverride == false || itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
				{ itemsite.TerminationDate = r.TerminationDate; IsUpdateFlag = true; }
				if (itemsite.SafetyStockOverride == false || itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
				{ itemsite.SafetyStock = r.SafetyStock; IsUpdateFlag = true; }
				if (itemsite.MinQtyOverride == false || itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
				{ itemsite.MinQty = r.MinQty; IsUpdateFlag = true; }
				if (itemsite.MaxQtyOverride == false || itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
				{ itemsite.MaxQty = r.MaxQty; IsUpdateFlag = true; }
				if (itemsite.TransferERQOverride == false || itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
				{ itemsite.TransferERQ = r.TransferERQ; IsUpdateFlag = true; }

				if (IsUpdateFlag && itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Notchanged)
				{
					itemsiterecords.Cache.SetStatus(itemsite, PXEntryStatus.Updated);
				}
			}
		}
		#endregion

		protected virtual void INItemBoxEx_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INItemBoxEx row = e.Row as INItemBoxEx;
			if (row == null) return;

			if (Item.Current == null) return;
			
			if (Item.Current.PackageOption == INPackageOption.Weight || Item.Current.PackageOption == INPackageOption.WeightAndVolume)
			{
				row.MaxQty = CalculateMaxQtyInBox(Item.Current, row);
			}
		}

		protected virtual void INItemBoxEx_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			INItemBoxEx row = e.Row as INItemBoxEx;
			if (row == null) return;

			CSBox box = PXSelect<CSBox, Where<CSBox.boxID, Equal<Current<INItemBoxEx.boxID>>>>.Select(this);
			if (box != null)
			{
				row.MaxWeight = box.MaxWeight;
				row.MaxVolume = box.MaxVolume;
				row.BoxWeight = box.BoxWeight;
				row.Description = box.Description;
			}

			if (Item.Current.PackageOption == INPackageOption.Weight || Item.Current.PackageOption == INPackageOption.WeightAndVolume)
			{
				row.MaxQty = CalculateMaxQtyInBox(Item.Current, row);
			}
		}



		private decimal GetRemainingPercentage()
		{
			decimal result = 100;

			foreach (INComponent comp in Components.Select())
			{
				if (comp.AmtOption == INAmountOption.Percentage)
				result -= (comp.Percentage ?? 0);
			}

			if (result > 0)
				return result;
			else
				return 0;
		}

		private decimal SumComponentsPercentage()
		{
			decimal result = 0;

			foreach (INComponent comp in Components.Select())
			{
				if (comp.AmtOption == INAmountOption.Percentage)
				result += (comp.Percentage ?? 0);
			}

			return result;
		}

		protected virtual void AppendGroupMask(string itemClassID, bool clear)
		{
			if (!String.IsNullOrEmpty(itemClassID))
			{
				INItemClass ic = PXSelect<INItemClass,
					Where<INItemClass.itemClassID, Equal<Required<INItemClass.itemClassID>>>>
					.Select(this, itemClassID);
				if (ic != null && ic.GroupMask != null)
				{
					if (clear)
					{
						Groups.Cache.Clear();
					}
					foreach (PX.SM.RelationGroup group in Groups.Select())
					{
						for (int i = 0; i < group.GroupMask.Length && i < ic.GroupMask.Length; i++)
						{
							if (group.Included != true && group.GroupMask[i] != 0x00 && (ic.GroupMask[i] & group.GroupMask[i]) == group.GroupMask[i])
							{
								group.Included = true;
								if (Groups.Cache.GetStatus(group) == PXEntryStatus.Notchanged || Groups.Cache.GetStatus(group) == PXEntryStatus.Held)
								{
									Groups.Cache.SetStatus(group, PXEntryStatus.Updated);
								}
								Groups.Cache.IsDirty = true;
								break;
							}
						}
					}
				}
			}
		}

		protected bool _JustInserted;
		public override bool IsDirty
		{
			get
			{
				if (_JustInserted)
				{
					return false;
				}
				return base.IsDirty;
			}
		}

		protected virtual void SetSiteDefault(PXCache sender, PXRowUpdatedEventArgs e)
		{
			InventoryItem item = (InventoryItem)PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, ((INItemSite)e.Row).InventoryID);

			if (item != null)
			{
				item.DfltSiteID = ((INItemSite)e.Row).SiteID;
				item.DfltReceiptLocationID = ((INItemSite)e.Row).DfltReceiptLocationID;
				item.DfltShipLocationID = ((INItemSite)e.Row).DfltShipLocationID;

				if (Item.Cache.GetStatus(item) == PXEntryStatus.Notchanged)
				{
					Item.Cache.SetStatus(item, PXEntryStatus.Updated);
				}
			}

			bool IsRefreshNeeded = false;

			foreach (INItemSite rec in itemsiterecords.Select())
			{
				if (object.Equals(rec.SiteID, ((INItemSite)e.Row).SiteID) == false && (bool)rec.IsDefault)
				{
					rec.IsDefault = false;
					if (itemsiterecords.Cache.GetStatus(rec) == PXEntryStatus.Notchanged)
					{
					itemsiterecords.Cache.SetStatus(rec, PXEntryStatus.Updated);
					}

					IsRefreshNeeded = true;
				}
			}

			if (IsRefreshNeeded)
			{
				itemsiterecords.View.RequestRefresh();
			}
		}

		protected virtual void SetSiteDefault(PXCache sender, PXRowInsertedEventArgs e)
		{
			SetSiteDefault(sender, new PXRowUpdatedEventArgs(e.Row, null, e.ExternalCall));
		}

		public override void Persist()
		{
			if (Item.Current != null && Groups.Cache.IsDirty)
			{
				PX.SM.UserAccess.PopulateNeighbours<InventoryItem>(Item.Cache, Item.Current,
					new PXDataFieldValue[] {
						new PXDataFieldValue(typeof(InventoryItem.inventoryID).Name, PXDbType.Int, 4, Item.Current.InventoryID, PXComp.NE),
						new PXDataFieldValue(typeof(InventoryItem.stkItem).Name, PXDbType.Bit, 1, 1, PXComp.EQ)
					},
					Groups,
					typeof(SegmentValue));
				PXSelectorAttribute.ClearGlobalCache<InventoryItem>();
			}

			foreach (ARSalesPrice price in ARSalesPrices.Cache.Inserted)
			{
				ARSalesPriceMaint.ValidateDuplicate(this, ARSalesPrices.Cache, price);
			}
			foreach (ARSalesPrice price in ARSalesPrices.Cache.Updated)
			{
				ARSalesPriceMaint.ValidateDuplicate(this, ARSalesPrices.Cache, price);
			}
			foreach (ARSalesPriceEx price in CustomerSalesPrice.Cache.Inserted)
			{
				ValidateDuplicateCustomerPrice(this, CustomerSalesPrice.Cache, price);
			}
			foreach (ARSalesPriceEx price in CustomerSalesPrice.Cache.Updated)
			{
				ValidateDuplicateCustomerPrice(this, CustomerSalesPrice.Cache, price);
			}
			foreach (APSalesPrice price in VendorSalesPrice.Cache.Inserted)
			{
				APVendorSalesPriceMaint.ValidateDuplicate(this, VendorSalesPrice.Cache, price);
				if (price.IsPromotionalPrice == false && price.PendingBreakQty == 0 && price.BreakQty == 0)
					InsertPOVendorInventory(price);
			}
			foreach (APSalesPrice price in VendorSalesPrice.Cache.Updated)
			{
				APVendorSalesPriceMaint.ValidateDuplicate(this, VendorSalesPrice.Cache, price);
			}

			foreach (InventoryItem item in Item.Cache.Inserted)
			{
				string uom = SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit ? item.BaseUnit : item.SalesUnit;
				if (uom != null)
				{
					InsertSalesPrice(Item.Cache, item, uom);
				}
			}

			foreach (POVendorInventory vi in VendorItems.Cache.Inserted)
			{
				InsertAPVendorPrice(vi);
			}

			base.Persist();
			Groups.Cache.Clear();
			GroupHelper.Clear();
		}

		public static void ValidateDuplicateCustomerPrice(PXGraph graph, PXCache sender, ARSalesPriceEx price)
		{
			PXSelectBase<ARSalesPriceEx> selectDuplicate = new PXSelect<ARSalesPriceEx, Where<ARSalesPriceEx.curyID, Equal<Required<ARSalesPriceEx.curyID>>, And<ARSalesPriceEx.customerID, Equal<Required<ARSalesPriceEx.customerID>>, And<ARSalesPriceEx.inventoryID, Equal<Required<ARSalesPriceEx.inventoryID>>, And<ARSalesPriceEx.uOM, Equal<Required<ARSalesPriceEx.uOM>>, And<ARSalesPriceEx.recordID, NotEqual<Required<ARSalesPriceEx.recordID>>, And<ARSalesPriceEx.pendingBreakQty, Equal<Required<ARSalesPriceEx.pendingBreakQty>>, And<ARSalesPriceEx.breakQty, Equal<Required<ARSalesPriceEx.breakQty>>, And<ARSalesPriceEx.isPromotionalPrice, Equal<Required<ARSalesPriceEx.isPromotionalPrice>>>>>>>>>>>(graph);
			ARSalesPriceEx duplicate;
			if (price.IsPromotionalPrice == true)
			{
				selectDuplicate.WhereAnd<Where2<Where<Required<ARSalesPriceEx.lastDate>, Between<ARSalesPriceEx.lastDate, ARSalesPriceEx.expirationDate>>, Or<Required<ARSalesPriceEx.expirationDate>, Between<ARSalesPriceEx.lastDate, ARSalesPriceEx.expirationDate>, Or<ARSalesPriceEx.lastDate, Between<Required<ARSalesPriceEx.lastDate>, Required<ARSalesPriceEx.expirationDate>>, Or<ARSalesPriceEx.expirationDate, Between<Required<ARSalesPriceEx.lastDate>, Required<ARSalesPriceEx.expirationDate>>>>>>>();
				duplicate = selectDuplicate.SelectSingle(price.CuryID, price.CustomerID, price.InventoryID, price.UOM, price.RecordID, price.PendingBreakQty, price.BreakQty, price.IsPromotionalPrice, price.LastDate, price.ExpirationDate, price.LastDate, price.ExpirationDate, price.LastDate, price.ExpirationDate);
			}
			else
			{
				duplicate = selectDuplicate.SelectSingle(price.CuryID, price.CustomerID, price.InventoryID, price.UOM, price.RecordID, price.PendingBreakQty, price.BreakQty, price.IsPromotionalPrice);
			}
			if (duplicate != null)
			{
				sender.RaiseExceptionHandling<ARSalesPriceEx.uOM>(price, price.UOM, new PXSetPropertyException(SO.Messages.DuplicateSalesPrice, PXErrorLevel.RowError));
			}
		}

		protected virtual void ValidatePackaging(InventoryItem row)
		{
			PXUIFieldAttribute.SetError<InventoryItem.weightUOM>(Item.Cache, row, null);
			PXUIFieldAttribute.SetError<InventoryItem.baseItemWeight>(Item.Cache, row, null);
			PXUIFieldAttribute.SetError<InventoryItem.volumeUOM>(Item.Cache, row, null);
			PXUIFieldAttribute.SetError<InventoryItem.baseItemVolume>(Item.Cache, row, null);
			
			//validate weight & volume:
			if (row.PackageOption == INPackageOption.Weight || row.PackageOption == INPackageOption.WeightAndVolume)
			{
				if (string.IsNullOrEmpty(row.WeightUOM))
					Item.Cache.RaiseExceptionHandling<InventoryItem.weightUOM>(row, row.WeightUOM, new PXSetPropertyException(Messages.ValueIsRequiredForAutoPackage, PXErrorLevel.Warning));

				if (row.BaseItemWeight <= 0)
					Item.Cache.RaiseExceptionHandling<InventoryItem.baseItemWeight>(row, row.BaseItemWeight, new PXSetPropertyException(Messages.ValueIsRequiredForAutoPackage, PXErrorLevel.Warning));

				if (row.PackageOption == INPackageOption.WeightAndVolume)
				{
					if (string.IsNullOrEmpty(row.VolumeUOM))
						Item.Cache.RaiseExceptionHandling<InventoryItem.volumeUOM>(row, row.VolumeUOM, new PXSetPropertyException(Messages.ValueIsRequiredForAutoPackage, PXErrorLevel.Warning));

					if (row.BaseItemVolume <= 0)
						Item.Cache.RaiseExceptionHandling<InventoryItem.baseItemVolume>(row, row.BaseItemVolume, new PXSetPropertyException(Messages.ValueIsRequiredForAutoPackage, PXErrorLevel.Warning));
				}
			}

			//validate boxes:
			foreach (INItemBoxEx box in Boxes.Select())
			{
				PXUIFieldAttribute.SetError<INItemBoxEx.boxID>(Boxes.Cache, box, null);
				PXUIFieldAttribute.SetError<INItemBoxEx.maxQty>(Boxes.Cache, box, null);

				if ((row.PackageOption == INPackageOption.Weight || row.PackageOption == INPackageOption.WeightAndVolume) && box.MaxWeight.GetValueOrDefault() == 0)
				{
					Boxes.Cache.RaiseExceptionHandling<INItemBoxEx.boxID>(box, box.BoxID, new PXSetPropertyException(Messages.MaxWeightIsNotDefined, PXErrorLevel.Warning));
				}

				if (row.PackageOption == INPackageOption.WeightAndVolume && box.MaxVolume.GetValueOrDefault() == 0)
				{
					Boxes.Cache.RaiseExceptionHandling<INItemBoxEx.boxID>(box, box.BoxID, new PXSetPropertyException(Messages.MaxVolumeIsNotDefined, PXErrorLevel.Warning));
				}

				if ((row.PackageOption == INPackageOption.Weight || row.PackageOption == INPackageOption.WeightAndVolume) && 
					(box.MaxWeight.GetValueOrDefault() < row.BaseItemWeight.GetValueOrDefault() ||
					(box.MaxVolume > 0 && row.BaseItemVolume > box.MaxVolume)))
				{
					Boxes.Cache.RaiseExceptionHandling<INItemBoxEx.boxID>(box, box.BoxID, new PXSetPropertyException(Messages.ItemDontFitInTheBox, PXErrorLevel.Warning));
				}

			}
		}

		#region Actions

		public PXAction<InventoryItem> action;
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual IEnumerable Action(PXAdapter adapter,
			[PXInt]
			[PXIntList(new int[] { 1, 2, 3 }, new string[] 
			{ 
					"Update Price",
					"Update Cost",
                    "View Restriction Group"                    

			})]
			int? actionID
			)
		{
			switch (actionID)
			{
				case 1:
					if (ItemSettings.Current != null && ItemSettings.Current.PendingBasePriceDate != null)
					{
						ItemSettings.Current.LastBasePrice = ItemSettings.Current.BasePrice;
						ItemSettings.Current.BasePrice = ItemSettings.Current.PendingBasePrice;
						ItemSettings.Current.BasePriceDate = ItemSettings.Current.PendingBasePriceDate ?? new DateTime(1900,1,1);
						ItemSettings.Current.PendingBasePrice = 0;
						ItemSettings.Current.PendingBasePriceDate = null;

						InsertSalesPrice(Item.Cache, ItemSettings.Current, SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit ? ItemSettings.Current.BaseUnit : ItemSettings.Current.SalesUnit);
						ItemSettings.Update(ItemSettings.Current);

						this.Save.Press();
					}
					break;
				case 2:
					if (ItemSettings.Current != null && ItemSettings.Current.PendingStdCostDate != null)
					{
						if (ItemSettings.Current.ValMethod == INValMethod.Standard)
						{
							INCostStatus layer = PXSelect<INCostStatus, Where<INCostStatus.inventoryID, Equal<Current<InventoryItem.inventoryID>>,
								And<INCostStatus.qtyOnHand, NotEqual<decimal0>>>>.SelectWindowed(this, 0, 1);

							if (layer == null)
							{
								InventoryItem old_row = PXCache<InventoryItem>.CreateCopy(ItemSettings.Current);

								ItemSettings.Current.LastStdCost = ItemSettings.Current.StdCost;
								ItemSettings.Current.StdCostDate = ItemSettings.Current.PendingStdCostDate.GetValueOrDefault((DateTime)this.Accessinfo.BusinessDate);
								ItemSettings.Current.StdCost = ItemSettings.Current.PendingStdCost;
								ItemSettings.Current.PendingStdCost = 0;
								ItemSettings.Current.PendingStdCostDate = null;

								ItemSettings.Cache.RaiseRowUpdated(ItemSettings.Current, old_row);

								this.Save.Press();
							}
							else
							{
								throw new PXException(Messages.QtyOnHandExists);
							}
						}
					}
					break;
				case 3:
					if (Item.Current != null)
					{
						INAccessDetailByItem graph = CreateInstance<INAccessDetailByItem>();
						graph.Item.Current = graph.Item.Search<InventoryItem.inventoryCD>(Item.Current.InventoryCD);
						throw new PXRedirectRequiredException(graph, false, "Restricted Groups");
					}
					break;
			}
			return adapter.Get();
		}


		public PXAction<InventoryItem> inquiry;
		[PXUIField(DisplayName = "Inquiries", MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual IEnumerable Inquiry(PXAdapter adapter,
			[PXInt]
			[PXIntList(new int[] { 1, 2, 3, 4, 5 }, new string[] 
			{ 
					"Summary", 
					"Allocation Details", 
					"Transaction Summary", 
					"Transaction Details", 
					"Transaction History" 
			})]
			int? inquiryID
			)
		{
			switch (inquiryID)
			{
				case 1:
					if (Item.Current != null)
					{
						InventorySummaryEnq graph = PXGraph.CreateInstance<InventorySummaryEnq>();
						graph.Filter.Current.InventoryID = Item.Current.InventoryID;
						graph.Filter.Select();
						throw new PXRedirectRequiredException(graph, "Inventory Summary");
					}
					break;
				case 2:
					if (Item.Current != null)
					{
						InventoryAllocDetEnq graph = PXGraph.CreateInstance<InventoryAllocDetEnq>();
						graph.Filter.Current.InventoryID = Item.Current.InventoryID;
						graph.Filter.Select();
						throw new PXRedirectRequiredException(graph, "Inventory Allocation Details");
					}
					break;
				case 3:
					if (Item.Current != null)
					{
						InventoryTranSumEnq graph = PXGraph.CreateInstance<InventoryTranSumEnq>();
						graph.Filter.Current.InventoryID = Item.Current.InventoryID;
						graph.Filter.Select();
						throw new PXRedirectRequiredException(graph, "Inventory Transaction Summary");
					}
					break;
				case 4:
					if (Item.Current != null)
					{
						InventoryTranDetEnq graph = PXGraph.CreateInstance<InventoryTranDetEnq>();
						graph.Filter.Current.InventoryID = Item.Current.InventoryID;
						graph.Filter.Select();
						throw new PXRedirectRequiredException(graph, "Inventory Transaction Details");
					}
					break;
				case 5:
					if (Item.Current != null)
					{
						InventoryTranHistEnq graph = PXGraph.CreateInstance<InventoryTranHistEnq>();
						graph.Filter.Current.InventoryID = Item.Current.InventoryID;
						graph.Filter.Select();
						throw new PXRedirectRequiredException(graph, "Inventory Transaction History");
					}
					break;

			}
			return adapter.Get();
		}

		public PXAction<InventoryItem> addWarehouseDetail;
		[PXUIField(DisplayName = "Add Warehouse Detail", MapEnableRights = PXCacheRights.Select)]
		[PXInsertButton]
		protected virtual IEnumerable AddWarehouseDetail(PXAdapter adapter)
		{
			foreach(InventoryItem item in adapter.Get())
			{
				if (item.InventoryID > 0)
				{
					INItemSiteMaint maint = PXGraph.CreateInstance<INItemSiteMaint>();
					PXCache cache = maint.itemsiterecord.Cache;
					IN.INItemSite rec = (IN.INItemSite)cache.CreateCopy(cache.Insert());
					rec.InventoryID = item.InventoryID;
					cache.Update(rec);
					cache.IsDirty = false;
					throw new PXRedirectRequiredException(maint, "Add Warehouse Detail");
				}
				yield return item;
			}
		}
		public PXAction<InventoryItem> updateReplenishment;
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		[PXUIField(DisplayName = "Reset to Default", MapEnableRights = PXCacheRights.Update)]
		protected virtual IEnumerable UpdateReplenishment(PXAdapter adapter)
		{
			if (this.replenishment.Current != null && insetup.Current.UseInventorySubItem == true) 
				foreach(INSubItemRep rep in this.subreplenishment.Select())
				{
					INSubItemRep upd = PXCache<INSubItemRep>.CreateCopy(rep);
					upd.SafetyStock = this.replenishment.Current.SafetyStock;
					upd.MinQty = this.replenishment.Current.MinQty;
					upd.MaxQty = this.replenishment.Current.MaxQty;
					this.subreplenishment.Update(upd);
				}				
			return adapter.Get();			
		}

		public PXAction<InventoryItem> generateSubitems;
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
		[PXUIField(DisplayName = "Generate Subitems", MapEnableRights = PXCacheRights.Update)]
		protected virtual IEnumerable GenerateSubitems(PXAdapter adapter)
		{
			if (this.replenishment.Current != null && insetup.Current.UseInventorySubItem == true)
			{
				PXResultset<Segment> r = PXSelect<Segment,
					Where<Segment.dimensionID, Equal<Required<Segment.dimensionID>>>>
					.Select(this, SubItemAttribute.DimensionName);
				List<string>[] values = new List<string>[r.Count];				
				int segments = 0;
				int counts = 0;
				foreach(Segment s in r)
				{					
					values[segments] = new List<string>();
					foreach(SegmentValue v in PXSelect<SegmentValue,
						Where<SegmentValue.dimensionID, Equal<Required<SegmentValue.dimensionID>>,
							And<SegmentValue.segmentID, Equal<Required<SegmentValue.segmentID>>>>>
							.Select(this, s.DimensionID, s.SegmentID))
					{
						values[segments].Add(v.Value);						
						counts += 1;
					}					
					segments += 1;					
				}
				int[] index = new int[segments];				

				for(int c=0 ;c<counts; c++)
				{
					StringBuilder subitem = new StringBuilder(); 
					for (int i = 0; i < segments; i++)
					{
						subitem.Append(values[i][index[i]]);
					}
					for(int k=segments-1;k>=0;k--)
					{
						index[k] += 1;
						if(index[k] < values[k].Count)
							break;
						index[k] = 0;
					}
					INSubItemRep ins = new INSubItemRep();
					ins.InventoryID = this.Item.Current.InventoryID;
					ins.ReplenishmentClassID = this.replenishment.Current.ReplenishmentClassID;
					this.subreplenishment.SetValueExt<INSubItemRep.subItemID>(ins, subitem.ToString());
					this.subreplenishment.Insert(ins);
				}				
			}
			return adapter.Get();
		}

		public PXAction<InventoryItem> generateLotSerialNumber;
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
		[PXUIField(DisplayName = "Generate Lot/Serial Number", Visible = false)]
		protected virtual IEnumerable GenerateLotSerialNumber(PXAdapter adapter)
		{
			foreach (InventoryItem item in adapter.Get())
			{
				item.LotSerNumberResult = INLotSerialNbrAttribute.GetNextNumber(adapter.View.Cache, item.InventoryID.GetValueOrDefault());
				yield return item;
			}
		}
		#endregion

		public static void Redirect(int? inventoryID)
		{
			Redirect(inventoryID, false);
		}

		public static void Redirect(int? inventoryID, bool newWindow)
		{
			InventoryItemMaint graph = PXGraph.CreateInstance<InventoryItemMaint>();
			graph.Item.Current = graph.Item.Search<InventoryItem.inventoryID>(inventoryID);
			if (graph.Item.Current != null)
			{
				if(newWindow)
					throw new PXRedirectRequiredException(graph, true, Messages.InventoryItem){Mode = PXBaseRedirectException.WindowMode.NewWindow};
				else
					throw new PXRedirectRequiredException(graph, Messages.InventoryItem);
			}
		}

		#region Private members
		private bool doResetDefaultsOnItemClassChange;
		#endregion

		private string SalesPriceUpdateUnit
		{
			get
			{
				SOSetup sosetup = PXSelect<SOSetup>.Select(this);
				if (sosetup != null && !string.IsNullOrEmpty(sosetup.SalesPriceUpdateUnit))
				{
					return sosetup.SalesPriceUpdateUnit;
				}

				return SalesPriceUpdateUnitType.BaseUnit;
			}
		}

        public PXAction<InventoryItem> viewGroupDetails;
        [PXUIField(DisplayName = Messages.ViewRestrictionGroup, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual IEnumerable ViewGroupDetails(PXAdapter adapter)
        {
            if (Groups.Current != null)
            {
				RelationGroups graph = CreateInstance<RelationGroups>();
                graph.HeaderGroup.Current = graph.HeaderGroup.Search<RelationHeader.groupName>(Groups.Current.GroupName);
                throw new PXRedirectRequiredException(graph, false, Messages.ViewRestrictionGroup);
            }
            return adapter.Get();
        }

		protected virtual decimal? CalculateMaxQtyInBox(InventoryItem item, INItemBoxEx box)
		{
			decimal? resultWeight = null;
			decimal? resultVolume = null;
			
			if (item.BaseWeight > 0)
			{
				if (box.MaxWeight > 0)
				{
					resultWeight = Math.Floor((box.MaxWeight.Value - box.BoxWeight.GetValueOrDefault()) / item.BaseWeight.Value);
				}
			}

			if (item.PackageOption == INPackageOption.Weight)
			{
				return resultWeight;
			}

			if (item.BaseVolume > 0)
			{
				if (box.MaxVolume > 0)
				{
					resultVolume = Math.Floor(box.MaxVolume.Value / item.BaseVolume.Value);
				}
			}

			
			if (resultWeight != null && resultVolume != null)
			{
				return Math.Min(resultWeight.Value, resultVolume.Value);
			}

			if (resultWeight != null)
				return resultWeight;

			if (resultVolume != null)
				return resultVolume;

			return null;
		}
	}

	public class NonStockItemMaint : PXGraph<NonStockItemMaint>
	{
		#region DAC Overrides
		


		#endregion


		public PXSelect<InventoryItem, Where<InventoryItem.stkItem, Equal<False>, And<Match<Current<AccessInfo.userName>>>>> Item;
		public PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<InventoryItem.inventoryID>>>> ItemSettings;
		public INUnitSelect<INUnit, InventoryItem.inventoryID, InventoryItem.itemClassID, InventoryItem.salesUnit, InventoryItem.purchaseUnit, InventoryItem.baseUnit, InventoryItem.lotSerClassID> itemunits;
		public PXSetupOptional<INSetup> insetup;
		public PXSetup<Company> Company;
		public PXSetupOptional<TX.TXAvalaraSetup> avalaraSetup; 
		public PXSelect<INComponent, Where<INComponent.inventoryID, Equal<Current<InventoryItem.inventoryID>>>> Components;
		public PXSelect<ARSalesPrice> SalesPrice;
		public PXSelect<INItemClass, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>> ItemClass;
		public POVendorInventorySelect<POVendorInventory,
				LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<POVendorInventory.inventoryID>>,
				LeftJoin<Vendor, On<Vendor.bAccountID, Equal<POVendorInventory.vendorID>>,
				LeftJoin<Location, On<Location.bAccountID, Equal<POVendorInventory.vendorID>,
							And<Location.locationID, Equal<POVendorInventory.vendorLocationID>>>>>>,
				Where<POVendorInventory.inventoryID, Equal<Current<InventoryItem.inventoryID>>>,
				InventoryItem> VendorItems;
		public CRAttributeList<InventoryItem> Answers;
        public PXSelect<INItemXRef, Where<INItemXRef.inventoryID, Equal<Current<InventoryItem.inventoryID>>>> itemxrefrecords;
        public PXSelect<GL.Branch, Where<GL.Branch.branchID, Equal<Required<GL.Branch.branchID>>>> CurrentBranch;

		public PXSelect<ARSalesPrice,
			Where<ARSalesPrice.inventoryID, Equal<Current<InventoryItem.inventoryID>>,
				And<ARSalesPrice.isCustClassPrice, Equal<boolTrue>>>,
			OrderBy<Asc<ARSalesPrice.custPriceClassID,
					Asc<ARSalesPrice.uOM,
					Asc<ARSalesPrice.curyID>>>>> ARSalesPrices;

		public PXSelectJoin<ARSalesPriceEx,
			LeftJoin<Customer, On<Customer.bAccountID, Equal<ARSalesPriceEx.customerID>>>,
			Where<ARSalesPriceEx.inventoryID, Equal<Current<InventoryItem.inventoryID>>,
			 And<ARSalesPriceEx.isCustClassPrice, Equal<boolFalse>>>,
			OrderBy<Asc<Customer.acctCD,
					Asc<ARSalesPriceEx.uOM,
					Asc<ARSalesPriceEx.curyID>>>>> CustomerSalesPrice;

		public PXSelectJoin<APSalesPrice,
			LeftJoin<Vendor, On<Vendor.bAccountID, Equal<APSalesPrice.vendorID>>>,
			Where<APSalesPrice.inventoryID, Equal<Current<InventoryItem.inventoryID>>>,
			OrderBy<Asc<Vendor.acctCD,
					Asc<APSalesPrice.uOM,
					Asc<APSalesPrice.curyID>>>>> VendorSalesPrice;

		public PXFilter<MassUpdateFilter> MassUpdateSettings;

		public NonStockItemMaint()
		{
			INSetup record = insetup.Current;

			PXUIFieldAttribute.SetVisible<INUnit.toUnit>(itemunits.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<INUnit.toUnit>(itemunits.Cache, null, false);

			PXUIFieldAttribute.SetVisible<INUnit.sampleToUnit>(itemunits.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<INUnit.sampleToUnit>(itemunits.Cache, null, false);
			FieldDefaulting.AddHandler<BAccountR.type>((sender, e) => { if (e.Row != null) e.NewValue = BAccountType.VendorType; });

			action.AddMenuAction(ChangeID);
		}
		#region Cache Attached
		#region INComponent
		[PXDBDefault(typeof(InventoryItem.inventoryID))]
		[PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<INComponent.inventoryID>>>>))]
		[PXDBInt(IsKey = true)]
		protected virtual void INComponent_InventoryID_CacheAttached(PXCache sender)
		{			
		}
		[PXDefault()]
		[Inventory(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr), Filterable = true, IsKey = true, DisplayName = "Inventory ID")]
		protected virtual void INComponent_ComponentID_CacheAttached(PXCache sender)
		{			
		}
		[PXDefault(typeof(InventoryItem.deferredCode))]
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Deferral Code", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(DRDeferredCode.deferredCodeID))]
		protected virtual void INComponent_DeferredCode_CacheAttached(PXCache sender)
		{			
		}
		#endregion

		#region POVendorInventory
        [PXDBInt()]
        [PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<POVendorInventory.inventoryID>>>>))]
        [PXDBLiteDefault(typeof(InventoryItem.inventoryID))]
        protected virtual void POVendorInventory_InventoryID_CacheAttached(PXCache sender)
        {
        }

		[SubItem(typeof(POVendorInventory.inventoryID), DisplayName = "Subitem")]
		protected virtual void POVendorInventory_SubItemID_CacheAttached(PXCache sender)
		{
		}

		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<POVendorInventory.vendorID>>>),
			DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location")]
		[PXFormula(typeof(Selector<POVendorInventory.vendorID, Vendor.defLocationID>))]
		[PXParent(typeof(Select<Location, Where<Location.bAccountID, Equal<Current<POVendorInventory.vendorID>>,
												And<Location.locationID, Equal<Current<POVendorInventory.vendorLocationID>>>>>))]
		protected virtual void POVendorInventory_VendorLocationID_CacheAttached(PXCache sender)
		{
		}

		#endregion	
		#region ARSalesPrice
		[PXDBInt()]
		[PXDBDefault(typeof(InventoryItem.inventoryID))]
		[PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<ARSalesPrice.inventoryID>>>>))]
		public virtual void ARSalesPrice_InventoryID_CacheAttached(PXCache sender) { }
		#endregion
		#region APSalesPrice
		[PXDBDefault(typeof(InventoryItem.inventoryID))]
		[Inventory]
		[PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<APSalesPrice.inventoryID>>>>))]
		public virtual void APSalesPrice_InventoryID_CacheAttached(PXCache sender) { }

		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<APSalesPrice.vendorID>>>),
			DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location")]
		[PXParent(typeof(Select<Location, Where<Location.bAccountID, Equal<Current<APSalesPrice.vendorID>>,
												And<Location.locationID, Equal<Current<APSalesPrice.vendorLocationID>>>>>))]
		[PXFormula(typeof(Selector<APSalesPrice.vendorID, Vendor.defLocationID>))]
		public virtual void APSalesPrice_VendorLocationID_CacheAttached(PXCache sender) { }
		#endregion
		#region INItemXRef
		[PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<INItemXRef.inventoryID>>>))]
        [Inventory(Filterable = true, DirtyRead = true, Enabled = false)]
        [PXDBDefault(typeof(InventoryItem.inventoryID), DefaultForInsert = true, DefaultForUpdate = false)]
        protected virtual void INItemXRef_InventoryID_CacheAttached(PXCache sender)
        {
        }
		#endregion
		#endregion

		#region Buttons Definition

		[PXCancelButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select)]
		protected virtual System.Collections.IEnumerable Cancel(PXAdapter a)
		{
			foreach (InventoryItem e in (new PXCancel<InventoryItem>(this, "Cancel")).Press(a))
			{
				if (Item.Cache.GetStatus(e) == PXEntryStatus.Inserted)
				{
					InventoryItem e1 = PXSelect<InventoryItem,
                        Where<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>, And<InventoryItem.stkItem, Equal<True>>>>
						.Select(this, e.InventoryCD);
					if (e1 != null)
					{
						Item.Cache.RaiseExceptionHandling<InventoryItem.inventoryCD>(e, e.InventoryCD,
							new PXSetPropertyException(Messages.StockItemExists));
					}
				}
				yield return e;
			}
		}
		public PXSave<InventoryItem> Save;
		public PXAction<InventoryItem> cancel;
		public PXInsert<InventoryItem> Insert;
		public PXCopyPasteAction<InventoryItem> Edit; 
		public PXDelete<InventoryItem> Delete;
		public PXFirst<InventoryItem> First;
		public PXPrevious<InventoryItem> Prev;
		public PXNext<InventoryItem> Next;
		public PXLast<InventoryItem> Last;

		public PXChangeID<InventoryItem, InventoryItem.inventoryCD> ChangeID;

		public PXAction<InventoryItem> updateCustPriceClass;
		[PXUIField(DisplayName = "Update Prices", Enabled = false)]
		[PXButton]
		public virtual IEnumerable UpdateCustPriceClass(PXAdapter adapter)
		{
			if (MassUpdateSettings.AskExt() == WebDialogResult.OK)
			{
				foreach (ARSalesPrice sp in PXSelect<ARSalesPrice, Where<ARSalesPrice.isCustClassPrice, Equal<boolTrue>, And<ARSalesPrice.inventoryID, Equal<Current<InventoryItem.inventoryID>>, And<ARSalesPrice.isPromotionalPrice, Equal<boolFalse>, And<ARSalesPrice.effectiveDate, IsNotNull, And<ARSalesPrice.effectiveDate, LessEqual<Required<MassUpdateFilter.effectiveDate>>>>>>>>.Select(this, MassUpdateSettings.Current.EffectiveDate))
				{
					ARSalesPrice copy = PXCache<ARSalesPrice>.CreateCopy(sp);
					copy.LastPrice = sp.SalesPrice;
					copy.LastBreakQty = sp.BreakQty;
					copy.LastDate = sp.EffectiveDate;
					copy.SalesPrice = sp.PendingPrice;
					copy.BreakQty = sp.PendingBreakQty;
					copy.EffectiveDate = null;
					copy.PendingPrice = 0;
					copy.PendingBreakQty = 0;
					copy.LastTaxID = copy.TaxID;
					copy.TaxID = copy.PendingTaxID;
					copy.PendingTaxID = null;

					this.ARSalesPrices.Update(copy);
				}
				ARSalesPrices.View.RequestRefresh();
			}
			return adapter.Get();
		}

		public PXAction<InventoryItem> updateCustomerPrice;
		[PXUIField(DisplayName = "Update Prices", MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXButton]
		public virtual IEnumerable UpdateCustomerPrice(PXAdapter adapter)
		{
			if (MassUpdateSettings.AskExt() == WebDialogResult.OK)
			{
				foreach (ARSalesPriceEx sp in PXSelect<ARSalesPriceEx, Where<ARSalesPriceEx.isCustClassPrice, Equal<boolFalse>, And<ARSalesPriceEx.inventoryID, Equal<Current<InventoryItem.inventoryID>>, And<ARSalesPriceEx.isPromotionalPrice, Equal<boolFalse>, And<ARSalesPriceEx.effectiveDate, IsNotNull, And<ARSalesPriceEx.effectiveDate, LessEqual<Required<MassUpdateFilter.effectiveDate>>>>>>>>.Select(this,  MassUpdateSettings.Current.EffectiveDate))
				{
					ARSalesPriceEx copy = PXCache<ARSalesPriceEx>.CreateCopy(sp);
					copy.LastPrice = sp.SalesPrice;
					copy.SalesPrice = sp.PendingPrice;
					copy.LastBreakQty = sp.BreakQty;
					copy.BreakQty = sp.PendingBreakQty;
					copy.LastDate = sp.EffectiveDate;
					copy.EffectiveDate = null;
					copy.PendingPrice = 0;
					copy.PendingBreakQty = 0;
					copy.LastTaxID = copy.TaxID;
					copy.TaxID = copy.PendingTaxID;
					copy.PendingTaxID = null;

					this.CustomerSalesPrice.Update(copy);
				}
				CustomerSalesPrice.View.RequestRefresh();
			}
			return adapter.Get();
		}

		public PXAction<InventoryItem> updateAPVendorPrice;
		[PXUIField(DisplayName = "Update Prices", MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXButton]
		public virtual IEnumerable UpdateAPVendorPrice(PXAdapter adapter)
		{
			if (MassUpdateSettings.AskExt() == WebDialogResult.OK)
			{
				foreach (APSalesPrice sp in PXSelect<APSalesPrice, Where<APSalesPrice.inventoryID, Equal<Current<InventoryItem.inventoryID>>, And<APSalesPrice.isPromotionalPrice, Equal<boolFalse>, And<APSalesPrice.effectiveDate, IsNotNull, And<APSalesPrice.effectiveDate, LessEqual<Required<MassUpdateFilter.effectiveDate>>>>>>>.Select(this, MassUpdateSettings.Current.EffectiveDate))
				{
					APSalesPrice copy = PXCache<APSalesPrice>.CreateCopy(sp);
					copy.LastPrice = sp.SalesPrice;
					copy.SalesPrice = sp.PendingPrice;
					copy.LastBreakQty = sp.BreakQty;
					copy.BreakQty = sp.PendingBreakQty;
					copy.LastDate = sp.EffectiveDate;
					copy.EffectiveDate = null;
					copy.PendingPrice = 0;
					copy.PendingBreakQty = 0;

					VendorSalesPrice.Update(copy);
				}
				VendorSalesPrice.View.RequestRefresh();
			}
			return adapter.Get();
		}
		#endregion

		#region InventoryItem Event Handlers

		protected virtual void InventoryItem_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;

            //Multiple Components are not supported for CashReceipt Deferred Revenue:
			DRDeferredCode dc = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Current<InventoryItem.deferredCode>>>>.Select(this);
            PXUIFieldAttribute.SetEnabled<InventoryItem.isSplitted>(sender, e.Row, e.Row != null && dc != null && dc.Method != DeferredMethodType.CashReceipt);
            PXUIFieldAttribute.SetEnabled<POVendorInventory.isDefault>(this.VendorItems.Cache, null, true);
			
			//Initial State for Components:
			Components.Cache.AllowDelete = false;
			Components.Cache.AllowInsert = false;
			Components.Cache.AllowUpdate = false;

			if (((InventoryItem)e.Row).IsSplitted == true)
			{
				Components.Cache.AllowDelete = true;
				Components.Cache.AllowInsert = true;
				Components.Cache.AllowUpdate = true;
				((InventoryItem)e.Row).TotalPercentage = SumComponentsPercentage();
				PXUIFieldAttribute.SetEnabled<InventoryItem.useParentSubID>(sender, e.Row, true);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled<InventoryItem.useParentSubID>(sender, e.Row, false);
				((InventoryItem)e.Row).UseParentSubID = false;
				((InventoryItem)e.Row).TotalPercentage = 100;
			}
			if (((InventoryItem)e.Row).NonStockReceipt == true)
			{
				PXUIFieldAttribute.SetRequired<InventoryItem.postClassID>(sender, true);
			}
			else
			{
				PXUIFieldAttribute.SetRequired<InventoryItem.postClassID>(sender, false);
			}

            if (PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>())
            {
                var branch = CurrentBranch.SelectSingle(PXAccess.GetBranchID());
                var item = Item.Current;

                if (branch != null && item != null)
                {
                    PXUIFieldAttribute.SetVisible<InventoryItem.isRUTROTDeductible>(Item.Cache, item, branch.AllowsRUTROT == true);
                }
            }

			this.ARSalesPrices.Cache.AllowInsert =
			this.ARSalesPrices.Cache.AllowUpdate =
			this.ARSalesPrices.Cache.AllowDelete = ((InventoryItem)e.Row).InventoryID > 0;

			this.CustomerSalesPrice.Cache.AllowInsert =
			this.CustomerSalesPrice.Cache.AllowUpdate =
			this.CustomerSalesPrice.Cache.AllowDelete = ((InventoryItem)e.Row).InventoryID > 0;

			this.VendorSalesPrice.Cache.AllowInsert =
			this.VendorSalesPrice.Cache.AllowUpdate =
			this.VendorSalesPrice.Cache.AllowDelete = ((InventoryItem)e.Row).InventoryID > 0;

			this.VendorItems.Cache.AllowInsert =
			this.VendorItems.Cache.AllowUpdate =
			this.VendorItems.Cache.AllowDelete = ((InventoryItem)e.Row).InventoryID > 0;
		}

		protected virtual void InventoryItem_ItemClassID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			InventoryItem row = (InventoryItem)e.Row;
			INItemClass ic = (INItemClass)PXSelectorAttribute.Select<INItemClass.itemClassID>(cache, row, e.NewValue);
			this.doResetDefaultsOnItemClassChange = false;

			if (ic != null)
			{
				this.doResetDefaultsOnItemClassChange = true;
				if (e.ExternalCall && cache.GetStatus(row) != PXEntryStatus.Inserted)
				{
					if (Item.Ask(AR.Messages.Warning, Messages.ItemClassChangeWarning, MessageButtons.YesNo) == WebDialogResult.No)
					{
						this.doResetDefaultsOnItemClassChange = false;
					}
				}
			}

		}

		protected virtual void InventoryItem_ItemClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetValueExt<InventoryItem.baseUnit>(e.Row, null);
			sender.SetValue<InventoryItem.salesUnit>(e.Row, null);
			sender.SetValue<InventoryItem.purchaseUnit>(e.Row, null);

			sender.SetDefaultExt<InventoryItem.baseUnit>(e.Row);
			sender.SetDefaultExt<InventoryItem.salesUnit>(e.Row);
			sender.SetDefaultExt<InventoryItem.purchaseUnit>(e.Row);
			sender.SetDefaultExt<InventoryItem.dfltSiteID>(e.Row);

			if (doResetDefaultsOnItemClassChange)
			{
				sender.SetDefaultExt<InventoryItem.taxCategoryID>(e.Row);
				sender.SetDefaultExt<InventoryItem.deferredCode>(e.Row);
				sender.SetDefaultExt<InventoryItem.itemType>(e.Row);
				sender.SetDefaultExt<InventoryItem.postClassID>(e.Row);
				sender.SetDefaultExt<InventoryItem.priceClassID>(e.Row);
				sender.SetDefaultExt<InventoryItem.markupPct>(e.Row);
				sender.SetDefaultExt<InventoryItem.minGrossProfitPct>(e.Row);

				INItemClass ic = ItemClass.Select();
				if (ic != null)
				{
					sender.SetValue<InventoryItem.priceWorkgroupID>(e.Row, ic.PriceWorkgroupID);
					sender.SetValue<InventoryItem.priceManagerID>(e.Row, ic.PriceManagerID);
				}
			}
		}

		protected virtual void InventoryItem_PostClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<InventoryItem.invtAcctID>(e.Row);
			sender.SetDefaultExt<InventoryItem.invtSubID>(e.Row);
			sender.SetDefaultExt<InventoryItem.salesAcctID>(e.Row);
			sender.SetDefaultExt<InventoryItem.salesSubID>(e.Row);
			sender.SetDefaultExt<InventoryItem.cOGSAcctID>(e.Row);
			sender.SetDefaultExt<InventoryItem.cOGSSubID>(e.Row);
			sender.SetDefaultExt<InventoryItem.pOAccrualAcctID>(e.Row);
			sender.SetDefaultExt<InventoryItem.pOAccrualSubID>(e.Row);
		}

		protected virtual void InventoryItem_IsSplitted_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			InventoryItem row = e.Row as InventoryItem;
			if (row != null)
			{
				if (row.IsSplitted == false)
				{
					foreach (INComponent c in Components.Select())
					{
						Components.Delete(c);
					}

					row.TotalPercentage = 100;
				}
				else
					row.TotalPercentage = 0;
			}
		}

		protected virtual void InventoryItem_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			((InventoryItem)e.Row).TotalPercentage = 100;
		}

		protected virtual void InventoryItem_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			InventoryItem row = e.Row as InventoryItem;

			if (row.IsSplitted == true)
			{
				if (string.IsNullOrEmpty(row.DeferredCode))
				{
					if (sender.RaiseExceptionHandling<InventoryItem.deferredCode>(e.Row, row.DeferredCode, new PXSetPropertyException(Data.ErrorMessages.FieldIsEmpty, typeof(InventoryItem.deferredCode).Name)))
					{
						throw new PXRowPersistingException(typeof(InventoryItem.deferredCode).Name, row.DeferredCode, Data.ErrorMessages.FieldIsEmpty, typeof(InventoryItem.deferredCode).Name);
					}
				}

				if (row.TotalPercentage != 100)
				{
					if (sender.RaiseExceptionHandling<InventoryItem.totalPercentage>(e.Row, row.TotalPercentage, new PXSetPropertyException(Messages.SumOfAllComponentsMustBeHundred)))
					{
						throw new PXRowPersistingException(typeof(InventoryItem.totalPercentage).Name, row.TotalPercentage, Messages.SumOfAllComponentsMustBeHundred);
					}
				}
			}

			if (!PXAccess.FeatureInstalled<FeaturesSet.distributionModule>())
			{
				row.NonStockReceipt = false;
				row.NonStockShip = false;
			}

			if (row.NonStockReceipt == true)
			{
				if (string.IsNullOrEmpty(row.PostClassID))
				{
					throw new PXRowPersistingException(typeof(InventoryItem.postClassID).Name, row.PostClassID, Data.ErrorMessages.FieldIsEmpty, typeof(InventoryItem.postClassID).Name);
				}
			}

			if (e.Operation == PXDBOperation.Delete)
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					PXDatabase.Delete<CSAnswers>(
						new PXDataFieldRestrict("EntityID", PXDbType.Int, ((InventoryItem)e.Row).InventoryID),
						new PXDataFieldRestrict("EntityType", PXDbType.Char, 2, ((InventoryItem)e.Row).EntityType, PXComp.EQ)
						);
					ts.Complete(this);
				}
			}
		}

		protected virtual void ARSalesPrice_UOM_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		private bool AlwaysFromBaseCurrency
		{
			get
			{
				bool alwaysFromBase = false;

				ARSetup arsetup = PXSelect<ARSetup>.Select(this);
				if (arsetup != null)
				{
					alwaysFromBase = arsetup.AlwaysFromBaseCury == true;
				}

				return alwaysFromBase;
			}
		}

		protected virtual void ARSalesPrice_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARSalesPrice row = (ARSalesPrice)e.Row;
			if (row == null) return;
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.pendingBreakQty>(sender, row, row.IsPromotionalPrice == false && !(row.CustPriceClassID == ARPriceClass.EmptyPriceClass && row.BreakQty == 0 && row.PendingBreakQty == 0 && ARSalesPrices.Cache.GetStatus(row) != PXEntryStatus.Inserted));
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.uOM>(sender, row, row.IsPromotionalPrice == true || (row.IsPromotionalPrice == false && !(row.CustPriceClassID == ARPriceClass.EmptyPriceClass && row.BreakQty == 0 && row.PendingBreakQty == 0 && ARSalesPrices.Cache.GetStatus(row) != PXEntryStatus.Inserted)));
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.custPriceClassID>(sender, row, row.IsPromotionalPrice == true || (row.IsPromotionalPrice == false && !(row.CustPriceClassID == ARPriceClass.EmptyPriceClass && row.BreakQty == 0 && row.PendingBreakQty == 0 && ARSalesPrices.Cache.GetStatus(row) != PXEntryStatus.Inserted)));		
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.effectiveDate>(sender, row, row.IsPromotionalPrice == false);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.pendingPrice>(sender, row, row.IsPromotionalPrice == false);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.pendingTaxID>(sender, row, row.IsPromotionalPrice == false);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.breakQty>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.salesPrice>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.taxID>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.lastDate>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.expirationDate>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.isPromotionalPrice>(sender, row, ARSalesPrices.Cache.GetStatus(row) == PXEntryStatus.Inserted);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.curyID>(sender, e.Row, AlwaysFromBaseCurrency == false && row.CustPriceClassID != ARPriceClass.EmptyPriceClass);
		}

		protected virtual void ARSalesPriceEx_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARSalesPriceEx row = (ARSalesPriceEx)e.Row;
			if (row == null) return;
			PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.effectiveDate>(sender, row, row.IsPromotionalPrice == false);
			PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.pendingBreakQty>(sender, row, row.IsPromotionalPrice == false);
			PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.pendingPrice>(sender, row, row.IsPromotionalPrice == false);
			PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.pendingTaxID>(sender, row, row.IsPromotionalPrice == false);
			PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.breakQty>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.salesPrice>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.taxID>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.lastDate>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.expirationDate>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.isPromotionalPrice>(sender, row, CustomerSalesPrice.Cache.GetStatus(row) == PXEntryStatus.Inserted);
			Customer cust = (Customer)PXParentAttribute.SelectParent(sender, row, typeof(Customer));
			if (cust != null)
				PXUIFieldAttribute.SetEnabled<ARSalesPriceEx.curyID>(sender, row, AlwaysFromBaseCurrency == false && (cust.CuryID == null ||  cust.AllowOverrideCury == true));
		}

		protected virtual void APSalesPrice_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			APSalesPrice row = (APSalesPrice)e.Row;
			if (row == null) return;
			PXUIFieldAttribute.SetEnabled<APSalesPrice.effectiveDate>(sender, row, row.IsPromotionalPrice == false);
			PXUIFieldAttribute.SetEnabled<APSalesPrice.pendingBreakQty>(sender, row, row.IsPromotionalPrice == false);
			PXUIFieldAttribute.SetEnabled<APSalesPrice.pendingPrice>(sender, row, row.IsPromotionalPrice == false);
			PXUIFieldAttribute.SetEnabled<APSalesPrice.breakQty>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<APSalesPrice.salesPrice>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<APSalesPrice.lastDate>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<APSalesPrice.expirationDate>(sender, row, row.IsPromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<APSalesPrice.isPromotionalPrice>(sender, row, VendorSalesPrice.Cache.GetStatus(row) == PXEntryStatus.Inserted);
			Vendor vend = (Vendor)PXParentAttribute.SelectParent(sender, row, typeof(Vendor));
			if (vend != null)
			{
				PXUIFieldAttribute.SetEnabled<APSalesPrice.curyID>(sender, row, vend.CuryID == null || vend.AllowOverrideCury == true);
			}
		}

		protected virtual void ARSalesPrice_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			ARSalesPrice row = (ARSalesPrice)e.Row;
			row.IsCustClassPrice = true;
			row.CuryID = Company.Current.BaseCuryID;
		}

		protected virtual void ARSalesPrice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ARSalesPrice row = (ARSalesPrice)e.Row;
			if (row.CustPriceClassID == null)
				sender.RaiseExceptionHandling<ARSalesPrice.custPriceClassID>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPrice.custPriceClassID).Name));
			if (row.IsPromotionalPrice == true && row.LastDate == null)
				sender.RaiseExceptionHandling<ARSalesPrice.lastDate>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPrice.lastDate).Name));
			if (row.IsPromotionalPrice == true && row.ExpirationDate == null)
				sender.RaiseExceptionHandling<ARSalesPrice.expirationDate>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPrice.expirationDate).Name));
			if (row.IsPromotionalPrice == true && row.ExpirationDate < row.LastDate)
				sender.RaiseExceptionHandling<ARSalesPrice.lastDate>(row, row.LastDate, new PXSetPropertyException(AR.Messages.LastDateExpirationDate, PXErrorLevel.RowError));
			if (row.CuryID == null)
				sender.RaiseExceptionHandling<ARSalesPrice.curyID>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPrice.curyID).Name));
		}

		protected virtual void ARSalesPriceEx_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			ARSalesPriceEx row = (ARSalesPriceEx)e.Row;
			row.IsCustClassPrice = false;
		}

		protected virtual void ARSalesPriceEx_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ARSalesPriceEx row = (ARSalesPriceEx)e.Row;
			if (row.CustomerID == null)
				sender.RaiseExceptionHandling<ARSalesPriceEx.customerID>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPriceEx.customerID).Name));
			if (row.IsPromotionalPrice == true && row.LastDate == null)
				sender.RaiseExceptionHandling<ARSalesPriceEx.lastDate>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPriceEx.lastDate).Name));
			if (row.IsPromotionalPrice == true && row.ExpirationDate == null)
				sender.RaiseExceptionHandling<ARSalesPriceEx.expirationDate>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPriceEx.expirationDate).Name));
			if (row.IsPromotionalPrice == true && row.ExpirationDate < row.LastDate)
				sender.RaiseExceptionHandling<ARSalesPriceEx.lastDate>(row, row.LastDate, new PXSetPropertyException(AR.Messages.LastDateExpirationDate, PXErrorLevel.RowError));
			if (row.CuryID == null)
				sender.RaiseExceptionHandling<ARSalesPriceEx.curyID>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPriceEx.curyID).Name));
		}

		protected virtual void ARSalesPrice_IsPromotionalPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARSalesPrice row = (ARSalesPrice)e.Row;
			if (row.IsPromotionalPrice == true)
			{
				row.PendingBreakQty = 0;
				row.PendingPrice = 0;
				row.EffectiveDate = null;
				row.LastDate = Accessinfo.BusinessDate;
			}
			else
			{
				row.BreakQty = 0;
				row.SalesPrice = 0;
				row.LastDate = null;
				row.ExpirationDate = null;
				row.EffectiveDate = Accessinfo.BusinessDate;
			}
		}

		protected virtual void ARSalesPrice_PendingPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARSalesPrice row = e.Row as ARSalesPrice;
			if (row != null)
			{
				if (row.EffectiveDate == null && row.IsPromotionalPrice == false && row.PendingPrice != 0m)
					sender.SetDefaultExt<ARSalesPrice.effectiveDate>(e.Row);
			}
		}

		protected virtual void ARSalesPriceEx_PendingPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARSalesPriceEx row = e.Row as ARSalesPriceEx;
			if (row != null)
			{
				if (row.EffectiveDate == null && row.IsPromotionalPrice == false && row.PendingPrice != 0m)
					sender.SetDefaultExt<ARSalesPriceEx.effectiveDate>(e.Row);
			}
		}

		protected virtual void APSalesPrice_PendingPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APSalesPrice row = e.Row as APSalesPrice;
			if (row != null)
			{
				if (row.EffectiveDate == null && row.IsPromotionalPrice == false && row.PendingPrice != 0m)
					sender.SetDefaultExt<APSalesPrice.effectiveDate>(e.Row);
			}
		}

		protected virtual void ARSalesPrice_CustPriceClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARSalesPrice row = (ARSalesPrice)e.Row;
			if (row.CustPriceClassID == ARPriceClass.EmptyPriceClass)
			{
				sender.SetValueExt<ARSalesPrice.isPromotionalPrice>(row, false);
			}
		}

		protected virtual void ARSalesPrice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARSalesPrice row = e.Row as ARSalesPrice;
			if (!sender.ObjectsEqual<ARSalesPrice.custPriceClassID, ARSalesPrice.curyID, ARSalesPrice.uOM, ARSalesPrice.pendingBreakQty, ARSalesPrice.pendingPrice, ARSalesPrice.effectiveDate>(e.Row, e.OldRow))
			{
				string uom = SalesPriceUpdateUnit == SalesPriceUpdateUnitType.SalesUnit ? Item.Current.SalesUnit : Item.Current.BaseUnit;
				if (sender.GetStatus(row) != PXEntryStatus.Inserted && row.CustPriceClassID == ARPriceClass.EmptyPriceClass && row.UOM == uom && row.CuryID == Company.Current.BaseCuryID && row.PendingBreakQty == 0 && row.BreakQty == 0 && row.IsPromotionalPrice != true)
				{
					InventoryItem copy = PXCache<InventoryItem>.CreateCopy(Item.Current);

					copy.PendingBasePriceDate = row.EffectiveDate;
					copy.PendingBasePrice = SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit ? row.PendingPrice ?? 0m : INUnitAttribute.ConvertFromBase(SalesPrice.Cache, Item.Current.InventoryID, Item.Current.SalesUnit, row.PendingPrice ?? 0m, INPrecision.UNITCOST);
					copy.BasePrice = SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit ? row.SalesPrice ?? 0m : INUnitAttribute.ConvertFromBase(SalesPrice.Cache, Item.Current.InventoryID, Item.Current.SalesUnit, row.SalesPrice ?? 0m, INPrecision.UNITCOST);
					copy.LastBasePrice = SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit ? row.LastPrice ?? 0m : INUnitAttribute.ConvertFromBase(SalesPrice.Cache, Item.Current.InventoryID, Item.Current.SalesUnit, row.LastPrice ?? 0m, INPrecision.UNITCOST);
					copy.BasePriceDate = row.LastDate;

					sender.Graph.RowUpdated.RemoveHandler<InventoryItem>(InventoryItem_RowUpdated);
					try
					{
						Item.Update(copy);
					}
					finally
					{
						sender.Graph.RowUpdated.AddHandler<InventoryItem>(InventoryItem_RowUpdated);
					}
				}
			}
		}

		protected virtual void ARSalesPriceEx_IsPromotionalPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARSalesPriceEx row = (ARSalesPriceEx)e.Row;
			if (row.IsPromotionalPrice == true)
			{
				row.PendingBreakQty = 0;
				row.PendingPrice = 0;
				row.EffectiveDate = null;
				row.LastDate = Accessinfo.BusinessDate;
			}
			else
			{
				row.BreakQty = 0;
				row.SalesPrice = 0;
				row.LastDate = null;
				row.ExpirationDate = null;
				row.EffectiveDate = Accessinfo.BusinessDate;
			}
		}

		protected virtual void APSalesPrice_IsPromotionalPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APSalesPrice row = (APSalesPrice)e.Row;
			if (row.IsPromotionalPrice == true)
			{
				row.PendingBreakQty = 0;
				row.PendingPrice = 0;
				row.EffectiveDate = null;
				row.LastDate = Accessinfo.BusinessDate;
			}
			else
			{
				row.BreakQty = 0;
				row.SalesPrice = 0;
				row.LastDate = null;
				row.ExpirationDate = null;
				row.EffectiveDate = Accessinfo.BusinessDate;
			}
		}

		protected virtual void ARSalesPriceEx_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARSalesPriceEx row = (ARSalesPriceEx)e.Row;

			if (AlwaysFromBaseCurrency == true)
			{
				row.CuryID = Company.Current.BaseCuryID;
			}
			else
			{
				Customer cust = (Customer)PXParentAttribute.SelectParent(sender, row, typeof(Customer));
				if (cust != null)
				{
					if (cust.CuryID != null)
						row.CuryID = cust.CuryID;
					else
						row.CuryID = Company.Current.BaseCuryID;
				}
			}

		}

		protected virtual void APSalesPrice_VendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APSalesPrice row = (APSalesPrice)e.Row;
			Vendor vend = (Vendor)PXParentAttribute.SelectParent(sender, row, typeof(Vendor));
			if (vend != null)
			{
				if (vend.CuryID != null)
					row.CuryID = vend.CuryID;
				else
					row.CuryID = Company.Current.BaseCuryID;
			}
		}

		protected virtual void APSalesPrice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			APSalesPrice price = e.Row as APSalesPrice;

			if (price.BreakQty==0 && price.PendingBreakQty==0 && price.IsPromotionalPrice!=true && (!sender.ObjectsEqual<APSalesPrice.pendingPrice>(e.Row, e.OldRow) ||
				!sender.ObjectsEqual<APSalesPrice.effectiveDate>(e.Row, e.OldRow) ||
				!sender.ObjectsEqual<APSalesPrice.salesPrice>(e.Row, e.OldRow)))
			{
				POVendorInventory vi = PXSelect<POVendorInventory,
					Where<POVendorInventory.inventoryID, Equal<Required<APSalesPrice.inventoryID>>,
					And<POVendorInventory.vendorID, Equal<Required<APSalesPrice.vendorID>>,
					And<POVendorInventory.vendorLocationID, Equal<Required<APSalesPrice.vendorLocationID>>,
					And<POVendorInventory.purchaseUnit, Equal<Required<APSalesPrice.uOM>>,
					And<POVendorInventory.curyID, Equal<Required<APSalesPrice.curyID>>>>>>>>.Select(this, price.InventoryID, price.VendorID, price.VendorLocationID, price.UOM, price.CuryID);

				if (vi != null)
				{
					POVendorInventory copy = PXCache<POVendorInventory>.CreateCopy(vi);
					copy.PendingPrice = price.PendingPrice;
					copy.PendingDate = price.EffectiveDate;
					copy.EffPrice = price.SalesPrice;
					copy.EffDate = price.LastDate;
					copy.LastPrice = price.LastPrice;

					sender.Graph.RowUpdated.RemoveHandler<POVendorInventory>(POVendorInventory_RowUpdated);
					try
					{
						VendorItems.Update(copy);
					}
					finally
					{
						sender.Graph.RowUpdated.AddHandler<POVendorInventory>(POVendorInventory_RowUpdated);
					}
				}
			}
		}

		protected virtual void APSalesPrice_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			APSalesPrice price = e.Row as APSalesPrice;
			POVendorInventory vendor = getPOVendorInventory(price);
			if (vendor != null)
				VendorItems.Delete(vendor);
		}

		protected virtual void APSalesPrice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			APSalesPrice row = (APSalesPrice)e.Row;
			if (row.IsPromotionalPrice == true && row.LastDate == null)
				sender.RaiseExceptionHandling<APSalesPrice.lastDate>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APSalesPrice.lastDate).Name));
			if (row.IsPromotionalPrice == true && row.ExpirationDate == null)
				sender.RaiseExceptionHandling<APSalesPrice.expirationDate>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APSalesPrice.expirationDate).Name));
			if (row.IsPromotionalPrice == true && row.ExpirationDate < row.LastDate)
				sender.RaiseExceptionHandling<APSalesPrice.lastDate>(row, row.LastDate, new PXSetPropertyException(AR.Messages.LastDateExpirationDate, PXErrorLevel.RowError));
			if (row.CuryID == null)
				sender.RaiseExceptionHandling<APSalesPrice.curyID>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APSalesPrice.curyID).Name));
		}

		protected virtual void ARSalesPrice_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			ARSalesPrice row = e.Row as ARSalesPrice;
			string uOM = SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit ? ItemSettings.Current.BaseUnit : ItemSettings.Current.SalesUnit;
			if (row.IsPromotionalPrice == false && row.CustPriceClassID == ARPriceClass.EmptyPriceClass && row.BreakQty == 0 && row.PendingBreakQty == 0 && row.UOM == uOM)
			{
				e.Cancel = true;
				throw new Exception(Messages.BaseSalesPriceDelete);
			}
		}

		private void InsertSalesPrice(PXCache cache, InventoryItem item, string uom)
		{
			ARSalesPrice price = PXSelect<ARSalesPrice,
													Where<ARSalesPrice.inventoryID, Equal<Required<InventoryItem.inventoryID>>,
															And2<Where<ARSalesPrice.custPriceClassID, IsNotNull, And<ARSalesPrice.custPriceClassID, Equal<ARPriceClass.emptyPriceClass>>>,
															And<ARSalesPrice.uOM, Equal<Required<ARSalesPrice.uOM>>,
															And<ARSalesPrice.isPromotionalPrice, Equal<boolFalse>,
															And<ARSalesPrice.pendingBreakQty, Equal<decimal0>,
															And<ARSalesPrice.breakQty, Equal<decimal0>,
															And<ARSalesPrice.curyID, Equal<Required<GL.Company.baseCuryID>>>>>>>>>>.SelectSingleBound(this, null, item.InventoryID, uom, Company.Current.BaseCuryID);
			if (price == null)
			{
				ARSalesPrice sp = new ARSalesPrice();
				sp.CustPriceClassID = AR.ARPriceClass.EmptyPriceClass;
				sp.InventoryID = item.InventoryID;
				sp.CuryID = Company.Current.BaseCuryID;
				sp.UOM = uom;

				sp.EffectiveDate = item.PendingBasePriceDate;
				sp.LastDate = item.BasePriceDate;

				if (uom != item.BaseUnit)
				{
					sp.SalesPrice = INUnitAttribute.ConvertToBase(cache, item.InventoryID, uom, item.BasePrice ?? 0, INPrecision.UNITCOST);
					sp.PendingPrice = INUnitAttribute.ConvertToBase(cache, item.InventoryID, uom, item.PendingBasePrice ?? 0, INPrecision.UNITCOST);
				}
				else
				{
					sp.SalesPrice = item.BasePrice;
					sp.PendingPrice = item.PendingBasePrice;
				}

				if (sp.PendingPrice <= 0m)
				{
					sp.PendingPrice = 0;
					sp.EffectiveDate = null;
				}

				SalesPrice.Insert(sp);
			}
		}

		private void InsertPOVendorInventory(APSalesPrice price)
		{
			POVendorInventory inventory = getPOVendorInventory(price);
			if (inventory == null)
			{
				POVendorInventory vi = new POVendorInventory();
				vi.InventoryID = price.InventoryID;
				vi.VendorID = price.VendorID;
				vi.VendorLocationID = price.VendorLocationID;
				vi.PurchaseUnit = price.UOM;
				vi.PendingDate = price.EffectiveDate;
				vi.PendingPrice = price.PendingPrice;
				vi.EffDate = price.LastDate;
				vi.EffPrice = price.SalesPrice;
				vi.LastPrice = price.LastPrice;
				VendorItems.Insert(vi);
			}
		}

		private POVendorInventory getPOVendorInventory(APSalesPrice price)
		{
			return PXSelect<POVendorInventory,
										Where<POVendorInventory.inventoryID, Equal<Required<APSalesPrice.inventoryID>>,
											And<POVendorInventory.vendorID, Equal<Required<APSalesPrice.vendorID>>,
											And<POVendorInventory.vendorLocationID, Equal<Required<APSalesPrice.vendorLocationID>>,
											And<POVendorInventory.purchaseUnit, Equal<Required<APSalesPrice.uOM>>,
											And<POVendorInventory.curyID, Equal<Required<APSalesPrice.curyID>>>>>>>>.Select(this, price.InventoryID, price.VendorID, price.VendorLocationID, price.UOM, price.CuryID);
		}

		private void InsertAPVendorPrice(POVendorInventory inventory)
		{
			APSalesPrice price = getAPSalesPrice(inventory);
			if (price == null)
			{
				APSalesPrice vp = new APSalesPrice();
				vp.InventoryID = inventory.InventoryID;
				vp.VendorID = inventory.VendorID;
				vp.VendorLocationID = inventory.VendorLocationID;
				vp.UOM = inventory.PurchaseUnit;
				vp.EffectiveDate = inventory.PendingDate;
				vp.PendingPrice = inventory.PendingPrice;
				vp.LastDate = inventory.EffDate;
				vp.SalesPrice = inventory.EffPrice;
				vp.LastPrice = inventory.LastPrice;
				vp.IsPromotionalPrice = false;
				vp.PendingBreakQty = 0;
				vp.BreakQty = 0;
				vp.LastBreakQty = 0;
				VendorSalesPrice.Insert(vp);
			}
		}

		private APSalesPrice getAPSalesPrice(POVendorInventory inventory)
		{
			return PXSelect<APSalesPrice,Where<APSalesPrice.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
											And<APSalesPrice.vendorID, Equal<Required<POVendorInventory.vendorID>>,
											And<APSalesPrice.vendorLocationID, Equal<Required<POVendorInventory.vendorLocationID>>,
											And<APSalesPrice.uOM, Equal<Required<POVendorInventory.purchaseUnit>>,
											And<APSalesPrice.curyID, Equal<Required<POVendorInventory.curyID>>,
											And<APSalesPrice.isPromotionalPrice, Equal<boolFalse>,
											And<APSalesPrice.pendingBreakQty, Equal<decimal0>,
											And<APSalesPrice.breakQty, Equal<decimal0>>>>>>>>>>.Select(this, inventory.InventoryID, inventory.VendorID, inventory.VendorLocationID, inventory.PurchaseUnit, inventory.CuryID);
		}

		protected virtual void InventoryItem_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			InventoryItem item = e.Row as InventoryItem;

			if (!sender.ObjectsEqual<InventoryItem.pendingBasePrice, InventoryItem.pendingBasePriceDate>(e.Row, e.OldRow) ||
				!sender.ObjectsEqual<InventoryItem.salesUnit>(e.Row, e.OldRow) && SalesPriceUpdateUnit != SalesPriceUpdateUnitType.BaseUnit ||
				!sender.ObjectsEqual<InventoryItem.baseUnit>(e.Row, e.OldRow) && SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit)
			{
				ARSalesPrice sp = PXSelect<ARSalesPrice,
					Where<ARSalesPrice.custPriceClassID, Equal<ARPriceClass.emptyPriceClass>,
					And<ARSalesPrice.inventoryID, Equal<Required<InventoryItem.inventoryID>>,
					And<ARSalesPrice.uOM, Equal<Required<InventoryItem.baseUnit>>,
					And<ARSalesPrice.curyID, Equal<Required<GL.Company.baseCuryID>>,
					And<ARSalesPrice.breakQty, Equal<decimal0>,
					And<ARSalesPrice.isPromotionalPrice, Equal<boolFalse>>>>>>>>.Select(this, item.InventoryID, SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit ? item.BaseUnit : item.SalesUnit, Company.Current.BaseCuryID);

				if (sp != null)
				{
					ARSalesPrice copy = PXCache<ARSalesPrice>.CreateCopy(sp);
					copy.PendingPrice = SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit ? item.PendingBasePrice : INUnitAttribute.ConvertToBase(SalesPrice.Cache, item.InventoryID, item.SalesUnit, item.PendingBasePrice ?? 0m, INPrecision.UNITCOST);
					copy.EffectiveDate = item.PendingBasePriceDate;
					copy.SalesPrice = SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit ? item.BasePrice : INUnitAttribute.ConvertToBase(SalesPrice.Cache, item.InventoryID, item.SalesUnit, item.BasePrice ?? 0m, INPrecision.UNITCOST);
					copy.LastDate = item.BasePriceDate;
					copy.LastPrice = SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit ? item.LastBasePrice : INUnitAttribute.ConvertToBase(SalesPrice.Cache, item.InventoryID, item.SalesUnit, item.LastBasePrice ?? 0m, INPrecision.UNITCOST);

					sender.Graph.RowUpdated.RemoveHandler<ARSalesPrice>(ARSalesPrice_RowUpdated);
					try
					{
						SalesPrice.Update(copy);
					}
					finally
					{
						sender.Graph.RowUpdated.AddHandler<ARSalesPrice>(ARSalesPrice_RowUpdated);
					}
				}
			}
		}

		protected virtual void InventoryItem_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			this.RowDeleting.RemoveHandler<ARSalesPrice>(ARSalesPrice_RowDeleting);
		}

		protected virtual void InventoryItem_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (e.Row == null) { return; }

			InventoryItem ii_rec = (InventoryItem)e.Row;

			// deleting only inventory-specific uoms
			foreach (INUnit inunit_rec in PXSelect<INUnit, Where<INUnit.unitType, Equal<short1>, And<INUnit.inventoryID, Equal<Required<INUnit.inventoryID>>>>>.Select(this, ii_rec.InventoryID))
			{
				itemunits.Delete(inunit_rec);
			}
		}

		#endregion

		#region INComponent Event Handles

		protected virtual void INComponent_Percentage_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			INComponent row = e.Row as INComponent;
			if (row != null && row.AmtOption == INAmountOption.Percentage)
			{
				row.Percentage = GetRemainingPercentage();
			}
		}

		protected virtual void INComponent_ComponentID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INComponent row = e.Row as INComponent;
			if (row != null)
			{
				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, row.ComponentID);
				if (item != null)
				{
					row.SalesAcctID = item.SalesAcctID;
					row.SalesSubID = item.SalesSubID;
					row.UOM = item.SalesUnit;
					row.DeferredCode = item.DeferredCode;
				}
			}
		}


		protected virtual void INComponent_AmtOption_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INComponent row = e.Row as INComponent;
			if (row != null)
			{
				if (row.AmtOption == INAmountOption.Percentage)
				{
					row.FixedAmt = null;
					row.Percentage = GetRemainingPercentage();
				}
				else
					row.Percentage = 0;
			}
		}


		#endregion

		public PXAction<InventoryItem> action;
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		protected virtual IEnumerable Action(PXAdapter adapter,
			[PXInt]
			[PXIntList(new int[] { 1, 2, 3 }, new string[] 
			{ 
					"Update Price", 
					"Update Cost",
          "View Restriction Groups"                    
			})]
			int? actionID
			)
		{
			switch (actionID)
			{
				case 1:
					if (ItemSettings.Current != null && ItemSettings.Current.PendingBasePriceDate != null)
					{
						ItemSettings.Current.LastBasePrice = ItemSettings.Current.BasePrice;
						ItemSettings.Current.BasePrice = ItemSettings.Current.PendingBasePrice;
						ItemSettings.Current.BasePriceDate = ItemSettings.Current.PendingBasePriceDate ?? new DateTime(1900, 1, 1);
						ItemSettings.Current.PendingBasePrice = 0;
						ItemSettings.Current.PendingBasePriceDate = null;

						InsertSalesPrice(Item.Cache, ItemSettings.Current, SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit ? ItemSettings.Current.BaseUnit : ItemSettings.Current.SalesUnit);
						ItemSettings.Update(ItemSettings.Current);

						this.Save.Press();
					}
					break;
				case 2:
					if (ItemSettings.Current != null && ItemSettings.Current.PendingStdCostDate != null)
					{
						ItemSettings.Current.LastStdCost = ItemSettings.Current.StdCost;
						ItemSettings.Current.StdCostDate = ItemSettings.Current.PendingStdCostDate.GetValueOrDefault((DateTime)this.Accessinfo.BusinessDate);
						ItemSettings.Current.StdCost = ItemSettings.Current.PendingStdCost;
						ItemSettings.Current.PendingStdCost = 0;
						ItemSettings.Current.PendingStdCostDate = null;
						ItemSettings.Update(ItemSettings.Current);

						this.Save.Press();
					}

					break;
				case 3:
					if (Item.Current != null)
					{
						INAccessDetailByItem graph = CreateInstance<INAccessDetailByItem>();
						graph.Item.Current = graph.Item.Search<InventoryItem.inventoryCD>(Item.Current.InventoryCD);
						throw new PXRedirectRequiredException(graph, false, "Restricted Groups");
					}
					break;
			}
			return adapter.Get();
		}


		private decimal GetRemainingPercentage()
		{
			decimal result = 100;

			foreach (INComponent comp in Components.Select())
			{
				if (comp.AmtOption == INAmountOption.Percentage)
				result -= (comp.Percentage ?? 0);
			}

			if (result > 0)
				return result;
			else
				return 0;
		}

		private decimal SumComponentsPercentage()
		{
			decimal result = 0;

			foreach (INComponent comp in Components.Select())
			{
				if (comp.AmtOption == INAmountOption.Percentage)
				result += (comp.Percentage ?? 0);
			}

			return result;
		}

		public override void Persist()
		{
			foreach (ARSalesPrice price in ARSalesPrices.Cache.Inserted)
			{
				ARSalesPriceMaint.ValidateDuplicate(this, ARSalesPrices.Cache, price);
			}
			foreach (ARSalesPrice price in ARSalesPrices.Cache.Updated)
			{
				ARSalesPriceMaint.ValidateDuplicate(this, ARSalesPrices.Cache, price);
			}
			foreach (ARSalesPriceEx price in CustomerSalesPrice.Cache.Inserted)
			{
				InventoryItemMaint.ValidateDuplicateCustomerPrice(this, CustomerSalesPrice.Cache, price);
			}
			foreach (ARSalesPriceEx price in CustomerSalesPrice.Cache.Updated)
			{
				InventoryItemMaint.ValidateDuplicateCustomerPrice(this, CustomerSalesPrice.Cache, price);
			}
			foreach (APSalesPrice price in VendorSalesPrice.Cache.Inserted)
			{
				APVendorSalesPriceMaint.ValidateDuplicate(this, VendorSalesPrice.Cache, price);
				if (price.IsPromotionalPrice == false && price.PendingBreakQty == 0 && price.BreakQty == 0)
					InsertPOVendorInventory(price);
			}
			foreach (APSalesPrice price in VendorSalesPrice.Cache.Updated)
			{
				APVendorSalesPriceMaint.ValidateDuplicate(this, VendorSalesPrice.Cache, price);
			}

			foreach (InventoryItem item in Item.Cache.Inserted)
			{
				string uom = SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit ? item.BaseUnit : item.SalesUnit;
				if (uom != null)
				{
					InsertSalesPrice(Item.Cache, item, uom);
				}
			}

			foreach (POVendorInventory vi in VendorItems.Cache.Inserted)
			{
				InsertAPVendorPrice(vi);
			}

			base.Persist();
		}

		#region Private members
		private bool doResetDefaultsOnItemClassChange;
		#endregion

		private string SalesPriceUpdateUnit
		{
			get
			{
				SOSetup sosetup = PXSelect<SOSetup>.Select(this);
				if (sosetup != null && !string.IsNullOrEmpty(sosetup.SalesPriceUpdateUnit))
				{
					return sosetup.SalesPriceUpdateUnit;
				}

				return SalesPriceUpdateUnitType.BaseUnit;
			}
        }

        #region InventoryItem
        #region InventoryCD
        [PXDefault()]
		[InventoryRaw(typeof(Where<InventoryItem.stkItem, Equal<False>>), IsKey = true, DisplayName = "Inventory ID", DescriptionField = typeof(InventoryItem.descr), Filterable = true)]
        [PX.Data.EP.PXFieldDescription]
        public virtual void InventoryItem_InventoryCD_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region ItemClassID
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Item Class", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<INItemClass.itemClassID, Where<INItemClass.stkItem, Equal<boolFalse>>>), DescriptionField = typeof(INItemClass.descr))]
        public virtual void InventoryItem_ItemClassID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region PostClassID
        [PXDBString(10, IsUnicode = true)]
        [PXSelector(typeof(INPostClass.postClassID), DescriptionField = typeof(INPostClass.descr))]
        [PXUIField(DisplayName = "Posting Class", Required = true)]
        [PXDefault(typeof(Search<INItemClass.postClassID, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual void InventoryItem_PostClassID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region LotSerClassID
        [PXDBString(10, IsUnicode = true)]
        public virtual void InventoryItem_LotSerClassID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region ItemType
        [PXDBString(1, IsFixed = true)]
        [PXDefault(INItemTypes.NonStockItem, typeof(Search<INItemClass.itemType, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>))]
        [PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible)]
        [INItemTypes.NonStockList()]
        public virtual void InventoryItem_ItemType_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region ValMethod
        [PXDBString(1, IsFixed = true)]
        [PXDefault(INValMethod.Standard)]
        public virtual void InventoryItem_ValMethod_CacheAttached(PXCache sender)
        {
        }
        #endregion
		#region InvtAcctID
		[Account(DisplayName = "Expense Accrual Account", DescriptionField = typeof(Account.description))]
		[PXDefault(typeof(Search<INPostClass.invtAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual void InventoryItem_InvtAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region InvtSubID
		[SubAccount(typeof(InventoryItem.invtAcctID), DisplayName = "Expense Accrual Sub.", DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.invtSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual void InventoryItem_InvtSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
        #region COGSAcctID
        [PXDefault(typeof(Search<INPostClass.cOGSAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>))]
        [Account(DisplayName = "Expense Account", DescriptionField = typeof(Account.description))]
        public virtual void InventoryItem_COGSAcctID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region COGSSubID
        [PXDefault(typeof(Search<INPostClass.cOGSSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>))]
        [SubAccount(typeof(InventoryItem.cOGSAcctID), DisplayName = "Expense Sub.", DescriptionField = typeof(Sub.description))]
        public virtual void InventoryItem_COGSSubID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region StkItem
        [PXDBBool()]
        [PXDefault(false)]
        public virtual void InventoryItem_StkItem_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region KitItem
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Is a Kit")]
        public virtual void InventoryItem_KitItem_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region DefaultSubItemOnEntry
        [PXDBBool()]
        [PXDefault(false)]
        public virtual void InventoryItem_DefaultSubItemOnEntry_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region DeferredCode
        [PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa", BqlField = typeof(InventoryItem.deferredCode))]
        [PXUIField(DisplayName = "Deferral Code")]
        [PXSelector(typeof(Search<DRDeferredCode.deferredCodeID>))]
        [PXDefault(typeof(Search<INItemClass.deferredCode, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual void InventoryItem_DeferredCode_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region IsSplitted
        [PXDBBool(BqlField = typeof(InventoryItem.isSplitted))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Split into Components")]
        public virtual void InventoryItem_IsSplitted_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region UseParentSubID
        [PXDBBool(BqlField = typeof(InventoryItem.useParentSubID))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Use Component Subaccounts")]
        public virtual void InventoryItem_UseParentSubID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region TotalPercentage
        [PXDecimal()]
        [PXUIField(DisplayName = "Total Percentage", Enabled = false)]
        public virtual void InventoryItem_TotalPercentage_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region NonStockReceipt
        [PXDBBool(BqlField = typeof(InventoryItem.nonStockReceipt))]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Require Receipt")]
        public virtual void InventoryItem_NonStockReceipt_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region NonStockShip
        [PXDBBool(BqlField = typeof(InventoryItem.nonStockShip))]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Require Shipment")]
        public virtual void InventoryItem_NonStockShip_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion


        #region POInventoryItem
        protected virtual void POVendorInventory_IsDefault_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            POVendorInventory current = e.Row as POVendorInventory;
            if ((POVendorInventory)this.VendorItems.SelectWindowed(0, 1, current.InventoryID) == null)
            {
                e.NewValue = true;
                e.Cancel = true;
            }
        }

        protected virtual void POVendorInventory_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            POVendorInventory current = e.Row as POVendorInventory;
            if (current.VendorID != null)
                if ((current.IsDefault == true && (this.Item.Current.PreferredVendorID != current.VendorID || this.Item.Current.PreferredVendorLocationID != current.VendorLocationID) ||
                    (current.IsDefault != true && this.Item.Current.PreferredVendorID == current.VendorID && this.Item.Current.PreferredVendorLocationID == current.VendorLocationID)))
            {
                    InventoryItem upd = this.Item.Current;
                upd.PreferredVendorID = current.IsDefault == true ? current.VendorID : null;
                upd.PreferredVendorLocationID = current.IsDefault == true ? current.VendorLocationID : null;
                    if (this.Item.Cache.GetStatus(upd) == PXEntryStatus.Notchanged)
                        this.Item.Cache.SetStatus(upd, PXEntryStatus.Updated);
                    this.VendorItems.View.RequestRefresh();
            }
        }

        protected virtual void POVendorInventory_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            POVendorInventory current = e.Row as POVendorInventory;
            POVendorInventory old = e.OldRow as POVendorInventory;
            if (current == null || old == null) return;

            if (current.VendorID != null)
                if ((current.IsDefault == true && (this.Item.Current.PreferredVendorID != current.VendorID || this.Item.Current.PreferredVendorLocationID != current.VendorLocationID) ||
                        (current.IsDefault != true && this.Item.Current.PreferredVendorID == current.VendorID && this.Item.Current.PreferredVendorLocationID == current.VendorLocationID)))
                {
                    InventoryItem upd = this.Item.Current;
                    upd.PreferredVendorID = current.IsDefault == true ? current.VendorID : null;
                    upd.PreferredVendorLocationID = current.IsDefault == true ? current.VendorLocationID : null;
                    if (this.Item.Cache.GetStatus(upd) == PXEntryStatus.Notchanged)
                        this.Item.Cache.SetStatus(upd, PXEntryStatus.Updated);
                    this.VendorItems.View.RequestRefresh();
                }

			if (!cache.ObjectsEqual<POVendorInventory.pendingDate>(e.Row, e.OldRow) ||
				!cache.ObjectsEqual<POVendorInventory.pendingPrice>(e.Row, e.OldRow) ||
				!cache.ObjectsEqual<POVendorInventory.effPrice>(e.Row, e.OldRow))
			{
				APSalesPrice sp = PXSelect<APSalesPrice,
					Where<APSalesPrice.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
					And<APSalesPrice.vendorID, Equal<Required<POVendorInventory.vendorID>>,
					And<APSalesPrice.vendorLocationID, Equal<Required<POVendorInventory.vendorLocationID>>,
					And<APSalesPrice.uOM, Equal<Required<POVendorInventory.purchaseUnit>>,
					And<APSalesPrice.curyID, Equal<Required<POVendorInventory.curyID>>,
					And<APSalesPrice.breakQty, Equal<decimal0>,
					And<APSalesPrice.pendingBreakQty, Equal<decimal0>,
					And<APSalesPrice.isPromotionalPrice, Equal<boolFalse>>>>>>>>>>.Select(this, current.InventoryID, current.VendorID, current.VendorLocationID, current.PurchaseUnit, current.CuryID);

				if (sp != null)
				{
					APSalesPrice copy = PXCache<APSalesPrice>.CreateCopy(sp);
					copy.PendingPrice = current.PendingPrice;
					copy.EffectiveDate = current.PendingDate;
					copy.SalesPrice = current.EffPrice;
					copy.LastDate = current.EffDate;
					copy.LastPrice = current.LastPrice;

					cache.Graph.RowUpdated.RemoveHandler<APSalesPrice>(APSalesPrice_RowUpdated);
					try
					{
						VendorSalesPrice.Update(copy);
					}
					finally
					{
						cache.Graph.RowUpdated.AddHandler<APSalesPrice>(APSalesPrice_RowUpdated);
					}
				}
			}
        }

        protected virtual void POVendorInventory_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            InventoryItem upd = PXCache<InventoryItem>.CreateCopy(this.Item.Current);
            POVendorInventory vendor = e.Row as POVendorInventory;
            object isdefault = cache.GetValueExt<POVendorInventory.isDefault>(e.Row);
            if (isdefault is PXFieldState)
            {
                isdefault = ((PXFieldState)isdefault).Value;
            }
            if ((bool?)isdefault == true)
            {
                upd.PreferredVendorID = null;
                upd.PreferredVendorLocationID = null;
                this.Item.Update(upd);
            }
			APSalesPrice sp = getAPSalesPrice(vendor);
			if (sp != null)
			{
				VendorSalesPrice.Delete(sp);
			}
        }
        #endregion POInventoryItem

        #region INItemXRef Event Handlers
        
        protected virtual void INItemXRef_InventoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (Item.Current != null)
            {
                e.NewValue = Item.Current.InventoryID;
                e.Cancel = true;
            }
        }

        protected virtual void INItemXRef_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void INItemXRef_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                if (((INItemXRef)e.Row).AlternateType != INAlternateType.VPN && ((INItemXRef)e.Row).AlternateType != INAlternateType.CPN)
                {
                    ((INItemXRef)e.Row).BAccountID = (int)0;
                }
                ((INItemXRef)e.Row).InventoryID = Item.Current.InventoryID;
                sender.Normalize();
            }
        }

        protected virtual void INItemXRef_BAccountID_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if (e.Row != null && (int?)e.ReturnValue == 0 && ((INItemXRef)e.Row).AlternateType != INAlternateType.VPN && ((INItemXRef)e.Row).AlternateType != INAlternateType.CPN)
            {
                e.ReturnValue = null;
                e.Cancel = true;
            }
        }

        protected virtual void INItemXRef_BAccountID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            if (e.Row != null && e.NewValue == null && ((INItemXRef)e.Row).AlternateType != INAlternateType.VPN && ((INItemXRef)e.Row).AlternateType != INAlternateType.CPN)
            {
                e.NewValue = (int)0;
                e.Cancel = true;
            }
        }

        protected virtual void INItemXRef_BAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (((INItemXRef)e.Row).AlternateType != INAlternateType.VPN && ((INItemXRef)e.Row).AlternateType != INAlternateType.CPN)
            {
                e.Cancel = true;
            }
        }
        
        #endregion
    }


    [Serializable]
	public partial class DFVendorInventory: IBqlTable
	{
		#region RecordID
		public abstract class recordID : PX.Data.IBqlField
		{
		}
		protected Int32? _RecordID;
		[PXDBInt(IsKey = true)]
		public virtual Int32? RecordID
		{
			get
			{
				return this._RecordID;
			}
			set
			{
				this._RecordID = value;
			}
		}
		#endregion
		#region AddLeadTimeDays
		public abstract class addLeadTimeDays : PX.Data.IBqlField
		{
		}
		protected Int16? _AddLeadTimeDays;
		[PXDefault((short)0)]
		[PXDBShort()]
		[PXUIField(DisplayName = "Add. Lead Time (Days)")]
		public virtual Int16? AddLeadTimeDays
		{
			get
			{
				return this._AddLeadTimeDays;
			}
			set
			{
				this._AddLeadTimeDays = value;
			}
		}
		#endregion
		#region VLeadTime
		public abstract class vLeadTime : PX.Data.IBqlField
		{
		}
		protected Int16? _VLeadTime;
		[PXShort(MinValue = 0, MaxValue = 100000)]
		[PXUIField(DisplayName = "Vendor Lead Time (Days)", Enabled = false)]		
		public virtual Int16? VLeadTime
		{
			get
			{
				return this._VLeadTime;
			}
			set
			{
				this._VLeadTime = value;
			}
		}
		#endregion
		#region Active
		public abstract class active : PX.Data.IBqlField
		{
		}
		protected Boolean? _Active;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? Active
		{
			get
			{
				return this._Active;
			}
			set
			{
				this._Active = value;
			}
		}
		#endregion
		#region MinOrdFreq
		public abstract class minOrdFreq : PX.Data.IBqlField
		{
		}
		protected Int32? _MinOrdFreq;
		[PXDBInt()]
		[PXUIField(DisplayName = "Min. Order Freq.(Days)")]
		[PXDefault(0)]
		public virtual Int32? MinOrdFreq
		{
			get
			{
				return this._MinOrdFreq;
			}
			set
			{
				this._MinOrdFreq = value;
			}
		}
		#endregion
		#region MinOrdQty
		public abstract class minOrdQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _MinOrdQty;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Min. Order Qty.")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? MinOrdQty
		{
			get
			{
				return this._MinOrdQty;
			}
			set
			{
				this._MinOrdQty = value;
			}
		}
		#endregion
		#region MaxOrdQty
		public abstract class maxOrdQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _MaxOrdQty;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Max Order Qty.")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? MaxOrdQty
		{
			get
			{
				return this._MaxOrdQty;
			}
			set
			{
				this._MaxOrdQty = value;
			}
		}
		#endregion
		#region LotSize
		public abstract class lotSize : PX.Data.IBqlField
		{
		}
		protected Decimal? _LotSize;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Lot Size")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LotSize
		{
			get
			{
				return this._LotSize;
			}
			set
			{
				this._LotSize = value;
			}
		}
		#endregion
		#region ERQ
		public abstract class eRQ : PX.Data.IBqlField
		{
		}
		protected Decimal? _ERQ;
		[PXDBQuantity]
		[PXUIField(DisplayName = "ERQ")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ERQ
		{
			get
			{
				return this._ERQ;
			}
			set
			{
				this._ERQ = value;
			}
		}
		#endregion		
	}

	[Serializable]
	[PXBreakInheritance]
	public partial class ARSalesPriceEx : ARSalesPrice
	{
		#region RecordID
		public new abstract class recordID : PX.Data.IBqlField
		{
		}
		#endregion
		#region CustPriceClassID
		public new abstract class custPriceClassID : PX.Data.IBqlField
		{
		}
		[PXDBString(10, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Customer Price Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[CustomerPriceClass]
		public override String CustPriceClassID
		{
			get
			{
				return this._CustPriceClassID;
			}
			set
			{
				this._CustPriceClassID = value;
			}
		}
		#endregion
		#region CustomerID
		public new abstract class customerID : PX.Data.IBqlField
		{
		}
		[Customer]
		[PXParent(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<ARSalesPriceEx.customerID>>>>))]
		public override Int32? CustomerID
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
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected new Int32? _InventoryID;
		[Inventory(DisplayName = "Inventory ID")]
		[PXDefault(typeof(InventoryItem.inventoryID))]
		public override Int32? InventoryID
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
	}

	#region MassUpdateFilter
	[Serializable]
	public partial class MassUpdateFilter : IBqlTable
	{
		#region EffectiveDate
		public abstract class effectiveDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _EffectiveDate;
		[PXDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Max. Pending Date", Required = true)]
		public virtual DateTime? EffectiveDate
		{
			get
			{
				return this._EffectiveDate;
			}
			set
			{
				this._EffectiveDate = value;
			}
		}
		#endregion
	}
	#endregion
}
