using System.ComponentModel.DataAnnotations;

namespace DebtCollection.Model.Dtos
{
    public class EarchiveForUploadDto : EarchiveDto
    {
        [Display(Name = "Status")]
        public string Status { get; set; }
    }
}