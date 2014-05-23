using DebtCollection.Model.Enums;
using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos
{
    [DataContract]
    public class OperationDebtorDto
    {
        [DataMember]
        public long Id { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string MiddleName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public StateDescriptionDto State { get; set; }

        [DataMember]
        public decimal AmountOfDuty { get; set; }

        [DataMember]
        public decimal AmountOfPenalty { get; set; }

        [DataMember]
        public SubscriberStatus Status { get; set; }

        [DataMember]
        public FileEntityDto CurrentClaimFile { get; set; }

        [DataMember]
        public FileEntityDto EndorsementFile { get; set; }

        [DataMember]
        public FileEntityDto AgreementFile { get; set; }

        [DataMember]
        public long DebtorId { get; set; }
    }
}