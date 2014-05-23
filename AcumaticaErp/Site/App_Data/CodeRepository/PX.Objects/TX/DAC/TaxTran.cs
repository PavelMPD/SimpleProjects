namespace PX.Objects.TX
{
	using System;
	using PX.Data;
	using PX.Objects.GL;
	using PX.Objects.CM;
	using PX.Objects.CS;

	[System.SerializableAttribute()]
	public partial class TaxTran : TaxDetail, PX.Data.IBqlTable
	{
		#region Selected
		public abstract class selected : IBqlField
		{
		}
		protected bool? _Selected = false;
		[PXBool]
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
		#region Module
		public abstract class module : PX.Data.IBqlField
		{
		}
		protected String _Module;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault(BatchModule.GL)]
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
		#region TranType
		public abstract class tranType : PX.Data.IBqlField
		{
		}
		protected String _TranType;
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(TaxAdjustment.docType))]
		[PXParent(typeof(Select<TaxAdjustment, Where<TaxAdjustment.docType, Equal<Current<TaxTran.tranType>>, And<TaxAdjustment.refNbr, Equal<Current<TaxTran.refNbr>>>>>))]
		[TaxAdjustmentType.List()]
		[PXUIField(DisplayName="Tran. Type")]
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
		#region RefNbr
		public abstract class refNbr : PX.Data.IBqlField
		{
		}
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(TaxAdjustment.refNbr))]
		[PXUIField(DisplayName = "Ref. Nbr.")]
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
        #region LineRefNbr
        public abstract class lineRefNbr : PX.Data.IBqlField
        {
        }
        protected String _LineRefNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXUIField(DisplayName = "Line Ref. Number")]
        [PXDefault("")]
        public virtual String LineRefNbr
        {
            get
            {
                return this._LineRefNbr;
            }
            set
            {
                this._LineRefNbr = value;
            }
        }
        #endregion
		#region OrigTranType
		public abstract class origTranType : PX.Data.IBqlField
		{
		}
		protected String _OrigTranType;
		[PXDBString(3, IsFixed = true, IsKey = true)]
		[PXUIField(DisplayName = "Orig. Tran. Type")]
		[PXDefault("")]
		public virtual String OrigTranType
		{
			get
			{
				return this._OrigTranType;
			}
			set
			{
				this._OrigTranType = value;
			}
		}
		#endregion
		#region OrigRefNbr
		public abstract class origRefNbr : PX.Data.IBqlField
		{
		}
		protected String _OrigRefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Orig. Doc. Number")]
		[PXDefault("")]
		public virtual String OrigRefNbr
		{
			get
			{
				return this._OrigRefNbr;
			}
			set
			{
				this._OrigRefNbr = value;
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
		#region Voided
		public abstract class voided : PX.Data.IBqlField
		{
		}
		protected Boolean? _Voided;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Voided
		{
			get
			{
				return this._Voided;
			}
			set
			{
				this._Voided = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		protected String _FinPeriodID;
		[GL.FinPeriodID()]
		[PXDBDefault(typeof(TaxAdjustment.finPeriodID))]
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
		#region FinDate
		public abstract class finDate : PX.Data.IBqlField
		{
		}
		[PXDBDate()]
		[PXDBDefault(typeof(Search<FinPeriod.finDate, Where<FinPeriod.finPeriodID, Equal<Current<TaxTran.finPeriodID>>>>))]
		public virtual DateTime? FinDate
		{
			get;
			set;
		}
		#endregion
		#region TaxPeriodID
		public abstract class taxPeriodID : PX.Data.IBqlField
		{
		}
		protected String _TaxPeriodID;
		[GL.FinPeriodID()]
		[PXDBDefault(typeof(TaxAdjustment.taxPeriod))]
		public virtual String TaxPeriodID
		{
			get
			{
				return this._TaxPeriodID;
			}
			set
			{
				this._TaxPeriodID = value;
			}
		}
		#endregion
		#region TaxID
		public abstract class taxID : PX.Data.IBqlField
		{
		}
		[PXDBString(Tax.taxID.Length, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Tax ID", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<Tax.taxID,Where<Tax.taxVendorID,Equal<Current<TaxAdjustment.vendorID>>>>))]
		public override String TaxID
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
		#region BranchID
		public abstract class branchID : PX.Data.IBqlField
		{
		}
		protected Int32? _BranchID;
		[Branch(typeof(TaxAdjustment.branchID), Enabled = false)]
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
		#region VendorID
		public abstract class vendorID : PX.Data.IBqlField
		{
		}
		protected Int32? _VendorID;
        [PXDBInt()]
        [PXDBDefault(typeof(TaxAdjustment.vendorID))]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region RevisionID
		public abstract class revisionID : PX.Data.IBqlField
		{
		}
		protected Int32? _RevisionID;
		[PXDBInt()]
		public virtual Int32? RevisionID
		{
			get
			{
				return this._RevisionID;
			}
			set
			{
				this._RevisionID = value;
			}
		}
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _BAccountID;
		[PXDBInt]
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
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.IBqlField
		{
		}
		protected String _TaxZoneID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName="Tax Zone")]
		[PXSelector(typeof(Search<TaxZone.taxZoneID>))]
		[PXDefault()]
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
		#region AccountID
		public abstract class accountID : PX.Data.IBqlField
		{
		}
		protected Int32? _AccountID;
		[Account(DisplayName = "Account", Visibility = PXUIVisibility.Visible)]
		[PXDefault()]
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
		[SubAccount(typeof(TaxTran.accountID), DisplayName = "Sub.", Visibility = PXUIVisibility.Visible)]
		[PXDefault()]
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
		#region TranDate
		public abstract class tranDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _TranDate;
		[PXDBDate()]
		[PXDBDefault(typeof(TaxAdjustment.docDate))]
		[PXUIField(DisplayName = "Tran. Date")]
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
		#region TaxInvoiceNbr
		public abstract class taxInvoiceNbr : PX.Data.IBqlField
		{
		}
		protected String _TaxInvoiceNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Invoice Nbr.")]
		public virtual String TaxInvoiceNbr
		{
			get
			{
				return this._TaxInvoiceNbr;
			}
			set
			{
				this._TaxInvoiceNbr = value;
			}
		}
		#endregion
		#region TaxInvoiceDate
		public abstract class taxInvoiceDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _TaxInvoiceDate;
		[PXDBDate(InputMask = "d", DisplayMask = "d")]
		[PXUIField(DisplayName = "Tax Invoice Date")]
		public virtual DateTime? TaxInvoiceDate
		{
			get
			{
				return this._TaxInvoiceDate;
			}
			set
			{
				this._TaxInvoiceDate = value;
			}
		}
		#endregion
		#region TaxType
		public abstract class taxType : PX.Data.IBqlField
		{
		}
		protected String _TaxType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault()]
		public virtual String TaxType
		{
			get
			{
				return this._TaxType;
			}
			set
			{
				this._TaxType = value;
			}
		}
		#endregion
		#region TaxBucketID
		public abstract class taxBucketID : PX.Data.IBqlField
		{
		}
		protected Int32? _TaxBucketID;
		[PXDBInt()]
		[PXDefault()]
		public virtual Int32? TaxBucketID
		{
			get
			{
				return this._TaxBucketID;
			}
			set
			{
				this._TaxBucketID = value;
			}
		}
		#endregion
		#region TaxRate
		public abstract class taxRate : PX.Data.IBqlField
		{
		}
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Rate", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public override Decimal? TaxRate
		{
			get
			{
				return this._TaxRate;
			}
			set
			{
				this._TaxRate = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		[PXDBLong()]
		[CurrencyInfo(typeof(TaxAdjustment.curyInfoID))]
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
		#region CuryTaxableAmt
		public abstract class curyTaxableAmt : PX.Data.IBqlField
		{
		}
		[PXDBCurrency(typeof(TaxTran.curyInfoID), typeof(TaxTran.taxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public override decimal? CuryTaxableAmt
		{
			get
			{
				return this._CuryTaxableAmt;
			}
			set
			{
				this._CuryTaxableAmt = value;
			}
		}
		#endregion
		#region TaxableAmt
		public abstract class taxableAmt : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryTaxAmt
		public abstract class curyTaxAmt : PX.Data.IBqlField
		{
		}
		[PXDBCurrency(typeof(TaxTran.curyInfoID),typeof(TaxTran.taxAmt))]
		[PXFormula(typeof(Mult<TaxTran.curyTaxableAmt, Div<TaxTran.taxRate, decimal100>>), typeof(SumCalc<TaxAdjustment.curyDocBal>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
		public override decimal? CuryTaxAmt
		{
			get
			{
				return this._CuryTaxAmt;
			}
			set
			{
				this._CuryTaxAmt = value;
			}
		}
		#endregion
		#region TaxAmt
		public abstract class taxAmt : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.IBqlField
		{
		}
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency")]
		[PXSelector(typeof(Currency.curyID))]
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
		#region ReportCuryID
		public abstract class reportCuryID : PX.Data.IBqlField
		{
		}
		protected String _ReportCuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Report Currency")]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String ReportCuryID
		{
			get
			{
				return this._ReportCuryID;
			}
			set
			{
				this._ReportCuryID = value;
			}
		}
		#endregion
		#region ReportCuryRateTypeID
		public abstract class reportCuryRateTypeID : PX.Data.IBqlField
		{
		}
		protected String _ReportCuryRateTypeID;
		[PXDBString(6, IsUnicode = true)]
		[PXUIField(DisplayName = "Report Currency Rate Type", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID), DescriptionField = typeof(CurrencyRateType.descr))]
		public virtual String ReportCuryRateTypeID
		{
			get
			{
				return this._ReportCuryRateTypeID;
			}
			set
			{
				this._ReportCuryRateTypeID = value;
			}
		}
		#endregion
		#region ReportCuryEffDate
		public abstract class reportCuryEffDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _ReportCuryEffDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Report Effective Date")]
		public virtual DateTime? ReportCuryEffDate
		{
			get
			{
				return this._ReportCuryEffDate;
			}
			set
			{
				this._ReportCuryEffDate = value;
			}
		}
		#endregion
		#region ReportCuryMultDiv
		public abstract class reportCuryMultDiv : PX.Data.IBqlField
		{
		}
		protected String _ReportCuryMultDiv;
		[PXDBString(1, IsFixed = true)]
		[PXDefault("M", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Report Mult Div")]
		public virtual String ReportCuryMultDiv
		{
			get
			{
				return this._ReportCuryMultDiv;
			}
			set
			{
				this._ReportCuryMultDiv = value;
			}
		}
		#endregion
		#region ReportCuryRate
		public abstract class reportCuryRate : PX.Data.IBqlField
		{
		}
		protected Decimal? _ReportCuryRate;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "1.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? ReportCuryRate
		{
			get
			{
				return this._ReportCuryRate;
			}
			set
			{
				this._ReportCuryRate = value;
			}
		}
		#endregion
		#region ReportTaxableAmt
		public abstract class reportTaxableAmt : IBqlField
		{
		}
		protected Decimal? _ReportTaxableAmt;
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDecimal]
		[PXUIField(DisplayName = "Report Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? ReportTaxableAmt
		{
			get
			{
				return this._ReportTaxableAmt;
			}
			set
			{
				this._ReportTaxableAmt = value;
			}
		}
		#endregion		
		#region ReportTaxAmt
		public abstract class reportTaxAmt : IBqlField
		{
		}
		protected Decimal? _ReportTaxAmt;
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDecimal]
		[PXUIField(DisplayName = "Report Tax Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? ReportTaxAmt
		{
			get
			{
				return this._ReportTaxAmt;
			}
			set
			{
				this._ReportTaxAmt = value;
			}
		}
		#endregion
		#region NonDeductibleTaxRate
		public abstract class nonDeductibleTaxRate : PX.Data.IBqlField
		{
		}
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Deductible Tax Rate", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public override Decimal? NonDeductibleTaxRate { get; set; }
		#endregion
		#region ExpenseAmt
		public abstract class expenseAmt : PX.Data.IBqlField
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expense Amount", Visibility = PXUIVisibility.Visible)]
		public override Decimal? ExpenseAmt { get; set; }
		#endregion
		#region CuryExpenseAmt
		public abstract class curyExpenseAmt : PX.Data.IBqlField
		{
		}
		[PXDBCurrency(typeof(TaxTran.curyInfoID), typeof(TaxTran.expenseAmt))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expense Amount", Visibility = PXUIVisibility.Visible)]
		public override Decimal? CuryExpenseAmt { get; set; }
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
	}
}
