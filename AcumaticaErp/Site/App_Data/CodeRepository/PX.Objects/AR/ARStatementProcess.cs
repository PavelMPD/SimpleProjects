using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.AR
{
	[TableAndChartDashboardType]
	public class ARStatementProcess : PXGraph<ARStatementProcess>
	{
		
		#region Cache Attached
		#region ARStatementCycle
		[PXDate()]
		[PXUIField(DisplayName = "Next Statement Date", Visibility = PXUIVisibility.Visible, Enabled = false)]
		protected virtual void ARStatementCycle_NextStmtDate_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion

		public ARStatementProcess()
		{
			ARSetup setup = ARSetup.Current;
            CyclesList.SetProcessDelegate<StatementCycleProcessBO>(StatementCycleProcessBO.ProcessCycles);
		}

		public PXSetup<ARSetup> ARSetup;
    	public PXCancel<ARStatementCycle> Cancel;

		[PXFilterable]
		public PXProcessing<ARStatementCycle> CyclesList;

		protected virtual IEnumerable cycleslist()
		{
			ARSetup setup = this.ARSetup.Select();			
			foreach (ARStatementCycle row in PXSelect<ARStatementCycle>.Select(this))
			{
				row.NextStmtDate = CalcStatementDateBefore(Accessinfo.BusinessDate.Value, row.PrepareOn, row.Day00, row.Day01);
				if (row.LastStmtDate.HasValue && row.NextStmtDate <= row.LastStmtDate)
					row.NextStmtDate = CalcNextStatementDate(row.LastStmtDate.Value, row.PrepareOn, row.Day00, row.Day01);
				if (row.NextStmtDate > this.Accessinfo.BusinessDate)
					continue;
				CyclesList.Cache.SetStatus(row, PXEntryStatus.Updated);
				yield return row;
			}
		}

        private static bool CheckForOpenPayments(PXGraph aGraph, string aStatementCycleID)
        {
            ARRegister doc = PXSelectJoin<ARPayment,
                                InnerJoin<Customer, On<ARPayment.customerID, Equal<Customer.bAccountID>>>,
                                Where<Customer.statementCycleId, Equal<Required<Customer.statementCycleId>>,
                And<ARPayment.openDoc, Equal<CS.boolTrue>>>>.SelectWindowed(aGraph, 0, 1, aStatementCycleID);
            return (doc != null);
        }

        private static bool CheckForOverdueInvoices(PXGraph aGraph, string aStatementCycleID, DateTime aOpDate)
        {
            ARBalances doc = PXSelectJoin<ARBalances,
                                InnerJoin<Customer, On<ARBalances.customerID, Equal<Customer.bAccountID>>>,
                                Where<Customer.statementCycleId, Equal<Required<Customer.statementCycleId>>,
                                And<ARBalances.oldInvoiceDate, LessEqual<Required<ARBalances.oldInvoiceDate>>>>>.SelectWindowed(aGraph, 0, 1, aStatementCycleID, aOpDate);
            return (doc != null);
        }

        private static bool CheckForUnreleasedCharges(PXGraph aGraph, string aStatementCycleID)
        {
            ARRegister doc = PXSelectJoin<ARRegister,
                            InnerJoin<Customer, On<ARRegister.customerID, Equal<Customer.bAccountID>>>,
                             Where<ARRegister.docType, Equal<ARDocType.finCharge>,
                             And<ARRegister.released, Equal<BQLConstants.BitOff>,
                             And<Customer.statementCycleId, Equal<Customer.statementCycleId>>>>>.SelectWindowed(aGraph, 0, 1, aStatementCycleID);
            return (doc != null);
        }
               
		public virtual void ARStatementCycle_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;

            ARStatementCycle row = (ARStatementCycle)e.Row;
            if (row.NextStmtDate.HasValue == false)
            {
                DateTime basisDate = row.LastStmtDate.HasValue? row.LastStmtDate.Value: Accessinfo.BusinessDate.HasValue? Accessinfo.BusinessDate.Value: DateTime.Now;
                row.NextStmtDate = CalcNextStatementDate(basisDate, row.PrepareOn, row.Day00, row.Day01);                
            }
			ARSetup setup = this.ARSetup.Select();
			cache.GetAttributes(e.Row,null); 
			if (setup.DefFinChargeFromCycle ?? false)
			{
				bool? hasOverdueInvoices = null;
				bool hasUnAppliedPayments = false;
				bool hasChargeableInvoices = false;
				if (row.RequirePaymentApplication ?? false)
				{
					hasOverdueInvoices = CheckForOverdueInvoices(this, row.StatementCycleId, row.NextStmtDate.Value);
					if (hasOverdueInvoices.Value)
					{
						if (CheckForOpenPayments(this, row.StatementCycleId))
						{
							hasUnAppliedPayments = true;						
						}
					}
				}

				if ((row.FinChargeApply ?? false) && (row.RequireFinChargeProcessing ?? false))
				{
					if (!hasOverdueInvoices.HasValue)
						hasOverdueInvoices = CheckForOverdueInvoices(this, row.StatementCycleId, row.NextStmtDate.Value);
					if (hasOverdueInvoices.Value
							&& (!row.LastFinChrgDate.HasValue || row.LastFinChrgDate.Value < row.NextStmtDate.Value))
					{
						hasChargeableInvoices = true;						
					}
				}
				if (hasChargeableInvoices && hasUnAppliedPayments)
				{
					this.CyclesList.Cache.RaiseExceptionHandling<ARStatementCycle.statementCycleId>(row, row.StatementCycleId,
							new PXSetPropertyException(Messages.WRN_ProcessStatementDetectsOverdueInvoicesAndUnappliedPayments, PXErrorLevel.RowWarning));
				}
				else 
				{
					if (hasChargeableInvoices) 
					{
						this.CyclesList.Cache.RaiseExceptionHandling<ARStatementCycle.statementCycleId>(row, row.StatementCycleId,
                            new PXSetPropertyException(Messages.WRN_ProcessStatementDetectsOverdueInvoices, PXErrorLevel.RowWarning));						
					}

					if (hasUnAppliedPayments) 
					{
						this.CyclesList.Cache.RaiseExceptionHandling<ARStatementCycle.statementCycleId>(row, row.StatementCycleId,
                            new PXSetPropertyException(Messages.WRN_ProcessStatementDetectsUnappliedPayments, PXErrorLevel.RowWarning));
					}
				}
			}
		}

		#region InstanceUtility functions
		public static DateTime? CalcNextStatementDate(DateTime aLastStmtDate, string aPrepareOn, int? aDay00, int? aDay01)
		{
			DateTime? nextDate = null;
			switch (aPrepareOn)
			{
				case PrepareOnType.FixedDayOfMonth:
					DateTime guessDate = new PXDateTime(aLastStmtDate.Year, aLastStmtDate.Month, aDay00 ?? 1);
					nextDate = getNextDate(guessDate, aLastStmtDate, aDay00 ?? 1);
					break;
				case PrepareOnType.EndOfMonth:
					DateTime dateTime = new DateTime(aLastStmtDate.Year, aLastStmtDate.Month , 1);
					dateTime = dateTime.AddMonths(1);
					TimeSpan diff = (dateTime.Subtract(aLastStmtDate));
					int days = diff.Days;
					if (days < 2)
						nextDate = dateTime.AddMonths(1).AddDays(-1);
					else
						nextDate = dateTime.AddDays(-1);
					break;
				case PrepareOnType.Custom:

					DateTime dateTime1 = DateTime.MinValue;
					DateTime dateTime2 = DateTime.MinValue;
					bool useBoth = (aDay00 != null) && (aDay01 != null);
					if (aDay00 != null)
						dateTime1 = new PXDateTime(aLastStmtDate.Year, aLastStmtDate.Month, aDay00.Value);
					if (aDay01 != null)
						dateTime2 = new PXDateTime(aLastStmtDate.Year, aLastStmtDate.Month, aDay01.Value);
					if (useBoth)
					{
						Int32 Day00 = (Int32)aDay00;
						Int32 Day01 = (Int32)aDay01;
						Utilities.SwapIfGreater(ref dateTime1, ref dateTime2);
						Utilities.SwapIfGreater(ref Day00, ref Day01);
						if (aLastStmtDate < dateTime1)
							nextDate = dateTime1;
						else
						{
							if (aLastStmtDate < dateTime2)
								nextDate = dateTime2;
							else
								nextDate = PXDateTime.DatePlusMonthSetDay(dateTime1, 1, Day00);
						}
					}
					else
					{
						DateTime dt = (dateTime1 != DateTime.MinValue) ? dateTime1 : dateTime2;
						if (dt != DateTime.MinValue)
						{
							nextDate = getNextDate(dt, aLastStmtDate, aDay00 ?? aDay01 ?? 1);
						}
					}
					break;
			}
			return nextDate;
		}


		public static DateTime CalcStatementDateBefore(DateTime aBeforeDate, string aPrepareOn, int? aDay00, int? aDay01)
		{
			DateTime statementDate = DateTime.MinValue;
			switch (aPrepareOn)
			{
				case PrepareOnType.FixedDayOfMonth:
					statementDate = new PXDateTime(aBeforeDate.Year, aBeforeDate.Month, aDay00 ?? 1);
					statementDate = getPrevDate(statementDate, aBeforeDate, aDay00 ?? 1);
					break;
				case PrepareOnType.EndOfMonth:
					DateTime dateTime = new DateTime(aBeforeDate.Year, aBeforeDate.Month, 1);
					statementDate = dateTime.AddDays(-1);
					break;
				case PrepareOnType.Custom:
					DateTime dateTime1 = DateTime.MinValue;
					DateTime dateTime2 = DateTime.MinValue;
					bool useBoth = (aDay00 != null) && (aDay01 != null);
					if (aDay00 != null)
						dateTime1 = new PXDateTime(aBeforeDate.Year, aBeforeDate.Month, aDay00.Value);
					if (aDay01 != null)
						dateTime2 = new PXDateTime(aBeforeDate.Year, aBeforeDate.Month, aDay01.Value);
					if (useBoth)
					{
						Int32 Day00 = (Int32)aDay00;
						Int32 Day01 = (Int32)aDay01;
						Utilities.SwapIfGreater(ref dateTime1, ref dateTime2);
						Utilities.SwapIfGreater(ref Day00, ref Day01);
						if (aBeforeDate > dateTime2)
							statementDate = dateTime2;
						else
						{
							if(aBeforeDate > dateTime1)
								statementDate = dateTime1;
							else
								statementDate = PXDateTime.DatePlusMonthSetDay(dateTime2, -1, Day01);
						}
					}
					else
					{
						DateTime dt = (dateTime1 != DateTime.MinValue) ? dateTime1 : dateTime2;
						if (dt != DateTime.MinValue)
						{
							statementDate = getPrevDate(dt, statementDate, aDay00 ?? aDay01 ?? 1);
						}
					}
					break;
				default:
					throw new PXException(Messages.UnknownPrepareOnType);
			}
			return statementDate;
		}

		public static DateTime FindNextStatementDate(DateTime aBusinessDate, ARStatementCycle aCycle) 
		{
			DateTime? result = CalcStatementDateBefore(aBusinessDate, aCycle.PrepareOn, aCycle.Day00, aCycle.Day01);
			if (aCycle.LastStmtDate.HasValue && result <= aCycle.LastStmtDate)
				result = CalcNextStatementDate(aCycle.LastStmtDate.Value, aCycle.PrepareOn, aCycle.Day00, aCycle.Day01);
			return result.HasValue ? result.Value : aBusinessDate;
		}

		public static DateTime FindNextStatementDateAfter(DateTime aBusinessDate, ARStatementCycle aCycle)
		{
			DateTime? result = null;
			if (aCycle.LastStmtDate.HasValue)
			{
				result = CalcNextStatementDate(aCycle.LastStmtDate.Value, aCycle.PrepareOn, aCycle.Day00, aCycle.Day01);
				if (result >= aBusinessDate) return result.Value;
			}
			result = CalcStatementDateBefore(aBusinessDate, aCycle.PrepareOn, aCycle.Day00, aCycle.Day01);
			do
			{
				result = CalcNextStatementDate(result.Value, aCycle.PrepareOn, aCycle.Day00, aCycle.Day01);
			}
			while ((result != null)&&(result < aBusinessDate));			
			return result.Value;
		}

		protected static DateTime getNextDate(DateTime aGuessDate, DateTime aLastStatementDate, int Day)
		{
			return (aLastStatementDate < aGuessDate) ? aGuessDate : PXDateTime.DatePlusMonthSetDay(aGuessDate, 1, Day);
		}

		protected static DateTime getPrevDate(DateTime aGuessDate, DateTime aBeforeDate, int Day)
		{
			return (aGuessDate < aBeforeDate) ? aGuessDate : PXDateTime.DatePlusMonthSetDay(aGuessDate, -1, Day);
		}

		#endregion
	}

	public static class Utilities
	{
		public static void SwapIfGreater<T>(ref T lhs, ref T rhs) where T : System.IComparable<T>
		{
			T temp;
			if (lhs.CompareTo(rhs) > 0)
			{
				temp = lhs;
				lhs = rhs;
				rhs = temp;
			}
		}
	}

    [PXHidden]
	public class StatementCycleProcessBO : PXGraph<StatementCycleProcessBO>
	{
        #region Internal Type Definitions
        protected class StatementKey : AP.Pair<int, string>
        {
            public StatementKey(int aBranchID, string aCury) : base(aBranchID, aCury) { }
            public override int GetHashCode()
            {
                return (this.first + 109 * this.second.GetHashCode());
            }
        }  
        #endregion
        #region Ctor + Public Selects
        public StatementCycleProcessBO()
        {
            ARSetup setup = ARSetup.Current;
        }

        public PXSetup<ARSetup> ARSetup;
        public PXSelect<ARStatementCycle, Where<ARStatementCycle.statementCycleId, Equal<Required<ARStatementCycle.statementCycleId>>>> CyclesList; 
        #endregion
        #region External Processing Functions

        public static void ProcessCycles(StatementCycleProcessBO graph, ARStatementCycle aCycle)
        {
            graph.Clear();
            ARStatementCycle cycle = graph.CyclesList.Select(aCycle.StatementCycleId);
            DateTime? statementDate = aCycle.NextStmtDate;
            using (PXTransactionScope ts = new PXTransactionScope())
            {
                if (statementDate.HasValue)
                {
                    graph.GenerateStatement(cycle, statementDate.Value);
                }
                else
                {
                    graph.GenerateStatement(cycle, graph.Accessinfo.BusinessDate.Value);
                }
                graph.CyclesList.Update(cycle);
                graph.CyclesList.Cache.Persist(PXDBOperation.Update);
                ts.Complete();
            }
        }
        public static void RegenerateLastStatement(StatementCycleProcessBO graph, ARStatementCycle aCycle)
        {
            graph.Clear();
            ARStatementCycle cycle = graph.CyclesList.Select(aCycle.StatementCycleId);
            if (cycle.LastStmtDate != null)
            {
                DateTime stmtDate = (DateTime)cycle.LastStmtDate;
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    DeleteStatement(cycle, stmtDate);
                    graph.GenerateStatement(cycle, stmtDate);
                    PXCache cache = graph.CyclesList.Cache;
                    cache.Update(cycle);
                    cache.Persist(PXDBOperation.Update);
                    ts.Complete();
                }
            }
        }
     
        #endregion
        #region Internal Processing functions
        protected virtual void GenerateStatement(ARStatementCycle aCycle, DateTime aStatementDate)
        {
            PXSelectBase<Customer> selectCustomer = new PXSelect<Customer, Where<Customer.statementCycleId, Equal<Required<Customer.statementCycleId>>>>(this);
            PXSelectBase<ARRegister> selectOpenDocs = new PXSelectJoin<ARRegister, LeftJoin<ARInvoice, On<ARInvoice.docType, Equal<ARRegister.docType>,
                                                            And<ARInvoice.refNbr, Equal<ARRegister.refNbr>>>>,
                                                            Where<ARRegister.customerID, Equal<Required<ARRegister.customerID>>,
                                                            And<ARRegister.released, Equal<BQLConstants.BitOn>,
                                                            And<ARRegister.docDate, LessEqual<Required<ARRegister.docDate>>,
                                                            And<Where<ARRegister.openDoc, Equal<BQLConstants.BitOn>,
                                                            Or<ARRegister.statementDate, IsNull>>>
                                                            >>>>(this);
            PXSelectBase<ARStatementDetail> selectDocsStmDetail = new PXSelect<ARStatementDetail, Where<ARStatementDetail.customerID,
                            Equal<Required<ARStatementDetail.customerID>>,
                            And<ARStatementDetail.docType, Equal<Required<ARStatementDetail.docType>>,
                            And<ARStatementDetail.refNbr, Equal<Required<ARStatementDetail.refNbr>>>>>, OrderBy<Asc<ARStatementDetail.statementDate>>>(this);

            PXSelectBase<ARStatement> selectAllPreviousByCustomer = new PXSelect<ARStatement,
                                                        Where<ARStatement.customerID, Equal<Required<ARStatement.customerID>>>,
                                                                        OrderBy<Asc<ARStatement.curyID, Desc<ARStatement.statementDate>>>>(this);
            PXSelectBase<ARStatement> selectPreviousStatement = new PXSelect<ARStatement,
                                        Where<ARStatement.branchID, Equal<Required<ARStatement.branchID>>,
                                                And<ARStatement.customerID, Equal<Required<ARStatement.customerID>>,
                                                And<ARStatement.curyID, Equal<Required<ARStatement.curyID>>>>>,
                                                OrderBy<Desc<ARStatement.statementDate>>>(this);
            bool ageCredits = (this.ARSetup.Current.AgeCredits ?? false);

            foreach (Customer iCst in selectCustomer.Select(aCycle.StatementCycleId))
            {
                GenerateCustomerStatement(aCycle, iCst, aStatementDate, new Dictionary<Tuple<int, string, int, DateTime>, ARStatement>());
            }
            aCycle.LastStmtDate = aStatementDate;
        }
        protected static void DeleteStatement(ARStatementCycle aStatementCycle, DateTime aStmtDate)
        {
            StatementCreateBO statementBO = PXGraph.CreateInstance<StatementCreateBO>();
            foreach (ARStatement iStm in statementBO.Statement.Select(aStatementCycle.StatementCycleId, aStmtDate))
            {
                statementBO.Statement.Delete(iStm);
            }
            statementBO.Actions.PressSave();
            if (aStatementCycle.LastStmtDate == aStmtDate)
            {
                aStatementCycle.LastStmtDate = statementBO.FindLastCycleStatementDate(aStatementCycle.StatementCycleId, aStmtDate);
            }
        }
        protected static bool PersistStatement(int? aCustomerID, DateTime aStmtDate, IEnumerable<ARStatement> aStatements, IEnumerable<ARStatementDetail> aDetails)
        {
            StatementCreateBO statement = PXGraph.CreateInstance<StatementCreateBO>();
            foreach (Customer cst in statement.Customer.Select(aCustomerID))
            {
                if (cst != null)
                {
                    cst.StatementLastDate = aStmtDate;
                    statement.Customer.Update(cst);
                }
                foreach (ARStatement it in aStatements)
                {
                    statement.Statement.Insert(it);
                }

                foreach (ARStatementDetail it in aDetails)
                {
                    statement.StatementDetail.Insert(it);
                    foreach (ARRegister iDoc in statement.Docs.Select(it.DocType, it.RefNbr))
                    {
                        if (iDoc.StatementDate == null)
                        {
                            iDoc.StatementDate = aStmtDate;
                            statement.Docs.Update(iDoc);
                        }
                    }
                }
                statement.Actions.PressSave();
            }
            return true;
        }
        protected static bool IsMultipleInstallmentMaster(ARInvoice aDoc)
        {
            return (aDoc != null && aDoc.InstallmentCntr.HasValue && aDoc.InstallmentCntr > 0);
        }
        
        #endregion
		#region Utility Functions
		protected static void Copy(ARStatement aDest, ARRegister aSrc)
		{
			aDest.CustomerID = aSrc.CustomerID;
			aDest.CuryID = aSrc.CuryID;
            aDest.BranchID = aSrc.BranchID;
		}
		protected static void Copy(ARStatement aDest, ARStatementCycle aSrc)
		{
			aDest.StatementCycleId = aSrc.StatementCycleId;
			aDest.AgeDays00 = 0;
			aDest.AgeDays01 = aSrc.AgeDays00;
			aDest.AgeDays02 = aSrc.AgeDays01;
			aDest.AgeDays03 = aSrc.AgeDays02;
		}
		protected static void Copy(ARStatement aDest, Customer aSrc)
		{
			aDest.StatementType = aSrc.StatementType;
			aDest.StatementCycleId = aSrc.StatementCycleId;
			aDest.DontPrint = aSrc.PrintStatements != true;
			aDest.DontEmail = aSrc.SendStatementByEmail != true;
		}
		protected static void Clear(ARStatement aDest)
		{
			aDest.AgeBalance00 = aDest.AgeBalance01 = aDest.AgeBalance02 = aDest.AgeBalance03 = aDest.AgeBalance04 = Decimal.Zero;
			aDest.CuryAgeBalance00 = aDest.CuryAgeBalance01 = aDest.CuryAgeBalance02 = aDest.CuryAgeBalance03 = aDest.CuryAgeBalance04 = Decimal.Zero;
			aDest.BegBalance = aDest.EndBalance = Decimal.Zero;
			aDest.CuryBegBalance = aDest.CuryEndBalance = Decimal.Zero;
		}
		protected static void Recalculate(ARStatement aDest)
		{
			aDest.CuryEndBalance += aDest.CuryBegBalance;
			aDest.EndBalance += aDest.BegBalance;
		}
		protected static void Copy(ARStatementDetail aDest, ARRegister aSrc)
		{
			aDest.DocType = aSrc.DocType;
			aDest.RefNbr = aSrc.RefNbr;
            aDest.BranchID = aSrc.BranchID;
			aDest.DocBalance = aSrc.DocBal;
			aDest.CuryDocBalance = aSrc.CuryDocBal;
			aDest.IsOpen = aSrc.OpenDoc;
		}
		protected static void Copy(ARStatementDetail aDest, ARStatement aSrc)
		{
			aDest.CustomerID = aSrc.CustomerID;
			aDest.CuryID = aSrc.CuryID;
			aDest.StatementDate = aSrc.StatementDate;
            //BranchID is copied earlier - from the document to both StatementHeader and details
		}
		protected static void Accumulate(ARStatement aDest, ARRegister aSrc1, ARStatementCycle aSrc2, bool isNewDoc, bool aAgeCredits)
		{

			ARInvoice inv = aSrc1 as ARInvoice;
			Copy(aDest, aSrc2);
			if (isNewDoc)
			{
				aDest.EndBalance = (aDest.EndBalance ?? Decimal.Zero) + ((aSrc1.Payable ?? false) ? aSrc1.OrigDocAmt : (-aSrc1.OrigDocAmt));
				aDest.CuryEndBalance = (aDest.CuryEndBalance ?? Decimal.Zero) + ((aSrc1.Payable ?? false) ? aSrc1.CuryOrigDocAmt : (-aSrc1.CuryOrigDocAmt));
			}
			//ARDocType.SmallCreditWO - is an invoice, but it must be processed as payment
			int days = 0;
			if (inv != null && inv.DocType != ARDocType.SmallCreditWO)
			{
				TimeSpan diff = (aDest.StatementDate.Value.Subtract(inv.DueDate.Value));
				days = diff.Days;
			}
			else 
			{
				TimeSpan diff = (aDest.StatementDate.Value.Subtract(aSrc1.DocDate.Value));
				days = diff.Days;
			}
			if( (inv != null && inv.DocType != ARDocType.SmallCreditWO) || aAgeCredits)
			{
				Decimal docBal = ((bool)aSrc1.Paying) ? -aSrc1.DocBal.Value : aSrc1.DocBal.Value;
				Decimal curyDocBal = ((bool)aSrc1.Paying) ? -aSrc1.CuryDocBal.Value : aSrc1.CuryDocBal.Value;
				if (days <= 0)
				{
					aDest.AgeBalance00 = (aDest.AgeBalance00 ?? Decimal.Zero) + docBal;
					aDest.CuryAgeBalance00 = (aDest.CuryAgeBalance00 ?? Decimal.Zero) + curyDocBal;
				}
				else if (!aSrc2.AgeDays00.HasValue || days <= aSrc2.AgeDays00)
				{
					aDest.AgeBalance01 = (aDest.AgeBalance01 ?? Decimal.Zero) + docBal;
					aDest.CuryAgeBalance01 = (aDest.CuryAgeBalance01 ?? Decimal.Zero) + curyDocBal;
				}
				else if (!aSrc2.AgeDays01.HasValue || days <= aSrc2.AgeDays01)
				{
					aDest.AgeBalance02 = (aDest.AgeBalance02 ?? Decimal.Zero) + docBal;
					aDest.CuryAgeBalance02 = (aDest.CuryAgeBalance02 ?? Decimal.Zero) + curyDocBal;
				}
				else if (!aSrc2.AgeDays02.HasValue || days <= aSrc2.AgeDays02)
				{
					aDest.AgeBalance03 = (aDest.AgeBalance03 ?? Decimal.Zero) + docBal;
					aDest.CuryAgeBalance03 = (aDest.CuryAgeBalance03 ?? Decimal.Zero) + curyDocBal;
				}
				else
				{
					aDest.AgeBalance04 = (aDest.AgeBalance04 ?? Decimal.Zero) + docBal;
					aDest.CuryAgeBalance04 = (aDest.CuryAgeBalance04 ?? Decimal.Zero) + curyDocBal;
				}
			}
			else
			{
				//Payments, when credits are not aged
				aDest.AgeBalance04 = aDest.AgeBalance04 - aSrc1.DocBal; //After completion we must apply resedual payments to previous buckets
				aDest.CuryAgeBalance04 = aDest.CuryAgeBalance04 - aSrc1.CuryDocBal;
			}

		}
		protected static void ApplyFIFORule(ARStatement aDest, bool aAgeCredits)
		{
			//Apply Extra payment in the correct sequence - first to oldest, then - to closer debts
			//We assume, that allpayments are already applyed to oldest -this function propagates them up.
			if (!aAgeCredits)
			{
				if (aDest.AgeBalance04 < 0)//|| (aDest.AgeDays03 == null)) //Extra payments
				{
					aDest.AgeBalance03 += aDest.AgeBalance04;
					aDest.AgeBalance04 = Decimal.Zero;
					aDest.CuryAgeBalance03 += aDest.CuryAgeBalance04;
					aDest.CuryAgeBalance04 = Decimal.Zero;

				}
				if (aDest.AgeBalance03 < 0)//|| (aDest.AgeDays02 == null))
				{
					aDest.AgeBalance02 += aDest.AgeBalance03;
					aDest.AgeBalance03 = Decimal.Zero;
					aDest.CuryAgeBalance02 += aDest.CuryAgeBalance03;
					aDest.CuryAgeBalance03 = Decimal.Zero;
				}
				if (aDest.AgeBalance02 < 0)//|| (aDest.AgeDays01 == null))
				{
					aDest.AgeBalance01 += aDest.AgeBalance02;
					aDest.AgeBalance02 = Decimal.Zero;
					aDest.CuryAgeBalance01 += aDest.CuryAgeBalance02;
					aDest.CuryAgeBalance02 = Decimal.Zero;
				}
				if (aDest.AgeBalance01 < 0)
				{
					aDest.AgeBalance00 += aDest.AgeBalance01;
					aDest.AgeBalance01 = Decimal.Zero;
					aDest.CuryAgeBalance00 += aDest.CuryAgeBalance01;
					aDest.CuryAgeBalance01 = Decimal.Zero;
				}
			}
		}
		#endregion

        #region Statements Generation

        public static void RegenerateStatements(StatementCycleProcessBO graph, ARStatementCycle aCycle, IEnumerable<Customer> customers)
        {
            graph.Clear();
            ARStatementCycle cycle = graph.CyclesList.Select(aCycle.StatementCycleId);
            if (cycle.LastStmtDate != null)
            {
                DateTime stmtDate = (DateTime)cycle.LastStmtDate;
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    //why don't we pass the graph to the DeleteStatements?
                    var trace = DeleteStatements(cycle, customers, stmtDate);
                    //don't we want graph.Accessinfo.BusinessDate.Value here???
                    foreach (var c in customers.GroupBy(cs => cs.BAccountID).Select(g => g.First()))
                    {
                        graph.GenerateCustomerStatement(cycle, c, stmtDate, TraceToDict(trace));
                    }

                    ts.Complete();
                }
            }
        }

        protected virtual void GenerateCustomerStatement(ARStatementCycle aCycle, Customer customer, DateTime aStatementDate,
            Dictionary<Tuple<int, string, int, DateTime>, ARStatement> statementsTrace)
        {
            PXSelectBase<Customer> selectCustomer = new PXSelect<Customer, Where<Customer.statementCycleId, Equal<Required<Customer.statementCycleId>>>>(this);
            PXSelectBase<ARRegister> selectOpenDocs = new PXSelectJoin<ARRegister, LeftJoin<ARInvoice, On<ARInvoice.docType, Equal<ARRegister.docType>,
                                                            And<ARInvoice.refNbr, Equal<ARRegister.refNbr>>>>,
                                                            Where<ARRegister.customerID, Equal<Required<ARRegister.customerID>>,
                                                            And<ARRegister.released, Equal<BQLConstants.BitOn>,
                                                            And<ARRegister.docDate, LessEqual<Required<ARRegister.docDate>>,
                                                            And<Where<ARRegister.openDoc, Equal<BQLConstants.BitOn>,
                                                            Or<ARRegister.statementDate, IsNull>>>>>>>(this);

            PXSelectBase<ARStatementDetail> selectDocsStmDetail = new PXSelect<ARStatementDetail, Where<ARStatementDetail.customerID,
                            Equal<Required<ARStatementDetail.customerID>>,
                            And<ARStatementDetail.docType, Equal<Required<ARStatementDetail.docType>>,
                            And<ARStatementDetail.refNbr, Equal<Required<ARStatementDetail.refNbr>>>>>, OrderBy<Asc<ARStatementDetail.statementDate>>>(this);

            PXSelectBase<ARStatement> selectAllPreviousByCustomer = new PXSelect<ARStatement,
                                                        Where<ARStatement.customerID, Equal<Required<ARStatement.customerID>>>,
                                                                        OrderBy<Asc<ARStatement.curyID, Desc<ARStatement.statementDate>>>>(this);

            PXSelectBase<ARStatement> selectPreviousStatement = new PXSelect<ARStatement,
                                        Where<ARStatement.branchID, Equal<Required<ARStatement.branchID>>,
                                                And<ARStatement.customerID, Equal<Required<ARStatement.customerID>>,
                                                And<ARStatement.curyID, Equal<Required<ARStatement.curyID>>>>>,
                                                OrderBy<Desc<ARStatement.statementDate>>>(this);

            bool ageCredits = (this.ARSetup.Current.AgeCredits ?? false);

            Dictionary<StatementKey, ARStatement> statements = new Dictionary<StatementKey, ARStatement>();
            List<ARStatementDetail> details = new List<ARStatementDetail>();
            foreach (PXResult<ARRegister, ARInvoice> iDoc in selectOpenDocs.Select(customer.BAccountID, aStatementDate))
            {
                ARStatement header = null;
                ARRegister doc = (ARRegister)iDoc;
                ARInvoice inv = (ARInvoice)iDoc;
                ARStatementDetail statementDetail = (ARStatementDetail)selectDocsStmDetail.Select(doc.CustomerID, doc.DocType, doc.RefNbr);
                bool isNewDoc = (statementDetail == null);
                StatementKey key = new StatementKey(doc.BranchID.Value, doc.CuryID);

                if (IsMultipleInstallmentMaster(inv))
                    continue; //Skip invoice, which is the first in multiple installments sequence (master)

                if (!statements.ContainsKey(key))
                {
                    header = new ARStatement();
                    Clear(header);
                    Copy(header, customer);
                    Copy(header, aCycle);
                    Copy(header, doc);
                    header.StatementDate = aStatementDate;

                    ARStatement trace = null;
                    bool gotTrace = statementsTrace.TryGetValue(new Tuple<int, string, int, DateTime>(header.BranchID.Value, header.CuryID, header.CustomerID.Value, header.StatementDate.Value),out trace);
                    if (gotTrace)
                    {
                        header.PrevPrintedCnt = trace.PrevPrintedCnt;
                        header.PrevEmailedCnt = trace.PrevEmailedCnt;
                    }
                    statements[key] = header;
                }
                else
                {
                    header = statements[key];
                }

                ARStatementDetail det = new ARStatementDetail();
                Copy(det, header);
                Copy(det, doc);

                if (doc.DocType != AR.ARDocType.CashSale && doc.DocType != AR.ARDocType.CashReturn)
                {
                    Accumulate(header, (doc.Payable == true ? inv : doc), aCycle, isNewDoc, ageCredits);
                }

                details.Add(det);
            }

            //Merge with previous statements - is needed for Balance Brought Forward
            Dictionary<StatementKey, DateTime> lastCuryStatement = new Dictionary<StatementKey, DateTime>();
            foreach (ARStatement iPrev in selectAllPreviousByCustomer.Select(customer.BAccountID))
            {
                ARStatement header = null;
                StatementKey key = new StatementKey(iPrev.BranchID.Value, iPrev.CuryID);
                if (lastCuryStatement.ContainsKey(key) && lastCuryStatement[key] > iPrev.StatementDate)
                    continue;

                if (!statements.ContainsKey(key))
                {
                    header = new ARStatement();
                    Clear(header);
                    header.BranchID = iPrev.BranchID;
                    header.CuryID = iPrev.CuryID;
                    header.CustomerID = iPrev.CustomerID;
                    header.StatementDate = aStatementDate;
                    Copy(header, customer);
                    Copy(header, aCycle);
                    statements[key] = header;
                }
                else
                {
                    header = statements[key];
                }

                header.BegBalance = iPrev.EndBalance;
                header.CuryBegBalance = iPrev.CuryEndBalance;
                Recalculate(header);
                lastCuryStatement[key] = iPrev.StatementDate.Value;
            }

            foreach (ARStatement iH in statements.Values)
            {
                ARStatement prev = selectPreviousStatement.Select(iH.CustomerID, iH.BranchID, iH.CuryID);
                if (prev != null)
                {
                    iH.BegBalance = prev.EndBalance;
                    iH.CuryBegBalance = prev.CuryEndBalance;
                }

                ApplyFIFORule(iH, ageCredits);
            }

            PersistStatement(customer.BAccountID, aStatementDate, statements.Values, details);
        }


        protected static IEnumerable<ARStatement> DeleteStatements(ARStatementCycle aStatementCycle, IEnumerable<Customer> customers, DateTime aStmtDate)
        {
            var trace = new List<ARStatement>();

            StatementCreateBO statementBO = PXGraph.CreateInstance<StatementCreateBO>();
            foreach (var customer in customers)
            {
                foreach (ARStatement iStm in statementBO.CustomerStatement.Select(aStatementCycle.StatementCycleId, customer.BAccountID, aStmtDate))
                {
                    trace.Add(StatementTrace(iStm));
                    statementBO.CustomerStatement.Delete(iStm);
                }
            }
            statementBO.Actions.PressSave();

            return trace;
        }


        protected static ARStatement StatementTrace(ARStatement statement)
        {
            var trace = new ARStatement
            { 
                BranchID = statement.BranchID,
                CuryID = statement.CuryID,
                CustomerID = statement.CustomerID,
                StatementDate = statement.StatementDate,
                PrevPrintedCnt = statement.PrevPrintedCnt,
                PrevEmailedCnt = statement.PrevEmailedCnt
            };

            if (statement.Printed == true)
                trace.PrevPrintedCnt++;

            if (statement.Emailed == true)
                trace.PrevEmailedCnt++;

            return trace;
        }

        protected static Dictionary<Tuple<int, string, int, DateTime>, ARStatement> TraceToDict(IEnumerable<ARStatement> statementsTrace)
        {
            return statementsTrace.ToDictionary(
                s => new Tuple<int, string, int, DateTime>(s.BranchID.Value, s.CuryID, s.CustomerID.Value, s.StatementDate.Value),
                s => s);
        }
        #endregion
}

	[PXHidden]
	public class StatementCreateBO : PXGraph<StatementCreateBO>
	{
		public PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>> Customer;
		public PXSelect<ARStatement, Where<ARStatement.statementCycleId, Equal<Required<ARStatement.statementCycleId>>,
								And<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>>>> Statement;
        public PXSelect<ARStatement, Where<ARStatement.statementCycleId, Equal<Required<ARStatement.statementCycleId>>,
                                And<ARStatement.customerID, Equal<Required<Customer.bAccountID>>,
                                And<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>>>>> CustomerStatement;
		public PXSelect<ARStatementDetail, Where<ARStatementDetail.customerID, Equal<Current<ARStatement.customerID>>,
					And<ARStatementDetail.statementDate, Equal<Current<ARStatement.statementDate>>,
					And<ARStatementDetail.curyID, Equal<Current<ARStatement.curyID>>>
					>>> StatementDetail;
		public PXSelect<ARRegister, Where<ARRegister.docType, Equal<Optional<ARStatementDetail.docType>>, And<ARRegister.refNbr, Equal<Optional<ARStatementDetail.refNbr>>>>> Docs;
		public virtual void ARStatementDetail_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			ARStatementDetail row = (ARStatementDetail)e.Row;
			foreach (ARRegister iDoc in this.Docs.Select(row.DocType, row.RefNbr))
			{
				if (row.StatementDate == iDoc.StatementDate)
				{
					iDoc.StatementDate = null;
					this.Docs.Update(iDoc);
				}
			}
		}

		public virtual void ARStatement_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			ARStatement row = (ARStatement)e.Row;
			foreach (Customer iCstm in this.Customer.Select(row.CustomerID))
			{
				if (row.StatementDate == iCstm.StatementLastDate)
				{
					iCstm.StatementLastDate = this.FindLastCstmStatementDate(row.CustomerID, row.StatementDate);
					this.Customer.Update(iCstm);
				}
			}
		}

		public DateTime? FindLastCstmStatementDate(int? aCustomer, DateTime? aBeforeDate)
		{
			PXSelectBase<ARStatement> sel = new PXSelect<ARStatement, Where<ARStatement.customerID, Equal<Required<ARStatement.customerID>>,
					And<ARStatement.statementDate, Less<Required<ARStatement.statementDate>>>>, OrderBy<Desc<ARStatement.statementDate>>>(this);
			ARStatement stmt = (ARStatement)sel.View.SelectSingle(aCustomer, aBeforeDate);
			if (stmt != null)
				return stmt.StatementDate;
			return null;
		}
		public DateTime? FindLastCycleStatementDate(string aCycleID, DateTime aBeforeDate)
		{
			PXSelectBase<ARStatement> sel = new PXSelect<ARStatement, Where<ARStatement.statementCycleId, Equal<Required<ARStatement.statementCycleId>>,
					And<ARStatement.statementDate, Less<Required<ARStatement.statementDate>>>>, OrderBy<Desc<ARStatement.statementDate>>>(this);
			ARStatement stmt = (ARStatement)sel.View.SelectSingle(aCycleID, aBeforeDate);
			if (stmt != null)
				return stmt.StatementDate;
			return null;
		}
	}
}
