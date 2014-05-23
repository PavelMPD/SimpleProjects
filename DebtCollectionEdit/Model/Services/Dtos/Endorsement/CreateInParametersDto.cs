using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos.Endorsement
{
    [DataContract]
    public class CreateInParametersDto : DebtorsActionParametersDto
    {
        [DataMember]
        public string NotarialPersonName { get; set; }
        [DataMember]
        public string PositionOfNotaryPerson { get; set; }
        [DataMember]
        public string NameOfNotaryOffice { get; set; }
        [DataMember]
        public string RegistryNumber { get; set; }
    }
}