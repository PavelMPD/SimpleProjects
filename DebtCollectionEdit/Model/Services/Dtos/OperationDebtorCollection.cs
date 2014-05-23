
using System.Runtime.Serialization;

namespace DebtCollection.Model.Services.Dtos
{
    [DataContract]
    public class OperationDebtorCollection
    {
        [DataMember]
        public OperationDebtorDto[] Debtors { get; set; }

        [DataMember]
        public int TotalDebtorsAmount { get; set; }

    }
}
