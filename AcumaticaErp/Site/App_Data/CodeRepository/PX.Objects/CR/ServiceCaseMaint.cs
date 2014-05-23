using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.TM;
using PX.Common;

namespace PX.Objects.CR
{
	public class ServiceCaseMaint : PXGraph<ServiceCaseMaint, CRServiceCase>
	{
		#region Constants

		private const int _ONE_MINUTE_TICKS = 60 * 10000000;

		#endregion

		#region Selects

		[PXHidden]
		public ToggleCurrency<CRServiceCase> 
			CurrencyView;

		[PXHidden] 
		public PXSetup<CRSetup> 
			crSetup;

		[PXHidden] 
		public PXSetup<INSetup>
			inSetup;

		[PXHidden] 
		public PXSetup<SO.SOSetup>
			soSetup;

		[PXHidden]
		public CMSetupSelect
			cmSetup;

		[PXViewName(Messages.ServiceCall)]
		public PXSelect<CRServiceCase> 
			ServiceCase;

		[PXViewName(Messages.EquipmentSummary)]
		public PXSelectJoin<CRServiceCase,
			LeftJoin<Equipment, On<Equipment.serviceItemID, Equal<CRServiceCase.serviceItemID>>>,
			Where<CRServiceCase.serviceCaseID, Equal<Current<CRServiceCase.serviceCaseID>>>>
			ServiceCaseCurrent;

		[PXHidden]
		public PXSelect<Customer,
			Where<Customer.bAccountID, Equal<Optional<CRServiceCase.customerID>>>>
			Customer;

		[PXHidden]
		public PXSelect<CurrencyInfo> 
			dummyCuryInfo;

		[PXFilterable]
		[PXViewName(Messages.CallDetails)]
		public PXSelectJoin<CRServiceCaseItem,
			LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<CRServiceCaseItem.inventoryID>>>,
			Where<CRServiceCaseItem.serviceCaseID, Equal<Optional<CRServiceCase.serviceCaseID>>>>
			Details;

		[PXHidden]
		public PXSelect<CRServiceCaseItem,
			Where<CRServiceCaseItem.serviceCaseID, Equal<Current<CRServiceCase.serviceCaseID>>>>
			Transactions;

		[PXHidden]
		public PXSelect<CRServiceCaseItemSplit,
			Where<CRServiceCaseItemSplit.serviceCaseID, Equal<Current<CRServiceCaseItem.serviceCaseID>>, 
				And<CRServiceCaseItemSplit.lineNbr, Equal<Current<CRServiceCaseItem.lineNbr>>>>> 
			Splits;

		[PXHidden]
		public LSServiceCaseItem 
			BinLotSerialSelect;

		[PXHidden]
		public PXSelect<CRServiceCaseItem, 
			Where<CRServiceCaseItem.serviceCaseID, Equal<Current<CRServiceCase.serviceCaseID>>, 
				And<CRServiceCaseItem.isFree, Equal<True>>>, 
			OrderBy<Asc<CRServiceCaseItem.serviceCaseID, 
				Asc<CRServiceCaseItem.lineNbr>>>> 
			FreeItems;

		[PXHidden]
		public PXSelect<CRServiceCaseDiscountDetail,
			Where<CRServiceCaseDiscountDetail.serviceCaseID, Equal<Current<CRServiceCase.serviceCaseID>>>>
			DiscountDetails;

		[PXViewName(Messages.Taxes)]
		public PXSelect<CRServiceCaseTaxTran,
			Where<CRServiceCaseTaxTran.serviceCaseID, Equal<Current<CRServiceCase.serviceCaseID>>>>
			Taxes;

		[PXFilterable]
		[PXViewName(Messages.Labor)]
		public PXSelectJoin<CRServiceCaseLabor,
			LeftJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<CR.CRServiceCaseLabor.employeeID>>>,
			Where<CRServiceCaseLabor.serviceCaseID, Equal<Current<CRServiceCase.serviceCaseID>>>>
			Labor;

		[PXViewName(Messages.DestinationAddress)]
		public PXSelect<CRServiceCaseDestinationAddress,
			Where<CRServiceCaseDestinationAddress.addressID, Equal<Current<CRServiceCase.destinationAddressID>>>>
			DestinationAddress;

		[PXViewName(Messages.BillingAddress)]
		public PXSelect<CRServiceCaseBillingAddress,
			Where<CRServiceCaseBillingAddress.addressID, Equal<Current<CRServiceCase.billAddressID>>>>
			BillingAddress;

		[PXFilterable]
		[PXViewName(Messages.Activities)]
		public CRActivityList<CRServiceCase>
			Activities;

		#endregion

		#region Ctors

		public ServiceCaseMaint()
		{
			this.EnshureCachePersistance(typeof(CRServiceCaseDiscountDetail));

			PXUIFieldAttribute.SetDisplayName<INUnit.fromUnit>(Caches[typeof(INUnit)], Messages.Unit);

			Activities.GetNewEmailAddress =
				() =>
				{
					Contact contact;
					if (ServiceCase.Current != null &&
						(contact = (Contact)PXSelect<Contact>.Search<Contact.contactID>(this, ServiceCase.Current.ContactID)) != null)
					{
						return new Email(contact.DisplayName, contact.EMail);
					}
					return Email.Empty;
				};
		}

		#endregion

		#region Actions

		public PXAction<CRServiceCase> Release;

		[PXUIField(DisplayName = "Release")]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Release)]
		protected virtual IEnumerable release(PXAdapter adapter)
		{
			var row = ServiceCase.Current;
			//TODO: need implementation
			if (row != null && row.Status != "R")
			{
				row.Status = "R";
				ServiceCase.Cache.Update(row);

				Save.Press();
			}
			return adapter.Get();
		}

		#endregion

		#region Event Handlers

		#region CurrencyInfo events

		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (cmSetup.Current.MCActivated == true)
			{
				var customerCuryID = ServiceCase.Current.
					With(c => (Customer)PXSelect<Customer,
						Where<Customer.bAccountID, Equal<Required<CRServiceCase.customerID>>>>.
					Select(this, c.CustomerID)).
					With(ctr => ctr.CuryID);
				if (!string.IsNullOrEmpty(customerCuryID))
				{
					e.NewValue = customerCuryID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (cmSetup.Current.MCActivated == true)
			{
				var customerCuryRateTypeID = ServiceCase.Current.
					With(c => (Customer)PXSelect<Customer,
						Where<Customer.bAccountID, Equal<Required<CRServiceCase.customerID>>>>.
					Select(this, c.CustomerID)).
					With(ctr => ctr.CuryRateTypeID);
				if (!string.IsNullOrEmpty(customerCuryRateTypeID))
				{
					e.NewValue = customerCuryRateTypeID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var current = ServiceCase.Cache.Current;
			if (current != null)
			{
				e.NewValue = ((CRServiceCase)current).CreatedDateTime.With(ctd => ctd.Date);
				e.Cancel = true;
			}
		}

		protected virtual void CurrencyInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var info = e.Row as CurrencyInfo;
			if (info == null) return;

			var curyenabled = !(info.IsReadOnly == true || (info.CuryID == info.BaseCuryID));

			var customer = ServiceCase.Current.
				With(c => (Customer)PXSelect<Customer,
						Where<Customer.bAccountID, Equal<Required<CRServiceCase.customerID>>>>.
				Select(this, c.CustomerID));
			if (customer != null && !(bool)customer.AllowOverrideRate)
			{
				curyenabled = false;
			}

			PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(sender, info, curyenabled);
			PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(sender, info, curyenabled);
			PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(sender, info, curyenabled);
			PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(sender, info, curyenabled);
		}

		#endregion

		#region CRServiceCase

		protected virtual void CRServiceCase_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var row = e.Row as CRServiceCase;
			if (row == null) return;

			var customerTaxZone = GetCustomerTaxZoneID();
			if (customerTaxZone != null)
				e.NewValue = customerTaxZone;
			else
			{
				var address = (CRServiceCaseAddress)BillingAddress.Select();
				if (address != null && !string.IsNullOrEmpty(address.PostalCode))
				{
					e.NewValue = TX.TaxBuilderEngine.GetTaxZoneByZip(this, address.PostalCode);
				}
			}
		}

		protected virtual void CRServiceCase_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (cmSetup.Current.MCActivated == true &&
				(e.ExternalCall || sender.GetValuePending<CRServiceCase.curyID>(e.Row) == null))
			{
				var info = CurrencyInfoAttribute.SetDefaults<CRServiceCase.curyInfoID>(sender, e.Row);
				var row = (CRServiceCase)e.Row;

				var message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(dummyCuryInfo.Cache, info);
				if (!string.IsNullOrEmpty(message))
				{
					sender.RaiseExceptionHandling<CRServiceCase.createdDateTime>(e.Row,
						row.CreatedDateTime, 
						new PXSetPropertyException(message, PXErrorLevel.Warning));
				}
				if (info != null) row.CuryID = info.CuryID;
			}

			SharedRecordAttribute.DefaultRecord<CRServiceCase.destinationAddressID>(sender, e.Row);
			SharedRecordAttribute.DefaultRecord<CRServiceCase.billAddressID>(sender, e.Row);
			sender.SetDefaultExt<CRServiceCase.locationID>(e.Row);
		}

		protected virtual void CRServiceCase_LocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<CRServiceCase.taxZoneID>(e.Row);
		}

		protected virtual void CRServiceCase_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CRServiceCase;
			if (row == null) return;

			row.Duration = null;
			if (row.StartDate != null && row.EndDate != null && row.StartDate.Value <= row.EndDate.Value)
				row.Duration = Convert.ToInt32((row.EndDate.Value - row.StartDate.Value).Ticks / _ONE_MINUTE_TICKS);

			PXUIFieldAttribute.SetEnabled<CRServiceCase.endDate>(ServiceCaseCurrent.Cache, row, row.AllDay != true);
			PXUIFieldAttribute.SetEnabled<CRServiceCase.reminderDate>(ServiceCaseCurrent.Cache, row, row.IsReminderOn == true);

			var estimatedCost = .0m;
			var finalCost = .0m;
			foreach (CRServiceCaseItem item in Transactions.Select(row.ServiceCaseID))
			{
				if (item.EstimatedCuryAmount != null) estimatedCost += (decimal)item.EstimatedCuryAmount;
				if (item.ActualCuryAmount != null) finalCost += (decimal)item.ActualCuryAmount;
			}
			row.EstimatedCost = estimatedCost;
			row.FinalCost = finalCost;

			//TODO: need review
			var isNotReleased = row.Status != "R";
			ServiceCase.Cache.AllowUpdate = isNotReleased;
			ServiceCase.Cache.AllowDelete = isNotReleased;
			var isNotInserted = sender.GetStatus(row) != PXEntryStatus.Inserted;
			Transactions.Cache.AllowUpdate =
				Transactions.Cache.AllowInsert =
				Transactions.Cache.AllowDelete = isNotReleased && isNotInserted;
			Labor.Cache.AllowUpdate =
				Transactions.Cache.AllowInsert =
				Transactions.Cache.AllowDelete = isNotReleased && isNotInserted;

			var customerIsEditable = isNotReleased && Transactions.Select(row.ServiceCaseID).Count == 0;
			PXUIFieldAttribute.SetEnabled<CRServiceCase.customerID>(sender, row, customerIsEditable);
		}

		protected virtual void CRServiceCase_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as CRServiceCase;
			var oldRow = e.OldRow as CRServiceCase;
			if (row == null || oldRow == null) return;

			if (row.LocationID != oldRow.LocationID)
			{
				var laborCache = Caches[typeof(CRServiceCaseLabor)];
				foreach (CRServiceCaseLabor labor in 
					PXSelect<CRServiceCaseLabor, 
						Where<CRServiceCaseLabor.serviceCaseID, Equal<Required<CRServiceCaseLabor.serviceCaseID>>>>.
					Select(this, row.ServiceCaseID))
				{
					var oldTimeSpent = labor.TimeSpent;
					var oldOvertimeSpent = labor.OvertimeSpent;
					var oldTimeBillable = labor.TimeBillable;
					var oldOvertimeBillable = labor.OvertimeBillable;

					SetOvertimeSpent(labor, ServiceCase.Current);
					SetTimeBillable(labor);

					if (labor.TimeSpent != oldTimeSpent || labor.OvertimeSpent != oldOvertimeSpent || 
						labor.TimeBillable != oldTimeBillable || labor.OvertimeBillable != oldOvertimeBillable)
					{
						laborCache.SetStatus(labor, PXEntryStatus.Updated);
					}
				}

				RecalculateAllDiscounts(true);
			}
		}

		protected virtual void CRServiceCase_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			var row = e.Row as CRServiceCase;
			if (row == null) return;

			if (row.StartDate == null) row.StartDate = PXTimeZoneInfo.Now;
		}

		protected virtual void CRServiceCase_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			if (e.Row == null) return;

			if (!Activities.CheckActivitesForDelete(e.Row))
			{
				e.Cancel = true;
				throw new PXException(Messages.ServiceCaseCannotBeDeleted);
			}
		}

		#endregion

		#region CRServiceCaseItem

		protected virtual void CRServiceCaseItem_SiteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var current = ServiceCase.Current;
			if (current != null && current.CustomerID != null && current.LocationID != null)
			{
				var loc = (Location)PXSelect<Location,
					Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,
						And<Location.locationID, Equal<Required<Location.locationID>>>>>.
					Select(this, current.CustomerID, current.LocationID);
				if (loc != null && loc.CSiteID != null)
				{
					e.NewValue = loc.CSiteID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CRServiceCaseItem_TaxCategoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (TX.TaxAttribute.GetTaxCalc<CRServiceCaseItem.taxCategoryID>(sender, e.Row) == TX.TaxCalc.Calc)
			{
				var dfltTaxCategoryID = ServiceCase.Current.
					With(sc => (TX.TaxZone)PXSelect<TX.TaxZone,
						Where<TX.TaxZone.taxZoneID, Equal<Required<TX.TaxZone.taxZoneID>>>>.
					Select(this, sc.TaxZoneID)).
					With(tz => tz.DfltTaxCategoryID);
				if (!string.IsNullOrEmpty(dfltTaxCategoryID) && ((CRServiceCaseItem)e.Row).InventoryID == null)
				{
					e.NewValue = dfltTaxCategoryID;
				}
			}
		}

		protected virtual void CRServiceCaseItem_IsFree_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as CRServiceCaseItem;
			if (row == null) return;

			if (e.ExternalCall) row.ManualDiscount = true;
		}

		protected virtual void CRServiceCaseItem_Discount_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as CRServiceCaseItem;
			if (row == null) return;

			if (e.ExternalCall) row.ManualDiscount = true;
		}

		protected virtual void CRServiceCaseItem_ActualCuryDiscountAmount_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var line = e.Row as CRServiceCaseItem;
			if (line == null || !e.ExternalCall || line.ActualCuryDiscountAmount == null || line.IsFree == true) return;


			if (e.ExternalCall) line.ManualDiscount = true;
		}

		protected virtual void CRServiceCaseItem_ActualCuryAmount_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as CRServiceCaseItem;
			if (row == null) return;

			if (e.ExternalCall) row.ManualPrice = true;
		}

		protected virtual void CRServiceCaseItem_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as CRServiceCaseItem;
			var oldRow = e.OldRow as CRServiceCaseItem;
			if (row == null || oldRow == null) return;

			var inventoryChanged = row.InventoryID != oldRow.InventoryID;
			var customerChanged = row.CustomerID != oldRow.CustomerID;

			if (inventoryChanged)
				sender.SetDefaultExt<CRServiceCaseItem.uOM>(e.Row);

			var current = ServiceCase.Current;
			var isFreeChanged = row.IsFree != oldRow.IsFree;
			var isUOMChanged = row.UOM != oldRow.UOM;
			if (inventoryChanged || isFreeChanged || isUOMChanged)
			{
				var newCuryUnitPrice = 0m;
				if (row.IsFree != true && row.ManualPrice != true) //TODO: need review 'ManualPrice'
				{
					string customerPriceClass = ARPriceClass.EmptyPriceClass;
					string priceClass = current.
							With(cr => (Location)PXSelect<Location,
								Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,
									And<Location.locationID, Equal<Required<Location.locationID>>>>>.
							Select(this, cr.CustomerID, cr.LocationID)).
							With(loc => loc.CPriceClassID);
					if (!string.IsNullOrEmpty(priceClass))
						customerPriceClass = priceClass;

					var curyInfo = (CurrencyInfo)PXSelect<CurrencyInfo,
						Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.
						Select(this, current.CuryInfoID);

					newCuryUnitPrice =
						ARSalesPriceMaint.CalculateSalesPrice(sender, customerPriceClass,
						(int)row.InventoryID,
						curyInfo,
						row.UOM,
						(DateTime)current.CreatedDateTime)
						?? 0m;
				}

				if (row.CuryUnitPrice != newCuryUnitPrice)
					sender.SetValueExt<CRServiceCaseItem.curyUnitPrice>(row, newCuryUnitPrice);
			}


			decimal? manualDiscount = null;
			if (row.ManualDiscount == true && row.Discount != oldRow.Discount) 
				manualDiscount = row.Discount;

			decimal? manualDiscountAmount = null; 
			if (row.ManualDiscount == true && row.ActualCuryDiscountAmount != oldRow.ActualCuryDiscountAmount) 
				manualDiscountAmount = row.ActualDiscountAmount;

			decimal? manualAmount = null; 
			if(row.ManualPrice == true && row.ActualCuryAmount != oldRow.ActualCuryAmount)
				manualAmount = row.ActualCuryAmount;
			if (manualAmount != null)
				manualDiscountAmount = (row.CuryUnitPrice ?? 0) * (row.ActualQuantity ?? 0) - (decimal)manualAmount;
			if (manualDiscountAmount != null)
				manualDiscount = (decimal)manualDiscountAmount * 100 / ((row.CuryUnitPrice ?? 0) * (row.ActualQuantity ?? 0));

			var quantityChanged = row.ActualQuantity != oldRow.ActualQuantity;
			if (inventoryChanged || customerChanged || quantityChanged || manualDiscount != null)
			{
				if (row.IsFree != true)
				{
					SO.SODiscountEngine<CRServiceCaseItem>.
						SetDiscounts(sender, row, current.LocationID, (DateTime)current.CreatedDateTime);
					if (manualDiscount != null)
						sender.SetValueExt<CRServiceCaseItem.discount>(row, manualDiscount);
					RecalculateDiscounts(sender, row);
				}
				else sender.SetValueExt<CRServiceCaseItem.discount>(row, 0m);
				RemoveUnappliableDiscountDetails();
			}

			var curyUnitPriceChanged = row.CuryUnitPrice != oldRow.CuryUnitPrice;
			var discountChanged = row.Discount != oldRow.Discount;
			if (curyUnitPriceChanged || discountChanged || quantityChanged || manualDiscountAmount != null)
			{
				var actualCuryDiscountAmount = 0m;
				if (row.IsFree != true)
				{
					actualCuryDiscountAmount = (row.CuryUnitPrice ?? 0) * (row.ActualQuantity ?? 0) * (row.Discount ?? 0) * 0.01m;
					if (manualDiscountAmount != null)
						actualCuryDiscountAmount = (decimal)manualDiscountAmount;
				}
				sender.SetValueExt<CRServiceCaseItem.actualCuryDiscountAmount>(row, actualCuryDiscountAmount);
			}

			var discountAmountChanged = row.ActualCuryDiscountAmount != oldRow.ActualCuryDiscountAmount;
			if (curyUnitPriceChanged || discountAmountChanged || quantityChanged || manualAmount != null)
			{
				var actualCuryAmount = 0m;
				if (row.IsFree != true)
				{
					actualCuryAmount = (row.CuryUnitPrice ?? 0) * (row.ActualQuantity ?? 0) - (row.ActualCuryDiscountAmount ?? 0);
					if (manualAmount != null)
						actualCuryAmount = (decimal)manualAmount;
				}
				sender.SetValueExt<CRServiceCaseItem.actualCuryAmount>(row, actualCuryAmount);
			}

			//TaxAttribute.Calculate<SOLine.taxCategoryID>(sender, e);
		}

		protected virtual void CRServiceCaseItem_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			//TODO: need review
			var row = e.Row as CRServiceCaseItem;
			if (row == null || row.InventoryID == null || row.IsFree == true) return;

			sender.SetDefaultExt<CRServiceCaseItem.curyUnitPrice>(row);
			RecalculateDiscounts(sender, row);
		}

		protected virtual void CRServiceCaseItem_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CRServiceCaseItem;

			if (row == null) return;

			var lineTypeInventory = IsStockInventory(row.InventoryID);

			PXUIFieldAttribute.SetEnabled<CRServiceCaseItem.subItemID>(sender, row, lineTypeInventory);
			PXUIFieldAttribute.SetEnabled<CRServiceCaseItem.locationID>(sender, row, lineTypeInventory);
			PXUIFieldAttribute.SetEnabled<CRServiceCaseItem.curyUnitPrice>(sender, row, row.IsFree != true);

			var notIsFree = row.IsFree != true;
			var notAutoFreeItem = row.ManualDiscount == true || notIsFree;
			var notEditable = notAutoFreeItem && notIsFree;

			PXUIFieldAttribute.SetEnabled<CRServiceCaseItem.isFree>(sender, e.Row, notAutoFreeItem);
			PXUIFieldAttribute.SetEnabled<CRServiceCaseItem.unitPrice>(sender, e.Row, notEditable);
			PXUIFieldAttribute.SetEnabled<CRServiceCaseItem.estimatedQuantity>(sender, e.Row, notAutoFreeItem);
			PXUIFieldAttribute.SetEnabled<CRServiceCaseItem.actualQuantity>(sender, e.Row, notAutoFreeItem);
			PXUIFieldAttribute.SetEnabled<CRServiceCaseItem.manualDiscount>(sender, e.Row, notEditable);
			PXUIFieldAttribute.SetEnabled<CRServiceCaseItem.unitPrice>(sender, e.Row, notEditable);
			PXUIFieldAttribute.SetEnabled<CRServiceCaseItem.discount>(sender, e.Row, notEditable);
			PXUIFieldAttribute.SetEnabled<CRServiceCaseItem.actualDiscountAmount>(sender, e.Row, notEditable);
			PXUIFieldAttribute.SetEnabled<CRServiceCaseItem.actualAmount>(sender, e.Row, notEditable);
		}

		protected virtual void CRServiceCaseItem_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (((CRServiceCaseItem)e.Row).InventoryID != null)
				RecalculateAllDiscounts(false);
		}

		protected virtual void CRServiceCaseItem_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || 
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				var lineTypeInventory = IsStockInventory(((CRServiceCaseItem)e.Row).InventoryID);
				PXDefaultAttribute.SetPersistingCheck<CRServiceCaseItem.subItemID>(sender, e.Row,
					lineTypeInventory ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
			}
		}

		#endregion

		#region CRServiceCaseLabor

		protected virtual void CRServiceCaseLabor_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			var row = e.Row as CRServiceCaseLabor;
			if (row == null) return;

			var current = ServiceCase.Current;
			if (row.StartDate == null && current != null && current.StartDate != null)
				row.StartDate = ((DateTime)current.StartDate).Date;
			if (row.TimeSpent == null) row.TimeSpent = 0;
			if (row.OvertimeSpent == null) row.OvertimeSpent = 0;
			if (row.TimeBillable == null) row.TimeBillable = 0;
			if (row.OvertimeBillable == null) row.OvertimeBillable = 0;
		}

		protected virtual void CRServiceCaseLabor_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as CRServiceCaseLabor;
			var oldRow = e.OldRow as CRServiceCaseLabor;
			if (row == null || oldRow == null) return;

			SetOvertimeSpent(row, ServiceCase.Current);

			if (row.TimeBillable == oldRow.TimeBillable && row.OvertimeBillable == oldRow.OvertimeBillable &&
				(row.StartDate != oldRow.StartDate || row.TimeSpent != oldRow.TimeSpent || row.OvertimeSpent != oldRow.OvertimeSpent))
			{
				SetTimeBillable(row);
			}

			ValidateTimeBillable(row);

			ValidateOvertimeBillable(row);
		}

		protected virtual void CRServiceCaseLabor_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = e.Row as CRServiceCaseLabor;
			if (row == null) return;

			ValidateTimeBillable(row);

			ValidateOvertimeBillable(row);
		}

		#endregion

		#region CRServiceCaseBillingAddress

		protected virtual void CRServiceCaseBillingAddress_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as CRServiceCaseBillingAddress;
			if (row == null) return;

			if (GetCustomerTaxZoneID() != null && !string.IsNullOrEmpty(row.PostalCode))
			{
				var taxZone = TX.TaxBuilderEngine.GetTaxZoneByZip(this, row.PostalCode);
				ServiceCase.Cache.SetValueExt<CRServiceCase.taxZoneID>(ServiceCase.Current, taxZone);
			}
		}

		#endregion

		#endregion

		#region Private Methods

		private string GetCustomerTaxZoneID()
		{
			var customerLocation = ServiceCase.Current.
				With(sc => (Location)PXSelect<Location, 
					Where<Location.locationID, Equal<Required<Location.locationID>>>>.
				Select(this, sc.LocationID));
			if (customerLocation != null && !string.IsNullOrEmpty(customerLocation.CTaxZoneID))
				return customerLocation.CTaxZoneID;
			return null;
		}

		private void RecalculateAllDiscounts(bool resetManualDisc)
		{
			foreach (CRServiceCaseDiscountDetail detail in DiscountDetails.Select())
			{
				DiscountDetails.Delete(detail);
			}

			var lines = Transactions.Select();
			foreach (CRServiceCaseItem line in lines)
			{
				//reset all manual discounts:
				if (resetManualDisc)
					line.ManualDiscount = false;

				SO.SODiscountEngine<CRServiceCaseItem>.ClearDetDiscComponents(line);

				if (Transactions.Cache.GetStatus(line) == PXEntryStatus.Notchanged) 
					Transactions.Cache.SetStatus(line, PXEntryStatus.Updated);
			}

			foreach (CRServiceCaseItem line in lines)
				Transactions.Update(line);

			UpdateFreeItemLines(ServiceCase.Current);
		}

		private void RemoveUnappliableDiscountDetails()
		{
			var detDiscounts = new List<string>();
			foreach (CRServiceCaseItem line in Transactions.Select())
			{
				if (!string.IsNullOrEmpty(line.DetDiscIDC1) && !string.IsNullOrEmpty(line.DetDiscSeqIDC1))
				{
					string key = string.Format("{0}.{1}", line.DetDiscIDC1, line.DetDiscSeqIDC1);
					if (!detDiscounts.Contains(key))
						detDiscounts.Add(key);
				}

				if (!string.IsNullOrEmpty(line.DetDiscIDC2) && !string.IsNullOrEmpty(line.DetDiscSeqIDC2))
				{
					string key = string.Format("{0}.{1}", line.DetDiscIDC2, line.DetDiscSeqIDC2);
					if (!detDiscounts.Contains(key))
						detDiscounts.Add(key);
				}
			}

			foreach (CRServiceCaseDiscountDetail detail in DiscountDetails.Select())
			{
				string key = string.Format("{0}.{1}", detail.DiscountID, detail.DiscountSequenceID);
				if (!detDiscounts.Contains(key))
					DiscountDetails.Delete(detail);
			}
		}
		
		private void RecalculateDiscounts(PXCache sender, CRServiceCaseItem line)
		{
			var current = ServiceCase.Current;
			if (current != null && line.Qty != null)
			{
				if (line.ManualDiscount == false)
				{
					SO.SODiscountEngine<CRServiceCaseItem>.
						CalculateLineDiscounts(
							sender, 
							line, 
							current.LocationID, 
							(DateTime)current.CreatedDateTime);
				}

				SO.SODiscountEngine<CRServiceCaseItem>.
					UpdateDiscountDetails<CRServiceCaseDiscountDetail>(
						sender,
						Transactions, 
						DiscountDetails, 
						line,
						current.LocationID,
						(DateTime)current.CreatedDateTime, 
						soSetup.Current.ProrateDiscounts == true);

				UpdateFreeItemLines(current);
			}
		}

		private void UpdateFreeItemLines(CRServiceCase current)
		{
			var groupedByInventory = new Dictionary<int, decimal>();

			foreach (CRServiceCaseDiscountDetail item in 
				PXSelect<CRServiceCaseDiscountDetail,
					Where<CRServiceCaseDiscountDetail.serviceCaseID, Equal<Required<CRServiceCaseDiscountDetail.serviceCaseID>>>>.
					Select(this, current.ServiceCaseID))
			{
				if (item.FreeItemID != null)
				{
					var val = item.FreeItemQty ?? 0;
					if (groupedByInventory.ContainsKey(item.FreeItemID.Value))
						groupedByInventory[item.FreeItemID.Value] += val;
					else
						groupedByInventory.Add(item.FreeItemID.Value, val);
				}
			}

			bool freeItemChanged = false;

			RowDeleted.RemoveHandler(typeof(CRServiceCaseItem), CRServiceCaseItem_RowDeleted);
			RowInserted.RemoveHandler(typeof(CRServiceCaseItem), CRServiceCaseItem_RowInserted);
			RowUpdated.RemoveHandler(typeof(CRServiceCaseItem), CRServiceCaseItem_RowUpdated);
			try
			{
				foreach (CRServiceCaseItem line in FreeItems.Select())
					if (line.ManualDiscount == false && line.InventoryID != null)
					{
						if (groupedByInventory.ContainsKey(line.InventoryID.Value))
						{
							if (groupedByInventory[line.InventoryID.Value] == 0)
							{
								FreeItems.Delete(line);
								freeItemChanged = true;
							}
						}
						else
						{
							FreeItems.Delete(line);
							freeItemChanged = true;
						}
					}

				int? defaultWarehouse = GetDefaultWarehouse();
				foreach (KeyValuePair<int, decimal> kv in groupedByInventory)
				{
					CRServiceCaseItem freeLine = GetFreeLineByItemID(kv.Key, current.ServiceCaseID);

					if (freeLine == null)
					{
						if (kv.Value > 0)
						{
							var line = new CRServiceCaseItem();
							line.InventoryID = kv.Key;
							line.IsFree = true;
							line.SiteID = defaultWarehouse;
							line.ActualQuantity = kv.Value;
							
							FreeItems.Insert(line);

							freeItemChanged = true;
						}
					}
					else
					{
						freeLine.ActualQuantity = kv.Value; //TODO: need set other fields

						FreeItems.Update(freeLine);

						freeItemChanged = true;
					}
				}
			}
			finally
			{
				RowDeleted.AddHandler(typeof(CRServiceCaseItem), CRServiceCaseItem_RowDeleted);
				RowInserted.AddHandler(typeof(CRServiceCaseItem), CRServiceCaseItem_RowInserted);
				RowUpdated.AddHandler(typeof(CRServiceCaseItem), CRServiceCaseItem_RowUpdated);
			}

			if (freeItemChanged)
				Details.View.RequestRefresh();
		}

		private int? GetDefaultWarehouse()
		{
			PXResultset<CRServiceCaseItem> resultset = Transactions.Select();

			if (resultset.Count == 0)
				return null;

			int? result = ((CRServiceCaseItem)resultset).SiteID;

			for (int i = 1; i < resultset.Count; i++)
			{
				if (((CRServiceCaseItem)resultset[i]).SiteID != result)
					return null;
			}

			return result;
		}

		private CRServiceCaseItem GetFreeLineByItemID(int? inventoryID, int? serviceCaseID)
		{
			return PXSelect<CRServiceCaseItem,
				Where<CRServiceCaseItem.serviceCaseID, Equal<Required<CRServiceCaseItem.serviceCaseID>>,
					And<CRServiceCaseItem.isFree, Equal<True>,
					And<CRServiceCaseItem.inventoryID, Equal<Required<CRServiceCaseItem.inventoryID>>,
					And<CRServiceCaseItem.manualDiscount, Equal<False>>>>>>.
				Select(this, serviceCaseID, inventoryID);
		}

		private bool IsStockInventory(int? inventoryID)
		{
			return ((InventoryItem)PXSelect<InventoryItem,
				Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.
				Select(this, inventoryID)).StkItem == true;
		}

		private void ValidateOvertimeBillable(CRServiceCaseLabor row)
		{
			if (row.OvertimeSpent != null && row.OvertimeBillable != null && row.OvertimeBillable > row.OvertimeSpent)
				Caches[row.GetType()].RaiseExceptionHandling<CRServiceCaseLabor.overtimeBillable>(row, row.OvertimeBillable,
					new PXSetPropertyException(Messages.OvertimeBillableCannotBeGreaterThanOvertimeSpent));
		}

		private void ValidateTimeBillable(CRServiceCaseLabor row)
		{
			if (row.TimeSpent != null && row.TimeBillable != null && row.TimeBillable > row.TimeSpent)
				Caches[row.GetType()].RaiseExceptionHandling<CRServiceCaseLabor.timeBillable>(row, row.TimeBillable,
					new PXSetPropertyException(Messages.BillableTimeCannotBeGreaterThanTimeSpent));
		}

		private static void SetTimeBillable(CRServiceCaseLabor item)
		{
			if (item == null) return;

			item.TimeBillable = item.TimeSpent != null && item.TimeSpent >= 0 ? item.TimeSpent : 0;
			item.OvertimeBillable = item.OvertimeSpent != null && item.OvertimeSpent >= 0 ? item.OvertimeSpent : 0;
		}

		private void SetOvertimeSpent(CRServiceCaseLabor item, CRServiceCase serviceCase)
		{
			if (item == null || item.StartDate == null || item.TimeSpent == null) return;
			try
			{
				var startDate = (DateTime)item.StartDate;
				var endDate = startDate.AddMinutes((int)item.TimeSpent);
				var empoyeeID = item.EmployeeID;
				var locationID = serviceCase.With(c => c.LocationID);

				var overtimespent = CalculateOvertime(startDate, endDate, empoyeeID, locationID);
				item.OvertimeSpent = (int)overtimespent.TotalMinutes;
			}
			catch (ArgumentOutOfRangeException)
			{
			}
		}

		private TimeSpan CalculateOvertime(DateTime start, DateTime end, int? employeeID, int? locationID)
		{
			string calendarId = null;
			/*if (CurrentServiceCaseClass.Current != null && !string.IsNullOrEmpty(CurrentServiceCaseClass.Current.CalendarID))
				calendarId = CurrentServiceCaseClass.Current.CalendarID;
			else*/
			if (string.IsNullOrEmpty(calendarId) && employeeID != null)
				calendarId = ((EPEmployee)PXSelect<EPEmployee,
								Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.
								Select(this, employeeID)).
								With(e => e.CalendarID);
			if (string.IsNullOrEmpty(calendarId) && locationID != null) 
				calendarId = ((Location)PXSelect<Location,
								Where<Location.locationID, Equal<Required<Location.locationID>>>>.
								Select(this, locationID)).
								With(l => l.CCalendarID);

			return calendarId == null ? new TimeSpan() : CalendarHelper.CalculateOvertime(this, start, end, calendarId);
		}

		#endregion
	}
}
