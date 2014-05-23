using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos.Claim
{
    [DataContract]
    public class UploadEdmsData : DebtorsActionParametersDto
    {
        [DataMember]
        public DateTime DrawnOnDate { get; set; }

        [DataMember]
        public List<EdmsClaimRecordDto> ClaimRecordDtos { get; set; }
    }
}