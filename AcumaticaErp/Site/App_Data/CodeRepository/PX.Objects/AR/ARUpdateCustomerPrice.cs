using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.TM;

namespace PX.Objects.AR
{
	[PX.Objects.GL.TableAndChartDashboardType]
	[Serializable]
	public class ARUpdateCustomerPrice : PXGraph<ARUpdateCustomerPrice>
	{
		public PXCancel<ItemFilter> Cancel;
		public PXFilter<ItemFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessingJoin<ARSalesPrice, ItemFilter,
						InnerJoin<InventoryItem, On<ARSalesPrice.inventoryID, Equal<InventoryItem.inventoryID>>>,
						Where<ARSalesPrice.effectiveDate, LessEqual<Current<ItemFilter.pendingBasePriceDate>>,
						And<ARSalesPrice.isPromotionalPrice, Equal<boolFalse>,
						And<ARSalesPrice.isCustClassPrice, Equal<boolFalse>,
			And<Where2<Where<Current<ItemFilter.customerID>, IsNull,
										Or<Current<ItemFilter.customerID>, Equal<ARSalesPrice.customerID>>>,
				And2<Where<Current<ItemFilter.ownerID>, IsNull,
					Or<Current<ItemFilter.ownerID>, Equal<InventoryItem.priceManagerID>>>,
								And2<Where<Current<ItemFilter.myWorkGroup>, Equal<boolFalse>,
									 Or<InventoryItem.priceWorkgroupID, InMember<CurrentValue<ItemFilter.currentOwnerID>>>>,
				And<Where<Current<ItemFilter.workGroupID>, IsNull,
					Or<Current<ItemFilter.workGroupID>, Equal<InventoryItem.priceWorkgroupID>>>>>>>>>>>> Items;

		#region CacheAttached
		[PXBool()]
		[PXUIField(DisplayName = "Selected")]
		public virtual void ARSalesPrice_Selected_CacheAttached(PXCache sender)
		{
		}

		#endregion

		public CMSetupSelect CMSetup;

		public ARUpdateCustomerPrice()
		{
			Items.SetSelected<ARSalesPrice.selected>();

			Items.SetProcessDelegate<ARUpdateSalesPriceProcess>(UpdateSalesPrice);
			Items.SetProcessCaption(SO.Messages.Process);
			Items.SetProcessAllCaption(Messages.ProcessAll);
		}

		protected virtual void ItemFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ItemFilter filter = e.Row as ItemFilter;
			if (filter == null) return;
			CMSetup cmsetup = CMSetup.Current;

			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(OwnedFilter.ownerID).Name, e.Row == null || (bool?)sender.GetValue(e.Row, typeof(OwnedFilter.myOwner).Name) == false);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(OwnedFilter.workGroupID).Name, e.Row == null || (bool?)sender.GetValue(e.Row, typeof(OwnedFilter.myWorkGroup).Name) == false);
		}

		public static void UpdateSalesPrice(ARUpdateSalesPriceProcess graph, ARSalesPrice item)
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
			#region CustomerID
			public abstract class customerID : PX.Data.IBqlField
			{
			}
			protected Int32? _CustomerID;
			[Customer]
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
		}
		#endregion
	}
}
