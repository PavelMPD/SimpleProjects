using System;

namespace DebtCollection.Model.Dtos
{
    public class BillingDebtorDto : IDto, IHousable
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        /// <summary>
        /// MSISDN
        /// </summary>
        public long SubscriberId { get; set; }
        public string TariffPlan { get; set; }
        public string DeviceType { get; set; }
        public long? Imei { get; set; }
        public string CertificateType { get; set; }
        public string CertificateId { get; set; }
        public DateTime? CertificateIssueDate { get; set; }
        public string Authority { get; set; }
        public DateTime? BirthDate { get; set; }
        public String RegistrationAddress { get; set; }
        public String PostalCode { get; set; }
        public long? AgreementNumber { get; set; }
        public DateTime? AgreementDate { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public DateTime? DebtDate { get; set; }
        public Decimal? DebtAmount { get; set; }
        public int? PeriodsCount { get; set; }
        public string ConnectionPoint { get; set; }
        public long ContractCode { get; set; }
        public DateTime? ContractDate { get; set; }
        public DateTime? DebtIncreaseDate { get; set; }
        public string MarketingAction { get; set; }
        public int? DebtTerm { get; set; }
        public int? LeftSubscriptions { get; set; }
        public string ActiveService { get; set; }
        public string IdentificationNumber { get; set; }
    }
}