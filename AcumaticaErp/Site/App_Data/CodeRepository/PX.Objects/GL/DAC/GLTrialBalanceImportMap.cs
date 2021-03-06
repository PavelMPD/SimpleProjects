using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CS;

namespace PX.Objects.GL
{
	[Serializable]
	[PXPrimaryGraph(typeof(JournalEntryImport))]
	[PXCacheName(Messages.GLTrialBalanceImportMap)]
	public partial class GLTrialBalanceImportMap : IBqlTable
	{
        #region BranchID
        public abstract class branchID : PX.Data.IBqlField
        {
        }
        protected Int32? _BranchID;
        [Branch()]
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

		#region Number
		public abstract class number : IBqlField { }

		protected String _Number;

		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Import Number", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(GLTrialBalanceImportMap.number))]
		[AutoNumber(typeof(GLSetup.tBImportNumberingID), typeof(GLTrialBalanceImportMap.importDate))]
		[PXFieldDescription]
		public virtual String Number
		{
			get { return _Number; }
			set { _Number = value; }
		}

		#endregion

		#region BatchNbr
		public abstract class batchNbr : IBqlField { }
		protected String _BatchNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleGL>>>))]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual String BatchNbr
		{
			get { return _BatchNbr; }
			set { _BatchNbr = value; }
		}
		#endregion

		#region ImportDate
		public abstract class importDate : IBqlField { }

		protected DateTime? _ImportDate;
		[PXDBDate(MaxValue = "06/06/2079", MinValue = "01/01/1900")]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Import Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? ImportDate
		{
			get { return _ImportDate; }
			set { _ImportDate = value; }
		}
		#endregion

		#region FinPeriodID
		public abstract class finPeriodID : IBqlField { }

		protected String _FinPeriodID;
		[OpenPeriod(typeof(GLTrialBalanceImportMap.importDate))]
		[PXDefault()]
		[PXUIField(DisplayName = "Period", Visibility = PXUIVisibility.Visible)]
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

		#region BegFinPeriod
		public abstract class begFinPeriod : IBqlField { }

		public virtual String BegFinPeriod
		{
			[PXDependsOnFields(typeof(finPeriodID))]
			get
			{
				return this._FinPeriodID == null ? null : 
					string.Concat(FiscalPeriodUtils.FiscalYear(this._FinPeriodID), "01");
			}
		}
		#endregion

		#region Description
		public abstract class description : IBqlField { }

		protected String _Description;
		[PXDBString]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible)]
		public virtual String Description
		{
			get { return _Description; }
			set { _Description = value; }
		}
		#endregion

		#region LedgerID
		public abstract class ledgerID : PX.Data.IBqlField
		{
		}
		protected Int32? _LedgerID;
		[PXDBInt]
		[PXDefault(typeof(Search<Branch.ledgerID, Where<Branch.branchID, Equal<Current<GLTrialBalanceImportMap.branchID>>>>))]
		[PXUIField(DisplayName = "Ledger", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search2<Ledger.ledgerID, LeftJoin<Branch, On<Branch.ledgerID, Equal<Ledger.ledgerID>>>, 
                            Where<Ledger.balanceType, NotEqual<BudgetLedger>, 
                            And<Where<Ledger.balanceType, NotEqual<LedgerBalanceType.actual>, 
                            Or<Branch.branchID, Equal<Optional<GLTrialBalanceImportMap.branchID>>>>>>>), SubstituteKey = typeof(Ledger.ledgerCD))]
		public virtual Int32? LedgerID
		{
			get { return _LedgerID; }
			set { _LedgerID = value; }
		}
		#endregion

		#region IsHold
		public abstract class isHold : IBqlField { }

		protected Boolean? _IsHold;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Hold")]
		public virtual Boolean? IsHold
		{
			get { return _IsHold; }
			set { _IsHold = value; }
		}
		#endregion

		#region Status
		public abstract class status : IBqlField { }

		protected Int32? _Status;
		[PXDBInt]
		[TrialBalanceImportMapStatus]
		[PXDefault(TrialBalanceImportMapStatusAttribute.HOLD)]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		public virtual Int32? Status
		{
			get { return _Status; }
			set { _Status = value; }
		}
		#endregion

		#region CreditTotalBalance
		public abstract class creditTotalBalance : IBqlField { }

		protected Decimal? _CreditTotalBalance;

		[CM.PXDBBaseCury(typeof(GLTrialBalanceImportMap.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Credit Total", Enabled = false)]
		public virtual Decimal? CreditTotalBalance
		{
			get { return _CreditTotalBalance; }
			set { _CreditTotalBalance = value; }
		}
		#endregion

		#region DebitTotalBalance
		public abstract class debitTotalBalance : IBqlField { }

		protected Decimal? _DebitTotalBalance;
		
		[CM.PXDBBaseCury(typeof(GLTrialBalanceImportMap.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Debit Total", Enabled = false)]
		public virtual Decimal? DebitTotalBalance
		{
			get { return _DebitTotalBalance; }
			set { _DebitTotalBalance = value; }
		}
		#endregion

		#region TotalBalance
		public abstract class totalBalance : IBqlField { }

		protected Decimal? _TotalBalance;

		[CM.PXDBBaseCury(typeof(GLTrialBalanceImportMap.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Control Total", Enabled = false)]
		public virtual Decimal? TotalBalance
		{
			get
			{
				return _TotalBalance;
			}
			set
			{
				_TotalBalance = value;
			}
		}
		#endregion

        #region LineCntr
        public abstract class lineCntr : PX.Data.IBqlField
        {
        }
        protected Int32? _LineCntr;
        [PXDBInt()]
        [PXDefault(0)]
        public virtual Int32? LineCntr
        {
            get
            {
                return this._LineCntr;
            }
            set
            {
                this._LineCntr = value;
            }
        }
        #endregion

		#region NoteID
		public abstract class noteID : PX.Data.IBqlField { }
		[PXNote(DescriptionField = typeof(GLTrialBalanceImportMap.number))]
		public virtual Int64? NoteID { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.IBqlField
		{
		}
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

		#region CreatedByID
		public abstract class createdByID : PX.Data.IBqlField
		{
		}
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.IBqlField
		{
		}
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.IBqlField
		{
		}
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		[PXUIField(DisplayName = "Last Modified", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion

		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.IBqlField
		{
		}
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion		
	}

}
