using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.TM;

namespace PX.Objects.AR
{
	public class ARCustomerSalesPriceMaint : PXGraph<ARCustomerSalesPriceMaint>, PXImportAttribute.IPXPrepareItems
	{
		#region DAC Overrides
		[PXDBString(10, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Customer Price Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual void ARSalesPrice_CustPriceClassID_CacheAttached(PXCache sender) { }

		[Customer]
		[PXDefault]
		public virtual void ARSalesPrice_CustomerID_CacheAttached(PXCache sender) { }

		[PXDBBool]
		[PXDefault(false)]
		public virtual void ARSalesPrice_IsCustClassPrice_CacheAttached(PXCache sender) { }
		#endregion
		#region Selects/Views

		public PXFilter<ARSalesPriceFilter> Filter;
		public PXFilter<MassCopyFilter> MassCopySettings;
		public PXFilter<MassCalcFilter> MassCalcSettings;
		public PXFilter<MassUpdateFilter> MassUpdateSettings;
		public PXFilter<OperationParam> Operations;

		[PXFilterable]
		[PXImport(typeof(ARSalesPriceFilter))]
		public CustomerSalesPriceProcessing<ARSalesPrice,
			InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<ARSalesPrice.inventoryID>>,
			LeftJoin<INItemCost, On<INItemCost.inventoryID, Equal<InventoryItem.inventoryID>>>>,
			Where<ARSalesPrice.customerID, Equal<Current<ARSalesPriceFilter.customerID>>,
			And2<Where<Current<ARSalesPriceFilter.curyID>, IsNull, Or<ARSalesPrice.curyID, Equal<Current<ARSalesPriceFilter.curyID>>>>,
			And<ARSalesPrice.isPromotionalPrice, Equal<Current<ARSalesPriceFilter.promotionalPrice>>,
			And<InventoryItem.itemStatus, NotEqual<INItemStatus.inactive>,
			And<InventoryItem.itemStatus, NotEqual<INItemStatus.toDelete>,
			And<Where2<Where<Current<ARSalesPriceFilter.itemClassID>, IsNull,
					Or<Current<ARSalesPriceFilter.itemClassID>, Equal<InventoryItem.itemClassID>>>,
				And2<Where<Current<ARSalesPriceFilter.inventoryPriceClassID>, IsNull,
					Or<Current<ARSalesPriceFilter.inventoryPriceClassID>, Equal<InventoryItem.priceClassID>>>,
				And2<Where<Current<ARSalesPriceFilter.ownerID>, IsNull,
					Or<Current<ARSalesPriceFilter.ownerID>, Equal<InventoryItem.priceManagerID>>>,
				And2<Where<Current<ARSalesPriceFilter.myWorkGroup>, Equal<boolFalse>,
						 Or<InventoryItem.priceWorkgroupID, InMember<CurrentValue<ARSalesPriceFilter.currentOwnerID>>>>,
				And<Where<Current<ARSalesPriceFilter.workGroupID>, IsNull,
					Or<Current<ARSalesPriceFilter.workGroupID>, Equal<InventoryItem.priceWorkgroupID>>>>>>>>>>>>>>,
			OrderBy<Asc<ARSalesPrice.inventoryID,
					Asc<ARSalesPrice.uOM,
					Asc<ARSalesPrice.lastDate>>>>> Records;

		public PXSelectJoin<ARSalesPrice,
			InnerJoin<InventoryItem, On<ARSalesPrice.inventoryID, Equal<InventoryItem.inventoryID>>>,
			Where<ARSalesPrice.curyID, Equal<Current<MassCopyFilter.curyID>>,
			And<ARSalesPrice.customerID, Equal<Current<MassCopyFilter.customerID>>>>> Target;

		public CMSetupSelect cmsetup;

		public virtual IEnumerable records([PXString]
			string action)
		{
			if (!string.IsNullOrEmpty(action))
				Operations.Current.Action = action;

			var view = new PXView(this, false, Records.View.BqlSelect);
			var startRow = PXView.StartRow;
			var totalRows = 0;
			var list = view.Select(null, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
							   ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;
			return list;
		}

		public virtual IEnumerable recordsForProcessAll()
		{
			//Optimization is not applicable since any of the stat fields can be used in UpdatePrice mode.

			//PXSelectBase<SOSalesPrice> select = new PXSelectJoin<SOSalesPrice,
			//   InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<ARSalesPrice.inventoryID>>>,
			//   Where<ARSalesPrice.curyID, Equal<Current<SalesPriceFilter.curyID>>,
			//   And<ARSalesPrice.custPriceClassID, Equal<Current<SalesPriceFilter.custPriceClassID>>,
			//   And<InventoryItem.itemStatus, NotEqual<INItemStatus.inactive>,
			//   And<InventoryItem.itemStatus, NotEqual<INItemStatus.toDelete>,
			//   And<Where2<Where<Current<SalesPriceFilter.itemClassID>, IsNull,
			//           Or<Current<SalesPriceFilter.itemClassID>, Equal<InventoryItem.itemClassID>>>,
			//       And<Where<Current<SalesPriceFilter.inventoryPriceClassID>, IsNull,
			//           Or<Current<SalesPriceFilter.inventoryPriceClassID>, Equal<InventoryItem.priceClassID>>>>>>>>>>>(this);
						
			//return select.Select();

			return records(null);
		}

		public PXSetup<Company> Company;
        public PXSetup<ARSetup> arsetup;
		public PXSelect<CurrencyInfo> DummyCuryInfo;

		#endregion



		#region Constants
		protected const string _ITEMS_VIEW_NAME = "Records";
		#endregion

		#region Fields
		private readonly string _curyIDFieldName;
		private readonly string _customerID;
		private readonly string _inventoryID;
		private readonly string _uOM;
		private readonly string _recordID;
		private readonly string _breakQty;
		private readonly string _lastDate;
		private readonly string _expirationDate;
		private readonly string _pendingBreakQty;
		#endregion

		#region Ctors

		public ARCustomerSalesPriceMaint()
		{
			Records.SetProcessDelegate(Process);
			Records.SetProcessListDelegate(recordsForProcessAll);
			Records.SetSelected<ARSalesPrice.selected>();

			_curyIDFieldName = Records.Cache.GetField(typeof(ARSalesPrice.curyID));
			_customerID = Records.Cache.GetField(typeof(ARSalesPrice.customerID));
			_inventoryID = Records.Cache.GetField(typeof(ARSalesPrice.inventoryID));
			_uOM = Records.Cache.GetField(typeof(ARSalesPrice.uOM));
			_recordID = Records.Cache.GetField(typeof(ARSalesPrice.recordID));
			_breakQty = Records.Cache.GetField(typeof(ARSalesPrice.breakQty));
			_lastDate = Records.Cache.GetField(typeof(ARSalesPrice.lastDate));
			_expirationDate = Records.Cache.GetField(typeof(ARSalesPrice.expirationDate));
			_pendingBreakQty = Records.Cache.GetField(typeof(ARSalesPrice.pendingBreakQty));

			FieldDefaulting.AddHandler<CR.BAccountR.type>((sender, e) => { if (e.Row != null) e.NewValue = CR.BAccountType.CustomerType; });

			PXUIFieldAttribute.SetEnabled<ARSalesPrice.curyID>(Records.Cache, null, false);
		}

		#endregion

		#region Buttons/Actions
		public PXSave<ARSalesPriceFilter> Save;
		public PXCancel<ARSalesPriceFilter> Cancel;

		public PXAction<ARSalesPriceFilter> preload;
		[PXUIField(DisplayName = "Preload from Inventory", MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXProcessButton]
		public virtual IEnumerable Preload(PXAdapter adapter)
		{
			if (Filter.Current != null && Filter.Current.CustomerID!=null && !string.IsNullOrEmpty(Filter.Current.CuryID))
			{
				PreloadFromInventory();

			}
			return adapter.Get();
		}


		public PXAction<ARSalesPriceFilter> wCopyNext;
		[PXButton]
		[PXUIField(MapEnableRights = PXCacheRights.Update, Visible = false)]
		public virtual IEnumerable WCopyNext(PXAdapter adapter)
		{
			if (MassCopySettings.Current.PriceOption == PriceOptionList.PriceClass && MassCopySettings.Current.CustPriceClassID == null)
			{
				MassCopySettings.Cache.RaiseExceptionHandling<MassCopyFilter.custPriceClassID>(MassCopySettings.Current, MassCopySettings.Current.CustPriceClassID,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowError));
			}
			if (MassCopySettings.Current.PriceOption == PriceOptionList.Customer && MassCopySettings.Current.CustomerID == null)
			{
				MassCopySettings.Cache.RaiseExceptionHandling<MassCopyFilter.customerID>(MassCopySettings.Current, MassCopySettings.Current.CustomerID,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowError));
			}
			if (Multicurrency == true && MassCopySettings.Current.CuryID == null)
			{
				MassCopySettings.Cache.RaiseExceptionHandling<MassCopyFilter.curyID>(MassCopySettings.Current, MassCopySettings.Current.CuryID,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowError));
			}
			if (Multicurrency == true && MassCopySettings.Current.RateTypeID == null && MassCopySettings.Current.CuryID != Filter.Current.CuryID && MassCopySettings.Current.CustomRate == null)
			{
				MassCopySettings.Cache.RaiseExceptionHandling<MassCopyFilter.rateTypeID>(MassCopySettings.Current, MassCopySettings.Current.RateTypeID,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowError));
			}
			if (MassCopySettings.Current.EffectiveDate == null)
			{
				MassCopySettings.Cache.RaiseExceptionHandling<MassCopyFilter.effectiveDate>(MassCopySettings.Current, MassCopySettings.Current.EffectiveDate,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowError));
			}
			if (MassCopySettings.Current.EffectiveDate > MassCopySettings.Current.ExpirationDate)
			{
				MassCopySettings.Cache.RaiseExceptionHandling<MassCopyFilter.expirationDate>(MassCopySettings.Current, MassCopySettings.Current.ExpirationDate,
					new PXSetPropertyException(Messages.LastDateExpirationDate));
			}
			if (MassCopySettings.Current.CustomRate == null)
				MassCopySettings.Cache.RaiseExceptionHandling<MassCopyFilter.customRate>(MassCopySettings.Current, MassCopySettings.Current.CustomRate,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
			return adapter.Get();
		}

		public PXAction<ARSalesPriceFilter> wCopySave;
		[PXButton]
		[PXUIField(MapEnableRights = PXCacheRights.Update, Visible = false)]
		public virtual IEnumerable WCopySave(PXAdapter adapter)
		{
			if (MassCopySettings.Current.EffectiveDate == null)
			{
				MassCopySettings.Cache.RaiseExceptionHandling<MassCopyFilter.effectiveDate>(MassCopySettings.Current, MassCopySettings.Current.EffectiveDate,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowError));
			}
			if (MassCopySettings.Current.EffectiveDate > MassCopySettings.Current.ExpirationDate)
			{
				MassCopySettings.Cache.RaiseExceptionHandling<MassCopyFilter.expirationDate>(MassCopySettings.Current, MassCopySettings.Current.ExpirationDate,
					new PXSetPropertyException(Messages.LastDateExpirationDate));
			}
			return adapter.Get();
		}

		public PXAction<ARSalesPriceFilter> wCalcNext;
		[PXButton]
		[PXUIField(MapEnableRights = PXCacheRights.Update, Visible = false)]
		public virtual IEnumerable WCalcNext(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<ARSalesPriceFilter> wCalcSave;
		[PXButton]
		[PXUIField(MapEnableRights = PXCacheRights.Update, Visible = false)]
		public virtual IEnumerable WCalcSave(PXAdapter adapter)
		{
			if (MassCalcSettings.Current.EffectiveDate == null)
			{
				MassCalcSettings.Cache.RaiseExceptionHandling<MassCalcFilter.effectiveDate>(MassCalcSettings.Current, MassCalcSettings.Current.EffectiveDate,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowError));
			}
			if (MassCalcSettings.Current.ExpirationDate == null && Filter.Current.PromotionalPrice == true)
			{
				MassCalcSettings.Cache.RaiseExceptionHandling<MassCalcFilter.expirationDate>(MassCalcSettings.Current, MassCalcSettings.Current.ExpirationDate,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.RowError));
			}
			if (MassCalcSettings.Current.EffectiveDate > MassCalcSettings.Current.ExpirationDate)
			{
				MassCalcSettings.Cache.RaiseExceptionHandling<MassCalcFilter.expirationDate>(MassCalcSettings.Current, MassCalcSettings.Current.ExpirationDate,
					new PXSetPropertyException(Messages.LastDateExpirationDate));
			}
			return adapter.Get();
		}

		#endregion

		#region Event Handlers
		public override void Persist()
		{
			foreach (ARSalesPrice price in Records.Cache.Inserted)
			{
				ValidateDuplicate(this, Records.Cache, price);
			}
			foreach (ARSalesPrice price in Records.Cache.Updated)
			{
				ValidateDuplicate(this, Records.Cache, price);
			}
			base.Persist();
			Records.Cache.Clear();
		}

		protected virtual void ARSalesPrice_BaseCuryID_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			e.ReturnValue = Company.Current.BaseCuryID;
		}

		protected virtual void ARSalesPrice_CustomerID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARSalesPrice row = e.Row as ARSalesPrice;
			if (row != null)
			{
				if (Filter.Current != null)
				{
					row.CustomerID = Filter.Current.CustomerID;
				}
			}
		}

		protected virtual void ARSalesPrice_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARSalesPrice row = e.Row as ARSalesPrice;
			if (row != null)
			{
				if (Filter.Current != null)
				{
					row.CuryID = Filter.Current.CuryID;
				}
			}
		}

		protected virtual void ARSalesPrice_SalesPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARSalesPrice row = e.Row as ARSalesPrice;
			if (row != null)
			{
				if (Filter.Current.PromotionalPrice == true)
				{
					InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>>>.Select(this, row.InventoryID);
					if (item != null)
					{
						if (row.CuryID == Company.Current.BaseCuryID)
						{
							if (row.UOM == item.BaseUnit)
							{
								e.NewValue = item.BasePrice;
							}
							else
							{
								e.NewValue = INUnitAttribute.ConvertToBase(sender, item.InventoryID, row.UOM ?? item.SalesUnit, item.BasePrice.Value, INPrecision.UNITCOST);
							}
						}
					}
				}
				else
				{
					e.NewValue = 0m;
				}
			}
		}

		protected virtual void ARSalesPrice_PendingPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARSalesPrice row = e.Row as ARSalesPrice;
			if (row != null)
			{
				if (Filter.Current.PromotionalPrice != true)
				{
					InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>>>.Select(this, row.InventoryID);
					if (item != null)
					{
						if (row.CuryID == Company.Current.BaseCuryID)
						{
							if (row.UOM == item.BaseUnit)
							{
								e.NewValue = item.BasePrice;
							}
							else
							{
								e.NewValue = INUnitAttribute.ConvertToBase(sender, item.InventoryID, item.SalesUnit, item.BasePrice.Value, INPrecision.UNITCOST);
							}
						}
					}
				}
				else
				{
					e.NewValue = 0m;
				}
			}
		}

		protected virtual void ARSalesPrice_EffectiveDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Filter.Current.PromotionalPrice == true)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void ARSalesPrice_LastDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Filter.Current.PromotionalPrice == true)
			{
				e.NewValue = Accessinfo.BusinessDate;
			}
		}
		
		protected virtual void ARSalesPrice_PendingPrice_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARSalesPrice row = e.Row as ARSalesPrice;
			if (row != null && MinGrossProfitValidation != MinGrossProfitValidationType.None && row.EffectiveDate != null)
			{
				var r = (PXResult< InventoryItem,INItemCost>)
				PXSelectJoin<InventoryItem, 
					LeftJoin<INItemCost, On<INItemCost.inventoryID, Equal<InventoryItem.inventoryID>>>,
					Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.SelectWindowed(this,0,1, row.InventoryID);

				InventoryItem item = r;
				INItemCost cost = r;
				if (item != null)
				{
					decimal newValue = (decimal)e.NewValue;
					if (row.UOM != item.BaseUnit)
					{
						try
						{
							newValue = INUnitAttribute.ConvertFromBase(sender, item.InventoryID, row.UOM, newValue, INPrecision.UNITCOST);
						}
						catch (PXUnitConversionException)
						{
							sender.RaiseExceptionHandling<ARSalesPrice.pendingPrice>(row, e.NewValue, new PXSetPropertyException(SO.Messages.FailedToConvertToBaseUnits, PXErrorLevel.Warning));
							return;
						}
					}

					decimal minPrice = PXPriceCostAttribute.MinPrice(item, cost);
					if (row.CuryID != Company.Current.BaseCuryID)
					{
                        ARSetup arsetup = PXSetup<ARSetup>.Select(this);

                        if (string.IsNullOrEmpty(arsetup.DefaultRateTypeID))
						{
							throw new PXException(SO.Messages.DefaultRateNotSetup);
						}

                        minPrice = ConvertAmt(Company.Current.BaseCuryID, row.CuryID, arsetup.DefaultRateTypeID, row.EffectiveDate.Value, minPrice);
					}


					if (newValue < minPrice)
					{
						switch (MinGrossProfitValidation)
						{
							case MinGrossProfitValidationType.Warning:
								sender.RaiseExceptionHandling<ARSalesPrice.pendingPrice>(row, e.NewValue, new PXSetPropertyException(SO.Messages.GrossProfitValidationFailed, PXErrorLevel.Warning));
								break;
							case MinGrossProfitValidationType.SetToMin:
								e.NewValue = minPrice;
								sender.RaiseExceptionHandling<ARSalesPrice.pendingPrice>(row, e.NewValue, new PXSetPropertyException(SO.Messages.GrossProfitValidationFailedAndPriceFixed, PXErrorLevel.Warning));
								break;
							default:
								break;
						}
					}
				}
			}
		}
		
		protected virtual void ARSalesPrice_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARSalesPrice row = e.Row as ARSalesPrice;
			if (Filter.Current.PromotionalPrice == true)
				sender.SetDefaultExt<ARSalesPrice.salesPrice>(e.Row);
			else
				sender.SetDefaultExt<ARSalesPrice.pendingPrice>(e.Row);
			InitCostFields(row);
		}

		protected virtual void ARSalesPrice_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARSalesPrice.uOM>(e.Row);
		}

		protected virtual void ARSalesPrice_PendingPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARSalesPrice row = e.Row as ARSalesPrice;
			if (row != null)
			{
				if (row.EffectiveDate == null && row.IsPromotionalPrice == false && row.PendingPrice > 0m)
					sender.SetDefaultExt<ARSalesPrice.effectiveDate>(e.Row);
			}
		}

		protected virtual void ARSalesPrice_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			ARSalesPrice row = e.Row as ARSalesPrice;
			row.IsPromotionalPrice = Filter.Current.PromotionalPrice;
			
			if (row != null)
			{
				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>>>.Select(this, row.InventoryID);
				e.Cancel = item == null;

				InitCostFields(row);
			}
			
		}

		protected virtual void ARSalesPrice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ARSalesPrice row = (ARSalesPrice)e.Row;
			if (row.IsPromotionalPrice == true && row.LastDate == null)
			{
				sender.RaiseExceptionHandling<ARSalesPrice.lastDate>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPrice.lastDate).Name));
			}
			if (row.IsPromotionalPrice == true && row.ExpirationDate == null)
			{
				sender.RaiseExceptionHandling<ARSalesPrice.expirationDate>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPrice.expirationDate).Name));
			}
			if (row.IsPromotionalPrice == true && row.ExpirationDate < row.LastDate)
			{
				sender.RaiseExceptionHandling<ARSalesPrice.lastDate>(row, row.LastDate, new PXSetPropertyException(Messages.LastDateExpirationDate, PXErrorLevel.RowError));
			}
		}

		public static void ValidateDuplicate(PXGraph graph, PXCache sender, ARSalesPrice price)
		{
			PXSelectBase<ARSalesPrice> selectDuplicate = new PXSelect<ARSalesPrice, Where<ARSalesPrice.curyID, Equal<Required<ARSalesPrice.curyID>>, And<ARSalesPrice.customerID, Equal<Required<ARSalesPrice.customerID>>, And<ARSalesPrice.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>, And<ARSalesPrice.uOM, Equal<Required<ARSalesPrice.uOM>>, And<ARSalesPrice.recordID, NotEqual<Required<ARSalesPrice.recordID>>, And<ARSalesPrice.pendingBreakQty, Equal<Required<ARSalesPrice.pendingBreakQty>>, And<ARSalesPrice.breakQty, Equal<Required<ARSalesPrice.breakQty>>, And<ARSalesPrice.isPromotionalPrice, Equal<Required<ARSalesPrice.isPromotionalPrice>>>>>>>>>>>(graph);
			ARSalesPrice duplicate;
			if (price.IsPromotionalPrice == true)
			{
				selectDuplicate.WhereAnd<Where2<Where<Required<ARSalesPrice.lastDate>, Between<ARSalesPrice.lastDate, ARSalesPrice.expirationDate>>, Or<Required<ARSalesPrice.expirationDate>, Between<ARSalesPrice.lastDate, ARSalesPrice.expirationDate>, Or<ARSalesPrice.lastDate, Between<Required<ARSalesPrice.lastDate>, Required<ARSalesPrice.expirationDate>>, Or<ARSalesPrice.expirationDate, Between<Required<ARSalesPrice.lastDate>, Required<ARSalesPrice.expirationDate>>>>>>>();
				duplicate = selectDuplicate.SelectSingle(price.CuryID, price.CustomerID, price.InventoryID, price.UOM, price.RecordID, price.PendingBreakQty, price.BreakQty, price.IsPromotionalPrice, price.LastDate, price.ExpirationDate, price.LastDate, price.ExpirationDate, price.LastDate, price.ExpirationDate);
			}
			else
			{
				duplicate = selectDuplicate.SelectSingle(price.CuryID, price.CustomerID, price.InventoryID, price.UOM, price.RecordID, price.PendingBreakQty, price.BreakQty, price.IsPromotionalPrice);
			}
			if (duplicate != null)
			{
				sender.RaiseExceptionHandling<ARSalesPrice.uOM>(price, price.UOM, new PXSetPropertyException(SO.Messages.DuplicateSalesPrice, PXErrorLevel.RowError));
			}
		}


		private void InitCostFields(ARSalesPrice row)
		{
			if (row != null)
			{
				var r = (PXResult<InventoryItem, INItemCost>)
					PXSelectJoin<InventoryItem, 					
						LeftJoin<INItemCost, On<INItemCost.inventoryID, Equal<InventoryItem.inventoryID>>>,
					Where<InventoryItem.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>>>
					.Select(this, row.InventoryID);
				InventoryItem item = r;
				INItemCost cost = r;
				if (item != null)
				{
					row.BaseCuryID = Company.Current.BaseCuryID;

					decimal? result;
					if (TryConvertToBase(Caches[typeof(InventoryItem)], item.InventoryID, row.UOM, PXPriceCostAttribute.MinPrice(item, cost), out result))
					{
						row.MinGPPrice = result;
					}
					if (TryConvertToBase(Caches[typeof(InventoryItem)], item.InventoryID, row.UOM, item.RecPrice.Value, out result))
					{
						row.RecPrice = result;
					}

					if (cost.LastCost != null && cost.LastCost > 0)
					{
						if (TryConvertToBase(Caches[typeof(InventoryItem)], item.InventoryID, row.UOM, cost.LastCost.Value, out result))
						{
							row.LastCost = result;
						}

						if (TryConvertToBase(Caches[typeof(InventoryItem)], item.InventoryID, row.UOM, cost.AvgCost.Value, out result))
						{
							row.AvgCost = result;
						}
					}
				}
			}
		}



		protected virtual void ARSalesPriceFilter_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARSalesPriceFilter row = e.Row as ARSalesPriceFilter;
			if (row != null)
			{
				Customer cust = (Customer)PXParentAttribute.SelectParent(sender, row, typeof(Customer));
				if (cust != null)
				{
					if (arsetup.Current.AlwaysFromBaseCury == false)
					{
						row.CuryID = cust.CuryID;
					}
				}
			}
		}

		protected virtual void ARSalesPriceFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARSalesPriceFilter row = e.Row as ARSalesPriceFilter;

			Records.Cache.AllowInsert = row != null && row.CuryID != null && row.CustomerID != null;

			Customer cust = (Customer)PXParentAttribute.SelectParent(sender, row, typeof(Customer));
			if (cust != null)
			{
				PXUIFieldAttribute.SetEnabled<ARSalesPriceFilter.curyID>(sender, row, AlwaysFromBaseCurrency == false && (cust.CuryID == null || cust.AllowOverrideCury == true));
			}

			#region Set State Grid Columns
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.taxID>(Records.Cache, null, row.PromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.lastDate>(Records.Cache, null, row.PromotionalPrice == true);
			PXUIFieldAttribute.SetVisible<ARSalesPrice.expirationDate>(Records.Cache, null, row.PromotionalPrice == true);
			PXUIFieldAttribute.SetVisible<ARSalesPrice.effectiveDate>(Records.Cache, null, row.PromotionalPrice == false);
			PXUIFieldAttribute.SetVisible<ARSalesPrice.pendingBreakQty>(Records.Cache, null, row.PromotionalPrice == false);
			PXUIFieldAttribute.SetVisible<ARSalesPrice.pendingPrice>(Records.Cache, null, row.PromotionalPrice == false);
			PXUIFieldAttribute.SetVisible<ARSalesPrice.pendingTaxID>(Records.Cache, null, row.PromotionalPrice == false);
			PXUIFieldAttribute.SetVisible<ARSalesPrice.lastBreakQty>(Records.Cache, null, row.PromotionalPrice == false);
			PXUIFieldAttribute.SetVisible<ARSalesPrice.lastPrice>(Records.Cache, null, row.PromotionalPrice == false);
			PXUIFieldAttribute.SetVisible<ARSalesPrice.lastTaxID>(Records.Cache, null, row.PromotionalPrice == false);

			PXUIFieldAttribute.SetDisplayName<ARSalesPrice.breakQty>(Records.Cache, row.PromotionalPrice == false ? "Current Break Qty" : "Break Qty");
			PXUIFieldAttribute.SetDisplayName<ARSalesPrice.salesPrice>(Records.Cache, row.PromotionalPrice == false ? "Current Price" : "Promotional Price");

			PXUIFieldAttribute.SetEnabled<ARSalesPrice.salesPrice>(Records.Cache, null, row.PromotionalPrice == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.breakQty>(Records.Cache, null, row.PromotionalPrice == true);
			#endregion

			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(OwnedFilter.ownerID).Name, e.Row == null || (bool?)sender.GetValue(e.Row, typeof(OwnedFilter.myOwner).Name) == false);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(OwnedFilter.workGroupID).Name, e.Row == null || (bool?)sender.GetValue(e.Row, typeof(OwnedFilter.myWorkGroup).Name) == false);

			preload.SetEnabled(Filter.Current.PromotionalPrice == false && Filter.Current.CustomerID != null && string.Equals(Filter.Current.CuryID, Company.Current.BaseCuryID));
		}

		protected virtual void ARSalesPriceFilter_CuryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARSalesPriceFilter row = e.Row as ARSalesPriceFilter;
			if (row.CuryID == null)
				row.CuryID = (string)e.OldValue;
		}

		protected virtual void MassCopyFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			MassCopyFilter row = e.Row as MassCopyFilter;

			bool multicurrency = arsetup.Current.AlwaysFromBaseCury != true && cmsetup.Current.MCActivated == true;

			PXUIFieldAttribute.SetVisible<MassCopyFilter.labelCury>(sender, null, multicurrency);
			PXUIFieldAttribute.SetVisible<MassCopyFilter.curyID>(sender, null, multicurrency);
			PXUIFieldAttribute.SetVisible<MassCopyFilter.rateTypeID>(sender, null, multicurrency);
			PXUIFieldAttribute.SetVisible<MassCopyFilter.currencyDate>(sender, null, multicurrency);


			PXUIFieldAttribute.SetEnabled<MassCopyFilter.custPriceClassID>(sender, row, row.PriceOption == PriceOptionList.PriceClass);
			PXUIFieldAttribute.SetEnabled<MassCopyFilter.customerID>(sender, row, row.PriceOption == PriceOptionList.Customer);
			PXUIFieldAttribute.SetRequired<MassCopyFilter.custPriceClassID>(sender, row.PriceOption == PriceOptionList.PriceClass);
			PXUIFieldAttribute.SetRequired<MassCopyFilter.customerID>(sender, row.PriceOption == PriceOptionList.Customer);

			if (row.PriceOption == PriceOptionList.PriceClass)
			{
				PXUIFieldAttribute.SetEnabled<MassCopyFilter.curyID>(sender, null, arsetup.Current.AlwaysFromBaseCury == false);
			}
			else
			{
				Customer cust = (Customer)PXParentAttribute.SelectParent(sender, row, typeof(Customer));
				if (cust != null)
				{
					PXUIFieldAttribute.SetEnabled<MassCopyFilter.curyID>(sender, null, cust.CuryID == null || cust.AllowOverrideCury == true);
				}
			}
			PXUIFieldAttribute.SetEnabled<MassCopyFilter.rateTypeID>(sender, row, row.CuryID != Filter.Current.CuryID);
			PXUIFieldAttribute.SetRequired<MassCopyFilter.rateTypeID>(sender, row.CuryID != Filter.Current.CuryID);
			PXUIFieldAttribute.SetEnabled<MassCopyFilter.customRate>(sender, row, row.CuryID != Filter.Current.CuryID);
		}

		protected virtual void MassCopyFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			MassCopyFilter row = e.Row as MassCopyFilter;
			if (row != null && Filter.Current != null)
			{
				if (row.CuryID == Filter.Current.CuryID)
				{
					row.RateTypeID = null;
				}
			}

		}

		protected virtual void MassCopyFilter_PriceOption_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			MassCopyFilter filter = (MassCopyFilter)e.Row;
			if (filter != null)
			{
				if (filter.PriceOption == PriceOptionList.PriceClass)
				{
					filter.CustomerID = null;
					sender.SetValueExt<MassCopyFilter.curyID>(filter, Company.Current.BaseCuryID);
				}
				else
				{
					sender.SetDefaultExt<MassCopyFilter.custPriceClassID>(filter);
				}
			}
		}

		protected virtual void MassCopyFilter_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			MassCopyFilter row = (MassCopyFilter)e.Row;
			if (row != null)
			{
				Customer cust = (Customer)PXParentAttribute.SelectParent(sender, row, typeof(Customer));
				if (cust != null)
				{
					if (cust.CuryID != null)
						sender.SetValueExt<MassCopyFilter.curyID>(row, cust.CuryID);
					else
						sender.SetValueExt<MassCopyFilter.curyID>(row, Company.Current.BaseCuryID);
				}
			}
		}

		protected virtual void MassCopyFilter_CuryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			MassCopyFilter row = (MassCopyFilter)e.Row;
			if (row != null)
			{
				if (row.CuryID != Filter.Current.CuryID)
					sender.SetDefaultExt<MassCopyFilter.rateTypeID>(row);
				else
				{
					row.RateTypeID = null;
					row.CustomRate = 1;
				}
			}
		}

		protected virtual void MassCopyFilter_RateTypeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			MassCopyFilter row = (MassCopyFilter)e.Row;
			if (row != null)
			{
				CurrencyRate rate = getCuryRate(Filter.Current.CuryID, row.CuryID, row.RateTypeID, row.CurrencyDate.Value);
				if (rate == null)
				{
					row.CustomRate = 1;
				}
				else
				{
					row.CustomRate = rate.CuryRate;
				}
			}
		}

		protected virtual void MassCopyFilter_CurrencyDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			MassCopyFilter row = (MassCopyFilter)e.Row;
			if (row != null)
			{
				CurrencyRate rate = getCuryRate(Filter.Current.CuryID, row.CuryID, row.RateTypeID, row.CurrencyDate.Value);
				if (rate == null)
				{
					row.CustomRate = 1;
				}
				else
				{
					row.CustomRate = rate.CuryRate;
				}
			}
		}

		protected virtual void MassCalcFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetVisible<MassCalcFilter.expirationDate>(sender, null, Filter.Current.PromotionalPrice == true);
		}
		
		#endregion

		private string MinGrossProfitValidation
		{
			get
			{
				SOSetup sosetup = PXSelect<SOSetup>.Select(this);
				if (sosetup != null)
				{
					if (string.IsNullOrEmpty(sosetup.MinGrossProfitValidation))
						return MinGrossProfitValidationType.Warning;
					else
						return sosetup.MinGrossProfitValidation;
				}
				else
					return MinGrossProfitValidationType.Warning;

			}
		}

		private bool Multicurrency
		{
			get
			{
				return arsetup.Current.AlwaysFromBaseCury != true && cmsetup.Current.MCActivated == true;
			}
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

		private void PreloadFromInventory()
		{
			PXSelectBase<InventoryItem> select = new
			PXSelectJoin<InventoryItem,
				LeftJoin<ARSalesPrice, On<ARSalesPrice.inventoryID, Equal<InventoryItem.inventoryID>,
				And<ARSalesPrice.customerID, Equal<Current<ARSalesPriceFilter.customerID>>>>>,
			Where<ARSalesPrice.inventoryID, IsNull,
					And<Where<InventoryItem.itemStatus, Equal<INItemStatus.active>, Or<InventoryItem.itemStatus, Equal<INItemStatus.noPurchases>>>>>>(this);

			int errorsCount = 0;

			foreach (InventoryItem item in select.Select())
			{
				ARSalesPrice sp = new ARSalesPrice();
				sp.CustomerID = Filter.Current.CustomerID;
				sp.InventoryID = item.InventoryID;
				sp.CuryID = Company.Current.BaseCuryID;
				sp.UOM = item.SalesUnit;

				try
				{
					sp = Records.Insert(sp);

					if (sp != null)
					{
						try
						{
							sp.EffectiveDate = item.PendingBasePriceDate;
							sp.LastDate = item.BasePriceDate;
							sp.SalesPrice = INUnitAttribute.ConvertToBase(Caches[typeof(InventoryItem)], item.InventoryID, item.SalesUnit, item.BasePrice ?? 0, INPrecision.UNITCOST);
							sp.PendingPrice = INUnitAttribute.ConvertToBase(Caches[typeof(InventoryItem)], item.InventoryID, item.SalesUnit, item.PendingBasePrice ?? 0, INPrecision.UNITCOST);

							Records.Update(sp);
						}
						catch (PXException)
						{
							Debug.Print(item.InventoryCD);
						}
					}

				}
				catch (PXException)
				{
					errorsCount++;
				}


			}

			if (errorsCount > 0)
			{
				throw new PXException(SO.Messages.FailToInsertSalesPrice, errorsCount);
			}
		}

		private void DoMassCopy(MassCopyFilter settings, List<ARSalesPrice> list)
		{
			if (settings != null && !string.IsNullOrEmpty(settings.CuryID))
			{
				if (settings.PriceOption == PriceOptionList.Customer)
				{
					foreach (ARSalesPrice sp in list)
					{
						decimal amt;
						decimal breakQty;
						string taxID;
						if (settings.UsePendingPrice == true && sp.PendingPrice != null && sp.PendingPrice > 0)
						{
							breakQty = sp.PendingBreakQty ?? 0m;
							taxID = sp.PendingTaxID;
							if (arsetup.Current.AlwaysFromBaseCury == false && !string.Equals(Filter.Current.CuryID, settings.CuryID))
							{
								amt = ConvertAmt(Filter.Current.CuryID, settings.CuryID, settings.RateTypeID, settings.CurrencyDate.Value, sp.PendingPrice ?? 0, settings.CustomRate);
							}
							else
							{
								amt = sp.PendingPrice ?? 0;
								settings.CuryID = Filter.Current.CuryID;
							}
						}
						else
						{
							breakQty = sp.BreakQty ?? 0m;
							taxID = sp.TaxID;
							if (arsetup.Current.AlwaysFromBaseCury == false && !string.Equals(Filter.Current.CuryID, settings.CuryID))
							{
								amt = ConvertAmt(Filter.Current.CuryID, settings.CuryID, settings.RateTypeID, settings.CurrencyDate.Value, sp.SalesPrice ?? 0, settings.CustomRate);
							}
							else
							{
								amt = sp.SalesPrice ?? 0;
								settings.CuryID = Filter.Current.CuryID;
							}
						}

						decimal correctedAmt = amt;
						if (settings.CorrectionPercent > 0)
						{
							correctedAmt = amt * settings.CorrectionPercent.Value / 100;
						}

						if (settings.Rounding != null)
						{
							correctedAmt = Math.Round(correctedAmt, settings.Rounding.Value, MidpointRounding.AwayFromZero);
						}

						PXSelectBase<ARSalesPrice> customerPriceExisting = new PXSelect<ARSalesPrice, Where<ARSalesPrice.curyID, Equal<Required<MassCopyFilter.curyID>>,
							And<ARSalesPrice.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>,
							And<ARSalesPrice.isPromotionalPrice, Equal<Required<ARSalesPrice.isPromotionalPrice>>,
							And<ARSalesPrice.uOM, Equal<Required<ARSalesPrice.uOM>>,
							And<ARSalesPrice.customerID, Equal<Required<MassCopyFilter.customerID>>>>>>>>(this);

						bool isPromotion = settings.ExpirationDate != null;
						ARSalesPrice existing;

						if (isPromotion == true)
						{
							customerPriceExisting.WhereAnd<Where<ARSalesPrice.breakQty, Equal<Required<ARSalesPrice.breakQty>>, And<Where2<Where<Required<MassCopyFilter.effectiveDate>, Between<ARSalesPrice.lastDate, ARSalesPrice.expirationDate>>, Or<Required<MassCopyFilter.expirationDate>, Between<ARSalesPrice.lastDate, ARSalesPrice.expirationDate>, Or<ARSalesPrice.lastDate, Between<Required<MassCopyFilter.effectiveDate>, Required<MassCopyFilter.expirationDate>>, Or<ARSalesPrice.expirationDate, Between<Required<MassCopyFilter.effectiveDate>, Required<MassCopyFilter.expirationDate>>>>>>>>>();
							existing = customerPriceExisting.Select(settings.CuryID, sp.InventoryID, isPromotion, sp.UOM, settings.CustomerID, breakQty, settings.EffectiveDate, settings.ExpirationDate, settings.EffectiveDate, settings.ExpirationDate, settings.EffectiveDate, settings.ExpirationDate);
						}
						else
						{
							customerPriceExisting.WhereAnd<Where<ARSalesPrice.pendingBreakQty, Equal<Required<ARSalesPrice.pendingBreakQty>>, And<ARSalesPrice.breakQty, Equal<decimal0>>>>();
							existing = customerPriceExisting.Select(settings.CuryID, sp.InventoryID, isPromotion, sp.UOM, settings.CustomerID, breakQty);
						}

						if (existing != null)
						{
							if (settings.OverrideExisting == true)
							{
								if (isPromotion == true)
								{
									existing.SalesPrice = correctedAmt;
									existing.TaxID = taxID;
									existing.LastDate = settings.EffectiveDate;
									existing.ExpirationDate = settings.ExpirationDate;
									Records.Update(existing);
								}
								else
								{
									existing.EffectiveDate = settings.EffectiveDate;
									existing.PendingPrice = correctedAmt;
									existing.PendingBreakQty = breakQty;
									existing.PendingTaxID = taxID;
									Records.Update(existing);
								}
							}
						}
						else
						{
							if (isPromotion == true)
							{
								ARSalesPrice newPrice = new ARSalesPrice();
								newPrice.InventoryID = sp.InventoryID;
								newPrice.CustomerID = settings.CustomerID;
								newPrice.CuryID = settings.CuryID;
								newPrice.UOM = sp.UOM;
								newPrice = Target.Insert(newPrice);

								newPrice.IsPromotionalPrice = true;
								newPrice.EffectiveDate = null;
								newPrice.PendingPrice = 0;
								newPrice.BreakQty = breakQty;
								newPrice.SalesPrice = correctedAmt;
								newPrice.TaxID = taxID;
								newPrice.LastDate = settings.EffectiveDate;
								newPrice.ExpirationDate = settings.ExpirationDate;
								Target.Update(newPrice);
							}
							else
							{
								ARSalesPrice newPrice = new ARSalesPrice();
								newPrice.InventoryID = sp.InventoryID;
								newPrice.CustomerID = settings.CustomerID;
								newPrice.CuryID = settings.CuryID;
								newPrice.UOM = sp.UOM;
								newPrice = Target.Insert(newPrice);

								newPrice.IsPromotionalPrice = false;
								newPrice.EffectiveDate = settings.EffectiveDate;
								newPrice.PendingPrice = correctedAmt;

								newPrice.SalesPrice = 0;
								newPrice.LastDate = null;
								newPrice.PendingBreakQty = breakQty;
								newPrice.PendingTaxID = taxID;
								Target.Update(newPrice);
							}
						}
					}
					this.Save.Press();
				}
				else
				{

					ARSalesPriceMaint graph = PXGraph.CreateInstance<ARSalesPriceMaint>();
					foreach (ARSalesPrice sp in list)
					{
						decimal amt;
						decimal breakQty;
						string taxID;
						if (settings.UsePendingPrice == true && sp.PendingPrice != null && sp.PendingPrice > 0)
						{
							breakQty = sp.PendingBreakQty ?? 0m;
							taxID = sp.PendingTaxID;
							if (arsetup.Current.AlwaysFromBaseCury == false && !string.Equals(Filter.Current.CuryID, settings.CuryID))
							{
								amt = ConvertAmt(Filter.Current.CuryID, settings.CuryID, settings.RateTypeID, settings.CurrencyDate.Value, sp.PendingPrice ?? 0, settings.CustomRate);
							}
							else
							{
								amt = sp.PendingPrice ?? 0;
								settings.CuryID = Filter.Current.CuryID;
							}
						}
						else
						{
							breakQty = sp.BreakQty ?? 0m;
							taxID = sp.TaxID;
							if (arsetup.Current.AlwaysFromBaseCury == false && !string.Equals(Filter.Current.CuryID, settings.CuryID))
							{
								amt = ConvertAmt(Filter.Current.CuryID, settings.CuryID, settings.RateTypeID, settings.CurrencyDate.Value, sp.SalesPrice ?? 0, settings.CustomRate);
							}
							else
							{
								amt = sp.SalesPrice ?? 0;
								settings.CuryID = Filter.Current.CuryID;
							}
						}

						decimal correctedAmt = amt;
						if (settings.CorrectionPercent > 0)
						{
							correctedAmt = amt * settings.CorrectionPercent.Value / 100;
						}

						if (settings.Rounding != null)
						{
							correctedAmt = Math.Round(correctedAmt, settings.Rounding.Value, MidpointRounding.AwayFromZero);
						}

						PXSelectBase<ARSalesPrice> custClassPriceExisting = new PXSelect<ARSalesPrice, Where<ARSalesPrice.curyID, Equal<Required<MassCopyFilter.curyID>>,
							And<ARSalesPrice.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>,
							And<ARSalesPrice.isPromotionalPrice, Equal<Required<ARSalesPrice.isPromotionalPrice>>,
							And<ARSalesPrice.uOM, Equal<Required<ARSalesPrice.uOM>>,
							And<ARSalesPrice.custPriceClassID, Equal<Required<MassCopyFilter.custPriceClassID>>>>>>>>(graph);

						bool isPromotion = settings.ExpirationDate != null;
						ARSalesPrice existing;
						if (isPromotion == true)
						{
							custClassPriceExisting.WhereAnd<Where<ARSalesPrice.breakQty, Equal<Required<ARSalesPrice.breakQty>>, And<Where2<Where<Required<MassCopyFilter.effectiveDate>, Between<ARSalesPrice.lastDate, ARSalesPrice.expirationDate>>, Or<Required<MassCopyFilter.expirationDate>, Between<ARSalesPrice.lastDate, ARSalesPrice.expirationDate>, Or<ARSalesPrice.lastDate, Between<Required<MassCopyFilter.effectiveDate>, Required<MassCopyFilter.expirationDate>>, Or<ARSalesPrice.expirationDate, Between<Required<MassCopyFilter.effectiveDate>, Required<MassCopyFilter.expirationDate>>>>>>>>>();
							existing = custClassPriceExisting.Select(settings.CuryID, sp.InventoryID, isPromotion, sp.UOM, settings.CustPriceClassID, breakQty, settings.EffectiveDate, settings.ExpirationDate, settings.EffectiveDate, settings.ExpirationDate, settings.EffectiveDate, settings.ExpirationDate);
						}
						else
						{
							custClassPriceExisting.WhereAnd<Where<ARSalesPrice.pendingBreakQty, Equal<Required<ARSalesPrice.pendingBreakQty>>, And<ARSalesPrice.breakQty, Equal<decimal0>>>>();
							existing = custClassPriceExisting.Select(settings.CuryID, sp.InventoryID, isPromotion, sp.UOM, settings.CustPriceClassID, breakQty);
						}

						if (existing != null)
						{
							if (settings.OverrideExisting == true)
							{
								if (isPromotion == true)
								{
									existing.SalesPrice = correctedAmt;
									existing.TaxID = taxID;
									existing.LastDate = settings.EffectiveDate;
									existing.ExpirationDate = settings.ExpirationDate;
									graph.Records.Update(existing);
								}
								else
								{
									existing.EffectiveDate = settings.EffectiveDate;
									existing.PendingPrice = correctedAmt;
									existing.PendingBreakQty = breakQty;
									existing.PendingTaxID = taxID;
									graph.Records.Update(existing);
								}
							}
						}
						else
						{
							if (isPromotion == true)
							{
								ARSalesPrice newPrice = new ARSalesPrice();
								newPrice.InventoryID = sp.InventoryID;
								newPrice.CustPriceClassID = settings.CustPriceClassID;
								newPrice.CuryID = settings.CuryID;
								newPrice.UOM = sp.UOM;
								newPrice = graph.Target.Insert(newPrice);

								newPrice.IsPromotionalPrice = true;
								newPrice.EffectiveDate = null;
								newPrice.BreakQty = breakQty;
								newPrice.SalesPrice = correctedAmt;
								newPrice.TaxID = taxID;
								newPrice.LastDate = settings.EffectiveDate;
								newPrice.ExpirationDate = settings.ExpirationDate;
								graph.Target.Update(newPrice);
							}
							else
							{
								ARSalesPrice newPrice = new ARSalesPrice();
								newPrice.InventoryID = sp.InventoryID;
								newPrice.CustPriceClassID = settings.CustPriceClassID;
								newPrice.CuryID = settings.CuryID;
								newPrice.UOM = sp.UOM;
								newPrice = graph.Target.Insert(newPrice);

								newPrice.IsPromotionalPrice = false;
								newPrice.EffectiveDate = settings.EffectiveDate;
								newPrice.PendingPrice = correctedAmt;

								newPrice.SalesPrice = 0;
								newPrice.LastDate = null;

								newPrice.PendingBreakQty = breakQty;
								newPrice.PendingTaxID = taxID;
								graph.Target.Update(newPrice);
							}
						}
					}
					graph.Save.Press();
				}
			}
		}

		private void DoMassUpdate(MassCalcFilter settings, IList<ARSalesPrice> list)
		{
			if (settings != null)
			{
				foreach (ARSalesPrice sp in list)
				{
					if (sp.Selected == true)
					{
						bool skipUpdate = false;						
						decimal correctedAmt = 0;
						decimal correctedAmtInBaseUnit = 0;
						decimal? result;
						decimal? breakQty = sp.IsPromotionalPrice == true ? sp.BreakQty : sp.PendingBreakQty;
						var r = (PXResult<InventoryItem, INItemCost>)
										PXSelectJoin<InventoryItem,
											LeftJoin<INItemCost, On<INItemCost.inventoryID, Equal<InventoryItem.inventoryID>>>,
											Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
											.SelectWindowed(this, 0, 1, sp.InventoryID);
						InventoryItem ii = r;
						INItemCost ic = r;
						switch (settings.PriceBasis)
						{
							case PriceBasisType.LastCost:
								skipUpdate = settings.UpdateOnZero != true && (ic.LastCost == null || ic.LastCost == 0);
								if (!skipUpdate)
								{									
									correctedAmtInBaseUnit = (ic.LastCost ?? 0) + ((ii.MarkupPct ?? 0) * 0.01m * (ic.LastCost ?? 0));

									if (ii.BaseUnit != sp.UOM)
									{
										if (TryConvertToBase(Caches[typeof(InventoryItem)], ii.InventoryID, sp.UOM, correctedAmtInBaseUnit, out result))
										{
											correctedAmt = result.Value;
										}
									}
									else
									{
										correctedAmt = correctedAmtInBaseUnit;
									}
								}

								break;
							case PriceBasisType.StdCost:
								if (ii.ValMethod != INValMethod.Standard)
								{
									skipUpdate = settings.UpdateOnZero != true && (ic.AvgCost == null || ic.AvgCost == 0);
									correctedAmtInBaseUnit = (ic.AvgCost ?? 0) + ((ii.MarkupPct ?? 0) * 0.01m * (ic.AvgCost ?? 0));
								}
								else
								{
									skipUpdate = settings.UpdateOnZero != true && (ii.StdCost == null || ii.StdCost == 0);
									correctedAmtInBaseUnit = (ii.StdCost ?? 0) + ((ii.MarkupPct ?? 0) * 0.01m * (ii.StdCost ?? 0));
								}

								if (ii.BaseUnit != sp.UOM)
								{
									if (TryConvertToBase(Caches[typeof(InventoryItem)], ii.InventoryID, sp.UOM, correctedAmtInBaseUnit, out result))
									{
										correctedAmt = result.Value;
									}
								}
								else
								{
									correctedAmt = correctedAmtInBaseUnit;
								}

								break;
							case PriceBasisType.PendingPrice:
								skipUpdate = settings.UpdateOnZero != true && (sp.PendingPrice == null || sp.PendingPrice == 0);
								correctedAmt = sp.PendingPrice ?? 0m;
								break;
							case PriceBasisType.CurrentPrice:
								skipUpdate = settings.UpdateOnZero != true && (sp.SalesPrice == null || sp.SalesPrice == 0);
								correctedAmt = sp.SalesPrice ?? 0;
								breakQty = sp.BreakQty;
								break;
							case PriceBasisType.RecommendedPrice:
								skipUpdate = settings.UpdateOnZero != true && (ii.RecPrice == null || ii.RecPrice == 0);
								correctedAmt = ii.RecPrice ?? 0;
								break;
						}

						if (!skipUpdate)
						{
							if (settings.CorrectionPercent != null)
							{
								correctedAmt = correctedAmt * (settings.CorrectionPercent.Value * 0.01m);
							}

							if (settings.Rounding != null)
							{
								correctedAmt = Math.Round(correctedAmt, settings.Rounding.Value, MidpointRounding.AwayFromZero);
							}

							ARSalesPrice u = (ARSalesPrice)Records.Cache.CreateCopy(sp);
							u.EffectiveDate = settings.EffectiveDate;
							if (sp.IsPromotionalPrice == true)
							{
								u.BreakQty = breakQty;
								u.SalesPrice = correctedAmt;
							}
							else
							{
								u.PendingBreakQty = breakQty;
								u.PendingPrice = correctedAmt;
							}
							Records.Update(u);
						}
					}
				}
			}
		}

		private decimal ConvertAmt(string from, string to, string rateType, DateTime effectiveDate, decimal amount, decimal? customRate = 1)
		{
			if (from == to)
				return amount;

			CurrencyRate rate = getCuryRate(from, to, rateType, effectiveDate);

			if (rate == null)
			{
				return amount * customRate ?? 1;
			}
			else
			{
				return rate.CuryMultDiv == "M" ? amount * rate.CuryRate ?? 1 : amount / rate.CuryRate ?? 1;
			}
		}

		private CurrencyRate getCuryRate(string from, string to, string curyRateType, DateTime curyEffDate)
		{
			return PXSelectReadonly<CurrencyRate,
							Where<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>,
							And<CurrencyRate.fromCuryID, Equal<Required<CurrencyRate.fromCuryID>>,
							And<CurrencyRate.curyRateType, Equal<Required<CurrencyRate.curyRateType>>,
							And<CurrencyRate.curyEffDate, LessEqual<Required<CurrencyRate.curyEffDate>>>>>>,
							OrderBy<Desc<CurrencyRate.curyEffDate>>>.SelectWindowed(this, 0, 1, to, from, curyRateType, curyEffDate);
		}

		private bool TryConvertToBase(PXCache cache, int? inventoryID, string uom, decimal value, out decimal? result)
		{
			result = null;

			try
			{
				result = INUnitAttribute.ConvertToBase(cache, inventoryID, uom, value, INPrecision.UNITCOST);
				return true;
			}
			catch (PXUnitConversionException)
			{
				return false;
			}
		}

		#region PXImportAttribute.IPXPrepareItems
		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (string.Compare(viewName, _ITEMS_VIEW_NAME, true) == 0)
			{
				if (values[_curyIDFieldName] == null)
					values[_curyIDFieldName] = Filter.Current.CuryID;

				if (values[_customerID] == null)
					values[_customerID] = Filter.Current.CustomerID;

				if (values[_breakQty] == null)
					values[_breakQty] = "0.00";

				if (values[_pendingBreakQty] == null)
					values[_pendingBreakQty] = "0.00";

				string inventoryCD = (string)values[_inventoryID];
				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>>>.Select(this, inventoryCD);
				if (item != null)
				{
					string uom = (string)values[_uOM];

					PXSelectBase<ARSalesPrice> selectOld_row = new PXSelect<ARSalesPrice,
						Where<ARSalesPrice.curyID, Equal<Required<ARSalesPrice.curyID>>,
						And<ARSalesPrice.customerID, Equal<Required<ARSalesPrice.customerID>>,
						And<ARSalesPrice.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>,
						And<ARSalesPrice.uOM, Equal<Required<ARSalesPrice.uOM>>,
						And<ARSalesPrice.isPromotionalPrice, Equal<Required<ARSalesPrice.isPromotionalPrice>>>>>>>>(this);

					ARSalesPrice old_row;
					if (Filter.Current.PromotionalPrice == true)
					{
						selectOld_row.WhereAnd<Where<ARSalesPrice.breakQty, Equal<Required<ARSalesPrice.breakQty>>, And<ARSalesPrice.lastDate, Equal<Required<ARSalesPrice.lastDate>>, And<ARSalesPrice.expirationDate, Equal<Required<ARSalesPrice.expirationDate>>>>>>();
						old_row = selectOld_row.SelectWindowed(0, 1, Filter.Current.CuryID, Filter.Current.CustomerID, item.InventoryID, uom ?? item.SalesUnit, Filter.Current.PromotionalPrice, values[_breakQty], values[_lastDate], values[_expirationDate]);
					}
					else
					{
						selectOld_row.WhereAnd<Where<ARSalesPrice.pendingBreakQty, Equal<Required<ARSalesPrice.pendingBreakQty>>>>();
						old_row = selectOld_row.SelectWindowed(0, 1, Filter.Current.CuryID, Filter.Current.CustomerID, item.InventoryID, uom ?? item.SalesUnit, Filter.Current.PromotionalPrice, values[_pendingBreakQty]);
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

		protected virtual void Process(List<ARSalesPrice> list)
		{
			switch (Operations.Current.Action)
			{
				case OperationParam.Copy:
					DoMassCopy(MassCopySettings.Current, list);
					break;
				case OperationParam.Calc:
					DoMassUpdate(MassCalcSettings.Current, list);
					break;
				case OperationParam.Update:
					this.Save.Press();
					ARUpdateSalesPriceProcess graph = PXGraph.CreateInstance<ARUpdateSalesPriceProcess>();
					foreach (ARSalesPrice sp in list)
					{
						if (sp.EffectiveDate != null && sp.EffectiveDate <= MassUpdateSettings.Current.EffectiveDate)
							graph.UpdateSalesPrice(sp);
					}
					SelectTimeStamp();
					break;
			}
		}
	}


	public class CustomerSalesPriceProcessing<Table, Join, Where, OrderBy> : PXProcessingJoin<Table, Join, Where, OrderBy>
		where Table : class, IBqlTable, new()
		where Join : IBqlJoin, new()
		where Where : IBqlWhere, new()
		where OrderBy: IBqlOrderBy, new()
	{
		protected PXSelectDelegate _listHandler;

		public CustomerSalesPriceProcessing() : base() { }
		public CustomerSalesPriceProcessing(PXGraph graph) : base(graph, null) { }
		public CustomerSalesPriceProcessing(PXGraph graph, Delegate handler) : base(graph, handler) { }

		protected override void _PrepareGraph<Primary>()
		{
			base._PrepareGraph<ARSalesPriceFilter>();

			_ScheduleButton.SetVisible(false);
			_OuterView.Cache.AllowDelete = true;
			_OuterView.Cache.AllowInsert = true;

			PXUIFieldAttribute.SetEnabled(_OuterView.Cache, typeof(ARSalesPrice.inventoryID).Name, true);
			PXUIFieldAttribute.SetEnabled(_OuterView.Cache, typeof(ARSalesPrice.curyID).Name, true);
			PXUIFieldAttribute.SetEnabled(_OuterView.Cache, typeof(ARSalesPrice.uOM).Name, true);
			PXUIFieldAttribute.SetEnabled(_OuterView.Cache, typeof(ARSalesPrice.effectiveDate).Name, true);
			PXUIFieldAttribute.SetEnabled(_OuterView.Cache, typeof(ARSalesPrice.pendingPrice).Name, true);
			PXUIFieldAttribute.SetEnabled(_OuterView.Cache, typeof(ARSalesPrice.pendingTaxID).Name, true);
			PXUIFieldAttribute.SetEnabled(_OuterView.Cache, typeof(ARSalesPrice.pendingBreakQty).Name, true);
			PXUIFieldAttribute.SetEnabled(_OuterView.Cache, typeof(ARSalesPrice.expirationDate).Name, true);
		}

		public virtual void SetProcessListDelegate(PXSelectDelegate handler)
		{
			if (handler == null) throw new ArgumentNullException("handler");

			_listHandler = handler;
		}

		[PXProcessButton]
		[PXUIField(DisplayName = ActionsMessages.ProcessAll, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		protected override IEnumerable ProcessAll(PXAdapter adapter)
		{
			ARCustomerSalesPriceMaint graph = (ARCustomerSalesPriceMaint)_Graph;

			WebDialogResult dialogResult = WebDialogResult.Cancel;
			switch (graph.Operations.Current.Action)
			{
				case OperationParam.Copy:
					graph.MassCopySettings.Current.Label = SO.Messages.AllWillBeCopied;
					dialogResult = graph.MassCopySettings.AskExt();
					break;
				case OperationParam.Calc:
					graph.MassCalcSettings.Current.Label = SO.Messages.AllWillBeCalced;
					dialogResult = graph.MassCalcSettings.AskExt();
					break;
				case OperationParam.Update:
					dialogResult = graph.MassUpdateSettings.AskExt();
					break;
			}

			if (dialogResult == WebDialogResult.OK)
			{
				PXView oldOuterView = _OuterView;
				PXFilterRow[] oldFilter = adapter.Filters;
				_OuterView = new PXView(_Graph, false, new Select<Table>(), _listHandler);

				adapter.Filters = View.GetExternalFilters();
				IEnumerable result = base.ProcessAll(adapter);
				adapter.Filters = oldFilter;
				_OuterView = oldOuterView;

				return result;
			}
			else
				return adapter.Get();
		}

		[PXProcessButton]
		[PXUIField(DisplayName = ActionsMessages.Process, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		protected override IEnumerable Process(PXAdapter adapter)
		{
			if (!ExistSelected())
				throw new PXException("No items are selected. Select one or multiple items and click Process again.");
			ARCustomerSalesPriceMaint graph = (ARCustomerSalesPriceMaint)_Graph;
			WebDialogResult dialogResult = WebDialogResult.Cancel;
			switch (graph.Operations.Current.Action)
			{
				case OperationParam.Copy:
					graph.MassCopySettings.Current.Label = SO.Messages.SelectedWillBeProcessed;
					dialogResult = graph.MassCopySettings.AskExt();
					break;
				case OperationParam.Calc:
					graph.MassCalcSettings.Current.Label = SO.Messages.SelectedWillBeProcessed;
					dialogResult = graph.MassCalcSettings.AskExt();
					break;
				case OperationParam.Update:
					dialogResult = graph.MassUpdateSettings.AskExt();
					break;
			}

			if (dialogResult == WebDialogResult.OK)
			{
				return base.Process(adapter);
			}
			else
				return adapter.Get();

		}

		public override void SetSelected<Field>()
		{
			_SelectedField = typeof(Field).Name;
		}

		private bool ExistSelected()
		{
			PXCache cache = _OuterView.Cache;
			foreach (Table item in Cache.Cached)
			{
				object sel = cache.GetValue(item, _SelectedField);
				if (sel != null && Convert.ToBoolean(sel))
				{
					return true;
				}
			}
			return false;
		}

	}
}

