using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using DebtCollection.Model.Enums;

namespace DebtCollection.Model
{
    public class OperationDebtor : Entity
    {
        public virtual Operation Operation { get; set; }
        public virtual Debtor Debtor { get; set; }

        public DocumentsProcessorCommandId? CurrentCommandId { get; set; }
        public CommandState? CommandStateId { get; set; }        

        public OperationDebtorStatus OperationStatus { get; set; }

        public string StateDescription { get; set; }
        
        [NotMapped]
        public bool DocumentPrepared { get; set; } //todo: remove

        [NotMapped]
        public FileEntity Document { get; set; } //todo: remove

        [NotMapped]
        public List<DocumentsProcessorCommandId> AllowedCommands { get; set; }

        public virtual DebtorsClaimOperationInfo DebtorsClaimOperationInfo { get; set; }
    }
}