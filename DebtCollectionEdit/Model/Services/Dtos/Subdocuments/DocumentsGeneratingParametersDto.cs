using System;
using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos.Subdocuments
{
    [DataContract]
    public class DocumentsGeneratingParametersDto : DocumentsActionParametersDto
    {
        [DataMember]
        public EdmsRegistrationType EdmsRegistrationType { get; set; }

        [DataMember]
        public string LawmanFullName { get; set; }

        [DataMember]
        public string OutgoingNumber { get; set; }

        [DataMember]
        public DateTime? OutgoingDate { get; set; }

        [DataMember]
        public string PaymentNumber { get; set; }

        [DataMember]
        public DateTime? PaymentDate { get; set; }

        [DataMember]
        public string OrganisationName { get; set; }

        [DataMember]
        public string OrganisationAddress { get; set; }

        [DataMember]
        public string ReturnReason { get; set; }

        [DataMember]
        public DateTime? ReturnDate { get; set; }

        [DataMember]
        public string CourtId { get; set; }

        [DataMember]
        public string ExecutorName { get; set; }

        [DataMember]
        public string ExecutorId { get; set; }

        [DataMember]
        public DateTime? ExecutionListDate { get; set; }

        [DataMember]
        public string JurisdictionId { get; set; }

        [DataMember]
        public DateTime? DirectionDateToAnotherCourt { get; set; }
    }
}