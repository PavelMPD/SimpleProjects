using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DebtCollection.Model.Enums;

namespace DebtCollection.Model.ReportFilesProcessing
{
    [Table("ProcessedFiles")]
    public class ProcessedFile : Entity
    {
        [Required]
        public string FileName { get; set; }
        [Required]
        public DateTime ProcessingDate { get; set; }
        [Required]
        public DateTime FileDate { get; set; }

        public int? TotalItemsToProcessCount { get; set; }

        public int SuccessfulyProcessedItemsCount { get; set; }

        public int FailedItemsCount { get; set; }

        public string Message { get; set; }

        public ProcessingStatus Status { get; set; }
        [Required]
        public ProcessingFileType FileType { get; set; }

        public virtual ICollection<ProcessedFileDetails> FileItems { get; set; }
    }
}