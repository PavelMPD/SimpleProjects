using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data.EP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.SM;
using PX.Data;
using PX.TM;
using OwnedFilter = PX.Objects.CR.OwnedFilter;


namespace PX.Objects.EP
{
	public class EmployeeActivitiesEntry : PXGraph<EmployeeActivitiesEntry>
	{
		#region Selects
		[PXHidden]
		public PXFilter<Contract> dummyContract;
		[PXHidden]
		public PXFilter<EPActivity> dummy;
		public PXFilter<EPActivityFilter> Filter;
		public PXSelectJoin<
				EPActivityApprove
				, LeftJoin<EPEarningType, On<EPEarningType.typeCD, Equal<EPActivityApprove.earningTypeID>>
					, LeftJoin<CRCase, On<CRCase.noteID, Equal<EPActivityApprove.refNoteID>>
							, LeftJoin<Contract, On<CRCase.contractID, Equal<Contract.contractID>>>
						>
					>
                , Where<EPActivityApprove.owner, Equal<Current<EPActivityFilter.ownerID>>
					, And<EPActivityApprove.isCorrected, Equal<False>
						, And<EPActivityApprove.trackTime, Equal<True>
							, And<EPActivityApprove.classID, NotEqual<CRActivityClass.emailRouting>
				                , And<EPActivityApprove.classID, NotEqual<CRActivityClass.task>
									, And<EPActivityApprove.classID, NotEqual<CRActivityClass.events>>
									>
								>
							>
						>
					>
				, OrderBy<Asc<EPActivityApprove.startDate>>
				> Activity;

		public PXSetupOptional<EPSetup> EPsetingst;

		#endregion

		public EmployeeActivitiesEntry()
		{
			EPActivityType activityType = (EPActivityType) PXSelect<EPActivityType>.Search<EPActivityType.type>(this, EPsetingst.Current.DefaultActivityType);
			if (activityType == null || activityType.RequireTimeByDefault != true)
			{
				throw new PXSetupNotEnteredException(Messages.SetupNotEntered, typeof(EPActivityType), Messages.ActivityType);
			}
			this.FieldUpdating.AddHandler(typeof(EPActivityApprove), "StartDate_Date", StartDateFieldUpdating);

			EPEmployee employeeByUserID = PXSelect<EPEmployee, Where<EP.EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>.Select(this);
			if (employeeByUserID == null)
			{
				//throw new PXException(Messages.MustBeEmployee);
				Redirector.Redirect(System.Web.HttpContext.Current, string.Format("~/Frames/Error.aspx?exceptionID={0}&typeID={1}", Messages.MustBeEmployee, "error"));
			}
        }

		#region Actions

		public PXSave<EPActivityFilter> Save;
		public PXCancel<EPActivityFilter> Cancel;
		public PXAction<EPActivityFilter> View;
		[PXUIField(DisplayName = Messages.View)]
		[PXButton()]
		public virtual IEnumerable view(PXAdapter adapter)
		{
			var row = Activity.Current;
			if (row != null)
			{
				PXRedirectHelper.TryRedirect(this, row, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXAction<EPActivityFilter> viewCase;
		[PXUIField(Visible = false)]
		[PXButton()]
		public virtual IEnumerable ViewCase(PXAdapter adapter)
		{
			EPActivity row = Activity.Current;
			if (row != null)
			{
				CRCase caseRow = PXSelect<CRCase>.Search<CRCase.noteID>(this, row.RefNoteID);
				if (caseRow != null)
					PXRedirectHelper.TryRedirect(this, caseRow, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXAction<EPActivityFilter> viewContract;
		[PXUIField(Visible = false)]
		[PXButton()]
		public virtual IEnumerable ViewContract(PXAdapter adapter)
		{
			EPActivity row = Activity.Current;
			if (row != null)
			{
				CT.Contract contractRow = PXSelectJoin<CT.Contract, InnerJoin<CRCase, On<CRCase.contractID, Equal<CT.Contract.contractID>>>, Where<CRCase.noteID, Equal<Required<EPActivity.refNoteID>>>>.Select(this, row.RefNoteID);
				if (contractRow != null)
					PXRedirectHelper.TryRedirect(this, contractRow, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		#endregion

		#region Event handlers

		protected virtual IEnumerable activity()
		{

			List<object> args = new List<object>();
			EPActivityFilter filterRow = Filter.Current;
			if (filterRow == null)
				return null;

			PXSelectBase<EPActivityApprove> cmd;
			cmd = new PXSelectJoin<EPActivityApprove
					, LeftJoin<EPEarningType, On<EPEarningType.typeCD, Equal<EPActivityApprove.earningTypeID>>
						, LeftJoin<CRCase, On<CRCase.noteID, Equal<EPActivityApprove.refNoteID>>
							, LeftJoin<Contract, On<CRCase.contractID, Equal<Contract.contractID>>>
							>
						>
					, Where<EPActivityApprove.owner, Equal<Current<EPActivityFilter.ownerID>>
						, And<EPActivityApprove.isCorrected, Equal<False>
							, And<EPActivityApprove.trackTime, Equal<True>
								, And<EPActivityApprove.classID, NotEqual<CRActivityClass.emailRouting>
									, And<EPActivityApprove.classID, NotEqual<CRActivityClass.task>
										, And<EPActivityApprove.classID, NotEqual<CRActivityClass.events>>
										>
									>
								>
							>
						>
					, OrderBy<Desc<EPActivityApprove.startDate>>
					>(this);

			if (filterRow.FromDate != null)
			{
				cmd.WhereAnd<Where<EPActivityApprove.startDate, GreaterEqual<Required<EPActivityFilter.fromDate>>>>();
				args.Add(filterRow.FromDate);
			}

			if (filterRow.TillDate != null)
			{
				cmd.WhereAnd<Where<EPActivityApprove.startDate, Less<Required<EPActivityFilter.tillDate>>>>();
				args.Add(filterRow.TillDate.Value.AddDays(1));
			}

			if (filterRow.ProjectID != null)
				cmd.WhereAnd<Where<EPActivityApprove.projectID, Equal<Current<EPActivityFilter.projectID>>>>();

			if (filterRow.ProjectTaskID != null)
				cmd.WhereAnd<Where<EPActivityApprove.projectTaskID, Equal<Current<EPActivityFilter.projectTaskID>>>>();

			if (filterRow.IncludeReject == true)
				cmd.WhereOr<Where<EPActivityApprove.uistatus, Equal<CR.ActivityStatusListAttribute.rejected>>>();

			return cmd.Select(args.ToArray());
		}

		protected virtual void EPActivityFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPStartDateAttribute.SetTimeVisible<EPActivityApprove.startDate>(Activity.Cache, null, EPsetingst.Current.RequireTimes == true);

			if (!PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>())
			{
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof(EPActivityApprove.timeSpent).Name, false);
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof(EPActivityApprove.timeBillable).Name, false);
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof(EPActivityApprove.isBillable).Name, false);

				PXUIFieldAttribute.SetVisible<EPActivityFilter.regularOvertime>(sender, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.regularTime>(sender, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.regularTotal>(sender, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.billableOvertime>(sender, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.billableTime>(sender, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.billableTotal>(sender, null, false);
			}

			if (!PXAccess.FeatureInstalled<FeaturesSet.projectModule>())
			{
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof(EPActivityApprove.projectTaskID).Name, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.projectTaskID>(sender, null, false);
			}
			EPActivityFilter row = (EPActivityFilter)e.Row;
			if (row != null)
			{
				row.BillableTime = 0;
				row.BillableOvertime = 0;
				row.BillableTotal = 0;
				row.RegularTime = 0;
				row.RegularOvertime = 0;
				row.RegularTotal = 0;
				foreach (PXResult<EPActivityApprove, EPEarningType> item in Activity.Select())
				{
					EPActivityApprove rowActivity = (EPActivityApprove)item;
					EPEarningType rowEarningType = (EPEarningType)item;

					if (rowEarningType.IsOvertime == true)
					{
						row.RegularOvertime += rowActivity.TimeSpent.GetValueOrDefault(0);
						row.BillableOvertime += rowActivity.TimeBillable.GetValueOrDefault(0);
					}
					else
					{
						row.RegularTime += rowActivity.TimeSpent.GetValueOrDefault(0);
						row.BillableTime += rowActivity.TimeBillable.GetValueOrDefault(0);
					}

					row.BillableTotal = row.BillableTime + row.BillableOvertime;
					row.RegularTotal = row.RegularTime + row.RegularOvertime;
				}
			}
		}

		protected virtual void EPActivityFilter_FromDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPActivityFilter row = (EPActivityFilter) e.Row;
			if (row != null && e.ExternalCall && row.FromDate > row.TillDate)
				row.TillDate = row.FromDate;
		}

		protected virtual void EPActivityFilter_TillDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPActivityFilter row = (EPActivityFilter)e.Row;
			if (row != null && e.ExternalCall && row.FromDate > row.TillDate)
				row.FromDate = row.TillDate;
		}

		protected virtual void EPActivityFilter_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (Activity.Cache.IsDirty && Filter.View.Ask(ActionsMessages.ConfirmationMsg, MessageButtons.YesNo) != WebDialogResult.Yes)
			{
				e.Cancel = true;
			}
			else
			{
				Activity.Cache.Clear();
			}

		}

		protected virtual void StartDateFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			EPActivityFilter rowFilter = Filter.Current;
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (rowFilter == null || e.NewValue == null)
				return;
			if (((DateTime)e.NewValue).Date < rowFilter.FromDate || ((DateTime)e.NewValue).Date > rowFilter.TillDate)
			{
				sender.RaiseExceptionHandling<EPActivityApprove.startDate>(row, rowFilter.FromDate, new PXSetPropertyException(Messages.StartDateOutOfRange));
				e.Cancel = true;
			}
		}

		protected virtual void EPActivityApprove_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			EPActivityFilter rowFilter = (EPActivityFilter)Filter.Current;

			if (row == null || rowFilter == null)
				return;

			if (row.Released == true)
			{
				PXUIFieldAttribute.SetEnabled(sender, row, false);
				EPStartDateAttribute.SetTimeEnabled<EPActivityApprove.startDate>(sender, row, false);
				EPStartDateAttribute.SetDateEnabled<EPActivityApprove.startDate>(sender, row, false);
			}
			else if (row.UIStatus == CR.ActivityStatusListAttribute.Open || row.UIStatus == CR.ActivityStatusListAttribute.Completed)
			{
				PXUIFieldAttribute.SetEnabled(sender, row, true);
				PXUIEnabledAttribute.SetEnabled<EPActivityApprove.timeBillable>(sender, row);
				PMProject project = (PMProject) PXSelectorAttribute.Select<EPActivityApprove.projectID>(sender, row);
				PXUIFieldAttribute.SetEnabled<EPActivityApprove.projectID>(sender, row, rowFilter.ProjectID == null);
				if (project != null)
					PXUIFieldAttribute.SetEnabled<EPActivityApprove.projectTaskID>(sender, row,
					                                                               rowFilter.ProjectTaskID == null &&
					                                                               project.BaseType ==
					                                                               PMProject.ProjectBaseType.Project);
				else
					PXUIFieldAttribute.SetEnabled<EPActivityApprove.projectTaskID>(sender, row, rowFilter.ProjectTaskID == null);
				EPStartDateAttribute.SetTimeEnabled<EPActivityApprove.startDate>(sender, row, true);
				EPStartDateAttribute.SetDateEnabled<EPActivityApprove.startDate>(sender, row, true);
				PXUIFieldAttribute.SetEnabled<EPActivityApprove.uistatus>(sender, row, false);
				PXUIFieldAttribute.SetEnabled<EPActivityApprove.approverID>(sender, row, false);
				PXUIFieldAttribute.SetEnabled<EPActivityApprove.timeCardCD>(sender, row, false);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(sender, row, false);
				EPStartDateAttribute.SetTimeEnabled<EPActivityApprove.startDate>(sender, row, false);
				EPStartDateAttribute.SetDateEnabled<EPActivityApprove.startDate>(sender, row, false);
				PXUIFieldAttribute.SetEnabled<EPActivityApprove.hold>(sender, row, true);
			}

			PXUIFieldAttribute.SetEnabled<EPActivityApprove.approvalStatus>(sender, row, false);

			if (row.UIStatus == CR.ActivityStatusListAttribute.Rejected)
				sender.RaiseExceptionHandling<EPActivityApprove.hold>(row, null, new PXSetPropertyException(Messages.Rejected, PXErrorLevel.RowWarning));
		}

		protected virtual void EPActivityApprove_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row == null)
				return;

			if (row.UIStatus == CR.ActivityStatusListAttribute.Approved || row.Released == true)
			{
				Filter.View.Ask((string.Format(Messages.ActivityIs, sender.GetValueExt<EPActivityApprove.uistatus>(row))), MessageButtons.OK);
				e.Cancel = true;
			}
			else if (row.TimeCardCD != null)
			{
				Filter.View.Ask(Messages.ActivityAssignedToTimeCard, MessageButtons.OK);
				e.Cancel = true;
			}
		}

		protected virtual void EPActivityApprove_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row == null)
				return;
			if (row.TimeCardCD != null)
			{
				Filter.View.Ask(Messages.ActivityAssignedToTimeCard, MessageButtons.OK);
				e.Cancel = true;
			}
		}

		protected virtual void EPActivityApprove_EarningTypeID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row == null || EPsetingst.Current == null)
				return;

			EPEmployee rowEmploye = PXSelect<EPEmployee>.Search<EPEmployee.userID>(this, Filter.Current.OwnerID);
			if (rowEmploye == null || row.StartDate == null)
				return;

			if (CalendarHelper.IsWorkDay(this, rowEmploye.CalendarID, (DateTime)row.StartDate))
			{
				e.NewValue = EPsetingst.Current.RegularHoursType;
			}
			else
			{
				e.NewValue = EPsetingst.Current.HolidaysType;
			}
		}

		protected virtual void EPActivityApprove_ProjectID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row == null)
				return;

			if (Filter.Current.ProjectID != null)
			{
				e.NewValue = Filter.Current.ProjectID;
				e.Cancel = true;
			}

		}

		protected virtual void EPActivityApprove_ProjectTaskID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			
			if (row == null)
				return;

			if (Filter.Current.ProjectTaskID != null)
			{
				e.NewValue = Filter.Current.ProjectTaskID;
				e.Cancel = true;
				return;
			}

			if (row.ParentTaskID != null)
			{
				EPActivityApprove rowParentTask = PXSelect<EPActivityApprove>.Search<EPActivityApprove.taskID>(this, row.ParentTaskID);
				if (rowParentTask != null && rowParentTask.ProjectID == row.ProjectID)
				{
					e.NewValue = rowParentTask.ProjectTaskID;
					e.Cancel = true;
				}
			}

			EPEarningType earningRow = (EPEarningType)PXSelectorAttribute.Select<EPActivityApprove.earningTypeID>(cache, row);
			if (e.NewValue == null && earningRow != null && earningRow.ProjectID == row.ProjectID)
			{
				e.NewValue = earningRow.TaskID;
				e.Cancel = true;
			}
		}

		protected virtual void EPActivityApprove_Hold_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row == null)
				return;
			if (row.UIStatus == CR.ActivityStatusListAttribute.Approved)
				cache.RaiseExceptionHandling<EPActivityApprove.hold>(row, null, new PXSetPropertyException(Messages.Approved, PXErrorLevel.RowWarning));

			if (row.Hold == true)
			{
				cache.SetValueExt<EPActivity.uistatus>(row, ActivityStatusListAttribute.Open);
			}
			else
			{
				cache.SetValueExt<EPActivity.uistatus>(row, ActivityStatusListAttribute.Completed);
			}

		}

		protected virtual void EPActivityApprove_Hold_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row != null)
				e.ReturnValue = (row.UIStatus == CR.ActivityStatusListAttribute.Rejected || row.UIStatus == CR.ActivityStatusListAttribute.Open);
		}

		protected virtual int? GetTimeBillable(EPActivityApprove row, int? OldTimeSpent)
		{
			if (row.TimeCardCD == null && row.Billed != true)
			{
				if (row.IsBillable != true)
					return null;
				else if ((OldTimeSpent ?? 0) == 0 || OldTimeSpent == row.TimeBillable)
					return row.TimeSpent;
				else
					return row.TimeBillable;
			}
			else
				return row.TimeBillable;
		}

		protected virtual void EPActivityApprove_IsBillable_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row == null)
				return;
			else
				row.TimeBillable = GetTimeBillable(row, null);
		}

		protected virtual void EPActivityApprove_TimeSpent_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row == null)
				return;
			else
				row.TimeBillable = GetTimeBillable(row, (int?)e.OldValue);
		}


		protected virtual void EPActivityApprove_Type_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove) e.Row;
			if (row == null)
			{
				e.NewValue = null;
			}
			else
			{
				e.NewValue = EPsetingst.Current.DefaultActivityType;
				e.Cancel = true;
			}
		}

		#endregion

		#region DAC Overrides
		#region ProjectTaskID
		[ActiveProjectTask(typeof(EPActivityApprove.projectID))]
		[PXFormula(typeof(Default<EPActivityApprove.parentTaskID, EPActivityApprove.earningTypeID>))]
		public virtual void EPActivityApprove_ProjectTaskID_CacheAttached(PXCache cache)
		{
		}
		#endregion

		#region ClassID
		[PXDefault(typeof(CRActivityClass.activity))]
		[PXDBInt]
		public virtual void EPActivityApprove_ClassID_CacheAttached(PXCache cache)
		{
		}
		#endregion

		#region StartDate
		[PXDefault(typeof(EPActivityFilter.fromDate))]
		[EPStartDate(DisplayName = "Date", DisplayNameDate = "Date", DisplayNameTime = "Start Time")]
		[PXUIField(DisplayName = "Date")]
		public virtual void EPActivityApprove_StartDate_CacheAttached(PXCache cache)
		{
		}
		#endregion

		#region Owner
		[PXDefault(typeof(EPActivityFilter.ownerID))]
		[PXDBGuid()]
		public virtual void EPActivityApprove_Owner_CacheAttached(PXCache cache)
		{
		}
		#endregion

		#region Subject
		[PXDBString(255, InputMask = "", IsUnicode = true)]
		[PXDefault]
		[PXFormula(
			typeof(
				Switch<
					Case<Where<Current<EPActivityApprove.subject>, IsNotNull>, Current<EPActivityApprove.subject>
						, Case<Where<EPActivityApprove.parentTaskID, IsNotNull>, Selector<EPActivityApprove.parentTaskID, EPActivityApprove.subject>
							, Case<Where<EPActivityApprove.projectTaskID, IsNotNull>, Selector<EPActivityApprove.projectTaskID, PMTask.description>>
							>
						>
					>
				)
			)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual void EPActivityApprove_Subject_CacheAttached(PXCache cache)
		{
		}
		#endregion

		#region ParentTaskID
		[PXUIField(DisplayName = "Task")]
		[PXDBInt]
		[PXSelector(
			typeof(Search<
				EPActivity.taskID
				, Where<EPActivity.taskID, NotEqual<Current<EPActivityApprove.taskID>>
					, And<EPActivity.owner, Equal<Current<EPActivityApprove.owner>>
						, And<EPActivity.classID, NotEqual<CRActivityClass.events>
							, And<Where<EPActivity.classID, Equal<CRActivityClass.task>, Or<EPActivity.classID, Equal<CRActivityClass.events>>>>
							>
						>
					>
				, OrderBy<Desc<EPActivityApprove.taskID>>>
				)
			, typeof(EPActivityApprove.taskID), typeof(EPActivityApprove.subject), typeof(EPActivityApprove.classInfo), typeof(EPActivityApprove.uistatus)
			, DescriptionField = typeof(EPActivityApprove.subject)
			)]
		protected virtual void EPActivityApprove_ParentTaskID_CacheAttached(PXCache cache)
		{
		}
		#endregion

		#region TimeCardCD
		[PXDBString(10)]
		[PXUIField(DisplayName = "Time Card Ref.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual void EPActivityApprove_TimeCardCD_CacheAttached(PXCache cache)
		{
		}
		#endregion

		#region Hold
		[PXBool]
		[PXUIField(FieldName = "Hold", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual void EPActivityApprove_Hold_CacheAttached(PXCache cache)
		{
		}
		#endregion

		#region TimeBillable
		[PXDBInt]
		[PXTimeList]
		[PXUIField(DisplayName = "Billable Time")]
		[PXUIEnabled(typeof(EPActivity.isBillable))]
		public virtual void EPActivityApprove_TimeBillable_CacheAttached(PXCache cache)
		{
		}
		#endregion

		#region Contract_ContractCD
		[PXDimensionSelector(ContractAttribute.DimensionName,
			typeof(Search2<Contract.contractCD, InnerJoin<ContractBillingSchedule, On<Contract.contractID, Equal<ContractBillingSchedule.contractID>>, LeftJoin<AR.Customer, On<AR.Customer.bAccountID, Equal<Contract.customerID>>>>
			, Where<Contract.isTemplate, Equal<boolFalse>, And<Contract.baseType, Equal<Contract.ContractBaseType>>>>),
			typeof(Contract.contractCD),
			typeof(Contract.contractCD), typeof(Contract.customerID), typeof(AR.Customer.acctName), typeof(Contract.locationID), typeof(Contract.description), typeof(Contract.status), typeof(Contract.expireDate), typeof(ContractBillingSchedule.lastDate), typeof(ContractBillingSchedule.nextDate), DescriptionField = typeof(Contract.description), Filterable = true)]
		[PXDBString(IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Contract ID", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXFieldDescription]
		public virtual void Contract_ContractCD_CacheAttached(PXCache cache)
		{
		}
		#endregion


		#endregion

		#region Filter
		[Serializable]
		public class EPActivityFilter : OwnedFilter
		{

			#region OwnerID
			public new abstract class ownerID : PX.Data.IBqlField
			{
			}
			[PXDBGuid]
			[PXUIField(DisplayName = "Employee")]
			[PXDefault(typeof(AccessInfo.userID))]
			[PX.TM.PXSubordinateOwnerSelector]
			public override Guid? OwnerID { set; get; }
			#endregion

			#region FromDate
			public abstract class fromDate : IBqlField
			{
			}
			[PXDBDate(DisplayMask = "d", PreserveTime = true)]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "From Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? FromDate { set; get; }
			#endregion

			#region TillDate
			public abstract class tillDate : IBqlField
			{
			}
			[PXDBDate(DisplayMask = "d", PreserveTime = true)]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Till Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? TillDate { set; get; }
			#endregion

			#region ProjectID
			public abstract class projectID : IBqlField
			{
			}
			[ActiveProject(DisplayName = PM.Messages.Project)]
			public virtual Int32? ProjectID { get; set; }
			#endregion

			#region ProjectTaskID
			public abstract class projectTaskID : IBqlField
			{
			}
			[ProjectTask(typeof(EPActivityFilter.projectID))]
			public virtual Int32? ProjectTaskID { set; get; }
			#endregion

			#region IncludeReject
			public abstract class includeReject : IBqlField
			{
			}
			[PXBool]
			[PXDefault(true)]
			[PXUIField(DisplayName = "Include All Rejected")]
			public virtual bool? IncludeReject { set; get; }
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

			#region BillableTime
			public abstract class billableTime : IBqlField
			{
			}
			[PXInt]
			[PXUIField(DisplayName = "Billable", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Int32? BillableTime { set; get; }
			#endregion
			#region BillableOvertime
			public abstract class billableOvertime : IBqlField
			{
			}
			[PXInt]
			[PXUIField(DisplayName = "Billable Overtime", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Int32? BillableOvertime { set; get; }
			#endregion
			#region BillableTotal
			public abstract class billableTotal : IBqlField
			{
			}
			[PXInt]
			[PXUIField(DisplayName = "Billable Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Int32? BillableTotal { set; get; }
			#endregion
			#endregion

		}
		#endregion

	}

}
