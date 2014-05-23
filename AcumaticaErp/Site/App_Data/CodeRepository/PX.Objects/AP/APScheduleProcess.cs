using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.BQLConstants;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.TX;

namespace PX.Objects.AP
{
	[TableAndChartDashboardType]
	public class APScheduleRun : PXGraph<APScheduleRun>
	{
        #region Cache Attached Events
        #region Schedule
        #region ScheduleID
        [PXDBString(10, IsUnicode = true, IsKey = true)]
        [PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.Visible, Enabled = false)]
        protected virtual void Schedule_ScheduleID_CacheAttached(PXCache sender)
        {
        }        
        #endregion
        #endregion
        #endregion

		public PXFilter<ScheduleRun.Parameters> Filter;
		public PXCancel<ScheduleRun.Parameters> Cancel;
		public PXFilteredProcessingJoin<Schedule, ScheduleRun.Parameters,
			LeftJoin<APRegisterAccess, On<APRegisterAccess.scheduleID, Equal<Schedule.scheduleID>,
			And<APRegisterAccess.scheduled, Equal<boolTrue>,
			And<Not<Match<APRegisterAccess, Current<AccessInfo.userName>>>>>>>,
			Where<Schedule.active, Equal<boolTrue>,
			And<Schedule.module, Equal<BatchModule.moduleAP>,
			And<Schedule.scheduleType,NotEqual<GLScheduleType.deferredExpense>, 
			And<Schedule.nextRunDate, LessEqual<Current<ScheduleRun.Parameters.endDate>>,
			And2<Where<Current<ScheduleRun.Parameters.startDate>, IsNull, Or<Schedule.nextRunDate, GreaterEqual<Current<ScheduleRun.Parameters.startDate>>>>,
			And<APRegisterAccess.docType, IsNull>>>>>>>
			Schedule_List;

		public PXSetup<APSetup> APSetup;

		public PXAction<ScheduleRun.Parameters> ViewSchedule;
		[PXUIField(DisplayName = "View Schedule", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable viewSchedule(PXAdapter adapter)
		{
			if (Schedule_List.Current != null)
			{
				PXRedirectHelper.TryRedirect(Schedule_List.Cache, Schedule_List.Current, "Schedule", PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXAction<ScheduleRun.Parameters> NewSchedule;
		[PXUIField(DisplayName = "New Schedule", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable newSchedule(PXAdapter adapter)
		{
			APScheduleMaint graph = CreateInstance<APScheduleMaint>();
			graph.Schedule_Header.Insert(new Schedule());
			graph.Schedule_Header.Cache.IsDirty = false;
			throw new PXRedirectRequiredException(graph, true, "New Schedule") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		public APScheduleRun()
		{
			APSetup setup = APSetup.Current;
			Schedule_List.SetProcessCaption("Run Selected");
			Schedule_List.SetProcessAllCaption("Run All");
		}

		protected virtual void Parameters_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;

            ScheduleRun.SetProcessDelegate<APScheduleProcess>(this, (ScheduleRun.Parameters)e.Row, Schedule_List);
		}

		protected virtual void Parameters_StartDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ScheduleRun.Parameters filterData = (ScheduleRun.Parameters)e.Row;
			if (filterData.EndDate != null && filterData.StartDate > filterData.EndDate)
			{
				filterData.EndDate = filterData.StartDate;
			}
		}

		protected virtual void Schedule_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			Schedule task = (Schedule)e.Row;
			if (task == null || PXLongOperation.Exists(UID)) return;

			if ((task.NoRunLimit ?? false) == false && task.RunCntr == task.RunLimit)
			{
				cache.RaiseExceptionHandling<Schedule.scheduleID>(e.Row, task.ScheduleID, new PXSetPropertyException(Messages.SheduleExecutionLimitExceeded, PXErrorLevel.RowError));
			}
			else if ((task.NoEndDate ?? false) == false && task.EndDate < Accessinfo.BusinessDate)
			{
				cache.RaiseExceptionHandling<Schedule.scheduleID>(e.Row, task.ScheduleID, new PXSetPropertyException(Messages.SheduleHasExpired, PXErrorLevel.RowError));
			}
			else if (task.NextRunDate > Accessinfo.BusinessDate)
			{
				cache.RaiseExceptionHandling<Schedule.scheduleID>(e.Row, task.ScheduleID, new PXSetPropertyException(Messages.SheduleNextExecutionDateExceeded, PXErrorLevel.RowWarning));
			}
		}
	}

	public class APScheduleProcess : PXGraph<APScheduleProcess>, IScheduleProcessing
	{
		public PXSelect<Schedule> Running_Schedule;
		public PXSelect<APTran> Tran_Created;
		public PXSelect<CurrencyInfo> CuryInfo_Created;

		public GLSetup GLSetup
		{
			get
			{
				return PXSelect<GLSetup>.Select(this);
			}
		}

		public APScheduleProcess()
		{
			//AutoNumberAttribute.SetAllowOverride<BatchNew.batchNbr>(Batch_Created.Cache, null, true);
		}
        public virtual void GenerateProc(Schedule s)
        {
            GenerateProc(s, 1, Accessinfo.BusinessDate.Value);
        }

        public virtual void GenerateProc(Schedule s, short Times, DateTime runDate)
		{
			List<ScheduleDet> sd = GL.ScheduleProcess.MakeSchedule(this, s, Times, runDate);

			APInvoiceEntry docgraph = CreateGraph();

			foreach(ScheduleDet sdet in sd)
			{
				foreach (PXResult<APInvoice, Vendor, CurrencyInfo> res in PXSelectJoin<APInvoice, InnerJoin<Vendor, On<Vendor.bAccountID,Equal<APInvoice.vendorID>>, InnerJoin<CurrencyInfo,On<CurrencyInfo.curyInfoID, Equal<APInvoice.curyInfoID>>>>, Where<APInvoice.scheduleID, Equal<Required<APInvoice.scheduleID>>, And<APInvoice.scheduled, Equal<boolTrue>>>>.Select(this, s.ScheduleID))
				{
					docgraph.Clear();
					docgraph.vendor.Current = (Vendor)res;
					APInvoice apdoc = (APInvoice)res;
					CurrencyInfo info = (CurrencyInfo)res;

					CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(info);
					new_info.CuryInfoID = null;
					new_info = docgraph.currencyinfo.Insert(new_info);

					APInvoice new_apdoc = PXCache<APInvoice>.CreateCopy(apdoc);
					new_apdoc.CuryInfoID = new_info.CuryInfoID;
					new_apdoc.DocDate = sdet.ScheduledDate;
					new_apdoc.FinPeriodID = sdet.ScheduledPeriod;
					new_apdoc.TranPeriodID = null;
					new_apdoc.DueDate = null;
					new_apdoc.DiscDate = null;
					new_apdoc.PayDate = null;
					new_apdoc.CuryOrigDiscAmt = null;
					new_apdoc.OrigDiscAmt = null;
					new_apdoc.RefNbr = null;
					new_apdoc.Scheduled = false;
					new_apdoc.CuryLineTotal = 0m;
					new_apdoc.CuryVatTaxableTotal = 0m;
					new_apdoc.CuryVatExemptTotal = 0m;
					new_apdoc.NoteID = null;
					new_apdoc = docgraph.Document.Insert(new_apdoc);

					TaxAttribute.SetTaxCalc<APTran.taxCategoryID, APTaxAttribute>(docgraph.Transactions.Cache, null, TaxCalc.ManualCalc);
					PXNoteAttribute.SetNote(docgraph.Document.Cache, new_apdoc, PXNoteAttribute.GetNote(Caches[typeof(APInvoice)], apdoc));
					PXNoteAttribute.SetFileNotes(docgraph.Document.Cache, new_apdoc, PXNoteAttribute.GetFileNotes(Caches[typeof(APInvoice)], apdoc));

					foreach (APTran aptran in PXSelect<APTran, Where<APTran.tranType, Equal<Required<APTran.tranType>>, And<APTran.refNbr, Equal<Required<APTran.refNbr>>>>>.Select(docgraph, apdoc.DocType, apdoc.RefNbr)) 
					{
						APTran new_aptran = PXCache<APTran>.CreateCopy(aptran);
						new_aptran.RefNbr = null;
					    new_aptran.CuryInfoID = null;
						docgraph.Transactions.Insert(new_aptran);
					}

					foreach (APTaxTran tax in PXSelect<APTaxTran, Where<APTaxTran.tranType, Equal<Required<APTaxTran.tranType>>, And<APTaxTran.refNbr, Equal<Required<APTaxTran.refNbr>>>>>.Select(docgraph, apdoc.DocType, apdoc.RefNbr))
					{
						APTaxTran new_aptax = new APTaxTran();
						new_aptax.TaxID = tax.TaxID;

						new_aptax = docgraph.Taxes.Insert(new_aptax);

						if (new_aptax != null)
						{
							new_aptax = PXCache<APTaxTran>.CreateCopy(new_aptax);
							new_aptax.TaxRate = tax.TaxRate;
							new_aptax.CuryTaxableAmt = tax.CuryTaxableAmt;
							new_aptax.CuryTaxAmt = tax.CuryTaxAmt;
							new_aptax = docgraph.Taxes.Update(new_aptax);
						}
					}
					docgraph.Save.Press();
				}
				s.LastRunDate = sdet.ScheduledDate;
				Running_Schedule.Cache.Update(s);
			}

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				Running_Schedule.Cache.Persist(PXDBOperation.Update);
				ts.Complete(this);
			}
			Running_Schedule.Cache.Persisted(false);
		}

		public virtual APInvoiceEntry CreateGraph()
		{
			return PXGraph.CreateInstance<APInvoiceEntry>();
		}
	}
}


