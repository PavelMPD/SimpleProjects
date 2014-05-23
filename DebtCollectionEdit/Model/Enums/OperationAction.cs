using System.Runtime.Serialization;

namespace DebtCollection.Model.Enums
{
    public enum OperationAction
    {
        [EnumMember]
        Unknown = 0,
        [EnumMember]
        ChangeStatusClaimed = 1,
        [EnumMember]
        ChangeStatusInCreated = 2,
        [EnumMember]
        ReplaceEndorsementDebtors = 3,
        [EnumMember]
        ChangeStatusToCreateIn = 4,
        [EnumMember]
        CreateCoverLetter = 5,
        [EnumMember]
        ChangeStatusToExecutionProcedure = 6,
        [EnumMember]
        EndorsementInitialOperation = 7,
        [EnumMember]
        ReCreateOperationDocsForNotarialPerson = 15,
        [EnumMember]
        ReCreateFailedDebtorDocsForNotarialPerson = 16,
        [EnumMember]
        PrepareClaimWithAutoEdms = 8,
        [EnumMember]
        UploadEdmsRequest = 9,
        [EnumMember]
        DownloadEdmsResponse = 10,
        [EnumMember]
        PrepareClaimSingleAction = 11,
        [EnumMember]
        GenerateDocSingleAction = 13,
        [EnumMember]
        GenerateDocWithAutoEdms = 14,
        [EnumMember]
        Cancel = 12,
    }
}
