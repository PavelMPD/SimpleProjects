using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebtCollection.Model
{
    [Table("PaymentAlerts")]
    public class PaymentAlert : Entity
    {
        public virtual PaymentInformation PymentInformation { get; set; }

        public long DebtorId { get; set; }

        public string FullName { get; set; }

        public bool Processed { get; set; }

        public string Reason { get; set; }

        public DateTime? ProcessingDate { get; set; }

        public string Executor { get; set; }

        public string Msisdn { get; set; }

        public decimal? PaymentAmount { get; set; }

        public DateTime? PaymentDate { get; set; }
    }
}
