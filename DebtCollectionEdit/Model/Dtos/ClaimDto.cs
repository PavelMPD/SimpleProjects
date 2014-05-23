
using System;

namespace DebtCollection.Model.Dtos
{
    public class ClaimDto
    {
        public long? Id { get; set; }

        public string ClaimRegistrationNumber { get; set; }
        public DateTime ClaimDate { get; set; }
        public DateTime ClaimNecessaryToPay { get; set; }
        public long ContractCode { get; set; }
        public string FullName { get; set; }
        public string PostalCode { get; set; }
        public string RegistrationAddress { get; set; }
        public decimal ClaimAmount { get; set; }
        public string TariffPlan { get; set; }
    }
}
