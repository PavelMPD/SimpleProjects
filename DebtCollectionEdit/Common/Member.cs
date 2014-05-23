using System;
using System.Linq.Expressions;
using DebtCollection.Common.Constants;

namespace DebtCollection.Common
{
    public static class Member
    {
        public enum FormattingWrapper
        {
            None,
            Value,
            Currency,
            Date
        }

        public static string GetFullName<TProp>(Expression<Func<TProp, object>> expression, FormattingWrapper useFormattingWrapper = FormattingWrapper.None, bool wrapVariableWithCurlyBraces = false, string pluralEndingForBaseClass = "", bool includeClassName = true) where TProp : class
        {
            Expression memberExpression;

            if (expression.Body.NodeType == ExpressionType.Convert)
            {
                memberExpression = ((UnaryExpression)expression.Body).Operand;
            }
            else if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = expression.Body;
            }
            else
            {
                throw new ArgumentException("Expression points to method");
            }

            var fullName = string.Format("{0}{1}{2}",
                includeClassName ? expression.Parameters[0].Type.Name : "",
                pluralEndingForBaseClass + (includeClassName ? "." : ""),
                memberExpression.ToString().Substring(expression.Parameters[0].Name.Length + 1));

            return AddFormattingWrapper(fullName, useFormattingWrapper, wrapVariableWithCurlyBraces);
        }

        public static string GetName<TProp>(Expression<Func<TProp, object>> expression, FormattingWrapper useFormattingWrapper = FormattingWrapper.None, bool wrapVariableWithCurlyBraces = false) where TProp : class
        {
            MemberExpression memberExpression;

            if (expression.Body.NodeType == ExpressionType.Convert)
            {
                memberExpression = (MemberExpression)((UnaryExpression)expression.Body).Operand;
            }
            else if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = (MemberExpression)expression.Body;
            }
            else
            {
                throw new ArgumentException("Expression points to method");
            }

            return AddFormattingWrapper(memberExpression.Member.Name, useFormattingWrapper);
        }

        private static string AddFormattingWrapper(string name, FormattingWrapper useFormattingWrapper, bool wrapVariableWithCurlyBraces = false)
        {
            string retVal = "";
            switch (useFormattingWrapper)
            {
                case FormattingWrapper.None:
                    retVal = name;
                    break;
                case FormattingWrapper.Value:
                    retVal = WithValueFormattingWrapper(name, wrapVariableWithCurlyBraces);
                    break;
                case FormattingWrapper.Currency:
                    retVal = WithCurrencyFormattingWrapper(name, wrapVariableWithCurlyBraces);
                    break;
                case FormattingWrapper.Date:
                    retVal = WithDateFormattingWrapper(name, wrapVariableWithCurlyBraces: wrapVariableWithCurlyBraces);
                    break;
            }
            return retVal;
        }

        public static string WithDateFormattingWrapper(string property, string format = SystemConstants.StandartDate, bool wrapVariableWithCurlyBraces = false)
        {
            if (!wrapVariableWithCurlyBraces)
            {
                return String.Format("#= Display.date({0}, '{1}') #", property, format);
            }

            return String.Format("Display.date(${{{0}}}, '{1}')", property, format);
        }

        public static string WithCurrencyFormattingWrapper(string property, bool wrapVariableWithCurlyBraces = false)
        {
            if (!wrapVariableWithCurlyBraces)
            {
                return String.Format("#= Display.currency({0}) #", property);
            }

            return String.Format("Display.currency(${{{0}}})", property);
        }

        public static string WithValueFormattingWrapper(string property, bool wrapVariableWithCurlyBraces = false)
        {
            if (!wrapVariableWithCurlyBraces)
            {
                return String.Format("#= Display.value({0}) #", property);
            }

            return String.Format("Display.value(${{{0}}})", property);
        }
    }
}
