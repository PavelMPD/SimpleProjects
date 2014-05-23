using System;
using PX.Common;
using PX.Data.EP;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.SM;
using PX.TM;
using PX.Data;

namespace PX.Objects.CR
{
	#region CRChildActivity

	[Serializable]
	public partial class CRChildActivity : EPActivity
	{
		#region TaskID

		public new abstract class taskID : IBqlField { }

		#endregion

		#region ParentTaskID

		public new abstract class parentTaskID : IBqlField { }

		#endregion

		#region ClassID

		public new abstract class classID : IBqlField { }

		#endregion

		#region CreatedDateTime

		public new abstract class createdDateTime : IBqlField { }

		#endregion

		#region Owner

		public new abstract class owner : IBqlField { }

		#endregion
	}

	#endregion

	#region TaskActivityMaintBase
	public class CRTaskMaint : CRBaseActivityMaint<CRTaskMaint>
	{
		#region Selects

		[PXHidden]
		public PXSelect<CT.Contract>
				BaseContract;

		[PXRefNoteSelectorAttribute(typeof(EPActivity), typeof(EPActivity.refNoteID))]
		public PXSelect<EPActivity, 
			Where<EPActivity.classID, Equal<CRActivityClass.task>>> 
			Tasks;

		[PXFilterable]
		[CRBAccountReference(typeof(Select<EPActivity, Where<EPActivity.taskID, Equal<Current<EPActivity.taskID>>>>), RefField = typeof(EPActivity.parentRefNoteID))]
		[PXViewDetailsButton(typeof(EPActivity),
			typeof(Select<EPActivity,
					Where<EPActivity.taskID, Equal<Current<CRChildActivity.taskID>>>>))]
		public CRChildActivityList<EPActivity>
			ChildActivities;

		[PXFilterable]
		public CRReferencedTaskList<EPActivity>
			ReferencedTasks;

		#endregion

		#region Ctors

		public CRTaskMaint()
			: base()
		{
			ChildActivities.Cache.AllowUpdate = false;

			this.EnshureCachePersistance(ChildActivities.Cache.GetItemType());
			var view = this.ReferencedTasks.View;
			PXEntityInfoAttribute.SetDescriptionDisplayName<EPActivity.source>(Tasks.Cache, "Related Entity");
			PXDBDateAndTimeAttribute.SetDateDisplayName<EPActivity.startDate>(Tasks.Cache, null, "Start Date");
			PXDBDateAndTimeAttribute.SetDateDisplayName<EPActivity.endDate>(Tasks.Cache, null, "Due Date");
		}

		#endregion

		#region Actions

		public PXDelete<EPActivity> Delete;

		public PXAction<EPActivity> Complete;
		[PXUIField(DisplayName = TM.Messages.CompleteTask, MapEnableRights = PXCacheRights.Select)]
		[PXButton(Tooltip = Messages.CompleteTaskTooltip,
			ShortcutCtrl = true, ShortcutChar = (char)75)] //Ctrl + K
		protected virtual void complete()
		{
			var row = Tasks.Current;
			if (row == null) return;

			CompleteTask(row);
		}

		public PXAction<EPActivity> CompleteAndFollowUp;
		[PXUIField(DisplayName = Messages.CompleteTaskAndFollowUp, MapEnableRights = PXCacheRights.Select)]
		[PXButton(Tooltip = Messages.CompleteTaskAndFollowUpTooltip,
			ShortcutCtrl = true, ShortcutShift = true, ShortcutChar = (char)75)] //Ctrl + Shift + K
		protected virtual void completeAndFollowUp()
		{
			EPActivity row = Tasks.Current;
			if (row == null) return;

			CompleteTask(row);

			CRTaskMaint graph = CreateInstance<CRTaskMaint>();

			EPActivity followUpTask = (EPActivity)graph.Tasks.Cache.CreateCopy(row);
			followUpTask.TaskID = null;
			followUpTask.ParentTaskID = row.ParentTaskID;
			followUpTask.UIStatus = null;
			followUpTask.NoteID = null;
			followUpTask.PercentCompletion = null;

			if (followUpTask.StartDate != null)
			{
				followUpTask.StartDate = ((DateTime) followUpTask.StartDate).AddDays(1D);
				graph.Tasks.Cache.SetDefaultExt<EPActivity.weekID>(followUpTask);
			}
			if (followUpTask.EndDate != null)
				followUpTask.EndDate = ((DateTime)followUpTask.EndDate).AddDays(1D);

			followUpTask.TimeBillable = null;
			followUpTask.OvertimeBillable = null;

			graph.Tasks.Cache.Insert(followUpTask);
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXAction<EPActivity> CancelActivity;
		[PXUIField(DisplayName = TM.Messages.CancelTask, MapEnableRights = PXCacheRights.Select)]
		[PXButton(Tooltip = TM.Messages.CancelTask)]
		protected virtual void cancelActivity()
		{
			var row = Tasks.Current;
			if (row == null) return;

			CancelTask(row);
		}

		#endregion

		#region Event Handlers
		public abstract class uistatus : IBqlField { }
		[PXDBString(2, IsFixed = true)]
		[TaskStatus]
		[PXUIField(DisplayName = "Status")]
		[PXDefault(ActivityStatusAttribute.Open, PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPActivity_UIStatus_CacheAttached(PXCache cache)
		{
		}

		public abstract class trackTime : IBqlField { }		
		[PXDBBool]
		[PXUIField(DisplayName = "Track Time", Visible = false)]
		[PXDefault(true)]
		protected virtual void EPActivity_TrackTime_CacheAttached(PXCache cache)
		{
		}


		[PXDBBool()]
		[PXUIField(DisplayName = "Billable", Visible = false)]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPActivity_IsBillable_CacheAttached(PXCache cache)
		{
		}

		
		[PXDBIdentity(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXSelector(typeof(EPActivity.taskID),
			new[] { typeof(EPActivity.taskID), typeof(EPActivity.extID) })]
		protected virtual void EPActivity_TaskID_CacheAttached(PXCache cache)
		{
		}

		[PXDBGuid()]
		[PXChildUpdatable(AutoRefresh = true)]
		[PXOwnerSelector(typeof(EPActivity.groupID))]
		[PXUIField(DisplayName = "Assigned To")]
		[PXDefault(typeof(Coalesce<
								Search<EPCompanyTreeMember.userID,
									Where<EPCompanyTreeMember.workGroupID, Equal<Current<EPActivity.groupID>>,
									And<EPCompanyTreeMember.userID, Equal<Current<AccessInfo.userID>>>>>,
								Search2<Users.pKID,
									InnerJoin<EPEmployee, On<EPEmployee.userID, Equal<Users.pKID>>>,
									Where<Users.pKID, Equal<Current<AccessInfo.userID>>,
									And<Current<EPActivity.groupID>, IsNull>>>>),
									PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<EPActivity.groupID>))]
		protected virtual void EPActivity_Owner_CacheAttached(PXCache cache)
		{
		}


		protected virtual void EPActivity_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			var row = (EPActivity)e.Row;
			if (row == null) return;

			row.ClassID = CRActivityClass.Task;			

			if (row.UIStatus == ActivityStatusListAttribute.Completed)
			{
				row.PercentCompletion = 100;
			}		
		}

		protected virtual void EPActivity_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as EPActivity;
			var oldRow = (EPActivity)e.OldRow;
			if (row == null || oldRow == null) return;

			row.ClassID = CRActivityClass.Task;

			if (row.UIStatus == ActivityStatusListAttribute.Completed)
			{
				row.PercentCompletion = 100;
				if(!object.Equals(sender.GetValueOriginal<EPActivity.uistatus>(row), ActivityStatusListAttribute.Completed))
					row.CompletedDateTime = PXTimeZoneInfo.Now;
			}			
		}

		protected virtual void EPActivity_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			EPActivity row = e.Row as EPActivity;
			if (row == null) return;

			string status = ((string) cache.GetValueOriginal<EPActivity.uistatus>(row) ?? ActivityStatusListAttribute.Open);
			bool editable = status == ActivityStatusListAttribute.Open || status == ActivityStatusListAttribute.Draft || status == ActivityStatusListAttribute.InProcess;

			PXUIFieldAttribute.SetEnabled(cache, row, editable);
			Delete.SetEnabled(editable);
			Complete.SetEnabled(editable);			
			CompleteAndFollowUp.SetEnabled(editable);
			CancelActivity.SetEnabled(editable);

			PXUIFieldAttribute.SetEnabled<EPActivity.taskID>(cache, row);
			PXUIFieldAttribute.SetEnabled<EPActivity.uistatus>(cache, row);

			PXUIFieldAttribute.SetEnabled<EPActivity.timeSpent>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<EPActivity.createdByID>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<EPActivity.completedDateTime>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<EPActivity.overtimeSpent>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<EPActivity.timeBillable>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<EPActivity.overtimeBillable>(cache, row, false);

			PXUIFieldAttribute.SetEnabled<EPActivity.reminderDate>(cache, row, row.IsReminderOn == true);

			GotoParentActivity.SetEnabled(row.ParentTaskID != null);

			this.ChildActivities.Cache.AllowDelete = 
			this.ReferencedTasks.Cache.AllowInsert =
			this.ReferencedTasks.Cache.AllowUpdate =
			this.ReferencedTasks.Cache.AllowDelete = editable;

			int timespent = 0; 
			int overtimespent = 0;
			int timebillable = 0;
			int overtimebillable = 0;

			foreach (EPActivity child in ChildActivities.Select(row.TaskID))
			{
				timespent += (child.TimeSpent ?? 0);
				overtimespent += (child.OvertimeSpent ?? 0);
				timebillable += (child.TimeBillable ?? 0);
				overtimebillable += (child.OvertimeBillable ?? 0);
			}

			row.TimeSpent = timespent;
			row.OvertimeSpent = overtimespent;
			row.TimeBillable = timebillable;
			row.OvertimeBillable = overtimebillable;
		}

		protected virtual void EPActivity_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = e.Row as EPActivity;
			if (row == null) return;

			VerifyReminder(row);
			if ((e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update) && row.Owner == null && row.GroupID == null)
			{
				var reminderDateDisplayName = PXUIFieldAttribute.GetDisplayName<EPActivity.owner>(Tasks.Cache);
				var exception = new PXSetPropertyException(ErrorMessages.FieldIsEmpty, reminderDateDisplayName);
				if (Tasks.Cache.RaiseExceptionHandling<EPActivity.owner>(row, null, exception))
				{
					throw new PXRowPersistingException(typeof(EPActivity.owner).Name, null, ErrorMessages.FieldIsEmpty, reminderDateDisplayName);
				}

			}
		}
		protected virtual void EPActivity_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
		{
			var row = e.Row as EPActivity;
			if (row == null ||
				e.TranStatus != PXTranStatus.Open ||
				cache.GetValuePending(row, PXImportAttribute.ImportFlag) != null)
			{
				return;
			}

			var operation = e.Operation & PXDBOperation.Command;
			if (operation == PXDBOperation.Insert || operation == PXDBOperation.Update)
				EPViewStatusAttribute.MarkAsViewed(this, row, true);
		}

		#endregion

		#region Private Methods

		private void CompleteTask(EPActivity row)
		{
			string origStatus = (string)this.Tasks.Cache.GetValueOriginal<EPActivity.uistatus>(row) ?? ActivityStatusAttribute.Open;			
			if (origStatus == ActivityStatusAttribute.Completed ||
					origStatus == ActivityStatusAttribute.Canceled)
			{
				return;
			}
			
			var activityCopy = (EPActivity)Tasks.Cache.CreateCopy(row);
		  activityCopy.UIStatus = ActivityStatusListAttribute.Completed;			
			Tasks.Cache.Update(activityCopy);
			EPViewStatusAttribute.MarkAsViewed(this, row);
			Actions.PressSave();
		}

		private void CancelTask(EPActivity row)
		{
			string origStatus = (string)this.Tasks.Cache.GetValueOriginal<EPActivity.uistatus>(row) ?? ActivityStatusAttribute.Open;
			if (origStatus == ActivityStatusAttribute.Completed ||
					origStatus == ActivityStatusAttribute.Canceled)

			{
				return;
			}

			var activityCopy = (EPActivity)Tasks.Cache.CreateCopy(row);			
      activityCopy.UIStatus = ActivityStatusListAttribute.Canceled;
			Tasks.Cache.Update(activityCopy);
			EPViewStatusAttribute.MarkAsViewed(this, row);
			Actions.PressSave();
		}

		private void VerifyReminder(EPActivity row)
		{
			if (row.IsReminderOn == true && row.ReminderDate == null)
			{
				var reminderDateDisplayName = PXUIFieldAttribute.GetDisplayName<EPActivity.reminderDate>(Tasks.Cache);
				var exception = new PXSetPropertyException(ErrorMessages.FieldIsEmpty, reminderDateDisplayName);
				if (Tasks.Cache.RaiseExceptionHandling<EPActivity.reminderDate>(row, null, exception))
				{
					throw new PXRowPersistingException(typeof(EPActivity.reminderDate).Name, null, ErrorMessages.FieldIsEmpty, reminderDateDisplayName);
				}
			}
		}

		#endregion

		#region Public Methods

		public override void CompleteRow(EPActivity row)
		{
			if (row != null) CompleteTask(row);
		}

		public override void CancelRow(EPActivity row)
		{
			if (row != null) CancelTask(row);
		}

		#endregion
	}

	#endregion
}
