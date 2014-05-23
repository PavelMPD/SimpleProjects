using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CA;
using System.Collections;
using PX.Objects.CR;

namespace PX.Objects.GL
{
	public class AccountMaint : PXGraph<AccountMaint>
	{
        public PXSavePerRow<Account, Account.accountID> Save;
		public PXCancel<Account> Cancel;
		[PXImport(typeof(Account))]
		[PXFilterable]
		public PXSelect<Account,Where<Match<Current<AccessInfo.userName>>>, OrderBy<Asc<Account.accountCD>>> AccountRecords;

		public PXSelectReadonly<GLSetup> GLSetup;
		public GLSetup GLSETUP
		{
			get
			{
				GLSetup setup = GLSetup.Select();
				if (setup == null)
				{
					setup = new GLSetup();
					setup.COAOrder = (short)0;
				}
				return setup;
			}
		}
		public PXSetup<Company> Company;
		public CMSetupSelect cmsetup;

		protected bool? IsCOAOrderVisible = null;

		public AccountMaint()
		{
			if (string.IsNullOrEmpty(Company.Current.BaseCuryID))
			{
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(Company), CS.Messages.BranchMaint);
			}
			if (IsCOAOrderVisible == null)
			{
				IsCOAOrderVisible = (GLSetup.Current.COAOrder > 3);
				PXUIFieldAttribute.SetVisible<Account.cOAOrder>(AccountRecords.Cache, null, (bool) IsCOAOrderVisible);
				PXUIFieldAttribute.SetEnabled<Account.cOAOrder>(AccountRecords.Cache, null, (bool)IsCOAOrderVisible);
			}
			CMSetup record = cmsetup.Current;
			PXUIFieldAttribute.SetVisible<Account.curyID>(AccountRecords.Cache, null, (bool)record.MCActivated);
			PXUIFieldAttribute.SetEnabled<Account.curyID>(AccountRecords.Cache, null, (bool)record.MCActivated);

			PXUIFieldAttribute.SetVisible<Account.revalCuryRateTypeId>(AccountRecords.Cache, null, (bool)record.MCActivated);
			PXUIFieldAttribute.SetEnabled<Account.revalCuryRateTypeId>(AccountRecords.Cache, null, (bool)record.MCActivated);
		}

		protected virtual void Account_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Account row = (Account)e.Row;
			if (row != null && row.CuryID != null && row.CuryID != Company.Current.BaseCuryID && row.RevalCuryRateTypeId == null)
			{
				sender.RaiseExceptionHandling<Account.revalCuryRateTypeId>(row, null, new PXSetPropertyException(Messages.RevaluationRateTypeIsNotDefined, PXErrorLevel.Warning));
			}
		}

		protected virtual void Account_COAOrder_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (IsCOAOrderVisible == false && e.Row != null && string.IsNullOrEmpty(((Account)e.Row).Type) == false)
			{
				e.NewValue = Convert.ToInt16(AccountType.COAOrderOptions[(int)GLSetup.Current.COAOrder].Substring(AccountType.Ordinal(((Account)e.Row).Type), 1));
				e.Cancel = true;
			}
		}

		protected virtual void Account_COAOrder_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (IsCOAOrderVisible == false && e.Row != null && string.IsNullOrEmpty(((Account)e.Row).Type) == false)
			{
				e.NewValue = Convert.ToInt16(AccountType.COAOrderOptions[(int)GLSetup.Current.COAOrder].Substring(AccountType.Ordinal(((Account)e.Row).Type), 1));
			}
		}

		protected virtual void Account_Type_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (IsCOAOrderVisible == false)
			{
				sender.SetDefaultExt<Account.cOAOrder>(e.Row);
			}
		}

		protected virtual void Account_Type_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Account acct = e.Row as Account;
			if (acct.Active != null && acct.Type != (string)e.NewValue)
			{
				bool hasHistory = acct.AccountID != null && 
					GLUtility.IsAccountHistoryExist(this, acct.AccountID);
				if (hasHistory)
				{
					throw new PXSetPropertyException(Messages.AccountExistsType);
				}
			}	

		}

		protected virtual void Account_CuryId_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Account acct = e.Row as Account;

			if ((acct.CuryID != null) && (acct.CuryID != Company.Current.BaseCuryID))
			{
				acct.RevalCuryRateTypeId = cmsetup.Current.GLRateTypeReval;
			}
			else
			{
				acct.RevalCuryRateTypeId = null;
			}
		}

	    protected virtual void Account_RevalCuryRateTypeId_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Account acct = e.Row as Account;

			if (((string)e.NewValue != null) && ((acct.CuryID == null) || (acct.CuryID == Company.Current.BaseCuryID)))
			{
			  throw new PXSetPropertyException(Messages.AccountRevalRateTypefailed);
			}

		}

		protected virtual void Account_CuryID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Account acct = cache.Locate(e.Row) as Account;

			string newCuryID = (string)e.NewValue;
			if (acct != null && string.IsNullOrEmpty(acct.CuryID) && !string.IsNullOrEmpty(newCuryID))
			{
				if (PXSelect<CuryGLHistory, Where<CuryGLHistory.accountID, Equal<Current<Account.accountID>>, And<CuryGLHistory.curyID, NotEqual<Required<CuryGLHistory.curyID>>, And<CuryGLHistory.balanceType, NotEqual<LedgerBalanceType.report>>>>>.SelectSingleBound(this, new object[] { acct }, newCuryID).Count == 0)
				{
					return;
				}
			}

			if (acct != null && acct.CuryID != newCuryID)
			{
				bool hasHistory = GLUtility.IsAccountHistoryExist(this, acct.AccountID);
				if (hasHistory)
				{
					throw new PXSetPropertyException(Messages.AccountExistsCurrencyID);
				}
			}	

            if (acct != null && acct.IsCashAccount == true && string.IsNullOrEmpty(newCuryID))
            {
                throw new PXSetPropertyException(Messages.CannotClearCurrencyInCashAccount);
            }
		}
		
		protected virtual void Account_CuryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Account row = e.Row as Account;
			bool transactionsForGivenCuryExists = PXSelect<CuryGLHistory, Where<CuryGLHistory.accountID, Equal<Current<Account.accountID>>, And<CuryGLHistory.curyID, NotEqual<Required<CuryGLHistory.curyID>>, And<CuryGLHistory.balanceType, NotEqual<LedgerBalanceType.report>>>>>.SelectSingleBound(this, new object[] { e.Row }, row.CuryID).Count > 0;

			if (row != null && transactionsForGivenCuryExists)
			{
				throw new PXException(Messages.AccountExistsCurrencyID);
			}
		}

        protected virtual void Account_AccountClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            sender.SetDefaultExt<Account.type>(e.Row);
        }

	    protected virtual void Account_BranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (string.IsNullOrEmpty(((Account)e.Row).CuryID))
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void Account_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			string newCuryid;
			if (e.Operation == PXDBOperation.Update && !string.IsNullOrEmpty(newCuryid = ((Account)e.Row).CuryID))
			{
				byte[] timestamp = PXDatabase.SelectTimeStamp();

				PXDatabase.Update<GLHistory>(new PXDataFieldAssign("CuryID", newCuryid),
						new PXDataFieldRestrict("AccountID", ((Account)e.Row).AccountID),
						new PXDataFieldRestrict("CuryID", PXDbType.VarChar, 5, null, PXComp.ISNULL),
						new PXDataFieldRestrict("tstamp", PXDbType.Timestamp, 8, timestamp, PXComp.LE));
			}
		}

		public PXAction<Account> viewRestrictionGroups;
		[PXUIField(DisplayName = Messages.ViewRestrictionGroups, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewRestrictionGroups(PXAdapter adapter)
		{
			if (AccountRecords.Current != null)
			{
				GLAccessByAccount graph = CreateInstance<GLAccessByAccount>();
				graph.Account.Current = graph.Account.Search<Account.accountCD>(AccountRecords.Current.AccountCD);
				throw new PXRedirectRequiredException(graph, false, "Restricted Groups");
			}
			return adapter.Get();
		}

        public PXAction<Account> accountByPeriodEnq;
        [PXUIField(DisplayName = Messages.ViewAccountByPeriod, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable AccountByPeriodEnq(PXAdapter adapter)
        {
            if (AccountRecords.Current != null)
            {
                AccountHistoryByYearEnq graph = CreateInstance<AccountHistoryByYearEnq>();
                graph.Filter.Current.AccountID = AccountRecords.Current.AccountID;
                throw new PXRedirectRequiredException(graph, false, Messages.ViewAccountByPeriod);
            }
            return adapter.Get();
        }

	}
}
