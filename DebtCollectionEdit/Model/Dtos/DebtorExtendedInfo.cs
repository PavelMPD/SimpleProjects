using System;

namespace DebtCollection.Model.Dtos
{
    public class DebtorExtendedInfo
    {
        public long SubscriberId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public DateTime? DebtDate { get; set; }
        public long ContractCode { get; set; }

        public Decimal DebtAmountYesterday { get; set; }
        public Decimal DebtAmountToday { get; set; }
    }
}