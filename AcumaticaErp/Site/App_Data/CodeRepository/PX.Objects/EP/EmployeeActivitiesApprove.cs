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
	public class EmployeeActivitiesApprove : PXGraph<EmployeeActivitiesApprove>
	{
		#region Selects

		[PXHidden]
		public PXFilter<CT.Contract> dummyContract;
		public PXFilter<EPActivityFilter> Filter;
		public PXSelectJoin<
				EPActivityApprove
				, LeftJoin<EPEarningType, On<EPEarningType.typeCD, Equal<EPActivityApprove.earningTypeID>>
					, LeftJoin<PMProject, On<PMProject.contractID, Equal<EPActivityApprove.projectID>>
						, LeftJoin<CRCase, On<CRCase.noteID, Equal<EPActivityApprove.refNoteID>>
							, LeftJoin<Contract, On<CRCase.contractID, Equal<Contract.contractID>>>
							>
						>
					>
				, Where2<
					 Where<EPActivityApprove.approverID, Equal<Current<EPActivityFilter.approverID>>, Or<Contract.approverID, Equal<Current<EPActivityFilter.approverID>>>>
					 , And<EPActivityApprove.approverID, IsNotNull
						 , And2<Where<EPActivityApprove.uistatus, NotEqual<ActivityStatusListAttribute.canceled>, And<EPActivityApprove.uistatus, NotEqual<ActivityStatusListAttribute.open>, And<EPActivityApprove.released, NotEqual<True>>>>
							, And<EPActivityApprove.startDate, Less<Add<Current<EPActivityFilter.tillDate>, int1>>
								, And2<Where<EPActivityApprove.startDate, GreaterEqual<Current<EPActivityFilter.fromDate>>, Or<Current<EPActivityFilter.fromDate>, IsNull>>
									, And2<Where<EPActivityApprove.projectID, Equal<Current<EPActivityFilter.projectID>>, Or<Current<EPActivityFilter.projectID>, IsNull>>
										, And2<Where<EPActivityApprove.projectTaskID, Equal<Current<EPActivityFilter.projectTaskID>>, Or<Current<EPActivityFilter.projectTaskID>, IsNull>>
											, And2<Where<EPActivityApprove.owner, Equal<Current<EPActivityFilter.employeeID>>, Or<Current<EPActivityFilter.employeeID>, IsNull>>
												, And<EPActivityApprove.released, Equal<False>
													, And<EPActivityApprove.trackTime, Equal<True>
														, And<EPActivityApprove.classID, NotEqual<CRActivityClass.emailRouting>
															, And<EPActivityApprove.classID, NotEqual<CRActivityClass.task>
																, And<EPActivityApprove.classID, NotEqual<CRActivityClass.events>>
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
				, OrderBy<Desc<EPActivityApprove.startDate>>
				> Activity;

		#endregion

		#region Actions

		public PXSave<EPActivityFilter> Save;
		public PXCancel<EPActivityFilter> Cancel;
		public PXAction<EPActivityFilter> approveAll;
		public PXAction<EPActivityFilter> rejectAll;
		public PXAction<EPActivityFilter> viewDetails;
		public PXAction<EPActivityFilter> viewCase;
		public PXAction<EPActivityFilter> viewContract;

		[PXUIField(DisplayName = Messages.ViewDetails)]
		[PXButton()]
		public virtual IEnumerable ViewDetails(PXAdapter adapter)
		{
			var row = Activity.Current;
			if (row != null)
			{
				PXRedirectHelper.TryRedirect(this, row, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.ApproveAll)]
		[PXButton()]
		public virtual IEnumerable ApproveAll(PXAdapter adapter)
		{
			if (Activity.Current == null || Filter.View.Ask(Messages.ApproveAll, MessageButtons.YesNo) != WebDialogResult.Yes)
			{
				return adapter.Get();
			}

			foreach (EPActivityApprove item in Activity.Select())
			{
				item.IsApprove = true;
				item.IsReject = false;
				Activity.Cache.Update(item);
			}
			Persist();
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.RejectAll)]
		[PXButton()]
		public virtual IEnumerable RejectAll(PXAdapter adapter)
		{
			if (Activity.Current == null || Filter.View.Ask(Messages.RejectAll, MessageButtons.YesNo) != WebDialogResult.Yes)
			{
				return adapter.Get();
			}

			foreach (EPActivityApprove item in Activity.Select())
			{
				item.IsApprove = false;
				item.IsReject = true;
				Activity.Cache.Update(item);
			}
			Persist();
			return adapter.Get();
		}

		[PXButton()]
		[PXUIField(Visible = false)]
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

		[PXButton()]
		[PXUIField(Visible = false)]
		public virtual IEnumerable ViewContract(PXAdapter adapter)
		{
			EPActivity row = Activity.Current;
			if (row != null)
			{
				Contract contractRow = PXSelectJoin<Contract, InnerJoin<CRCase, On<CRCase.contractID, Equal<Contract.contractID>>>,Where<CRCase.noteID, Equal<Required<EPActivity.refNoteID>>>>.Select(this, row.RefNoteID);
				if (contractRow != null)
					PXRedirectHelper.TryRedirect(this, contractRow, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		#endregion

		#region Event handlers

		protected virtual void EPActivityFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>())
			{
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof(EPActivity.timeSpent).Name, false);
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof(EPActivity.timeBillable).Name, false);
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof(EPActivity.isBillable).Name, false);

				PXUIFieldAttribute.SetVisible<EPActivityFilter.regularOvertime>(sender, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.regularTime>(sender, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.regularTotal>(sender, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.billableOvertime>(sender, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.billableTime>(sender, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.billableTotal>(sender, null, false);
			}

			if (!PXAccess.FeatureInstalled<FeaturesSet.projectModule>())
			{
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof(EPActivity.projectTaskID).Name, false);
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
			EPActivityFilter row = (EPActivityFilter)e.Row;
			if (row != null && e.ExternalCall && row.FromDate > row.TillDate)
				row.TillDate = row.FromDate;
		}

		protected virtual void EPActivityFilter_TillDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPActivityFilter row = (EPActivityFilter)e.Row;
			if (row != null && e.ExternalCall && row.FromDate != null && row.FromDate > row.TillDate)
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

		protected virtual void EPActivityApprove_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPActivity row = (EPActivity)e.Row;
			if (row == null)
				return;

			PXUIFieldAttribute.SetEnabled(sender, row, false);
			PXUIFieldAttribute.SetEnabled<EPActivityApprove.isApprove>(sender, row, true);
			PXUIFieldAttribute.SetEnabled<EPActivityApprove.isReject>(sender, row, true);
		}

		protected virtual void EPActivityApprove_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove) e.Row;
			if (row == null)
				return;
			if (row.IsApprove == true)
			{
				sender.SetValueExt<EPActivity.uistatus>(row, ActivityStatusListAttribute.Approved);
				row.ApprovedDate = Accessinfo.BusinessDate;
			}
			else if (row.IsReject == true)
			{
				sender.SetValueExt<EPActivity.uistatus>(row, ActivityStatusListAttribute.Rejected);
			}
			else if (row.UIStatus == ActivityStatusListAttribute.Rejected || row.UIStatus == ActivityStatusListAttribute.Approved)
			{
				sender.SetValueExt<EPActivity.uistatus>(row, ActivityStatusListAttribute.Completed);
			}
		}

		#endregion

		#region Filter
		[Serializable]
		public class EPActivityFilter : IBqlTable
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
			[PXDBGuid]
			[PXUIField(DisplayName = "Employee")]
			[PX.TM.PXSubordinateOwnerSelector]
			public virtual Guid? EmployeeID { set; get; }
			#endregion

			#region FromDate
			public abstract class fromDate : IBqlField
			{
			}
			[PXDBDate(DisplayMask = "d", PreserveTime = true)]
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
			public virtual Int32? ProjectID { set; get; }
			#endregion

			#region ProjectTaskID
			public abstract class projectTaskID : IBqlField
			{
			}
			[ProjectTask(typeof(EPActivityFilter.projectID))]
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
	[Serializable]
	public partial class EPActivityApprove : EPActivity
	{
		public new class taskID : IBqlField { }

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
				return _IsApprove ?? UIStatus == ActivityStatusListAttribute.Approved;
			}
			set
			{
				_IsApprove = value;
				if (_IsApprove == true)
					_IsReject = false;
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
				return _IsReject ?? UIStatus == ActivityStatusListAttribute.Rejected;
			}
			set
			{
				_IsReject = value;
				if (_IsReject == true)
					_IsApprove = false;
			}
		}

		#endregion

		#region Owner
		public new abstract class owner : PX.Data.IBqlField
		{
		}
		[PXDBGuid()]
		[PXChildUpdatable(AutoRefresh = true)]
		[PXOwnerSelector(typeof(groupID))]//, SubstituteKey = typeof(EPEmployee.acctName))] TODO: if use SubstituteKey got the error "'Employee' Not found in system"
		[PXUIField(DisplayName = "Employee")]
		public override Guid? Owner { get; set; }
		#endregion

		#region ProjectID
		public new abstract class projectID : IBqlField
		{
		}
		#endregion

		#region ProjectTaskID
		public new abstract class projectTaskID : IBqlField
		{
		}
		#endregion

		#region StartDate
		public new abstract class startDate : PX.Data.IBqlField
		{
		}
		#endregion

		#region WeekID
		public new abstract class weekID : IBqlField
		{
		}
		#endregion

		#region Subject
		public new abstract class subject : PX.Data.IBqlField
		{
		}

		[PXDBString(255, InputMask = "", IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXNavigateSelector(typeof(EPActivity.subject))]
		public override String Subject
		{
			[PXDependsOnFields(typeof(classID), typeof(taskID))]
			get
			{
				return _ClassID == CRActivityClass.History ? "Changeset " + _TaskID : this._Subject;
			}
			set
			{
				this._Subject = value;
			}
		}
		#endregion

		#region TimeSpent
		public new abstract class timeSpent : IBqlField
		{
		}
		//[CRDBTimeSpan]
		//[PXDefault("00:00", PersistingCheck = PXPersistingCheck.Nothing)]
		//[PXUIField(DisplayName = "Time")]
		//public override Int32? TimeSpent { set; get; }
		#endregion

		#region ParentTaskID
		public new abstract class parentTaskID : PX.Data.IBqlField
		{
		}

		[PXDBInt]
		[PXUIField(DisplayName = "Task")]
		public override int? ParentTaskID { get; set; }
		#endregion

		#region TimeCardCD
		public new abstract class timeCardCD : IBqlField
		{
		}
		[PXDBString(10)]
		[PXUIField(DisplayName = "Time Card Ref.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public override String TimeCardCD { get; set; }
		#endregion

		#region OvertimeBillable
		public new abstract class overtimeBillable : PX.Data.IBqlField
		{
		}
		[CRDBTimeSpan]
		[PXFormula(typeof(Switch<Case<Where<EPActivity.billed, NotEqual<True>, And<EPActivity.timeCardCD, IsNull, And<EPActivity.isBillable, Equal<True>>>>,
			Switch<Case<Where<EPActivity.overtimeSpent, IsNotNull, And<EPActivity.overtimeSpent, Greater<int0>>>, EPActivity.overtimeSpent>, int0>>>))]
		[PXUIField(DisplayName = "Billable OT")]
		public override Int32? OvertimeBillable { get; set; }
		#endregion

		#region UIStatus
		public new abstract class uistatus : PX.Data.IBqlField { }

		[PXDBString(2, IsFixed = true)]
		[ActivityStatus]
		[PXUIField(DisplayName = "Status")]
		[PXDefault(ActivityStatusAttribute.Open, PersistingCheck = PXPersistingCheck.Nothing)]
		public override string UIStatus { get; set; }
		#endregion

	}



}
