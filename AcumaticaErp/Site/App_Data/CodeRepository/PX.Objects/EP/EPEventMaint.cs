using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Export.Imc;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.SM;
using PX.TM;
using ResHandler = PX.TM.PXResourceScheduleAttribute.HandlerAttribute;

namespace PX.Objects.EP
{
	#region NewAttendeeSelectorAttribute
	public sealed class NewAttendeeSelectorAttribute : PXCustomSelectorAttribute
	{
		private class Definition : IPrefetchable<object>
		{
			private List<object> _items;

			public IEnumerable<object> Items
			{
				get
				{
					return _items;
				}
			}

			public void Prefetch(object parameter)
			{
				_items = new List<object>();
				var res = PXSelect<EPAttendee, 
					Where<EPAttendee.eventID, Equal<Required<EPAttendee.eventID>>>>.
					Select(new PXGraph(), parameter);
				if (res != null)
					foreach (EPAttendee row in res)
					{
						_items.Add(row.UserID);
					}
			}
		}

		private BqlCommand _selectCommand;
		private PXView _selectAll;

		private readonly Type _eventKeyType;
		private readonly Type _eventOwnerType;
		private readonly Type _eventDACType;

		private object _reculc;
		private object[] _cachedAttendees;
		private Definition _definition;

		public NewAttendeeSelectorAttribute(Type eventKeyType, Type eventOwnerType)
			: this(eventKeyType, eventOwnerType, new Type[0]) { }

		public NewAttendeeSelectorAttribute(Type eventKeyType, Type eventOwnerType, params Type[] fieldList)
			: base(typeof(Users.pKID), fieldList)
		{
			_eventKeyType = eventKeyType;
			_eventOwnerType = eventOwnerType;
			_eventDACType = eventOwnerType.DeclaringType;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_selectCommand = BqlCommand.CreateInstance(typeof(Select<,>), typeof(Users), 
				typeof(Where<,,>), typeof(Users.pKID), typeof(NotEqual<>), typeof(Current<>), _eventOwnerType,
					typeof(Or<,>), typeof(Current<>), _eventOwnerType, typeof(IsNull));

			_selectAll = new PXView(_Graph, true, new Select<Users>());

			sender.Graph.RowInserted.AddHandler(sender.GetItemType(), (cache, args) => _reculc = null);
			sender.Graph.RowUpdated.AddHandler(sender.GetItemType(), (cache, args) => _reculc = null);
			sender.Graph.RowDeleted.AddHandler(sender.GetItemType(), (cache, args) => _reculc = null);
		}

		public IEnumerable GetRecords()
		{
			if (PXView.Searches != null && PXView.Searches.Length > 0)
			{
				var searchesAreSpecified = false;
				foreach (object val in PXView.Searches)
				{
					if (val != null)
					{
						searchesAreSpecified = true;
						break;
					}
				}
				if (searchesAreSpecified)
				{
					var start = PXView.StartRow;
					var total = 0;
					foreach (Users item in _selectAll.
						Select(null, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref start, PXView.MaximumRows, ref total))
					{
						yield return item;
					}
					PXView.StartRow = 0;
					yield break;
				}
			}

			var cache = _Graph.Caches[_eventDACType];
			var currentEventKey = cache.GetValue(cache.Current, _eventKeyType.Name);
			var currentEventOwner = cache.GetValue(cache.Current, _eventOwnerType.Name);

			var selectedAttendees = GetSelectedAttendees(currentEventKey, currentEventOwner);

			var command =
				selectedAttendees.Length > 0
					? _selectCommand.WhereAnd(NotInHelper<Users.pKID>.Create(selectedAttendees.Length))
					: _selectCommand;

			var startRow = PXView.StartRow;
			var totalRows = 0;
			foreach (Users row in new PXView(_Graph, true, command).
				Select(null, selectedAttendees, null, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
			{
				yield return row;
			}
			PXView.StartRow = 0;
		}

		private object[] GetSelectedAttendees(object currentEventKey, object currentEventOwner)
		{
			Definition dbDef;
			if ((_reculc == null || !object.Equals(_reculc, currentEventKey)) | 
				(dbDef = PXDatabase.GetSlot<Definition, object>(this.GetType().Name + (int)currentEventKey, currentEventKey, typeof(Users))) != _definition)
			{
				var res = new List<object>(dbDef.Items);
				var attendeeCache = _Graph.Caches[typeof(EPAttendee)];
				foreach (EPAttendee row in attendeeCache.Inserted)
					if (row.UserID != null) res.Add(row.UserID);
				foreach (EPAttendee row in attendeeCache.Updated)
					if (row.UserID != null) res.Add(row.UserID);
				if (currentEventOwner != null)
				{
					res.Insert(0, currentEventOwner);
					res.Insert(0, currentEventOwner);
				}
				_cachedAttendees = res.ToArray();
				_reculc = currentEventKey;
				_definition = dbDef;
			}
			return _cachedAttendees;
		}
	}
	#endregion

	#region StringGuidAttribute

	public sealed class StringGuidAttribute : PXEventSubscriberAttribute, PX.Data.IPXFieldSelectingSubscriber
	{
		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			Guid val;
			if (e.ReturnValue != null && e.ReturnValue is string && 
				GUID.TryParse(e.ReturnValue as string, out val))
			{
				e.ReturnValue = val;
			}
		}
	}

	#endregion

	#region AddAttendeeFilter

	[Serializable]
	[DebuggerDisplay("Index = {Index}, PKID = {PKID}")]
	public partial class AddAttendeeFilter : IBqlTable
	{
		public abstract class index : IBqlField { }

		protected Int32? _Index;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(Visible = false)]
		public Int32? Index
		{
			get { return _Index; }
			set { _Index = value; }
		}

		public abstract class pkID : IBqlField { }

		protected Guid? _PKID;
		[PXDBGuid]
		[PXUIField(DisplayName = "User Name")]
		[NewAttendeeSelectorAttribute(typeof(EPActivity.taskID), typeof(EPActivity.owner), typeof(Users.displayName), typeof(Users.email))]
		/*[PXSelector(typeof(Search<Users.pKID, 
			Where<Users.pKID, NotEqual<Current<EPActivity.owner>>, 
				Or<Current<EPActivity.owner>, IsNull>>>), 
			SubstituteKey = typeof(Users.username))]*/
		public Guid? PKID
		{
			get { return _PKID; }
			set { _PKID = value; }
		}
	}

	#endregion

	#region DeleteOtherAttendeeFilter

	[Serializable]
	public partial class DeleteOtherAttendeeFilter : IBqlTable
	{
		#region AttendeeID

		public abstract class attendeeID : IBqlField { }

		protected Int32? _AttendeeID;

		[PXDBInt]
		[PXUIField(Visible = false)]
		public virtual Int32? AttendeeID
		{
			get { return _AttendeeID; }
			set { _AttendeeID = value; }
		}

		#endregion

		#region WithNotification

		public abstract class withNotification : IBqlField { }

		protected Boolean? _WithNotification;

		[PXDBBool]
		[PXUIField(DisplayName = "With Notification")]
		public virtual Boolean? WithNotification
		{
			get { return _WithNotification; }
			set { _WithNotification = value; }
		}

		#endregion
	}

	#endregion

	#region EPOtherAttendeeWithNotification

	[Serializable]
	public partial class EPOtherAttendeeWithNotification : EPOtherAttendee
	{
		public new abstract class eventID : IBqlField { }
		public new abstract class attendeeID : IBqlField { }

		#region NotifyOnRowUpdated

		public abstract class notifyOnRowUpdated : IBqlField { }

		protected Boolean? _NotifyOnRowUpdated;

		[PXBool]
		[PXDefault(false)]
		[PXUIField(Visible = false)]
		public virtual Boolean? NotifyOnRowUpdated
		{
			get { return _NotifyOnRowUpdated; }
			set { _NotifyOnRowUpdated = value; }
		}

		#endregion
	}

	#endregion

	#region AllTypeAttendee

	[Serializable]
	public partial class AllTypeAttendee : IBqlTable
	{
		#region Key

		public abstract class key : IBqlField { }

		protected String _Key;

		[PXString(IsKey = true)]
		[PXUIField(Visible = false)]
		public virtual String Key
		{
			get { return _Key; }
			set { _Key = value; }
		}

		#endregion

		#region Type

		public abstract class type : IBqlField { }

		protected Int32? _Type;

		[PXInt(IsKey = true)]
		[PXUIField(Visible = false)]
		public virtual Int32? Type
		{
			get { return _Type; }
			set { _Type = value; }
		}

		#endregion

		#region EventID
		public abstract class eventID : IBqlField { }

		protected Int32? _EventID;
		[PXInt(IsKey = true)]
		[PXDBDefault(typeof(EPActivity.taskID))]
		public virtual Int32? EventID
		{
			get { return _EventID; }
			set { _EventID = value; }
		}
		#endregion

		#region Email
		public abstract class email : IBqlField { }

		protected String _Email;
		[PXString]
		public virtual String Email
		{
			get { return _Email; }
			set { _Email = value; }
		}
		#endregion

		#region Name

		public abstract class name : IBqlField { }

		protected String _Name;

		[PXString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Name")]
		[NewAttendeeSelector(typeof(EPActivity.taskID), typeof(EPActivity.owner), typeof(Users.displayName))]
		/*[PXSelector(typeof(Search<Users.pKID, 
			Where<Users.pKID, NotEqual<Current<EPActivity.owner>>, 
				Or<Current<EPActivity.owner>, IsNull>>>))]*/
		[StringGuid]
		public virtual String Name
		{
			get { return _Name; }
			set { _Name = value; }
		}

		#endregion

		#region DisplayName

		public abstract class displayName : IBqlField { }

		protected String _DisplayName;

		[PXString(50, IsUnicode = true)]
		[PXUIField(Visible = false)]
		[PXSelector(typeof(Users.fullName))]
		public virtual String DisplayName
		{
			get { return _DisplayName; }
			set { _DisplayName = value; }
		}

		#endregion

		#region Comment

		public abstract class comment : IBqlField { }

		protected String _Comment;

		[PXString(255)]
		[PXUIField(DisplayName = "Comment")]
		public virtual String Comment
		{
			get { return _Comment; }
			set { _Comment = value; }
		}

		#endregion

		#region Invitation

		public abstract class invitation : IBqlField { }

		protected Int32? _Invitation;

		[PXInt]
		[PXUIField(DisplayName = "Invitation", Enabled = false)]
		[PXDefault(PXInvitationStatusAttribute.NOTINVITED)]
		[PXInvitationStatus]
		public virtual Int32? Invitation
		{
			get { return _Invitation ?? PXInvitationStatusAttribute.NOTINVITED; }
			set { _Invitation = value; }
		}

		#endregion
	}

	#endregion

	#region EPAttendeeForAcceptReject

	[Serializable]
	public partial class EPAttendeeForAcceptReject : EPAttendee
	{
		public new abstract class userID : IBqlField { }
	}

	#endregion

	#region SendCardFilter

	[Serializable]
	public partial class SendCardFilter : IBqlTable
	{
		#region Email

		public abstract class email : IBqlField { }

		protected String _Email;

		[PXString]
		[PXUIField(DisplayName = "Email")]
		public virtual String Email
		{
			get { return _Email; }
			set { _Email = value; }
		}

		#endregion
	}

	#endregion

	public class EPEventMaint : CRBaseActivityMaint<EPEventMaint>
	{
		#region PXSelectEvents
		public sealed class PXSelectEvents<owner> : PXSelectJoin<EPActivity,
			InnerJoin<Users, On<Users.pKID, Equal<EPActivity.owner>>>,
				Where<EPActivity.owner, Equal<owner>,
					And<EPActivity.taskID, NotEqual<Current<EPActivity.taskID>>,
					And<Where<EPActivity.startDate, GreaterEqual<Optional<EPActivity.startDate>>,
						And<EPActivity.startDate, LessEqual<Optional<EPActivity.startDate>>,
							Or<Where<EPActivity.startDate, Less<Optional<EPActivity.startDate>>,
								And<EPActivity.endDate, Greater<Optional<EPActivity.endDate>>>>>>>>>>,
				OrderBy<Asc<Users.username, Asc<EPActivity.startDate, Asc<EPActivity.endDate>>>>>
			where owner : IBqlOperand
		{
			public PXSelectEvents(PXGraph graph) : base(graph) { }
			public PXSelectEvents(PXGraph graph, Delegate handler) : base(graph, handler) { }

			public static PXResultset<EPActivity> ExecuteSelect(PXGraph graph, DateTime? start, DateTime? end, params object[] parameters)
			{
				List<object> pList = new List<object>(parameters);
				pList.Add(start);
				pList.Add(end);
				pList.Add(start);
				pList.Add(start);
				return Select(graph, pList.ToArray());
			}
		}
		#endregion

		#region ConfirmNotificationResult

		private enum ConfirmNotificationResult
		{
			No,
			Yes,
			YesForAll
		}

		#endregion

		#region ConfirmNotificationHandler

		private delegate ConfirmNotificationResult ConfirmNotificationHandler();

		#endregion

		#region NotificationHandler

		private delegate void NotificationHandler(ConfirmNotificationResult target);

		#endregion

		#region NotificationOnAction

		private class NotificationOnAction
		{
			public delegate IEnumerable ActionHandler(PXAdapter adapter);

			private readonly ConfirmNotificationHandler _confirmHandler;
			private readonly NotificationHandler _notificationHandler;

			public NotificationOnAction(ConfirmNotificationHandler confirmHandler, NotificationHandler notificationHandler)
			{
				if (confirmHandler == null) throw new ArgumentNullException("confirmHandler");
				if (notificationHandler == null) throw new ArgumentNullException("notificationHandler");

				_confirmHandler = confirmHandler;
				_notificationHandler = notificationHandler;
			}

			public virtual IEnumerable HandleAction(ActionHandler action, PXAdapter adapter)
			{
				if (action == null) throw new ArgumentNullException("action");

				ConfirmNotificationResult confirm = _confirmHandler();
				IEnumerable result = action(adapter);
				if (confirm != ConfirmNotificationResult.No) _notificationHandler(confirm);
				foreach (object item in result)
					yield return item;
				adapter.View.ClearDialog();
				yield break;
			}
		}

		#endregion

		#region SaveActionWithNotification

		private class SaveActionWithNotification<TNode> : PXAction<TNode>
			where TNode : class, IBqlTable, new()
		{
			private readonly NotificationOnAction _notificationHandler;

			public SaveActionWithNotification(PXGraph graph, string name,
				ConfirmNotificationHandler confirmHandler, NotificationHandler notificationHandler)
				: base(graph, name)
			{
				_notificationHandler = new NotificationOnAction(confirmHandler, notificationHandler);
			}

			[PXUIField(DisplayName = ActionsMessages.Save,
				MapEnableRights = PXCacheRights.Update,
				MapViewRights = PXCacheRights.Update)]
			[PXSaveButton]
			protected override IEnumerable Handler(PXAdapter adapter)
			{
				return _notificationHandler.HandleAction(
					a =>
					{
						var res = a.Get();
						var graph = a.View.Graph;
						graph.Persist();
						graph.SelectTimeStamp();
						return res;
					}, 
					adapter);
			}
		}

		#endregion

		#region SaveCloseActionWithNotification

		private class SaveCloseActionWithNotification<TNode> : PXAction<TNode>
			where TNode : class, IBqlTable, new()
		{
			private readonly NotificationOnAction _notificationHandler;

			public SaveCloseActionWithNotification(PXGraph graph, string name,
				ConfirmNotificationHandler confirmHandler, NotificationHandler notificationHandler)
				: base(graph, name)
			{
				_notificationHandler = new NotificationOnAction(confirmHandler, notificationHandler);
			}

			[PXUIField(DisplayName = ActionsMessages.SaveClose,
				MapEnableRights = PXCacheRights.Update,
				MapViewRights = PXCacheRights.Update)]
			[PXSaveCloseButton]
			protected override IEnumerable Handler(PXAdapter adapter)
			{
				return _notificationHandler.HandleAction(
					a =>
					{
						var res = a.Get();
						var graph = a.View.Graph;
						graph.Persist();
						graph.SelectTimeStamp();
						return res;
					},
					adapter);
			}
		}

		#endregion

		#region DeleteActionWithNotification

		private class DeleteActionWithNotification<TNode> : PXDelete<TNode>
			where TNode : class, IBqlTable, new()
		{
			private readonly NotificationOnAction _notificationHandler;

			public DeleteActionWithNotification(PXGraph graph, string name,
				ConfirmNotificationHandler confirmHandler, NotificationHandler notificationHandler)
				: base(graph, name)
			{
				_notificationHandler = new NotificationOnAction(confirmHandler, notificationHandler);
			}

			[PXUIField(DisplayName = ActionsMessages.Delete,
				MapEnableRights = PXCacheRights.Delete,
				MapViewRights = PXCacheRights.Delete)]
			[PXDeleteButton]
			protected override IEnumerable Handler(PXAdapter adapter)
			{
				return _notificationHandler.HandleAction(base.Handler, adapter);
			}
		}

		#endregion

		#region NotificationTypes

		private enum NotificationTypes
		{
			Invitation,
			Reschedule,
			Cancel
		}

		#endregion

		#region Constants
		private const string _ATTENDEE_VIEW_NAME = "AttendeeResources";
		private const string _EMAIL_VALIDATION = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

		private const int _OTHER_ATTENDEE_TYPE = 0;
		private const int _USER_ATTENDEE_TYPE = 1;

		private const string _SAVE_ACTION_NAME = "Save";
		private const string _SAVECLOSE_ACTION_NAME = "SaveClose";
		private const string _DELETE_ACTION_NAME = "Delete";

		private const string _ASK_KEY_0 = "key0";
		private const string _ASK_KEY_1 = "key1";
		private const string _ASK_KEY_2 = "key2";
		private const string _ASK_KEY_3 = "key3";
		private const string _ASK_KEY_4 = "key4";
		private const string _ASK_KEY_5 = "key5";
		private const string _ASK_KEY_6 = "key6";
		private const string _ASK_KEY_7 = "key7";

		#endregion

		#region Fields

		private static readonly Regex _emailValidationRegex;
		private static readonly string _WIKI_NEW_LINE;

		#endregion

		#region Selects

		[PXHidden]
		public PXSelect<CT.Contract>
				BaseContract;

		[PXViewName(Objects.EP.Messages.Events)]
		[PXRefNoteSelector(typeof(EPActivity), typeof(EPActivity.refNoteID))]
		public PXSelect<EPActivity, 
			Where<EPActivity.classID, Equal<CRActivityClass.events>>> 
			Events;

		[PXFilterable]
		[CRBAccountReference(typeof(Select<EPActivity, Where<EPActivity.taskID, Equal<Current<EPActivity.taskID>>>>), RefField=typeof(EPActivity.parentRefNoteID))]
		public CRChildActivityList<EPActivity> 
			ChildActivities;

		public PXSetup<EPSetup> 
			Setup;

		public PXSelect<AddAttendeeFilter> NewAttendeeCurrent;

		public PXSelectJoin<EPAttendee,
			InnerJoin<Users, On<Users.pKID, Equal<EPAttendee.userID>>>,
			Where<EPAttendee.eventID, Equal<Current<EPActivity.taskID>>>> 
			Attendees;

		public PXSelectOrderBy<Users,
			OrderBy<Asc<Users.fullName, Asc<Users.email>>>> 
			NotAttendees;

		[PXResourceSchedule(typeof(EPActivity), typeof(EPActivity), typeof(EPActivity.startDate), typeof(EPActivity.endDate),
			DescriptionBqlField = typeof(Users.fullName),
			ItemDescriptionBqlField = typeof(EPActivity.tooltip),
			TargetTable = typeof(EPActivity),
			TargetStartBqlField = typeof(EPActivity.startDate),
			TargetEndBqlField = typeof(EPActivity.endDate))]
		public PXSelectJoin<Users,
			InnerJoin<EPAttendee, On<EPAttendee.userID, Equal<Users.pKID>>,
				LeftJoin<EPActivity, On<EPActivity.owner, Equal<EPAttendee.userID>>>>,
			Where<EPAttendee.eventID, Equal<Current<EPActivity.taskID>>>> 
			AttendeeResources;

		public PXSelect<Users,
			Where<Users.pKID, Equal<Current<EPActivity.owner>>>> 
			CurrentOwner;

		public PXSelect<EPOtherAttendeeWithNotification,
			Where<EPOtherAttendeeWithNotification.eventID, Equal<Current<EPActivity.taskID>>>> OtherAttendees;

		[PXFilterable]
		public PXSelect<AllTypeAttendee> 
			AllAttendees;

		[PXFilterable]
		public PXSelect<AllTypeAttendee> 
			AllAttendeesAndOwner;

		public PXFilter<DeleteOtherAttendeeFilter> 
			ConfirmDeleteOtherAttendees;

		public PXSelect<EPAttendeeMessage,
			Where<EPAttendeeMessage.eventID, Equal<Current<EPActivity.taskID>>>> 
			AttendeeMessages;

		public PXSelect<EPOtherAttendeeMessage,
			Where<EPAttendeeMessage.eventID, Equal<Current<EPActivity.taskID>>>> 
			OtherAttendeeMessages;

		public PXSelect<Users> 
			UsersSearch;

		public PXFilter<SendCardFilter> 
			SendCardSettings;

		public PXSelectJoin<CSCalendar,
			InnerJoin<EPEmployee, On<EPEmployee.calendarID, Equal<CSCalendar.calendarID>>>,
			Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>> 
			WorkCalendar;

		[PXHidden]
		public PXSelect<EPAttendeeForAcceptReject,
			Where<EPAttendeeForAcceptReject.userID, Equal<Current<AccessInfo.userID>>>> 
			CurrentAttendee;

		#endregion

		#region Ctors

		static EPEventMaint()
		{
			_emailValidationRegex = new Regex(_EMAIL_VALIDATION, RegexOptions.Compiled);
			_WIKI_NEW_LINE = Environment.NewLine + Environment.NewLine;
		}

		public EPEventMaint()
			: base()
		{
			CorrectActions();

			PXUIFieldAttribute.SetVisible(Caches[typeof(Users)], typeof(Users.username).Name, true);
			var activitiesCache = Caches[typeof (EPActivity)];
			PXUIFieldAttribute.SetDisplayName(activitiesCache, typeof(EPActivity.owner).Name, Messages.CreatedBy);
			PXUIFieldAttribute.SetDisplayName(activitiesCache, typeof(EPActivity.startDate).Name, "Start Time");

			PXEntityInfoAttribute.SetDescriptionDisplayName<EPActivity.source>(activitiesCache, "Related Entity");

			this.Action.AddMenuAction(CompleteAndFollowUp);
			this.Action.AddMenuAction(ExportCard);
			this.Action.AddMenuAction(sendCard);
		}

		private void CorrectActions()
		{
			Actions[_SAVE_ACTION_NAME] = new SaveActionWithNotification<EPActivity>(this, _SAVE_ACTION_NAME,
				ConfirmAttendeeInvitations, InviteAttendees);
			Actions[_SAVECLOSE_ACTION_NAME] = new SaveCloseActionWithNotification<EPActivity>(this, _SAVECLOSE_ACTION_NAME,
				ConfirmAttendeeInvitations, InviteAttendees);
			Actions[_DELETE_ACTION_NAME] = new DeleteActionWithNotification<EPActivity>(this, _DELETE_ACTION_NAME,
				ConfirmCancelAttendeeInvitations, CancelAttendeeInvitations);
		}

		#endregion

		#region Data Handlers

		protected virtual IEnumerable newAttendeeCurrent()
		{
			if (NewAttendeeCurrent.Cache.Current == null)
			{
				NewAttendeeCurrent.Cache.Insert();
				NewAttendeeCurrent.Cache.IsDirty = false;
			}
			yield return NewAttendeeCurrent.Cache.Current;
		}

		protected virtual IEnumerable notAttendees()
		{
			foreach (Users user in PXSelect<Users>.Select(this))
				if (Attendees.Search<EPAttendee.userID>(user.PKID).Count == 0)
					yield return user;
		}

		protected virtual IEnumerable allAttendees()
		{
			foreach (EPOtherAttendeeWithNotification otherAttendee in OtherAttendees.Select())
				yield return new AllTypeAttendee
								{
									Type = _OTHER_ATTENDEE_TYPE,
									Key = ((int)otherAttendee.AttendeeID).ToString(),
									EventID = otherAttendee.EventID,
									Name = otherAttendee.Name,
									DisplayName = otherAttendee.Name,
									Email = otherAttendee.Email,
									Comment = otherAttendee.Comment,
									Invitation = otherAttendee.Invitation
								};
			foreach (PXResult<EPAttendee, Users> item in Attendees.Select())
			{
				var epAttendee = (EPAttendee)item;
				var user = (Users)item;

				var displayName = user.DisplayName;
				var email = user.Email;

				var employeeContactPair = user.PKID.
					With(_ => PXSelectJoin<EPEmployee,
						LeftJoin<Contact, On<Contact.contactID, Equal<EPEmployee.defContactID>>>, 
						Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.
					Select(this, _.Value));
				if (employeeContactPair != null && employeeContactPair.Count > 0)
				{
					var employee = (EPEmployee)employeeContactPair[0][typeof(EPEmployee)];
					var contact = (Contact)employeeContactPair[0][typeof(Contact)];
					if (!string.IsNullOrWhiteSpace(employee.AcctName)) displayName = employee.AcctName;
					if (!string.IsNullOrWhiteSpace(contact.EMail)) email = contact.EMail;
				}
				yield return new AllTypeAttendee
								{
									Type = _USER_ATTENDEE_TYPE,
									Key = ((Guid)epAttendee.UserID).ToString(),
									EventID = epAttendee.EventID,
									Invitation = epAttendee.Invitation,
									Name = Convert.ToString(user.PKID),
									DisplayName = displayName,
									Email = email,
									Comment = user.Comment
								};
			}
		}

		public virtual IEnumerable allAttendeesAndOwner()
		{
			foreach (var attendee in AllAttendees.Select())
				yield return attendee;

			var owner = EventOwner;
			if (owner == null) yield break;

			var ownerAsAttendee = new AllTypeAttendee()
									{
										Type = _USER_ATTENDEE_TYPE,
										Key = ((Guid)owner.PKID).ToString(),
										EventID = Events.Current == null ? null : Events.Current.TaskID,
										Invitation = PXInvitationStatusAttribute.ACCEPTED,
										Name = Convert.ToString(owner.PKID),
										DisplayName = owner.DisplayName,
										Email = owner.Email,
										Comment = owner.Comment
									};
			yield return new PXResult<AllTypeAttendee>(ownerAsAttendee);
		}

		[ResHandler(_ATTENDEE_VIEW_NAME, ResHandler.Types.GetAdditionalRecords)]
		public virtual IEnumerable OwnerEvents(DateTime? start, DateTime? end)
		{
			Users currentOwner = EventOwner;
			if (currentOwner == null) yield break;

			if (GetAttendeesByEventOwner((EPActivity)Events.Current).GetEnumerator().MoveNext())
				yield break;

			PXResultset<EPActivity> eventsDBResult = PXSelectEvents<Current<EPActivity.owner>>.ExecuteSelect(this, start, end);
			foreach (PXResult<EPActivity> row in eventsDBResult)
			{
				EPActivity @event = (EPActivity)row;
				EPAttendee attendee = new EPAttendee();
				attendee.EventID = @event.TaskID;
				attendee.UserID = currentOwner.PKID;
				yield return new PXResult<Users, EPAttendee, EPActivity>(currentOwner, attendee, @event);
			}
			if (eventsDBResult.Count == 0)
				yield return new PXResult<Users, EPAttendee, EPActivity>(currentOwner, new EPAttendee(), new EPActivity());
		}

		[ResHandler(_ATTENDEE_VIEW_NAME, ResHandler.Types.GetAdditionalRecords)]
		public virtual IEnumerable InsertedAttendeeEvents(DateTime? start, DateTime? end)
		{
			foreach (EPAttendee attendee in this.Caches[typeof(EPAttendee)].Inserted)
				if (attendee.UserID.HasValue)
				{
					PXResultset<EPActivity> eventsDBResult =
						PXSelectEvents<Required<EPActivity.owner>>.ExecuteSelect(this, start, end, attendee.UserID);
					foreach (PXResult<EPActivity, Users> row in eventsDBResult)
					{
						EPActivity @event = (EPActivity)row;
						Users user = (Users)row;
						yield return new PXResult<Users, EPAttendee, EPActivity>(user, attendee, @event);
					}
					if (eventsDBResult.Count == 0)
					{
						PXResultset<Users> usersDBResult =
							PXSelect<Users, Where<Users.pKID, Equal<Required<Users.pKID>>>>.Select(this, attendee.UserID);
						if (usersDBResult.Count > 0)
							yield return new PXResult<Users, EPAttendee, EPActivity>(
								(Users)usersDBResult, attendee, new EPActivity());
					}
				}
		}

		#endregion

		#region Actions

		public PXDelete<EPActivity> Delete;

		public PXAction<EPActivity> Complete;
		[PXUIField(DisplayName = PX.TM.Messages.CompleteEvent, MapEnableRights = PXCacheRights.Select)]
		[PXButton(Tooltip = PX.Objects.EP.Messages.CompleteEventTooltip,
			ShortcutCtrl = true, ShortcutChar = (char)75)] //Ctrl + K
		protected virtual void complete()
		{
			var row = Events.Current;
			if (row == null) return;

			CompleteEvent(Events.Current);
		}

		public PXAction<EPActivity> CompleteAndFollowUp;
		[PXUIField(DisplayName = Messages.CompleteAndFollowUpEvent, MapEnableRights = PXCacheRights.Select)]
		[PXButton(Tooltip = Messages.CompleteAndFollowUpEventTooltip, ShortcutCtrl = true, ShortcutShift = true, ShortcutChar = (char)75)] //Ctrl + Shift + K
		protected virtual void completeAndFollowUp()
		{
			EPActivity row = Events.Current;
			if (row == null) return;

			CompleteEvent(row);

			EPEventMaint graph = CreateInstance<EPEventMaint>();

			EPActivity followUpActivity = (EPActivity)graph.Events.Cache.CreateCopy(row);
			followUpActivity.TaskID = null;
			followUpActivity.ParentTaskID = row.ParentTaskID;
			followUpActivity.UIStatus = null;
			followUpActivity.NoteID = null;
			followUpActivity.PercentCompletion = null;

			if (followUpActivity.StartDate != null)
			{
				followUpActivity.StartDate = ((DateTime) followUpActivity.StartDate).AddDays(1D);
				graph.Events.Cache.SetDefaultExt<EPActivity.weekID>(followUpActivity);
			}
			if (followUpActivity.EndDate != null)
				followUpActivity.EndDate = ((DateTime)followUpActivity.EndDate).AddDays(1D);

			followUpActivity.TimeBillable = null;
			followUpActivity.OvertimeBillable = null;

			graph.Events.Cache.Insert(followUpActivity);
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXAction<EPActivity> CancelActivity;

		[PXUIField(DisplayName = PX.TM.Messages.CancelEvent, MapEnableRights = PXCacheRights.Select)]
		[PXButton(Tooltip = PX.TM.Messages.CancelTask)]
		protected virtual IEnumerable cancelActivity(PXAdapter adapter)
		{
			var notificator = new NotificationOnAction(ConfirmCancelAttendeeInvitations, CancelAttendeeInvitations);
			return notificator.HandleAction(ad =>
				{
					if (Events.Current != null)
						CancelEvent(Events.Current);
					return ad.Get();
				}, adapter);
		}

		public PXAction<EPActivity> acceptInvitation;
		[PXButton(Tooltip = Messages.AcceptInvitationTooltip)]
		[PXUIField(DisplayName = Messages.AcceptInvitation, Visible = false, MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable AcceptInvitation(PXAdapter adapter)
		{
			AcceptParticipation(true);
			return adapter.Get();
		}

		public PXAction<EPActivity> rejectInvitation;

		[PXButton(Tooltip = Messages.RejectInvitationTooltip)]
		[PXUIField(DisplayName = Messages.RejectInvitation, Visible = false, MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable RejectInvitation(PXAdapter adapter)
		{
			AcceptParticipation(false);
			return adapter.Get();
		}

		public PXAction<EPActivity> ExportCard;
		[PXUIField(DisplayName = PX.TM.Messages.ExportCard, MapEnableRights = PXCacheRights.Select)]
		[PXButton(Tooltip = PX.TM.Messages.ExportCardTooltip)]
		public virtual void exportCard()
		{
			var row = Events.Current;
			if (row != null)
			{
				var vCard = VCalendarProcessor.CreateVEvent(row);
				throw new EPIcsExportRedirectException(vCard);
			}
		}

		[ResHandler(_ATTENDEE_VIEW_NAME, ResHandler.Types.AddRecord)]
		public virtual void AddAttendee()
		{
			Users currentOwner = EventOwner;
			AddAttendeeFilter newAttendeeParams = NewAttendeeCurrent.Current;
			if (newAttendeeParams != null &&
				(currentOwner == null || newAttendeeParams.PKID != currentOwner.PKID) &&
				Attendees.Search<EPAttendee.userID>(newAttendeeParams.PKID).Count == 0)
			{
				EPAttendee newAttendee = new EPAttendee();
				newAttendee.EventID = Events.Current.TaskID;
				newAttendee.UserID = newAttendeeParams.PKID;
				Attendees.Insert(newAttendee);

				newAttendeeParams.PKID = null;
				NewAttendeeCurrent.Cache.IsDirty = false;
			}
		}

		[ResHandler(_ATTENDEE_VIEW_NAME, ResHandler.Types.DeleteRecord)]
		public virtual void RemoveAttendee(object item)
		{
		}

		[ResHandler(_ATTENDEE_VIEW_NAME, ResHandler.Types.ViewDetails)]
		public virtual void ViewAttendeeEventDetails(object item)
		{
			if (item is EPActivity)
			{
				var taskID = ((EPActivity)item).TaskID;
				EPActivity row;
				if (taskID.HasValue && (row = Events.Search<EPActivity.taskID>(taskID)) != null)
					PXRedirectHelper.TryOpenPopup(Events.Cache, row, null);
			}
		}

		public PXDBAction<EPActivity> sendInvitations;
		[PXButton(Tooltip = Messages.SendInvitationsTooltip)]
		[PXUIField(DisplayName = Messages.SendInvitations, Visible = false, MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable SendInvitations(PXAdapter adapter)
		{
			PXResultset<AllTypeAttendee> attendees = AllAttendees.Select();
			PXResultset<AllTypeAttendee> notInvitedAttendees =
				AllAttendees.SearchAll<Asc<AllTypeAttendee.invitation>>(new object[] { PXInvitationStatusAttribute.NOTINVITED });
			if (attendees.Count > 0 && notInvitedAttendees.Count > 0)
			{
				AssertEventStatus();

				if (notInvitedAttendees.Count == attendees.Count)
				{
					// send email to all
					SendEMails(NotificationTypes.Invitation, attendees);
				}
				else
				{
					WebDialogResult confirmResult = AllAttendees.Ask(
						_ASK_KEY_0,
						Messages.SendInvitations, Messages.NotifyNotInvitedAttendees, MessageButtons.YesNoCancel);
					switch (confirmResult)
					{
						case WebDialogResult.Yes:
							// send only email to not invited
							SendEMails(NotificationTypes.Invitation, notInvitedAttendees);
							break;
						case WebDialogResult.No:
							// send email to all
							SendEMails(NotificationTypes.Invitation, attendees);
							break;
					}
				}
			}
			adapter.View.ClearDialog();
			return adapter.Get();
		}

		public PXDBAction<EPActivity> sendPersonalInvitation;
		[PXButton(Tooltip = Messages.SendPersonalInvitationTooltip)]
		[PXUIField(DisplayName = Messages.SendPersonalInvitation, Visible = false, MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable SendPersonalInvitation(PXAdapter adapter)
		{
			AssertEventStatus();

			Users eventOwner;
			if (AllAttendeesAndOwner.Current != null && (eventOwner = EventOwner) != null && eventOwner.PKID != null && 
				string.Compare(AllAttendeesAndOwner.Current.Key, ((Guid)eventOwner.PKID).ToString(), true) != 0)
			{
				WebDialogResult confirm = WebDialogResult.Yes;
				if (AllAttendeesAndOwner.Current.Invitation != PXInvitationStatusAttribute.NOTINVITED)
					confirm = AllAttendees.Ask(
											 _ASK_KEY_1,
											 Messages.SendPersonalInvitation,
											 Messages.ResendPersonalInvitation,
											 MessageButtons.YesNo);
				if (confirm == WebDialogResult.Yes)
				{
					SendEMail(NotificationTypes.Invitation, AllAttendeesAndOwner.Current);
					AllAttendees.Ask(Messages.SendPersonalInvitation, Messages.SendInvitationSuccessful, MessageButtons.OK);
				}
			}
			adapter.View.ClearDialog();
			return adapter.Get();
		}

		public PXAction<EPActivity> sendCard;
		[PXButton(Tooltip = Messages.SendCardTooltip)]
		[PXUIField(DisplayName = Messages.SendCard, MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable SendCard(PXAdapter adapter)
		{
			if (Events.Current == null) return adapter.Get();

			WebDialogResult confirm = SendCardSettings.AskExt();
			adapter.View.ClearDialog();

			if (confirm == WebDialogResult.OK)
			{
				if (!CheckEmail(SendCardSettings.Current.Email))
					Events.Ask(Messages.SendCard, Messages.EmailIncorrect, MessageButtons.OK);

				var @event = Events.Current;
				var newLine = Environment.NewLine + Environment.NewLine;
				var mailTo = SendCardSettings.Current.Email;
				var mailBody = "Event Number: " + @event.TaskID.Value + newLine + 
							"Subject: " + @event.Subject + newLine +
							GetEventStringInfo(@event, newLine, string.Empty);
				var mailSubject = "Event: " + @event.Subject;
				var sender = new NotificationGenerator
								{
									To = mailTo,
									Subject = mailSubject,
									Body = mailBody
								};
				using (var buffer = new MemoryStream())
				{
					CreateVEvent().Write(buffer);
					sender.AddAttachment("event.ics", buffer.ToArray());
				}
				sender.Send();
			}
			return adapter.Get();
		}

		public PXMenuAction<EPActivity> Action;

		#endregion

		#region Graph Event Handlers

		public override int ExecuteInsert(string viewName, IDictionary values, params object[] parameters)
		{
			if (viewName == "AllAttendees" || viewName == "AllAttendeesAndOwner")
			{
				Guid userId;
				Users foundUser = !GUID.TryParse(Convert.ToString(values["Name"]), out userId) ? null :
																													UsersSearch.SearchWindowed<Asc<Users.pKID>>(new object[] { userId }, 0, 1);
				if (foundUser == null)
				{
					OrderedDictionary newOtherValues = new OrderedDictionary();
					newOtherValues["EventID"] = Events.Current.TaskID;
					newOtherValues["Name"] = values["Name"];
					newOtherValues["Email"] = values["Email"];
					newOtherValues["Comment"] = values["Comment"];
					AllAttendees.View.Clear();
					int insertOtherResult = 0;
					EPOtherAttendee otherAttendee = PXSelect<EPOtherAttendee,
						Where<EPOtherAttendee.eventID, Equal<Required<EPActivity.taskID>>,
							And<EPOtherAttendee.email, Equal<Required<EPOtherAttendee.email>>>>>.Select(this, Events.Current.TaskID,values["Email"]);
					if (otherAttendee == null)
					{
						insertOtherResult = base.ExecuteInsert("OtherAttendees", newOtherValues);
					}
					if (insertOtherResult > 0) GetOtherAttendeeValuesExt(values, OtherAttendees.Current.AttendeeID);
					return insertOtherResult;
				}
				OrderedDictionary newValues = new OrderedDictionary();
				newValues["EventID"] = Events.Current.TaskID;
				newValues["UserID"] = foundUser.PKID;
				AllAttendees.View.Clear();
				int insertResult = base.ExecuteInsert("Attendees", newValues);
				if (insertResult > 0) GetAttendeeValuesExt(values, Attendees.Current.UserID);
				return insertResult;
			}
			return base.ExecuteInsert(viewName, values, parameters);
		}

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (viewName == "AllAttendees" || viewName == "AllAttendeesAndOwner")
			{
				bool oldRowIsOtherAttendee = _OTHER_ATTENDEE_TYPE.Equals(Convert.ToInt32(keys["Type"]));
				Guid userId;
				Users foundUser = !GUID.TryParse(Convert.ToString(values["Name"]), out userId) ? null :
																													UsersSearch.SearchWindowed<Asc<Users.pKID>>(new object[] { userId }, 0, 1);
				if (foundUser == null)
				{
					OrderedDictionary newOtherKeys = new OrderedDictionary();
					newOtherKeys["AttendeeID"] = oldRowIsOtherAttendee ? keys["Key"] : null;
					newOtherKeys["EventID"] = keys["EventID"];
					OrderedDictionary newOtherValues = new OrderedDictionary();
					newOtherValues["Name"] = values["Name"];
					newOtherValues["Email"] = values["Email"];
					newOtherValues["Comment"] = values["Comment"];
					AllAttendees.View.Clear();
					if (!oldRowIsOtherAttendee) base.ExecuteDelete("Attendees", keys, null);
					int updateOtherResult = base.ExecuteUpdate("OtherAttendees", newOtherKeys, newOtherValues, parameters);
					if (updateOtherResult > 0) GetOtherAttendeeValuesExt(values, newOtherKeys["AttendeeID"]);
					return updateOtherResult;
				}

				OrderedDictionary newKeys = new OrderedDictionary();
				newKeys["UserID"] = foundUser.PKID;
				newKeys["EventID"] = keys["EventID"];
				AllAttendees.View.Clear();
				if (oldRowIsOtherAttendee) base.ExecuteDelete("OtherAttendees", keys, null);
				int updateResult = base.ExecuteUpdate("Attendees", newKeys, new OrderedDictionary(), parameters);
				if (updateResult > 0) GetAttendeeValuesExt(values, newKeys["UserID"]);
				return updateResult;
			}
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		public override int ExecuteDelete(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (viewName == "AllAttendees" || viewName == "AllAttendeesAndOwner")
				switch (Convert.ToInt32(keys["Type"]))
				{
					case _OTHER_ATTENDEE_TYPE:
						var otherAttKeys = new OrderedDictionary();
						otherAttKeys["AttendeeID"] = keys["Key"];
						otherAttKeys["EventID"] = keys["EventID"];
						OtherAttendees.View.Clear();
						return base.ExecuteDelete("OtherAttendees", otherAttKeys, null);
					case _USER_ATTENDEE_TYPE:
						if (IsOwner(keys["Key"])) return 0;
						var attKeys = new OrderedDictionary();
						attKeys["UserID"] = keys["Key"];
						attKeys["EventID"] = keys["EventID"];
						Attendees.View.Clear();
						return base.ExecuteDelete("Attendees", attKeys, null);
					default:
						return 0;
				}
			return base.ExecuteDelete(viewName, keys, values, parameters);
		}

		#endregion

		#region Event Handlers

		#region EPActivity
		[EPStartDate(DisplayName = "Start Date", DisplayNameDate = "Date", DisplayNameTime = "Start Time")]		
		[PXFormula(typeof(Round30Minutes<TimeZoneNow>))]
		[PXUIField(DisplayName = "Start Date")]		
		protected virtual void EPActivity_StartDate_CacheAttached(PXCache cache)
		{

		}
		[PXDBBool]
		[PXUIField(DisplayName = "Track Time", Visible = false)]
		[PXDefault(true)]
		protected virtual void EPActivity_TrackTime_CacheAttached(PXCache cache)
		{
		}

		[PXDBBool()]
		[PXUIField(DisplayName = "Billable", Visible = false)]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPActivity_IsBillable_CacheAttached(PXCache cache)
		{			
		}

		protected virtual void EPActivity_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			var row = e.Row as EPActivity;
			if (row == null) return;

			row.ClassID = CRActivityClass.Event;
				
		}

		protected virtual void EPActivity_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			var row = (EPActivity)e.Row;
			row.ClassID = CRActivityClass.Event;
			row.IsBillable = false;
		}

		protected virtual void EPActivity_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			PXResourceScheduleAttribute.AssumeWorkingTime(AttendeeResources.View, Setup.Current.SearchOnlyInWorkingTime == true);

			EPActivity row = e.Row as EPActivity;
			if (row == null) return;

			string origStatus = (string)cache.GetValueOriginal<EPActivity.uistatus>(row) ?? ActivityStatusListAttribute.Open;			

			bool editable = origStatus == ActivityStatusListAttribute.Open;
			PXUIFieldAttribute.SetEnabled(cache, row, editable);

			PXUIFieldAttribute.SetEnabled<EPActivity.taskID>(cache, row);			
			PXUIFieldAttribute.SetEnabled<EPActivity.timeSpent>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<EPActivity.createdByID>(cache, row, false);

			PXUIFieldAttribute.SetEnabled<EPActivity.completedDateTime>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<EPActivity.overtimeSpent>(cache, e.Row, false);
			PXUIFieldAttribute.SetEnabled<EPActivity.timeBillable>(cache, e.Row, false);
			PXUIFieldAttribute.SetEnabled<EPActivity.overtimeBillable>(cache, e.Row, false);

			PXUIFieldAttribute.SetEnabled<EPActivity.reminderDate>(cache, row, row.IsReminderOn == true);

			PXUIFieldAttribute.SetEnabled<EPActivity.groupID>(cache, e.Row, false);
			PXUIFieldAttribute.SetEnabled<EPActivity.owner>(cache, e.Row, false);

			GotoParentActivity.SetEnabled(row.ParentTaskID != null);

			this.ChildActivities.Cache.AllowInsert =
			this.ChildActivities.Cache.AllowDelete =
			this.Attendees.Cache.AllowInsert =
			this.Attendees.Cache.AllowUpdate =
			this.Attendees.Cache.AllowDelete = editable;

			int timespent = 0;
			int overtimespent = 0;
			int timebillable = 0;
			int overtimebillable = 0;

			foreach (EPActivity child in ChildActivities.Select(row.TaskID))
			{
				timespent += (child.TimeSpent ?? 0);
				overtimespent += (child.OvertimeSpent ?? 0);
				timebillable += (child.TimeBillable ?? 0);
				overtimebillable += (child.OvertimeBillable ?? 0);
			}

			row.TimeSpent = timespent;
			row.OvertimeSpent = overtimespent;
			row.TimeBillable = timebillable;
			row.OvertimeBillable = overtimebillable;

			PXResourceScheduleAttribute.AllowUpdate(AttendeeResources.View, cache.AllowUpdate);
			AllowOperationsWithView(Attendees, cache.AllowUpdate);
			AllowOperationsWithView(OtherAttendees, cache.AllowUpdate);
			AllowOperationsWithView(AllAttendees, cache.AllowUpdate);

			bool isOwner = IsCurrentUserOwnerOfEvent(row);
			if (!isOwner)
			{
				cache.AllowInsert = false;
				cache.AllowUpdate = false;
				cache.AllowDelete = false;
				PXUIFieldAttribute.SetEnabled(cache, e.Row, false);
				
				Actions[_SAVE_ACTION_NAME].SetEnabled(false);
				Actions[_SAVECLOSE_ACTION_NAME].SetEnabled(false);

				acceptInvitation.SetVisible(true);
				rejectInvitation.SetVisible(true);
			}
			else
			{
				sendPersonalInvitation.SetVisible(true);
				sendInvitations.SetVisible(true);
			}
			CompleteAndFollowUp.SetVisible(false);
			Complete.SetVisible(isOwner);
			CancelActivity.SetVisible(isOwner);

			if (CurrentAttendee.Current == null)
			{
				PXResultset<EPAttendeeForAcceptReject> res = CurrentAttendee.Select();
				if (res != null && res.Count > 0) CurrentAttendee.Current = (EPAttendeeForAcceptReject)res[0];
			}

			int? attendeeInvitation = CurrentAttendee.Current == null ? PXInvitationStatusAttribute.NOTINVITED : CurrentAttendee.Current.Invitation;
			acceptInvitation.SetEnabled(attendeeInvitation != PXInvitationStatusAttribute.ACCEPTED && attendeeInvitation != PXInvitationStatusAttribute.CANCELED);
			rejectInvitation.SetEnabled(attendeeInvitation != PXInvitationStatusAttribute.REJECTED && attendeeInvitation != PXInvitationStatusAttribute.CANCELED);
		}

		protected virtual void EPActivity_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			var row = (EPActivity)e.Row;
			if (row == null) return;

			if (!string.IsNullOrEmpty(row.TimeCardCD) || row.Billed == true)
				throw new PXException(CR.Messages.EventCannotBeDeleted);
		}

		protected virtual void EPActivity_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			var command = e.Operation & PXDBOperation.Command;
			if (command != PXDBOperation.Insert && command != PXDBOperation.Update) return;

			var row = e.Row as EPActivity;
			if (row == null) return;

			VerifyReminder(row);

			var dbEvent = ReadDBEvent(row.TaskID);
			if (dbEvent == null) return;

			bool isDateChanged = dbEvent.StartDate != row.StartDate || dbEvent.EndDate != row.EndDate;
			if (isDateChanged && !IsEventInThePast(row) && IsEventEditable(row))
			{
				var invitedAttendees = GetInvitedAttendees();
				if (invitedAttendees.Count > 0)
				{
					WebDialogResult confirm = Events.Ask(
						_ASK_KEY_7,
						Messages.ConfirmRescheduleNotificationHeader,
						Messages.ConfirmRescheduleNotificationText,
						MessageButtons.YesNo);
					if (confirm == WebDialogResult.Yes) SendEMails(NotificationTypes.Reschedule, invitedAttendees);
				}
			}					
		}
		protected virtual void EPActivity_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
		{
			var row = e.Row as EPActivity;
			if (row == null ||
				e.TranStatus != PXTranStatus.Open ||
				cache.GetValuePending(row, PXImportAttribute.ImportFlag) != null)
			{
				return;
			}

			var operation = e.Operation & PXDBOperation.Command;
			if (operation == PXDBOperation.Insert || operation == PXDBOperation.Update)
				EPViewStatusAttribute.MarkAsViewed(this, row, true);
		}

		#endregion

		#region AddAttendeeFilter

		protected virtual void AddAttendeeFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			cache.IsDirty = false;
		}

		protected virtual void AddAttendeeFilter_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		#endregion

		#region EPOtherAttendeeWithNotification

		protected virtual void EPOtherAttendeeWithNotification_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			EPOtherAttendeeWithNotification row = e.Row as EPOtherAttendeeWithNotification;
			if (row != null && Events.Current != null) row.EventID = Events.Current.TaskID;
		}

		protected virtual void EPOtherAttendeeWithNotification_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			EPOtherAttendeeWithNotification row = e.Row as EPOtherAttendeeWithNotification;
			if (row != null && IsCurrentEventPersisted && !IsCurrentEventInThePast && IsCurrentEventEditable)
			{
				ConfirmDeleteOtherAttendees.Current.AttendeeID = row.AttendeeID;
				ConfirmDeleteOtherAttendees.Current.WithNotification = true;
				WebDialogResult confirmDelete = ConfirmDeleteOtherAttendees.Ask(
					_ASK_KEY_2,
					Messages.ConfirmDeleteAttendeeHeader,
					string.Format("{0} {1} {2}?", Messages.ConfirmDeleteAttendeeText, row.Email, row.Name),
					MessageButtons.YesNo);
				if (confirmDelete != WebDialogResult.Yes) e.Cancel = true;
				else if (row.Invitation != PXInvitationStatusAttribute.REJECTED)
				{
					SendEMail(NotificationTypes.Cancel, new AllTypeAttendee
								{
									Type = _OTHER_ATTENDEE_TYPE,
									Key = row.AttendeeID.ToString(),
									EventID = row.EventID,
									Name = row.Name,
									DisplayName = row.Name,
									Email = row.Email,
									Comment = row.Comment,
									Invitation = row.Invitation
								});
				}
			}
		}

		protected virtual void EPOtherAttendeeWithNotification_Email_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{

			if (e.NewValue == null || !CheckEmail(e.NewValue.ToString()))
				throw new PXSetPropertyException(Messages.InvalidEmail, PXErrorLevel.Error);
		}

		protected virtual void EPOtherAttendeeWithNotification_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
		{
			EPOtherAttendeeWithNotification row = e.Row as EPOtherAttendeeWithNotification;
			EPOtherAttendeeWithNotification newRow = e.NewRow as EPOtherAttendeeWithNotification;
			if (row == null || newRow == null ||
				!IsCurrentEventPersisted || IsCurrentEventInThePast || !IsCurrentEventEditable) return;

			newRow.NotifyOnRowUpdated = false;
			if (newRow.Email != row.Email && row.Invitation != PXInvitationStatusAttribute.NOTINVITED)
			{
				WebDialogResult confirm = AllAttendees.Ask(
					_ASK_KEY_3,
					Messages.EMailWasChanged,
					string.Format("{0} ({1})?", Messages.SendInvitationToNewEMail, newRow.Email),
					MessageButtons.YesNo);
				switch (confirm)
				{
					case WebDialogResult.Yes:
						newRow.NotifyOnRowUpdated = true;
						break;
					case WebDialogResult.Cancel:
						e.Cancel = true;
						break;
				}
				newRow.Invitation = PXInvitationStatusAttribute.INVITED;
			}
		}

		protected virtual void EPOtherAttendeeWithNotification_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			EPOtherAttendeeWithNotification row = e.Row as EPOtherAttendeeWithNotification;
			if (row != null && row.NotifyOnRowUpdated == true)
				SendEMail(NotificationTypes.Invitation, SearchAllTypeAttendee(_OTHER_ATTENDEE_TYPE, row.AttendeeID));
			AllAttendees.ClearDialog();
		}

		#endregion

		#region DeleteOtherAttendeeFilter

		protected virtual void DeleteOtherAttendeeFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			DeleteOtherAttendeeFilter row = e.Row as DeleteOtherAttendeeFilter;
			if (row != null && row.AttendeeID.HasValue)
			{
				EPOtherAttendeeWithNotification attendee =
					OtherAttendees.Search<EPOtherAttendeeWithNotification.attendeeID>(row.AttendeeID);
				PXUIFieldAttribute.SetEnabled<DeleteOtherAttendeeFilter.withNotification>(cache, row,
					attendee == null || attendee.Invitation != PXInvitationStatusAttribute.NOTINVITED);
			}
		}

		#endregion

		#region AllTypeAttendee

		protected virtual void AllTypeAttendee_Comment_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
		{
			_allTypeAttendee_FieldSelecting(e, "Comment");
		}

		protected virtual void AllTypeAttendee_Email_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
		{
			_allTypeAttendee_FieldSelecting(e, "Email");
		}

		protected virtual void AllTypeAttendee_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void AllTypeAttendee_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var row = e.Row as AllTypeAttendee;
			if (row != null && row.Type == _USER_ATTENDEE_TYPE && IsOwner(row.Key))
			{
				PXUIFieldAttribute.SetEnabled(cache, row, false);
				//PXUIFieldAttribute.SetEnabled<AllTypeAttendee.name>(cache, row, false);
				//PXUIFieldAttribute.SetEnabled<AllTypeAttendee.displayName>(cache, row, false);
			}
		}

		#endregion

		#region Users
		
		protected virtual void Users_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetVisible<Users.pKID>(cache, e.Row, false);
		}

		#endregion

		#endregion

		#region Public Methods

		[ResHandler(_ATTENDEE_VIEW_NAME, ResHandler.Types.GetWorkStartTime)]
		public DateTime? GetWorkStartTime(object row, DateTime date)
		{
			var user = row as Users;
			if (user == null) return date.Date;

			DateTime? result = null;
			PXResultset<CSCalendar> calendarSet = WorkCalendar.SelectWindowed(0, 1, user.PKID);
			CSCalendar calendar = calendarSet.Count > 0 ? (CSCalendar)calendarSet[0] : null;
			if (calendar != null)
				switch (date.DayOfWeek)
				{
					case DayOfWeek.Monday:
						if (calendar.MonWorkDay == true) result = calendar.MonStartTime;
						break;
					case DayOfWeek.Tuesday:
						if (calendar.TueWorkDay == true) result = calendar.TueStartTime;
						break;
					case DayOfWeek.Wednesday:
						if (calendar.WedWorkDay == true) result = calendar.WedStartTime;
						break;
					case DayOfWeek.Thursday:
						if (calendar.ThuWorkDay == true) result = calendar.ThuStartTime;
						break;
					case DayOfWeek.Friday:
						if (calendar.FriWorkDay == true) result = calendar.FriStartTime;
						break;
					case DayOfWeek.Saturday:
						if (calendar.SatWorkDay == true) result = calendar.SatStartTime;
						break;
					case DayOfWeek.Sunday:
						if (calendar.SunWorkDay == true) result = calendar.SunStartTime;
						break;
				}
			if (result != null) result = AddTimeToDate(date, result);
			return result;
		}

		[ResHandler(_ATTENDEE_VIEW_NAME, ResHandler.Types.GetWorkEndTime)]
		public DateTime? GetWorkEndTime(object row, DateTime date)
		{
			var user = row as Users;
			if (user == null) return date.Date.AddDays(1D);

			DateTime? result = null;
			PXResultset<CSCalendar> calendarSet = WorkCalendar.SelectWindowed(0, 1, user.PKID);
			CSCalendar calendar = calendarSet.Count > 0 ? (CSCalendar)calendarSet[0] : null;
			if (calendar != null)
				switch (date.DayOfWeek)
				{
					case DayOfWeek.Monday:
						if (calendar.MonWorkDay == true) result = calendar.MonEndTime;
						break;
					case DayOfWeek.Tuesday:
						if (calendar.TueWorkDay == true) result = calendar.TueEndTime;
						break;
					case DayOfWeek.Wednesday:
						if (calendar.WedWorkDay == true) result = calendar.WedEndTime;
						break;
					case DayOfWeek.Thursday:
						if (calendar.ThuWorkDay == true) result = calendar.ThuEndTime;
						break;
					case DayOfWeek.Friday:
						if (calendar.FriWorkDay == true) result = calendar.FriEndTime;
						break;
					case DayOfWeek.Saturday:
						if (calendar.SatWorkDay == true) result = calendar.SatEndTime;
						break;
					case DayOfWeek.Sunday:
						if (calendar.SunWorkDay == true) result = calendar.SunEndTime;
						break;
				}
			if (result != null) result = AddTimeToDate(date, result);
			return result;
		}

		public override void CompleteRow(EPActivity row)
		{
			if (row != null) CompleteEvent(row);
		}

		public override void CancelRow(EPActivity row)
		{
			if (row != null) CancelEvent(row);
		}

		#endregion

		#region Private Methods

		private DateTime? RoundTo30Min(DateTime? rawDate)
		{
			if (rawDate == null) return null;

			var date = (DateTime) rawDate;
			var minutes = date.Minute;
			if (minutes != 0)
				minutes = minutes <= 30 ? 30 : 60;

			return new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0).AddMinutes(minutes);
		}

		private void VerifyReminder(EPActivity row)
		{
			if (row.IsReminderOn == true && row.ReminderDate == null)
			{
				var reminderDateDisplayName = PXUIFieldAttribute.GetDisplayName<EPActivity.reminderDate>(Events.Cache);
				var exception = new PXSetPropertyException(ErrorMessages.FieldIsEmpty, reminderDateDisplayName);
				if (Events.Cache.RaiseExceptionHandling<EPActivity.reminderDate>(row, null, exception))
				{
					throw new PXRowPersistingException(typeof(EPActivity.reminderDate).Name, null, ErrorMessages.FieldIsEmpty, reminderDateDisplayName);
				}
			}
		}

		private void AcceptParticipation(bool accept)
		{
			if (Events.Current == null) return;

			foreach (var row in CurrentAttendee.Select())
			{
				var attendee = (EPAttendeeForAcceptReject)row;
				attendee.Invitation = accept ? PXInvitationStatusAttribute.ACCEPTED : PXInvitationStatusAttribute.REJECTED;
				CurrentAttendee.Update(attendee);
			}
			using (var tscope = new PXTransactionScope())
			{
				this.Persist(typeof(EPAttendeeForAcceptReject), PXDBOperation.Update);
				tscope.Complete();
			}
		}

		private void CompleteEvent(EPActivity row)
		{
			string origStatus = (string)this.Events.Cache.GetValueOriginal<EPActivity.uistatus>(row) ?? ActivityStatusAttribute.Open;

			if (row == null ||
				origStatus == ActivityStatusAttribute.Completed ||
				origStatus == ActivityStatusAttribute.Canceled)
			{
				return;
			}

			var activityCopy = (EPActivity)Events.Cache.CreateCopy(row);
			activityCopy.UIStatus = ActivityStatusAttribute.Completed;
			Events.Cache.Update(activityCopy);
			EPViewStatusAttribute.MarkAsViewed(this, row);
			Actions.PressSave();
		}

		private void CancelEvent(EPActivity row)
		{
			string origStatus = (string)this.Events.Cache.GetValueOriginal<EPActivity.uistatus>(row) ?? ActivityStatusAttribute.Open;
			if (row == null ||
				origStatus == ActivityStatusAttribute.Completed ||
				origStatus == ActivityStatusAttribute.Canceled)
			{
				return;
			}

			var activityCopy = (EPActivity)Events.Cache.CreateCopy(row);
			activityCopy.UIStatus = ActivityStatusAttribute.Canceled;
			Events.Cache.Update(activityCopy);
			EPViewStatusAttribute.MarkAsViewed(this, row);
			Actions.PressSave();
		}

		private static bool CheckEmail(string email)
		{
			return !string.IsNullOrEmpty(email) && _emailValidationRegex.IsMatch(email);
		}

		private static DateTime AddTimeToDate(DateTime date, DateTime? result)
		{
			return new DateTime(date.Year, date.Month, date.Day, result.Value.Hour, result.Value.Minute, result.Value.Second);
		}

		private bool IsEventInThePast(EPActivity row)
		{
			return false;
			// TODO: need implementation
		}

		private bool IsCurrentEventInThePast
		{
			get
			{
				return IsEventInThePast(Events.Current);
			}
		}

		private static void _allTypeAttendee_FieldSelecting(PXFieldSelectingEventArgs e, string fieldName)
		{
			AllTypeAttendee row = e.Row as AllTypeAttendee;
			if (row != null && _USER_ATTENDEE_TYPE.Equals(row.Type))
			{
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, null, null,
					null, null, null, null, fieldName, null, null, null, PXErrorLevel.Undefined, false, null, false,
					PXUIVisibility.Undefined, null, null, null);
			}
		}

		private Users EventOwner
		{
			get
			{
				PXResultset<Users> currentOwnerDBResult = CurrentOwner.Select();
				return currentOwnerDBResult.Count > 0 ? (Users)currentOwnerDBResult[0] : null;
			}
		}

		private IEnumerable<EPAttendee> GetAttendeesByEventOwner(EPActivity @event)
		{
			if (@event == null) yield break;

			foreach (PXResult<EPAttendee> item in PXSelect<EPAttendee,
				Where<EPAttendee.eventID, Equal<Required<EPAttendee.eventID>>,
					And<EPAttendee.userID, Equal<Required<EPAttendee.userID>>>>>.
					Select(this, @event.TaskID, @event.Owner))
			{
				yield return (EPAttendee)item;
			}
		}

		private void SendEMail(NotificationTypes invite, object attendee)
		{
			SendEMails(invite, new object[] { attendee });
		}

		private void SendEMails(NotificationTypes invite, ICollection attendees)
		{
			// Check
			var mails = new List<AllTypeAttendee>();
			foreach (object item in attendees)
			{
				AllTypeAttendee attendee = item is PXResult ? item as PXResult<AllTypeAttendee> : (AllTypeAttendee)item;
				if (!CheckEmail(attendee.Email) &&
					Events.Ask(
						_ASK_KEY_4,
						Messages.EmailIncorrect,
						attendees.Count > 1 ? Messages.ConfirmIncorrectEmailInvitations : Messages.ConfirmOneIncorrectEmailInvitation,
						MessageButtons.YesNo) != WebDialogResult.Yes)
				{
					continue;
				}
				mails.Add(attendee);
			}

			//Send
			byte[] card;
			using (var buffer = new MemoryStream())
			{
				CreateVEvent().Write(buffer);
				card = buffer.ToArray();
			}

			var owner = EventOwner;
			var @event = Events.Current;
			var newLine = Environment.NewLine + Environment.NewLine;
			string contactInfo = null;
			var settings = Setup.Current;
			if (owner != null && settings.AddContactInformation == true)
			{
				contactInfo = "Contact person: " + newLine;
				contactInfo += "Name: " + owner.DisplayName + newLine;
				if (!string.IsNullOrWhiteSpace(owner.Email))
					contactInfo += "Email: " + owner.Email + newLine;
				if (!string.IsNullOrWhiteSpace(owner.Phone))
					contactInfo += "Phone: " + owner.Phone + newLine;
			}
			foreach (var attendee in mails)
			{
				// body
				var sender = settings.IsSimpleNotification == true ?
					SimpleFillMail(invite, attendee, @event, owner, contactInfo) :
					TemplateFillMail(invite, @event);

				// address
				sender.MailAccountId = MailAccountManager.DefaultMailAccountID;
				sender.To = attendee.Email;

				// subject
				string subjectPrefix;
				switch (invite)
				{
					case NotificationTypes.Cancel:
						subjectPrefix = "Cancel invitation to ";
						break;
					case NotificationTypes.Reschedule:
						subjectPrefix = "Reschedule of ";
						break;
					case NotificationTypes.Invitation:
					default:
						subjectPrefix = "Invitation to ";
						break;
				}
				sender.Subject = subjectPrefix + @event.Subject;

				// attachments
				sender.AddAttachment("event.ics", card);

				sender.ParentTaskID = @event.TaskID;
				
				foreach (EPActivity item in sender.Send())
				{
					var msg = item;
					// Update attendees flags
					switch ((int)attendee.Type)
					{
						case _OTHER_ATTENDEE_TYPE:
							var otherMessage = new EPOtherAttendeeMessage();
							otherMessage.EventID = @event.TaskID;
							otherMessage.AttendeeID = Convert.ToInt32(attendee.Key);
							otherMessage.MessageID = msg.ImcUID;
							OtherAttendeeMessages.Insert(otherMessage);
							break;
						case _USER_ATTENDEE_TYPE:
							var message = new EPAttendeeMessage();
							message.EventID = @event.TaskID;
							message.UserID = new Guid(attendee.Key);
							message.MessageID = msg.ImcUID;
							AttendeeMessages.Insert(message);
							break;
					}
				}
				switch (invite)
				{
					case NotificationTypes.Invitation:
						attendee.Invitation = PXInvitationStatusAttribute.INVITED;
						break;
					case NotificationTypes.Reschedule:
						attendee.Invitation = PXInvitationStatusAttribute.RESCHEDULED;
						break;
					case NotificationTypes.Cancel:
						attendee.Invitation = PXInvitationStatusAttribute.CANCELED;
						break;
				}
				UpdateAllTypeAttendee(attendee);
			}
			if (mails.Count > 0) SafetyPersist(Attendees, AttendeeMessages, OtherAttendees, OtherAttendeeMessages);
		}

		private NotificationGenerator SimpleFillMail(NotificationTypes invite, AllTypeAttendee attendee, EPActivity @event, Users owner, string contactInfo)
		{
			var sender = new NotificationGenerator();
			string headerAddInfo;
			string bodyAddInfo = string.Empty;
			switch (invite)
			{
				case NotificationTypes.Cancel:
					headerAddInfo = "Event was canceled.";
					break;
				case NotificationTypes.Reschedule:
					headerAddInfo = "Event was rescheduled.";
					bodyAddInfo = GetEventStringInfo(@event, _WIKI_NEW_LINE, "New ");
					break;
				case NotificationTypes.Invitation:
				default:
					if (owner != null) headerAddInfo = owner.DisplayName + " invited you to an event. ";
					else headerAddInfo = "You are invited to an event.";
					bodyAddInfo = GetEventStringInfo(@event, _WIKI_NEW_LINE, string.Empty);
					break;
			}

			var body = headerAddInfo + _WIKI_NEW_LINE;
			body += "Event Number: " + (int)@event.TaskID + _WIKI_NEW_LINE;
			body += "Subject: " + @event.Subject.Trim() + _WIKI_NEW_LINE;
			if (!string.IsNullOrWhiteSpace(@event.Location))
				body += "Location: " + @event.Location.Trim() + _WIKI_NEW_LINE;
			body += bodyAddInfo + _WIKI_NEW_LINE;
			body += _WIKI_NEW_LINE + contactInfo;

			sender.Body = body;
			return sender;
		}

		private TemplateNotificationGenerator TemplateFillMail(NotificationTypes invite, EPActivity @event)
		{
			var settings = Setup.Current;
			int? templateId;
			switch (invite)
			{
				case NotificationTypes.Cancel:
					templateId = settings.CancelInvitationTemplateID;
					break;
				case NotificationTypes.Reschedule:
					templateId = settings.RescheduleTemplateID;
					break;
				case NotificationTypes.Invitation:
				default:
					templateId = settings.InvitationTemplateID;
					break;
			}

			if (templateId == null)
				throw new Exception(Messages.EmailTemplateIsNotConfigured);

			var sender = TemplateNotificationGenerator.Create(@event, (int)templateId);
			return sender;
		}

		private void SafetyPersist(params PXSelectBase[] views)
		{
			using (var tscope = new PXTransactionScope())
			{
				foreach (var view in views)
					view.Cache.Persist(PXDBOperation.Insert);
				foreach (var view in views)
					view.Cache.Persist(PXDBOperation.Update);
				foreach (var view in views)
					view.Cache.Persist(PXDBOperation.Delete);
				tscope.Complete(this);
			}
			foreach (var view in views)
			{
				view.View.Clear();
				view.Cache.Clear();
				view.Cache.IsDirty = false;
			}
		}

		private void UpdateAllTypeAttendee(AllTypeAttendee row)
		{
			if (row == null) return;

			switch (Convert.ToInt32(row.Type))
			{
				case _USER_ATTENDEE_TYPE:
					EPAttendee attendee = Attendees.Search<EPAttendee.userID>(row.Key);
					if (attendee != null)
					{
						attendee.Invitation = row.Invitation;
						Attendees.Update(attendee);
					}
					break;
				case _OTHER_ATTENDEE_TYPE:
					EPOtherAttendeeWithNotification otherAttendee =
						OtherAttendees.Search<EPOtherAttendeeWithNotification.attendeeID>(row.Key);
					if (otherAttendee != null)
					{
						otherAttendee.Name = row.Name;
						otherAttendee.Email = row.Email;
						otherAttendee.Comment = row.Comment;
						otherAttendee.Invitation = row.Invitation;
						OtherAttendees.Update(otherAttendee);
					}
					break;
			}
		}

		private bool IsCurrentEventPersisted
		{
			get
			{
				PXEntryStatus status = Events.Cache.GetStatus(Events.Current);
				return status != PXEntryStatus.Inserted && status != PXEntryStatus.InsertedDeleted;
			}
		}

		private EPActivity ReadDBEvent(object taskId)
		{
			PXResultset<EPActivity> resultset =
				PXSelectReadonly<EPActivity,
					Where<EPActivity.taskID, Equal<Required<EPActivity.taskID>>>>.
				Select(this, taskId);
			return resultset.Count > 0 ? (EPActivity)resultset[0] : null;
		}

		private void AssertEventStatus()
		{
			if (!IsCurrentEventPersisted) throw new Exception(Messages.EventIsNotSaved);
			if (IsCurrentEventInThePast) throw new Exception(Messages.EventIsThePast);
			if (!IsCurrentEventEditable) throw new Exception(Messages.EventIsNotEditable);
		}

		private ConfirmNotificationResult ConfirmCancelAttendeeInvitations()
		{
			if (IsCurrentEventPersisted && !IsCurrentEventInThePast && IsCurrentEventEditable)
			{
				var invitedAttendees = GetInvitedAttendees();
				if (invitedAttendees.Count > 0)
				{
					WebDialogResult confirmResult = AllAttendees.Ask(
						_ASK_KEY_5,
						Messages.CancelInvitation,
						Messages.ConfirmCancelAttendeeInvitations,
						MessageButtons.YesNoCancel);
					if (confirmResult == WebDialogResult.Yes) return ConfirmNotificationResult.Yes;
				}
			}
			return ConfirmNotificationResult.No;
		}

		private List<AllTypeAttendee> GetInvitedAttendees()
		{
			var invitedAttendees = new List<AllTypeAttendee>();
			foreach (var item in AllAttendees.Select())
			{
				var attendee = (AllTypeAttendee)item;
				if (attendee.Invitation != PXInvitationStatusAttribute.NOTINVITED && 
				    attendee.Invitation != PXInvitationStatusAttribute.REJECTED)
					invitedAttendees.Add(attendee);
			}
			return invitedAttendees;
		}

		private void CancelAttendeeInvitations(ConfirmNotificationResult target)
		{
			SendEMails(NotificationTypes.Cancel, GetInvitedAttendees());
		}

		private ConfirmNotificationResult ConfirmAttendeeInvitations()
		{
			if (IsCurrentEventPersisted && !IsCurrentEventInThePast && IsCurrentEventEditable)
			{
				PXResultset<AllTypeAttendee> attendees = AllAttendees.Select();
				PXResultset<AllTypeAttendee> notInvitedAttendees =
					AllAttendees.SearchAll<Asc<AllTypeAttendee.invitation>>(new object[] { PXInvitationStatusAttribute.NOTINVITED });
				if (attendees.Count > 0 && notInvitedAttendees.Count > 0)
				{
					WebDialogResult confirmResult = AllAttendees.Ask(
						_ASK_KEY_6,
						Messages.SendInvitations,
						Messages.NotifyAttendees,
						MessageButtons.YesNoCancel);
					if (confirmResult == WebDialogResult.Yes)
					{
						if (notInvitedAttendees.Count == attendees.Count) return ConfirmNotificationResult.YesForAll;
						return ConfirmNotificationResult.Yes;
					}
				}
			}
			return ConfirmNotificationResult.No;
		}

		private void InviteAttendees(ConfirmNotificationResult target)
		{
			switch (target)
			{
				case ConfirmNotificationResult.YesForAll:
					SendEMails(NotificationTypes.Invitation, AllAttendees.Select());
					break;
				case ConfirmNotificationResult.Yes:
					SendEMails(NotificationTypes.Invitation, 
						AllAttendees.SearchAll<Asc<AllTypeAttendee.invitation>>(new object[] { PXInvitationStatusAttribute.NOTINVITED }));
					break;
			}
		}

		private static void AllowOperationsWithView(PXSelectBase view, bool canModifyAttendees)
		{
			view.Cache.AllowDelete = canModifyAttendees;
			view.Cache.AllowInsert = canModifyAttendees;
			view.Cache.AllowUpdate = canModifyAttendees;
		}

		private static void GetValuesExt(PXCache cache, IDictionary values)
		{
			var list = new List<string>(values.Count);
			foreach (string key in values.Keys)
				list.Add(key);
			foreach (string key in list)
				values[key] = cache.GetValueExt(cache.Current, key);
		}

		private void GetAttendeeValuesExt(IDictionary values, object userId)
		{
			AllAttendees.Current = SearchAllTypeAttendee(_USER_ATTENDEE_TYPE, userId);
			GetValuesExt(AllAttendees.Cache, values);
		}

		private void GetOtherAttendeeValuesExt(IDictionary values, object attendeeId)
		{
			AllAttendees.Current = SearchAllTypeAttendee(_OTHER_ATTENDEE_TYPE, attendeeId);
			GetValuesExt(AllAttendees.Cache, values);
		}

		private AllTypeAttendee SearchAllTypeAttendee(int type, object key)
		{
			return AllAttendees.SearchWindowed<Asc<AllTypeAttendee.type, Asc<AllTypeAttendee.key>>>(
				new object[] { type, Convert.ToString(key) }, 0, 1);
		}

		private string GetEventStringInfo(EPActivity @event, string newLineString, string prefix)
		{
			var start = @event.StartDate.Value;
			var end = @event.EndDate.Value;
			var timeZone = LocaleInfo.GetTimeZone().DisplayName;
			string bodyAddInfo = prefix + "Start Date: " + start.ToLongDateString() + " " + start.ToShortTimeString() + " " + timeZone + newLineString +
								 prefix + "End Date: " + end.ToLongDateString() + " " + end.ToShortTimeString() + " " + timeZone;
			EPActivity gEvent = @event as EPActivity;
			if (gEvent != null)
			{
				PXStringState valueExt = Events.Cache.GetValueExt(gEvent, "Duration") as PXStringState;
				if (valueExt != null)
				{
					bodyAddInfo += newLineString + prefix + "Duration: ";
					string valueText = valueExt.Value.ToString();
					bodyAddInfo += string.IsNullOrEmpty(valueExt.InputMask) ? valueText :
						PX.Common.Mask.Format(valueExt.InputMask, valueText);
				}
			}
			if (!string.IsNullOrEmpty(@event.Body))
			{
				var description = Tools.ConvertHtmlToSimpleText(@event.Body);
				description = description.Replace(Environment.NewLine, newLineString);
				bodyAddInfo += newLineString + description;
			}
			return bodyAddInfo;
		}

		private bool IsCurrentUserOwnerOfEvent(EPActivity row)
		{
			object currentLoginUser = Caches[typeof(AccessInfo)].Current;
			return row == null || row.Owner == null || currentLoginUser == null ||
				row.Owner == ((AccessInfo)currentLoginUser).UserID;
		}

		private bool IsEventEditable(EPActivity row)
		{
			string origStatus = (string)this.Events.Cache.GetValueOriginal<EPActivity.uistatus>(row) ?? ActivityStatusAttribute.Open;

			return row == null ||
			       origStatus != ActivityStatusAttribute.Completed ||
			       origStatus != ActivityStatusAttribute.Canceled ;
		}

		private bool IsCurrentEventEditable
		{
			get
			{
				return IsEventEditable(Events.Current);
			}
		}

		private vEvent CreateVEvent()
		{
			var vevent = VCalendarProcessor.CreateVEvent(Events.Current);
			vevent.Method = "REQUEST";
			return vevent;
		}

		private bool IsOwner(object pkId)
		{
			Guid userId;
			if (pkId == null || !GUID.TryParse(pkId.ToString(), out userId)) return false;

			var owner = EventOwner;
			return owner != null && (Guid)owner.PKID == userId;
		}

		#endregion
	}
}
