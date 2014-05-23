using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos
{
    [DataContract]
    public class OrderDto
    {
        [DataMember]
        public string FieldName { get; set; }

        [DataMember]
        public bool IsDesc { get; set; }
    }
}
