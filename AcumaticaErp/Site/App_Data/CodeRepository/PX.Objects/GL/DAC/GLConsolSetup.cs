namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	using PX.Objects.CS;
	
	[System.SerializableAttribute()]
	public partial class GLConsolSetup : PX.Data.IBqlTable
	{
        #region Selected
        public abstract class selected : IBqlField
        {
        }
        protected bool? _Selected = false;
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Selected")]
        public bool? Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
            }
        }
        #endregion
		#region SetupID
		public abstract class setupID : PX.Data.IBqlField
		{
		}
		protected Int32? _SetupID;
		[PXDBIdentity(IsKey = true)]
		public virtual Int32? SetupID
		{
			get
			{
				return this._SetupID;
			}
			set
			{
				this._SetupID = value;
			}
		}
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsActive;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				this._IsActive = value;
			}
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.IBqlField
		{
		}
		protected Int32? _BranchID;
		[Branch(DisplayName = "Consolidation Branch", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region LedgerId
		public abstract class ledgerId : PX.Data.IBqlField
		{
		}
		protected Int32? _LedgerId;
		[PXDBInt()]
		[PXDefault(typeof(Search<Branch.ledgerID, Where<Branch.branchID, Equal<Current<Batch.branchID>>>>))]
		[PXUIField(DisplayName = "Consolidation Ledger", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search2<Ledger.ledgerID, LeftJoin<Branch, On<Branch.ledgerID, Equal<Ledger.ledgerID>>>, Where<Ledger.balanceType, NotEqual<BudgetLedger>, And<Where<Ledger.balanceType, NotEqual<LedgerBalanceType.actual>, Or<Branch.branchID, Equal<Current<GLConsolSetup.branchID>>>>>>>),
						SubstituteKey = typeof(Ledger.ledgerCD))]
		public virtual Int32? LedgerId
		{
			get
			{
				return this._LedgerId;
			}
			set
			{
				this._LedgerId = value;
			}
		}
		#endregion
		#region SegmentValue
		public abstract class segmentValue : PX.Data.IBqlField
		{
		}
		protected String _SegmentValue;
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Consolidation Segment Value", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<SegmentValue.value, Where<SegmentValue.dimensionID, 
			Equal<SubAccountAttribute.dimensionName>, And<SegmentValue.segmentID,Equal<Current<GLSetup.consolSegmentId>>>>>))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String SegmentValue
		{
			get
			{
				return this._SegmentValue;
			}
			set
			{
				this._SegmentValue = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.IBqlField
		{
		}
		protected String _Description;
		[PXDefault()]
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Consolidation Unit", Visibility = PXUIVisibility.Visible)]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region Login
		public abstract class login : PX.Data.IBqlField
		{
		}
		protected String _Login;
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Username", Visibility = PXUIVisibility.Visible)]
		public virtual String Login
		{
			get
			{
				return this._Login;
			}
			set
			{
				this._Login = value;
			}
		}
		#endregion
		#region Password
		public abstract class password : PX.Data.IBqlField
		{
		}
		protected String _Password;		
		[PXRSACryptStringAttribute(IsUnicode = true)]
		[PXUIField(DisplayName = "Password", Visibility = PXUIVisibility.Visible)]
		public virtual String Password
		{
			get
			{
				return this._Password;
			}
			set
			{
				this._Password = value;
			}
		}
		#endregion
		#region Url
		public abstract class url : PX.Data.IBqlField
		{
		}
		protected String _Url;
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "URL", Visibility = PXUIVisibility.Visible)]
		public virtual String Url
		{
			get
			{
				return this._Url;
			}
			set
			{
				this._Url = value;
			}
		}
		#endregion
		#region SourceLedgerCD
		public abstract class sourceLedgerCD : PX.Data.IBqlField
		{
		}
		protected String _SourceLedgerCD;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Source Ledger")]
		[PXSelector(typeof(Search<GLConsolLedger.ledgerCD, Where<GLConsolLedger.setupID, Equal<Optional<GLConsolSetup.setupID>>>>))]
		public virtual String SourceLedgerCD
		{
			get
			{
				return this._SourceLedgerCD;
			}
			set
			{
				this._SourceLedgerCD = value;
			}
		}
		#endregion
		#region SourceBranchCD
		public abstract class sourceBranchCD : PX.Data.IBqlField
		{
		}
		protected String _SourceBranchCD;
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Source Branch")]
		[PXSelector(typeof(Search<GLConsolBranch.branchCD, Where<GLConsolBranch.setupID, Equal<Optional<GLConsolSetup.setupID>>, And<GLConsolBranch.ledgerCD, Equal<Optional<GLConsolSetup.sourceLedgerCD>>>>>))]
		public virtual String SourceBranchCD
		{
			get
			{
				return this._SourceBranchCD;
			}
			set
			{
				this._SourceBranchCD = value;
			}
		}
		#endregion
		#region PasteFlag
		public abstract class pasteFlag : PX.Data.IBqlField
		{
		}
		protected Boolean? _PasteFlag;
		[PXDBBool()]
		[PXUIField(DisplayName = "Paste Segment Value", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Boolean? PasteFlag
		{
			get
			{
				return this._PasteFlag;
			}
			set
			{
				this._PasteFlag = value;
			}
		}
		#endregion
		#region LastPostPeriod
		public abstract class lastPostPeriod : PX.Data.IBqlField
		{
		}
		protected String _LastPostPeriod;
		[GL.FinPeriodID()]
		[PXUIField(DisplayName = "Last Post Period", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String LastPostPeriod
		{
			get
			{
				return this._LastPostPeriod;
			}
			set
			{
				this._LastPostPeriod = value;
			}
		}
		#endregion
		#region StartPeriod
		public abstract class startPeriod : PX.Data.IBqlField
		{
		}
		protected String _StartPeriod;
		[GL.FinPeriodSelector]
		[PXUIField(DisplayName = "Start Period")]
		public virtual String StartPeriod
		{
			get
			{
				return this._StartPeriod;
			}
			set
			{
				this._StartPeriod = value;
			}
		}
		#endregion
		#region EndPeriod
		public abstract class endPeriod : PX.Data.IBqlField
		{
		}
		protected String _EndPeriod;
		[GL.FinPeriodSelector]
		[PXUIField(DisplayName = "End Period")]
		public virtual String EndPeriod
		{
			get
			{
				return this._EndPeriod;
			}
			set
			{
				this._EndPeriod = value;
			}
		}
		#endregion
		#region LastConsDate
		public abstract class lastConsDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _LastConsDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Last Consolidation Date", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual DateTime? LastConsDate
		{
			get
			{
				return this._LastConsDate;
			}
			set
			{
				this._LastConsDate = value;
			}
		}
		#endregion
		#region BypassAccountSubValidation
		public abstract class bypassAccountSubValidation : PX.Data.IBqlField
		{
		}
		protected Boolean? _BypassAccountSubValidation;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Bypass Account/Sub Validation")]
		public virtual Boolean? BypassAccountSubValidation
		{
			get
			{
				return this._BypassAccountSubValidation;
			}
			set
			{
				this._BypassAccountSubValidation = value;
			}
		}
		#endregion
	}
}
