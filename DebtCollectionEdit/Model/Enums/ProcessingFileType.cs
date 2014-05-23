
namespace DebtCollection.Model.Enums
{
    public enum ProcessingFileType
    {
        // sequence is important. Providers are sorted by FileType and AllDebtors 
        // should go before ExDebtors, ExDebtors should go before the rest
        AllDebtors = 0,
        ExDebtors = 1,
        Crm = 2,
        Dm = 3,
        PrepareForClaimStatuses = 4,
        RequiresIn = 5,
        FullTextIndexer = 6
    }
}
