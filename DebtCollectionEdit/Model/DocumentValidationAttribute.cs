using System;

namespace DebtCollection.Model
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class RequiredForDocumentGeneration : Attribute { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class RegExpForDocumentGeneration : Attribute
    {
        public string Pattern { get; set; }

        public RegExpForDocumentGeneration(string pattern)
        {
            Pattern = pattern;
        }
    }
}
