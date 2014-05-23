using PX.Data.EP;
using PX.Data;
using PX.Objects.EP;
using PX.Objects.SO;
using System.Collections;

namespace PX.Objects.CT
{
	using System;
	using PX.Data;
    using PX.Objects.AR;
    using PX.Objects.CM;
    using PX.Objects.CR;
    using PX.Objects.CS;
    using PX.Objects.IN;
	using PX.TM;
	using PX.Objects.GL;
    using PX.Objects.PM;
	
	[System.SerializableAttribute()]

	[PXPrimaryGraph(new Type[] {
		typeof(TemplateMaint),
		typeof(ContractMaint)
	},
		new Type[] {
		typeof(Select<Contract, 
			Where<Contract.contractID, Equal<Current<Contract.contractID>>,
			And<Contract.baseType, Equal<Contract.ContractBaseType>,
			And<Contract.isTemplate, Equal<True>>>>>),
		typeof(Select<Contract, 
			Where<Contract.contractID, Equal<Current<Contract.contractID>>, 
			And<Contract.baseType, Equal<Contract.ContractBaseType>,
			And<Contract.isTemplate, Equal<False>>>>>)
		})]
	[PXCacheName(Messages.CTContract)]
	public partial class Contract : PX.Data.IBqlTable, IAttributeSupport
	{		
		public class ContractBaseType : Constant<string>
		{
			public const string Contract = "C";
			public ContractBaseType() : base(Contract) { ;}
		}

        #region Selected
		public abstract class selected : IBqlField
		{
		}
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
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

		#region ContractID
		public abstract class contractID : PX.Data.IBqlField
		{
		}
		protected Int32? _ContractID;
		[PXDBIdentity()]
        [PXSelector(typeof(Search<Contract.contractID, Where<Contract.baseType, Equal<ContractBaseType>>>))]
        [PXUIField(DisplayName = "Contract ID")]
		public virtual Int32? ContractID
		{
			get
			{
				return this._ContractID;
			}
			set
			{
				this._ContractID = value;
			}
		}
		#endregion
        #region ContractCD
        public abstract class contractCD : PX.Data.IBqlField
        {
        }
        protected String _ContractCD;
        [PXDimensionSelector(ContractAttribute.DimensionName,
            typeof(Search2<Contract.contractCD, InnerJoin<ContractBillingSchedule, On<Contract.contractID, Equal<ContractBillingSchedule.contractID>>, LeftJoin<Customer, On<Customer.bAccountID, Equal<Contract.customerID>>>>
			,Where<Contract.isTemplate, Equal<boolFalse>, And<Contract.baseType, Equal<Contract.ContractBaseType>>>>),
            typeof(Contract.contractCD),
			typeof(Contract.contractCD), typeof(Contract.customerID), typeof(Customer.acctName), typeof(Contract.locationID), typeof(Contract.description), typeof(Contract.status), typeof(Contract.expireDate), typeof(ContractBillingSchedule.lastDate), typeof(ContractBillingSchedule.nextDate), DescriptionField = typeof(Contract.description), Filterable = true)]
		[PXDBString(IsUnicode = true, IsKey = true, InputMask = "")]
        [PXDefault()]
        [PXUIField(DisplayName = "Contract ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
        public virtual String ContractCD
        {
            get
            {
                return this._ContractCD;
            }
            set
            {
				if (this._ContractCD != value)
					_contractInfo = null;
                this._ContractCD = value;
            }
        }
        #endregion
        #region TemplateID
        public abstract class templateID : PX.Data.IBqlField
        {
        }
        protected Int32? _TemplateID;
        [PXDefault]
        [ContractTemplate(Required = true)]
		[PXRestrictor(typeof(Where<ContractTemplate.status, Equal<ContractStatus.ContractStatusActivated>>), Messages.TemplateIsNotActivated, typeof(ContractTemplate.contractCD))]
		[PXRestrictor(typeof(Where<ContractTemplate.effectiveFrom, LessEqual<Current<AccessInfo.businessDate>>, Or<ContractTemplate.effectiveFrom, IsNull>>), Messages.TemplateIsNotStarted)]
		[PXRestrictor(typeof(Where<ContractTemplate.discontinueAfter, GreaterEqual<Current<AccessInfo.businessDate>>, Or<ContractTemplate.discontinueAfter, IsNull>>), Messages.TemplateIsExpired)]
        public virtual Int32? TemplateID
        {
            get
            {
                return this._TemplateID;
            }
            set
            {
                this._TemplateID = value;
            }
        }
        #endregion
        #region Description
        public abstract class description : PX.Data.IBqlField
        {
        }
        protected String _Description;
        [PXDBString(60, IsUnicode = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
        public virtual String Description
        {
            get
            {
                return this._Description;
            }
            set
            {
				if (this._Description != value)
					_contractInfo = null;
                this._Description = value;
            }
        }
        #endregion
		#region ContractInfo

		public abstract class contractInfo : IBqlField { }

		public String _contractInfo;
		[PXString]
		[PXUIField(Visible = false)]
		public virtual String ContractInfo
		{
			get
			{
				if (_contractInfo == null)
					_contractInfo = string.Format("{0} - {1}", ContractCD, Description);
				return _contractInfo;
			}
		}

		#endregion
		#region OriginalContractID
		public abstract class originalContractID : PX.Data.IBqlField
		{
		}
		protected Int32? _OriginalContractID;
		[Contract(DisplayName="Contract", Enabled=false)]
		public virtual Int32? OriginalContractID
		{
			get
			{
				return this._OriginalContractID;
			}
			set
			{
				this._OriginalContractID = value;
			}
		}
		#endregion
		#region MasterContractID
		public abstract class masterContractID : PX.Data.IBqlField
		{
		}
		protected Int32? _MasterContractID;
		[PXDBInt()]
        [PXSelector(typeof(Contract.contractID))]
        [PXUIField(DisplayName = "Master Contract")]
		public virtual Int32? MasterContractID
		{
			get
			{
				return this._MasterContractID;
			}
			set
			{
				this._MasterContractID = value;
			}
		}
		#endregion
        #region CaseItemID
        public abstract class caseItemID : PX.Data.IBqlField
        {
        }
        protected Int32? _CaseItemID;
        [PXDBInt()]
        [PXUIField(DisplayName = "Case Count Item")]
        [PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.stkItem, Equal<False>, And<Match<Current<AccessInfo.userName>>>>>), typeof(InventoryItem.inventoryCD))]
        public virtual Int32? CaseItemID
        {
            get
            {
                return this._CaseItemID;
            }
            set
            {
                this._CaseItemID = value;
            }
        }
        #endregion
        #region BaseType
		public abstract class baseType : PX.Data.IBqlField
		{
		}
		protected String _BaseType;
		[PXUIField(DisplayName = "Entity Type", Enabled=false)]
		[PXDBString(1, IsFixed = true)]
		[PXStringList(new string[] { ContractBaseType.Contract, PM.PMProject.ProjectBaseType.Project }, new string[] { Messages.Contract, PM.Messages.Project })]
		[PXDefault(ContractBaseType.Contract)]
		public virtual String BaseType
		{
			get
			{
				return this._BaseType;
			}
			set
			{
				this._BaseType = value;
			}
		}
		#endregion
        #region Type
		public abstract class type : PX.Data.IBqlField
		{
		}
		protected String _Type;
		[PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Contract Type")]
        [ContractType.List()]
        [PXDefault(ContractType.Renewable)]
		public virtual String Type
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
		#region ClassType
		public abstract class classType : PX.Data.IBqlField
		{
		}
		protected String _ClassType;
		[PXDBString(1, IsFixed = true)]
		public virtual String ClassType
		{
			get
			{
				return this._ClassType;
			}
			set
			{
				this._ClassType = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.IBqlField
		{
		}
		protected Int32? _CustomerID;
        //[PXDefault]
        [Customer(DescriptionField = typeof(Customer.acctName), Visibility=PXUIVisibility.SelectorVisible)]
		public virtual Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.IBqlField
		{
		}
		protected Int32? _LocationID;
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<Contract.customerID>>>), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location", DescriptionField = typeof(Location.descr))]
		[PXDefault(typeof(Search<BAccount.defLocationID, Where<BAccount.bAccountID, Equal<Current<Contract.customerID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
        #region RateTableID
        public abstract class rateTableID : PX.Data.IBqlField
        {
        }
        protected String _RateTableID;
        [PXDBString(PMRateTable.rateTableID.Length, IsUnicode = true)]
        public virtual String RateTableID
        {
            get
            {
                return this._RateTableID;
            }
            set
            {
                this._RateTableID = value;
            }
        }
        #endregion
        #region Balance
        public abstract class balance : IBqlField { }
        protected decimal? _Balance;
        [PXBaseCury]
        [PXUIField(DisplayName = "Balance", Enabled = false)]
        public decimal? Balance
        {
            get
            {
                return this._Balance;
            }
            set
            {
                this._Balance = value;
            }
        }
        #endregion
		#region Status
		public abstract class status : PX.Data.IBqlField
		{
		}
		protected String _Status;
		[PXDBString(1, IsFixed = true)]
        [ContractStatus.List()]
        [PXDefault(ContractStatus.Draft)]
        [PXUIField(DisplayName = "Status", Required=true, Visibility=PXUIVisibility.SelectorVisible, Enabled=false)]
		public virtual String Status
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
		#region Duration
		public abstract class duration : PX.Data.IBqlField
		{
		}
		protected Int32? _Duration;
		[PXDBInt(MinValue = 1, MaxValue = 1000)]
		[PXUIField(DisplayName = "Duration")]
		[PXDefault(1)]
		public virtual Int32? Duration
		{
			get
			{
				return this._Duration;
			}
			set
			{
				this._Duration = value;
			}
		}
		#endregion
		#region DurationType
		public abstract class durationType : PX.Data.IBqlField
		{
		}
		protected string _DurationType;

		[PXDBString(1, IsFixed = true)]
		[DurationType.List()]
		[PXDefault(CT.DurationType.Annual)]
		[PXUIField(DisplayName = "Duration Unit")]
		public virtual string DurationType
		{
			get
			{
				return this._DurationType;
			}
			set
			{
				this._DurationType = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _StartDate;
        [PXDefault]
		[PXDBDate()]
        [PXUIField(DisplayName = "Setup Date", Required=true)]
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
		#region ActivationDate
		public abstract class activationDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _ActivationDate;
		[PXDefault(PersistingCheck=PXPersistingCheck.Nothing)]
		[PXDBDate()]
		[PXFormula(typeof(Switch<
				Case<Where<Contract.startDate, Greater<Current<Contract.activationDate>>>, Current<Contract.startDate>>,
				Current<Contract.activationDate>>))]
		[PXUIField(DisplayName = "Activation Date")]
		public virtual DateTime? ActivationDate
		{
			get
			{
				return this._ActivationDate;
			}
			set
			{
				this._ActivationDate = value;
			}
		}
		#endregion
		#region ExpireDate
		public abstract class expireDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _ExpireDate;
        [PXDBDate()]
		[PXUIField(DisplayName = "Expiration Date", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? ExpireDate
		{
			get
			{
				return this._ExpireDate;
			}
			set
			{
				this._ExpireDate = value;
			}
		}
		#endregion
        #region TerminationDate
        public abstract class terminationDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _TerminationDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Termination Date", Visibility = PXUIVisibility.SelectorVisible, Enabled=false)]
        public virtual DateTime? TerminationDate
        {
            get
            {
                return this._TerminationDate;
            }
            set
            {
                this._TerminationDate = value;
            }
        }
        #endregion

		#region GracePeriod
		public abstract class gracePeriod : PX.Data.IBqlField
		{
		}
		protected Int32? _GracePeriod;

        /// <summary>
        /// Period in days Contract is serviced even after it has expired. Warning is shown whenever user
        /// selects the contract that falls in this period.
        /// </summary>
		[PXDBInt(MinValue=0, MaxValue=365)]
		[PXDefault(0)]
        [PXUIField(DisplayName = "Grace Period")]
		public virtual Int32? GracePeriod
		{
			get
			{
				return this._GracePeriod;
			}
			set
			{
				this._GracePeriod = value;
			}
		}
		#endregion
		#region GraceDate
		public abstract class graceDate : IBqlField {}

		/// <summary>
		/// End Date of Grace Period.
		/// </summary>
		[PXDBCalced(typeof(Add<Contract.expireDate, Contract.gracePeriod>), typeof(DateTime))]
		public virtual DateTime? GraceDate { get; set; }
		#endregion
		#region AutoRenew
		public abstract class autoRenew : PX.Data.IBqlField
		{
		}
		protected Boolean? _AutoRenew;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Renew Automatically")]
		public virtual Boolean? AutoRenew
		{
			get
			{
				return this._AutoRenew;
			}
			set
			{
				this._AutoRenew = value;
			}
		}
		#endregion
		#region AutoRenewDays
		public abstract class autoRenewDays : PX.Data.IBqlField
		{
		}
		protected Int32? _AutoRenewDays;
        [PXDBInt(MinValue = 0, MaxValue = 365)]
		[PXDefault(0)]
        [PXUIField(DisplayName = "Auto Renew Point")]
		public virtual Int32? AutoRenewDays
		{
			get
			{
				return this._AutoRenewDays;
			}
			set
			{
				this._AutoRenewDays = value;
			}
		}
		#endregion
		#region IsTemplate
        public abstract class isTemplate : PX.Data.IBqlField
        {
        }
        protected Boolean? _IsTemplate;
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? IsTemplate
        {
            get
            {
                return this._IsTemplate;
            }
            set
            {
                this._IsTemplate = value;
            }
        }
        #endregion
        #region CuryID
        public abstract class curyID : PX.Data.IBqlField
        {
        }
        protected String _CuryID;
        [PXDefault]
        [PXDBString(5, IsUnicode = true)]
        [PXSelector(typeof(Currency.curyID))]
        [PXUIField(DisplayName = "Currency")]
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
        #region RateTypeID
        public abstract class rateTypeID : PX.Data.IBqlField
        {
        }
        protected String _RateTypeID;
        [PXDBString(6, IsUnicode = true)]
        [PXSelector(typeof(PX.Objects.CM.CurrencyRateType.curyRateTypeID))]
        [PXUIField(DisplayName = "Rate Type", Required=true)]
        [PXDefault(typeof(Search<CMSetup.aRRateTypeDflt>), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual String RateTypeID
        {
            get
            {
                return this._RateTypeID;
            }
            set
            {
                this._RateTypeID = value;
            }
        }
        #endregion
        #region AllowOverrideCury
        public abstract class allowOverrideCury : PX.Data.IBqlField
        {
        }
        protected Boolean? _AllowOverrideCury;
        [PXDBBool()]
        [PXDefault(false)]
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
        [PXDefault(false)]
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
        #region CalendarID
        public abstract class calendarID : PX.Data.IBqlField
        {
        }
        protected String _CalendarID;
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Calendar")]
        [PXSelector(typeof(Search<CSCalendar.calendarID>), DescriptionField = typeof(CSCalendar.description))]
        public virtual String CalendarID
        {
            get
            {
                return this._CalendarID;
            }
            set
            {
                this._CalendarID = value;
            }
        }
        #endregion
		#region AutomaticReleaseAR
		public abstract class automaticReleaseAR : PX.Data.IBqlField
		{
		}
		protected Boolean? _AutomaticReleaseAR;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Automatically Release AR Documents", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? AutomaticReleaseAR
		{
			get
			{
				return this._AutomaticReleaseAR;
			}
			set
			{
				this._AutomaticReleaseAR = value;
			}
		}
		#endregion
        #region Refundable
        public abstract class refundable : PX.Data.IBqlField
        {
        }
        protected Boolean? _Refundable;
        [PXDBBool()]
        [PXDefault(false, PersistingCheck=PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Refundable")]
        public virtual Boolean? Refundable
        {
            get
            {
                return this._Refundable;
            }
            set
            {
                this._Refundable = value;
            }
        }
        #endregion
        #region RefundPeriod
        public abstract class refundPeriod : PX.Data.IBqlField
        {
        }
        protected Int32? _RefundPeriod;
        [PXDBInt(MinValue = 0, MaxValue = 365)]
        [PXDefault(0,PersistingCheck=PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Refund Period")]
        public virtual Int32? RefundPeriod
        {
            get
            {
                return this._RefundPeriod;
            }
            set
            {
                this._RefundPeriod = value;
            }
        }
        #endregion
        #region EffectiveFrom
        public abstract class effectiveFrom : PX.Data.IBqlField
        {
        }
        protected DateTime? _EffectiveFrom;
        [PXDBDate()]
        [PXUIField(DisplayName = "Effective From")]
        public virtual DateTime? EffectiveFrom
        {
            get
            {
                return this._EffectiveFrom;
            }
            set
            {
                this._EffectiveFrom = value;
            }
        }
        #endregion
        #region DiscontinueAfter
        public abstract class discontinueAfter : PX.Data.IBqlField
        {
        }
        protected DateTime? _DiscontinueAfter;
        [PXDBDate()]
        [PXUIField(DisplayName = "Discontinue After")]
        public virtual DateTime? DiscontinueAfter
        {
            get
            {
                return this._DiscontinueAfter;
            }
            set
            {
                this._DiscontinueAfter = value;
            }
        }
        #endregion
        #region DiscountID
        public abstract class discountID : PX.Data.IBqlField
        {
        }
        protected String _DiscountID;
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Promo Code")]
		[PromoDiscIDSelector(typeof(Search<ARDiscount.discountID, Where<ARDiscount.type, Equal<DiscountType.LineDiscount>>>))]
		public virtual String DiscountID
        {
            get
            {
                return this._DiscountID;
            }
            set
            {
                this._DiscountID = value;
            }
        }
		public class PromoDiscIDSelectorAttribute : PXCustomSelectorAttribute
		{
			protected BqlCommand _select;
			public PromoDiscIDSelectorAttribute(Type type)
				: base(type)
			{
				this._ViewName = "_SODiscount_LinePromo_";
			}

			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);

				_select = BqlCommand.CreateInstance(typeof(Select5<ARDiscount,
					InnerJoin<DiscountSequence, On<ARDiscount.discountID, Equal<DiscountSequence.discountID>>,
					LeftJoin<DiscountCustomer, On<DiscountCustomer.discountID, Equal<DiscountSequence.discountID>, And<DiscountCustomer.discountSequenceID, Equal<DiscountSequence.discountSequenceID>>>,
					LeftJoin<DiscountItem, On<DiscountItem.discountID, Equal<DiscountSequence.discountID>, And<DiscountItem.discountSequenceID, Equal<DiscountSequence.discountSequenceID>>>,
					LeftJoin<DiscountCustomerPriceClass, On<DiscountCustomerPriceClass.discountID, Equal<DiscountSequence.discountID>, And<DiscountCustomerPriceClass.discountSequenceID, Equal<DiscountSequence.discountSequenceID>>>,
					LeftJoin<DiscountInventoryPriceClass, On<DiscountInventoryPriceClass.discountID, Equal<DiscountSequence.discountID>, And<DiscountInventoryPriceClass.discountSequenceID, Equal<DiscountSequence.discountSequenceID>>>>>>>>,
					Where2<
						Where<ARDiscount.applicableTo, NotEqual<ARDiscount.applicableTo.customer>, And<ARDiscount.applicableTo, NotEqual<ARDiscount.applicableTo.customerAndInventory>, And<ARDiscount.applicableTo, NotEqual<ARDiscount.applicableTo.customerAndInventoryPrice>, Or<DiscountCustomer.customerID, Equal<Current<Contract.customerID>>>>>>,
					//And2<Where<SODiscount.applicableTo, NotEqual<SODiscount.applicableTo.inventory>, And<SODiscount.applicableTo, NotEqual<SODiscount.applicableTo.customerAndInventory>, And<SODiscount.applicableTo, NotEqual<SODiscount.applicableTo.customerPriceAndInventory>, Or<SODiscountItem.inventoryID, Equal<Current<SOLine.inventoryID>>>>>>,
					//And2<Where<SODiscount.applicableTo, NotEqual<SODiscount.applicableTo.customerAndInventoryPrice>, And<SODiscount.applicableTo, NotEqual<SODiscount.applicableTo.customerPriceAndInventoryPrice>, And<SODiscount.applicableTo, NotEqual<SODiscount.applicableTo.inventoryPrice>, Or<SODiscountInventoryPriceClass.inventoryPriceClassID, Equal<Current<InventoryItem.priceClassID>>>>>>,
						And2<Where<ARDiscount.applicableTo, NotEqual<ARDiscount.applicableTo.customerPrice>, And<ARDiscount.applicableTo, NotEqual<ARDiscount.applicableTo.customerPriceAndInventory>, And<ARDiscount.applicableTo, NotEqual<ARDiscount.applicableTo.customerPriceAndInventoryPrice>, Or<DiscountCustomerPriceClass.customerPriceClassID, Equal<Current<CR.Location.cPriceClassID>>>>>>,
						And<DiscountSequence.isActive, Equal<True>,
						And<ARDiscount.type, Equal<DiscountType.LineDiscount>,
						And<Where<DiscountSequence.isPromotion, Equal<False>, Or<Current<Contract.startDate>, Between<DiscountSequence.startDate, DiscountSequence.endDate>>>>>>>>,
					Aggregate<GroupBy<ARDiscount.discountID>>>));
			}

			public virtual IEnumerable GetRecords()
			{
				Location item = PXSelect<Location, Where<Location.bAccountID, Equal<Current<Contract.customerID>>, And<Location.locationID, Equal<Current<Contract.locationID>>>>>.Select(_Graph);

				PXView view = _Graph.TypedViews.GetView(_select, true);
				return view.SelectMultiBound(new object[] { item });
			}

			public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
			{
				if (!string.IsNullOrEmpty((string)e.NewValue))
				{
					if (PXSelect<ARDiscount, Where<ARDiscount.discountID, Equal<Required<ARDiscount.discountID>>, And<ARDiscount.type, Equal<DiscountType.LineDiscount>>>>.Select(sender.Graph, e.NewValue).Count == 0)
					{
						throw new PXSetPropertyException(ErrorMessages.ElementDoesntExist, e.NewValue);
					}
				}
			}
		}
        #endregion
        #region DetailedBilling
        public abstract class detailedBilling : PX.Data.IBqlField
        {
			public const int Summary = 0;
			public const int Detail = 1; 
        }
        protected Int32? _DetailedBilling;
        [PXDBInt()]
		[PXDefault(detailedBilling.Summary)]
		[PXIntList(new int[] {detailedBilling.Summary, detailedBilling.Detail}, new string[] { Messages.Summary, Messages.Detail })]
		[PXUIField(DisplayName = "Billing Format")]
        public virtual Int32? DetailedBilling
        {
            get
            {
                return this._DetailedBilling;
            }
            set
            {
                this._DetailedBilling = value;
            }
        }
        #endregion
		#region AllowOverride
		public abstract class allowOverride : PX.Data.IBqlField
		{
		}
		protected Boolean? _AllowOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Enable Template Items Override")]
		public virtual Boolean? AllowOverride
		{
			get
			{
				return this._AllowOverride;
			}
			set
			{
				this._AllowOverride = value;
			}
		}
		#endregion
		#region RefreshOnRenewal
		public abstract class refreshOnRenewal : PX.Data.IBqlField
		{
		}
		protected Boolean? _RefreshOnRenewal;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Refresh Items from Template on Renewal")]
		public virtual Boolean? RefreshOnRenewal
		{
			get
			{
				return this._RefreshOnRenewal;
			}
			set
			{
				this._RefreshOnRenewal = value;
			}
		}
		#endregion
		#region IsContinuous
		public abstract class isContinuous : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsContinuous;
		[PXDBBool()]
		[PXUIField(DisplayName = "Shift Expire Date on Renew", Visibility = PXUIVisibility.Invisible)]
		[PXDefault(true)]
		public virtual Boolean? IsContinuous
		{
			get
			{
				return this._IsContinuous;
			}
			set
			{
				this._IsContinuous = value;
			}
		}
		#endregion
		#region DefaultAccountID
		public abstract class defaultAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _DefaultAccountID;
		[Account(DisplayName = "Default Account")]
		public virtual Int32? DefaultAccountID
		{
			get
			{
				return this._DefaultAccountID;
			}
			set
			{
				this._DefaultAccountID = value;
			}
		}
		#endregion
		#region DefaultSubID
		public abstract class defaultSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _DefaultSubID;
		[SubAccount(DisplayName = "Default Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? DefaultSubID
		{
			get
			{
				return this._DefaultSubID;
			}
			set
			{
				this._DefaultSubID = value;
			}
		}
		#endregion
        #region DefaultAccrualAccountID
        public abstract class defaultAccrualAccountID : PX.Data.IBqlField
        {
        }
        protected Int32? _DefaultAccrualAccountID;
        [Account(DisplayName = "Default Accrual Account")]
        public virtual Int32? DefaultAccrualAccountID
        {
            get
            {
                return this._DefaultAccrualAccountID;
            }
            set
            {
                this._DefaultAccrualAccountID = value;
            }
        }
        #endregion
        #region DefaultSubID
        public abstract class defaultAccrualSubID : PX.Data.IBqlField
        {
        }
        protected Int32? _DefaultAccrualSubID;
        [SubAccount(DisplayName = "Default Accrual Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
        public virtual Int32? DefaultAccrualSubID
        {
            get
            {
                return this._DefaultAccrualSubID;
            }
            set
            {
                this._DefaultAccrualSubID = value;
            }
        }
        #endregion
		#region RestrictToEmployeeList
		public abstract class restrictToEmployeeList : PX.Data.IBqlField
		{
		}
		protected Boolean? _RestrictToEmployeeList;
		[PXDBBool()]
		[PXDefault(false)]
        [PXUIField(DisplayName = "Restrict Employees")]
		public virtual Boolean? RestrictToEmployeeList
		{
			get
			{
				return this._RestrictToEmployeeList;
			}
			set
			{
				this._RestrictToEmployeeList = value;
			}
		}
		#endregion
		#region RestrictToResourceList
		public abstract class restrictToResourceList : PX.Data.IBqlField
		{
		}
		protected Boolean? _RestrictToResourceList;
		[PXDBBool()]
		[PXDefault(false)]
        [PXUIField(DisplayName = "Restrict Equipment")]
		public virtual Boolean? RestrictToResourceList
		{
			get
			{
				return this._RestrictToResourceList;
			}
			set
			{
				this._RestrictToResourceList = value;
			}
		}
		#endregion
		#region ApproverID
		public abstract class approverID : PX.Data.IBqlField
		{
		}
		protected Int32? _ApproverID;
		[PXDBInt]
		public virtual Int32? ApproverID
		{
			get
			{
				return this._ApproverID;
			}
			set
			{
				this._ApproverID = value;
			}
		}
		#endregion
        #region WorkgroupID
        public abstract class workgroupID : IBqlField { }
        protected int? _WorkgroupID;
        [PXInt]
        [PXFormula(typeof(Selector<Contract.customerID, BAccount.workgroupID>))]
        public virtual int? WorkgroupID
        {
            get
            {
                return this._WorkgroupID;
            }
            set
            {
                this._WorkgroupID = value;
            }
        }
        #endregion
        #region OwnerID
        public abstract class ownerID : IBqlField { }
        protected Guid? _OwnerID;
        [PXDBGuid()]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXOwnerSelector(typeof(Contract.workgroupID))]
        [PXUIField(DisplayName = "Owner", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Guid? OwnerID
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
        #region SalesPersonID
        public abstract class salesPersonID : PX.Data.IBqlField
        {
        }
        protected Int32? _SalesPersonID;
        [PXDefault(PersistingCheck=PXPersistingCheck.Nothing)]
        [SalesPerson(DescriptionField = typeof(SalesPerson.descr), DisplayName = "Sales Person")]
        public virtual Int32? SalesPersonID
        {
            get
            {
                return this._SalesPersonID;
            }
            set
            {
                this._SalesPersonID = value;
            }
        }
        #endregion
		#region ScheduleStartsOn
		public abstract class scheduleStartsOn : PX.Data.IBqlField
		{
		}
		protected String _ScheduleStartsOn;
		[PXDefault(ScheduleStartOption.ActivationDate, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(1, IsFixed = true)]
		[ScheduleStartOption.List()]
		[PXUIField(DisplayName = "Billing Schedule Starts On")]
		public virtual String ScheduleStartsOn
		{
			get
			{
				return this._ScheduleStartsOn;
			}
			set
			{
				this._ScheduleStartsOn = value;
			}
		}
		#endregion

		#region BillingID
		public abstract class billingID : PX.Data.IBqlField
		{
		}
		protected String _BillingID;
        [PXDBString(15, IsUnicode = true)]
		public virtual String BillingID
		{
			get
			{
				return this._BillingID;
			}
			set
			{
				this._BillingID = value;
			}
		}
		#endregion
		#region AllocationID
		public abstract class allocationID : PX.Data.IBqlField
		{
		}
		protected String _AllocationID;
		[PXDBString(15, IsUnicode = true)]
		public virtual String AllocationID
		{
			get
			{
				return this._AllocationID;
			}
			set
			{
				this._AllocationID = value;
			}
		}
		#endregion
        #region ContractAccountGroup
        public abstract class contractAccountGroup : PX.Data.IBqlField
        {
        }
        protected int? _ContractAccountGroup;
        [PXSelector(typeof(Search<PMAccountGroup.groupID, Where<PMAccountGroup.type, Equal<PMAccountType.offBalance>>>), SubstituteKey=typeof(PMAccountGroup.groupCD))]
        [PXUIField(DisplayName = "Account Group")]
        [PXDBInt]
        public virtual int? ContractAccountGroup
        {
            get
            {
                return this._ContractAccountGroup;
            }
            set
            {
                this._ContractAccountGroup = value;
            }
        }
        #endregion

        #region PendingSetup
        public abstract class pendingSetup : PX.Data.IBqlField
        {
        }
        protected Decimal? _PendingSetup;
        [PXDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName="Pending Setup", Enabled=false)]
        public virtual Decimal? PendingSetup
        {
            get
            {
                return this._PendingSetup;
            }
            set
            {
                this._PendingSetup = value;
            }
        }
        #endregion
        #region PendingRecurring
        public abstract class pendingRecurring : PX.Data.IBqlField
        {
        }
        protected Decimal? _PendingRecurring;
        [PXDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Pending Recurring",Enabled=false)]
        public virtual Decimal? PendingRecurring
        {
            get
            {
                return this._PendingRecurring;
            }
            set
            {
                this._PendingRecurring = value;
            }
        }
        #endregion
        #region PendingRenewal
        public abstract class pendingRenewal : PX.Data.IBqlField
        {
        }
        protected Decimal? _PendingRenewal;
        [PXDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Pending Renewal", Enabled=false)]
        public virtual Decimal? PendingRenewal
        {
            get
            {
                return this._PendingRenewal;
            }
            set
            {
                this._PendingRenewal = value;
            }
        }
        #endregion
        #region TotalPending
        public abstract class totalPending : PX.Data.IBqlField
        {
        }
        protected Decimal? _TotalPending;
        [PXDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Add<Contract.pendingRecurring, Add<Contract.pendingRenewal, Contract.pendingSetup>>))]
        [PXUIField(DisplayName = "Total Pending", Enabled=false)]
        public virtual Decimal? TotalPending
        {
            get
            {
                return this._TotalPending;
            }
            set
            {
                this._TotalPending = value;
            }
        }
        #endregion

        #region CurrentSetup
        public abstract class currentSetup : PX.Data.IBqlField
        {
        }
        protected Decimal? _CurrentSetup;
        [PXDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Current Setup",Enabled=false)]
        public virtual Decimal? CurrentSetup
        {
            get
            {
                return this._CurrentSetup;
            }
            set
            {
                this._CurrentSetup = value;
            }
        }
        #endregion
        #region CurrentRecurring
        public abstract class currentRecurring : PX.Data.IBqlField
        {
        }
        protected Decimal? _CurrentRecurring;
        [PXDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Current Recurring", Enabled=false)]
        public virtual Decimal? CurrentRecurring
        {
            get
            {
                return this._CurrentRecurring;
            }
            set
            {
                this._CurrentRecurring = value;
            }
        }
        #endregion
        #region CurrentRenewal
        public abstract class currentRenewal : PX.Data.IBqlField
        {
        }
        protected Decimal? _CurrentRenewal;
        [PXDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Current Renewal",Enabled=false)]
        public virtual Decimal? CurrentRenewal
        {
            get
            {
                return this._CurrentRenewal;
            }
            set
            {
                this._CurrentRenewal = value;
            }
        }
        #endregion

		#region TotalsCalculated
		public abstract class totalsCalculated : PX.Data.IBqlField
		{
		}
		[PXInt()]
		public virtual int? TotalsCalculated
		{
			get;
			set;
		}
		#endregion
		#region TotalRecurring
		public abstract class totalRecurring : PX.Data.IBqlField
        {
        }
        protected Decimal? _TotalRecurring;
        [PXDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Recurring Total", Enabled = false)]
        public virtual Decimal? TotalRecurring
        {
            get
            {
                return this._TotalRecurring;
            }
            set
            {
                this._TotalRecurring = value;
            }
        }
        #endregion
        #region TotalUsage
        public abstract class totalUsage : PX.Data.IBqlField
        {
        }
        protected Decimal? _TotalUsage;
        [PXDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Extra Usage Total", Enabled = false)]
        public virtual Decimal? TotalUsage
        {
            get
            {
                return this._TotalUsage;
            }
            set
            {
                this._TotalUsage = value;
            }
        }
        #endregion
        #region TotalDue
        public abstract class totalDue : PX.Data.IBqlField
        {
        }
        protected Decimal? _TotalDue;
        [PXDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Total Due", Enabled = false)]
        public virtual Decimal? TotalDue
        {
            get
            {
                return this._TotalDue;
            }
            set
            {
                this._TotalDue = value;
            }
        }
        #endregion

		#region Hold
		public abstract class hold : PX.Data.IBqlField
		{
		}
		protected Boolean? _Hold;
		[PXDBBool()]
		[PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.Visible)]
		[PXDefault(true)]
		[PXNoUpdate]
		public virtual Boolean? Hold
		{
			get
			{
				return this._Hold;
			}
			set
			{
				this._Hold = value;
			}
		}
		#endregion
		#region Approved
		public abstract class approved : PX.Data.IBqlField
		{
		}
		protected Boolean? _Approved;
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Approved", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Boolean? Approved
		{
			get
			{
				return this._Approved;
			}
			set
			{
				this._Approved = value;
			}
		}
		#endregion
		#region Rejected
		public abstract class rejected : IBqlField
		{
		}
		protected bool? _Rejected = false;
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Reject", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public bool? Rejected
		{
			get
			{
				return _Rejected;
			}
			set
			{
				_Rejected = value;
			}
		}
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsActive;
		[PXDBBool()]
		[PXDefault(false)]
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
		#region IsCompleted
		public abstract class isCompleted : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsCompleted;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsCompleted
		{
			get
			{
				return this._IsCompleted;
			}
			set
			{
				this._IsCompleted = value;
			}
		}
		#endregion
        #region IsCancelled
        public abstract class isCancelled : PX.Data.IBqlField
        {
        }
        protected Boolean? _IsCancelled;
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? IsCancelled
        {
            get
            {
                return this._IsCancelled;
            }
            set
            {
                this._IsCancelled = value;
            }
        }
        #endregion
        #region IsPendingUpdate
        public abstract class isPendingUpdate : PX.Data.IBqlField
        {
        }
        protected Boolean? _IsPendingUpdate;
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? IsPendingUpdate
        {
            get
            {
                return this._IsPendingUpdate;
            }
            set
            {
                this._IsPendingUpdate = value;
            }
        }
        #endregion
        #region AutoAllocate
        public abstract class autoAllocate : PX.Data.IBqlField
        {
        }
        protected Boolean? _AutoAllocate;
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? AutoAllocate
        {
            get
            {
                return this._AutoAllocate;
            }
            set
            {
                this._AutoAllocate = value;
            }
        }
        #endregion
        #region IsLastActionUndoable
        public abstract class isLastActionUndoable : PX.Data.IBqlField
        {
        }
        protected Boolean? _IsLastActionUndoable;
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? IsLastActionUndoable
        {
            get
            {
                return this._IsLastActionUndoable;
            }
            set
            {
                this._IsLastActionUndoable = value;
            }
        }
        #endregion

        #region VisibleInGL
        public abstract class visibleInGL : PX.Data.IBqlField
		{
		}
		protected Boolean? _VisibleInGL;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? VisibleInGL
		{
			get
			{
				return this._VisibleInGL;
			}
			set
			{
				this._VisibleInGL = value;
			}
		}
		#endregion
		#region VisibleInAP
		public abstract class visibleInAP : PX.Data.IBqlField
		{
		}
		protected Boolean? _VisibleInAP;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? VisibleInAP
		{
			get
			{
				return this._VisibleInAP;
			}
			set
			{
				this._VisibleInAP = value;
			}
		}
		#endregion
		#region VisibleInAR
		public abstract class visibleInAR : PX.Data.IBqlField
		{
		}
		protected Boolean? _VisibleInAR;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? VisibleInAR
		{
			get
			{
				return this._VisibleInAR;
			}
			set
			{
				this._VisibleInAR = value;
			}
		}
		#endregion
		#region VisibleInSO
		public abstract class visibleInSO : PX.Data.IBqlField
		{
		}
		protected Boolean? _VisibleInSO;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? VisibleInSO
		{
			get
			{
				return this._VisibleInSO;
			}
			set
			{
				this._VisibleInSO = value;
			}
		}
		#endregion
		#region VisibleInPO
		public abstract class visibleInPO : PX.Data.IBqlField
		{
		}
		protected Boolean? _VisibleInPO;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? VisibleInPO
		{
			get
			{
				return this._VisibleInPO;
			}
			set
			{
				this._VisibleInPO = value;
			}
		}
		#endregion
		#region VisibleInEP
		public abstract class visibleInEP : PX.Data.IBqlField
		{
		}
		protected Boolean? _VisibleInEP;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? VisibleInEP
		{
			get
			{
				return this._VisibleInEP;
			}
			set
			{
				this._VisibleInEP = value;
			}
		}
		#endregion
		#region VisibleInIN
		public abstract class visibleInIN : PX.Data.IBqlField
		{
		}
		protected Boolean? _VisibleInIN;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? VisibleInIN
		{
			get
			{
				return this._VisibleInIN;
			}
			set
			{
				this._VisibleInIN = value;
			}
		}
		#endregion
		#region VisibleInCA
		public abstract class visibleInCA : PX.Data.IBqlField
		{
		}
		protected Boolean? _VisibleInCA;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "CA")]
		public virtual Boolean? VisibleInCA
		{
			get
			{
				return this._VisibleInCA;
			}
			set
			{
				this._VisibleInCA = value;
			}
		}
		#endregion
		#region VisibleInCR
		public abstract class visibleInCR : PX.Data.IBqlField
		{
		}
		protected Boolean? _VisibleInCR;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "CR")]
		public virtual Boolean? VisibleInCR
		{
			get
			{
				return this._VisibleInCR;
			}
			set
			{
				this._VisibleInCR = value;
			}
		}
		#endregion
		#region NonProject
		public abstract class nonProject : PX.Data.IBqlField
		{
		}
		protected Boolean? _NonProject;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? NonProject
		{
			get
			{
				return this._NonProject;
			}
			set
			{
				this._NonProject = value;
			}
		}
		#endregion
		#region RevID
		public abstract class revID : PX.Data.IBqlField
		{
		}
		[PXDBInt(MinValue = 1)]
		[PXDefault(1, PersistingCheck = PXPersistingCheck.Null)]
		public virtual int? RevID { get; set; }
		#endregion
		#region LastActiveRevID
		public abstract class lastActiveRevID : PX.Data.IBqlField
		{
		}
		[PXDBInt(MinValue = 1)]
		public virtual int? LastActiveRevID { get; set; }
		#endregion
		#region LineCtr
		public abstract class lineCtr : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
		[PXDefault(0)]
		public virtual int? LineCtr { get; set; }
		#endregion
		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.IBqlField
		{
		}
		protected Int64? _NoteID;
		[PXNote(new Type[0], DescriptionField = typeof(Contract.contractCD))]
		public virtual Int64? NoteID
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
        #endregion

        #region Labels
        #region DaysBeforeExpiration
        public abstract class daysBeforeExpiration : PX.Data.IBqlField
        {
        }
        protected String _DaysBeforeExpiration;
        [PXString]
        [PXUIField]
        public virtual string DaysBeforeExpiration
        {
            get { return Messages.labels_DaysBeforeExpiration; }
        }
        #endregion
        #region Days
        public abstract class days : PX.Data.IBqlField
        {
        }
        protected String _Days;
        [PXString]
        [PXUIField]
        public virtual string Days
        {
            get { return Messages.labels_Days; }
        }
        #endregion
        #region Min
        public abstract class min : PX.Data.IBqlField
        {
        }
        protected String _Min;
        [PXString]
        [PXUIField]
        public virtual string Min
        {
            get { return Messages.labels_Min; }
        }
        #endregion
        #endregion

		#region ServiceActivate
		public abstract class serviceActivate : PX.Data.IBqlField
		{
		}
		protected Boolean? _ServiceActivate;
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? ServiceActivate
		{
			get
			{
				return this._ServiceActivate;
			}
			set
			{
				this._ServiceActivate = value;
			}
		}
		#endregion
        #region IAttributeSupport Members
		#region ClassID
		public abstract class classID : IBqlField
		{
		}
		[PXString(20)]
		[PXFormula(typeof(Selector<Contract.templateID, Contract.contractCD>))]
		[PXSelector(typeof(Contract.contractCD))]
		public virtual string ClassID { get; set; }
		#endregion

        #region Attributes
        public abstract class attributes : IBqlField
		{
		}
		[CRAttributesField(typeof(CSAnswerType.contractAnswerType),
			typeof(Contract.contractID),
			typeof(Contract.classID))]
		public virtual string[] Attributes { get; set; }
		#endregion

		

		public int? ID
		{
			get { return ContractID; }
		}


		public virtual string EntityType
		{
			get { return CSAnswerType.Contract; }
		}

		#endregion

	}


    public static class ContractType
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { Renewable, Expiring, Unlimited},
                new string[] { Messages.Renewable, Messages.Expiring, Messages.Unlimited }) { ; }
        }
        public const string Renewable = "R";
        public const string Expiring = "E";
		public const string Unlimited = "U";

        public class ContractRenewable : Constant<string>
        {
            public ContractRenewable() : base(ContractType.Renewable) { ;}
        }

        public class ContractExpiring : Constant<string>
        {
            public ContractExpiring() : base(ContractType.Expiring) { ;}
        }

        public class ContractUnlimited : Constant<string>
        {
            public ContractUnlimited() : base(ContractType.Unlimited) { ;}
        }
    }

    public static class ContractStatus
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { Draft, InApproval, Activated, Expired, Canceled, InUpgrade, PendingActivation },
                new string[] { Messages.Draft, Messages.InApproval, Messages.Activated, Messages.Expired, Messages.Canceled, Messages.InUpgrade, Messages.PendingActivation }) { ; }
        }
        public const string Draft = "D";
        public const string InApproval = "I";
        public const string Activated = "A";
        public const string Expired = "E";
        public const string Canceled = "C";
		public const string Completed = "F";
        public const string InUpgrade = "U";
		public const string PendingActivation = "P";

        public class ContractStatusDraft : Constant<string>
        {
            public ContractStatusDraft() : base(ContractStatus.Draft) { ;}
        }
        public class ContractStatusInApproval : Constant<string>
        {
            public ContractStatusInApproval() : base(ContractStatus.InApproval) { ;}
        }
        public class ContractStatusActivated : Constant<string>
        {
            public ContractStatusActivated() : base(ContractStatus.Activated) { ;}
        }
        public class ContractStatusExpired : Constant<string>
        {
            public ContractStatusExpired() : base(ContractStatus.Expired) { ;}
        }
        public class ContractStatusCanceled : Constant<string>
        {
            public ContractStatusCanceled() : base(ContractStatus.Canceled) { ;}
        }
		public class ContractStatusCompleted : Constant<string>
		{
			public ContractStatusCompleted() : base(ContractStatus.Completed) { ;}
		}
        public class ContractStatusUpgrade : Constant<string>
        {
            public ContractStatusUpgrade() : base(ContractStatus.InUpgrade) { ;}
        }

    }

    public static class DurationType
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { Annual, Quarterly, Monthly, Custom },
                new string[] { Messages.Annual, Messages.Quarterly, Messages.Monthly, Messages.Custom }) { ; }
        }
        public const string Monthly = "M";
        public const string Quarterly = "Q";
        public const string Annual = "A";
        public const string Custom = "C";

        public class DurationAnnual : Constant<string>
        {
            public DurationAnnual() : base(DurationType.Annual) { ;}
        }
        public class DurationMonthly : Constant<string>
        {
            public DurationMonthly() : base(DurationType.Monthly) { ;}
        }
        public class DurationQuarterly : Constant<string>
        {
            public DurationQuarterly() : base(DurationType.Quarterly) { ;}
        }
        public class DurationCustom : Constant<string>
        {
            public DurationCustom() : base(DurationType.Custom) { ;}
        }
    }

	public static class CTRoundingTarget
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { None, Case, Activity },
				new string[] { Messages.None, Messages.Case, Messages.Activity }) { ; }
		}

        public const string None = "N";
		public const string Case = "C";
        public const string Activity = "A";
	}

	public static class ScheduleStartOption
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { SetupDate, ActivationDate },
				new string[] { Messages.SetupDate, Messages.ActivationDate }) { ; }
		}

		public const string SetupDate = "S";
		public const string ActivationDate = "A";
	}
}
