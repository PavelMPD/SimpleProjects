using DebtCollection.Common;
using System;

namespace DebtCollection.Model.Dtos
{
    public class ExistingDebtorDto : TrackableTuple, IHousable
    {
        //debtor info
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public long SubscriberId { get; set; }
        public string TariffPlan { get; set; }
        public long? Imei { get; set; }
        public string CertificateId { get; set; }
        public DateTime? CertificateIssueDate { get; set; }
        public string Authority { get; set; }
        public String RegistrationAddress { get; set; }
        public long? AgreementNumber { get; set; }
        public DateTime? AgreementDate { get; set; }
        public string Phone1 { get; set; }
        public Decimal? DebtAmount { get; set; }
        public long ContractCode { get; set; }
        public string SubscriberStatus { get; set; }
        public DateTime? ExecutionListDate { get; set; }
        public decimal? CostOfSearch { get; set; }
        public string Note { get; set; }
        public string IdentificationNumber { get; set; }
        public string DeviceType { get; set; }

        //claim info
        public DateTime? ClaimDate { get; set; }
        public DateTime? DrawnOnDate { get; set; }

        //complaint & endorsment
        public int WhenCreated { get; set; }
        public DateTime? LegalOpinionDate { get; set; }
        public decimal? AmountOfPenalty { get; set; }
        public string EndorsmentNumber { get; set; }
        public string CourtDecision { get; set; }
        public decimal? CorrectedAmountOfPenalty { get; set; }
        public decimal? CorrectedAmountOfDuty { get; set; }
        public string ExecutionListLocation1 { get; set; }
        public string ExecutionListLocation2 { get; set; }
        // public decimal? FullAmountOfDebt { get; set; }
        public string MarkOfRecovery { get; set; }
        public string Stage { get; set; }
        public string CourtAddress { get; set; }
        public string CourtName { get; set; }
        public string JudgeName { get; set; }
        public string NameOfNotaryOffice { get; set; }
        public DateTime? ProductionDate { get; set; }

        //payment
        public DateTime? PaymentDate { get; set; }
        public string OperatingAccount { get; set; }
        public decimal? FinePaymentComission { get; set; }
        public decimal? FinePayment { get; set; }
        public decimal? DutyPaymentComission { get; set; }
        public decimal? DutyPayment { get; set; }

        //writing columns
        public string ProcessingStatus { get; set; }
        public string ProcessingNotes { get; set; }
    }
}