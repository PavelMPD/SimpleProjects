using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.CT;
using PX.Objects.PM;
using PX.Objects.GL;
using System.Diagnostics;
using PX.SM;
using PX.TM;
using Branch = PX.Objects.GL.Branch;

namespace PX.Objects.EP
{   
    [Serializable]
	public class TimeCardMaint : PXGraph<TimeCardMaint, EPTimeCard>
	{
		#region DAC Overrides

		[PXDBDate()]
		[PXDefault(typeof(EPTimeCard.createdDateTime), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		[PXDefault(typeof(EPTimeCard.employeeID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_BAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong()]
		protected virtual void EPApproval_CuryInfoID_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region Selects

		//This view is required for 2 purposes - 1 so that correct cache is initialized ApproverEmployee -> EPEmployee
		//										 2 Employee object with all its properties can be used in the Assignment Rules.
		[PXViewName(Messages.Employee)]
        public PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPTimeCard.employeeID>>>> dummy;
            
        [PXHidden]
		public PXSetup<EPSetup> EpSetup;

        [PXHidden]
        public PXSelect<EPEarningType> EarningTypes;
            
        [PXHidden]
		public PXSelect<BAccount> AccountBase;

		[PXViewName(Messages.TimeCardDocument)]
		public PXSelectJoin<EPTimeCard,
						InnerJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<EPTimeCard.employeeID>>>,
						Where<EPTimeCard.createdByID, Equal<Current<AccessInfo.userID>>,
										Or<EPEmployee.userID, Equal<Current<AccessInfo.userID>>,
										Or<EPEmployee.userID, OwnedUser<Current<AccessInfo.userID>>,
										Or<EPTimeCard.noteID, Approver<Current<AccessInfo.userID>>>>>>> Document;

		
		[PXViewName(Messages.TimeCardSummary)]
		public PXSelect<EPTimeCardSummary, Where<EPTimeCardSummary.timeCardCD, Equal<Current<EPTimeCard.timeCardCD>>>,OrderBy<Asc<EPTimeCardSummary.lineNbr>>> Summary;

        protected  IEnumerable summary()
		{
            EPTimeCard document = Document.Current;

            if (PXView.Currents != null)
                foreach (object current in PXView.Currents)
                {
                    var currentDocument = current as EPTimeCard;
                    if (currentDocument != null)
                        document = currentDocument;
                }

            if (document == null)
                return new List<EPTimeCardSummary>();

            if (document.WeekID == null)
                return new List<EPTimeCardSummary>();

            /*
             * List all summary records that are in tthe database.
             * Add new ones based on the new activities added.
             * Remove the ones that are empty - no activities associated.
             * Update existing ones - new activities must be aggregated              
             */

            
			PXSelectBase<EPTimeCardSummary> select = new PXSelect<EPTimeCardSummary, Where<EPTimeCardSummary.timeCardCD, Equal<Current<EPTimeCard.timeCardCD>>>,OrderBy<Asc<EPTimeCardSummary.lineNbr>>>(this);
			List<string> missingSummaryRecords = new List<string>();

            Dictionary<string, SummaryRecord> sumList = new Dictionary<string, SummaryRecord>();

            foreach (EPTimecardDetail activity in Activities.Select())
            {
	            if (string.IsNullOrEmpty(activity.EarningTypeID))
		            continue;
                string key = string.Format("{0}.{1}.{2}.{3}.{4}", activity.EarningTypeID.ToUpper(),
                                                       activity.ProjectID.GetValueOrDefault(),
                                                       activity.ProjectTaskID.GetValueOrDefault(-1),
                                                       activity.IsBillable.GetValueOrDefault(),
                                                       activity.ParentTaskID.GetValueOrDefault(-1));

                SummaryRecord record = null;
                EPTimeCardSummary summary = GetSummaryRecord(activity);
                if (summary != null)
                {
                    if (sumList.ContainsKey(summary.LineNbr.ToString()))
                    {
                        record = sumList[summary.LineNbr.ToString()];
                    }
                }
                else
                {
                    if (sumList.ContainsKey(key))
                    {
                        record = sumList[key];
                    }
                }
                
                if (activity.SummaryLineNbr != null)
                {
                    if (summary != null)
                    {
                        if (record == null)
                        {
                            record = new SummaryRecord(summary);
                            sumList.Add(record.Summary.LineNbr.ToString(), record);
                        }
                        record.LinkedDetails.Add(activity);
                    }
                    else
                    {
                        Activities.Cache.SetValue<EPTimecardDetail.summaryLineNbr>(activity, null);
                        if (Activities.Cache.GetStatus(activity) == PXEntryStatus.Notchanged)
                            Activities.Cache.SetStatus(activity, PXEntryStatus.Updated);
                        if (record == null)
                        {
                            record = new SummaryRecord(null);
                            sumList.Add(key, record);
                        }
                        record.SummaryKey = key;
                        record.NotLinkedDetails.Add(activity);
                    }
                }
                else
                {
                    if (summary != null)
                    {
                        if (record == null)
                        {
                            record = new SummaryRecord(summary);
                            sumList.Add(record.Summary.LineNbr.ToString(), record);
                        }
                       
                    }
                    else
                    {
                        Activities.Cache.SetValue<EPTimecardDetail.summaryLineNbr>(activity, null);
                        if (Activities.Cache.GetStatus(activity) == PXEntryStatus.Notchanged)
                            Activities.Cache.SetStatus(activity, PXEntryStatus.Updated);
                        if (record == null)
                        {
                            record = new SummaryRecord(null);
                            sumList.Add(key, record);
                        }
                        record.SummaryKey = key;
                    }
                    record.NotLinkedDetails.Add(activity);
                }
            }

            List<EPTimeCardSummary> list = new List<EPTimeCardSummary>();

			foreach ( EPTimeCardSummary item in select.Select() )
			{
			    if (sumList.ContainsKey(item.LineNbr.ToString()))
			    {
				    bool hasChanges = false;

				    EPTimeCardSummary summary = sumList[item.LineNbr.ToString()].Summary;
				    EPTimeCardSummary keyItem = sumList[item.LineNbr.ToString()].SummariseDetails();
				    if (summary.Mon.GetValueOrDefault() != keyItem.Mon.GetValueOrDefault())
				    {
					    hasChanges = true;
					    summary.Mon = keyItem.Mon;
				    }
				    if (summary.Tue.GetValueOrDefault() != keyItem.Tue.GetValueOrDefault())
				    {
					    hasChanges = true;
					    summary.Tue = keyItem.Tue;
				    }
				    if (summary.Wed.GetValueOrDefault() != keyItem.Wed.GetValueOrDefault())
				    {
					    hasChanges = true;
					    summary.Wed = keyItem.Wed;
				    }
				    if (summary.Thu.GetValueOrDefault() != keyItem.Thu.GetValueOrDefault())
				    {
					    hasChanges = true;
					    summary.Thu = keyItem.Thu;
				    }
				    if (summary.Fri.GetValueOrDefault() != keyItem.Fri.GetValueOrDefault())
				    {
					    hasChanges = true;
					    summary.Fri = keyItem.Fri;
				    }
				    if (summary.Sat.GetValueOrDefault() != keyItem.Sat.GetValueOrDefault())
				    {
					    hasChanges = true;
					    summary.Sat = keyItem.Sat;
				    }
				    if (summary.Sun.GetValueOrDefault() != keyItem.Sun.GetValueOrDefault())
				    {
					    hasChanges = true;
					    summary.Sun = keyItem.Sun;
				    }

				    dontSyncDetails = true;
				    try
				    {
					    if (hasChanges)
						    Summary.Update(summary);
				    }
				    finally
				    {
					    dontSyncDetails = false;
				    }
				    list.Add(summary);
			    }
			    else
			    {
				    if (Summary.Cache.GetStatus(item) == PXEntryStatus.Notchanged)
				    {
					    item.Mon = 0;
					    item.Tue = 0;
					    item.Wed = 0;
					    item.Thu = 0;
					    item.Fri = 0;
					    item.Sat = 0;
					    item.Sun = 0;
						dontSyncDetails = true;
						try
						{
							Summary.Update(item);
						}
						finally
						{
							dontSyncDetails = false;
						}
				    }
					list.Add(item);
			    }
            }

            foreach (SummaryRecord record in sumList.Values)
            {
                if (record.SummaryKey != null)
                {
                    string[] parts = record.SummaryKey.Split('.');
                    EPTimeCardSummary keyItem = new EPTimeCardSummary();
                    keyItem.EarningType = parts[0];
                    keyItem.ProjectID = parts[1] == "-1" ? null : (int?) int.Parse(parts[1]);
                    keyItem.ProjectTaskID = parts[2] == "-1" ? null : (int?) int.Parse(parts[2]);
                    keyItem.IsBillable = bool.Parse(parts[3]);
                    keyItem.ParentTaskID = parts[4] == "-1" ? null : (int?) int.Parse(parts[4]);

                    List<EPTimecardDetail> details = GetDetails(keyItem, Document.Current, true);
                    EPTimeCardSummary summary = null;
                    foreach (EPTimecardDetail detail in details)
                    {
                        if (detail.TimeSpent.GetValueOrDefault() != 0)
                            summary = AddToSummary(summary, detail);
                    }

                    if (summary != null)
                        list.Add(summary);
                }
            }


            //populate summary for the unreleased timecard from the previous version:
            if (document.IsReleased != true)
            {
                if (Activities.Select().Count == 0)
                {
                    PXSelectBase<EPTimeCardRecord> selectOldRecords = new PXSelect<EPTimeCardRecord, Where<EPTimeCardRecord.timeCardCD, Equal<Required<EPTimeCardRecord.timeCardCD>>>>(this);
                    foreach (EPTimeCardRecord record in selectOldRecords.Select(document.TimeCardCD))
                    {
                        EPTimeCardSummary summary = (EPTimeCardSummary)Summary.Cache.CreateInstance();
                        summary.Mon = record.Mon;
                        summary.Tue = record.Tue;
                        summary.Wed = record.Wed;
                        summary.Thu = record.Thu;
                        summary.Fri = record.Fri;
                        summary.Sat = record.Sat;
                        summary.Sun = record.Sun;
                        summary.EarningType = EpSetup.Current.RegularHoursType;
                        summary.ProjectID = record.ProjectID;
                        summary.ProjectTaskID = record.TaskID;
                        Summary.Update(summary);
                    }
                }
            }


		    foreach (EPTimeCardSummary sr in list)
		    {
                Debug.WriteLine(sr.ToString());
		    }

			return list;
		}

		
		[PXViewName(Messages.TimeCardActivities)]
        public PXSelectJoin<EPTimecardDetail,
            InnerJoin<EPEmployee, On<EPEmployee.userID, Equal<EPTimecardDetail.owner>>,
			LeftJoin<CRCase, On<CRCase.noteID, Equal<EPTimecardDetail.refNoteID>>,
			LeftJoin<AR.Customer, On<AR.Customer.bAccountID, Equal<CRCase.customerID>>,
			LeftJoin<Contract, On<Contract.contractID, Equal<CRCase.contractID>>,
			LeftJoin<PMProject, On<PMProject.contractID, Equal<EPTimecardDetail.projectID>>>>>>>,
			Where<EPEmployee.bAccountID, Equal<Current<EPTimeCard.employeeID>>,
                And<EPTimecardDetail.weekID, Equal<Current<EPTimeCard.weekId>>,
                And<EPTimecardDetail.classID, NotEqual<CRActivityClass.emailRouting>,
                And<EPTimecardDetail.classID, NotEqual<CRActivityClass.task>,
                And<EPTimecardDetail.classID, NotEqual<CRActivityClass.events>,
                And<EPTimecardDetail.trackTime, Equal<True>,
                And<Where<EPTimecardDetail.timeCardCD, IsNull, Or<EPTimecardDetail.timeCardCD, Equal<Current<EPTimeCard.timeCardCD>>>>>>>>>>>,
            OrderBy<Asc<EPTimecardDetail.startDate>>> Activities;

		protected IEnumerable activities()
		{
			EPTimeCard document = Document.Current;
			
            if (PXView.Currents != null)
                foreach (object current in PXView.Currents)
                {
                    var currentDocument = current as EPTimeCard;
                    if (currentDocument != null)
                        document = currentDocument;
                }

            if (document == null) 
                yield break;

            if (document.WeekID == null)
                yield break;


			bool showAllActivities = document.IsHold == true && document.IsApproved != true &&
				document.IsRejected != true && document.IsReleased != true;

           if (showAllActivities)
			{
				foreach (PXResult<EPTimecardDetail, EPEmployee> row in QSelect(this, Activities.View.BqlSelect))
			    {
                    yield return row;
			    }
			}
			else
			{
				foreach (PXResult<EPTimecardDetail, EPEmployee, CRCase, AR.Customer, Contract, PMProject> row in
                    PXSelectJoin<EPTimecardDetail,
                    InnerJoin<EPEmployee, On<EPEmployee.userID, Equal<EPTimecardDetail.owner>>,
					LeftJoin<CRCase, On<CRCase.noteID, Equal<EPTimecardDetail.refNoteID>>,
					LeftJoin<AR.Customer, On<AR.Customer.bAccountID, Equal<CRCase.customerID>>,
					LeftJoin<Contract, On<Contract.contractID, Equal<CRCase.contractID>>,
					LeftJoin<PMProject, On<PMProject.contractID, Equal<EPTimecardDetail.projectID>>>>>>>,
                    Where<EPTimecardDetail.timeCardCD, Equal<Required<EPTimeCard.timeCardCD>>>,
                    OrderBy<Asc<EPTimecardDetail.startDate>>>.
					Select(this, document.TimeCardCD))
				{
                    yield return row;
				}
			}
		}

        [PXCopyPasteHiddenFields(typeof(EPTimeCardItem.noteID), typeof(Note.noteText))]
        public PXSelect<EPTimeCardItem,
            Where<EPTimeCardItem.timeCardCD, Equal<Current<EPTimeCard.timeCardCD>>>,
            OrderBy<Asc<EPTimeCardItem.lineNbr>>> Items;

        public PXSelectJoin<EPTimecardTask,
            InnerJoin<EPEmployee, On<EPEmployee.userID, Equal<EPTimecardTask.owner>>>,
			Where<EPEmployee.bAccountID, Equal<Current<EPTimeCard.employeeID>>, And<EPTimecardTask.classID, Equal<CRActivityClass.task>>>> Tasks;

            //TODO: Check with Kesha if this is required and implemented correctly!
		[PXViewName(Messages.TimeCardApproval)]
		public EPApprovalAction<EPTimeCard, EPTimeCard.isApproved, EPTimeCard.isRejected> Approval;

        //TODO: Check with Kesha if this is required and implemented correctly!
		private BqlCommand _approvalCommand;

        //TODO: Check with Kesha if this is required and implemented correctly!
		protected IEnumerable approval()
		{
			return this.QuickSelect(_approvalCommand);
		}

		#endregion
		
		public TimeCardMaint()
		{
            this.CopyPaste.SetVisible(false);

			if (EpSetup.Current.TimeCardNumberingID == null)
				throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(EPSetup), typeof(EPSetup).Name);

			if (EpSetup.Current.TimeCardAssignmentMapID == null)
				throw new PXSetupNotEnteredException(Messages.EmptyTimecardAssignMap, typeof(EPSetup), typeof(EPSetup).Name);

			EPEmployee employeeByUserID = PXSelect<EPEmployee, Where<EP.EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>.Select(this);
			if (employeeByUserID == null)
			{
				//throw new PXException(Messages.MustBeEmployee);
				Redirector.Redirect(System.Web.HttpContext.Current, string.Format("~/Frames/Error.aspx?exceptionID={0}&typeID={1}", Messages.MustBeEmployee, "error"));
			}

            PXUIFieldAttribute.SetEnabled<EPTimecardDetail.endDate>(Activities.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<EPTimecardDetail.overtimeSpent>(Activities.Cache, null, !ShowActivityTime);
            PXUIFieldAttribute.SetEnabled<EPTimecardDetail.uistatus>(Activities.Cache, null, false);
            PXUIFieldAttribute.SetDisplayName<EPTimecardDetail.endDate>(Activities.Cache, Messages.CompletedAt);

            PXDBDateAndTimeAttribute.SetTimeVisible<EPTimecardDetail.startDate>(Activities.Cache, null, ShowActivityTime);
            PXUIFieldAttribute.SetVisible<EPTimecardDetail.endDate>(Activities.Cache, null, ShowActivityTime);
			
		    bool timeReportingInstalled = PXAccess.FeatureInstalled<CS.FeaturesSet.timeReportingModule>();
            if (!timeReportingInstalled)
            {
                preloadFromTasks.SetVisible(false);
                PXUIFieldAttribute.SetVisible<EPTimeCardSummary.parentTaskID>(Summary.Cache, null, false);
                PXUIFieldAttribute.SetVisible<EPTimecardDetail.approvalStatus>(Summary.Cache, null, false);
                createActivity.SetVisible(false);
            }

            
            //TODO: Check with Kesha if this is required and implemented correctly!
			//InitializeApproval:
			var command = Approval.View.BqlSelect.GetType();
			command = BqlCommand.AppendJoin(command,
				typeof(LeftJoin<ApproverEmployee, On<ApproverEmployee.userID, Equal<EPApproval.ownerID>>>));
			command = BqlCommand.AppendJoin(command,
				typeof(LeftJoin<ApprovedByEmployee, On<ApprovedByEmployee.userID, Equal<EPApproval.approvedByID>>>));
			_approvalCommand = BqlCommand.CreateInstance(command);

            //TODO: Check with Kesha if this is required and implemented correctly!
			CRCaseActivityHelper.Attach(this);

            //TODO: Check with Kesha if this is required and implemented correctly!
            var startDate_date_fieldName = typeof(EPTimecardDetail.startDate).Name + PXDBDateAndTimeAttribute.DATE_FIELD_POSTFIX;
            FieldUpdating.AddHandler(typeof(EPTimecardDetail), startDate_date_fieldName, EPTimecardDetail_StartDate_Date_FieldUpdating);
            FieldUpdated.AddHandler(typeof(EPTimecardDetail), startDate_date_fieldName, EPTimecardDetail_StartDate_Date_FieldUpdated);
            FieldVerifying.AddHandler(typeof(EPTimecardDetail), startDate_date_fieldName, EPTimecardDetail_StartDate_Date_FieldVerifying);
            var startDate_time_fieldName = typeof(EPTimecardDetail.startDate).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX;
            FieldUpdating.AddHandler(typeof(EPTimecardDetail), startDate_time_fieldName, EPTimecardDetail_StartDate_Time_FieldUpdating);
		}
        
		public static IEnumerable QSelect(PXGraph graph, BqlCommand bqlCommand)
		{
			var view = new PXView(graph, false, bqlCommand);
			var startRow = PXView.StartRow;
			int totalRows = 0;
			var list = view.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
								   ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;
			return list;
		}
        

        #region Setup Settings

        /// <summary>
        /// Gets the source for the generated PMTran.AccountID
        /// </summary>
        public string ExpenseAccountSource
        {
            get
            {
                string result = PM.PMExpenseAccountSource.InventoryItem;

                PMSetup setup = PXSelect<PMSetup>.Select(this);
                if (setup != null && !string.IsNullOrEmpty(setup.ExpenseAccountSource))
                {
                    result = setup.ExpenseAccountSource;
                }

                return result;
            }
        }

        public string ExpenseSubMask
        {
            get
            {
                string result = null;

                PMSetup setup = PXSelect<PMSetup>.Select(this);
                if (setup != null && !string.IsNullOrEmpty(setup.ExpenseSubMask))
                {
                    result = setup.ExpenseSubMask;
                }

                return result;
            }
        }

        public string ExpenseAccrualAccountSource
        {
            get
            {
                string result = PM.PMExpenseAccountSource.InventoryItem;

                PMSetup setup = PXSelect<PMSetup>.Select(this);
                if (setup != null && !string.IsNullOrEmpty(setup.ExpenseAccountSource))
                {
                    result = setup.ExpenseAccrualAccountSource;
                }

                return result;
            }
        }

        public string ExpenseAccrualSubMask
        {
            get
            {
                string result = null;

                PMSetup setup = PXSelect<PMSetup>.Select(this);
                if (setup != null && !string.IsNullOrEmpty(setup.ExpenseAccrualSubMask))
                {
                    result = setup.ExpenseAccrualSubMask;
                }

                return result;
            }
        }

        public string ActivityTimeUnit
        {
            get
            {
                string result = EPSetup.Minute;

                EPSetup setup = PXSelect<EPSetup>.Select(this);
                if (setup != null && !string.IsNullOrEmpty(setup.ActivityTimeUnit))
                {
                    result = setup.ActivityTimeUnit;
                }

                return result;
            }
        }

        public string EmployeeRateUnit
        {
            get
            {
                string result = EPSetup.Hour;

                EPSetup setup = PXSelect<EPSetup>.Select(this);
                if (setup != null && !string.IsNullOrEmpty(setup.EmployeeRateUnit))
                {
                    result = setup.EmployeeRateUnit;
                }

                return result;
            }
        }

        public bool ShowActivityTime
        {
            get
            {
                bool result = false;

                EPSetup setup = PXSelect<EPSetup>.Select(this);
                if (setup != null)
                {
                    result = setup.RequireTimes == true;
                }

                return result;
            }
        }
        #endregion

		#region Actions

		public PXAction<EPTimeCard> viewActivity;
		[PXUIField(Visible = false)]
		[PXButton]
		protected virtual IEnumerable ViewActivity(PXAdapter adapter)
		{
			if (Activities.Current != null)
			{
				this.Save.Press();

				var keys = Data.EP.ActivityService.GetKeys(Activities.Current);
				Data.EP.ActivityService.Open(keys);
			}
			return adapter.Get();
		}

		public PXAction<EPTimeCard> createActivity;
		[PXUIField(DisplayName = Messages.AddActivity)]
		[PXButton(Tooltip = Messages.AddActivityTooltip,
			ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		protected virtual IEnumerable CreateActivity(PXAdapter adapter)
		{
			var doc = Document.Current;
			if (doc != null)
			{
				new ActivityService().CreateActivity(null, EpSetup.Current.DefaultActivityType);
			}
			return adapter.Get();
		}

		public PXAction<EPTimeCard> action;
		[PXUIField(DisplayName = Messages.Actions)]
		[PXButton]
		protected virtual IEnumerable Action(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<EPTimeCard> assign;
		[PXUIField(DisplayName = Messages.Assign)]
		[PXButton]
		protected virtual IEnumerable Assign(PXAdapter adapter)
		{
			var mapId = EpSetup.Current.TimeCardAssignmentMapID;
			if (mapId == null)
				throw new PXSetPropertyException(Messages.EmptyTimecardAssignMap, "Time & Expenses Preferences");

			var document = Document.Current;
			if (document != null)
				Approval.Assign(document, mapId);

			return adapter.Get();
		}

		public PXAction<EPTimeCard> release;
		[PXUIField(DisplayName = Messages.Release)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Release)]
		protected virtual IEnumerable Release(PXAdapter adapter)
		{
			var releaseGraph = (PM.RegisterEntry)PXGraph.CreateInstance(typeof(PM.RegisterEntry));
            
			foreach (EPTimeCard item in adapter.Get<EPTimeCard>())
			{
				if (item.IsReleased == true)
				{
					throw new PXException(Messages.AlreadyReleased);
				}

				releaseGraph.Clear();

			    PMSetup setup = PXSelect<PMSetup>.Select(releaseGraph);
                if (setup == null)
                {
                    //Setup may be null because the PM module is not enabled. and yet we need to be able to generate PMTran for the Contract billing.
                    releaseGraph.Setup.Insert();
                }
                
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					if (string.IsNullOrEmpty(item.OrigTimeCardCD))
					{
						ProcessRegularTimecard(releaseGraph, item);
					}
					else
					{
						ProcessCorrectingTimecard(releaseGraph, item);
					}

					releaseGraph.Save.Press();

                    item.Status = EPTimeCard.ReleasedStatus;
					item.IsReleased = true;
					Document.Update(item);

					this.Save.Press();

					ts.Complete();
				}

				if (EpSetup.Current.AutomaticReleasePM == true)
				{
					PX.Objects.PM.RegisterRelease.Release(releaseGraph.Document.Current);
				}


				yield return item;
			}
		}

        public PXAction<EPTimeCard> correct;
		[PXUIField(DisplayName = Messages.Correct)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Release)]
        protected virtual IEnumerable Correct(PXAdapter adapter)
		{
            if (Document.Current != null)
            {
				EPTimeCard source = GetLastCorrection(Document.Current);

	            if (source.IsReleased != true)
					return new EPTimeCard[] { source };

                EPTimeCard newCard = (EPTimeCard)Document.Cache.Insert();
                newCard.EmployeeID = source.EmployeeID;
                newCard.WeekID = source.WeekID;
                newCard.OrigTimeCardCD = source.TimeCardCD;

                foreach (PXResult<EPTimecardDetail> actRow in Activities.View.SelectMultiBound(new object[]{source}))
                {
                    EPTimecardDetail act = (EPTimecardDetail)actRow;

                    EPTimecardDetail newActivity = PXCache<EPTimecardDetail>.CreateCopy(act);
                    newActivity.Released = false;
                    newActivity.TaskID = null;
                    newActivity.TimeCardCD = null;
                    newActivity.TimeSheetCD = null;
                    newActivity.OrigTaskID = act.TaskID;//relation between the original activity and the corrected one.
                    newActivity.SummaryLineNbr = null;
                    
                    newActivity = Activities.Insert(newActivity);
                    newActivity.RefNoteID = act.RefNoteID;

                    Activities.Cache.SetValue<EPTimecardDetail.isCorrected>(act, true);
                    Activities.Cache.SetStatus(act, PXEntryStatus.Updated);
                }

                foreach (EPTimeCardItem item in Items.View.SelectMultiBound(new object[] { source }))
                {
                    EPTimeCardItem record = Items.Insert();
                    record.ProjectID = item.ProjectID;
                    record.TaskID = item.TaskID;
                    record.Description = item.Description;
                    record.InventoryID = item.InventoryID;
                    record.UOM = item.UOM;
                    record.Mon = item.Mon;
                    record.Tue = item.Tue;
                    record.Wed = item.Wed;
                    record.Thu = item.Thu;
                    record.Fri = item.Fri;
                    record.Sat = item.Sat;
                    record.Sun = item.Sun;
                    record.OrigLineNbr = item.LineNbr;//relation between the original activity and the corrected one.
                }


                return new EPTimeCard[] { newCard };
            }

		    return adapter.Get();
		}

        
        public PXAction<EPTimeCard> preloadFromTasks;
        [PXUIField(DisplayName = Messages.PreloadFromTasks)]
        [PXButton]
        protected virtual void PreloadFromTasks()
        {
            if (Tasks.AskExt() == WebDialogResult.OK)
            {
                foreach (EPTimecardTask task in Tasks.Cache.Updated)
                {
                    if (task.Selected == true)
                    {
						bool alreadyExists = false;
						foreach (EPTimeCardSummary existing in Summary.Select())
						{
							if (existing.ParentTaskID == task.ProjectTaskID && existing.ProjectID == task.ProjectID &&
								existing.ProjectTaskID == task.ProjectTaskID)
							{
								alreadyExists = true;
								break;
							}
						}

						if (alreadyExists)
							continue;

                        EPTimeCardSummary summary = (EPTimeCardSummary) Summary.Cache.CreateInstance();
						summary.ParentTaskID = task.TaskID;
						summary.ProjectID = task.ProjectID;
						summary.ProjectTaskID = task.ProjectTaskID;
						summary.IsBillable = task.IsBillable;
						summary.Description = task.Subject;

						summary = Summary.Insert(summary);
                    }
                }
            }
        }

        public PXAction<EPTimeCard> preloadFromPreviousTimecard;
        [PXUIField(DisplayName = Messages.PreloadFromPreviousTimecard)]
        [PXButton]
        protected virtual void PreloadFromPreviousTimecard()
        {
            if (Document.Current == null)
                return;

            if (Document.Current.WeekID == null)
                return;

            EPEmployee employee =
                PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPTimeCard.employeeID>>>>.Select(this);

            if (employee == null)
                return;

            EPTimeCard previous = PXSelect<EPTimeCard, Where<EPTimeCard.employeeID, Equal<Current<EPTimeCard.employeeID>>, And<EPTimeCard.weekId, Equal<Required<EPTimeCard.weekId>>>>>.Select(this, Document.Current.WeekID.Value - 1);
            if (previous == null)
                return;

            PXSelectBase<EPTimeCardSummary> summarySelect = new PXSelect<EPTimeCardSummary, Where<EPTimeCardSummary.timeCardCD, Equal<Required<EPTimeCardSummary.timeCardCD>>>>(this);
            foreach (EPTimeCardSummary item in summarySelect.Select(previous.TimeCardCD))
            {
                if (item.EarningType != EpSetup.Current.HolidaysType &&
                    item.EarningType != EpSetup.Current.VacationsType)
                {

					bool alreadyExists = false;
					foreach (EPTimeCardSummary existing in Summary.Select())
					{
						if (existing.ParentTaskID == item.ProjectTaskID && existing.ProjectID == item.ProjectID &&
							existing.ProjectTaskID == item.ProjectTaskID)
						{
							alreadyExists = true;
							break;
						}
					}

					if (alreadyExists)
						continue;

                    EPTimeCardSummary summary = (EPTimeCardSummary) PXCache<EPTimeCardSummary>.CreateCopy(item);
                    summary.TimeCardCD = null;
                    summary.Description = null;
                    summary.Mon = null;
                    summary.Tue = null;
                    summary.Wed = null;
                    summary.Thu = null;
                    summary.Fri = null;
                    summary.Sat = null;
                    summary.Sun = null;

                    Summary.Insert(summary);
                }
            }

        }

        public PXAction<EPTimeCard> preloadHolidays;
        [PXUIField(DisplayName = Messages.PreloadHolidays)]
        [PXButton]
        protected virtual void PreloadHolidays()
        {
            if (Document.Current == null)
                return;

            if (Document.Current.WeekStartDate == null)
                return;

            EPEmployee employee =
                PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPTimeCard.employeeID>>>>.Select(this);

            if (employee == null)
                return;

			EPEmployeeRate rate = CreateEmployeeCostEngine().GetEmployeeRate(Document.Current.EmployeeID, Document.Current.WeekStartDate);
			if (rate == null || rate.RegularHours == null)
                return;

            int hrsPerDay = rate.RegularHours.Value / 5;

            DateTime firstDay = Document.Current.WeekStartDate.Value;
            if (firstDay.DayOfWeek == DayOfWeek.Sunday)
                firstDay = firstDay.AddDays(1);
            
            EPTimeCardSummary summary = new EPTimeCardSummary();
            summary.ProjectID = PM.ProjectDefaultAttribute.NonProject(this);
            summary.EarningType = EpSetup.Current.HolidaysType;
            if (!CalendarHelper.IsWorkDay(this, employee.CalendarID, firstDay))
            {
                summary.Mon = hrsPerDay * 60;
            }
            if (!CalendarHelper.IsWorkDay(this, employee.CalendarID, firstDay.AddDays(1)))
            {
                summary.Tue = hrsPerDay * 60;
            }
            if (!CalendarHelper.IsWorkDay(this, employee.CalendarID, firstDay.AddDays(2)))
            {
                summary.Wed = hrsPerDay * 60;
            }
            if (!CalendarHelper.IsWorkDay(this, employee.CalendarID, firstDay.AddDays(3)))
            {
                summary.Thu = hrsPerDay * 60;
            }
            if (!CalendarHelper.IsWorkDay(this, employee.CalendarID, firstDay.AddDays(4)))
            {
                summary.Fri = hrsPerDay * 60;
            }
           
            summary.IsBillable = false;
            summary.Description = "Holiday";

            if ( summary.GetTimeTotal() > 0 )
                Summary.Insert(summary);
        }

        public PXAction<EPTimeCard> normalizeTimecard;
        [PXUIField(DisplayName = Messages.NormalizeTimecard)]
        [PXButton]
        protected virtual void NormalizeTimecard()
        {
            if (Document.Current == null)
                return;
            
            if (!Summary.Cache.AllowInsert)
                return;

			EPEmployeeRate rate = CreateEmployeeCostEngine().GetEmployeeRate(Document.Current.EmployeeID, Document.Current.WeekStartDate);
            if ( rate == null || rate.RegularHours == null)
                return;

            int minutesPerday = rate.RegularHours.Value*60/5;

            int delta = (rate.RegularHours.GetValueOrDefault() * 60) - Document.Current.TimeSpentCalc.GetValueOrDefault();

            if (delta <= 0)
                return;

            EPTimeCardSummary summary = PXSelect<EPTimeCardSummary, Where<EPTimeCardSummary.timeCardCD, Equal<Current<EPTimeCard.timeCardCD>>,
                And<EPTimeCardSummary.earningType, Equal<Required<EPTimeCardSummary.earningType>>,
                And<EPTimeCardSummary.projectID, Equal<Required<EPTimeCardSummary.projectID>>,
                And<EPTimeCardSummary.isBillable, Equal<False>>>>>>.Select(this, EpSetup.Current.RegularHoursType, PM.ProjectDefaultAttribute.NonProject(this));

            if (summary == null)
            {
                summary = (EPTimeCardSummary) Summary.Cache.CreateInstance();
                summary.ProjectID = PM.ProjectDefaultAttribute.NonProject(this);
                summary.IsBillable = false;
                summary.Description = "Normalization";
                summary = Summary.Insert(summary);
            }

            int monQuota = minutesPerday - summary.Mon.GetValueOrDefault();
            summary.Mon = summary.Mon.GetValueOrDefault() + delta < monQuota ? delta : monQuota;
            delta = delta - monQuota;

            if (delta >= 0)
            {
                int tueQuota = minutesPerday - summary.Tue.GetValueOrDefault();
                summary.Tue = summary.Tue.GetValueOrDefault() + delta < tueQuota ? delta : tueQuota;
                delta = delta - tueQuota;
            }

            if (delta >= 0)
            {
                int wedQuota = minutesPerday - summary.Wed.GetValueOrDefault();
                summary.Wed = summary.Wed.GetValueOrDefault() + delta < wedQuota ? delta : wedQuota;
                delta = delta - wedQuota;
            }

            if (delta >= 0)
            {
                int thuQuota = minutesPerday - summary.Thu.GetValueOrDefault();
                summary.Thu = summary.Thu.GetValueOrDefault() + delta < thuQuota ? delta : thuQuota;
                delta = delta - thuQuota;
            }

            if (delta >= 0)
            {
                int friQuota = minutesPerday - summary.Fri.GetValueOrDefault();
                summary.Fri = summary.Fri.GetValueOrDefault() + delta < friQuota ? delta : friQuota;
                delta = delta - friQuota;
            }

            //by this time delta should be less then equal to zero
            Debug.Assert(delta <=0);

            Summary.Update(summary);
        }

        public PXAction<EPTimeCard> View;
        [PXUIField(DisplayName = Messages.View)]
        [PXButton()]
        public virtual IEnumerable view(PXAdapter adapter)
        {
            var row = Activities.Current;
            if (row != null)
            {
				if (Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Inserted)
				{
					throw new PXException(Messages.TimecardMustBeSaved);
				}
	            PXAutomation.GetStep(this,
	                                                  new object[] {Views[AutomationView].Cache.Current},
	                                                  BqlCommand.CreateInstance(typeof (Select<>), Views[AutomationView].Cache.GetItemType())
										            );

				var result = new List<object>(Save.Press(adapter).Cast<object>());
                PXRedirectHelper.TryRedirect(this, row, PXRedirectHelper.WindowMode.NewWindow);
				return result;

            }
            return adapter.Get();
        }

		public PXAction<EPTimeCard> viewPMTran;
		[PXUIField(DisplayName = PM.Messages.ViewTransactions, FieldClass = ProjectAttribute.DimensionName)]
	    [PXButton()]
		public virtual IEnumerable ViewPMTran(PXAdapter adapter)
	    {
			if (Document.Current != null)
			{
				RegisterEntry graph = PXGraph.CreateInstance<RegisterEntry>();
				graph.Clear();
				PMRegister registr = PXSelect<PMRegister
					, Where<PMRegister.origDocType, Equal<PMOrigDocType.timeCard>
						, And<PMRegister.origDocNbr, Equal<Current<EPTimeCard.timeCardCD>>>>>.SelectSingleBound(this, new object[] { Document});
				if (registr == null)
				{
					adapter.View.Ask(Messages.TransactionNotExists, MessageButtons.OK);
				}
				else
				{
					graph.Document.Current = registr;
					throw new PXRedirectRequiredException(graph, PM.Messages.ViewTransactions);
				}
			} 
			return adapter.Get();
	    }

	    #endregion

		#region Event Handlers

	    protected virtual void EPTimecardDetail_EarningTypeID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
	    {
		    EPTimecardDetail row = (EPTimecardDetail) e.Row;
			if (row != null && e.NewValue == null)
		    {
			    e.Cancel = true;
				throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(EPTimecardDetail.earningTypeID).Name);
		    }
	    }

		protected virtual void EPTimeCardSummary_EarningType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
	    {
			EPTimeCardSummary row = (EPTimeCardSummary)e.Row;
			if (row != null && e.NewValue == null)
		    {
			    e.Cancel = true;
				throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(EPTimeCardSummary.earningType).Name);
		    }
	    }

		

	    protected virtual void EPTimeCard_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
            EPTimeCard row = e.Row as EPTimeCard;
			if (row == null) return;

			if (row.WeekID == null)
				row.WeekID = GetNextWeekID(row.EmployeeID);
        }

		protected virtual void EPTimeCard_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as EPTimeCard;
			var oldRow = e.OldRow as EPTimeCard;
			if (row == null || oldRow == null) return;

			var employeeChanged = row.EmployeeID != oldRow.EmployeeID;
			if (row.WeekID == null || employeeChanged)
				row.WeekID = GetNextWeekID(row.EmployeeID);

            if (row.Status == EPTimeCard.HoldStatus && oldRow.Status == EPTimeCard.OpenStatus)
            {
                PXSelectBase<EPApproval> select = new PXSelect<EPApproval, Where<EPApproval.refNoteID, Equal<Required<EPApproval.refNoteID>>>>(this);
                foreach (EPApproval approval in select.Select(row.NoteID))
                {
                    this.Caches[typeof (EPApproval)].Delete(approval);
                }
            }

        }

	    protected virtual void EPTimeCard_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
	    {
			var row = e.Row as EPTimeCard;
			if (row == null) return;

			foreach (EPTimeCardSummary summary in Summary.Select())
			{
				try
				{
					dontSyncDetails = true;
					Summary.Delete(summary);
				}
				finally
				{
					dontSyncDetails = false;
				}
			}

			try
			{
				dontSyncSummary = true;
				PXSelectBase<EPTimecardDetail> selectDetail = new PXSelect<EPTimecardDetail
					, Where<
						EPTimecardDetail.weekID, Equal<Current<EPTimeCard.weekId>>
						, And<EPTimecardDetail.released, NotEqual<True>
							, And<EPTimecardDetail.isCorrected, NotEqual<True>
								, And2<Where<EPTimecardDetail.summaryLineNbr, IsNotNull, Or<EPTimecardDetail.origTaskID, IsNotNull>>
									, And<Where<EPTimecardDetail.timeCardCD, IsNull, Or<EPTimecardDetail.timeCardCD, Equal<Current<EPTimeCard.timeCardCD>>>>>
									>
								>
							>
						>
					>(this);
				foreach (EPTimecardDetail activity in selectDetail.Select())
				{
					if (activity.OrigTaskID != null)
					{
						EPTimecardDetail originActivity = PXSelect<EPTimecardDetail>.Search<EPTimecardDetail.taskID>(this, activity.OrigTaskID);
						if (originActivity != null)
						{
							originActivity.IsCorrected = false;
							Activities.Update(originActivity);
						}
					}
					Activities.Delete(activity);
				}

			}
            finally
            {
				dontSyncSummary = false;
			}

		}


	    protected virtual void EPTimeCard_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
		    EPTimeCard row = (EPTimeCard) e.Row;
            EPTimeCard timeCard = PXSelect<EPTimeCard
								, Where<
									EPTimeCard.employeeID, Equal<Current<EPTimeCard.employeeID>>
										, And<EPTimeCard.weekId, Greater<Current<EPTimeCard.weekId>>
											, And<Where<EPTimeCard.timeCardCD, Equal<Current<EPTimeCard.origTimeCardCD>>, Or<Current<EPTimeCard.origTimeCardCD>, IsNull>>>
										>
									>
								>.SelectWindowed(this, 0, 1);
			if (timeCard != null && timeCard.TimeCardCD != row.OrigTimeCardCD)
            {
                throw new PXException(Messages.TimeCardNoDelete);
            }

		}

        protected virtual void EPTimeCard_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as EPTimeCard;
			if (row == null) return;

			var isSubordinatedEmployee = row.EmployeeID == null ||
				PXSubordinateSelectorAttribute.IsSubordinated(this, row.EmployeeID);

			if (!isSubordinatedEmployee)
			{
				PXUIFieldAttribute.SetEnabled(sender, row, false);
				PXUIFieldAttribute.SetEnabled<EPTimeCard.timeCardCD>(sender, row, true);
			}

	        EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPTimeCard.employeeID>>>>.Select(this);
			if (employee != null)
			{
				PXTimeZoneInfo currentTimeZone = LocaleInfo.GetTimeZone();

				UserPreferences pref = PXSelect<UserPreferences, Where<UserPreferences.userID, Equal<Required<UserPreferences.userID>>>>.Select(sender.Graph, employee.UserID);
				if (pref != null && !string.IsNullOrEmpty(pref.TimeZone) && currentTimeZone.Id != pref.TimeZone)
				{
					PXTimeZoneInfo empTimeZone = PXTimeZoneInfo.FindSystemTimeZoneById(pref.TimeZone);
					PXUIFieldAttribute.SetWarning<EPTimeCard.employeeID>(sender, e.Row, Messages.TimeInTimezoneOfEmployee + empTimeZone.DisplayName);
				}
				else if (employee.CalendarID != null)
				{
					CSCalendar cal = PXSelect<CSCalendar, Where<CSCalendar.calendarID, Equal<Required<CSCalendar.calendarID>>>>.Select(this, employee.CalendarID);
					if (cal != null && !string.IsNullOrEmpty(cal.TimeZone) && currentTimeZone.Id != cal.TimeZone)
					{
						PXTimeZoneInfo empTimeZone = PXTimeZoneInfo.FindSystemTimeZoneById(cal.TimeZone);
							PXUIFieldAttribute.SetWarning<EPTimeCard.employeeID>(sender, e.Row, Messages.TimeInTimezoneOfEmployee + empTimeZone.DisplayName);
					}
				}
			}
			

			Document.Cache.AllowDelete = isSubordinatedEmployee && row.IsReleased != true && row.Status == EPTimeCard.HoldStatus;

			var allowEdit = isSubordinatedEmployee &&
				row.IsHold == true && row.IsApproved != true &&
				row.IsRejected != true && row.IsReleased != true;

		    Activities.Cache.AllowInsert = allowEdit;
			Activities.Cache.AllowUpdate = allowEdit;
			Activities.Cache.AllowDelete = allowEdit;
            Summary.Cache.AllowInsert = allowEdit;
            Summary.Cache.AllowUpdate = allowEdit;
            Summary.Cache.AllowDelete = allowEdit;
            Items.Cache.AllowInsert = allowEdit;
            Items.Cache.AllowUpdate = allowEdit;
            Items.Cache.AllowDelete = allowEdit;
            preloadFromTasks.SetEnabled(allowEdit);
            preloadFromPreviousTimecard.SetEnabled(allowEdit);
            preloadHolidays.SetEnabled(allowEdit);
            normalizeTimecard.SetEnabled(allowEdit);
			viewPMTran.SetEnabled(row.IsReleased == true);

			var isInserted = sender.GetStatus(row) == PXEntryStatus.Inserted;
			createActivity.SetEnabled(allowEdit && !isInserted);
            PXUIFieldAttribute.SetEnabled<EPTimeCard.status>(sender, row, !isInserted);

            if (row.EmployeeID != null)
            {
                if (Summary.Select().Count > 0)
                {
                    PXUIFieldAttribute.SetEnabled<EPTimeCard.employeeID>(sender, row, false);
                }
                else
                {
                    PXUIFieldAttribute.SetEnabled<EPTimeCard.employeeID>(sender, row, allowEdit);
                }
            }

			var isFirst = IsFirstTimeCard(row.EmployeeID);
			PXUIFieldAttribute.SetEnabled<EPTimeCard.weekId>(sender, row, isFirst && allowEdit);

		    RecalculateTotals(row);
            string errorMsg;
		    ValidateTotals(row, out errorMsg);

            row.TimecardType = string.IsNullOrEmpty(row.OrigTimeCardCD) ? "N" : "C";
            if (row.IsReleased == true)
            {
                EPTimeCard correction = PXSelect<EPTimeCard, Where<EPTimeCard.origTimeCardCD, Equal<Required<EPTimeCard.origTimeCardCD>>>>.Select(this, row.TimeCardCD);
                if (correction != null)
                {
                    row.TimecardType = "D";
                }
            }

	        if (row.WeekID != null)
	        {
		        PX.Objects.EP.PXWeekSelector2Attribute.WeekInfo weekInfo = PXWeekSelector2Attribute.GetWeekInfo(this,row.WeekID.Value);
				PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.mon>(Summary.Cache, null, weekInfo.Mon.Enabled);
		        PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.tue>(Summary.Cache, null, weekInfo.Tue.Enabled);
		        PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.wed>(Summary.Cache, null, weekInfo.Wed.Enabled);
		        PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.thu>(Summary.Cache, null, weekInfo.Thu.Enabled);
		        PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.fri>(Summary.Cache, null, weekInfo.Fri.Enabled);
		        PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.sat>(Summary.Cache, null, weekInfo.Sat.Enabled);
		        PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.sun>(Summary.Cache, null, weekInfo.Sun.Enabled);
		        PXUIFieldAttribute.SetEnabled<EPTimeCardItem.mon>(Items.Cache, null, weekInfo.Mon.Enabled);
		        PXUIFieldAttribute.SetEnabled<EPTimeCardItem.tue>(Items.Cache, null, weekInfo.Tue.Enabled);
		        PXUIFieldAttribute.SetEnabled<EPTimeCardItem.wed>(Items.Cache, null, weekInfo.Wed.Enabled);
		        PXUIFieldAttribute.SetEnabled<EPTimeCardItem.thu>(Items.Cache, null, weekInfo.Thu.Enabled);
		        PXUIFieldAttribute.SetEnabled<EPTimeCardItem.fri>(Items.Cache, null, weekInfo.Fri.Enabled);
		        PXUIFieldAttribute.SetEnabled<EPTimeCardItem.sat>(Items.Cache, null, weekInfo.Sat.Enabled);
		        PXUIFieldAttribute.SetEnabled<EPTimeCardItem.sun>(Items.Cache, null, weekInfo.Sun.Enabled);
	        }
		}

		protected virtual void EPTimeCard_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
            EPTimeCard row = e.Row as EPTimeCard;
			if (row == null) return;

			bool isOpen = row.IsHold != true && row.IsApproved != true &&
				row.IsRejected != true && row.IsReleased != true;
			bool isHold = row.IsHold == true && row.IsApproved != true &&
				row.IsRejected != true && row.IsReleased != true;

			if (isOpen)
			{
                List<EPTimecardDetail> details = new List<EPTimecardDetail>();
                PXView detailsView = new PXView(this, false, Activities.View.BqlSelect);
                foreach (PXResult<EPTimecardDetail, EPEmployee> res in detailsView.SelectMultiBound(new object[] {row}))
                {
                    EPTimecardDetail act = (EPTimecardDetail) res;
                    details.Add(act);
					if (act.UIStatus != ActivityStatusAttribute.Completed && act.UIStatus != ActivityStatusAttribute.Canceled && act.UIStatus != ActivityStatusAttribute.Approved)
						Activities.Cache.RaiseExceptionHandling<EPTimecardDetail.startDate>(act, null, new PXSetPropertyException(Messages.ActivityIsNotCompleted, PXErrorLevel.RowError));
                }

                RecalculateTotals(row, details);
				string errorMsg;
				if (!ValidateTotals(row, out errorMsg))
				{
					EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.Select(this, row.EmployeeID);
					if (employee.HoursValidation == HoursValidationOption.Validate)
					{
						sender.SetValue<EPTimeCard.isHold>(row, true);
						sender.SetValue<EPTimeCard.status>(row, EPTimeCard.HoldStatus);
						throw new PXException(Messages.TimecardIsNotValid + " " + errorMsg);
					}
				}
                foreach (EPTimecardDetail act in details)
                {
                    int? laborClassID = null;

                    #region Initialize LaborClass and raise an error if one is not found

					laborClassID = CreateEmployeeCostEngine().GetLaborClass(act);

                    if (laborClassID == null)
                    {
                        sender.SetValue<EPTimeCard.isHold>(row, true);
                        sender.SetValue<EPTimeCard.status>(row, EPTimeCard.HoldStatus);
                        
                        if (act.IsOvertimeCalc == true)
                            throw new PXException(Messages.OvertimeLaborClassNotSpecified);
                        else
                            throw new PXException(Messages.LaborClassNotSpecified);
                    }

                    #endregion

					EPTimecardDetail cp = (EPTimecardDetail)Activities.Cache.CreateCopy(act);
					cp.LabourItemID = laborClassID;
					cp.TimeCardCD = row.TimeCardCD;
                    
                    try
                    {
                        dontSyncSummary = true;
						Activities.Update(cp);
                    }
                    finally
                    {
                        dontSyncSummary = false;
                    }
			    }
            }
			if (isHold)
			{
				foreach (EPTimecardDetail item in Activities.Select())
				{
					EPTimecardDetail cp = (EPTimecardDetail)Activities.Cache.CreateCopy(item);
					cp.TimeCardCD = null;
					dontSyncSummary = true;
					try
					{
						Activities.Update(cp);
					}
                    finally
                    {
                        dontSyncSummary = false;
                    }
                }
			}
		}

       protected virtual void EPTimeCard_EmployeeID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			EPTimeCard row = e.Row as EPTimeCard;
			if (row != null)
			{
				EPTimeCard futureTimeCard = PXSelect<EPTimeCard, Where<EPTimeCard.employeeID, Equal<Current<EPTimeCard.employeeID>>, And<EPTimeCard.weekId, Greater<Current<EPTimeCard.weekId>>>>>.Select(this);
				if (futureTimeCard != null)
				{
					throw new PXSetPropertyException(Messages.TimeCardInFutureExists);
				}

			}
		}

        protected virtual void EPTimeCard_IsApproved_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            EPTimeCard row = e.Row as EPTimeCard;
            if (row == null) return;

            if ((bool?) e.NewValue == true)
            {
                //all non zero activities must be approved before timecard can be approved.
                PXView detailsView = new PXView(this, false, Activities.View.BqlSelect);
                foreach (PXResult<EPTimecardDetail, EPEmployee> res in detailsView.SelectMultiBound(new object[] {row}))
                {
                    EPTimecardDetail act = (EPTimecardDetail) res;

                    if (act.Released == true)
                        continue;

					if ((act.TimeSpent.GetValueOrDefault() != 0 || act.TimeBillable.GetValueOrDefault() != 0) && act.ApprovalStatus != ActivityStatusListAttribute.Completed && act.UIStatus != ActivityStatusListAttribute.Canceled)
                    {
                        e.Cancel = true;

                        Activities.Cache.RaiseExceptionHandling<EPTimecardDetail.timeSpent>(act, act.TimeSpent,
                                                                                            new PXSetPropertyException(
                                                                                                Messages
                                                                                                    .ActivityIsNotApproved,
                                                                                                PXErrorLevel.RowError));

                        throw new PXException(Messages.OneOrMoreActivitiesAreNotApproved);
                    }
                }
            }
        }

        protected virtual void EPTimeCardSummary_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            EPTimeCardSummary row = e.Row as EPTimeCardSummary;
            if (row == null) return;

            bool isEnabled = true;
            
            if (sender.GetStatus(row) != PXEntryStatus.Inserted && row.ProjectID != null)
            {
                List<EPTimecardDetail> selectManualDetails = GetDetails(row, Document.Current, true);
                if (selectManualDetails.Count > 0)
                {
                    isEnabled = false;
                }
            }

            PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.earningType>(sender, row, isEnabled);
            PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.projectID>(sender, row, isEnabled);
            PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.projectTaskID>(sender, row, isEnabled);
            PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.isBillable>(sender, row, isEnabled && !ProjectDefaultAttribute.IsNonProject(this, row.ProjectID));

	        PXResultset<EPTimecardDetailOrig> approv = PXSelect<
				EPTimecardDetailOrig
				, Where<
					EPTimecardDetailOrig.approverID, IsNotNull
					, And2<Where<EPTimecardDetailOrig.projectID, Equal<Current<EPTimeCardSummary.projectID>>, Or<EPTimecardDetailOrig.projectID, IsNull, And<Current<EPTimeCardSummary.projectID>, IsNull>>>
						, And2<Where<EPTimecardDetailOrig.projectID, Equal<Current<EPTimeCardSummary.projectID>>, Or<EPTimecardDetailOrig.projectID, IsNull, And<Current<EPTimeCardSummary.projectID>, IsNull>>>
							, And2<Where<EPTimecardDetailOrig.parentTaskID, Equal<Current<EPTimeCardSummary.parentTaskID>>, Or<EPTimecardDetailOrig.parentTaskID, IsNull, And<Current<EPTimeCardSummary.parentTaskID>, IsNull>>>
								, And<EPTimecardDetailOrig.earningTypeID, Equal<Current<EPTimeCardSummary.earningType>>
									, And<EPTimecardDetailOrig.weekID, Equal<Current<EPTimeCard.weekId>>
										, And<EPTimecardDetailOrig.released, NotEqual<True>
											, And<EPTimecardDetailOrig.uistatus, NotEqual<ActivityStatusListAttribute.canceled>
												>
											>
										>
									>
								>
							>
						>
					>
				>.Select(this, new object[]{row, Document.Current});

	        if (approv.Count != 0)
	        {
				bool hasOpen = approv.Any(_ => ((EPTimecardDetailOrig)_).UIStatus == ActivityStatusListAttribute.Open);
				bool hasApproved = approv.Any(_ => ((EPTimecardDetailOrig)_).UIStatus == ActivityStatusListAttribute.Approved);
				bool hasRejected = approv.Any(_ => ((EPTimecardDetailOrig)_).UIStatus == ActivityStatusListAttribute.Rejected);
				bool hasCompleted = approv.Any(_ => ((EPTimecardDetailOrig)_).UIStatus == ActivityStatusListAttribute.Completed);
				if (hasCompleted || hasOpen)
					row.ApprovalStatus = ApprovalStatusListAttribute.PendingApproval;
				else if (hasApproved && hasRejected)
					row.ApprovalStatus = ApprovalStatusListAttribute.PartiallyApprove;
				else if (hasApproved)
					row.ApprovalStatus = ApprovalStatusListAttribute.Approved;
				else if (hasRejected)
					row.ApprovalStatus = ApprovalStatusListAttribute.Rejected;
				else
					row.ApprovalStatus = ApprovalStatusListAttribute.NotRequired;
			}
			else
				row.ApprovalStatus = ApprovalStatusListAttribute.NotRequired;

        }

        protected virtual void EPTimeCardSummary_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            EPTimeCardSummary row = e.Row as EPTimeCardSummary;
            if (row == null) return;

            EPEarningType earningType = PXSelect<EPEarningType, Where<EPEarningType.typeCD, Equal<Required<EPEarningType.typeCD>>>>.Select(this, row.EarningType);
            if (earningType != null)
            {
                row.EarningType = earningType.TypeCD;
                if (row.IsBillable == null)
                    row.IsBillable = earningType.isBillable == true;

                if (earningType.ProjectID != null && row.ProjectID == null)
                {
                    row.ProjectID = earningType.ProjectID;
                }

                if (earningType.TaskID != null && row.ProjectTaskID == null )
                {
                    row.ProjectTaskID = earningType.TaskID;
                }
            }
            
            EPTimeCardSummary previousRecord = PXSelect<EPTimeCardSummary, 
                Where<EPTimeCardSummary.lineNbr, NotEqual<Current<EPTimeCardSummary.lineNbr>>,
                And<EPTimeCardSummary.timeCardCD, Equal<Current<EPTimeCardSummary.timeCardCD>>>>, OrderBy<Desc<EPTimeCardSummary.lineNbr>>>.SelectSingleBound(this, new object[] { row });
            if (previousRecord != null && row.ProjectID == null && !string.Equals(previousRecord.EarningType, row.EarningType, StringComparison.InvariantCultureIgnoreCase) && previousRecord.ProjectID != null)
            {
                row.ProjectID = previousRecord.ProjectID;
                row.ProjectTaskID = previousRecord.ProjectTaskID;
            }
            


        }
        
        protected virtual void EPTimeCardSummary_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            Debug.Print("EPTimeCardSummary_RowInserted Start");
            Debug.Indent();
            try
            {
                EPTimeCardSummary row = e.Row as EPTimeCardSummary;
                if (row == null) return;
                if (dontSyncDetails) return;

                UpdateAdjustingActivities(row);
            }
            finally
            {
                Debug.Unindent();
                Debug.Print("EPTimeCardSummary_RowInserted End");
            }
        }

        protected virtual void EPTimeCardSummary_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            Debug.Print("EPTimeCardSummary_RowUpdated Start");
            Debug.Indent();
            try
            {
                EPTimeCardSummary row = e.Row as EPTimeCardSummary;
                EPTimeCardSummary oldRow = e.OldRow as EPTimeCardSummary;

                if (row == null) return;

                Debug.Print("Old: {0}", oldRow);
                Debug.Print("New: {0}", row);

                if (dontSyncDetails) return;

                if (oldRow.ProjectID != row.ProjectID || oldRow.ProjectTaskID != row.ProjectTaskID ||
                    oldRow.IsBillable != row.IsBillable || oldRow.ParentTaskID != row.ParentTaskID || oldRow.EarningType != row.EarningType)
                {
                    if (ProjectDefaultAttribute.IsNonProject(this, row.ProjectID))
                    {
                        row.IsBillable = false;
                    }

                    //It is assumed that this fields can be changed only for the auto activities. If atleast one manual activity is present these fields are not editable.
                    //hence here we will just delete the existing auto details and recreate new one:
                    
                    List<EPTimecardDetail> list = GetDetails(oldRow, Document.Current, false);

                    try
                    {
                        dontSyncSummary = true;
                        foreach (EPTimecardDetail item in list)
                        {
                            if ( item.SummaryLineNbr == row.LineNbr )
                                Activities.Delete(item);
                        }
                    }
                    finally
                    {
                        dontSyncSummary = false;
                    }
                }
               
                UpdateAdjustingActivities(row);
            }
            finally
            {
                Debug.Unindent();
                Debug.Print("EPTimeCardSummary_RowUpdated End");
            }
        }

        protected virtual void EPTimeCardSummary_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            Debug.Print("EPTimeCardSummary_RowDeleting Start");
            Debug.Indent();
            try
            {
                EPTimeCardSummary row = e.Row as EPTimeCardSummary;
                if (row == null) return;

                if (dontSyncDetails)
                    return;

                if (row.Mon == null && row.Tue == null && row.Wed == null && row.Thu == null && row.Fri == null &&
                    row.Sat == null && row.Sun == null)
                    return; //Grid on pressing <Enter> at the last field automatically insert new row then delets it. Allow to delete such a row without checking. 

                if (GetDetails(row, Document.Current, true).Count > 0)//exists a detail record
                {
                    if (FindDuplicate(row) == null)//this is the last row that the detail record is referencing
                    {
                        e.Cancel = true;
                        throw new PXException("Summary record cannot be deleted.");
                    }
                }

            }
            finally
            {
                Debug.Unindent();
                Debug.Print("EPTimeCardSummary_RowDeleting End");
            }
        }
        
        protected virtual void EPTimeCardSummary_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            Debug.Print("EPTimeCardSummary_RowDeleted Start");
            Debug.Indent();
            try
            {
                EPTimeCardSummary row = e.Row as EPTimeCardSummary;
                if (row == null) return;
                if (dontSyncDetails) return;

                UpdateAdjustingActivities(row);
            }
            finally
            {
                Debug.Unindent();
                Debug.Print("EPTimeCardSummary_RowDeleted End");
            }
        }

        protected virtual void EPTimeCardSummary_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            EPTimeCardSummary row = e.Row as EPTimeCardSummary;
            if (row == null) return;
            
            PMTask currentTask = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<PMTask.taskID>>>>.Select(this, row.ProjectTaskID);
            if (currentTask != null)
            {
                PMTask newTask = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskCD, Equal<Required<PMTask.taskCD>>>>>.Select(this, row.ProjectID, currentTask.TaskCD);
                if (newTask != null)
                {
                    row.ProjectTaskID = newTask.TaskID;
                }
                else
                {
                    row.ParentTaskID = null;
                }
                
            }

        }

        
        protected virtual void EPTimecardDetail_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			EPTimecardDetail row = e.Row as EPTimecardDetail;
			if (row != null)
			{
                using (new PXConnectionScope())
                {
                    InitEarningType(row);
                }

			    RecalculateFields(row);

                if (row.StartDate != null)
                    row.Day = ((int)row.StartDate.Value.DayOfWeek).ToString();
			}
		}
        
        protected virtual void EPTimecardDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPTimecardDetail row = e.Row as EPTimecardDetail;
			if (row != null)
			{
                if (row.Released == true)
                    PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
                else
                {
                    PXUIFieldAttribute.SetEnabled<EPTimecardDetail.billableTimeCalc>(sender, e.Row, row.IsOvertimeCalc != true && row.IsBillable == true);
                    PXUIFieldAttribute.SetEnabled<EPTimecardDetail.billableOvertimeCalc>(sender, e.Row, row.IsOvertimeCalc == true && row.IsBillable == true);

                    EPEarningType EarningTypes = (EPEarningType)PXSelectorAttribute.Select<EPTimecardDetail.earningTypeID>(sender, row);
                    PXUIFieldAttribute.SetEnabled<EPTimecardDetail.isBillable>(sender, row, !ProjectDefaultAttribute.IsNonProject(this, row.ProjectID));
                }
			}
		}

		protected virtual void EPTimecardDetail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
            EPTimecardDetail row = e.Row as EPTimecardDetail;
			if (row == null) return;

			EPTimeCard doc = Document.Current;
			
			if (doc == null || doc.WeekID == null || doc.EmployeeID == null)
			{
				e.Cancel = true;
				return;
			}

            
            EPEarningType earningType = InitEarningType(row);
			if (earningType != null)
			{
				row.EarningTypeID = earningType.TypeCD;
			}

            if (ProjectDefaultAttribute.IsNonProject(this, row.ProjectID))
            {
                row.IsBillable = false;
            }
            else
            {
				if (earningType != null)
					row.IsBillable = earningType.isBillable == true;
            }

            RecalculateFields(row);
			
			PXSelectReadonly<EPEmployee,
					Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.Clear(this);

            EPEmployee employee = PXSelectReadonly<EPEmployee,
					Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.
					Select(this, doc.EmployeeID);
			row.Owner = employee.With(_ => _.UserID);

			row.Billed = false;
		    row.TrackTime = true;

            if (earningType != null && earningType.ProjectID != null && row.ProjectID == null)
            {
                row.ProjectID = earningType.ProjectID;
            }

            if (earningType != null && earningType.TaskID != null && row.ProjectTaskID == null)
            {
                row.ProjectTaskID = earningType.TaskID;
            }

		    if (row.IsBillable == true)
		        row.TimeBillable = row.TimeSpent;

			if (row.ClassID == null) row.ClassID = CRActivityClass.Activity;

			if (row.Type == null) row.Type = EpSetup.Current.DefaultActivityType;

			row.UIStatus = ActivityStatusListAttribute.Completed;

			if (row.StartDate == null)
			{
				row.StartDate = GetNextActivityStartDate(row, (int)doc.WeekID, (int)doc.EmployeeID);
			}
			row.EndDate = row.StartDate + TimeSpan.FromMinutes(row.TimeSpent ?? 0);
			if (row.UIStatus == ActivityStatusAttribute.Completed)
				row.CompletedDateTime = row.EndDate;

			row.WeekID = doc.WeekID;

		    row.Day = ((int) row.StartDate.Value.DayOfWeek).ToString();

            //autodefault to non-project if project module is off:
            if (row.ProjectID == null && !ProjectAttribute.IsPMVisible(this, BatchModule.EP))
                row.ProjectID = ProjectDefaultAttribute.NonProject(this);
		}

		protected virtual void EPTimecardDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
            Debug.Print("EPTimecardDetail_RowInserted Start");
            Debug.Indent();
		    try
		    {
                EPTimecardDetail row = e.Row as EPTimecardDetail;
                if (row == null) return;
                if (dontSyncSummary) return;

                EPTimeCardSummary summary = GetSummaryRecord(row);
                AddToSummary(summary, row);
		    }
		    finally
		    {
                Debug.Unindent();
                Debug.Print("EPTimecardDetail_RowInserted End");
		    }

		   
		}

		protected virtual void EPTimecardDetail_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
            EPTimecardDetail row = e.NewRow as EPTimecardDetail;
            EPTimecardDetail oldRow = e.Row as EPTimecardDetail;
			if (row == null || row.Billed == true) return;

			EPTimeCard doc = Document.Current;
			if (doc == null || doc.WeekID == null)
			{
				e.Cancel = true;
				return;
			}

            EPEarningType earningType = InitEarningType(row);
            row.EarningTypeID = earningType.TypeCD;

			if (row.EarningTypeID != oldRow.EarningTypeID || row.ProjectID != oldRow.ProjectID)
		    {
                if (ProjectDefaultAttribute.IsNonProject(this, row.ProjectID))
                {
                    row.IsBillable = false;
                }
                else
                {
					row.IsBillable = earningType.isBillable == true;
                }
		    }
		    if ( row.ProjectID == null && row.TimeSpent.GetValueOrDefault() != 0)
                sender.RaiseExceptionHandling<EPTimecardDetail.projectID>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(EPTimecardDetail.projectID).Name));

			if (row.StartDate == null || PXWeekSelector2Attribute.GetWeekID(this, (DateTime)row.StartDate) != doc.WeekID)
			{
				row.StartDate = PXWeekSelector2Attribute.GetWeekSrartDate(this, (int)doc.WeekID);
			}
			row.EndDate = row.StartDate + TimeSpan.FromMinutes(row.TimeSpent ?? 0);
			if (row.UIStatus == ActivityStatusAttribute.Completed)
				row.CompletedDateTime = row.EndDate;

			row.WeekID = doc.WeekID;
		}

		protected virtual void EPTimecardDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
            Debug.Print("EPTimecardDetail_RowUpdated Start");
            Debug.Indent();
		    try
		    {
			    EPTimecardDetail row = e.Row as EPTimecardDetail;
			    EPTimecardDetail oldRow = e.OldRow as EPTimecardDetail;
                if (row == null) return;

                if (row.SummaryLineNbr != null)
                {
                    EPTimeCardSummary summary = GetSummaryRecord(row);
                    if (summary != null && (summary.IsBillable.GetValueOrDefault() != row.IsBillable.GetValueOrDefault() ||
                        summary.EarningType != row.EarningTypeID ||
                        summary.ProjectID != row.ProjectID ||
                        summary.ProjectTaskID != row.ProjectTaskID ||
                        summary.ParentTaskID != row.ParentTaskID))
                    {
                        row.SummaryLineNbr = null;//summary and line are no longer linked with each other.
                    }
                }

		        if (row.EarningTypeID != oldRow.EarningTypeID)
		            InitEarningType(row);

                if (row.EarningTypeID != oldRow.EarningTypeID || row.TimeSpent.GetValueOrDefault() != oldRow.TimeSpent.GetValueOrDefault() ||
                    row.IsBillable.GetValueOrDefault() != oldRow.IsBillable.GetValueOrDefault())
		        {
		            RecalculateFields(row);
		        }
                
                if (row.StartDate.GetValueOrDefault() != oldRow.StartDate.GetValueOrDefault() || row.EndDate.GetValueOrDefault() != oldRow.EndDate.GetValueOrDefault() || row.EarningTypeID != oldRow.EarningTypeID || row.ProjectID.GetValueOrDefault() != oldRow.ProjectID.GetValueOrDefault() ||
                    row.ProjectTaskID.GetValueOrDefault() != oldRow.ProjectTaskID.GetValueOrDefault() || row.TimeSpent.GetValueOrDefault() != oldRow.TimeSpent.GetValueOrDefault() || row.IsBillable.GetValueOrDefault() != oldRow.IsBillable.GetValueOrDefault() || row.TimeBillable.GetValueOrDefault() != oldRow.TimeBillable.GetValueOrDefault())
		        {
					if ((row.ApprovalStatus == ActivityStatusListAttribute.Completed || row.ApprovalStatus == ActivityStatusListAttribute.Rejected) && row.ApproverID != null)
		            {
						row.UIStatus = ActivityStatusListAttribute.Completed;
		            }
		        }

		        if (!dontSyncSummary)
		        {
		            EPTimeCardSummary summaryOld = GetSummaryRecord(oldRow);
                    SubtractFromSummary(summaryOld, oldRow);

                    EPTimeCardSummary summary = GetSummaryRecord(row);
		            AddToSummary(summary, row);
                   
		        }
		    }
            finally
            {
                Debug.Unindent();
                Debug.Print("EPTimecardDetail_RowUpdated End");
            }

		}

        protected virtual void EPTimecardDetail_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            Debug.Print("EPTimecardDetail_RowDeleting Start");
            Debug.Indent();
            try
            {
                EPTimecardDetail row = e.Row as EPTimecardDetail;

                if (row != null)
                {
	                if (row.Released == true)
	                {
						throw new PXException(Messages.ActivityIsReleased);
					}
	                if (Document.Current != null && !string.IsNullOrEmpty(Document.Current.OrigTimeCardCD) && ((EPTimecardDetail)e.Row).OrigTaskID != null
                        && Document.Cache.GetStatus(Document.Current) != PXEntryStatus.Deleted)
                    {
                        throw new PXException(Messages.CannotDeleteCorrectionActivity);
                    }

                    if (row.SummaryLineNbr == null)
                    {
                        if (!dontSyncSummary)
                        {
                            EPTimeCardSummary summary = GetSummaryRecord(row);
                            SubtractFromSummary(summary, row);
                        }
                    }
                    
                }
            }
            finally
            {
                Debug.Unindent();
                Debug.Print("EPTimecardDetail_RowDeleting End");
            }
        }

        protected virtual void EPTimecardDetail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            Debug.Print("EPTimecardDetail_RowDeleted Start");
            Debug.Indent();
            try
            {
                EPTimecardDetail row = e.Row as EPTimecardDetail;

                if (row != null && row.SummaryLineNbr != null)//adjust activity is deleted
                {
                    if (!dontSyncSummary)
                    {
                        EPTimeCardSummary summary = GetSummaryRecord(row);
                        SubtractFromSummary(summary, row);
                    }
                }
            }
            finally
            {
                Debug.Unindent();
                Debug.Print("EPTimecardDetail_RowDeleted End");
            }
        }
        
		protected virtual void EPTimecardDetail_StartDate_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
            EPTimecardDetail row = e.Row as EPTimecardDetail;
			if (row == null) return;

			EPTimeCard doc = Document.Current;
			if (row.StartDate == null && doc != null && doc.WeekID != null && doc.EmployeeID != null)
			{
				row.StartDate = GetNextActivityStartDate(row, (int)doc.WeekID, (int)doc.EmployeeID);
			}
		}
        
        protected virtual void EPTimecardDetail_Day_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if (Document.Current == null) return;
            PXWeekSelector2Attribute.WeekInfo weekInfo = PXWeekSelector2Attribute.GetWeekInfo(this, Document.Current.WeekID.Value);

            List<string> allowedValues = new List<string>();
            List<string> allowedLabels = new List<string>();

            if (weekInfo.Mon.Enabled)
            {
                allowedValues.Add("1");
                allowedLabels.Add("Monday");
            }

            if (weekInfo.Tue.Enabled)
            {
                allowedValues.Add("2");
                allowedLabels.Add("Tuesday");
            }

            if (weekInfo.Wed.Enabled)
            {
                allowedValues.Add("3");
                allowedLabels.Add("Wednesday");
            }

            if (weekInfo.Thu.Enabled)
            {
                allowedValues.Add("4");
                allowedLabels.Add("Thursday");
            }

            if (weekInfo.Fri.Enabled)
            {
                allowedValues.Add("5");
                allowedLabels.Add("Friday");
            }

            if (weekInfo.Sat.Enabled)
            {
                allowedValues.Add("6");
                allowedLabels.Add("Saturday");
            }

            if (weekInfo.Sun.Enabled)
            {
                allowedValues.Add("0");
                allowedLabels.Add("Sunday");
            }

            e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 1, false, typeof(EPTimecardDetail.day).Name, false, 1, null,
                                                    allowedValues.ToArray(), allowedLabels.ToArray(), true, allowedValues[0]);
        } 

        protected virtual void EPTimecardDetail_Day_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            EPTimecardDetail row = e.Row as EPTimecardDetail;
            if (row == null) return;

            if (Document.Current != null)
            {
                PXWeekSelector2Attribute.WeekInfo weekInfo = PXWeekSelector2Attribute.GetWeekInfo(this, Document.Current.WeekID.Value);

                DayOfWeek val = (DayOfWeek) Enum.Parse(typeof (DayOfWeek), row.Day);

                switch (val)
                {
                    case DayOfWeek.Monday: sender.SetValueExt<EPTimecardDetail.startDate>(row, weekInfo.Mon.Date); break;
                    case DayOfWeek.Tuesday: sender.SetValueExt<EPTimecardDetail.startDate>(row, weekInfo.Tue.Date); break;
                    case DayOfWeek.Wednesday: sender.SetValueExt<EPTimecardDetail.startDate>(row, weekInfo.Wed.Date); break;
                    case DayOfWeek.Thursday: sender.SetValueExt<EPTimecardDetail.startDate>(row, weekInfo.Thu.Date); break;
                    case DayOfWeek.Friday: sender.SetValueExt<EPTimecardDetail.startDate>(row, weekInfo.Fri.Date); break;
                    case DayOfWeek.Saturday: sender.SetValueExt<EPTimecardDetail.startDate>(row, weekInfo.Sat.Date); break;
                    case DayOfWeek.Sunday: sender.SetValueExt<EPTimecardDetail.startDate>(row, weekInfo.Sun.Date); break;

                }
            }
        }

		protected virtual void EPTimecardDetail_StartDate_Date_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
            EPTimecardDetail row = e.Row as EPTimecardDetail;
			if (row == null) return;

			int? weekId = row.WeekID ?? Document.Current.With(_ => _.WeekID);
			if (weekId == null) return;

			DateTime? newValue = null;
			DateTime valFromString;
			if (e.NewValue is string &&
				DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, System.Globalization.DateTimeStyles.None, out valFromString))
			{
				newValue = valFromString;
			}
			if (e.NewValue is DateTime)
				newValue = (DateTime)e.NewValue;

            PXWeekSelector2Attribute.WeekInfo weekInfo = PXWeekSelector2Attribute.GetWeekInfo(this, Document.Current.WeekID.Value);
            if (newValue != null && !weekInfo.IsValid(((DateTime)newValue).Date))
			{
				throw new PXSetPropertyException(Messages.DateNotInWeek);
			}
		}

		protected virtual void EPTimecardDetail_Owner_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
            EPTimecardDetail row = e.Row as EPTimecardDetail;
			if (row == null) return;

			if (row.Owner == null)
			{
                EPEmployee employee = Document.Current.
					With(_ => PXSelectReadonly<EPEmployee,
					Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.
					Select(this, _.EmployeeID));
				row.Owner = employee.With(_ => _.UserID);
			}
		}

		protected virtual void EPTimecardDetail_StartDate_Date_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			var row = e.Row as EPTimecardDetail;
			if (row == null) return;

			sender.SetValuePending(e.Row, typeof(EPTimecardDetail.startDate).Name + "_oldValue", row.StartDate);
		}

		protected virtual void EPTimecardDetail_StartDate_Time_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			var row = e.Row as EPTimecardDetail;
			if (row == null) return;

			DateTime? newValue = null;
			DateTime valFromString;
			if (e.NewValue is string &&
				DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, System.Globalization.DateTimeStyles.None, out valFromString))
			{
				newValue = valFromString;
			}
			if (e.NewValue is DateTime)
				newValue = (DateTime)e.NewValue;

			var oldValue = (DateTime?)sender.GetValuePending(e.Row, typeof(EPTimecardDetail.startDate).Name + "_oldValue");
			if (oldValue != null && newValue != null &&
				(int)oldValue.Value.TimeOfDay.TotalMinutes == (int)newValue.Value.TimeOfDay.TotalMinutes)
			{
				e.Cancel = true;
			}
		}

		protected virtual void EPTimecardDetail_StartDate_Date_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as EPTimecardDetail;
			if (row == null) return;

			var oldValue = (DateTime?)sender.GetValuePending(e.Row, typeof(EPTimecardDetail.startDate).Name + "_oldValue");
			var newValue = row.StartDate;
			if (newValue != null &&
				(oldValue == null ||
					oldValue != newValue && ((int)oldValue.Value.TimeOfDay.TotalMinutes == (int)newValue.Value.TimeOfDay.TotalMinutes)))
			{
				var calendarId = CRActivityMaint.GetCalendarID(this, row);
				if (!string.IsNullOrEmpty(calendarId))
				{
					DateTime start;
					DateTime end;
					CalendarHelper.CalculateStartEndTime(this, calendarId, newValue.Value.Date, out start, out end);
					row.StartDate = start;
				}
			}

            row.Day = ((int)row.StartDate.Value.DayOfWeek).ToString();
		}

        protected virtual void EPTimecardDetail_BillableTimeCalc_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            EPTimecardDetail row = e.Row as EPTimecardDetail;
            if (row == null) return;
            int? newBillableTime = (int?) e.NewValue;

            if (row.TimeSpent < newBillableTime)
            {
                throw new PXSetPropertyException(CR.Messages.BillableTimeCannotBeGreaterThanTimeSpent);
            }
        }
        
        protected virtual void EPTimecardDetail_BillableTimeCalc_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            EPTimecardDetail row = e.Row as EPTimecardDetail;
            if (row == null) return;
            row.TimeBillable = row.BillableTimeCalc;

        }

        protected virtual void EPTimecardDetail_BillableOvertimeCalc_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            EPTimecardDetail row = e.Row as EPTimecardDetail;
            if (row == null) return;
            int? newBillableTime = (int?)e.NewValue;

            if (row.TimeSpent < newBillableTime)
            {
                throw new PXSetPropertyException(CR.Messages.OvertimeBillableCannotBeGreaterThanOvertimeSpent);
            }
        }

        protected virtual void EPTimecardDetail_BillableOvertimeCalc_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            EPTimecardDetail row = e.Row as EPTimecardDetail;
            if (row == null) return;
            row.TimeBillable = row.BillableOvertimeCalc;
            
        }

        protected virtual void EPTimecardDetail_IsBillable_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            EPTimecardDetail row = e.Row as EPTimecardDetail;
            if (row == null) return;
            
            if (row.IsBillable == true && row.TimeBillable.GetValueOrDefault() == 0)
            {
                row.TimeBillable = row.TimeSpent;
            }
        }

        protected virtual void EPTimeCardItem_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            sender.SetDefaultExt<EPTimeCardItem.uOM>(e.Row);
        }

        protected virtual void EPApproval_Details_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (this.Document.Current == null) return;

            e.NewValue = null;
            Type[] fields = new Type[]
			{
				typeof(EPTimeCard.weekDescription), typeof(EPTimeCard.timeSpentCalc), typeof(EPTimeCard.overtimeSpentCalc),typeof(EPTimeCard.timeBillableCalc), typeof(EPTimeCard.overtimeBillableCalc)
			};
            foreach (Type t in fields)
            {
                PXStringState strState = this.Document.Cache.GetValueExt(this.Document.Current, t.Name) as PXStringState;
                if (strState != null)
                {
                    string value =
                        strState.InputMask != null ? Mask.Format(strState.InputMask, strState) :
                        strState.Value != null ? strState.Value.ToString() : null;

                    if (!string.IsNullOrEmpty(value))
                        e.NewValue += (e.NewValue != null ? ", " : string.Empty) + strState.DisplayName + "=" + value.Trim();
                }
            }
        }
        
		#endregion
        
		#region Protected Methods

        protected virtual void ProcessRegularTimecard(PM.RegisterEntry releaseGraph, EPTimeCard timecard)
        {
            PXCache registerCache = releaseGraph.Document.Cache;
            PXCache tranCache = releaseGraph.Transactions.Cache;
            PM.PMRegister doc = (PM.PMRegister)registerCache.Insert();
            doc.OrigDocType = PMOrigDocType.Timecard;
            doc.OrigDocNbr = timecard.TimeCardCD;
            releaseGraph.FieldVerifying.AddHandler<PMTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });//restriction should be applicable only for budgeting.
            
            EP.EPEmployee emp = PXSelect<EP.EPEmployee, Where<EP.EPEmployee.bAccountID, Equal<Required<EP.EPEmployee.bAccountID>>>>.Select(this, timecard.EmployeeID);
            if (emp != null)
            {
                doc.Description = string.Format("{0} - {1}", emp.AcctName, timecard.WeekID);
            }

            foreach (EPTimecardDetail act in Activities.Select())
            {
                if (act.Released == true)//activities can be released throught employee activity release screen (if TC is not required for an employee)
                    continue;

				if (act.RefNoteID != null && PXSelect<CRCase, Where<CRCase.noteID, Equal<Required<CRCase.noteID>>>>.Select(this, act.RefNoteID).Count == 1)
					{
                    //Add Contract-Usage
                    PMTran contractUsageTran = releaseGraph.CreateContractUsage(act);
                    if ( contractUsageTran != null )
                        Activities.Cache.SetValue<EPTimecardDetail.billed>(act, true);
                }
                
				decimal? cost = CreateEmployeeCostEngine().CalculateEmployeeCost(act, timecard.EmployeeID, act.StartDate.Value);
                releaseGraph.CreateTransaction(act, timecard.EmployeeID, act.StartDate.Value, act.TimeSpent, act.TimeBillable, cost);
                Activities.Cache.SetValueExt<EPTimecardDetail.released>(act, true);
				Activities.Cache.SetValue<EPTimecardDetail.employeeRate>(act, IN.PXDBPriceCostAttribute.Round(this.Caches<EPTimecardDetail>(), (decimal)cost));
                Activities.Cache.SetStatus(act, PXEntryStatus.Updated);
            }


            PXWeekSelector2Attribute.WeekInfo week = PXWeekSelector2Attribute.GetWeekInfo(this, timecard.WeekID.Value);

            foreach (EPTimeCardItem item in Items.View.SelectMultiBound(new object[] { timecard }))
            {
                #region Create Transactions By Days

                if (item.Sun.GetValueOrDefault() > 0)
                {
                    CreateItemTransaction(item, timecard.EmployeeID, week.Sun.Date, item.Sun, tranCache);
                }

                if (item.Mon.GetValueOrDefault() > 0)
                {
                    CreateItemTransaction(item, timecard.EmployeeID, week.Mon.Date, item.Mon, tranCache);
                }

                if (item.Tue.GetValueOrDefault() > 0)
                {
                    CreateItemTransaction(item, timecard.EmployeeID, week.Tue.Date, item.Tue, tranCache);
                }

                if (item.Wed.GetValueOrDefault() > 0)
                {
                    CreateItemTransaction(item, timecard.EmployeeID, week.Wed.Date, item.Wed, tranCache);
                }

                if (item.Thu.GetValueOrDefault() > 0)
                {
                    CreateItemTransaction(item, timecard.EmployeeID, week.Thu.Date, item.Thu, tranCache);
                }

                if (item.Fri.GetValueOrDefault() > 0)
                {
                    CreateItemTransaction(item, timecard.EmployeeID, week.Fri.Date, item.Fri, tranCache);
                }

                if (item.Sat.GetValueOrDefault() > 0)
                {
                    CreateItemTransaction(item, timecard.EmployeeID, week.Sat.Date, item.Sat, tranCache);
                }

                #endregion
            }
        }

        protected virtual void ProcessCorrectingTimecard(PM.RegisterEntry releaseGraph, EPTimeCard timecard)
        {
            PXCache registerCache = releaseGraph.Document.Cache;
            PXCache tranCache = releaseGraph.Transactions.Cache;
            PM.PMRegister doc = (PM.PMRegister)registerCache.Insert();
            doc.OrigDocType = PMOrigDocType.Timecard;
            doc.OrigDocNbr = timecard.TimeCardCD;
            releaseGraph.FieldVerifying.AddHandler<PMTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });//restriction should be applicable only for budgeting.

            EP.EPEmployee emp = PXSelect<EP.EPEmployee, Where<EP.EPEmployee.bAccountID, Equal<Required<EP.EPEmployee.bAccountID>>>>.Select(this, timecard.EmployeeID);
            if (emp != null)
            {
                doc.Description = string.Format("{0} - {1} correction", emp.AcctName, timecard.WeekID);
            }

            //process deleted activities:
            PXSelectBase<EPTimecardDetailOrig> selectDeletedActivities = new PXSelectJoin<EPTimecardDetailOrig,
                    InnerJoin<EPEmployee, On<EPEmployee.userID, Equal<EPTimecardDetailOrig.owner>>,
                    LeftJoin<EPTimecardDetailEx, On<EPTimecardDetailOrig.taskID, Equal<EPTimecardDetailEx.origTaskID>>>>,
                    Where<EPEmployee.bAccountID, Equal<Required<EPTimeCard.employeeID>>,
                        And<EPTimecardDetailOrig.weekID, Equal<Required<EPTimeCard.weekId>>,
                        And<EPTimecardDetailOrig.classID, NotEqual<CRActivityClass.emailRouting>,
                        And<EPTimecardDetailOrig.classID, NotEqual<CRActivityClass.task>,
		                And<EPTimecardDetailOrig.classID, NotEqual<CRActivityClass.events>,
                        And<EPTimecardDetailOrig.isIncome, NotEqual<True>,
                        And<EPTimecardDetailOrig.timeSheetCD, IsNull,
                        And<EPTimecardDetailEx.taskID, IsNull,
                        And<Where<EPTimecardDetailOrig.timeCardCD, IsNull, Or<EPTimecardDetailOrig.timeCardCD, Equal<Required<EPTimeCard.timeCardCD>>>>>>>>>>>>>,
                    OrderBy<Asc<EPTimecardDetailOrig.startDate>>>(this);

            foreach (PXResult<EPTimecardDetailOrig, EPEmployee, EPTimecardDetailEx> res in selectDeletedActivities.Select(timecard.EmployeeID, timecard.WeekID, timecard.OrigTimeCardCD))
            {
                EPTimecardDetailOrig orig = (EPTimecardDetailOrig)res;

				decimal? origCost = orig.EmployeeRate ?? CreateEmployeeCostEngine().CalculateEmployeeCost(orig, timecard.EmployeeID, orig.StartDate.Value);

                releaseGraph.CreateTransaction(orig, timecard.EmployeeID, orig.StartDate.Value, -orig.TimeSpent, -orig.TimeBillable, origCost);
                Activities.Cache.SetValueExt<EPTimecardDetail.released>(orig, true);
				Activities.Cache.SetValue<EPTimecardDetail.employeeRate>(orig, IN.PXDBPriceCostAttribute.Round(this.Caches<EPTimecardDetail>(), (decimal)origCost));
                Activities.Cache.SetStatus(orig, PXEntryStatus.Updated);
            }

            //process new and modified activities:
            PXSelectBase<EPTimecardDetail> selectActivities = new PXSelectJoin<EPTimecardDetail,
                    InnerJoin<EPEmployee, On<EPEmployee.userID, Equal<EPTimecardDetailOrig.owner>>,
                    LeftJoin<EPTimecardDetailOrig, On<EPTimecardDetailOrig.taskID, Equal<EPTimecardDetail.origTaskID>>>>,
                    Where<EPEmployee.bAccountID, Equal<Required<EPTimeCard.employeeID>>,
                        And<EPTimecardDetail.weekID, Equal<Required<EPTimeCard.weekId>>,
                        And<EPTimecardDetail.classID, NotEqual<CRActivityClass.emailRouting>,
                        And<EPTimecardDetail.classID, NotEqual<CRActivityClass.task>,
		                And<EPTimecardDetail.classID, NotEqual<CRActivityClass.events>,
                        And<EPTimecardDetail.isIncome, NotEqual<True>,
                        And<EPTimecardDetail.timeSheetCD, IsNull,
                        And<Where<EPTimecardDetail.timeCardCD, IsNull, Or<EPTimecardDetail.timeCardCD, Equal<Required<EPTimeCard.timeCardCD>>>>>>>>>>>>,
                    OrderBy<Asc<EPTimecardDetail.startDate>>>(this);

            foreach (PXResult<EPTimecardDetail, EPEmployee, EPTimecardDetailOrig> res in selectActivities.Select(timecard.EmployeeID, timecard.WeekID, timecard.TimeCardCD))
            {
                EPTimecardDetail act = (EPTimecardDetail)res;

                if (act.Released == true )//new added activities can be released throught employee activity release screen (if TC is not required for an employee)
                    continue;

                if (act.RefNoteID != null)
                {
                    CRCase crCase = PXSelect<CRCase, Where<CRCase.noteID, Equal<Required<CRCase.noteID>>>>.Select(this, act.RefNoteID);
                    if (crCase != null && crCase.ContractID != null)
                    {
                        //Add Contract-Usage
                        releaseGraph.CreateContractUsage(act);
                        Activities.Cache.SetValue<EPTimecardDetail.billed>(act, true);
                    }
                }

                EPTimecardDetailOrig orig = (EPTimecardDetailOrig)res;
                Activities.Cache.RaiseRowSelected(act);

                decimal? origCost = null;
				decimal? cost = CreateEmployeeCostEngine().CalculateEmployeeCost(act, timecard.EmployeeID, act.StartDate.Value);
                if (orig.TaskID != null)
					origCost = orig.EmployeeRate ?? CreateEmployeeCostEngine().CalculateEmployeeCost(orig, timecard.EmployeeID, orig.StartDate.Value);

                if (orig.TaskID == null || (act.ProjectID == orig.ProjectID && act.ProjectTaskID == orig.ProjectTaskID && origCost == cost && act.IsBillable == orig.IsBillable))
                {
                    int? time = act.TimeSpent;
                    if (orig.TaskID != null)
                    {
                        time = ((act.TimeSpent ?? 0) - (orig.TimeSpent ?? 0));
                    }

                    int? timeBillable = act.TimeBillable;
                    if (orig.TaskID != null)
                    {
                        timeBillable = ((act.TimeBillable ?? 0) - (orig.TimeBillable ?? 0));
                    }

                    releaseGraph.CreateTransaction(act, timecard.EmployeeID, act.StartDate.Value, time, timeBillable, cost);
                }
                else
                {
                    //delete previous:
                    releaseGraph.CreateTransaction(orig, timecard.EmployeeID, orig.StartDate.Value, -orig.TimeSpent, -orig.TimeBillable, origCost);
                   
                    //add new:
                    releaseGraph.CreateTransaction(act, timecard.EmployeeID, act.StartDate.Value, act.TimeSpent, act.TimeBillable, cost);
                }
				Activities.Cache.SetValue<EPTimecardDetail.employeeRate>(act, IN.PXDBPriceCostAttribute.Round(this.Caches<EPTimecardDetail>(), (decimal)cost));
                Activities.Cache.SetValueExt<EPTimecardDetail.released>(act, true);
				Activities.Cache.SetStatus(act, PXEntryStatus.Updated);
            }
            

            //Non-Stock Items:
            PXWeekSelector2Attribute.WeekInfo week = PXWeekSelector2Attribute.GetWeekInfo(this, timecard.WeekID.Value);
            //process deleted items
            PXSelectBase<EPTimeCardItemOrig> selectDeletedItems = new PXSelectJoin<EPTimeCardItemOrig,
                LeftJoin<EPTimeCardItemEx, On<EPTimeCardItemEx.timeCardCD, Equal<Current<EPTimeCard.timeCardCD>>,
                    And<EPTimeCardItemEx.origLineNbr, Equal<EPTimeCardItemOrig.lineNbr>>>>,
                Where<EPTimeCardItemOrig.timeCardCD, Equal<Current<EPTimeCard.origTimeCardCD>>,
                    And<EPTimeCardItemEx.timeCardCD, IsNull>>>(this);

            foreach (PXResult<EPTimeCardItemOrig, EPTimeCardItemEx> res in selectDeletedItems.View.SelectMultiBound(new object[] { timecard }))
            {
                EPTimeCardItemOrig orig = (EPTimeCardItemOrig)res;

                #region Create Transactions By Days

                if (orig.Sun.GetValueOrDefault() > 0)
                {
                    CreateItemTransaction(orig, timecard.EmployeeID, week.Sun.Date, -orig.Sun, tranCache);
                    
                }

                if (orig.Mon.GetValueOrDefault() > 0)
                {
                    CreateItemTransaction(orig, timecard.EmployeeID, week.Mon.Date, -orig.Mon, tranCache);
                }

                if (orig.Tue.GetValueOrDefault() > 0)
                {
                    CreateItemTransaction(orig, timecard.EmployeeID, week.Tue.Date, -orig.Tue, tranCache);
                }

                if (orig.Wed.GetValueOrDefault() > 0)
                {
                    CreateItemTransaction(orig, timecard.EmployeeID, week.Wed.Date, -orig.Wed, tranCache);
                }

                if (orig.Thu.GetValueOrDefault() > 0)
                {
                    CreateItemTransaction(orig, timecard.EmployeeID, week.Thu.Date, -orig.Thu, tranCache);
                }

                if (orig.Fri.GetValueOrDefault() > 0)
                {
                    CreateItemTransaction(orig, timecard.EmployeeID, week.Fri.Date, -orig.Fri, tranCache);
                }

                if (orig.Sat.GetValueOrDefault() > 0)
                {
                    CreateItemTransaction(orig, timecard.EmployeeID, week.Sat.Date, -orig.Sat, tranCache);
                }

                #endregion

            }

            //process added items
            PXSelectBase<EPTimeCardItem> selectAddedItems = new PXSelect<EPTimeCardItem,
                Where<EPTimeCardItem.timeCardCD, Equal<Current<EPTimeCard.timeCardCD>>,
                    And<EPTimeCardItem.origLineNbr, IsNull>>>(this);
            foreach (EPTimeCardItem item in selectAddedItems.View.SelectMultiBound(new object[] { timecard }))
            {
                #region Create Transactions By Days

                if (item.Sun.GetValueOrDefault() > 0)
                {
                    CreateItemTransaction(item, timecard.EmployeeID, week.Sun.Date, item.Sun, tranCache);
                }

                if (item.Mon.GetValueOrDefault() > 0)
                {
                    CreateItemTransaction(item, timecard.EmployeeID, week.Mon.Date, item.Mon, tranCache);
                }

                if (item.Tue.GetValueOrDefault() > 0)
                {
                    CreateItemTransaction(item, timecard.EmployeeID, week.Tue.Date, item.Tue, tranCache);
                }

                if (item.Wed.GetValueOrDefault() > 0)
                {
                    CreateItemTransaction(item, timecard.EmployeeID, week.Wed.Date, item.Wed, tranCache);
                }

                if (item.Thu.GetValueOrDefault() > 0)
                {
                    CreateItemTransaction(item, timecard.EmployeeID, week.Thu.Date, item.Thu, tranCache);
                }

                if (item.Fri.GetValueOrDefault() > 0)
                {
                    CreateItemTransaction(item, timecard.EmployeeID, week.Fri.Date, item.Fri, tranCache);
                }

                if (item.Sat.GetValueOrDefault() > 0)
                {
                    CreateItemTransaction(item, timecard.EmployeeID, week.Sat.Date, item.Sat, tranCache);
                }

                #endregion
            }

            //process modified records
            PXSelectBase<EPTimeCardItemOrig> selectModifiedItems = new PXSelectJoin<EPTimeCardItemOrig,
                LeftJoin<EPTimeCardItemEx, On<EPTimeCardItemEx.timeCardCD, Equal<Current<EPTimeCard.timeCardCD>>,
                    And<EPTimeCardItemEx.origLineNbr, Equal<EPTimeCardItemOrig.lineNbr>>>>,
                Where<EPTimeCardItemOrig.timeCardCD, Equal<Current<EPTimeCard.origTimeCardCD>>,
                    And<EPTimeCardItemEx.timeCardCD, IsNotNull>>>(this);
            foreach (PXResult<EPTimeCardItemOrig, EPTimeCardItemEx> res in selectModifiedItems.View.SelectMultiBound(new object[] { timecard }))
            {
                EPTimeCardItemOrig orig = (EPTimeCardItemOrig)res;
                EPTimeCardItemEx item = (EPTimeCardItemEx)res;

                #region Create Transactions By Days

                if (item.Sun.GetValueOrDefault() - orig.Sun.GetValueOrDefault() != 0)
                {
                    CreateItemTransaction(item, timecard.EmployeeID, week.Sun.Date, item.Sun.GetValueOrDefault() - orig.Sun.GetValueOrDefault(), tranCache);
                }

                if (item.Mon.GetValueOrDefault() - orig.Mon.GetValueOrDefault() != 0)
                {
                    CreateItemTransaction(item, timecard.EmployeeID, week.Mon.Date, item.Mon.GetValueOrDefault() - orig.Mon.GetValueOrDefault(), tranCache);
                }

                if (item.Tue.GetValueOrDefault() - orig.Tue.GetValueOrDefault() != 0)
                {
                    CreateItemTransaction(item, timecard.EmployeeID, week.Tue.Date, item.Tue.GetValueOrDefault() - orig.Tue.GetValueOrDefault(), tranCache);
                }

                if (item.Wed.GetValueOrDefault() - orig.Wed.GetValueOrDefault() != 0)
                {
                    CreateItemTransaction(item, timecard.EmployeeID, week.Wed.Date, item.Wed.GetValueOrDefault() - orig.Wed.GetValueOrDefault(), tranCache);
                }

                if (item.Thu.GetValueOrDefault() - orig.Thu.GetValueOrDefault() != 0)
                {
                    CreateItemTransaction(item, timecard.EmployeeID, week.Thu.Date, item.Thu.GetValueOrDefault() - orig.Thu.GetValueOrDefault(), tranCache);
                }

                if (item.Fri.GetValueOrDefault() - orig.Fri.GetValueOrDefault() != 0)
                {
                    CreateItemTransaction(item, timecard.EmployeeID, week.Fri.Date, item.Fri.GetValueOrDefault() - orig.Fri.GetValueOrDefault(), tranCache);
                }

                if (item.Sat.GetValueOrDefault() - orig.Sat.GetValueOrDefault() != 0)
                {
                    CreateItemTransaction(item, timecard.EmployeeID, week.Sat.Date, item.Sat.GetValueOrDefault() - orig.Sat.GetValueOrDefault(), tranCache);
                }

                #endregion


            }

        }
        
        protected virtual EPEarningType InitEarningType(EPTimecardDetail row)
        {
            if (row == null)
                throw new ArgumentNullException();

            EPEarningType earningType = PXSelect<EPEarningType, Where<EPEarningType.typeCD, Equal<Required<EPEarningType.typeCD>>>>.Select(this, row.EarningTypeID);

			if (earningType != null && row.EarningTypeID != null && Document.Current.EmployeeID != null && row.StartDate != null)
            {
                row.IsOvertimeCalc = earningType.IsOvertime;
				row.OvertimeMultiplierCalc = CreateEmployeeCostEngine().GetOvertimeMultiplier((string)row.EarningTypeID, (int)Document.Current.EmployeeID, (DateTime)row.StartDate);
            }

            return earningType;
        }

        protected virtual void RecalculateFields(EPTimecardDetail row)
        {
            if (row == null)
                throw new ArgumentNullException();

            row.BillableTimeCalc = null;
            row.BillableOvertimeCalc = null;
            row.RegularTimeCalc = null;
            row.OverTimeCalc = null;

            if (row.IsOvertimeCalc == true)
            {
                row.OverTimeCalc = row.TimeSpent;
                if (row.IsBillable == true)
                {
                    row.BillableOvertimeCalc = row.TimeBillable;
                }
            }
            else
            {
                row.RegularTimeCalc = row.TimeSpent;
                if (row.IsBillable == true)
                {
                    row.BillableTimeCalc = row.TimeBillable;
                }
            }
        }

        protected virtual EPTimeCardSummary GetSummaryRecord(EPTimecardDetail activity)
		{
            if (activity == null)
                throw new ArgumentNullException();

			if (activity.SummaryLineNbr != null)
			{
				return PXSelect<EPTimeCardSummary, Where<EPTimeCardSummary.timeCardCD, Equal<Current<EPTimeCard.timeCardCD>>, And<EPTimeCardSummary.lineNbr, Equal<Required<EPTimeCardSummary.lineNbr>>>>>.Select(this, activity.SummaryLineNbr);
			}

			PXSelectBase<EPTimeCardSummary> select = new PXSelect<EPTimeCardSummary, Where<EPTimeCardSummary.timeCardCD, Equal<Current<EPTimeCard.timeCardCD>>,
			And<EPTimeCardSummary.earningType, Equal<Current<EPTimecardDetail.earningTypeID>>,
            And<EPTimeCardSummary.projectID, Equal<Required<EPTimecardDetail.projectID>>,
            And<EPTimeCardSummary.isBillable, Equal<Current<EPTimecardDetail.isBillable>>>>>>>(this);

			if ( activity.ParentTaskID != null )
			{
                select.WhereAnd<Where<EPTimeCardSummary.parentTaskID, Equal<Current<EPTimecardDetail.parentTaskID>>>>();
			}

			if ( activity.ProjectTaskID != null)
			{
                select.WhereAnd<Where<EPTimeCardSummary.projectTaskID, Equal<Current<EPTimecardDetail.projectTaskID>>>>();
			}
			int projectID = activity.ProjectID.GetValueOrDefault(ProjectDefaultAttribute.NonProject(this).GetValueOrDefault(0));
			return (EPTimeCardSummary)select.View.SelectSingleBound(new object[] { Document.Current, activity }, projectID);
		}

        protected virtual EPTimeCardSummary AddToSummary(EPTimeCardSummary summary, EPActivity activity)
		{
			return AddToSummary(summary, activity, 1);
		}

        protected virtual void SubtractFromSummary(EPTimeCardSummary summary, EPActivity activity)
		{
            summary = AddToSummary(summary, activity, -1);

            try
            {
                dontSyncDetails = true;
                if (summary != null && summary.TimeSpent.Value == 0)
                {
                    Summary.Delete(summary);//cascadly will delete detail record through PXParent.
                }

            }
            finally
            {
                dontSyncDetails = false;
            }
			
		}

        /// <summary>
        /// When True detail row is not updated when a summary record is modified.
        /// </summary>
		protected bool dontSyncDetails = false;
        protected virtual EPTimeCardSummary AddToSummary(EPTimeCardSummary summary, EPActivity activity, int mult)
		{
            if (activity == null)
                throw new ArgumentNullException();

            Debug.Print("AddToSummary Mult:{0} Start", mult);
            Debug.Indent();

            if (activity.TimeSpent.GetValueOrDefault() == 0)
            {
                Debug.Unindent();
                Debug.Print("Activity is empty. Exiting AddToSummary");
                return null;
            }

            if (activity.ProjectID == null)
            {
                Debug.Unindent();
                Debug.Print("Activity.ProjectID is empty. Exiting AddToSummary");
                return null;
            }
            
           if ( summary == null )
			{
                summary = (EPTimeCardSummary) Summary.Cache.CreateInstance();
				summary.EarningType = activity.EarningTypeID;
				summary.ParentTaskID = activity.ParentTaskID;
				summary.ProjectID = activity.ProjectID;
				summary.ProjectTaskID = activity.ProjectTaskID;
				summary.IsBillable = activity.IsBillable;
				
				if ( activity.ParentTaskID != null)
				{
					EPActivity parentTask = PXSelect<EPActivity, Where<EPActivity.taskID, Equal<Required<EPActivity.taskID>>>>.Select(this, activity.ParentTaskID);
					if ( parentTask != null )
						summary.Description = parentTask.Subject;
				}
				else
				{
					PMTask pmTask = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.taskID>>, And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(this, activity.ProjectID, activity.ParentTaskID);
					if (pmTask != null)
						summary.Description = pmTask.Description;
				}

				dontSyncDetails = true;
				try
				{
					summary = Summary.Insert(summary);
                    summary.ProjectID = activity.ProjectID;//may have been overriden in rowInserting()
                    summary.ProjectTaskID = activity.ProjectTaskID;//may have been overriden in rowInserting()
				}
				finally
				{
					dontSyncDetails = false;
				}
			}

            AddActivityTimeToSummary(summary, activity, mult);

		    try
			{
				dontSyncDetails = true;
				summary = Summary.Update(summary);
			}
			finally
			{
				dontSyncDetails = false;
			}

            Debug.Unindent();
            Debug.Print("AddToSummary End");
            

			return summary;
		}

        protected virtual void AddActivityTimeToSummary(EPTimeCardSummary summary, EPActivity activity, int mult)
        {
            if (activity.TimeSpent != null)
            {
                switch (activity.StartDate.Value.DayOfWeek)
                {
                    case DayOfWeek.Monday:

                        summary.Mon = summary.Mon.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
                        break;
                    case DayOfWeek.Tuesday:
                        summary.Tue = summary.Tue.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
                        break;
                    case DayOfWeek.Wednesday:
                        summary.Wed = summary.Wed.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
                        break;
                    case DayOfWeek.Thursday:
                        summary.Thu = summary.Thu.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
                        break;
                    case DayOfWeek.Friday:
                        summary.Fri = summary.Fri.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
                        break;
                    case DayOfWeek.Saturday:
                        summary.Sat = summary.Sat.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
                        break;
                    case DayOfWeek.Sunday:
                        summary.Sun = summary.Sun.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
                        break;
                }
            }
        }

        /// <summary>
        /// When True Summary records are not updated as a result of a detail row update.
        /// </summary>
		protected bool dontSyncSummary = false;
        protected virtual void UpdateAdjustingActivities(EPTimeCardSummary summary)
		{
            if (summary == null)
                throw new ArgumentNullException();

            EPTimeCard doc = PXSelect<EPTimeCard, Where<EPTimeCard.timeCardCD, Equal<Required<EPTimeCard.timeCardCD>>>>.Select(this, summary.TimeCardCD);
            if (doc == null)
                return;

            Dictionary<DayOfWeek, DayActivities> dict = GetActivities(summary, doc);
            
            PX.Objects.EP.PXWeekSelector2Attribute.WeekInfo weekInfo = PXWeekSelector2Attribute.GetWeekInfo(this, doc.WeekID.Value);

            if (weekInfo.Mon.Enabled)
                UpdateAdjustingActivities(summary, dict, DayOfWeek.Monday, SetEmployeeTime(weekInfo.Mon.Date).Value);
            if (weekInfo.Tue.Enabled)
                UpdateAdjustingActivities(summary, dict, DayOfWeek.Tuesday, SetEmployeeTime(weekInfo.Tue.Date).Value);
            if (weekInfo.Wed.Enabled)
                UpdateAdjustingActivities(summary, dict, DayOfWeek.Wednesday, SetEmployeeTime(weekInfo.Wed.Date).Value);
            if (weekInfo.Thu.Enabled)
                UpdateAdjustingActivities(summary, dict, DayOfWeek.Thursday, SetEmployeeTime(weekInfo.Thu.Date).Value);
            if (weekInfo.Fri.Enabled)
                UpdateAdjustingActivities(summary, dict, DayOfWeek.Friday, SetEmployeeTime(weekInfo.Fri.Date).Value);
            if (weekInfo.Sat.Enabled)
                UpdateAdjustingActivities(summary, dict, DayOfWeek.Saturday, SetEmployeeTime(weekInfo.Sat.Date).Value);
            if (weekInfo.Sun.Enabled)
                UpdateAdjustingActivities(summary, dict, DayOfWeek.Sunday, SetEmployeeTime(weekInfo.Sun.Date).Value);
        }

        protected virtual DateTime? SetEmployeeTime(DateTime? date)
        {
            EPEmployee emp = dummy.Select();
            if (emp != null)
            {
				CSCalendar cal = PXSelect<CSCalendar, Where<CSCalendar.calendarID, Equal<Required<CSCalendar.calendarID>>>>.Select(this, emp.CalendarID);
                if (cal != null)
                {
                    switch (date.Value.DayOfWeek)
                    {
                        case DayOfWeek.Monday: return PXDBDateAndTimeAttribute.CombineDateTime(date, cal.MonStartTime);
                        case DayOfWeek.Tuesday: return PXDBDateAndTimeAttribute.CombineDateTime(date, cal.TueStartTime);
                        case DayOfWeek.Wednesday: return PXDBDateAndTimeAttribute.CombineDateTime(date, cal.WedStartTime);
                        case DayOfWeek.Thursday: return PXDBDateAndTimeAttribute.CombineDateTime(date, cal.ThuStartTime);
                        case DayOfWeek.Friday: return PXDBDateAndTimeAttribute.CombineDateTime(date, cal.FriStartTime);
                        case DayOfWeek.Saturday: return PXDBDateAndTimeAttribute.CombineDateTime(date, cal.SatStartTime);
                        case DayOfWeek.Sunday: return PXDBDateAndTimeAttribute.CombineDateTime(date, cal.SunStartTime);
                    }
                }
            }

            return PXDBDateAndTimeAttribute.CombineDateTime(date, new DateTime(2008, 1, 1, 9, 0, 0));//use default 9:00
        }

        protected virtual void UpdateAdjustingActivities(EPTimeCardSummary summary, Dictionary<DayOfWeek, DayActivities> dict, DayOfWeek dayOfWeek, DateTime startDate)
        {
            if (summary == null)
                throw new ArgumentNullException("summary");

            if (dict == null)
                throw new ArgumentNullException("dict");

            Debug.Print("UpdateAdjustingActivities for {0} Start", dayOfWeek);
            Debug.Indent();

            int summaryTimeTotal = 0;

            if (Summary.Cache.GetStatus(summary) != PXEntryStatus.Deleted && Summary.Cache.GetStatus(summary) != PXEntryStatus.InsertedDeleted)
                summaryTimeTotal = summary.GetTimeTotal(dayOfWeek).GetValueOrDefault();

            int dayTotal = 0;
            if (dict.ContainsKey(dayOfWeek))
            {
                dayTotal = dict[dayOfWeek].GetTotalTime(summary.LineNbr.Value);
            }

            EPTimecardDetail adjust = null;
            if (dict.ContainsKey(dayOfWeek))
            {
                adjust = dict[dayOfWeek].GetAdjustingActivity(summary.LineNbr.Value);
            }

            if (summaryTimeTotal != dayTotal)
            {
                if (adjust == null && summaryTimeTotal - dayTotal != 0)
                {
                    dontSyncSummary = true;
                    try
                    {
                        adjust = Activities.Insert();
						adjust.EarningTypeID = summary.EarningType;
                        adjust.SummaryLineNbr = summary.LineNbr;
                        adjust.StartDate = startDate;
                        adjust.Day = ((int)adjust.StartDate.Value.DayOfWeek).ToString();
                        adjust.IsBillable = summary.IsBillable;
                        if (!string.IsNullOrEmpty(summary.Description))
                            adjust.Subject = summary.Description;
                        else
                        {
							adjust.Subject = string.Format("Summary {0} Activities", dayOfWeek);
                        }
                        adjust.ClassID = CRActivityClass.Activity;
						adjust.Type = EpSetup.Current.DefaultActivityType;
                        adjust.TimeSpent = (summaryTimeTotal - dayTotal);
                        if (adjust.IsBillable == true)
                            adjust.TimeBillable = adjust.TimeSpent;
                        adjust.ProjectID = summary.ProjectID;
                        adjust.ProjectTaskID = summary.ProjectTaskID;
                        adjust.ParentTaskID = summary.ParentTaskID;
                        adjust.UIStatus = ActivityStatusAttribute.Completed;
						Activities.Cache.SetDefaultExt<EPTimecardDetail.approverID>(adjust);
						Activities.Cache.SetDefaultExt<EPTimecardDetail.approvalStatus>(adjust);
                        InitEarningType(adjust);
                        RecalculateFields(adjust);
                    }
                    finally
                    {
                        dontSyncSummary = false;
                    }
                }
                else if (adjust != null && summaryTimeTotal == 0 && adjust.SummaryLineNbr != null)//delete only adjusting activity that was added automatically on summary update.
                {
                    dontSyncSummary = true;
                    try
                    {
                        Activities.Delete(adjust);
                    }
                    finally
                    {
                        dontSyncSummary = false;
                    }
                }
                else if (adjust != null)
                {
                    adjust.TimeSpent = adjust.TimeSpent + summaryTimeTotal - dayTotal;
                    if (!string.IsNullOrEmpty(summary.Description))
                        adjust.Subject = summary.Description;
                    adjust.IsBillable = summary.IsBillable;
					if (adjust.IsBillable == true)
						adjust.TimeBillable = adjust.TimeSpent;
					RecalculateFields(adjust);

                    dontSyncSummary = true;
                    try
                    {
                        adjust = Activities.Update(adjust);
                    }
                    finally
                    {
                        dontSyncSummary = false;
                    }

                }
            }
            else
            {
                if (adjust != null)
                {
                    if (!string.IsNullOrEmpty(summary.Description))
                    {
                        Activities.Cache.SetValue<EPTimecardDetail.subject>(adjust, summary.Description);
                        if (Activities.Cache.GetStatus(adjust) == PXEntryStatus.Notchanged)
                            Activities.Cache.SetStatus(adjust, PXEntryStatus.Updated);
                    }
                }
            }

            Debug.Unindent();
            Debug.Print("UpdateAdjustingActivities for {0} End", dayOfWeek);
        }

        protected virtual Dictionary<DayOfWeek, DayActivities> GetActivities(EPTimeCardSummary summary, EPTimeCard doc)
		{
            if (summary == null)
                throw new ArgumentNullException("summary");
            if (doc == null)
                throw new ArgumentNullException("doc");

			Dictionary<DayOfWeek, DayActivities> dict = new Dictionary<DayOfWeek, DayActivities>();

            EPTimeCardSummary duplicate = FindDuplicate(summary);

            foreach (EPTimecardDetail activity in GetDetails(summary, doc, false))
			{
                if ( duplicate != null && duplicate.LineNbr == activity.SummaryLineNbr )
                    continue;

                DayOfWeek day = activity.StartDate.Value.DayOfWeek;

				if ( dict.ContainsKey(activity.StartDate.Value.DayOfWeek) )
				{
					dict[activity.StartDate.Value.DayOfWeek].Activities.Add(activity);
				}
				else
				{
					DayActivities d = new DayActivities();
					d.Day = day;
					d.Activities.Add(activity);
					dict.Add(activity.StartDate.Value.DayOfWeek, d);
				}
			}

			return dict;
		}

        protected virtual EPTimeCardSummary FindDuplicate(EPTimeCardSummary summary)
        {
            EPTimeCardSummary duplicate;
            PXSelectBase<EPTimeCardSummary> select = new PXSelect<EPTimeCardSummary,
                Where<EPTimeCardSummary.timeCardCD, Equal<Current<EPTimeCardSummary.timeCardCD>>,
                    And<EPTimeCardSummary.earningType, Equal<Current<EPTimeCardSummary.earningType>>,
                    And<EPTimeCardSummary.isBillable, Equal<Current<EPTimeCardSummary.isBillable>>,
                    And<EPTimeCardSummary.lineNbr, NotEqual<Current<EPTimeCardSummary.lineNbr>>>>>>>(this);

            if (summary.ParentTaskID != null)
            {
                select.WhereAnd<Where<EPTimeCardSummary.parentTaskID, Equal<Current<EPTimeCardSummary.parentTaskID>>>>();
            }

            if (summary.ProjectID != null)
            {
                select.WhereAnd<Where<EPTimeCardSummary.projectID, Equal<Current<EPTimeCardSummary.projectID>>>>();
                if (summary.ProjectTaskID != null)
                {
                    select.WhereAnd<Where<EPTimeCardSummary.projectTaskID, Equal<Current<EPTimeCardSummary.projectTaskID>>>>();
                }
                duplicate = (EPTimeCardSummary)select.View.SelectSingleBound(new object[] { summary });
            }
            else
            {
                select.WhereAnd<Where<EPTimeCardSummary.projectID, IsNull, Or<EPTimeCardSummary.projectID, Equal<Required<EPTimeCardSummary.projectID>>>>>();
                duplicate = (EPTimeCardSummary) select.View.SelectSingleBound(new object[] { summary }, new object[] { PM.ProjectDefaultAttribute.NonProject(this) });
            }

            return duplicate;
        }

        protected virtual List<EPTimecardDetail> GetDetails(EPTimeCardSummary summary, EPTimeCard doc, bool onlyManual)
        {
            if (summary == null)
                throw new ArgumentNullException("summary");
            if (doc == null)
                throw new ArgumentNullException("doc");

            PXSelectBase<EPTimecardDetail> select = new PXSelectJoin<EPTimecardDetail,
            InnerJoin<EPEmployee, On<EPEmployee.userID, Equal<EPTimecardDetail.owner>>>,
			Where<EPEmployee.bAccountID, Equal<Current<EPTimeCard.employeeID>>,
            And<EPTimecardDetail.earningTypeID, Equal<Current<EPTimeCardSummary.earningType>>,
            And<EPTimecardDetail.classID, NotEqual<CRActivityClass.emailRouting>,
            And<EPTimecardDetail.classID, NotEqual<CRActivityClass.task>,
            And<EPTimecardDetail.classID, NotEqual<CRActivityClass.events>,
            And<EPTimecardDetail.isBillable, Equal<Current<EPTimeCardSummary.isBillable>>,
            And<EPTimecardDetail.weekID, Equal<Current<EPTimeCard.weekId>>,
            And<EPTimecardDetail.trackTime, Equal<True>,
            And<Where<EPTimecardDetail.timeCardCD, IsNull, Or<EPTimecardDetail.timeCardCD, Equal<Current<EPTimeCard.timeCardCD>>>>>>>>>>>>>>(this);

            if (onlyManual == true)
            {
                select.WhereAnd<Where<EPTimecardDetail.summaryLineNbr, IsNull>>();
            }

            List<object> resultset;
            if (summary.ParentTaskID != null)
            {
                select.WhereAnd<Where<EPTimecardDetail.parentTaskID, Equal<Current<EPTimeCardSummary.parentTaskID>>>>();
            }

            if (summary.ProjectID != null)
            {
                select.WhereAnd<Where<EPTimecardDetail.projectID, Equal<Current<EPTimeCardSummary.projectID>>>>();
                if (summary.ProjectTaskID != null)
                {
                    select.WhereAnd<Where<EPTimecardDetail.projectTaskID, Equal<Current<EPTimeCardSummary.projectTaskID>>>>();
                }
                else
                {
                    select.WhereAnd<Where<EPTimecardDetail.projectTaskID, IsNull>>();
                }
                resultset = select.View.SelectMultiBound(new object[] { summary, doc });
            }
            else
            {
                select.WhereAnd<Where<EPTimecardDetail.projectID, IsNull, Or<EPTimecardDetail.projectID, Equal<Required<EPTimecardDetail.projectID>>>>>();
                resultset = select.View.SelectMultiBound(new object[] { summary, doc }, new object[] { PM.ProjectDefaultAttribute.NonProject(this) });
            }

            List<EPTimecardDetail> result = new List<EPTimecardDetail>(resultset.Count);
            foreach (PXResult<EPTimecardDetail, EPEmployee> item in resultset)
            {
                result.Add((EPTimecardDetail)item);
            }

            return result;
        }

        protected virtual void RecalculateTotals(EPTimeCard timecard)
        {
            if (timecard == null)
                throw new ArgumentNullException();

            List<EPTimecardDetail> list = new List<EPTimecardDetail>();

            foreach (EPTimecardDetail detail in Activities.Select())
            {
               list.Add(detail);
            }

            RecalculateTotals(timecard, list);
        }

        protected virtual void RecalculateTotals(EPTimeCard timecard, List<EPTimecardDetail> details )
        {
            if (timecard == null)
                throw new ArgumentNullException("timecard");

            if (details == null)
                throw new ArgumentNullException("details");

            int timeSpent = 0;
            int regularTime = 0;
            int overtimeSpent = 0;
            int timeBillable = 0;
            int overtimeBillable = 0;

            foreach (EPTimecardDetail detail in details)
            {
                timeSpent += detail.TimeSpent.GetValueOrDefault();
                regularTime += detail.RegularTimeCalc.GetValueOrDefault();
                timeBillable += detail.BillableTimeCalc.GetValueOrDefault();
                overtimeSpent += detail.OverTimeCalc.GetValueOrDefault();
                overtimeBillable += detail.BillableOvertimeCalc.GetValueOrDefault();
            }


            timecard.TimeSpentCalc = regularTime;
            timecard.OvertimeSpentCalc = overtimeSpent;
            timecard.TotalSpentCalc = timeSpent;

            timecard.TimeBillableCalc = timeBillable;
            timecard.OvertimeBillableCalc = overtimeBillable;
            timecard.TotalBillableCalc = timeBillable + overtimeBillable;
        }



        protected virtual bool ValidateTotals(EPTimeCard timecard, out string errorMsg)
        {
            if (timecard == null)
                throw new ArgumentNullException();

            bool valid = true;
            errorMsg = null;

            EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.Select(this, timecard.EmployeeID);
            if (employee == null || employee.HoursValidation == HoursValidationOption.None)
            {
                return true;//always valid
            }

            PXUIFieldAttribute.SetError<EPTimeCard.timeSpentCalc>(Document.Cache, timecard, null);
            PXUIFieldAttribute.SetError<EPTimeCard.overtimeSpentCalc>(Document.Cache, timecard, null);
            PXUIFieldAttribute.SetError<EPTimeCard.totalSpentCalc>(Document.Cache, timecard, null);

            DateTime date = Accessinfo.BusinessDate ?? DateTime.Now;
	        bool isFullWeek = true;
			if (timecard.WeekID != null)
			{
				EPWeekRaw week = PXSelectorAttribute.Select<EPTimeCard.weekId>(Document.Cache, timecard) as EPWeekRaw;
				if (week != null)
				{
					date = week.StartDate.Value;
					isFullWeek = week.IsFullWeek.Value;
				}
			}

			EPEmployeeRate rate = CreateEmployeeCostEngine().GetEmployeeRate(timecard.EmployeeID, date);
			if (isFullWeek && (rate == null || rate.RegularHours == null))
            {
                valid = false;
                errorMsg = Messages.RGIsNotDefinedForEmployee;
                Document.Cache.RaiseExceptionHandling<EPTimeCard.employeeID>(timecard, timecard.EmployeeID, new PXSetPropertyException(errorMsg, PXErrorLevel.Warning));
            }
            else if (isFullWeek && (timecard.TimeSpentCalc > (rate.RegularHours * 60)))
            {
                valid = false;
                errorMsg = string.Format(Messages.TotalTimeForWeekCannotExceedHours, rate.RegularHours);
                Document.Cache.RaiseExceptionHandling<EPTimeCard.timeSpentCalc>(timecard, timecard.TimeSpentCalc, new PXSetPropertyException(errorMsg, PXErrorLevel.Warning));
            }
            else if ( isFullWeek && (timecard.TimeSpentCalc < (rate.RegularHours*60)))
            {
                valid = false;

                if (timecard.OvertimeSpentCalc > 0)
                {
                    errorMsg = string.Format(Messages.OvertimeNotAllowed, rate.RegularHours);
                    Document.Cache.RaiseExceptionHandling<EPTimeCard.overtimeSpentCalc>(timecard, timecard.OvertimeSpentCalc, new PXSetPropertyException(errorMsg, PXErrorLevel.Warning));
                }
                else
                {
                    errorMsg = string.Format(Messages.TimecradIsNotNormalized, rate.RegularHours);
                    Document.Cache.RaiseExceptionHandling<EPTimeCard.totalSpentCalc>(timecard, timecard.TotalSpentCalc, new PXSetPropertyException(errorMsg, PXErrorLevel.Warning));
                }
                
            }

            return valid;
        }
					
        protected virtual PM.PMTran CreateItemTransaction(EPTimeCardItem record, int? employeeID, DateTime? date, decimal? qty, PXCache tranCache)
        {
            InventoryItem nsItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, record.InventoryID);
            if (nsItem == null)
            {
                throw new PXException(Messages.InventoryItemIsEmpty);
            }

            if (nsItem.InvtAcctID == null)
            {
                throw new PXException(Messages.ExpenseAccrualIsRequired, nsItem.InventoryCD.Trim());
            }

            if (nsItem.InvtSubID == null)
            {
                throw new PXException(Messages.ExpenseAccrualSubIsRequired, nsItem.InventoryCD.Trim());
            }

            Contract contract = PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(this, record.ProjectID);

            decimal? cost = 0m;

            int? accountID = nsItem.COGSAcctID;
            int? offsetaccountID = nsItem.InvtAcctID;
            int? accountGroupID = null;
            string subCD = null;
            string offsetSubCD = null;

            int? branchID = null;
            EP.EPEmployee emp = PXSelect<EP.EPEmployee, Where<EP.EPEmployee.bAccountID, Equal<Required<EP.EPEmployee.bAccountID>>>>.Select(this, employeeID);
            if (emp != null)
            {
                Branch branch = PXSelect<Branch, Where<Branch.bAccountID, Equal<Required<EPEmployee.parentBAccountID>>>>.Select(this, emp.ParentBAccountID);
                if (branch != null)
                {
                    branchID = branch.BranchID;
                }
            }

            if (contract.BaseType == PMProject.ProjectBaseType.Project)//contract do not record money only usage.
            {
                cost = nsItem.StdCost;

                if (contract.NonProject != true)
                {
                    PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(this, record.ProjectID, record.TaskID);

                    #region Combine Account and Subaccount

                    if (ExpenseAccountSource == PMAccountSource.Project)
                    {
                        if (contract.DefaultAccountID != null)
                        {
                            accountID = contract.DefaultAccountID;
                            Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
                            if (account.AccountGroupID == null)
                            {
                                throw new PXException(Messages.NoAccountGroupOnProject, account.AccountCD.Trim(), contract.ContractCD.Trim());
                            }
                            accountGroupID = account.AccountGroupID;
                        }
                        else
                        {
                            PXTrace.WriteWarning(Messages.NoDefualtAccountOnProject, contract.ContractCD.Trim());
                        }
                    }
                    else if (ExpenseAccountSource == PMAccountSource.Task)
                    {

                        if (task.DefaultAccountID != null)
                        {
                            accountID = task.DefaultAccountID;
                            Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
                            if (account.AccountGroupID == null)
                            {
                                throw new PXException(Messages.NoAccountGroupOnTask, account.AccountCD.Trim(), contract.ContractCD.Trim(), task.TaskCD.Trim());
                            }
                            accountGroupID = account.AccountGroupID;
                        }
                        else
                        {
                            PXTrace.WriteWarning(Messages.NoDefualtAccountOnTask, contract.ContractCD.Trim(), task.TaskCD.Trim());
                        }
                    }
                    else if (ExpenseAccountSource == PMAccountSource.Resource)
                    {
                        if (emp.ExpenseAcctID != null)
                        {
                            accountID = emp.ExpenseAcctID;
                            Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
                            if (account.AccountGroupID == null)
                            {
                                throw new PXException(Messages.NoAccountGroupOnEmployee, account.AccountCD, emp.AcctCD.Trim());
                            }
                            accountGroupID = account.AccountGroupID;
                        }
                        else
                        {
                            PXTrace.WriteWarning(Messages.NoExpenseAccountOnEmployee, emp.AcctCD.Trim());
                        }
                    }
                    else
                    {
                        if (accountID == null)
                        {
                            throw new PXException(Messages.NoExpenseAccountOnInventory, nsItem.InventoryCD.Trim());
                        }

                        //defaults to InventoryItem.COGSAcctID
                        Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
                        if (account.AccountGroupID == null)
                        {
                            throw new PXException(Messages.NoAccountGroupOnInventory, account.AccountCD.Trim(), nsItem.InventoryCD.Trim());
                        }
                        accountGroupID = account.AccountGroupID;
                    }


                    if (accountGroupID == null)
                    {
                        //defaults to InventoryItem.COGSAcctID
                        Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
                        if (account.AccountGroupID == null)
                        {
                            throw new PXException(Messages.AccountGroupIsNotAssignedForAccount, account.AccountCD.Trim());
                        }
                        accountGroupID = account.AccountGroupID;
                    }


                    if (!string.IsNullOrEmpty(ExpenseSubMask))
                    {
                        if (ExpenseSubMask.Contains(PMAccountSource.InventoryItem) && nsItem.COGSSubID == null)
                        {
                            throw new PXException(Messages.NoExpenseSubOnInventory, nsItem.InventoryCD.Trim());
                        }
                        if (ExpenseSubMask.Contains(PMAccountSource.Project) && contract.DefaultSubID == null)
                        {
                            throw new PXException(Messages.NoExpenseSubOnProject, contract.ContractCD.Trim());
                        }
                        if (ExpenseSubMask.Contains(PMAccountSource.Task) && task.DefaultSubID == null)
                        {
                            throw new PXException(Messages.NoExpenseSubOnTask, contract.ContractCD.Trim(), task.TaskCD.Trim());
                        }
                        if (ExpenseSubMask.Contains(PMAccountSource.Resource) && emp.ExpenseSubID == null)
                        {
                            throw new PXException(Messages.NoExpenseSubOnEmployee, emp.AcctCD.Trim());
                        }


                        subCD = PM.SubAccountMaskAttribute.MakeSub<PMSetup.expenseSubMask>(this, ExpenseSubMask,
                            new object[] { nsItem.COGSSubID, contract.DefaultSubID, task.DefaultSubID, emp.ExpenseSubID },
                            new Type[] { typeof(InventoryItem.cOGSSubID), typeof(Contract.defaultSubID), typeof(PMTask.defaultSubID), typeof(EPEmployee.expenseSubID) });
                    }

                    #endregion

                    #region Combine Accrual Account and Subaccount

                    if (ExpenseAccrualAccountSource == PMAccountSource.Project)
                    {
                        if (contract.DefaultAccrualAccountID != null)
                        {
                            offsetaccountID = contract.DefaultAccrualAccountID;
                        }
                        else
                        {
                            PXTrace.WriteWarning(EP.Messages.NoDefualtAccrualAccountOnProject, contract.ContractCD.Trim());
                        }
                    }
                    else if (ExpenseAccrualAccountSource == PMAccountSource.Task)
                    {
                        if (task.DefaultAccrualAccountID != null)
                        {
                            offsetaccountID = task.DefaultAccrualAccountID;
                        }
                        else
                        {
                            PXTrace.WriteWarning(EP.Messages.NoDefualtAccountOnTask, contract.ContractCD.Trim(), task.TaskCD.Trim());
                        }
                    }
                    else
                    {
                        if (offsetaccountID == null)
                        {
                            throw new PXException(EP.Messages.NoAccrualExpenseAccountOnInventory, nsItem.InventoryCD.Trim());
                        }
                    }

                    if (!string.IsNullOrEmpty(ExpenseAccrualSubMask))
                    {
                        if (ExpenseAccrualSubMask.Contains(PMAccountSource.InventoryItem) && nsItem.InvtSubID == null)
                        {
                            throw new PXException(EP.Messages.NoExpenseAccrualSubOnInventory, nsItem.InventoryCD.Trim());
                        }
                        if (ExpenseAccrualSubMask.Contains(PMAccountSource.Project) && contract.DefaultAccrualSubID == null)
                        {
                            throw new PXException(EP.Messages.NoExpenseAccrualSubOnProject, contract.ContractCD.Trim());
                        }
                        if (ExpenseAccrualSubMask.Contains(PMAccountSource.Task) && task.DefaultAccrualSubID == null)
                        {
                            throw new PXException(EP.Messages.NoExpenseAccrualSubOnTask, contract.ContractCD.Trim(), task.TaskCD.Trim());
                        }
						if (ExpenseAccrualSubMask.Contains(PMAccountSource.Resource) && emp.ExpenseSubID == null)
						{
							throw new PXException(Messages.NoExpenseSubOnEmployee, emp.AcctCD.Trim());
						}

                        offsetSubCD = PM.SubAccountMaskAttribute.MakeSub<PMSetup.expenseAccrualSubMask>(this, ExpenseAccrualSubMask,
							new object[] { nsItem.InvtSubID, contract.DefaultAccrualSubID, task.DefaultAccrualSubID, emp.ExpenseSubID },
                            new Type[] { typeof(InventoryItem.invtSubID), typeof(Contract.defaultAccrualSubID), typeof(PMTask.defaultAccrualSubID), typeof(EPEmployee.expenseSubID)  });
                    }

                    #endregion
                }
                else
                {
                    //defaults to InventoryItem.COGSAcctID
                    Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
                    if (account.AccountGroupID == null)
                    {
                        throw new PXException(Messages.NoAccountGroupOnInventory, account.AccountCD.Trim(), nsItem.InventoryCD.Trim());
                    }
                    accountGroupID = account.AccountGroupID;
                }
            }

            int? subID = nsItem.COGSSubID;
            int? offsetSubID = nsItem.InvtSubID;
            EPSetup epsetup = PXSelect<EPSetup>.Select(this);
            if (epsetup != null && epsetup.PostToOffBalance == true)
            {
                accountGroupID = epsetup.OffBalanceAccountGroupID;
                accountID = null;
                offsetaccountID = null;
                offsetSubID = null;
                subCD = null;
                subID = null;
            }

            PMTran tran = (PMTran)tranCache.Insert();
            tran.BranchID = branchID;
            tran.AccountID = accountID;
            if (string.IsNullOrEmpty(subCD))
                tran.SubID = subID;
            if (string.IsNullOrEmpty(offsetSubCD))
                tran.OffsetSubID = offsetSubID;
            tran.AccountGroupID = accountGroupID;
            tran.ProjectID = record.ProjectID;
            tran.TaskID = record.TaskID;
            tran.InventoryID = record.InventoryID;
            tran.ResourceID = employeeID;
            tran.Date = date;
            tran.FinPeriodID = FinPeriodIDAttribute.GetPeriod(tran.Date.Value);
            tran.Qty = qty;
            tran.UOM = record.UOM;
            tran.Billable = true;
            tran.BillableQty = tran.Qty;
			tran.UnitRate = PXDBPriceCostAttribute.Round(this.Caches<PMTran>(), (decimal)cost);
            tran.Amount = null;
            tran.Description = record.Description;
            tran.OffsetAccountID = offsetaccountID;
            tran.IsQtyOnly = contract.BaseType == Contract.ContractBaseType.Contract;
            
            tran = (PMTran)tranCache.Update(tran);

            if (!string.IsNullOrEmpty(subCD))
                tranCache.SetValueExt<PMTran.subID>(tran, subCD);

            if (!string.IsNullOrEmpty(offsetSubCD))
                tranCache.SetValueExt<PMTran.offsetSubID>(tran, offsetSubCD);

            string noteText = PXNoteAttribute.GetNote(Items.Cache, record);
            PXNoteAttribute.SetNote(tranCache, tran, noteText);

            Guid[] fileIds = PXNoteAttribute.GetFileNotes(Items.Cache, record);
            if (fileIds != null && fileIds.Length > 0)
                PXNoteAttribute.SetFileNotes(tranCache, tran, fileIds);

            return tran;
        }

        protected virtual EmployeeCostEngine CreateEmployeeCostEngine()
        {
            return new EmployeeCostEngine(this);
        }

		protected PXCache CreateInstanceCache<TNode>(Type graphType)
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

        protected virtual bool IsFirstTimeCard(int? employeeID)
		{
			return employeeID == null ||
				PXSelectReadonly<EPTimeCard,
					Where<EPTimeCard.employeeID, Equal<Required<EPTimeCard.employeeID>>>>.
				SelectWindowed(this, 0, 1, employeeID).Count == 0;
		}

        protected virtual int? GetNextWeekID(int? employeeID)
		{
			var isFist = IsFirstTimeCard(employeeID);
			if (!isFist)
			{
				var lastCard = (EPTimeCard)PXSelectReadonly<EPTimeCard,
					Where<EPTimeCard.employeeID, Equal<Required<EPTimeCard.employeeID>>>,
					OrderBy<Desc<EPTimeCard.weekId>>>.
					SelectWindowed(this, 0, 1, employeeID);
				if (lastCard != null && lastCard.WeekID != null)
				{
					return PXWeekSelector2Attribute.GetNextWeekID(this, (int)lastCard.WeekID);
				}
			}
			return Accessinfo.BusinessDate.With(_ => PXWeekSelector2Attribute.GetWeekID(this, _));
		}

        
        protected virtual DateTime GetNextActivityStartDate(EPActivity row, int weekId, int employeeId)
		{
			DateTime date;
			var lastDate = ((PXResult<EPActivity, EPEmployee>)PXSelectJoin<EPActivity,
					InnerJoin<EPEmployee, On<EPEmployee.userID, Equal<EPActivity.owner>>>,
					Where<EPActivity.weekID, Equal<Required<EPActivity.weekID>>,
						And<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>,
						And<EPActivity.timeSheetCD, IsNull>>>,
					OrderBy<Desc<EPActivity.startDate>>>.
				SelectWindowed(this, 0, 1, weekId, employeeId)).
				With(_ => ((EPActivity)_).StartDate);
			if (lastDate != null)
			{
				date = ((DateTime)lastDate).Date.AddDays(1D);
				if (PXWeekSelector2Attribute.GetWeekID(this, date) != weekId)
					date = ((DateTime)lastDate).Date;
			}
			else date = PXWeekSelector2Attribute.GetWeekSrartDate(this, weekId);
			var calendarId = CRActivityMaint.GetCalendarID(this, row);
			if (!string.IsNullOrEmpty(calendarId))
			{
				DateTime startDate;
				DateTime endDate;
				CalendarHelper.CalculateStartEndTime(this, calendarId, date, out startDate, out endDate);
				date = startDate;
			}
			return date;
		}

		protected virtual EPTimeCard GetLastCorrection(EPTimeCard source)
	    {
			if (source.IsReleased == true)
			{
				EPTimeCard res = Document.Search<EPTimeCard.origTimeCardCD>(source.TimeCardCD);
				if (res != null)
					return GetLastCorrection(res);
			}
			return source;
	    }


	    #endregion

		#region Local Types

		public class DayActivities
		{
            public List<EPTimecardDetail> Activities;
			public DayOfWeek Day;

			public DayActivities()
			{
                Activities = new List<EPTimecardDetail>();
			}

            public int GetTotalTime(int summaryLineNbr)
			{
				int total = 0;
                foreach (EPTimecardDetail activity in Activities)
				{
                    if ( activity.SummaryLineNbr == null || activity.SummaryLineNbr == summaryLineNbr)
                        total += activity.TimeSpent.GetValueOrDefault();
				}

				return total;
			}

            public EPTimecardDetail GetAdjustingActivity(int summaryLineNbr)
			{
                foreach (EPTimecardDetail activity in Activities)
				{
                    if (activity.SummaryLineNbr == summaryLineNbr && activity.Released != true)
						return activity;
				}

				if (Activities.Count == 1 && Activities[0].SummaryLineNbr == null && Activities[0].Released != true)
                {
                    return Activities[0];
                }

				return null;
			}

		}

        /// <summary>
		/// Required for correct join
		/// </summary>
		[PXHidden]
        [Serializable]
        public partial class EPTimecardDetailOrig : EPTimecardDetail
		{
            #region simple BQL Fields override

            public new abstract class taskID : PX.Data.IBqlField { }
            public new abstract class parentTaskID : PX.Data.IBqlField { }
            public new abstract class origTaskID : PX.Data.IBqlField { }

            #endregion
		}

		/// <summary>
		/// Required for correct join
		/// </summary>
		[PXHidden]
        [Serializable]
        public partial class EPTimecardDetailEx : EPTimecardDetail
		{
			#region simple BQL Fields override
            
		    public new abstract class taskID : PX.Data.IBqlField { }
            public new abstract class parentTaskID : PX.Data.IBqlField { }
            public new abstract class origTaskID : PX.Data.IBqlField { }
            
            #endregion
		}

        [PXHidden]
        [Serializable]
        public partial class EPTimecardDetail : EPActivity
        {
            #region TimeCardCD

            public new abstract class timeCardCD : IBqlField { }

            [PXDBString(10)]
            [PXUIField(Visible = false)]
            public override String TimeCardCD { get; set; }

            #endregion
            #region ProjectID
            public new abstract class projectID : IBqlField { }
            [EPTimeCardActiveProjectAttribute]
            public override Int32? ProjectID { get; set; }
            #endregion
            #region ProjectTaskID
            public new abstract class projectTaskID : IBqlField { }

			[ActiveProjectTask(typeof(EPActivity.projectID), BatchModule.EP, AllowNullIfContract = true, DisplayName = "Project Task")]
            public override Int32? ProjectTaskID { get; set; }
            #endregion
            #region WeekID

            public new abstract class weekID : IBqlField { }

            [PXDBInt]
            [PXUIField(DisplayName = "Time Card Week")]
            public override Int32? WeekID
            {
                get { return this._WeekID; }
                set { this._WeekID = value; }
            }

            #endregion
            #region StartDate
            public new abstract class startDate : PX.Data.IBqlField
            {
            }
            [PXDefault]
			[EPDBDateAndTime(typeof(EPTimecardDetail.owner), WithoutDisplayNames = true)]
            [PXUIField(DisplayName = "Date")]
            public override DateTime? StartDate
            {
                get
                {
                    return this._StartDate;
                }
                set
                {
                    this._StartDate = value;
                }
            }
            #endregion
			#region CompletedDateTime
			public new abstract class completedDateTime : PX.Data.IBqlField
			{
			}
			[EPDBDateAndTime(typeof(EPTimecardDetail.owner), InputMask = "g", PreserveTime = true)]
			[PXUIField(DisplayName = "Completed At", Enabled = false)]
			public override DateTime? CompletedDateTime
			{
				get
				{
					return this._CompletedDateTime;
				}
				set
				{
					this._CompletedDateTime = value;
				}
			}
			#endregion
            #region TimeSpent
            public new abstract class timeSpent : IBqlField
            {
            }
            [PXTimeList]
            [PXDBInt]
            [PXUIField(DisplayName = "Time Spent")]
            public override Int32? TimeSpent
            {
                get
                {
                    return _TimeSpent;
                }
                set
                {
                    this._TimeSpent = value;
                }
            }
            #endregion
            #region Type
            public new abstract class type : IBqlField {}

	        [PXDBString(5, IsFixed = true, IsUnicode = false)]
	        [PXUIField(DisplayName = "Type", Required = true)]
	        [PXSelector(typeof (EPActivityType.type), DescriptionField = typeof (EPActivityType.description))]
	        [PXRestrictor(typeof (Where<EPActivityType.active, Equal<True>>), CR.Messages.InactiveActivityType, typeof (EPActivityType.type))]
	        [PXDefault(typeof (Search<EPSetup.defaultActivityType>), PersistingCheck = PXPersistingCheck.Nothing)]
	        public override String Type { get; set; }
            #endregion	
            #region IsBillable
            public new abstract class isBillable : PX.Data.IBqlField
            {
            }
            [PXDBBool()]
            [PXUIField(DisplayName = "Billable")]
            public override Boolean? IsBillable
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
            #region tstamp
            public new abstract class Tstamp : PX.Data.IBqlField
            {
            }

            [PXDBTimestamp(RecordComesFirst = true)]
            public override Byte[] tstamp
            {
                get
                {
                    return this._tstamp;
                }
                set
                {
                    this._tstamp = value;
                }
            }
            #endregion
           
            #region Unbound Fields (Calculated in the TimecardMaint graph)

            public abstract class day : IBqlField { }
            [PXString()]
            [PXUIField(DisplayName = "Day")]
            //[PXStringList(new string[] { "1", "2", "3", "4", "5", "6", "0" }, new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" })]
            public virtual string Day { get; set; }

            public abstract class isOvertimeCalc : IBqlField { }
            [PXBool()]
            [PXDefault(false, PersistingCheck=PXPersistingCheck.Nothing)]
            public virtual Boolean? IsOvertimeCalc { get; set; }

            public abstract class overtimeMultiplierCalc : IBqlField { }
            [PXDecimal(1)]
            [PXUIField(DisplayName = "OT Mult", Enabled = false)]
            public virtual Decimal? OvertimeMultiplierCalc { get; set; }

            private int? _billableTimeCalc;
            public abstract class billableTimeCalc : IBqlField { }
            [PXTimeList]
            [PXInt]
            [PXUIField(DisplayName = "Billable Time")]
            public virtual Int32? BillableTimeCalc {
                get { return _billableTimeCalc;  }
                set { _billableTimeCalc = value;  }
            }

            public abstract class billableOvertimeCalc : IBqlField { }
            [PXTimeList]
            [PXInt]
            [PXUIField(DisplayName = "Billable OT")]
            public virtual Int32? BillableOvertimeCalc { get; set; }

            public abstract class regularTimeCalc : IBqlField { }
            [PXInt]
            [PXUIField(DisplayName = "RH", Enabled = false)]
            public virtual Int32? RegularTimeCalc { get; set; }


            public abstract class overTimeCalc : IBqlField { }
            [PXInt]
            [PXUIField(DisplayName = "OT", Enabled = false)]
            public virtual Int32? OverTimeCalc { get; set; }

            #endregion

            #region simple BQL Fields override

            public new abstract class taskID : PX.Data.IBqlField { }
            public new abstract class parentTaskID : PX.Data.IBqlField { }
            public new abstract class origTaskID : PX.Data.IBqlField { }
            public new abstract class owner : PX.Data.IBqlField { }
            public new abstract class classID : PX.Data.IBqlField { }
            public new abstract class uistatus : PX.Data.IBqlField { }
            public new abstract class endDate : PX.Data.IBqlField { }
            public new abstract class overtimeSpent : PX.Data.IBqlField { }
            public new abstract class summaryLineNbr : IBqlField { }
            public new abstract class isCorrected : IBqlField { }
            #endregion
        }

        [PXHidden]
        [Serializable]
        public partial class EPTimecardTask : EPActivity
        {
            #region TimeCardCD

            public new abstract class timeCardCD : IBqlField { }

            [PXDBString(10)]
            [PXUIField(Visible = false)]
            public override String TimeCardCD { get; set; }

            #endregion
            #region ProjectID
            public new abstract class projectID : IBqlField { }
            [Project]
            public override Int32? ProjectID { get; set; }
            #endregion
            #region ProjectTaskID
            public new abstract class projectTaskID : IBqlField { }

            [ActiveProjectTask(typeof(EPActivity.projectID), BatchModule.EP, AllowNullIfContract = true, DisplayName = "Project Task")]
            public override Int32? ProjectTaskID { get; set; }
            #endregion
            #region WeekID

            public new abstract class weekID : IBqlField { }

            [PXDBInt]
            [PXUIField(DisplayName = "Time Card Week")]
            public override Int32? WeekID
            {
                get { return this._WeekID; }
                set { this._WeekID = value; }
            }

            #endregion
            #region StartDate
            public new abstract class startDate : PX.Data.IBqlField
            {
            }
            [PXDefault]
            [EPDBDateAndTime(typeof(EPTimecardTask.owner), WithoutDisplayNames = true)]
            [PXUIField(DisplayName = "Date")]
            public override DateTime? StartDate
            {
                get
                {
                    return this._StartDate;
                }
                set
                {
                    this._StartDate = value;
                }
            }
            #endregion
            #region TimeSpent
            public new abstract class timeSpent : IBqlField
            {
            }
            [PXTimeList]
            [PXDBInt]
            [PXUIField(DisplayName = "Time Spent")]
            public override Int32? TimeSpent
            {
                get
                {
                    return _TimeSpent;
                }
                set
                {
                    this._TimeSpent = value;
                }
            }
            #endregion

            #region Unbound Fields (Calculated in the TimecardMaint graph)

            public abstract class isOvertimeCalc : IBqlField { }
            [PXBool()]
            [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual Boolean? IsOvertimeCalc { get; set; }

            public abstract class overtimeMultiplierCalc : IBqlField { }
            [PXDecimal(1)]
            [PXUIField(DisplayName = "OT Mult")]
            public virtual Decimal? OvertimeMultiplierCalc { get; set; }

            private int? _billableTimeCalc;
            public abstract class billableTimeCalc : IBqlField { }
            [PXInt]
            [PXUIField(DisplayName = "Billable Time")]
            public virtual Int32? BillableTimeCalc
            {
                get { return _billableTimeCalc; }
                set { _billableTimeCalc = value; }
            }

            public abstract class billableOvertimeCalc : IBqlField { }
            [PXInt]
            [PXUIField(DisplayName = "Billable OT")]
            public virtual Int32? BillableOvertimeCalc { get; set; }

            public abstract class regularTimeCalc : IBqlField { }
            [PXInt]
            [PXUIField(DisplayName = "RH", Enabled = false)]
            public virtual Int32? RegularTimeCalc { get; set; }


            public abstract class overTimeCalc : IBqlField { }
            [PXInt]
            [PXUIField(DisplayName = "OT", Enabled = false)]
            public virtual Int32? OverTimeCalc { get; set; }

           

            #endregion

            #region simple BQL Fields override

            public new abstract class taskID : PX.Data.IBqlField { }
            public new abstract class parentTaskID : PX.Data.IBqlField { }
            public new abstract class origTaskID : PX.Data.IBqlField { }
            public new abstract class owner : PX.Data.IBqlField { }
            public new abstract class classID : PX.Data.IBqlField { }
            public new abstract class type : PX.Data.IBqlField { }
            public new abstract class uistatus : PX.Data.IBqlField { }
            public new abstract class endDate : PX.Data.IBqlField { }
            public new abstract class overtimeSpent : PX.Data.IBqlField { }
            public new abstract class summaryLineNbr : IBqlField { }
            #endregion
        }

        [Serializable]
        public class EPTimeCardItemOrig : EPTimeCardItem
        {
            #region TimeCardCD

            public new abstract class timeCardCD : IBqlField { }

            #endregion
            #region LineNbr
            public new abstract class lineNbr : IBqlField { }

            #endregion

            #region OrigLineNbr
            public new abstract class origLineNbr : IBqlField { }
            #endregion
        }

        [Serializable]
        public class EPTimeCardItemEx : EPTimeCardItem
        {
            #region TimeCardCD

            public new abstract class timeCardCD : IBqlField { }

            #endregion
            #region LineNbr
            public new abstract class lineNbr : IBqlField { }

            #endregion

            #region OrigLineNbr
            public new abstract class origLineNbr : IBqlField { }
            #endregion
        }

        private class SummaryRecord
        {
            public SummaryRecord(EPTimeCardSummary summary)
            {
                this.Summary = summary;
                LinkedDetails = new List<EPTimecardDetail>();
                NotLinkedDetails = new List<EPTimecardDetail>();
            }

            public EPTimeCardSummary Summary { get; private set; }
            public string SummaryKey { get; set; }
            public List<EPTimecardDetail> LinkedDetails { get; private set; }
            public List<EPTimecardDetail> NotLinkedDetails { get; private set; }

            public EPTimeCardSummary SummariseDetails()
            {
                EPTimeCardSummary keyItem = new EPTimeCardSummary();

                if (Summary != null)
                {
                    keyItem.TimeCardCD = Summary.TimeCardCD;
                    keyItem.LineNbr = Summary.LineNbr;
                }

                foreach (EPTimecardDetail detail in LinkedDetails)
                {
                    AddActivityTimeToSummary(keyItem, detail, 1);
                }

                foreach (EPTimecardDetail detail in NotLinkedDetails)
                {
                    AddActivityTimeToSummary(keyItem, detail, 1);
                }

                return keyItem;
            }

            private void AddActivityTimeToSummary(EPTimeCardSummary summary, EPActivity activity, int mult)
            {
                if (activity.TimeSpent != null)
                {
                    switch (activity.StartDate.Value.DayOfWeek)
                    {
                        case DayOfWeek.Monday:

                            summary.Mon = summary.Mon.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
                            break;
                        case DayOfWeek.Tuesday:
                            summary.Tue = summary.Tue.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
                            break;
                        case DayOfWeek.Wednesday:
                            summary.Wed = summary.Wed.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
                            break;
                        case DayOfWeek.Thursday:
                            summary.Thu = summary.Thu.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
                            break;
                        case DayOfWeek.Friday:
                            summary.Fri = summary.Fri.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
                            break;
                        case DayOfWeek.Saturday:
                            summary.Sat = summary.Sat.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
                            break;
                        case DayOfWeek.Sunday:
                            summary.Sun = summary.Sun.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
                            break;
                    }
                }
            }
        }

		#endregion
	}
}
