using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.SO
{
	public class SOOrderShipmentProcess : PXGraph<SOOrderShipmentProcess, SOOrderShipment>
	{
		public PXSelectJoin<SOOrderShipment,
			InnerJoin<SOOrder, 
				On<SOOrder.orderType, Equal<SOOrderShipment.orderType>, 
				And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>,
			InnerJoin<ARInvoice,
				On<ARInvoice.docType, Equal<SOOrderShipment.invoiceType>,
					And<ARInvoice.refNbr, Equal<SOOrderShipment.invoiceNbr>,
						And<ARInvoice.released, Equal<boolTrue>>>>>>,
			Where<SOOrderShipment.invoiceType, Equal<Current<ARInvoice.docType>>, 
				And<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>>>> Items;
		public PXSelect<SOAdjust,
				Where<SOAdjust.adjdOrderType, Equal<Required<SOAdjust.adjdOrderType>>,
					And<SOAdjust.adjdOrderNbr, Equal<Required<SOAdjust.adjdOrderNbr>>>>> Adjustments;


		public SOOrderShipmentProcess()
		{
			PXCache cache = this.Caches[typeof(SOOrder)];
			cache = this.Caches[typeof(SOShipment)];
			cache = this.Caches[typeof(SOOrderShipment)];
			cache = this.Caches[typeof(SOAdjust)];

			this.Views.Caches.Add(typeof(SOOrder));
			this.Views.Caches.Add(typeof(SOShipment));
			this.Views.Caches.Add(typeof(SOOrderShipment));
			this.Views.Caches.Add(typeof(SOAdjust));
		}

	}
}
