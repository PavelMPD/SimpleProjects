using System;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Export.Imc;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.EP
{
	public class EPEventVCalendarProcessor : IVCalendarProcessor
	{
		private PXSelectBase<EPActivity> _infoSelect;
		private PXSelectBase<EPOtherAttendeeWithNotification> _otherAttendees;
		private PXSelectBase<EPAttendee> _attendees;

		private PXGraph _graph;

		protected PXGraph Graph
		{
			get { return _graph ?? (_graph = new PXGraph()); }
		}

		public virtual void Process(vEvent card, object item)
		{
			var row = item as EPActivity;
			if (row == null) return;

			FillCommon(card, row);
			FillOrganizer(card, row);
			FillAttendee(card, row);
		}

		private PXSelectBase<EPActivity> InfoSelect
		{
			get
			{
				if (_infoSelect == null)
				{
					_infoSelect = 
						new PXSelectJoin<EPActivity,
							LeftJoin<Users, On<Users.pKID, Equal<EPActivity.owner>>,
							LeftJoin<EPEmployee, On<EPEmployee.userID, Equal<EPActivity.owner>>,
							LeftJoin<Contact, On<Contact.contactID, Equal<EPEmployee.defContactID>>>>>,
							Where<EPActivity.taskID, Equal<Required<EPActivity.taskID>>>>(Graph);
				}
				return _infoSelect;
			}
		}

		private PXSelectBase<EPOtherAttendeeWithNotification> OtherAttendees
		{
			get
			{
				if (_otherAttendees == null)
				{
					_otherAttendees =
						new PXSelect<EPOtherAttendeeWithNotification,
							Where<EPOtherAttendeeWithNotification.eventID,
								Equal<Required<EPOtherAttendeeWithNotification.eventID>>>>(Graph);
				}
				return _otherAttendees;
			}
		}

		private PXSelectBase<EPAttendee> Attendees
		{
			get
			{
				if (_attendees == null)
				{
					_attendees =
						new PXSelectJoin<EPAttendee,
							LeftJoin<Users, On<Users.pKID, Equal<EPAttendee.userID>>,
							LeftJoin<EPEmployee, On<EPEmployee.userID, Equal<EPAttendee.userID>>,
							LeftJoin<Contact, On<Contact.contactID, Equal<EPEmployee.defContactID>>>>>,
							Where<EPAttendee.eventID, Equal<Required<EPAttendee.eventID>>>>(Graph);
				}
				return _attendees;
			}
		}

		private void FillAttendee(vEvent card, EPActivity row)
		{
			OtherAttendees.View.Clear();
			foreach (EPOtherAttendeeWithNotification otherAttendee in OtherAttendees.Select(row.TaskID))
				{
					card.Attendees.Add(
						new vEvent.Attendee(
							otherAttendee.Name,
							otherAttendee.Email,
							vEvent.Attendee.Statuses.Accepted));
				}

			Attendees.View.Clear();
			foreach (PXResult<EPAttendee, Users, EPEmployee, Contact> item in Attendees.Select(row.TaskID))
			{
				var attendee = (EPAttendee)item[typeof(EPAttendee)];
				var user = (Users)item[typeof(Users)];
				var contact = (Contact)item[typeof(Contact)];
				string fullName;
				string email;
				ExtractAttendeeInfo(user, contact, out fullName, out email);
				var status = vEvent.Attendee.Statuses.NeedAction;
				switch (attendee.Invitation)
				{
					case PXInvitationStatusAttribute.ACCEPTED:
						status = vEvent.Attendee.Statuses.Accepted;
						break;
					case PXInvitationStatusAttribute.REJECTED:
						status = vEvent.Attendee.Statuses.Declined;
						break;
				}
				card.Attendees.Add(
					new vEvent.Attendee(
						user.FullName,
						user.Email,
						status));
			}
		}

		private static void FillCommon(vEvent card, EPActivity row)
		{
			if (row.StartDate == null)
				throw new ArgumentNullException("row", Messages.NullStartDate);

			var timeZone = LocaleInfo.GetTimeZone();
			var startDate = PXTimeZoneInfo.ConvertTimeToUtc((DateTime)row.StartDate, timeZone);
			card.Summary = row.Subject;
			card.IsHtml = true;
			card.Description = row.Body;
			card.StartDate = startDate;
			card.EndDate = row.EndDate.HasValue
								? PXTimeZoneInfo.ConvertTimeToUtc((DateTime)row.EndDate, timeZone)
								: startDate;
			card.Location = row.Location;
			card.IsPrivate = row.IsPrivate ?? false;
			card.UID = row.ImcUID == null ? "ACUMATICA_" + row.TaskID.ToString() : row.ImcUID.ToString();
		}

		private void FillOrganizer(vEvent card, EPActivity row)
		{
			InfoSelect.View.Clear();
			var set = InfoSelect.Select(row.TaskID);
			if (set == null || set.Count == 0) return;

			var owner = (Users)set[0][typeof(Users)];
			var contact = (Contact)set[0][typeof(Contact)];
			string fullName;
			string email;
			ExtractAttendeeInfo(owner, contact, out fullName, out email);

			card.OrganizerName = fullName;
			card.OrganizerEmail = email;

			if (!string.IsNullOrEmpty(fullName))
				card.Attendees.Add(new vEvent.Attendee(
					fullName,
					email,
					vEvent.Attendee.Statuses.Accepted,
					vEvent.Attendee.Rules.Chair));
		}

		private static void ExtractAttendeeInfo(Users user, Contact contact, out string fullName, out string email)
		{
			fullName = user.FullName;
			email = user.Email;

			if (contact.DisplayName != null && contact.DisplayName.Trim().Length > 0)
				fullName = contact.DisplayName;
			if (contact.EMail != null && contact.EMail.Trim().Length > 0)
				email = contact.EMail;
		}
	}
}
