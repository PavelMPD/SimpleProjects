using System;
using PX.Data;
using PX.SM;
using System.Collections;
using System.Collections.Generic;
using PX.CS;
using PX.Reports.ARm;

namespace PX.Objects.CS
{
	public class RMReportMaintPM : PXGraphExtension<RMReportMaintGL, RMReportMaint>
	{	
		protected void RMReport_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected del)
		{
            if (e.Row == null) return;

            PXUIFieldAttribute.SetVisible<RMReportPM.requestStartAccountGroup>(sender, e.Row, ((RMReport)e.Row).Type == RMType.PM);
			PXUIFieldAttribute.SetVisible<RMReportPM.requestEndAccountGroup>(sender, e.Row, ((RMReport)e.Row).Type == RMType.PM);
			PXUIFieldAttribute.SetVisible<RMReportPM.requestStartProject>(sender, e.Row, ((RMReport)e.Row).Type == RMType.PM);
			PXUIFieldAttribute.SetVisible<RMReportPM.requestEndProject>(sender, e.Row, ((RMReport)e.Row).Type == RMType.PM);
			PXUIFieldAttribute.SetVisible<RMReportPM.requestStartProjectTask>(sender, e.Row, ((RMReport)e.Row).Type == RMType.PM);
			PXUIFieldAttribute.SetVisible<RMReportPM.requestEndProjectTask>(sender, e.Row, ((RMReport)e.Row).Type == RMType.PM);
			PXUIFieldAttribute.SetVisible<RMReportPM.requestStartInventory>(sender, e.Row, ((RMReport)e.Row).Type == RMType.PM);
			PXUIFieldAttribute.SetVisible<RMReportPM.requestEndInventory>(sender, e.Row, ((RMReport)e.Row).Type == RMType.PM);
			del(sender, e);
		}

		[PXOverride]
		public virtual bool IsFieldVisible(string field, RMReport report, Func<string, RMReport, bool> baseMethod)
		{
			string reportType = report != null ? report.Type : null;
			
			if (reportType == RMType.PM)
			{
				if (
					field.Equals(typeof(RMDataSourceGL.ledgerID).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourceGL.accountClassID).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourceGL.startAccount).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourceGL.endAccount).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourceGL.startSub).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourceGL.endSub).Name, StringComparison.InvariantCultureIgnoreCase)
					)
				{
					return false;
				}
			}

			return baseMethod(field, report);			
		}

		[PXOverride]
		public virtual void dataSourceFieldSelecting(PXFieldSelectingEventArgs e, string field)
		{
			RMReport report = (RMReport)e.Row;
			RMDataSource dataSource = report != null ? Base.DataSourceByID.Select(report.DataSourceID) : null;
			if (dataSource == null)
			{
				object defValue;
				if (Base.DataSourceByID.Cache.RaiseFieldDefaulting(field, null, out defValue))
				{
					Base.DataSourceByID.Cache.RaiseFieldUpdating(field, null, ref defValue);
				}
				Base.DataSourceByID.Cache.RaiseFieldSelecting(field, null, ref defValue, true);
				e.ReturnState = defValue;

			}
			else
			{
				e.ReturnState = Base.DataSourceByID.Cache.GetStateExt(dataSource, field);
			}

			//Fix AmountType Combo for PM:
			if (report != null && report.Type == RMType.PM && field.Equals(typeof(RMDataSource.amountType).Name, StringComparison.InvariantCultureIgnoreCase))
			{
				e.ReturnState = PXIntState.CreateInstance(e.ReturnValue, field, false, 0, null, null,
														  new int[] { BalanceType.NotSet, BalanceType.Amount, BalanceType.Quantity, BalanceType.TurnoverAmount, BalanceType.TurnoverQuantity, BalanceType.BudgetAmount, BalanceType.BudgetQuantity, BalanceType.RevisedAmount, BalanceType.RevisedQuantity, BalanceType.BudgetPTDAmount, BalanceType.BudgetPTDQuantity, BalanceType.RevisedPTDAmount, BalanceType.RevisedPTDQuantity },
														  new string[]
														  {
															  PXLocalizer.Localize(Messages.NotSet, typeof(Messages).FullName),
															  PXLocalizer.Localize(Messages.Amount, typeof(Messages).FullName),
															  PXLocalizer.Localize(Messages.Quantity, typeof(Messages).FullName),
															  PXLocalizer.Localize(Messages.AmountTurnover, typeof(Messages).FullName),
															  PXLocalizer.Localize(Messages.QuantityTurnover, typeof(Messages).FullName),
															  PXLocalizer.Localize(Messages.BudgetAmount, typeof(Messages).FullName),
															  PXLocalizer.Localize(Messages.BudgetQuantity, typeof(Messages).FullName),
															  PXLocalizer.Localize(Messages.RevisedAmount, typeof(Messages).FullName),
															  PXLocalizer.Localize(Messages.RevisedQuantity, typeof(Messages).FullName),
															  PXLocalizer.Localize(Messages.BudgetPTDAmount, typeof(Messages).FullName),
															  PXLocalizer.Localize(Messages.BudgetPTDQuantity, typeof(Messages).FullName),
															  PXLocalizer.Localize(Messages.RevisedPTDAmount, typeof(Messages).FullName),
															  PXLocalizer.Localize(Messages.RevisedPTDQuantity, typeof(Messages).FullName),
														  },
														  typeof(short), 0);
				((PXFieldState)e.ReturnState).DisplayName = PXLocalizer.Localize(Messages.AmountType, typeof(Messages).FullName);
			}
					
			Base1.dataSourceFieldSelecting(e,field);					

			if (e.ReturnState is PXFieldState)
			{
				((PXFieldState)e.ReturnState).SetFieldName("DataSource" + field);
				((PXFieldState)e.ReturnState).Visible = Base.IsFieldVisible(field, report); 
			}
		}										
	}
}
