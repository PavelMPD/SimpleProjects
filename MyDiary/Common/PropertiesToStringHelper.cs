using System;
using System.Text;

namespace MyDiary.Common
{
    public static class PropertiesToStringHelper
    {
        public static String PropertiesToString(this Object obj)
        {
            var sb = new StringBuilder();
            foreach (var property in obj.GetType().GetProperties())
            {
                sb.AppendLine(String.Format("{0}='{1}'", property.Name, property.GetValue(obj)));
            }
            return sb.ToString();
        }
    }
}
