using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace DebtCollection.Model
{
    [DataContract]
    public enum EdmsRegistrationType
    {
        [EnumMember]
        [Display(Name = "Без регистрации")]
        NoRegistration,

        [EnumMember]
        [Display(Name = "Автоматическая")]
        Automatic,

        [EnumMember]
        [Display(Name = "Ручная(через Excel файл)")]
        Manual
    }
}