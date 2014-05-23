using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos.Subdocuments
{
    [DataContract]
    public class RegisteredDocumentsDto : DocumentsActionParametersDto
    {
        [DataMember]
        public List<EdmsSubdocumentRecordDto> SubdocumentRecordDtos { get; set; }
    }
}