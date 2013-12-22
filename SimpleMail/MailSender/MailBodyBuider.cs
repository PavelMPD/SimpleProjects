using System;
using Common;
using MailSender.CimUpdateDataResultWrappers;

namespace MailSender
{
    public interface IMailBodyBuider
    {
        String Build(CimUpdateDataResult updateDataResult);
    }

    public class MailBodyBuider : IMailBodyBuider
    {
        public String Build(CimUpdateDataResult updateDataResult)
        {
            String updateDataResultXml = updateDataResult.Serialize();

        }
    }
}
