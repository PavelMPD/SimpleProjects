using PX.Common;
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.CR
{
	public class NewLeadEmailProcessor : BasicEmailProcessor
	{
		protected override bool Process(Package package)
		{
			var account = package.Account;
			if (account.IncomingProcessing != true || 
				account.CreateLead != true)
			{
				return false;
			}

			var message = package.Message;
			if (message.IsIncome != true) return false;
			if (message.RefNoteID != null) return false;

			var leadCache = package.Graph.Caches[typeof(Contact)];
			var lead = (Contact)leadCache.Insert();
			lead.ClassID = ((CRSetup)PXSelect<CRSetup>.SelectWindowed(package.Graph, 0, 1)).With(_ => _.DefaultLeadClassID);
		    lead.ContactType = ContactTypesAttribute.Lead;
			lead.EMail = package.Address;
			lead.LastName = package.Description;
			if (lead.LastName == null || lead.LastName.Trim().Length == 0)
				lead.LastName = "From " + package.Address;
			lead = (Contact)leadCache.Update(lead);
			PersistRecord(package, lead);

			message.RefNoteID = lead.NoteID;

			return true;
		}
	}

	/*public class LeadCommonEmailProcessor : BasicEmailProcessor
	{
		protected override bool Process(Package package)
		{
			var account = package.Account;
			if (account.IncomingProcessing != true)
			{
				return false;
			}

			var message = package.Message;
			if (message.IsIncome != true) return false;
			if (message.RefNoteID != null) return false;


			var graph = package.Graph;

			PXSelect<Contact,
				Where<Contact.noteID, Equal<Required<Contact.noteID>>,
					And<Contact.contactType, Equal<ContactTypesAttribute.lead>>>>.
				Clear(graph);

			var contact = (Contact)PXSelect<Contact,
				Where<Contact.noteID, Equal<Required<Contact.noteID>>,
					And<Contact.contactType, Equal<ContactTypesAttribute.lead>>>>.
				Select(graph, message.RefNoteID);

			if (contact == null || contact.ContactID == null) return false;

			if (!RouterEmailProcessor.IsFromInternalUser(graph, message) &&
				contact.MajorStatus == LeadMajorStatusesAttribute._PENDING_CUSTOMER)
			{
				var contactCache = graph.Caches[typeof(Contact)];
				var newContact = (Contact)contactCache.CreateCopy(contact);
				newContact.MajorStatus = LeadMajorStatusesAttribute._OPEN;
				newContact.Status = LeadStatusesAttribute._OPEN;
				newContact.Resolution = LeadResolutionsAttribute._ANSWER_PROVIDER;
				newContact = (Contact)contactCache.Update(newContact);
				PersistRecord(package, newContact);
			}

			return true;
		}
	}*/
}
