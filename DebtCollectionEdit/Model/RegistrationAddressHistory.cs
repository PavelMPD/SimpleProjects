using System;

namespace DebtCollection.Model
{
    public class RegistrationAddressHistory : Entity
    {
        /// <summary>
        /// Address change date
        /// </summary>
        public DateTime ChangeDate { get; set; }

        /// <summary>
        /// New address value
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Debtor
        /// </summary>
        public virtual Debtor Debtor { get; set; }
    }
}