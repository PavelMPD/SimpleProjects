using System;
using PX.Data;
using PX.Objects.TX;
using PX.Objects.CM;
using PX.Objects.CS;

namespace PX.Objects.CR
{
	[PXProjection(typeof(Select<CRServiceCaseTax, Where<CRServiceCaseTax.lineNbr, Equal<intMax>>>), Persistent = true)]
    [Serializable]
	public partial class CRServiceCaseTaxTran : CRServiceCaseTax
	{
		#region ServiceCaseID
		public new abstract class serviceCaseID : IBqlField { }

		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(CRServiceCase.serviceCaseID))]
		[PXUIField(Visible = false)]
		[PXParent(typeof(Select<CRServiceCase,
			Where<CRServiceCase.serviceCaseID, Equal<Current<CRServiceCaseTaxTran.serviceCaseID>>>>))]
		public override Int32? ServiceCaseID
		{
			get { return base.ServiceCaseID; }
			set { base.ServiceCaseID = value; }
		}
		#endregion

		#region LineNbr
		public new abstract class lineNbr : IBqlField { }

		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(Visible = false)]
		[PXParent(typeof(Select<CRServiceCaseItem,
			Where<CRServiceCaseItem.serviceCaseID, Equal<Current<CRServiceCaseTaxTran.serviceCaseID>>,
				And<CRServiceCaseItem.lineNbr, Equal<Current<CRServiceCaseTaxTran.lineNbr>>>>>))]
		public override Int32? LineNbr
		{
			get { return base.LineNbr; }
			set { base.LineNbr = value; }
		}
		#endregion

		#region TaxID
		public new abstract class taxID : IBqlField { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Tax ID", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Tax.taxID))]
		public override String TaxID
		{
			get
			{
				return base.TaxID;
			}
			set
			{
				base.TaxID = value;
			}
		}
		#endregion

		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.IBqlField { }

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
		public new abstract class curyTaxableAmt : IBqlField { }

		[PXDBCurrency(typeof(CRServiceCaseTaxTran.curyInfoID), typeof(CRServiceCaseTaxTran.taxableAmt))]
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
		public new abstract class taxableAmt : IBqlField { }
		#endregion

		#region CuryTaxAmt
		public new abstract class curyTaxAmt : IBqlField { }

		[PXDBCurrency(typeof(CRServiceCaseTaxTran.curyInfoID), typeof(CRServiceCaseTaxTran.taxAmt))]
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
		public new abstract class taxAmt : IBqlField { }
		#endregion
	}
}
