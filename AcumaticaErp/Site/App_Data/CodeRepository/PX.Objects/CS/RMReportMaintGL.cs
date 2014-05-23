using System;
using PX.Data;
using PX.SM;
using System.Collections;
using System.Collections.Generic;
using PX.CS;
using PX.Reports.ARm;

namespace PX.Objects.CS
{
	public class RMReportMaintGL : PXGraphExtension<RMReportMaint>
	{
		public PXSetup<GL.GLSetup> setup;
		protected string acctWildcard;
		protected string subWildcard;
		protected string ytdNetIncomeCD;
		protected short finPeriods;
		protected string perWildcard = "______";
				
		[PXOverride]
		public virtual bool IsFieldVisible(string field, RMReport report)
		{
			string reportType = report != null ? report.Type : null;

			if (reportType == RMType.GL)
			{
				if (
					field.Equals(typeof(RMDataSourcePM.startAccountGroup).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourcePM.endAccountGroup).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourcePM.startProject).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourcePM.endProject).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourcePM.startProjectTask).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourcePM.endProjectTask).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourcePM.startInventory).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourcePM.endInventory).Name, StringComparison.InvariantCultureIgnoreCase)
					)
				{
					return false;
				}
			}	
			return true;
		}
			
		protected virtual void EnsureWildcards()
		{
			if (acctWildcard == null)
			{
				Dimension dim = PXSelect<Dimension,
					Where<Dimension.dimensionID, Equal<Required<Dimension.dimensionID>>>>
					.Select(this.Base, GL.AccountAttribute.DimensionName); 
				if (dim != null && dim.Length != null)
				{
					acctWildcard = new String('_', (int)dim.Length);
				}
				else
				{
					acctWildcard = "";
				}
				dim = PXSelect<Dimension,
					Where<Dimension.dimensionID, Equal<Required<Dimension.dimensionID>>>>
					.Select(this.Base, GL.SubAccountAttribute.DimensionName); 
				if (dim != null && dim.Length != null)
				{
					subWildcard = new String('_', (int)dim.Length);
				}
				else
				{
					subWildcard = "";
				}
				GL.FinYearSetup fysetup = PXSelect<GL.FinYearSetup>.Select(this.Base); 
				if (fysetup != null && fysetup.FinPeriods != null)
				{
					finPeriods = (short)fysetup.FinPeriods;
				}
				GL.Account niacct = PXSelect<GL.Account,
					Where<GL.Account.accountID, Equal<Required<GL.GLSetup.ytdNetIncAccountID>>>>
					.Select(this.Base, setup.Current.YtdNetIncAccountID); 
				if (niacct != null)
				{
					ytdNetIncomeCD = niacct.AccountCD;
				}
			}
		}
		
		protected void RMReport_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected del)
		{
            if (e.Row == null) return;

            PXUIFieldAttribute.SetVisible<RMReportGL.requestAccountClassID>(sender, e.Row, ((RMReport)e.Row).Type == RMType.GL);
			PXUIFieldAttribute.SetVisible<RMReportGL.requestEndAccount>(sender, e.Row, ((RMReport)e.Row).Type == RMType.GL);
			PXUIFieldAttribute.SetVisible<RMReportGL.requestEndSub>(sender, e.Row, ((RMReport)e.Row).Type == RMType.GL);
			PXUIFieldAttribute.SetVisible<RMReportGL.requestLedgerID>(sender, e.Row, ((RMReport)e.Row).Type == RMType.GL);
			PXUIFieldAttribute.SetVisible<RMReportGL.requestStartAccount>(sender, e.Row, ((RMReport)e.Row).Type == RMType.GL);
			PXUIFieldAttribute.SetVisible<RMReportGL.requestStartSub>(sender, e.Row, ((RMReport)e.Row).Type == RMType.GL);
			del(sender, e);
		}

		[PXOverride]
		public virtual void dataSourceFieldSelecting(PXFieldSelectingEventArgs e, string field)
		{			
			RMReport report = (RMReport)e.Row;	

			//Fix AmountType Combo for GL:[PXIntList("0;Not Set,1;Turnover,2;Credit,3;Debit,4;Beg. Balance,5;Ending Balance")]
			if (report != null && report.Type == RMType.GL && field.Equals(typeof(RMDataSource.amountType).Name, StringComparison.InvariantCultureIgnoreCase))
			{
				e.ReturnState = PXIntState.CreateInstance(e.ReturnValue, field, false, 0, null, null,
														  new int[6] { BalanceType.NotSet, BalanceType.Turnover, BalanceType.Credit, BalanceType.Debit, BalanceType.BeginningBalance, BalanceType.EndingBalance },
														  new string[6]
														  {
															  PXLocalizer.Localize(Messages.NotSet, typeof(Messages).FullName),
															  PXLocalizer.Localize(Messages.Turnover, typeof(Messages).FullName),
															  PXLocalizer.Localize(Messages.Credit, typeof(Messages).FullName),
															  PXLocalizer.Localize(Messages.Debit, typeof(Messages).FullName),
															  PXLocalizer.Localize(Messages.BegBalance, typeof(Messages).FullName),
															  PXLocalizer.Localize(Messages.EndingBalance, typeof(Messages).FullName),
														  },
														  typeof(short), BalanceType.NotSet);
				((PXFieldState)e.ReturnState).DisplayName = PXLocalizer.Localize(Messages.AmountType, typeof(Messages).FullName);
			}			
		}						
	}
}
