using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Test
{
    public static class DebugExtensions
    {
        private static object GetPropertyValue(object o, string Name)
        {
            return o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(x => x.Name == Name).First().GetValue(o, null);
        }

        public static string ToTraceString(this IQueryable query)
        {
            var oquery = (ObjectQuery)GetPropertyValue(GetPropertyValue(query, "InternalQuery"), "ObjectQuery");
            return oquery.ToTraceString();
        }
    }
}
