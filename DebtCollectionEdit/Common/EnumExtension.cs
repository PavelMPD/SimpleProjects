using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DebtCollection.Common
{
    public static class EnumExtension
    {
        public static string GetDisplayName(this Enum enumValue, string defaultValue = "")
        {
            var member = enumValue.GetType().GetMember(enumValue.ToString())[0];
            var displayAttribute = member.GetCustomAttributes(typeof(DisplayAttribute), false);
            if (!displayAttribute.Any())
            {
                return defaultValue;
            }
            return ((DisplayAttribute)displayAttribute[0]).Name;
        }

        public static int GetOrderValue(this Enum enumValue, int defaultValue = 0)
        {
            var member = enumValue.GetType().GetMember(enumValue.ToString())[0];
            var orderAtribute = member.GetCustomAttributes(typeof(OrderAttribute), false);
            if (!orderAtribute.Any())
            {
                return defaultValue;
            }
            return ((OrderAttribute)orderAtribute[0]).Order;
        }

        public static bool IsMultipleGenerationAllowed(this Enum enumValue)
        {
            var member = enumValue.GetType().GetMember(enumValue.ToString())[0];
            var singleGenerationAttribute = member.GetCustomAttributes(typeof(SingleGenerationAttribute), false);
            if (!singleGenerationAttribute.Any())
            {
                return true;
            }
            return false;
        }
    }
}
