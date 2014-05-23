using System;
using System.Collections;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.GL;

namespace PX.Objects.FA
{
    [PX.Objects.GL.TableAndChartDashboardType]
    public class FACostDetailsInq : PXGraph<FACostDetailsInq>
    {
        public PXCancel<AccountFilter> Cancel;
        public PXFilter<AccountFilter> Filter;
        public PXSelectJoin<Transact, LeftJoin<FABook, On<Transact.bookID, Equal<FABook.bookID>>>, Where<Transact.assetID, Equal<Current<AccountFilter.assetID>>, And<Transact.finPeriodID, LessEqual<Current<AccountFilter.periodID>>, And<FABook.updateGL, Equal<True>, And<Transact.released, Equal<True>, And<Where<Transact.debitAccountID, Equal<Current<AccountFilter.accountID>>, And<Transact.debitSubID, Equal<Current<AccountFilter.subID>>, Or<Transact.creditAccountID, Equal<Current<AccountFilter.accountID>>, And<Transact.creditSubID, Equal<Current<AccountFilter.subID>>>>>>>>>>>> Transactions;

        public FACostDetailsInq()
        {
            Transactions.Cache.AllowInsert = false;
            Transactions.Cache.AllowUpdate = false;
            Transactions.Cache.AllowDelete = false;
        }

        public virtual IEnumerable transactions(PXAdapter adapter)
        {
            AccountFilter filter = Filter.Current;
            if (filter == null) yield break;

            foreach (Transact tran in PXSelectJoin<Transact, LeftJoin<FABook, On<Transact.bookID, Equal<FABook.bookID>>>, Where<Transact.assetID, Equal<Current<AccountFilter.assetID>>, And<Transact.finPeriodID, LessEqual<Current<AccountFilter.periodID>>, And<FABook.updateGL, Equal<True>, And<Transact.released, Equal<True>, And<Where<Transact.debitAccountID, Equal<Current<AccountFilter.accountID>>, And<Transact.debitSubID, Equal<Current<AccountFilter.subID>>, Or<Transact.creditAccountID, Equal<Current<AccountFilter.accountID>>, And<Transact.creditSubID, Equal<Current<AccountFilter.subID>>>>>>>>>>>>.Select(this))
            {
                tran.CreditAmt = tran.TranAmt;
                if (tran.CreditAccountID == filter.AccountID && tran.CreditSubID == filter.SubID)
                {
                    tran.CreditAmt = tran.TranAmt;
                    tran.DebitAmt = 0m;
                }
                else if (tran.DebitAccountID == filter.AccountID && tran.DebitSubID == filter.SubID)
                {
                    tran.CreditAmt = 0m;
                    tran.DebitAmt = tran.TranAmt;
                }
                else continue;
                yield return tran;
            }
        }
    }

    [Serializable]
    public partial class AccountFilter : IBqlTable
    {
        #region PeriodID
        public abstract class periodID : IBqlField
        {
        }
        protected String _PeriodID;
        [PXUIField(DisplayName = "Through Period", Required = true)]
        [PXDBString(6)]
        [FinPeriodIDFormatting]
        [PXDefault]
        public virtual String PeriodID
        {
            get
            {
                return _PeriodID;
            }
            set
            {
                _PeriodID = value;
            }
        }
        #endregion
        #region AssetID
        public abstract class assetID : IBqlField
        {
        }
        protected int? _AssetID;
        [PXDBInt]
        [PXSelector(typeof(Search2<FixedAsset.assetID, InnerJoin<FADetails, On<FADetails.assetID, Equal<FixedAsset.assetID>>>, Where<FixedAsset.recordType, Equal<FARecordType.assetType>, And<FADetails.status, Equal<FixedAssetStatus.active>>>>),
            SubstituteKey = typeof(FixedAsset.assetCD),
            DescriptionField = typeof(FixedAsset.description))]
        [PXUIField(DisplayName = "Fixed Asset")]
        [PXDefault]
        public virtual int? AssetID
        {
            get
            {
                return _AssetID;
            }
            set
            {
                _AssetID = value;
            }
        }
        #endregion
        #region BookID
        public abstract class bookID : IBqlField
        {
        }
        protected int? _BookID;
        [PXDBInt]
        [PXSelector(typeof(FABook.bookID),
                    SubstituteKey = typeof(FABook.bookCode),
                    DescriptionField = typeof(FABook.description))]
        [PXDefault(typeof(Search<FABookBalance.bookID, Where<FABookBalance.assetID, Equal<Current<AccountFilter.assetID>>, And<FABookBalance.updateGL, Equal<True>>>>))]
        [PXUIField(DisplayName = "Book")]
        public virtual int? BookID
        {
            get
            {
                return _BookID;
            }
            set
            {
                _BookID = value;
            }
        }
        #endregion
        #region AccountID
        public abstract class accountID : IBqlField
        {
        }
        protected Int32? _AccountID;
        [Account(DisplayName = "Account", DescriptionField = typeof(Account.description))]
        [PXDefault]
        public virtual Int32? AccountID
        {
            get
            {
                return _AccountID;
            }
            set
            {
                _AccountID = value;
            }
        }
        #endregion
        #region SubID
        public abstract class subID : IBqlField
        {
        }
        protected Int32? _SubID;
        [SubAccount(DisplayName = "Subaccount")]
        [PXDefault]
        public virtual Int32? SubID
        {
            get
            {
                return _SubID;
            }
            set
            {
                _SubID = value;
            }
        }
        #endregion
    }

    [Serializable]
    public partial class Transact : FATran
    {
        #region DebitAmt
        public abstract class debitAmt : IBqlField
        {
        }
        protected decimal? _DebitAmt;
        [PXBaseCury]
        [PXUIField(DisplayName = "Debit")]
        [PXDefault]
        public virtual decimal? DebitAmt
        {
            get
            {
                return _DebitAmt;
            }
            set
            {
                _DebitAmt = value;
            }
        }
        #endregion
        #region CreditAmt
        public abstract class creditAmt : IBqlField
        {
        }
        protected decimal? _CreditAmt;
        [PXBaseCury]
        [PXUIField(DisplayName = "Credit")]
        [PXDefault]
        public virtual decimal? CreditAmt
        {
            get
            {
                return _CreditAmt;
            }
            set
            {
                _CreditAmt = value;
            }
        }
        #endregion
        #region AccountID
        public abstract class accountID : IBqlField
        {
        }
        protected Int32? _AccountID;
        [Account(DisplayName = "Account", DescriptionField = typeof(Account.description), IsDBField = false)]
        [PXDefault]
        public virtual Int32? AccountID
        {
            get
            {
                return _AccountID;
            }
            set
            {
                _AccountID = value;
            }
        }
        #endregion
        #region SubID
        public abstract class subID : IBqlField
        {
        }
        protected Int32? _SubID;
        [SubAccount(DisplayName = "Subaccount", IsDBField = false)]
        [PXDefault]
        public virtual Int32? SubID
        {
            get
            {
                return _SubID;
            }
            set
            {
                _SubID = value;
            }
        }
        #endregion
    }
}
