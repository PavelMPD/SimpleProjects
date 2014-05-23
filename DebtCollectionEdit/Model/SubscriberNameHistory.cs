using System;

namespace DebtCollection.Model
{
    public class SubscriberNameHistory : Entity
    {
        /// <summary>
        /// Address change date
        /// </summary>
        public DateTime ChangeDate { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        /// <summary>
        /// Debtor
        /// </summary>
        public virtual Debtor Debtor { get; set; }
    }
}