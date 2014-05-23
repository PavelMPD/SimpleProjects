using System.Runtime.Serialization;

namespace DebtCollection.Model.Enums
{
    [DataContract]
    public enum DocumentType
    {
        [EnumMember]
        Unknown = 0,

        [EnumMember]
        Claim = 1,

        [EnumMember]
        Endorsement = 3,

        [EnumMember]
        Complaint = 5,

        [EnumMember]
        EndorsementSubdocument = 6,

        [EnumMember]
        ComplaintSubdocument = 7,

        [EnumMember]
        Subdocument = 8,
    }
}