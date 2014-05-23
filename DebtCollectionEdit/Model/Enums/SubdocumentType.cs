using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using DebtCollection.Common;
using DebtCollection.Model.Attributes;

namespace DebtCollection.Model.Enums
{
    

    [DataContract]
    public enum SubdocumentType
    {
        [Display(Name = " ")]
        [AllowedForDocument(SubDocumentAllowedType.All)]
        [EnumMember]
        Unknown,

        [Display(Name = "Сопроводительное письмо для направления исполнительной  надписи в суд")]
        [AllowedForDocument(SubDocumentAllowedType.Endorsement)]
        [EnumMember]
        CoverLetter,

        [Display(Name = "Письмо об оплате")]
        [AllowedForDocument(SubDocumentAllowedType.All)]
        [EnumMember]
        Subdoc2,

        [Display(Name = "Письмо о направлении на работу без проверки имущественного положения")]
        [AllowedForDocument(SubDocumentAllowedType.All)]
        [EnumMember]
        Subdoc3,

        [Display(Name = "Письмо на розыск")]
        [AllowedForDocument(SubDocumentAllowedType.All)]
        [EnumMember]
        Subdoc4,

        [Display(Name = "Отзыв исполнительного документа")]
        [AllowedForDocument(SubDocumentAllowedType.All)]
        [EnumMember]
        Subdoc5,

        [Display(Name = "Ответ в суд о непоступлении денежных средств")]
        [AllowedForDocument(SubDocumentAllowedType.All)]
        [EnumMember]
        Subdoc6,

        [Display(Name = "Заявление о принятии к исполнению и объявлению в розыск")]
        [AllowedForDocument(SubDocumentAllowedType.All)]
        [EnumMember]
        Subdoc7,

        [Display(Name = "Заявление на дубликат исполнительного листа при утере на работе")]
        [AllowedForDocument(SubDocumentAllowedType.Complaint)]
        [EnumMember]
        [SingleGeneration]
        Subdoc8,

        [Display(Name = "Заявление на дубликат исполнительного листа при проверки бухгалтерии")]
        [EnumMember]
        [SingleGeneration]
        [AllowedForDocument(SubDocumentAllowedType.Complaint)]
        Subdoc9,

        [Display(Name = "Заявление на дубликат исполнительного листа при направлении по тер-ти")]
        [AllowedForDocument(SubDocumentAllowedType.Complaint)]
        [EnumMember]
        Subdoc10,

        [Display(Name = "Заявление на дубликат исполнительного листа при возвращении без исполнения")]
        [AllowedForDocument(SubDocumentAllowedType.Complaint)]
        [EnumMember]
        Subdoc11,

        [Display(Name = "Заявление в суд о проверке бухгалтерии")]
        [AllowedForDocument(SubDocumentAllowedType.All)]
        [EnumMember]
        [SingleGeneration]
        Subdoc12,

        [Display(Name = "Заявление в суд для повторного исполнения")]
        [AllowedForDocument(SubDocumentAllowedType.All)]
        [EnumMember]
        Subdoc13,

        [Display(Name = "Запрос на квитанции")]
        [AllowedForDocument(SubDocumentAllowedType.All)]
        [EnumMember]
        Subdoc14,

        [Display(Name = "Запрос в суд о результатах исполнения решения суда (кроме статуса - направлен в другой суд)")]
        [AllowedForDocument(SubDocumentAllowedType.Complaint)]
        [EnumMember]
        Subdoc15,

        [Display(Name = "Запрос в суд о результатах исполнения решения суда (для статуса - направлен в другой суд)")]
        [AllowedForDocument(SubDocumentAllowedType.Complaint)]
        [EnumMember]
        Subdoc16,

        [Display(Name = "Запрос в суд о результатах исполнения исполнительной надписи (статус - направлен в другой суд)")]
        [AllowedForDocument(SubDocumentAllowedType.Endorsement)]
        [EnumMember]
        Subdoc17,

        [Display(Name = "Запрос в суд о результатах исполнения исполнительной надписи (кроме статуса - направлен в другой суд)")]
        [AllowedForDocument(SubDocumentAllowedType.Endorsement)]
        [EnumMember]
        Subdoc18,

        [Display(Name = "Запрос в суд на предоставление постановления о возвращении ид и акта о невозможности взыскания")]
        [AllowedForDocument(SubDocumentAllowedType.All)]
        [EnumMember]
        Subdoc19,

        [Display(Name = "Запрос в суд на предоставление постановления о возвращении ид")]
        [AllowedForDocument(SubDocumentAllowedType.All)]
        [EnumMember]
        Subdoc20,

        [Display(Name = "Запрос в суд на предоставление даты направления ид по тер-ти")]
        [AllowedForDocument(SubDocumentAllowedType.All)]
        [EnumMember]
        Subdoc21,

        [Display(Name = "Запрос в суд на предоставление даты направления ид в организацию")]
        [AllowedForDocument(SubDocumentAllowedType.All)]
        [EnumMember]
        [SingleGeneration]
        Subdoc22,

        [Display(Name = "Запрос в суд на предоставление акта о невозможности взыскания")]
        [AllowedForDocument(SubDocumentAllowedType.All)]
        [EnumMember]
        Subdoc23,

        [Display(Name = "Запрос в суд на предоставление адреса организации")]
        [AllowedForDocument(SubDocumentAllowedType.All)]
        [EnumMember]
        [SingleGeneration]
        Subdoc24,

        [Display(Name = "Запрос в организацию об удержаниях")]
        [AllowedForDocument(SubDocumentAllowedType.All)]
        [EnumMember]
        [SingleGeneration]
        Subdoc25,

        [Display(Name = "Запрос на рс")]
        [AllowedForDocument(SubDocumentAllowedType.All)]
        [EnumMember]
        Subdoc26,

        [Display(Name = "Отзыв исполнительного документа")]
        [AllowedForDocument(SubDocumentAllowedType.All)]
        [EnumMember]
        Subdoc27,

        [Display(Name = "Запрос в организацию на платежные поручения")]
        [AllowedForDocument(SubDocumentAllowedType.All)]
        [EnumMember]
        Subdoc28
    }
}