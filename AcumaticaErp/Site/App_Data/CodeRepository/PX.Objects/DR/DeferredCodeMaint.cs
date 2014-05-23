using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.DR
{
	public class DeferredCodeMaint : PXGraph<DeferredCodeMaint, DRDeferredCode>
	{
		public PXSelect<DRDeferredCode> deferredcode;
		

		private void SetPeriodicallyControlsState(PXCache cache, DRDeferredCode s)
		{
			PXUIFieldAttribute.SetEnabled<DRDeferredCode.fixedDay>(cache, s, s.ScheduleOption == DRDeferredCode.ScheduleOptionFixedDate);
		}


		protected virtual void DRDeferredCode_Method_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRDeferredCode row = e.Row as DRDeferredCode;
			if (row != null && row.Method == DeferredMethodType.CashReceipt)
			{				
				row.AccountType = DeferredAccountType.Income;
			}
		}


		protected virtual void DRDeferredCode_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;

            SetPeriodicallyControlsState(sender, (DRDeferredCode)e.Row);

			if (((DRDeferredCode)e.Row).Method == DeferredMethodType.CashReceipt)
			{				
				PXUIFieldAttribute.SetEnabled<DRDeferredCode.accountType>(sender, e.Row, false);
			}
		}
		
		private void CheckAccountType(PXCache sender, object Row, Int32? AccountID, string AccountType)
		{
			Account account = null;

			if (AccountID == null)
			{
				account = (Account)PXSelectorAttribute.Select<DRDeferredCode.accountID>(sender, Row);
			}
			else
			{
				account = (Account)PXSelectorAttribute.Select<DRDeferredCode.accountID>(sender, Row, AccountID);
			}

			if (account != null && AccountType == "E" && account.Type != "A")
			{
				sender.RaiseExceptionHandling<DRDeferredCode.accountID>(Row, account.AccountCD, new PXSetPropertyException(CS.Messages.AccountTypeWarn, PXErrorLevel.Warning, GL.Messages.Asset));
			}
			if (account != null && AccountType == "I" && account.Type != "L")
			{
				sender.RaiseExceptionHandling<DRDeferredCode.accountID>(Row, account.AccountCD, new PXSetPropertyException(CS.Messages.AccountTypeWarn, PXErrorLevel.Warning, GL.Messages.Liability));
			}
		}

		protected virtual void DRDeferredCode_AccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CheckAccountType(sender, e.Row, (Int32?)e.NewValue, ((DRDeferredCode)e.Row).AccountType);
		}

		protected virtual void DRDeferredCode_AccountType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CheckAccountType(sender, e.Row, null, (string)e.NewValue);
		}
	}
}
