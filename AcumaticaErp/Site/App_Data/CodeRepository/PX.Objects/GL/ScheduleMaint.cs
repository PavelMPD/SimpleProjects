using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.BQLConstants;
using PX.Objects.CA;
using PX.Objects.GL.Overrides.ScheduleMaint;
using PX.Objects.CS;

namespace PX.Objects.GL
{
	public class ScheduleMaint : PXGraph<ScheduleMaint, Schedule>
	{
		public PXAction<Schedule> RunNow;
		public PXSelect<Schedule, Where<Schedule.module, Equal<BatchModule.moduleGL>>> Schedule_Header;
		public PXSelect<Batch, Where<Batch.scheduleID, Equal<Current<Schedule.scheduleID>>, And<Batch.scheduled, Equal<boolFalse>>>> Batch_History;
		public PXSelect<BatchSelection, Where<BatchSelection.scheduleID, Equal<Current<Schedule.scheduleID>>, And<BatchSelection.scheduled, Equal<boolTrue>>>> Batch_Detail;
		public PXSelect<GLTran> GLTransactions;
		public PXSelect<CATran> CATransactions;

		public ScheduleMaint()
		{
			GLSetup gls = GLSetup.Current;
			Batch_History.Cache.AllowDelete = false;
			Batch_History.Cache.AllowInsert = false;
			Batch_History.Cache.AllowUpdate = false;
            CopyPaste.SetVisible(false);

			Views.Caches.Remove(typeof (Batch));
		}

        public PXAction<Schedule> viewBatchD;
        [PXUIField(DisplayName = Messages.ViewBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ViewBatchD(PXAdapter adapter)
        {
            Batch row = Batch_Detail.Current;
            if (row != null)
            {
                JournalEntry graph = CreateInstance<JournalEntry>();
                graph.BatchModule.Current = row;
                throw new PXRedirectRequiredException(graph, true, "View Batch") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            return adapter.Get();
        }


        public PXAction<Schedule> viewBatch;
        [PXUIField(DisplayName = Messages.ViewBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ViewBatch(PXAdapter adapter)
        {
            Batch row = Batch_History.Current;
            if (row != null)
            {
                JournalEntry graph = CreateInstance<JournalEntry>();
                graph.BatchModule.Current = row;
                throw new PXRedirectRequiredException(graph, true, "View Batch") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            return adapter.Get();
        }

        public PXSetup<GLSetup> GLSetup;

        [PXUIField(DisplayName = "Run Now", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXProcessButton]
        public virtual IEnumerable runNow(PXAdapter adapter)
        {
            this.Save.Press();

            Schedule schedule = Schedule_Header.Current;
			if (schedule.NextRunDate > Accessinfo.BusinessDate)
			{
				throw new PXException(Messages.SheduleNextExecutionDateExceeded);
			}
			else
			{
				if (schedule.NoRunLimit == false && schedule.RunCntr >= schedule.RunLimit)
				{
					throw new PXException(Messages.SheduleExecutionLimitExceeded);
				}
				else
				{
					if (schedule.NoEndDate == false && schedule.EndDate < Accessinfo.BusinessDate)
					{
						throw new PXException(Messages.SheduleHasExpired);
					}
					else
					{
						PXLongOperation.StartOperation(this,
							delegate()
							{
								ScheduleProcess sp = PXGraph.CreateInstance<ScheduleProcess>();
								sp.GenerateProc(schedule);
							}
						);
					}
				}
			}

			return adapter.Get();
        }

		protected virtual void Schedule_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;

            SetControlsState(cache, (Schedule)e.Row);
		}

        protected virtual void Schedule_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
			if (e.Row != null)
			{
				cache.SetDefaultExt<Schedule.weeks>(e.Row);
				cache.SetDefaultExt<Schedule.days>(e.Row);
				cache.SetDefaultExt<Schedule.periods>(e.Row);
				cache.SetDefaultExt<Schedule.months>(e.Row);
			}
        }

		protected virtual void Schedule_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
            Schedule schedule = (Schedule)e.Row;
            bool nextRunDate = true;
            if (schedule.RunLimit <= 0 && !(bool)schedule.NoRunLimit)
            {
                sender.RaiseExceptionHandling<Schedule.runLimit>(schedule, schedule.RunLimit, new PXSetPropertyException(CS.Messages.Entry_GT, "0"));
                nextRunDate = false;
            }
            if (schedule.ScheduleType == "D" && schedule.DailyFrequency <= 0)
            {
                sender.RaiseExceptionHandling<Schedule.dailyFrequency>(schedule, schedule.DailyFrequency, new PXSetPropertyException(CS.Messages.Entry_GT, "0"));
                nextRunDate = false;
            }
            if (schedule.ScheduleType == "W" && schedule.WeeklyFrequency <= 0)
            {
                sender.RaiseExceptionHandling<Schedule.weeklyFrequency>(schedule, schedule.WeeklyFrequency, new PXSetPropertyException(CS.Messages.Entry_GT, "0"));
                nextRunDate = false;
            }
            if (schedule.ScheduleType == "P" && schedule.PeriodFrequency <= 0)
            {
                sender.RaiseExceptionHandling<Schedule.periodFrequency>(schedule, schedule.PeriodFrequency, new PXSetPropertyException(CS.Messages.Entry_GT, "0"));
                nextRunDate = false;
            }
            if (schedule.ScheduleType == "P" && schedule.PeriodDateSel == "D" && schedule.PeriodFixedDay <= 0)
            {
                sender.RaiseExceptionHandling<Schedule.periodFixedDay>(schedule, schedule.PeriodFixedDay, new PXSetPropertyException(CS.Messages.Entry_GT, "0"));
                nextRunDate = false;
            }
            if (schedule.EndDate == null && schedule.NoEndDate == false)
            {
                sender.RaiseExceptionHandling<Schedule.endDate>(schedule, null, new PXSetPropertyException(CR.Messages.EmptyValueErrorFormat, typeof(Schedule.endDate).Name));
                nextRunDate = false;
            }
            if (!(bool)schedule.WeeklyOnDay1 && !(bool)schedule.WeeklyOnDay2 && !(bool)schedule.WeeklyOnDay3 && !(bool)schedule.WeeklyOnDay4 && !(bool)schedule.WeeklyOnDay5 && !(bool)schedule.WeeklyOnDay6 && !(bool)schedule.WeeklyOnDay7)
            {
                sender.RaiseExceptionHandling<Schedule.weeklyOnDay1>(schedule, null, new PXSetPropertyException(Messages.DayOfWeekNotSelected));
                nextRunDate = false;
            }

            if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete && nextRunDate)
            {
                ((Schedule)e.Row).NextRunDate = GL.ScheduleProcess.GetNextRunDate(this, (Schedule)e.Row);
            }
        }

		protected virtual void BatchSelection_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            if (e.Row == null || string.IsNullOrWhiteSpace(((BatchSelection)e.Row).BatchNbr)) return;
            PXUIFieldAttribute.SetEnabled<BatchSelection.module>(sender, e.Row, !(bool)((BatchSelection)e.Row).Scheduled);
			PXUIFieldAttribute.SetEnabled<BatchSelection.batchNbr>(sender, e.Row, !(bool)((BatchSelection)e.Row).Scheduled);
		}

		private void SetControlsState(PXCache cache, Schedule s)
		{
			Boolean isMonthly = (s.ScheduleType == "M");
			Boolean isPeriodically = (s.ScheduleType == "P");
			Boolean isWeekly = (s.ScheduleType == "W");
			Boolean isDaily = (s.ScheduleType == "D");
			Boolean isNotProcessed = (s.LastRunDate == null);
            bool isActive = (s.Active ?? false);

            if (isActive)
            {
                RunNow.SetEnabled(true);
            }
            else
            {
                RunNow.SetEnabled(false);
            }

            PXUIFieldAttribute.SetVisible<Schedule.dailyFrequency>(cache, s, isDaily);
            PXUIFieldAttribute.SetVisible<Schedule.days>(cache, s, isDaily);
            SetMonthlyControlsState(cache,s);

            PXUIFieldAttribute.SetVisible<Schedule.weeklyFrequency>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay1>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay2>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay3>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay4>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay5>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay6>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay7>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeks>(cache, s, isWeekly);

            PXUIFieldAttribute.SetVisible<Schedule.monthlyFrequency>(cache, s, isMonthly);
            PXUIFieldAttribute.SetVisible<Schedule.monthlyDaySel>(cache, s, isMonthly);

            PXUIFieldAttribute.SetVisible<Schedule.periodFrequency>(cache, s, isPeriodically);
            PXUIFieldAttribute.SetVisible<Schedule.periodDateSel>(cache, s, isPeriodically);
            PXUIFieldAttribute.SetVisible<Schedule.periodFixedDay>(cache, s, isPeriodically);
            PXUIFieldAttribute.SetVisible<Schedule.periods>(cache, s, isPeriodically);
            SetPeriodicallyControlsState(cache,s);

            PXUIFieldAttribute.SetEnabled<Schedule.endDate>(cache, s, !(bool)s.NoEndDate);
            PXUIFieldAttribute.SetEnabled<Schedule.runLimit>(cache, s, !(bool)s.NoRunLimit);
            PXDefaultAttribute.SetPersistingCheck<Schedule.endDate>(cache, s, (s.NoEndDate == true ? PXPersistingCheck.Nothing : PXPersistingCheck.Null));

            PXUIFieldAttribute.SetEnabled<BatchSelection.module>(Batch_Detail.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<BatchSelection.batchNbr>(Batch_Detail.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<BatchSelection.ledgerID>(Batch_Detail.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<BatchSelection.dateEntered>(Batch_Detail.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<BatchSelection.finPeriodID>(Batch_Detail.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<BatchSelection.curyControlTotal>(Batch_Detail.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<BatchSelection.curyID>(Batch_Detail.Cache, null, false);

			cache.AllowDelete = isNotProcessed;
		}

		private void SetMonthlyControlsState(PXCache cache, Schedule s)
		{
			Boolean isMonthly = (s.ScheduleType == "M");
			PXUIFieldAttribute.SetEnabled<Schedule.monthlyOnDay>(cache, s, isMonthly && s.MonthlyDaySel == "D");
			PXUIFieldAttribute.SetEnabled<Schedule.monthlyOnWeek>(cache, s, isMonthly && s.MonthlyDaySel == "W");
			PXUIFieldAttribute.SetEnabled<Schedule.monthlyOnDayOfWeek>(cache, s, isMonthly && s.MonthlyDaySel == "W");
		}

		private void SetPeriodicallyControlsState(PXCache cache, Schedule s)
		{
			Boolean isPeriodically = (s.ScheduleType == "P");
			PXUIFieldAttribute.SetEnabled<Schedule.periodFixedDay>(cache, s, isPeriodically && s.PeriodDateSel == "D");
		}

		protected virtual void Schedule_NoEndDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			if (!(bool)e.OldValue && (bool) ((Schedule)e.Row).NoEndDate != (bool)e.OldValue)
			{
				Schedule s = (Schedule) e.Row;
				s.EndDate = null;
			}
		}

		protected virtual void Schedule_NoRunLimit_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			if ((bool)((Schedule)e.Row).NoRunLimit != (bool)e.OldValue)
			{
				Schedule s = (Schedule)e.Row;
				if((bool)e.OldValue)
					s.RunLimit = 1;
				else
					s.RunLimit = 0;
			}
		}

		protected virtual void Schedule_ScheduleType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Schedule s = (Schedule)e.Row;
			if (s.NextRunDate != null && !object.Equals(e.OldValue, s.ScheduleType))
			{
				throw new PXException(Messages.ScheduleTypeCantBeChanged);
			}
		}

		protected virtual void BatchSelection_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			Batch b = e.Row as Batch;
			if (b != null && b.Voided == false)
			{
				b.Scheduled = true;
				b.ScheduleID = Schedule_Header.Current.ScheduleID;
			}
		}

		protected virtual void BatchSelection_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			BatchSelection batch = (BatchSelection)e.Row;
			if (batch != null && !String.IsNullOrWhiteSpace(batch.Module) && !String.IsNullOrWhiteSpace(batch.BatchNbr))
			{
                batch = PXSelectReadonly<BatchSelection,
                    Where<BatchSelection.module, Equal<Required<BatchSelection.module>>,
                        And<BatchSelection.batchNbr, Equal<Required<BatchSelection.batchNbr>>>>>
                    .Select(this, batch.Module, batch.BatchNbr);

                PXSelectorAttribute selectorAttr = (PXSelectorAttribute)sender.GetAttributesReadonly<BatchSelection.batchNbr>(batch).Find(
                    (PXEventSubscriberAttribute attr) => { return attr is PXSelectorAttribute; });

                BqlCommand selectorSearch = selectorAttr.GetSelect();
 
                if (batch != null && selectorSearch.Meet(sender, batch))
                {
                    Batch_Detail.Delete(batch);
                    Batch_Detail.Update(batch);
                }
                else
                {
                    batch = (BatchSelection)e.Row;
                    sender.RaiseExceptionHandling<BatchSelection.batchNbr>(batch, batch.BatchNbr, new PXSetPropertyException(Messages.BatchNbrNotValid));
                    Batch_Detail.Delete(batch);
                }

			}
		}

		protected virtual void Schedule_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			foreach (BatchSelection b in PXSelect<BatchSelection, Where<BatchSelection.scheduleID, Equal<Required<Schedule.scheduleID>>>>.Select(this, ((Schedule)e.Row).ScheduleID))
			{
				if (b.Scheduled == true)
				{
					b.Voided = true;
					b.Scheduled = false;
				}
				b.ScheduleID = null;

				if (Batch_Detail.Cache.GetStatus(b) == PXEntryStatus.Notchanged)
				{
					Batch_Detail.Cache.SetStatus(b, PXEntryStatus.Updated);
				}
				PXDBDefaultAttribute.SetDefaultForUpdate<BatchSelection.scheduleID>(Batch_Detail.Cache, b, false);
			}
		}

		public override void Persist()
		{
            foreach (BatchSelection b in Batch_Detail.Cache.Updated)
			{
				if (b.Voided == false)
				{
					foreach (CATran catran in PXSelect<CATran, Where<CATran.origModule, Equal<Current<BatchSelection.module>>,
															And<CATran.origRefNbr, Equal<Current<BatchSelection.batchNbr>>,
															And<CATran.origTranType, Equal<CAAPARTranType.gLEntry>>>>>.SelectMultiBound(this, new object[] { b }))
					{
						CATransactions.Delete(catran);
					}

					foreach (GLTran gltran in PXSelect<GLTran, Where<GLTran.module, Equal<Current<BatchSelection.module>>,
																And<GLTran.batchNbr, Equal<Current<BatchSelection.batchNbr>>>>>.SelectMultiBound(this, new object[] { b }))
					{
						gltran.CATranID = null;
						gltran.LedgerBalanceType = "N";
						GLTransactions.Update(gltran);
					}

					b.Scheduled = true;
					b.ScheduleID = Schedule_Header.Current.ScheduleID;
					Batch_Detail.Cache.Update(b);
				}
			}

			foreach (BatchSelection b in Batch_Detail.Cache.Deleted)
			{
				PXDBDefaultAttribute.SetDefaultForUpdate<BatchSelection.scheduleID>(Batch_Detail.Cache, b, false);
				b.Scheduled = false;
				b.Voided = true;
				b.ScheduleID = null;
				Batch_Detail.Cache.SetStatus(b, PXEntryStatus.Updated);
				Batch_Detail.Cache.Update(b);
			}

            base.Persist();
		}
	}
}

namespace PX.Objects.GL.Overrides.ScheduleMaint
{
	[PXPrimaryGraph(null)]
    [PXCacheName(Messages.BatchSelection)]
    [Serializable]
	public partial class BatchSelection : Batch
	{
		#region Module
		public new abstract class module : PX.Data.IBqlField
		{
		}
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault(BatchModule.GL)]
		[PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.SelectorVisible)]
		[BatchModule.GLOnlyList()]		
		public override String Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;
			}
		}
		#endregion		
		#region BatchNbr
		public new abstract class batchNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.released, Equal<boolFalse>, And<Batch.hold, Equal<boolFalse>, And<Batch.voided, Equal<boolFalse>, And<Batch.module, Equal<moduleGL>>>>>>))]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.SelectorVisible)]
		public override String BatchNbr
		{
			get
			{
				return this._BatchNbr;
			}
			set
			{
				this._BatchNbr = value;
			}
		}
		#endregion
		#region ScheduleID
		public new abstract class scheduleID : IBqlField
		{
		}
		[PXDBString(10, IsUnicode = true)]
		[PXDBDefault(typeof(Schedule.scheduleID))]
		[PXParent(typeof(Select<Schedule, Where<Schedule.scheduleID, Equal<Current<Batch.scheduleID>>>>), LeaveChildren=true)]
		public override string ScheduleID
		{
			get
			{
				return this._ScheduleID;
			}
			set
			{
				this._ScheduleID = value;
			}
		}
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		[PXDBLong()]
		public override Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		[FinPeriodID()]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.Visible)]
		public override String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region TranPeriodID
		public new abstract class tranPeriodID : IBqlField
		{
		}
		#endregion
		#region Released
		public new abstract class released : PX.Data.IBqlField
		{
		}
		#endregion
		#region Hold
		public new abstract class hold : PX.Data.IBqlField
		{
		}
		#endregion
		#region Scheduled
		public new abstract class scheduled : PX.Data.IBqlField
		{
		}
		#endregion
		#region Voided
		public new abstract class voided : PX.Data.IBqlField
		{
		}
		#endregion
		#region CreatedByID
		public new abstract class createdByID : PX.Data.IBqlField
		{
		}
		#endregion
		#region LastModifiedByID
		public new abstract class lastModifiedByID : PX.Data.IBqlField
		{
		}
		#endregion
	}

}
