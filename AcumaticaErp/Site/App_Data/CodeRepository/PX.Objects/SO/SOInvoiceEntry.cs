using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.TX;
using PX.Objects.IN;
using PX.Objects.AR;
using PX.Objects.EP;
using POReceipt = PX.Objects.PO.POReceipt;
using POReceiptLine = PX.Objects.PO.POReceiptLine;
using POLineType = PX.Objects.PO.POLineType;
using Avalara.AvaTax.Adapter;
using Avalara.AvaTax.Adapter.TaxService;
using AvaAddress = Avalara.AvaTax.Adapter.AddressService;
using AvaMessage = Avalara.AvaTax.Adapter.Message;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;
using LocationStatus = PX.Objects.IN.Overrides.INDocumentRelease.LocationStatus;
using LotSerialStatus = PX.Objects.IN.Overrides.INDocumentRelease.LotSerialStatus;
using ItemLotSerial = PX.Objects.IN.Overrides.INDocumentRelease.ItemLotSerial;

namespace PX.Objects.SO
{
	public class SOInvoiceEntry : ARInvoiceEntry
	{		
		public PXAction<ARInvoice> addShipment;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddShipment(PXAdapter adapter)
		{
			bool RequireControlTotal = ARSetup.Current.RequireControlTotal == true;

			foreach (SOOrderShipment shipment in shipmentlist.Cache.Updated)
			{
				if (shipment.Selected == true)
				{
					foreach (PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact, SOOrderType, SOOrderTypeOperation> order in 
						PXSelectJoin<SOOrderShipment,
						InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOOrderShipment.orderType>, And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>,
						InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<SOOrder.curyInfoID>>,
						InnerJoin<SOAddress, On<SOAddress.addressID, Equal<SOOrder.billAddressID>>,
						InnerJoin<SOContact, On<SOContact.contactID, Equal<SOOrder.billContactID>>,
						InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrder.orderType>>,
						InnerJoin<SOOrderTypeOperation, 
					         On<SOOrderTypeOperation.orderType, Equal<SOOrder.orderType>,
									And<SOOrderTypeOperation.operation, Equal<SOOrderShipment.operation>>>>>>>>>,
					Where<SOOrderShipment.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>, And<SOOrderShipment.shipmentType, Equal<Current<SOOrderShipment.shipmentType>>, And<SOOrderShipment.orderType, Equal<Current<SOOrderShipment.orderType>>, And<SOOrderShipment.orderNbr, Equal<Current<SOOrderShipment.orderNbr>>>>>>>.SelectMultiBound(this, new object[] { shipment }))
					{
						ARSetup.Current.RequireControlTotal = false;
						this.InvoiceOrder((DateTime)this.Accessinfo.BusinessDate, order, customer.Current, null);
						ARSetup.Current.RequireControlTotal = RequireControlTotal;
						continue;
					}

					foreach (PXResult<SOOrderReceipt, SOOrder, CurrencyInfo, SOAddress, SOContact, SOOrderType, SOOrderTypeOperation, Customer> res in 
						PXSelectJoin<SOOrderReceipt,
						InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOOrderReceipt.orderType>, And<SOOrder.orderNbr, Equal<SOOrderReceipt.orderNbr>>>,
						InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<SOOrder.curyInfoID>>,
						InnerJoin<SOAddress, On<SOAddress.addressID, Equal<SOOrder.billAddressID>>,
						InnerJoin<SOContact, On<SOContact.contactID, Equal<SOOrder.billContactID>>,
						InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrder.orderType>>,						
						InnerJoin<SOOrderTypeOperation, 
					         On<SOOrderTypeOperation.orderType, Equal<SOOrder.orderType>,
									And<SOOrderTypeOperation.operation, Equal<Current<SOOrderShipment.operation>>>>,
						InnerJoin<Customer, On<Customer.bAccountID, Equal<SOOrder.customerID>>>>>>>>>,
					Where<SOOrderReceipt.receiptNbr, Equal<Current<SOOrderShipment.shipmentNbr>>, And<SOShipmentType.dropShip, Equal<Current<SOOrderShipment.shipmentType>>, And<SOOrderReceipt.orderType, Equal<Current<SOOrderShipment.orderType>>, And<SOOrderReceipt.orderNbr, Equal<Current<SOOrderShipment.orderNbr>>>>>>>.SelectMultiBound(this, new object[] { shipment }))
					{
						SOOrderReceipt receipt = res;
						SOOrder order = res;

						PXResultset<SOShipLine, SOLine> details = new PXResultset<SOShipLine, SOLine>();

						foreach (PXResult<POReceiptLine, SOLine> line in PXSelectJoin<POReceiptLine,
							InnerJoin<SOLine, On<SOLine.pOType, Equal<POReceiptLine.pOType>, And<SOLine.pONbr, Equal<POReceiptLine.pONbr>, And<SOLine.pOLineNbr, Equal<POReceiptLine.pOLineNbr>>>>>,
							Where2<Where<POReceiptLine.lineType, Equal<POLineType.goodsForDropShip>, Or<POReceiptLine.lineType, Equal<POLineType.nonStockForDropShip>>>, And<POReceiptLine.receiptNbr, Equal<Current<SOOrderReceipt.receiptNbr>>, And<SOLine.orderType, Equal<Current<SOOrderReceipt.orderType>>, And<SOLine.orderNbr, Equal<Current<SOOrderReceipt.orderNbr>>>>>>>.SelectMultiBound(this, new object[] { receipt }))
						{
							details.Add(new PXResult<SOShipLine, SOLine>((SOShipLine)line, line));
						}

						shipmentlist.Cache.SetStatus(shipment, PXEntryStatus.Notchanged);
						shipmentlist.Cache.Remove(shipment);

						ARSetup.Current.RequireControlTotal = false;
						this.InvoiceOrder((DateTime)this.Accessinfo.BusinessDate, new PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact, SOOrderType, SOOrderTypeOperation>((PXResult<SOOrderReceipt, SOOrder>)res, order, (CurrencyInfo)res, (SOAddress)res, (SOContact)res, (SOOrderType)res, (SOOrderTypeOperation)res), details, (Customer)res, null);
						ARSetup.Current.RequireControlTotal = RequireControlTotal;
					}
				}
				else if (shipment.InvoiceNbr == null)
				{
					shipmentlist.Cache.SetStatus(shipment, PXEntryStatus.Notchanged);
					shipmentlist.Cache.Remove(shipment);
				}
			}

			shipmentlist.View.Clear();
			//shipmentlist.Cache.Clear(); 
			return adapter.Get();
		}

		public PXAction<ARInvoice> addShipmentCancel;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddShipmentCancel(PXAdapter adapter)
		{
			foreach (SOOrderShipment shipment in shipmentlist.Cache.Updated)
			{
				if (shipment.InvoiceNbr == null)
				{
					shipmentlist.Cache.SetStatus(shipment, PXEntryStatus.Notchanged);
					shipmentlist.Cache.Remove(shipment);
				}
			}

			shipmentlist.View.Clear();
			//shipmentlist.Cache.Clear();
			return adapter.Get();
		}
		public PXSelect<ARTran, Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>, And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>, And<ARTran.lineType, Equal<SOLineType.freight>>>>, OrderBy<Asc<ARTran.tranType, Asc<ARTran.refNbr, Asc<ARTran.lineNbr>>>>> Freight;
		public PXSelect<ARTran, Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>, And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>, And<ARTran.lineType, Equal<SOLineType.discount>>>>, OrderBy<Asc<ARTran.tranType, Asc<ARTran.refNbr, Asc<ARTran.lineNbr>>>>> Discount;
		public PXSelect<SOInvoice, Where<SOInvoice.docType, Equal<Optional<ARInvoice.docType>>, And<SOInvoice.refNbr, Equal<Optional<ARInvoice.refNbr>>>>> SODocument;
		public PXSelectOrderBy<SOOrderShipment, OrderBy<Asc<SOOrderShipment.orderType, Asc<SOOrderShipment.orderNbr, Asc<SOOrderShipment.shipmentNbr, Asc<SOOrderShipment.shipmentType>>>>>> shipmentlist;
		public PXSelect<SOShipment> shipments;
		public PXSelect<SOInvoiceDiscountDetail, Where<SOInvoiceDiscountDetail.tranType, Equal<Current<ARInvoice.docType>>, And<SOInvoiceDiscountDetail.refNbr, Equal<Current<ARInvoice.refNbr>>>>> DiscountDetails;
		public PXSelect<SOFreightDetail, Where<SOFreightDetail.docType, Equal<Current<ARInvoice.docType>>, And<SOFreightDetail.refNbr, Equal<Current<ARInvoice.refNbr>>>>> FreightDetails;
		public PXSelectReadonly<CCProcTran, Where<CCProcTran.refNbr, Equal<Current<SOInvoice.refNbr>>, 
												And<CCProcTran.docType, Equal<Current<SOInvoice.docType>>>>,
												OrderBy<Desc<CCProcTran.tranNbr>>> ccProcTran;

		public PXSelect<SOAdjust> soadjustments;

		public PXSetup<SOOrderType, Where<SOOrderType.orderType, Equal<Optional<SOOrder.orderType>>>> soordertype;

		#region Cache Attached
		#region ARTran
		[PXDBString(2, IsFixed = true)]
		[SOLineType.List()]
		[PXUIField(DisplayName = "Line Type", Visible = false, Enabled = false)]
		[PXDefault(SOLineType.NonInventory)]
		protected virtual void ARTran_LineType_CacheAttached(PXCache sender)
		{ 
		}

		[NonStockItem(Filterable = true)]
		protected virtual void ARTran_InventoryID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category")]
		[SOInvoiceTax()]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
        [PXDefault(typeof(Search<InventoryItem.taxCategoryID,
			Where<InventoryItem.inventoryID, Equal<Current<ARTran.inventoryID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing, SearchOnDefault = false)]
		protected override void ARTran_TaxCategoryID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region ARInvoice
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[ARDocType.SOEntryList()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		protected virtual void ARInvoice_DocType_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[ARInvoiceType.RefNbr(typeof(Search2<ARInvoice.refNbr,
					InnerJoin<SOInvoice, On<SOInvoice.docType, Equal<ARInvoice.docType>, And<SOInvoice.refNbr, Equal<ARInvoice.refNbr>>>,
					InnerJoin<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>>>,
					Where<ARInvoice.docType, Equal<Optional<ARInvoice.docType>>,
					And<Match<Customer, Current<AccessInfo.userName>>>>, OrderBy<Desc<ARInvoice.refNbr>>>), Filterable = true)]
		[ARInvoiceType.Numbering()]
		[ARInvoiceNbr()]
		protected virtual void ARInvoice_RefNbr_CacheAttached(PXCache sender)
		{
		}
		[ARCustomerCredit(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), Filterable = true)]
		[PXDefault()]
		protected virtual void ARInvoice_CustomerID_CacheAttached(PXCache sender)
		{
		}
		[SOOpenPeriod(typeof(ARRegister.docDate))]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void ARInvoice_FinPeriodID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<Customer.termsID, Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>, And<Current<ARInvoice.docType>, NotEqual<ARDocType.creditMemo>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, Or<Terms.visibleTo, Equal<TermsVisibleTo.customer>>>>), DescriptionField = typeof(Terms.descr), Filterable = true)]
		[SOInvoiceTerms()]
		protected virtual void ARInvoice_TermsID_CacheAttached(PXCache sender)
		{
		}
		[PXDBDate()]
		[PXUIField(DisplayName = "Due Date", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void ARInvoice_DueDate_CacheAttached(PXCache sender)
		{
		}
		[PXDBDate()]
		[PXUIField(DisplayName = "Cash Discount Date", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void ARInvoice_DiscDate_CacheAttached(PXCache sender)
		{
		}
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.origDocAmt))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void ARInvoice_CuryOrigDocAmt_CacheAttached(PXCache sender)
		{
		}
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.docBal), BaseCalc = false)]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		protected virtual void ARInvoice_CuryDocBal_CacheAttached(PXCache sender)
		{
		}
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.origDiscAmt))]
		[PXUIField(DisplayName = "Cash Discount", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void ARInvoice_CuryOrigDiscAmt_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region SOAdjust
		[PXDBInt()]
		[PXDefault()]
		protected virtual void SOAdjust_CustomerID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault()]
		protected virtual void SOAdjust_AdjdOrderType_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		protected virtual void SOAdjust_AdjdOrderNbr_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
		[PXDefault()]
		protected virtual void SOAdjust_AdjgDocType_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		protected virtual void SOAdjust_AdjgRefNbr_CacheAttached(PXCache sender)
		{
		}
		[PXDBCurrency(typeof(SOAdjust.adjdCuryInfoID), typeof(SOAdjust.adjAmt))]
		[PXFormula(typeof(Sub<SOAdjust.curyOrigAdjdAmt, SOAdjust.curyAdjdBilledAmt>))]
		[PXUIField(DisplayName = "Applied To Order")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		protected virtual void SOAdjust_CuryAdjdAmt_CacheAttached(PXCache sender)
		{
		}
		[PXDBDecimal(4)]
		[PXFormula(typeof(Sub<SOAdjust.origAdjAmt, SOAdjust.adjBilledAmt>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		protected virtual void SOAdjust_AdjAmt_CacheAttached(PXCache sender)
		{
		}
		[PXDBDecimal(4)]
		[PXFormula(typeof(Sub<SOAdjust.curyOrigAdjgAmt, SOAdjust.curyAdjgBilledAmt>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		protected virtual void SOAdjust_CuryAdjgAmt_CacheAttached(PXCache sender)
		{
		}
		[PXDBLong()]
		[PXDefault()]
		[CurrencyInfo(ModuleCode = "SO", CuryIDField = "AdjdOrigCuryID")]
		protected virtual void SOAdjust_AdjdOrigCuryInfoID_CacheAttached(PXCache sender)
		{
		}
		[PXDBLong()]
		[PXDefault()]
		[CurrencyInfo(ModuleCode = "SO", CuryIDField = "AdjgCuryID")]
		protected virtual void SOAdjust_AdjgCuryInfoID_CacheAttached(PXCache sender)
		{
		}
		[PXDBLong()]
		[PXDefault()]
		[CurrencyInfo(ModuleCode = "SO", CuryIDField = "AdjdCuryID")]
		protected virtual void SOAdjust_AdjdCuryInfoID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion

		public virtual IEnumerable ccproctran() 
		{

			Dictionary<int, CCProcTran> existsingTran = new Dictionary<int, CCProcTran>();	
			foreach (CCProcTran iTran in PXSelectReadonly<CCProcTran, 
				Where<CCProcTran.refNbr, Equal<Current<ARInvoice.refNbr>>,
					And<CCProcTran.docType, Equal<Current<ARInvoice.docType>>>>,
				OrderBy<Desc<CCProcTran.tranNbr>>>.SelectMultiBound(this, PXView.Currents)) 
			{
				if (existsingTran.ContainsKey(iTran.TranNbr.Value)) continue;
				existsingTran[iTran.TranNbr.Value] = iTran;
				yield return iTran;
			}
			
			foreach (CCProcTran iTran1 in PXSelectReadonly2<CCProcTran, 
					InnerJoin<SOOrderShipment, On<SOOrderShipment.orderNbr, Equal<CCProcTran.origRefNbr>, 
						And<SOOrderShipment.orderType, Equal<CCProcTran.origDocType>>>>, 
					Where<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>, 
						And<SOOrderShipment.invoiceType, Equal<Current<ARInvoice.docType>>, 
						And<CCProcTran.refNbr, IsNull>>>,
					OrderBy<Desc<CCProcTran.tranNbr>>>.SelectMultiBound(this, PXView.Currents))
			{
				if (existsingTran.ContainsKey(iTran1.TranNbr.Value)) continue;
				existsingTran[iTran1.TranNbr.Value] = iTran1;
				yield return iTran1;
			}
		}
		public PXSelectReadonly<CCProcTran, Where<CCProcTran.tranNbr, Equal<Current<SOInvoice.cCAuthTranNbr>>>> ccLastTran; 

		public PXSetup<SOSetup> sosetup;
        public PXSetup<ARSetup> arsetup;
		public PXSetup<Company> Company;

		public PXSelect<SOLine2> soline;
		public PXSelect<SOMiscLine2> somiscline;
		public PXSelect<SOTax> sotax;
		public PXSelect<SOTaxTran> sotaxtran;
		public PXSelect<SOOrder> soorder;

		public PXAction<ARInvoice> hold;
		[PXUIField(DisplayName = "Hold")]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		protected virtual IEnumerable Hold(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<ARInvoice> creditHold;
		[PXUIField(DisplayName = "Credit Hold")]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		protected virtual IEnumerable CreditHold(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<ARInvoice> flow;
		[PXUIField(DisplayName = "Flow")]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		protected virtual IEnumerable Flow(PXAdapter adapter)
		{
			Save.Press();					
			return adapter.Get();
		}

		[PXUIField(DisplayName = "Release", Visible = false)]
		[PXButton()]
		public override IEnumerable Release(PXAdapter adapter)
		{
			List<ARRegister> list = new List<ARRegister>();
			foreach (ARInvoice order in adapter.Get<ARInvoice>())
			{
				list.Add(order);
			}

			if (!IsExternalTax)
			{
				Save.Press();
			}
			else
			{
				try
				{
					skipAvalaraCallOnSave = true;
					Save.Press();
				}
				finally
				{
					skipAvalaraCallOnSave = false;
				}
			}

			PXLongOperation.StartOperation(this, delegate()
			{
				List<ARRegister> listWithTax = new List<ARRegister>();
				if (!IsExternalTax)
				{
					foreach (ARInvoice ardoc in list)
					{
						if (ardoc.IsTaxValid != true && AvalaraMaint.IsExternalTax(this, ardoc.TaxZoneID))
						{
							ARInvoice doc = new ARInvoice();
							doc.DocType = ardoc.DocType;
							doc.RefNbr = ardoc.RefNbr;
							doc.OrigModule = ardoc.OrigModule;
							doc.ApplyPaymentWhenTaxAvailable = ardoc.ApplyPaymentWhenTaxAvailable;
							listWithTax.Add(ARExternalTaxCalc.Process(doc));
						}
						else
						{
							listWithTax.Add(ardoc);
						}
					}
				}
				else
				{
					listWithTax.AddRange(list);
				}

				SOInvoiceEntry ie = PXGraph.CreateInstance<SOInvoiceEntry>();				
				INIssueEntry ingraph = PXGraph.CreateInstance<INIssueEntry>();
				SOOrderShipmentProcess docgraph = PXGraph.CreateInstance<SOOrderShipmentProcess>();
				HashSet<object> processed = new HashSet<object>();
						
				//Field Verification can fail if IN module is not "Visible";therfore suppress it:
				ingraph.FieldVerifying.AddHandler<INTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
				ingraph.FieldVerifying.AddHandler<INTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
				

				ARDocumentRelease.ReleaseDoc(listWithTax, adapter.MassProcess, null, delegate(ARRegister ardoc, bool isAborted)
				{
					List<object> items = new List<object>();
					items.Add(ardoc);
					PXAutomation.RemovePersisted(ie, typeof(ARInvoice), items);

					docgraph.Clear();
					foreach (PXResult<SOOrderShipment, SOOrder> ordershipment in docgraph.Items.View.SelectMultiBound(new object[] { ardoc }))
					{
						SOOrderShipment copy = PXCache<SOOrderShipment>.CreateCopy(ordershipment);
						SOOrder order = ordershipment;
						copy.InvoiceReleased = true;
						docgraph.Items.Update(copy);

						if (order.Completed == true && order.BilledCntr <= 1 && order.ShipmentCntr <= order.BilledCntr + order.ReleasedCntr)
						{
							foreach (SOAdjust adj in docgraph.Adjustments.Select(order.OrderType, order.OrderNbr))
							{
								SOAdjust adjcopy = PXCache<SOAdjust>.CreateCopy(adj);
								adjcopy.CuryAdjdAmt = 0m;
								adjcopy.CuryAdjgAmt = 0m;
								adjcopy.AdjAmt = 0m;
								docgraph.Adjustments.Update(adjcopy);
							}
						}
						processed.Add(ardoc);
					}
					docgraph.Actions.PressSave();
				});
				PXAutomation.StorePersisted(ie, typeof(ARInvoice), new List<object>(processed));
				PXAutomation.CompleteAction(docgraph);				
			});
			return list;
		}

		public PXAction<ARInvoice> post;
		[PXUIField(DisplayName = "Post", Visible = false)]
		[PXButton()]
		protected virtual IEnumerable Post(PXAdapter adapter)
		{
			List<ARRegister> list = new List<ARRegister>();
			foreach (ARInvoice order in adapter.Get<ARInvoice>())
			{
				list.Add(order);
			}

			Save.Press();

			PXLongOperation.StartOperation(this, delegate()
			{
				SOInvoiceEntry ie = PXGraph.CreateInstance<SOInvoiceEntry>();
				INIssueEntry ingraph = PXGraph.CreateInstance<INIssueEntry>();
				ingraph.FieldVerifying.AddHandler<INTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
				ingraph.FieldVerifying.AddHandler<INTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
				ingraph.FieldVerifying.AddHandler<INTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
				DocumentList<INRegister> inlist = new DocumentList<INRegister>(ingraph);

				bool failed = false;

				foreach (ARInvoice ardoc in list)
				{
					try
					{
						ie.PostInvoice(ingraph, ardoc, inlist);

						if (adapter.MassProcess)
						{
							PXProcessing<ARInvoice>.SetInfo(list.IndexOf(ardoc), ActionsMessages.RecordProcessed);
						}
					}
					catch (Exception ex)
					{
						if (!adapter.MassProcess)
						{
							throw;
						}
						PXProcessing<ARInvoice>.SetError(list.IndexOf(ardoc), ex);
						failed = true;
					} 
				}

				if (ie.sosetup.Current.AutoReleaseIN == true && inlist.Count > 0 && inlist[0].Hold == false)
				{
					INDocumentRelease.ReleaseDoc(inlist, false);
				}

				if (failed)
				{
					throw new PXOperationCompletedException(ErrorMessages.SeveralItemsFailed);
				}
			});

			return adapter.Get();
		}

		//throw new PXReportRequiredException(parameters, "SO642000", "Shipment Confirmation");
		[PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		protected override IEnumerable Report(PXAdapter adapter,
			[PXString(8, InputMask = "CC.CC.CC.CC")]
			string reportID
			)
		{
			if (!String.IsNullOrEmpty(reportID))
			{
				Save.Press();
				int i = 0;
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				string actualRecoprtID = null;
				foreach (ARInvoice doc in adapter.Get<ARInvoice>())
				{
					parameters["ARInvoice.DocType" + i.ToString()] = doc.DocType;
					parameters["ARInvoice.RefNbr" + i.ToString()] = doc.RefNbr;
					actualRecoprtID = new NotificationUtility(this).SearchReport(ARNotificationSource.Customer, customer.Current, reportID, doc.BranchID);					
					i++;
				}
				if (i > 0)
				{
					throw new PXReportRequiredException(parameters, actualRecoprtID, "Report " + actualRecoprtID);
				}
			}
			return adapter.Get();
		}

		
		#region Credit Card Processing Buttons
		public PXAction<ARInvoice> captureCCPayment;
		[PXUIField(DisplayName = "Capture CC Payment", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable CaptureCCPayment(PXAdapter adapter)
		{
			List<ARRegister> list = new List<ARRegister>();
			foreach (ARInvoice item in adapter.Get<ARInvoice>())
			{
				list.Add(item);
				SOInvoice ext = (SOInvoice) SODocument.View.SelectSingleBound(new object[] { item });
				CCPaymentEntry.ReleaseDelegate releaseDelegate = null;
				CCPaymentEntry.UpdateDocStateDelegate docStateDelegate = UpdateSOInvoiceState;
				CCPaymentEntry.CaptureCCPayment<SOInvoice>(ext, this.ccProcTran, releaseDelegate, docStateDelegate);
			}

			return list;
		}

		public PXAction<ARInvoice> authorizeCCPayment;
		[PXUIField(DisplayName = "Authorize CC Payment", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable AuthorizeCCPayment(PXAdapter adapter)
		{
			List<ARRegister> list = new List<ARRegister>();
			foreach (ARInvoice item in adapter.Get<ARInvoice>())
			{
				list.Add(item);
				SOInvoice ext = (SOInvoice)SODocument.View.SelectSingleBound(new object[] { item });
				CCPaymentEntry.AuthorizeCCPayment<SOInvoice>(ext, ccProcTran, UpdateSOInvoiceState);
			}
			return list;
		}

		public PXAction<ARInvoice> voidCCPayment;
		[PXUIField(DisplayName = "Void CC Payment", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable VoidCCPayment(PXAdapter adapter)
		{
			List<ARRegister> list = new List<ARRegister>();
			foreach (ARInvoice item in adapter.Get<ARInvoice>())
			{
				list.Add(item);
				SOInvoice ext = (SOInvoice)SODocument.View.SelectSingleBound(new object[] { item });
				CCPaymentEntry.VoidCCPayment<SOInvoice>(ext, ccProcTran, null, UpdateSOInvoiceState);
			}
			return list;
		}

		public PXAction<ARInvoice> creditCCPayment;
		[PXUIField(DisplayName = "Refund CC Payment", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable CreditCCPayment(PXAdapter adapter)
		{
			foreach (ARInvoice doc in adapter.Get<ARInvoice>())
			{
				SOInvoice ext = (SOInvoice)SODocument.View.SelectSingleBound(new object[] { doc });
				string PCRefTranNbr = ext.RefTranExtNbr;
				if (String.IsNullOrEmpty(ext.RefTranExtNbr))
				{
					this.SODocument.Cache.RaiseExceptionHandling<SOOrder.refTranExtNbr>(ext, ext.RefTranExtNbr, new PXSetPropertyException(AR.Messages.ERR_PCTransactionNumberOfTheOriginalPaymentIsRequired));
				}
				else
				{
					CCPaymentEntry.CreditCCPayment<SOInvoice>(ext, PCRefTranNbr, ccProcTran, null, UpdateSOInvoiceState);
				}
			}
			return adapter.Get();
		}

		#endregion


		public SOInvoiceEntry()
			: base()
		{
			{
				SOSetup record = sosetup.Current;
			}

			Document.View = new PXView(this, false, new Select2<ARInvoice,
			LeftJoin<SOInvoice, On<SOInvoice.docType, Equal<ARInvoice.docType>, And<SOInvoice.refNbr, Equal<ARInvoice.refNbr>>>,
			LeftJoin<ARChildInvoice, On<ARChildInvoice.docType, Equal<ARInvoice.docType>, And<ARChildInvoice.refNbr, Equal<ARInvoice.refNbr>>>,
			LeftJoin<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>>>>,
			Where<ARInvoice.docType, Equal<Optional<ARInvoice.docType>>,
			And2<Where<ARChildInvoice.refNbr, IsNull, And<SOInvoice.refNbr, IsNull, Or<ARChildInvoice.refNbr, IsNotNull, And<SOInvoice.refNbr, IsNotNull>>>>,
			And<Where<Customer.bAccountID, IsNull,
			Or<Match<Customer, Current<AccessInfo.userName>>>>>>>>());

			this.Views["Document"] = Document.View;

			PXUIFieldAttribute.SetVisible<SOOrderShipment.orderType>(shipmentlist.Cache, null, true);
			PXUIFieldAttribute.SetVisible<SOOrderShipment.orderNbr>(shipmentlist.Cache, null, true);
			PXUIFieldAttribute.SetVisible<SOOrderShipment.shipmentNbr>(shipmentlist.Cache, null, true);

			PXDBLiteDefaultAttribute.SetDefaultForInsert<SOOrderShipment.invoiceNbr>(shipmentlist.Cache, null, true);
			PXDBLiteDefaultAttribute.SetDefaultForUpdate<SOOrderShipment.invoiceNbr>(shipmentlist.Cache, null, true);

			TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(Transactions.Cache, null, TaxCalc.ManualLineCalc);
			this.ccLastTran.Cache.AllowInsert = false;
			this.ccLastTran.Cache.AllowUpdate = false;
			this.ccLastTran.Cache.AllowDelete = false;
		}

		public override void Persist()
		{
			CopyFreightNotesAndFilesToARTran();

			foreach (ARTran tran in Transactions.Cache.Deleted)
			{
				ARTran siblings = PXSelect<ARTran, Where<ARTran.sOOrderType, Equal<Required<ARTran.sOOrderType>>,
					And<ARTran.sOOrderNbr, Equal<Required<ARTran.sOOrderNbr>>,
					And<ARTran.sOShipmentType, Equal<Required<ARTran.sOShipmentType>>,
					And<ARTran.sOShipmentNbr, Equal<Required<ARTran.sOShipmentNbr>>,
					And<ARTran.tranType, Equal<Required<ARTran.tranType>>,
					And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>>>>>>>>.SelectWindowed(this, 0, 1,
					tran.SOOrderType, tran.SOOrderNbr, tran.SOShipmentType, tran.SOShipmentNbr, tran.TranType, tran.RefNbr);

				if (siblings != null && siblings.LineType != SOLineType.Freight)
				{
					throw new PXException(Messages.PartialInvoice);
				}
			}

			foreach (ARInvoice invoice in Document.Cache.Deleted)
			{
				foreach (SOInvoice ext in SODocument.Cache.Deleted)
				{
					if (string.Equals(ext.DocType, invoice.DocType) && string.Equals(ext.RefNbr, invoice.RefNbr) && 
						(invoice.IsCCPayment == true ||ext.IsCCPayment == true) && ccProcTran.View.SelectMultiBound( new object[] { invoice, ext }).Count > 0)
					{
						ARPaymentEntry docgraph = PXGraph.CreateInstance<ARPaymentEntry>();
						docgraph.AutoPaymentApp = true;
						docgraph.arsetup.Current.HoldEntry = false;
						docgraph.arsetup.Current.RequireControlTotal = false;

						ARPayment payment = new ARPayment()
						{
							DocType = ARDocType.Payment,
							AdjDate = ext.AdjDate,
							AdjFinPeriodID = ext.AdjFinPeriodID
						};

						payment = PXCache<ARPayment>.CreateCopy(docgraph.Document.Insert(payment));
						payment.CustomerID = invoice.CustomerID;
						payment.CustomerLocationID = invoice.CustomerLocationID;
						payment.ARAccountID = invoice.ARAccountID;
						payment.ARSubID = invoice.ARSubID;

                        payment.PaymentMethodID = ext.PaymentMethodID;
                        payment.PMInstanceID = ext.PMInstanceID;
						payment.CashAccountID = ext.CashAccountID;
						payment.ExtRefNbr = ext.ExtRefNbr ?? string.Format(AR.Messages.ARAutoPaymentRefNbrFormat, payment.DocDate);
						payment.CuryOrigDocAmt = ext.CuryPaymentAmt;

						docgraph.Document.Update(payment);

						using (PXTransactionScope ts = new PXTransactionScope())
						{
							docgraph.Save.Press();

							PXDatabase.Update<CCProcTran>(
								new PXDataFieldAssign("DocType", docgraph.Document.Current.DocType),
								new PXDataFieldAssign("RefNbr", docgraph.Document.Current.RefNbr),
								new PXDataFieldRestrict("DocType", PXDbType.Char, 3, ext.DocType, PXComp.EQ),
								new PXDataFieldRestrict("RefNbr", PXDbType.NVarChar, 15, ext.RefNbr, PXComp.EQ)
								);

							int i = 0;
							bool ccproctranupdated = false;

							foreach (SOOrderShipment order in PXSelect<SOOrderShipment,
								Where<SOOrderShipment.invoiceType, Equal<Required<SOOrderShipment.invoiceType>>,
								And<SOOrderShipment.invoiceNbr, Equal<Required<SOOrderShipment.invoiceNbr>>>>>.Select(docgraph, ext.DocType, ext.RefNbr))
							{
								ccproctranupdated |= PXDatabase.Update<CCProcTran>(
									new PXDataFieldAssign("DocType", docgraph.Document.Current.DocType),
									new PXDataFieldAssign("RefNbr", docgraph.Document.Current.RefNbr),
									new PXDataFieldRestrict("OrigDocType", PXDbType.Char, 3, order.OrderType, PXComp.EQ),
									new PXDataFieldRestrict("OrigRefNbr", PXDbType.NVarChar, 15, order.OrderNbr, PXComp.EQ),
									new PXDataFieldRestrict("RefNbr", PXDbType.NVarChar, 15, null, PXComp.ISNULL)
									);

								if (ccproctranupdated && i > 0)
								{
									throw new PXException(AR.Messages.ERR_CCMultiplyPreauthCombined);
								}

								i++;
							}

							docgraph.ccProcTran.View.Clear();
							docgraph.Document.Cache.RaiseRowSelected(docgraph.Document.Current);

							PXFieldState voidState;
							if ((voidState = (PXFieldState)docgraph.voidCheck.GetState(Document.Current)) == null || voidState.Enabled == false)
							{
								throw new PXException(AR.Messages.ERR_CCTransactionMustBeVoided);
							}

							List<object> tovoid = new List<object>();
							tovoid.Add(docgraph.Document.Current);

							foreach(object item in docgraph.voidCheck.Press(new PXAdapter(new DummyView(docgraph, docgraph.Document.View.BqlSelect, tovoid)))) { ; }

							base.Persist();

							ts.Complete();
						}

						return;
					}
				}
			}

			foreach (ARAdjust adj in Adjustments.Cache.Inserted)
			{
				if (adj.CuryAdjdAmt == 0m)
				{
					Adjustments.Cache.SetStatus(adj, PXEntryStatus.InsertedDeleted);
				}
			}

			foreach (ARAdjust adj in Adjustments.Cache.Updated)
			{
				if (adj.CuryAdjdAmt == 0m)
				{
					Adjustments.Cache.SetStatus(adj, PXEntryStatus.Deleted);
				}
			}

			foreach (ARInvoice ardoc in Document.Cache.Cached)
			{
				if ((Document.Cache.GetStatus(ardoc) == PXEntryStatus.Inserted || Document.Cache.GetStatus(ardoc) == PXEntryStatus.Updated) && ardoc.DocType == ARDocType.Invoice && ardoc.Released == false && ardoc.ApplyPaymentWhenTaxAvailable != true)
				{
					SOInvoice ext = SODocument.Select(ardoc.DocType, ardoc.RefNbr);

					if (ardoc.CuryDocBal - ardoc.CuryOrigDiscAmt - ext.CuryPaymentTotal - ext.CuryCCCapturedAmt < 0m)
					{
						foreach (PXResult<ARAdjust> res in Adjustments_Raw.View.SelectMultiBound(new object[] { ardoc }))
						{
							ARAdjust adj = res;
							if (Adjustments.Cache.GetStatus(adj) == PXEntryStatus.Notchanged)
							{
								Adjustments.Cache.SetStatus(adj, PXEntryStatus.Updated);
							}
							Adjustments.Cache.RaiseExceptionHandling<ARAdjust.curyAdjdAmt>(adj, adj.CuryAdjdAmt, new PXSetPropertyException(AR.Messages.Application_Amount_Cannot_Exceed_Document_Amount));
							throw new PXException(AR.Messages.Application_Amount_Cannot_Exceed_Document_Amount);
						}
					}
				}
			}

			base.Persist();
		}

		protected override void RecalcUnbilledTax()
		{
			Dictionary<string, KeyValuePair<string, string>> orders =
																new Dictionary<string, KeyValuePair<string, string>>();

			foreach (ARTran line in Transactions.Select())
			{
				string key = string.Format("{0}.{1}", line.SOOrderType, line.SOOrderNbr);
				if (!orders.ContainsKey(key))
				{
					orders.Add(key, new KeyValuePair<string, string>(line.SOOrderType, line.SOOrderNbr));
				}

			}


			SOOrderEntry soOrderEntry = PXGraph.CreateInstance<SOOrderEntry>();
			soOrderEntry.RowSelecting.RemoveHandler<SOOrder>(soOrderEntry.SOOrder_RowSelecting);
			foreach (KeyValuePair<string, string> kv in orders.Values)
			{
				soOrderEntry.Clear(PXClearOption.ClearAll);
				soOrderEntry.Document.Current = soOrderEntry.Document.Search<SOOrder.orderNbr>(kv.Value, kv.Key);
				soOrderEntry.CalculateAvalaraTax(soOrderEntry.Document.Current);
				soOrderEntry.Persist();
			}
		}

		protected virtual void SOLine2_BaseShippedQty_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				e.FieldName = string.Empty;
				e.Cancel = true;
			}
		}

		protected virtual void SOLine2_ShippedQty_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				e.FieldName = string.Empty;
				e.Cancel = true;
			}
		}

		protected override void ARInvoice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				PXResult<SOOrderShipment, SOOrderType> sotype =
					(PXResult<SOOrderShipment, SOOrderType>)
					PXSelectJoin<SOOrderShipment, 
					InnerJoin<SOOrderType, 
								 On<SOOrderType.orderType, Equal<SOOrderShipment.orderType>>>, 
					Where<SOOrderShipment.invoiceType, Equal<Current<ARInvoice.docType>>, 
						And<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>>>>
						.SelectSingleBound(this, new object[] { e.Row });

				if (sotype != null)
				{
					if (string.IsNullOrEmpty(((ARInvoice)e.Row).RefNbr) && ((SOOrderType)sotype).UserInvoiceNumbering == true)
					{
						throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<SOOrder.invoiceNbr>(soorder.Cache));
					}

					if (((SOOrderType)sotype).MarkInvoicePrinted == true)
					{
						((ARInvoice)e.Row).Printed = true;
					}

					if (((SOOrderType)sotype).MarkInvoiceEmailed == true)
					{
						((ARInvoice)e.Row).Emailed = true;
					}

					AutoNumberAttribute.SetNumberingId<ARInvoice.refNbr>(Document.Cache, ((SOOrderType)sotype).ARDocType, ((SOOrderType)sotype).InvoiceNumberingID);
				}
			}

			base.ARInvoice_RowPersisting(sender, e);
		}

		protected virtual void ARInvoice_OrigModule_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = GL.BatchModule.SO;
			e.Cancel = true;
		}

		protected override void ARInvoice_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			base.ARInvoice_RowInserted(sender, e);

			SODocument.Cache.Insert();
			SODocument.Cache.IsDirty = false;

			SODocument.Current.AdjDate = ((ARInvoice)e.Row).DocDate;
			SODocument.Current.AdjFinPeriodID = ((ARInvoice)e.Row).FinPeriodID;
			SODocument.Current.AdjTranPeriodID = ((ARInvoice)e.Row).TranPeriodID;
			
		}

		protected override void ARInvoice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARSetup.Current.RequireControlTotal = (((ARInvoice)e.Row).DocType == ARDocType.CashSale || ((ARInvoice)e.Row).DocType == ARDocType.CashReturn) ? true : ARSetup.Current.RequireControlTotal;

			base.ARInvoice_RowUpdated(sender, e);

			ARInvoice doc = e.Row as ARInvoice;
			if ((doc.DocType == ARDocType.CashSale || doc.DocType == ARDocType.CashReturn) && doc.Released != true)
			{
				if (sender.ObjectsEqual<ARInvoice.curyDocBal, ARInvoice.curyOrigDiscAmt>(e.Row, e.OldRow) == false && doc.CuryDocBal - doc.CuryOrigDiscAmt != doc.CuryOrigDocAmt)
				{
					if (doc.CuryDocBal != null && doc.CuryOrigDiscAmt != null && doc.CuryDocBal != 0)
						sender.SetValueExt<ARInvoice.curyOrigDocAmt>(doc, doc.CuryDocBal - doc.CuryOrigDiscAmt);
					else
						sender.SetValueExt<ARInvoice.curyOrigDocAmt>(doc, 0m);
				}
				else if (sender.ObjectsEqual<ARInvoice.curyOrigDocAmt>(e.Row, e.OldRow) == false)
				{
					if (doc.CuryDocBal != null && doc.CuryOrigDocAmt != null && doc.CuryDocBal != 0)
						sender.SetValueExt<ARInvoice.curyOrigDiscAmt>(doc, doc.CuryDocBal - doc.CuryOrigDocAmt);
					else
						sender.SetValueExt<ARInvoice.curyOrigDiscAmt>(doc, 0m);
				}
			}

			if ((doc.DocType == ARDocType.CashSale || doc.DocType == ARDocType.CashReturn) && doc.Released != true && doc.Hold != true)
			{
				if (doc.CuryDocBal < doc.CuryOrigDocAmt)
				{
					sender.RaiseExceptionHandling<ARInvoice.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, new PXSetPropertyException(AR.Messages.CashSaleOutOfBalance));
				}
				else
				{
					sender.RaiseExceptionHandling<ARInvoice.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, null);
				}
			}

			if (!sender.ObjectsEqual<ARInvoice.customerID, ARInvoice.docDate, ARInvoice.finPeriodID, ARInvoice.curyTaxTotal, ARInvoice.curyOrigDocAmt, ARInvoice.docDesc, ARInvoice.curyOrigDiscAmt>(e.Row, e.OldRow))
			{
				SODocument.Current = (SOInvoice)SODocument.Select() ?? (SOInvoice)SODocument.Cache.Insert();
				SODocument.Current.CustomerID = ((ARInvoice)e.Row).CustomerID;

                if ((((ARInvoice)e.Row).DocType == ARDocType.CashSale 
                        || ((ARInvoice)e.Row).DocType == ARDocType.CashReturn 
                        || ((ARInvoice)e.Row).DocType == ARDocType.Invoice) && !sender.ObjectsEqual<ARInvoice.customerID>(e.Row, e.OldRow))
				{
                    SODocument.Cache.SetDefaultExt<SOInvoice.paymentMethodID>(SODocument.Current);
					SODocument.Cache.SetDefaultExt<SOInvoice.pMInstanceID>(SODocument.Current);
				}

				SODocument.Current.AdjDate = ((ARInvoice)e.Row).DocDate;
				SODocument.Current.AdjFinPeriodID = ((ARInvoice)e.Row).FinPeriodID;
				SODocument.Current.AdjTranPeriodID = ((ARInvoice)e.Row).TranPeriodID;
				SODocument.Current.CuryTaxTotal = ((ARInvoice)e.Row).CuryTaxTotal;
				SODocument.Current.TaxTotal = ((ARInvoice)e.Row).TaxTotal;
				SODocument.Current.CuryPaymentAmt = ((ARInvoice)e.Row).CuryOrigDocAmt - ((ARInvoice)e.Row).CuryOrigDiscAmt - SODocument.Current.CuryPaymentTotal;
				SODocument.Current.DocDesc = ((ARInvoice)e.Row).DocDesc;
				SODocument.Current.PaymentProjectID = PM.ProjectDefaultAttribute.NonProject(this);

				if (SODocument.Cache.GetStatus(SODocument.Current) == PXEntryStatus.Notchanged)
				{
					SODocument.Cache.SetStatus(SODocument.Current, PXEntryStatus.Updated);
				}
			}
		}

		protected override void ARInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			base.ARInvoice_RowSelected(cache, e);

			PXUIFieldAttribute.SetVisible<ARInvoice.projectID>(cache, e.Row, PM.ProjectAttribute.IsPMVisible(this, BatchModule.SO) || PM.ProjectAttribute.IsPMVisible(this, BatchModule.AR) || PXAccess.FeatureInstalled<FeaturesSet.contractManagement>());
			PXUIFieldAttribute.SetVisible<ARTran.taskID>(Transactions.Cache, null, PM.ProjectAttribute.IsPMVisible(this, BatchModule.SO) || PM.ProjectAttribute.IsPMVisible(this, BatchModule.AR));

			if (e.Row != null)
			{
				ARInvoice doc = e.Row as ARInvoice;
				if (((ARInvoice)e.Row).DocType == ARDocType.CashSale || ((ARInvoice)e.Row).DocType == ARDocType.CashReturn)
				{
					PXUIFieldAttribute.SetVisible<ARInvoice.curyOrigDocAmt>(cache, e.Row);
					PXUIFieldAttribute.SetEnabled<ARInvoice.curyOrigDocAmt>(cache, e.Row, ((ARInvoice)e.Row).Released == false);
				}

				SODocument.Cache.AllowUpdate = Document.Cache.AllowUpdate;
				FreightDetails.Cache.AllowUpdate = Document.Cache.AllowUpdate;

				
				CCProcTran lastCCran;
				CCPaymentState ccPaymentState = CCPaymentEntry.ResolveCCPaymentState(ccProcTran.Select(), out lastCCran);

				bool isCCCaptured = (ccPaymentState & CCPaymentState.Captured) != 0;
				bool isCCPreAuthorized = (ccPaymentState & CCPaymentState.PreAuthorized) != 0;
				
				bool isAuthorizedCashSale = (doc.DocType == ARDocType.CashSale && (isCCPreAuthorized || isCCCaptured));
				bool isRefundedCashReturn = doc.DocType == ARDocType.CashReturn && (ccPaymentState & CCPaymentState.Refunded) != 0;
				Transactions.Cache.AllowDelete = !isAuthorizedCashSale && !isRefundedCashReturn;
				Transactions.Cache.AllowUpdate = !isAuthorizedCashSale && !isRefundedCashReturn;
				Transactions.Cache.AllowInsert = !isAuthorizedCashSale && !isRefundedCashReturn;
				PXUIFieldAttribute.SetEnabled<ARInvoice.curyOrigDocAmt>(cache, doc, !isAuthorizedCashSale && !isRefundedCashReturn);
				PXUIFieldAttribute.SetEnabled<ARInvoice.curyOrigDiscAmt>(cache, doc, !isAuthorizedCashSale && !isRefundedCashReturn);


				release.SetEnabled(arsetup.Current.IntegratedCCProcessing != true || doc.DocType != ARDocType.CashReturn || (ccPaymentState & CCPaymentState.Refunded) != 0);
			}
		}

		protected virtual void ARInvoice_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			ARInvoice row = (ARInvoice)e.Row;
			
			if (row != null && e.IsReadOnly==false
				&& String.IsNullOrEmpty(row.DocType) == false && String.IsNullOrEmpty(row.RefNbr) == false)
			{
				row.IsCCPayment = false;
				using (new PXConnectionScope())
				{
					PXResult<CustomerPaymentMethodC, CA.PaymentMethod, SOInvoice> pmInstance = (PXResult<CustomerPaymentMethodC, CA.PaymentMethod, SOInvoice>)
										 PXSelectJoin<CustomerPaymentMethodC,
										InnerJoin<CA.PaymentMethod,
											On<CA.PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethodC.paymentMethodID>>,
																			InnerJoin<SOInvoice, On<SOInvoice.pMInstanceID, Equal<CustomerPaymentMethodC.pMInstanceID>>>>,
									Where<SOInvoice.docType, Equal<Required<SOInvoice.docType>>,
										And<SOInvoice.refNbr, Equal<Required<SOInvoice.refNbr>>,
										And<CA.PaymentMethod.paymentType, Equal<CA.PaymentMethodType.creditCard>,
											And<CA.PaymentMethod.aRIsProcessingRequired, Equal<True>>>>>>.Select(this, row.DocType, row.RefNbr);
					if (pmInstance != null)
					{
						row.IsCCPayment = true;
					}
				}
			}
		}


		protected override void ARInvoice_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			string CreditRule = customer.Current.CreditRule;
			try
			{
				base.ARInvoice_CustomerID_FieldUpdated(sender, e);
			}
			finally
			{
				customer.Current.CreditRule = CreditRule;
			}
		}

		protected virtual void SOInvoice_RowSelecting(PXCache sender, PXRowSelectingEventArgs e) 
		{
			if (e.Row != null && e.IsReadOnly == false && ((SOInvoice)e.Row).CuryPaymentTotal == null)
			{
				using (new PXConnectionScope())
				{
					bool IsReadOnly = (sender.GetStatus(e.Row) == PXEntryStatus.Notchanged);
					PXFormulaAttribute.CalcAggregate<ARAdjust.curyAdjdAmt>(Adjustments.Cache, e.Row, IsReadOnly);
					sender.RaiseFieldUpdated<SOInvoice.curyPaymentTotal>(e.Row, null);

					PXDBCurrencyAttribute.CalcBaseValues<SOInvoice.curyPaymentTotal>(sender, e.Row);
				}
			}
		}

		protected virtual void SOInvoice_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;

            SOInvoice doc = (SOInvoice)e.Row;
			ARInvoice arDoc = this.Document.Current;

			bool docNotOnHold = (arDoc!= null) && (arDoc.OpenDoc == true && arDoc.Released == false); //It's always  on Hold ???
			bool enableCCProcess = false;
			bool docTypePayment = (doc.DocType == ARDocType.Invoice || doc.DocType == ARDocType.CashSale);
			bool isCashReturn = doc.DocType == ARDocType.CashReturn;

			doc.IsCCPayment = false;
			doc.PaymentProjectID = PM.ProjectDefaultAttribute.NonProject(this);
			if (doc.PMInstanceID!= null) 
			{
				PXResult<CustomerPaymentMethodC,CA.PaymentMethod> pmInstance  =(PXResult<CustomerPaymentMethodC,CA.PaymentMethod>)
					               PXSelectJoin<CustomerPaymentMethodC,
									InnerJoin<CA.PaymentMethod,
										On<CA.PaymentMethod.paymentMethodID,Equal<CustomerPaymentMethodC.paymentMethodID>>>,
								Where<CustomerPaymentMethodC.pMInstanceID, Equal<Optional<SOInvoice.pMInstanceID>>,
									And<CA.PaymentMethod.paymentType,Equal<CA.PaymentMethodType.creditCard>,
										And<CA.PaymentMethod.aRIsProcessingRequired,Equal<True>>>>>.Select(this,doc.PMInstanceID);
				if (pmInstance != null)
				{
					doc.IsCCPayment = true;
					enableCCProcess = (isCashReturn || docTypePayment || doc.DocType == ARDocType.Refund || doc.DocType == ARDocType.VoidPayment);
				}
			}
			arDoc.IsCCPayment = doc.IsCCPayment; 
			bool isCCVoided = false;
			bool isCCCaptured = false;
			bool isCCPreAuthorized = false;
			bool isCCRefunded = false;
			bool isCCVoidingAttempted = false; //Special flag for VoidPayment Release logic 

			if (enableCCProcess)
			{
				CCProcTran lastCCran;
				CCPaymentState ccPaymentState = CCPaymentEntry.ResolveCCPaymentState(ccProcTran.Select(), out lastCCran);
				isCCVoided = (ccPaymentState & CCPaymentState.Voided) != 0;
				isCCCaptured = (ccPaymentState & CCPaymentState.Captured) != 0;
				isCCPreAuthorized = (ccPaymentState & CCPaymentState.PreAuthorized) != 0;
				isCCRefunded = (ccPaymentState & CCPaymentState.Refunded) != 0;
				isCCVoidingAttempted = (ccPaymentState & CCPaymentState.VoidFailed) != 0;
				doc.CCPaymentStateDescr = CCPaymentEntry.FormatCCPaymentState(ccPaymentState);
				doc.CCAuthTranNbr = lastCCran != null ? lastCCran.TranNbr : null; 
			}

			bool canAuthorize = docNotOnHold && docTypePayment && !(isCCPreAuthorized || isCCCaptured);
			bool canCapture = docNotOnHold && docTypePayment && !(isCCCaptured);
			bool canVoid = docNotOnHold && ((isCCCaptured  || isCCPreAuthorized) && docTypePayment);
			

			this.authorizeCCPayment.SetEnabled(enableCCProcess && canAuthorize);
			this.captureCCPayment.SetEnabled(enableCCProcess && canCapture);
			this.voidCCPayment.SetEnabled(enableCCProcess && canVoid);
			this.creditCCPayment.SetEnabled(enableCCProcess && isCashReturn && !isCCRefunded && doc.RefTranExtNbr != null);
			PXUIFieldAttribute.SetEnabled(this.ccLastTran.Cache, null, false);

			//PXUIFieldAttribute.SetEnabled<SOInvoice.cCAuthExpirationDate>(sender, doc, false);
			//PXUIFieldAttribute.SetEnabled<SOInvoice.curyCCPreAuthAmount>(sender, doc, false);

			PXUIFieldAttribute.SetEnabled<SOInvoice.refTranExtNbr>(sender, doc, doc.PMInstanceID.HasValue && doc.IsCCPayment == true && isCashReturn && !isCCRefunded);
			PXUIFieldAttribute.SetVisible<SOInvoice.cCPaymentStateDescr>(sender, doc, enableCCProcess);
			PXUIFieldAttribute.SetEnabled<SOInvoice.curyDiscTot>(SODocument.Cache, e.Row, Document.Cache.AllowUpdate);
            bool allowPaymentInfo = Document.Cache.AllowUpdate && (((SOInvoice)e.Row).DocType == ARDocType.CashSale || ((SOInvoice)e.Row).DocType == ARDocType.CashReturn || ((SOInvoice)e.Row).DocType == ARDocType.Invoice)
                                        && !isCCPreAuthorized && !isCCCaptured;
            bool isPMInstanceRequired = false;
            if (allowPaymentInfo && (String.IsNullOrEmpty(doc.PaymentMethodID) == false))
            {
                CA.PaymentMethod pm = PXSelect<CA.PaymentMethod, Where<CA.PaymentMethod.paymentMethodID, Equal<Required<CA.PaymentMethod.paymentMethodID>>>>.Select(this, doc.PaymentMethodID);
                isPMInstanceRequired = (pm.IsAccountNumberRequired == true);
            }
            PXUIFieldAttribute.SetEnabled<SOInvoice.paymentMethodID>(SODocument.Cache, e.Row, allowPaymentInfo);
			PXUIFieldAttribute.SetEnabled<SOInvoice.pMInstanceID>(SODocument.Cache, e.Row, allowPaymentInfo && isPMInstanceRequired);
			PXUIFieldAttribute.SetEnabled<SOInvoice.cashAccountID>(SODocument.Cache, e.Row, Document.Cache.AllowUpdate && (((SOInvoice)e.Row).PMInstanceID != null || string.IsNullOrEmpty(doc.PaymentMethodID)==false));
			PXUIFieldAttribute.SetEnabled<SOInvoice.extRefNbr>(SODocument.Cache, e.Row, Document.Cache.AllowUpdate && (((SOInvoice)e.Row).PMInstanceID != null || string.IsNullOrEmpty(doc.PaymentMethodID) == false));
			PXUIFieldAttribute.SetEnabled<SOInvoice.cleared>(SODocument.Cache, e.Row, Document.Cache.AllowUpdate && (((SOInvoice)e.Row).PMInstanceID != null || string.IsNullOrEmpty(doc.PaymentMethodID) == false) && (((SOInvoice)e.Row).DocType == ARDocType.CashSale || ((SOInvoice)e.Row).DocType == ARDocType.CashReturn));
			PXUIFieldAttribute.SetEnabled<SOInvoice.clearDate>(SODocument.Cache, e.Row, Document.Cache.AllowUpdate && (((SOInvoice)e.Row).PMInstanceID != null || string.IsNullOrEmpty(doc.PaymentMethodID) == false) && (((SOInvoice)e.Row).DocType == ARDocType.CashSale || ((SOInvoice)e.Row).DocType == ARDocType.CashReturn));

			if (IsExternalTax == true && ((SOInvoice)e.Row).IsTaxValid != true)
				PXUIFieldAttribute.SetWarning<SOInvoice.curyTaxTotal>(sender, e.Row, AR.Messages.TaxIsNotUptodate);

            if (Document.Current != null && Document.Current.SkipDiscounts == true)
            {
                DiscountDetails.Cache.AllowDelete = false;
                DiscountDetails.Cache.AllowUpdate = false;
                DiscountDetails.Cache.AllowInsert = false;
            }
            else
            {
                DiscountDetails.Cache.AllowDelete = Transactions.Cache.AllowDelete;
                DiscountDetails.Cache.AllowUpdate = Transactions.Cache.AllowUpdate;
                DiscountDetails.Cache.AllowInsert = Transactions.Cache.AllowInsert;
            }

			bool isAuthorizedCashSale = (doc.DocType == ARDocType.CashSale && (isCCPreAuthorized || isCCCaptured));
			PXUIFieldAttribute.SetEnabled<SOInvoice.curyDiscTot>(sender, doc, !isAuthorizedCashSale);
			
		}

        protected virtual void SOInvoice_PaymentMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            sender.SetDefaultExt<SOInvoice.pMInstanceID>(e.Row);
            sender.SetDefaultExt<SOInvoice.cashAccountID>(e.Row);
            sender.SetDefaultExt<SOInvoice.isCCCaptureFailed>(e.Row);
        }

		protected virtual void SOInvoice_PMInstanceID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			//sender.SetDefaultExt<SOInvoice.paymentMethodID>(e.Row);
			sender.SetDefaultExt<SOInvoice.cashAccountID>(e.Row);
			sender.SetDefaultExt<SOInvoice.isCCCaptureFailed>(e.Row);
			sender.SetValueExt<SOInvoice.refTranExtNbr>(e.Row, null);
		}

		protected virtual void SOInvoice_CuryDiscTot_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOInvoice row = e.Row as SOInvoice;
			if (row == null) return;

			if (IsExternalTax == true)
			{
				row.IsTaxValid = false;
			}
		}


		protected virtual void SOInvoice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
			{
				SOInvoice doc = (SOInvoice)e.Row;

				if ((doc.DocType == ARDocType.CashSale || doc.DocType == ARDocType.CashReturn))
				{
                    if (String.IsNullOrEmpty(doc.PaymentMethodID) == true)
                    {
                        if (sender.RaiseExceptionHandling<SOInvoice.pMInstanceID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(SOInvoice.pMInstanceID).Name)))
                        {
                            throw new PXRowPersistingException(typeof(SOInvoice.pMInstanceID).Name, null, ErrorMessages.FieldIsEmpty, typeof(SOInvoice.pMInstanceID).Name);
                        }
                    }
                    else
                    {
                        
                        CA.PaymentMethod pm = PXSelect<CA.PaymentMethod, Where<CA.PaymentMethod.paymentMethodID, Equal<Required<CA.PaymentMethod.paymentMethodID>>>>.Select(this, doc.PaymentMethodID);
                        bool pmInstanceRequired = (pm.IsAccountNumberRequired == true);
                        if (pmInstanceRequired && doc.PMInstanceID == null)
                        {
                            if (sender.RaiseExceptionHandling<SOInvoice.pMInstanceID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(SOInvoice.pMInstanceID).Name)))
                            {
                                throw new PXRowPersistingException(typeof(SOInvoice.pMInstanceID).Name, null, ErrorMessages.FieldIsEmpty, typeof(SOInvoice.pMInstanceID).Name);
                            }
                        }
                    }
				}

				bool isCashSale = (doc.DocType == AR.ARDocType.CashSale) || (doc.DocType == AR.ARDocType.CashReturn);
                if (isCashSale && SODocument.GetValueExt<SOInvoice.cashAccountID>((SOInvoice)e.Row) == null)
				{
					if (sender.RaiseExceptionHandling<SOInvoice.cashAccountID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(SOInvoice.cashAccountID).Name)))
					{
						throw new PXRowPersistingException(typeof(SOInvoice.cashAccountID).Name, null, ErrorMessages.FieldIsEmpty, typeof(SOInvoice.cashAccountID).Name);
					}
				}

				object acctcd;

				if ((acctcd = SODocument.GetValueExt<SOInvoice.cashAccountID>((SOInvoice)e.Row)) != null && sender.GetValue<SOInvoice.cashAccountID>(e.Row) == null)
				{
					sender.RaiseExceptionHandling<SOInvoice.cashAccountID>(e.Row, null, null);
					sender.SetValueExt<SOInvoice.cashAccountID>(e.Row, acctcd is PXFieldState ? ((PXFieldState)acctcd).Value : acctcd);
				}

				//if (doc.PMInstanceID != null && string.IsNullOrEmpty(doc.ExtRefNbr))
				//{
				//    if (sender.RaiseExceptionHandling<SOInvoice.extRefNbr>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(SOInvoice.extRefNbr).Name)))
				//    {
				//        throw new PXRowPersistingException(typeof(SOInvoice.extRefNbr).Name, null, ErrorMessages.FieldIsEmpty, typeof(SOInvoice.extRefNbr).Name);
				//    }
				//}
			}
		}

		protected virtual void SOInvoice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<SOInvoice.curyDiscTot>(e.OldRow, e.Row))
			{
                AddDiscount(sender, e);
			}

			if (!sender.ObjectsEqual<SOInvoice.isCCCaptured>(e.Row, e.OldRow) && ((SOInvoice)e.Row).IsCCCaptured == true)
			{
				ARInvoice copy = (ARInvoice)Document.Cache.CreateCopy(Document.Current);

				copy.CreditHold = false;

				Document.Cache.Update(copy);
			}

			if (!sender.ObjectsEqual<SOInvoice.curyPaymentTotal>(e.OldRow, e.Row))
			{
				ARInvoice ardoc = Document.Search<ARInvoice.refNbr>(((SOInvoice)e.Row).RefNbr, ((SOInvoice)e.Row).DocType);
				//is null on delete operation
				if (ardoc != null)
				{
					((SOInvoice)e.Row).CuryPaymentAmt = ardoc.CuryOrigDocAmt - ardoc.CuryOrigDiscAmt - ((SOInvoice)e.Row).CuryPaymentTotal;
				}
			}

			if (!sender.ObjectsEqual<SOInvoice.pMInstanceID, SOInvoice.paymentMethodID, SOInvoice.cashAccountID>(e.Row, e.OldRow))
			{ 
				ARInvoice ardoc = Document.Search<ARInvoice.refNbr>(((SOInvoice)e.Row).RefNbr, ((SOInvoice)e.Row).DocType);
				//is null on delete operation
				if (ardoc != null)
				{
					ardoc.PMInstanceID = ((SOInvoice)e.Row).PMInstanceID;
					ardoc.PaymentMethodID = ((SOInvoice)e.Row).PaymentMethodID;
					ardoc.CashAccountID = ((SOInvoice)e.Row).CashAccountID;
					
					if (Document.Cache.GetStatus(ardoc) == PXEntryStatus.Notchanged)
					{
						Document.Cache.SetStatus(ardoc, PXEntryStatus.Updated);
					}
				}
			}
		}

		protected virtual void SOInvoice_CuryDiscTot_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOInvoice row = e.Row as SOInvoice;
			if (row.CuryLineTotal + row.CuryMiscTot >= 0m && row.CuryLineTotal + row.CuryMiscTot < Convert.ToDecimal(e.NewValue))
			{
				throw new PXSetPropertyException(CS.Messages.Entry_LE, (row.CuryLineTotal + row.CuryMiscTot).ToString());
			}

			if (row.CuryLineTotal + row.CuryMiscTot < 0m && row.CuryLineTotal + row.CuryMiscTot > Convert.ToDecimal(e.NewValue))
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, (row.CuryLineTotal + row.CuryMiscTot).ToString());
			}
		}

		protected override void ARAdjust_CuryAdjdAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARAdjust adj = (ARAdjust)e.Row;
			Terms terms = PXSelect<Terms, Where<Terms.termsID, Equal<Current<ARInvoice.termsID>>>>.Select(this);

			if (terms != null && terms.InstallmentType != TermsInstallmentType.Single && (decimal)e.NewValue > 0m)
			{
				throw new PXSetPropertyException(AR.Messages.PrepaymentAppliedToMultiplyInstallments);
			}

			if (adj.CuryDocBal == null)
			{
				PXResult<ARPayment, CurrencyInfo> res = (PXResult<ARPayment, CurrencyInfo>)PXSelectReadonly2<ARPayment, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARPayment.curyInfoID>>>, Where<ARPayment.docType, Equal<Required<ARPayment.docType>>, And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>.Select(this, adj.AdjgDocType, adj.AdjgRefNbr);

				ARPayment payment = PXCache<ARPayment>.CreateCopy(res);
				CurrencyInfo pay_info = (CurrencyInfo)res;
				CurrencyInfo inv_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<ARInvoice.curyInfoID>>>>.Select(this);

				ARAdjust other = PXSelectGroupBy<ARAdjust, Where<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>, And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>, And<ARAdjust.released, Equal<False>, And<Where<ARAdjust.adjdDocType, NotEqual<Required<ARAdjust.adjdDocType>>, Or<ARAdjust.adjdRefNbr, NotEqual<Required<ARAdjust.adjdRefNbr>>>>>>>>, Aggregate<GroupBy<ARAdjust.adjgDocType, GroupBy<ARAdjust.adjgRefNbr, Sum<ARAdjust.curyAdjgAmt, Sum<ARAdjust.adjAmt>>>>>>.Select(this, adj.AdjgDocType, adj.AdjgRefNbr, adj.AdjdDocType, adj.AdjdRefNbr);
				if (other != null && other.AdjdRefNbr != null)
				{
					payment.CuryDocBal -= other.CuryAdjgAmt;
					payment.DocBal -= other.AdjAmt;
				}

				decimal CuryDocBal;
				if (string.Equals(pay_info.CuryID, inv_info.CuryID))
				{
					CuryDocBal = (decimal)payment.CuryDocBal;
				}
				else
				{
					PXDBCurrencyAttribute.CuryConvCury(sender, inv_info, (decimal)payment.DocBal, out CuryDocBal);
				}

				adj.CuryDocBal = CuryDocBal - adj.CuryAdjdAmt;
			}

			if ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjdAmt - (decimal)e.NewValue < 0)
			{
				throw new PXSetPropertyException(AR.Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjdAmt).ToString());
			}
		}

        protected override void ARTran_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            if (e.ExternalCall)
                RecalculateDiscounts(sender, (ARTran)e.Row);
            TaxAttribute.Calculate<ARTran.taxCategoryID>(sender, e);

            if (SODocument.Current != null)
            {
                SODocument.Current.IsTaxValid = false;
                if (SODocument.Cache.GetStatus(SODocument.Current) == PXEntryStatus.Notchanged)
                    SODocument.Cache.SetStatus(SODocument.Current, PXEntryStatus.Updated);
            }

            if (Document.Current != null)
            {
                Document.Current.IsTaxValid = false;
                if (SODocument.Cache.GetStatus(SODocument.Current) == PXEntryStatus.Notchanged)
                    SODocument.Cache.SetStatus(SODocument.Current, PXEntryStatus.Updated);
            }
        }

		protected override void ARTran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			base.ARTran_RowDeleted(sender, e);

            if (((ARTran)e.Row).InventoryID != null)
            {
                if (((ARTran)e.Row).LineType != SOLineType.Discount)
                    DiscountEngine<ARTran>.RecalculateGroupAndDocumentDiscounts(sender, Transactions, DiscountDetails, Document.Current.CustomerLocationID, Document.Current.DocDate.Value, Document.Current.SkipDiscounts);
                RecalculateTotalDiscount();
            }

			ARTran siblings = PXSelect<ARTran, Where<ARTran.sOOrderType, Equal<Required<ARTran.sOOrderType>>,
				And<ARTran.sOOrderNbr, Equal<Required<ARTran.sOOrderNbr>>,
				And<ARTran.sOShipmentType, Equal<Required<ARTran.sOShipmentType>>,
				And<ARTran.sOShipmentNbr, Equal<Required<ARTran.sOShipmentNbr>>,
				And<ARTran.tranType, Equal<Required<ARTran.tranType>>,
				And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>>>>>>>>.SelectWindowed(this, 0, 1,
				((ARTran)e.Row).SOOrderType, ((ARTran)e.Row).SOOrderNbr, ((ARTran)e.Row).SOShipmentType, ((ARTran)e.Row).SOShipmentNbr, ((ARTran)e.Row).TranType, ((ARTran)e.Row).RefNbr);

			if (siblings == null)
			{
				SOOrderShipment ordershipment;
				if ((ordershipment = PXSelect<SOOrderShipment, Where<SOOrderShipment.orderType, Equal<Required<SOOrderShipment.orderType>>,
					And<SOOrderShipment.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>,
					And<SOOrderShipment.shipmentType, Equal<Required<SOOrderShipment.shipmentType>>,
					And<SOOrderShipment.shipmentNbr, Equal<Required<SOOrderShipment.shipmentNbr>>>>>>>.SelectWindowed(this, 0, 1,
					((ARTran)e.Row).SOOrderType, ((ARTran)e.Row).SOOrderNbr, ((ARTran)e.Row).SOShipmentType, ((ARTran)e.Row).SOShipmentNbr)) != null)
				{
					shipmentlist.Delete(ordershipment);
				}
			}

			if (SODocument.Current != null)
			{
				SODocument.Current.IsTaxValid = false;
				if (SODocument.Cache.GetStatus(SODocument.Current) == PXEntryStatus.Notchanged)
					SODocument.Cache.SetStatus(SODocument.Current, PXEntryStatus.Updated);
			}

			if (Document.Current != null)
			{
				Document.Current.IsTaxValid = false;
				if (Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Notchanged)
					Document.Cache.SetStatus(Document.Current, PXEntryStatus.Updated);
			}
		}

		protected override void ARTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARTran row = (ARTran)e.Row;
			ARTran oldRow = (ARTran)e.OldRow;

			if (row != null)
			{
				if (!sender.ObjectsEqual<ARTran.qty,
					ARTran.uOM>(e.OldRow, e.Row))
				{
					SetFlatUnitPrice(sender, row);
				}

                if (!sender.ObjectsEqual<ARTran.branchID>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.inventoryID>(e.Row, e.OldRow) ||
                    !sender.ObjectsEqual<ARTran.qty>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.curyUnitPrice>(e.Row, e.OldRow) ||
                    !sender.ObjectsEqual<ARTran.curyExtPrice>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.curyDiscAmt>(e.Row, e.OldRow) ||
                    !sender.ObjectsEqual<ARTran.discPct>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.manualDisc>(e.Row, e.OldRow) ||
                    !sender.ObjectsEqual<ARTran.discountID>(e.Row, e.OldRow))
                    RecalculateDiscounts(sender, row);

				TaxAttribute.Calculate<ARTran.taxCategoryID>(sender, e);

				//if any of the fields that was saved in avalara has changed mark doc as TaxInvalid.
				if (SODocument.Current != null && IsExternalTax == true &&
					!sender.ObjectsEqual<ARTran.accountID, ARTran.inventoryID, ARTran.tranDesc, ARTran.tranAmt, ARTran.tranDate, ARTran.taxCategoryID>(e.Row, e.OldRow))
				{
					if (SODocument.Current != null && SODocument.Current.IsTaxValid == true)
					{
					    SODocument.Current.IsTaxValid = false;
					    if (SODocument.Cache.GetStatus(SODocument.Current) == PXEntryStatus.Notchanged)
					        SODocument.Cache.SetStatus(SODocument.Current, PXEntryStatus.Updated);
					}

					if (Document.Current != null)
					{
					    Document.Current.IsTaxValid = false;
					    if (Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Notchanged)
					        Document.Cache.SetStatus(Document.Current, PXEntryStatus.Updated);
					}
				}
			}
		}

		protected override void ARTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.ARTran_RowSelected(sender, e);

			ARTran row = e.Row as ARTran;
			if (row != null)
			{
				PXUIFieldAttribute.SetEnabled<ARTran.inventoryID>(sender, row, row.SOOrderNbr == null);
				PXUIFieldAttribute.SetEnabled<ARTran.qty>(sender, row, row.SOOrderNbr == null);
				PXUIFieldAttribute.SetEnabled<ARTran.uOM>(sender, row, row.SOOrderNbr == null);
			}
		}

		protected override void ARTran_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (((ARTran)e.Row).SOOrderNbr == null)
			{
				sender.SetDefaultExt<ARTran.lineType>(e.Row);
			}

			base.ARTran_InventoryID_FieldUpdated(sender, e);
		}

		protected virtual void ARTran_LineType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && ((ARTran)e.Row).SOOrderNbr == null && ((ARTran)e.Row).InventoryID != null)
			{
				InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<ARTran.inventoryID>(sender, e.Row);

				if (item != null)
				{
					e.NewValue = item.NonStockShip == true ? SOLineType.NonInventory : SOLineType.MiscCharge;
					e.Cancel = true;
				}
			}
		}

		protected override void ARTran_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARTran.unitPrice>(e.Row);
			sender.SetValue<ARTran.unitPrice>(e.Row, null);
		}

		protected override void ARTran_Qty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			base.ARTran_Qty_FieldUpdated(sender, e);
			ARTran row = e.Row as ARTran;
			if (row != null)
			{
				sender.SetDefaultExt<ARTran.tranDate>(row);
				sender.SetValueExt<ARTran.manualDisc>(row, false);

                if (row != null && row.InventoryID != null && row.UOM != null && row.IsFree != true)
                {
                    string customerPriceClass = ARPriceClass.EmptyPriceClass;
                    Location c = location.Select();
                    if (c != null && !string.IsNullOrEmpty(c.CPriceClassID))
                        customerPriceClass = c.CPriceClassID;

                    DateTime date = Document.Current.DocDate.Value;

                    if (row.TranType == ARDocType.CreditMemo && row.OrigInvoiceDate != null)
                    {
                        date = row.OrigInvoiceDate.Value;
                    }

                    decimal? price = ARSalesPriceMaint.CalculateSalesPrice(sender, customerPriceClass, row.CustomerID, row.InventoryID, currencyinfo.Select(), row.UOM, row.Qty, date, row.CuryUnitPrice);
                    if (price != null)
                    sender.SetValueExt<ARTran.curyUnitPrice>(e.Row, price);
                }
			}
		}
				
		protected override void ARTran_TaxCategoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && string.IsNullOrEmpty(((ARTran)e.Row).SOOrderNbr) == false)
			{
				//tax category is taken from invoice lines
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void ARTran_SalesPersonID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && string.IsNullOrEmpty(((ARTran)e.Row).SOOrderNbr) == false)
			{
				//salesperson is taken from invoice lines
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected override void ARTran_AccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && string.IsNullOrEmpty(((ARTran)e.Row).SOOrderType) == false)
			{
                ARTran tran = (ARTran)e.Row;

                if (tran != null)
                {
                    PXResult<InventoryItem, INPostClass, INSite> item = (PXResult<InventoryItem, INPostClass, INSite>)PXSelectJoin<InventoryItem, LeftJoin<INPostClass, On<INPostClass.postClassID, Equal<InventoryItem.postClassID>>, CrossJoin<INSite>>, Where<InventoryItem.inventoryID, Equal<Required<ARTran.inventoryID>>, And<INSite.siteID, Equal<Required<ARTran.siteID>>>>>.Select(this, tran.InventoryID, tran.SiteID);
                    Location customerloc = location.Current;

                    if (item == null)
                    {
                        return;
                    }

                    ReasonCode reasoncode = PXSelectJoin<ReasonCode, InnerJoin<SOLine, On<SOLine.reasonCode, Equal<ReasonCode.reasonCodeID>>>, Where<SOLine.orderNbr, Equal<Required<ARTran.sOOrderNbr>>, And<SOLine.lineNbr, Equal<Required<ARTran.sOOrderLineNbr>>>>>.Select(this, tran.SOOrderNbr, tran.SOOrderLineNbr);

                    switch (soordertype.Current.SalesAcctDefault)
                    {
                        case SOSalesAcctSubDefault.MaskItem:
                            e.NewValue = GetValue<InventoryItem.salesAcctID>((InventoryItem)item);
                            e.Cancel = true;
                            break;
                        case SOSalesAcctSubDefault.MaskSite:
                            e.NewValue = GetValue<INSite.salesAcctID>((INSite)item);
                            e.Cancel = true;
                            break;
                        case SOSalesAcctSubDefault.MaskClass:
                            e.NewValue = GetValue<INPostClass.salesAcctID>((INPostClass)item);
                            e.Cancel = true;
                            break;
                        case SOSalesAcctSubDefault.MaskLocation:
                            e.NewValue = GetValue<Location.cSalesAcctID>(customerloc);
                            e.Cancel = true;
                            break;
                        case SOSalesAcctSubDefault.MaskReasonCode:
                            e.NewValue = GetValue<ReasonCode.salesAcctID>(reasoncode);
                            e.Cancel = true;
                            break;
                    }
                }
			}
			else
			{
				base.ARTran_AccountID_FieldDefaulting(sender, e);
			}
		}

		protected override void ARTran_SubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && string.IsNullOrEmpty(((ARTran)e.Row).SOOrderType) == false)
			{
                ARTran tran = (ARTran)e.Row;

                if (tran != null && tran.AccountID != null)
                {
                    PXResult<InventoryItem, INPostClass, INSite> item = (PXResult<InventoryItem, INPostClass, INSite>)PXSelectJoin<InventoryItem, LeftJoin<INPostClass, On<INPostClass.postClassID, Equal<InventoryItem.postClassID>>, CrossJoin<INSite>>, Where<InventoryItem.inventoryID, Equal<Required<ARTran.inventoryID>>, And<INSite.siteID, Equal<Required<ARTran.siteID>>>>>.Select(this, tran.InventoryID, tran.SiteID);
                    ReasonCode reasoncode = PXSelectJoin<ReasonCode, InnerJoin<SOLine, On<SOLine.reasonCode, Equal<ReasonCode.reasonCodeID>>>, Where<SOLine.orderNbr, Equal<Required<ARTran.sOOrderNbr>>, And<SOLine.lineNbr, Equal<Required<ARTran.sOOrderLineNbr>>>>>.Select(this, tran.SOOrderNbr, tran.SOOrderLineNbr);
                    EPEmployee employee = (EPEmployee)PXSelectJoin<EPEmployee, InnerJoin<SOOrder, On<EPEmployee.userID, Equal<SOOrder.ownerID>>>,Where<SOOrder.orderNbr, Equal<Required<ARTran.sOOrderNbr>>>>.Select(this, tran.SOOrderNbr);
                    Location companyloc =
                        (Location)PXSelectJoin<Location, InnerJoin<BAccountR, On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>, InnerJoin<Branch, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>>>, Where<Branch.branchID, Equal<Required<ARTran.branchID>>>>.Select(this, tran.BranchID);
                    Location customerloc = location.Current;
                    SalesPerson salesperson = (SalesPerson)PXSelect<SalesPerson, Where<SalesPerson.salesPersonID, Equal<Current<ARTran.salesPersonID>>>>.Select(this);

                    object item_SubID = GetValue<InventoryItem.salesSubID>((InventoryItem)item);
                    object site_SubID = GetValue<INSite.salesSubID>((INSite)item);
                    object postclass_SubID = GetValue<INPostClass.salesSubID>((INPostClass)item);
                    object customer_SubID = GetValue<Location.cSalesSubID>(customerloc);
                    object employee_SubID = GetValue<EPEmployee.salesSubID>(employee);
                    object company_SubID = GetValue<Location.cMPSalesSubID>(companyloc);
                    object salesperson_SubID = GetValue<SalesPerson.salesSubID>(salesperson);
                    object reasoncode_SubID = GetValue<ReasonCode.salesSubID>(reasoncode);

                    object value = null;

                    try
                    {
                        value = SOSalesSubAccountMaskAttribute.MakeSub<SOOrderType.salesSubMask>(this, soordertype.Current.SalesSubMask,
                                                                                                 new object[] 
                                                                                             { 
                                                                                                 item_SubID, 
                                                                                                 site_SubID, 
                                                                                                 postclass_SubID, 
                                                                                                 customer_SubID, 
                                                                                                 employee_SubID, 
                                                                                                 company_SubID, 
                                                                                                 salesperson_SubID, 
                                                                                                 reasoncode_SubID 
                                                                                             },
                                                                                                 new Type[] 
                                                                                             { 
                                                                                                 typeof(InventoryItem.salesSubID), 
                                                                                                 typeof(INSite.salesSubID), 
                                                                                                 typeof(INPostClass.salesSubID), 
                                                                                                 typeof(Location.cSalesSubID), 
                                                                                                 typeof(EPEmployee.salesSubID), 
                                                                                                 typeof(Location.cMPSalesSubID), 
                                                                                                 typeof(SalesPerson.salesSubID), 
                                                                                                 typeof(ReasonCode.subID) 
                                                                                             });

                        sender.RaiseFieldUpdating<ARTran.subID>(tran, ref value);
                    }
                    catch (PXMaskArgumentException ex)
                    {
                        sender.RaiseExceptionHandling<ARTran.subID>(e.Row, null, new PXSetPropertyException(ex.Message));
                        value = null;
                    }
                    catch (PXSetPropertyException ex)
                    {
                        sender.RaiseExceptionHandling<ARTran.subID>(e.Row, value, ex);
                        value = null;
                    }

                    e.NewValue = (int?)value;
                    e.Cancel = true;
                }
			}
			else
			{
				base.ARTran_SubID_FieldDefaulting(sender, e);
			}
		}

		protected virtual void AddDiscount(PXCache sender, PXRowUpdatedEventArgs e)
		{
            AddDiscountDetails(sender, e);
            
            ARTran discount = (ARTran)Discount.Cache.CreateInstance();
			discount.LineType = SOLineType.Discount;
			discount.DrCr = (Document.Current.DrCr == "D") ? "C" : "D";
			discount.FreezeManualDisc = true;
			discount = (ARTran)Discount.Select() ?? (ARTran)Discount.Cache.Insert(discount);

			ARTran old_row = (ARTran)Discount.Cache.CreateCopy(discount);

			discount.CuryTranAmt = (decimal?)sender.GetValue<SOInvoice.curyDiscTot>(e.Row);
			discount.TaxCategoryID = null;
			discount.TranDesc = PXMessages.LocalizeNoPrefix(Messages.DocDiscDescr);
			
			DefaultDiscountAccountAndSubAccount(discount);

            if (discount.TaskID == null && !PM.ProjectDefaultAttribute.IsNonProject(this, discount.ProjectID))
            {
                PM.PMProject project = PXSelect<PM.PMProject, Where<PM.PMProject.contractID, Equal<Required<PM.PMProject.contractID>>>>.Select(this, discount.ProjectID);
                if (project != null && project.BaseType != "C")
                {
                    PM.PMAccountTask task = PXSelect<PM.PMAccountTask, Where<PM.PMAccountTask.accountID, Equal<Required<PM.PMAccountTask.accountID>>>>.Select(this, discount.AccountID);
                    if (task != null)
                    {
                        discount.TaskID = task.TaskID;
                    }
                    else
                    {
                        Account ac = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, discount.AccountID);
                        throw new PXException(string.Format(Messages.AccountMappingNotConfigured, project.ContractCD, ac.AccountCD));
                    }
                }
            }
			
			if (Discount.Cache.GetStatus(discount) == PXEntryStatus.Notchanged)
			{
				Discount.Cache.SetStatus(discount, PXEntryStatus.Updated);
			}

			discount.ManualDisc = true; //escape SOManualDiscMode.RowUpdated
			Discount.Cache.RaiseRowUpdated(discount, old_row);

			decimal auotDocDisc = GetAutoDocDiscount();
			if (auotDocDisc == discount.CuryTranAmt)
			{
				discount.ManualDisc = false;
			}
			
			if (discount.CuryTranAmt == 0)
			{
				Discount.Delete(discount);
			}
		}

        protected virtual void AddDiscountDetails(PXCache sender, PXRowUpdatedEventArgs e)
        {
            foreach (SOInvoiceDiscountDetail dDetail in DiscountDetails.Select())
            {
                ARInvoiceDiscountDetail arDiscountDetail = new ARInvoiceDiscountDetail();
                SOInvoiceDiscountDetail discountCopy = (SOInvoiceDiscountDetail)DiscountDetails.Cache.CreateCopy(dDetail);
                foreach (ARInvoiceDiscountDetail aDetail in ARDiscountDetails.Select())
                {
                    if (aDetail.DiscountID == discountCopy.DiscountID && aDetail.DiscountSequenceID == discountCopy.DiscountSequenceID && aDetail.Type == discountCopy.Type)
                    {
                        arDiscountDetail = aDetail;
                    }
                }
                arDiscountDetail.CuryDiscountableAmt = discountCopy.CuryDiscountableAmt;
                arDiscountDetail.CuryDiscountAmt = discountCopy.CuryDiscountAmt;
                arDiscountDetail.CuryInfoID = discountCopy.CuryInfoID;
                arDiscountDetail.DiscountableAmt = discountCopy.DiscountableAmt;
                arDiscountDetail.DiscountableQty = discountCopy.DiscountableQty;
                arDiscountDetail.DiscountAmt = discountCopy.DiscountAmt;
                arDiscountDetail.DiscountID = discountCopy.DiscountID;
                arDiscountDetail.DiscountPct = discountCopy.DiscountPct;
                arDiscountDetail.DiscountSequenceID = discountCopy.DiscountSequenceID;
                arDiscountDetail.FreeItemID = discountCopy.FreeItemID;
                arDiscountDetail.FreeItemQty = discountCopy.FreeItemQty;
                arDiscountDetail.Type = discountCopy.Type;
                ARDiscountDetails.Update(arDiscountDetail);
            }
        }

		protected virtual void SOFreightDetail_AccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOFreightDetail row = e.Row as SOFreightDetail;
			if (row != null && row.TaskID == null)
			{
				sender.SetDefaultExt<SOFreightDetail.taskID>(e.Row);
			}
		}


		protected virtual void SOFreightDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (Document.Current != null)
			{
				PXUIFieldAttribute.SetEnabled<SOFreightDetail.curyPremiumFreightAmt>(sender, e.Row, Document.Current.Released != true);
			}
		}
				
		protected virtual void SOFreightDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SOFreightDetail row = e.Row as SOFreightDetail;
			if (row != null)
			{
				foreach (ARTran tran in PXSelect<ARTran,
				Where<ARTran.lineType, Equal<SOLineType.freight>,
				And<ARTran.tranType, Equal<Current<ARInvoice.docType>>,
				And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>,
				And<ARTran.sOShipmentNbr, Equal<Required<ARTran.sOShipmentNbr>>,
				And<ARTran.sOShipmentType, NotEqual<SOShipmentType.dropShip>>>>>>>.Select(this, row.ShipmentNbr))
				{
					Freight.Delete(tran);
				}

				if (row.CuryTotalFreightAmt > 0)
					AddFreightTransaction(row);
			}
		}

		protected virtual void SOFreightDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			SOFreightDetail row = e.Row as SOFreightDetail;
			if (row != null)
			{
				if (row.CuryTotalFreightAmt > 0)
					AddFreightTransaction(row);
			}
		}


		public override IEnumerable transactions()
		{
			return PXSelectJoin<ARTran, 
				LeftJoin<SOLine, On<SOLine.orderType, Equal<ARTran.sOOrderType>, And<SOLine.orderNbr, Equal<ARTran.sOOrderNbr>, And<SOLine.lineNbr, Equal<ARTran.sOOrderLineNbr>>>>>, 
			Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>, And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>, 
				And<ARTran.lineType, NotEqual<SOLineType.freight>, And<ARTran.lineType, NotEqual<SOLineType.discount>>>>>, 
			OrderBy<Asc<ARTran.tranType, Asc<ARTran.refNbr, Asc<ARTran.lineNbr>>>>>.Select(this);
		}

		public override int ExecuteInsert(string viewName, IDictionary values, params object[] parameters)
		{
			switch (viewName)
			{
				case "Freight":
					values[PXDataUtils.FieldName<ARTran.lineType>()] = SOLineType.Freight;
					break;
				case "Discount":
					values[PXDataUtils.FieldName<ARTran.lineType>()] = SOLineType.Discount;
					break;
			}
			return base.ExecuteInsert(viewName, values, parameters);
		}

		public virtual IEnumerable sHipmentlist()
		{
			PXSelectBase<ARTran> cmd = new PXSelect<ARTran, Where<ARTran.sOShipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>, And<ARTran.sOShipmentType, Equal<Current<SOOrderShipment.shipmentType>>, And<ARTran.sOOrderType, Equal<Current<SOOrderShipment.orderType>>, And<ARTran.sOOrderNbr, Equal<Current<SOOrderShipment.orderNbr>>>>>>>(this);

			DocumentList<ARInvoice, SOInvoice> list = new DocumentList<ARInvoice, SOInvoice>(this);
			list.Add(Document.Current, SODocument.Select());

			bool newInvoice = Transactions.Select().Count == 0;

			foreach (SOOrderShipment shipment in shipmentlist.Cache.Updated)
			{
				yield return shipment;
			}

			foreach (PXResult<SOOrderShipment, SOOrder, SOShipLine, SOOrderType, SOOrderTypeOperation, ARTran> order in 
			PXSelectJoinGroupBy<SOOrderShipment,
			InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOOrderShipment.orderType>, And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>,
			InnerJoin<SOShipLine, On<SOShipLine.shipmentNbr, Equal<SOOrderShipment.shipmentNbr>, And<SOShipLine.origOrderType, Equal<SOOrderShipment.orderType>, And<SOShipLine.origOrderNbr, Equal<SOOrderShipment.orderNbr>>>>,
			InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrderShipment.orderType>>,
			InnerJoin<SOOrderTypeOperation, 
							On<SOOrderTypeOperation.orderType, Equal<SOOrder.orderType>,
							And<SOOrderTypeOperation.operation, Equal<SOOrderShipment.operation>>>,
			LeftJoin<ARTran, On<ARTran.sOShipmentNbr, Equal<SOShipLine.shipmentNbr>, And<ARTran.sOShipmentType, Equal<SOShipLine.shipmentType>, And<ARTran.sOShipmentLineNbr, Equal<SOShipLine.lineNbr>>>>>>>>>,
			Where<SOOrderShipment.customerID, Equal<Current<ARInvoice.customerID>>,
				And<SOOrderShipment.hold, Equal<boolFalse>,
				And<SOOrderShipment.confirmed, Equal<boolTrue>, And<SOOrderShipment.shipmentType, NotEqual<SOShipmentType.dropShip>,
				And<SOOrderType.aRDocType, Equal<Current<ARInvoice.docType>>,
				And<ARTran.refNbr, IsNull>>>>>>,
			Aggregate<GroupBy<SOOrderShipment.shipmentNbr, GroupBy<SOOrderShipment.orderType, GroupBy<SOOrderShipment.orderNbr>>>>>.Select(this))
			{
				if (shipmentlist.Cache.Locate((SOOrderShipment)order) == null && cmd.View.SelectSingleBound(new object[] { (SOOrderShipment)order }) == null)
				{
					if (newInvoice || list.Find<ARInvoice.customerID, SOInvoice.billAddressID, SOInvoice.billContactID, ARInvoice.curyID, ARInvoice.termsID, ARInvoice.hidden>(((SOOrder)order).CustomerID, ((SOOrder)order).BillAddressID, ((SOOrder)order).BillContactID, ((SOOrder)order).CuryID, ((SOOrder)order).TermsID, false) != null)
					{
						yield return (SOOrderShipment)order;
					}
				}
			}

			foreach (PXResult<POReceipt, SOOrder, POReceiptLine> order in PXSelectJoinGroupBy<POReceipt,
					CrossJoin<SOOrder,
					InnerJoin<POReceiptLine, On<POReceiptLine.receiptNbr, Equal<POReceipt.receiptNbr>>,
					InnerJoin<SOLine, On<SOLine.pOType, Equal<POReceiptLine.pOType>, And<SOLine.pONbr, Equal<POReceiptLine.pONbr>, And<SOLine.pOLineNbr, Equal<POReceiptLine.pOLineNbr>, And<SOLine.orderType, Equal<SOOrder.orderType>, And<SOLine.orderNbr, Equal<SOOrder.orderNbr>>>>>>,
					LeftJoin<ARTran, On<ARTran.sOShipmentNbr, Equal<POReceiptLine.receiptNbr>, And<ARTran.sOShipmentType, Equal<SOShipmentType.dropShip>, And<ARTran.sOShipmentLineNbr, Equal<POReceiptLine.lineNbr>, And<ARTran.sOOrderType, Equal<SOLine.orderType>, And<ARTran.sOOrderNbr, Equal<SOLine.orderNbr>, And<ARTran.sOOrderLineNbr, Equal<SOLine.lineNbr>>>>>>>,
					InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOLine.orderType>>,
					InnerJoin<SOOrderTypeOperation, 
							On<SOOrderTypeOperation.orderType, Equal<SOLine.orderType>,
							And<SOOrderTypeOperation.operation, Equal<SOLine.operation>>>>>>>>>,
					Where<POReceipt.released, Equal<boolTrue>,
						And2<Where<POReceiptLine.lineType, Equal<POLineType.goodsForDropShip>, Or<POReceiptLine.lineType, Equal<POLineType.nonStockForDropShip>>>,
						And<SOOrder.customerID, Equal<Current<ARInvoice.customerID>>,
						And<SOOrderType.aRDocType, Equal<Current<ARInvoice.docType>>,
						And<ARTran.refNbr, IsNull>>>>>,
					Aggregate<GroupBy<POReceipt.receiptNbr,
						GroupBy<POReceipt.createdByID,
						GroupBy<POReceipt.lastModifiedByID,
						GroupBy<POReceipt.released,
						GroupBy<POReceipt.ownerID,
						GroupBy<POReceipt.hold,
						GroupBy<SOOrder.orderType,
						GroupBy<SOOrder.orderNbr,
						Sum<POReceiptLine.receiptQty,
						Sum<POReceiptLine.extWeight,
						Sum<POReceiptLine.extVolume>>>>>>>>>>>>>.Select(this))
			{
				SOOrderShipment cached;

				if ((cached = (SOOrderShipment)shipmentlist.Cache.Locate((SOOrderShipment)order)) == null && cmd.View.SelectSingleBound(new object[] { (SOOrderShipment)order }) == null)
				{
					if (newInvoice || list.Find<ARInvoice.customerID, SOInvoice.billAddressID, SOInvoice.billContactID, ARInvoice.curyID, ARInvoice.termsID, ARInvoice.hidden>(((SOOrder)order).CustomerID, ((SOOrder)order).BillAddressID, ((SOOrder)order).BillContactID, ((SOOrder)order).CuryID, ((SOOrder)order).TermsID, false) != null)
					{
						shipmentlist.Cache.SetStatus((SOOrderShipment)order, PXEntryStatus.Held);
						yield return (SOOrderShipment)order;
					}
				}
				else if (shipmentlist.Cache.GetStatus(cached) == PXEntryStatus.Notchanged || shipmentlist.Cache.GetStatus(cached) == PXEntryStatus.Held)
				{
					yield return (SOOrderShipment)order;
				}
			}
		}

		protected virtual void SOOrderShipment_ShipmentNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void SOOrderShipment_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			if (!string.Equals(((SOOrderShipment)e.Row).ShipmentNbr, Constants.NoShipmentNbr) && ((SOOrderShipment)e.Row).ShipmentType != SOShipmentType.DropShip)
			{
				SOOrderShipment copy = PXCache<SOOrderShipment>.CreateCopy((SOOrderShipment)e.Row);

				((SOOrderShipment)e.Row).InvoiceType = null;
				((SOOrderShipment)e.Row).InvoiceNbr = null;
				shipmentlist.Cache.SetStatus(((SOOrderShipment)e.Row), PXEntryStatus.Updated);
				shipmentlist.Cache.RaiseRowUpdated(e.Row, copy);

				//Probably not needed because of PXFormula referencing SOShipment
				foreach (SOShipment shipment in PXSelect<SOShipment, Where<SOShipment.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>>>.SelectSingleBound(this, new object[] { e.Row }))
				{
					//persist shipments to workflow
					shipments.Cache.SetStatus(shipment, PXEntryStatus.Updated);
				}

				e.Cancel = true;
			}
		}

		protected virtual void SOOrderShipment_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<SOOrderShipment.selected>(sender, e.Row, true);
		}

		protected virtual void SOOrder_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			SOOrder oldRow = e.OldRow as SOOrder;
			if (row != null && oldRow != null && row.UnbilledOrderQty != oldRow.UnbilledOrderQty)
			{
				row.IsUnbilledTaxValid = false;
			}
			

			if (e.OldRow != null)
			{
				ARReleaseProcess.UpdateARBalances(this, (SOOrder)e.OldRow, -((SOOrder)e.OldRow).UnbilledOrderTotal, -((SOOrder)e.Row).OpenOrderTotal);
			}
			ARReleaseProcess.UpdateARBalances(this, (SOOrder)e.Row, ((SOOrder)e.Row).UnbilledOrderTotal, ((SOOrder)e.Row).OpenOrderTotal);
		}

		public override IEnumerable adjustments()
		{
			CurrencyInfo inv_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<ARInvoice.curyInfoID>>>>.Select(this);

			foreach (PXResult<ARAdjust, ARPayment, CurrencyInfo> res in Adjustments_Raw.Select())
			{
				ARPayment payment = PXCache<ARPayment>.CreateCopy(res);
				ARAdjust adj = (ARAdjust)res;
				CurrencyInfo pay_info = (CurrencyInfo)res;

				ARAdjust other = PXSelectGroupBy<ARAdjust, Where<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>, And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>, And<ARAdjust.released, Equal<False>, And<Where<ARAdjust.adjdDocType, NotEqual<Required<ARAdjust.adjdDocType>>, Or<ARAdjust.adjdRefNbr, NotEqual<Required<ARAdjust.adjdRefNbr>>>>>>>>, Aggregate<GroupBy<ARAdjust.adjgDocType, GroupBy<ARAdjust.adjgRefNbr, Sum<ARAdjust.curyAdjgAmt, Sum<ARAdjust.adjAmt>>>>>>.Select(this, adj.AdjgDocType, adj.AdjgRefNbr, adj.AdjdDocType, adj.AdjdRefNbr);
				if (other != null && other.AdjdRefNbr != null)
				{
					payment.CuryDocBal -= other.CuryAdjgAmt;
					payment.DocBal -= other.AdjAmt;
				}

				decimal CuryDocBal;
				if (string.Equals(pay_info.CuryID, inv_info.CuryID))
				{
					CuryDocBal = (decimal)payment.CuryDocBal;
				}
				else
				{
					PXDBCurrencyAttribute.CuryConvCury(Adjustments.Cache, inv_info, (decimal)payment.DocBal, out CuryDocBal);
				}

				if (adj.Released == false)
				{
					if (adj.CuryAdjdAmt > CuryDocBal)
					{
						//if reconsidered need to calc RGOL
						adj.CuryDocBal = CuryDocBal;
						adj.CuryAdjdAmt = 0m;
					}
					else
					{
						adj.CuryDocBal = CuryDocBal - adj.CuryAdjdAmt;
					}
				}
				yield return res;
			}

			if (Document.Current != null && (Document.Current.DocType == ARDocType.Invoice || Document.Current.DocType == ARDocType.DebitMemo) && Document.Current.Released == false)
			{
				using (ReadOnlyScope rs = new ReadOnlyScope(Adjustments.Cache, Document.Cache, arbalances.Cache))
				{
					//same as ARInvoiceEntry but without released constraint and with hold constraint
					foreach (PXResult<AR.ARPayment, CurrencyInfo, ARAdjust> res in PXSelectReadonly2<ARPayment, 
						InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARPayment.curyInfoID>>, 
						LeftJoin<ARAdjust, 
							On<ARAdjust.adjgDocType, Equal<ARPayment.docType>, 
							And<ARAdjust.adjgRefNbr, Equal<ARPayment.refNbr>, 
							And<ARAdjust.adjNbr, Equal<ARPayment.lineCntr>, 
							And<ARAdjust.released, Equal<False>, 
							And<ARAdjust.hold, Equal<False>, 
							And<ARAdjust.voided, Equal<False>, 
							And<Where<ARAdjust.adjdDocType, NotEqual<Current<ARInvoice.docType>>, 
								Or<ARAdjust.adjdRefNbr, NotEqual<Current<ARInvoice.refNbr>>>>>>>>>>>>>, 
						Where<ARPayment.customerID, Equal<Current<ARInvoice.customerID>>, 
							And2<Where<ARPayment.docType, Equal<ARDocType.payment>, 
								Or<ARPayment.docType, Equal<ARDocType.prepayment>, 
								Or<ARPayment.docType, Equal<ARDocType.creditMemo>>>>, 
							And<ARPayment.docDate, LessEqual<Current<ARInvoice.docDate>>, 
							And<ARPayment.finPeriodID, LessEqual<Current<ARInvoice.finPeriodID>>, 
							And<ARPayment.openDoc, Equal<boolTrue>, 
							And<ARAdjust.adjdRefNbr, IsNull>>>>>>>.Select(this))
					{
						ARPayment payment = PXCache<ARPayment>.CreateCopy(res);
						ARAdjust adj = new ARAdjust();
						CurrencyInfo pay_info = (CurrencyInfo)res;

						adj.CustomerID = Document.Current.CustomerID;
						adj.AdjdDocType = Document.Current.DocType;
						adj.AdjdRefNbr = Document.Current.RefNbr;
						adj.AdjdBranchID = Document.Current.BranchID;
						adj.AdjgDocType = payment.DocType;
						adj.AdjgRefNbr = payment.RefNbr;
						adj.AdjgBranchID = payment.BranchID;
						adj.AdjNbr = payment.LineCntr;

						ARAdjust other = PXSelectGroupBy<ARAdjust, Where<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>, And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>, And<ARAdjust.released, Equal<False>, And<Where<ARAdjust.adjdDocType, NotEqual<Required<ARAdjust.adjdDocType>>, Or<ARAdjust.adjdRefNbr, NotEqual<Required<ARAdjust.adjdRefNbr>>>>>>>>, Aggregate<GroupBy<ARAdjust.adjgDocType, GroupBy<ARAdjust.adjgRefNbr, Sum<ARAdjust.curyAdjgAmt, Sum<ARAdjust.adjAmt>>>>>>.Select(this, adj.AdjgDocType, adj.AdjgRefNbr, adj.AdjdDocType, adj.AdjdRefNbr);
						if (other != null && other.AdjdRefNbr != null)
						{
							payment.CuryDocBal -= other.CuryAdjgAmt;
							payment.DocBal -= other.AdjAmt;
						}

						if (Adjustments.Cache.Locate(adj) == null)
						{
							adj.AdjgCuryInfoID = payment.CuryInfoID;
							adj.AdjdOrigCuryInfoID = Document.Current.CuryInfoID;
							//if LE constraint is removed from payment selection this must be reconsidered
							adj.AdjdCuryInfoID = Document.Current.CuryInfoID;

							decimal CuryDocBal;
							if (string.Equals(pay_info.CuryID, inv_info.CuryID))
							{
								CuryDocBal = (decimal)payment.CuryDocBal;
							}
							else
							{
								PXDBCurrencyAttribute.CuryConvCury(Adjustments.Cache, inv_info, (decimal)payment.DocBal, out CuryDocBal);
							}
							adj.CuryDocBal = CuryDocBal;

							yield return new PXResult<ARAdjust, AR.ARPayment>(Adjustments.Insert(adj), payment);
						}
					}
				}
			}
		}

		public virtual void InvoiceOrder(DateTime invoiceDate, PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact, SOOrderType> order, Customer customer, DocumentList<ARInvoice, SOInvoice> list)
		{
			InvoiceOrder(invoiceDate, order, null, customer, list);
		}

		public virtual void InvoiceOrder(DateTime invoiceDate, PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact, SOOrderType> order, PXResultset<SOShipLine, SOLine> details, Customer customer, DocumentList<ARInvoice, SOInvoice> list)
		{
			ARInvoice newdoc;
			SOOrder soOrder = order;
			SOOrderType ordertype = order;

            decimal ApprovedBalance = 0;
            decimal PrevDocBal = 0;

            PXRowUpdated ApprovedBalanceCollector = delegate(PXCache sender, PXRowUpdatedEventArgs e)
            {
                ARInvoice ARDoc = (ARInvoice)e.Row;
                if ((decimal)ARDoc.DocBal > (decimal)ARDoc.ApprovedCreditAmt)
                {
                    if ((bool)((SOOrder)order).ApprovedCredit && (decimal)ARDoc.DocBal > PrevDocBal)
                    {
                        ApprovedBalance += (decimal)ARDoc.DocBal - PrevDocBal;
                        ARDoc.ApprovedCreditAmt = ApprovedBalance;
                    }
                    ARDoc.ApprovedCredit = (ApprovedBalance == (decimal)ARDoc.DocBal ? true : false);
                    PrevDocBal = (decimal)ARDoc.DocBal;
                }
            };

            this.RowUpdated.AddHandler(typeof(ARInvoice), ApprovedBalanceCollector);

			if (list != null)
			{
				bool iscc = false;
				DateTime? orderInvoiceDate = (sosetup.Current.UseShipDateForInvoiceDate == true ? ((SOOrderShipment)order).ShipDate : soOrder.InvoiceDate);

				if (soOrder.BillSeparately == false)
				{
					iscc = PXSelectReadonly<CCProcTran, Where<CCProcTran.origDocType, Equal<Required<CCProcTran.origDocType>>, And<CCProcTran.origRefNbr, Equal<Required<CCProcTran.origRefNbr>>, And<CCProcTran.refNbr, IsNull>>>>.Select(this, soOrder.OrderType, soOrder.OrderNbr).Count > 0;
				}

				if (soOrder.PaymentCntr == 0 && soOrder.BillSeparately == false && iscc == false)
				{
					if(soOrder.PaymentMethodID == null && soOrder.CashAccountID == null)
						newdoc = list.Find<ARInvoice.docType, ARInvoice.docDate, ARInvoice.branchID, ARInvoice.customerID, ARInvoice.customerLocationID, SOInvoice.billAddressID, SOInvoice.billContactID, SOInvoice.extRefNbr, ARInvoice.curyID, ARInvoice.termsID, ARInvoice.hidden>(((SOOrderType)order).ARDocType, orderInvoiceDate ?? invoiceDate, soOrder.BranchID, soOrder.CustomerID, soOrder.CustomerLocationID, soOrder.BillAddressID, soOrder.BillContactID, soOrder.ExtRefNbr, soOrder.CuryID, soOrder.TermsID, false) ?? (ARInvoice)new ARInvoice();
					else if (soOrder.CashAccountID == null)
						newdoc = list.Find<ARInvoice.docType, ARInvoice.docDate, ARInvoice.branchID, ARInvoice.customerID, ARInvoice.customerLocationID, SOInvoice.billAddressID, SOInvoice.billContactID, SOInvoice.pMInstanceID, SOInvoice.extRefNbr, ARInvoice.curyID, ARInvoice.termsID, ARInvoice.hidden>(((SOOrderType)order).ARDocType, orderInvoiceDate ?? invoiceDate, soOrder.BranchID, soOrder.CustomerID, soOrder.CustomerLocationID, soOrder.BillAddressID, soOrder.BillContactID, soOrder.PMInstanceID, soOrder.ExtRefNbr, soOrder.CuryID, soOrder.TermsID, false) ?? (ARInvoice)new ARInvoice();					
					else
						newdoc = list.Find<ARInvoice.docType, ARInvoice.docDate, ARInvoice.branchID, ARInvoice.customerID, ARInvoice.customerLocationID, SOInvoice.billAddressID, SOInvoice.billContactID, SOInvoice.pMInstanceID, SOInvoice.cashAccountID, SOInvoice.extRefNbr, ARInvoice.curyID, ARInvoice.termsID, ARInvoice.hidden>(((SOOrderType)order).ARDocType, orderInvoiceDate ?? invoiceDate, soOrder.BranchID, soOrder.CustomerID, soOrder.CustomerLocationID, soOrder.BillAddressID, soOrder.BillContactID, soOrder.PMInstanceID, soOrder.CashAccountID, soOrder.ExtRefNbr, soOrder.CuryID, soOrder.TermsID, false) ?? (ARInvoice)new ARInvoice();
				}
				else
				{
					newdoc = list.Find<ARInvoice.hidden, ARInvoice.hiddenOrderType, ARInvoice.hiddenOrderNbr>(true, soOrder.OrderType, soOrder.OrderNbr);
					if (newdoc == null)
					{
						newdoc = new ARInvoice();
						newdoc.HiddenOrderType = soOrder.OrderType;
						newdoc.HiddenOrderNbr = soOrder.OrderNbr;
						newdoc.Hidden = true;
					}
				}

				if (newdoc.RefNbr != null)
				{
					Document.Current = this.Document.Search<ARInvoice.refNbr>(newdoc.RefNbr, newdoc.DocType);
				}
				else
				{
					this.Clear();

					string docType = ((SOOrderType)order).ARDocType;
					if (((SOOrderShipment)order).Operation == ((SOOrderType)order).DefaultOperation)
					{
						newdoc.DocType = docType;
					}
					else
					{
						//for RMA switch document type if previous shipment was not invoiced previously in the current run, i.e. list.Find() returned null
 						newdoc.DocType = 
							docType == ARDocType.Invoice ? ARDocType.CreditMemo :
							docType == ARDocType.DebitMemo ? ARDocType.CreditMemo :
							docType == ARDocType.CreditMemo ? ARDocType.Invoice :
							docType == ARDocType.CashSale ? ARDocType.CashReturn :
							docType == ARDocType.CashReturn ? ARDocType.CashSale :
							null;
					}

					newdoc.DocDate = orderInvoiceDate ?? invoiceDate;

					if (string.IsNullOrEmpty(soOrder.FinPeriodID) == false)
					{
						newdoc.FinPeriodID = soOrder.FinPeriodID;
					}

					if (soOrder.InvoiceNbr != null)
					{
						newdoc.RefNbr = soOrder.InvoiceNbr;
						newdoc.RefNoteID = soOrder.NoteID;
					}

					if (((SOOrderType)order).UserInvoiceNumbering == true && string.IsNullOrEmpty(newdoc.RefNbr))
					{
						throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<SOOrder.invoiceNbr>(soorder.Cache));
					}

					AutoNumberAttribute.SetNumberingId<ARInvoice.refNbr>(Document.Cache, ((SOOrderType)order).ARDocType, ((SOOrderType)order).InvoiceNumberingID);

					newdoc = (ARInvoice)Document.Cache.CreateCopy(this.Document.Insert(newdoc));

					newdoc.BranchID = soOrder.BranchID;
					newdoc.CustomerID = ((SOOrder)order).CustomerID;
					newdoc.CustomerLocationID = ((SOOrder)order).CustomerLocationID;
					newdoc.TermsID = ((SOOrder)order).TermsID;
					newdoc.DiscDate = ((SOOrder)order).DiscDate;
					newdoc.DueDate = ((SOOrder)order).DueDate;
					newdoc.TaxZoneID = ((SOOrder)order).TaxZoneID;
					newdoc.AvalaraCustomerUsageType = ((SOOrder)order).AvalaraCustomerUsageType;
					newdoc.SalesPersonID = ((SOOrder)order).SalesPersonID;
					newdoc.DocDesc = ((SOOrder)order).OrderDesc;
					newdoc.InvoiceNbr = ((SOOrder)order).CustomerOrderNbr;
					newdoc.CuryID = ((SOOrder)order).CuryID;
					newdoc.ProjectID = ((SOOrder)order).ProjectID ?? PM.ProjectDefaultAttribute.NonProject(this);
				    newdoc.Hold = ordertype.InvoiceHoldEntry;

					if (((SOOrderType)order).MarkInvoicePrinted == true)
					{
						newdoc.Printed = true;
					}

					if (((SOOrderType)order).MarkInvoiceEmailed == true)
					{
						newdoc.Emailed = true;
					}

					if (soOrder.PMInstanceID != null || string.IsNullOrEmpty(soOrder.PaymentMethodID) == false)
					{
						newdoc.PMInstanceID = soOrder.PMInstanceID;
						newdoc.PaymentMethodID = soOrder.PaymentMethodID;
						newdoc.CashAccountID = soOrder.CashAccountID;
					}

					newdoc = this.Document.Update(newdoc);


					if (soOrder.PMInstanceID != null || string.IsNullOrEmpty(soOrder.PaymentMethodID) == false)
					{
						SODocument.Current.PMInstanceID = soOrder.PMInstanceID;
						SODocument.Current.PaymentMethodID = soOrder.PaymentMethodID;
						SODocument.Current.CashAccountID = soOrder.CashAccountID;						
						if(SODocument.Current.CashAccountID == null)
							SODocument.Cache.SetDefaultExt<SOInvoice.cashAccountID>(SODocument.Current);
						SODocument.Current.ExtRefNbr = soOrder.ExtRefNbr;
						//clear error in case invoice currency different from default cash account for customer
						SODocument.Cache.RaiseExceptionHandling<SOInvoice.cashAccountID>(SODocument.Current, null, null);
					}

					foreach (CurrencyInfo info in this.currencyinfo.Select())
					{
						if (((SOOrder)order).InvoiceDate != null)
						{
							PXCache<CurrencyInfo>.RestoreCopy(info, (CurrencyInfo)order);
							info.CuryInfoID = newdoc.CuryInfoID;
						}
					}
					AddressAttribute.CopyRecord<ARInvoice.billAddressID>(this.Document.Cache, newdoc, (SOAddress)order, true);
					ContactAttribute.CopyRecord<ARInvoice.billContactID>(this.Document.Cache, newdoc, (SOContact)order, true);					
				}
			}
			else
			{
				newdoc = (ARInvoice)Document.Cache.CreateCopy(Document.Current);

                if (Transactions.SelectSingle() == null)
                {
                    newdoc.CustomerID = ((SOOrder)order).CustomerID;
                    newdoc.ProjectID = ((SOOrder)order).ProjectID;
                    newdoc.CustomerLocationID = ((SOOrder)order).CustomerLocationID;
                    newdoc.SalesPersonID = ((SOOrder)order).SalesPersonID;
                    newdoc.TaxZoneID = ((SOOrder)order).TaxZoneID;
                    newdoc.AvalaraCustomerUsageType = ((SOOrder)order).AvalaraCustomerUsageType;
                    newdoc.DocDesc = ((SOOrder)order).OrderDesc;
                    newdoc.InvoiceNbr = ((SOOrder)order).CustomerOrderNbr;
                    newdoc.TermsID = ((SOOrder)order).TermsID;

					foreach (CurrencyInfo info in this.currencyinfo.Select())
					{
						PXCache<CurrencyInfo>.RestoreCopy(info, (CurrencyInfo)order);
						info.CuryInfoID = newdoc.CuryInfoID;
						newdoc.CuryID = info.CuryID;
					}
                }

                newdoc = this.Document.Update(newdoc);

				

				AddressAttribute.CopyRecord<ARInvoice.billAddressID>(this.Document.Cache, newdoc, (SOAddress)order, true);
				ContactAttribute.CopyRecord<ARInvoice.billContactID>(this.Document.Cache, newdoc, (SOContact)order, true);								
			}

			PXSelectBase<SOInvoiceDiscountDetail> selectInvoiceDiscounts = new PXSelect<SOInvoiceDiscountDetail,
			Where<SOInvoiceDiscountDetail.tranType, Equal<Current<SOInvoice.docType>>,
			And<SOInvoiceDiscountDetail.refNbr, Equal<Current<SOInvoice.refNbr>>,
			And<SOInvoiceDiscountDetail.orderType, Equal<Required<SOInvoiceDiscountDetail.orderType>>,
			And<SOInvoiceDiscountDetail.orderNbr, Equal<Required<SOInvoiceDiscountDetail.orderNbr>>>>>>>(this);

			foreach (SOInvoiceDiscountDetail detail in selectInvoiceDiscounts.Select(((SOOrderShipment)order).OrderType, ((SOOrderShipment)order).OrderNbr))
			{
			    DiscountDetails.Delete(detail);
			}

			TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualCalc);

			if (details != null)
			{
				foreach (SOShipLine shipline in details)
				{
					this.Caches[typeof(SOShipLine)].Insert(shipline);
				}
			}

			DateTime? origInvoiceDate = null;
			foreach (PXResult<SOShipLine, SOLine, SOOrderTypeOperation, ARTran> res in PXSelectJoin<SOShipLine, InnerJoin<SOLine, On<SOLine.orderType, Equal<SOShipLine.origOrderType>, And<SOLine.orderNbr, Equal<SOShipLine.origOrderNbr>, And<SOLine.lineNbr, Equal<SOShipLine.origLineNbr>>>>, InnerJoin<SOOrderTypeOperation, On<SOOrderTypeOperation.orderType, Equal<SOLine.orderType>, And<SOOrderTypeOperation.operation, Equal<SOLine.operation>>>, LeftJoin<ARTran, On<ARTran.sOShipmentNbr, Equal<SOShipLine.shipmentNbr>, And<ARTran.sOShipmentType, Equal<SOShipLine.shipmentType>, And<ARTran.sOShipmentLineNbr, Equal<SOShipLine.lineNbr>, And<ARTran.sOOrderType, Equal<SOShipLine.origOrderType>, And<ARTran.sOOrderNbr, Equal<SOShipLine.origOrderNbr>, And<ARTran.sOOrderLineNbr, Equal<SOShipLine.origLineNbr>>>>>>>>>>, Where<SOShipLine.shipmentNbr, Equal<Required<SOShipLine.shipmentNbr>>, And<SOShipLine.origOrderType, Equal<Required<SOShipLine.origOrderType>>, And<SOShipLine.origOrderNbr, Equal<Required<SOShipLine.origOrderNbr>>, And<ARTran.refNbr, IsNull>>>>>.Select(this, ((SOOrderShipment)order).ShipmentNbr, ((SOOrderShipment)order).OrderType, ((SOOrderShipment)order).OrderNbr))
			{
				SOLine orderline = (SOLine)res;
				SOShipLine shipline = (SOShipLine)res;

                if (Math.Abs((decimal)shipline.BaseShippedQty) < 0.0000005m)
                {
                    continue;
                }

				if (origInvoiceDate == null && orderline.InvoiceDate != null)
					origInvoiceDate = orderline.InvoiceDate;

				ARTran newtran = new ARTran();
				newtran.BranchID = orderline.BranchID;
				newtran.AccountID = orderline.SalesAcctID;
				newtran.SubID = orderline.SalesSubID;
				newtran.SOOrderType = shipline.OrigOrderType;
				newtran.SOOrderNbr = shipline.OrigOrderNbr;
				newtran.SOOrderLineNbr = shipline.OrigLineNbr;
				newtran.SOShipmentNbr = shipline.ShipmentNbr;
				newtran.SOShipmentType = shipline.ShipmentType;
				newtran.SOShipmentLineNbr = shipline.LineNbr;

				newtran.LineType = orderline.LineType;
				newtran.InventoryID = shipline.InventoryID;
                newtran.SiteID = orderline.SiteID;
				newtran.UOM = shipline.UOM;

				newtran.Qty = shipline.ShippedQty;
				newtran.BaseQty = shipline.BaseShippedQty;

                newtran.Commissionable = orderline.Commissionable;
                newtran.GroupDiscountRate = orderline.GroupDiscountRate;

				decimal shippedQtyInBaseUnits = INUnitAttribute.ConvertToBase(Transactions.Cache, newtran.InventoryID, shipline.UOM, shipline.ShippedQty.Value, INPrecision.QUANTITY);
				decimal shippedQtyInOrderUnits = INUnitAttribute.ConvertFromBase(Transactions.Cache, newtran.InventoryID, orderline.UOM, shippedQtyInBaseUnits, INPrecision.QUANTITY);

				if (shippedQtyInOrderUnits != orderline.OrderQty || shipline.UOM != orderline.UOM)
				{
					decimal curyUnitPriceInBaseUnits = INUnitAttribute.ConvertFromBase(Transactions.Cache, newtran.InventoryID, orderline.UOM, orderline.CuryUnitPrice.Value, INPrecision.UNITCOST);
					decimal curyUnitPriceInShippedUnits = INUnitAttribute.ConvertToBase(Transactions.Cache, newtran.InventoryID, shipline.UOM, curyUnitPriceInBaseUnits, INPrecision.UNITCOST);

                    if (arsetup.Current.LineDiscountTarget == LineDiscountTargetType.SalesPrice)
					{
						decimal? salesPriceAfterDiscount = curyUnitPriceInShippedUnits * (1m - orderline.DiscPct / 100m);
						newtran.CuryTranAmt = shipline.ShippedQty * PXCurrencyAttribute.Round(Transactions.Cache, newtran, salesPriceAfterDiscount ?? 0, CMPrecision.TRANCURY);
					}
					else
					{
						decimal? curyTranAmt = shipline.ShippedQty * curyUnitPriceInShippedUnits * (1m - orderline.DiscPct / 100m);
						newtran.CuryTranAmt = PXCurrencyAttribute.Round(Transactions.Cache, newtran, curyTranAmt ?? 0, CMPrecision.TRANCURY);
					}

					newtran.CuryUnitPrice = curyUnitPriceInShippedUnits;
					newtran.CuryDiscAmt = (shipline.ShippedQty * curyUnitPriceInShippedUnits) - newtran.CuryTranAmt;
				}
				else
				{
					newtran.CuryUnitPrice = orderline.CuryUnitPrice;
                    newtran.CuryTranAmt = orderline.CuryLineAmt;
					newtran.CuryDiscAmt = orderline.CuryDiscAmt;
				}

				if (newdoc.DocType == ((SOOrderType)order).ARDocType && ((SOOrderType)order).DefaultOperation != ((SOOrderTypeOperation)res).Operation)
				{
					//keep BaseQty positive for PXFormula
					newtran.Qty = -newtran.Qty;
					newtran.CuryDiscAmt = -newtran.CuryDiscAmt;
					newtran.CuryTranAmt = -newtran.CuryTranAmt;
				}

				newtran.ProjectID = orderline.ProjectID;
				newtran.TaskID = orderline.TaskID;
				newtran.TranDesc = orderline.TranDesc;
				newtran.SalesPersonID = orderline.SalesPersonID;
				newtran.TaxCategoryID = orderline.TaxCategoryID;
				newtran.DiscPct = orderline.DiscPct;
				
				newtran.ManualDisc = orderline.ManualDisc == true || orderline.IsFree == true;
				newtran.FreezeManualDisc = true;

                newtran.DiscountID = orderline.DiscountID;
                newtran.DiscountSequenceID = orderline.DiscountSequenceID;

				newtran.DetDiscIDC1 = orderline.DetDiscIDC1;
				newtran.DetDiscIDC2 = orderline.DetDiscIDC2;
				newtran.DetDiscSeqIDC1 = orderline.DetDiscSeqIDC1;
				newtran.DetDiscSeqIDC2 = orderline.DetDiscSeqIDC2;
				newtran.DetDiscApp = orderline.DetDiscApp;
				newtran.DocDiscIDC1 = orderline.DocDiscIDC1;
				newtran.DocDiscIDC2 = orderline.DocDiscIDC2;
				newtran.DocDiscSeqIDC1 = orderline.DocDiscSeqIDC1;
				newtran.DocDiscSeqIDC2 = orderline.DocDiscSeqIDC2;

				foreach (ARTran existing in Transactions.Cache.Inserted)
				{
					if (Transactions.Cache.ObjectsEqual<ARTran.sOShipmentNbr, ARTran.sOShipmentType, ARTran.sOShipmentLineNbr, ARTran.sOOrderType, ARTran.sOOrderNbr, ARTran.sOOrderLineNbr>(newtran, existing))
					{
						Transactions.Cache.RestoreCopy(newtran, existing);
						break;
					}
				}

				if (newtran.LineNbr == null)
				{
					newtran = this.Transactions.Insert(newtran);

					if (((SOOrderType)order).CopyLineNotesToInvoice == true)
					{
						if (((SOOrderType)order).CopyLineNotesToInvoiceOnlyNS == false || orderline.LineType == SOLineType.NonInventory)
						{
							PXNoteAttribute.SetNote(Caches[typeof(ARTran)], newtran, PXNoteAttribute.GetNote(Caches[typeof(SOLine)], orderline));
						}
					}

					if (((SOOrderType)order).CopyLineFilesToInvoice == true)
					{
						if (((SOOrderType)order).CopyLineFilesToInvoiceOnlyNS == false || orderline.LineType == SOLineType.NonInventory)
						{
							PXNoteAttribute.SetFileNotes(Caches[typeof(ARTran)], newtran, PXNoteAttribute.GetFileNotes(Caches[typeof(SOLine)], orderline));
						}
					}
				}
				else
				{
					newtran = this.Transactions.Update(newtran);
					TaxAttribute.Calculate<ARTran.taxCategoryID>(Transactions.Cache, new PXRowUpdatedEventArgs(newtran, null, true));
				}

			}
			
			PXSelectBase<ARTran> cmd = new PXSelect<ARTran, Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>, And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>, And<ARTran.sOOrderType, Equal<Current<SOMiscLine2.orderType>>, And<ARTran.sOOrderNbr, Equal<Current<SOMiscLine2.orderNbr>>, And<ARTran.sOOrderLineNbr, Equal<Current<SOMiscLine2.lineNbr>>>>>>>>(this);

			foreach (SOMiscLine2 orderline in PXSelect<SOMiscLine2, Where<SOMiscLine2.orderType, Equal<Required<SOMiscLine2.orderType>>, And<SOMiscLine2.orderNbr, Equal<Required<SOMiscLine2.orderNbr>>, And<Where<SOMiscLine2.curyUnbilledAmt, Greater<decimal0>, Or<SOMiscLine2.curyLineAmt, LessEqual<decimal0>>>>>>>.Select(this, ((SOOrderShipment)order).OrderType, ((SOOrderShipment)order).OrderNbr))
			{
				if (cmd.View.SelectSingleBound(new object[] { Document.Current, orderline }) == null)
				{
					ARTran newtran = new ARTran();
					newtran.BranchID = orderline.BranchID;
					newtran.AccountID = orderline.SalesAcctID;
					newtran.SubID = orderline.SalesSubID;
					newtran.SOOrderType = orderline.OrderType;
					newtran.SOOrderNbr = orderline.OrderNbr;
					newtran.SOOrderLineNbr = orderline.LineNbr;
					newtran.SOShipmentNbr = ((SOOrderShipment)order).ShipmentNbr;
					newtran.SOShipmentType = ((SOOrderShipment)order).ShipmentType; 
					newtran.SOShipmentLineNbr = null;

					newtran.LineType = SOLineType.MiscCharge;
					newtran.InventoryID = orderline.InventoryID;
					newtran.TaskID = orderline.TaskID;
					newtran.SalesPersonID = orderline.SalesPersonID;
                    newtran.Commissionable = orderline.Commissionable;
					newtran.UOM = orderline.UOM;
					newtran.Qty = orderline.UnbilledQty;
					newtran.BaseQty = orderline.BaseUnbilledQty;
					newtran.CuryUnitPrice = orderline.CuryUnitPrice;
                    newtran.CuryDiscAmt = orderline.CuryDiscAmt;
					newtran.CuryTranAmt = orderline.CuryUnbilledAmt;
					newtran.TranDesc = orderline.TranDesc;
					newtran.TaxCategoryID = orderline.TaxCategoryID;
                    newtran.DiscPct = orderline.DiscPct;
					newtran.ManualDisc = orderline.ManualDisc == true || orderline.IsFree == true;
					newtran.FreezeManualDisc = true;

                    newtran.DiscountID = orderline.DiscountID;
                    newtran.DiscountSequenceID = orderline.DiscountSequenceID;

					newtran.DetDiscIDC1 = orderline.DetDiscIDC1;
					newtran.DetDiscIDC2 = orderline.DetDiscIDC2;
					newtran.DetDiscSeqIDC1 = orderline.DetDiscSeqIDC1;
					newtran.DetDiscSeqIDC2 = orderline.DetDiscSeqIDC2;
					newtran.DetDiscApp = orderline.DetDiscApp;
					newtran.DocDiscIDC1 = orderline.DocDiscIDC1;
					newtran.DocDiscIDC2 = orderline.DocDiscIDC2;
					newtran.DocDiscSeqIDC1 = orderline.DocDiscSeqIDC1;
					newtran.DocDiscSeqIDC2 = orderline.DocDiscSeqIDC2;

					newtran = this.Transactions.Insert(newtran);

					if (((SOOrderType)order).CopyLineNotesToInvoice == true)
					{
						PXNoteAttribute.SetNote(Caches[typeof(ARTran)], newtran, PXNoteAttribute.GetNote(Caches[typeof(SOMiscLine2)], orderline));
					}

					if (((SOOrderType)order).CopyLineFilesToInvoice == true)
					{
						PXNoteAttribute.SetFileNotes(Caches[typeof(ARTran)], newtran, PXNoteAttribute.GetFileNotes(Caches[typeof(SOMiscLine2)], orderline));
					}
				}
			}

			SODocument.Current = (SOInvoice)SODocument.Select() ?? (SOInvoice)SODocument.Cache.Insert();
			SODocument.Current.BillAddressID = soOrder.BillAddressID;
			SODocument.Current.BillContactID = soOrder.BillContactID;
			SODocument.Current.ShipAddressID = soOrder.ShipAddressID;
			SODocument.Current.ShipContactID = soOrder.ShipContactID;
			SODocument.Current.IsCCCaptured = soOrder.IsCCCaptured;
			SODocument.Current.IsCCCaptureFailed = soOrder.IsCCCaptureFailed;
			SODocument.Current.PaymentProjectID = PM.ProjectDefaultAttribute.NonProject(this);
			
			if (soOrder.IsCCCaptured == true)
			{
				SODocument.Current.CuryCCCapturedAmt = soOrder.CuryCCCapturedAmt;
				SODocument.Current.CCCapturedAmt = soOrder.CCCapturedAmt;
			}

			SODocument.Current.RefTranExtNbr = soOrder.RefTranExtNbr;

			SOOrderShipment shipment = PXCache<SOOrderShipment>.CreateCopy((SOOrderShipment)order);
			shipment.InvoiceType = SODocument.Current.DocType;
			shipment.InvoiceNbr = SODocument.Current.RefNbr;
			shipmentlist.Cache.Update(shipment);

			FillFreightDetails((SOOrder)order, shipment);

            /*In case Discounts were not recalculated add prorated Doc discounts */
            if (ordertype.RecalculateDiscOnPartialShipment != true)
            {
                //add prorated document discount details from invoice:
                PXSelectBase<SOOrderDiscountDetail> selectOrderDocGroupDiscounts = new PXSelect<SOOrderDiscountDetail,
                Where<SOOrderDiscountDetail.orderType, Equal<Required<SOOrderDiscountDetail.orderType>>,
                And<SOOrderDiscountDetail.orderNbr, Equal<Required<SOOrderDiscountDetail.orderNbr>>>>>(this);

                decimal? rate = 1m;
                if (soOrder.LineTotal > 0m)
                    rate = shipment.LineTotal / soOrder.LineTotal;

                foreach (SOOrderDiscountDetail docGroupDisc in selectOrderDocGroupDiscounts.Select(((SOOrderShipment)order).OrderType, ((SOOrderShipment)order).OrderNbr))
                {
                        SOInvoiceDiscountDetail dd = new SOInvoiceDiscountDetail();
                        dd.Type = docGroupDisc.Type;
                        dd.DiscountID = docGroupDisc.DiscountID;
                        dd.DiscountSequenceID = docGroupDisc.DiscountSequenceID;
                        dd.OrderType = docGroupDisc.OrderType;
                        dd.OrderNbr = docGroupDisc.OrderNbr;
                        dd.TranType = newdoc.DocType;
                        dd.RefNbr = newdoc.RefNbr;
                        dd.DiscountPct = docGroupDisc.DiscountPct;
                        dd.FreeItemID = docGroupDisc.FreeItemID;
                        dd.FreeItemQty = docGroupDisc.FreeItemQty;

                        if (docGroupDisc.Type == DiscountType.Group)
                        {
                            SOOrderEntry soOrderQ = (SOOrderEntry)PXGraph.CreateInstance(typeof(SOOrderEntry));
                            soOrderQ.Document.Current = order;

                            Dictionary<DiscountSequenceKey, DiscountEngine<SOLine>.DiscountDetailToLineCorrelation<SOOrderDiscountDetail>> grLinesOrderCorrelation = DiscountEngine<SOLine>.CollectGroupDiscountToLineCorrelation(soOrderQ.Transactions.Cache, soOrderQ.Transactions, soOrderQ.DiscountDetails, soOrder.CustomerLocationID, (DateTime)soOrder.OrderDate, false);

                            foreach (KeyValuePair<DiscountSequenceKey, DiscountEngine<SOLine>.DiscountDetailToLineCorrelation<SOOrderDiscountDetail>> dsGroup in grLinesOrderCorrelation)
                            {
                                if (dsGroup.Key.DiscountID == docGroupDisc.DiscountID && dsGroup.Key.DiscountSequenceID == docGroupDisc.DiscountSequenceID)
                                {
                                    decimal invoicedGroupAmt = 0m;
                                    foreach (SOLine soLine in dsGroup.Value.listOfApplicableLines)
                                    {
                                        foreach (ARTran tran in Transactions.Select())
                                        {
                                            if (soLine.LineNbr == tran.SOOrderLineNbr)
                                                invoicedGroupAmt += (tran.CuryLineAmt ?? 0m);
                                        }
                                    }
                                    rate = (invoicedGroupAmt / (decimal)dsGroup.Value.discountDetailLine.CuryDiscountableAmt);
                                }
                            }
                        }

                        SOInvoiceDiscountDetail located = DiscountDetails.Locate(dd);
                        if (located != null)
                        {
                            located.DiscountAmt += docGroupDisc.DiscountAmt * rate;
                            located.CuryDiscountAmt += docGroupDisc.CuryDiscountAmt * rate;
                            located.DiscountableAmt += docGroupDisc.DiscountableAmt * rate;
                            located.CuryDiscountableAmt += docGroupDisc.CuryDiscountableAmt * rate;
                            located.DiscountableQty += docGroupDisc.DiscountableQty * rate;

                            DiscountDetails.Update(located);
                        }
                        else
                        {
                            dd.DiscountAmt = docGroupDisc.DiscountAmt * rate;
                            dd.CuryDiscountAmt = docGroupDisc.CuryDiscountAmt * rate;
                            dd.DiscountableAmt = docGroupDisc.DiscountableAmt * rate;
                            dd.CuryDiscountableAmt = docGroupDisc.CuryDiscountableAmt * rate;
                            dd.DiscountableQty = docGroupDisc.DiscountableQty * rate;

                            DiscountDetails.Insert(dd);
                        }

                    }
                }
            else
            {
                //Recalculate all discounts
                foreach (ARTran tran in Transactions.Select())
                {
                    RecalculateDiscounts(this.Transactions.Cache, tran);
                }
            }
			RecalculateTotalDiscount();

			foreach (PXResult<SOTaxTran, Tax> res in PXSelectJoin<SOTaxTran,
				InnerJoin<Tax, On<SOTaxTran.taxID, Equal<Tax.taxID>>>,
				Where<SOTaxTran.orderType, Equal<Required<SOTaxTran.orderType>>, And<SOTaxTran.orderNbr, Equal<Required<SOTaxTran.orderNbr>>>>>.Select(this, ((SOOrderShipment)order).OrderType, ((SOOrderShipment)order).OrderNbr))
			{
				SOTaxTran tax = (SOTaxTran)res;
				ARTaxTran newtax = new ARTaxTran();
				newtax.Module = BatchModule.AR;
                Taxes.Cache.SetDefaultExt<ARTaxTran.origTranType>(newtax);
                Taxes.Cache.SetDefaultExt<ARTaxTran.origRefNbr>(newtax);
				Taxes.Cache.SetDefaultExt<ARTaxTran.lineRefNbr>(newtax);
				newtax.TranType = Document.Current.DocType;
				newtax.RefNbr = Document.Current.RefNbr;
				newtax.TaxID = tax.TaxID;
				newtax.TaxRate = 0m;

				this.Taxes.Delete(newtax);

				newtax = this.Taxes.Insert(newtax);
			}

			decimal? CuryApplAmt = 0m;
			bool Calculated = false;

			

			foreach (SOAdjust soadj in PXSelectJoin<SOAdjust, InnerJoin<AR.ARPayment, On<AR.ARPayment.docType, Equal<SOAdjust.adjgDocType>, And<AR.ARPayment.refNbr, Equal<SOAdjust.adjgRefNbr>>>>, Where<SOAdjust.adjdOrderType, Equal<Required<SOAdjust.adjdOrderType>>, And<SOAdjust.adjdOrderNbr, Equal<Required<SOAdjust.adjdOrderNbr>>, And<AR.ARPayment.openDoc, Equal<True>>>>>.Select(this, ((SOOrderShipment)order).OrderType, ((SOOrderShipment)order).OrderNbr))
			{
				ARAdjust prev_adj = null;
				bool found = false;

				foreach (ARAdjust adj in Adjustments.Select())
				{
					if (Calculated)
					{
						CuryApplAmt -= adj.CuryAdjdAmt;
					}

					if (string.Equals(adj.AdjgDocType, soadj.AdjgDocType) && string.Equals(adj.AdjgRefNbr, soadj.AdjgRefNbr))
					{
						if (soadj.CuryAdjdAmt > 0m)
						{
							ARAdjust copy = PXCache<ARAdjust>.CreateCopy(adj);
							copy.CuryAdjdAmt += (soadj.CuryAdjdAmt > adj.CuryDocBal) ? adj.CuryDocBal : soadj.CuryAdjdAmt;
							copy.AdjdOrderType = soadj.AdjdOrderType;
							copy.AdjdOrderNbr = soadj.AdjdOrderNbr;
							prev_adj = Adjustments.Update(copy);
						}

						found = true;

						if (Calculated)
						{
							CuryApplAmt += adj.CuryAdjdAmt;
							break;
						}
					}

					CuryApplAmt += adj.CuryAdjdAmt;
				}

				//if soadjust is not available in adjustments mark as billed
				if (!found)
				{
				/*
					soadj.Billed = true;
					soadjustments.Cache.SetStatus(soadj, PXEntryStatus.Updated);
				*/
				}

				Calculated = true;

				if (!IsExternalTax)
				{
					if (CuryApplAmt > Document.Current.CuryDocBal - Document.Current.CuryOrigDiscAmt && prev_adj != null)
					{
						prev_adj = PXCache<ARAdjust>.CreateCopy(prev_adj);

						if (prev_adj.CuryAdjdAmt > (CuryApplAmt - (Document.Current.CuryDocBal - Document.Current.CuryOrigDiscAmt)))
						{
							prev_adj.CuryAdjdAmt -= (CuryApplAmt - (Document.Current.CuryDocBal - Document.Current.CuryOrigDiscAmt));
							CuryApplAmt = Document.Current.CuryDocBal - Document.Current.CuryOrigDiscAmt;
						}
						else
						{
							CuryApplAmt -= prev_adj.CuryAdjdAmt;
							prev_adj.CuryAdjdAmt = 0m;
						}

						prev_adj = Adjustments.Update(prev_adj);
					}
				}
			}

			newdoc = (ARInvoice)Document.Cache.CreateCopy(Document.Current);
			newdoc.OrigDocDate = origInvoiceDate;
			SOInvoice socopy = (SOInvoice)SODocument.Cache.CreateCopy(SODocument.Current);

			PXFormulaAttribute.CalcAggregate<ARAdjust.curyAdjdAmt>(Adjustments.Cache, SODocument.Current, false);
			Document.Cache.RaiseFieldUpdated<SOInvoice.curyPaymentTotal>(SODocument.Current, null);
			PXDBCurrencyAttribute.CalcBaseValues<SOInvoice.curyPaymentTotal>(SODocument.Cache, SODocument.Current);

			SODocument.Cache.RaiseRowUpdated(SODocument.Current, socopy);

			List<string> ordersdistinct = new List<string>();
			foreach (SOOrderShipment shipments in PXSelect<SOOrderShipment, Where<SOOrderShipment.invoiceType, Equal<Current<ARInvoice.docType>>, And<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>>>>.Select(this))
			{
				string key = string.Format("{0}|{1}", shipments.OrderType, shipments.OrderNbr);
				if (!ordersdistinct.Contains(key))
				{
					ordersdistinct.Add(key);
				}

				if (ordersdistinct.Count > 1)
				{
					newdoc.InvoiceNbr = null;
					newdoc.SalesPersonID = null;
					newdoc.DocDesc = null;
					break;
				}

				#region Update FreeItemQty for DiscountDetails based on shipments
				
				PXSelectBase<SOShipmentDiscountDetail> selectShipmentDiscounts = new PXSelect<SOShipmentDiscountDetail,
						Where<SOShipmentDiscountDetail.orderType, Equal<Required<SOShipmentDiscountDetail.orderType>>,
						And<SOShipmentDiscountDetail.orderNbr, Equal<Required<SOShipmentDiscountDetail.orderNbr>>,
						And<SOShipmentDiscountDetail.shipmentNbr, Equal<Required<SOShipmentDiscountDetail.shipmentNbr>>>>>>(this);

				foreach (SOShipmentDiscountDetail sdd in selectShipmentDiscounts.Select(shipments.OrderType, shipments.OrderNbr, shipments.ShipmentNbr))
				{
					SOInvoiceDiscountDetail idd = PXSelect<SOInvoiceDiscountDetail,
						Where<SOInvoiceDiscountDetail.tranType, Equal<Current<ARInvoice.docType>>,
						And<SOInvoiceDiscountDetail.refNbr, Equal<Current<ARInvoice.refNbr>>,
						And<SOInvoiceDiscountDetail.orderType, Equal<Required<SOInvoiceDiscountDetail.orderType>>,
						And<SOInvoiceDiscountDetail.orderNbr, Equal<Required<SOInvoiceDiscountDetail.orderNbr>>,
						And<SOInvoiceDiscountDetail.discountID, Equal<Required<SOInvoiceDiscountDetail.discountID>>,
						And<SOInvoiceDiscountDetail.discountSequenceID, Equal<Required<SOInvoiceDiscountDetail.discountSequenceID>>>>>>>>>.Select(this, shipments.OrderType, shipments.OrderNbr, sdd.DiscountID, sdd.DiscountSequenceID);

					if (idd != null)
					{
						if (idd.FreeItemID == null)
						{
							idd.FreeItemID = sdd.FreeItemID;
							idd.FreeItemQty = sdd.FreeItemQty;
						}
						else
							idd.FreeItemQty = sdd.FreeItemQty;

						DiscountDetails.Update(idd);
					}
					else
					{
						idd = new SOInvoiceDiscountDetail();
						idd.Type = DiscountType.Line;
						idd.TranType = newdoc.DocType;
						idd.RefNbr = newdoc.RefNbr;
						idd.OrderType = sdd.OrderType;
						idd.OrderNbr = sdd.OrderNbr;
						idd.DiscountID = sdd.DiscountID;
						idd.DiscountSequenceID = sdd.DiscountSequenceID;
						idd.FreeItemID = sdd.FreeItemID;
						idd.FreeItemQty = sdd.FreeItemQty;

						DiscountDetails.Insert(idd);
					}
				} 

				#endregion
			}

            this.Document.Update(newdoc);

			if (list != null)
			{
				if (Transactions.Search<ARTran.sOOrderType, ARTran.sOOrderNbr, ARTran.sOShipmentType, ARTran.sOShipmentNbr>(shipment.OrderType, shipment.OrderNbr, shipment.ShipmentType, shipment.ShipmentNbr).Count > 0)
				{
					try
					{
						this.Document.Current.ApplyPaymentWhenTaxAvailable = true;
						this.Save.Press();
					}
					finally
					{
						this.Document.Current.ApplyPaymentWhenTaxAvailable = false;
					}
					

					if (list.Find(this.Document.Current) == null)
					{
						list.Add(this.Document.Current, this.SODocument.Current);
					}
				}
				else
				{
					this.Clear();
				}
			}
            this.RowUpdated.RemoveHandler(typeof(ARInvoice), ApprovedBalanceCollector);
		}

		public virtual void PostInvoice(INIssueEntry docgraph, ARInvoice invoice, DocumentList<INRegister> list)
		{
			SOOrderEntry oe = null;
			SOShipmentEntry se = null;

			foreach (PXResult<SOOrderShipment, SOOrder> res in PXSelectJoin<SOOrderShipment, InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOOrderShipment.orderType>, And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>>, Where<SOOrderShipment.invoiceType, Equal<Current<ARInvoice.docType>>, And<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>>>>.SelectMultiBound(this, new object[] { invoice }))
			{
				if (((SOOrderShipment)res).ShipmentType == SOShipmentType.DropShip)
				{
					if (se == null)
					{
						se = PXGraph.CreateInstance<SOShipmentEntry>();
					}
					else
					{
						se.Clear();
					}
					se.PostReceipt(docgraph, res, list);
				}
				else if (string.Equals(((SOOrderShipment)res).ShipmentNbr, Constants.NoShipmentNbr))
				{
					if (oe == null)
					{
						oe = PXGraph.CreateInstance<SOOrderEntry>();
					}
					else
					{
						oe.Clear();
					}
					oe.PostOrder(docgraph, (SOOrder)res, list);
				}
				else
				{
					if (se == null)
					{
						se = PXGraph.CreateInstance<SOShipmentEntry>();

						se.Caches[typeof(SiteStatus)] = docgraph.Caches[typeof(SiteStatus)];
						se.Caches[typeof(LocationStatus)] = docgraph.Caches[typeof(LocationStatus)];
						se.Caches[typeof(LotSerialStatus)] = docgraph.Caches[typeof(LotSerialStatus)];
						se.Caches[typeof(ItemLotSerial)] = docgraph.Caches[typeof(ItemLotSerial)];

						se.Views.Caches.Remove(typeof(SiteStatus));
						se.Views.Caches.Remove(typeof(LocationStatus));
						se.Views.Caches.Remove(typeof(LotSerialStatus));
						se.Views.Caches.Remove(typeof(ItemLotSerial));
					}
					else
					{
						se.Clear();
					}
					se.PostShipment(docgraph, res, list);
				}
			}		
		}

		private void DefaultDiscountAccountAndSubAccount(ARTran tran)
		{
			ARTran firstTranWithOrderType = PXSelect<ARTran, Where<ARTran.tranType, Equal<Current<SOInvoice.docType>>,
				And<ARTran.refNbr, Equal<Current<SOInvoice.refNbr>>,
				And<ARTran.sOOrderType, IsNotNull>>>>.Select(this);

			if (firstTranWithOrderType != null)
			{
				SOOrderType type = PXSelect<SOOrderType, Where<SOOrderType.orderType, Equal<Required<SOOrderType.orderType>>>>.Select(this, firstTranWithOrderType.SOOrderType);

				if (type != null)
				{
					Location customerloc = location.Current;
					Location companyloc =
						(Location)PXSelectJoin<Location, InnerJoin<BAccountR, On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>, InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>>,Where<Branch.branchID, Equal<Current<ARRegister.branchID>>>>.Select(this);

					switch (type.DiscAcctDefault)
					{
						case SODiscAcctSubDefault.OrderType:
							tran.AccountID = (int?)GetValue<SOOrderType.discountAcctID>(type);
							break;
						case SODiscAcctSubDefault.MaskLocation:
							tran.AccountID = (int?)GetValue<Location.cDiscountAcctID>(customerloc);
							break;
					}


					if (tran.AccountID == null)
					{
						tran.AccountID = type.DiscountAcctID;
					}

					Discount.Cache.RaiseFieldUpdated<ARTran.accountID>(tran, null);

					if (tran.AccountID != null)
					{
						object ordertype_SubID = GetValue<SOOrderType.discountSubID>(type);
						object customer_Location = GetValue<Location.cDiscountSubID>(customerloc);
						object company_Location = GetValue<Location.cMPDiscountSubID>(companyloc);

						object value = SODiscSubAccountMaskAttribute.MakeSub<SOOrderType.discSubMask>(this, type.DiscSubMask,
								new object[] { ordertype_SubID, customer_Location, company_Location },
								new Type[] { typeof(SOOrderType.discountSubID), typeof(Location.cDiscountSubID), typeof(Location.cMPDiscountSubID) });

						Discount.Cache.RaiseFieldUpdating<ARTran.subID>(tran, ref value);

						tran.SubID = (int?)value;
					}
				}
			}

		}

		#region Freight
		private void FillFreightDetails(SOOrder order, SOOrderShipment ordershipment)
		{
            SOFreightDetail freightdet;

            if (ordershipment.ShipmentType == SOShipmentType.DropShip)
			{
                freightdet = PXSelect<SOFreightDetail, Where<SOFreightDetail.docType, Equal<Current<ARInvoice.docType>>, And<SOFreightDetail.refNbr, Equal<Current<SOFreightDetail.refNbr>>, And<SOFreightDetail.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>>>>>.SelectSingleBound(this, new object[] { ordershipment });

                if (freightdet == null)
                {
                    freightdet = new SOFreightDetail();
                    freightdet.CuryInfoID = Document.Current.CuryInfoID;
                    freightdet.ShipmentNbr = ordershipment.ShipmentNbr;
                    freightdet.ShipmentType = ordershipment.ShipmentType;
                    freightdet.OrderType = ordershipment.OrderType;
                    freightdet.OrderNbr = ordershipment.OrderNbr;
                    freightdet.ProjectID = order.ProjectID;

                    int? accountID;
                    object subID;
                    GetFreightAccountAndSubAccount(order, order.ShipVia, out accountID, out subID);
                    freightdet.AccountID = accountID;
                    FreightDetails.Cache.RaiseFieldUpdating<SOFreightDetail.subID>(freightdet, ref subID);
                    freightdet.SubID = (int?)subID;

                    freightdet = FreightDetails.Insert(freightdet);
                }

                freightdet = PXCache<SOFreightDetail>.CreateCopy(freightdet);

                freightdet.ShipTermsID = order.ShipTermsID; //!!
                freightdet.ShipVia = order.ShipVia; //!!
                freightdet.ShipZoneID = order.ShipZoneID; //!!
                freightdet.TaxCategoryID = order.FreightTaxCategoryID;

                freightdet.Weight = 0m;
                freightdet.Volume = 0m;
                freightdet.CuryLineTotal = 0m;
                freightdet.CuryFreightCost = 0m;
                freightdet.CuryFreightAmt = 0m;
                freightdet.CuryPremiumFreightAmt = 0m;

                decimal dropShipLineAmt = 0m;
                foreach (PXResult<SOLine, PO.POLine, POReceiptLine> res in PXSelectJoin<SOLine, 
                    InnerJoin<PO.POLine, On<PO.POLine.lineNbr, Equal<SOLine.pOLineNbr>, And<PO.POLine.lineNbr, Equal<SOLine.pOLineNbr>>>, 
                    InnerJoin<POReceiptLine, On<POReceiptLine.pOLineNbr, Equal<PO.POLine.lineNbr>, And<POReceiptLine.pONbr, Equal<PO.POLine.orderNbr>>>>>, 
                    Where<POReceiptLine.receiptNbr, Equal<Required<POReceiptLine.receiptNbr>>, And<SOLine.orderType, 
                    Equal<Required<SOLine.orderType>>, And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>>>>>.Select(this, ordershipment.ShipmentNbr, ordershipment.OrderType, ordershipment.OrderNbr))
                {
                    SOLine line2 = (SOLine)res;
                    dropShipLineAmt += line2.LineAmt ?? 0m;
                }

                foreach (PXResult<SOOrderShipment, SOOrder> res in PXSelectJoin<SOOrderShipment, InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOOrderShipment.orderType>, And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>>, Where<SOOrderShipment.invoiceType, Equal<Current<ARInvoice.docType>>, And<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>>>>.SelectMultiBound(this, new object[] { ordershipment }))
                {
                    SOOrder order2 = (SOOrder)res;
                    freightdet.CuryLineTotal += dropShipLineAmt;
                    decimal? ShipRatio = order2.CuryLineTotal > 0m ? dropShipLineAmt / order2.LineTotal : 1m;

                    freightdet.CuryPremiumFreightAmt += PXDBCurrencyAttribute.Round(FreightDetails.Cache, freightdet, (decimal)(order2.CuryPremiumFreightAmt * (ShipRatio > 1m ? 1m : ShipRatio) ?? 0m), CMPrecision.TRANCURY);
                    freightdet.CuryFreightAmt += PXDBCurrencyAttribute.Round(FreightDetails.Cache, freightdet, (decimal)(order2.CuryFreightAmt * (ShipRatio > 1m ? 1m : ShipRatio) ?? 0m), CMPrecision.TRANCURY);
                }

                freightdet.CuryTotalFreightAmt = freightdet.CuryPremiumFreightAmt + freightdet.CuryFreightAmt;
                FreightDetails.Update(freightdet);

                if (freightdet.CuryTotalFreightAmt > 0 && freightdet.TaskID == null && !PM.ProjectDefaultAttribute.IsNonProject(this, freightdet.ProjectID))
                {
                    Account ac = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, freightdet.AccountID);
                    throw new PXException(Messages.TaskWasNotAssigned, ac.AccountCD);
                }
                return;
			}
			
			if (string.Equals(ordershipment.ShipmentNbr, Constants.NoShipmentNbr))
			{
				freightdet = PXSelect<SOFreightDetail, Where<SOFreightDetail.docType, Equal<Current<ARInvoice.docType>>, And<SOFreightDetail.refNbr, Equal<Current<SOFreightDetail.refNbr>>, And<SOFreightDetail.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>>>>>.SelectSingleBound(this, new object[] { ordershipment });

				if (freightdet == null)
				{
					freightdet = new SOFreightDetail();
					freightdet.CuryInfoID = Document.Current.CuryInfoID;
					freightdet.ShipmentNbr = ordershipment.ShipmentNbr;
					freightdet.ShipmentType = ordershipment.ShipmentType;
                    freightdet.OrderType = ordershipment.OrderType;
                    freightdet.OrderNbr = ordershipment.OrderNbr;
					freightdet.ProjectID = order.ProjectID;

					int? accountID;
					object subID;
					GetFreightAccountAndSubAccount(order, order.ShipVia, out accountID, out subID);
					freightdet.AccountID = accountID;
					FreightDetails.Cache.RaiseFieldUpdating<SOFreightDetail.subID>(freightdet, ref subID);
					freightdet.SubID = (int?)subID;

					freightdet = FreightDetails.Insert(freightdet);
				}

				freightdet = PXCache<SOFreightDetail>.CreateCopy(freightdet);

				freightdet.ShipTermsID = order.ShipTermsID; //!!
				freightdet.ShipVia = order.ShipVia; //!!
				freightdet.ShipZoneID = order.ShipZoneID; //!!
				freightdet.TaxCategoryID = order.FreightTaxCategoryID;

				freightdet.Weight = 0m;
				freightdet.Volume = 0m;
				freightdet.CuryLineTotal = 0m;
				freightdet.CuryFreightCost = 0m;
				freightdet.CuryFreightAmt = 0m;
				freightdet.CuryPremiumFreightAmt = 0m;

				foreach (PXResult<SOOrderShipment, SOOrder> res in PXSelectJoin<SOOrderShipment, InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOOrderShipment.orderType>, And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>>, Where<SOOrderShipment.invoiceType, Equal<Current<ARInvoice.docType>>, And<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>, And<SOOrderShipment.shipmentNbr, Equal<Constants.noShipmentNbr>>>>>.SelectMultiBound(this, new object[] { ordershipment }))
				{
					SOOrder order2 = (SOOrder)res;
					freightdet.Weight += order2.OrderWeight;
					freightdet.Volume += order2.OrderVolume;
					freightdet.CuryLineTotal += order2.CuryLineTotal;
					freightdet.CuryFreightCost += order2.CuryFreightCost;
					freightdet.CuryFreightAmt += order2.CuryFreightAmt;
					freightdet.CuryPremiumFreightAmt += order2.CuryPremiumFreightAmt;
				}

				freightdet.CuryTotalFreightAmt = freightdet.CuryPremiumFreightAmt + freightdet.CuryFreightAmt;
				FreightDetails.Update(freightdet);

				if (freightdet.CuryTotalFreightAmt > 0 && freightdet.TaskID == null && !PM.ProjectDefaultAttribute.IsNonProject(this, freightdet.ProjectID))
				{
					Account ac = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, freightdet.AccountID);
					throw new PXException(Messages.TaskWasNotAssigned, ac.AccountCD);
				}
			}
			else
			{
				freightdet = PXSelect<SOFreightDetail, Where<SOFreightDetail.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>>>.SelectSingleBound(this, new object[] { ordershipment });

				if (freightdet == null)
				{
					SOShipment shipment = PXSelect<SOShipment, Where<SOShipment.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>>>.Select(this, new object[] { ordershipment });

					if (shipment != null)
					{
						freightdet = new SOFreightDetail();
						freightdet.CuryInfoID = Document.Current.CuryInfoID;
						freightdet.ShipmentNbr = ordershipment.ShipmentNbr;
						freightdet.ShipmentType = ordershipment.ShipmentType;
                        if ((PXSelect<SOOrderShipment, Where<SOOrderShipment.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>>>.Select(this)).Count == 1)
                        {
                            freightdet.OrderType = ordershipment.OrderType;
                            freightdet.OrderNbr = ordershipment.OrderNbr;
                        }
						freightdet.ProjectID = order.ProjectID;

						freightdet.ShipTermsID = shipment.ShipTermsID;
						freightdet.ShipVia = shipment.ShipVia;
						freightdet.ShipZoneID = shipment.ShipZoneID;
						freightdet.TaxCategoryID = shipment.TaxCategoryID;
						freightdet.Weight = shipment.ShipmentWeight;
						freightdet.Volume = shipment.ShipmentVolume;
						freightdet.LineTotal = shipment.LineTotal;
						PXCurrencyAttribute.CuryConvCury<SOFreightDetail.curyLineTotal>(FreightDetails.Cache, freightdet);
						freightdet.FreightCost = shipment.FreightCost;
						PXCurrencyAttribute.CuryConvCury<SOFreightDetail.curyFreightCost>(FreightDetails.Cache, freightdet);
						freightdet.FreightAmt = shipment.FreightAmt;
						PXCurrencyAttribute.CuryConvCury<SOFreightDetail.curyFreightAmt>(FreightDetails.Cache, freightdet);

						freightdet.CuryPremiumFreightAmt = 0m;
						//recalculate All Premium Freight for Shipment
						foreach (PXResult<SOOrderShipment, SOOrder> res in PXSelectJoin<SOOrderShipment, InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOOrderShipment.orderType>, And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>>, Where<SOOrderShipment.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>, And<SOOrderShipment.shipmentType, NotEqual<SOShipmentType.dropShip>>>>.SelectMultiBound(this, new object[] { ordershipment }))
						{
							SOOrderShipment ordershipment2 = (SOOrderShipment)res;
							SOOrder order2 = (SOOrder)res;

							if (sosetup.Current.FreightAllocation == FreightAllocationList.Prorate)
							{
								if (order2.CuryLineTotal > 0m)
								{
									decimal? ShipRatio = ordershipment2.LineTotal / order2.LineTotal;
									freightdet.CuryPremiumFreightAmt += PXDBCurrencyAttribute.Round(FreightDetails.Cache, freightdet, (decimal)(order2.CuryPremiumFreightAmt * (ShipRatio > 1m ? 1m : ShipRatio) ?? 0m), CMPrecision.TRANCURY);
								}
								else
								{
									freightdet.CuryPremiumFreightAmt += order2.CuryPremiumFreightAmt;
								}
							}
							else
							{
								SOOrderShipment prev_shipment = PXSelectReadonly<SOOrderShipment,
									Where<SOOrderShipment.shipmentNbr, NotEqual<Current<SOOrderShipment.shipmentNbr>>, And<SOOrderShipment.shipmentType, NotEqual<SOShipmentType.dropShip>,
									And<SOOrderShipment.orderType, Equal<Current<SOOrderShipment.orderType>>,
									And<SOOrderShipment.orderNbr, Equal<Current<SOOrderShipment.orderNbr>>,
									And<SOOrderShipment.invoiceNbr, IsNotNull>>>>>>.SelectSingleBound(this, new object[] { ordershipment2 });
								if (prev_shipment == null)
								{
									freightdet.CuryPremiumFreightAmt += order2.CuryPremiumFreightAmt;
								}
								else
								{
									freightdet.CuryPremiumFreightAmt = 0m;
								}
							}
						}

						freightdet.CuryTotalFreightAmt = freightdet.CuryPremiumFreightAmt + freightdet.CuryFreightAmt;

						int? accountID;
						object subID;
						GetFreightAccountAndSubAccount(order, shipment.ShipVia, out accountID, out subID);
						freightdet.AccountID = accountID;
						FreightDetails.Cache.RaiseFieldUpdating<SOFreightDetail.subID>(freightdet, ref subID);
						freightdet.SubID = (int?)subID;

						freightdet = FreightDetails.Insert(freightdet);
					}
				}
			}
		}

		private void AddFreightTransaction(SOFreightDetail fd)
		{
			ARTran freight = new ARTran();
			freight.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.FreightDescr, fd.ShipVia);
			freight.SOShipmentNbr = fd.ShipmentNbr;
			freight.SOShipmentType = fd.ShipmentType ?? SOShipmentType.Issue;
            freight.SOOrderType = fd.OrderType;
            freight.SOOrderNbr = fd.OrderNbr;
			freight.LineType = SOLineType.Freight;
			freight.CuryTranAmt = fd.CuryTotalFreightAmt;
			freight.TaxCategoryID = fd.TaxCategoryID;
			freight.AccountID = fd.AccountID;
			freight.SubID = fd.SubID;
			freight.ProjectID = fd.ProjectID;

			if (fd.TaskID != null)
				freight.TaskID = fd.TaskID;
			
			freight = (ARTran)Freight.Cache.Insert(freight);

			if (freight.TaskID == null && !PM.ProjectDefaultAttribute.IsNonProject(this, freight.ProjectID))
			{
				Account ac = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, freight.AccountID);
				throw new PXException(Messages.TaskWasNotAssigned, ac.AccountCD);
			}
			
		}

		protected virtual void CopyFreightNotesAndFilesToARTran()
		{
			foreach (SOFreightDetail fd in FreightDetails.Select())
			{
				foreach (ARTran tran in PXSelect<ARTran,
				Where<ARTran.lineType, Equal<SOLineType.freight>,
				And<ARTran.tranType, Equal<Current<ARInvoice.docType>>,
				And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>,
				And<ARTran.sOShipmentNbr, Equal<Required<ARTran.sOShipmentNbr>>,
				And<ARTran.sOShipmentType, NotEqual<SOShipmentType.dropShip>>>>>>>.Select(this, fd.ShipmentNbr))
				{
					string note = PXNoteAttribute.GetNote(FreightDetails.Cache, fd);
					if (note != null)
						PXNoteAttribute.SetNote(Freight.Cache, tran, note);

					Guid[] files = PXNoteAttribute.GetFileNotes(FreightDetails.Cache, fd);
					if (files != null && files.Length > 0)
						PXNoteAttribute.SetFileNotes(Freight.Cache, tran, files);
				}
			}
		}


		private void GetFreightAccountAndSubAccount(SOOrder order, string ShipVia, out int? accountID, out object subID)
		{
			accountID = null;
			subID = null;
			SOOrderType type = PXSelect<SOOrderType, Where<SOOrderType.orderType, Equal<Required<SOOrderType.orderType>>>>.Select(this, order.OrderType);

			if (type != null)
			{
				Location customerloc = location.Current;
				Carrier carrier = PXSelect<Carrier, Where<Carrier.carrierID, Equal<Required<Carrier.carrierID>>>>.Select(this, ShipVia);

                Location companyloc =
                        (Location)PXSelectJoin<Location, InnerJoin<BAccountR, On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>, InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>>, Where<Branch.branchID, Equal<Current<ARRegister.branchID>>>>.Select(this);

				switch (type.FreightAcctDefault)
				{
					case SOFreightAcctSubDefault.OrderType:
						accountID = (int?)GetValue<SOOrderType.freightAcctID>(type);
						break;
					case SOFreightAcctSubDefault.MaskLocation:
						accountID = (int?)GetValue<Location.cFreightAcctID>(customerloc);
						break;
					case SOFreightAcctSubDefault.MaskShipVia:
						accountID = (int?)GetValue<Carrier.freightSalesAcctID>(carrier);
						break;
				}

				if (accountID == null)
				{
					accountID = type.FreightAcctID;

					if (accountID == null)
					{
						throw new PXException(Messages.FreightAccountIsRequired);
					}

				}

				if (accountID != null)
				{
					object orderType_SubID = GetValue<SOOrderType.freightSubID>(type);
					object customer_Location_SubID = GetValue<Location.cFreightSubID>(customerloc);
					object carrier_SubID = GetValue<Carrier.freightSalesSubID>(carrier);
                    object branch_SubID = GetValue<Location.cMPFreightSubID>(companyloc);

					subID = SOFreightSubAccountMaskAttribute.MakeSub<SOOrderType.freightSubMask>(this, type.FreightSubMask,
						new object[] { orderType_SubID, customer_Location_SubID, carrier_SubID, branch_SubID },
						new Type[] { typeof(SOOrderType.freightSubID), typeof(Location.cFreightSubID), typeof(Carrier.freightSalesSubID), typeof(Location.cMPFreightSubID) });

				}
			}
		}
		#endregion

		#region Discount

        protected override void RecalculateDiscounts(PXCache sender, ARTran line)
        {
            if (line.InventoryID != null && line.Qty != null && line.CuryLineAmt != null && line.IsFree != true)
            {
                DateTime? docDate = Document.Current.DocDate;
                int? customerLocationID = Document.Current.CustomerLocationID;

                SOLine soline = PXSelect<SOLine, Where<SOLine.orderType, Equal<Required<SOLine.orderType>>,
                        And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
                        And<SOLine.lineNbr, Equal<Required<SOLine.lineNbr>>>>>>.Select(this, line.SOOrderType, line.SOOrderNbr, line.SOOrderLineNbr);
                if (soline != null)
                {
                    docDate = soline.OrderDate;
                }

                DiscountEngine<ARTran>.SetDiscounts<SOInvoiceDiscountDetail>(sender, Transactions, line, DiscountDetails, customerLocationID, Document.Current.CuryID, docDate.Value, Document.Current.SkipDiscounts, false, recalcdiscountsfilter.Current);

                foreach (SOInvoiceDiscountDetail discountDetail in DiscountDetails.Select())
                {
                    if (discountDetail.OrderType == null)
                        discountDetail.OrderType = line.SOOrderType ?? Messages.NoOrder;
                    if (discountDetail.OrderNbr == null)
                        discountDetail.OrderNbr = line.SOOrderNbr ?? Messages.NoOrderType;
                }
                RecalculateTotalDiscount();
            }
        }

		protected virtual void RecalculateTotalDiscount()
		{
			if (Document.Current != null && SODocument.Current != null)
			{
				decimal total = GetAutoDocDiscount();
				SOInvoice old_row = PXCache<SOInvoice>.CreateCopy(SODocument.Current);

				SODocument.Cache.SetValueExt<SOInvoice.curyDiscTot>(SODocument.Current, total);
				SODocument.Cache.RaiseRowUpdated(SODocument.Current, old_row);
			}
		}

		protected decimal GetAutoDocDiscount()
		{
			decimal total = 0;
			foreach (SOInvoiceDiscountDetail record in PXSelect<SOInvoiceDiscountDetail,
					Where<SOInvoiceDiscountDetail.tranType, Equal<Current<ARInvoice.docType>>,
					And<SOInvoiceDiscountDetail.refNbr, Equal<Current<ARInvoice.refNbr>>,
					And<SOInvoiceDiscountDetail.type, NotEqual<DiscountType.LineDiscount>>>>>.Select(this))
			{
				total += record.CuryDiscountAmt ?? 0;
			}

			return total;
		}

		private bool ProrateDiscount
		{
			get
			{
				SOSetup sosetup = PXSelect<SOSetup>.Select(this);

				if (sosetup == null)
				{
					return true;//default true
				}
				else
				{
					if (sosetup.ProrateDiscounts == null)
						return true;
					else
						return sosetup.ProrateDiscounts == true;
				}

			}
		}
		#endregion

		public static void UpdateSOInvoiceState(IBqlTable aDoc, PX.CCProcessing.CCTranType aLastOperation)
		{
			SOInvoiceEntry graph = PXGraph.CreateInstance<SOInvoiceEntry>();
			graph.UpdateDocState(aDoc as SOInvoice, aLastOperation);
		}

        public virtual void UpdateDocState(SOInvoice doc, PX.CCProcessing.CCTranType lastOperation)
        {
            this.Document.Current = Document.Search<ARInvoice.refNbr>(doc.RefNbr, doc.DocType);

            bool needUpdate = CCPaymentEntry.UpdateCapturedState<SOInvoice>(doc, this.ccProcTran.Select());

            if (needUpdate)
            {
                //doc.PreAuthTranNumber = null;
                doc = this.SODocument.Update(doc);
                Document.Search<ARInvoice.refNbr>(doc.RefNbr, doc.DocType);
				if (doc.IsCCCaptured == true) 
				{
					foreach (CCProcTran tran in this.ccProcTran.Select()) 
					{
						if (String.IsNullOrEmpty(tran.RefNbr) || String.IsNullOrEmpty(tran.DocType)) 
						{
							tran.DocType = doc.DocType;
							tran.RefNbr = doc.RefNbr;
							this.ccProcTran.Update(tran);
						}
					}
				}
                this.Save.Press();
            }
        }

		internal sealed class DummyView : PXView
		{
			List<object> _Records;
			internal DummyView(PXGraph graph, BqlCommand command, List<object> records)
				: base(graph, true, command)
			{
				_Records = records;
			}
			public override List<object> Select(object[] currents, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
			{
				return _Records;
			}
		}

		#region Avalara Tax

		protected override void ApplyAvalaraTax(ARInvoice invoice, GetTaxResult result)
		{
			TaxZone taxZone = (TaxZone)taxzone.View.SelectSingleBound(new object[] { invoice });
			AP.Vendor vendor = PXSelect<AP.Vendor, Where<AP.Vendor.bAccountID, Equal<Required<AP.Vendor.bAccountID>>>>.Select(this, taxZone.TaxVendorID);

			if (vendor == null)
				throw new PXException("Tax Vendor is required but not found for the External TaxZone.");

			Dictionary<string, ARTaxTran> existingRows = new Dictionary<string, ARTaxTran>();
			foreach (PXResult<ARTaxTran, Tax> res in Taxes.View.SelectMultiBound(new object[] { invoice }))
			{
				ARTaxTran taxTran = (ARTaxTran)res;
				existingRows.Add(taxTran.TaxID.Trim().ToUpperInvariant(), taxTran);
			}

			this.Views.Caches.Add(typeof(Tax));

			bool requireControlTotal = ARSetup.Current.RequireControlTotal == true;

			if (invoice.Hold != true)
				ARSetup.Current.RequireControlTotal = false;

			try
			{

				for (int i = 0; i < result.TaxSummary.Count; i++)
				{
					string taxID = result.TaxSummary[i].TaxName.ToUpperInvariant();

					//Insert Tax if not exists - just for the selectors sake
					Tax tx = PXSelect<Tax, Where<Tax.taxID, Equal<Required<Tax.taxID>>>>.Select(this, taxID);
					if (tx == null)
					{
						tx = new Tax();
						tx.TaxID = taxID;
						//tx.Descr = string.Format("Avalara {0} {1}%", taxID, Convert.ToDecimal(result.TaxSummary[i].Rate)*100);
						tx.Descr = string.Format("Avalara {0}", taxID);
						tx.TaxType = CSTaxType.Sales;
						tx.TaxCalcType = CSTaxCalcType.Doc;
						tx.TaxCalcLevel = CSTaxCalcLevel.CalcOnItemAmt;
						tx.TaxApplyTermsDisc = CSTaxTermsDiscount.ToTaxableAmount;
						tx.SalesTaxAcctID = vendor.SalesTaxAcctID;
						tx.SalesTaxSubID = vendor.SalesTaxSubID;
						tx.ExpenseAccountID = vendor.TaxExpenseAcctID;
						tx.ExpenseSubID = vendor.TaxExpenseSubID;
						tx.TaxVendorID = taxZone.TaxVendorID;

						this.Caches[typeof(Tax)].Insert(tx);
					}

					ARTaxTran existing = null;
					existingRows.TryGetValue(taxID, out existing);

					if (existing != null)
					{
						existing.TaxAmt = Math.Abs(result.TaxSummary[i].Tax);
						existing.CuryTaxAmt = Math.Abs(result.TaxSummary[i].Tax);
						existing.TaxableAmt = Math.Abs(result.TaxSummary[i].Taxable);
						existing.CuryTaxableAmt = Math.Abs(result.TaxSummary[i].Taxable);
						existing.TaxRate = Convert.ToDecimal(result.TaxSummary[i].Rate);

						Taxes.Update(existing);
						existingRows.Remove(existing.TaxID.Trim().ToUpperInvariant());
					}
					else
					{
						ARTaxTran tax = new ARTaxTran();
						tax.Module = BatchModule.AR;
						tax.TranType = invoice.DocType;
						tax.RefNbr = invoice.RefNbr;
						tax.TaxID = taxID;
						tax.TaxAmt = Math.Abs(result.TaxSummary[i].Tax);
						tax.CuryTaxAmt = Math.Abs(result.TaxSummary[i].Tax);
						tax.TaxableAmt = Math.Abs(result.TaxSummary[i].Taxable);
						tax.CuryTaxableAmt = Math.Abs(result.TaxSummary[i].Taxable);
						tax.TaxRate = Convert.ToDecimal(result.TaxSummary[i].Rate);
						tax.TaxType = "S";
						tax.TaxBucketID = 0;
						tax.AccountID = vendor.SalesTaxAcctID;
						tax.SubID = vendor.SalesTaxSubID;

						Taxes.Insert(tax);
					}
				}

				foreach (ARTaxTran taxTran in existingRows.Values)
				{
					Taxes.Delete(taxTran);
				}

				SOInvoice soInvoice = PXSelect<SOInvoice, Where<SOInvoice.docType, Equal<Required<SOInvoice.docType>>, And<SOInvoice.refNbr, Equal<Required<SOInvoice.refNbr>>>>>.Select(this, invoice.DocType, invoice.RefNbr);

				invoice.CuryTaxTotal = Math.Abs(result.TotalTax);
				Document.Cache.SetValueExt<ARInvoice.isTaxSaved>(invoice, true);
			}
			finally
			{
				ARSetup.Current.RequireControlTotal = requireControlTotal;
			}


			if (invoice.ApplyPaymentWhenTaxAvailable == true)
			{
				PXSelectBase<ARAdjust> select = new PXSelectJoin<ARAdjust,
					InnerJoin<ARPayment, On<ARAdjust.adjgDocType, Equal<ARPayment.docType>, And<ARAdjust.adjgRefNbr, Equal<ARPayment.refNbr>>>>,
					Where<ARAdjust.adjdDocType, Equal<Required<ARInvoice.docType>>,
					And<ARAdjust.adjdRefNbr, Equal<Required<ARInvoice.refNbr>>>>>(this);


				foreach (PXResult<ARAdjust, ARPayment> res in select.Select(invoice.DocType, invoice.RefNbr))
				{
					ARAdjust row = (ARAdjust)res;
					ARPayment payment = (ARPayment)res;

					ARAdjust copy = PXCache<ARAdjust>.CreateCopy(row);
					copy.CuryAdjdAmt = Math.Min(copy.CuryAdjdAmt.GetValueOrDefault(), invoice.CuryDocBal.GetValueOrDefault());
					Adjustments.Update(copy);
				}
			}

		}

		protected override decimal? GetDocDiscount()
		{
			return SODocument.Current.CuryDiscTot;
		}
		
		#endregion
	}
}
