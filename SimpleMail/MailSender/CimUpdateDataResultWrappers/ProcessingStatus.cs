using System.ComponentModel.DataAnnotations;

namespace DebtCollection.Model.Enums
{
    public enum ProcessingStatus
    {
        [Display(Name = "неизвестно")]
        Unknown = 0,
        [Display(Name = "успешно")]
        Successful = 1,
        [Display(Name = "частично")]
        Warning = 2,
        [Display(Name = "не успешно")]
        Fail = 3
    }
}