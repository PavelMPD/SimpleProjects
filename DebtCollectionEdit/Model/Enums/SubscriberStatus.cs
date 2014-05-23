using DebtCollection.Common;
using System.ComponentModel.DataAnnotations;

namespace DebtCollection.Model.Enums
{
    public enum SubscriberStatus
    {
        [Display(Name = "Без статуса")]
        [Order(1)]
        Unknown = 0,

        [Display(Name = "Обзвонить")]
        [Order(0)]
        RequiresRollCall = 1,

        [Display(Name = "Обзвонили")]
        [Order(4)]
        RollCallComplete = 2,

        [Display(Name = "Выставить претензию")]
        [Order(6)]
        RequiresClaim = 3,

        [Display(Name = "Претензия выставлена")]
        [Order(7)]
        Claimed = 4,

        [Display(Name = "Исполнительное производство")]
        [Order(13)]
        ExecutionProcedure = 5,

        [Display(Name = "ИН выставлена")]
        [Order(12)]
        InCreated = 6,

        [Display(Name = "Нет договора")]
        [Order(9)]
        NoAgreement = 7,

        [Display(Name = "Требуется обнуление")]
        [Order(14)]
        NullingRequired = 9,

        [Display(Name = "Обнулен")]
        [Order(17)]
        Nulled = 10,

        [Display(Name = "Подготовить к ИН")]
        [Order(8)]
        RequiresIn = 11,

        [Display(Name = "Нет контактного телефона")]
        [Order(3)]
        NoContactPhone = 13,

        [Display(Name = "Задолженность погашена")]
        [Order(15)]
        DebtPaid = 14,

        [Display(Name = "Оплачено по претензии")]
        [Order(16)]
        ClaimPaid = 15,

        [Display(Name = "Выставить ИН")]
        [Order(10)]
        CreateIn = 17,

        [Display(Name = "Оплата госпошлины")]
        [Order(11)]
        StateDutyPaid = 18,

        [Display(Name = "Подготовить к претензии")]
        [Order(5)]
        PrepareForClaim = 19,

        [Display(Name = "Dummy")]
        [Order(2)]
        Dummy = 20
    }
}
