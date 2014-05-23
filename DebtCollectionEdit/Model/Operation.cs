using System;
using System.Collections.Generic;
using DebtCollection.Model.Enums;

namespace DebtCollection.Model
{
    public class Operation : Entity
    {
        public DocumentType DocumentType { get; set; }
        public OperationStatus Status { get; set; }
        public string StateDescription { get; set; }
        public DateTime StartTime { get; set; }

        //todo: remove
        public DocumentsProcessorCommandId CurrentCommandId { get; set; }

        //todo: remove
        public CommandState CommandStateId { get; set; }

        public virtual EndorsementOperationInfo EndorsementOperationInfo { get; set; }
        public virtual ClaimOperationInfo ClaimOperationInfo { get; set; }
        public virtual SubdocumentOperationInfo SubdocumentOperationInfo { get; set; }

        public virtual ICollection<OperationDebtor> OperationDebtors { get; set; }
        public virtual ICollection<OperationSubdocument> OperationSubdocuments { get; set; }
    }
}