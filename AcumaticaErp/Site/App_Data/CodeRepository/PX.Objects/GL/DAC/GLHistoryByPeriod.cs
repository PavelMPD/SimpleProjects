namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	[PXProjection(typeof(Select5<GLHistory,
		InnerJoin<FinPeriod, On<FinPeriod.finPeriodID, GreaterEqual<GLHistory.finPeriodID>>>,
	   Aggregate<
	   GroupBy<GLHistory.branchID,
	   GroupBy<GLHistory.ledgerID,
	   GroupBy<GLHistory.accountID,
	   GroupBy<GLHistory.subID,
	   Max<GLHistory.finPeriodID,
		GroupBy<FinPeriod.finPeriodID
        >>>>>>>>))]
	[PXPrimaryGraph(typeof(AccountByPeriodEnq), Filter = typeof(AccountByPeriodFilter))]
	public partial class GLHistoryByPeriod : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.IBqlField
		{
		}
		protected Int32? _BranchID;
		[PXDBInt(IsKey = true, BqlField = typeof(GLHistory.branchID))]
		[PXSelector(typeof(Branch.branchID), SubstituteKey = typeof(Branch.branchCD))]
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
		[PXDBInt(IsKey = true, BqlField = typeof(GLHistory.ledgerID))]
		[PXSelector(typeof(Ledger.ledgerID), SubstituteKey = typeof(Ledger.ledgerCD))]
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
		#region AccountID
		public abstract class accountID : PX.Data.IBqlField
		{
		}
		protected Int32? _AccountID;
		[Account(IsKey = true, BqlField = typeof(GLHistory.accountID))]
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
		protected Int32? _SubID;
		[SubAccount(IsKey = true, BqlField = typeof(GLHistory.subID))]
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
		#region LastActivityPeriod
		public abstract class lastActivityPeriod : PX.Data.IBqlField
		{
		}
		protected String _LastActivityPeriod;
		[GL.FinPeriodID(BqlField = typeof(GLHistory.finPeriodID))]
		public virtual String LastActivityPeriod
		{
			get
			{
				return this._LastActivityPeriod;
			}
			set
			{
				this._LastActivityPeriod = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		protected String _FinPeriodID;
		[GL.FinPeriodID(IsKey = true, BqlField = typeof(FinPeriod.finPeriodID))]
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
	}
}
