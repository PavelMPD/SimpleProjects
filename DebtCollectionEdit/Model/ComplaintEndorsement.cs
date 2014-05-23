using DebtCollection.Model.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebtCollection.Model
{
    [Table("ComplaintsEndorsements")]
    public class ComplaintEndorsement : Entity
    {

        public long DocId { get; set; }
        /// <summary>
        /// ФИО
        /// </summary>
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }

        public long DebtorId { get; set; }

        /// <summary>
        /// Абонентский номер
        /// </summary>
        [Display(Name = "Абонентский номер")]
        public string MSISDN { get; set; }

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
        /// Адрес регистрации
        /// </summary>
        [Display(Name = "Адрес регистрации")]
        public string RegistrationAddress { get; set; }

        /// <summary>
        /// Тип документа
        /// </summary>
        [Display(Name = "Тип")]
        public string DocType { get; set; }

        /// <summary>
        /// ИД типа документа
        /// </summary>
        public int DocTypeId { get; set; }

        /// <summary>
        /// Номер иска/ИН
        /// </summary>
        [Display(Name = "Номер иска/ИН")]
        public string INNumber { get; set; }

        /// <summary>
        /// Дата иска/ИН
        /// </summary>
        [Display(Name = "Дата иска/ИН")]
        public DateTime? INDate { get; set; }

        /// <summary>
        /// Наименование нотариальной конторы
        /// </summary>
        [Display(Name = "Наименование нотариальной конторы")]
        public string NameOfNotaryOffice { get; set; }

        /// <summary>
        /// Подсудность
        /// </summary>
        [Display(Name = "Текущая подсудность")]
        public string Jurisdiction { get; set; }

        /// <summary>
        /// Регион
        /// </summary>
        [Display(Name = "Регион текущей подсудности")]
        public string Region { get; set; }

        /// <summary>
        /// Первоначальная подсудность
        /// </summary>
        [Display(Name = "Первоначальная подсудность")]
        public string InitialJurisdiction { get; set; }

        /// <summary>
        /// Регион первоначальной подсудности
        /// </summary>
        [Display(Name = "Регион первоначальной подсудности")]
        public string InitialRegion { get; set; }

        /// <summary>
        /// ФИО судьи/нотариуса
        /// </summary>
        [Display(Name = "ФИО судьи/нотариуса")]
        public string NameOfJurPerson { get; set; }

        /// <summary>
        /// Дата вынесения решения суда
        /// </summary>
        [Display(Name = "Дата вынесения решения суда")]
        public DateTime? DateOfCourtDecision { get; set; }

        /// <summary>
        /// Сумма штрафа, руб.
        /// </summary>
        [Display(Name = "Сумма штрафа, руб.")]
        public decimal AmountOfPenalty { get; set; }

        /// <summary>
        /// Сумма госпошлины, руб.
        /// </summary>
        [Display(Name = "Сумма госпошлины, руб.")]
        public decimal AmountOfDuty { get; set; }

        /// <summary>
        /// Сумма штрафа по решению/ИН, руб.
        /// </summary>
        [Display(Name = "Сумма штрафа по решению/ИН, руб.")]
        public decimal AmountOfPenaltyByDecision { get; set; }

        /// <summary>
        /// Сумма госпошлины по решению/ИН, руб.
        /// </summary>
        [Display(Name = "Сумма госпошлины по решению/ИН, руб.")]
        public decimal AmountOfDutyByDecision { get; set; }

        /// <summary>
        /// Сумма к взысканию, руб.
        /// </summary>
        [Display(Name = "Сумма к взысканию, руб.")]
        public decimal DemandedAmount { get; set; }

        /// <summary>
        /// Осталось оплатить, руб.
        /// </summary>
        [Display(Name = "Осталось оплатить, руб.")]
        public decimal FullAmountOfDebt { get; set; }

        /// <summary>
        /// Примечание 
        /// </summary>
        [Display(Name = "Примечание")]
        public string Note { get; set; }

        /// <summary>
        /// Сумма поступивших платежей
        /// </summary>
        [Display(Name = "Сумма поступивших платежей, руб.")]
        public decimal? PaymentsAmount { get; set; }

        /// <summary>
        /// Дата последнего платежа
        /// </summary>
        [Display(Name = "Дата последнего платежа")]
        public DateTime? LastPaymentDate { get; set; }

        /// <summary>
        /// Стадия
        /// </summary>
        [Display(Name = "Стадия")]
        public Stage? Stage { get; set; }

        /// <summary>
        /// Дата изменения стадии
        /// </summary>
        [Display(Name = "Дата изменения стадии")]
        public DateTime? StageChangeDate { get; set; }

        /// <summary>
        /// Приказное производство
        /// </summary>
        [Display(Name = "Приказное производство")]
        public string MarkOfRecovery { get; set; }

        /// <summary>
        /// Расходы на розыск
        /// </summary>
        [Display(Name = "Расходы на розыск")]
        public decimal? CostOfSearch { get; set; }

        /// <summary>
        /// Дата направления ИЛ
        /// </summary>
        [Display(Name = "Место нахождения ИЛ")]
        public DateTime? ExecutionListDate { get; set; }

        /// <summary>
        /// История ИД
        /// </summary>
        [Display(Name = "История ИД")]
        public string ExecutionListLocation { get; set; }

        /// <summary>
        /// Номер договора
        /// </summary>
        [Display(Name = "Номер договора")]
        public long? AgreementNumber { get; set; }

        /// <summary>
        /// Дата договора
        /// </summary>
        [Display(Name = "Дата договора")]
        public DateTime? AgreementDate { get; set; }

        /// <summary>
        /// Создать запрос (напоминания)
        /// </summary>
        [Display(Name = "Создать запрос")]
        public DateTime? NextActionDate { get; set; }

        /// <summary>
        /// Дата договора (напоминания)
        /// </summary>
        [Display(Name = "Кол-во напоминаний")]
        public int? NextActionCounter { get; set; }

        public bool TreatAsDebt { get; set; }

        /// <summary>
        /// Организация
        /// </summary>
        [Display(Name = "Организация")]
        public string Organisation { get; set; }

        /// <summary>
        /// Адрес организации
        /// </summary>
        [Display(Name = "Адрес организации")]
        public string OrganisationAddress { get; set; }

        /// <summary>
        /// Сущность судебного решения
        /// </summary>
        [Display(Name = "Сущность судебного решения")]
        public string CourtDecision { get; set; }

        /// <summary>
        /// Когда выставлено
        /// </summary>
        [Display(Name = "Когда выставлено")]
        public int? WhenPutYear { get; set; }

        /// <summary>
        /// Код контракта (=CO_ID, уникальное поле для поиска в биллинге)
        /// </summary>
        [Display(Name = "CO_ID")]
        public long ContractCode { get; set; }
    }
}