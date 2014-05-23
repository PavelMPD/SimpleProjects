using System;
using System.Collections;
using System.Collections.Generic;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.EP;
using PX.SM;

namespace PX.Objects.CR
{
	[DashboardType((int)DashboardTypeAttribute.Type.Default)]
	public class CRCommunicationInbox : PXGraph<CRCommunicationInbox>
	{
		#region Select
		public PXSelect<EPActivity> Message;

		[PXViewName(Messages.Selection)]
		public PXFilter<EmailFilter> Filter;

		[PXFilterable]
		[PXViewDetailsButton(typeof(EmailFilter))]
		public PXFilteredProcessingJoin<EPActivity, EmailFilter,
			LeftJoin<EMailAccount, On<EMailAccount.emailAccountID, Equal<EPActivity.mailAccountID>>>,
			Where<EPActivity.classID, Equal<CRActivityClass.email>,
				And<EPActivity.isIncome, Equal<True>,
				And<EPActivity.mpstatus, NotEqual<MailStatusListAttribute.deleted>,
				And2<Where<Current<EmailFilter.emailAccountID>, IsNull,
					Or<EPActivity.mailAccountID, Equal<Current<EmailFilter.emailAccountID>>>>,
				And<Where<Current<EmailFilter.searchText>, IsNull,
					Or<EPActivity.subject, Like<Current<EmailFilter.likeText>>,
					Or<EPActivity.mailFrom, Like<Current<EmailFilter.likeText>>,
					Or<EMailAccount.description, Like<Current<EmailFilter.likeText>>>>>>>>>>>> 
			Inbox;
		#endregion

		#region Ctors
		public CRCommunicationInbox()
		{
			Inbox.SetProcessDelegate(LongRunProcessor.ProcessItem);

			PXUIFieldAttribute.SetVisible<EMailAccount.description>(Caches[typeof(EMailAccount)], null, true);
			PXUIFieldAttribute.SetVisible<EPActivity.mailFrom>(Inbox.Cache, null, true);
			PXUIFieldAttribute.SetDisplayName<EMailAccount.description>(Caches[typeof(EMailAccount)],PXLocalizer.Localize(Messages.EMailAccountDescription, typeof(Messages).ToString()));

			Actions["Schedule"].SetVisible(false);
			Actions["Process"].SetVisible(false);
		    Actions["ProcessAll"].SetVisible(false);
			Actions.Move("Process","Cancel");
		}
		#endregion

		#region Processing

		private static class LongRunProcessor
		{
			public static void ProcessItem(EPActivity item)
			{
				EMailMessageReceiver.ProcessMessage(item);
				if (item.MPStatus == MailStatusListAttribute.Failed)
					throw new PXException(item.Exception);
			}
		}

		#endregion

		#region Delegates
		public static void LongProcessReceive()
		{
			EMailAccountMaint eMailAccountMaint = CreateInstance<EMailAccountMaint>();
			PXAdapter eMailAccountMaintAdapter = new PXAdapter(eMailAccountMaint.EMailAccounts.View);
			foreach (object item in eMailAccountMaint.ReceiveAll.Press(eMailAccountMaintAdapter)) { }
		}

		#endregion

		#region Event Handlers
		[PXDBString(255, InputMask = "", IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Subject", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXNavigateSelector(typeof (EPActivity.subject))]
		protected virtual void EPActivity_Subject_CacheAttached(PXCache sender)
		{
		}

		[EPStartDate(DisplayName = "Date", DisplayNameDate = "Date", DisplayNameTime = "Time")]
		[PXUIField(DisplayName = "Date")]
		protected virtual void EPActivity_StartDate_CacheAttached(PXCache sender)
		{
		}
		#endregion
		

		#region Action
		public PXCancel<EmailFilter> Cancel;

		public PXAction<EmailFilter> ReceiveAll;
		[PXUIField(DisplayName = Messages.ReceiveAll)]
		[PXButton(Tooltip = Messages.ReceiveAll)]
		protected virtual void receiveAll()
		{
			PXLongOperation.StartOperation(this, delegate() { LongProcessReceive(); });
		}

		public PXAction<EmailFilter> ViewEntity;
		[PXUIField(MapEnableRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		protected void viewEntity()
		{
			if (Inbox.Current != null && Inbox.Current.RefNoteID != null)
			{
				EntityHelper entity = new EntityHelper(this);
				entity.NavigateToRow((long) Inbox.Current.RefNoteID, PXRedirectHelper.WindowMode.New);
			}
		}

		public PXAction<EmailFilter> CreateNew;
		[PXUIField(DisplayName = "New")]
		[PXButton(ImageKey = Web.UI.Sprite.Main.AddNew, Tooltip = Messages.AddEmail)]
		public virtual void createNew()
		{
			var graph = PXGraph.CreateInstance<CREmailActivityMaint>();
			graph.Message.Current = graph.Message.Insert();
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXAction<EmailFilter> DeleteRow;
		[PXUIField(DisplayName = "Delete")]
		[PXButton(ImageKey = Web.UI.Sprite.Main.Remove, Tooltip = Messages.TrashCurrent, ConfirmationMessage = Messages.TrashCurrentConfirm)]
		public virtual void deleteRow()
		{
			CREmailActivityMaint graph = null;
			foreach(EPActivity item in GetSelectedItems())
			{
				if (graph == null) graph = CreateInstance<CREmailActivityMaint>();
				graph.Message.Current = graph.Message.Search<EPActivity.taskID>(item.TaskID);
				graph.Delete.Press();
				if (Inbox.Current == item) Inbox.Current = null;
				Inbox.Cache.Remove(item);
			}
		}

		public PXAction<EmailFilter> Reply;
		[PXUIField(DisplayName = Messages.Reply)]
		[PXReplyMailButton]
		protected void reply()
		{
			var item = Inbox.Current;
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
			var item = Inbox.Current;
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
			var item = Inbox.Current;
			if (item == null) return;

			var graph = CreateInstance<CREmailActivityMaint>();
			graph.Message.Current = graph.Message.Search<EPActivity.taskID>(item.TaskID);
			graph.Forward.PressButton();
		}
		#endregion

		#region Private Method

		private IEnumerable GetSelectedItems()
		{
			var found = false;
			foreach (EPActivity row in Inbox.Cache.Updated)
				if (row.Selected == true)
				{
					found = true;
					yield return row;
				}
			if (!found && Inbox.Current != null)
				yield return Inbox.Current;
		}

		#endregion
	}
}
