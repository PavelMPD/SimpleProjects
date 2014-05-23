using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.GL.Overrides.CloseGraph;
using PX.Objects.GL;
using PX.Objects.PO;
using System.Diagnostics;



namespace PX.Objects.AP
{
	[TableAndChartDashboardType]
	public class APClosing : Closing
	{
		protected override IEnumerable periodList()
		{
			string fiscalYear = null;
			APSetup.Current = APSetup.Select();
			if (APSetup.Current == null)
				yield break;
			foreach (FinPeriod per in PXSelect<FinPeriod, Where<FinPeriod.aPClosed, Equal<boolFalse>>>.Select(this))
			{
				if (fiscalYear == null)
				{
					fiscalYear = per.FinYear;
				}
				if (per.FinYear == fiscalYear)
				{
					yield return per;
				}
			}
		}

		protected override void StartClosePeriod(List<FinPeriod> list)
		{
			PXLongOperation.StartOperation(this, delegate() { ClosePeriod(list); });
		}

		public static new void ClosePeriod(List<FinPeriod> list)
		{
			APClosing pg = PXGraph.CreateInstance<APClosing>();
			for (int i = 0; i < list.Count; i++)
			{
				pg.Clear();
				FinPeriod per = list[i];
				pg.ClosePeriodProc(per);
			}
		}
		protected override void RunShowReport(Dictionary<string, string> d )
		{
			string ReportID = "AP656000"; 
			if (d.Count > 0)
			{
				d[ReportMessages.CheckReportFlag] = ReportMessages.CheckReportFlagValue;
				throw new PXReportRequiredException(d, ReportID, PXBaseRedirectException.WindowMode.New, "Open AP Documents");
			}
		}
		protected override void ClosePeriodProc(FinPeriod p)
        {
            APRegister prebookedDoc = PXSelect<APRegister, Where<APRegister.voided, Equal<boolFalse>,
                                            And<APRegister.prebooked, Equal<boolTrue>,
                                            And<APRegister.released, Equal<boolFalse>,
                                            And<APRegister.finPeriodID, Equal<Required<APRegister.finPeriodID>>>>>>>.Select(this, p.FinPeriodID);
            if (prebookedDoc != null)
            {
                throw new PXException(Messages.PeriodHasPrebookedDocs);
            }

			PXSelectBase select = new PXSelectJoin<APRegister,
											LeftJoin<APAdjust, On<APAdjust.adjgDocType, Equal<APRegister.docType>, And<APAdjust.adjgRefNbr, Equal<APRegister.refNbr>, And<APAdjust.released, Equal<False>>>>>,
											Where<APRegister.voided, Equal<boolFalse>,
											And<APRegister.scheduled, Equal<boolFalse>,
											And<Where<APAdjust.adjgFinPeriodID, IsNull, And<APRegister.released, Equal<False>, And<APRegister.finPeriodID, Equal<Required<APRegister.finPeriodID>>, Or<APAdjust.adjgFinPeriodID, Equal<Required<APAdjust.adjgFinPeriodID>>>>>>>>>>(this); 
			object doc = select.View.SelectSingle(p.FinPeriodID, p.FinPeriodID);
			if (doc != null)
			{
				throw new PXException(Messages.PeriodHasUnreleasedDocs);
			}

			PO.LandedCostTran lcTran = PXSelectJoin<LandedCostTran,
											InnerJoin<POReceipt, On<LandedCostTran.pOReceiptNbr, Equal<POReceipt.receiptNbr>>>,
											Where<LandedCostTran.source, Equal<LandedCostTranSource.fromPO>,
											And<POReceipt.released, Equal<True>,
											And<LandedCostTran.postponeAP,Equal<False>,
											And<LandedCostTran.processed, Equal<False>,
												And<LandedCostTran.invoiceDate, GreaterEqual<Required<LandedCostTran.invoiceDate>>,				
												And<LandedCostTran.invoiceDate, Less<Required<LandedCostTran.invoiceDate>>>>>>>>>.Select(this, p.StartDate, p.EndDate);
			if (lcTran != null && lcTran.LCTranID.HasValue) 
			{
				throw new PXException(Messages.PeriodHasAPDocsFromPO_LCToBeCreated);
			}
			
			p.APClosed = true;
			Caches[typeof(FinPeriod)].Update(p);

			Actions.PressSave();
		}
	}
}

