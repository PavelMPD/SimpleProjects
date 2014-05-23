using System.Collections;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;

namespace PX.Objects.EP
{
    //[PXGraphName(Messages.EmailRoutingMaint, typeof(EPActivity))]
	public class EmailRoutingMaint : PXGraph<EmailRoutingMaint>
	{
		#region Selects

		public PXSelect<EPActivity,
			Where<EPActivity.classID, Equal<CRActivityClass.emailRouting>>>
			Message;

		public PXSelect<EPActivity,
			Where<EPActivity.taskID, Equal<Current<EPActivity.taskID>>>> 
			Activites;

		public EPViewSelect<EPActivity, EPActivity>
			EPViews;


		#endregion

		#region Ctor

		public EmailRoutingMaint()
		{
			PXUIFieldAttribute.SetEnabled(Message.Cache, null, false);
			PXUIFieldAttribute.SetEnabled(Activites.Cache, null, false);
		}

		#endregion

		#region Actions

		public PXCancel<EPActivity> Cancel;

		public PXDelete<EPActivity> Delete;

		public PXAction<EPActivity> GotoParentActivity;
		[PXUIField(DisplayName = "View Parent", MapEnableRights = PXCacheRights.Select)]
		[PXLookupButton(Tooltip = "View Parent Email")]
		protected IEnumerable gotoParentActivity(PXAdapter adapter)
		{
			var parentOriginalActivity = Activites.Current.With(act => DefaultEmailProcessor.GetParentOriginalActivity(this, (int)act.TaskID));
			if (parentOriginalActivity != null)
			{
				new EntityHelper(this).NavigateToRow((long)parentOriginalActivity.NoteID, PXRedirectHelper.WindowMode.New);
			}
			return adapter.Get();
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

		protected virtual void EPActivity_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var row = e.Row as EPActivity;
			if (row == null) return;

			var activity = DefaultEmailProcessor.GetParentOriginalActivity(this, (int)row.TaskID);
			if (activity != null)
			{
				GotoParentActivity.SetVisible(true);

				PXUIFieldAttribute.SetReadOnly<EPActivity.entityDescription>(cache, e.Row);
				var isMessageFailed = row.MPStatus == MailStatusListAttribute.Failed;
				row.EntityDescription = isMessageFailed ? CacheUtility.GetErrorDescription(row.Exception) : string.Empty;
				var helper = new EntityHelper(this);
				var entity = activity.RefNoteID.With(refNoteId => helper.GetEntityRow(refNoteId.Value));
				if (entity != null)
					row.EntityDescription += CacheUtility.GetDescription(helper, entity, entity.GetType());
			}
			else
			{
				GotoParentActivity.SetVisible(false);
			}
		}
		

		#endregion
	}
}
