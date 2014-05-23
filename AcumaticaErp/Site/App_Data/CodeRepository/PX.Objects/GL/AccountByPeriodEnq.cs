using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.BQLConstants;
using PX.Objects.CM;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.CR;

namespace PX.Objects.GL
{
	#region FilterClass
	[System.SerializableAttribute()]
	public partial class AccountByPeriodFilter : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.IBqlField
		{
		}
		protected Int32? _BranchID;
		[Branch()]
        [PXRestrictor(typeof(Where<True, Equal<True>>), GL.Messages.BranchInactive, ReplaceInherited = true)]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region LedgerID
		public abstract class ledgerID : PX.Data.IBqlField
		{
		}
		protected Int32? _LedgerID;
		[PXDBInt()]
		[PXDefault(typeof(Search<Branch.ledgerID, Where<Branch.branchID, Equal<Current<AccountByPeriodFilter.branchID>>>>))]
		[PXUIField(DisplayName = "Ledger", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Ledger.ledgerID), SubstituteKey = typeof(Ledger.ledgerCD), DescriptionField = typeof(Ledger.descr))]
		public virtual Int32? LedgerID
		{
			get
			{
				return this._LedgerID;
			}
			set
			{
				this._LedgerID = value;
			}
		}
		#endregion
		#region StartPeriodID
		public abstract class startPeriodID : PX.Data.IBqlField
		{
		}
		protected String _StartPeriodID;
		[PXDefault()]
		[AnyPeriodFilterable(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "From Period", Visibility = PXUIVisibility.Visible)]
		public virtual String StartPeriodID
		{
			get
			{
				return this._StartPeriodID;
			}
			set
			{
				this._StartPeriodID = value;
			}
		}
		#endregion
		#region EndPeriodID
		public abstract class endPeriodID : PX.Data.IBqlField
		{
		}
		protected String _EndPeriodID;
		[PXDefault()]
		[AnyPeriodFilterable(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "To Period", Visibility = PXUIVisibility.Visible)]
		public virtual String EndPeriodID
		{
			get
			{
				return this._EndPeriodID;
			}
			set
			{
				this._EndPeriodID = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _StartDate;
		[PXDBDate()]
		//[PXDefault(typeof(Search<FinPeriod.startDate, Where<FinPeriod.finPeriodID, Equal<Current<AccountByPeriodFilter.finPeriodID>>>>))]
		[PXUIField(DisplayName = "From Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _EndDate;
		[PXDBDate()]
		//[PXDefault(typeof(Search<FinPeriod.endDate, Where<FinPeriod.finPeriodID, Equal<Current<AccountByPeriodFilter.finPeriodID>>>>))]
		//[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]
		public virtual DateTime? EndDate
		{
			get
			{
				return this._EndDate;
			}
			set
			{
				this._EndDate = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.IBqlField
		{
		}
		protected Int32? _AccountID;		
		[AccountAny()]
		[PXDefault()]
		public virtual Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.IBqlField
		{
		}
		protected String _SubID;
		[SubAccountRestrictedRaw(DisplayName = "Subaccount",SuppressValidation=true)]
		public virtual String SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region ShowSummary
		public abstract class showSummary : PX.Data.IBqlField
		{
		}
		protected Boolean? _ShowSummary;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Show Summary")]
		public virtual Boolean? ShowSummary
		{
			get
			{
				return this._ShowSummary;
			}
			set
			{
				this._ShowSummary = value;
			}
		}
		#endregion
		#region IncludeUnreleased
		public abstract class includeUnreleased : PX.Data.IBqlField
		{
		}
		protected Boolean? _IncludeUnreleased;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Include Unreleased")]
		public virtual Boolean? IncludeUnreleased
		{
			get
			{
				return this._IncludeUnreleased;
			}
			set
			{
				this._IncludeUnreleased = value;
			}
		}
		#endregion
		#region IncludeUnposted
		public abstract class includeUnposted : PX.Data.IBqlField
		{
		}
		protected Boolean? _IncludeUnposted;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Include Unposted")]
		public virtual Boolean? IncludeUnposted
		{
			get
			{
				return this._IncludeUnposted;
			}
			set
			{
				this._IncludeUnposted = value;
			}
		}
		#endregion
		#region BegBal
		public abstract class begBal : PX.Data.IBqlField
		{
		}
		protected Decimal? _BegBal;
		[PXDBBaseCury(typeof(AccountByPeriodFilter.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Beginning Balance", Enabled = false, Visible = true)]
		public virtual Decimal? BegBal
		{
			get
			{
				return this._BegBal;
			}
			set
			{
				this._BegBal = value;
			}
		}
		#endregion
		#region CreditTotal
		public abstract class creditTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _CreditTotal;
		[PXDBBaseCury(typeof(AccountByPeriodFilter.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Acct. Credit Total", Enabled = false, Visible = false)]
		public virtual Decimal? CreditTotal
		{
			get
			{
				return this._CreditTotal;
			}
			set
			{
				this._CreditTotal = value;
			}
		}
		#endregion
		#region DebitTotal
		public abstract class debitTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _DebitTotal;
		[PXDBBaseCury(typeof(AccountByPeriodFilter.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Acct. Debit Total", Enabled = false, Visible = false)]
		public virtual Decimal? DebitTotal
		{
			get
			{
				return this._DebitTotal;
			}
			set
			{
				this._DebitTotal = value;
			}
		}
		#endregion
		#region EndBal
		public abstract class endBal : PX.Data.IBqlField
		{
		}
		protected Decimal? _EndBal;
		[PXDBBaseCury(typeof(AccountByPeriodFilter.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ending Balance", Enabled = false, Visible = true)]
		public virtual Decimal? EndBal
		{
			get
			{
				return this._EndBal;
			}
			set
			{
				this._EndBal = value;
			}
		}
		#endregion
		#region TranCreditTotal
		public abstract class tranCreditTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranCreditTotal;
		[PXDBBaseCury(typeof(AccountByPeriodFilter.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Credit Total", Enabled = false, Visible =false)]
		public virtual Decimal? TranCreditTotal
		{
			get
			{
				return this._TranCreditTotal;
			}
			set
			{
				this._TranCreditTotal = value;
			}
		}
		#endregion
        #region TranDebitTotal
        public abstract class tranDebitTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _TranDebitTotal;
        [PXDBBaseCury(typeof(AccountByPeriodFilter.ledgerID))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Debit Total", Enabled = false, Visible =false)]
        public virtual Decimal? TranDebitTotal
        {
            get
            {
                return this._TranDebitTotal;
            }
            set
            {
                this._TranDebitTotal = value;
            }
        }
        #endregion
        #region TurnOver
		public abstract class turnOver : PX.Data.IBqlField
		{
		}
		protected Decimal? _turnOver;
		[PXDBBaseCury(typeof(AccountByPeriodFilter.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Turnover", Enabled = false, Visible =true )]
		public virtual Decimal? TurnOver
		{
			get
			{
                return this._turnOver;
			}
			set
			{
                this._turnOver = value;
			}
		}
		#endregion
		#region ShowCuryDetails
		public abstract class showCuryDetail : PX.Data.IBqlField
		{
		}
		protected bool? _ShowCuryDetail;
		[PXDBBool()]
		[PXDefault()]
		[PXUIField(DisplayName = "Show Currency Details", Visibility = PXUIVisibility.Visible)]
		public virtual bool? ShowCuryDetail
		{
			get
			{
				return this._ShowCuryDetail;
			}
			set
			{
				this._ShowCuryDetail = value;
			}
		}
		#endregion
		#region SubCD Wildcard
		public abstract class subCDWildcard : PX.Data.IBqlField { };
		[PXDBString(30, IsUnicode = true)]
		public virtual String SubCDWildcard
		{
			[PXDependsOnFields(typeof(subID))]
			get
			{
				return SubCDUtils.CreateSubCDWildcard(this._SubID, SubAccountAttribute.DimensionName);
			}
		}
		#endregion
		#region PeriodStartDate
		public abstract class periodStartDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _PeriodStartDate;
		[PXDBDate()]
		[PXDefault(typeof(Search<FinPeriod.startDate, Where<FinPeriod.finPeriodID, Equal<Current<AccountByPeriodFilter.startPeriodID>>>>),PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Period Start Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
		public virtual DateTime? PeriodStartDate
		{
			get
			{
				return this._PeriodStartDate;
			}
			set
			{
				this._PeriodStartDate = value;
			}
		}
		#endregion
		#region PeriodEndDate
		public abstract class periodEndDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _PeriodEndDate;
		[PXDBDate()]
		[PXDefault(typeof(Search<FinPeriod.endDate, Where<FinPeriod.finPeriodID, Equal<Current<AccountByPeriodFilter.endPeriodID>>>>),PersistingCheck = PXPersistingCheck.Nothing)]
		//[PXUIField(DisplayName = "Period End Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
		public virtual DateTime? PeriodEndDate
		{
			get
			{
				return this._PeriodEndDate;
			}
			set
			{
				this._PeriodEndDate = value;
			}
		}
		#endregion
		#region PeriodEndDateUI
		public abstract class periodEndDateUI : PX.Data.IBqlField
		{
		}

		[PXDate()]
		[PXUIField(DisplayName = "Period End Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
		public virtual DateTime? PeriodEndDateUI
		{
			get
			{
				return this._PeriodEndDate.HasValue ? this._PeriodEndDate.Value.AddDays(-1) : this._PeriodEndDate;
			}			
		}
		#endregion

		#region EndDateUI
		public abstract class endDateUI : PX.Data.IBqlField
		{
		}

		[PXDate()]
		[PXUIField(DisplayName = "To Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]
		public virtual DateTime? EndDateUI
		{
			get
			{				
				return ((this._EndDate.HasValue) ?(DateTime?) this._EndDate.Value.AddDays(-1) : null);
			}
			set
			{				
				this._EndDate = (value.HasValue) ? value.Value.AddDays(1) : value;				
			}
		}
		#endregion

		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		protected String _FinPeriodID;		
		[GL.FinPeriodID()]
		public String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
				this._StartPeriodID = value;
				this._EndPeriodID = value;
			}
		}
		#endregion

		public virtual void ClearTotals()
		{
			this._BegBal = 0m;
			this._EndBal = 0m;
			this._DebitTotal = 0m;
			this._CreditTotal = 0m;
			this._TranCreditTotal = 0m;
			this._TranDebitTotal = 0m;
		}
	}
	#endregion
	#region DAC class extension - additional fields
    [Serializable]
    public partial class GLTranR : GLTran, ISignedBalances
	{
		public new abstract class tranDate : PX.Data.IBqlField { }
		public new abstract class module : PX.Data.IBqlField { }
		public new abstract class batchNbr : PX.Data.IBqlField { }
		public new abstract class debitAmt : PX.Data.IBqlField { }
		public new abstract class creditAmt : PX.Data.IBqlField { }
		public new abstract class curyDebitAmt : PX.Data.IBqlField { }
		public new abstract class curyCreditAmt : PX.Data.IBqlField { }
		public new abstract class subID : PX.Data.IBqlField { }
		public new abstract class tranDesc : PX.Data.IBqlField { }
		public new abstract class tranType : PX.Data.IBqlField { }
		
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField { }
		[IN.Inventory(Visible=false)]
		public override Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion

		#region TranDate
		[PXDBDate()]
		[PXDBDefault(typeof(Batch.dateEntered))]
		[PXUIField(DisplayName = "Tran. Date", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public override DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		#region RefNbr
		[PXDBString(15, IsUnicode = true)]
		[PXDefault("", typeof(Search<GLTran.refNbr, Where<GLTran.module, Equal<Current<GLTran.module>>, And<GLTran.batchNbr, Equal<Current<GLTran.batchNbr>>>>>))]
		[PXUIField(DisplayName = "Ref. Number", Visibility = PXUIVisibility.SelectorVisible)]
		public override String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region TranDesc
		[PXDBString(60, IsUnicode = true)]
		[PXDefault(typeof(Search<GLTran.tranDesc, Where<GLTran.module, Equal<Current<GLTran.module>>, And<GLTran.batchNbr, Equal<Current<GLTran.batchNbr>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public override String TranDesc
		{
			get
			{
				return this._TranDesc;
			}
			set
			{
				this._TranDesc = value;
			}
		}
		#endregion
		#region BegBalance
		public abstract class begBalance : PX.Data.IBqlField { }
		protected Decimal? _BegBalance;
		[PXBaseCury(typeof(GLTranR.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Beg. Balance", Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? BegBalance
		{
			get
			{
				return this._BegBalance;
			}
			set
			{
				this._BegBalance = value;
			}
		}
		#endregion
		#region DebitAmt
		[PXDBBaseCury(typeof(GLTranR.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Debit Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public override Decimal? DebitAmt
		{
			get
			{
				return this._DebitAmt;
			}
			set
			{
				this._DebitAmt = value;
			}
		}
		#endregion
		#region CreditAmt
		[PXDBBaseCury(typeof(GLTranR.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Credit Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public override Decimal? CreditAmt
		{
			get
			{
				return this._CreditAmt;
			}
			set
			{
				this._CreditAmt = value;
			}
		}
		#endregion
		#region CuryDebitAmt
		[PXDBCury(typeof(GLTranR.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Curr. Debit Amount", Visibility = PXUIVisibility.Visible)]
		public override Decimal? CuryDebitAmt
		{
			get
			{
				return this._CuryDebitAmt;
			}
			set
			{
				this._CuryDebitAmt = value;
			}
		}
		#endregion
		#region CuryCreditAmt
		[PXDBCury(typeof(GLTranR.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Curr. Credit Amount", Visibility = PXUIVisibility.Visible)]
		public override Decimal? CuryCreditAmt
		{
			get
			{
				return this._CuryCreditAmt;
			}
			set
			{
				this._CuryCreditAmt = value;
			}
		}
		#endregion
		#region SubID
		[SubAccount()]		
		public override Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.IBqlField
		{
		}
		protected string _CuryID;
		[PXString]
		[PXUIField(DisplayName = "Currency ID", Visibility = PXUIVisibility.Visible)]
		public virtual string CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region EndBalance
		public abstract class endBalance : PX.Data.IBqlField { }
		[PXBaseCury(typeof(GLTranR.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ending Balance", Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? EndBalance
		{
			[PXDependsOnFields(typeof(begBalance),typeof(type),typeof(debitAmt),typeof(creditAmt))]
			get
			{
				return this.Type == null ? null : this._BegBalance + AccountRules.CalcSaldo(this.Type, this.DebitAmt ?? 0m, this.CreditAmt ?? 0m);
			}

		}
		#endregion
		#region CuryBegBalance
		public abstract class curyBegBalance : PX.Data.IBqlField { }
		protected Decimal? _CuryBegBalance;
		[PXCury(typeof(GLTranR.curyID))]
		[PXUIField(DisplayName = "Curr. Beg. Balance", Visible = true)]       
        public virtual Decimal? CuryBegBalance
		{
			get
			{
				return this._CuryBegBalance;
			}
			set
			{
				this._CuryBegBalance = value;
			}
		}
		#endregion
		#region CuryEndBalance
		public abstract class curyEndBalance : PX.Data.IBqlField { }
		[PXCury(typeof(GLTranR.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Curr. Ending Balance", Visible = true)]
		public virtual Decimal? CuryEndBalance
		{
			[PXDependsOnFields(typeof(curyBegBalance),typeof(type),typeof(curyDebitAmt),typeof(curyCreditAmt))]
			get
			{
				return this.Type == null ? null : this._CuryBegBalance + AccountRules.CalcSaldo(this.Type, this.CuryDebitAmt ?? 0m, this.CuryCreditAmt ?? 0m);
			}

		}
		#endregion
        #region SignBegBalance
        public abstract class signBegBalance : IBqlField {}
        [PXBaseCury(typeof (GLTranR.ledgerID))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Beg. Balance")]
        public virtual Decimal? SignBegBalance { get; set; }
        #endregion
        #region SignEndBalance
        public abstract class signEndBalance : IBqlField {}
        [PXBaseCury(typeof(GLTranR.ledgerID))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Ending Balance")]
        public virtual Decimal? SignEndBalance { get; set; }
        #endregion
        #region SignCuryBegBalance
        public abstract class signCuryBegBalance : IBqlField {}
        [PXCury(typeof(GLTranR.curyID))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Curr. Beg. Balance")]
        public virtual Decimal? SignCuryBegBalance  { get; set; }
        #endregion
        #region SignCuryEndBalance
        public abstract class signCuryEndBalance : IBqlField {}
        [PXCury(typeof(GLTranR.curyID))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Curr. Ending Balance")]
        public virtual Decimal? SignCuryEndBalance { get; set; }
        #endregion

		#region Type
		public abstract class type : IBqlField { }
		protected string _Type;
		[PXString(1)]
		[PXFormula(typeof(Selector<GLTran.accountID, Account.type>))]
		public virtual string Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}
		#endregion
		#region ReferenceID
		public new abstract class referenceID : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
		[PXDimensionSelector("BIZACCT", typeof(Search<BAccountR.bAccountID>), typeof(BAccountR.acctCD), DescriptionField = typeof(BAccountR.acctName), DirtyRead = true)]
		[PXUIField(DisplayName="Customer/Vendor", Enabled=false,Visible=false)]
		public override Int32? ReferenceID
		{
			get
			{
				return this._ReferenceID;
			}
			set
			{
				this._ReferenceID = value;
			}
		}
		#endregion				
	}

	#endregion

	[PX.Objects.GL.TableAndChartDashboardType]
	public class AccountByPeriodEnq : PXGraph<AccountByPeriodEnq>
	{
		#region Internal Types
		protected class SortedView : PXView
		{
			public SortedView(PXGraph graph, bool isReadOnly, BqlCommand select) : base(graph, isReadOnly, select) { }
			public SortedView(PXGraph graph, bool isReadOnly, BqlCommand select, Delegate handler) : base(graph, isReadOnly, select, handler) { }
			protected override void SortResult(List<object> list, PXView.PXSearchColumn[] sorts, bool reverseOrder)
			{
				base.SortResult(list, sorts, reverseOrder);
				if (list.Count > 0)
				{
					AccountByPeriodEnq graph = this._Graph as AccountByPeriodEnq;
					if (graph != null)
					{
						decimal startingBalance = 0m;
						Decimal? startingCuryBalance = 0m;
						string curyID = null;
						graph.RetrieveStartingBalance(ref startingBalance, ref startingCuryBalance, ref curyID);
                        GLSetup glSetup = graph.glsetup.Current;
                        bool reverseSign = (glSetup != null) && (glSetup.TrialBalanceSign == GLSetup.trialBalanceSign.Reversed);
                        foreach (object it in list)
						{
							GLTranR tran = (GLTranR)it;
							tran.BegBalance = startingBalance;
							startingBalance = tran.EndBalance ?? 0m; //Safe, because we just assign non-null value on decalartion
							if (!string.IsNullOrEmpty(tran.CuryID))
							{
								tran.CuryBegBalance = startingCuryBalance ?? 0m;
								startingCuryBalance = tran.CuryEndBalance ?? 0m;
							}
                            GLHistoryEnquiryResult.recalculateSignAmount(tran, reverseSign);
						}
					}
				}
			}
		}
		public class SortedSelect : PXSelectBase<GLTranR>
		{
			public SortedSelect(PXGraph graph, Delegate handler)
			{
				bool isReadOnly = true;
				this._Graph = graph;
				this.View = new SortedView(graph, isReadOnly,
								new Select3<GLTranR, OrderBy<Asc<GLTranR.tranDate,
								Asc<GLTranR.refNbr,
								Asc<GLTranR.batchNbr,
								Asc<GLTranR.module,
								Asc<GLTranR.lineNbr>>>>>>>(), handler);
			}
		}
		#endregion
		#region declaration

		public PXCancel<AccountByPeriodFilter> Cancel;
		public PXAction<AccountByPeriodFilter> PreviousPeriod;
		public PXAction<AccountByPeriodFilter> NextPeriod;
		public PXFilter<AccountByPeriodFilter> Filter;
		public PXAction<AccountByPeriodFilter> viewBatch;
		public PXAction<AccountByPeriodFilter> viewAPDocument;
		[PXFilterable]
		public SortedSelect GLTranEnq;
		public PXSetup<GLSetup> glsetup;
		public PXSelect<Account, Where<Account.accountID, Equal<Current<AccountByPeriodFilter.accountID>>>> AccountInfo;
		public FinPeriod fiscalperiod
		{
			get
			{
				return PXSelectReadonly<FinPeriod, Where<FinPeriod.finPeriodID, Equal<Current<AccountByPeriodFilter.startPeriodID>>>>.Select(this);
			}
		}

		#endregion
		#region Ctor
		public AccountByPeriodEnq()
		{
			GLSetup setup = glsetup.Current;
			GLTranEnq.Cache.AllowInsert = false;
			GLTranEnq.Cache.AllowDelete = false;
			GLTranEnq.Cache.AllowUpdate = false;
		}
		#endregion       
		#region Button Delegates
		[PXUIField(DisplayName = ActionsMessages.Previous, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		public virtual IEnumerable previousperiod(PXAdapter adapter)
		{
			AccountByPeriodFilter filter = Filter.Current as AccountByPeriodFilter;
			FinPeriod nextperiod = PXSelect<FinPeriod,
										Where<FinPeriod.finPeriodID,
										Less<Current<AccountByPeriodFilter.startPeriodID>>>,
										OrderBy<Desc<FinPeriod.finPeriodID>>
										>.SelectWindowed(this, 0, 1);
			if (nextperiod == null)
			{
				nextperiod = PXSelectOrderBy<FinPeriod,
									OrderBy<Desc<FinPeriod.finPeriodID>>>.SelectWindowed(this, 0, 1);
				if (nextperiod == null) yield return filter;
			}

			filter.StartPeriodID = nextperiod != null ? nextperiod.FinPeriodID : null;
			FinPeriod nextperiod2 = PXSelect<FinPeriod,
										Where<FinPeriod.finPeriodID,
										Less<Current<AccountByPeriodFilter.endPeriodID>>>,
										OrderBy<Desc<FinPeriod.finPeriodID>>
										>.SelectWindowed(this, 0, 1);
			if (nextperiod2 == null)
			{
				nextperiod2 = PXSelectOrderBy<FinPeriod,
									OrderBy<Desc<FinPeriod.finPeriodID>>>.SelectWindowed(this, 0, 1);
				if (nextperiod2 == null) yield return filter;
			}
			filter.EndPeriodID = nextperiod2 != null ? nextperiod2.FinPeriodID : null;
			ResetFilterDates(filter);
			yield return filter;
		}

		[PXUIField(DisplayName = ActionsMessages.Next, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		public virtual IEnumerable nextperiod(PXAdapter adapter)
		{
			AccountByPeriodFilter filter = Filter.Current as AccountByPeriodFilter;
			FinPeriod nextperiod = PXSelect<FinPeriod,
										Where<FinPeriod.finPeriodID,
										Greater<Current<AccountByPeriodFilter.startPeriodID>>>,
										OrderBy<Asc<FinPeriod.finPeriodID>>
										>.SelectWindowed(this, 0, 1);
			if (nextperiod == null)
			{
				nextperiod = PXSelectOrderBy<FinPeriod,
									OrderBy<Asc<FinPeriod.finPeriodID>>>.SelectWindowed(this, 0, 1);
				if (nextperiod == null) yield return filter;
			}
			filter.StartPeriodID = nextperiod != null ? nextperiod.FinPeriodID : null;
			FinPeriod nextperiod2 = PXSelect<FinPeriod,
										Where<FinPeriod.finPeriodID,
										Greater<Current<AccountByPeriodFilter.endPeriodID>>>,
										OrderBy<Asc<FinPeriod.finPeriodID>>
										>.SelectWindowed(this, 0, 1);
			if (nextperiod2 == null)
			{
				nextperiod2 = PXSelectOrderBy<FinPeriod,
									OrderBy<Asc<FinPeriod.finPeriodID>>>.SelectWindowed(this, 0, 1);
				if (nextperiod2 == null) yield return filter;
			}
			filter.EndPeriodID = nextperiod2 != null ? nextperiod2.FinPeriodID : null;
			ResetFilterDates(filter);
			yield return filter;
		}

		#region Redirection buttons
		[PXUIField(DisplayName = Messages.ViewBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton()]
		public virtual IEnumerable ViewBatch(PXAdapter adapter)
		{
			if (this.GLTranEnq.Current != null)
			{
				AccountByPeriodFilter filter = this.Filter.Current as AccountByPeriodFilter;
				if (filter != null)
				{
					if (!(filter.ShowSummary ?? false))
					{
						Batch batch = FindBatch(this, this.GLTranEnq.Current);
						if (batch != null)
						{
							JournalEntry graph = PXGraph.CreateInstance<JournalEntry>();
							graph.BatchModule.Current = batch;
							throw new PXRedirectRequiredException(graph, true, "ViewBatch"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
						}
					}
					else
					{
						//Switch to details view - on the same screen
						GLTranR row = (GLTranR)this.GLTranEnq.Current;
						filter.ShowSummary = false;
						filter.StartDate = new DateTime(row.TranDate.Value.Year, row.TranDate.Value.Month, row.TranDate.Value.Day);
						filter.EndDate = filter.StartDate.Value.AddDays(1);
						this.Filter.Update(filter);
					}
				}
			}
			return Filter.Select();
		}

		[PXUIField(DisplayName = Messages.ViewSourceDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton()]
		public virtual IEnumerable ViewAPDocument(PXAdapter adapter)
		{
			if (this.GLTranEnq.Current != null)
			{
				GLTran tran = (GLTran)this.GLTranEnq.Current;
				
				IDocGraphCreator creator = null;
				switch (tran.Module) 
				{
					case BatchModule.AP:
						creator = new APDocGraphCreator(); break;
					case BatchModule.AR:
						creator = new ARDocGraphCreator(); break;
					case BatchModule.CA:
						creator = new CADocGraphCreator(); break;
					case BatchModule.DR:
						creator = new DRDocGraphCreator(); break;
					case PX.Objects.GL.BatchModule.IN:
						creator = new INDocGraphCreator(); break;
                    case BatchModule.FA:
				        creator = new FADocGraphCreator(); break;
					case BatchModule.PM:
						creator = new PMDocGraphCreator(); break;
				}
				if(creator!=null)
				{
					PXGraph graph = creator.Create(tran);	
					if (graph != null)
					{
						throw new PXRedirectRequiredException(graph, true, "ViewAPDocument") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
					}
				}
                throw new PXException(Messages.SourceDocumentCanNotBeFound);
			}
			return Filter.Select();
		}
		#endregion
		#endregion

		protected virtual IEnumerable gltranenq()
		{

			AccountByPeriodFilter filter = Filter.Current as AccountByPeriodFilter;
			PXSelectBase<GLTranR> cmd;
			bool showCurrency = (filter.ShowCuryDetail.HasValue && filter.ShowCuryDetail.Value);
            PXUIFieldAttribute.SetVisible<GLTranR.curyCreditAmt>(GLTranEnq.Cache, null, showCurrency);
            PXUIFieldAttribute.SetVisible<GLTranR.curyDebitAmt>(GLTranEnq.Cache, null, showCurrency);
            PXUIFieldAttribute.SetVisible<GLTranR.curyBegBalance>(GLTranEnq.Cache, null, showCurrency);
            PXUIFieldAttribute.SetVisible<GLTranR.curyEndBalance>(GLTranEnq.Cache, null, showCurrency);
            PXUIFieldAttribute.SetVisible<GLTranR.signCuryBegBalance>(GLTranEnq.Cache, null, showCurrency);
            PXUIFieldAttribute.SetVisible<GLTranR.signCuryEndBalance>(GLTranEnq.Cache, null, showCurrency);
            PXUIFieldAttribute.SetVisible<GLTranR.curyID>(GLTranEnq.Cache, null, showCurrency);

			if (filter.AccountID == null)
				yield break;
			PXUIFieldAttribute.SetVisible<GLTranR.begBalance>(GLTranEnq.Cache, null, true);
			PXUIFieldAttribute.SetVisible<GLTranR.endBalance>(GLTranEnq.Cache, null, true);

			if (filter.ShowSummary == true)
			{
				PXUIFieldAttribute.SetVisible<GLTranR.batchNbr>(GLTranEnq.Cache, null, false);
				PXUIFieldAttribute.SetVisible<GLTranR.accountID>(GLTranEnq.Cache, null, false);
				PXUIFieldAttribute.SetVisible<GLTranR.subID>(GLTranEnq.Cache, null, false);
				PXUIFieldAttribute.SetVisible<GLTranR.refNbr>(GLTranEnq.Cache, null, false);
				PXUIFieldAttribute.SetVisible<GLTranR.lineNbr>(GLTranEnq.Cache, null, false);


				cmd = new PXSelectJoinGroupBy<GLTranR,
									InnerJoin<Sub, On<GLTranR.subID, Equal<Sub.subID>, And<Match<Sub, Current<AccessInfo.userName>>>>,
									InnerJoin<Account, On<GLTranR.accountID, Equal<Account.accountID>, And<Match<Account, Current<AccessInfo.userName>>>>, 
                                    LeftJoin<Batch, On<GLTranR.module, Equal<Batch.module>, And<GLTranR.batchNbr, Equal<Batch.batchNbr>>>>>>,
									Where<GLTranR.ledgerID, Equal<Current<AccountByPeriodFilter.ledgerID>>,
										And<GLTranR.accountID, Equal<Current<AccountByPeriodFilter.accountID>>,
										And<GLTranR.finPeriodID, GreaterEqual<Current<AccountByPeriodFilter.startPeriodID>>,
										And<GLTranR.finPeriodID, LessEqual<Current<AccountByPeriodFilter.endPeriodID>>,
                                        And<Batch.voided, NotEqual<True>,
                                        And<Batch.scheduled, NotEqual<True>>>>>>>,
									Aggregate<Sum<GLTranR.creditAmt,
										Sum<GLTranR.debitAmt,
										Sum<GLTranR.curyCreditAmt,
										Sum<GLTranR.curyDebitAmt,
										GroupBy<GLTranR.ledgerID,
										GroupBy<GLTranR.accountID,
										GroupBy<GLTranR.finPeriodID,
										GroupBy<GLTranR.tranDate>>>>>>>>>
									>(this);
			}
			else
			{
				PXUIFieldAttribute.SetVisible<GLTranR.batchNbr>(GLTranEnq.Cache, null, true);
				PXUIFieldAttribute.SetVisible<GLTranR.module>(GLTranEnq.Cache, null, true);


				cmd = new PXSelectJoin<GLTranR,
								InnerJoin<Sub, On<GLTranR.subID, Equal<Sub.subID>, And<Match<Sub, Current<AccessInfo.userName>>>>,
								InnerJoin<Account, On<GLTranR.accountID, Equal<Account.accountID>, And<Match<Account, Current<AccessInfo.userName>>>>,
                                LeftJoin<Batch, On<GLTranR.module, Equal<Batch.module>, And<GLTranR.batchNbr, Equal<Batch.batchNbr>>>>>>,
                                Where<GLTranR.ledgerID, Equal<Current<AccountByPeriodFilter.ledgerID>>,
									And<GLTranR.accountID, Equal<Current<AccountByPeriodFilter.accountID>>,									
									And<GLTranR.finPeriodID, GreaterEqual<Current<AccountByPeriodFilter.startPeriodID>>,
									And<GLTranR.finPeriodID, LessEqual<Current<AccountByPeriodFilter.endPeriodID>>,
                                    And<Batch.voided, NotEqual<True>,
                                    And<Batch.scheduled, NotEqual<True>>>>>>>,
                                OrderBy<Asc<GLTranR.tranDate,
									Asc<GLTranR.refNbr,
									Asc<GLTranR.batchNbr,
									Asc<GLTranR.module,
									Asc<GLTranR.lineNbr>>>>>>
								>(this);
			}			

			if (filter.IncludeUnposted == false)
			{
				cmd.WhereAnd<Where<GLTranR.posted, Equal<BitOn>>>();
			}
			else if (filter.IncludeUnposted == true && filter.IncludeUnreleased == false)
			{
				cmd.WhereAnd<Where<GLTranR.released, Equal<BitOn>>>();
			}

			if (filter.StartDate.HasValue) 
			{
				cmd.WhereAnd<Where<GLTranR.tranDate, GreaterEqual<Current<AccountByPeriodFilter.startDate>>>>();				
			}

			if (filter.EndDate.HasValue)
			{
				cmd.WhereAnd<Where<GLTranR.tranDate, Less<Current<AccountByPeriodFilter.endDate>>>>();
			}

			if (filter.BranchID != null)
			{
				cmd.WhereAnd<Where<GLTranR.branchID, Equal<Current<AccountByPeriodFilter.branchID>>>>();
			}

			if (!SubCDUtils.IsSubCDEmpty(filter.SubID))
			{
				cmd.WhereAnd<Where<Sub.subCD, Like<Current<AccountByPeriodFilter.subCDWildcard>>>>();
			}
			foreach (PXResult<GLTranR, Sub, Account> it in cmd.Select())
			{
				GLTranR tran = (GLTranR)it;
				Account acct = (Account)it;
				tran.Type = acct.Type;

				if (filter.ShowSummary == true)
				{
					tran.TranDesc = Enum.GetName(typeof(DayOfWeek), tran.TranDate.Value.DayOfWeek);
				}
				if (acct.CuryID != null)
				{
					tran.CuryID = acct.CuryID;
				}
				else
				{
					tran.CuryID = null;
					tran.CuryCreditAmt = null;
					tran.CuryDebitAmt = null;
				}
				yield return tran;
			}
            
		}

		#region Filter Events Handlers
		[PXDBDefault(typeof(Batch.finPeriodID))]
		[GL.FinPeriodID()]
		[PXUIField(DisplayName = "Period")]
		protected virtual void GLTranR_FinPeriodID_CacheAttached(PXCache sender)
		{
		}

        [Branch(Required = false)]
        [PXRestrictor(typeof(Where<True, Equal<True>>), GL.Messages.BranchInactive, ReplaceInherited = true)]
        protected virtual void AccountByPeriodFilter_BranchID_CacheAttached(PXCache sender)
        {
        }

		protected virtual void AccountByPeriodFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;

            AccountByPeriodFilter row = (AccountByPeriodFilter)e.Row;
			if (row!=null && (!row.IncludeUnposted ?? false))
			{
				row.IncludeUnreleased = false;
			}
			PXUIFieldAttribute.SetEnabled<AccountByPeriodFilter.includeUnreleased>(cache, row, row.IncludeUnposted ?? false);
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
				PXUIFieldAttribute.SetEnabled<AccountByPeriodFilter.showCuryDetail>(cache, e.Row, isDenominated);
				if (!isDenominated)
					row.ShowCuryDetail = false;
				if (row.EndDate.HasValue && row.PeriodEndDate.HasValue && row.EndDate > row.PeriodEndDate)
				{
					cache.RaiseExceptionHandling<AccountByPeriodFilter.endDateUI>(e.Row, row.EndDateUI, new PXSetPropertyException("To have effect a date must be set inside a period(s) dates range"));
				}
				else 
				{
					cache.RaiseExceptionHandling<AccountByPeriodFilter.endDateUI>(e.Row, null, null);
				}				
			}

			if (row.EndDate.HasValue && ((row.PeriodEndDate.HasValue && row.EndDate > row.PeriodEndDate) || (row.PeriodStartDate.HasValue && row.EndDate < row.PeriodStartDate)))
			{
				cache.RaiseExceptionHandling<AccountByPeriodFilter.endDateUI>(e.Row, row.EndDateUI, new PXSetPropertyException(Messages.DateMustBeSetWithingPeriodDatesRange, PXErrorLevel.Warning));
			}
			else
			{
				cache.RaiseExceptionHandling<AccountByPeriodFilter.endDateUI>(e.Row, null, null);
			}

			if (row.StartDate.HasValue && ((row.PeriodStartDate.HasValue && row.StartDate < row.PeriodStartDate) || (row.PeriodEndDate.HasValue && row.StartDate >= row.PeriodEndDate)))
			{
				cache.RaiseExceptionHandling<AccountByPeriodFilter.startDate>(e.Row, row.StartDate, new PXSetPropertyException(Messages.DateMustBeSetWithingPeriodDatesRange, PXErrorLevel.Warning));				
			}
			else
			{
				cache.RaiseExceptionHandling<AccountByPeriodFilter.startDate>(e.Row, null, null);
			}
		}
		protected virtual void AccountByPeriodFilter_StartPeriodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			AccountByPeriodFilter row = (AccountByPeriodFilter)e.Row;
			
			if (String.Compare(row.StartPeriodID,row.EndPeriodID) > 0) 
			{
				cache.SetValue<AccountByPeriodFilter.endPeriodID>(e.Row,row.StartPeriodID);
			}
			//cache.SetDefaultExt<AccountByPeriodFilter.endPeriodID>(e.Row);
			this.ResetFilterDates(row);
		}
		protected virtual void AccountByPeriodFilter_EndPeriodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			AccountByPeriodFilter row = (AccountByPeriodFilter)e.Row;
			if (String.Compare(row.StartPeriodID, row.EndPeriodID) > 0)
			{
				cache.SetValue<AccountByPeriodFilter.startPeriodID>(e.Row, row.EndPeriodID);
			}
			this.ResetFilterDates(row);
		}
		protected virtual void AccountByPeriodFilter_ShowSummary_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			AccountByPeriodFilter row = (AccountByPeriodFilter)e.Row;
			if (row.ShowSummary ?? false)
			{
				this.ResetFilterDates(row);
			}
		}
		protected virtual void AccountByPeriodFilter_SubID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void AccountByPeriodFilter_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<AccountByPeriodFilter.ledgerID>(e.Row);
		}
        
		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}
		#endregion

		#region Utility Functions
		public static Batch FindBatch(PXGraph aGraph, GLTran aTran)
		{
			PXSelectBase<Batch> sel = new PXSelect<Batch, Where<Batch.module, Equal<Required<Batch.module>>, And<Batch.batchNbr, Equal<Required<Batch.batchNbr>>>>>(aGraph);
			return (Batch)sel.View.SelectSingle(aTran.Module, aTran.BatchNbr);
		}
		public static void Copy(GLHistoryEnqFilter aDest, AccountByPeriodFilter aSrc)
		{
			aDest.AccountID = aSrc.AccountID;
			aDest.SubCD = aSrc.SubID;
			aDest.LedgerID = aSrc.LedgerID;
			aDest.FinPeriodID = aSrc.StartPeriodID;
		    aDest.BranchID = aSrc.BranchID;
		}
        public static void Aggregate(AccountByPeriodFilter aDest, GLTranR aSrc)
        {
            aDest.TranDebitTotal += aSrc.DebitAmt ?? 0m;
            aDest.TranCreditTotal += aSrc.CreditAmt ?? 0m;
            aDest.TurnOver = System.Math.Abs((aDest.TranDebitTotal ?? 0m) - (aDest.TranCreditTotal ?? 0m));
        }
		public static void Aggregate(AccountByPeriodFilter aDest, GLHistoryEnquiryResult aSrc)
		{
			aDest.BegBal += aSrc.BegBalance ?? 0m;
			aDest.EndBal += aSrc.EndBalance ?? 0m;
			aDest.DebitTotal += aSrc.PtdDebitTotal ?? 0m;
			aDest.CreditTotal += aSrc.PtdCreditTotal ?? 0m;
		}

		protected virtual void RetrieveStartingBalance(ref decimal balance, ref Decimal? aCuryBalance, ref string aCuryID)
		{
			balance = 0m;
			aCuryBalance = 0m;
			aCuryID = null;
			AccountByPeriodFilter filter = this.Filter.Current as AccountByPeriodFilter;
			if (filter != null)
			{
				if (filter.AccountID != null && filter.LedgerID != null)
				{
					AccountHistoryEnq acctHistoryBO = PXGraph.CreateInstance<AccountHistoryEnq>();
					GLHistoryEnqFilter histFilter = acctHistoryBO.Filter.Current;
					AccountByPeriodEnq.Copy(histFilter, filter);
					acctHistoryBO.Filter.Update(histFilter);
					bool isFirst = true;
					foreach (GLHistoryEnquiryResult iRes in acctHistoryBO.EnqResult.Select())
					{
						balance += iRes.BegBalance ?? 0m;
						if (isFirst)
						{
							aCuryID = iRes.CuryID;
							isFirst = false;
						}
						if (aCuryID != null && aCuryID == iRes.CuryID)
						{
							aCuryBalance += iRes.CuryBegBalance;
						}
						else
						{
							aCuryID = null;
							aCuryBalance = null;
						}
					}
					decimal balAdjustment = 0m;
					Decimal? curyBalAdjustment = 0m;
					string adjCuryID = aCuryID;
					this.RetrieveStartingBalanceAdjustment(ref balAdjustment, ref curyBalAdjustment, ref adjCuryID);
					balance += balAdjustment;
					if (aCuryID != null && adjCuryID == aCuryID)
					{
						aCuryBalance += curyBalAdjustment;
					}
					else
					{
						aCuryBalance = null;
						aCuryID = null;
					}

				}
			}

		}
		protected virtual void RetrieveStartingBalanceAdjustment(ref decimal aAjust, ref Decimal? aCuryAdjust, ref string aCuryID)
		{
			aAjust = 0m;
			aCuryAdjust = 0m;
			AccountByPeriodFilter filter = this.Filter.Current as AccountByPeriodFilter;
			if (filter != null)
			{

				if (filter.AccountID != null && filter.LedgerID != null)
				{
					FinPeriod period = this.fiscalperiod;
					if (period != null)
					{
						decimal adjCredit = 0m;
						decimal adjDebit = 0m;
						decimal adjCuryDebit = 0m;
						decimal adjCuryCredit = 0m;
						string acctType = AccountType.Expense; //Exact type doesn't metter - only for empty list;
						bool sameCury = true;
						bool isFirst = true;
						if (filter.StartDate.HasValue)
						{
							PXSelectBase<GLTran> cmd = new PXSelectJoinGroupBy<GLTran,
										InnerJoin<Account, On<GLTran.accountID, Equal<Account.accountID>>>,
										Where<GLTran.ledgerID, Equal<Current<AccountByPeriodFilter.ledgerID>>,
										And<GLTran.accountID, Equal<Current<AccountByPeriodFilter.accountID>>,
										And<GLTran.finPeriodID, GreaterEqual<Current<AccountByPeriodFilter.startPeriodID>>,
										And<GLTran.finPeriodID, LessEqual<Current<AccountByPeriodFilter.endPeriodID>>,
										And<GLTran.tranDate, Less<Current<AccountByPeriodFilter.startDate>>>>>>>,
										Aggregate<Sum<GLTran.debitAmt,
										Sum<GLTran.creditAmt,
										Sum<GLTran.curyCreditAmt,
										Sum<GLTran.curyDebitAmt,
										GroupBy<GLTran.accountID
										>>>>>>>(this);

							if (filter.BranchID != null)
							{
								cmd.WhereAnd<Where<GLTranR.branchID, Equal<Current<AccountByPeriodFilter.branchID>>>>();
							}

							foreach (PXResult<GLTran, Account> iRes in cmd.Select())
							{
								GLTran it = (GLTran)iRes;
								Account iAcct = (Account)iRes;
								adjDebit += it.DebitAmt ?? 0m;
								adjCredit += it.CreditAmt ?? 0m;
								if (isFirst)
								{
									aCuryID = iAcct.CuryID;
									isFirst = false;
								}
								if (sameCury && iAcct.CuryID == aCuryID)
								{
									adjCuryDebit += it.CuryDebitAmt ?? 0m;
									adjCuryCredit += it.CuryCreditAmt ?? 0m;
								}
								else
								{
									sameCury = false;
								}
								acctType = iAcct.Type;
							}
						}
						aAjust = AccountRules.CalcSaldo(acctType, adjDebit, adjCredit);
						if (sameCury)
						{
							aCuryAdjust = AccountRules.CalcSaldo(acctType, adjCuryDebit, adjCuryCredit);
						}
						else
						{
							aCuryAdjust = null;
							aCuryID = null;
						}

					}
				}
			}
		}
		protected virtual void ResetFilterDates(AccountByPeriodFilter aRow)
		{
			FinPeriod period = PXSelectReadonly<FinPeriod, Where<FinPeriod.finPeriodID, Equal<Required<FinPeriod.finPeriodID>>>>.Select(this, aRow.StartPeriodID);
			FinPeriod endPeriod = PXSelectReadonly<FinPeriod, Where<FinPeriod.finPeriodID, Equal<Required<FinPeriod.finPeriodID>>>>.Select(this, aRow.EndPeriodID);
			if (period != null && endPeriod!= null)
			{
				aRow.PeriodStartDate = period.StartDate <= endPeriod.StartDate ? period.StartDate : endPeriod.StartDate;
				aRow.PeriodEndDate = endPeriod.EndDate >= period.EndDate ? endPeriod.EndDate : period.EndDate;
				aRow.EndDate = null;
				aRow.StartDate = null;
			}
			
		}

		#endregion

		protected virtual IEnumerable filter() 
		{
			PXCache cache = this.Caches[typeof(AccountByPeriodFilter)];
			if (cache != null)
			{
				AccountByPeriodFilter filter = cache.Current as AccountByPeriodFilter;
				if (filter != null)
				{

					//bool byPeriod = !string.IsNullOrEmpty(filter.FiscalPeriod);
					filter.ClearTotals();
					//Count

					bool isFirst = true;
					foreach (GLTranR it in this.GLTranEnq.Select())
					{
						Aggregate(filter, it);
						if (isFirst) 
						{
							filter.BegBal = it.BegBalance;
							isFirst = false;
						}
						filter.EndBal = it.EndBalance;
					}
				}
				yield return cache.Current;
				cache.IsDirty = false;

			}
		}


	}


	public interface  IDocGraphCreator
	{
		PXGraph Create(GLTran tran);
		PXGraph Create(string aTranType, string aRefNbr, int? referenceID);
		
	}
	public class ARDocGraphCreator:IDocGraphCreator
	{
		public ARDocGraphCreator() { ;}


		public virtual PXGraph Create(GLTran tran) 
		{			
			return Create(tran.TranType, tran.RefNbr, null);
		}

		public virtual PXGraph Create(string aTranType, string aRefNbr, int? referenceID)
		{
			PXGraph graph = null;
			bool? isInvoiceType = ARDocType.Payable(aTranType);
			bool combined = (aTranType == ARDocType.CashSale) || (aTranType == ARDocType.CashReturn);
			if (combined)
			{
				ARCashSaleEntry destGraph = PXGraph.CreateInstance<ARCashSaleEntry>();
				destGraph.Document.Current = destGraph.Document.Search<ARRegister.refNbr>(aRefNbr, aTranType);
				graph = destGraph;
			}
			else
			{
				if (isInvoiceType.HasValue)
				{
					if (isInvoiceType.Value)
					{
						//Invoice Or CreditAdjustment
						ARInvoiceEntry destGraph = PXGraph.CreateInstance<ARInvoiceEntry>();
						destGraph.Document.Current = destGraph.Document.Search<ARRegister.refNbr>(aRefNbr, aTranType);
						graph = destGraph;
					}
					else
					{
						//Paymnet Or DebitAdjustment
						AR.ARPaymentEntry destGraph = PXGraph.CreateInstance<ARPaymentEntry>();
						destGraph.Document.Current = destGraph.Document.Search<ARRegister.refNbr>(aRefNbr, aTranType);
						graph = destGraph;
					}
				}
			}
			return graph;
		}

	}
	public class APDocGraphCreator:IDocGraphCreator 
	{
		public APDocGraphCreator() { ;}
		public virtual PXGraph Create(GLTran tran) 
		{
			return Create(tran.TranType, tran.RefNbr, null);
		}

		public virtual PXGraph Create(string aTranType, string aRefNbr, int? referenceID)		
		{
			PXGraph graph = null;
			bool? isInvoiceType = APDocType.Payable(aTranType);
			bool combined = (aTranType == APDocType.QuickCheck) || (aTranType == APDocType.VoidQuickCheck);
			if (combined)
			{
				AP.APQuickCheckEntry destGraph = PXGraph.CreateInstance<APQuickCheckEntry>();
				destGraph.Document.Current = destGraph.Document.Search<APRegister.refNbr>(aRefNbr, aTranType);
				graph = destGraph;
			}
			else
			{
				if (isInvoiceType.HasValue)
				{
					if (isInvoiceType.Value)
					{
						//Invoice Or CreditAdjustment
						AP.APInvoiceEntry destGraph = PXGraph.CreateInstance<APInvoiceEntry>();
						destGraph.Document.Current = destGraph.Document.Search<APRegister.refNbr>(aRefNbr, aTranType);
						graph = destGraph;

					}
					else
					{
						//Paymnet Or DebitAdjustment
						APPaymentEntry destGraph = PXGraph.CreateInstance<APPaymentEntry>();
						destGraph.Document.Current = destGraph.Document.Search<APRegister.refNbr>(aRefNbr, aTranType);
						graph = destGraph;
					}
				}
			}
			return graph;
		}

	}
	public class CADocGraphCreator : IDocGraphCreator
	{
		public CADocGraphCreator() { ;}
		public virtual PXGraph Create(GLTran tran)
		{		
			return Create(tran.TranType, tran.RefNbr, null);
		}

		public virtual PXGraph Create(string aTranType, string aRefNbr, int? referenceID)
		{
			if (aTranType == CA.CATranType.CATransferIn || aTranType == CA.CATranType.CATransferOut || aTranType == CA.CATranType.CATransferRGOL) 
			{
				CA.CashTransferEntry destGraph = PXGraph.CreateInstance<CA.CashTransferEntry>();
				destGraph.Transfer.Current = destGraph.Transfer.Search<CA.CATransfer.transferNbr>(aRefNbr);
				return destGraph;
			}
			if(aTranType == CA.CATranType.CATransferExp || aTranType == CA.CATranType.CAAdjustment)
			{
				CA.CATranEntry destGraph = PXGraph.CreateInstance<CA.CATranEntry>();
				destGraph.CAAdjRecords.Current = destGraph.CAAdjRecords.Search<CA.CAAdj.adjRefNbr>(aRefNbr, aTranType);
				return destGraph;
			}
			return null;
		}

	}
	public class DRDocGraphCreator : IDocGraphCreator
	{
		public DRDocGraphCreator() { ;}
		public virtual PXGraph Create(GLTran tran)
		{
			return Create(tran.TranType, tran.RefNbr, null);
		}
		public virtual PXGraph Create(string aTranType, string aRefNbr, int? referenceID)
		{		
			if ( !string.IsNullOrEmpty(aRefNbr) )
			{
				int scheduleID;

				if (int.TryParse(aRefNbr, out scheduleID))
				{
					DR.DraftScheduleMaint destGraph = PXGraph.CreateInstance<PX.Objects.DR.DraftScheduleMaint>();
					destGraph.Schedule.Current = destGraph.Schedule.Search<DR.DRSchedule.scheduleID>(scheduleID);
					return destGraph;
				}
			}
						
			return null;
		}
	}

    public class FADocGraphCreator : IDocGraphCreator
    {
        public FADocGraphCreator() { ;}
		public virtual PXGraph Create(GLTran tran)
        {
			return Create(tran.TranType, tran.RefNbr, null);
		}
		public virtual PXGraph Create(string aTranType, string aRefNbr, int? referenceID)
		{	
            if (!string.IsNullOrEmpty(aRefNbr))
            {
                FA.TransactionEntry destGraph = PXGraph.CreateInstance<FA.TransactionEntry>();
                destGraph.Document.Current = destGraph.Document.Search<FA.FARegister.refNbr>(aRefNbr);
                return destGraph;
            }

            return null;
        }
    }
    
    public class INDocGraphCreator : IDocGraphCreator
	{
		public INDocGraphCreator() { ;}
		public virtual PXGraph Create(GLTran tran)
		{
			return Create(tran.TranType, tran.RefNbr, null);
		}

		public virtual PXGraph Create(string aTranType, string aRefNbr, int? referenceID)
		{	
			if (aTranType == IN.INTranType.Receipt)
			{
				IN.INReceiptEntry destGraph = PXGraph.CreateInstance<PX.Objects.IN.INReceiptEntry>();
				destGraph.receipt.Current = destGraph.receipt.Search<IN.INRegister.refNbr>(aRefNbr);
				return destGraph;
			}

			if (aTranType == IN.INTranType.Issue ||
				aTranType == IN.INTranType.Return ||
				aTranType == IN.INTranType.DebitMemo ||
				aTranType == IN.INTranType.CreditMemo ||
				aTranType == IN.INTranType.Invoice)
			{
				IN.INIssueEntry destGraph = PXGraph.CreateInstance<PX.Objects.IN.INIssueEntry>();
				destGraph.issue.Current = destGraph.issue.Search<IN.INRegister.refNbr>(aRefNbr);
				return destGraph;
			}

			if (aTranType == IN.INTranType.Adjustment ||
				aTranType == IN.INTranType.StandardCostAdjustment ||
				aTranType == IN.INTranType.ReceiptCostAdjustment)
			{
				IN.INAdjustmentEntry destGraph = PXGraph.CreateInstance<PX.Objects.IN.INAdjustmentEntry>();
				destGraph.adjustment.Current = destGraph.adjustment.Search<IN.INRegister.refNbr>(aRefNbr);
				return destGraph;
			}

			if (aTranType == IN.INTranType.Transfer)
			{
				IN.INTransferEntry destGraph = PXGraph.CreateInstance<PX.Objects.IN.INTransferEntry>();
				destGraph.transfer.Current = destGraph.transfer.Search<IN.INRegister.refNbr>(aRefNbr);
				return destGraph;
			}

			if (aTranType == IN.INTranType.StandardCostAdjustment)
			{
				IN.INTransferEntry destGraph = PXGraph.CreateInstance<PX.Objects.IN.INTransferEntry>();
				destGraph.transfer.Current = destGraph.transfer.Search<IN.INRegister.refNbr>(aRefNbr);
				return destGraph;
			}


			
			return null;
		}

	}

	public class GLDocGraphCreator : IDocGraphCreator
	{
		public GLDocGraphCreator() { ;}
		public virtual PXGraph Create(GLTran tran)
		{
			return Create(tran.TranType, tran.RefNbr, null);
		}

		public virtual PXGraph Create(string aTranType, string aRefNbr, int? referenceID)
		{	
			GL.JournalEntry destGraph = PXGraph.CreateInstance<PX.Objects.GL.JournalEntry>();
			destGraph.BatchModule.Current = destGraph.BatchModule.Search<GL.Batch.batchNbr>(aRefNbr);
			return destGraph;
		}

	}

	public class PMDocGraphCreator : IDocGraphCreator
	{
		public PMDocGraphCreator()
		{
		}


		public virtual PXGraph Create(string aTranType, string aRefNbr, int? referenceID)
		{
			throw new PXException("This interface method is not supported for this Document type");
		}

		public virtual PXGraph Create(GLTran tran)
		{
			if (tran.PMTranID != null)
			{
				PM.RegisterEntry destGraph = PXGraph.CreateInstance<PM.RegisterEntry>();

				PM.PMTran pmTran = PXSelect<PM.PMTran, Where<PM.PMTran.tranID, Equal<Required<PM.PMTran.tranID>>>>.Select(destGraph, tran.PMTranID);

				if (pmTran != null)
				{
					destGraph.Document.Current = PXSelect<PM.PMRegister,
						Where<PM.PMRegister.module, Equal<Required<PM.PMRegister.module>>,
							And<PM.PMRegister.refNbr, Equal<Required<PM.PMRegister.refNbr>>>>>.Select(destGraph, pmTran.TranType, pmTran.RefNbr);
					return destGraph;
				}

			}

			return null;
		}
	}
}
