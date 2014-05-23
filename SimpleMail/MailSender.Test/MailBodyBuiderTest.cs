using Common;
using NUnit.Framework;
using System.Diagnostics;
using MailSender.CimUpdateDataResultWrappers;
using DebtCollection.Model.Enums;

namespace MailSender.Test.CimUpdateDataResultWrappers
{
    [TestFixture]
    public class MailBodyBuiderTest
    {
        [Test]
        public void BuildTest()
        {
            IMailBodyBuider mailBodyBuider = new MailBodyBuider();
            var updateResult = GetCimUpdateDataResult();
            //Trace.WriteLine(updateResult.Serialize());
            Trace.WriteLine(mailBodyBuider.Build(updateResult));
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
