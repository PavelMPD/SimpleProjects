namespace PX.Objects.CT
{
	using System;
	using PX.Data;
    using PX.Objects.CM;
    using PX.Objects.IN;
    using PX.Objects.CS;
	using PX.Objects.DR;
	using PX.Objects.AR;

	[System.SerializableAttribute()]
    [PXCacheName(Messages.ContractDetail)]
	[PXProjection(typeof(Select2<ContractDetail, 
		InnerJoin<Contract, On<ContractDetail.contractID, Equal<Contract.contractID>, And<ContractDetail.revID, Equal<Contract.revID>>>,
		LeftJoin<ContractDetailExt, On<ContractDetailExt.contractID, Equal<ContractDetail.contractID>, And<ContractDetailExt.lineNbr, Equal<ContractDetail.lineNbr>, And<ContractDetailExt.revID, Equal<Contract.lastActiveRevID>>>>>>>), new Type[] { typeof(ContractDetail) },
		Persistent = true)]
	public partial class ContractDetail : PX.Data.IBqlTable
	{
		#region ContractDetailID
		public abstract class contractDetailID : PX.Data.IBqlField
		{
		}
		protected Int32? _ContractDetailID;
		[PXDBIdentity()]
		public virtual Int32? ContractDetailID
		{
			get
			{
				return this._ContractDetailID;
			}
			set
			{
				this._ContractDetailID = value;
			}
		}
		#endregion
		#region ContractID
		public abstract class contractID : PX.Data.IBqlField
		{
		}
		protected Int32? _ContractID;
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(Contract.contractID))]
        [PXParent(typeof(Select<Contract, Where<Contract.contractID, Equal<Current<ContractDetail.contractID>>>>))]
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
		#region LineNbr
		public abstract class lineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[ContractLineNbr(typeof(Contract.lineCtr))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region RevID
		public abstract class revID : PX.Data.IBqlField
		{
		}
		[PXDBInt(MinValue = 1)]
		[PXDefault(typeof(Contract.revID), PersistingCheck = PXPersistingCheck.Null)]
		public virtual int? RevID { get; set; }
		#endregion
        #region TaskID
        public abstract class taskID : PX.Data.IBqlField
        {
        }
        protected Int32? _TaskID;
        [PXDefault(0)]
        [PXDBInt]
        public virtual Int32? TaskID
        {
            get
            {
                return this._TaskID;
            }
            set
            {
                this._TaskID = value;
            }
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.IBqlField
        {
        }
        protected Int32? _InventoryID;
        [PXDefault(PersistingCheck=PXPersistingCheck.Nothing)]
        [PXDBInt()]
        [PXUIField(DisplayName = "Non-Stock Item")]
        [PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.stkItem, Equal<False>, And<Match<Current<AccessInfo.userName>>>>>), typeof(InventoryItem.inventoryCD))]
        public virtual Int32? InventoryID
        {
            get
            {
                return this._InventoryID;
            }
            set
            {
                this._InventoryID = value;
            }
        }
        #endregion
        #region ContractItemID
        public abstract class contractItemID : PX.Data.IBqlField
        {
        }
        protected Int32? _ContractItemID;
        [PXDBInt()]
        [PXDefault()]
        [PXDimensionSelector(ContractItemAttribute.DimensionName, typeof(Search<ContractItem.contractItemID>),
                                                                    typeof(ContractItem.contractItemCD),
                                                                    typeof(ContractItem.contractItemCD), typeof(ContractItem.descr))]
        [PXUIField(DisplayName = "Item Code")]
        public virtual Int32? ContractItemID
        {
            get
            {
                return this._ContractItemID;
            }
            set
            {
                this._ContractItemID = value;
            }
        }
        #endregion
		#region CuryID
		public abstract class curyID : PX.Data.IBqlField
		{
		}
		#endregion
        #region CuryInfoID
        public abstract class curyInfoID : PX.Data.IBqlField
        {
        }
        protected Int64? _CuryInfoID;
        [PXDBLong()]
        [CurrencyInfo(ModuleCode = "CT")]
        public virtual Int64? CuryInfoID
        {
            get
            {
                return this._CuryInfoID;
            }
            set
            {
                this._CuryInfoID = value;
            }
        }
        #endregion
		#region ItemFee
		public abstract class itemFee : PX.Data.IBqlField
		{
		}
		protected Decimal? _ItemFee;
		[PXDBDecimal(typeof(Search<INSetup.decPlPrcCst>))]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? ItemFee
		{
			get
			{
				return this._ItemFee;
			}
			set
			{
				this._ItemFee = value;
			}
		}
		#endregion
        #region CuryItemFee
        public abstract class curyItemFee : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryItemFee;
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXDBCurrency(typeof(ContractDetail.curyInfoID), typeof(ContractDetail.itemFee))]
        [PXUIField(DisplayName = "Item Fee")]
        public virtual Decimal? CuryItemFee
        {
            get
            {
                return this._CuryItemFee;
            }
            set
            {
                this._CuryItemFee = value;
            }
        }
        #endregion
		#region AccountSource
		public abstract class accountSource : PX.Data.IBqlField
		{
		}
		protected String _AccountSource;
		[PXDBString(1, IsFixed = true)]
		public virtual String AccountSource
		{
			get
			{
				return this._AccountSource;
			}
			set
			{
				this._AccountSource = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.IBqlField
		{
		}
		protected Int32? _AccountID;
		[PXDBInt()]
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
		#region SubMask
		public abstract class subMask : PX.Data.IBqlField
		{
		}
		protected String _SubMask;
		[PXDBString(30, IsUnicode = true, InputMask = "")]
		public virtual String SubMask
		{
			get
			{
				return this._SubMask;
			}
			set
			{
				this._SubMask = value;
			}
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.IBqlField
		{
		}
		protected Int32? _SubID;
		[PXDBInt()]
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
		#region Description
		public abstract class description : PX.Data.IBqlField
		{
		}
		protected String _Description;
		[PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        [PXFormula(typeof(Selector<ContractDetail.contractItemID, ContractItem.descr>))]
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
		#region ResetUsage
		public abstract class resetUsage : PX.Data.IBqlField
		{
		}
		protected string _ResetUsage;
		[PXDefault(ResetUsageOption.Never)]
		[PXUIField(DisplayName = "Reset Usage", Required = true)]
		[PXDBString(1, IsFixed = true)]
		[ResetUsageOption.List()]
		public virtual string ResetUsage
		{
			get
			{
				return this._ResetUsage;
			}
			set
			{
				this._ResetUsage = value;
			}
		}
		#endregion
		#region Included
		public abstract class included : PX.Data.IBqlField
		{
		}
		protected Decimal? _Included;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal,"0.0")]
        [PXUIField(DisplayName = "Included", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? Included
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
		#region Max
		public abstract class max : PX.Data.IBqlField
		{
		}
		protected Decimal? _Max;
		[PXDBQuantity]
        [PXUIField(DisplayName = "Max.", Visibility = PXUIVisibility.Visible)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[Obsolete()]
		public virtual Decimal? Max
		{
			get
			{
				return this._Max;
			}
			set
			{
				this._Max = value;
			}
		}
		#endregion
		#region Min
		public abstract class min : PX.Data.IBqlField
		{
		}
		protected Decimal? _Min;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal,"0.0")]
        [PXUIField(DisplayName = "Min.", Visibility = PXUIVisibility.Visible)]
		[Obsolete()]
		public virtual Decimal? Min
		{
			get
			{
				return this._Min;
			}
			set
			{
				this._Min = value;
			}
		}
		#endregion
		#region Used
		public abstract class used : PX.Data.IBqlField
		{
		}
		protected Decimal? _Used;
		[PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Used", Visibility = PXUIVisibility.Visible, Enabled=false)]
        public virtual Decimal? Used
		{
			get
			{
				return this._Used;
			}
			set
			{
				this._Used = value;
			}
		}
		#endregion
        #region UsedTotal
        public abstract class usedTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _UsedTotal;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Used Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public virtual Decimal? UsedTotal
        {
            get
            {
                return this._UsedTotal;
            }
            set
            {
                this._UsedTotal = value;
            }
        }
        #endregion
        #region UOM
        public abstract class uOM : PX.Data.IBqlField
        {
        }
        protected String _UOM;
        [PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<ContractDetail.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [INUnit(typeof(ContractDetail.inventoryID))]
        public virtual String UOM
        {
            get
            {
                return this._UOM;
            }
            set
            {
                this._UOM = value;
            }
        }
        #endregion
		#region Qty
		public abstract class qty : PX.Data.IBqlField
		{
		}
		protected Decimal? _Qty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity")]
		public virtual Decimal? Qty
		{
			get
			{
				return this._Qty;
			}
			set
			{
				this._Qty = value;
			}
		}
		#endregion
		#region LastQty
		public abstract class lastQty : PX.Data.IBqlField
		{
		}
		[PXDBDecimal(BqlField = typeof(ContractDetailExt.qty))]
		public virtual Decimal? LastQty { get; set; }
		#endregion
		#region Change
		public abstract class change : PX.Data.IBqlField
		{
		}
		protected Decimal? _Change;
		[PXDecimal]
		[PXFormula(typeof(Sub<ContractDetail.qty, Switch<Case<Where<ContractDetail.lastQty, IsNotNull>, ContractDetail.lastQty>, int0>>))]
		[PXUIField(DisplayName = "Difference", Enabled = false)]
		public virtual Decimal? Change
		{
			get
			{
				return this._Change;
			}
			set
			{
				this._Change = value;
			}
		}
		#endregion
		#region Deposit
		public abstract class deposit : IBqlField { }
		[PXBool]
		[PXFormula(typeof(Selector<ContractDetail.contractItemID, ContractItem.deposit>))]
		public virtual bool? Deposit
		{
			get;
			set;
		}
		#endregion
		#region DepositAmt
		public abstract class depositAmt : IBqlField { }
		[PXDBCury(typeof(ContractDetail.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Deposit Amount", Enabled = false)]
		public virtual decimal? DepositAmt
		{
			get;
			set;
		}
		#endregion
		#region DepositUsed
		public abstract class depositUsed : IBqlField { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Deposit Used", Enabled = false)]
		public virtual decimal? DepositUsed
		{
			get;
			set;
		}
		#endregion
		#region DepositUsedTotal
		public abstract class depositUsedTotal : IBqlField { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Deposit Used Total", Enabled = false)]
		public virtual decimal? DepositUsedTotal
		{
			get;
			set;
		}
		#endregion
		#region Recurring Included
		public abstract class recurringIncluded : IBqlField { }
		[PXUIField(DisplayName = "Included", Enabled = false)]
		[PXDecimal]
		[PXFormula(typeof(Switch<Case<Where<ContractDetail.deposit, Equal<True>>, ContractDetail.depositAmt>, ContractDetail.qty>))]
		public virtual decimal? RecurringIncluded
		{
			get;
			set;
		}
		#endregion
		#region Recurring Used
		public abstract class recurringUsed : IBqlField { }
		[PXUIField(DisplayName = "Used", Enabled = false)]
		[PXDecimal]
		[PXFormula(typeof(Switch<Case<Where<ContractDetail.deposit, Equal<True>>, ContractDetail.depositUsed>, ContractDetail.used>))]
		public virtual decimal? RecurringUsed
		{
			get;
			set;
		}
		#endregion
		#region Recurring UsedTotal
		public abstract class recurringUsedTotal : IBqlField { }
		[PXUIField(DisplayName = "Used Total", Enabled = false)]
		[PXDecimal]
		[PXFormula(typeof(Switch<Case<Where<ContractDetail.deposit, Equal<True>>, ContractDetail.depositUsedTotal>, ContractDetail.usedTotal>))]
		public virtual decimal? RecurringUsedTotal
		{
			get;
			set;
		}
		#endregion
		#region LastBilledDate
		public abstract class lastBilledDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _LastBilledDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Last Billed Date", Enabled=false)]
        public virtual DateTime? LastBilledDate
        {
            get
            {
                return this._LastBilledDate;
            }
            set
            {
                this._LastBilledDate = value;
            }
        }
         #endregion
        #region LastBilledQty
        public abstract class lastBilledQty : PX.Data.IBqlField
        {
        }
        protected Decimal? _LastBilledQty;
		[PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Last Billed Qty.", Enabled=false)]
        public virtual Decimal? LastBilledQty
        {
            get
            {
                return this._LastBilledQty;
            }
            set
            {
                this._LastBilledQty = value;
            }
        }
        #endregion
		#region BaseDiscountID
		public abstract class baseDiscountID : PX.Data.IBqlField
		{
		}
		[PXDBString(10, IsUnicode = true)]
		public virtual String BaseDiscountID
		{
			get;
			set;
		}
		#endregion
		#region BaseDiscountSeq
		public abstract class baseDiscountSeq : PX.Data.IBqlField
		{
		}
		[PXDBString(10, IsUnicode = true)]
		public virtual String BaseDiscountSeq
		{
			get;
			set;
		}
		#endregion
		#region RecurringDiscountID
		public abstract class recurringDiscountID : PX.Data.IBqlField
		{
		}
		[PXDBString(10, IsUnicode = true)]
		public virtual String RecurringDiscountID
		{
			get;
			set;
		}
		#endregion
		#region RecurringDiscountSeq
		public abstract class recurringDiscountSeq : PX.Data.IBqlField
		{
		}
		[PXDBString(10, IsUnicode = true)]
		public virtual String RecurringDiscountSeq
		{
			get;
			set;
		}
		#endregion
		#region RenewalDiscountID
		public abstract class renewalDiscountID : PX.Data.IBqlField
		{
		}
		[PXDBString(10, IsUnicode = true)]
		public virtual String RenewalDiscountID
		{
			get;
			set;
		}
		#endregion
		#region RenewalDiscountSeq
		public abstract class renewalDiscountSeq : PX.Data.IBqlField
		{
		}
		[PXDBString(10, IsUnicode = true)]
		public virtual String RenewalDiscountSeq
		{
			get;
			set;
		}
		#endregion
        #region BaseDiscountPct
        public abstract class baseDiscountPct : IBqlField { }
		[PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Setup Discount,%", Enabled = false)]
		public decimal? BaseDiscountPct
        {
            get;
            set;
        }
        #endregion
        #region RecurringDiscountPct
        public abstract class recurringDiscountPct : IBqlField { }
		[PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Recurring Discount,%", Enabled = false)]
		public decimal? RecurringDiscountPct
        {
            get;
            set;
        }
        #endregion
        #region RenewalDiscountPct
        public abstract class renewalDiscountPct : IBqlField { }
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Renewal Discount,%", Enabled = false)]
		public decimal? RenewalDiscountPct
        {
            get;
            set;
        }
        #endregion
		#region LastBaseDiscountPct
		public abstract class lastBaseDiscountPct : IBqlField { }
		[PXDBDecimal(BqlField = typeof(ContractDetailExt.baseDiscountPct))]
		public virtual decimal? LastBaseDiscountPct
		{
			get;
			set;
		}
		#endregion
		#region LastRecurringDiscountPct
		public abstract class lastRecurringDiscountPct : IBqlField { }
		[PXDBDecimal(BqlField = typeof(ContractDetailExt.recurringDiscountPct))]
		public virtual decimal? LastRecurringDiscountPct
		{
			get;
			set;
		}
		#endregion
		#region LastRenewalDiscountPct
		public abstract class lastRenewalDiscountPct : IBqlField { }
		[PXDBDecimal(BqlField = typeof(ContractDetailExt.renewalDiscountPct))]
		public virtual decimal? LastRenewalDiscountPct
		{
			get;
			set;
		}
		#endregion
		#region BaseDiscountAmt
		public abstract class baseDiscountAmt : IBqlField { }
		[PXDecimal(6)]
        [PXUIField(DisplayName = "Setup Discount", Enabled = false)]
		public decimal? BaseDiscountAmt
		{
			get;
			set;
		}
		#endregion
		#region RecurringDiscountAmt
		public abstract class recurringDiscountAmt : IBqlField { }
		[PXDecimal(6)]
		[PXUIField(DisplayName = "Recurring Discount", Enabled = false)]
		public decimal? RecurringDiscountAmt
		{
			get;
			set;
		}
		#endregion
		#region RenewalDiscountAmt
		public abstract class renewalDiscountAmt : IBqlField { }
		[PXDecimal(6)]
		[PXUIField(DisplayName = "Renewal Discount", Enabled = false)]
		public decimal? RenewalDiscountAmt
		{
			get;
			set;
		}
		#endregion

		#region BasePrice
		public abstract class basePrice : IBqlField { }
		[PXDBDecimal(6)]
		[PXFormula(typeof(Selector<ContractDetail.contractItemID, ContractItem.basePrice>))]
		public decimal? BasePrice
		{
			get;
			set;
		}
		#endregion
		#region BasePriceOption
		public abstract class basePriceOption : IBqlField { }
		[PXDBString(1, IsFixed = true)]
		[PXFormula(typeof(Selector<ContractDetail.contractItemID, ContractItem.basePriceOption>))]
		public string BasePriceOption
		{
			get;
			set;
		}
		#endregion
		#region RenewalPrice
		public abstract class renewalPrice : IBqlField { }
		[PXDBDecimal(6)]
		[PXFormula(typeof(Selector<ContractDetail.contractItemID, ContractItem.renewalPrice>))]
		public decimal? RenewalPrice
		{
			get;
			set;
		}
		#endregion
		#region RenewalPriceOption
		public abstract class renewalPriceOption : IBqlField { }
		[PXDBString(1, IsFixed = true)]
		[PXFormula(typeof(Selector<ContractDetail.contractItemID, ContractItem.renewalPriceOption>))]
		public string RenewalPriceOption
		{
			get;
			set;
		}
		#endregion
		#region FixedRecurringPrice
		public abstract class fixedRecurringPrice : IBqlField { }
		[PXDBDecimal(6)]
		[PXFormula(typeof(Selector<ContractDetail.contractItemID, ContractItem.fixedRecurringPrice>))]
		public decimal? FixedRecurringPrice
		{
			get;
			set;
		}
		#endregion
		#region FixedRecurringPriceOption
		public abstract class fixedRecurringPriceOption : IBqlField { }
		[PXDBString(1, IsFixed = true)]
		[PXFormula(typeof(Selector<ContractDetail.contractItemID, ContractItem.fixedRecurringPriceOption>))]
		public string FixedRecurringPriceOption
		{
			get;
			set;
		}
		#endregion
		#region UsagePrice
		public abstract class usagePrice : IBqlField { }
		[PXDBDecimal(6)]
		[PXFormula(typeof(Selector<ContractDetail.contractItemID, ContractItem.usagePrice>))]
		public decimal? UsagePrice
		{
			get;
			set;
		}
		#endregion
		#region UsagePriceOption
		public abstract class usagePriceOption : IBqlField { }
		[PXDBString(1, IsFixed = true)]
		[PXFormula(typeof(Selector<ContractDetail.contractItemID, ContractItem.usagePriceOption>))]
		public string UsagePriceOption
		{
			get;
			set;
		}
		#endregion
		#region BasePriceVal
		public abstract class basePriceVal : IBqlField { }
		[PXDecimal(6)]
		[PXFormula(typeof(
			Switch<
				Case<Where<Selector<ContractDetail.contractItemID, ContractItem.baseItemID>, IsNull>, decimal0,
				Case<Where<ContractDetail.basePriceOption, Equal<PriceOption.itemPrice>>, Selector<ContractDetail.contractItemID, IsNull<NullIf<Selector<ContractItem.baseItemID, ARSalesPrice2.salesPrice>, decimal0>, Selector<ContractItem.baseItemID, ARSalesPrice.salesPrice>>>,
				Case<Where<ContractDetail.basePriceOption, Equal<PriceOption.itemPercent>>, Div<Mult<ContractItem.basePrice, IsNull<NullIf<Selector<ContractItem.baseItemID, ARSalesPrice2.salesPrice>, decimal0>, Selector<ContractItem.baseItemID, ARSalesPrice.salesPrice>>>, decimal100>>>>,
				Current<ContractDetail.basePrice>>))]
		[PXUIField(DisplayName = "Setup Price")]
		public decimal? BasePriceVal
		{
			get;
			set;
		}
		#endregion
		#region BasePriceEditable
		public abstract class basePriceEditable : IBqlField { }
		[PXBool()]
		[PXFormula(typeof(
			Switch<
				Case<Where<Selector<ContractDetail.contractItemID, ContractItem.baseItemID>, IsNull, Or<ContractDetail.basePriceOption, NotEqual<PriceOption.manually>>>, False>,
				True>))]
		public bool? BasePriceEditable
		{
			get;
			set;
		}
		#endregion
		#region RenewalPriceVal
		public abstract class renewalPriceVal : IBqlField { }
		[PXDecimal(6)]
		[PXFormula(typeof(
			Switch<
				Case<Where<Selector<ContractDetail.contractItemID, ContractItem.renewalItemID>, IsNull>, decimal0,
				Case<Where<ContractDetail.renewalPriceOption, Equal<PriceOption.basePercent>>, Div<Mult<ContractDetail.renewalPrice, ContractDetail.basePriceVal>, decimal100>,
				Case<Where<ContractDetail.renewalPriceOption, Equal<PriceOption.itemPrice>>, Selector<ContractDetail.contractItemID, IsNull<NullIf<Selector<ContractItem.renewalItemID, ARSalesPrice2.salesPrice>, decimal0>, Selector<ContractItem.renewalItemID, ARSalesPrice.salesPrice>>>,
				Case<Where<ContractDetail.renewalPriceOption, Equal<PriceOption.itemPercent>>, Div<Mult<ContractDetail.renewalPrice, Selector<ContractDetail.contractItemID, IsNull<NullIf<Selector<ContractItem.renewalItemID, ARSalesPrice2.salesPrice>, decimal0>, Selector<ContractItem.renewalItemID, ARSalesPrice.salesPrice>>>>, decimal100>>>>>,
				ContractDetail.renewalPrice>
				))]
		[PXUIField(DisplayName = "Renewal Price")]
		public decimal? RenewalPriceVal
		{
			get;
			set;
		}
		#endregion
		#region RenewalPriceEditable
		public abstract class renewalPriceEditable : IBqlField { }
		[PXBool()]
		[PXFormula(typeof(
			Switch<
				Case<Where<Selector<ContractDetail.contractItemID, ContractItem.renewalItemID>, IsNull, Or<ContractDetail.renewalPriceOption, NotEqual<PriceOption.manually>>>, False>,
				True>))]
		public bool? RenewalPriceEditable
		{
			get;
			set;
		}
		#endregion
		#region FixedRecurringPriceVal
		public abstract class fixedRecurringPriceVal : IBqlField { }
		[PXDecimal(6)]
		[PXFormula(typeof(
			Switch<
				Case<Where<Selector<ContractDetail.contractItemID, ContractItem.recurringItemID>, IsNull>, decimal0,
				Case<Where<ContractDetail.fixedRecurringPriceOption, Equal<PriceOption.basePercent>>, Div<Mult<ContractDetail.fixedRecurringPrice, ContractDetail.basePriceVal>, decimal100>,
				Case<Where<ContractDetail.fixedRecurringPriceOption, Equal<PriceOption.itemPrice>>, Selector<ContractDetail.contractItemID, IsNull<NullIf<Selector<ContractItem.recurringItemID, ARSalesPrice2.salesPrice>, decimal0>, Selector<ContractItem.recurringItemID, ARSalesPrice.salesPrice>>>,
				Case<Where<ContractDetail.fixedRecurringPriceOption, Equal<PriceOption.itemPercent>>, Div<Mult<ContractDetail.fixedRecurringPrice, Selector<ContractDetail.contractItemID, IsNull<NullIf<Selector<ContractItem.recurringItemID, ARSalesPrice2.salesPrice>, decimal0>, Selector<ContractItem.recurringItemID, ARSalesPrice.salesPrice>>>>, decimal100>>>>>,
				ContractDetail.fixedRecurringPrice>
				))]
		[PXUIField(DisplayName = "Recurring Price")]
		public decimal? FixedRecurringPriceVal
		{
			get;
			set;
		}
		#endregion
		#region FixedRecurringPriceEditable
		public abstract class fixedRecurringPriceEditable : IBqlField { }
		[PXBool()]
		[PXFormula(typeof(
			Switch<
				Case<Where<Selector<ContractDetail.contractItemID, ContractItem.recurringItemID>, IsNull, Or<ContractDetail.fixedRecurringPriceOption, NotEqual<PriceOption.manually>>>, False>,
				True>))]
		public bool? FixedRecurringPriceEditable
		{
			get;
			set;
		}
		#endregion
		#region UsagePriceVal
		public abstract class usagePriceVal : IBqlField { }
		[PXDecimal(6)]
		[PXFormula(typeof(
			Switch<
				Case<Where<Selector<ContractDetail.contractItemID, ContractItem.recurringItemID>, IsNull>, decimal0,
				Case<Where<ContractDetail.usagePriceOption, Equal<PriceOption.basePercent>>, Div<Mult<ContractDetail.usagePrice, ContractDetail.basePriceVal>, decimal100>,
				Case<Where<ContractDetail.usagePriceOption, Equal<PriceOption.itemPrice>>, Selector<ContractDetail.contractItemID, IsNull<NullIf<Selector<ContractItem.recurringItemID, ARSalesPrice2.salesPrice>, decimal0>, Selector<ContractItem.recurringItemID, ARSalesPrice.salesPrice>>>,
				Case<Where<ContractDetail.usagePriceOption, Equal<PriceOption.itemPercent>>, Div<Mult<ContractDetail.usagePrice, Selector<ContractDetail.contractItemID, IsNull<NullIf<Selector<ContractItem.recurringItemID, ARSalesPrice2.salesPrice>, decimal0>, Selector<ContractItem.recurringItemID, ARSalesPrice.salesPrice>>>>, decimal100>>>>>,
				ContractDetail.usagePrice>
				))]
		[PXUIField(DisplayName = "Extra Usage Price")]
		public decimal? UsagePriceVal
		{
			get;
			set;
		}
		#endregion
		#region UsagePriceEditable
		public abstract class usagePriceEditable : IBqlField { }
		[PXBool()]
		[PXFormula(typeof(
			Switch<
				Case<Where<Selector<ContractDetail.contractItemID, ContractItem.recurringItemID>, IsNull, Or<ContractDetail.usagePriceOption, NotEqual<PriceOption.manually>>>, False>,
				True>))]
		public bool? UsagePriceEditable
		{
			get;
			set;
		}
		#endregion
        #region WarningAmountForDeposit
        public abstract class warningAmountForDeposit : IBqlField { }
        protected bool? _WarningAmountForDeposit;
        [PXBool()]          
        public virtual bool? WarningAmountForDeposit
        {
            get { return _WarningAmountForDeposit; }
            set { _WarningAmountForDeposit = value; }
        }
        #endregion
		#region System Columns
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
	}

	[System.SerializableAttribute()]
	[PXProjection(typeof(Select<ContractDetailExt>), Persistent = true)]
	[PXBreakInheritance()]
	public partial class ContractDetailExt : ContractDetail
	{
		#region ContractDetailID
		new public abstract class contractDetailID : PX.Data.IBqlField
		{
		}
		new protected Int32? _ContractDetailID;
		[PXDBIdentity(IsKey = true)]
		public override Int32? ContractDetailID
		{
			get
			{
				return this._ContractDetailID;
			}
			set
			{
				this._ContractDetailID = value;
			}
		}
		#endregion
		#region ContractID
		new public abstract class contractID : PX.Data.IBqlField
		{
		}
		new protected Int32? _ContractID;
		#endregion
		#region LineNbr
		new public abstract class lineNbr : PX.Data.IBqlField
		{
		}
		new protected Int32? _LineNbr;
		[PXDBInt()]
		[PXLineNbr(typeof(Contract.lineCtr))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		public override Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region RevID
		new public abstract class revID : PX.Data.IBqlField
		{
		}
		[PXDBInt(MinValue = 1, IsKey = true)]
		[PXDefault(typeof(Contract.revID), PersistingCheck = PXPersistingCheck.Null)]
		public override int? RevID { get; set; }
		#endregion
		#region ContractItemID
		new public abstract class contractItemID : PX.Data.IBqlField
		{
		}
		#endregion
		#region Qty
		new public abstract class qty : PX.Data.IBqlField
		{
		}
		#endregion
		#region Used
		new public abstract class used : PX.Data.IBqlField
		{
		}
		#endregion
		#region UsedTotal
		new public abstract class usedTotal : PX.Data.IBqlField
		{
		}
		new protected Decimal? _UsedTotal;
		#endregion
		#region LastQty
		new public abstract class lastQty : PX.Data.IBqlField
		{
		}
		[PXQuantity()]
		public override Decimal? LastQty { get; set; }
		#endregion
		#region Deposit
		new public abstract class deposit : IBqlField { }
		[PXBool]
		[PXFormula(typeof(Selector<ContractDetailExt.contractItemID, ContractItem.deposit>))]
		public override bool? Deposit
		{
			get;
			set;
		}
		#endregion
		#region DepositAmt
		new public abstract class depositAmt : IBqlField { }
		#endregion
		#region DepositUsed
		new public abstract class depositUsed : IBqlField { }
		#endregion
		#region DepositUsedTotal
		new public abstract class depositUsedTotal : IBqlField { }
		#endregion
		#region Recurring Included
		new public abstract class recurringIncluded : IBqlField { }
		[PXUIField(DisplayName = "Included", Enabled = false)]
		[PXDecimal]
		[PXFormula(typeof(Switch<Case<Where<ContractDetailExt.deposit, Equal<True>>, ContractDetailExt.depositAmt>, ContractDetailExt.qty>))]
		public override decimal? RecurringIncluded
		{
			get;
			set;
		}
		#endregion
		#region Recurring Used
		new public abstract class recurringUsed : IBqlField { }
		[PXUIField(DisplayName = "Used", Enabled = false)]
		[PXDecimal]
		[PXFormula(typeof(Switch<Case<Where<ContractDetailExt.deposit, Equal<True>>, ContractDetailExt.depositUsed>, ContractDetailExt.used>))]
		public override decimal? RecurringUsed
		{
			get;
			set;
		}
		#endregion
		#region Recurring UsedTotal
		new public abstract class recurringUsedTotal : IBqlField { }
		[PXUIField(DisplayName = "Used Total", Enabled = false)]
		[PXDecimal]
		[PXFormula(typeof(Switch<Case<Where<ContractDetailExt.deposit, Equal<True>>, ContractDetailExt.depositUsedTotal>, ContractDetailExt.usedTotal>))]
		public override decimal? RecurringUsedTotal
		{
			get;
			set;
		}
		#endregion
		#region BaseDiscountPct
		new public abstract class baseDiscountPct : IBqlField { }
		#endregion
		#region RecurringDiscountPct
		new public abstract class recurringDiscountPct : IBqlField { }
		#endregion
		#region RenewalDiscountPct
		new public abstract class renewalDiscountPct : IBqlField { }
		#endregion
		#region LastBaseDiscountPct
		new public abstract class lastBaseDiscountPct : IBqlField { }
		[PXDecimal()]
		public override decimal? LastBaseDiscountPct
		{
			get;
			set;
		}
		#endregion
		#region LastRecurringDiscountPct
		new public abstract class lastRecurringDiscountPct : IBqlField { }
		[PXDecimal()]
		public override decimal? LastRecurringDiscountPct
		{
			get;
			set;
		}
		#endregion
		#region LastRenewalDiscountPct
		new public abstract class lastRenewalDiscountPct : IBqlField { }
		[PXDecimal()]
		public override decimal? LastRenewalDiscountPct
		{
			get;
			set;
		}
		#endregion
	}
	
	
	[System.SerializableAttribute()]
	[ContractDetailAccumAttribute]
	[PXBreakInheritance()]
	public partial class ContractDetailAcum : ContractDetail
	{
		#region ContractDetailID
		public new abstract class contractDetailID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		public override Int32? ContractDetailID
		{
			get
			{
				return this._ContractDetailID;
			}
			set
			{
				this._ContractDetailID = value;
			}
		}
		#endregion
		#region ContractID
		public new abstract class contractID : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
		public override Int32? ContractID
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
		#region LineNbr
		public new abstract class lineNbr : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
		public override Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region RevID
		public new abstract class revID : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
		public override int? RevID { get; set; }
		#endregion
        #region TaskID
        public new abstract class taskID : PX.Data.IBqlField
        {
        }
        [PXDBInt()]
        public override Int32? TaskID
        {
            get
            {
                return this._TaskID;
            }
            set
            {
                this._TaskID = value;
            }
        }
        #endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}		
		
		[PXDBInt()]
		public override Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		[PXDBLong()]
		[CurrencyInfo(ModuleCode = "CT")]
		public override Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region ItemFee
		public new abstract class itemFee : PX.Data.IBqlField
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? ItemFee
		{
			get
			{
				return this._ItemFee;
			}
			set
			{
				this._ItemFee = value;
			}
		}
		#endregion
		#region CuryItemFee
		public new abstract class curyItemFee : PX.Data.IBqlField
		{
		}
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ContractDetail.curyInfoID), typeof(ContractDetail.itemFee))]
		[PXUIField(DisplayName = "Item Fee")]
		public override Decimal? CuryItemFee
		{
			get
			{
				return this._CuryItemFee;
			}
			set
			{
				this._CuryItemFee = value;
			}
		}
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
		public override Int32? AccountID
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
		public new abstract class subID : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
		public override Int32? SubID
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
		#region Description
		public new abstract class description : PX.Data.IBqlField
		{
		}
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public override String Description
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
		#region ResetUsage
		public new abstract class resetUsage : PX.Data.IBqlField
		{
		}
		[PXDefault(ResetUsageOption.Never)]
		[PXUIField(DisplayName = "Reset Usage")]
		[PXDBString(1, IsFixed = true)]
		[ResetUsageOption.List()]
		public override string ResetUsage
		{
			get
			{
				return this._ResetUsage;
			}
			set
			{
				this._ResetUsage = value;
			}
		}
		#endregion
		#region Included
		public new abstract class included : PX.Data.IBqlField
		{
		}
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Included", Visibility = PXUIVisibility.Visible)]
		public override Decimal? Included
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
		#region Used
		public new abstract class used : PX.Data.IBqlField
		{
		}
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Used", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public override Decimal? Used
		{
			get
			{
				return this._Used;
			}
			set
			{
				this._Used = value;
			}
		}
		#endregion
        #region UsedTotal
        public new abstract class usedTotal : PX.Data.IBqlField
        {
        }
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Used Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public override Decimal? UsedTotal
        {
            get
            {
                return this._UsedTotal;
            }
            set
            {
                this._UsedTotal = value;
            }
        }
        #endregion
        #region UOM
        public new abstract class uOM : PX.Data.IBqlField
        {
        }
        [PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<ContractDetail.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [INUnit(typeof(ContractDetail.inventoryID))]
        public override String UOM
        {
            get
            {
                return this._UOM;
            }
            set
            {
                this._UOM = value;
            }
        }
        #endregion
		#region LastBilledDate
		public new abstract class lastBilledDate : PX.Data.IBqlField
		{
		}
		[PXDBDate()]
		[PXUIField(DisplayName = "Last Billed Date", Enabled = false)]
		public override DateTime? LastBilledDate
		{
			get
			{
				return this._LastBilledDate;
			}
			set
			{
				this._LastBilledDate = value;
			}
		}
		#endregion
		#region LastBilledQty
		public new abstract class lastBilledQty : PX.Data.IBqlField
		{
		}
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last Billed Qty.", Enabled = false)]
		public override Decimal? LastBilledQty
		{
			get
			{
				return this._LastBilledQty;
			}
			set
			{
				this._LastBilledQty = value;
			}
		}
		#endregion

		#region System Columns
		#region tstamp
		public new abstract class Tstamp : PX.Data.IBqlField
		{
		}
		[PXDBTimestamp()]
		public override Byte[] tstamp
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
		public new abstract class createdByID : PX.Data.IBqlField
		{
		}
		[PXDBCreatedByID()]
		public override Guid? CreatedByID
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
		public new abstract class createdByScreenID : PX.Data.IBqlField
		{
		}
		[PXDBCreatedByScreenID()]
		public override String CreatedByScreenID
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
		public new abstract class createdDateTime : PX.Data.IBqlField
		{
		}
		[PXDBCreatedDateTime()]
		public override DateTime? CreatedDateTime
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
		public new abstract class lastModifiedByID : PX.Data.IBqlField
		{
		}
		[PXDBLastModifiedByID()]
		public override Guid? LastModifiedByID
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
		public new abstract class lastModifiedByScreenID : PX.Data.IBqlField
		{
		}
		[PXDBLastModifiedByScreenID()]
		public override String LastModifiedByScreenID
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
		public new abstract class lastModifiedDateTime : PX.Data.IBqlField
		{
		}
		[PXDBLastModifiedDateTime()]
		public override DateTime? LastModifiedDateTime
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
	}
	
    public static class ContractDetailType
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { Setup, Renewal, Billing, UsagePrice, Reinstallment },
                new string[] { Messages.Setup, Messages.Renewal, Messages.Billing, Messages.UsagePrice, Messages.Reinstallment }) { ; }
        }
        public const string Setup = "S";
        public const string Renewal = "R";
        public const string Billing = "B";
        public const string UsagePrice = "U";
		public const string Reinstallment = "I";

        public class ContractDetailSetup : Constant<string>
        {
            public ContractDetailSetup() : base(ContractDetailType.Setup) { ;}
        }
        public class ContractDetailRenewal : Constant<string>
        {
            public ContractDetailRenewal() : base(ContractDetailType.Renewal) { ;}
        }
        public partial class ContractDetail : Constant<string>
        {
            public ContractDetail() : base(ContractDetailType.Billing) { ;}
        }
        public class ContractDetailUsagePrice : Constant<string>
        {
            public ContractDetailUsagePrice() : base(ContractDetailType.UsagePrice) { ;}
        }
		public class ContractDetailReinstallment : Constant<string>
		{
			public ContractDetailReinstallment() : base(ContractDetailType.Reinstallment) { ;}
		}
    }

	public static class ResetUsageOption
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Never, OnBilling, OnRenewal },
				new string[] { Messages.Never, Messages.OnBilling, Messages.OnRenewal }) { ; }
		}

		public class ListForProjectAttribute : PXStringListAttribute
		{
			public ListForProjectAttribute()
				: base(
				new string[] { Never, OnBilling },
				new string[] { Messages.Never, Messages.OnBilling }) { ; }
		}

		public const string Never = "N";
		public const string OnBilling = "B";
		public const string OnRenewal = "R";

		public class onBilling : Constant<string>
		{
			public onBilling() : base(ResetUsageOption.OnBilling) { ;}
		}

		public class onRenewal : Constant<string>
		{
			public onRenewal() : base(ResetUsageOption.OnRenewal) { ;}
		}
	}

	public static class ContractDetailTypeSetupAndRenewal
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { ContractDetailType.Setup, ContractDetailType.Renewal, ContractDetailType.Reinstallment },
				new string[] { Messages.Setup, Messages.Renewal, Messages.Reinstallment }) { ; }
		}
	}
}
