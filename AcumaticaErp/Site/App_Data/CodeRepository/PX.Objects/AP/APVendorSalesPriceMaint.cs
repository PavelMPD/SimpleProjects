using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.TM;
using PX.Objects.PO;

namespace PX.Objects.AP
{
	public class APVendorSalesPriceMaint : PXGraph<APVendorSalesPriceMaint>, PXImportAttribute.IPXPrepareItems
	{
		#region DAC Overrides
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<APSalesPrice.vendorID>>>),
			DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location")]
		[PXParent(typeof(Select<Location, Where<Location.bAccountID, Equal<Current<APSalesPrice.vendorID>>,
												And<Location.locationID, Equal<Current<APSalesPrice.vendorLocationID>>>>>))]
		[PXDefault(typeof(APSalesPriceFilter.locationID), PersistingCheck=PXPersistingCheck.Nothing)]
		public virtual void APSalesPrice_VendorLocationID_CacheAttached(PXCache sender) { }
		#endregion

		#region Selects/Views

		public PXSave<APSalesPriceFilter> Save;
		public PXCancel<APSalesPriceFilter> Cancel;

		public PXFilter<APSalesPriceFilter> Filter;
		public PXFilter<MassUpdateFilter> MassUpdateSettings;

		[PXFilterable]
		[PXImport(typeof(APSalesPriceFilter))]
		public PXSelectJoin<APSalesPrice,
				LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<APSalesPrice.inventoryID>>>,
				Where<APSalesPrice.vendorID, Equal<Current<APSalesPriceFilter.vendorID>>,
			    And2<Where<APSalesPrice.vendorLocationID, IsNull, Or<APSalesPrice.vendorLocationID, Equal<Current<APSalesPriceFilter.locationID>>>>,
				And2<Where<Current<APSalesPriceFilter.curyID>, IsNull, Or<APSalesPrice.curyID, Equal<Current<APSalesPriceFilter.curyID>>>>,
				And<APSalesPrice.isPromotionalPrice, Equal<Current<APSalesPriceFilter.promotionalPrice>>,
				And<InventoryItem.itemStatus, NotEqual<INItemStatus.inactive>,
				And<InventoryItem.itemStatus, NotEqual<INItemStatus.toDelete>,
				And<Where2<Where<Current<APSalesPriceFilter.itemClassID>, IsNull,
						Or<Current<APSalesPriceFilter.itemClassID>, Equal<InventoryItem.itemClassID>>>,
					And2<Where<Current<APSalesPriceFilter.ownerID>, IsNull,
						Or<Current<APSalesPriceFilter.ownerID>, Equal<InventoryItem.priceManagerID>>>,
					And2<Where<Current<APSalesPriceFilter.myWorkGroup>, Equal<boolFalse>,
						 Or<InventoryItem.priceWorkgroupID, InMember<CurrentValue<APSalesPriceFilter.currentOwnerID>>>>,
					And<Where<Current<APSalesPriceFilter.workGroupID>, IsNull,
						Or<Current<APSalesPriceFilter.workGroupID>, Equal<InventoryItem.priceWorkgroupID>>>>>>>>>>>>>>,
				OrderBy<Asc<APSalesPrice.inventoryID,
					Asc<APSalesPrice.uOM,
					Asc<APSalesPrice.lastDate>>>>> Records;

		public PXSetup<Company> Company;
		public PXSelect<POVendorInventory> VendorItems;
		#endregion

		#region Constants
		protected const string _ITEMS_VIEW_NAME = "Records";
		#endregion

		#region Fields
		private readonly string _curyIDFieldName;
		private readonly string _vendorID;
		private readonly string _vendorLocationID;
		private readonly string _inventoryID;
		private readonly string _uOM;
		private readonly string _recordID;
		private readonly string _breakQty;
		private readonly string _lastDate;
		private readonly string _expirationDate;
		private readonly string _pendingBreakQty;
		#endregion

		#region Ctor
		public APVendorSalesPriceMaint() 
		{
			_curyIDFieldName = Records.Cache.GetField(typeof(APSalesPrice.curyID));
			_vendorID = Records.Cache.GetField(typeof(APSalesPrice.vendorID));
			_vendorLocationID = Records.Cache.GetField(typeof(APSalesPrice.vendorLocationID));
			_inventoryID = Records.Cache.GetField(typeof(APSalesPrice.inventoryID)); ;
			_uOM = Records.Cache.GetField(typeof(APSalesPrice.uOM));
			_recordID = Records.Cache.GetField(typeof(APSalesPrice.recordID));
			_breakQty = Records.Cache.GetField(typeof(APSalesPrice.breakQty));
			_lastDate = Records.Cache.GetField(typeof(APSalesPrice.lastDate));
			_expirationDate = Records.Cache.GetField(typeof(APSalesPrice.expirationDate));
			_pendingBreakQty = Records.Cache.GetField(typeof(APSalesPrice.pendingBreakQty));

			FieldDefaulting.AddHandler<CR.BAccountR.type>((sender, e) => { if (e.Row != null) e.NewValue = CR.BAccountType.VendorType; });
		}
		#endregion

		public PXAction<APSalesPriceFilter> update;
		[PXUIField(DisplayName = "Update  Vendor Prices", MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXProcessButton]
		public virtual IEnumerable Update(PXAdapter adapter)
		{
			if (MassUpdateSettings.AskExt() == WebDialogResult.OK)
			{
				this.Save.Press();
				APUpdateSalesPriceProcess graph = PXGraph.CreateInstance<APUpdateSalesPriceProcess>();
				foreach (APSalesPrice sp in PXSelect<APSalesPrice, Where<APSalesPrice.vendorID, Equal<Required<APSalesPriceFilter.vendorID>>, And<APSalesPrice.effectiveDate, IsNotNull, And<APSalesPrice.effectiveDate, LessEqual<Required<MassUpdateFilter.effectiveDate>>, And<APSalesPrice.isPromotionalPrice, Equal<boolFalse>, And<Where<APSalesPrice.vendorLocationID, Equal<Required<APSalesPriceFilter.locationID>>, Or<APSalesPrice.vendorLocationID, IsNull>>>>>>>>.Select(this, Filter.Current.VendorID, MassUpdateSettings.Current.EffectiveDate, Filter.Current.LocationID))
					graph.UpdateSalesPrice(sp);
				Records.View.RequestRefresh();
				SelectTimeStamp();
			}
			return adapter.Get();
		}

		public virtual void APSalesPriceFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			APSalesPriceFilter row=(APSalesPriceFilter)e.Row;
			Records.Cache.AllowInsert = row.VendorID != null && row.CuryID != null;
			Records.Cache.AllowUpdate =
			Records.Cache.AllowDelete = row.VendorID != null;

			Vendor vend = (Vendor)PXParentAttribute.SelectParent(sender, e.Row, typeof(Vendor));
			if (vend != null)
			{
				PXUIFieldAttribute.SetEnabled<APSalesPriceFilter.curyID>(sender, e.Row, vend.CuryID == null || vend.AllowOverrideCury == true);
			}

			PXUIFieldAttribute.SetVisible<APSalesPrice.pendingBreakQty>(Records.Cache, null, row.PromotionalPrice == false);
			PXUIFieldAttribute.SetVisible<APSalesPrice.pendingPrice>(Records.Cache, null, row.PromotionalPrice == false);
			PXUIFieldAttribute.SetVisible<APSalesPrice.effectiveDate>(Records.Cache, null, row.PromotionalPrice == false);
			PXUIFieldAttribute.SetVisible<APSalesPrice.lastPrice>(Records.Cache, null, row.PromotionalPrice == false);
			PXUIFieldAttribute.SetVisible<APSalesPrice.lastBreakQty>(Records.Cache, null, row.PromotionalPrice == false);
			PXUIFieldAttribute.SetVisible<APSalesPrice.expirationDate>(Records.Cache, null, row.PromotionalPrice == true);

			PXUIFieldAttribute.SetEnabled<APSalesPrice.salesPrice>(Records.Cache, null, row.PromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<APSalesPrice.breakQty>(Records.Cache, null, row.PromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<APSalesPrice.lastDate>(Records.Cache, null, row.PromotionalPrice == true);

			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(TM.OwnedFilter.ownerID).Name, e.Row == null || (bool?)sender.GetValue(e.Row, typeof(TM.OwnedFilter.myOwner).Name) == false);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(TM.OwnedFilter.workGroupID).Name, e.Row == null || (bool?)sender.GetValue(e.Row, typeof(TM.OwnedFilter.myWorkGroup).Name) == false);

			update.SetEnabled(row.PromotionalPrice != true);
		}

		public virtual void APSalesPriceFilter_VendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APSalesPriceFilter row=e.Row as APSalesPriceFilter;
			if (row!=null)
			{
				Vendor vend = (Vendor)PXParentAttribute.SelectParent(sender, e.Row, typeof(Vendor));
				if (vend != null)
				{
					row.CuryID = vend.CuryID;
				}
			}

		}

		protected virtual void APSalesPrice_VendorID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Filter != null)
			{
				e.NewValue = Filter.Current.VendorID;
			}
		}

		protected virtual void APSalesPrice_VendorLocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Filter != null)
			{
				e.NewValue = Filter.Current.LocationID;
			}
		}

		protected virtual void APSalesPrice_IsPromotionalPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Filter != null)
			{
				e.NewValue = Filter.Current.PromotionalPrice;
			}
		}

		protected virtual void APSalesPrice_EffectiveDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Filter.Current.PromotionalPrice == true)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void APSalesPrice_LastDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Filter != null && Filter.Current.PromotionalPrice==true)
			{
				e.NewValue = Accessinfo.BusinessDate;
			}
		}

		protected virtual void APSalesPrice_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			APSalesPrice row = (APSalesPrice)e.Row;
			if (row != null && Filter.Current != null)
			{
				row.CuryID = Filter.Current.CuryID;
				row.IsPromotionalPrice = Filter.Current.PromotionalPrice;
			}
		}

		protected virtual void APSalesPrice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			APSalesPrice row = (APSalesPrice)e.Row;
			if (row.IsPromotionalPrice == true && row.LastDate == null)
			{
				sender.RaiseExceptionHandling<APSalesPrice.lastDate>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APSalesPrice.lastDate).Name));
			}
			if (row.IsPromotionalPrice == true && row.ExpirationDate == null)
			{
				sender.RaiseExceptionHandling<APSalesPrice.expirationDate>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APSalesPrice.expirationDate).Name));
			}
			if (row.IsPromotionalPrice == true && row.ExpirationDate < row.LastDate)
			{
				sender.RaiseExceptionHandling<APSalesPrice.lastDate>(row, row.LastDate, new PXSetPropertyException(AR.Messages.LastDateExpirationDate, PXErrorLevel.RowError));
			}
			if (IsStockInventory(row.InventoryID) && row.SubItemID == null)
			{
				sender.RaiseExceptionHandling<APSalesPrice.subItemID>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APSalesPrice.subItemID).Name));
			}
		}

		protected virtual void APSalesPrice_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			APSalesPrice row = e.Row as APSalesPrice;
			if (row == null) return;

			if (IsStockInventory(row.InventoryID))
			{
				PXUIFieldAttribute.SetEnabled<APSalesPrice.subItemID>(sender, row, true);
				PXDefaultAttribute.SetPersistingCheck<APSalesPrice.subItemID>(sender, row, PXPersistingCheck.NullOrBlank);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled<APSalesPrice.subItemID>(sender, row, false);
				PXDefaultAttribute.SetPersistingCheck<APSalesPrice.subItemID>(sender, row, PXPersistingCheck.Nothing);
			}
		}

		protected virtual void APSalesPrice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			APSalesPrice price = e.Row as APSalesPrice;

			if (price.BreakQty == 0 && price.PendingBreakQty == 0 && price.IsPromotionalPrice != true && (!sender.ObjectsEqual<APSalesPrice.pendingPrice>(e.Row, e.OldRow) ||
				!sender.ObjectsEqual<APSalesPrice.effectiveDate>(e.Row, e.OldRow) ||
				!sender.ObjectsEqual<APSalesPrice.salesPrice>(e.Row, e.OldRow)))
			{
				POVendorInventory vi = getPOVendorInventory(price);

				if (vi != null)
				{
					POVendorInventory copy = PXCache<POVendorInventory>.CreateCopy(vi);
					copy.PendingPrice = price.PendingPrice;
					copy.PendingDate = price.EffectiveDate;
					copy.EffPrice = price.SalesPrice;
					copy.EffDate = price.LastDate;
					copy.LastPrice = price.LastPrice;
					VendorItems.Update(copy);
				}
			}
		}

		protected virtual void APSalesPrice_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			APSalesPrice price = e.Row as APSalesPrice;
			if (price.IsPromotionalPrice == false && price.PendingBreakQty == 0 && price.BreakQty == 0)
			{
				POVendorInventory vi = getPOVendorInventory(price);
				if (vi != null)
					VendorItems.Delete(vi);
			}
		}

		protected virtual void APSalesPrice_AllLocations_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APSalesPrice row = (APSalesPrice)e.Row;
			if (e.NewValue != null && (bool)e.NewValue == true)
			{
				APSalesPrice parent =
				PXSelect<APSalesPrice,
				Where<APSalesPrice.inventoryID, Equal<Current<APSalesPrice.inventoryID>>,
					And<APSalesPrice.uOM, Equal<Current<APSalesPrice.uOM>>,
					And<APSalesPrice.pendingBreakQty, Equal<Current<APSalesPrice.pendingBreakQty>>,
					And<APSalesPrice.vendorID, Equal<Current<APSalesPrice.vendorID>>,
					And<APSalesPrice.vendorLocationID, IsNull,
					And<APSalesPrice.recordID, NotEqual<Current<APSalesPrice.recordID>>,
					And<APSalesPrice.curyID, Equal<Current<APSalesPrice.curyID>>>>>>>>>>
					.SelectSingleBound(this, new object[] { e.Row }, null);

				if (parent != null)
				{
					throw new PXSetPropertyException(PO.Messages.POVendorInventoryDuplicate);
				}
			}
		}

		protected virtual void APSalesPrice_AllLocations_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APSalesPrice row = (APSalesPrice)e.Row;
			if (row.AllLocations == true)
			{
				row.VendorLocationID = null;
			}
			else
			{
				row.VendorLocationID = Filter.Current.LocationID;
			}
		}

		protected virtual void APSalesPrice_PendingPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APSalesPrice row = (APSalesPrice)e.Row;
			if (row.EffectiveDate == null && row.IsPromotionalPrice == false && row.PendingPrice > 0m)
				sender.SetDefaultExt<APSalesPrice.effectiveDate>(row);
		}

		public override void Persist()
		{
			foreach (APSalesPrice price in Records.Cache.Inserted)
			{
				ValidateDuplicate(this, Records.Cache, price);
				if (price.IsPromotionalPrice == false && price.PendingBreakQty == 0 && price.BreakQty == 0)
				{
					InsertPOVendorInventory(price);
				}
			}
			foreach (APSalesPrice price in Records.Cache.Updated)
			{
				ValidateDuplicate(this, Records.Cache, price);
			}
			base.Persist();
			Records.Cache.Clear();
		}

		public static void ValidateDuplicate(PXGraph graph, PXCache sender, APSalesPrice price)
		{
			PXSelectBase<APSalesPrice> selectDuplicate = new PXSelect<APSalesPrice, Where<APSalesPrice.curyID, Equal<Required<APSalesPrice.curyID>>, And<APSalesPrice.vendorID, Equal<Required<APSalesPrice.vendorID>>, And<APSalesPrice.inventoryID, Equal<Required<APSalesPrice.inventoryID>>, And<APSalesPrice.uOM, Equal<Required<APSalesPrice.uOM>>, And<APSalesPrice.recordID, NotEqual<Required<APSalesPrice.recordID>>, And<APSalesPrice.pendingBreakQty, Equal<Required<APSalesPrice.pendingBreakQty>>, And<APSalesPrice.breakQty, Equal<Required<APSalesPrice.breakQty>>, And<APSalesPrice.isPromotionalPrice, Equal<Required<APSalesPrice.isPromotionalPrice>>, And2<Where<APSalesPrice.subItemID, Equal<Required<APSalesPrice.subItemID>>, Or<Required<APSalesPrice.subItemID>, IsNull, And<APSalesPrice.subItemID, IsNull>>>, And<Where<APSalesPrice.vendorLocationID, Equal<Required<APSalesPrice.vendorLocationID>>, Or<APSalesPrice.vendorLocationID, IsNull, And<Required<APSalesPrice.vendorLocationID>, IsNull>>>>>>>>>>>>>>(graph);
			APSalesPrice duplicate;
			if (price.IsPromotionalPrice == true)
			{
				selectDuplicate.WhereAnd<Where2<Where<Required<APSalesPrice.lastDate>, Between<APSalesPrice.lastDate, APSalesPrice.expirationDate>>, Or<Required<APSalesPrice.expirationDate>, Between<APSalesPrice.lastDate, APSalesPrice.expirationDate>, Or<APSalesPrice.lastDate, Between<Required<APSalesPrice.lastDate>, Required<APSalesPrice.expirationDate>>, Or<APSalesPrice.expirationDate, Between<Required<APSalesPrice.lastDate>, Required<APSalesPrice.expirationDate>>>>>>>();
				duplicate = selectDuplicate.SelectSingle(price.CuryID, price.VendorID, price.InventoryID, price.UOM, price.RecordID, price.PendingBreakQty, price.BreakQty, price.IsPromotionalPrice, price.SubItemID, price.SubItemID, price.VendorLocationID, price.VendorLocationID, price.LastDate, price.ExpirationDate, price.LastDate, price.ExpirationDate, price.LastDate, price.ExpirationDate);
			}
			else
			{
				duplicate = selectDuplicate.SelectSingle(price.CuryID, price.VendorID, price.InventoryID, price.UOM, price.RecordID, price.PendingBreakQty, price.BreakQty, price.IsPromotionalPrice, price.SubItemID, price.SubItemID, price.VendorLocationID, price.VendorLocationID);
			}
			if (duplicate != null)
			{
				sender.RaiseExceptionHandling<APSalesPrice.uOM>(price, price.UOM, new PXSetPropertyException(SO.Messages.DuplicateSalesPrice, PXErrorLevel.RowError));
			}
		}

		private bool IsStockInventory(int? inventoryID)
		{
			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, inventoryID);
			return item != null && item.StkItem == true;
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

        #region Cost Calculation

        /// <summary>
        /// Calculates Unit Cost.
        /// </summary>
        /// <param name="sender">Cache</param>
        /// <param name="inventoryID">Inventory</param>
        /// <param name="curyID">Currency</param>
        /// <param name="UOM">Unit of measure</param>
        /// <param name="date">Date</param>
        /// <returns>Unit Cost</returns>
        public static decimal? CalculateUnitCost(PXCache sender, int? vendorID, int? vendorLocationID, int? inventoryID, CurrencyInfo currencyinfo, string UOM, decimal? quantity, DateTime date, decimal? currentUnitCost, bool alwaysFromBaseCurrency = false)
        {
            InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(sender.Graph, inventoryID);
            UnitCostItem ucItem = FindUnitCost(sender, vendorID, vendorLocationID, inventoryID, alwaysFromBaseCurrency ? currencyinfo.BaseCuryID : currencyinfo.CuryID, Math.Abs(quantity ?? 0m), UOM, date);

            if (ucItem != null)
            {
                decimal unitCost = ucItem.Cost;

                if (ucItem.CuryID != currencyinfo.CuryID)
                {
                    PXCurrencyAttribute.CuryConvCury(sender, currencyinfo, ucItem.Cost, out unitCost);
                }

                if (UOM == null)
                {
                    return null;
                }

                if (ucItem.UOM != UOM)
                {
                    decimal salesPriceInBase = INUnitAttribute.ConvertFromBase(sender, inventoryID, ucItem.UOM, unitCost, INPrecision.UNITCOST);
                    unitCost = INUnitAttribute.ConvertToBase(sender, inventoryID, UOM, salesPriceInBase, INPrecision.UNITCOST);
                }

                if (unitCost == 0m && currentUnitCost != null && currentUnitCost != 0m)
                    return currentUnitCost;
                else
                    return unitCost;
            }

            if (currentUnitCost != null && currentUnitCost != 0m)
                return currentUnitCost;
            else
                return null;
        }

        public static UnitCostItem FindUnitCost(PXCache sender, int? inventoryID, string curyID, string UOM, DateTime date)
        {
            return FindUnitCost(sender, null, null, inventoryID, curyID, 0m, UOM, date);
        }

        public static UnitCostItem FindUnitCost(PXCache sender, int? vendorID, int? vendorLocationID, int? inventoryID, string curyID, decimal? quantity, string UOM, DateTime date)
        {
            PXSelectBase<APSalesPrice> unitCost = new PXSelect<AP.APSalesPrice, Where<AP.APSalesPrice.inventoryID, Equal<Required<AP.APSalesPrice.inventoryID>>, 
            And<AP.APSalesPrice.vendorID, Equal<Required<AP.APSalesPrice.vendorID>>,
            And<AP.APSalesPrice.vendorLocationID, Equal<Required<AP.APSalesPrice.vendorLocationID>>,
            And<AP.APSalesPrice.curyID, Equal<Required<AP.APSalesPrice.curyID>>,
            And<AP.APSalesPrice.uOM, Equal<Required<AP.APSalesPrice.uOM>>,
            And<Where2<Where2<Where<AP.APSalesPrice.breakQty, LessEqual<Required<AP.APSalesPrice.breakQty>>>,
            And<Where2<Where<AP.APSalesPrice.lastDate, LessEqual<Required<AP.APSalesPrice.lastDate>>,
                And<AP.APSalesPrice.isPromotionalPrice, Equal<False>>>,
            Or<Where<AP.APSalesPrice.lastDate, LessEqual<Required<AP.APSalesPrice.lastDate>>,
                And<AP.APSalesPrice.expirationDate, GreaterEqual<Required<AP.APSalesPrice.expirationDate>>, And<AP.APSalesPrice.isPromotionalPrice, Equal<True>>>>>>>>,
            
            Or<Where<AP.APSalesPrice.lastBreakQty, LessEqual<Required<AP.APSalesPrice.lastBreakQty>>,
                And<AP.APSalesPrice.lastDate, Greater<Required<AP.APSalesPrice.lastDate>>,
                And<AP.APSalesPrice.isPromotionalPrice, Equal<False>>>>>>>>>>>>,

            OrderBy<Desc<AP.APSalesPrice.isPromotionalPrice, Desc<AP.APSalesPrice.vendorID, Desc<AP.APSalesPrice.breakQty, Desc<AP.APSalesPrice.lastBreakQty>>>>>>(sender.Graph);

            PXSelectBase<APSalesPrice> unitCostBaseUOM = new PXSelectJoin<AP.APSalesPrice, InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<APSalesPrice.inventoryID>, And<InventoryItem.baseUnit, Equal<APSalesPrice.uOM>>>>, 
                Where<AP.APSalesPrice.inventoryID, Equal<Required<AP.APSalesPrice.inventoryID>>,
            And<AP.APSalesPrice.vendorID, Equal<Required<AP.APSalesPrice.vendorID>>,
            And<AP.APSalesPrice.vendorLocationID, Equal<Required<AP.APSalesPrice.vendorLocationID>>,
            And<AP.APSalesPrice.curyID, Equal<Required<AP.APSalesPrice.curyID>>,
            And<Where2<Where2<Where<AP.APSalesPrice.breakQty, LessEqual<Required<AP.APSalesPrice.breakQty>>>,
            And<Where2<Where<AP.APSalesPrice.lastDate, LessEqual<Required<AP.APSalesPrice.lastDate>>,
            And<AP.APSalesPrice.isPromotionalPrice, Equal<False>>>,
            Or<Where<AP.APSalesPrice.lastDate, LessEqual<Required<AP.APSalesPrice.lastDate>>,
            And<AP.APSalesPrice.expirationDate, GreaterEqual<Required<AP.APSalesPrice.expirationDate>>, And<AP.APSalesPrice.isPromotionalPrice, Equal<True>>>>>>>>,

            Or<Where<AP.APSalesPrice.lastBreakQty, LessEqual<Required<AP.APSalesPrice.lastBreakQty>>,
            And<AP.APSalesPrice.lastDate, Greater<Required<AP.APSalesPrice.lastDate>>,
            And<AP.APSalesPrice.isPromotionalPrice, Equal<False>>>>>>>>>>>,

            OrderBy<Desc<AP.APSalesPrice.isPromotionalPrice, Desc<AP.APSalesPrice.vendorID, Desc<AP.APSalesPrice.breakQty, Desc<AP.APSalesPrice.lastBreakQty>>>>>>(sender.Graph);

            APSalesPrice item = unitCost.SelectWindowed(0, 1, inventoryID, vendorID, vendorLocationID, curyID, UOM, quantity, date, date, date, quantity, date);

            string uomFound = null;

            if (item == null)
            {
                decimal baseUnitQty = INUnitAttribute.ConvertToBase(sender, inventoryID, UOM, (decimal)quantity, INPrecision.QUANTITY);
                item = unitCostBaseUOM.Select(inventoryID, vendorID, vendorLocationID, curyID, baseUnitQty, date, date, date, quantity, date);

                if (item == null)
                {
                    return null;
                }
                else
                {
                    uomFound = item.UOM;
                }
            }
            else
            {
                uomFound = UOM;
            }

            if (item == null)
            {
                return null;
            }

            if (item.LastDate != null && date < item.LastDate.Value)
                return new UnitCostItem(uomFound, (item.LastPrice ?? 0), item.CuryID);
            else
                return new UnitCostItem(uomFound, (item.SalesPrice ?? 0), item.CuryID);
        }

        public class UnitCostItem
        {
            private string uom;

            public string UOM
            {
                get { return uom; }
            }

            private decimal cost;

            public decimal Cost
            {
                get { return cost; }
            }

            private string curyid;
            public string CuryID
            {
                get { return curyid; }
            }

            public UnitCostItem(string uom, decimal cost, string curyid)
            {
                this.uom = uom;
                this.cost = cost;
                this.curyid = curyid;
            }

        }

        #endregion

		#region PXImportAttribute.IPXPrepareItems
		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (string.Compare(viewName, _ITEMS_VIEW_NAME, true) == 0)
			{
				if (values[_curyIDFieldName] == null)
					values[_curyIDFieldName] = Filter.Current.CuryID;

				if (values[_vendorID] == null)
					values[_vendorID] = Filter.Current.VendorID;

				if (values[_vendorLocationID] == null)
					values[_vendorLocationID] = Filter.Current.LocationID;

				if (values[_breakQty] == null)
					values[_breakQty] = "0.00";

				if (values[_pendingBreakQty] == null)
					values[_pendingBreakQty] = "0.00";

				string inventoryCD = (string)values[_inventoryID];
				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>>>.Select(this, inventoryCD);
				if (item != null)
				{
					string uom = (string)values[_uOM];

					PXSelectBase<APSalesPrice> selectOld_row = new PXSelect<APSalesPrice,
						Where<APSalesPrice.curyID, Equal<Required<APSalesPrice.curyID>>,
						And<APSalesPrice.vendorID, Equal<Required<APSalesPrice.vendorID>>,
						And2<Where<APSalesPrice.vendorLocationID, IsNull, And<Required<APSalesPrice.vendorLocationID>, IsNull, Or<APSalesPrice.vendorLocationID, Equal<Required<APSalesPrice.vendorLocationID>>>>>,
						And<APSalesPrice.inventoryID, Equal<Required<APSalesPrice.inventoryID>>,
						And<APSalesPrice.uOM, Equal<Required<APSalesPrice.uOM>>,
						And<APSalesPrice.isPromotionalPrice, Equal<Required<APSalesPrice.isPromotionalPrice>>>>>>>>>(this);

					APSalesPrice old_row;
					if (Filter.Current.PromotionalPrice == true)
					{
						selectOld_row.WhereAnd<Where<APSalesPrice.breakQty, Equal<Required<APSalesPrice.breakQty>>, And<APSalesPrice.lastDate, Equal<Required<APSalesPrice.lastDate>>, And<APSalesPrice.expirationDate, Equal<Required<APSalesPrice.expirationDate>>>>>>();
						old_row = selectOld_row.SelectWindowed(0, 1, Filter.Current.CuryID, Filter.Current.VendorID, Filter.Current.LocationID, Filter.Current.LocationID, item.InventoryID, uom ?? item.SalesUnit, Filter.Current.PromotionalPrice, values[_breakQty], values[_lastDate], values[_expirationDate]);
					}
					else
					{
						selectOld_row.WhereAnd<Where<APSalesPrice.pendingBreakQty, Equal<Required<APSalesPrice.pendingBreakQty>>>>();
						old_row = selectOld_row.SelectWindowed(0, 1, Filter.Current.CuryID, Filter.Current.VendorID, Filter.Current.LocationID, Filter.Current.LocationID, item.InventoryID, uom ?? item.SalesUnit, Filter.Current.PromotionalPrice, values[_pendingBreakQty]);
					}

					if (old_row != null)
					{
						if (keys.Contains(_recordID))
						{
							keys[_recordID] = old_row.RecordID;
							values[_recordID] = old_row.RecordID;
						}
					}
				}
			}
			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public virtual void PrepareItems(string viewName, IEnumerable items)
		{
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

	[Serializable]
	public partial class APSalesPriceFilter : IBqlTable
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
		#region WorkGroupID
		public abstract class workGroupID : PX.Data.IBqlField
		{
		}
		protected Int32? _WorkGroupID;
		[PXDBInt]
		[PXUIField(DisplayName = "Product Workgroup")]
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

		#region VendorID
		public abstract class vendorID : PX.Data.IBqlField
		{
		}
		protected Int32? _VendorID;
		[PXUIField(DisplayName = "Vendor")]
		[VendorNonEmployeeActive()]
		[PXParent(typeof(Select<Vendor, Where<Vendor.bAccountID, Equal<Current<APSalesPriceFilter.vendorID>>>>))]
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
		#region LocationID
		public abstract class locationID : PX.Data.IBqlField
		{
		}
		protected Int32? _LocationID;
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<APSalesPriceFilter.vendorID>>>),
			DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Search<Vendor.defLocationID, Where<Vendor.bAccountID, Equal<Current<APSalesPriceFilter.vendorID>>>>))]
		[PXFormula(typeof(Default<APSalesPriceFilter.vendorID>))]
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
		#region ItemClassID
		public abstract class itemClassID : PX.Data.IBqlField
		{
		}
		protected String _ItemClassID;
		[PXDBString(10)]
		[PXUIField(DisplayName = "Item Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(INItemClass.itemClassID), DescriptionField = typeof(INItemClass.descr))]
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

		#region CuryID
		public abstract class curyID : PX.Data.IBqlField
		{
		}
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, BqlField = typeof(Vendor.curyID))]
		[PXSelector(typeof(Search<Currency.curyID>))]
		[PXUIField(DisplayName = "Currency")]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		
		#region PromotionalPrice
		public abstract class promotionalPrice : PX.Data.IBqlField
		{
		}
		[PXDefault(false)]
		[PXDBBool]
		[PXUIField(DisplayName = "Promotional Prices")]
		public virtual Boolean? PromotionalPrice
		{ get; set; }
		#endregion


	}
}
