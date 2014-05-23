using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using PX.Common;
using PX.Common.Mail;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.EP
{
	public class RouterEmailProcessor : BasicEmailProcessor
	{
		private class MailAddressList : IEnumerable<Mailbox>
		{
			private readonly HybridDictionary _items = new HybridDictionary();

			public void AddRange(IEnumerable<Mailbox> addresses)
			{
				if (addresses != null)
					foreach (Mailbox address in addresses)
						Add(address);
			}

			public void Add(Mailbox address)
			{
				var key = address.Address.With(_ => _.Trim()).With(_ => _.ToLower());
				if (!string.IsNullOrEmpty(key) && !_items.Contains(key)) 
					_items.Add(key, address);
			}

			public void Remove(Mailbox address)
			{
				var key = address.Address.With(_ => _.Trim()).With(_ => _.ToLower());
				if (_items.Contains(key))
					_items.Remove(key);
			}

			public IEnumerator<Mailbox> GetEnumerator()
			{
				foreach (DictionaryEntry item in _items)
					yield return (Mailbox)item.Value;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public int Count
			{
				get { return _items.Count; }
			}
		}

		protected override bool Process(Package package)
		{
			var graph = package.Graph;
			var message = package.Message;

			var isFromInternalUser = IsFromInternalUser(graph, message);
			var recipients = new MailAddressList();
			if (isFromInternalUser)
				recipients.AddRange(GetExternalRecipient(graph, message));
			else
				recipients.AddRange(GetInternalRecipient(graph, message));
			ExcludeMailboxes(recipients, message.MailFrom);
			ExcludeAddresses(recipients, message.MailTo);
			ExcludeAddresses(recipients, message.MailCc);
			ExcludeAddresses(recipients, message.MailBcc);
			if (recipients.Count == 0)
			{
				return false;
			}

			if (isFromInternalUser)
			{
				SendCopyMessageToOutside(graph, message, recipients);
				MarkAsRoutingEmail(graph, message);
			}
			else
			{
				SendCopyMessageToInside(graph, package.Account, message, recipients);
			}
			package.Graph.EnshureCachePersistance(message.GetType());

			return true;
		}

		private void ExcludeMailboxes(MailAddressList recipients, string addrStr)
		{
			if (string.IsNullOrEmpty(addrStr)) return;

			var addresses = MailboxList.Parse(addrStr);
			foreach (Mailbox address in addresses)
				recipients.Remove(address);
		}

		private void ExcludeAddresses(MailAddressList recipients, string addrStr)
		{
			if (string.IsNullOrEmpty(addrStr)) return;

			var addresses = AddressList.Parse(addrStr);
			foreach (PX.Common.Mail.Address address in addresses)
			{
				var group = address as Group;
				if (group != null)
					foreach (Mailbox mailbox in group.Members)
						recipients.Remove(mailbox);
				else
					recipients.Remove((Mailbox)address);
			}
		}

		private IEnumerable<Mailbox> GetExternalRecipient(PXGraph graph, EPActivity message)
		{
			var previousMessage = GetPreviousExternalMessage(graph, message);
			if (previousMessage != null)
				return GetExternalMailOutside(previousMessage);

			//TODO: need implementation
			//var source = FindSource(graph, (long)message.RefNoteID);
			return new Mailbox[0];
		}

		private object FindSource(PXGraph graph, long noteId)
		{
			return new EntityHelper(graph).GetEntityRow(noteId);
		}

		private void FindOwner(PXGraph graph, IAssign source, out Contact employee, out Users user)
		{
			employee = null;
			user = null;
			if (source == null || source.OwnerID == null) return;

			FindOwner(graph, source.OwnerID, out employee, out user);
		}

		private void FindOwner(PXGraph graph, Guid? ownerId, out Contact employee, out Users user)
		{
			employee = null;
			user = null;
			if (ownerId == null) return;

			PXSelectJoin<Users,
				LeftJoin<EPEmployee, On<EPEmployee.userID, Equal<Users.pKID>>,
				LeftJoin<Contact, On<Contact.contactID, Equal<EPEmployee.defContactID>>>>,
				Where<Users.pKID, Equal<Required<Users.pKID>>>>.
				Clear(graph);
			var row = (PXResult<Users, EPEmployee, Contact>)PXSelectJoin<Users,
				LeftJoin<EPEmployee, On<EPEmployee.userID, Equal<Users.pKID>>,
				LeftJoin<Contact, On<Contact.contactID, Equal<EPEmployee.defContactID>>>>,
				Where<Users.pKID, Equal<Required<Users.pKID>>>>.
				Select(graph, ownerId);

			employee = (Contact)row;
			user = (Users)row;
		}

		private IEnumerable<Mailbox> GetInternalRecipient(PXGraph graph, EPActivity message)
		{
			var previousMessage = GetPreviousInternalMessage(graph, message);

			if (previousMessage != null)
				return GetInternalMailOutside(previousMessage);

			var result = new List<Mailbox>();
			var initialMessage = GetParentMessage(graph, message);
			if (initialMessage != null)
			{
				var ownerAddress = GetOwnerAddress(graph, initialMessage.Owner);
				if (ownerAddress != null)
					result.Add(ownerAddress);
			}

			if (result.Count == 0)
			{
				var ownerAddress = GetOwnerAddress(graph, message.RefNoteID);
				if (ownerAddress != null) result.Add(ownerAddress);
				var parentOwnerAddress = GetOwnerAddress(graph, message.ParentRefNoteID);
				if (parentOwnerAddress != null) result.Add(parentOwnerAddress);
			}
			return result;
		}

		private Mailbox GetOwnerAddress(PXGraph graph, long? noteId)
		{
			if (noteId == null) return null;

			var source = FindSource(graph, (long) noteId);
			Contact owner;
			Users user;
			FindOwner(graph, source as IAssign, out owner, out user);
			var ownerAddress = GenerateAddress(owner, user);
			return ownerAddress;
		}

		private Mailbox GetOwnerAddress(PXGraph graph, Guid? ownerId)
		{
			Contact owner;
			Users user;
			FindOwner(graph, ownerId, out owner, out user);
			return GenerateAddress(owner, user);
		}

		private Mailbox GenerateAddress(Contact employee, Users user)
		{
			string displayName = null;
			string address = null;
			if (user != null && user.PKID != null)
			{
				var userDisplayName = user.FullName.With(_ => _.Trim());
				if (!string.IsNullOrEmpty(userDisplayName))
					displayName = userDisplayName;
				var userAddress = user.Email.With(_ => _.Trim());
				if (!string.IsNullOrEmpty(userAddress))
					address = userAddress;
			}
			if (employee != null && employee.BAccountID != null)
			{
				var employeeDisplayName = employee.DisplayName.With(_ => _.Trim());
				if (!string.IsNullOrEmpty(employeeDisplayName))
					displayName = employeeDisplayName;
				var employeeAddress = employee.EMail.With(_ => _.Trim());
				if (!string.IsNullOrEmpty(employeeAddress))
					address = employeeAddress;
			}
			if (string.IsNullOrEmpty(address)) return null;
			return new Mailbox(displayName, address);
		}

		private IEnumerable<Mailbox> GetInternalMailOutside(EPActivity message)
		{
			var prevActivity = message;
			if (prevActivity == null) yield break;

			if (prevActivity.IsIncome == true)
			{
				var mailFrom = prevActivity.MailFrom.With(_ => _.Trim());
				if (!string.IsNullOrEmpty(mailFrom))
					yield return Mailbox.Parse(mailFrom);
			}
			else
			{
				foreach (Mailbox address in ParseAddresses(prevActivity.MailTo))
					yield return address;
				foreach (Mailbox address in ParseAddresses(prevActivity.MailCc))
					yield return address;
				//TODO: need BCC
			}
		}

		private IEnumerable<Mailbox> GetExternalMailOutside(EPActivity message)
		{
			var prevActivity = message;
			if (prevActivity == null) yield break;

			if (prevActivity.IsIncome == true)
			{
				var mailFrom = prevActivity.MailFrom.With(_ => _.Trim());
				if (!string.IsNullOrEmpty(mailFrom))
					yield return Mailbox.Parse(mailFrom);
			}
			else
			{
				foreach (Mailbox address in ParseAddresses(prevActivity.MailTo))
					yield return address;
				foreach (Mailbox address in ParseAddresses(prevActivity.MailCc))
					yield return address;
				//TODO: need BCC
			}
		}

		private static IEnumerable<Mailbox> ParseAddresses(string address)
		{
			address = address.With(_ => _.Trim());
			if (!string.IsNullOrEmpty(address))
				foreach (PX.Common.Mail.Address item in AddressList.Parse(address))
				{
					var group = item as Group;
					if (group != null)
						foreach (Mailbox mailbox in group.Members)
							yield return mailbox;
					else yield return (Mailbox)item;
				}
		}

		private static EPActivity GetParentMessage(PXGraph graph, EPActivity message)
		{
			if (message.Ticket == null) return null;
			return SelectActivity(graph, message.Ticket);
		}

		private static EPActivity GetPreviousInternalMessage(PXGraph graph, EPActivity message)
		{
			if (message.Ticket == null) return null;

			EPActivity prevActivity = SelectActivity(graph, message.Ticket);
			while (prevActivity != null &&
				prevActivity.ClassID != CRActivityClass.EmailRouting)
			{
				prevActivity = SelectActivity(graph, prevActivity.ParentTaskID);
			}
			return prevActivity;
		}

		private static EPActivity GetPreviousExternalMessage(PXGraph graph, EPActivity message)
		{
			if (message.Ticket == null) return null;

			EPActivity prevActivity = SelectActivity(graph, message.Ticket);
			while (prevActivity != null && 
				prevActivity.ClassID == CRActivityClass.EmailRouting)
			{
				prevActivity = SelectActivity(graph, prevActivity.ParentTaskID);
			}
			return prevActivity;
		}

		private static EPActivity SelectActivity(PXGraph graph, int? taskId)
		{
			PXSelect<EPActivity,
				Where<EPActivity.taskID, Equal<Required<EPActivity.taskID>>>>.
				Clear(graph);
			var prevActivity = (EPActivity)PXSelect<EPActivity,
									Where<EPActivity.taskID, Equal<Required<EPActivity.taskID>>>>.
									Select(graph, taskId);
			return prevActivity;
		}

		private void GenerateOwnerAddress(PXGraph graph, Guid? owner, out string displayName, out string address)
		{
			displayName = null;
			address = null;
			if (owner == null) return;

			var records = PXSelectJoin<Users,
				LeftJoin<EPEmployee, On<EPEmployee.userID, Equal<Users.pKID>>,
				LeftJoin<Contact, On<Contact.contactID, Equal<EPEmployee.defContactID>>>>,
				Where<Users.pKID, Equal<Required<Users.pKID>>>>.
				SelectWindowed(graph, 0, 1, owner);
			if (records == null || records.Count == 0) return;

			var row = records[0];
			var employee = (Contact)row[typeof(Contact)];
			var user = (Users)row[typeof(Users)];
			if (user != null && user.PKID != null)
			{
				var userDisplayName = user.FullName.With(_ => _.Trim());
				if (!string.IsNullOrEmpty(userDisplayName))
					displayName = userDisplayName;
				var userAddress = user.Email.With(_ => _.Trim());
				if (!string.IsNullOrEmpty(userAddress))
					address = userAddress;
			}
			if (employee != null && employee.BAccountID != null)
			{
				var employeeDisplayName = employee.DisplayName.With(_ => _.Trim());
				if (!string.IsNullOrEmpty(employeeDisplayName))
					displayName = employeeDisplayName;
				var employeeAddress = employee.EMail.With(_ => _.Trim());
				if (!string.IsNullOrEmpty(employeeAddress))
					address = employeeAddress;
			}
		}

		private void MarkAsRoutingEmail(PXGraph graph, EPActivity message)
		{
			var cache = graph.Caches[message.GetType()];
			message.ClassID = CRActivityClass.EmailRouting;
			message.RefNoteID = null;
			message.ParentRefNoteID = null;
			cache.Update(message);
		}

		//TODO: need optimizae DB requests
		internal static bool IsFromInternalUser(PXGraph graph, EPActivity message)
		{
			var @from = Mailbox.Parse(message.MailFrom).With(_ => _.Address).With(_ => _.Trim());

			PXSelect<Users,
				Where2<Where<Users.guest, Equal<False>, Or<Users.guest, IsNull>>,
					And<Users.email, Equal<Required<Users.email>>>>>.
				Clear(graph);
			var usersEmail = (Users)PXSelect<Users,
					Where2<Where<Users.guest, Equal<False>, Or<Users.guest, IsNull>>,
						And<Users.email, Equal<Required<Users.email>>>>>.
						Select(graph, @from) != null;
			if (usersEmail) return true;

			PXSelectJoin<EPEmployee,
				LeftJoin<Contact, On<Contact.contactID, Equal<EPEmployee.defContactID>>>,
				Where<EPEmployee.userID, IsNotNull, And<Contact.eMail, Equal<Required<Contact.eMail>>>>>.
				Clear(graph);

			return (EPEmployee)PXSelectJoin<EPEmployee,
				LeftJoin<Contact, On<Contact.contactID, Equal<EPEmployee.defContactID>>>,
				Where<EPEmployee.userID, IsNotNull, And<Contact.eMail, Equal<Required<Contact.eMail>>>>>.
				Select(graph, @from) != null;
		}

		private void SendCopyMessageToOutside(PXGraph graph, EPActivity message, IEnumerable<Mailbox> email)
		{
			var cache = graph.Caches[message.GetType()];
			var copy = (EPActivity)cache.CreateCopy(message);
			copy.TaskID = null;
			copy.IsIncome = false;
			copy.ParentTaskID = message.TaskID;
			copy.MailTo = ConcatAddresses(email); //TODO: need add address description
			copy.MailCc = null;
			copy.MailBcc = null;
			copy.MPStatus = MailStatusListAttribute.PreProcess;
			copy.ClassID = CRActivityClass.Email;
			var imcUid = Guid.NewGuid();
			copy.ImcUID = imcUid;
			copy.MessageId = GetType().Name + "_" + imcUid.ToString().Replace("-", string.Empty);						
			copy.IsPrivate = message.IsPrivate;
			copy.Owner = null;
			copy = (EPActivity)cache.CreateCopy(cache.Insert(copy));			
			//Update owner and reset owner if employee not found
			copy.Owner = message.Owner;
			try
			{
				cache.Update(copy);
			}
			catch (PXSetPropertyException)
			{
				copy.Owner = null;
				cache.Update(copy);
			}
			
			var noteFiles = PXNoteAttribute.GetFileNotes(cache, message);
			if (noteFiles != null)
				PXNoteAttribute.SetFileNotes(cache, copy, noteFiles);
		}

		private void SendCopyMessageToInside(PXGraph graph, EMailAccount account, EPActivity message, IEnumerable<Mailbox> email)
		{
			var cache = graph.Caches[message.GetType()];
			var copy = (EPActivity)cache.CreateCopy(message);
			copy.TaskID = null;
			copy.IsIncome = false;
			copy.ParentTaskID = message.TaskID;
			copy.MailTo = ConcatAddresses(email); //TODO: need add address description
			copy.MailCc = null;
			copy.MailBcc = null;
			copy.MPStatus = MailStatusListAttribute.PreProcess;
			copy.ClassID = CRActivityClass.EmailRouting;
			new AddInfoEmailProcessor().Process(new EmailProcessEventArgs(graph, account, copy));
			copy.RefNoteID = null;
			copy.ParentRefNoteID = null;
			var imcUid = Guid.NewGuid();
			copy.ImcUID = imcUid;
			copy.MessageId = GetType().Name + "_" + imcUid.ToString().Replace("-", string.Empty);
			copy.Owner = message.Owner;
			copy.IsPrivate = message.IsPrivate;
			cache.Insert(copy);
			var noteFiles = PXNoteAttribute.GetFileNotes(cache, message);
			if (noteFiles != null)
				PXNoteAttribute.SetFileNotes(cache, copy, noteFiles);
		}

		private string ConcatAddresses(IEnumerable<Mailbox> addresses)
		{
			var list = new MailboxList();
			foreach (Mailbox address in addresses)
				list.Add(address);
			return list.ToString();
		}
	}
}
