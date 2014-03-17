using System;
using DebtCollection.Common;
using DebtCollection.Model.Enums;

namespace MailSender.CimUpdateDataResultWrappers
{
    [Serializable]
    public class CimPartUpdateDataResult
    {
        public ProcessingStatus Status { get; set; }

        public String StatusText
        {
            get { return EnumHelper.GetDisplayValue(Status); }
            set { throw new NotSupportedException("Setting the StatusText property is not supported"); }
        }

        public String StatusHtmlStyle
        {
            get
            {
                return GetHtmlStyle(Status);
            }
            set { throw new NotSupportedException("Setting the StatusHtmlStyle property is not supported"); }
        }

        public String Message { get; set; }

        private String GetHtmlStyle(ProcessingStatus status)
        {
            switch (status)
            {
                case ProcessingStatus.Successful:
                    return "color:green";
                case ProcessingStatus.Warning:
                    return "color:#E36C0A";
                case ProcessingStatus.Fail:
                    return "color:red";
            }
            return String.Empty;
        }
    }
}
