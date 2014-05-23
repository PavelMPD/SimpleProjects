using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos.Endorsement
{
    [DataContract]
    public class CoverLettersParametersDto : DebtorsActionParametersDto
    {
        [DataMember]
        public bool UseEdms { get; set; }

        [DataMember]
        public string ExecuterName { get; set; }

        [DataMember]
        public string ExecuterNumber { get; set; }
    }
}