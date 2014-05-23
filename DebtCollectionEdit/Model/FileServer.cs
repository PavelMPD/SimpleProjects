using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebtCollection.Model
{
    public class FileServer : CachedEntity
    {
        [StringLength(100)]
        public string DisplayName { get; set; }

        [StringLength(500)]
        public string RootFolderPath { get; set; }
    }
}