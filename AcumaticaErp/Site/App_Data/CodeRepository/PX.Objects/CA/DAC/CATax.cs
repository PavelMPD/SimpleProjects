using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.TX;

namespace PX.Objects.CA
{
	[System.SerializableAttribute()]
	public partial class CATax : TaxDetail, PX.Data.IBqlTable
	{
		#region AdjTranType
		public abstract class adjTranType : PX.Data.IBqlField
		{
		}
		protected String _AdjTranType;
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(CAAdj.adjTranType))]
		[PXUIField(DisplayName = "Tran. Type", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual String AdjTranType
		{
			get
			{
				return this._AdjTranType;
			}
			set
			{
				this._AdjTranType = value;
			}
		}
		#endregion
		#region AdjRefNbr
		public abstract class adjRefNbr : PX.Data.IBqlField
		{
		}
		protected String _AdjRefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(CAAdj.adjRefNbr))]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual String AdjRefNbr
		{
			get
			{
				return this._AdjRefNbr;
			}
			set
			{
				this._AdjRefNbr = value;
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
		[PXParent(typeof(Select<CASplit, Where<CASplit.adjTranType, Equal<Current<CATax.adjTranType>>, And<CASplit.adjRefNbr, Equal<Current<CATax.adjRefNbr>>, And<CASplit.lineNbr, Equal<Current<CATax.lineNbr>>>>>>))]
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
		[CurrencyInfo(typeof(CAAdj.curyInfoID))]
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
		[PXDBCurrency(typeof(CATax.curyInfoID), typeof(CATax.taxableAmt))]
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
		[PXDBCurrency(typeof(CATax.curyInfoID), typeof(CATax.taxAmt))]
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
