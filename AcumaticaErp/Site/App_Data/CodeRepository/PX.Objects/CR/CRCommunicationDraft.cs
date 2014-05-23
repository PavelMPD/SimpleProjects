using PX.Data;
using PX.Data.EP;
using PX.SM;

namespace PX.Objects.CR
{
	public class CRCommunicationDraft : PXGraph<CRCommunicationDraft>
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
				And<EPActivity.mpstatus, Equal<MailStatusListAttribute.draft>,
				And2<Where<EPActivity.isIncome, IsNull,
						Or<EPActivity.isIncome, Equal<False>>>,
				And2<Where<Current<EmailFilter.emailAccountID>, IsNull,
					Or<EPActivity.mailAccountID, Equal<Current<EmailFilter.emailAccountID>>>>,
				And<Where<Current<EmailFilter.searchText>, IsNull,
						Or<EPActivity.subject, Like<Current<EmailFilter.likeText>>,
						Or<EPActivity.mailTo, Like<Current<EmailFilter.likeText>>,
						Or<EMailAccount.description, Like<Current<EmailFilter.likeText>>>>>>>>>>>>
			Draft;
		#endregion

		#region Ctors
		public CRCommunicationDraft()
		{
			PXUIFieldAttribute.SetVisible<EMailAccount.description>(Caches[typeof(EMailAccount)], null, true);
			PXUIFieldAttribute.SetVisible<EPActivity.mailTo>(Draft.Cache, null, true);
			PXUIFieldAttribute.SetDisplayName<EMailAccount.description>(Caches[typeof(EMailAccount)], "From");
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
			if (Draft.Current != null && Draft.Current.RefNoteID != null)
			{
				EntityHelper entity = new EntityHelper(this);
				entity.NavigateToRow((long) Draft.Current.RefNoteID, PXRedirectHelper.WindowMode.New);
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
			var item = Draft.Current;
			if (item == null) return;

			var graph = PXGraph.CreateInstance<CREmailActivityMaint>();
			graph.Message.Current = graph.Message.Search<EPActivity.taskID>(item.TaskID);
			graph.Delete.Press();
		}
		#endregion
	}
}
