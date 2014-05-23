namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	using PX.Objects.CM;

	[System.SerializableAttribute()]
	public partial class GLHistory : BaseGLHistory, PX.Data.IBqlTable
	{

		#region LedgerID
		public abstract class ledgerID : PX.Data.IBqlField
		{
		}
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.IBqlField
		{
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.IBqlField
		{
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.IBqlField
		{
		}
		#endregion
		#region FinPeriod
		public abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		#endregion
        #region FinYear
        public abstract class finYear : PX.Data.IBqlField
        { 
        }
        [PXDBCalced(typeof(Substring<finPeriodID, CS.int1, CS.int4>), typeof(string))]
        public virtual string FinYear
        {
            get;
            set;
        }
        #endregion
        #region BalanceType
        public abstract class balanceType : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.IBqlField
		{
		}
		#endregion
		#region DetDeleted
		public abstract class detDeleted : PX.Data.IBqlField
		{
		}
		protected Boolean? _DetDeleted;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? DetDeleted
		{
			get
			{
				return this._DetDeleted;
			}
			set
			{
				this._DetDeleted = value;
			}
		}
		#endregion
		#region YearClosed
		public abstract class yearClosed : PX.Data.IBqlField { }
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? YearClosed
		{
			get;
			set;
		}
		#endregion
		#region FinPtdCredit
		public abstract class finPtdCredit : PX.Data.IBqlField
		{
		}
		#endregion
		#region FinPtdDebit
		public abstract class finPtdDebit : PX.Data.IBqlField
		{
		}
		#endregion
		#region FinYtdBalance
		public abstract class finYtdBalance : PX.Data.IBqlField
		{
		}
		#endregion
		#region FinBegBalance
		public abstract class finBegBalance : PX.Data.IBqlField
		{
		}
		#endregion
		#region FinPtdRevalued
		public abstract class finPtdRevalued : PX.Data.IBqlField
		{
		}
		#endregion
		#region TranPtdCredit
		public abstract class tranPtdCredit : PX.Data.IBqlField
		{
		}
		#endregion
		#region TranPtdDebit
		public abstract class tranPtdDebit : PX.Data.IBqlField
		{
		}
		#endregion
		#region TranYtdBalance
		public abstract class tranYtdBalance : PX.Data.IBqlField
		{
		}
		#endregion
		#region TranBegBalance
		public abstract class tranBegBalance : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryFinPtdCredit
		public abstract class curyFinPtdCredit : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryFinPtdDebit
		public abstract class curyFinPtdDebit : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryFinYtdBalance
		public abstract class curyFinYtdBalance : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryFinBegBalance
		public abstract class curyFinBegBalance : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryTranPtdCredit
		public abstract class curyTranPtdCredit : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryTranPtdDebit
		public abstract class curyTranPtdDebit : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryTranYtdBalance
		public abstract class curyTranYtdBalance : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryTranBegBalance
		public abstract class curyTranBegBalance : PX.Data.IBqlField
		{
		}
		#endregion 
		#region AllocPtdBalance
		public abstract class allocPtdBalance : PX.Data.IBqlField
		{
		}
		protected Decimal? _AllocPtdBalance;
		[PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? AllocPtdBalance
		{
			get
			{
				return this._AllocPtdBalance;
			}
			set
			{
				this._AllocPtdBalance = value;
			}
		}
	#endregion
		#region AllocBegBalance
		public abstract class allocBegBalance : PX.Data.IBqlField
		{
		}
		protected Decimal? _AllocBegBalance;
		[PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? AllocBegBalance
		{
			get
			{
				return this._AllocBegBalance;
			}
			set
			{
				this._AllocBegBalance = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.IBqlField
		{
		}
		#endregion
	}	

	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(AccountByPeriodEnq), Filter = typeof(AccountByPeriodFilter))]
	public class BaseGLHistory 
	{
		#region LedgerID
		protected Int32? _LedgerID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Ledger", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(Ledger.ledgerID),
			typeof(Ledger.ledgerCD), typeof(Ledger.baseCuryID), typeof(Ledger.descr), typeof(Ledger.balanceType),
			DescriptionField = typeof(Ledger.descr), SubstituteKey = typeof(Ledger.ledgerCD))]
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
		#region BranchID
		public abstract class branchID : PX.Data.IBqlField
		{
		}
		protected Int32? _BranchID;
		[Branch(IsKey = true)]
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
		#region AccountID
		protected Int32? _AccountID;
		/*[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Account", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(Account.accountID), DescriptionField = typeof(Account.description), SubstituteKey = typeof(Account.accountCD))]*/
		[Account(Visibility = PXUIVisibility.Invisible,IsKey = true, DescriptionField = typeof(Account.description))]
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
		protected Int32? _SubID;
		/*[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Subaccount", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(Sub.subID), DescriptionField = typeof(Sub.description), SubstituteKey = typeof(Sub.subCD))]*/
		[SubAccount(IsKey = true, Visibility = PXUIVisibility.Invisible, DescriptionField = typeof(Sub.description))]
		[PXDefault()]
		public virtual Int32? SubID
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
		#region FinPeriod
		protected String _FinPeriodID;
		[GL.FinPeriodID(IsKey=true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Financial Period", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(FinPeriod.finPeriodID), DescriptionField = typeof(FinPeriod.descr),SelectorMode=PXSelectorMode.NoAutocomplete)]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region BalanceType
		protected String _BalanceType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(LedgerBalanceType.Actual)]
		[LedgerBalanceType.List()]
		[PXUIField(DisplayName = "Balance Type")]
		public virtual String BalanceType
		{
			get
			{
				return this._BalanceType;
			}
			set
			{
				this._BalanceType = value;
			}
		}
		#endregion
		#region CuryID
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true)]
		public virtual String CuryID
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
		#region FinPtdCredit
		protected Decimal? _FinPtdCredit;
		[PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinPtdCredit
		{
			get
			{
				return this._FinPtdCredit;
			}
			set
			{
				this._FinPtdCredit = value;
			}
		}
		#endregion
		#region FinPtdDebit
		protected Decimal? _FinPtdDebit;
		[PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinPtdDebit
		{
			get
			{
				return this._FinPtdDebit;
			}
			set
			{
				this._FinPtdDebit = value;
			}
		}
		#endregion
		#region FinYtdBalance
		protected Decimal? _FinYtdBalance;
		[PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinYtdBalance
		{
			get
			{
				return this._FinYtdBalance;
			}
			set
			{
				this._FinYtdBalance = value;
			}
		}
		#endregion
		#region FinBegBalance
		protected Decimal? _FinBegBalance;
		[PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinBegBalance
		{
			get
			{
				return this._FinBegBalance;
			}
			set
			{
				this._FinBegBalance = value;
			}
		}
		#endregion
		#region FinPtdRevalued
		protected Decimal? _FinPtdRevalued;
		[PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinPtdRevalued
		{
			get
			{
				return this._FinPtdRevalued;
			}
			set
			{
				this._FinPtdRevalued = value;
			}
		}
		#endregion
		#region TranPtdCredit
		protected Decimal? _TranPtdCredit;
		[PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranPtdCredit
		{
			get
			{
				return this._TranPtdCredit;
			}
			set
			{
				this._TranPtdCredit = value;
			}
		}
		#endregion
		#region TranPtdDebit
		protected Decimal? _TranPtdDebit;
		[PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranPtdDebit
		{
			get
			{
				return this._TranPtdDebit;
			}
			set
			{
				this._TranPtdDebit = value;
			}
		}
		#endregion
		#region TranYtdBalance
		protected Decimal? _TranYtdBalance;
		[PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranYtdBalance
		{
			get
			{
				return this._TranYtdBalance;
			}
			set
			{
				this._TranYtdBalance = value;
			}
		}
		#endregion
		#region TranBegBalance
		protected Decimal? _TranBegBalance;
		[PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranBegBalance
		{
			get
			{
				return this._TranBegBalance;
			}
			set
			{
				this._TranBegBalance = value;
			}
		}
		#endregion
		#region CuryFinPtdCredit
		protected Decimal? _CuryFinPtdCredit;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryFinPtdCredit
		{
			get
			{
				return this._CuryFinPtdCredit;
			}
			set
			{
				this._CuryFinPtdCredit = value;
			}
		}
		#endregion
		#region CuryFinPtdDebit
		protected Decimal? _CuryFinPtdDebit;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryFinPtdDebit
		{
			get
			{
				return this._CuryFinPtdDebit;
			}
			set
			{
				this._CuryFinPtdDebit = value;
			}
		}
		#endregion
		#region CuryFinYtdBalance
		protected Decimal? _CuryFinYtdBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryFinYtdBalance
		{
			get
			{
				return this._CuryFinYtdBalance;
			}
			set
			{
				this._CuryFinYtdBalance = value;
			}
		}
		#endregion
		#region CuryFinBegBalance
		protected Decimal? _CuryFinBegBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryFinBegBalance
		{
			get
			{
				return this._CuryFinBegBalance;
			}
			set
			{
				this._CuryFinBegBalance = value;
			}
		}
		#endregion
		#region CuryTranPtdCredit
		protected Decimal? _CuryTranPtdCredit;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryTranPtdCredit
		{
			get
			{
				return this._CuryTranPtdCredit;
			}
			set
			{
				this._CuryTranPtdCredit = value;
			}
		}
		#endregion
		#region CuryTranPtdDebit
		protected Decimal? _CuryTranPtdDebit;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryTranPtdDebit
		{
			get
			{
				return this._CuryTranPtdDebit;
			}
			set
			{
				this._CuryTranPtdDebit = value;
			}
		}
		#endregion
		#region CuryTranYtdBalance
		protected Decimal? _CuryTranYtdBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryTranYtdBalance
		{
			get
			{
				return this._CuryTranYtdBalance;
			}
			set
			{
				this._CuryTranYtdBalance = value;
			}
		}
		#endregion
		#region CuryTranBegBalance
		protected Decimal? _CuryTranBegBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryTranBegBalance
		{
			get
			{
				return this._CuryTranBegBalance;
			}
			set
			{
				this._CuryTranBegBalance = value;
			}
		}
		#endregion
		#region FinFlag
		protected bool? _FinFlag = true;
		[PXBool()]
		public virtual bool? FinFlag
		{
			get
			{
				return this._FinFlag;
			}
			set
			{
				this._FinFlag = value;
			}
		}
		#endregion
		#region REFlag
		public abstract class rEFlag : PX.Data.IBqlField { }
		[PXBool()]
		public virtual bool? REFlag
		{
			get;
			set;
		}
		#endregion
		#region PtdCredit
		[PXDecimal(4)]
		public virtual Decimal? PtdCredit
		{
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdCredit : this._TranPtdCredit;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdCredit = value;
				}
				else
				{
					this._TranPtdCredit = value;
				}
			}
		}
		#endregion
		#region PtdDebit
		[PXDecimal(4)]
		public virtual Decimal? PtdDebit
		{
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdDebit : this._TranPtdDebit;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdDebit = value;
				}
				else
				{
					this._TranPtdDebit = value;
				}
			}
		}
		#endregion
		#region YtdBalance
		[PXDecimal(4)]
		public virtual Decimal? YtdBalance
		{
			get
			{
				return ((bool)_FinFlag) ? this._FinYtdBalance : this._TranYtdBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinYtdBalance = value;
				}
				else
				{
					this._TranYtdBalance = value;
				}
			}
		}
		#endregion
		#region BegBalance
		[PXDecimal(4)]
		public virtual Decimal? BegBalance
		{
			get
			{
				return ((bool)_FinFlag) ? this._FinBegBalance : this._TranBegBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinBegBalance = value;
				}
				else
				{
					this._TranBegBalance = value;
				}
			}
		}
		#endregion
		#region PtdRevalued
		[PXDecimal(4)]
		public virtual Decimal? PtdRevalued
		{
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdRevalued : null;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdRevalued = value;
				}
			}
		}
		#endregion
		#region CuryPtdCredit
		[PXDecimal(4)]
		public virtual Decimal? CuryPtdCredit
		{
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdCredit : this._CuryTranPtdCredit;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdCredit = value;
				}
				else
				{
					this._CuryTranPtdCredit = value;
				}
			}
		}
		#endregion
		#region CuryPtdDebit
		[PXDecimal(4)]
		public virtual Decimal? CuryPtdDebit
		{
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdDebit : this._CuryTranPtdDebit;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdDebit = value;
				}
				else
				{
					this._CuryTranPtdDebit = value;
				}
			}
		}
		#endregion
		#region CuryYtdBalance
		[PXDecimal(4)]
		public virtual Decimal? CuryYtdBalance
		{
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinYtdBalance : this._CuryTranYtdBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinYtdBalance = value;
				}
				else
				{
					this._CuryTranYtdBalance = value;
				}
			}
		}
		#endregion
		#region CuryBegBalance
		[PXDecimal(4)]
		public virtual Decimal? CuryBegBalance
		{
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinBegBalance : this._CuryTranBegBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinBegBalance = value;
				}
				else
				{
					this._CuryTranBegBalance = value;
				}
			}
		}
		#endregion
		#region tstamp
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
	}
}
