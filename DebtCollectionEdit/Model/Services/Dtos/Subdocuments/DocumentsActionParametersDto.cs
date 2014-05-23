using DebtCollection.Model.Enums;
using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos.Subdocuments
{
    [DataContract]

    [KnownType(typeof(DocumentsGeneratingParametersDto))]
    [KnownType(typeof(RegisteredDocumentsDto))]
    public class DocumentsActionParametersDto : ActionParametersDto, IDocumentsData
    {
        [DataMember]
        public ActionDocumentDescriptor[] Documents { get; set; }

        [DataMember]
        public SubdocumentType SubdocumentType { get; set; }

        [DataMember]
        public long[] Subdocuments { get; set; }
    }
}