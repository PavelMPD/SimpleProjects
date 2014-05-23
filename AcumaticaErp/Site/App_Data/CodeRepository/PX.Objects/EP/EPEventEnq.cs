using System;
using System.Collections;
using System.IO;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Export.Imc;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.SM;
using PX.TM;

namespace PX.Objects.EP
{
	[TableDashboardType]
	//[DashboardType(EPCalendarFilter.DASHBOARD_TYPE)]
	[EPCalendarDashboardGraph(typeof(EPCalendarEnq), "Filter", "Events")]
	public class EPEventEnq : PXGraph<EPEventEnq>
	{
		#region Selects
		public PXFilter<CR.OwnedFilter> Filter;

		[PXFilterable]
		[PXViewDetailsButton(typeof(CR.OwnedFilter))]
		public PXSelectReadonly3<EPActivity,
			OrderBy<Asc<EPActivity.startDate,
				Desc<EPActivity.priority>>>>
			Events;
		#endregion

		#region Actions

		public PXCancel<CR.OwnedFilter> Cancel;

		public PXAction<CR.OwnedFilter> CreateNew;
		[PXButton(ImageKey = Web.UI.Sprite.Main.AddNew, Tooltip = Messages.AddEvent)]
		[PXUIField(DisplayName = "New")]
		public virtual void createNew()
		{
			var graph = PXGraph.CreateInstance<EPEventMaint>();
			graph.Events.Insert();
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXAction<CR.OwnedFilter> ViewEntity;
		[PXUIField(DisplayName = Messages.ViewEntity, Visible = false)]
		[PXLookupButton(Tooltip = Messages.ttipViewEntity)]
		protected void viewEntity()
		{
			var row = Events.Current;
			if (row == null) return;

			new EntityHelper(this).NavigateToRow(row.RefNoteID, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXAction<CR.OwnedFilter> ViewOwner;
		[PXUIField(DisplayName = Messages.ViewOwner, Visible = false)]
		[PXLookupButton(Tooltip = Messages.ttipViewOwner)]
		protected virtual IEnumerable viewOwner(PXAdapter adapter)
		{
			var current = Events.Current;
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

		public PXAction<CR.OwnedFilter> CancelActivity;
		[PXUIField(DisplayName = Messages.CancelEvent)]
		[PXButton(Tooltip = Messages.CancelEvent)]
		protected void cancelActivity()
		{
			var row = Events.Current;
			if (row == null) return;

			var graph = PXGraph.CreateInstance<EPEventMaint>();
			graph.Events.Current = graph.Events.Search<EPActivity.taskID>(row.TaskID);
			graph.CancelActivity.PressButton();
		}

		public PXAction<CR.OwnedFilter> Complete;
		[PXUIField(DisplayName = Messages.CompleteEvent)]
		[PXButton(Tooltip = Messages.CompleteEvent)]
		protected void complete()
		{
			var row = Events.Current;
			if (row == null) return;

			var graph = PXGraph.CreateInstance<EPEventMaint>();
			graph.Events.Current = graph.Events.Search<EPActivity.taskID>(row.TaskID);
			graph.Complete.PressButton();
		}

		//TODO: need implementation
		/*public PXAction<CR.OwnedFilter> ExportCalendar;
		[PXUIField(DisplayName = Messages.ExportCalendar)]
		[PXButton(Tooltip = Messages.ExportCalendarTooltip)]
		public IEnumerable exportCalendar(PXAdapter adapter)
		{
			var events = Events.Select().RowCast<EPActivity>();
			var calendar = (vCalendarIcs)VCalendarProcessor.CreateVCalendar(events);
			throw new EPIcsExportRedirectException(calendar);
		}

		public PXAction<CR.OwnedFilter> ExportCard;
		[PXUIField(DisplayName = Messages.ExportCard)]
		[PXButton(Tooltip = Messages.ExportCardTooltip)]
		public IEnumerable exportCard(PXAdapter adapter)
		{
			if (Events.Current == null) return adapter.Get();
			var card = VCalendarProcessor.CreateVEvent(Events.Current);
			throw new EPIcsExportRedirectException(card);
		}*/

		#endregion

		#region Data Handlers
		protected virtual IEnumerable events()
		{
			BqlCommand command = new Select2<EPActivity,
				LeftJoin<EPAttendee, On<EPAttendee.userID, Equal<Current<AccessInfo.userID>>,
					And<EPAttendee.eventID, Equal<EPActivity.taskID>>>,
				LeftJoin<EPView,
					On<EPView.noteID, Equal<EPActivity.noteID>,
						And<EPView.userID, Equal<Current<AccessInfo.userID>>>>>>,
				Where<EPActivity.classID, Equal<CRActivityClass.events>>,
				OrderBy<Desc<EPActivity.priority, Asc<EPActivity.startDate, Asc<EPActivity.endDate>>>>>();

			var filter = Filter.Current;
			if (filter.OwnerID != null)
			{
				command = new Select2<EPActivity,
				LeftJoin<EPAttendee, On<EPAttendee.userID, Equal<CurrentValue<CR.OwnedFilter.ownerID>>,
					And<EPAttendee.eventID, Equal<EPActivity.taskID>>>,
				LeftJoin<EPView,
					On<EPView.noteID, Equal<EPActivity.noteID>,
						And<EPView.userID, Equal<Current<AccessInfo.userID>>>>>>,
				Where2<Where<EPActivity.createdByID, Equal<CurrentValue<CR.OwnedFilter.ownerID>>, Or<EPAttendee.userID, IsNotNull>>,
					And<Where<EPActivity.classID, Equal<CRActivityClass.events>>>>,
				OrderBy<Desc<EPActivity.priority, Asc<EPActivity.startDate, Asc<EPActivity.endDate>>>>>();
			}

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

		#region Event Handlers

		protected virtual void OwnedFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var me = true.Equals(sender.GetValue(e.Row, typeof(CR.OwnedFilter.myOwner).Name));
			var myGroup = true.Equals(sender.GetValue(e.Row, typeof(CR.OwnedFilter.myWorkGroup).Name));

			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(CR.OwnedFilter.ownerID).Name, !me);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(CR.OwnedFilter.workGroupID).Name, !myGroup);
		}

		#endregion

		#region public Methods

		public void Import(byte[] content, string fileExtension)
		{
			//TODO: need implementation
			/*vCalendar calendar = new vCalendar();
			using (TextReader reader = new StreamReader(new MemoryStream(content)))
			{
				calendar.Read(reader);
			}
			foreach (vEvent item in calendar.Events)
			{
				var inserted = Events.Insert(CreateEvent(item));
				if (inserted.ClassID == null) inserted.ClassID = 1;
			}
			Save.Press();*/
		}

		/*private static EPActivity CreateEvent(vEvent card)
		{
			EPActivity epEvent = new EPActivity();
			epEvent.Subject = card.Summary;
			epEvent.Body = card.Description;
			epEvent.StartDate = card.StartDate.ToLocalTime();
			epEvent.EndDate = card.EndDate.ToLocalTime();
			//card.Category = "meeting";//epEvent
			epEvent.Location = card.Location;
			return epEvent;
		}*/

		#endregion
	}
}
