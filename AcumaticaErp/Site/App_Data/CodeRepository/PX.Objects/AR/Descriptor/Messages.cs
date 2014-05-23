using System;
using PX.Data;
using PX.Common;

namespace PX.Objects.AR
{

	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		// Add your messages here as follows (see line below):
		#region Validation and Processing Messages
		public const string Prefix = "AR Error";
		public const string Document_Status_Invalid = AP.Messages.Document_Status_Invalid;
		public const string Entry_LE = AP.Messages.Entry_LE;
		public const string Entry_GE = AP.Messages.Entry_GE; 
		public const string UnknownDocumentType = AP.Messages.UnknownDocumentType;
		public const string Only_Open_Documents_MayBe_Processed = AP.Messages.Only_Open_Documents_MayBe_Processed;
		public const string Document_OnHold_CannotRelease = AP.Messages.Document_OnHold_CannotRelease;
		public const string ApplDate_Less_DocDate = AP.Messages.ApplDate_Less_DocDate;
		public const string WriteOff_ApplDate_Less_DocDate = "Write-Off {0} cannot be less than Document Date.";
		public const string ApplPeriod_Less_DocPeriod = AP.Messages.ApplPeriod_Less_DocPeriod;
		public const string ApplDate_Greater_DocDate = AP.Messages.ApplDate_Greater_DocDate;
		public const string ApplPeriod_Greater_DocPeriod = AP.Messages.ApplPeriod_Greater_DocPeriod;
		public const string DocumentBalanceNegative = AP.Messages.DocumentBalanceNegative;
		public const string DocumentApplicationAlreadyVoided = AP.Messages.DocumentApplicationAlreadyVoided;
		public const string DocumentOutOfBalance = AP.Messages.DocumentOutOfBalance;
		public const string CashSaleOutOfBalance = AP.Messages.QuickCheckOutOfBalance;
		public const string SheduleNextExecutionDateExceeded = GL.Messages.SheduleNextExecutionDateExceeded;
		public const string SheduleExecutionLimitExceeded = GL.Messages.SheduleExecutionLimitExceeded;
		public const string SheduleHasExpired = GL.Messages.SheduleHasExpired;
        public const string MultipleApplicationError = AP.Messages.MultipleApplicationError;
		public const string Cannot_Change_Details_Exists = "This field cannot be modified if one or more application is entered for the document.";
		public const string Only_Invoices_MayBe_Payed = "Only Invoices and Debit Adjustments can be selected for payment.";
		public const string VoidAppl_CheckNbr_NotMatchOrigPayment = "Void Payment must have the same Reference Number as the voided payment.";
		public const string FinChargeCanNotBeDeleted = "Financial charges cannot be entered directly. Please use Overdue Charges calculation process.";
		public const string CreditLimitWasExceeded = "The customer's credit limit has been exceeded.";
		public const string CreditDaysPastDueWereExceeded = "The customer's Days Past Due number of days has been exceeded!";
		public const string CustomerIsOnCreditHold = "The customer status is 'Credit Hold'.";
        public const string CustomerIsOnHold = "The customer status is 'On Hold'.";
        public const string CustomerIsInactive = "The customer status is 'Inactive'.";
        public const string SalesPersonIsInactive = "The sales person status is 'Inactive'.";
		public const string CustomerSmallBalanceAllowOff = "Write-Offs are not allowed for the customer.";
		public const string CreditHoldEntry = "Document status is 'On Credit Hold'.";
		public const string AdminHoldEntry = "Document status is 'On Hold'.";
		public const string SPCommissionCalcFailure = "Commission calculation process failed with one or more error.";
		public const string SPFuturePeriodIsInvalidToProcess = "Processing date is less than the start date for the selected commission period.";
		public const string SPOpenPeriodProcessingConfirmation = "Processing date is less than the end date for the selected commission period. All commissions entered after the comission period is closed will go to the next comission period. Please confirm that you want to continue with processing?";
		public const string SalesPersonAddedForAllLocations = "This Sales Person is added for all the Customer locations already.";
		public const string SalesPersonWithHistoryMayNotBeDeleted = "One or more AR transactions exists for the selected Sales Person. This record cannot be deleted.";
		public const string AllCustomerLocationsAreAdded = "All Customer locations has been added already.";
		public const string ErrorAutoApply = "Unexpected error occurred while applying {0} {1} to {2} {3}.";
		public const string CustomerClassChangeWarning = "Please confirm if you want to update current Customer settings with the Customer Class defaults. Original settings will be preserved otherwise.";
		public const string TempCrLimitInvalidDate = "Start date must be less or equal to the end date.";
        public const string TempCrLimitPeriodsCrossed = "Credit limit for this customer has already been exceeded.";
		public const string DuplicateCustomerPayment = "Payment with Payment Ref. '{0}' dated '{1}' already exists for this Customer and have the same Payment Method. It's Reference Number - {2} {3}.";
		public const string PaymentMethodIsAlreadyDefined = "You cannot add more than one Payment Method of this type for the Customer.";
		public const string ERR_UnreleasedFinChargesForDocument = "At least one unreleased overdue charge document has been found for this document. Processing has been aborted.";
		public const string WRN_FinChargeCustomerHasOpenPayments = "One or more unapplied or unreleased payments has been found for this Customer. Calculation of Overdue Charges can be affected by these documents. It is recommended to release and apply these documents prior to the processing.";
        public const string ERR_EmailIsRequiredForSendByEmailOptions = "Email address must be specified if any of the following options is activated: {0}.";
        public const string ERR_EmailIsRequiredForOption = "Email address must be specified if '{0}' option is activated.";
		public const string WRN_ProcessStatementDetectsUnappliedPayments = "One or more Customers with unapplied payment documents has been found. It is recommended to run Auto Apply Payments process prior to this Statement Cycle closure.";
		public const string WRN_ProcessStatementDetectsOverdueInvoices = "One or more Customers with overdue documents has been found. It is recommended to run Calculate Overdue Charges process prior to this Statement Cycle closure.";
		public const string WRN_ProcessStatementDetectsOverdueInvoicesAndUnappliedPayments = "One or more Customers with unapplied payments and overdue documents has been found. It's recommened to run Auto Apply Payments and Calculate Overdue Charges process prior to this Statement Cycle closure.";
		public const string Invoice_NotPrinted_CannotRelease = "Invoice/Memo document was not printed and cannot be released.";
		public const string Invoice_NotEmailed_CannotRelease = "Invoice/Memo document was not emailed and cannot be released.";
		public const string ERR_IncorrectFormatOfPMExpiryDate = "An invalid expiration date has been provided.";
		public const string ERR_ProcessingCenterForCardNotConfigured = "Processing center for this card type is not configured properly.";
		public const string ERR_ProcessingCenterTypeIsInvalid = "Type {0} defined for the processing center {1} cannot is not located within processing object.";
		public const string ERR_ProcessingCenterTypeInstanceCreationFailed = "Cannot instantiate processing object of {0} type for the processing center {1}.";
		public const string ERR_CCPaymentProcessingInternalError = "Error during request processing. Transaction ID:{0}, Error:{1}";
		public const string ERR_CCProcessingReferensedTransactionNotAuthorized = "Transaction {0} failed authorization";
		public const string ERR_CCProcessingTransactionMayNotBeVoided = "Transaction of {0} type cannot be voided";
		public const string ERR_CCProcessingTransactionMayNotBeCredited = "Transaction {0} type cannot not be credited";
		public const string ERR_CCTransactionCurrentlyInProgress = "This document has one or more transaction under processing.";
		public const string ERR_CCAuthorizedPaymentAlreadyCaptured = "This payment has been captured already.";
        public const string ERR_CCPaymentAlreadyAuthorized = "This payment has been pre-authorized already.";
        public const string ERR_CCPaymentIsAlreadyRefunded = "This payment has been refunded already.";
		public const string ERR_CCNoTransactionToVoid = "There is no successful transaction to void.";
		public const string ERR_CCTransactionOfThisTypeInvalidToVoid = "This type of transaction cannot be voided";
		public const string ERR_CCTransactionWasNotAuthorizedByProcCenter = "Authorization for transaction {0} failed. See transaction description for details.";
		public const string ERR_DuplicatedSalesPersonAdded = "This Sales Person is already added";
        public const string OverdueChargeDateAndFinPeriodAreRequired = "Overdue Charge Date and Fin. Period are required";
        public const string RecordAlreadyExists = "Record already exists";
        public const string TransactionIsAlreadyExpired = "Transaction is already expired";
        public const string UnknownPrepareOnType = "Unknown PrepareOn type";
        public const string CreditCardWithID_0_IsNotDefined = "Credit Card with ID {0} is not defined";
		public const string Cash_Sale_Cannot_Have_Multiply_Installments = "Multiple Installments are not allowed for Cash Sale.";
		public const string Multiply_Installments_Cannot_be_Reversed = "Multiple installments invoice cannot be reversed, Please reverse original invoice '{0}'.";
		public const string Application_Amount_Cannot_Exceed_Document_Amount = "Total application amount cannot exceeed document amount.";
		public const string CustomerPMInstanceHasDuplicatedDescription = "This customer already has a Payment Method with this Description.";
		public const string ERR_CCTransactionMustBeAuthorizedBeforeCapturing = "Transaction must be authorized before it may be captured";
		public const string ERR_CCOriginalTransactionNumberIsRequiredForVoiding = "Original transaction is required to may be voided";
		public const string ERR_CCOriginalTransactionNumberIsRequiredForVoidingOrCrediting = "Original transaction is required to may be voided/credited";
		public const string ERR_CCOriginalTransactionNumberIsRequiredForCrediting = "Original transaction is required to may be credited";
		public const string ERR_CCUnknownOperationType = "This operation is not implemented yet";
		public const string ERR_CCCreditCardHasExpired = "Credit card for the customer {1} is expired on {0}";
		public const string ERR_CCAuthorizationTransactionIsNotFound = "Priorly Authorized Transaction {0} is not found";
		public const string ERR_CCAuthorizationTransactionHasExpired = "Authorizing Transaction {0} has already expired. Authorization must be redone";
		public const string ERR_CCProcessingCenterUsedForAuthIsNotValid = "Processing center {0}, specified in authorizing transaction {1} can't be found";
		public const string ERR_CCProcessingCenterIsNotSpecified = "Processing center for payment method {0} is not specified";
		public const string ERR_CCProcessingCenterUsedForAuthIsNotActive = "Processing center {0}, specified in authorizing transaction {1} is inactive";
		public const string ERR_CCProcessingIsInactive = "Processing center {0} is inactive";
		public const string ERR_CCProcessingCenterIsNotActive = "Processing center {0} is inactive";
		public const string ERR_CCTransactionToVoidIsNotFound = "Transaction to be Void {0} is not found";
		public const string ERR_CCProcessingCenterUsedInReferencedTranNotFound = "Processing center {0}, specified in referenced transaction {1} can't be found";
		public const string ERR_CCProcessingCenterNotFound = "Processing center can't be found";
		public const string ERR_CCProcessingCenterUsedInReferencedTranNotActive = "Processing center {0}, specified in referenced transaction {1} is inactive";
		public const string ERR_CCTransactionToCreditIsNotFound = "Transaction to be Credited {0} is not found";
		public const string ERR_CCMultiplyPreauthCombined = "Multiply preauthorized orders combined in one invoice.";
		public const string ERR_CCTransactionMustBeVoided = "CC Payment must be voided.";
		public const string ERR_CCExternalAuthorizationNumberIsRequiredForCaptureOnlyTrans = "Authorization Number, received from Processing Center is required for this type of transaction.";
		public const string ARPaymentIsCreatedProcessingINProgress = "Payment {0} has been created";
		public const string ARPaymentIsCreatedButProcessingFailed = "Payments has been saccessfully created for several invoices, but their processing has failed. Please, check error in the specific row or check payment settings for the customer";
		public const string ARPaymentIsCreatedButProcessingFailedAndSomePaymentsFailed = "Payments was not success created for one or more selected documnets. For others, Payments has been saccessfully created, but their processing has failed. Please, check  an error in the specific rows";
		public const string CreationOfARPaymentFailedForSomeInvoices = "Creation of the Payment document has failed for one or more selected documents. Please, check specific error in each row";
		public const string PaymentProcessingFailedErrorReported= "Processing of Payment {0} {1} has failed. Reported error is: '{2}'";
		public const string ReservedWord = "'{0}' is a reserved word and cannot be used here.";
        public const string PrepaymentAppliedToMultiplyInstallments = "Prepayments cannot be applied to multiple-installment invoices.";
		public const string InvalidCashReceiptDeferredCode = "On Cash Receipt Deferred Code is not valid for the given document.";
		public const string SPCommissionPeriodMayNotBeProcessedThereArePeriodsOpenBeforeIt = "This Commission Period cannot be processed - all the previous commission periods must be closed first";
		public const string SPCommissionPeriodMayNotBeClosedThereArePeriodsOpenBeforeIt = "This Commission Period cannot be closed - all the previous commission periods must be closed first";
		public const string SPCommissionPeriodMayNotBeReopendThereAreClosedPeriodsAfterIt = "This Commission Period cannot be reopened - there are closed commission periods after it";
		public const string DuplicateInvoiceNbr = "Document with this Invoice Nbr. already exists.";
		public const string CannotSaveNotes = "Cannot save notes.";
		public const string CreditCardExpirationNotificationByEMailFailedCheckConfiguration = "E-mail notification for the Customer {0} has failed. Please, check Customer's e-mail or notification configuration";
		public const string CreditCardExpirationNotificationException = "Notification by E-mail failed: {0}";
		public const string InvoiceNotificationFailed = "Recipients not found to process email invoice.";
		public const string ARPaymentIsIncludedIntoCADepositAndCannotBeVoided = "This payment is included into Payment Deposit document. It can't be voided until deposit is released or the payment is excluded from it.";
		public const string PaymentIsRefunded = "Payment has been refunded with Refund {0}.";
		public const string AccountIsSameAsDeferred = "Transaction Account is same as Deferral Account specified in Deferred Code.";
		public const string DocumentNotFound = "Document {0} {1} cannot be found in the system.";
		public const string OriginalDocumentIsNotSet = "Original document is not set.";
		public const string DiscountOutOfDate = "Discount is out of date {0}.";
		public const string ERR_PCTransactionNumberOfTheOriginalPaymentIsRequired = "A valid PC Transaction number of the original payment is required";
		public const string DocsDepositAsBatchSettingDoesNotMatchClearingAccountFlag = "'Batch deposit' setting does not match 'Clearing Account' flag of the Cash Account";
        public const string PMDeltaOptionRequired = "Billing Option is required for ARTran when Amount is less than original PM Transaction";
		public const string PMDeltaOptionNotValid = "Bill Later Option is not valid under current settings. The Amount of the given transaction is set to zero. In the AR Preferences the 'Post All Transactions on Updating GL' is OFF. Either delete this line from the invoice so that it can be billed next time or enable posting of zero transactions in the proferences.";
		public const string DefaultPaymentForTheCustomerMethodMayNotBeDeleted = "This Payment Method is set as default for the Customer. It may not be deleted.";
		public const string PaymentMethodInstanceIsUsedInDocumentsAndMayNotBeDeleted = "This Payment Method was used in one or more documents. It may not be deleted - instead, you can make it inactive.";
		public const string CustomerIsInStatus = "Customer is {0}.";
		public const string CashAccountIsNotConfiguredForPaymentMethodInAR = "The Cash Account specified is not configuered for usage in AR for the Payment Method {0}";
        public const string CustomerClassCanNotBeDeletedBecauseItIsUsed = "This Customer Class can not be deleted because it is used in AR Setup.";
		public const string InactiveCreditCardMayNotBeProcessed = "The credit card with ID {0} is inactive and may not be processed";
		public const string InactiveCustomerPaymentMethodIsUsedInTheScheduledInvoices = "This Customer Payment method is inactive, but there are scheduled invoices using it. You need to correct them in order to avoid invoice processing interruptions.";
		public const string OnlyLastRowCanBeDeleted = "Only last row can be deleted";
        public const string ThisValueMUSTExceed = "This value MUST exceed {0}";
        public const string ThisValueCanNotExceed = "This value can not exceed {0}";
		public const string TaxIsNotUptodate = "Tax is not up-to-date.";
		public const string FreightTaxIsNotUptodate = "Freight Tax is not up-to-date.";
        public const string NoPaymentInstance = "There is no Customer Payment Method associated with the given record. This Payment method does not require specific information for the given customer.";
		public const string AskConfirmation = "Confirmation";
		public const string AskUpdateLastRefNbr = "Do you want to update Last Reference Number with entered number?";
        public const string GroupUpdateConfirm = "Restriction Groups will be reset for all Customer that belongs to this customer class.  This might override the custom settings. Please confirm your action";
		public const string SettingReaderIsNull = "Settings reader may not be null when initializing processing plugin!";
		public const string CardProcessingError = "Credit card processing error. {0} : {1}";
		public const string TokenizationNotSupported = "Tokenization feature is not supported by processing";
		public const string HostedFormNotSupoorted = "Hosted form feature is not supported by processing";
		public const string FeatureNotSupportedByProcessing = "{0} feature is not supported by processing";
		public const string NOCCPID = "No Payment Profile ID in detials for payment method {0}!";
		public const string CantEnterAllDetailsAtOnce = "You can't enter Payment Profileg ID and card details at the same time." +
														" Enter only Payment Profile ID to sync it with processing center." +
		                                                " Enter only details to create new payment method instance and sync it with processing center.";
		public const string AutoSyncImpossible = "More than one unsynchronized method found at processing center! Automatic syncronization is not possible. Please syncronize manually.";
		public const string CouldntGetPMIDetails = "Couldn't get details from processing center for payment method instance {0}";
        public const string DateToSettleCrossDunningLetterOfNextLevel = "'{0}'+'{1}' should not exceed the '{0}' of the next level Dunning Letter.";
        public const string NoStatementToRegenerate = "There is no Statement available to regenerate.  Go to Prepare Statement to create a Statement.";
        public const string StatementCycleNotSpecified = "Statement Cycle not specified for the Customer.";
        public const string NoStatementsForCustomer = "There is no Statement available for the Customer.  Go to Prepare Statement to create a Statement.";
		public const string ReasonCodeIsRequired = "Reason Code must be specified before running the process.";
        public const string DiscountGreaterLineTotal = "Discount Total may not be greater than Line Total";
        public const string GroupDiscountExceedLimit = "Total group discount exceeds limit configured for this customer class. Document Discount was not calculated.";
        public const string DocDiscountExceedLimit = "Total Group and Document discount exceeds limit configured for this customer class ({0}%).";
        public const string AccountMappingNotConfigured = "Account Task Mapping is not configured for the following Project: {0}, Account: {1}";
        public const string LineDiscountAmtMayNotBeGreaterExtPrice = "Discount Amount may not be greater than Ext. Price.";
		public const string WriteOffIsDisabled = "Write-Off is disabled for the given customer. Set non zero write-off limit on the Customer screen and try again.";
		public const string WriteOffIsOutOfLimit = "Document balance exceeds the configured write-off limit for the given customer (Limit = {0}). Change the write-off limit on the Customer screen and try again.";
		public const string LastDateExpirationDate = "The Expiration Date should not be earlier than the Effective Date.";
        public const string FreeItemMayNotBeEmpty = "Free Item may not be empty. Please select Free Item before activating discount.";
        public const string FreeItemMayNotBeEmptyPending = "Free Item may not be empty. Please select Pending Free Item and update discount before activating it.";
		public const string CarriersCannotBeMixed = "Common carrier and Local carrier cannot be mixed in one invoice. Tax calculation will be invalid.";
		public const string MultipleShipAddressOnInvoice = "Invoice references multiple shipments that were shipped to different locations. Tax calculation will be invalid.";
		public const string PaymentMethodNotConfigured = "To create tokenized payment methods you must first configure 'Payment Profile ID' in Payment Method's 'Settings for Use in AR'";
		public const string PostingToAvalaraFailed = "Document was released succesfully but failed to post tax to avalara with the following message: {0}";
		public const string NotAllCardsShown = "Some cards for {0} payment method(s) are not shown here because their data is stored at processing center";
		
		#endregion

		#region Translatable Strings used in the code
		public const string Document = "Document";
		public const string OrigDocument = "Orig. Document";
		public const string ViewLastCharge = "View Last Charge";
		public const string NewSchedule = "New Schedule";
		public const string ViewSchedule = "View Schedule";
		public const string MultiplyInstallmentsTranDesc = AP.Messages.MultiplyInstallmentsTranDesc;
		public const string DeferredRevenueTranDesc = "Deferred Revenue Recognition.";
		public const string NewInvoice = "Enter New Invoice";
		public const string NewPayment = "Enter New Payment";
		public const string Customer = "Customer";
		public const string ARAccess = "Customer Access";
		public const string ARAccessDetail = "Customer Access Detail";
		public const string Warning = "Warning";
		public const string FinChargeDocDescr = "Overdue charge";
		public const string FinChargeDocumentFormat = "{0} {1}";
		public const string SalesPerson = "Sales Person";
		public const string PrintInvoiceMemo = "Print Invoice/Memo";
		public const string VoidCommissions = "Void Commissions";
		public const string ClosePeriod = "Close Period";
		public const string ReopenPeriod = "Reopen Period";
		public const string Days = "Days";
		public const string MessageDescription = "Message Description";
		public const string OverDays = "Over Days";
		public const string DocumentDateSelection = "Document Date Selection";
		public const string Shipping = "Shipping";
		public const string Billing = "Billing";
		public const string ARAutoPaymentRefNbrFormat = "ACCB-{0:yyyy-MM-dd}";
		public const string ReviewSPComissionPeriod = "Review Commission Period";
		public const string Attention = "Attention!";
		public const string ProcessAll = "Process All";
		public const string CustomerPaymentMethodView = "Customer Payment Method";
		public const string CustomerView = "Customer";
		public const string BillingContactView = "Billing Contact";
		public const string CashSaleInvoice = "Cash Sale Invoice";
		public const string CashReturnInvoice = "Cash Return Invoice";
		public const string Calculate = "Calculate";
		public const string AllTransactions = "All Transactions";
		public const string FailedOnlyTransactions =  "Failed Only";
		public const string CreditCardIsExpired = "CC Expired";
		public const string ViewVendor = "View Vendor";
		public const string ViewBusnessAccount = "View Business Account";
		public const string ExtendToVendor = "Extend To Vendor";
		public const string ImportedExternalCCTransaction = "Imported External Transaction";
        public const string ARBalanceByCustomerReport = "AR Balance by Customer";
        public const string CustomerHistoryReport = "Customer History";
        public const string ARAgedPastDueReport = "AR Aged Past Due";
        public const string ARAgedOutstandingReport = "AR Aged Outstanding";
        public const string ARRegisterReport = "AR Register";
        public const string DocDiscDescr = "Group and Document Discount";
		public const string BasePriceClassDescription = "Base Price Class";
        public const string ViewARDiscountSequence = "View Discount Sequence";

        #endregion

		#region Graphs Names
		public const string ARAutoApplyPayments = "Payment Application Process";
		public const string ARCreateWriteOff = "Balance Write Off Process";
		public const string CustomerClassMaint = "Customer Classes Maintenance";
		public const string CustomerMaint = "Customer Maintenance";
		public const string CustomerPaymentMethodMaint = "Customer Payment Methods Maintenance";
		public const string ARInvoiceEntry = "AR Invoice Entry";		
		public const string ARPaymentEntry = "AR Payment Entry";
		public const string ARDocumentRelease = "AR Documents Release Process";
		public const string ARPrintInvoices = "AR Invoice Printing Process";
		public const string ARReleaseProcess = "AR Release Process";
		public const string ARCustomerBalanceEnq = "Customers Balance - Summary Inquiry";
		public const string ARDocumentEnq = "Customer Balance - Detail Inquiry";
		public const string ARStatementProcess = "Customer Statement Preparation Process";
		public const string ARStatementDetails = "Statements History - Details by Date Inquiry";
		public const string ARStatementPrint = "Customer Statement Printing Process";
		public const string ARStatementForCustomer = "Statements History - Details by Customer Inquiry";
		public const string ARStatementHistory = "Statements History - Summary Inquiry";
		public const string ARStatementMaint = "Statement Cycle Maintenance";
		public const string SalesPersonMaint = "Sales Person Maintenance";
		public const string ARSPCommissionProcess = "Sales Person Commission Preparation Process";
		public const string ARSPCommissionDocEnq = "Sales Person Commission - Details Inquiry";
		public const string ARSPCommissionReview = "Sales Person Commission Period Closing Process";
		public const string ARFinChargesApplyMaint = "Overdue Charges Calculation Process";
		public const string ARFinChargesMaint = "Overdue Charge Codes Maintenance";
		public const string ARIntegrityCheck = "Customer Balances Validation Process";
		public const string ARScheduleMaint = "AR Scheduled Tasks Maintenance";
		public const string ARScheduleProcess = "AR Scheduled Tasks Process";
		public const string ARScheduleRun = "AR Sheduled Tasks Processing List";
		public const string ARSetupMaint = "Accounts Receivables Setup";
		public const string ARCCPaymentProcessing = "Credit Card Payments Processing";
		public const string ARExpiringCreditCardsEnq = "Expiring Credit Cards Inquiry";
		public const string ARSmallCreditWriteOffEntry = "Small Credit Write-Off Creation";
		public const string ARSmallBalanceWriteOffEntry = "Small Balance Write-Off Creation";
		public const string StatementCreateBO = "Statement Creation";
		public const string ARSPCommissionUpdate = "Commission History Creation";
		public const string ARTempCrLimitMaint = "Temporary Credit Limit Maintenance";
		public const string CCTransactionsHistoryEnq = "Credit Card Transactions History";
		public const string ARFailedCCPaymentTransEnq = "Payment Processing Log";
		public const string ARPriceClassMaint = "Customer Price Class Maintenance";
		public const string ARInvoice = "AR Invoice/Memo";
		public const string ARTran = "AR Transactions";
		public const string ARAddress = "AR Address";
		public const string ARContact = "AR Contact";
		public const string ChargeARInvoice = "Charge AR Invoices";
		public const string CaptureARPayment = "Capture AR Payments";
		public const string ARCustomerPaymentMethodExpirationProcess = "Credit Card expiration processing";
		public const string ARExpiredCreditCardsProcess = "Expired Credit Card Processing";
		public const string ARExpiringCreditCardsProcess = "Expiring Credit Card Notification Processing";
		public const string CCExpirationNotifyAll = "Notify All";
		public const string CCExpirationNotify = "Notify";
		public const string CCDeactivateAll = "Deactivate all";
		public const string CCDeactivate = "Deactivate";
        public const string DunningLetter = "Dunning Letter";
        public const string ProcessDL = "Process Dunning Letter";
        public const string CustomerCreditHold = "Customer Credit Hold Processing";
		public const string ARExternalTaxPost = "AR External Tax Posting";
		public const string ARExternalTaxPostProcess = "AR External Tax Post Process";
		public const string ARExternalTaxCalc = "AR External Tax Posting";
		public const string ARExternalTaxCalcProcess = "AR External Tax Post Process";
        public const string ARSetup = "Account Receivable Preferences";

		#endregion 

		#region DAC Names
        public const string CustomerPaymentMethodInfo = "Customer Payment Method";
		public const string CustomerPaymentMethod = "Customer Payment Method";
		public const string CustomerPaymentMethodDetail = "Customer Payment Method Detail";
		public const string ARSalesPerTran = "AR Salesperson Comission";
		public const string ARTaxTran = "AR Tax";
		public const string ARAdjust = "Applications";
		public const string ARPayment = "AR Payment";
		public const string CustSalesPeople = "Customer Salespersons";		
		public const string CustomerNotificationSource = "Customer Notification Source";
		public const string CustomerNotificationRecipient = "Customer Notification Recipient";
		public const string CustomerBalanceSummary = "Balance Summary";
		public const string ARCashSale = "Cash Sale";
		public const string CustomerClass = "Customer Class";
		public const string StatementCycle = "Statement Cycle";

        public const string ARBalanceByCustomer = "AR Balance by Customer";
        public const string CustomerHistory = "Customer History";
        public const string ARAgedPastDue = "AR Aged Past Due";
        public const string ARAgedOutstanding = "AR Aged Outstanding";
        public const string ARRegister = "AR Register";
        public const string CustomerDetails = "Customer Details";
        public const string DocumentSelection = "AR Document to Process";
        public const string ARDocument = "AR Document";

        public const string ARInvoiceDiscountDetail = "AR Invoice Discount Detail";

		#endregion

		#region Document Type
		public const string Register = "Register";		
		public const string Invoice = "Invoice";
		public const string DebitMemo = "Debit Memo";
		public const string CreditMemo = "Credit Memo";
		public const string Payment = "Payment";
		public const string Prepayment = "Prepayment";
		public const string Refund = "Customer Refund";
		public const string VoidPayment = "Void Payment";
		public const string FinCharge = "Overdue Charge";
		public const string SmallBalanceWO = "Balance WO";
		public const string SmallCreditWO = "Credit WO";
		public const string DeferredRevenue = "Deferred Revenue";
		public const string CashSale = "Cash Sale";
		public const string CashReturn = "Cash Return";
		public const string NoUpdate = "No Update";
		public const string SalesOrderPrefix = "SO - ";

		
		#endregion
		
        #region Recalculate Discounts Options
        public const string CurrentLine = "Current Line";
        public const string AllLines = "All Lines";
		#endregion

		#region Report Document Type
		public const string PrintInvoice = "INVOICE";
		public const string PrintDebitMemo = "DRMEMO";
		public const string PrintCreditMemo = "CRMEMO";
		public const string PrintPayment = "PAYMENT";
		public const string PrintPrepayment = "PREPAYMENT";
		public const string PrintRefund = "REFUND";
		public const string PrintVoidPayment = "VOIDPAY";
		public const string PrintFinCharge = "FINCHG";
		public const string PrintSmallBalanceWO = "BALANCE WO";
		public const string PrintSmallCreditWO = "CREDIT WO";
		public const string PrintCashSale = "CASH SALE";
		public const string PrintCashReturn = "CASH RET";
		#endregion

		#region Document Status
		public const string Hold = "On Hold";
		public const string Balanced = "Balanced";
		public const string Voided = "Voided";
		public const string Scheduled = "Scheduled";
		public const string Open = "Open";
		public const string Closed = "Closed";
		public const string PendingPrint = "Pending Print";
		public const string PendingEmail = "Pending Email";
		public const string CCHold = "Pending CC Processing";
		public const string CreditHold = "Credit Hold";
		#endregion

		#region AR Mask Codes
		public const string MaskItem = "Non-Stock Item";
		public const string MaskLocation = "Customer Location";
		public const string MaskEmployee = "Employee";
		//public const string MaskCompany = "Company Location";
        public const string MaskCompany = "Branch";
        public const string MaskSalesPerson = "Salesperson";
		#endregion

		#region Commission Period Type
		public const string Monthly = "Monthly";
		public const string Quarterly = "Quarterly";
		public const string Yearly = "Yearly";
		public const string FiscalPeriod = "By Financial Period";
		#endregion

		#region PMInstanceSearchType

		public const string PMInstanceSearchByPartialNumber = "Search by Partial Number";
		public const string PMInstanceSearchByFullNumber = "Search By Full Number";
		
		#endregion

		#region Commission Period Status
		public const string PeriodPrepared = "Prepared";
		public const string PeriodOpen = "Open";
		public const string PeriodClosed = "Closed";
		#endregion

		#region SPCommnCalcTypes
		public const string ByInvoice = "Invoice";
		public const string ByPayment = "Payment";
		#endregion

		#region CCProcessingState
		public const string	CCNone = "None";
		public const string	CCPreAuthorized ="Pre-Authorized";
		public const string	CCPreAuthorizationFailed = "Pre-Authorization Failed";
		public const string	CCCaptured = "Captured";
		public const string	CCCaptureFailed = "Capture Failed";
		public const string	CCVoided = "Voided";
		public const string CCVoidFailed = "Voiding failed";
		public const string	CCRefunded = "Refunded";
		public const string	CCRefundFailed = "Refund Failed";
		public const string	CCPreAuthorizationExpired = "Pre-Authorization Expired";
		#endregion

		#region Custom Actions
		public const string	ViewCustomer = "View Customer";
		public const string	ViewPaymentMethod = "View Payment Method";
		public const string ViewPayment= "View Payment";
		public const string ViewDocument = "View Document";
        public const string ViewOrigDocument = "View Orig. Document";
		public const string ProcessPrintInvoice = "Print Invoices";
		public const string ProcessEmailInvoice = "Email Invoices";
		public const string ProcessPrintStatement = "Print Statement";
		public const string ProcessEmailStatement = "Email Statement";
		public const string ProcessMarkDontEmail = "Mark as Do not Email";
		public const string ProcessMarkDontPrint = "Mark as Do not Print";
        public const string RegenerateStatement = "Regenerate Statement";
        public const string CustomerStatementHistory = "Customer Statement History";

        public const string ProcessPrintDL = "Print Dunning Letter";
        public const string ProcessEmailDL = "Email Dunning Letter";

		public const string RegenerateLastStatement = "Regenerate Last Statement";
		#endregion

        #region Report Names
        public const string CustomerStatement = "Customer Statement";
        public const string CustomerStatementMC = "Customer Statement MC";
        #endregion

        #region DiscountAppliedTo
        public const string ExtendedPrice = "Item Extended Price";
        public const string SalesPrice = "Item Price";
        #endregion

		#region Price Option
		public const string PriceClass = "Price Class";
		#endregion

		#region Price Basis
		public const string LastCost = "Last Cost + Markup %";
		public const string StdCost = "Avg./Std. Cost + Markup %";
		public const string CurrentPrice = "Current Price";
		public const string PendingPrice = "Pending Price";
		public const string RecommendedPrice = "MSRP";
		#endregion

        public const string DueDateRefNbr = "Due Date, Reference Nbr.";
		public const string DocDateRefNbr = "Doc. Date, Reference Nbr.";
        public const string RefNbr = "Reference Nbr.";
		public const string OrderNbr = "Order Nbr.";
		public const string OrderDateOrderNbr = "Order Date, Order Nbr.";

        public const string CompleteLine = "Complete Line";
        public const string IncompleteLine = "Bill Later";

		public const string LabelCury = "If copying to a different currency, select the rate type for currency conversion";
	}
}
