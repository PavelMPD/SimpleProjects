using DebtCollection.Model.Enums;
using System;
using System.Collections.Generic;

namespace DebtCollection.Model.Payment
{
    public interface IPayableDocument : IEntity
    {
        Debtor Debtor { get; set; }
        ICollection<DocumentPayment> DocumentPayments { get; set; }
        decimal AmountOfPenalty { get; set; }

        decimal? CorrectedAmountOfPenalty { get; set; }

        decimal AmountOfDuty { get; set; }

        decimal? CorrectedAmountOfDuty { get; set; }

        decimal? FullAmountOfDebt { get; set; }

        Stage? Stage { get; set; }

        Stage? PreviousStage { get; set; }

        DateTime? StageChangeDate { get; set; }

        bool StopReminding { get; set; }

        DateTime? NextActionDate { get; set; }

        int? NextActionCounter { get; set; }
    }

    public static class PayableDocumentExtension
    {
        public static decimal GetRestOfDuty(this IPayableDocument payableDocument)
        {
            decimal fullAmountOfDebt = GetActualFullAmountOfDebt(payableDocument);
            decimal duty = GetActualDuty(payableDocument); ;
            decimal penalty = GetActualPenalty(payableDocument);

            decimal paid = (duty + penalty) - fullAmountOfDebt;

            if (paid >= duty)
            {
                return 0;
            }

            return duty - paid;
        }

        public static decimal GetRestPenalty(this IPayableDocument payableDocument)
        {
            decimal fullAmountOfDebt = GetActualFullAmountOfDebt(payableDocument);
            decimal duty = GetActualDuty(payableDocument); ;
            decimal penalty = GetActualPenalty(payableDocument);

            decimal paid = (duty + penalty) - fullAmountOfDebt;

            if ((paid - duty) >= penalty)
            {
                return 0;
            }

            return penalty - (paid - duty);
        }

        private static decimal GetActualDuty(IPayableDocument payableDocument)
        {
            return payableDocument.CorrectedAmountOfDuty != null ?
                             payableDocument.CorrectedAmountOfDuty.Value :
                             payableDocument.AmountOfDuty;
        }

        private static decimal GetActualPenalty(IPayableDocument payableDocument)
        {
            return payableDocument.CorrectedAmountOfPenalty != null ?
                          payableDocument.CorrectedAmountOfPenalty.Value :
                          payableDocument.AmountOfPenalty;
        }

        private static decimal GetActualFullAmountOfDebt(IPayableDocument payableDocument)
        {
            return payableDocument.FullAmountOfDebt != null ?
                          payableDocument.FullAmountOfDebt.Value :
                          0;
        }
    }
}
