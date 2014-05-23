using PX.Common;

namespace PX.Objects.AR
{
    [PXLocalizable]
    public static class RUTROTMessages
    {
        public const string RUTType = "RUT";
        public const string ROTType = "ROT";

        public const string DeductibleExceedsAllowance = "Deductible amount exceeds allowance amount";
        public const string PeopleAreRequiredForDeduction = "At least one person must be specified for ROT or RUT deductible document";
        public const string PositiveUndistributedAmount = "The deductible amount is not distributed among the household members entirely.";
        public const string NegativeUndistributedAmount = "Distributed amount exceeds deductible amount";
        public const string UndistributedAmount = "Deduction is not distributed properly";

        public const string NonpositivePersonAmount = "Non-positive amounts are not allowed in ROT & RUT distribution";
        public const string PersonExceedsAllowance = "Amount exceeds personal allowance";

        public const string FailedToExport = "Failed to export file";

        public const string ClaimNextRefDecreased = "The field value was decreased. This may lead to duplicated reference numbers for exported documents";

        #region Export Validation
        public const string NoDocumentsSelected = "No documents selected for export.";
        public const string AtLeastOneBuyerMustBeMentioned = "At least one buyer must be mentioned in claim.";
        public const string NoMoreThan100Buyers = "Claim must include no more than 100 buyers.";

        public const string DateShouldBeSpecifiedOnDocument = "Date should be specified for each claimed invoice.";
        public const string DocumentDateIsBelowAllowed = "Claim is allowed for documents not earlier than {0:d}.";
        public const string SomeDocumentDatesIncorrect = "Some documents have incorrect dates.";
        public const string AllDocumentsMustBeSameYear = "All documents in the claim must be from the same year.";
        public const string PaymentDatesMustNotExceedClaimDate = "Dates of payments must not exceed claim date.";

        public const string CompanyIDMustBeSameForDocuments = "Company ID must be the same for all documents in claim.";
        public const string CompanyIDMustNotBeSameAsBuyerID = "Claimer ID must not be the same as any of the Buyer IDs in claim.";
        public const string SomeCompanyBuyerIDsAreIncorrect = "Company ID and/or Buyer ID are specified incorrectly for some documents.";

        public const string PaymentMustCoverDeductible = "Payment must cover deductible amount.";
        public const string ClaimedPaidMustNotExceedTotal = "The sum of claimed and paid amounts must not exceed invoice amount.";
        public const string SomeAmountsIncorrect = "Amounts are incorrect for some documents.";
        public const string ClaimedTotalTooMuch = "Claim amount must not exceed SEK 50000 per company.";
        #endregion

        #region
        public const string FieldClass = "RUTROT";
        #endregion
    }
}
