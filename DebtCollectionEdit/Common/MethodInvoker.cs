using System;
using System.Reflection;

namespace DebtCollection.Common
{
    public static class MethodInvoker
    {
        public static object InvokeStaticMethod(System.Type type, string methodName,
                                                params object[] args)
        {
            return InvokeMethod(type, null, methodName,
                                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                                args);
        }

        public static object SetProperty(Type type, object objectInstance,
            string propertyName, params object[] args)
        {
            return  InvokeInstanceMethod(type, objectInstance, "set_" + propertyName, args);
        }

        public static object GetProperty(Type type, object objectInstance,
            string propertyName)
        {
            return InvokeInstanceMethod(type, objectInstance, "get_" + propertyName);
        }

        public static object InvokeInstanceMethod(System.Type type, object objectInstance,
                                                  string methodName, params object[] args)
        {
            return InvokeMethod(type, objectInstance, methodName,
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                args);
        }

        private static object InvokeMethod(System.Type type, object objectInstance,
                                           string methodName, BindingFlags flags, params object[] args)
        {
            try
            {
                MethodInfo methodInfo = type.GetMethod(methodName, flags);
                if (methodInfo == null)
                {
                    throw new ArgumentException("Type " + type.ToString() +
                                                " doesn't contains method " + methodName);
                }

                object objRet = methodInfo.Invoke(objectInstance, args);
                return objRet;
            }
            catch
            {
                throw;
            }
        }
    }
}