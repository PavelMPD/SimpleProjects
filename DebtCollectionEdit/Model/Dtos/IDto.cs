namespace DebtCollection.Model.Dtos
{
    public interface IDto
    {
        long SubscriberId { get; set; }
        string MarketingAction { get; set; }
        string TariffPlan { get; set; }
    }
}
