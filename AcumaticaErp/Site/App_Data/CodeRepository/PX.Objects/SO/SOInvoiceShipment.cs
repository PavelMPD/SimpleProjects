namespace PX.Objects.SO
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;
	using PX.Data;
	using PX.Objects.CS;
	using PX.Objects.IN;
	using PX.Objects.AR;
	using PX.Objects.CM;
	using POReceipt = PX.Objects.PO.POReceipt;
	using POReceiptLine = PX.Objects.PO.POReceiptLine;
	using POLineType = PX.Objects.PO.POLineType;


	[Obsolete()]
	public class SOInvoiceOrder : PXGraph<SOInvoiceOrder>
	{
	}

	[PX.Objects.GL.TableAndChartDashboardType]
	public class SOInvoiceShipment : PXGraph<SOInvoiceShipment>
	{
		public PXCancel<SOShipmentFilter> Cancel;
		public PXFilter<SOShipmentFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessing<SOShipment, SOShipmentFilter> Orders;
		public PXSelect<SOShipLine> dummy_select_to_bind_events;
		public PXSetup<SOSetup> sosetup;

		public PXAction<SOShipmentFilter> viewDocument;
		[PXUIField(DisplayName = Messages.ViewDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (Orders.Current != null)
			{
				SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();
				docgraph.Document.Current = docgraph.Document.Search<SOShipment.shipmentNbr>(Orders.Current.ShipmentNbr);
				throw new PXRedirectRequiredException(docgraph, true, "Shipment") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		public SOInvoiceShipment()
		{
			Orders.SetSelected<SOShipment.selected>();
			PXUIFieldAttribute.SetDisplayName<Carrier.description>(Caches[typeof(Carrier)], Messages.CarrierDescr);
			PXUIFieldAttribute.SetDisplayName<INSite.descr>(Caches[typeof(INSite)], Messages.SiteDescr);

			object item = sosetup.Current;
		}

		public virtual void SOShipmentFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;

            SOShipmentFilter filter = e.Row as SOShipmentFilter;
			if (filter != null && !String.IsNullOrEmpty(filter.Action))
			{
				Dictionary<string, object> parameters = Filter.Cache.ToDictionary(filter);
				Orders.SetProcessTarget(null, null, null, filter.Action, parameters);
			}
			string actionID = (string)Orders.GetTargetFill(null, null, null, filter.Action, "@actionID");
			PXUIFieldAttribute.SetEnabled<SOShipmentFilter.invoiceDate>(sender, filter, actionID == "2" || actionID == "6");
			PXUIFieldAttribute.SetEnabled<SOShipmentFilter.packagingType>(sender, filter, actionID != "6");
			PXUIFieldAttribute.SetVisible<SOShipmentFilter.showPrinted>(sender, filter, actionID == "7");

			if (sosetup.Current.UseShipDateForInvoiceDate == true)
			{
				sender.RaiseExceptionHandling<SOShipmentFilter.invoiceDate>(filter, null, new PXSetPropertyException(Messages.UseInvoiceDateFromShipmentDateWarning, PXErrorLevel.Warning));
				PXUIFieldAttribute.SetEnabled<SOShipmentFilter.invoiceDate>(sender, filter, false);
			}
		}

		protected bool _ActionChanged = false;

		public virtual void SOShipmentFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			_ActionChanged = !sender.ObjectsEqual<SOShipmentFilter.action>(e.Row, e.OldRow);
			if (_ActionChanged)
				((SOShipmentFilter)e.Row).PackagingType = SOShipmentFilter.Both;
		}

		protected virtual void SOShipLine_LineAmt_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Option) == PXDBOperation.GroupBy)
			{
				e.FieldName = BqlCommand.Null;
				e.Cancel = true;
			}
		}

		protected virtual void SOShipLine_UnitPrice_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Option) == PXDBOperation.GroupBy)
			{
				e.FieldName = BqlCommand.Null;
				e.Cancel = true;
			}
		}

		protected virtual void SOShipLine_DiscPct_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Option) == PXDBOperation.GroupBy)
			{
				e.FieldName = BqlCommand.Null;
				e.Cancel = true;
			}
		}

		public virtual IEnumerable orders()
		{
			PXUIFieldAttribute.SetDisplayName<SOShipment.shipVia_Carrier_description>(Caches[typeof(SOShipment)], Messages.CarrierDescr);
			PXUIFieldAttribute.SetDisplayName<SOShipment.siteID_INSite_descr>(Caches[typeof(SOShipment)], Messages.SiteDescr);
			PXUIFieldAttribute.SetDisplayName<SOShipment.customerID>(Caches[typeof(SOShipment)], Messages.CustomerID);

			SOShipmentFilter filter = Filter.Current;
			if (filter.Action == "<SELECT>")
			{
				yield break;
			}

			string actionID = (string)Orders.GetTargetFill(null, null, null, filter.Action, "@actionID");

			if (_ActionChanged)
			{
				Orders.Cache.Clear();
			}

			foreach (SOShipment order in Orders.Cache.Updated)
			{
				yield return order;
			}

			PXSelectBase cmd;

			switch (actionID)
			{
				case "2":
					cmd = new PXSelectJoinGroupBy<SOShipment,
						LeftJoin<Carrier, On<SOShipment.shipVia, Equal<Carrier.carrierID>>,
						InnerJoin<SOShipLine, On<SOShipLine.shipmentNbr, Equal<SOShipment.shipmentNbr>>,
						InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOShipLine.origOrderType>, And<SOOrder.orderNbr, Equal<SOShipLine.origOrderNbr>>>,
						InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOShipLine.origOrderType>>,
						InnerJoin<SOOrderTypeOperation, 
							On<SOOrderTypeOperation.orderType, Equal<SOShipLine.origOrderType>,
							And<SOOrderTypeOperation.operation, Equal<SOShipment.operation>>>,
						InnerJoin<INSite, On<INSite.siteID, Equal<SOShipment.siteID>>,
						LeftJoin<ARTran, On<ARTran.sOShipmentNbr, Equal<SOShipLine.shipmentNbr>, And<ARTran.sOShipmentType, NotEqual<SOShipmentType.dropShip>, And<ARTran.sOShipmentLineNbr, Equal<SOShipLine.lineNbr>>>>>>>>>>>,
						Where<SOShipment.confirmed, Equal<boolTrue>,
						And<SOOrderType.aRDocType, NotEqual<ARDocType.noUpdate>,
						And<ARTran.refNbr, IsNull,
						And<Match<INSite, Current<AccessInfo.userName>>>>>>,
						Aggregate<GroupBy<SOShipment.shipmentNbr,
						GroupBy<SOShipment.createdByID,
						GroupBy<SOShipment.lastModifiedByID,
						GroupBy<SOShipment.confirmed,
						GroupBy<SOShipment.ownerID,
						GroupBy<SOShipment.released,
						GroupBy<SOShipment.hold,
						GroupBy<SOShipment.resedential,
						GroupBy<SOShipment.saturdayDelivery,
						GroupBy<SOShipment.groundCollect,
						GroupBy<SOShipment.insurance,
						GroupBy<SOShipment.shippedViaCarrier,
						GroupBy<SOShipment.labelsPrinted>>>>>>>>>>>>>>>(this);
					break;
				case "3":
					cmd = new PXSelectJoinGroupBy<SOShipment, 
					LeftJoin<Carrier, On<SOShipment.shipVia, Equal<Carrier.carrierID>>,
					InnerJoin<INSite, On<INSite.siteID, Equal<SOShipment.siteID>>,
					InnerJoin<SOOrderShipment, On<SOOrderShipment.shipmentType, Equal<SOShipment.shipmentType>, And<SOOrderShipment.shipmentNbr, Equal<SOShipment.shipmentNbr>>>,
					InnerJoin<SOShipLine, On<SOShipLine.shipmentType, Equal<SOOrderShipment.shipmentType>, And<SOShipLine.shipmentNbr, Equal<SOOrderShipment.shipmentNbr>, And<SOShipLine.origOrderType, Equal<SOOrderShipment.orderType>, And<SOShipLine.origOrderNbr, Equal<SOOrderShipment.orderNbr>>>>>,
					InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOShipLine.origOrderType>, And<SOOrder.orderNbr, Equal<SOShipLine.origOrderNbr>>>,
					InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrderShipment.orderType>>>>>>>>,
					Where<SOShipment.confirmed, Equal<boolTrue>, 
						And<SOOrderShipment.invtRefNbr, IsNull,
						And<SOShipLine.lineType, Equal<SOLineType.inventory>,
						And<SOOrderType.iNDocType, NotEqual<INTranType.noUpdate>,
						And<Match<INSite, Current<AccessInfo.userName>>>>>>>,
					Aggregate<GroupBy<SOShipment.shipmentNbr,
						GroupBy<SOShipment.createdByID,
						GroupBy<SOShipment.lastModifiedByID,
						GroupBy<SOShipment.confirmed,
						GroupBy<SOShipment.ownerID,
						GroupBy<SOShipment.released,
						GroupBy<SOShipment.hold,
						GroupBy<SOShipment.resedential,
						GroupBy<SOShipment.saturdayDelivery,
						GroupBy<SOShipment.groundCollect,
						GroupBy<SOShipment.saturdayDelivery,
						GroupBy<SOShipment.shippedViaCarrier,
						GroupBy<SOShipment.labelsPrinted>>>>>>>>>>>>>>>(this);
					break;
				case "6":
					cmd = new PXSelectJoinGroupBy<POReceipt,
					CrossJoin<SOOrder,
					InnerJoin<POReceiptLine, On<POReceiptLine.receiptNbr, Equal<POReceipt.receiptNbr>>,
					InnerJoin<SOLine, On<SOLine.pOType, Equal<POReceiptLine.pOType>, And<SOLine.pONbr, Equal<POReceiptLine.pONbr>, And<SOLine.pOLineNbr, Equal<POReceiptLine.pOLineNbr>, And<SOLine.orderType, Equal<SOOrder.orderType>, And<SOLine.orderNbr, Equal<SOOrder.orderNbr>>>>>>,
					LeftJoin<ARTran, On<ARTran.sOShipmentNbr, Equal<POReceiptLine.receiptNbr>, And<ARTran.sOShipmentType, Equal<SOShipmentType.dropShip>, And<ARTran.sOShipmentLineNbr, Equal<POReceiptLine.lineNbr>, And<ARTran.sOOrderType, Equal<SOLine.orderType>, And<ARTran.sOOrderNbr, Equal<SOLine.orderNbr>, And<ARTran.sOOrderLineNbr, Equal<SOLine.lineNbr>>>>>>>,
					InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOLine.orderType>>,
					InnerJoin<SOOrderTypeOperation, 
							On<SOOrderTypeOperation.orderType, Equal<SOOrder.orderType>,
							And<SOOrderTypeOperation.operation, Equal<SOLine.operation>>>>>>>>>,
					Where<POReceipt.released, Equal<boolTrue>,
						And2<Where<POReceiptLine.lineType, Equal<POLineType.goodsForDropShip>, Or<POReceiptLine.lineType, Equal<POLineType.nonStockForDropShip>>>,
						And<SOOrderType.aRDocType, NotEqual<ARDocType.noUpdate>,
						And<ARTran.refNbr, IsNull>>>>,
					Aggregate<GroupBy<POReceipt.receiptNbr,
						GroupBy<POReceipt.createdByID,
						GroupBy<POReceipt.lastModifiedByID,
						GroupBy<POReceipt.released,
						GroupBy<POReceipt.ownerID,
						GroupBy<POReceipt.hold>>>>>>>>(this);
					break;
				case "9":
					cmd = new PXSelectJoinGroupBy<SOShipment, InnerJoin<INSite, On<INSite.siteID, Equal<SOShipment.siteID>>,
						LeftJoin<Carrier, On<SOShipment.shipVia, Equal<Carrier.carrierID>>,
						InnerJoin<SOOrderShipment, On<SOOrderShipment.shipmentType, Equal<SOShipment.shipmentType>, And<SOOrderShipment.shipmentNbr, Equal<SOShipment.shipmentNbr>>>,
						InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOOrderShipment.orderType>, And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>>>>>,
					Where<SOShipment.labelsPrinted, Equal<False>, And<Match<INSite, Current<AccessInfo.userName>>>>,
					Aggregate<GroupBy<SOShipment.shipmentNbr,
						GroupBy<SOShipment.createdByID,
						GroupBy<SOShipment.lastModifiedByID,
						GroupBy<SOShipment.confirmed,
						GroupBy<SOShipment.ownerID,
						GroupBy<SOShipment.released,
						GroupBy<SOShipment.hold,
						GroupBy<SOShipment.resedential,
						GroupBy<SOShipment.saturdayDelivery,
						GroupBy<SOShipment.groundCollect,
						GroupBy<SOShipment.saturdayDelivery,
						GroupBy<SOShipment.shippedViaCarrier,
						GroupBy<SOShipment.labelsPrinted>>>>>>>>>>>>>>>(this);
					break;
				default:
					cmd = new PXSelectJoinGroupBy<SOShipment, 
						LeftJoin<Carrier, On<SOShipment.shipVia, Equal<Carrier.carrierID>>,
						InnerJoin<INSite, On<INSite.siteID, Equal<SOShipment.siteID>>,
						InnerJoin<SOOrderShipment, On<SOOrderShipment.shipmentType, Equal<SOShipment.shipmentType>, And<SOOrderShipment.shipmentNbr, Equal<SOShipment.shipmentNbr>>>,
						InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOOrderShipment.orderType>, And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>>>>>,
					Where<Match<INSite, Current<AccessInfo.userName>>>,
					Aggregate<GroupBy<SOShipment.shipmentNbr,
						GroupBy<SOShipment.createdByID,
						GroupBy<SOShipment.lastModifiedByID,
						GroupBy<SOShipment.confirmed,
						GroupBy<SOShipment.ownerID,
						GroupBy<SOShipment.released,
						GroupBy<SOShipment.hold,
						GroupBy<SOShipment.resedential,
						GroupBy<SOShipment.saturdayDelivery,
						GroupBy<SOShipment.groundCollect,
						GroupBy<SOShipment.saturdayDelivery,
						GroupBy<SOShipment.shippedViaCarrier,
						GroupBy<SOShipment.labelsPrinted>>>>>>>>>>>>>>>(this);
					break;
			}

			if (typeof(PXSelectBase<SOShipment>).IsAssignableFrom(cmd.GetType()))
			{
				((PXSelectBase<SOShipment>)cmd).WhereAnd<Where<SOShipment.shipDate, LessEqual<Current<SOShipmentFilter.endDate>>>>();

				if (filter.SiteID != null)
				{
					((PXSelectBase<SOShipment>)cmd).WhereAnd<Where<SOShipment.siteID, Equal<Current<SOShipmentFilter.siteID>>>>();
				}

				if (!string.IsNullOrEmpty(filter.CarrierPluginID))
				{
					((PXSelectBase<SOShipment>)cmd).WhereAnd<Where<Carrier.carrierPluginID, Equal<Current<SOShipmentFilter.carrierPluginID>>>>();
				}

				if (!string.IsNullOrEmpty(filter.ShipVia))
				{
					((PXSelectBase<SOShipment>)cmd).WhereAnd<Where<SOShipment.shipVia, Equal<Current<SOShipmentFilter.shipVia>>>>();
				}

				if (filter.StartDate != null)
				{
					((PXSelectBase<SOShipment>)cmd).WhereAnd<Where<SOShipment.shipDate, GreaterEqual<Current<SOShipmentFilter.startDate>>>>();
				}

				if (filter.CustomerID != null)
				{
					((PXSelectBase<SOShipment>)cmd).WhereAnd<Where<SOShipment.customerID, Equal<Current<SOShipmentFilter.customerID>>>>();
				}

				if ( actionID == "7" && filter.ShowPrinted == false)
				{
					((PXSelectBase<SOShipment>)cmd).WhereAnd<Where<SOShipment.labelsPrinted, Equal<False>>>();
				}
								
				if (filter.PackagingType == SOShipmentFilter.Manual)
				{
					((PXSelectBase<SOShipment>)cmd).WhereAnd<Where<SOOrder.isManualPackage, Equal<True>>>();
				}
				else if (filter.PackagingType == SOShipmentFilter.Auto)
				{
					((PXSelectBase<SOShipment>)cmd).WhereAnd<Where<SOOrder.isManualPackage, NotEqual<True>>>();
				}


				int startRow = PXView.StartRow;
				int totalRows = 0;

				foreach (object res in ((PXSelectBase<SOShipment>)cmd).View.Select(null, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
				{
					SOShipment order = PXResult.Unwrap<SOShipment>(res);
					SOOrder so = PXResult.Unwrap<SOOrder>(res);

					if (order.BilledOrderCntr + order.UnbilledOrderCntr + order.ReleasedOrderCntr == 1)
					{
						order.CustomerOrderNbr = so.CustomerOrderNbr;
					}

					object item;
					if ((item = (SOShipment)Orders.Cache.Locate(order)) == null || Orders.Cache.GetStatus(item) == PXEntryStatus.Notchanged)
					{
						yield return order;
					}
				}
				PXView.StartRow = 0;
			}

			if (typeof(PXSelectBase<POReceipt>).IsAssignableFrom(cmd.GetType()))
			{
				((PXSelectBase<POReceipt>)cmd).WhereAnd<Where<POReceipt.receiptDate, LessEqual<Current<SOShipmentFilter.endDate>>>>();

				/*
				if (filter.SiteID != null)
				{
					((PXSelectBase<POReceipt>)cmd).WhereAnd<Where<POReceipt.siteID, Equal<Current<SOShipmentFilter.siteID>>>>();
				}

				if (!string.IsNullOrEmpty(filter.ShipVia))
				{
					((PXSelectBase<POReceipt>)cmd).WhereAnd<Where<POReceipt.shipVia, Equal<Current<SOShipmentFilter.shipVia>>>>();
				}
				*/

				if (filter.StartDate != null)
				{
					((PXSelectBase<POReceipt>)cmd).WhereAnd<Where<POReceipt.receiptDate, GreaterEqual<Current<SOShipmentFilter.startDate>>>>();
				}

				foreach (PXResult<POReceipt, SOOrder> res in ((PXSelectBase<POReceipt>)cmd).Select())
				{
					SOShipment order = res;

					if ((order = (SOShipment)Orders.Cache.Locate(order)) == null)
					{
						Orders.Cache.SetStatus(order = res, PXEntryStatus.Held);
						yield return order;
					}
					else if (Orders.Cache.GetStatus(order) == PXEntryStatus.Notchanged || Orders.Cache.GetStatus(order) == PXEntryStatus.Held)
					{
						yield return order;
					}
				}
			}
			Orders.Cache.IsDirty = false;
		}
	}

    [Serializable]
	public partial class SOShipmentFilter : IBqlTable
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
		#region InvoiceDate
		public abstract class invoiceDate : IBqlField
		{
		}
		protected DateTime? _InvoiceDate;
		[PXDBDate]
		[PXUIField(DisplayName = "Invoice Date")]
		[PXDefault(typeof(AccessInfo.businessDate))]
		public virtual DateTime? InvoiceDate
		{
			get
			{
				return _InvoiceDate;
			}
			set
			{
				_InvoiceDate = value;
			}
		}
		#endregion
		#region PackagingType
		public abstract class packagingType : PX.Data.IBqlField
		{
		}

		public const string Auto = "A";
		public const string Manual = "M";
		public const string Both = "B";
		protected String _PackagingType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault("B")]
		[PXStringList(
				new string[] { Both, Auto, Manual},
				new string[] { Messages.PackagingType_Both, Messages.PackagingType_Auto, Messages.PackagingType_Manual })]
		[PXUIField(DisplayName = "Packaging Type")]
		public virtual String PackagingType
		{
			get
			{
				return this._PackagingType;
			}
			set
			{
				this._PackagingType = value;
			}
		}
		#endregion
		#region ShowPrinted
		public abstract class showPrinted : PX.Data.IBqlField
		{
		}
		protected bool? _ShowPrinted;
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Show Printed")]
		public virtual bool? ShowPrinted
		{
			get
			{
				return this._ShowPrinted;
			}
			set
			{
				this._ShowPrinted = value;
			}
		}
		#endregion

	}
}
