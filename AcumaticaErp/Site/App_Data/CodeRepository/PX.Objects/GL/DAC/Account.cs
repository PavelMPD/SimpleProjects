namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	using PX.Objects.CM;
	using PX.Objects.PM;

	public static class AccountType
	{
		public static string[] COAOrderOptions = new string[] { "1233", "1234", "3412", "2311" };

		public static int Ordinal(string Type)
		{
			switch (Type)
			{ 
				case Asset:
					return 0;
				case Liability:
					return 1;
				case Income:
					return 2;
				case Expense:
					return 3;
				default:
					throw new PXArgumentException();
			}
		}

		public static string Literal(short Ordinal)
		{
			switch (Ordinal)
			{
				case 0:
					return Asset;
				case 1:
					return Liability;
				case 2:
					return Income;
				case 3:
					return Expense;
				default:
					throw new PXArgumentException();
			}
		}

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Asset, Liability, Income, Expense },
				new string[] { Messages.Asset, Messages.Liability, Messages.Income, Messages.Expense }) { }
		}

		public const string Asset = "A";
		public const string Liability = "L";
		public const string Income = "I";
		public const string Expense = "E";

		public class asset : Constant<string>
		{
			public asset() : base(Asset) { ;}
		}

		public class liability : Constant<string>
		{
			public liability() : base(Liability) { ;}
		}

		public class income : Constant<string>
		{
			public income() : base(Income) { ;}
		}

		public class expense : Constant<string>
		{
			public expense() : base(Expense) { ;}
		}
	}

	public static class AccountPostOption 
	{
		public const string Summary ="S";
		public const string Detail = "D";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Summary, Detail},
				new string[] { Messages.PostSummary, Messages.PostDetail }) { }
		}
	}
	
	[System.SerializableAttribute()]
	[PXCacheName(Messages.Account)]
	[PXPrimaryGraph(typeof(AccountHistoryByYearEnq), Filter = typeof(AccountByYearFilter))]
	public partial class Account : PX.Data.IBqlTable, PX.SM.IIncludable
	{
		#region AccountID
		public abstract class accountID : PX.Data.IBqlField
		{
		}
		protected Int32? _AccountID;
		[PXDBIdentity()]
		[PXUIField(DisplayName = "Account ID", Visibility = PXUIVisibility.Visible, Visible = false)]
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
		#region AccountCD
		public abstract class accountCD : PX.Data.IBqlField
		{
		}
		protected String _AccountCD;
		[PXDefault()]
		[AccountRaw(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String AccountCD
		{
			get
			{
				return this._AccountCD;
			}
			set
			{
				this._AccountCD = value;
			}
		}
		#endregion
		#region AccountClassID
		public abstract class accountClassID : PX.Data.IBqlField
		{
		}
		protected string _AccountClassID;
		[PXDBString(20, IsUnicode = true)]
		[PXUIField(DisplayName = "Account Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(AccountClass.accountClassID), DescriptionField=typeof(AccountClass.descr))]
		public virtual string AccountClassID
		{
			get
			{
				return this._AccountClassID;
			}
			set
			{
				this._AccountClassID = value;
			}
		}
		#endregion
        #region Type
        public abstract class type : PX.Data.IBqlField
        {
        }
        protected string _Type;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(AccountType.Asset, typeof(Search<AccountClass.type, Where<AccountClass.accountClassID, Equal<Current<Account.accountClassID>>>>))]
        [AccountType.List()]
        [PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region COAOrder
		public abstract class cOAOrder : PX.Data.IBqlField
		{
		}
		protected short? _COAOrder;
		[PXDBShort(MinValue = 0, MaxValue = 255)]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "COAOrder", Visibility = PXUIVisibility.Visible)]
		public virtual short? COAOrder
		{
			get
			{
				return this._COAOrder;
			}
			set
			{
				this._COAOrder = value;
			}
		}
		#endregion
        #region Active
		public abstract class active : PX.Data.IBqlField
		{
		}
		protected Boolean? _Active;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? Active
		{
			get
			{
				return this._Active;
			}
			set
			{
				this._Active = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.IBqlField
		{
		}
		protected String _Description;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region PostOption
		public abstract class postOption : PX.Data.IBqlField
		{
		}
		protected string _PostOption;
		[PXDBString(1)]
		[PXDefault(AccountPostOption.Detail)]
		[AccountPostOption.List()]
		[PXUIField(DisplayName = "Post Option", Visibility = PXUIVisibility.Visible)]
		public virtual string PostOption
		{
			get
			{
				return this._PostOption;
			}
			set
			{
				this._PostOption = value;
			}
		}
		#endregion
		#region DirectPost
		public abstract class directPost : PX.Data.IBqlField
		{
		}
		protected Boolean? _DirectPost;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Direct Post", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? DirectPost
		{
			get
			{
				return this._DirectPost;
			}
			set
			{
				this._DirectPost = value;
			}
		}
		#endregion
		#region NoSubDetail
		public abstract class noSubDetail : PX.Data.IBqlField
		{
		}
		protected Boolean? _NoSubDetail;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Default Sub", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? NoSubDetail
		{
			get
			{
				return this._NoSubDetail;
			}
			set
			{
				this._NoSubDetail = value;
			}
		}
		#endregion
		#region RequireUnits
		public abstract class requireUnits : PX.Data.IBqlField
		{
		}
		protected Boolean? _RequireUnits;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Units", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? RequireUnits
		{
			get
			{
				return this._RequireUnits;
			}
			set
			{
				this._RequireUnits = value;
			}
		}
		#endregion
		#region GLConsolAccountCD
		public abstract class gLConsolAccountCD : PX.Data.IBqlField
		{
		}
		protected String _GLConsolAccountCD;
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Consolidation Account", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(GLConsolAccount.accountCD), DescriptionField = typeof(GLConsolAccount.description))]
		public virtual String GLConsolAccountCD
		{
			get
			{
				return this._GLConsolAccountCD;
			}
			set
			{
				this._GLConsolAccountCD = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.IBqlField
		{
		}
		protected string _CuryID;
		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Currency.curyID))]
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
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.IBqlField
		{
		}
		protected Int32? _AccountGroupID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Account Group", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector(AccountGroupAttribute.DimensionName, typeof(Search<PMAccountGroup.groupID, 
			Where<PMAccountGroup.isActive, Equal<True>,
				And2<
					Where2<Where<Current<Account.type>, Equal<AccountType.asset>,
							Or<Current<Account.type>, Equal<AccountType.liability>>>,
						And<Where<PMAccountGroup.type, Equal<AccountType.asset>, 
							Or<PMAccountGroup.type, Equal<AccountType.liability>>>>>,
					Or2<Where<Current<Account.type>, Equal<AccountType.expense>,
							Or<Current<Account.type>, Equal<AccountType.income>>>,
						And<Where<PMAccountGroup.type, Equal<AccountType.expense>,
							Or<PMAccountGroup.type, Equal<AccountType.income>>>>>>>>),
				typeof(PMAccountGroup.groupCD), typeof(PMAccountGroup.groupCD), typeof(PMAccountGroup.description), typeof(PMAccountGroup.type), typeof(PMAccountGroup.isActive))]
		public virtual Int32? AccountGroupID
		{
			get
			{
				return this._AccountGroupID;
			}
			set
			{
				this._AccountGroupID = value;
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
		#region RevalCuryRateTypeId
		public abstract class revalCuryRateTypeId : PX.Data.IBqlField
		{
		}
		protected String _RevalCuryRateTypeId;
		[PXDBString(6, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "Revaluation Rate Type")]
		public virtual String RevalCuryRateTypeId
		{
			get
			{
				return this._RevalCuryRateTypeId;
			}
			set
			{
				this._RevalCuryRateTypeId = value;
			}
		}
		#endregion
		#region Box1099
		public abstract class box1099 : PX.Data.IBqlField
		{
		}
		protected Int16? _Box1099;
		[PXDBShort()]
		[PXIntList(new int[] { 0 }, new string[] { Messages.Undefined })]
		[PXUIField(DisplayName = "1099 Box", Visibility = PXUIVisibility.Visible)]
		public virtual Int16? Box1099
		{
			get
			{
				return this._Box1099;
			}
			set
			{
				this._Box1099 = value;
			}
		}
		#endregion
		#region NoteID

		public abstract class noteID : IBqlField { }

		[PXNote(DescriptionField = typeof(Account.accountCD))]
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
		#region IsCashAccount
		public abstract class isCashAccount : PX.Data.IBqlField
		{
		}
        protected bool? _IsCashAccount;
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual bool? IsCashAccount
		{
			get
			{
				return this._IsCashAccount;
			}
			set
			{
                this._IsCashAccount = value;
			}
		}
		#endregion

		#region TypeTotal
		// this field is used in reports only
		public abstract class typeTotal { }
		[PXString(1)]
		[PXDefault(AccountType.Asset)]
		[PXStringList(new string[]{AccountType.Asset, AccountType.Liability, AccountType.Income, AccountType.Expense},
					new string[]{"Assets Total", "Liability Total", "Income Total", "Expense Total"})]
		[PXUIField]
		public virtual string TypeTotal
		{
			get { return this.Type; }
			set { }
		}
		#endregion

		#region ReadableActive
		// this field is used in reports only
		public abstract class readableActive { }
		[PXInt]
		[PXDefault(1)]
		[PXIntList(new int[] { 1, 0 },
					new string[] { "Yes", "No" })]
		[PXUIField]
		public virtual int? ReadableActive
		{
			[PXDependsOnFields(typeof(active))]
			get
			{
				return this.Active == null || this.Active.Value == false ? 0 : 1;
			}
			set { }
		}
		#endregion
		
		#region TransactionsForGivenCurrencyExists

		public abstract class transactionsForGivenCurrencyExists { }
		[PXBool()]
		public virtual bool? TransactionsForGivenCurrencyExists {get; set;}
		
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
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.IBqlField
		{
		}
		protected String _TaxCategoryID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TX.TaxCategory.taxCategoryID), DescriptionField = typeof(TX.TaxCategory.descr))]
		[PXRestrictor(typeof(Where<TX.TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TX.TaxCategory.taxCategoryID))]
		public virtual String TaxCategoryID
		{
			get
			{
				return this._TaxCategoryID;
			}
			set
			{
				this._TaxCategoryID = value;
			}
		}
		#endregion
	}
}
