using System.ComponentModel.DataAnnotations.Schema;

namespace DebtCollection.Model
{
    public class EndorsementOperationInfo : Entity
    {
        public virtual Operation Operation { get; set; }

        public string DocumentName { get; set; }
        public string DocumentNote { get; set; }
        public decimal StateDutyTotal { get; set; }

        public virtual FileEntity ApplicationFile { get; set; }
        public virtual FileEntity AttachmentFile { get; set; }
        
        public string PositionOfNotaryPerson { get; set; }
        public string NameOfNotaryPerson { get; set; }
        public string NameOfNotaryOffice { get; set; }
        public string RegistryNumber { get; set; }
        public string Operator { get; set; }

        /// <summary>
        /// This one is used to determine whether to update endorsement numbers or not
        /// </summary>
        [NotMapped]
        public bool? RegistryNumberHasBeenChanged { get; set; }
    }
}
