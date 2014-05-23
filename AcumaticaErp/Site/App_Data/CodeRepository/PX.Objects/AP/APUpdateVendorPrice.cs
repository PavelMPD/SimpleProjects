using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.TM;

namespace PX.Objects.AP
{
	[PX.Objects.GL.TableAndChartDashboardType]
	[Serializable]
	public class APUpdateVendorPrice : PXGraph<APUpdateVendorPrice>
	{
		public PXCancel<ItemFilter> Cancel;
		public PXFilter<ItemFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessingJoin<APSalesPrice, ItemFilter,
						InnerJoin<InventoryItem, On<APSalesPrice.inventoryID, Equal<InventoryItem.inventoryID>>>,
						Where<APSalesPrice.effectiveDate, LessEqual<Current<ItemFilter.pendingBasePriceDate>>,
						And<APSalesPrice.isPromotionalPrice, Equal<boolFalse>,
			And<Where2<Where<Current<ItemFilter.vendorID>, IsNull,
										Or<Current<ItemFilter.vendorID>, Equal<APSalesPrice.vendorID>>>,
				And2<Where<Current<ItemFilter.inventoryID>, IsNull,
										Or<Current<ItemFilter.inventoryID>, Equal<APSalesPrice.inventoryID>>>,
				And2<Where<Current<ItemFilter.ownerID>, IsNull,
					Or<Current<ItemFilter.ownerID>, Equal<InventoryItem.priceManagerID>>>,
								And2<Where<Current<ItemFilter.myWorkGroup>, Equal<boolFalse>,
									 Or<InventoryItem.priceWorkgroupID, InMember<CurrentValue<ItemFilter.currentOwnerID>>>>,
				And<Where<Current<ItemFilter.workGroupID>, IsNull,
					Or<Current<ItemFilter.workGroupID>, Equal<InventoryItem.priceWorkgroupID>>>>>>>>>>>> Items;

		#region CacheAttached
		[PXBool()]
		[PXUIField(DisplayName = "Selected")]
		public virtual void APSalesPrice_Selected_CacheAttached(PXCache sender)
		{
		}

		#endregion

		public CMSetupSelect CMSetup;

		public APUpdateVendorPrice()
		{
			Items.SetSelected<APSalesPrice.selected>();

			Items.SetProcessDelegate<APUpdateSalesPriceProcess>(UpdateSalesPrice);
			Items.SetProcessCaption(SO.Messages.Process);
			Items.SetProcessAllCaption(SO.Messages.ProcessAll);
		}

		protected virtual void ItemFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ItemFilter filter = e.Row as ItemFilter;
			if (filter == null) return;
			CMSetup cmsetup = CMSetup.Current;

			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(OwnedFilter.ownerID).Name, e.Row == null || (bool?)sender.GetValue(e.Row, typeof(OwnedFilter.myOwner).Name) == false);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(OwnedFilter.workGroupID).Name, e.Row == null || (bool?)sender.GetValue(e.Row, typeof(OwnedFilter.myWorkGroup).Name) == false);
		}

		public static void UpdateSalesPrice(APUpdateSalesPriceProcess graph, APSalesPrice item)
		{
			graph.UpdateSalesPrice(item);
		}


		#region Local Types

		[Serializable]
		public partial class ItemFilter : IBqlTable
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
			[PXUIField(DisplayName = "Price Manager")]
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
			[PXUIField(DisplayName = "Price Workgroup")]
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


			#region PendingBasePriceDate
			public abstract class pendingBasePriceDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _PendingBasePriceDate;
			[PXDBDate()]
			[PXUIField(DisplayName = "Max. Pending Date")]
			[PXDefault(typeof(AccessInfo.businessDate))]
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
			#region VendorID
			public abstract class vendorID : PX.Data.IBqlField
			{
			}
			protected Int32? _VendorID;
			[Vendor]
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
			#region InventoryID
			public abstract class inventoryID : PX.Data.IBqlField
			{
			}
			protected Int32? _InventoryID;
			[Inventory]
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
			#region Descr
			public abstract class descr : PX.Data.IBqlField
			{
			}
			protected String _Descr;
			[PXDBString(50, IsUnicode = true)]
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
			#region DescrWildCard
			public abstract class descrWildCard : PX.Data.IBqlField
			{
			}
			protected String _DescrWildCard;
			[PXDBString(50, IsUnicode = true)]
			public virtual String DescrWildCard
			{
				get
				{
					return DescrUtils.CreateDescrWildcard(Descr);
				}
			}
			#endregion
		}

		public class DescrUtils
		{
			public static string CreateDescrWildcard(string descr)
			{
			if (string.IsNullOrEmpty(descr)) return descr;
			return "%" + descr + "%";

			}
		}
		#endregion
	}

	public class APUpdateSalesPriceProcess : PXGraph<APUpdateSalesPriceProcess>
	{

		public virtual void UpdateSalesPrice(APSalesPrice item)
		{
			using (PXConnectionScope cs = new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					DateTime updateTime = DateTime.Now;

					//FOR UI:
					item.LastPrice = item.SalesPrice;
					item.SalesPrice = item.PendingPrice;
					item.LastBreakQty = item.BreakQty;
					item.BreakQty = item.PendingBreakQty;
					item.LastDate = item.EffectiveDate;
					item.EffectiveDate = null;
					item.PendingPrice = 0;
					item.PendingBreakQty = 0;

					PXDatabase.Update<APSalesPrice>(
									new PXDataFieldAssign(typeof(APSalesPrice.lastPrice).Name, PXDbType.Decimal, item.LastPrice),
									new PXDataFieldAssign(typeof(APSalesPrice.lastBreakQty).Name, PXDbType.Decimal, item.LastBreakQty),
									new PXDataFieldAssign(typeof(APSalesPrice.salesPrice).Name, PXDbType.Decimal, item.SalesPrice),
									new PXDataFieldAssign(typeof(APSalesPrice.breakQty).Name, PXDbType.Decimal, item.BreakQty),
									new PXDataFieldAssign(typeof(APSalesPrice.lastDate).Name, PXDbType.DateTime, item.LastDate),
									new PXDataFieldAssign(typeof(APSalesPrice.pendingPrice).Name, PXDbType.Decimal, 0),
									new PXDataFieldAssign(typeof(APSalesPrice.effectiveDate).Name, PXDbType.DateTime, null),
									new PXDataFieldAssign(typeof(APSalesPrice.pendingBreakQty).Name, PXDbType.Decimal, 0),
									new PXDataFieldAssign(typeof(APSalesPrice.lastModifiedDateTime).Name, PXDbType.DateTime, updateTime),

									new PXDataFieldRestrict(typeof(APSalesPrice.recordID).Name, PXDbType.Int, item.RecordID),
									PXDataFieldRestrict.OperationSwitchAllowed
									);

					PXDatabase.Update<APSalesPrice>(
									new PXDataFieldAssign(typeof(APSalesPrice.salesPrice).Name, PXDbType.Decimal, item.SalesPrice),
									new PXDataFieldAssign(typeof(APSalesPrice.lastDate).Name, PXDbType.DateTime, item.LastDate),
									new PXDataFieldAssign(typeof(APSalesPrice.lastModifiedDateTime).Name, PXDbType.DateTime, updateTime),

									new PXDataFieldRestrict(typeof(APSalesPrice.inventoryID).Name, PXDbType.Int, item.InventoryID),
									new PXDataFieldRestrict(typeof(APSalesPrice.vendorID).Name, PXDbType.Int, item.VendorID),
									new PXDataFieldRestrict(typeof(APSalesPrice.vendorLocationID).Name, PXDbType.Int, item.VendorLocationID),
									new PXDataFieldRestrict(typeof(APSalesPrice.curyID).Name, PXDbType.NVarChar, item.CuryID),
									new PXDataFieldRestrict(typeof(APSalesPrice.uOM).Name, PXDbType.NVarChar, item.UOM),
									new PXDataFieldRestrict(typeof(APSalesPrice.subItemID).Name, PXDbType.Int, item.SubItemID),
									new PXDataFieldRestrict(typeof(APSalesPrice.breakQty).Name, PXDbType.Decimal, item.BreakQty),
									new PXDataFieldRestrict(typeof(APSalesPrice.isPromotionalPrice).Name, PXDbType.Bit, false),
									new PXDataFieldRestrict(typeof(APSalesPrice.recordID).Name, PXDbType.Int, 4, item.RecordID, PXComp.NE)
									);

					if (item.BreakQty == 0)
					{
						PXDatabase.Update<PO.POVendorInventory>(
										new PXDataFieldAssign(typeof(PO.POVendorInventory.lastPrice).Name, PXDbType.Decimal, item.LastPrice),
										new PXDataFieldAssign(typeof(PO.POVendorInventory.effPrice).Name, PXDbType.Decimal, item.SalesPrice),
										new PXDataFieldAssign(typeof(PO.POVendorInventory.effDate).Name, PXDbType.DateTime, item.LastDate),
										new PXDataFieldAssign(typeof(PO.POVendorInventory.pendingPrice).Name, PXDbType.Decimal, 0),
										new PXDataFieldAssign(typeof(PO.POVendorInventory.pendingDate).Name, PXDbType.DateTime, null),
										new PXDataFieldAssign(typeof(PO.POVendorInventory.lastModifiedDateTime).Name, PXDbType.DateTime, updateTime),

										new PXDataFieldRestrict(typeof(PO.POVendorInventory.inventoryID).Name, PXDbType.Int, item.InventoryID),
										new PXDataFieldRestrict(typeof(PO.POVendorInventory.vendorID).Name, PXDbType.Int, item.VendorID),
										new PXDataFieldRestrict(typeof(PO.POVendorInventory.vendorLocationID).Name, PXDbType.Int, item.VendorLocationID),
										new PXDataFieldRestrict(typeof(PO.POVendorInventory.curyID).Name, PXDbType.NVarChar, item.CuryID),
										new PXDataFieldRestrict(typeof(PO.POVendorInventory.purchaseUnit).Name, PXDbType.NVarChar, item.UOM),
										new PXDataFieldRestrict(typeof(PO.POVendorInventory.subItemID).Name, PXDbType.Int, item.SubItemID)
										);
					}


					ts.Complete();
				}
			}

		}
	}
}
