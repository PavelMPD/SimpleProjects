using PX.Data;
using PX.Data.EP;
using PX.SM;

namespace PX.Objects.CR
{
	public class CRCommunicationOutbox : PXGraph<CRCommunicationOutbox>
	{
		#region Select
		[PXViewName(Messages.Selection)]
		public PXFilter<EmailFilter> Filter;

		[PXFilterable]
		[PXViewDetailsButton(typeof(EmailFilter))]
		public PXFilteredProcessingJoin<EPActivity,EmailFilter,
			LeftJoin<EMailAccount, On<EMailAccount.emailAccountID, Equal<EPActivity.mailAccountID>>>,
			Where<EPActivity.classID, Equal<CRActivityClass.email>,
			And2<Where<EPActivity.mpstatus, Equal<MailStatusListAttribute.preProcess>,
				Or<EPActivity.mpstatus, Equal<MailStatusListAttribute.inProcess>,
				Or<EPActivity.mpstatus, Equal<MailStatusListAttribute.failed>>>>, 
			And2<Where<EPActivity.isIncome, IsNull,
					Or<EPActivity.isIncome, Equal<False>>>,
			And2<Where<Current<EmailFilter.emailAccountID>, IsNull,
					Or<EPActivity.mailAccountID, Equal<Current<EmailFilter.emailAccountID>>>>,
			And<Where<Current<EmailFilter.searchText>, IsNull,
					Or<EPActivity.subject, Like<Current<EmailFilter.likeText>>,
					Or<EPActivity.mailTo, Like<Current<EmailFilter.likeText>>,
					Or<EMailAccount.description, Like<Current<EmailFilter.likeText>>>>>>>>>>>> 
			Outbox;
		#endregion
		
		#region Ctors
		public CRCommunicationOutbox()
		{
			Outbox.SetProcessDelegate(LongRunProcessor.ProcessItem);

			PXUIFieldAttribute.SetVisible<EMailAccount.description>(Caches[typeof(EMailAccount)], null, true);
			PXUIFieldAttribute.SetVisible<EPActivity.mailTo>(Outbox.Cache, null, true);
			PXUIFieldAttribute.SetDisplayName<EMailAccount.description>(Caches[typeof(EMailAccount)], "From");
		    Outbox.Cache.AllowInsert = false;
		    Outbox.Cache.AllowUpdate = false;
		    Outbox.Cache.AllowDelete = true;

			Actions["Schedule"].SetVisible(false);
			Actions["Process"].SetCaption("Send");
			Actions["ProcessAll"].SetCaption("Send All");
			Actions.Move("Process", "Cancel");
		}
		#endregion

		#region Delegates
		protected virtual void EPActivity_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            EPActivity item = (EPActivity)e.Row;
            if (item == null) return;

            var graph = PXGraph.CreateInstance<CREmailActivityMaint>();
            graph.Message.Current = graph.Message.Search<EPActivity.taskID>(item.TaskID);
            graph.Delete.Press();
            Outbox.Cache.IsDirty = false;
        }

		#endregion

		#region Event Handlers
		[PXDBString(255, InputMask = "", IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Subject", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXNavigateSelector(typeof(EPActivity.subject))]
		protected virtual void EPActivity_Subject_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region Processing

		private static class LongRunProcessor
		{
			public static void ProcessItem(EPActivity item)
			{
				MailSendProvider.SendMessage(item);

				if (item.MPStatus == MailStatusListAttribute.Failed)
					throw new PXException(item.Exception);
			}
		}

		#endregion

		#region Action
		public PXCancel<EmailFilter> Cancel;

		public PXAction<EmailFilter> ViewEntity;
		[PXUIField(MapEnableRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		protected void viewEntity()
		{
			if (Outbox.Current != null && Outbox.Current.RefNoteID != null)
			{
				EntityHelper entity = new EntityHelper(this);
				entity.NavigateToRow((long) Outbox.Current.RefNoteID, PXRedirectHelper.WindowMode.New);
			}
		}

		public PXAction<EmailFilter> CreateNew;
		[PXUIField(DisplayName = "New")]
		[PXButton(ImageKey = Web.UI.Sprite.Main.AddNew, Tooltip = Messages.AddTask)]
		public virtual void createNew()
		{
			var graph = PXGraph.CreateInstance<CREmailActivityMaint>();
			graph.Message.Current = graph.Message.Insert();
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXAction<EmailFilter> Reply;
		[PXUIField(DisplayName = Messages.Reply)]
		[PXReplyMailButton]
		protected void reply()
		{
			var item = Outbox.Current;
			if (item == null) return;

			var graph = CreateInstance<CREmailActivityMaint>();
			graph.Message.Current = graph.Message.Search<EPActivity.taskID>(item.TaskID);
			graph.Reply.PressButton();
		}

		public PXAction<EmailFilter> ReplyAll;
		[PXUIField(DisplayName = Messages.ReplyAll)]
		[PXReplyMailButton]
		public virtual void replyAll()
		{
			var item = Outbox.Current;
			if (item == null) return;

			var graph = CreateInstance<CREmailActivityMaint>();
			graph.Message.Current = graph.Message.Search<EPActivity.taskID>(item.TaskID);
			graph.ReplyAll.PressButton();
		}

		public PXAction<EmailFilter> Forward;
		[PXUIField(DisplayName = Messages.Forward)]
		[PXForwardMailButton]
		protected void forward()
		{
			var item = Outbox.Current;
			if (item == null) return;

			var graph = CreateInstance<CREmailActivityMaint>();
			graph.Message.Current = graph.Message.Search<EPActivity.taskID>(item.TaskID);
			graph.Forward.PressButton();
		}

		public PXAction<EmailFilter> DeleteRow;
		[PXUIField(DisplayName = "Delete")]
		[PXButton(ImageKey = Web.UI.Sprite.Main.Remove, Tooltip = Messages.TrashCurrent, ConfirmationMessage = Messages.TrashCurrentConfirm)]
		public virtual void deleteRow()
		{
			var item = Outbox.Current;
			if (item == null) return;

			var graph = PXGraph.CreateInstance<CREmailActivityMaint>();
			graph.Message.Current = graph.Message.Search<EPActivity.taskID>(item.TaskID);
			graph.Delete.Press();
		}

		#endregion
	}
}
