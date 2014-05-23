using DebtCollection.Model.Enums;
using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos.Endorsement
{
    [DataContract]
    public class EndorsementOperationDto : OperationDto
    {
        public EndorsementOperationDto()
        {
            DocumentType = DocumentType.Endorsement;
        }

        [DataMember]
        public string Notes { get; set; }

        [DataMember]
        public decimal StateDutyTotal { get; set; }

        [DataMember]
        public long? ApplicationFileId { get; set; }

        [DataMember]
        public long? AttachmentFileId { get; set; }

        [DataMember]
        public string Operator { get; set; }
    }
}
