namespace PX.Objects.EP

{
	using System;
	using PX.Data;
	using PX.Objects.GL;
	using PX.Objects.CS;
	using PX.Objects.CR;
	using PX.Objects.AR;
	using PX.Objects.CM;
	using PX.Objects.IN;
	using PX.Objects.TX;
	using PX.SM;
	using PX.Objects.CT;
	using PX.Objects.PM;

	
	
	[System.SerializableAttribute()]
	public partial class EPExpenseClaimDetails : PX.Data.IBqlTable
	{
		#region RefNbr
		public abstract class refNbr : PX.Data.IBqlField
		{
		}
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(EPExpenseClaim.refNbr))]
		[PXParent(typeof(Select<EPExpenseClaim,Where<EPExpenseClaim.refNbr,Equal<Current<EPExpenseClaimDetails.refNbr>>>>))]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXLineNbr(typeof(EPExpenseClaim.lineCntr))]
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
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(typeof(EPExpenseClaim.curyInfoID))]
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
		#region ExpenseDate
		public abstract class expenseDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _ExpenseDate;
		[PXDBDate()]
		[PXDefault(typeof(Search<EPExpenseClaim.docDate, Where<EPExpenseClaim.refNbr, Equal<Current<EPExpenseClaimDetails.refNbr>>>>))]
		[PXUIField(DisplayName = "Date")]
		public virtual DateTime? ExpenseDate
		{
			get
			{
				return this._ExpenseDate;
			}
			set
			{
				this._ExpenseDate = value;
			}
		}
		#endregion
		#region ExpenseRefNbr
		public abstract class expenseRefNbr : PX.Data.IBqlField
		{
		}
		protected String _ExpenseRefNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "Ref. Nbr.")]
		public virtual String ExpenseRefNbr
		{
			get
			{
				return this._ExpenseRefNbr;
			}
			set
			{
				this._ExpenseRefNbr = value;
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
		[EPTax(typeof(EPExpenseClaim), typeof(EPTax), typeof(EPTaxTran))]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
        [PXDefault(typeof(Search<InventoryItem.taxCategoryID,
			Where<InventoryItem.inventoryID, Equal<Current<EPExpenseClaimDetails.inventoryID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[PXDefault()]
		[Inventory(typeof(Where<InventoryItem.itemType, Equal<INItemTypes.expenseItem>>), DisplayName = "Expense ID")]
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
		#region UOM
		public abstract class uOM : PX.Data.IBqlField
		{
		}
		protected String _UOM;
		[PXDefault(typeof(Search<InventoryItem.purchaseUnit, Where<InventoryItem.inventoryID, Equal<Current<EPExpenseClaimDetails.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[INUnit(typeof(EPExpenseClaimDetails.inventoryID), DisplayName = "UOM")]
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
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity", Visibility = PXUIVisibility.Visible)]
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
		#region CuryUnitCost
		public abstract class curyUnitCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryUnitCost;
        [PXDBCurrency(typeof(Search<INSetup.decPlPrcCst>), typeof(EPExpenseClaimDetails.curyInfoID), typeof(EPExpenseClaimDetails.unitCost))]
		[PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryUnitCost
		{
			get
			{
				return this._CuryUnitCost;
			}
			set
			{
				this._CuryUnitCost = value;
			}
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _UnitCost;
		[PXDBDecimal(6)]
		[PXDefault(typeof(Search<INItemCost.lastCost, Where<INItemCost.inventoryID, Equal<Current<EPExpenseClaimDetails.inventoryID>>>>))]
		public virtual Decimal? UnitCost
		{
			get
			{
				return this._UnitCost;
			}
			set
			{
				this._UnitCost = value;
			}
		}
		#endregion
		#region CuryEmployeePart
		public abstract class curyEmployeePart : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryEmployeePart;
		[PXDBCurrency(typeof(EPExpenseClaimDetails.curyInfoID), typeof(EPExpenseClaimDetails.employeePart))]
		[PXUIField(DisplayName = "Employee Part")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryEmployeePart
		{
			get
			{
				return this._CuryEmployeePart;
			}
			set
			{
				this._CuryEmployeePart = value;
			}
		}
		#endregion
		#region EmployeePart
		public abstract class employeePart : PX.Data.IBqlField
		{
		}
		protected Decimal? _EmployeePart;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? EmployeePart
		{
			get
			{
				return this._EmployeePart;
			}
			set
			{
				this._EmployeePart = value;
			}
		}
		#endregion
		#region CuryExtCost
		public abstract class curyExtCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryExtCost;
		[PXDBCurrency(typeof(EPExpenseClaimDetails.curyInfoID), typeof(EPExpenseClaimDetails.extCost))]
		[PXUIField(DisplayName = "Total Amount")]
		[PXFormula(typeof(Mult<EPExpenseClaimDetails.qty, EPExpenseClaimDetails.curyUnitCost>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryExtCost
		{
			get
			{
				return this._CuryExtCost;
			}
			set
			{
				this._CuryExtCost = value;
			}
		}
		#endregion
		#region ExtCost
		public abstract class extCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _ExtCost;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ExtCost
		{
			get
			{
				return this._ExtCost;
			}
			set
			{
				this._ExtCost = value;
			}
		}
		#endregion		
		#region CuryTranAmt
		public abstract class curyTranAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTranAmt;
		[PXDBCurrency(typeof(EPExpenseClaimDetails.curyInfoID), typeof(EPExpenseClaimDetails.tranAmt))]
		[PXFormula(typeof(Sub<EPExpenseClaimDetails.curyExtCost, EPExpenseClaimDetails.curyEmployeePart>))]
		[PXUIField(DisplayName = "Claim Amount", Enabled=false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranAmt
		{
			get
			{
				return this._CuryTranAmt;
			}
			set
			{
				this._CuryTranAmt = value;
			}
		}
		#endregion
		#region TranAmt
		public abstract class tranAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranAmt
		{
			get
			{
				return this._TranAmt;
			}
			set
			{
				this._TranAmt = value;
			}
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.IBqlField
		{
		}
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Transaction Descr.", Visibility = PXUIVisibility.Visible)]
		public virtual String TranDesc
		{
			get
			{
				return this._TranDesc;
			}
			set
			{
				this._TranDesc = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.IBqlField
		{
		}
		protected Int32? _CustomerID;
		[CustomerActive(DescriptionField = typeof(Customer.acctName))]
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
		#region CustomerLocationID
		public abstract class customerLocationID : PX.Data.IBqlField
		{
		}
		protected Int32? _CustomerLocationID;
		[PXDefault(typeof(Search<Customer.defLocationID, Where<Customer.bAccountID, Equal<Current<EPExpenseClaimDetails.customerID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[LocationID(typeof(Where<Location.bAccountID, Equal<Optional<EPExpenseClaimDetails.customerID>>>), DescriptionField = typeof(Location.descr))]
		public virtual Int32? CustomerLocationID
		{
			get
			{
				return this._CustomerLocationID;
			}
			set
			{
				this._CustomerLocationID = value;
			}
		}
		#endregion
        #region ContractID
        public abstract class contractID : PX.Data.IBqlField
        {
        }
        protected Int32? _ContractID;

        [PXDBInt]
        [PXUIField(DisplayName = "Project/Contract")]
		[PXDimensionSelector(ProjectAttribute.DimensionName, typeof(Search2<PMProject.contractID,
			LeftJoin<EPEmployeeContract, On<EPEmployeeContract.contractID, Equal<PMProject.contractID>, And<EPEmployeeContract.employeeID, Equal<Current<EPExpenseClaim.employeeID>>>>>,
			Where<PMProject.isTemplate, Equal<False>,
			And<PMProject.isActive, Equal<True>,
			And<PMProject.isCompleted, Equal<False>,
			And<Where<PMProject.baseType, Equal<Contract.ContractBaseType>, Or<Where<PMProject.restrictToEmployeeList, Equal<False>, Or<EPEmployeeContract.employeeID, IsNotNull>>>>>>>>,
			OrderBy<Desc<PMProject.contractCD>>>), typeof(PMProject.contractCD), Filterable = true, ValidComboRequired = true, CacheGlobal = true)]
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
        #region TaskID
        public abstract class taskID : PX.Data.IBqlField
        {
        }
        protected Int32? _TaskID;
        [ActiveProjectTask(typeof(EPExpenseClaimDetails.contractID), BatchModule.EP, AllowNullIfContract = true, DisplayName = "Project Task")]
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
		#region Billable
		public abstract class billable : PX.Data.IBqlField
		{
		}
		protected Boolean? _Billable;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Billable")]
		public virtual Boolean? Billable
		{
			get
			{
				return this._Billable;
			}
			set
			{
				this._Billable = value;
			}
		}
		#endregion
		#region Billed
		public abstract class billed : PX.Data.IBqlField
		{
		}
		protected Boolean? _Billed;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Billed
		{
			get
			{
				return this._Billed;
			}
			set
			{
				this._Billed = value;
			}
		}
		#endregion
		#region Approved
		public abstract class approved : PX.Data.IBqlField
		{
		}
		protected Boolean? _Approved;
		[PXDBBool()]
		[PXDefault(false)]
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
		#region Released
		public abstract class released : PX.Data.IBqlField
		{
		}
		protected Boolean? _Released;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
			}
		}
		#endregion
		#region ExpenseAccountID
		public abstract class expenseAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _ExpenseAccountID;
		[PXDefault]
		[Account(DisplayName = "Expense Account", Visibility = PXUIVisibility.Visible)]
		public virtual Int32? ExpenseAccountID
		{
			get
			{
				return this._ExpenseAccountID;
			}
			set
			{
				this._ExpenseAccountID = value;
			}
		}
		#endregion
		#region ExpenseSubID
		public abstract class expenseSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _ExpenseSubID;
		[SubAccount(typeof(EPExpenseClaimDetails.expenseAccountID), DisplayName = "Expense Sub.", Visibility = PXUIVisibility.Visible)]
		[PXDefault]
		public virtual Int32? ExpenseSubID
		{
			get
			{
				return this._ExpenseSubID;
			}
			set
			{
				this._ExpenseSubID = value;
			}
		}
		#endregion
		#region SalesAccountID
		public abstract class salesAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _SalesAccountID;
		[Account(DisplayName = "Sales Account", Visibility = PXUIVisibility.Visible)]
	  
		public virtual Int32? SalesAccountID
		{
			get
			{
				return this._SalesAccountID;
			}
			set
			{
				this._SalesAccountID = value;
			}
		}
		#endregion
		#region SalesSubID
		public abstract class salesSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _SalesSubID;
		[SubAccount(typeof(EPExpenseClaimDetails.salesAccountID), DisplayName = "Sales Sub.", Visibility = PXUIVisibility.Visible)]
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
		#region ARDocType
		public abstract class aRDocType : PX.Data.IBqlField
		{
		}
		protected String _ARDocType;
		[PXDBString(3, IsFixed = true)]
		[ARDocType.List()]
		[PXUIField(DisplayName = "AR Doument Type", Visibility = PXUIVisibility.Visible, Enabled = false, TabOrder = -1)]
		public virtual String ARDocType
		{
			get
			{
				return this._ARDocType;
			}
			set
			{
				this._ARDocType = value;
			}
		}
		#endregion
		#region ARRefNbr
		public abstract class aRRefNbr : PX.Data.IBqlField
		{
		}
		protected String _ARRefNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "AR Reference Nbr.")]
		[PXSelector(typeof(Search<ARInvoice.refNbr, Where<ARInvoice.docType, Equal<Optional<EPExpenseClaimDetails.aRDocType>>>>))]
		public virtual String ARRefNbr
		{
			get
			{
				return this._ARRefNbr;
			}
			set
			{
				this._ARRefNbr = value;
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
		#region NoteID
		public abstract class noteID : PX.Data.IBqlField
		{
		}
		protected Int64? _NoteID;
		[PXNote()]
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
		#region Selected
		public abstract class selected : IBqlField
		{
		}
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField]
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
	}
}
