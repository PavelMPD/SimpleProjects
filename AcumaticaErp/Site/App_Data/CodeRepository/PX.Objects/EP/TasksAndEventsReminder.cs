using System;
using System.Collections;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.EP
{
	#region Class Names Usings

	using EPViewJoin = LeftJoin<EPView, On<EPView.noteID, Equal<EPActivity.noteID>,
		And<EPView.userID, Equal<Current<AccessInfo.userID>>>>>;

	using EPViewJoinWhere = Where<EPView.status, IsNull, Or<EPView.status, Equal<EPViewStatusAttribute.NotViewed>>>;

	using EPAttendeeJoin = LeftJoin<EPAttendee, On<EPAttendee.userID, Equal<Current<AccessInfo.userID>>,
		And<EPAttendee.eventID, Equal<EPActivity.taskID>>>>;

	using EPGenericActivityAttendeeWhere = Where<EPActivity.owner, Equal<Current<AccessInfo.userID>>,
			Or<EPAttendee.userID, IsNotNull>>;

	using ReminderJoin = LeftJoin<EPAttendee,
		On<EPAttendee.userID, Equal<Current<AccessInfo.userID>>, And<EPAttendee.eventID, Equal<EPActivity.taskID>>>,
		LeftJoin<EPReminder,
			On<EPReminder.noteID, Equal<EPActivity.noteID>, And<EPReminder.userID, Equal<Current<AccessInfo.userID>>>>>>;

	using OpenEPGenericActivityWhere =
				 Where<EPActivity.uistatus, NotEqual<ActivityStatusAttribute.canceled>,
					 And<EPActivity.uistatus, NotEqual<ActivityStatusAttribute.completed>,
					 And<EPActivity.uistatus, NotEqual<ActivityStatusAttribute.released>>>>;

	using NonCanceledActivityWhere =
				 Where<EPActivity.uistatus, NotEqual<ActivityStatusAttribute.canceled>,
					 And<EPActivity.uistatus, NotEqual<ActivityStatusAttribute.released>>>;

		#endregion

	[PXHidden]
	public class TasksAndEventsReminder : PXGraph<TasksAndEventsReminder>
	{
		#region Additional Classes

		#region TaskClassId

		public sealed class TaskClassId : Constant<int>
		{
			public const int _CLASS_ID = 0;

			public TaskClassId() : base(_CLASS_ID) { }
		}

		#endregion

		#region EventClassId

		public sealed class EventClassId : Constant<int>
		{
			public const int _CLASS_ID = 1;

			public EventClassId() : base(_CLASS_ID) { }
		}

		#endregion

		#region SelectBaseEPGenericActivity

		public abstract class SelectBaseEPGenericActivity :
			PXSelectJoinGroupBy<EPActivity,
			EPAttendeeJoin,
			EPGenericActivityAttendeeWhere,
			Aggregate<Count>>
		{
			protected SelectBaseEPGenericActivity(PXGraph graph)
				: base(graph)
			{
				Initalize();
			}

			protected SelectBaseEPGenericActivity(PXGraph graph, Delegate handler)
				: base(graph, handler)
			{
				Initalize();
			}

			protected abstract BqlCommand GetBqlCommand(BqlCommand source);

			public int Count()
			{
				return Select().RowCount ?? 0;
			}

			private void Initalize()
			{
				var newSelect = GetBqlCommand(View.BqlSelect);
				if (newSelect != null) View = new PXView(_Graph, true, newSelect);
			}
		}

		public class SelectBaseEPGenericActivity<TWhere> : SelectBaseEPGenericActivity
			where TWhere : IBqlWhere, new()
		{
			public SelectBaseEPGenericActivity(PXGraph graph) : base(graph) { }

			public SelectBaseEPGenericActivity(PXGraph graph, Delegate handler) : base(graph, handler) { }

			protected override BqlCommand GetBqlCommand(BqlCommand source)
			{
				return source.WhereAnd(typeof(TWhere));
			}
		}

		public class SelectBaseEPGenericActivity<TWhere, TWhere2> : SelectBaseEPGenericActivity<TWhere>
			where TWhere : IBqlWhere, new()
			where TWhere2 : IBqlWhere, new()
		{
			public SelectBaseEPGenericActivity(PXGraph graph) : base(graph) { }

			public SelectBaseEPGenericActivity(PXGraph graph, Delegate handler) : base(graph, handler) { }

			protected override BqlCommand GetBqlCommand(BqlCommand source)
			{
				return base.GetBqlCommand(source).WhereAnd(typeof(TWhere2));
			}
		}

		public class SelectBaseEPGenericActivity<TJoin, TWhere, TWhere2> : SelectBaseEPGenericActivity<TWhere, TWhere2>
			where TWhere : IBqlWhere, new()
			where TWhere2 : IBqlWhere, new()
			where TJoin : IBqlJoin, new()
		{
			public SelectBaseEPGenericActivity(PXGraph graph) : base(graph) { }

			public SelectBaseEPGenericActivity(PXGraph graph, Delegate handler) : base(graph, handler) { }

			protected override BqlCommand GetBqlCommand(BqlCommand source)
			{
				return BqlCommand.AppendJoin<TJoin>(base.GetBqlCommand(source));
			}
		}

		public class SelectBaseEPGenericActivity2<TJoin> : SelectBaseEPGenericActivity
			where TJoin : IBqlJoin, new()
		{
			public SelectBaseEPGenericActivity2(PXGraph graph) : base(graph) { }

			public SelectBaseEPGenericActivity2(PXGraph graph, Delegate handler) : base(graph, handler) { }

			protected override BqlCommand GetBqlCommand(BqlCommand source)
			{
				return BqlCommand.AppendJoin<TJoin>(source);
			}
		}

		#endregion

		#region SelectNewEPGenericActivity (New Today)

		public sealed class SelectNewEPGenericActivity<TClassId> :
			SelectBaseEPGenericActivity<
				EPViewJoin,
				Where2<OpenEPGenericActivityWhere, And<EPActivity.classID, Equal<TClassId>>>,
				Where2<EPViewJoinWhere,
					And<EPActivity.startDate, GreaterEqual<Today>,
					And<EPActivity.startDate, Less<Tomorrow>>>>>
			where TClassId : IBqlOperand
		{
			public SelectNewEPGenericActivity(PXGraph graph) : base(graph) { }

			public SelectNewEPGenericActivity(PXGraph graph, Delegate handler) : base(graph, handler) { }
		}

		#endregion

		#region SelectNewAndOverdueNewEPGenericActivity (New today+overdue)

		public sealed class SelectNewAndOverdueNewEPGenericActivity<TClassId> :
			SelectBaseEPGenericActivity<
				EPViewJoin,
				Where2<OpenEPGenericActivityWhere, And<EPActivity.classID, Equal<TClassId>>>,
				EPViewJoinWhere>
			where TClassId : IBqlOperand
		{
			public SelectNewAndOverdueNewEPGenericActivity(PXGraph graph) : base(graph) { }

			public SelectNewAndOverdueNewEPGenericActivity(PXGraph graph, Delegate handler) : base(graph, handler) { }
		}

		#endregion

		#region SelectTodayEPGenericActivity (Open today)

		public sealed class SelectTodayEPGenericActivity<TClassId> :
			SelectBaseEPGenericActivity<
				Where<EPActivity.classID, Equal<TClassId>>,
				Where2<OpenEPGenericActivityWhere,
					And<EPActivity.startDate, GreaterEqual<Today>,
					And<EPActivity.startDate, Less<Tomorrow>>>>>
			where TClassId : IBqlOperand
		{
			public SelectTodayEPGenericActivity(PXGraph graph) : base(graph) { }

			public SelectTodayEPGenericActivity(PXGraph graph, Delegate handler) : base(graph, handler) { }
		}

		#endregion

		#region SelectOpenEPGenericActivity (Open all)

		public sealed class SelectOpenEPGenericActivity<TClassId> :
			SelectBaseEPGenericActivity<
				Where<EPActivity.classID, Equal<TClassId>>,
				OpenEPGenericActivityWhere>
			where TClassId : IBqlOperand
		{
			public SelectOpenEPGenericActivity(PXGraph graph) : base(graph) { }

			public SelectOpenEPGenericActivity(PXGraph graph, Delegate handler) : base(graph, handler) { }
		}

		#endregion

		#region SelectOpenAndOverdueEPGenericActivity (Open today+overdue)

		public sealed class SelectOpenAndOverdueEPGenericActivity<TClassId, TTomorroy> :
			SelectBaseEPGenericActivity<
				Where<EPActivity.classID, Equal<TClassId>>,
				Where2<OpenEPGenericActivityWhere, 
				And<Where<EPActivity.endDate, Less<TTomorroy>, 
						Or<EPActivity.startDate, IsNull, 
						Or<EPActivity.startDate, Less<TTomorroy>>>>>>>
			where TClassId : IBqlOperand
			where TTomorroy : IBqlOperand
		{
			public SelectOpenAndOverdueEPGenericActivity(PXGraph graph) : base(graph) { }

			public SelectOpenAndOverdueEPGenericActivity(PXGraph graph, Delegate handler) : base(graph, handler) { }
		}

		#endregion

		#region ReminderListWhere

		public sealed class ReminderListWhere : WrappedWhere<
			Where2<Where<EPActivity.classID, Equal<TaskClassId>,
				Or<EPActivity.classID, Equal<EventClassId>>>,
					And2<OpenEPGenericActivityWhere,
					And<EPActivity.isReminderOn, Equal<True>,
					And2<Where<EPReminder.date, IsNull, And<EPActivity.reminderDate, LessEqual<Now>,
							Or<EPReminder.date, Less<Now>, And<EPReminder.dismiss, NotEqual<True>>>>>,
					And<Where<EPActivity.owner, Equal<Current<AccessInfo.userID>>,
						Or<EPAttendee.userID, IsNotNull>>>>>>>>
		{

		}

		#endregion

		#region ActivityListWhere

		public sealed class ActivityListWhere : WrappedWhere<
			Where2<Where<EPActivity.classID, Equal<TaskClassId>,
				Or<EPActivity.classID, Equal<EventClassId>,
				Or<EPActivity.classID, Equal<CRActivityClass.activity>,
				Or<EPActivity.classID, Equal<CRActivityClass.email>>>>>,
					And2<NonCanceledActivityWhere,
					And<Where<EPActivity.refNoteID, GreaterEqual<Zero>, 
					And<Where<EPActivity.refNoteID, Equal<Required<EPActivity.refNoteID>>>>>>>>>
		{

		}

		#endregion

		#region SelectRemindEPGenericActivityCount

		public sealed class SelectRemindEPGenericActivityCount :
			PXSelectJoinGroupBy<EPActivity,
			ReminderJoin,
			ReminderListWhere,
			Aggregate<Count>>
		{
			public SelectRemindEPGenericActivityCount(PXGraph graph) : base(graph) { }
		}

		#endregion

		#region ActivityStatistics

		private abstract class ActivityStatistics : IPrefetchable<PXGraph>
		{
			protected int _open;
			protected int _today;
			protected int _new;

			private DateTime _day;

			public int Open
			{
				get { return _open; }
			}

			public int Today
			{
				get { return _today; }
			}

			public int New
			{
				get { return _new; }
			}

			public virtual void Prefetch(PXGraph parameter)
			{
				_day = DateTime.Today;
			}

			protected static ActivityStatistics GetFromSlot<TStatistics>(string key, PXGraph graph, params Type[] tables)
				where TStatistics : class, IPrefetchable<PXGraph>, new()
			{
				var slot = GetSlot<TStatistics>(key, graph, tables);
				if (slot != null && slot._day != DateTime.Today)
				{
					PXDatabase.ResetSlot<TStatistics>(key, tables);
					slot = GetSlot<TStatistics>(key, graph, tables);
				}
				return slot;
			}

			private static ActivityStatistics GetSlot<TStatistics>(string key, PXGraph graph, params Type[] tables)
				where TStatistics : class, IPrefetchable<PXGraph>, new()
			{
				return PXDatabase.GetSlot<TStatistics, PXGraph>(key, graph, tables) as ActivityStatistics;
			}
		}

		#endregion

		#region EPTasksStatistics

		private sealed class EPTasksStatistics : ActivityStatistics
		{
			public static readonly EPTasksStatistics Empty = new EPTasksStatistics();

			public override void Prefetch(PXGraph graph)
			{
				base.Prefetch(graph);

				_open = new SelectOpenEPGenericActivity<TaskClassId>(graph).Count();
				_today = new SelectOpenAndOverdueEPGenericActivity<TaskClassId, Tomorrow>(graph).Count();
				_new = new SelectNewAndOverdueNewEPGenericActivity<TaskClassId>(graph).Count();
			}

			public static ActivityStatistics GetFromSlot(string key, PXGraph graph)
			{
				return GetFromSlot<EPTasksStatistics>(key, graph, typeof(EPActivity), typeof(EPAttendee), typeof(EPView), typeof(UserPreferences));
			}
		}

		#endregion

		#region EPEventsStatistics

		private sealed class EPEventsStatistics : ActivityStatistics
		{
			public static readonly EPEventsStatistics Empty = new EPEventsStatistics();

			public override void Prefetch(PXGraph graph)
			{
				base.Prefetch(graph);

				_open = new SelectOpenEPGenericActivity<EventClassId>(graph).Count();
				_today = new SelectTodayEPGenericActivity<EventClassId>(graph).Count();
				_new = new SelectNewEPGenericActivity<EventClassId>(graph).Count();
			}

			public static ActivityStatistics GetFromSlot(string key, PXGraph graph)
			{
				return GetFromSlot<EPEventsStatistics>(key, graph, typeof(EPActivity), typeof(EPAttendee), typeof(EPView), typeof(UserPreferences));
			}
		}

		#endregion

		#region EPActivityReminder

		private sealed class EPActivityReminder : IPrefetchable<PXGraph>, IExpires
		{
			#region IExpires
			private bool _dbChanged;
			public bool DBChanged 
			{ 
				get 
				{
					if (DateTime.UtcNow >= ExpirationTime)
						_dbChanged = true;
					return _dbChanged; 
				}
				set { _dbChanged = value; }
			}

			public DateTime ExpirationTime { get; set; }
			#endregion

			public static readonly EPActivityReminder Empty = new EPActivityReminder();

			PXResult<EPActivity>[] _reminderList;
			public PXResult<EPActivity>[] ReminderList
			{
				get { return _reminderList; }
			}

			public EPActivityReminder()
			{
				ExpirationTime = DateTime.UtcNow.AddMinutes(1);
			}

			public void Prefetch(PXGraph parameter)
			{
				var reminderListViewInfo = new PXSelectReadonly2<EPActivity,
					LeftJoin<EPView, On<EPView.noteID, Equal<EPActivity.noteID>, And<EPView.userID, Equal<Current<AccessInfo.userID>>>>, ReminderJoin>,
					ReminderListWhere,
					OrderBy<Asc<EPActivity.reminderDate>>>(parameter);
				_reminderList = reminderListViewInfo.Select().ToArray();
			}

			public static EPActivityReminder GetFromSlot(string key, PXGraph graph)
			{
				return PXDatabase.GetSlot<EPActivityReminder, PXGraph>(key, graph, typeof(EPActivity), typeof(EPView), typeof(EPAttendee), typeof(UserPreferences));
			}
		}

		#endregion

		#region Wrapped Where

		public class WrappedWhere<TWhere> : IBqlWhere
			where TWhere : IBqlWhere, new()
		{
			private readonly TWhere _realWhere;

			public WrappedWhere()
			{
				_realWhere = new TWhere();
			}

			public virtual void Parse(PXGraph graph, List<IBqlParameter> pars, List<Type> tables, List<Type> fields, List<IBqlSortColumn> sortColumns, StringBuilder text, BqlCommand.Selection selection)
			{
				_realWhere.Parse(graph, pars, tables, fields, sortColumns, text, selection);
			}

			public virtual void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
			{
				_realWhere.Verify(cache, item, pars, ref result, ref value);
			}
		}

		#endregion

		#region DeferFilterInfo

		[Serializable]
		public partial class DeferFilterInfo : IBqlTable
		{
			#region Type

			public abstract class type : IBqlField
			{
			}

			[PXInt]
			[PXDefault(5)]
			[PXIntList(new [] { 5, 10, 15, 30, 60, 120, 240, 720, 1440 },
				new [] { Messages.Min5, Messages.Min10, Messages.Min15, 
						Messages.Min30, Messages.Min60, Messages.Min120, 
						Messages.Min240, Messages.Min720, Messages.Min1440 })]
			public virtual Int32? Type { get; set; }

			#endregion
		}

		#endregion

		#endregion

		#region Constants

		private const string _REMINDER_SLOT_KEY_PREFIX = "ReminderStatistics@";
		private const string _TASKS_SLOT_KEY_PREFIX = "TasksStatistics@";
		private const string _EVENTS_SLOT_KEY_PREFIX = "EventsStatistics@";
		private const string _REMINDERLIST_SLOT_KEY_PREFIX = "ReminderList@";

		#endregion

		#region Selects

		[PXHidden]
		public PXFilter<DeferFilterInfo>
			DeferFilter;

		public PXSelectReadonly<EPActivity>
			ReminderListCurrent;

		public PXSelectReadonly2<EPActivity,
			ReminderJoin,
			ReminderListWhere,
			OrderBy<Asc<EPActivity.reminderDate>>>
			ReminderList;

		public PXSelectJoin<EPReminder,
			InnerJoin<EPActivity, On<EPActivity.taskID, Equal<Required<EPActivity.taskID>>>>,
			Where<EPReminder.noteID, Equal<EPActivity.noteID>,
				And<EPReminder.userID, Equal<Current<AccessInfo.userID>>>>>
			RemindInfo;

		public PXSelectReadonly<EPActivity,
			ActivityListWhere,
			OrderBy<Asc<EPActivity.startDate>>>
			ActivityList;

		public PXSelectGroupBy<EPActivity,
			ActivityListWhere,
			Aggregate<Count>>
			ActivityCount;

		[PXHidden]
		public PXSetup<EPSetup>
			Setup;
		
		[PXDBInt()]
		protected virtual void EPActivity_ProjectID_CacheAttached(PXCache sender)
		{ 
		}

		[PXDBInt()]
		protected virtual void EPActivity_ProjectTaskID_CacheAttached(PXCache sender)
		{
		}
		
		#endregion

		#region Data Handlers

		public virtual IEnumerable reminderListCurrent([PXInt] int? taskID)
		{
			if (taskID == null)
			{
				if (ReminderList.Current == null) yield break;
				taskID = ReminderList.Current.TaskID;
			}

			yield return (EPActivity)PXSelectReadonly<EPActivity>.Search<EPActivity.taskID>(this, taskID);
		}

		public virtual IEnumerable reminderList()
		{
			return ActivityReminder.ReminderList;
		}

		#endregion

		#region Actions

		public PXAction<EPActivity> navigate;
		[PXButton]
		public virtual IEnumerable Navigate(PXAdapter adapter)
		{
			var current = ReminderList.Current;
			NavigateToItem(current);
			return adapter.Get();
		}

		public virtual void NavigateToItem(EPActivity current)
		{
			if (current != null)
			{
				var graphType = EPActivityPrimaryGraphAttribute.GetGraphType(current);
				if (!PXAccess.VerifyRights(graphType))
				{
					ReminderList.Ask(CR.Messages.AccessDenied, CR.Messages.FormNoAccessRightsMessage(graphType), MessageButtons.OK, MessageIcon.Error);
				}
				else
				{
					var graph = (PXGraph)PXGraph.CreateInstance(graphType);
					var cache = graph.Caches[current.GetType()];
					var searchView = new PXView(
						graph,
						false,
						BqlCommand.CreateInstance(typeof(Select<>), cache.GetItemType()));
					var startRow = 0;
					var totalRows = 0;
					var acts = searchView.
						Select(null, null,
						       new object[] { current.TaskID },
						       new string[] { typeof(EPActivity.taskID).Name },
						       null, null, ref startRow, 1, ref totalRows);

					if (acts != null && acts.Count > 0)
					{
						var act = acts[0];
						cache.Current = act;
						PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
					}
				}
			}
		}

		public PXAction<EPActivity> viewActivity;
		[PXButton]
		[PXUIField(DisplayName = Messages.ViewDetails)]
		public virtual IEnumerable ViewActivity(PXAdapter adapter)
		{
			if (ReminderList.Current != null)
			{
				PXRedirectHelper.TryOpenPopup(ReminderList.Cache, ReminderList.Current, "Open");
			}
			return adapter.Get();
		}

		public PXAction<EPActivity> dismiss;
		[PXButton]
		public virtual IEnumerable Dismiss(PXAdapter adapter)
		{
			var id = ExtractActivityID(adapter);
			if (id != null) UpdateAcitivtyRemindInfo(id, reminder => reminder.Dismiss = true);
			return adapter.Get();
		}

		public PXAction<EPActivity> dismissAll;
		[PXButton]
		[PXUIField(DisplayName = Messages.DismissAll)]
		public virtual IEnumerable DismissAll(PXAdapter adapter)
		{
			foreach (EPActivity row in ReminderList.Select())
				UpdateAcitivtyRemindInfo(row.TaskID, reminder => reminder.Dismiss = true);
			return adapter.Get();
		}

		public PXAction<EPActivity> dismissCurrent;
		[PXButton]
		[PXUIField(DisplayName = Messages.Dismiss)]
		public virtual IEnumerable DismissCurrent(PXAdapter adapter)
		{
			if (ReminderList.Current != null)
				UpdateAcitivtyRemindInfo(ReminderList.Current.TaskID, reminder => reminder.Dismiss = true);
			return adapter.Get();
		}

		public PXAction<EPActivity> defer;
		[PXButton]
		public virtual IEnumerable Defer(PXAdapter adapter)
		{
			var id = ExtractActivityID(adapter);
			if (id != null)
				UpdateAcitivtyRemindInfo(id,
					reminder =>
					{
						reminder.Date = PXTimeZoneInfo.Now.AddMinutes(Convert.ToInt32(adapter.Parameters[1]));
					});
			return adapter.Get();
		}

		public PXAction<EPActivity> deferCurrent;
		[PXButton]
		[PXUIField(DisplayName = Messages.Snooze)]
		public virtual IEnumerable DeferCurrent(PXAdapter adapter)
		{
			if (ReminderList.Current != null)
			{
				var minutes = DeferFilter.Current.Type == null ? 5 : (int)DeferFilter.Current.Type;
				UpdateAcitivtyRemindInfo(ReminderList.Current.TaskID,
					reminder =>
					{
						reminder.Date = PXTimeZoneInfo.Now.AddMinutes(minutes);
					});
			}
			return adapter.Get();
		}

		public PXAction<EPActivity> completeRow;
		[PXButton(Tooltip = Messages.CompleteTooltipS)]
		[PXUIField(DisplayName = Messages.Complete)]
		public virtual IEnumerable CompleteRow(PXAdapter adapter)
		{
			var activity = ReadActivity(ExtractActivityID(adapter));
			var graphType = activity.With(_ => _.ClassID).With(_ => EPActivityPrimaryGraphAttribute.GetGraphType(_.Value));
			if (graphType != null)
			{
				var graph = Activator.CreateInstance(graphType) as IActivityMaint;
				graph.CompleteRow(activity);
			}
			adapter.Parameters = new object[0];
			return adapter.Get();
		}

		public PXAction<EPActivity> cancelRow;
		[PXButton(Tooltip = Messages.CancelTooltipS)]
		[PXUIField(DisplayName = Messages.Cancel)]
		public virtual IEnumerable CancelRow(PXAdapter adapter)
		{
			var activity = ReadActivity(ExtractActivityID(adapter));
			var graphType = activity.With(_ => _.ClassID).With(_ => EPActivityPrimaryGraphAttribute.GetGraphType(_.Value));
			if (graphType != null)
			{
				var graph = Activator.CreateInstance(graphType) as IActivityMaint;
				graph.CancelRow(activity);
			}
			adapter.Parameters = new object[0];
			return adapter.Get();
		}

		public PXAction<EPActivity> openInquiry;
		[PXButton(Tooltip = Messages.CancelTooltipS)]
		[PXUIField(DisplayName = Messages.Cancel)]
		public virtual IEnumerable OpenInquiry(PXAdapter adapter)
		{
			if (adapter.Parameters.Length > 0 && adapter.Parameters[0] != null)
			{
				var refNoteID = (long)adapter.Parameters[0];
				OpenInquiryScreen(refNoteID);
			}
			return adapter.Get();
		}

		public virtual void OpenInquiryScreen(long refNoteID)
		{
			if (refNoteID >= 0)
			{
				var gr = PXGraph.CreateInstance<ActivitiesMaint>();
				gr.Filter.Current.NoteID = refNoteID;
				PXRedirectHelper.TryRedirect(gr, PXRedirectHelper.WindowMode.NewWindow);
			}
		}

		#endregion

		#region Public Methods

		public virtual int GetListCount()
		{
			return (int)(new SelectRemindEPGenericActivityCount(this)).Select().RowCount;
		}

		public virtual int GetOpenTasksCount()
		{
			return TasksStatistics.Open;
		}

		public virtual int GetTodayTasksCount()
		{
			return TasksStatistics.Today;
		}

		public virtual int GetNewTasksCount()
		{
			return TasksStatistics.New;
		}

		public virtual int GetOpenEventsCount()
		{
			return EventsStatistics.Open;
		}

		public virtual int GetTodayEventsCount()
		{
			return EventsStatistics.Today;
		}

		public virtual int GetNewEventsCount()
		{
			return EventsStatistics.New;
		}

        public virtual long GetTasksDefaultFilterID()
        {
            var epSetup = SetupCurrent;
            return (epSetup == null || epSetup.DefTasksFilterID == null) ? -1 : epSetup.DefTasksFilterID.Value;
        }

        public virtual long GetEventsDefaultFilterID()
        {
            var epSetup = SetupCurrent;
            return (epSetup == null || epSetup.DefEventsFilterID == null) ? -1 : epSetup.DefEventsFilterID.Value;
        }

		#endregion

		#region Private Methods

		private EPSetup SetupCurrent
		{
			get
			{
				try
				{
					return Setup.Current;
				}
				catch (OutOfMemoryException) { }
				catch (OverflowException) { }
				catch (PXSetPropertyException) { }
				return null;
			}
		}

		private delegate void UpdateRemindInfo(EPReminder info);

		private void UpdateAcitivtyRemindInfo(object id, UpdateRemindInfo handler)
		{
			EPReminder remindInfo = RemindInfo.Select(id);
			if (remindInfo == null)
			{
				remindInfo = (EPReminder)RemindInfo.Cache.Insert();
				EPActivity activity = PXSelect<EPActivity, Where<EPActivity.taskID, Equal<Required<EPActivity.taskID>>>>.
					Select(this, id);
				remindInfo.NoteID = PXNoteAttribute.GetNoteID(Caches[typeof(EPActivity)], activity,
															   EntityHelper.GetNoteField(typeof(EPActivity)));
				remindInfo.UserID = PXAccess.GetUserID();
				remindInfo.Date = PXTimeZoneInfo.Now;
				RemindInfo.Cache.Normalize();
			}
			handler(remindInfo);
			RemindInfo.Update(remindInfo);
			using (var ts = new PXTransactionScope())
			{
				RemindInfo.Cache.Persist(PXDBOperation.Insert);
				RemindInfo.Cache.Persist(PXDBOperation.Update);
				ts.Complete(this);
			}
			RemindInfo.Cache.Persisted(false);
			ActivityList.Cache.Clear();
			ActivityList.View.Clear();
			ActivityCount.Cache.Clear();
			ActivityCount.View.Clear();
			ReminderList.Cache.Clear();
			ReminderList.View.Clear();
			ReminderListCurrent.View.Clear();
		}

		private EPActivity ReadActivity(object taskId)
		{
			if (taskId == null) return null;
			return (EPActivity)PXSelect<EPActivity,
				Where<EPActivity.taskID, Equal<Required<EPActivity.taskID>>>>.
				Select(this, taskId);
		}

		private static object ExtractActivityID(PXAdapter adapter)
		{
			if (adapter == null || adapter.Parameters == null ||
				adapter.Parameters.Length < 1)
			{
				return null;
			}

			var data = adapter.Parameters[0];
			var strData = data as string[];
			if (strData != null && strData.Length > 0) 
				return strData[0];

			return data;
		}

		private ActivityStatistics TasksStatistics
		{
			get { return EPTasksStatistics.GetFromSlot(_TASKS_SLOT_KEY_PREFIX + PXAccess.GetUserID(), this) ?? EPTasksStatistics.Empty; }
		}

		private ActivityStatistics EventsStatistics
		{
			get { return EPEventsStatistics.GetFromSlot(_EVENTS_SLOT_KEY_PREFIX + PXAccess.GetUserID(), this) ?? EPEventsStatistics.Empty; }
		}

		private EPActivityReminder ActivityReminder
		{
			get { return EPActivityReminder.GetFromSlot(_REMINDERLIST_SLOT_KEY_PREFIX + PXAccess.GetUserID(), this) ?? EPActivityReminder.Empty; }
		}

		#endregion
	}
}
