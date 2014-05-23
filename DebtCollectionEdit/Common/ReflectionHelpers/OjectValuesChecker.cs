using System;
using System.Reflection;

namespace DebtCollection.Common.ReflectionHelpers
{
    public static class OjectValuesChecker
    {
        public static bool TheSameAsDefault<T>(T testingObject)
            where T: class, new()
        {
            return ReflectiveEquals(testingObject, new T());
        }

        public static bool ReflectiveEquals(object first, object second)
        {
            if (first == null && second == null)
            {
                return true;
            }
            if (first == null || second == null)
            {
                return false;
            }
            Type firstType = first.GetType();
            if (second.GetType() != firstType)
            {
                return false;
            }
            
            foreach (PropertyInfo propertyInfo in firstType.GetProperties())
            {
                if (propertyInfo.CanRead)
                {
                    object firstValue = propertyInfo.GetValue(first, null);
                    object secondValue = propertyInfo.GetValue(second, null);
                    if (!object.Equals(firstValue, secondValue))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}