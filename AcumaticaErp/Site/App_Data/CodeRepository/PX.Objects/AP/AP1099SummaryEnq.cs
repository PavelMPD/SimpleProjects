using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.AP
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class AP1099SummaryEnq : PXGraph<AP1099SummaryEnq>
	{
		public PXSelect<AP1099Year> Year_Header;

        public PXCancel<AP1099Year> Cancel;
        public PXFirst<AP1099Year> First;
        public PXPrevious<AP1099Year> Prev;
        public PXNext<AP1099Year> Next;
        public PXLast<AP1099Year> Last;

		[PXFilterable]
		public PXSelectJoinGroupBy<AP1099Box, 
			LeftJoin<AP1099History, On<AP1099History.boxNbr, Equal<AP1099Box.boxNbr>, 
			And<AP1099History.finYear, Equal<Current<AP1099Year.finYear>>>>>, 
			Where<boolTrue,Equal<boolTrue>>, 
			Aggregate<GroupBy<AP1099Box.boxNbr, Sum<AP1099History.histAmt>>>> Year_Summary;

		protected virtual void AP1099Year_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
		    if (e.Row == null) return;

			bool OpenYear = ((AP1099Year)e.Row).Status == "N";
			close1099Year.SetEnabled(OpenYear);
		}

		public PXAction<AP1099Year> close1099Year;
		[PXUIField(DisplayName = Messages.CloseYear, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable Close1099Year(PXAdapter adapter)
		{
			PXCache cache = Year_Header.Cache;
			List<AP1099Year> list = new List<AP1099Year>();
			foreach (AP1099Year year in adapter.Get())
			{
				if (year.Status != "N")
				{
					throw new PXException();
				}
				year.Status = "C";
				cache.Update(year);
				list.Add((AP1099Year)year);
			}
			if (list.Count == 0)
			{
				throw new PXException();
			}
			Persist();
			if (list.Count > 0)
			{
				PXLongOperation.StartOperation(this, delegate() { AP1099SummaryEnq.CloseYearProc(list); });
			}
			return list;
		}

		public static void CloseYearProc(List<AP1099Year> list)
		{
		}

		public AP1099SummaryEnq()
		{
			APSetup setup = APSetup.Current;
			PXUIFieldAttribute.SetEnabled<AP1099Box.boxNbr>(Year_Summary.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<AP1099Box.descr>(Year_Summary.Cache, null, false);
		}

		public PXSetup<APSetup> APSetup;

        public PXAction<AP1099Year> year1099SummaryReport;
        [PXUIField(DisplayName = Messages.Year1099SummaryReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual IEnumerable Year1099SummaryReport(PXAdapter adapter)
        {
            AP1099Year filter = Year_Header.Current;
            if (filter != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                Branch cbranch = PXSelect<Branch, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>.Select(this);
                parameters["MasterBranchID"] = cbranch != null ? cbranch.BranchCD : null;
                parameters["FinYear"] = filter.FinYear;
                throw new PXReportRequiredException(parameters, "AP654000", Messages.Year1099SummaryReport);
            }
            return adapter.Get();
        }

        public PXAction<AP1099Year> year1099DetailReport;
        [PXUIField(DisplayName = Messages.Year1099DetailReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual IEnumerable Year1099DetailReport(PXAdapter adapter)
        {
            AP1099Year filter = Year_Header.Current;
            if (filter != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                Branch cbranch = PXSelect<Branch, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>.Select(this);
                parameters["MasterBranchID"] = cbranch != null ? cbranch.BranchCD : null;
                parameters["FinYear"] = filter.FinYear;
                throw new PXReportRequiredException(parameters, "AP654500", Messages.Year1099DetailReport);
            }
            return adapter.Get();
        }

	}
}
