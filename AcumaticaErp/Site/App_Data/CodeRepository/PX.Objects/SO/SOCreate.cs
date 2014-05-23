using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.AR;
using PX.TM;
using PX.Objects.PO;

namespace PX.Objects.SO
{
	[PX.Objects.GL.TableAndChartDashboardType]
    [Serializable]
	public class SOCreate : PXGraph<SOCreate>
	{
		public PXCancel<SOCreateFilter> Cancel;
		public PXFilter<SOCreateFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessingJoin<SOFixedDemand, SOCreateFilter,
			InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<SOFixedDemand.inventoryID>>,			
			LeftJoin<SOOrder, On<SOOrder.noteID, Equal<SOFixedDemand.refNoteID>>,
			LeftJoin<SOLine, On<SOLine.planID, Equal<SOFixedDemand.planID>>>>>,			
			Where2<Where<SOFixedDemand.inventoryID, Equal<Current<SOCreateFilter.inventoryID>>, Or<Current<SOCreateFilter.inventoryID>, IsNull>>,
			And2<Where<SOFixedDemand.siteID, Equal<Current<SOCreateFilter.siteID>>, Or<Current<SOCreateFilter.siteID>, IsNull>>,
			And<Where<InventoryItem.itemClassID, Equal<Current<SOCreateFilter.itemClassID>>, Or<Current<SOCreateFilter.itemClassID>, IsNull>>>>>> FixedDemand;

		public SOCreate()
		{			
			PXUIFieldAttribute.SetEnabled<SOFixedDemand.replenishmentSourceSiteID>(FixedDemand.Cache, null, true);			

			PXUIFieldAttribute.SetDisplayName<InventoryItem.descr>(this.Caches[typeof(InventoryItem)], PO.Messages.InventoryItemDescr);
			PXUIFieldAttribute.SetDisplayName<INSite.descr>(this.Caches[typeof(INSite)], Messages.SiteDescr);
			PXUIFieldAttribute.SetDisplayName<INPlanType.descr>(this.Caches[typeof(INPlanType)], PO.Messages.PlanTypeDescr);												
		}

		protected virtual void SOCreateFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOCreateFilter filter = Filter.Current;

			FixedDemand.SetProcessDelegate(list => SOCreateProc(list, filter.PurchDate));

			TimeSpan span;
			Exception message;
			PXLongRunStatus status = PXLongOperation.GetStatus(this.UID, out span, out message);

			PXUIFieldAttribute.SetVisible<SOLine.orderNbr>(Caches[typeof(SOLine)], null, (status == PXLongRunStatus.Completed || status == PXLongRunStatus.Aborted));

		}

		public static void SOCreateProc(List<SOFixedDemand> list, DateTime? PurchDate)
		{
			SOOrderEntry docgraph = PXGraph.CreateInstance<SOOrderEntry>();
			SOSetup sosetup = docgraph.sosetup.Current;
			DocumentList<SOOrder> created = new DocumentList<SOOrder>(docgraph);

			foreach (SOFixedDemand demand in list)
			{
				string OrderType = 
					sosetup.TransferOrderType ?? SOOrderTypeConstants.TransferOrder;

				string ErrorText = string.Empty;

				try
				{
					if (demand.ReplenishmentSourceSiteID == null)
					{
						PXProcessing<SOFixedDemand>.SetWarning(list.IndexOf(demand), Messages.MissingSourceSite);
						continue;
					}

					if( demand.ReplenishmentSourceSiteID == demand.SiteID)
					{
						PXProcessing<SOFixedDemand>.SetWarning(list.IndexOf(demand), Messages.EqualSourceDestinationSite);
						continue;						
					}
					SOOrder order;					
					POLine poline = PXSelect<POLine, Where<POLine.planID, Equal<Required<POLine.planID>>>>.Select(docgraph, demand.PlanID);

					if (poline != null)
					{												
						order =
							created.Find<SOOrder.orderType, SOOrder.destinationSiteID, SOOrder.defaultSiteID, SOOrder.origPOType, SOOrder.origPONbr>(
								OrderType, demand.SiteID, demand.ReplenishmentSourceSiteID, poline.OrderType, poline.OrderNbr);
					}
					else
						order = created.Find<SOOrder.orderType, SOOrder.destinationSiteID, SOOrder.defaultSiteID>(OrderType, demand.SiteID, demand.ReplenishmentSourceSiteID);

					if(order == null) order = new SOOrder();
					
					if (order.OrderNbr == null)
					{
						docgraph.Clear();

						INSite sourceSite = PXSelect<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>.Select(docgraph, demand.ReplenishmentSourceSiteID);
						order.BranchID = sourceSite.BranchID;
						order.OrderType = OrderType;
						order = PXCache<SOOrder>.CreateCopy(docgraph.Document.Insert(order));
						order.DefaultSiteID = demand.ReplenishmentSourceSiteID;
						order.DestinationSiteID = demand.SiteID;
						order.Status = SOOrderStatus.Open;
						order.OrderDate = PurchDate;
						if (poline != null)
						{
							order.OrigPOType = poline.OrderType;
							order.OrigPONbr = poline.OrderNbr;								
						}
						docgraph.Document.Update(order);
					}
					else if (docgraph.Document.Cache.ObjectsEqual(docgraph.Document.Current, order) == false)
					{
						docgraph.Document.Current = docgraph.Document.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderType);
					}
					SOLine line = new SOLine();
					line = PXCache<SOLine>.CreateCopy(docgraph.Transactions.Insert(line));						
					line.InventoryID = demand.InventoryID;
					line.SubItemID = demand.SubItemID;
					line.SiteID = demand.ReplenishmentSourceSiteID;
					line.UOM = demand.UOM;
					line.OrderQty = demand.OrderQty;
					if(poline != null)
					{
						line.OrigPOType = poline.OrderType;
						line.OrigPONbr = poline.OrderNbr;
						line.OrigPOLineNbr = poline.LineNbr;
					}
					
					line = docgraph.Transactions.Update(line);
					
					if(line.PlanID == null)
						throw new PXRowPersistedException(typeof(SOLine).Name, line, RQ.Messages.UnableToCreateSOOrders);

					PXCache cache = docgraph.Caches[typeof(INItemPlan)];
					string demandPlanType = demand.PlanType;
					//cache.SetStatus(demand, PXEntryStatus.Updated);					
					//demand.SupplyPlanID = line.PlanID;					
					INItemPlan rp = PXCache<INItemPlan>.CreateCopy(demand);
					cache.RaiseRowDeleted(demand);
					rp.PlanType = INPlanConstants.Plan94;
					rp.FixedSource = null;
					rp.SupplyPlanID = line.PlanID;
					cache.RaiseRowInserted(rp);
					cache.SetStatus(rp, PXEntryStatus.Updated);					

					if (docgraph.Transactions.Cache.IsInsertedUpdatedDeleted)
					{
						using (PXTransactionScope scope = new PXTransactionScope())
						{
							docgraph.Save.Press();
							if (demandPlanType == INPlanConstants.Plan90)
							{
								docgraph.Replenihment.Current = docgraph.Replenihment.Search<INReplenishmentOrder.noteID>(demand.RefNoteID);
								if (docgraph.Replenihment.Current != null)
								{
									INReplenishmentLine rLine =
										PXCache<INReplenishmentLine>.CreateCopy(docgraph.ReplenishmentLines.Insert(new INReplenishmentLine()));
									rLine.InventoryID = line.InventoryID;
									rLine.SubItemID = line.SubItemID;
									rLine.UOM = line.UOM;									
									rLine.Qty = line.OrderQty;
									rLine.SOType = line.OrderType;
									rLine.SONbr = docgraph.Document.Current.OrderNbr;
									rLine.SOLineNbr = line.LineNbr;
									rLine.SiteID = demand.ReplenishmentSourceSiteID;
									rLine.DestinationSiteID = demand.SiteID;
									rLine.PlanID = demand.PlanID;
									docgraph.ReplenishmentLines.Update(rLine);
									INItemPlan plan = PXSelect<INItemPlan,
										Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.SelectWindowed(docgraph, 0, 1,
										                                                                             demand.SupplyPlanID);
									if (plan != null)
									{									
										//plan.SupplyPlanID = rp.PlanID;
										rp.SupplyPlanID = plan.PlanID;
										cache.SetStatus(rp, PXEntryStatus.Updated);
									}

									docgraph.Save.Press();
								}
							}
							scope.Complete();
						}
						
						PXProcessing<SOFixedDemand>.SetInfo(list.IndexOf(demand), string.Format(Messages.TransferOrderCreated, docgraph.Document.Current.OrderNbr) + "\r\n" + ErrorText);						
						

						if (created.Find(docgraph.Document.Current) == null)
						{
							created.Add(docgraph.Document.Current);
						}
					}
				}
				catch (Exception e)
				{
					PXProcessing<SOFixedDemand>.SetError(list.IndexOf(demand), e);
				}
			}
		}

		public PXAction<SOCreateFilter> inventorySummary;
		[PXUIField(DisplayName = "Inventory Summary", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual System.Collections.IEnumerable InventorySummary(PXAdapter adapter)
		{
			PXCache tCache = FixedDemand.Cache;
			SOFixedDemand line = FixedDemand.Current;
			if (line == null) return adapter.Get();

			InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<SOFixedDemand.inventoryID>(tCache, line);
			if (item != null && item.StkItem == true)
			{
				INSubItem sbitem = (INSubItem)PXSelectorAttribute.Select<SOFixedDemand.subItemID>(tCache, line);
				InventorySummaryEnq.Redirect(item.InventoryID,
											 ((sbitem != null) ? sbitem.SubItemCD : null),
											 line.SiteID,
											 line.LocationID);
			}
			return adapter.Get();
		}

		[Serializable]
		public partial class SOCreateFilter : IBqlTable
		{
			#region CurrentOwnerID
			public abstract class currentOwnerID : PX.Data.IBqlField
			{
			}

			[PXDBGuid]
			[CR.CRCurrentOwnerID]
			public virtual Guid? CurrentOwnerID { get; set; }
			#endregion
			#region MyOwner
			public abstract class myOwner : PX.Data.IBqlField
			{
			}
			protected Boolean? _MyOwner;
			[PXDBBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Me")]
			public virtual Boolean? MyOwner
			{
				get
				{
					return _MyOwner;
				}
				set
				{
					_MyOwner = value;
				}
			}
			#endregion
			#region OwnerID
			public abstract class ownerID : PX.Data.IBqlField
			{
			}
			protected Guid? _OwnerID;
			[PXDBGuid]
			[PXUIField(DisplayName = "Product Manager")]
			[PX.TM.PXSubordinateOwnerSelector]
			public virtual Guid? OwnerID
			{
				get
				{
					return (_MyOwner == true) ? CurrentOwnerID : _OwnerID;
				}
				set
				{
					_OwnerID = value;
				}
			}
			#endregion
			#region WorkGroupID
			public abstract class workGroupID : PX.Data.IBqlField
			{
			}
			protected Int32? _WorkGroupID;
			[PXDBInt]
			[PXUIField(DisplayName = "Product  Workgroup")]
			[PXSelector(typeof(Search<EPCompanyTree.workGroupID,
				Where<EPCompanyTree.workGroupID, Owned<Current<AccessInfo.userID>>>>),
			 SubstituteKey = typeof(EPCompanyTree.description))]
			public virtual Int32? WorkGroupID
			{
				get
				{
					return (_MyWorkGroup == true) ? null : _WorkGroupID;
				}
				set
				{
					_WorkGroupID = value;
				}
			}
			#endregion
			#region MyWorkGroup
			public abstract class myWorkGroup : PX.Data.IBqlField
			{
			}
			protected Boolean? _MyWorkGroup;
			[PXDefault(false)]
			[PXDBBool]
			[PXUIField(DisplayName = "My", Visibility = PXUIVisibility.Visible)]
			public virtual Boolean? MyWorkGroup
			{
				get
				{
					return _MyWorkGroup;
				}
				set
				{
					_MyWorkGroup = value;
				}
			}
			#endregion
			#region FilterSet
			public abstract class filterSet : PX.Data.IBqlField
			{
			}
			[PXDefault(false)]
			[PXDBBool]
            public virtual Boolean? FilterSet
			{
				get
				{
					return
						this.OwnerID != null ||
						this.WorkGroupID != null ||
						this.MyWorkGroup == true;
				}
			}
			#endregion
			#region VendorID
			public abstract class vendorID : PX.Data.IBqlField
			{
			}
			protected Int32? _VendorID;
			[VendorNonEmployeeActive]
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
			#region SiteID
			public abstract class siteID : PX.Data.IBqlField
			{
			}
			protected Int32? _SiteID;
			[IN.Site(DisplayName = "Warehouse ID")]
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
			#region EndDate
			public abstract class endDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _EndDate;
			[PXDBDate]
			[PXUIField(DisplayName = "Date Promised")]
			[PXDefault(typeof(AccessInfo.businessDate))]
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
			#region PurchDate
			public abstract class purchDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _PurchDate;
			[PXDBDate]
			[PXUIField(DisplayName = "Creation Date")]
			[PXDefault(typeof(AccessInfo.businessDate))]
			public virtual DateTime? PurchDate
			{
				get
				{
					return this._PurchDate;
				}
				set
				{
					this._PurchDate = value;
				}
			}
			#endregion
			#region CustomerID
			public abstract class customerID : PX.Data.IBqlField
			{
			}
			protected Int32? _CustomerID;
			[Customer]
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
			[StockItem]
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
			#region ItemClassID
			public abstract class itemClassID : PX.Data.IBqlField
			{
			}
			protected String _ItemClassID;
			[PXDBString(10, IsUnicode = true)]
			[PXUIField(DisplayName = "Item Class ID", Visibility = PXUIVisibility.SelectorVisible)]
			[PXSelector(typeof(Search<INItemClass.itemClassID, Where<INItemClass.stkItem, Equal<boolTrue>>>), DescriptionField = typeof(INItemClass.descr))]
			public virtual String ItemClassID
			{
				get
				{
					return this._ItemClassID;
				}
				set
				{
					this._ItemClassID = value;
				}
			}
			#endregion
		}

		/// <summary>
		/// Returns records that are displayed in SOCreate screen. 
		/// Please refer to SOCreate screen documentation for details. 
		/// </summary>
		public class SOCreateProjectionAttribute : TM.OwnedFilter.ProjectionAttribute
		{
			public SOCreateProjectionAttribute()
				: base(typeof(SOCreateFilter),
				BqlCommand.Compose(
			typeof(Select2<,,>),
				typeof(INItemPlan),
				typeof(InnerJoin<INPlanType,
											On<INPlanType.planType, Equal<INItemPlan.planType>>,
				InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INItemPlan.inventoryID>>,
				InnerJoin<INUnit, 
				       On<INUnit.inventoryID, Equal<InventoryItem.inventoryID>, 
				      And<INUnit.fromUnit, Equal<InventoryItem.purchaseUnit>, 
				      And<INUnit.toUnit, Equal<InventoryItem.baseUnit>>>>,
				LeftJoin<IN.S.INItemSite, On<IN.S.INItemSite.inventoryID, Equal<INItemPlan.inventoryID>, And<IN.S.INItemSite.siteID, Equal<INItemPlan.siteID>>>>>>>),
			typeof(Where2<,>),
			typeof(Where<INItemPlan.hold, Equal<boolFalse>,
							And<INItemPlan.fixedSource, Equal<INReplenishmentSource.transfer>,	
							And<Where<INItemPlan.supplyPlanID, IsNull,
											Or<INItemPlan.planType, Equal<INPlanConstants.plan6B>,
											Or<INItemPlan.planType, Equal<INPlanConstants.plan6E>>>>>>>),
			typeof(And<>),
			TM.OwnedFilter.ProjectionAttribute.ComposeWhere(
			typeof(SOCreateFilter),
			typeof(InventoryItem.productWorkgroupID),
			typeof(InventoryItem.productManagerID))))
			{
			}
		}

		/*[Serializable]
		[PXProjection(typeof(Select2<INItemPlan,
			InnerJoin<INPlanType, On<INPlanType.planType, Equal<INItemPlan.planType>, And<INPlanType.isFixed, Equal<boolTrue>, And<INPlanType.isDemand, Equal<boolTrue>>>>,
			InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INItemPlan.inventoryID>>,
			InnerJoin<INUnit, On<INUnit.inventoryID, Equal<InventoryItem.inventoryID>, And<INUnit.fromUnit, Equal<InventoryItem.purchaseUnit>, And<INUnit.toUnit, Equal<InventoryItem.baseUnit>>>>>>>,
			Where<INItemPlan.supplyPlanID, IsNull, And<INItemPlan.hold, Equal<boolFalse>>>>))]*/
		[SOCreateProjectionAttribute]
        [Serializable]
		public partial class SOFixedDemand : INItemPlan
		{
			#region Selected
			public new abstract class selected : IBqlField
			{
			}
			#endregion
			#region InventoryID
			public new abstract class inventoryID : PX.Data.IBqlField
			{
			}
			#endregion
			#region SiteID
			public new abstract class siteID : PX.Data.IBqlField
			{
			}
			#endregion
			#region PlanDate
			public new abstract class planDate : PX.Data.IBqlField
			{
			}
			[PXDBDate]
			[PXDefault]
			[PXUIField(DisplayName = "Requested On")]
			public override DateTime? PlanDate
			{
				get
				{
					return this._PlanDate;
				}
				set
				{
					this._PlanDate = value;
				}
			}
			#endregion
			#region PlanID
			public new abstract class planID : PX.Data.IBqlField
			{
			}
			#endregion
			#region PlanType
			public new abstract class planType : PX.Data.IBqlField
			{
			}
			[PXDBString(2, IsFixed = true)]
			[PXDefault]
			[PXUIField(DisplayName = "Plan Type")]
			[PXSelector(typeof(Search<INPlanType.planType>), CacheGlobal = true, DescriptionField = typeof(INPlanType.descr))]
			public override String PlanType
			{
				get
				{
					return this._PlanType;
				}
				set
				{
					this._PlanType = value;
				}
			}
			#endregion
			#region ReplenishmentSourceSiteID
			public abstract class replenishmentSourceSiteID : PX.Data.IBqlField
			{
			}
			protected Int32? _ReplenishmentSourceSiteID;
			[IN.SiteAvail(typeof(SOFixedDemand.inventoryID), typeof(SOFixedDemand.subItemID),new Type[] { typeof(INSite.siteCD), typeof(INSiteStatus.qtyOnHand), typeof(INSite.descr), typeof(INSite.replenishmentClassID) }, DisplayName = "Source Warehouse", DescriptionField = typeof(INSite.descr), BqlField = typeof(INItemPlan.sourceSiteID))]
			public virtual Int32? ReplenishmentSourceSiteID
			{
				get
				{
					return this._ReplenishmentSourceSiteID;
				}
				set
				{
					this._ReplenishmentSourceSiteID = value;
				}
			}
			#endregion
			#region SubItemID
			public new abstract class subItemID : PX.Data.IBqlField
			{
			}
			#endregion
			#region LocationID
			public new abstract class locationID : PX.Data.IBqlField
			{
			}
			#endregion
			#region LotSerialNbr
			public new abstract class lotSerialNbr : PX.Data.IBqlField
			{
			}
			#endregion
			#region SupplyPlanID
			public new abstract class supplyPlanID : PX.Data.IBqlField
			{
			}
			#endregion
			#region PlanQty
			public new abstract class planQty : PX.Data.IBqlField
			{
			}
			[PXDBQuantity]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Requested Qty.")]
			public override Decimal? PlanQty
			{
				get
				{
					return this._PlanQty;
				}
				set
				{
					this._PlanQty = value;
				}
			}
			#endregion
			#region UOM
			public abstract class uOM : PX.Data.IBqlField
			{
			}
			protected String _UOM;
			[PXDBString(BqlField = typeof(INUnit.fromUnit))]
			[PXUIField(DisplayName = "UOM")]
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
			#region UnitMultDiv
			public abstract class unitMultDiv : PX.Data.IBqlField
			{
			}
			protected String _UnitMultDiv;
			[PXDBString(1, IsFixed = true, BqlField = typeof(INUnit.unitMultDiv))]
			public virtual String UnitMultDiv
			{
				get
				{
					return this._UnitMultDiv;
				}
				set
				{
					this._UnitMultDiv = value;
				}
			}
			#endregion
			#region UnitRate
			public abstract class unitRate : PX.Data.IBqlField
			{
			}
			protected Decimal? _UnitRate;
			[PXDBDecimal(6, BqlField = typeof(INUnit.unitRate))]
			public virtual Decimal? UnitRate
			{
				get
				{
					return this._UnitRate;
				}
				set
				{
					this._UnitRate = value;
				}
			}
			#endregion
			#region OrderQty
			public abstract class orderQty : PX.Data.IBqlField
			{
			}
			protected Decimal? _OrderQty;
			[PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>, Mult<INItemPlan.planQty, INUnit.unitRate>>, Div<INItemPlan.planQty, INUnit.unitRate>>), typeof(decimal))]
			[PXQuantity]
			[PXUIField(DisplayName = "Quantity")]
			public virtual Decimal? OrderQty
			{
				get
				{
					return this._OrderQty;
				}
				set
				{
					this._OrderQty = value;
				}
			}
			#endregion
			#region RefNoteID
			public new abstract class refNoteID : PX.Data.IBqlField
			{
			}
			[PXRefNote]
			[PXUIField(DisplayName = "Reference Nbr.")]
			public override Int64? RefNoteID
			{
				get
				{
					return this._RefNoteID;
				}
				set
				{
					this._RefNoteID = value;
				}
			}
			#endregion
			#region Hold
			public new abstract class hold : PX.Data.IBqlField
			{
			}
			#endregion
			#region VendorID_Vendor_acctName
			public abstract class vendorID_Vendor_acctName : PX.Data.IBqlField
			{
			}
			#endregion
			#region InventoryID_InventoryItem_descr
			public abstract class inventoryID_InventoryItem_descr : PX.Data.IBqlField
			{
			}
			#endregion
			#region SiteID_INSite_descr
			public abstract class siteID_INSite_descr : PX.Data.IBqlField
			{
			}
			#endregion		
		}
	}

}
