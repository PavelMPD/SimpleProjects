using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Script.Serialization;

namespace DebtCollection.Model
{
    public class DebtAmountHistory : Entity
    {
        /// <summary>
        /// Debt amount change date
        /// </summary>
        public DateTime ChangeDate { get; set; }

        /// <summary>
        /// New debt amount value
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Debtor
        /// </summary>
        public virtual Debtor Debtor { get; set; }
    }
}