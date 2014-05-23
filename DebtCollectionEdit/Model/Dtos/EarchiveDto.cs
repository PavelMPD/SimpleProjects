using DebtCollection.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace DebtCollection.Model.Dtos
{
    public class EarchiveDto : TrackableTuple, IHousable
    {
        [Display(Name = "Телефонный номер абонента")]
        public long? SubscriberId { get; set; }

        [Display(Name = "Вид документа")]
        public string CertificateType { get; set; }

        [Display(Name = "Дата  выдачи паспорта")]
        public DateTime? CertificateIssueDate { get; set; }

        [Display(Name = "Номер паспорта")]
        public string CertificateId { get; set; }

        [Display(Name = "Орган выдавший паспорт")]
        public string Authority { get; set; }

        [Display(Name = "Идентификационный номер")]
        public string IdentificationNumber { get; set; }

        [Display(Name = "Дата рождения")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "ФИО абонента")]
        public string Fullname { get; set; }

        [Display(Name = "Адрес абонента")]
        public string RegistrationAddress { get; set; }

        [Display(Name = "СУД")]
        public string JurisdictionName { get; set; }

        [Display(Name = "Адрес суда")]
        public string JurisdictionAddress { get; set; }

        [Display(Name = "Контактный тел 1")]
        public string Phone { get; set; }

        [Display(Name = "Номер контракта")]
        public string AgreementNumber { get; set; }

        [Display(Name = "Дата контракта")]
        public DateTime? ContractDate { get; set; }

        [Display(Name = "Долг")]
        public decimal AmountOfPenalty { get; set; }

        [Display(Name = "Тип оборудования")]
        public string DeviceType { get; set; }

        [Display(Name = "Статус обработки")]//Обработан, Ошибка
        public string ProcessingStatus { get; set; }

        [Display(Name = "Примечания обработки")]
        public string ProcessingNotes { get; set; }
    }
}