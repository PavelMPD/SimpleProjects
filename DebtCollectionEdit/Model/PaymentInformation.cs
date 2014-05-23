using DebtCollection.Model.Attributes;
using DebtCollection.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebtCollection.Model
{
    public class PaymentInformation : Entity, IAuditable
    {
        public DateTime? Date { get; set; }
        public virtual Debtor Debtor { get; set; }
        public OperatingAccount? OperatingAccount { get; set; }
        public string DocumentNumber { get; set; }
        public PaymentType? Type { get; set; }
        public string RecipientAgent { get; set; }

        public decimal RecipientAgentComission { get; set; }
        public string RecipientAgentBankMfi { get; set; }

        public decimal PaymentTotal { get; set; }
        public decimal DutyPaymentTotal { get; set; }
        public decimal FinePaymentTotal { get; set; }

        /// <summary>
        /// Сколько заплатили комиссии
        /// </summary>
        public decimal CommissionAmount { get; set; }
        public decimal RestTotal { get; set; }
        public Guid? ExternalId { get; set; }

        public string Comments { get; set; }

        [Auditable("Платеж отменен")]
        public bool Canceled { get; set; }

        public bool CloseDebt5Processed { get; set; }

        /// <summary>
        /// Разброс платежа по документом
        /// </summary>
        public virtual ICollection<DocumentPayment> DocumentPayments { get; set; }

        //Логика для подсчета коммисии
        public static decimal CalculateRecipientAgentComission(decimal? paymentTotal, decimal? cmmissionAmount)
        {
            return paymentTotal == 0 ? 0 : decimal.Round(((cmmissionAmount ?? 0) / (paymentTotal ?? 1)) * 100, 2);
        }

        /// <summary>
        /// Дата создание платежа
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Дата отмены платежа
        /// </summary>
        public DateTime? CancellationDate { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)] //The trick. Used for Endorsement and Debtor because they don't have Debtor_Id field to map
        public long? DebtorId { get; set; }

        [NotMapped] // The trick for audit
        public Debtor AuditDebtor { get { return Debtor; } set { } }
    }
}
