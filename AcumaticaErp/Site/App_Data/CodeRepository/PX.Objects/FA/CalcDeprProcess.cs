using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.SM;

namespace PX.Objects.FA
{
	public class CalcDeprProcess : PXGraph<CalcDeprProcess>
	{
		public PXCancel<BalanceFilter> Cancel;
		public PXFilter<BalanceFilter> Filter;
        public PXAction<BalanceFilter> Calculate;
        public PXAction<BalanceFilter> CalculateAll;
        public PXAction<BalanceFilter> CalcDepr;
		public PXAction<BalanceFilter> CalcDeprAll;
		[PXFilterable]
        public PXFilteredProcessingJoin<FABookBalance, BalanceFilter, 
                InnerJoin<FixedAsset, On<FixedAsset.assetID, Equal<FABookBalance.assetID>>,
                InnerJoin<FADetails, On<FADetails.assetID, Equal<FABookBalance.assetID>>,
                LeftJoin<Account, On<Account.accountID, Equal<FixedAsset.fAAccountID>>>>>> Balances;
		public PXSetup<Company> company;
	    public PXSetup<FASetup> fasetup; 

		protected bool Depreciate = false;

        public CalcDeprProcess()
        {
            object setup = fasetup.Current;
        }

		[PXDBInt(IsKey = true)]
		[PXSelector(typeof(Search<FixedAsset.assetID>),
			SubstituteKey = typeof(FixedAsset.assetCD), CacheGlobal = true, DescriptionField = typeof(FixedAsset.description))]
		[PXUIField(DisplayName = "Fixed Asset", Enabled = false)]
		public virtual void FABookBalance_AssetID_CacheAttached(PXCache sender)
		{
		}

		protected virtual IEnumerable balances()
		{
			BalanceFilter filter = Filter.Current;

            PXSelectBase<FABookBalance> cmd;
            //PXFilteredProcessing sets IsDirty = false when processing all records and all records are placed in cache, 
            //until _Header refresh we will return readonly result, after it _InProc will be returned bypassing this delegate
            //if (false)
            //{
            //    cmd = new PXSelectReadonly2<FABookBalance,
            //        InnerJoin<FixedAsset, On<FixedAsset.assetID, Equal<FABookBalance.assetID>>,
            //        InnerJoin<FADetails, On<FADetails.assetID, Equal<FABookBalance.assetID>>>>,
            //        Where<FABookBalance.depreciate, Equal<True>, And<FABookBalance.status, Equal<FixedAssetStatus.active>, And<FADetails.status, Equal<FixedAssetStatus.active>>>>>(this);
            //}
            //else
            {
                cmd = new PXSelectJoin<FABookBalance,
                      InnerJoin<FixedAsset, On<FixedAsset.assetID, Equal<FABookBalance.assetID>>,
                      InnerJoin<FADetails, On<FADetails.assetID, Equal<FABookBalance.assetID>>>>,
                      Where<FABookBalance.depreciate, Equal<True>, And<FABookBalance.status, Equal<FixedAssetStatus.active>, And<FADetails.status, Equal<FixedAssetStatus.active>>>>>(this);
            }
			if (filter.BookID != null)
			{
				cmd.WhereAnd<Where<FABookBalance.bookID, Equal<Current<BalanceFilter.bookID>>>>();
			}
			if (filter.ClassID != null)
			{
				cmd.WhereAnd<Where<FixedAsset.classID, Equal<Current<BalanceFilter.classID>>>>();
			}
			if (filter.BranchID != null)
			{
                cmd.Join<LeftJoin<FALocationHistory, On<FALocationHistory.revisionID, Equal<FADetails.locationRevID>,
                                                And<FALocationHistory.assetID, Equal<FADetails.assetID>>>>>();
				cmd.WhereAnd<Where<FALocationHistory.locationID, Equal<Current<BalanceFilter.branchID>>>>();
			}
			if (!string.IsNullOrEmpty(filter.PeriodID))
			{
				cmd.WhereAnd<Where<FABookBalance.currDeprPeriod, LessEqual<Current<BalanceFilter.periodID>>>>();
			}
            if(filter.ParentAssetID != null)
            {
                cmd.WhereAnd<Where<FixedAsset.parentAssetID, Equal<Current<BalanceFilter.parentAssetID>>>>();
            }

            int startRow = PXView.StartRow;
            int totalRows = 0;

            List<PXFilterRow> newFilters = new List<PXFilterRow>();
            foreach (PXFilterRow f in PXView.Filters)
            {
                if (f.DataField.ToLower() == "status")
                {
                    f.DataField = "FADetails__Status";
                }
                newFilters.Add(f);
            } 
            List<object> list = cmd.View.Select(PXView.Currents, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, newFilters.ToArray(), ref startRow, PXView.MaximumRows, ref totalRows);
            PXView.StartRow = 0;
		    return list;
		}

        protected virtual void FABookBalance_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            FABookBalance bal = (FABookBalance)e.Row;
            if (bal == null || PXLongOperation.Exists(UID)) return;

            try
            {
                AssetProcess.CheckUnreleasedTransactions(this, bal);
            }
            catch (PXException exc)
            {
                PXUIFieldAttribute.SetEnabled<FABookBalance.selected>(sender, bal, false);
                sender.RaiseExceptionHandling<FABookBalance.selected>(bal, null, new PXSetPropertyException(exc.MessageNoNumber, PXErrorLevel.RowWarning));
            }
        }

		protected virtual void BalanceFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row != null)
			{
				BalanceFilter filter = Filter.Current;

				if (Depreciate)
				{
					Balances.SetProcessDelegate(delegate(List<FABookBalance> list)
					{
                        if (PXLongOperation.GetTaskList().Where(_ => _.Screen == "FA.50.20.00").ToArray().Count() > 1)
                        {
                            throw new PXException(Messages.AnotherDeprRunning);
                        }
                        AssetProcess.DepreciateAsset(list, null, filter.PeriodID, true);
					});
				}
				else
				{
					Balances.SetProcessDelegate(delegate(List<FABookBalance> list)
					{
                        if (PXLongOperation.GetTaskList().Where(_ => _.Screen == "FA.50.20.00").ToArray().Count() > 1)
                        {
                            throw new PXException(Messages.AnotherDeprRunning);
                        }
                        AssetProcess.CalculateAsset(list, filter.PeriodID);
					});
				}

				Balances.SetProcessVisible(false);
				Balances.SetProcessAllVisible(false);

				PXButtonState pstate = Actions["Process"].GetState(e.Row) as PXButtonState;
				PXButtonState pastate = Actions["ProcessAll"].GetState(e.Row) as PXButtonState;
				bool penabled = pstate == null || pstate.Enabled;
				bool paenabled = pastate == null || pastate.Enabled;

				Calculate.SetEnabled(penabled);
				CalculateAll.SetEnabled(paenabled);

				CalcDepr.SetEnabled(!string.IsNullOrEmpty(filter.PeriodID) && fasetup.Current.UpdateGL == true && penabled);
				CalcDeprAll.SetEnabled(!string.IsNullOrEmpty(filter.PeriodID) && fasetup.Current.UpdateGL == true && paenabled);
            }
		}

		[PXUIField(DisplayName = Messages.CalcDeprProc, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable calcDepr(PXAdapter adapter)
		{
			Depreciate = true;
			return Actions["Process"].Press(adapter);
		}

		[PXUIField(DisplayName = Messages.CalcDeprAllProc, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable calcDeprAll(PXAdapter adapter)
		{
			Depreciate = true;
			return Actions["ProcessAll"].Press(adapter);
		}

        [PXUIField(DisplayName = Messages.CalcProc, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXProcessButton]
        public virtual IEnumerable calculate(PXAdapter adapter)
        {
            Depreciate = false;
            return Actions["Process"].Press(adapter);
        }

        [PXUIField(DisplayName = Messages.CalcAllProc, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXProcessButton]
        public virtual IEnumerable calculateAll(PXAdapter adapter)
        {
            Depreciate = false;
            return Actions["ProcessAll"].Press(adapter);
        }

        public PXAction<BalanceFilter> viewAsset;
        [PXUIField(DisplayName = Messages.ViewAsset, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable ViewAsset(PXAdapter adapter)
        {
            if (Balances.Current != null)
            {
                AssetMaint graph = CreateInstance<AssetMaint>();
                graph.CurrentAsset.Current = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FABookBalance.assetID>>>>.Select(this);
                if (graph.CurrentAsset.Current != null)
                {
                    throw new PXRedirectRequiredException(graph, true, "ViewAsset") { Mode = PXBaseRedirectException.WindowMode.Same };
                }
            }
            return adapter.Get();
        }

        public PXAction<BalanceFilter> viewBook;
        [PXUIField(DisplayName = Messages.ViewBook, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable ViewBook(PXAdapter adapter)
        {
            if (Balances.Current != null)
            {
                BookMaint graph = CreateInstance<BookMaint>();
                graph.Book.Current = PXSelect<FABook, Where<FABook.bookID, Equal<Current<FABookBalance.bookID>>>>.Select(this);
                if (graph.Book.Current != null)
                {
                    throw new PXRedirectRequiredException(graph, true, "ViewBook") { Mode = PXBaseRedirectException.WindowMode.Same };
                }
            }
            return adapter.Get();
        }

        public PXAction<BalanceFilter> viewClass;
        [PXUIField(DisplayName = Messages.ViewClass, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable ViewClass(PXAdapter adapter)
        {
            if (Balances.Current != null)
            {
                AssetClassMaint graph = CreateInstance<AssetClassMaint>();
                graph.CurrentAssetClass.Current = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FABookBalance.classID>>>>.Select(this);
                if (graph.CurrentAssetClass.Current != null)
                {
                    throw new PXRedirectRequiredException(graph, true, "ViewClass") { Mode = PXBaseRedirectException.WindowMode.Same };
                }
            }
            return adapter.Get();
        }
    }

	[Serializable]
    public partial class BalanceFilter : ProcessAssetFilter
	{
		#region CalcOptions
		public abstract class calcOptions : IBqlField
		{
			#region List
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
				new[] { Calculate, Depreciate },
				new[] { Messages.Calculate, Messages.Depreciate }) { }
			}

			public const string Calculate = "C";
			public const string Depreciate = "D";

			public class calculate : Constant<string>
			{
				public calculate() : base(Calculate) { }
			}
			public class depreciate : Constant<string>
			{
				public depreciate() : base(Depreciate) { }
			}
			#endregion
		}
		protected String _CalcOptions;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(calcOptions.Depreciate)]
		[calcOptions.List]
		[PXUIField(DisplayName = "Calculation Options", Required = true)]
		public virtual String CalcOptions
		{
			get
			{
				return _CalcOptions;
			}
			set
			{
				_CalcOptions = value;
			}
		}
		#endregion
		#region PeriodID
		public abstract class periodID : IBqlField
		{
		}
		protected String _PeriodID;
		[PXUIField(DisplayName = "Depreciate Through", Required = true)]
		[PXString(6)]
		[FinPeriodIDFormatting()]
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
	}
}
