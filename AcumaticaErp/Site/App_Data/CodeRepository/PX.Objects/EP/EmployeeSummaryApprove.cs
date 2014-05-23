using System;
using System.Collections;
using PX.Data.EP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.PM;
using PX.Data;
using PX.TM;


namespace PX.Objects.EP
{
	public class EmployeeSummaryApprove : PXGraph<EmployeeSummaryApprove>
	{
		#region Selects

		public PXFilter<EPSummaryFilter> Filter;
		public PXSelectJoin<EPSummaryApprove
			, LeftJoin<EPEarningType, On<EPEarningType.typeCD, Equal<EPSummaryApprove.earningType>>>
			, Where2<Where<EPSummaryApprove.taskApproverID, Equal<Current<EPSummaryFilter.approverID>>, Or<Where<EPSummaryApprove.taskApproverID, IsNull, And<EPSummaryApprove.approverID, Equal<Current<EPSummaryFilter.approverID>>>>>>
				, And2<Where<EPSummaryApprove.weekId, GreaterEqual<Current<EPSummaryFilter.fromWeek>>, Or<Current<EPSummaryFilter.fromWeek>, IsNull>>
					, And2<Where<EPSummaryApprove.weekId, LessEqual<Current<EPSummaryFilter.tillWeek>>, Or<Current<EPSummaryFilter.tillWeek>, IsNull>>
						, And2<Where<EPSummaryApprove.projectID, Equal<Current<EPSummaryFilter.projectID>>, Or<Current<EPSummaryFilter.projectID>, IsNull>>
							, And2<Where<EPSummaryApprove.projectTaskID, Equal<Current<EPSummaryFilter.projectTaskID>>, Or<Current<EPSummaryFilter.projectTaskID>, IsNull>>
								, And<Where<EPSummaryApprove.employeeID, Equal<Current<EPSummaryFilter.employeeID>>, Or<Current<EPSummaryFilter.employeeID>, IsNull>>
									>
								>
							>
						>
					>
				>
			> Summary;
		public PXSelect<EPActivity> activity;

		#endregion

		#region Actions

		public PXSave<EPSummaryFilter> Save;
		public PXCancel<EPSummaryFilter> Cancel;
		public PXAction<EPSummaryFilter> approveAll;
		public PXAction<EPSummaryFilter> rejectAll;
		public PXAction<EPSummaryFilter> viewDetails;

		[PXUIField(DisplayName = "View Time Card")]
		[PXButton()]
		public virtual IEnumerable ViewDetails(PXAdapter adapter)
		{
			var row = Summary.Current;
			if (row != null)
			{
				PXRedirectHelper.TryRedirect(this, PXSelectorAttribute.Select<EPSummaryApprove.timeCardCD>(Summary.Cache, row), PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.ApproveAll)]
		[PXButton()]
		public virtual IEnumerable ApproveAll(PXAdapter adapter)
		{
			if (Summary.Current == null || Filter.View.Ask(Messages.ApproveAll, MessageButtons.YesNo) != WebDialogResult.Yes)
			{
				return adapter.Get();
			}

			foreach (EPSummaryApprove item in Summary.Select())
			{
				item.IsApprove = true;
				item.IsReject = false;
				Summary.Cache.Update(item);
			}
			Persist();
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.RejectAll)]
		[PXButton()]
		public virtual IEnumerable RejectAll(PXAdapter adapter)
		{
			if (Summary.Current == null || Filter.View.Ask(Messages.RejectAll, MessageButtons.YesNo) != WebDialogResult.Yes)
			{
				return adapter.Get();
			}

			foreach (EPSummaryApprove item in Summary.Select())
			{
				item.IsApprove = false;
				item.IsReject = true;
				Summary.Cache.Update(item);
			}
			Persist();
			return adapter.Get();
		}

		#endregion

		#region Event handlers

		protected virtual void EPSummaryFilter_FromWeek_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPSummaryFilter row = (EPSummaryFilter)e.Row;
			if (row != null && e.ExternalCall && row.FromWeek > row.TillWeek)
				row.TillWeek = row.FromWeek;
		}

		protected virtual void EPSummaryFilter_TillWeek_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPSummaryFilter row = (EPSummaryFilter)e.Row;
			if (row != null && e.ExternalCall && row.FromWeek != null && row.FromWeek > row.TillWeek)
				row.FromWeek = row.TillWeek;
		}

		protected virtual void EPSummaryFilter_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (Summary.Cache.IsDirty && Filter.View.Ask(ActionsMessages.ConfirmationMsg, MessageButtons.YesNo) != WebDialogResult.Yes)
				e.Cancel = true;
			else
				Summary.Cache.Clear();
		}

		protected virtual void EPSummaryApprove_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPSummaryApprove row = (EPSummaryApprove)e.Row;
			if (row == null)
				return;

			PXUIFieldAttribute.SetEnabled(sender, row, false);
			if (row.HasComplite != null || row.HasReject != null || row.HasApprove != null)
			{
				PXUIFieldAttribute.SetEnabled<EPSummaryApprove.isApprove>(sender, row, true);
				PXUIFieldAttribute.SetEnabled<EPSummaryApprove.isReject>(sender, row, true);
			}
			if (row.HasOpen != null)
				sender.RaiseExceptionHandling<EPSummaryApprove.weekId>(row, null, new PXSetPropertyException(EP.Messages.HasOpenActivity, PXErrorLevel.RowWarning));
		}

		protected virtual void EPSummaryFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPSummaryFilter row = (EPSummaryFilter)e.Row;
			if (row != null)
			{
				row.RegularTime = 0;
				row.RegularOvertime = 0;
				row.RegularTotal = 0;
				foreach (PXResult<EPSummaryApprove, EPEarningType> item in Summary.Select())
				{
					EPSummaryApprove rowActivity = (EPSummaryApprove)item;
					EPEarningType rowEarningType = (EPEarningType)item;

					if (rowEarningType.IsOvertime == true)
					{
						row.RegularOvertime += rowActivity.TimeSpent.GetValueOrDefault(0);
					}
					else
					{
						row.RegularTime += rowActivity.TimeSpent.GetValueOrDefault(0);
					}

					row.RegularTotal = row.RegularTime + row.RegularOvertime;
				}
			}

		}


		protected virtual void EPSummaryApprove_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			EPSummaryApprove row = (EPSummaryApprove)e.Row;
			if (row == null)
				return;
			if (row.IsApprove == true && row.IsReject == true)
				return;

			PXResultset<EPActivity> activityList = PXSelectJoin<
					EPActivity
					, InnerJoin<EPTimeCard, On<Required<EPTimeCardSummary.timeCardCD>, Equal<EPTimeCard.timeCardCD>, And<EPTimeCard.weekId, Equal<EPActivity.weekID>>>
						, InnerJoin<EPEmployee, On<EPTimeCard.employeeID, Equal<EPEmployee.bAccountID>, And<EPEmployee.userID, Equal<EPActivity.owner>>>
							>
						>
						, Where<
							Required<EPTimeCardSummary.earningType>, Equal<EPActivity.earningTypeID>
							, And<Required<EPTimeCardSummary.projectID>, Equal<EPActivity.projectID>
								, And<Required<EPTimeCardSummary.projectTaskID>, Equal<EPActivity.projectTaskID>
									, And2<Where<Required<EPTimeCardSummary.parentTaskID>, Equal<EPActivity.parentTaskID>, Or<Required<EPTimeCardSummary.parentTaskID>, IsNull, And<EPActivity.parentTaskID, IsNull>>>
										, And<EPActivity.trackTime, Equal<True>
											, And<EPActivity.classID, NotEqual<CRActivityClass.emailRouting>
												, And<EPActivity.classID, NotEqual<CRActivityClass.task>
													, And<EPActivity.classID, NotEqual<CRActivityClass.events>
														, And<EPActivity.released, Equal<False>
															, And<EPActivity.trackTime, Equal<True>
																, And<EPActivity.classID, NotEqual<CRActivityClass.emailRouting>
																	, And<EPActivity.classID, NotEqual<CRActivityClass.task>
																		, And<EPActivity.classID, NotEqual<CRActivityClass.events>
																			, And<EPActivity.approverID, IsNotNull
																					, And<Where<EPActivity.uistatus, NotEqual<ActivityStatusListAttribute.canceled>, And<EPActivity.uistatus, NotEqual<ActivityStatusListAttribute.open>>>
																					>
																				>
																			>
																		>
																	>
																>
															>
														>
													>
												>
											>
										>
									>
								>
							>
						>.Select(this, row.TimeCardCD, row.EarningType, row.ProjectID, row.ProjectTaskID, row.ParentTaskID);

			foreach(EPActivity act in activityList)
			{

				if (row.IsApprove == true)
				{
					if (act.UIStatus != ActivityStatusListAttribute.Approved)
					{
						activity.Cache.SetValueExt<EPActivity.uistatus>(act, ActivityStatusListAttribute.Approved);
						activity.Cache.SetValueExt<EPActivity.approvedDate>(act, Accessinfo.BusinessDate);
					}
				}
				else if (row.IsReject == true)
				{
					if(act.UIStatus != ActivityStatusListAttribute.Rejected)
						activity.Cache.SetValueExt<EPActivity.uistatus>(act, ActivityStatusListAttribute.Rejected);
				}
				activity.Cache.Persist(act, PXDBOperation.Update);
			}
		}

		#endregion

	}

	#region EPSummaryFilter
	[Serializable]
	public class EPSummaryFilter : IBqlTable
	{

		#region ApproverID
		public abstract class approverID : PX.Data.IBqlField
		{
		}
		protected Int32? _ApproverID;
		[PXDBInt]
		[PXSubordinateSelector]
		[PXDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>))]
		[PXUIField(DisplayName = "Approver", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? ApproverID { get; set; }
		#endregion

		#region EmployeeID
		public abstract class employeeID : PX.Data.IBqlField
		{
		}
		[PXDBInt]
		[PXSubordinateSelector]
		[PXUIField(DisplayName = "Employee")]
		public virtual Int32? EmployeeID { set; get; }
		#endregion

		#region FromWeek
		public abstract class fromWeek : IBqlField
		{
		}
		[PXDBInt]
		[PXWeekSelector2(DescriptionField = typeof(EPWeekRaw.shortDescription))]
		[PXUIField(DisplayName = "From Week", Visibility = PXUIVisibility.Visible)]
		public virtual Int32? FromWeek { set; get; }
		#endregion

		#region TillWeek
		public abstract class tillWeek : IBqlField
		{
		}
		[PXDBInt]
		[PXWeekSelector2(DescriptionField = typeof(EPWeekRaw.shortDescription))]
		[PXUIField(DisplayName = "Until Week", Visibility = PXUIVisibility.Visible)]
		public virtual Int32? TillWeek { set; get; }
		#endregion

		#region ProjectID
		public abstract class projectID : IBqlField
		{
		}
		[ActiveProject(DisplayName = PM.Messages.Project)]
		public virtual Int32? ProjectID { set; get; }
		#endregion

		#region ProjectTaskID
		public abstract class projectTaskID : IBqlField
		{
		}
		[ProjectTask(typeof(EPSummaryFilter.projectID))]
		public virtual Int32? ProjectTaskID { set; get; }
		#endregion

		#region Total
		#region Regular
		public abstract class regularTime : IBqlField
		{
		}
		[PXInt]
		[PXUIField(DisplayName = "Spent", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Int32? RegularTime { set; get; }
		#endregion
		#region RegularOvertime
		public abstract class regularOvertime : IBqlField
		{
		}
		[PXInt]
		[PXUIField(DisplayName = "Regular Overtime", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Int32? RegularOvertime { set; get; }
		#endregion
		#region RegularTotal
		public abstract class regularTotal : IBqlField
		{
		}
		[PXInt]
		[PXUIField(DisplayName = "Regular Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Int32? RegularTotal { set; get; }
		#endregion

		#endregion

	}
	#endregion



	public partial class EPActivityReject : EPActivity
	{
		public new class uistatus : PX.Data.IBqlField { }
		public new class taskID : IBqlField { }
	}

	public partial class EPActivityComplite : EPActivity
	{
		public new class uistatus : PX.Data.IBqlField { }
		public new class taskID : IBqlField { }
	}

	public partial class EPActivityOpen : EPActivity
	{
		public new class uistatus : PX.Data.IBqlField { }
		public new class taskID : IBqlField { }
	}

	#region EPSummaryApprove

	[Serializable]
	[PXProjection(typeof(
		Select5<EPTimeCardSummary
			, InnerJoin<EPTimeCard, On<EPTimeCardSummary.timeCardCD, Equal<EPTimeCard.timeCardCD>>
				, InnerJoin<EPEmployee, On<EPTimeCard.employeeID, Equal<EPEmployee.bAccountID>>
					, InnerJoin<EPActivity
						, On<
							EPTimeCardSummary.earningType, Equal<EPActivity.earningTypeID>
							, And<EPTimeCardSummary.projectID, Equal<EPActivity.projectID>
								, And<EPTimeCardSummary.projectID, Equal<EPActivity.projectID>
									, And<EPTimeCardSummary.projectTaskID, Equal<EPActivity.projectTaskID>
										, And2<Where<EPTimeCardSummary.parentTaskID, Equal<EPActivity.parentTaskID>, Or<EPTimeCardSummary.parentTaskID, IsNull, And<EPActivity.parentTaskID, IsNull>>>
											, And<EPEmployee.userID, Equal<EPActivity.owner>
												, And<EPTimeCard.weekId, Equal<EPActivity.weekID>
													, And<EPActivity.released, Equal<False>
														, And<EPActivity.trackTime, Equal<True>
															, And<EPActivity.classID, NotEqual<CRActivityClass.emailRouting>
																, And<EPActivity.classID, NotEqual<CRActivityClass.task>
																	, And<EPActivity.classID, NotEqual<CRActivityClass.events>
																		, And<EPActivity.approverID, IsNotNull
																			, And<Where<EPActivity.uistatus, NotEqual<ActivityStatusListAttribute.canceled>/*, And<EPActivity.uistatus, NotEqual<ActivityStatusListAttribute.open>>*/>
																				>
																			>
																		>
																	>
																>
															>
														>
													>
												>
											>
										>
									>
								>
							>
						, LeftJoin<EPActivityApprove, On<EPActivityApprove.taskID, Equal<EPActivity.taskID>, And<EPActivityApprove.uistatus, Equal<ActivityStatusListAttribute.approved>>>
							, LeftJoin<EPActivityReject, On<EPActivityReject.taskID, Equal<EPActivity.taskID>, And<EPActivityReject.uistatus, Equal<ActivityStatusListAttribute.rejected>>>
								, LeftJoin<EPActivityComplite, On<EPActivityComplite.taskID, Equal<EPActivity.taskID>, And<EPActivityComplite.uistatus, Equal<ActivityStatusListAttribute.completed>>>
									, LeftJoin<EPActivityOpen, On<EPActivityOpen.taskID, Equal<EPActivity.taskID>, And<EPActivityOpen.uistatus, Equal<ActivityStatusListAttribute.open>>>
										, LeftJoin<PMProject, On<PMProject.contractID, Equal<EPTimeCardSummary.projectID>>
											, LeftJoin<PMTask, On<PMTask.taskID, Equal<EPTimeCardSummary.projectTaskID>>
												>
											>
										>
									>
								>
							>
						>
					>
				>
			, Aggregate<
				GroupBy<EPTimeCardSummary.timeCardCD
				, GroupBy<EPTimeCardSummary.lineNbr
				, GroupBy<EPTimeCardSummary.earningType
				, GroupBy<EPTimeCardSummary.parentTaskID
				, GroupBy<EPTimeCardSummary.projectID
				, GroupBy<EPTimeCardSummary.projectTaskID
				, GroupBy<EPTimeCardSummary.mon
				, GroupBy<EPTimeCardSummary.tue
				, GroupBy<EPTimeCardSummary.wed
				, GroupBy<EPTimeCardSummary.thu
				, GroupBy<EPTimeCardSummary.fri
				, GroupBy<EPTimeCardSummary.sat
				, GroupBy<EPTimeCardSummary.sun
				, GroupBy<EPTimeCardSummary.isBillable
				, GroupBy<EPTimeCardSummary.description
				, GroupBy<EPTimeCardSummary.noteID
				, GroupBy<EPTimeCard.weekId
				, GroupBy<PMProject.approverID
				, GroupBy<PMTask.approverID
				, GroupBy<EPEmployee.userID
				, Max<EPActivityComplite.uistatus
				, Max<EPActivityApprove.uistatus
				, Max<EPActivityReject.uistatus
				, Max<EPActivityOpen.uistatus
						>>>>>>>>>>>>>>>>>>>>>>>
					>
				>
			>
		), Persistent = false)]
	public partial class EPSummaryApprove : IBqlTable
	{
		#region Approve
		public abstract class isApprove : IBqlField
		{
		}
		protected bool? _IsApprove;
		[PXBool]
		[PXUIField(DisplayName = "Approve")]
		public virtual bool? IsApprove
		{
			get
			{
				return _IsApprove ?? HasApprove != null && HasComplite == null && HasReject == null;
			}
			set
			{
				_IsApprove = value;
				_IsReject = !value;
			}
		}
		#endregion
		#region Reject
		public abstract class isReject : IBqlField
		{
		}
		protected bool? _IsReject;
		[PXBool]
		[PXUIField(DisplayName = "Reject")]
		public virtual bool? IsReject
		{
			get
			{
				return _IsReject ?? HasApprove == null && HasComplite == null && HasReject != null;
			}
			set
			{
				_IsReject = value;
				_IsApprove = !value;
			}
		}
		#endregion

		#region TimeCardCD

		public abstract class timeCardCD : IBqlField { }

		[PXDBDefault(typeof(EPTimeCard.timeCardCD))]
		[PXDBString(10, IsKey = true, BqlField = typeof(EPTimeCardSummary.timeCardCD))]
		[PXUIField(DisplayName = "Time Card")]
		[PXSelector(typeof(Search<EPTimeCard.timeCardCD>))]
		public virtual String TimeCardCD { get; set; }

		#endregion
		#region LineNbr
		public abstract class lineNbr : IBqlField { }

		[PXDBInt(IsKey = true, BqlField = typeof(EPTimeCardSummary.lineNbr))]
		[PXLineNbr(typeof(EPTimeCard.summaryLineCntr))]
		[PXUIField(Visible = false)]
		public virtual Int32? LineNbr { get; set; }
		#endregion
		#region EarningType
		public abstract class earningType : IBqlField { }
		[PXDBString(2, IsFixed = true, IsUnicode = false, InputMask = ">LL", BqlField = typeof(EPTimeCardSummary.earningType))]
		[PXDefault(typeof(Search<EPSetup.regularHoursType>))]
		[PXSelector(typeof(EPEarningType.typeCD))]
		[PXUIField(DisplayName = "Earning Type")]
		public virtual string EarningType { get; set; }
		#endregion

		#region ParentTaskID
		public abstract class parentTaskID : PX.Data.IBqlField
		{
		}
		protected Int32? _ParentTaskID;

		[PXUIField(DisplayName = "Task ID")]
		[PXDBInt(BqlField = typeof(EPTimeCardSummary.parentTaskID))]
		[PXSelector(typeof(Search2<EPActivity.taskID, InnerJoin<EPEmployee, On<EPEmployee.userID, Equal<EPActivity.owner>>>,
			Where<EPEmployee.bAccountID, Equal<Current<EPTimeCard.employeeID>>, And<EPActivity.classID, Equal<CRActivityClass.task>>>>))]
		public virtual int? ParentTaskID
		{
			get
			{
				return this._ParentTaskID;
			}
			set
			{
				this._ParentTaskID = value;
			}
		}
		#endregion

		#region ProjectID
		public abstract class projectID : IBqlField { }
		[ProjectDefault(GL.BatchModule.EP, ForceProjectExplicitly = true)]
		[EPTimeCardActiveProjectAttribute(BqlField = typeof(EPTimeCardSummary.projectID))]
		public virtual Int32? ProjectID { get; set; }
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : IBqlField { }
		[ActiveProjectTask(typeof(EPTimeCardSummary.projectID), GL.BatchModule.EP, AllowNullIfContract = true, DisplayName = "Project Task", BqlField = typeof(EPTimeCardSummary.projectTaskID))]
		public virtual Int32? ProjectTaskID { get; set; }
		#endregion
		#region TimeSpent
		public abstract class timeSpent : IBqlField { }

		[PXInt]
		[PXUIField(DisplayName = "Time Spent", Enabled = false)]
		public virtual Int32? TimeSpent
		{
			get
			{
				return Mon.GetValueOrDefault() +
					   Tue.GetValueOrDefault() +
					   Wed.GetValueOrDefault() +
					   Thu.GetValueOrDefault() +
					   Fri.GetValueOrDefault() +
					   Sat.GetValueOrDefault() +
					   Sun.GetValueOrDefault();
			}
		}
		#endregion
		#region Sun
		public abstract class sun : IBqlField { }
		[PXTimeList]
		[PXDBInt(BqlField = typeof(EPTimeCardSummary.sun))]
		[PXUIField(DisplayName = "Sun")]
		public virtual Int32? Sun { get; set; }
		#endregion
		#region Mon
		public abstract class mon : IBqlField { }

		protected int? _Mon;
		[PXTimeList]
		[PXDBInt(BqlField = typeof(EPTimeCardSummary.mon))]
		[PXUIField(DisplayName = "Mon")]
		public virtual Int32? Mon
		{
			get { return _Mon; }
			set { _Mon = value; }
		}
		#endregion
		#region Tue
		public abstract class tue : IBqlField { }
		[PXTimeList]
		[PXDBInt(BqlField = typeof(EPTimeCardSummary.tue))]
		[PXUIField(DisplayName = "Tue")]
		public virtual Int32? Tue { get; set; }
		#endregion
		#region Wed
		public abstract class wed : IBqlField { }
		[PXTimeList]
		[PXDBInt(BqlField = typeof(EPTimeCardSummary.wed))]
		[PXUIField(DisplayName = "Wed")]
		public virtual Int32? Wed { get; set; }
		#endregion
		#region Thu
		public abstract class thu : IBqlField { }
		[PXTimeList]
		[PXDBInt(BqlField = typeof(EPTimeCardSummary.thu))]
		[PXUIField(DisplayName = "Thu")]
		public virtual Int32? Thu { get; set; }
		#endregion
		#region Fri
		public abstract class fri : IBqlField { }
		[PXTimeList]
		[PXDBInt(BqlField = typeof(EPTimeCardSummary.fri))]
		[PXUIField(DisplayName = "Fri")]
		public virtual Int32? Fri { get; set; }
		#endregion
		#region Sat
		public abstract class sat : IBqlField { }
		[PXTimeList]
		[PXDBInt(BqlField = typeof(EPTimeCardSummary.sat))]
		[PXUIField(DisplayName = "Sat")]
		public virtual Int32? Sat { get; set; }
		#endregion
		#region IsBillable
		public abstract class isBillable : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsBillable;
		[PXDBBool(BqlField = typeof(EPTimeCardSummary.isBillable))]
		[PXDefault()]
		[PXUIField(DisplayName = "Billable")]
		public virtual Boolean? IsBillable
		{
			get
			{
				return this._IsBillable;
			}
			set
			{
				this._IsBillable = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.IBqlField
		{
		}
		protected String _Description;
		[PXDBString(255, IsUnicode = true, BqlField = typeof(EPTimeCardSummary.description))]
		[PXUIField(DisplayName = "Description")]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion

		#region hasApprove
		public abstract class hasApprove : PX.Data.IBqlField { }
		[PXDBString(2, IsFixed = true, BqlField = typeof(EPActivityApprove.uistatus))]
		public virtual string HasApprove { get; set; }
		#endregion

		#region hasReject
		public abstract class hasReject : PX.Data.IBqlField { }
		[PXDBString(2, IsFixed = true, BqlField = typeof(EPActivityReject.uistatus))]
		public virtual string HasReject { get; set; }
		#endregion

		#region hasComplite
		public abstract class hasComplite : PX.Data.IBqlField { }
		[PXDBString(2, IsFixed = true, BqlField = typeof(EPActivityComplite.uistatus))]
		public virtual string HasComplite { get; set; }
		#endregion

		#region hasComplite
		public abstract class hasOpen : PX.Data.IBqlField { }
		[PXDBString(2, IsFixed = true, BqlField = typeof(EPActivityOpen.uistatus))]
		public virtual string HasOpen { get; set; }
		#endregion

		#region WeekID
		public abstract class weekId : IBqlField { }
		[PXDBInt(BqlField = typeof(EPTimeCard.weekId))]
		[PXUIField(DisplayName = "Week")]
		[PXWeekSelector2(SubstituteKey = typeof(EPWeekRaw.shortDescription))]
		public virtual Int32? WeekID { get; set; }
		#endregion

		#region ApproverID
		public abstract class approverID : PX.Data.IBqlField
		{
		}
		private Int32? _ProjectApproverID;
		[PXDBInt(BqlField = typeof(PMProject.approverID))]
		[PXEPEmployeeSelector]
		[PXUIField(DisplayName = "Approver", Visibility = PXUIVisibility.SelectorVisible)]
		public Int32? ApproverID {
			get { return _TaskApproverID ?? _ProjectApproverID; }
			set { _ProjectApproverID = value; }
		}
		#endregion

		#region TaskApproverID
		public abstract class taskApproverID : PX.Data.IBqlField
		{
		}

		private Int32? _TaskApproverID;
		[PXDBInt(BqlField = typeof(PMTask.approverID))]
		[PXEPEmployeeSelector]
		[PXUIField(DisplayName = "Task Project Manager", Visibility = PXUIVisibility.SelectorVisible)]
		public Int32? TaskApproverID
		{
			get { return _TaskApproverID; }
			set { _TaskApproverID = value; }
		}
		#endregion

		#region OwnerID
		public abstract class ownerID : PX.Data.IBqlField
		{
		}
		[PXDBGuid(BqlField = typeof(EPEmployee.userID))]
		[PXUIField(DisplayName = "Employee")]
		[PX.TM.PXOwnerSelector]
		public virtual Guid? OwnerID { set; get; }
		#endregion

		#region EmployeeID
		public abstract class employeeID : PX.Data.IBqlField
		{
		}
		[PXDBInt(BqlField = typeof(EPEmployee.bAccountID))]
		[PXSelector(typeof(Search<EPEmployee.bAccountID>), DescriptionField = typeof(EPEmployee.acctName))]
		[PXUIField(DisplayName = "Employee")]
		public virtual Int32? EmployeeID { set; get; }
		#endregion

	}
	#endregion


}
