using System;
using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos.Claim
{
    [DataContract]
    public class EdmsClaimRecordDto
    {
        [DataMember]
        public string RegistrationNumber { get; set; }

        [DataMember]
        public DateTime? RegistrationDate { get; set; }

        [DataMember]
        public string FullName { get; set; }

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public long ContractCode { get; set; }

        [DataMember]
        public long ClaimId { get; set; }
    }
}