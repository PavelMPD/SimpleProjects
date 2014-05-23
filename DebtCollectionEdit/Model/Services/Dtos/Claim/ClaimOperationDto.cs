using DebtCollection.Model.Enums;
using System;
using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos.Claim
{
    [DataContract]
    public class ClaimOperationDto : OperationDto
    {
        public ClaimOperationDto()
        {
            DocumentType = DocumentType.Claim;
        }

        [DataMember]
        public DateTime DrawnOnDate { get; set; }

        [DataMember]
        public string Operator { get; set; }
    }
}