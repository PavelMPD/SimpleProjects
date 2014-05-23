using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using PX.Objects.CR;
using PX.SM;
using PX.TM;
using PX.Data;
using System.Collections;

namespace PX.Objects.EP
{
	[DashboardType((int)DashboardTypeAttribute.Type.Task, GL.TableAndChartDashboardTypeAttribute._AMCHARTS_DASHBOART_TYPE)]
	public class EPTaskEnq : PXGraph<EPTaskEnq>
	{
		#region TaskFilter

		[Serializable]
		public partial class TaskFilter : PX.Objects.CR.OwnedFilter
		{
			#region OwnerID

			public new abstract class ownerID : IBqlField
			{
			}

			#endregion

			#region myOwner

			public new abstract class myOwner : IBqlField
			{

			}

			#endregion

			#region myWorkGroup

			public new abstract class myWorkGroup : IBqlField
			{

			}

			#endregion

			#region workGroupID

			public new abstract class workGroupID : IBqlField
			{

			}

			#endregion

			#region currentOwnerID

			public new abstract class currentOwnerID : IBqlField { }

			#endregion

			#region IsEscalated

			public abstract class isEscalated : IBqlField { }
			[PXDefault(false)]
			[PXBool]
			[PXUIField(DisplayName = "Escalated")]
			public virtual Boolean? IsEscalated { get; set; }

			#endregion

			#region IsFollowUp

			public abstract class isFollowUp : IBqlField { }
			[PXDefault(false)]
			[PXBool]
			[PXUIField(DisplayName = "Follow Up")]
			public virtual Boolean? IsFollowUp { get; set; }

			#endregion
		}

		#endregion

		#region MyEPCompanyTree

		[Serializable]
		[PXCacheName(Messages.Company)]
		public partial class MyEPCompanyTree : EPCompanyTree
		{
			public new abstract class workGroupID : IBqlField { }

			public new abstract class description : IBqlField { }
		}

		#endregion

		#region Selects

		[PXHidden]
		public PXSelect<BAccount>
			BaseBAccount;

		[PXHidden]
		public PXSelect<BAccount2>
			BAccount2View;

		[PXHidden]
		public PXSelect<EPCompanyTree>
			CompanyTree;

		public PXFilter<TaskFilter>
			Filter;

		[PXFilterable]
		[PXViewDetailsButton(typeof(TaskFilter))]
		public PXSelectReadonly3<EPActivity,
			OrderBy<Asc<EPActivity.endDate,
				Desc<EPActivity.priority,
				Asc<EPActivity.startDate>>>>>
			Tasks;

		#endregion

		#region Ctors

		public EPTaskEnq()
			: base()
		{
			PXUIFieldAttribute.SetDisplayName<MyEPCompanyTree.description>(Caches[typeof(MyEPCompanyTree)], Messages.WorkGroup);

			PXUIFieldAttribute.SetDisplayName<EPActivity.taskID>(Tasks.Cache, Messages.TaskID);
			PXUIFieldAttribute.SetDisplayName<EPActivity.endDate>(Tasks.Cache, CR.Messages.DueDate);

			PXUIFieldAttribute.SetVisible<EPActivity.taskID>(Tasks.Cache, null, true);
			PXUIFieldAttribute.SetVisible<EPActivity.subject>(Tasks.Cache, null, true);
			PXUIFieldAttribute.SetVisible<EPActivity.priority>(Tasks.Cache, null, true);
			PXUIFieldAttribute.SetVisible<EPActivity.uistatus>(Tasks.Cache, null, true);
			PXUIFieldAttribute.SetVisible<EPActivity.startDate>(Tasks.Cache, null, true);
			PXUIFieldAttribute.SetVisible<EPActivity.endDate>(Tasks.Cache, null, true);
			PXUIFieldAttribute.SetVisible<EPActivity.categoryID>(Tasks.Cache, null, false);
			PXUIFieldAttribute.SetVisible<EPActivity.percentCompletion>(Tasks.Cache, null, false);
			PXUIFieldAttribute.SetVisible<EPActivity.refNoteID>(Tasks.Cache, null, false);
		}

		#endregion

		#region Data Handlers

		public virtual IEnumerable tasks()
		{
			var command = new Select3<EPActivity,
				LeftJoin<MyEPCompanyTree,
					On<MyEPCompanyTree.workGroupID, Equal<EPActivity.groupID>>,
				LeftJoin<EPView,
					On<EPView.noteID, Equal<EPActivity.noteID>,
						And<EPView.userID, Equal<Current<AccessInfo.userID>>>>>>,
				OrderBy<Asc<EPActivity.endDate,
					Desc<EPActivity.priority,
					Asc<EPActivity.startDate>>>>>() as BqlCommand;
			var filter = Filter.Current;

			if (filter.OwnerID == null && filter.WorkGroupID == null)
			{
				command = command.WhereAnd(
					typeof(Where<EPActivity.groupID, Owned<CurrentValue<AccessInfo.userID>>,
										Or<Where<EPActivity.groupID, IsNull, 
												 And<EPActivity.owner, OwnedUser<CurrentValue<AccessInfo.userID>>>>>>));
			}
			else
			{
				if (filter.OwnerID != null)
					command = command.WhereOr(typeof (Where<EPActivity.owner, Equal<CurrentValue<TaskFilter.ownerID>>>));
				if (filter.WorkGroupID != null)
					command = command.WhereAnd(typeof (Where<EPActivity.groupID, Equal<CurrentValue<TaskFilter.workGroupID>>>));
				if (filter.MyWorkGroup == true)
					command = command.WhereAnd(typeof (Where<EPActivity.groupID, InMember<CurrentValue<TaskFilter.currentOwnerID>>>));
				if (filter.IsEscalated == true)
					command = command.WhereOr(typeof (Where<EPActivity.groupID,
						                          Escalated<CurrentValue<TaskFilter.ownerID>, EPActivity.groupID, EPActivity.owner, EPActivity.startDate>>));
				if (filter.IsFollowUp == true)
					command = command.WhereOr(typeof (Where<EPActivity.createdByID, Equal<CurrentValue<TaskFilter.ownerID>>>));
			}
			command = command.WhereAnd(typeof(Where<EPActivity.classID, Equal<CRActivityClass.task>>));

			var view = new PXView(this, true, command);
			var startRow = PXView.StartRow;
			int totalRows = 0;
			var list = view.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
								   ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;
			return list;
		}

		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region Actions

		public PXCancel<TaskFilter> Cancel;

		public PXAction<TaskFilter> CreateNew;
		[PXUIField(DisplayName = "New")]
		[PXButton(ImageKey = Web.UI.Sprite.Main.AddNew, Tooltip = Messages.AddTask)]
		public virtual void createNew()
		{
			var graph = PXGraph.CreateInstance<CRTaskMaint>();
			graph.Tasks.Insert();
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXAction<TaskFilter> CancelActivity;
		[PXUIField(DisplayName = PX.TM.Messages.CancelTask)]
		[PXButton(Tooltip = PX.TM.Messages.CancelTask)]
		protected virtual void cancelActivity()
		{
			var row = Tasks.Current;
			if (row == null) return;
            if (row.UIStatus == ActivityStatusAttribute.Draft)
                throw new PXException(Messages.CompleteAndCancelNotAvailableForTask);
			var graph = PXGraph.CreateInstance<CRTaskMaint>();
			graph.Tasks.Current = graph.Tasks.Search<EPActivity.taskID>(row.TaskID);
			graph.CancelActivity.PressButton();

            Tasks.Cache.ClearQueryCache();
		}

		public PXAction<TaskFilter> Complete;
		[PXUIField(DisplayName = PX.TM.Messages.CompleteTask)]
		[PXButton(Tooltip = PX.TM.Messages.CompleteTask)]
		protected virtual void complete()
		{
			var row = Tasks.Current;
			if (row == null) return;
			if (row.UIStatus == ActivityStatusAttribute.Draft)
                throw new PXException(Messages.CompleteAndCancelNotAvailableForTask);
			var graph = PXGraph.CreateInstance<CRTaskMaint>();
			graph.Tasks.Current = graph.Tasks.Search<EPActivity.taskID>(row.TaskID);
			graph.Complete.PressButton();

			Tasks.Cache.ClearQueryCache();
		}

		public PXAction<TaskFilter> ViewEntity;
		[PXUIField(DisplayName = Messages.ViewEntity, Visible = false)]
		[PXLookupButton(Tooltip = Messages.ttipViewEntity)]
		protected virtual void viewEntity()
		{
			var row = Tasks.Current;
			if (row == null) return;

			new EntityHelper(this).NavigateToRow(row.RefNoteID, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXAction<TaskFilter> ViewOwner;
		[PXUIField(DisplayName = Messages.ViewOwner, Visible = false)]
		[PXLookupButton(Tooltip = Messages.ttipViewOwner)]
		protected virtual IEnumerable viewOwner(PXAdapter adapter)
		{
			var current = Tasks.Current;
			if (current != null && current.Owner != null)
			{
				var employee = (EPEmployee)PXSelect<EPEmployee,
					Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.
					Select(this, current.Owner);
				if (employee != null)
					PXRedirectHelper.TryRedirect(this, employee, PXRedirectHelper.WindowMode.NewWindow);

				var user = (Users)PXSelect<Users,
					Where<Users.pKID, Equal<Required<Users.pKID>>>>.
					Select(this, current.Owner);
				if (user != null)
					PXRedirectHelper.TryRedirect(this, user, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		#endregion

		#region Event Handlers

		protected virtual void TaskFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as TaskFilter;
			if (row == null) return;

			var isCorrectOwner = row.OwnerID != null;
			if (!isCorrectOwner)
			{
				row.IsEscalated = false;
				row.IsFollowUp = false;
			}

			PXUIFieldAttribute.SetEnabled<TaskFilter.isEscalated>(sender, row, isCorrectOwner);
			PXUIFieldAttribute.SetEnabled<TaskFilter.isFollowUp>(sender, row, isCorrectOwner);
			var me = true.Equals(sender.GetValue(e.Row, typeof(TaskFilter.myOwner).Name));
			var myGroup = true.Equals(sender.GetValue(e.Row, typeof(TaskFilter.myWorkGroup).Name));

			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(TaskFilter.ownerID).Name, !me);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(TaskFilter.workGroupID).Name, !myGroup);
		}

		#endregion
	}
}
