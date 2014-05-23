using DebtCollection.Model.Enums;
using DebtCollection.Model.Services.Dtos.Endorsement;
using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos
{
    [DataContract]
    [KnownType(typeof(ReplaceEndorsementsDebtorsResultDto))]
    public class OperationResult
    {
        public OperationResult(OperationStatus status)
        {
            Status = status;
        }

        [DataMember]
        public OperationStatus Status { get; set; }
    }
}