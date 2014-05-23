using DebtCollection.Model.Services.Dtos.Subdocuments;
using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos
{
    [DataContract]

    [KnownType(typeof(DebtorsActionParametersDto))]
    [KnownType(typeof(DocumentsActionParametersDto))]
    public class ActionParametersDto
    {
        [DataMember]
        public long OperationId { get; set; }
    }
}