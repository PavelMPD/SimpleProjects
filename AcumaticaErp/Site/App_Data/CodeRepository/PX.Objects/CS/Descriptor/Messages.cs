using System;
using PX.Data;
using PX.Common;

namespace PX.Objects.CS
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		// Add your messages here as follows (see line below):
		// public const string YourMessage = "Your message here.";

		#region Validation and Processing Messages
		public const string Prefix = "CS Error";
		public const string Entry_LE = "Entry must be less or equal {0}";
		public const string Entry_GE = "Entry must be greater or equal {0}";
		public const string Entry_LT = "Entry must be less {0}";
		public const string Entry_GT = "Entry must be greater {0}";
		public const string Entry_EQ = "Entry must be equal {0}";
		public const string Entry_NE = "Entry must not be equal {0}";
		public const string Entry_Multiple = "{0}, {1}, {2} etc.";
		public const string FieldReadOnly = "You are not allowed to change this field.";
		public const string StateIDMustBeUnique = "State/Region ID must be unique.";
		public const string StateNameCantBeEmpty = "State/Region Name cannot be empty.";
		public const string StateIDCantBeEmpty = "State/Region ID cannot be empty.";
		public const string SegmentHasChilds = "Segment '{0}' cannot be deleted as it has one or more override segments.";
		public const string SegmentIsNotLast = "Segment '{0}' cannot be deleted as it is not last segment.";
		public const string SegmentHasValues = "Segment '{0}' cannot be deleted as it has one or more segment values defined.";
        public const string SegmentNotOverridden = "Segment '{0}' is not overridden and cannot be deleted.";
		public const string DimensionIsEmpty = "Segmented key must have at least one segment.";
		public const string SameNumberingMask = "Start, End Number, Last and Warning Numbers must have the identical length and numbering mask.";
		public const string StartNumMustBeGreaterEndNum = "Start Number must be greater than the End Number.";
		public const string WarnNumMustBeLessEndNum = "Warning Number must be less than  the End Number.";
		public const string WarnNumMustBeGreaterStartNum = "Warning Number must be greater than the Start Number.";
		public const string LastNumMustBeLessEndNum = "Last Number must be less than the End Number.";
		public const string LastNumMustBeGreaterOrEqualStartNum = "Last Number must be greater than or equal to the Start Number-1.";
		public const string NewSymbolLength = "New Symbol length must not exceed Start or End Number length.";
		public const string ListOfInstallmentsComplete = "Installments Schedule is completed. The total percentage equal to 100%.";
		public const string NumberOfInstallmentsDoesntMatch = "The number of records for the installment schedule must match the value in the Number of Installments field.";
		public const string SumOfInstallmentsMustBe100 = "The total percentage for Installments Schedule must be equal to 100%.";
		public const string NumberingIDNull = "Numbering ID is null.";
		public const string CantAutoNumber = "Cannot generate the next number for the sequence.";
		public const string CantAutoNumberSpecific = "Cannot generate the next number for {0}.";
		public const string CantManualNumber = "Cannot generate the next number. Manual Numbering is activated for '{0}'";
		public const string WarningNumReached = "The numbering sequence is expiring.";
		public const string EndOfNumberingReached = "Cannot generate the next number for the sequence because it is expired.";
		public const string CountryNameCantBeEmpty = "Country Name cannot be empty.";
		public const string CountryIDCantBeEmpty = "CountryID cannot be empty.";
		public const string NumberingIDRequired = "Numbering ID must be specified if Auto Numbering is enabled for one of the Segments.";
		public const string NumberingIDRequiredCustom = "{0} - Numbering ID must be specified if Auto Numbering is enabled for one of the Segments.";
		public const string NumberingViolatesSegmentDef = "Auto Numbering format violates the segment format. Segmented Key: '{0}' Segment: '{1}'.";
		public const string NumberingIsUsedFailedDelete = "This Numbering ID is used in Segmented Key: '{0}' Segment: '{1}' and cannot be deleted.";
		public const string TermsIDEmpty = "TermsID cannot not be empty.";
		public const string TermsDiscountGreater = "Discount entered exceeds the discount specified for this terms.";
		public const string DescrEmpty = "Description cannot be empty.";
		public const string ValueNotInRange = "Value must be in the range of [1-31].";
		public const string DayToDayFrom = "Day To must be greater or equal then the Day From.";
		public const string ValueMustBeLessEqualDueDay = "Value must less or equal then the Due Day.";
		public const string ValueMustBeGreaterEqualDiscDay = "Value must greater or equal then a discount day.";
		public const string NumberOfInstalments0 = "Number of installments must be greater than 0.";
		public const string OptionCantBeSelected = "This option cannot be used with the selected Due Date Type.";
		public const string OptionFixedNumberOfDays = "Only Fixed Number of Days option can be used with the selected Due Date Type.";
		public const string OptionEndOfMonth = "Only End of Month, Day of the Month and Day of Next Month options can be used with the selected Due Date Type.";
		public const string OptionDayOfTheMonth = "Only Day of the Month option can be used with the selected Due Date Type.";
		public const string MappedSegValueLength = "Total mapped segment value length should have {0} symbols.";
        public const string NumberingIDCannotBeUsedWithSegment = "Numbering ID cannot be used with '{0}' segment";
		public const string NumberingIDCannotBeUsedWithSegmentCustom = "{0} - Numbering ID '{1}' cannot be used with '{2}' segment";
        public const string EnsureSegmentLength = ": Ensure the segment's length matches the length of the numbering.";
        public const string EnsureSegmentMask = ": Ensure the segment's mask allows numerics.";
        public const string SegmentLengthLimit = "Total segment length cannot exceed 30 symbols.";
		public const string SegmentHasValuesFailedUpdate = "Segment '{0}' has values and cannot be updated.";
		public const string SegmentHasChildsFailedUpdate = "Segment '{0}' has override segments and cannot be updated.";
		public const string SegmentValueExistsType = "Segment value already exists.";
		public const string AccountTypeWarn = "Account Type is not {0}.";
		public const string ItemExistsReenter = "Item exists, please enter it again.";
		public const string MaskSourceMissing = "{0} {1} is missing.";
		public const string MaskSourceMissing2 = "{0} '{2}' {1} is missing.";
		public const string FieldShouldBePositive = "'{0}' should be positive.";
		public const string FieldShouldNotBeNegative = "'{0}' should not be negative.";
		public const string ReferencedByEmployeeClass = "This record cannot be deleted because it is referenced by Employee Class - {0}";
		public const string ReferencedByCaseClass = "This record cannot be deleted because it is referenced by Case Class - {0}";
		public const string ReferencedByCarrier = "This record cannot be deleted because it is referenced by Carrier - {0}";
		public const string ReferencedByContract = "This record cannot be deleted because it is referenced by Contract - {0}";
		public const string ReferencedByEmployee = "This record cannot be deleted because it is referenced by Employee - {0}";
		public const string InvalidMask = "Invalid mask specified in segment {0} for {1}.";
		public const string ImportSettings = "Import Settings from Plug-in";
		public const string FailedToCreateCarrierPlugin = "Failed to Create Carrier Plug-in. Please check that Plug-In(Type) is valid Type Name. {0}";
		public const string FailedToFindCarrierPlugin = "Failed to Find Carrier Plug-in with the given ID - {0}";
		public const string Warning = "Warning";
		public const string SegmentHasValuesQuestion = "Segment '{0}' has one or more segment values defined. Would you like to delete them?";
		public const string ConversionForCarrierUOMNotFound = "Unit Conversion is not setup , typeof(Select<PM.PMSetup>)for the Carrier UOM. Please setup Unit Conversion FROM {0} TO {1}.";
		public const string GroupUpdateConfirm = "Restriction Groups will be reset for all Items that belongs to this item class.  This might override the custom settings. Please confirm your action";
		public const string StartDateNotUnique = "Start Date is not unique";
		public const string OneFieldMustBeFilled = "Either New Number Symbol or Manual Numbering should be set";
		public const string IncorrectFromDate = "'From' date cannot be less than 'To' date.";
		public const string CannotBeEmpty = "Cannot be empty.";
		public const string BranchNotEnoughRights = "You don't have enough rights on branch '{0}'";
		public const string BranchCantBeDeactivated = "Branch cannot be deactivated.";
		public const string NotificationSourceArgument = "Template and report must be defined in same class.";
		public const string ErrorFormat = "Specified format isn't supported for this type notification.";
		public const string ValidateAddress = "Validate Address";
		public const string ValidateAddresses = "Validate Addresses";
		public const string UnknownErrorOnAddressValidation = "An unknown error has happen during address validation";
		public const string AddressVerificationServiceCreationErrorHTTP = "Address verification Service {0} is failed to be created";
		public const string AddressVerificationServiceCreationError = "Address verification Service {0} is failed to be created";
		public const string AddressVerificationServiceIsNotActive = "The Address Verification Service (AVS) configured for the country '{0}' is not active";
		public const string AddressVerificationServiceIsNotSetup = "The Address Verification Service (AVS) is not configured for the country '{0}'";
		public const string AddressVerificationServiceReturnsField = "AVS Returns: '{0}'";
		public const string ShipViaFK = "Carrier cannot be deleted. One or more Ship Via is depends on this Carrier.";
		public const string AvalaraAVSUnknownError = "Avalara AVS: Unknown Error";
		public const string RefreshSettings = "Refresh Component List";
		public const string FeaturesUsageWarning = "This feature is in use, disabling it may cause unexpected results.";
        public const string SegmentedKeyAddingNewSegmentRequiresUpdating = "Adding new segment may require updating related exisiting records.";
        public const string SubaccountAddingNewSegmentRequiresUpdating = "Adding new Subaccount segment will require updating all existing Subaccounts through the GL>Configuration>Subaccounts screen.";
        public const string InSubitemDuplicate = "Segment can not be deleted. Deleteing segment will cause duplicate subitems.";
		#endregion //Validation and Processing Messages

		#region Graph Names
		public const string ReasonCodeMaint = "Reason Codes Maintenance";
		public const string RMReportMaint = "ARM Reports Maintenance";
		public const string RMColumnSetMaint = "ARM Column Sets Maintenance";
		public const string RMRowSetMaint = "ARM Row Sets Maintenance";
		public const string RMUnitSetMaint = "ARM Unit Sets Maintenance";
		public const string CSCalendarMaint = "Calendars Maintenance";
		public const string CSFilterMaint = "System Filters Maintenance";
		public const string CSDeferredCodeMaint = "Deferred Codes Maintenance";
		public const string CompanyLocationMaint = "Company Locations Maintenance";
		public const string CompanySetup = "Company Setup";
		public const string BranchMaint = "Company Branches";
		public const string DimesionMaint = "Segmented Keys Maintenance";
		public const string SegmentMaint = "Segmented Values Maintenance";
		public const string SalesTaxMaint = "Sales Tax Maintenance";
		public const string TaxCategoryMaint = "Tax Category Maintenance";
		public const string TaxZoneMaint = "Tax Zone Maintenance";
		public const string CarrierMaint = "Carriers Maintenance";
		public const string CarrierPluginMaint = "Carrier Plug-in Maintenance";
		public const string CountryMaint = "Countries Maintenance";
		public const string NumberingMaint = "Numbering Sequences Maintenance";
		public const string FOBPointMaint = "FOB Points Maintenance";
		public const string ShipTermsMaint = "Shipment Terms Maintenance";
		public const string ShippingZoneMaint = "Shipping Zone Maintenance";
		public const string TermsMaint = "Terms Maintenance";
		public const string PreferencesMaint = "Preferences Maintenance";
		public const string Country = "Country";
		public const string RMReportReader = "ARM Reports Reader";
		public const string CSAttributeMaint = "Attribute Maintenance";
		public const string CSBoxMaint = "Box Maintenance";
		public const string DaylightShiftMaint = "Daylight Saving Time Calendar Maintenance";
		#endregion //Graph Names

		#region Cache Names
		public const string Branch = "Branch Summary";
		public const string State = "State";
		public const string CSAnswers = "Answers";
		public const string DaylightShift = "Daylight Shift";
		public const string CalendarYear = "Calendar Year";
		public const string SegmentValue = "Segment Value";
		#endregion

		#region Custom Actions
		public const string ViewEmployee = "Employee Details";
		public const string NewEmployee = "New Employee";

		public const string ttipRenumber = "Renumber rows.";

		public const string AddToSiteMap = "Add To Site Map";
		public const string ttipAddToSiteMap = "Add this report to site map.";
		public const string ViewSegment = "View Segment";
		public const string CopyUnitSet = "Copy Unit Set";
		//public const string Up = "Up";
		//public const string Down = "Down";
		//public const string Left = "Left";
		//public const string Right = "Right";

		#endregion

		#region Translatable Strings used in the codes
		public const string All = "All";
		public const string Vendor = "Vendors";
		public const string Customer = "Customers";
		public const string Disabled = "Disabled";
		public const string FixedNumberOfDays = "Fixed Number of Days";
		public const string DayOfNextMonth = "Day of Next Month";
		public const string EndOfMonth = "End of Month";
		public const string EndOfNextMonth = "End of Next Month";
		public const string DayOfTheMonth = "Day of the Month";
		public const string Prox = "Fixed Number of Days starting Next Month"; 
		public const string Custom = "Custom";
		public const string Weekly = "Weekly";
		public const string Monthly = "Monthly";
		public const string SemiMonthly = "Semi-Monthly";
		public const string Single = "Single";
		public const string Multiple = "Multiple";
		public const string EqualParts = "Equal Parts";
		public const string AllTaxInFirst = "Tax in First Installment";
		public const string SplitByPercents = "Split by Percent in Table";
		public const string NoNumberNewSymbol = "<SELECT>";
		public const string OperatorAnd = "And";
		public const string OperatorOr = "Or";
		public const string WorkDay = "Work Day:";
		public const string WorkingHours = "Working Hours:";
		public const string GoodsAreMoved = "Goods Are Moved:";

		public const string ttipCopyStyle = "Copies current style record";
		public const string ttipPasteStyle = "Pastes copied style in current style record";
		public const string MoveToExternalNode = "Move to external node.";
		public const string MoveToInternalNode = "Move to internal node.";
		public const string MoveUp = "Move Up";
		public const string MoveDown = "Move Down";

		public const string CodeNotInt = "The 'Code' column contains one or more non-integer values";
		public const string RowCodeExists = "The record with the same 'Code' field value already exists.";
		public const string SIUnits = "SI Units (Kilogram)";
		public const string USUnits = "US Units (Pound)";
		public const string GroupMultiple = "<MULTIPLE>";
		public const string CannotStartWithDigit = "Cannot start with digit.";
		public const string CannotContainEmptyChars = "Cannot contain empty chars.";
		public const string Date = "Date";
		#endregion //Translatable Strings used in the codes

		#region Deferred Code Type
		public const string Income = "Revenue";
		public const string Expense = "Expense";
		#endregion

		#region Deferred Method Type
		public const string EvenPeriods = "Even Periods";
		public const string ProrateDays = "Even Periods, Prorate days";
		public const string ExactDays = "Exact days per period";
		#endregion

		#region MultDiv Type
		public const string Multiply = "Multiply";
		public const string Divide = "Divide";
		#endregion
		
		#region ARmReport parameters
		//Title 
		public const string StartBranchTitle = "Start Branch :";
		public const string EndBranchTitle = "End Branch :";
		public const string BookCodeTitle = "Ledger :";
		public const string PeriodTitle = "Financial Period :";
		public const string StartPeriodTitle = "Start Financial Period :";
		public const string EndPeriodTitle = "End Financial Period :";
		public const string StartAccTitle = "Start Account :";
		public const string EndAccTitle = "End Account :";
		public const string StartSubTitle = "Start Sub :";
		public const string EndSubTitle = "End Sub :";
		public const string AccountClassTitle = "Account Class :";

		public const string StartAccountGroupTitle = "Start Account Group :";
		public const string EndAccountGroupTitle = "End Account Group :";
		public const string StartProjectTitle = "Start Project :";
		public const string EndProjectTitle = "End Project :";
		public const string StartTaskTitle = "Start Task :";
		public const string EndTaskTitle = "End Task :";
		public const string StartInventoryTitle = "Start Inventory :";
		public const string EndInventoryTitle = "End Inventory :";		
		#endregion

		#region RMReportPM&GL
		public const string NotSet = "Not Set";
		public const string Amount = "Amount";
		public const string Quantity = "Quantity";
		public const string AmountTurnover = "Amount Turnover";
		public const string QuantityTurnover = "Quantity Turnover";
		public const string BudgetAmount = "Budget Amount";
		public const string BudgetQuantity = "Budget Quantity";
		public const string RevisedAmount = "Revised Amount";
		public const string RevisedQuantity = "Revised Quantity";
		public const string BudgetPTDAmount = "Budget PTD Amount";
		public const string BudgetPTDQuantity = "Budget PTD Quantity";
		public const string RevisedPTDAmount = "Revised PTD Amount";
		public const string RevisedPTDQuantity = "Revised PTD Quantity";
		public const string Turnover = "Turnover";
		public const string Credit = "Credit";
		public const string Debit = "Debit";
		public const string BegBalance = "Beg. Balance";
		public const string EndingBalance = "Ending Balance";
		public const string AmountType = "Amount Type";
		#endregion

		#region Reason Codes related
		public const string ReasonCode = "Reason Code";
		public const string Sales = "Sales";
		public const string CreditWriteOff = "Credit Write-Off";
		public const string BalanceWriteOff = "Balance Write-Off";
		public const string CustomerLocation = "Customer Location";
		public const string Employee = "Employee";
		public const string Salesperson = "Salesperson";
		#endregion

		public static string GetLocal(string message)
		{
			return PXLocalizer.Localize(message, typeof(Messages).FullName);
		}
	}
}
