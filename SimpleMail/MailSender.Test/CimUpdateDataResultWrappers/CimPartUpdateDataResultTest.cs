using Common;
using NUnit.Framework;
using System.Diagnostics;
using MailSender.CimUpdateDataResultWrappers;
using DebtCollection.Model.Enums;

namespace MailSender.Test.CimUpdateDataResultWrappers
{
    [TestFixture]
    public class CimPartUpdateDataResultTest
    {
        [Test]
        public void SerializeToXml()
        {
            var updateResult = GetCimUpdateDataResult();
            Trace.WriteLine(updateResult.Serialize());
        }

        private CimUpdateDataResult GetCimUpdateDataResult()
        {
            return new CimUpdateDataResult()
            {
                DebtorsUpdateResult = new CimPartUpdateDataResult()
                {
                    Status = ProcessingStatus.Successful,
                    Message = "успешно"
                },
                CallsUpdateResult = new CimPartUpdateDataResult()
                {
                    Status = ProcessingStatus.Warning,
                    Message = "частично"
                },
                PaymentsUpdateResult = new CimPartUpdateDataResult()
                {
                    Status = ProcessingStatus.Fail,
                    Message = "не успешно"
                },
            };
        }
    }
}
