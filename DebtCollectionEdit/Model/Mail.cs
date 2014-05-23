using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebtCollection.Model
{
    [Table("Mail")]
    public class Mail : Entity
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime? SentDate { get; set; }
    }
}
