using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using System.Collections;
using PX.Objects.BQLConstants;

namespace PX.Objects.GL
{
    [Serializable]
	public partial class AccountByYearFilter : GLHistoryEnqFilter
	{
		public new abstract class branchID : PX.Data.IBqlField { }
		public new abstract class ledgerID : PX.Data.IBqlField { }
		public new abstract class accountID : PX.Data.IBqlField { }
		public new abstract class subID : PX.Data.IBqlField	{ }
		public new abstract class subCD : PX.Data.IBqlField	{ }
		public new abstract class subCDWildcard : PX.Data.IBqlField { };
		public new abstract class begFinPeriod : PX.Data.IBqlField {}
	
		public override String BegFinPeriod
		{
			get
			{
				if (this._FinYear!= null)
					return FirstPeriodOfYear(this._FinYear);
				else
					return null;
			}
		}		
		
		#region FinYear
		public abstract class finYear : PX.Data.IBqlField
		{
		}
		protected String _FinYear;
		[PXDBString(4)]
		[PXDefault()]
		[PXUIField(DisplayName = "Financial Year", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search3<FinYear.year, OrderBy<Desc<FinYear.startDate>>>))]
		public virtual String FinYear
		{
			get
			{
				return this._FinYear;
			}
			set
			{
				this._FinYear = value;
			}
		}
		#endregion
	}

	[PX.Objects.GL.TableAndChartDashboardType]
	public class AccountHistoryByYearEnq: PXGraph<AccountHistoryByYearEnq>
	{	
        #region Type Override events
        [AccountAny(true)]
        protected virtual void GLHistoryEnquiryResult_AccountID_CacheAttached(PXCache sender)
        {
        }

        [FinPeriodID(IsKey = true)]
        [PXUIField(DisplayName = "Period", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 0)]
        protected virtual void GLHistoryEnquiryResult_LastActivityPeriod_CacheAttached(PXCache sender)
        {
        }

        #endregion

		public PXCancel<AccountByYearFilter> Cancel;
		public PXAction<AccountByYearFilter> PreviousPeriod;
		public PXAction<AccountByYearFilter> NextPeriod;
		public PXFilter<AccountByYearFilter> Filter;
		public PXAction<AccountByYearFilter> accountDetails;
		public PXAction<AccountByYearFilter> accountBySub;
		[PXFilterable]
		public PXSelectOrderBy<GLHistoryEnquiryResult, OrderBy<Asc<GLHistoryEnquiryResult.lastActivityPeriod>>> EnqResult;
		public PXSetup<GLSetup> glsetup;
		public PXSelect<Account, Where<Account.accountID, Equal<Current<AccountByYearFilter.accountID>>>> AccountInfo;
		public FinYear fiscalyear
		{
			get
			{
				return PXSelectReadonly<FinYear, Where<FinYear.year, Equal<Current<AccountByYearFilter.finYear>>>>.Select(this);
			}
		}

		private AccountByYearFilter CurrentFilter 
		{
			get { return this.Filter.Current as AccountByYearFilter; }
		}

		public AccountHistoryByYearEnq()
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
			AccountByYearFilter filter = this.CurrentFilter;
			FinYear nextperiod = PXSelect<FinYear,
										Where<FinYear.year,
										Less<Current<AccountByYearFilter.finYear>>>,
										OrderBy<Desc<FinYear.year>>
										>.SelectWindowed(this, 0, 1);
			if (nextperiod == null)
			{
				nextperiod = PXSelectOrderBy<FinYear,
									OrderBy<Desc<FinYear.year>>
								 >.SelectWindowed(this, 0, 1);
				if (nextperiod == null) yield return filter;
			}
			filter.FinYear = nextperiod != null ? nextperiod.Year : null;
			yield return filter;
		}

		[PXUIField(DisplayName = ActionsMessages.Next, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		public virtual IEnumerable nextperiod(PXAdapter adapter)
		{
			AccountByYearFilter filter = this.CurrentFilter;
			FinYear nextperiod = PXSelect<FinYear,
										Where<FinYear.year,
										Greater<Current<AccountByYearFilter.finYear>>>,
										OrderBy<Asc<FinYear.year>>
										>.SelectWindowed(this, 0, 1);
			if (nextperiod == null)
			{
				nextperiod = PXSelectOrderBy<FinYear,
					OrderBy<Asc<FinYear.year>>
					>.SelectWindowed(this, 0, 1);
				if (nextperiod == null) yield return filter;
			}

			filter.FinYear = nextperiod != null ? nextperiod.Year : null;
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
				filter.SubID = this.Filter.Current.SubCD;
				filter.StartPeriodID = this.EnqResult.Current.LastActivityPeriod;
				filter.EndPeriodID = filter.StartPeriodID;
				filter.ShowCuryDetail = this.Filter.Current.ShowCuryDetail;
				graph.Filter.Update(filter);
				throw new PXRedirectRequiredException(graph, "Account Details");
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.ViewAccountBySub)]
		[PXLookupButton]
		protected virtual IEnumerable AccountBySub(PXAdapter adapter)
		{
			if (this.EnqResult.Current != null)
			{
				AccountHistoryBySubEnq graph = PXGraph.CreateInstance<AccountHistoryBySubEnq>();
				GLHistoryEnqFilter filter = PXCache<GLHistoryEnqFilter>.CreateCopy(graph.Filter.Current);
				filter.AccountID = this.Filter.Current.AccountID;
				filter.LedgerID = this.Filter.Current.LedgerID;
				filter.BranchID = this.Filter.Current.BranchID;
				filter.SubCD = this.Filter.Current.SubCD;
				filter.FinPeriodID = this.EnqResult.Current.LastActivityPeriod;
				filter.ShowCuryDetail = this.Filter.Current.ShowCuryDetail;
				graph.Filter.Update(filter);
				throw new PXRedirectRequiredException(graph, "Account by Subaccount");
			}
			return adapter.Get();
		}

		protected virtual void AccountByYearFilter_AccountID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue != null && !(e.NewValue is string))
			{
				Account acct = PXSelect<Account>.Search<Account.accountID>(this, e.NewValue);
				if (acct != null)
				{
					e.NewValue = acct.AccountCD;
				}
			}
		}

		protected virtual IEnumerable enqResult()
		{
			AccountByYearFilter filter = this.CurrentFilter;
			bool showCurrency = filter.ShowCuryDetail.HasValue && filter.ShowCuryDetail.Value;

			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.curyID>(EnqResult.Cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.curyPtdCreditTotal>(EnqResult.Cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.curyPtdDebitTotal>(EnqResult.Cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.curyBegBalance>(EnqResult.Cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.curyEndBalance>(EnqResult.Cache, null, showCurrency);
            PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.signCuryBegBalance>(EnqResult.Cache, null, showCurrency);
            PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.signCuryEndBalance>(EnqResult.Cache, null, showCurrency);

			if (filter.AccountID == null || filter.LedgerID == null || filter.FinYear == null)
			{
				yield break; //Prevent code from accessing database;
			}

			PXSelectBase<GLHistoryByPeriod> cmd = new PXSelectJoinGroupBy<GLHistoryByPeriod,
								InnerJoin<Account,
										On<GLHistoryByPeriod.accountID, Equal<Account.accountID>, And<Match<Account, Current<AccessInfo.userName>>>>,
								InnerJoin<FinPeriod,
										On<GLHistoryByPeriod.finPeriodID, Equal<FinPeriod.finPeriodID>>,
								InnerJoin<Sub,
										On<GLHistoryByPeriod.subID, Equal<Sub.subID>,And<Match<Sub, Current<AccessInfo.userName>>>>,
								LeftJoin<GLHistory, On<GLHistoryByPeriod.accountID, Equal<GLHistory.accountID>,
										And<GLHistoryByPeriod.subID, Equal<GLHistory.subID>,
										And<GLHistoryByPeriod.branchID, Equal<GLHistory.branchID>,
										And<GLHistoryByPeriod.ledgerID, Equal<GLHistory.ledgerID>,
										And<GLHistoryByPeriod.finPeriodID, Equal<GLHistory.finPeriodID>>>>>>,
								LeftJoin<AH, On<GLHistoryByPeriod.ledgerID, Equal<AH.ledgerID>,
										And<GLHistoryByPeriod.branchID, Equal<AH.branchID>,
										And<GLHistoryByPeriod.accountID, Equal<AH.accountID>,
										And<GLHistoryByPeriod.subID, Equal<AH.subID>,
										And<GLHistoryByPeriod.lastActivityPeriod, Equal<AH.finPeriodID>>>>>>>>>>>,
								Where<GLHistoryByPeriod.ledgerID, Equal<Current<AccountByYearFilter.ledgerID>>,
										And<FinPeriod.finYear, Equal<Current<AccountByYearFilter.finYear>>,
										And<GLHistoryByPeriod.accountID, Equal<Current<AccountByYearFilter.accountID>>,
										And<
											Where2<
												Where<Account.accountID,NotEqual<Current<GLSetup.ytdNetIncAccountID>>,And<Where<Account.type, Equal<AccountType.asset>, 
													Or<Account.type, Equal<AccountType.liability>>>>>,
												Or<
												Where<GLHistoryByPeriod.lastActivityPeriod, GreaterEqual<Required<GLHistoryByPeriod.lastActivityPeriod>>,
												And<Where<Account.type, Equal<AccountType.expense>, 
												Or<Account.type, Equal<AccountType.income>,
												Or<Account.accountID, Equal<Current<GLSetup.ytdNetIncAccountID>>>>>>>>>>>>>,
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
										GroupBy<GLHistoryByPeriod.finPeriodID
								 >>>>>>>>>>>>>>>(this);
			
			if (filter.SubID != null)
			{
				cmd.WhereAnd<Where<GLHistoryByPeriod.subID, Equal<Current<AccountByYearFilter.subID>>>>();
			}

			if (filter.BranchID != null)
			{
				cmd.WhereAnd<Where<GLHistoryByPeriod.branchID, Equal<Current<AccountByYearFilter.branchID>>>>();
			}

			if (!SubCDUtils.IsSubCDEmpty(filter.SubCD))
			{
				cmd.WhereAnd<Where<Sub.subCD, Like<Current<AccountByYearFilter.subCDWildcard>>>>();
			}

				string yearBegPeriod = filter.BegFinPeriod;
                GLSetup glSetup = glsetup.Current;
                bool reverseSign = (glSetup != null) && (glSetup.TrialBalanceSign == GLSetup.trialBalanceSign.Reversed);
                foreach (PXResult<GLHistoryByPeriod, Account, FinPeriod, Sub, GLHistory, AH> it in cmd.Select(yearBegPeriod))
				{
					GLHistoryByPeriod baseview = (GLHistoryByPeriod)it;
					Account acct = (Account)it;
					GLHistory ah = (GLHistory)it;
					AH ah1 = (AH)it;

                    if (reverseSign && acct.AccountID == glSetup.YtdNetIncAccountID) continue;

					GLHistoryEnquiryResult item = new GLHistoryEnquiryResult();
					item.AccountID = baseview.AccountID;
					item.LedgerID = baseview.LedgerID;
					item.LastActivityPeriod = baseview.FinPeriodID;
					item.PtdCreditTotal = ah.FinPtdCredit;
					item.PtdDebitTotal = ah.FinPtdDebit;
					item.CuryID = ah1.CuryID;
					item.Type = acct.Type;
					item.EndBalance = ah1.FinYtdBalance;
					if(!string.IsNullOrEmpty(ah1.CuryID))
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

		protected virtual void AccountByYearFilter_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			AccountByYearFilter filter = e.Row as AccountByYearFilter;
			if (filter != null)
			{
				if (filter.FinPeriodID != null && filter.FinPeriodID != "")
				{
					filter.FinYear = FiscalPeriodUtils.FiscalYear(filter.FinPeriodID); //Fill year from finPeriodID
				}

				if (filter.FinYear == null || filter.FinYear == "")
				{
					DateTime businessDate = this.Accessinfo.BusinessDate.Value;
					filter.FinYear = businessDate.Year.ToString("0000");
				}
			}
		}
		protected virtual void AccountByYearFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
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
					acctDef = this.AccountInfo.Current;
				}
				bool isDenominated = !string.IsNullOrEmpty(acctDef.CuryID);
				PXUIFieldAttribute.SetEnabled<AccountByYearFilter.showCuryDetail>(cache, e.Row, isDenominated);
				if (!isDenominated)
					row.ShowCuryDetail = false;
			}
		}
		protected virtual void AccountByYearFilter_SubCD_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void AccountByYearFilter_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<AccountByYearFilter.ledgerID>(e.Row);
		}
	}
}
