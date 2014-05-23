using System.ComponentModel.DataAnnotations;

namespace DebtCollection.Model.Reports
{
    public class ReportOneItem : Entity
    {
        [Display(Name = "Подсудность")]
        public string JurisdictionName { get; set; }

        [Display(Name = "Штраф (Сумма штрафа), руб.")]
        public decimal AmountOfPenalty { get; set; }

        [Display(Name = "Госпошлина (Сумма госпошлины), руб.")]
        public decimal AmountOfDuty { get; set; }
    }
}
