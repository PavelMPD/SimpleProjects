using System.ComponentModel.DataAnnotations;

namespace DebtCollection.Model.Enums
{
    public enum Stage
    {
        //Неизвестна
        [Display(Name = " ")]
        Unknown = 0,

        [Display(Name = "Прекращено в связи со смертью абонента")]
        StoppedBecauseOfDeath = 1,

        [Display(Name = "Без исполнения")]
        WithNoExecution = 2,

        [Display(Name = "Удержание по месту работы")]
        WithholdingOnWorkplace = 3,

        [Display(Name = "Направлено в другой суд")]
        RedirectedToOtherCourt = 4,

        [Display(Name = "Розыск")]
        Searching = 5,

        [Display(Name = "Без исполнения/Повторно в суд")]
        WithNoExecutionRepeatToCourt = 6,

        [Display(Name = "Без исполнения/Розыск")]
        WithNoExecutionInSearch = 7,

        [Display(Name = "На исполнении")]
        Executing = 8,

        [Display(Name = "Дубликат")]
        Dublicate = 9,

        [Display(Name = "Проверка оплаты")]
        CheckingPayment = 10,

        // DELETED!
        //[Display(Name = "Корректировка штрафа")]
        //FineCorrection = 11,

        [Display(Name = "Приостановлено до установления правопреемника должника")]
        SuspendedOther = 12,

        [Display(Name = "Иные основания приостановления")]
        Suspended = 13,

        [Display(Name = "Иные основания прекращения")]
        OtherReasons = 14,

        [Display(Name = "Запрос")]
        Request = 15,

        [Display(Name = "Акция")]
        Action = 16,

        [Display(Name = "Оплата поступила")]
        Paid = 17,

        [Display(Name = "Контроль исполнения")]
        ExecutionControl = 18,

        [Display(Name = "Запрос на квитанции")]
        ReceiptRequest   = 19,

        [Display(Name = "Квитанции в суде")]
        ReceiptsInCourt = 20,

        [Display(Name = "Запрос на акт")]
        RequestToAct = 21,

        [Display(Name = "Запрос на рс")]
        RequestForAccount = 22,

        [Display(Name = "Не оплачиваем розыск")]
        DoNotPayForSearch = 23,

        [Display(Name = "Возврат дс")]
        CashBack = 24,

        [Display(Name = "Проверка бухгалтерии")]
        CheckingAccounts = 25,

        [Display(Name = "Выставлена ИН")]
        InCreated = 26,

        [Display(Name = "Решение суда вынесено")]
        CourtDecisionPassed = 27,

        [Display(Name = "Уменьшение штрафа")]
        PenaltyDecrease = 28
    }
}
