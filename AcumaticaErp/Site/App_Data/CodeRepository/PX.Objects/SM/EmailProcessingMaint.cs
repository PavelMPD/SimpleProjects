using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;

namespace PX.SM
{
	//[PXGraphName(PX.Objects.EP.Messages.EmailProcessing)]
    [Serializable]
	public class EmailProcessingMaint : PXGraph<EmailProcessingMaint>
	{
		#region EmailProcessingFilter

        [Serializable]
		public partial class EmailProcessingFilter : OwnedFilter
		{
			#region Account

			public abstract class account : IBqlField { }

			[PXInt]
			[PXUIField(DisplayName = "Account")]
			[PXEMailAccountIDSelectorAttribute(false,true)]
			public virtual Int32? Account { get; set; }

			#endregion

			#region Type

			public abstract class type : IBqlField { }

			[PXInt]
			[PXDefault(0)]
			[PXUIField(DisplayName = "Type")]
			[PXIntList(new [] { 0, 1, 2 }, new [] { "All", "Incoming", "Outgoing" })]
			public virtual Int32? Type { get; set; }

			#endregion

			#region ownerID
			public new abstract class ownerID : IBqlField { }
			#endregion

			#region myOwner
			public new abstract class myOwner : IBqlField { }
			#endregion

			#region myWorkGroup
			public new abstract class myWorkGroup : IBqlField { }
			#endregion

			#region workGroupID
			public new abstract class workGroupID : IBqlField { }
			#endregion
		}

		#endregion

		#region AllEmailes

		public sealed class AllEmailes : Constant<int>
		{
			public AllEmailes() : base(0) { }
		}

		#endregion

		#region IncomingEmails

		public sealed class IncomingEmails : Constant<int>
		{
			public IncomingEmails() : base(1) { }
		}

		#endregion

		#region OutgoingEmails

		public sealed class OutgoingEmails : Constant<int>
		{
			public OutgoingEmails() : base(2) { }
		}

		#endregion

		#region Selects

		[PXCacheName(PX.Objects.EP.Messages.Filter)]
		public PXFilter<EmailProcessingFilter>
			Filter;

		[PXCacheName(PX.Objects.EP.Messages.Emails)]
		[PXFilterable]
		public PXFilteredProcessingJoin<EPActivity, EmailProcessingFilter, 
			LeftJoin<EMailAccount, On<EMailAccount.emailAccountID, Equal<EPActivity.mailAccountID>>>,
			Where<EPActivity.mpstatus, Equal<MailStatusListAttribute.preProcess>,
 				And2<Where<EPActivity.classID, Equal<CRActivityClass.email>, 
					Or<EPActivity.classID, Equal<CRActivityClass.emailRouting>>>,
				And2<Where<Current<EmailProcessingFilter.account>, IsNull,
					Or<EPActivity.mailAccountID, Equal<Current<EmailProcessingFilter.account>>>>,
				And2<Where<Current<EmailProcessingFilter.type>, Equal<AllEmailes>, 
					Or2<Where<Current<EmailProcessingFilter.type>, Equal<IncomingEmails>, And<EPActivity.isIncome, Equal<True>>>, 
					Or<Where<Current<EmailProcessingFilter.type>, Equal<OutgoingEmails>, And<EPActivity.isIncome, NotEqual<True>>>>>>,
				And<Where<Current<EmailProcessingFilter.ownerID>, IsNull,
						Or<EPActivity.owner, Equal<Current<EmailProcessingFilter.ownerID>>>>>>>>>>
			FilteredItems;

		#endregion

		#region Ctors

		public EmailProcessingMaint()
		{
			InitializeUI();

			InitializeProcessing();
		}

		private void InitializeUI()
		{
			PXUIFieldAttribute.SetDisplayName(Caches[typeof (EMailAccount)], typeof (EMailAccount.address).Name, "Account");
				//TODO: need review
			Actions.Move("Process", "Cancel");
			Actions.Move("Cancel", "Save");
		}

		private void InitializeProcessing()
		{
			FilteredItems.SetSelected<EPActivity.selected>();
			FilteredItems.SetProcessDelegate(CREmailActivityMaint.ProcessMessage);
		}

		#endregion

		#region Actions

		public PXCancel<EmailProcessingFilter> Cancel;

		public PXAction<EmailProcessingFilter> ViewDetails;

		[PXUIField(DisplayName = Objects.EP.Messages.ViewDetails, Visible = false)]
		[PXButton(ImageUrl = "~/Icons/Menu/entry_16.gif", DisabledImageUrl = "~/Icons/Menu/entry_16_NotActive.gif")]
		protected IEnumerable viewDetails(PXAdapter adapter)
		{
			var row = FilteredItems.Current;
			if (row != null)
			{
				var graph = CreateInstance<CREmailActivityMaint>();
				graph.Message.Current = graph.Message.Search<EPActivity.taskID>(row.TaskID);
				throw new PXRedirectRequiredException(graph, true, Objects.EP.Messages.ViewDetails)
					{
						Mode = PXBaseRedirectException.WindowMode.NewWindow
					};
			}
			return adapter.Get();
		}

		#endregion

		#region Event Handlers

		protected virtual void EmailProcessingFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as EmailProcessingFilter;
			if (row == null) return;

			var me = true.Equals(sender.GetValue(e.Row, typeof(EmailProcessingFilter.myOwner).Name));
			var myGroup = true.Equals(sender.GetValue(e.Row, typeof(EmailProcessingFilter.myWorkGroup).Name));

			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(EmailProcessingFilter.ownerID).Name, !me);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(EmailProcessingFilter.workGroupID).Name, !myGroup);
		}

		[PXDBString(255, InputMask = "", IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Subject", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXNavigateSelector(typeof(EPActivity.subject))]
		protected virtual void EPActivity_Subject_CacheAttached(PXCache sender)
		{
		}

		#endregion
	}
}
