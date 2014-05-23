using DebtCollection.Model.Enums;

namespace DebtCollection.Model.Payment
{
    public interface IDocument
    {
        long Id { get; set; }

        Stage? Stage { get; set; }

        DocumentType DocumentType { get; }
    }
}
