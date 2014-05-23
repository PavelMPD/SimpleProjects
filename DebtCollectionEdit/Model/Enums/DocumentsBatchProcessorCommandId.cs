namespace DebtCollection.Model.Enums
{
    public enum DocumentsProcessorCommandId
    {
        //common
        UploadEdmsRequest = 15,
        DownloadEdmsResponse = 16,

        // Claim
        PrepareClaims = 1,
        SetClaimedStatus = 2,
        RegisterClaimInEdms = 6,

        // Endorsement
        AssignInNumber = 19,
        GetAgreement = 3,
        GenerateEndorsement = 4,
        SetStatusToInCreated = 5,
        SaveStateDutyList = 7,
        SetStatusToStateDutyPaid = 8,
        GenerateApplicationForNotary = 9,
        ReplaceEndorsementDebtors = 10,
        GenerateCoverLetter = 11,
        SetStatusToExecutionProcedure = 12,
        RegisterCoverLetterInEdms = 13,
        GenerateAttachmentForNotary = 14,

        //subdocuments
        RegisterSubdocumentInEdms = 17,
        GenerateSubdocument = 18,
    }
}
