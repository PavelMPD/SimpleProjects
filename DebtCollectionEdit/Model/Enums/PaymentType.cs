using System.ComponentModel.DataAnnotations;

namespace DebtCollection.Model.Enums
{
    public enum PaymentType
    {
        [Display(Name = "")]
        Unknown = 0,

        [Display(Name = "Безналичный платеж через EASYPAY")]
        CashlessEasyPay = 1,

        [Display(Name = "Безналичный платеж по расписанию")]
        CashlessRecurring = 2,

        [Display(Name = "Безналичный платеж (Автоплатежи)")]
        CashlessAuto = 3,

        [Display(Name = "Безналичный платёж с использованием eMoney")]
        Webmoney = 4,

        [Display(Name = "Наличными через устройства cash-in")]
        CashIn = 5,

        [Display(Name = "Безналичный платеж через Internet")]
        CashlessInternet = 6,

        [Display(Name = "Безналичный платеж через банкомат")]
        CashlessAtm = 7,

        [Display(Name = "Наличными")]
        Cash = 8,

        [Display(Name = "Платеж через Ezetop")]
        Ezetop = 9,

        [Display(Name = "За оборудование")]
        Equipment = 10,

        [Display(Name = "Безналичный платеж через WAP ")]
        CashlessWap = 11,

        [Display(Name = "Безналичный платеж через POS-терминал")]
        CashlessPos = 12,

        [Display(Name = "Безналичный расчет")]
        Cashless = 13,
    }
}
