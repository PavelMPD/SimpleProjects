using System.ComponentModel.DataAnnotations.Schema;
using DebtCollection.Model.Enums;

namespace DebtCollection.Model.ReportFilesProcessing
{
    [Table("ProcessedFilesDetails")]
    public class ProcessedFileDetails : Entity
    {
        public int ItemNumber { get; set; }

        public string Message { get; set; }

        public ProcessingStatus Status { get; set; }

        public virtual ProcessedFile ProcessedFile { get; set; }
    }
}