using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using System.Collections;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.CM;
using PX.Objects.EP;
using PX.Objects.CT;
using System.Collections.Specialized;

namespace PX.Objects.PM
{
    [Serializable]
	public class RegisterEntry : PXGraph<RegisterEntry, PMRegister>
	{
		#region DAC Attributes Override

		[PXDBInt()]
		[PXUIField(DisplayName = "Inventory ID")]
		[PMInventorySelector(typeof(Search2<InventoryItem.inventoryID,
		LeftJoin<PMAccountGroup, On<PMAccountGroup.groupID, Equal<Current<PMTran.accountGroupID>>>,
		LeftJoin<PMProject, On<PMProject.contractID, Equal<Current<PMTran.projectID>>>,
		LeftJoin<PMInventorySelectorAttribute.Cogs, On<PMInventorySelectorAttribute.Cogs.accountID, Equal<InventoryItem.cOGSAcctID>>,
		LeftJoin<PMInventorySelectorAttribute.Exp, On<PMInventorySelectorAttribute.Exp.accountID, Equal<InventoryItem.cOGSAcctID>>,
		LeftJoin<PMInventorySelectorAttribute.Sale, On<PMInventorySelectorAttribute.Sale.accountID, Equal<InventoryItem.salesAcctID>>>>>>>,
		Where2< Where<PMAccountGroup.type, Equal<AccountType.expense>,
			And<PMAccountGroup.groupID, Equal<PMInventorySelectorAttribute.Cogs.accountGroupID>,
			And<InventoryItem.stkItem, Equal<True>,
			Or<PMAccountGroup.type, Equal<AccountType.expense>,
				And<PMAccountGroup.groupID, Equal<PMInventorySelectorAttribute.Exp.accountGroupID>,
			And<InventoryItem.stkItem, Equal<False>,
			Or<PMAccountGroup.type, Equal<AccountType.income>,
			And<PMAccountGroup.groupID, Equal<PMInventorySelectorAttribute.Sale.accountGroupID>,
			Or<PMAccountGroup.type, Equal<AccountType.liability>,
			Or<PMAccountGroup.type, Equal<AccountType.asset>,
            Or<PMAccountGroup.type, Equal<PMAccountType.offBalance>>>>>>>>>>>>,
		Or2<Where<PMProject.baseType, Equal<PMProject.ContractBaseType>, And<InventoryItem.stkItem, Equal<False>>>, Or<PMProject.nonProject, Equal<True>>>>>),
		SubstituteKey = typeof(InventoryItem.inventoryCD), Filterable = true)]
		protected virtual void PMTran_InventoryID_CacheAttached(PXCache sender) { }

		[PXDefault(typeof(Search<InventoryItem.baseUnit, Where<InventoryItem.inventoryID, Equal<Current<PMTran.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PMUnit(typeof(PMTran.inventoryID))]
		protected virtual void PMTran_UOM_CacheAttached(PXCache sender) { }

		#endregion

		[PXHidden]
		public PXSelect<BAccount> dummy;

		public PXSelect<PMRegister, Where<PMRegister.module, Equal<Optional<PMRegister.module>>>> Document;
		public PXSelect<PMTran, 
			Where<PMTran.tranType, Equal<Current<PMRegister.module>>, 
			And<PMTran.refNbr, Equal<Current<PMRegister.refNbr>>>>> Transactions;
		public PXSelect<PMAllocationSourceTran, 
			Where<PMAllocationSourceTran.allocationID, Equal<Required<PMAllocationSourceTran.allocationID>>,
			And<PMAllocationSourceTran.tranID, Equal<Required<PMAllocationSourceTran.tranID>>>>> SourceTran;
		public PXSelect<PMAllocationAuditTran> AuditTrans;
        public PXSelect<ContractDetailAcum> ContractItems;
		public PXSelect<PMTaskAllocTotalAccum> AllocationTotals;
        public PXSetup<PMSetup> Setup;
        public PXSelect<EPActivity> Activities; 

        public RegisterEntry()
        {
            PMSetup setup = PXSelect<PMSetup>.Select(this);

            if (PXAccess.FeatureInstalled<CS.FeaturesSet.projectModule>() && setup == null)
            {
                throw new PXException(Messages.SetupNotConfigured);
            }
        }

       
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


        public PXAction<PMRegister> release;
        [PXUIField(DisplayName = GL.Messages.Release)]
        [PXProcessButton]
        public IEnumerable Release(PXAdapter adapter)
        {
			ReleaseDocument(Document.Current);

			yield return Document.Current;
        }

		public virtual void ReleaseDocument(PMRegister doc)
		{
			if (doc != null && doc.Released != true)
			{
				this.Save.Press();
				PXLongOperation.StartOperation(this, delegate()
				{
					RegisterRelease.Release(doc);
				});
			}
		}

		public PXAction<PMRegister> reverse;
		[PXUIField(DisplayName = Messages.ReverseAllocation)]
		[PXProcessButton(Tooltip=Messages.ReverseAllocationTip)]
		public void Reverse()
		{
			if (Document.Current != null && Document.Current.IsAllocation == true && Document.Current.Released == true)
			{
				PMRegister reversalExist = PXSelect<PMRegister, Where<PMRegister.module, Equal<Current<PMRegister.module>>, And<PMRegister.origRefNbr, Equal<Current<PMRegister.refNbr>>>>>.Select(this);

				if (reversalExist != null)
				{
					throw new PXException(Messages.ReversalExists, reversalExist.RefNbr);
				}

				RegisterEntry target = null;
				List<ProcessInfo<Batch>> infoList;
				using (new PXConnectionScope())
				{
					using (PXTransactionScope ts = new PXTransactionScope())
					{
						target = PXGraph.CreateInstance<RegisterEntry>();
						target.FieldVerifying.AddHandler<PMTran.inventoryID>(SuppressFieldVerifying);
						PMRegister doc = (PMRegister)target.Document.Cache.Insert();
						doc.Description = Document.Current.Description + " Reversal";
						doc.OrigDocType = PMOrigDocType.Reversal;
						doc.OrigDocNbr = Document.Current.RefNbr;
						doc.OrigRefNbr = Document.Current.RefNbr;

						foreach (PMTran tran in Transactions.Select())
						{
							if (tran.IsNonGL == true)
							{
								//debit:
								PMTran debit = new PMTran();
								debit.BranchID = tran.BranchID;
								debit.AccountGroupID = tran.AccountGroupID;
								debit.ProjectID = tran.ProjectID;
								debit.TaskID = tran.TaskID;
								debit.InventoryID = tran.InventoryID;
								debit.Description = tran.Description;
								debit.Date = tran.Date;
								debit.FinPeriodID = tran.FinPeriodID;
								debit.UOM = tran.UOM;
								debit.Qty = -tran.Qty;
								debit.Billable = tran.Billable;
								debit.BillableQty = -tran.BillableQty;
								debit.Amount = -tran.Amount;
								debit.Allocated = true;
								debit.Billed = true;
								debit.OrigTranID = tran.TranID;
								debit.StartDate = tran.StartDate;
								debit.EndDate = tran.EndDate;
								target.Transactions.Insert(debit);

								//credit:
                                //if (tran.OffsetAccountGroupID != null)
                                //{
                                //    PMTran credit = new PMTran();
                                //    credit.BranchID = tran.BranchID;
                                //    credit.AccountGroupID = tran.OffsetAccountGroupID;
                                //    credit.ProjectID = tran.ProjectID;
                                //    credit.TaskID = tran.TaskID;
                                //    credit.InventoryID = tran.InventoryID;
                                //    credit.Description = tran.Description;
                                //    credit.Date = tran.Date;
                                //    credit.FinPeriodID = tran.FinPeriodID;
                                //    credit.UOM = tran.UOM;
                                //    credit.Qty = tran.Qty;
                                //    credit.Billable = tran.Billable;
                                //    credit.BillableQty = tran.BillableQty;
                                //    credit.Amount = tran.Amount;
                                //    credit.Allocated = true;
                                //    credit.Billed = true;
                                //    credit.OrigTranID = tran.TranID;
                                //    credit.StartDate = tran.StartDate;
                                //    credit.EndDate = tran.EndDate;
                                //    target.Transactions.Insert(credit);
                                //}
							}
							else
							{
								PMTran reversal = new PMTran();
								reversal.BranchID = tran.BranchID;
								reversal.ProjectID = tran.ProjectID;
								reversal.TaskID = tran.TaskID;
								reversal.InventoryID = tran.InventoryID;
								reversal.Description = tran.Description;
								reversal.UOM = tran.UOM;
								reversal.Billable = tran.Billable;
								reversal.Allocated = true;
								reversal.Billed = true;
								reversal.Date = tran.Date;
								reversal.FinPeriodID = tran.FinPeriodID;
								reversal.OrigTranID = tran.TranID;
								reversal.StartDate = tran.StartDate;
								reversal.EndDate = tran.EndDate;
								

								if (tran.OffsetAccountID != null)
								{
									Account offsetAccount = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, tran.OffsetAccountID);

									if (offsetAccount == null)
										throw new PXException(Messages.AccountNotFound, tran.OffsetAccountID);

									reversal.AccountGroupID = offsetAccount.AccountGroupID;
									reversal.Qty = tran.Qty;
									reversal.BillableQty = tran.BillableQty;
									reversal.Amount = tran.Amount;
									reversal.AccountID = tran.OffsetAccountID;
									reversal.SubID = tran.OffsetSubID;
									reversal.OffsetAccountID = tran.AccountID;
									reversal.OffsetSubID = tran.SubID;
								}
								else
								{
									//single-sided

									reversal.AccountGroupID = tran.AccountGroupID;
									reversal.Qty = -tran.Qty;
									reversal.BillableQty = -tran.BillableQty;
									reversal.Amount = -tran.Amount;
									reversal.AccountID = tran.AccountID;
									reversal.SubID = tran.SubID;

								}

								target.Transactions.Insert(reversal);
							}
							tran.Billed = true;
							Transactions.Update(tran);
						}

						target.Save.Press();
											
						List<PMRegister> list = new List<PMRegister>();
						list.Add(doc);
						bool releaseSuccess = RegisterRelease.ReleaseWithoutPost(list, false, out infoList);
						if (!releaseSuccess)
						{
							throw new PXException(GL.Messages.DocumentsNotReleased);
						}
												
						Transactions.Cache.AllowUpdate = true;
						foreach (PMTran tran in Transactions.Select())
						{
							UnallocateTran(tran);
						}

						this.Save.Press();
						ts.Complete();
					}

					//Posting should always be performed outside of transaction
					bool postSuccess = RegisterRelease.Post(infoList, false);
					if (!postSuccess)
					{
						throw new PXException(GL.Messages.DocumentsNotPosted);
					}
				}

				target.Document.Current = PXSelect<PMRegister, Where<PMRegister.module, Equal<Current<PMRegister.module>>, And<PMRegister.origRefNbr, Equal<Current<PMRegister.refNbr>>>>>.Select(this);
				throw new PXRedirectRequiredException(target, "Open Reversal");
			}
		}

		public PXAction<PMRegister> viewProject;
        [PXUIField(DisplayName = Messages.ViewProject, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ViewProject(PXAdapter adapter)
        {
            if (Transactions.Current != null)
            {
                var graph = CreateInstance<ProjectEntry>();
                graph.Project.Current = graph.Project.Search<PMProject.contractID>(Transactions.Current.ProjectID);
                throw new PXRedirectRequiredException(graph, true, Messages.ViewProject) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            return adapter.Get();
        }

        public PXAction<PMRegister> viewTask;
        [PXUIField(DisplayName = Messages.ViewTask, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ViewTask(PXAdapter adapter)
        {
            var graph = CreateInstance<ProjectTaskEntry>();
            graph.Task.Current = PXSelect<PMTask, Where<PMTask.taskID, Equal<Current<PMTran.taskID>>>>.Select(this);
            throw new PXRedirectRequiredException(graph, true, Messages.ViewTask) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }

		public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
		}
		

		#region Event Handlers


		protected virtual void PMRegister_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			PMRegister row = e.Row as PMRegister;
			if (row != null)
			{
				if (row.Released != true && row.OrigDocType == PMOrigDocType.Timecard && !string.IsNullOrEmpty(row.OrigDocNbr))
				{ 
					EPTimeCard timeCard = PXSelect<EPTimeCard, Where<EPTimeCard.timeCardCD, Equal<Required<EPTimeCard.timeCardCD>>>>.Select(this, row.OrigDocNbr);
					if (timeCard != null)
					{
						Views.Caches.Add(typeof(EPTimeCard));
						UnreleaseTimeCard(timeCard);
					}
				}
			}
		}

		protected virtual void UnreleaseTimeCard(EPTimeCard timeCard)
		{
			timeCard.IsReleased = false;
			timeCard.Status = EPTimeCard.ApprovedStatus;
			Caches[typeof(EPTimeCard)].Update(timeCard);
		}

		
		protected virtual void PMRegister_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PMRegister row = e.Row as PMRegister;
			if (row != null)
			{
				PXUIFieldAttribute.SetEnabled<PMRegister.date>(sender, row, row.Released != true);
				PXUIFieldAttribute.SetEnabled<PMRegister.description>(sender, row, row.Released != true);
                PXUIFieldAttribute.SetEnabled<PMRegister.status>(sender, row, row.Released != true);
                PXUIFieldAttribute.SetEnabled<PMRegister.hold>(sender, row, row.Released != true);
				
				Document.Cache.AllowUpdate = row.Released != true && row.Module == BatchModule.PM;
				Document.Cache.AllowDelete = row.Released != true && row.Module == BatchModule.PM;
				release.SetEnabled(row.Released != true && row.Hold != true);

				Transactions.Cache.AllowDelete = row.Released != true && row.IsAllocation != true;
				Transactions.Cache.AllowInsert = row.Released != true && row.IsAllocation != true && row.Module == BatchModule.PM;
				Transactions.Cache.AllowUpdate = row.Released != true;

				reverse.SetEnabled(row.Released == true && row.IsAllocation == true);

				PXUIFieldAttribute.SetVisible<PMRegister.origDocType>(sender, row, row.Module == BatchModule.PM);
				PXUIFieldAttribute.SetVisible<PMRegister.origDocNbr>(sender, row, row.Module == BatchModule.PM);

				decimal qty = 0, billableQty = 0, amount = 0;
				if (!this.IsImport)
				{
					//no need to calculate when doing import. It will just slow down the import.

					foreach (PMTran tran in Transactions.Select())
					{
						qty += tran.Qty.GetValueOrDefault();
						billableQty += tran.BillableQty.GetValueOrDefault();
						amount += tran.Amount.GetValueOrDefault();
					}
				}

				row.QtyTotal = qty;
				row.BillableQtyTotal = billableQty;
				row.AmtTotal = amount;
			}
		}

		protected virtual void PMTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PMTran row = e.Row as PMTran;
			if (row != null)
			{
				PXUIFieldAttribute.SetEnabled<PMTran.billableQty>(sender, e.Row, row.Billable == true);
				PXUIFieldAttribute.SetEnabled<PMTran.projectID>(sender, e.Row, row.Allocated != true);
				PXUIFieldAttribute.SetEnabled<PMTran.taskID>(sender, e.Row, row.Allocated != true);
				PXUIFieldAttribute.SetEnabled<PMTran.accountGroupID>(sender, e.Row, row.Allocated != true);
				PXUIFieldAttribute.SetEnabled<PMTran.accountID>(sender, e.Row, row.Allocated != true);
				PXUIFieldAttribute.SetEnabled<PMTran.offsetAccountID>(sender, e.Row, row.Allocated != true);
			}
		}

		protected virtual void PMTran_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			PMTran row = e.Row as PMTran;
			if (row != null)
			{
			    AddAllocatedTotal(row);

				if (row.BillableQty != 0)
				{
					AddUsage(sender, row.ProjectID, row.InventoryID, row.BillableQty, row.UOM);
				}
			}
		}

		protected virtual void PMTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			PMTran row = e.Row as PMTran;
			PMTran oldRow = e.OldRow as PMTran;
			if (row != null && oldRow != null && row.Released != true)
			{
				if (row.Amount == null)
				{
					if ( row.UseBillableQty == true )
						row.Amount = PXCurrencyAttribute.BaseRound(this, row.BillableQty * row.UnitRate);
					else
						row.Amount = PXCurrencyAttribute.BaseRound(this, row.Qty * row.UnitRate);
				}
				else if ( !IsImport && row.Amount == oldRow.Amount &&
					(row.BillableQty != oldRow.BillableQty || row.UnitRate != oldRow.UnitRate ||
					row.Qty != oldRow.Qty || row.UseBillableQty != oldRow.UseBillableQty))
				{
					if (row.UseBillableQty == true)
						row.Amount = PXCurrencyAttribute.BaseRound(this, row.BillableQty * row.UnitRate);
					else
						row.Amount = PXCurrencyAttribute.BaseRound(this, row.Qty * row.UnitRate);
				}

				if (row.Amount != oldRow.Amount || row.BillableQty != oldRow.BillableQty || row.Qty != oldRow.Qty)
				{
					SubtractAllocatedTotal(oldRow);
					AddAllocatedTotal(row);
                }
			}
		}

        protected virtual void PMTran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            PMTran row = e.Row as PMTran;
			UnallocateTran(row);
            UnreleaseActivity(row);
        }

		protected virtual void UnallocateTran(PMTran row)
		{
			if (row != null && row.Allocated == true)
			{
				PXSelectBase<PMAllocationAuditTran> select = new PXSelectJoin<PMAllocationAuditTran,
					InnerJoin<PMTran, On<PMTran.tranID, Equal<PMAllocationAuditTran.sourceTranID>>>,
					Where<PMAllocationAuditTran.tranID, Equal<Required<PMAllocationAuditTran.tranID>>>>(this);

				foreach (PXResult<PMAllocationAuditTran, PMTran> res in select.Select(row.TranID))
				{
					PMAllocationAuditTran aTran = (PMAllocationAuditTran) res;
					PMTran pmTran = (PMTran)res;

					if (!(pmTran.TranType == row.TranType && pmTran.RefNbr == row.RefNbr))
					{
						pmTran.Allocated = false;
						Transactions.Update(pmTran);
					}

					PMAllocationSourceTran ast = SourceTran.Select(aTran.AllocationID, aTran.SourceTranID);
					SourceTran.Delete(ast);
					AuditTrans.Delete(aTran);
				}

				SubtractAllocatedTotal(row);
			}
		}

        protected virtual void UnreleaseActivity(PMTran row)
        {
			if (row.OrigRefID != null && Document.Current != null && Document.Current.IsAllocation != true)
            {
                EPActivity activity = PXSelect<EPActivity, Where<EPActivity.noteID, Equal<Required<EPActivity.noteID>>>>.Select(this, row.OrigRefID);
                if (activity != null)
                {
                    activity.Released = false;
                    activity.EmployeeRate = null;
                    Activities.Update(activity);
                }
            }
        }

		protected virtual void PMTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			PMTran row = e.Row as PMTran;
			if (row != null )
			{
				PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMTran.projectID>>>>.Select(this, row.ProjectID);
				if (project != null && row.AccountGroupID == null && project.BaseType == PMProject.ProjectBaseType.Project && !ProjectDefaultAttribute.IsNonProject(this, project.ContractID))
				{
					sender.RaiseExceptionHandling<PMTran.accountGroupID>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(PMTran.accountGroupID).Name));
				}
			}
		}
				
		protected virtual void PMTran_ResourceID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMTran row = e.Row as PMTran;
			if (row != null)
			{
				sender.SetDefaultExt<PMTran.resourceLocID>(row);
			}
		}

		protected virtual void PMTran_BAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMTran row = e.Row as PMTran;
			if (row != null)
			{
				sender.SetDefaultExt<PMTran.locationID>(row);
			}
		}

        protected virtual void PMTran_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMTran row = e.Row as PMTran;
			if (row != null && string.IsNullOrEmpty(row.Description) && row.InventoryID != null && row.InventoryID != PMProjectStatus.EmptyInventoryID)
			{
				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, row.InventoryID);
				if (item != null)
				{
					row.Description = item.Descr;
				}

                sender.SetDefaultExt<PMTran.uOM>(e.Row);
			}
		}

		protected virtual void PMTran_AccountGroupID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMTran row = e.Row as PMTran;
			if (row != null)
			{
				PMAccountGroup oldGroup = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(this, e.OldValue);
				PMAccountGroup newGroup = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(this, row.AccountGroupID);

				if (oldGroup != null && newGroup != null && oldGroup.Type == newGroup.Type)
				{
					//do not reset inventoryID
				}
				else
				{
					row.InventoryID = PMProjectStatus.EmptyInventoryID;
				}

			}
		}

		protected virtual void PMTran_Qty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMTran row = e.Row as PMTran;
			if (row != null && row.Billable == true)
			{
				sender.SetDefaultExt<PMTran.billableQty>(e.Row);
			}
		}

		protected virtual void PMTran_Billable_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMTran row = e.Row as PMTran;
			if (row != null)
			{
				if (row.Billable == true)
				{
					PXUIFieldAttribute.SetEnabled<PMTran.billableQty>(sender, e.Row, true);
					sender.SetDefaultExt<PMTran.billableQty>(e.Row);
				}
				else
				{
					PXUIFieldAttribute.SetEnabled<PMTran.billableQty>(sender, e.Row, false);
                    sender.SetValueExt<PMTran.billableQty>(e.Row, 0m);
				}
			}
		}
		
        protected virtual void PMTran_BillableQty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            PMTran row = e.Row as PMTran;
            if (row != null && row.BillableQty != 0)
            {
                SubtractUsage(sender, row.ProjectID, row.InventoryID, (decimal?)e.OldValue, row.UOM);
                AddUsage(sender, row.ProjectID, row.InventoryID, row.BillableQty, row.UOM);
            }
        }

        protected virtual void PMTran_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            PMTran row = e.Row as PMTran;
			if (row != null && row.BillableQty != 0)
            {
                SubtractUsage(sender, row.ProjectID, row.InventoryID, row.BillableQty, (string)e.OldValue);
                AddUsage(sender, row.ProjectID, row.InventoryID, row.BillableQty, row.UOM);
            }
        }
		
		protected virtual void PMTran_Date_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMTran row = e.Row as PMTran;
			if (row != null)
			{
				sender.SetDefaultExt<PMTran.finPeriodID>(row);
			}
		}
		
        protected  virtual void PMTran_ResourceID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            PMTran row = e.Row as PMTran;
            if (row != null && e.NewValue != null )
            {
                PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMTran.projectID>>>>.Select(this);
                if ( project != null && project.RestrictToEmployeeList == true)
                {
					EPEmployeeContract rate = PXSelect<EPEmployeeContract, Where<EPEmployeeContract.contractID, Equal<Current<PMTran.projectID>>,
						And<EPEmployeeContract.employeeID, Equal<Required<EPEmployeeContract.employeeID>>>>>.Select(this, e.NewValue);
                    if ( rate == null )
                    {
                    	EPEmployee emp = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.Select(this, e.NewValue);
						if (emp != null)
							e.NewValue = emp.AcctCD;

						throw new PXSetPropertyException(Messages.EmployeeNotInProjectList);
                    }
                }
            }
        }

		protected virtual void PMTran_BillableQty_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PMTran row = e.Row as PMTran;
			if (row != null && row.Billable == true)
			{
				e.NewValue = row.Qty;
			}
		}

       	#endregion

		protected void SuppressFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

        private void AddUsage(PXCache sender, int? contractID, int? inventoryID, decimal? used, string UOM)
        {
            if (contractID != null && inventoryID != null)
            {
                Contract contract = PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(this, contractID);

                if (contract.BaseType == Contract.ContractBaseType.Contract)
                {
					//update all revisions starting from last active
					foreach (ContractDetailExt targetItem in PXSelectJoin<ContractDetailExt,
						InnerJoin<ContractItem, On<ContractItem.contractItemID, Equal<ContractDetailExt.contractItemID>>,
						InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<ContractItem.recurringItemID>>>>,
						Where<ContractDetailExt.contractID, Equal<Required<ContractDetailExt.contractID>>, And<ContractDetailExt.revID, GreaterEqual<Required<ContractDetailExt.revID>>,
						And<ContractItem.recurringItemID, Equal<Required<ContractItem.recurringItemID>>>>>>.Select(this, contractID, contract.LastActiveRevID, inventoryID))
					{
						decimal inTargetUnit = used ?? 0;
						if (!string.IsNullOrEmpty(UOM))
						{
							inTargetUnit = INUnitAttribute.ConvertToBase(sender, inventoryID, UOM, used ?? 0, INPrecision.QUANTITY);
						}

						ContractDetailAcum item = new ContractDetailAcum();
						item.ContractDetailID = targetItem.ContractDetailID;

						item = ContractItems.Insert(item);
						item.Used += inTargetUnit;
						item.UsedTotal += inTargetUnit;
					}
                }
                else
                {
                    ContractDetailEx targetItem = PXSelect<ContractDetailEx,
                    Where<ContractDetailEx.contractID, Equal<Required<ContractDetailEx.contractID>>,
                    And<ContractDetailEx.inventoryID, Equal<Required<ContractDetailEx.inventoryID>>>>>.Select(this, contractID, inventoryID);

					if (targetItem != null)
					{
						decimal inTargetUnit = used ?? 0;
						if (!string.IsNullOrEmpty(UOM))
						{
							inTargetUnit = INUnitAttribute.ConvertToBase(sender, inventoryID, UOM, used ?? 0, INPrecision.QUANTITY);
						}

						ContractDetailAcum item = new ContractDetailAcum();
						item.ContractDetailID = targetItem.ContractDetailID;

						item = ContractItems.Insert(item);
						item.Used += inTargetUnit;
						item.UsedTotal += inTargetUnit;
					}
                }
            }
        }

        private void SubtractUsage(PXCache sender, int? contractID, int? inventoryID, decimal? used, string UOM)
        {
			if ( used != 0 )
				AddUsage(sender, contractID, inventoryID, -used, UOM);
        }

		private void AddAllocatedTotal(PMTran tran)
		{
			if (tran.OrigProjectID != null && tran.OrigTaskID != null && tran.OrigAccountGroupID != null)
			{
				PMTaskAllocTotalAccum tat = new PMTaskAllocTotalAccum();
				tat.ProjectID = tran.OrigProjectID;
				tat.TaskID = tran.OrigTaskID;
				tat.AccountGroupID = tran.OrigAccountGroupID;
				tat.InventoryID = tran.InventoryID;

				tat = AllocationTotals.Insert(tat);
				tat.Amount += tran.Amount;
				tat.Quantity += (tran.Billable == true && tran.UseBillableQty == true) ? tran.BillableQty : tran.Qty;
			}
		}

		private void SubtractAllocatedTotal(PMTran tran)
		{
			if (tran.OrigProjectID != null && tran.OrigTaskID != null && tran.OrigAccountGroupID != null)
			{
				PMTaskAllocTotalAccum tat = new PMTaskAllocTotalAccum();
				tat.ProjectID = tran.OrigProjectID;
				tat.TaskID = tran.OrigTaskID;
				tat.AccountGroupID = tran.OrigAccountGroupID;
				tat.InventoryID = tran.InventoryID;

				tat = AllocationTotals.Insert(tat);
				tat.Amount -= tran.Amount;
				tat.Quantity -= (tran.Billable == true && tran.UseBillableQty == true) ? tran.BillableQty : tran.Qty;
			}
		}

		public virtual PMTran CreateTransaction(EPActivity activity, int? employeeID, DateTime date, int? timeSpent, int? timeBillable, decimal? cost)
		{
			if (activity.UIStatus == ActivityStatusAttribute.Canceled)
				return null;

			if (timeSpent.GetValueOrDefault() == 0 && timeBillable.GetValueOrDefault() == 0)
				return null;
            
			InventoryItem laborItem = PXSelect<InventoryItem, Where<InventoryItem.stkItem, Equal<False>, And<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>>.Select(this, activity.LabourItemID);
			if (laborItem == null)
			{
				PXTrace.WriteError(EP.Messages.InventoryItemIsEmpty);
				throw new PXException(EP.Messages.InventoryItemIsEmpty);
			}

			if (laborItem.InvtAcctID == null)
			{
				PXTrace.WriteError(EP.Messages.ExpenseAccrualIsRequired, laborItem.InventoryCD.Trim());
				throw new PXException(EP.Messages.ExpenseAccrualIsRequired, laborItem.InventoryCD.Trim());
			}

			if (laborItem.InvtSubID == null)
			{
				PXTrace.WriteError(EP.Messages.ExpenseAccrualSubIsRequired, laborItem.InventoryCD.Trim());
				throw new PXException(EP.Messages.ExpenseAccrualSubIsRequired, laborItem.InventoryCD.Trim());
			}

			string ActivityTimeUnit = EPSetup.Minute;
			EPSetup epSetup = PXSetup<EPSetup>.Select(this);
			if (!string.IsNullOrEmpty(epSetup.ActivityTimeUnit))
			{
				ActivityTimeUnit = epSetup.ActivityTimeUnit;
			}
            
			Contract contract = PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(this, activity.ProjectID);

			decimal qty = timeSpent.GetValueOrDefault();
			if (qty > 0 && epSetup.MinBillableTime > qty)
				qty = (decimal)epSetup.MinBillableTime;
			try
			{
			qty = INUnitAttribute.ConvertGlobalUnits(this, ActivityTimeUnit, laborItem.BaseUnit, qty, INPrecision.QUANTITY);
			}
			catch (PXException ex)
			{
				PXTrace.WriteError(ex);
				throw ex;
			}

			decimal bilQty = timeBillable.GetValueOrDefault();
			if (bilQty > 0 && epSetup.MinBillableTime > bilQty)
				bilQty = (decimal)epSetup.MinBillableTime;
			try
			{ 
			bilQty = INUnitAttribute.ConvertGlobalUnits(this, ActivityTimeUnit, laborItem.BaseUnit, bilQty, INPrecision.QUANTITY);
			}
			catch (PXException ex)
			{
				PXTrace.WriteError(ex);
				throw ex;
			}
			int? accountID = laborItem.COGSAcctID;
            int? offsetaccountID = laborItem.InvtAcctID;
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

			if (contract.BaseType == PMProject.ProjectBaseType.Project && contract.NonProject != true )//contract do not record money only usage.
			{

				if (contract.NonProject != true)
				{
					PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(this, activity.ProjectID, activity.ProjectTaskID);

					#region Combine Account and Subaccount

					if (ExpenseAccountSource == PMAccountSource.Project)
					{
						if (contract.DefaultAccountID != null)
						{
							accountID = contract.DefaultAccountID;
							Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
							if (account.AccountGroupID == null)
							{
								throw new PXException(EP.Messages.NoAccountGroupOnProject, account.AccountCD.Trim(), contract.ContractCD.Trim());
							}
							accountGroupID = account.AccountGroupID;
						}
						else
						{
							PXTrace.WriteWarning(EP.Messages.NoDefualtAccountOnProject, contract.ContractCD.Trim());
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
								throw new PXException(EP.Messages.NoAccountGroupOnTask, account.AccountCD.Trim(), contract.ContractCD.Trim(), task.TaskCD.Trim());
							}
							accountGroupID = account.AccountGroupID;
						}
						else
						{
							PXTrace.WriteWarning(EP.Messages.NoDefualtAccountOnTask, contract.ContractCD.Trim(), task.TaskCD.Trim());
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
								throw new PXException(EP.Messages.NoAccountGroupOnEmployee, account.AccountCD, emp.AcctCD.Trim());
							}
							accountGroupID = account.AccountGroupID;
						}
						else
						{
							PXTrace.WriteWarning(EP.Messages.NoExpenseAccountOnEmployee, emp.AcctCD.Trim());
						}
					}
					else
					{
						if (accountID == null)
						{
							PXTrace.WriteError(EP.Messages.NoExpenseAccountOnInventory, laborItem.InventoryCD.Trim());
							throw new PXException(EP.Messages.NoExpenseAccountOnInventory, laborItem.InventoryCD.Trim());
						}

						//defaults to InventoryItem.COGSAcctID
						Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
						if (account.AccountGroupID == null)
						{
							PXTrace.WriteError(EP.Messages.NoAccountGroupOnInventory, account.AccountCD.Trim(), laborItem.InventoryCD.Trim());
							throw new PXException(EP.Messages.NoAccountGroupOnInventory, account.AccountCD.Trim(), laborItem.InventoryCD.Trim());
						}
						accountGroupID = account.AccountGroupID;
					}


					if (accountGroupID == null)
					{
						//defaults to InventoryItem.COGSAcctID
						Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
						if (account.AccountGroupID == null)
						{
							PXTrace.WriteError(EP.Messages.AccountGroupIsNotAssignedForAccount, account.AccountCD.Trim());
							throw new PXException(EP.Messages.AccountGroupIsNotAssignedForAccount, account.AccountCD.Trim());
						}
						accountGroupID = account.AccountGroupID;
					}


					if (!string.IsNullOrEmpty(ExpenseSubMask))
					{
						if (ExpenseSubMask.Contains(PMAccountSource.InventoryItem) && laborItem.COGSSubID == null)
						{
							PXTrace.WriteError(EP.Messages.NoExpenseSubOnInventory, laborItem.InventoryCD.Trim());
							throw new PXException(EP.Messages.NoExpenseSubOnInventory, laborItem.InventoryCD.Trim());
						}
						if (ExpenseSubMask.Contains(PMAccountSource.Project) && contract.DefaultSubID == null)
						{
							PXTrace.WriteError(EP.Messages.NoExpenseSubOnProject, contract.ContractCD.Trim());
							throw new PXException(EP.Messages.NoExpenseSubOnProject, contract.ContractCD.Trim());
						}
						if (ExpenseSubMask.Contains(PMAccountSource.Task) && task.DefaultSubID == null)
						{
							PXTrace.WriteError(EP.Messages.NoExpenseSubOnTask, contract.ContractCD.Trim(), task.TaskCD.Trim());
							throw new PXException(EP.Messages.NoExpenseSubOnTask, contract.ContractCD.Trim(), task.TaskCD.Trim());
						}
						if (ExpenseSubMask.Contains(PMAccountSource.Resource) && emp.ExpenseSubID == null)
						{
							PXTrace.WriteError(EP.Messages.NoExpenseSubOnEmployee, emp.AcctCD.Trim());
							throw new PXException(EP.Messages.NoExpenseSubOnEmployee, emp.AcctCD.Trim());
						}


						subCD = PM.SubAccountMaskAttribute.MakeSub<PMSetup.expenseSubMask>(this, ExpenseSubMask,
							new object[] { laborItem.COGSSubID, contract.DefaultSubID, task.DefaultSubID, emp.ExpenseSubID },
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
							PXTrace.WriteError(EP.Messages.NoAccrualExpenseAccountOnInventory, laborItem.InventoryCD.Trim());
                            throw new PXException(EP.Messages.NoAccrualExpenseAccountOnInventory, laborItem.InventoryCD.Trim());
                        }
                    }

                    if (!string.IsNullOrEmpty(ExpenseAccrualSubMask))
                    {
                        if (ExpenseAccrualSubMask.Contains(PMAccountSource.InventoryItem) && laborItem.InvtSubID == null)
                        {
							PXTrace.WriteError(EP.Messages.NoExpenseAccrualSubOnInventory, laborItem.InventoryCD.Trim());
                            throw new PXException(EP.Messages.NoExpenseAccrualSubOnInventory, laborItem.InventoryCD.Trim());
                        }
                        if (ExpenseAccrualSubMask.Contains(PMAccountSource.Project) && contract.DefaultAccrualSubID == null)
                        {
							PXTrace.WriteError(EP.Messages.NoExpenseAccrualSubOnProject, contract.ContractCD.Trim());
                            throw new PXException(EP.Messages.NoExpenseAccrualSubOnProject, contract.ContractCD.Trim());
                        }
                        if (ExpenseAccrualSubMask.Contains(PMAccountSource.Task) && task.DefaultAccrualSubID == null)
                        {
							PXTrace.WriteError(EP.Messages.NoExpenseAccrualSubOnTask, contract.ContractCD.Trim(), task.TaskCD.Trim());
                            throw new PXException(EP.Messages.NoExpenseAccrualSubOnTask, contract.ContractCD.Trim(), task.TaskCD.Trim());
                        }
						if (ExpenseAccrualSubMask.Contains(PMAccountSource.Resource) && emp.ExpenseSubID == null)
						{
							PXTrace.WriteError(EP.Messages.NoExpenseSubOnEmployee, emp.AcctCD.Trim());
							throw new PXException(EP.Messages.NoExpenseSubOnEmployee, emp.AcctCD.Trim());
						}
						
						offsetSubCD = PM.SubAccountMaskAttribute.MakeSub<PMSetup.expenseAccrualSubMask>(this, ExpenseAccrualSubMask,
							new object[] { laborItem.InvtSubID, contract.DefaultSubID, task.DefaultSubID, emp.ExpenseSubID },
							new Type[] { typeof(InventoryItem.invtSubID), typeof(Contract.defaultAccrualSubID), typeof(PMTask.defaultAccrualSubID), typeof(EPEmployee.expenseSubID) });
                    }

                    #endregion
				}
				else
				{
					//defaults to InventoryItem.COGSAcctID
					Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
					if (account.AccountGroupID == null)
					{
						throw new PXException(EP.Messages.NoAccountGroupOnInventory, account.AccountCD.Trim(), laborItem.InventoryCD.Trim());
					}
					accountGroupID = account.AccountGroupID;
				}
			}

            int? subID = laborItem.COGSSubID;
            int? offsetSubID = laborItem.InvtSubID;
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
			
            //verify that the InventoryItem will be accessable/visible in the selector:
            PMAccountGroup accountGroup = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(this, accountGroupID);
            if (accountGroup != null && accountGroup.Type == AccountType.Income && laborItem.SalesAcctID == null)
            {
                PXTrace.WriteWarning(EP.Messages.NoSalesAccountOnInventory, laborItem.InventoryCD.Trim());
            }
			EmployeeCostEngine costEngine = new EmployeeCostEngine(this);
            PMTran tran = (PMTran)Transactions.Insert();
			tran.BranchID = branchID;
			tran.AccountID = accountID;
			if (string.IsNullOrEmpty(subCD))
				tran.SubID = subID;
            if (string.IsNullOrEmpty(offsetSubCD))
                tran.OffsetSubID = offsetSubID;
            if (contract.BaseType == Contract.ContractBaseType.Contract)
		    {
		        tran.BAccountID = contract.CustomerID;
		        tran.LocationID = contract.LocationID;
		    }
		    tran.AccountGroupID = accountGroupID;
			tran.ProjectID = activity.ProjectID;
			tran.TaskID = activity.ProjectTaskID;
			tran.InventoryID = activity.LabourItemID;
			tran.ResourceID = employeeID;
			tran.Date = date;
			tran.FinPeriodID = FinPeriodIDAttribute.GetPeriod(tran.Date.Value);
			tran.Qty = PXDBQuantityAttribute.Round(this, qty);
			tran.Billable = activity.IsBillable;
			tran.BillableQty = bilQty;
			tran.UOM = laborItem.BaseUnit;
			tran.UnitRate = PXDBPriceCostAttribute.Round(this.Caches<PMTran>(), (decimal)cost);
			tran.Amount = null;
            tran.OffsetAccountID = offsetaccountID;
            tran.IsQtyOnly = contract.BaseType == Contract.ContractBaseType.Contract;
			tran.Description = activity.Subject;
			tran.StartDate = activity.StartDate;
			tran.EndDate = activity.EndDate;
			tran.OrigRefID = activity.NoteID;
			tran.EarningType = activity.EarningTypeID;
			tran.OvertimeMultiplier = costEngine.GetOvertimeMultiplier(activity.EarningTypeID, (int)employeeID, (DateTime)activity.StartDate);
			if (activity.RefNoteID != null)
			{
				Note note = PXSelect<Note, Where<Note.noteID, Equal<Required<Note.noteID>>>>.Select(this, activity.RefNoteID);
				if (note != null && note.EntityType == typeof(CRCase).FullName)
				{
					//Activity associated with the case will be billed (or is already billed) by the Case Billing procedure. 
					tran.Allocated = true;
					tran.Billed = true;
				}
			}
			tran = Transactions.Update(tran);

			if (!string.IsNullOrEmpty(subCD))
				Transactions.SetValueExt<PMTran.subID>(tran, subCD);
            
            if (!string.IsNullOrEmpty(offsetSubCD))
                Transactions.SetValueExt<PMTran.offsetSubID>(tran, offsetSubCD);

			if (epSetup.CopyNotesPM == true)
			{
				string noteText = PXNoteAttribute.GetNote(Caches[typeof (EPActivity)], activity);
                PXNoteAttribute.SetNote(Transactions.Cache, tran, noteText);
			}

			if (epSetup.CopyFilesPM == true)
			{
				Guid[] fileIds = PXNoteAttribute.GetFileNotes(Caches[typeof (EPActivity)], activity);
				if (fileIds != null && fileIds.Length > 0)
                    PXNoteAttribute.SetFileNotes(Transactions.Cache, tran, fileIds);
			}
			return tran;
		}

        public virtual PMTran CreateContractUsage(EPActivity activity)
        {
            if (activity.UIStatus == ActivityStatusAttribute.Canceled)
                return null;

            if (activity.RefNoteID == null)
                return null;

			if (activity.IsBillable != true)
				return null;

			CRCase refCase = PXSelect<CRCase, Where<CRCase.noteID, Equal<Required<CRCase.noteID>>>>.Select(this, activity.RefNoteID);
            
            if (refCase == null)
                throw new Exception(CR.Messages.CaseCannotBeFound);

            CRCaseClass caseClass = PXSelect<CRCaseClass, Where<CRCaseClass.caseClassID, Equal<Required<CRCaseClass.caseClassID>>>>.Select(this, refCase.CaseClassID);

			if (caseClass.PerItemBilling != BillingTypeListAttribute.PerActivity)
                return null;//contract-usage will be created as a result of case release.

            Contract contract = PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(this, refCase.ContractID);
            if (contract == null)
                throw new Exception(CR.Messages.ContractCannotBeFound);

            int? laborItemID = CRCaseClassLaborMatrix.GetLaborClassID(this, caseClass.CaseClassID, activity.EarningTypeID);

            if (laborItemID == null)
                laborItemID = EP.EPContractRate.GetContractLaborClassID(this, activity);

            if (laborItemID == null)
            {
                EP.EPEmployee employeeSettings = PXSelect<EP.EPEmployee, Where<EP.EPEmployee.userID, Equal<Required<EP.EPEmployee.userID>>>>.Select(this, activity.Owner);
                if (employeeSettings != null)
                {
                    laborItemID = EP.EPEmployeeClassLaborMatrix.GetLaborClassID(this, employeeSettings.BAccountID, activity.EarningTypeID) ??
                                  employeeSettings.LabourItemID;
                }
            }

            InventoryItem laborItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, laborItemID);

            if (laborItem == null)
            {
                throw new PXException(CR.Messages.LaborNotConfigured);
                
            }

            int billableMinutes = activity.TimeBillable ?? 0;

			if (caseClass.PerItemBilling == BillingTypeListAttribute.PerActivity && caseClass.RoundingInMinutes > 1)
            {
				decimal fraction = Convert.ToDecimal(billableMinutes) / Convert.ToDecimal(caseClass.RoundingInMinutes);
                int points = Convert.ToInt32(Math.Ceiling(fraction));
				billableMinutes = points * (caseClass.RoundingInMinutes ?? 0);
            }

			if (caseClass.PerItemBilling == BillingTypeListAttribute.PerActivity && caseClass.MinBillTimeInMinutes > 0)
            {
				billableMinutes = Math.Max(billableMinutes, (int)caseClass.MinBillTimeInMinutes);
            }

            if (billableMinutes > 0)
            {
                PMTran newLabourTran = new PMTran();
                newLabourTran.ProjectID = refCase.ContractID;
                newLabourTran.InventoryID = laborItem.InventoryID;
                newLabourTran.AccountGroupID = contract.ContractAccountGroup;
                newLabourTran.OrigRefID = activity.NoteID;
                newLabourTran.BAccountID = refCase.CustomerID;
                newLabourTran.LocationID = refCase.LocationID;
                newLabourTran.Description = activity.Subject;
                newLabourTran.StartDate = activity.StartDate;
                newLabourTran.EndDate = activity.EndDate;
                newLabourTran.Date = activity.EndDate;
                newLabourTran.UOM = laborItem.SalesUnit;
                newLabourTran.Qty = Convert.ToDecimal(TimeSpan.FromMinutes(billableMinutes).TotalHours);
                newLabourTran.BillableQty = newLabourTran.Qty;
                newLabourTran.Released = true;
                newLabourTran.Allocated = true;
                newLabourTran.IsQtyOnly = true;
                newLabourTran.BillingID = contract.BillingID;
                return this.Transactions.Insert(newLabourTran);
            }
            else
            {
                return null;
            }
            
        }

        [Serializable]
		public partial class ContractDetailEx : ContractDetail
		{
			#region ContractDetailID
			public new abstract class contractDetailID : PX.Data.IBqlField
			{
			}

			[PXDBInt()]
			public override Int32? ContractDetailID
			{
				get
				{
					return this._ContractDetailID;
				}
				set
				{
					this._ContractDetailID = value;
				}
			}
			#endregion

			#region ContractID
			public new abstract class contractID : PX.Data.IBqlField
			{
			}
			[PXDBInt(IsKey = true)]
			public override Int32? ContractID
			{
				get
				{
					return this._ContractID;
				}
				set
				{
					this._ContractID = value;
				}
			}
			#endregion
			#region TaskID
			public new abstract class taskID : PX.Data.IBqlField
			{
			}

			[PXDefault(0)]
			[PXDBInt(IsKey = true)]
			public override Int32? TaskID
			{
				get
				{
					return this._TaskID;
				}
				set
				{
					this._TaskID = value;
				}
			}
			#endregion
			#region InventoryID
			public new abstract class inventoryID : PX.Data.IBqlField
			{
			}

			[PXDBInt(IsKey = true)]
			public override Int32? InventoryID
			{
				get
				{
					return this._InventoryID;
				}
				set
				{
					this._InventoryID = value;
				}
			}
			#endregion
		}
	}
}
