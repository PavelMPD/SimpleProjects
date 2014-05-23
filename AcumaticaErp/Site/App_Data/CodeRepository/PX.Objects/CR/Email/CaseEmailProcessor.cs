using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.CR
{
	public class NewCaseEmailProcessor : BasicEmailProcessor
	{
		protected override bool Process(Package package)
		{
			var account = package.Account;
			if (account.IncomingProcessing != true ||
			    account.CreateCase != true)
			{
				return false;
			}

			var message = package.Message;
			if (message.IsIncome != true) return false;
			if (message.RefNoteID != null) return false;


			var graph = package.Graph;
			SetCRSetup(graph);
			SetAccessInfo(graph);

			var caseCache = graph.Caches[typeof (CRCase)];
			var @case = (CRCase) caseCache.Insert();
			@case.MajorStatus = CRCaseMajorStatusesAttribute._NEW;
			@case.Status = CRCaseStatusesAttribute._NEW;
			//@case.EMail = package.Address;
			@case.Subject = message.Subject;
			if (@case.Subject == null || @case.Subject.Trim().Length == 0)
				@case.Subject = GetFromString(package.Address, package.Description);
			@case.Description = message.Body;

			var contact = FindContact(graph, package.Address);
			if (contact != null)
			{
				message.ParentRefNoteID = contact.NoteID;
				@case.ContactID = contact.ContactID;
			}

			var bAccount = FindAccount(graph, contact);
			if (bAccount != null)
			{
				message.ParentRefNoteID = bAccount.NoteID;
				@case.CustomerID = bAccount.BAccountID;
			}

			@case = (CRCase) caseCache.Update(@case);

			PersistRecord(package, @case);
			message.RefNoteID = @case.NoteID;
			return true;
		}

		private static string GetFromString(string address, string description)
		{
			return string.Format("From {0} {1}", description, address);
		}

		private void SetAccessInfo(PXGraph graph)
		{
			graph.Caches[typeof(AccessInfo)].Current = graph.Accessinfo;
		}

		private void SetCRSetup(PXGraph graph)
		{
			var crSetupCache = graph.Caches[typeof(CRSetup)];
			crSetupCache.Current = (CRSetup)PXSelect<CRSetup>.SelectWindowed(graph, 0, 1);
		}

		private BAccount FindAccount(PXGraph graph, Contact contact)
		{
			if (contact == null || contact.BAccountID == null) return null;

			PXSelect<BAccount,
				Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Clear(graph);
			var account = (BAccount)PXSelect<BAccount,
										Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
										Select(graph, contact.BAccountID);
			return account;
		}

		private Contact FindContact(PXGraph graph, string address)
		{
			PXSelect<Contact,
				Where<Contact.eMail, Equal<Required<Contact.eMail>>>>.
				Clear(graph);
			var contact = (Contact)PXSelect<Contact,
										Where<Contact.eMail, Equal<Required<Contact.eMail>>>>.
										SelectWindowed(graph, 0, 1, address);
			return contact;
		}
	}

	public class CaseCommonEmailProcessor : BasicEmailProcessor
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
			if (message.RefNoteID == null) return false;


			var graph = package.Graph;

			PXSelect<CRCase,
				Where<CRCase.noteID, Equal<Required<CRCase.noteID>>>>.
				Clear(graph);

			var @case = (CRCase)PXSelect<CRCase,
				Where<CRCase.noteID, Equal<Required<CRCase.noteID>>>>.
				Select(graph, message.RefNoteID);

			if (@case == null || @case.CaseID == null) return false;

			if (!RouterEmailProcessor.IsFromInternalUser(graph, message) &&
				(@case.MajorStatus == CRCaseMajorStatusesAttribute._PENDING_CUSTOMER ||
				@case.Resolution == CRCaseResolutionsAttribute._CUSTOMER_PRECLOSED))
			{
				var caseCache = graph.Caches[typeof(CRCase)];
				var newCase = (CRCase)caseCache.CreateCopy(@case);
				newCase.MajorStatus = CRCaseMajorStatusesAttribute._OPEN;
				newCase.Status = CRCaseStatusesAttribute._OPEN;
				newCase.Resolution = CRCaseResolutionsAttribute._CUSTOMER_REPLIED;
				newCase = (CRCase)caseCache.Update(newCase);
				PersistRecord(package, newCase);
			}

			return true;
		}
	}
}
