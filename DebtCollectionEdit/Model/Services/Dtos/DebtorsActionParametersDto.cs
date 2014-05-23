using DebtCollection.Model.Services.Dtos.Claim;
using DebtCollection.Model.Services.Dtos.Endorsement;
using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos
{
    [DataContract]
    [KnownType(typeof(ClaimInitialData))]
    [KnownType(typeof(UploadEdmsData))]

    [KnownType(typeof(CreateInParametersDto))]
    [KnownType(typeof(EndorsementInitialData))]
    [KnownType(typeof(CoverLettersParametersDto))]
    public class DebtorsActionParametersDto : ActionParametersDto, IDebtorsData
    {
        [DataMember]
        public long[] DebtorsId { get; set; }
    }
}