
namespace DebtCollection.Model.Reports
{
    public class ReportFourItem : Entity
    {
        public string JurisdictionName { get; set; }

        public decimal FineComission { get; set; }

        public decimal Fine { get; set; }

        public decimal DutyComission { get; set; }

        public decimal Duty { get; set; }

        public decimal Rest { get; set; }
    }
}
