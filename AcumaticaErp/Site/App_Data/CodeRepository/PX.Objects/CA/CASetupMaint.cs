using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.CA
{
	public class CASetupMaint : PXGraph<CASetupMaint>
	{
		public PXSelect<CASetup> CASetupRecord;
		public PXSave<CASetup> Save;
		public PXCancel<CASetup> Cancel;

		public CASetupMaint()
		{
			GLSetup setup = GLSetup.Current;
		}

		public PXSetup<GLSetup> GLSetup;
		public CM.CMSetupSelect CMSetup;
		public PXSelect<CASetupApproval> Approval;


		protected virtual void CASetup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CASetup row = (CASetup)e.Row;			
			PXUIFieldAttribute.SetVisible<CASetup.curyRateTypeID>(sender,  null, (bool)CMSetup.Current.MCActivated);
			PXUIFieldAttribute.SetEnabled<CASetup.holdEntry>(sender,  null, row == null || row.RequestApproval != true);
		}
		protected virtual void CASetup_RequestApproval_FieldUpdated(PXCache sedner, PXFieldUpdatedEventArgs e)
		{
			CASetup row = (CASetup)e.Row;
			if(row != null && row.RequestApproval == true)
				row.HoldEntry = true;					
		}
		protected virtual void CASetup_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			CASetup row = (CASetup) e.NewRow;
			if (e.NewRow == null || row == null || row.TransitAcctId == null) return;

			CashAccount cashAccount =
				PXSelect<CashAccount, Where<CashAccount.accountID, Equal<Required<CASetup.transitAcctId>>>>.Select(
					this, row.TransitAcctId);
			if (cashAccount == null) return;
			if (cashAccount.SubID != (int) row.TransitSubID)
			{
				Sub subAccount = PXSelect<Sub, Where<Sub.subID, Equal<Required<CASetup.transitSubID>>>>.Select(
					this, row.TransitSubID);

				sender.RaiseExceptionHandling<CASetup.transitSubID>(row, subAccount.SubCD,
				                                                    new PXSetPropertyException(Messages.WrongSubIdForCashAccount));
			}
		}
	}
}
