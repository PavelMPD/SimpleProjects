using System;
using System.Collections;
using PX.Data;

namespace PX.Objects.AP
{
#if flase
	[Obsolete("This graph has been replaced by APDocumentEnq and should not be used",true)]
	public class APDocumentsByVendor : PXGraph<APDocumentsByVendor>
	{
		protected APDocumentEnq enq;
		public APDocumentsByVendor()
		{
			enq = PXGraph.CreateInstance<APDocumentEnq>();
		}
		[ReportView]
		public PXSelectReadonly<Vendor>
			Vendors;
		private class DocumentsView : PXView
		{
			public DocumentsView(PXGraph graph, Delegate handler)
				: base(graph, true, new Select<APDocumentEnq.APDocumentResult>(), handler)
			{
			}
			public override System.Collections.Generic.List<object> Select(object[] currents, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
			{
				if (currents.Length > 0 && currents[0] is Vendor)
				{
					((APDocumentsByVendor)_Graph).Vendors.Current = (Vendor)currents[0];
				}
				return base.Select(currents, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
			}
		}
		public class DocumentsSelect : PXSelectBase<APDocumentEnq.APDocumentResult>
		{
			public DocumentsSelect(PXGraph graph, Delegate handler)
			{
				_Graph = graph;
				View = new DocumentsView(graph, handler);
			}
		}
		[ReportView]
		public DocumentsSelect
			Documents;
		protected virtual IEnumerable documents()
		{
			if (Vendors.Current == null)
			{
				yield break;
			}
			enq.Filter.Current.VendorID = Vendors.Current.BAccountID;
			enq.Filter.Update(enq.Filter.Current);
			foreach (APDocumentEnq.APDocumentResult res in enq.Documents.Select())
			{
				switch (res.DocType)
				{
					case "ACR":
						res.DocType = "Credit Adjustment";
						break;
					case "ADR":
						res.DocType = "Debit Adjustment";
						break;
					case "CHK":
						res.DocType = "Check";
						break;
					case "INV":
						res.DocType = "Bill";
						break;
					case "PPM":
						res.DocType = "Prepayment";
						break;
					case "REF":
						res.DocType = "Refund";
						break;
					case "VCK":
						res.DocType = "Void Check";
						break;
				}
				yield return res;
			}
		}
	}
#endif
}
