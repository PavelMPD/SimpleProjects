using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.TX;

namespace PX.Objects.AP
{
	[System.SerializableAttribute()]
	[PXBreakInheritance]
	public partial class APSalesPrice : PX.Data.IBqlTable
	{
		#region RecordID
		public abstract class recordID : PX.Data.IBqlField
		{
		}
		protected Int32? _RecordID;
		[PXDBIdentity(IsKey = true)]
		public virtual Int32? RecordID
		{
			get
			{
				return this._RecordID;
			}
			set
			{
				this._RecordID = value;
			}
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.IBqlField
		{
		}
		protected Int32? _VendorID;
		[Vendor]
		[PXParent(typeof(Select<Vendor, Where<Vendor.bAccountID, Equal<Current<APSalesPrice.vendorID>>>>))]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region VendorLocationID
		public abstract class vendorLocationID : PX.Data.IBqlField
		{
		}
		protected Int32? _VendorLocationID;
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<APSalesPrice.vendorID>>>),
			DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location")]
		[PXParent(typeof(Select<Location, Where<Location.bAccountID, Equal<Current<APSalesPrice.vendorID>>,
												And<Location.locationID, Equal<Current<APSalesPrice.vendorLocationID>>>>>))]	
		public virtual Int32? VendorLocationID
		{
			get
			{
				return this._VendorLocationID;
			}
			set
			{
				this._VendorLocationID = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[Inventory(DisplayName = "Inventory ID")]
		[PXDefault()]
		[PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<APSalesPrice.inventoryID>>>>))] 
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.IBqlField
		{
		}
		protected Int32? _SubItemID;
		[SubItem(typeof(APSalesPrice.inventoryID), DisplayName = "Subitem")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion		
		#region VendorInventoryID
		public abstract class vendorInventoryID : PX.Data.IBqlField
		{
		}
		protected String _VendorInventoryID;
		[PXDBString(50, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Vendor Inventory ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String VendorInventoryID
		{
			get
			{
				return this._VendorInventoryID;
			}
			set
			{
				this._VendorInventoryID = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.IBqlField
		{
		}
		protected string _CuryID;
		[PXDBString(5)]
		[PXDefault()]
		[PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
		[PXUIField(DisplayName = "Currency", Required=false, Enabled=false)]
		public virtual string CuryID
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
		#region UOM
		public abstract class uOM : PX.Data.IBqlField
		{
		}
		protected String _UOM;
		[PXDefault(typeof(Search<InventoryItem.purchaseUnit, Where<InventoryItem.inventoryID, Equal<Current<APSalesPrice.inventoryID>>>>))]
		[INUnit(typeof(APSalesPrice.inventoryID))]
		public virtual String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
			}
		}
		#endregion
		#region IsPromotionalPrice
		public abstract class isPromotionalPrice : IBqlField
		{
		}
		protected bool? _IsPromotionalPrice;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Promotional")]
		public virtual bool? IsPromotionalPrice
		{
			get
			{
				return _IsPromotionalPrice;
			}
			set
			{
				_IsPromotionalPrice = value;
			}
		}
		#endregion
		#region EffectiveDate
		public abstract class effectiveDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _EffectiveDate;
		[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDate()]
		[PXUIField(DisplayName = "Pending Price Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? EffectiveDate
		{
			get
			{
				return this._EffectiveDate;
			}
			set
			{
				this._EffectiveDate = value;
			}
		}
		#endregion
		#region LastDate
		public abstract class lastDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _LastDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Effective Date", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual DateTime? LastDate
		{
			get
			{
				return this._LastDate;
			}
			set
			{
				this._LastDate = value;
			}
		}
		#endregion
		#region SalesPrice
		public abstract class salesPrice : PX.Data.IBqlField
		{
		}
		protected Decimal? _SalesPrice;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBPriceCost]
		[PXUIField(DisplayName = "Current Price", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? SalesPrice
		{
			get
			{
				return this._SalesPrice;
			}
			set
			{
				this._SalesPrice = value;
			}
		}
		#endregion
		#region LastPrice
		public abstract class lastPrice : PX.Data.IBqlField
		{
		}
		protected Decimal? _LastPrice;
		[PXDBPriceCost]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last Price", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? LastPrice
		{
			get
			{
				return this._LastPrice;
			}
			set
			{
				this._LastPrice = value;
			}
		}
		#endregion
		#region PendingPrice
		public abstract class pendingPrice : PX.Data.IBqlField
		{
		}
		protected Decimal? _PendingPrice;
		[PXDBPriceCost]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Pending Price", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? PendingPrice
		{
			get
			{
				return this._PendingPrice;
			}
			set
			{
				this._PendingPrice = value;
			}
		}
		#endregion
		#region BreakQty
		public abstract class breakQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _BreakQty;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBQuantity(MinValue=0)]
		[PXUIField(DisplayName = "Current Break Qty", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? BreakQty
		{
			get
			{
				return this._BreakQty;
			}
			set
			{
				this._BreakQty = value;
			}
		}
		#endregion
		#region LastBreakQty
		public abstract class lastBreakQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _LastBreakQty;
		[PXDBQuantity(MinValue=0)]
		[PXUIField(DisplayName = "Last Break Qty", Visibility = PXUIVisibility.Visible, Enabled=false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LastBreakQty
		{
			get
			{
				return this._LastBreakQty;
			}
			set
			{
				this._LastBreakQty = value;
			}
		}
		#endregion
		#region PendingBreakQty
		public abstract class pendingBreakQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _PendingBreakQty;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBQuantity(MinValue=0)]
		[PXUIField(DisplayName = "Pending Break Qty", Visibility = PXUIVisibility.Visible, Enabled=true)]
		public virtual Decimal? PendingBreakQty
		{
			get
			{
				return this._PendingBreakQty;
			}
			set
			{
				this._PendingBreakQty = value;
			}
		}
		#endregion

		#region ExpirationDate
		public abstract class expirationDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _ExpirationDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Expiration Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? ExpirationDate
		{
			get
			{
				return this._ExpirationDate;
			}
			set
			{
				this._ExpirationDate = value;
			}
		}
		#endregion
		

		#region Selected
		public abstract class selected : IBqlField
		{
		}
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Service)]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region AllLocations
		public abstract class allLocations : IBqlField
		{
		}
		protected bool? _AllLocations = false;
		[PXBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "All Locations")]
		[PXDBCalced(typeof(Switch<Case<Where<APSalesPrice.vendorLocationID, IsNull>, True>, False>), typeof(bool))]
		public virtual bool? AllLocations
		{
			get
			{
				return _AllLocations;
			}
			set
			{
				_AllLocations = value;
			}
		}
		#endregion


		#region System Columns
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
		#endregion
	}
}
