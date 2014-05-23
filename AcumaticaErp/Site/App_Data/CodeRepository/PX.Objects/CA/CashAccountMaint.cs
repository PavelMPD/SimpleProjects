using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.AP;
using PX.Objects.CM;

namespace PX.Objects.CA
{
    [Serializable]
    public class CashAccountMaint : PXGraph<CashAccountMaint, CashAccount>
	{
        #region Internal Types
        
        [Serializable]
        [PXProjection(typeof(Select<PaymentMethodAccount>), Persistent = false)]
        public partial class PaymentMethodAccount2 : IBqlTable
        {
            #region PaymentMethodID
            public abstract class paymentMethodID : PX.Data.IBqlField
            {
            }
            protected String _PaymentMethodID;
            [PXDBString(10, IsUnicode = true, IsKey = true, BqlField = typeof(PaymentMethodAccount.paymentMethodID))]
            [PXDefault(typeof(PaymentMethod.paymentMethodID))]            
            [PXSelector(typeof(Search<PaymentMethod.paymentMethodID>))]
            [PXUIField(DisplayName = "Payment Method")]
            public virtual String PaymentMethodID
            {
                get
                {
                    return this._PaymentMethodID;
                }
                set
                {
                    this._PaymentMethodID = value;
                }
            }
            #endregion
            #region CashAccountID
            public abstract class cashAccountID : PX.Data.IBqlField
            {
            }
            protected Int32? _CashAccountID;
            
            [PXDBInt(BqlField = typeof(PaymentMethodAccount.cashAccountID), IsKey=true)]
            [PXDefault()]
            public virtual Int32? CashAccountID
            {
                get
                {
                    return this._CashAccountID;
                }
                set
                {
                    this._CashAccountID = value;
                }
            }
            #endregion            
            #region UseForAP
            public abstract class useForAP : PX.Data.IBqlField
            {
            }
            protected Boolean? _UseForAP;
            [PXDBBool(BqlField = typeof(PaymentMethodAccount.useForAP))]
            [PXDefault(true)]
            [PXUIField(DisplayName = "Use in AP")]
            public virtual Boolean? UseForAP
            {
                get
                {
                    return this._UseForAP;
                }
                set
                {
                    this._UseForAP = value;
                }
            }
            #endregion
        }

        [Flags]
        enum CashAccountOptions
        {
            None = 0x0,
            HasPTSettings = 0x1,
            HasPTInstances = 0x2
        }
        #endregion

        #region Ctor + Public Selects
        public CashAccountMaint()
        {
            GLSetup setup = GLSetup.Current;
            CASetup setup1 = this.casetup.Current;
        }

        [PXDBDefault(typeof(CashAccount.cashAccountID))]
        [PXDBInt(IsKey = true)]
        [PXParent(typeof(Select<CashAccount, Where<CashAccount.cashAccountID, Equal<Current<PaymentMethodAccount.cashAccountID>>>>))]
        protected virtual void PaymentMethodAccount_CashAccountID_CacheAttached(PXCache sender)
        {
        }


        #region Action Buttons

        public PXAction<CashAccount> viewPTInstance;

        [PXUIField(DisplayName = "View Instance", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
        public virtual IEnumerable ViewPTInstance(PXAdapter adapter)
        {
            if (this.PTInstances.Current != null)
            {
                PaymentTypeInstanceMaint graph = PXGraph.CreateInstance<PaymentTypeInstanceMaint>();
                graph.PaymentTypeInstance.Current = this.PTInstances.Current;
                throw new PXPopupRedirectException(graph, "Corporate Card Definition", true);
                //throw new PXRedirectRequiredException(graph, "Current PTInstance");
            }
            return adapter.Get();
        }

        public PXAction<CashAccount> addPTInstance;

        [PXUIField(DisplayName = "Add Instance", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
        public virtual IEnumerable AddPTInstance(PXAdapter adapter)
        {
            CashAccount cashAcct = this.CashAccount.Current;
            if (cashAcct != null && (this.CashAccount.Cache.GetStatus(cashAcct) != PXEntryStatus.Inserted))
            {
                PaymentTypeInstanceMaint graph = PXGraph.CreateInstance<PaymentTypeInstanceMaint>();
                PaymentTypeInstance ptInstance = new PaymentTypeInstance();
                ptInstance.CashAccountID = cashAcct.CashAccountID;
                PaymentMethodAccount detail = null;
                foreach (PXResult<PaymentMethodAccount, PaymentMethod> iDet in this.Details.Select())
                {
                    PaymentMethod pt = (PaymentMethod)iDet;
                    PaymentMethodAccount dt = (PaymentMethodAccount)iDet;
                    if ((bool)pt.APAllowInstances)
                    {
                        if (detail == null || ((bool)dt.APIsDefault))
                            detail = dt;
                        if ((bool)detail.APIsDefault)
                            break;
                    }
                }
                if (detail != null)
                    ptInstance.PaymentMethodID = detail.PaymentMethodID;
                ptInstance = graph.PaymentTypeInstance.Insert(ptInstance);
                throw new PXPopupRedirectException(graph, "Corporate Card Definition", true);
                //throw new PXRedirectRequiredException(graph, "Current PTInstance");
            }
            return adapter.Get();
        }

        #endregion

        public PXSelect<Account, Where<Account.accountID, Equal<Optional<CashAccount.accountID>>, And<Match<Current<AccessInfo.userName>>>>> Account_AccountID;
        public PXSelect<CashAccount, Where<Match<Current<AccessInfo.userName>>>> CashAccount;
        public PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Current<CashAccount.cashAccountID>>>
            > CurrentCashAccount;
        public PXSelectJoin<PaymentMethodAccount, InnerJoin<PaymentMethod,
                On<PaymentMethod.paymentMethodID, Equal<PaymentMethodAccount.paymentMethodID>>>,
                Where<PaymentMethodAccount.cashAccountID, Equal<Current<CashAccount.cashAccountID>>>
            > Details;
        public PXSelectJoin<CashAccountETDetail,
                    InnerJoin<CAEntryType, On<CAEntryType.entryTypeId, Equal<CashAccountETDetail.entryTypeID>>>,
                        Where<CashAccountETDetail.accountID, Equal<Current<CashAccount.cashAccountID>>>> ETDetails;
        public PXSelect<CAEntryType> EntryTypes;
        public PXSelect<CashAccountDeposit, Where<CashAccountDeposit.accountID, Equal<Current<CashAccount.cashAccountID>>>> Deposits;
    
        public PXSelect<PaymentMethodAccount2, Where<PaymentMethodAccount2.cashAccountID, Equal<Current2<CashAccount.cashAccountID>>>> PaymentMethodForRemittance;

        public PXSelectJoin<CashAccountPaymentMethodDetail, InnerJoin<PaymentMethodDetail,
                               On<PaymentMethodDetail.paymentMethodID, Equal<CashAccountPaymentMethodDetail.paymentMethodID>,
                               And<PaymentMethodDetail.detailID, Equal<CashAccountPaymentMethodDetail.detailID>,
                               And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
                               Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>>,
                               Where<CashAccountPaymentMethodDetail.accountID, Equal<Optional2<CashAccount.cashAccountID>>,
                               And<CashAccountPaymentMethodDetail.paymentMethodID, Equal<Optional2<PaymentMethodAccount2.paymentMethodID>>>>,
                                     OrderBy<Asc<PaymentMethodDetail.orderIndex>>> PaymentDetails;

        public PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Optional2<PaymentMethodAccount.paymentMethodID>>,
                                And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
                                    Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>> PaymentTypeDetails;
        public PXSelectReadonly2<PaymentTypeInstance, LeftJoin<CR.BAccount, On<CR.BAccount.bAccountID, Equal<PaymentTypeInstance.bAccountID>>>, Where<PaymentTypeInstance.cashAccountID, Equal<Current<CashAccount.cashAccountID>>>> PTInstances;
        public PXSetup<Company> Company;
        public PXSetup<GLSetup> GLSetup;
        public PXSetup<CASetup> casetup;
        public CMSetupSelect cmsetup;
        #endregion

        public IEnumerable paymentMethodForRemittance() 
        {
            PXCache cache = this.Caches[typeof(PaymentMethodAccount2)];
            cache.AllowDelete = true;
            cache.AllowInsert = true;
            cache.Clear();
            foreach (PaymentMethodAccount it in PXSelect<PaymentMethodAccount, Where<PaymentMethodAccount.useForAP, Equal<True>, And<PaymentMethodAccount.cashAccountID, Equal<Current2<CashAccount.cashAccountID>>>>>.Select(this))
            {

                PaymentMethodDetail pmDetail = PXSelectReadonly<PaymentMethodDetail,
                                                    Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
                                                    And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
                                                    Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>.Select(this, it.PaymentMethodID);
                if (pmDetail == null || String.IsNullOrEmpty(pmDetail.PaymentMethodID) == true) continue;
                PaymentMethodAccount2 row2 = new PaymentMethodAccount2();
                row2.PaymentMethodID = it.PaymentMethodID;
                row2.CashAccountID = it.CashAccountID;
                row2.UseForAP = it.UseForAR;               
                cache.Insert(row2);
                string message;
                if(remittancePMErrors.TryGetValue(row2.PaymentMethodID, out message))
                {
                    cache.RaiseExceptionHandling<PaymentMethodAccount2.paymentMethodID>(row2, row2.PaymentMethodID,new PXSetPropertyException(message));
                    remittancePMErrors.Remove(row2.PaymentMethodID);
                }
                yield return row2;
            }
            cache.AllowDelete = false;
            cache.AllowInsert = false;
            cache.AllowUpdate = false;
            Caches[typeof(PaymentMethodAccount2)].IsDirty = false;
        }

        #region Internal Flags
        private bool isPaymentMergedFlag = false;        
        #endregion

        #region CashAccount Events
        protected virtual void CashAccount_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        { 
            CashAccount cashacct = e.Row as CashAccount;
            if (cashacct != null)
            {
                if (CheckCashAccountForTransactions(cashacct))
                {
                    throw new PXException(Messages.ERR_CashAccountHasTransactions_DeleteForbidden);
                }

                if (IsCashAccountInUse(cashacct))
                {
                    throw new PXException(Messages.ERR_CashAccountIsUsed_DeleteForbidden);
                }
            }
        }

        protected virtual void CashAccount_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            CashAccount cashacct = this.CashAccount.Current;
            
            using (PXConnectionScope cs = new PXConnectionScope())
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    PXDatabase.Delete<CashAccountCheck>(new PXDataFieldRestrict("AccountID", PXDbType.Int, 4, cashacct.CashAccountID, PXComp.EQ));                    
                    ts.Complete();
                }
            }
        }

        protected virtual void CashAccount_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            PXUIFieldAttribute.SetEnabled<CashAccount.curyID>(Caches[typeof(CashAccount)], null, false);
            PXUIFieldAttribute.SetVisible<CashAccount.curyRateTypeID>(sender, null, (bool)cmsetup.Current.MCActivated);            
            CashAccount row = (CashAccount)e.Row;
            if (row == null)
                return;


            this.RecalcOptions(row);
            PXUIFieldAttribute.SetEnabled<CashAccount.reconcile>(sender, row, !(bool)row.ClearingAccount);
            PXUIFieldAttribute.SetEnabled<CashAccount.reconNumberingID>(sender, row, !(bool)row.ClearingAccount);
            PXUIFieldAttribute.SetEnabled<CashAccount.referenceID>(sender, row, !(bool)row.ClearingAccount);
            //PXUIFieldAttribute.SetEnabled<CashAccount.clearingAccount>(sender, row, (row.ReferenceID == null));
            bool usedAsClearing = false;
            bool isClearing = (bool)row.ClearingAccount;
            if (isClearing)
            {
                CashAccountDeposit reference = PXSelect<CashAccountDeposit, Where<CashAccountDeposit.depositAcctID, Equal<Required<CashAccountDeposit.depositAcctID>>>>.Select(this, row.CashAccountID);
                usedAsClearing = (reference != null);
            }
            PXUIFieldAttribute.SetEnabled<CashAccount.clearingAccount>(sender, row, (!usedAsClearing) || (!isClearing));
            bool stmtImportToSingleAccount = this.casetup.Current.ImportToSingleAccount ?? false;
            PXUIFieldAttribute.SetVisible<CashAccount.statementImportTypeName>(sender, row, stmtImportToSingleAccount);
            this.Deposits.Cache.AllowInsert = !(bool)row.ClearingAccount;
            this.Deposits.Cache.AllowUpdate = !(bool)row.ClearingAccount;
            //***this.Details.Cache.AllowInsert = this.Details.Cache.AllowUpdate = !(bool)row.ClearingAccount;
	        bool hasCATransactions = CheckCashAccountForTransactions(row);
			PXUIFieldAttribute.SetEnabled<CashAccount.accountID>(sender, row, !hasCATransactions);
			PXUIFieldAttribute.SetEnabled<CashAccount.subID>(sender, row, !hasCATransactions);
			PXUIFieldAttribute.SetEnabled<CashAccount.branchID>(sender, row, !hasCATransactions);
        }

        protected virtual void CashAccount_AccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            CashAccount cashacc = (CashAccount)e.Row;
            if (e.NewValue != null)
            {
                Account acct = Account_AccountID.SelectSingle((int)e.NewValue);
                if (String.IsNullOrEmpty(acct.CuryID))
                {
                    if ((bool)cmsetup.Current.MCActivated)
                    {
                        throw new PXException(Messages.CashAccount_MayBeCreatedFromDenominatedAccountOnly);
                    }
                    else
                    {
                        cashacc.CuryID = Company.Current.BaseCuryID;
                        cashacc.BranchID = this.Accessinfo.BranchID;
                        acct = PXCache<Account>.CreateCopy(acct);
                        acct.CuryID = Company.Current.BaseCuryID;
                        Account_AccountID.Update(acct);
                    }
                }
                else
                {
                    cashacc.CuryID = acct.CuryID;
                    cashacc.BranchID = this.Accessinfo.BranchID;
                }
                cashacc.Descr = acct.Description;
            }
        }       

        protected virtual void CashAccount_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            CashAccount cashacct = e.Row as CashAccount;
            if (cashacct.ReconNumberingID == null && cashacct.Reconcile == true)
            {
                sender.RaiseExceptionHandling<CashAccount.reconNumberingID>(cashacct, cashacct.ReconNumberingID, new PXSetPropertyException(Messages.RequiresReconNumbering, "ReconNumbering"));
            }
         }

        protected virtual void CashAccount_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            CashAccount cashacct = e.Row as CashAccount;
            if (cashacct == null) return;

			if (cashacct.ReconNumberingID == null && cashacct.Reconcile == true)
            {
                sender.RaiseExceptionHandling<CashAccount.reconNumberingID>(cashacct, cashacct.ReconNumberingID, new PXSetPropertyException(Messages.RequiresReconNumbering, "ReconNumbering"));
            } 
            CashAccount otherCashAcc = PXSelect<CashAccount, Where<CashAccount.accountID, Equal<Current<CashAccount.accountID>>,
                And<CashAccount.subID, Equal<Current<CashAccount.subID>>, And<CashAccount.branchID, Equal<Current<CashAccount.branchID>>,
                And<CashAccount.cashAccountID, NotEqual<Current<CashAccount.cashAccountID>>>>>>>.Select(this);
            if (otherCashAcc != null)
            {
                throw new PXException(Messages.CashAccountExist);
            }

            if (cashacct.CashAccountID != null && cashacct.CashAccountID != -1)
            {
                PXEntryStatus status = this.CashAccount.Cache.GetStatus(e.Row);
                if (status != PXEntryStatus.InsertedDeleted && status != PXEntryStatus.Deleted)
                {
                    bool isInserted = (status == PXEntryStatus.Inserted);
                    if (isInserted)
                    {
                        if (HasGLTrans(cashacct.AccountID, cashacct.SubID, cashacct.BranchID))
                        {
                            sender.RaiseExceptionHandling<CashAccount.cashAccountCD>(e.Row, cashacct.CashAccountCD, new PXSetPropertyException(Messages.GLTranExistForThisCashAcct, PXErrorLevel.Warning));
                        }
                    }
                }
            }
            #region Validate CashPaymentTypeDetails for Required fields
            /*Dictionary<string, string> failedPTypes = new Dictionary<string, string>(); 
            foreach( CashAccountPaymentMethodDetail iDet in this.PaymentDetails.Cache.Inserted) 
            {
                CashAccountPaymentMethodDetail detail = iDet; 
                if (!ValidateDetail(iDet))
                {
                    failedPTypes[detail.PaymentMethodID] = detail.PaymentMethodID; 
                    e.Cancel = true;
                }
            }
            foreach (CashAccountPaymentMethodDetail iDet in this.PaymentDetails.Cache.Updated)
            {
                CashAccountPaymentMethodDetail detail = iDet;
                if (!ValidateDetail(iDet))
                {
                    failedPTypes[detail.PaymentMethodID] = detail.PaymentMethodID;
                    e.Cancel = true;
                }
            }
            if (failedPTypes.Count > 0) 
            {
                if (!failedPTypes.ContainsKey(cashacct.PaymentMethodID)) 
                {
                    foreach (string key in failedPTypes.Keys)
                    {
                        cashacct.PaymentMethodID = key;
                        break;
                    }
                }
            }	*/
            #endregion
        }

        protected virtual void CashAccount_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            CashAccount cashacct = e.Row as CashAccount;
            Account acc = Account_AccountID.SelectSingle(cashacct.AccountID);
            bool lastCashAcc = (PXSelect<CashAccount, Where<CashAccount.accountID, Equal<Required<CashAccount.accountID>>>>.Select(this, cashacct.AccountID).Count == 0);
            if (acc != null && acc.IsCashAccount != !lastCashAcc)
            {
                acc = PXCache<Account>.CreateCopy(acc);
                acc.IsCashAccount = !lastCashAcc;
                Account_AccountID.Update(acc);
            }
        }

        protected virtual void CashAccount_ClearingAccount_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CashAccount row = (CashAccount)e.Row;
            if (row.ClearingAccount == true)
            {
                row.Reconcile = false;
                row.ReconNumberingID = null;
                foreach (CashAccountDeposit it in this.Deposits.Select())
                {
                    this.Deposits.Delete(it);
                }                
            }
        }

        protected virtual void CashAccount_ClearingAccount_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            CashAccount row = (CashAccount)e.Row;
            if ((bool)e.NewValue == true)
            {
                bool hasDeposits = this.Deposits.Select().Count > 0;
                bool hasPaymentType = this.Details.Select().Count > 0;
                bool hasAP = false;
                /*foreach (PaymentMethodAccount pm in this.Details.Select())
                {
                    if (pm.UseForAP.GetValueOrDefault(false))
                        hasAP = true;
                }*/
                if (row.ReferenceID != null || hasDeposits || hasAP)
                {
                    e.NewValue = false;
                    throw new PXSetPropertyException(Messages.CashAccountMayNotBeMadeClearingAccount, PXErrorLevel.Error);
                }
            }
        }
        #endregion

        #region Detail Events

        protected virtual void PaymentMethodAccount_UseForAP_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
            if (row != null)
            {
                CashAccount cx = CurrentCashAccount.Select();
                if (cx.ClearingAccount.GetValueOrDefault(false))
                {
                    row.UseForAP = false;
                }
                else if (String.IsNullOrEmpty(row.PaymentMethodID) == false)
                {
                    PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, row.PaymentMethodID);
                    row.UseForAP = (pm != null && pm.UseForAP == true);
                    e.Cancel = true;
                }
            }
        }

        protected virtual void PaymentMethodAccount_UseForAR_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
            if (row != null && String.IsNullOrEmpty(row.PaymentMethodID) == false)
            {
                PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, row.PaymentMethodID);
                row.UseForAR = (pm != null && pm.UseForAR == true);
                e.Cancel = true;
            }
        }

        protected virtual void PaymentMethodAccount_BatchLastRefNbr_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
            if (row != null && String.IsNullOrEmpty(row.PaymentMethodID) == false)
            {
                PaymentMethod pt = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, row.PaymentMethodID);
                if (pt.APCreateBatchPayment == true)
                {
                    e.NewValue = "00000000";
                    e.Cancel = true;
                }
                else
                {
                    e.NewValue = null;
                    e.Cancel = true;
                }
            }
        }

        protected virtual void PaymentMethodAccount_PaymentMethodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
            if (row != null && String.IsNullOrEmpty(row.PaymentMethodID) == false)
            {
                cache.SetDefaultExt<PaymentMethodAccount.aPBatchLastRefNbr>(e.Row);
            }
        }

        protected virtual void PaymentMethodAccount_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            CashAccount acct = this.CashAccount.Current;
            PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
            if (acct != null && (this.CashAccount.Cache.GetStatus(acct) == PXEntryStatus.Notchanged))
            {
                this.CashAccount.Cache.SetStatus(acct, PXEntryStatus.Updated);
            }           
            PaymentDetails.View.RequestRefresh();
        }

        protected virtual void PaymentMethodAccount_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
        {
            PaymentMethodAccount oldRow = (PaymentMethodAccount)e.Row;
            PaymentMethodAccount newRow = (PaymentMethodAccount)e.NewRow;
            if (newRow == null) return;
            if (oldRow.PaymentMethodID != newRow.PaymentMethodID)
            {
                foreach (PaymentMethodAccount it in Details.Select())
                {
                    if (!object.ReferenceEquals(oldRow, it) && it.PaymentMethodID == newRow.PaymentMethodID)
                    {
                        throw new PXSetPropertyException("PaymentMethodID" + Messages.PaymentTypeNotValid);
                    }
                }
            }
        }

        protected virtual void PaymentMethodAccount_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
            if (row == null) return;

            if (row.PaymentMethodID != null)
            {
                foreach (PaymentMethodAccount it in Details.Select())
                {
                    if (!object.ReferenceEquals(row, it) && it.PaymentMethodID == row.PaymentMethodID)
                    {
                        throw new PXSetPropertyException(Messages.DuplicatedPaymentMethodForCashAccount, row.PaymentMethodID);
                    }
                }
            }

            if (row.APIsDefault == true && String.IsNullOrEmpty(row.PaymentMethodID))
            {
                throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(PaymentMethodAccount.paymentMethodID).Name);
            }
        }

        protected virtual void PaymentMethodAccount_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            CashAccount acct = this.CashAccount.Current;
            PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
            if (row != null && String.IsNullOrEmpty(row.PaymentMethodID) == false)
            {
                PaymentMethod pt = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, row.PaymentMethodID);
                bool enabled = (pt != null) && pt.APCreateBatchPayment == true;
                PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPBatchLastRefNbr>(cache, row, enabled);
            }
            //PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.useForAP>(cache, row, !acct.ClearingAccount.GetValueOrDefault(false));
            //***PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.useForAR>(cache, row, false);
            bool EnableAP = (row != null && row.UseForAP == true);
            bool EnableAR = (row != null && row.UseForAR == true);
            PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPIsDefault>(cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPAutoNextNbr>(cache, e.Row, EnableAP);
            PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPLastRefNbr>(cache, e.Row, EnableAP);
            PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aRIsDefault>(cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aRIsDefaultForRefund>(cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aRAutoNextNbr>(cache, e.Row, EnableAR);
            PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aRLastRefNbr>(cache, e.Row, EnableAR);
            if(row!=null)
            {
                PXEntryStatus status = cache.GetStatus(e.Row);
                if (status != PXEntryStatus.Deleted && status != PXEntryStatus.InsertedDeleted)
                {
                    isPaymentMergedFlag = false;
                    this.FillPaymentDetails(row.PaymentMethodID);
                }
            }            
        }
        
        protected virtual void PaymentMethodAccount2_RowPersisting(PXCache sender, PXRowPersistingEventArgs e) 
        {
            e.Cancel = true;
        }
        #endregion

        #region ETDetail Events
        protected virtual void CashAccountETDetail_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            CAEntryType caEntryType = PXSelect<CAEntryType,
                Where<CAEntryType.entryTypeId, Equal<Required<CAEntryType.entryTypeId>>>>.
                SelectWindowed(this, 0, 1, ((CashAccountETDetail)e.Row).EntryTypeID);
            if (caEntryType == null)
            {
                cache.RaiseExceptionHandling<CashAccountETDetail.entryTypeID>(e.Row, ((CashAccountETDetail)e.Row).EntryTypeID, new PXException(Messages.EntryTypeIDDoesNotExist));
                e.Cancel = true;
            }
        }

        protected virtual void CashAccountETDetail_EntryTypeID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            CashAccountETDetail row = (CashAccountETDetail)e.Row;
            cache.SetDefaultExt<CashAccountETDetail.offsetAccountID>(e.Row);
            cache.SetDefaultExt<CashAccountETDetail.offsetSubID>(e.Row);
        }

        protected virtual void CashAccountETDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            CashAccountETDetail row = (CashAccountETDetail)e.Row;
            if (row != null)
            {
                bool usesCashAccount = false;
                CAEntryType et = (CAEntryType)PXSelectorAttribute.Select<CashAccountETDetail.entryTypeID>(sender, e.Row);
                if (et != null)
                {
                    if (row.OffsetAccountID.HasValue && row.OffsetSubID.HasValue)
                    {
                        CashAccount cashAcct = null;
                        Account acct = null;
                        cashAcct = PXSelectReadonly<CashAccount, Where<CashAccount.accountID, Equal<Required<CashAccount.accountID>>,
                            And<CashAccount.subID, Equal<Required<CashAccount.subID>>>>>.Select(this, row.OffsetAccountID, row.OffsetSubID);
                        acct = PXSelectReadonly<GL.Account, Where<GL.Account.accountID, Equal<Required<GL.Account.accountID>>>>.Select(this, row.OffsetAccountID);
                        usesCashAccount = (cashAcct != null);
                        if (et.UseToReclassifyPayments == true)
                        {
                            if (!usesCashAccount)
                            {
                                sender.RaiseExceptionHandling<CashAccountETDetail.offsetAccountID>(e.Row, acct.AccountCD, new PXSetPropertyException(Messages.CAEntryTypeUsedForPaymentReclassificationMustHaveCashAccount, PXErrorLevel.Error));
                            }
                            else
                            {
                                CashAccount current = this.CashAccount.Current;
                                if (current.CashAccountID == cashAcct.CashAccountID)
                                {
                                    sender.RaiseExceptionHandling<CashAccountETDetail.offsetAccountID>(e.Row, acct.AccountCD, new PXSetPropertyException(Messages.OffsetAccountMayNotBeTheSameAsCurrentAccount, PXErrorLevel.Error));
                                }
                                else if (cashAcct.CuryID != current.CuryID)
                                {
                                    sender.RaiseExceptionHandling<CashAccountETDetail.offsetAccountID>(e.Row, acct.AccountCD, new PXSetPropertyException(Messages.OffsetAccountForThisEntryTypeMustBeInSameCurrency, PXErrorLevel.Error));
                                }
                                else
                                {
                                    PaymentMethodAccount pmAccount = PXSelectReadonly2<PaymentMethodAccount, InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<PaymentMethodAccount.paymentMethodID>,
                                                                            And<PaymentMethod.isActive, Equal<True>>>>,
                                                                        Where<PaymentMethodAccount.cashAccountID, Equal<Required<PaymentMethodAccount.cashAccountID>>,
                                                                            And<Where<PaymentMethodAccount.useForAP, Equal<True>,
                                                                                    Or<PaymentMethodAccount.useForAR, Equal<True>>>>>>.Select(this, cashAcct.CashAccountID);

                                    if (pmAccount == null || pmAccount.CashAccountID.HasValue == false)
                                    {
                                        sender.RaiseExceptionHandling<CashAccountETDetail.offsetAccountID>(e.Row, acct.AccountCD, new PXSetPropertyException(Messages.EntryTypeCashAccountIsNotConfiguredToUseWithAnyPaymentMethod, PXErrorLevel.Warning));
                                    }
                                    else
                                    {
                                        sender.RaiseExceptionHandling<CashAccountETDetail.offsetAccountID>(e.Row, null, null);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        sender.RaiseExceptionHandling<CashAccountETDetail.offsetAccountID>(e.Row, null, null);
                    }
                    
                }
                PXUIFieldAttribute.SetEnabled<CashAccountETDetail.taxZoneID>(sender, e.Row, (et != null && et.Module == GL.BatchModule.CA));
                PXUIFieldAttribute.SetEnabled<CashAccountETDetail.offsetCashAccountID>(sender, e.Row, (et != null && et.UseToReclassifyPayments == true));
                PXUIFieldAttribute.SetEnabled<CashAccountETDetail.offsetAccountID>(sender, e.Row, (et != null && et.UseToReclassifyPayments != true));
                PXUIFieldAttribute.SetEnabled<CashAccountETDetail.offsetSubID>(sender, e.Row, (et != null && et.UseToReclassifyPayments != true));
            }
        }

        protected virtual void CashAccountETDetail_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            CashAccountETDetail row = (CashAccountETDetail)e.Row;
            bool usesCashAccount = false;
            CAEntryType et = (CAEntryType)PXSelectorAttribute.Select<CashAccountETDetail.entryTypeID>(sender, e.Row);
            if (et != null && et.UseToReclassifyPayments == true)
            {
                if (row.OffsetAccountID.HasValue && row.OffsetSubID.HasValue)
                {
                    CashAccount cashAcct = null;
                    Account acct = null;
                    cashAcct = PXSelectReadonly<CashAccount, Where<CashAccount.accountID, Equal<Required<CashAccount.accountID>>,
                        And<CashAccount.subID, Equal<Required<CashAccount.subID>>>>>.Select(this, row.OffsetAccountID, row.OffsetSubID);
                    acct = PXSelectReadonly<GL.Account, Where<GL.Account.accountID, Equal<Required<GL.Account.accountID>>>>.Select(this, row.OffsetAccountID);
                    usesCashAccount = (cashAcct != null);
                    if (!usesCashAccount)
                    {
                        if (sender.RaiseExceptionHandling<CashAccountETDetail.offsetAccountID>(e.Row, acct.AccountCD, new PXSetPropertyException(Messages.CAEntryTypeUsedForPaymentReclassificationMustHaveCashAccount, PXErrorLevel.Error)))
                        {
                            throw new PXRowPersistingException(typeof(CashAccountETDetail.offsetAccountID).Name, acct.AccountCD, Messages.CAEntryTypeUsedForPaymentReclassificationMustHaveCashAccount);
                        }
                    }
                    else
                    {
                        CashAccount current = this.CashAccount.Current;
                        if (current.CashAccountID == cashAcct.CashAccountID)
                        {
                            if (sender.RaiseExceptionHandling<CashAccountETDetail.offsetAccountID>(e.Row, acct.AccountCD, new PXSetPropertyException(Messages.OffsetAccountMayNotBeTheSameAsCurrentAccount, PXErrorLevel.Error)))
                            {
                                throw new PXRowPersistingException(typeof(CashAccountETDetail.offsetAccountID).Name, acct.AccountCD, Messages.OffsetAccountMayNotBeTheSameAsCurrentAccount);
                            }
                        }
                        if (cashAcct.CuryID != current.CuryID)
                        {
                            if (sender.RaiseExceptionHandling<CashAccountETDetail.offsetAccountID>(e.Row, acct.AccountCD, new PXSetPropertyException(Messages.OffsetAccountForThisEntryTypeMustBeInSameCurrency, PXErrorLevel.Error)))
                            {
                                throw new PXRowPersistingException(typeof(CashAccountETDetail.offsetAccountID).Name, acct.AccountCD, Messages.OffsetAccountForThisEntryTypeMustBeInSameCurrency);
                            }
                        }
                    }
                }
            }
        }

        protected virtual void CashAccountETDetail_OffsetCashAccountID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            CashAccountETDetail row = (CashAccountETDetail)e.Row;
            cache.SetDefaultExt<CashAccountETDetail.offsetAccountID>(e.Row);
            cache.SetDefaultExt<CashAccountETDetail.offsetSubID>(e.Row);
            cache.SetDefaultExt<CashAccountETDetail.offsetBranchID>(e.Row);
        }

        protected virtual void CashAccountETDetail_OffsetAccountID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            CashAccountETDetail row = (CashAccountETDetail)e.Row;
            if (row != null && row.OffsetCashAccountID != null)
            {
                CashAccount acct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.SelectWindowed(this, 0, 1, row.OffsetCashAccountID);
                if (acct != null)
                {
                    e.NewValue = acct.AccountID;
                    e.Cancel = true;
                }
            }
        }

        protected virtual void CashAccountETDetail_OffsetSubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            CashAccountETDetail row = (CashAccountETDetail)e.Row;
            if (row != null && row.OffsetCashAccountID != null)
            {
                CashAccount acct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.SelectWindowed(this, 0, 1, row.OffsetCashAccountID);
                if (acct != null)
                {
                    e.NewValue = acct.SubID;
                    e.Cancel = true;
                }
            }
        }

        protected virtual void CashAccountETDetail_OffsetBranchID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            CashAccountETDetail row = (CashAccountETDetail)e.Row;
            if (row != null && row.OffsetCashAccountID != null)
            {
                CashAccount acct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.SelectWindowed(this, 0, 1, row.OffsetCashAccountID);
                if (acct != null)
                {
                    e.NewValue = acct.BranchID;
                    e.Cancel = true;
                }
            }
        }

        #endregion

        #region Deposit Events
        protected virtual void CashAccountDeposit_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            CashAccountDeposit row = (CashAccountDeposit)e.Row;
        }

        protected virtual void CashAccountDeposit_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            CashAccountDeposit row = (CashAccountDeposit)e.Row;
        }

        protected virtual void CashAccountDeposit_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
        {
            CashAccountDeposit row = (CashAccountDeposit)e.Row;
        }

        protected virtual void CashAccountDeposit_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            CashAccountDeposit row = (CashAccountDeposit)e.Row;
        }
        #endregion


        #region CashAccountPaymentMethodDetail Events
        protected virtual void CashAccountPaymentMethodDetail_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                CashAccountPaymentMethodDetail row = (CashAccountPaymentMethodDetail)e.Row;
                if (row != null)
                {
                    PaymentMethodDetail iTempl = this.FindTemplate(row);
                    bool isRequired = (iTempl != null) && (iTempl.IsRequired ?? false);
                    PXDefaultAttribute.SetPersistingCheck<CashAccountPaymentMethodDetail.detailValue>(cache, row, (isRequired) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
                }
            }
        }

        protected virtual void CashAccountPaymentMethodDetail_DetailValue_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            CashAccountPaymentMethodDetail row = (CashAccountPaymentMethodDetail)e.Row;
            if (String.IsNullOrEmpty((string)e.NewValue))
                new PXSetPropertyException(Messages.ERR_RequiredValueNotEnterd);
        }

        #endregion

        #region Overrides

        public override void Clear()
        {
            base.Clear();
            isPaymentMergedFlag = false;
        }

        Dictionary<string, string> remittancePMErrors = new Dictionary<string, string>();
        public override void Persist()
        {
            #region Validate CashPaymentTypeDetails for Required fields        
            foreach (CashAccountPaymentMethodDetail iDet in this.PaymentDetails.Cache.Inserted)
            {
                CashAccountPaymentMethodDetail detail = iDet;
                if (!ValidateDetail(iDet))
                {
                    remittancePMErrors[detail.PaymentMethodID] = Messages.SomeRemittanceSettingsForCashAccountAreNotValid;                    
                }
            }
            foreach (CashAccountPaymentMethodDetail iDet in this.PaymentDetails.Cache.Updated)
            {
                CashAccountPaymentMethodDetail detail = iDet;
                if (!ValidateDetail(iDet))
                {
                    remittancePMErrors[detail.PaymentMethodID] = Messages.SomeRemittanceSettingsForCashAccountAreNotValid;                    
                }
            }
            if (remittancePMErrors.Count > 0)
            {                
                throw new PXException(Messages.SomeRemittanceSettingsForCashAccountAreNotValid);
            }
            #endregion
            using (PXTransactionScope ts = new PXTransactionScope())
            {               
                base.Persist();
                ts.Complete();
            }            
        }

        #endregion

        #region Utility Functions

        protected virtual void FillPaymentDetails(string aPaymentMethodID)
        {
            CashAccount account = this.CurrentCashAccount.Current;
            if (account != null)
            {
                int? accountID = account.CashAccountID;
                if (!isPaymentMergedFlag)
                {

                    if (!string.IsNullOrEmpty(aPaymentMethodID))
                    {
                        List<PaymentMethodDetail> toAdd = new List<PaymentMethodDetail>();
                        foreach (PaymentMethodDetail it in PXSelect<PaymentMethodDetail,
                                                            Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
                                                               And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
                                                                    Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>.
                                                                                                Select(this, aPaymentMethodID))
                        {
                            CashAccountPaymentMethodDetail detail = null;

                            foreach (CashAccountPaymentMethodDetail iPDet in this.PaymentDetails.Select(accountID, aPaymentMethodID))
                            {
                                if (iPDet.DetailID == it.DetailID)
                                {
                                    detail = iPDet;
                                    break;
                                }
                            }
                            if (detail == null)
                            {
                                toAdd.Add(it);
                            }
                        }
                        using (ReadOnlyScope rs = new ReadOnlyScope(this.PaymentDetails.Cache))
                        {
                            foreach (PaymentMethodDetail it in toAdd)
                            {
                                CashAccountPaymentMethodDetail detail = new CashAccountPaymentMethodDetail();
                                detail.AccountID = account.CashAccountID;
                                detail.PaymentMethodID = aPaymentMethodID;
                                detail.DetailID = it.DetailID;
                                detail = this.PaymentDetails.Insert(detail);
                            }
                            if (toAdd.Count > 0)
                            {
                                this.PaymentDetails.View.RequestRefresh();
                            }
                        }
                    }
                    this.isPaymentMergedFlag = true;
                }
            }

        }

        public virtual bool IsCashAccountInUse(CashAccount aAcct)
        {
            VendorClass vndClass = PXSelect<VendorClass, Where<VendorClass.cashAcctID, Equal<Required<VendorClass.cashAcctID>>>>.SelectWindowed(this, 0, 1, aAcct.CashAccountID);
            if (vndClass != null) return true;
            CR.Location vnd = PXSelect<CR.Location, Where<CR.Location.vCashAccountID, Equal<Required<CR.Location.vCashAccountID>>>>.SelectWindowed(this, 0, 1, aAcct.CashAccountID);
            if (vnd != null) return true;
            AR.CustomerPaymentMethod cpm = PXSelect<AR.CustomerPaymentMethod, Where<AR.CustomerPaymentMethod.cashAccountID, Equal<Required<AR.CustomerPaymentMethod.cashAccountID>>>>.SelectWindowed(this, 0, 1, aAcct.CashAccountID);
            if (cpm != null) return true;
            PaymentTypeInstance pmi = PXSelect<PaymentTypeInstance, Where<PaymentTypeInstance.cashAccountID, Equal<Required<PaymentTypeInstance.cashAccountID>>>>.Select(this, aAcct.CashAccountID);
            if (pmi != null) return true;
            return false;
        }

        public virtual bool HasGLTrans(int? aAccountID, int? subID, int? branchID)
        {
            if (aAccountID != null && subID != null && branchID != null)
            {
                GLTran glTranExist = PXSelect<GLTran, Where<GLTran.accountID, Equal<Required<GLTran.accountID>>, And<GLTran.subID, Equal<Required<GLTran.subID>>,
                    And<GLTran.branchID, Equal<Required<GLTran.branchID>>>>>>.SelectWindowed(this, 0, 1, aAccountID, subID, branchID);
                return (glTranExist != null) && (glTranExist.BatchNbr != null);
            }
            else
            {
                return false;
            }
        }


        public virtual bool CheckCashAccountForTransactions(CashAccount aAcct)
        {
            CATran ctr = PXSelect<CATran, Where<CATran.cashAccountID, Equal<Required<CATran.cashAccountID>>>>.SelectWindowed(this, 0, 1, aAcct.CashAccountID);
            if (ctr != null) return true;
            return false;
        }

        private void RecalcOptions(CashAccount aRow)
        {
            CashAccountOptions options = this.DetectOptions();
            bool cacheIsDirty = Caches[typeof(CashAccount)].IsDirty;
            aRow.PTInstancesAllowed = (options & CashAccountOptions.HasPTInstances) != 0;
            aRow.AcctSettingsAllowed = (options & CashAccountOptions.HasPTSettings) != 0;
            Caches[typeof(CashAccount)].IsDirty = cacheIsDirty;
        }
        private CashAccountOptions DetectOptions()
        {
            bool AcctSettingsAllowed = false;
            bool CardsAllowed = false;
            PaymentTypeInstance ptInstance = (PaymentTypeInstance)this.PTInstances.Select();
            CardsAllowed = (ptInstance != null);
            foreach (PXResult<PaymentMethodAccount, PaymentMethod> iPt in this.Details.Select())
            {
                PaymentMethod pt = (PaymentMethod)iPt;
                if (AcctSettingsAllowed == false)
                {
                    PaymentMethodDetail ptd = PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
                                                And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
                                                Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>.Select(this, pt.PaymentMethodID);
                    if (ptd != null)
                        AcctSettingsAllowed = true;
                }
                if (CardsAllowed == false && (bool)pt.APAllowInstances) CardsAllowed = true;
                if ((bool)CardsAllowed && (bool)AcctSettingsAllowed) break;
            }
            CashAccountOptions result = CardsAllowed ? CashAccountOptions.HasPTInstances : CashAccountOptions.None;
            if (AcctSettingsAllowed)
                result |= CashAccountOptions.HasPTSettings;
            return result;
        }

        protected virtual PaymentMethodDetail FindTemplate(CashAccountPaymentMethodDetail aDet)
        {
            PaymentMethodDetail res = PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
                    And<PaymentMethodDetail.detailID, Equal<Required<PaymentMethodDetail.detailID>>,
                    And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
                        Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>>.Select(this, aDet.PaymentMethodID, aDet.DetailID);
            return res;
        }

        protected virtual bool ValidateDetail(CashAccountPaymentMethodDetail aRow)
        {
            PaymentMethodDetail iTempl = this.FindTemplate(aRow);
            if (iTempl != null && (iTempl.IsRequired ?? false) && String.IsNullOrEmpty(aRow.DetailValue))
            {
                this.PaymentDetails.Cache.RaiseExceptionHandling<CashAccountPaymentMethodDetail.detailValue>(aRow, aRow.DetailValue, new PXSetPropertyException(Messages.ERR_RequiredValueNotEnterd));
                return false;
            }
            return true;
        }


        #endregion
    }
}
