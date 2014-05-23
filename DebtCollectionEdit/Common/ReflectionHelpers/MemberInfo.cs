using System;

namespace DebtCollection.Common.ReflectionHelpers
{
    public class MemberInfo
    {
        public string MemberName { get; set; }
        public bool Nullable { get; set; }
        public Type Type { get; set; }
    }
}