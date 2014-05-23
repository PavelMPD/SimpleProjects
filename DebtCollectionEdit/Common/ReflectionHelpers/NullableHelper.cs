using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace DebtCollection.Common.ReflectionHelpers
{
    public static class NullableHelper
    {
        public static object ToNullable(object value, Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                object result = FormatterServices.GetUninitializedObject(type);
                result = value;
                return result;
            }

            throw new InvalidOperationException();
        }


        public static object ToNullable(string s, Type type)
        {            
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                object result = FormatterServices.GetUninitializedObject(type);

                try
                {
                    if (!string.IsNullOrEmpty(s) && s.Trim().Length > 0)
                    {
                        TypeConverter conv = TypeDescriptor.GetConverter(type);
                        result = conv.ConvertFrom(s);
                    }
                    else
                    {
                        result = null;
                    }
                }
                catch
                {
                }

                return result;
            }

            return s;
        }
    }
}