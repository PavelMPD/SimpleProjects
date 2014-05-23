using DebtCollection.Model.Attributes;
using DebtCollection.Model.Enums;
using DebtCollection.Model.Payment;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebtCollection.Model
{
    /// <summary>
    /// Информация об исках
    /// </summary>
    [Table("Complaints")]
    public class Complaint : Entity, IPayableDocument, IAuditable
    {
        public virtual Debtor Debtor { get; set; }

        /// <summary>
        /// Имя
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Имя (Иск)")]
        public string FirstName { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Фамилия (Иск)")]
        public string LastName { get; set; }

        /// <summary>
        /// Отчество
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Фамилия (Иск)")]
        public string MiddleName { get; set; }

        /// <summary>
        /// Документ удостоверяющий личность
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Документ, удостоверяющий личность (Иск)")]
        public string CertificateType { get; set; }

        /// <summary>
        /// Номер документа (удостоверяющего личность)
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Номер документа (Иск)")]
        public string CertificateId { get; set; }

        /// <summary>
        /// Дата выдачи документа (удостоверяющего личность)
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Дата выдачи документа (Иск)")]
        public DateTime? CertificateIssueDate { get; set; }

        /// <summary>
        /// Орган, выдавший документ (удостоверяющий личность)
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Орган, выдавший документ (Иск)")]
        public string Authority { get; set; }

        /// <summary>
        /// Идентификационный номер (из паспорта)
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Идентификационный номер (Иск)")]
        public string IdentificationNumber { get; set; }

        /// <summary>
        /// Адрес регистрации
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Адрес регистрации (Иск)")]
        public String RegistrationAddress { get; set; }

        /// <summary>
        /// Адрес регистрации
        /// </summary>
        [Auditable("Почтовый индекс (Иск)")]
        public String PostalCode { get; set; }

        /// <summary>
        /// Сущность судебного решения
        /// </summary>
        [Auditable("Сущность судебного решения (Иск)")]
        public string CourtDecision { get; set; }

        /// <summary>
        /// ФИО судьи
        /// </summary>
        [Auditable("ФИО судьи")]
        public string NameOfJudge { get; set; }

        /// <summary>
        /// Дата вынесения решения суда
        /// </summary>
        [Auditable("Дата вынесения решения суда (Иск)")]
        public DateTime? DateOfCourtDecision { get; set; }

        /// <summary>
        /// Адрес суда  текущей подсудности
        /// </summary>
        [Auditable("Адрес суда  текущей подсудности (Иск)")]
        public string JurisdictionAddress { get; set; }

        /// <summary>
        /// Текущая подсудность
        /// </summary>
        [Auditable("Текущая подсудность (Иск)")]
        public string JurisdictionName { get; set; }

        /// <summary>
        /// Текущая подсудность в род. падеже
        /// </summary>
        [Auditable("Текущая подсудность в род. падеже (Иск)")]
        public string JurisdictionNameForDocument { get; set; }

        /// <summary>
        /// Регион суда текущей подсудности
        /// </summary>
        [Auditable("Регион суда текущей подсудности (Иск)")]
        public string JurisdictionRegion { get; set; }

        /// <summary>
        /// Адрес суда первоначальной подсудности
        /// </summary>
        [Auditable("Адрес суда первоначальной подсудности (Иск)")]
        public string InitialJurisdictionAddress { get; set; }

        /// <summary>
        /// Первоначальная подсудность
        /// </summary>
        [Auditable("Первоначальная подсудность (Иск)")]
        public string InitialJurisdictionName { get; set; }

        /// <summary>
        /// Первоначальная подсудность в род. падеже
        /// </summary>
        [Auditable("Первоначальная подсудность в род. падеже (Иск)")]
        public string InitialJurisdictionNameForDocument { get; set; }

        /// <summary>
        /// Регион первоначальной подсудности
        /// </summary>
        [Auditable("Регион первоначальной подсудности (Иск)")]
        public string InitialJurisdictionRegion { get; set; }

        /// <summary>
        /// Сумма штрафа, руб.
        /// </summary>
        [Auditable("Сумма штрафа, руб. (Иск)")]
        public decimal AmountOfPenalty { get; set; }

        /// <summary>
        /// Сумма госпошлины, руб.
        /// </summary>
        [Auditable("Сумма госпошлины, руб. (Иск)")]
        public decimal AmountOfDuty { get; set; }

        /// <summary>
        /// Сумма к взысканию, руб.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal? DemandedAmount { get; set; }

        /// <summary>
        /// Сумма штрафа по решению, руб.
        /// </summary>
        [Auditable("Сумма штрафа по решению, руб. (Иск)")]
        public decimal? CorrectedAmountOfPenalty { get; set; }

        /// <summary>
        /// Сумма госпошлины по решению, руб.
        /// </summary>
        [Auditable("Сумма госпошлины по решению, руб. (Иск)")]
        public decimal? CorrectedAmountOfDuty { get; set; }

        /// <summary>
        /// Отметка о взыскании задолженности по приказному производству
        /// </summary>
        [Auditable("Приказное производство")]
        public bool? MarkOfRecovery { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        [Auditable("Примечание (Иск)")]
        public string Note { get; set; }

        /// <summary>
        /// Прикрепленные файлы
        /// </summary>
        public virtual ICollection<FileEntity> Files { get; set; }

        [Auditable("Стадия (работы) (Иск)")]
        public Stage? Stage { get; set; }

        [Auditable("Дата направления ИЛ (Иск)")]
        public DateTime? ExecutionListDate { get; set; }

        public Stage? PreviousStage { get; set; }

        public DateTime? StageChangeDate { get; set; }

        /// <summary>
        ///  История ИД
        /// </summary>
        [Auditable("История ИД")]
        public string ExecutionListLocation { get; set; }

        public bool TreatAsDebt { get; set; }

        /// <summary>
        /// Дата предъявления иска
        /// </summary>
        public DateTime? ProductionDate { get; set; }

        /// <summary>
        /// Осталось оплатить по долгу
        /// </summary>
        public decimal? FullAmountOfDebt { get; set; }

        public virtual ICollection<ComplaintSubdocument> Subdocuments { get; set; }

        /// <summary>
        /// Платежи на документ
        /// </summary>
        public virtual ICollection<DocumentPayment> DocumentPayments { get; set; }

        public bool StopReminding { get; set; }

        public DateTime? NextActionDate { get; set; }

        public int? NextActionCounter { get; set; }

        /// <summary>
        /// Расходы на розыск
        /// </summary>
        [Auditable("Расходы на розыск (Иск)")]
        public decimal? CostOfSearch { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]  //The trick. Used for Endorsement and Debtor because they don't have Debtor_Id field to map
        public long? DebtorId { get; set; }

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
