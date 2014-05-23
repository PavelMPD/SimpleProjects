using DebtCollection.Model.Services.Dtos.Subdocuments;

namespace DebtCollection.Model.Services.Dtos
{
    public interface IDocumentsData
    {
        ActionDocumentDescriptor[] Documents { get; set; }
    }
}