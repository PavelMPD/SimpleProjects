using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Linq;
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
        private const String CimDailyNotificationTemplate = "MailSender.Templates.CimDailyNotification.xslt";

        public String Build(CimUpdateDataResult updateDataResult)
        {
            Assembly assembly = GetType().Module.Assembly;
            if (assembly.GetManifestResourceNames().Count(r => (r.ToString(CultureInfo.InvariantCulture).Equals(CimDailyNotificationTemplate, StringComparison.Ordinal))) == 1)
            {
                XDocument updateDataResultXml = XDocument.Parse(updateDataResult.Serialize());
                using (var textWriter = new StringWriter())
                {
                    var xslt = new XslCompiledTransform();
                    xslt.Load(XmlReader.Create(assembly.GetManifestResourceStream(CimDailyNotificationTemplate)));

                    xslt.Transform(updateDataResultXml.CreateReader(), new XsltArgumentList(), textWriter);
                    return textWriter.ToString();
                }
            }
            return String.Empty;
        }
    }
}
