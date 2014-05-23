using System.ComponentModel.DataAnnotations.Schema;
using DebtCollection.Model.Enums;

namespace DebtCollection.Model
{
    public class OperationSubdocument : Entity
    {
        public virtual Operation Operation { get; set; }

        public DocumentType DocumentType { get; set; }

        public virtual Complaint Complaint { get; set; }
        public virtual Endorsement Endorsement { get; set; }

        public DocumentsProcessorCommandId? CurrentCommandId { get; set; }
        public CommandState? CommandStateId { get; set; }

        public OperationSubdocumentStatus OperationStatus { get; set; }
        public string StateDescription { get; set; }

        public virtual ComplaintSubdocument ComplaintSubdocument { get; set; }
        public virtual EndorsementSubdocument EndorsementSubdocument { get; set; }        
    }
}