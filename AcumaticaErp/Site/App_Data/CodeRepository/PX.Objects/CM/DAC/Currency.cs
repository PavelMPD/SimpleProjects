namespace PX.Objects.CM
{
	using System;
	using PX.Data;
	using PX.Objects.GL;
	using PX.Objects.BQLConstants;
	
	[PXPrimaryGraph(
		new Type[] { typeof(CurrencyMaint)},
		new Type[] { typeof(Select<Currency, 
			Where<Currency.curyID, Equal<Current<Currency.curyID>>>>)
		})]
	[System.SerializableAttribute()]
	[PXCacheName(Messages.Currency)]
	public partial class Currency : PX.Data.IBqlTable
	{
		#region CuryID
		public abstract class curyID : PX.Data.IBqlField
		{
		}
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, IsKey = true, InputMask = ">LLLLL")]
		[PXDefault()]
		[PXUIField(DisplayName = "Currency ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<Currency.curyID, Where<Currency.curyID, NotEqual<Current<Company.baseCuryID>>>>), CacheGlobal = true)]
		[PX.Data.EP.PXFieldDescription]
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
		#region RealGainAcctID
		public abstract class realGainAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _RealGainAcctID;
		[PXDefault]
		[Account(null,
			DisplayName = "Realized Gain Account", 
			Visibility = PXUIVisibility.Visible, 
			DescriptionField = typeof(Account.description))]
		public virtual Int32? RealGainAcctID
		{
			get
			{
				return this._RealGainAcctID;
			}
			set
			{
				this._RealGainAcctID = value;
			}
		}
		#endregion
		#region RealGainSubID
		public abstract class realGainSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _RealGainSubID;
        [PXDefault]
        [SubAccount(typeof(Currency.realGainAcctID), 
			DescriptionField = typeof(Sub.description), 
			DisplayName = "Realized Gain Subaccount", 
			Visibility = PXUIVisibility.Visible)]
		public virtual Int32? RealGainSubID
		{
			get
			{
				return this._RealGainSubID;
			}
			set
			{
				this._RealGainSubID = value;
			}
		}
		#endregion
		#region RealLossAcctID
		public abstract class realLossAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _RealLossAcctID;
        [PXDefault]
        [Account(null, 
			DisplayName = "Realized Loss Account",
		   Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description))]
		public virtual Int32? RealLossAcctID
		{
			get
			{
				return this._RealLossAcctID;
			}
			set
			{
				this._RealLossAcctID = value;
			}
		}
		#endregion
		#region RealLossSubID
		public abstract class realLossSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _RealLossSubID;
        [PXDefault]
        [SubAccount(typeof(Currency.realLossAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Realized Loss Subaccount",
			Visibility = PXUIVisibility.Visible)]
		public virtual Int32? RealLossSubID
		{
			get
			{
				return this._RealLossSubID;
			}
			set
			{
				this._RealLossSubID = value;
			}
		}
		#endregion
		#region RevalGainAcctID
		public abstract class revalGainAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _RevalGainAcctID;
        [PXDefault]
        [Account(null, 
			DisplayName = "Revaluation Gain Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description))]
		public virtual Int32? RevalGainAcctID
		{
			get
			{
				return this._RevalGainAcctID;
			}
			set
			{
				this._RevalGainAcctID = value;
			}
		}
		#endregion
		#region RevalGainSubID
		public abstract class revalGainSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _RevalGainSubID;
        [PXDefault]
        [SubAccount(typeof(Currency.revalGainAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Revaluation Gain Subaccount",
			Visibility = PXUIVisibility.Visible)]
		public virtual Int32? RevalGainSubID
		{
			get
			{
				return this._RevalGainSubID;
			}
			set
			{
				this._RevalGainSubID = value;
			}
		}
		#endregion
		#region RevalLossAcctID
		public abstract class revalLossAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _RevalLossAcctID;
        [PXDefault]
        [Account(null, 
			DisplayName = "Revaluation Loss Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description))]
		public virtual Int32? RevalLossAcctID
		{
			get
			{
				return this._RevalLossAcctID;
			}
			set
			{
				this._RevalLossAcctID = value;
			}
		}
		#endregion
		#region RevalLossSubID
		public abstract class revalLossSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _RevalLossSubID;
        [PXDefault]
        [SubAccount(typeof(Currency.revalLossAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Revaluation Loss Subaccount",
			Visibility = PXUIVisibility.Visible)]
		public virtual Int32? RevalLossSubID
		{
			get
			{
				return this._RevalLossSubID;
			}
			set
			{
				this._RevalLossSubID = value;
			}
		}
		#endregion
		#region ARProvAcctID
		public abstract class aRProvAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _ARProvAcctID;
		[Account(null,
			DisplayName = "AR Provisioning Account",
			DescriptionField = typeof(Account.description))]
		public virtual Int32? ARProvAcctID
		{
			get
			{
				return this._ARProvAcctID;
			}
			set
			{
				this._ARProvAcctID = value;
			}
		}
		#endregion
		#region ARProvSubID
		public abstract class aRProvSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _ARProvSubID;
		[SubAccount(typeof(Currency.aRProvAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "AR Provisioning Subaccount")]
		public virtual Int32? ARProvSubID
		{
			get
			{
				return this._ARProvSubID;
			}
			set
			{
				this._ARProvSubID = value;
			}
		}
		#endregion
		#region APProvAcctID
		public abstract class aPProvAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _APProvAcctID;
		[Account(null,
			DisplayName = "AP Provisioning Account",
			DescriptionField = typeof(Account.description))]
		public virtual Int32? APProvAcctID
		{
			get
			{
				return this._APProvAcctID;
			}
			set
			{
				this._APProvAcctID = value;
			}
		}
		#endregion
		#region APProvSubID
		public abstract class aPProvSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _APProvSubID;
		[SubAccount(typeof(Currency.aPProvAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "AP Provisioning Subaccount")]
		public virtual Int32? APProvSubID
		{
			get
			{
				return this._APProvSubID;
			}
			set
			{
				this._APProvSubID = value;
			}
		}
		#endregion
		#region TranslationGainAcctID
		public abstract class translationGainAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _TranslationGainAcctID;
        [PXDefault]
        [Account(null, 
			DisplayName = "Translation Gain Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description))]
		public virtual Int32? TranslationGainAcctID
		{
			get
			{
				return this._TranslationGainAcctID;
			}
			set
			{
				this._TranslationGainAcctID = value;
			}
		}
		#endregion
		#region TranslationGainSubID
		public abstract class translationGainSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _TranslationGainSubID;
        [PXDefault]
        [SubAccount(typeof(Currency.translationGainAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Translation Gain Subaccount",
			Visibility = PXUIVisibility.Visible)]
		public virtual Int32? TranslationGainSubID
		{
			get
			{
				return this._TranslationGainSubID;
			}
			set
			{
				this._TranslationGainSubID = value;
			}
		}
		#endregion
		#region TranslationLossAcctID
		public abstract class translationLossAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _TranslationLossAcctID;
        [PXDefault]
        [Account(null, 
			DisplayName = "Translation Loss Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description))]
		public virtual Int32? TranslationLossAcctID
		{
			get
			{
				return this._TranslationLossAcctID;
			}
			set
			{
				this._TranslationLossAcctID = value;
			}
		}
		#endregion
		#region TranslationLossSubID
		public abstract class translationLossSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _TranslationLossSubID;
        [PXDefault]
        [SubAccount(typeof(Currency.translationLossAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Translation Loss Subaccount",
			Visibility = PXUIVisibility.Visible)]
		public virtual Int32? TranslationLossSubID
		{
			get
			{
				return this._TranslationLossSubID;
			}
			set
			{
				this._TranslationLossSubID = value;
			}
		}
		#endregion
		#region UnrealizedGainAcctID
		public abstract class unrealizedGainAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _UnrealizedGainAcctID;
        [PXDefault]
        [Account(null, 
			DisplayName = "Unrealized Gain Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description))]
		public virtual Int32? UnrealizedGainAcctID
		{
			get
			{
				return this._UnrealizedGainAcctID;
			}
			set
			{
				this._UnrealizedGainAcctID = value;
			}
		}
		#endregion
		#region UnrealizedGainSubID
		public abstract class unrealizedGainSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _UnrealizedGainSubID;
        [PXDefault]
        [SubAccount(typeof(Currency.unrealizedGainAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Unrealized Gain Subaccount",
			Visibility = PXUIVisibility.Visible)]
		public virtual Int32? UnrealizedGainSubID
		{
			get
			{
				return this._UnrealizedGainSubID;
			}
			set
			{
				this._UnrealizedGainSubID = value;
			}
		}
		#endregion
		#region UnrealizedLossAcctID
		public abstract class unrealizedLossAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _UnrealizedLossAcctID;
        [PXDefault]
        [Account(null, 
			DisplayName = "Unrealized Loss Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description))]
		public virtual Int32? UnrealizedLossAcctID
		{
			get
			{
				return this._UnrealizedLossAcctID;
			}
			set
			{
				this._UnrealizedLossAcctID = value;
			}
		}
		#endregion
		#region UnrealizedLossSubID
		public abstract class unrealizedLossSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _UnrealizedLossSubID;
        [PXDefault]
        [SubAccount(typeof(Currency.unrealizedLossAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Unrealized Loss Subaccount",
			Visibility = PXUIVisibility.Visible)]
		public virtual Int32? UnrealizedLossSubID
		{
			get
			{
				return this._UnrealizedLossSubID;
			}
			set
			{
				this._UnrealizedLossSubID = value;
			}
		}
		#endregion
		#region RoundingGainAcctID
		public abstract class roundingGainAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _RoundingGainAcctID;
        [PXDefault]
        [Account(null, 
			DisplayName = "Rounding Gain Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description))]
		public virtual Int32? RoundingGainAcctID
		{
			get
			{
				return this._RoundingGainAcctID;
			}
			set
			{
				this._RoundingGainAcctID = value;
			}
		}
		#endregion
		#region RoundingGainSubID
		public abstract class roundingGainSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _RoundingGainSubID;
        [PXDefault]
        [SubAccount(typeof(Currency.roundingGainAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Rounding Gain Subaccount",
			Visibility = PXUIVisibility.Visible)]
		public virtual Int32? RoundingGainSubID
		{
			get
			{
				return this._RoundingGainSubID;
			}
			set
			{
				this._RoundingGainSubID = value;
			}
		}
		#endregion
		#region RoundingLossAcctID
		public abstract class roundingLossAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _RoundingLossAcctID;
        [PXDefault]
        [Account(null, 
			DisplayName = "Rounding Loss Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description))]
		public virtual Int32? RoundingLossAcctID
		{
			get
			{
				return this._RoundingLossAcctID;
			}
			set
			{
				this._RoundingLossAcctID = value;
			}
		}
		#endregion
		#region RoundingLossSubID
		public abstract class roundingLossSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _RoundingLossSubID;
        [PXDefault]
        [SubAccount(typeof(Currency.roundingLossAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Rounding Loss Subaccount",
			Visibility = PXUIVisibility.Visible)]
		public virtual Int32? RoundingLossSubID
		{
			get
			{
				return this._RoundingLossSubID;
			}
			set
			{
				this._RoundingLossSubID = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.IBqlField
		{
		}
		protected String _Description;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region CurySymbol
		public abstract class curySymbol : PX.Data.IBqlField
		{
		}
		protected String _CurySymbol;
		[PXDBString(10, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Currency Symbol")]
		public virtual String CurySymbol
		{
			get
			{
				return this._CurySymbol;
			}
			set
			{
				this._CurySymbol = value;
			}
		}
		#endregion
		#region CuryCaption
		public abstract class curyCaption : PX.Data.IBqlField
		{
		}
		protected String _CuryCaption;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Currency Caption")]
		public virtual String CuryCaption
		{
			get
			{
				return this._CuryCaption;
			}
			set
			{
				this._CuryCaption = value;
			}
		}
		#endregion
		#region DecimalPlaces
		public abstract class decimalPlaces : PX.Data.IBqlField
		{
		}
		protected Int16? _DecimalPlaces;
		[PXDBShort(MinValue = 0, MaxValue = 4)]
		[PXDefault((short)2)]
		[PXUIField(DisplayName = "Decimal Precision")]
		public virtual short? DecimalPlaces
		{
			get
			{
				return this._DecimalPlaces;
			}
			set
			{
				this._DecimalPlaces = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : IBqlField { }

		[PXNote(DescriptionField = typeof(Currency.curyID))]
		public virtual Int64? NoteID { get; set; }
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
	}
    [Serializable]
	public partial class Currency2 : Currency
	{
		public new abstract class curyID : PX.Data.IBqlField
		{
		}

		public new abstract class decimalPlaces : PX.Data.IBqlField
		{
		}
	}
}
