using System;
using PX.Data;
using PX.Objects.TX;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.AP;

namespace PX.Objects.EP
{
	[PXProjection(typeof(Select<EPTax, Where<EPTax.lineNbr, Equal<intMax>>>), Persistent = true)]
    [Serializable]
	public partial class EPTaxTran : EPTax
	{
		#region RefNbr
		public new abstract class refNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(EPExpenseClaim.refNbr))]
		[PXUIField(DisplayName = "Ref. Nbr.", Enabled = false, Visible = false)]
        [PXParent(typeof(Select<EPExpenseClaim, Where<EPExpenseClaim.refNbr, Equal<Current<EPTaxTran.refNbr>>>>))]
		public override String RefNbr
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
		[CurrencyInfo(typeof(EPExpenseClaim.curyInfoID))]
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
		[PXDBCurrency(typeof(EPTaxTran.curyInfoID), typeof(EPTaxTran.taxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
        [PXUnboundFormula(typeof(Switch<Case<WhereExempt<EPTaxTran.taxID>, EPTaxTran.curyTaxableAmt>, decimal0>), typeof(SumCalc<EPExpenseClaim.curyVatExemptTotal>))]
        [PXUnboundFormula(typeof(Switch<Case<WhereTaxable<EPTaxTran.taxID>, EPTaxTran.curyTaxableAmt>, decimal0>), typeof(SumCalc<EPExpenseClaim.curyVatTaxableTotal>))]
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
		[PXDBCurrency(typeof(EPTaxTran.curyInfoID), typeof(EPTaxTran.taxAmt))]
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
