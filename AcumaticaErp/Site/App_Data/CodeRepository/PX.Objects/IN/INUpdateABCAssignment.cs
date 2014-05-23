using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using PX.SM;
using PX.Data;
using PX.Objects.BQLConstants;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.GL;


namespace PX.Objects.IN
{
	#region Update Settings

	[Serializable]
	public partial class UpdateABCAssignmentSettings : PX.Data.IBqlTable
	{
		#region SiteID
		public abstract class siteID : PX.Data.IBqlField
		{
		}
		protected Int32? _SiteID;
		[PXDefault()]
		[Site()]
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
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		protected String _FinPeriodID;
		//[FinPeriodID]
		//[FinPeriodSelector(typeof(AccessInfo.businessDate))]
		[INClosedPeriod(typeof(AccessInfo.businessDate))]
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
	}

	#endregion

	#region UpdateResult
    [Serializable]
	public partial class UpdateABCAssignmentResult : PX.Data.IBqlTable
	{

		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField { }
		protected Int32? _InventoryID;
		[Inventory(IsKey = true, Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Inventory ID")]
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
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected String _Descr;
		[PXString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
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

		#region OldABCCode
		public abstract class oldABCCode : PX.Data.IBqlField
		{
		}
		protected String _OldABCCode;
		[PXString(1)]
		[PXUIField(DisplayName = "Current ABC Code", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String OldABCCode
		{
			get
			{
				return this._OldABCCode;
			}
			set
			{
				this._OldABCCode = value;
			}
		}
		#endregion
		#region ABCCodeFixed
		public abstract class aBCCodeFixed : PX.Data.IBqlField { }
		protected bool? _ABCCodeFixed = false;
		[PXBool]
		[PXUIField(DisplayName = "Fixed", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual bool? ABCCodeFixed
		{
			get
			{
				return this._ABCCodeFixed;
			}
			set
			{
				this._ABCCodeFixed = value;
			}
		}
		#endregion
		#region NewABCCode
		public abstract class newABCCode : PX.Data.IBqlField
		{
		}
		protected String _NewABCCode;
		[PXString(1)]
		[PXUIField(DisplayName = "Projected ABC Code", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String NewABCCode
		{
			get
			{
				return this._NewABCCode;
			}
			set
			{
				this._NewABCCode = value;
			}
		}
		#endregion

		#region YtdCost
		public abstract class ytdCost : IBqlField
		{
		}
		protected decimal? _YtdCost;
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Criteria Value")]
		public virtual decimal? YtdCost
		{
			get
			{
				return _YtdCost;
			}
			set
			{
				_YtdCost = value;
			}
		}
		#endregion
		#region Ratio
		public abstract class ratio : IBqlField
		{
		}
		protected decimal? _Ratio;
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Ratio, %")]
		public virtual decimal? Ratio
		{
			get
			{
				return _Ratio;
			}
			set
			{
				_Ratio = value;
			}
		}
		#endregion
		#region CumulativeRatio
		public abstract class cumulativeRatio : IBqlField
		{
		}
		protected decimal? _CumulativeRatio;
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Cumulative Ratio, %")]
		public virtual decimal? CumulativeRatio
		{
			get
			{
				return _CumulativeRatio;
			}
			set
			{
				_CumulativeRatio = value;
			}
		}
		#endregion


		#region //OldMovementClass
		/*
		public abstract class oldMovementClass : PX.Data.IBqlField
		{
		}
		protected String _OldMovementClass;
		[PXString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Old Movement Class", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String OldMovementClass
		{
			get
			{
				return this._OldMovementClass;
			}
			set
			{
				this._OldMovementClass = value;
			}
		}
		*/
		#endregion
		#region //MovementClassFixed
		/*
		public abstract class movementClassFixed : PX.Data.IBqlField { }
		protected bool? _MovementClassFixed = false;
		[PXBool]
		[PXUIField(DisplayName = "Fixed", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual bool? MovementClassFixed
		{
			get
			{
				return this._MovementClassFixed;
			}
			set
			{
				this._MovementClassFixed = value;
			}
		}
		*/
		#endregion
		#region //NewMovementClass
		/*
		public abstract class newMovementClass : PX.Data.IBqlField
		{
		}
		protected String _NewMovementClass;
		[PXString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "New Movement Class", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String NewMovementClass
		{
			get
			{
				return this._NewMovementClass;
			}
			set
			{
				this._NewMovementClass = value;
			}
		}
		*/
		#endregion

	}
	#endregion


	[PX.Objects.GL.TableAndChartDashboardType]
	public class INUpdateABCAssignment : PXGraph<INUpdateABCAssignment>
	{
		public PXCancel<UpdateABCAssignmentSettings> Cancel;

		public PXFilter<UpdateABCAssignmentSettings> UpdateSettings;

		public PXSelectOrderBy<UpdateABCAssignmentResult, OrderBy<Desc<UpdateABCAssignmentResult.ytdCost>>> ResultPreview;

		public PXSelect<INItemSite> itemsite;

		public PXAction<UpdateABCAssignmentSettings> Process;

		[PXUIField(DisplayName = Messages.Process, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable process(PXAdapter adapter)
		{
			//  recalc and save to INItemSite
			CalcABCAssignments(true);

			return adapter.Get();
		}



		public INUpdateABCAssignment()
		{
			ResultPreview.Cache.AllowInsert = false;
			ResultPreview.Cache.AllowDelete = false;
			ResultPreview.Cache.AllowUpdate = false;
		}


		private List<UpdateABCAssignmentResult> CalcABCAssignments (bool updateDB)
		{

			UpdateABCAssignmentSettings us = UpdateSettings.Current;

			List<UpdateABCAssignmentResult> result_list = new List<UpdateABCAssignmentResult>();

			if (us == null) { return result_list; } //empty

			if ( (us.SiteID == null ) || (us.FinPeriodID == null) ) { return result_list; } //empty

			if (updateDB)
			{
				itemsite.Cache.Clear();
			}

			PXSelectBase<INItemSite> cmd = new

				PXSelectJoin<INItemSite,

					InnerJoin<InventoryItem,
						On<InventoryItem.inventoryID, Equal<INItemSite.inventoryID>,
							And<InventoryItem.stkItem, NotEqual<boolFalse>,
							And<Match<InventoryItem, Current<AccessInfo.userName>>>>>,

					LeftJoin<ItemCostHistByPeriodByItemSite,
						On<ItemCostHistByPeriodByItemSite.inventoryID, Equal<INItemSite.inventoryID>,
							And<ItemCostHistByPeriodByItemSite.siteID, Equal<INItemSite.siteID>,
							And<ItemCostHistByPeriodByItemSite.finPeriodID, Equal<Current<UpdateABCAssignmentSettings.finPeriodID>>>>>>>,

				Where<INItemSite.siteID, Equal<Current<UpdateABCAssignmentSettings.siteID>>>,

				OrderBy<Desc<ItemCostHistByPeriodByItemSite.tranYtdCost, Asc<INItemSite.inventoryID>>>>(this);
			// if  'By Fin. Period' option will be added - we'll need change OrderBy here


			PXResultset<INItemSite> intermediateResult = (PXResultset<INItemSite>)cmd.Select();

			//	1. set next non-fixed item position to [0]
			//	2. for each ABC code X starting from 'A' to 'Z'
			//	{
			//		2.1 move items having X code to the result list, counting their cost until cumulative cost not greater than cumulative ABC cost
			//		 (fixed-ABC-code items do not change their code)
			//	}


			// 0.1 
			PXResultset<INABCCode> abc_codes = 
				PXSelectOrderBy<INABCCode,
					//Where<INABCCode.aBCPct, NotEqual<decimal0>>,
					OrderBy<Asc<INABCCode.aBCCodeID>>>.Select(this);

			//0.2
			decimal total_cost_on_site = 0m;
			foreach (PXResult<INItemSite, InventoryItem, ItemCostHistByPeriodByItemSite> it in intermediateResult)
			{
				total_cost_on_site += (((ItemCostHistByPeriodByItemSite)it).TranYtdCost ?? 0m);
			}

			// 0.3
			if ((abc_codes.Count == 0) || (total_cost_on_site == 0))
			{
				// nothing to change :
				foreach (PXResult<INItemSite, InventoryItem, ItemCostHistByPeriodByItemSite> it in intermediateResult)
				{
					INItemSite is_rec = (INItemSite)it;
					InventoryItem ii_rec = (InventoryItem)it;
					ItemCostHistByPeriodByItemSite ich_rec = (ItemCostHistByPeriodByItemSite)it;
					UpdateABCAssignmentResult r_rec = new UpdateABCAssignmentResult();

					r_rec.ABCCodeFixed = is_rec.ABCCodeIsFixed;
					r_rec.Descr = ii_rec.Descr;
					r_rec.InventoryID = is_rec.InventoryID;
					r_rec.OldABCCode = is_rec.ABCCodeID;
					r_rec.NewABCCode = is_rec.ABCCodeID; // null ?

					result_list.Add(r_rec);
				}

				return result_list;

			}


			//	1. set next item position to [0]
			int next_item_to_process = 0;
			decimal cumulative_cost = 0m;
			decimal cumulative_abc_pct = 0m;

			//	2. for each ABC code X starting from 'A' to 'Z'
			foreach (PXResult<INABCCode> abc_it in abc_codes)
			{
				INABCCode abc_rec = (INABCCode) abc_it; 
				cumulative_abc_pct += (abc_rec.ABCPct ?? 0m);

				// 2.1 move items having X code to the result list, counting their cost until cumulative cost not greater than cumulative ABC cost
				// (fixed-ABC-code items do not change their code)

				while (next_item_to_process < intermediateResult.Count)
				{
					PXResult<INItemSite, InventoryItem, ItemCostHistByPeriodByItemSite> it = (PXResult<INItemSite, InventoryItem, ItemCostHistByPeriodByItemSite>)intermediateResult[next_item_to_process];

					INItemSite is_rec = (INItemSite)it;
					InventoryItem ii_rec = (InventoryItem)it;
					ItemCostHistByPeriodByItemSite ich_rec = (ItemCostHistByPeriodByItemSite)it;
					if ( ( (cumulative_cost + (ich_rec.TranYtdCost ?? 0m) ) / total_cost_on_site ) <= ( cumulative_abc_pct / 100m ) )
					{
						cumulative_cost += (ich_rec.TranYtdCost ?? 0m);
						UpdateABCAssignmentResult r_rec = new UpdateABCAssignmentResult();
						r_rec.ABCCodeFixed = is_rec.ABCCodeIsFixed;
						r_rec.Descr = ii_rec.Descr;
						r_rec.InventoryID = is_rec.InventoryID;
						if (is_rec.ABCCodeIsFixed ?? false)
						{
							r_rec.NewABCCode = is_rec.ABCCodeID;
						}
						else
						{
							r_rec.NewABCCode = abc_rec.ABCCodeID;
						}
						r_rec.OldABCCode = is_rec.ABCCodeID;
						r_rec.YtdCost = (ich_rec.TranYtdCost ?? 0m);
						r_rec.Ratio = r_rec.YtdCost/total_cost_on_site*100;
						r_rec.CumulativeRatio = cumulative_cost/total_cost_on_site*100;

						result_list.Add(r_rec);

						if (updateDB && (is_rec.ABCCodeID != r_rec.NewABCCode))
						{
							is_rec.ABCCodeID = r_rec.NewABCCode; 
							itemsite.Update(is_rec);
						}
						next_item_to_process++;
					}
					else
					{
						break;
					}
				}
			}

			if (updateDB)
			{
				this.Actions.PressSave();
			}
			return result_list;
		}

		protected virtual IEnumerable resultPreview()
		{
			return CalcABCAssignments(false);
		}
	}


	#region ItemCostHistByPeriodByItemSite projection

	// projection to calc totals from INItemCostHist grouped by ItemSite combination
	/*

		SELECT 
	 		l.SiteID,
	 		ich.InventoryID,
			ich.FinPeriodID
			--,... some aggregated values
		FROM
					INItemCostHistByPeriod ichbp
			JOIN	INItemCostHist ich
				ON
						ich.InventoryID = ichbp.InventoryID
					AND	ich.CostSubItemID = ichbp.CostSubItemID
					AND	ich.CostSiteID = ichbp.CostSiteID 
					AND	ich.AccountID = ichbp.AccountID 
					AND	ich.SubID = ichbp.SubID
					AND	ich.FinPeriodID = ichbp.LastActivityPeriod
	 		JOIN	INLocation l 
				ON 
							( l.IsCosted = 0 AND l.SiteID = ichbp.CostSiteID )
						OR	( l.IsCosted <> 0 AND l.LocationID = ichbp.CostSiteID )
		GROUP BY 
	 		l.SiteID,	 
	 		ichbp.InventoryID,
			ichbp.FinPeriodID

	*/
	[Serializable()]
	[PXProjection(typeof(Select5<INItemCostHistByPeriod,

		InnerJoin<INItemCostHist,
			On<
					INItemCostHist.inventoryID, Equal<INItemCostHistByPeriod.inventoryID>,
				And<INItemCostHist.costSubItemID, Equal<INItemCostHistByPeriod.costSubItemID>,
				And<INItemCostHist.costSiteID, Equal<INItemCostHistByPeriod.costSiteID>,
				And<INItemCostHist.accountID, Equal<INItemCostHistByPeriod.accountID>,
				And<INItemCostHist.subID, Equal<INItemCostHistByPeriod.subID>,
				And<INItemCostHist.finPeriodID, Equal<INItemCostHistByPeriod.lastActivityPeriod>>>>>>>,

		InnerJoin<INLocation,
			On<
				Where2<

						Where<
										INLocation.isCosted, Equal<boolFalse>,
									And<INItemCostHistByPeriod.costSiteID, Equal<INLocation.siteID>>>,
					Or<
						Where<
									INLocation.isCosted, NotEqual<boolFalse>,
								And<INItemCostHistByPeriod.costSiteID, Equal<INLocation.locationID>>>>>>>>,

		Aggregate<
			GroupBy<INLocation.siteID,
			GroupBy<INItemCostHistByPeriod.inventoryID,
			GroupBy<INItemCostHistByPeriod.finPeriodID,
			Sum<INItemCostHist.finYtdCost,				// for the possible future extension
			Sum<INItemCostHist.tranYtdCost,

			// these four aggregates were added for Movement class assignment process
			Sum<INItemCostHist.finBegQty,  
			Sum<INItemCostHist.tranBegQty,
			Sum<INItemCostHist.finYtdQty,
			Sum<INItemCostHist.tranYtdQty>>>>>>>>>>>))]
	public partial class ItemCostHistByPeriodByItemSite : PX.Data.IBqlTable
	{
		#region SiteID
		public abstract class siteID : PX.Data.IBqlField
		{
		}
		protected Int32? _SiteID;
		[Site(BqlField = typeof(INLocation.siteID))]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[Inventory(BqlField = typeof(INItemCostHistByPeriod.inventoryID))]
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
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.IBqlField { };
		protected String _FinPeriodID;
		[GL.FinPeriodID(BqlField = typeof(INItemCostHistByPeriod.finPeriodID))]
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
		#region FinYtdCost
		public abstract class finYtdCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _FinYtdCost;
		[PXDBBaseCury(BqlField = typeof(INItemCostHist.finYtdCost))]
		public virtual Decimal? FinYtdCost
		{
			get
			{
				return this._FinYtdCost;
			}
			set
			{
				this._FinYtdCost = value;
			}
		}
		#endregion
		#region TranYtdCost
		public abstract class tranYtdCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranYtdCost;
		[PXDBBaseCury(BqlField = typeof(INItemCostHist.tranYtdCost))]
		public virtual Decimal? TranYtdCost
		{
			get
			{
				return this._TranYtdCost;
			}
			set
			{
				this._TranYtdCost = value;
			}
		}
		#endregion

		// these four aggregates were added for Movement class assignment process
		#region FinBegQty
		public abstract class finBegQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _FinBegQty;
		[PXDBQuantity(BqlField = typeof(INItemCostHist.finBegQty))]
		public virtual Decimal? FinBegQty
		{
			get
			{
				return this._FinBegQty;
			}
			set
			{
				this._FinBegQty = value;
			}
		}
		#endregion
		#region TranBegQty
		public abstract class tranBegQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranBegQty;
		[PXDBQuantity(BqlField = typeof(INItemCostHist.tranBegQty))]
		public virtual Decimal? TranBegQty
		{
			get
			{
				return this._TranBegQty;
			}
			set
			{
				this._TranBegQty = value;
			}
		}
		#endregion
		#region FinYtdQty
		public abstract class finYtdQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _FinYtdQty;
		[PXDBQuantity(BqlField = typeof(INItemCostHist.finYtdQty))]
		public virtual Decimal? FinYtdQty
		{
			get
			{
				return this._FinYtdQty;
			}
			set
			{
				this._FinYtdQty = value;
			}
		}
		#endregion
		#region TranYtdQty
		public abstract class tranYtdQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranYtdQty;
		[PXDBQuantity(BqlField = typeof(INItemCostHist.tranYtdQty))]
		public virtual Decimal? TranYtdQty
		{
			get
			{
				return this._TranYtdQty;
			}
			set
			{
				this._TranYtdQty = value;
			}
		}
		#endregion
	}

	#endregion ItemCostHistByItemSite projection
	
	#region //ItemCostHistByItemSite projection // it would be too simple :)

	// projection to calc totals from INItemCostHist gruped by ItemSite combination
	/*

		SELECT 
	 		l.SiteID,
	 		ich.InventoryID,
			ich.FinPeriodID
			--,... some aggregated values
		FROM
					INItemCostHist ich
	 		JOIN	INLocation l 
				ON 
							( l.IsCosted = 0 AND l.SiteID = ich.CostSiteID )
						OR	( l.IsCosted <> 0 AND l.LocationID = ich.CostSiteID )
		GROUP BY 
	 		l.SiteID,	 
	 		ich.InventoryID,
			ich.FinPeriodID

	*/
	/*
	[Serializable()]
	[PXProjection(typeof(Select5<INItemCostHist,
		InnerJoin<INLocation,
			On<
				Where2<

						Where<
										INLocation.isCosted, Equal<boolFalse>,
									And<INItemCostHist.costSiteID, Equal<INLocation.siteID>>>,
					Or<
						Where<
									INLocation.isCosted, NotEqual<boolFalse>,
								And<INItemCostHist.costSiteID, Equal<INLocation.locationID>>>>>>>,

		Aggregate<
			GroupBy<INLocation.siteID,
			GroupBy<INItemCostHist.inventoryID,
			GroupBy<INItemCostHist.finPeriodID,
			Sum<INItemCostHist.finYtdCost,				// for the possible future extension
			Sum<INItemCostHist.tranYtdCost>>>>>>>))]
	public partial class ItemCostHistByItemSite : PX.Data.IBqlTable
	{
		#region SiteID
		public abstract class siteID : PX.Data.IBqlField
		{
		}
		protected Int32? _SiteID;
		[Site(BqlField = typeof(INLocation.siteID))]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[Inventory(BqlField = typeof(INItemCostHist.inventoryID))]
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
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.IBqlField { };
		protected String _FinPeriodID;
		[GL.FinPeriodID(BqlField = typeof(INItemCostHist.finPeriodID))]
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
		#region FinYtdCost			
		public abstract class finYtdCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _FinYtdCost;
		[PXDBBaseCury(BqlField = typeof(INItemCostHist.finYtdCost))]
		public virtual Decimal? FinYtdCost
		{
			get
			{
				return this._FinYtdCost;
			}
			set
			{
				this._FinYtdCost = value;
			}
		}
		#endregion   
		#region TranYtdCost
		public abstract class tranYtdCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranYtdCost;
		[PXDBBaseCury(BqlField = typeof(INItemCostHist.tranYtdCost))]
		public virtual Decimal? TranYtdCost
		{
			get
			{
				return this._TranYtdCost;
			}
			set
			{
				this._TranYtdCost = value;
			}
		}
		#endregion
	}
	*/
	#endregion ItemCostHistByItemSite projection
}
