namespace PX.Objects.PO
{
	using System;
	using PX.Data;
	using PX.Objects.TX;
	using PX.Objects.CM;
    using PX.Objects.CS;

	[System.SerializableAttribute()]
	public partial class POReceiptTax : TaxDetail, PX.Data.IBqlTable
	{
		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.IBqlField
		{
		}
		protected String _ReceiptNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDBDefault(typeof(POReceipt.receiptNbr))]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual String ReceiptNbr
		{
			get
			{
				return this._ReceiptNbr;
			}
			set
			{
				this._ReceiptNbr = value;
			}
		}
		#endregion
		#region DetailType
		public abstract class detailType : PX.Data.IBqlField
		{
		}
		protected Int16? _DetailType;
		[PXDBShort(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Detail Type", Visibility = PXUIVisibility.Visible, Visible = false)]
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
		[PXParent(typeof(Select<POReceiptLine, Where<POReceiptLine.receiptNbr, Equal<Current<POReceiptTax.receiptNbr>>,
								  And<POReceiptLine.lineNbr, Equal<Current<POReceiptTax.lineNbr>>>>>))]
		[PXParent(typeof(Select<POReceiptLineR1, Where<POReceiptLineR1.receiptNbr, Equal<Current<POReceiptTax.receiptNbr>>,
								  And<POReceiptLineR1.lineNbr, Equal<Current<POReceiptTax.lineNbr>>>>>))]
		
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
		[CurrencyInfo(typeof(POReceipt.curyInfoID))]
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

		[PXDBCurrency(typeof(POReceiptTax.curyInfoID), typeof(POReceiptTax.taxableAmt))]
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

		[PXDBCurrency(typeof(POReceiptTax.curyInfoID), typeof(POReceiptTax.taxAmt))]
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

	[PXProjection(typeof(Select<POReceiptTax, Where<POReceiptTax.lineNbr, Equal<POReceiptTaxTran.lineNbrValue>>>), Persistent = true)]
    [Serializable]
	[PXBreakInheritance()]
	public partial class POReceiptTaxTran : POReceiptTax
	{
		#region Receipt Nbr
		public new abstract class receiptNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDBDefault(typeof(POReceipt.receiptNbr))]
		[PXUIField(DisplayName = "Receipt Nbr.", Enabled = false, Visible = false)]
		public override String ReceiptNbr
		{
			get
			{
				return this._ReceiptNbr;
			}
			set
			{
				this._ReceiptNbr = value;
			}
		}
		#endregion
		#region DetailType
		public new abstract class detailType : PX.Data.IBqlField
		{
		}

		[PXDBShort(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Back Order", Visibility = PXUIVisibility.Visible, Visible = false)]
    [PXParent(typeof(Select<POReceipt, Where<POReceipt.receiptNbr, Equal<Current<POReceiptTaxTran.receiptNbr>>>>))]
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
		[PXDefault(POReceiptTaxTran.LineNbrValue)]
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
		[CurrencyInfo(typeof(POReceipt.curyInfoID))]
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
		[PXDBCurrency(typeof(POReceiptTaxTran.curyInfoID), typeof(POReceiptTaxTran.taxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
        [PXUnboundFormula(typeof(Switch<Case<Where2<WhereExempt<POReceiptTaxTran.taxID>, And<POReceiptTaxTran.detailType, Equal<short0>>>, POReceiptTaxTran.curyTaxableAmt>, decimal0>), typeof(SumCalc<POReceipt.curyVatExemptTotal>))]
        [PXUnboundFormula(typeof(Switch<Case<Where2<WhereTaxable<POReceiptTaxTran.taxID>, And<POReceiptTaxTran.detailType, Equal<short0>>>, POReceiptTaxTran.curyTaxableAmt>, decimal0>), typeof(SumCalc<POReceipt.curyVatTaxableTotal>))]
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
		[PXDBCurrency(typeof(POReceiptTaxTran.curyInfoID), typeof(POReceiptTaxTran.taxAmt))]
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
		public class lineNbrValue : Constant<int>
		{
			public lineNbrValue() : base(LineNbrValue) { ;}
		}

		public const int LineNbrValue =  Int32.MaxValue;
	}

	public class POReceiptTaxDetailType
	{
		public const short ReceiptTax = 0;
		public const short UnbilledTax = 1;

		public class receiptTax : PX.Objects.CS.short0
		{
		}
		public class unbilledTax : PX.Objects.CS.short1
		{
		}
	}

}
