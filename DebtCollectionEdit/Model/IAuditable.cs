
namespace DebtCollection.Model
{
    public interface IAuditable
    {
        long? DebtorId { get; set; }

        Debtor AuditDebtor { get; set; }
    }
}
