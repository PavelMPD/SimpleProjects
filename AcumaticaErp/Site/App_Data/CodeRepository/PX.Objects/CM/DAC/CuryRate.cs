namespace PX.Objects.CM
{
	using System;
	using PX.Data;
	using PX.Objects.CM;

    [Serializable]
	public partial class CurrencyRateByDate2 : CurrencyRate
	{
		#region FromCuryID
		public new abstract class fromCuryID : PX.Data.IBqlField
		{
		}
		#endregion
		#region ToCuryID
		public new abstract class toCuryID : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryRateType
		public new abstract class curyRateType : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryEffDate
		public new abstract class curyEffDate : PX.Data.IBqlField
		{
		}
		#endregion
		#region NextEffDate
		public abstract class nextEffDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _NextEffDate;
		[PXDate()]
		[PXDBScalar(typeof(Search<CurrencyRate2.curyEffDate,
			Where<CurrencyRate2.fromCuryID, Equal<CurrencyRateByDate2.fromCuryID>,
			And<CurrencyRate2.toCuryID, Equal<CurrencyRateByDate2.toCuryID>,
			And<CurrencyRate2.curyRateType, Equal<CurrencyRateByDate2.curyRateType>,
			And<CurrencyRate2.curyEffDate, Greater<CurrencyRateByDate2.curyEffDate>>>>>,
			OrderBy<Asc<CurrencyRate2.curyEffDate>>>))]
		public virtual DateTime? NextEffDate
		{
			get
			{
				return this._NextEffDate;
			}
			set
			{
				this._NextEffDate = value;
			}
		}
		#endregion
		#region CuryMultDiv
		public new abstract class curyMultDiv : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryRate
		public new abstract class curyRate : PX.Data.IBqlField
		{
		}
		#endregion
	}

    [PXCacheName(Messages.CurrencyRate2)]
    [Serializable]
	public partial class CurrencyRate2 : CurrencyRate
	{
		public new abstract class curyEffDate : IBqlField
		{
		}
		public new abstract class fromCuryID : IBqlField
		{
		}
		public new abstract class toCuryID : IBqlField
		{
		}
		public new abstract class curyRateType : IBqlField
		{
		}
	}

    [Serializable]
	public partial class CurrencyRateByDate : CurrencyRate
	{
		#region FromCuryID
		public new abstract class fromCuryID : PX.Data.IBqlField
		{
		}
		#endregion
		#region ToCuryID
		public new abstract class toCuryID : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryRateType
		public new abstract class curyRateType : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryEffDate
		public new abstract class curyEffDate : PX.Data.IBqlField
		{
		}
		#endregion
		#region NextEffDate
		public abstract class nextEffDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _NextEffDate;
		[PXDate()]
		[PXDBScalar(typeof(Search<CurrencyRate2.curyEffDate,
			Where<CurrencyRate2.fromCuryID, Equal<CurrencyRateByDate.fromCuryID>,
			And<CurrencyRate2.toCuryID, Equal<CurrencyRateByDate.toCuryID>,
			And<CurrencyRate2.curyRateType, Equal<CurrencyRateByDate.curyRateType>,
			And<CurrencyRate2.curyEffDate, Greater<CurrencyRateByDate.curyEffDate>>>>>,
			OrderBy<Asc<CurrencyRate2.curyEffDate>>>))]
		public virtual DateTime? NextEffDate
		{
			get
			{
				return this._NextEffDate;
			}
			set
			{
				this._NextEffDate = value;
			}
		}
		#endregion
		#region CuryMultDiv
		public new abstract class curyMultDiv : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryRate
		public new abstract class curyRate : PX.Data.IBqlField
		{
		}
		#endregion
	}

	[System.SerializableAttribute()]
    [PXCacheName(Messages.CurrencyRate)]
	public partial class CurrencyRate : PX.Data.IBqlTable
	{
		#region CuryRateID
		public abstract class curyRateID : PX.Data.IBqlField
		{
		}
		protected Int32? _CuryRateID;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "CuryRate ID", Visibility = PXUIVisibility.Visible)]
		public virtual Int32? CuryRateID
		{
			get
			{
				return this._CuryRateID;
			}
			set
			{
				this._CuryRateID = value;
			}
		}
		#endregion
		#region FromCuryID
		public abstract class fromCuryID : PX.Data.IBqlField
		{
		}
		protected String _FromCuryID;
		[PXDBString(5, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "From Currency", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		[PXSelector(typeof(Search<Currency.curyID, Where<Currency.curyID, NotEqual<Current<CuryRateFilter.toCurrency>>>>))]
		public virtual String FromCuryID
		{
			get
			{
				return this._FromCuryID;
			}
			set
			{
				this._FromCuryID = value;
			}
		}
		#endregion
		#region CuryRateType
		public abstract class curyRateType : PX.Data.IBqlField
		{
		}
		protected String _CuryRateType;
		[PXDBString(6, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Currency Rate Type", Visibility = PXUIVisibility.Visible, Required = true)]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID), DescriptionField = typeof(CurrencyRateType.descr))]
		public virtual String CuryRateType
		{
			get
			{
				return this._CuryRateType;
			}
			set
			{
				this._CuryRateType = value;
			}
		}
		#endregion
		#region CuryEffDate
		public abstract class curyEffDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _CuryEffDate;
		[PXDBDate()]
		[PXDefault(typeof(CuryRateFilter.effDate))]
		[PXUIField(DisplayName = "Currency Effective Date", Visibility = PXUIVisibility.Visible, Required = true)]
		public virtual DateTime? CuryEffDate
		{
			get
			{
				return this._CuryEffDate;
			}
			set
			{
				this._CuryEffDate = value;
			}
		}
		#endregion
		#region CuryMultDiv
		public abstract class curyMultDiv : PX.Data.IBqlField
		{
		}
		protected String _CuryMultDiv;
		[PXDBString(1, IsFixed = true)]
		[PXDefault("M")]
		[PXUIField(DisplayName = "Mult/Div", Visibility = PXUIVisibility.Visible, Required = true)]
		[PXStringList("M;Multiply,D;Divide")]
		public virtual String CuryMultDiv
		{
			get
			{
				return this._CuryMultDiv;
			}
			set
			{
				this._CuryMultDiv = value;
			}
		}
		#endregion
		#region CuryRate
		public abstract class curyRate : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryRate;
		[PXDBDecimal(8, MinValue = 0d)]
		[PXDefault()]
		[PXUIField(DisplayName = "Currency Rate", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		public virtual Decimal? CuryRate
		{
			get
			{
				return this._CuryRate;
			}
			set
			{
				this._CuryRate = value;
			}
		}
		#endregion
		#region RateReciprocal
		public abstract class rateReciprocal : PX.Data.IBqlField
		{
		}
		protected Decimal? _RateReciprocal;
		[PXDBDecimal(8)]
		[PXUIField(DisplayName = "Rate Reciprocal", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, Required = true)]
		public virtual Decimal? RateReciprocal
		{
			get
			{
				return this._RateReciprocal;
			}
			set
			{
				this._RateReciprocal = value;
			}
		}
		#endregion
		#region ToCuryID
		public abstract class toCuryID : PX.Data.IBqlField
		{
		}
		protected String _ToCuryID;
		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "To Currency", Visibility = PXUIVisibility.Invisible)]
		[PXDefault(typeof(CuryRateFilter.toCurrency))]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String ToCuryID
		{
			get
			{
				return this._ToCuryID;
			}
			set
			{
				this._ToCuryID = value;
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
		#region CreatedByID
		public abstract class createdByID : PX.Data.IBqlField
		{
		}
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.IBqlField
		{
		}
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.IBqlField
		{
		}
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.IBqlField
		{
		}
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#region Methods
		public static implicit operator CurrencyInfo(CurrencyRate item)
		{
			CurrencyInfo ret	= new CurrencyInfo();
			ret.BaseCuryID		= item.ToCuryID;
			ret.CuryRateTypeID	= item.CuryRateType;
			ret.CuryEffDate		= item.CuryEffDate == null ? (DateTime?)null : ((DateTime)item.CuryEffDate).AddDays(-1);
			ret.CuryID			= item.FromCuryID;
			ret.CuryMultDiv		= item.CuryMultDiv;
			ret.CuryRate		= item.CuryRate;
			ret.RecipRate		= item.RateReciprocal;

			return ret;
		}
		#endregion
	}

	[System.SerializableAttribute()]
	public partial class CuryRateFilter : PX.Data.IBqlTable
	{
		#region ToCurrency
		public abstract class toCurrency : PX.Data.IBqlField
		{
		}
		protected string _ToCurrency;
		[PXDBString(5, IsUnicode = true)]
		[PXDefault(typeof(Search<PX.Objects.GL.Company.baseCuryID>))]
		[PXUIField(DisplayName = "To Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Currency.curyID))]
		public virtual string ToCurrency
		{
			get
			{
				return this._ToCurrency;
			}
			set
			{
				this._ToCurrency = value;
			}
		}
		#endregion
		#region EffDate
		public abstract class effDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _EffDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Effective Date")]
		[PXDefault(typeof(AccessInfo.businessDate))]
		public virtual DateTime? EffDate
		{
			get
			{
				return this._EffDate;
			}
			set
			{
				this._EffDate = value;
			}
		}
		#endregion
	}

	public class CuryMultDivType
	{
		public const string Mult = "M";
		public const string Div = "D";

		public class mult : Constant<string>
		{
			public mult():base(Mult)
			{
			}
		}
		public class div : Constant<string>
		{
			public div()
				: base(Div)
			{
			}
		}
	}
}
