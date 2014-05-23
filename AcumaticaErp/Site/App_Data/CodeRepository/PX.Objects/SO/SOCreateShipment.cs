using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.AR;

namespace PX.Objects.SO
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class SOCreateShipment : PXGraph<SOCreateShipment>
	{
		public PXCancel<SOOrderFilter> Cancel;
		public PXFilter<SOOrderFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessing<SOOrder, SOOrderFilter> Orders;

		public PXAction<SOOrderFilter> viewDocument;
		[PXUIField(DisplayName = Messages.ViewDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (Orders.Current != null)
			{
				SOOrderEntry docgraph = PXGraph.CreateInstance<SOOrderEntry>();
				docgraph.Document.Current = docgraph.Document.Search<SOOrder.orderNbr>(Orders.Current.OrderNbr, Orders.Current.OrderType);
				throw new PXRedirectRequiredException(docgraph, true, "Order"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
			}
			return adapter.Get();
		}

		public SOCreateShipment()
		{
			Orders.SetSelected<SOOrder.selected>();
			PXUIFieldAttribute.SetDisplayName<Carrier.description>(Caches[typeof(Carrier)], Messages.CarrierDescr);
			PXUIFieldAttribute.SetDisplayName<INSite.descr>(Caches[typeof(INSite)], Messages.SiteDescr);
		}
		
		public virtual void SOOrderFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOOrderFilter filter = e.Row as SOOrderFilter;
            if (filter == null) return;
            string actionID = (string)Orders.GetTargetFill(null, null, null, filter.Action, "@actionID");
            PXUIFieldAttribute.SetVisible<SOOrderFilter.shipmentDate>(sender, null, actionID == "1");
            PXUIFieldAttribute.SetVisible<SOOrderFilter.siteID>(sender, null, actionID == "1");
            if (!String.IsNullOrEmpty(filter.Action))
			{
				string siteCD = Filter.GetValueExt<SOOrderFilter.siteID>(filter) as string;
                Orders.SetProcessTarget(null, null, null, filter.Action, null, actionID == "1" ? filter.ShipmentDate : filter.EndDate, siteCD);
			}
        }

		protected bool _ActionChanged = false;

		public virtual void SOOrderFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			_ActionChanged = !sender.ObjectsEqual<SOOrderFilter.action>(e.Row, e.OldRow);
		}

		public virtual IEnumerable orders()
		{
			PXUIFieldAttribute.SetDisplayName<SOOrder.customerID>(Caches[typeof(SOOrder)], Messages.CustomerID);

			List<SOOrder> ret = new List<SOOrder>();

			SOOrderFilter filter = PXCache<SOOrderFilter>.CreateCopy(Filter.Current);
			if (filter.Action == "<SELECT>")
			{
				return ret;
			}

			string actionID = (string)Orders.GetTargetFill(null, null, null, filter.Action, "@actionID");
			
			if (_ActionChanged)
			{
				Orders.Cache.Clear();
			}

			foreach (SOOrder order in Orders.Cache.Updated)
			{
				ret.Add(order);
			}

			PXSelectBase<SOOrder> cmd;

			switch (actionID)
			{
				case "1":
					cmd = new PXSelectJoinGroupBy<SOOrder,
						InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrder.orderType>>,
						LeftJoin<Carrier, On<SOOrder.shipVia, Equal<Carrier.carrierID>>,
						InnerJoin<SOShipmentPlan, On<SOOrder.orderType, Equal<SOShipmentPlan.orderType>, And<SOOrder.orderNbr, Equal<SOShipmentPlan.orderNbr>>>,
						LeftJoin<SOOrderShipment, On<SOOrderShipment.orderType, Equal<SOShipmentPlan.orderType>, And<SOOrderShipment.orderNbr, Equal<SOShipmentPlan.orderNbr>, And<SOOrderShipment.siteID, Equal<SOShipmentPlan.siteID>, And<SOOrderShipment.confirmed, Equal<boolFalse>>>>>>>>>,
						Where<SOShipmentPlan.inclQtySOBackOrdered, Equal<short0>, And<SOOrderShipment.shipmentNbr, IsNull>>, 
						Aggregate<
							GroupBy<SOOrder.orderType, 
							GroupBy<SOOrder.orderNbr>>>>(this);

					if (filter.DateSel == "S")
					{
						cmd.WhereAnd<Where<SOShipmentPlan.planDate, LessEqual<Current<SOOrderFilter.endDate>>>>();

						if (filter.StartDate != null)
						{
							cmd.WhereAnd<Where<SOShipmentPlan.planDate, GreaterEqual<Current<SOOrderFilter.startDate>>>>();
						}

						filter.DateSel = string.Empty;
					}
					break;
				case "3": 
					cmd = new PXSelectJoinGroupBy<SOOrder,
						InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrder.orderType>>,
						LeftJoin<Carrier, On<SOOrder.shipVia, Equal<Carrier.carrierID>>,
						LeftJoin<SOOrderShipment, On<SOOrderShipment.orderType, Equal<SOOrder.orderType>, And<SOOrderShipment.orderNbr, Equal<SOOrder.orderNbr>>>,
						LeftJoin<ARInvoice, On<ARInvoice.docType, Equal<SOOrderShipment.invoiceType>, And<ARInvoice.refNbr, Equal<SOOrderShipment.invoiceNbr>>>>>>>,
						Where<SOOrderType.aRDocType, NotEqual<ARDocType.noUpdate>,
						And2<Where<SOOrderType.requireShipping, Equal<boolFalse>, Or<SOOrder.shipmentCntr, Equal<short0>, And<SOOrder.openLineCntr, Equal<short0>>>>,
						And<ARInvoice.refNbr, IsNull>>>,
						Aggregate<
							GroupBy<SOOrder.orderType, 
							GroupBy<SOOrder.orderNbr>>>>(this);
					break;
				case "3.5":
					cmd = new PXSelectJoinGroupBy<SOOrder,
						InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrder.orderType>>,
						LeftJoin<Carrier, On<SOOrder.shipVia, Equal<Carrier.carrierID>>,
						InnerJoin<SOOrderShipment, On<SOOrderShipment.orderType, Equal<SOOrder.orderType>, And<SOOrderShipment.orderNbr, Equal<SOOrder.orderNbr>>>,
						LeftJoin<ARInvoice, On<ARInvoice.docType, Equal<SOOrderShipment.invoiceType>, And<ARInvoice.refNbr, Equal<SOOrderShipment.invoiceNbr>>>>>>>, 
						Where<SOOrderShipment.confirmed, Equal<boolTrue>,
						And<SOOrderType.aRDocType, NotEqual<ARDocType.noUpdate>,
						And<ARInvoice.refNbr, IsNull>>>,
						Aggregate<
							GroupBy<SOOrder.orderType, 
							GroupBy<SOOrder.orderNbr>>>>(this);
					break;
				default:
					if (!string.IsNullOrEmpty(filter.Action) && filter.Action.StartsWith("PrepareInvoice", StringComparison.OrdinalIgnoreCase))
					{
						cmd = new PXSelectJoinGroupBy<SOOrder,
									InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrder.orderType>, And<SOOrderType.aRDocType, NotEqual<ARDocType.noUpdate>>>,
									LeftJoin<Carrier, On<SOOrder.shipVia, Equal<Carrier.carrierID>>,
									LeftJoin<SOOrderShipment, On<SOOrderShipment.orderType, Equal<SOOrder.orderType>, And<SOOrderShipment.orderNbr, Equal<SOOrder.orderNbr>>>,
									LeftJoin<ARInvoice, On<ARInvoice.docType, Equal<SOOrderShipment.invoiceType>, And<ARInvoice.refNbr, Equal<SOOrderShipment.invoiceNbr>>>>>>>,
								Where<SOOrder.hold, Equal<boolFalse>, And<SOOrder.cancelled, Equal<boolFalse>,
									And<Where<Sub<Sub<Sub<SOOrder.shipmentCntr, 
																				SOOrder.openShipmentCntr>, 
																				SOOrder.billedCntr>, 
																				SOOrder.releasedCntr>, Greater<short0>,
									Or2<Where<SOOrder.orderQty, Equal<decimal0>, 
												And<SOOrder.curyUnbilledMiscTot, Greater<decimal0>>>,
									Or<Where<SOOrderType.requireShipping, Equal<boolFalse>, And<ARInvoice.refNbr, IsNull>>>>>>>>,
								Aggregate<
									GroupBy<SOOrder.orderType, 
									GroupBy<SOOrder.orderNbr>>>>(this);
					}
					else
					{
						cmd = new PXSelectJoin<SOOrder, InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrder.orderType>>,
							LeftJoin<Carrier, On<SOOrder.shipVia, Equal<Carrier.carrierID>>>>>(this);
					}
					break;
			}

			switch (filter.DateSel)
			{
				case "S":
					cmd.WhereAnd<Where<SOOrder.shipDate, LessEqual<Current<SOOrderFilter.endDate>>>>();
					break;
				case "C":
					cmd.WhereAnd<Where<SOOrder.cancelDate, LessEqual<Current<SOOrderFilter.endDate>>>>();
					break;
				case "O":
					cmd.WhereAnd<Where<SOOrder.orderDate, LessEqual<Current<SOOrderFilter.endDate>>>>();
					break;
			}

			if (filter.StartDate != null)
			{
				switch (filter.DateSel)
				{
					case "S":
						cmd.WhereAnd<Where<SOOrder.shipDate, GreaterEqual<Current<SOOrderFilter.startDate>>>>();
						break;
					case "C":
						cmd.WhereAnd<Where<SOOrder.cancelDate, GreaterEqual<Current<SOOrderFilter.startDate>>>>();
						break;
					case "O":
						cmd.WhereAnd<Where<SOOrder.orderDate, GreaterEqual<Current<SOOrderFilter.startDate>>>>();
						break;
				}
			}

			if (filter.SiteID != null)
			{
				switch (actionID)
				{ 
					case "1":
						cmd.WhereAnd<Where<SOShipmentPlan.siteID, Equal<Current<SOOrderFilter.siteID>>>>();
					break;
				}
			}

			if (!string.IsNullOrEmpty(filter.CarrierPluginID))
			{
				cmd.WhereAnd<Where<Carrier.carrierPluginID, Equal<Current<SOOrderFilter.carrierPluginID>>>>();
			}

			if (!string.IsNullOrEmpty(filter.ShipVia))
			{
				cmd.WhereAnd<Where<SOOrder.shipVia, Equal<Current<SOOrderFilter.shipVia>>>>();
			}

			if(filter.CustomerID != null)
			{
				cmd.WhereAnd<Where<SOOrder.customerID, Equal<Current<SOOrderFilter.customerID>>>>();
			}

			int startRow = PXView.StartRow;
			int totalRows = 0;

			List<PXFilterRow> newFilters = new List<PXFilterRow>();
			foreach(PXFilterRow f in PXView.Filters)
			{
				if (f.DataField.ToLower() == "behavior")
				{
					f.DataField = "SOOrderType__Behavior";
				}
				newFilters.Add(f);
			}

			foreach (PXResult<SOOrder, SOOrderType> res in cmd.View.Select(null, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, newFilters.ToArray(), ref startRow, PXView.MaximumRows, ref totalRows))
			{
				SOOrder order = res;
				SOOrder cached;

				order.Behavior = ((SOOrderType)res).Behavior;
				order.ARDocType = ((SOOrderType)res).ARDocType;
				order.DefaultOperation = ((SOOrderType)res).DefaultOperation;

				if ((cached = (SOOrder)Orders.Cache.Locate(order)) == null || Orders.Cache.GetStatus(cached) == PXEntryStatus.Notchanged)
				{
					ret.Add(order);
				}
			}

			PXView.StartRow = 0;

			Orders.Cache.IsDirty = false;

			return ret;
		}
	}

    [Serializable]
	public partial class SOOrderFilter : IBqlTable
	{
		#region Action
		public abstract class action : PX.Data.IBqlField
		{
		}
		protected string _Action;
		[PXAutomationMenu]
		public virtual string Action
		{
			get
			{
				return this._Action;
			}
			set
			{
				this._Action = value;
			}
		}
		#endregion
		#region DateSel
		public abstract class dateSel : PX.Data.IBqlField
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(new string[] { ShipDate, CancelBy, OrderDate },
						new string[] { Messages.ShipDate, Messages.CancelBy, Messages.OrderDate })
				{
				}
			}
			public const string ShipDate = "S";
			public const string CancelBy = "C";
			public const string OrderDate = "O";

			public class shipDate : Constant<string> { public shipDate() : base(ShipDate) { } }
			public class cancelBy : Constant<string> { public cancelBy() : base(CancelBy) { } }
			public class orderDate : Constant<string> { public orderDate() : base(OrderDate) { } }
		}
		protected string _DateSel;
		[PXDBString]
		[PXDefault(dateSel.ShipDate)]
		[PXUIField(DisplayName = "Select By")]
		[dateSel.List]
		public virtual string DateSel
		{
			get
			{
				return this._DateSel;
			}
			set
			{
				this._DateSel = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _StartDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
		[PXDefault()]
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
		[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region SiteID
		public abstract class siteID : PX.Data.IBqlField
		{
		}
		protected Int32? _SiteID;
		[IN.Site(DisplayName = "Warehouse", DescriptionField = typeof(INSite.descr))]
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
		#region CarrierPluginID
		public abstract class carrierPluginID : PX.Data.IBqlField
		{
		}
		protected String _CarrierPluginID;
		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Carrier", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CarrierPlugin.carrierPluginID>))]
		public virtual String CarrierPluginID
		{
			get
			{
				return this._CarrierPluginID;
			}
			set
			{
				this._CarrierPluginID = value;
			}
		}
		#endregion
		#region ShipVia
		public abstract class shipVia : PX.Data.IBqlField
		{
		}
		protected String _ShipVia;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Ship Via")]
		[PXSelector(typeof(Search<Carrier.carrierID>), DescriptionField = typeof(Carrier.description), CacheGlobal = true)]
		public virtual String ShipVia
		{
			get
			{
				return this._ShipVia;
			}
			set
			{
				this._ShipVia = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : IBqlField
		{
		}
		protected int? _CustomerID;
		[PXUIField(DisplayName = "Customer")]
		[Customer(DescriptionField = typeof(Customer.acctName))]
		public virtual int? CustomerID
		{
			get
			{
				return _CustomerID;
			}
			set
			{
				_CustomerID = value;
			}
		}
		#endregion
        #region ShipmentDate
        public abstract class shipmentDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _ShipmentDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Shipment Date", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(typeof(AccessInfo.businessDate))]
        public virtual DateTime? ShipmentDate
        {
            get
            {
                return this._ShipmentDate;
            }
            set
            {
                this._ShipmentDate = value;
            }
        }
        #endregion

    }
}
