using System.Web.Compilation;
using System.Web.UI;
using PX.Web.Controls;
using PX.Web.UI;

public partial class PXTasksAndEventsNavPanelHostControl : UserControl
{
	private const string _TASKS_AND_EVENTS_REMINDER_GRAPH = "PX.Objects.EP.TasksAndEventsReminder";

	protected override void OnInit(System.EventArgs e)
	{
		if (BuildManager.GetType(_TASKS_AND_EVENTS_REMINDER_GRAPH, false) != null)
		{
			PXTasksAndEventsNavPanelHost pnlTasksAndEvents = new PXTasksAndEventsNavPanelHost();
			pnlTasksAndEvents.ID = "pnlTasksAndEvents"; 
			pnlTasksAndEvents.GraphType=_TASKS_AND_EVENTS_REMINDER_GRAPH;
			pnlTasksAndEvents.GetOpenTasksCountMethod = "GetOpenTasksCount";
			pnlTasksAndEvents.GetTodayTasksCountMethod="GetTodayTasksCount"; 
			pnlTasksAndEvents.GetNewTasksCountMethod="GetNewTasksCount";
			pnlTasksAndEvents.GetOpenEventsCountMethod = "GetOpenEventsCount";
			pnlTasksAndEvents.GetTodayEventsCountMethod = "GetTodayEventsCount";
			pnlTasksAndEvents.GetNewEventsCountMethod = "GetNewEventsCount";
			pnlTasksAndEvents.EventImage = Sprite.Main.GetFullUrl(Sprite.Main.Calendar);
			pnlTasksAndEvents.TaskImage = Sprite.Main.GetFullUrl(Sprite.Main.DataEntry);
			pnlTasksAndEvents.TasksLabel = PX.Data.PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.TasksAndEventsPanel.Tasks);
			pnlTasksAndEvents.EventsLabel = PX.Data.PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.TasksAndEventsPanel.Events);
			pnlTasksAndEvents.Target = "main";
			pnlTasksAndEvents.TasksUrl = "~/Pages/EP/EP404000.aspx";
			pnlTasksAndEvents.EventsUrl = "~/Pages/EP/EP404100.aspx";
			pnlTasksAndEvents.HostID = ID;
			mainDiv.Controls.Add(pnlTasksAndEvents);
		}
		base.OnInit(e);
	}
}
