using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Common.Mail;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.EP
{
	public class ActivityService : IActivityService
	{
		private PXView _view;
		private PXView _viewCount;

		public IEnumerable Select(object refNoteID, int? filterId = null)
		{
			View.Clear();
			int startRow = 0;
			int totalRows = 50;
			var filters = ReadFilters(filterId);
			var list = View.Select(null, new object[] { refNoteID }, null, new [] { typeof(EPActivity.taskID).Name }, new [] { true }, filters, ref startRow, 0, ref totalRows);
			return list.Select(row => row is PXResult ? ((PXResult)row)[0] : row);
		}

		private PXFilterRow[] ReadFilters(int? filterId)
		{
			if (filterId == null) return null;

			var filterCache = View.Graph.Caches[typeof(FilterRow)];
			var res = new List<PXFilterRow>();
			foreach (FilterRow row in PXSelect<FilterRow, 
				Where<FilterRow.filterID, Equal<Required<FilterRow.filterID>>>>.
				Select(View.Graph, filterId))
			{
				var item = new PXFilterRow(row.DataField, 
					(PXCondition)row.Condition,
					filterCache.GetValueExt<FilterRow.valueSt>(row.ValueSt), 
					filterCache.GetValueExt<FilterRow.valueSt2>(row.ValueSt2))
					{
						OpenBrackets = row.OpenBrackets ?? 0,
						CloseBrackets = row.CloseBrackets ?? 0,
						OrOperator = row.Operator == 1
					};
				res.Add(item);
			}
			return res.ToArray();
		}

		public virtual string GetKeys(object item)
		{
			var cache = View.Cache;
			var sb = new StringBuilder();
			var notFirst = false;
			foreach (string key in cache.Keys)
			{
				if (notFirst) sb.Append(',');
				sb.Append(cache.GetValue(item, key));
				notFirst = true;
			}
			return sb.ToString();
		}

		public virtual bool ShowTime(object item)
		{
			var act = (EPActivity)item;
			return Array.IndexOf(ActivitiesWithTime, act.ClassID) > -1;
		}

		public virtual DateTime? GetStartDate(object item)
		{
			var act = (EPActivity)item;
			return act.StartDate;
		}

		public virtual DateTime? GetEndDate(object item)
		{
			var act = (EPActivity)item;
			return act.EndDate;
		}

		public virtual void Cancel(string keys)
		{
			var act = SearchItem(keys) as EPActivity;
			if (act == null) return;

			var graphType = act.With(_ => _.ClassID).With(_ => EPActivityPrimaryGraphAttribute.GetGraphType(_.Value));
			if (graphType != null)
			{
				var graph = Activator.CreateInstance(graphType) as IActivityMaint;
				if (graph != null) graph.CancelRow(act);
			}
		}

		public virtual void Complete(string keys)
		{
			var act = SearchItem(keys) as EPActivity;
			if (act == null) return;

			var graphType = act.With(_ => _.ClassID).With(_ => EPActivityPrimaryGraphAttribute.GetGraphType(_.Value));
			if (graphType != null)
			{
				var graph = Activator.CreateInstance(graphType) as IActivityMaint;
				if (graph != null) graph.CompleteRow(act);
			}
		}

		public virtual void Defer(string keys, int minuts)
		{
		}

		public virtual void Dismiss(string keys)
		{
		}

		public virtual void Open(string keys)
		{
			var act = SearchItem(keys) as EPActivity;
			((TasksAndEventsReminder)Graph).NavigateToItem(act);
		}

		public virtual bool IsViewed(object item)
		{
			return true;
		}

		public virtual string GetImage(object item)
		{
			var act = (EPActivity)item;
			return act.ClassIcon;
		}

		public virtual string GetTitle(object item)
		{
			var act = (EPActivity)item;
			return act.Subject;
		}

		public int GetCount(object refNoteID)
		{
			ViewCount.Clear();
			return (int)((PXResult)ViewCount.SelectSingle(refNoteID)).RowCount;
		}

		protected virtual object SearchItem(string keys)
		{
			return (EPActivity)PXSelect<EPActivity>.
				Search<EPActivity.taskID>(View.Graph, keys);
		}

		protected PXGraph Graph
		{
			get
			{
				return View.Graph;
			}
		}

		private PXView View
		{
			get
			{
				if (_view == null || _viewCount == null)
				{
					CreateView();
					if (_view == null || _viewCount == null) throw new ArgumentNullException("Method CreateView returns null.");
				}
				return _view;
			}
		}

		private PXView ViewCount
		{
			get
			{
				if (_view == null || _viewCount == null)
				{
					CreateView();
					if (_view == null || _viewCount == null) throw new ArgumentNullException("Method CreateView returns null.");
				}
				return _viewCount;
			}
		}

		protected virtual void CreateView()
		{
			var graph = PXGraph.CreateInstance<TasksAndEventsReminder>();
			_view = graph.ActivityList.View;
			_viewCount = graph.ActivityCount.View;
		}

		public virtual void ShowAll(object refNoteID)
		{
			if (refNoteID is long)
				((TasksAndEventsReminder)Graph).OpenInquiryScreen((long)refNoteID);
		}

		protected virtual int[] ActivitiesWithTime
		{
			get
			{
				return new[] 
				{ 
					CRActivityClass.Event, CRActivityClass.Activity, CRActivityClass.Email 
				};
			}
		}

		public virtual IEnumerable<Data.EP.ActivityService.IActivityType> GetActivityTypes()
		{			
			List<EPActivityType> activityTypes = PXDatabase.GetSlot<List<EPActivityType>>(typeof(EPActivityType).Name, typeof(EPActivityType));
			if (activityTypes.Count == 0)
			{
				PXSelect<EPActivityType, Where<EPActivityType.active, Equal<True>, And<EPActivityType.isInternal, Equal<True>>>>.Clear(Graph);
				activityTypes.AddRange(PXSelect<EPActivityType, Where<EPActivityType.active, Equal<True>, And<EPActivityType.isInternal, Equal<True>>>>.Select(Graph).RowCast<EPActivityType>());				
			}
			return activityTypes;			
		}

		// TODO: Need to remove all CreateActyvity* methods
		public virtual void CreateTask(object refNoteID)
		{
			CreateTask(refNoteID, null);
		}

		public virtual void CreateTask(object refNoteID, Action<object> initializeHandler)
		{
			CreateActivity(CRActivityClass.Task, (long?)refNoteID, null, initializeHandler);
		}

		public virtual void CreateEvent(object refNoteID)
		{
			CreateEvent(refNoteID, null);
		}

		public virtual void CreateEvent(object refNoteID, Action<object> initializeHandler)
		{
			CreateActivity(CRActivityClass.Event, (long?)refNoteID, null, initializeHandler);
		}

		public virtual void CreateActivity(object refNoteID, string typeCode)
		{
			CreateActivity(refNoteID, typeCode, null);
		}

		public virtual void CreateActivity(object refNoteID, string typeCode, Action<object> initializeHandler)
		{
			CreateActivity(CRActivityClass.Activity, (long?)refNoteID, typeCode, initializeHandler);
		}

		public void OpenMailPopup(string link)
		{
			PXGraph graph = new CREmailActivityMaint();
			var activityCache = graph.Caches[typeof(EPActivity)];
			var newEmail = (EPActivity)activityCache.CreateInstance();
			FillMailAccount(newEmail);
			newEmail.Type = null;
			newEmail.IsIncome = false;
			var body = link;
			newEmail.Body = string.IsNullOrEmpty(body) ? link + newEmail.Body : body;
			activityCache.Insert(newEmail);
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
		}

		public void CreateEmailActivity(object refNoteID, int EmailAccountID)
		{
			CreateEmailActivity(refNoteID, EmailAccountID, null);
		}

		public void CreateEmailActivity(object refNoteID, int EmailAccountID, Action<object> initializeHandler)
		{
			var graphType = EPActivityPrimaryGraphAttribute.GetGraphType(CRActivityClass.Email);
			if (!PXAccess.VerifyRights(graphType))
				throw new AccessViolationException(CR.Messages.FormNoAccessRightsMessage(graphType));

			var cache = CreateInstanceCache<EPActivity>(graphType);
			if (cache != null)
			{
				var graph = cache.Graph;
				
				var newEmail = (EPActivity)cache.CreateInstance();
				newEmail.Type = null;
				newEmail.IsIncome = false;
				if (EmailAccountID != 0) 
					FillMailAccount(newEmail, EmailAccountID);
				else
					FillMailAccount(newEmail);

				FillMailCC(graph, newEmail, (long?)refNoteID);
				newEmail.RefNoteID = (long?)refNoteID;
				newEmail.Body = GenerateMailBody(graph);				

				if (initializeHandler != null)
					initializeHandler(newEmail);
				cache.Insert(newEmail);

				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
			}
		}

		private void CreateActivity(int classId, long? refNoteID, string typeCode, Action<object> initializeHandler)
		{
			var graphType = EPActivityPrimaryGraphAttribute.GetGraphType(classId);

			if (!PXAccess.VerifyRights(graphType))
			{
				throw new AccessViolationException(CR.Messages.FormNoAccessRightsMessage(graphType));
			}

			var cache = CreateInstanceCache<EPActivity>(graphType);
			if (cache != null)
			{
				var owner = EmployeeMaint.GetCurrentEmployeeID(cache.Graph);

				var activity = (EPActivity)cache.CreateCopy(cache.Insert());					
				activity.ClassID = classId;
				activity.RefNoteID = refNoteID;
				if (!string.IsNullOrEmpty(typeCode))
					activity.Type = typeCode;
				activity.Owner = owner;

				if (initializeHandler != null)
					initializeHandler(activity);
				cache.Update(activity);
				PXRedirectHelper.TryRedirect(cache.Graph, PXRedirectHelper.WindowMode.NewWindow);
			}
		}

		protected static string GenerateMailBody(PXGraph graph)
		{
			string res = null;
			var signature = ((UserPreferences)PXSelect<UserPreferences>.
				Search<UserPreferences.userID>(graph, PXAccess.GetUserID())).
				With(pref => pref.MailSignature);
			if (signature != null && (signature = signature.Trim()) != string.Empty)
				res += "<br />" + signature;
			return res;
		}

		private static void FillMailAccount(EPActivity message)
		{
			message.MailAccountID = MailAccountManager.DefaultMailAccountID;
		}

		private static void FillMailAccount(EPActivity message,int MailAccountID)
		{
			message.MailAccountID = MailAccountID;
		}

		private static void FillMailCC(PXGraph graph, EPActivity message, long? refNoteId)
		{
			if (refNoteId != null && refNoteId >= 0)
				foreach (Mailbox email in CRRelationsList<CRRelation.refNoteID>.GetEmailsForCC(graph, (long)refNoteId))
					if (email != null)
						message.MailCc += email.ToString() + "; ";
		}

		private PXCache CreateInstanceCache<TNode>(Type graphType)
			where TNode : IBqlTable
		{
			if (graphType != null)
			{
				var graph = PXGraph.CreateInstance(graphType);
				graph.Clear();
				foreach (Type type in graph.Views.Caches)
				{
					var cache = graph.Caches[type];
					if (typeof(TNode).IsAssignableFrom(cache.GetItemType()))
						return cache;
				}
			}
			return null;
		}
	}
}
