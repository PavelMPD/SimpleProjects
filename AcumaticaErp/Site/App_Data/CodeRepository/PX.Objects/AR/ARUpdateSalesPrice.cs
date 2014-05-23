using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using System.Collections;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.CM;
using PX.TM;
using PX.Objects.SO;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	[PX.Objects.GL.TableAndChartDashboardType]
    [Serializable]
	public class ARUpdateSalesPrice : PXGraph<ARUpdateSalesPrice>
	{
		public PXCancel<ItemFilter> Cancel;
		public PXFilter<ItemFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessingJoin<ARSalesPrice, ItemFilter,
						InnerJoin<InventoryItem, On<ARSalesPrice.inventoryID, Equal<InventoryItem.inventoryID>>>,
						Where<ARSalesPrice.effectiveDate, LessEqual<Current<ItemFilter.pendingBasePriceDate>>,
						And<ARSalesPrice.isPromotionalPrice, Equal<boolFalse>,
						And<ARSalesPrice.isCustClassPrice, Equal<boolTrue>,
            And<Where2<Where<Current<ItemFilter.curyID>, IsNull,
										Or<Current<ItemFilter.curyID>, Equal<ARSalesPrice.curyID>>>,
                And2<Where<Current<ItemFilter.custPriceClassID>, IsNull,
										Or<Current<ItemFilter.custPriceClassID>, Equal<ARSalesPrice.custPriceClassID>>>,
                And2<Where<Current<ItemFilter.ownerID>, IsNull,
                    Or<Current<ItemFilter.ownerID>, Equal<InventoryItem.priceManagerID>>>,
								And2<Where<Current<ItemFilter.myWorkGroup>, Equal<boolFalse>,
									 Or<InventoryItem.priceWorkgroupID, InMember<CurrentValue<ItemFilter.currentOwnerID>>>>,
                And<Where<Current<ItemFilter.workGroupID>, IsNull,
                    Or<Current<ItemFilter.workGroupID>, Equal<InventoryItem.priceWorkgroupID>>>>>>>>>>>>> Items;

		#region CacheAttached
		[PXBool()]
		[PXUIField(DisplayName = "Selected")]
		public virtual void ARSalesPrice_Selected_CacheAttached(PXCache sender)
		{
		}
		
		#endregion				

		public CMSetupSelect CMSetup;		

        public ARUpdateSalesPrice()
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
			CMSetup	cmsetup = CMSetup.Current;
			PXUIFieldAttribute.SetVisible<ItemFilter.curyID>(sender, filter, (bool)cmsetup.MCActivated);

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
			#region CustPriceClassID
			public abstract class custPriceClassID : PX.Data.IBqlField
			{
			}
			protected String _CustPriceClassID;
			[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
			[PXUIField(DisplayName = "Customer Price Class ID", Visibility = PXUIVisibility.SelectorVisible)]
			[CustomerPriceClass(DescriptionField=typeof(ARPriceClass.description))]
			public virtual String CustPriceClassID
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
			#region CuryID
			public abstract class curyID : PX.Data.IBqlField
			{
			}
			protected string _CuryID;
			[PXDBString(5, IsUnicode = true)]
			[PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
			[PXUIField(DisplayName = "Currency")]
			public virtual string CuryID
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
		}		
		#endregion
	}

	public class ARUpdateSalesPriceProcess : PXGraph<ARUpdateSalesPriceProcess>
	{
		public PXSetup<Company> Company;
		public PXSetup<SOSetup> sosetup;
		public PXSelect<InventoryItem> Inventory;

		public virtual void UpdateSalesPrice(ARSalesPrice item)
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
					item.LastTaxID = item.TaxID;
					item.TaxID = item.PendingTaxID;
					item.PendingTaxID = null;


					PXDatabase.Update<ARSalesPrice>(
									new PXDataFieldAssign("LastPrice", PXDbType.Decimal, item.LastPrice),
									new PXDataFieldAssign("LastTaxID", PXDbType.NVarChar, item.LastTaxID),
									new PXDataFieldAssign("LastBreakQty", PXDbType.Decimal, item.LastBreakQty),
									new PXDataFieldAssign("SalesPrice", PXDbType.Decimal, item.SalesPrice),
									new PXDataFieldAssign("TaxID", PXDbType.NVarChar, item.TaxID),
									new PXDataFieldAssign("BreakQty", PXDbType.Decimal, item.BreakQty),
									new PXDataFieldAssign("LastDate", PXDbType.DateTime, item.LastDate),
									new PXDataFieldAssign("PendingPrice", PXDbType.Decimal, 0m),
									new PXDataFieldAssign("PendingTaxID", PXDbType.Decimal, null),
									new PXDataFieldAssign("EffectiveDate", PXDbType.DateTime, null),
									new PXDataFieldAssign("PendingBreakQty", PXDbType.Decimal, 0m),
									new PXDataFieldAssign("LastModifiedDateTime", PXDbType.DateTime, updateTime),

									new PXDataFieldRestrict("RecordID", PXDbType.Int, item.RecordID),
									PXDataFieldRestrict.OperationSwitchAllowed
									);

					PXDatabase.Update<ARSalesPrice>(
									new PXDataFieldAssign("SalesPrice", PXDbType.Decimal, item.SalesPrice),
									new PXDataFieldAssign("LastDate", PXDbType.DateTime, item.LastDate),
									new PXDataFieldAssign("LastModifiedDateTime", PXDbType.DateTime, updateTime),
									new PXDataFieldRestrict("InventoryID", PXDbType.Int, item.InventoryID),
									item.IsCustClassPrice == true
										? new PXDataFieldRestrict("CustPriceClassID", PXDbType.NVarChar, item.CustPriceClassID)
										: new PXDataFieldRestrict("CustomerID", PXDbType.Int, item.CustomerID),
									new PXDataFieldRestrict("CuryID", PXDbType.NVarChar, item.CuryID),
									new PXDataFieldRestrict("UOM", PXDbType.NVarChar, item.UOM),
									new PXDataFieldRestrict("IsPromotionalPrice", PXDbType.Bit, false),
									new PXDataFieldRestrict("BreakQty", PXDbType.Decimal, item.BreakQty),
									new PXDataFieldRestrict("RecordID", PXDbType.Int, 4, item.RecordID, PXComp.NE)
									);

					InventoryItem ii = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, item.InventoryID);
					string uom = PXAccess.FeatureInstalled<FeaturesSet.distributionModule>() && sosetup.Current.SalesPriceUpdateUnit == SalesPriceUpdateUnitType.SalesUnit ? ii.SalesUnit : ii.BaseUnit;
					if (item.UOM == uom && item.CuryID == Company.Current.BaseCuryID && item.CustPriceClassID == AR.ARPriceClass.EmptyPriceClass && item.BreakQty == 0 && item.PendingBreakQty == 0 && item.IsPromotionalPrice == false)
					{
						decimal price = PXAccess.FeatureInstalled<FeaturesSet.distributionModule>() && sosetup.Current.SalesPriceUpdateUnit == SalesPriceUpdateUnitType.SalesUnit ? INUnitAttribute.ConvertFromBase(Inventory.Cache, ii.InventoryID, ii.SalesUnit, item.SalesPrice ?? 0m, INPrecision.UNITCOST) : item.SalesPrice ?? 0m;
						decimal lastPrice = PXAccess.FeatureInstalled<FeaturesSet.distributionModule>() && sosetup.Current.SalesPriceUpdateUnit == SalesPriceUpdateUnitType.SalesUnit ? INUnitAttribute.ConvertFromBase(Inventory.Cache, ii.InventoryID, ii.SalesUnit, item.LastPrice ?? 0m, INPrecision.UNITCOST) : item.LastPrice ?? 0m;
						PXDatabase.Update<InventoryItem>(
										new PXDataFieldAssign("BasePrice", PXDbType.Decimal, price),
										new PXDataFieldAssign("BasePriceDate", PXDbType.DateTime, item.LastDate),
										new PXDataFieldAssign("PendingBasePrice", PXDbType.Decimal, 0m),
										new PXDataFieldAssign("PendingBasePriceDate", PXDbType.DateTime, null),
										new PXDataFieldAssign("LastBasePrice", PXDbType.Decimal, lastPrice),
										new PXDataFieldAssign("LastModifiedDateTime", PXDbType.DateTime, updateTime),
										new PXDataFieldRestrict("InventoryID", PXDbType.Int, item.InventoryID)
										);

					}


					ts.Complete();
				}
			}

		}
	}
}
