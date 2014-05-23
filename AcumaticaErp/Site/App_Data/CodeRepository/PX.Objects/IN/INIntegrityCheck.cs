using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.IN.Overrides.INDocumentRelease;
using PX.Objects.CS;
using Company = PX.Objects.GL.Company;
using SOOrderType = PX.Objects.SO.SOOrderType;
using SOShipLineSplit = PX.Objects.SO.SOShipLineSplit;
using POReceiptLineSplit = PX.Objects.PO.POReceiptLineSplit;
using PX.Objects.GL;
using PX.Objects.SO;

namespace PX.Objects.IN
{
    [Serializable]
	public partial class ReadOnlySiteStatus : INSiteStatus
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? InventoryID
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
		public new abstract class subItemID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? SubItemID
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
		public new abstract class siteID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? SiteID
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
	}

    [Serializable]
	public partial class ReadOnlyLocationStatus : INLocationStatus
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? InventoryID
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
		public new abstract class subItemID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? SubItemID
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
		public new abstract class siteID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? SiteID
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
		public new abstract class locationID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? LocationID
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
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.IBqlField
		{
		}
		#endregion
	}

	[PXProjection(typeof(Select5<INItemSite, InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INItemSite.inventoryID>>>, Where<boolTrue, Equal<boolTrue>>, Aggregate<GroupBy<INItemSite.inventoryID, GroupBy<INItemSite.siteID>>>>), Persistent = true)]
    [Serializable]
	public partial class INItemSiteSummary : PX.Data.IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.IBqlField
		{
		}
		protected Boolean? _Selected = false;
		[PXBool()]
		[PXUIField(DisplayName = "Selected")]
		public virtual Boolean? Selected
		{
			get
			{
				return this._Selected;
			}
			set
			{
				this._Selected = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[PXDBInt(BqlField = typeof(INItemSite.inventoryID))]
		[PXDefault()]
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
		#region InventoryCD
		public abstract class inventoryCD : PX.Data.IBqlField
		{
		}
		protected string _InventoryCD;
		[PXDBString(BqlField = typeof(InventoryItem.inventoryCD), InputMask = "", IsKey = true)]
		[PXUIField(DisplayName = "Inventory ID")]
		[InventoryCD()]
		[PXDefault()]
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
		public class InventoryCDAttribute : PXAggregateAttribute
		{
			public InventoryCDAttribute()
				: base()
			{
				PXDimensionAttribute attr = new PXDimensionAttribute(InventoryAttribute.DimensionName);
				attr.ValidComboRequired = true;
				_Attributes.Add(attr);

				PXSelectorAttribute selectorattr = new PXSelectorAttribute(typeof(Search<InventoryItem.inventoryCD, Where<Match<Current<AccessInfo.userName>>>>));
				_Attributes.Add(selectorattr);
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.IBqlField
		{
		}
		protected Int32? _SiteID;
		[IN.Site(IsKey = true, BqlField = typeof(INItemSite.siteID), DescriptionField = null)]
		[PXDefault()]
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
	}

	[PX.Objects.GL.TableAndChartDashboardType]
	public class INIntegrityCheck : PXGraph<INIntegrityCheck>
	{
		public PXCancel<INSiteFilter> Cancel;
		public PXFilter<INSiteFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessingJoin<INItemSiteSummary,
			INSiteFilter,
			LeftJoin<INSiteStatusSummary, On<INSiteStatusSummary.inventoryID, Equal<INItemSiteSummary.inventoryID>,
				And<INSiteStatusSummary.siteID, Equal<INItemSiteSummary.siteID>>>>,
			Where<boolTrue, Equal<boolTrue>>>
			INItemList;
		public PXSetup<INSetup> insetup;
		public PXSelect<INSiteStatus> sitestatus_s;
		public PXSelect<SiteStatus> sitestatus;
		public PXSelect<LocationStatus> locationstatus;
		public PXSelect<LotSerialStatus> lotserialstatus;
        public PXSelect<ItemLotSerial> itemlotserial;
		public PXSelect<INItemPlan> initemplan;

		public PXSelect<ItemSiteHist> itemsitehist;
        public PXSelect<ItemSiteHistD> itemsitehistd;
        public PXSelect<ItemCostHist> itemcosthist;
		public PXSelect<ItemSalesHistD> itemsalehistd;
		public PXSelect<ItemCustSalesStats> itemcustsalesstats;

		public INIntegrityCheck()
		{
			INSetup record = insetup.Current;

			INItemList.SetProcessCaption(Messages.Process);
			INItemList.SetProcessAllCaption(Messages.ProcessAll);

			PXDBDefaultAttribute.SetDefaultForUpdate<INTranSplit.refNbr>(this.Caches[typeof(INTranSplit)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<INTranSplit.tranDate>(this.Caches[typeof(INTranSplit)], null, false);
		}

		protected virtual void INSiteFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;

            INSiteFilter filter = (INSiteFilter)e.Row;

			INItemList.SetProcessDelegate<INIntegrityCheck>(delegate(INIntegrityCheck graph, INItemSiteSummary itemsite)
			{
				graph.Clear(PXClearOption.PreserveTimeStamp);
				graph.IntegrityCheckProc(itemsite, filter != null && filter.RebuildHistory == true ? filter.FinPeriodID : null, filter.ReplanBackorders == true);
			});
			PXUIFieldAttribute.SetEnabled<INSiteFilter.finPeriodID>(sender, null, filter.RebuildHistory == true);
		}

		/*
		public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			if (PXLongOperation.Exists(this.UID))
			{
				int intstartRow = 0;
				int inttotalRows = 0;
				return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref intstartRow, maximumRows, ref inttotalRows);
			}
			else
			{
				return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, (filters != null && filters.Length > 0 ? 0 : maximumRows), ref totalRows);
			}
		}
		*/
		protected virtual void INItemSiteSummary_InventoryCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		public virtual IEnumerable initemlist()
		{
			foreach (INItemSiteSummary cached in INItemList.Cache.Updated)
			{
				yield return new PXResult<INItemSiteSummary, INSiteStatusSummary>(cached, null);
			}

			var view = new PXView(this, true, new Select2<INItemSiteSummary,
				FullJoin<INSiteStatusSummary, On<INSiteStatusSummary.inventoryID, Equal<INItemSiteSummary.inventoryID>, And<INSiteStatusSummary.siteID, Equal<INItemSiteSummary.siteID>>>,
				InnerJoin<InventoryItem, On2<Where<InventoryItem.inventoryID, Equal<INItemSiteSummary.inventoryID>, Or<InventoryItem.inventoryID, Equal<INSiteStatusSummary.inventoryID>>>,
				And<Match<InventoryItem, Current<AccessInfo.userName>>>>>>,
				Where2<Where<INItemSiteSummary.siteID, Equal<Current<INSiteFilter.siteID>>, Or<INSiteStatusSummary.siteID, Equal<Current<INSiteFilter.siteID>>>>,
					And<InventoryItem.stkItem, Equal<True>>>,
				OrderBy<Asc<InventoryItem.inventoryCD>>>());
			var startRow = PXView.StartRow;
			var totalRows = 0;

			foreach (PXResult<INItemSiteSummary, INSiteStatusSummary, InventoryItem> res in view.Select(null, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
								 ref startRow, PXView.MaximumRows, ref totalRows))
			{
				INItemSiteSummary itemsite = res;
				INSiteStatusSummary summary = res;
				object cached;

				if (itemsite.InventoryID == null)
				{
					itemsite = new INItemSiteSummary();
					itemsite.InventoryID = summary.InventoryID;
					itemsite.SiteID = summary.SiteID;
					itemsite.InventoryCD = ((InventoryItem)res).InventoryCD;
				}

				if (summary.InventoryID == null)
				{
					summary = new INSiteStatusSummary();
					summary.InventoryID = itemsite.InventoryID;
					summary.SiteID = itemsite.SiteID;
				}

				if ((cached = INItemList.Cache.Locate(itemsite)) == null)
				{
					INItemList.Cache.SetStatus(itemsite, PXEntryStatus.Held);
					yield return new PXResult<INItemSiteSummary, INSiteStatusSummary>(itemsite, summary);
				}
				else if (INItemList.Cache.GetStatus(cached) == PXEntryStatus.Notchanged || INItemList.Cache.GetStatus(cached) == PXEntryStatus.Held)
				{
					yield return new PXResult<INItemSiteSummary, INSiteStatusSummary>((INItemSiteSummary)cached, summary);
				}
			}
			PXView.StartRow = 0;
		}

		public virtual void IntegrityCheckProc(INItemSiteSummary itemsite, string minPeriod, bool replanBackorders)
		{
			using (PXConnectionScope cs = new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					foreach (INItemPlan p in PXSelectReadonly2<INItemPlan, LeftJoin<Note, On<Note.noteID, Equal<INItemPlan.refNoteID>>>, Where<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>, And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>, And<Note.noteID, IsNull>>>>.SelectMultiBound(this, new object[] { itemsite }))
					{
						PXDatabase.Delete<INItemPlan>(new PXDataFieldRestrict("PlanID", PXDbType.BigInt, 8, p.PlanID, PXComp.EQ));
					}

					foreach (INItemPlan p in PXSelectReadonly2<INItemPlan, 
						InnerJoin<INRegister, On<INRegister.noteID, Equal<INItemPlan.refNoteID>, And<INRegister.siteID, Equal<INItemPlan.siteID>>>>, 
						Where<INRegister.docType, Equal<INDocType.transfer>,
							And<INRegister.released, Equal<boolTrue>, 
							And<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>, 
							And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>>>>>>.SelectMultiBound(this, new object[] { itemsite }))
					{
						PXDatabase.Delete<INItemPlan>(new PXDataFieldRestrict("PlanID", PXDbType.BigInt, 8, p.PlanID, PXComp.EQ));
					}

					foreach (PXResult<INTranSplit, INRegister, INSite, INItemSite> res in PXSelectJoin<INTranSplit, 
						InnerJoin<INRegister, On<INRegister.docType, Equal<INTranSplit.docType>, And<INRegister.refNbr, Equal<INTranSplit.refNbr>>>, 
						InnerJoin<INSite, On<INSite.siteID, Equal<INRegister.toSiteID>>,
						LeftJoin<INItemSite, On<INItemSite.inventoryID, Equal<INTranSplit.inventoryID>, And<INItemSite.siteID, Equal<INRegister.toSiteID>>>,
						LeftJoin<INTran, On<INTran.origTranType, Equal<INTranSplit.tranType>, And<INTran.origRefNbr, Equal<INTranSplit.refNbr>, And<INTran.origLineNbr, Equal<INTranSplit.lineNbr>>>>, 
						LeftJoin<INItemPlan, On<INItemPlan.planID, Equal<INTranSplit.planID>>>>>>>, 
						Where<INRegister.docType, Equal<INDocType.transfer>,
							And2<Where<INRegister.transferType, Equal<INTransferType.twoStep>, And<INRegister.released, Equal<boolTrue>, And<INTranSplit.released, Equal<boolTrue>, 
								Or<INRegister.released, Equal<boolFalse>>>>>, 
							And<INTranSplit.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>, 
							And<INTranSplit.siteID, Equal<Current<INItemSiteSummary.siteID>>, 
							And<INItemPlan.planID, IsNull, 
							And<INTran.refNbr, IsNull>>>>>>>.SelectMultiBound(this, new object[] { itemsite }))
					{
						INTranSplit split = res;
						INRegister doc = res;

						INItemPlan plan = INItemPlanIDAttribute.DefaultValues(this.Caches[typeof(INTranSplit)], res);
						if (plan.LocationID == null)
						{
							plan.LocationID = ((INItemSite)res).DfltReceiptLocationID ?? ((INSite)res).ReceiptLocationID;
						}

						plan = (INItemPlan)this.Caches[typeof(INItemPlan)].Insert(plan);

						split.PlanID = plan.PlanID;
						Caches[typeof(INTranSplit)].SetStatus(split, PXEntryStatus.Updated);
					}

					PXDatabase.Update<INSiteStatus>(
							new PXDataFieldRestrict("InventoryID", PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
							new PXDataFieldRestrict("SiteID", PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
							new PXDataFieldAssign("QtyAvail", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyHardAvail", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyNotAvail", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyINIssues", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyINReceipts", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyINTransit", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyINAssemblySupply", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyINAssemblyDemand", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyINReplaned", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPOPrepared", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPOOrders", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPOReceipts", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtySOBooked", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtySOShipped", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtySOShipping", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtySOBackOrdered", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtySOFixed", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPOFixedOrders", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPOFixedPrepared", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPOFixedReceipts", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtySODropShip", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPODropShipOrders", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPODropShipPrepared", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPODropShipReceipts", PXDbType.Decimal, 0m)
						);

					PXDatabase.Update<INLocationStatus>(
							new PXDataFieldRestrict("InventoryID", PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
							new PXDataFieldRestrict("SiteID", PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
							new PXDataFieldAssign("QtyAvail", PXDbType.DirectExpression, "QtyOnHand"),
							new PXDataFieldAssign("QtyHardAvail", PXDbType.DirectExpression, "QtyOnHand"),
							new PXDataFieldAssign("QtyINIssues", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyINReceipts", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyINTransit", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyINAssemblySupply", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyINAssemblyDemand", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPOPrepared", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPOOrders", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPOReceipts", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtySOBooked", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtySOShipped", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtySOShipping", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtySOBackOrdered", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtySOFixed", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPOFixedOrders", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPOFixedPrepared", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPOFixedReceipts", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtySODropShip", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPODropShipOrders", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPODropShipPrepared", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPODropShipReceipts", PXDbType.Decimal, 0m)
						);

					PXDatabase.Update<INLotSerialStatus>(
							new PXDataFieldRestrict("InventoryID", PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
							new PXDataFieldRestrict("SiteID", PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
							new PXDataFieldAssign("QtyAvail", PXDbType.DirectExpression, "QtyOnHand"),
							new PXDataFieldAssign("QtyHardAvail", PXDbType.DirectExpression, "QtyOnHand"),
							new PXDataFieldAssign("QtyINIssues", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyINReceipts", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyINTransit", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyINAssemblySupply", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyINAssemblyDemand", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPOPrepared", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPOOrders", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPOReceipts", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtySOBooked", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtySOShipped", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtySOShipping", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtySOBackOrdered", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtySOFixed", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPOFixedOrders", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPOFixedPrepared", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPOFixedReceipts", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtySODropShip", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPODropShipOrders", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPODropShipPrepared", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyPODropShipReceipts", PXDbType.Decimal, 0m)
						);

                    PXDatabase.Update<INItemLotSerial>(
                            new PXDataFieldRestrict("InventoryID", PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
                            new PXDataFieldAssign("QtyAvail", PXDbType.DirectExpression, "QtyOnHand"),
                            new PXDataFieldAssign("QtyHardAvail", PXDbType.DirectExpression, "QtyOnHand"),
                            new PXDataFieldAssign("QtyINTransit", PXDbType.Decimal, 0m)
                        );


					foreach (PXResult<ReadOnlyLocationStatus, INLocation> res in PXSelectJoinGroupBy<ReadOnlyLocationStatus, InnerJoin<INLocation, On<INLocation.locationID, Equal<ReadOnlyLocationStatus.locationID>>>, Where<ReadOnlyLocationStatus.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>, And<ReadOnlyLocationStatus.siteID, Equal<Current<INItemSiteSummary.siteID>>>>, Aggregate<GroupBy<ReadOnlyLocationStatus.inventoryID, GroupBy<ReadOnlyLocationStatus.siteID, GroupBy<ReadOnlyLocationStatus.subItemID, GroupBy<INLocation.inclQtyAvail, Sum<ReadOnlyLocationStatus.qtyOnHand>>>>>>>.SelectMultiBound(this, new object[] { itemsite }))
					{
						SiteStatus status = new SiteStatus();
						status.InventoryID = ((ReadOnlyLocationStatus)res).InventoryID;
						status.SubItemID = ((ReadOnlyLocationStatus)res).SubItemID;
						status.SiteID = ((ReadOnlyLocationStatus)res).SiteID;
						status = (SiteStatus)sitestatus.Cache.Insert(status);

						if (((INLocation)res).InclQtyAvail == true)
						{
							status.QtyAvail += ((ReadOnlyLocationStatus)res).QtyOnHand;
							status.QtyHardAvail += ((ReadOnlyLocationStatus)res).QtyOnHand;
						}
						else
						{
							status.QtyNotAvail += ((ReadOnlyLocationStatus)res).QtyOnHand;
						}
					}

					INPlanType plan60 = PXSelect<INPlanType, Where<INPlanType.planType, Equal<INPlanConstants.plan60>>>.Select(this);
					INPlanType plan70 = PXSelect<INPlanType, Where<INPlanType.planType, Equal<INPlanConstants.plan70>>>.Select(this);
					INPlanType plan74 = PXSelect<INPlanType, Where<INPlanType.planType, Equal<INPlanConstants.plan74>>>.Select(this);
					INPlanType plan76 = PXSelect<INPlanType, Where<INPlanType.planType, Equal<INPlanConstants.plan76>>>.Select(this);

					foreach (PXResult<INItemPlan, INPlanType, SOShipLineSplit, POReceiptLineSplit> res in PXSelectJoin<INItemPlan, 
						InnerJoin<INPlanType, On<INPlanType.planType, Equal<INItemPlan.planType>>, 
						LeftJoin<SOShipLineSplit, On<SOShipLineSplit.planID, Equal<INItemPlan.planID>>, 
						LeftJoin<POReceiptLineSplit, On<POReceiptLineSplit.planID, Equal<INItemPlan.planID>>>>>, 
						Where<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>, And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>>>>.SelectMultiBound(this, new object[] { itemsite }))
					{
						INItemPlan plan = (INItemPlan)res;
						INPlanType plantype = (INPlanType)res;
						INPlanType locplantype;
						SOShipLineSplit sosplit = (SOShipLineSplit)res;
						POReceiptLineSplit posplit = (POReceiptLineSplit)res;

						if (plan.InventoryID != null &&
								plan.SubItemID != null &&
								plan.SiteID != null)
						{
							switch (plan.PlanType)
							{
								case INPlanConstants.Plan61:
									if (sosplit.ShipmentNbr == null)
									{
										PXDatabase.Delete<INItemPlan>(new PXDataFieldRestrict("PlanID", PXDbType.BigInt, 8, plan.PlanID, PXComp.EQ));
										continue;
									}

									SOOrderType ordetype = PXSelect<SOOrderType, Where<SOOrderType.orderType, Equal<Current<SOShipLineSplit.origOrderType>>>>.SelectSingleBound(this, new object[] { sosplit });

									if (ordetype.RequireLocation == false)
									{
										locplantype = plantype * (plan60 ^ plantype);
									}
									else
									{
										locplantype = plantype;
									}

									if (sosplit.IsComponentItem == true)
									{
										plantype = locplantype;
									}

									break;
								case INPlanConstants.Plan63:
									locplantype = plantype;

									if (sosplit.ShipmentNbr != null)
									{
										plantype = plantype * (plantype ^ plantype);
									}
									break;
								case INPlanConstants.Plan71:
								case INPlanConstants.Plan72:
									if (posplit.ReceiptNbr == null)
									{
										PXDatabase.Delete<INItemPlan>(new PXDataFieldRestrict("PlanID", PXDbType.BigInt, 8, plan.PlanID, PXComp.EQ));
										continue;
									}
									locplantype = plantype * (plan70 ^ plantype);
									if (posplit.PONbr == null)
									{
										plantype = plantype * (plan70 ^ plantype);
									}
									break;
								case INPlanConstants.Plan77:
									locplantype = plantype * (plan76 ^ plantype);
									if (posplit.ReceiptNbr == null || posplit.PONbr == null)
									{
										plantype = plantype * (plan76 ^ plantype);
									}
									break;
								case INPlanConstants.Plan75:
									locplantype = plantype * (plan74 ^ plantype);
									if (posplit.ReceiptNbr == null || posplit.PONbr == null)
									{
										plantype = plantype * (plan74 ^ plantype);
									}
									break;
								default:
									locplantype = plantype;
									break;
							}

							if (plan.LocationID != null)
							{
								LocationStatus item = INItemPlanIDAttribute.UpdateAllocatedQuantitiesBase<LocationStatus>(this, plan, locplantype, true);
								INItemPlanIDAttribute.UpdateAllocatedQuantitiesBase<SiteStatus>(this, plan, plantype, (bool)item.InclQtyAvail);
								if (!string.IsNullOrEmpty(plan.LotSerialNbr))
								{
									INItemPlanIDAttribute.UpdateAllocatedQuantitiesBase<LotSerialStatus>(this, plan, locplantype, true);
                                    INItemPlanIDAttribute.UpdateAllocatedQuantitiesBase<ItemLotSerial>(this, plan, locplantype, true);

								}
							}
							else
							{
								INItemPlanIDAttribute.UpdateAllocatedQuantitiesBase<SiteStatus>(this, plan, plantype, true);
							}
						}
					}
					if (replanBackorders)
						INReleaseProcess.ReplanBackOrders(this);

					Caches[typeof(INTranSplit)].Persist(PXDBOperation.Update);

					sitestatus.Cache.Persist(PXDBOperation.Insert);
					sitestatus.Cache.Persist(PXDBOperation.Update);

					locationstatus.Cache.Persist(PXDBOperation.Insert);
					locationstatus.Cache.Persist(PXDBOperation.Update);

					lotserialstatus.Cache.Persist(PXDBOperation.Insert);
					lotserialstatus.Cache.Persist(PXDBOperation.Update);

                    itemlotserial.Cache.Persist(PXDBOperation.Insert);
                    itemlotserial.Cache.Persist(PXDBOperation.Update);


					if (minPeriod != null)
					{
						FinPeriod period =
							PXSelect<FinPeriod,
								Where<FinPeriod.finPeriodID, Equal<Required<FinPeriod.finPeriodID>>>>
								.SelectWindowed(this, 0, 1, minPeriod);
						if (period == null) return;
						DateTime startDate = (DateTime)period.StartDate;

						PXDatabase.Delete<INItemCostHist>(
							new PXDataFieldRestrict("InventoryID", PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
							new PXDataFieldRestrict("CostSiteID", PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
							new PXDataFieldRestrict("FinPeriodID", PXDbType.Char, 6, minPeriod, PXComp.GE)
							);

						PXDatabase.Delete<INItemSalesHistD>(
							new PXDataFieldRestrict("InventoryID", PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
							new PXDataFieldRestrict("SiteID", PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
							new PXDataFieldRestrict("QtyPlanSales", PXDbType.Decimal, 0m),
							new PXDataFieldRestrict("SDate", PXDbType.DateTime, 8, startDate, PXComp.GE)

							);
						PXDatabase.Delete<INItemCustSalesStats>(
							new PXDataFieldRestrict("InventoryID", PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
							new PXDataFieldRestrict("SiteID", PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
							new PXDataFieldRestrict("LastDate", PXDbType.DateTime, 8, startDate, PXComp.GE));

						PXDatabase.Update<INItemSalesHistD>(
							new PXDataFieldAssign("QtyIssues", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("QtyExcluded", PXDbType.Decimal, 0m),
							new PXDataFieldRestrict("InventoryID", PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
							new PXDataFieldRestrict("SiteID", PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
							new PXDataFieldRestrict("SDate", PXDbType.DateTime, 8, startDate, PXComp.GE)
							);

						foreach (INLocation loc in PXSelectReadonly2<INLocation, InnerJoin<INItemCostHist, On<INItemCostHist.costSiteID, Equal<INLocation.locationID>>>, Where<INLocation.siteID, Equal<Current<INItemSiteSummary.siteID>>, And<INItemCostHist.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>>>>.SelectMultiBound(this, new object[] { itemsite }))
						{
							PXDatabase.Delete<INItemCostHist>(
								new PXDataFieldRestrict("InventoryID", PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
								new PXDataFieldRestrict("CostSiteID", PXDbType.Int, 4, loc.LocationID, PXComp.EQ),
								new PXDataFieldRestrict("FinPeriodID", PXDbType.Char, 6, minPeriod, PXComp.GE)
								);
						}

						PXDatabase.Delete<INItemSiteHist>(
							new PXDataFieldRestrict("InventoryID", PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
							new PXDataFieldRestrict("SiteID", PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
							new PXDataFieldRestrict("FinPeriodID", PXDbType.Char, 6, minPeriod, PXComp.GE)
							);

                        PXDatabase.Delete<INItemSiteHistD>(
                            new PXDataFieldRestrict("InventoryID", PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
                            new PXDataFieldRestrict("SiteID", PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
                            new PXDataFieldRestrict("SDate", PXDbType.DateTime, 8, startDate, PXComp.GE)
                            );

                        INTran prev_tran = null;
						foreach (PXResult<INTran, INTranSplit> res in PXSelectReadonly2<INTran, InnerJoin<INTranSplit, On<INTranSplit.tranType, Equal<INTran.tranType>, And<INTranSplit.refNbr, Equal<INTran.refNbr>, And<INTranSplit.lineNbr, Equal<INTran.lineNbr>>>>>, Where<INTran.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>, And<INTran.siteID, Equal<Current<INItemSiteSummary.siteID>>, And<INTran.finPeriodID, GreaterEqual<Required<INTran.finPeriodID>>, And<INTran.released, Equal<boolTrue>>>>>, OrderBy<Asc<INTran.tranType, Asc<INTran.refNbr, Asc<INTran.lineNbr>>>>>.SelectMultiBound(this, new object[] { itemsite }, minPeriod))
						{
							INTran tran = res;
							INTranSplit split = res;

							if (!Caches[typeof(INTran)].ObjectsEqual(prev_tran, tran))
							{
								INReleaseProcess.UpdateSalesHistD(this, tran);
								INReleaseProcess.UpdateCustSalesStats(this, tran);

								prev_tran = tran;
							}

							if (split.BaseQty != 0m)
							{
								INReleaseProcess.UpdateSiteHist(this, res, split);
								INReleaseProcess.UpdateSiteHistD(this, split);
							}
						}

						foreach (PXResult<INTran, INTranCost> res in PXSelectReadonly2<INTran, InnerJoin<INTranCost, On<INTranCost.tranType, Equal<INTran.tranType>, And<INTranCost.refNbr, Equal<INTran.refNbr>, And<INTranCost.lineNbr, Equal<INTran.lineNbr>>>>>, Where<INTran.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>, And<INTran.siteID, Equal<Current<INItemSiteSummary.siteID>>, And<INTranCost.finPeriodID, GreaterEqual<Required<INTran.finPeriodID>>, And<INTran.released, Equal<boolTrue>>>>>>.SelectMultiBound(this, new object[] { itemsite }, minPeriod))
						{
							INReleaseProcess.UpdateCostHist(this, (INTranCost)res, (INTran)res);
						}						

						itemcosthist.Cache.Persist(PXDBOperation.Insert);
						itemcosthist.Cache.Persist(PXDBOperation.Update);

						itemsitehist.Cache.Persist(PXDBOperation.Insert);
						itemsitehist.Cache.Persist(PXDBOperation.Update);

                        itemsitehistd.Cache.Persist(PXDBOperation.Insert);
                        itemsitehistd.Cache.Persist(PXDBOperation.Update);
                        
                        itemsalehistd.Cache.Persist(PXDBOperation.Insert);
						itemsalehistd.Cache.Persist(PXDBOperation.Update);

						itemcustsalesstats.Cache.Persist(PXDBOperation.Insert);
						itemcustsalesstats.Cache.Persist(PXDBOperation.Update);
					}

					ts.Complete();
				}

				sitestatus.Cache.Persisted(false);
				locationstatus.Cache.Persisted(false);
				lotserialstatus.Cache.Persisted(false);

				itemcosthist.Cache.Persisted(false);
				itemsitehist.Cache.Persisted(false);
                itemsitehistd.Cache.Persisted(false);
            }
		}
	}
}
