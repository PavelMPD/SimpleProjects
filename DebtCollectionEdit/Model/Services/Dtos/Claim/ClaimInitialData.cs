using System;
using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos.Claim
{
    [DataContract]
    public class ClaimInitialData : DebtorsActionParametersDto
    {
        [DataMember]
        public DateTime DrawnOnDate { get; set; }

        [DataMember]
        public EdmsRegistrationType EdmsRegistrationType { get; set; }

        [DataMember]
        public string Operator { get; set; }
    }
}