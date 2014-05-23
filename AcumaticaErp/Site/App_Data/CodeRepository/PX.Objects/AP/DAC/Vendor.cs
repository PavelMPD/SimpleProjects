using PX.Data.EP;

namespace PX.Objects.AP
{
	using System;
	using PX.Data;
	using PX.Objects.GL;
	using PX.Objects.CS;
	using PX.Objects.CM;
	using PX.Objects.CA;
	using PX.Objects.TX;
    using PX.Objects.CR;
	using PX.Objects.EP.Standalone;

    public static class RoundingTypes
    {
        public const string Mathematical = "R";
        public const string Ceil = "C";
        public const string Floor = "F";
    }

	[System.SerializableAttribute()]
	[PXTable(typeof(CR.BAccount.bAccountID))]

	[PXPrimaryGraph(new Type[] {
					typeof(VendorMaint),
					typeof(EP.EmployeeMaint)},
					new Type[] {
					typeof(Select<AP.Vendor, Where<AP.Vendor.bAccountID, Equal<Current<BAccount.bAccountID>>>>),
					typeof(Select<EP.EPEmployee, Where<EP.EPEmployee.bAccountID, Equal<Current<BAccount.bAccountID>>>>) 
					})]
	[PXCacheName(Messages.Vendor)]
	public partial class Vendor : CR.BAccount, PX.SM.IIncludable
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
		[VendorRaw(IsKey = true)]
		[PXDefault()]
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
		#region AcctName
		public new abstract class acctName : PX.Data.IBqlField
		{
		}
		[PXDBString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Vendor Name", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region Type
		public new abstract class type : IBqlField
		{
		}
		[PXDBString(2, IsFixed = true)]
		[PXDefault(BAccountType.VendorType)]
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
		#region VendorClassID
		public abstract class vendorClassID : PX.Data.IBqlField
		{
		}
		protected String _VendorClassID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<APSetup.dfltVendorClassID>))]
		[PXSelector(typeof(Search2<VendorClass.vendorClassID, LeftJoin<EPEmployeeClass, On<EPEmployeeClass.vendorClassID, Equal<VendorClass.vendorClassID>>>, Where<EPEmployeeClass.vendorClassID, IsNull>>), DescriptionField = typeof(VendorClass.descr), CacheGlobal = true)]
		[PXUIField(DisplayName = "Vendor Class")]
		public virtual String VendorClassID
		{
			get
			{
				return this._VendorClassID;
			}
			set
			{
				this._VendorClassID = value;
			}
		}
		#endregion
		#region TermsID
		public abstract class termsID : PX.Data.IBqlField
		{
		}
		protected String _TermsID;
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.vendor>, Or<Terms.visibleTo, Equal<TermsVisibleTo.all>>>>), DescriptionField = typeof(Terms.descr), CacheGlobal = true)]
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.termsID), PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region DefPOAddressID
		public abstract class defPOAddressID : PX.Data.IBqlField
		{
		}
		protected Int32? _DefPOAddressID;
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Address.addressID))]
		public virtual Int32? DefPOAddressID
		{
			get
			{
				return this._DefPOAddressID;
			}
			set
			{
				this._DefPOAddressID = value;
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
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.curyID), PersistingCheck = PXPersistingCheck.Nothing)]
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
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.curyRateTypeID) , PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region PriceListCuryID
		public abstract class priceListCuryID : PX.Data.IBqlField
		{
		}
		protected String _PriceListCuryID;
		[PXDBString(5, IsUnicode = true)]
		[PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.curyID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Currency ID")]
		public virtual String PriceListCuryID
		{
			get
			{
				return this._PriceListCuryID;
			}
			set
			{				
				this._PriceListCuryID = value;
			}
		}
		#endregion
		#region DefaultUOM
		public abstract class defaultUOM : PX.Data.IBqlField
		{
		}
		protected String _DefaultUOM;
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Default UOM")]
		public virtual String DefaultUOM
		{
			get
			{
				return this._DefaultUOM;
			}
			set
			{
				this._DefaultUOM = value;
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
		[PXDefault(false, typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.allowOverrideCury))]
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
		[PXDefault(false, typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.allowOverrideRate))]
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
		[Account(DisplayName = "Cash Discount Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.discTakenAcctID))]
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
		[SubAccount(typeof(Vendor.discTakenAcctID), DisplayName = "Cash Discount Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.discTakenSubID))]
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
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.prepaymentAcctID), PersistingCheck = PXPersistingCheck.Nothing)]
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
		[SubAccount(typeof(Vendor.prepaymentAcctID), DisplayName = "Prepayment Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.prepaymentSubID), PersistingCheck = PXPersistingCheck.Nothing)]
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
        #region POAccrualAcctID
        public abstract class pOAccrualAcctID : PX.Data.IBqlField
        {
        }
        protected Int32? _POAccrualAcctID;
        [Account(DisplayName = "PO Accrual Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXDefault(typeof(Search<VendorClass.pOAccrualAcctID, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? POAccrualAcctID
        {
            get
            {
                return this._POAccrualAcctID;
            }
            set
            {
                this._POAccrualAcctID = value;
            }
        }
        #endregion
        #region POAccrualSubID
        public abstract class pOAccrualSubID : PX.Data.IBqlField
        {
        }
        protected Int32? _POAccrualSubID;
        [SubAccount(typeof(Vendor.pOAccrualAcctID), DisplayName = "PO Accrual Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<VendorClass.pOAccrualSubID, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? POAccrualSubID
        {
            get
            {
                return this._POAccrualSubID;
            }
            set
            {
                this._POAccrualSubID = value;
            }
        }
        #endregion
		#region PrebookAcctID
		public abstract class prebookAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _PrebookAcctID;
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.prebookAcctID), PersistingCheck = PXPersistingCheck.Nothing)]
        [Account(DisplayName = "Reclassification Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		public virtual Int32? PrebookAcctID
		{
			get
			{
				return this._PrebookAcctID;
			}
			set
			{
				this._PrebookAcctID = value;
			}
		}
		#endregion
		#region PrebookSubID
		public abstract class prebookSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _PrebookSubID;
		
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.prebookSubID), PersistingCheck = PXPersistingCheck.Nothing)]
        [SubAccount(typeof(Vendor.prebookAcctID), DisplayName = "Reclassification Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? PrebookSubID
		{
			get
			{
				return this._PrebookSubID;
			}
			set
			{
				this._PrebookSubID = value;
			}
		}
		#endregion
		#region PayToParent
		public abstract class payToParent : PX.Data.IBqlField
		{
		}
		protected Boolean? _PayToParent;
		[PXDBBool()]
		[PXUIField(DisplayName = "Pay To Parent Account")]
		[PXDefault(false)]
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
		#region BaseRemitContactID
		public abstract class baseRemitContactID : PX.Data.IBqlField
		{
		}
		protected Int32? _BaseRemitContactID;
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		[PXUIField(DisplayName = "Default Contact", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(Search<Contact.contactID,
			Where<Contact.bAccountID,
			Equal<Current<BAccount.bAccountID>>,
			And<Contact.contactType, Equal<ContactTypesAttribute.person>>>>),
			DirtyRead = true)]
		public virtual Int32? BaseRemitContactID
		{
			get
			{
				return this._BaseRemitContactID;
			}
			set
			{
				this._BaseRemitContactID = value;
			}
		}
		#endregion
		#region TaxZoneID
		public new abstract class taxZoneID : PX.Data.IBqlField
		{
		}
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.taxZoneID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Tax Zone ID")]
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
					new string[] { Active, Hold, HoldPayments, Inactive, OneTime },
					new string[] { CR.Messages.Active, CR.Messages.Hold, CR.Messages.HoldPayments, CR.Messages.Inactive, CR.Messages.OneTime }) { }
			}
		}
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status")]
		[status.List()]
		[PXDefault(status.Active)]
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
		#region Vendor1099
		public abstract class vendor1099 : PX.Data.IBqlField
		{
		}
		protected Boolean? _Vendor1099;
		[PXDBBool()]
		[PXUIField(DisplayName = "1099 Vendor")]
		[PXDefault(false)]
		public virtual Boolean? Vendor1099
		{
			get
			{
				return this._Vendor1099;
			}
			set
			{
				this._Vendor1099 = value;
			}
		}
		#endregion
		#region Box1099
		public abstract class box1099 : PX.Data.IBqlField
		{
		}
		protected Int16? _Box1099;
		[PXDBShort()]
		[PXIntList(new int[] { 0 }, new string[] { "undefined" })]
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
		#region TaxAgency
		public abstract class taxAgency : PX.Data.IBqlField
		{
		}
		protected Boolean? _TaxAgency;
		[PXDBBool()]
		[PXUIField(DisplayName = "Vendor is Tax Agency")]
		[PXDefault(false)]
		public virtual Boolean? TaxAgency
		{
			get
			{
				return this._TaxAgency;
			}
			set
			{
				this._TaxAgency = value;
			}
		}
		#endregion
        
        #region UpdClosedTaxPeriods
        public abstract class updClosedTaxPeriods : PX.Data.IBqlField
        {
        }
        protected Boolean? _UpdClosedTaxPeriods;
        [PXDBBool()]
        [PXUIField(DisplayName = "Update closed reporting periods")]
        [PXDefault(false)]
        public virtual Boolean? UpdClosedTaxPeriods
        {
            get
            {
                return this._UpdClosedTaxPeriods;
            }
            set
            {
                this._UpdClosedTaxPeriods = value;
            }
        }
        #endregion

        #region TaxReportPrecision
        public abstract class taxReportPrecision : PX.Data.IBqlField
        {
        }
        protected Int16? _TaxReportPrecision;
        [PXDBShort(MaxValue = 9, MinValue = 0)]
        [PXDefault("2", typeof(Search<Currency.decimalPlaces, Where<Currency.curyID, Equal<Current<Vendor.curyID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Tax Report Precision")]
        public virtual Int16? TaxReportPrecision
        {
            get
            {
                return this._TaxReportPrecision;
            }
            set
            {
                this._TaxReportPrecision = value;
            }
        }
        #endregion

        #region TaxReportRounding
        public abstract class taxReportRounding : PX.Data.IBqlField
        {
        }
        protected String _TaxReportRounding;
        [PXDBString(1)]
        [PXDefault(RoundingTypes.Mathematical)]
        [PXUIField(DisplayName = "Tax Report Rounding")]
        [PXStringList(new string[] { RoundingTypes.Mathematical, RoundingTypes.Ceil, RoundingTypes.Floor }, new string[] { "Mathematical", "Ceiling", "Floor" })]
        public virtual String TaxReportRounding
        {
            get
            {
                return this._TaxReportRounding;
            }
            set
            {
                this._TaxReportRounding = value;
            }
        }
        #endregion

        #region TaxUseVendorCurPrecision
        public abstract class taxUseVendorCurPrecision : PX.Data.IBqlField
        {
        }
        protected Boolean? _TaxUseVendorCurPrecision;
        [PXDBBool()]
        [PXUIField(DisplayName = "Use Currency Precision")]
        [PXDefault(false)]
        public virtual Boolean? TaxUseVendorCurPrecision
        {
            get
            {
                return this._TaxUseVendorCurPrecision;
            }
            set
            {
                this._TaxUseVendorCurPrecision = value;
            }
        }
        #endregion

		#region TaxReportFinPeriod
		public abstract class taxReportFinPeriod : PX.Data.IBqlField
		{
		}
		protected Boolean? _TaxReportFinPeriod;
		[PXDBBool()]
		[PXUIField(DisplayName = "Use financial period end date to report tax")]
		[PXDefault(false)]
		public virtual Boolean? TaxReportFinPeriod
		{
			get
			{
				return this._TaxReportFinPeriod;
			}
			set
			{
				this._TaxReportFinPeriod = value;
			}
		}
		#endregion

		#region TaxPeriodType
		public abstract class taxPeriodType : PX.Data.IBqlField
		{
		}
		protected String _TaxPeriodType;
		[PXDBString(1)]
		[PXDefault(VendorTaxPeriodType.Monthly)]
		[PXUIField(DisplayName = "Reporting Period Type")]
		[VendorTaxPeriodType.List()]
		public virtual String TaxPeriodType
		{
			get
			{
				return this._TaxPeriodType;
			}
			set
			{
				this._TaxPeriodType = value;
			}
		}
		#endregion
		#region SalesTaxAcctID
		public abstract class salesTaxAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _SalesTaxAcctID;
		[Account(DisplayName = "Tax Payable Account", DescriptionField = typeof(Account.description))]
		public virtual Int32? SalesTaxAcctID
		{
			get
			{
				return this._SalesTaxAcctID;
			}
			set
			{
				this._SalesTaxAcctID = value;
			}
		}
		#endregion
		#region SalesTaxSubID
		public abstract class salesTaxSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _SalesTaxSubID;
		[SubAccount(typeof(Vendor.salesTaxAcctID), DescriptionField = typeof(Sub.description), DisplayName = "Tax Payable Sub.")]
		public virtual Int32? SalesTaxSubID
		{
			get
			{
				return this._SalesTaxSubID;
			}
			set
			{
				this._SalesTaxSubID = value;
			}
		}
		#endregion
		#region PurchTaxAcctID
		public abstract class purchTaxAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _PurchTaxAcctID;
		[Account(DisplayName = "Tax Claimable Account", DescriptionField = typeof(Account.description))]
		public virtual Int32? PurchTaxAcctID
		{
			get
			{
				return this._PurchTaxAcctID;
			}
			set
			{
				this._PurchTaxAcctID = value;
			}
		}
		#endregion
		#region PurchTaxSubID
		public abstract class purchTaxSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _PurchTaxSubID;
		[SubAccount(typeof(Vendor.purchTaxAcctID), DescriptionField = typeof(Sub.description), DisplayName = "Tax Claimable Sub.")]
		public virtual Int32? PurchTaxSubID
		{
			get
			{
				return this._PurchTaxSubID;
			}
			set
			{
				this._PurchTaxSubID = value;
			}
		}
		#endregion
		#region TaxExpenseAcctID
		public abstract class taxExpenseAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _TaxExpenseAcctID;
		[Account(DisplayName = "Tax Expense Account", DescriptionField = typeof(Account.description))]
		public virtual Int32? TaxExpenseAcctID
		{
			get
			{
				return this._TaxExpenseAcctID;
			}
			set
			{
				this._TaxExpenseAcctID = value;
			}
		}
		#endregion
		#region TaxExpenseSubID
		public abstract class taxExpenseSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _TaxExpenseSubID;
		[SubAccount(typeof(Vendor.taxExpenseAcctID), DescriptionField = typeof(Sub.description), DisplayName = "Tax Expense Sub.")]
		public virtual Int32? TaxExpenseSubID
		{
			get
			{
				return this._TaxExpenseSubID;
			}
			set
			{
				this._TaxExpenseSubID = value;
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
		#region OwnerID
		public new abstract class ownerID : IBqlField { }
		[PXDBGuid()]
		[PXUIField(DisplayName = "Owner", Visibility = PXUIVisibility.Invisible)]
		public override Guid? OwnerID
		{
			get
			{
				return this._OwnerID;
			}
			set
			{
				this._OwnerID = value;
			}
		}
		#endregion
		#region NoteID
		public abstract new class noteID : PX.Data.IBqlField
		{
		}
		[PXNote(
		   typeof(Vendor.acctCD),
		   typeof(Vendor.acctName),
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
			ForeignRelations = new Type[] { typeof(Vendor.defContactID), typeof(Vendor.defAddressID) },
			ExtraSearchResultColumns = new Type[] { typeof(CR.Contact) },
			DescriptionField = typeof(Vendor.acctCD),
			Selector = typeof(Vendor.acctCD),
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
		#region LandedCostVendor
		public abstract class landedCostVendor : PX.Data.IBqlField
		{
		}
		protected Boolean? _LandedCostVendor;
		[PXDBBool()]
		[PXUIField(DisplayName = "Landed Cost Vendor")]
		[PXDefault(false)]
		public virtual Boolean? LandedCostVendor
		{
			get
			{
				return this._LandedCostVendor;
			}
			set
			{
				this._LandedCostVendor = value;
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

        #region LineDiscountTarget
        public abstract class lineDiscountTarget : PX.Data.IBqlField
        {
        }
        protected String _LineDiscountTarget;
        [PXDBString(1, IsFixed = true)]
        [LineDiscountTargetType.List()]
        [PXDefault(LineDiscountTargetType.ExtendedPrice)]
        [PXUIField(DisplayName = "Apply Line Discounts to", Visibility = PXUIVisibility.Visible, Required = true)]
        public virtual String LineDiscountTarget
        {
            get
            {
                return this._LineDiscountTarget;
            }
            set
            {
                this._LineDiscountTarget = value;
            }
        }
        #endregion
        #region IgnoreConfiguredDiscounts
        public abstract class ignoreConfiguredDiscounts : PX.Data.IBqlField
        {
        }
        protected Boolean? _IgnoreConfiguredDiscounts;
        [PXDBBool()]
        [PXUIField(DisplayName = "Ignore Configured Discounts When Vendor Price is Defined")]
        [PXDefault(false)]
        public virtual Boolean? IgnoreConfiguredDiscounts
        {
            get
            {
                return this._IgnoreConfiguredDiscounts;
            }
            set
            {
                this._IgnoreConfiguredDiscounts = value;
            }
        }
        #endregion

	}

	public static class LineDiscountTargetType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { ExtendedPrice, SalesPrice },
				new string[] { Messages.ExtendedPrice, Messages.SalesPrice }) { ; }
		}
		public const string ExtendedPrice = "E";
		public const string SalesPrice = "S";
	}	
}
