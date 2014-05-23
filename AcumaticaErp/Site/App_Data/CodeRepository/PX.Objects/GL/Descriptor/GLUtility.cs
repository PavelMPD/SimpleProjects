using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.GL
{
	public static class AccountRules
	{
		public static bool IsCreditBalance(string aAcctType)
		{
			if (IsGIRLSAccount(aAcctType)) return true;
			if (IsDEALAccount(aAcctType))
				return false;
			throw new PXException(string.Format(Messages.UnknownAccountTypeDetected, aAcctType));
		}
		public static bool IsGIRLSAccount(string aAcctType)
		{
			switch (aAcctType)
			{
				case "G": //Gain Account type-not implemented
				case AccountType.Income:
				case "R": //Revenues Account type-not implemented 
				case AccountType.Liability:
				case "S":  //Stockholder(s) Accoun type -not implemented
					return true;
			}
			return false;
		}
		public static bool IsDEALAccount(string aAcctType)
		{
			switch (aAcctType)
			{
				case "D":			//Dividend Account type - not implemented
				case AccountType.Expense:
				case AccountType.Asset: 
				case "0":			//Losses Account type - not implemented
					return true;
			}
			return false;
		}

		public static decimal CalcSaldo(string aAcctType, decimal aDebitAmt, decimal aCreditAmt)
		{
			return (IsCreditBalance(aAcctType) ? (aCreditAmt - aDebitAmt) : (aDebitAmt - aCreditAmt));

		}

	}
	public class GLUtility
	{
		public static bool IsAccountHistoryExist(PXGraph graph)
		{
			PXSelectBase select = new PXSelect<GLHistory>(graph);
			Object result = select.View.SelectSingle();
			return (result != null);
		}

		public static bool IsAccountHistoryExist(PXGraph graph, int? accountID)
		{
			PXSelectBase select = new PXSelect<GLHistory, Where<GLHistory.accountID, Equal<Required<GLHistory.accountID>>>>(graph);
			Object result = select.View.SelectSingle(accountID);
			return (result != null);
		}

		public static bool IsLedgerHistoryExist(PXGraph graph, int? ledgerID)
		{
			PXSelectBase select = new PXSelect<GLHistory, Where<GLHistory.ledgerID, Equal<Required<GLHistory.ledgerID>>>>(graph);
			Object result = select.View.SelectSingle(ledgerID);
			return (result != null);
		}
	}
	public static class FiscalPeriodUtils
	{
		public static string FiscalYear(string aFiscalPeriod)
		{
			return aFiscalPeriod.Substring(0, YEAR_LENGTH);
		}

		public static string PeriodInYear(string aFiscalPeriod)
		{
			return aFiscalPeriod.Substring(YEAR_LENGTH, PERIOD_LENGTH);
		}

		public static string Assemble(string aYear, string aPeriod)
		{
			if (aYear.Length != 4 || aPeriod.Length != 2)
			{
				throw new PXArgumentException((string)null, Messages.YearOrPeriodFormatIncorrect);
			}
			return (aYear + aPeriod);
		}

		public const int YEAR_LENGTH = 4;
		public const int PERIOD_LENGTH = 2;
		public const int FULL_LENGHT = YEAR_LENGTH + PERIOD_LENGTH;
		public const string FirstPeriodOfYear = "01";
		public static FinPeriod FindPrevPeriod(PXGraph graph, string fiscalPeriodId, bool aClosedOrActive )
		{
			FinPeriod nextperiod = null;
			if (!string.IsNullOrEmpty(fiscalPeriodId))
			{
				if (aClosedOrActive)
				{
					nextperiod = PXSelect<FinPeriod,
										Where2<
											Where<FinPeriod.closed, Equal<boolTrue>,
											Or<FinPeriod.active, Equal<boolTrue>>>,
										And<FinPeriod.finPeriodID,
										Less<Required<FinPeriod.finPeriodID>>>>,
										OrderBy<Desc<FinPeriod.finPeriodID>>
										>.SelectWindowed(graph, 0, 1, fiscalPeriodId);
				}
				else 
				{
					nextperiod = PXSelect<FinPeriod,
									Where<FinPeriod.finPeriodID,
									Less<Required<FinPeriod.finPeriodID>>>,
									OrderBy<Desc<FinPeriod.finPeriodID>>
									>.SelectWindowed(graph, 0, 1, fiscalPeriodId);
				}
			}
			if (nextperiod == null)
			{
				nextperiod = FindLastPeriod(graph, true);

			}
			return nextperiod;
		}
		public static FinPeriod FindNextPeriod(PXGraph graph, string fiscalPeriodId, bool aClosedOrActive)
		{
			FinPeriod nextperiod = null;
			if (!string.IsNullOrEmpty(fiscalPeriodId))
			{
				if(aClosedOrActive){
				nextperiod = PXSelect<FinPeriod,
										Where2<
											Where<FinPeriod.closed, Equal<boolTrue>,
											Or<FinPeriod.active, Equal<boolTrue>>>,
										And<FinPeriod.finPeriodID,
										Greater<Required<FinPeriod.finPeriodID>>>>,
										OrderBy<Asc<FinPeriod.finPeriodID>>
										>.SelectWindowed(graph, 0, 1, fiscalPeriodId);
				}
				else 
				{
					nextperiod = PXSelect<FinPeriod,
									Where<FinPeriod.finPeriodID,
									Less<Required<FinPeriod.finPeriodID>>>,
									OrderBy<Desc<FinPeriod.finPeriodID>>
									>.SelectWindowed(graph, 0, 1, fiscalPeriodId);
				}
			}
			if (nextperiod == null)
			{
				nextperiod = FindLastPeriod(graph, true);
			}
			return nextperiod;
		}
		public static FinPeriod FindLastPeriod(PXGraph graph, bool aClosedOrActive)
		{
			return (aClosedOrActive ? (PXSelect<FinPeriod,
					Where<FinPeriod.closed, Equal<boolTrue>,
											Or<FinPeriod.active, Equal<boolTrue>>>,
					OrderBy<Desc<FinPeriod.finPeriodID>>
					>.SelectWindowed(graph, 0, 1)) : (
					PXSelectOrderBy<FinPeriod, OrderBy<Desc<FinPeriod.finPeriodID>>
					>.SelectWindowed(graph, 0, 1)));
		}
	}

}
