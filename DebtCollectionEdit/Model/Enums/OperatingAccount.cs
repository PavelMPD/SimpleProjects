using System.ComponentModel.DataAnnotations;

namespace DebtCollection.Model.Enums
{
    public enum OperatingAccount
    {
        [Display(Name = "неизвестно")]
        Unknown = 0,

        [Display(Name = "рс")]
        SettlementAccount = 1,

        [Display(Name = "реестр")]
        Register = 2,

        [Display(Name = "абонентский номер")]
        SubscriberNumber = 3,

        [Display(Name = "рс * 8575")]
        SettlementAccount8575 = 4,

        [Display(Name = "рс * 6450")]
        SettlementAccount6450 = 5,

        [Display(Name = "рс * 8328")]
        SettlementAccount8328 = 6
    }
}