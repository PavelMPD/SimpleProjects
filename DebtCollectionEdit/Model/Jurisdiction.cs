using System.ComponentModel.DataAnnotations.Schema;

namespace DebtCollection.Model
{
    [Table("Jurisdictions")]
    public class Jurisdiction : Entity
    {
        public string Name { get; set; }
        public string NameForDocument { get; set; }
        public string Address { get; set; }
        public string Region { get; set; }
    }
}
