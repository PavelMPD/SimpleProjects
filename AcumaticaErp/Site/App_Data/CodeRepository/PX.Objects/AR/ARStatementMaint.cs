using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.BQLConstants;
using PX.Objects.AP;

namespace PX.Objects.AR
{
	public class ARStatementMaint : PXGraph<ARStatementMaint, ARStatementCycle>
	{
		public PXSelect<ARStatementCycle> ARStatementCycleRecord;
		public PXAction<ARStatementCycle> RecreateLast;

		[PXUIField(DisplayName = Messages.RegenerateLastStatement, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable recreateLast(PXAdapter adapter)
		{
			ARStatementCycle row = ARStatementCycleRecord.Current;
			if (row.LastStmtDate!= null)
			{
				PXLongOperation.StartOperation(this, delegate() { StatementCycleProcessBO.RegenerateLastStatement(new StatementCycleProcessBO(), row); });
			}
			return adapter.Get();
		}

		public virtual void ARStatementCycle_PrepareOn_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			ARStatementCycle row = (ARStatementCycle)e.Row;
			if (row.PrepareOn == PrepareOnType.EndOfMonth)
			{
				row.Day00 = null;
				row.Day01 = null;
			}
			else
			{
				if (row.PrepareOn == PrepareOnType.FixedDayOfMonth)
				{
					row.Day01 = null;
					if (row.Day00 == null) 
					{
						row.Day00 = 1;
					}
				}
			}
			
		}

		protected virtual void ARStatementCycle_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

			ARStatementCycle row = (ARStatementCycle)e.Row;
			PXUIFieldAttribute.SetEnabled<ARStatementCycle.day00>(cache, null, (row.PrepareOn == PrepareOnType.FixedDayOfMonth || row.PrepareOn == PrepareOnType.Custom));
			PXUIFieldAttribute.SetEnabled<ARStatementCycle.day01>(cache, null, (row.PrepareOn == PrepareOnType.Custom));
			PXUIFieldAttribute.SetEnabled<ARStatementCycle.finChargeID>(cache, null, (row.FinChargeApply??false));
			
			bool isRequired = row.FinChargeApply ?? false;
			PXDefaultAttribute.SetPersistingCheck<ARStatementCycle.finChargeID>(cache, row, isRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXUIFieldAttribute.SetRequired<ARStatementCycle.finChargeID>(cache, isRequired);
			PXUIFieldAttribute.SetEnabled<ARStatementCycle.requireFinChargeProcessing>(cache, null, (row.FinChargeApply ?? false));
		
		}

		protected virtual void ARStatementCycle_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			if (e.Row == null) return;
			PXSelectorAttribute.CheckAndRaiseForeignKeyException(cache, e.Row, typeof(ARStatement.statementCycleId));
			PXSelectorAttribute.CheckAndRaiseForeignKeyException(cache, e.Row, typeof(Customer.statementCycleId));
			PXSelectorAttribute.CheckAndRaiseForeignKeyException(cache, e.Row, typeof(CustomerClass.statementCycleId));
		}

		protected virtual void ARStatementCycle_FinChargeApply_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			ARStatementCycle row = (ARStatementCycle)e.Row;
			if (!(row.FinChargeApply ?? false))
			{
				row.FinChargeID = null;
				row.RequireFinChargeProcessing = false;
			}
		}
	}
}
