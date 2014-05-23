using System.Runtime.Serialization;

namespace DebtCollection.Model.Enums
{
    [DataContract]
    public enum OperationDebtorStatus
    {
        [EnumMember]
        Unknown = 0,

        [EnumMember]
        Normal = 1,

        [EnumMember]
        Failed = 2,

        [EnumMember]
        NotIncludedIntoOperation = 3,

        [EnumMember]
        Corrupted = 4
    }
}