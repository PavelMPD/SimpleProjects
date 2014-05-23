using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.PO;
using ItemStats = PX.Objects.IN.Overrides.INDocumentRelease.ItemStats;
using System.Collections;

namespace PX.Objects.IN
{
	public class INItemSiteMaint : PXGraph<INItemSiteMaint, INItemSite>
	{
		public PXFilter<AP.Vendor> _Vendor_;
		public PXFilter<EP.EPEmployee> _Employee_;

		public PXSelectJoin<INItemSite, LeftJoin<INSite, On<INSite.siteID, Equal<INItemSite.siteID>>>, Where<INItemSite.inventoryID, Equal<Optional<INItemSite.inventoryID>>, And<INSite.siteID, IsNull, Or<Match<INSite, Current<AccessInfo.userName>>>>>> itemsiterecord;
		public PXSelectJoin<INItemSite, InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INItemSite.inventoryID>>>, Where<INItemSite.inventoryID, Equal<Current<INItemSite.inventoryID>>, And<INItemSite.siteID, Equal<Current<INItemSite.siteID>>>>> itemsitesettings;
		public PXSetup<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<INItemSite.inventoryID>>>> itemrecord;
		public PXSetup<INPostClass, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>> postclass;
		public PXSetup<INLotSerClass, Where<INLotSerClass.lotSerClassID, Equal<Current<InventoryItem.lotSerClassID>>>> lotserclass;
		public PXSetup<INSite, Where<INSite.siteID, Equal<Current<INItemSite.siteID>>>> insite;

		public INUnitSelect<INUnit, INItemSite.inventoryID, InventoryItem.itemClassID, INItemSite.dfltSalesUnit, INItemSite.dfltPurchaseUnit, InventoryItem.baseUnit, InventoryItem.lotSerClassID> itemunits;
		public PXSelect<INItemSiteReplenishment,
			Where<INItemSiteReplenishment.siteID, Equal<Current<INItemSite.siteID>>,
			And<INItemSiteReplenishment.inventoryID, Equal<Current<INItemSite.inventoryID>>>>> subitemrecords;

		public PXSetup<INSetup> insetup;

		public PXSelectJoin<POVendorInventory, 
			InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<POVendorInventory.inventoryID>>>> PreferredVendorItem;

		protected IEnumerable preferredVendorItem()
		{
			foreach (var item in PXSelectJoin<POVendorInventory,
			InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<POVendorInventory.inventoryID>>>,
			Where<POVendorInventory.inventoryID, Equal<Current<INItemSite.inventoryID>>,
				And<POVendorInventory.vendorID, Equal<Current<INItemSite.preferredVendorID>>,
				And<POVendorInventory.subItemID, Equal<Current<InventoryItem.defaultSubItemID>>,
				And<POVendorInventory.purchaseUnit, Equal<InventoryItem.purchaseUnit>,
				And<Where<POVendorInventory.vendorLocationID, Equal<Current<INItemSite.preferredVendorLocationID>>,
								Or<POVendorInventory.vendorLocationID, IsNull>>>>>>>,
				OrderBy<Desc<POVendorInventory.vendorLocationID,
							Asc<POVendorInventory.recordID>>>>.SelectSingleBound(this, null))
			{
				yield return item;
			}
		}
		public PXSelect<ItemStats> itemstats;

		public INItemSiteMaint()
		{
			INSetup record = insetup.Current;

			PXUIFieldAttribute.SetVisible<INUnit.toUnit>(itemunits.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<INUnit.toUnit>(itemunits.Cache, null, false);

			PXUIFieldAttribute.SetVisible<INUnit.sampleToUnit>(itemunits.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<INUnit.sampleToUnit>(itemunits.Cache, null, false);

			this.PreferredVendorItem.Cache.AllowUpdate = false;
			PXUIFieldAttribute.SetEnabled(this.PreferredVendorItem.Cache, null, false);			
		}

		protected virtual void INItemSite_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{       
			INItemSite itemsite = (INItemSite)e.Row;
			if (itemsite != null && itemsite.InventoryID != null && itemsite.SiteID != null)
			{
				InventoryItem item = itemrecord.Current;
				if (itemrecord.Current != null)
				{
					DefaultItemSiteByItem(this, itemsite, item, insite.Current, postclass.Current);					
				}
                this.itemrecord.Cache.IsDirty = true;
			}
		}


		public static void DefaultItemSiteByItem(PXGraph graph, INItemSite itemsite, InventoryItem item, INSite site, INPostClass postclass)
		{
			if (item != null)
			{
				itemsite.PendingStdCost = item.PendingStdCost;
				itemsite.PendingStdCostDate = item.PendingStdCostDate;
				itemsite.StdCost = item.StdCost;
				itemsite.StdCostDate = item.StdCostDate;
				itemsite.LastStdCost = item.LastStdCost;

				itemsite.PendingBasePrice = item.PendingBasePrice;
				itemsite.PendingBasePriceDate = item.PendingBasePriceDate;
				itemsite.BasePrice = item.BasePrice;
				itemsite.BasePriceDate = item.BasePriceDate;
				itemsite.LastBasePrice = item.LastBasePrice;

				itemsite.MarkupPct = item.MarkupPct;
				itemsite.RecPrice = item.RecPrice;

				itemsite.ABCCodeID = item.ABCCodeID;
				itemsite.ABCCodeIsFixed = item.ABCCodeIsFixed;
				itemsite.MovementClassID = item.MovementClassID;
				itemsite.MovementClassIsFixed = item.MovementClassIsFixed;

				itemsite.PreferredVendorID = item.PreferredVendorID;
				itemsite.PreferredVendorLocationID = item.PreferredVendorLocationID;
				itemsite.ReplenishmentClassID = site != null ? site.ReplenishmentClassID : null;
				itemsite.DfltReceiptLocationID = site.ReceiptLocationID;
				itemsite.DfltShipLocationID = site.ShipLocationID;

				DefaultItemReplenishment(graph, itemsite);
				DefaultSubItemReplenishment(graph, itemsite);
			}			
			if (itemsite.InvtAcctID == null)
			{
				if(site.OverrideInvtAccSub == true)
				{
					itemsite.InvtAcctID = site.InvtAcctID;
					itemsite.InvtSubID = site.InvtSubID;
				}
				else if(postclass != null)
				{
					itemsite.InvtAcctID = INReleaseProcess.GetAccountDefaults<INPostClass.invtAcctID>(graph, item, site, postclass);
					itemsite.InvtSubID = INReleaseProcess.GetAccountDefaults<INPostClass.invtSubID>(graph, item, site, postclass);
				}
				itemsite.StdCost = item.StdCost;
			}			
		}

		public static bool DefaultItemReplenishment(PXGraph graph, INItemSite itemsite)
		{			
			if (itemsite == null) return false;

			INItemRep rep = 
			(INItemRep)PXSelect<INItemRep,
				Where<INItemRep.inventoryID, Equal<Current<INItemSite.inventoryID>>,
				And<INItemRep.replenishmentClassID, Equal<Current<INItemSite.replenishmentClassID>>>>>
				.SelectSingleBound(graph, new object[] {itemsite}) 
				?? new INItemRep();

			if (itemsite.ReplenishmentPolicyOverride == false)
			{
				itemsite.ReplenishmentPolicyID = rep.ReplenishmentPolicyID;
				itemsite.ReplenishmentMethod = rep.ReplenishmentMethod ?? INReplenishmentMethod.None;
				itemsite.ReplenishmentSource = rep.ReplenishmentSource ?? INReplenishmentSource.None;
				itemsite.ReplenishmentSourceSiteID = rep.ReplenishmentSourceSiteID;				
			}
			
			if (itemsite.MaxShelfLifeOverride != true)
				itemsite.MaxShelfLife = rep.MaxShelfLife;

			if (itemsite.LaunchDateOverride != true)
				itemsite.LaunchDate = rep.LaunchDate;

			if (itemsite.TerminationDateOverride != true)
				itemsite.TerminationDate = rep.TerminationDate;

			if (itemsite.SafetyStockOverride != true)
				itemsite.SafetyStock = rep.SafetyStock;

			if(itemsite.ServiceLevelOverride != true)
				itemsite.ServiceLevel = rep.ServiceLevel;

			if (itemsite.MinQtyOverride != true)
				itemsite.MinQty = rep.MinQty;

			if (itemsite.MaxQtyOverride != true)
				itemsite.MaxQty = rep.MaxQty;

			if (itemsite.TransferERQOverride != true)
				itemsite.TransferERQ = rep.TransferERQ;

			return
				itemsite.ReplenishmentPolicyOverride != true ||
				itemsite.SafetyStockOverride != true ||
				itemsite.MaxShelfLifeOverride != true ||
				itemsite.LaunchDateOverride != true ||
				itemsite.TerminationDateOverride != true ||
				itemsite.ServiceLevelOverride != true ||
				itemsite.MinQtyOverride != true ||
				itemsite.MaxQtyOverride != true ||
				itemsite.TransferERQOverride != true;

		}

		public static void DefaultSubItemReplenishment(PXGraph graph, INItemSite itemsite)
		{
			if (itemsite == null) return;

			foreach (INItemSiteReplenishment r in 
				PXSelect<INItemSiteReplenishment,
						Where<INItemSiteReplenishment.siteID, Equal<Current<INItemSite.siteID>>,
						  And<INItemSiteReplenishment.inventoryID, Equal<Current<INItemSite.inventoryID>>>>>
							.SelectMultiBound(graph, new object[] {itemsite}))
			{
				graph.Caches[typeof(INItemSiteReplenishment)].Delete(r);	
			}				

			foreach (INSubItemRep r in
				PXSelect<INSubItemRep,
				Where<INSubItemRep.inventoryID, Equal<Current<INItemSite.inventoryID>>,
					And<INSubItemRep.replenishmentClassID, Equal<Current<INItemSite.replenishmentClassID>>>>>
				.Select(graph,itemsite.InventoryID))
			{
				INItemSiteReplenishment sr = new INItemSiteReplenishment();
				sr.InventoryID = r.InventoryID;
				sr.SiteID = itemsite.SiteID;
				sr.SubItemID = r.SubItemID;
				sr.SafetyStock = r.SafetyStock;
				sr.MinQty = r.MinQty;
				sr.MaxQty = r.MaxQty;
				sr.TransferERQ = r.TransferERQ;
				sr.ItemStatus = r.ItemStatus;
				graph.Caches[typeof(INItemSiteReplenishment)].Insert(sr);
			}
			if (graph.Caches[typeof(INItemSiteReplenishment)].IsDirty)
				graph.Views.Caches.Add(typeof(INItemSiteReplenishment));			
		}

		protected virtual void INItemSite_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			INItemSite itemsite = (INItemSite)e.Row;
			INItemSite olditemsite = (INItemSite)e.OldRow;
			InventoryItem item = itemrecord.Current;

			if (olditemsite.StdCostOverride == true && itemsite.StdCostOverride == false)
			{
				itemsite.PendingStdCost = item.PendingStdCost;
				itemsite.PendingStdCostDate = item.PendingStdCostDate;
				itemsite.StdCost = item.StdCost;
				itemsite.StdCostDate = item.StdCostDate;
				itemsite.LastStdCost = item.LastStdCost;
			}

			if (olditemsite.BasePriceOverride == true && itemsite.BasePriceOverride == false)
			{
				itemsite.PendingBasePrice = item.PendingBasePrice;
				itemsite.PendingBasePriceDate = item.PendingBasePriceDate;
				itemsite.BasePrice = item.BasePrice;
				itemsite.BasePriceDate = item.BasePriceDate;
				itemsite.LastBasePrice = item.LastBasePrice;
			}

			if (olditemsite.ABCCodeOverride == true && itemsite.ABCCodeOverride == false)
			{
				itemsite.ABCCodeID = item.ABCCodeID;
				itemsite.ABCCodeIsFixed = item.ABCCodeIsFixed;
			}

			if (olditemsite.MovementClassOverride == true && itemsite.MovementClassOverride == false)
			{
				itemsite.ABCCodeID = item.MovementClassID;
				itemsite.ABCCodeIsFixed = item.MovementClassIsFixed;
			}

			if (olditemsite.RecPriceOverride == true && itemsite.RecPriceOverride == false)
			{
				itemsite.RecPrice = item.RecPrice;
			}
			if (olditemsite.MarkupPctOverride == true && itemsite.MarkupPctOverride == false)
			{
				itemsite.MarkupPct = item.MarkupPct;
			}
			if (olditemsite.PreferredVendorOverride == true && itemsite.PreferredVendorOverride == false)
			{
				itemsite.PreferredVendorID = item.PreferredVendorID;
				itemsite.PreferredVendorLocationID = item.PreferredVendorLocationID;
			}		

			INItemSiteMaint.DefaultItemReplenishment(this, itemsite);

			foreach (ItemStats stats in itemstats.Cache.Inserted)
			{
				itemstats.Cache.Delete(stats);
			}

            if (itemsite.PreferredVendorID == null)
            {
                itemsite.PreferredVendorLocationID = null;
            }

			if (itemsite.LastCostDate != null && itemsite.LastCost != null && itemsite.LastCost != 0m)
			{
				ItemStats stats = new ItemStats();
				stats.InventoryID = itemsite.InventoryID;
				stats.SiteID = itemsite.SiteID;

				stats = itemstats.Insert(stats);

				stats.LastCost = itemsite.LastCost;
				stats.LastCostDate = itemsite.LastCostDate;
			}
			if((olditemsite.ReplenishmentClassID != itemsite.ReplenishmentClassID && itemsite.SubItemOverride != true) ||
				 (olditemsite.SubItemOverride == true && itemsite.SubItemOverride == false))
				DefaultSubItemReplenishment(this, itemsite);			
			
			if(itemsite.PreferredVendorOverride == true && item.DefaultSubItemID != null &&
				(itemsite.PreferredVendorID != olditemsite.PreferredVendorID ||
				 itemsite.PreferredVendorLocationID != olditemsite.PreferredVendorLocationID))
			{
				POVendorInventory rec = PXSelect<POVendorInventory,
					Where<POVendorInventory.inventoryID, Equal<Current<INItemSite.inventoryID>>,
						And<POVendorInventory.subItemID, Equal<Current<InventoryItem.defaultSubItemID>>,
						And<POVendorInventory.purchaseUnit, Equal<Current<InventoryItem.purchaseUnit>>,
						And<POVendorInventory.vendorID, Equal<Current<INItemSite.preferredVendorID>>,
						And<Where2<Where<Current<INItemSite.preferredVendorLocationID>, IsNotNull, 
										And<POVendorInventory.vendorLocationID, Equal<Current<INItemSite.preferredVendorLocationID>>>>,
						  Or<Where<Current<INItemSite.preferredVendorLocationID>, IsNull, 
									 And<POVendorInventory.vendorLocationID,IsNull>>>>>>>>>>
					.SelectSingleBound(this, new object[]{itemsite, item});
				
				if(rec == null)
				{
					rec = new POVendorInventory();
					rec.InventoryID = item.InventoryID;
					rec.SubItemID = item.DefaultSubItemID;
					rec.PurchaseUnit = item.PurchaseUnit;
					rec.VendorID = itemsite.PreferredVendorID;
					rec.VendorLocationID = itemsite.PreferredVendorLocationID;					
					this.PreferredVendorItem.Insert(rec);			
				}
			}
			
           


		}

		protected virtual void INItemSite_LastCost_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			((INItemSite)e.Row).LastCostDate = this.Accessinfo.BusinessDate;
		}
		
		protected virtual void INItemSite_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            INItemSite row = (INItemSite)e.Row;
			
			bool isTransfer = (e.Row != null) && INReplenishmentSource.IsTransfer(row.ReplenishmentSource);
			bool isFixedReorderQty = (e.Row != null) && (row.ReplenishmentMethod == INReplenishmentMethod.FixedReorder);
			PXUIFieldAttribute.SetEnabled<INItemSite.pendingStdCost>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).StdCostOverride));
			PXUIFieldAttribute.SetEnabled<INItemSite.pendingStdCostDate>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).StdCostOverride));

			PXUIFieldAttribute.SetEnabled<INItemSite.pendingBasePrice>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).BasePriceOverride));
			PXUIFieldAttribute.SetEnabled<INItemSite.pendingBasePriceDate>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).BasePriceOverride));

			PXUIFieldAttribute.SetEnabled<INItemSite.aBCCodeID>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).ABCCodeOverride));
			PXUIFieldAttribute.SetEnabled<INItemSite.aBCCodeIsFixed>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).ABCCodeOverride));

			PXUIFieldAttribute.SetEnabled<INItemSite.movementClassID>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).MovementClassOverride));
			PXUIFieldAttribute.SetEnabled<INItemSite.movementClassIsFixed>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).MovementClassOverride));

			PXUIFieldAttribute.SetEnabled<INItemSite.preferredVendorID>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).PreferredVendorOverride));
			PXUIFieldAttribute.SetEnabled<INItemSite.preferredVendorLocationID>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).PreferredVendorOverride));

			PXUIFieldAttribute.SetEnabled<INItemSite.replenishmentPolicyID>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).ReplenishmentPolicyOverride));
			PXUIFieldAttribute.SetEnabled<INItemSite.replenishmentMethod>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).ReplenishmentPolicyOverride));
			PXUIFieldAttribute.SetEnabled<INItemSite.replenishmentSource>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).ReplenishmentPolicyOverride));
			PXUIFieldAttribute.SetEnabled<INItemSite.replenishmentSourceSiteID>(sender, e.Row,
				(e.Row != null && (bool)((INItemSite)e.Row).ReplenishmentPolicyOverride) && INReplenishmentSource.IsTransfer(((INItemSite)e.Row).ReplenishmentSource));

			PXUIFieldAttribute.SetEnabled<INItemSite.maxShelfLife>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).MaxShelfLifeOverride));
			PXUIFieldAttribute.SetEnabled<INItemSite.launchDate>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).LaunchDateOverride));
			PXUIFieldAttribute.SetEnabled<INItemSite.terminationDate>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).TerminationDateOverride));
			PXUIFieldAttribute.SetEnabled<INItemSite.serviceLevel>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).ServiceLevelOverride));
			PXUIFieldAttribute.SetEnabled<INItemSite.serviceLevelPct>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).ServiceLevelOverride));
			PXUIFieldAttribute.SetEnabled<INItemSite.safetyStock>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).SafetyStockOverride));
			PXUIFieldAttribute.SetEnabled<INItemSite.minQty>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).MinQtyOverride));
			PXUIFieldAttribute.SetEnabled<INItemSite.maxQty>(sender, e.Row, (e.Row != null && (bool)((INItemSite)e.Row).MaxQtyOverride ));
			PXUIFieldAttribute.SetEnabled<INItemSite.transferERQ>(sender, e.Row, (row != null && row.TransferERQOverride == true && isTransfer==true && isFixedReorderQty));
			PXUIFieldAttribute.SetEnabled<INItemSite.transferERQOverride>(sender, e.Row, (row != null && isTransfer == true && isFixedReorderQty));
			PXUIFieldAttribute.SetEnabled<INItemSite.markupPct>(sender, e.Row, (e.Row != null && ((INItemSite)e.Row).MarkupPctOverride == true));
			PXUIFieldAttribute.SetEnabled<INItemSite.recPrice>(sender, e.Row, (e.Row != null && ((INItemSite)e.Row).RecPriceOverride == true));

		
			this.subitemrecords.Cache.AllowInsert =
				this.subitemrecords.Cache.AllowUpdate =
				this.subitemrecords.Cache.AllowDelete = ((INItemSite)e.Row).SubItemOverride == true;
			this.updateReplenishment.SetEnabled(this.subitemrecords.Cache.AllowInsert);
			if (isTransfer && row.ReplenishmentSourceSiteID == row.SiteID)
			{
				sender.RaiseExceptionHandling<INItemSite.replenishmentSourceSiteID>(e.Row,row.ReplenishmentSourceSiteID,new PXSetPropertyException(Messages.ReplenishmentSourceSiteMustBeDifferentFromCurrenSite, PXErrorLevel.Warning)); 
			}
			else 
			{
				sender.RaiseExceptionHandling<INItemSite.replenishmentSourceSiteID>(e.Row,row.ReplenishmentSourceSiteID,null); 
			}
			this.itemrecord.Cache.IsDirty = false;

		}

		public override int Persist(Type cacheType, PXDBOperation operation)
		{
			if (cacheType == typeof(INUnit) && operation == PXDBOperation.Update)
			{
				base.Persist(cacheType, PXDBOperation.Insert);
			}
			return base.Persist(cacheType, operation);
		}

		protected virtual void INItemSite_DfltShipLocationID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null) return;

			INItemSite is_row = (INItemSite)e.Row;

			INLocation l = PXSelect<INLocation, Where<INLocation.locationID, Equal<Required<INLocation.locationID>>>>.Select(this, e.NewValue);
			if (l == null) return;
			if (!(l.SalesValid ?? true))
			{
				if (itemsiterecord.Ask(AP.Messages.Warning, Messages.IssuesAreNotAllowedFromThisLocationContinue, MessageButtons.YesNo, false) == WebDialogResult.No)
				{
					e.NewValue = is_row.DfltShipLocationID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void INItemSiteReplenishment_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INItemSiteReplenishment row = (INItemSiteReplenishment)e.Row;
			PXUIFieldAttribute.SetEnabled<INItemSiteReplenishment.safetyStockSuggested>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<INItemSiteReplenishment.minQtySuggested>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<INItemSiteReplenishment.maxQtySuggested>(sender, null, false);			
			PXUIFieldAttribute.SetEnabled<INItemSiteReplenishment.demandPerDayAverage>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<INItemSiteReplenishment.demandPerDayMSE>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<INItemSiteReplenishment.demandPerDayMAD>(sender, null, false);		
		}
		public PXAction<INItemSite> updateReplenishment;
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		[PXUIField(DisplayName = "Default Settings", MapEnableRights = PXCacheRights.Update)]
		protected virtual IEnumerable UpdateReplenishment(PXAdapter adapter)
		{
			foreach (PXResult<INItemSite> r in adapter.Get())
			{
				INItemSite s = r;
				if (s.SubItemOverride == true && insetup.Current.UseInventorySubItem == true)
					foreach (INItemSiteReplenishment rep in this.subitemrecords.View.SelectMulti(new object[]{s}))
					{
						INItemSiteReplenishment upd = PXCache<INItemSiteReplenishment>.CreateCopy(rep);
						upd.SafetyStock = s.SafetyStock ?? 0m;
						upd.MinQty = s.MinQty ?? 0m;
						upd.MaxQty = s.MaxQty ?? 0m;
						upd.TransferERQ = s.TransferERQ ?? 0m;
						this.subitemrecords.Update(upd);
					}
				yield return s;
			}
		}
	}
}
