using DebtCollection.Model.Enums;
using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos
{
    [DataContract]
    public class StateDescriptionDto
    {
        [DataMember]
        public OperationDebtorStatus OperationStatus { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public bool DocumentPrepared { get; set; }
    }
}
