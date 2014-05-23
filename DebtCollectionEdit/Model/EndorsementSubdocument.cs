using DebtCollection.Model.Enums;
using System;

namespace DebtCollection.Model
{
    public class EndorsementSubdocument : Entity, IEdmsRegistrableDocument, IAssumingAnswerDocument
    {
        public SubdocumentType DocumentType { get; set; }
        public virtual Endorsement Endorsement { get; set; }
        public virtual FileEntity File { get; set; }
        public virtual OperationSubdocument OperationSubdocument { get; set; }
        public string RegistrationNumber { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? GenerationDate { get; set; }
        public DateTime? AnswerRecievedDate { get; set; }
    }
}
