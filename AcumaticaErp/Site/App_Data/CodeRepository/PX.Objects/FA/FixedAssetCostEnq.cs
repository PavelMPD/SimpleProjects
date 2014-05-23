using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.GL;

namespace PX.Objects.FA
{
    [PX.Objects.GL.TableAndChartDashboardType]
    public class FixedAssetCostEnq : PXGraph<FixedAssetCostEnq>
    {
        public PXCancel<FixedAssetFilter> Cancel;
        public PXFilter<FixedAssetFilter> Filter;
        public PXSelect<Amounts> Amts;
            
        public FixedAssetCostEnq()
        {
            Amts.Cache.AllowInsert = false;
            Amts.Cache.AllowUpdate = false;
            Amts.Cache.AllowDelete = false;
        }

        public virtual IEnumerable amts(PXAdapter adapter)
        {
            FixedAssetFilter filter = Filter.Current;
            if(filter == null) yield break;

            FixedAsset asset = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FixedAssetFilter.assetID>>>>.Select(this);

            Dictionary<string, Amounts> dict = new Dictionary<string, Amounts>();
            foreach (PXResult<FATran, Account, Sub, CreditAccount, CreditSub, FABookPeriod> res in PXSelectJoin<FATran, LeftJoin<Account, On<FATran.debitAccountID, Equal<Account.accountID>>, LeftJoin<Sub, On<FATran.debitSubID, Equal<Sub.subID>>, LeftJoin<CreditAccount, On<FATran.creditAccountID, Equal<CreditAccount.accountID>>, LeftJoin<CreditSub, On<FATran.creditSubID, Equal<CreditSub.subID>>, LeftJoin<FABookPeriod, On<FATran.bookID, Equal<FABookPeriod.bookID>, And<FATran.finPeriodID, Equal<FABookPeriod.finPeriodID>>>, LeftJoin<FABook, On<FATran.bookID, Equal<FABook.bookID>>>>>>>>, Where<FATran.assetID, Equal<Current<FixedAssetFilter.assetID>>, And<FATran.finPeriodID, LessEqual<Current<FixedAssetFilter.periodID>>, And<FATran.released, Equal<True>, And<FABook.updateGL, Equal<True>, And<Where<FATran.tranType, Equal<FATran.tranType.purchasingPlus>, Or<FATran.tranType, Equal<FATran.tranType.purchasingMinus>, Or<FATran.tranType, Equal<FATran.tranType.depreciationPlus>, Or<FATran.tranType, Equal<FATran.tranType.depreciationMinus>, Or<FATran.tranType, Equal<FATran.tranType.transferPurchasing>, Or<FATran.tranType, Equal<FATran.tranType.transferDepreciation>>>>>>>>>>>>>.Select(this))
            {
                Account dacct = res;
                Sub dsub = res;
                CreditAccount cacct = res;
                CreditSub csub = res;
                FATran tran = res;
                FABookPeriod period = res;

                if (tran.DebitAccountID != asset.FAAccrualAcctID || tran.DebitSubID != asset.FAAccrualSubID)
                {
                    Amounts record = null;
                    string dkey = string.Format("{0}{1}", dacct.AccountCD, dsub.SubCD);
                    if (!dict.TryGetValue(dkey, out record))
                    {
                        record = new Amounts
                                     {
                                         AccountID = tran.DebitAccountID,
                                         SubID = tran.DebitSubID,
                                         AcctDescr = dacct.Description,
                                         SubDescr = dsub.Description,
                                         ItdAmt = 0m,
                                         YtdAmt = 0m,
                                         PtdAmt = 0m
                                     };
                    }
                    record.ItdAmt += tran.TranAmt;
                    if (filter.PeriodID.Substring(0, 4) == period.FinYear)
                    {
                        record.YtdAmt += tran.TranAmt;
                    }
                    if (filter.PeriodID == tran.FinPeriodID)
                    {
                        record.PtdAmt += tran.TranAmt;
                    }
                    dict[dkey] = record;
                }

                if (tran.CreditAccountID != asset.FAAccrualAcctID || tran.CreditSubID != asset.FAAccrualSubID)
                {
                    Amounts record = null;
                    string ckey = string.Format("{0}{1}", cacct.AccountCD, csub.SubCD);
                    if (!dict.TryGetValue(ckey, out record))
                    {
                        record = new Amounts
                                     {
                                         AccountID = tran.CreditAccountID,
                                         SubID = tran.CreditSubID,
                                         AcctDescr = cacct.Description,
                                         SubDescr = csub.Description,
                                         ItdAmt = 0m,
                                         YtdAmt = 0m,
                                         PtdAmt = 0m
                                     };
                    }
                    record.ItdAmt -= tran.TranAmt;
                    if (filter.PeriodID.Substring(0, 4) == period.FinYear)
                    {
                        record.YtdAmt -= tran.TranAmt;
                    }
                    if (filter.PeriodID == tran.FinPeriodID)
                    {
                        record.PtdAmt -= tran.TranAmt;
                    }
                    dict[ckey] = record;
                }
            }

            foreach (Amounts amt in dict.Values)
            {
                if (amt.ItdAmt != 0m || amt.YtdAmt != 0m || amt.PtdAmt != 0m)
                    yield return amt;
            }
        }

        public PXAction<FixedAssetFilter> viewDetails;
        [PXUIField(DisplayName = Messages.ViewDetails, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable ViewDetails(PXAdapter adapter)
        {
            Amounts amt = Amts.Current;
            FixedAssetFilter filter = Filter.Current;
            if (amt != null && filter != null)
            {
                FACostDetailsInq graph = CreateInstance<FACostDetailsInq>();
                graph.Filter.Insert(new AccountFilter(){AssetID = filter.AssetID, PeriodID = filter.PeriodID, AccountID = amt.AccountID, SubID = amt.SubID});
                graph.Filter.Cache.IsDirty = false;
                throw new PXRedirectRequiredException(graph, true, "ViewDetails"){Mode = PXBaseRedirectException.WindowMode.Same};
            }
            return adapter.Get();
        }

        [Serializable]
        public partial class FixedAssetFilter : IBqlTable
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
        }

        [Serializable]
        public partial class Amounts : IBqlTable
        {
            #region AccountID
            public abstract class accountID : IBqlField
            {
            }
            protected Int32? _AccountID;
            [Account(IsKey = true, DisplayName = "Account", DescriptionField = typeof(Account.description))]
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
            #region AcctDescr
            public abstract class acctDescr : IBqlField
            {
            }
            protected String _AcctDescr;
            [PXDBString(60, IsUnicode = true)]
            [PXUIField(DisplayName = "Account Description")]
            public virtual String AcctDescr
            {
                get
                {
                    return _AcctDescr;
                }
                set
                {
                    _AcctDescr = value;
                }
            }
            #endregion
            #region SubID
            public abstract class subID : IBqlField
            {
            }
            protected Int32? _SubID;
            [SubAccount(IsKey = true, DisplayName = "Subaccount")]
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
            #region SubDescr
            public abstract class subDescr : IBqlField
            {
            }
            protected String _SubDescr;
            [PXDBString(255, IsUnicode = true)]
            [PXUIField(DisplayName = "Subaccount Description", FieldClass = SubAccountAttribute.DimensionName)]
            public virtual String SubDescr
            {
                get
                {
                    return _SubDescr;
                }
                set
                {
                    _SubDescr = value;
                }
            }
            #endregion
            #region ItdAmt
            public abstract class itdAmt : IBqlField
            {
            }
            protected Decimal? _ItdAmt;
            [PXDBBaseCury]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Incep to Date")]
            public virtual Decimal? ItdAmt
            {
                get
                {
                    return _ItdAmt;
                }
                set
                {
                    _ItdAmt = value;
                }
            }
            #endregion
            #region YtdAmt
            public abstract class ytdAmt : IBqlField
            {
            }
            protected Decimal? _YtdAmt;
            [PXDBBaseCury]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Year to Date")]
            public virtual Decimal? YtdAmt
            {
                get
                {
                    return _YtdAmt;
                }
                set
                {
                    _YtdAmt = value;
                }
            }
            #endregion
            #region PtdAmt
            public abstract class ptdAmt : IBqlField
            {
            }
            protected Decimal? _PtdAmt;
            [PXDBBaseCury]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Period to Date")]
            public virtual Decimal? PtdAmt
            {
                get
                {
                    return _PtdAmt;
                }
                set
                {
                    _PtdAmt = value;
                }
            }
            #endregion
        }

        [Serializable]
        public partial class CreditAccount : Account
        {
            public new class accountID : IBqlField
            {
                 
            }
        }

        [Serializable]
        public partial class CreditSub : Sub
        {
            public new class subID : IBqlField
            {

            }
        }
    }
}
