using PX.Common;
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.CR
{
	public class ContactBAccountEmailProcessor : BasicEmailProcessor
	{
		protected override bool Process(Package package)
		{
			var account = package.Account;
			if (account.IncomingProcessing != true || 
				account.CreateActivity != true)
			{
				return false;
			}

			var message = package.Message;
			if (message.IsIncome != true) return false;
			if (message.RefNoteID != null) return false;

			PXSelect<Contact, 
				Where<Contact.eMail, Equal<Required<Contact.eMail>>>>.
				Clear(package.Graph);
			var contact = (Contact)PXSelect<Contact, 
				Where<Contact.eMail, Equal<Required<Contact.eMail>>>>.
				SelectWindowed(package.Graph, 0, 1, package.Address);
			if (contact != null && contact.ContactID != null)
			{
				PXSelect<BAccount,
					Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Clear(package.Graph);
				var accountNoteId = contact.BAccountID.
					With(_ => (BAccount)PXSelect<BAccount,
						Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
						Select(package.Graph, _.Value)).
					With(_ => _.NoteID);
				message.RefNoteID = contact.NoteID;
				message.ParentRefNoteID = accountNoteId;
				return true;
			}
			return false;
		}
	}
}
