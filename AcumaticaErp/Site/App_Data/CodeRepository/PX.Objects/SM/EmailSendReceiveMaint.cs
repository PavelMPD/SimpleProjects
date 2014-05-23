using System;
using System.Collections;
using PX.Data;
using PX.Objects.CR;

namespace PX.SM
{
	//[PXGraphName(PX.Objects.EP.Messages.EmailSendReceiveMaint)]
    [Serializable]
	public class EmailSendReceiveMaint : PXGraph<EmailSendReceiveMaint>
	{
		#region OperationFilter

        [Serializable]
		public partial class OperationFilter : IBqlTable
		{
			#region Operation

			public abstract class operation : IBqlField { }

			[PXAutomationMenu]
			public virtual String Operation { get; set; }

			#endregion
		}

		#endregion

		#region Selects

		[PXHidden]
		public PXFilter<OperationFilter>
			Filter;

		[PXCacheName(PX.Objects.EP.Messages.Emails)]
		[PXFilterable]
		public PXFilteredProcessing<EMailAccount, OperationFilter>
			FilteredItems;

		public IEnumerable filteredItems()
		{
			foreach (EMailAccount account in PXSelect<EMailAccount>.Select(this))
			{
				if (account.Address != null)
				{
					if (!String.IsNullOrEmpty(account.Address.Trim()))
					{
						yield return account;
					}
				}
			}
		}
		#endregion


		#region Ctors

		public EmailSendReceiveMaint()
		{
			CorrectUI();
			InitializeProcessing();
		}

		private void CorrectUI()
		{
			Actions.Move("Process", "Cancel");
			Actions.Move("Cancel", "Save");
		}

		private void InitializeProcessing()
		{
			FilteredItems.SetSelected<EMailAccount.selected>();

			PXProcessingStep[] targets = PXAutomation.GetProcessingSteps(this);
			if (targets.Length > 0)
			{
				FilteredItems.SetProcessTarget(targets[0].GraphName,
					targets.Length > 1 ? null : targets[0].Name,
					targets[0].Actions[0].Name,
					targets[0].Actions[0].Menus[0],
					null, null);
			}
			else
			{
				throw new PXException(PX.Objects.SO.Messages.MissingMassProcessWorkFlow);
			}
		}

		#endregion

		#region Actions

		public PXCancel<OperationFilter> Cancel;

		public PXAction<OperationFilter> ViewDetails;

		[PXUIField(DisplayName = PX.Objects.EP.Messages.ViewDetails, Visible = false)]
		[PXButton(ImageUrl = "~/Icons/Menu/entry_16.gif", DisabledImageUrl = "~/Icons/Menu/entry_16_NotActive.gif")]
		protected IEnumerable viewDetails(PXAdapter adapter)
		{
			var cache = FilteredItems.Cache;
			var row = FilteredItems.Current;
			if (row != null)
				PXRedirectHelper.TryOpenPopup(cache, row, PX.Objects.EP.Messages.ViewDetails);
			return adapter.Get();
		}

		#endregion

		#region Event Handlers

		public virtual void OperationFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as OperationFilter;
			if (row == null) return;

			if (row.Operation == PXAutomationMenuAttribute.Undefined)
			{
				Actions["Process"].SetEnabled(false);
				Actions["ProcessAll"].SetEnabled(false);
			}
			else
			{
				Actions["Process"].SetEnabled(true);
				Actions["ProcessAll"].SetEnabled(true);
			}
			FilteredItems.SetProcessTarget(null, null, null, row.Operation);
		}

		public virtual void EMailAccount_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as EMailAccount;
			if (row == null) return;

			var outbox = PXSelectGroupBy<EPActivity,
                Where<EPActivity.mailAccountID, Equal<Required<EPActivity.mailAccountID>>,
					And<EPActivity.mpstatus, Equal<MailStatusListAttribute.preProcess>,
					And<EPActivity.isIncome, NotEqual<True>>>>,
				Aggregate<Count>>.
				Select(this, row.EmailAccountID).
				RowCount ?? 0;
			row.OutboxCount = outbox;

			var inbox = PXSelectGroupBy<EPActivity,
				Where<EPActivity.mailAccountID, Equal<Required<EPActivity.mailAccountID>>,
					And<EPActivity.mpstatus, Equal<MailStatusListAttribute.preProcess>,
					And<EPActivity.isIncome, Equal<True>>>>,
				Aggregate<Count>>.
                Select(this, row.EmailAccountID).
				RowCount ?? 0;
			row.InboxCount = inbox;
		}

		#endregion
	}
}
