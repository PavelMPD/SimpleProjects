using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using DebtCollection.Common.ReflectionHelpers;
using System.Linq;

namespace DebtCollection.Common
{
    public abstract class TrackableTuple
    {    
        private List<string> errors;

        public Dictionary<string, string> Empties { get; set; }

        public int RowId { get; set; }

        public void AddError(string errorMessage)
        {
            Errors.Add(errorMessage);
        }

        public List<string> Errors
        {
            get
            {
                if (errors == null)
                {
                    errors = new List<string>();
                }

                return errors;
            }
        }

        public bool IsFieldEmpty<T>(Expression<Func<T, object>> field)
        {
            MemberInfo memberInfo = ReflectionMemberInformer.GetMemberInfo(field);

            return IsFieldEmpty(memberInfo.MemberName);
        }

        public bool IsFieldEmpty(string fieldName)
        {
            if (Empties.Values.Contains(fieldName))
            {
                return true;
            }

            return false;
        }

        public string GetColumnNameByExpression<T>(Expression<Func<T, object>> field)
        {
            MemberInfo memberInfo = ReflectionMemberInformer.GetMemberInfo(field);

            KeyValuePair<string, string> keyValue = 
                Empties.Single(keyValuePair => keyValuePair.Value == memberInfo.MemberName);

            return keyValue.Key;
        }

        public string ErrorsMessage
        {
            get
            {
                if (Errors.Count == 0)
                {
                    return "Нет ошибок";
                }

                StringBuilder builder = new StringBuilder();
                foreach (string error in Errors)
                {
                    builder.Append(error);
                    builder.AppendLine();
                }

                return builder.ToString();

            }
        }

        public string EmptiesMessage
        {
            get
            {
                if (Empties.Count == 0)
                {
                    return "Нет пустых полей";
                }

                StringBuilder builder = new StringBuilder();                

                foreach (string emptieHeader in Empties.Keys)
                {
                    builder.Append(emptieHeader);
                    builder.Append(" ");
                }

                return builder.ToString();
            }
        }
    }
}