using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.GL
{
	[System.SerializableAttribute()]
	[CRCacheIndependentPrimaryGraphList(
        new Type[] { typeof(BranchMaint), typeof(BranchMaint), typeof(BranchMaint) },
		new Type[] { typeof(Select<BranchMaint.BranchBAccount, Where<BranchMaint.BranchBAccount.branchBAccountID, Equal<Current<Branch.bAccountID>>>>),
                     typeof(Where<Branch.branchID, Less<Zero>>),
                     typeof(Where<True, Equal<True>>)})]
    [PXCacheName(CS.Messages.BranchMaint)]
    public partial class Branch : PX.Data.IBqlTable, PX.SM.IIncludable
	{
		#region BranchID
		public abstract class branchID : PX.Data.IBqlField
		{
		}
		protected Int32? _BranchID;
		[PXDBIdentity()]
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
		#region BranchCD
		public abstract class branchCD : PX.Data.IBqlField
		{
		}
		protected String _BranchCD;
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDBLiteDefault(typeof(BAccount.acctCD))]
		[PXDimensionSelector("BIZACCT", typeof(Search<Branch.branchCD, Where<Match<Current<AccessInfo.userName>>>>), typeof(Branch.branchCD))]
		[PXUIField(DisplayName = "Branch", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String BranchCD
		{
			get
			{
				return this._BranchCD;
			}
			set
			{
				this._BranchCD = value;
			}
		}
		#endregion
		#region RoleName
		public abstract class roleName : IBqlField { }
		private string _RoleName;
		[PXDBString(64, IsUnicode = true, InputMask = "")]
		[PXSelector(typeof(Search<Roles.rolename, Where<Roles.guest, Equal<False>>>), DescriptionField = typeof(Roles.descr))]
		[PXUIField(DisplayName = "Access Role")]
		public string RoleName
		{
			get
			{
				return _RoleName;
			}
			set
			{
				_RoleName = value;
			}
		}
		#endregion
		#region LogoName
		public abstract class logoName : IBqlField { }

		[PXDBString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Logo")]
		public string LogoName { get; set; }
		#endregion
		#region LedgerID
		public abstract class ledgerID : PX.Data.IBqlField
		{
		}
		protected Int32? _LedgerID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Posting Ledger", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<Ledger.ledgerID, Where<Ledger.balanceType, Equal<ActualLedger>>>), DescriptionField = typeof(Ledger.descr), SubstituteKey = typeof(Ledger.ledgerCD))]
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
		#region LedgerCD
		public abstract class ledgerCD : PX.Data.IBqlField
		{
		}
		protected string _LedgerCD;
		[PXString(10, IsUnicode = true)]
		public virtual string LedgerCD
		{
			get
			{
				return this._LedgerCD;
			}
			set
			{
				this._LedgerCD = value;
			}
		}
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _BAccountID;
		[PXDBInt()]
		[PXUIField(Visible = true, Enabled = false)]
		public virtual Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		#region Active
		public abstract class active : IBqlField { }
		[PXDBBool()]
		[PXUIField(DisplayName = "Active")]
		[PXDefault(true)]
		public bool? Active
		{
			get;
			set;
		}
		#endregion
		#region PhoneMask
		public abstract class phoneMask : PX.Data.IBqlField
		{
		}
		protected String _PhoneMask;
		[PXDBString(50)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Phone Mask")]
		public virtual String PhoneMask
		{
			get
			{
				return this._PhoneMask;
			}
			set
			{
				this._PhoneMask = value;
			}
		}
		#endregion
		#region CountryID
		public abstract class countryID : PX.Data.IBqlField
		{
		}
		protected String _CountryID;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Default Country")]
		[PXSelector(typeof(Country.countryID), DescriptionField = typeof(Country.description))]
		public virtual String CountryID
		{
			get
			{
				return this._CountryID;
			}
			set
			{
				this._CountryID = value;
			}
		}
		#endregion
		#region GroupMask
		public abstract class groupMask : IBqlField { }
		protected Byte[] _GroupMask;
		[PXDBGroupMask()]
		public virtual Byte[] GroupMask
		{
			get
			{
				return this._GroupMask;
			}
			set
			{
				this._GroupMask = value;
			}
		}
		#endregion
		#region AcctMapNbr
		public abstract class acctMapNbr : PX.Data.IBqlField
		{
		}
		protected int? _AcctMapNbr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual int? AcctMapNbr
		{
			get
			{
				return this._AcctMapNbr;
			}
			set
			{
				this._AcctMapNbr = value;
			}
		}
		#endregion
		#region AcctName
		public abstract class acctName : PX.Data.IBqlField
		{
		}
		protected String _AcctName;
		[PXDBScalar(typeof(Search<BAccountR.acctName, Where<BAccountR.bAccountID, Equal<Branch.bAccountID>>>))]
		[PXString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Branch Name", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String AcctName
		{
			get
			{
				return this._AcctName;
			}
			set
			{
				this._AcctName = value;
			}
		}
		#endregion
		#region BaseCuryID
		public abstract class baseCuryID : PX.Data.IBqlField
		{
		}
		protected String _BaseCuryID;
		[PXDBScalar(typeof(Search<Company.baseCuryID>))]
		[PXString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Base Currency ID", Enabled = false)]
		[PXSelector(typeof(Search<Currency.curyID>))]
		public virtual String BaseCuryID
		{
			get
			{
				return this._BaseCuryID;
			}
			set
			{
				this._BaseCuryID = value;
			}
		}
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

		#region Included
		public abstract class included : PX.Data.IBqlField
		{
		}
		protected bool? _Included;
		[PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXBool]
		[PXUIField(DisplayName = "Included")]
		public virtual bool? Included
		{
			get
			{
				return this._Included;
			}
			set
			{
				this._Included = value;
			}
		}
        #endregion

        #region AllowsRUTROT
        public abstract class allowsRUTROT : IBqlField { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Uses ROT & RUT deduction", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual Boolean? AllowsRUTROT { get; set; }
        #endregion
        #region RUTROTDeductionPct
        public abstract class rUTROTDeductionPct : IBqlField { }

        [PXDBDecimal(MinValue = 0, MaxValue = 100)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Deduction,%", FieldClass = AR.RUTROTMessages.FieldClass)]
        public Decimal? RUTROTDeductionPct { get; set; }
        #endregion
        #region RUTROTPersonalAllowanceLimit
        public abstract class rUTROTPersonalAllowanceLimit : IBqlField { }

        [PXDBDecimal(MinValue = 0, MaxValue = 100000000)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Personal allowance limit", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual Decimal? RUTROTPersonalAllowanceLimit { get; set; }
        #endregion
        #region RUTROTCuryID
        public abstract class rUTROTCuryID : PX.Data.IBqlField
        {
        }

        [PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
        [PXUIField(DisplayName = "Currency", FieldClass = AR.RUTROTMessages.FieldClass)]
        [PXSelector(typeof(Currency.curyID))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual String RUTROTCuryID { get; set; }
        #endregion
        #region RUTROTClaimNextRefNbr
        public abstract class rUTROTClaimNextRefNbr : PX.Data.IBqlField { }

        [PXDBInt(MinValue = 0, MaxValue = 100000000)]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Next Claim Nbr", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual int? RUTROTClaimNextRefNbr { get; set; }
        #endregion
        #region RUTROTOrgNbrValidRegEx
        public abstract class rUTROTOrgNbrValidRegEx : PX.Data.IBqlField
        {
        }

        [PXDBString(255)]
        [PXUIField(DisplayName = "Org. Nbr. Validation Reg. Exp.", FieldClass = AR.RUTROTMessages.FieldClass)]
        public virtual String RUTROTOrgNbrValidRegEx { get; set; }
        #endregion
    }
    public class BranchAlias : Branch
    {
        #region BranchID
        public new abstract class branchID : PX.Data.IBqlField
        {
        }
        #endregion
        #region BranchCD
        public new abstract class branchCD : PX.Data.IBqlField
        {
        }
        #endregion
        #region RoleName
        public new abstract class roleName : IBqlField { }
       #endregion
        #region LogoName
        public new abstract class logoName : IBqlField { }
        #endregion
        #region LedgerID
        public new abstract class ledgerID : PX.Data.IBqlField
        {
        }
        #endregion
        #region LedgerCD
        public new abstract class ledgerCD : PX.Data.IBqlField
        {
        }
        #endregion
        #region BAccountID
        public new abstract class bAccountID : PX.Data.IBqlField
        {
        }
        #endregion
        #region Active
        public new abstract class active : IBqlField { }
        #endregion
        #region PhoneMask
        public new abstract class phoneMask : PX.Data.IBqlField
        {
        }
        #endregion
        #region CountryID
        public new abstract class countryID : PX.Data.IBqlField
        {
        }
        #endregion
        #region GroupMask
        public new abstract class groupMask : IBqlField { }
        #endregion
        #region AcctMapNbr
        public new abstract class acctMapNbr : PX.Data.IBqlField
        {
        }
        #endregion
        #region AcctName
        public new abstract class acctName : PX.Data.IBqlField
        {
        }
        #endregion
        #region BaseCuryID
        public new abstract class baseCuryID : PX.Data.IBqlField
        {
        }
        #endregion
        #region tstamp
        public new abstract class Tstamp : PX.Data.IBqlField
        {
        }
        #endregion
        #region Included
        public new abstract class included : PX.Data.IBqlField
        {
        }
        #endregion
    }
}
