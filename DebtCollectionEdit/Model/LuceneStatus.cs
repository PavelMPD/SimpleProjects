using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebtCollection.Model
{
    [Table("Lucene")]
    public class LuceneStatus: Entity
    {
        public bool IndexLocked { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}
