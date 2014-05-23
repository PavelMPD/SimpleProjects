using System;
using System.Collections.Generic;
using PX.Common;
using PX.Data;
using PX.Objects.CS;
using System.Collections;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.CT;
using PX.Objects.CR;

namespace PX.Objects.PM
{
	public class TemplateTaskMaint : PXGraph<TemplateTaskMaint>
	{
        #region DAC Attributes Override

        #region PMTask
        [Project(typeof(Where<PMProject.isTemplate, Equal<True>, And<PMProject.nonProject, Equal<False>>>), DisplayName = "Template ID", IsKey = true)]
        [PXParent(typeof(Select<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
        [PXDefault(typeof(PMProject.contractID))]
        protected virtual void PMTask_ProjectID_CacheAttached(PXCache sender)
        {
        }

        [PXDimensionSelector(ProjectTaskAttribute.DimensionName,
            typeof(Search<PMTask.taskCD, Where<PMTask.projectID, Equal<Current<PMTask.projectID>>>>),
            typeof(PMTask.taskCD),
            typeof(PMTask.taskCD), typeof(PMTask.locationID), typeof(PMTask.description), typeof(PMTask.status), DescriptionField = typeof(PMTask.description))]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
        [PXDefault]
        [PXUIField(DisplayName = "Task ID", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void PMTask_TaskCD_CacheAttached(PXCache sender)
        {
        }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(ProjectTaskStatus.Active)]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.Invisible, Visible = false)]
        protected virtual void PMTask_Status_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region PMProjectStatus
        [PXDBIdentity(IsKey = true)]
        protected virtual void PMProjectStatus_RowID_CacheAttached(PXCache sender)
        {
        }

        [PXDefault]
        [AccountGroupAttribute()]
        protected virtual void PMProjectStatus_AccountGroupID_CacheAttached(PXCache sender)
        {
        }

        [PXDefault(typeof(PMTask.projectID))]
        [PXDBInt]
        protected virtual void PMProjectStatus_ProjectID_CacheAttached(PXCache sender)
        {
        }

        [PXDefault(typeof(PMTask.taskID))]
        [PXDBInt]
        [PXParent(typeof(Select<PMTask, Where<PMTask.projectID, Equal<Current<PM.PMProjectStatus.projectID>>, And<PMTask.taskID, Equal<Current<PM.PMProjectStatus.projectTaskID>>>>>))]
        protected virtual void PMProjectStatus_ProjectTaskID_CacheAttached(PXCache sender)
        {
        }

        [PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.Invisible, Visible = false)]
        [PXDefault("")]
        [FinPeriodID]
        protected virtual void PMProjectStatus_PeriodID_CacheAttached(PXCache sender)
        {
        }

        [PXCheckUnique(typeof(PM.PMProjectStatus.accountGroupID), typeof(PM.PMProjectStatus.projectID), typeof(PM.PMProjectStatus.projectTaskID), typeof(PM.PMProjectStatus.periodID))]
        [PXDBInt]
        [PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
        [PMInventorySelector(typeof(Search2<InventoryItem.inventoryID,
            InnerJoin<PMAccountGroup, On<PMAccountGroup.groupID, Equal<Current<PM.PMProjectStatus.accountGroupID>>>,
            LeftJoin<PMInventorySelectorAttribute.Cogs, On<PMInventorySelectorAttribute.Cogs.accountID, Equal<InventoryItem.cOGSAcctID>>,
						LeftJoin<PMInventorySelectorAttribute.Exp, On<PMInventorySelectorAttribute.Exp.accountID, Equal<InventoryItem.cOGSAcctID>>,
            LeftJoin<PMInventorySelectorAttribute.Sale, On<PMInventorySelectorAttribute.Sale.accountID, Equal<InventoryItem.salesAcctID>>>>>>,
			Where<PMAccountGroup.type, Equal<AccountType.expense>,
			And<PMAccountGroup.groupID, Equal<PMInventorySelectorAttribute.Cogs.accountGroupID>,
			And<InventoryItem.stkItem, Equal<True>,
			Or<PMAccountGroup.type, Equal<AccountType.expense>,
				And<PMAccountGroup.groupID, Equal<PMInventorySelectorAttribute.Exp.accountGroupID>,
			And<InventoryItem.stkItem, Equal<False>,
			Or<PMAccountGroup.type, Equal<AccountType.income>,
			And<PMAccountGroup.groupID, Equal<PMInventorySelectorAttribute.Sale.accountGroupID>,
			Or<PMAccountGroup.type, Equal<AccountType.liability>,
			Or<PMAccountGroup.type, Equal<AccountType.asset>>>>>>>>>>>>), SubstituteKey = typeof(InventoryItem.inventoryCD), Filterable = true)]
        protected virtual void PMProjectStatus_InventoryID_CacheAttached(PXCache sender)
        {
        }

        [PXDBBool]
        [PXDefault(true)]
        protected virtual void PMProjectStatus_IsTemplate_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region ContractDetail
        
        [PXDBInt]
        [PXDBDefault(typeof(PMTask.projectID))]
        protected virtual void ContractDetail_ContractID_CacheAttached(PXCache sender) { }

        [PXDBInt]
        [PXDBDefault(typeof(PMTask.taskID))]
        [PXParent(typeof(Select<PMTask, Where<PMTask.projectID, Equal<Current<ContractDetail.contractID>>, And<PMTask.taskID, Equal<Current<ContractDetail.taskID>>>>>))]
        protected virtual void ContractDetail_TaskID_CacheAttached(PXCache sender) { }

		[PXDBInt()]
		[PXDBDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void ContractDetail_ContractItemID_CacheAttached(PXCache sender) { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Prepayment", Visibility = PXUIVisibility.Invisible, Visible=false)]
        protected virtual void ContractDetail_PrePayment_CacheAttached(PXCache sender) { }

		[PXDBInt(MinValue = 1)]
		[PXDefault(1, PersistingCheck = PXPersistingCheck.Null)]
		protected virtual void ContractDetail_RevID_CacheAttached(PXCache sender) { }

		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(PMTask.lineCtr))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		protected virtual void ContractDetail_LineNbr_CacheAttached(PXCache sender) { }
        #endregion
        
        #endregion

		#region Views/Selects

		public PXSelectJoin<PMTask, LeftJoin<PMProject, On<PMTask.projectID, Equal<PMProject.contractID>>>, Where<PMProject.nonProject, Equal<False>, And<PMProject.isTemplate, Equal<True>>>> Task;
		public PXSelect<PMTask, Where<PMTask.taskID, Equal<Current<PMTask.taskID>>>> TaskProperties;
        public PXSelect<ContractDetail,
            Where<ContractDetail.contractID, Equal<Current<PMTask.projectID>>,
            And<ContractDetail.taskID, Equal<Current<PMTask.taskID>>>>> BillingItems;
        public PXSelectJoin<PMProjectStatus, LeftJoin<PMAccountGroup, On<PMProjectStatus.accountGroupID, Equal<PMAccountGroup.groupID>>>, Where<PMProjectStatus.projectID, Equal<Current<PMTask.projectID>>, And<PMProjectStatus.projectTaskID, Equal<Current<PMTask.taskID>>>>> ProjectStatus;
		[PXViewName(Messages.TaskAnswers)]
		public CRAttributeList<PMTask> Answers;
		public PXSetup<PMSetup> Setup;
		public PXSetup<Company> CompanySetup;

		#endregion
				
		#region	Actions/Buttons

		public PXSave<PMTask> Save;
		public PXCancel<PMTask> Cancel;
		public PXInsert<PMTask> Insert;
		public PXDelete<PMTask> Delete;
		public PXFirst<PMTask> First;
		public PXPrevious<PMTask> previous;
		public PXNext<PMTask> next;
		public PXLast<PMTask> Last;
						
        #endregion		 

        public TemplateTaskMaint()
		{
			if (Setup.Current == null)
			{
				throw new PXException(Messages.SetupNotConfigured);
			}
		}
               
		#region Event Handlers


		protected virtual void PMTask_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PMTask row = e.Row as PMTask;
			if (row != null)
			{
				PMProject prj = PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>.SelectSingleBound(this, new object[] { row });
                PXUIFieldAttribute.SetEnabled<PMTask.autoIncludeInPrj>(sender, row, prj != null && prj.NonProject != true);
				//PXUIFieldAttribute.SetVisible<PMTask.projectID>(sender, row, prj != null && prj.NonProject != true);
			}
		}

        protected virtual void PMTask_CustomerID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            PMTask row = e.Row as PMTask;
            if(row == null) return;

            PMProject prj = PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>.SelectSingleBound(this, new object[] { row });
            if(prj != null && prj.NonProject == true)
            {
                e.Cancel = true;
            }
        }
		
		protected virtual void PMTask_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			PMTask row = e.Row as PMTask;
			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>.Select(this);
			if (row != null && project != null)
			{
				row.BillingID = project.BillingID;
				row.DefaultAccountID = project.DefaultAccountID;
				row.DefaultSubID = project.DefaultSubID;

			}
		}
		
		protected virtual void PMTask_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMTask row = e.Row as PMTask;
			if (row != null)
			{
				sender.SetDefaultExt<PMTask.customerID>(e.Row);
				sender.SetDefaultExt<PMTask.defaultAccountID>(e.Row);
				sender.SetDefaultExt<PMTask.defaultSubID>(e.Row);
			}
		}

		
        protected virtual void ContractDetail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            ContractDetail row = e.Row as ContractDetail;
            if (row != null)
            {
                row.ContractID = Task.Current.ProjectID;

                #region Check for Uniqueness
                if (row.InventoryID.HasValue)
                {
                    ContractDetail item = PXSelect<ContractDetail,
                        Where<ContractDetail.inventoryID, Equal<Required<InventoryItem.inventoryID>>,
                        And<ContractDetail.contractID, Equal<Current<PMTask.projectID>>,
                        And<ContractDetail.taskID, Equal<Current<PMTask.taskID>>>>>>.SelectWindowed(this, 0, 1, row.InventoryID);

                    if (item != null && item.ContractDetailID != row.ContractDetailID)
                    {
                        sender.RaiseExceptionHandling<ContractDetail.inventoryID>(row, row.InventoryID, new PXException(CT.Messages.ItemNotUnique));
                        e.Cancel = true;
                    }
                }
                #endregion
            }
        }

        protected virtual void ContractDetail_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
        {
            ContractDetail row = e.Row as ContractDetail;
            if (row != null)
            {
                #region Check for Uniqueness
                if (row.InventoryID.HasValue)
                {
                    ContractDetail item = PXSelect<ContractDetail,
                        Where<ContractDetail.inventoryID, Equal<Required<InventoryItem.inventoryID>>,
                        And<ContractDetail.contractID, Equal<Current<PMTask.projectID>>,
                        And<ContractDetail.taskID, Equal<Current<PMTask.taskID>>>>>>.SelectWindowed(this, 0, 1, row.InventoryID);

                    if (item != null && item.ContractDetailID != row.ContractDetailID)
                    {
                        sender.RaiseExceptionHandling<ContractDetail.inventoryID>(row, row.InventoryID, new PXException(CT.Messages.ItemNotUnique));
                        e.Cancel = true;
                    }
                }
                #endregion
            }
        }

        protected virtual void ContractDetail_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            OnContractDetailInventoryIDFieldUpdated(sender, e);
        }

        protected virtual void OnContractDetailInventoryIDFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            ContractDetail row = (ContractDetail)e.Row;
            if (row == null) return;

            InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, row.InventoryID);
            if (item != null)
            {
                row.UOM = item.SalesUnit;
                row.ItemFee = item.BasePrice;
                row.Description = item.Descr;

                sender.SetDefaultExt<ContractDetail.curyItemFee>(e.Row);
            }
        }

        protected virtual void ContractDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            ContractDetail row = e.Row as ContractDetail;
            if (row != null && Task.Current != null)
            {
                PXUIFieldAttribute.SetEnabled<ContractDetail.included>(sender, row, Task.Current.IsActive != true);
            }
        }

		#endregion
				
	}
}
