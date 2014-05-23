using System;

namespace DebtCollection.Model
{
    public class ClaimOperationInfo : Entity, IEdmsRegistrableOperation
    {
        public virtual Operation Operation { get; set; }

        public DateTime DrawnOnDate { get; set; }

        public EdmsRegistrationType EdmsRegistrationType { get; set; }

        public string Operator { get; set; }
    }
}