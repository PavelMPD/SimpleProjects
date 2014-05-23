using PX.Common;
using PX.Data.EP;
using PX.Objects.CR;
using PX.Data;
using System;
using PX.Objects.GL;
using PX.Objects.PM;

namespace PX.Objects.EP
{
	public class CRActivityMaint : CRBaseActivityMaint<CRActivityMaint>
	{
		#region Constants

		private static readonly EPSetup EmptyEpSetup = new EPSetup();

		#endregion

		#region Selects

		[PXHidden]
		public PXSelect<CT.Contract>
			BaseContract;

		[PXRefNoteSelectorAttribute(typeof(EPActivity), typeof(EPActivity.refNoteID))]
		public PXSelect<EPActivity,
			Where<EPActivity.classID, Equal<CRActivityClass.activity>>>
			Activites;

		#endregion

		#region Ctors

		public CRActivityMaint()
		{
			CRCaseActivityHelper.Attach(this);
			PXEntityInfoAttribute.SetDescriptionDisplayName<EPActivity.source>(Activites.Cache, "Related Entity");
		}

		#endregion

		#region Actions

		public PXDelete<EPActivity> Delete;

		public PXAction<EPActivity> MarkAsCompleted;
		[PXUIField(DisplayName = Messages.Complete)]
		[PXButton(Tooltip = Messages.MarkAsCompletedTooltip,
			ShortcutCtrl = true, ShortcutChar = (char)75)] //Ctrl + K
		public virtual void markAsCompleted()
		{
			var row = Activites.Current;
			if (row == null) return;

			CompleteActivity(row);
		}

		public PXAction<EPActivity> MarkAsCompletedAndFollowUp;
		[PXUIField(DisplayName = Messages.CompleteAndFollowUp)]
		[PXButton(Tooltip = Messages.CompleteAndFollowUpTooltip,
			ShortcutCtrl = true, ShortcutShift = true, ShortcutChar = (char)75)] //Ctrl + Shift + K
		public virtual void markAsCompletedAndFollowUp()
		{
			EPActivity row = Activites.Current;
			if (row == null) return;

			CompleteActivity(row);

			CRActivityMaint graph = CreateInstance<CRActivityMaint>();

			EPActivity followUpActivity = (EPActivity)graph.Activites.Cache.CreateCopy(row);
			followUpActivity.TaskID = null;
			followUpActivity.ParentTaskID = row.ParentTaskID;
			followUpActivity.UIStatus = null;
			followUpActivity.NoteID = null;
			followUpActivity.PercentCompletion = null;

			if (followUpActivity.StartDate != null)
			{
				followUpActivity.StartDate = ((DateTime)followUpActivity.StartDate).AddDays(1D);
				graph.Activites.Cache.SetDefaultExt<EPActivity.weekID>(followUpActivity);
			}
			if (followUpActivity.EndDate != null)
				followUpActivity.EndDate = ((DateTime)followUpActivity.EndDate).AddDays(1D);

			followUpActivity.TimeBillable = null;
			followUpActivity.OvertimeBillable = null;

			graph.Activites.Insert(followUpActivity);
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);

		}

		#endregion

		#region Event Handlers

		[PXUIField(DisplayName = "Task")]
		[PXDBInt]
		[PXSelector(typeof(Search<EPActivity.taskID,
			Where<EPActivity.taskID, NotEqual<Current<EPActivity.taskID>>,
				And<Where<EPActivity.classID, Equal<CRActivityClass.task>,
						Or<EPActivity.classID, Equal<CRActivityClass.events>>>>>,
			OrderBy<Desc<EPActivity.taskID>>>),
			typeof(EPActivity.taskID), typeof(EPActivity.subject), typeof(EPActivity.classInfo), typeof(EPActivity.uistatus),
			DescriptionField = typeof(EPActivity.subject))]
		protected virtual void EPActivity_ParentTaskID_CacheAttached(PXCache cache)
		{

		}

		protected virtual void EPActivity_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			//TODO redesign by task #32833
			var row = (EPActivity)e.Row;
			if (row == null) return;
			row.ClassID = CRActivityClass.Activity;
		}

		protected virtual void EPActivity_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			var row = (EPActivity)e.Row;
			var oldRow = (EPActivity)e.OldRow;
			if (row == null || oldRow == null) return;
			row.ClassID = CRActivityClass.Activity;
		}

		protected virtual void EPActivity_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			EPActivity row = (EPActivity)e.Row;
			if (row == null) return;

			bool isPmVisible = ProjectAttribute.IsPMVisible(this, BatchModule.EP);
			PXUIFieldAttribute.SetVisible<EPActivity.projectID>(cache, row, isPmVisible);
			PXUIFieldAttribute.SetVisible<EPActivity.projectTaskID>(cache, row, isPmVisible);

			PXUIFieldAttribute.SetEnabled<EPActivity.endDate>(cache, row, false);

			bool showMinutes = EPSetupCurrent.RequireTimes == true;
			PXDBDateAndTimeAttribute.SetTimeVisible<EPActivity.startDate>(cache, row, showMinutes && row.TrackTime == true);
			PXDBDateAndTimeAttribute.SetTimeVisible<EPActivity.endDate>(cache, row, showMinutes && row.TrackTime == true);

			bool wasUsed = !string.IsNullOrEmpty(row.TimeCardCD) || row.Billed == true;
			if (wasUsed) PXUIFieldAttribute.SetEnabled(cache, row, false);
			PXUIFieldAttribute.SetVisible<EPActivity.timeSpent>(cache, row, row.TrackTime == true);
			PXUIFieldAttribute.SetVisible<EPActivity.earningTypeID>(cache, row, row.TrackTime == true);
			PXUIFieldAttribute.SetVisible<EPActivity.isBillable>(cache, row, row.TrackTime == true);
			PXUIFieldAttribute.SetVisible<EPActivity.released>(cache, row, row.TrackTime == true);
			PXUIFieldAttribute.SetVisible<EPActivity.timeBillable>(cache, row, row.IsBillable == true && row.TrackTime == true);
			PXUIFieldAttribute.SetVisible<EPActivity.uistatus>(cache, row, row.TrackTime == true);
			PXUIFieldAttribute.SetVisible<EPActivity.approverID>(cache, row, row.TrackTime == true);
			PXUIFieldAttribute.SetVisible<EPActivity.projectID>(cache, row, row.TrackTime == true);
			PXUIFieldAttribute.SetVisible<EPActivity.projectTaskID>(cache, row, row.TrackTime == true);

			PXUIFieldAttribute.SetVisible<EPActivity.overtimeSpent>(cache, row, false);
			PXUIFieldAttribute.SetVisible<EPActivity.overtimeBillable>(cache, row, false);

			string origStatus =
				(string)this.Activites.Cache.GetValueOriginal<EPActivity.uistatus>(row) ?? ActivityStatusListAttribute.Open;

			bool? oringTrackTime =
			(bool?)this.Activites.Cache.GetValueOriginal<EPActivity.trackTime>(row) ?? false;

			if (origStatus == ActivityStatusListAttribute.Completed && oringTrackTime != true)
				origStatus = ActivityStatusListAttribute.Open;

			if (row.Released == true)
				origStatus = ActivityStatusListAttribute.Completed;

			if (origStatus == ActivityStatusListAttribute.Open)
			{
				PXUIFieldAttribute.SetEnabled(cache, row, true);
				PXUIFieldAttribute.SetEnabled<EPActivity.timeBillable>(cache, row, !wasUsed && row.IsBillable == true);
				PXUIFieldAttribute.SetEnabled<EPActivity.overtimeBillable>(cache, row, !wasUsed && row.IsBillable == true);
				Delete.SetEnabled(!wasUsed);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(cache, row, false);
				PXUIFieldAttribute.SetEnabled<EPActivity.uistatus>(cache, row, !wasUsed && row.Released != true);
				Delete.SetEnabled(false);
			}

			PXUIFieldAttribute.SetEnabled<EPActivity.released>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<EPActivity.taskID>(cache, row);

			MarkAsCompleted.SetEnabled(origStatus == ActivityStatusListAttribute.Open);
			MarkAsCompleted.SetVisible(origStatus == ActivityStatusListAttribute.Open && row.TrackTime == true);
			MarkAsCompletedAndFollowUp.SetVisible(false);

			ValidateTimeBillable(cache, row);
			ValidateOvertimeBillable(cache, row);

			GotoParentActivity.SetEnabled(row.ParentTaskID != null);

			PXDefaultAttribute.SetPersistingCheck<EPActivity.owner>(cache, row, row.TrackTime == true ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<EPActivity.type>(cache, row, PXPersistingCheck.Null);
		}

		protected virtual void EPActivity_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			var row = (EPActivity)e.Row;
			if (row == null) return;

			if (!string.IsNullOrEmpty(row.TimeCardCD) || row.Billed == true)
				throw new PXException(Messages.ActivityIsBilled);
		}

		protected virtual void EPActivity_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = e.Row as EPActivity;
			if (row == null) return;

			ValidateTimeBillable(sender, row);
			ValidateOvertimeBillable(sender, row);
		}

		protected virtual void EPActivity_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			var row = e.Row as EPActivity;
			if (row == null ||
				e.TranStatus != PXTranStatus.Open ||
				sender.GetValuePending(row, PXImportAttribute.ImportFlag) != null)
			{
				return;
			}

			var operation = e.Operation & PXDBOperation.Command;
			if (operation == PXDBOperation.Insert || operation == PXDBOperation.Update)
				EPViewStatusAttribute.MarkAsViewed(this, row, true);
		}

		#endregion

		#region Public Methods

		public override void CompleteRow(EPActivity row)
		{
			CompleteActivity(row);
		}

		public static TimeSpan CalculateOvertime(PXGraph graph, EPActivity act, DateTime start, DateTime end)
		{
			var calendarId = GetCalendarID(graph, act);
			return calendarId == null ? new TimeSpan() : CalendarHelper.CalculateOvertime(graph, start, end, calendarId);
		}

		public static string GetCalendarID(PXGraph graph, EPActivity act)
		{
			var projectCalendar = act.ProjectID.
				With(_ => (CT.Contract)PXSelect<CT.Contract,
					Where<CT.Contract.contractID, Equal<Required<CT.Contract.contractID>>>>.
				Select(graph, _.Value)).
				With(_ => _.CalendarID);
			if (projectCalendar != null) return projectCalendar;

			var projectTaskCalendar = act.ProjectTaskID.
				With(_ => (PXResult<Location, PM.PMTask>)PXSelectJoin<Location,
					InnerJoin<PM.PMTask, On<PM.PMTask.locationID, Equal<Location.locationID>>>,
					Where<PM.PMTask.taskID, Equal<Required<PM.PMTask.taskID>>>>.
				Select(graph, _.Value)).
				With(_ => ((Location)_).CCalendarID);
			if (projectTaskCalendar != null) return projectTaskCalendar;

			var caseLocationCalendar = act.RefNoteID.
				With(_ => (PXResult<Location, CRCase>)PXSelectJoin<Location,
					InnerJoin<CRCase, On<CRCase.locationID, Equal<Location.locationID>>>,
					Where<CRCase.noteID, Equal<Required<CRCase.noteID>>>>.
				Select(graph, _.Value)).
				With(_ => ((Location)_).CCalendarID);
			if (caseLocationCalendar != null) return caseLocationCalendar;

			var employeeCalendar = act.Owner.
				With(_ => (EPEmployee)PXSelect<EPEmployee,
					Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.
				Select(graph, _.Value)).
				With(_ => _.CalendarID);
			if (employeeCalendar != null) return employeeCalendar;

			return null;
		}

		#endregion

		#region Private Methods

		private void CompleteActivity(EPActivity activity)
		{
			string origStatus = (string)this.Activites.Cache.GetValueOriginal<EPActivity.uistatus>(activity) ?? ActivityStatusListAttribute.Open;

			if (activity == null ||
				origStatus == ActivityStatusListAttribute.Completed ||
				origStatus == ActivityStatusListAttribute.Canceled)
			{
				return;
			}

			var activityCopy = (EPActivity)Activites.Cache.CreateCopy(activity);
			activityCopy.UIStatus = ActivityStatusListAttribute.Completed;
			Activites.Cache.Update(activityCopy);
			EPViewStatusAttribute.MarkAsViewed(this, activity);
			Save.Press();
		}

		private void ValidateTimeBillable(PXCache sender, EPActivity row)
		{
			sender.RaiseExceptionHandling<EPActivity.timeBillable>(row, null, null);
			if (row.TimeBillable != null && row.TimeBillable > row.TimeSpent)
			{
				var exception = new PXSetPropertyException(CR.Messages.BillableTimeCannotBeGreaterThanTimeSpent);
				sender.RaiseExceptionHandling<EPActivity.timeBillable>(row, row.TimeBillable, exception);
			}
		}

		private void ValidateOvertimeBillable(PXCache sender, EPActivity row)
		{
			sender.RaiseExceptionHandling<EPActivity.overtimeBillable>(row, null, null);
			if (row.OvertimeBillable != null && row.OvertimeBillable > row.OvertimeSpent)
			{
				var exception = new PXSetPropertyException(CR.Messages.OvertimeBillableCannotBeGreaterThanOvertimeSpent);
				sender.RaiseExceptionHandling<EPActivity.overtimeBillable>(row, row.OvertimeBillable, exception);
			}
		}

		private EPSetup EPSetupCurrent
		{
			get
			{
				var res = (EPSetup)PXSelect<EPSetup>.
					SelectWindowed(this, 0, 1);
				return res ?? EmptyEpSetup;
			}
		}


		#endregion
	}
}
