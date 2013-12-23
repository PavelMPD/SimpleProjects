using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailSender.CimUpdateDataResultWrappers
{
    [Serializable]
    public class CimPartUpdateDataResult
    {
        public ResultStatus Status { get; set; }
        public String Message { get; set; }
    }
}
