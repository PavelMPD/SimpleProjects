using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.AP;
using PX.Objects.CS;
using SOLine3 = PX.Objects.PO.POOrderEntry.SOLine3;
using PX.Objects.SO;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.TM;
using PX.Objects.CM;

namespace PX.Objects.PO
{
	[PX.Objects.GL.TableAndChartDashboardType]
    [Serializable]
	public class POCreate : PXGraph<POCreate>
	{
		public PXCancel<POCreateFilter> Cancel;
		public PXFilter<POCreateFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessingJoin<POFixedDemand, POCreateFilter,
			InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<POFixedDemand.inventoryID>>,
			LeftJoin<Vendor, On<Vendor.bAccountID, Equal<POFixedDemand.vendorID>>,
			LeftJoin<POVendorInventory,
			      On<POVendorInventory.recordID, Equal<POFixedDemand.recordID>>,
			LeftJoin<Location, On<Location.bAccountID, Equal<POFixedDemand.vendorID>, And<Location.locationID, Equal<POFixedDemand.vendorLocationID>>>,
			LeftJoin<SOOrder, On<SOOrder.noteID, Equal<POFixedDemand.refNoteID>>,
			LeftJoin<SOLine, On<SOLine.planID, Equal<POFixedDemand.planID>>>>>>>>,
			Where2<Where<POFixedDemand.vendorID, Equal<Current<POCreateFilter.vendorID>>, Or<Current<POCreateFilter.vendorID>, IsNull>>,
			And2<Where<POFixedDemand.inventoryID, Equal<Current<POCreateFilter.inventoryID>>, Or<Current<POCreateFilter.inventoryID>, IsNull>>,
			And2<Where<POFixedDemand.siteID, Equal<Current<POCreateFilter.siteID>>, Or<Current<POCreateFilter.siteID>, IsNull>>,
			And2<Where<SOOrder.customerID, Equal<Current<POCreateFilter.customerID>>, Or<Current<POCreateFilter.customerID>, IsNull>>,
			And2<Where<SOOrder.orderType, Equal<Current<POCreateFilter.orderType>>, Or<Current<POCreateFilter.orderType>, IsNull>>,
			And2<Where<SOOrder.orderNbr, Equal<Current<POCreateFilter.orderNbr>>, Or<Current<POCreateFilter.orderNbr>, IsNull>>,
			And<Where<InventoryItem.itemClassID, Equal<Current<POCreateFilter.itemClassID>>, Or<Current<POCreateFilter.itemClassID>, IsNull>>>>>>>>>> FixedDemand;
		public POCreate()
		{
			PXUIFieldAttribute.SetEnabled<POFixedDemand.orderQty>(FixedDemand.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<POFixedDemand.fixedSource>(FixedDemand.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<POFixedDemand.replenishmentSourceSiteID>(FixedDemand.Cache, null, true);						
			PXUIFieldAttribute.SetEnabled<POFixedDemand.vendorID>(FixedDemand.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<POFixedDemand.vendorLocationID>(FixedDemand.Cache, null, true);

			PXUIFieldAttribute.SetDisplayName<InventoryItem.descr>(this.Caches[typeof(InventoryItem)], Messages.InventoryItemDescr);
			PXUIFieldAttribute.SetDisplayName<INSite.descr>(this.Caches[typeof(INSite)], Messages.SiteDescr);
			PXUIFieldAttribute.SetDisplayName<Vendor.acctName>(this.Caches[typeof(Vendor)], Messages.VendorAcctName);
			PXUIFieldAttribute.SetDisplayName<Customer.acctName>(this.Caches[typeof(Customer)], Messages.CustomerAcctName);
			PXUIFieldAttribute.SetDisplayName<SOOrder.customerLocationID>(this.Caches[typeof(SOOrder)], Messages.CustomerLocationID);
			PXUIFieldAttribute.SetDisplayName<INPlanType.descr>(this.Caches[typeof(INPlanType)], Messages.PlanTypeDescr);

			PXUIFieldAttribute.SetDisplayName<POVendorInventory.effPrice>(this.Caches[typeof(POVendorInventory)], Messages.VendorPrice);
			PXUIFieldAttribute.SetRequired<POVendorInventory.effPrice>(this.Caches[typeof(POVendorInventory)], false);

			PXUIFieldAttribute.SetDisplayName<SOLine.curyUnitPrice>(this.Caches[typeof(SOLine)], Messages.CustomerPrice);
			PXUIFieldAttribute.SetDisplayName<SOLine.unitPrice>(this.Caches[typeof(SOLine)], Messages.CustomerPrice);
			PXUIFieldAttribute.SetDisplayName<SOLine.uOM>(this.Caches[typeof(SOLine)], Messages.CustomerPriceUOM);
			PXUIFieldAttribute.SetRequired<SOLine.uOM>(this.Caches[typeof(SOLine)], false);

			PXUIFieldAttribute.SetDisplayName<POLine.orderNbr>(this.Caches[typeof(POLine)], Messages.POLineOrderNbr);
		}
		protected IEnumerable filter()
		{
			POCreateFilter filter = this.Filter.Current;
			filter.OrderVolume = 0;
			filter.OrderWeight = 0;
			filter.OrderTotal = 0;
			foreach(POFixedDemand demand in this.FixedDemand.Cache.Updated)
				if(demand.Selected == true)
				{
					filter.OrderVolume += demand.ExtVolume ?? 0m;
					filter.OrderWeight += demand.ExtWeight ?? 0m;
					filter.OrderTotal  += demand.ExtCost ?? 0m;
				}
			yield return filter;
		}
		protected IEnumerable fixedDemand()
		{
			foreach(PXResult<POFixedDemand,InventoryItem,Vendor,POVendorInventory> rec in 
				PXSelectJoin<POFixedDemand,
				InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<POFixedDemand.inventoryID>>,
				LeftJoin<Vendor, On<Vendor.bAccountID, Equal<POFixedDemand.vendorID>>,
				LeftJoin<POVendorInventory,
					  On<POVendorInventory.recordID, Equal<POFixedDemand.recordID>>,	
				LeftJoin<Location, On<Location.bAccountID, Equal<POFixedDemand.vendorID>, And<Location.locationID, Equal<POFixedDemand.vendorLocationID>>>,
				LeftJoin<SOOrder, On<SOOrder.noteID, Equal<POFixedDemand.refNoteID>>,
				LeftJoin<SOLine, On<SOLine.planID, Equal<POFixedDemand.planID>>>>>>>>,
				Where2<Where<POFixedDemand.vendorID, Equal<Current<POCreateFilter.vendorID>>, Or<Current<POCreateFilter.vendorID>, IsNull>>,
				And2<Where<POFixedDemand.inventoryID, Equal<Current<POCreateFilter.inventoryID>>, Or<Current<POCreateFilter.inventoryID>, IsNull>>,
				And2<Where<POFixedDemand.siteID, Equal<Current<POCreateFilter.siteID>>, Or<Current<POCreateFilter.siteID>, IsNull>>,
				And2<Where<POFixedDemand.replenishmentSourceSiteID, Equal<Current<POCreateFilter.replenishmentSourceSiteID>>, Or<Current<POCreateFilter.replenishmentSourceSiteID>, IsNull>>,				
				And2<Where<SOOrder.customerID, Equal<Current<POCreateFilter.customerID>>, Or<Current<POCreateFilter.customerID>, IsNull>>,
				And2<Where<SOOrder.orderType, Equal<Current<POCreateFilter.orderType>>, Or<Current<POCreateFilter.orderType>, IsNull>>,
				And2<Where<SOOrder.orderNbr, Equal<Current<POCreateFilter.orderNbr>>, Or<Current<POCreateFilter.orderNbr>, IsNull>>,
				And<Where<InventoryItem.itemClassID, Equal<Current<POCreateFilter.itemClassID>>, Or<Current<POCreateFilter.itemClassID>, IsNull>>>>>>>>>>>.Select(this))
			{
				POFixedDemand demand = rec;
				POVendorInventory price = rec;
				InventoryItem item = rec;
				if (demand.RecordID != null && demand.EffPrice == null)
				{
					demand.EffPrice = price.EffPrice;
					demand.AddLeadTimeDays = price.AddLeadTimeDays;
				}
				demand.UnitWeight = item.BaseWeight;
				demand.UnitVolume = item.BaseVolume;
				FixedDemand.Cache.RaiseFieldUpdated<POFixedDemand.orderQty>(demand, null);
				yield return rec;
			}

		}	
		protected virtual void POCreateFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			POCreateFilter filter = (POCreateFilter)e.Row;
			if(filter == null) return;

			if (filter.VendorID != null &&
			   !sender.ObjectsEqual<POCreateFilter.vendorID>(e.Row, e.OldRow))
				filter.ReplenishmentSourceSiteID = null;

			if (filter.ReplenishmentSourceSiteID != null &&
			   !sender.ObjectsEqual<POCreateFilter.replenishmentSourceSiteID>(e.Row, e.OldRow))
				filter.VendorID = null;
		}
		protected virtual void POCreateFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			POCreateFilter filter = Filter.Current;

			if (filter == null) return;

			FixedDemand.SetProcessDelegate(delegate(List<POFixedDemand> list)
			{
				CreateProc(list, filter.PurchDate, filter.OrderNbr != null);
			});

			TimeSpan span;
			Exception message;
			PXLongRunStatus status = PXLongOperation.GetStatus(this.UID, out span, out message);

			PXUIFieldAttribute.SetVisible<POLine.orderNbr>(Caches[typeof(POLine)], null, (status == PXLongRunStatus.Completed || status == PXLongRunStatus.Aborted));
			PXUIFieldAttribute.SetVisible<POCreateFilter.orderTotal>(sender, null, filter.VendorID != null);
		}

		protected virtual void POFixedDemand_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			POFixedDemand row = (POFixedDemand)e.Row;
			if(row != null && row.Selected != true 
				&& sender.ObjectsEqual<POFixedDemand.selected>(e.Row, e.OldRow))
			{
				row.Selected = true;
			}
		}
		protected virtual void POFixedDemand_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			POFixedDemand row = e.Row as POFixedDemand;
			if (row == null) return;

			PXUIFieldAttribute.SetEnabled<POFixedDemand.orderQty>(sender, row, row.PlanType == INPlanConstants.Plan90);
			PXUIFieldAttribute.SetEnabled<POFixedDemand.fixedSource>(FixedDemand.Cache, row, row.PlanType == INPlanConstants.Plan90);			
			PXUIFieldAttribute.SetEnabled<POFixedDemand.replenishmentSourceSiteID>(sender, row, row.FixedSource == INReplenishmentSource.Transfer);			
			PXUIFieldAttribute.SetEnabled<POFixedDemand.vendorID>(sender, row, row.FixedSource == INReplenishmentSource.Purchased);
			PXUIFieldAttribute.SetEnabled<POFixedDemand.vendorLocationID>(sender, row, row.FixedSource == INReplenishmentSource.Purchased);			
		}

		protected virtual void POFixedDemand_VendorLocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			POFixedDemand row = (POFixedDemand )e.Row;
			if(row != null)
			{
				e.NewValue =
					PX.Objects.PO.POItemCostManager.FetchLocation(
						this,
						row.VendorID,
						row.InventoryID,
						row.SubItemID,
						row.SiteID);
				e.Cancel = true;
			}
		}
		protected virtual void POFixedDemand_OrderQty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			POFixedDemand row = (POFixedDemand )e.Row;
			if (row != null && row.PlanUnitQty < (Decimal?)e.NewValue)
			{
				e.NewValue = row.PlanUnitQty;
				sender.RaiseExceptionHandling<POFixedDemand.orderQty>(row, null,
				                                                      new PXSetPropertyException<POFixedDemand.orderQty>(
				                                                      	Messages.POOrderQtyValidation, PXErrorLevel.Warning));
			}
		}
		protected virtual void POFixedDemand_RecordID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			POFixedDemand row = (POFixedDemand)e.Row;
			POVendorInventory result = null;
			if (row == null) return;
			foreach (PXResult<POVendorInventory, BAccountR, InventoryItem> rec in 
				PXSelectJoin<POVendorInventory,
				InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<POVendorInventory.vendorID>>,
					InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<POVendorInventory.inventoryID>>>>,
				Where<POVendorInventory.vendorID, Equal<Current<POFixedDemand.vendorID>>,
				And<POVendorInventory.inventoryID, Equal<Current<POFixedDemand.inventoryID>>,
				And<POVendorInventory.active, Equal<boolTrue>,
				And2<Where<POVendorInventory.vendorLocationID, Equal<Current<POFixedDemand.vendorLocationID>>,
					    Or<POVendorInventory.vendorLocationID, IsNull>>,
					  And<Where<POVendorInventory.subItemID, Equal<Current<POFixedDemand.subItemID>>,
						     Or<POVendorInventory.subItemID, Equal<InventoryItem.defaultSubItemID>>>>>>>>>
				.SelectMultiBound(this, new object[] {e.Row}))
			{
				POVendorInventory price = rec;
				InventoryItem item = rec;
				if (price.VendorLocationID == row.VendorLocationID && 
					price.SubItemID == row.SubItemID)
				{
					result = price;
					break;
				}

				if (price.VendorLocationID == row.VendorLocationID)
					result = price;

				if (result != null && result.VendorLocationID != row.VendorLocationID &&
					price.SubItemID == row.SubItemID)
					result = price;

				if (result == null)
					result = price;
			}
			if(result != null)
			{
				e.NewValue = result.RecordID;
				e.Cancel = true;
			}
			

		}
		protected virtual void POFixedDemand_RecordID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			POFixedDemand row = (POFixedDemand )e.Row;
			if (row != null )
			{
				POVendorInventory price = PXSelect<POVendorInventory,
					Where<POVendorInventory.recordID, 
					Equal<Required<POVendorInventory.recordID>>>>
					.SelectSingleBound(this, null, row.RecordID);
				row.EffPrice = (price != null) ? price.EffPrice : 0;
				row.AddLeadTimeDays = (price != null) ? price.AddLeadTimeDays : null;
				FixedDemand.Cache.RaiseFieldUpdated<POFixedDemand.effPrice>(row, null);				
			}
		}

		public static void CreateProc(List<POFixedDemand> list, DateTime? orderDate, bool extSort)
		{
			PXRedirectRequiredException poredirect = CreatePOOrders(list, orderDate, extSort);
			PXRedirectRequiredException soredirect = CreateSOOrders(list, orderDate);
			if (poredirect != null && soredirect == null)
				throw poredirect;
			if (poredirect == null && soredirect != null)
				throw soredirect;

		}

		public static PXRedirectRequiredException CreatePOOrders(List<POFixedDemand> list, DateTime? PurchDate, bool extSort)
		{
			POOrderEntry docgraph = PXGraph.CreateInstance<POOrderEntry>();
			docgraph.Views.Caches.Add(typeof(SOLine3));
			POSetup setup = docgraph.POSetup.Current;

			DocumentList<POOrder> created = new DocumentList<POOrder>(docgraph);
			DocumentList<POLine> ordered = new DocumentList<POLine>(docgraph);

			if (extSort)
				list.Sort((a, b) =>
					{
						SOLine3 aline = PXSelect<SOLine3, Where<SOLine3.planID, Equal<Required<SOLine3.planID>>>>.Select(docgraph, a.PlanID);
						SOLine3 bline = PXSelect<SOLine3, Where<SOLine3.planID, Equal<Required<SOLine3.planID>>>>.Select(docgraph, b.PlanID);

						int aSortOrder = 0;
						int bSortOrder = 0;

						if (aline != null && bline != null)
						{
							aSortOrder += (1 + ((IComparable)aline.OrderType).CompareTo(bline.OrderType)) / 2 * 100;
							bSortOrder += (1 - ((IComparable)aline.OrderType).CompareTo(bline.OrderType)) / 2 * 100;

							aSortOrder += (1 + ((IComparable)aline.OrderNbr).CompareTo(bline.OrderNbr)) / 2 * 10;
							bSortOrder += (1 - ((IComparable)aline.OrderNbr).CompareTo(bline.OrderNbr)) / 2 * 10;

							aSortOrder += (1 + ((IComparable)aline.LineNbr).CompareTo(bline.LineNbr)) / 2;
							bSortOrder += (1 - ((IComparable)aline.LineNbr).CompareTo(bline.LineNbr)) / 2;
						}

						return aSortOrder.CompareTo(bSortOrder);
					});

			foreach (POFixedDemand demand in list)
			{
				if (demand.FixedSource != INReplenishmentSource.Purchased) continue;

				string OrderType =
					demand.PlanType == INPlanConstants.Plan6D ? POOrderType.DropShip :
					demand.PlanType == INPlanConstants.Plan6E ? POOrderType.DropShip :
					demand.PlanType == INPlanConstants.Plan6T ? POOrderType.Transfer :
					POOrderType.RegularOrder;
				string replanType = null;

				if (demand.VendorID == null || demand.VendorLocationID == null)
				{
					PXProcessing<POFixedDemand>.SetWarning(list.IndexOf(demand), Messages.MissingVendorOrLocation);
					continue;
				}			

				PXErrorLevel ErrorLevel = PXErrorLevel.RowInfo;
				string ErrorText = string.Empty;

				try
				{
					POOrder order;
					SOOrder soorder = PXSelect<SOOrder, Where<SOOrder.noteID, Equal<Required<SOOrder.noteID>>>>.Select(docgraph, demand.RefNoteID);
					SOLine3 soline = PXSelect<SOLine3, Where<SOLine3.planID, Equal<Required<SOLine3.planID>>>>.Select(docgraph, demand.PlanID);

					string BLType = null;
					string BLOrderNbr = null;

					if (demand.PlanType == INPlanConstants.Plan6B ||
							demand.PlanType == INPlanConstants.Plan6E)
					{
						BLType = soline.POType;
						BLOrderNbr = soline.PONbr;
					}

					if (OrderType == POOrderType.RegularOrder || OrderType == POOrderType.Transfer)
					{
						order = created.Find<POOrder.orderType, POOrder.vendorID, POOrder.vendorLocationID, POOrder.siteID, POOrder.bLOrderNbr>(OrderType, demand.VendorID, demand.VendorLocationID, demand.SiteID, BLOrderNbr) ?? new POOrder();
					}
					else if (OrderType == POOrderType.DropShip)
					{
						order = created.Find<POOrder.orderType, POOrder.vendorID, POOrder.vendorLocationID, POOrder.siteID, POOrder.bLOrderNbr, POOrder.sOOrderType, POOrder.sOOrderNbr>
							(OrderType, demand.VendorID, demand.VendorLocationID, demand.SiteID, BLOrderNbr, soline.OrderType, soline.OrderNbr)
							?? new POOrder();
					}
					else
					{
						order = created.Find<POOrder.orderType, POOrder.vendorID, POOrder.vendorLocationID, POOrder.shipToBAccountID, POOrder.shipToLocationID, POOrder.siteID, POOrder.bLOrderNbr>
							(OrderType, demand.VendorID, demand.VendorLocationID, soorder.CustomerID, soorder.CustomerLocationID, demand.SiteID, BLOrderNbr)
							?? new POOrder();
					}

					if (order.OrderNbr == null)
					{
						docgraph.Clear();

						order.OrderType = OrderType;
						order = PXCache<POOrder>.CreateCopy(docgraph.Document.Insert(order));
						order.VendorID = demand.VendorID;
						order.VendorLocationID = demand.VendorLocationID;
						order.SiteID = demand.SiteID;
						order.OrderDate = OrderType == POOrderType.DropShip ? demand.PlanDate : PurchDate;
						order.BLType = BLType;
						order.BLOrderNbr = BLOrderNbr;

						if (OrderType == POOrderType.DropShip || extSort)
						{
							order.SOOrderType = soline.OrderType;
							order.SOOrderNbr = soline.OrderNbr;
						}

						if (!string.IsNullOrEmpty(order.BLOrderNbr))
						{
							POOrder blanket = PXSelect<POOrder, Where<POOrder.orderType, Equal<Current<POOrder.bLType>>, And<POOrder.orderNbr, Equal<Current<POOrder.bLOrderNbr>>>>>.SelectSingleBound(docgraph, new object[] { order });
							if (blanket != null)
							{
								order.VendorRefNbr = blanket.VendorRefNbr;
							}
						}

						if (OrderType == POOrderType.DropShip)
						{
							order.ShipDestType = POShippingDestination.Customer;
							order.ShipToBAccountID = soorder.CustomerID;
							order.ShipToLocationID = soorder.CustomerLocationID;
						}
						else if (setup.ShipDestType == POShipDestType.Site)
						{
							order.ShipDestType = POShippingDestination.Site;
							order.SiteID = demand.SiteID;
						}

						if (docgraph.CMSetup.Current.MCActivated == true && OrderType != POOrderType.Transfer)
						{
							//GetValuePending will fall in CurrencyInfo_CuryIdFieldSelecting()
							docgraph.currencyinfo.Current.CuryID = null;
						}

						order = docgraph.Document.Update(order);

                        if (OrderType == POOrderType.DropShip)
                        {
                            SOAddress soAddress = PXSelect<SOAddress, Where<SOAddress.addressID, Equal<Required<SOOrder.shipAddressID>>>>.Select(docgraph, soorder.ShipAddressID);

                            if (soAddress.IsDefaultAddress == false)
                            {
                                AddressAttribute.CopyRecord<POOrder.shipAddressID>(docgraph.Document.Cache, order, soAddress, true);
                            }
                            SOContact soContact = PXSelect<SOContact, Where<SOContact.contactID, Equal<Required<SOOrder.shipContactID>>>>.Select(docgraph, soorder.ShipContactID);

                            if (soContact.IsDefaultContact == false)
                            {
                                ContactAttribute.CopyRecord<POOrder.shipContactID>(docgraph.Document.Cache, order, soContact, true);
                            }
                        }
                    }
					else if (docgraph.Document.Cache.ObjectsEqual(docgraph.Document.Current, order) == false)
					{
						docgraph.Document.Current = docgraph.Document.Search<POOrder.orderNbr>(order.OrderNbr, order.OrderType);
					}

					POLine line = null;
					//Sales Orders to Blanket should not be grouped together
					//Drop Ships to Blankets are not grouped either
					if (OrderType == POOrderType.RegularOrder && demand.PlanType != INPlanConstants.Plan6B)
					{
						line = setup.CopyLineDescrSO == true && soline != null
								? ordered.Find
									<POLine.vendorID, POLine.vendorLocationID, POLine.siteID, POLine.inventoryID, POLine.subItemID,
									POLine.requestedDate, POLine.tranDesc>
									(demand.VendorID, demand.VendorLocationID, demand.SiteID, demand.InventoryID, demand.SubItemID,
									 soline != null ? soline.ShipDate : null, soline.TranDesc)
								: ordered.Find
									<POLine.vendorID, POLine.vendorLocationID, POLine.siteID, POLine.inventoryID, POLine.subItemID,
									POLine.requestedDate>
									(demand.VendorID, demand.VendorLocationID, demand.SiteID, demand.InventoryID, demand.SubItemID,
									 soline != null ? soline.ShipDate : null);

					}
					if (line == null) line = new POLine();

					if (line.OrderNbr == null)
					{
						if (demand.PlanType == INPlanConstants.Plan90)
							line.LineType = POLineType.GoodsForReplenishment;
						else if (OrderType == POOrderType.RegularOrder || OrderType == POOrderType.Transfer)
						{
							if (soline != null)
								line.LineType = (soline.LineType == SO.SOLineType.Inventory
													? POLineType.GoodsForSalesOrder
													: POLineType.NonStockForSalesOrder);
							else
								line.LineType = POLineType.GoodsForSalesOrder;
						}
						else
						{
							if (soline != null)
							{
								line.LineType = (soline.LineType == SO.SOLineType.Inventory
													? POLineType.GoodsForDropShip
													: POLineType.NonStockForDropShip);

								INPostClass postclass = (PXResult<InventoryItem, INPostClass>)PXSelectJoin<InventoryItem, InnerJoin<INPostClass, On<INPostClass.postClassID, Equal<InventoryItem.postClassID>>>, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(docgraph, soline.InventoryID);
								line.ExpenseSubID = (soline.LineType == SOLineType.NonInventory && postclass != null && postclass.COGSSubFromSales == true ? soline.SalesSubID : null);
							}
						}

						if (demand.PlanType != INPlanConstants.Plan90)
						{
							line.PromisedDate = demand.PlanDate;
							if (soline != null)
								line.RequestedDate = soline.ShipDate;
						}
						line.VendorLocationID = demand.VendorLocationID;
						line.InventoryID = demand.InventoryID;
						line.SubItemID = demand.SubItemID;
						line.SiteID = demand.SiteID;
						line.UOM = demand.UOM;
						line.OrderQty = demand.OrderQty;
						if (soline != null)
						{
							if (setup.CopyLineDescrSO == true)							
								line.TranDesc = soline.TranDesc;
							
							line.ProjectID = soline.ProjectID;
							line.TaskID = soline.TaskID;
						}
						line = docgraph.Transactions.Insert(line);

						if (setup.CopyLineNoteSO == true && soline != null)
						{
							PXNoteAttribute.SetNote(docgraph.Transactions.Cache, line,
								PXNoteAttribute.GetNote(docgraph.Caches[typeof(SOLine3)], soline));
						}
						line = PXCache<POLine>.CreateCopy(line);
						ordered.Add(line);
					}
					else
					{
						line = (POLine)PXSelect<POLine, Where<POLine.orderType, Equal<Current<POOrder.orderType>>, And<POLine.orderNbr, Equal<Current<POOrder.orderNbr>>, And<POLine.lineNbr, Equal<Current<POLine.lineNbr>>>>>>.SelectSingleBound(docgraph, new object[] { line });
						line = PXCache<POLine>.CreateCopy(line);
						line.OrderQty += demand.OrderQty;
					}

					if (demand.PlanType == INPlanConstants.Plan6B ||
						demand.PlanType == INPlanConstants.Plan6E)
					{
						replanType =
							demand.PlanType == INPlanConstants.Plan6B
								? INPlanConstants.Plan66
								: INPlanConstants.Plan6D;
						demand.FixedSource = INReplenishmentSource.Purchased;

						line.POType = soline.POType;
						line.PONbr = soline.PONbr;
						line.POLineNbr = soline.POLineNbr;

						POLine blanket_line = PXSelect<POLine, Where<POLine.orderType, Equal<Current<POLine.pOType>>, And<POLine.orderNbr, Equal<Current<POLine.pONbr>>, And<POLine.lineNbr, Equal<Current<POLine.pOLineNbr>>>>>>.SelectSingleBound(docgraph, new object[] { line });

						if (blanket_line != null)
						{
							//POOrderEntry() is persisted on each loop, BaseOpenQty will include everything in List<POLine> ordered
							if (demand.PlanQty > blanket_line.BaseOpenQty)
							{
								line.OrderQty -= demand.OrderQty;

								if (string.Equals(line.UOM, blanket_line.UOM))
								{
									line.OrderQty += blanket_line.OpenQty;
								}
								else
								{
									PXDBQuantityAttribute.CalcBaseQty<POLine.orderQty>(docgraph.Transactions.Cache, line);
									line.BaseOrderQty += blanket_line.BaseOpenQty;
									PXDBQuantityAttribute.CalcTranQty<POLine.orderQty>(docgraph.Transactions.Cache, line);
								}

								ErrorLevel = PXErrorLevel.RowWarning;
								ErrorText += string.Format(Messages.QuantityReducedToBlanketOpen, line.PONbr);
							}
							line.CuryUnitCost = blanket_line.CuryUnitCost;
							line.UnitCost = blanket_line.UnitCost;
						}
					}

					line = docgraph.Transactions.Update(line);
					PXCache cache = docgraph.Caches[typeof(INItemPlan)];
					CreateSplitDemand(cache, demand);

					cache.SetStatus(demand, PXEntryStatus.Updated);
					demand.SupplyPlanID = line.PlanID;

					if (replanType != null)
					{
						cache.RaiseRowDeleted(demand);
						demand.PlanType = replanType;
						cache.RaiseRowInserted(demand);
					}

					if (soline != null)
					{
						soline.POType = line.OrderType;
						soline.PONbr = line.OrderNbr;
						soline.POLineNbr = line.LineNbr;

						docgraph.FixedDemand.Cache.SetStatus(soline, PXEntryStatus.Updated);
					}

					if (docgraph.Transactions.Cache.IsInsertedUpdatedDeleted)
					{
						using (PXTransactionScope scope = new PXTransactionScope())
						{
							docgraph.Save.Press();
							if (demand.PlanType == INPlanConstants.Plan90)
							{
								docgraph.Replenihment.Current = docgraph.Replenihment.Search<INReplenishmentOrder.noteID>(demand.RefNoteID);
								if (docgraph.Replenihment.Current != null)
								{
									INReplenishmentLine rLine =
										PXCache<INReplenishmentLine>.CreateCopy(docgraph.ReplenishmentLines.Insert(new INReplenishmentLine()));
									rLine.InventoryID = line.InventoryID;
									rLine.SubItemID = line.SubItemID;
									rLine.UOM = line.UOM;
									rLine.VendorID = line.VendorID;
									rLine.VendorLocationID = line.VendorLocationID;
									rLine.Qty = line.OrderQty;
									rLine.POType = line.OrderType;
									rLine.PONbr = docgraph.Document.Current.OrderNbr;
									rLine.POLineNbr = line.LineNbr;
									rLine.SiteID = demand.SiteID;
									rLine.PlanID = demand.PlanID;
									docgraph.ReplenishmentLines.Update(rLine);
									docgraph.Caches[typeof(INItemPlan)].Delete(demand);
									docgraph.Save.Press();
								}
							}
							scope.Complete();
						}

						if (ErrorLevel == PXErrorLevel.RowInfo)
						{
							PXProcessing<POFixedDemand>.SetInfo(list.IndexOf(demand), string.Format(Messages.PurchaseOrderCreated, docgraph.Document.Current.OrderNbr) + "\r\n" + ErrorText);
						}
						else
						{
							PXProcessing<POFixedDemand>.SetWarning(list.IndexOf(demand), string.Format(Messages.PurchaseOrderCreated, docgraph.Document.Current.OrderNbr) + "\r\n" + ErrorText);
						}

						if (created.Find(docgraph.Document.Current) == null)
						{
							created.Add(docgraph.Document.Current);
						}
					}
				}
				catch (Exception e)
				{
					PXProcessing<POFixedDemand>.SetError(list.IndexOf(demand), e);
				}
			}
			if (created.Count == 1)
			{
				using (new PXTimeStampScope(null))
				{
					docgraph.Clear();
					docgraph.Document.Current = docgraph.Document.Search<POOrder.orderNbr>(created[0].OrderNbr, created[0].OrderType);
					return new PXRedirectRequiredException(docgraph, Messages.POOrder);
				}
			}
			return null;
		}

		public static PXRedirectRequiredException CreateSOOrders(List<POFixedDemand> list, DateTime? TransDate)
		{
			SOOrderEntry docgraph = PXGraph.CreateInstance<SOOrderEntry>();
			SOSetup sosetup = docgraph.sosetup.Current;
			DocumentList<SOOrder> created = new DocumentList<SOOrder>(docgraph);

			foreach (POFixedDemand demand in list)
			{
				if (demand.FixedSource != INReplenishmentSource.Transfer) continue;

				string OrderType =
					sosetup.TransferOrderType ?? SOOrderTypeConstants.TransferOrder;

				string ErrorText = string.Empty;

				try
				{
					if (demand.ReplenishmentSourceSiteID == null)
					{
						PXProcessing<POFixedDemand>.SetWarning(list.IndexOf(demand), SO.Messages.MissingSourceSite);
						continue;
					}

					if (demand.ReplenishmentSourceSiteID == demand.SiteID)
					{
						PXProcessing<POFixedDemand>.SetWarning(list.IndexOf(demand), SO.Messages.EqualSourceDestinationSite);
						continue;
					}
					SOOrder order;
					POLine poline = PXSelect<POLine, Where<POLine.planID, Equal<Required<POLine.planID>>>>.Select(docgraph, demand.PlanID);

					if (poline != null)
					{
						order =
							created.Find<SOOrder.orderType, SOOrder.destinationSiteID, SOOrder.defaultSiteID, SOOrder.origPOType, SOOrder.origPONbr>(
								OrderType, demand.SiteID, demand.ReplenishmentSourceSiteID, poline.OrderType, poline.OrderNbr);
					}
					else
						order = created.Find<SOOrder.orderType, SOOrder.destinationSiteID, SOOrder.defaultSiteID>(OrderType, demand.SiteID, demand.ReplenishmentSourceSiteID);

					if (order == null) order = new SOOrder();

					if (order.OrderNbr == null)
					{
						docgraph.Clear();

						INSite sourceSite = PXSelect<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>.Select(docgraph, demand.ReplenishmentSourceSiteID);
						order.BranchID = sourceSite.BranchID;
						order.OrderType = OrderType;
						order = PXCache<SOOrder>.CreateCopy(docgraph.Document.Insert(order));
						order.DefaultSiteID = demand.ReplenishmentSourceSiteID;
						order.DestinationSiteID = demand.SiteID;
						order.Status = SOOrderStatus.Open;
						order.OrderDate = TransDate;
						if (poline != null)
						{
							order.OrigPOType = poline.OrderType;
							order.OrigPONbr = poline.OrderNbr;
						}
						docgraph.Document.Update(order);
					}
					else if (docgraph.Document.Cache.ObjectsEqual(docgraph.Document.Current, order) == false)
					{
						docgraph.Document.Current = docgraph.Document.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderType);
					}
					SOLine line = new SOLine();
					line = PXCache<SOLine>.CreateCopy(docgraph.Transactions.Insert(line));
					line.InventoryID = demand.InventoryID;
					line.SubItemID = demand.SubItemID;
					line.SiteID = demand.ReplenishmentSourceSiteID;
					line.UOM = demand.UOM;
					line.OrderQty = demand.OrderQty;
					if (poline != null)
					{
						line.OrigPOType = poline.OrderType;
						line.OrigPONbr = poline.OrderNbr;
						line.OrigPOLineNbr = poline.LineNbr;
					}

					line = docgraph.Transactions.Update(line);

					if (line.PlanID == null)
						throw new PXRowPersistedException(typeof(SOLine).Name, line, RQ.Messages.UnableToCreateSOOrders);
					PXCache cache = docgraph.Caches[typeof(INItemPlan)];			
					CreateSplitDemand(cache, demand);
					string demandPlanType = demand.PlanType;
					//cache.SetStatus(demand, PXEntryStatus.Updated);					
					//demand.SupplyPlanID = line.PlanID;					
					INItemPlan rp = PXCache<INItemPlan>.CreateCopy(demand);
					cache.RaiseRowDeleted(demand);
					rp.PlanType = INPlanConstants.Plan94;
					rp.FixedSource = null;
					rp.SupplyPlanID = line.PlanID;
					cache.RaiseRowInserted(rp);
					cache.SetStatus(rp, PXEntryStatus.Updated);

					if (docgraph.Transactions.Cache.IsInsertedUpdatedDeleted)
					{
						using (PXTransactionScope scope = new PXTransactionScope())
						{
							docgraph.Save.Press();
							if (demandPlanType == INPlanConstants.Plan90)
							{
								docgraph.Replenihment.Current = docgraph.Replenihment.Search<INReplenishmentOrder.noteID>(demand.RefNoteID);
								if (docgraph.Replenihment.Current != null)
								{
									INReplenishmentLine rLine =
										PXCache<INReplenishmentLine>.CreateCopy(docgraph.ReplenishmentLines.Insert(new INReplenishmentLine()));
									rLine.InventoryID = line.InventoryID;
									rLine.SubItemID = line.SubItemID;
									rLine.UOM = line.UOM;
									rLine.Qty = line.OrderQty;
									rLine.SOType = line.OrderType;
									rLine.SONbr = docgraph.Document.Current.OrderNbr;
									rLine.SOLineNbr = line.LineNbr;
									rLine.SiteID = demand.ReplenishmentSourceSiteID;
									rLine.DestinationSiteID = demand.SiteID;
									rLine.PlanID = demand.PlanID;
									docgraph.ReplenishmentLines.Update(rLine);
									INItemPlan plan = PXSelect<INItemPlan,
										Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.SelectWindowed(docgraph, 0, 1,
																													 demand.SupplyPlanID);
									if (plan != null)
									{
										//plan.SupplyPlanID = rp.PlanID;
										rp.SupplyPlanID = plan.PlanID;
										cache.SetStatus(rp, PXEntryStatus.Updated);
									}

									docgraph.Save.Press();
								}
							}
							scope.Complete();
						}

						PXProcessing<POFixedDemand>.SetInfo(list.IndexOf(demand), string.Format(SO.Messages.TransferOrderCreated, docgraph.Document.Current.OrderNbr) + "\r\n" + ErrorText);


						if (created.Find(docgraph.Document.Current) == null)
						{
							created.Add(docgraph.Document.Current);
						}
					}
				}
				catch (Exception e)
				{
					PXProcessing<POFixedDemand>.SetError(list.IndexOf(demand), e);
				}
			}
			if (created.Count == 1)
			{
				using (new PXTimeStampScope(null))
				{
					docgraph.Clear();
					docgraph.Document.Current = docgraph.Document.Search<SOOrder.orderNbr>(created[0].OrderNbr, created[0].OrderType);
					return new PXRedirectRequiredException(docgraph, SO.Messages.SOOrder);
				}
			}
			return null;
		}

		private static void CreateSplitDemand(PXCache cache, POFixedDemand demand)
		{
			if (demand.OrderQty != demand.PlanUnitQty)
			{
				INItemPlan orig_demand = PXSelectReadonly<INItemPlan,
					Where<INItemPlan.planID, Equal<Current<INItemPlan.planID>>>>
					.SelectSingleBound(cache.Graph, new object[] {demand});

				INItemPlan split = PXCache<INItemPlan>.CreateCopy(orig_demand);
				split.PlanID = null;
				split.PlanQty = demand.PlanUnitQty - demand.OrderQty;
				if (demand.UnitMultDiv == MultDiv.Multiply)
					split.PlanQty *= demand.UnitRate;
				else
					split.PlanQty /= demand.UnitRate;
				cache.Insert(split);
				cache.RaiseRowDeleted(demand);
				demand.PlanQty = orig_demand.PlanQty - split.PlanQty;
				cache.RaiseRowInserted(demand);
			}
		}


		[Serializable()]
		public partial class POCreateFilter : IBqlTable
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
			[PXDefault(false)]
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
			#region VendorID
			public abstract class vendorID : PX.Data.IBqlField
			{
			}
			protected Int32? _VendorID;
			[Vendor(typeof(Search<BAccountR.bAccountID,
				Where<BAccountR.type, Equal<BAccountType.companyType>, Or<Vendor.type, NotEqual<BAccountType.employeeType>>>>), CacheGlobal = true, Filterable = true)]
			[PXRestrictor(typeof(Where<Vendor.status, IsNull,
									Or<Vendor.status, Equal<BAccount.status.active>,
									Or<Vendor.status, Equal<BAccount.status.oneTime>>>>), AP.Messages.VendorIsInStatus, typeof(Vendor.status))]
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
			#region SiteID
			public abstract class siteID : PX.Data.IBqlField
			{
			}
			protected Int32? _SiteID;
			[IN.Site(DisplayName = "Warehouse ID")]
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
			#region ReplenishmentSourceSiteID
			public abstract class replenishmentSourceSiteID : PX.Data.IBqlField
			{
			}
			protected Int32? _ReplenishmentSourceSiteID;
			[IN.Site(DisplayName = "Source Warehouse", DescriptionField = typeof(INSite.descr))]			
			public virtual Int32? ReplenishmentSourceSiteID
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
			#region EndDate
			public abstract class endDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _EndDate;
			[PXDBDate()]
			[PXUIField(DisplayName = "Date Promised")]
			[PXDefault(typeof(AccessInfo.businessDate))]
			public virtual DateTime? EndDate
			{
				get
				{
					return this._EndDate;
				}
				set
				{
					this._EndDate = value;
				}
			}
			#endregion
			#region PurchDate
			public abstract class purchDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _PurchDate;
			[PXDBDate()]
			[PXUIField(DisplayName = "Creation Date")]
			[PXDefault(typeof(AccessInfo.businessDate))]
			public virtual DateTime? PurchDate
			{
				get
				{
					return this._PurchDate;
				}
				set
				{
					this._PurchDate = value;
				}
			}
			#endregion
			#region CustomerID
			public abstract class customerID : PX.Data.IBqlField
			{
			}
			protected Int32? _CustomerID;
			[Customer()]
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
			#region InventoryID
			public abstract class inventoryID : PX.Data.IBqlField
			{
			}
			protected Int32? _InventoryID;
			[StockItem()]
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
			#region ItemClassID
			public abstract class itemClassID : PX.Data.IBqlField
			{
			}
			protected String _ItemClassID;
			[PXDBString(10, IsUnicode = true)]
			[PXUIField(DisplayName = "Item Class ID", Visibility = PXUIVisibility.SelectorVisible)]
			[PXSelector(typeof(Search<INItemClass.itemClassID>), DescriptionField = typeof(INItemClass.descr))]
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
			#region OrderType
			public abstract class orderType : PX.Data.IBqlField
			{
			}
			protected String _OrderType;
			[PXDBString(2, IsFixed = true, InputMask = ">aa")]			
			[PXSelector(typeof(Search<SOOrderType.orderType, Where<SOOrderType.active, Equal<boolTrue>>>))]
			[PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String OrderType
			{
				get
				{
					return this._OrderType;
				}
				set
				{
					this._OrderType = value;
				}
			}
			#endregion			
			#region OrderNbr
			public abstract class orderNbr : PX.Data.IBqlField
			{
			}
			protected String _OrderNbr;
			[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]			
			[PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
			[SO.SO.RefNbr(typeof(Search2<SOOrder.orderNbr,
				LeftJoin<Customer, On<SOOrder.customerID, Equal<Customer.bAccountID>,
						And<Where<Match<Customer, Current<AccessInfo.userName>>>>>>,
				Where<SOOrder.orderType, Equal<Optional<POCreateFilter.orderType>>,
				And<Where<SOOrder.orderType, Equal<SOOrderTypeConstants.transferOrder>,
				 Or<Customer.bAccountID, IsNotNull>>>>,
				 OrderBy<Desc<SOOrder.orderNbr>>>))]
			[PXFormula(typeof(Default<POCreateFilter.orderType>))]
			public virtual String OrderNbr
			{
				get
				{
					return this._OrderNbr;
				}
				set
				{
					this._OrderNbr = value;
				}
			}
			#endregion
			#region OrderWeight
			public abstract class orderWeight : PX.Data.IBqlField
			{
			}
			protected Decimal? _OrderWeight;
			[PXDBDecimal(6)]
			[PXUIField(DisplayName = "Weight", Enabled = false)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? OrderWeight
			{
				get
				{
					return this._OrderWeight;
				}
				set
				{
					this._OrderWeight = value;
				}
			}
			#endregion
			#region OrderVolume
			public abstract class orderVolume : PX.Data.IBqlField
			{
			}
			protected Decimal? _OrderVolume;
			[PXDBDecimal(6)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Volume", Enabled = false)]
			public virtual Decimal? OrderVolume
			{
				get
				{
					return this._OrderVolume;
				}
				set
				{
					this._OrderVolume = value;
				}
			}
			#endregion
			#region OrderTotal
			public abstract class orderTotal : PX.Data.IBqlField
			{
			}
			protected Decimal? _OrderTotal;
			[PXDBDecimal(typeof(Search<Currency.decimalPlaces, Where<Currency.curyID, Equal<Current<POCreateFilter.vendorID>>>>))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Total", Enabled = false)]
			public virtual Decimal? OrderTotal
			{
				get
				{
					return this._OrderTotal;
				}
				set
				{
					this._OrderTotal = value;
				}
			}
			#endregion
		
		}
        
	    /// <summary>
	    /// Specialized version of the Projection Attribute. Defines Projection as <br/>
        /// a select of INItemPlan Join INPlanType Join InventoryItem Join INUnit Left Join INItemSite <br/>
        /// filtered by InventoryItem.workgroupID and InventoryItem.productManagerID according to the values <br/>
        /// in the POCreateFilter: <br/>
        /// 1. POCreateFilter.ownerID is null or  POCreateFilter.ownerID = InventoryItem.productManagerID <br/>
        /// 2. POCreateFilter.workGroupID is null or  POCreateFilter.workGroupID = InventoryItem.productWorkgroupID <br\>
        /// 3. POCreateFilter.myWorkGroup = false or  InventoryItem.productWorkgroupID =InMember<POCreateFilter.currentOwnerID> <br/>
        /// 4. InventoryItem.productWorkgroupID is null or  InventoryItem.productWorkgroupID =Owened<POCreateFilter.currentOwnerID><br/>        
	    /// </summary>
		public class POCreateProjectionAttribute : TM.OwnedFilter.ProjectionAttribute
		{
            /// <summary>
            /// Default ctor
            /// </summary>
			public POCreateProjectionAttribute()
				: base(typeof(POCreateFilter),
				BqlCommand.Compose(
			typeof(Select2<,,>),
				typeof(INItemPlan),
				typeof(InnerJoin<INPlanType, 
				              On<INPlanType.planType, Equal<INItemPlan.planType>>,
				InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INItemPlan.inventoryID>>,
				InnerJoin<INUnit, On<INUnit.inventoryID, Equal<InventoryItem.inventoryID>, And<INUnit.fromUnit, Equal<InventoryItem.purchaseUnit>, And<INUnit.toUnit, Equal<InventoryItem.baseUnit>>>>,
				LeftJoin<IN.S.INItemSite, On<IN.S.INItemSite.inventoryID, Equal<INItemPlan.inventoryID>, And<IN.S.INItemSite.siteID, Equal<INItemPlan.siteID>>>>>>>),
			typeof(Where2<,>),
			typeof(Where<INItemPlan.hold, Equal<False>,
					// fixed source is not a good criteria for this
					// not erased in SO where PO create is unchecked
				    // And<INItemPlan.fixedSource, IsNotNull, 			
					  And<INPlanType.isFixed, Equal<True>, And<INPlanType.isDemand, Equal<True>,
				      And<Where<INItemPlan.supplyPlanID, IsNull, 
				              Or<INItemPlan.planType, Equal<INPlanConstants.plan6B>,
				              Or<INItemPlan.planType, Equal<INPlanConstants.plan6E>>>>>>>>),
			typeof(And<>),
			TM.OwnedFilter.ProjectionAttribute.ComposeWhere(
			typeof(POCreateFilter),
			typeof(InventoryItem.productWorkgroupID),
			typeof(InventoryItem.productManagerID))))
			{
			}
		}
	
		
	}

	[POCreate.POCreateProjectionAttribute]
	[Serializable]
	public partial class POFixedDemand : INItemPlan
	{
		#region Selected
		public new abstract class selected : IBqlField
		{
		}
		#endregion
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
		#region PlanDate
		public new abstract class planDate : PX.Data.IBqlField
		{
		}
		[PXDBDate()]
		[PXDefault()]
		[PXUIField(DisplayName = "Requested On")]
		public override DateTime? PlanDate
		{
			get
			{
				return this._PlanDate;
			}
			set
			{
				this._PlanDate = value;
			}
		}
		#endregion
		#region PlanID
		public new abstract class planID : PX.Data.IBqlField
		{
		}
		#endregion
		#region PlanType
		public new abstract class planType : PX.Data.IBqlField
		{
		}
		[PXDBString(2, IsFixed = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Plan Type")]
		[PXSelector(typeof(Search<INPlanType.planType>), CacheGlobal = true, DescriptionField = typeof(INPlanType.descr))]
		public override String PlanType
		{
			get
			{
				return this._PlanType;
			}
			set
			{
				this._PlanType = value;
			}
		}
		#endregion
		#region SubItemID
		public new abstract class subItemID : PX.Data.IBqlField
		{
		}
		#endregion
		#region LocationID
		public new abstract class locationID : PX.Data.IBqlField
		{
		}
		#endregion
		#region LotSerialNbr
		public new abstract class lotSerialNbr : PX.Data.IBqlField
		{
		}
		#endregion
		#region ReplenishmentSourceSiteID
		public abstract class replenishmentSourceSiteID : PX.Data.IBqlField
		{
		}
		protected Int32? _ReplenishmentSourceSiteID;
		[IN.Site(DisplayName = "Source Warehouse", DescriptionField = typeof(INSite.descr), BqlField = typeof(INItemPlan.sourceSiteID))]
		[PXFormula(typeof(Default<POFixedDemand.fixedSource>))]
		[PXDefault(typeof(Search<INItemSiteSettings.replenishmentSourceSiteID,
			Where<INItemSiteSettings.inventoryID, Equal<Current<POFixedDemand.inventoryID>>,
			And<INItemSiteSettings.siteID, Equal<Current<POFixedDemand.siteID>>,
			And<Current<POFixedDemand.fixedSource>, Equal<INReplenishmentSource.transfer>>>>>),
		PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? ReplenishmentSourceSiteID
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
		#region VendorID
		public new abstract class vendorID : PX.Data.IBqlField
		{
		}
		[Vendor(typeof(Search<BAccountR.bAccountID,
			Where<Current<POFixedDemand.planType>, Equal<INPlanConstants.plan6T>, And<BAccountR.type, Equal<BAccountType.companyType>, Or<Current<POFixedDemand.planType>, NotEqual<INPlanConstants.plan6T>, And<Vendor.type, NotEqual<BAccountType.employeeType>>>>>>))]
		[PXRestrictor(typeof(Where<Vendor.status, IsNull,
								Or<Vendor.status, Equal<BAccount.status.active>,
								Or<Vendor.status, Equal<BAccount.status.oneTime>>>>), AP.Messages.VendorIsInStatus, typeof(Vendor.status))]
		[PXFormula(typeof(Default<POFixedDemand.fixedSource>))]
		[PXDefault(typeof(Coalesce<
			Search2<BAccountR.bAccountID,
			InnerJoin<INItemSiteSettings, On<INItemSiteSettings.inventoryID, Equal<Current<POFixedDemand.inventoryID>>, And<INItemSiteSettings.siteID, Equal<Current<POFixedDemand.siteID>>>>,
			LeftJoin<INSite, On<INSite.siteID, Equal<INItemSiteSettings.replenishmentSourceSiteID>>,
			LeftJoin<GL.Branch, On<GL.Branch.branchID, Equal<INSite.branchID>>>>>,
			Where<INItemSiteSettings.preferredVendorID, Equal<BAccountR.bAccountID>, And<Current<POFixedDemand.fixedSource>, NotEqual<INReplenishmentSource.transfer>,
					Or<GL.Branch.bAccountID, Equal<BAccountR.bAccountID>, And<Current<POFixedDemand.fixedSource>, Equal<INReplenishmentSource.transfer>>>>>>,
			Search<InventoryItem.preferredVendorID,
				Where<InventoryItem.inventoryID, Equal<Current<POFixedDemand.inventoryID>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public override Int32? VendorID
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
		public new abstract class vendorLocationID : PX.Data.IBqlField
		{
		}
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<POFixedDemand.vendorID>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(Default<POFixedDemand.vendorID>))]
		public override Int32? VendorLocationID
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
		#region RecordID
		public abstract class recordID : PX.Data.IBqlField
		{
		}
		protected Int32? _RecordID;
		[PXDBScalar(typeof(Search2<POVendorInventory.recordID,
			InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<POVendorInventory.vendorID>>,
			InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<POVendorInventory.inventoryID>>>>,
			Where<POVendorInventory.vendorID, Equal<INItemPlan.vendorID>,
			  And<POVendorInventory.inventoryID, Equal<INItemPlan.inventoryID>,
				And<POVendorInventory.active, Equal<boolTrue>,
				And2<Where<POVendorInventory.vendorLocationID, Equal<INItemPlan.vendorLocationID>,
						Or<POVendorInventory.vendorLocationID, IsNull>>,
				And<Where<POVendorInventory.subItemID, Equal<INItemPlan.subItemID>,
							 Or<POVendorInventory.subItemID, Equal<InventoryItem.defaultSubItemID>>>>>>>>,
			OrderBy<
			Asc<Switch<Case<Where<POVendorInventory.vendorLocationID, Equal<INItemPlan.vendorLocationID>>, boolFalse>, boolTrue>,
			Asc<Switch<Case<Where<POVendorInventory.subItemID, Equal<INItemPlan.subItemID>>, boolFalse>, boolTrue>>>>>))]
		[PXFormula(typeof(Default<POFixedDemand.vendorLocationID>))]
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
		#region SupplyPlanID
		public new abstract class supplyPlanID : PX.Data.IBqlField
		{
		}
		#endregion
		#region PlanQty
		public new abstract class planQty : PX.Data.IBqlField
		{
		}
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Requested Qty.")]
		public override Decimal? PlanQty
		{
			get
			{
				return this._PlanQty;
			}
			set
			{
				this._PlanQty = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.IBqlField
		{
		}
		protected String _UOM;
		[PXDBString(BqlField = typeof(INUnit.fromUnit))]
		[PXUIField(DisplayName = "UOM")]
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
		#region UnitMultDiv
		public abstract class unitMultDiv : PX.Data.IBqlField
		{
		}
		protected String _UnitMultDiv;
		[PXDBString(1, IsFixed = true, BqlField = typeof(INUnit.unitMultDiv))]
		public virtual String UnitMultDiv
		{
			get
			{
				return this._UnitMultDiv;
			}
			set
			{
				this._UnitMultDiv = value;
			}
		}
		#endregion
		#region UnitRate
		public abstract class unitRate : PX.Data.IBqlField
		{
		}
		protected Decimal? _UnitRate;
		[PXDBDecimal(6, BqlField = typeof(INUnit.unitRate))]
		public virtual Decimal? UnitRate
		{
			get
			{
				return this._UnitRate;
			}
			set
			{
				this._UnitRate = value;
			}
		}
		#endregion
		#region PlanUnitQty
		public abstract class planUnitQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _PlanUnitQty;
		[PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>, Mult<INItemPlan.planQty, INUnit.unitRate>>, Div<INItemPlan.planQty, INUnit.unitRate>>), typeof(decimal))]
		[PXQuantity()]
		public virtual Decimal? PlanUnitQty
		{
			get
			{
				return this._PlanUnitQty;
			}
			set
			{
				this._PlanUnitQty = value;
			}
		}
		#endregion
		#region OrderQty
		public abstract class orderQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _OrderQty;
		[PXQuantity()]
		[PXUIField(DisplayName = "Quantity")]
		public virtual Decimal? OrderQty
		{
			[PXDependsOnFields(typeof(planUnitQty))]
			get
			{
				return this._OrderQty ?? this._PlanUnitQty;
			}
			set
			{
				this._OrderQty = value;
			}
		}
		#endregion
		#region RefNoteID
		public new abstract class refNoteID : PX.Data.IBqlField
		{
		}
		[PXRefNote()]
		[PXUIField(DisplayName = "Reference Nbr.")]
		public override Int64? RefNoteID
		{
			get
			{
				return this._RefNoteID;
			}
			set
			{
				this._RefNoteID = value;
			}
		}
		#endregion
		#region Hold
		public new abstract class hold : PX.Data.IBqlField
		{
		}
		#endregion
		#region VendorID_Vendor_acctName
		public abstract class vendorID_Vendor_acctName : PX.Data.IBqlField
		{
		}
		#endregion
		#region InventoryID_InventoryItem_descr
		public abstract class inventoryID_InventoryItem_descr : PX.Data.IBqlField
		{
		}
		#endregion
		#region SiteID_INSite_descr
		public abstract class siteID_INSite_descr : PX.Data.IBqlField
		{
		}
		#endregion
		#region AddLeadTimeDays
		public abstract class addLeadTimeDays : PX.Data.IBqlField
		{
		}
		protected Int16? _AddLeadTimeDays;
		[PXShort()]
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
		#region EffPrice
		public abstract class effPrice : PX.Data.IBqlField
		{
		}
		protected Decimal? _EffPrice;
		[PXPriceCost()]
		[PXUIField(DisplayName = "Vendor Price", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? EffPrice
		{
			get
			{
				return this._EffPrice;
			}
			set
			{
				this._EffPrice = value;
			}
		}
		#endregion
		#region UnitWeight
		public abstract class unitWeight : PX.Data.IBqlField
		{
		}
		protected Decimal? _UnitWeight;
		[PXDecimal(6)]
		[PXUIField(DisplayName = "Unit Weight")]
		public virtual Decimal? UnitWeight
		{
			get
			{
				return this._UnitWeight;
			}
			set
			{
				this._UnitWeight = value;
			}
		}
		#endregion
		#region UnitVolume
		public abstract class unitVolume : PX.Data.IBqlField
		{
		}
		protected Decimal? _UnitVolume;
		[PXDecimal(6)]
		[PXUIField(DisplayName = "Unit Volume")]
		public virtual Decimal? UnitVolume
		{
			get
			{
				return this._UnitVolume;
			}
			set
			{
				this._UnitVolume = value;
			}
		}
		#endregion
		#region ExtWeight
		public abstract class extWeight : PX.Data.IBqlField
		{
		}
		protected Decimal? _ExtWeight;
		[PXDecimal(6)]
		[PXUIField(DisplayName = "Weight")]
		[PXFormula(typeof(Mult<POFixedDemand.orderQty, POFixedDemand.unitWeight>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ExtWeight
		{
			get
			{
				return this._ExtWeight;
			}
			set
			{
				this._ExtWeight = value;
			}
		}
		#endregion
		#region ExtVolume
		public abstract class extVolume : PX.Data.IBqlField
		{
		}
		protected Decimal? _ExtVolume;
		[PXDecimal(6)]
		[PXUIField(DisplayName = "Volume")]
		[PXFormula(typeof(Mult<POFixedDemand.orderQty, POFixedDemand.unitVolume>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ExtVolume
		{
			get
			{
				return this._ExtVolume;
			}
			set
			{
				this._ExtVolume = value;
			}
		}
		#endregion
		#region ExtCost
		public abstract class extCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _ExtCost;
		[PXDecimal(typeof(Search<Currency.decimalPlaces, Where<Currency.curyID, Equal<Current<POCreate.POCreateFilter.vendorID>>>>))]
		[PXUIField(DisplayName = "Extended Amt.", Enabled = false)]
		[PXFormula(typeof(Mult<POFixedDemand.orderQty, POFixedDemand.effPrice>))]
		public virtual Decimal? ExtCost
		{
			get
			{
				return this._ExtCost;
			}
			set
			{
				this._ExtCost = value;
			}
		}
		#endregion
	}
}
