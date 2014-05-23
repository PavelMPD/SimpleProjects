using System;
using PX.Data;
using PX.Common;

namespace PX.Objects.TX
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		// Add your messages here as follows (see line below):
		// public const string YourMessage = "Your message here.";

		#region Validation and Processing Messages
        public const string CannotProcessW = "Tax Invoice Date or Tax Invoice Nbr not set";
        public const string Prefix = "TX Error";
		public const string Document_Status_Invalid = "Document Status is invalid for processing.";
		public const string Only_Prepared_CanBe_Adjusted = "You can only adjust Tax Reports with Prepared status.";
		public const string DocumentOutOfBalance = "Document is out of balance.";
		public const string TaxAlreadyBlocked = "Tax is blocked already.";
		public const string TaxAlreadyInList = "This tax is already included into the list.";
		public const string NetTaxMustBeTax = "Net Tax line must have 'Tax' type.";
		public const string NetTaxMustExist = "Net Tax line does not exist for this Vendor.";
		public const string UseTaxExcludedFromTotals = "Use Tax is excluded from the document totals.";
		public const string NoLinesMatchTax = "No lines match the tax.";
		public const string TaxCategoryIsReferenced = "This Tax Category is referenced by {0} record and cannot be deleted.";
		public const string TaxableCategoryIDIsNotSet = "Taxable Category ID is not entered in the Import Settings. Please correct this and try again.";
		public const string TaxAgencyWithoutTran = "Tax Agency has no tax transactions.";
		public const string CannotPrepareReportForClosedOrPreparedPeriod = "Cannot prepare tax report for Closed or Prepared period.";
		public const string CannotPrepareReportPreviousOpen = "Cannot prepare tax report for open period when previous period isn't closed.";
		public const string CannotPrepareReportExistPrepared = "Cannot prepare tax report for period when previous prepared period isn't closed.";
		public const string CannotCloseReportForNotPreparedPeriod = "Cannot close tax report for Closed or Open period.";
		public const string CannotCloseReportForNotPreparedBranch = "Cannot close tax report. Branch {0} is not prepared for selected period.";
        public const string TaxBoxNumbersMustBeUnique = "Tax box numbers must be unique.";
		public const string CannotAdjustTaxForClosedOrPreparedPeriod = "Cannot adjust tax for Closed or Prepared period '{0}'.";
		public const string OriginalDocumentAlreadyContainsTaxRecord = "Original document already contains tax record.";
        public const string AnAdjustmentToReportedTaxPeriodWillBeMade = "An adjustment to the reported tax period will be made.";
		public const string NoAdjustmentToReportedTaxPeriodWillBeMade = "There is no transactions to adjust in the selected reporting period.";
	    public const string TaxGroupUsesTaxAmountButHasNoNetLine = "Tax group uses tax amount but has no net line set.";
	    public const string TheseTwoOptionsCantBeCombined = "These two options can't be combined.";
	    public const string ThisOptionCanOnlyBeUsedWithTaxTypeVAT = "This option can only be used with tax type VAT.";
	    public const string TaxRateMustBe0WhenUsingThisOption = "Tax rate must be 0 when using this option.";
        public const string TheseTwoOptionsShouldBeCombined = "These two options should be combined.";
		public const string TaxRoundingErrorDefineRoundingSettings = "Tax rounding error. Please define rounding settings in CM setup.";
		public const string TaxReportRateNotFound = "Tax report preparing failed. There is no currency rate to convert tax from currency '{0}' to report currency '{1}' for date '{2}'";
		public const string FailedToGetTaxes = "Failed to get Taxes from Avalara. Check Trace for details";
		public const string FailedToApplyTaxes ="Avalara returned the calculated tax successfuly. Failed to apply the avalara tax to the document.";
		public const string FailedToCancelTaxes = "Failed to Cancel the Tax on Avalara during the rollback. Details in Trace.";
	    
		public const string OneOrMoreTaxTransactionsFromPreviousPeriodsWillBeReported =
	        "One or more tax transactions from the previous periods will be reported into the current period.";
        public const string EffectiveTaxNotFound = "Can't find effective rate for '{0}' (type '{1}')";
        public const string InactiveTaxCategory = "Tax Category '{0}' is inactive";
        public const string BucketContainsOnlyAggregateLines = "Tax reporting group {0} contains only aggregate amount lines! Please review your tax reporting setup.";
        public const string BucketIsSubSetofAnotherBucket = "Tax group contains the same tax amount lines as tax group '{0}'";
        public const string TempLineisAggregate =
            "Report line {0} is detailed by tax zones and is an aggregate. Lines cannot be detailed by tax zones and aggregates at the same time";
		public const string ClaimableAndPayableAccountsAreTheSame = "Tax Claimable and Tax Payable accounts and subaccounts for Tax {0} are the same. It's impossible to enter this Tax via GL in this configuration.";
		public const string TaxRevNotFound = "No Tax Rate exists for Tax {0} and Tax Type {1}";
        #endregion

		#region Translatable Strings used in the code
		public const string CurrentBatchRecord = "Current batch record";
		public const string TranDesc1 = "{0}";
		public const string TranDesc2 = "{0},{1}";
		public const string DefaultInputGroup = "Default Input Group";
		public const string DefaultOutputGroup = "Default Output Group";
		public const string TaxAmount = "Tax Amount";
		public const string TaxableAmount = "Taxable Amount";
        public const string TaxOutDateTooEarly = "Entered date is before some of tax revisions' starting dates";
		#endregion

		#region Graph Names
		public const string ReportTax = "Tax Preparation Process";
		public const string ReportTaxReview = "Tax Filing Process";
		public const string ReportTaxProcess = "Tax Report Creator";
        public const string TaxHistoryManager = "Tax History Manager";
		public const string ReportTaxDetail = "Tax Report Details Inquiry";
		public const string SalesTaxMaint = "Taxes Maintenance";
		public const string TaxAdjustmentEntry = "Tax Adjustment Entry";
		public const string TaxBucketMaint = "Tax Reporting Groups Maintenance";
		public const string TaxCategoryMaint = "Tax Categories Maintenance";
		public const string TaxReportMaint = "Tax Report Setting Maintenance";
		public const string TaxZoneMaint = "Tax Zones Maintenance";
		public const string TaxExplorer = "Tax Explorer";
		public const string TaxImport = "Tax Import";
		public const string TaxImportSettings = "Tax Import Settings";
		public const string TaxImportDataMaint = "Tax Import Data Maintenance";
		public const string TaxImportZipDataMaint = "Tax Import Zip Data Maintenance";
		public const string AvalaraMaint = "AvaTax Configuration";
		#endregion

		#region Cache Names
		public const string TaxZone = "Tax Zone";
		public const string TaxAdjustment = "Tax Adjustment";
		public const string TaxCategory = "Tax Category";
		public const string Tax = "Tax";
		public const string VendorMaster = "Tax Agency";
		#endregion

		#region Combo Values
		// TaxType
		public const string Output = "Output";
		public const string Input = "Input";
		#region Tax Bucket Type
		public const string Purchase = "Purchase";
		#endregion

		#region Tax Period Type
		public const string Monthly = "Monthly";
		public const string SemiMonthly = "Semi-Monthly";
		public const string Quarterly = "Quarterly";
		public const string Yearly = "Yearly";
		public const string FiscalPeriod = "By Financial Period";
        public const string BiMonthly = "Once in Two Months";
		public const string SemiAnnually = "Semi-Annually";
		#endregion

		#region Tax Period Status
		public const string Prepared = "Prepared";
		public const string Open = "Open";
		public const string Closed = "Closed";
		#endregion

		#region Adjustment Status
		public const string AdjHold = "On Hold";
		public const string AdjBalanced = "Balanced";
		public const string AdjReleased = "Released";
		#endregion

		#region Adjustment Types
		public const string IncreaseTax = "Increase Tax";
		public const string ReduceTax = "Reduce Tax";
		#endregion

		#region VAT Invoice Types
		public const string InputVAT = "Input VAT";
		public const string OutputVAT = "Output VAT";
		#endregion

		#region Tax Type
		public const string Sales = "Sales";
		public const string Use = "Use";
		public const string VAT = "VAT";
		public const string Withholding = "Withholding";
		#endregion
		//CSTaxTermsDiscount
		public const string DiscountToTaxableAmount = "Reduces Taxable Amount";
		public const string DiscountToTotalAmount = "Does Not Affect Taxable Amount";
		#endregion

		#region Custom Actions
		public const string NewVendor = "New Vendor";
		public const string EditVendor = "Edit Vendor";
		public const string ReviewBatch = "Review Batch";
		public const string Release = "Release";
		public const string Document = "Document";
		public const string AdjustTax = "Adjust Tax";
		public const string NewAdjustment = "New Adjustment";
		public const string VoidReport = "Void Report";
		public const string ClosePeriod = "Close Period";
		public const string ViewDocuments = "View Documents";
		public const string Report = "Report";
		public const string PrepareTaxReport = "Prepare Tax Report";
		public const string Review = "Review";
		public const string TestConnection = "Test Connection";
	    public const string ViewGroupDetails = "Group Details";
		#endregion

		public const string Avalara = "Avalara";
	    public const string AvalaraUrlIsMissing = "URL is missing.";
		public const string AvalaraIsNotActive = "Avalara is not activated.";
		public const string AvalaraBranchToCompanyCodeMappingIsMissing = "Branch to Company Code mapping in the Avalara Setup is missing.";
		public const string Custom = "Custom";
		public const string ConnectionToAvalaraFailed = "Connection to Avalara failed.";
		public const string FailedToDeleteFromAvalara = "Failed to delete Taxes from Avalara. Details in the Trace.";

		#region Avalara Customer Usage
		public const string A = "Federal Government";
		public const string B = "State/Local Govt.";
		public const string C = "Tribal Government";
		public const string D = "Foreign Diplomat";
		public const string E = "Charitable Organization";
		public const string F = "Religious/Education";
		public const string G = "Resale";
		public const string H = "Agricultural Production";
		public const string I = "Industrial Prod/Mfg.";
		public const string J = "Direct Pay Permit";
		public const string K = "Direct Mail";
		public const string L = "Other";
		public const string N = "Local Governament";
		public const string P = "Commercial Aquaculture";
		public const string Q = "Commercial Fishery";
		public const string R = "Non-resident"; 
		#endregion
	}
}
