using System;
using PX.Data;
using PX.Common;

namespace PX.Objects.CA
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		// Add your messages here as follows (see line below):
		// public const string YourMessage = "Your message here.";
		#region Validation and Processing Messages
		public const string Prefix = "CA Error";
		public const string CATranNotSaved = "An error occurred while saving CATran for the table '{0}'";
		public const string DocumentOutOfBalance = AP.Messages.DocumentOutOfBalance;
		public const string DocumentStatusInvalid = AP.Messages.Document_Status_Invalid;
		public const string DuplicatedPaymentTypeDetail = "Record already exists.";
		public const string DuplicatedPaymentMethodDetail = "Record already exists.";
		public const string CashAccount_PaymentTypes_Undefined = "At least one Payment Method must be defined for the Cash Account.";
		public const string CashAccount_MayBeCreatedFromDenominatedAccountOnly = "Only denominated GL Account can be converted to Cash type.";
		public const string DocAllreadyExists = "Reconciliation Statement for this date is exist.";
		public const string OpenDocAllreadyExists = "There is an open Reconciliation Statement exists for this Cash Account. You must close the open statement first.";
		public const string ReleasedDocCanNotBeDel = "Released document cannot be deleted.";
		public const string TransferDocCanNotBeDel = "This transaction cannot be deleted. Use Cash Transfer Entry screen.";
		public const string TransferCAAreEquals = "Destination Cash Account must be different from the source.";
		public const string UnreleasedTrxInRecon = "Reconciliation cannot be completed as one or more unreleased document exists in this statement.";
		public const string GLTranExistForThisCashAcct = "One or more GL transactions that are not reflected in Cash Management module was found. Run Validate Accounts Balances process to synchronize balances.";
		public const string CantEditDisbReceipt = "You cannot change the Type for the Entry Type if one or more transactions was entered already.";
		public const string CantEditModule = "You cannot change the Module for the Entry Type if one or more transactions was entered already.";
		public const string PaymentMethodDefaultAccountCantBeDeleted = "Default Cash Account cannot be deleted.";
		public const string DuplicatedKeyForRow = "Record with this ID already exists.";
		public const string ERR_CashAccountHasTransactions_DeleteForbidden = "This Cash Account cannot be deleted as one or more transaction already exists.";
		public const string ERR_CashAccountIsUsed_DeleteForbidden = "This Cash Account is used somewhere in the system and cannot be deleted.";
		public const string ERR_IncorrectFormatOfPTInstanceExpiryDate = "Incorrect date format provided.";
		public const string ERR_RequiredValueNotEnterd = "This field is required.";
		public const string ValueIsNotValid = "Provided value does not pass validation rules defined for this field.";
		public const string ProcessingCenterIsAlreadyAssignedToTheCard = "This Processing Center is already assigned to the Payment Method";
		public const string PaymentMethodIsAlreadyAssignedToTheProcessingCenter = "This Payment Method is already assigned to the Processing Center";
		public const string RowIsDuplicated = "Row is duplicated";
		public const string RequiresReconNumbering = "Requires Reconciliation Numbering";
		public const string SubIDValidateFailed = "Sub CD validated failed, because there is GLHistory with such Account and another SubCD";
		public const string EntryTypeIDDoesNotExist = "This Entry Type ID does not exist";
		public const string TransactionNotComplete = "Transaction Not Complete";
		public const string TransactionNotFound = "Cash Transaction Not Found";
		public const string DocumentOnHold = "Document On Hold";
		public const string OneOrMoreItemsAreNotReleased = "One or more items are not released";
		public const string OneOrMoreItemsAreNotReleasedAndStatementCannotBeCompleted = "One or more items are not released and statement cannot be completed";
		public const string DocNotFound = "Document Not Found";
		public const string GLTranCanNotBeReleasedFromCAModule = "GL Tran Can Not Be Released From CA Module";
		public const string APDocumentsCanNotBeReleasedFromCAModule = "AP Documents Can Not Be Released From CA Module";
		public const string ARDocumentsCanNotBeReleasedFromCAModule = "AR Documents Can Not Be Released From CA Module";
		public const string TheDocumentHasBeenAlreadyPublished = "The document has been already published";
		public const string ThisDocTypeNotAvailableForRelease = "This Doccument Type Not Available For Release";
		public const string OriginalDocAlreadyReleased = "Original document has already been released";
		public const string PaymentTypeNotValid = "Payment Type Not Valid";
		public const string CanNotVoidStatement = "There are newer non-voided statements.";
		public const string CanNotCreateStatement = "Can not create statement - current statement is not reconciled.";
		public const string CashAccounNotReconcile = "CashAccount Not Reconcile";
		public const string ReconciledDocCanNotBeNotCleared = "Reconciled document can not be not cleared";
		public const string ImportProcessingCenterSettings = "Import Settings from Processing Type";
		public const string ProcessingCenterIDIsRequiredForImport = "Processing CenterID is required for this operation";
		public const string ProcessingObjectTypeIsNotSpecified = "Type of the object for the Credit Card processing is not specified";
		public const string InstanceOfTheProcessingTypeCanNotBeCreated = "Instance of the Type {0} can't be created";
		public const string PaymentMethodAccountIsInUseAndCantBeDeleted = "This Cash Account is used  in one or more Customer Payment Methods and can not be deleted";
		public const string PaymentMethodIsInUseAndCantBeDeleted = "This Payment Method is used in one or more Customer Payment Methods and can not be deleted";
		public const string NoOneDocumentSelectToRelease = "Nothing Documents selected to release";
		public const string CashAccountMayNotBeMadeClearingAccount = "Only Cash Account withount Payment Methods, Clearing Accounts and Bank may be configured as Clearing Account";
		public const string DontHaveAppoveRights = "You don't have access rights to approve document.";
		public const string DontHaveRejectRights = "You don't have access rights to reject document.";
		public const string CABatchExportProviderIsNotConfigured = "This Batch  can not be exported - it's Payment Method is not configured properly";
		public const string ReleasedDocumentMayNotBeAddedToCABatch = "This document is released and can not be added to the batch";
		public const string ReleasedDocumentMayNotBeDeletedFromCABatch = "This document is released and can not be deleted from the batch";
		public const string CABatchDefaultExportFilenameTemplate = "{0}-{1}-{2:yyyyMMdd}{3:00000}.txt";  //Do not translate this message, only change it if required
		public const string CABatchStatusIsNotValidForProcessing = "Document status is not valid for processing";
		public const string CABatchContainsUnreleasedPaymentsAndCannotBeReleased = "This  batch contains unreleased payments. It can'not be released untill all the payments are released successfully";
		public const string DateSeqNumberIsOutOfRange = "Date Sequence Number is out of range";
		public const string DocumentOnHoldCanNotBeReleased = "Statement on Hold can't be released";
		public const string DocumentIsUnbalancedItCanNotBeReleased = "Statement is not balanced";
		public const string StatementCanNotBeReleasedSomeDetailsMatchedDeletedDocument = "Statement can not be released - same of the details matched deleted document";
		public const string StatementCanNotBeReleasedThereAreUnmatchedDetails = "Statement can not be released - same of the details are not matched";
		public const string StatementCanNotBeReleasedSomeDetailsMatchUnreleasedDocuments = "Statement can not be released - some of the details match unreleased document";
		public const string PaymentTypeIsRequiredToCreateAPDocument = "Payment Method is required to create an AP Document";
		public const string PaymentMethodIsRequiredToCreateARDocument = "Payment Method is required to create an AP Document";
		public const string EntryTypeIsRequiredToCreateCADocument = "Entry Type is required to create a CA Document";
		public const string PayeeLocationIsRequiredToCreateAP_ARDocument = "Customer/Vendor Location is required to create a Document";
		public const string PayeeIsRequiredToCreateAP_ARDocument = "Customer/Vendor is required to create a Document";
		public const string DocumentIsAlreadyCreatedForThisDetail = "Document is already created!";
		public const string StatementEndDateMustBeGreaterThenStartDate = "End Balance Date should be greater then Start Balance Date";
		public const string StatementEndBalanceDateIsRequired = "End Balance Date is required";
		public const string StatementStartBalanceDateIsRequired = "Start Balance Date is required";
		public const string StatementIsOutOfBalanceThereAreUnmatchedDetails = "Statement is out of balance - there are unmatched details";
		public const string StatementIsOutOfBalanceEndBalanceDoesNotMatchDetailsTotal = "Statement is not balanced - end balance does not match details total";																						
		public const string StatementDetailIsAlreadyMatched = "This detail is already matched with another CA transaction";
		public const string CashAccountWithExtRefNbrIsNotFoundInTheSystem = "Account with Ext Ref Number {0} is not found in the system";
		public const string CashAccountHasCurrencyDifferentFromOneInStatement = "Account {0} has currency {1} different from one specified in the statement. Statement can not be imported. Please, check correctness of the cash account's Ext Ref Nbr and other settings";
		public const string TransactionWithFitIdHasAlreadyBeenImported = "Transaction with FITID {0} is found in the existing Statement: {1} for the Account: {2}-'{3}'. Most likely, this file has already been imported";
		public const string OFXImportErrorAccountInfoIsIncorrect = "Account information in the file is invalid or has an unsupported format";
		public const string OFXParsingErrorTransactionValueHasInvalidFormat = "The Value {0} for the Field {1} in the transaction {2} has invalid format: {3}";
		public const string OFXParsingErrorValueHasInvalidFormat = "The Field {0} has invalid format: {1}";
		public const string OFXUnsupportedEncodingDetected = "Unsupported Encoding {0} or Charset (1) detected in the header";
		public const string UnsavedDataInThisScreenWillBeLostConfirmation = "Unsaved data in this screen will be lost. Continue?";
		public const string ImportConfirmationTitle = "Confirmation";
		public const string ViewResultingDocument = "View Resulting Document";
		public const string DuplicatedPaymentMethodForCashAccount = "Payment method '{0}' is already added to this Cash Account";
		public const string DuplicatedCashAccountForPaymentMethod = "Cash Account '{0}' is already added to this Payment method";
		public const string APPaymentApplicationInvoiceIsNotReleased = "Invoice with number {0} is not released. Application can be made only to the released invoices";
		public const string APPaymentApplicationInvoiceIsClosed = "Invoice with number {0} is closed.";
		public const string APPaymentApplicationInvoiceDateIsGreaterThenPaymentDate = "Invoice with the number {0} is found, but it's date is greater then date of the transaction. It can not  be used for the payment application";
		public const string APPaymentApplicationInvoiceUnrealeasedApplicationExist = "There are unreleased applications to the Invoice number {0}. It can not be used for this payment application.";
		public const string APPaymentApplicationInvoiceIsPartOfPrepaymentOrDebitAdjustment = "Invoice with the number {0} is found, but there it's used in prepayment or debit adjustment. It can not be used for this payment application.";
		public const string APPaymentApplicationInvoiceIsNotFound =  "Invoice number {0} match neither internal nor external Invoice numbers registered in the system. You need to enter this invoice before the application.";
		public const string ARPaymentApplicationInvoiceIsNotReleased = "Invoice with number {0} is not released. Application can be made only to the released invoices";
		public const string ARPaymentApplicationInvoiceIsClosed = "Invoice with number {0} is closed.";
		public const string ARPaymentApplicationInvoiceDateIsGreaterThenPaymentDate = "Invoice with the number {0} is found, but it's date is greater then date of the transaction. It can not  be used for the payment application";
		public const string ARPaymentApplicationInvoiceUnrealeasedApplicationExist = "There are unreleased applications to the Invoice number {0}. It can not be used for this payment application.";
		public const string ARPaymentApplicationInvoiceIsNotFound = "Invoice number {0} match neither internal nor external Invoice numbers registered in the system. You need to enter this invoice before the application.";
		public const string CASomeOfDocumentsCanNotBereconciledTheyAreNotReleased = "Some of the listed documents may not be reconciled - they are not released.";
		public const string CAFinPeriodRequiredForSheduledTransactionIsNotDefinedInTheSystem = "A scheduled document {0} {1} {2} assigned to the Schedule {3} needs a financial period, but it's not defined in the system";
		public const string FinPeriodsAreNotDefinedForDateRangeProvided = "Financial periods are not defined for the date range provided. Scheduled documents may not be included correctly";
		public const string CurrencyRateIsRequiredToConvertFromCuryToBaseCuryForAccount = "A currency rate for conversion from Currecy {0} to Base Currency {1} is not found for account{2}";
		public const string RowMatchesCATranWithDifferentExtRefNbr = "This row is matched with the document having not exactly matching Ext. Ref. Nbr.";
		public const string MatchingCATransIsNotValid = "Matching Transaction is not valid. Probably, it has been deleted";
		public const string RowsWithSuspiciousMatchingWereDetected = "There were detected {0} rows, having not exact matching";
		public const string CABatchContainsVoidedPaymentsAndConnotBeReleased = "This Batch Payments contains voided documents. You must remove them to be able to release the Batch";
		public const string CAEntryTypeUsedForPaymentReclassificationMustHaveCashAccount = "Entry Type which is used for Payment Reclassification must have a Cash Account as Offset Account";
		public const string EntryTypeCashAccountIsNotConfiguredToUseWithAnyPaymentMethod = "This Cash Account is not configured for usage with any Payment Method. Please, check the configuration of the Payment Methods before using the Payments Reclassifications";
		public const string OffsetAccountForThisEntryTypeMustBeInSameCurrency = "Offset account must be a Cash Account in the same currency as current Cash Account";
		public const string OffsetAccountMayNotBeTheSameAsCurrentAccount = "Offset account may not be the same as current Cash Account";
		public const string NoActivePaymentMethodIsConfigueredForCashAccountInModule = "There is no active Payment Method which may be used with account '{0}' to create documents for Module '{1}'. Please, check the configuration for the Cash Account '{0}'.";
		public const string EntryTypeRequiresCashAccountButNoOneIsConfigured = "This Entry Type requires to set a Cash Account with currency {0} as an Offset Account. Currently, there is no such a Cash Account defined in the system";
		public const string UploadFileHasUnrecognizedBankStatementFormat = "This file format is not supported for the bank statement import. You must create an import scenario for this file extention prior uploading it.";

		public const string StatementServiceReaderCreationError = "A Statement Reader Service of a type {0} is failed to create";
		public const string CashAccountMustBeSelectedToImportStatement = "You need to select a Cash Account, for which a statement will be imported";
		public const string StatementImportServiceMustBeConfiguredForTheCashAccount = "You have to configure Statement Import Service for the selected Cash Account. Please, check account settings in the 'Cash Accounts' interface";
		public const string StatementImportServiceMustBeConfiguredInTheCASetup = "You have to configure Statement Import Service. Please, check 'Bank Statement Settings' section in the 'Cash Management Preferences'";
		public const string ImportedStatementIsMadeForAnotherAccount = "The Statement in the file selected is created for another account: {0}-'{1}'. Please, select a correct file";
		public const string CashAccountExist = "Cash account for this account, sub account and branch already exist";
		public const string SomeRemittanceSettingsForCashAccountAreNotValid = "Some Remittance Settings for this Payment Method have invalid values. Please Check.";
		public const string SomeSettingsAreInvalidSaveFailed = "Save failed - some settings for this Cash Account are incorrect";
		public const string WrongSubIdForCashAccount = "Wrong sub account for this account";
		public const string DocumentMustByAppliedInFullBeforeItMayBeCreated = "Applications must be entered for full amount of this document before it may be created";
		public const string TranCuryNotMatchAcctCury = "Transaction's Currency does not Match CashAccount's Currency";
		public const string CryptoSettingsChanged = "Encryption settings were changed during last system update. To finalize changes please press save button manually.";
		public const string ShouldContainBQLField = "Parameter should cointain a BqlField!";
		public const string CouldNotInsertPMDetail = "Converter was not able to setup Payment Method Detail due to ID conflict. Please contact support for help.";
		public const string NoProcCenterSetAsDefault = "No processing center was set as default";

		#endregion

		#region Translatable Strings used in the code
		public const string CardNumber = "Card Number";
		public const string ExpirationDate = "Expiration Date";
		public const string NameOnCard = "Name on the Card";
		public const string CCVCode = "Card Verification Code";
		public const string CCPID = "Payment Profile ID";
		public const string ReportID = "Report ID";
		public const string ReportName = "Report Name";
		public const string Day = "Day";
		public const string Week = "Week";
		public const string Month = "Month";
		public const string Period = "Financial Period";
		public const string ViewExpense = "View Expense";
		public const string DocumentCount = "Document Count";
		public const string AutoReconcile = "Auto-Reconcile";
		public const string Release = "Release";
		public const string Void = "Void";
		public const string AddARPayment = "Add Payment";		
		public const string ViewBatch = "View Batch";
		public const string ViewDetails = "View Details";
		public const string Reports = "Reports";
		public const string CashTransactions = "Cash Transactions";
		public const string Approval = "Approval";
		public const string Approved = "Approved";
		public const string Export = "Export";
		public const string MatchSelected = "Match Selected";
		public const string ImportStatement = "Import";
		public const string ViewDocument = "View Document";
		public const string ClearMatch = "Clear Match";
		public const string CreateAllDocuments = "Create All";
		public const string CreateDocument = "Create Document";
		public const string UploadFile = "Upload File";
		public const string MatchSettings = "Match Settings";
		public const string CashForecastTran = "Cash Forecast Transactions";
		public const string CashForecastReport = "Cash Flow Forecast Report";
		public const string ViewAsReport = "View As a Report";
		public const string ViewAsTabReport = "View As a Tab Report";
		public const string DateFormat = "yyyy-MM-dd";
        public const string AccountDescription = "Account Description";

		#endregion

		#region Graphs Names
		public const string CABalValidate = "Cash Account Validation Process";
		public const string CADocPrepare = "Reconciliation Statement Preparation Process";
		public const string CAReconEntry = "Reconciliation Statement Entry";
		public const string CAReconEnq = "Reconciliation Statement Summary Entry";
		public const string CASetup = "Cash Management Preferences";
		public const string CASetupMaint = "Setup Cash Management";
		public const string CashAccountMaint = "Cash Account Maintenance";
		public const string CashTransferEntry = "Cash Transfer Entry";
		public const string CATranEnq = "Cash Transactions Summary Entry";
		public const string CATranEntry = "Cash Transaction Details Entry";
				public const string CATranApproval = "Cash Transaction Approval";
		public const string CATrxRelease = "Cash Transactions Release Process";
		public const string EntryTypeMaint = "Entry Types Maintenance";
		public const string PaymentTypeMaint = "Payment Types Maintenance";
		public const string PaymentMethodMaint = "Payment Methods Maintenance";
		public const string CCProcessingCenter = "Processing Center";
		public const string PaymentTypeInstanceMaint = "Corporate Cards Maintenance";
		public const string CAReleaseProcess = "Cash Transactions Release";
		public const string CADepositEntry= "Payment Deposits";
		public const string CAPaymentReclassificationProcessing = "CA Payment Reclassification Processing";
		public const string CABatchEntry = "Payment Batch Entry";
		public const string CABankStatementEntry = "Bank Statement Entry";
		public const string CashFlowForecastEnq = "Cash Flow Forecast Enquiry";
		public const string CashForecastEntry = "Cash Flow Forecast Enquiry";

		#endregion

		#region DAC Names
		public const string CABatch = "CA Batch";
		public const string CABatchDetail = "CA Batch Details";
		public const string PaymentMethodDetail = "Payment Method Detail";
		public const string CARecon = "Reconciliation Statement";
		public const string PaymentMethod = "Payment Method";
		public const string CashAccount = "Cash Account";
		public const string PaymentType = "Payment Type";
		public const string CashFlowForecast = "Cash Flow Forecast Record";
		public const string CashFlowForecast2 = "Cash Flow Forecast Record";
		#endregion

		#region Tran Type
		public const string CATransferOut = "Transfer Out";
		public const string CATransferIn = "Transfer In";
		public const string CATransferExp = "Expense Entry";
		public const string CATransfer = "Transfer";
		public const string CAAdjustment = "Cash Entry";
		public const string GLEntry = "GL Entry";
		public const string CADeposit = "CA Deposit";
		public const string CAVoidDeposit = "CA Void Deposit";
		
		#endregion

		#region CA Transfer & CA Deposit Status
		public const string Balanced = "Balanced";
		public const string Hold = "On Hold";
		public const string Released = "Released";
		public const string Pending = "Pending Approval";
		public const string Rejected = "Rejected";
		public const string Voided = "Voided";
		public const string Exported = "Exported";

		#endregion

		#region Dr Cr Type
		public const string CADebit = "Receipt";
		public const string CACredit = "Disbursement";
		#endregion

		#region CA Modules
		public const string ModuleAP = "AP";
		public const string ModuleAR = "AR";
		public const string ModuleCA = "CA";
		#endregion

		#region CA Reconcilation
		public const string ReconDateNotAvailable = "Reconciliation date must be later than the date of the previous released Reconciliation Statement";
		public const string PrevStatementNotReconciled = "Previous Statement Not Reconciled";
		public const string LastDateToLoadNotAvailable = "Last Date to load must be greater or equal then reconciliation date";
		public const string HoldDocCanNotAddToReconciliation = "Document on hold cannot be added to reconciliation.";
		public const string NotReleasedDocCanNotAddToReconciliation = "Unreleased document cannot be added to Reconciliation.";
		public const string HoldDocCanNotBeRelease = "Document on hold cannot be released";
		public const string ClearedDateNotAvailable = "Clear Date NOT Available;";
		public const string PrevStatementHasGreaterOREqualDate = "Previos Statement Has Greater Or Equal Date";
		public const string OrigDocCanNotBeFound = "Orig. Document Can Not Be Found";
		public const string ThisCATranOrigDocTypeNotDefined = "This CATran Orig. Document Type Not Defined";
		public const string ReloadRecon = "Please reload the reconciliation record.";
		public const string VoidPendingStatus = "Void Pending";
		public const string VoidedTransactionHavingNotReleasedVoidCannotBeAddedToReconciliation = "This transaction has a voiding transaction which is not released. It may not be added to the reconciliation";
		public const string TransactionsWithVoidPendingStatusMayNotBeAddedToReconciliation = "Transactions having a 'Void Pending' status may not be added to the reconciliation";
		#endregion

		#region OFXFileReader
		public const string  ContentIsNotAValidOFXFile="Provided content is not recognized as a valid OFX format";
		public const string UnknownFormatOfTheOFXHeader = "Unrecognized format of the message header";
		public const string OFXDocumentHasUnclosedTag = "Document has invalid format - tag at position {0} is missing closing bracket (>)";
		

		#endregion
		#region
		public const string PeriodHasUnreleasedDocs = "Period has Unreleased Documents";
		public const string PeriodHasHoldDocs		= "Period has Hold Documents";
		#endregion
	}
}
