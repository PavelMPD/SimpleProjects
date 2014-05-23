using System.Collections;
using PX.Common;
using PX.Data;
using PX.SM;
using PX.Objects.CR.DAC;
using PX.Objects.EP;

namespace PX.Objects.CR
{
	public class CRCommunicationOverview : PXGraph<CRCommunicationOverview>
	{
		#region Select
		public PXSelect<CommunacationSummary> Summary;
		[PXViewDetailsButton(typeof(CommunacationSummary))]
		public PXSelect<EPActivity> AllEmailsGrid;
		[PXViewDetailsButton(typeof(CommunacationSummary))]
		public PXSelect<EPActivity> AllTasksGrid;
		[PXViewDetailsButton(typeof(CommunacationSummary))]
		public PXSelect<EPActivity> AllEventsGrid;
		[PXViewDetailsButton(typeof(CommunacationSummary))]
		public PXSelect<CRAnnouncement> AllAnnouncementsGrid;
		[PXViewDetailsButton(typeof(CommunacationSummary))]
		public PXSelect<EPExpenseClaim> AllApprovalsGrid;
		public PXFilter<CRCommunicationAnnouncementPreview.CRAnnouncementDetails> AnnouncementsDetails;
		#endregion

		#region Delegates
		protected IEnumerable summary()
		{
			int AllEmailsCount = 0;
			int AlreadyViewEmailsCount = 0;
			
			int AllTasksCount = 0;
			int AlreadyViewTasksCount = 0;

			int AllEventsCount = 0;
			int AlreadyViewEventsCount = 0;

			int AllAnnouncementsCount = 0;
			int AlreadyViewAnnouncementsCount = 0;

			int AllApprovalsCount = 0;
			int AlreadyViewApprovalsCount = 0;

			PXResult AllEmails = PXSelectGroupBy<EPActivity,
				Where<EPActivity.classID, Equal<CRActivityClass.email>,
					And<EPActivity.owner, Equal<Current<AccessInfo.userID>>,
					And<EPActivity.isIncome, Equal<True>,
					And<EPActivity.mpstatus, NotEqual<MailStatusListAttribute.deleted>>>>>,
				Aggregate<Count<EPActivity.taskID>>>.Select(this);
			if (AllEmails != null && AllEmails.RowCount != null)
				AllEmailsCount = (int)AllEmails.RowCount;

			PXResult AlreadyViewEmails = PXSelectJoinGroupBy<EPActivity,
			InnerJoin<EPView, On<EPView.noteID, Equal<EPActivity.noteID>,
				And<EPView.status, Equal<EPViewStatusAttribute.Viewed>>>>,
			Where<EPActivity.classID, Equal<CRActivityClass.email>,
					And<EPActivity.owner, Equal<Current<AccessInfo.userID>>,
					And<EPView.userID, Equal<Current<AccessInfo.userID>>,
					And<EPActivity.isIncome, Equal<True>,
					And<EPActivity.mpstatus, NotEqual<MailStatusListAttribute.deleted>>>>>>,
				Aggregate<Count<EPActivity.taskID>>>.Select(this);
			if (AlreadyViewEmails != null && AlreadyViewEmails.RowCount != null)
				AlreadyViewEmailsCount = (int)AlreadyViewEmails.RowCount;
			
			PXResult AllTasks = PXSelectGroupBy<EPActivity,
			Where<EPActivity.classID, Equal<CRActivityClass.task>,
					And<EPActivity.owner, Equal<Current<AccessInfo.userID>>>>,
				Aggregate<Count<EPActivity.taskID>>>.Select(this);

			if (AllTasks != null && AllTasks.RowCount != null)
				AllTasksCount = (int)AllTasks.RowCount;

			PXResult AlreadyViewTasks = PXSelectJoinGroupBy<EPActivity,
			InnerJoin<EPView,On<EPView.noteID, Equal<EPActivity.noteID>,
				And<EPView.status, Equal<EPViewStatusAttribute.Viewed>>>>, 
			Where<EPActivity.classID, Equal<CRActivityClass.task>,
					And<EPActivity.owner, Equal<Current<AccessInfo.userID>>,
					And<EPView.userID, Equal<Current<AccessInfo.userID>>>>>,
				Aggregate<Count>>.Select(this);
			if (AlreadyViewTasks != null && AlreadyViewTasks.RowCount != null)
				AlreadyViewTasksCount = (int)AlreadyViewTasks.RowCount;

			PXResult AllEvents = PXSelectGroupBy<EPActivity,
			Where<EPActivity.classID, Equal<CRActivityClass.events>,
				And<EPActivity.owner, Equal<Current<AccessInfo.userID>>>>,
				Aggregate<Count<EPActivity.taskID>>>.Select(this);
			if (AllEvents != null && AllEvents.RowCount != null)
				AllEventsCount = (int)AllEvents.RowCount;

			PXResult AlreadyViewEvents = PXSelectJoinGroupBy<EPActivity,
			InnerJoin<EPView, On<EPView.noteID, Equal<EPActivity.noteID>,
				And<EPView.status, Equal<EPViewStatusAttribute.Viewed>>>>,
			Where<EPActivity.classID, Equal<CRActivityClass.events>,
					And<EPActivity.owner, Equal<Current<AccessInfo.userID>>,
					And<EPView.userID, Equal<Current<AccessInfo.userID>>>>>,
				Aggregate<Count<EPActivity.taskID>>>.Select(this);
			if (AlreadyViewEvents != null && AlreadyViewEvents.RowCount != null)
				AlreadyViewEventsCount = (int)AlreadyViewEvents.RowCount;

			PXResult AllAnnouncements = PXSelectGroupBy<CRAnnouncement,
				Aggregate<Count<CRAnnouncement.announcementsID>>>.Select(this);
			if (AllAnnouncements != null && AllAnnouncements.RowCount != null)
				AllAnnouncementsCount = (int)AllAnnouncements.RowCount;

			PXResult AlreadyViewAnnouncements = PXSelectJoinGroupBy<CRAnnouncement,
			InnerJoin<EPView, On<EPView.noteID, Equal<CRAnnouncement.noteID>>>,
				Aggregate<Count<CRAnnouncement.announcementsID>>>.Select(this);
			if (AlreadyViewAnnouncements != null && AlreadyViewAnnouncements.RowCount != null)
				AlreadyViewAnnouncementsCount = (int)AlreadyViewAnnouncements.RowCount;

			PXResult AllApprovals = PXSelectGroupBy<EPExpenseClaim,
				Where<EPExpenseClaim.approverID, Equal<Current<AccessInfo.userID>>>,
				Aggregate<Count<EPExpenseClaim.approverID>>>.Select(this);
			if (AllApprovals != null && AllApprovals.RowCount != null)
				AllApprovalsCount = (int)AllApprovals.RowCount;

			PXResult AlreadyViewApprovals = PXSelectJoinGroupBy<EPExpenseClaim,
			InnerJoin<EPView, On<EPView.noteID, Equal<EPExpenseClaim.noteID>>>,
				Aggregate<Count<EPExpenseClaim.approverID>>>.Select(this);
			if (AlreadyViewApprovals != null && AlreadyViewApprovals.RowCount != null)
				AlreadyViewApprovalsCount = (int)AlreadyViewApprovals.RowCount;


			CommunacationSummary currentSummary = new CommunacationSummary();
			currentSummary.Emails = AllEmailsCount.ToString() + " (" + (AllEmailsCount - AlreadyViewEmailsCount).ToString() + ")";
			currentSummary.Tasks = AllTasksCount.ToString() + " (" + (AllTasksCount - AlreadyViewTasksCount).ToString() + ")";
			currentSummary.Events = AllEventsCount.ToString() + " (" + (AllEventsCount - AlreadyViewEventsCount).ToString() + ")";
			currentSummary.Announcements = AllAnnouncementsCount.ToString() + " (" + (AllAnnouncementsCount - AlreadyViewAnnouncementsCount).ToString() + ")";
			currentSummary.Approvals = AllApprovalsCount.ToString() + " (" + (AllApprovalsCount - AlreadyViewApprovalsCount).ToString() + ")";
			
			yield return currentSummary;
		}

		protected IEnumerable alltasksgrid()
		{
			return PXSelectJoin<EPActivity,
					LeftJoin<EPView, On<EPView.noteID, Equal<EPActivity.noteID>,
						And<EPView.status, Equal<EPViewStatusAttribute.Viewed>,
						And<EPView.userID, Equal<Current<AccessInfo.userID>>>>>>,
					Where<EPActivity.classID, Equal<CRActivityClass.task>,
						And<EPActivity.owner, Equal<Current<AccessInfo.userID>>>>,
					OrderBy<Asc<EPView.userID, Desc<EPActivity.createdDateTime>>>>.SelectWindowed(this, 0, 5);
		}

		protected IEnumerable alleventsgrid()
		{
			return PXSelectJoin<EPActivity,
					LeftJoin<EPView, On<EPView.noteID, Equal<EPActivity.noteID>,
						And<EPView.status, Equal<EPViewStatusAttribute.Viewed>,
						And<EPView.userID, Equal<Current<AccessInfo.userID>>>>>>,
					Where<EPActivity.classID, Equal<CRActivityClass.events>,
						And<EPActivity.owner, Equal<Current<AccessInfo.userID>>>>,
						OrderBy<Asc<EPView.userID, Desc<EPActivity.createdDateTime>>>>.SelectWindowed(this, 0, 5);
			
		}

		protected IEnumerable allannouncementsgrid()
		{
			return PXSelectJoinOrderBy<CRAnnouncement,
				LeftJoin<EPView, On<EPView.noteID, Equal<CRAnnouncement.noteID>,
					And<EPView.status, Equal<EPViewStatusAttribute.Viewed>,
						And<EPView.userID, Equal<Current<AccessInfo.userID>>>>>>,
				OrderBy<Asc<EPView.userID, Desc<EPActivity.createdDateTime>>>>.SelectWindowed(this, 0, 5);
		}

		protected IEnumerable allemailsgrid()
		{
			return PXSelectJoin<EPActivity,
				LeftJoin<EPView, On<EPView.noteID, Equal<EPActivity.noteID>,
					And<EPView.status, Equal<EPViewStatusAttribute.Viewed>,
						And<EPView.userID, Equal<Current<AccessInfo.userID>>>>>>,
				Where<EPActivity.classID, Equal<CRActivityClass.email>,
					And<EPActivity.owner, Equal<Current<AccessInfo.userID>>,
					And<EPActivity.isIncome, Equal<True>,
					And<EPActivity.mpstatus, NotEqual<MailStatusListAttribute.deleted>>>>>,
				OrderBy<Asc<EPView.userID, Desc<EPActivity.createdDateTime>>>>.SelectWindowed(this, 0, 5);
		}

		protected IEnumerable allapprovalsgrid()
		{
			return PXSelectJoin<EPExpenseClaim,
				LeftJoin<EPView, On<EPView.noteID, Equal<EPExpenseClaim.noteID>,
					And<EPView.status, Equal<EPViewStatusAttribute.Viewed>,
						And<EPView.userID, Equal<Current<AccessInfo.userID>>>>>>,
				Where<EPExpenseClaim.approverID, Equal<Current<AccessInfo.userID>>>,
					OrderBy<Asc<EPView.userID, Desc<EPExpenseClaim.createdDateTime>>>>.SelectWindowed(this, 0, 5);
		}
		#endregion

		#region Actions
		public PXAction<CommunacationSummary> GetMoreEmails;
		[PXUIField(DisplayName = Messages.GetMore)]
		[PXLookupButton(Tooltip = Messages.GetMore, ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
		protected virtual void getMoreEmails()
		{
			var epEmailGraph = PXGraph.CreateInstance<CRCommunicationInbox>();
			PXRedirectHelper.TryRedirect(epEmailGraph, PXRedirectHelper.WindowMode.Same);
		}

		public PXAction<CommunacationSummary> GetMoreTasks;
		[PXUIField(DisplayName = Messages.GetMore)]
		[PXLookupButton(Tooltip = Messages.GetMore, ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
		protected virtual void getMoreTasks()
		{
			var epTaskGraph = PXGraph.CreateInstance<CRTaskMaint>();
			PXRedirectHelper.TryRedirect(epTaskGraph, PXRedirectHelper.WindowMode.Same);
		}

		public PXAction<CommunacationSummary> GetMoreEvents;
		[PXUIField(DisplayName = Messages.GetMore)]
		[PXLookupButton(Tooltip = Messages.GetMore, ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
		protected virtual void getMoreEvents()
		{
			var epEventGraph = PXGraph.CreateInstance<EPEventMaint>();
			PXRedirectHelper.TryRedirect(epEventGraph, PXRedirectHelper.WindowMode.Same);
		}

		public PXAction<CommunacationSummary> GetMoreAnnouncements;
		[PXUIField(DisplayName = Messages.GetMore, Visible = false)]
		[PXLookupButton(Tooltip = Messages.GetMore, ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
		protected virtual void getMoreAnnouncements()
		{
			var epAnnouncementGraph = PXGraph.CreateInstance<CRCommunicationAnnouncement>();
			PXRedirectHelper.TryRedirect(epAnnouncementGraph, PXRedirectHelper.WindowMode.Same);
		}

		public PXAction<CommunacationSummary> AnnouncementsViewDetails;
		[PXUIField(Visible = false)]
		[PXLookupButton()]
		protected virtual void announcementsViewDetails()
		{
			AnnouncementsDetails.Current.Body = "<html><head></head><body>";
			AnnouncementsDetails.Current.Body = AnnouncementsDetails.Current.Body + "<font size=\"4\">";
			AnnouncementsDetails.Current.Body = AnnouncementsDetails.Current.Body + AllAnnouncementsGrid.Current.Subject;
			AnnouncementsDetails.Current.Body = AnnouncementsDetails.Current.Body + "</font>";
			AnnouncementsDetails.Current.Body = AnnouncementsDetails.Current.Body + "<br/><br/>" + Tools.RemoveHeader(AllAnnouncementsGrid.Current.Body);
			AnnouncementsDetails.Current.Body = AnnouncementsDetails.Current.Body + "</body></html>";
			
			if (AnnouncementsDetails.AskExt() == WebDialogResult.OK)
			{
			}
		}

		public PXAction<CommunacationSummary> GetMoreApprovals;
		[PXUIField(DisplayName = Messages.GetMore)]
		[PXLookupButton(Tooltip = Messages.GetMore, ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
		protected virtual void getMoreApprovals()
		{            
            var epApprovalsGraph = PXGraph.CreateInstance<EPApprovalProcess>();
            PXRedirectHelper.TryRedirect(epApprovalsGraph, PXRedirectHelper.WindowMode.Same);
		}
		#endregion
	}
}
