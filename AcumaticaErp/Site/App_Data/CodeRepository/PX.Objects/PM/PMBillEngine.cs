using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CT;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.PM
{
	public class PMBillEngine : PXGraph<PMBillEngine>
	{
		#region DAC Attributes Override

		[PXDBInt()]
		protected virtual void ContractDetail_ContractItemID_CacheAttached(PXCache sender) { }

		#endregion

		public PXSelect<PMProject> Project;
		public PXSelect<ContractDetail> ContractDetail;
		public PXSelect<ContractBillingSchedule> BillingSchedule;
		public PXSelect<PMTran> Transactions;

		public bool IncludeTodaysTransactions
		{
			get
			{
				bool result = true;
				PMSetup setup = PXSelect<PMSetup>.Select(this);
				if (setup != null && setup.CutoffDate == PMCutOffDate.Excluded)
				{
					result = false;
				}

				return result;
			}
		}

		public PMBillEngine()
		{
			PXDBDefaultAttribute.SetDefaultForUpdate<ContractBillingSchedule.contractID>(BillingSchedule.Cache, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ContractDetail.contractID>(ContractDetail.Cache, null, false);
		}

		public virtual bool Bill(int? projectID, DateTime? invoiceDate)
		{
			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, projectID);
			ContractBillingSchedule schedule = PXSelect<ContractBillingSchedule>.Search<ContractBillingSchedule.contractID>(this, project.ContractID);

			DateTime billingDate;

			if (invoiceDate == null)
			{
				if (schedule.Type == BillingType.OnDemand)
					billingDate = Accessinfo.BusinessDate ?? DateTime.Now;
				else
				billingDate = schedule.NextDate.Value;
			}
			else
				billingDate = invoiceDate.Value;

			Customer customer = null;
			if (project.CustomerID != null)
			{
				customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, project.CustomerID);
			}
			
			if (customer == null)
				throw new PXException(Messages.NoCustomer);

			List<BillingData> list = new List<BillingData>();
			List<PMTran> reversalTrans = new List<PMTran>();

			PXSelectBase<PMTask> selectTasks = new PXSelect<PMTask,
				Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
				And<PMTask.billingID, IsNotNull,
				And<PMTask.isActive, Equal<True>>>>>(this);

			foreach (PMTask task in selectTasks.Select(project.ContractID))
			{
				if ((task.BillingOption == PMBillingOption.OnTaskCompletion && task.IsCompleted == true) ||
					  (task.BillingOption == PMBillingOption.OnProjectCompetion && project.IsCompleted == true) ||
					  task.BillingOption == PMBillingOption.OnBilling)
				{
					list.AddRange(BillTask(task, billingDate));
					reversalTrans.AddRange(ReverseWipTask(task, billingDate));
				}
			}
			
			//Regroup by Invoices:
			Dictionary<string, List<BillingData>> invoices = new Dictionary<string, List<BillingData>>();
			string emptyInvoiceDescriptionKey = "!@#$%^&";

			foreach (BillingData data in list)
			{
				if (string.IsNullOrEmpty(data.Rule.InvoiceDescription))
				{
					if (invoices.ContainsKey(emptyInvoiceDescriptionKey))
					{
						invoices[emptyInvoiceDescriptionKey].Add(data);
					}
					else
					{
						invoices.Add(emptyInvoiceDescriptionKey, new List<BillingData>(new BillingData[] { data }));
					}
				}
				else
				{
					if (invoices.ContainsKey(data.Rule.InvoiceDescription))
					{
						invoices[data.Rule.InvoiceDescription].Add(data);
					}
					else
					{
						invoices.Add(data.Rule.InvoiceDescription, new List<BillingData>(new BillingData[] { data }));
					}
				}

				//Reverse On Billing:
				
				if ( data.PMTran != null )
				{
					bool reverseOnBilling = false;

					if ( data.PMTran.Reverse == PMReverse.OnBilling)
					{
						reverseOnBilling = true;
					}
					else if ( data.PMTran.Reverse != PMReverse.Never && IsNonGL(data.PMTran) )
					{
						reverseOnBilling = true;
					}
					
					if (reverseOnBilling)
					{
						reversalTrans.AddRange(ReverseTran(data.PMTran));
					}
				}
			}


			//Schedule update:
			schedule.NextDate = GetNextBillingDate(this, schedule, schedule.NextDate);
			schedule.LastDate = this.Accessinfo.BusinessDate;
			BillingSchedule.Update(schedule);


			//ContractDetail update:
			PXSelectBase<ContractDetailExt> cis = new PXSelect<ContractDetailExt, Where<ContractDetailExt.contractID, Equal<Required<ContractDetailExt.contractID>>,
				And<ContractDetailExt.resetUsage, Equal<ResetUsageOption.onBilling>>>>(this);
			foreach (ContractDetailExt ci in cis.Select(project.ContractID))
			{
				ci.Used = 0;
				ContractDetail.Update(ci);
			}
			
			List<ARRegister> doclist = new List<ARRegister>();
			PMRegister reversalDoc = null;

			if (invoices.Count > 0)
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					ARInvoiceEntry invoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
					invoiceEntry.FieldVerifying.AddHandler<ARInvoice.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
					invoiceEntry.FieldVerifying.AddHandler<ARTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
					invoiceEntry.FieldVerifying.AddHandler<ARTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

					foreach (KeyValuePair<string, List<BillingData>> kv in invoices)
					{
						invoiceEntry.Clear(PXClearOption.ClearAll);

						string description = kv.Key == emptyInvoiceDescriptionKey ? null : kv.Key;
						string docDesc = description ?? string.Format(PXMessages.LocalizeNoPrefix(CT.Messages.BillingFor), project.ContractCD, project.Description);
						ARInvoice invoice = (ARInvoice)invoiceEntry.Caches[typeof(ARInvoice)].CreateInstance();
						invoice.DocType = GetDocType(kv.Value);
						int mult = 1;
						if (invoice.DocType == ARDocType.CreditMemo)
							mult = -1;

						invoice = (ARInvoice) invoiceEntry.Caches[typeof (ARInvoice)].Insert(invoice);
						invoiceEntry.Caches[typeof(ARInvoice)].SetValueExt<ARInvoice.customerID>(invoice, customer.AcctCD);
						invoiceEntry.Caches[typeof(ARInvoice)].SetValue<ARInvoice.customerLocationID>(invoice, project.LocationID);
						invoiceEntry.Caches[typeof(ARInvoice)].SetValueExt<ARInvoice.docDate>(invoice, billingDate);
						invoiceEntry.Caches[typeof(ARInvoice)].SetValue<ARInvoice.docDesc>(invoice, docDesc);
						invoiceEntry.Caches[typeof(ARInvoice)].SetValue<ARInvoice.projectID>(invoice, project.ContractID);

						CurrencyInfo curyinfo = (CurrencyInfo)invoiceEntry.Caches[typeof(CurrencyInfo)].Current;
						foreach (BillingData data in kv.Value)
						{
							data.Tran.ExtPrice = data.Tran.ExtPrice.GetValueOrDefault() * mult;
							data.Tran.Qty = data.Tran.Qty.GetValueOrDefault() * mult;
							data.Tran.TranAmt = data.Tran.TranAmt.GetValueOrDefault() * mult;

							decimal curyamount;
							PXDBCurrencyAttribute.CuryConvCury(invoiceEntry.Caches[typeof(CurrencyInfo)], curyinfo, data.Tran.UnitPrice.GetValueOrDefault(), out curyamount);
							data.Tran.CuryUnitPrice = curyamount;
							PXDBCurrencyAttribute.CuryConvCury(invoiceEntry.Caches[typeof(CurrencyInfo)], curyinfo, data.Tran.ExtPrice.GetValueOrDefault(), out curyamount);
							data.Tran.CuryExtPrice = curyamount;
							data.Tran.CuryTranAmt = data.Tran.CuryExtPrice;
							data.Tran.FreezeManualDisc = true;
                            data.Tran.CuryInfoID = curyinfo.CuryInfoID;
							ARTran newTran = (ARTran)invoiceEntry.Caches[typeof(ARTran)].Insert(data.Tran);

							if (data.Tran.TranAmt > newTran.TranAmt)
							{
								newTran.PMDeltaOption = ARTran.pMDeltaOption.CompleteLine;//autocomplete when currency descrepency exists.
							}

							if (data.Tran.AccountID != null)
							{
								ARTran copy = (ARTran)invoiceEntry.Caches[typeof(ARTran)].CreateCopy(newTran);
								copy.AccountID = data.Tran.AccountID;

								copy = (ARTran)invoiceEntry.Caches[typeof(ARTran)].Update(copy);

								if (data.SubCD != null)
									invoiceEntry.Caches[typeof(ARTran)].SetValueExt<ARTran.subID>(copy, data.SubCD);
							}

							if (data.Note != null)
								PXNoteAttribute.SetNote(invoiceEntry.Caches[typeof(ARTran)], newTran, data.Note);
							if (data.Files != null && data.Files.Length > 0)
								PXNoteAttribute.SetFileNotes(invoiceEntry.Caches[typeof(ARTran)], newTran, data.Files);


							//item.RefLineNbr = newTran.LineNbr;
						}

                        ARInvoice oldInvoice = (ARInvoice)invoiceEntry.Caches[typeof(ARInvoice)].CreateCopy(invoice);

                        invoice.CuryOrigDocAmt = invoice.CuryDocBal;
                        invoiceEntry.Caches[typeof(ARInvoice)].RaiseRowUpdated(invoice, oldInvoice);
                        invoiceEntry.Caches[typeof(ARInvoice)].SetValue<ARInvoice.curyOrigDocAmt>(invoice, invoice.CuryDocBal);

						if (project.AutomaticReleaseAR == true)
							invoiceEntry.Caches[typeof(ARInvoice)].SetValueExt<ARInvoice.hold>(invoice, false);

						doclist.Add((ARInvoice)invoiceEntry.Caches[typeof(ARInvoice)].Current);
						invoiceEntry.Save.Press();
					}

					Actions.PressSave();

					if (reversalTrans.Count > 0)
					{
						RegisterEntry pmEntry = PXGraph.CreateInstance<RegisterEntry>();
						pmEntry.FieldVerifying.AddHandler<PMTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });//Project can be completed.
						pmEntry.FieldVerifying.AddHandler<PMTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });//Task can be completed.
						pmEntry.FieldVerifying.AddHandler<PMTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
						reversalDoc = (PMRegister)pmEntry.Document.Cache.Insert();
						reversalDoc.OrigDocType = PMOrigDocType.AllocationReversal;
						reversalDoc.Description = "Allocation Reversal on Billing";
						pmEntry.Document.Current = reversalDoc;

						foreach (PMTran tran in reversalTrans)
						{
							pmEntry.Transactions.Insert(tran);
						}
						pmEntry.Save.Press();
					}

					ts.Complete();
				}

			}
			else
			{
				this.Persist(typeof(ContractBillingSchedule), PXDBOperation.Update);
				this.Persist(typeof(Contract), PXDBOperation.Update);
			}

			if (project.AutomaticReleaseAR == true)
			{
				try
				{
					ARDocumentRelease.ReleaseDoc(doclist, false);
				}
				catch (Exception ex)
				{
					throw new PXException(Messages.AutoReleaseARFailed, ex);
				}

				if (reversalDoc != null)
				{
					try
					{
						RegisterRelease.Release(reversalDoc);
					}
					catch (Exception ex)
					{
						throw new PXException(Messages.AutoReleaseOfReversalFailed, ex);
					}
				}

			}

			return doclist.Count > 0;
		}

		protected  virtual string GetDocType(List<BillingData> value)
		{
			decimal amount = 0;
			foreach (BillingData data in value)
			{
				amount += data.Tran.ExtPrice.GetValueOrDefault();
			}

			if (amount >= 0)
				return ARDocType.Invoice;
			else
				return ARDocType.CreditMemo;
		}


		protected virtual bool IsNonGL(PMTran tran)
		{
			if (tran.IsNonGL == true)
				return true;

			if (tran.AccountID == null && tran.OffsetAccountID == null)
				return true;

			return false;
		}

		public virtual List<BillingData> BillTask(PMTask task, DateTime billingDate)
		{
			List<BillingData> list = new List<BillingData>();
			PXSelectBase<PMBillingRule> billingRuleSelect = new PXSelect<PMBillingRule, Where<PMBillingRule.billingID, Equal<Required<PMBillingRule.billingID>>>>(this);

			foreach (PMBillingRule rule in billingRuleSelect.Select(task.BillingID))
			{
				list.AddRange(BillTask(task, rule, billingDate));
			}

			return list;
		}

		public virtual List<PMTran> ReverseWipTask(PMTask task, DateTime billingDate)
		{
			List<PMTran> list = new List<PMTran>();
			PXSelectBase<PMBillingRule> billingRuleSelect = new PXSelect<PMBillingRule, Where<PMBillingRule.billingID, Equal<Required<PMBillingRule.billingID>>, And<PMBillingRule.wipAccountGroupID, IsNotNull>>>(this);

			foreach (PMBillingRule rule in billingRuleSelect.Select(task.BillingID))
			{
				list.AddRange(ReverseWipTask(task, rule, billingDate));
			}

			return list;
		}

		protected virtual List<PMTran> ReverseWipTask(PMTask task, PMBillingRule rule, DateTime billingDate)
		{
			List<PMTran> list = new List<PMTran>();

			//usage:            
			PXSelectBase<PMTran> select = new PXSelect<PMTran,
				Where<PMTran.projectID, Equal<Required<PMTran.projectID>>,
				And<PMTran.taskID, Equal<Required<PMTran.taskID>>,
				And<PMTran.accountGroupID, Equal<Required<PMTran.accountGroupID>>,
				And<PMTran.date, Less<Required<PMTran.date>>,
				And<PMTran.billed, Equal<False>,
				And<PMTran.released, Equal<True>,
				And<PMTran.reversed, Equal<False>>>>>>>>>(this);

			DateTime cuttofDate = billingDate;//all transactions  excluding the current day.
			ContractBillingSchedule schedule = PXSelect<ContractBillingSchedule>.Search<ContractBillingSchedule.contractID>(this, task.ProjectID);
			if (schedule != null && schedule.Type == BillingType.OnDemand)
			{
				cuttofDate = billingDate.AddDays(1);//all transactions including the current day.
			}
			else
			{
				if (IncludeTodaysTransactions)
				{
					cuttofDate = billingDate.AddDays(1);
				}
			}

			foreach (PMTran tran in select.Select(task.ProjectID, task.TaskID, rule.WipAccountGroupID, cuttofDate))
			{
				list.AddRange(ReverseTran(tran));

				tran.Billed = true;
				tran.BilledDate = billingDate;

				Transactions.Update(tran);
			}

			return list;
		}

		protected virtual List<BillingData> BillTask(PMTask task, PMBillingRule rule, DateTime billingDate)
		{
			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, task.ProjectID);
			Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, project.CustomerID);

			List<BillingData> list = new List<BillingData>();
			Dictionary<int, decimal> availableQty = new Dictionary<int, decimal>();
			Dictionary<int, ContractDetail> billingItems = new Dictionary<int, ContractDetail>();

			//recurent Billing:
			PXSelectBase<ContractDetail> selectBilling = new PXSelect<ContractDetail,
				Where<ContractDetail.contractID, Equal<Required<ContractDetail.contractID>>,
				And<ContractDetail.taskID, Equal<Required<ContractDetail.taskID>>>>>(this);

			foreach (ContractDetail billing in selectBilling.Select(task.ProjectID, task.TaskID))
			{
				billingItems.Add(billing.InventoryID.Value, billing);

				if (billing.Included > 0)
				{
					if (billing.ResetUsage == ResetUsageOption.OnBilling)
					{
						availableQty.Add(billing.InventoryID.Value, billing.Included.Value);
					}
					else
					{
						decimal qtyLeft = billing.Included.Value - billing.LastBilledQty ?? 0;

						if (qtyLeft > 0)
						{
							availableQty.Add(billing.InventoryID.Value, qtyLeft);
						}
					}
				}

				bool bill = false;
				if (billing.ResetUsage == ResetUsageOption.OnBilling)
				{
					bill = true;
				}
				else
				{
					if (billing.LastBilledDate == null)
						bill = true;
				}

				if (bill)
				{
					ARTran arTran = new ARTran();
					arTran.InventoryID = billing.InventoryID;
					arTran.TranDesc = billing.Description;
					arTran.Qty = billing.Included;
					arTran.UOM = billing.UOM;
					arTran.ExtPrice = billing.ItemFee;
					arTran.TranAmt = arTran.ExtPrice;
					arTran.ProjectID = task.ProjectID;
					arTran.TaskID = task.TaskID;
					arTran.Commissionable = false; //todo

					string subCD = null;
					#region Set Account and Subaccount
					if (billing.AccountSource != PMAccountSource.None)
					{

						if (rule.AccountSource == PMAccountSource.RecurringBillingItem)
						{
							if (billing.AccountID != null)
							{
								arTran.AccountID = billing.AccountID;
							}
							else
							{
								InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, billing.InventoryID);
								throw new PXException(Messages.BillingRuleAccountIsNotConfiguredForBillingRecurent, item.InventoryCD);
							}
						}
						else if (billing.AccountSource == PMAccountSource.Project)
						{
							if (project.DefaultAccountID != null)
							{
								arTran.AccountID = project.DefaultAccountID;
							}
							else
							{
								InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, billing.InventoryID);
								throw new PXException(Messages.ProjectAccountIsNotConfiguredForBillingRecurent, item.InventoryCD, project.ContractCD);
							}
						}
						else if (billing.AccountSource == PMAccountSource.Task)
						{
							if (task.DefaultAccountID != null)
							{
								arTran.AccountID = task.DefaultAccountID;
							}
							else
							{
								InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, billing.InventoryID);
								throw new PXException(Messages.TaskAccountIsNotConfiguredForBillingRecurent, item.InventoryCD, project.ContractCD, task.TaskCD);
							}
						}
						else if (billing.AccountSource == PMAccountSource.InventoryItem)
						{
							InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, billing.InventoryID);

							if (item != null)
							{
								if (item.SalesAcctID != null)
								{
									arTran.AccountID = item.SalesAcctID;
								}
								else
								{
									throw new PXException(Messages.InventoryAccountIsNotConfiguredForBillingRecurent, item.InventoryCD);
								}
							}
						}
						else if (billing.AccountSource == PMAccountSource.Customer && customer != null)
						{
							CR.Location customerLoc = PXSelect<CR.Location, Where<CR.Location.bAccountID, Equal<Required<CR.Location.bAccountID>>, And<CR.Location.locationID, Equal<Required<CR.Location.locationID>>>>>.Select(this, customer.BAccountID, customer.DefLocationID);
							if (customerLoc != null)
							{
								if (customerLoc.CSalesAcctID != null)
								{
									arTran.AccountID = customerLoc.CSalesAcctID;
								}
								else
								{
									InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, billing.InventoryID);
									throw new PXException(Messages.CustomerAccountIsNotConfiguredForBillingRecurent, item.InventoryCD, customer.AcctCD);
								}
							}
						}

						if (arTran.AccountID == null && !string.IsNullOrEmpty(billing.SubMask))
						{
							InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, billing.InventoryID);
							throw new PXException(Messages.SubAccountCannotBeComposed, item.InventoryCD);
						}
						else if (arTran.AccountID != null && !string.IsNullOrEmpty(billing.SubMask))
						{
							subCD = PMRecurentBillSubAccountMaskAttribute.MakeSub<PMBillingRule.subMask>(this, billing.SubMask,
								new object[] { billing.SubID, project.DefaultSubID, task.DefaultSubID },
								new Type[] { typeof(PMBillingRule.subID), typeof(PMProject.defaultSubID), typeof(PMTask.defaultSubID) });
						}
					}

					#endregion

					list.Add(new BillingData(arTran, rule, null, subCD, null, null));
					
					billing.LastBilledDate = billingDate;
					ContractDetail.Update(billing);
				}
			}


			
			int mult = 1;
			PMAccountGroup ag = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(this, rule.AccountGroupID);
			if (ag == null)
			{
				throw new PXException(Messages.AccountGroupInBillingRuleNotFound, rule.BillingID, rule.AccountGroupID);
			}
			if (ag.Type == GL.AccountType.Liability || ag.Type == GL.AccountType.Income)
			{
				mult = -1;
			}

			DateTime cuttofDate = billingDate;//all transactions  excluding the current day.
			ContractBillingSchedule schedule = PXSelect<ContractBillingSchedule>.Search<ContractBillingSchedule.contractID>(this, task.ProjectID);
			if (schedule != null && schedule.Type == BillingType.OnDemand)
			{
				cuttofDate = billingDate.AddDays(1);//all transactions including the current day.
			}
			else
			{
				if (IncludeTodaysTransactions)
				{
					cuttofDate = billingDate.AddDays(1);
				}
			}

			List<PMTran> billingBase = SelectBillingBase(task.ProjectID, task.TaskID, rule.AccountGroupID, cuttofDate,
														 rule.IncludeNonBillable == true);

			foreach (PMTran tran in billingBase)
			{
				ARTran arTran = new ARTran();
				arTran.BranchID = tran.BranchID;
				if (tran.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
					arTran.InventoryID = tran.InventoryID;
				arTran.TranDesc = tran.Description;
				arTran.UOM = tran.UOM;
				arTran.Qty = tran.BillableQty * mult;
				arTran.ExtPrice = tran.Amount * mult;
				if (arTran.Qty != 0)
				{
					arTran.UnitPrice = arTran.ExtPrice / arTran.Qty;
				}
				else
					arTran.UnitPrice = 0;
				arTran.TranAmt = arTran.ExtPrice;
				arTran.ProjectID = task.ProjectID;
				arTran.TaskID = task.TaskID;
				arTran.PMTranID = tran.TranID;
				arTran.Commissionable = false; //todo
				arTran.Date = tran.Date;

				string subCD = null;
				#region Set Account and Subaccount

				int? employeeSubID = null;

				if (tran.ResourceID != null)
				{
					EP.EPEmployee emp = PXSelect<EP.EPEmployee, Where<EP.EPEmployee.bAccountID, Equal<Required<EP.EPEmployee.bAccountID>>>>.Select(this, tran.ResourceID);

					if (emp != null)
					{
						employeeSubID = emp.SalesSubID;
					}
				}

				if (rule.AccountSource != PMAccountSource.None)
				{

					if (rule.AccountSource == PMAccountSource.BillingRule)
					{
						if (rule.AccountID != null)
						{
							arTran.AccountID = rule.AccountID;
						}
						else
						{
							throw new PXException(Messages.BillingRuleAccountIsNotConfiguredForBilling, rule.BillingID);
						}
					}
					else if (rule.AccountSource == PMAccountSource.Project)
					{
						if (project.DefaultAccountID != null)
						{
							arTran.AccountID = project.DefaultAccountID;
						}
						else
						{
							throw new PXException(Messages.ProjectAccountIsNotConfiguredForBilling, rule.BillingID, project.ContractCD);
						}
					}
					else if (rule.AccountSource == PMAccountSource.Task)
					{
						if (task.DefaultAccountID != null)
						{
							arTran.AccountID = task.DefaultAccountID;
						}
						else
						{
							throw new PXException(Messages.TaskAccountIsNotConfiguredForBilling, rule.BillingID, project.ContractCD, task.TaskCD);
						}
					}
					else if (rule.AccountSource == PMAccountSource.InventoryItem)
					{
						InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, tran.InventoryID);

						if (item != null)
						{
							if (item.SalesAcctID != null)
							{
								arTran.AccountID = item.SalesAcctID;
							}
							else
							{
								throw new PXException(Messages.InventoryAccountIsNotConfiguredForBilling, rule.BillingID, item.InventoryCD);
							}
						}
					}
					else if (rule.AccountSource == PMAccountSource.Customer && customer != null)
					{
						CR.Location customerLoc = PXSelect<CR.Location, Where<CR.Location.bAccountID, Equal<Required<CR.Location.bAccountID>>, And<CR.Location.locationID, Equal<Required<CR.Location.locationID>>>>>.Select(this, customer.BAccountID, customer.DefLocationID);
						if (customerLoc != null)
						{
							if (customerLoc.CSalesAcctID != null)
							{
								arTran.AccountID = customerLoc.CSalesAcctID;
							}
							else
							{
								throw new PXException(Messages.CustomerAccountIsNotConfiguredForBilling, rule.BillingID, customer.AcctCD);
							}
						}
					}
					else if (rule.AccountSource == PMAccountSource.Resource)
					{
						EP.EPEmployee emp = PXSelect<EP.EPEmployee, Where<EP.EPEmployee.bAccountID, Equal<Required<EP.EPEmployee.bAccountID>>>>.Select(this, tran.ResourceID);

						if (emp != null)
						{
							if (emp.SalesAcctID != null)
							{
								arTran.AccountID = emp.SalesAcctID;
							}
							else
							{
								throw new PXException(Messages.EmployeeAccountIsNotConfiguredForBilling, rule.BillingID, emp.AcctCD);
							}
						}
					}

					if (arTran.AccountID == null && !string.IsNullOrEmpty(rule.SubMask))
					{
						throw new PXException(Messages.SubAccountCannotBeComposed, rule.BillingID);
					}
					else if (arTran.AccountID != null && !string.IsNullOrEmpty(rule.SubMask))
					{
						subCD = PMBillSubAccountMaskAttribute.MakeSub<PMBillingRule.subMask>(this, rule.SubMask,
							new object[] { tran.SubID, rule.SubID, project.DefaultSubID, task.DefaultSubID, employeeSubID },
							new Type[] { typeof(PMTran.subID), typeof(PMBillingRule.subID), typeof(PMProject.defaultSubID), typeof(PMTask.defaultSubID), typeof(EP.EPEmployee.salesSubID) });
					}
				}

				#endregion

				string note = PXNoteAttribute.GetNote(Transactions.Cache, tran);
				Guid[] files = PXNoteAttribute.GetFileNotes(Transactions.Cache, tran);
				list.Add(new BillingData(arTran, rule, tran, subCD, note, files));

				if (billingItems.ContainsKey(tran.InventoryID.Value))
				{
					if (availableQty.ContainsKey(tran.InventoryID.Value))
					{
						decimal available = availableQty[tran.InventoryID.Value];

						if (tran.BillableQty <= available)
						{
							//Transaction is already payed for as a post payment included. Thus it should be free.
							arTran.TranDesc = PXMessages.LocalizeNoPrefix(CT.Messages.PrefixIncludedUsage) + " " + tran.Description;
							availableQty[tran.InventoryID.Value] -= arTran.Qty.Value;//decrease available qty
							arTran.UnitPrice = 0;
							arTran.ExtPrice = 0;
							arTran.TranAmt = 0;
						}
						else
						{
							arTran.TranDesc = PXMessages.LocalizeNoPrefix(CT.Messages.PrefixOverused) + " " + tran.Description;
							arTran.Qty = arTran.Qty - available;
							availableQty[tran.InventoryID.Value] = 0;//all available qty was used.
						}
					}
				}

				tran.Billed = true;
				tran.BilledDate = billingDate;

				Transactions.Update(tran);
			}

			return list;

		}

		public bool IsEmulation { get; set; }

		protected List<PMTran> SelectBillingBase(int? projectID, int? taskID, int? accountGroupID, DateTime? cuttofDate, bool includeNonBillable)
		{
			List<PMTran> list = new List<PMTran>();

			if (IsEmulation)
			{
				foreach (PMTran tran in Transactions.Cache.Cached)
				{
					if ( tran.ProjectID == projectID && tran.TaskID == taskID && tran.AccountGroupID == accountGroupID && tran.Billed != true &&
						tran.Released == true && tran.Reversed != true && tran.Date < cuttofDate && tran.TranType != BatchModule.AR)
						list.Add(tran);
				}
			}
			else
			{
				PXSelectBase<PMTran> select = new PXSelect<PMTran,
					Where<PMTran.projectID, Equal<Required<PMTran.projectID>>,
					And<PMTran.taskID, Equal<Required<PMTran.taskID>>,
					And<PMTran.accountGroupID, Equal<Required<PMTran.accountGroupID>>,
					And<PMTran.date, Less<Required<PMTran.date>>,
					And<PMTran.billed, Equal<False>,
					And<PMTran.released, Equal<True>,
					And<PMTran.reversed, Equal<False>>>>>>>>>(this);

				if (includeNonBillable != true)
				{
					select.WhereAnd<Where<PMTran.billable, Equal<True>>>();
				}

				foreach (PMTran tran in select.Select(projectID, taskID, accountGroupID, cuttofDate))
				{
					list.Add(tran);
				}
			}


			return list;
		}

		public virtual IList<PMTran> ReverseTran(PMTran tran)
		{
			List<PMTran> list = new List<PMTran>();

			if (IsNonGL(tran))
			{
				list.AddRange(ReverseTranNonGL(tran));
			}
			else
			{
				list.Add(ReverseTranGL(tran));
			}

			return list;
		}

		protected virtual PMTran ReverseTranGL(PMTran tran)
		{
			Account offsetAccount = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, tran.OffsetAccountID);

			if (offsetAccount != null && offsetAccount.AccountGroupID != null)
			{
				//Debit-Credit reversal
				PMTran rvrs = PXCache<PMTran>.CreateCopy(tran);
				rvrs.TranID = null;
				rvrs.TranType = null;
				rvrs.RefNbr = null;
				rvrs.RefLineNbr = null;
				rvrs.BatchNbr = null;
				rvrs.TranDate = null;
				rvrs.TranPeriodID = null;
				rvrs.Released = null;
				rvrs.AccountID = tran.OffsetAccountID;
				rvrs.SubID = tran.OffsetSubID;
				rvrs.OffsetAccountID = tran.AccountID;
				rvrs.OffsetSubID = tran.SubID;
				rvrs.AccountGroupID = offsetAccount.AccountGroupID;
				return rvrs;
			}
			else
			{
				//-ve reversal
				PMTran rvrs = PXCache<PMTran>.CreateCopy(tran);
				rvrs.TranID = null;
				rvrs.TranType = null;
				rvrs.RefNbr = null;
				rvrs.RefLineNbr = null;
				rvrs.BatchNbr = null;
				rvrs.TranDate = null;
				rvrs.TranPeriodID = null;
				rvrs.Released = null;
				rvrs.Amount *= -1;
				rvrs.Qty *= -1;
				rvrs.BillableQty *= -1;
				return rvrs;
			}
		}

		protected virtual IList<PMTran> ReverseTranNonGL(PMTran tran)
		{
			List<PMTran> list = new List<PMTran>();

			//debit:
			PMTran debit = new PMTran();
			debit.AccountGroupID = tran.AccountGroupID;
			debit.OffsetAccountGroupID = tran.OffsetAccountGroupID;
			debit.ProjectID = tran.ProjectID;
			debit.Date = tran.Date;
			debit.FinPeriodID = tran.FinPeriodID;
			debit.TaskID = tran.TaskID;
			debit.InventoryID = tran.InventoryID;
			debit.Description = tran.Description;
			debit.UOM = tran.UOM;
			debit.Qty = -tran.Qty;
			debit.Billable = tran.Billable;
			debit.BillableQty = -tran.BillableQty;
			debit.Amount = -tran.Amount;
			debit.Allocated = true;
			debit.Billed = true;
			debit.IsNonGL = true;
			list.Add(debit);

			//credit:
			if (tran.OffsetAccountGroupID != null)
			{
				PMTran credit = new PMTran();
				credit.AccountGroupID = tran.OffsetAccountGroupID;
				credit.ProjectID = tran.ProjectID;
				credit.TaskID = tran.TaskID;
				credit.InventoryID = tran.InventoryID;
				credit.Description = tran.Description;
				credit.Date = tran.Date;
				credit.FinPeriodID = tran.FinPeriodID;
				credit.UOM = tran.UOM;
				credit.Qty = tran.Qty;
				credit.Billable = tran.Billable;
				credit.BillableQty = tran.BillableQty;
				credit.Amount = tran.Amount;
				credit.Allocated = true;
				credit.Billed = true;
				credit.IsNonGL = true;
				list.Add(credit);
			}

			return list;
		}

		public static DateTime? GetNextBillingDate(PXGraph graph, ContractBillingSchedule schedule, DateTime? date)
		{
			switch (schedule.Type)
			{
				case BillingType.Annual:
					return date.Value.AddYears(1);
				case BillingType.Monthly:
                    return date.Value.AddMonths(1);
				case BillingType.Weekly:
                    return date.Value.AddDays(7);
				case BillingType.Quarterly:
                    return date.Value.AddMonths(3);
                case BillingType.OnDemand:
			        return null;
				default:
					throw new ArgumentException("Invalid Schedule type", "schedule");
			}
		}

		
		public class BillingData
		{
			public ARTran Tran { get; private set; }
			public string SubCD { get; private set; }
			public PMBillingRule Rule { get; private set; }
			public PMTran PMTran { get; private set; }
			public string Note { get; private set; }
			public Guid[] Files { get; private set; }

			public BillingData(ARTran tran, PMBillingRule rule, PMTran pmTran, string subCD, string note, Guid[] files)
			{
				this.Tran = tran;
				this.Rule = rule;
				this.PMTran = pmTran;
				this.SubCD = subCD;
				this.Note = note;
				this.Files = files;
			}
		}
	}
}
