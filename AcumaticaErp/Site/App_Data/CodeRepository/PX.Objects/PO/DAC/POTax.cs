namespace PX.Objects.PO
{
	using System;
	using PX.Data;
	using PX.Objects.TX;
	using PX.Objects.CM;
    using PX.Objects.CS;
	
	[System.SerializableAttribute()]
	public partial class POTax :TaxDetail, PX.Data.IBqlTable
	{
		#region OrderType
		public abstract class orderType : PX.Data.IBqlField
		{
		}
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(POOrder.orderType), DefaultForUpdate = false)]
		[PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.Visible, Visible = false)]
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
		[PXDBDefault(typeof(POOrder.orderNbr),DefaultForUpdate=false)]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
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
		#region DetailType
		public abstract class detailType : PX.Data.IBqlField
		{
		}
		protected Int16? _DetailType;
		[PXDBShort(IsKey = true)]
		[PXDefault(POTaxDetailType.OrderTax)]
		[PXUIField(DisplayName = "Tax Detail Type", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Int16? DetailType
		{
			get
			{
				return this._DetailType;
			}
			set
			{
				this._DetailType = value;
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
		[PXParent(typeof(Select<POLine, Where<POLine.orderType, Equal<Current<POTax.orderType>>,
			                      And<POLine.orderNbr, Equal<Current<POTax.orderNbr>>, 
								  And<POLine.lineNbr, Equal<Current<POTax.lineNbr>>>>>>))]
		[PXParent(typeof(Select<POLineR, Where<POLineR.orderType, Equal<Current<POTax.orderType>>,
								  And<POLineR.orderNbr, Equal<Current<POTax.orderNbr>>,
								  And<POLineR.lineNbr, Equal<Current<POTax.lineNbr>>>>>>))]
		[PXParent(typeof(Select<POLineUOpen, Where<POLineUOpen.orderType, Equal<Current<POTax.orderType>>,
								  And<POLineUOpen.orderNbr, Equal<Current<POTax.orderNbr>>,
								  And<POLineUOpen.lineNbr, Equal<Current<POTax.lineNbr>>>>>>))]
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
		public  abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		
		[PXDBLong()]
		[CurrencyInfo(typeof(POOrder.curyInfoID))]
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

		[PXDBCurrency(typeof(POTax.curyInfoID), typeof(POTax.taxableAmt))]
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

		[PXDBCurrency(typeof(POTax.curyInfoID), typeof(POTax.taxAmt))]
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

	[PXProjection(typeof(Select<POTax, Where<POTax.lineNbr, Equal<POTaxTran.lineNbrValue>>>), Persistent = true)]
    [Serializable]
	public partial class POTaxTran : POTax 
	{
		#region OrderType
		public new abstract class orderType : PX.Data.IBqlField
		{
		}
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(POOrder.orderType), DefaultForUpdate = false)]
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
		#region Order
		public new abstract class orderNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDBDefault(typeof(POOrder.orderNbr), DefaultForUpdate = false)]
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
		#region DetailType
		public new abstract class detailType : PX.Data.IBqlField
		{
		}
		
		[PXDBShort(IsKey = true)]
		[PXDefault(POTaxDetailType.OrderTax)]
		[PXUIField(DisplayName = "Tax Detail Type", Visibility = PXUIVisibility.Visible, Visible = false)]
        [PXParent(typeof(Select<POOrder, Where<POOrder.orderType, Equal<Current<POTaxTran.orderType>>, And<POOrder.orderNbr, Equal<Current<POTaxTran.orderNbr>>>>>))]
		public override Int16? DetailType
		{
			get
			{
				return this._DetailType;
			}
			set
			{
				this._DetailType = value;
			}
		}
		#endregion
		#region Line Nbr
		public new abstract class lineNbr : PX.Data.IBqlField
		{
		}

		[PXDBInt(IsKey = true)]
		[PXDefault(LineNbrValue)]
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
		[CurrencyInfo(typeof(POOrder.curyInfoID))]
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
		[PXDBCurrency(typeof(POTaxTran.curyInfoID), typeof(POTaxTran.taxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
        [PXUnboundFormula(typeof(Switch<Case<Where2<WhereExempt<POTaxTran.taxID>, And<POTaxTran.detailType, Equal<short0>>>, POTaxTran.curyTaxableAmt>, decimal0>), typeof(SumCalc<POOrder.curyVatExemptTotal>))]
        [PXUnboundFormula(typeof(Switch<Case<Where2<WhereTaxable<POTaxTran.taxID>, And<POTaxTran.detailType, Equal<short0>>>, POTaxTran.curyTaxableAmt>, decimal0>), typeof(SumCalc<POOrder.curyVatTaxableTotal>))]
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
		[PXDBCurrency(typeof(POTaxTran.curyInfoID), typeof(POTaxTran.taxAmt))]
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
		public  class lineNbrValue : Constant<Int32>
		{
			public lineNbrValue() : base(LineNbrValue) {;}
		}

		public const Int32 LineNbrValue = int.MaxValue;
		
	}


	public class POTaxDetailType 
	{
		public const short OrderTax = 0;
		public const short OrderOpenTax = 1;

		public class orderTax : PX.Objects.CS.short0 
		{
		}
		public class orderOpenTax : PX.Objects.CS.short1
		{
		}
	}
}
