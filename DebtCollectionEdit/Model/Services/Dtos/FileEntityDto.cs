using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos
{
    [DataContract]
    public class FileEntityDto
    {
        [DataMember]
        public long Id { get; set; }

        [DataMember]
        public string Link { get; set; }

        [DataMember]
        public string Name { get; set; }
    }
}