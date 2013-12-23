using Common;
using NUnit.Framework;
using System.Diagnostics;
using MailSender.CimUpdateDataResultWrappers;

namespace MailSender.Test.CimUpdateDataResultWrappers
{
    [TestFixture]
    public class CimPartUpdateDataResultTest
    {
        [Test]
        public void SerializeToXml()
        {
            var partResult = new CimUpdateDataResult()
            {
                DebtorsUpdateResult = new CimPartUpdateDataResult()
                {
                    Status = ResultStatus.Success,
                    Message = "Успешно"
                },
                CallsUpdateResult = new CimPartUpdateDataResult(),
                PaymentsUpdateResult = new CimPartUpdateDataResult()
            };
            Trace.WriteLine(partResult.Serialize());
        }
    }
}
