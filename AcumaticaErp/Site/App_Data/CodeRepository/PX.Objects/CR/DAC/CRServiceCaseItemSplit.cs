using System;
using PX.Data;

namespace PX.Objects.CR
{
	[Serializable]
	[PXCacheName(Messages.CallDetailsSplit)]
	public partial class CRServiceCaseItemSplit : IBqlTable, IN.ILSDetail
	{
		#region ServiceCaseID
		public abstract class serviceCaseID : IBqlField { }

		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(CRServiceCaseItem.serviceCaseID))]
		[PXParent(typeof(Select<CRServiceCaseItem, 
			Where<CRServiceCaseItem.serviceCaseID, Equal<Current<CRServiceCaseItemSplit.serviceCaseID>>, 
				And<CRServiceCaseItem.lineNbr, Equal<Current<CRServiceCaseItemSplit.lineNbr>>>>>))]
		public virtual Int32? ServiceCaseID { get; set; }
		#endregion

		#region LineNbr
		public abstract class lineNbr : IBqlField { }

		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(CRServiceCaseItem.lineNbr))]
		public virtual Int32? LineNbr { get; set; }
		#endregion

		#region SplitLineNbr
		public abstract class splitLineNbr : IBqlField { }

		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXLineNbr(typeof(CRServiceCase.lineCntr))]
		public virtual Int32? SplitLineNbr { get; set; }
		#endregion

		#region InvtMult
		public abstract class invtMult : IBqlField { }

		[PXDBShort]
		[PXDefault((short)-1)]
		public virtual Int16? InvtMult { get; set; }
		#endregion

		#region InventoryID
		public abstract class inventoryID : IBqlField { }

		[IN.StockItem(Enabled = false, Visible = true)]
		[PXDefault(typeof(CRServiceCaseItem.inventoryID))]
		public virtual Int32? InventoryID { get; set; }
		#endregion

		#region IsStockItem
		public bool? IsStockItem
		{
			get
			{
				return true;
			}
			set { }
		}
		#endregion

		#region SiteID
		public abstract class siteID : IBqlField { }

		[IN.Site]
		[PXDefault(typeof(CRServiceCaseItem.siteID))]
		public virtual Int32? SiteID { get; set; }
		#endregion

		#region LocationID
		public abstract class locationID : IBqlField { }

		//TODO: need review
		[IN.LocationAvail(typeof(CRServiceCaseItemSplit.inventoryID), typeof(CRServiceCaseItemSplit.subItemID), typeof(CRServiceCaseItemSplit.siteID), typeof(CRServiceCaseItemSplit.tranType), typeof(CRServiceCaseItemSplit.invtMult))]
		[PXDefault]
		public virtual Int32? LocationID { get; set; }
		#endregion

		#region SubItemID
		public abstract class subItemID : PX.Data.IBqlField { }

		[IN.SubItem(typeof(CRServiceCaseItemSplit.inventoryID))]
		[PXDefault(typeof(Search<IN.InventoryItem.defaultSubItemID,
			Where<IN.InventoryItem.inventoryID, Equal<Current<CRServiceCaseItemSplit.inventoryID>>,
				And<IN.InventoryItem.defaultSubItemOnEntry, Equal<True>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<CRServiceCaseItemSplit.inventoryID>))]
		[IN.SubItemStatusVeryfier(typeof(CRServiceCaseItemSplit.inventoryID), typeof(CRServiceCaseItemSplit.siteID),
			IN.InventoryItemStatus.Inactive, IN.InventoryItemStatus.NoSales)]
		public virtual Int32? SubItemID { get; set; }
		#endregion

		#region LotSerialNbr
		public abstract class lotSerialNbr : IBqlField { }

		[IN.INLotSerialNbr(typeof(CRServiceCaseItemSplit.inventoryID), typeof(CRServiceCaseItemSplit.subItemID), typeof(CRServiceCaseItemSplit.locationID), typeof(CRServiceCaseItemSplit.lotSerialNbr))]
		public virtual String LotSerialNbr { get; set; }

		#endregion

		#region LotSerClassID
		public abstract class lotSerClassID : IBqlField { }

		[PXString(10, IsUnicode = true)]
		public virtual String LotSerClassID { get; set; }
		#endregion

		#region AssignedNbr
		public abstract class assignedNbr : IBqlField { }

		[PXString(30, IsUnicode = true)]
		public virtual String AssignedNbr { get; set; }
		#endregion

		#region ExpireDate
		public abstract class expireDate : IBqlField { }

		[IN.INExpireDate]
		public virtual DateTime? ExpireDate { get; set; }
		#endregion

		#region UOM
		public abstract class uOM : IBqlField { }

		[IN.INUnit(typeof(CRServiceCaseItemSplit.inventoryID), DisplayName = "UOM", Enabled = false)]
		[PXDefault(typeof(CRServiceCaseItem.uOM))]
		public virtual String UOM { get; set; }
		#endregion

		#region Qty
		public abstract class qty : IBqlField { }

		[IN.PXDBQuantity(typeof(CRServiceCaseItemSplit.uOM), typeof(CRServiceCaseItemSplit.baseQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity")]
		public virtual Decimal? Qty { get; set; }
		#endregion

		#region BaseQty
		public abstract class baseQty : IBqlField { }

		[PXDBDecimal(6)]
		public virtual Decimal? BaseQty { get; set; }
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



		public static implicit operator CRServiceCaseItemSplit(CRServiceCaseItem item)
		{
			return new CRServiceCaseItemSplit
					{
						ServiceCaseID = item.ServiceCaseID,
						LineNbr = item.LineNbr,
						SplitLineNbr = 1,
						InventoryID = item.InventoryID,
						SiteID = item.SiteID,
						SubItemID = item.SubItemID,
						LocationID = item.LocationID,
						LotSerialNbr = item.LotSerialNbr,
						ExpireDate = item.ExpireDate,
						Qty = item.Qty,
						UOM = item.UOM,
						BaseQty = item.BaseQty,
						InvtMult = item.InvtMult
					};
		}

		public static implicit operator CRServiceCaseItem(CRServiceCaseItemSplit item)
		{
			return new CRServiceCaseItem
					{
						ServiceCaseID = item.ServiceCaseID,
						LineNbr = item.LineNbr,
						InventoryID = item.InventoryID,
						SiteID = item.SiteID,
						SubItemID = item.SubItemID,
						LocationID = item.LocationID,
						LotSerialNbr = item.LotSerialNbr,
						Qty = item.Qty,
						BaseQty = item.BaseQty,
						UOM = item.UOM,
						InvtMult = item.InvtMult,
					};
		}
	}
}
