using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using System.Collections;
using PX.Objects.BQLConstants;

namespace PX.Objects.GL
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class AccountHistoryBySubEnq: PXGraph<AccountHistoryBySubEnq>
	{
        #region Cache Attached Events

        #region GLHistoryEnquiryResult
        #region SubCD

		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]        
        [PXUIField(DisplayName = "Subaccount", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 0)]
        [PXDimension("SUBACCOUNT")]
        protected virtual void GLHistoryEnquiryResult_SubCD_CacheAttached(PXCache sender)
        {
        }
        
        #endregion
        #region AccountID

        [AccountAny(true)]
        [PXDefault()]
        protected virtual void GLHistoryEnquiryResult_AccountID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region Description
    
        [PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        protected virtual void GLHistoryEnquiryResult_Description_CacheAttached(PXCache sender)
        {
        }

        #endregion
        #region LastActivityPeriod
        [FinPeriodID()]
        [PXUIField(DisplayName = "Financial Period")]
        protected virtual void GLHistoryEnquiryResult_LastActivityPeriod_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion
        #endregion

        public PXCancel<GLHistoryEnqFilter> Cancel;
		public PXAction<GLHistoryEnqFilter> PreviousPeriod;
		public PXAction<GLHistoryEnqFilter> NextPeriod;
		public PXFilter<GLHistoryEnqFilter> Filter;
		public PXAction<GLHistoryEnqFilter> accountDetails;
		[PXFilterable]
		public PXSelectOrderBy<GLHistoryEnquiryResult, OrderBy<Asc<GLHistoryEnquiryResult.subCD>>> EnqResult;
		public PXSetup<GLSetup> glsetup;
		public PXSelect<Account, Where<Account.accountID, Equal<Current<GLHistoryEnqFilter.accountID>>>> AccountInfo;

		private GLHistoryEnqFilter CurrentFilter
		{
			get { return this.Filter.Current as GLHistoryEnqFilter; }
		}

		public AccountHistoryBySubEnq()
		{
			GLSetup setup = glsetup.Current;
			//SWUIFieldAttribute.SetEnabled(EnqResult.Cache, null, false);
			EnqResult.Cache.AllowInsert = false;
			EnqResult.Cache.AllowDelete = false;
			EnqResult.Cache.AllowUpdate = false;
		}

		[PXUIField(DisplayName = ActionsMessages.Previous, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		public virtual IEnumerable previousperiod(PXAdapter adapter)
		{
			GLHistoryEnqFilter filter = this.CurrentFilter;
			FinPeriod nextperiod = PXSelect<FinPeriod,
										Where<FinPeriod.finPeriodID,
										Less<Current<GLHistoryEnqFilter.finPeriodID>>>,
										OrderBy<Desc<FinPeriod.finPeriodID>>
										>.SelectWindowed(this, 0, 1);
			if (nextperiod == null)
			{
				nextperiod = PXSelectOrderBy<FinPeriod,
									OrderBy<Desc<FinPeriod.finPeriodID>>>.SelectWindowed(this, 0, 1);
				if (nextperiod == null) yield return filter;
			}

			filter.FinPeriodID = nextperiod != null ? nextperiod.FinPeriodID : null;
			yield return filter;
		}

		[PXUIField(DisplayName =ActionsMessages.Next, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		public virtual IEnumerable nextperiod(PXAdapter adapter)
		{
			GLHistoryEnqFilter filter = this.CurrentFilter;
			FinPeriod nextperiod = PXSelect<FinPeriod,
										Where<FinPeriod.finPeriodID,
										Greater<Current<GLHistoryEnqFilter.finPeriodID>>>,
										OrderBy<Asc<FinPeriod.finPeriodID>>
										>.SelectWindowed(this, 0, 1);
			if (nextperiod == null)
			{
				nextperiod = PXSelectOrderBy<FinPeriod,
									OrderBy<Asc<FinPeriod.finPeriodID>>>.SelectWindowed(this, 0, 1);
				if (nextperiod == null) yield return filter;
			}
			filter.FinPeriodID = nextperiod != null ? nextperiod.FinPeriodID : null;
			yield return filter;
		}

		[PXUIField(DisplayName = Messages.ViewAccountDetails)]
		[PXLookupButton]
		protected virtual IEnumerable AccountDetails(PXAdapter adapter)
		{
			if (this.EnqResult.Current != null)
			{
				if (this.EnqResult.Current.AccountID == this.glsetup.Current.YtdNetIncAccountID)
					throw new PXException(Messages.DetailsReportIsNotAllowedForYTDNetIncome);
				AccountByPeriodEnq graph = PXGraph.CreateInstance<AccountByPeriodEnq>();
				AccountByPeriodFilter filter = PXCache<AccountByPeriodFilter>.CreateCopy(graph.Filter.Current);
				filter.AccountID = this.Filter.Current.AccountID;
				filter.LedgerID = this.Filter.Current.LedgerID;
				filter.BranchID = this.Filter.Current.BranchID;
				filter.SubID = this.EnqResult.Current.SubCD;
				filter.StartPeriodID = this.Filter.Current.FinPeriodID;
				filter.EndPeriodID = filter.StartPeriodID;
				filter.ShowCuryDetail = this.Filter.Current.ShowCuryDetail;
				graph.Filter.Update(filter);
				throw new PXRedirectRequiredException(graph, "Account Details");
			}
			return adapter.Get();
		}

		protected virtual IEnumerable enqResult()
		{
			GLHistoryEnqFilter filter = this.CurrentFilter;
			bool showCurrency = filter.ShowCuryDetail.HasValue && filter.ShowCuryDetail.Value;

			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.curyID>(EnqResult.Cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.curyPtdCreditTotal>(EnqResult.Cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.curyPtdDebitTotal>(EnqResult.Cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.curyBegBalance>(EnqResult.Cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.curyEndBalance>(EnqResult.Cache, null, showCurrency);
            PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.signCuryBegBalance>(EnqResult.Cache, null, showCurrency);
            PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.signCuryEndBalance>(EnqResult.Cache, null, showCurrency);
            if (filter.AccountID == null || filter.LedgerID == null || filter.FinPeriodID == null)
			{
				yield break; //Prevent code from accessing database;
			}

			PXSelectBase<GLHistoryByPeriod> cmd = new PXSelectJoinGroupBy<GLHistoryByPeriod,
							InnerJoin<Account,
									On<GLHistoryByPeriod.accountID, Equal<Account.accountID>, And<Match<Account, Current<AccessInfo.userName>>>>,
							InnerJoin<Sub,
									On<GLHistoryByPeriod.subID, Equal<Sub.subID>, And<Match<Sub, Current<AccessInfo.userName>>>>,
							LeftJoin<GLHistory, On<GLHistoryByPeriod.accountID, Equal<GLHistory.accountID>,
									And<GLHistoryByPeriod.ledgerID, Equal<GLHistory.ledgerID>,
									And<GLHistoryByPeriod.branchID, Equal<GLHistory.branchID>,
									And<GLHistoryByPeriod.subID, Equal<GLHistory.subID>,
									And<GLHistoryByPeriod.finPeriodID, Equal<GLHistory.finPeriodID>>>>>>,
							LeftJoin<AH, On<GLHistoryByPeriod.ledgerID, Equal<AH.ledgerID>,
									And<GLHistoryByPeriod.branchID, Equal<AH.branchID>,
									And<GLHistoryByPeriod.accountID, Equal<AH.accountID>,
									And<GLHistoryByPeriod.subID, Equal<AH.subID>,
									And<GLHistoryByPeriod.lastActivityPeriod, Equal<AH.finPeriodID>>>>>>>>>>,
							Where<GLHistoryByPeriod.ledgerID, Equal<Current<GLHistoryEnqFilter.ledgerID>>,
									And<GLHistoryByPeriod.accountID, Equal<Current<GLHistoryEnqFilter.accountID>>,
									And<GLHistoryByPeriod.finPeriodID, Equal<Current<GLHistoryEnqFilter.finPeriodID>>,
									And<
										Where2<
												Where<Account.accountID,NotEqual<Current<GLSetup.ytdNetIncAccountID>>,And<Where<Account.type, Equal<AccountType.asset>, 
													Or<Account.type, Equal<AccountType.liability>>>>>,
										Or<Where<GLHistoryByPeriod.lastActivityPeriod, GreaterEqual<Required<GLHistoryByPeriod.lastActivityPeriod>>,
											And<Where<Account.type, Equal<AccountType.expense>, 
											Or<Account.type, Equal<AccountType.income>,
											Or<Account.accountID,Equal<Current<GLSetup.ytdNetIncAccountID>>>>>>>>>>>>>,
							Aggregate<
									Sum<AH.finYtdBalance,
									Sum<AH.curyFinYtdBalance,
									Sum<GLHistory.finPtdDebit,
									Sum<GLHistory.finPtdCredit,
									Sum<GLHistory.finBegBalance,
									Sum<GLHistory.finYtdBalance,
									Sum<GLHistory.curyFinBegBalance,
									Sum<GLHistory.curyFinYtdBalance,
									Sum<GLHistory.curyFinPtdCredit,
									Sum<GLHistory.curyFinPtdDebit,
									GroupBy<GLHistoryByPeriod.ledgerID,
									GroupBy<GLHistoryByPeriod.accountID,
									GroupBy<GLHistoryByPeriod.subID
							 >>>>>>>>>>>>>>>(this);

			if (filter.BranchID != null)
			{
				cmd.WhereAnd<Where<GLHistoryByPeriod.branchID, Equal<Current<GLHistoryEnqFilter.branchID>>>>();
			}

			if (!SubCDUtils.IsSubCDEmpty(filter.SubCD))
			{
				cmd.WhereAnd<Where<Sub.subCD, Like<Current<GLHistoryEnqFilter.subCDWildcard>>>>();
			}

			string yearBegPeriod = filter.BegFinPeriod;
            GLSetup glSetup = glsetup.Current;
            bool reverseSign = (glSetup != null) && (glSetup.TrialBalanceSign == GLSetup.trialBalanceSign.Reversed);
            foreach (PXResult<GLHistoryByPeriod, Account, Sub, GLHistory, AH> it in cmd.Select(yearBegPeriod))
			{
				GLHistoryByPeriod baseview = (GLHistoryByPeriod)it;
				Account acct = (Account)it;
				GLHistory ah = (GLHistory)it;
				AH ah1 = (AH)it;

                if (reverseSign && acct.AccountID == glSetup.YtdNetIncAccountID) continue;
                
                GLHistoryEnquiryResult item = new GLHistoryEnquiryResult();
				item.AccountID = baseview.AccountID;
				item.LedgerID = baseview.LedgerID;
				item.Type = acct.Type;
				item.Description = acct.Description;
				item.LastActivityPeriod = baseview.LastActivityPeriod;
				item.PtdCreditTotal = ah.FinPtdCredit;
				item.PtdDebitTotal = ah.FinPtdDebit;
				item.CuryID = ah1.CuryID;
				item.SubCD = ((Sub)it).SubCD;
				item.EndBalance = ah1.FinYtdBalance;
				if (!String.IsNullOrEmpty(ah1.CuryID))
				{
					item.CuryEndBalance = ah1.CuryFinYtdBalance; // 
					item.CuryPtdCreditTotal = ah.CuryFinPtdCredit;
					item.CuryPtdDebitTotal = ah.CuryFinPtdDebit;
				}
				else
				{
					item.CuryEndBalance = null;
					item.CuryPtdCreditTotal = null;
					item.CuryPtdDebitTotal = null;
				}
				item.recalculate(true); // End balance is considered as correct digit - so we need to calculate begBalance base on ending one
                item.recalculateSignAmount(reverseSign);
                yield return item;
			}
		}
		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}

		
		protected virtual void GLHistoryEnqFilter_SubCD_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
		protected virtual void GLHistoryEnqFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			GLHistoryEnqFilter row = (GLHistoryEnqFilter)e.Row;
			if (row != null && row.AccountID != null) 
			{
				Account acctDef = null;
				if (this.AccountInfo.Current == null || row.AccountID != this.AccountInfo.Current.AccountID)
				{
					acctDef = this.AccountInfo.Select();
				}
				else 
				{
					acctDef=this.AccountInfo.Current;
				}
				bool isDenominated = !string.IsNullOrEmpty(acctDef.CuryID);
				PXUIFieldAttribute.SetEnabled<GLHistoryEnqFilter.showCuryDetail>(cache, e.Row, isDenominated);
				if (!isDenominated)
					row.ShowCuryDetail = false;
			}
		}

		protected virtual void GLHistoryEnqFilter_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<GLHistoryEnqFilter.ledgerID>(e.Row);
		}
		[AnyPeriodFilterable(typeof(AccessInfo.businessDate))]
		[PXDefault()]
		[PXUIField(DisplayName = "Period", Visibility = PXUIVisibility.Visible)]
		protected virtual void GLHistoryEnqFilter_FinPeriodID_CacheAttached(PXCache sender)
		{
		}

	}

}
