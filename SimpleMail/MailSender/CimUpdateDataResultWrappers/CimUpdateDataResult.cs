using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailSender.CimUpdateDataResultWrappers
{
    [Serializable]
    public class CimUpdateDataResult
    {
        public CimPartUpdateDataResult DebtorsUpdateResult { get; set; }
        public CimPartUpdateDataResult CallsUpdateResult { get; set; }
        public CimPartUpdateDataResult PaymentsUpdateResult { get; set; }
    }
}
