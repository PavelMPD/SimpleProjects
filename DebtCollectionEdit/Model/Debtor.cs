using DebtCollection.Model.Attributes;
using DebtCollection.Model.Enums;
using Microsoft.Linq.Translations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DebtCollection.Model
{
    [Table("Debtors")]
    public class Debtor : Entity//, IAuditable
    {
        /// <summary>
        /// Имя
        /// </summary>
        [Auditable("Имя")]
        public string FirstName { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        [Auditable("Фамилия")]
        public string LastName { get; set; }

        /// <summary>
        /// Отчество
        /// </summary>
        [Auditable("Отчество")]
        public string MiddleName { get; set; }

        [NotMapped]
        public string FullName
        {
            get { return string.Format("{0} {1} {2}", LastName, FirstName, MiddleName); }
        }

        /// <summary>
        /// Абонентский номер (MSISDN)
        /// </summary>
        [Auditable("Абонентский номер")]
        public long? SubscriberId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string Msisdn { get; set; }

        /// <summary>
        /// Тарифный план
        /// </summary>
        [Auditable("Тарифный план")]
        public string TariffPlan { get; set; }

        /// <summary>
        /// Тип оборудования
        /// </summary>
        [Auditable("Тип оборудования")]
        public string DeviceType { get; set; }

        /// <summary>
        /// IMEI модема
        /// </summary>
        [Auditable("IMEI оборудования")]
        public long? Imei { get; set; }

        /// <summary>
        /// Документ удостоверяющий личность
        /// </summary>
        [Auditable("Документ, удостоверяющий личность")]
        public string CertificateType { get; set; }

        /// <summary>
        /// Номер документа (удостоверяющего личность)
        /// </summary>
        [Auditable("Номер документа")]
        public string CertificateId { get; set; }

        /// <summary>
        /// Дата выдачи документа (удостоверяющего личность)
        /// </summary>
        [Auditable("Дата выдачи документа")]
        public DateTime? CertificateIssueDate { get; set; }

        /// <summary>
        /// Орган, выдавший документ (удостоверяющий личность)
        /// </summary>
        [Auditable("Орган, выдавший документ")]
        public string Authority { get; set; }

        /// <summary>
        /// Дата рождения 
        /// </summary>
        [Auditable("Дата рождения")]
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Адрес регистрации
        /// </summary>
        [Auditable("Адрес регистрации")]
        public String RegistrationAddress { get; set; }

        /// <summary>
        /// Почтовый индекс
        /// </summary>
        [Auditable("Почтовый индекс")]
        public String PostalCode { get; set; }

        /// <summary>
        /// Номер договора (=SIM number, уникальное поле для поиска в E-archive)
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Номер договора")]
        public long? AgreementNumber { get; set; }

        /// <summary>
        /// Дата договора (=дата покупки оборудования)
        /// </summary>
        [RequiredForDocumentGeneration]
        [Auditable("Дата договора")]
        public DateTime? AgreementDate { get; set; }

        /// <summary>
        /// Контактный номер
        /// </summary>
        [Auditable("Контактный номер")]
        public string Phone1 { get; set; }

        /// <summary>
        /// Контактный номер 2
        /// </summary>
        [Auditable("Контактный номер 2")]
        public string Phone2 { get; set; }

        /// <summary>
        /// Дата образования задолженности
        /// </summary>
        public DateTime DebtDate { get; set; }

        /// <summary>
        /// Сумма задолженности
        /// </summary>
        [Auditable("Сумма задолженности, руб")]
        public decimal DebtAmount { get; set; }

        /// <summary>
        /// Количество периодов использования услуги life:) Интернет (=количество прошедших подписок)
        /// </summary>
        public int? PeriodsCount { get; set; }

        /// <summary>
        /// Точка подключения
        /// </summary>
        public string ConnectionPoint { get; set; }

        /// <summary>
        /// Код контракта (=CO_ID, уникальное поле для поиска в биллинге)
        /// </summary>
        [Required]
        public long ContractCode { get; set; }

        /// <summary>
        /// Дата контракта (=дата активации сим-карты)
        /// </summary>
        public DateTime? ContractDate { get; set; }

        /// <summary>
        /// Дата списания за Интернет пакет (=дата увеличения задолженности)
        /// </summary>
        public DateTime? DebtIncreaseDate { get; set; }

        /// <summary>
        /// Маркетинговая акция
        /// </summary>
        [Auditable("Маркетинговая акция")]
        public string MarketingAction { get; set; }

        /// <summary>
        /// Срок задолженности (= кол-во дней от даты возникновения задолженности)
        /// </summary>
        public int? DebtTerm { get; set; }

        /// <summary>
        /// Количество оставшихся обязательных подписок
        /// </summary>
        [Auditable("Количество оставшихся обязательных подписок")]
        public int? LeftSubscriptions { get; set; }

        /// <summary>
        /// Активная услуга life:) интернет
        /// </summary>
        [Auditable("Активная услуга life:) интернет")]
        public string ActiveService { get; set; }
 

        /// <summary>
        /// Информация об ин
        /// </summary>
        public virtual Endorsement Endorsement { get; set; }

        /// <summary>
        /// Информация об исках
        /// </summary>
        public virtual ICollection<Complaint> Complaints { get; set; }

        /// <summary>
        /// The history of debt amount changes
        /// </summary>
        public virtual ICollection<DebtAmountHistory> DebtAmountHistory { get; set; }

        /// <summary>
        /// The history of registration address changes
        /// </summary>
        public virtual ICollection<RegistrationAddressHistory> RegistrationAddressHistory { get; set; }

        /// <summary>
        /// The history of subscriber name changes
        /// </summary>
        public virtual ICollection<SubscriberNameHistory> SubscriberNameHistory { get; set; }

        //-RollCall tab

        /// <summary>
        /// Дата обзвона
        /// </summary>
        public DateTime? RollCallDate { get; set; }

        /// <summary>
        /// Ответственный
        /// </summary>
        public string RollCallResponsible { get; set; }

        /// <summary>
        /// Подстатус обзвона
        /// </summary>
        public string RollCallSubStatus { get; set; }

        /// <summary>
        /// Комментарии
        /// </summary>
        public string RollCallComments { get; set; }

        /// <summary>
        /// Информация о претензиях
        /// </summary>
        public virtual ICollection<Claim> Claims { get; set; }

        /// <summary>
        /// Notes from payment info tab
        /// </summary>
        public virtual string PaymentInfoNote { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        [Auditable("Статус")]
        public SubscriberStatus SubscriberStatus { get; set; }

        /// <summary>
        /// Дата изменения статуса
        /// </summary>
        public DateTime? SubscriberStatusChangeDate { get; set; }

        /// <summary>
        /// Notes from payment info tab
        /// </summary>
        public virtual ICollection<PaymentInformation> PaymentsInformation { get; set; }

        public virtual FileEntity AgreementScan { get; set; }

        /// <summary>
        /// Подсудность
        /// </summary>
        public virtual Jurisdiction Jurisdiction { get; set; }

        //todo: rename
        public virtual ICollection<OperationDebtor> DocumentOperationDebtors { get; set; }

        /// <summary>
        /// Идентификационный номер (из паспорта)
        /// </summary>
        [Auditable("Идентификационный номер")]
        public string IdentificationNumber { get; set; }


        /// <summary>
        /// Для востановления статуса после отмены платежа
        /// </summary>
        public SubscriberStatus? PreviousSubscriberStatus { get; set; }

        /// <summary>
        /// Для востановления статуса после отмены платежа
        /// </summary>
        public DateTime? PreviousSubscriberStatusChangeDate { get; set; }


        /// <summary>
        ///  (Общая сумма штрафа, руб) TotalFineWithFee in main grid
        /// </summary>
        private static readonly CompiledExpression<Debtor, decimal> TotalFineWithFreeExpression =
             DefaultTranslationOf<Debtor>.Property(d => d.TotalFineWithFee)
                .Is(d => d.PaymentsInformation.Select(x => x.FinePaymentTotal).Union(new decimal[] { 0 }).Sum());

        public decimal TotalFineWithFee
        {
            get
            {
                return TotalFineWithFreeExpression.Evaluate(this);
            }
        }

        /// <summary>
        /// (Общая сумма госпошлины, руб) TotalDutyWithFee in main grid
        /// </summary>
        private static readonly CompiledExpression<Debtor, decimal> TotalDutyWithFreeExpression =
            DefaultTranslationOf<Debtor>.Property(d => d.TotalDutyWithFee)
                .Is(d => d.PaymentsInformation.Select(p => p.DutyPaymentTotal).Union(new decimal[] { 0 }).Sum());

        public decimal TotalDutyWithFee
        {
            get
            {
                return TotalDutyWithFreeExpression.Evaluate(this);
            }
        }

        /// <summary>
        ///  Остаток, руб (на самом деле сумма остатков платежей)  PaymentsRest in main grid
        /// </summary>
        private static readonly CompiledExpression<Debtor, decimal> PaymentsRestExpression =
            DefaultTranslationOf<Debtor>.Property(d => d.PaymentsRest)
                .Is(d => d.PaymentsInformation.Select(p => p.RestTotal).Union(new decimal[] { 0 }).Sum());

        public decimal PaymentsRest
        {
            get
            {
                return PaymentsRestExpression.Evaluate(this);
            }
        }

        /// <summary>
        /// "Сумма поступивших платежей, руб
        /// </summary>
        private static readonly CompiledExpression<Debtor, decimal> PaymentsTotalExpression =
              DefaultTranslationOf<Debtor>.Property(d => d.PaymentsTotal)
                 .Is(d => d.PaymentsInformation.Select(p => p.PaymentTotal).Union(new decimal[] { 0 }).Sum());

        public decimal PaymentsTotal
        {
            get
            {
                return PaymentsTotalExpression.Evaluate(this);
            }
        }


        /// <summary>
        /// Сумма к взысканию, руб.:
        /// </summary>
        [NotMapped]
        public decimal FullAmountOfDebt
        {
            get
            {
                decimal fullAmountOfDebt = 0;
                if (Endorsement != null)
                {
                    fullAmountOfDebt += Endorsement.DemandedAmount;
                }

                if (Complaints != null)
                {
                    fullAmountOfDebt += Complaints.Where(c => c.DemandedAmount != null).Select(c => c.DemandedAmount).Sum(c => c.Value);
                }

                return fullAmountOfDebt;
            }
        }

        /// <summary>
        /// Осталось погасить, руб.
        /// </summary>
        [NotMapped]
        public decimal DebtRemaining
        {
            get
            {
                decimal debtRemaining = 0;

                if (Endorsement != null)
                {
                    debtRemaining += Endorsement.FullAmountOfDebt ?? 0;
                }

                if (Complaints != null)
                {
                    debtRemaining += Complaints.Where(c => c.FullAmountOfDebt != null).Select(c => c.FullAmountOfDebt).Sum(c => c.Value);
                }

                return debtRemaining;
            }
        }
 
        [NotMapped] //The trick. Used for Endorsement and Debtor because they don't have Debtor_Id field to map
        public long? DebtorId
        {
            get { return Id; }
            set { }
        }

        [NotMapped]
        public Debtor AuditDebtor { get { return this; } set { } }
 
     }
}