using System;
using PX.Data;
using PX.Common;

namespace PX.Objects.GL
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		// Add your messages here as follows (see line below):
		// public const string YourMessage = "Your message here.";
		//Messages
		#region Validation and Processing Messages
		public const string Prefix = "GL Error";
		public const string InterBranchFeatureIsDisabled = "Inter-Branch Transactions feature is disabled.";
		public const string SourceAccountNotSpecified = "You have to specify one or more source GL Accounts.";
		public const string DestAccountNotSpecified = "You have to specify one or more destination GL Accounts.";
        public const string AccountsNotInBaseCury = "One or more source GL accounts are denominated in foreign currencies. These GL accounts cannot be used for GL allocation.";
		public const string SumOfDestsMustBe100 = "The total percentage for destination GL Accounts must equal 100%.";
		public const string ListOfAllocDestComplete = "The total percentage for destination GL Accounts must equal 100%.";
		public const string PrevOperationNotCompleteYet = "The previous operation has not been completed.";
        public const string AllocsNotProcessed = "The system failed to process one or more GL allocations.";
		public const string UnpostedBatches = "There are un-posted GL Batches for this GL Allocation for the selected Financial Period. You must post these batches before executing this GL Allocation.";
		public const string AllocCantBeProcessed = "This GL Allocation cannot be processed. There are GL Allocations with higher priority that must be executed first.";
		public const string AmountCantBeDistributed = "GL Allocation cannot be completed. Allocated amount cannot be fully distributed to destination GL Accounts. Please verify the GL Allocation settings.";
		public const string NoPeriodsForNextYear = "Financial Year cannot be closed. The next Financial Year contains no open Financial Periods.";
		public const string PeriodOpenInAPModule = "Financial Period is open in AP Module.";
		public const string PeriodOpenInARModule = "Financial Period is open in AR Module.";
		public const string PeriodOpenInINModule = "Financial Period is open in IN Module.";
		public const string PeriodOpenInCAModule = "Financial Period is open in CA Module.";
        public const string PeriodOpenInFAModule = "Financial Period is open in FA Module.";
        public const string PeriodHasUnpostedBatches = "Selected financial period cannot be closed because one or more un-posted batches exist for this period.";
		public const string PeriodAlreadyUsed = "You cannot delete a Financial Period which has already been used.";
		public const string DeleteSubseqYears = "You cannot delete this Financial Year. The next Financial Year must be deleted first.";
		public const string YearHasMaxNumOfPeriods = "All Financial Periods for this Financial Year are configured.";
		public const string DataInconsistent = "Row cannot be inserted. Data is not inconsistent.";
		public const string DeleteSubseqPeriods = "Financial Period cannot be deleted. You must delete all Financial Periods after this Financial Period first.";
		public const string ConfigDataNotEntered = "You must configure the Financial Year Settings first.";
		public const string ConfigDataNotEnteredCMSetupAndFinancialYear = "Required configuration data are not entered into CM Setup and Financial Year.";
		public const string FiscalYearsHaveUnclosedPeriods = "One or more open Financial Periods exist. Financial Year Settings can only be changed if all the Financial Periods in the system are closed.";
        public const string FiscalPeriodsDefined = "You cannot delete a financial year Settings because one or more financial periods exist in the system.";
		public const string AllPeriodsAlreadyDefined = "All Financial Periods are configured.";
		public const string ModifyPeriodsInUserDefinedMode = "Modification of Financial Periods settings is only allowed when the Custom Number of Periods option is selected.";
		public const string NotAllFiscalPeriodsDefined = "Please configure all the Financial Periods for the Year to commit your changes.";
		public const string NoDateAdjustment = "Date adjustment is not implemented for the period type: {0}.";
		public const string NoOpenPeriod = "There are no open Financial Periods defined in the system.";
		public const string NoOpenPeriodAfter = "There are no open Financial Periods after {0} defined in the system.";
		public const string BatchStatusInvalid = "Batch Status invalid for processing.";
		public const string ImportStatusInvalid = "Status invalid for processing.";
		public const string AccountMissing = "One or more GL Accounts cannot be found.";
		public const string BrachAcctMapMissing = "Missing Account Mapping for Branch \"{0}\" and \"{1}\"";
		public const string BranchUsedWithLedger = "Ledger {0} is assigned to the branch.";
		public const string BranchUsedWithAccount = "Account {0} is assigned to the branch.";
		public const string BranchUsedWithSite = "Warehouse {0} is assigned to the branch.";
		public const string BranchUsedWithFixedAsset = "Account {0} is assigned to the branch.";
		public const string LedgerMissing = "One or more Ledgers cannot be found.";
		public const string BranchMissing = "One or more Branches cannot be found.";
		public const string CuryInfoMissing = "One or more CuryInfo records cannot be found.";
		public const string BatchOutOfBalance = "Batch is out of balance, please review.";
		public const string ScheduleTypeCantBeChanged = "Schedule Type cannot be changed for the processed Schedule.";
		public const string NoPeriodsDefined = "Financial Period cannot be found in the system.";
		public const string BudgetArticleNotFound = "Budget Article cannot be found in the system.";
		public const string BudgetTreeNodeNotFound = "Budget Tree node cannot be found in the system.";
		public const string InvalidField = "Invalid field: '{0}'.";
		public const string AccountCuryNotTransactionCury = "Denominated GL Account currency is different from transaction currency.";
		public const string AccountInactive = "Account is inactive.";
		public const string BranchInactive = "Branch is inactive.";
        public const string SubaccountInactive = "Subaccount is inactive.";
		public const string YearOrPeriodFormatIncorrect = "Invalid Year or Period format.";
		public const string AccountExistsType = "You cannot change the selected GL Account type because transactions for this GL Account already exist.";
		public const string AccountExistsCurrencyID = "You cannot change denomination currency for the selected GL Account because transactions for the existing currency id exist for this GL Account.";
		public const string AccountExistsHistory = "You cannot delete GL Account because transactions for this GL Account exist.";
        public const string CannotClearCurrencyInCashAccount = "You cannot delete denomination currency for selected Cash Account.";
		public const string AccountExistsHistory2 = "Transaction history for this GL Account exist.";
		public const string SubExistsHistory = "You cannot delete Subaccount because transactions for this Subaccount exist.";
		public const string LedgerExistsHistory = "You cannot delete Ledger because transactions for this Ledger exist.";
		public const string StartPeriodFormat = "Incorrect format provided for the Start Period.";
		public const string EndPeriodFormat = "Incorrect format provided for the End Period.";
		public const string EndPeriodLaterStartPeriod = "The End Period must be greater than the Start Period.";
		public const string ValueForWeight = "Weight value must be in the range of {0}-{1}.";
		public const string ValueForLimitPercent = "Limit Percent value must be in the range of 0-100.";
		public const string ValueForPercent = "Percent value must be positive.";
		public const string EndDateGreaterEqualStartDate = "Financial Period End Date must be greater or equal then the Start Date.";
		public const string BaseCuryIDCantBeChanged = "Base Currency ID cannot be changed.";
		public const string NoPostNetIncome = "Cannot post transactions directly to Year to Date Net Income.";
		public const string TranDateOutOfRange = "Financial Period for {0} not found.";
		public const string FiscalPeriodInactive = "Financial Period '{0}' is inactive.";
		public const string FiscalPeriodClosed = "Financial Period '{0}' is closed.";
		public const string FiscalPeriodInactiveOrClosed = "Financial Period {0} is inactive or closed.";
		public const string FiscalPeriodNotCurrent = "Transaction date is outside the specified period date range.";
		public const string FiscalPeriodNoPeriods = "At least one period should be defined.";
		public const string FiscalPeriodEndDateNotEqualFinYearEndDate = "End Date of the last Financial Period ({0}) is not equal to the end date of the Financial Year ({1}). Financial Year or Financial Periods configuration should be updated. Please select update method.";
		public const string FiscalPeriodEndDateLessThanStartDate = "End Date may not be less than Start Date";
		public const string FiscalPeriodAdjustmentPeriodError = "Adjustment period should be the last period of the financial year.";
		public const string FiscalPeriodMethodModifyNextYear = "Start Date of the next Financial Year will be moved to match end date of the last period of the current year ({0}).";
		public const string FiscalPeriodMethodModifyNextYearSetup = "Financial Year settings will be modified. Start date of Financial Year will be moved to {0} ";
		public const string FiscalPeriodMethodModifyNextYearSetupBack = "Financial Year settings will be modified. Start date of Financial Year will be moved {0} day(s) back.";
		public const string FiscalPeriodMethodModifyNextYearSetupForward = "Financial Year settings will be modified. Start date of Financial Year will be moved {0} day(s) forward.";
		public const string FiscalPeriodMethodModifyNextYearNoChange = "Financial Year settings will not be modified.";
		public const string FiscalPeriodMethodExtendLastPeriod = "Last period end date will be moved to {0}";
		public const string FiscalPeriodMethodWeekStartWarning = "<html><head></head><body><br><br><b>Warning: Financial Period start day of week will be moved from {0} to {1}</b></body></html>";
		public const string FiscalPeriodMethodEndDateMoveWarning = "<html><head></head><body><br><br><b>Warning: End Date of the last financial period will be moved to {0} to meet Financial Year settings (Financial Period start day of week: {1})</b></body></html>";
		public const string FiscalYearCustomPeriodsMessageTitle = "Modify financial periods";
		public const string FiscalYearCustomPeriodsMessage = "Are you sure you want to modify financial periods for this year? This action could affect statistics, budgets and data on reports.";
		public const string SourceDocumentCanNotBeFound = "Source document for the selected transaction cannot be found.";
		public const string LastBatchCanNotBeFound = "Last batch for the selected allocation cannot be found.";
		public const string AccountRevalRateTypefailed = "Revaluation Rate Type can only be entered for a GL Account that is denominated in foreign currency.";
		public const string AllocationSrcEmptySubAccMask = "Source Sub mask cannot be empty. Use the '?' symbol as wildcard.";
		public const string AllocationSrcEmptyAccMask = "Source Account mask cannot be empty.";
        public const string ConsolidationError = "An error occurred during the consolidation process.";
		public const string ConsolidationBatchOutOfBalance = "Consolidation GL Batch number {0} is out of balance, please review.";
		public const string ConsolidationConnection = "Cannot connect to the consolidation Web Service.";
		public const string DocumentsNotReleased = "One or more documents could not be released.";
		public const string DocumentsNotPosted = "One or more documents was released but could not be posted.";
        public const string PeriodEndDateIsAfterYearEnd = "The End Date of the financial period must belong to the same financial year as its Start Date.";
		public const string InvalidCashAccountSub = "Specified Subaccount cannot be used with this Cash Account.";
		public const string AllocationBatchDateIsBeforeTheExistingBatchForPeriod = "This GL Allocation was last executed on {0:d}. You cannot run this GL Allocation with an earlier date.";
		public const string AllocationIsNotApplicableForThePeriod = "This GL Allocation is not configured to be executed for the selected Financial Period.";
		public const string AllocationProcessingRequireFinPeriodIDAndDate = "You must provide the Date and Financial Period for processing.";
		public const string SheduleNextExecutionDateExceeded = "The next generation date for this task is greater than the current business date.";
		public const string SheduleExecutionLimitExceeded = "The task reached the configured limit and will not be processed. Please change the task limit or deactivate it.";
		public const string SheduleHasExpired = "The task will not be processed. The expiration date is less than the current business date. Please change the business date or deactivate the task.";
		public const string TranAmountsDenormalized = "Both Debit and Credit parts of transaction are not zero. Exiting with error.";
		public const string DetailsReportIsNotAllowedForYTDNetIncome = "Year to Date Net Income account cannot be selected for inquiry.";
		public const string DuplicateAccountSubEntry = "Duplicate GL Account/Sub Entry.";
		public const string DuplicateAccountSubEntryParams = "Duplicate GL Account/Sub Entry ({0} - {1})";
		public const string AcctSubMayNotBeEmptyForNonGroup = "Account/Subaccount may not be empty for non-node lines.";
		public const string AcctSubMaskNotBeEmptyForNonGroup = "Account/Subaccount Mask may not be empty for non-node lines.";
		public const string AcctMaskMayNotBeEmptyForGroup = "Account Mask may not be empty if Subaccount Mask is entered.";
		public const string SubMaskMayNotBeEmptyForGroup = "Subaccount Mask may not be empty if Account Mask is entered.";
		public const string FutureAllocationApplicationDetected = "GL Allocation {0} was already executed after the selected Financial Period.";
		public const string CantChangeField = "{0} cannot be changed.";
		public const string AllocationStartPeriodHasIncorrectFormat = "Start Period has incorrect format";
		public const string AllocationEndPeriodHasIncorrectFormat = "End Period has incorrect format";
		public const string AllocationEndPeriodIsBeforeStartPeriod = "End Period should be later then Starting Period";
		public const string BudgetArticleIsNotAllocatedProperly = "The Budget Article is not allocated properly.";
		public const string BudgetApproveUnexpectedError = "Unexpected error occurred.";
		public const string BudgetItemsApprovalFailure = "Several items failed to be approved.";
		public const string BudgetPreloadArticlesConfirmation = "Do you want to preload articles from the year {0}?";
		public const string BudgetArticlesPreloadFromConfigurationTitle = "Preload from Budget Configuration";
		public const string BudgetArticlesPreloadFromConfiguration = "Budget Tree will be preloaded from Budget Configuration.";
		public const string BudgetLineAmountNotEqualAllocated = "Allocated Amount is not equal to Article Amount. Article cannot be released.";
		public const string BudgetTreeOverlappingAccounts = "Account-Subaccount pair overlaps with another Account-Subaccount pair: {0} - {1}";
		public const string BudgetTreeOverlappingMask = "Account-Subaccount mask pair overlaps with another Account-Subaccount mask pair: {0} - {1}";
		public const string BudgetTreeIncorrectAccountMask = "Account Mask should not extend beyond the boundaries of the parent node's Account Mask ({0})";
		public const string BudgetTreeIncorrectSubMask = "Subaccount Mask should not extend beyond the boundaries of the parent node's Subaccount Mask ({0})";
		public const string BudgetTreeDeleteGroupTitle = "Delete group";
		public const string BudgetTreeDeleteGroupMessage = "All child records will be deleted. Are you sure you want to delete group?";
		public const string BudgetTreePreloadArticlesTitle = "Can not preload articles";
		public const string BudgetTreePreloadArticlesMessage = "No lines can be preloaded using Account mask provided: {0}";
        public const string BudgetTreePreloadArticlesTooManyMessage = "{0} subarticles will be created. Are you sure you want to continue?";
		public const string BudgetTreePreloadArticlesNothingToPreload = "No lines can be preloaded using Account/Subaccount mask provided.";
		public const string BudgetTreeCannotMoveGroupTitle = "Cannot move group";
		public const string BudgetTreeCannotMoveGroupAggregatingArticle = "Group cannot be moved into the aggregating article.";
		public const string BudgetAccountNotAllowed = "Selected account is not allowed in this group (Account mask: {0})";
		public const string BudgetSubaccountNotAllowed = "Selected subaccount is not allowed in this group or does not exist (Subaccount mask: {0})";
		public const string BudgetUpdateTitle = "Convert Budget";
		public const string BudgetUpdateMessage = "Warning: This action is irreversible. Are you sure you want to convert budget?";
		public const string BudgetUpdateConflictMessage = "One or more existing Budget articles are conflicting with the current Budget Configuration. First conflicting line: {0} - {1}. Budget cannot be converted.";
		public const string BudgetRollback = "Roll Back to Released Values";
		public const string BudgetConvert = "Convert Budget Using Current Budget Configuration";
		public const string BudgetRollbackMessage = "All Budget Articles will be rolled back to the last released values. All changes will be lost.";
		public const string BudgetConvertMessage = "Current budget will be converted using the budget tree from the Budget Configuration form.";
		public const string BudgetManageAction = "Select Action";
		public const string BudgetDifferentCurrency = "Ledger currency is different from the Budget currency";
		public const string BudgetPendingChangesTitle = "Pending changes";
		public const string BudgetPendingChangesMessage = "Budget has pending changes. Please review Budget and save or discard pending changes before moving to the next Budget.";
		public const string BudgetDeleteTitle = "Delete budget";
		public const string BudgetDeleteMessage = "Budget has one or more lines released. Budget can not be deleted.";
		public const string Confirmation = "Confirmation";
		public const string BudgetArticleDescrAggregated = "Aggregated";
		public const string BudgetArticleDescrCompared = "Compared";
		public const string ApprovedBudgetArticleCanNotBeDeleted = "Approved Budget Articles with non-zero amounts cannot be deleted.";
		public const string ReleasedBudgetArticleCanNotBeDeleted = "Budget Articles with non-zero Released amount cannot be deleted.";
		public const string PreconfiguredArticlesCanNotBeDeleted = "Preconfigured Articles cannot be deleted.";
		public const string ComparisonLinesCanNotBeDeleted = "Comparison line cannot be deleted.";
		public const string PreserveChildArticlesConfirmation = "Do you want to preserve child articles?";
		public const string ApprovedBudgetChildArticlesCanNotBeDeleted = "Approved Child Budget Articles with non-zero amounts cannot be deleted.";
		public const string UnknownAllocationPercentLimitType = "Algorithm for the Percent Limit Type {0} is not implemented";
		public const string UnknownAccountTypeDetected = "Unknown account type {0}";
		public const string AllocationSourceAccountSubInterlacingDetected = "Account-Sub combination can not be included into several source lines.";
		public const string AllocationDistributionTargetOverflowDetected = "Allocation cannot be completed - Distribution algorithm produced too large number for Account{0} Sub {1}. Most probable reason - total weight of the Destination Accounts is too small, giving exception 0/0";
		public const string CantFindConsolidationLedger = "Cannot find the source ledger '{0}'.";
		public const string CantFindConsolidationBranch = "Cannot find the source branch '{0}'.";
		public const string NumberRecodsProcessed = "{0} records processed successfully.";
		public const string NoRecordsToProcess = "There are no records to process.";
		public const string ConsolidationBatch = "Consolidation created from '{0}'.";
		public const string AccountOrSubNotFound = "Either Account ID '{0}' or Sub. ID '{1}' specified is invalid.";
		public const string FiscalPeriodInvalid = "Financial Period '{0}' is invalid.";
        public const string TranslationAlreadyReversed = "Translation '{0}' has already been reversed.";
		public const string ImportAccountCDIsEmpty = "Account CD cannot be empty";
		public const string ImportAccountIDIsEmpty = "Account is not mapped";
		public const string ImportSubAccountCDIsEmpty = "SubAccount CD cannot be empty";
		public const string ImportSubAccountIDIsEmpty = "SubAccount is not mapped";
		public const string ImportAccountCDNotFound = "Account cannot be mapped";
		public const string ImportYtdBalanceIsEmpty = "Balance is incorrect";
		public const string FailedControlTotalBalance = "Total Balance is failed, please review";
		public const string InvalidNetIncome = "Year to Date Net Income account is not configured properly in GL Setup.";
		public const string InvalidRetEarnings = "Retained Earnings account is not configured properly in GL Setup.";
		public const string ImportantConfirmation = "Important";
		public const string FirstFinYearDecrementConfirmation = "Warning - This operation will shift the company's First Financial Year one year earlier. Shifting the First Year to an earlier date can affect statistics and data on reports. Do you want to continue?";
		public const string FirstFinYearDecrementConfirmationGeneric = "Warning - This operation will shift the First Year one year earlier. Do you want to continue?";
		public const string InsertFinYearBeforeFirstConfirmation = "Warning - The system detected a financial year that is earlier than the First Financial Year defined for the company. This will create a new first financial year in the database. Do you want to continue?";
		public const string ThisAccountClassMayNotBeDeletedBecauseItIsUsedIn = "This Account Class may not be deleted because it is used in {0}.";
		public const string MultiplyCurrencyInfo = "Multiply documents consolidated in original batch, first document currency information was used.";
		public const string PeriodsStartingDateIsTooFarFromYearStartingDate = "Difference between Periods starting date and Year start date must not exceed {0} days";
		public const string ERR_InfiniteLoopDetected = "System error - Infinit Loop detected";
		public const string AllTheFinPeriodsAreAlreadyInserted = "All the periods are already inserted";
        public const string PeriodTemplatesCanotBeGeneratedForThisPeriodType = "Period templates can't be generated for this Period Type";
		public const string MapAccountError = "Account From should be less that Account To.";
		public const string MapAccountDuplicate = "Entered account range overlaps with another one.";
        public const string ERR_AllocationDestinationAccountMustNotBeDuplicated = "Destination accounts may not be duplicated for this allocation type";
        public const string AllocationDestinationAccountAreIdentical = "These Allocation destinations have identical accounts settings. Probably, they should be merged.";
		public const string ERR_DocumentForThisRowIsNotCreatedYet = "A Document for this row is not created yet";
		public const string RowMayNotBeDeletedItReferesExistsingDocument = "This row may not be deleted - a it refers an existing document";
		public const string PaymentMethodWithARIntegratedProcessingMayNotBeEntered = "Payment Methods which have 'Integrated Processing' setting set may not be used in this interface";
		public const string PaymentMethodWithAPPrintCheckOrAPCreateBatchMayNotBeEntered = "Payment Methods which have 'Print Check' or 'Create Batch Payment' setting set may not be used in this interface";
		public const string DocLineHasUnvalidOrUnsupportedType = "Row {0} has invalid on unsupported type {1} {2}";
		public const string CreationOfSomeOfTheIncludedDocumentsFailed = "{0} of {1} documents were not created";
		public const string PostingOfSomeOfTheIncludedDocumentsFailed = "Documents were successfully created, but {0} of {1} were not posted";
		public const string UnsupportedTypeOfTheDocumentIsDetected = "Unsupported type of the document detected on line {0} - {1} {2} {3}";
		public const string DocumentWasNotCreatedForTheLine = "Document is not created for the line {0} {1}";
		public const string ERR_BatchContainRowsReferencingExistingDocumentAndCanNotBeDeleted = "This batch contains rows referencing created documents. It may not be deleted";
		public const string TransactionCodeMayNotBeChangedForTheLineWithAssinedReferencedNumber = "The reference number has been assigned - it's not possible to change Transaction Code.";
		public const string ProjectIsRequired = "Project is Required but was not specified. Account '{0}' used in the GL Transaction is mapped to Project Account Group.";
		public const string DocumentIsOutOfBalance = "This document is not balanced. The difference is {0}.";
		public const string TaxDetailCanNotBeInsertedManully = "Tax detail can not be inserted manually";
        public const string GLHistoryValidationRunning = "Batch released but not posted because GL history validation process is running. Wait for completion and post it manually.";
		public const string DateMustBeSetWithingPeriodDatesRange = "To have an effect Date must be set between Period Start Date and Period End Date.";
        public const string ActualLedgerInBaseCurrency = "Actual ledger '{0}' must be defined in base currency {1} only.";
		public const string DocumentTypeIsNotSupportedYet = "This Tran. type is not supported yet. It may not be set 'Active'";
		public const string DocumentMustHaveBalanceToMayBeSplitted = "You must enter a document's Total Amount before you may add splits";
		public const string DocumentMustByAppliedInFullBeforeItMayBeReleased = "You have to apply full amount of this document before it may be released";
        public const string AllocationSourceAccountsContainInactiveContraAccount = "One or more of allocation source contains inactive contra accounts";
        public const string AllocationDestinationAccountsContainInactiveAccount = "One or more of destination accounts is inactive";
        public const string AllocationDestinationAccountsContainInactiveBasisAccount = "All basis accounts are inactive for one or more destination accounts";

		public const string DocumentIsNotAppliedInFull = "This document is not fully applied";
		
        public const string DayOfWeekNotSelected = "You must select at least one day of week";
        public const string CashAccountDoesNotExist = "Cash account for this account, sub account and branch doesn't exist";
        public const string BatchNbrNotValid = "Batch Number is not valid!";
        public const string YTDNetIncomeMayBeLiability = "YTD Net Income Account may be only liability type";

        public const string UnrecognizedTaxFoundUsedInDocument = "Tax {0} used in Document with Reference Nbr. {1} is not found in the system";
        public const string DocumentWithTaxIsNotBalanced = "Document with Reference Nbr. {0} is not balanced. Tax information may not be created for it.";
        public const string DocumentWithTaxContainsBothSalesAndPurchaseTransactions = "Document with Reference Nbr. {0} contains both sales and purhase Tax transactions. Tax information may not be created for it.";
        public const string DocumentContainsSalesTaxTransactionsButNoIncomeAccounts = "Document with Reference Nbr. {0} contains sales tax transactions, but there are no transactions to income or asset accounts which may be considered as taxable. Tax information may not be created for it.";
        public const string DocumentContainsPurchaseTaxTransactionsButNoExpenseAccounts = "Document with Reference Nbr. {0} contains purchase tax transactions, but there are no transactions to expense or asset accounts which may be considered as taxable. Tax information may not be created for it.";
        public const string SeveralTaxRevisionFoundForTheTax = "There are several records for the Tax Rate for tax {0} and date {1}. Tax inforamtion may not be created";
        public const string TaxableAndTaxAmountsHaveDifferentSignsForDocument = "Taxable and Tax Amount have different signs for tax {0} in the document {1}";
        public const string TaxAmountIsNegativeForTheDocument = "Tax Amount is negative for tax {0} in the document {1}";
        public const string TaxAmountEnteredDoesNotMatchToAmountCalculatedFromTaxableForTheDocument = "Tax Amount {0} does not match the amount {1} calculated from taxable amount {2} and tax Rate {3}% for Tax {4} in the document {5}";
		public const string DeductedTaxAmountEnteredDoesNotMatchToAmountCalculatedFromTaxableForTheDocument = "Tax Amount {0} does not match the amount {1} calculated from taxable amount {2} and tax Rate {3}% and Non-deductible tax rate {4}% for Tax {5} in the document {6}";
        public const string TypeForTheTaxTransactionIsNoRegonized = "Type of tax transaction is not recognized from Tax Type{0} and sign {1}";
        public const string NoTaxableLinesForTaxID = "Document Reference Nbr. {0} doesn't contain any taxable lines that can be applied to {1} Tax";
        public const string TaxIDMissingForAccountAssociatedWithTaxes = "Account is associated with one or more taxes, but Tax ID is not specified";
		public const string RevaluationRateTypeIsNotDefined = "Revaluation Rate Type is not Defined";



        #endregion

		#region Translatable Strings used in the code
		public const string AdjustmentPeriod = "Adjustment Period";
		public const string QuarterDescr = "Quarter# {0}";
		public const string PeriodDescr = "Period# {0}";
		public const string SummaryTranDesc = "";
		public const string RoundingDiff = "Rounding difference";
		public const string AllocTranSourceDescr = "Src: {0}";
		public const string AllocTranDestDescr = "Dst: {0}";
		public const string ConsolidationDetail = "Consolidation detail";
		public const string PeriodFormatted = "Period {0:00}";
		public const string Times = "Times";
		public const string Period = "Period(s)";
		public const string Week = "Week(s)";
		public const string Month = "Month(s)";
		public const string Day = "Day(s)";
		public const string PredefinedSettings = "Predefined Settings";
		public const string COAOrderOp0 = "1:Assets 2:Liabilities 3:Income and Expenses";
		public const string COAOrderOp1 = "1:Assets 2:Liabilities 3:Income 4:Expenses";
		public const string COAOrderOp2 = "1:Income 2:Expenses 3:Assets 4:Liabilities";
		public const string COAOrderOp3 = "1:Income and Expenses 2:Assets 3:Liabilities";
		public const string COAOrderOp128 = "Custom Chart of Accounts Order";
		public const string Periods = "Periods";
		public const string DecremenFirstYear = "Shift the First Year";
		//public const string Confirmation = "Confirmation";
		//public const string BudgetArticleDescrAggregated = "Aggregated";
		//public const string BudgetArticleDescrCompared = "Compared";
		public const string ThereIsDenominatedAccount = "There is a denominated account.";
		public const string BatchDetails = "Batch Details";
        public const string InterCoTranDesc = "Balancing entry for: {0}";
        public const string Draft = "Draft";



		#region PeriodTypes
		public const string PT_Month = "Month";
		public const string PT_BiMonth = "Two Months";
		public const string PT_Quarter = "Quarter";

		public const string PT_Week = "Week";
		public const string PT_BiWeek = "Two Weeks";
		public const string PT_FourWeeks = "Four Weeks";
		public const string PT_FourFourFive = "4-4-5 Week";
		public const string PT_FourFiveFour = "4-5-4 Week";
		public const string PT_FiveFourFour = "5-4-4 Week";
		public const string PT_CustomPeriodsNumber = "Custom Number of Periods";
		public const string PT_CustomPeriodLength = "Custom-Length Periods"; 
		#endregion
		#endregion

		#region Graph Names
		public const string JournalEntry = "Journal Transactions Entry";
		public const string AccountByPeriodEnq = "GL Account History - Detailed Inquiry";
		public const string AccountClassMaint = "GL Account Class Maintenance";
		public const string AccountHistoryBySubEnq = "GL Account History - Inquiry by Sub ";
		public const string AccountHistoryByYearEnq = "GL Account History - Inquiry by Period";
		public const string AccountHistoryEnq = "GL Account History - Summary Inquiry";
		public const string AccountMaint = "Accounts Maintenance";
		public const string AllocationMaint = "Allocations Maintenance";
		public const string AllocationProcess = "Allocation Processing";
		public const string FiscalYearSetupMaint = "Financial Year Setup";
		public const string JournalEntryImportProcess = "Journal Transactions Entry Import";
		public const string PostProcess = "Process of Posting GL Transactions";
		public const string ScheduleProcess = "Repeating Tasks Processing";
		public const string ScheduleMaint = "Repeating Tasks Maintenance";
		public const string GLConsolReadMaint = "Consolidation Maintenance";
		public const string GLConsolSetupMaint = "Consolidation Setup";
		public const string GLSetupMaint = "General Ledger Preferences";
		public const string SubAccountMaint = "Subaccount Maintenance";
		public const string GeneralLedgerMaint = "General Ledger Maintenance";
		public const string FiscalPeriodMaint = "Financial Period Maintenance";
		public const string ClosingProcess = "Financial Period Closing Process";
		public const string BudgetApproveProcess = "Budget Approval";
		public const string BatchReleaseProcess = "GL Batches Release Process";
		public const string BatchPostProcess = "GL Posting Process";
		public const string GLHistoryValidate = "Validate Account History";
		public const string BudgetMaint = "Budgets Maintenance";
        public const string GLTransactionCodesMaint = "GL Transaction Codes Setup";
		public const string GLNumberCodesMaint = "GL Numbering Codes Setup";
		public const string JournalEntryWithSubModulePosting = "Voucher Entry";
		#endregion

		#region Cache Names
		public const string SubAccountSegment = "Subaccount Segment";
		public const string BudgetArticle = "Budget Article";
		public const string Batch = "GL Batch";
		public const string Transaction = "GL Transaction";
		public const string Account = "GL Account";
		public const string AccountClass = "GL Account Class";
		public const string Sub = "Subaccount";
		public const string GLTrialBalanceImportMap = "Trial Balance Import";
		public const string GLTrialBalanceImportDetails = "Trial Balance Import Details";
		public const string GLHistoryEnquiryResult = "GL History Enquiry Results";
		public const string Schedule = "Schedule";
		public const string Allocation = "Allocation";
		public const string FinancialYear = "Financial Year";
		public const string GLTranDoc = "GL Tran Document";
		public const string GLDocBatch = "GL Document Batch";
        public const string TransactionDoc = "Journal Voucher";
        public const string BranchAcctMap = "Branch Account Map";
        public const string BranchAcctMapTo = "Branch Account Map To";
        public const string BranchAcctMapFrom = "Branch Account Map From";
        public const string BatchSelection = "Batch to Process";
        #endregion

		#region Combo Values
		public const string Actual = "Actual";
		public const string Report = "Reporting";
		public const string Statistical = "Statistical";
		public const string Budget = "Budget";

		public const string ModuleGL = "GL";
		public const string ModuleAP = "AP";
		public const string ModuleAR = "AR";
		public const string ModuleCM = "CM";
		public const string ModuleCA = "CA";
		public const string ModuleIN = "IN";
		public const string ModuleDR = "DR";
		public const string ModulePO = "PO";
		public const string ModuleSO = "SO";
		public const string ModeleFA = "FA";
		public const string ModulePM = "PM";
		public const string ModuleTX = "TX";

		//-Batch status	
		public const string Hold = "On Hold";
		public const string Released = "Released";
		public const string Balanced = "Balanced";
		public const string Unposted = "Unposted";
		public const string Posted = "Posted";
		public const string Completed = "Completed";
		public const string Voided = "Voided";
		public const string PartiallyReleased = "Partially Released";
		public const string Scheduled = "Scheduled";

		//Batch Type
		public const string BTNormal = "Normal";
		public const string BTRecurring = "Recurring";
		public const string BTConsolidation = "Consolidation";
		public const string BTTrialBalance = "Trial Balance";

		//AccountType
		public const string Asset = "Asset";
		public const string Liability = "Liability";
		public const string Income = "Income";
		public const string Expense = "Expense";

		//AccountPost Options
		public const string PostSummary = "Summary";
		public const string PostDetail = "Detail";

		public const string GLAccess = "GL Access";
		public const string GLAccessBudget = "GL Access Budget";
		public const string GLAccessBranchAccount = "GL Access Branch Account";
		public const string GLAccessBranchSub = "GL Access Branch Sub";		
		public const string GLAccessDetail = "GL Access Detail";
		public const string GLAccessByAccount = "GL Access by Account";
		public const string GLAccessByBranch = "GL Access by Branch";		
		public const string GLAccessBySub = "GL Access by Subaccount";
		public const string GLAccessByArticle = "GL Access by Budget Article";

		public const string Daily = "Daily";
		public const string Weekly = "Weekly";
		public const string Monthly = "Monthly";
		public const string Periodically = "By Financial Period";
		//Allocation Distribution
		public const string ByPercent = "By Percent";
		public const string ByWeight = "By Weight";
		public const string ByAccountPTD = "By Dest. Account PTD";
		public const string ByAccountYTD = "By Dest. Account YTD";
		public const string ByExternalRule = "By External Rule";
		//Allocation Collection Method
		public const string CollectByAccountPTD = "By Account PTD";
		public const string CollectFromPreviousAllocation = "From Prev. GL Allocation";
		//Allocation SourceLimit 
		public const string PercentLimitTypeByPeriod = "Period";
		public const string PercentLimitTypeByAllocation = "GL Allocation";

		//Auto Rev Options
		public const string AutoRevOnPost = "On Post";
		public const string AutoRevOnPeriodClosing = "On Period Closing";

        //Trial Balances Sign Type
        public const string Normal = "Normal";
        public const string Reversed = "Reversed";

		public const string Undefined = "Undefined";
		//Schedule MonthlyOnWeek options
		public const string OnFirstWeekOfMonth = "1st";
		public const string OnSecondWeekOfMonth = "2nd";
		public const string OnThirdWeekOfMonth = "3rd";
		public const string OnFourthWeekOfMonth = "4th";
		public const string OnLastWeekOfMonth = "Last";

		//Schedule MonthlyOnDayOfWeek options
		public const string MonthlyOnSunday = "Sunday";
		public const string MonthlyOnMonday = "Monday";
		public const string MonthlyOnTuesday = "Tuesday";
		public const string MonthlyOnWednesday = "Wednesday";
		public const string MonthlyOnThursday = "Thursday";
		public const string MonthlyOnFriday = "Friday";
		public const string MonthlyOnSaturday = "Saturday";
		public const string MonthlyOnWeekday = "Weekday";
		public const string MonthlyOnWeekend = "Weekend";

		public const string PeriodStartDate = "Start of Financial Period";
		public const string PeriodEndDate = "End of Financial Period";
		public const string PeriodFixedDate = "Fixed Day of the Period";

		//Trial Balance Import Statuses
		public const string New = "New";
		public const string Valid = "Validated";
		public const string Duplicate = "Duplicate";
		public const string Error = "Error";

        public const string RunTillDate = "On this date";
        public const string RunMultipleTimes = "After running this number of schedules";

		//Budget Distribution Methods
		public const string Evenly = "Evenly";
		public const string PreviousYear = "Proportionally to the Previous Year";
		public const string ComparedValues = "Proportionally to Compared Values";

		//Budget Preload Actions
		public const string ReloadAll = "Delete and Reload All Articles";
		public const string UpdateExisting = "Update Existing Articles Only";
		public const string UpdateAndLoad = "Update Existing Articles and Load Nonexistent Articles";
		public const string LoadNotExisting = "Load Nonexistent Articles Only";

		//Financial Periods Save Actions
		public const string FinPeriodUpdateNextYearStart = "Modify start date of the next year";
		public const string FinPeriodUpdateFinYearSetup = "Modify financial year settings";
		public const string FinPeriodExtendLastPeriod = "Extend last period";
		public const string FinPeriodShortenLastPeriod = "Shorten last period";

		//Financial Periods Save Actions
		public const string EndYearCalculation_Default = "Last day of the financial year";
		public const string EndYearCalculation_LastDay = "Include last <Day of Week> of the financial year";
		public const string EndYearCalculation_ClosestDay = "Include <Day of Week> nearest to the end of the financial year";
		#endregion

		#region Custom Actions
		public const string Refresh = "Refresh";
		public const string Save = "Save";
		public const string ttipRefresh = "Refresh";
		public const string ttipSave = "Commit Changes";
		public const string Release = "Release";

		public const string ProcPost = "Post";
		public const string ProcPostAll = "Post All";

		public const string ProcRelease = "Release";
		public const string ProcReleaseAll = "Release All";

		public const string ProcApprove = "Approve";
		public const string ProcApproveAll = "Approve All";

		public const string ProcSynchronize = "Synchronize";
		public const string ProcSynchronizeAll = "Synchronize All";

		public const string ProcValidate = "Validate";
		public const string ProcValidateAll = "Validate All";

		public const string Process = IN.Messages.Process;
		public const string ProcessAll = IN.Messages.ProcessAll;

		public const string ProcRunSelected = "Run Selected";
		public const string ProcRunAll = "Run All";

		public const string ProcRunNow = "Run Now";

		public const string ClosePeriods = "Close Periods";
		public const string ShowDocuments = "Print Open Documents";
		public const string GeneratePeriods = "Generate Periods";
		public const string GeneratePeriodsToolTip = "Auto fill periods";
		public const string AutoFillFiscalYearSetup = "Auto";
		public const string AutoFillFiscalYearSetupToolTip = "Auto fill periods";

		public const string GLEditDetails = "GL Edit Details";

		public const string AddToRepeatingTasks = "Add to Schedule";
		public const string ReverseBatch = "Reverse Batch";
		public const string BatchRegisterDetails = "Batch Register Details";

		public const string ViewGLEdit = "GL Edit";

		public const string ViewBatch = "View Batch";
		public const string ttipViewBatch = "Shows selected batch.";
		public const string ViewSourceDocument = "View Source Document";

		public const string ViewAccountDetails = "Account Details";
		public const string ViewAccountBySub = "Account by Subaccount";
		public const string ViewAccountByPeriod = "Account by Period";

		public const string ViewCashAccount = "Cash Account";
		public const string Membership = "Membership";
		public const string ttipMembership = "View user membership.";
		public const string ViewRestrictionGroups = "View Restriction Groups";

		public const string ShowDocumentTaxes = "Show Taxes";
		public const string ViewResultDocument = "View Document";

		public const string PreloadArticles = "Preload Articles";
		public const string Distribute = "Distribute";
		public const string PreloadArticlesTree = "Preload Accounts";
		public const string ConfigureSecurity = "Configure Security";

		#endregion


	}
}

