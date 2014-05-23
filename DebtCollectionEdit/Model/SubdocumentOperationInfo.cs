using DebtCollection.Model.Enums;
using System;

namespace DebtCollection.Model
{
    public class SubdocumentOperationInfo : Entity, IEdmsRegistrableOperation
    {
        public virtual Operation Operation { get; set; }

        public EdmsRegistrationType EdmsRegistrationType { get; set; }
        public SubdocumentType SubdocumentType { get; set; }

        public string LawmanFullName { get; set; }
        public string OutgoingNumber { get; set; }
        public DateTime? OutgoingDate { get; set; }
        public string PaymentNumber { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string OrganisationName { get; set; }
        public string OrganisationAddress { get; set; }
        public string ReturnReason { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string CourtId { get; set; }
        public string ExecutorName { get; set; }
        public string ExecutorId { get; set; }
        public DateTime? ExecutionListDate { get; set; }
        public string JurisdictionId { get; set; }
        public DateTime? DirectionDateToAnotherCourt { get; set; }
    }
}