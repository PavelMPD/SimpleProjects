namespace PX.Objects.SO
{
	using System;
	using PX.Data;
	using PX.Objects.CM;
	using PX.Objects.TX;
	using PX.Objects.CS;
	
	[System.SerializableAttribute()]
	public partial class SOTax : TaxDetail, PX.Data.IBqlTable
	{
		#region OrderType
		public abstract class orderType : PX.Data.IBqlField
		{
		}
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault(typeof(SOOrder.orderType))]
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.IBqlField
		{
		}
		protected String _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDBDefault(typeof(SOOrder.orderNbr), DefaultForUpdate = false)]
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
		#region BONbr
		public abstract class bONbr : PX.Data.IBqlField
		{
		}
		protected Int16? _BONbr;
		[PXDBShort(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Back Order", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Int16? BONbr
		{
			get
			{
				return this._BONbr;
			}
			set
			{
				this._BONbr = value;
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
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(Select<SOLine, Where<SOLine.orderType, Equal<Current<SOTax.orderType>>, And<SOLine.orderNbr, Equal<Current<SOTax.orderNbr>>, And<SOLine.lineNbr, Equal<Current<SOTax.lineNbr>>>>>>))]
		[PXParent(typeof(Select<SOOrder, Where<SOOrder.orderType, Equal<Current<SOTax.orderType>>, And<SOOrder.orderNbr, Equal<Current<SOTax.orderNbr>>, And<Current<SOTax.lineNbr>, Equal<int32000>>>>>))]
		[PXParent(typeof(Select<SOLine2, Where<SOLine2.orderType, Equal<Current<SOTax.orderType>>, And<SOLine2.orderNbr, Equal<Current<SOTax.orderNbr>>, And<SOLine2.lineNbr, Equal<Current<SOTax.lineNbr>>>>>>))]
		[PXParent(typeof(Select<SOLine4, Where<SOLine4.orderType, Equal<Current<SOTax.orderType>>, And<SOLine4.orderNbr, Equal<Current<SOTax.orderNbr>>, And<SOLine4.lineNbr, Equal<Current<SOTax.lineNbr>>>>>>))]
		[PXParent(typeof(Select<SOMiscLine2, Where<SOMiscLine2.orderType, Equal<Current<SOTax.orderType>>, And<SOMiscLine2.orderNbr, Equal<Current<SOTax.orderNbr>>, And<SOMiscLine2.lineNbr, Equal<Current<SOTax.lineNbr>>>>>>))]
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
		#region TaxID
		public abstract class taxID : PX.Data.IBqlField
		{
		}
		[PXDBString(Tax.taxID.Length, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Tax ID")]
		[PXSelector(typeof(Tax.taxID))]
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
		#region TaxRate
		public abstract class taxRate : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		[PXDBLong()]
		[CurrencyInfo(typeof(SOOrder.curyInfoID))]
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
		[PXDBCurrency(typeof(SOTax.curyInfoID), typeof(SOTax.taxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public override Decimal? CuryTaxableAmt
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
		[PXDBCurrency(typeof(SOTax.curyInfoID), typeof(SOTax.taxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
		public override Decimal? CuryTaxAmt
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
	}
}
