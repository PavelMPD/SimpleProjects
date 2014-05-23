using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Common;

namespace PX.Objects.SO
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		public const string Prefix = "SO Error";

		#region Captions
		public const string ViewDocument = "View Document";
		#endregion

		#region Graph Names
		public const string SODiscountMaint = "Discount Maintenance";
		public const string SODiscountSequenceMaint = "Discount Sequence Maintenance";
		public const string SOOrderEntry = "Sales Order Entry";
		public const string SOShipmentEntry = "Shipment Entry";
		public const string SOInvoiceEntry = "Invoice Entry";
		public const string SOSalesPriceMaintenance = "Sales Price Maintenance";
		public const string SOUpdateDiscounts = "Update Discounts";
		public const string SOUpdateSalesPrice = "Update Sales Price";
		public const string SOSalesPriceImport = "Sales Price Import";
		public const string SOOrder = "Sales Order";
		public const string SOLine = "Sales Order Line";
		public const string BillingAddress = "Billing Address";
		public const string BillingContact = "Billing Contact";
		public const string ShippingAddress = "Shipping Address";
		public const string ShippingContact = "Shipping Contact";
		public const string SOCarrierRates = "Carrier Rates";
		public const string SOPaymentEntry = "SO Payment Entry";
		public const string SOCreate = "Create Transfer Orders";
		public const string SOExternalTaxCalcProcess = "SO avalara Tax Calculation Process";
        public const string SOSetup = "Sales Orders Preferences";
        #endregion

		#region DAC Names
		public const string SOAdjust = "Sales Order Adjust";
		public const string SOLineSplit = "Sales Order Line Split";
		public const string SOOrderDiscountDetail = "Sales Order Discount Detail";
		public const string SOSalesPerTran = "SO Salesperson Comission";
		public const string SOTaxTran = "Sales Order Tax";
		public const string SOMiscLine = "Sales Order Misc. Charge";
		public const string SOOrderShipment = "Sales Order Shipment";
		public const string SOShipmentDiscountDetail = "Shipment Discount Detail";
		public const string SOShipLineSplit = "Shipment Line Split";
		public const string SOPackageDetail = "SO Package Detail";
		public const string SOShipment = "Shipment";
		public const string SOShipLine = "Shipment Line";
		public const string ShipmentContact = "Shipment Contact";
		public const string ShipmentAddress = "Shipment Address";
		public const string SOInvoiceDiscountDetail = "SO Invoice Discount Detail";
		public const string SOFreightDetail = "SO Freight Detail";
		public const string SOInvoice = "SO Invoice";
		public const string DiscountSequence = "Discount Sequence";
        public const string SODiscountSetupDocument = "Document-Level Discount";
        public const string SODiscountSetupLine = "Item-Level Discount";
		#endregion

		#region Field Names
		public const string OrigOrderQty = "Ordered Qty.";
		public const string OpenOrderQty = "Open Qty.";
		public const string SiteDescr = "Warehouse Description";
		public const string CarrierDescr = "Ship Via Description";
		public const string CustomerID = "Customer ID";
		#endregion

		#region Validation and Processing Messages

		public const string UnappliedBalanceIncludesAR = "Includes Unreleased AR Applications";
		public const string MissingShipmentControlTotal = "Control Total is required for shipment confirmation.";
		public const string UnableConfirmZeroShipment = "Unable to confirm zero shipment {0}.";
		public const string UnableConfirmShipment = "Unable to confirm empty shipment {0}.";
		public const string MissingMassProcessWorkFlow = "Work Flow is not set up for this screen.";
		public const string DocumentOutOfBalance = AR.Messages.DocumentOutOfBalance;
		public const string DocumentBalanceNegative = AR.Messages.DocumentBalanceNegative;
		public const string AssignNotSetup = "Default Sales Order Assignment Map is not entered in SO setup";
		public const string AssignNotSetup_Shipment = "Default Sales Order Shipment Assignment Map is not entered in SO setup";
		public const string CannotShipComplete = "Order cannot be shipped in full.";
        public const string CannotShipCompleteTraced = "Order {0} {1} cannot be shipped in full.";
        public const string CannotShipTraced = "Order {0} {1} does not contain any available items. Check previous warnings.";
        public const string NothingToShipTraced = "Order {0} {1} does not contain any items planned for shipment on '{2}'.";
        public const string NothingToReceiveTraced = "Order {0} {1} does not contain any items planned for receipt on '{2}'.";
		public const string UniqueItemConstraint = "Same Item cannot be listed more than once. Same item cannot belong to two or more active discount sequences of the same discount code";
        public const string UniqueBranchConstraint = "Same Branch cannot be listed more than once. Same branch cannot belong to two or more active discount sequences of the same discount code";
        public const string UniqueWarehouseConstraint = "Same Warehouse cannot be listed more than once. Same warehouse cannot belong to two or more active discount sequences of the same discount code";
		public const string UnconditionalDiscUniqueConstraint = "Unconditional discounts cannot have active overlapping sequences.";
		public const string DiscSeqCannotActivate = "Cannot activate the sequence because one or more constraints failed to validate.";
		public const string InvoiceCheck_QtyNegative = "Item '{0} {1}' in invoice '{2}' quantity returned is greater than quantity invoiced.";
		public const string InvoiceCheck_QtyLotSerialNegative = "Item '{0} {1}' in invoice '{2}' lot/serial number '{3}' quantity returned is greater than quantity invoiced.";
		public const string InvoiceCheck_SerialAlreadyReturned = "Item '{0} {1}' in invoice '{2}' serial number '{3}' is already returned.";
		public const string InvoiceCheck_LotSerialInvalid = "Item '{0} {1}' in invoice '{2}' lot/serial number '{3}' is missing from invoice.";
		public const string OrderCheck_QtyNegative = "Item '{0} {1}' in order '{2} {3}' quantity shipped is greater then quantity ordered.";
        public const string OrderSplitCheck_QtyNegative = "For item '{0} {1}' in order '{2} {3}', the quantity shipped is greater than the quantity allocated.";
		public const string BinLotSerialInvalid = "Shipment Scheduling and Bin/Lot/Serial assignment are not possible for non-stock items.";
		public const string BackOrderCannotBeDeleted = "Back Order cannot be deleted.";

		public const string Availability_Info = IN.Messages.Availability_Info;
		public const string Availability_AllocatedInfo = "On Hand {1} {0}, Available {2} {0}, Available for Shipping {3} {0}, Allocated {4} {0}";

		public const string DiscountEngine_InventoryIDIsNull = "Given line is not properly initialized before calling this method. InventoryID property should be set to a valid value.";
		public const string DiscountEngine_QtyIsNull = "Given line is not properly initialized before calling this method. Qty property should be set to a valid value.";
		public const string DiscountEngine_DateIsNull = "Given line is not properly initialized before calling this method. Date property should be set to a valid value.";
		public const string DiscountEngine_CuryExtPriceIsNull = "Given line is not properly initialized before calling this method. CuryExtPrice property should be set to a valid value.";
		public const string SequenceExists = "Cannot delete a Discount if there already exist one or more Discount Sequences associated with the given Discount";
		public const string SequenceExistsApplicableTo = "Cannot change Applicable To for the Discount if there already exist one or more Discount Sequences associated with the given Discount";
		public const string ShipmentExistsForSiteCannotReopen = "New shipment already created for order {0} {1}, current shipment cannot be reopened.";
		public const string ShipmentInvoicedCannotReopen = "Shipment already invoiced for order {0} {1}, shipment cannot be reopened.";
		public const string ShipmentCancelledCannotReopen = "Order {0} {1} is cancelled, shipment cannot be reopened.";
		public const string PromptReplenishment = "Replenishment is required for item '{0}'.";
        public const string ItemNotAvailableTraced = "There is no stock available for item {0} warehouse {1}";
		public const string ItemNotAvailable = "There is no stock available for this item.";
		public const string GrossProfitValidationFailed = "Minimum Gross Profit requirement is not satisfied.";
		public const string GrossProfitValidationFailedAndFixed = "Minimum Gross Profit requirement is not satisfied. Discount was reduced to maximum valid value.";
		public const string GrossProfitValidationFailedAndPriceFixed = "Minimum Gross Profit requirement is not satisfied. Sales price was set to minimum valid value.";
		public const string DefaultRateNotSetup = "Default Rate Type is not configured in SO Setup.";
		public const string FreightAccountIsRequired = "Freight Account is required. Order Type is not properly setup.";
		public const string NoDropShipLocation = "Drop-Ship Location is not configured for warehouse {0}";
		public const string NoRMALocation = "RMA Location is not configured for warehouse {0}";
		public const string FailToInsertSalesPrice = "Failed to preload {0} records from the Inventory.";
		public const string OrderTypeShipmentOrLocation = "Process Shipment or Require Location should be defined when Inventory Transaction Type is specified.";
		public const string OrderTypeUnsupportedCombination = "Selected combination of Inventory Transaction Type and AR Document Type is not supported.";
		public const string OrderTypeUnsupportedOperation = "Selected Inventory Transaction Type is not supported for this type operation.";		
		public const string FailedToConvertToBaseUnits = "Failed to convet to Base Units and Check for Minimum Gross Profit requirement";
		public const string SalesPriceSourceDestEquals = "Source and Destination Currency and Price Class are the same.";
		public const string DiscountsNotvalid = "One or more validations failed for the given discount sequence. Please fix the errors and try again.";
		public const string OnlyOneFlatAllowed = "Only one Flat Price discount is allowed for every \"Applicable To\" target.";
		public const string PromotionDateOverlap = "There is already a promotion for item {0} that overlaps the interval from {1} to {2}.";
		public const string MultipleFlatDiscount = "There is already a regular discount for item {0} defined in another discount sequence.";
		public const string CarrierServiceError = "Carrier Service returned error. {0}";
		public const string DefaultWarehouseIsRequired = "Default Warehouse is required to calculate Freight. It's address is used as shipment origin.";
		public const string WarehouseIsRequired = "Warehouse is required. It's address is used as shipment origin.";
		public const string InvalidWeightUOM = "Weight UOM in INSetup is not valid. Expecting KG or POUND.";
		public const string RateTypeNotSpecified = "Carrier returned rates in currency that differs from the Order currency. RateType that is used to convert from one currency to another is not specified is SOSetup.";
		public const string ReprintLabels = "Labels can be used only once. Are you sure you want to reprint labels?";
		public const string PackagesShipped = "Packages already shipped via Carrier. Labels are generaned. To regenerate labels first you have to Cancel previous shipment.";
		public const string NoPackagesOrder = "Order does not contain any packages.";
        public const string WeightExceedsBoxSpecs = "The weight specified exceeds the max. weight of the box. Choose a bigger box or use multiple boxes.";
		public const string TransferOrderCreated = "Transfer Order '{0}' created.";
		public const string OrderApplyAmount_Cannot_Exceed_OrderTotal = "Amount Applied to Order cannot exceed Order Total";
		public const string AuthorizationMayNotBeEnteredForTheOrderWithZeroBalance = "Authorization Number may not be entered if the order has 0 total or is already payed in full";
		public const string PreAutorizationAmountShouldBeEntered  = "Pre-authorized Amount must be entered if pre-authorization number is provided";
		public const string CannotCancelCCProcessed = "Cannot cancel order. Credit Card pre-authorised or captured.";
		public const string BinLotSerialNotAssigned = IN.Messages.BinLotSerialNotAssigned;
		public const string CarrierUOMIsRequired = "Carrier UOM is required. Please setup UOM for Carrier {0} and retry.";
		public const string ConfirmationIsRequired = "Confirmation for each and every Package is required. Please confirm and retry.";
		public const string MixedFormat = "Your selection contains mixed label format: {0} Thermal Labels and {1} Laser labels. Please select records of same kind and try again.";
		public const string FailedToSaveMergedFile = "Failed to Save Merged file.";
		public const string NoLinesMatchPromo = "No lines match Promo Code.";
		public const string MissingSourceSite = "Missing Source Site to create transfer.";
		public const string EqualSourceDestinationSite = "Source and destination sites should not be the same.";
		public const string EmailSalesOrderError = "Email sales order has failed. Please, check Customer's email or notification configuration";
		public const string PartialInvoice = "Sales Order/Shipment cannot be invoiced partially.";
		public const string TransfertNonStock = "Transfer isn't supported for non stock item";
		public const string ShippedLineDeleting = "Cannot delete line with shipment.";
		public const string ConfirmShipDateRecalc = "Do you want to update order lines with changed requested date and recalculate scheduled shipment date?";
		public const string PlanDateGreaterShipDate = "Scheduled Shipment Date greater than Shipment Date";
		public const string CannotDeleteTemplateOrderType = "Order type cannot be deleted. It is in use as template for order type '{0}'.";
		public const string CustomeCarrierAccountIsNotSetup = "Customer Account is not configured. Please setup the Carrier Account on the Carrier Plug-in screen.";
		public const string TaskWasNotAssigned = "Failed to automatically assign Project Task to the Transaction. Please check that the Account-Task mapping exists for the given account '{0}' and Task is Active and Visible in the given Module.";
		public const string PackageIsRequired = "At least one confirmed package is required to confirm this shipment.";
		public const string InvalidInventory = "Invalid Inventory Item. You can only select Inventory Item that exists in the Order.";
		public const string UnbilledBalanceWithoutTaxTaxIsNotUptodate = "Balance does not include Tax. Unbilled Tax is not up-to-date.";
		public const string UseInvoiceDateFromShipmentDateWarning = "Shipment Date will be used for Invoice Date.";
		public const string DiscountTypeCannotChanged = "Discount Type can not be changed if Discount Code has Discount Sequence";
		public const string InvalidShipmentCounters = "Shipment counters are corrupted.";
		public const string SalesOrderWillBeDeleted = "The Sales Order has a payment applied. Deleting the Sales Order will delete the payment reservation. Continue?";

		public const string BoxesNotDefined = "There should be at least one box common between the boxes defined on the Inventory Item level and the boxes defined in the ShipVia screen. Please correct this and try again.";
		public const string AtleastOnePackageIsRequired = "When using 'Manual Packaging' option at least one package must be defined before a Rate Quote can be requested from the Carriers.";
		public const string AtleastOnePackageIsInvalid = "Warehouse must be defined for all packages before a Rate Quote can be requested from the Carriers.";
		public const string AutoPackagingZeroPackWarning = "Autopackaging for {0} resulted in zero packages. Please check your settings. Make sure that boxes used for packing are configured for a carrier.";
		public const string AutoPackagingIssuesCheckTrace = "There were some issues with autopackaging please check the Trace information for details.";

        public const string NoDiscountFound = "The Discount Code {0} has no matching Discount Sequence to apply.";
        public const string DiscountGreaterLineTotal = "Discount Total may not be greater than Line Total.";
        public const string DiscountGreaterLineMiscTotal = "Discount Total may not be greater than Line Total + Misc. Total.";
        public const string NoApplicableSequenceFound = "No applicable discount sequence found.";
        public const string MultipleApplicableSequencesFound = "Two or more applicable discount sequences found. Please select discount sequence manually.";
        public const string UnapplicableSequence = "Discount Sequence cannot be applied to this document.";
        public const string DocumentDicountCanNotBeAdded = "Skip Document Discounts option is set for one or more group discounts. Document discount can not be added.";
        public const string NoOrder = "<NEW>";
        public const string NoOrderType = "IN";
		public const string DuplicateSalesPrice = "Duplicate Sales Price";
        public const string AccountMappingNotConfigured = "Account Task Mapping is not configured for the following Project: {0}, Account: {1}";
        public const string NoLineSelected = "No Document Details line selected. Please select Document Details line.";
		public const string NoBoxForItem = "Packages is not configured properly for item {0}. Please correct in on Stock Items screen and try again. Given item do not fit in any of the box configured for it.";
		public const string OrderTypeInactive = "Order Type is inactive.";
		#endregion

		#region Translatable Strings used in the code


		public const string SOCreateShipment = "Create Shipment";

		public const string ShipComplete = "Ship Complete";
		public const string BackOrderAllowed = "Back Order Allowed";
		public const string CancelRemainder = "Cancel Remainder";

		public const string Inventory = "Goods for Inventory";
		public const string NonInventory = "Non-Inventory Goods";
		public const string MiscCharge = "Misc. Charge";

		public const string SalesOrder = "Sales Order";
		public const string SalesOrderCustomer = "Customer";
		public const string SalesOrderBillingAddress = "Billing Address";
		public const string SalesOrderShippingAddress = "Shipping Address";
		public const string SalesOrderShipment = "Shipment";
		public const string SalesOrderShipmentCustomer = "Customer";
		public const string SalesOrderShipmentAddress = "Address";
		public const string Assign = CR.Messages.Assign;

		public const string ShipVia = "Ship Via";
		public const string DocDiscDescr = "Document Discount";
		public const string LineDiscDescr = "Item Discount";
		public const string FreightDescr = "Freight ShipVia {0}";
		public const string OrderType = "Order Type";
		public const string AddInvoice = "Add Invoice";

		public const string SelectedWillBeProcessed = "Only the selected items will be processed.";
		public const string AllWillBeCopied = "WARNING: Every item listed will be copied.";
		public const string AllWillBeCalced = "WARNING: Every item listed will be updated.";
		public const string RefreshFreight = "Refresh Freight Cost";

		public const string ShipDate = "Ship Date";
		public const string CancelBy = "Cancel By";
		public const string OrderDate = "Order Date";
		public const string SearchString = "Search String";
		
		#endregion

		#region Order + Shipment Statuses
		public const string Order = "Order";
		public const string Open = "Open";
		public const string Hold = "On Hold";
		public const string CreditHold = "Credit Hold";
		public const string Completed = "Completed";
		public const string Cancelled = "Canceled";
		public const string Confirmed = "Confirmed";
		public const string Invoiced = "Invoiced";
		public const string BackOrder = "Back Order";
		public const string Shipping = "Shipping";
		public const string Receipted = "Receipted";
		public const string AutoGenerated = "Auto-Generated";
		public const string PartiallyInvoiced = "Partially Invoiced";
		#endregion

		#region Shipment Type
		public const string Normal = "Shipment";
		#endregion

		#region Operation Type
		public const string Issue = IN.Messages.Issue;
		public const string Receipt = IN.Messages.Receipt;
		#endregion

		#region Order Behavior
		public const string SOName = "Sales Order";
		public const string INName = "Invoice";
		public const string QTName = "Quote";
		public const string RMName = "RMA Order";
		public const string CMName = "Credit Memo";
		#endregion

		#region Discount Type

		public const string Line = "Line";
		public const string Group = "Group";
		public const string Document = "Document";
		public const string Flat = "Flat-Price";
		public const string Unconditional = "Unconditional";

		#endregion

		#region Discount Target
		public const string Customer = "Customer";
		public const string Discount_Inventory = "Item";
		public const string CustomerPrice = "Customer Price Class";
		public const string InventoryPrice = "Item Price Class";
		public const string CustomerAndInventory = "Customer and Item";
		public const string CustomerPriceAndInventory = "Customer Price Class and Item";
		public const string CustomerAndInventoryPrice = "Customer and Item Price Class";
		public const string CustomerPriceAndInventoryPrice = "Customer Price Class and Item Price Class";

        public const string CustomerAndBranch = "Customer and Branch";
        public const string CustomerPriceAndBranch = "Customer Price Class and Branch";
        public const string Warehouse = "Warehouse";
        public const string WarehouseAndInventory = "Warehouse and Item";
        public const string WarehouseAndCustomer = "Warehouse and Customer";
        public const string WarehouseAndInventoryPrice = "Warehouse and Item Price Class";
        public const string WarehouseAndCustomerPrice = "Warehouse and Customer Price Class";
        public const string Branch = "Branch";
		#endregion

		#region Discount Option
		public const string Percent = "Percent";
		public const string Amount = "Amount";
        public const string FreeItem = "Free Item";
		#endregion

		#region BreakdownType
		public const string Quantity = "Quantity";
		#endregion

		#region AddItemType
		public const string BySite = "All Items";
		public const string ByCustomer = "Purchased Since";
		#endregion

		#region Sales Price Update Unit

		public const string BaseUnit = "Base Unit";
		public const string SalesUnit = "Sales Unit";

		#endregion

		#region Custom Actions
		public const string Process = IN.Messages.Process;
		public const string ProcessAll = IN.Messages.ProcessAll;
		#endregion

		#region DiscountAppliedTo
		public const string ExtendedPrice = "Item Extended Price";
		public const string SalesPrice = "Item Price";
		#endregion

		#region FreeItemShipType
		public const string Proportional = "Proportional";
		public const string OnLastShipment = "On Last Shipment";
		#endregion

		#region Freight Allocation
		public const string FullAmount = "Full Amount First Time";
		public const string Prorate = "Allocate Proportionally";
		#endregion

		#region Minimal Gross Profit Validation Allocation
		public const string None = "No Validation";
		public const string Warning = "Warning";
		public const string SetToMin = "Set to minimum";
		#endregion

		#region PackagingType

		public const string PackagingType_Auto = "Auto";
		public const string PackagingType_Manual = "Manual";
		public const string PackagingType_Both = "Auto and Manual"; 
		#endregion
	}
}
