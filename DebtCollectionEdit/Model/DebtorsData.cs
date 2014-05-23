using DebtCollection.Common;
using DebtCollection.Model.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebtCollection.Model
{
    [Table("DebtorsData")]
    public class DebtorData : Entity
    {
        /// <summary>
        /// Имя
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Отчество
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// ФИО
        /// </summary>
        [Display(Name = "ФИО")]
        public string FullName { get; set; }

        /// <summary>
        /// Ссылка на карточку должника
        /// </summary>
        public string DebtorDetailsLink { get; set; }

        /// <summary>
        /// MSISDN (Абонентский  номер)
        /// </summary>
        [Display(Name = "Абонентский номер")]
        public string Msisdn { get; set; }

        /// <summary>
        /// Сумма задолженности
        /// </summary>
        [Display(Name = "Сумма задолженности")]
        public long? DebtAmount { get; set; }

        /// <summary>
        /// Дата образования задолженности
        /// </summary>
        [Display(Name = "Дата образования задолженности")]
        public DateTime? DebtDate { get; set; }

        /// <summary>
        /// Контактный номер
        /// </summary>
        [Display(Name = "Контактный номер")]
        public string Phone1 { get; set; }

        /// <summary>
        /// Количество периодов использования услуги life:) Интернет
        /// </summary>
        [Display(Name = "Количество периодов использования услуги life:) Интернет")]
        public int? PeriodsCount { get; set; }

        /// <summary>
        /// Маркетинговое ID (Маркетинговая акция)
        /// </summary>
        [Display(Name = "Маркетинговая акция")]
        public string MarketingAction { get; set; }

        /// <summary>
        /// Активный интернет/микс пакет
        /// </summary>
        [Display(Name = "Активная услуга life:) интернет")]
        public string ActiveService { get; set; }

        /// <summary>
        /// Статус обонента и его суть
        /// </summary>
        [Display(Name = "Статус")]
        public SubscriberStatus SubscriberStatus { get; set; }

        /// <summary>
        /// Дата изменения статуса
        /// </summary>
        [Display(Name = "Дата изменения статуса")]
        public DateTime? SubscriberStatusChangeDate { get; set; }

        /// <summary>
        /// Тарифный план
        /// </summary>
        [Display(Name = "Тарифный план")]
        public string TariffPlan { get; set; }

        /// <summary>
        /// Тип оборудования
        /// </summary>
        [Display(Name = "Тип оборудования")]
        public string DeviceType { get; set; }

        /// <summary>
        /// Номер договора
        /// </summary>
        [Display(Name = "Код контракта")]
        public string AgreementNumber { get; set; }

        /// <summary>
        /// Дата договора
        /// </summary>
        [Display(Name = "Дата контракта")]
        public DateTime? AgreementDate { get; set; }

        /// <summary>
        /// Адрес регистрации
        /// </summary>
        [Display(Name = "Адрес регистрации")]
        public string RegistrationAddress { get; set; }

        /// <summary>
        /// Сумма штрафа, руб.
        /// </summary>
        [Display(Name = " Сумма штрафа, руб.")]
        public decimal AmountOfPenalty { get; set; }

        /// <summary>
        /// Сумма госпошлины, руб.
        /// </summary>
        [Display(Name = "Сумма госпошлины, руб.")]
        public decimal AmountOfDuty { get; set; }

        /// <summary>
        /// Сумма к взысканию, руб.
        /// </summary>
        [Display(Name = "Сумма к взысканию, руб")]
        public decimal? DemandedAmount { get; set; }

        /// <summary>
        /// Общая сумма штрафа, руб 
        /// </summary>
        [Display(Name = "Общая сумма штрафа, руб.")]
        public long TotalFineWithFee { get; set; }

        /// <summary>
        /// Общая сумма госпошлины, поступившая на расчетный счет
        /// </summary>
        [Display(Name = "Общая сумма госпошлины, руб.")]
        public long TotalDutyWithFee { get; set; }

        /// <summary>
        ///  Остаток, руб (на самом деле сумма остатков платежей)
        /// </summary>
        [Display(Name = "Сумма остатка, поступившая на расчетный счет, руб.")]
        public long PaymentsRest { get; set; }

        /// <summary>
        /// "Сумма поступивших платежей, руб
        /// </summary>
        [Display(Name = "Сумма поступивших платежей, руб.")]
        public decimal PaymentsTotal { get; set; }

        // --- New ---//

        /// <summary>
        /// Подстатус обзвона 
        /// </summary>
        [Display(Name = "Подстатус обзвона")]
        public string RollCallSubStatus { get; set; }

        /// <summary>
        /// Дата обзвона
        /// </summary>
        [Display(Name = "Дата обзвона")]
        public DateTime? RollCallDate { get; set; }

        /// <summary>
        /// Почтовый индекс
        /// </summary>
        [Display(Name = "Почтовый индекс")]
        public string PostalCode { get; set; }

        /// <summary>
        /// Дата, когда  выставить претензию
        /// </summary>
        [Display(Name = "Дата, когда  выставить претензию")]
        public DateTime? ClaimWhenPutDate { get; set; }

        /// <summary>
        /// Дата выставления претензии
        /// </summary>
        [Display(Name = "Дата выставления претензии")]
        public DateTime? ClaimDate { get; set; }

        /// <summary>
        /// Дата, до которой необходимо было оплатить претензиию
        /// </summary>
        [Display(Name = "Дата, до которой необходимо было оплатить претензиию")]
        public DateTime? ClaimNecessaryToPay { get; set; }

        /// <summary>
        /// Наименоване документа удостоверяющего личность
        /// </summary>
        [Display(Name = "Наименоване документа удостоверяющего личность")]
        public string CertificateType { get; set; }

        /// <summary>
        /// Номер документа удостоверяющего личность
        /// </summary>
        [Display(Name = "Номер документа удостоверяющего личность")]
        public string CertificateId { get; set; }

        /// <summary>
        /// Дата выдачи документа удостоверяющего личность
        /// </summary>
        [Display(Name = "Дата выдачи документа удостоверяющего личность")]
        public DateTime? CertificateIssueDate { get; set; }

        /// <summary>
        /// Орган, выдавший документ удостоверяющий личность
        /// </summary>
        [Display(Name = "Орган, выдавший документ удостоверяющий личность")]
        public string Authority { get; set; }

        /// <summary>
        /// Идентификационный номер (в паспорте)
        /// </summary>
        [Display(Name = "Идентификационный номер")]
        public string IdentificationNumber { get; set; }

        /// <summary>
        /// Сумма штрафа по решению, руб. (In)
        /// </summary>
        [Display(Name = "Сумма штрафа по решению, руб")]
        public decimal? AmountOfPenaltyByIn { get; set; }

        /// <summary>
        /// Номер ИН
        /// </summary>
        [Display(Name = "Номер ИН")]
        public string InNumber { get; set; }

        /// <summary>
        /// Дата ИН
        /// </summary>
        [Display(Name = "Дата ИН")]
        public DateTime? InDate { get; set; }

        /// <summary>
        /// Наименование нотариальной конторы
        /// </summary>
        [Display(Name = "Наименование нотариальной конторы")]
        public string NameOfNotaryOffice { get; set; }

        /// <summary>
        /// ФИО нотариуса
        /// </summary>
        [Display(Name = "ФИО нотариуса")]
        public string NameOfNotaryPerson { get; set; }

        /// <summary>
        /// Подсудность
        /// </summary>
        [Display(Name = "Подсудность")]
        public string Jurisdiction { get; set; }

        /// <summary>
        /// Полная сумма долга, руб.    
        /// (Осталось оплатить по ИН, руб.)
        /// </summary>
        [Display(Name = "Осталось оплатить по ИН, руб.")]
        public decimal? FullAmountOfDebt { get; set; }

        /// <summary>
        /// Полная сумма долга, руб.    
        /// (Осталось оплатить, руб.) = Иски + ИН
        /// </summary>
        [Display(Name = "Осталось оплатить, руб.")]
        public decimal? FullAmountOfDebtComplaintEndorsement { get; set; }

        /// <summary>
        /// Дата последнего платежа
        /// </summary>
        [Display(Name = "Дата последнего платежа")]
        public DateTime? LastPaymentDate { get; set; }

        /// <summary>
        /// Cтадия (In)
        /// </summary>
        [Display(Name = "Cтадия")]
        public Stage? Stage { get; set; }

        /// <summary>
        /// Код контракта (=CO_ID, уникальное поле для поиска в биллинге)
        /// </summary>
        [Display(Name = "CO_ID")]
        public long ContractCode { get; set; }
        
        public string StageAsSktring
        {
            get { return Stage.HasValue ? Stage.Value.GetDisplayName() : ""; }
        }

        /// <summary>
        /// Дата изменения стадии (In)
        /// </summary>
        [Display(Name = "Дата изменения стадии")]
        public DateTime? StageChangeDate { get; set; }
    }
}