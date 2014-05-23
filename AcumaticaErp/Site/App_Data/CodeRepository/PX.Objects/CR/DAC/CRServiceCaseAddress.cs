using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.CR
{
	#region CRServiceCaseAddress

	[PXCacheName(Messages.Address)]
	[Serializable]
	public partial class CRServiceCaseAddress : IBqlTable, IAddress, IAddressBase
	{
		#region AddressID
		public abstract class addressID : IBqlField { }

		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Address ID", Visible = false)]
		public virtual Int32? AddressID { get; set; }
		#endregion

		#region CustomerID
		public abstract class customerID : IBqlField { }

		[PXDBInt]
		[PXDBDefault(typeof(CRServiceCase.customerID))]
		public virtual Int32? CustomerID { get; set; }

		public Int32? BAccountID
		{
			get { return CustomerID; }
			set { CustomerID = value; }
		}
		#endregion

		#region CustomerAddressID
		public abstract class customerAddressID : IBqlField { }

		[PXDBInt]
		public virtual Int32? CustomerAddressID { get; set; }

		public Int32? BAccountAddressID
		{
			get { return CustomerAddressID; }
			set { CustomerAddressID = value; }
		}
		#endregion

		#region IsDefaultAddress
		public abstract class isDefaultAddress : IBqlField { }

		[PXDBBool]
		[PXUIField(DisplayName = "Default Customer Address", Visibility = PXUIVisibility.Visible)]
		[PXDefault(true)]
		public virtual Boolean? IsDefaultAddress { get; set; }
		#endregion

		#region OverrideAddress
		public abstract class overrideAddress : IBqlField { }

		[PXBool]
		[PXUIField(DisplayName = "Override Address", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? OverrideAddress
		{
			[PXDependsOnFields(typeof(isDefaultAddress))]
			get
			{
				return (bool?)(this.IsDefaultAddress == null ? this.IsDefaultAddress : this.IsDefaultAddress == false);
			}
			set
			{
				this.IsDefaultAddress = (bool?)(value == null ? value : value == false);
			}
		}
		#endregion

		#region AddressLine1
		public abstract class addressLine1 : IBqlField { }

		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Address Line 1", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String AddressLine1 { get; set; }
		#endregion

		#region AddressLine2
		public abstract class addressLine2 : IBqlField { }

		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Address Line 2")]
		public virtual String AddressLine2 { get; set;}
		#endregion

		#region AddressLine3
		public abstract class addressLine3 : IBqlField { }

		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Address Line 3")]
		public virtual String AddressLine3 { get; set; }
		#endregion

		#region City
		public abstract class city : IBqlField { }

		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "City", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String City { get; set; }
		#endregion

		#region CountryID
		public abstract class countryID : IBqlField { }

		[PXDefault(typeof(Search<GL.Branch.countryID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Country")]
		[PXSelector(typeof(Search<Country.countryID>), DescriptionField = typeof(Country.description), CacheGlobal = true)]
		public virtual String CountryID { get; set; }
		#endregion

		#region State
		public abstract class state : IBqlField { }

		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "State")]
		[CR.State(typeof(CRServiceCaseAddress.countryID), DescriptionField = typeof(State.name))]
		public virtual String State { get; set; }
		#endregion

		#region PostalCode
		public abstract class postalCode : IBqlField { }

		[PXDBString(20)]
		[PXUIField(DisplayName = "Postal Code")]
		[PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), CountryIdField = typeof(CRServiceCaseAddress.countryID))]
		public virtual String PostalCode { get; set; }
		#endregion

		#region RevisionID

		public Int32? RevisionID
		{
			get { return 0;}
			set {}
		}

		#endregion

		#region IsValidated
		public abstract class isValidated : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsValidated;

		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBBool()]
		[PXUIField(DisplayName = "Validated", FieldClass = CS.Messages.ValidateAddress)]
		public virtual Boolean? IsValidated
		{
			get
			{
				return this._IsValidated;
			}
			set
			{
				this._IsValidated = value;
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

	#endregion

	#region CRServiceCaseBillingAddress

	[Serializable]
	[PXCacheName(Messages.BillingAddress)]
	public partial class CRServiceCaseBillingAddress : CRServiceCaseAddress
	{
		#region AddressID

		public new abstract class addressID : IBqlField
		{
		}

		#endregion

		#region CustomerID

		public new abstract class customerID : IBqlField
		{
		}

		#endregion

		#region CustomerAddressID

		public new abstract class customerAddressID : IBqlField
		{
		}

		#endregion

		#region IsDefaultAddress

		public new abstract class isDefaultAddress : IBqlField
		{
		}

		#endregion

		#region OverrideAddress

		public new abstract class overrideAddress : IBqlField
		{
		}

		#endregion

		#region RevisionID

		public abstract class revisionID : IBqlField
		{
		}

		#endregion

		#region CountryID

		public new abstract class countryID : IBqlField
		{
		}

		[PXDefault(typeof(Search<GL.Branch.countryID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Country")]
		[PXSelector(typeof (Search<Country.countryID>), DescriptionField = typeof (Country.description), CacheGlobal = true)]
		public override String CountryID
		{
			get { return base.CountryID; }
			set { base.CountryID = value; }
		}

		#endregion

		#region State

		public new abstract class state : IBqlField
		{
		}

		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "State")]
		[State(typeof (countryID), DescriptionField = typeof (State.name))]
		public override String State
		{
			get { return base.State; }
			set { base.State = value; }
		}

		#endregion

		#region PostalCode

		public new abstract class postalCode : IBqlField
		{
		}

		[PXDBString(20)]
		[PXUIField(DisplayName = "Postal Code")]
		[PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), CountryIdField = typeof(countryID))]
		public override String PostalCode
		{
			get { return base.PostalCode; }
			set { base.PostalCode = value; }
		}

		#endregion
	}

	#endregion

	#region CRServiceCaseDestinationAddress

	[Serializable]
	[PXCacheName(Messages.DestinationAddress)]
	public partial class CRServiceCaseDestinationAddress : CRServiceCaseAddress
	{
		#region AddressID

		public new abstract class addressID : IBqlField
		{
		}

		#endregion

		#region CustomerID

		public new abstract class customerID : IBqlField
		{
		}

		#endregion

		#region CustomerAddressID

		public new abstract class customerAddressID : IBqlField
		{
		}

		#endregion

		#region IsDefaultAddress

		public new abstract class isDefaultAddress : IBqlField
		{
		}

		#endregion

		#region OverrideAddress

		public new abstract class overrideAddress : IBqlField
		{
		}

		#endregion

		#region RevisionID

		public abstract class revisionID : IBqlField
		{
		}

		#endregion

		#region CountryID

		public new abstract class countryID : IBqlField
		{
		}

		[PXDefault(typeof(Search<GL.Branch.countryID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Country")]
		[PXSelector(typeof (Search<Country.countryID>), DescriptionField = typeof (Country.description), CacheGlobal = true)]
		public override String CountryID
		{
			get { return base.CountryID; }
			set { base.CountryID = value; }
		}

		#endregion

		#region State

		public new abstract class state : IBqlField
		{
		}

		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "State")]
		[State(typeof (countryID), DescriptionField = typeof (State.name))]
		public override String State
		{
			get { return base.State; }
			set { base.State = value; }
		}

		#endregion

		#region PostalCode

		public new abstract class postalCode : IBqlField
		{
		}

		[PXDBString(20)]
		[PXUIField(DisplayName = "Postal Code")]
		[PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), CountryIdField = typeof(countryID))]
		public override String PostalCode
		{
			get { return base.PostalCode; }
			set { base.PostalCode = value; }
		}

		#endregion
	}

	#endregion
}
