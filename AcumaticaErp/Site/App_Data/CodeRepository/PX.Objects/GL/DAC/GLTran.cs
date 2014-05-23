namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	using PX.Objects.CM;
	using PX.Objects.CS;
    using PX.Objects.PM;
	using PX.Objects.CR;
    using PX.Objects.TX;
	
	[System.SerializableAttribute()]
	[PXCacheName(Messages.Transaction)]
	[PXPrimaryGraph(typeof(JournalEntry))]
	public partial class GLTran : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.IBqlField
		{
		}
		protected Int32? _BranchID;
		[Branch(typeof(Batch.branchID))]
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
		#region Module
		public abstract class module : PX.Data.IBqlField
		{
		}
		protected String _Module;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(Batch))]
		[PXUIField(DisplayName="Module",Visibility=PXUIVisibility.Visible,Visible=false)]
		public virtual String Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;
			}
		}
		#endregion
		#region BatchNbr
		public abstract class batchNbr : PX.Data.IBqlField
		{
		}
		protected String _BatchNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(Batch))]
		[PXParent(typeof(Select<Batch,Where<Batch.module,Equal<Current<GLTran.module>>,And<Batch.batchNbr, Equal<Current<GLTran.batchNbr>>>>>))]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual String BatchNbr
		{
			get
			{
				return this._BatchNbr;
			}
			set
			{
				this._BatchNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		[PXLineNbr(typeof(Batch.lineCntr))]
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
		#region LedgerID
		public abstract class ledgerID : PX.Data.IBqlField
		{
		}
		protected Int32? _LedgerID;
		[PXDBInt()]
		[PXFormula(typeof(Switch<Case<Where<Selector<Current<Batch.ledgerID>, Ledger.balanceType>, Equal<LedgerBalanceType.actual>>, Selector<GLTran.branchID, Branch.ledgerID>>, Current<Batch.ledgerID>>))]
		[PXDefault()]
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
		#region AccountID
		public abstract class accountID : PX.Data.IBqlField
		{
		}
		protected Int32? _AccountID;
		[Account(typeof(GLTran.branchID), LedgerID = typeof(GLTran.ledgerID), DescriptionField = typeof(Account.description))]
		[PXDefault]
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
		#region SubID
		public abstract class subID : PX.Data.IBqlField
		{
		}
		protected Int32? _SubID;
		[SubAccount(typeof(GLTran.accountID), typeof(GLTran.branchID))]
		[PXDefault]
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
		#region ProjectID
		public abstract class projectID : PX.Data.IBqlField
		{
		}
		protected Int32? _ProjectID;
        [GLProjectDefault(typeof(GLTran.ledgerID), AccountType = typeof(GLTran.accountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[ActiveProjectForModule(BatchModule.GL, true)]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.IBqlField
		{
		}
		protected Int32? _TaskID;
		[ActiveProjectTask(typeof(GLTran.projectID), BatchModule.GL, AllowNullIfContract = true, DisplayName = "Project Task")]
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
		#region RefNbr
		public abstract class refNbr : PX.Data.IBqlField
		{
		}
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true)]
		//[PXDefault("")]
		[PXDBLiteDefault(typeof(Batch.refNbr), PersistingCheck = PXPersistingCheck.Nothing, DefaultForUpdate = false, DefaultForInsert = false)]
		[PXUIField(DisplayName = "Ref. Number", Visibility = PXUIVisibility.Visible)]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[IN.Inventory(Enabled = false,Visible = false)]
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
		[IN.INUnit(typeof(GLTran.inventoryID), typeof(GLTran.accountID), typeof(GLTran.accountRequireUnits))]
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
		[IN.PXDBQuantity()]
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
		#region DebitAmt
		public abstract class debitAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _DebitAmt;
		[PXDBBaseCury(typeof(GLTran.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(null, typeof(SumCalc<Batch.debitTotal>))]
		public virtual Decimal? DebitAmt
		{
			get
			{
				return this._DebitAmt;
			}
			set
			{
				this._DebitAmt = value;
			}
		}
		#endregion
		#region CreditAmt
		public abstract class creditAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CreditAmt;
		[PXDBBaseCury(typeof(GLTran.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(null, typeof(SumCalc<Batch.creditTotal>))]
		public virtual Decimal? CreditAmt
		{
			get
			{
				return this._CreditAmt;
			}
			set
			{
				this._CreditAmt = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(typeof(CurrencyInfo.curyInfoID))]
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
		#region CuryDebitAmt
		public abstract class curyDebitAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryDebitAmt;
	//	[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Debit Amount", Visibility = PXUIVisibility.Visible)]
		[PXFormula(null, typeof(SumCalc<Batch.curyDebitTotal>))]
		[PXDBCurrency(typeof(GLTran.curyInfoID),typeof(GLTran.debitAmt))]
		public virtual Decimal? CuryDebitAmt
		{
			get
			{
				return this._CuryDebitAmt;
			}
			set
			{
				this._CuryDebitAmt = value;
			}
		}
		#endregion
		#region CuryCreditAmt
		public abstract class curyCreditAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryCreditAmt;
	//	[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Credit Amount", Visibility = PXUIVisibility.Visible)]
		[PXFormula(null, typeof(SumCalc<Batch.curyCreditTotal>))]
		[PXDBCurrency(typeof(GLTran.curyInfoID), typeof(GLTran.creditAmt))]
		public virtual Decimal? CuryCreditAmt
		{
			get
			{
				return this._CuryCreditAmt;
			}
			set
			{
				this._CuryCreditAmt = value;
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
		#region Posted
		public abstract class posted : PX.Data.IBqlField
		{
		}
		protected Boolean? _Posted;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Posted
		{
			get
			{
				return this._Posted;
			}
			set
			{
				this._Posted = value;
			}
		}
		#endregion
        #region NonBillable
        public abstract class nonBillable : PX.Data.IBqlField
        {
        }
        protected Boolean? _NonBillable;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Non Billable")]
        public virtual Boolean? NonBillable
        {
            get
            {
                return this._NonBillable;
            }
            set
            {
                this._NonBillable = value;
            }
        }
        #endregion
		#region IsInterCompany
		public abstract class isInterCompany : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsInterCompany;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsInterCompany
		{
			get
			{
				return this._IsInterCompany;
			}
			set
			{
				this._IsInterCompany = value;
			}
		}
		#endregion
		#region SummPost
		public abstract class summPost : PX.Data.IBqlField
		{
		}
		protected Boolean? _SummPost;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? SummPost
		{
			get
			{
				return this._SummPost;
			}
			set
			{
				this._SummPost = value;
			}
		}
		#endregion
		#region ZeroPost
		public abstract class zeroPost : PX.Data.IBqlField
		{
		}
		[PXBool()]
		public virtual Boolean? ZeroPost 
		{
			get;
			set;
		}
		#endregion
		#region OrigModule
		public abstract class origModule : PX.Data.IBqlField
		{
		}
		protected String _OrigModule;
		[PXDBString(2, IsFixed = true)]
		public virtual String OrigModule
		{
			get
			{
				return this._OrigModule;
			}
			set
			{
				this._OrigModule = value;
			}
		}
		#endregion
		#region OrigBatchNbr
		public abstract class origBatchNbr : PX.Data.IBqlField
		{
		}
		protected String _OrigBatchNbr;
		[PXDBString(15, IsUnicode = true)]
		public virtual String OrigBatchNbr
		{
			get
			{
				return this._OrigBatchNbr;
			}
			set
			{
				this._OrigBatchNbr = value;
			}
		}
		#endregion
		#region OrigLineNbr
		public abstract class origLineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _OrigLineNbr;
		[PXDBInt()]
		public virtual Int32? OrigLineNbr
		{
			get
			{
				return this._OrigLineNbr;
			}
			set
			{
				this._OrigLineNbr = value;
			}
		}
		#endregion
		#region OrigAccountID
		public abstract class origAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _OrigAccountID;
		[Account(DisplayName = "Original Account", Visibility = PXUIVisibility.Invisible)]
		public virtual Int32? OrigAccountID
		{
			get
			{
				return this._OrigAccountID;
			}
			set
			{
				this._OrigAccountID = value;
			}
		}
		#endregion
		#region OrigSubID
		public abstract class origSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _OrigSubID;
		[SubAccount(typeof(GLTran.origAccountID), DisplayName = "Original Subaccount", Visibility = PXUIVisibility.Invisible)]
		public virtual Int32? OrigSubID
		{
			get
			{
				return this._OrigSubID;
			}
			set
			{
				this._OrigSubID = value;
			}
		}
		#endregion
		#region TranID
		public abstract class tranID : PX.Data.IBqlField
		{
		}
		protected Int32? _TranID;
		[PXDBIdentity()]
		public virtual Int32? TranID
		{
			get
			{
				return this._TranID;
			}
			set
			{
				this._TranID = value;
			}
		}
		#endregion
		#region TranType
		public abstract class tranType : PX.Data.IBqlField
		{
		}
		protected String _TranType;
		[PXDBString(3, IsFixed = true)]
		[PXDefault("")]
		public virtual String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region TranClass
		public abstract class tranClass : PX.Data.IBqlField
		{
		}
		protected String _TranClass;
		[PXDBString(1, IsFixed = true)]
		[PXDefault("N")]
		public virtual String TranClass
		{
			get
			{
				return this._TranClass;
			}
			set
			{
				this._TranClass = value;
			}
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.IBqlField
		{
		}
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true)]
		//[PXDefault(typeof(Search<GLTran.tranDesc, Where<GLTran.module, Equal<Current<GLTran.module>>, And<GLTran.batchNbr, Equal<Current<GLTran.batchNbr>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Transaction Description", Visibility = PXUIVisibility.Visible)]
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
		#region TranDate
		public abstract class tranDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _TranDate;
		[PXDBDate()]
		[PXDefault(typeof(Batch.dateEntered))]
		[PXUIField(DisplayName = "Transaction Date", Visibility = PXUIVisibility.Visible, Enabled =false)]
		public virtual DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		#region TranLineNbr
		public abstract class tranLineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _TranLineNbr;
		[PXDBInt()]
		public virtual Int32? TranLineNbr
		{
			get
			{
				return this._TranLineNbr;
			}
			set
			{
				this._TranLineNbr = value;
			}
		}
		#endregion
		#region ReferenceID
		public abstract class referenceID : PX.Data.IBqlField
		{
		}
		protected Int32? _ReferenceID;
		[PXDBInt()]
		[PXSelector(typeof(Search<BAccountR.bAccountID, Where<BAccountR.type, NotEqual<BAccountType.companyType>,
			And<BAccountR.type, NotEqual<BAccountType.prospectType>>>>), SubstituteKey = typeof(BAccountR.acctCD))]
		[PXUIField(DisplayName = "Customer/Vendor", Enabled = false, Visible = false)]		
		public virtual Int32? ReferenceID
		{
			get
			{
				return this._ReferenceID;
			}
			set
			{
				this._ReferenceID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		protected String _FinPeriodID;
		[PXDBDefault(typeof(Batch.finPeriodID))]
		[GL.FinPeriodID()]
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
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.IBqlField
		{
		}
		protected String _TranPeriodID;
		[GL.FinPeriodID(typeof(GLTran.tranDate))]
		public virtual String TranPeriodID
		{
			get
			{
				return this._TranPeriodID;
			}
			set
			{
				this._TranPeriodID = value;
			}
		}
		#endregion
		#region PostYear
		public abstract class postYear : PX.Data.IBqlField
		{
		}
		[PXString(4, IsFixed = true)]
		public virtual String PostYear
		{
			[PXDependsOnFields(typeof(finPeriodID))]
			get
			{
				return (_FinPeriodID == null) ? null : FiscalPeriodUtils.FiscalYear(this._FinPeriodID);
			}
		}
		#endregion
		#region TranYear
		public abstract class tranYear : PX.Data.IBqlField
		{
		}
		[PXString(4, IsFixed = true)]
		public virtual String TranYear
		{
			[PXDependsOnFields(typeof(tranPeriodID))]
			get
			{
				return (_TranPeriodID == null) ? null : FiscalPeriodUtils.FiscalYear(this._TranPeriodID);
			}
		}
		#endregion
		#region NextPostYear
		public abstract class nextPostYear : PX.Data.IBqlField
		{
		}
		[PXString(6, IsFixed = true)]
		public virtual String NextPostYear
		{
			[PXDependsOnFields(typeof(postYear))]
			get
			{
				return (this.PostYear == null) ? null : AutoNumberAttribute.NextNumber(this.PostYear) + "00";
			}
		}
		#endregion
		#region NextTranYear
		public abstract class nextTranYear : PX.Data.IBqlField
		{
		}
		[PXString(6, IsFixed = true)]
		public virtual String NextTranYear
		{
			[PXDependsOnFields(typeof(tranYear))]
			get
			{
				return (this.TranYear == null) ? null : AutoNumberAttribute.NextNumber(this.TranYear) + "00";
			}
		}
		#endregion
		#region CATranID
		public abstract class cATranID : PX.Data.IBqlField
		{
		}
		protected Int64? _CATranID;
		[PXDBLong()]
		[GLCashTranID()]
		public virtual Int64? CATranID
		{
			get
			{
				return this._CATranID;
			}
			set
			{
				this._CATranID = value;
			}
		}
		#endregion
		#region PMTranID
		public abstract class pMTranID : PX.Data.IBqlField
		{
		}
		protected Int64? _PMTranID;
		[PXDBChildIdentity(typeof(PMTran.tranID))]
		[PXDBLong()]
		public virtual Int64? PMTranID
		{
			get
			{
				return this._PMTranID;
			}
			set
			{
				this._PMTranID = value;
			}
		}
		#endregion
		#region OrigPMTranID
		public abstract class origPMTranID : PX.Data.IBqlField
		{
		}
		protected Int64? _OrigPMTranID;
		[PXDBLong()]
		public virtual Int64? OrigPMTranID
		{
			get
			{
				return this._OrigPMTranID;
			}
			set
			{
				this._OrigPMTranID = value;
			}
		}
		#endregion
        #region LedgerBalanceType
        public abstract class ledgerBalanceType : PX.Data.IBqlField
        {
        }
        protected string _LedgerBalanceType;
        [PXString(1, IsFixed = true, InputMask = "")]
        public virtual string LedgerBalanceType
        {
            get
            {
                return this._LedgerBalanceType;
            }
            set
            {
                this._LedgerBalanceType = value;
            }
        }
        #endregion
		#region AccountRequireUnits
		public abstract class accountRequireUnits : PX.Data.IBqlField
		{
		}
		protected Boolean? _AccountRequireUnits;
		[PXBool()]
		public virtual Boolean? AccountRequireUnits
		{
			get
			{
				return this._AccountRequireUnits;
			}
			set
			{
				this._AccountRequireUnits = value;
			}
		}
		#endregion
		#region AccountBranchID
		public abstract class accountBranchID : PX.Data.IBqlField
		{
		}
		protected Int32? _AccountBranchID;
		[PXInt()]
		public virtual Int32? AccountBranchID
		{
			get
			{
				return this._AccountBranchID;
			}
			set
			{
				this._AccountBranchID = value;
			}
		}
		#endregion
        #region TaxID
        public abstract class taxID : PX.Data.IBqlField
        {
        }
        protected String _TaxID;

        [PXDBString(TX.Tax.taxID.Length, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Tax ID")]
		[PXSelector(typeof(Search<Tax.taxID, Where2<Where<Tax.taxType, Equal<CSTaxType.sales>,
									Or<Tax.taxType, Equal<CSTaxType.vat>,
									Or<Tax.taxType, Equal<CSTaxType.use>>>>,
								And<Where2<Where<Tax.purchTaxAcctID, Equal<Current<GLTran.accountID>>,
									And<Tax.purchTaxSubID, Equal<Current<GLTran.subID>>>>,
									Or<Where<Tax.salesTaxAcctID, Equal<Current<GLTran.accountID>>,
									And<Tax.salesTaxSubID, Equal<Current<GLTran.subID>>>>>>>>>))]
		//[PXSelector(typeof(Search<Tax.taxID, Where<Tax.taxType, Equal<CSTaxType.sales>,
		//                            Or<Tax.taxType, Equal<CSTaxType.vat>,
		//                            Or<Tax.taxType, Equal<CSTaxType.use>>>>>))]
        public virtual String TaxID
        {
            get
            {
                return this._TaxID;
            }
            set
            {
                this._TaxID = value;
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
		[PXDefault(typeof(Search<Account.taxCategoryID, Where<Account.accountID, Equal<Current<GLTran.accountID>>>>),
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
