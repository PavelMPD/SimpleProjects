using DebtCollection.Model.Enums;
using DebtCollection.Model.Services.Dtos.Claim;
using DebtCollection.Model.Services.Dtos.Endorsement;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos
{
    [DataContract]
    [KnownType(typeof(ClaimOperationDto))]
    [KnownType(typeof(EndorsementOperationDto))]
    public class OperationDto
    {
        [DataMember]
        public long Id { get; set; }

        // TODO: raname it to smth like Operation title. the name used because of EndorsementOperationDto
        [DataMember]
        public string DocumentName { get; set; }

        [DataMember]
        public DocumentType DocumentType { get; set; }

        [DataMember]
        public OperationStatus OperationStatus { get; set; }

        [DataMember]
        public string StateDescription { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public List<OperationDebtorDto> Debtors { get; set; }

        [DataMember]
        public List<OperationAction> AvailableOperations { get; set; }
    }
}