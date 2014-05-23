using PX.Objects.AP;
using PX.Objects.CS;

namespace PX.Objects.TX
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	[PXCacheName(Messages.TaxZone)]
	[PXPrimaryGraph(
		new Type[] { typeof(TaxZoneMaint)},
		new Type[] { typeof(Select<TaxZone, 
			Where<TaxZone.taxZoneID, Equal<Current<TaxZone.taxZoneID>>>>)
		})]
	public partial class TaxZone : PX.Data.IBqlTable
	{
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.IBqlField
		{
		}
		protected String _TaxZoneID;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Tax Zone ID",Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search3<TaxZone.taxZoneID, OrderBy<Asc<TaxZone.taxZoneID>>>), CacheGlobal = true)]
		[PX.Data.EP.PXFieldDescription]
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
		#region DfltTaxCategoryID
		public abstract class dfltTaxCategoryID : PX.Data.IBqlField
		{
		}
		protected String _DfltTaxCategoryID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Default Tax Category",Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(TaxCategory.taxCategoryID),DescriptionField = typeof(TaxCategory.descr))]
		public virtual String DfltTaxCategoryID
		{
			get
			{
				return this._DfltTaxCategoryID;
			}
			set
			{
				this._DfltTaxCategoryID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected String _Descr;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description",Visibility = PXUIVisibility.SelectorVisible)]
		[PX.Data.EP.PXFieldDescription]
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
		#region TaxVendorID
		public abstract class taxVendorID : PX.Data.IBqlField
		{
		}
		protected Int32? _TaxVendorID;
		[Vendor(typeof(Search<Vendor.bAccountID, Where<Vendor.taxAgency, Equal<boolTrue>>>), DisplayName = "Tax Agency ID", DescriptionField = typeof(Vendor.acctName))]
		public virtual Int32? TaxVendorID
		{
			get
			{
				return this._TaxVendorID;
			}
			set
			{
				this._TaxVendorID = value;
			}
		}
		#endregion
		#region IsImported
		public abstract class isImported : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsImported;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsImported
		{
			get
			{
				return this._IsImported;
			}
			set
			{
				this._IsImported = value;
			}
		}
		#endregion
		#region IsExternal
		public abstract class isExternal : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsExternal;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName="External Tax Provider")]
		public virtual Boolean? IsExternal
		{
			get
			{
				return this._IsExternal;
			}
			set
			{
				this._IsExternal = value;
			}
		}
		#endregion
		
		#region NoteID
		public abstract class noteID : IBqlField { }

		[PXNote(DescriptionField = typeof(TaxZone.taxZoneID),
			Selector = typeof(Search<TaxZone.taxZoneID>))]
		public virtual Int64? NoteID { get; set; }
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
