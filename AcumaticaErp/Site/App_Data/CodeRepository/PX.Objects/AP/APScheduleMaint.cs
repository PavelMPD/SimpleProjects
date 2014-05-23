using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.BQLConstants;
using PX.Objects.AP.Overrides.ScheduleMaint;
using PX.Objects.GL;
using PX.Objects.CS;

namespace PX.Objects.AP
{
	public class APScheduleMaint : PXGraph<APScheduleMaint, Schedule>
	{
        #region Cache Attached Events
        #region Schedule
        #region ScheduleID

        [PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 0)]
        [AutoNumber(typeof(GLSetup.scheduleNumberingID), typeof(AccessInfo.businessDate))]
        [PXSelector(typeof(Search2<Schedule.scheduleID,
            LeftJoin<APRegisterAccess, On<APRegisterAccess.scheduleID, Equal<Schedule.scheduleID>,
            And<APRegisterAccess.scheduled, Equal<boolTrue>,
            And<Not<Match<APRegisterAccess, Current<AccessInfo.userName>>>>>>>,
            Where<Schedule.module, Equal<BatchModule.moduleAP>,
            And<APRegisterAccess.docType, IsNull>>>))]
        [PXDefault]
        protected virtual void Schedule_ScheduleID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion
        #endregion

		public PXAction<Schedule> RunNow;
		public PXSelect<Vendor> Vendors;
		public PXSelectJoin<Schedule,
			LeftJoin<APRegisterAccess, On<APRegisterAccess.scheduleID, Equal<Schedule.scheduleID>,
			And<APRegisterAccess.scheduled, Equal<boolTrue>,
			And<Not<Match<APRegisterAccess, Current<AccessInfo.userName>>>>>>>,
			Where<Schedule.module, Equal<BatchModule.moduleAP>,
			And<APRegisterAccess.docType, IsNull>>> Schedule_Header;
		public PXSelect<APRegister, Where<APRegister.scheduleID, Equal<Current<Schedule.scheduleID>>, And<APRegister.scheduled, Equal<boolFalse>>>> Document_History;
		public PXSelect<DocumentSelection, Where<DocumentSelection.scheduleID, Equal<Current<Schedule.scheduleID>>, And<DocumentSelection.scheduled, Equal<boolTrue>>>> Document_Detail;

		public APScheduleMaint()
		{
			APSetup aps = APSetup.Current;
			GLSetup gls = GLSetup.Current;
			Document_History.Cache.AllowDelete = false;
			Document_History.Cache.AllowInsert = false;
			Document_History.Cache.AllowUpdate = false;
            CopyPaste.SetVisible(false);
		}

		public PXSetup<APSetup> APSetup;
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
								APScheduleProcess sp = PXGraph.CreateInstance<APScheduleProcess>();
								sp.GenerateProc(schedule);
							}
						);
					}
				}
			}

			yield return schedule;
		}

		protected virtual void Schedule_Module_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = BatchModule.AP;
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
                sender.RaiseExceptionHandling<Schedule.weeklyOnDay1>(schedule, null, new PXSetPropertyException(GL.Messages.DayOfWeekNotSelected));
                nextRunDate = false;
            }

            if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete && nextRunDate)
            {
                ((Schedule)e.Row).NextRunDate = GL.ScheduleProcess.GetNextRunDate(this, (Schedule)e.Row);
            }

        }

		protected virtual void DocumentSelection_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if(e.Row == null) return;
			PXUIFieldAttribute.SetEnabled<DocumentSelection.docType>(sender, e.Row, ((DocumentSelection)e.Row).Scheduled != true);
			PXUIFieldAttribute.SetEnabled<DocumentSelection.refNbr>(sender, e.Row, ((DocumentSelection)e.Row).Scheduled != true);
		}

		private void SetControlsState(PXCache cache, Schedule s)
		{
			Boolean isDeferred = (s.ScheduleType == "R" || s.ScheduleType == "E");
			Boolean isMonthly = (s.ScheduleType == "M");
			Boolean isPeriodically = (s.ScheduleType == "P");
			Boolean isWeekly = (s.ScheduleType == "W");
			Boolean isDaily = (s.ScheduleType == "D");
			Boolean isNotProcessed = (s.LastRunDate == null);

			if (isDeferred)
			{
				isNotProcessed = (Document_History.Select().Count == 0);
			}

			if (isDeferred)
			{
				GLScheduleType.FullListAttribute attr = new GLScheduleType.FullListAttribute();
				PXStringListAttribute.SetList<Schedule.formScheduleType>(cache, null, attr.AllowedValues, attr.AllowedLabels);
			}
			else
			{
				GLScheduleType.ListAttribute attr = new GLScheduleType.ListAttribute();
				PXStringListAttribute.SetList<Schedule.formScheduleType>(cache, null, attr.AllowedValues, attr.AllowedLabels);
			}

			PXUIFieldAttribute.SetEnabled<Schedule.noRunLimit>(cache, s, isDeferred == false);
			PXUIFieldAttribute.SetEnabled<Schedule.noEndDate>(cache, s, isDeferred == false);
			PXUIFieldAttribute.SetEnabled<Schedule.runLimit>(cache, s, isDeferred == false && s.NoRunLimit == false);
			PXUIFieldAttribute.SetEnabled<Schedule.endDate>(cache, s, isDeferred == false && s.NoEndDate == false);

			PXUIFieldAttribute.SetEnabled<Schedule.nextRunDate>(cache, s, isDeferred == false && isNotProcessed == false);
			PXUIFieldAttribute.SetEnabled<Schedule.formScheduleType>(cache, s, isNotProcessed);
			PXUIFieldAttribute.SetEnabled<Schedule.startDate>(cache, s, isNotProcessed);

            PXUIFieldAttribute.SetVisible<Schedule.monthlyFrequency>(cache, s, isMonthly);
            PXUIFieldAttribute.SetVisible<Schedule.monthlyDaySel>(cache, s, isMonthly);
			SetMonthlyControlsState(cache, s, isMonthly);

            PXUIFieldAttribute.SetVisible<Schedule.periodFrequency>(cache, s, isPeriodically);
            PXUIFieldAttribute.SetVisible<Schedule.periodDateSel>(cache, s, isPeriodically);
            PXUIFieldAttribute.SetVisible<Schedule.periodFixedDay>(cache, s, isPeriodically);
            PXUIFieldAttribute.SetVisible<Schedule.periods>(cache, s, isPeriodically);
			SetPeriodicallyControlsState(cache, s, isPeriodically);

            PXUIFieldAttribute.SetVisible<Schedule.weeklyFrequency>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay1>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay2>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay3>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay4>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay5>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay6>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay7>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeks>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.dailyFrequency>(cache, s, isDaily);
            PXUIFieldAttribute.SetVisible<Schedule.days>(cache, s, isDaily);

			PXDefaultAttribute.SetPersistingCheck<Schedule.endDate>(cache, s, (s.NoEndDate == true ? PXPersistingCheck.Nothing : PXPersistingCheck.Null));

			PXUIFieldAttribute.SetEnabled<DocumentSelection.vendorID>(Document_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DocumentSelection.status>(Document_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DocumentSelection.docDate>(Document_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DocumentSelection.finPeriodID>(Document_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DocumentSelection.curyOrigDocAmt>(Document_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DocumentSelection.curyID>(Document_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DocumentSelection.docDesc>(Document_Detail.Cache, null, false);

			cache.AllowDelete = isNotProcessed;
		}

		private void SetMonthlyControlsState(PXCache cache, Schedule s, Boolean isMonthly)
		{
			PXUIFieldAttribute.SetEnabled<Schedule.monthlyOnDay>(cache, s, isMonthly && s.MonthlyDaySel == "D");
			PXUIFieldAttribute.SetEnabled<Schedule.monthlyOnWeek>(cache, s, isMonthly && s.MonthlyDaySel == "W");
			PXUIFieldAttribute.SetEnabled<Schedule.monthlyOnDayOfWeek>(cache, s, isMonthly && s.MonthlyDaySel == "W");
		}

		private void SetPeriodicallyControlsState(PXCache cache, Schedule s, Boolean isPeriodically)
		{
			PXUIFieldAttribute.SetEnabled<Schedule.periodFixedDay>(cache, s, isPeriodically && s.PeriodDateSel == "D");
		}

		
		protected virtual void Schedule_NoEndDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			if (!(bool)e.OldValue && (bool)((Schedule)e.Row).NoEndDate != (bool)e.OldValue)
			{
				Schedule s = (Schedule)e.Row;
				s.EndDate = null;
			}
		}

		protected virtual void Schedule_NoRunLimit_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			if ((bool)e.OldValue && (bool)((Schedule)e.Row).NoRunLimit != (bool)e.OldValue)
			{
				Schedule s = (Schedule)e.Row;
				s.RunLimit = 1;
			}
		}

		protected virtual void Schedule_ScheduleType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Schedule s = (Schedule)e.Row;
			if (s.NextRunDate != null && !object.Equals(e.OldValue, s.ScheduleType))
			{
				throw new PXException(PX.Objects.GL.Messages.ScheduleTypeCantBeChanged);
			}
		}

		protected virtual void DocumentSelection_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			APRegister ap = e.Row as APRegister;
			if (ap != null && ap.Voided == false)
			{
				ap.ScheduleID = Schedule_Header.Current.ScheduleID;
				ap.Scheduled = true;
			}
		}

        protected virtual void DocumentSelection_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            DocumentSelection docSelection = (DocumentSelection)e.Row;
            if (docSelection != null && !String.IsNullOrWhiteSpace(docSelection.DocType) && !String.IsNullOrWhiteSpace(docSelection.RefNbr))
            {
                docSelection = PXSelectReadonly<DocumentSelection, Where<DocumentSelection.docType, Equal<Required<DocumentSelection.docType>>,
                    And<DocumentSelection.refNbr, Equal<Required<DocumentSelection.refNbr>>>>>.Select(this, docSelection.DocType, docSelection.RefNbr);
                if (docSelection != null)
                {
                    Document_Detail.Delete(docSelection);
                    Document_Detail.Update(docSelection);
                }
                else
                {
                    docSelection = (DocumentSelection)e.Row;
                    Document_Detail.Delete(docSelection);
                    cache.RaiseExceptionHandling<DocumentSelection.refNbr>(docSelection, docSelection.RefNbr, new PXSetPropertyException(Messages.ReferenceNotValid));
                }
            }
        }

		protected virtual void Schedule_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			foreach (DocumentSelection b in PXSelect<DocumentSelection, Where<DocumentSelection.scheduleID, Equal<Required<Schedule.scheduleID>>>>.Select(this, ((Schedule)e.Row).ScheduleID))
			{
				if ((bool)b.Scheduled)
				{
					b.Voided = true;
					b.Scheduled = false;
				}
				b.ScheduleID = null;

				if (Document_Detail.Cache.GetStatus(b) == PXEntryStatus.Notchanged)
				{
					Document_Detail.Cache.SetStatus(b, PXEntryStatus.Updated);
				}
				PXDBDefaultAttribute.SetDefaultForUpdate<DocumentSelection.scheduleID>(Document_Detail.Cache, b, false);
			}
		}
		
		public override void Persist()
		{
			foreach (DocumentSelection b in Document_Detail.Cache.Updated)
			{
				if (b.Voided == false)
				{
					b.Scheduled = true;
					b.ScheduleID = Schedule_Header.Current.ScheduleID;
					Document_Detail.Cache.Update(b);
				}
			}

			foreach (DocumentSelection b in Document_Detail.Cache.Deleted)
			{
				PXDBDefaultAttribute.SetDefaultForUpdate<DocumentSelection.scheduleID>(Document_Detail.Cache, b, false);
				b.Voided = true;
				b.OpenDoc = false;
				b.Scheduled = false;
				b.ScheduleID = null;
				Document_Detail.Cache.SetStatus(b, PXEntryStatus.Updated);
				Document_Detail.Cache.Update(b);
			}
			base.Persist();
		}

		public PXAction<Schedule> viewDocument;
		[PXUIField(DisplayName = SO.Messages.ViewDocument, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (Document_Detail.Current == null) return adapter.Get();
			APInvoiceEntry graph = CreateInstance<APInvoiceEntry>();
			graph.Document.Current = graph.Document.Search<APInvoice.refNbr>(Document_Detail.Current.RefNbr, Document_Detail.Current.DocType);
			throw new PXRedirectRequiredException(graph, true, "Document"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
		}

		public PXAction<Schedule> viewGenDocument;
        [PXUIField(DisplayName = SO.Messages.ViewDocument, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable ViewGenDocument(PXAdapter adapter)
		{
			if (Document_Detail.Current == null) return adapter.Get();
			APInvoiceEntry graph = CreateInstance<APInvoiceEntry>();
			graph.Document.Current = graph.Document.Search<APInvoice.refNbr>(Document_History.Current.RefNbr, Document_History.Current.DocType);
			throw new PXRedirectRequiredException(graph, true, "Generated Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

	}
}

namespace PX.Objects.AP.Overrides.ScheduleMaint
{
	[PXPrimaryGraph(null)]
    [Serializable]
	public partial class DocumentSelection : APRegister
	{
		#region ScheduleID
		public new abstract class scheduleID : IBqlField
		{
		}
		[PXDBString(10, IsUnicode = true)]
		[PXDBDefault(typeof(Schedule.scheduleID))]
		[PXParent(typeof(Select<Schedule, Where<Schedule.scheduleID, Equal<Current<APRegister.scheduleID>>>>))]
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
		#region DocType
		public new abstract class docType : PX.Data.IBqlField
		{
		}
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault(APDocType.Invoice)]
		[PXStringList(new string[] { APDocType.Invoice }, new string[] { Messages.Invoice })]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		public override String DocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
				this._DocType = value;
			}
		}
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[APInvoiceType.RefNbr(typeof(Search<APRegister.refNbr, Where<APRegister.docType, Equal<Optional<APRegister.docType>>, And<APRegister.released, Equal<boolFalse>, And<APRegister.hold, Equal<boolFalse>, And<APRegister.voided, Equal<boolFalse>>>>>>))]
		public override String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
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
		#region OpenDoc
		public new abstract class openDoc : PX.Data.IBqlField
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
