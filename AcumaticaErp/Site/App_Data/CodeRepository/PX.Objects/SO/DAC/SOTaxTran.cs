using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.TX;
using PX.Objects.CM;
using PX.Objects.CS;

namespace PX.Objects.SO
{
	[PXProjection(typeof(Select<SOTax, Where<SOTax.lineNbr, Equal<intMax>>>), Persistent = true)]
	[PXCacheName(Messages.SOTaxTran)]
    [Serializable]
	[PXBreakInheritance()]
	public partial class SOTaxTran : SOTax
	{
		#region OrderType
		public new abstract class orderType : PX.Data.IBqlField
		{
		}
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(SOOrder.orderType))]
		[PXUIField(DisplayName = "Order Type", Enabled = false, Visible = false)]
		public override String OrderType
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
		public new abstract class orderNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDBDefault(typeof(SOOrder.orderNbr))]
		[PXUIField(DisplayName = "Order Nbr.", Enabled = false, Visible = false)]
		public override String OrderNbr
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
		public new abstract class bONbr : PX.Data.IBqlField
		{
		}
		[PXDBShort(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Back Order", Visibility = PXUIVisibility.Visible, Visible = false)]
        [PXParent(typeof(Select<SOOrder, Where<SOOrder.orderType, Equal<Current<SOTaxTran.orderType>>, And<SOOrder.orderNbr, Equal<Current<SOTaxTran.orderNbr>>>>>))]
		public override Int16? BONbr
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
		public new abstract class lineNbr : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault(int.MaxValue)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
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
		#region TaxID
		public new abstract class taxID : PX.Data.IBqlField
		{
		}
		[PXDBString(Tax.taxID.Length, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Tax ID", Visibility = PXUIVisibility.Visible)]
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
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.IBqlField
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
		public new abstract class curyTaxableAmt : PX.Data.IBqlField
		{
		}
		[PXDBCurrency(typeof(SOTaxTran.curyInfoID), typeof(SOTaxTran.taxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
        [PXUnboundFormula(typeof(Switch<Case<Where2<WhereExempt<SOTaxTran.taxID>, And<SOTaxTran.bONbr, Equal<short0>>>, SOTaxTran.curyTaxableAmt>, decimal0>), typeof(SumCalc<SOOrder.curyVatExemptTotal>))]
        [PXUnboundFormula(typeof(Switch<Case<Where2<WhereTaxable<SOTaxTran.taxID>, And<SOTaxTran.bONbr, Equal<short0>>>, SOTaxTran.curyTaxableAmt>, decimal0>), typeof(SumCalc<SOOrder.curyVatTaxableTotal>))]
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
		public new abstract class taxableAmt : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryTaxAmt
		public new abstract class curyTaxAmt : PX.Data.IBqlField
		{
		}
		[PXDBCurrency(typeof(SOTaxTran.curyInfoID), typeof(SOTaxTran.taxAmt))]
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
		public new abstract class taxAmt : PX.Data.IBqlField
		{
		}
		#endregion
	}
}
