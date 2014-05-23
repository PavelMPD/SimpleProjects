using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace DebtCollection.Common
{
    public class OrderAttribute : Attribute
    {
        public OrderAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; set; }
    }

    // This attribute indicates that subdocument could be generated only
    // for one document per operation
    public class SingleGenerationAttribute : Attribute
    {
    }

    public static class EnumHelper
    {



        public static Array GetValuesByOrder<T>(Type enumType)
        {
            IOrderedEnumerable<T> array = Enum.GetValues(enumType).Cast<T>().OrderBy(EnumHelper.GetOrderValue);

            return array.ToArray();
        }
        public static int GetOrderValue<T>(T value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            var attributes = fieldInfo.GetCustomAttributes(
                typeof(OrderAttribute), false) as OrderAttribute[];

            if (attributes == null) return 0;
            return attributes[0].Order;
        }
        public static T GetValueFromDescription<T>(string displayName, T defaultValue)
        {
            if (String.IsNullOrEmpty(displayName))
            {
                throw new ArgumentException("Parameter can't be null", "displayName");
            }

            displayName = displayName.Trim().ToUpperInvariant();

            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (FieldInfo field in type.GetFields())
            {
                Attribute customAttribute = field.GetCustomAttribute(
                    typeof(DisplayAttribute), false);

                if (customAttribute != null)
                {
                    if (((DisplayAttribute)customAttribute).Name.ToUpperInvariant() == displayName)
                        return (T)field.GetValue(null);
                }
            }
            return defaultValue;
        }
        public static string GetDisplayValue<T>(T value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            var descriptionAttributes = fieldInfo.GetCustomAttributes(
                typeof(DisplayAttribute), false) as DisplayAttribute[];

            if (descriptionAttributes == null) return string.Empty;
            return (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Name : value.ToString();
        }
    }
}