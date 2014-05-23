using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using PX.Common.Mail;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR.MassProcess;
using PX.Objects.EP;
using PX.Objects.PM;
using PX.SM;
using PX.TM;
using PX.Objects.CS;
using PX.Common;
using ActivityService = PX.Data.EP.ActivityService;

namespace PX.Objects.CR
{
	#region ActivityContactFilter

	[Serializable]
	public partial class ActivityContactFilter : IBqlTable
	{
		#region ContactID

		public abstract class contactID : IBqlField { }
		
		[PXInt]
		[PXUIField(DisplayName = "Select Contact")]
		[PXSelector(typeof(Search<Contact.contactID,
			Where<Contact.bAccountID, Equal<Current<BAccount.bAccountID>>,
			And<Contact.contactType, NotEqual<ContactTypesAttribute.bAccountProperty>>>>),
			DescriptionField = typeof(Contact.displayName),
			Filterable = true)]
		public virtual Int32? ContactID { get; set; }

		#endregion

		#region NoteID

		public abstract class noteID : IBqlField { }

		[PXLong]
		public virtual Int64? NoteID { get; set; }

		#endregion
	}

	#endregion

	#region CRActivityList

	public abstract class CRActivityListBaseAttribute : PXViewExtensionAttribute
	{
		private readonly BqlCommand _command;

		private PXView _view;

		private string _hostViewName;
		private PXSelectBase _sel;

		protected CRActivityListBaseAttribute() { }

		protected CRActivityListBaseAttribute(Type @select)
		{
			if (@select == null) throw new ArgumentNullException("select");

			if (typeof(IBqlSelect).IsAssignableFrom(@select))
			{
				_command = BqlCommand.CreateInstance(@select);
			}
			else
			{
				var error = string.Format("Incorrect select expression '{0}'", @select.Name);
				throw new PXArgumentException(error, "@select");
			}
		}

		public override void ViewCreated(PXGraph graph, string viewName)
		{
			Initialize(graph, viewName);

			var selectorDac = GraphSelector.Cache.GetItemType();
			var commandDac = GetCommandDac();
			AttachHandlers(graph, selectorDac, commandDac);
		}

		private Type GetCommandDac()
		{
			var commandTables = _command.With(_ => _.GetTables());
			var commandDac = (commandTables != null && commandTables.Length > 0) ? commandTables[0] : null;
			return commandDac;
		}

		private void Initialize(PXGraph graph, string viewName)
		{
			_hostViewName = viewName;
			_sel = GetSelectView(graph);

			if (_command != null)
				_view = new PXView(graph, true, _command);
		}

		protected PXSelectBase GraphSelector
		{
			get { return _sel; }
		}

		protected abstract void AttachHandlers(PXGraph graph, Type selectorDAC, Type commnadDAC);

		protected static void CorrectView(PXSelectBase sel, BqlCommand command)
		{
			var graph = sel.View.Graph;
			var newView = new PXView(graph, sel.View.IsReadOnly, command);
			var oldView = sel.View;
			sel.View = newView;
			string viewName;
			if (graph.ViewNames.TryGetValue(oldView, out viewName))
			{
				graph.Views[viewName] = newView;
				graph.ViewNames.Remove(oldView);
				graph.ViewNames[newView] = viewName;
			}
		}

		protected object SelectRecord()
		{
			if (_view == null) 
				throw new InvalidOperationException("Select command is not specified");

			var dataRecord = _view.SelectSingle();
			if (dataRecord == null) return null;

			var res = dataRecord as PXResult;
			if (res == null) return dataRecord;

			return res[0];
		}

		private PXSelectBase GetSelectView(PXGraph graph)
		{
			var selectView = graph.GetType().GetField(_hostViewName).GetValue(graph);
			var selectViewType = selectView.GetType();
			Type typeDefinition = selectViewType;
			if (selectViewType.IsGenericType)
				typeDefinition = selectViewType.GetGenericTypeDefinition();
			if (!typeof(CRActivityList<>).IsAssignableFrom(typeDefinition))
			{
				var attributeTypeName = GetType().Name;
				var error = string.Format("Attribute '{0}' can only be used on '{1}' view or its childs.", 
					attributeTypeName, selectViewType.Name);
				throw new PXArgumentException(error);
			}
			return (PXSelectBase)selectView;
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class CRBAccountReferenceAttribute : PXViewExtensionAttribute
	{
		private readonly BqlCommand _command;
		private PXView _bAccountView;

		private string RefFieldName
		{
			get { return RefField != null ? RefField.Name : EntityHelper.GetNoteField(_bAccountView.Cache.GetItemType()); }
		}

		public Type RefField { get; set; }

		public bool Persistent { get; set; }

		public CRBAccountReferenceAttribute(Type sel)
		{
			Persistent = false;
			if (sel == null) throw new ArgumentNullException("select");

			if (typeof(IBqlSelect).IsAssignableFrom(sel))
			{
				_command = BqlCommand.CreateInstance(sel);
			}
			else
			{
				string error = string.Format("Incorrect select expression '{0}'", sel.Name);
				throw new PXArgumentException(error, "sel");
			}
		}

		public override void ViewCreated(PXGraph graph, string viewName)
		{
			_bAccountView = new PXView(graph, true, _command);
			PXView _view = graph.Views[viewName];
			graph.FieldDefaulting.AddHandler(_view.Cache.GetItemType(), typeof(EPActivity.parentRefNoteID).Name, ParentRefNoteID_FieldDefaulting);

			if (Persistent)
			{
				graph.Views.Caches.Remove(_view.Cache.GetItemType());
				graph.Views.Caches.Add(_view.Cache.GetItemType());
				graph.RowPersisting.AddHandler(_view.Cache.GetItemType(), RowPersisting);
			}
		}

		private long? GetRefNoteID(PXCache sender)
		{
			object record = _bAccountView.SelectSingle();
			return GetRefNoteID(sender, record);
		}

		private long? GetRefNoteID(PXCache sender, object record)
		{
			return record != null ? (long?) sender.Graph.Caches[record.GetType()].GetValue(record, RefFieldName) : null;
		}

		private void ParentRefNoteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			object record = _bAccountView.SelectSingle();
			if(record != null)
				e.NewValue = GetRefNoteID(sender);
		}

		private void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			sender.SetValue(e.Row, typeof(EPActivity.parentRefNoteID).Name, GetRefNoteID(_bAccountView.Cache));
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class CRDefaultMailToAttribute : CRActivityListBaseAttribute
	{
		public interface IEmailMessageTarget
		{
			string DisplayName { get; }
			string Address { get; }
		}

		private bool _takeCurrent;

		public CRDefaultMailToAttribute()
			: base()
		{
			_takeCurrent = true;
		}

		public CRDefaultMailToAttribute(Type @select)
			: base(@select)
		{
		}

		protected override void AttachHandlers(PXGraph graph, Type selectorDAC, Type commnadDAC)
		{
			graph.RowInserting.AddHandler(selectorDAC, RowInserting);
		}

		private void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			var row = e.Row as EPActivity;
			if (row == null) return;

			string emailAddress = null;

			IEmailMessageTarget record;
			if (_takeCurrent)
			{
				var primaryDAC = ((IActivityList) GraphSelector).PrimaryViewType;
				var primaryCache = sender.Graph.Caches[primaryDAC];
				record = primaryCache.Current as IEmailMessageTarget;
			}
			else
			{
				record = SelectRecord() as IEmailMessageTarget;
			}

			if (record != null)
			{
				var displayName = record.DisplayName.With(_ => _.Trim());
				var address = record.Address.With(_ => _.Trim());
				if (!string.IsNullOrEmpty(address))
					emailAddress = string.IsNullOrEmpty(displayName) ? address : Mailbox.Create(displayName, address);
			}
			row.MailTo = emailAddress;
		}
	}
	
	public sealed class ProjectTaskActivities: CRActivityList<PMTask>
	{
		public ProjectTaskActivities(PXGraph graph)
			: base(graph)
		{
			_Graph.RowInserting.AddHandler<EPActivity>(RowInserting);
		}

		private void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			EPActivity row = (EPActivity) e.Row;
			if (row == null) return;

			row.ProjectID = ((PMTask) sender.Graph.Caches[typeof (PMTask)].Current).ProjectID;
			row.ProjectTaskID = ((PMTask)sender.Graph.Caches[typeof(PMTask)].Current).TaskID;
		}

		protected override void SetCommandCondition()
		{
			var command = ProjectTaskActivities.GenerateOriginalCommand();
			var where = BqlCommand.Compose(typeof(Where<,,>),typeof(EPActivity.projectTaskID),typeof(Equal<>), typeof(Current<>), typeof(PM.PMTask.taskID),
                 typeof(And<,>), typeof(EPActivity.isCorrected), typeof(Equal<>), typeof(False)
                );
			command = command.WhereAnd(where);
			View = new PXView(View.Graph, View.IsReadOnly, command);
		}
		protected override IEnumerable NewActivityByType(PXAdapter adapter, string type)
		{
			//DONE redesign by task #32833
			CreateActivity(CRActivityClass.Activity, type);
			return adapter.Get();
		}

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.MailSend)]
		[PXShortCut(true, false, false, 'A', 'M')]
		public override IEnumerable NewMailActivity(PXAdapter adapter)
		{
			//DONE redesign by task #32833
			CreateActivity(CRActivityClass.Email, null);
			return adapter.Get();
		}

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Task)]
		[PXShortCut(true, false, false, 'K', 'C')]
		public override IEnumerable NewTask(PXAdapter adapter)
		{
			//DONE redesign by task #32833
			CreateActivity(CRActivityClass.Task, null);
			return adapter.Get();
		}
	}
	
	public sealed class ProjectActivities : CRActivityList<PM.PMProject>
	{
		public ProjectActivities(PXGraph graph)
			: base(graph)
		{
			_Graph.RowInserting.AddHandler<EPActivity>(RowInserting);
			_Graph.RowSelected.AddHandler<PMProject>(RowSelected);
		}

		private void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			EPActivity row = (EPActivity)e.Row;
			if (row == null) return;

			row.TrackTime = true;
			row.ProjectID = ((PMProject)sender.Graph.Caches[typeof(PMProject)].Current).ContractID;
		}

		private void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PM.PMProject row = (PM.PMProject) e.Row;
			if (row == null || View == null || View.Cache == null)
				return;
			View.Cache.AllowInsert = row.IsActive == true;
		}

		protected override void SetCommandCondition()
		{
			var command = ProjectActivities.GenerateOriginalCommand();
			var where = BqlCommand.Compose(
                typeof(Where<,,>),typeof(EPActivity.projectID), typeof(Equal<>), typeof(Current<>), typeof(PM.PMProject.contractID),
                typeof(And<,>), typeof(EPActivity.isCorrected),typeof(Equal<>), typeof(False)
                );
			command = command.WhereAnd(where);
			View = new PXView(View.Graph, View.IsReadOnly, command);
		}
		protected override IEnumerable NewActivityByType(PXAdapter adapter, string type)
		{
			//DONE redesign by task #32833
			CreateActivity(CRActivityClass.Activity, type);
			return adapter.Get();
		}

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.MailSend)]
		[PXShortCut(true, false, false, 'A', 'M')]
		public override IEnumerable NewMailActivity(PXAdapter adapter)
		{
			//DONE redesign by task #32833
			CreateActivity(CRActivityClass.Email, null);
			return adapter.Get();
		}

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Task)]
		[PXShortCut(true, false, false, 'K', 'C')]
		public override IEnumerable NewTask(PXAdapter adapter)
		{
			//DONE redesign by task #32833
			CreateActivity(CRActivityClass.Task, null);
			return adapter.Get();
		}
		
	}

	public sealed class OpportunityActivities : CRActivityList<CROpportunity>
	{
		public OpportunityActivities(PXGraph graph)
			: base(graph)
		{			
		}
		
		
		protected override void SetCommandCondition()
		{
			var command = ProjectActivities.GenerateOriginalCommand();
			var where = BqlCommand.Compose(
								typeof(Where<,,>), typeof(EPActivity.isCorrected), typeof(Equal<>), typeof(False),
								typeof(And<>), 
								typeof(Where<,,>), typeof(EPActivity.refNoteID), typeof(Equal<>), typeof(Current<>), typeof(CROpportunity.noteID),
								typeof(Or<,>), typeof(EPActivity.refNoteID), typeof(Equal<>), typeof(Current<>), typeof(Contact.noteID)								
								);
			command = command.WhereAnd(where);
			View = new PXView(View.Graph, View.IsReadOnly, command);
		}	

	}

	public class CRActivityList<TPrimaryView> : CRActivityListBase<TPrimaryView, EPActivity>
		where TPrimaryView : class, IBqlTable
	{
		public CRActivityList(PXGraph graph) : base(graph)
		{
		}
	}

	public class CRActivityListReadonly<TPrimaryView> : CRActivityListBase<TPrimaryView, EPActivity>
			where TPrimaryView : class, IBqlTable
	{
		public CRActivityListReadonly(PXGraph graph)
			: base(graph)
		{
			var cache = _Graph.Caches[typeof(EPActivity)];
			PXUIFieldAttribute.SetEnabled(cache, null, typeof(EPActivity.subject).Name, false);
			PXUIFieldAttribute.SetEnabled(cache, null, typeof(EPActivity.priority).Name, false);
			PXUIFieldAttribute.SetEnabled(cache, null, typeof(EPActivity.uistatus).Name, false);
			PXUIFieldAttribute.SetEnabled(cache, null, typeof(EPActivity.startDate).Name, false);
			PXUIFieldAttribute.SetEnabled(cache, null, typeof(EPActivity.endDate).Name, false);
			PXUIFieldAttribute.SetEnabled(cache, null, typeof(EPActivity.noteID).Name, false);
			PXUIFieldAttribute.SetEnabled(cache, null, typeof(EPActivity.createdByID).Name, false);
			PXUIFieldAttribute.SetEnabled(cache, null, typeof(EPActivity.body).Name, false);
			PXUIFieldAttribute.SetEnabled(cache, null, typeof(EPActivity.categoryID).Name, false);

			PXUIFieldAttribute.SetVisible(cache, null, typeof(EPActivity.priority).Name, false);
			cache.AllowUpdate = false;
		}
	}
	public class CRChildActivityList<TParentActivity> : CRActivityListBase<TParentActivity, CRChildActivity>
		where TParentActivity : EPActivity
	{
		public CRChildActivityList(PXGraph graph) : base(graph)
		{
			_Graph.RowInserting.AddHandler<CRChildActivity>(RowInserting);
		}

		private void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			CRChildActivity row = (CRChildActivity)e.Row;
			if (row == null) return;

			row.ProjectID = ((EPActivity)sender.Graph.Caches[typeof(EPActivity)].Current).ProjectID;
			row.ProjectTaskID = ((EPActivity)sender.Graph.Caches[typeof(EPActivity)].Current).ProjectTaskID;
			row.ParentTaskID = ((EPActivity)sender.Graph.Caches[typeof(EPActivity)].Current).TaskID;
		}

		public IEnumerable SelectByParentID(object parentId)
		{
			return PXSelect<CRChildActivity,
				Where<CRChildActivity.parentTaskID, Equal<Required<CRChildActivity.parentTaskID>>>>.
				Select(_Graph, parentId).RowCast<CRChildActivity>();
		}

		protected override void ReadNoteIDFieldInfo(out string noteField, out Type noteBqlField)
		{
			noteField = typeof(EPActivity.refNoteID).Name;
			noteBqlField = _Graph.Caches[typeof(TParentActivity)].GetBqlField(noteField);
		}

		protected override void SetCommandCondition()
		{
			var newCmd = OriginalCommand.WhereAnd(
				BqlCommand.Compose(typeof(Where<,>),
					typeof(CRChildActivity.parentTaskID),
					typeof(Equal<>),
					typeof(Optional<>),
					typeof(TParentActivity).GetNestedType(typeof(EPActivity.taskID).Name)));
			View = new PXView(View.Graph, View.IsReadOnly, newCmd);
		}
	}

	public delegate long? CalculateAlternativeNoteIdHandler(object row);

	public interface IActivityList
	{
		bool CheckActivitesForDelete(params object[] currents);
		Type PrimaryViewType { get; }
	}

	public abstract class CRActivityListBase<TActivity> : PXSelectBase
		where TActivity : EPActivity, new()
	{
		protected internal const string _NEWTASK_COMMAND = "NewTask";
		protected internal const string _NEWEVENT_COMMAND = "NewEvent";
		protected internal const string _VIEWACTIVITY_COMMAND = "ViewActivity";
		protected internal const string _VIEWALLACTIVITIES_COMMAND = "ViewAllActivities";
		protected internal const string _NEWACTIVITY_COMMAND = "NewActivity";
		protected internal const string _NEWMAILACTIVITY_COMMAND = "NewMailActivity";
		protected internal const string _REGISTERACTIVITY_COMMAND = "RegisterActivity";
		protected internal const string _OPENACTIVITYOWNER_COMMAND = "OpenActivityOwner";

		public static BqlCommand GenerateOriginalCommand()
		{
			var classIdField = typeof(TActivity).GetNestedType(typeof(EPActivity.classID).Name);
			var createdDateTimeField = typeof(TActivity).GetNestedType(typeof(EPActivity.createdDateTime).Name);
			var id = typeof(TActivity).GetNestedType(typeof(EPActivity.taskID).Name);

			return BqlCommand.CreateInstance(
				typeof(Select<,,>), typeof(TActivity),			
				typeof(Where<,>),
					classIdField, typeof(GreaterEqual<>), typeof(Zero),
				typeof(OrderBy<>), typeof(Desc<,>), createdDateTimeField, typeof(Desc<>), id);
		}
	}

	[PXDynamicButton(new string[] { "NewTask", "NewEvent", "ViewActivity", "NewMailActivity", "RegisterActivity", "OpenActivityOwner", "ViewAllActivities", "NewActivity" },
					 new string[] { Messages.AddTask, Messages.AddEvent, Messages.Details, Messages.AddEmail, Messages.RegisterActivity, Messages.OpenActivityOwner, Messages.ViewAllActivities, Messages.AddActivity },
					 TranslationKeyType = typeof(Messages))]
	public class CRActivityListBase<TPrimaryView, TActivity> : CRActivityListBase<TActivity>, IActivityList
		where TPrimaryView : class, IBqlTable
		where TActivity : EPActivity, new()
	{
		#region Constants

		protected const string _PRIMARY_WORKGROUP_ID = "WorkgroupID";

		#endregion

		#region Fields

		public delegate Email GetEmailHandler();

		private int? _defaultEMailAccountId;

		private readonly BqlCommand _originalCommand;
		private readonly string _noteField;
		private readonly Type _noteBqlField;

		private bool _enshureTableData = true;

		#endregion

		#region Ctor

		public CRActivityListBase(PXGraph graph)
		{
			_Graph = graph;

			ReadNoteIDFieldInfo(out _noteField, out _noteBqlField);

			var cache = _Graph.Caches[typeof(TActivity)];

			graph.RowSelected.AddHandler(typeof(TPrimaryView), Table_RowSelected);
			if (typeof(EPActivity).IsAssignableFrom(typeof(TPrimaryView)))
				graph.RowPersisted.AddHandler<TPrimaryView>(Table_RowPersisted);
			graph.RowDeleting.AddHandler<TActivity>(Activity_RowDeleting);
			graph.RowDeleted.AddHandler<TActivity>(Activity_RowDeleted);
			graph.RowPersisting.AddHandler<TActivity>(Activity_RowPersisting);
			graph.RowSelected.AddHandler<TActivity>(Activity_RowSelected);

			graph.FieldDefaulting.AddHandler(typeof(TActivity), typeof(EPActivity.refNoteID).Name, Activity_RefNoteID_FieldDefaulting);
			//graph.FieldDefaulting.AddHandler(typeof(TActivity), typeof(EPActivity.parentRefNoteID).Name, Activity_ParentRefNoteID_FieldDefaulting);

			AddActions(graph);
			AddPreview(graph);

			PXUIFieldAttribute.SetVisible(cache, null, typeof(EPActivity.taskID).Name, false);

			_originalCommand = GenerateOriginalCommand();
			View = new PXView(graph, false, OriginalCommand);
			PXDBDefaultAttribute.SetSourceType<EPActivity.refNoteID>(cache, _noteBqlField);

			SetCommandCondition();
		}

		#endregion

		#region Implementation

		#region Preview

		private void AddPreview(PXGraph graph)
		{
			graph.Initialized += sender =>
				{
					string viewName;
					if (graph.ViewNames.TryGetValue(this.View, out viewName))
					{
						var att = new CRPreviewAttribute(typeof(TPrimaryView), typeof(TActivity));
						att.Attach(graph, viewName, null);
					}
				};
		}

		#endregion

		#region Add Actions
		protected void AddActions(PXGraph graph)
		{
			AddAction(graph, _NEWTASK_COMMAND, Messages.AddTask, NewTask);
			AddAction(graph, _NEWEVENT_COMMAND, Messages.AddEvent, NewEvent);
			AddAction(graph, _VIEWACTIVITY_COMMAND, Messages.Details, ViewActivity);
			AddAction(graph, _NEWMAILACTIVITY_COMMAND, Messages.AddEmail, NewMailActivity);
			AddAction(graph, _OPENACTIVITYOWNER_COMMAND, string.Empty, OpenActivityOwner);
			AddAction(graph, _VIEWALLACTIVITIES_COMMAND, Messages.ViewAllActivities, ViewAllActivities);

			AddActivityQuickActionsAsMenu(graph);
		}

		private void AddActivityQuickActionsAsMenu(PXGraph graph)
		{
			List<ActivityService.IActivityType> types = new List<ActivityService.IActivityType>(new EP.ActivityService().GetActivityTypes());

			PXAction btn = AddAction(graph, _NEWACTIVITY_COMMAND, 
			                    PXMessages.LocalizeNoPrefix(Messages.AddActivity),
			                    false,
			                    NewActivityByType);

			if (types.Count <= 0) return;

			btn.SetVisible(true);
			List<ButtonMenu> menuItems = new List<ButtonMenu>(types.Count);

			foreach (ActivityService.IActivityType type in types)
			{
				ButtonMenu menuItem = new ButtonMenu( type.Type, PXMessages.LocalizeFormatNoPrefix(Messages.AddTypedActivityFormat, type.Description), null);
				if (type.IsDefault == true)
					menuItems.Insert(0, menuItem);
				else
					menuItems.Add(menuItem);
			}
			btn.SetMenu(menuItems.ToArray());
		}

		internal void AddAction(PXGraph graph, string name, string displayName, PXButtonDelegate handler)
		{
			AddAction(graph, name, displayName, true, handler, null);
		}

		internal PXAction AddAction(PXGraph graph, string name, string displayName, bool visible, PXButtonDelegate handler, params PXEventSubscriberAttribute[] attrs)
		{
			var uiAtt = new PXUIFieldAttribute
							{
								DisplayName = PXMessages.LocalizeNoPrefix(displayName),
								MapEnableRights = PXCacheRights.Select
							};
			if (!visible) uiAtt.Visible = false;
			var addAttrs = new List<PXEventSubscriberAttribute> { uiAtt };
			if (attrs != null)
				foreach (PXEventSubscriberAttribute attr in attrs)
					if (attr != null)
						addAttrs.Add(attr);
			var res = (PXAction)Activator.CreateInstance(typeof(PXNamedAction<>).MakeGenericType(
				new Type[] { typeof(TPrimaryView) }), new object[] { graph, name, handler, addAttrs.ToArray() });
			graph.Actions[name] = res;
			return res;
		}
		#endregion

		protected PXCache CreateInstanceCache<TNode>(Type graphType)
			where TNode : IBqlTable
		{
			if (graphType != null)
			{
				if (EnshureTableData && _Graph.IsDirty)
				{
					PXAutomation.GetStep(_Graph,
						new object[] { _Graph.Views[_Graph.AutomationView].Cache.Current },
						BqlCommand.CreateInstance(typeof(Select<>), _Graph.Views[_Graph.AutomationView].Cache.GetItemType()));
					_Graph.Actions.PressSave();
				}

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

		public Type PrimaryViewType
		{
			get { return typeof (TPrimaryView); }
		}

		#region View Record

		#endregion

		#region Delete Record
		public virtual void DeleteActivites(params object[] currents)
		{
			if (!typeof(EPActivity).IsAssignableFrom(typeof(EPActivity))) return;

			foreach (var item in View.SelectMultiBound(currents))
				Cache.Delete(item is PXResult ? ((PXResult)item)[typeof(EPActivity)] : item);
		}

		public virtual bool CheckActivitesForDelete(params object[] currents)
		{
			return CheckActivitesForDeleteInternal("act_cannot_delete", Messages.ConfirmDeleteActivities, currents);
		}

		protected virtual bool CheckActivitesForDeleteInternal(string key, string msg, params object[] currents)
		{
			if (!typeof(EPActivity).IsAssignableFrom(typeof(EPActivity))) return true;

			foreach (object item in View.SelectMultiBound(currents))
			{
				EPActivity row = (EPActivity)(item is PXResult ? ((PXResult)item)[typeof(EPActivity)] : item);
				if (row.Billed == true || !string.IsNullOrEmpty(row.TimeCardCD) || row.MPStatus == MailStatusListAttribute.InProcess)
				{
					return View.Ask(key, msg, MessageButtons.YesNoCancel) == WebDialogResult.Yes;
				}
			}
			return true;
		}

		private void DeleteActivity(EPActivity current)
		{
			var graphType = EPActivityPrimaryGraphAttribute.GetGraphType(current);
			var cache = CreateInstanceCache<EPActivity>(graphType);
			if (cache != null)
			{
				if (!cache.AllowDelete)
					throw new PXException(ErrorMessages.CantDeleteRecord);

				var searchView = new PXView(
					cache.Graph,
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
					cache.Delete(act);
					cache.Graph.Actions.PressSave();
				}
			}
		}
		#endregion

		protected void CreateActivity(int classId, string type)
		{
			Type graphType = EPActivityPrimaryGraphAttribute.GetGraphType(classId);

			if (!PXAccess.VerifyRights(graphType))
			{
				_Graph.Views[_Graph.PrimaryView].Ask(null, Messages.AccessDenied, Messages.FormNoAccessRightsMessage(graphType), MessageButtons.OK, MessageIcon.Error);
			}
			else
			{
				PXCache cache = CreateInstanceCache<EPActivity>(graphType);
				if (cache != null)
				{
					EPActivity newact = (EPActivity)cache.CreateInstance();
					newact.ClassID = classId;
					newact.Type = type;
					newact = (EPActivity)cache.Insert(newact);
					TActivity activity = (TActivity)_Graph.Caches[typeof (TActivity)].CreateInstance();
					PXCache<EPActivity>.RestoreCopy(activity, newact);
					activity = ((PXCache<TActivity>)_Graph.Caches[typeof(TActivity)]).InitNewRow(activity);
					_Graph.Actions.PressSave();

					activity.ClassID = classId;
					_Graph.Caches[typeof(TActivity)].SetValueExt(activity, typeof(EPActivity.type).Name, !string.IsNullOrEmpty(type) ? type : activity.Type);

					//TODO: move to OwnerWorgroupDefaultingAttribute
					Guid? owner = EmployeeMaint.GetCurrentEmployeeID(_Graph);
					int? groupID = GetParrentGroup();
					activity.Owner = owner;
					if (PXOwnerSelectorAttribute.BelongsToWorkGroup(_Graph, groupID, activity.Owner))
						activity.GroupID = groupID;
					//TODO: move to EmailDefaultingAttribute
					if (activity.ClassID == CRActivityClass.Email)
					{
						activity.MailAccountID = DefaultEMailAccountId;
						FillMailReply(activity);
						FillMailTo(activity);
						if (activity.RefNoteID != null)
							FillMailCC(activity, (long)activity.RefNoteID);
						FillMailSubject(activity);
						activity.Body = GenerateMailBody();
					}
					//END TODO
					
					cache.Update(activity);
					PXRedirectHelper.TryRedirect(cache.Graph, PXRedirectHelper.WindowMode.NewWindow);
				}
			}
		}

		private int? GetParrentGroup()
		{
			PXCache cache = _Graph.Caches[typeof(TPrimaryView)];
			return (int?)cache.GetValue(cache.Current, _PRIMARY_WORKGROUP_ID);
		}

		protected EPActivity CurrentActivity
		{
			get
			{
				var tableCache = _Graph.Caches[typeof(TActivity)];
				return tableCache.With(_ => (TActivity)_.Current);
			}
		}
		
		public virtual void SendNotification(string source, string notificationCD, int? branchID, IDictionary<string, string> parameters)
		{
			Guid? SetupID = new NotificationUtility(_Graph).SearchSetupID(source, notificationCD);
			if (SetupID == null)
				throw new PXException(Messages.EmailNotificationSetupNotFound, notificationCD);

			PXCache sourceCache = _Graph.Caches[typeof(TPrimaryView)];

			if (sourceCache.Current == null)
				throw new PXException(Messages.EmailNotificationError);
			
			if (branchID == null)
				branchID = this._Graph.Accessinfo.BranchID;

			if (parameters == null)
			{
				parameters = new Dictionary<string, string>();
				foreach (string key in sourceCache.Keys)
				{
					object value = sourceCache.GetValueExt(sourceCache.Current, key);
					parameters[key] = value != null ? value.ToString() : null;
				}
			}

			Send((Guid)SetupID, branchID, parameters);
		}

		private void Send(Guid setupID, int? branchID, IDictionary<string, string> reportParams)
		{
			PXCache cache = _Graph.Caches[typeof(TPrimaryView)];
			TPrimaryView row = (TPrimaryView)cache.Current;

			TActivity activity = ((PXCache<TActivity>)_Graph.Caches[typeof(TActivity)]).InitNewRow();
			long? refNoteId = activity.RefNoteID;
			long? parentRefNoteId = activity.ParentRefNoteID;

			var sourceRow = (parentRefNoteId ?? refNoteId).With(_ => new EntityHelper(_Graph).GetEntityRow(_.Value));

			var utility = new NotificationUtility(_Graph);			

			NotificationSource source = utility.GetSource(sourceRow, setupID, branchID);
			if (source == null)
				throw new PXException(PX.SM.Messages.NotificationSourceNotFound);

			var accountId = source.EMailAccountID ?? DefaultEMailAccountId;
			if (accountId == null)
				throw new PXException(ErrorMessages.EmailNotConfigured);

			RecipientList recipients = utility.GetRecipients(sourceRow, source);

			var sent = false;
			if (source.ReportID != null)
			{
				var sender = new ReportNotificationGenerator(source.ReportID)
				{
					MailAccountId = accountId,
					Format = source.Format,
					AdditionalRecipents = recipients,
					Parameters = reportParams,
					NotificationID = source.NotificationID
				};
				sent |= sender.Send().Any();
			}
			else if (source.NotificationID != null)
			{
				var sender = TemplateNotificationGenerator.Create(row, (int)source.NotificationID);
				if (source.EMailAccountID != null)
				{
					sender.MailAccountId = accountId;
				}
				sender.BodyFormat = source.Format;
				sender.RefNoteID = refNoteId;
				sender.ParentRefNoteID = parentRefNoteId;
				sender.Watchers = recipients;
				sent |= sender.Send().Any();
			}

			if (!sent)
				throw new PXException(Messages.EmailNotificationError);
		}

		private static long? GetNoteId(PXGraph graph, object row)
		{
			if (row == null) return null;

			var rowType = row.GetType();
			var noteField = EntityHelper.GetNoteField(rowType);
			var cache = graph.Caches[rowType];
			return PXNoteAttribute.GetNoteID(cache, row, noteField);
		}

		#endregion

		#region Email Methods

		protected virtual void FillMailReply(EPActivity message)
		{
			PX.Common.Mail.Mailbox mailAddress = null;
			var isCorrect = message.MailReply != null &&
				PX.Common.Mail.Mailbox.TryParse(message.MailReply, out mailAddress) && 
				!string.IsNullOrEmpty(mailAddress.Address);
			if (isCorrect)
			{
				isCorrect = PXSelect<EMailAccount,
					Where<EMailAccount.address, Equal<Required<EMailAccount.address>>>>.
					Select(_Graph, mailAddress.Address).
					Count > 0;
			}

			var result = message.MailReply;
			if (!isCorrect)
				result = DefaultEMailAccountId.
					With(_ => (EMailAccount)PXSelect<EMailAccount,
						Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.
					Select(_Graph, _.Value)).
					With(_ => _.Address);
			if (string.IsNullOrEmpty(result))
			{
				var firstAcct = (EMailAccount)PXSelect<EMailAccount>.SelectWindowed(_Graph, 0, 1);
				if (firstAcct != null) result = firstAcct.Address;
			}
			message.MailReply = result;
		}
		
		protected virtual void FillMailTo(EPActivity message)
		{
			string customMailTo;
			if (GetNewEmailAddress != null && !string.IsNullOrEmpty(customMailTo = GetNewEmailAddress().ToString()))
				message.MailTo = customMailTo.With(_ => _.Trim());
		}

		protected virtual void FillMailCC(EPActivity message, long refNoteId)
		{
			if (refNoteId >= 0)
				foreach (Mailbox email in CRRelationsList<CRRelation.refNoteID>.GetEmailsForCC(_Graph, refNoteId))
					message.MailCc += email.ToString() + "; ";
		}

		protected virtual void FillMailSubject(EPActivity message)
		{
			if (!string.IsNullOrEmpty(DefaultSubject))
				message.Subject = DefaultSubject;
		}

		protected virtual string GenerateMailBody()
		{
			string res = null;
			var signature = ((UserPreferences)PXSelect<UserPreferences>.
				Search<UserPreferences.userID>(_Graph, PXAccess.GetUserID())).
				With(pref => pref.MailSignature);
			if (signature != null && (signature = signature.Trim()) != string.Empty)
				res += "<br />" + signature;
			return res;
		}

		#endregion

		#region Actions

		[PXButton]
		[PXUIField(Visible = false)]
		public virtual IEnumerable OpenActivityOwner(PXAdapter adapter)
		{
			var act = Cache.Current as EPActivity;
			if (act != null)
			{
				var empl = (EPEmployee)PXSelectReadonly<EPEmployee,
					Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.
					Select(_Graph, act.Owner);
				if (empl != null)
					PXRedirectHelper.TryRedirect(_Graph.Caches[typeof(EPEmployee)], empl, string.Empty, PXRedirectHelper.WindowMode.NewWindow);

				var usr = (Users)PXSelectReadonly<Users,
					Where<Users.pKID, Equal<Required<Users.pKID>>>>.
					Select(_Graph, act.Owner);
				if (usr != null)
					PXRedirectHelper.TryRedirect(_Graph.Caches[typeof(Users)], usr, string.Empty, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Task)]
		[PXShortCut(true, false, false, 'K', 'C')]		
		public virtual IEnumerable NewTask(PXAdapter adapter)
		{
			CreateActivity(CRActivityClass.Task, null);
			return adapter.Get();
		}

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Event)]
		[PXShortCut(true, false, false, 'E', 'C')]
		public virtual IEnumerable NewEvent(PXAdapter adapter)
		{
			CreateActivity(CRActivityClass.Event, null);
			return adapter.Get();
		}

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		[PXShortCut(true, false, false, 'A', 'C')]
		public virtual IEnumerable NewActivity(PXAdapter adapter)
		{
			return NewActivityByType(adapter);
		}

		protected virtual IEnumerable NewActivityByType(PXAdapter adapter)
		{
			return NewActivityByType(adapter, adapter.Menu);
		}

		protected virtual IEnumerable NewActivityByType(PXAdapter adapter, string type)
		{
			CreateActivity(CRActivityClass.Activity, type);
			return adapter.Get();
		}

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.MailSend)]
		[PXShortCut(true, false, false, 'A', 'M')]
		public virtual IEnumerable NewMailActivity(PXAdapter adapter)
		{
			CreateActivity(CRActivityClass.Email, null);
			return adapter.Get();
		}

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewAllActivities(PXAdapter adapter)
		{
			var gr = new ActivitiesMaint();
			gr.Filter.Current.NoteID = ((PXCache<TActivity>)_Graph.Caches[typeof(TActivity)]).InitNewRow().RefNoteID;
			throw new PXPopupRedirectException(gr, string.Empty, true);
		}

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewActivity(PXAdapter adapter)
		{
			EPActivity current = CurrentActivity;
			var graphType = EPActivityPrimaryGraphAttribute.GetGraphType(current);
			if (!PXAccess.VerifyRights(graphType))
			{
				adapter.View.Ask(null, Messages.AccessDenied, Messages.FormNoAccessRightsMessage(graphType), MessageButtons.OK, MessageIcon.Error);
			}
			else 
			{
				PXCache cache = CreateInstanceCache<EPActivity>(graphType);
				if (cache != null)
				{
					var searchView = new PXView(
						cache.Graph,
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
						PXRedirectHelper.TryRedirect(cache.Graph, PXRedirectHelper.WindowMode.NewWindow);
					}
				}
			}
			return adapter.Get();
		}

		#endregion

		#region Buttons

		public PXAction ButtonViewAllActivities
		{
			get { return _Graph.Actions[_VIEWALLACTIVITIES_COMMAND]; }
		}

		#endregion

		#region Properties

		public bool EnshureTableData
		{
			get { return _enshureTableData; }
			set { _enshureTableData = value; }
		}

		public virtual GetEmailHandler GetNewEmailAddress { get; set; }

		public string DefaultSubject { get; set; }

		public int? DefaultEMailAccountId
		{
			get
			{
				return _defaultEMailAccountId ?? MailAccountManager.DefaultMailAccountID;
			}
			set { _defaultEMailAccountId = value; }
		}

		#endregion

		#region Event Handlers
		protected virtual void Table_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			object row = e.Row;
			if (sender.Graph.GetType() != typeof(PXGraph) && sender.Graph.GetType() != typeof(PXGenericInqGrph))
			{
				Type itemType = sender.GetItemType();
				DynamicRowSelected rs = new DynamicRowSelected(itemType, row, sender, this);
				//will be called after graph event
				sender.Graph.RowSelected.AddHandler(itemType, rs.RowSelected);
			}
			
		}

		private class DynamicRowSelected
		{
			private Type ItemType;
			private object Row;
			private PXCache Cache;
			private CRActivityListBase<TPrimaryView, TActivity> BaseClass;
			public DynamicRowSelected(Type itemType, object row, PXCache cache, CRActivityListBase<TPrimaryView, TActivity> baseClass)
			{
				ItemType = itemType;
				Row = row;
				Cache = cache;
				BaseClass = baseClass;
			}
			public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				BaseClass.CorrectButtons(Cache, Row, Cache.GetStatus(Row));
				sender.Graph.RowSelected.RemoveHandler(ItemType, RowSelected);
			}
		}

		internal void CorrectButtons(PXCache sender, object row, PXEntryStatus status)
		{
			if (!EnshureTableData) return;

			row = row ?? sender.Current;
			var viewButtonsEnabled = row != null;

			viewButtonsEnabled = viewButtonsEnabled && Array.IndexOf(NotEditableStatuses, status) < 0;
			var editButtonEnabled = viewButtonsEnabled && this.View.Cache.AllowInsert;
			PXActionCollection actions = sender.Graph.Actions;

			actions[_NEWTASK_COMMAND].SetEnabled(editButtonEnabled);
			actions[_NEWEVENT_COMMAND].SetEnabled(editButtonEnabled);
			actions[_NEWMAILACTIVITY_COMMAND].SetEnabled(editButtonEnabled);
			actions[_NEWACTIVITY_COMMAND].SetEnabled(editButtonEnabled);

			PXButtonState state = actions[_NEWACTIVITY_COMMAND].GetState(row) as PXButtonState;
			if (state != null && state.Menus != null)
				foreach (var button in state.Menus)
				{
					button.Enabled = editButtonEnabled;
				}
		}

		protected virtual void Table_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Completed)
			{
				object row = e.Row;
				CorrectButtons(sender, row, PXEntryStatus.Notchanged);
			}
		}

		protected virtual void Activity_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			if (e.Row != null)
			{
				DeleteActivity((EPActivity)e.Row);

				sender.SetStatus(e.Row, PXEntryStatus.Notchanged);
				sender.Remove(e.Row);

				sender.SetValuePending(e.Row, "cacheIsDirty", sender.IsDirty);
			}
		}

		protected virtual void Activity_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (e.Row != null)
				sender.IsDirty = true.Equals(sender.GetValuePending(e.Row, "cacheIsDirty"));
		}

		protected virtual void Activity_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Delete) == PXDBOperation.Delete) e.Cancel = true;
		}

		protected virtual void Activity_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as EPActivity;
			if (row == null) return;

			if (row.ClassID == CRActivityClass.Task || row.ClassID == CRActivityClass.Event)
			{
				int timespent = 0;
				int overtimespent = 0;
				int timebillable = 0;
				int overtimebillable = 0;

				foreach (EPActivity child in 
					PXSelect<CRChildActivity,
						Where<CRChildActivity.parentTaskID, Equal<Required<CRChildActivity.parentTaskID>>>>.
						Select(_Graph, row.TaskID))
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
			}
		}

		protected virtual void Activity_RefNoteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = GetNoteId(sender.Graph, sender.Graph.Caches[typeof(TPrimaryView)].Current);
		}

		#endregion

		public TActivity Current
		{
			get { return (TActivity) View.Cache.Current; }
		}

		public virtual PXResultset<TActivity> Select(params object[] arguments)
		{
			var ret = new PXResultset<TActivity>();
			foreach (object item in View.SelectMulti(arguments))
				if (!(item is PXResult<TActivity>))
				{
					if (item is TActivity)
					{
						ret.Add(new PXResult<TActivity>((TActivity) item));
					}
				}
				else
					ret.Add((PXResult<TActivity>) item);
			return ret;
		}


		protected virtual void ReadNoteIDFieldInfo(out string noteField, out Type noteBqlField)
		{
			var cache = _Graph.Caches[typeof (TPrimaryView)];
			noteField = EntityHelper.GetNoteField(typeof (TPrimaryView));
			if (string.IsNullOrEmpty(_noteField))
				throw new ArgumentException(
					string.Format("Type '{0}' must contain field with PX.Data.NoteIDAttribute on it",
								  typeof (TPrimaryView).GetLongName()));
			noteBqlField = cache.GetBqlField(_noteField);
		}

		protected virtual void SetCommandCondition()
		{
			var refID = PrimaryViewType == null || !typeof (BAccount).IsAssignableFrom(PrimaryViewType)
				            ? typeof (EPActivity.refNoteID)
				            : typeof (EPActivity.parentRefNoteID);

			var newCmd = OriginalCommand.WhereAnd(
				BqlCommand.Compose(typeof(Where<,,>),
					typeof(EPActivity.isCorrected),
					typeof(NotEqual<True>),
					typeof(And<,>),					
					refID,
					typeof(Equal<>),
					typeof(Current<>),
					_noteBqlField));
			View = new PXView(View.Graph, View.IsReadOnly, newCmd);
		}

		protected virtual PXEntryStatus[] NotEditableStatuses
		{
			get
			{
				return new[] { PXEntryStatus.Inserted, PXEntryStatus.InsertedDeleted };
			}
		}

		protected BqlCommand OriginalCommand
		{
			get { return _originalCommand; }
		}
	}

	#endregion

	#region CRReferencedTaskList

	public class CRReferencedTaskList<TPrimaryView> : PXSelectBase
		where TPrimaryView : class, IBqlTable
	{
		private const string _VIEWTASK_COMMAND = "ViewTask";

		public CRReferencedTaskList(PXGraph graph)
		{
			_Graph = graph;

			AddActions();
			AddEventHandlers();

			View = new PXView(graph, false, GetCommand(), new PXSelectDelegate(Handler));
			_Graph.EnshureCachePersistance(typeof(CRActivityRelation));
		}

		private void AddEventHandlers()
		{
			_Graph.FieldUpdating.AddHandler<CRActivityRelation.subject>((sender, args) => args.Cancel = true);
			_Graph.FieldUpdating.AddHandler<CRActivityRelation.startDate>((sender, args) => args.Cancel = true);
			_Graph.FieldUpdating.AddHandler<CRActivityRelation.endDate>((sender, args) => args.Cancel = true);
			_Graph.FieldUpdating.AddHandler<CRActivityRelation.completedDateTime>((sender, args) => args.Cancel = true);
			_Graph.FieldUpdating.AddHandler<CRActivityRelation.status>((sender, args) => args.Cancel = true);
			_Graph.FieldDefaulting.AddHandler<CRActivityRelation.taskID>(
				(sender, args) =>
					{
						var cache = _Graph.Caches[typeof(EPActivity)];
						var taskIDFieldName = cache.GetField(typeof(EPActivity).GetNestedType(typeof(EPActivity.taskID).Name));
						args.NewValue = cache.GetValue(cache.Current, taskIDFieldName);
					});
			_Graph.RowInserted.AddHandler<CRActivityRelation>((sender, args) => FillReadonlyValues(args.Row as CRActivityRelation));
			_Graph.RowUpdated.AddHandler<CRActivityRelation>((sender, args) => FillReadonlyValues(args.Row as CRActivityRelation));
			_Graph.RowPersisting.AddHandler<CRActivityRelation>(
				(sender, args) =>
					{
						if ((args.Row as CRActivityRelation).With(_ => _.RefTaskID) == null) args.Cancel = true;
					});
			_Graph.RowSelected.AddHandler(typeof(EPActivity), 
				(sender, args) =>
					{
						var tasksCache = sender.Graph.Caches[typeof(CRActivityRelation)];
						var isInserted = sender.GetStatus(args.Row) == PXEntryStatus.Inserted;
						EPActivity row = args.Row as EPActivity;
						var isEditable = row != null && row.UIStatus == ActivityStatusAttribute.Open;
						tasksCache.AllowDelete = 
							tasksCache.AllowUpdate = 
								tasksCache.AllowInsert =
									!isInserted && isEditable;
					});
		}

		private void FillReadonlyValues(CRActivityRelation row)
		{
			if (row == null || row.RefTaskID == null) return;

			var act = (EPActivity)PXSelect<EPActivity,
						Where<EPActivity.taskID, Equal<Required<EPActivity.taskID>>>>.
						Select(_Graph, row.RefTaskID);
			if (act != null && act.TaskID != null)
			{
				row.Subject = act.Subject;
				row.StartDate = act.StartDate;
				row.EndDate = act.EndDate;
				row.Status = act.UIStatus;
				row.CompletedDateTime = act.CompletedDateTime;
			}
		}

		private void AddActions()
		{
			AddAction(_Graph, _VIEWTASK_COMMAND, Messages.ViewActivity, ViewTask);
		}

		private PXAction AddAction(PXGraph graph, string name, string displayName, PXButtonDelegate handler)
		{
			var uiAtt = new PXUIFieldAttribute
			{
				DisplayName = PXMessages.LocalizeNoPrefix(displayName),
				MapEnableRights = PXCacheRights.Select
			};
			var res = (PXAction)Activator.CreateInstance(typeof(PXNamedAction<>).MakeGenericType(
				new[] { typeof(TPrimaryView) }), new object[] { graph, name, handler, new PXEventSubscriberAttribute[] { uiAtt } });
			graph.Actions[name] = res;
			return res;
		}

		private IEnumerable Handler()
		{
			BqlCommand command = View.BqlSelect;
			var startRow = PXView.StartRow;
			int totalRows = 0;
			var list = new PXView(PXView.CurrentGraph, false, command).
				Select(null, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
					   ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;
			foreach (PXResult<CRActivityRelation, EPActivity> record in list)
			{
				var row = (CRActivityRelation)record[typeof(CRActivityRelation)];
				var act = (EPActivity) record[typeof(EPActivity)];
				var status = View.Cache.GetStatus(row);
				if (status == PXEntryStatus.Inserted || status == PXEntryStatus.Updated)
				{
					act = (EPActivity)PXSelect<EPActivity,
						Where<EPActivity.taskID, Equal<Required<EPActivity.taskID>>>>.
						Select(_Graph, row.RefTaskID);
				}
				if (act != null && act.TaskID != null)
				{
					row.Subject = act.Subject;
					row.StartDate = act.StartDate;
					row.EndDate = act.EndDate;
					row.Status = act.UIStatus;
					row.CompletedDateTime = act.CompletedDateTime;
				}
				yield return row;
			}
		}

		private static BqlCommand GetCommand()
		{
			return BqlCommand.CreateInstance(
				typeof(Select2<CRActivityRelation,
					InnerJoin<EPActivity, On<EPActivity.taskID, Equal<CRActivityRelation.refTaskID>>>,
					Where<CRActivityRelation.taskID, Equal<Current<EPActivity.taskID>>>,
					OrderBy<Asc<CRActivityRelation.refTaskID>>>));
		}

		[PXUIField(Visible = false)]
		[PXButton]
		public virtual IEnumerable ViewTask(PXAdapter adapter)
		{
			var current = (EPActivity)PXSelect<EPActivity,
				Where<EPActivity.taskID, Equal<Current<CRActivityRelation.refTaskID>>>>.
				Select(_Graph);
			var graphType = current.With(_ => EPActivityPrimaryGraphAttribute.GetGraphType(_));
			if (!PXAccess.VerifyRights(graphType))
			{
				adapter.View.Ask(null, Messages.AccessDenied, Messages.FormNoAccessRightsMessage(graphType), MessageButtons.OK, MessageIcon.Error);
			}
			else
			{
				PXCache cache = CreateInstanceCache<EPActivity>(graphType);
				if (cache != null)
				{
					var searchView = new PXView(
						cache.Graph,
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
						throw new PXPopupRedirectException(cache.Graph, graphType.Name, true)
						{
							Mode = PXBaseRedirectException.WindowMode.New
						};
					}
				}
			}
			return adapter.Get();
		}

		protected PXCache CreateInstanceCache<TNode>(Type graphType)
			where TNode : IBqlTable
		{
			if (graphType != null)
			{
				if (_Graph.IsDirty)
					_Graph.Actions.PressSave();

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

	#endregion

	#region Email

	public struct Email
	{
		private static Regex _emailRegex = new Regex("(?<Name>[^;]+?)\\s*\\((?<Address>[^;]*?)\\)",
			RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

		private readonly string _address;
		private readonly string _displayName;

		private string _toString;

		public static readonly Email Empty = new Email(null, null);

		public Email(string displayName, string address)
		{
			_displayName = displayName;
			if (_displayName != null) _displayName = _displayName.Replace(';', ' '); //.Replace(',', ' ');
			_address = address ?? string.Empty;

			_toString = null;
		}

		public string Address
		{
			get { return _address; }
		}

		public string DisplayName
		{
			get { return _displayName; }
		}

		public override string ToString()
		{
			if (_toString == null)
			{
				var isAddressComplex = _emailRegex.IsMatch(_address);
				_toString = isAddressComplex || string.IsNullOrEmpty(_displayName)
								? CorrectEmailAddress(_address)
								: TextUtils.QuoteString(_displayName )+ " <" + _address + ">";
			}
			return _toString;
		}

		private static string CorrectEmailAddress(string address)
		{
			var sb = new StringBuilder(address.Length * 2);
			foreach (string str in address.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
			{
				var cStr = str.Trim();
				if (string.IsNullOrEmpty(cStr)) continue;
				if (_emailRegex.IsMatch(address))
				{
					sb.Append(cStr);
				}
				else
				{
					var value = cStr.Replace('<', '(').Replace('>', ')');
					sb.AppendFormat("{0} <{0}>", value);
				}
				sb.Append(';');
			}
			return sb.ToString();
		}
	}

	#endregion

	#region NotificationUtility

	public sealed class NotificationUtility
	{
		private readonly PXGraph _graph;

		public NotificationUtility(PXGraph graph)
		{
			_graph = graph;
		}

		public Guid? SearchSetupID(string source, string notificationCD)
		{
			if (source == null || notificationCD == null) return null;
			NotificationSetup setup =
			PXSelect<NotificationSetup,
				Where<NotificationSetup.sourceCD, Equal<Required<NotificationSetup.sourceCD>>,
					And<NotificationSetup.notificationCD, Equal<Required<NotificationSetup.notificationCD>>>>>
					.SelectWindowed(_graph, 0, 1, source, notificationCD);
			return setup == null ? null : setup.SetupID;
		}

		public string SearchReport(string sourceType, object row, string reportID, int? branchID)
		{
			if (sourceType == null) return reportID;
			NotificationSetup setup =
			PXSelect<NotificationSetup,
				Where<NotificationSetup.sourceCD, Equal<Required<NotificationSetup.sourceCD>>,
					And<NotificationSetup.reportID, Equal<Required<NotificationSetup.reportID>>>>>
					.SelectWindowed(_graph, 0, 1, sourceType, reportID);

			if (setup == null) return reportID;

			NotificationSource notification = GetSource(row, (Guid)setup.SetupID, branchID);

			return notification == null || notification.ReportID == null
				? reportID :
				notification.ReportID;
		}

		public NotificationSource GetSource(object row, Guid setupID, int? branchID)
		{
			if (row == null) return null;
			PXGraph graph = CreatePrimaryGraph(row);
			NavigateRow(graph, row);

			PXView notificationView = null;
			graph.Views.TryGetValue("NotificationSources", out notificationView);

			if (notificationView == null)
			{
				foreach (PXView view in graph.GetViewNames().Select(name => graph.Views[name]).Where(view => typeof(NotificationSource).IsAssignableFrom(view.GetItemType())))
				{
					notificationView = view;
					break;
				}
			}
			if (notificationView == null) return null;
			NotificationSource result = null;
			foreach (NotificationSource rec in notificationView.SelectMulti())
			{
				if (rec.SetupID == setupID && rec.NBranchID == branchID)
					return rec;
				if(rec.SetupID == setupID && rec.NBranchID == null)
					result = rec;
			}
			return result;
		}

		public RecipientList GetRecipients(object row, NotificationSource source)
		{
			if (row == null) return null;
			PXGraph graph = CreatePrimaryGraph(row);
			NavigateRow(graph, row);
			NavigateRow(graph, source, false);


			PXView recipientView;
			graph.Views.TryGetValue("NotificationRecipients", out recipientView);

			if (recipientView == null)
			{
				foreach (PXView view in graph.GetViewNames().Select(name => graph.Views[name]).Where(view => typeof (NotificationRecipient).IsAssignableFrom(view.GetItemType())))
				{
					recipientView = view;
					break;
				}
			}
			if (recipientView != null)
			{
				RecipientList recipient = null;
				Dictionary<string, string> errors = new Dictionary<string, string>();
				int count = 0;
				foreach (NotificationRecipient item in recipientView.SelectMulti())
				{
					NavigateRow(graph, item, false);
					if (item.Active == true)
					{
						count++;
						if (string.IsNullOrWhiteSpace(item.Email))
						{
							string currEmail = ((NotificationRecipient) graph.Caches[typeof (NotificationRecipient)].Current).Email;
							if (string.IsNullOrWhiteSpace(currEmail))
							{
								Contact contact = PXSelect<Contact, Where<Contact.contactID, Equal<Current<NotificationRecipient.contactID>>>>.SelectSingleBound(_graph, new object[] {item});
								NotificationContactType.ListAttribute list = new NotificationContactType.ListAttribute();
								StringBuilder display = new StringBuilder(list.ValueLabelDic[item.ContactType]);
								if (contact != null)
								{
									display.Append(" ");
									display.Append(contact.DisplayName);
								}
								errors.Add(count.ToString(CultureInfo.InvariantCulture), string.Format(Messages.EmptyEmail, display));
							}
							else
							{
								item.Email = currEmail;
							}
						}
						if (!string.IsNullOrWhiteSpace(item.Email))
						{
							if (recipient == null)
								recipient = new RecipientList();
							recipient.Add(item);
						}
					}
				}
				if (errors.Any())
				{
					NotificationSetup nsetup = PXSelect<NotificationSetup, Where<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>.SelectSingleBound(_graph, new object[] {source});
					throw new PXOuterException(errors, _graph.GetType(), row, Messages.InvalidRecipients, errors.Count, count, nsetup.NotificationCD, nsetup.Module);
				}
				else
				{
					return recipient;
				}
			}
			return null;
		}

		private PXGraph CreatePrimaryGraph(object row)
		{
			Type graphType = new EntityHelper(_graph).GetPrimaryGraphType(row, false);
			if (graphType == null)
				throw new PXException(PX.SM.Messages.NotificationGraphNotFound);

			var res = graphType == _graph.GetType()
				? _graph
				: (PXGraph)PXGraph.CreateInstance(graphType);
			return res;
		}

		private static void NavigateRow(PXGraph graph, object row, bool primaryView = true)
		{
			Type type = row.GetType();
			PXCache primary = graph.Views[graph.PrimaryView].Cache;
			if (primary.GetItemType().IsAssignableFrom(row.GetType()))
			{
				graph.Caches[type].Current = row;
			}
			else if (row.GetType().IsAssignableFrom(primary.GetItemType()))
			{
				object current = primary.CreateInstance();
				PXCache parent = (PXCache)Activator.CreateInstance(typeof(PXCache<>).MakeGenericType(row.GetType()), primary.Graph);
				parent.RestoreCopy(current, row);
				primary.Current = current;
			}
			else
			if (primaryView)
			{
				object[] searches = new object[primary.Keys.Count];
				string[] sortcolumns = new string[primary.Keys.Count];
				for(int i=0 ;i < primary.Keys.Count; i++)
				{
					searches[i] = graph.Caches[type].GetValue(row, primary.Keys[i]);
					sortcolumns[i] = primary.Keys[i];
				}
				int startRow = 0, totalRows = 0;
				graph.Views[graph.PrimaryView].Select(null, null, searches, sortcolumns, null, null, ref startRow, 1, ref totalRows);
			}
			else
			{
				primary = graph.Caches[type];
				object current = primary.CreateInstance();
				PXCache parent = (PXCache)Activator.CreateInstance(typeof(PXCache<>).MakeGenericType(type), primary.Graph);
				parent.RestoreCopy(current, row);
				primary.Current = current;
			}
		}
	}

	#endregion

	#region CRNotificationSetupList

	public class CRNotificationSetupList<Table> : PXSelect<Table>
		where Table : NotificationSetup, new()		
	{
		public CRNotificationSetupList(PXGraph graph)
			: base(graph)
		{
			graph.Views.Caches.Add(typeof(NotificationSource));
			graph.Views.Caches.Add(typeof(NotificationRecipient));
			graph.RowDeleted.AddHandler(typeof(Table), OnRowDeleted);			
		}

		protected virtual void OnRowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			NotificationSetup row = (NotificationSetup)e.Row;

			PXCache source = cache.Graph.Caches[typeof(NotificationSource)];
			foreach (NotificationSource item in
				PXSelect<NotificationSource,
			Where<NotificationSource.setupID, Equal<Required<NotificationSource.setupID>>>>.Select(cache.Graph, row.SetupID))
			{
				source.Delete(item);
			}

			PXCache recipient = cache.Graph.Caches[typeof(NotificationRecipient)];
			foreach (NotificationRecipient item in
				PXSelect<NotificationRecipient,
			Where<NotificationRecipient.setupID, Equal<Required<NotificationRecipient.setupID>>>>.Select(cache.Graph, row.SetupID))
			{
				recipient.Delete(item);
			}
		}

	}

	#endregion

	#region CRClassNotificationSourceList

	public class CRClassNotificationSourceList<Table, ClassID, SourceCD> : PXSelect<Table>
		where Table : NotificationSource, new()		
		where ClassID : IBqlField
		where SourceCD : Constant<string>
	{
		public CRClassNotificationSourceList(PXGraph graph)
			: base(graph)
		{						
			this.View = new PXView(graph, false,
				BqlCommand.CreateInstance(
				BqlCommand.Compose(
				typeof(Select2<,,,>), typeof(Table),
				typeof(InnerJoin<,>), typeof(NotificationSetup), 
				typeof(On<,,>), typeof(NotificationSetup.setupID), typeof(Equal<>), typeof(Table).GetNestedType(typeof(NotificationSource.setupID).Name),
				typeof(And<,>), typeof(NotificationSetup.sourceCD), typeof(Equal<>), typeof(SourceCD),
				typeof(Where<,>), typeof(Table).GetNestedType(typeof(NotificationSource.classID).Name), typeof(Equal<>), typeof(Optional<>), typeof(ClassID),
				typeof(OrderBy<>),
				typeof(Asc<>),
				typeof(Table).GetNestedType(typeof(NotificationSource.setupID).Name))));
		
			this.setupNotifications = 
				new PXView(graph, false,
				BqlCommand.CreateInstance(
				BqlCommand.Compose(typeof(Select<,>), typeof(NotificationSetup),
				typeof(Where<,>), typeof(NotificationSetup.sourceCD), typeof(Equal<>), typeof(SourceCD))));

			graph.RowInserted.AddHandler(BqlCommand.GetItemType(typeof(ClassID)), OnClassRowInserted);			
			graph.RowInserted.AddHandler<NotificationSource>(OnSourceRowInseted);						
		}
		private PXView setupNotifications;
		protected virtual void OnClassRowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{			
			if (e.Row == null || cache.GetValue(e.Row, typeof(ClassID).Name) == null) return;

			foreach (NotificationSetup n in	setupNotifications.SelectMulti())
			{
				Table source = new Table();
				source.SetupID = n.SetupID;
				this.Cache.Insert(source);
			}
		}

		protected virtual void OnSourceRowInseted(PXCache cache, PXRowInsertedEventArgs e)
		{
			NotificationSource source = (NotificationSource)e.Row;
			PXCache rCache = cache.Graph.Caches[typeof(NotificationRecipient)];
			foreach (NotificationSetupRecipient r in
					PXSelect<NotificationSetupRecipient,
					Where<NotificationSetupRecipient.setupID, Equal<Required<NotificationSetupRecipient.setupID>>>>
					.Select(cache.Graph, source.SetupID))
			{
				try
				{
					NotificationRecipient rec = (NotificationRecipient)rCache.CreateInstance();
					rec.SetupID = source.SetupID;
					rec.ContactType = r.ContactType;
					rec.ContactID = r.ContactID;
					rec.Active = r.Active;
					rec.Hidden = r.Hidden;
					rCache.Insert(rec);
				}
				catch(Exception ex)
				{
					PXTrace.WriteError(ex);
				}
			}
		}
	}

	#endregion

	#region CRNotificationSourceList

	public class CRNotificationSourceList<Source, SourceClass, NotificationType> : EPDependNoteList<NotificationSource, NotificationSource.refNoteID, Source>
		where Source : class, IBqlTable	
		where SourceClass : class, IBqlField
		where NotificationType : class, IBqlOperand
	{
		protected readonly PXView _SourceView;
		protected readonly PXView _ClassView;

		public CRNotificationSourceList(PXGraph graph)
			: base(graph)
		{			
			this.View = new PXView(graph, false,
				BqlCommand.CreateInstance(
				BqlCommand.Compose(
				typeof(Select<,,>), typeof(NotificationSource),
				typeof(Where<boolTrue, Equal<boolTrue>>),
				typeof(OrderBy<>),
				typeof(Asc<>),
				typeof(NotificationSource).GetNestedType(typeof(NotificationSource.setupID).Name))),
				new PXSelectDelegate(NotificationSources));

			_SourceView = new PXView(graph, false,
				BqlCommand.CreateInstance(
				BqlCommand.Compose(typeof(Select<,>), typeof(NotificationSource), ComposeWhere)));



			_ClassView = new PXView(graph, true,
									BqlCommand.CreateInstance(BqlCommand.Compose(
										typeof (Select2<,,>),
										typeof (NotificationSource),
										typeof (InnerJoin<NotificationSetup, On<NotificationSetup.setupID, Equal<NotificationSource.setupID>>>),
										typeof (Where<,,>), typeof (NotificationSetup.sourceCD), typeof (Equal<>), typeof (NotificationType),
																typeof(And<,>), typeof(NotificationSource.classID), typeof(Equal<>), typeof(Optional<>), typeof(SourceClass))));

			graph.RowPersisting.AddHandler<NotificationSource>(OnRowPersisting);
			graph.RowDeleting.AddHandler<NotificationSource>(OnRowDeleting);
			graph.RowUpdated.AddHandler<NotificationSource>(OnRowUpdated);
			graph.RowSelected.AddHandler<NotificationSource>(OnRowSelected);
			
		}		
		protected virtual IEnumerable NotificationSources()
		{
			List<NotificationSource> result = new List<NotificationSource>();
			foreach(NotificationSource item in _SourceView.SelectMulti())
			{
				result.Add(item);
			}
			foreach (NotificationSource classItem in GetClassItems())
			{								
				if (result.Find(i => i.SetupID == classItem.SetupID && i.NBranchID == classItem.NBranchID) == null)
					result.Add(classItem);
			}						
			return result;
		}

		protected override void Source_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{			
			sender.Current = e.Row;
			internalDelete = true;
			try
			{
				foreach (NotificationSource item in _SourceView.SelectMulti())
				{
					this._SourceView.Cache.Delete(item);
				}
			}
			finally
			{
				internalDelete = false;
			}
		}
		private bool internalDelete;
		protected virtual void OnRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if(!sender.ObjectsEqual<NotificationSource.overrideSource>(e.Row, e.OldRow))
			{
				NotificationSource row = (NotificationSource) e.Row;
				if (row.OverrideSource == false)
				{
					internalDelete = true;
					sender.Delete(row);
					this.View.RequestRefresh();
				}
			}
		}
		protected virtual void OnRowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			if (internalDelete) return;
			NotificationSource row = (NotificationSource)e.Row;
			foreach(NotificationSource classItem in GetClassItems())
				if (classItem.SetupID == row.SetupID && classItem.NBranchID == row.NBranchID && row.OverrideSource == false)
				{
					e.Cancel = true;
					throw new PXRowPersistingException(typeof(NotificationSource).Name, null, Messages.DeleteClassNotification);
				}	
			if(!e.Cancel)
				this.View.RequestRefresh();
		}
		protected virtual void OnRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{			
			NotificationSource row = (NotificationSource)e.Row;
			if(row.ClassID != null)
			{
				if (e.Operation == PXDBOperation.Delete)
					e.Cancel = true;

				if (e.Operation == PXDBOperation.Update)
				{
					sender.SetStatus(row, PXEntryStatus.Deleted);
					NotificationSource ins = (NotificationSource)sender.CreateInstance();
					ins.SetupID = row.SetupID;
					ins = (NotificationSource)sender.Insert(ins);

					NotificationSource clone = PXCache<NotificationSource>.CreateCopy(row);
					clone.NBranchID = ins.NBranchID;
					clone.SourceID  = ins.SourceID;
					clone.RefNoteID = ins.RefNoteID;
					clone.ClassID  = null;					
					clone = (NotificationSource)sender.Update(clone);
					if (clone != null)
					{						
						sender.PersistInserted(clone);						
						sender.Normalize();
						sender.SetStatus(clone, PXEntryStatus.Notchanged);
						PXCache source = sender.Graph.Caches[BqlCommand.GetItemType(SourceNoteID)];
						long? refNoteID = (long?)source.GetValue(source.Current, SourceNoteID.Name);
						if (refNoteID != null)
						{
							foreach (NotificationRecipient r in PXSelect<NotificationRecipient,
							Where<NotificationRecipient.sourceID, Equal<Required<NotificationRecipient.sourceID>>,
							  And<NotificationRecipient.refNoteID, Equal<Required<NotificationRecipient.refNoteID>>,
								And<NotificationRecipient.classID, IsNotNull>>>>
							.Select(sender.Graph, row.SourceID, refNoteID))
							{								
								PXCache cache = sender.Graph.Caches[typeof(NotificationRecipient)];
								if (cache.GetStatus(r) == PXEntryStatus.Updated ||
									cache.GetStatus(r) == PXEntryStatus.Inserted) continue;
								NotificationRecipient u = (NotificationRecipient)cache.CreateCopy(r);
								u.SourceID = clone.SourceID;
								u.ClassID  = null;
								cache.Update(u);
							}
						}
					}
					e.Cancel = true;
				}				
			}
		}
		public virtual void OnRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;
			NotificationSource row = (NotificationSource)e.Row;
			bool classitem = GetClassItems().Any(cs => cs.SourceID == row.SourceID);
			PXUIFieldAttribute.SetEnabled<NotificationSource.overrideSource>(sender, row, !classitem);
			PXUIFieldAttribute.SetEnabled<NotificationSource.setupID>(sender, row, !classitem);			
		}

		private IEnumerable<NotificationSource> GetClassItems()
		{
			foreach (object rec in _ClassView.SelectMulti())
			{
				NotificationSource classItem =
					(rec is PXResult && ((PXResult)rec)[0] is NotificationSource) ? (NotificationSource)((PXResult)rec)[0] :
					(rec is NotificationSource ? (NotificationSource)rec : null);
				if (classItem == null) continue;
				yield return classItem;
			}
		}
	}

	#endregion

	#region CRNotificationRecipientList

	public class CRNotificationRecipientList<Source, SourceClassID> : EPDependNoteList<NotificationRecipient, NotificationRecipient.refNoteID, Source> 
		where Source : class, IBqlTable
		where SourceClassID : class, IBqlField
	{
		protected readonly PXView _SourceView;
		protected readonly PXView _ClassView;

		public CRNotificationRecipientList(PXGraph graph)
			: base(graph)
		{
			Type table = typeof(NotificationRecipient);
			Type where = BqlCommand.Compose(
				typeof(Where<,,>),typeof(NotificationRecipient.sourceID), typeof(Equal<Optional<NotificationSource.sourceID>>),
				typeof(And<,>),typeof(NotificationRecipient.refNoteID), typeof(Equal<>), typeof(Current<>), this.SourceNoteID);

			this.View = new PXView(graph, false,
				BqlCommand.CreateInstance(
				BqlCommand.Compose(
				typeof(Select<,,>), table,
				typeof(Where<boolTrue, Equal<boolTrue>>),
				typeof(OrderBy<>),
				typeof(Asc<>),
				table.GetNestedType(typeof(NotificationRecipient.orderID).Name))),
				new PXSelectDelegate(NotificationRecipients));

			_SourceView = new PXView(graph, false,
				BqlCommand.CreateInstance(
				BqlCommand.Compose(typeof(Select<,>), typeof(NotificationRecipient), where)));

	

			_ClassView = new PXView(graph, true,
									BqlCommand.CreateInstance(BqlCommand.Compose(
										typeof (Select<,>), typeof (NotificationRecipient),
										typeof (Where<,,>), typeof (NotificationRecipient.classID), typeof (Equal<>), typeof (Current<>), typeof (SourceClassID),
										typeof (And<,,>), typeof (NotificationRecipient.setupID), typeof (Equal<Current<NotificationSource.setupID>>),
										typeof (And<NotificationRecipient.refNoteID, IsNull>)
																  )));

			graph.RowPersisting.AddHandler<NotificationRecipient>(OnRowPersisting);
			graph.RowSelected.AddHandler<NotificationRecipient>(OnRowSeleted);
			graph.RowDeleting.AddHandler<NotificationRecipient>(OnRowDeleting);
			graph.RowUpdated.AddHandler<NotificationRecipient>(OnRowUpdated);
			graph.RowInserting.AddHandler<NotificationRecipient>(OnRowInserting);
			graph.RowUpdating.AddHandler<NotificationRecipient>(OnRowUpdating);
		}


		protected virtual IEnumerable NotificationRecipients()
		{
			var result = new List<NotificationRecipient>();			
			foreach (NotificationRecipient item in _SourceView.SelectMulti())
			{
				item.OrderID = item.NotificationID;
				result.Add(item);
			}
			
			foreach (NotificationRecipient classItem in GetClassItems())
			{
				NotificationRecipient item = result.Find(i =>
					i.ContactType == classItem.ContactType &&
					i.ContactID == classItem.ContactID);
				if (item == null)
				{
					item = classItem;
					result.Add(item);
				}
				item.OrderID = int.MinValue + classItem.NotificationID;
			}
			return result;
		}

		protected override void Source_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			sender.Current = e.Row;
			internalDelete = true;
			try
			{
				foreach (NotificationRecipient item in _SourceView.SelectMulti())
				{
					this._SourceView.Cache.Delete(item);
				}
			}
			finally
			{
				internalDelete = false;
			}
		}
		private bool internalDelete;

		protected virtual void OnRowSeleted(PXCache sender, PXRowSelectedEventArgs e)
		{
			NotificationRecipient row = (NotificationRecipient)e.Row;
			if (row == null) return;
			bool updatableContractID = 
				(row.ContactType == NotificationContactType.Contact ||
			   row.ContactType == NotificationContactType.Employee);
			bool updatableContactType = !GetClassItems().Any(classItem => row.ContactType == classItem.ContactType &&
																		  row.ContactID == classItem.ContactID);

			PXUIFieldAttribute.SetEnabled(sender, row, typeof(NotificationRecipient.contactID).Name, updatableContactType && updatableContractID);
			PXUIFieldAttribute.SetEnabled(sender, row, typeof(NotificationRecipient.contactType).Name, updatableContactType);
		}
		protected virtual void OnRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			NotificationRecipient row = (NotificationRecipient)e.Row;											
			
			if (row.ClassID != null)
			{				
				if (e.Operation == PXDBOperation.Update)
				{
					sender.SetStatus(row, PXEntryStatus.Deleted);
					NotificationRecipient ins   = (NotificationRecipient)sender.Insert();
					NotificationRecipient clone = PXCache<NotificationRecipient>.CreateCopy(row);
					clone.NotificationID = ins.NotificationID;
					clone.RefNoteID      = ins.RefNoteID;
					clone.ClassID        = null;
					NotificationSource source = 
						PXSelectReadonly<NotificationSource,
						Where<NotificationSource.setupID, Equal<Required<NotificationSource.setupID>>,
							And<NotificationSource.refNoteID, Equal<Required<NotificationSource.refNoteID>>>>>.
							Select(sender.Graph, row.SetupID, row.RefNoteID);
					if (source != null)
						clone.SourceID = source.SourceID;

					clone = (NotificationRecipient)sender.Update(clone);					
					if (clone != null)
					{						
						sender.PersistInserted(clone);						
						sender.Normalize();
						sender.SetStatus(clone, PXEntryStatus.Notchanged);						
					}
					e.Cancel = true;
				}
			}			 
		}
		protected virtual void OnRowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			if (internalDelete) return;
			NotificationRecipient row = (NotificationRecipient)e.Row;
			foreach (NotificationRecipient classItem in GetClassItems())
				if (classItem.SetupID			== row.SetupID && 
					  classItem.ContactType == row.ContactType && 
						classItem.ContactID		== row.ContactID)
				{
					if (row.RefNoteID == null)
					{
						e.Cancel = true;
						throw new PXRowPersistingException(typeof (NotificationRecipient).Name, null, Messages.DeleteClassNotification);
					}
				}
			if(!e.Cancel)	
				this.View.RequestRefresh();
		}
		protected virtual void OnRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			NotificationRecipient row = (NotificationRecipient)e.Row;
			PXCache source = sender.Graph.Caches[BqlCommand.GetItemType(SourceNoteID)];
			row.RefNoteID = (long?)source.GetValue(source.Current, SourceNoteID.Name);
		}
		protected virtual void OnRowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (e.Row != null)
			{
				e.Cancel = !sender.Graph.IsImport && !ValidateDuplicates(sender, (NotificationRecipient)e.Row, null);
				if (e.Cancel != true)
				{
					NotificationRecipient r = (NotificationRecipient)e.Row;
					NotificationSource source = (NotificationSource)sender.Graph.Caches[typeof(NotificationSource)].Current;
					r.ClassID = source != null ? source.ClassID : null;
				}
			}
		}

		protected virtual void OnRowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (e.Row != null && e.NewRow != null && CheckUpdated(sender, (NotificationRecipient)e.Row, (NotificationRecipient)e.NewRow))
				e.Cancel = !ValidateDuplicates(sender, (NotificationRecipient)e.NewRow, (NotificationRecipient)e.Row);
		}
		private IEnumerable<NotificationRecipient> GetClassItems()
		{
			foreach (object rec in _ClassView.SelectMulti())
			{
				NotificationRecipient classItem =
					(rec is PXResult && ((PXResult)rec)[0] is NotificationRecipient) ? (NotificationRecipient)((PXResult)rec)[0] :
					(rec is NotificationRecipient ? (NotificationRecipient)rec : null);
				if (classItem == null) continue;
				yield return classItem;
			}
		}
		private bool CheckUpdated(PXCache sender, NotificationRecipient row, NotificationRecipient newRow)
		{
			return row.ContactType != newRow.ContactType || row.ContactID != newRow.ContactID;
		}

		private bool ValidateDuplicates(PXCache sender, NotificationRecipient row, NotificationRecipient oldRow)
		{			
			foreach (NotificationRecipient sibling in this.View.SelectMulti())
			{
				if (!CheckUpdated(sender, sibling, row) && row != sibling)
				{

					if (oldRow == null || row.ContactType != oldRow.ContactType)
						sender.RaiseExceptionHandling<NotificationRecipient.contactType>(
							row, row.ContactType,
							new PXSetPropertyException(ErrorMessages.DuplicateEntryAdded));

					if (oldRow == null || row.ContactID != oldRow.ContactID)
						sender.RaiseExceptionHandling<NotificationRecipient.contactID>(
							row, row.ContactType,
							new PXSetPropertyException(ErrorMessages.DuplicateEntryAdded));
					return false;
				}
			}
			sender.RaiseExceptionHandling<NotificationRecipient.contactType>(
							row, row.ContactType, null);
			sender.RaiseExceptionHandling<NotificationRecipient.contactID>(
							row, row.ContactID, null);
			return true;
		}

	}

	#endregion

	#region EPActivityPreview

	//TODO: move to corresponding control PX.Web.Controls.dll
	/*[Serializable]
	public class EPActivityPreview : EPGenericActivity
	{
		#region TaskID

		public new abstract class taskID : IBqlField { }

		#endregion

		#region Subject

		public new abstract class subject : IBqlField { }

		[PXDBString(255, InputMask = "", IsUnicode = true)]
		[PXUIField(DisplayName = "Summary", IsReadOnly = true)]
		public override string Subject
		{
			get
			{
				return base.Subject;
			}
			set
			{
				base.Subject = value;
			}
		}

		#endregion

		#region ColoredCategory

		public abstract class coloredCategory : IBqlField { }

		[PXString]
		[PXUIField(DisplayName = "Category")]
		public virtual String ColoredCategory { get; set; }

		#endregion

		#region DueDateDescription

		public abstract class dueDateDescription : IBqlField { }

		private string _dueDateDescription;
		[PXString]
		[PXUIField(DisplayName = "Due Date")]
		public virtual String DueDateDescription
		{
			get
			{
				if (_dueDateDescription == null)
				{
					_dueDateDescription = string.Empty;
					if (StartDate != null) _dueDateDescription = "Start " + ((DateTime)StartDate).ToString("g");
					if (EndDate != null)
					{
						if (_dueDateDescription.Length > 0) _dueDateDescription += ", end ";
						else _dueDateDescription += "End ";
						_dueDateDescription += ((DateTime)EndDate).ToString("g");
					}
				}
				return _dueDateDescription;
			}
		}

		#endregion

		public new abstract class owner : IBqlField { }

		[PXDBGuid]
		[PXUIField(DisplayName = "Performed by")]
		public override Guid? Owner
		{
			get
			{
				return base.Owner;
			}
			set
			{
				base.Owner = value;
			}
		}
	}*/

	#endregion

	#region PXActionExt<TNode>

	public class PXActionExt<TNode> : PXAction<TNode>
		where TNode : class, IBqlTable, new()
	{
		public delegate void AdapterPrepareHandler(PXAdapter adapter);
		public delegate void AdapterPrepareInsertHandler(PXAdapter adapter, object previousCurrent);

		private PXView _view;
		private AdapterPrepareHandler _selectPrepareHandler;
		private AdapterPrepareInsertHandler _insertPrepareHandler;

		protected PXActionExt(PXGraph graph) : base(graph)
		{
		}

		public PXActionExt(PXGraph graph, string name) : base(graph, name)
		{
		}

		public PXActionExt(PXGraph graph, Delegate handler) : base(graph, handler)
		{
		}

		public virtual void SetPrepareHandlers(AdapterPrepareHandler selectPrepareHandler, AdapterPrepareInsertHandler insertPrepareHandler)
		{
			SetPrepareHandlers(selectPrepareHandler, insertPrepareHandler, null);
		}

		public virtual void SetPrepareHandlers(AdapterPrepareHandler selectPrepareHandler, AdapterPrepareInsertHandler insertPrepareHandler, PXView view)
		{
			if (view != null && !typeof(TNode).IsAssignableFrom(view.GetItemType()))
				throw new ArgumentException(string.Format("Item of view must inherit '{0}'", typeof(TNode).Name), "view");
			_view = view;
			_selectPrepareHandler = selectPrepareHandler;
			_insertPrepareHandler = insertPrepareHandler;
		}

		public override IEnumerable Press(PXAdapter adapter)
		{
			var newAdapter = new PXAdapter(_view ?? adapter.View);
			PXAdapter.Copy(adapter, newAdapter);
			if (_selectPrepareHandler != null) _selectPrepareHandler(adapter);
			var result = new List<object>(base.Press(newAdapter).Cast<object>());
			PXAdapter.Copy(newAdapter, adapter);
			return result;
		}

		protected virtual void InsertExt(PXAdapter adapter, object previousCurrent)
		{
			if (_insertPrepareHandler != null) _insertPrepareHandler(adapter, previousCurrent);
			Insert(adapter);
		}

		protected override void Insert(PXAdapter adapter)
		{
			var vals = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			if (adapter.Searches != null)
				for (int i = 0; i < adapter.Searches.Length && i < adapter.SortColumns.Length; i++)
					vals[adapter.SortColumns[i]] = adapter.Searches[i];
			foreach (string key in adapter.View.Cache.Keys)
				if (!vals.ContainsKey(key)) vals.Add(key, null);
			if (adapter.View.Cache.Insert(vals) == 1)
			{
				if (adapter.SortColumns == null)
					adapter.SortColumns = adapter.View.Cache.Keys.ToArray();
				else
				{
					var cols = new List<string>(adapter.SortColumns);
					foreach (string key in adapter.View.Cache.Keys)
						if (!CompareIgnoreCase.IsInList(cols, key))
							cols.Add(key);
					adapter.SortColumns = cols.ToArray();
				}
				adapter.Searches = new object[adapter.SortColumns.Length];
				for (int i = 0; i < adapter.Searches.Length; i++)
				{
					object val;
					if (vals.TryGetValue(adapter.SortColumns[i], out val))
						adapter.Searches[i] = val is PXFieldState ? ((PXFieldState) val).Value : val;
				}
				adapter.StartRow = 0;
			}
		}
	}

	#endregion

	#region PXFirstExt

	public class PXFirstExt<TNode> : PXActionExt<TNode>
		where TNode : class, IBqlTable, new()
	{
		public PXFirstExt(PXGraph graph, string name)
			: base(graph, name)
		{
		}

		[PXUIField(DisplayName = ActionsMessages.First, MapEnableRights = PXCacheRights.Select)]
		[PXFirstButton]
		protected override IEnumerable Handler(PXAdapter adapter)
		{
			adapter.Currents = new object[] { adapter.View.Cache.Current };
			var previousCurrent = adapter.View.Cache.Current;
			_Graph.SelectTimeStamp();
			adapter.StartRow = 0;
			adapter.Searches = null;
			bool anyFound = false;
			foreach (object item in adapter.Get())
			{
				yield return item;
				anyFound = true;
			}
			if (!anyFound && adapter.MaximumRows == 1 && adapter.View.Cache.AllowInsert)
			{
				InsertExt(adapter, previousCurrent);
				foreach (object ret in adapter.Get())
				{
					yield return ret;
				}
				adapter.View.Cache.IsDirty = false;
			}
		}
	}

	#endregion

	#region PXLastExt

	public class PXLastExt<TNode> : PXActionExt<TNode>
		where TNode : class, IBqlTable, new()
	{
		public PXLastExt(PXGraph graph, string name)
			: base(graph, name)
		{
		}
		[PXUIField(DisplayName = ActionsMessages.Last, MapEnableRights = PXCacheRights.Select)]
		[PXLastButton]
		protected override IEnumerable Handler(PXAdapter adapter)
		{
			adapter.Currents = new object[] { adapter.View.Cache.Current };
			var previousCurrent = adapter.View.Cache.Current;
			_Graph.SelectTimeStamp();
			adapter.StartRow = -adapter.MaximumRows;
			adapter.Searches = null;
			bool anyFound = false;
			foreach (object item in adapter.Get())
			{
				yield return item;
				anyFound = true;
			}
			if (!anyFound && adapter.MaximumRows == 1 && adapter.View.Cache.AllowInsert)
			{
				InsertExt(adapter, previousCurrent);
				foreach (object ret in adapter.Get())
				{
					yield return ret;
				}
				adapter.View.Cache.IsDirty = false;
			}
		}
	}

	#endregion

	#region PXPreviousExt

	public class PXPreviousExt<TNode> : PXActionExt<TNode>
		where TNode : class, IBqlTable, new()
	{
		public PXPreviousExt(PXGraph graph, string name)
			: base(graph, name)
		{
		}
		[PXUIField(DisplayName = ActionsMessages.Previous, MapEnableRights = PXCacheRights.Select)]
		[PXPreviousButton]
		protected override IEnumerable Handler(PXAdapter adapter)
		{
			bool inserted = adapter.View.Cache.Current != null && adapter.View.Cache.GetStatus(adapter.View.Cache.Current) == PXEntryStatus.Inserted;
			if (inserted) return MoveToLast(adapter);

			var previousCurrent = adapter.View.Cache.Current;
			adapter.StartRow -= adapter.MaximumRows;
			_Graph.SelectTimeStamp();
			List<object> ret = (List<object>)adapter.Get();
			object curr = adapter.View.Cache.Current;
			adapter.Currents = new object[] { curr };
			_Graph.Clear(PXClearOption.PreserveTimeStamp);
			if (ret.Count == 0)
			{
				if (adapter.View.Cache.AllowInsert && adapter.MaximumRows == 1)
				{
					adapter.Currents = null;
					InsertExt(adapter, previousCurrent);
					ret = (List<object>)adapter.Get();
					adapter.View.Cache.IsDirty = false;
				}
				else
				{
					adapter.StartRow = -adapter.MaximumRows;
					adapter.Searches = null;
					ret = (List<object>) adapter.Get();
					if (ret.Count == 0 && adapter.View.Cache.AllowInsert && adapter.MaximumRows == 1)
					{
						InsertExt(adapter, previousCurrent);
						ret = (List<object>) adapter.Get();
						adapter.View.Cache.IsDirty = false;
					}
				}
			}
			return ret;
		}

		private IEnumerable MoveToLast(PXAdapter adapter)
		{
			adapter.Currents = new object[] { adapter.View.Cache.Current };
			var previousCurrent = adapter.View.Cache.Current;
			_Graph.Clear();
			_Graph.SelectTimeStamp();
			adapter.StartRow = -adapter.MaximumRows;
			adapter.Searches = null;
			bool anyFound = false;
			foreach (object item in adapter.Get())
			{
				yield return item;
				anyFound = true;
			}
			if (!anyFound && adapter.MaximumRows == 1 && adapter.View.Cache.AllowInsert)
			{
				InsertExt(adapter, previousCurrent);
				foreach (object ret in adapter.Get())
				{
					yield return ret;
				}
				adapter.View.Cache.IsDirty = false;
			}
		}
	}

	#endregion

	#region PXNextExt

	public class PXNextExt<TNode> : PXActionExt<TNode>
		where TNode : class, IBqlTable, new()
	{
		public PXNextExt(PXGraph graph, string name)
			: base(graph, name)
		{
		}
		[PXUIField(DisplayName = ActionsMessages.Next, MapEnableRights = PXCacheRights.Select)]
		[PXNextButton]
		protected override IEnumerable Handler(PXAdapter adapter)
		{
			bool inserted = adapter.View.Cache.Current != null && adapter.View.Cache.GetStatus(adapter.View.Cache.Current) == PXEntryStatus.Inserted;
			var previousCurrent = adapter.View.Cache.Current;
			adapter.StartRow += adapter.MaximumRows;
			_Graph.SelectTimeStamp();
			List<object> ret = (List<object>)adapter.Get();
			adapter.Currents = new object[] { adapter.View.Cache.Current };
			_Graph.Clear(PXClearOption.PreserveTimeStamp);
			if (ret.Count == 0)
			{
				if (!inserted && adapter.View.Cache.AllowInsert && adapter.MaximumRows == 1)
				{
					InsertExt(adapter, previousCurrent);
					ret = (List<object>)adapter.Get();
					adapter.View.Cache.IsDirty = false;
				}
				else
				{
					adapter.StartRow = 0;
					adapter.Searches = null;
					ret = (List<object>)adapter.Get();
					if (ret.Count == 0 && adapter.View.Cache.AllowInsert && adapter.MaximumRows == 1)
					{
						InsertExt(adapter, previousCurrent);
						ret = (List<object>)adapter.Get();
						adapter.View.Cache.IsDirty = false;
					}
				}
			}
			return ret;
		}
	}

	#endregion

	#region PXInsertExt

	public class PXInsertExt<TNode> : PXActionExt<TNode>
		where TNode : class, IBqlTable, new()
	{
		public PXInsertExt(PXGraph graph, string name)
			: base(graph, name)
		{
		}
		[PXUIField(DisplayName = ActionsMessages.Insert, MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert)]
		[PXInsertButton]
		protected override IEnumerable Handler(PXAdapter adapter)
		{
			if (!adapter.View.Cache.AllowInsert)
			{
				throw new PXException(ErrorMessages.CantInsertRecord);
			}
			var previousCurrent = adapter.View.Cache.Current;
			_Graph.Clear();
			_Graph.SelectTimeStamp();
			InsertExt(adapter, previousCurrent);
			var newSearches = new ArrayList();
			var newSortColumns = new List<string>();
			for (int i = 0; i < adapter.Searches.Length && i < adapter.SortColumns.Length; i++)
			{
				var sortColumn = adapter.SortColumns[i];
				if (adapter.View.Cache.Keys.Contains(sortColumn, StringComparer.OrdinalIgnoreCase))
				{
					newSearches.Add(adapter.Searches[i]);
					newSortColumns.Add(sortColumn);
				}
			}
			adapter.SortColumns = newSortColumns.ToArray();
			adapter.Searches = newSearches.ToArray();
			foreach (object ret in adapter.Get())
			{
				yield return ret;
			}
			adapter.View.Cache.IsDirty = false;
		}
	}

	#endregion

	#region PXDeleteExt

	public class PXDeleteExt<TNode> : PXActionExt<TNode>
		where TNode : class, IBqlTable, new()
	{
		public PXDeleteExt(PXGraph graph, string name)
			: base(graph, name)
		{
		}
		public override object GetState(object row)
		{
			object state = base.GetState(row);
			PXButtonState bs = state as PXButtonState;
			if (bs != null && !String.IsNullOrEmpty(bs.ConfirmationMessage))
			{
				if (typeof(TNode).IsDefined(typeof(PXCacheNameAttribute), true))
				{
					PXCacheNameAttribute attr = (PXCacheNameAttribute)(typeof(TNode).GetCustomAttributes(typeof(PXCacheNameAttribute), true)[0]);
					bs.ConfirmationMessage = String.Format(bs.ConfirmationMessage, attr.GetName());
				}
				else
				{
					bs.ConfirmationMessage = String.Format(bs.ConfirmationMessage, typeof(TNode).Name);
				}
			}
			return state;
		}
		[PXUIField(DisplayName = ActionsMessages.Delete, MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Delete)]
		[PXDeleteButton(ConfirmationMessage = ActionsMessages.ConfirmDeleteExplicit)]
		protected override IEnumerable Handler(PXAdapter adapter)
		{
			if (!adapter.View.Cache.AllowDelete)
			{
				throw new PXException(ErrorMessages.CantDeleteRecord);
			}
			var previousCurrent = adapter.View.Cache.Current;
			int startRow = adapter.StartRow;
			foreach (object item in adapter.Get())
			{
				adapter.View.Cache.Delete(item);
			}
			try
			{
				_Graph.Actions.PressSave();
			}
			catch
			{
				_Graph.Clear();
				throw;
			}
			_Graph.SelectTimeStamp();
			adapter.StartRow = startRow;
			bool anyFound = false;
			foreach (object item in adapter.Get())
			{
				yield return item;
				anyFound = true;
			}
			if (!anyFound && adapter.MaximumRows == 1)
			{
				if (adapter.View.Cache.AllowInsert)
				{
					InsertExt(adapter, previousCurrent);
					foreach (object ret in adapter.Get())
					{
						yield return ret;
					}
					adapter.View.Cache.IsDirty = false;
				}
				else
				{
					adapter.StartRow = 0;
					adapter.Searches = null;
					foreach (object item in adapter.Get())
					{
						yield return item;
					}
				}
			}
		}
	}

	#endregion

	#region PXCancelExt

	public class PXCancelExt<TNode> : PXActionExt<TNode>
		where TNode : class, IBqlTable, new()
	{
		public PXCancelExt(PXGraph graph, string name)
			: base(graph, name)
		{
		}
		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
		[PXCancelButton]
		protected override IEnumerable Handler(PXAdapter adapter)
		{
			adapter.Currents = new object[] { adapter.View.Cache.Current };
			var previousCurrent = adapter.View.Cache.Current;
			_Graph.Clear();
			_Graph.SelectTimeStamp();
			bool anyFound = false;
			bool perfromSearch = true;
			if (adapter.MaximumRows == 1)
			{
				perfromSearch = adapter.View.Cache.Keys.Count == 0;
				if (adapter.Searches != null)
				{
					for (int i = 0; i < adapter.Searches.Length; i++)
					{
						if (adapter.Searches[i] != null)
						{
							perfromSearch = true;
							break;
						}
					}
				}
			}
			if (perfromSearch)
			{
				foreach (object item in adapter.Get())
				{
					yield return item;
					anyFound = true;
				}
			}
			if (!anyFound && adapter.MaximumRows == 1)
			{
				if (adapter.View.Cache.AllowInsert)
				{
					_Graph.Clear();
					_Graph.SelectTimeStamp();
					InsertExt(adapter, previousCurrent);
					foreach (object ret in adapter.Get())
					{
						yield return ret;
					}
					adapter.View.Cache.IsDirty = false;
				}
				else
				{
					adapter.Currents = null;
					adapter.StartRow = 0;
					adapter.Searches = null;
					foreach (object item in adapter.Get())
					{
						yield return item;
					}
				}
			}
		}
	}

	#endregion

	#region CRRelationsList<TNoteField>

	public class CRRelationsList<TNoteField> : PXSelect<CRRelation>
		where TNoteField : IBqlField
	{
		public CRRelationsList(PXGraph graph) 
			: base(graph, GetHandler())
		{
			VerifyParameters();
			AttacheEventHandlers(graph);
		}

		private static void VerifyParameters()
		{
			if (BqlCommand.GetItemType(typeof (TNoteField)) == null)
				throw new ArgumentException(string.Format(Messages.IBqlFieldMustBeNested,
					typeof (TNoteField).GetLongName()), "TNoteField");
			if (!typeof (IBqlTable).IsAssignableFrom(BqlCommand.GetItemType(typeof (TNoteField))))
				throw new ArgumentException(string.Format(Messages.IBqlTableMustBeInherited,
					BqlCommand.GetItemType(typeof (TNoteField)).GetLongName()), "TNoteField");
		}

		private void AttacheEventHandlers(PXGraph graph)
		{
			PXDBDefaultAttribute.SetSourceType<CRRelation.refNoteID>(graph.Caches[typeof(CRRelation)], typeof(TNoteField));
			graph.FieldDefaulting.AddHandler(typeof(CRRelation), typeof(CRRelation.refNoteID).Name, CRRelation_RefNoteID_FieldDefaulting);
			graph.RowInserted.AddHandler(typeof(CRRelation), CRRelation_RowInserted);
			graph.RowUpdated.AddHandler(typeof(CRRelation), CRRelation_RowUpdated);
			graph.RowSelected.AddHandler(typeof(CRRelation), CRRelation_RowSelected);

			graph.Initialized +=
				sender =>
					{
						sender.Views.Caches.Remove(typeof(TNoteField));
						if (!sender.Views.Caches.Contains(typeof(CRRelation)))
							sender.Views.Caches.Add(typeof(CRRelation));

						AppendActions(graph);
					};
		}

		private void AppendActions(PXGraph graph)
		{
			var viewName = graph.ViewNames[View];
			var primaryDAC = BqlCommand.GetItemType(typeof(TNoteField));
			PXNamedAction.AddAction(graph, primaryDAC, viewName + "_EntityDetails", null, EntityDetailsHandler);
			PXNamedAction.AddAction(graph, primaryDAC, viewName + "_ContactDetails", null, ContactDetailsHandler);
		}

		private IEnumerable EntityDetailsHandler(PXAdapter adapter)
		{
			var graph = adapter.View.Graph;
			var cache = graph.Caches[typeof(CRRelation)];
			var row = (cache.Current as CRRelation).
				With(_ => _.EntityID).
				With(_ => (BAccount)PXSelect<BAccount, 
					Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(graph, _.Value));
			if (row != null)
				PXRedirectHelper.TryRedirect(graph, row, PXRedirectHelper.WindowMode.NewWindow);
			return adapter.Get();
		}

		private IEnumerable ContactDetailsHandler(PXAdapter adapter)
		{
			var graph = adapter.View.Graph;
			var cache = graph.Caches[typeof(CRRelation)];
			var row = (cache.Current as CRRelation).
				With(_ => _.ContactID).
				With(_ => (Contact)PXSelect<Contact,
					Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
					Select(graph, _.Value));
			if (row != null)
				PXRedirectHelper.TryRedirect(graph, row, PXRedirectHelper.WindowMode.NewWindow);
			return adapter.Get();
		}

		protected virtual void CRRelation_RefNoteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var refCache = sender.Graph.Caches[BqlCommand.GetItemType(typeof(TNoteField))];
			e.NewValue = refCache.GetValue(refCache.Current, typeof(TNoteField).Name);
		}

		protected virtual void CRRelation_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			var row = (CRRelation)e.Row;

			FillRow(sender.Graph, row);
		}

		protected virtual void CRRelation_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = (CRRelation)e.Row;
			var oldRow = (CRRelation)e.OldRow;

			if (row.ContactID == oldRow.ContactID && row.EntityID != oldRow.EntityID)
				row.ContactID = null;

			FillRow(sender.Graph, row);
		}

		protected virtual void CRRelation_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CRRelation;
			if (row == null) return;

			var enableContactID = 
				row.EntityID.
				With(id => (BAccount)PXSelect<BAccount>.
					Search<BAccount.bAccountID>(sender.Graph, id.Value)).
				Return(acct => acct.Type != BAccountType.EmployeeType, 
				true);
			PXUIFieldAttribute.SetEnabled(sender, row, typeof(CRRelation.contactID).Name, enableContactID);
		}

		private static void FillRow(PXGraph graph, CRRelation row)
		{
			var search = row.EntityID.
					With(id => PXSelect<BAccount,
						Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(graph, id.Value));
			string acctType = null;
			if (search == null || search.Count == 0)
			{
				row.Name = null;
				row.EntityCD = null;
			}
			else
			{
				var account = (BAccount)search[0][typeof(BAccount)];
				row.Name = account.AcctName;
				row.EntityCD = account.AcctCD;
				acctType = account.Type;
			}

			Contact contactSearch;
			PXResultset<EPEmployeeSimple> employeeSearch;
			if (acctType == BAccountType.EmployeeType &&
				(employeeSearch = row.EntityID.
					With(eId => PXSelectJoin<EPEmployeeSimple,
					LeftJoin<Contact, On<Contact.contactID, Equal<EPEmployeeSimple.defContactID>>,
					LeftJoin<Users, On<Users.pKID, Equal<EPEmployeeSimple.userID>>>>,
					Where<EPEmployeeSimple.bAccountID, Equal<Required<EPEmployeeSimple.bAccountID>>>>.
					Select(graph, eId.Value))) != null)
			{
				row.ContactName = null;
				var contact = (Contact)employeeSearch[0][typeof(Contact)];
				row.Email = contact.EMail;
				var user = (Users)employeeSearch[0][typeof(Users)];
				if (string.IsNullOrEmpty(row.Name))
					row.Name = user.FullName;
				if (string.IsNullOrEmpty(row.Email))
					row.Email = user.Email;
			}
			else if ((contactSearch = row.ContactID.
						With(cId => (Contact)PXSelect<Contact>.
							Search<Contact.contactID>(graph, cId.Value))) != null)
			{
				row.ContactName = contactSearch.DisplayName;
				row.Email = contactSearch.EMail;
			}
			else
			{
				row.ContactName = null;
				row.Email = null;
			}
		}

		private static PXSelectDelegate GetHandler()
		{
			return () =>
				{
					var command = new Select2<CRRelation,
						LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CRRelation.entityID>>,							
						LeftJoin<Contact,
								  On<Contact.contactID, Equal<Switch<Case<Where<BAccount.type, Equal<BAccountType.employeeType>>,BAccount.defContactID>,CRRelation.contactID>>>,  
						LeftJoin<Users, On<Users.pKID, Equal<Contact.userID>>>>>,
						Where<CRRelation.refNoteID, Equal<Current<TNoteField>>>>();
					var startRow = PXView.StartRow;
					int totalRows = 0;
					var list = new PXView(PXView.CurrentGraph, false, command).
						Select(null, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
						       ref startRow, PXView.MaximumRows, ref totalRows);
					PXView.StartRow = 0;
					foreach (PXResult<CRRelation, BAccount, Contact, Users> row in list)
					{
						var relation = (CRRelation)row[typeof(CRRelation)];
						var account = (BAccount)row[typeof(BAccount)];
						relation.Name = account.AcctName;
						relation.EntityCD = account.AcctCD;
						var contact = (Contact)row[typeof(Contact)];
						if (contact.ContactID == null && relation.ContactID != null &&
						    account.Type != BAccountType.EmployeeType)
						{
							var directContact = (Contact)PXSelect<Contact>.
								                             Search<Contact.contactID>(PXView.CurrentGraph, relation.ContactID);
							if (directContact != null) contact = directContact;
						}
						relation.Email = contact.EMail;
						var user = (Users)row[typeof(Users)];
						if (account.Type != BAccountType.EmployeeType) 
							relation.ContactName = contact.DisplayName;
						else
						{
							if (string.IsNullOrEmpty(relation.Name))
								relation.Name = user.FullName;
							if (string.IsNullOrEmpty(relation.Email))
								relation.Email = user.Email;
						}
					}
					return list;
				};
		}

		public static IEnumerable<Mailbox> GetEmailsForCC(PXGraph graph, long refNoteID)
		{
			var command = new Select2<CRRelation,
							LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CRRelation.entityID>>,
							LeftJoin<Contact,
								On<Contact.contactID, Equal<Switch<Case<Where<BAccount.type, Equal<BAccountType.employeeType>>,BAccount.defContactID>,CRRelation.contactID>>>,
							LeftJoin<Users, On<Users.pKID, Equal<Contact.userID>>>>>,
							Where<CRRelation.refNoteID, Equal<Required<CRRelation.refNoteID>>>>();
			var list = new PXView(graph, false, command).SelectMulti(refNoteID);
			foreach (PXResult<CRRelation, BAccount, Contact, Users> row in list)
			{
				var relation = (CRRelation)row[typeof(CRRelation)];
				if (relation.AddToCC == true)
				{
					var account = (BAccount)row[typeof(BAccount)];
					var name = account.AcctName;
					var contact = (Contact)row[typeof(Contact)];
					if (contact.ContactID == null && relation.ContactID != null &&
						account.Type != BAccountType.EmployeeType)
					{
						var directContact = (Contact)PXSelect<Contact>.
							Search<Contact.contactID>(PXView.CurrentGraph, relation.ContactID);
						if (directContact != null) contact = directContact;
					}
					var email = contact.EMail;
					var user = (Users)row[typeof(Users)];
					if (account.Type != BAccountType.EmployeeType)
						name = contact.DisplayName;
					else
					{
						if (string.IsNullOrEmpty(name))
							name = user.FullName;
						if (string.IsNullOrEmpty(email))
							email = user.Email;
					}
					if (email != null && (email = email.Trim()) != string.Empty)
						yield return new Mailbox(name, email);
				}
			}
		}
	
	}

	#endregion

	#region IAttributeSupport

	public interface IAttributeSupport
	{
		Int32? ID { get; }
		string EntityType { get; }
		String ClassID { get; }
	}

	#endregion

	#region CRSubscriptionsSelect

	public sealed class CRSubscriptionsSelect
	{
		public static IEnumerable Select(PXGraph graph, int? mailListID)
		{
			var startRow = PXView.StartRow;
			int totalRows = 0;			
			var list = Select(graph, mailListID, PXView.Searches, PXView.SortColumns, PXView.Descendings,
								   ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;
			return list;
		}
	
		public static IEnumerable Select(PXGraph graph, int? mailListID, object[] searches, string[] sortColumns, bool[] descendings, ref int startRow, int maxRows, ref int totalRows)
		{
			CRMarketingList list;
			if (mailListID == null ||
				(list = (CRMarketingList)PXSelect<CRMarketingList>.Search<CRMarketingList.marketingListID>(graph, mailListID)) == null)
			{
				return new PXResultset<Contact, BAccount, BAccountParent, Address, State>();
			}

			MergeFilters(graph, mailListID);

			BqlCommand command = new Select2<Contact,
				LeftJoin<BAccount,
					On<BAccount.bAccountID, Equal<Contact.bAccountID>>,
				LeftJoin<BAccountParent,
					On<BAccountParent.bAccountID, Equal<Contact.parentBAccountID>>,
				LeftJoin<Address,
					On<Address.addressID, Equal<Contact.defAddressID>>,
				LeftJoin<State,
					On<State.countryID, Equal<Address.countryID>,
						And<State.stateID, Equal<Address.state>>>>>>>,
				Where<True,Equal<True>>>();

			if (list.NoCall == true)
				command = command.WhereAnd(
					typeof(Where<Contact.noCall, IsNull,
								Or<Contact.noCall, NotEqual<True>>>));
			if (list.NoEMail == true)
				command = command.WhereAnd(
					typeof(Where<Contact.noEMail, IsNull,
								Or<Contact.noEMail, NotEqual<True>>>));
			if (list.NoFax == true)
				command = command.WhereAnd(
					typeof(Where<Contact.noFax, IsNull,
								Or<Contact.noFax, NotEqual<True>>>));
			if (list.NoMail == true)
				command = command.WhereAnd(
					typeof(Where<Contact.noMail, IsNull,
								Or<Contact.noMail, NotEqual<True>>>));
			if (list.NoMarketing == true)
				command = command.WhereAnd(
					typeof(Where<Contact.noMarketing, IsNull,
								Or<Contact.noMarketing, NotEqual<True>>>));
			if (list.NoMassMail == true)
				command = command.WhereAnd(
					typeof(Where<Contact.noMassMail, IsNull,
								Or<Contact.noMassMail, NotEqual<True>>>));

			var view = new PXView(graph, true, command);
			return view.Select(null, null, searches, sortColumns, descendings, PXView.Filters, ref startRow, maxRows, ref totalRows);
		}

		public static void MergeFilters(PXGraph graph, int? mailListID)
		{
			var filters = PXView.Filters; //new PXView.PXFilterRowCollection(new PXFilterRow[]{});
			CRMarketingList list = PXSelect<CRMarketingList,
				Where<CRMarketingList.marketingListID, Equal<Required<CRMarketingList.marketingListID>>>>.
				Select(graph, mailListID);

			long mailListNoteID = PXNoteAttribute.GetNoteID<CRMarketingList.noteID>(graph.Caches[typeof (CRMarketingList)], list);
				
			PXCache targetCache = graph.Caches[typeof(Contact)];
			foreach (CRFixedFilterRow filter in
				PXSelect<CRFixedFilterRow,
					Where<CRFixedFilterRow.refNoteID, Equal<Required<CRFixedFilterRow.refNoteID>>>>.
				Select(graph, mailListNoteID))
			{
				if (filter.IsUsed == true)
				{
					var f = new PXFilterRow
						{
							OpenBrackets = filter.OpenBrackets ?? 0,
							DataField = filter.DataField,
							Condition = (PXCondition) (filter.Condition ?? 0),
							Value = targetCache.ValueFromString(filter.DataField, filter.ValueSt) ?? filter.ValueSt,
							Value2 = targetCache.ValueFromString(filter.DataField, filter.ValueSt2) ?? filter.ValueSt2,
							CloseBrackets = filter.CloseBrackets ?? 0,
							OrOperator = filter.Operator == 1
						};
					filters.Add(f);
				}
			}						
		}
	}

	#endregion

	#region _100Percents

	public sealed class _100Percents : Constant<Decimal>
	{
		public _100Percents() : base(100m) { }
	}

	#endregion

	#region LSServiceCaseItem

	public sealed class LSServiceCaseItem : IN.LSSelect<CRServiceCaseItem, CRServiceCaseItemSplit,
		Where<CRServiceCaseItemSplit.serviceCaseID, Equal<Current<CRServiceCase.serviceCaseID>>>>
	{
		#region Ctor
		public LSServiceCaseItem(PXGraph graph)
			: base(graph)
		{
			MasterQtyField = typeof(CRServiceCaseItem.actualQuantity);
			graph.FieldDefaulting.AddHandler<CRServiceCaseItemSplit.subItemID>(CRServiceCaseItemSplit_SubItemID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<CRServiceCaseItemSplit.locationID>(CRServiceCaseItemSplit_LocationID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<CRServiceCaseItemSplit.invtMult>(CRServiceCaseItemSplit_InvtMult_FieldDefaulting);
			graph.RowSelected.AddHandler<CRServiceCase>(Parent_RowSelected);
			graph.RowUpdated.AddHandler<CRServiceCase>(CRServiceCase_RowUpdated);
		}
		#endregion

		#region Event Handlers
		private void CRServiceCase_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (IsLSEntryEnabled && !sender.ObjectsEqual<CRServiceCase.hold>(e.Row, e.OldRow) && 
				(bool?)sender.GetValue<CRServiceCase.hold>(e.Row) == false)
			{
				var cache = sender.Graph.Caches[typeof(CRServiceCase)];

				foreach (CRServiceCaseItem item in PXParentAttribute.SelectSiblings(cache, null, typeof(CRServiceCase)))
				{
					if (Math.Abs((decimal)item.BaseQty) >= 0.0000005m && item.UnassignedQty >= 0.0000005m)
					{
						cache.RaiseExceptionHandling<CRServiceCaseItem.actualQuantity>(item, item.Qty, new PXSetPropertyException(IN.Messages.BinLotSerialNotAssigned));

						if (cache.GetStatus(item) == PXEntryStatus.Notchanged)
						{
							cache.SetStatus(item, PXEntryStatus.Updated);
						}
					}
				}
			}
		}

		private void Parent_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var isLsEntryEnabled = IsLSEntryEnabled;
			PXUIFieldAttribute.SetRequired<CRServiceCaseItem.locationID>(Cache, isLsEntryEnabled);
			PXUIFieldAttribute.SetVisible<CRServiceCaseItem.locationID>(Cache, null, isLsEntryEnabled);
			PXUIFieldAttribute.SetVisible<CRServiceCaseItem.lotSerialNbr>(Cache, null, isLsEntryEnabled);
			PXUIFieldAttribute.SetVisible<CRServiceCaseItem.expireDate>(Cache, null, isLsEntryEnabled);
		}

		protected override void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				if (IsLSEntryEnabled)
				{
					var cache = sender.Graph.Caches[typeof(CRServiceCase)];
					var doc = PXParentAttribute.SelectParent(sender, e.Row, typeof(CRServiceCase)) ?? cache.Current;

					if ((bool?) cache.GetValue<CRServiceCase.hold>(doc) == false)
						foreach (CRServiceCaseItem item in PXParentAttribute.SelectSiblings(cache, null, typeof(CRServiceCase)))
						{
							if (Math.Abs((decimal)item.BaseQty) >= 0.0000005m && item.UnassignedQty >= 0.0000005m)
							{
								cache.RaiseExceptionHandling<CRServiceCaseItem.actualQuantity>(
									item, item.Qty, new PXSetPropertyException(IN.Messages.BinLotSerialNotAssigned));
							}
						}
				}
			}

			base.Master_RowPersisting(sender, e);
		}

		protected override void Master_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (IsLSEntryEnabled)
			{
				base.Master_RowInserted(sender, e);
			}
			else
			{
				sender.SetValue<CRServiceCaseItem.locationID>(e.Row, null);
				sender.SetValue<CRServiceCaseItem.lotSerialNbr>(e.Row, null);
				sender.SetValue<CRServiceCaseItem.expireDate>(e.Row, null);

				AvailabilityCheck(sender, (CRServiceCaseItem)e.Row);
			}
		}

		protected override void Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (IsLSEntryEnabled)
			{
				base.Master_RowUpdated(sender, e);
			}
			else
			{
				sender.SetValue<CRServiceCaseItem.locationID>(e.Row, null);
				sender.SetValue<CRServiceCaseItem.lotSerialNbr>(e.Row, null);
				sender.SetValue<CRServiceCaseItem.expireDate>(e.Row, null);

				AvailabilityCheck(sender, (CRServiceCaseItem)e.Row);
			}
		}

		protected override void Master_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (IsLSEntryEnabled)
				base.Master_RowDeleted(sender, e);
		}

		public void CRServiceCaseItemSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var operation = e.Operation & PXDBOperation.Command;
			if (e.Row != null && (operation == PXDBOperation.Insert || operation == PXDBOperation.Update))
			{
				var row = (CRServiceCaseItemSplit)e.Row;
				if (row.BaseQty != 0m && row.LocationID == null)
					ThrowFieldIsEmpty<CRServiceCaseItemSplit.locationID>(sender, e.Row);
			}
		}

		public void CRServiceCaseItemSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var cache = sender.Graph.Caches[typeof(CRServiceCaseItem)];
			var current = cache.Current as CRServiceCaseItem;
			if (current != null && (e.Row == null || current.LineNbr == ((CRServiceCaseItemSplit)e.Row).LineNbr))
			{
				e.NewValue = current.SubItemID;
				e.Cancel = true;
			}
		}

		public void CRServiceCaseItemSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var cache = sender.Graph.Caches[typeof(CRServiceCaseItem)];
			var current = cache.Current as CRServiceCaseItem;
			if (current != null && (e.Row == null || current.LineNbr == ((CRServiceCaseItemSplit)e.Row).LineNbr))
			{
				e.NewValue = current.LocationID;
				e.Cancel = true;
			}
		}

		public void CRServiceCaseItemSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(CRServiceCaseItem)];
			var current = cache.Current as CRServiceCaseItem;
			if (current != null && (e.Row == null || current.LineNbr == ((CRServiceCaseItemSplit) e.Row).LineNbr))
				using (InvtMultScope<CRServiceCaseItem> ms = new InvtMultScope<CRServiceCaseItem>(current))
				{
					e.NewValue = current.InvtMult;
					e.Cancel = true;
				}
		}
		#endregion

		protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXSetPropertyException e)
		{
			if (row is CRServiceCaseItem)
			{
				sender.RaiseExceptionHandling<CRServiceCaseItem.actualQuantity>(row, newValue,
					new PXSetPropertyException(e.Message, PXErrorLevel.Warning, 
						sender.GetStateExt<CRServiceCaseItem.inventoryID>(row),
						sender.GetStateExt<CRServiceCaseItem.subItemID>(row),
						sender.GetStateExt<CRServiceCaseItem.siteID>(row),
						sender.GetStateExt<CRServiceCaseItem.locationID>(row),
						sender.GetValue<CRServiceCaseItem.lotSerialNbr>(row)));
			}
			if (row is CRServiceCaseItemSplit)
			{
				sender.RaiseExceptionHandling<CRServiceCaseItemSplit.qty>(row, newValue, 
					new PXSetPropertyException(e.Message, PXErrorLevel.Warning,
						sender.GetStateExt<CRServiceCaseItemSplit.inventoryID>(row),
						sender.GetStateExt<CRServiceCaseItemSplit.subItemID>(row),
						sender.GetStateExt<CRServiceCaseItemSplit.siteID>(row),
						sender.GetStateExt<CRServiceCaseItemSplit.locationID>(row),
						sender.GetValue<CRServiceCaseItemSplit.lotSerialNbr>(row)));
			}
		}

		public override CRServiceCaseItemSplit Convert(CRServiceCaseItem item)
		{
			using (var ms = new InvtMultScope<CRServiceCaseItem>(item))
			{
				return (CRServiceCaseItemSplit)item;
			}
		}

		private static void ThrowFieldIsEmpty<Field>(PXCache sender, object data)
			where Field : IBqlField
		{
			if (sender.RaiseExceptionHandling<Field>(data, null, 
				new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Field).Name)))
			{
				throw new PXRowPersistingException(typeof(Field).Name, null, ErrorMessages.FieldIsEmpty, typeof(Field).Name);
			}
		}

		private bool IsLSEntryEnabled
		{
			get
			{
				var current = _Graph.Caches[typeof(CRServiceCaseItem)].Current as CRServiceCaseItem;
				if (current != null)
				{
					var inventory = (IN.InventoryItem)PXSelect<IN.InventoryItem,
						Where<IN.InventoryItem.inventoryID, Equal<Required<IN.InventoryItem.inventoryID>>>>.
						Select(_Graph, current.InventoryID);
					return inventory != null && inventory.StkItem == true;
				}
				return false;
			}
		}
	}
	#endregion

	#region BqlSetup<Field>

	public class BqlSetup<Field> : IBqlOperand, IBqlCreator
		where Field : IBqlField
	{
		private static readonly Type _table;
		private static readonly MethodInfo _select;

		static BqlSetup()
		{
			_table = BqlCommand.GetItemType(typeof(Field));
			foreach(MethodInfo method in 
				BqlCommand.Compose(typeof(PXSelectReadonly<>), _table).
				GetMethods(BindingFlags.Public | BindingFlags.Static))
			{
				if (method.Name == "SelectWindowed" && !method.IsGenericMethod)
				{
					_select = method;
					break;
				}
			}
		}

		public virtual void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			value = GetValue(cache.Graph);
		}

		public virtual void Parse(PXGraph graph, List<IBqlParameter> pars, List<Type> tables, List<Type> fields, List<IBqlSortColumn> sortColumns, StringBuilder text, BqlCommand.Selection selection)
		{
			if (graph != null && text != null)
			{
				text.Append(PXDatabase.Provider.SqlDialect.enquoteValue(GetValue(graph)));
			}
		}

		private static object GetValue(PXGraph graph)
		{
			object res = _select.Invoke(null, new object[] { graph, 0, 1, null });
			if (res is IPXResultset) res = ((IPXResultset)res).GetItem(0, 0);
			if (res == null) return null;
			var cache = graph.Caches[_table];
			var fieldName = cache.GetField(typeof(Field));
			return cache.GetValue(res, fieldName);
		}
	}

	#endregion

	#region CSAttributeSelector<TAnswer, TEntityId, TEntityType, TEntityClassId>

	public class CSAttributeSelector<TAnswer, TEntityId, TEntityType, TEntityClassId> 
		: PXSelect<TAnswer>
		where TAnswer : class, IBqlTable, new()
	{
		public CSAttributeSelector(PXGraph graph) : base(graph)
		{
			Intialize();
		}

		public CSAttributeSelector(PXGraph graph, Delegate handler) : base(graph, handler)
		{
			Intialize();
		}

		private void Intialize()
		{
			var graph = View.Graph;
			var isReadonly = View.IsReadOnly;
			var command = CreateCommand();
			View = new PXView(graph, isReadonly, command);
		}

		private BqlCommand CreateCommand()
		{
			var cache = _Graph.Caches[typeof(TAnswer)];
			var attIdField = cache.GetBqlField(typeof(CSAnswers.attributeID).Name);
			var entityIdField = cache.GetBqlField(typeof(CSAnswers.entityID).Name);
			var entityTypeField = cache.GetBqlField(typeof(CSAnswers.entityType).Name);
			var res = BqlCommand.CreateInstance(
				typeof(Select2<,,,>), typeof(TAnswer), 
				typeof(InnerJoin<,,>), typeof(CSAttributeGroup),
					typeof(On<,>), typeof(CSAttributeGroup.attributeID), typeof(Equal<>), attIdField, 
				typeof(LeftJoin<,>), typeof(CSAttribute),
					typeof(On<,>), typeof(CSAttribute.attributeID), typeof(Equal<>), attIdField,
				typeof(Where<,,>), entityIdField, typeof(Equal<>), typeof(TEntityId),
					typeof(And<,,>), entityTypeField, typeof(Equal<>), typeof(TEntityType),
					typeof(And<,,>), typeof(CSAttributeGroup.entityClassID), typeof(Equal<>), typeof(TEntityClassId),
					typeof(And<,>), typeof(CSAttributeGroup.type), typeof(Equal<>), typeof(TEntityType),
				typeof(OrderBy<Asc<CSAttributeGroup.sortOrder>>));
			res = CorrectCommand(res);
			return res;
		}

		protected virtual BqlCommand CorrectCommand(BqlCommand command)
		{
			return command;
		}
	}

	#endregion

	#region CSAttributeSelector
	public class CSAttributeSelector<TAnswer, TEntityId, TEntityType, TEntityClassId, TJoin, TWhere>
		: CSAttributeSelector<TAnswer, TEntityId, TEntityType, TEntityClassId>
		where TAnswer: class, IBqlTable, new()
	{
		public CSAttributeSelector(PXGraph graph) : base(graph)
		{
			
		}

		public CSAttributeSelector(PXGraph graph, Delegate handler) : base(graph, handler)
		{
		}

		protected override BqlCommand CorrectCommand(BqlCommand command)
		{
			var res = base.CorrectCommand(command);
			res = BqlCommand.CreateInstance(BqlCommand.AppendJoin(res.GetType(), typeof(TJoin)));
			res = res.WhereAnd(typeof(TWhere));
			return res;
		}
	}
	#endregion

	#region CRDBTimeSpanAttribute

	public sealed class CRDBTimeSpanAttribute : PXDBTimeSpanAttribute
	{
		private const string _FIELD_POSTFIX = "_byString";
		private const string _INPUTMASK = "##### hrs ## mins";
		private const int _SHARPS_COUNT = 7;
		private const string _DEFAULT_VALUE = "      0";

		private string _byStringFieldName;

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_byStringFieldName = _FieldName + _FIELD_POSTFIX;
			sender.Fields.Add(_byStringFieldName);
			sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), _byStringFieldName, _FieldName_byString_FieldSelecting);
		}

		private void _FieldName_byString_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs args)
		{
			var mainState = sender.GetStateExt(args.Row, _FieldName) as PXFieldState;
			var displayName = mainState.With(_ => _.DisplayName);
			var visible = mainState.With(_ => _.Visible);
			var val = sender.GetValue(args.Row, _FieldOrdinal);
			var valStr = _DEFAULT_VALUE;
			if (val != null)
			{
				var ts = TimeSpan.FromMinutes((int)val);
				valStr = ((int)ts.TotalHours).ToString() + ((int)ts.Minutes).ToString("00");
				if (valStr.Length < _SHARPS_COUNT)
					valStr = new string(' ', _SHARPS_COUNT - valStr.Length) + valStr;
			}

			var newState = PXStringState.CreateInstance(valStr, null, false, _byStringFieldName, false, null,
															_INPUTMASK, null, null, null, null);
			newState.Enabled = false;
			newState.Visible = visible;
			newState.DisplayName = displayName;

			args.ReturnState = newState;
		}
	}

	#endregion

	#region CRTimeSpanAttribute

	public sealed class CRTimeSpanAttribute : PXTimeSpanAttribute
	{
		private const string _FIELD_POSTFIX = "_byString";
		private const string _TIME_FIELD_POSTFIX = "_byTimeString";
		private const string _INPUTMASK = "##### hrs ## mins";
		private const int _SHARPS_COUNT = 7;
		private const string _DEFAULT_VALUE = "      0";

		private string _byStringFieldName;
		private string _byTimeStringFieldName;

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_byStringFieldName = _FieldName + _FIELD_POSTFIX;
			sender.Fields.Add(_byStringFieldName);
			sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), _byStringFieldName, _FieldName_byString_FieldSelecting);

			_byTimeStringFieldName = _FieldName + _TIME_FIELD_POSTFIX;
			sender.Fields.Add(_byTimeStringFieldName);
			sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), _byTimeStringFieldName, _FieldName_byTimeString_FieldSelecting);
		}

		private void _FieldName_byString_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs args)
		{
			var mainState = sender.GetStateExt(args.Row, _FieldName) as PXFieldState;
			var displayName = mainState.With(_ => _.DisplayName);
			var visible = mainState.With(_ => _.Visible);
			var val = sender.GetValue(args.Row, _FieldOrdinal);
			var valStr = _DEFAULT_VALUE;
			if (val != null)
			{
				var ts = TimeSpan.FromMinutes((int)val);
				valStr = ((int)ts.TotalHours).ToString() + ((int)ts.Minutes).ToString("00");
				if (valStr.Length < _SHARPS_COUNT)
					valStr = new string(' ', _SHARPS_COUNT - valStr.Length) + valStr;
			}

			var newState = PXStringState.CreateInstance(valStr, null, false, _byStringFieldName, false, null,
															_INPUTMASK, null, null, null, null);
			newState.Enabled = false;
			newState.Visible = visible;
			newState.DisplayName = displayName;

			args.ReturnState = newState;
		}

		private void _FieldName_byTimeString_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs args)
		{
			var mainState = sender.GetStateExt(args.Row, _FieldName) as PXFieldState;
			var displayName = mainState.With(_ => _.DisplayName);
			var visible = mainState.With(_ => _.Visible);
			var val = sender.GetValue(args.Row, _FieldOrdinal);
			var valStr = string.Empty;
			if (val != null)
			{
				var ts = TimeSpan.FromMinutes((int)val);
				valStr = string.Format("{0:00}:{1:00}", (int)ts.TotalHours, ts.Minutes);
			}

			var newState = PXStringState.CreateInstance(valStr, null, false, _byTimeStringFieldName, false, null,
															null, null, null, null, null);
			newState.Enabled = false;
			newState.Visible = visible;
			newState.DisplayName = displayName;

			args.ReturnState = newState;
		}
	}

	#endregion

	#region CRCaseActivityHelper<TableRefNoteID>

	public class CRCaseActivityHelper
	{
		#region Ctor

		public static CRCaseActivityHelper Attach(PXGraph graph)
		{
			var res = new CRCaseActivityHelper();

			graph.RowInserted.AddHandler<EPActivity>(res.ActivityRowInserted);
			graph.RowSelected.AddHandler<EPActivity>(res.ActivityRowSelected);
			graph.RowUpdated.AddHandler<EPActivity>(res.ActivityRowUpdated);

			return res;
		}
		#endregion

		#region Event Handlers

		protected virtual void ActivityRowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			var item = e.Row as EPActivity;
			if (item == null) return;

			var graph = sender.Graph;
			var @case = GetCase(graph, item.RefNoteID) ?? GetCase(graph, item.ParentRefNoteID);
			if (@case != null && @case.Released == true) item.IsBillable = false;
		}

		protected virtual void ActivityRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var item = e.Row as EPActivity;
			var oldItem = e.OldRow as EPActivity;
			if (item == null || oldItem == null) return;

			var graph = sender.Graph;
			var @case = GetCase(graph, item.RefNoteID) ?? GetCase(graph, item.ParentRefNoteID);
			if (@case != null && @case.Released == true) item.IsBillable = false;
		}

		protected virtual void ActivityRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var item = e.Row as EPActivity;
			if (item == null || !string.IsNullOrEmpty(item.TimeCardCD)) return;

			var graph = sender.Graph;
			var @case = GetCase(graph, item.RefNoteID) ?? GetCase(graph, item.ParentRefNoteID);			
			if(@case != null && @case.Released == true )
				PXUIFieldAttribute.SetEnabled<EPActivity.isBillable>(sender, item, false);
		}

		#endregion

		#region Private Methods

		private CRCase GetCase(PXGraph graph, object refNoteID)
		{
			if (refNoteID == null) return null;
			return (CRCase)PXSelect<CRCase,
				Where<CRCase.noteID, Equal<Required<CRCase.noteID>>>>.
				SelectWindowed(graph, 0, 1, refNoteID);
		}

		#endregion
	}

	#endregion

	#region CRAttributeList

	public class CRAttributeList<TReference> : PXSelectBase<CSAnswers>
		where TReference : IBqlTable
	{
		private readonly HybridDictionary _ids = new HybridDictionary();

		public CRAttributeList(PXGraph graph)
		{
			_Graph = graph;
			View = new PXView(graph, false, 
				new Select3<CSAnswers, OrderBy<Asc<CSAnswers.order>>>(), 
				new PXSelectDelegate(SelectDelegate));

			_Graph.EnshureCachePersistance(typeof(CSAnswers));
			PXDBAttributeAttribute.Activate(_Graph.Caches[typeof(TReference)]);
			_Graph.FieldSelecting.AddHandler(typeof(CSAnswers), typeof(CSAnswers.value).Name, FieldSelectingHandler);
			_Graph.FieldSelecting.AddHandler(typeof(CSAnswers), typeof(CSAnswers.isRequired).Name, IsRequiredSelectingHandler);
			_Graph.FieldSelecting.AddHandler(typeof(CSAnswers), typeof(CSAnswers.attributeID).Name, AttrFieldSelectingHandler);
			_Graph.RowPersisting.AddHandler(typeof(CSAnswers), RowPersistingHandler);
			_Graph.RowPersisting.AddHandler(typeof(TReference), ReferenceRowPersistingHandler);
			_Graph.RowUpdating.AddHandler(typeof(TReference), ReferenceRowUpdatingHandler);
		}

		virtual protected IEnumerable SelectDelegate()
		{
			var row = GetCurrentRow();
			foreach (PXResult<CSAnswers, CSAttribute, CSAttributeGroup> item in SelecteInternal(row))
			{
				CSAttribute ag = item[typeof(CSAttribute)] as CSAttribute;
				CSAnswers ca = item[typeof(CSAnswers)] as CSAnswers;
				if (PXSiteMap.IsPortal)
				{
					if (ag != null)
					{
						if (ag.IsInternal != true)
						{
							yield return ca;
						}
					}
				}
				else
				{
					yield return ca;
				}
			}
		}

		protected virtual string GetDefaultAnswerValue(CSAttributeGroup group)
		{
			return group.DefaultValue;
		}

		protected IEnumerable<PXResult<CSAnswers, CSAttribute, CSAttributeGroup>> SelecteInternal(IAttributeSupport row)
		{
			if (row == null) return new PXResult<CSAnswers, CSAttribute, CSAttributeGroup>[0];

			var cache = _Graph.Caches[typeof(CSAnswers)];
			var map = new HybridDictionary();
			

			if (row.ID != null)
				foreach (CSAnswers answer in
					PXSelect<CSAnswers,
								Where<CSAnswers.entityID, Equal<Required<CSAnswers.entityID>>,
									And<CSAnswers.entityType, Equal<Required<CSAnswers.entityType>>>>>.Select(_Graph, row.ID, row.EntityType))
				{

					map[answer.AttributeID] = new PXResult<CSAnswers, CSAttribute, CSAttributeGroup>(answer, new CSAttribute(), new CSAttributeGroup());
				}

			short maxOrder = 0;
			var cacheOldDirty = cache.IsDirty;
			var mainAttributes = new List<string>();
			if (row.ClassID != null)
				foreach (PXResult<CSAttributeGroup, CSAttribute> record in
					PXSelectJoin<CSAttributeGroup,
					LeftJoin<CSAttribute, On<CSAttribute.attributeID, Equal<CSAttributeGroup.attributeID>>>, 
					Where<CSAttributeGroup.entityClassID, Equal<Required<CSAttributeGroup.entityClassID>>,
						And<CSAttributeGroup.type, Equal<Required<CSAttributeGroup.type>>>>>.
					Select(_Graph, row.ClassID, row.EntityType))
				{
					var group = (CSAttributeGroup)record;
					var att = (CSAttribute)record;
					mainAttributes.Add(att.AttributeID);
					if (map.Contains(group.AttributeID))
					{
						PXResult<CSAnswers, CSAttribute, CSAttributeGroup> currentres = map[@group.AttributeID] as PXResult<CSAnswers, CSAttribute, CSAttributeGroup>;
						var answer = currentres[typeof(CSAnswers)] as CSAnswers;
						answer.Order = group.SortOrder;
						if (PXSiteMap.IsPortal && att.IsInternal != null && group.Required != null && (bool)att.IsInternal && (bool)group.Required)
							answer.IsRequired = false;
						else
							answer.IsRequired = group.With(_ => _.Required);
						map[group.AttributeID] = new PXResult<CSAnswers, CSAttribute, CSAttributeGroup>(answer, att, group);
					}
					else
					{
						var answer = (CSAnswers)cache.CreateInstance();
						answer.AttributeID = group.AttributeID;
						answer.EntityID = row.ID;
						answer.EntityType = row.EntityType;
						answer.Order = group.SortOrder;
						answer.Value = GetDefaultAnswerValue(group);
						if (PXSiteMap.IsPortal && att.IsInternal != null && group.Required != null && (bool)att.IsInternal && (bool)group.Required)
							answer.IsRequired = false;
						else
							answer.IsRequired = group.With(_ => _.Required);
						answer = (CSAnswers)(cache.Insert(answer) ?? cache.Locate(answer));
						map[group.AttributeID] = new PXResult<CSAnswers, CSAttribute, CSAttributeGroup>(answer, att, group);
					}

					if (group.SortOrder != null)
						maxOrder = Math.Max((short)group.SortOrder, maxOrder);
				}

			var keys = new object[map.Count];
			map.Keys.CopyTo(keys, 0);
			foreach (object key in keys)
			{
				if (mainAttributes.Contains((string)key)) continue;
				
			    CSAnswers att = PXResult.Unwrap<CSAnswers>(map[key]);
				if (string.IsNullOrEmpty(att.Value))
				{
					map.Remove(key);
					cache.Delete(att);
				}
				else
				{
					att.IsRequired = false;
				}
			}
			cache.IsDirty = cacheOldDirty;

			foreach (DictionaryEntry item in map)
			{
				PXResult<CSAnswers, CSAttribute, CSAttributeGroup> currentres = item.Value as PXResult<CSAnswers, CSAttribute, CSAttributeGroup>;
				var answer = currentres[typeof(CSAnswers)] as CSAnswers;
				if (answer == null || answer.Order != null) continue;

				maxOrder++;
				answer.Order = maxOrder;
			}

			List<PXResult<CSAnswers, CSAttribute, CSAttributeGroup>> ret = new List<PXResult<CSAnswers, CSAttribute, CSAttributeGroup>>();
			foreach (DictionaryEntry item in map)
			{
				ret.Add(item.Value as PXResult<CSAnswers, CSAttribute, CSAttributeGroup>);
			}
			return ret;
		}

		private void FieldSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var row = e.Row as CSAnswers;
			if (row == null) return;
			
			var question = (CSAttribute)PXSelect<CSAttribute,
				Where<CSAttribute.attributeID, Equal<Required<CSAttribute.attributeID>>>>.
				Select(_Graph, row.AttributeID);

			PXResultset<CSAttributeDetail> options = PXSelect<CSAttributeDetail,
				Where<CSAttributeDetail.attributeID, Equal<Required<CSAnswers.attributeID>>>,
				OrderBy<Asc<CSAttributeDetail.sortOrder>>>.
				Select(_Graph, row.AttributeID);

			int required = -1;
			if (question != null && row.IsRequired == true)
				required = 1;

			if (options.Count > 0)
			{
				//ComboBox:
				var allowedValues = new List<string>();
				var allowedLabels = new List<string>();

				foreach (CSAttributeDetail option in options)
				{
					if ((bool)option.Disabled && row.Value != option.ValueID) continue;
					allowedValues.Add(option.ValueID);   
					allowedLabels.Add(option.Description);
				}

				string mask = question != null ? question.EntryMask : null;

				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, CSAttributeDetail.ParameterIdLength,
					true, typeof(CSAnswers.value).Name, false, required, mask, allowedValues.ToArray(), allowedLabels.ToArray(), true, null);
				if (question.ControlType == CSAttribute.MultiSelectCombo)
				{
					((PXStringState)e.ReturnState).MultiSelect = true;
				}
			}
			else if (question != null)
			{
				if (question.ControlType.Value == CSAttribute.CheckBox)
				{
					e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(bool), false, false, required,
						null, null, false, typeof(CSAnswers.value).Name, null, null, null, PXErrorLevel.Undefined, true, true, null, 
						PXUIVisibility.Visible, null, null, null);
				}
				else if (question.ControlType.Value == CSAttribute.Datetime)
				{
					string mask = question != null ? question.EntryMask : null;
					e.ReturnState = PXDateState.CreateInstance(e.ReturnState, typeof(CSAnswers.value).Name, false, required, mask, mask, 
						null, null);
				}
				else
				{
					//TextBox:
					string mask = question != null ? question.EntryMask : null;

					const int answerValueLength = 60;//TODO: need review
					e.ReturnState = PXStringState.CreateInstance(e.ReturnState, answerValueLength, null, typeof(CSAnswers.value).Name, 
						false, required, mask, null, null, true, null);
				}
			}
			if (e.ReturnState != null)
			{
				var error = PXUIFieldAttribute.GetError<CSAnswers.value>(sender, row);
				if (error != null)
				{
					var state = (PXFieldState) e.ReturnState;
					state.Error = error;
					state.ErrorLevel = PXErrorLevel.RowError;
				}
			}
		}

		private void AttrFieldSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled<CSAnswers.attributeID>(sender, e.Row, false);
		}

		private void IsRequiredSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var row = e.Row as CSAnswers;
			if (row == null || e.ReturnValue != null) return;
			CSAttributeGroup attrGoup = PXSelect<
				CSAttributeGroup
				, Where<CSAttributeGroup.type, Equal<Current<CSAnswers.entityType>>
						, And<CSAttributeGroup.attributeID, Equal<Current<CSAnswers.attributeID>>>>>.SelectSingleBound(_Graph, new object[] {row});
			if (attrGoup != null)
				e.ReturnValue = attrGoup.Required;
		}

		private void ReferenceRowPersistingHandler(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = e.Row as IAttributeSupport;
			if (row == null || row.ID == null) return;

			_ids[row.ID.ToString() + row.EntityType.ToString()] = e.Row;

			PXCache answersCache = _Graph.Caches[typeof(CSAnswers)];

			// remove dublicate
			List<CSAnswers> listAnswers = new List<CSAnswers>();
			List<CSAnswers> deleteAnswers = new List<CSAnswers>();
			List<CSAnswers> insertAnswers = new List<CSAnswers>();
			foreach (CSAnswers ans in SelecteInternal(row))
			{
				listAnswers.Add(ans);
			}
			foreach (CSAnswers ans in listAnswers)
			{
				if (ans.Value == null)
				{
					foreach (CSAnswers ans1 in answersCache.Cached)
					{
						if (ans.AttributeID == ans1.AttributeID  &&
							ans1.Value != null)
						{
							if (ans.EntityType == ans1.EntityType)
							{
								deleteAnswers.Add(ans);
								insertAnswers.Add(ans1);
							}
							else
							{
								ans.Value = ans1.Value;
							}
						}
					}
				}
			}
			foreach (CSAnswers ans in deleteAnswers)
			{
				listAnswers.Remove(ans);
			}
			foreach (CSAnswers ans in insertAnswers)
			{
				listAnswers.Add(ans);
			}

			List<string> emptyRequired = new List<string>();
			foreach (CSAnswers answer in listAnswers)
			{
				if (e.Operation == PXDBOperation.Delete)
				{
					answersCache.Delete(answer);
				}
				else if (string.IsNullOrEmpty(answer.Value) && answer.IsRequired == true && !_Graph.UnattendedMode)
				{
				    string displayName = "";					
                    if (sender.GetStateExt(null, string.Format("{0}_Attributes", answer.AttributeID)) != null)
				    {
                        displayName = sender.GetStateExt(null, string.Format("{0}_Attributes", answer.AttributeID)).With(_ => _ as PXFieldState).With(_ => _.DisplayName).Replace("$Attributes$-", "");
				    }
				    emptyRequired.Add(displayName);
					var mayNotBeEmpty = string.Format(ErrorMessages.FieldIsEmpty, displayName);
					answersCache.RaiseExceptionHandling<CSAnswers.value>(answer, answer.Value, new PXSetPropertyException(mayNotBeEmpty, PXErrorLevel.RowError, typeof(CSAnswers.value).Name));
					PXUIFieldAttribute.SetError<CSAnswers.value>(answersCache, answer, mayNotBeEmpty);
				}
			}
			if (emptyRequired.Count > 0)
				throw new PXException(Messages.RequiredAttributesAreEmpty, string.Join(", ", emptyRequired.Select(s => string.Format("'{0}'", s))));
		}

		private void ReferenceRowUpdatingHandler(PXCache sender, PXRowUpdatingEventArgs e)
		{
			IAttributeSupport row = (IAttributeSupport)e.Row;
			IAttributeSupport newRow = (IAttributeSupport)e.NewRow;

      if (row == null || newRow == null || string.Equals(row.ClassID, newRow.ClassID, StringComparison.InvariantCultureIgnoreCase))		
				return;			

			var newAttrList = new HashSet<string>();
			if (newRow.ClassID != null)
				foreach (PXResult<CSAttributeGroup, CSAttribute> record in
					PXSelectJoin<CSAttributeGroup,
						LeftJoin<CSAttribute, On<CSAttribute.attributeID, Equal<CSAttributeGroup.attributeID>>>,
						Where<CSAttributeGroup.entityClassID, Equal<Required<CSAttributeGroup.entityClassID>>,
							And<CSAttributeGroup.type, Equal<Required<CSAttributeGroup.type>>>>>.
						Select(_Graph, newRow.ClassID, newRow.EntityType))
				{
					CSAttribute attr = record;
					if (attr.AttributeID != null)
						newAttrList.Add(attr.AttributeID);
				}

			foreach (CSAnswers answersRow in
				PXSelect<CSAnswers, 
				Where<CSAnswers.entityID, Equal<Required<CSAnswers.entityID>>, 
				And<CSAnswers.entityType, Equal<Required<CSAnswers.entityType>>>>>
				.SelectMultiBound(sender.Graph, null, row.ID, row.EntityType))
			{
				CSAnswers copy = PXCache <CSAnswers>.CreateCopy(answersRow);
				View.Cache.Delete(answersRow);
				if (newAttrList.Contains(copy.AttributeID))
				{
					View.Cache.Insert(copy);
				}
			}
			sender.IsDirty = true;
		}

		private void RowPersistingHandler(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation != PXDBOperation.Insert && e.Operation != PXDBOperation.Update) return;

			var row = e.Row as CSAnswers;
			if (row == null) return;

			if (row.EntityID != null && _ids.Contains(row.EntityID.ToString() + row.EntityType.ToString())
				&& ((IAttributeSupport)_ids[row.EntityID.ToString() + row.EntityType.ToString()]).EntityType == row.EntityType)
			{
				var primaryRecord = _ids[row.EntityID.ToString() + row.EntityType.ToString()] as IAttributeSupport;
				if (primaryRecord != null)
				{
					foreach (CSAnswers rows in sender.Inserted)
					{
						if (rows.EntityID == primaryRecord.ID && rows.AttributeID == row.AttributeID && rows.EntityType == row.EntityType)
						{
							e.Cancel = true;
							if (row.Value != null && rows.Value == null )
								rows.Value = row.Value; 
							sender.Delete(row);
							break;
						}
					}
					row.EntityID = primaryRecord.ID;
				}
			}
			if (row.EntityID == null || row.EntityType == null)
			{
				e.Cancel = true;
				sender.Delete(row);
			}
			else if (string.IsNullOrEmpty(row.Value))
			{
				var mayNotBeEmpty = string.Format(ErrorMessages.FieldIsEmpty, sender.GetStateExt<CSAnswers.value>(null).With(_ => _ as PXFieldState).With(_ => _.DisplayName));
				if (row.IsRequired == true &&
					sender.RaiseExceptionHandling<CSAnswers.value>(e.Row, row.Value, new PXSetPropertyException(mayNotBeEmpty, PXErrorLevel.RowError, typeof(CSAnswers.value).Name)))
				{
					throw new PXRowPersistingException(typeof(CSAnswers.value).Name, row.Value, mayNotBeEmpty, typeof(CSAnswers.value).Name);
				}
				e.Cancel = true;
				sender.Delete(row);
			}
		}

		protected IAttributeSupport GetCurrentRow()
		{
			return _Graph.Caches[typeof(TReference)].Current as IAttributeSupport;
		}

		public void CopyAllAttributes(IAttributeSupport row, IAttributeSupport src)
		{
			CopyAttributes(row, src, true);
		}

		public void CopyAttributes(IAttributeSupport row, IAttributeSupport src)
		{
			CopyAttributes(row, src, false);
		}

		private void CopyAttributes(IAttributeSupport row, IAttributeSupport src, bool copyall)
		{
			if (row == null || src == null) return;

			List<CSAnswers> sources = SelecteInternal(src).RowCast<CSAnswers>().ToList();
			List<CSAnswers> dests = SelecteInternal(row).RowCast<CSAnswers>().ToList();

			foreach (var res in dests.Join(sources, d => d.AttributeID, s => s.AttributeID, (d, s) => new {Dst = d, Src = s }))
			{
				if ((copyall || string.IsNullOrEmpty(res.Dst.Value)) && res.Src != null && !string.IsNullOrEmpty(res.Src.Value))
				{
					CSAnswers answer = PXCache<CSAnswers>.CreateCopy(res.Dst);
					answer.Value = res.Src.Value;
					_Graph.Caches<CSAnswers>().Update(answer);
				}
			}
		}
	}

	#endregion
	#region CRAttributeSourceList

	public class CRAttributeSourceList<TReference, TSourceField> : CRAttributeList<TReference>
		where TReference : class, IBqlTable, new() 
		where TSourceField : IBqlField
	{
		public CRAttributeSourceList(PXGraph graph) : base(graph)
		{
			_Graph.FieldUpdated.AddHandler<TSourceField>(ReferenceSourceFieldUpdated);
		}

		private IAttributeSupport _AttributeSource = null;
		protected  IAttributeSupport AttributeSource
		{
			get 
			{ 
				PXCache<TReference> cache = _Graph.Caches<TReference>();
				if (_AttributeSource == null || _AttributeSource.ID != (int?) cache.GetValue<TSourceField>(cache.Current))
				{
					_AttributeSource = PXSelectorAttribute.Select<TSourceField>(cache, cache.Current) as IAttributeSupport;
				}
				return _AttributeSource;
			}
		}

		protected override string GetDefaultAnswerValue(CSAttributeGroup group)
		{
			if (AttributeSource != null)
			{
				CSAnswers answer = PXSelect<CSAnswers,
					Where<CSAnswers.entityType, Equal<Required<CSAnswers.entityType>>, 
						And<CSAnswers.entityID, Equal<Required<CSAnswers.entityID>>,
						And<CSAnswers.attributeID, Equal<Required<CSAttributeGroup.attributeID>>>>>>.Select(_Graph, AttributeSource.EntityType, AttributeSource.ID, group.AttributeID);
				if (answer != null && !string.IsNullOrEmpty(answer.Value))
					return answer.Value;
			}
			return base.GetDefaultAnswerValue(group);
		}

		protected void ReferenceSourceFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CopyAttributes(e.Row as IAttributeSupport, AttributeSource);
		}

	}
	#endregion

	#region AddressSelectBase

	public abstract class AddressSelectBase : PXSelectBase, ICacheType<Address>
	{
		private const string _BUTTON_ACTION = "ValidateAddress"; //TODO: need concat with graph selector name;
		private const string _VIEWONMAP_ACTION = "ViewOnMap";

		protected Type _itemType;

		protected int _addressIdFieldOrdinal;
		protected int _asMainFieldOrdinal;
		private int _accountIdFieldOrdinal;

		private PXAction _action;
		private PXAction _mapAction;

		protected AddressSelectBase(PXGraph graph)
		{
			Initialize(graph);
			CreateView(graph);
			AttacheHandlers(graph);
			AppendButton(graph);
		}

		public bool DoNotCorrectUI { get; set; }

		private void AppendButton(PXGraph graph)
		{
			_action = PXNamedAction.AddAction(graph, _itemType, _BUTTON_ACTION, CS.Messages.ValidateAddress, CS.Messages.ValidateAddress, ValidateAddress);
			_mapAction = PXNamedAction.AddAction(graph, _itemType, _VIEWONMAP_ACTION, Messages.ViewOnMap, ViewOnMap);
		}

		private void Initialize(PXGraph graph)
		{
			_Graph = graph;
			_Graph.EnshureCachePersistance(typeof(Address));
			_Graph.Initialized += sender => sender.Views.Caches.Remove(IncorrectPersistableDAC);

			var addressIdDAC = GetDAC(AddressIdField);
			var asMainDAC = GetDAC(AsMainField);
			var accounDAC = GetDAC(AccountIdField);
			if (addressIdDAC != asMainDAC || asMainDAC != accounDAC)
				throw new Exception(string.Format("Fields '{0}', '{1}' and '{2}' are defined in different DACs",
					addressIdDAC.Name, asMainDAC.Name, accounDAC));
			_itemType = addressIdDAC;

			var cache = _Graph.Caches[_itemType];
			_addressIdFieldOrdinal = cache.GetFieldOrdinal(AddressIdField.Name);
			_asMainFieldOrdinal = cache.GetFieldOrdinal(AsMainField.Name);
			_accountIdFieldOrdinal = cache.GetFieldOrdinal(AccountIdField.Name);
		}

		protected abstract Type AccountIdField { get; }

		protected abstract Type AsMainField { get; }

		protected abstract Type AddressIdField { get; }

		protected abstract Type IncorrectPersistableDAC { get; }

		private static Type GetDAC(Type type)
		{
			var res = type.DeclaringType;
			if (res == null)
				throw new Exception(string.Format("DAC for field '{0}' can not be found", type.Name));
			return res;
		}

		private void CreateView(PXGraph graph)
		{
			var command = new Select<Address>();
			View = new PXView(graph, false, command, new PXSelectDelegate(SelectDelegate));
		}

		private void AttacheHandlers(PXGraph graph)
		{
			graph.RowInserted.AddHandler(_itemType, RowInsertedHandler);
			graph.RowUpdating.AddHandler(_itemType, RowUpdatingHandler);
			graph.RowUpdated.AddHandler(_itemType, RowUpdatedHandler);
			graph.RowSelected.AddHandler(_itemType, RowSelectedHandler);
			graph.RowDeleted.AddHandler(_itemType, RowDeletedHandler);
		}

		private void RowDeletedHandler(PXCache sender, PXRowDeletedEventArgs e)
		{
			var currentAddressId = sender.GetValue(e.Row, _addressIdFieldOrdinal);
			var isMainAddress = sender.GetValue(e.Row, AsMainField.Name) as bool?;
			if (isMainAddress == true) return;

			var currentAddress = currentAddressId.
				With(_ => (Address)PXSelect<Address,
					Where<Address.addressID, Equal<Required<Address.addressID>>>>.
				Select(sender.Graph, _));
			if (currentAddress != null)
			{
				var addressCache = sender.Graph.Caches[typeof(Address)];
				addressCache.Delete(currentAddress);
			}
		}

		private void RowSelectedHandler(PXCache sender, PXRowSelectedEventArgs e)
		{
			var addressCache = sender.Graph.Caches[typeof(Address)];
			var asMain = false;
			var isValidated = false;

			var accountId = sender.GetValue(e.Row, _accountIdFieldOrdinal);
			var account = accountId.
					With(_ => (BAccount)PXSelect<BAccount,
						Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(_Graph, _));
			var accountAddressId = account.With(_ => _.DefAddressID);
			var containsAccount = accountAddressId != null;

			var currentAddressId = sender.GetValue(e.Row, _addressIdFieldOrdinal);
			var currentAddress = currentAddressId.
				With(_ => (Address)PXSelect<Address,
					Where<Address.addressID, Equal<Required<Address.addressID>>>>.
				Select(sender.Graph, _));
			if (currentAddress != null)
			{
				isValidated = currentAddress.IsValidated == true;

				if (currentAddressId.Equals(accountAddressId))
					asMain = true;
			}
			else
			{
				PXEntryStatus status = sender.GetStatus(e.Row);
				if (status != PXEntryStatus.Inserted && status != PXEntryStatus.Deleted && status != PXEntryStatus.InsertedDeleted)
				{
					sender.SetValue(e.Row, _addressIdFieldOrdinal, null);
					RowInsertedHandler(sender, new PXRowInsertedEventArgs(e.Row, true));
					sender.SetStatus(e.Row, PXEntryStatus.Updated);
				}
			}

			sender.SetValue(e.Row, _asMainFieldOrdinal, asMain);
			if (!DoNotCorrectUI)
			{
				PXUIFieldAttribute.SetEnabled(addressCache, currentAddress, !asMain);
				PXUIFieldAttribute.SetEnabled(sender, e.Row, AsMainField.Name, containsAccount);
			}
			_action.SetEnabled(!isValidated);
		}

		protected virtual IEnumerable SelectDelegate()
		{
			var primaryCache = _Graph.Caches[_itemType];
			var primaryRecord = GetPrimaryRow();
			var currentAddressId = primaryCache.GetValue(primaryRecord, _addressIdFieldOrdinal);

			yield return (Address)PXSelect<Address,
				Where<Address.addressID, Equal<Required<Address.addressID>>>>.
				Select(_Graph, currentAddressId);
		}

		protected Type ItemType
		{
			get { return _itemType; }
		}

		protected abstract object GetPrimaryRow();

		private void RowInsertedHandler(PXCache sender, PXRowInsertedEventArgs e)
		{
			var row = e.Row;
			var asMain = sender.GetValue(row, _asMainFieldOrdinal);
			var accountId = sender.GetValue(row, _accountIdFieldOrdinal);
			var account = accountId.
				With(_ => (BAccount)PXSelect<BAccount,
					Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Select(_Graph, _));
			var accountAddressId = account.With(_ => _.DefAddressID);
			var accountAddress = accountAddressId.
				With<int?, Address>(_ => (Address)PXSelect<Address,
					Where<Address.addressID, Equal<Required<Address.addressID>>>>.
				Select(sender.Graph, _));
			var currentAddressId = sender.GetValue(row, _addressIdFieldOrdinal);

			if (accountAddress == null)
			{
				asMain = false;
				sender.SetValue(row, _asMainFieldOrdinal, false);
			}

			var addressCache = sender.Graph.Caches[typeof(Address)];
			if (accountAddress != null && true.Equals(asMain))
			{
				if (currentAddressId != null && !object.Equals(currentAddressId, accountAddressId))
				{
					var currentAddress = (Address)PXSelect<Address,
						Where<Address.addressID, Equal<Required<Address.addressID>>>>.
						Select(sender.Graph, currentAddressId);
					var oldDirty = addressCache.IsDirty;
					addressCache.Delete(currentAddress);
					addressCache.IsDirty = oldDirty;
				}
				sender.SetValue(row, _addressIdFieldOrdinal, accountAddressId);
			}
			else
			{
				if (currentAddressId == null || object.Equals(currentAddressId, accountAddressId))
				{
					var oldDirty = addressCache.IsDirty;
					Address addr;
					if (accountAddress != null)
					{
						addr = (Address)addressCache.CreateCopy(accountAddress);
					}
					else
					{
						addr = (Address)addressCache.CreateInstance();
					}
					addr.AddressID = null;
					addr.BAccountID = (int?)accountId;
					addr = (Address)addressCache.Insert(addr);

					sender.SetValue(row, _addressIdFieldOrdinal, addr.AddressID);
					addressCache.IsDirty = oldDirty;
				}
			}
		}

		private void RowUpdatingHandler(PXCache sender, PXRowUpdatingEventArgs e)
		{
			var row = e.NewRow;
			var oldRow = e.Row;

			var asMain = sender.GetValue(row, _asMainFieldOrdinal);
			var oldAsMain = sender.GetValue(oldRow, _asMainFieldOrdinal);

			var addressId = sender.GetValue(row, _addressIdFieldOrdinal);

			var accountId = sender.GetValue(row, _accountIdFieldOrdinal);
			var account = accountId.
				With(_ => (BAccount)PXSelect<BAccount,
					Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Select(_Graph, _));
			var accountAddressId = account.With(_ => _.DefAddressID);
			var accountAddress = accountAddressId.
				With<int?, Address>(_ => (Address)PXSelect<Address,
					Where<Address.addressID, Equal<Required<Address.addressID>>>>.
				Select(sender.Graph, _));

			var oldAccountId = sender.GetValue(oldRow, _accountIdFieldOrdinal);
			if (!object.Equals(accountId, oldAccountId))
			{
				var oldAddressId = sender.GetValue(row, _addressIdFieldOrdinal);
				var oldAccount = accountId.
					With(_ => (BAccount)PXSelect<BAccount,
						Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(_Graph, _));
				var oldAccountAddressId = oldAccount.With(_ => _.DefAddressID);
				var oldAccountAddress = oldAccountAddressId.
					With<int?, Address>(_ => (Address)PXSelect<Address,
						Where<Address.addressID, Equal<Required<Address.addressID>>>>.
					Select(sender.Graph, _));
				oldAsMain = oldAccountAddress != null && object.Equals(oldAddressId, oldAccountAddressId);
				if (true.Equals(oldAsMain))
				{
					asMain = true;
					addressId = accountAddressId;
					sender.SetValue(row, _addressIdFieldOrdinal, accountAddressId);
				} 
			}

			if (true.Equals(asMain))
			{
				
				if (accountAddress == null)
				{
					asMain = false;
					sender.SetValue(row, _asMainFieldOrdinal, false);
				}
			}

			if (!object.Equals(asMain, oldAsMain))
			{
				if (true.Equals(asMain))
				{
					sender.SetValue(row, _addressIdFieldOrdinal, accountAddressId);
				}
				else
				{
					if (object.Equals(accountAddressId, addressId))
						sender.SetValue(row, _addressIdFieldOrdinal, null);
				}
			}
		}

		private void RowUpdatedHandler(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = e.Row;
			var oldRow = e.OldRow;

			var accountId = sender.GetValue(row, _accountIdFieldOrdinal);
			var addressId = sender.GetValue(row, _addressIdFieldOrdinal);
			var oldAddressId = sender.GetValue(oldRow, _addressIdFieldOrdinal);
			var addressCache = _Graph.Caches[typeof(Address)];
			if (!object.Equals(addressId, oldAddressId))
			{
				var account = accountId.
					With(_ => (BAccount)PXSelect<BAccount,
						Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(_Graph, _));
				var accountAddressId = account.With(_ => _.DefAddressID);
				var accountWithDefAddress = oldAddressId.
					With(_ => (BAccount)PXSelect<BAccount,
						Where<BAccount.defAddressID, Equal<Required<BAccount.defAddressID>>>>.
					Select(_Graph, _));
				if (accountWithDefAddress == null)
				{
					var oldAddress = oldAddressId.
						With(_ => (Address)PXSelect<Address, 
							Where<Address.addressID, Equal<Required<Address.addressID>>>>.
						Select(_Graph, _));
					if (oldAddress != null)
					{
						var oldIsDirty = addressCache.IsDirty;
						addressCache.Delete(oldAddress);
						addressCache.IsDirty = oldIsDirty;
					}
				}

				if (addressId == null)
				{
					var oldDirty = addressCache.IsDirty;
					Address addr;
					var accountAddress = accountAddressId.
						With<int?, Address>(_ => (Address)PXSelect<Address,
							Where<Address.addressID, Equal<Required<Address.addressID>>>>.
						Select(_Graph, _));
					if (accountAddress != null && object.Equals(accountAddressId, oldAddressId))
					{
						addr = (Address)addressCache.CreateCopy(accountAddress);
					}
					else
					{
						addr = (Address)addressCache.CreateInstance();
					}
					if (addr != null)
					{
						addr.AddressID = null;
						addr.BAccountID = (int?)accountId;
						addr = (Address)addressCache.Insert(addr);

						sender.SetValue(row, _addressIdFieldOrdinal, addr.AddressID);
						sender.SetValue(row, _asMainFieldOrdinal, false);
						addressCache.IsDirty = oldDirty;
						addressId = addr.AddressID;
					}
				}
			}
			if (addressId == null)
			{
				var oldDirty = addressCache.IsDirty;
				var addr = (Address)addressCache.CreateInstance();
				addr.AddressID = null;
				addr.BAccountID = (int?)accountId;
				addr = (Address)addressCache.Insert(addr);

				sender.SetValue(row, _addressIdFieldOrdinal, addr.AddressID);
				sender.SetValue(row, _asMainFieldOrdinal, false);
				addressCache.IsDirty = oldDirty;
			}
		}

		[PXUIField(DisplayName = CS.Messages.ValidateAddress, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		protected virtual IEnumerable ValidateAddress(PXAdapter adapter)
		{
			var graph = adapter.View.Graph;
			var primaryCache = graph.Caches[_itemType];
			var primaryRecord = primaryCache.Current;
			if (primaryRecord != null)
			{
				var addressId = primaryCache.GetValue(primaryRecord, _addressIdFieldOrdinal);
				var address = addressId.With(_ => (Address)PXSelect<Address,
						Where<Address.addressID, Equal<Required<Address.addressID>>>>.
						Select(graph, _));
				if (address != null && address.IsValidated != true)
					PXAddressValidator.Validate<Address>(graph, address, true);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.ViewOnMap)]
		[PXButton()]
		protected virtual IEnumerable ViewOnMap(PXAdapter adapter)
		{
			var graph = adapter.View.Graph;
			var primaryCache = graph.Caches[_itemType];
			var primaryRecord = primaryCache.Current;
			if (primaryRecord != null)
			{
				var currentAddressId = primaryCache.GetValue(primaryRecord, _addressIdFieldOrdinal);
				var currentAddress = currentAddressId.With(_ => (Address)PXSelect<Address,
						Where<Address.addressID, Equal<Required<Address.addressID>>>>.
						Select(graph, _));
				if (currentAddress != null)
					BAccountUtility.ViewOnMap(currentAddress);
			}
			return adapter.Get();
		}
	}

	#endregion

	#region AddressSelect

	public sealed class AddressSelect<TAddressIdField, TAsMainField, TAccountIdField> : AddressSelectBase
		where TAddressIdField : IBqlField
		where TAsMainField : IBqlField
		where TAccountIdField : IBqlField
	{
		public AddressSelect(PXGraph graph) : base(graph)
		{
		}

		protected override Type AccountIdField
		{
			get { return typeof(TAccountIdField); }
		}

		protected override Type AsMainField
		{
			get { return typeof(TAsMainField); }
		}

		protected override Type AddressIdField
		{
			get { return typeof(TAddressIdField); }
		}

		protected override Type IncorrectPersistableDAC
		{
			get { return typeof(TAddressIdField); }
		}

		protected override object GetPrimaryRow()
		{
			return _Graph.Caches[ItemType].Current;
		}
	}

	#endregion

	#region AddressSelect2

	public sealed class AddressSelect2<TAddressIdFieldSearch, TAsMainField, TAccountIdField> : AddressSelectBase
		where TAddressIdFieldSearch : IBqlSearch
		where TAsMainField : IBqlField
		where TAccountIdField : IBqlField
	{
		private Type _addressIdField;
		private PXView _select;

		public AddressSelect2(PXGraph graph) : base(graph)
		{
		}

		private void Initialize()
		{
			if (_addressIdField != null) return;

			var search = BqlCommand.CreateInstance(typeof(TAddressIdFieldSearch));
			_addressIdField = ((IBqlSearch)search).GetField();
			_select = new PXView(_Graph, false, GetSelectCommand(search));
		}

		private BqlCommand GetSelectCommand(BqlCommand search)
		{
			var arr = BqlCommand.Decompose(search.GetType());
			if (arr.Length < 2)
				throw new Exception("Unsupported search command detected");

			Type oldCommand = arr[0];
			Type newCommand = null;
			if (oldCommand == typeof(Search<,>))
				newCommand = typeof(Select<,>);
			if (oldCommand == typeof(Search2<,>))
				newCommand = typeof(Select2<,>);
			if (oldCommand == typeof(Search2<,,>))
				newCommand = typeof(Select2<,,>);

			if (newCommand == null)
				throw new Exception("Unsupported search command detected");

			arr[0] = newCommand;
			arr[1] = arr[1].DeclaringType;
			return BqlCommand.CreateInstance(arr);
		}

		protected override Type AccountIdField
		{
			get { return typeof(TAccountIdField); }
		}

		protected override Type AsMainField
		{
			get { return typeof(TAsMainField); }
		}

		protected override Type AddressIdField
		{
			get
			{
				Initialize();
				return _addressIdField;
			}
		}

		protected override Type IncorrectPersistableDAC
		{
			get { return typeof(TAddressIdFieldSearch); }
		}

		protected override object GetPrimaryRow()
		{
			var record = _select.SelectSingle();
			if (record is PXResult)
				record = ((PXResult)record)[ItemType];
			return record;
		}

		protected override IEnumerable SelectDelegate()
		{
			PXCache primaryCache = _Graph.Caches[_itemType];
			object primaryRecord = GetPrimaryRow();
			object currentAddressId = primaryCache.GetValue(primaryRecord, _addressIdFieldOrdinal);
			PXCache addrCache = _Graph.Caches<Address>();
			foreach (Address addr in PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(_Graph, currentAddressId))
			{
				Address a = addr;
				bool? asMain = (bool?)primaryCache.GetValue(primaryRecord, _asMainFieldOrdinal);
				if (asMain == true)
				{
					a = PXCache<Address>.CreateCopy(a);
					PXUIFieldAttribute.SetEnabled(addrCache, a, false);
				}
				else
				{
					PXUIFieldAttribute.SetEnabled(addrCache, a, true);
				}
				yield return a;
			}
		}

	}

	#endregion

	#region ContactSelectBase

	public abstract class ContactSelectBase : PXSelectBase
	{
		protected Type _itemType;

		protected int _contactIdFieldOrdinal;
		protected int _asMainFieldOrdinal;
		private int _accountIdFieldOrdinal;

		protected ContactSelectBase(PXGraph graph)
		{
			Initialize(graph);
			CreateView(graph);
			AttacheHandlers(graph);
		}

		public bool DoNotCorrectUI { get; set; }

		private void Initialize(PXGraph graph)
		{
			_Graph = graph;
			_Graph.EnshureCachePersistance(typeof(Contact));
			_Graph.Initialized += sender => sender.Views.Caches.Remove(IncorrectPersistableDAC);

			var contactIdDAC = GetDAC(ContactIdField);
			var asMainDAC = GetDAC(AsMainField);
			var accounDAC = GetDAC(AccountIdField);
			if (contactIdDAC != asMainDAC || asMainDAC != accounDAC)
				throw new Exception(string.Format("Fields '{0}', '{1}' and '{2}' are defined in different DACs",
					contactIdDAC.Name, asMainDAC.Name, accounDAC));
			_itemType = contactIdDAC;

			var cache = _Graph.Caches[_itemType];
			_contactIdFieldOrdinal = cache.GetFieldOrdinal(ContactIdField.Name);
			_asMainFieldOrdinal = cache.GetFieldOrdinal(AsMainField.Name);
			_accountIdFieldOrdinal = cache.GetFieldOrdinal(AccountIdField.Name);
		}

		protected abstract Type AccountIdField { get; }

		protected abstract Type AsMainField { get; }

		protected abstract Type ContactIdField { get; }

		protected abstract Type IncorrectPersistableDAC { get; }

		private static Type GetDAC(Type type)
		{
			var res = type.DeclaringType;
			if (res == null)
				throw new Exception(string.Format("DAC for field '{0}' can not be found", type.Name));
			return res;
		}

		private void CreateView(PXGraph graph)
		{
			var command = new Select<Contact>();
			View = new PXView(graph, false, command, new PXSelectDelegate(SelectDelegate));
		}

		private void AttacheHandlers(PXGraph graph)
		{
			graph.RowInserted.AddHandler(_itemType, RowInsertedHandler);
			graph.RowUpdating.AddHandler(_itemType, RowUpdatingHandler);
			graph.RowUpdated.AddHandler(_itemType, RowUpdatedHandler);
			graph.RowSelected.AddHandler(_itemType, RowSelectedHandler);
			graph.RowDeleted.AddHandler(_itemType, RowDeletedHandler);
		}

		private void RowDeletedHandler(PXCache sender, PXRowDeletedEventArgs e)
		{
			var currentContactId = sender.GetValue(e.Row, _contactIdFieldOrdinal);
			var isMainContact = sender.GetValue(e.Row, AsMainField.Name) as bool?;
			if (isMainContact == true) return;

			var currentContact = currentContactId.
				With(_ => (Contact)PXSelect<Contact,
					Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
				Select(sender.Graph, _));
			if (currentContact != null)
			{
				var contactCache = sender.Graph.Caches[typeof(Contact)];
				contactCache.Delete(currentContact);
			}
		}

		private void RowSelectedHandler(PXCache sender, PXRowSelectedEventArgs e)
		{
			var contactCache = sender.Graph.Caches[typeof(Contact)];
			var asMain = false;

			var accountId = sender.GetValue(e.Row, _accountIdFieldOrdinal);

			var currentContactId = sender.GetValue(e.Row, _contactIdFieldOrdinal);
			var currentContact = currentContactId.
				With(_ => (Contact)PXSelect<Contact,
					Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
				Select(sender.Graph, _));
			if (currentContactId != null && currentContact != null)
			{
				var account = accountId.
					With(_ => (BAccount)PXSelect<BAccount,
						Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(_Graph, _));
				var accountContactId = account.With(_ => _.DefContactID);
				if (currentContactId.Equals(accountContactId))
				{
					asMain = true;
				}
			}

			sender.SetValue(e.Row, _asMainFieldOrdinal, asMain);
			if (!DoNotCorrectUI) PXUIFieldAttribute.SetEnabled(contactCache, currentContact, !asMain);
		}

		protected virtual IEnumerable SelectDelegate()
		{
			var primaryCache = _Graph.Caches[_itemType];
			var primaryRecord = GetPrimaryRow();
			var currentContactId = primaryCache.GetValue(primaryRecord, _contactIdFieldOrdinal);

			yield return (Contact)PXSelect<Contact,
				Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
				Select(_Graph, currentContactId);
		}

		protected Type ItemType
		{
			get { return _itemType; }
		}

		protected abstract object GetPrimaryRow();

		private void RowInsertedHandler(PXCache sender, PXRowInsertedEventArgs e)
		{
			var row = e.Row;
			var asMain = sender.GetValue(row, _asMainFieldOrdinal);
			var accountId = sender.GetValue(row, _accountIdFieldOrdinal);
			var account = accountId.With(_ => (BAccount)PXSelect<BAccount,
				Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Select(_Graph, _));
			var accountContactId = account.With(_ => _.DefContactID);
			var accountContact = accountContactId.
				With<int?, Contact>(_ => (Contact)PXSelect<Contact,
					Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
				Select(sender.Graph, _));
			var currentContactId = sender.GetValue(row, _contactIdFieldOrdinal);

			if (accountContact == null)
			{
				asMain = false;
				sender.SetValue(row, _asMainFieldOrdinal, false);
			}

			var contactCache = sender.Graph.Caches[typeof(Contact)];
			if (accountContact != null && true.Equals(asMain))
			{
				if (currentContactId != null && !object.Equals(currentContactId, accountContactId))
				{
					var currentContact = (Contact)PXSelect<Contact,
						Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
						Select(sender.Graph, currentContactId);
					var oldDirty = contactCache.IsDirty;
					contactCache.Delete(currentContact);
					contactCache.IsDirty = oldDirty;
				}
				sender.SetValue(row, _contactIdFieldOrdinal, accountContactId);
			}
			else
			{
				if (currentContactId == null || object.Equals(currentContactId, accountContactId))
				{
					var oldDirty = contactCache.IsDirty;
					Contact cnt;
					if (accountContact != null)
					{
						cnt = (Contact)contactCache.CreateCopy(accountContact);
					}
					else
					{
						cnt = (Contact)contactCache.CreateInstance();
					}
					cnt.ContactID = null;
					cnt.BAccountID = (int?)accountId;
					cnt = (Contact)contactCache.Insert(cnt);

					sender.SetValue(row, _contactIdFieldOrdinal, cnt.ContactID);
					contactCache.IsDirty = oldDirty;
				}
			}
		}

		private void RowUpdatingHandler(PXCache sender, PXRowUpdatingEventArgs e)
		{
			var row = e.NewRow;
			var oldRow = e.Row;

			var asMain = sender.GetValue(row, _asMainFieldOrdinal);
			var oldAsMain = sender.GetValue(oldRow, _asMainFieldOrdinal);

			var contactId = sender.GetValue(row, _contactIdFieldOrdinal);

			var accountId = sender.GetValue(row, _accountIdFieldOrdinal);
			var account = accountId.
				With(_ => (BAccount)PXSelect<BAccount,
					Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Select(_Graph, _));
			var accountContactId = account.With(_ => _.DefContactID);
			var accountContact = accountContactId.
				With<int?, Contact>(_ => (Contact)PXSelect<Contact,
					Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
				Select(sender.Graph, _));

			var oldAccountId = sender.GetValue(oldRow, _accountIdFieldOrdinal);
			if (!object.Equals(accountId, oldAccountId))
			{
				var oldContactId = sender.GetValue(row, _contactIdFieldOrdinal);
				var oldAccount = accountId.
					With(_ => (BAccount)PXSelect<BAccount,
						Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(_Graph, _));
				var oldAccountContactId = oldAccount.With(_ => _.DefContactID);
				var oldAccountContact = oldAccountContactId.
					With<int?, Contact>(_ => (Contact)PXSelect<Contact,
						Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
					Select(sender.Graph, _));
				oldAsMain = oldAccountContact != null && object.Equals(oldContactId, oldAccountContactId);
				if (true.Equals(oldAsMain))
				{
					asMain = true;
					contactId = accountContactId;
					sender.SetValue(row, _contactIdFieldOrdinal, accountContactId);
				} 
			}

			if (true.Equals(asMain))
			{
				
				if (accountContact == null)
				{
					asMain = false;
					sender.SetValue(row, _asMainFieldOrdinal, false);
				}
			}

			if (!object.Equals(asMain, oldAsMain))
			{
				if (true.Equals(asMain))
				{
					sender.SetValue(row, _contactIdFieldOrdinal, accountContactId);
				}
				else
				{
					if (object.Equals(accountContactId, contactId))
						sender.SetValue(row, _contactIdFieldOrdinal, null);
				}
			}
		}

		private void RowUpdatedHandler(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = e.Row;
			var oldRow = e.OldRow;

			var accountId = sender.GetValue(row, _accountIdFieldOrdinal);
			var contactId = sender.GetValue(row, _contactIdFieldOrdinal);
			var oldContactId = sender.GetValue(oldRow, _contactIdFieldOrdinal);
			var contactCache = _Graph.Caches[typeof(Contact)];
			if (!object.Equals(contactId, oldContactId))
			{
				var account = accountId.
					With(_ => (BAccount)PXSelect<BAccount,
						Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(_Graph, _));
				var accountContactId = account.With(_ => _.DefContactID);
				var accountWithDefContact = oldContactId.
					With(_ => (BAccount)PXSelect<BAccount,
						Where<BAccount.defContactID, Equal<Required<BAccount.defContactID>>>>.
					Select(_Graph, _));
				if (accountWithDefContact == null)
				{
					var oldContact = oldContactId.
						With(_ => (Contact)PXSelect<Contact,
							Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
						Select(_Graph, _));
					if (oldContact != null)
					{
						var oldIsDirty = contactCache.IsDirty;
						contactCache.Delete(oldContact);
						contactCache.IsDirty = oldIsDirty;
					}
				}

				if (contactId == null)
				{
					var oldDirty = contactCache.IsDirty;
					Contact cnt;
					var accountContact = accountContactId.
						With<int?, Contact>(_ => (Contact)PXSelect<Contact,
							Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
						Select(_Graph, _));
					if (accountContact != null && object.Equals(accountContactId, oldContactId))
					{
						cnt = (Contact)contactCache.CreateCopy(accountContact);
					}
					else
					{
						cnt = (Contact)contactCache.CreateInstance();
					}
					if (cnt != null)
					{
						cnt.ContactID = null;
						cnt.BAccountID = (int?)accountId;
						cnt = (Contact)contactCache.Insert(cnt);

						sender.SetValue(row, _contactIdFieldOrdinal, cnt.ContactID);
						sender.SetValue(row, _asMainFieldOrdinal, false);
						contactCache.IsDirty = oldDirty;
						contactId = cnt.ContactID;
					}
				}
			}
			if (contactId == null)
			{
				var oldDirty = contactCache.IsDirty;
				var addr = (Contact)contactCache.CreateInstance();
				addr.ContactID = null;
				addr.BAccountID = (int?)accountId;
				addr = (Contact)contactCache.Insert(addr);

				sender.SetValue(row, _contactIdFieldOrdinal, addr.ContactID);
				sender.SetValue(row, _asMainFieldOrdinal, false);
				contactCache.IsDirty = oldDirty;
			}
		}
	}

	#endregion

	#region ContactSelect

	public sealed class ContactSelect<TContactIdField, TAsMainField, TAccountIdField> : ContactSelectBase
		where TContactIdField : IBqlField
		where TAsMainField : IBqlField
		where TAccountIdField : IBqlField
	{
		public ContactSelect(PXGraph graph)
			: base(graph)
		{
		}

		protected override Type AccountIdField
		{
			get { return typeof(TAccountIdField); }
		}

		protected override Type AsMainField
		{
			get { return typeof(TAsMainField); }
		}

		protected override Type ContactIdField
		{
			get { return typeof(TContactIdField); }
		}

		protected override Type IncorrectPersistableDAC
		{
			get { return typeof(TContactIdField); }
		}

		protected override object GetPrimaryRow()
		{
			return _Graph.Caches[ItemType].Current;
		}
	}

	#endregion

	#region ContactSelect2

	public sealed class ContactSelect2<TContactIdFieldSearch, TAsMainField, TAccountIdField> : ContactSelectBase
		where TContactIdFieldSearch : IBqlSearch
		where TAsMainField : IBqlField
		where TAccountIdField : IBqlField
	{
		private Type _addressIdField;
		private PXView _select;

		public ContactSelect2(PXGraph graph)
			: base(graph)
		{
		}

		private void Initialize()
		{
			if (_addressIdField != null) return;

			var search = BqlCommand.CreateInstance(typeof(TContactIdFieldSearch));
			_addressIdField = ((IBqlSearch)search).GetField();
			_select = new PXView(_Graph, false, GetSelectCommand(search));
		}

		private BqlCommand GetSelectCommand(BqlCommand search)
		{
			var arr = BqlCommand.Decompose(search.GetType());
			if (arr.Length < 2)
				throw new Exception("Unsupported search command detected");

			Type oldCommand = arr[0];
			Type newCommand = null;
			if (oldCommand == typeof(Search<,>))
				newCommand = typeof(Select<,>);
			if (oldCommand == typeof(Search2<,>))
				newCommand = typeof(Select2<,>);
			if (oldCommand == typeof(Search2<,,>))
				newCommand = typeof(Select2<,,>);

			if (newCommand == null)
				throw new Exception("Unsupported search command detected");

			arr[0] = newCommand;
			arr[1] = arr[1].DeclaringType;
			return BqlCommand.CreateInstance(arr);
		}

		protected override Type AccountIdField
		{
			get { return typeof(TAccountIdField); }
		}

		protected override Type AsMainField
		{
			get { return typeof(TAsMainField); }
		}

		protected override Type ContactIdField
		{
			get
			{
				Initialize();
				return _addressIdField;
			}
		}

		protected override Type IncorrectPersistableDAC
		{
			get { return typeof(TContactIdFieldSearch); }
		}

		protected override object GetPrimaryRow()
		{
			object record = _select.SelectSingle();
			if (record is PXResult)
				record = ((PXResult)record)[ItemType];
			return record;
		}

		protected override IEnumerable SelectDelegate()
		{
			PXCache primaryCache = _Graph.Caches[_itemType];
			object primaryRecord = GetPrimaryRow();
			object currentContactId = primaryCache.GetValue(primaryRecord, _contactIdFieldOrdinal);
			PXCache contactCache = _Graph.Caches<Contact>();
			foreach (Contact cnt in PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.Select(_Graph, currentContactId))
			{
				Contact c = cnt;
				bool? asMain = (bool?)primaryCache.GetValue(primaryRecord, _asMainFieldOrdinal);
				if (asMain == true)
				{
					c = PXCache<Contact>.CreateCopy(c);
					PXUIFieldAttribute.SetEnabled(contactCache, c, false);
				}
				else
				{
					PXUIFieldAttribute.SetEnabled(contactCache, c, true);
				}
				yield return c;
			}
		}
	}

	#endregion

	#region PXOwnerFilteredSelect

	public class PXOwnerFilteredSelect<TFilter, TSelect, TGroupID, TOwnerID> : PXSelectBase
		where TFilter : OwnedFilter, new()
		where TSelect : IBqlSelect
		where TGroupID : IBqlField
		where TOwnerID : IBqlField
	{
		private BqlCommand _command;
		private Type _selectTarget;
		private Type _newRecordTarget;

		public PXOwnerFilteredSelect(PXGraph graph)
			: this(graph, false)
		{
			
		}

		protected PXOwnerFilteredSelect(PXGraph graph, bool readOnly)
			: base()
		{
			_Graph = graph;

			InitializeView(readOnly);
			InitializeSelectTarget();
			AppendActions();
			AppendEventHandlers();
		}

		public Type NewRecordTarget
		{
			get { return _newRecordTarget; }
			set
			{
				if (value != null)
				{
					if (!typeof(PXGraph).IsAssignableFrom(value))
						throw new ArgumentException(string.Format("{0} is excpected", typeof(PXGraph).GetLongName()), "value");
					if (value.GetConstructor(new Type[0]) == null)
						throw new ArgumentException("Default constructor is excpected", "value");
				}
				_newRecordTarget = value;
			}
		}

		private void AppendEventHandlers()
		{
			_Graph.RowSelected.AddHandler<TFilter>(RowSelectedHandler);
		}

		private void RowSelectedHandler(PXCache sender, PXRowSelectedEventArgs e)
		{
			var me = true.Equals(sender.GetValue(e.Row, typeof(OwnedFilter.myOwner).Name));
			var myGroup = true.Equals(sender.GetValue(e.Row, typeof(OwnedFilter.myWorkGroup).Name));

			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(OwnedFilter.ownerID).Name, !me);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(OwnedFilter.workGroupID).Name, !myGroup);
		}

		private void AppendActions()
		{
			_Graph.Initialized += sender =>
				{
					var name = _Graph.ViewNames[View] + "_AddNew";
					PXNamedAction.AddAction(_Graph, typeof(TFilter), name, Messages.AddNew, new PXButtonDelegate(AddNewHandler));
				};
		}

		[PXButton(Tooltip = "Add New Record", CommitChanges = true)]
		private IEnumerable AddNewHandler(PXAdapter adapter)
		{
			var filterCache = _Graph.Caches[typeof(TFilter)];
			var currentFilter = filterCache.Current;
			if (NewRecordTarget != null && _selectTarget != null && currentFilter != null)
			{
				var currentOwnerId = filterCache.GetValue(currentFilter, typeof (OwnedFilter.ownerID).Name);
				var currentWorkgroupId = filterCache.GetValue(currentFilter, typeof (OwnedFilter.workGroupID).Name);

				var targetGraph = (PXGraph)PXGraph.CreateInstance(NewRecordTarget);
				var targetCache = targetGraph.Caches[_selectTarget];
				var row = targetCache.Insert();
				var newRow = targetCache.CreateCopy(row);

				EPCompanyTreeMember member = PXSelect<EPCompanyTreeMember,
												Where<EPCompanyTreeMember.userID, Equal<Required<OwnedFilter.ownerID>>,
												  And<EPCompanyTreeMember.workGroupID, Equal<Required<OwnedFilter.workGroupID>>>>>.
				Select(targetGraph, currentOwnerId, currentWorkgroupId);
				if (member == null) currentOwnerId = null;
				
				targetCache.SetValue(newRow, typeof(TGroupID).Name, currentWorkgroupId);
				targetCache.SetValue(newRow, typeof(TOwnerID).Name, currentOwnerId);
				targetCache.Update(newRow);
				PXRedirectHelper.TryRedirect(targetGraph, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		private void InitializeSelectTarget()
		{
			var selectTabels = _command.GetTables();
			if (selectTabels == null || selectTabels.Length == 0)
				throw new Exception("Primary table of given select command cannot be found");

			_selectTarget = selectTabels[0];
			_Graph.EnshureCachePersistance(_selectTarget);
		}

		private void InitializeView(bool readOnly)
		{
			_command = CreateCommand();
			View = new PXView(_Graph, readOnly, _command, new PXSelectDelegate(Handler));
		}

		private IEnumerable Handler()
		{
			var filterCache = _Graph.Caches[typeof(TFilter)];
			var currentFilter = filterCache.Current;
			if (filterCache.Current == null) return new object[0];

			var parameters = GetParameters(filterCache, currentFilter);

			return _Graph.QuickSelect(_command, parameters);
		}

		private static object[] GetParameters(PXCache filterCache, object currentFilter)
		{
			var currentOwnerId = filterCache.GetValue(currentFilter, typeof(OwnedFilter.ownerID).Name);
			var currentWorkgroupId = filterCache.GetValue(currentFilter, typeof(OwnedFilter.workGroupID).Name);
			var currentMyWorkgroup = filterCache.GetValue(currentFilter, typeof(OwnedFilter.myWorkGroup).Name);
			var parameters = new object[]
				{
					currentOwnerId, currentOwnerId, 
					currentMyWorkgroup, currentMyWorkgroup, 
					currentWorkgroupId, currentWorkgroupId, currentMyWorkgroup
				};
			return parameters;
		}

		private static BqlCommand CreateCommand()
		{
			var command = BqlCommand.CreateInstance(typeof(TSelect));
			var additionalCondition = BqlCommand.Compose(
				typeof(Where2<Where<Required<OwnedFilter.ownerID>, IsNull,
								Or<Required<OwnedFilter.ownerID>, Equal<TOwnerID>>>,
							And<
								Where2<
									Where<Required<OwnedFilter.myWorkGroup>, IsNull,
										Or<Required<OwnedFilter.myWorkGroup>, Equal<False>>>,
									And2<
										Where<Required<OwnedFilter.workGroupID>, IsNull, 
											Or<TGroupID, Equal<Required<OwnedFilter.workGroupID>>>>,
										Or<Required<OwnedFilter.myWorkGroup>, Equal<True>,
									And<TGroupID, InMember<Current<AccessInfo.userID>>>>>>>>));
			return command.WhereAnd(additionalCondition);
		}
	}

	#endregion

	#region PXOwnerFilteredSelectReadonly

	public class PXOwnerFilteredSelectReadonly<TFilter, TSelect, TGroupID, TOwnerID> 
		: PXOwnerFilteredSelect<TFilter, TSelect, TGroupID, TOwnerID>
		where TFilter : OwnedFilter, new()
		where TSelect : IBqlSelect
		where TGroupID : IBqlField
		where TOwnerID : IBqlField
	{
		public PXOwnerFilteredSelectReadonly(PXGraph graph)
			:base(graph, true)
		{
		}
	}

	#endregion

	#region CRLastNameDefaultAttribute

	internal sealed class CRLastNameDefaultAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber
	{
		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var contactType = sender.GetValue(e.Row, typeof(Contact.contactType).Name);
			var val = sender.GetValue(e.Row, _FieldOrdinal) as string;
			if (contactType != null && (contactType.Equals(ContactTypesAttribute.Lead) || contactType.Equals(ContactTypesAttribute.Person)) && string.IsNullOrWhiteSpace(val))
			{
				if (sender.RaiseExceptionHandling(_FieldName, e.Row, null, new PXSetPropertyKeepPreviousException(PXMessages.LocalizeFormat(ErrorMessages.FieldIsEmpty, _FieldName))))
				{
					throw new PXRowPersistingException(_FieldName, null, ErrorMessages.FieldIsEmpty, _FieldName);
				}
			}
		}
	}

	#endregion

	#region CRContactBAccountDefaultAttribute

	internal sealed class CRContactBAccountDefaultAttribute :  PXEventSubscriberAttribute, IPXRowInsertingSubscriber, IPXRowUpdatingSubscriber, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber
	{
		private Dictionary<object, object> _persistedItems;

		public override void CacheAttached(PXCache sender)
		{
			_persistedItems = new Dictionary<object, object>();
			sender.Graph.RowPersisting.AddHandler(typeof(BAccount), SourceRowPersisting);
		}

		public void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			SetDefaultValue(sender, e.Row);
		}

		public void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			SetDefaultValue(sender, e.Row);
		}

		private void SetDefaultValue(PXCache sender, object row)
		{
			if (IsLeadOrPerson(sender, row)) return;

			var val = sender.GetValue(row, _FieldOrdinal);
			if (val != null) return;

			PXCache cache = sender.Graph.Caches[typeof (BAccount)];
			if (cache.Current != null)
			{
				var newValue = cache.GetValue(cache.Current, typeof (BAccount.bAccountID).Name);
				sender.SetValue(row, _FieldOrdinal, newValue);
			}
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (IsLeadOrPerson(sender, e.Row)) return;

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && true ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update && true)
			{
				object key = sender.GetValue(e.Row, _FieldOrdinal);
				if (key != null)
				{
					object parent;
					if (_persistedItems.TryGetValue(key, out parent))
					{
						key = sender.Graph.Caches[typeof(BAccount)].GetValue(parent, typeof(BAccount.bAccountID).Name);
						sender.SetValue(e.Row, _FieldOrdinal, key);
						if (key != null)
						{
							_persistedItems[key] = parent;
						}
					}
				}
			}
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && true ||
				 (e.Operation & PXDBOperation.Command) == PXDBOperation.Update && true) &&
				sender.GetValue(e.Row, _FieldOrdinal) == null)
			{
				throw new PXRowPersistingException(_FieldName, null, String.Format("'{0}' may not be empty.", _FieldName));
			}
		}

		public void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (IsLeadOrPerson(sender, e.Row)) return;

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Aborted)
			{
				object key = sender.GetValue(e.Row, _FieldOrdinal);
				if (key != null)
				{
					object parent;
					if (_persistedItems.TryGetValue(key, out parent))
					{
						var sourceField = typeof(BAccount.bAccountID).Name;
						sender.SetValue(e.Row, _FieldOrdinal, sender.Graph.Caches[typeof(BAccount)].GetValue(parent, sourceField));
					}
				}
			}
		}

		private void SourceRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (IsLeadOrPerson(sender, e.Row)) return;

			var sourceField = typeof(BAccount.bAccountID).Name;
			object key = sender.GetValue(e.Row, sourceField);
			if (key != null)
				_persistedItems[key] = e.Row;
		}

		private bool IsLeadOrPerson(PXCache sender, object row)
		{
			var contactType = sender.GetValue(row, typeof(Contact.contactType).Name);
			return contactType != null &&
				(contactType.Equals(ContactTypesAttribute.Lead) ||
					contactType.Equals(ContactTypesAttribute.Person));
		}
	}

	#endregion

	#region BAccountType Attribute
	public class BAccountType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { VendorType, CustomerType, CombinedType, EmployeeType, EmpCombinedType, ProspectType, CompanyType },
				new string[] { Messages.VendorType, Messages.CustomerType, Messages.CombinedType, Messages.EmployeeType, Messages.EmpCombinedType, Messages.ProspectType, Messages.CompanyType, }) { ; }
		}

		public const string VendorType = "VE";
		public const string CustomerType = "CU";
		public const string CombinedType = "VC";
		public const string EmployeeType = "EP";
		public const string EmpCombinedType = "EC";
		public const string ProspectType = "PR";
		public const string CompanyType = "CP";

		public class vendorType : Constant<string>
		{
			public vendorType() : base(VendorType) { ;}
		}
		public class customerType : Constant<string>
		{
			public customerType() : base(CustomerType) { ;}
		}
		public class combinedType : Constant<string>
		{
			public combinedType() : base(CombinedType) { ;}
		}
		public class employeeType : Constant<string>
		{
			public employeeType() : base(EmployeeType) { ;}
		}
		public class empCombinedType : Constant<string>
		{
			public empCombinedType() : base(EmpCombinedType) { ;}
		}
		public class prospectType : Constant<string>
		{
			public prospectType() : base(ProspectType) { ;}
		}
		public class companyType : Constant<string>
		{
			public companyType() : base(CompanyType) { ;}
		}
	}
	#endregion

	#region IActivityMaint

	public interface IActivityMaint
	{
		void CancelRow(EPActivity row);
		void CompleteRow(EPActivity row);
	}

	#endregion

	#region SelectContactEmailSync

	public class SelectContactEmailSync<TWhere> : PXSelectBase<Contact>
		where TWhere : IBqlWhere, new()
	{
		public SelectContactEmailSync(PXGraph graph, Delegate handler) : this(graph)
		{
			View = new PXView(_Graph, false, View.BqlSelect, handler);
		}

		public SelectContactEmailSync(PXGraph graph)
		{
			_Graph = graph;
			View = new PXView(_Graph, false, new Select<Contact>());
			View.WhereAnd<TWhere>();

			_Graph.FieldUpdated.AddHandler(typeof(Contact), typeof(Contact.eMail).Name, FieldUpdated<Contact.eMail, Users.email>);
			_Graph.FieldUpdated.AddHandler(typeof(Contact), typeof(Contact.firstName).Name, FieldUpdated<Contact.firstName, Users.firstName>);
			_Graph.FieldUpdated.AddHandler(typeof(Contact), typeof(Contact.lastName).Name, FieldUpdated<Contact.lastName, Users.lastName>);
		}

		protected virtual void FieldUpdated<TSrcField, TDstField>(PXCache sender, PXFieldUpdatedEventArgs e)
			where TSrcField : IBqlField
			where TDstField : IBqlField
		{
			Contact row = (Contact)e.Row;
			Users user = PXSelect<Users, Where<Users.pKID, Equal<Current<Contact.userID>>>>.SelectSingleBound(_Graph, new object[] { row });
			if (user != null)
			{
				PXCache usercache = _Graph.Caches[typeof(Users)];
				usercache.SetValue<TDstField>(user, sender.GetValue<TSrcField>(row));
				usercache.Update(user);
			}
		}
	}

	#endregion

	#region CRDuplicateContactList
	public class CRDuplicateContactList : PXSelectBase<CRDuplicateRecord>
	{
		#region MergeParams
		[Serializable]
		public class MergeParams : IBqlTable
		{
			#region SourceContactID
			public abstract class sourceContactID : IBqlField { }

			[PXDBInt]
			[PXDefault(typeof(Contact.contactID))]
			public virtual int? SourceContactID { get; set; }
			#endregion

			#region ContactID
			public abstract class contactID : IBqlField { }

			[PXDBInt]
			[PXUIField(DisplayName = "Target", Required = true)]
			[PXDefault(typeof(Contact.contactID))]
			[CRDuplicateContactsSelector(typeof(MergeParams.sourceContactID))]
			public virtual int? ContactID { get; set; }
			#endregion
		}
		#endregion

		public class MergeLead : CRBaseUpdateProcess<MergeLead, Contact, PXMassMergableFieldAttribute, Contact.classID, CSAnswerType.leadAnswerType> { }
		public class MergeAddress : CRBaseUpdateProcess<MergeAddress, Address, PXMassMergableFieldAttribute, Contact.classID, CSAnswerType.leadAnswerType> { }

		private const string MergeActionName = "merge";
		private const string AttachActionName = "attachToAccount";
		private const string MergeParamsViewName = "mergeParams";
		private const string FieldsViewName = "ValueConflicts";

		private readonly PXVirtualTableView<MergeParams> mergeParam;

		public CRDuplicateContactList(PXGraph graph)
		{
			_Graph = graph;
			View = new PXView(_Graph, false,
				new Select2<CRDuplicateRecord,
				LeftJoin<Contact, On<Contact.contactID, Equal<CRDuplicateRecord.contactID>>,
				LeftJoin<CRLeadContactValidationProcess.Contact2,
							On<CRLeadContactValidationProcess.Contact2.contactID, Equal<CRDuplicateRecord.duplicateContactID>>,
				LeftJoin<BAccountR, On<BAccountR.bAccountID, Equal<CRLeadContactValidationProcess.Contact2.bAccountID>>>>>,
			 Where<CRDuplicateRecord.contactID, Equal<Current<Contact.contactID>>,
				 And<CRDuplicateRecord.duplicateContactID, NotEqual<CRDuplicateRecord.contactID>, 
				 And<CRDuplicateRecord.validationType, Equal<Switch<Case<Where<Contact.contactType, Equal<ContactTypesAttribute.bAccountProperty>,
																																				And<CRLeadContactValidationProcess.Contact2.contactType, Equal<ContactTypesAttribute.bAccountProperty>>>, ValidationTypesAttribute.account,
																																 Case<Where<Contact.contactType, Equal<ContactTypesAttribute.bAccountProperty>,
																																				Or<CRLeadContactValidationProcess.Contact2.contactType, Equal<ContactTypesAttribute.bAccountProperty>>>, ValidationTypesAttribute.leadAccount>>,
																																 ValidationTypesAttribute.leadContact>>,
				 And<CRDuplicateRecord.score, GreaterEqual<
							Switch<Case<Where<CRLeadContactValidationProcess.Contact2.contactType, Equal<ContactTypesAttribute.bAccountProperty>>,
													Current<CRSetup.leadToAccountValidationThreshold>>, Current<CRSetup.leadValidationThreshold>>>,
				 And<Current<Contact.duplicateFound>, Equal<True>,
				 And<CRLeadContactValidationProcess.Contact2.duplicateStatus, NotEqual<DuplicateStatusAttribute.duplicated>,
				 And2<Where<CRLeadContactValidationProcess.Contact2.contactType, NotEqual<ContactTypesAttribute.bAccountProperty>,
								Or<CRLeadContactValidationProcess.Contact2.contactID, Equal<BAccountR.defContactID>>>,
				 And<Where<CRLeadContactValidationProcess.Contact2.contactType, NotEqual<ContactTypesAttribute.bAccountProperty>,								
				 				Or<Contact.bAccountID, IsNull,
 								Or<CRLeadContactValidationProcess.Contact2.bAccountID, IsNull, 
								Or<Contact.bAccountID, NotEqual<CRLeadContactValidationProcess.Contact2.bAccountID>>>>>>>>>>>>>>());

			_Graph.Views.Add(MergeParamsViewName, mergeParam = new PXVirtualTableView<MergeParams>(_Graph));
			_Graph.Views.Add(FieldsViewName, new PXView(_Graph, false, 
				new Select<FieldValue, Where<FieldValue.attributeID, IsNull>, OrderBy<Asc<FieldValue.order>>>(), 
				(PXSelectDelegate)valueConflicts));

			PXDBAttributeAttribute.Activate(_Graph.Caches[typeof(Contact)]);
			//Init PXVirtual Static constructor
			typeof(FieldValue).GetCustomAttributes(typeof(PXVirtualAttribute), false);

			_Graph.FieldSelecting.AddHandler<FieldValue.value>(delegate(PXCache sender, PXFieldSelectingEventArgs e)
				{
					if (e.Row == null) return;
					e.ReturnState = InitValueFieldState(e.Row as FieldValue);
				});
			_Graph.RowSelected.AddHandler<Contact>(delegate(PXCache sender, PXRowSelectedEventArgs e)
				{
					sender.Graph.Actions[MergeActionName].SetEnabled(e.Row != null && sender.GetStatus(e.Row) != PXEntryStatus.Inserted);
				});

			_Graph.FieldDefaulting.AddHandler<MergeParams.contactID>(MergeParams_ContactID_FieldDefaulting);

			PXNamedAction.AddAction(_Graph, _Graph.Views[_Graph.PrimaryView].Cache.GetItemType(), MergeActionName, Messages.Merge, Merge);
			PXNamedAction.AddAction(_Graph, _Graph.Views[_Graph.PrimaryView].Cache.GetItemType(), AttachActionName, Messages.AttachToAccount, Attach);

			PXUIFieldAttribute.SetDisplayName<BAccountR.type>(_Graph.Caches<BAccountR>(), Messages.BAccountType);
		}
		
		protected virtual void MergeParams_ContactID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			Contact current = (Contact) _Graph.Caches<Contact>().Current;
			List<Contact> contacts = PXSelectorAttribute.SelectAll<MergeParams.contactID>(_Graph.Caches<MergeParams>(), _Graph.Caches<MergeParams>().Current)
				.RowCast<Contact>()
				.Where(c => c.ContactType == ContactTypesAttribute.Person)
				.ToList();
			e.NewValue = current.ContactType == ContactTypesAttribute.Person || contacts.Count == 0 ? current.ContactID : contacts[0].ContactID;
		}

		public IEnumerable valueConflicts()
		{
			return _Graph.Caches[typeof(FieldValue)].Cached.Cast<FieldValue>().Where(fld => fld.Hidden != true);
		}

		public static Tuple<IEnumerable<string>, IEnumerable<string>> GetPossibleValues<T>(PXGraph graph, IEnumerable<T> collection, string propName)
			where T : IBqlTable
		{
			PXCache cache = graph.Caches[typeof(T)];
			return collection.Cast<object>()
							.Select(o => cache.GetStateExt(o, propName))
							.Cast<PXFieldState>()
							.Select(st =>
							{
								string value = null;
								string label = string.Empty;
								if (st != null)
								{
									var stringState = st as PXStringState;
									value = st.Value != null ? st.Value.ToString() : null;

									if (stringState != null && stringState.AllowedValues != null)
									{
										int i = Array.IndexOf(stringState.AllowedValues, value);
										label = (i == -1) ? value : stringState.AllowedLabels[i];
									}
								}
								return new[] { value != null ? new Tuple<string, string>(value, label) : new Tuple<string, string>(null, string.Empty) };
							})
							.SelectMany(z => z.Select(entry => entry))
							.GroupBy(z => z.Item1)
							.Select(g => new Tuple<string, string>(g.Key, g.First(z => z.Item1 == g.Key).Item2))
							.OrderBy(pair => pair.Item1)
							.UnZip();
		}


		protected PXFieldState InitValueFieldState(FieldValue field)
		{
			Tuple<IEnumerable<string>, IEnumerable<string>> possibleValues = new Tuple<IEnumerable<string>, IEnumerable<string>>(new string[0], new string[0]);
			List<Contact> contacts = new List<Contact>(PXSelectorAttribute.SelectAll<MergeParams.contactID>(_Graph.Caches[typeof(MergeParams)], _Graph.Caches[typeof(MergeParams)].Current).RowCast<Contact>());
			if (field.CacheName == typeof(Contact).FullName)
			{
				possibleValues = GetPossibleValues(_Graph, contacts, field.Name);
			}
			else if (field.CacheName == typeof(Address).FullName)
			{
				PXSelectBase<Address> cmd = new PXSelect<Address>(_Graph);
				List<int?> addressIDs = new List<int?>(contacts.Where(c => c.DefAddressID != null).Select(c => c.DefAddressID));
				foreach (int? a in addressIDs)
				{
					cmd.WhereOr<Where<Address.addressID, Equal<Required<Contact.defAddressID>>>>();
				}
				possibleValues = GetPossibleValues(_Graph, cmd.Select(addressIDs.Cast<object>().ToArray()).RowCast<Address>(), field.Name);
			}

			string[] values = possibleValues.Item1.ToArray();
			string[] labels = possibleValues.Item2.ToArray();

			return PXStringState.CreateInstance(field.Value, null, null, typeof(FieldValue.value).Name,
				false, 0, null, values, labels, null, null);
		}

		protected void InsertPropertyValue(FieldValue field, Dictionary<Type, object> targets)
		{
			Type t = Type.GetType(field.CacheName);
			PXCache cache = _Graph.Caches[t];
			object target = targets[t];

			PXStringState state = InitValueFieldState(field) as PXStringState;

			if (state != null)
			{
				if (state.AllowedValues == null || !state.AllowedValues.Any() || state.AllowedValues.Count() == 1 && field.AttributeID == null)
					return;
				if (state.AllowedValues.Count() == 1)
				{
					field.Hidden = true;
					field.Value = state.AllowedValues[0];
				}
				else if (target != null)
				{
					state.Required = true;
					object value = cache.GetValueExt(target, field.Name);
					if (value is PXFieldState) value = ((PXFieldState) value).Value;
					field.Value = value != null ? value.ToString() : null;
				}
			}
			_Graph.Caches[typeof(FieldValue)].Insert(field);
		}

		protected void FillPropertyValue()
		{
			PXCache cache = _Graph.Caches[typeof(FieldValue)];
			cache.Clear();

			PXCache<MergeParams> pcache = _Graph.Caches<MergeParams>();
			pcache.SetDefaultExt<MergeParams.contactID>(pcache.Current);

			int order = 0;
			List<FieldValue> fields = new List<FieldValue>(MergeLead.GetProcessingProperties(_Graph, ref order));
			HashSet<string> fieldNames = new HashSet<string>(fields.Select(f => f.Name));
			fields.AddRange(MergeAddress.GetMarkedProperties(_Graph, ref order).Where(fld => fieldNames.Add(fld.Name)));

			Contact contact = PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.Select(_Graph, ((MergeParams)pcache.Current).ContactID);
			Address address = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(_Graph, contact.DefAddressID);
			Dictionary<Type, object> targets = new Dictionary<Type, object>
			{
				{typeof (Contact), contact},
				{typeof (Address), address}
			};
			foreach (FieldValue fld in fields)
			{
				InsertPropertyValue(fld, targets);
			}
			cache.IsDirty = false;
		}

		[PXUIField(DisplayName = Messages.Merge)]
		[PXButton]
		public virtual IEnumerable Merge(PXAdapter adapter)
		{
			MergeParams parms = _Graph.Caches[typeof (MergeParams)].Current as MergeParams;			
			if (parms != null && adapter.Get().Cast<Contact>().Any(contact => contact.ContactID != parms.SourceContactID))
			{
				_Graph.Caches[typeof (MergeParams)].Delete(parms);
				_Graph.Caches[typeof (MergeParams)].Insert(new MergeParams());
			}

			List<Contact> contacts = new List<Contact>(PXSelectorAttribute.SelectAll<MergeParams.contactID>(_Graph.Caches[typeof(MergeParams)], _Graph.Caches[typeof(MergeParams)].Current)
				.RowCast<Contact>()
				.Select(c => _Graph.Caches[typeof(Contact)].CreateCopy(c))
				.Cast<Contact>());

			if(contacts.Count < 2) throw new PXException(Messages.DuplicatesNotSelected);

			if (AskExt((graph, name) => FillPropertyValue()) != WebDialogResult.OK 
				|| !mergeParam.VerifyRequired()) return adapter.Get();

			int? targetID = ((MergeParams)_Graph.Caches[typeof(MergeParams)].Current).ContactID;
			List<FieldValue> values = new List<FieldValue>(_Graph.Caches[typeof(FieldValue)].Cached.Cast<FieldValue>()
				.Select(v => _Graph.Caches[typeof(FieldValue)].CreateCopy(v))
				.Cast<FieldValue>());

			_Graph.Actions.PressSave();
			PXLongOperation.StartOperation(this._Graph,
				()=>MergeContacts(
				                               (int) targetID,
				                               contacts,
				                               values
				                               )
				);

			return adapter.Get();
		}
		[PXUIField(DisplayName = Messages.AttachToAccount)]
		[PXButton]
		public virtual IEnumerable Attach(PXAdapter adapter)
		{
			Contact duplicate = PXSelect<Contact, 
				Where<Contact.contactID, Equal<Current<CRDuplicateRecord.duplicateContactID>>>>
				.SelectSingleBound(this._Graph, new object[] { this.View.Cache.Current });
			if(duplicate == null || duplicate.ContactType != ContactTypesAttribute.BAccountProperty)
				throw new PXException(Messages.AttachToAccountNotFound);

			PXCache cache = this._Graph.Caches[typeof (Contact)];
			foreach (var item in adapter.Get())
			{
				Contact contact = PXResult.Unwrap<Contact>(item);
				if (contact != null)
				{
					Contact upd = (Contact)cache.CreateCopy(contact);
					upd.BAccountID = duplicate.BAccountID;
					upd = (Contact)cache.Update(upd);
					if(upd != null)
						cache.RestoreCopy(contact, upd);
				}
				yield return item;
			}
		}
		public static void MergeContacts(int targetID, List<Contact> contacts, List<FieldValue> values)
		{
			PXGraph graph = new PXGraph();
			PXPrimaryGraphCollection primaryGraph = new PXPrimaryGraphCollection(graph);
			Contact target = PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.Select(graph, targetID);

			graph = primaryGraph[target];
			PXCache cache = graph.Caches[typeof(Contact)];
			string entityType = CSAnswerType.GetAnswerType(cache.GetItemType());
			string entityID = CSAnswerType.GetEntityID(cache.GetItemType());
			target = PXCache<Contact>.CreateCopy(target);

			Address targetAddress = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(graph, target.DefAddressID);

			Dictionary<Type, object> targets = new Dictionary<Type, object> { { typeof(Contact), target }, { typeof(Address), targetAddress } };


			foreach (FieldValue fld in values)
			{
				if (fld.AttributeID == null)
				{
					Type type = Type.GetType(fld.CacheName);
					PXFieldState state = (PXFieldState)graph.Caches[type].GetStateExt(targets[type], fld.Name);
					if (state == null || !Equals(state.Value, fld.Value))
					{
						graph.Caches[type].SetValueExt(targets[type], fld.Name, fld.Value);
						targets[type] = graph.Caches[type].CreateCopy(graph.Caches[type].Update(targets[type]));
					}
				}
				else
				{
					PXCache attrCache = cache.Graph.Caches[typeof(CSAnswers)];
					CSAnswers attr = new CSAnswers
					{
						AttributeID = fld.AttributeID,
						EntityID = cache.GetValue(target, entityID) as int?,
						EntityType = entityType,
						Value = fld.Value
					};
					attrCache.Update(attr);
				}
			}

			bool needConvert = false;
			PXRedirectRequiredException redirect = null;
			using (PXTransactionScope scope = new PXTransactionScope())
			{
				foreach (Contact contact in contacts.Where(c => c.ContactID != targetID))
				{
					if (contact.ContactType != ContactTypesAttribute.Lead && target.ContactType == ContactTypesAttribute.Lead)
						needConvert = true;

					PXCache Cases = graph.Caches[typeof (CRCase)];
					foreach (CRCase cas in PXSelect<CRCase,
						Where<CRCase.contactID, Equal<Current<Contact.contactID>>>>.SelectMultiBound(graph, new object[] {contact})
						                                                           .RowCast<CRCase>()
						                                                           .Select(cas => (CRCase) Cases.CreateCopy(cas)))
					{
						if (target.BAccountID != contact.BAccountID)
						{
							throw new PXException(Messages.CannotChangeBAccount, contact.DisplayName);
						}
						cas.ContactID = target.ContactID;
						Cases.Update(cas);
					}

					PXCache Opportunities = graph.Caches[typeof (CROpportunity)];
					foreach (CROpportunity opp in PXSelect<CROpportunity,
						Where<CROpportunity.contactID, Equal<Current<Contact.contactID>>>>.SelectMultiBound(graph, new object[] {contact})
						                                                                  .RowCast<CROpportunity>()
						                                                                  .Select(opp => (CROpportunity) Opportunities.CreateCopy(opp)))
					{
						if (target.BAccountID != contact.BAccountID)
						{
							throw new PXException(Messages.CannotChangeBAccount, contact.DisplayName);
						}
						opp.ContactID = target.ContactID;
						Opportunities.Update(opp);
					}

					PXCache Relations = graph.Caches[typeof (CRRelation)];
					foreach (CRRelation rel in PXSelectJoin<CRRelation,
						LeftJoin<CRRelation2, On<CRRelation.entityID, Equal<CRRelation2.entityID>,
							And<CRRelation.role, Equal<CRRelation2.role>,
								And<CRRelation2.refNoteID, Equal<Required<Contact.noteID>>>>>>,
						Where<CRRelation2.entityID, IsNull,
							And<CRRelation.refNoteID, Equal<Required<Contact.noteID>>>>>.Select(graph, target.NoteID, contact.NoteID)
							                                                            .RowCast<CRRelation>()
							                                                            .Select(rel => (CRRelation) Relations.CreateCopy(rel)))
					{
						rel.RelationID = null;
						rel.RefNoteID = target.NoteID;
						Relations.Insert(rel);
					}

					PXCache Subscriptions = graph.Caches[typeof (CRMarketingListMember)];
					foreach (CRMarketingListMember mmember in PXSelectJoin<CRMarketingListMember,
						LeftJoin<CRMarketingListMember2, On<CRMarketingListMember.marketingListID, Equal<CRMarketingListMember2.marketingListID>,
							And<CRMarketingListMember2.contactID, Equal<Required<Contact.contactID>>>>>,
						Where<CRMarketingListMember.contactID, Equal<Required<Contact.contactID>>,
							And<CRMarketingListMember2.marketingListID, IsNull>>>.Select(graph, target.ContactID, contact.ContactID)
							                                                     .RowCast<CRMarketingListMember>()
							                                                     .Select(mmember => (CRMarketingListMember) Subscriptions.CreateCopy(mmember)))
					{
						mmember.ContactID = target.ContactID;
						Subscriptions.Insert(mmember);
					}

					PXCache Members = graph.Caches[typeof (CRCampaignMembers)];
					foreach (CRCampaignMembers cmember in PXSelectJoin<CRCampaignMembers,
						LeftJoin<CRCampaignMembers2, On<CRCampaignMembers.campaignID, Equal<CRCampaignMembers2.campaignID>,
							And<CRCampaignMembers2.contactID, Equal<Required<Contact.contactID>>>>>,
						Where<CRCampaignMembers2.campaignID, IsNull,
							And<CRCampaignMembers.contactID, Equal<Required<Contact.contactID>>>>>.Select(graph, target.ContactID, contact.ContactID)
							                                                                      .RowCast<CRCampaignMembers>()
							                                                                      .Select(cmember => (CRCampaignMembers) Members.CreateCopy(cmember)))
					{
						cmember.ContactID = target.ContactID;
						Members.Insert(cmember);
					}

					PXCache NWatchers = graph.Caches[typeof (ContactNotification)];
					foreach (ContactNotification watcher in PXSelectJoin<ContactNotification,
						LeftJoin<ContactNotification2, On<ContactNotification.setupID, Equal<ContactNotification2.setupID>,
							And<ContactNotification2.contactID, Equal<Required<Contact.contactID>>>>>,
						Where<ContactNotification2.setupID, IsNull,
							And<ContactNotification.contactID, Equal<Required<Contact.contactID>>>>>.Select(graph, target.ContactID, contact.ContactID)
							                                                                        .RowCast<ContactNotification>()
							                                                                        .Select(watcher => (ContactNotification) NWatchers.CreateCopy(watcher)))
					{
						watcher.NotificationID = null;
						watcher.ContactID = target.ContactID;
						NWatchers.Insert(watcher);
					}

					if (contact.UserID != null)
					{
						Users user = PXSelect<Users, Where<Users.pKID, Equal<Required<Contact.userID>>>>.Select(graph, contact.UserID);
						if (user != null)
						{
							if (!graph.Views.Caches.Contains(typeof(Users)))
								graph.Views.Caches.Add(typeof(Users));

							user.IsApproved = false;
							graph.Caches[typeof (Users)].Update(user);
						}
					}

					graph.Actions.PressSave();					

					PXGraph operGraph = primaryGraph[contact];					
					RunAction(operGraph, contact, "Close as Duplicate");
					operGraph.Actions.PressSave();
				}
				target.DuplicateFound = false;
				RunAction(graph, target, "Mark As Validated");
				graph.Actions.PressSave();
				if (needConvert)
					try
					{										
						RunAction(graph, target, "ConvertToContact");
					}
					catch (PXRedirectRequiredException r)
					{
						redirect = r;
					}				
				scope.Complete();
			}			
			throw redirect ?? new PXRedirectRequiredException(graph, Messages.Contact);	
		}

		private static void RunAction(PXGraph graph, Contact contact, string menu)
		{
				int startRow = 0;
				int totalRows = 1;
				graph.Views[graph.PrimaryView].Select(null, null, new object[] { contact.ContactID }, new[] { typeof(Contact.contactID).Name }, null, null, ref startRow, 1, ref totalRows);
				PXAdapter a = new PXAdapter(graph.Views[graph.PrimaryView])
					{
						StartRow = 0,
						MaximumRows = 1,
						Searches = new object[] { contact.ContactID },
						Menu = menu,
						SortColumns = new[] {typeof (Contact.contactID).Name}
					};
				foreach (var c in graph.Actions["Action"].Press(a)){}
		}

	}
	#endregion

	#region CRDuplicateBAccountList
	public class CRDuplicateBAccountList : PXSelectBase<CRDuplicateRecord>
	{
		#region MergeParams
		[Serializable]
		public class MergeParams : IBqlTable
		{
			#region SourceBAccountID
			public abstract class sourceBAccountID : IBqlField { }
			[PXDBInt]
			[PXDefault(typeof(BAccount.bAccountID))]
			public virtual int? SourceBAccountID { get; set; }
			#endregion

			#region BAccountID
			public abstract class bAccountID : IBqlField { }
			[PXDBInt]
			[PXUIField(DisplayName = "Target", Required = true)]
			[PXDefault(typeof(BAccount.bAccountID))]
			[CRDuplicateBAccountSelector(typeof(MergeParams.sourceBAccountID))]
			public virtual int? BAccountID { get; set; }
			#endregion
		}
		#endregion

		public class MergeBAccount : CRBaseUpdateProcess<MergeBAccount, BAccount, PXMassMergableFieldAttribute, BAccount.classID, CSAnswerType.accountAnswerType> { }
		public class MergeContact : CRBaseUpdateProcess<MergeContact, Contact, PXMassMergableFieldAttribute, Contact.classID, CSAnswerType.leadAnswerType> { }
		public class MergeAddress : CRBaseUpdateProcess<MergeAddress, Address, PXMassMergableFieldAttribute, Contact.classID, CSAnswerType.leadAnswerType> { }

		private const string MergeActionName = "merge";
		private const string MergeParamsViewName = "mergeParams";
		private const string FieldsViewName = "ValueConflicts";

		private readonly PXVirtualTableView<MergeParams> mergeParam;

		public CRDuplicateBAccountList(PXGraph graph)
		{
			_Graph = graph;
			View = new PXView(_Graph, false,
				new Select2<CRDuplicateRecord,
				InnerJoin<BAccount, On<BAccount.defContactID, Equal<CRDuplicateRecord.contactID>>,
				LeftJoin<Contact, On<Contact.contactID, Equal<CRDuplicateRecord.contactID>>,
				LeftJoin<CRLeadContactValidationProcess.Contact2,
							On<CRLeadContactValidationProcess.Contact2.contactID, Equal<CRDuplicateRecord.duplicateContactID>>,
				LeftJoin<BAccountR, On<BAccountR.bAccountID, Equal<CRLeadContactValidationProcess.Contact2.bAccountID>>>>>>,
			 Where<BAccount.bAccountID, Equal<Current<BAccount.bAccountID>>,
				And<CRDuplicateRecord.duplicateContactID, NotEqual<CRDuplicateRecord.contactID>,
				And<CRDuplicateRecord.validationType, Equal<Switch<Case<Where<Contact.contactType, Equal<ContactTypesAttribute.bAccountProperty>,
																											And<CRLeadContactValidationProcess.Contact2.contactType, Equal<ContactTypesAttribute.bAccountProperty>>>, ValidationTypesAttribute.account,
																								Case<Where<Contact.contactType, Equal<ContactTypesAttribute.bAccountProperty>,
																											Or<CRLeadContactValidationProcess.Contact2.contactType, Equal<ContactTypesAttribute.bAccountProperty>>>, ValidationTypesAttribute.leadAccount>>,
																								ValidationTypesAttribute.leadContact>>,
				And<Current<Contact.duplicateFound>, Equal<True>,
				And2<Where<CRLeadContactValidationProcess.Contact2.contactType, NotEqual<ContactTypesAttribute.bAccountProperty>,
								Or<CRLeadContactValidationProcess.Contact2.contactID, Equal<BAccountR.defContactID>>>,
				And<CRDuplicateRecord.score, GreaterEqual<
							Switch<Case<Where<CRLeadContactValidationProcess.Contact2.contactType, Equal<ContactTypesAttribute.bAccountProperty>>,
													Current<CRSetup.accountValidationThreshold>>, 
										Current<CRSetup.leadToAccountValidationThreshold>>>,				 
				 And<Where<Contact.bAccountID, IsNull,
 								Or<CRLeadContactValidationProcess.Contact2.bAccountID, IsNull, 
								Or<Contact.bAccountID, NotEqual<CRLeadContactValidationProcess.Contact2.bAccountID>>>>>>>>>>>>());

			_Graph.Views.Add(MergeParamsViewName, mergeParam = new PXVirtualTableView<MergeParams>(_Graph));
			_Graph.Views.Add(FieldsViewName, new PXView(_Graph, false,
				new Select<FieldValue, Where<FieldValue.attributeID, IsNull>, OrderBy<Asc<FieldValue.order>>>(),
				(PXSelectDelegate)valueConflicts));

			PXDBAttributeAttribute.Activate(_Graph.Caches[typeof(BAccount)]);
			//Init PXVirtual Static constructor
			typeof(FieldValue).GetCustomAttributes(typeof(PXVirtualAttribute), false);

			_Graph.FieldSelecting.AddHandler<FieldValue.value>(delegate(PXCache sender, PXFieldSelectingEventArgs e)
			{
				if (e.Row == null) return;
				e.ReturnState = InitValueFieldState(e.Row as FieldValue);
			});
			_Graph.RowSelected.AddHandler<Contact>(delegate(PXCache sender, PXRowSelectedEventArgs e)
			{
				sender.Graph.Actions[MergeActionName].SetEnabled(e.Row != null && sender.GetStatus(e.Row) != PXEntryStatus.Inserted);
			});
			_Graph.FieldDefaulting.AddHandler<MergeParams.bAccountID>(MergeParams_BAccountID_FieldDefaulting);

			PXNamedAction.AddAction(_Graph, _Graph.Views[_Graph.PrimaryView].Cache.GetItemType(), MergeActionName, Messages.Merge, Merge);
			PXUIFieldAttribute.SetDisplayName<BAccountR.type>(_Graph.Caches<BAccountR>(), Messages.BAccountType);
		}

		protected virtual void MergeParams_BAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			BAccount current = (BAccount)_Graph.Caches<BAccount>().Current;
			List<BAccount> accounts = PXSelectorAttribute.SelectAll<MergeParams.bAccountID>(_Graph.Caches<MergeParams>(), _Graph.Caches<MergeParams>().Current)
				.RowCast<BAccount>()
				.Where(a => a.Type == BAccountType.CustomerType || a.Type == BAccountType.VendorType || a.Type == BAccountType.CombinedType)
				.ToList();
			e.NewValue = current.Type == BAccountType.CustomerType
				|| current.Type == BAccountType.VendorType
				|| current.Type == BAccountType.CombinedType
				|| accounts.Count == 0
				? current.BAccountID : accounts[0].BAccountID;
		}

		public IEnumerable valueConflicts()
		{
			return _Graph.Caches[typeof(FieldValue)].Cached.Cast<FieldValue>().Where(fld => fld.Hidden != true);
		}

		protected PXFieldState InitValueFieldState(FieldValue field)
		{
			Tuple<IEnumerable<string>, IEnumerable<string>> possibleValues = new Tuple<IEnumerable<string>, IEnumerable<string>>(new string[0], new string[0]);
			List<BAccount> bAccounts = PXSelectorAttribute.SelectAll<MergeParams.bAccountID>(_Graph.Caches[typeof (MergeParams)], _Graph.Caches[typeof(MergeParams)].Current).RowCast<BAccount>().ToList();
			if (field.CacheName == typeof(BAccount).FullName)
			{
				possibleValues = CRDuplicateContactList.GetPossibleValues(_Graph, bAccounts, field.Name);
			}
			else if (field.CacheName == typeof (Contact).FullName)
			{
				PXSelectBase<Contact> cmd = new PXSelect<Contact>(_Graph);
				List<int?> defContactIDs = new List<int?>(bAccounts.Select(acc => acc.DefContactID));
				foreach (int? c in defContactIDs)
				{
					cmd.WhereOr<Where<Contact.contactID, Equal<Required<BAccount.defContactID>>>>();					
				}
				possibleValues = CRDuplicateContactList.GetPossibleValues(_Graph, cmd.Select(defContactIDs.Cast<object>().ToArray()).RowCast<Contact>(), field.Name);
			}
			else if (field.CacheName == typeof(Address).FullName)
			{
				PXSelectBase<Address> cmd = new PXSelect<Address>(_Graph);
				List<int?> defAddressIDs = new List<int?>(bAccounts.Select(acc => acc.DefAddressID));
				foreach (int? a in defAddressIDs)
				{
					cmd.WhereOr<Where<Address.addressID, Equal<Required<BAccount.defAddressID>>>>();
				}
				possibleValues = CRDuplicateContactList.GetPossibleValues(_Graph, cmd.Select(defAddressIDs.Cast<object>().ToArray()).RowCast<Address>(), field.Name);
			}

			string[] values = possibleValues.Item1.ToArray();
			string[] labels = possibleValues.Item2.ToArray();

			return PXStringState.CreateInstance(field.Value, null, null, typeof(FieldValue.value).Name,
				false, 0, null, values, labels, null, null);
		}

		protected void InsertPropertyValue(FieldValue field, Dictionary<Type, object> targets)
		{
			Type t = Type.GetType(field.CacheName);
			PXCache cache = _Graph.Caches[t];
			object target = targets[t];

			PXStringState state = InitValueFieldState(field) as PXStringState;

			if (state != null)
			{
				if (state.AllowedValues == null || !state.AllowedValues.Any() || state.AllowedValues.Count() == 1 && field.AttributeID == null)
					return;
				if (state.AllowedValues.Count() == 1)
				{
					field.Hidden = true;
					field.Value = state.AllowedValues[0];
				}
				else if (target != null)
				{
					state.Required = true;
					object value = cache.GetValueExt(target, field.Name);
					if (value is PXFieldState) value = ((PXFieldState)value).Value;
					field.Value = value != null ? value.ToString() : null;
				}
			}
			_Graph.Caches[typeof(FieldValue)].Insert(field);
		}

		protected void FillPropertyValue()
		{
			PXCache cache = _Graph.Caches[typeof(FieldValue)];
			cache.Clear();
			
			PXCache<MergeParams> pcache = _Graph.Caches<MergeParams>();
			pcache.SetDefaultExt<MergeParams.bAccountID>(pcache.Current);
			
			int order = 0;
			List<FieldValue> fields = new List<FieldValue>(MergeBAccount.GetProcessingProperties(_Graph, ref order));
			HashSet<string> fieldNames = new HashSet<string>(fields.Select(f => f.Name));

			fields.AddRange(MergeContact.GetMarkedProperties(_Graph, ref order).Where(fld => fieldNames.Add(fld.Name)));
			fields.AddRange(MergeAddress.GetMarkedProperties(_Graph, ref order).Where(fld => fieldNames.Add(fld.Name)));

			BAccount account = PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.Select(_Graph, ((MergeParams)pcache.Current).BAccountID);
			Contact contact = PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.Select(_Graph, account.DefContactID);
			Address address = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(_Graph, account.DefAddressID);
			Dictionary<Type, object> targets = new Dictionary<Type, object>
			{
				{typeof (BAccount), account},
				{typeof (Contact), contact},
				{typeof (Address), address}
			};
			foreach (FieldValue fld in fields)
			{
				InsertPropertyValue(fld, targets);
			}
			cache.IsDirty = false;
		}

		[PXUIField(DisplayName = Messages.Merge)]
		[PXButton]
		public virtual IEnumerable Merge(PXAdapter adapter)
		{
			MergeParams parms = _Graph.Caches[typeof(MergeParams)].Current as MergeParams;
			if (parms != null && adapter.Get().Cast<BAccount>().Any(account => account.BAccountID != parms.SourceBAccountID))
			{
				_Graph.Caches[typeof(MergeParams)].Delete(parms);
				_Graph.Caches[typeof(MergeParams)].Insert(new MergeParams());
			}

			List<BAccount> baccounts = new List<BAccount>(PXSelectorAttribute.SelectAll<MergeParams.bAccountID>(_Graph.Caches[typeof(MergeParams)], _Graph.Caches[typeof(MergeParams)].Current)
				.RowCast<BAccount>()
				.Select(c => _Graph.Caches[typeof(BAccount)].CreateCopy(c))
				.Cast<BAccount>());
			
			if (baccounts.Count < 2) throw new PXException(Messages.DuplicatesNotSelected);

			if (AskExt((graph, name) => FillPropertyValue()) != WebDialogResult.OK
				|| !mergeParam.VerifyRequired()) return adapter.Get();

			int? targetID = ((MergeParams)_Graph.Caches[typeof(MergeParams)].Current).BAccountID;
			List<FieldValue> values = new List<FieldValue>(_Graph.Caches[typeof(FieldValue)].Cached.Cast<FieldValue>()
				.Select(v => _Graph.Caches[typeof(FieldValue)].CreateCopy(v))
				.Cast<FieldValue>());

			_Graph.Actions.PressSave();
			//PXLongOperation.StartOperation(this, delegate
			{
				MergeBAccounts(
					(int)targetID,
					baccounts,
					values
				);
			}
			//);
			
			return adapter.Get();
		}

		public static void MergeBAccounts(int targetID, List<BAccount> accounts, List<FieldValue> values)
		{
			PXGraph graph = new PXGraph();
			PXPrimaryGraphCollection primaryGraph = new PXPrimaryGraphCollection(graph);
			BAccount target = (BAccount)PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.Select(graph, targetID);

			graph = primaryGraph[target];
			PXCache cache = graph.Caches[typeof(BAccount)];
			string entityType = CSAnswerType.GetAnswerType(cache.GetItemType());
			string entityID = CSAnswerType.GetEntityID(cache.GetItemType());

			PXView view = new PXView(graph, false, BqlCommand.CreateInstance(BqlCommand.Compose(
				typeof(Select<,>), 
					cache.GetItemType(), 
					typeof(Where<,>),
						Type.GetType(string.Format("{0}+{1}", cache.GetItemType().FullName, typeof(BAccount.bAccountID).Name)),
						typeof(Equal<>), typeof(Required<>), typeof(BAccount.bAccountID)
			)));
			object copy = view.SelectSingle(targetID);

			Contact targetContact = PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.Select(graph, target.DefContactID);
			Address targetAddress = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(graph, target.DefAddressID);

			Dictionary<Type, object> targets = new Dictionary<Type, object> {{typeof (BAccount), copy}, {typeof(Contact), targetContact}, {typeof(Address), targetAddress}};

			foreach (FieldValue fld in values)
			{
				if (fld.AttributeID == null)
				{
					Type type = Type.GetType(fld.CacheName);
					PXFieldState state = (PXFieldState)graph.Caches[type].GetStateExt(targets[type], fld.Name);
					if (state == null || !Equals(state.Value, fld.Value))
					{
						graph.Caches[type].SetValueExt(targets[type], fld.Name, fld.Value);
						targets[type] = graph.Caches[type].CreateCopy(graph.Caches[type].Update(targets[type]));
					}
				}
				else
				{
					PXCache attrCache = graph.Caches[typeof(CSAnswers)];
					CSAnswers attr = new CSAnswers
					{
						AttributeID = fld.AttributeID,
						EntityID = cache.GetValue(copy, entityID) as int?,
						EntityType = entityType,
						Value = fld.Value
					};
					attrCache.Update(attr);
				}
			}

			target = (BAccount) targets[typeof (BAccount)];
			using (PXTransactionScope scope = new PXTransactionScope())
			{
				foreach (BAccount baccount in accounts.Where(a => a.BAccountID != targetID))
				{
					if (baccount.Type != BAccountType.ProspectType)
						throw new PXException(Messages.MergeNonProspect);

					int? defContactID = baccount.DefContactID;
					PXCache Contacts = graph.Caches[typeof(Contact)];
					foreach (Contact contact in PXSelect<Contact, Where<Contact.bAccountID, Equal<Current<BAccount.bAccountID>>>>
						.SelectMultiBound(graph, new object[] { baccount })
						.RowCast<Contact>()
						.Where(c => c.ContactID != defContactID)
						.Select(c => (Contact)Contacts.CreateCopy(c)))
					{
						contact.BAccountID = target.BAccountID;
						Contacts.Update(contact);
					}

					PXCache Cases = graph.Caches[typeof(CRCase)];
					foreach (CRCase cas in PXSelect<CRCase, Where<CRCase.customerID, Equal<Current<BAccount.bAccountID>>>>
						.SelectMultiBound(graph, new object[] { baccount })
						.RowCast<CRCase>()
						.Select(cas => (CRCase)Cases.CreateCopy(cas)))
					{
						cas.CustomerID = target.BAccountID;
						Cases.Update(cas);
					}

					PXCache Opportunities = graph.Caches[typeof(CROpportunity)];
					foreach (CROpportunity opp in PXSelect<CROpportunity, Where<CROpportunity.bAccountID, Equal<Current<BAccount.bAccountID>>>>
						.SelectMultiBound(graph, new object[] { baccount })
						.RowCast<CROpportunity>()
						.Select(opp => (CROpportunity)Opportunities.CreateCopy(opp)))
					{
						opp.BAccountID = target.BAccountID;
						opp.LocationID = target.DefLocationID;
						Opportunities.Update(opp);
					}

					PXCache Relations = graph.Caches[typeof(CRRelation)];
					foreach (CRRelation rel in PXSelectJoin<CRRelation,
						LeftJoin<CRRelation2, 
							On<CRRelation.entityID, Equal<CRRelation2.entityID>,
							And<CRRelation.role, Equal<CRRelation2.role>,
							And<CRRelation2.refNoteID, Equal<Required<BAccount.noteID>>>>>>,
						Where<CRRelation2.entityID, IsNull,
							And<CRRelation.refNoteID, Equal<Required<BAccount.noteID>>>>>
						.Select(graph, target.NoteID, baccount.NoteID)
						.RowCast<CRRelation>()
						.Select(rel => (CRRelation)Relations.CreateCopy(rel)))
					{
						rel.RelationID = null;
						rel.RefNoteID = target.NoteID;
						Relations.Insert(rel);
					}
					graph.Actions.PressSave();
					PXGraph dupGraph = primaryGraph[baccount];
					dupGraph.Caches[typeof(BAccount)].Delete(baccount);
					dupGraph.Actions.PressSave();
				}				
				RunAction(graph, target, "MarkAsValidated");
				graph.Actions.PressSave();			
				scope.Complete();
			}
			throw new PXRedirectRequiredException(graph, Messages.BAccount);
		}

		private static void RunAction(PXGraph graph, BAccount baccount, string menu)
		{
			int startRow = 0;
			int totalRows = 1;
			graph.Views[graph.PrimaryView].Select(null, null, new object[] { baccount.BAccountID }, new[] { typeof(BAccount.bAccountID).Name }, null, null, ref startRow, 1, ref totalRows);
			PXAdapter a = new PXAdapter(graph.Views[graph.PrimaryView])
			{
				StartRow = 0,
				MaximumRows = 1,
				Searches = new object[] { baccount.BAccountID },
				Menu = menu,
				SortColumns = new[] { typeof(BAccount.bAccountID).Name }
			};
			foreach (var c in graph.Actions["Action"].Press(a)) { }
		}

	}
	#endregion
	[Serializable]
	public partial class CRRelation2: CRRelation
	{
		public new abstract class refNoteID : IBqlField { }
		public new abstract class entityID : IBqlField { }
		public new abstract class role : IBqlField { }
	}

	[Serializable]
	public partial class CRMarketingListMember2 : CRMarketingListMember
	{
		public new abstract class contactID : IBqlField { }
		public new abstract class marketingListID : IBqlField { }
	}

	[Serializable]
	public partial class CRCampaignMembers2 : CRCampaignMembers
	{
		public new abstract class contactID : IBqlField { }
		public new abstract class campaignID : IBqlField { }
	}

	[Serializable]
	public partial class ContactNotification2 : ContactNotification
	{
		public new abstract class contactID : IBqlField { }
		public new abstract class setupID : IBqlField { }
	}

	public class PXVirtualTableView<TTable> : PXView 
		where TTable : IBqlTable
	{
		public PXVirtualTableView(PXGraph graph) : base(graph, false, new Select<TTable>())
		{
			_Delegate = (PXSelectDelegate)Get;
			_Graph.Defaults[_Graph.Caches[typeof(TTable)].GetItemType()] = getFilter;
			_Graph.RowPersisting.AddHandler(typeof(TTable), persisting);
		}
		public IEnumerable Get()
		{
			PXCache cache = _Graph.Caches[typeof(TTable)];
			cache.AllowInsert = true;
			cache.AllowUpdate = true;
			object curr = cache.Current;
			if (curr != null && cache.Locate(curr) == null)
			{
				try
				{
					curr = cache.Insert(curr);
				}
				catch
				{
					cache.SetStatus(curr, PXEntryStatus.Inserted);
				}
			}
			yield return curr;
			cache.IsDirty = false;
		}

		private TTable current;
		private bool _inserting = false;
		private object getFilter()
		{
			PXCache cache = _Graph.Caches[typeof(TTable)];

			if (!_inserting)
			{
				try
				{
					_inserting = true;
					if (current == null)
					{
						current = (TTable)(cache.Insert() ?? cache.Locate(cache.CreateInstance()));
						cache.IsDirty = false;
					}
					else if (cache.Locate(current) == null)
					{
						try
						{
							current = (TTable)cache.Insert(current);
						}
						catch
						{
							cache.SetStatus(current, PXEntryStatus.Inserted);
						}
						cache.IsDirty = false;
					}
				}
				finally
				{
					_inserting = false;
				}
			}
			return current;
		}
		private static void persisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		public bool VerifyRequired()
		{
			return VerifyRequired(false);
		}
		public virtual bool VerifyRequired(bool suppressError)
		{
			Cache.RaiseRowSelected(Cache.Current);
			bool result = true;
			PXRowPersistingEventArgs e = new PXRowPersistingEventArgs(PXDBOperation.Insert, Cache.Current);
			foreach (var field in Cache.Fields)
			{
				foreach (var defAttr in Cache.GetAttributes(Cache.Current, field).OfType<PXDefaultAttribute>())
				{
					defAttr.RowPersisting(Cache, e);
					bool error = !string.IsNullOrEmpty(PXUIFieldAttribute.GetError(Cache, Cache.Current, field));
					if (error) result = false;

					if (suppressError && error)
					{
						Cache.RaiseExceptionHandling(field, Cache.Current, null, null);
						return false;
					}
				}
			}
			return result;
		}

	}

}
