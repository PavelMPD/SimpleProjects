namespace SW.Objects.CS
{
	using System;
	using SW.Data;
	using SW.Objects.GL;
	using SW.Objects.TX;

	class SWLocationIDAttribute : SWDBIntAsStringAttribute 
	{
		public SWLocationIDAttribute() : base(6) { }
	}

	[System.SerializableAttribute()]
	public class Location : SW.Data.IBqlTable
	{
		#region BAccountID
		public abstract class bAccountID : SW.Data.IBqlField
		{
		}
		protected Int32? _BAccountID;
		[SWDBInt(IsKey = true)]
		[SWDBLiteDefault(typeof(BAccount.bAccountID))]
		[SWUIField(DisplayName = "Business Account ID", Visible = false, Enabled = false)]
		[SWParent(typeof(Select<BAccount,
			Where<BAccount.bAccountID,
			Equal<Current<Location.bAccountID>>>>)
			)]
		public virtual Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : SW.Data.IBqlField
		{
		}
		protected Int32? _LocationID;
		//[SWLocationID(IsKey = true)]
		[SWDBIdentity()]
		[SWUIField(Visible = false,Enabled=false)]
		//[SWSelector(typeof(Search<Location.locationID,Where<Location.bAccountID,Equal<Current<Location.bAccountID>>>>))]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region LocationCD
		public abstract class locationCD : SW.Data.IBqlField
		{
		}
		protected String _LocationCD;
		[LocationRaw(typeof(Where<Location.bAccountID, Equal<Current<Location.bAccountID>>>), IsKey = true, Visibility = SWUIVisibility.SelectorVisible, DisplayName = "LocationID")]
		[SWDefault(PersistingCheck =SWPersistingCheck.NullOrBlank)]
		public virtual String LocationCD
		{
			get
			{
				return this._LocationCD;
			}
			set
			{
				this._LocationCD = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : SW.Data.IBqlField
		{
		}
		protected String _Descr;
		[SWDBString(60,IsUnicode = true)]
		[SWUIField(DisplayName = "Location Name", Visibility = SWUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : SW.Data.IBqlField
		{
		}
		protected String _TaxZoneID;
		[SWDBString(10)]
		[SWDefault(typeof(Search<BAccount.taxZoneID, Where<BAccount.bAccountID,Equal<Current<Location.bAccountID>>>>),PersistingCheck=SWPersistingCheck.Nothing)]
		[SWUIField(DisplayName = "Tax Zone ID")]
		[SWSelector(typeof(Search<TaxZone.taxZoneID>), CacheGlobal = true)]
		public virtual String TaxZoneID
		{
			get
			{
				return this._TaxZoneID;
			}
			set
			{
				this._TaxZoneID = value;
			}
		}
		#endregion
		#region TaxRegistrationID
		public abstract class taxRegistrationID : SW.Data.IBqlField
		{
		}
		protected String _TaxRegistrationID;
		[SWDBString(50)]
		[SWUIField(DisplayName = "Tax Registration ID")]
		public virtual String TaxRegistrationID
		{
			get
			{
				return this._TaxRegistrationID;
			}
			set
			{
				this._TaxRegistrationID = value;
			}
		}
		#endregion
		#region DefAddressID
		public abstract class defAddressID : SW.Data.IBqlField
		{
		}
		protected Int32? _DefAddressID;
		[SWDBInt()]
		[SWDBChildIdentity(typeof(Address.addressID))]
		[SWUIField(DisplayName = "Default Address")]
		[SWSelector(typeof(Search<Address.addressID,
			Where<Address.bAccountID,
			Equal<Current<BAccount.bAccountID>>>>),
			DirtyRead = true)]
		public virtual Int32? DefAddressID
		{
			get
			{
				return this._DefAddressID;
			}
			set
			{
				this._DefAddressID = value;
			}
		}
		#endregion
		#region BaseContactID
		public abstract class baseContactID : SW.Data.IBqlField
		{
		}
		protected Int32? _BaseContactID;
		[SWDBInt()]
		[SWUIField(DisplayName = "Default Contact")]
		[SWSelector(typeof(Search<Contact.contactID,
			Where<Contact.bAccountID,
			Equal<Current<Location.bAccountID>>,
			And<Contact.contactType, Equal<BQLConstants.contactTypePerson>>>>),
			DirtyRead = true)]
		public virtual Int32? BaseContactID
		{
			get
			{
				return this._BaseContactID;
			}
			set
			{
				this._BaseContactID = value;
			}
		}
		#endregion
		#region DefContactID
		public abstract class defContactID : SW.Data.IBqlField
		{
		}
		protected Int32? _DefContactID;
		[SWDBInt()]
		[SWDBChildIdentity(typeof(Contact.contactID))]

		public virtual Int32? DefContactID
		{
			get
			{
				return this._DefContactID;
			}
			set
			{
				this._DefContactID = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : SW.Data.IBqlField
		{
		}
		protected Int64? _NoteID;
		[SWNote()]
		public virtual Int64? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region IsActive
		public abstract class isActive : SW.Data.IBqlField
		{
		}
		protected bool? _IsActive;
		[SWDBBool()]
		[SWDefault(true)]
		[SWUIField(DisplayName = "Active")]
		public virtual bool? IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				this._IsActive = value;
			}
		}
		#endregion
		#region ExpenseAcctID
		public abstract class expenseAcctID : SW.Data.IBqlField
		{
		}
		protected Int32? _ExpenseAcctID;
		[SWDefault(typeof(Search<AP.Vendor.expenseAcctID, Where<AP.Vendor.bAccountID, Equal<Current<Location.bAccountID>>>>), PersistingCheck = SWPersistingCheck.Nothing)]
		[Account(DisplayName = "Expense Account", Visibility = SWUIVisibility.Visible, DescriptionField = typeof(Account.description), Visible = false)]
		public virtual Int32? ExpenseAcctID
		{
			get
			{
				return this._ExpenseAcctID;
			}
			set
			{
				this._ExpenseAcctID = value;
			}
		}
		#endregion
		#region ExpenseSubID
		public abstract class expenseSubID : SW.Data.IBqlField
		{
		}
		protected Int32? _ExpenseSubID;
		[SWDefault(typeof(Search<AP.Vendor.expenseSubID, Where<AP.Vendor.bAccountID, Equal<Current<Location.bAccountID>>>>),PersistingCheck=SWPersistingCheck.Nothing)]
		[SubAccount(typeof(Location.expenseAcctID), DisplayName = "Expense Sub", Visibility = SWUIVisibility.Visible, DescriptionField = typeof(Sub.description), Visible = false)]
		public virtual Int32? ExpenseSubID
		{
			get
			{
				return this._ExpenseSubID;
			}
			set
			{
				this._ExpenseSubID = value;
			}
		}
		#endregion
		#region CarrierID
		public abstract class carrierID : SW.Data.IBqlField
		{
		}
		protected String _CarrierID;
		[SWDBString(15)]
		[SWUIField(DisplayName = "Ship Via")]
		[SWSelector(typeof(Search<Carrier.carrierID>), CacheGlobal = true,DescriptionField = typeof(Carrier.description))]
		public virtual String CarrierID
		{
			get
			{
				return this._CarrierID;
			}
			set
			{
				this._CarrierID = value;
			}
		}
		#endregion
		#region ShipTermsID
		public abstract class shipTermsID : SW.Data.IBqlField
		{
		}
		protected String _ShipTermsID;
		[SWDBString(10)]
		[SWUIField(DisplayName = "Ship Terms")]
		[SWSelector(typeof(Search<ShipTerms.shipTermsID>),CacheGlobal=true,DescriptionField = typeof(ShipTerms.description))]
		public virtual String ShipTermsID
		{
			get
			{
				return this._ShipTermsID;
			}
			set
			{
				this._ShipTermsID = value;
			}
		}
		#endregion
		#region FOBPointID
		public abstract class fOBPointID : SW.Data.IBqlField
		{
		}
		protected String _FOBPointID;
		[SWDBString(15)]
		[SWUIField(DisplayName = "FOB Point")]
		[SWSelector(typeof(FOBPoint.fOBPointID), CacheGlobal = true, DescriptionField = typeof(FOBPoint.description))]
		public virtual String FOBPointID
		{
			get
			{
				return this._FOBPointID;
			}
			set
			{
				this._FOBPointID = value;
			}
		}
		#endregion
		#region LeadTime
		public abstract class leadTime : SW.Data.IBqlField
		{
		}
		protected Int16? _LeadTime;
		[SWDBShort(MinValue=0,MaxValue=100000)]
		[SWUIField(DisplayName = "Lead Time (days)", Visible = false)]
		public virtual Int16? LeadTime
		{
			get
			{
				return this._LeadTime;
			}
			set
			{
				this._LeadTime = value;
			}
		}
		#endregion
		#region WarehouseLocationID
		public abstract class warehouseLocationID : SW.Data.IBqlField
		{
		}
		protected Int32? _WarehouseLocationID;
		[SWDBInt()]
		[SWUIField(DisplayName = "Reciv. Location ID", Visible = false)]
		[SWSelector(typeof(Search2<Location.locationID,
						InnerJoin<Company,On<Location.bAccountID,Equal<Company.bAccountID>>>>),
						SubstituteKey = typeof(Location.locationCD),
						DescriptionField = typeof(Location.descr))]
		public virtual Int32? WarehouseLocationID
		{
			get
			{
				return this._WarehouseLocationID;
			}
			set
			{
				this._WarehouseLocationID = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : SW.Data.IBqlField
		{
		}
		protected Byte[] _tstamp;
		[SWDBTimestamp()]
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
		public abstract class createdByID : SW.Data.IBqlField
		{
		}
		protected Guid? _CreatedByID;
		[SWDBCreatedByID()]
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
		public abstract class createdByScreenID : SW.Data.IBqlField
		{
		}
		protected String _CreatedByScreenID;
		[SWDBCreatedByScreenID()]
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
		public abstract class createdDateTime : SW.Data.IBqlField
		{
		}
		protected DateTime? _CreatedDateTime;
		[SWDBCreatedDateTime()]
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
		public abstract class lastModifiedByID : SW.Data.IBqlField
		{
		}
		protected Guid? _LastModifiedByID;
		[SWDBLastModifiedByID()]
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
		public abstract class lastModifiedByScreenID : SW.Data.IBqlField
		{
		}
		protected String _LastModifiedByScreenID;
		[SWDBLastModifiedByScreenID()]
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
		public abstract class lastModifiedDateTime : SW.Data.IBqlField
		{
		}
		protected DateTime? _LastModifiedDateTime;
		[SWDBLastModifiedDateTime()]
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
