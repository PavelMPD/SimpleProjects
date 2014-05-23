using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using System.Collections;
using PX.Objects.IN;
using PX.Objects.AR;
using PX.Objects.GL;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CM;
using System.Diagnostics;

namespace PX.Objects.DR
{
    [Serializable]
	public class DraftScheduleMaint : PXGraph<DraftScheduleMaint, DRSchedule>
	{
		public PXSelect<DRSchedule> Schedule;
		public PXSelect<DRSchedule, Where<DRSchedule.scheduleID, Equal<Current<DRSchedule.scheduleID>>>> DocumentProperties;
		public PXSelect<DRScheduleDetail, Where<DRScheduleDetail.scheduleID, Equal<Current<DRSchedule.scheduleID>>>> Components;
		public PXSelect<DRScheduleTran,
			Where<DRScheduleTran.scheduleID, Equal<Current<DRScheduleDetail.scheduleID>>,
			And<DRScheduleTran.componentID, Equal<Current<DRScheduleDetail.componentID>>,
			And<DRScheduleTran.lineNbr, NotEqual<Current<DRScheduleDetail.creditLineNbr>>>>>> Transactions;
		public PXSelect<DRScheduleTran,
			Where<DRScheduleTran.scheduleID, Equal<Current<DRScheduleDetail.scheduleID>>,
			And<DRScheduleTran.componentID, Equal<Current<DRScheduleDetail.componentID>>,
			And<DRScheduleTran.status, Equal<DRScheduleTranStatus.OpenStatus>,
			And<DRScheduleTran.lineNbr, NotEqual<Current<DRScheduleDetail.creditLineNbr>>>>>>> OpenTransactions;
		public PXSelect<DRScheduleTran,
			Where<DRScheduleTran.scheduleID, Equal<Current<DRScheduleDetail.scheduleID>>,
			And<DRScheduleTran.componentID, Equal<Current<DRScheduleDetail.componentID>>,
			And<DRScheduleTran.status, Equal<DRScheduleTranStatus.ProjectedStatus>,
			And<DRScheduleTran.lineNbr, NotEqual<Current<DRScheduleDetail.creditLineNbr>>>>>>> ProjectedTransactions;
		public PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Current<DRScheduleDetail.defCode>>>> DeferredCode;
		public PXSelect<DRExpenseBalance> ExpenseBalance;
		public PXSelect<DRExpenseProjectionAccum> ExpenseProjection;
		public PXSelect<DRRevenueBalance> RevenueBalance;
		public PXSelect<DRRevenueProjectionAccum> RevenueProjection;
		public PXSelect<DRScheduleEx> Associated;
		
		public DraftScheduleMaint()
		{
		}

		public virtual IEnumerable associated([PXDBInt] int? scheduleID)
		{
			if (scheduleID != null)
			{
				DRSchedule sc = PXSelect<DRSchedule, Where<DRSchedule.scheduleID, Equal<Current<DRScheduleDetail.scheduleID>>>>.Select(this);
				if (sc != null)
				{
					if (sc.Module == BatchModule.AR)
					{
						if (sc.DocType == ARDocType.CreditMemo)
						{
							ARTran arTran = PXSelect<ARTran,
								Where<ARTran.tranType, Equal<Current<DRScheduleEx.docType>>,
								And<ARTran.refNbr, Equal<Current<DRScheduleEx.refNbr>>,
								And<ARTran.lineNbr, Equal<Required<DRSchedule.lineNbr>>>>>>.Select(this, sc.LineNbr);

							if (arTran != null)
							{
								return PXSelect<DRScheduleEx, Where<DRScheduleEx.scheduleID, Equal<Required<DRScheduleEx.scheduleID>>>>.Select(this, arTran.DefScheduleID);
							}
						}
						else if (sc.DocType == ARDocType.Invoice || sc.DocType == ARDocType.DebitMemo)
						{
							List<DRScheduleEx> list = new List<DRScheduleEx>();
							foreach (ARTran arTran in PXSelect<ARTran, Where<ARTran.defScheduleID, Equal<Current<DRScheduleDetail.scheduleID>>>>.Select(this))
							{
								DRScheduleEx relatedSchedule = PXSelect<DRScheduleEx,
																	Where<DRScheduleEx.module, Equal<BatchModule.moduleAR>,
																	And<DRScheduleEx.docType, Equal<Required<ARTran.tranType>>,
																	And<DRScheduleEx.refNbr, Equal<Required<ARTran.refNbr>>,
																	And<DRSchedule.lineNbr, Equal<Required<ARTran.lineNbr>>>>>>>.Select(this, arTran.TranType, arTran.RefNbr, arTran.LineNbr);
								if ( relatedSchedule != null )
									list.Add(relatedSchedule);

							}

							return list;
						}
					}
					else if (sc.Module == BatchModule.AP)
					{
						if (sc.DocType == APDocType.DebitAdj)
						{
							APTran apTran = PXSelect<APTran,
								Where<APTran.tranType, Equal<Current<DRScheduleDetail.docType>>,
								And<APTran.refNbr, Equal<Current<DRScheduleDetail.refNbr>>,
								And<APTran.lineNbr, Equal<Required<DRSchedule.lineNbr>>>>>>.Select(this, sc.LineNbr);

							if (apTran != null)
							{
								return PXSelect<DRScheduleEx, Where<DRScheduleEx.scheduleID, Equal<Required<DRScheduleEx.scheduleID>>>>.Select(this, apTran.DefScheduleID);
							}
						}
						else if (sc.DocType == APDocType.Invoice || sc.DocType == APDocType.CreditAdj)
						{
							List<DRScheduleEx> list = new List<DRScheduleEx>();
							foreach (APTran apTran in PXSelect<APTran, Where<APTran.defScheduleID, Equal<Current<DRScheduleDetail.scheduleID>>>>.Select(this))
							{
								DRScheduleEx relatedSchedule = PXSelect<DRScheduleEx,
																	Where<DRScheduleEx.module, Equal<BatchModule.moduleAP>,
																	And<DRScheduleEx.docType, Equal<Required<APTran.tranType>>,
																	And<DRScheduleEx.refNbr, Equal<Required<APTran.refNbr>>,
																	And<DRSchedule.lineNbr, Equal<Required<APTran.lineNbr>>>>>>>.Select(this, apTran.TranType, apTran.RefNbr, apTran.LineNbr);
								list.Add(relatedSchedule);
							}

							return list;
						}


					}
				}
			}
			return new List<DRScheduleEx>();
		}

		#region Actions/Buttons


		public PXAction<DRSchedule> viewDoc;
		[PXUIField(DisplayName = "View Document")]
		[PXButton]
		public virtual IEnumerable ViewDoc(PXAdapter adapter)
		{
			if (Schedule.Current != null)
			{
				switch (Schedule.Current.Module)
				{
					case BatchModule.AR:
						ARInvoiceEntry arTarget = PXGraph.CreateInstance<ARInvoiceEntry>();
						arTarget.Clear();


						ARInvoice invoice = PXSelect<ARInvoice, Where<ARInvoice.docType, Equal<Current<DRSchedule.docType>>, And<ARInvoice.refNbr, Equal<Current<DRSchedule.refNbr>>>>>.Select(this);
						if (invoice != null)
						{
							arTarget.Document.Current = invoice;
							throw new PXRedirectRequiredException(arTarget, "ViewDocument");
						}
						break;
					case BatchModule.AP:
						APInvoiceEntry apTarget = PXGraph.CreateInstance<APInvoiceEntry>();
						apTarget.Clear();


						APInvoice invoice2 = PXSelect<APInvoice, Where<APInvoice.docType, Equal<Current<DRSchedule.docType>>, And<APInvoice.refNbr, Equal<Current<DRSchedule.refNbr>>>>>.Select(this);
						if (invoice2 != null)
						{
							apTarget.Document.Current = invoice2;
							throw new PXRedirectRequiredException(apTarget, "ViewInvoice");
						}
						break;
				}
			}
			return adapter.Get();
		}

		public PXAction<DRSchedule> viewSchedule;
		[PXUIField(DisplayName = "View Schedule")]
		[PXButton]
		public virtual IEnumerable ViewSchedule(PXAdapter adapter)
		{
			if (Associated.Current != null)
			{
				DraftScheduleMaint target = PXGraph.CreateInstance<DraftScheduleMaint>();
				target.Clear();
				target.Schedule.Current = PXSelect<DRSchedule, Where<DRSchedule.scheduleID, Equal<Required<DRSchedule.scheduleID>>>>.Select(this, Associated.Current.ScheduleID);
				throw new PXRedirectRequiredException(target, "View Referenced Schedule");
			}
			return adapter.Get();
		}

		public PXAction<DRSchedule> viewBatch;
		[PXUIField(DisplayName = "View GL Batch")]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewBatch(PXAdapter adapter)
		{
			JournalEntry target = PXGraph.CreateInstance<JournalEntry>();
			target.Clear();
			Batch batch = PXSelect<Batch, Where<Batch.module, Equal<BatchModule.moduleDR>, And<Batch.batchNbr, Equal<Current<DRScheduleTran.batchNbr>>>>>.Select(this);
			if (batch != null)
			{
				target.BatchModule.Current = batch;
				throw new PXRedirectRequiredException(target, "ViewBatch"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
			}

			return adapter.Get();
		}

		public PXAction<DRSchedule> release;
		[PXUIField(DisplayName = "Release", Enabled = false)]
		[PXButton]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			ReleaseCustomSchedule();

			return adapter.Get();

		}


		public PXAction<DRSchedule> genTran;
		[PXUIField(DisplayName = Messages.GenerateTransactions)]
		[PXButton]
		public virtual IEnumerable GenTran(PXAdapter adapter)
		{
			if (Components.Current != null)
			{
				DRDeferredCode defCode = DeferredCode.Select();
				if (defCode != null)
				{
					PXResultset<DRScheduleTran> res = Transactions.Select();

					if (res.Count > 0)
					{
						WebDialogResult result = Components.View.Ask(Components.Current, GL.Messages.Confirmation, Messages.RegenerateTran, MessageButtons.YesNo, MessageIcon.Question);
						if (result == WebDialogResult.Yes)
						{
							CreateTransactions(defCode, Accessinfo.BranchID);
						}
					}
					else
					{
                        CreateTransactions(defCode, Accessinfo.BranchID);
					}
				}
			}
			return adapter.Get();
		}

		#endregion

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (viewName == "Schedule")
			{
				if (keys["ScheduleID"] == null)
				{
					foreach (DRSchedule sc in Schedule.Cache.Inserted)
					{
						keys["ScheduleID"] = sc.ScheduleID;
					}
				}
			}

			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		public override void Persist()
		{
			RunVerification();

			base.Persist();
		}

		#region Event Handlers

		protected virtual void DRSchedule_RefNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRSchedule row = e.Row as DRSchedule;
			if (row != null)
			{
				if (row.Module == BatchModule.AR)
				{
					ARInvoice invoice = PXSelect<ARInvoice, Where<ARInvoice.docType, Equal<Current<DRSchedule.docType>>, And<ARInvoice.refNbr, Equal<Current<DRSchedule.refNbr>>>>>.Select(this);
					if (invoice != null)
					{
						object oldRow = sender.CreateCopy(row);
						row.BAccountID = invoice.CustomerID;
						sender.RaiseFieldUpdated<DRSchedule.bAccountID>(row, oldRow);
					}


				}
				else if (row.Module == BatchModule.AP)
				{
					APInvoice bill = PXSelect<APInvoice, Where<APInvoice.docType, Equal<Current<DRSchedule.docType>>, And<APInvoice.refNbr, Equal<Current<DRSchedule.refNbr>>>>>.Select(this);
					if (bill != null)
					{
						object oldRow = sender.CreateCopy(row);
						row.BAccountID = bill.VendorID;
						sender.RaiseFieldUpdated<DRSchedule.bAccountID>(row, oldRow);
					}

				}
			}
		}

		protected virtual void DRSchedule_RefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			DRSchedule row = e.Row as DRSchedule;
			if (row != null)
			{
				e.Cancel = true;
			}
		}

		protected virtual void DRSchedule_BAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<DRSchedule.bAccountLocID>(e.Row);
		}

		protected virtual void DRSchedule_DocDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			DRSchedule row = e.Row as DRSchedule;
			if (row != null)
			{
				e.NewValue = Accessinfo.BusinessDate;
			}
		}

		protected virtual void DRSchedule_DocumentType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRSchedule row = e.Row as DRSchedule;
			if (row != null)
			{
				string newModule = DRScheduleDocumentType.ExtractModule(row.DocumentType);
				row.DocType = DRScheduleDocumentType.ExtractDocType(row.DocumentType);
				row.Module = newModule;

                if (row.Module == BatchModule.AR)
                {
                    row.BAccountType = CR.BAccountType.CustomerType;
                }
                else if (row.Module == BatchModule.AP)
                {
                    row.BAccountType = CR.BAccountType.VendorType;
                }
                
			}
		}

		protected virtual void DRSchedule_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			DRSchedule row = e.Row as DRSchedule;
			if (row != null)
			{
				if (string.IsNullOrEmpty(row.Module))
				{
					Components.Cache.RaiseExceptionHandling<DRScheduleDetail.documentType>(Components.Current, null, new PXSetPropertyException(Data.ErrorMessages.FieldIsEmpty, typeof(DRScheduleDetail.documentType).Name));
				}

				if (string.IsNullOrEmpty(row.FinPeriodID))
				{
					Components.Cache.RaiseExceptionHandling<DRScheduleDetail.finPeriodID>(Components.Current, null, new PXSetPropertyException(Data.ErrorMessages.FieldIsEmpty, typeof(DRScheduleDetail.finPeriodID).Name));
				}
			}
		}

		protected virtual void DRSchedule_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			DRSchedule row = e.Row as DRSchedule;
			if (row != null)
			{
				row.IsCustom = true;
				row.IsDraft = true;
			}
		}

		protected virtual void DRSchedule_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			DRSchedule row = e.Row as DRSchedule;

			if (row != null)
			{
				row.DocumentType = DRScheduleDocumentType.BuildDocumentType(row.Module, row.DocType);

				if (row.Module == BatchModule.AR)
				{
                    row.BAccountType = CR.BAccountType.CustomerType;

					ARTran tran = PXSelect<ARTran, Where<ARTran.tranType, Equal<Current<DRSchedule.docType>>,
						And<ARTran.refNbr, Equal<Current<DRSchedule.refNbr>>,
						And<ARTran.lineNbr, Equal<Current<DRSchedule.lineNbr>>>>>>.Select(this);

					if (tran != null)
					{
						row.OrigLineAmt = tran.TranAmt;
					}
				}
				else
				{
                    row.BAccountType = CR.BAccountType.VendorType;

					APTran tran = PXSelect<APTran, Where<APTran.tranType, Equal<Current<DRSchedule.docType>>,
						And<APTran.refNbr, Equal<Current<DRSchedule.refNbr>>,
						And<APTran.lineNbr, Equal<Current<DRSchedule.lineNbr>>>>>>.Select(this);

					if (tran != null)
					{
						row.OrigLineAmt = tran.TranAmt;
					}
				}

				release.SetVisible(row.IsCustom == true);
				release.SetEnabled(row.IsCustom == true);
				PXUIFieldAttribute.SetEnabled<DRSchedule.documentType>(sender, row, row.IsCustom == true);
				PXUIFieldAttribute.SetEnabled<DRSchedule.finPeriodID>(sender, row, row.IsCustom == true);
				PXUIFieldAttribute.SetEnabled<DRSchedule.refNbr>(sender, row, row.IsCustom == true);
				PXUIFieldAttribute.SetEnabled<DRSchedule.lineNbr>(sender, row, row.IsCustom == true);
				PXUIFieldAttribute.SetEnabled<DRSchedule.docDate>(sender, row, row.IsCustom == true);
				PXUIFieldAttribute.SetEnabled<DRSchedule.bAccountID>(sender, row, row.IsCustom == true);
				PXUIFieldAttribute.SetEnabled<DRSchedule.bAccountLocID>(sender, row, row.IsCustom == true);
				PXUIFieldAttribute.SetEnabled<DRSchedule.projectID>(sender, row, row.IsCustom == true);
				PXUIFieldAttribute.SetEnabled<DRSchedule.taskID>(sender, row, row.IsCustom == true);
				
				PXUIFieldAttribute.SetVisible<DRSchedule.origLineAmt>(sender, row, row.IsCustom != true);

				Components.Cache.AllowInsert = row.IsDraft == true;
				Components.Cache.AllowUpdate = row.IsDraft == true;
				Components.Cache.AllowDelete = row.IsDraft == true;
			}
		}

		protected virtual void DRSchedule_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			DRSchedule row = e.Row as DRSchedule;
			if (!sender.ObjectsEqual<DRSchedule.documentType, DRSchedule.refNbr, DRSchedule.lineNbr, DRSchedule.bAccountID, DRSchedule.finPeriodID, DRSchedule.docDate>(e.Row, e.OldRow))
			{
				foreach (DRScheduleDetail detail in Components.Select())
				{
					SyncProperties(row, detail);

					Components.Update(detail);
				}
			}
		}

	    protected virtual void DRSchedule_LineNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
	    {
		    DRSchedule row = e.Row as DRSchedule;
		    if (row == null) return;

		    DRSchedule duplicate = PXSelect<DRSchedule, Where<DRSchedule.docType, Equal<Current<DRSchedule.docType>>,
			    And<DRSchedule.refNbr, Equal<Current<DRSchedule.refNbr>>,
				    And<DRSchedule.lineNbr, Equal<Required<DRSchedule.lineNbr>>,
					    And<DRSchedule.scheduleID, NotEqual<Current<DRSchedule.scheduleID>>>>>>>.Select(this, e.NewValue);

		    if (duplicate != null )
		    {
				sender.RaiseExceptionHandling<DRSchedule.lineNbr>(row, e.NewValue, new PXSetPropertyException(Messages.DuplicateSchedule, duplicate.ScheduleID));
		    }
			
	    }

		private static void SyncProperties(DRSchedule row, DRScheduleDetail detail)
		{
			detail.Module = row.Module;
			detail.DocumentType = row.DocumentType;
			detail.DocType = row.DocType;
			detail.RefNbr = row.RefNbr;
			detail.LineNbr = row.LineNbr;
			detail.BAccountID = row.BAccountID;
			detail.FinPeriodID = row.FinPeriodID;
			detail.DocDate = row.DocDate;
		}

		
		protected virtual void DRScheduleDetail_DocumentType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;
			if (row != null)
			{
				string newModule = DRScheduleDocumentType.ExtractModule(row.DocumentType);
				row.DocType = DRScheduleDocumentType.ExtractDocType(row.DocumentType);

				if (row.Module != newModule)
				{
					row.Module = newModule;
					row.DefCode = null;
					row.DefAcctID = null;
					row.DefSubID = null;
					row.BAccountID = null;
					row.AccountID = null;
					row.SubID = null;

					InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<DRScheduleDetail.componentID>>>>.Select(this, row.ComponentID);
					if (item != null)
					{
						row.AccountID = row.Module == BatchModule.AP ? item.COGSAcctID : item.SalesAcctID;
						row.SubID = row.Module == BatchModule.AP ? item.COGSSubID : item.SalesSubID;

					}
				}

				row.RefNbr = null;
			}
		}

		protected virtual void DRScheduleDetail_TotalAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;
			if (row != null && row.Status == DRScheduleStatus.Draft)
			{
				row.DefAmt = row.TotalAmt;
			}
		}

		protected virtual void DRScheduleDetail_DefCode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;
			if (row != null)
			{
				DRDeferredCode defCode = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Required<DRScheduleDetail.defCode>>>>.Select(this, row.DefCode);

				if (defCode != null)
				{
					row.DefCode = defCode.DeferredCodeID;
					row.DefAcctID = defCode.AccountID;
					row.DefSubID = defCode.SubID;
				}
			}
		}

		protected virtual void DRScheduleDetail_DocDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;
			if (row != null)
			{
				e.NewValue = Accessinfo.BusinessDate;
			}
		}

        protected virtual void DRScheduleDetail_DefCodeType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (Schedule.Current != null)
            {
                e.NewValue = Schedule.Current.Module == BatchModule.AP? DeferredAccountType.Expense: DeferredAccountType.Income;
                e.Cancel = true;
            }
        }

		protected virtual void DRScheduleDetail_BAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;
			if (row != null)
			{
				if (row.ComponentID == null || row.ComponentID == DRScheduleDetail.EmptyComponentID || row.AccountID == null)
				{
					switch (row.Module)
					{
						case BatchModule.AP:
							PXResultset<Vendor> res = PXSelectJoin<Vendor,
								InnerJoin<Location, On<Vendor.bAccountID, Equal<Location.bAccountID>,
									And<Vendor.defLocationID, Equal<Location.locationID>>>>,
								Where<Vendor.bAccountID, Equal<Current<DRScheduleDetail.bAccountID>>>>.Select(this);

							Location loc = (Location)res[0][1];

							if (loc.VExpenseAcctID != null)
							{
								row.AccountID = loc.VExpenseAcctID;
								row.SubID = loc.VExpenseSubID;
							}
							break;
						case BatchModule.AR:
							PXResultset<Customer> res2 = PXSelectJoin<Customer,
								InnerJoin<Location, On<Customer.bAccountID, Equal<Customer.bAccountID>,
									And<Customer.defLocationID, Equal<Location.locationID>>>>,
								Where<Customer.bAccountID, Equal<Current<DRScheduleDetail.bAccountID>>>>.Select(this);

							Location loc2 = (Location)res2[0][1];

							if (loc2.CSalesAcctID != null)
							{
								row.AccountID = loc2.CSalesAcctID;
								row.SubID = loc2.CSalesSubID;
							}
							break;
					}
				}
			}
		}

		protected virtual void DRScheduleDetail_ScheduleID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;
			if (row != null)
			{
				e.Cancel = true;
			}
		}
				
		protected virtual void DRScheduleDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;
			if (row != null && Schedule.Current != null)
			{
				row.Status = DRScheduleStatus.Draft;
				row.IsCustom = true;
				row.ScheduleID = Schedule.Current.ScheduleID;
				SyncProperties(Schedule.Current, row);


				if (row.ComponentID == null)
				{
					row.ComponentID = DRScheduleDetail.EmptyComponentID;
				}
			}

			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<DRScheduleDetail.componentID>>>>.Select(this, row.ComponentID);
			if (item != null)
			{
				row.AccountID = row.Module == BatchModule.AP ? item.COGSAcctID : item.SalesAcctID;
				row.SubID = row.Module == BatchModule.AP ? item.COGSSubID : item.SalesSubID;

				DRDeferredCode defCode = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Required<DRScheduleDetail.defCode>>>>.Select(this, item.DeferredCode);

				if (defCode != null)
				{
					row.DefCode = defCode.DeferredCodeID;
					row.DefAcctID = defCode.AccountID;
					row.DefSubID = defCode.SubID;
				}
			}


		}

		protected virtual void DRScheduleDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;

			if (row != null)
			{
				Delete.SetEnabled(false);
				row.DocumentType = DRScheduleDocumentType.BuildDocumentType(row.Module, row.DocType);

				release.SetVisible(row.IsCustom == true);
				release.SetEnabled(false);

				row.DefTotal = SumOpenAndProjectedTransactions(row);
				PXUIFieldAttribute.SetEnabled<DRScheduleDetail.componentID>(sender, row, row.ComponentID != DRScheduleDetail.EmptyComponentID);
				if (row.Status == DRScheduleStatus.Draft)
				{
					release.SetEnabled(true);
					Delete.SetEnabled(true);
				}
				

								
				Transactions.Cache.AllowInsert = row.Status != DRScheduleStatus.Closed;
				Transactions.Cache.AllowUpdate = row.Status != DRScheduleStatus.Closed;
				Transactions.Cache.AllowDelete = row.Status != DRScheduleStatus.Closed;
			}
		}
		
		protected virtual void DRScheduleTran_RecDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRScheduleTran row = e.Row as DRScheduleTran;
			if (row != null)
			{
				row.FinPeriodID = FinPeriodIDAttribute.PeriodFromDate(this, row.RecDate);
			}
		}

		protected virtual void DRScheduleTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			DRScheduleTran row = e.Row as DRScheduleTran;
			if (row != null)
			{
				PXUIFieldAttribute.SetEnabled<DRScheduleTran.recDate>(sender, row, row.Status == DRScheduleTranStatus.Open || row.Status == DRScheduleTranStatus.Projected);
				PXUIFieldAttribute.SetEnabled<DRScheduleTran.amount>(sender, row, row.Status == DRScheduleTranStatus.Open || row.Status == DRScheduleTranStatus.Projected);
				PXUIFieldAttribute.SetEnabled<DRScheduleTran.accountID>(sender, row, row.Status == DRScheduleTranStatus.Open || row.Status == DRScheduleTranStatus.Projected);
				PXUIFieldAttribute.SetEnabled<DRScheduleTran.subID>(sender, row, row.Status == DRScheduleTranStatus.Open || row.Status == DRScheduleTranStatus.Projected);
			}
		}

		protected virtual void DRScheduleTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			DRScheduleTran row = e.Row as DRScheduleTran;
			if (!sender.ObjectsEqual<DRScheduleTran.finPeriodID, DRScheduleTran.accountID, DRScheduleTran.subID, DRScheduleTran.amount>(e.Row, e.OldRow))
			{
				if (Components.Current != null && Components.Current.Status == DRScheduleStatus.Open)
				{
					DRScheduleTran oldRow = (DRScheduleTran)e.OldRow;
					Subtract(oldRow);
					Add(row);
				}
			}
		}

		protected virtual void DRScheduleTran_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			DRScheduleTran row = e.Row as DRScheduleTran;
			if (row != null && Components.Current != null && Components.Current.Status == DRScheduleStatus.Open)
			{
				Add(row);
			}
		}

		protected virtual void DRScheduleTran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			DRScheduleTran row = e.Row as DRScheduleTran;
			if (row != null && Components.Current != null && Components.Current.Status == DRScheduleStatus.Open)
			{
				Subtract(row);
			}
		}


		#endregion

		private void RunVerification()
		{
			if (Schedule.Current != null && Schedule.Current.RefNbr != null && Schedule.Current.OrigLineAmt.GetValueOrDefault() != 0)
			{
				decimal total = 0;
				foreach (DRScheduleDetail sd in Components.Select())
				{
					total += sd.TotalAmt ?? 0;

					DRDeferredCode dc = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Current<DRScheduleDetail.defCode>>>>.Select(this);
					bool checkTotal = true;

					if (dc != null && dc.Method == DeferredMethodType.CashReceipt)
					{
						checkTotal = false;
					}

					if (checkTotal)
					{
						decimal defTotal = SumOpenAndProjectedTransactions(sd);
						if (defTotal != sd.DefAmt)
						{
							if (Components.Cache.RaiseExceptionHandling<DRScheduleDetail.defTotal>(sd, defTotal, new PXSetPropertyException(Messages.DeferredAmountSumError)))
							{
								throw new PXRowPersistingException(typeof(DRScheduleDetail.defTotal).Name, defTotal, Messages.DeferredAmountSumError);
							}
						}
					}
				}

				if (total != Schedule.Current.OrigLineAmt)
				{
					throw new PXException(Messages.SumOfComponentsError);
				}

			}
		}

		private void CreateTransactions(DRDeferredCode defCode, int? branchID)
		{
			if (Components.Current != null && Components.Current.Status == DRScheduleStatus.Draft)
			{
				foreach (DRScheduleTran tran in Transactions.Select())
				{
					Transactions.Delete(tran);
				}

				if (Schedule.Current != null && Components.Current != null && defCode != null)
				{
					IList<DRScheduleTran> tranList = DRProcess.GenerateTransactions(this, Schedule.Current, Components.Current, defCode, branchID);

					foreach (DRScheduleTran tran in tranList)
					{
						Transactions.Insert(tran);
					}
				}
			}
		}

		private void ReleaseCustomSchedule()
		{
			foreach ( DRScheduleDetail detail in Components.Select() )
			{
				ScheduleMaint maint = PXGraph.CreateInstance<ScheduleMaint>();
				maint.Clear();
				maint.Document.Current = detail;

				maint.ReleaseCustomSchedule();

			}
		}

		private void Subtract(DRScheduleTran tran)
		{
			Debug.Print("Subtract FinPeriod={0} Status={1} Amount={2}", tran.FinPeriodID, tran.Status, tran.Amount);
			DRDeferredCode code = DeferredCode.Select();

			if (code.AccountType == DeferredAccountType.Expense)
			{
				SubtractExpenseFromProjection(tran);
				SubtractExpenseFromBalance(tran);
			}
			else
			{
				SubtractRevenueFromProjection(tran);
				SubtractRevenueFromBalance(tran);
			}
		}

		private void SubtractRevenueFromProjection(DRScheduleTran tran)
		{
			if (tran.FinPeriodID == null) return;
			DRRevenueProjectionAccum hist = new DRRevenueProjectionAccum();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = Components.Current.AccountID;
			hist.SubID = Components.Current.SubID;
			hist.ComponentID = Components.Current.ComponentID ?? 0;
			hist.ProjectID = Components.Current.ProjectID ?? 0;
			hist.CustomerID = Components.Current.BAccountID ?? 0;

			hist = RevenueProjection.Insert(hist);
			hist.PTDProjected -= tran.Amount;
		}

		private void SubtractExpenseFromProjection(DRScheduleTran tran)
		{
			if (tran.FinPeriodID == null) return;
			DRExpenseProjectionAccum hist = new DRExpenseProjectionAccum();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = Components.Current.AccountID;
			hist.SubID = Components.Current.SubID;
			hist.ComponentID = Components.Current.ComponentID ?? 0;
			hist.ProjectID = Components.Current.ProjectID ?? 0;
			hist.VendorID = Components.Current.BAccountID ?? 0;

			hist = ExpenseProjection.Insert(hist);
			hist.PTDProjected -= tran.Amount;
		}

		private void SubtractRevenueFromBalance(DRScheduleTran tran)
		{
			if (tran.FinPeriodID == null) return;
			DRRevenueBalance hist = new DRRevenueBalance();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = Components.Current.DefAcctID;
			hist.SubID = Components.Current.DefSubID;
			hist.ComponentID = Components.Current.ComponentID ?? 0;
			hist.ProjectID = Components.Current.ProjectID ?? 0;
			hist.CustomerID = Components.Current.BAccountID ?? 0;

			hist = RevenueBalance.Insert(hist);
			hist.PTDProjected -= tran.Amount;
			hist.EndProjected += tran.Amount;
		}

		private void SubtractExpenseFromBalance(DRScheduleTran tran)
		{
			if (tran.FinPeriodID == null) return;
			DRExpenseBalance hist = new DRExpenseBalance();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = Components.Current.DefAcctID;
			hist.SubID = Components.Current.DefSubID;
			hist.ComponentID = Components.Current.ComponentID ?? 0;
			hist.ProjectID = Components.Current.ProjectID ?? 0;
			hist.VendorID = Components.Current.BAccountID ?? 0;

			hist = ExpenseBalance.Insert(hist);
			hist.PTDProjected -= tran.Amount;
			hist.EndProjected += tran.Amount;
		}

		private void Add(DRScheduleTran tran)
		{
			Debug.Print("Add FinPeriod={0} Status={1} Amount={2}", tran.FinPeriodID, tran.Status, tran.Amount);
			DRDeferredCode code = DeferredCode.Select();

			if (code.AccountType == DeferredAccountType.Expense)
			{
				AddExpenseToProjection(tran);
				AddExpenseToBalance(tran);
			}
			else
			{
				AddRevenueToProjection(tran);
				AddRevenueToBalance(tran);
			}
		}

		private void AddRevenueToProjection(DRScheduleTran tran)
		{
			if (tran.FinPeriodID == null) return;
			DRRevenueProjectionAccum hist = new DRRevenueProjectionAccum();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = Components.Current.AccountID;
			hist.SubID = Components.Current.SubID;
			hist.ComponentID = Components.Current.ComponentID ?? 0;
			hist.ProjectID = Components.Current.ProjectID ?? 0;
			hist.CustomerID = Components.Current.BAccountID ?? 0;

			hist = RevenueProjection.Insert(hist);
			hist.PTDProjected += tran.Amount;
		}

		private void AddExpenseToProjection(DRScheduleTran tran)
		{
			if (tran.FinPeriodID == null) return;
			DRExpenseProjectionAccum hist = new DRExpenseProjectionAccum();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = Components.Current.AccountID;
			hist.SubID = Components.Current.SubID;
			hist.ComponentID = Components.Current.ComponentID ?? 0;
			hist.ProjectID = Components.Current.ProjectID ?? 0;
			hist.VendorID = Components.Current.BAccountID ?? 0;

			hist = ExpenseProjection.Insert(hist);
			hist.PTDProjected += tran.Amount;
		}

		private void AddRevenueToBalance(DRScheduleTran tran)
		{
			if (tran.FinPeriodID == null) return;
			DRRevenueBalance hist = new DRRevenueBalance();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = Components.Current.DefAcctID;
			hist.SubID = Components.Current.DefSubID;
			hist.ComponentID = Components.Current.ComponentID ?? 0;
			hist.ProjectID = Components.Current.ProjectID ?? 0;
			hist.CustomerID = Components.Current.BAccountID ?? 0;

			hist = RevenueBalance.Insert(hist);
			hist.PTDProjected += tran.Amount;
			hist.EndProjected -= tran.Amount;
		}

		private void AddExpenseToBalance(DRScheduleTran tran)
		{
			if (tran.FinPeriodID == null) return;
			DRExpenseBalance hist = new DRExpenseBalance();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = Components.Current.DefAcctID;
			hist.SubID = Components.Current.DefSubID;
			hist.ComponentID = Components.Current.ComponentID ?? 0;
			hist.ProjectID = Components.Current.ProjectID ?? 0;
			hist.VendorID = Components.Current.BAccountID ?? 0;

			hist = ExpenseBalance.Insert(hist);
			hist.PTDProjected += tran.Amount;
			hist.EndProjected -= tran.Amount;
		}

		private decimal SumOpenAndProjectedTransactions(DRScheduleDetail row)
		{
			decimal total = 0;
			foreach (DRScheduleTran tran in OpenTransactions.View.SelectMultiBound(new object[]{row}))
			{
				total += tran.Amount.Value;
			}

			foreach (DRScheduleTran tran in ProjectedTransactions.View.SelectMultiBound(new object[] { row }))
			{
				total += tran.Amount.Value;
			}

			return total;
		}

        [PXCacheName(Messages.DRScheduleEx)]
        [Serializable]
		public partial class DRScheduleEx : DRSchedule
		{
			#region ScheduleID
			public new abstract class scheduleID : PX.Data.IBqlField
			{
			}
			[PXDBInt(IsKey = true)]
			[PXUIField(DisplayName = "Schedule ID")]
			public override Int32? ScheduleID
			{
				get
				{
					return this._ScheduleID;
				}
				set
				{
					this._ScheduleID = value;
				}
			}
			#endregion
		}
	}
}
