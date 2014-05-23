using System;
using PX.Data;
using PX.Common;

namespace PX.Objects.CM
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		// Add your messages here as follows (see line below):
		// public const string YourMessage = "Your message here.";
		#region Validation and Processing Messages
        public const string CurrencyInfo = "Currency Info";
		public const string Prefix = "CM Error";
		public const string CuryRateCantBeZero = "Currency rate cannot be set to zero.";
        public const string DestShouldDifferFromOrig = "The destination currency should be different from the source currency.";
		public const string InvalidField = "Invalid field: '{0}'.";
		public const string CuryIDCannotBeChanged = "{0} cannot be changed.";
		public const string RateTypeCannotBeChanged = "{0} cannot be changed.";
		public const string RateIsNotDefinedForThisDate = "Rate is not defined for rate type '{0}' for currency '{1}' for this date!";
		public const string RateIsNotDefinedForThisDateShort = "Rate is not defined for this date.";
		public const string RateIsNotDefinedForThisDateVerbose = "Rate is not defined for rate type '{0}' from currency '{1}' to currency '{2}' for date {3}";
		public const string RateNotFound = "Currency Rate is not defined.";
		public const string RateTypeNotFound = "Currency Rate Type is not defined.";
		public const string TranslationAllreadyExists = "Translation '{0}' already exists for another period! Use Translation Batch Reversal to make it voided!";
		public const string TranslationLSetupMissing = "Required configuration data are not entered into Translation Setup";
		public const string APARRevalAlreadyExists = "AP/AR Relavuation already exists for this period! Use AP/AR Reval Batch Reversal to make it voided!";
		public const string APARRevalHaveReleasedDet = "This Revaluation has released details! Revaluation Processing is impossible!";
		public const string TransactionCanNotBeDeleted = "Released or voided transaction can not be deleted!";
		public const string TransactionCanNotBeCreated = "Transaction can not be created!";
		public const string TranslDefIsAlreadyUsed = "Translation definition '{0}' is already used in existing translations!";
		public const string RateVarianceExceeded = "Warning - Rate Variance exceeded.";
		public const string CurrencyInfoNotSaved = "An error occurred while saving Currency Info for the table '{0}'";
		public const string SubAccountFromCanNonBeEmpty = "Subaccount From Can Non Be Empty";
		public const string SubAccountToCanNonBeEmpty = "Subaccount To Can Non Be Empty";
		public const string AccountFromCanNonBeEmpty = "Account From Can Non Be Empty";
		public const string AccountToCanNonBeEmpty = "Account To Can Non Be Empty";
		public const string SuchRangeCrossWithRangeOnTheExistingDefenition = "Such Range Cross With Range On The Existing Defenition '{0}' Range: '{1}. {2}' - '{3}. {4}'";
        public const string NotValidCombination = "Invalid Combination of Accounts and Subaccounts";
		public const string ThereAreNoTransactionsMade = "There are no Transactions Made";
		public const string CurrencyRateNotFound = "Currency Rate not Found";
		public const string FuturePeriodIsNotAvalableForTranslation = "Future Period Is Not Available For Translation";
		public const string ThereIsNoSuchPeriodInTheTableOfFinancesPeriods = "There Is No Such Period In The Table Of Finances Periods";
        public const string DateNotBelongFinancialPeriod = "Date Not Belong Financial Period";
		public const string TranslationHistoryIsOutOfBalance = "Translation History is out of Balance";
		public const string TranslationOnPreviosPeriodNotReleased = "Translation On Previos Period Not Released";
		public const string TranslationDefinitionCanNotBeActive = "Translation definition can not be active";
		public const string TranslationDestinationLedegrIDCanNotBeChanged = "Translation Destination Ledeger ID can not be changed";
		public const string TranslationDefinitionLedgerNotFound = "Translation Definition Ledger Not Found";
		public const string TranslationDefinitionIncorrectBranchID = "Translation Destination Branch can not be changed to selected value";
		public const string NotReleasedTranslationExists = "Translation '{0}' is not released! Release or Delete this translation";
		public const string ReleasedTranslationExistsInGreaterPeriod = "Translation '{0}' is released in future period! Reverse this translation or create another one in the future period.";
		public const string YTDNetIncomeAccountWillBeExclude = "The range of accounts includes the YTD Net Income Account. Translation for this account will not be performed";
		public const string TranslationDefinitionHasSomeCrossIntervals = "Translation Definition has some cross intervals. So Translation cannot be created.";
		public const string SuchRateTypeAlreadyExist = "Such Currency Rate already exist";
		public const string NoRevaluationEntryWasMade = "No revaluation entry was made since Original Balance equals the Revalued Balance";
		public const string MultiCurrencyNotActivated = "Multi-Currency is not activated";
		#endregion

		#region Translatable Strings used in the code
		public const string GLTranSuccessfullyCreated = "GLTran successfully created";
		public const string TranslationSuccessfullyReleased = "Translation Successfully Released";
		public const string NothingSelected = "Nothing selected. Release not complete.";
		public const string NotAllItemsReleased = "Not all items released";
		public const string Revalue = "Revalue";
		public const string Periods = "Periods";
		public const string CuryDisplayName = "Currency";
		#endregion

        #region CM Gain/Loss Mask Codes
        public const string MaskCurrency = "Currency";
        public const string MaskCompany = "Branch Location";
        #endregion

		#region Graph Names
		public const string CMSetupMaint = "Setup Currency Management";
		public const string CurrencyMaint = "Currencies Maintenance";
		public const string CurrencyRateTypeMaint = "Currencies Rate Types Maintenance";
		public const string CuryRateMaint = "Currencies Exchange Rate Maintenance";
		public const string RevaluationMaint = "Denominated GL Account Revaluation Process";
		public const string TranslationDefinitionMaint = "Statement Transaltion Definition Maintenance";
		public const string TranslationEnq = "Statement Translaiton Entry";
		public const string TranslationProcess = "Statement Translation Preparation Process";
		public const string TranslationHistoryMaint = "Statement Translations History Inquiry";
		public const string TranslationRelease = "Statement Translation Release Process";
        public const string TranslationSetupMaint = "Statement Translation Setup";
		public const string RevalueAPAccounts = "Revalue AP Accounts";
		public const string RevalueARAccounts = "Revalue AR Accounts";
		public const string RevalueGLAccounts = "Revalue GL Accounts";
		#endregion

		#region Cache Names
		public const string FinPeriodIsNotActive = "Fin Period is not Active";
		public const string TranslationHistory = "Translation History";
		public const string Currency = "Currency";
		public const string TranslDef = "Translation Definition";
        public const string CurrencyRate2 = "Effective Currency Rate";
        public const string CurrencyRate = "Currency Rate";
		#endregion

		#region Combo Values
		//TranslationStatus and RevalStatus
		public const string Hold = "On Hold";
		public const string Balanced = "Balanced";
		public const string Released = "Released";
		public const string Voided = "Voided";

		//TranslationLineType
		public const string Translation = "Translation Line";
		public const string GainLoss = "Transl.Gain/Loss";
		#endregion

		#region Custom Actions
		public const string ViewTranslationBatch = "View Translation Batch";
		public const string ViewReversingBatch = "View Reversing Batch";
		public const string ViewTranslationDetails = "View Translation Details";
		public const string CreateTranslation = "Create Translation";
		public const string Release = "Release";
		public const string ReleaseAll = "Release All";
		public const string ViewTranslation = "View Translation";
		public const string Reverse = "Reverse";
		public const string ViewBatch = "View Batch";
        public const string TranslationDetailsReport = "Translation Details";

	    #endregion
	}
}
