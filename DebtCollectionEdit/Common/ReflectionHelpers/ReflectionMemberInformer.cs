using System;
using System.Linq.Expressions;

namespace DebtCollection.Common.ReflectionHelpers
{
    public static class ReflectionMemberInformer
    {
        public static MemberInfo GetMemberInfo<T>(
            this T instance,
            Expression<Func<T, object>> expression)
        {
            return GetMemberInfo(expression);
        }

        public static MemberInfo GetMemberInfo<T>(
            Expression<Func<T, object>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentException(
                    "The expression cannot be null.");
            }

            return GetMemberInfo(expression.Body);
        }

        public static MemberInfo GetMemberInfo<T>(
            this T instance,
            Expression<Action<T>> expression)
        {
            return GetMemberInfo(expression);
        }

        public static MemberInfo GetMemberInfo<T>(
            Expression<Action<T>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentException(
                    "The expression cannot be null.");
            }

            return GetMemberInfo(expression.Body);
        }

        private static MemberInfo GetMemberInfo(
            Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentException(
                    "The expression cannot be null.");
            }

            if (expression is MemberExpression)
            {
                // Reference type property or field
                var memberExpression =
                    (MemberExpression)expression;
                
                return new MemberInfo()
                    {
                        MemberName = memberExpression.Member.Name,
                        Type = memberExpression.Type
                    };
            }

            if (expression is MethodCallExpression)
            {
                // Reference type method
                var methodCallExpression =
                    (MethodCallExpression)expression;
                return new MemberInfo()
                    {
                        MemberName = methodCallExpression.Method.Name
                    };
            }

            if (expression is UnaryExpression)
            {
                // Property, field of method returning value type
                var unaryExpression = (UnaryExpression)expression;
                return GetMemberInfo(unaryExpression);
            }

            throw new ArgumentException("Invalid expression");
        }

        private static MemberInfo GetMemberInfo(
            UnaryExpression unaryExpression)
        {
            MemberInfo info = new MemberInfo();

            if (unaryExpression.Operand.Type.IsGenericType &&
                unaryExpression.Operand.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                info.Nullable = true;                
            }

            info.Type = unaryExpression.Operand.Type;

            if (unaryExpression.Operand is MethodCallExpression)
            {
                var methodExpression =
                    (MethodCallExpression)unaryExpression.Operand;

                info.MemberName = methodExpression.Method.Name;
                return info;
            }

            info.MemberName = ((MemberExpression) unaryExpression.Operand)
                .Member.Name;

            return info;
        }
    }
}