using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Common;

namespace PX.Objects.IN
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		#region Validation and Processing Messages
		public const string Prefix = "IN Error";
		public const string Document_Status_Invalid = "Document Status is invalid for processing.";
		public const string DocumentOutOfBalance = "Document is out of balance.";
		public const string Document_OnHold_CannotRelease = "Document is On Hold and cannot be released.";
		public const string Inventory_Negative = "Inventory quantity for {0} in warehouse '{1} {2}' will go negative.";
		public const string Inventory_Negative2 = "Inventory quantity will go negative.";
		public const string Transfered_Item_Receipted = "Inventory item '{0}' already receipted in Receipt '{1}'";
		public const string SubItemSeg_Missing_ConsolidatedVal = "Subitem Segmented Key missing one or more Consolidated values.";
		public const string TranType_Invalid = "Invalid Transaction Type.";
		public const string InternalError = "Internal Error: {0}.";
		public const string SerialQtyError = "Invalid quantity on hand for one or more serial number.";
		public const string NotPrimaryLocation = "Selected item is not allowed in this location.";
		public const string LocationReceiptsInvalid = "Selected Location is not valid for receipts.";
		public const string LocationSalesInvalid = "Selected Location is not valid for sales.";
		public const string LocationTransfersInvalid = "Selected Location is not valid for transfers.";
		public const string Location = "Location";
		public const string StandardCostNoCostOnlyAdjust = "Cost only adjustments are not allowed for Standard Cost items.";
		public const string StatusCheck_QtyNegative = "Updating item '{0} {1}' in warehouse '{2}' quantity available will go negative.";
		public const string StatusCheck_QtyNegative2 = "Updating item '{0} {1}' in warehouse '{2}' in cost layer '{3}' quantity  will go negative.";
		public const string StatusCheck_QtyAvailNegative = "Updating item '{0} {1}' in warehouse '{2}' quantity available for shipment will go negative.";
        public const string StatusCheck_QtyLocationNegative = "!Updating data for item '{0} {1}' on warehouse '{2} {3}' will result in negative available quantity.";
		public const string StatusCheck_QtyLotSerialNegative = "Updating item '{0} {1}' in warehouse '{2} {3}' lot/serial number '{4}' quantity available will go negative.";
		public const string StatusCheck_QtyOnHandNegative = "Updating item '{0} {1}' in warehouse '{2}' quantity on hand will go negative.";
		public const string StatusCheck_QtyLocationOnHandNegative = "Updating item '{0} {1}' in warehouse '{2} {3}' quantity on hand will go negative.";
		public const string StatusCheck_QtyLotSerialOnHandNegative = "Updating item '{0} {1}' in warehouse '{2} {3}' lot/serial number '{4}' quantity on hand will go negative.";
        public const string StatusCheck_QtySerialNotSingle = "Updating the quantity of the item '{0}' with serial number '{1}' will result in a quantity on hand for this serial number greater than 1.";
		public const string StatusCheck_QtyCostImblance = "Updating item '{0} {1}' in warehouse '{2}' caused cost to quantity imbalance.";
		public const string EmptyAutoIncValue = "Auto-Incremental value is not set in {0}.";
		public const string LotSerTrackExpirationInvalid = "Only classes with enabled Track Expiration can use Expiration Issue Method.";
		public const string LotSerClass = "Lot/Serial Class";
		public const string InventoryItem = "Inventory Item";
		public const string StartDateMustBeLessOrEqualToTheEndDate = "Start date must be less or equal to the end date.";
        public const string ValMethodCannotBeChanged = "Valuation method cannot be changed from '{0}' to '{1}' while stock is not zero.";
		public const string ValMethodChanged = "Valuation method changed but cost history is not updated.";
		public const string ThisItemClassCanNotBeDeletedBecauseItIsUsedIn = "This Item Class can not be deleted because it is used in {0}.";
		public const string TotalPctShouldBe100 = "Total % should be 100%";
		public const string ThisValueShouldBeBetweenP0AndP1 = "This value should be between {0} and {1}";
		public const string PercentageValueShouldBeBetween0And100 = "Percentage value should be between 0 and 100";
		public const string SpecificOnlyNumbered = "Specific valuated items should be lot or serial numbered during receipt.";
		public const string InsuffQty_LineQtyUpdated = "Insufficient quantity available. Line quantity was changed to match.";
		public const string SerialItem_LineQtyUpdated = "Invalid quantity specified for serial item. Line quantity was changed to match.";
		public const string SerialItemAdjustment_LineQtyUpdated = "Serialized item adjustment can be made for zero or one '{0}' items. Line quantity was changed to match.";
		public const string SerialItemAdjustment_UOMUpdated = "Serialized item adjustment can be made for zero or one '{0}' items. UOM was changed to match.";
		public const string StatusMayNotBeSetManually = "{0} Status May Not Be Set Manually";
		public const string SiteLocationOverride = "Update default location for all items on this site by selected location?";

		public const string Availability_Info = "On Hand {1} {0}, Available {2} {0}, Available for Shipping {3} {0}";		
		public const string PICountInProgress = "Physical count '{0}' in progress in this warehouse/location.";
        public const string PICountInProgressDuringRelease = "Physical count in progress for {0} in warehouse '{1} {2}'";
		public const string InventoryShouldBeUsedInCurrentPI = "Combination of selected Inventory Item and Warehouse Location is not allowed for this Physical Count.";
		public const string InventoryLocationPairShouldBeUsedInCurrentPI = "Selected combination of Inventory Item and Warehouse Location is not included into this Physical Count.";
		public const string ThisCombinationIsUsedAlready = "This Combination Is Used Already in Line Nbr. {0}";
		public const string ThisSerialNumberIsUsedAlready = "This  Serial Number Is Used Already in Line Nbr. {0}";
		public const string ThisSerialNumberIsUsedInItem = "This Serial Number Is Used Already for the item";
		public const string ThisSerialNumberNotMoved = "This Serial Number Is Used Already for the item and should be moved from previous location in Line Nbr. {0}";
		public const string PICreateAbjustment = "Unable to create adjustment for line '{0}'. Insufficient Qty. On Hand for item '{1} {2}' in warehouse '{3} {4}'";
		public const string ConfirmationXRefUpdate = "Substitute previous cross references information?";
		public const string AlternatieIDNotUnique = "Value '{0}' for Alternate ID is already used for another inventory item.";
		public const string FractionalUnitConversion = "Fractional unit conversions not supported for serial numbered items";
		public const string SiteUsageDeleted = "Unable to delete warehouse, item {0} is usage";
		public const string ItemLotSerClassVerifying = "Lot serial class cannot be changed when its tracing method not compatible with previous class and item is use.";
		public const string SerialNumberAlreadyIssued = "Serial Number '{1}' for item '{0}' already issued.";
        public const string SerialNumberAlreadyIssuedIn = "Serial Number '{1}' for item '{0}' already issued in '{2}'.";
        public const string SerialNumberAlreadyReceived = "Serial Number '{1}' for item '{0}' is already received.";
        public const string SerialNumberAlreadyReceivedIn = "Serial Number '{1}' for item '{0}' is already received in '{2}'.";
        public const string SerialNumberDuplicated = "Duplicate serial number '{1}' for item '{0}' is found in document.";
		public const string NumericLotSerSegmentNotExists = "'{0}' segment must be defined for lot/serial class.";
		public const string NumericLotSerSegmentMultiple = "Multiple '{0}' segments defined for lot/serial class.";
		public const string SumOfAllComponentsMustBeHundred = "Total Percentage for Components must be 100. Please correct the percentage split for the components.";
		public const string ItemClassChangeWarning = "Please confirm if you want to update current Item settings with the Inventory Class defaults. Original settings will be preserved otherwise.";
		public const string MissingUnitConversion = "Unit conversion is missing.";
		public const string MissingUnitConversionVerbose = "Unit conversion {0} is missing.";
		public const string DfltQtyShouldBeBetweenMinAndMaxQty = "Component Qty should be between Min. and Max. Qty.";
		public const string KitMayNotIncludeItselfAsComponentPart = "Kit May Not Include Itself As Component Part";
		public const string IssuesAreNotAllowedFromThisLocationContinue = "Issues are not allowed from this Location. Continue ?";
		public const string NonStockKitAssemblyNotAllowed = "Non-Stock Kit Assembly is not allowed.";
		public const string LSCannotAutoNumberItem = "Cannot generate the next lot/serial number for item {0}.";
		public const string LocationCostedWarning = "There is non zero Quantity on Hand for this item on selected Warehouse Location. You can only change Cost Separately option when the Qty on Hand is equal to zero";
		public const string LocationCostedSetWarning = "Last Inventory cost on warehouse will not be updated if the item has been received on this Warehouse Location.";
		public const string NotDefaultSubItemSegment = "Inventory SubItem can only be switched off for the Default Subitem segment.";
		public const string PeriofNbrCanNotBeGreaterThenInSetup = "Period Number can not be greater then Turnover Periods per Year on the InSetup.";
		public const string PossibleValuesAre = "Possible Values are: 1,2,3,4,6,12.";
		public const string NonStockItemExists = "This ID is already used for another Non-Stock Item.";
		public const string StockItemExists = "This ID is already used for another Stock Item.";
		public const string QtyOnHandExists = "There is non zero Quantity on Hand for this item. You can only change Cost when the Qty on Hand is equal to zero";
		public const string PILineDeleted = "Unable to delete line, just manually added line can be deleted.";
		public const string PIEmpty = "Cannot generate the physical inventory count. List of details is empty.";
		public const string PIPhysicalQty = "Serial-numbered items should have physical quantity only 1 or 0.";
		public const string BinLotSerialNotAssigned = "One or more lines have unassigned Location and/or Lot/Serial Number";
		public const string AdjstmentCreated = "Adjustment '{0}' created.";
		public const string SingleRevisionForNS = "Non-Stock kit can contain only one revision.";
		public const string RestictedSubItem = "Subitem status restricts using it for selected site.";
		public const string CantGetPrimaryView = "Can't get the primary view type for the graph {0}";
		public const string UnknownSegmentType = "Unknown segment type";
		public const string TooShortNum = "Lot/Serial Number must be {0} characters long";
		public const string UnableNavigateDocument = "Unable to navigate on document.";
		public const string ReplenihmentPlanDeleted = "Proccess recpneishment for zero quantity just delete previous plan.";
		public const string ReceiptAddedForPO = "Item {0}{1} receipted {2} {3} for Purchase Order {4}";
		public const string PILineUpdated = "Item {0}{1} updated physical quantity {2} {3} line {4}.";
		public const string ConversionNotFound = "Unit Conversion is not setup on 'Units Of Measure' screen. Please setup Unit Conversion FROM {0} TO {1}.";
		public const string BoxesRequired = "At least one box must be specified in the Boxes grid for the given packaging option.";
		public const string PeriodHasINDocsFromPO_LCToBeCreated = "There one or more pending IN Adjustment originating from the existing Landed Cost transaction in PO module which belong to this period. They have to be created and released before the period may be closed in IN. Please, check the screen 'Process Landed Cost'(PO.50.60.00)";
		public const string PeriodHasINDocsFromAP_LCToBeCreated = "There one or more pending IN Adjustment originating from the existing Landed Cost transaction in AP module which belong to this period. They have to be created and released before the period may be closed in IN. Please, check the screen 'Process Landed Cost'(AP.50.65.00)";
		public const string ReplenishmentSourceSiteMustBeDifferentFromCurrenSite = "Replenishment Source Warehouse must be different then current Warehouse";
        public const string InactiveWarehouse = "Warehouse '{0}' is inactive";
        public const string SubitemDeleteError = "You cannot delete Subitem because it is already in use.";
	    public const string CantDeactivateSite = "Can't deactivate warehouse. It has unreleased transactions.";
		public const string PeriodsOverlap = "Periods overlap.";
		public const string UnableImportSeasonsWithOverlapedPeriods = "Period {0} - {1} is overlapping with another season. Import could not be completed.";
		public const string SeasonInvalidDate = "String '{0}' could not be processed as a valid datetime.";
		public const string ItemCannotPurchase = "Item cannot be Purchased";
		public const string ItemCannotSale = "Item cannot be Sold";
		public const string ValueIsRequiredForAutoPackage = "Value is required for Auto packaging to work correctly.";
		public const string MaxWeightIsNotDefined = "Box Max. Weight must be defined for Auto Packaging to work correctly.";
		public const string MaxVolumeIsNotDefined = "Box Max. Volume must be defined for Auto Packaging to work correctly.";
        public const string MaxQtyIsNotDefined = "Box Max. Quantity must be specified for Auto Packaging to work correctly.";
        public const string OneOrMoreExpDateIsEmpty = "One or more Bin/Lot/Serial have empty expiration date";
		public const string BaseSalesPriceDelete = "You should not delete Base Price.";
		public const string ItemDontFitInTheBox = "The item can't fit the given Box.";
        #endregion

		#region Translatable Strings used in the code
		public const string LS = "Lot/Serial";
		public const string ClearSettings = "Clear Settings";
		public const string Multiple = "<MULTIPLE>";
		public const string Unassigned = "<UNASSIGNED>";
		public const string ExceptLocationNotAvailable = "[*]  Except Location Not Available";
		public const string ExceptExpiredNotAvailable = "[**] Except Expired and  Loc. Not Available";
		public const string EstimatedCosts = "[*]  Estimated Costs";
		public const string CustomerID = "Customer ID";
		public const string Customer = "Customer";
		public const string CustomerName = "Customer Name";
		public const string Contact = "Contact";
		public const string ReceiptType = "Receipt Type";
		public const string ReceiptNbr = "Receipt Nbr.";
		public const string ExpireDateLessThanStartDate = "Expire Date must be greater than Start Date";
		public const string ProductionVarianceTranDesc = "Production Variance";
		public const string SeasonalSettingsAreOverlaped = "Seasonal settings are not defined correctly (overlap detected)";
		public const string AttemptToComparePeriodsOfDifferentType = "Period of different types can not be compared";
		public const string ThisTypeOfForecastModelIsNotImplemetedYet = "The model type {0} is not implemented yet";
		public const string InternalErrorSequenceIsNotSortedCorrectly = "InternalError: Sequence's  sorting order is wrong or it's not sorted";
		#endregion

		#region Graph Names
		public const string INUnitMaint = "Inventory Unit Maintenance";
		public const string INItemClassMaint = "Item Class Maintenance";
		public const string NonStockItemMaint = "Non-Stock Items Maintenance";
		public const string InventoryItemMaint = "Inventory Items Maintenance";
		public const string INItemSiteMaint = "Inventory Item Warehouse Detail";
		public const string INSiteMaint = "Warehouse Maintenance";
		public const string INReceiptEntry = "Receipt Entry";
		public const string INIssueEntry = "Issue Entry";
		public const string INAdjustmentEntry = "Adjustment Entry";
		public const string INTransferEntry = "Transfer Entry";
		public const string INDocumentRelease = "Release IN Documents";
		public const string InventorySummaryEnq = "Inventory Summary Enquiry";
		public const string INPostClassMaint = "Posting Class Maintenance";
		public const string INLotSerClassMaint = "Lot/Serial Class Maintenance";
        public const string INSetup = "Iventory Preferences";
        public const string INSetupMaint = "IN Setup";
		public const string InventoryAllocDetEnq = "Inventory Allocation Detail Inquiry";
		public const string InventoryTranDetEnq = "Inventory Transaction Detail Inquiry";
		public const string InventoryTranSumEnq = "Inventory Transaction Summary Inquiry";
		public const string InventoryTranHistEnq = "Inventory Transaction History Inquiry";
		public const string InventoryTranByAcctEnq = "Inventory Transaction By Account Inquiry";
		public const string InventoryLotSerInq = "Inventory Lot/Serial Inquiry";
		public const string INABCCodeMaint = "ABC Code Maintenance";
		public const string INMovementClassMaint = "Movement Class Maintenance";
		public const string INPICycleMaint = "PI Cycle Maintenance";
		public const string INPriceClassMaint = "Inventory Price Class Maintenance";
		public const string INUpdateBasePrice = "Update Base Price";
		public const string PIGenerator = "PI Tags Generator";
		public const string INPIEntry = "Physical Inventory Entry";
		public const string INPIReview = "Physical Inventory Review";
		public const string INUpdateABCAssignment = "Update ABC Assignments";
		public const string INUpdateMCAssignment = "Update Movement Class Assignments";
		public const string INReplenishmentClassMaint = "Replenishment Class";
		public const string INAccess = "Warehouse Access";
		public const string INAccessItem = "Inventory Item Access";
		public const string INAccessDetail = "Warehouse Access Detail";
		public const string INAccessDetailItem = "Inventory Item Access Detail";
		public const string INAccessDetailClass = "Item Class Access Detail";
		public const string INKitSpecMaint = "Kit Specification Maintenance";
		public const string KitAssemblyEntry = "Kit Assembly Entry";
		public const string KitSubstitutionIsRestricted = "Manual Component substitution is not allowed by the Kit specification.";
		public const string KitQtyVarianceIsRestricted = "Quantity is dictated by the Kit specification and cannot be changed manualy for the given component.";
		public const string KitQtyOutOfBounds = "Quantity is out of bounds. Specification dictates that it should be within [{0}-{1}] {2}.";
		public const string KitQtyNotEvenDistributed = "Quantity of Components is not valid. Quantity must be such that it can be uniformly distributed among the kits produced.";
		public const string KitItemMustBeUniqueAccrosSubItems = "Component Item must be unique for the given Kit accross Component ID and Subitem combinations.";
		public const string KitItemMustBeUnique = "Component Item must be unique for the given Kit.";		
		public const string ReplenishmentPolicyMaint = "Inventory Replenishment Seasonality";
		public const string INReplenishmentCreate = "Inventory Replenishment Create";
        public const string EquipmentMaint = "Equipment Maintenance";
        public const string CategoryMaint = "Category Maintenance";
        public const string UsingKitAsItsComponent = "Non-stock kit can't using as its own component";
        public const string SNComponentInSNKit = "Serial-numbered components are allowed only in serial-numbered kits";
		#endregion

		#region Cache Names
		public const string Warehouse = "Warehouse";
		public const string ItemClass = "Item Class";
		public const string Warranty = "Warranty";
		public const string Equipment = "Equipment";
		public const string Register = "Receipt";
		public const string INSite = "Warehouse";
		public const string ItemWarehouseSettings = "Item/Warehouse Settings";
		public const string PostingClass = "Posting Class";
		public const string KitSpecification = "Kit Specification";
		public const string ReplenishmentPolicy = "Replenishment Policy";
        public const string INSubItem = "IN Sub Item";
        public const string InventoryUnitConversions = "Inventory Unit Conversions";
        public const string DeferredRevenueComponents = "Deferred Revenue Components";
        public const string ItemCostStatistics = "Item Cost Statistics";
	    public const string ItemReplenishmentSettings = "Item Replenishment Settings";
        public const string SubitemReplenishmentSettings = "Subitem Replenishment Settings";
	    public const string XReferences = "Cross-Reference";
        public const string INComponentTran = "IN Component";
        public const string INOverheadTran = "IN Overhead";
        public const string INTran = "IN Transaction";
        public const string INComponentTranSplit = "IN Component Split";
        public const string INKitTranSplit = "IN Kit Split";
        public const string INTranSplit = "IN Transaction Split";
		#endregion

		#region View Names

		public const string ServiceCalls = "Service Calls";

		#endregion

		#region Combo Values

        public const string ModulePI = "PI";

		#region Inventory Mask Codes
		public const string MaskItem = "Inventory Item";
		public const string MaskSite = "Warehouse";
		public const string MaskClass = "Posting Class";
		public const string MaskReasonCode = "Reason Code";
        public const string MaskVendor = "Vendor";
		#endregion

		#region Item Types
		public const string NonStockItem = "Non-Stock Item";
		public const string LaborItem = "Labor";
		public const string ServiceItem = "Service";
		public const string ChargeItem = "Charge";
		public const string ExpenseItem = "Expense";

		public const string FinishedGood = "Finished Good";
		public const string Component = "Component Part";
		public const string SubAssembly = "Subassembly";
		#endregion
	
		#region Valuation Methods
		public const string Standard = "Standard";
		public const string Average = "Average";
		public const string FIFO = "FIFO";
		public const string Specific = "Specific";
		#endregion

		#region Lot Serial Assignment
		public const string WhenReceived = "When Received";
		public const string WhenUsed = "When Used";
		#endregion

		#region Lot Serial Tracking
		public const string NotNumbered = "Not Tracked";
		public const string LotNumbered = "Track Lot Numbers";
		public const string SerialNumbered = "Track Serial Numbers";
		#endregion

		#region Lot Serial Issue Method
		public const string LIFO = "LIFO";
		public const string Sequential = "Sequential";
		public const string Expiration = "Expiration";
		public const string UserEnterable = "User-Enterable";
		#endregion

		#region Lot Serial Segment Type
		public const string NumericVal = "Auto-Incremental Value";
		public const string FixedConst = "Constant";
		public const string DayConst = "Day";
		public const string MonthConst = "Month";
		public const string MonthLongConst = "Month Long";
		public const string YearConst = "Year";
		public const string YearLongConst = "Year Long";
		public const string DateConst = "Custom Date Format";
		#endregion

		#region Transaction / Journal Types
		public const string Assembly = "Assembly";
		public const string Receipt = "Receipt";
		public const string Issue = "Issue";
		public const string Return = "Return";
		public const string Invoice = "Invoice";
		public const string DebitMemo = "Debit Memo";
		public const string CreditMemo = "Credit Memo";
		public const string Transfer = "Transfer";
		public const string Adjustment = "Adjustment";
		public const string Undefined = "Not Used in Inventory";
		public const string StandardCostAdjustment = "Standard Cost Adjustment";
		public const string NegativeCostAdjustment = "Negative Cost Adjustment";
		public const string ReceiptCostAdjustment = "Receipt Cost Adjustment";
		public const string NoUpdate = "No Update";
		public const string Production = "Production";
		public const string Change = "Change";
		public const string Disassembly = "Disassembly";
		public const string DropShip = "Drop-Shipment";

		#endregion

		#region Transfer Types
		public const string OneStep = "1-Step";
		public const string TwoStep = "2-Step";
		#endregion		

		#region Item Status
		public const string Active = "Active";
		public const string NoSales = "No Sales";
		public const string NoPurchases = "No Purchases";
		public const string NoRequest = "No Request";
		public const string Inactive = "Inactive";
		public const string ToDelete = "Marked for Deletion";
		#endregion

		#region Qty Allocation Doc Type
		public const string qadINTransfer = "IN Transfer";
		public const string qadINReceipt = "IN Receipt";
		public const string qadINIssue = "IN Issue";
		public const string qadPOOrder = "PO Order";
		public const string qadPOReceipt = "PO Receipt";
		public const string qadSOOrder = "SO Order";
		public const string qadSOShipment = "SO Shipment";
		public const string qadSOInvoice = "SO Invoice";
		#endregion

		#region Document Status
		public const string Hold = "On Hold";
		public const string Balanced = "Balanced";
		public const string Released = "Released";

		// some additional statuses for PIHeader
		public const string Counting = "Counting In Progress";
		public const string DataEntering = "Data Entering";
		public const string Completed = "Completed";
		public const string Cancelled = "Canceled";

		// some additional statuses for PIDetail
		public const string NotEntered = "Not Entered";
		public const string Entered = "Entered";
		public const string Voided = "Voided";
		public const string Skipped = "Skipped";

		// LineType for PIDetail		
		//public const string Normal = "Normal"; // [Normal] is used from layer type
		public const string Blank = "Blank";
		public const string UserEntered = "UserEntered";

		// some additional statuses for PICountStatus
		// public const string InProgress = "In Progress"; // defined above
		public const string Available = "Available";
		public const string InProgress = "In Progress";
		public const string NotAvailable = "Not Available";

		#endregion

		#region Primary Item Validation
		public const string PrimaryNothing = "No Validation";
		public const string PrimaryItemError = "Primary Item Error";
		public const string PrimaryItemClassError = "Primary Item Class Error";
		public const string PrimaryItemWarning = "Primary Item Warning";
		public const string PrimaryItemClassWarning = "Primary Item Class Warning";
        #endregion

		#region Location Validation Types
		public const string LocValidate = "Do Not Allow On-the-Fly Entry";
		public const string LocNoValidate = "Allow On-the-Fly Entry";
		public const string LocWarn = "Warn But Allow On-the-Fly Entry";
		#endregion

		#region Alternate Types
		public const string CPN = "Customer Part Number";
		public const string VPN = "Vendor Part Number";
		public const string Global = "Global";
		public const string Barcode = "Barcode";
		public const string Substitute = "Substitute";
		public const string Obsolete = "Obsolete";
		#endregion

		#region Layer Types
		public const string Normal = "Normal";
		public const string Oversold = "Oversold";
		#endregion

		#region Physical Inventory Types
		public const string ByInventory = "By Inventory";
		public const string ByLocation = "By Location";
		#endregion

		#region PrimaryItemValidationType

		public const string Warning = "Warning";
		public const string Error = "Error";

		#endregion


		#region INPriceOption

		public const string Percentage = "Percentage";
		public const string FixedAmt = "Fixed Amount";


		#endregion

		#region INReplenishmentType
		public const string None = "None";
		public const string MinMax = "Min./Max.";
		public const string FixedReorder = "Fixed Reorder Qty";
		#endregion

		#region INReplenishmentSource
		public const string Purchased = "Purchase";
		public const string Manufactured = "Manufacturing";
		public const string PurchaseToOrder = "Purchase to Order";
		public const string TransferToOrder = "Transfer to Order";    
		#endregion

		#region Cost Source
		public const string AverageCost = "Average";
		public const string LastCost = "Last";
		#endregion

		#region PackageOption
		public const string Weight = "By Weight";
		public const string Quantity = "By Quantity";
		public const string WeightAndVolume = "By Weight & Volume";
		public const string Manual = "Manual"; 
		#endregion

		#region Demand Period Type
		public const string Month = "Month";
		public const string Week = "Week";
		public const string Day = "Day";
		public const string Quarter = "Quarter";
		
		#endregion

		#region DemandForecastModelType
		public const string DFM_None = "None";
		public const string DFM_MovingAverage = "Moving Average";
		public const string DFM_ExponentialSmoothing = "Exponential Smoothing";
		public const string DFM_LinearRegression = "Linear Regression";
		public const string DFM_ExponentialSmoothingWithTrend = "Exponential Smoothing With Trend";
		public const string DFM_ExponentialSmoothingSeasonal = "Exponential Smoothing Seasonal";

		#endregion

		#endregion

		#region Custom Actions
		public const string Release = "Release";
		public const string Process = "Process";
		public const string ProcessAll = "Process All";
		public const string ViewInventoryAllocDet = "Inventory Allocation Details";
		public const string ViewInventoryTranDet = "Inventory Transaction Details";
		public const string INEditDetails = "Inventory Edit Details";
		public const string INRegisterDetails = "Inventory Register Detailed";
		public const string INItemLabels = "Inventory Item Labels";
		public const string INLocationLabels = "Location Labels";
		public const string ViewDocument = "View Document";
		public const string GeneratePI = "Generate PI";
		public const string FinishCounting = "Finish Counting";
		public const string CancelPI = "Cancel PI";
		public const string CompletePI = "Complete PI";
		public const string InventorySummary = "Summary";
		public const string InventoryAllocDet = "Allocation Details";
		public const string InventoryTranSum = "Transaction Summmary";
		public const string InventoryTranHist = "Transaction History";
		public const string InventoryTranDet = "Transaction Details";
		public const string SetNotEnteredToZero = "Set Not Entered To Zero";
		public const string SetNotEnteredToSkipped = "Set Not Entered To Skipped";
		public const string UpdateCost = "Update Actual Cost";
		public const string BinLotSerial = "Bin/Lot/Serial";
		public const string Generate = "Generate";
		public const string Add = "Add";
        public const string AddNewLine = "Add New Line";
        public const string ViewRestrictionGroup = "Group Details";
		public const string Calculate = "Calculate";
		public const string Clear = "Clear";

		#endregion

		#region PI Generation Sort Order Combos
		public const string ByLocationID = "By Location";
		public const string ByInventoryID = "By Inventory ID";
		public const string BySubItem = "By Subitem";
		public const string ByLotSerial = "By Lot/Serial Number";
		public const string ByInventoryDescription = "By Inventory Description";
		#endregion

		#region PI Generation Methods
		public const string FullPhysicalInventory = "Full Physical Inventory";
		public const string ByCycleCountFrequency = "By Cycle Count Frequency";
		public const string ByMovementClassCountFrequency = "By Movement Class Count Frequency";
		public const string ByABCClassCountFrequency = "By ABC Code Count Frequency";
		public const string MovementClassTolerance = "Outside Movement Class Tolerance";
		public const string ABCClassTolerance = "Outside ABC Code Tolerance";
		public const string ByCycleID = "By Cycle";
		public const string LastCountDate = "Last Count On Or Before";
		public const string ByPreviousPIID = "By Previous Physical Count";
		public const string ByItemClassID = "By Item Class";
		public const string ListOfItems = "List Of Items";
		public const string RandomlySelectedItems = "Random Items (up to)";
		public const string ItemsHavingNegativeBookQty = "Items Having Negative Book Qty.";
		public const string ByMovementClass = "By Movement Class";
		public const string ByABCClass = "By ABC Code";
		public const string ByCycle = "By Cycle";		

		#endregion

	}
}
