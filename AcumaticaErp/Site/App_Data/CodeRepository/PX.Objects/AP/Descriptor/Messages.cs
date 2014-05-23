using System;
using PX.Data;
using PX.Common;

namespace PX.Objects.AP
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		// Add your messages here as follows (see line below):
		// public const string YourMessage = "Your message here.";
		#region Validation and Processing Messages
		public const string Prefix = "AP Error";
		public const string InternalError = IN.Messages.InternalError;
		public const string SheduleNextExecutionDateExceeded = GL.Messages.SheduleNextExecutionDateExceeded;
		public const string SheduleExecutionLimitExceeded = GL.Messages.SheduleExecutionLimitExceeded;
		public const string SheduleHasExpired = GL.Messages.SheduleHasExpired;
		public const string BatchesNotReleased = "One or more batches failed to release.";
		public const string Entry_LE = "Entry must be less or equal to {0}";
		public const string Entry_GE = "Entry must be greater or equal to {0}";
		public const string Document_Status_Invalid = "Document Status is invalid for processing.";
		public const string Document_OnHold_CannotRelease = "Document is On Hold and cannot be released.";
		public const string Check_NotPrinted_CannotRelease = "Check is not Printed and cannot be released.";
        public const string ZeroCheck_CannotPrint = "Zero Check cannot be Printed or Exported.";
        public const string UnknownDocumentType = "Document type is unknown it cannot be processed.";
		public const string Only_Invoices_MayBe_Payed = "Only Bills and Credit Adjustments can be selected for payment.";
		public const string Only_Open_Documents_MayBe_Processed = "Only Open documents can be selected for payment.";
		public const string VoidAppl_CheckNbr_NotMatchOrigPayment = "Void Check must have the same Reference Number as the voided payment.";
		public const string ApplDate_Less_DocDate = "{0} cannot be less than Document Date.";
		public const string ApplPeriod_Less_DocPeriod = "{0} cannot be less than Document Financial Period.";
		public const string ApplDate_Greater_DocDate = "{0} cannot be greater than Document Date.";
		public const string ApplPeriod_Greater_DocPeriod = "{0} cannot be greater than Document Financial Period.";
		public const string AP1099_Vendor_Cannot_Have_Discounts = "Terms discounts are not allowed for 1099 Vendors.";
		public const string AP1099_Vendor_Cannot_Have_Multiply_Installments = "Multiple Installments are not allowed for 1099 Vendors.";
		public const string AP1099_PaymentDate_NotIn_OpenYear = "Payment date {0} must fall into open 1099 Year.";
		public const string Employee_Cannot_Have_Discounts = "Terms discounts are not allowed for Employees.";
		public const string Employee_Cannot_Have_Multiply_Installments = "Multiple Installments are not allowed for Employees.";
		public const string DocumentBalanceNegative = "Document Balance will go negative. Document will not be released.";
		public const string PrepaymentNotPayedFull = "Prepayment '{0}' is not paid in full. Document will not be released.";
		public const string DocumentOutOfBalance = "Document is out of balance.";
		public const string QuickCheckOutOfBalance = "Payment amount should be less or equal to invoice amount.";
		public const string DocumentApplicationAlreadyVoided = "This document application is already voided. Document will not be released.";
		public const string PrepaymentCannotBeVoided = "Prepayment cannot be voided. Document will not be released.";
		public const string PrepaymentCannotBeVoided2 = "Prepayment cannot be voided. Document has unreleased application records. Document will not be released.";
		public const string PrepaymentCannotBeVoided3 = "Prepayment cannot be voided. Is payed by check {0}.";
		public const string AskUpdateLastRefNbr = "Do you want to update Last Reference Number with entered number?";
		public const string CheckCuryNotPPCury = "Payment currency cannot be different from the selected Prepayment currency.";
		public const string CashCuryNotPPCury = "Cash Account currency cannot be different from Prepayment currency.";
		public const string PaymentTypeNoPrintCheck = "This Payment Method is not configured to print checks.";
		public const string VendorCuryNotPPCury = "Vendor Cash Account currency is different from the Prepayment currency. Prepayment document will not be selected for Payment.";
		public const string VendorMissingCashAccount = "Cash Account is not set up for Vendor.";
		public const string VendorCuryDifferentDefPayCury = "Vendor currency is different from the default Cash Account Currency.";
		public const string DuplicateVendorRefDoc = "Document with this Vendor Ref. already exists for this Vendor - see document with reference number '{0}' dated '{1:d}' .";
		public const string VendorClassChangeWarning = "Please confirm if you want to update current Vendor settings with the Vendor Class defaults. Original settings will be preserved otherwise.";
		public const string MultipleApplicationError = "Multiple applications exists for this document. Please reverse these applications individually and then void the document.";
		public const string PPVSubAccountMaskCanNotBeAssembled = "PPV Subaccount mask cannot be assembled correctly. Please, check settings for the Inventory Posting Class";
		public const string TaxVendorDeleteErr = "Cannot delete Tax Vendor. There are Taxes associated with this Vendor.";
		public const string Quick_Check_Cannot_Have_Multiply_Installments = "Multiple Installments are not allowed for Quick Checks.";
		public const string Multiply_Installments_Cannot_be_Reversed = "Multiple installments bill cannot be reversed, Please reverse original bill '{0}'.";
		public const string Check_Cannot_Unhold_Until_Printed = "Cannot remove from hold until check is printed.";
		public const string Application_Amount_Cannot_Exceed_Document_Amount = "Total application amount cannot exceeed document amount.";
		public const string ProcessingOfLandedCostTransForAPDocFailed = "Processing of the Landed Cost for one or more AP Documents has failed";
        public const string PrepaymentAppliedToMultiplyInstallments = "Prepayments cannot be applied to multiple-installment invoices.";
		public const string PaymnetTypeIsNotValidForCashAccount = "Payment Method specified is not valid for this Cash Account";
		public const string AccountIsSameAsDeferred = "Transaction Account is same as Deferral Account specified in Deferred Code.";
		public const string DebitAdjustmentRowReferecesPOOrderOrPOReceipt = "By reversing an AP invoice that was matched to a PO receipt, your PO Receipt will re-open and associated AP accrual account will be affected upon release of this document.";
		public const string QuantityBilledIsGreaterThenPOReceiptQuantity = "Quantity billed is greater then the quantity in the original PO Receipt for this row";
		public const string APLanededCostTranForNonLCVendor = "This vendor is not a landed cost vendor. He may not have Landed Cost included in the document";
		public const string APPaymentDoesNotMatchCABatchByAccountOrPaymentType = "One of the Payments in selection have wrong Cash Account or PaymentMethod";
		public const string APLandedCost_NoPOReceiptNumberSpecified = "At least one PO Receipt Number should be specified for this Landed Cost";
		public const string EmployeeClassExists = "This ID is already used for the Employee Class.";
		public const string VendorIsInStatus = "Vendor is {0}.";
		public const string VendorIsEmployee = "Vendor is employee.";
		public const string PaymentIsPayedByCheck = "Prepayment cannot be voided. Please void Check {0} instead.";
		public const string PaymentIsRefunded = "Prepayment has been refunded with Refund {0}.";
        public const string APPaymentsAreAddedToTheBatchButWasNotUpdatedCorrectly = "AP Payments has been successfully added to the Batch Payment {0}, but update of its statused failed.";
        public const string VendorClassCanNotBeDeletedBecauseItIsUsed = "This Vendor Class can not be deleted because it is used in AP Setup.";
		public const string NextCheckNumberIsRequiredForProcessing = "Next Check Number is required to print AP Payments with 'Payment Ref.' empty";
        public const string NextCheckNumberCanNotBeInc = "Next Check Number can't be incremented."; 
        public const string NextCheckNumberIsRequiredError = "Next Check Number is required to print this payment";
		public const string PeriodHasAPDocsFromPO_LCToBeCreated = "There one or more pending AP Bill originating from the existing Landed Cost transaction in PO module which belong to this period. They have to be created and released before the period may be closed in AP. Please, check the screen 'Process Landed Cost'(PO.50.60.00)";
		public const string ProcessSelectedRecordsTooltip = "Process Selected Records";
		public const string ProcessAllRecordsTooltip = "Process All Records";
		public const string PreliminaryAPExpenceBooking = "Preliminary AP Expense Booking";
		public const string PreliminaryAPExpenceBookingAdjustment = "Preliminary AP Expense Booking Adjustment";
        public const string PrebookingAccountIsRequiredForPrebooking = "Pre-releasing Account must be specified to perform Pre-releasing";
        public const string PrebookingSubAccountIsRequiredForPrebooking = "Pre-releasing Sub Account must be specified to perform Pre-releasing";
		public const string InvoicesWithMultipleInstallmentTermsMayNotBePrebooked = "Invoices with multiple installments terms may not be pre-released";
		public const string LinkToThePrebookingBatchIsMissing = "The document {0} {1} is marked as pre-released, but the link to the Pre-releasing batch is missed";
		public const string PrebookedDocumentsMayNotBeVoidedAfterTheyAreReleased = "Pre-released Documents can not be voided after they are released";
		public const string LinkToThePrebookingBatchIsMissingVoidImpossible = "The document {0} {1} is marked as pre-released, but the link to the Pre-releasing batch is missed. Void operation may not be made";
		public const string PrebookedDocumentMayNotBeVoidedIfPaymentsWereAppliedToIt = "Pre-released Document may not be voided if payment(s) has been applied to it";
        public const string PrebookingBatchDoesNotExistsInTheSystemVoidImpossible = "Pre-releasing batch {0} {1} may not be found in DB. Void operation is impossible.";
		public const string APTransactionIsNotFoundInTheReversingBatch = "AP Transaction is not found in the reversing batch";
		public const string TaxesForThisDocumentHaveBeenReportedVoidIsNotPossible = "Tax report has been created for the document {0} {1}. Void operation is impossible.";
		public const string ThisDocumentConatinsTransactionsLinkToPOVoidIsNotPossible = "Document conatains details, linked to document(s) in Purchase Order Module. Void Operation is not possible";
        public const string SomeChargeNotRelatedWithCashAccount = "Some Finance Charges have not related with selected Cash Account. Do you want to delete this Finance Charges?";
        public const string ReferenceNotValid = "Reference Number is not valid";
        public const string GroupUpdateConfirm = "Restriction Groups will be reset for all Vendors that belongs to this vendor class.  This might override the custom settings. Please confirm your action";
		public const string DocumentNotApprovedNotProceed = "Document is not approved for payment and will not be processed.";
        public const string DiscountCodeAlreadyExist = "Discount Code already exists.";
        public const string DocDiscDescr = "Group and Document Discount";
        public const string DiscountGreaterLineTotal = "Discount Total may not be greater than Line Total";
        public const string AccountMappingNotConfigured = "Account Task Mapping is not configured for the following Project: {0}, Account: {1}";
		public const string SameRefNbr = "{0} with reference number {1} already exists. Enter another reference number.";
		#endregion

		#region Translatable Strings used in the code
		public const string VendorViewLocation = "Location Details";
		public const string VendorNewLocation = "Add Location";
		public const string NewInvoice = "Enter New Bill";
		public const string NewPayment = "Enter New Payment";
		public const string MultiplyInstallmentsTranDesc = "Multiple Installment split";
		public const string AskConfirmation = "Confirmation";
		public const string Warning = "Warning";
		public const string ApprovedForPayment = "Approved for Payment";
		public const string DeferredExpenseTranDesc = "Deferred Expense Recognition";
		public const string ReprintCaption = "Reprint";
		public const string CloseYear = "Close Year";
		public const string Times = "(times)";
		public const string DocDateSelection = "Document Date Selection";
		public const string Periods = "Periods";
		public const string ViewAPDocument = "View AP Document";
		public const string Shipping = "Shipping";
		public const string Remittance = "Remittance";
		public const string ViewCustomer = "View Customer";
		public const string ViewBusnessAccount = "View Business Account";
		public const string ExtendToCustomer = "Extend To Customer";
		public const string LandedCostAccrualCorrection = "Landed Cost Accrual correction";
		public const string LandedCostVariance = "Landed Cost Variance";
		public const string AddPostponedLandedCost = "Add Postponed Landed Cost";
		public const string LandedCostSplit = "Landed Cost Split";
		public const string VendorID = "Vendor ID";
		public const string Approved = "Approved";
		public const string Approve = "Approve";
        public const string Year1099SummaryReport = "Year 1099 Summary";
        public const string Year1099DetailReport = "Year 1099 Detail";
        public const string APBalanceByVendorReport = "AP Balance by Vendor";
        public const string VendorHistoryReport = "Vendor History";
        public const string APAgedPastDueReport = "AP Aged Past Due";
        public const string APAgedOutstandingReport = "AP Aged Outstanding";
        public const string APRegisterReport = "AP Register";
		public const string ViewBatch = "View Batch";
		public const string ReverseApp = "Reverse Application";
		public const string ViewAppDoc = "View Application Document";
        public const string ViewAPDiscountSequence = "View Discount Sequence";
        #endregion

		#region Graph Names
		public const string APDocumentRelease = "Bills and Adjustments Release Process";
		public const string APReleaseProcess = "AP Release Process";
		public const string APInvoiceEntry = "Enter Bill";
		public const string APPaymentEntry = "Create Check";
		public const string APApproveBills = "Approve Bills for Payment";
		public const string APPayBills = "Pay Bills";
		public const string APPayBill = "Pay Bill";
		public const string APReverseBill = "Reverse Bill";
		public const string APPrintChecks = "Checks Printing Process";
		public const string APReleaseChecks = "Check Release Process";
		public const string APPaymentCreation = "AP Payment Creation";
		public const string APDocumentEnq = "Vendor Details";
		public const string APVendorBalanceEnq = "Vendor Balance Inquiry - Summary";
		public const string APPendingInvoicesEnq = "Bills Pending Payment Inquiry";
		public const string APChecksToPrintEnq = "Checks Pending to Process Inquiry";
		public const string APIntegrityCheck = "Vendor Balances Validation Process";
		public const string APScheduleMaint = "AP Scheduled Tasks Maintenance";
		public const string APScheduleProcess = "AP Scheduled Tasks Processing";
		public const string APScheduleRun = "AP Scheduled Tasks List";
		public const string APSetupMaint = "Setup Accounts Payable";
		public const string VendorClassMaint = "Vendor Class Maintenance";
		public const string VendorMaint = "Vendor Maintenance";
		public const string Vendor = "Vendor";
		public const string VendorLocation = "Vendor Location";
		public const string APAccess = "Vendor Access";
		public const string APAccessDetail = "Vendor Access Detail";
		public const string VendorLocationMaint = "Vendor Locations Maintenance";
		public const string AP1099DetailEnq = "1099 Details Inquiry";
		public const string AP1099SummaryEnq = "1099 Summary Inquiry";
		public const string APPayment = "Payment";
		public const string APAdjust = "Adjust";
		public const string APAdjustHistory = "Adjust History";
		public const string APDocumentLandedCostTranProcessing = "AP Invoice Landed Cost Processing";
        public const string LandedCostTranR = "Postponed Landed Cost";
        public const string LocationAPAccountSub = "Location GL Accounts";
        public const string LocationAPPaymentInfo = "Location Payment Settings";
        public const string APPaySelReport = "Bill For Payment";
        public const string APPayNotSelReport = "Bill For Approval";
        public const string APCashRequirementReport = "Cash Requirement";
		#endregion

		#region DAC Names
		public const string VendorPaymentTypeDetail = "Payment Type Detail";
		public const string VendorBalanceSummary = "Balance Summary";
		public const string VendorNotificationSource = "Vendor Notification Source";
		public const string VendorNotificationRecipient = "Vendor Notification Recipient";
        public const string APInvoice = "AP Invoice";
        public const string APSetup = "Account Payable Preferences";
		public const string APTran = "AP Transactions";
		public const string APTaxTran = "AP Tax Details";
		public const string APLandedCostTran = "AP Landed Cost";
		public const string VendorClass = "Vendor Class";
		public const string Document = "Document";
		public const string APAddress = "AP Address";
		public const string APContact = "AP Contact";

        public const string BalanceByVendor = "AP Balance by Vendor";   //MMK 2011/10/03
        public const string VendorHistory = "Vendor History";
        public const string APAgedPastDue = "AP Aged Past Due";
        public const string APAgedOutstanding = "AP Aged Outstanding";
        public const string APDocumentRegister = "AP Document Register";
        public const string RepVendorDetails = "Vendor Details";

        public const string APInvoiceDiscountDetail = "AP Invoice Discount Detail";

		#endregion

		#region Document Type
		public const string Invoice = "Bill";
		public const string CreditAdj = "Credit Adj.";
		public const string DebitAdj = "Debit Adj.";
		public const string Check = "Check";
		public const string Prepayment = "Prepayment";
		public const string Refund = "Vendor Refund";
		public const string VoidCheck = "Voided Check";
		public const string DeferredExpense = "Deferred Expense";
		public const string QuickCheck = "Quick Check";
		public const string VoidQuickCheck = "Void Quick Check";
		#endregion

		#region Report Document Type
		public const string PrintInvoice = "BILL";
		public const string PrintCreditAdj = "CRADJ";
		public const string PrintDebitAdj = "DRADJ";
		public const string PrintCheck = "CHECK";
		public const string PrintPrepayment = "PREPAY";
		public const string PrintRefund = "REF";
		public const string PrintVoidCheck = "VOIDCK";
		public const string PrintQuickCheck = "QCHECK";
		public const string PrintVoidQuickCheck = "VOIDQCK";
		#endregion

		#region Document Status
		public const string Hold = "On Hold";
		public const string Balanced = "Balanced";
		public const string Voided = "Voided";
		public const string Scheduled = "Scheduled";
		public const string Open = "Open";
		public const string Closed = "Closed";
		public const string Printed = "Printed";
		public const string Prebooked = "Pre-Released";
		#endregion

		#region
		public const string PeriodHasUnreleasedDocs = "Period has Unreleased Documents";
		public const string PeriodHasHoldDocs		= "Period has Hold Documents";
		public const string PeriodHasPrebookedDocs = "Period has Pre-released Documents";
		#endregion

		#region AP Mask Codes
			public const string MaskItem = "Non-Stock Item";
			public const string MaskLocation = "Vendor Location";
			public const string MaskEmployee = "Employee";
			//public const string MaskCompany = "Company Location";
            public const string MaskCompany = "Branch";
			public const string MaskProject = "Project";
        #endregion

		#region Pay By
		public const string DueDate = "Due Date";
		public const string DiscountDate = "Discount Date";
		#endregion

		#region Check Processsing Option
		public const string ReleaseChecks = "Release";
		public const string VoidChecks = "Void and Reprint";
		public const string DeleteChecks = "Reprint";
		public const string Void = "Void Prepayment";
		#endregion

        #region DiscountAppliedTo
        public const string ExtendedPrice = "Extended Cost";
        public const string SalesPrice = "Unit Cost";
        #endregion

        #region Discount Target
        public const string VendorUnconditional = "Unconditional";
        public const string VendorAndInventory = "Item";
        public const string VendorInventoryPrice = "Item Price Class";
        public const string Vendor_Location = "Location";
        public const string VendorLocationaAndInventory = "Item and Location";
        #endregion
	}
}
