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
	#region InventoryTranSumEnqFilter
	[Serializable]
	public partial class InventoryTranSumEnqFilter : PX.Data.IBqlTable
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		//[PXDefault()]
		[Inventory(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.stkItem, NotEqual<boolFalse>, And<Where<Match<Current<AccessInfo.userName>>>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))]
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
		[SubItemRawExt(typeof(InventoryTranSumEnqFilter.inventoryID), DisplayName = "Subitem")]
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
		[Location(typeof(InventoryTranSumEnqFilter.siteID), Visibility = PXUIVisibility.Visible, KeepEntry = false, DescriptionField = typeof(INLocation.descr), DisplayName = "Location")]
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
		#region ByFinancialPeriod
		public abstract class byFinancialPeriod : PX.Data.IBqlField
		{
		}
		protected bool? _ByFinancialPeriod;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "By Financial Period")]
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
		#region SubItemDetails
		public abstract class subItemDetails : PX.Data.IBqlField
		{
		}
		protected bool? _SubItemDetails;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Subitem Details", FieldClass = SubItemAttribute.DimensionName)]
		public virtual bool? SubItemDetails
		{
			get
			{
				return this._SubItemDetails;
			}
			set
			{
				this._SubItemDetails = value;
			}
		}
		#endregion
		#region SiteDetails
		public abstract class siteDetails : PX.Data.IBqlField
		{
		}
		protected bool? _SiteDetails;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Warehouse Details")]
		public virtual bool? SiteDetails
		{
			get
			{
				return this._SiteDetails;
			}
			set
			{
				this._SiteDetails = value;
			}
		}
		#endregion
		#region LocationDetails
		// if true than SiteDetails should be true too
		public abstract class locationDetails : PX.Data.IBqlField
		{
		}
		protected bool? _LocationDetails;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Location Details")]
		public virtual bool? LocationDetails
		{
			get
			{
				return this._LocationDetails;
			}
			set
			{
				this._LocationDetails = value;
			}
		}
		#endregion
		#region ShowItemsWithoutMovement
		public abstract class showItemsWithoutMovement : PX.Data.IBqlField
		{
		}
		protected bool? _ShowItemsWithoutMovement;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Show Items Without Movement")]
		public virtual bool? ShowItemsWithoutMovement
		{
			get
			{
				return this._ShowItemsWithoutMovement;
			}
			set
			{
				this._ShowItemsWithoutMovement = value;
			}
		}
		#endregion
	}
	#endregion

	[PX.Objects.GL.TableAndChartDashboardType]
	public class InventoryTranSumEnq : PXGraph<InventoryTranSumEnq>
	{

		public PXFilter<InventoryTranSumEnqFilter> Filter;

		[PXFilterable]
		public PXSelectOrderBy<INItemSiteHist,
				OrderBy<
						Asc<INItemSiteHist.inventoryID,
						Asc<INItemSiteHist.subItemID,
						Asc<INItemSiteHist.siteID,
						Asc<INItemSiteHist.locationID>>>>>> ResultRecords;

		public PXCancel<InventoryTranSumEnqFilter> Cancel;
		public PXAction<InventoryTranSumEnqFilter> PreviousPeriod;
		public PXAction<InventoryTranSumEnqFilter> NextPeriod;


		public InventoryTranSumEnq()
		{
			ResultRecords.Cache.AllowInsert = false;
			ResultRecords.Cache.AllowDelete = false;
			ResultRecords.Cache.AllowUpdate = false;

            FieldSelecting.AddHandler<INItemSiteHist.tranPtdQtyAdjusted>(QtyFieldSelecting<INItemSiteHist.tranPtdQtyAdjusted, INItemSiteHist.finPtdQtyAdjusted>);
            FieldSelecting.AddHandler<INItemSiteHist.tranBegQty>(QtyFieldSelecting<INItemSiteHist.tranBegQty, INItemSiteHist.finBegQty>);
            FieldSelecting.AddHandler<INItemSiteHist.tranPtdQtyAssemblyIn>(QtyFieldSelecting<INItemSiteHist.tranPtdQtyAssemblyIn, INItemSiteHist.finPtdQtyAssemblyIn>);
            FieldSelecting.AddHandler<INItemSiteHist.tranPtdQtyAssemblyOut>(QtyFieldSelecting<INItemSiteHist.tranPtdQtyAssemblyOut, INItemSiteHist.finPtdQtyAssemblyOut>);
            FieldSelecting.AddHandler<INItemSiteHist.tranPtdQtyCreditMemos>(QtyFieldSelecting<INItemSiteHist.tranPtdQtyCreditMemos, INItemSiteHist.finPtdQtyCreditMemos>);
            FieldSelecting.AddHandler<INItemSiteHist.tranPtdQtyDropShipSales>(QtyFieldSelecting<INItemSiteHist.tranPtdQtyDropShipSales, INItemSiteHist.finPtdQtyDropShipSales>);
            FieldSelecting.AddHandler<INItemSiteHist.tranPtdQtyIssued>(QtyFieldSelecting<INItemSiteHist.tranPtdQtyIssued, INItemSiteHist.finPtdQtyIssued>);
            FieldSelecting.AddHandler<INItemSiteHist.tranPtdQtyReceived>(QtyFieldSelecting<INItemSiteHist.tranPtdQtyReceived, INItemSiteHist.finPtdQtyReceived>);
            FieldSelecting.AddHandler<INItemSiteHist.tranPtdQtySales>(QtyFieldSelecting<INItemSiteHist.tranPtdQtySales, INItemSiteHist.finPtdQtySales>);
            FieldSelecting.AddHandler<INItemSiteHist.tranPtdQtyTransferIn>(QtyFieldSelecting<INItemSiteHist.tranPtdQtyTransferIn, INItemSiteHist.finPtdQtyTransferIn>);
            FieldSelecting.AddHandler<INItemSiteHist.tranPtdQtyTransferOut>(QtyFieldSelecting<INItemSiteHist.tranPtdQtyTransferOut, INItemSiteHist.finPtdQtyTransferOut>);
            FieldSelecting.AddHandler<INItemSiteHist.tranYtdQty>(QtyFieldSelecting<INItemSiteHist.tranYtdQty, INItemSiteHist.finYtdQty>);
		}

		protected virtual void InventoryTranSumEnqFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

            PXUIFieldAttribute.SetVisible<INItemSiteHist.subItemID>(ResultRecords.Cache, null, Filter.Current.SubItemDetails == true);
			PXUIFieldAttribute.SetVisible<INItemSiteHist.siteID>(ResultRecords.Cache, null, Filter.Current.SiteDetails == true || Filter.Current.LocationDetails == true);
			PXUIFieldAttribute.SetVisible<INItemSiteHist.locationID>(ResultRecords.Cache, null, Filter.Current.LocationDetails == true);
		}

        protected virtual void QtyFieldSelecting<TranQtyField, FinQtyField>(PXCache sender, PXFieldSelectingEventArgs e)
            where TranQtyField : IBqlField
            where FinQtyField : IBqlField
        {
            INItemSiteHist hist = (INItemSiteHist)e.Row;
	        if (hist == null) return;

            e.ReturnValue = Filter.Current.ByFinancialPeriod == true ? sender.GetValue<FinQtyField>(hist) : sender.GetValue<TranQtyField>(hist);
	    }

	    protected virtual IEnumerable resultRecords()
		{
			if (Filter.Current == null || string.IsNullOrEmpty(Filter.Current.FinPeriodID))
			{
				yield break;
			}

			PXSelectBase<INItemSiteHistByPeriod> cmd = new PXSelectJoin<INItemSiteHistByPeriod,
			InnerJoin<INItemSiteHist, On<INItemSiteHist.inventoryID, Equal<INItemSiteHistByPeriod.inventoryID>, And<INItemSiteHist.siteID, Equal<INItemSiteHistByPeriod.siteID>, And<INItemSiteHist.subItemID, Equal<INItemSiteHistByPeriod.subItemID>, And<INItemSiteHist.locationID, Equal<INItemSiteHistByPeriod.locationID>, And<INItemSiteHist.finPeriodID, Equal<INItemSiteHistByPeriod.lastActivityPeriod>>>>>>,
			InnerJoin<INSubItem, On<INSubItem.subItemID, Equal<INItemSiteHistByPeriod.subItemID>>,
			InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INItemSiteHistByPeriod.inventoryID>>,
			InnerJoin<INSite, On<INSite.siteID, Equal<INItemSiteHistByPeriod.siteID>>>>>>,
			Where<INItemSiteHistByPeriod.finPeriodID, Equal<Current<InventoryTranSumEnqFilter.finPeriodID>>,
				And2<Match<InventoryItem, Current<AccessInfo.userName>>, And<Match<INSite, Current<AccessInfo.userName>>>>>>(this);

			bool SiteDetails = Filter.Current.SiteDetails == true || Filter.Current.LocationDetails == true;
			bool LocationDetails = Filter.Current.LocationDetails == true;
			bool SubItemDetails = Filter.Current.SubItemDetails == true;

			if (Filter.Current.InventoryID != null)
			{
				cmd.WhereAnd<Where<INItemSiteHistByPeriod.inventoryID, Equal<Current<InventoryTranSumEnqFilter.inventoryID>>>>();
			}

			if (!SubCDUtils.IsSubCDEmpty(Filter.Current.SubItemCD))
			{
				cmd.WhereAnd<Where<INSubItem.subItemCD, Like<Current<InventoryTranSumEnqFilter.subItemCDWildcard>>>>();
			}

			if (Filter.Current.SiteID != null)
			{
				cmd.WhereAnd<Where<INItemSiteHistByPeriod.siteID, Equal<Current<InventoryTranSumEnqFilter.siteID>>>>();
			}

			if ((Filter.Current.LocationID ?? -1) != -1)
			{
				cmd.WhereAnd<Where<INItemSiteHistByPeriod.locationID, Equal<Current<InventoryTranSumEnqFilter.locationID>>>>();
			}

			PXResultset<INItemSiteHistByPeriod> list = cmd.Select();

			list.Sort(delegate(PXResult<INItemSiteHistByPeriod> ra, PXResult<INItemSiteHistByPeriod> rb)
				{
					INItemSiteHistByPeriod a = ra;
					INItemSiteHistByPeriod b = rb;

					int? aInventoryID = a.InventoryID;
					int? bInventoryID = b.InventoryID;
					int ret = ((IComparable)aInventoryID).CompareTo(bInventoryID);
					if (ret != 0)
					{
						return ret;
					}

					if (SubItemDetails)
					{
						int? aSubItemID = a.SubItemID;
						int? bSubItemID = b.SubItemID;
						ret = ((IComparable)aSubItemID).CompareTo(bSubItemID);
						if (ret != 0)
						{
							return ret;
						}
					}

					if (SiteDetails)
					{
						int? aSiteID = a.SiteID;
						int? bSiteID = b.SiteID;
						ret = ((IComparable)aSiteID).CompareTo(bSiteID);
						if (ret != 0)
						{
							return ret;
						}
					}

					if (LocationDetails)
					{
						int? aLocationID = a.LocationID;
						int? bLocationID = b.LocationID;
						ret = ((IComparable)aLocationID).CompareTo(bLocationID);
						return ret;
					}
					else
					{
						return 0;
					}
				});

			INItemSiteHist prev_hist = new INItemSiteHist();

			foreach (PXResult<INItemSiteHistByPeriod, INItemSiteHist> res in list)
			{
				INItemSiteHistByPeriod byperiod = res;
				INItemSiteHist hist = res;

				if (string.Equals(byperiod.FinPeriodID, byperiod.LastActivityPeriod) == false)
				{
					hist.TranBegQty = hist.TranYtdQty;
					hist.TranPtdQtyAdjusted = 0m;
					hist.TranPtdQtyAssemblyIn = 0m;
					hist.TranPtdQtyAssemblyOut = 0m;
					hist.TranPtdQtyCreditMemos = 0m;
					hist.TranPtdQtyDropShipSales = 0m;
					hist.TranPtdQtyIssued = 0m;
					hist.TranPtdQtyReceived = 0m;
					hist.TranPtdQtySales = 0m;
					hist.TranPtdQtyTransferIn = 0m;
					hist.TranPtdQtyTransferOut = 0m;

					hist.FinBegQty = hist.FinYtdQty;
					hist.FinPtdQtyAdjusted = 0m;
					hist.FinPtdQtyAssemblyIn = 0m;
					hist.FinPtdQtyAssemblyOut = 0m;
					hist.FinPtdQtyCreditMemos = 0m;
					hist.FinPtdQtyDropShipSales = 0m;
					hist.FinPtdQtyIssued = 0m;
					hist.FinPtdQtyReceived = 0m;
					hist.FinPtdQtySales = 0m;
					hist.FinPtdQtyTransferIn = 0m;
					hist.FinPtdQtyTransferOut = 0m;
				}

				if (object.Equals(prev_hist.InventoryID, hist.InventoryID) == false ||
					SubItemDetails && object.Equals(prev_hist.SubItemID, hist.SubItemID) == false ||
					SiteDetails && object.Equals(prev_hist.SiteID, hist.SiteID) == false ||
					LocationDetails && object.Equals(prev_hist.LocationID, hist.LocationID) == false)
				{
					if (prev_hist.InventoryID != null)
					{
						if (Filter.Current.ShowItemsWithoutMovement == true || string.Equals(prev_hist.FinPeriodID, prev_hist.LastActivityPeriod))
						{
							yield return prev_hist;
						}
					}
					prev_hist = PXCache<INItemSiteHist>.CreateCopy(hist);
					prev_hist.FinPeriodID = byperiod.FinPeriodID;
					prev_hist.LastActivityPeriod = byperiod.LastActivityPeriod;
				}
				else
				{
					if (string.Compare(prev_hist.LastActivityPeriod, byperiod.LastActivityPeriod) < 0)
					{
						prev_hist.LastActivityPeriod = byperiod.LastActivityPeriod;
					}
					prev_hist.TranBegQty += hist.TranBegQty;
					prev_hist.TranPtdQtyAdjusted += hist.TranPtdQtyAdjusted;
					prev_hist.TranPtdQtyAssemblyIn += hist.TranPtdQtyAssemblyIn;
					prev_hist.TranPtdQtyAssemblyOut += hist.TranPtdQtyAssemblyOut;
					prev_hist.TranPtdQtyCreditMemos += hist.TranPtdQtyCreditMemos;
					prev_hist.TranPtdQtyDropShipSales += hist.TranPtdQtyDropShipSales;
					prev_hist.TranPtdQtyIssued += hist.TranPtdQtyIssued;
					prev_hist.TranPtdQtyReceived += hist.TranPtdQtyReceived;
					prev_hist.TranPtdQtySales += hist.TranPtdQtySales;
					prev_hist.TranPtdQtyTransferIn += hist.TranPtdQtyTransferIn;
					prev_hist.TranPtdQtyTransferOut += hist.TranPtdQtyTransferOut;
					prev_hist.TranYtdQty += hist.TranYtdQty;

					prev_hist.FinBegQty += hist.FinBegQty;
					prev_hist.FinPtdQtyAdjusted += hist.FinPtdQtyAdjusted;
					prev_hist.FinPtdQtyAssemblyIn += hist.FinPtdQtyAssemblyIn;
					prev_hist.FinPtdQtyAssemblyOut += hist.FinPtdQtyAssemblyOut;
					prev_hist.FinPtdQtyCreditMemos += hist.FinPtdQtyCreditMemos;
					prev_hist.FinPtdQtyDropShipSales += hist.FinPtdQtyDropShipSales;
					prev_hist.FinPtdQtyIssued += hist.FinPtdQtyIssued;
					prev_hist.FinPtdQtyReceived += hist.FinPtdQtyReceived;
					prev_hist.FinPtdQtySales += hist.FinPtdQtySales;
					prev_hist.FinPtdQtyTransferIn += hist.FinPtdQtyTransferIn;
					prev_hist.FinPtdQtyTransferOut += hist.FinPtdQtyTransferOut;
					prev_hist.FinYtdQty += hist.FinYtdQty;
				}
			}

			if (prev_hist.InventoryID != null)
			{
				if (Filter.Current.ShowItemsWithoutMovement == true || string.Equals(prev_hist.FinPeriodID, prev_hist.LastActivityPeriod))
				{
					yield return prev_hist;
				}
			}
		}


		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}

		public virtual void InventoryTranSumEnqFilter_InventoryCD_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		#region Button Delegates
		[PXUIField(DisplayName = ActionsMessages.Previous, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		public virtual IEnumerable previousperiod(PXAdapter adapter)
		{
			InventoryTranSumEnqFilter filter = Filter.Current as InventoryTranSumEnqFilter;
			FinPeriod nextperiod = PXSelect<FinPeriod,
																	Where<FinPeriod.finPeriodID,
																	Less<Current<InventoryTranSumEnqFilter.finPeriodID>>>,
																	OrderBy<Desc<FinPeriod.finPeriodID>>
																	>.SelectWindowed(this, 0, 1);
			if (nextperiod == null)
			{
				nextperiod = PXSelectOrderBy<FinPeriod,
														OrderBy<Desc<FinPeriod.finPeriodID>>>.SelectWindowed(this, 0, 1);
				if (nextperiod == null) yield return filter;
			}

			filter.FinPeriodID = nextperiod.FinPeriodID;
			yield return filter;
		}

		[PXUIField(DisplayName = ActionsMessages.Next, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		public virtual IEnumerable nextperiod(PXAdapter adapter)
		{
			InventoryTranSumEnqFilter filter = Filter.Current as InventoryTranSumEnqFilter;
			FinPeriod nextperiod = PXSelect<FinPeriod,
																	Where<FinPeriod.finPeriodID,
																	Greater<Current<InventoryTranSumEnqFilter.finPeriodID>>>,
																	OrderBy<Asc<FinPeriod.finPeriodID>>
																	>.SelectWindowed(this, 0, 1);
			if (nextperiod == null)
			{
				nextperiod = PXSelectOrderBy<FinPeriod,
														OrderBy<Asc<FinPeriod.finPeriodID>>>.SelectWindowed(this, 0, 1);
				if (nextperiod == null) yield return filter;
			}
			filter.FinPeriodID = nextperiod.FinPeriodID;
			yield return filter;
		}
		#endregion

		#region View Actions
		public PXAction<InventoryTranSumEnqFilter> viewInventoryTranDet;
		[PXUIField(DisplayName = Messages.ViewInventoryTranDet)]
		[PXLookupButton]
		protected virtual IEnumerable ViewInventoryTranDet(PXAdapter adapter)
		{
			if ((this.ResultRecords.Current != null) && (this.Filter.Current != null))
			{
				INItemSiteHist res = this.ResultRecords.Current;
				InventoryTranSumEnqFilter currentFilter = this.Filter.Current;
				InventoryTranDetEnq graph = PXGraph.CreateInstance<InventoryTranDetEnq>();

				graph.Filter.Current.ByFinancialPeriod = currentFilter.ByFinancialPeriod;
				graph.Filter.Current.FinPeriodID = currentFilter.FinPeriodID;
				graph.Filter.Current.InventoryID = res.InventoryID;
				if (res.SubItemID != null)
				{
					INSubItem foundSubItem = PXSelect<INSubItem, Where<INSubItem.subItemID, Equal<Required<INSubItem.subItemID>>>>.Select(graph, res.SubItemID);
					if (foundSubItem != null)
					{
						graph.Filter.Current.SubItemCD = foundSubItem.SubItemCD;
					}
				}
				graph.Filter.Current.SiteID = res.SiteID; // possibly null
				graph.Filter.Current.LocationID = res.LocationID;
				throw new PXRedirectRequiredException(graph, "Inventory Transaction Details");
			}
			return adapter.Get();
		}

		public PXAction<InventoryTranSumEnqFilter> viewItem;
		[PXLookupButton()]
		[PXUIField(DisplayName = Messages.InventoryItem)]
		protected virtual IEnumerable ViewItem(PXAdapter a)
		{
			if (this.ResultRecords.Current != null)
				InventoryItemMaint.Redirect(this.ResultRecords.Current.InventoryID, true);
			return a.Get();
		}

		public PXAction<InventoryTranSumEnqFilter> viewSummary;
		[PXLookupButton()]
		[PXUIField(DisplayName = Messages.InventorySummary)]
		protected virtual IEnumerable ViewSummary(PXAdapter a)
		{
			if (this.ResultRecords.Current != null)
			{
				InventorySummaryEnq.Redirect(
					this.ResultRecords.Current.InventoryID,
					this.Filter.Current.SubItemCD,
					this.Filter.Current.SiteID,
					this.Filter.Current.LocationID, false);
			}
			return a.Get();
		}

		public PXAction<InventoryTranSumEnqFilter> viewAllocDet;
		[PXLookupButton()]
		[PXUIField(DisplayName = Messages.InventoryAllocDet)]
		protected virtual IEnumerable ViewAllocDet(PXAdapter a)
		{
			if (this.ResultRecords.Current != null)
			{
				InventoryAllocDetEnq.Redirect(
					this.ResultRecords.Current.InventoryID,
					this.Filter.Current.SubItemCD,
					null,
					this.Filter.Current.SiteID,
					this.Filter.Current.LocationID);
			}
			return a.Get();
		}

		#endregion

		public static void Redirect(string finPeriodID, int? inventoryID, string subItemCD, int? siteID, int? locationID)
		{
			InventoryTranSumEnq graph = PXGraph.CreateInstance<InventoryTranSumEnq>();
			if (!string.IsNullOrEmpty(finPeriodID))
				graph.Filter.Current.FinPeriodID = finPeriodID;

			graph.Filter.Current.InventoryID = inventoryID;
			graph.Filter.Current.SubItemCD = subItemCD;
			graph.Filter.Current.SiteID = siteID;
			graph.Filter.Current.LocationID = locationID;

			throw new PXRedirectRequiredException(graph, Messages.InventoryTranSum);
		}
	}
}


