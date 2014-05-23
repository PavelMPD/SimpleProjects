using DebtCollection.Model.Attributes;
using DebtCollection.Model.Enums;
using DebtCollection.Model.Payment;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebtCollection.Model
{
    /// <summary>
    /// Информация об ИН
    /// </summary>
    [Table("Endorsements")]
    public class Endorsement : Entity, IPayableDocument, IAuditable
    {
        public virtual Debtor Debtor { get; set; }

        /// <summary>
        /// Имя
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Имя (ИН)")]
        public string FirstName { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Фамилия (ИН)")]
        public string LastName { get; set; }

        /// <summary>
        /// Отчество
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Отчество (ИН)")]
        public string MiddleName { get; set; }

        /// <summary>
        /// Документ удостоверяющий личность
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Документ, удостоверяющий личность (ИН)")]
        public string CertificateType { get; set; }

        /// <summary>
        /// Номер документа (удостоверяющего личность)
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Номер документа (ИН)")]
        public string CertificateId { get; set; }

        /// <summary>
        /// Дата выдачи документа (удостоверяющего личность)
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Дата выдачи документа (ИН)")]
        public DateTime? CertificateIssueDate { get; set; }

        /// <summary>
        /// Орган, выдавший документ (удостоверяющий личность)
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Орган, выдавший документ (ИН)")]
        public string Authority { get; set; }

        /// <summary>
        /// Идентификационный номер (из паспорта)
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Идентификационный номер (ИН)")]
        public string IdentificationNumber { get; set; }

        /// <summary>
        /// Адрес регистрации
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Адрес регистрации (ИН)")]
        public String RegistrationAddress { get; set; }

        /// <summary>
        /// Номер ИН
        /// </summary>
        public string InNumber { get; set; }

        /// <summary>
        /// Дата ИН
        /// </summary>
        public DateTime? InDate { get; set; }

        /// <summary>
        /// Наименование нотариальной конторы
        /// </summary>
        public string NameOfNotaryOffice { get; set; }

        /// <summary>
        /// ФИО нотариуса
        /// </summary>
        public string NameOfNotaryPerson { get; set; }

        /// <summary>
        /// Сумма штрафа, руб.
        /// </summary>
        [Auditable("Сумма штрафа, руб (ИН)")]
        public decimal AmountOfPenalty { get; set; }

        /// <summary>
        /// Сумма госпошлины, руб.
        /// </summary>
        [Auditable("Сумма госпошлины, руб (ИН)")]
        public decimal AmountOfDuty { get; set; }

        /// <summary>
        /// Сумма к взысканию, руб.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal DemandedAmount { get; set; }

        /// <summary>
        /// Полная сумма долга, руб.
        /// </summary>
        public decimal? FullAmountOfDebt { get; set; }

        /// <summary>
        /// Сумма штрафа по решению, руб.
        /// </summary>
        [Auditable("Сумма штрафа по ИН, руб")]
        public decimal? CorrectedAmountOfPenalty { get; set; }

        /// <summary>
        /// Сумма госпошлины по решению, руб.
        /// </summary>
        [Auditable("Сумма госпошлины по ИН, руб")]
        public decimal? CorrectedAmountOfDuty { get; set; }

        /// <summary>
        /// Адрес суда  текущей подсудности
        /// </summary>
        [Auditable("Адрес суда  текущей подсудности (ИН)")]
        public string JurisdictionAddress { get; set; }

        /// <summary>
        /// Текущая подсудность
        /// </summary>
        [Auditable("Текущая подсудность (ИН)")]
        public string JurisdictionName { get; set; }

        /// <summary>
        /// Текущая подсудность в род. падеже
        /// </summary>
        [Auditable("Текущая подсудность в род. падеже (ИН)")]
        public string JurisdictionNameForDocument { get; set; }

        /// <summary>
        /// Регион суда текущей подсудности
        /// </summary>
        [Auditable("Регион суда текущей подсудности (ИН)")]
        public string JurisdictionRegion { get; set; }

        /// <summary>
        /// Адрес суда первоначальной подсудности
        /// </summary>
        [Auditable("Адрес суда первоначальной подсудности (ИН)")]
        public string InitialJurisdictionAddress { get; set; }

        /// <summary>
        /// Первоначальная подсудность
        /// </summary>
        [Auditable("Первоначальная подсудность (ИН)")]
        public string InitialJurisdictionName { get; set; }

        /// <summary>
        /// Первоначальная подсудность в род. падеже
        /// </summary>
        [Auditable("Первоначальная подсудность в род. падеже (ИН)")]
        public string InitialJurisdictionNameForDocument { get; set; }

        /// <summary>
        /// Регион первоначальной подсудности
        /// </summary>
        [Auditable("Регион первоначальной подсудности (ИН)")]
        public string InitialJurisdictionRegion { get; set; }

        /// <summary>
        /// Почтовый индекс
        /// </summary>
        [Auditable("Почтовый индекс (ИН)")]
        public String PostalCode { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        [Auditable("Примечание (ИН)")]
        public string Note { get; set; }

        [Auditable("Стадия (ИН)")]
        public Stage? Stage { get; set; }

        public Stage? PreviousStage { get; set; }

        public DateTime? StageChangeDate { get; set; }

        /// <summary>
        ///  История ИД
        /// </summary>
        [Auditable("История ИД")]
        public string ExecutionListLocation { get; set; }

        public virtual FileEntity CoverLetter { get; set; }

        public virtual FileEntity EndorsementFile { get; set; }

        public virtual ICollection<EndorsementSubdocument> Subdocuments { get; set; }

        /// <summary>
        /// Платежи на документ
        /// </summary>
        public virtual ICollection<DocumentPayment> DocumentPayments { get; set; }

        public bool StopReminding { get; set; }

        /// <summary>
        /// Причина корректировки штрафа
        /// </summary>
        [Auditable("Причина корректировки штрафа (ИН)")]
        public string CorrectionReason { get; set; }

        /// <summary>
        /// Расходы на розыск
        /// </summary>
        [Auditable("Расходы на розыск (ИН)")]
        public decimal? CostOfSearch { get; set; }

        /// <summary>
        /// Дата направления и/д
        /// </summary>
        [Auditable("Дата направления ИЛ (ИН)")]
        public DateTime? ExecutionListDate { get; set; }

        public bool TreatAsDebt { get; set; }

        public DateTime? NextActionDate { get; set; }

        public int? NextActionCounter { get; set; }

        [NotMapped] //The trick. Used for Endorsement and Debtor because they don't have Debtor_Id field to map
        public long? DebtorId { get { return Id; } set { } }

        [NotMapped] // The trick for audit
        public Debtor AuditDebtor { get { return Debtor; } set { } }

        /// <summary>
        /// Организация
        /// </summary>
        [Auditable("Организация")]
        public string Organisation { get; set; }

        /// <summary>
        /// Адрес организации
        /// </summary>
        [Auditable("Адрес организации")]
        public string OrganisationAddress { get; set; }

        /// <summary>
        /// Когда выставлено
        /// </summary>
        [Auditable("Когда выставлено")]
        public int? WhenPutYear { get; set; }
    }
}
