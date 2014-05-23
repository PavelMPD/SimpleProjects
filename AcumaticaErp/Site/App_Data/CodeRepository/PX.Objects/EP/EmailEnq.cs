using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.EP
{
	#region NavigationFilter

	[Serializable]
	public class NavigationFilter : IBqlTable
	{
		public abstract class email : IBqlField { }

		[PXString]
		public String Email { get; set; }
	}

	#endregion

	//[PXGraphName(Messages.EmailsEnq, typeof(EPActivity))]  
    [Serializable]
	public class EmailEnq : PXGraph<EmailEnq>
	{
		#region Box

		[PXCacheName(Messages.MailBox)]
		[PXVirtual]
        [Serializable]
		public partial class Box : IBqlTable
		{
			#region Key

			public abstract class key : IBqlField { }
			[PXString(IsKey = true)]
			[PXUIField(Visible = false)]
			public virtual string Key { get; set; }
			#endregion

			#region Folder
			[PXUIField(Visible = false)]
			public virtual int Folder { get; set; }
			#endregion

			#region Name

			public abstract class name : IBqlField { }

			[PXString]
			[PXUIField(DisplayName = "Name")]
			public virtual string Name { get; set; }

			#endregion

			#region Icon

			public abstract class icon : IBqlField { }

			[PXString]
			[PXUIField(Visible = false)]
			public virtual string Icon { get; set; }

			#endregion
		}

		#endregion

		#region MailBoxFilter

		[PXHidden]
        [Serializable]
		public partial class MailBoxFilter : IBqlTable
		{
			#region FilterID

			public abstract class filterID : IBqlField { }

			[PXDBIdentity(IsKey = true)]
			[PXUIField(Visible = false)]
			public virtual int? FilterID { get; set; }

			#endregion

			#region Name

			public abstract class name : IBqlField { }

			[PXString]
			[PXUIField(DisplayName = "Name")]
			public virtual string Name { get; set; }

			#endregion

			#region Icon

			public abstract class icon : IBqlField { }

			[PXString]
			[PXUIField(Visible = false)]
			public virtual string Icon { get; set; }

			#endregion
		}

		#endregion

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
		}

		#endregion

		#region Constants

		private const int _INBOX_FID = 0;
		private const int _DRAFT_FID = 1;
		private const int _SENT_FID = 2;
		private const int _OUTBOX_FID = 3;
		private const int _REMOVED_FID = 4;

		#endregion

		#region Selects

		[PXViewName(Messages.MailBoxes)]
		public PXSelect<Box>
			Boxes;

		[PXViewName(Messages.Selection)]
		public PXFilter<EmailFilter>
			Filter;

		[PXViewName(Messages.Emails)]
		[PXFilterable]
		public PXSelect<EPActivity,
			Where2<
				Where<EPActivity.classID, Equal<CRActivityClass.email>, 
					Or<EPActivity.classID, Equal<CRActivityClass.emailRouting>>>,
				And<EPActivity.mailAccountID, IsNotNull,
				And<Where<Current<EmailFilter.searchText>, IsNull,
						Or<EPActivity.subject, Like<Current<EmailFilter.likeText>>,
						Or<EPActivity.body, Like<Current<EmailFilter.likeText>>,
						Or<EPActivity.mailFrom, Like<Current<EmailFilter.likeText>>,
						Or<EPActivity.mailTo, Like<Current<EmailFilter.likeText>>,
						Or<EPActivity.mailCc, Like<Current<EmailFilter.likeText>>,
						Or<EPActivity.mailBcc, Like<Current<EmailFilter.likeText>>>>>>>>>>>>,
						OrderBy<Desc<EPActivity.processDate>>> 
			Emails; //TODO: need search by EmailAccount.Address as well

		[PXHidden]
		public PXFilter<NavigationFilter>
			NavigationFilter;
					
		#endregion

		#region Actions

		public PXCancel<EmailFilter>
			Cancel;

		public PXAction<EmailFilter>
			SendAll;

		[PXUIField(DisplayName = "Send All")]
		[PXButton(ImageUrl = "~/Icons/Menu/mailSend.gif", DisabledImageUrl = "~/Icons/Menu/mailSendD.gif")]
		protected IEnumerable sendAll(PXAdapter adapter)
		{
			PXLongOperation.StartOperation(this.UID, delegate() { TryProcessBox("SendAll"); });
			return adapter.Get();
		}

		public PXAction<EmailFilter>
			ReceiveAll;

		[PXUIField(DisplayName = "Receive All")]
		[PXButton(ImageUrl = "~/Icons/Menu/mailReceive.gif", DisabledImageUrl = "~/Icons/Menu/mailReceiveD.gif")]
		protected IEnumerable receiveAll(PXAdapter adapter)
		{
			PXLongOperation.StartOperation(this.UID, delegate() { TryProcessBox("ReceiveAll"); });
			return adapter.Get();
		}

		public PXAction<EmailFilter>
			SendReceiveAll;

		[PXUIField(DisplayName = "Send/Receive All")]
		[PXButton(ImageUrl = "~/Icons/Menu/mailSendReceive.gif", DisabledImageUrl = "~/Icons/Menu/mailSendReceiveD.gif")]
		protected IEnumerable sendReceiveAll(PXAdapter adapter)
		{
			PXLongOperation.StartOperation(this.UID, delegate() { TryProcessBox("SendReceiveAll"); });
			return adapter.Get();
		}

		public PXAction<EmailFilter>
			CreateNew;

		[PXUIField(DisplayName = PX.Objects.EP.Messages.CreateNew)]
		[PXButton(ImageUrl = "~/Icons/Menu/entry_16.gif", DisabledImageUrl = "~/Icons/Menu/entry_16_NotActive.gif")]
		protected IEnumerable createNew(PXAdapter adapter)
		{
			int? accountId;
			int? filterId;
			DecodeBoxKey(Boxes.Current.Key, out accountId, out filterId);
			PX.Data.EP.ActivityService.CreateEmailActivity(null, (int)accountId);
			Persist();
			return adapter.Get();
		}

		public PXAction<EmailFilter>
			Delete;

		[PXUIField(DisplayName = PX.Objects.EP.Messages.Delete)]
		[PXDeleteButton(ConfirmationMessage = ActionsMessages.ConfirmDelete)]
		protected IEnumerable delete(PXAdapter adapter)
		{
			bool isanyselected = false;
			var DeleteList = new List<EPActivity>();
			foreach (EPActivity email in Emails.Select())
			{
				if (email != null)
				{
					if (email.Selected == true)
					{
						DeleteList.Add(email);
						isanyselected = true;
					}
				}
			}
			if (isanyselected == false)
			{
				if (Emails.Current != null)
				{
					DeleteList.Add(Emails.Current);
				}
			}
			foreach (var accountmessages in DeleteList)
			{
				Emails.Cache.Delete(accountmessages);
			}
			Persist();
			return adapter.Get();
		}

		public PXAction<EmailFilter>
			ViewDetails;

		[PXUIField(DisplayName = PX.Objects.EP.Messages.ViewDetails)]
		[PXButton(ImageUrl = "~/Icons/Menu/entry_16.gif", DisabledImageUrl = "~/Icons/Menu/entry_16_NotActive.gif")]
		protected IEnumerable viewDetails(PXAdapter adapter)
		{
			var row = Emails.Current;
			if (row != null)
			{
				var graph = PXGraph.CreateInstance<CREmailActivityMaint>();
				graph.Message.Current = graph.Message.Search<EPActivity.taskID>(row.TaskID);
				throw new PXRedirectRequiredException(graph, true, PX.Objects.EP.Messages.ViewDetails)
				{
					Mode = PXBaseRedirectException.WindowMode.NewWindow
				};
			}
			return adapter.Get();
		}

		public PXAction<EmailFilter> Process;

		[PXUIField(DisplayName = EP.Messages.Process)]
		[PXButton(Tooltip = EP.Messages.ttipProcess)]
		protected virtual IEnumerable process(PXAdapter a)
		{
			automatically(a);
			return a.Get();
		}

		public PXAction<EmailFilter> Automatically;
		[PXUIField(DisplayName = EP.Messages.Automatically)]
		[PXButton(ImageUrl = "", Tooltip = EP.Messages.ttipAutomatically)]
		protected virtual IEnumerable automatically(PXAdapter a)
		{
			PXLongOperation.StartOperation(this.UID, delegate() { automaticallyProcess(); });
			return a.Get();
		}

		public PXAction<EmailFilter> ConvertToLead;
		[PXButton(Tooltip = EP.Messages.ttipConvertToLead)]
		[PXUIField(DisplayName = EP.Messages.ConvertToLead)]
		protected virtual IEnumerable convertToLead(PXAdapter a)
		{
			PXLongOperation.StartOperation(this.UID, delegate() { convertToLeadProcess(); });
			return a.Get();
		}

		public PXAction<EmailFilter> ConvertToCase;
		[PXButton(Tooltip = EP.Messages.ttipConvertToCase)]
		[PXUIField(DisplayName = EP.Messages.ConvertToCase)]
		protected virtual IEnumerable convertToCase(PXAdapter a)
		{
			PXLongOperation.StartOperation(this.UID, delegate() { convertToCaseProcess(); });
			return a.Get();
		}

		public PXAction<EmailFilter> Resend;
		[PXButton(Tooltip = EP.Messages.ttipsend)]
		[PXUIField(DisplayName = EP.Messages.Resend)]
		protected virtual IEnumerable resend(PXAdapter a)
		{
			PXLongOperation.StartOperation(this.UID, delegate() { ResendProcess(); });
			return a.Get();
		}


		#endregion

		#region Data Handlers

		protected virtual IEnumerable boxes(
			[PXString]
			string key)
		{
			int? accountId;
			int? filterId;
			DecodeBoxKey(key, out accountId, out filterId);

			if (accountId == null)
				return EnumerateBoxes();

			if (filterId == null)
				return EnumerateFiltes((int)accountId);

			return new Box[0];
		}

		protected virtual IEnumerable emails(
			[PXString]
			string boxKey)
		{
			int? accountId;
			int? filterId;
			DecodeBoxKey(boxKey, out accountId, out filterId);

			var command = Emails.View.BqlSelect;
			var parameters = new List<object>();

			List<int> classIds = new List<int>();
			foreach (Box Box in EnumerateBoxes())
			{
				int? val;
				int? val2;
				DecodeBoxKey(Box.Key, out val, out val2);
				classIds.Add((int)val);
			}
			if (classIds != null && classIds.Count > 0)
			{
                command = command.WhereAnd(InHelper<EPActivity.mailAccountID>.Create(classIds.Count));
				foreach (int classId in classIds)
					parameters.Add(classId);
			}

			var account = accountId.With(_ => (EMailAccount)PXSelect<EMailAccount,
				Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.
				Select(this, _.Value));
			if (account != null)
			{
				command = command.WhereAnd(typeof(Where<EPActivity.mailAccountID, Equal<Required<EMailAccount.emailAccountID>>>));
				parameters.Add(account.EmailAccountID);
			}

			if (filterId != null)
				command = AppendEmailFilter((int)filterId, command);

			if (boxKey != null)
			{
				string[] arr;
				if (!string.IsNullOrEmpty(boxKey) && (arr = boxKey.Split('$')).Length == 2)
				{
					Boxes.Current = Boxes.Select(String.Concat(arr[0], "$"));
					if (!String.IsNullOrEmpty(arr[1]))
						Boxes.Current.Folder = Convert.ToInt32(arr[1]);
					else
						Boxes.Current.Folder = -1;
					switch (Boxes.Current.Folder)
					{
						case _INBOX_FID:
							PXUIFieldAttribute.SetVisible<EPActivity.mailFrom>(Caches[typeof(EPActivity)], null, true);
							PXUIFieldAttribute.SetVisible<EPActivity.mailTo>(Caches[typeof(EPActivity)], null, false);
							break;
						case _DRAFT_FID:
							PXUIFieldAttribute.SetVisible<EPActivity.mailFrom>(Caches[typeof(EPActivity)], null, false);
							PXUIFieldAttribute.SetVisible<EPActivity.mailTo>(Caches[typeof(EPActivity)], null, true);
							break;
						case _SENT_FID:
							PXUIFieldAttribute.SetVisible<EPActivity.mailFrom>(Caches[typeof(EPActivity)], null, false);
							PXUIFieldAttribute.SetVisible<EPActivity.mailTo>(Caches[typeof(EPActivity)], null, true);
							break;
						case _OUTBOX_FID:
							PXUIFieldAttribute.SetVisible<EPActivity.mailFrom>(Caches[typeof(EPActivity)], null, false);
							PXUIFieldAttribute.SetVisible<EPActivity.mailTo>(Caches[typeof(EPActivity)], null, true);
							break;
						case _REMOVED_FID:
							PXUIFieldAttribute.SetVisible<EPActivity.mailFrom>(Caches[typeof(EPActivity)], null, true);
							PXUIFieldAttribute.SetVisible<EPActivity.mailTo>(Caches[typeof(EPActivity)], null, true);
							break;
						default:
							PXUIFieldAttribute.SetVisible<EPActivity.mailFrom>(Caches[typeof(EPActivity)], null, true);
							PXUIFieldAttribute.SetVisible<EPActivity.mailTo>(Caches[typeof(EPActivity)], null, true);
							break;
					}
				}
			}
			else
			{
				Boxes.Current = Boxes.Select(boxKey);
			}
			return this.QuickSelect(command, parameters.ToArray());
		}

		private BqlCommand AppendEmailFilter(int filterId, BqlCommand command)
		{
			//TODO: need implement using DB filters
			switch (filterId)
			{
				case _INBOX_FID:
					return command.WhereAnd(typeof(Where<EPActivity.isIncome, Equal<True>,
													And<EPActivity.mpstatus, NotEqual<ActivityStatusAttribute.canceled>>>));
				case _DRAFT_FID:
					return command.WhereAnd(typeof(Where2<Where<EPActivity.isIncome, IsNull,
															Or<EPActivity.isIncome, Equal<False>>>,
														And<EPActivity.mpstatus, Equal<MailStatusListAttribute.draft>>>));
				case _SENT_FID:
					return command.WhereAnd(typeof(Where2<Where<EPActivity.isIncome, IsNull,
															Or<EPActivity.isIncome, Equal<False>>>,
														And<EPActivity.mpstatus, Equal<MailStatusListAttribute.processed>>>));
				case _OUTBOX_FID:
					return command.WhereAnd(typeof(Where2<Where<EPActivity.isIncome, IsNull,
															Or<EPActivity.isIncome, Equal<False>>>,
														And<EPActivity.mpstatus, NotEqual<MailStatusListAttribute.processed>,
														And<EPActivity.mpstatus, NotEqual<MailStatusListAttribute.canceled>>>>));
				case _REMOVED_FID:
					return command.WhereAnd(typeof(Where<EPActivity.mpstatus, Equal<MailStatusListAttribute.canceled>>));
			}
			return command;
		}

		#endregion

		#region Protected Methods

		protected virtual string GenerateMailBody()
		{
			string res = null;
			var signature = ((UserPreferences)PXSelect<UserPreferences>.
				Search<UserPreferences.userID>(this, PXAccess.GetUserID())).
				With(pref => pref.MailSignature);
			if (signature != null && (signature = signature.Trim()) != string.Empty)
				res += "<br />" + signature;
			return res;
		}

		protected void DecodeBoxKey(string key, out int? accountId, out int? filterId)
		{
			accountId = null;
			filterId = null;

			string[] arr;
			if (!string.IsNullOrEmpty(key) && (arr = key.Split('$')).Length == 2)
			{
				int acctVal;
				if (int.TryParse(arr[0], out acctVal))
					accountId = acctVal;
				int filterVal;
				if (int.TryParse(arr[1], out filterVal))
					filterId = filterVal;
			}
		}

		protected string EncodeBoxKey(int? accountId, int? filterId)
		{
			return string.Concat(accountId, "$", filterId);
		}

		#endregion

		#region Private Methods

		private IEnumerable EnumerateFiltes(int accountId)
		{
			foreach (MailBoxFilter filter in
				GetBoxFilters(accountId))
			{
				var key = EncodeBoxKey(accountId, filter.FilterID);
				yield return new Box { Key = key, Name = filter.Name, Icon = filter.Icon };
			}
		}

		private IEnumerable GetBoxFilters(int accountId)
		{
			int accounttype = 0;
			EMailAccount _account = (EMailAccount)PXSelect<EMailAccount,
				Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.Select(this, accountId);
			if (_account != null)
			{
				if (String.IsNullOrEmpty(_account.OutcomingHostName))
					accounttype = 1;
				if (String.IsNullOrEmpty(_account.IncomingHostName))
					accounttype = 2;
				if (String.IsNullOrEmpty(_account.OutcomingHostName) && String.IsNullOrEmpty(_account.OutcomingHostName))
					accounttype = 3;
				switch (accounttype)
				{
					case 0:
						yield return new MailBoxFilter { FilterID = _INBOX_FID, Name = Messages.InboxBox, Icon = "~/Icons/Menu/inboxBox.gif" };
						yield return new MailBoxFilter { FilterID = _DRAFT_FID, Name = Messages.DraftBox, Icon = "~/Icons/Menu/draftBox.gif" };
						yield return new MailBoxFilter { FilterID = _SENT_FID, Name = Messages.SentBox, Icon = "~/Icons/Menu/sentBox.gif" };
						yield return new MailBoxFilter { FilterID = _OUTBOX_FID, Name = Messages.OutboxBox, Icon = "~/Icons/Menu/outboxBox.gif" };
						yield return new MailBoxFilter { FilterID = _REMOVED_FID, Name = Messages.RemovedBox, Icon = "~/Icons/Menu/removedBox.gif" };
						break;
					case 1:
						yield return new MailBoxFilter { FilterID = _INBOX_FID, Name = Messages.InboxBox, Icon = "~/Icons/Menu/inboxBox.gif" };
						yield return new MailBoxFilter { FilterID = _REMOVED_FID, Name = Messages.RemovedBox, Icon = "~/Icons/Menu/removedBox.gif" };
						break;
					case 2:
						yield return new MailBoxFilter { FilterID = _DRAFT_FID, Name = Messages.DraftBox, Icon = "~/Icons/Menu/draftBox.gif" };
						yield return new MailBoxFilter { FilterID = _SENT_FID, Name = Messages.SentBox, Icon = "~/Icons/Menu/sentBox.gif" };
						yield return new MailBoxFilter { FilterID = _OUTBOX_FID, Name = Messages.OutboxBox, Icon = "~/Icons/Menu/outboxBox.gif" };
						yield return new MailBoxFilter { FilterID = _REMOVED_FID, Name = Messages.RemovedBox, Icon = "~/Icons/Menu/removedBox.gif" };
						break;
					case 3:
						break;
				}
			}
			else // all mail box
			{
				yield return new MailBoxFilter { FilterID = _INBOX_FID, Name = Messages.InboxBox, Icon = "~/Icons/Menu/inboxBox.gif" };
				yield return new MailBoxFilter { FilterID = _DRAFT_FID, Name = Messages.DraftBox, Icon = "~/Icons/Menu/draftBox.gif" };
				yield return new MailBoxFilter { FilterID = _SENT_FID, Name = Messages.SentBox, Icon = "~/Icons/Menu/sentBox.gif" };
				yield return new MailBoxFilter { FilterID = _OUTBOX_FID, Name = Messages.OutboxBox, Icon = "~/Icons/Menu/outboxBox.gif" };
				yield return new MailBoxFilter { FilterID = _REMOVED_FID, Name = Messages.RemovedBox, Icon = "~/Icons/Menu/removedBox.gif" };
			}
		}

		private IEnumerable EnumerateBoxes()
		{
			foreach (EMailAccount account in GetAccounts())
			{
				if (account.Address != null)
				{
					if (!String.IsNullOrEmpty(account.Address.Trim()))
					{
						if(!(String.IsNullOrEmpty(account.IncomingHostName) && String.IsNullOrEmpty(account.OutcomingHostName)))
						{
							var key = EncodeBoxKey(account.EmailAccountID, null);
							yield return new Box { Key = key, Name = account.Address, Icon = "~/Icons/Menu/mailBox.gif" };
						}
					}
				}
			}
			var mainkey = EncodeBoxKey(0, null);
			yield return new Box { Key = mainkey, Name = "All Mail Boxes", Icon = "~/Icons/Menu/mailBox.gif" };
		}

		private IEnumerable GetAccounts()
		{
			return PXSelect<EMailAccount,
				Where<EMailAccount.userName, Equal<Current<AccessInfo.userName>>,
					Or<EMailAccount.userName, IsNull>>,
				OrderBy<Asc<EMailAccount.description>>>.
				Select(this).
				RowCast<EMailAccount>();
		}

		private void TryProcessBox(string actionName)
		{
			var current = Boxes.Current;
			if (current != null)
			{
				int? accountId;
				int? filterId;
				DecodeBoxKey(current.Key, out accountId, out filterId);
				PressAccountButton(accountId, actionName);
			}
		}

		private void automaticallyProcess()
		{
			bool isanyselected = false;

			var ReceiveDictionary = new Dictionary<int, List<EPActivity>>();
			var SendDictionary = new Dictionary<int, List<EPActivity>>();

			EPActivity currentemail = Emails.Current;
			foreach (EPActivity email in Emails.Select())
			{
				if (email != null)
				{
					if (email.Selected == true && IsMailProccessable(email))
					{
						if (email.IsIncome == true)
						{
							if(ReceiveDictionary.ContainsKey((int)email.MailAccountID))
							{
								ReceiveDictionary[(int)email.MailAccountID].Add(email);
							}
							else
							{
								ReceiveDictionary.Add((int)email.MailAccountID, new List<EPActivity>());
								ReceiveDictionary[(int)email.MailAccountID].Add(email);
							}
						}
						else
						{
							if (SendDictionary.ContainsKey((int)email.MailAccountID))
							{
								SendDictionary[(int)email.MailAccountID].Add(email);
							}
							else
							{
								SendDictionary.Add((int)email.MailAccountID, new List<EPActivity>());
								SendDictionary[(int)email.MailAccountID].Add(email);
							}
						}
						isanyselected = true;
					}
				}
			}
			if (isanyselected == false)
			{
				if (currentemail != null && IsMailProccessable(currentemail))
				{
					if (currentemail.IsIncome == true)
					{
						ReceiveDictionary.Add((int)currentemail.MailAccountID, new List<EPActivity>());
						ReceiveDictionary[(int)currentemail.MailAccountID].Add(currentemail);
					}
					else
					{
						SendDictionary.Add((int)currentemail.MailAccountID, new List<EPActivity>());
						SendDictionary[(int)currentemail.MailAccountID].Add(currentemail);
					}
				}
			}
			foreach (var accountmessages in ReceiveDictionary)
			{
				foreach (var message in ReceiveDictionary[accountmessages.Key])
				{
					EMailMessageReceiver.ProcessMessage(message);
				}
			}
			foreach (var accountmessages in SendDictionary)
			{
				foreach (var message in SendDictionary[accountmessages.Key])
				{
					MailSendProvider.SendMessage(message);
				}
			}
		}

		private bool IsMailProccessable(EPActivity email)
		{
			return email.MPStatus == MailStatusListAttribute.Processed ||
				email.MPStatus == MailStatusListAttribute.Failed;
		}

		private void convertToLeadProcess()
		{
			var Receive = new CommonMailReceiveProvider();
			var MailDictionary = GetMailByAccount();
			foreach (var accountmessages in MailDictionary)
			{
				EMailAccount emailaccount = PXSelect<EMailAccount,
					Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.Select(this, accountmessages.Key)[0];
				emailaccount.CreateLead = true;
				emailaccount.IncomingProcessing = true;

				emailaccount.ConfirmReceipt = false;
				emailaccount.CreateActivity = false;
				emailaccount.CreateCase = false;
				emailaccount.ProcessUnassigned = false;
				emailaccount.DeleteUnProcessed = false;

				foreach (var message in MailDictionary[accountmessages.Key])
				{
					if(message.IsIncome == true)
					{
						message.RefNoteID = null;
						message.ParentRefNoteID = null;
						Receive.ProcessMessage(emailaccount, message);
					}
				}
			}
		}

		private void convertToCaseProcess()
		{
			var Receive = new CommonMailReceiveProvider();
			var MailDictionary = GetMailByAccount();
			foreach (var accountmessages in MailDictionary)
			{
				EMailAccount emailaccount = PXSelect<EMailAccount,
					Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.Select(this, accountmessages.Key)[0];
				emailaccount.CreateCase = true;
				emailaccount.IncomingProcessing = true;

				emailaccount.ConfirmReceipt = false;
				emailaccount.CreateActivity = false;
				emailaccount.CreateLead = false;
				emailaccount.ProcessUnassigned = false;
				emailaccount.DeleteUnProcessed = false;
				foreach (var message in MailDictionary[accountmessages.Key])
				{
					if (message.IsIncome == true)
					{
						message.RefNoteID = null;
						message.ParentRefNoteID = null;
						Receive.ProcessMessage(emailaccount, message);
					}
				}
			}
		}

		private void ResendProcess()
		{
			bool isanyselected = false;
			var Send = new CommonMailSendProvider();

			var SendDictionary = new Dictionary<int, List<EPActivity>>();

			EPActivity currentemail = Emails.Current;
			foreach (EPActivity email in Emails.Select())
			{
				if (email != null)
				{
					if (email.Selected == true)
					{
						if (email.IsIncome != true)
						{
							if (IsMailProccessable(email))
							{
								email.MPStatus = MailStatusListAttribute.PreProcess;
								if (SendDictionary.ContainsKey((int)email.MailAccountID))
								{
									SendDictionary[(int)email.MailAccountID].Add(email);
								}
								else
								{
									SendDictionary.Add((int)email.MailAccountID, new List<EPActivity>());
									SendDictionary[(int)email.MailAccountID].Add(email);
								}
							}
						}
						isanyselected = true;
					}
				}
			}
			if (isanyselected == false)
			{
				if (currentemail != null)
				{
					if (currentemail.IsIncome != true)
					{
						if (IsMailProccessable(currentemail))
						{
							currentemail.MPStatus = MailStatusListAttribute.PreProcess;
							SendDictionary.Add((int)currentemail.MailAccountID, new List<EPActivity>());
							SendDictionary[(int)currentemail.MailAccountID].Add(currentemail);
						}
					}
				}
			}
			foreach (var accountmessages in SendDictionary)
			{
				foreach (var message in SendDictionary[accountmessages.Key])
				{
					Send.SendMessage(message);
				}
			}
		}

		private Dictionary<int, List<EPActivity>> GetMailByAccount()
		{
			bool isanyselected = false;
			var MailDictionary = new Dictionary<int, List<EPActivity>>();
			EPActivity currentemail = Emails.Current;
			foreach (EPActivity email in Emails.Select())
			{
				if (email != null)
				{
					if (email.Selected == true)
					{
						if (MailDictionary.ContainsKey((int)email.MailAccountID))
						{
							MailDictionary[(int)email.MailAccountID].Add(email);
						}
						else
						{
							MailDictionary.Add((int)email.MailAccountID, new List<EPActivity>());
							MailDictionary[(int)email.MailAccountID].Add(email);
						}
						isanyselected = true;
					}
				}
			}
			if (isanyselected == false)
			{
				if (currentemail != null)
				{
					MailDictionary.Add((int)currentemail.MailAccountID, new List<EPActivity>());
					MailDictionary[(int)currentemail.MailAccountID].Add(currentemail);
				}
			}
			return MailDictionary;
		}
		

		private void PressAccountButton(int? accountId, string actionName)
		{
			var account = accountId.With(_ => (EMailAccount)PXSelect<EMailAccount,
					Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.
					Select(this, _.Value));
			if (account != null)
			{
				PressAccountButtonForOneEmail(actionName, account);
			}
			else
			{
				String erroremail = null;
				foreach (EMailAccount _account in GetAccounts())
				{
					if (_account.Address != null)
					{
						if (!String.IsNullOrEmpty(_account.Address.Trim()) && !(actionName == "ReceiveAll" && String.IsNullOrEmpty(_account.IncomingHostName)))
						{
							PressAccountButtonForOneEmail(actionName, _account);
						}
						else
						{
							erroremail = _account.Address;
						}
					}
				}
				if (!String.IsNullOrEmpty(erroremail))
				{
					throw new ArgumentException(string.Format("Incoming Mail Server for '{0}' is empty", erroremail));
				}
			}
		}

		private void PressAccountButtonForOneEmail(string actionName, EMailAccount account)
		{
			var graph = CreateInstance<EMailAccountMaint>();
			graph.EMailAccounts.Current = account;
			var action = graph.Actions[actionName];
			if (action == null)
				throw new ArgumentException(string.Format("Cannot find action '{0}' in graph '{1}'", actionName, graph.GetType().Name));
			var adapter = new PXAdapter(graph.CurrentEMailAccounts);
			var iterator = action.Press(adapter).GetEnumerator();
			while (iterator.MoveNext()) { }
		}
		#endregion
	}
}
