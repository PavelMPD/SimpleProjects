using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.CM;
using System.Diagnostics;
using PX.Objects.AP;

namespace PX.Objects.DR
{
	public class DRProcess : PXGraph<DRProcess>
	{		
		public PXSelect<DRSchedule> Schedule;
		public PXSelect<DRScheduleDetail> ScheduleDetail;
		public PXSelect<DRScheduleTran> Transactions;
		public PXSelect<DRExpenseBalance> ExpenseBalance;
		public PXSelect<DRExpenseProjectionAccum> ExpenseProjection;
		public PXSelect<DRRevenueBalance> RevenueBalance;
		public PXSelect<DRRevenueProjectionAccum> RevenueProjection;

		protected virtual void DRScheduleDetail_RefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void DRScheduleDetail_BAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void DRSchedule_BAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		public List<Batch> RunRecognition(SortedList<string, PX.Objects.DR.DRRecognition.DRBatch> list, DateTime? recDate)
		{			
			List<Batch> batchlist = new List<Batch>(list.Count);

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				foreach (KeyValuePair<string, PX.Objects.DR.DRRecognition.DRBatch> kv in list)
				{
					JournalEntry je = PXGraph.CreateInstance<JournalEntry>();
					je.FieldVerifying.AddHandler<GLTran.referenceID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
					je.FieldVerifying.AddHandler<GLTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
					je.FieldVerifying.AddHandler<GLTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
					je.FieldVerifying.AddHandler<GLTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			
					Batch newbatch = new Batch();
					newbatch.Module = BatchModule.DR;
					newbatch.Status = "U";
					newbatch.Released = true;
					newbatch.Hold = false;
					newbatch.FinPeriodID = kv.Value.FinPeriod;
					newbatch.TranPeriodID = kv.Value.FinPeriod;
                    newbatch.DateEntered = recDate;
					je.BatchModule.Insert(newbatch);

					List<DRScheduleTran> drTranList = new List<DRScheduleTran>();
					foreach (PX.Objects.DR.DRRecognition.DRTranKey key in kv.Value.Trans)
					{
						DRScheduleTran drTran = PXSelect<DRScheduleTran, 
							Where<DRScheduleTran.scheduleID, Equal<Required<DRScheduleTran.scheduleID>>,
							And<DRScheduleTran.componentID, Equal<Required<DRScheduleTran.componentID>>,
							And<DRScheduleTran.lineNbr, Equal<Required<DRScheduleTran.lineNbr>>>>>>.Select(je, key.ScheduleID, key.ComponentID, key.LineNbr);
						drTranList.Add(drTran);
						DRSchedule schedule = GetScheduleByID(key.ScheduleID);
						DRScheduleDetail scheduleDetail = GetScheduleDetail(key.ScheduleID, key.ComponentID);
						
						GLTran tran = new GLTran();
						tran.SummPost = false;
						tran.TranType = schedule.DocType;
						tran.RefNbr = scheduleDetail.ScheduleID.ToString();
						if ( scheduleDetail.BAccountID != null && scheduleDetail.BAccountID != DRScheduleDetail.EmptyBAccountID )
							tran.ReferenceID = scheduleDetail.BAccountID;
						if ( scheduleDetail.ComponentID != null && scheduleDetail.ComponentID != DRScheduleDetail.EmptyComponentID )
							tran.InventoryID = scheduleDetail.ComponentID;
						
						if (IsReversed(scheduleDetail))
						{
							tran.AccountID = drTran.Amount >= 0 ? drTran.AccountID : scheduleDetail.DefAcctID;
							tran.SubID = drTran.Amount >= 0 ? drTran.SubID : scheduleDetail.DefSubID;
						}
						else
						{
                            tran.AccountID = drTran.Amount >= 0 ? scheduleDetail.DefAcctID : drTran.AccountID;
							tran.SubID = drTran.Amount >= 0 ? scheduleDetail.DefSubID : drTran.SubID;
						}
					    tran.BranchID = drTran.BranchID;
						tran.CuryDebitAmt = scheduleDetail.Module == BatchModule.AR ? Math.Abs(drTran.Amount.Value) : 0;
						tran.DebitAmt = scheduleDetail.Module == BatchModule.AR ? Math.Abs(drTran.Amount.Value) : 0;
						tran.CuryCreditAmt = scheduleDetail.Module == BatchModule.AR ? 0 : Math.Abs(drTran.Amount.Value);
						tran.CreditAmt = scheduleDetail.Module == BatchModule.AR ? 0 : Math.Abs(drTran.Amount.Value);
						tran.TranDesc = schedule.TranDesc;
						tran.Released = true;
                        tran.TranDate = drTran.RecDate;
						tran.ProjectID = schedule.ProjectID;
						tran.TaskID = schedule.TaskID;

						je.GLTranModuleBatNbr.Insert(tran);

						GLTran tran2 = new GLTran();
						tran2.SummPost = false;
						tran2.TranType = schedule.DocType;
						tran2.RefNbr = scheduleDetail.ScheduleID.ToString();
						if (scheduleDetail.BAccountID != null && scheduleDetail.BAccountID != DRScheduleDetail.EmptyBAccountID)
							tran2.ReferenceID = scheduleDetail.BAccountID;
						if (scheduleDetail.ComponentID != null && scheduleDetail.ComponentID != DRScheduleDetail.EmptyComponentID)
							tran2.InventoryID = scheduleDetail.ComponentID;
						if (IsReversed(scheduleDetail))
						{
							tran2.AccountID = drTran.Amount >= 0 ? scheduleDetail.DefAcctID : drTran.AccountID;
							tran2.SubID = drTran.Amount >= 0 ? scheduleDetail.DefSubID : drTran.SubID;
						}
						else
						{
							tran2.AccountID = drTran.Amount >= 0 ? drTran.AccountID : scheduleDetail.DefAcctID;
							tran2.SubID = drTran.Amount >= 0 ? drTran.SubID : scheduleDetail.DefSubID;
						}
						tran2.BranchID = drTran.BranchID;
						tran2.CuryDebitAmt = scheduleDetail.Module == BatchModule.AR ? 0 : Math.Abs(drTran.Amount.Value);
                        tran2.DebitAmt = scheduleDetail.Module == BatchModule.AR ? 0 : Math.Abs(drTran.Amount.Value);
						tran2.CuryCreditAmt = scheduleDetail.Module == BatchModule.AR ? Math.Abs(drTran.Amount.Value) : 0;
						tran2.CreditAmt = scheduleDetail.Module == BatchModule.AR ? Math.Abs(drTran.Amount.Value) : 0;
						tran2.TranDesc = schedule.TranDesc;
						tran2.Released = true;
                        tran2.TranDate = drTran.RecDate;
						tran2.ProjectID = schedule.ProjectID;
						tran2.TaskID = schedule.TaskID;
						je.GLTranModuleBatNbr.Insert(tran2);

					}
					je.Save.Press();


					batchlist.Add(je.BatchModule.Current);

					foreach ( DRScheduleTran drTran in drTranList )
					{
						drTran.BatchNbr = je.BatchModule.Current.BatchNbr;
						drTran.Status = DRScheduleTranStatus.Posted;
						drTran.TranDate = je.BatchModule.Current.DateEntered;
						drTran.FinPeriodID = je.BatchModule.Current.FinPeriodID;//Bug: 20528
						Transactions.Update(drTran);

						DRScheduleDetail sc = GetScheduleDetail(drTran.ScheduleID, drTran.ComponentID);
						
						sc.DefAmt -= drTran.Amount.Value;
						sc.LastRecFinPeriodID = drTran.FinPeriodID;
						if (sc.DefAmt == 0)
						{
							sc.Status = DRScheduleStatus.Closed;
							sc.CloseFinPeriodID = drTran.FinPeriodID;
							sc.IsOpen = false;
						}
						ScheduleDetail.Update(sc);
						
																		
						DRDeferredCode dc = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Required<DRDeferredCode.deferredCodeID>>>>.Select(this, sc.DefCode);
												
						UpdateBalance(drTran, sc, GetScheduleByID(sc.ScheduleID),  dc.AccountType);
						
					}
					
				}

				this.Actions.PressSave();

				ts.Complete();
			}
			
			return batchlist;

		}

		public virtual void CreateSchedule(ARTran tran, DRDeferredCode defCode, ARInvoice document)
		{
			CreateSchedule(tran, defCode, document, false);
		}

		/// <summary>
		/// Creates DRSchedule record with multiple DRScheduleDetail records depending on the DeferredCode schedule.
		/// </summary>
		/// <param name="tran">AR Transaction</param>
		/// <param name="defCode">Deferred Code</param>
		/// <param name="document">AR Invoice</param>
		/// <remarks>
		/// Records are created only in the Cache. You have to manually call Perist method.
		/// </remarks>
		public virtual void CreateSchedule(ARTran tran, DRDeferredCode defCode, ARInvoice document, bool isDraft)
		{
			if (tran == null)
				throw new ArgumentNullException("tran");
			if (defCode == null)
				throw new ArgumentNullException("defCode");

			InventoryItem inv = GetInvetoryItemByID(tran.InventoryID);

			ARSetup arsetup = PXSetup<ARSetup>.Select(this);

			PXSelectBase<DRSchedule> select = new PXSelect<DRSchedule,
						Where<DRSchedule.module, Equal<BatchModule.moduleAR>,
						And<DRSchedule.docType, Equal<Required<ARTran.tranType>>,
						And<DRSchedule.refNbr, Equal<Required<ARTran.refNbr>>,
						And<DRSchedule.lineNbr, Equal<Required<ARTran.lineNbr>>>>>>>(this);
			
			if (tran.DefScheduleID == null)
			{
				decimal qtyInBaseUnits = (tran.Qty ?? 0);
				if ( tran.InventoryID != null )
					qtyInBaseUnits = INUnitAttribute.ConvertToBase(Caches[typeof(ARTran)], tran.InventoryID, tran.UOM, (tran.Qty ?? 0), INPrecision.QUANTITY);

				if (isDraft)
				{
					CreateOriginalSchedule(tran.BranchID, BatchModule.AR, tran.TranType, tran.RefNbr, tran.LineNbr, tran.TranAmt, document.DocDate, document.CustomerID, document.CustomerLocationID, defCode, inv, document.FinPeriodID, tran.AccountID, tran.SubID, tran.TranDesc, arsetup.SalesSubMask, qtyInBaseUnits, tran.ProjectID, tran.TaskID, isDraft);
				}
				else
				{					
					DRSchedule sc = select.Select(tran.TranType, tran.RefNbr, tran.LineNbr);

					if (sc != null)
					{
						ReavaluateDraftSchedule(sc, tran.TranType, tran.RefNbr, tran.LineNbr, tran.TranAmt, document.DocDate, document.CustomerID, document.CustomerLocationID, defCode, document.FinPeriodID, tran.TranDesc, isDraft, true, tran.BranchID);
					}
					else
					{
						CreateOriginalSchedule(tran.BranchID, BatchModule.AR, tran.TranType, tran.RefNbr, tran.LineNbr, tran.TranAmt, document.DocDate, document.CustomerID, document.CustomerLocationID, defCode, inv, document.FinPeriodID, tran.AccountID, tran.SubID, tran.TranDesc, arsetup.SalesSubMask, qtyInBaseUnits, tran.ProjectID, tran.TaskID, isDraft);
					}
				}
			}
			else
			{
				if (defCode.Method == DeferredMethodType.CashReceipt)
				{
					if (document.DocType == ARDocType.CreditMemo)
					{
						UpdateOriginalSchedule(tran, defCode, document.DocDate, document.FinPeriodID, document.CustomerID, document.CustomerLocationID);
					}
				}
				else
				{
                    if (document.DocType == ARDocType.CreditMemo)
					{
						if (isDraft)
						{
							CreateRelatedSchedule(tran.BranchID, tran.DefScheduleID, BatchModule.AR, tran.TranType, tran.RefNbr, tran.LineNbr, tran.TranAmt, document.FinPeriodID, document.DocDate, document.CustomerID, document.CustomerLocationID, defCode, inv, document.FinPeriodID, tran.AccountID, tran.SubID, tran.TranDesc, arsetup.SalesSubMask, tran.ProjectID, tran.TaskID, isDraft);
						}
						else
						{
							DRSchedule sc = select.Select(tran.TranType, tran.RefNbr, tran.LineNbr);
							if (sc != null)
							{
								ReavaluateDraftSchedule(sc, tran.TranType, tran.RefNbr, tran.LineNbr, tran.TranAmt, document.DocDate, document.CustomerID, document.CustomerLocationID, defCode, document.FinPeriodID, tran.TranDesc, isDraft, true, tran.BranchID);
							}
							else
							{
								CreateRelatedSchedule(tran.BranchID, tran.DefScheduleID, BatchModule.AR, tran.TranType, tran.RefNbr, tran.LineNbr, tran.TranAmt, document.FinPeriodID, document.DocDate, document.CustomerID, document.CustomerLocationID, defCode, inv, document.FinPeriodID, tran.AccountID, tran.SubID, tran.TranDesc, arsetup.SalesSubMask, tran.ProjectID, tran.TaskID, isDraft);
							}
						}
					}
                    else if (document.DocType == ARDocType.DebitMemo)
                    {
						if (isDraft)
						{
							CreateRelatedSchedule2(tran.BranchID, tran.DefScheduleID, BatchModule.AR, tran.TranType, tran.RefNbr, tran.LineNbr, tran.TranAmt, document.FinPeriodID, document.DocDate, document.CustomerID, document.CustomerLocationID, defCode, inv, document.FinPeriodID, tran.AccountID, tran.SubID, tran.TranDesc, arsetup.SalesSubMask, tran.ProjectID, tran.TaskID, isDraft);
						}
						else
						{
							DRSchedule sc = select.Select(tran.TranType, tran.RefNbr, tran.LineNbr);
							if (sc != null)
							{
								ReavaluateDraftSchedule(sc, tran.TranType, tran.RefNbr, tran.LineNbr, tran.TranAmt, document.DocDate, document.CustomerID, document.CustomerLocationID, defCode, document.FinPeriodID, tran.TranDesc, isDraft, true, tran.BranchID);
							}
							else
							{
								CreateRelatedSchedule2(tran.BranchID, tran.DefScheduleID, BatchModule.AR, tran.TranType, tran.RefNbr, tran.LineNbr, tran.TranAmt, document.FinPeriodID, document.DocDate, document.CustomerID, document.CustomerLocationID, defCode, inv, document.FinPeriodID, tran.AccountID, tran.SubID, tran.TranDesc, arsetup.SalesSubMask, tran.ProjectID, tran.TaskID, isDraft);
							}
						}
                    }
				}
			}
		}

		public virtual void CreateSchedule(APTran tran, DRDeferredCode defCode, APInvoice document)
		{
			CreateSchedule(tran, defCode, document, false);
		}

		/// <summary>
		/// Creates DRSchedule record with multiple DRScheduleDetail records depending on the DeferredCode schedule.
		/// </summary>
		/// <param name="tran">AP Transaction</param>
		/// <param name="defCode">Deferred Code</param>
		/// <param name="document">AP Invoice</param>
		/// <remarks>
		/// Records are created only in the Cache. You have to manually call Perist method.
		/// </remarks>
		public virtual void CreateSchedule(APTran tran, DRDeferredCode defCode, APInvoice document, bool isDraft)
		{
			if (tran == null)
				throw new ArgumentNullException("tran");
			if (defCode == null)
				throw new ArgumentNullException("defCode");

			InventoryItem inv = GetInvetoryItemByID(tran.InventoryID);

			APSetup apsetup = PXSetup<APSetup>.Select(this);

			PXSelectBase<DRSchedule> select = new PXSelect<DRSchedule,
						Where<DRSchedule.module, Equal<BatchModule.moduleAP>,
						And<DRSchedule.docType, Equal<Required<APTran.tranType>>,
						And<DRSchedule.refNbr, Equal<Required<APTran.refNbr>>,
						And<DRSchedule.lineNbr, Equal<Required<APTran.lineNbr>>>>>>>(this);

			if (tran.DefScheduleID == null)
			{
				decimal qtyInBaseUnits = (tran.Qty ?? 0);
				if (tran.InventoryID != null)
					qtyInBaseUnits = INUnitAttribute.ConvertToBase(Caches[typeof(APTran)], tran.InventoryID, tran.UOM, (tran.Qty ?? 0), INPrecision.QUANTITY);

				DRSchedule sc = select.Select(tran.TranType, tran.RefNbr, tran.LineNbr);

				if (sc != null)
				{
					ReavaluateDraftSchedule(sc, tran.TranType, tran.RefNbr, tran.LineNbr, tran.TranAmt, document.DocDate, document.VendorID, document.VendorLocationID, defCode, document.FinPeriodID, tran.TranDesc, isDraft, true, tran.BranchID);
				}
				else
				{
					CreateOriginalSchedule(tran.BranchID, BatchModule.AP, tran.TranType, tran.RefNbr, tran.LineNbr, tran.TranAmt, document.DocDate, document.VendorID, document.VendorLocationID, defCode, inv, document.FinPeriodID, tran.AccountID, tran.SubID, tran.TranDesc, apsetup.ExpenseSubMask, qtyInBaseUnits, tran.ProjectID, tran.TaskID, isDraft);
				}
				
			}
			else
			{
				if (document.DocType == APDocType.DebitAdj)
				{
					CreateRelatedSchedule(tran.BranchID, tran.DefScheduleID, BatchModule.AP, tran.TranType, tran.RefNbr, tran.LineNbr, tran.TranAmt, document.FinPeriodID, document.DocDate, document.VendorID, document.VendorLocationID, defCode, inv, document.FinPeriodID, tran.AccountID, tran.SubID, tran.TranDesc, apsetup.ExpenseSubMask, tran.ProjectID, tran.TaskID, isDraft);
				}
			}
		}

        /// <summary>
        /// Rebuilds DR Balance History Tables.
        /// </summary>
        /// <param name="item">Type of Balance to rebuild</param>
        public virtual void RunIntegrityCheck(PX.Objects.DR.DRBalanceValidation.DRBalanceType item)
        {
            switch (item.AccountType)
            {
                case DeferredAccountType.Income:
                    RunIntegrityCheckRevenue();
                    break;
                case DeferredAccountType.Expense:
                    RunIntegrityCheckExpense();
                    break;

                default:
                    throw new PXException(Messages.DR_SYS_InvalidAccountType, DeferredAccountType.Expense, DeferredAccountType.Income, item.AccountType);
            }
        }

        private void RunIntegrityCheckRevenue()
        {
            PXSelectBase<DRScheduleTran> incomingTranSelect = new PXSelectJoin<DRScheduleTran,
                InnerJoin<DRScheduleDetail, On<DRScheduleDetail.scheduleID, Equal<DRScheduleTran.scheduleID>,
                    And<DRScheduleDetail.componentID, Equal<DRScheduleTran.componentID>>>,
                InnerJoin<DRSchedule, On<DRSchedule.scheduleID, Equal<DRScheduleTran.scheduleID>>>>,
                Where<DRScheduleTran.lineNbr, Equal<DRScheduleDetail.creditLineNbr>,
                And<DRScheduleTran.module, Equal<BatchModule.moduleAR>>>>(this);

            foreach (PXResult<DRScheduleTran, DRScheduleDetail, DRSchedule> res in incomingTranSelect.Select())
            {
                DRScheduleTran tran = (DRScheduleTran) res;
                DRScheduleDetail sd = (DRScheduleDetail) res;
                DRSchedule sc = (DRSchedule) res;

                InitBalance(tran, sd, sc, DeferredAccountType.Income);
            }
            
            PXSelectBase<DRScheduleTran> openTranSelect = new PXSelectJoin<DRScheduleTran,
               InnerJoin<DRScheduleDetail, On<DRScheduleDetail.scheduleID, Equal<DRScheduleTran.scheduleID>,
                   And<DRScheduleDetail.componentID, Equal<DRScheduleTran.componentID>>>,
               InnerJoin<DRSchedule, On<DRSchedule.scheduleID, Equal<DRScheduleTran.scheduleID>>>>,
               Where<DRScheduleTran.lineNbr, NotEqual<DRScheduleDetail.creditLineNbr>,
               And<DRScheduleTran.module, Equal<BatchModule.moduleAR>,
               And<DRScheduleTran.status, Equal<DRScheduleTranStatus.OpenStatus>>>>>(this);

            foreach (PXResult<DRScheduleTran, DRScheduleDetail, DRSchedule> res in openTranSelect.Select())
            {
                DRScheduleTran tran = (DRScheduleTran)res;
                DRScheduleDetail sd = (DRScheduleDetail)res;
                DRSchedule sc = (DRSchedule)res;

                UpdateBalanceProjection(tran, sd, sc, DeferredAccountType.Income);
            }

            PXSelectBase<DRScheduleTran> postedTranSelect = new PXSelectJoin<DRScheduleTran,
              InnerJoin<DRScheduleDetail, On<DRScheduleDetail.scheduleID, Equal<DRScheduleTran.scheduleID>,
                  And<DRScheduleDetail.componentID, Equal<DRScheduleTran.componentID>>>,
              InnerJoin<DRSchedule, On<DRSchedule.scheduleID, Equal<DRScheduleTran.scheduleID>>>>,
              Where<DRScheduleTran.lineNbr, NotEqual<DRScheduleDetail.creditLineNbr>,
              And<DRScheduleTran.module, Equal<BatchModule.moduleAR>,
              And<DRScheduleTran.status, Equal<DRScheduleTranStatus.PostedStatus>>>>>(this);

            foreach (PXResult<DRScheduleTran, DRScheduleDetail, DRSchedule> res in postedTranSelect.Select())
            {
                DRScheduleTran tran = (DRScheduleTran)res;
                DRScheduleDetail sd = (DRScheduleDetail)res;
                DRSchedule sc = (DRSchedule)res;

                UpdateBalance(tran, sd, sc, DeferredAccountType.Income);
            }


            using (new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					PXDatabase.Delete<DRRevenueBalance>();
					PXDatabase.Delete<DRRevenueProjection>();

                    this.Actions.PressSave();
					ts.Complete(this);
				}
			}

        }

        private void RunIntegrityCheckExpense()
        {
            PXSelectBase<DRScheduleTran> incomingTranSelect = new PXSelectJoin<DRScheduleTran,
               InnerJoin<DRScheduleDetail, On<DRScheduleDetail.scheduleID, Equal<DRScheduleTran.scheduleID>,
                   And<DRScheduleDetail.componentID, Equal<DRScheduleTran.componentID>>>,
               InnerJoin<DRSchedule, On<DRSchedule.scheduleID, Equal<DRScheduleTran.scheduleID>>>>,
               Where<DRScheduleTran.lineNbr, Equal<DRScheduleDetail.creditLineNbr>,
               And<DRScheduleTran.module, Equal<BatchModule.moduleAP>>>>(this);

            foreach (PXResult<DRScheduleTran, DRScheduleDetail, DRSchedule> res in incomingTranSelect.Select())
            {
                DRScheduleTran tran = (DRScheduleTran)res;
                DRScheduleDetail sd = (DRScheduleDetail)res;
                DRSchedule sc = (DRSchedule)res;

                InitBalance(tran, sd, sc, DeferredAccountType.Expense);
            }

            PXSelectBase<DRScheduleTran> openTranSelect = new PXSelectJoin<DRScheduleTran,
               InnerJoin<DRScheduleDetail, On<DRScheduleDetail.scheduleID, Equal<DRScheduleTran.scheduleID>,
                   And<DRScheduleDetail.componentID, Equal<DRScheduleTran.componentID>>>,
               InnerJoin<DRSchedule, On<DRSchedule.scheduleID, Equal<DRScheduleTran.scheduleID>>>>,
               Where<DRScheduleTran.lineNbr, NotEqual<DRScheduleDetail.creditLineNbr>,
               And<DRScheduleTran.module, Equal<BatchModule.moduleAP>,
               And<DRScheduleTran.status, Equal<DRScheduleTranStatus.OpenStatus>>>>>(this);

            foreach (PXResult<DRScheduleTran, DRScheduleDetail, DRSchedule> res in openTranSelect.Select())
            {
                DRScheduleTran tran = (DRScheduleTran)res;
                DRScheduleDetail sd = (DRScheduleDetail)res;
                DRSchedule sc = (DRSchedule)res;

                UpdateBalanceProjection(tran, sd, sc, DeferredAccountType.Expense);
            }

            PXSelectBase<DRScheduleTran> postedTranSelect = new PXSelectJoin<DRScheduleTran,
              InnerJoin<DRScheduleDetail, On<DRScheduleDetail.scheduleID, Equal<DRScheduleTran.scheduleID>,
                  And<DRScheduleDetail.componentID, Equal<DRScheduleTran.componentID>>>,
              InnerJoin<DRSchedule, On<DRSchedule.scheduleID, Equal<DRScheduleTran.scheduleID>>>>,
              Where<DRScheduleTran.lineNbr, NotEqual<DRScheduleDetail.creditLineNbr>,
              And<DRScheduleTran.module, Equal<BatchModule.moduleAP>,
              And<DRScheduleTran.status, Equal<DRScheduleTranStatus.PostedStatus>>>>>(this);

            foreach (PXResult<DRScheduleTran, DRScheduleDetail, DRSchedule> res in postedTranSelect.Select())
            {
                DRScheduleTran tran = (DRScheduleTran)res;
                DRScheduleDetail sd = (DRScheduleDetail)res;
                DRSchedule sc = (DRSchedule)res;

                UpdateBalance(tran, sd, sc, DeferredAccountType.Expense);
            }


            using (new PXConnectionScope())
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    PXDatabase.Delete<DRExpenseBalance>();
                    PXDatabase.Delete<DRExpenseProjection>();

                    this.Actions.PressSave();
                    ts.Complete(this);
                }
            }
        }
	
		private void CreateOriginalSchedule(int? branchID, string module, string tranType, string refNbr, int? lineNbr, decimal? tranAmt, DateTime? docDate, int? customerID,  int? customerLocID, DRDeferredCode defCode, InventoryItem inv, string docFinPeriod, int? acctID, int? subID, string tranDesc, string subMask, decimal? qtyInBaseUnit, int? projectID, int? projectTaskID , bool isDraft)
		{
			DRSchedule schedule = new DRSchedule();
			schedule.DocType = tranType;
			schedule.RefNbr = refNbr;
			schedule.LineNbr = lineNbr;
			schedule.DocDate = docDate;
			schedule.BAccountID = customerID;
			schedule.BAccountLocID = customerLocID;
			schedule.Module = module;
			schedule.FinPeriodID = docFinPeriod;
			schedule.TranDesc = tranDesc;
			schedule.ProjectID = projectID;
			schedule.TaskID = projectTaskID;
			schedule.IsDraft = isDraft;
			schedule.IsCustom = false;
			
			schedule = Schedule.Insert(schedule);

			if (inv != null && inv.IsSplitted == true)
			{
                PXSelectBase<INComponent> selectFixedComp = new
					PXSelectJoin<INComponent,
					InnerJoin<DRDeferredCode, On<INComponent.deferredCode, Equal<DRDeferredCode.deferredCodeID>>,
					InnerJoin<InventoryItem, On<INComponent.componentID, Equal<InventoryItem.inventoryID>>>>,
					Where<INComponent.inventoryID, Equal<Required<INComponent.inventoryID>>,
                    And<INComponent.amtOption, Equal<INAmountOption.fixedAmt>>>>(this);

				PXResultset<INComponent> listFixed = selectFixedComp.Select(inv.InventoryID);

                PXSelectBase<INComponent> selectComp = new
                    PXSelectJoin<INComponent,
                    InnerJoin<DRDeferredCode, On<INComponent.deferredCode, Equal<DRDeferredCode.deferredCodeID>>,
                    InnerJoin<InventoryItem, On<INComponent.componentID, Equal<InventoryItem.inventoryID>>>>,
                    Where<INComponent.inventoryID, Equal<Required<INComponent.inventoryID>>,
                    And<INComponent.amtOption, Equal<INAmountOption.percentage>>>>(this);

                PXResultset<INComponent> list = selectComp.Select(inv.InventoryID);

                decimal total = 0;
                foreach (PXResult<INComponent, DRDeferredCode, InventoryItem> res in listFixed)
                {
                    INComponent component = (INComponent)res;
                    DRDeferredCode compDefCode = (DRDeferredCode)res;
                    InventoryItem compInv = (InventoryItem)res;

                    total += PXDBCurrencyAttribute.BaseRound(this, (component.FixedAmt ?? 0) * (qtyInBaseUnit ?? 0));

					decimal amount = PXDBCurrencyAttribute.BaseRound(this, (component.FixedAmt ?? 0) * (qtyInBaseUnit ?? 0));

                    DRScheduleDetail sd = InsertScheduleDetail(branchID, schedule, component, compInv, compDefCode, amount, defCode.AccountID, defCode.SubID, isDraft);

                    if (inv.UseParentSubID == false)
                    {
                        object newSubCD = SubstituteSegment(subID, subMask, component.SalesSubID);
                        if (newSubCD != null)
                        {
                            ScheduleDetail.Cache.RaiseFieldUpdating<DRScheduleDetail.subID>(sd, ref newSubCD);
                            sd.SubID = (int?)newSubCD;
                            ScheduleDetail.Update(sd);
                        }
                    }

					if (!isDraft)
					{
                        AddTranToSchedule(branchID, sd, compDefCode, schedule);
					}
                }

                decimal amtRemaining = tranAmt.Value - total;


                if (amtRemaining < 0)
                {
                    throw new PXException(Messages.FixedAmtSumOverload);
                }

                if (list.Count == 0 && amtRemaining != 0)
                {
                    throw new PXException(Messages.OnlyFixed);
                }

                total = 0;
				for (int i = 0; i < list.Count - 1; i++)
				{
					INComponent component = (PXResult<INComponent, DRDeferredCode, InventoryItem>)list[i];
					DRDeferredCode compDefCode = (PXResult<INComponent, DRDeferredCode, InventoryItem>)list[i];
					InventoryItem compInv = (PXResult<INComponent, DRDeferredCode, InventoryItem>)list[i];

                    decimal amtRaw = amtRemaining * component.Percentage.Value * 0.01m;
					decimal amt = PXDBCurrencyAttribute.BaseRound(this, amtRaw);
					total += amt;

                    DRScheduleDetail sd = InsertScheduleDetail(branchID, schedule, component, compInv, compDefCode, amt, defCode.AccountID, defCode.SubID, isDraft);

					if (inv.UseParentSubID == false)
					{
						object newSubCD = SubstituteSegment(subID, subMask, component.SalesSubID);
						if (newSubCD != null)
						{
							ScheduleDetail.Cache.RaiseFieldUpdating<DRScheduleDetail.subID>(sd, ref newSubCD);
							sd.SubID = (int?)newSubCD;
							ScheduleDetail.Update(sd);
						}
					}

					if (!isDraft)
					{
                        AddTranToSchedule(branchID, sd, compDefCode, schedule);
					}
				}

				INComponent lastComponent = (PXResult<INComponent, DRDeferredCode>)list[list.Count - 1];
				DRDeferredCode lastCompDefCode = (PXResult<INComponent, DRDeferredCode>)list[list.Count - 1];
				InventoryItem lastCompInv = (PXResult<INComponent, DRDeferredCode, InventoryItem>)list[list.Count - 1];
                decimal lastAmt = amtRemaining - total;

                DRScheduleDetail lastSd = InsertScheduleDetail(branchID, schedule, lastComponent, lastCompInv, lastCompDefCode, lastAmt, defCode.AccountID, defCode.SubID, isDraft);
				if (inv.UseParentSubID == false )
				{
					object newSubCD = SubstituteSegment(subID, subMask, lastComponent.SalesSubID);
					if (newSubCD != null)
					{
						ScheduleDetail.Cache.RaiseFieldUpdating<DRScheduleDetail.subID>(lastSd, ref newSubCD);
						lastSd.SubID = (int?)newSubCD;
						ScheduleDetail.Update(lastSd);
					}
				}

				if (!isDraft)
				{
                    AddTranToSchedule(branchID, lastSd, lastCompDefCode, schedule);
				}
			}
			else
			{
                DRScheduleDetail sd = InsertScheduleDetail(branchID, schedule, inv == null ? DRScheduleDetail.EmptyComponentID : inv.InventoryID, defCode, tranAmt.Value, defCode.AccountID, defCode.SubID, acctID, subID, isDraft);

				if (!isDraft)
				{
                    AddTranToSchedule(branchID, sd, defCode, schedule);
				}
			}
		}


		public virtual void ReavaluateDraftSchedule(DRSchedule sc, ARTran tran, DRDeferredCode defCode, ARInvoice document)
		{
			ReavaluateDraftSchedule(sc, tran.TranType, tran.RefNbr, tran.LineNbr, tran.TranAmt, document.DocDate, document.CustomerID, document.CustomerLocationID, defCode, document.FinPeriodID, tran.TranDesc, true, false, tran.BranchID);
		}

		public virtual void ReavaluateDraftSchedule(DRSchedule sc, APTran tran, DRDeferredCode defCode, APInvoice document)
		{
			ReavaluateDraftSchedule(sc, tran.TranType, tran.RefNbr, tran.LineNbr, tran.TranAmt, document.DocDate, document.VendorID, document.VendorLocationID, defCode, document.FinPeriodID, tran.TranDesc, true, false, tran.BranchID);
		}


		private void ReavaluateDraftSchedule(DRSchedule sc, string tranType, string refNbr, int? lineNbr, decimal? tranAmt, DateTime? docDate, int? customerID, int? customerLocID, DRDeferredCode defCode, string docFinPeriod, string tranDesc, bool isDraft, bool releaseMode, int? branchID)
		{
			sc.DocDate = docDate;
			sc.BAccountID = customerID;
			sc.BAccountLocID = customerLocID;
			sc.FinPeriodID = docFinPeriod;
			sc.TranDesc = tranDesc;
			sc.IsCustom = false;
			sc.IsDraft = !releaseMode;
			sc.BAccountType = sc.Module == BatchModule.AP ? PX.Objects.CR.BAccountType.VendorType : PX.Objects.CR.BAccountType.CustomerType;
			
			sc = Schedule.Update(sc);

			IList<DRScheduleDetail> list = GetScheduleDetails(sc.ScheduleID);
			decimal? detailsSum = list.Sum(d => d.TotalAmt);
						
			if (tranAmt != detailsSum)
			{
				if (list.Count == 1)
				{
					list[0].TotalAmt = tranAmt;
					list[0].DefAmt = tranAmt;
				}
				else if (list.Count > 1)
				{
					decimal correctedTotal = 0;
					for (int i = 0; i < list.Count - 1; i++)
					{
						decimal correctedRaw = list[i].TotalAmt.Value * tranAmt.Value / detailsSum.Value;
						decimal corrected = decimal.Round(correctedRaw, 0, MidpointRounding.AwayFromZero);
						correctedTotal += corrected;

						list[i].TotalAmt = corrected;
						list[i].DefAmt = corrected;
					}

					list[list.Count - 1].TotalAmt = tranAmt - correctedTotal;
					list[list.Count - 1].DefAmt = tranAmt - correctedTotal;
				}
			}

			foreach (DRScheduleDetail sd in list)
			{
				sd.DocDate = sc.DocDate;
				sd.BAccountID = sc.BAccountID;
				sd.FinPeriodID = sc.FinPeriodID;
				sd.Status = releaseMode ? DRScheduleStatus.Open : DRScheduleStatus.Draft;
				ScheduleDetail.Update(sd);
			}


			PXSelectBase<DRScheduleDetail> detailSelect = new PXSelectJoin<DRScheduleDetail,
				InnerJoin<DRDeferredCode, On<DRDeferredCode.deferredCodeID, Equal<DRScheduleDetail.defCode>>>,
				Where<DRScheduleDetail.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>>>(this);

			PXResultset<DRScheduleDetail> detailResultset = detailSelect.Select(sc.ScheduleID);
			foreach (PXResult<DRScheduleDetail, DRDeferredCode> res in detailResultset)
			{
				DRScheduleDetail sd = (DRScheduleDetail)res;
				DRDeferredCode sdDefCode = (DRDeferredCode)res;

				IList<DRScheduleTran> tranList = GetScheduleTrans(sd.ScheduleID, sd.ComponentID);

				if (tranList.Count == 0)
				{
					if ( releaseMode )
						AddTranToSchedule(branchID, sd, sdDefCode, sc);
				}
				else
				{
					decimal? tranSum = tranList.Sum(t => t.Amount);

					if (tranSum != sd.TotalAmt)
					{
						if (tranList.Count == 1)
						{
							Subtract(tranList[0], sd, sdDefCode.AccountType);
							tranList[0].Amount = sd.TotalAmt;
							Add(tranList[0], sd, sdDefCode.AccountType);

							Transactions.Update(tranList[0]);
						}
						else if (tranList.Count > 1)
						{
							decimal correctedTotal = 0;
							for (int i = 0; i < tranList.Count - 1; i++)
							{
								decimal correctedRaw = tranList[i].Amount.Value * sd.TotalAmt.Value / tranSum.Value;
								decimal corrected = decimal.Round(correctedRaw, 0, MidpointRounding.AwayFromZero);
								correctedTotal += corrected;

								Subtract(tranList[i], sd, sdDefCode.AccountType);
								tranList[i].Amount = corrected;
								Add(tranList[i], sd, sdDefCode.AccountType);

								Transactions.Update(tranList[i]);
							}

							Subtract(tranList[tranList.Count - 1], sd, sdDefCode.AccountType);
							tranList[tranList.Count - 1].Amount = sd.TotalAmt - correctedTotal;
							Add(tranList[tranList.Count - 1], sd, sdDefCode.AccountType);

							Transactions.Update(tranList[tranList.Count - 1]);
						}
					}
				}

				if ( releaseMode )
					CreateCreditLine(branchID, sc, defCode, sd);
			}
		}



		private string SubstituteSegment(int? subID, string subMask, int? newSubID)
		{
			Sub subAccount = PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(this, subID);
			Sub newSubAccount = PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(this, newSubID);

			if (subAccount != null && newSubAccount != null)
			{
				char[] result = subAccount.SubCD.ToCharArray();

				for (int i = 0; i < subMask.Length; i++)
				{
					if (subMask[i].ToString() == ARAcctSubDefault.MaskItem)
					{
						result[i] = newSubAccount.SubCD[i];
					}
				}

				return new string(result);
			}

			return null;
		}

		private void CreateRelatedSchedule(int? branchID, int? defScheduleID, string module, string tranType, string refNbr, int? lineNbr, decimal? tranAmt, string currentFinPeriod, DateTime? docDate, int? bAccountID, int? locationID, DRDeferredCode defCode, InventoryItem inv, string docFinPeriod, int? acctID, int? subID, string tranDesc, string subMask, int? projectID, int? projectTaskID, bool isDraft)
		{
			DRSchedule origSchedule = GetScheduleByID(defScheduleID);

			DRSchedule schedule = new DRSchedule();
			schedule.DocType = tranType;
			schedule.RefNbr = refNbr;
			schedule.LineNbr = lineNbr;
			schedule.DocDate = docDate;
			schedule.BAccountID = bAccountID;
			schedule.BAccountLocID = locationID;
			schedule.Module = module;
			schedule.FinPeriodID = docFinPeriod;
			schedule.TranDesc = tranDesc;
			schedule.IsDraft = isDraft;
			schedule.IsCustom = false;
			schedule.ProjectID = projectID;
			schedule.TaskID = projectTaskID;

			schedule = Schedule.Insert(schedule);

			PXSelectBase<DRScheduleDetail> origDetailsSelect = new PXSelect<DRScheduleDetail, Where<DRScheduleDetail.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>>>(this);
			PXResultset<DRScheduleDetail> origDetails = origDetailsSelect.Select(origSchedule.ScheduleID);

			decimal origTotalAmt = SumTotal(origDetails);

			decimal adjustTotalAmt = tranAmt.Value; 
			
			foreach (DRScheduleDetail origDetail in origDetails)
			{
				decimal part = 0;
				if ( origTotalAmt != 0 )
					part = origDetail.TotalAmt.Value * adjustTotalAmt / origTotalAmt;
				decimal takeFromSalesRaw = 0;
				if ( origDetail.TotalAmt.Value != 0)
					takeFromSalesRaw = part * (origDetail.TotalAmt.Value - origDetail.DefAmt.Value) / origDetail.TotalAmt.Value; 
				decimal takeFromSales = PXDBCurrencyAttribute.BaseRound(this, takeFromSalesRaw);
				decimal partWithExtra = 0;
				if ( origTotalAmt != 0 ) 
					partWithExtra = origDetail.TotalAmt.Value * tranAmt.Value / origTotalAmt;
				decimal adjustDeferredAmountRaw = partWithExtra - takeFromSales;
				decimal adjustDeferredAmount = PXDBCurrencyAttribute.BaseRound(this, adjustDeferredAmountRaw);

				INComponent comp = null;
				DRDeferredCode compDefCode = null;
				if (inv != null)
				{
					comp = GetComponentByID(inv.InventoryID, origDetail.ComponentID);
					if (comp != null)
					{
						compDefCode = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Required<DRDeferredCode.deferredCodeID>>>>.Select(this, comp.DeferredCode);
					}
				}
								
				InventoryItem component = GetInvetoryItemByID(origDetail.ComponentID);
				DRScheduleDetail sd = null;
				if (compDefCode != null)
				{
					// use def code of component
					sd = InsertScheduleDetail(branchID, schedule, comp, component, compDefCode, PXDBCurrencyAttribute.BaseRound(this, partWithExtra), origDetail.DefAcctID, origDetail.DefSubID, isDraft);

                    if (inv.UseParentSubID == false)
                    {
                        object newSubCD = SubstituteSegment(subID, subMask, component.SalesSubID);
                        if (newSubCD != null)
                        {
                            ScheduleDetail.Cache.RaiseFieldUpdating<DRScheduleDetail.subID>(sd, ref newSubCD);
                            sd.SubID = (int?)newSubCD;
                            ScheduleDetail.Update(sd);
                        }
                    }
                    
				}
				else
				{
					//use DefCode and accounts of line
					sd = InsertScheduleDetail(branchID, schedule, component == null ? DRScheduleDetail.EmptyComponentID : component.InventoryID , defCode, PXDBCurrencyAttribute.BaseRound(this, partWithExtra), origDetail.DefAcctID, origDetail.DefSubID, acctID, subID, isDraft);
				}

				IList<DRScheduleTran> tranList = new List<DRScheduleTran>(); 
								
				short lineCounter = 0;
				if (takeFromSales > 0)
				{
					lineCounter++;
					tranList.Add( AddTranToSchedule_TakeFromSales(sd, currentFinPeriod, takeFromSales) );
				}

				if (adjustDeferredAmount > 0)
				{
					IList<DRScheduleTran> relTrans = GenerateRelatedTransactions(origDetail, sd, compDefCode ?? defCode, adjustDeferredAmount, lineCounter, docDate.Value, branchID);

					if (tranList.Count > 0)
					{
						List<DRScheduleTran> del = new List<DRScheduleTran>();
						foreach (DRScheduleTran relTran in relTrans)
						{
							if (relTran.RecDate.Value < tranList[0].RecDate.Value)
							{
								tranList[0].Amount += relTran.Amount;
								del.Add(relTran);
							}
						}

						foreach (DRScheduleTran d in del)
							relTrans.Remove(d);
					}

					foreach (DRScheduleTran dt in relTrans)
					{
						Transactions.Insert(dt);
						tranList.Add(dt);
					}
				}

				UpdateBalanceProjection(tranList, sd, schedule, defCode.AccountType);
			}
		}

        private void CreateRelatedSchedule2(int? branchID, int? defScheduleID, string module, string tranType, string refNbr, int? lineNbr, decimal? tranAmt, string currentFinPeriod, DateTime? docDate, int? bAccountID, int? locationID, DRDeferredCode defCode, InventoryItem inv, string docFinPeriod, int? acctID, int? subID, string tranDesc, string subMask, int? projectID, int? projectTaskID, bool isDraft)
        {
            DRSchedule origSchedule = GetScheduleByID(defScheduleID);

            DRSchedule schedule = new DRSchedule();
            schedule.DocType = tranType;
            schedule.RefNbr = refNbr;
            schedule.LineNbr = lineNbr;
            schedule.DocDate = docDate;
            schedule.BAccountID = bAccountID;
            schedule.BAccountLocID = locationID;
            schedule.Module = module;
            schedule.FinPeriodID = docFinPeriod;
            schedule.TranDesc = tranDesc;
			schedule.IsDraft = isDraft;
			schedule.IsCustom = false;
			schedule.ProjectID = projectID;
			schedule.TaskID = projectTaskID;


            schedule = Schedule.Insert(schedule);

            PXSelectBase<DRScheduleDetail> origDetailsSelect = new PXSelect<DRScheduleDetail, Where<DRScheduleDetail.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>>>(this);
            PXResultset<DRScheduleDetail> origDetails = origDetailsSelect.Select(origSchedule.ScheduleID);

            decimal origTotalAmt = SumTotal(origDetails);

            
            foreach (DRScheduleDetail origDetail in origDetails)
            {
                decimal partRaw = origDetail.TotalAmt.Value * tranAmt.Value / origTotalAmt;
                decimal part = PXDBCurrencyAttribute.BaseRound(this, partRaw);

                INComponent comp = null;
                DRDeferredCode compDefCode = null;
                if (inv != null)
                {
                    comp = GetComponentByID(inv.InventoryID, origDetail.ComponentID);
                    if (comp != null)
                    {
                        compDefCode = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Required<DRDeferredCode.deferredCodeID>>>>.Select(this, comp.DeferredCode);
                    }
                }

                InventoryItem component = GetInvetoryItemByID(origDetail.ComponentID);
                DRScheduleDetail sd = null;
                if (compDefCode != null)
                {
                    // use def code of component
                    sd = InsertScheduleDetail(branchID, schedule, comp, component, compDefCode, part, origDetail.DefAcctID, origDetail.DefSubID, isDraft);

                    if (inv.UseParentSubID == false)
                    {
                        object newSubCD = SubstituteSegment(subID, subMask, component.SalesSubID);
                        if (newSubCD != null)
                        {
                            ScheduleDetail.Cache.RaiseFieldUpdating<DRScheduleDetail.subID>(sd, ref newSubCD);
                            sd.SubID = (int?)newSubCD;
                            ScheduleDetail.Update(sd);
                        }
                    }

                }
                else
                {
                    //use DefCode and accounts of line
                    sd = InsertScheduleDetail(branchID, schedule, component == null ? DRScheduleDetail.EmptyComponentID : component.InventoryID, defCode, part, origDetail.DefAcctID, origDetail.DefSubID, acctID, subID, isDraft);
                }

                IList<DRScheduleTran> tranList = new List<DRScheduleTran>();

                short lineCounter = 0;

                IList<DRScheduleTran> relTrans = GenerateRelatedTransactions(origDetail, sd, compDefCode ?? defCode, part, lineCounter, docDate.Value, branchID);

                if (tranList.Count > 0)
                {
                    List<DRScheduleTran> del = new List<DRScheduleTran>();
                    foreach (DRScheduleTran relTran in relTrans)
                    {
                        if (relTran.RecDate.Value < tranList[0].RecDate.Value)
                        {
                            tranList[0].Amount += relTran.Amount;
                            del.Add(relTran);
                        }
                    }

                    foreach (DRScheduleTran d in del)
                        relTrans.Remove(d);
                }

                foreach (DRScheduleTran dt in relTrans)
                {
                    Transactions.Insert(dt);
                    tranList.Add(dt);
                }

                UpdateBalanceProjection(tranList, sd, schedule, defCode.AccountType);
            }
        }


		private void UpdateOriginalSchedule(ARTran tran, DRDeferredCode defCode, DateTime? docDate, string docFinPeriod, int? bAccountID, int? locationID)
		{
			DRSchedule origSchedule = GetScheduleByID(tran.DefScheduleID);
			DRScheduleDetail origDetail = PXSelect<DRScheduleDetail, Where<DRScheduleDetail.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>>>.Select(this, origSchedule.ScheduleID);

			decimal origTotalAmt = origDetail.TotalAmt.Value;

			decimal adjustTotalAmt;
			decimal extra = 0;

			if (origTotalAmt <= tran.TranAmt)
			{
				adjustTotalAmt = origTotalAmt;
				extra = tran.TranAmt.Value - origTotalAmt;
			}
			else
			{
				adjustTotalAmt = tran.TranAmt.Value;
			}

			decimal part = origDetail.TotalAmt.Value * adjustTotalAmt / origTotalAmt;
			decimal takeFromSalesRaw = part * (origDetail.TotalAmt.Value - origDetail.DefAmt.Value) / origDetail.TotalAmt.Value;
			decimal takeFromSales = PXDBCurrencyAttribute.BaseRound(this, takeFromSalesRaw);
			decimal partWithExtra = origDetail.TotalAmt.Value * tran.TranAmt.Value / origTotalAmt;
			decimal adjustDeferredAmountRaw = partWithExtra - takeFromSales;
			decimal adjustDeferredAmount = PXDBCurrencyAttribute.BaseRound(this, adjustDeferredAmountRaw);
			InventoryItem component = GetInvetoryItemByID(origDetail.ComponentID);
			
			if (takeFromSales > 0)
			{
				DRScheduleTran nowTran = new DRScheduleTran();
				nowTran.AccountID = origDetail.AccountID;
				nowTran.SubID = origDetail.SubID;
				nowTran.Amount = -takeFromSales;
				nowTran.RecDate = this.Accessinfo.BusinessDate;
				nowTran.FinPeriodID = docFinPeriod;
				nowTran.Module = origDetail.Module;
				nowTran.ScheduleID = origDetail.ScheduleID;
				nowTran.ComponentID = origDetail.ComponentID;
				nowTran.Status = DRScheduleTranStatus.Open;

				Transactions.Insert(nowTran);
				UpdateBalanceProjection(nowTran, origDetail, origSchedule, defCode.AccountType);

				origDetail.DefAmt -= takeFromSales;
			}

			if (adjustDeferredAmount > 0)
			{
				origDetail.DefAmt -= adjustDeferredAmount;

				PXSelectBase<DRScheduleTran> projectedTranSelect = new
					PXSelect<DRScheduleTran, Where<DRScheduleTran.scheduleID, Equal<Required<DRSchedule.scheduleID>>,
					And<DRScheduleTran.componentID, Equal<Required<DRScheduleTran.componentID>>,
					And<DRScheduleTran.status, Equal<DRScheduleTranStatus.ProjectedStatus>>>>>(this);
				PXResultset<DRScheduleTran> projectedTrans = projectedTranSelect.Select(origDetail.ScheduleID, origDetail.ComponentID);

				decimal deltaRaw = adjustDeferredAmount / projectedTrans.Count;
				decimal delta = PXCurrencyAttribute.BaseRound(this, deltaRaw);

				foreach (DRScheduleTran dt in projectedTrans)
				{
					DRRevenueBalance histMin = new DRRevenueBalance();
					histMin.FinPeriodID = tran.FinPeriodID;
					histMin.AcctID = origDetail.DefAcctID;
					histMin.SubID = origDetail.DefSubID;
					histMin.ComponentID = origDetail.ComponentID ?? 0;
					histMin.ProjectID = origDetail.ProjectID ?? 0;
					histMin.CustomerID = origDetail.BAccountID;
					histMin = RevenueBalance.Insert(histMin);
					histMin.PTDProjected -= dt.Amount;
					histMin.EndProjected += dt.Amount;

					DRRevenueProjectionAccum projMin = new DRRevenueProjectionAccum();
					projMin.FinPeriodID = tran.FinPeriodID;
					projMin.AcctID = origDetail.AccountID;
					projMin.SubID = origDetail.SubID;
					projMin.ComponentID = origDetail.ComponentID ?? 0;
					projMin.ProjectID = origDetail.ProjectID ?? 0;
					projMin.CustomerID = origDetail.BAccountID;
					projMin = RevenueProjection.Insert(projMin);
					projMin.PTDProjected -= dt.Amount;
					
					dt.Amount -= delta;
					Transactions.Update(dt);

					DRRevenueBalance histPlus = new DRRevenueBalance();
					histPlus.FinPeriodID = tran.FinPeriodID;
					histPlus.AcctID = origDetail.DefAcctID;
					histPlus.SubID = origDetail.DefSubID;
					histPlus.ComponentID = origDetail.ComponentID ?? 0;
					histPlus.ProjectID = origDetail.ProjectID ?? 0;
					histPlus.CustomerID = origDetail.BAccountID;
					histPlus = RevenueBalance.Insert(histPlus);
					histPlus.PTDProjected += dt.Amount;
					histPlus.EndProjected -= dt.Amount;

					DRRevenueProjectionAccum projPlus = new DRRevenueProjectionAccum();
					projPlus.FinPeriodID = tran.FinPeriodID;
					projPlus.AcctID = origDetail.AccountID;
					projPlus.SubID = origDetail.SubID;
					projPlus.ComponentID = origDetail.ComponentID ?? 0;
					projPlus.ProjectID = origDetail.ProjectID ?? 0;
					projPlus.CustomerID = origDetail.BAccountID;
					projPlus = RevenueProjection.Insert(projPlus);
					projPlus.PTDProjected += dt.Amount;
				}
			}

			ScheduleDetail.Update(origDetail);

			DRRevenueBalance hist = new DRRevenueBalance();
			hist.FinPeriodID =  docFinPeriod;
			hist.AcctID = origDetail.DefAcctID;
			hist.SubID = origDetail.DefSubID;
			hist.ComponentID = origDetail.ComponentID ?? 0;
			hist.ProjectID = origDetail.ProjectID ?? 0;
			hist.CustomerID = origDetail.BAccountID;

			hist = RevenueBalance.Insert(hist);
			hist.PTDDeferred -= tran.TranAmt;
			hist.EndBalance -= tran.TranAmt;
			
		}
		
		private decimal SumTotal(PXResultset<DRScheduleDetail> details)
		{
			decimal result = 0;

			foreach (DRScheduleDetail row in details)
				result += row.TotalAmt.Value;

			return result;
		}
				
		private DRSchedule GetScheduleByID(int? scheduleID)
		{
			return PXSelect<DRSchedule, Where<DRSchedule.scheduleID, Equal<Required<DRSchedule.scheduleID>>>>.Select(this, scheduleID);
		}

		private InventoryItem GetInvetoryItemByID(int? inventoryID)
		{
			return PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, inventoryID);
		}

		private List<DRScheduleTran> GetExistingTrans(DRScheduleDetail sd)
		{
			PXResultset<DRScheduleTran> existingTrans = PXSelect<DRScheduleTran,
				Where<DRScheduleTran.scheduleID, Equal<Required<DRScheduleTran.scheduleID>>,
				And<DRScheduleTran.componentID, Equal<Required<DRScheduleTran.componentID>>,
				And<DRScheduleTran.lineNbr, NotEqual<Required<DRScheduleTran.lineNbr>>,
				And<DRScheduleTran.status , NotEqual<DRScheduleTranStatus.ProjectedStatus>>>>>, 
				OrderBy<Asc<DRScheduleTran.finPeriodID>>>.Select(this, sd.ScheduleID, sd.ComponentID, sd.CreditLineNbr);

			List<DRScheduleTran> list = new List<DRScheduleTran>(existingTrans.Count);

			foreach (DRScheduleTran tran in existingTrans)
				list.Add(tran);

			return list;
		}
				
		private INComponent GetComponentByID(int? inventoryID, int? componentID)
		{
			return PXSelect<INComponent, Where<INComponent.inventoryID, Equal<Required<INComponent.inventoryID>>,
				And<INComponent.componentID, Equal<Required<INComponent.componentID>>>>>.Select(this, inventoryID, componentID);
		}
		
		private DRScheduleDetail GetScheduleDetail(int? scheduleID, int? componentID)
		{
			return PXSelect<DRScheduleDetail,
			Where<DRScheduleDetail.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>,
			And<DRScheduleDetail.componentID, Equal<Required<DRScheduleDetail.componentID>>>>>.Select(this, scheduleID, componentID);
		}

		private IList<DRScheduleDetail> GetScheduleDetails(int? scheduleID)
		{
			PXSelectBase<DRScheduleDetail> select = new PXSelect<DRScheduleDetail,
				Where<DRScheduleDetail.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>>>(this);

			PXResultset<DRScheduleDetail> resultset = select.Select(scheduleID);
			List<DRScheduleDetail> list = new List<DRScheduleDetail>(resultset.Count);
			foreach (DRScheduleDetail ds in resultset)
			{
				list.Add(ds);
			}

			return list;
		}

		private IList<DRScheduleTran> GetScheduleTrans(int? scheduleID, int? componentID)
		{
			PXSelectBase<DRScheduleTran> select = new PXSelect<DRScheduleTran,
						Where<DRScheduleTran.scheduleID, Equal<Required<DRScheduleTran.scheduleID>>,
						And<DRScheduleTran.componentID, Equal<Required<DRScheduleTran.componentID>>>>>(this);

			PXResultset<DRScheduleTran> resultset = select.Select(scheduleID, componentID);

			List<DRScheduleTran> list = new List<DRScheduleTran>(resultset.Count);
			foreach (DRScheduleTran tran in resultset)
			{
				list.Add(tran);
			}

			return list;
						
		}


		private void AddTranToSchedule(int? branchID, DRScheduleDetail sd, DRDeferredCode defCode, DRSchedule sc)
		{
			IList<DRScheduleTran> tranList = GenerateTransactions(this, sc, sd, defCode, branchID);

			foreach (DRScheduleTran tran in tranList)
			{
				Transactions.Insert(tran);
			}

			UpdateBalanceProjection(tranList, sd, sc, defCode.AccountType);
		}

		private DRScheduleTran AddTranToSchedule_TakeFromSales(DRScheduleDetail sd, string currentFinPeriod, decimal amount)
		{
			DRScheduleTran nowTran = new DRScheduleTran();
			nowTran.AccountID = sd.AccountID;
			nowTran.SubID = sd.SubID;
			nowTran.Amount = amount;
			nowTran.RecDate = sd.DocDate;
			nowTran.FinPeriodID = currentFinPeriod;
			nowTran.LineNbr = 1;
			nowTran.Module = sd.Module;
			nowTran.ScheduleID = sd.ScheduleID;
			nowTran.ComponentID = sd.ComponentID;
			nowTran.Status = DRScheduleTranStatus.Open;
			nowTran.IsSamePeriod = nowTran.FinPeriodID == sd.FinPeriodID;

			return Transactions.Insert(nowTran);

			
		}

		#region History & Accumulator functions
		private void UpdateBalanceProjection(IList<DRScheduleTran> tranList, DRScheduleDetail sd, DRSchedule sc, string deferredAccountType)
		{
			foreach (DRScheduleTran tran in tranList)
			{
				UpdateBalanceProjection(tran, sd, sc, deferredAccountType);
			}
		}

		private void UpdateBalanceProjection(DRScheduleTran tran, DRScheduleDetail sd, DRSchedule sc, string deferredAccountType)
		{
			switch (deferredAccountType)
			{
				case DeferredAccountType.Expense:
					UpdateExpenseBalanceProjection(tran, sd, sc);
					UpdateExpenseProjection(tran, sd, sc);
					break;
				case DeferredAccountType.Income:
					UpdateRevenueBalanceProjection(tran, sd, sc);
					UpdateRevenueProjection(tran, sd, sc);
					break;

				default:
					throw new PXException(Messages.DR_SYS_InvalidAccountType, DeferredAccountType.Expense, DeferredAccountType.Income, deferredAccountType);
			}
		}

		private void UpdateRevenueBalanceProjection(DRScheduleTran tran, DRScheduleDetail sd, DRSchedule sc)
		{
			DRRevenueBalance hist = new DRRevenueBalance();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = sd.DefAcctID;
			hist.SubID = sd.DefSubID;
			hist.ComponentID = sd.ComponentID ?? 0;
			hist.ProjectID = sd.ProjectID ?? 0;
			hist.CustomerID = sd.BAccountID ?? 0;

			hist = RevenueBalance.Insert(hist);

			if (IsReversed(sd))
			{
				hist.PTDProjected -= tran.Amount;
				hist.EndProjected += tran.Amount;
			}
			else
			{
				hist.PTDProjected += tran.Amount;
				hist.EndProjected -= tran.Amount;
			}
		}

		private void UpdateExpenseBalanceProjection(DRScheduleTran tran, DRScheduleDetail sd, DRSchedule sc)
		{
			DRExpenseBalance hist = new DRExpenseBalance();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = sd.DefAcctID;
			hist.SubID = sd.DefSubID;
			hist.ComponentID = sd.ComponentID ?? 0;
			hist.ProjectID = sd.ProjectID ?? 0;
			hist.VendorID = sd.BAccountID ?? 0;

			hist = ExpenseBalance.Insert(hist);

			if (IsReversed(sd))
			{
				hist.PTDProjected -= tran.Amount;
				hist.EndProjected += tran.Amount;
			}
			else
			{
				hist.PTDProjected += tran.Amount;
				hist.EndProjected -= tran.Amount;
			}
		}

		private void UpdateBalance(DRScheduleTran tran, DRScheduleDetail sd, DRSchedule sc, string deferredAccountType)
		{
			switch (deferredAccountType)
			{
				case DeferredAccountType.Expense:
					UpdateExpenseBalance(tran, sd, sc);
					UpdateExpenseRecognition(tran, sd, sc);
					break;
				case DeferredAccountType.Income:
					UpdateRevenueBalance(tran, sd, sc);
					UpdateRevenueRecognition(tran, sd, sc);
					break;
				default:
					throw new PXException(Messages.DR_SYS_InvalidAccountType, DeferredAccountType.Expense, DeferredAccountType.Income, deferredAccountType);
			}
		}

		private void UpdateRevenueBalance(DRScheduleTran tran, DRScheduleDetail sd, DRSchedule sc)
		{
			DRRevenueBalance hist = new DRRevenueBalance();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = sd.DefAcctID;
			hist.SubID = sd.DefSubID;
			hist.ComponentID = sd.ComponentID ?? 0;
			hist.ProjectID = sd.ProjectID ?? 0;
			hist.CustomerID = sd.BAccountID ?? 0;

			hist = RevenueBalance.Insert(hist);

			if (IsReversed(sd))
			{
				hist.PTDRecognized -= tran.Amount;
				if (tran.FinPeriodID == sd.FinPeriodID)
					hist.PTDRecognizedSamePeriod -= tran.Amount;
				hist.EndBalance += tran.Amount;
			}
			else
			{
				hist.PTDRecognized += tran.Amount;
				if (tran.FinPeriodID == sd.FinPeriodID)
					hist.PTDRecognizedSamePeriod += tran.Amount;
				hist.EndBalance -= tran.Amount;
			}
		}
				
		private void UpdateExpenseBalance(DRScheduleTran tran, DRScheduleDetail sd, DRSchedule sc)
		{
			DRExpenseBalance hist = new DRExpenseBalance();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = sd.DefAcctID;
			hist.SubID = sd.DefSubID;
			hist.ComponentID = sd.ComponentID ?? 0;
			hist.ProjectID = sd.ProjectID ?? 0;
			hist.VendorID = sd.BAccountID ?? 0;

			hist = ExpenseBalance.Insert(hist);

			if (IsReversed(sd))
			{
				hist.PTDRecognized -= tran.Amount;
				if (tran.FinPeriodID == sd.FinPeriodID)
					hist.PTDRecognizedSamePeriod -= tran.Amount;
				hist.EndBalance += tran.Amount;
			}
			else
			{
				hist.PTDRecognized += tran.Amount;
				if (tran.FinPeriodID == sd.FinPeriodID)
					hist.PTDRecognizedSamePeriod += tran.Amount;
				hist.EndBalance -= tran.Amount;
			}
		}
				
		private void UpdateRevenueProjection(DRScheduleTran tran, DRScheduleDetail sd, DRSchedule sc)
		{
			DRRevenueProjectionAccum hist = new DRRevenueProjectionAccum();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = sd.AccountID;
			hist.SubID = sd.SubID;
			hist.ComponentID = sd.ComponentID ?? 0;
			hist.ProjectID = sd.ProjectID ?? 0;
			hist.CustomerID = sd.BAccountID ?? 0;

			hist = RevenueProjection.Insert(hist);

			if (IsReversed(sd))
			{
				if (tran.FinPeriodID == sd.FinPeriodID)
					hist.PTDRecognizedSamePeriod -= tran.Amount;
				hist.PTDProjected -= tran.Amount;
			}
			else
			{
				if (tran.FinPeriodID == sd.FinPeriodID)
					hist.PTDRecognizedSamePeriod += tran.Amount;
				hist.PTDProjected += tran.Amount;
			}
		}

		private void UpdateExpenseProjection(DRScheduleTran tran, DRScheduleDetail sd, DRSchedule sc)
		{
			DRExpenseProjectionAccum hist = new DRExpenseProjectionAccum();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = sd.AccountID;
			hist.SubID = sd.SubID;
			hist.ComponentID = sd.ComponentID ?? 0;
			hist.ProjectID = sd.ProjectID ?? 0;
			hist.VendorID = sd.BAccountID ?? 0;

			hist = ExpenseProjection.Insert(hist);

			if (IsReversed(sd))
			{
				if (tran.FinPeriodID == sd.FinPeriodID)
					hist.PTDRecognizedSamePeriod -= tran.Amount;
				hist.PTDProjected -= tran.Amount;
			}
			else
			{
				if (tran.FinPeriodID == sd.FinPeriodID)
					hist.PTDRecognizedSamePeriod += tran.Amount;
				hist.PTDProjected += tran.Amount;
			}
		}
				
		private void UpdateRevenueRecognition(DRScheduleTran tran, DRScheduleDetail sd, DRSchedule sc)
		{
			DRRevenueProjectionAccum hist = new DRRevenueProjectionAccum();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = sd.AccountID;
			hist.SubID = sd.SubID;
			hist.ComponentID = sd.ComponentID ?? 0;
			hist.ProjectID = sd.ProjectID ?? 0;
			hist.CustomerID = sd.BAccountID ?? 0;

			hist = RevenueProjection.Insert(hist);

			if (IsReversed(sd))
			{
				hist.PTDRecognized -= tran.Amount;
				if (tran.FinPeriodID == sd.FinPeriodID)
					hist.PTDRecognizedSamePeriod -= tran.Amount;
			}
			else
			{
				hist.PTDRecognized += tran.Amount;
				if (tran.FinPeriodID == sd.FinPeriodID)
					hist.PTDRecognizedSamePeriod += tran.Amount;
			}
		}

		private void UpdateExpenseRecognition(DRScheduleTran tran, DRScheduleDetail sd, DRSchedule sc)
		{
			DRExpenseProjectionAccum hist = new DRExpenseProjectionAccum();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = sd.AccountID;
			hist.SubID = sd.SubID;
			hist.ComponentID = sd.ComponentID ?? 0;
			hist.ProjectID = sd.ProjectID ?? 0;
			hist.VendorID = sd.BAccountID ?? 0;

			hist = ExpenseProjection.Insert(hist);

			if (IsReversed(sd))
			{
				hist.PTDRecognized -= tran.Amount;
				if (tran.FinPeriodID == sd.FinPeriodID)
					hist.PTDRecognizedSamePeriod -= tran.Amount;
			}
			else
			{
				hist.PTDRecognized += tran.Amount;
				if (tran.FinPeriodID == sd.FinPeriodID)
					hist.PTDRecognizedSamePeriod += tran.Amount;
			}
		}

		private void InitBalance(DRScheduleTran tran, DRScheduleDetail sd, DRSchedule sc, string deferredAccountType)
		{
			switch (deferredAccountType)
			{
				case DeferredAccountType.Expense:
					InitExpenseBalance(tran, sd, sc);
					break;
				case DeferredAccountType.Income:
					InitRevenueBalance(tran, sd, sc);
					break;
				default:
					throw new PXException(Messages.DR_SYS_InvalidAccountType, DeferredAccountType.Expense, DeferredAccountType.Income, deferredAccountType);
			}
		}

		private void InitRevenueBalance(DRScheduleTran tran, DRScheduleDetail sd, DRSchedule sc)
		{
			DRRevenueBalance hist = new DRRevenueBalance();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = sd.DefAcctID;
			hist.SubID = sd.DefSubID;
			hist.ComponentID = sd.ComponentID ?? 0;
			hist.ProjectID = sd.ProjectID ?? 0;
			hist.CustomerID = sd.BAccountID ?? 0;

			hist = RevenueBalance.Insert(hist);

			if (IsReversed(sd))
			{
				hist.PTDDeferred -= tran.Amount;
				hist.EndBalance -= tran.Amount;
				hist.EndProjected -= tran.Amount;
			}
			else
			{
				hist.PTDDeferred += tran.Amount;
				hist.EndBalance += tran.Amount;
				hist.EndProjected += tran.Amount;
			}
		}

		private void InitExpenseBalance(DRScheduleTran tran, DRScheduleDetail sd, DRSchedule sc)
		{
			DRExpenseBalance hist = new DRExpenseBalance();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = sd.DefAcctID;
			hist.SubID = sd.DefSubID;
			hist.ComponentID = sd.ComponentID ?? 0;
			hist.ProjectID = sd.ProjectID ?? 0;
			hist.VendorID = sd.BAccountID ?? 0;

			hist = ExpenseBalance.Insert(hist);

			if (IsReversed(sd))
			{
				hist.PTDDeferred -= tran.Amount;
				hist.EndBalance -= tran.Amount;
				hist.EndProjected -= tran.Amount;
			}
			else
			{
				hist.PTDDeferred += tran.Amount;
				hist.EndBalance += tran.Amount;
				hist.EndProjected += tran.Amount;
			}
		}


		private void Subtract(DRScheduleTran tran, DRScheduleDetail sd, string defCodeType)
		{
			if (defCodeType == DeferredAccountType.Expense)
			{
				SubtractExpenseFromProjection(tran, sd);
				SubtractExpenseFromBalance(tran, sd);
			}
			else
			{
				SubtractRevenueFromProjection(tran, sd);
				SubtractRevenueFromBalance(tran, sd);
			}
		}

		private void SubtractRevenueFromProjection(DRScheduleTran tran, DRScheduleDetail sd)
		{
			DRRevenueProjectionAccum hist = new DRRevenueProjectionAccum();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = sd.AccountID;
			hist.SubID = sd.SubID;
			hist.ComponentID = sd.ComponentID ?? 0;
			hist.ProjectID = sd.ProjectID ?? 0;
			hist.CustomerID = sd.BAccountID ?? 0;

			hist = RevenueProjection.Insert(hist);
			hist.PTDProjected -= tran.Amount;
		}

		private void SubtractExpenseFromProjection(DRScheduleTran tran, DRScheduleDetail sd)
		{
			DRExpenseProjectionAccum hist = new DRExpenseProjectionAccum();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = sd.AccountID;
			hist.SubID = sd.SubID;
			hist.ComponentID = sd.ComponentID ?? 0;
			hist.ProjectID = sd.ProjectID ?? 0;
			hist.VendorID = sd.BAccountID ?? 0;

			hist = ExpenseProjection.Insert(hist);
			hist.PTDProjected -= tran.Amount;
		}

		private void SubtractRevenueFromBalance(DRScheduleTran tran, DRScheduleDetail sd)
		{
			DRRevenueBalance hist = new DRRevenueBalance();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = sd.DefAcctID;
			hist.SubID = sd.DefSubID;
			hist.ComponentID = sd.ComponentID ?? 0;
			hist.ProjectID = sd.ProjectID ?? 0;
			hist.CustomerID = sd.BAccountID ?? 0;

			hist = RevenueBalance.Insert(hist);
			hist.PTDProjected -= tran.Amount;
			hist.EndProjected += tran.Amount;
		}

		private void SubtractExpenseFromBalance(DRScheduleTran tran, DRScheduleDetail sd)
		{
			DRExpenseBalance hist = new DRExpenseBalance();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = sd.DefAcctID;
			hist.SubID = sd.DefSubID;
			hist.ComponentID = sd.ComponentID ?? 0;
			hist.ProjectID = sd.ProjectID ?? 0;
			hist.VendorID = sd.BAccountID ?? 0;

			hist = ExpenseBalance.Insert(hist);
			hist.PTDProjected -= tran.Amount;
			hist.EndProjected += tran.Amount;
		}

		private void Add(DRScheduleTran tran, DRScheduleDetail sd, string defCodeType)
		{
			if (defCodeType == DeferredAccountType.Expense)
			{
				AddExpenseToProjection(tran, sd);
				AddExpenseToBalance(tran, sd);
			}
			else
			{
				AddRevenueToProjection(tran, sd);
				AddRevenueToBalance(tran, sd);
			}
		}

		private void AddRevenueToProjection(DRScheduleTran tran, DRScheduleDetail sd)
		{
			DRRevenueProjectionAccum hist = new DRRevenueProjectionAccum();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = sd.AccountID;
			hist.SubID = sd.SubID;
			hist.ComponentID = sd.ComponentID ?? 0;
			hist.ProjectID = sd.ProjectID ?? 0;
			hist.CustomerID = sd.BAccountID ?? 0;

			hist = RevenueProjection.Insert(hist);
			hist.PTDProjected += tran.Amount;
		}

		private void AddExpenseToProjection(DRScheduleTran tran, DRScheduleDetail sd)
		{
			DRExpenseProjectionAccum hist = new DRExpenseProjectionAccum();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = sd.AccountID;
			hist.SubID = sd.SubID;
			hist.ComponentID = sd.ComponentID ?? 0;
			hist.ProjectID = sd.ProjectID ?? 0;
			hist.VendorID = sd.BAccountID ?? 0;

			hist = ExpenseProjection.Insert(hist);
			hist.PTDProjected += tran.Amount;
		}

		private void AddRevenueToBalance(DRScheduleTran tran, DRScheduleDetail sd)
		{
			DRRevenueBalance hist = new DRRevenueBalance();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = sd.DefAcctID;
			hist.SubID = sd.DefSubID;
			hist.ComponentID = sd.ComponentID ?? 0;
			hist.ProjectID = sd.ProjectID ?? 0;
			hist.CustomerID = sd.BAccountID ?? 0;

			hist = RevenueBalance.Insert(hist);
			hist.PTDProjected += tran.Amount;
			hist.EndProjected -= tran.Amount;
		}

		private void AddExpenseToBalance(DRScheduleTran tran, DRScheduleDetail sd)
		{
			DRExpenseBalance hist = new DRExpenseBalance();
			hist.FinPeriodID = tran.FinPeriodID;
			hist.AcctID = sd.DefAcctID;
			hist.SubID = sd.DefSubID;
			hist.ComponentID = sd.ComponentID ?? 0;
			hist.ProjectID = sd.ProjectID ?? 0;
			hist.VendorID = sd.BAccountID ?? 0;

			hist = ExpenseBalance.Insert(hist);
			hist.PTDProjected += tran.Amount;
			hist.EndProjected -= tran.Amount;
		} 
		#endregion


        private DRScheduleDetail InsertScheduleDetail(int? branchID, DRSchedule sc, INComponent component, InventoryItem compItem, DRDeferredCode defCode, decimal amount, int? defAcctID, int? defSubID, bool isDraft)
		{
			int? acctID = sc.Module == BatchModule.AP ? compItem.COGSAcctID : component.SalesAcctID;
			int? subID = sc.Module == BatchModule.AP ? compItem.COGSSubID : component.SalesSubID;

			return InsertScheduleDetail(branchID, sc, compItem.InventoryID, defCode, amount, defAcctID, defSubID, acctID, subID, isDraft);
		}

		private DRScheduleDetail InsertScheduleDetail(int? branchID, DRSchedule sc, int? componentID, DRDeferredCode defCode, decimal amount, int? defAcctID, int? defSubID, int? acctID, int? subID, bool isDraft)
		{
			DRScheduleDetail sd = new DRScheduleDetail();
			sd.ScheduleID = sc.ScheduleID;
			sd.ComponentID = componentID;
			sd.TotalAmt = amount;
			sd.DefAmt = amount;
			sd.DefCode = defCode.DeferredCodeID;
			sd.Status = DRScheduleStatus.Open;
			sd.IsOpen = true;
			sd.Module = sc.Module;
			sd.DocType = sc.DocType;
			sd.RefNbr = sc.RefNbr;
			sd.LineNbr = sc.LineNbr;
			sd.FinPeriodID = sc.FinPeriodID;
			sd.BAccountID = sc.BAccountID;
			sd.AccountID = acctID;
			sd.SubID = subID;
			sd.DefAcctID = defAcctID;
			sd.DefSubID = defSubID;
			sd.CreditLineNbr = 0;
			sd.DocDate = sc.DocDate;
			sd.BAccountType = sc.Module == BatchModule.AP ? PX.Objects.CR.BAccountType.VendorType : PX.Objects.CR.BAccountType.CustomerType;
			sd = ScheduleDetail.Insert(sd);
			sd.Status = isDraft ? DRScheduleStatus.Draft : DRScheduleStatus.Open;
			sd.IsCustom = false;

			if (!isDraft)
			{
				//create credit line:
				CreateCreditLine(branchID, sc, defCode, sd);
			}

			return sd;
		}

		private void CreateCreditLine(int? branchID, DRSchedule sc, DRDeferredCode defCode, DRScheduleDetail sd)
		{
			DRScheduleTran tran = new DRScheduleTran();
		    tran.BranchID = branchID;
			tran.AccountID = sd.AccountID;
			tran.SubID = sd.SubID;
			tran.Amount = sd.TotalAmt;
			tran.RecDate = this.Accessinfo.BusinessDate;
			tran.TranDate = this.Accessinfo.BusinessDate;
			tran.FinPeriodID = sd.FinPeriodID;
			tran.LineNbr = 0;
			tran.Module = sd.Module;
			tran.ScheduleID = sd.ScheduleID;
			tran.ComponentID = sd.ComponentID;
			tran.Status = DRScheduleTranStatus.Posted;

			tran = Transactions.Insert(tran);

			InitBalance(tran, sd, sc, defCode.AccountType);
		}

		

		private bool IsReversed(DRScheduleDetail sd)
		{
			return sd.DocType == ARDocType.CreditMemo || sd.DocType == APDocType.DebitAdj;
		}
						
		public static IList<DRScheduleTran> GenerateTransactions(PXGraph graph, DRSchedule sc, DRScheduleDetail sd, DRDeferredCode defCode, int? branchID)
		{
			decimal defAmount = sd.TotalAmt.Value;

			List<DRScheduleTran> list = new List<DRScheduleTran>();

			short lineCounter = 0;
			if (defCode.ReconNowPct.Value > 0)
			{
				decimal recNowRaw = defAmount * defCode.ReconNowPct.Value * 0.01m;
				decimal recNow = PXDBCurrencyAttribute.BaseRound(graph, recNowRaw);
				defAmount -= recNow;

				lineCounter++;
				DRScheduleTran nowTran = new DRScheduleTran();
			    nowTran.BranchID = branchID;
				nowTran.AccountID = sd.AccountID;
				nowTran.SubID = sd.SubID;
				nowTran.Amount = recNow;
				nowTran.RecDate = sc.DocDate;
				nowTran.FinPeriodID = sd.FinPeriodID;
				nowTran.LineNbr = lineCounter;
				nowTran.Module = sd.Module;
				nowTran.ScheduleID = sd.ScheduleID;
				nowTran.ComponentID = sd.ComponentID;
				nowTran.Status = defCode.Method == DeferredMethodType.CashReceipt ? DRScheduleTranStatus.Projected : DRScheduleTranStatus.Open;
				nowTran.IsSamePeriod = true;
				list.Add(nowTran);
			}

			List<DRScheduleTran> deferredList = new List<DRScheduleTran>(defCode.Occurrences.Value);
			string deferredPeriod = null;
			for (int i = 0; i < defCode.Occurrences.Value; i++)
			{
				try
				{
					if (deferredPeriod == null)
						deferredPeriod = FinPeriodSelectorAttribute.PeriodPlusPeriod(graph, sd.FinPeriodID, defCode.StartOffset.Value);
					else
						deferredPeriod = FinPeriodSelectorAttribute.PeriodPlusPeriod(graph, deferredPeriod, defCode.Frequency.Value);
				}
				catch (PXFinPeriodException ex)
				{
					throw new PXException(string.Format(Messages.NoFinPeriod, defCode.DeferredCodeID), ex);
				}
				lineCounter++;
				DRScheduleTran defTran = new DRScheduleTran();
			    defTran.BranchID = branchID;
				defTran.AccountID = sd.AccountID;
				defTran.SubID = sd.SubID;
				defTran.RecDate = GetDate(graph, defCode, deferredPeriod, sc.DocDate.Value);
				defTran.FinPeriodID = FinPeriodSelectorAttribute.PeriodFromDate(defTran.RecDate);
				defTran.LineNbr = lineCounter;
				defTran.Module = sd.Module;
				defTran.ScheduleID = sd.ScheduleID;
				defTran.ComponentID = sd.ComponentID;
				defTran.Status = defCode.Method == DeferredMethodType.CashReceipt ? DRScheduleTranStatus.Projected : DRScheduleTranStatus.Open;
				defTran.IsSamePeriod = defTran.FinPeriodID == sd.FinPeriodID;
				deferredList.Add(defTran);
			}

			SetAmounts(graph, defCode, deferredList, defAmount, sc.DocDate.Value);

			list.AddRange(deferredList);

			return list;
		}

		private IList<DRScheduleTran> GenerateRelatedTransactions(DRScheduleDetail origDetail, DRScheduleDetail schedule, DRDeferredCode defCode, decimal defAmount, short lineCounter, DateTime docDate, int? branchID)
		{
			PXResultset<DRScheduleTran> openTransactions;

			if (defCode.Method == DeferredMethodType.CashReceipt)
			{
				openTransactions = PXSelect<DRScheduleTran, Where<DRScheduleTran.status, Equal<DRScheduleTranStatus.ProjectedStatus>,
				And<DRScheduleTran.scheduleID, Equal<Required<DRScheduleTran.scheduleID>>,
				And<DRScheduleTran.componentID, Equal<Required<DRScheduleTran.componentID>>,
				And<DRScheduleTran.finPeriodID, GreaterEqual<Required<DRScheduleTran.finPeriodID>>>>>>>.Select(this, origDetail.ScheduleID, origDetail.ComponentID, schedule.FinPeriodID);
			}
			else
			{
				openTransactions = PXSelect<DRScheduleTran, Where<DRScheduleTran.status, Equal<DRScheduleTranStatus.OpenStatus>,
				And<DRScheduleTran.scheduleID, Equal<Required<DRScheduleTran.scheduleID>>,
				And<DRScheduleTran.componentID, Equal<Required<DRScheduleTran.componentID>>,
				And<DRScheduleTran.finPeriodID, GreaterEqual<Required<DRScheduleTran.finPeriodID>>>>>>>.Select(this, origDetail.ScheduleID, origDetail.ComponentID, schedule.FinPeriodID);
			}

			List<DRScheduleTran> deferredList;
			if (openTransactions.Count > 0)
			{
				deferredList = new List<DRScheduleTran>(openTransactions.Count);
				decimal openAmt = 0;
				foreach (DRScheduleTran origTran in openTransactions)
				{
					openAmt += origTran.Amount.Value;
					lineCounter++;
					DRScheduleTran defTran = new DRScheduleTran();
				    defTran.BranchID = branchID;
					defTran.AccountID = schedule.AccountID;
					defTran.SubID = schedule.SubID;
					defTran.RecDate = origTran.RecDate;
					defTran.FinPeriodID = origTran.FinPeriodID;
					defTran.LineNbr = lineCounter;
					defTran.Module = schedule.Module;
					defTran.ScheduleID = schedule.ScheduleID;
					defTran.ComponentID = schedule.ComponentID;
					defTran.Status = DRScheduleTranStatus.Open;
					defTran.IsSamePeriod = defTran.FinPeriodID == schedule.FinPeriodID;
					deferredList.Add(defTran);
				}

				decimal mltp = defAmount / openAmt;

				decimal total = 0;
				for (int i = 0; i < openTransactions.Count - 1; i++)
				{
					decimal raw = mltp * ((DRScheduleTran)openTransactions[i]).Amount.Value;
					deferredList[i].Amount = PXDBCurrencyAttribute.BaseRound(this, raw);
					total += deferredList[i].Amount.Value;
				}
				
				deferredList[deferredList.Count - 1].Amount = defAmount - total;
			}
			else
			{
				deferredList = new List<DRScheduleTran>(1);
				lineCounter++;
				DRScheduleTran defTran = new DRScheduleTran();
			    defTran.BranchID = branchID;
				defTran.AccountID = schedule.AccountID;
				defTran.SubID = schedule.SubID;
				defTran.RecDate = schedule.DocDate;
				defTran.FinPeriodID = schedule.FinPeriodID;
				defTran.LineNbr = lineCounter;
				defTran.Module = schedule.Module;
				defTran.ScheduleID = schedule.ScheduleID;
				defTran.ComponentID = schedule.ComponentID;
				defTran.Status = DRScheduleTranStatus.Open;
				defTran.IsSamePeriod = defTran.FinPeriodID == schedule.FinPeriodID;
				defTran.Amount = defAmount;
				deferredList.Add(defTran);
				
			}

			return deferredList;
		}

		
		private static DateTime GetDate(PXGraph graph, DRDeferredCode code, string finPeriod, DateTime docDate)
		{
			DateTime date = docDate;
			switch (code.ScheduleOption)
			{
				case DRDeferredCode.ScheduleOptionStart:
					date = FinPeriodSelectorAttribute.PeriodStartDate(graph, finPeriod);
					break;
				case DRDeferredCode.ScheduleOptionEnd:
					date = FinPeriodSelectorAttribute.PeriodEndDate(graph, finPeriod);
					break;
				case DRDeferredCode.ScheduleOptionFixedDate:

					DateTime startDate = FinPeriodSelectorAttribute.PeriodStartDate(graph, finPeriod);
					DateTime endDate = FinPeriodSelectorAttribute.PeriodEndDate(graph, finPeriod);

					if (code.FixedDay.Value <= startDate.Day)
						date = startDate;
					else if (code.FixedDay.Value >= endDate.Day)
						date = endDate;
					else
						date = new DateTime(startDate.Year, startDate.Month, code.FixedDay.Value);
					break;
			}

			if (date < docDate)
			    date = docDate;

			return date;
		}

		private static void SetAmounts(PXGraph graph, DRDeferredCode code, IList<DRScheduleTran> deferredTransactions, decimal deferredAmount, DateTime docDate)
		{
			switch (code.Method)
			{
				case DeferredMethodType.CashReceipt:
				case DeferredMethodType.EvenPeriods:
					SetAmountsEvenPeriods(graph, deferredTransactions, deferredAmount);
					break;
				case DeferredMethodType.ProrateDays:
					SetAmountsProrateDays(graph, deferredTransactions, deferredAmount, code, docDate);
					break;
				case DeferredMethodType.ExactDays:
					SetAmountsExactDays(graph, deferredTransactions, deferredAmount, code);
					break;
			}
		}

		private static void SetAmountsEvenPeriods(PXGraph graph, IList<DRScheduleTran> deferredTransactions, decimal deferredAmount)
		{
			if (deferredTransactions.Count > 0)
			{
				decimal amtRaw = deferredAmount / deferredTransactions.Count;
				decimal amt = PXDBCurrencyAttribute.BaseRound(graph, amtRaw);
				decimal recAmt = 0;
				for (int i = 0; i < deferredTransactions.Count - 1; i++)
				{
					deferredTransactions[i].Amount = amt;
					recAmt += amt;
				}

				deferredTransactions[deferredTransactions.Count - 1].Amount = deferredAmount - recAmt;
			}
		}

		private static void SetAmountsProrateDays(PXGraph graph, IList<DRScheduleTran> deferredTransactions, decimal deferredAmount, DRDeferredCode code, DateTime docDate)
		{
			if (deferredTransactions.Count > 0)
			{
				if (deferredTransactions.Count == 1)
				{
					deferredTransactions[0].Amount = deferredAmount;
				}
				else
				{
					string currentPeriod = FinPeriodSelectorAttribute.PeriodFromDate(docDate);
					DateTime startDateOfCurrentPeriod = FinPeriodSelectorAttribute.PeriodStartDate(graph, currentPeriod);

					if (code.StartOffset > 0 || startDateOfCurrentPeriod == docDate)
					{
						SetAmountsEvenPeriods(graph, deferredTransactions, deferredAmount);
					}
					else
					{
						DateTime endDateOfCurrentPeriod = FinPeriodSelectorAttribute.PeriodEndDate(graph, currentPeriod);//returns last day of the month with time 12AM!!!
						TimeSpan spanOfCurrentPeriod = endDateOfCurrentPeriod.Subtract(startDateOfCurrentPeriod);
						int daysInPeriod = spanOfCurrentPeriod.Days + 1;//one is added for the time because 30.12.2009 12AM subtract 01.12.2009 12AM will give 29
						TimeSpan span = endDateOfCurrentPeriod.Subtract(docDate);
						int daysInFirstPeriod = span.Days;// +1;//one is added for the time because 30.12.2009 12AM subtract 01.12.2009 12AM will give 29

						decimal amtRaw = deferredAmount / (deferredTransactions.Count - 1);
						decimal firstRaw = amtRaw * daysInFirstPeriod / daysInPeriod;

						decimal amt = PXDBCurrencyAttribute.BaseRound(graph, amtRaw);
						decimal firstAmt = PXDBCurrencyAttribute.BaseRound(graph, firstRaw);
						deferredTransactions[0].Amount = firstAmt;

						decimal recAmt = firstAmt;
						for (int i = 1; i < deferredTransactions.Count - 1; i++)
						{
							deferredTransactions[i].Amount = amt;
							recAmt += amt;
						}

						deferredTransactions[deferredTransactions.Count - 1].Amount = deferredAmount - recAmt;
					}
				}
			}
		}

		private static void SetAmountsExactDays(PXGraph graph, IList<DRScheduleTran> deferredTransactions, decimal deferredAmount, DRDeferredCode code)
		{
			int totalDays = 0;
			foreach (DRScheduleTran row in deferredTransactions)
			{
				DateTime endDateOfPeriod = FinPeriodSelectorAttribute.PeriodEndDate(graph, row.FinPeriodID);
				DateTime startDateOfPeriod = FinPeriodSelectorAttribute.PeriodStartDate(graph, row.FinPeriodID);
				TimeSpan spanOfPeriod = endDateOfPeriod.Subtract(startDateOfPeriod);
				totalDays += spanOfPeriod.Days + 1;
			}

			decimal amountPerDay = deferredAmount / totalDays;

			decimal recAmt = 0;
			for (int i = 0; i < deferredTransactions.Count - 1; i++)
			{
				DateTime endDateOfPeriod = FinPeriodSelectorAttribute.PeriodEndDate(graph, deferredTransactions[i].FinPeriodID);
				DateTime startDateOfPeriod = FinPeriodSelectorAttribute.PeriodStartDate(graph, deferredTransactions[i].FinPeriodID);
				TimeSpan spanOfPeriod = endDateOfPeriod.Subtract(startDateOfPeriod);
				decimal amountRaw = (spanOfPeriod.Days + 1) * amountPerDay;
				decimal amount = PXDBCurrencyAttribute.BaseRound(graph, amountRaw);

				deferredTransactions[i].Amount = amount;
				recAmt += amount;
			}

			deferredTransactions[deferredTransactions.Count - 1].Amount = deferredAmount - recAmt;
		}

	}
}
