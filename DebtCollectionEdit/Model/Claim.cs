using DebtCollection.Model.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebtCollection.Model
{
    /// <summary>
    /// Информация о претензиях
    /// </summary>
    [Table("Claims")]
    public class Claim : Entity, IAuditable
    {
        [RequiredForDocumentGeneration]
        public virtual Debtor Debtor { get; set; }

        /// <summary>
        /// Дата, когда  выставить претензию
        /// </summary>
        public DateTime? ClaimWhenPutDate { get; set; }

        /// <summary>
        /// Дата выставления
        /// </summary>
        public DateTime? ClaimDate { get; set; }

        /// <summary>
        /// Регистрационный номер ????
        /// </summary>
        public string ClaimRegistrationNumber { get; set; }

        /// <summary>
        /// Дата оплаты по претензии
        /// </summary>
        public DateTime? DateOfClaimPayment { get; set; }

        /// <summary>
        /// Дата, до которой необходимо было оплатить
        /// </summary>
        [Auditable("Дата, до которой необходимо было оплатить (Претензия)")]
        public DateTime? ClaimNecessaryToPay { get; set; }

        /// <summary>
        /// Сумма по претензии
        /// </summary>
        public decimal ClaimAmount { get; set; }

        /// <summary>
        /// Ссылка на документ 
        /// </summary>
        public FileEntity File { get; set; }

        /// <summary>
        /// Имя
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Имя (Претензия)")]
        public string FirstName { get; set; }

        /// <summary>
        /// Отчество
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Отчество (Претензия)")]
        public string MiddleName { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Фамилия (Претензия)")]
        public string LastName { get; set; }

        /// <summary>
        /// Адрес регистрации
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Адрес регистрации (Претензия)")]
        public string RegistrationAddress { get; set; }

        /// <summary>
        /// Почтовый индекс
        /// </summary>
        [RegExpForDocumentGeneration("^[0-9]{6}$")]
        [RequiredForDocumentGeneration]
        [Auditable("Почтовый индекс (Претензия)")]
        public string PostalCode { get; set; }

        /// <summary>
        /// Тарифный план
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Тарифный план (Претензия)")]
        public string TariffPlan { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]  //The trick. Used for Endorsement and Debtor because they don't have Debtor_Id field to map
        public long? DebtorId { get; set; }

        [NotMapped] // The trick for audit
        public Debtor AuditDebtor { get { return Debtor; } set { } }
    }
}
