using DebtCollection.Model.Enums;

namespace DebtCollection.Model
{
    public class DebtorsClaimOperationInfo : Entity
    {
        public bool DocumentPrepared { get; set; }

        public virtual OperationDebtor OperationDebtor { get; set; }

        public virtual Claim Claim { get; set; }
    }
}