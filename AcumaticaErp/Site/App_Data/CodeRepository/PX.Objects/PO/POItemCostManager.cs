using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Objects.IN;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CR;

namespace PX.Objects.PO
{
    [Serializable]
	public static class POItemCostManager
	{
		public class ItemCost
		{
			public ItemCost(InventoryItem item, decimal cost)
				: this(item, null, null, cost, cost)
			{
			}

			public ItemCost(InventoryItem item, string uom, decimal cost)
				: this(item, uom, null, cost, cost)
			{
			}

			public ItemCost(InventoryItem item, string uom, string curyID, decimal cost, decimal baseCost)
			{
				this.Item = item;
				this.uom = uom;
				this.CuryID = curyID;
				this.Cost = cost;
				this.BaseCost = baseCost;
			}

			public string UOM
			{
				get
				{
					return this.Item != null ? this.uom ?? this.Item.BaseUnit : null;
				}
			}

			private readonly string uom;

			public readonly InventoryItem Item;
			public readonly string CuryID;
			public readonly decimal Cost;
			public readonly decimal BaseCost;

			public decimal Convert<InventoryIDField, CuryInfoIDField>(PXGraph graph, object row, string uom)
				where InventoryIDField : IBqlField
				where CuryInfoIDField : IBqlField
			{
				return Convert<InventoryIDField, CuryInfoIDField>(graph, row, row, uom);
			}

			public decimal Convert<InventoryIDField, CuryInfoIDField>(PXGraph graph, object inventoryRow, object curyRow, string uom)
				where InventoryIDField : IBqlField
				where CuryInfoIDField : IBqlField
			{
				ItemCost price = this;
				if (price == null || price.Cost == 0 || price.Item == null || inventoryRow == null || curyRow == null)
					return 0;

				decimal result = price.Cost;

				PXCache curyCache = graph.Caches[curyRow.GetType()];
				PXCurrencyAttribute.CuryConvCury<CuryInfoIDField>(curyCache, curyRow, price.BaseCost, out result, true);


				if (price.UOM != uom && !string.IsNullOrEmpty(uom))
				{
					if (inventoryRow == null) return 0;

					PXCache invCache = graph.Caches[inventoryRow.GetType()];
					decimal baseUOM =
						price.UOM != price.Item.BaseUnit ?
						INUnitAttribute.ConvertFromBase<InventoryIDField>(invCache, inventoryRow, price.UOM, result, INPrecision.UNITCOST) :
						result;

					result =
						uom != price.Item.BaseUnit ?
						INUnitAttribute.ConvertToBase<InventoryIDField>(invCache, inventoryRow, uom, baseUOM, INPrecision.UNITCOST) :
						baseUOM;
				}

				return result;
			}

		}

		private static ItemCost FetchHistoryLastCost(PXGraph aGraph, int? VendorID, int? VendorLocationID, int? aInventoryID, int? aSubItemID)
		{
			//Not implemented yet
			return null;
		}

		private static ItemCost FetchLastCost(PXGraph graph, InventoryItem item, DateTime? docDate)
		{
			if (item.StkItem == false || item.ValMethod == INValMethod.Standard)
			{
				if (!docDate.HasValue || (item.StdCostDate.HasValue && item.StdCostDate.Value <= docDate.Value))
				{
					return new ItemCost(item, item.StdCost.Value);
				}
			}
			return null;
		}

		private static ItemCost FetchSiteLastCost(PXGraph graph, InventoryItem item, INItemCost cost, int? siteID)
		{
			decimal? result = null;

			INItemStats res = null;
			if (siteID != null)
			{
				res = (INItemStats)PXSelect<INItemStats,
					 Where<INItemStats.inventoryID, Equal<Required<INItemStats.inventoryID>>,
					 And<INItemStats.siteID, Equal<Required<INItemStats.siteID>>>>>.
					 Select(graph, item.InventoryID, siteID);
			}
			else
			{
				res = (INItemStats)PXSelect<INItemStats,
					 Where<INItemStats.inventoryID, Equal<Required<INItemStats.inventoryID>>>,
						OrderBy<Desc<INItemStats.lastCostDate>>>.Select(graph, item.InventoryID);
			}
			if (res != null)
				result = res.LastCost;

			return new ItemCost(item, (result ?? cost.LastCost).GetValueOrDefault());
		}

		public static int? FetchLocation(PXGraph graph, int? vendorID, int? itemID, int? subItemID, int? siteID)
		{
			BAccountR company = PXSelectJoin<BAccountR, 
				InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>, 
				Where<BAccountR.bAccountID, Equal<Required<BAccountR.bAccountID>>>>.Select(graph, vendorID);
			if (company != null)
			{
				return company.DefLocationID;
			}

			var rec = (PXResult<INItemSiteSettings, Vendor, POVendorInventory>)
			PXSelectJoin<INItemSiteSettings,
					LeftJoin<Vendor,
						On<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>,
					LeftJoin<POVendorInventory,
						On<POVendorInventory.inventoryID, Equal<INItemSiteSettings.inventoryID>,
						And<POVendorInventory.active, Equal<boolTrue>,
						And<POVendorInventory.vendorID, Equal<Vendor.bAccountID>,
						And<Where<POVendorInventory.subItemID, Equal<Required<POVendorInventory.subItemID>>,
								Or<POVendorInventory.subItemID, Equal<INItemSiteSettings.defaultSubItemID>,
								Or<POVendorInventory.subItemID, IsNull,
								Or<Where<Required<POVendorInventory.subItemID>, IsNull,
								     And<POVendorInventory.subItemID, Equal<True>>>>>>>>>>>>>,
				Where<INItemSiteSettings.inventoryID, Equal<Required<INItemSiteSettings.inventoryID>>,
					And<INItemSiteSettings.siteID, Equal<Required<INItemSiteSettings.siteID>>>>,
				OrderBy<Asc<POVendorInventory.effPrice,
						Asc<Switch<Case<Where<POVendorInventory.subItemID, Equal<INItemSiteSettings.defaultSubItemID>>, boolTrue>, boolFalse>,
						Asc<Switch<Case<Where<POVendorInventory.vendorLocationID, IsNull>, boolTrue>, boolFalse>>>>>>
					.SelectWindowed(graph, 0, 1, vendorID, subItemID, subItemID, itemID, siteID);
			if (rec == null) return null;
			POVendorInventory i = rec;
			INItemSiteSettings site = rec;
			Vendor vendor = rec;

			if (i.VendorLocationID != null) 
				return i.VendorLocationID;
			if (site.PreferredVendorID == vendorID)
				return site.PreferredVendorLocationID ?? vendor.DefLocationID;
			if(vendor.BAccountID == vendorID)
				return vendor.DefLocationID;
			return null;
		}

		public static decimal? Fetch<InventoryIDField, CuryInfoIDField>(PXGraph graph, object row, int? vendorID, int? vendorLocationID, DateTime? docDate, string curyID, int? inventoryID, int? subItemID, int? siteID, string uom)
			where InventoryIDField : IBqlField
			where CuryInfoIDField : IBqlField
		{
			ItemCost price = Fetch(graph, vendorID, vendorLocationID, docDate, curyID, inventoryID, subItemID, siteID, uom, false);
			return price.Convert<InventoryIDField, CuryInfoIDField>(graph, row, uom);
		}	
	
		public static decimal? Fetch<InventoryIDField, CuryInfoIDField>(PXGraph graph, object row, int? vendorID, int? vendorLocationID, DateTime? docDate, string curyID, int? inventoryID, int? subItemID, int? siteID, string uom, bool onlyVendor)
			where InventoryIDField : IBqlField
			where CuryInfoIDField : IBqlField
		{
			ItemCost price = Fetch(graph, vendorID, vendorLocationID, docDate, curyID, inventoryID, subItemID, siteID, uom, onlyVendor);
			return price.Convert<InventoryIDField, CuryInfoIDField>(graph, row, uom);
		}
		public static ItemCost Fetch(PXGraph graph, int? vendorID, int? vendorLocationID, DateTime? docDate, string curyID, int? inventoryID, int? subItemID, int? siteID, string uom)
		{
			return Fetch(graph, vendorID, vendorLocationID, docDate, curyID, inventoryID, subItemID, siteID, uom, false);
		}
		public static ItemCost Fetch(PXGraph graph, int? vendorID, int? vendorLocationID, DateTime? docDate, string curyID, int? inventoryID, int? subItemID, int? siteID, string uom, bool onlyVendor)
		{
			InventoryItem item = null;
			INItemCost cost = null;
			Vendor vendor = null;
			POVendorInventory vendorPrice = null;

			foreach (PXResult<InventoryItem, INItemCost, Vendor, POVendorInventory> r in
				PXSelectJoin<InventoryItem,
					LeftJoin<INItemCost, On<INItemCost.inventoryID, Equal<InventoryItem.inventoryID>>,
					LeftJoin<Vendor,
								 On<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>,
					LeftJoin<POVendorInventory,
								 On<POVendorInventory.inventoryID, Equal<InventoryItem.inventoryID>,
								And<POVendorInventory.active, Equal<boolTrue>,
								And<POVendorInventory.vendorID, Equal<Vendor.bAccountID>,
							 And2<Where<POVendorInventory.subItemID, Equal<Required<POVendorInventory.subItemID>>,
											Or<POVendorInventory.subItemID, Equal<InventoryItem.defaultSubItemID>,
											Or<POVendorInventory.subItemID, IsNull,
											Or<Where<Required<POVendorInventory.subItemID>, IsNull,
											     And<POVendorInventory.subItemID, Equal<True>>>>>>>,

							 And2<Where<POVendorInventory.purchaseUnit, Equal<Required<POVendorInventory.purchaseUnit>>,
											Or<POVendorInventory.purchaseUnit, Equal<InventoryItem.purchaseUnit>>>,
							 And<Where<POVendorInventory.vendorLocationID, Equal<Required<POVendorInventory.vendorLocationID>>,
											Or<POVendorInventory.vendorLocationID, IsNull>>>>>>>>>>>,
					Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>,
					OrderBy<
					  Asc<Switch<Case<Where<POVendorInventory.purchaseUnit, Equal<InventoryItem.purchaseUnit>>, boolTrue>, boolFalse>,
						Asc<Switch<Case<Where<POVendorInventory.subItemID, Equal<InventoryItem.defaultSubItemID>>, boolTrue>, boolFalse>,
						Asc<Switch<Case<Where<POVendorInventory.vendorLocationID, IsNull>, boolTrue>, boolFalse>,
						Asc<InventoryItem.inventoryCD>>>>>>
					.Select(graph, vendorID, subItemID, subItemID, uom, vendorLocationID, inventoryID))
			{
				item = r;
				cost = r;
				vendor = r;
				vendorPrice = r;
				if (vendorPrice != null) break;
			}
			if (vendorPrice != null && vendorPrice.VendorID != null)
			{
				Decimal? price =
					(docDate != null && docDate >= vendorPrice.EffDate && vendorPrice.EffPrice > 0)
					? vendorPrice.EffPrice
					: vendorPrice.LastPrice;
				string purchaseUnit = vendorPrice.PurchaseUnit ?? item.PurchaseUnit;

				if (price == 0) price = null;

				if (price != null)
				{
					Company company = new PXSetup<Company>(graph).Current;
					if (vendorPrice.CuryID == company.BaseCuryID || vendorPrice.CuryID == null)
						return new ItemCost(item, purchaseUnit, price.Value);
					try
					{
						Decimal basePrice;
						PXCurrencyAttribute.CuryConvBase(graph.Caches[typeof(CurrencyInfo)], GetCurrencyInfo(graph, vendor.CuryRateTypeID, vendorPrice.CuryID), price.Value, out basePrice, true);
						return new ItemCost(item, purchaseUnit, vendorPrice.CuryID, price.Value, basePrice);
					}
					catch
					{
						return new ItemCost(item, purchaseUnit, 0);
					}
				}
			}

			return
				FetchHistoryLastCost(graph, vendorID, vendorLocationID, inventoryID, subItemID) ??
				(onlyVendor ? null : FetchLastCost(graph, item, docDate)) ??
				(onlyVendor ? null : FetchSiteLastCost(graph, item, cost, siteID)) ??
				new ItemCost(item, 0);
		}
	
		private static CurrencyInfo GetCurrencyInfo(PXGraph graph, string curyRateTypeID, string curyID)
		{
			if (curyID == null)
				curyID = new PXSetup<Company>(graph).Current.BaseCuryID;
			if (curyRateTypeID == null)
				curyRateTypeID = new CMSetupSelect(graph).Current.APRateTypeDflt;

			CurrencyInfo info = new CurrencyInfo();
			info.ModuleCode = "AP";
			info.CuryID = curyID;
			info.CuryRateTypeID = curyRateTypeID;
			info.CuryInfoID = null;
			PXCache cache = graph.Caches[typeof(CurrencyInfo)];
			info = (CurrencyInfo)cache.Insert(info);
			cache.Delete(info);
			return info;
		}

		public static void Update(PXGraph graph, int? vendorID, int? vendorLocationID, string curyID, int? inventoryID, int? subItemID, string uom, decimal curyCost)
		{
			if (curyCost <= 0 || string.IsNullOrEmpty(uom) ||
				vendorID == null ||
				vendorLocationID == null) return;
			
			PXCache cache = graph.Caches[typeof(POVendorInventoryPriceUpdate)];

			foreach (PXResult<InventoryItem, Vendor, Company> r in
				PXSelectJoin<InventoryItem,
					LeftJoin<Vendor,
								 On<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>,
					CrossJoin<Company>>,
					Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.
					Select(graph, vendorID, inventoryID))
			{
				InventoryItem item = r;
				Vendor vendor = r;
				Company company = r;
				if (item.InventoryID == null || vendor.BAccountID == null || 
					(item.StkItem == true && subItemID == null)) continue;
				INSetup setup = new PXSetup<INSetup>(graph).Current;

                int? savedSubItemID = item.StkItem == true ? subItemID : null;
					
				POVendorInventory existVendorPrice = PXSelectReadonly<POVendorInventory,
					Where<POVendorInventory.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
						And<POVendorInventory.subItemID, Equal<Required<POVendorInventory.subItemID>>,
						And<POVendorInventory.purchaseUnit, Equal<Required<POVendorInventory.purchaseUnit>>,
						And<POVendorInventory.vendorID, Equal<Required<POVendorInventory.vendorID>>,
						And<POVendorInventory.vendorLocationID, Equal<Required<POVendorInventory.vendorLocationID>>>>>>>>
					.SelectWindowed(graph, 0, 1, item.InventoryID, savedSubItemID, uom, vendorID, vendorLocationID);
				string priceCuryID = (existVendorPrice != null ? existVendorPrice.CuryID : vendor.CuryID) ?? company.BaseCuryID;

				POVendorInventoryPriceUpdate vendorPrice = (POVendorInventoryPriceUpdate)cache.CreateInstance();
				vendorPrice.InventoryID = inventoryID;
				vendorPrice.SubItemID = savedSubItemID;
				vendorPrice.VendorID = vendorID;
				vendorPrice.VendorLocationID = vendorLocationID;
				vendorPrice.PurchaseUnit = uom;			
				vendorPrice = (POVendorInventoryPriceUpdate)cache.Insert(vendorPrice);
                if (item.StkItem != true) vendorPrice.SubItemID = savedSubItemID;
                vendorPrice.CuryID = priceCuryID; 
                cache.Normalize();
				vendorPrice.Active = true;
				Decimal cost = ConventCury(graph, vendor.CuryRateTypeID, curyID, curyCost, priceCuryID);
				vendorPrice.LastPrice = ConvertUOM(graph, item, uom, cost, item.PurchaseUnit);				
			}
		}
		
		public static Decimal ConventCury(PXGraph graph, string curyRateTypeID, string curyID, decimal curyCost, string destinationCuryID)
		{
			if (curyID == destinationCuryID) return curyCost;

			Decimal baseCost = curyCost;
			string BaseCuryID = new PXSetup<Company>(graph).Current.BaseCuryID;
			if (curyID != BaseCuryID)
				PXCurrencyAttribute.CuryConvBase(graph.Caches[typeof(CurrencyInfo)], GetCurrencyInfo(graph, curyRateTypeID, curyID), curyCost, out baseCost, true);
			Decimal cost = baseCost;
			if (destinationCuryID != BaseCuryID)
				PXCurrencyAttribute.CuryConvCury(graph.Caches[typeof(CurrencyInfo)], GetCurrencyInfo(graph, curyRateTypeID, destinationCuryID), baseCost, out cost, true);
			return cost;
		}

		public static Decimal ConvertUOM(PXGraph graph, InventoryItem item, string uom, decimal cost, string destinationUOM)
		{
			if (item == null) return 0;
			if (destinationUOM == uom) return cost;

			PXCache invCache = graph.Caches[typeof(InventoryItem)];
			Decimal baseCost =
				uom != item.BaseUnit ?
				INUnitAttribute.ConvertFromBase<InventoryItem.inventoryCD>(invCache, item, uom, cost, INPrecision.UNITCOST) :
				cost;

			return
				destinationUOM != item.BaseUnit ?
				INUnitAttribute.ConvertToBase<InventoryItem.inventoryCD>(invCache, item, destinationUOM, baseCost, INPrecision.UNITCOST) :
				baseCost;

		}

		[POVendorInventoryAccumulator]
        [Serializable]
		public partial class POVendorInventoryPriceUpdate : POVendorInventory
		{
			#region RecordID
			public new abstract class recordID : PX.Data.IBqlField
			{
			}
			[PXDBIdentity()]
			public override Int32? RecordID
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
			#region VendorID
			public new abstract class vendorID : PX.Data.IBqlField
			{
			}
			[VendorNonEmployeeActive(Visibility = PXUIVisibility.SelectorVisible,
				DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true, DisplayName = "Vendor ID", IsKey = true)]			
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
			[LocationID(typeof(Where<Location.bAccountID, Equal<Current<POVendorInventoryPriceUpdate.vendorID>>>),
				DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location", IsKey = true)]			
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
			#region InventoryID
			public new abstract class inventoryID : PX.Data.IBqlField
			{
			}
			[Inventory(Filterable = true, DirtyRead = true, IsKey = true)]			
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
			#region SubItemID
			public new abstract class subItemID : PX.Data.IBqlField
			{
			}
			[SubItem(typeof(POVendorInventoryPriceUpdate.inventoryID), DisplayName = "Subitem", BqlField = typeof(POVendorInventory.subItemID), IsKey = true)]
			public override Int32? SubItemID
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
			#region PurchaseUnit
			public new abstract class purchaseUnit : PX.Data.IBqlField
			{
			}
			[INUnit(typeof(POVendorInventory.inventoryID), DisplayName = "Purchase Unit", Visibility = PXUIVisibility.Visible, IsKey = true)]
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
			#region LastPrice
			public new abstract class lastPrice : PX.Data.IBqlField
			{
			}
			[PXDBPriceCost(BqlField = typeof(POVendorInventory.lastPrice))]
			[PXUIField(DisplayName = "Last Vendor Price", Enabled = false)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public override Decimal? LastPrice
			{
				get
				{
					return this._LastPrice;
				}
				set
				{
					this._LastPrice = value;
				}
			}
			#endregion
			#region Active
			public new abstract class active : PX.Data.IBqlField
			{
			}
			[PXDBBool()]
			[PXDefault(true)]
			[PXUIField(DisplayName = "Active")]
			public override Boolean? Active
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

			#region System Columns
			#region tstamp
			public new abstract class Tstamp : PX.Data.IBqlField
			{
			}			
			public override Byte[] tstamp
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
			#endregion
		}
		public class POVendorInventoryAccumulatorAttribute : PXAccumulatorAttribute
		{
			public POVendorInventoryAccumulatorAttribute()
			{
				base._SingleRecord = true;
			}
			protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
			{
				if (!base.PrepareInsert(sender, row, columns))				
					return false;				

				POVendorInventoryPriceUpdate val = (POVendorInventoryPriceUpdate)row;
				columns.Update<POVendorInventoryPriceUpdate.lastPrice>(val.CuryID, PXDataFieldAssign.AssignBehavior.Initialize);				
				columns.Update<POVendorInventoryPriceUpdate.lastPrice>(val.LastPrice, PXDataFieldAssign.AssignBehavior.Replace);
				columns.Update<POVendorInventoryPriceUpdate.active>(val.Active, PXDataFieldAssign.AssignBehavior.Replace);				
				return true;
			}
		}
	}
}
