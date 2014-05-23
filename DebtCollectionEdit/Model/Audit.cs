
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebtCollection.Model
{
    [Table("Audit")]
    public class Audit : Entity
    {
        public string Action { get; set; }
        public DateTime ActionDate { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Executor { get; set; }
        public virtual Debtor Debtor { get; set; }
    }
}
