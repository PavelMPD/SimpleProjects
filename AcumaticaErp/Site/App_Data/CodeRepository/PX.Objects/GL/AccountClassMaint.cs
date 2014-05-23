using System;
using PX.Data;
using System.Collections;
using PX.Objects.CS;
using PX.CS;

namespace PX.Objects.GL
{
	public class AccountClassMaint : PXGraph<AccountClassMaint>
	{
        public PXSavePerRow<AccountClass> Save;
		public PXCancel<AccountClass> Cancel;
		[PXImport(typeof(AccountClass))]
		public PXSelect<AccountClass> AccountClassRecords;

		public AccountClassMaint()
		{
			if (Company.Current.BAccountID.HasValue == false)
			{
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(Branch), CS.Messages.BranchMaint);
			}
		}
		public PXSetup<Branch> Company;

		protected virtual void AccountClass_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			if (e.Row == null) return;

			AccountClass row = (AccountClass)e.Row;

			Account account_rec = (Account)PXSelect<Account, Where<Account.accountClassID, Equal<Required<AccountClass.accountClassID>>>>.SelectWindowed(this, 0, 1, row.AccountClassID);
			if (account_rec != null)
			{
				throw new PXException(Messages.ThisAccountClassMayNotBeDeletedBecauseItIsUsedIn, "Account: " + account_rec.AccountCD);
			}

			RMDataSource rmds_rec = PXSelect<RMDataSource, Where<RMDataSourceGL.accountClassID, Equal<Required<AccountClass.accountClassID>>>>.SelectWindowed(this, 0, 1, row.AccountClassID);
			if (rmds_rec != null)
			{
				throw new PXException(Messages.ThisAccountClassMayNotBeDeletedBecauseItIsUsedIn, "Analitycal Report Manager");
			}
		}
	}
}
