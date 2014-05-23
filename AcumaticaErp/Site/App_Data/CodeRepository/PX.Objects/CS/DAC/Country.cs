namespace PX.Objects.CS
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	[PXCacheName(Messages.Country)]
	[PXPrimaryGraph(
		new Type[] { typeof(CountryMaint)},
		new Type[] { typeof(Select<Country, 
			Where<Country.countryID, Equal<Current<Country.countryID>>>>)
		})]
	public partial class Country : PX.Data.IBqlTable
	{
		#region CountryID
		public abstract class countryID : PX.Data.IBqlField
		{
		}
		protected String _CountryID;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Country ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Country.countryID), CacheGlobal = true)]
		public virtual String CountryID
		{
			get
			{
				return this._CountryID;
			}
			set
			{
				this._CountryID = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.IBqlField
		{
		}
		protected String _Description;
		[PXDBString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Country", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region ZipCodeMask
		public abstract class zipCodeMask : PX.Data.IBqlField
		{
		}
		protected String _ZipCodeMask;
		[PXDBString(50)]
		[PXUIField(DisplayName = "Postal Code Mask")]
		public virtual String ZipCodeMask
		{
			get
			{
				return this._ZipCodeMask;
			}
			set
			{
				this._ZipCodeMask = value;
			}
		}
		#endregion
		#region ZipCodeRegexp
		public abstract class zipCodeRegexp : PX.Data.IBqlField
		{
		}
		protected String _ZipCodeRegexp;
		[PXDBString(255)]
		[PXUIField(DisplayName = "Postal Code Validation Reg. Exp.")]
		public virtual String ZipCodeRegexp
		{
			get
			{
				return this._ZipCodeRegexp;
			}
			set
			{
				this._ZipCodeRegexp = value;
			}
		}
		#endregion
		#region PhoneCountryCode
		public abstract class phoneCountryCode : PX.Data.IBqlField
		{
		}
		protected String _PhoneCountryCode;
		[PXDBString(5)]
		[PXUIField(DisplayName = "Country Phone Code")]
		public virtual String PhoneCountryCode
		{
			get
			{
				return this._PhoneCountryCode;
			}
			set
			{
				this._PhoneCountryCode = value;
			}
		}
		#endregion
		#region PhoneMask
		public abstract class phoneMask : PX.Data.IBqlField
		{
		}
		protected String _PhoneMask;
		[PXDBString(50)]
		[PXUIField(DisplayName = "Phone Mask")]
		public virtual String PhoneMask
		{
			get
			{
				return this._PhoneMask;
			}
			set
			{
				this._PhoneMask = value;
			}
		}
		#endregion
		#region PhoneRegexp
		public abstract class phoneRegexp : PX.Data.IBqlField
		{
		}
		protected String _PhoneRegexp;
		[PXDBString(255)]
		[PXUIField(DisplayName = "Phone Validation Reg. Exp.")]
		public virtual String PhoneRegexp
		{
			get
			{
				return this._PhoneRegexp;
			}
			set
			{
				this._PhoneRegexp = value;
			}
		}
		#endregion
		#region IsTaxRegistrationRequired
		public abstract class isTaxRegistrationRequired : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsTaxRegistrationRequired;
		[PXDBBool()]
		[PXUIField(DisplayName = "Tax Registration Required")]
		public virtual Boolean? IsTaxRegistrationRequired
		{
			get
			{
				return this._IsTaxRegistrationRequired;
			}
			set
			{
				this._IsTaxRegistrationRequired = value;
			}
		}
		#endregion
		#region TaxRegistrationMask
		public abstract class taxRegistrationMask : PX.Data.IBqlField
		{
		}
		protected String _TaxRegistrationMask;
		[PXDBString(255)]
		[PXUIField(DisplayName = "Tax Registration Mask")]
		public virtual String TaxRegistrationMask
		{
			get
			{
				return this._TaxRegistrationMask;
			}
			set
			{
				this._TaxRegistrationMask = value;
			}
		}
		#endregion
		#region TaxRegistrationRegexp
		public abstract class taxRegistrationRegexp : PX.Data.IBqlField
		{
		}
		protected String _TaxRegistrationRegexp;
		[PXDBString(255)]
		[PXUIField(DisplayName = "Tax Reg. Validation Reg. Exp.")]
		public virtual String TaxRegistrationRegexp
		{
			get
			{
				return this._TaxRegistrationRegexp;
			}
			set
			{
				this._TaxRegistrationRegexp = value;
			}
		}
		#endregion
		#region AllowStateEdit
		public abstract class allowStateEdit : PX.Data.IBqlField
		{
		}
		protected Boolean? _AllowStateEdit;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow State Edit")]
		public virtual Boolean? AllowStateEdit
		{
			get
			{
				return this._AllowStateEdit;
			}
			set
			{
				this._AllowStateEdit = value;
			}
		}
		#endregion
		#region AddressVerificationTypeName
		public abstract class addressVerificationTypeName : PX.Data.IBqlField
		{
		}
		protected String _AddressVerificationTypeName;
		[PXDBString(255)]
		[PXUIField(DisplayName = "Address Verification Service")]
		[CA.PXProviderTypeSelector(typeof(IAddressValidationService))]
		public virtual String AddressVerificationTypeName
		{
			get
			{
				return this._AddressVerificationTypeName;
			}
			set
			{
				this._AddressVerificationTypeName = value;
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
}
