using PX.Objects.GL;

namespace PX.Objects.PO
{
	using System;
	using PX.Data;
	using PX.Objects.IN;
	using PX.Objects.CM;
	using PX.Objects.AP;
	using PX.Objects.CS;
	using PX.Objects.CR;

	[System.SerializableAttribute()]
    [PXCacheName(Messages.InventoryItemVendorDetails)]
	public partial class POVendorInventory : PX.Data.IBqlTable
	{
		#region RecordID
		public abstract class recordID : PX.Data.IBqlField
		{
		}
		protected Int32? _RecordID;
		[PXDBIdentity(IsKey=true)]
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
		[VendorNonEmployeeActive(Visibility = PXUIVisibility.SelectorVisible,
			DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true, DisplayName = "Vendor ID")]
		[PXDefault(typeof(Vendor.bAccountID))]
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
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<POVendorInventory.vendorID>>>),				
			DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location")]
		[PXFormula(typeof(Default<POVendorInventory.vendorID>))]
		[PXParent(typeof(Select<Location, Where<Location.bAccountID, Equal<Current<POVendorInventory.vendorID>>,
												And<Location.locationID, Equal<Current<POVendorInventory.vendorLocationID>>>>>))]		
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
		#region AllLocations
		public abstract class allLocations : PX.Data.IBqlField
		{
		}
		protected Boolean? _AllLocations;
		[PXBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "All Locations")]
		[PXDBCalced(typeof(Switch<Case<Where<POVendorInventory.vendorLocationID, IsNull>, True>, False>), typeof(bool))]
		public virtual Boolean? AllLocations
		{
			get
			{
				return this._AllLocations;
			}
			set
			{
				this._AllLocations = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[Inventory(Filterable = true, DirtyRead = true, Enabled = false)]
		[PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<POVendorInventory.inventoryID>>>>))]		
		[PXDBLiteDefault(typeof(InventoryItem.inventoryID))]
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
		[SubItem(typeof(POVendorInventory.inventoryID), DisplayName = "Subitem")]		
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
		#region PurchaseUnit
		public abstract class purchaseUnit : PX.Data.IBqlField
		{
		}
		protected String _PurchaseUnit;
		[PXDefault(typeof(Search<InventoryItem.purchaseUnit, Where<InventoryItem.inventoryID, Equal<Current<POVendorInventory.inventoryID>>>>))]
		[PXFormula(typeof(Default<POVendorInventory.inventoryID>))]
		[INUnit(typeof(POVendorInventory.inventoryID), DisplayName = "Purchase Unit", Visibility = PXUIVisibility.Visible)]
		[PXCheckUnique(typeof(POVendorInventory.vendorID), typeof(POVendorInventory.vendorLocationID), typeof(POVendorInventory.inventoryID), typeof(POVendorInventory.subItemID), typeof(POVendorInventory.purchaseUnit), IgnoreNulls = false, ClearOnDuplicate = true)]
		public virtual String PurchaseUnit
		{
			get
			{
				return this._PurchaseUnit;
			}
			set
			{
				this._PurchaseUnit = value;
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
		#region VLeadTime
		public abstract class vLeadTime : PX.Data.IBqlField
		{
		}
		protected Int16? _VLeadTime;
		[PXShort(MinValue = 0, MaxValue = 100000)]
		[PXUIField(DisplayName = "Vendor Lead Time (Days)", Enabled=false)]
		[PXDBScalar(typeof(Search<Location.vLeadTime, 
			Where<Location.bAccountID, Equal<POVendorInventory.vendorID>,
			  And<Location.locationID, Equal<POVendorInventory.vendorLocationID>>>>))]
		public virtual Int16? VLeadTime
		{
			get
			{
				return this._VLeadTime;
			}
			set
			{
				this._VLeadTime = value;
			}
		}
		#endregion
		#region OverrideSettings
		public abstract class overrideSettings : PX.Data.IBqlField
		{
		}
		protected Boolean? _OverrideSettings;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override")]
		public virtual Boolean? OverrideSettings
		{
			get
			{
				return this._OverrideSettings;
			}
			set
			{
				this._OverrideSettings = value;
			}
		}
		#endregion
		#region AddLeadTimeDays
		public abstract class addLeadTimeDays : PX.Data.IBqlField
		{
		}
		protected Int16? _AddLeadTimeDays;
		[PXDefault((short)0)]
		[PXDBShort()]
		[PXUIField(DisplayName = "Add. Lead Time (Days)")]
		public virtual Int16? AddLeadTimeDays
		{
			get
			{
				return this._AddLeadTimeDays;
			}
			set
			{
				this._AddLeadTimeDays = value;
			}
		}
		#endregion		
		#region Active
		public abstract class active : PX.Data.IBqlField
		{
		}
		protected Boolean? _Active;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? Active
		{
			get
			{
				return this._Active;
			}
			set
			{
				this._Active = value;
			}
		}
		#endregion
		#region MinOrdFreq
		public abstract class minOrdFreq : PX.Data.IBqlField
		{
		}
		protected Int32? _MinOrdFreq;
		[PXDBInt()]
		[PXUIField(DisplayName = "Min. Order Freq.(Days)")]
		[PXDefault(0)]
		public virtual Int32? MinOrdFreq
		{
			get
			{
				return this._MinOrdFreq;
			}
			set
			{
				this._MinOrdFreq = value;
			}
		}
		#endregion
		#region MinOrdQty
		public abstract class minOrdQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _MinOrdQty;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Min. Order Qty.")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? MinOrdQty
		{
			get
			{
				return this._MinOrdQty;
			}
			set
			{
				this._MinOrdQty = value;
			}
		}
		#endregion
		#region MaxOrdQty
		public abstract class maxOrdQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _MaxOrdQty;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Max Order Qty.")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? MaxOrdQty
		{
			get
			{
				return this._MaxOrdQty;
			}
			set
			{
				this._MaxOrdQty = value;
			}
		}
		#endregion
		#region LotSize
		public abstract class lotSize : PX.Data.IBqlField
		{
		}
		protected Decimal? _LotSize;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Lot Size")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LotSize
		{
			get
			{
				return this._LotSize;
			}
			set
			{
				this._LotSize = value;
			}
		}
		#endregion
		#region ERQ
		public abstract class eRQ : PX.Data.IBqlField
		{
		}
		protected Decimal? _ERQ;
		[PXDBQuantity]
		[PXUIField(DisplayName = "EOQ")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ERQ
		{
			get
			{
				return this._ERQ;
			}
			set
			{
				this._ERQ = value;
			}
		}
		#endregion		
		#region PendingPrice
		public abstract class pendingPrice : PX.Data.IBqlField
		{
		}
		protected Decimal? _PendingPrice;
		[PXDBPriceCost()]
		[PXUIField(DisplayName = "Pending Vendor Price")]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		#region PendingDate
		public abstract class pendingDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _PendingDate;
		[PXDBDate()]		
		[PXUIField(DisplayName = "Pending Price Date")]
		[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual DateTime? PendingDate
		{
			get
			{
				return this._PendingDate;
			}
			set
			{
				this._PendingDate = value;
			}
		}
		#endregion
		#region EffPrice
		public abstract class effPrice : PX.Data.IBqlField
		{
		}
		protected Decimal? _EffPrice;
		[PXDBPriceCost()]
		[PXUIField(DisplayName = "Current Vendor Price", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? EffPrice
		{
			get
			{
				return this._EffPrice;
			}
			set
			{
				this._EffPrice = value;
			}
		}
		#endregion
		#region EffDate
		public abstract class effDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _EffDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Effective Date", Enabled = false)]
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
		#region LastPrice
		public abstract class lastPrice : PX.Data.IBqlField
		{
		}
		protected Decimal? _LastPrice;
		[PXDBPriceCost()]
		[PXUIField(DisplayName = "Last Vendor Price", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		#region CuryID
		public abstract class curyID : PX.Data.IBqlField
		{
		}
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true)]
		[PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
		[PXDefault(typeof(Coalesce<
			Search<Vendor.curyID, Where<Vendor.bAccountID, Equal<Current<POVendorInventory.vendorID>>>>,
			Search<Company.baseCuryID>>), PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXFormula(typeof(Default<POVendorInventory.vendorID>))]
		[PXUIField(DisplayName = "Currency ID")]
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
		#region IsDefault
		public abstract class isDefault : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsDefault;
		[PXBool()]
		[PXUIField(DisplayName = "Default", Enabled=false)]
		[PODefaultVendor(typeof(POVendorInventory.inventoryID), typeof(POVendorInventory.vendorID), typeof(POVendorInventory.vendorLocationID))]
		public virtual Boolean? IsDefault
		{
			get
			{
				return this._IsDefault;
			}
			set
			{
				this._IsDefault = value;
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

		#region EffectivePriceValue
		public abstract class effectivePriceValue : PX.Data.IBqlField
		{
		}
		protected Decimal? _EffectivePriceValue;
		[PXDecimal()]
		[PXDBCalced(typeof(Switch<
			Case<Where<POVendorInventory.effPrice, IsNull>, decimalMax,
			Case<Where<POVendorInventory.effPrice, LessEqual<decimal0>>, decimalMax>>,
			POVendorInventory.effPrice>), typeof(decimal))]
		public virtual Decimal? EffectivePriceValue
		{
			get
			{
				return this._EffectivePriceValue;
			}
			set
			{
				this._EffectivePriceValue = value;
			}
		}
		#endregion		
	}

    [Serializable]
	public class POVendorInventoryU : POVendorInventory
	{
		#region RecordID
		public new abstract class recordID : IBqlField { }
		#endregion
		#region VendorID
		public new abstract class vendorID : IBqlField { }
		#endregion
		#region VendorLocationID
		public new abstract class vendorLocationID : IBqlField { }
		#endregion
		#region InventoryID
		public new abstract class inventoryID : IBqlField { }
		#endregion
		#region SubItemID
		public new abstract class subItemID : IBqlField { }
		#endregion
		#region PurchaseUnit
		public new class purchaseUnit : PX.Data.IBqlField
		{
		}
		[PXDefault(typeof(Search<InventoryItem.purchaseUnit, Where<InventoryItem.inventoryID, Equal<Current<POVendorInventory.inventoryID>>>>))]
		[INUnit(null, typeof(InventoryItem.baseUnit), DisplayName = "Purchase Unit", Visibility = PXUIVisibility.Visible)]
		[PXCheckUnique(typeof(POVendorInventory.vendorID), typeof(POVendorInventory.vendorLocationID), typeof(POVendorInventory.inventoryID), typeof(POVendorInventory.purchaseUnit))]
		public override String PurchaseUnit
		{
			get
			{
				return this._PurchaseUnit;
			}
			set
			{
				this._PurchaseUnit = value;
			}
		}
		#endregion	
	}

    [Serializable]
	public partial class POVendorPriceUpdate : IBqlTable
	{
		#region PendingDate
		public abstract class pendingDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _PendingDate;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Max. Pending Date")]
		public virtual DateTime? PendingDate
		{
			get
			{
				return this._PendingDate;
			}
			set
			{
				this._PendingDate = value;
			}
		}
		#endregion
	}
}
