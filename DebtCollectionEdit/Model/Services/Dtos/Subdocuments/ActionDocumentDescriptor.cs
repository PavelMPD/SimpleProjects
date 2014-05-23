using DebtCollection.Model.Enums;
using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos.Subdocuments
{
    [DataContract]
    public class ActionDocumentDescriptor
    {
        [DataMember]
        public long Id { get; set; }

        [DataMember]
        public DocumentType DocumentType { get; set; }
    }
}