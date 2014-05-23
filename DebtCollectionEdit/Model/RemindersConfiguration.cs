using System.ComponentModel.DataAnnotations.Schema;

namespace DebtCollection.Model
{
    [Table("RemindersConfiguration")]
    public class RemindersConfiguration : Entity
    {
        public string Schedule { get; set; }

        public bool IsPaymentsTracing { get; set; }
    }
}
