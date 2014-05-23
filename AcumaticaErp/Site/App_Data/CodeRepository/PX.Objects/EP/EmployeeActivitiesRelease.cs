using System;
using System.Collections;
using System.Configuration;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Data;
using System.Collections.Generic;
using PX.Reports.ARm;
using System.Linq;


namespace PX.Objects.EP
{
	public class EmployeeActivitiesRelease : PXGraph<EmployeeActivitiesRelease>
	{
		public EmployeeActivitiesRelease()
		{
			if (PXSelect<EPSetup>.Select(this) == null)
				throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(EPSetup), Messages.EPSetup);
			Activity.SetProcessDelegate(ReleaseActivities);
			Activity.SetSelected<EPActivityApprove.selected>();
		}

		#region Selects
		[PXHidden]
		public PXFilter<CT.Contract> dummyContract;
		public PXFilter<EPActivityFilter> Filter;
		public PXCancel<EPActivityFilter> Cancel;

		public PXFilteredProcessingJoin<
			EPActivityApprove
			, EPActivityFilter
			, LeftJoin<EPEarningType, On<EPEarningType.typeCD, Equal<EPActivityApprove.earningTypeID>>
				, InnerJoin<EPEmployee
					, On<
						EPEmployee.userID, Equal<EPActivityApprove.owner>
						, And<Where<EPEmployee.timeCardRequired, NotEqual<True>, Or<EPEmployee.timeCardRequired, IsNull>>>
						>
					, LeftJoin<CRCase, On<CRCase.noteID, Equal<EPActivityApprove.refNoteID>>
						, LeftJoin<CRCaseClass, On<CRCaseClass.caseClassID, Equal<CRCase.caseClassID>>
							, LeftJoin<CT.Contract, On<CRCase.contractID, Equal<CT.Contract.contractID>>>
							>
						>
					>
				>
			, Where2<
				Where2<
					Where<EPActivityApprove.uistatus, Equal<ActivityStatusListAttribute.completed>, And<EPActivityApprove.approverID, IsNull>>
					, Or<Where<EPActivityApprove.uistatus, Equal<ActivityStatusListAttribute.approved>,And<EPActivityApprove.approverID, IsNotNull>>>
					>
				, And<EPActivityApprove.startDate, Less<Add<Current<EPActivityFilter.tillDate>, int1>>
					, And2<Where<EPActivityApprove.startDate, GreaterEqual<Current<EPActivityFilter.fromDate>>, Or<Current<EPActivityFilter.fromDate>, IsNull>>
						, And2<Where<EPActivityApprove.projectID, Equal<Current<EPActivityFilter.projectID>>, Or<Current<EPActivityFilter.projectID>, IsNull>>
							, And2<Where<EPActivityApprove.projectTaskID, Equal<Current<EPActivityFilter.projectTaskID>>, Or<Current<EPActivityFilter.projectTaskID>, IsNull>>
								, And2<Where<CT.Contract.contractID, Equal<Current<EPActivityFilter.contractID>>, Or<Current<EPActivityFilter.contractID>, IsNull>>
									, And<EPActivityApprove.projectID, IsNotNull
										, And<EPActivityApprove.released, NotEqual<True>
											, And<EPActivityApprove.trackTime, Equal<True>
												, And<EPActivityApprove.origTaskID, IsNull
													, And<EPActivityApprove.classID, NotEqual<CRActivityClass.emailRouting>
														, And<EPActivityApprove.classID, NotEqual<CRActivityClass.task>
															, And<EPActivityApprove.classID, NotEqual<CRActivityClass.events>
																, And2<Where<EPActivityApprove.owner, Equal<Current<EPActivityFilter.employeeID>>, Or<Current<EPActivityFilter.employeeID>, IsNull>>
																	, And<Where<CRCaseClass.perItemBilling, Equal<BillingTypeListAttribute.perActivity>, Or<CRCaseClass.caseClassID, IsNull, Or<CRCaseClass.isBillable, Equal<False>>>>>
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
			, OrderBy<Desc<EPActivityApprove.startDate>>
			> Activity;

		#endregion

		#region Actions

		public PXAction<EPActivityFilter> viewDetails;

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

		public PXAction<EPActivityFilter> viewCase;
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

		public PXAction<EPActivityFilter> viewContract;
		[PXButton()]
		[PXUIField(Visible = false)]
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

		protected virtual void EPActivityFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>())
			{
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof (EPActivity.timeSpent).Name, false);
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof (EPActivity.timeBillable).Name, false);
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof (EPActivity.isBillable).Name, false);

				PXUIFieldAttribute.SetVisible<EPActivityFilter.regularOvertime>(sender, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.regularTime>(sender, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.regularTotal>(sender, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.billableOvertime>(sender, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.billableTime>(sender, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.billableTotal>(sender, null, false);
			}

			if (!PXAccess.FeatureInstalled<FeaturesSet.projectModule>())
			{
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof (EPActivity.projectTaskID).Name, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.projectTaskID>(sender, null, false);
			}
			EPActivityFilter row = (EPActivityFilter) e.Row;
			if (row != null)
			{

				PXResult res = (PXResult)Activity.Select();
				if (res == null || res[typeof(EPEarningType)] == null)
					return;

				row.BillableTime = 0;
				row.BillableOvertime = 0;
				row.BillableTotal = 0;
				row.RegularTime = 0;
				row.RegularOvertime = 0;
				row.RegularTotal = 0;
				foreach (PXResult<EPActivityApprove, EPEarningType> item in Activity.Select())
				{
					EPActivityApprove rowActivity = (EPActivityApprove) item;
					EPEarningType rowEarningType = (EPEarningType) item;

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
			EPActivityFilter row = (EPActivityFilter) e.Row;
			if (row != null && e.ExternalCall && row.FromDate != null && row.FromDate > row.TillDate)
				row.FromDate = row.TillDate;
		}

		#endregion

		#region Filter

		[Serializable]
		public class EPActivityFilter : CR.OwnedFilter
		{

			#region EmployeeID

			public abstract class employeeID : PX.Data.IBqlField
			{
			}

			[PXDBGuid]
			[PXUIField(DisplayName = "Employee")]
			[PX.TM.PXOwnerSelector]
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

			[ProjectTask(typeof (EPActivityFilter.projectID))]
			public virtual Int32? ProjectTaskID { set; get; }

			#endregion

			#region ContractID

			public abstract class contractID : IBqlField
			{
			}

			[CT.Contract(DisplayName = CT.Messages.Contract)]
			public virtual Int32? ContractID { set; get; }

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

		protected static void ReleaseActivities(List<EPActivityApprove> activities)
		{
			RegisterEntry registerEntry = (RegisterEntry)PXGraph.CreateInstance(typeof(RegisterEntry));

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				if (!RecordContractUsage(activities, "Contract-Usage"))
				{
					throw new PXException("Failed to create contract-usage transactions.");
				}

				if (!RecordCostTrans(registerEntry, activities))
				{
					throw new PXException("Failed to create cost transactions.");
				}

				ts.Complete();
			}

			EPSetup setup = PXSelect<EPSetup>.Select(registerEntry);

			if (setup != null && setup.AutomaticReleasePM == true)
			{
				PX.Objects.PM.RegisterRelease.Release(registerEntry.Document.Current);
			}
		}

        public static bool RecordContractUsage(List<EPActivityApprove> activities, string description)
        {
            RegisterEntry registerEntry = PXGraph.CreateInstance<RegisterEntry>();
            registerEntry.FieldVerifying.AddHandler<PMTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
            registerEntry.FieldVerifying.AddHandler<PMTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });//restriction should be applicable only for budgeting.
            registerEntry.Document.Cache.Insert();
            registerEntry.Document.Current.Description = description;
            registerEntry.Document.Current.Released = true;
			registerEntry.Views.Caches.Add(typeof(EPActivity));
			PXCache activityCache = registerEntry.Caches<EPActivity>();

            bool success = true;
			bool activityAdded = false;

            for (int i = 0; i < activities.Count; i++)
            {
                EPActivity activity = activities[i];
                try
                {
					if (activity.RefNoteID != null && PXSelect<CRCase, Where<CRCase.noteID, Equal<Required<CRCase.noteID>>>>.Select(registerEntry, activity.RefNoteID).Count == 1)
                    {
                        //Add Contract-Usage
						activityAdded = registerEntry.CreateContractUsage(activity) != null || activityAdded;
	                    activity.Billed = true;
						activityCache.Update(activity);
					}
                }
                catch (Exception e)
                {
                    PXProcessing<EPActivityApprove>.SetError(i, e is PXOuterException ? e.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)e).InnerMessages) : e.Message);
                    success = false;
                }
            }

			if (success)
			{
				if (activityAdded)
				{
					registerEntry.Save.Press();
				}
				else
				{
					activityCache.Persist(PXDBOperation.Update);
				}
			}

            return success;
        }

        public static bool RecordCostTrans(RegisterEntry registerEntry, List<EPActivityApprove> activities)
        {
            registerEntry.FieldVerifying.AddHandler<PMTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });//restriction should be applicable only for budgeting.
            EmployeeCostEngine costEngine = new EmployeeCostEngine(registerEntry);

            registerEntry.Views.Caches.Add(typeof(EPActivity));
            PXCache activityCache = registerEntry.Caches<EPActivity>();

            registerEntry.Document.Cache.Insert();
            bool success = true;
	        bool activityAdded = false;
            for (int i = 0; i < activities.Count; i++)
            {
				EPActivity activity = PXSelect<EPActivity>.Search<EPActivity.taskID>(registerEntry, activities[i].TaskID);

                if (activity.Released == true) //activity can be released to PM via Timecard prior to releasing the case.
                    continue;
                
                try
                {
					if (activity.ProjectTaskID != null)//cost transactions are created only if project is set.
					{
						EPEmployee employee = PXSelect<EPEmployee>.Search<EPEmployee.userID>(registerEntry, activity.Owner);
						activity.LabourItemID = costEngine.GetLaborClass(activity);
						activityCache.Update(activity);

						decimal? cost = costEngine.CalculateEmployeeCost(activity, employee.BAccountID, activity.StartDate.Value);
						registerEntry.CreateTransaction(activity, employee.BAccountID, activity.StartDate.Value, activity.TimeSpent, activity.TimeBillable, cost);
						activity.EmployeeRate = cost;
						activityAdded = true;
					}
					
                    activity.Released = true;
                    activityCache.Update(activity);
                }
                catch (Exception e)
                {
                    PXProcessing<EPActivityApprove>.SetError(i, e is PXOuterException ? e.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)e).InnerMessages) : e.Message);
                    success = false;
                }
            }
			if (success)
            {
	            if (activityAdded)
	            {
		            registerEntry.Save.Press();
	            }
	            else
	            {
		            activityCache.Persist(PXDBOperation.Update);
	            }
            }

            return success;
        }
	}
}

