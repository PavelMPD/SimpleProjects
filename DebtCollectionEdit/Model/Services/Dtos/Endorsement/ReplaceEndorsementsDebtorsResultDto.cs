using DebtCollection.Model.Enums;
using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos.Endorsement
{
    [DataContract]
    public class ReplaceEndorsementsDebtorsResultDto : OperationResult
    {
        public ReplaceEndorsementsDebtorsResultDto(OperationStatus status)
            : base(status)
        {

        }

        /// <summary>
        /// Заменено
        /// </summary>
        [DataMember]
        public long Replaced { get; set; }

        /// <summary>
        /// Не нашлось замены
        /// </summary>
        [DataMember]
        public long Deleted { get; set; }
    }
}
