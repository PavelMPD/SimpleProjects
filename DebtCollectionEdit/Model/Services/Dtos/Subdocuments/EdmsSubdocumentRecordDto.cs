using DebtCollection.Model.Enums;
using System;
using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos.Subdocuments
{
    [DataContract]
    public class EdmsSubdocumentRecordDto
    {
        [DataMember]
        public string RegistrationNumber { get; set; }

        [DataMember]
        public DateTime? RegistrationDate { get; set; }

        [DataMember]
        public string FullName { get; set; }

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public long ContractCode { get; set; }

        [DataMember]
        public DocumentType DocumentType { get; set; }

        [DataMember]
        public long DocumentId { get; set; }

        [DataMember]
        public SubdocumentType SubdocumentType { get; set; }
    }
}