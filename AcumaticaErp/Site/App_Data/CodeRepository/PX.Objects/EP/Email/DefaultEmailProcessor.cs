using System;
using PX.Common;
using PX.Common.Mail;
using PX.Data;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.EP
{
	public class DefaultEmailProcessor : BasicEmailProcessor
	{
		protected override bool Process(Package package)
		{
			var message = package.Message;
			if (message.RefNoteID != null) return false;
			if (message.Ticket == null) return false;

			var graph = package.Graph;
			message.StartDate = PXTimeZoneInfo.Now;
			message.Owner = GetKnownSender(graph, message);

			var parentMessage = GetParentOriginalActivity(graph, (int)message.Ticket);
			if (parentMessage == null) return false;

			message.ParentTaskID = parentMessage.TaskID;
			message.RefNoteID = parentMessage.RefNoteID;
			message.ParentRefNoteID = parentMessage.ParentRefNoteID;
			message.ProjectID = parentMessage.ProjectID;
			message.ProjectTaskID = parentMessage.ProjectTaskID;
			message.IsPrivate = parentMessage.IsPrivate;
			
			if (message.Owner == null) message.Owner = parentMessage.Owner;
			return true;
		}
		
		private Guid? GetKnownSender(PXGraph graph, EPActivity message)
		{
			var @from = Mailbox.Parse(message.MailFrom).With(_ => _.Address).With(_ => _.Trim());

			PXSelectJoin<EPEmployee,
				InnerJoin<Contact, On<Contact.contactID, Equal<EPEmployee.defContactID>>,
				InnerJoin<Users, On<Users.pKID, Equal<EPEmployee.userID>>>>,
				Where<Contact.eMail, Equal<Required<Contact.eMail>>>>.
				Clear(graph);

			var employeeEmail = (EPEmployee)PXSelectJoin<EPEmployee,
				InnerJoin<Contact, On<Contact.contactID, Equal<EPEmployee.defContactID>>,
				InnerJoin<Users, On<Users.pKID, Equal<EPEmployee.userID>>>>,
				Where<Contact.eMail, Equal<Required<Contact.eMail>>>>.
				Select(graph, @from);
			if (employeeEmail != null) return employeeEmail.UserID;

			return null;
		}

		public static EPActivity GetParentOriginalActivity(PXGraph graph, int taskId)
		{
			PXSelectReadonly<EPActivity,
				Where<EPActivity.taskID, Equal<Required<EPActivity.taskID>>>>.
				Clear(graph);

			var res = (EPActivity)PXSelectReadonly<EPActivity,
				Where<EPActivity.taskID, Equal<Required<EPActivity.taskID>>>>.
				Select(graph, taskId);
			while (res != null && res.ClassID == CRActivityClass.EmailRouting)
			{
				if (res.ParentTaskID == null) res = null;
				else
					res = (EPActivity)PXSelectReadonly<EPActivity,
							Where<EPActivity.taskID, Equal<Required<EPActivity.taskID>>>>.
							Select(graph, res.ParentTaskID);
			}
			return res;
		}
	}
}
