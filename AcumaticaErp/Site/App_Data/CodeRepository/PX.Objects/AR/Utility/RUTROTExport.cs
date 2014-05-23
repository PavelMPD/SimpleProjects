using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Xml;

namespace PX.Objects.AR
{
    public class RUTExport
    {
        protected const string NsMain = "se/skatteverket/hunten/ansokan/2.0";
        protected const string NsHtko = "se/skatteverket/hunten/komponent/2.0";
        protected const string NsXsi = "http://www.w3.org/2001/XMLSchema-instance";

        protected const string PrefixHtko = "htko";

        #region Elements
        protected const string Root = "HtAnsokan";
        protected const string CompanyID = "AnsokningsNr";
        protected const string ClaimsSection = "HushallAnsokan";
        protected const string Claim = "Arenden";
        protected const string Buyer = "Kopare";

        protected const string PayDate = "BetalningsDatum";
        protected const string InvoiceAmt = "FaktureratBelopp";
        protected const string PaidAmt = "BetaltBelopp";
        protected const string ClaimedAmt = "BegartBelopp";
        protected const string InvoiceNbr = "FaktureNr";
        #endregion


        private XmlWriter Writer { get; set; }

        protected void WriteStartHtko(string element)
        {
            Writer.WriteStartElement(PrefixHtko, element, NsHtko);
        }

        protected void WriteHtko(string element, string value)
        {
            Writer.WriteElementString(PrefixHtko, element, NsHtko, value);
        }

        protected void WriteRoot()
        {
            Writer.WriteStartElement(Root, NsMain);
            Writer.WriteAttributeString("xmlns", PrefixHtko, null, NsHtko);
            Writer.WriteAttributeString("xmlns", "xsi", null, NsXsi);
        }

        protected virtual string ClaimsSectionElement { get { return ClaimsSection; } }

        protected void WriteClaimsStart(string claimId)
        {
            WriteHtko(CompanyID, claimId);
            WriteStartHtko(ClaimsSectionElement);
        }

        protected void WriteEnd()
        {
            Writer.WriteEndElement();
            Writer.WriteEndElement();
        }

        protected string AmountString(decimal value)
        {
            return value.ToString("0.######");
        }

        protected virtual void WriteSpecialPersonClaimInfo(RUTExportItem item)
        {
        }

        protected void WritePersonClaim(RUTExportItem item)
        {
            WriteStartHtko(Claim);

            WriteHtko(Buyer, item.PersonID);
            WriteHtko(PayDate, item.PayDate.ToString("yyyy-MM-dd"));
            WriteHtko(InvoiceAmt, AmountString(item.TotalAmtForWork));
            WriteHtko(PaidAmt, AmountString(item.PaidAmt));
            WriteHtko(ClaimedAmt, AmountString(item.ClaimedAmt));
            if (!String.IsNullOrWhiteSpace(item.InvoiceNbr))
            {
                WriteHtko(InvoiceNbr, item.InvoiceNbr);
            }

            WriteSpecialPersonClaimInfo(item);

            Writer.WriteEndElement();
        }

        public byte[] Export(IEnumerable<ClaimRUTROTProcess.DocumentDetails> documents)
        {
            using (var stream = new MemoryStream())
            {
                using (Writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true }))
                {
                    WriteRoot();

                    var claims = RUTExportItem.Create(documents);

                    WriteClaimsStart(documents.First().ClaimNbr.ToString());

                    foreach (var c in claims)
                    {
                        WritePersonClaim(c);
                    }

                    WriteEnd();

                    Writer.Flush();
                    return stream.ToArray();
                }
            }
        }
    }

    public class ROTExport : RUTExport
    {
        #region Elements
        protected const string ROTClaimSection = "RotAnsokan";
        protected const string PropertyID = "Fastighetsbeteckning";
        protected const string ApartmentID = "LagenhetsNr";
        protected const string OrganizationNbr = "BrfOrgNr";
        #endregion

        protected const string _OrgNbrPrefix = "16";
        protected virtual string OrgNbrPrefix { get { return _OrgNbrPrefix; } } 

        protected override string ClaimsSectionElement { get { return ROTClaimSection; } }

        protected override void WriteSpecialPersonClaimInfo(RUTExportItem item)
        {
            if (IsForProperty(item.RutRotInvoice))
            {
                WriteHtko(PropertyID, item.RutRotInvoice.ROTEstateAppartment);
            }
            else
            {
                WriteHtko(ApartmentID, item.RutRotInvoice.ROTEstateAppartment);
                WriteHtko(OrganizationNbr, OrgNbrPrefix + item.RutRotInvoice.ROTOrganizationNbr);
            }
        }

        protected bool IsForProperty(ARInvoice invoice)
        {
            return invoice.ROTOrganizationNbr == null;
        }

    }

    public class RUTExportItem
    {
        public string PersonID { get; set; }
        public decimal TotalAmtForWork { get; set; }
        public decimal PaidAmt { get; set; }
        public decimal ClaimedAmt { get; set; }
        public DateTime PayDate { get; set; }
        public string InvoiceNbr { get; set; }

        public ARInvoice RutRotInvoice { get; set; }

        public static IEnumerable<RUTExportItem> Create(ClaimRUTROTProcess.DocumentDetails document)
        {
            var items = new List<RUTExportItem>();

            var payDate = document.Payments.Select(p => p.Item2.AdjgDocDate).Where(d => d != null).Max(d => d.Value);
            var invNbr = document.Document.InvoiceNbr;
            var rrInvoice = document.Document;

            foreach (var dp in document.Distribution.Zip(document.PaymentDistribution, (d, p) => new { Distribution = d, Payment = p }))
            {
                items.Add(new RUTExportItem
                {
                    InvoiceNbr = invNbr,
                    TotalAmtForWork = dp.Payment + dp.Distribution.CuryAmount ?? 0.0m,
                    PaidAmt = dp.Payment,
                    PayDate = payDate,
                    PersonID = dp.Distribution.PersonalID,
                    ClaimedAmt = dp.Distribution.CuryAmount ?? 0.0m,
                    RutRotInvoice = rrInvoice
                });
            }

            return items;
        }

        public static IEnumerable<RUTExportItem> Create(IEnumerable<ClaimRUTROTProcess.DocumentDetails> documents)
        {
            return documents.SelectMany(d => Create(d)).ToList().OrderBy(item => item.PersonID);
        }
    }
}
