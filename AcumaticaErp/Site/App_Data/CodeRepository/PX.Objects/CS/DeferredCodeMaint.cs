using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.CS
{
	//public class DeferredCodeMaint : PXGraph<DeferredCodeMaint, DeferredCode>
	//{
	//    public PXSelect<DeferredCode> deferredcode;

	//    private void SetPeriodicallyControlsState(PXCache cache, DeferredCode s)
	//    {
	//        PXUIFieldAttribute.SetEnabled<DeferredCode.periodFixedDay>(cache, s, s.PeriodDateSel == "D");
	//    }

	//    protected virtual void DeferredCode_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
	//    {
	//        SetPeriodicallyControlsState(sender, (DeferredCode)e.Row);
	//    }

	//    protected virtual void DeferredCode_PeriodFrequency_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
	//    {
	//        if ((short)e.NewValue == (short)0)
	//        {
	//            throw new PXSetPropertyException(Messages.Entry_GT, "0");
	//        }

	//        bool IsValid = true;

	//        short? DeferPeriods = ((DeferredCode)e.Row).DeferPeriods;
	//        short? ReconPeriods = ((DeferredCode)e.Row).ReconPeriods;

	//        if (sender.GetValuePending<DeferredCode.deferPeriods>(e.Row) != null)
	//        {
	//            DeferPeriods = (short?)sender.GetValuePending<DeferredCode.deferPeriods>(e.Row);
	//        }

	//        if (sender.GetValuePending<DeferredCode.reconPeriods>(e.Row) != null)
	//        {
	//            ReconPeriods = (short?)sender.GetValuePending<DeferredCode.reconPeriods>(e.Row);
	//        }

	//        if (DeferPeriods != null && DeferPeriods % (short)e.NewValue != 0)
	//        {
	//            IsValid = false;
	//        }

	//        if (ReconPeriods != null && ReconPeriods % (short)e.NewValue != 0)
	//        {
	//            IsValid = false;
	//        }

	//        if (!IsValid)
	//        {
	//            short?[] values = new short?[] { 2, 3, 5, 7 };

	//            StringBuilder sb = new StringBuilder("1");

	//            for (int i = 0; i < values.Length; i++)
	//            {
	//                if (ReconPeriods % values[i] == 0 && DeferPeriods % values[i] == 0)
	//                {
	//                    sb.Append(string.Format(", {0}", values[i]));
	//                }
	//            }
				
	//            throw new PXSetPropertyException(Messages.Entry_EQ, sb.ToString());
	//        }

	//    }

	//    protected virtual void DeferredCode_DeferPeriods_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
	//    {
	//        if (((DeferredCode)e.Row).ReconPeriods >= (short)e.NewValue)
	//        {
	//            throw new PXSetPropertyException(Messages.Entry_LT, ((DeferredCode)e.Row).ReconPeriods.ToString());
	//        }

	//        if (((DeferredCode)e.Row).PeriodFrequency != null && (short)e.NewValue % ((DeferredCode)e.Row).PeriodFrequency != 0)
	//        {
	//            short PeriodFrequency = (short)((DeferredCode)e.Row).PeriodFrequency;
	//            throw new PXSetPropertyException(Messages.Entry_EQ, string.Format(Messages.Entry_Multiple, PeriodFrequency, PeriodFrequency * (short)2, PeriodFrequency * (short)3));
	//        }

	//    }

	//    protected virtual void DeferredCode_ReconPeriods_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
	//    {
	//        if (((DeferredCode)e.Row).DeferPeriods <= (short)e.NewValue)
	//        {
	//            throw new PXSetPropertyException(Messages.Entry_GT, ((DeferredCode)e.Row).DeferPeriods.ToString());
	//        }

	//        if (((DeferredCode)e.Row).PeriodFrequency != null && (short)e.NewValue % ((DeferredCode)e.Row).PeriodFrequency != 0)
	//        {
	//            short PeriodFrequency = (short)((DeferredCode)e.Row).PeriodFrequency;
	//            throw new PXSetPropertyException(Messages.Entry_EQ, string.Format(Messages.Entry_Multiple, PeriodFrequency, PeriodFrequency * (short)2, PeriodFrequency * (short)3));
	//        }
	//    }

	//    private void CheckAccountType(PXCache sender, object Row, Int32? AccountID, string AccountType)
	//    {
	//        Account account = null;

	//        if (AccountID == null)
	//        {
	//            account = (Account)PXSelectorAttribute.Select<DeferredCode.accountID>(sender, Row);
	//        }
	//        else
	//        {
	//            account = (Account)PXSelectorAttribute.Select<DeferredCode.accountID>(sender, Row, AccountID);
	//        }

	//        if (account != null && AccountType == "E" && account.Type != "A")
	//        {
	//            sender.RaiseExceptionHandling<DeferredCode.accountID>(Row, account.AccountCD, new PXSetPropertyException(Messages.AccountTypeWarn, PXErrorLevel.Warning, GL.Messages.Asset));
	//        }
	//        if (account != null && AccountType == "I" && account.Type != "L")
	//        {
	//            sender.RaiseExceptionHandling<DeferredCode.accountID>(Row, account.AccountCD, new PXSetPropertyException(Messages.AccountTypeWarn, PXErrorLevel.Warning, GL.Messages.Liability));
	//        }
	//    }

	//    protected virtual void DeferredCode_AccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
	//    {
	//        CheckAccountType(sender, e.Row, (Int32?)e.NewValue, ((DeferredCode)e.Row).AccountType);
	//    }

	//    protected virtual void DeferredCode_AccountType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
	//    {
	//        CheckAccountType(sender, e.Row, null, (string)e.NewValue);
	//    }
	//}
}
