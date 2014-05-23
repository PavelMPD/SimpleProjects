using DebtCollection.Model.Services.Dtos;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DebtCollection.Model.Services
{
    [DataContract]
    public class DocumentOperations
    {
        [DataMember]
        public int TotalOperationsAmount { get; set; }

        [DataMember]
        public List<OperationDto> Operations { get; set; }
    }
}