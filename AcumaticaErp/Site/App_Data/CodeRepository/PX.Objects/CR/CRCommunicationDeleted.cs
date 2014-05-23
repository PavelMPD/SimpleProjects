using System;
using System.Collections;
using PX.Data;
using PX.Data.EP;
using PX.SM;

namespace PX.Objects.CR
{
	#region EmailFilter
	[PXCacheName(Messages.Filter)]
    [Serializable]
	public partial class EmailFilter : IBqlTable
	{
		#region SearchText
		public abstract class searchText : IBqlField { }
		private string _searchText;
		[PXString(255, InputMask = "[a-zA-Z0-9]*")]
		[PXUIField(DisplayName = "Search")]
		public virtual string SearchText
		{
			get { return _searchText; }
			set
			{
				if (_searchText != value)
					_likeText = null;

				_searchText = value;
			}
		}
		#endregion

		#region LikeText
		public abstract class likeText : IBqlField { }
		protected string _likeText;
		[PXString]
		[PXUIField(Visible = false)]
		public virtual string LikeText
		{
			get { return _likeText ?? (_likeText = string.Concat("%", SearchText, "%")); }
		}
		#endregion

		#region EmailAccountID
		public abstract class emailAccountID : IBqlField { }
		[PXInt()]
		[PXUIField(DisplayName = "Account")]
		[PXSelector(typeof(Search<EMailAccount.emailAccountID, Where<EMailAccount.userName, IsNull>>),
			typeof(EMailAccount.address), typeof(EMailAccount.description), typeof(EMailAccount.replyAddress),
			DescriptionField = typeof(EMailAccount.description))]
		public virtual Int32? EmailAccountID { get; set; }
		#endregion
	}
	#endregion

	public class CRCommunicationDeleted : PXGraph<CRCommunicationSend>
	{
		#region Select
		[PXViewName(Messages.Selection)]
		public PXFilter<EmailFilter>
			Filter;

		[PXFilterable]
		[PXViewDetailsButton(typeof(EmailFilter))]
		public PXSelectReadonly2<EPActivity,
			LeftJoin<EMailAccount, On<EMailAccount.emailAccountID, Equal<EPActivity.mailAccountID>>>,
			Where<EPActivity.classID, Equal<CRActivityClass.email>,
				And2<Where<EPActivity.mpstatus, Equal<MailStatusListAttribute.deleted>>,
				And2<Where<Current<EmailFilter.emailAccountID>, IsNull,
					Or<EPActivity.mailAccountID, Equal<Current<EmailFilter.emailAccountID>>>>,
				And<Where<Current<EmailFilter.searchText>, IsNull,
						Or<EPActivity.subject, Like<Current<EmailFilter.likeText>>,
						Or<EPActivity.mailTo, Like<Current<EmailFilter.likeText>>,
						Or<EPActivity.mailFrom, Like<Current<EmailFilter.likeText>>,
						Or<EMailAccount.description, Like<Current<EmailFilter.likeText>>>>>>>>>>>>
			Deleted;
		#endregion

		#region Ctors
		public CRCommunicationDeleted()
		{
			PXUIFieldAttribute.SetVisible<EMailAccount.description>(Caches[typeof(EMailAccount)], null, true);
			PXUIFieldAttribute.SetVisible<EPActivity.mailFrom>(Deleted.Cache, null, true);
			PXUIFieldAttribute.SetVisible<EPActivity.mailTo>(Deleted.Cache, null, true);
			PXUIFieldAttribute.SetDisplayName<EMailAccount.description>(Caches[typeof(EMailAccount)], "Account");
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

		#region Action
		public PXCancel<EmailFilter> Cancel;

		public PXAction<EmailFilter> ViewEntity;
		[PXUIField(MapEnableRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		protected void viewEntity()
		{
			if (Deleted.Current != null && Deleted.Current.RefNoteID != null)
			{
				EntityHelper entity = new EntityHelper(this);
				entity.NavigateToRow((long) Deleted.Current.RefNoteID, PXRedirectHelper.WindowMode.New);
			}
		}

		public PXAction<EmailFilter> Reply;
		[PXUIField(DisplayName = Messages.Reply)]
		[PXReplyMailButton]
		protected void reply()
		{
			CREmailActivityMaint crMailActivityMaint = CreateInstance<CREmailActivityMaint>();
			crMailActivityMaint.Message.Current = Deleted.Current;
			crMailActivityMaint.Reply.PressButton();
		}

		public PXAction<EmailFilter> ReplyAll;
		[PXUIField(DisplayName = Messages.ReplyAll)]
		[PXReplyMailButton]
		public virtual void replyAll()
		{
			CREmailActivityMaint crMailActivityMaint = CreateInstance<CREmailActivityMaint>();
			crMailActivityMaint.Message.Current = Deleted.Current;
			crMailActivityMaint.ReplyAll.PressButton();
		}

		public PXAction<EmailFilter> Forward;
		[PXUIField(DisplayName = Messages.Forward)]
		[PXForwardMailButton]
		protected void forward()
		{
			CREmailActivityMaint crMailActivityMaint = CreateInstance<CREmailActivityMaint>();
			crMailActivityMaint.Message.Current = Deleted.Current;
			crMailActivityMaint.Forward.PressButton();
		}

		public PXAction<EmailFilter> DeleteRow;
		[PXUIField(DisplayName = "Delete")]
		[PXDeleteButton]
		public virtual void deleteRow()
		{
			var item = Deleted.Current;
			if (item == null) return;

			Deleted.Cache.Delete(item);
			Persist();
		}
		#endregion
	}
}

