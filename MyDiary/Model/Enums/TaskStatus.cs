using System.ComponentModel.DataAnnotations;

namespace MyDiary.Model.Enums
{
    public enum TaskStatus
    {
        [Display(Name="Новая")]
        IsNew = 1,
        [Display(Name="В работе")]
        InProgress = 2,
        [Display(Name = "Выполнена")]
        Succeeded = 3
    }
}
