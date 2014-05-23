namespace PX.Objects.AR
{
	using System;
	using PX.Data;
	using PX.Objects.IN;
	using PX.Objects.CM;
	using PX.Objects.TX;
	using PX.Objects.CS;

	[System.SerializableAttribute()]
	public partial class ARSalesPrice : PX.Data.IBqlTable
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
		#region CustPriceClassID
		public abstract class custPriceClassID : PX.Data.IBqlField
		{
		}
		protected String _CustPriceClassID;
		[PXDBString(10, InputMask = ">aaaaaaaaaa")]
		[PXDefault()]
		[PXUIField(DisplayName = "Customer Price Class", Visibility = PXUIVisibility.SelectorVisible)]
		[CustomerPriceClass]
		public virtual String CustPriceClassID
		{
			get
			{
				return this._CustPriceClassID;
			}
			set
			{
				this._CustPriceClassID = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.IBqlField
		{
		}
		protected Int32? _CustomerID;
		[Customer]
		[PXParent(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<ARSalesPrice.customerID>>>>))]
		public virtual Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
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
		[PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<ARSalesPrice.inventoryID>>>>))] 
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
		#region CuryID
		public abstract class curyID : PX.Data.IBqlField
		{
		}
		protected string _CuryID;
		[PXDBString(5)]
		[PXDefault()]
		[PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
		[PXUIField(DisplayName = "Currency")]
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
		[PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<ARSalesPrice.inventoryID>>>>))]
		[INUnit(typeof(ARSalesPrice.inventoryID))]
		//[PXCheckUnique(typeof(ARSalesPrice.custPriceClassID), typeof(ARSalesPrice.inventoryID), typeof(ARSalesPrice.curyID))]
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
		#region IsCustClassPrice
		public abstract class isCustClassPrice : IBqlField
		{
		}
		protected bool? _IsCustClassPrice;
		[PXDBBool]
		[PXDefault(true)]
		public virtual bool? IsCustClassPrice
		{
			get
			{
				return _IsCustClassPrice;
			}
			set
			{
				_IsCustClassPrice = value;
			}
		}
		#endregion
		#region EffectiveDate
		public abstract class effectiveDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _EffectiveDate;
		[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck=PXPersistingCheck.Nothing)]
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
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBPriceCost]
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
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBPriceCost]
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
		#region TaxID
		public abstract class taxID : PX.Data.IBqlField
		{
		}
		protected String _TaxID;
		[PXUIField(DisplayName = "Tax", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXSelector(typeof(Tax.taxID))]
		[PXDBString(Tax.taxID.Length)]
		public virtual String TaxID
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
		#region LastTaxID
		public abstract class lastTaxID : PX.Data.IBqlField
		{
		}
		protected String _LastTaxID;
		[PXUIField(DisplayName = "Last Tax", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXSelector(typeof(Tax.taxID))]
		[PXDBString(Tax.taxID.Length)]
		public virtual String LastTaxID
		{
			get
			{
				return this._LastTaxID;
			}
			set
			{
				this._LastTaxID = value;
			}
		}
		#endregion
		#region PendingTaxID
		public abstract class pendingTaxID : PX.Data.IBqlField
		{
		}
		protected String _PendingTaxID;
		[PXUIField(DisplayName = "Pending Tax", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Tax.taxID))]
		[PXDBString(Tax.taxID.Length)]
		public virtual String PendingTaxID
		{
			get
			{
				return this._PendingTaxID;
			}
			set
			{
				this._PendingTaxID = value;
			}
		}
		#endregion
		#region BreakQty
		public abstract class breakQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _BreakQty;
		[PXDBQuantity(MinValue=0)]
		[PXUIField(DisplayName = "Current Break Qty", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		#region BaseCuryID
		public abstract class baseCuryID : PX.Data.IBqlField
		{
		}
		protected string _BaseCuryID;
		[PXString(5)]
		[PXUIField(DisplayName = "Base Currency", Enabled = false)]
		public virtual string BaseCuryID
		{
			get
			{
				return this._BaseCuryID;
			}
			set
			{
				this._BaseCuryID = value;
			}
		}
		#endregion
		#region LastCost
		public abstract class lastCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _LastCost;
		[PXPriceCost()]
		[PXUIField(DisplayName = "Base Last Cost", Enabled = false)]
		public virtual Decimal? LastCost
		{
			get
			{
				return this._LastCost;
			}
			set
			{
				this._LastCost = value;
			}
		}
		#endregion
		#region AvgCost
		public abstract class avgCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _AvgCost;
		[PXPriceCost()]
		[PXUIField(DisplayName = "Base Average Cost", Enabled = false)]
		public virtual Decimal? AvgCost
		{
			get
			{
				return this._AvgCost;
			}
			set
			{
				this._AvgCost = value;
			}
		}
		#endregion
		#region MinGPPrice
		public abstract class minGPPrice : PX.Data.IBqlField
		{
		}
		protected Decimal? _MinGPPrice;
		[PXPriceCost()]
		[PXUIField(DisplayName = "Min. Gross Profit Price", Enabled = false)]
		public virtual Decimal? MinGPPrice
		{
			get
			{
				return this._MinGPPrice;
			}
			set
			{
				this._MinGPPrice = value;
			}
		}
		#endregion
		#region RecPrice
		public abstract class recPrice : PX.Data.IBqlField
		{
		}
		protected Decimal? _RecPrice;
		[PXPriceCost()]
		[PXUIField(DisplayName = "MSRP", Enabled = false)]
		public virtual Decimal? RecPrice
		{
			get
			{
				return this._RecPrice;
			}
			set
			{
				this._RecPrice = value;
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

	public partial class ARSalesPrice2 : ARSalesPrice
	{
		#region CustPriceClassID
		public new abstract class custPriceClassID : PX.Data.IBqlField
		{
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryID
		public new abstract class curyID : PX.Data.IBqlField
		{
		}
		#endregion
		#region UOM
		public new abstract class uOM : PX.Data.IBqlField
		{
		}
		#endregion
		#region BreakQty
		public new abstract class breakQty : PX.Data.IBqlField
		{
		}
		#endregion
		#region SalesPrice
		public new abstract class salesPrice : PX.Data.IBqlField
		{
		}
		#endregion
		#region LastBreakQty
		public new abstract class lastBreakQty : PX.Data.IBqlField
		{
		}
		#endregion
		#region LastPrice
		public new abstract class lastPrice : PX.Data.IBqlField
		{
		}
		#endregion
	}


	[PXProjection(typeof(Select2<ARSalesPrice,
			InnerJoin<InventoryItem, On<ARSalesPrice.inventoryID, Equal<InventoryItem.inventoryID>>,
			LeftJoin<ARPriceClass, On<ARSalesPrice.custPriceClassID, Equal<ARPriceClass.priceClassID>>>>>))]
    [Serializable]
	public partial class ARSalesPriceEx : ARSalesPrice
	{
		#region InventoryCD
		public abstract class inventoryCD : PX.Data.IBqlField
		{
		}
		protected string _InventoryCD;
		[PXDefault()]
		[PXDBString(InputMask = "", BqlField = typeof(InventoryItem.inventoryCD))]
		[PXUIField(DisplayName = "Inventory ID")]
		public virtual String InventoryCD
		{
			get
			{
				return this._InventoryCD;
			}
			set
			{
				this._InventoryCD = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected string _Descr;
		[PXDBString(60, IsUnicode = true, BqlField = typeof(InventoryItem.descr))]
		[PXUIField(DisplayName = "Description")]
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
		#region SortOrder
		public abstract class sortOrder : PX.Data.IBqlField
		{
		}
		protected Int16? _SortOrder;
		[PXDBShort(BqlField = typeof(AR.ARPriceClass.sortOrder))]
		[PXUIField(DisplayName = "Sort Order")]
		public virtual Int16? SortOrder
		{
			[PXDependsOnFields(typeof(custPriceClassID))]
			get
			{
				if (this._CustPriceClassID == ARPriceClass.EmptyPriceClass)
					return ARPriceClass.EmptyPriceClassSortOrder;
				else
					return this._SortOrder;
			}
			set
			{
				this._SortOrder = value;
			}
		}
		#endregion

	}

}
