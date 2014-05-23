using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Compilation;
using PX.Common;
using PX.Common.Mail;
using PX.Data;
using PX.Data.EP;
using PX.Data.Wiki.Parser;
using PX.Objects.EP;
using PX.SM;
using PX.TM;
using FileInfo = PX.SM.FileInfo;

namespace PX.Objects.CR
{
	public class CREmailActivityMaint : CRBaseActivityMaint<CREmailActivityMaint>
	{

        public class TemplateSourceType
        {
            public class ListAttribute : PXStringListAttribute
            {
                public ListAttribute()
                    : base(
                        new string[]{Notificataion, Activity, KnowledgeBase},
                        new string[]{Messages.EmailNotificationTemplate, Messages.EmailActivityTemplate, Messages.KnowledgeBaseArticle}
                    ){}
            }

            public const string Notificataion = "NO";
            public const string Activity = "AC";
            public const string KnowledgeBase = "KB";

            public class notification : Constant<string>
            {
                public notification() : base(Notificataion) {}
            }
            public class activity : Constant<string>
            {
                public activity() : base(Activity) {}
            }
        }

        #region NotificatonFilter
        [Serializable]
        public partial class NotificationFilter: IBqlTable
        {

            #region Type

            [PXDBString(2, IsFixed = true)]
            [PXDefault(TemplateSourceType.Notificataion)]
            [TemplateSourceType.List]
            [PXUIField(DisplayName = "Source Type", Visibility = PXUIVisibility.Visible, Enabled = true)]
            public virtual string Type { get; set; }
            #endregion

            #region NotificationName
            public abstract class notificationName : IBqlField {}

            [PXSelector(typeof(Notification.name), DescriptionField = typeof(Notification.name))]
            [PXDBString(255, InputMask = "", IsUnicode = true)]
            [PXUIField(DisplayName = "Template")]
            public virtual string NotificationName { get; set; }
            #endregion

            #region TemplateActivity
            public abstract class templateActivity : IBqlField {}

            [PXDBInt]
            [PXUIField(DisplayName = "Template", Visibility = PXUIVisibility.SelectorVisible)]
            [PXSelector(typeof(Search<EPActivity.taskID, 
                Where<EPActivity.refNoteID, Equal<Current<EPActivity.refNoteID>>>>),
                typeof(EPActivity.subject),
                typeof(EPActivity.type),
                SubstituteKey = typeof(EPActivity.subject))]
            public virtual int? TemplateActivity { get; set; }
            #endregion

            #region PageID
            public abstract class pageID : PX.Data.IBqlField {}

            protected Guid? _PageID;
            [PXDBGuid]
            [PXUIField(DisplayName = "Template", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual Guid? PageID { get; set; }
            #endregion

            #region AppendText
            public abstract class appendText : IBqlField { }

            [PXDBBool]
            [PXUIField(DisplayName = "Append", Visibility = PXUIVisibility.SelectorVisible)]
            [PXDefault(true)]
            public virtual bool? AppendText { get; set; }
            #endregion
        }
        #endregion

        #region SendEmailParams
        public class SendEmailParams
		{
			private readonly IList<FileInfo> _attachments;

			public SendEmailParams()
			{
				_attachments = new List<FileInfo>();
			}

			public IList<FileInfo> Attachments
			{
				get { return _attachments; }
			}

			public string From { get; set; }

			public string To { get; set; }

			public string Cc { get; set; }

			public string Bcc { get; set; }

			public string Subject { get; set; }

			public string Body { get; set; }

			public object Source { get; set; }

			public object ParentSource { get; set; }

			public string TemplateID { get; set; }
		}
		#endregion

		#region Selects

		//TODO: need review
		public PXSelect<EPActivity,
			Where<EPActivity.classID, Equal<CRActivityClass.email>, 
				Or<EPActivity.classID, Equal<CRActivityClass.emailRouting>>>>
			Message;

		[PXRefNoteSelectorAttribute(typeof(EPActivity), typeof(EPActivity.refNoteID))]
		public PXSelect<EPActivity,
			Where<EPActivity.taskID, Equal<Current<EPActivity.taskID>>>>
			Activites;

		[PXHidden]
		public PXSetup<CRSetup>
			crSetup;

        [PXHidden]
        public PXSelect<CT.Contract>
            BaseContract;

		public PXSelect<Notification> Notification;

	    public PX.Objects.SM.SPWikiCategoryMaint.PXSelectWikiFoldersTree Folders;

        [PXViewName("Notification Template")]
        public PXFilter<NotificationFilter> NotificationInfo;

		#endregion

		#region Ctors

		public CREmailActivityMaint()
		{
			FieldVerifying.AddHandler(typeof(UploadFile), typeof(UploadFile.name).Name, UploadFileNameFieldVerifying);

			CRCaseActivityHelper.Attach(this);

			var relEntityView = new PXView(this, true, new Select<EPActivity>(), new PXSelectDelegate(GetRelatedEntity));
			Views.Add("RelatedEntity", relEntityView);

			PXEntityInfoAttribute.SetDescriptionDisplayName<EPActivity.source>(Activites.Cache, "Related Entity");
			this.Action.AddMenuAction(LoadEmailSource);
			this.Action.AddMenuAction(process);
			this.Action.AddMenuAction(CancelSending);
			this.Action.AddMenuAction(DownloadEmlFile);

		}

		#endregion

		#region Actions

		public PXAction<EPActivity> Delete;
		[PXUIField(DisplayName = Messages.Delete, MapEnableRights = PXCacheRights.Update)]
		[PXDeleteButton]
		protected virtual IEnumerable delete(PXAdapter adapter)
		{			
			foreach (EPActivity record in adapter.Get())
			{
				var newMessage = (EPActivity)Message.Cache.CreateCopy(record);
				TryCorrectMailDisplayNames(newMessage);
				if (newMessage.MPStatus != MailStatusListAttribute.Deleted)
				{
					newMessage.MPStatus = MailStatusListAttribute.Deleted;
					newMessage = Message.Update(newMessage);
				}
				else
					newMessage = Message.Delete(newMessage);
				Actions.PressSave();
				yield return newMessage;				
			}
		}
		
		public PXAction<EPActivity> Send;
		[PXUIField(DisplayName = Messages.Send, MapEnableRights = PXCacheRights.Select)]
		[PXSendMailButton]
		protected virtual IEnumerable send(PXAdapter adapter)
		{
			var message = Message.Current;
			if (message == null) return new EPActivity[0];

			var res = new[] { message };
			if (message.MPStatus != ActivityStatusListAttribute.Draft &&
					message.MPStatus != MailStatusListAttribute.Failed)
			{
				return res;
			}

			if (!VerifyEmailFields(message)) return res;


			var newMessage = (EPActivity)Message.Cache.CreateCopy(message);
			TryCorrectMailDisplayNames(newMessage);
			newMessage.MPStatus = MailStatusListAttribute.PreProcess;
			newMessage = (EPActivity)Message.Cache.Update(newMessage);
			Actions.PressSave();	

			return new[] { newMessage };
		}

		public PXAction<EPActivity> Forward;
		[PXUIField(DisplayName = Messages.Forward, MapEnableRights = PXCacheRights.Select)]
		[PXForwardMailButton]
		protected void forward()
		{
			var oldMessage = Message.Current;

			var message = (EPActivity)Message.Cache.CreateCopy(oldMessage);

			message.TaskID = null;
			message.MPStatus = ActivityStatusListAttribute.Draft;
			Message.Cache.SetValueExt<EPActivity.startDate>(message, PXTimeZoneInfo.Now);
			message.CompletedDateTime = null;
			message.ParentTaskID = oldMessage.TaskID;
			message.Exception = null;
			message.MessageId = "<" + Guid.NewGuid() + "_acumatica>";
			message.NoteID = null;
			message.IsIncome = false;
			message.MailTo = null;
			message.MailCc = null;
			message.MailBcc = null;
			message.Subject = GetSubjectPrefix(oldMessage.Subject, true);
			message.Body = CreateReplyBody(oldMessage.MailFrom, oldMessage.MailTo, oldMessage.Subject,
				oldMessage.Body, (DateTime)oldMessage.LastModifiedDateTime);
			message.Owner = null;

			var targetGraph = PXGraph.CreateInstance<CREmailActivityMaint>();
			message = (EPActivity)targetGraph.Message.Cache.Insert(message);
			message.NoteID = PXNoteAttribute.GetNoteID<EPActivity.noteID>(targetGraph.Message.Cache, message);

			CopyAttachments(targetGraph, oldMessage, message);

			throw new PXRedirectRequiredException(targetGraph, true, "Forward Message") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		public PXAction<EPActivity> Reply;
		[PXUIField(DisplayName = Messages.Reply, MapEnableRights = PXCacheRights.Select)]
		[PXReplyMailButton]
		protected void reply()
		{
			var oldMessage = Message.Current;

			var message = (EPActivity)Message.Cache.CreateCopy(oldMessage);

			message.TaskID = null;
			message.MPStatus = ActivityStatusListAttribute.Draft;
			message.StartDate = PXTimeZoneInfo.Now;
			message.ParentTaskID = oldMessage.TaskID;
			message.Exception = null;
			message.MessageId = "<" + Guid.NewGuid() + "_acumatica>";
			message.NoteID = null;
			message.IsIncome = false;
			message.MailTo = GetReplyAddress(oldMessage);
			message.MailCc = null;
			message.MailBcc = null;
			message.Subject = GetSubjectPrefix(oldMessage.Subject, false);
			message.Body = CreateReplyBody(oldMessage.MailFrom, oldMessage.MailTo, oldMessage.Subject,
										   oldMessage.Body, (DateTime)oldMessage.LastModifiedDateTime);
			message.Owner = null;
			message.IsPrivate = oldMessage.IsPrivate;

			message = (EPActivity)Message.Cache.Insert(message);			

			PXRedirectHelper.TryRedirect(this, message, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXAction<EPActivity> ReplyAll;
		[PXUIField(DisplayName = Messages.ReplyAll, MapEnableRights = PXCacheRights.Select)]
		[PXReplyMailButton]
		protected void replyAll()
		{
			var oldMessage = Message.Current;

			var message = (EPActivity)Message.Cache.CreateCopy(oldMessage);

			message.TaskID = null;
			message.MPStatus = ActivityStatusListAttribute.Draft;
			message.ParentTaskID = oldMessage.TaskID;
			message.Exception = null;
			message.MessageId = "<" + Guid.NewGuid() + "_acumatica>";
			message.NoteID = null;
			message.IsIncome = false;
			var mailAccountAddress = GetMailAccountAddress(message);
			message.MailTo = GetReplyAllAddress(oldMessage, mailAccountAddress);
			message.MailCc = GetReplyAllCCAddress(oldMessage, mailAccountAddress);
			message.MailBcc = GetReplyAllBCCAddress(oldMessage, mailAccountAddress);
			message.Subject = GetSubjectPrefix(oldMessage.Subject, false);
			message.Body = CreateReplyBody(oldMessage.MailFrom, oldMessage.MailTo, oldMessage.Subject,
										   oldMessage.Body, (DateTime)oldMessage.LastModifiedDateTime);
			message.Owner = null;
			message.IsPrivate = oldMessage.IsPrivate;

			message = (EPActivity)Message.Cache.Insert(message);

			PXRedirectHelper.TryRedirect(this, message, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXAction<EPActivity> process;
		[PXUIField(DisplayName = "Process", MapEnableRights = PXCacheRights.Select)]
		[PXButton(ImageUrl = "PX.Web.UI.Images.Data.pinionStart.gif", DisabledImageUrl = "PX.Web.UI.Images.Data.pinionStartD.gif")]
		protected void Process()
		{
			ProcessMessage(Message.Current);
		}

		public static void ProcessMessage(EPActivity message)
		{
			if (MailAccountManager.IsMailProcessingOff) throw new PXException(EP.Messages.MailProcessingIsTurnedOff);

			if (message != null &&
				(message.MPStatus == MailStatusListAttribute.PreProcess ||
				message.MPStatus == MailStatusListAttribute.Failed))
			{
				if (message.IsIncome == true)
					EMailMessageReceiver.ProcessMessage(message);
				else
					MailSendProvider.SendMessage(message);
				if(!string.IsNullOrEmpty(message.Exception))
					throw new PXException(message.Exception);
			}
		}

		public PXAction<EPActivity> CancelSending;
		[PXUIField(DisplayName = EP.Messages.CancelSending, MapEnableRights = PXCacheRights.Select)]
		[PXButton(ImageUrl = "~/Icons/Cancel_Active.gif",
			DisabledImageUrl = "~/Icons/Cancel_NotActive.gif",
			Tooltip = EP.Messages.CancelSendingTooltip)]
		public virtual void cancelSending()
		{
			var message = Message.Current;
			if (message != null && message.MPStatus == MailStatusListAttribute.PreProcess)
			{
				var newMessage = (EPActivity)Message.Cache.CreateCopy(message);
				newMessage.MPStatus = ActivityStatusAttribute.Draft;
				Message.Cache.Update(newMessage);
				Actions.PressSave();
			}
		}

		public PXAction<EPActivity> DownloadEmlFile;
		[PXUIField(DisplayName = EP.Messages.DownloadEmlFile)]
		[PXButton(ImageUrl = "~/Icons/Eml_Active.gif",
			DisabledImageUrl = "~/Icons/Eml_NotActive.gif",
			Tooltip = EP.Messages.DownloadEmlFileTooltip)]
		public virtual void downloadEmlFile()
		{
			var message = Message.Current;
			if (message != null && message.IsIncome == true)
			{
				var mail = EMailMessageReceiver.GetOriginalMail(message);
				throw PXExportHandlerEml.GenerateException(mail);
			}
		}

	    public PXAction<EPActivity> LoadEmailSource;
        [PXUIField(DisplayName = "Select Source", MapEnableRights = PXCacheRights.Select)]
        [PXButton(Tooltip = "Select Template")]
        public virtual void loadEmailSource()
        {
            WebDialogResult res = NotificationInfo.AskExt();
            if (res == WebDialogResult.OK)
            {
                if (NotificationInfo.Current.Type == TemplateSourceType.Notificataion)
                {
                    Notification notification = PXSelect<Notification,
                        Where<Notification.name, Equal<Required<Notification.name>>>>.
                        Select(this, NotificationInfo.Current.NotificationName);
                    if (notification == null) return;

                    Message.Current.MailAccountID = notification.NFrom ?? Message.Current.MailAccountID;
                    Message.Current.MailTo = notification.NTo ?? Message.Current.MailTo;
                    Message.Current.MailCc = notification.NCc ?? Message.Current.MailCc;
                    Message.Current.MailBcc = notification.NBcc ?? Message.Current.MailBcc;
                    Message.Current.Subject = notification.Subject ?? Message.Current.Subject;

                    if (!notification.Body.Equals(String.Empty))
                    {
                        if ((bool) NotificationInfo.Current.AppendText)
                        {
							Activites.Current.Body += notification.Body; 
                        }
                        else
                        {
							Activites.Current.Body = notification.Body;
                        }
                    } 
                }
                if (NotificationInfo.Current.Type == TemplateSourceType.Activity)
                {
                    EPActivity activity = PXSelect<EPActivity,
                        Where<EPActivity.taskID, Equal<Required<EPActivity.taskID>>>>.
                        Select(this, NotificationInfo.Current.TemplateActivity);
                    if (activity == null) return;

					if ((bool)NotificationInfo.Current.AppendText)
                    {
						Activites.Current.Body += activity.Body; 
                    }
                    else
                    {
						Activites.Current.Body = activity.Body;
                    }
                }
                if (NotificationInfo.Current.Type == TemplateSourceType.KnowledgeBase)
                {
                    PXResult<WikiPage, WikiPageLanguage> result = (PXResult<WikiPage, WikiPageLanguage>)PXSelectJoin<WikiPage,
                                        InnerJoin<WikiPageLanguage, On<WikiPageLanguage.pageID, Equal<WikiPage.pageID>>,
                                        InnerJoin<WikiRevision, On<WikiRevision.pageID, Equal<WikiPage.pageID>>>>,
                                        Where<WikiPage.pageID, Equal<Required<WikiPage.pageID>>>,
                                        OrderBy<Desc<WikiRevision.pageRevisionID>>>.SelectSingleBound(new PXGraph(), null, NotificationInfo.Current.PageID);

                    if ( result == null ) return;

                    WikiRevision wr = result[typeof(WikiRevision)] as WikiRevision;

                    if ( wr == null ) return;

					if ((bool)NotificationInfo.Current.AppendText)
                    {
						Activites.Current.Body += PXWikiParser.Parse(wr.Content); 
                    }
                    else
                    {
						Activites.Current.Body = PXWikiParser.Parse(wr.Content);
                    }
                }
            }
        }

		public PXMenuAction<EPActivity> Action;

		#endregion

		#region Data Handlers

		public IEnumerable GetRelatedEntity()
		{
			var current = Message.Current;
			if (current != null && current.RefNoteID != null)
			{
				var row = new EntityHelper(this).GetEntityRow(current.RefNoteID);
				if (row != null) yield return row;
			}
		}

		public override void Persist()
		{
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				base.Persist();

				CorrectFileNames();

				ts.Complete();
			}
		}

		#endregion

		#region Event Handlers


		[PXUIField(DisplayName = "Parent")]
		[PXDBInt]
		[PXSelector(typeof(Search<EPActivity.taskID,
			Where<EPActivity.taskID, NotEqual<Current<EPActivity.taskID>>,
				And<Where<EPActivity.classID, Equal<CRActivityClass.task>,
						Or<EPActivity.classID, Equal<CRActivityClass.events>,
						Or<EPActivity.classID, Equal<CRActivityClass.email>>>>>>,
			OrderBy<Desc<EPActivity.taskID>>>),
			typeof(EPActivity.taskID), typeof(EPActivity.subject), typeof(EPActivity.classInfo), typeof(EPActivity.uistatus),
			DescriptionField = typeof(EPActivity.subject))]
		protected virtual void EPActivity_ParentTaskID_CacheAttached(PXCache cache)
		{

		}

		[PXEMailAccountIDSelector(true)]
		[PXDBInt]
		[PXUIField(DisplayName = "From")]
		protected void EPActivity_MailAccountID_CacheAttached(PXCache sender)
		{

		}

		[CREmailSelector]
		[PXDBString(1000, IsUnicode = true)]
		[PXUIField(DisplayName = "To")]
		protected void EPActivity_MailTo_CacheAttached(PXCache sender)
		{

		}

		[CREmailSelector(true)]
		[PXDBString(1000, IsUnicode = true)]
		[PXUIField(DisplayName = "CC")]
		protected void EPActivity_MailCc_CacheAttached(PXCache sender)
		{

		}

		[CREmailSelector(true)]
		[PXDBString(1000, IsUnicode = true)]
		[PXUIField(DisplayName = "BCC")]
		protected void EPActivity_MailBcc_CacheAttached(PXCache sender)
		{

		}

		[PXDefault(MailStatusListAttribute.Draft)]
		[PXDBString(2)]
		[MailStatusList]
		[PXUIField(DisplayName = "Mail Status", Enabled = false)]
		protected virtual void EPActivity_MPStatus_CacheAttached(PXCache cache)
		{
		}

		[PXDBString(255, InputMask = "", IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Subject", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXNavigateSelector(typeof(EPActivity.subject))]
		protected virtual void EPActivity_Subject_CacheAttached(PXCache sender)
		{
		}

		protected virtual void NotificationFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			NotificationFilter row = (NotificationFilter)e.Row;
			if (row == null) return;

			bool isNotification = row.Type.Equals(TemplateSourceType.Notificataion);
			bool isKBArticle = row.Type.Equals(TemplateSourceType.KnowledgeBase);
			bool isActivity = row.Type.Equals(TemplateSourceType.Activity);
			PXUIFieldAttribute.SetVisible<NotificationFilter.templateActivity>(cache, row, isActivity);
			PXUIFieldAttribute.SetVisible<NotificationFilter.notificationName>(cache, row, isNotification);
			PXUIFieldAttribute.SetVisible<NotificationFilter.pageID>(cache, row, isKBArticle);
		}

		protected virtual void EPActivity_Body_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			var signature = GetSignature();
			if (!string.IsNullOrEmpty(signature))
				e.NewValue = Tools.AppendToHtmlBody(e.NewValue as string, "<br />" + signature);
		}

		protected virtual void EPActivity_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var row = (EPActivity)e.Row;
			if (row == null) return;

			var isPmVisible = PM.ProjectAttribute.IsPMVisible(this, GL.BatchModule.EP);
			PXUIFieldAttribute.SetVisible<EPActivity.projectID>(cache, row, isPmVisible);
			PXUIFieldAttribute.SetVisible<EPActivity.projectTaskID>(cache, row, isPmVisible);
			var wasUsed = !string.IsNullOrEmpty(row.TimeCardCD) || row.Billed == true;
			if (wasUsed) PXUIFieldAttribute.SetEnabled(cache, row, false);

			PXUIFieldAttribute.SetVisible<EPActivity.trackTime>(cache, row, row.IsIncome != true);
			PXUIFieldAttribute.SetVisible<EPActivity.timeSpent>(cache, row, row.TrackTime == true);
			PXUIFieldAttribute.SetVisible<EPActivity.earningTypeID>(cache, row, row.TrackTime == true);
			PXUIFieldAttribute.SetVisible<EPActivity.isBillable>(cache, row, row.TrackTime == true);
			PXUIFieldAttribute.SetVisible<EPActivity.released>(cache, row, row.TrackTime == true);
			PXUIFieldAttribute.SetVisible<EPActivity.timeBillable>(cache, row, row.IsBillable == true && row.TrackTime == true);
			PXUIFieldAttribute.SetVisible<EPActivity.overtimeBillable>(cache, row, row.IsBillable == true && row.TrackTime == true);
			PXUIFieldAttribute.SetVisible<EPActivity.uistatus>(cache, row, row.TrackTime == true);
			PXUIFieldAttribute.SetVisible<EPActivity.approverID>(cache, row, row.TrackTime == true);
			PXUIFieldAttribute.SetVisible<EPActivity.overtimeSpent>(cache, row, false);
			PXUIFieldAttribute.SetVisible<EPActivity.overtimeBillable>(cache, row, false);
			PXUIFieldAttribute.SetVisible<EPActivity.projectID>(cache, row, row.TrackTime == true);
			PXUIFieldAttribute.SetVisible<EPActivity.projectTaskID>(cache, row, row.TrackTime == true);

			var showMinutes = EPSetupCurrent.RequireTimes == true;
			PXDBDateAndTimeAttribute.SetTimeVisible<EPActivity.startDate>(cache, row, showMinutes && row.TrackTime == true);
			PXDBDateAndTimeAttribute.SetTimeVisible<EPActivity.endDate>(cache, row, showMinutes && row.TrackTime == true);			

			string origStatus =
				(string)this.Activites.Cache.GetValueOriginal<EPActivity.uistatus>(row) ?? ActivityStatusListAttribute.Open;
			bool? oringTrackTime =
				(bool?)this.Activites.Cache.GetValueOriginal<EPActivity.trackTime>(row) ?? false;

			if (origStatus == ActivityStatusAttribute.Completed && oringTrackTime != true)
				origStatus = ActivityStatusAttribute.Open;

			if (row.Released == true)
				origStatus = ActivityStatusAttribute.Completed;
			
			PXUIFieldAttribute.SetEnabled(cache, row, row.MPStatus == MailStatusListAttribute.Draft || row.MPStatus == MailStatusListAttribute.Failed);							
			Delete.SetEnabled(!wasUsed && row.Released != true && row.MPStatus != MailStatusListAttribute.InProcess && row.MPStatus != MailStatusListAttribute.Processed);								
						
			if (origStatus == ActivityStatusListAttribute.Open)
			{
				PXUIFieldAttribute.SetEnabled<EPActivity.isExternal>(cache, row, true);
				PXUIFieldAttribute.SetEnabled<EPActivity.parentTaskID>(cache, row, true);
				PXUIFieldAttribute.SetEnabled<EPActivity.projectID>(cache, row, true);
				PXUIFieldAttribute.SetEnabled<EPActivity.projectTaskID>(cache, row, true);
				PXUIFieldAttribute.SetEnabled<EPActivity.weekID>(cache, row, true);
				PXUIFieldAttribute.SetEnabled<EPActivity.trackTime>(cache, row, true);
				PXUIFieldAttribute.SetEnabled<EPActivity.timeSpent>(cache, row, true);
				PXUIFieldAttribute.SetEnabled<EPActivity.isBillable>(cache, row, true);
				PXUIFieldAttribute.SetEnabled<EPActivity.earningTypeID>(cache, row, true);
				PXUIFieldAttribute.SetEnabled<EPActivity.timeBillable>(cache, row, !wasUsed && row.IsBillable == true);
				PXUIFieldAttribute.SetEnabled<EPActivity.overtimeBillable>(cache, row, !wasUsed && row.IsBillable == true);
				Delete.SetEnabled(!wasUsed && row.Released != true);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled<EPActivity.isExternal>(cache, row, false);
				PXUIFieldAttribute.SetEnabled<EPActivity.parentTaskID>(cache, row, false);
				PXUIFieldAttribute.SetEnabled<EPActivity.projectID>(cache, row, false);
				PXUIFieldAttribute.SetEnabled<EPActivity.projectTaskID>(cache, row, false);
				PXUIFieldAttribute.SetEnabled<EPActivity.weekID>(cache, row, false);
				PXUIFieldAttribute.SetEnabled<EPActivity.trackTime>(cache, row, false);
				PXUIFieldAttribute.SetEnabled<EPActivity.timeSpent>(cache, row, false);
				PXUIFieldAttribute.SetEnabled<EPActivity.isBillable>(cache, row, false);
				PXUIFieldAttribute.SetEnabled<EPActivity.earningTypeID>(cache, row, false);
			}
			PXUIFieldAttribute.SetEnabled<EPActivity.type>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<EPActivity.mpstatus>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<EPActivity.released>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<EPActivity.uistatus>(cache, row, !wasUsed && row.MPStatus == MailStatusListAttribute.Processed);
			PXUIFieldAttribute.SetEnabled<EPActivity.startDate>(cache, row, origStatus == ActivityStatusListAttribute.Open && row.MPStatus == MailStatusListAttribute.Draft && row.Released != true);
			PXUIFieldAttribute.SetEnabled<EPActivity.owner>(cache, row, origStatus == ActivityStatusListAttribute.Open && row.MPStatus == MailStatusListAttribute.Draft && row.Released != true);
			PXUIFieldAttribute.SetEnabled<EPActivity.groupID>(cache, row, origStatus == ActivityStatusListAttribute.Open && row.MPStatus == MailStatusListAttribute.Draft && row.Released != true);

			ValidateTimeBillable(cache, row);
			ValidateOvertimeBillable(cache, row);

			row.EntityDescription = CacheUtility.GetErrorDescription(row.Exception) + GetEntityDescription(row);

			GotoParentActivity.SetEnabled(row.ParentTaskID != null);

			var isIncome = row.IsIncome == true;

			Send.SetVisible(!isIncome && (row.MPStatus == MailStatusListAttribute.Failed || row.MPStatus == MailStatusListAttribute.Draft));			
			Reply.SetVisible(isIncome);
			ReplyAll.SetVisible(isIncome);

			LoadEmailSource.SetVisible(row.IsIncome != true);
			DownloadEmlFile.SetVisible(isIncome);
			
			CancelSending.SetVisible(!isIncome && row.MPStatus == MailStatusListAttribute.PreProcess);
			CancelSending.SetEnabled(!isIncome && row.MPStatus == MailStatusListAttribute.PreProcess);
			process.SetEnabled(row.MPStatus == MailStatusListAttribute.PreProcess || (isIncome && row.MPStatus == MailStatusListAttribute.Failed));

			var isInserted = cache.GetStatus(row) == PXEntryStatus.Inserted;
			
			Forward.SetEnabled(!isInserted);
			Reply.SetEnabled(!isInserted);
			ReplyAll.SetEnabled(!isInserted);
			DownloadEmlFile.SetEnabled(isIncome);
			LoadEmailSource.SetEnabled(!isIncome && row.MPStatus == MailStatusListAttribute.Draft);

			PXDefaultAttribute.SetPersistingCheck<EPActivity.owner>(cache, row, row.TrackTime == true && row.MPStatus != MailStatusListAttribute.Deleted ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
		}

		protected virtual void EPActivity_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			var row = e.Row as EPActivity;
			if (row == null) return;

			row.ClassID = CRActivityClass.Email;

			if (row.Owner == null)
			{
				var newOwner = EmployeeMaint.GetCurrentEmployeeID(this);
				if (PXOwnerSelectorAttribute.BelongsToWorkGroup(this, row.GroupID, newOwner))
					row.Owner = newOwner;
			}

			if (row.ParentTaskID != null)
			{
				var parentAct = (EPActivity)PXSelect<EPActivity,
					Where<EPActivity.taskID, Equal<Required<EPActivity.taskID>>>>.
					Select(this, row.ParentTaskID);
				if (parentAct != null && parentAct.TaskID != null &&
					(parentAct.ProjectID != null || parentAct.ProjectTaskID != null))
				{
					row.ProjectID = parentAct.ProjectID;
					row.ProjectTaskID = parentAct.ProjectTaskID;
				}
			}

			if (string.IsNullOrEmpty(row.MessageId))
				row.MessageId = "<" + Guid.NewGuid() + "_acumatica>";

			if (row.IsIncome != true && row.ClassID != CRActivityClass.EmailRouting)
				row.MailFrom = FillMailFrom(this,row.Owner, true);

			if (row.MailAccountID == null && row.IsIncome != true && row.ClassID != CRActivityClass.EmailRouting)
				row.MailAccountID = GetDefaultAccountId(row.Owner);
		}

		protected virtual void EPActivity_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as EPActivity;
			var oldRow = e.OldRow as EPActivity;
			if (row == null || oldRow == null) return;

			row.ClassID = CRActivityClass.Email;


			if (row.ClassID != CRActivityClass.EmailRouting && oldRow.Owner != row.Owner)
				row.MailFrom = FillMailFrom(this, row.Owner, true);

			if (row.IsIncome != true && row.ClassID != CRActivityClass.EmailRouting && oldRow.Owner != row.Owner)
				row.MailAccountID = GetDefaultAccountId(row.Owner);
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

		protected virtual void EPActivity_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			var row = (EPActivity)e.Row;
			if (row == null) return;

			if (row.Billed == true || !string.IsNullOrEmpty(row.TimeCardCD) || row.MPStatus == MailStatusListAttribute.InProcess)
			{
				cache.SetStatus(e.Row, PXEntryStatus.Notchanged);
				throw new PXException(TM.Messages.EmailActivityCannotBeDeleted);
			}
		}

		protected virtual void UploadFileNameFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		#endregion

		#region SendEmail


		public static void SendEmail(SendEmailParams sendEmailParams)
		{
			SendEmail(PXGraph.CreateInstance<CREmailActivityMaint>(), sendEmailParams, null);
		}

		protected static void SendEmail(CREmailActivityMaint graph, SendEmailParams sendEmailParams, Action<EPActivity> handler)
		{
			var cache = graph.Message.Cache;
			var activityCache = cache;
			var activityIsDirtyOld = activityCache.IsDirty;
			var newEmail = cache.NonDirtyInsert<EPActivity>(null);
			var owner = EP.EmployeeMaint.GetCurrentEmployeeID(graph);
			newEmail.MailFrom = FillMailFrom(graph, owner);
			newEmail.MailReply = FillMailReply(graph, newEmail.MailReply);
			newEmail.MailTo = sendEmailParams.To;
			newEmail.MailCc = sendEmailParams.Cc;
			newEmail.MailBcc = sendEmailParams.Bcc;
			newEmail.Subject = sendEmailParams.Subject;

			newEmail.MPStatus = ActivityStatusListAttribute.Draft;
			activityCache.IsDirty = activityIsDirtyOld;
			cache.Current = newEmail;

			var sourceType = sendEmailParams.Source.With(s => s.GetType());
			var sourceCache = sourceType.With(type => graph.Caches[type]);
			var refNoteId = sourceCache.With(c => PXNoteAttribute.GetNoteID(c,
				sendEmailParams.Source, EntityHelper.GetNoteField(sourceType)));
			newEmail.RefNoteID = refNoteId != 0 ? (long?)refNoteId : null;

			var parentSourceType = sendEmailParams.ParentSource.With(s => s.GetType());
			var parentSourceCache = parentSourceType.
				With(type => Activator.CreateInstance(BqlCommand.Compose(typeof(PXCache<>), type), graph) as PXCache);
			var parentRefNoteId = parentSourceCache.
				With(c => PXNoteAttribute.GetNoteID(c,
							sendEmailParams.ParentSource,
							EntityHelper.GetNoteField(parentSourceType)));
			newEmail.ParentRefNoteID = parentRefNoteId != 0 ? (long?)parentRefNoteId : null; ;

			newEmail.Type = null;
			newEmail.IsIncome = false;
			newEmail.IsBillable = false;
			var newBody = sendEmailParams.Body;
			newEmail.Owner = owner;
			newEmail.Subject = newEmail.Subject;
			if (!string.IsNullOrEmpty(sendEmailParams.TemplateID))
			{
				string tplBody;
				if (TryLoadTemplate(sendEmailParams.TemplateID, sendEmailParams.Source, out tplBody))
				{
					if (!IsHtml(tplBody)) 
						tplBody = Tools.ConvertSimpleTextToHtml(tplBody);

					string orgHead;
					ParseHtml(tplBody, out orgHead);
					var html = new StringBuilder();
					html.AppendLine("<html><head>");
					html.Append(orgHead);
					html.Append("</head><body>");
					html.Append(tplBody);
					html.Append("<br/>");
					html.Append(Tools.RemoveHeader(newEmail.Body));
					html.Append("</body></html>");
					newBody = html.ToString();
				}
				else
				{
					newBody = newEmail.Body;
				}
			}
			else
			{
				if (!IsHtml(newBody))
					newBody = Tools.ConvertSimpleTextToHtml(newBody);

				var html = new StringBuilder();
				html.AppendLine("<html><body>");
				html.Append(Tools.RemoveHeader(newBody));
				html.Append("<br/>");
				html.Append(Tools.RemoveHeader(newEmail.Body));
				html.Append("</body></html>");
				newBody = html.ToString();
			}
			newEmail.Body = newBody;
			if (sendEmailParams.Attachments.Count > 0)
			{
				AttachFiles(newEmail, refNoteId, cache, sendEmailParams.Attachments);
			}
			if (handler != null) handler(newEmail);
			graph.Caches[newEmail.GetType()].RaiseRowSelected(newEmail);
			throw new PXPopupRedirectException(graph, graph.GetType().Name, true);
		}

		private static string FillMailReply(PXGraph graph, string str)
		{
			PX.Common.Mail.Mailbox mailAddress = null;
			var isCorrect = str != null &&
				PX.Common.Mail.Mailbox.TryParse(str, out mailAddress) &&
				!string.IsNullOrEmpty(mailAddress.Address);
			if (isCorrect)
			{
				isCorrect = PXSelect<EMailAccount,
					Where<EMailAccount.address, Equal<Required<EMailAccount.address>>>>.
					Select(graph, mailAddress.Address).
					Count > 0;
			}

			var result = str;
			if (!isCorrect)
				result = MailAccountManager.GetDefaultEmailAccount().With(_ => _.Address);
			if (string.IsNullOrEmpty(result))
			{
				var firstAcct = (EMailAccount)PXSelect<EMailAccount>.SelectWindowed(graph, 0, 1);
				if (firstAcct != null) result = firstAcct.Address;
			}
			return result;
		}


		private static string FillMailFrom(PXGraph graph, Guid? owner, bool allowUseCurrentUser = false)
		{
			string result = null;
			if (owner != null)
			{
				var row = PXSelectJoin<Users,
				LeftJoin<Contact, On<Contact.userID, Equal<Users.pKID>>>,
				Where<Users.pKID, Equal<Required<Users.pKID>>>>.
				Select(graph, owner);
				if (row != null && row.Count > 0)
				{
					var cont = (Contact)row[0][typeof(Contact)];
					var usr = (Users)row[0][typeof(Users)];

					var displayName = cont.DisplayName;

					if (string.IsNullOrEmpty(displayName))
						displayName = usr.FullName;

					var email = cont.EMail;
					if (email == null || (email = email.Trim()) == string.Empty)
						email = usr.Email;
					result = PX.Common.Mail.Mailbox.Create(displayName, email);
				}
			}
			if (string.IsNullOrEmpty(result) && allowUseCurrentUser)
			{				
					result = graph.Accessinfo.UserID.
						With(id => (Users)PXSelect<Users>.Search<Users.pKID>(graph, id)).
						With(u => PX.Common.Mail.Mailbox.Create(u.FullName, u.Email));				
			}		

			return result;
		}

		//TODO: need review
		private static void ParseHtml(string str, out string head)
		{
			head = string.Empty;
			const string htmlStart = "<html";
			const string headStart = "<head";
			const string headEnd = "head>";
			int index;
			int endIndex;
			if ((index = str.IndexOf(htmlStart, StringComparison.OrdinalIgnoreCase)) >= 0 &&
				(index = str.IndexOf(headStart, index + htmlStart.Length, StringComparison.OrdinalIgnoreCase)) >= 0 &&
				(endIndex = str.IndexOf(headEnd, index + headStart.Length, StringComparison.OrdinalIgnoreCase)) >= 0)
			{
				head = str.Substring(index, endIndex - index + headEnd.Length);
			}
		}

		private static bool TryLoadTemplate(string notificationName, object entity, out string content)
		{
			content = null;

			if (string.IsNullOrEmpty(notificationName)) return false;

			var search = (Notification)PXSelect<Notification, 
				Where<Notification.name, Equal<Required<Notification.name>>>>.
				Select(new PXGraph(), notificationName);
			if (search == null || search.NotificationID == null) return false;

			if (entity == null) content = search.Body;
			else
			{
				var graphType = PXSiteMap.Provider.FindSiteMapNodeByScreenID(search.ScreenID).With(_ => _.GraphType);
				var templateGraph = CreateGraph(graphType);
				var type = entity.GetType();
				var keys = GetKeys(entity, templateGraph.Caches[type]);
				content = PXTemplateContentParser.Instance.Process(search.Body, templateGraph, type, keys);
			}
			if (!IsHtml(content)) content = Tools.ConvertSimpleTextToHtml(content);

			return true;
		}

		private static readonly Regex _HtmlRegex = new Regex(
			@"^.*\<html( [^\>]*)?\>.*(\<head( [^\>]*)?\>(?<head>.*)\</([^\>]* )?head\>)?.*\<body.*",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private static bool IsHtml(string text)
		{
			return !string.IsNullOrEmpty(text) && _HtmlRegex.IsMatch(text);
		}

		private static PXGraph CreateGraph(string graphTypeName)
		{
			Type graphType = null;
			if (graphTypeName != null &&
				((graphType = BuildManager.GetType(graphTypeName, false)) != null &&
				typeof(PXGraph).IsAssignableFrom(graphType)))
			{
				return (PXGraph)PXGraph.CreateInstance(graphType);
			}
			return new PXGraph();
		}

		private static object[] GetKeys(object e, PXCache cache)
		{
			var keys = new List<object>();

			foreach (Type t in cache.BqlKeys)
				keys.Add(cache.GetValue(e, t.Name));

			return keys.ToArray();
		}
		#endregion

		#region Private Methods

		private bool VerifyEmailFields(EPActivity row)
		{
			var res = true;

			//From
			Message.Cache.RaiseExceptionHandling<EPActivity.mailAccountID>(row, null, null);
			if (row.MailAccountID == null)
			{
				var exception = new PXSetPropertyException(ErrorMessages.FieldIsEmpty);
				Message.Cache.RaiseExceptionHandling<EPActivity.mailAccountID>(row, null, exception);
				PXUIFieldAttribute.SetError<EPActivity.mailAccountID>(Message.Cache, row, exception.Message);
				res = false;
			}

			//To
			Message.Cache.RaiseExceptionHandling<EPActivity.mailTo>(row, null, null);
			if (string.IsNullOrWhiteSpace(row.MailTo))
			{
				var exception = new PXSetPropertyException(ErrorMessages.FieldIsEmpty);
				Message.Cache.RaiseExceptionHandling<EPActivity.mailTo>(row, null, exception);
				PXUIFieldAttribute.SetError<EPActivity.mailTo>(Message.Cache, row, exception.Message);
				res = false;
			}

			return res;
		}

		private Int32? GetDefaultAccountId(Guid? owner)
		{
			if (owner != null)
			{
				foreach (UserPreferences curacc in PXSelect<UserPreferences,
					Where<UserPreferences.userID, Equal<Required<UserPreferences.userID>>>>.Select(this, owner))
				{
					if (curacc.DefaultEMailAccountID != null)
						return curacc.DefaultEMailAccountID;
				}
			}
			foreach (PreferencesEmail curacc in PXSelect<PreferencesEmail>.Select(this))
			{
				if (curacc.DefaultEMailAccountID != null)
					return curacc.DefaultEMailAccountID;
			}
			return null;
		}

		private static string GetReplyAddress(EPActivity oldMessage)
		{
			var newAddressList = new AddressList();
			if (oldMessage.MailReply != null)
				foreach (Mailbox item in AddressList.Parse(oldMessage.MailReply))
				{
					string displayName = null;
					if (String.IsNullOrWhiteSpace(item.DisplayName) && oldMessage.MailFrom != null)
					{
						foreach (Mailbox item1 in AddressList.Parse(oldMessage.MailFrom))
							if (string.Equals(item1.Address, item.Address, StringComparison.OrdinalIgnoreCase))
								displayName = item1.DisplayName;
					}
					else
					{
						displayName = item.DisplayName;
					}
					var newitem = new Mailbox(displayName, item.Address);
					newAddressList.Add(newitem);
				}
			if (newAddressList.Count == 0 && oldMessage.MailFrom != null)
				foreach (Mailbox item in AddressList.Parse(oldMessage.MailFrom))
				{
					string displayName = item.DisplayName;
					var newitem = new Mailbox(displayName, item.Address);
					newAddressList.Add(newitem);
				} 
			return newAddressList.ToString();
		}

		private string GetMailAccountAddress(EPActivity oldMessage)
		{
			return oldMessage.MailAccountID.
				With(_ => (EMailAccount)PXSelect<EMailAccount,
					Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.
				Select(this, _.Value)).
				With(_ => _.Address);
		}

		private string GetReplyAllCCAddress(EPActivity oldMessage, string mailAccountAddress)
		{
			var newAddressList = new AddressList();
			if (oldMessage.MailCc != null)
			{
				foreach (Mailbox item in AddressList.Parse(oldMessage.MailCc))
				{
					if (!string.Equals(item.Address, mailAccountAddress, StringComparison.OrdinalIgnoreCase))
					{
						var newitem = new Mailbox(item.DisplayName, item.Address);
						newAddressList.Add(newitem);
					}
				}
			}
			return newAddressList.ToString();
		}

		private string GetReplyAllBCCAddress(EPActivity oldMessage, string mailAccountAddress)
		{
			var newAddressList = new AddressList();
			if (oldMessage.MailBcc != null)
			{
				foreach (Mailbox item in AddressList.Parse(oldMessage.MailBcc))
				{
					if (!string.Equals(item.Address, mailAccountAddress, StringComparison.OrdinalIgnoreCase))
					{
						var newitem = new Mailbox(item.DisplayName, item.Address);
						newAddressList.Add(newitem);
					}
				}
			}
			return newAddressList.ToString();
		}

		private string GetReplyAllAddress(EPActivity oldMessage, string mailAccountAddress)
		{
			var newAddressList = new AddressList();
			if (oldMessage.MailReply != null)
				foreach (Mailbox item in AddressList.Parse(oldMessage.MailReply))
				{
					string displayName = null;
					if (String.IsNullOrEmpty(item.DisplayName) && oldMessage.MailFrom != null)
					{
						foreach (Mailbox item1 in AddressList.Parse(oldMessage.MailFrom))
							if (string.Equals(item1.Address, item.Address, StringComparison.OrdinalIgnoreCase))
								displayName = item1.DisplayName;
					}
					else
					{
						displayName = item.DisplayName;
					}
					var newitem = new Mailbox(displayName, item.Address);
					newAddressList.Add(newitem);
				}
			if (newAddressList.Count == 0 && oldMessage.MailFrom != null)
				foreach (Mailbox item in AddressList.Parse(oldMessage.MailFrom))
				{
					string displayName = item.DisplayName;
					var newitem = new Mailbox(displayName, item.Address);
					newAddressList.Add(newitem);
				} 

			if (oldMessage.MailTo != null)
			{
				foreach (Mailbox item in AddressList.Parse(oldMessage.MailTo))
				{
					if (!string.Equals(item.Address, mailAccountAddress, StringComparison.OrdinalIgnoreCase))
					{
						var newitem = new Mailbox(item.DisplayName, item.Address);
						newAddressList.Add(newitem);
					}
				}
			}

			return newAddressList.ToString();
		}

		private static string GetSubjectPrefix(string subject, bool forward)
		{
			if (subject.StartsWith("RE: ") || subject.StartsWith("FW: "))
				subject = subject.Substring(3);

			return (forward ? "FW: " : "RE: ") + subject;
		}

		private string CreateReplyBody(string mailFrom, string mailTo, string subject, string message, DateTime lastModifiedDateTime)
		{
			var wikiTitle =
				"<br /><div class=\"wiki\" style=\"border-top:solid 1px black;padding:2px 0px;line-height:1.5em;\">" +
				"\r\n<b>From:</b> " + mailFrom +
				"<br/>\r\n<b>Sent:</b> " + lastModifiedDateTime +
				"<br/>\r\n<b>To:</b> " + mailTo +
				"<br/>\r\n<b>Subject:</b> " + subject +
				"<br/><br/>\r\n</div>";

			var signature = GetSignature();

			if (string.IsNullOrEmpty(message))
				return "<html><body>" + signature + wikiTitle + "</body></html>";

			var bodyIndex = message.IndexOf("<body>");
			if (bodyIndex == -1) bodyIndex = message.IndexOf("<body ");
			if (bodyIndex > 0)
			{
				var positionIndex = message.IndexOf(">", bodyIndex) + 1;
				return signature + message.Substring(0, positionIndex) + wikiTitle + message.Substring(positionIndex);
			}

			return signature + wikiTitle + message;
		}

		private void ValidateTimeBillable(PXCache sender, EPActivity row)
		{
			sender.RaiseExceptionHandling<EPActivity.timeBillable>(row, null, null);
			if (row.TimeBillable != null && row.TimeBillable > row.TimeSpent)
			{
				var exception = new PXSetPropertyException(CR.Messages.BillableTimeCannotBeGreaterThanTimeSpent);
				var value = PXDBTimeSpanAttribute.FromMinutes((int)row.TimeBillable);
				sender.RaiseExceptionHandling<EPActivity.timeBillable>(row, value, exception);
			}
		}

		private void ValidateOvertimeBillable(PXCache sender, EPActivity row)
		{
			sender.RaiseExceptionHandling<EPActivity.overtimeBillable>(row, null, null);
			if (row.OvertimeBillable != null && row.OvertimeBillable > row.OvertimeSpent)
			{
				var exception = new PXSetPropertyException(CR.Messages.OvertimeBillableCannotBeGreaterThanOvertimeSpent);
				var value = PXDBTimeSpanAttribute.FromMinutes((int)row.OvertimeBillable);
				sender.RaiseExceptionHandling<EPActivity.overtimeBillable>(row, value, exception);
			}
		}

		private void TryCorrectMailDisplayNames(EPActivity message)
		{			
			var ownerDisplayName = FillMailFrom(this, message.Owner);

			if (ownerDisplayName == null)
			{
				ownerDisplayName = 
				message.With(id => (EMailAccount)PXSelect<EMailAccount>.
					Search<EMailAccount.emailAccountID>(this, id.MailAccountID)).
				With(a => a.Description);
			}

			//from
			PX.Common.Mail.Mailbox fromBox;
			var fromAddress = message.MailFrom;
			if (!string.IsNullOrEmpty(fromAddress) &&
				PX.Common.Mail.Mailbox.TryParse(fromAddress, out fromBox))
			{
				message.MailFrom = PX.Common.Mail.Mailbox.Create(ownerDisplayName, fromBox.Address);
				Caches[message.GetType()].Update(message);
			}

			//reply
			PX.Common.Mail.Mailbox replyBox;
			var replyAddress = message.MailReply;
			if (!string.IsNullOrEmpty(replyAddress) &&
				PX.Common.Mail.Mailbox.TryParse(replyAddress, out replyBox) &&
				!object.Equals(replyBox.DisplayName, ownerDisplayName))
			{
				message.MailReply = PX.Common.Mail.Mailbox.Create(ownerDisplayName, replyBox.Address);
				Caches[message.GetType()].Update(message);
			}
		}

		private void CorrectFileNames()
		{
			var noteId = Message.Current.With(m => m.NoteID);
			var actNoteId = Activites.Current.With(act => act.NoteID);
			if (noteId == null || actNoteId == null) return;

			var searchText = "[" + Message.Current.MessageId + "]";
			var replaceText = "[" + Activites.Current.NoteID + "]";
			var cache = Caches[typeof(UploadFile)];
			PXSelectJoin<UploadFile,
					InnerJoin<NoteDoc, On<NoteDoc.fileID, Equal<UploadFile.fileID>>>,
					Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.
					Clear(this);
			foreach (UploadFile file in
				PXSelectJoin<UploadFile,
					InnerJoin<NoteDoc, On<NoteDoc.fileID, Equal<UploadFile.fileID>>>,
					Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.
				Select(this, noteId))
			{
				if (!string.IsNullOrEmpty(file.Name) && file.Name.Contains(searchText))
				{
					file.Name = file.Name.Replace(searchText, replaceText);
					cache.PersistUpdated(file);
				}
			}
		}

		private string GetEntityDescription(EPActivity row)
		{
			string res = string.Empty;
			var helper = new EntityHelper(this);
			var entity = row.RefNoteID.With(_ => helper.GetEntityRow(_.Value, true));
			if (entity != null)
			{
				var entityType = entity.GetType();
				row.EntityType = entityType.FullName;

				var graphType = helper.GetPrimaryGraphType(entity, false);
				row.GraphType = graphType.With(graph => graph.FullName);

				res = CacheUtility.GetDescription(helper, entity, entityType);
			}
			return res;
		}

		protected static void AttachFiles(EPActivity newEmail, long refNoteId, PXCache cache, IEnumerable<FileInfo> files)
		{
			var uploadFile = PXGraph.CreateInstance<UploadFileMaintenance>();
			var filesID = new List<Guid>();
			foreach (FileInfo file in files)
			{
				var format = refNoteId < 1 ? "[{0}] {2}{3}" : "[{0}] [{2}] \\{3}";
				var separator = file.FullName.IndexOf('\\') > -1 ? string.Empty : "\\";
				file.FullName = string.Format(format, newEmail.ImcUID, refNoteId, separator, file.FullName);
				uploadFile.SaveFile(file, FileExistsAction.CreateVersion);
				var uid = (Guid)file.UID;
				if (!filesID.Contains(uid))
					filesID.Add(uid);
			}
			cache.SetValueExt(newEmail, "NoteFiles", filesID.ToArray());
		}

		private string GetSignature()
		{
			var signature = ((UserPreferences)PXSelect<UserPreferences>.
				Search<UserPreferences.userID>(this, PXAccess.GetUserID())).
				With(pref => pref.MailSignature);
			if (signature != null && (signature = signature.Trim()) != string.Empty)
				return signature;
			return string.Empty;
		}

		private void CopyAttachments(PXGraph targetGraph, EPActivity message, EPActivity newMessage)
		{
			if (message == null || newMessage == null) return;

			var cache = Message.Cache;
			var filesIDs = (cache.GetStateExt(message, "NoteFiles") as PXFieldState).With(_ => _.Value as string[]);
			if (filesIDs == null || filesIDs.Length == 0) return;

			var copyFileIDs = new List<Guid>();
			foreach (string filesKey in filesIDs)
			{
				var guidString = filesKey;
				var separatorIndex = filesKey.IndexOf('$');
				if (separatorIndex > -1) guidString = filesKey.Substring(0, separatorIndex);
				Guid fileId;
				if (GUID.TryParse(guidString, out fileId))
					copyFileIDs.Add(fileId);
			}
			var newFileIDs = copyFileIDs;

			targetGraph.Caches[newMessage.GetType()].SetValueExt(newMessage, "NoteFiles", newFileIDs.ToArray());
			Caches[typeof(Note)].IsDirty = false;
			Caches[typeof(NoteDoc)].IsDirty = false;
			cache.IsDirty = false;
		}

		private EPSetup EPSetupCurrent
		{
			get
			{				
				return (EPSetup) PXSelect<EPSetup>.SelectWindowed(this, 0, 1) ?? EmptyEpSetup;
			}
		}
		private static readonly EPSetup EmptyEpSetup = new EPSetup();

		#endregion

		#region Public Methods
		#endregion

	}
}
