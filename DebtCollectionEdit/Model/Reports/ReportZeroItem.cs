namespace DebtCollection.Model.Reports
{
    public class ReportZeroItem : Entity
    {
        public string DeviceType { get; set; }
        public int NumberOfClaimsPut { get; set; }
        public decimal ClaimsTotalAmount { get; set; }
        public int NumberOfPaidClaims { get; set; }
        public decimal PaidClaimsTotalAmount { get; set; }
        public decimal AmountOfReceivedMoney { get; set; }
        public int NumberOfCreatedIn { get; set; }
        public decimal InTotalAmount { get; set; }
        public int ReturnedDocs_X { get; set; }
        public int ReturnedDocs_Y { get; set; }
    }
}
