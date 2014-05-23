using System;

namespace DebtCollection.Model.Attributes
{
    public class Auditable : Attribute
    {
        private readonly string _logFieldName;

        public Auditable()
        {
            _logFieldName = string.Empty;
        }

        public Auditable(string logFieldName)
        {
            _logFieldName = logFieldName;
        }

        public string LogFieldName { get { return _logFieldName; } }
    }
}
