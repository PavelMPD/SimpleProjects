using PX.Data.EP;
using PX.Objects.CR;
using PX.Objects.SO;

namespace PX.Objects.AR
{
	using System;
	using PX.Data;
	using PX.Objects.CM;
	using PX.Objects.GL;
	using PX.Objects.CA;
	using PX.Objects.CS;
	using PX.Objects.TX;

	public static class StatementTypes 
	{
		public const string CS_OPEN_ITEM = "O";
		public const string CS_BALANCE_BROUGHT_FORWARD = "B"; 
	}

	public static class CreditRuleTypes
	{
		public const string CS_DAYS_PAST_DUE = "D";
		public const string CS_CREDIT_LIMIT = "C";
		public const string CS_BOTH = "B";
		public const string CS_NO_CHECKING = "N";
	}

    /// <summary>
    /// Fixed List Selector. Defines a name-value pair list of a possible Statement types <br/>
    /// compatible with <see cref="StatementTypes"/>.<br/> 
    /// </summary>
	public class StatementTypeAttribute: PXStringListAttribute
	{
		public StatementTypeAttribute() : base(new string[] { StatementTypes.CS_OPEN_ITEM, StatementTypes.CS_BALANCE_BROUGHT_FORWARD }, new string[] { "Open Item", "Balance Brought Forward" }) { }
	}

    /// <summary>
    /// Fixed List Selector. Defines a name-value pair list of a possible CreditRules <br/>
    /// compatible with <see cref="CreditRuleTypes"/><br/>    
    /// </summary>	
	public class CreditRuleAttribute: PXStringListAttribute
	{
		public CreditRuleAttribute() : base(new string[] { CreditRuleTypes.CS_DAYS_PAST_DUE, CreditRuleTypes.CS_CREDIT_LIMIT, CreditRuleTypes.CS_BOTH, CreditRuleTypes.CS_NO_CHECKING }, new string[] { "Days Past Due", "Credit Limit", "Limit and Days Past Due", "Disabled" }) { }
	}
	[PXCacheName(Messages.CustomerClass)]
	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(CustomerClassMaint))]
	public partial class CustomerClass : PX.Data.IBqlTable
	{
		#region CustomerClassID
		public abstract class customerClassID : PX.Data.IBqlField
		{
		}
		protected String _CustomerClassID;
		
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Class ID", Visibility=PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(CustomerClass.customerClassID), CacheGlobal = true)]
		[PXFieldDescription]
		public virtual String CustomerClassID
		{
			get
			{
				return this._CustomerClassID;
			}
			set
			{
				this._CustomerClassID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected String _Descr;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region TermsID
		public abstract class termsID : PX.Data.IBqlField
		{
		}
		protected String _TermsID;
		[PXDefault(typeof(Search2<CustomerClass.termsID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.customer>, Or<Terms.visibleTo, Equal<TermsVisibleTo.all>>>>), DescriptionField = typeof(Terms.descr), CacheGlobal = true)]
		public virtual String TermsID
		{
			get
			{
				return this._TermsID;
			}
			set
			{
				this._TermsID = value;
			}
		}
			#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.IBqlField
		{
		}
		protected String _TaxZoneID;
		[PXDefault(typeof(Search2<CustomerClass.taxZoneID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Tax Zone ID")]
		[PXSelector(typeof(Search<TaxZone.taxZoneID>), CacheGlobal = true)]
		public virtual String TaxZoneID
		{
			get
			{
				return this._TaxZoneID;
			}
			set
			{
				this._TaxZoneID = value;
			}
		}
		#endregion
		#region RequireTaxZone
		public abstract class requireTaxZone : PX.Data.IBqlField
		{
		}
		protected Boolean? _RequireTaxZone;
		[PXDBBool()]
		[PXDefault(false, typeof(Search2<CustomerClass.requireTaxZone, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Require Tax Zone")]
		public virtual Boolean? RequireTaxZone
		{
			get
			{
				return this._RequireTaxZone;
			}
			set
			{
				this._RequireTaxZone = value;
			}
		}
		#endregion
        #region PriceClassID
        public abstract class priceClassID : PX.Data.IBqlField
        {
        }
        protected String _PriceClassID;
		[PXDefault(typeof(Search2<CustomerClass.priceClassID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
        [PXUIField(DisplayName = "Price Class ID")]
        [PXSelector(typeof(Search<ARPriceClass.priceClassID>), CacheGlobal = true)]
        public virtual String PriceClassID
        {
            get
            {
                return this._PriceClassID;
            }
            set
            {
                this._PriceClassID = value;
            }
        }
        #endregion
		#region CuryID
		public abstract class curyID : PX.Data.IBqlField
		{
		}
		protected String _CuryID;
		[PXDefault(typeof(Search2<CustomerClass.curyID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(5, IsUnicode = true)]
		[PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
		[PXUIField(DisplayName = "Currency ID")]
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
		#region CuryRateTypeID
		public abstract class curyRateTypeID : PX.Data.IBqlField
		{
		}
		protected String _CuryRateTypeID;
		[PXDBString(6, IsUnicode = true)]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXDefault(typeof(Coalesce<Search2<CustomerClass.curyRateTypeID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>,
								  Search<CMSetup.aRRateTypeDflt>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Currency Rate Type")]
		public virtual String CuryRateTypeID
		{
			get
			{
				return this._CuryRateTypeID;
			}
			set
			{
				this._CuryRateTypeID = value;
			}
		}
		#endregion
		#region AllowOverrideCury
		public abstract class allowOverrideCury : PX.Data.IBqlField
		{
		}
		protected Boolean? _AllowOverrideCury;
		[PXDBBool()]
		[PXUIField(DisplayName = "Enable Currency Override")]
		[PXDefault(false, typeof(Coalesce<Search2<CustomerClass.allowOverrideCury, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>,
										  Search<CMSetup.aRCuryOverride>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? AllowOverrideCury
		{
			get
			{
				return this._AllowOverrideCury;
			}
			set
			{
				this._AllowOverrideCury = value;
			}
		}
		#endregion
		#region AllowOverrideRate
		public abstract class allowOverrideRate : PX.Data.IBqlField
		{
		}
		protected Boolean? _AllowOverrideRate;
		[PXDBBool()]
		[PXUIField(DisplayName = "Enable Rate Override")]
		[PXDefault(false, typeof(Coalesce<Search2<CustomerClass.allowOverrideRate, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>,
										  Search<CMSetup.aRRateTypeOverride>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? AllowOverrideRate
		{
			get
			{
				return this._AllowOverrideRate;
			}
			set
			{
				this._AllowOverrideRate = value;
			}
		}
		#endregion
		#region ARAcctID
		public abstract class aRAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _ARAcctID;
		[PXDefault(typeof(Search2<CustomerClass.aRAcctID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "AR Account", Visibility=PXUIVisibility.Visible, DescriptionField=typeof(Account.description))]
		public virtual Int32? ARAcctID
		{
			get
			{
				return this._ARAcctID;
			}
			set
			{
				this._ARAcctID = value;
			}
		}
		#endregion
		#region ARSubID
		public abstract class aRSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _ARSubID;
		[PXDefault(typeof(Search2<CustomerClass.aRSubID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(CustomerClass.aRAcctID), DisplayName = "AR Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? ARSubID
		{
			get
			{
				return this._ARSubID;
			}
			set
			{
				this._ARSubID = value;
			}
		}
		#endregion
        #region DiscountAcctID
        public abstract class discountAcctID : PX.Data.IBqlField
        {
        }
        protected Int32? _DiscountAcctID;
        [PXDefault(typeof(Search2<CustomerClass.discountAcctID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [Account(DisplayName = "Discount Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required = false)]
        public virtual Int32? DiscountAcctID
        {
            get
            {
                return this._DiscountAcctID;
            }
            set
            {
                this._DiscountAcctID = value;
            }
        }
        #endregion
        #region DiscountSubID
        public abstract class discountSubID : PX.Data.IBqlField
        {
        }
        protected Int32? _DiscountSubID;
        [PXDefault(typeof(Search2<CustomerClass.discountSubID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [SubAccount(typeof(CustomerClass.discountAcctID), DisplayName = "Discount Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = false)]
        public virtual Int32? DiscountSubID
        {
            get
            {
                return this._DiscountSubID;
            }
            set
            {
                this._DiscountSubID = value;
            }
        }
        #endregion
		#region DiscTakenAcctID
		public abstract class discTakenAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _DiscTakenAcctID;
		[PXDefault(typeof(Search2<CustomerClass.discTakenAcctID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Cash Discount Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]

		public virtual Int32? DiscTakenAcctID
		{
			get
			{
				return this._DiscTakenAcctID;
			}
			set
			{
				this._DiscTakenAcctID = value;
			}
		}
				#endregion
		#region DiscTakenSubID
		public abstract class discTakenSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _DiscTakenSubID;
		[PXDefault(typeof(Search2<CustomerClass.discTakenSubID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(CustomerClass.discTakenAcctID), DisplayName = "Cash Discount Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]

		public virtual Int32? DiscTakenSubID
		{
			get
			{
				return this._DiscTakenSubID;
			}
			set
			{
				this._DiscTakenSubID = value;
			}
		}
		#endregion
		#region SalesAcctID
		public abstract class salesAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _SalesAcctID;
		[PXDefault(typeof(Search2<CustomerClass.salesAcctID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Sales Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		public virtual Int32? SalesAcctID
		{
			get
			{
				return this._SalesAcctID;
			}
			set
			{
				this._SalesAcctID = value;
			}
		}
		#endregion
		#region SalesSubID
		public abstract class salesSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _SalesSubID;
		[PXDefault(typeof(Search2<CustomerClass.salesSubID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(CustomerClass.salesAcctID), DisplayName = "Sales Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]

		public virtual Int32? SalesSubID
		{
			get
			{
				return this._SalesSubID;
			}
			set
			{
				this._SalesSubID = value;
			}
		}
		#endregion
		#region COGSAcctID
		public abstract class cOGSAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _COGSAcctID;
		[PXDefault(typeof(Search2<CustomerClass.cOGSAcctID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "COGS Account", Visibility=PXUIVisibility.Visible, DescriptionField=typeof(Account.description))]
		public virtual Int32? COGSAcctID
		{
			get
			{
				return this._COGSAcctID;
			}
			set
			{
				this._COGSAcctID = value;
			}
		}
		#endregion
		#region COGSSubID
		public abstract class cOGSSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _COGSSubID;
		[PXDefault(typeof(Search2<CustomerClass.cOGSSubID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(CustomerClass.cOGSAcctID), DisplayName = "COGS Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? COGSSubID
		{
			get
			{
				return this._COGSSubID;
			}
			set
			{
				this._COGSSubID = value;
			}
		}
		#endregion
		#region FreightAcctID
		public abstract class freightAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _FreightAcctID;
		[PXDefault(typeof(Search2<CustomerClass.freightAcctID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Freight Account", Visibility=PXUIVisibility.Visible, DescriptionField=typeof(Account.description))]
		public virtual Int32? FreightAcctID
		{
			get
			{
				return this._FreightAcctID;
			}
			set
			{
				this._FreightAcctID = value;
			}
		}
		#endregion
		#region FreightSubID
		public abstract class freightSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _FreightSubID;
		[PXDefault(typeof(Search2<CustomerClass.freightSubID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(CustomerClass.freightAcctID), DisplayName = "Freight Sub.",
			Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? FreightSubID
		{
			get
			{
				return this._FreightSubID;
			}
			set
			{
				this._FreightSubID = value;
			}
		}
		#endregion
		#region MiscAcctID
		public abstract class miscAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _MiscAcctID;
		[PXDefault(typeof(Search2<CustomerClass.miscAcctID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Misc. Account", Visibility=PXUIVisibility.Visible, DescriptionField=typeof(Account.description))]
		
		public virtual Int32? MiscAcctID
		{
			get
			{
				return this._MiscAcctID;
			}
			set
			{
				this._MiscAcctID = value;
			}
		}
		#endregion
		#region MiscSubID
		public abstract class miscSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _MiscSubID;
		[PXDefault(typeof(Search2<CustomerClass.miscSubID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(CustomerClass.miscAcctID), DisplayName = "Misc. Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		
		public virtual Int32? MiscSubID
		{
			get
			{
				return this._MiscSubID;
			}
			set
			{
				this._MiscSubID = value;
			}
		}
		#endregion
		#region PrepaymentAcctID
		public abstract class prepaymentAcctID : IBqlField
		{
		}
		protected int? _PrepaymentAcctID;
		[PXDefault(typeof(Search2<CustomerClass.prepaymentAcctID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Prepayment Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		public virtual int? PrepaymentAcctID
		{
			get
			{
				return _PrepaymentAcctID;
			}
			set
			{
				_PrepaymentAcctID = value;
			}
		}
		#endregion
		#region PrepaymentSubID
		public abstract class prepaymentSubID : IBqlField
		{
		}
		protected int? _PrepaymentSubID;
		[PXDefault(typeof(Search2<CustomerClass.prepaymentSubID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(CustomerClass.prepaymentAcctID), DisplayName = "Prepayment Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual int? PrepaymentSubID
		{
			get
			{
				return _PrepaymentSubID;
			}
			set
			{
				_PrepaymentSubID = value;
			}
		}
		#endregion
        #region UnrealizedGainAcctID
        public abstract class unrealizedGainAcctID : PX.Data.IBqlField
        {
        }
        protected Int32? _UnrealizedGainAcctID;
        [Account(null,
            DisplayName = "Unrealized Gain Account",
            Visibility = PXUIVisibility.Visible,
            DescriptionField = typeof(Account.description))]
        public virtual Int32? UnrealizedGainAcctID
        {
            get
            {
                return this._UnrealizedGainAcctID;
            }
            set
            {
                this._UnrealizedGainAcctID = value;
            }
        }
        #endregion
        #region UnrealizedGainSubID
        public abstract class unrealizedGainSubID : PX.Data.IBqlField
        {
        }
        protected Int32? _UnrealizedGainSubID;
        [SubAccount(typeof(CustomerClass.unrealizedGainAcctID),
            DescriptionField = typeof(Sub.description),
            DisplayName = "Unrealized Gain Sub.",
            Visibility = PXUIVisibility.Visible)]
        public virtual Int32? UnrealizedGainSubID
        {
            get
            {
                return this._UnrealizedGainSubID;
            }
            set
            {
                this._UnrealizedGainSubID = value;
            }
        }
        #endregion
        #region UnrealizedLossAcctID
        public abstract class unrealizedLossAcctID : PX.Data.IBqlField
        {
        }
        protected Int32? _UnrealizedLossAcctID;
        [Account(null,
            DisplayName = "Unrealized Loss Account",
            Visibility = PXUIVisibility.Visible,
            DescriptionField = typeof(Account.description))]
        public virtual Int32? UnrealizedLossAcctID
        {
            get
            {
                return this._UnrealizedLossAcctID;
            }
            set
            {
                this._UnrealizedLossAcctID = value;
            }
        }
        #endregion
        #region UnrealizedLossSubID
        public abstract class unrealizedLossSubID : PX.Data.IBqlField
        {
        }
        protected Int32? _UnrealizedLossSubID;
        [SubAccount(typeof(CustomerClass.unrealizedLossAcctID),
            DescriptionField = typeof(Sub.description),
            DisplayName = "Unrealized Loss Sub.",
            Visibility = PXUIVisibility.Visible)]
        public virtual Int32? UnrealizedLossSubID
        {
            get
            {
                return this._UnrealizedLossSubID;
            }
            set
            {
                this._UnrealizedLossSubID = value;
            }
        }
        #endregion
		#region AutoApplyPayments
		public abstract class autoApplyPayments : PX.Data.IBqlField
		{
		}
		protected Boolean? _AutoApplyPayments;
		[PXDBBool()]
		[PXDefault(false, typeof(Search2<CustomerClass.autoApplyPayments, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Auto-Apply Payments")]
		public virtual Boolean? AutoApplyPayments
		{
			get
			{
				return this._AutoApplyPayments;
			}
			set
			{
				this._AutoApplyPayments = value;
			}
		}
		#endregion
		#region PrintStatements
		public abstract class printStatements : PX.Data.IBqlField
		{
		}
		protected Boolean? _PrintStatements;
		[PXDBBool()]
		[PXDefault(false, typeof(Search2<CustomerClass.printStatements, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXUIField(DisplayName = "Print Statements")]
		public virtual Boolean? PrintStatements
		{
			get
			{
				return this._PrintStatements;
			}
			set
			{
				this._PrintStatements = value;
			}
		}
		#endregion
		#region PrintCuryStatements
		public abstract class printCuryStatements : PX.Data.IBqlField
		{
		}
		protected Boolean? _PrintCuryStatements;
		[PXDBBool()]
		[PXDefault(false, typeof(Search2<CustomerClass.printCuryStatements, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXUIField(DisplayName = "Multi-Currency Statements")]
		public virtual Boolean? PrintCuryStatements
		{
			get
			{
				return this._PrintCuryStatements;
			}
			set
			{
				this._PrintCuryStatements = value;
			}
		}
		#endregion
		#region SendStatementByEmail
		public abstract class sendStatementByEmail : PX.Data.IBqlField
		{
		}
		protected Boolean? _SendStatementByEmail;
		[PXDBBool()]
		[PXDefault(false, typeof(Search2<CustomerClass.sendStatementByEmail, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXUIField(DisplayName = "Send Statements By Email")]
		public virtual Boolean? SendStatementByEmail
		{
			get
			{
				return this._SendStatementByEmail;
			}
			set
			{
				this._SendStatementByEmail = value;
			}
		}
		#endregion

		#region CreditRule
		public abstract class creditRule : PX.Data.IBqlField
		{
		}
		protected String _CreditRule;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(CreditRuleTypes.CS_DAYS_PAST_DUE, typeof(Search2<CustomerClass.creditRule, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[CreditRule()]
		[PXUIField(DisplayName = "Credit Verification")]
		public virtual String CreditRule
		{
			get
			{
				return this._CreditRule;
			}
			set
			{
				this._CreditRule = value;
			}
		}
		#endregion
		#region CreditLimit
		public abstract class creditLimit : PX.Data.IBqlField
		{
		}
		protected Decimal? _CreditLimit;
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search2<CustomerClass.creditLimit, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXUIField(DisplayName = "Credit Limit")]
		public virtual Decimal? CreditLimit
		{
			get
			{
				return this._CreditLimit;
			}
			set
			{
				this._CreditLimit = value;
			}
		}
		#endregion
		#region CreditDaysPastDue
		public abstract class creditDaysPastDue : PX.Data.IBqlField
		{
		}
		protected Int16? _CreditDaysPastDue;
		[PXDefault((short)0, typeof(Search2<CustomerClass.creditDaysPastDue, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXDBShort(MinValue =0, MaxValue =3650)]
		[PXUIField(DisplayName = "Credit Days Past Due")]
		public virtual Int16? CreditDaysPastDue
		{
			get
			{
				return this._CreditDaysPastDue;
			}
			set
			{
				this._CreditDaysPastDue = value;
			}
		}
		#endregion
		#region StatementType
		public abstract class statementType : PX.Data.IBqlField
		{
		}
		protected String _StatementType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(StatementTypes.CS_OPEN_ITEM, typeof(Search2<CustomerClass.statementType, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[StatementType()]
		[PXUIField(DisplayName = "Statement Type")]
		public virtual String StatementType
		{
			get
			{
				return this._StatementType;
			}
			set
			{
				this._StatementType = value;
			}
		}
		#endregion
		#region StatementCycleId
		public abstract class statementCycleId : PX.Data.IBqlField
		{
		}
		protected String _StatementCycleId;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search2<CustomerClass.statementCycleId, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Statement Cycle ID")]
		[PXSelector(typeof(ARStatementCycle.statementCycleId))]
		public virtual String StatementCycleId
		{
			get
			{
				return this._StatementCycleId;
			}
			set
			{
				this._StatementCycleId = value;
			}
		}
		#endregion
		#region SmallBalanceAllow
		public abstract class smallBalanceAllow : PX.Data.IBqlField
		{
		}
		protected Boolean? _SmallBalanceAllow;
		[PXDBBool()]
		[PXDefault(false, typeof(Search2<CustomerClass.smallBalanceAllow, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXUIField(DisplayName = "Enable Write-Offs")]
		public virtual Boolean? SmallBalanceAllow
		{
			get
			{
				return this._SmallBalanceAllow;
			}
			set
			{
				this._SmallBalanceAllow = value;
			}
		}
		#endregion
		#region SmallBalanceLimit
		public abstract class smallBalanceLimit : PX.Data.IBqlField
		{
		}
		protected Decimal? _SmallBalanceLimit;
		[PXDBCury(typeof(CustomerClass.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search2<CustomerClass.smallBalanceLimit, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXUIField(DisplayName = "Write-Off Limit")]
		public virtual Decimal? SmallBalanceLimit
		{
			get
			{
				return this._SmallBalanceLimit;
			}
			set
			{
				this._SmallBalanceLimit = value;
			}
		}
		#endregion
		#region FinChargeApply
		public abstract class finChargeApply : PX.Data.IBqlField
		{
		}
		protected Boolean? _FinChargeApply;
		[PXDBBool()]
		[PXDefault(false, typeof(Search2<CustomerClass.finChargeApply, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXUIField(DisplayName = "Apply Overdue Charges")]
		public virtual Boolean? FinChargeApply
		{
			get
			{
				return this._FinChargeApply;
			}
			set
			{
				this._FinChargeApply = value;
			}
		}
		#endregion
		#region FinChargeID
		public abstract class finChargeID : PX.Data.IBqlField
		{
		}
		protected String _FinChargeID;
		[PXDefault(typeof(Search2<CustomerClass.finChargeID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Overdue Charge ID")]
		[PXSelector(typeof(ARFinCharge.finChargeID), DescriptionField = typeof(ARFinCharge.finChargeDesc))]
		public virtual String FinChargeID
		{
			get
			{
				return this._FinChargeID;
			}
			set
			{
				this._FinChargeID = value;
			}
		}
		#endregion
		#region CountryID
		public abstract class countryID : PX.Data.IBqlField
		{
		}
		protected String _CountryID;
		[PXDefault(typeof(Search<GL.Branch.countryID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Country")]
		[PXSelector(typeof(Search<Country.countryID>), DescriptionField = typeof(Country.description), CacheGlobal = true)]
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
		#region OverLimitAmount
		public abstract class overLimitAmount : PX.Data.IBqlField
		{
		}
		protected Decimal? _OverLimitAmount;
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search2<CustomerClass.overLimitAmount, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXUIField(DisplayName = "Over-Limit Amount")]
		public virtual Decimal? OverLimitAmount
		{
			get
			{
				return this._OverLimitAmount;
			}
			set
			{
				this._OverLimitAmount = value;
			}
		}
		#endregion

		#region DefPaymentMethodID
		public abstract class defPaymentMethodID : PX.Data.IBqlField
		{
		}
		protected String _DefPaymentMethodID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Method")]
		[PXDefault(typeof(Search2<CustomerClass.defPaymentMethodID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID, Where<PaymentMethod.useForAR, Equal<True>,And<PaymentMethod.isActive, Equal<boolTrue>>>>), DescriptionField = typeof(PaymentMethod.descr), CacheGlobal = true)]
		public virtual String DefPaymentMethodID
		{
			get
			{
				return this._DefPaymentMethodID;
			}
			set
			{
				this._DefPaymentMethodID = value;
			}
		}
		#endregion
		#region PrintInvoices
		public abstract class printInvoices : PX.Data.IBqlField
		{
		}
		protected Boolean? _PrintInvoices;
		[PXDBBool()]
		[PXDefault(false, typeof(Search2<CustomerClass.printInvoices, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXUIField(DisplayName = "Print Invoices")]
		public virtual Boolean? PrintInvoices
		{
			get
			{
				return this._PrintInvoices;
			}
			set
			{
				this._PrintInvoices = value;
			}
		}
		#endregion
		#region MailInvoices
		public abstract class mailInvoices : PX.Data.IBqlField
		{
		}
		protected Boolean? _MailInvoices;
		[PXDBBool()]
		[PXDefault(false, typeof(Search2<CustomerClass.mailInvoices, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXUIField(DisplayName = "Send Invoices by Email")]
		public virtual Boolean? MailInvoices
		{
			get
			{
				return this._MailInvoices;
			}
			set
			{
				this._MailInvoices = value;
			}
		}
		#endregion
		#region DefaultLocationCDFromBranch
		public abstract class defaultLocationCDFromBranch : IBqlField
		{
		}
		protected bool? _DefaultLocationCDFromBranch;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Default Location ID from Branch")]
		public virtual bool? DefaultLocationCDFromBranch
		{
			get
			{
				return _DefaultLocationCDFromBranch;
			}
			set
			{
				_DefaultLocationCDFromBranch = value;
			}
		}
		#endregion

		#region ShipVia
		public abstract class shipVia : IBqlField
		{
		}
		protected string _ShipVia;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Ship Via")]
		[PXSelector(typeof(Search<Carrier.carrierID>), DescriptionField = typeof(Carrier.description), CacheGlobal = true)]
		public virtual string ShipVia
		{
			get
			{
				return _ShipVia;
			}
			set
			{
				_ShipVia = value;
			}
		}
		#endregion
		#region ShipComplete
		public abstract class shipComplete : IBqlField
		{
		}
		protected string _ShipComplete;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(SOShipComplete.CancelRemainder)]
		[SOShipComplete.List]
		[PXUIField(DisplayName = "Ship Complete")]
		public virtual string ShipComplete
		{
			get
			{
				return _ShipComplete;
			}
			set
			{
				_ShipComplete = value;
			}
		}
		#endregion
		#region ShipTermsID
		public abstract class shipTermsID : IBqlField
		{
		}
		protected string _ShipTermsID;
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Shipping Terms")]
		[PXSelector(typeof(ShipTerms.shipTermsID), CacheGlobal = true)]
		public virtual string ShipTermsID
		{
			get
			{
				return _ShipTermsID;
			}
			set
			{
				_ShipTermsID = value;
			}
		}
		#endregion
		#region SalesPersonID
		public abstract class salesPersonID : IBqlField
		{
		}
		protected int? _SalesPersonID;
		[SalesPerson]
		[PXDefault(typeof(Search<CustSalesPeople.salesPersonID, Where<CustSalesPeople.isDefault, Equal<boolTrue>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? SalesPersonID
		{
			get
			{
				return _SalesPersonID;
			}
			set
			{
				_SalesPersonID = value;
			}
		}
		#endregion

        #region DiscountLimit
        public abstract class discountLimit : PX.Data.IBqlField
        {
        }
        protected Decimal? _DiscountLimit;
        [PXDBDecimal(MaxValue = 100, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "50.0")]
        [PXUIField(DisplayName = "Group/Document Discount Limit (%)")]
        public virtual Decimal? DiscountLimit
        {
            get
            {
                return this._DiscountLimit;
            }
            set
            {
                this._DiscountLimit = value;
            }
        }
        #endregion

		#region NoteID
		public abstract class noteID : PX.Data.IBqlField { }
		[PXNote(DescriptionField = typeof(CustomerClass.customerClassID))]
		public virtual Int64? NoteID { get; set; }
		#endregion

		#region GroupMask
		public abstract class groupMask : IBqlField
		{
		}
		protected Byte[] _GroupMask;
		[SingleGroup]
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
	}
}
