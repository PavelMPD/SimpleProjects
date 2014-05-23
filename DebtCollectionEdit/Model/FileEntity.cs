using DebtCollection.Model.Enums;
using System.ComponentModel.DataAnnotations.Schema;


namespace DebtCollection.Model
{
    [Table("Files")]
    public class FileEntity : Entity
    {
        public string FileName { get; set; }

        public string DisplayName { get; set; }

        public FileEntityType FileType { get; set; }

        public long FileServerId { get; set; }

        //public virtual ICollection<Complaint> Complaints { get; set; }

        public virtual Debtor Debtor { get; set; }

        //public virtual Endorsement Endorsement{ get; set; }

        //public virtual Claim Claim { get; set; }

        public virtual string ResponseType
        {
            get { return GetResponseType(FileType); }
        }

        public static string GetResponseType(FileEntityType fileEntityType)
        {
            switch (fileEntityType)
            {
                case FileEntityType.Docx:
                    return "application/docx";
                case FileEntityType.Xlsx:
                case FileEntityType.TreatAsDebt:
                case FileEntityType.CloseDebt:
                    return "application/xlsx";
                case FileEntityType.Doc:
                    return "application/doc";
                case FileEntityType.Xls:
                    return "application/xls";
                case FileEntityType.Pdf:
                    return "application/pdf";
                case FileEntityType.Zip:
                    return "application/zip";
                case FileEntityType.Erip210:
                    return "application/210";
                default:
                    return "";
            }
        }
    }
}
