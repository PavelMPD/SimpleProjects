using System;

namespace DebtCollection.Model
{
    public interface IEdmsRegistrableDocument
    {
        string RegistrationNumber { get; set; }
        DateTime? RegistrationDate { get; set; }
    }
}