using System;
using System.Linq;

using DebtCollection.Common;
using DebtCollection.Model.Enums;

namespace DebtCollection.Model.Attributes
{
    public class AllowedForDocumentAttribute : Attribute
    {
        public AllowedForDocumentAttribute(SubDocumentAllowedType subDocumentAllowed)
        {
            SubDocumentAllowed = subDocumentAllowed;
        }

        public SubDocumentAllowedType SubDocumentAllowed { get; set; }
    }

    public static class GetSubDocumentAllowedTypeExtension
    {
        public static SubDocumentAllowedType GetSubDocumentAllowedType(this Enum enumValue, 
                                                             SubDocumentAllowedType defaultValue = SubDocumentAllowedType.All)
        {
            var member = enumValue.GetType().GetMember(enumValue.ToString())[0];
            var attribute = member.GetCustomAttributes(typeof(AllowedForDocumentAttribute), false);
            if (!attribute.Any())
            {
                return defaultValue;
            }
            return ((AllowedForDocumentAttribute)attribute[0]).SubDocumentAllowed;
        }
    }
}
