using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.TX;

namespace PX.Objects.CR
{
	[Serializable]
	public partial class CRServiceCaseTax : TaxDetail, PX.Data.IBqlTable
	{
		#region ServiceCaseID
		public abstract class serviceCaseID : IBqlField { }

		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(CRServiceCase.serviceCaseID))]
		[PXUIField(Visible = false)]
		[PXParent(typeof(Select<CRServiceCase,
			Where<CRServiceCase.serviceCaseID, Equal<Current<CRServiceCaseTax.serviceCaseID>>>>))]
		public virtual Int32? ServiceCaseID { get; set; }
		#endregion

		#region LineNbr
		public abstract class lineNbr : IBqlField { }

		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(Visible = false)]
		[PXParent(typeof(Select<CRServiceCaseItem,
			Where<CRServiceCaseItem.serviceCaseID, Equal<Current<CRServiceCaseTax.serviceCaseID>>,
				And<CRServiceCaseItem.lineNbr, Equal<Current<CRServiceCaseTax.lineNbr>>>>>))]
		public virtual Int32? LineNbr { get; set; }
		#endregion

		#region TaxID
		public abstract class taxID : IBqlField { }
		[PXDBString(10, IsUnicode = true, IsKey = true)]
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
		public abstract class taxRate : IBqlField { }
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : IBqlField { }

		[PXDBLong]
		[CurrencyInfo(typeof(CRServiceCase.curyInfoID))]
		public override Int64? CuryInfoID
		{
			get
			{
				return base.CuryInfoID;
			}
			set
			{
				base.CuryInfoID = value;
			}
		}
		#endregion

		#region CuryTaxableAmt
		public abstract class curyTaxableAmt : IBqlField { }

		[PXDBCurrency(typeof(CRServiceCaseTax.curyInfoID), typeof(CRServiceCaseTax.taxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public override Decimal? CuryTaxableAmt
		{
			get
			{
				return base.CuryTaxableAmt;
			}
			set
			{
				base.CuryTaxableAmt = value;
			}
		}
		#endregion

		#region TaxableAmt
		public abstract class taxableAmt : IBqlField { }
		#endregion

		#region CuryTaxAmt
		public abstract class curyTaxAmt : IBqlField { }

		[PXDBCurrency(typeof(CRServiceCaseTax.curyInfoID), typeof(CRServiceCaseTax.taxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
		public override Decimal? CuryTaxAmt
		{
			get
			{
				return base.CuryTaxAmt;
			}
			set
			{
				base.CuryTaxAmt = value;
			}
		}
		#endregion

		#region TaxAmt
		public abstract class taxAmt : IBqlField { }
		#endregion

		#region tstamp
		public abstract class Tstamp : IBqlField { }

		[PXDBTimestamp]
		public virtual Byte[] tstamp { get; set; }
		#endregion
	}
}
