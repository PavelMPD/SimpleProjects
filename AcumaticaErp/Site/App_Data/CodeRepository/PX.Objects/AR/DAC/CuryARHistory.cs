namespace PX.Objects.AR
{
	using System;
	using PX.Data;
	using PX.Objects.CM;
	
	[System.SerializableAttribute()]
	public partial class CuryARHistory : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.IBqlField
		{
		}
		protected Int32? _BranchID;
		[PXDBInt(IsKey = true)]
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
		#region AccountID
		public abstract class accountID : PX.Data.IBqlField
		{
		}
		protected Int32? _AccountID;
		[PXDBInt(IsKey = true)]
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
		[PXDBInt(IsKey = true)]
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
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		protected String _FinPeriodID;
		[GL.FinPeriodID(IsKey=true)]
		[PXDefault()]
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
		#region CustomerID
		public abstract class customerID : PX.Data.IBqlField
		{
		}
		protected Int32? _CustomerID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
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
		#region CuryID
		public abstract class curyID : PX.Data.IBqlField
		{
		}
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, IsKey = true)]
		[PXDefault()]
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
		#region DetDeleted
		public abstract class detDeleted : PX.Data.IBqlField
		{
		}
		protected Boolean? _DetDeleted;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? DetDeleted
		{
			get
			{
				return this._DetDeleted;
			}
			set
			{
				this._DetDeleted = value;
			}
		}
		#endregion

		#region FinPtdDrAdjustments
		public abstract class finPtdDrAdjustments : PX.Data.IBqlField
		{
		}
		protected Decimal? _FinPtdDrAdjustments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinPtdDrAdjustments
		{
			get
			{
				return this._FinPtdDrAdjustments;
			}
			set
			{
				this._FinPtdDrAdjustments = value;
			}
		}
		#endregion
		#region FinPtdCrAdjustments
		public abstract class finPtdCrAdjustments : PX.Data.IBqlField
		{
		}
		protected Decimal? _FinPtdCrAdjustments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinPtdCrAdjustments
		{
			get
			{
				return this._FinPtdCrAdjustments;
			}
			set
			{
				this._FinPtdCrAdjustments = value;
			}
		}
		#endregion
		#region FinPtdSales
		public abstract class finPtdSales : PX.Data.IBqlField
		{
		}
		protected Decimal? _FinPtdSales;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? FinPtdSales
		{
			get
			{
				return this._FinPtdSales;
			}
			set
			{
				this._FinPtdSales = value;
			}
		}
		#endregion
		#region FinPtdPayments
		public abstract class finPtdPayments : PX.Data.IBqlField
		{
		}
		protected Decimal? _FinPtdPayments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? FinPtdPayments
		{
			get
			{
				return this._FinPtdPayments;
			}
			set
			{
				this._FinPtdPayments = value;
			}
		}
		#endregion
		#region FinPtdDiscounts
		public abstract class finPtdDiscounts : PX.Data.IBqlField
		{
		}
		protected Decimal? _FinPtdDiscounts;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? FinPtdDiscounts
		{
			get
			{
				return this._FinPtdDiscounts;
			}
			set
			{
				this._FinPtdDiscounts = value;
			}
		}
		#endregion
		#region FinYtdBalance
		public abstract class finYtdBalance : PX.Data.IBqlField
		{
		}
		protected Decimal? _FinYtdBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinYtdBalance
		{
			get
			{
				return this._FinYtdBalance;
			}
			set
			{
				this._FinYtdBalance = value;
			}
		}
		#endregion
		#region FinBegBalance
		public abstract class finBegBalance : PX.Data.IBqlField
		{
		}
		protected Decimal? _FinBegBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinBegBalance
		{
			get
			{
				return this._FinBegBalance;
			}
			set
			{
				this._FinBegBalance = value;
			}
		}
		#endregion
		#region FinPtdCOGS
		public abstract class finPtdCOGS : PX.Data.IBqlField
		{
		}
		protected Decimal? _FinPtdCOGS;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? FinPtdCOGS
		{
			get
			{
				return this._FinPtdCOGS;
			}
			set
			{
				this._FinPtdCOGS = value;
			}
		}
		#endregion
		#region FinPtdRGOL
		public abstract class finPtdRGOL : PX.Data.IBqlField
		{
		}
		protected Decimal? _FinPtdRGOL;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? FinPtdRGOL
		{
			get
			{
				return this._FinPtdRGOL;
			}
			set
			{
				this._FinPtdRGOL = value;
			}
		}
		#endregion
		#region FinPtdFinCharges
		public abstract class finPtdFinCharges : PX.Data.IBqlField
		{
		}
		protected Decimal? _FinPtdFinCharges;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? FinPtdFinCharges
		{
			get
			{
				return this._FinPtdFinCharges;
			}
			set
			{
				this._FinPtdFinCharges = value;
			}
		}
		#endregion
		#region FinPtdRevalued
		public abstract class finPtdRevalued : PX.Data.IBqlField
		{
		}
		protected Decimal? _FinPtdRevalued;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinPtdRevalued
		{
			get
			{
				return this._FinPtdRevalued;
			}
			set
			{
				this._FinPtdRevalued = value;
			}
		}
		#endregion
		#region FinPtdDeposits
		public abstract class finPtdDeposits : PX.Data.IBqlField
		{
		}
		protected Decimal? _FinPtdDeposits;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinPtdDeposits
		{
			get
			{
				return this._FinPtdDeposits;
			}
			set
			{
				this._FinPtdDeposits = value;
			}
		}
		#endregion
		#region FinYtdDeposits
		public abstract class finYtdDeposits : PX.Data.IBqlField
		{
		}
		protected Decimal? _FinYtdDeposits;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinYtdDeposits
		{
			get
			{
				return this._FinYtdDeposits;
			}
			set
			{
				this._FinYtdDeposits = value;
			}
		}
		#endregion
		
		#region TranPtdDrAdjustments
		public abstract class tranPtdDrAdjustments : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranPtdDrAdjustments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranPtdDrAdjustments
		{
			get
			{
				return this._TranPtdDrAdjustments;
			}
			set
			{
				this._TranPtdDrAdjustments = value;
			}
		}
		#endregion
		#region TranPtdCrAdjustments
		public abstract class tranPtdCrAdjustments : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranPtdCrAdjustments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranPtdCrAdjustments
		{
			get
			{
				return this._TranPtdCrAdjustments;
			}
			set
			{
				this._TranPtdCrAdjustments = value;
			}
		}
		#endregion
		#region TranPtdSales
		public abstract class tranPtdSales : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranPtdSales;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? TranPtdSales
		{
			get
			{
				return this._TranPtdSales;
			}
			set
			{
				this._TranPtdSales = value;
			}
		}
		#endregion
		#region TranPtdPayments
		public abstract class tranPtdPayments : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranPtdPayments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? TranPtdPayments
		{
			get
			{
				return this._TranPtdPayments;
			}
			set
			{
				this._TranPtdPayments = value;
			}
		}
		#endregion
		#region TranPtdDiscounts
		public abstract class tranPtdDiscounts : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranPtdDiscounts;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? TranPtdDiscounts
		{
			get
			{
				return this._TranPtdDiscounts;
			}
			set
			{
				this._TranPtdDiscounts = value;
			}
		}
		#endregion
		#region TranYtdBalance
		public abstract class tranYtdBalance : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranYtdBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranYtdBalance
		{
			get
			{
				return this._TranYtdBalance;
			}
			set
			{
				this._TranYtdBalance = value;
			}
		}
		#endregion
		#region TranBegBalance
		public abstract class tranBegBalance : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranBegBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranBegBalance
		{
			get
			{
				return this._TranBegBalance;
			}
			set
			{
				this._TranBegBalance = value;
			}
		}
		#endregion
		#region TranPtdRGOL
		public abstract class tranPtdRGOL : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranPtdRGOL;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? TranPtdRGOL
		{
			get
			{
				return this._TranPtdRGOL;
			}
			set
			{
				this._TranPtdRGOL = value;
			}
		}
		#endregion
		#region TranPtdCOGS
		public abstract class tranPtdCOGS : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranPtdCOGS;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? TranPtdCOGS
		{
			get
			{
				return this._TranPtdCOGS;
			}
			set
			{
				this._TranPtdCOGS = value;
			}
		}
		#endregion
		#region TranPtdFinCharges
		public abstract class tranPtdFinCharges : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranPtdFinCharges;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? TranPtdFinCharges
		{
			get
			{
				return this._TranPtdFinCharges;
			}
			set
			{
				this._TranPtdFinCharges = value;
			}
		}
		#endregion
		#region TranPtdDeposits
		public abstract class tranPtdDeposits : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranPtdDeposits;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranPtdDeposits
		{
			get
			{
				return this._TranPtdDeposits;
			}
			set
			{
				this._TranPtdDeposits = value;
			}
		}
		#endregion
		#region TranYtdDeposits
		public abstract class tranYtdDeposits : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranYtdDeposits;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranYtdDeposits
		{
			get
			{
				return this._TranYtdDeposits;
			}
			set
			{
				this._TranYtdDeposits = value;
			}
		}
		#endregion


		#region CuryFinPtdDrAdjustments
		public abstract class curyFinPtdDrAdjustments : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryFinPtdDrAdjustments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFinPtdDrAdjustments
		{
			get
			{
				return this._CuryFinPtdDrAdjustments;
			}
			set
			{
				this._CuryFinPtdDrAdjustments = value;
			}
		}
		#endregion
		#region CuryFinPtdCrAdjustments
		public abstract class curyFinPtdCrAdjustments : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryFinPtdCrAdjustments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFinPtdCrAdjustments
		{
			get
			{
				return this._CuryFinPtdCrAdjustments;
			}
			set
			{
				this._CuryFinPtdCrAdjustments = value;
			}
		}
		#endregion
		#region CuryFinPtdSales
		public abstract class curyFinPtdSales : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryFinPtdSales;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryFinPtdSales
		{
			get
			{
				return this._CuryFinPtdSales;
			}
			set
			{
				this._CuryFinPtdSales = value;
			}
		}
		#endregion
		#region CuryFinPtdPayments
		public abstract class curyFinPtdPayments : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryFinPtdPayments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryFinPtdPayments
		{
			get
			{
				return this._CuryFinPtdPayments;
			}
			set
			{
				this._CuryFinPtdPayments = value;
			}
		}
		#endregion
		#region CuryFinPtdDiscounts
		public abstract class curyFinPtdDiscounts : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryFinPtdDiscounts;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryFinPtdDiscounts
		{
			get
			{
				return this._CuryFinPtdDiscounts;
			}
			set
			{
				this._CuryFinPtdDiscounts = value;
			}
		}
		#endregion
		#region CuryFinPtdFinCharges
		public abstract class curyFinPtdFinCharges : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryFinPtdFinCharges;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFinPtdFinCharges
		{
			get
			{
				return this._CuryFinPtdFinCharges;
			}
			set
			{
				this._CuryFinPtdFinCharges = value;
			}
		}
		#endregion
		#region CuryFinYtdBalance
		public abstract class curyFinYtdBalance : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryFinYtdBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryFinYtdBalance
		{
			get
			{
				return this._CuryFinYtdBalance;
			}
			set
			{
				this._CuryFinYtdBalance = value;
			}
		}
		#endregion
		#region CuryFinBegBalance
		public abstract class curyFinBegBalance : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryFinBegBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFinBegBalance
		{
			get
			{
				return this._CuryFinBegBalance;
			}
			set
			{
				this._CuryFinBegBalance = value;
			}
		}
		#endregion
		#region CuryFinPtdDeposits
		public abstract class curyFinPtdDeposits : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryFinPtdDeposits;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFinPtdDeposits
		{
			get
			{
				return this._CuryFinPtdDeposits;
			}
			set
			{
				this._CuryFinPtdDeposits = value;
			}
		}
		#endregion
		#region CuryFinYtdDeposits
		public abstract class curyFinYtdDeposits : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryFinYtdDeposits;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFinYtdDeposits
		{
			get
			{
				return this._CuryFinYtdDeposits;
			}
			set
			{
				this._CuryFinYtdDeposits = value;
			}
		}
		#endregion


		#region CuryTranPtdDrAdjustments
		public abstract class curyTranPtdDrAdjustments : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTranPtdDrAdjustments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranPtdDrAdjustments
		{
			get
			{
				return this._CuryTranPtdDrAdjustments;
			}
			set
			{
				this._CuryTranPtdDrAdjustments = value;
			}
		}
		#endregion
		#region CuryTranPtdCrAdjustments
		public abstract class curyTranPtdCrAdjustments : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTranPtdCrAdjustments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranPtdCrAdjustments
		{
			get
			{
				return this._CuryTranPtdCrAdjustments;
			}
			set
			{
				this._CuryTranPtdCrAdjustments = value;
			}
		}
		#endregion
		#region CuryTranPtdSales
		public abstract class curyTranPtdSales : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTranPtdSales;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryTranPtdSales
		{
			get
			{
				return this._CuryTranPtdSales;
			}
			set
			{
				this._CuryTranPtdSales = value;
			}
		}
		#endregion
		#region CuryTranPtdPayments
		public abstract class curyTranPtdPayments : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTranPtdPayments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryTranPtdPayments
		{
			get
			{
				return this._CuryTranPtdPayments;
			}
			set
			{
				this._CuryTranPtdPayments = value;
			}
		}
		#endregion
		#region CuryTranPtdDiscounts
		public abstract class curyTranPtdDiscounts : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTranPtdDiscounts;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryTranPtdDiscounts
		{
			get
			{
				return this._CuryTranPtdDiscounts;
			}
			set
			{
				this._CuryTranPtdDiscounts = value;
			}
		}
		#endregion
		#region CuryTranPtdFinCharges
		public abstract class curyTranPtdFinCharges : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTranPtdFinCharges;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranPtdFinCharges
		{
			get
			{
				return this._CuryTranPtdFinCharges;
			}
			set
			{
				this._CuryTranPtdFinCharges = value;
			}
		}
		#endregion
		#region CuryTranYtdBalance
		public abstract class curyTranYtdBalance : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTranYtdBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryTranYtdBalance
		{
			get
			{
				return this._CuryTranYtdBalance;
			}
			set
			{
				this._CuryTranYtdBalance = value;
			}
		}
		#endregion
		#region CuryTranBegBalance
		public abstract class curyTranBegBalance : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTranBegBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranBegBalance
		{
			get
			{
				return this._CuryTranBegBalance;
			}
			set
			{
				this._CuryTranBegBalance = value;
			}
		}
		#endregion
		#region CuryTranPtdDeposits
		public abstract class curyTranPtdDeposits : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTranPtdDeposits;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranPtdDeposits
		{
			get
			{
				return this._CuryTranPtdDeposits;
			}
			set
			{
				this._CuryTranPtdDeposits = value;
			}
		}
		#endregion
		#region CuryTranYtdDeposits
		public abstract class curyTranYtdDeposits : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTranYtdDeposits;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranYtdDeposits
		{
			get
			{
				return this._CuryTranYtdDeposits;
			}
			set
			{
				this._CuryTranYtdDeposits = value;
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
		#region FinFlag
		public abstract class finFlag : IBqlField { }
        protected Boolean? _FinFlag = true;
		[PXBool()]
        public virtual Boolean? FinFlag
		{
			get
			{
				return this._FinFlag;
			}
			set
			{
				this._FinFlag = value;
			}
		}
		#endregion

		#region PtdCrAdjustments
		[PXDecimal(4)]
		public virtual Decimal? PtdCrAdjustments
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finPtdCrAdjustments),typeof(tranPtdCrAdjustments))]
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdCrAdjustments : this._TranPtdCrAdjustments;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdCrAdjustments = value;
				}
				else
				{
					this._TranPtdCrAdjustments = value;
				}
			}
		}
		#endregion
		#region PtdDrAdjustments
		[PXDecimal(4)]
		public virtual Decimal? PtdDrAdjustments
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finPtdDrAdjustments),typeof(tranPtdDrAdjustments))]
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdDrAdjustments : this._TranPtdDrAdjustments;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdDrAdjustments = value;
				}
				else
				{
					this._TranPtdDrAdjustments = value;
				}
			}
		}
		#endregion
		#region PtdSales
		[PXDecimal(4)]
		public virtual Decimal? PtdSales
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finPtdSales),typeof(tranPtdSales))]
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdSales : this._TranPtdSales;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdSales = value;
				}
				else
				{
					this._TranPtdSales = value;
				}
			}
		}
		#endregion
		#region PtdPayments
		[PXDecimal(4)]
		public virtual Decimal? PtdPayments
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finPtdPayments),typeof(tranPtdPayments))]
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdPayments : this._TranPtdPayments;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdPayments = value;
				}
				else
				{
					this._TranPtdPayments = value;
				}
			}
		}
		#endregion
		#region PtdDiscounts
		[PXDecimal(4)]
		public virtual Decimal? PtdDiscounts
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finPtdDiscounts),typeof(tranPtdDiscounts))]
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdDiscounts : this._TranPtdDiscounts;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdDiscounts = value;
				}
				else
				{
					this._TranPtdDiscounts = value;
				}
			}
		}
		#endregion
		#region YtdBalance
		[PXDecimal(4)]
		public virtual Decimal? YtdBalance
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finYtdBalance),typeof(tranYtdBalance))]
			get
			{
				return ((bool)_FinFlag) ? this._FinYtdBalance : this._TranYtdBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinYtdBalance = value;
				}
				else
				{
					this._TranYtdBalance = value;
				}
			}
		}
		#endregion
		#region BegBalance
		[PXDecimal(4)]
		public virtual Decimal? BegBalance
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finBegBalance),typeof(tranBegBalance))]
			get
			{
				return ((bool)_FinFlag) ? this._FinBegBalance : this._TranBegBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinBegBalance = value;
				}
				else
				{
					this._TranBegBalance = value;
				}
			}
		}
		#endregion
		#region PtdCOGS
		[PXDecimal(4)]
		public virtual Decimal? PtdCOGS
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finPtdCOGS),typeof(tranPtdCOGS))]
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdCOGS : this._TranPtdCOGS;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdCOGS = value;
				}
				else
				{
					this._TranPtdCOGS = value;
				}
			}
		}
		#endregion
		#region PtdRGOL
		[PXDecimal(4)]
		public virtual Decimal? PtdRGOL
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finPtdRGOL),typeof(tranPtdRGOL))]
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdRGOL : this._TranPtdRGOL;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdRGOL = value;
				}
				else
				{
					this._TranPtdRGOL = value;
				}
			}
		}
		#endregion
		#region PtdFinCharges
		[PXDecimal(4)]
		public virtual Decimal? PtdFinCharges
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finPtdFinCharges),typeof(tranPtdFinCharges))]
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdFinCharges : this._TranPtdFinCharges;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdFinCharges = value;
				}
				else
				{
					this._TranPtdFinCharges = value;
				}
			}
		}
		#endregion
		#region PtdDeposits
		[PXDecimal(4)]
		public virtual Decimal? PtdDeposits
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finPtdDeposits),typeof(tranPtdDeposits))]
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdDeposits : this._TranPtdDeposits;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdDeposits = value;
				}
				else
				{
					this._TranPtdDeposits = value;
				}
			}
		}
		#endregion
		#region YtdDeposits
		[PXDecimal(4)]
		public virtual Decimal? YtdDeposits
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finYtdDeposits),typeof(tranYtdDeposits))]
			get
			{
				return ((bool)_FinFlag) ? this._FinYtdDeposits : this._TranYtdDeposits;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinYtdDeposits = value;
				}
				else
				{
					this._TranYtdDeposits = value;
				}
			}
		}
		#endregion
        #region PtdItemDiscounts
        [PXDecimal(4)]
        public virtual Decimal? PtdItemDiscounts
        {
            get;
            set;
        }
        #endregion

		#region CuryPtdCrAdjustments
		[PXDecimal(4)]
		public virtual Decimal? CuryPtdCrAdjustments
		{
			[PXDependsOnFields(typeof(finFlag),typeof(curyFinPtdCrAdjustments),typeof(curyTranPtdCrAdjustments))]
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdCrAdjustments : this._CuryTranPtdCrAdjustments;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdCrAdjustments = value;
				}
				else
				{
					this._CuryTranPtdCrAdjustments = value;
				}
			}
		}
		#endregion
		#region CuryPtdDrAdjustments
		[PXDecimal(4)]
		public virtual Decimal? CuryPtdDrAdjustments
		{
			[PXDependsOnFields(typeof(finFlag),typeof(curyFinPtdDrAdjustments),typeof(curyTranPtdDrAdjustments))]
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdDrAdjustments : this._CuryTranPtdDrAdjustments;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdDrAdjustments = value;
				}
				else
				{
					this._CuryTranPtdDrAdjustments = value;
				}
			}
		}
		#endregion
		#region CuryPtdSales
		[PXDecimal(4)]
		public virtual Decimal? CuryPtdSales
		{
			[PXDependsOnFields(typeof(finFlag),typeof(curyFinPtdSales),typeof(curyTranPtdSales))]
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdSales : this._CuryTranPtdSales;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdSales = value;
				}
				else
				{
					this._CuryTranPtdSales = value;
				}
			}
		}
		#endregion
		#region CuryPtdPayments
		[PXDecimal(4)]
		public virtual Decimal? CuryPtdPayments
		{
			[PXDependsOnFields(typeof(finFlag),typeof(curyFinPtdPayments),typeof(curyTranPtdPayments))]
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdPayments : this._CuryTranPtdPayments;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdPayments = value;
				}
				else
				{
					this._CuryTranPtdPayments = value;
				}
			}
		}
		#endregion
		#region CuryPtdDiscounts
		[PXDecimal(4)]
		public virtual Decimal? CuryPtdDiscounts
		{
			[PXDependsOnFields(typeof(finFlag),typeof(curyFinPtdDiscounts),typeof(curyTranPtdDiscounts))]
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdDiscounts : this._CuryTranPtdDiscounts;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdDiscounts = value;
				}
				else
				{
					this._CuryTranPtdDiscounts = value;
				}
			}
		}
		#endregion
		#region CuryPtdFinCharges
		[PXDecimal(4)]
		public virtual Decimal? CuryPtdFinCharges
		{
			[PXDependsOnFields(typeof(finFlag),typeof(curyFinPtdFinCharges),typeof(curyTranPtdFinCharges))]
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdFinCharges : this._CuryTranPtdFinCharges;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdFinCharges = value;
				}
				else
				{
					this._CuryTranPtdFinCharges = value;
				}
			}
		}
		#endregion
		#region CuryYtdBalance
		[PXDecimal(4)]
		public virtual Decimal? CuryYtdBalance
		{
			[PXDependsOnFields(typeof(finFlag),typeof(curyFinYtdBalance),typeof(curyTranYtdBalance))]
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinYtdBalance : this._CuryTranYtdBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinYtdBalance = value;
				}
				else
				{
					this._CuryTranYtdBalance = value;
				}
			}
		}
		#endregion
		#region CuryBegBalance
		[PXDecimal(4)]
		public virtual Decimal? CuryBegBalance
		{
			[PXDependsOnFields(typeof(finFlag),typeof(curyFinBegBalance),typeof(curyTranBegBalance))]
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinBegBalance : this._CuryTranBegBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinBegBalance = value;
				}
				else
				{
					this._CuryTranBegBalance = value;
				}
			}
		}
		#endregion
		#region CuryPtdDeposits
		[PXDecimal(4)]
		public virtual Decimal? CuryPtdDeposits
		{
			[PXDependsOnFields(typeof(finFlag),typeof(curyFinPtdDeposits),typeof(curyTranPtdDeposits))]
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdDeposits : this._CuryTranPtdDeposits;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdDeposits = value;
				}
				else
				{
					this._CuryTranPtdDeposits = value;
				}
			}
		}
		#endregion
		#region CuryYtdDeposits
		[PXDecimal(4)]
		public virtual Decimal? CuryYtdDeposits
		{
			[PXDependsOnFields(typeof(finFlag),typeof(curyFinYtdDeposits),typeof(curyTranYtdDeposits))]
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinYtdDeposits : this._CuryTranYtdDeposits;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinYtdDeposits = value;
				}
				else
				{
					this._CuryTranYtdDeposits = value;
				}
			}
		}
		#endregion
	}
}
