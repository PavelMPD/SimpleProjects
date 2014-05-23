using PX.SM;
using System;
using PX.Data;
using System.Collections.Generic;
using System.Collections;
using PX.Objects.GL;
using System.Diagnostics;

namespace PX.Objects.PM
{
	public class ProjectBalanceValidation : PXGraph<ProjectBalanceValidation>
	{
		public PXCancel<PMProject> Cancel;
		public PXProcessing<PMProject, Where<PMProject.baseType, Equal<PMProject.ProjectBaseType>,
			And<PMProject.isTemplate, Equal<False>,
			And<PMProject.nonProject, Equal<False>,
			And<PMProject.isActive, Equal<True>, Or<PMProject.isCompleted, Equal<True>>>>>>> Items;
				

		public ProjectBalanceValidation()
		{
			Items.SetSelected<PMProject.selected>();
			Items.SetProcessDelegate<ProjectBalanceValidationProcess>(Validate);
			Items.SetProcessCaption(GL.Messages.ProcValidate);
			Items.SetProcessAllCaption(GL.Messages.ProcValidateAll);
		}

		private static void Validate(ProjectBalanceValidationProcess graph, PMProject item)
		{
			graph.Clear();
			graph.RunProjectBalanceVerification(item);
			graph.Actions.PressSave();
		}
		
	}
	  
    [Serializable]
	public class ProjectBalanceValidationProcess : PXGraph<ProjectBalanceValidationProcess>
	{
		//public PXSelect<PMProjectStatusAccum> ProjectStatus;
		public PXSelect<PMTaskTotal> TaskTotals;
		public PXSelect<PMTaskAllocTotalAccum> AllocationTotals;
		public PXSelect<PMHistoryAccum> History;
		public PXSelect<PMHistory2Accum> History2;
		public PXSelect<PMTran, Where<PMTran.projectID, Equal<Required<PMTran.projectID>>,
				And<PMTran.released, Equal<True>>>> Transactions;

		public virtual void RunProjectBalanceVerification(PMProject project)
		{
			PXDatabase.Delete<PMTaskTotal>(new PXDataFieldRestrict(typeof(PMTaskTotal.projectID).Name, PXDbType.Int, 4, project.ContractID, PXComp.EQ));
			PXDatabase.Delete<PMTaskAllocTotal>(new PXDataFieldRestrict(typeof(PMTaskAllocTotal.projectID).Name, PXDbType.Int, 4, project.ContractID, PXComp.EQ));
			PXDatabase.Delete<PMHistory>(new PXDataFieldRestrict(typeof(PMHistory.projectID).Name, PXDbType.Int, 4, project.ContractID, PXComp.EQ));
						
			foreach (PMProjectStatusEx status in PXSelect<PMProjectStatusEx, Where<PMProjectStatusEx.projectID, Equal<Required<PMProjectStatusEx.projectID>>>>.Select(this, project.ContractID))
			{
				PXDatabase.Update<PMProjectStatus>(new PXDataFieldRestrict(typeof(PMProjectStatus.projectID).Name, PXDbType.Int, 4, project.ContractID, PXComp.EQ),
					new PXDataFieldAssign(typeof(PMProjectStatus.actualAmount).Name, PXDbType.Decimal, 0m),
					new PXDataFieldAssign(typeof(PMProjectStatus.actualQty).Name, PXDbType.Decimal, 0m));

				PMHistory2Accum hist2 = new PMHistory2Accum();
				hist2.ProjectID = status.ProjectID;
				hist2.ProjectTaskID = status.ProjectTaskID;
				hist2.AccountGroupID = status.AccountGroupID;
				hist2.InventoryID = status.InventoryID ?? PMProjectStatus.EmptyInventoryID;
				hist2.PeriodID = status.PeriodID;

				hist2 = History2.Insert(hist2);
				hist2.PTDBudgetAmount += status.Amount;
				hist2.PTDBudgetQty += status.Qty;
				hist2.BudgetAmount += status.Amount;
				hist2.BudgetQty += status.Qty;
				hist2.PTDRevisedAmount += status.RevisedAmount;
				hist2.PTDRevisedQty += status.RevisedQty;
				hist2.RevisedAmount += status.RevisedAmount;
				hist2.RevisedQty += status.RevisedQty;
			}

			PXSelectBase<PMTran> select = new PXSelectJoin<PMTran,
				LeftJoin<Account, On<PMTran.accountID, Equal<Account.accountID>>,
				InnerJoin<PMProject, On<PMProject.contractID, Equal<PMTran.projectID>>,
				LeftJoin<PMTask, On<PMTask.projectID, Equal<PMTran.projectID>, And<PMTask.taskID, Equal<PMTran.taskID>>>,
				LeftJoin<OffsetAccount, On<PMTran.offsetAccountID, Equal<OffsetAccount.accountID>>,
				LeftJoin<PMAccountGroup, On<PMAccountGroup.groupID, Equal<Account.accountGroupID>>,
				LeftJoin<OffsetPMAccountGroup, On<OffsetPMAccountGroup.groupID, Equal<OffsetAccount.accountGroupID>>>>>>>>,
				Where<PMTran.projectID, Equal<Required<PMTran.projectID>>,
				And<PMTran.released, Equal<True>>>>(this);


			foreach (PXResult<PMTran, Account, PMProject, PMTask, OffsetAccount, PMAccountGroup, OffsetPMAccountGroup> res in select.Select(project.ContractID))
			{
				PMTran tran = (PMTran)res;
				Account acc = (Account)res;
				PMAccountGroup ag = (PMAccountGroup)res;
				OffsetAccount offsetAcc = (OffsetAccount)res;
				OffsetPMAccountGroup offsetAg = (OffsetPMAccountGroup)res;
				PMTask task = (PMTask)res;

				IList<PMHistory> list = RegisterReleaseProcess.UpdateProjectBalance(this, task, tran, acc, ag, offsetAcc, offsetAg);

				#region History Update
				foreach (PMHistory item in list)
				{
					PMHistoryAccum hist = new PMHistoryAccum();
					hist.ProjectID = item.ProjectID;
					hist.ProjectTaskID = item.ProjectTaskID;
					hist.AccountGroupID = item.AccountGroupID;
					hist.InventoryID = item.InventoryID;
					hist.PeriodID = item.PeriodID;

					hist = History.Insert(hist);
					hist.FinPTDAmount += item.FinPTDAmount.GetValueOrDefault();
					hist.FinYTDAmount += item.FinYTDAmount.GetValueOrDefault();
					hist.FinPTDQty += item.FinPTDQty.GetValueOrDefault();
					hist.FinYTDQty += item.FinYTDQty.GetValueOrDefault();
					hist.TranPTDAmount += item.TranPTDAmount.GetValueOrDefault();
					hist.TranYTDAmount += item.TranYTDAmount.GetValueOrDefault();
					hist.TranPTDQty += item.TranPTDQty.GetValueOrDefault();
					hist.TranYTDQty += item.TranYTDQty.GetValueOrDefault();
				}


				
				#endregion
			
			}

			

			PXSelectBase<PMTran> select2 = new PXSelect<PMTran, 
				Where<PMTran.origProjectID, Equal<Required<PMTran.origProjectID>>, 
				And<PMTran.origTaskID, IsNotNull, 
				And<PMTran.origAccountGroupID, IsNotNull>>>>(this);

			foreach (PMTran tran in select2.Select(project.ContractID))
			{
				PMTaskAllocTotalAccum tat = new PMTaskAllocTotalAccum();
				tat.ProjectID = tran.OrigProjectID;
				tat.TaskID = tran.OrigTaskID;
				tat.AccountGroupID = tran.OrigAccountGroupID;
				tat.InventoryID = tran.InventoryID;

				tat = AllocationTotals.Insert(tat);
				tat.Amount += tran.Amount;
				tat.Quantity += tran.Qty;
			}

			foreach (PMProjectStatusAccum item in this.Caches[typeof(PMProjectStatusAccum)].Inserted)
			{
				Debug.Print("Task={0} AG={1} Qty={2} Amt={3}", item.ProjectTaskID, item.AccountGroupID, item.ActualQty, item.ActualAmount);

			}
		}

		[PXHidden]
        [Serializable]
		public partial class OffsetAccount : Account
		{
			#region AccountID
			public new abstract class accountID : PX.Data.IBqlField
			{
			}
			#endregion
			#region AccountCD
			public new abstract class accountCD : PX.Data.IBqlField
			{
			}
			#endregion
			#region AccountGroupID
			public new abstract class accountGroupID : PX.Data.IBqlField
			{
			}
			#endregion
		}

		[PXHidden]
        [Serializable]
		public partial class OffsetPMAccountGroup : PMAccountGroup
		{
			#region GroupID
			public new abstract class groupID : PX.Data.IBqlField
			{
			}

			#endregion
			#region GroupCD
			public new abstract class groupCD : PX.Data.IBqlField
			{
			}
			#endregion
			#region Type
			public new abstract class type : PX.Data.IBqlField
			{
			}
			#endregion
		}

		[PXHidden]
        [Serializable]
		public partial class PMProjectStatusEx : PMProjectStatus
		{
			#region AccountGroupID
			public new abstract class accountGroupID : PX.Data.IBqlField
			{
			}
			#endregion
			#region ProjectID
			public new abstract class projectID : PX.Data.IBqlField
			{
			}
			#endregion
			#region ProjectTaskID
			public new abstract class projectTaskID : PX.Data.IBqlField
			{
			}
			
			#endregion
			#region PeriodID
			public new abstract class periodID : PX.Data.IBqlField
			{
			}
			
			#endregion
			#region InventoryID
			public new abstract class inventoryID : PX.Data.IBqlField
			{
			}
			
			#endregion
		}
	}
}
