using System;
using PX.Data;
using PX.Objects.CM;

namespace PX.Objects.CR
{
	[PXCacheName(Messages.ServiceCallItem)]
	[Serializable]
	public partial class CRServiceCaseItem : IBqlTable, IN.ILSPrimary, SO.IDiscountable
	{
		#region ServiceCaseID
		public abstract class serviceCaseID : IBqlField { }

		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(CRServiceCase.serviceCaseID))]
		[PXNavigateSelector(typeof(CRServiceCase.serviceCaseID))]
		[PXParent(typeof(Select<CRServiceCase,
			Where<CRServiceCase.serviceCaseID, Equal<Current<CRServiceCaseItem.serviceCaseID>>>>))]
		public virtual Int32? ServiceCaseID { get; set; }
		#endregion

		#region LineNbr
		public abstract class lineNbr : IBqlField { }

		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXLineNbr(typeof(CRServiceCase.lineCntr))]
		public virtual Int32? LineNbr { get; set; }
		#endregion

		#region InvtMult
		public abstract class invtMult : IBqlField { }

		[PXDBShort]
		[PXDefault((short)-1)]
		public virtual Int16? InvtMult { get; set; }
		#endregion

		#region InventoryID

		public abstract class inventoryID : IBqlField { }

		[PXDBInt]
		[PXDefault]
		[PXUIField(DisplayName = "Inventory ID")]
		[PXSelector(typeof(Search<IN.InventoryItem.inventoryID>),
			typeof(IN.InventoryItem.inventoryCD), typeof(IN.InventoryItem.descr),
			typeof(IN.InventoryItem.itemClassID), typeof(IN.InventoryItem.itemStatus),
			typeof(IN.InventoryItem.itemType))]
		public virtual Int32? InventoryID { get; set; }

		#endregion

		#region SubItemID
		public abstract class subItemID : IBqlField { }

		[IN.SubItem(typeof(CRServiceCaseItem.inventoryID))]
		public virtual Int32? SubItemID { get; set; }
		#endregion

		#region Description
		public abstract class description : IBqlField { }

		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible)]
		public virtual String Description { get; set; }
		#endregion

		#region CustomerID
		public abstract class customerID : IBqlField { }

		[PXDBInt]
		[PXDefault(typeof(CRServiceCase.customerID))]
		public virtual Int32? CustomerID { get; set; }
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : IBqlField { }

		[PXDBLong]
		[CurrencyInfo(typeof(CRServiceCase.curyInfoID))]
		public virtual Int64? CuryInfoID { get; set; }

		public decimal? CuryLineAmt
		{
			get { return ActualCuryAmount; }
		}

		public decimal? CuryExtPrice
		{
			[PXDependsOnFields(typeof(curyUnitPrice),typeof(actualQuantity))]
			get { return (CuryUnitPrice ?? 0m)*(ActualQuantity ?? 0m); }
		}

		public decimal? CuryDiscAmt
		{
			get { return ActualCuryDiscountAmount; }
			set { ActualCuryDiscountAmount = value; }
		}

		public decimal? DiscPct
		{
			get { return Discount; }
			set { Discount = value; }
		}

		#endregion

		#region UOM
		public abstract class uOM : IBqlField { }

		[PXDefault(typeof(Search<IN.InventoryItem.salesUnit,
			Where<IN.InventoryItem.inventoryID, Equal<Current<CRServiceCaseItem.inventoryID>>>>))]
		[IN.INUnit(typeof(CRServiceCaseItem.inventoryID))]
		public virtual String UOM { get; set; }

		#endregion

		#region EstimatedQuantity
		public abstract class estimatedQuantity : IBqlField { }

		[IN.PXDBQuantity(typeof(CRServiceCaseItem.uOM), typeof(CRServiceCaseItem.estimatedBaseQuantity), MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Estimated Quantity", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? EstimatedQuantity { get; set; }
		#endregion

		#region EstimatedBaseQuantity
		public abstract class estimatedBaseQuantity : IBqlField { }

		[PXDBDecimal(6, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? EstimatedBaseQuantity { get; set; }
		#endregion

		#region ActualQuantity
		public abstract class actualQuantity : IBqlField { }

		[IN.PXDBQuantity(typeof(CRServiceCaseItem.uOM), typeof(CRServiceCaseItem.actualBaseQuantity), MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Quantity", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? ActualQuantity { get; set; }
		#endregion

		#region ActualBaseQuantity
		public abstract class actualBaseQuantity : IBqlField { }

		[PXDBDecimal(6, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ActualBaseQuantity { get; set; }
		#endregion

		#region UnassignedQty
		public abstract class unassignedQty : PX.Data.IBqlField { }

		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnassignedQty { get; set; }
		#endregion

		#region SiteID
		public abstract class siteID : IBqlField { }

		[IN.SiteAvail(typeof(CRServiceCaseItem.inventoryID), typeof(CRServiceCaseItem.subItemID))]
		[PXDefault(typeof(Search2<Location.cSiteID,
			InnerJoin<IN.InventoryItem,
				On<IN.InventoryItem.inventoryID, Equal<Current<CRServiceCaseItem.inventoryID>>,
					And<IN.InventoryItem.stkItem, Equal<True>>>>,
			Where<Location.bAccountID, Equal<Current<CRServiceCase.customerID>>,
				And<Location.locationID, Equal<Current<CRServiceCase.locationID>>>>>))]
		public virtual Int32? SiteID { get; set; }
		#endregion

		#region LocationID
		public abstract class locationID : IBqlField { }

		[IN.LocationAvail(typeof(CRServiceCaseItem.inventoryID), typeof(CRServiceCaseItem.subItemID),
			typeof(CRServiceCaseItem.siteID), typeof(CRServiceCaseItem.tranType), typeof(CRServiceCaseItem.invtMult))]
		[PXDefault]
		public virtual Int32? LocationID { get; set; }
		#endregion

		#region LotSerialNbr
		public abstract class lotSerialNbr : IBqlField { }

		[IN.INLotSerialNbr(typeof(CRServiceCaseItem.inventoryID), typeof(CRServiceCaseItem.subItemID), typeof(CRServiceCaseItem.locationID),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String LotSerialNbr { get; set; }
		#endregion

		#region CuryUnitPrice
		public abstract class curyUnitPrice : IBqlField { }

		[PXDBCurrency(typeof(Search<IN.INSetup.decPlPrcCst>), typeof(CRServiceCaseItem.curyInfoID), typeof(CRServiceCaseItem.unitPrice), MinValue = 0)]
		[PXDefault(typeof(Search<IN.InventoryItem.basePrice,
			Where<IN.InventoryItem.inventoryID, Equal<Current<CRServiceCaseItem.inventoryID>>>>))]
		[PXUIField(DisplayName = "Unit Price", Enabled = false)]
		public virtual Decimal? CuryUnitPrice { get; set; }
		#endregion

		#region UnitPrice
		public abstract class unitPrice : IBqlField { }

		[IN.PXDBPriceCost]
		public virtual Decimal? UnitPrice { get; set; }
		#endregion

		#region IsFree
		public abstract class isFree : IBqlField { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Free Item")]
		public virtual Boolean? IsFree { get; set; }
		#endregion

		#region DetDiscIDC1
		public abstract class detDiscIDC1 : IBqlField { }

		[PXDBString(10, IsUnicode = true)]
		public virtual String DetDiscIDC1 { get; set; }
		#endregion

		#region DetDiscSeqIDC1
		public abstract class detDiscSeqIDC1 : IBqlField { }

		[PXDBString(10, IsUnicode = true)]
		public virtual String DetDiscSeqIDC1 { get; set; }
		#endregion

		#region DetDiscIDC2
		public abstract class detDiscIDC2 : IBqlField { }

		[PXDBString(10, IsUnicode = true)]
		public virtual String DetDiscIDC2 { get; set; }
		#endregion

		#region DetDiscSeqIDC2
		public abstract class detDiscSeqIDC2 : IBqlField { }

		[PXDBString(10, IsUnicode = true)]
		public virtual String DetDiscSeqIDC2 { get; set; }
		#endregion

		#region DetDiscApp
		public abstract class detDiscApp : IBqlField { }

		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? DetDiscApp { get; set; }
		#endregion

		#region PromoDiscID
		public virtual string PromoDiscID
		{
			get
			{
				return null;
			}
			set
			{ ;}
		}
		#endregion

		#region DocDiscIDC1
		public abstract class docDiscIDC1 : IBqlField { }

		[PXDBString(10, IsUnicode = true)]
		public virtual String DocDiscIDC1 { get; set; }
		#endregion

		#region DocDiscSeqIDC1
		public abstract class docDiscSeqIDC1 : IBqlField { }

		[PXDBString(10, IsUnicode = true)]
		public virtual String DocDiscSeqIDC1 { get; set; }
		#endregion

		#region DocDiscIDC2
		public abstract class docDiscIDC2 : IBqlField { }

		[PXDBString(10, IsUnicode = true)]
		public virtual String DocDiscIDC2 { get; set; }
		#endregion

		#region DocDiscSeqIDC2
		public abstract class docDiscSeqIDC2 : IBqlField { }

		[PXDBString(10, IsUnicode = true)]
		public virtual String DocDiscSeqIDC2 { get; set; }
		#endregion

		#region Discount
		public abstract class discount : IBqlField { }

		[PXDBDecimal(6, MinValue = 0, MaxValue = 100)]
		[PXUIField(DisplayName = "Discount, %")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? Discount { get; set; }
		#endregion

		#region EstimatedCuryDiscountAmount
		public abstract class estimatedCuryDiscountAmount : IBqlField { }

		[PXDBCurrency(typeof(Search<IN.INSetup.decPlPrcCst>), typeof(CRServiceCaseItem.curyInfoID), typeof(CRServiceCaseItem.estimatedDiscountAmount), MinValue = 0)]
		[PXUIField(DisplayName = "Estimated Discount Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? EstimatedCuryDiscountAmount { get; set; }
		#endregion

		#region EstimatedDiscountAmount
		public abstract class estimatedDiscountAmount : IBqlField { }

		[PXDBDecimal(4, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? EstimatedDiscountAmount { get; set; }
		#endregion

		#region ManualDiscount
		public abstract class manualDiscount : IBqlField { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Manual Discount", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? ManualDiscount { get; set; }

		public bool? ManualDisc
		{
			get { return ManualDiscount; }
		}
		#endregion

		#region ActualCuryDiscountAmount
		public abstract class actualCuryDiscountAmount : IBqlField { }

		[PXDBCurrency(typeof(Search<IN.INSetup.decPlPrcCst>), typeof(CRServiceCaseItem.curyInfoID), typeof(CRServiceCaseItem.actualDiscountAmount), MinValue = 0)]
		[PXUIField(DisplayName = "Actual Discount Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ActualCuryDiscountAmount { get; set; }
		#endregion

		#region ActualDiscountAmount
		public abstract class actualDiscountAmount : IBqlField { }

		[PXDBDecimal(4, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ActualDiscountAmount { get; set; }
		#endregion

		#region EstimatedCuryAmount
		public abstract class estimatedCuryAmount : IBqlField { }

		[PXDBCurrency(typeof(CRServiceCaseItem.curyInfoID), typeof(CRServiceCaseItem.estimatedAmount), MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Estimated Amount", Enabled = false)]
		public virtual Decimal? EstimatedCuryAmount { get; set; }
		#endregion

		#region EstimatedAmount
		public abstract class estimatedAmount : IBqlField { }

		[PXDBDecimal(4, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? EstimatedAmount { get; set; }
		#endregion

		#region ManualPrice
		public abstract class manualPrice : IBqlField { }

		[PXDBBool]
		[PXDefault(false)]
		public virtual Boolean? ManualPrice { get; set; }
		#endregion

		#region ActualCuryAmount
		public abstract class actualCuryAmount : IBqlField { }

		[PXDBCurrency(typeof(CRServiceCaseItem.curyInfoID), typeof(CRServiceCaseItem.actualAmount), MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Amount")]
		public virtual Decimal? ActualCuryAmount { get; set; }
		#endregion

		#region ActualAmount
		public abstract class actualAmount : IBqlField { }

		[PXDBDecimal(4, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Amount", Enabled = false)]
		public virtual Decimal? ActualAmount { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : IBqlField { }

		[PXNote]
		public virtual Int64? NoteID { get; set; }
		#endregion

		#region TranType
		public abstract class tranType : IBqlField { }

		[PXString]
		[PXStringList(new string[] { IN.INTranType.Invoice }, new string[] { IN.Messages.Invoice })]
		[PXUIField(Enabled = false)]
		public virtual String TranType
		{
			get
			{
				return IN.INTranType.Invoice;
			}
		}
		#endregion

		#region TranDate
		public virtual DateTime? TranDate
		{
			get { return this._CreatedDateTime; }
		}
		#endregion

		#region ExpireDate
		public abstract class expireDate : IBqlField { }

		[IN.INExpireDate(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual DateTime? ExpireDate { get; set; }
		#endregion

		#region TaxCategoryID
		public abstract class taxCategoryID : IBqlField { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TX.TaxCategory.taxCategoryID), DescriptionField = typeof(TX.TaxCategory.descr))]
		[PXDefault(typeof(Search<IN.InventoryItem.taxCategoryID,
			Where<IN.InventoryItem.inventoryID, Equal<Current<CRServiceCaseItem.inventoryID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing, SearchOnDefault = false)]
		/*[CRServiceCaseTax(CuryDocBal = typeof(CRServiceCase.estimatedCuryItemAmount),
			CuryLineTotal = typeof(CRServiceCase.estimatedCuryLineTotal),
			CuryTranAmt = typeof(CRServiceCaseItem.estimatedCuryAmount),
			CuryTaxTotal = typeof(CRServiceCase.estimatedCuryTaxTotal))]*/
		[CRServiceCaseTax()]
		public virtual String TaxCategoryID { get; set; }
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
		[PXDBCreatedDateTimeUtc]
		[PXUIField(DisplayName = "Service Call Date", Enabled = false)]
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
		[PXDBLastModifiedDateTimeUtc]
		[PXUIField(DisplayName = "Last Activity", Enabled = false)]
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

		#region ILSPrimary

		public decimal? Qty
		{
			get { return ActualQuantity; }
			set { ActualQuantity = value; }
		}

		public decimal? BaseQty
		{
			get { return ActualBaseQuantity; }
			set { ActualBaseQuantity = value; }
		}

		#endregion
	}
}
