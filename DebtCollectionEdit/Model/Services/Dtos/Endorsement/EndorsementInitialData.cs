using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos.Endorsement
{
    [DataContract]
    public class EndorsementInitialData : DebtorsActionParametersDto
    {
        [DataMember]
        public string StateDutyListName { get; set; }

        [DataMember]
        public string StateDutyListNote { get; set; }

        [DataMember]
        public decimal StateDuty { get; set; }

        [DataMember]
        public string Operator { get; set; }
    }
}