using System.Collections;
using PX.Data;
using PX.Objects.CR;
using PX.SM;
using PX.TM;
using OwnedFilter = PX.Objects.CR.OwnedFilter;

namespace PX.Objects.EP
{
	[DashboardType((int)DashboardTypeAttribute.Type.Default, GL.TableAndChartDashboardTypeAttribute._AMCHARTS_DASHBOART_TYPE)]
	public class ActivitiesEnq : PXGraph<ActivitiesEnq>
	{
		#region Selects
		public PXFilter<CR.OwnedFilter> Filter;

		[PXViewName(Messages.Activities)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(CR.OwnedFilter))]
		public PXSelectReadonly3<EPActivity, 
			OrderBy<Desc<EPActivity.startDate>>>
			Activities;

		public PXSetup<EPSetup> epsetup; 
		#endregion

		#region Ctors

		public ActivitiesEnq()
		{
			PXUIFieldAttribute.SetDisplayName<EPActivity.startDate>(Activities.Cache, Messages.Date);
			PXUIFieldAttribute.SetDisplayName<EPActivity.endDate>(Activities.Cache, Messages.CompletedAt);

			var activityCache = Caches[typeof(EPActivity)];
			PXUIFieldAttribute.SetVisible(activityCache, null, false);
			PXUIFieldAttribute.SetVisible<EPActivity.subject>(activityCache, null);
			PXUIFieldAttribute.SetVisible<EPActivity.uistatus>(activityCache, null);
			PXUIFieldAttribute.SetVisible<EPActivity.startDate>(activityCache, null);
			PXUIFieldAttribute.SetVisible<EPActivity.timeSpent>(activityCache, null);
			PXUIFieldAttribute.SetVisible<EPActivity.overtimeSpent>(activityCache, null);
			PXUIFieldAttribute.SetVisible<EPActivity.source>(activityCache, null);

			var isPmVisible = PM.ProjectAttribute.IsPMVisible(this, GL.BatchModule.EP);
			PXUIFieldAttribute.SetVisible<EPActivity.projectID>(Activities.Cache, null, isPmVisible);
			PXUIFieldAttribute.SetVisible<EPActivity.projectTaskID>(Activities.Cache, null, isPmVisible);
		}

		#endregion

		#region Data Handlers

		protected virtual IEnumerable activities()
		{
			var command = Activities.View.BqlSelect;
			var filter = Filter.Current;

			if (filter.OwnerID != null)
				command = command.WhereOr(typeof(Where<EPActivity.owner, Equal<CurrentValue<OwnedFilter.ownerID>>>));
			if (filter.WorkGroupID != null)
				command = command.WhereAnd(typeof(Where<EPActivity.groupID, Equal<CurrentValue<OwnedFilter.workGroupID>>>));
			if (filter.MyWorkGroup == true)
				command = command.WhereAnd(typeof(Where<EPActivity.groupID, InMember<CurrentValue<OwnedFilter.currentOwnerID>>>));
			command = command.WhereAnd(typeof(Where<EPActivity.classID, Equal<CRActivityClass.activity>, 
										Or<EPActivity.classID, Equal<CRActivityClass.email>>>));

			var view = new PXView(this, true, command);
			var startRow = PXView.StartRow;
			int totalRows = 0;
			var list = view.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
								   ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;
			return list;
		}

		#endregion

		#region Actions

		public PXCancel<OwnedFilter> Cancel;

		public PXAction<OwnedFilter> createNew;
		[PXUIField(DisplayName = Messages.AddActivity)]
		[PXButton(Tooltip="Add New Activity", ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
		protected virtual IEnumerable CreateNew(PXAdapter adapter)
		{
			OwnedFilter filter = Filter.Current;
			EPSetup setup = epsetup.Current;
			Data.EP.ActivityService.CreateActivity(null, setup.DefaultActivityType);
			return adapter.Get();
		}

		public PXAction<OwnedFilter> ViewOwner;
		[PXUIField(DisplayName = Messages.ViewOwner, Visible = false)]
		[PXLookupButton(Tooltip = Messages.ttipViewOwner)]
		protected virtual IEnumerable viewOwner(PXAdapter adapter)
		{
			var current = Activities.Current;
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

		public PXAction<OwnedFilter> ViewEntity;
		[PXUIField(DisplayName = Messages.ViewEntity, Visible = false)]
		[PXLookupButton(Tooltip = Messages.ttipViewEntity)]
		protected virtual void viewEntity()
		{
			var row = Activities.Current;
			if (row == null) return;

			new EntityHelper(this).NavigateToRow(row.RefNoteID, PXRedirectHelper.WindowMode.NewWindow);
		}

		#endregion

		#region Event Handlers

		protected virtual void OwnedFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var me = true.Equals(sender.GetValue(e.Row, typeof(CR.OwnedFilter.myOwner).Name));
			var myGroup = true.Equals(sender.GetValue(e.Row, typeof(CR.OwnedFilter.myWorkGroup).Name));

			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(CR.OwnedFilter.ownerID).Name, !me);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(CR.OwnedFilter.workGroupID).Name, !myGroup);
		}

		#endregion
	}
}
