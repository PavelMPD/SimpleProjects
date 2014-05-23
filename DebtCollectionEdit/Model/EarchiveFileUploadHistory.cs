using DebtCollection.Model.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;


namespace DebtCollection.Model
{
    [Table("EArchiveFilesUploadHistory")]
    public class EarchiveFileUploadHistory : Entity
    {
        public DateTime UploadDate { get; set; }

        public virtual FileEntity File { get; set; }

        public ProcessingStatus Status { get; set; }

        public string Message { get; set; }
    }
}
