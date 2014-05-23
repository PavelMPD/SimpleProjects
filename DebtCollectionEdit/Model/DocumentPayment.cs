using System.ComponentModel.DataAnnotations.Schema;

namespace DebtCollection.Model
{
    [Table("DocumentPayments")]
    public class DocumentPayment : Entity
    {
        public virtual PaymentInformation PaymentInformation { get; set; }

        public virtual Endorsement Endorsement { get; set; }

        public virtual Complaint Complaint { get; set; }

        public decimal? DocumentDutyPaymentAmount { get; set; }

        public decimal? DocumentPenaltyPaymentAmount { get; set; }
    }
}
