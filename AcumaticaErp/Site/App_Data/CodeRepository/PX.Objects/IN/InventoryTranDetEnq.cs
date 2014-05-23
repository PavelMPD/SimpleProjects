using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using PX.SM;
using PX.Data;
using PX.Objects.BQLConstants;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.IN
{

	#region FilterDAC

	[Serializable]
	public partial class InventoryTranDetEnqFilter : PX.Data.IBqlTable
	{

		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		protected String _FinPeriodID;
		//[FinPeriodID]
		[FinPeriodSelector(typeof(AccessInfo.businessDate))]
		[PXDefault()]
		[PXUIField(DisplayName = "Period", Visibility = PXUIVisibility.Visible)]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion

		#region PeriodStartDate
		public abstract class periodStartDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _PeriodStartDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Period Start Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
		public virtual DateTime? PeriodStartDate
		{
			get
			{
				return this._PeriodStartDate;
			}
			set
			{
				this._PeriodStartDate = value;
			}
		}
		#endregion
		#region PeriodEndDate
		public abstract class periodEndDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _PeriodEndDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Period End Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
		public virtual DateTime? PeriodEndDate
		{
			get
			{
				return this._PeriodEndDate;
			}
			set
			{
				this._PeriodEndDate = value;
			}
		}
		#endregion

		#region PeriodEndDateInclusive
		public abstract class periodEndDateInclusive : PX.Data.IBqlField { };
		[PXDBDate()]
		[PXUIField(DisplayName = "Period End Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
		public virtual DateTime? PeriodEndDateInclusive
		{
			get
			{
				if (this._PeriodEndDate == null)
				{
					return null;
				}
				else
				{
					return ((DateTime)this._PeriodEndDate).AddDays(-1);
				}
			}
		}
		#endregion


		#region StartDate
		public abstract class startDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _StartDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Start Date")]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _EndDate;
		[PXDBDate()]
		//[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "End Date")]
		public virtual DateTime? EndDate
		{
			get
			{
				return this._EndDate;
			}
			set
			{
				this._EndDate = value;
			}
		}
		#endregion

		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[PXDefault()]
		[Inventory(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.stkItem, NotEqual<boolFalse>, And<Where<Match<Current<AccessInfo.userName>>>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))] // ??? zzz stock / nonstock ?
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

		#region SubItemCD
		public abstract class subItemCD : PX.Data.IBqlField
		{
		}
		protected String _SubItemCD;
		[SubItemRawExt(typeof(InventoryTranDetEnqFilter.inventoryID), DisplayName = "Subitem")]
		public virtual String SubItemCD
		{
			get
			{
				return this._SubItemCD;
			}
			set
			{
				this._SubItemCD = value;
			}
		}
		#endregion
		#region SubItemCD Wildcard
		public abstract class subItemCDWildcard : PX.Data.IBqlField { };
		[PXDBString(30, IsUnicode = true)]
		public virtual String SubItemCDWildcard
		{
			get
			{
				//return SubItemCDUtils.CreateSubItemCDWildcard(this._SubItemCD);
				return SubCDUtils.CreateSubCDWildcard(this._SubItemCD, SubItemAttribute.DimensionName);
			}
		}
		#endregion

		#region SiteID
		public abstract class siteID : PX.Data.IBqlField
		{
		}
		protected Int32? _SiteID;
		//        [Site(Visibility = PXUIVisibility.Visible)]
		[Site(DescriptionField = typeof(INSite.descr), Required = false, DisplayName = "Warehouse")]
		//        [PXDefault()]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion

		#region LocationID
		public abstract class locationID : PX.Data.IBqlField
		{
		}
		protected Int32? _LocationID;
		[Location(typeof(InventoryTranDetEnqFilter.siteID), Visibility = PXUIVisibility.Visible, KeepEntry = false, DescriptionField = typeof(INLocation.descr), DisplayName = "Location")]
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

		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.IBqlField
		{
		}
		protected String _LotSerialNbr;
		//[INLotSerialNbr(typeof(INTran.inventoryID), typeof(INTran.subItemID), typeof(INTran.locationID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(100, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Lot/Serial Nbr.")]
		public virtual String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region LotSerialNbrWildcard
		public abstract class lotSerialNbrWildcard : PX.Data.IBqlField { };
		[PXDBString(100, IsUnicode = true)]
		public virtual String LotSerialNbrWildcard
		{
			get
			{
				return PXDatabase.Provider.SqlDialect.WildcardAnything + this._LotSerialNbr + PXDatabase.Provider.SqlDialect.WildcardAnything;
			}
		}
		#endregion

		#region ByFinancialPeriod
		public abstract class byFinancialPeriod : PX.Data.IBqlField
		{
		}
		protected bool? _ByFinancialPeriod;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "By Financial Period (Without Running Values)")]
		public virtual bool? ByFinancialPeriod
		{
			get
			{
				return this._ByFinancialPeriod;
			}
			set
			{
				this._ByFinancialPeriod = value;
			}
		}
		#endregion
		#region SummaryByDay
		public abstract class summaryByDay : PX.Data.IBqlField
		{
		}
		protected bool? _SummaryByDay;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Summary By Day")]
		public virtual bool? SummaryByDay
		{
			get
			{
				return this._SummaryByDay;
			}
			set
			{
				this._SummaryByDay = value;
			}
		}
		#endregion
		#region IncludeUnreleased
		public abstract class includeUnreleased : PX.Data.IBqlField
		{
		}
		protected bool? _IncludeUnreleased;
		[PXDBBool()]
		[PXDefault()]
		[PXUIField(DisplayName = "Include Unreleased (Without Costs)", Visibility = PXUIVisibility.Visible)]
		public virtual bool? IncludeUnreleased
		{
			get
			{
				return this._IncludeUnreleased;
			}
			set
			{
				this._IncludeUnreleased = value;
			}
		}
		#endregion
	}

	#endregion


	#region ResultSet
    [Serializable]
	public partial class InventoryTranDetEnqResult : PX.Data.IBqlTable
	{
		#region GridLineNbr
		// just for sorting in gris
		public abstract class gridLineNbr : PX.Data.IBqlField { }
		protected Int32? _GridLineNbr;
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Grid Line Nbr.", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public virtual Int32? GridLineNbr
		{
			get
			{
				return this._GridLineNbr;
			}
			set
			{
				this._GridLineNbr = value;
			}
		}
		#endregion

		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField { }
		protected Int32? _InventoryID;
		[Inventory(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Inventory ID")]
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
		#region TranDate
		public abstract class tranDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _TranDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Date")]
		public virtual DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		#region TranType
		public abstract class tranType : PX.Data.IBqlField { }
		protected String _TranType;
		[PXString(3)] // ???
		[INTranType.List()]
		[PXUIField(DisplayName = "Tran. Type", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.IBqlField
		{
		}
		protected String _DocType;
		[PXDBString(1, IsKey = true, IsFixed = true)]		
		[PXUIField(DisplayName = "Doc Type", Visibility = PXUIVisibility.Visible, Visible=false)]
		public virtual String DocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
				this._DocType = value;
			}
		}
		#endregion
		#region DocRefNbr
		public abstract class docRefNbr : PX.Data.IBqlField
		{
		}
		protected String _DocRefNbr;
		[PXString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<INRegister.refNbr>))]
		public virtual String DocRefNbr
		{
			get
			{
				return this._DocRefNbr;
			}
			set
			{
				this._DocRefNbr = value;
			}
		}
		#endregion

		#region SubItemID
		public abstract class subItemID : PX.Data.IBqlField
		{
		}
		protected Int32? _SubItemID;
		[SubItem(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Subitem")]
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
		#region SiteId
		public abstract class siteID : PX.Data.IBqlField { }
		protected Int32? _SiteID;
		//[PXDBInt(IsKey = true)] //???
		[Site(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Warehouse")]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.IBqlField { }
		protected Int32? _LocationID;
		//            [PXDBInt(IsKey = true)] //???
		[Location(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location")]
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
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.IBqlField
		{
		}
		protected String _LotSerialNbr;
		[PXDBString(100, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Lot/Serial Number", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion

		#region FinPerNbr
		public abstract class finPerNbr : PX.Data.IBqlField { };
		protected String _FinPerNbr;
		[GL.FinPeriodID()]
		[PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String FinPerNbr
		{
			get
			{
				return this._FinPerNbr;
			}
			set
			{
				this._FinPerNbr = value;
			}
		}
		#endregion
		#region TranPerNbr
		public abstract class tranPerNbr : PX.Data.IBqlField { };
		protected String _TranPerNbr;
		[GL.FinPeriodID()]
		[PXUIField(DisplayName = "Tran. Period", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String TranPerNbr
		{
			get
			{
				return this._TranPerNbr;
			}
			set
			{
				this._TranPerNbr = value;
			}
		}
		#endregion

		#region Released
		public abstract class released : PX.Data.IBqlField { }
		protected bool? _Released = false;
		[PXBool]
		[PXUIField(DisplayName = "Released", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual bool? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
			}
		}
		#endregion

		//  Qtys in stock UOM here

		#region BegQty
		public abstract class begQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _BegQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Beginning Qty.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? BegQty
		{
			get
			{
				return this._BegQty;
			}
			set
			{
				this._BegQty = value;
			}
		}
		#endregion
		#region QtyIn
		public abstract class qtyIn : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyIn;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. In", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? QtyIn
		{
			get
			{
				return this._QtyIn;
			}
			set
			{
				this._QtyIn = value;
			}
		}
		#endregion
		#region QtyOut
		public abstract class qtyOut : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyOut;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Out", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? QtyOut
		{
			get
			{
				return this._QtyOut;
			}
			set
			{
				this._QtyOut = value;
			}
		}
		#endregion
		#region EndQty
		public abstract class endQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _EndQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ending Qty.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? EndQty
		{
			get
			{
				return this._EndQty;
			}
			set
			{
				this._EndQty = value;
			}
		}
		#endregion

		#region BegBalance
		public abstract class begBalance : PX.Data.IBqlField
		{
		}
		protected Decimal? _BegBalance;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Beginning Balance [*]", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? BegBalance
		{
			get
			{
				return this._BegBalance;
			}
			set
			{
				this._BegBalance = value;
			}
		}
		#endregion
		#region ExtCostIn
		public abstract class extCostIn : PX.Data.IBqlField
		{
		}
		protected Decimal? _ExtCostIn;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cost In [*]", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? ExtCostIn
		{
			get
			{
				return this._ExtCostIn;
			}
			set
			{
				this._ExtCostIn = value;
			}
		}
		#endregion
		#region ExtCostOut
		public abstract class extCostOut : PX.Data.IBqlField
		{
		}
		protected Decimal? _ExtCostOut;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cost Out [*]", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? ExtCostOut
		{
			get
			{
				return this._ExtCostOut;
			}
			set
			{
				this._ExtCostOut = value;
			}
		}
		#endregion
		#region EndBalance
		public abstract class endBalance : PX.Data.IBqlField
		{
		}
		protected Decimal? _EndBalance;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ending Balance [*]", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? EndBalance
		{
			get
			{
				return this._EndBalance;
			}
			set
			{
				this._EndBalance = value;
			}
		}
		#endregion

		#region UnitCost
		public abstract class unitCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _UnitCost;
		[PXDBPriceCost()]
		//[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "In/Out Unit Cost [*]", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? UnitCost
		{
			get
			{
				return this._UnitCost;
			}
			set
			{
				this._UnitCost = value;
			}
		}
		#endregion
	}
	#endregion


	[PX.Objects.GL.TableAndChartDashboardType]
	public class InventoryTranDetEnq : PXGraph<InventoryTranDetEnq>
	{

		public PXFilter<InventoryTranDetEnqFilter> Filter;

		[PXFilterable]
		public PXSelectJoin<InventoryTranDetEnqResult, 
			CrossJoin<INTran>,
			Where<True, Equal<True>>, 
		OrderBy<Asc<InventoryTranDetEnqResult.gridLineNbr>>> ResultRecords;
		
		public PXCancel<InventoryTranDetEnqFilter> Cancel;

		public PXAction<InventoryTranDetEnqFilter> PreviousPeriod;
		public PXAction<InventoryTranDetEnqFilter> NextPeriod;

		#region Cache Attached
		public PXSelect<INTran> Tran;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "SO Order Type", Visible = false, Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<SO.SOOrderType.orderType>))]
		protected virtual void INTran_SOOrderType_CacheAttached(PXCache sender)
		{
		}
		protected String _SOOrderNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "SO Order Nbr.", Visible = false, Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<SO.SOOrder.orderNbr>))]
		protected virtual void INTran_SOOrderNbr_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "PO Receipt Nbr.", Visible = false, Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<PO.POReceipt.receiptNbr>))]
		protected virtual void INTran_POReceiptNbr_CacheAttached(PXCache sender)
		{
		}
		#endregion

		public InventoryTranDetEnq()
		{
			ResultRecords.Cache.AllowInsert = false;
			ResultRecords.Cache.AllowDelete = false;
			ResultRecords.Cache.AllowUpdate = false;
		}









		protected virtual IEnumerable resultRecords()
		{
			InventoryTranDetEnqFilter filter = Filter.Current;

			bool summaryByDay = filter.SummaryByDay ?? false;
			bool includeUnreleased = filter.IncludeUnreleased ?? false;
			bool byFinancialPeriod = filter.ByFinancialPeriod ?? false;

			// if summaryByDay : hide all document-specific values (refnbr, trandate etc.)
			// if includeUnreleased: don't calc and hide cost values
			// if byFinancialPeriod : don't calc and hide running values (Beg, End Qty and Cost)

			PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.inventoryID>(ResultRecords.Cache, null, false);

			PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.begQty>(ResultRecords.Cache, null, !byFinancialPeriod);
			PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.endQty>(ResultRecords.Cache, null, !byFinancialPeriod);

			PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.begBalance>(ResultRecords.Cache, null, !includeUnreleased & !byFinancialPeriod);
			PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.endBalance>(ResultRecords.Cache, null, !includeUnreleased & !byFinancialPeriod);

			PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.extCostIn>(ResultRecords.Cache, null, !includeUnreleased);
			PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.extCostOut>(ResultRecords.Cache, null, !includeUnreleased);
			PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.unitCost>(ResultRecords.Cache, null, !includeUnreleased & !summaryByDay);

			PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.finPerNbr>(ResultRecords.Cache, null, !summaryByDay);
			PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.tranPerNbr>(ResultRecords.Cache, null, !summaryByDay);

			PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.tranType>(ResultRecords.Cache, null, !summaryByDay);
			PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.docRefNbr>(ResultRecords.Cache, null, !summaryByDay);
			PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.subItemID>(ResultRecords.Cache, null, !summaryByDay);
			PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.siteID>(ResultRecords.Cache, null, !summaryByDay);
			PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.locationID>(ResultRecords.Cache, null, !summaryByDay);
			PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.lotSerialNbr>(ResultRecords.Cache, null, !summaryByDay);
			PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.released>(ResultRecords.Cache, null, !summaryByDay);
			PXUIFieldAttribute.SetVisible(Tran.Cache, null, !summaryByDay);

			var resultList = new List<PXResult<InventoryTranDetEnqResult,INTran>>();

			if (filter.InventoryID == null)
			{
				return resultList;  //empty
			}

			if (filter.FinPeriodID == null)
			{
				return resultList;  //empty
			}

			PXSelectBase<INTran> cmd = new PXSelectReadonly3<INTran,

					LeftJoin<INTranSplit,
							On<INTran.docType, Equal<INTranSplit.docType>,
									And<INTran.refNbr, Equal<INTranSplit.refNbr>,
									And<INTran.lineNbr, Equal<INTranSplit.lineNbr>>>>,

					LeftJoin<INTranSplitCost,    // left , not inner - because of possible Unreleased 
							On<Current<InventoryTranDetEnqFilter.includeUnreleased>, NotEqual<boolTrue>,  // no need to join actually if we dont need cost
									And<INTranSplitCost.docType, Equal<INTranSplit.docType>,
									And<INTranSplitCost.refNbr, Equal<INTranSplit.refNbr>,
									And<INTranSplitCost.lineNbr, Equal<INTranSplit.lineNbr>,
									And<INTranSplitCost.splitLineNbr, Equal<INTranSplit.splitLineNbr>>>>>>,


					LeftJoin<INSubItem,
									On<INSubItem.subItemID, Equal<INTran.subItemID>, Or<INSubItem.subItemID, Equal<INTranSplit.subItemID>>>,
					InnerJoin<INSite, On<INSite.siteID, Equal<INTran.siteID>>>>>>,
					OrderBy<Asc<INTran.inventoryID, Asc<INTran.tranDate, Asc<INTran.lastModifiedDateTime>>>>>(this);

			if (byFinancialPeriod)
			{
				// in case of "by Financial Period" we do NOT need all the trans to calc BegQty, BegBalance - so Equal, not LessEqual
				cmd.WhereAnd<Where<INTran.finPeriodID, Equal<Current<InventoryTranDetEnqFilter.finPeriodID>>>>();
			}
			else
			{
				// in case of "by Tran period" ("by Financial Period" = OFF ) we need all the previous trans to calc BegQty, BegBalance
				cmd.WhereAnd<Where<INTran.tranPeriodID, LessEqual<Current<InventoryTranDetEnqFilter.finPeriodID>>>>();
			}

			cmd.WhereAnd<Where<INTran.inventoryID, Equal<Current<InventoryTranDetEnqFilter.inventoryID>>, And<Match<INSite, Current<AccessInfo.userName>>>>>();

			if (filter.SiteID != null)
			{
				cmd.WhereAnd<Where<INTran.siteID, Equal<Current<InventoryTranDetEnqFilter.siteID>>>>();
			}

			//TODO: consider use of local filtering
			if (!SubCDUtils.IsSubCDEmpty(filter.SubItemCD))
			{
				cmd.WhereAnd<Where<INSubItem.subItemCD, Like<Current<InventoryTranDetEnqFilter.subItemCDWildcard>>>>();
			}

			if ((filter.LocationID ?? -1) != -1) // there are cases when filter.LocationID = -1
			{
				cmd.WhereAnd<Where<INTran.locationID, Equal<Current<InventoryTranDetEnqFilter.locationID>>, Or<INTranSplit.locationID, Equal<Current<InventoryTranDetEnqFilter.locationID>>>>>();
			}

			if ((filter.LotSerialNbr ?? "") != "")
			{
				cmd.WhereAnd<Where<INTran.lotSerialNbr, Like<Current<InventoryTranDetEnqFilter.lotSerialNbrWildcard>>, Or<INTranSplit.lotSerialNbr, Like<Current<InventoryTranDetEnqFilter.lotSerialNbrWildcard>>>>>();
			}

			if (!includeUnreleased)
			{
				cmd.WhereAnd<Where<INTran.released, Equal<boolTrue>>>();
			}


			if (byFinancialPeriod && (filter.StartDate != null))
			//( if by Tran Period (i.e byFinancialPeriod == false ) - we need all the trans to calc BegQty )        
			{

				cmd.WhereAnd<Where<INTran.tranDate, GreaterEqual<Current<InventoryTranDetEnqFilter.startDate>>>>();
			}

			if (filter.EndDate != null)
			{
				cmd.WhereAnd<Where<INTran.tranDate, LessEqual<Current<InventoryTranDetEnqFilter.endDate>>>>();
			}

			PXResultset<INTran> intermediateResult = (PXResultset<INTran>)cmd.Select();

			decimal cumulativeQty = 0m;
			decimal cumulativeBalance = 0m;
			int gridLineNbr = 0;

			foreach (PXResult<INTran, INTranSplit, INTranSplitCost, INSubItem> it in intermediateResult)
			{
				INTranSplit ts_rec = (INTranSplit)it;
				INTran t_rec = (INTran)it;
				INTranSplitCost tsc_rec = (INTranSplitCost)it;

				decimal rowQty = (ts_rec.InvtMult * ts_rec.BaseQty) ?? 0m;
				decimal rowExtCost;

				if (ts_rec.InvtMult == null)
				{
					rowExtCost = (t_rec.InvtMult * t_rec.TranCost) ?? 0m;
				}
				else if (tsc_rec.TranCostQty != 0m)
				{
					rowExtCost = (ts_rec.InvtMult * ts_rec.BaseQty * tsc_rec.TranCostCost / tsc_rec.TranCostQty) ?? 0m;
				}
				else
				{
					rowExtCost = (ts_rec.InvtMult * tsc_rec.TranCostCost) ?? 0m;
				}

				// if summaryByDay : hide all document-specific values (refnbr, trandate etc.)
				// if includeUnreleased: don't calc and hide cost values
				// if byFinancialPeriod : don't calc and hide running totals

				if
						(
										(
														(byFinancialPeriod & (t_rec.FinPeriodID.CompareTo(filter.FinPeriodID) < 0))  // ��� ��������, �� ����� ���� ������ �� ����� - ��. WhereAnd
												|| (!byFinancialPeriod & (t_rec.TranPeriodID.CompareTo(filter.FinPeriodID) < 0))
										)
								|| (filter.StartDate != null && (t_rec.TranDate < filter.StartDate))
						)
				{
					if (!byFinancialPeriod)
					{
						cumulativeQty += rowQty;
						if (!includeUnreleased) { cumulativeBalance += rowExtCost; }
					}
				}
				else
				{
					if (summaryByDay)
					{
						if ((resultList.Count > 0) && (((InventoryTranDetEnqResult)resultList[resultList.Count - 1]).TranDate == t_rec.TranDate))
						{
							InventoryTranDetEnqResult lastItem = resultList[resultList.Count - 1];
							if (rowQty >= 0)
							{
								lastItem.QtyIn += rowQty;
							}
							else
							{
								lastItem.QtyOut -= rowQty;
							}
							if (!byFinancialPeriod) { lastItem.EndQty += rowQty; }
							if (!includeUnreleased)
							{
								if (rowExtCost >= 0m)
								{
									lastItem.ExtCostIn += rowExtCost;
								}
								else
								{
									lastItem.ExtCostOut -= rowExtCost;
								}
								if (!byFinancialPeriod) { lastItem.EndBalance += rowExtCost; }
							}
							// UnitCost not set - since not shown as meaningless
						}
						else
						{
							InventoryTranDetEnqResult item = new InventoryTranDetEnqResult();
							if (!byFinancialPeriod)
							{
								item.BegQty = cumulativeQty;
								if (!includeUnreleased) { item.BegBalance = cumulativeBalance; }
							}
							item.TranDate = t_rec.TranDate;
							if (rowQty >= 0)
							{
								item.QtyIn = rowQty;
								item.QtyOut = 0m;
							}
							else
							{
								item.QtyIn = 0m;
								item.QtyOut = -rowQty;
							}
							if (!includeUnreleased)
							{
								if (rowExtCost >= 0)
								{
									item.ExtCostIn = rowExtCost;
									item.ExtCostOut = 0m;
								}
								else
								{
									item.ExtCostIn = 0m;
									item.ExtCostOut = -rowExtCost;
								}
							}
							if (!byFinancialPeriod)
							{
								item.EndQty = item.BegQty + rowQty;
								if (!includeUnreleased) { item.EndBalance = item.BegBalance + rowExtCost; }
							}
							item.GridLineNbr = ++gridLineNbr;
							resultList.Add( new PXResult<InventoryTranDetEnqResult, INTran>(item,null));
						}
						if (!byFinancialPeriod)
						{
							cumulativeQty += rowQty;
							if (!includeUnreleased) { cumulativeBalance += rowExtCost; }
						}
						// UnitCost not set - since not shown
					}
					else
					{
						InventoryTranDetEnqResult item = new InventoryTranDetEnqResult();
						if (!byFinancialPeriod)
						{
							item.BegQty = cumulativeQty;
							if (!includeUnreleased) { item.BegBalance = cumulativeBalance; }
						}
						item.TranDate = t_rec.TranDate;
						if (rowQty >= 0)
						{
							item.QtyIn = rowQty;
							item.QtyOut = 0m;
						}
						else
						{
							item.QtyIn = 0m;
							item.QtyOut = -rowQty;
						}
						if (!includeUnreleased)
						{
							if (rowExtCost >= 0)
							{
								item.ExtCostIn = rowExtCost;
								item.ExtCostOut = 0m;
							}
							else
							{
								item.ExtCostIn = 0m;
								item.ExtCostOut = -rowExtCost;
							}
						}

						if (rowQty != 0m) { item.UnitCost = rowExtCost / rowQty; }

						if (!byFinancialPeriod)
						{
							item.EndQty = item.BegQty + rowQty;
							if (!includeUnreleased) { item.EndBalance = item.BegBalance + rowExtCost; }
						}


						item.InventoryID = t_rec.InventoryID;
						item.TranType = t_rec.TranType;
						item.DocType = t_rec.DocType;
						item.DocRefNbr = t_rec.RefNbr;
						item.SubItemID = ts_rec.SubItemID ?? t_rec.SubItemID;
						item.SiteID = t_rec.SiteID;
						item.LocationID = ts_rec.LocationID ?? t_rec.LocationID;
						item.LotSerialNbr = ts_rec.LotSerialNbr ?? t_rec.LotSerialNbr;
						item.FinPerNbr = t_rec.FinPeriodID;
						item.TranPerNbr = t_rec.TranPeriodID;
						item.Released = t_rec.Released;
						item.GridLineNbr = ++gridLineNbr;
						resultList.Add( new PXResult<InventoryTranDetEnqResult, INTran>(item, t_rec));
						if (!byFinancialPeriod)
						{
							cumulativeQty += rowQty;
							if (!includeUnreleased) { cumulativeBalance += rowExtCost; }
						}
					}
				}

			}
			return resultList;
		}


		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}

		protected virtual void InventoryTranDetEnqFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			InventoryTranDetEnqFilter f = (InventoryTranDetEnqFilter)e.Row;
			if ((f.PeriodStartDate == null) && (f.PeriodEndDate == null))
			{
				ResetFilterDates(f);
			}
		}


		protected virtual void ResetFilterDates(InventoryTranDetEnqFilter aRow)
		{
			FinPeriod period = PXSelectReadonly<FinPeriod, Where<FinPeriod.finPeriodID, Equal<Required<FinPeriod.finPeriodID>>>>.Select(this, aRow.FinPeriodID);
			if (period != null)
			{
				aRow.PeriodStartDate = period.StartDate;
				aRow.PeriodEndDate = (DateTime)period.EndDate;
				aRow.EndDate = null;
				aRow.StartDate = null;
			}
		}

		protected virtual void InventoryTranDetEnqFilter_FinPeriodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			InventoryTranDetEnqFilter row = (InventoryTranDetEnqFilter)e.Row;
			this.ResetFilterDates(row);
		}

		//private bool _StartEndDateVerificationChain = false;

		protected virtual void InventoryTranDetEnqFilter_StartDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			InventoryTranDetEnqFilter row = (InventoryTranDetEnqFilter)e.Row;
			DateTime? newValue = (DateTime?)e.NewValue;
			/*          // �� ��������� : ��� ByFinancialPeriod==true ���� ����� �� �������� � ������
									if (newValue.HasValue && row.PeriodStartDate.HasValue && row.PeriodEndDate.HasValue)
									{
											if ((newValue < row.PeriodStartDate.Value) || (newValue >= row.PeriodEndDate.Value))
											{
													throw new PXSetPropertyException("Start Date must fall into the period");
											}
									} 
			*/

			if (newValue.HasValue && row.EndDate.HasValue)
			{
				if ((newValue > row.EndDate.Value))
				{
					throw new PXSetPropertyException(Messages.StartDateMustBeLessOrEqualToTheEndDate);
				}
			}

			InventoryTranDetEnqFilter currentFilter = Filter.Current;

			//PXFieldState state = (PXFieldState)cache.GetStateExt(currentFilter, typeof(InventoryTranDetEnqFilter.endDate).Name);

			//          string error = PXUIFieldAttribute.GetError<InventoryTranDetEnqFilter.endDate>(cache, e.Row);
			//          if (string.IsNullOrEmpty(error) == false) { };

			/*
									if (!_StartEndDateVerificationChain)
									{
											_StartEndDateVerificationChain = true;
											object endDate = row.EndDate;
											cache.RaiseFieldVerifying<InventoryTranDetEnqFilter.endDate>(e.Row, ref endDate);
											_StartEndDateVerificationChain = false;
									}
			*/

		}

		protected virtual void InventoryTranDetEnqFilter_EndDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			InventoryTranDetEnqFilter row = (InventoryTranDetEnqFilter)e.Row;
			DateTime? newValue = (DateTime?)e.NewValue;
			if (newValue.HasValue && row.StartDate.HasValue)
			{
				if ((newValue < row.StartDate.Value))
				{
					throw new PXSetPropertyException(Messages.StartDateMustBeLessOrEqualToTheEndDate);
				}
			}
			/*
									if (!_StartEndDateVerificationChain)
									{
											_StartEndDateVerificationChain = true;
											object startDate = row.StartDate;
											cache.RaiseFieldVerifying<InventoryTranDetEnqFilter.startDate>(e.Row, ref startDate);
											_StartEndDateVerificationChain = false;
									}
			*/
		}


		#region Button Delegates
		[PXUIField(DisplayName = ActionsMessages.Previous, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		public virtual IEnumerable previousperiod(PXAdapter adapter)
		{
			InventoryTranDetEnqFilter filter = Filter.Current as InventoryTranDetEnqFilter;
			FinPeriod nextperiod = PXSelect<FinPeriod,
																	Where<FinPeriod.finPeriodID,
																	Less<Current<InventoryTranDetEnqFilter.finPeriodID>>>,
																	OrderBy<Desc<FinPeriod.finPeriodID>>
																	>.SelectWindowed(this, 0, 1);
			if (nextperiod == null)
			{
				nextperiod = PXSelectOrderBy<FinPeriod,
														OrderBy<Desc<FinPeriod.finPeriodID>>>.SelectWindowed(this, 0, 1);
				if (nextperiod == null) yield return filter;
			}

			filter.FinPeriodID = nextperiod.FinPeriodID;
			ResetFilterDates(filter);
			yield return filter;
		}

		[PXUIField(DisplayName = ActionsMessages.Next, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		public virtual IEnumerable nextperiod(PXAdapter adapter)
		{
			InventoryTranDetEnqFilter filter = Filter.Current as InventoryTranDetEnqFilter;
			FinPeriod nextperiod = PXSelect<FinPeriod,
																	Where<FinPeriod.finPeriodID,
																	Greater<Current<InventoryTranDetEnqFilter.finPeriodID>>>,
																	OrderBy<Asc<FinPeriod.finPeriodID>>
																	>.SelectWindowed(this, 0, 1);
			if (nextperiod == null)
			{
				nextperiod = PXSelectOrderBy<FinPeriod,
														OrderBy<Asc<FinPeriod.finPeriodID>>>.SelectWindowed(this, 0, 1);
				if (nextperiod == null) yield return filter;
			}
			filter.FinPeriodID = nextperiod.FinPeriodID;
			ResetFilterDates(filter);
			yield return filter;
		}
		#endregion

		#region View Actions
		public PXAction<InventoryTranDetEnqFilter> viewItem;
		[PXLookupButton()]
		[PXUIField(DisplayName = Messages.InventoryItem)]
		protected virtual IEnumerable ViewItem(PXAdapter a)
		{
			if (this.ResultRecords.Current != null)
				InventoryItemMaint.Redirect(this.ResultRecords.Current.InventoryID, true);
			return a.Get();
		}

		public PXAction<InventoryTranDetEnqFilter> viewSummary;
		[PXLookupButton()]
		[PXUIField(DisplayName = Messages.InventorySummary)]
		protected virtual IEnumerable ViewSummary(PXAdapter a)
		{
			if (this.ResultRecords.Current != null)
			{
				PXSegmentedState subItem =
					this.ResultRecords.Cache.GetValueExt<InventoryTranDetEnqResult.subItemID>
					(this.ResultRecords.Current) as PXSegmentedState;
				InventorySummaryEnq.Redirect(
					this.ResultRecords.Current.InventoryID,
					subItem != null ? (string)subItem.Value : null,
					this.ResultRecords.Current.SiteID,
					this.ResultRecords.Current.LocationID, false);
			}
			return a.Get();
		}

		public PXAction<InventoryTranDetEnqFilter> viewAllocDet;
		[PXLookupButton()]
		[PXUIField(DisplayName = Messages.InventoryAllocDet)]
		protected virtual IEnumerable ViewAllocDet(PXAdapter a)
		{
			if (this.ResultRecords.Current != null)
			{
				PXSegmentedState subItem =
					this.ResultRecords.Cache.GetValueExt<InventoryTranDetEnqResult.subItemID>
					(this.ResultRecords.Current) as PXSegmentedState;
				InventoryAllocDetEnq.Redirect(
					this.ResultRecords.Current.InventoryID,
					subItem != null ? (string)subItem.Value : null,
					null,
					this.ResultRecords.Current.SiteID,
					this.ResultRecords.Current.LocationID);
			}
			return a.Get();
		}

		#endregion

		public static void Redirect(string finPeriodID, int? inventoryID, string subItemCD, string lotSerNum, int? siteID, int? locationID)
		{
			InventoryTranDetEnq graph = PXGraph.CreateInstance<InventoryTranDetEnq>();
			if (!string.IsNullOrEmpty(finPeriodID))
				graph.Filter.Current.FinPeriodID = finPeriodID;

			graph.Filter.Current.InventoryID = inventoryID;
			graph.Filter.Current.SubItemCD = subItemCD;
			graph.Filter.Current.SiteID = siteID;
			graph.Filter.Current.LocationID = locationID;
			graph.Filter.Current.LotSerialNbr = lotSerNum;

			throw new PXRedirectRequiredException(graph, Messages.InventoryTranDet);
		}
	}

	// almost the same as INTranDetail
	// but location determined from INTranSplit (some adjustments (std-costed items) have LocationID = NULL)
    [Obsolete()]
	[PXProjection(typeof(Select5<INTranCost,
		InnerJoin<INCostStatus, On<INCostStatus.costID, Equal<INTranCost.costID>>,
		InnerJoin<INCostSubItemXRef, On<INCostSubItemXRef.costSubItemID, Equal<INTranCost.costSubItemID>>,
		InnerJoin<INTranSplit,
			On<INTranSplit.tranType, Equal<INTranCost.tranType>,
				And<INTranSplit.refNbr, Equal<INTranCost.refNbr>,
				And<INTranSplit.lineNbr, Equal<INTranCost.lineNbr>,
				And<INTranSplit.subItemID, Equal<INCostSubItemXRef.subItemID>,
				And<INTranSplit.locationID, Equal<INLocation.locationID>,
				And<
					Where<INCostStatus.lotSerialNbr, Equal<INTranSplit.lotSerialNbr>,
						Or<INCostStatus.lotSerialNbr, IsNull>>>>>>>>,



		InnerJoin<INLocation,
			On<INLocation.costSiteID, Equal<INTranCost.costSiteID>,
			Or<INLocation.siteID, Equal<INTranCost.costSiteID>, And<
				Where<INCostStatus.valMethod, Equal<INValMethod.standard>, Or<INCostStatus.valMethod, Equal<INValMethod.specific>>>>>>,
		InnerJoin<INTran, On<INTran.tranType, Equal<INTranSplit.tranType>, And<INTran.refNbr, Equal<INTranSplit.refNbr>, And<INTran.lineNbr, Equal<INTranSplit.lineNbr>>>>>>>>>,
		Aggregate<
			GroupBy<INTranSplit.tranType,
			GroupBy<INTranSplit.refNbr,
			GroupBy<INTranSplit.lineNbr,
			GroupBy<INTranSplit.splitLineNbr,
			GroupBy<INTran.finPeriodID,
			GroupBy<INTran.tranPeriodID,
			GroupBy<INTranCost.inventoryID,
			GroupBy<INTranCost.costSiteID,
			GroupBy<INTranCost.costSubItemID,
			GroupBy<INTranCost.invtAcctID,
			GroupBy<INTranCost.invtSubID,
			Sum<INTranCost.qty,
			Sum<INTranCost.tranCost>>>>>>>>>>>>>>>))]
    [Serializable]
	public partial class INTranDetailLocFromTranSplit : PX.Data.IBqlTable
	{
		#region TranType
		public abstract class tranType : PX.Data.IBqlField
		{
		}
		protected String _TranType;
		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(INTranSplit.tranType))]
		[INTranType.List()]
		public virtual String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.IBqlField
		{
		}
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(INTranSplit.refNbr))]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(INTranSplit.lineNbr))]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region SplitLineNbr
		public abstract class splitLineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _SplitLineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(INTranSplit.splitLineNbr))]
		public virtual Int32? SplitLineNbr
		{
			get
			{
				return this._SplitLineNbr;
			}
			set
			{
				this._SplitLineNbr = value;
			}
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _TranDate;
		[PXDBDate(BqlField = typeof(INTranCost.tranDate))]
		public virtual DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		protected String _FinPeriodID;
		[GL.FinPeriodID(BqlField = typeof(INTran.finPeriodID))]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.IBqlField
		{
		}
		protected String _TranPeriodID;
		[GL.FinPeriodID(BqlField = typeof(INTran.tranPeriodID))]
		public virtual String TranPeriodID
		{
			get
			{
				return this._TranPeriodID;
			}
			set
			{
				this._TranPeriodID = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[StockItem(BqlField = typeof(INTranCost.inventoryID))]
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
		#region CostSubItemID
		public abstract class costSubItemID : PX.Data.IBqlField
		{
		}
		protected Int32? _CostSubItemID;
		[SubItem(BqlField = typeof(INTranCost.costSubItemID))]
		public virtual Int32? CostSubItemID
		{
			get
			{
				return this._CostSubItemID;
			}
			set
			{
				this._CostSubItemID = value;
			}
		}
		#endregion
		#region CostSiteID
		public abstract class costSiteID : PX.Data.IBqlField
		{
		}
		protected Int32? _CostSiteID;
		[PXDBInt(BqlField = typeof(INTranCost.costSiteID))]
		public virtual Int32? CostSiteID
		{
			get
			{
				return this._CostSiteID;
			}
			set
			{
				this._CostSiteID = value;
			}
		}
		#endregion
		#region InvtAcctID
		public abstract class invtAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _InvtAcctID;
		[Account(BqlField = typeof(INTranCost.invtAcctID))]
		public virtual Int32? InvtAcctID
		{
			get
			{
				return this._InvtAcctID;
			}
			set
			{
				this._InvtAcctID = value;
			}
		}
		#endregion
		#region InvtSubID
		public abstract class invtSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _InvtSubID;
		[SubAccount(BqlField = typeof(INTranCost.invtSubID))]
		public virtual Int32? InvtSubID
		{
			get
			{
				return this._InvtSubID;
			}
			set
			{
				this._InvtSubID = value;
			}
		}
		#endregion
		#region InvtMult
		public abstract class invtMult : PX.Data.IBqlField
		{
		}
		protected Int16? _InvtMult;
		[PXDBShort(BqlField = typeof(INTranCost.invtMult))]
		public virtual Int16? InvtMult
		{
			get
			{
				return this._InvtMult;
			}
			set
			{
				this._InvtMult = value;
			}
		}
		#endregion
		#region SumQty
		public abstract class sumQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _SumQty;
		[PXDBQuantity(BqlField = typeof(INTranCost.qty))]
		public virtual Decimal? SumQty
		{
			get
			{
				return this._SumQty;
			}
			set
			{
				this._SumQty = value;
			}
		}
		#endregion
		#region SumTranCost
		public abstract class sumTranCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _SumTranCost;
		[CM.PXDBBaseCury(BqlField = typeof(INTranCost.tranCost))]
		public virtual Decimal? SumTranCost
		{
			get
			{
				return this._SumTranCost;
			}
			set
			{
				this._SumTranCost = value;
			}
		}
		#endregion
		////INTranSplit
		#region SubItemID
		public abstract class subItemID : PX.Data.IBqlField
		{
		}
		protected Int32? _SubItemID;
		[SubItem(BqlField = typeof(INTranSplit.subItemID))]
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
		#region SiteID
		public abstract class siteID : PX.Data.IBqlField
		{
		}
		protected Int32? _SiteID;
		[IN.Site(BqlField = typeof(INTranSplit.siteID))]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.IBqlField
		{
		}
		protected Int32? _LocationID;
		[IN.Location(BqlField = typeof(INTranSplit.locationID))]
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
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.IBqlField
		{
		}
		protected String _LotSerialNbr;
		[PXDBString(100, IsUnicode = true, BqlField = typeof(INTranSplit.lotSerialNbr))]
		public virtual String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.IBqlField
		{
		}
		protected String _UOM;
		[PXDBString(6, IsUnicode = true, BqlField = typeof(INTranSplit.uOM))]
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
		#region Qty
		public abstract class qty : PX.Data.IBqlField
		{
		}
		protected Decimal? _Qty;
		[PXDBQuantity(BqlField = typeof(INTranSplit.qty))]
		public virtual Decimal? Qty
		{
			get
			{
				return this._Qty;
			}
			set
			{
				this._Qty = value;
			}
		}
		#endregion
		#region BaseQty
		public abstract class baseQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _BaseQty;
		[PXDBQuantity(BqlField = typeof(INTranSplit.baseQty))]
		public virtual Decimal? BaseQty
		{
			get
			{
				return this._BaseQty;
			}
			set
			{
				this._BaseQty = value;
			}
		}
		#endregion
		#region TranCost
		public abstract class tranCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranCost;
		[CM.PXBaseCury()]
		public virtual Decimal? TranCost
		{
			[PXDependsOnFields(typeof(tranType),typeof(sumTranCost),typeof(sumQty),typeof(baseQty))]
			get
			{
				if (TranType == INTranType.Adjustment || TranType == INTranType.StandardCostAdjustment || TranType == INTranType.NegativeCostAdjustment)
				{
					return SumTranCost;
				}
				else if (SumQty == 0m)
				{
					return 0m;
				}
				else
				{
					return BaseQty * SumTranCost / SumQty;
				}
			}
			set
			{
				this._TranCost = value;
			}
		}
		#endregion
	}



}


