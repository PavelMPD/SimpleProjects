using System;

namespace DebtCollection.Model.Dtos
{
    public class CrmItemDto : IDto
    {
        public long SubscriberId { get; set; }
        public string FullName { get; set; }
        public string SubStatus { get; set; }
        public string Responsible { get; set; }
        public string Comments { get; set; }
        public DateTime Date { get; set; }
        public string MarketingAction { get; set; }
        public string TariffPlan { get; set; }
    }
}