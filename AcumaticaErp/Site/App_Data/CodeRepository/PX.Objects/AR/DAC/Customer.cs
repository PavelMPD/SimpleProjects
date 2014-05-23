using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.GL;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.TX;
using PX.Objects.CR;
using PX.TM;
using PX.Objects.AP;

namespace PX.Objects.AR
{
	[System.SerializableAttribute()]
	public partial class CustomerMaster : Customer
	{
		#region BAccountID
		public new abstract class bAccountID : PX.Data.IBqlField
		{
		}
		[Customer(IsKey = true, DisplayName = "Customer ID")]
		public override Int32? BAccountID
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
		#region AcctCD
		public new abstract class acctCD : PX.Data.IBqlField
		{
		}
		[PXDBString(30, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Customer ID", Visibility = PXUIVisibility.SelectorVisible)]
		public override String AcctCD
		{
			get
			{
				return this._AcctCD;
			}
			set
			{
				this._AcctCD = value;
			}
		}
		#endregion
	}


	[System.SerializableAttribute()]
	[PXTable(typeof(BAccount.bAccountID))]
	[PXPrimaryGraph(typeof(CustomerMaint))]
	[PXCacheName(Messages.Customer)]
	[PXEMailSource]
	public partial class Customer : BAccount, PX.SM.IIncludable
	{
		#region BAccountID
		public new abstract class bAccountID : PX.Data.IBqlField
		{
		}
		#endregion
		#region AcctCD
		public abstract new class acctCD : PX.Data.IBqlField
		{
		}
		[CustomerRaw(IsKey = true)]
		[PXDefault]
		[PXFieldDescription]
		public override String AcctCD
		{
			get
			{
				return this._AcctCD;
			}
			set
			{
				this._AcctCD = value;
			}
		}
		#endregion
		#region Type
		public new abstract class type : PX.Data.IBqlField
		{
		}
		[PXDBString(2, IsFixed = true)]
		[PXDefault(BAccountType.CustomerType)]
		[PXUIField(DisplayName = "Type")]
		[BAccountType.List()]
		public override String Type
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

		#region CustomerClassID
		public abstract class customerClassID : PX.Data.IBqlField
		{
		}
		protected String _CustomerClassID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<ARSetup.dfltCustomerClassID>))]
		[PXSelector(typeof(CustomerClass.customerClassID), DescriptionField = typeof(CustomerClass.descr), CacheGlobal = true)]
		[PXUIField(DisplayName = "Customer Class")]
		
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
		#region LanguageID
		public abstract class languageID : PX.Data.IBqlField
		{
		}
		protected String _LanguageID;
		[PXDBString(4, IsFixed = true)]
	//	[PXUIField(DisplayName = "Language")]
		public virtual String LanguageID
		{
			get
			{
				return this._LanguageID;
			}
			set
			{
				this._LanguageID = value;
			}
		}
		#endregion
		#region DefSOAddressID
		public abstract class defSOAddressID : PX.Data.IBqlField
		{
		}
		protected Int32? _DefSOAddressID;
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Address.addressID))]
		public virtual Int32? DefSOAddressID
		{
			get
			{
				return this._DefSOAddressID;
			}
			set
			{
				this._DefSOAddressID = value;
			}
		}
		#endregion
		#region DefBillAddressID
		public abstract class defBillAddressID : PX.Data.IBqlField
		{
		}
		protected Int32? _DefBillAddressID;
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Address.addressID))]
		public virtual Int32? DefBillAddressID
		{
			get
			{
				return this._DefBillAddressID;
			}
			set
			{
				this._DefBillAddressID = value;
			}
		}
		#endregion
		#region DefBillContactID
		public abstract class defBillContactID : PX.Data.IBqlField
		{
		}
		protected Int32? _DefBillContactID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Default Contact", Visibility = PXUIVisibility.Invisible)]
        [PXDBChildIdentity(typeof(Contact.contactID))]
		public virtual Int32? DefBillContactID
		{
			get
			{
				return this._DefBillContactID;
			}
			set
			{
				this._DefBillContactID = value;
			}
		}
		#endregion
		#region BaseBillContactID
		public abstract class baseBillContactID : PX.Data.IBqlField
		{
		}
		protected Int32? _BaseBillContactID;
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		[PXSelector(typeof(Search<Contact.contactID, Where<Contact.bAccountID,
					Equal<Current<Customer.bAccountID>>,
					And<Contact.contactType, Equal<ContactTypesAttribute.person>>>>))]
       	[PXUIField(DisplayName = "Default Contact")]		
		public virtual Int32? BaseBillContactID
		{
			get
			{
				return this._BaseBillContactID;
			}
			set
			{
				this._BaseBillContactID = value;
			}
		}
		#endregion
		#region TaxZoneID
		public new abstract class taxZoneID : PX.Data.IBqlField
		{
		}
		
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone ID")]
		[PXDefault(typeof(Search<CustomerClass.taxZoneID, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>),PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<TaxZone.taxZoneID>), DescriptionField = typeof(TaxZone.descr), CacheGlobal = true)]
		public override String TaxZoneID
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
		#region TermsID
		public abstract class termsID : PX.Data.IBqlField
		{
		}
		protected String _TermsID;
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.customer>,Or<Terms.visibleTo,Equal<TermsVisibleTo.all>>>>), DescriptionField = typeof(Terms.descr), CacheGlobal = true)]
		[PXDefault(typeof(Search<CustomerClass.termsID, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Terms")]
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
		#region CuryID
		public abstract class curyID : PX.Data.IBqlField
		{
		}
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true)]
		[PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
		[PXUIField(DisplayName = "Currency ID")]
		[PXDefault(typeof(Search<CustomerClass.curyID, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		[PXDefault(typeof(Search<CustomerClass.curyRateTypeID, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Curr. Rate Type ")]
		
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
		[PXDefault(false, typeof(Search<CustomerClass.allowOverrideCury, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		//[PXDefault(typeof(Search<CMSetup.aPCuryOverride>))]
		[PXUIField(DisplayName = "Enable Currency Override")]
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
		[PXDefault(false, typeof(Search<CustomerClass.allowOverrideRate, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		//[PXDefault(typeof(Search<CMSetup.aPRateTypeOverride>))]
		[PXUIField(DisplayName = "Enable Rate Override")]
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
		#region DiscTakenAcctID
		public abstract class discTakenAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _DiscTakenAcctID;
		[PXDefault(typeof(Search<CustomerClass.discTakenAcctID,Where<CustomerClass.customerClassID,Equal<Current<Customer.customerClassID>>>>))]
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
		[PXDefault(typeof(Search<CustomerClass.discTakenSubID,Where<CustomerClass.customerClassID,Equal<Current<Customer.customerClassID>>>>))]
		[SubAccount(typeof(Customer.discTakenAcctID), DisplayName = "Cash Discount Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]

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
		#region PrepaymentAcctID
		public abstract class prepaymentAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _PrepaymentAcctID;
		[Account(DisplayName = "Prepayment Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXDefault(typeof(Search<CustomerClass.prepaymentAcctID, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? PrepaymentAcctID
		{
			get
			{
				return this._PrepaymentAcctID;
			}
			set
			{
				this._PrepaymentAcctID = value;
			}
		}
		#endregion
		#region PrepaymentSubID
		public abstract class prepaymentSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _PrepaymentSubID;
		[SubAccount(typeof(Customer.prepaymentAcctID), DisplayName = "Prepayment Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<CustomerClass.prepaymentSubID, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? PrepaymentSubID
		{
			get
			{
				return this._PrepaymentSubID;
			}
			set
			{
				this._PrepaymentSubID = value;
			}
		}
		#endregion
		/*
		#region SalesAcctID
		public abstract class salesAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _SalesAcctID;
		[PXDefault(typeof(Search<CustomerClass.salesAcctID,Where<CustomerClass.customerClassID,Equal<Current<Customer.customerClassID>>>>))]
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
		[PXDefault(typeof(Search<CustomerClass.salesSubID,Where<CustomerClass.customerClassID,Equal<Current<Customer.customerClassID>>>>))]
		[SubAccount(typeof(Customer.salesAcctID), DisplayName = "Sales Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]

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
		*/
		#region COGSAcctID
		public abstract class cOGSAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _COGSAcctID;
		[PXDefault(typeof(Search<CustomerClass.cOGSAcctID,Where<CustomerClass.customerClassID,Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		[PXDefault(typeof(Search<CustomerClass.cOGSSubID, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(Customer.cOGSAcctID), DisplayName = "COGS Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
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

		#region AutoApplyPayments
		public abstract class autoApplyPayments : PX.Data.IBqlField
		{
		}
		protected Boolean? _AutoApplyPayments;
		[PXDBBool()]
		[PXDefault(false, typeof(Search<CustomerClass.autoApplyPayments,Where<CustomerClass.customerClassID,Equal<Current<Customer.customerClassID>>>>))]
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
		[PXUIField(DisplayName = "Print Statements")]
		[PXDefault(false, typeof(Search<CustomerClass.printStatements,Where<CustomerClass.customerClassID,Equal<Current<Customer.customerClassID>>>>))]
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
		[PXUIField(DisplayName = "Multi-Currency Statements")]
		[PXDefault(false, typeof(Search<CustomerClass.printCuryStatements,Where<CustomerClass.customerClassID,Equal<Current<Customer.customerClassID>>>>))]
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
		[PXDefault(false, typeof(Search<CustomerClass.sendStatementByEmail, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
		[PXUIField(DisplayName = "Send Statements by Email")]
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
		[CreditRule()]
		[PXDefault(typeof(Search<CustomerClass.creditRule, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
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
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<CustomerClass.creditLimit, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		[PXDBShort(MinValue = 0, MaxValue = 3650)]
		[PXUIField(DisplayName = "Credit Days Past Due")]
		[PXDefault(TypeCode.Int16, "0", typeof(Search<CustomerClass.creditDaysPastDue, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		[PXDefault(typeof(Search<CustomerClass.statementType, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
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
		[PXUIField(DisplayName = "Statement Cycle ID")]
		[PXSelector(typeof(ARStatementCycle.statementCycleId))]
		[PXDefault(typeof(Search<CustomerClass.statementCycleId, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
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

		#region StatementLastDate
		public abstract class statementLastDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _StatementLastDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Statement Last Date",Enabled = false)]
		public virtual DateTime? StatementLastDate
		{
			get
			{
				return this._StatementLastDate;
			}
			set
			{
				this._StatementLastDate = value;
			}
		}
		#endregion

		#region SmallBalanceAllow
		public abstract class smallBalanceAllow : PX.Data.IBqlField
		{
		}
		protected Boolean? _SmallBalanceAllow;
		[PXDBBool()]
		[PXDefault(false, typeof(Search<CustomerClass.smallBalanceAllow, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
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
		[PXDBCury(typeof(Customer.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<CustomerClass.smallBalanceLimit, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
		[PXUIField(DisplayName = "Write-Off Limit", Enabled = false)]
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
		[PXDefault(false, typeof(Search<CustomerClass.finChargeApply, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
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
		#region PayToParent
		public abstract class payToParent : PX.Data.IBqlField
		{
		}
		protected Boolean? _PayToParent;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Bill To Parent")]
		public virtual Boolean? PayToParent
		{
			get
			{
				return this._PayToParent;
			}
			set
			{
				this._PayToParent = value;
			}
		}
		#endregion
		#region IsBillAddressSameAsMain
		public abstract class isBillSameAsMain : PX.Data.IBqlField
		{
		}
		protected bool? _IsBillSameAsMain;
		[PXBool()]
		[PXUIField(DisplayName = "Same as Main")]
		public virtual bool? IsBillSameAsMain
		{
			get
			{
				return this._IsBillSameAsMain;
			}
			set
			{
				this._IsBillSameAsMain = value;
			}
		}
		#endregion
		#region IsBillContSameAsMain
		public abstract class isBillContSameAsMain : PX.Data.IBqlField
		{
		}
		protected bool? _IsBillContSameAsMain;
		[PXBool()]
		[PXUIField(DisplayName = "Same as Main")]
		public virtual bool? IsBillContSameAsMain
		{
			get
			{
				return this._IsBillContSameAsMain;
			}
			set
			{
				this._IsBillContSameAsMain = value;
			}
		}
		#endregion
		#region DefLocationID
		public new abstract class defLocationID : PX.Data.IBqlField
		{
		}
		#endregion
		#region DefAddressID
		public new abstract class defAddressID : PX.Data.IBqlField
		{
		}
		#endregion
		#region DefContactID
		public new abstract class defContactID : PX.Data.IBqlField
		{
		}
		#endregion
		#region Status
		public new abstract class status : BAccount.status
		{
			public new class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { Active, Hold, CreditHold, Inactive, OneTime },
					new string[] { CR.Messages.Active, CR.Messages.Hold, CR.Messages.CreditHold, CR.Messages.Inactive, CR.Messages.OneTime }) { }
			}
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault(status.Active)]
		[PXUIField(DisplayName = "Status")]
		[status.List()]
		public override String Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		
		#endregion

		#region AcctName
		public new abstract class acctName : PX.Data.IBqlField
		{
		}
		[PXDBString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Customer Name", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public override String AcctName
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

        #region PaymentMethodID
        public abstract class defPaymentMethodID : PX.Data.IBqlField
        {
        }
        protected String _DefPaymentMethodID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(typeof(Search<CustomerClass.defPaymentMethodID,
                                   Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search2<PaymentMethod.paymentMethodID, LeftJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.paymentMethodID, Equal<PaymentMethod.paymentMethodID>,
									And<CustomerPaymentMethod.bAccountID, Equal<Current<Customer.bAccountID>>>>>,
                                Where<Where<PaymentMethod.isActive, Equal<boolTrue>,
                                And<PaymentMethod.useForAR, Equal<boolTrue>,                                
                                    Or<Where<CustomerPaymentMethod.pMInstanceID, IsNotNull>>>>>>), DescriptionField = typeof(PaymentMethod.descr))]
        [PXUIField(DisplayName = "Default Payment Method", Enabled = false)]
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
		#region CCProcessingID
		public abstract class cCProcessingID : PX.Data.IBqlField
		{
		}
		[PXDBString(1024, IsUnicode = true)]
		public virtual string CCProcessingID { get; set; }
		#endregion
		#region DefPMInstanceID
		public abstract class defPMInstanceID : PX.Data.IBqlField
		{
		}
		protected Int32? _DefPMInstanceID;
		[PXDefault(typeof(Search<PaymentMethod.pMInstanceID, Where<PaymentMethod.paymentMethodID, Equal<Current<Customer.defPaymentMethodID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBInt()]
		[PXDBChildIdentity(typeof(CustomerPaymentMethod.pMInstanceID))]
		public virtual Int32? DefPMInstanceID
		{
			get
			{
				return this._DefPMInstanceID;
			}
			set
			{
				this._DefPMInstanceID = value;
			}
		}
		#endregion
		#region PrintInvoices
		public abstract class printInvoices : PX.Data.IBqlField
		{
		}
		protected Boolean? _PrintInvoices;
		[PXDBBool()]
		[PXUIField(DisplayName = "Print Invoices")]
		[PXDefault(false, typeof(Search<CustomerClass.printInvoices, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
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
		[PXUIField(DisplayName = "Send Invoices by Email")]
		[PXDefault(false, typeof(Search<CustomerClass.mailInvoices, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
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
		#region NoteID
		public abstract new class noteID : PX.Data.IBqlField
		{
		}
		[PXNote(
			typeof(Customer.acctCD),
		   typeof(Customer.acctName),
			typeof(Contact.eMail),
			typeof(Contact.phone1),
			typeof(Contact.phone2),
			typeof(Contact.phone3),
			typeof(Contact.fax),
			typeof(Address.addressLine1),
			typeof(Address.addressLine2),
			typeof(Address.addressLine3),
			typeof(Address.city),
			typeof(Address.countryID),
			typeof(Address.state),
			typeof(Address.postalCode),
			ForeignRelations = new Type[] { typeof(Customer.defContactID), typeof(Customer.defAddressID) },
			ExtraSearchResultColumns = new Type[] { typeof(CR.Contact) },
			DescriptionField = typeof(Customer.acctCD),
			Selector = typeof(Customer.acctCD),
			ShowInReferenceSelector = true
			)]
		public override Int64? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion

		#region Included
		public abstract class included : PX.Data.IBqlField
		{
		}
		protected bool? _Included;
		[PXBool]
		[PXUIField]
		[PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
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
	}
}
