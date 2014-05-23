using System.Runtime.Serialization;

namespace DebtCollection.Model.Enums
{
    [DataContract]
    public enum OperationStatus
    {
        /// <summary>
        /// Status is undefined
        /// </summary>
        [EnumMember]
        Undefined = 0,

        /// <summary>
        /// Operation is processing
        /// </summary>
        [EnumMember]
        InProgress = 1,

        /// <summary>
        /// Operation completed with success
        /// </summary>
        [EnumMember]
        Succeeded = 2,

        /// <summary>
        /// Operation completed with error
        /// </summary>
        [EnumMember]
        Failed = 3,

        /// <summary>
        /// Operation canceled
        /// </summary>
        [EnumMember]
        Canceled = 4,

        /// <summary>
        /// Operation completed with error - (can't be continued?)
        /// </summary>
        [EnumMember]
        Crashed = 5
    }
}