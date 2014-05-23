using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.SM;
using PX.TM;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;
using LocationStatus = PX.Objects.IN.Overrides.INDocumentRelease.LocationStatus;
using LotSerialStatus = PX.Objects.IN.Overrides.INDocumentRelease.LotSerialStatus;
using ItemLotSerial = PX.Objects.IN.Overrides.INDocumentRelease.ItemLotSerial;
using POLineType = PX.Objects.PO.POLineType;
using POReceipt = PX.Objects.PO.POReceipt;
using POReceiptLine = PX.Objects.PO.POReceiptLine;

namespace PX.Objects.SO
{
	public class SOShipmentEntry : PXGraph<SOShipmentEntry, SOShipment>
	{
		public ToggleCurrency<SOShipment> CurrencyView;
		public PXSelectJoin<SOShipment,
			LeftJoin<Customer, On<Customer.bAccountID, Equal<SOShipment.customerID>>,
			LeftJoin<INSite, On<INSite.siteID, Equal<SOShipment.siteID>>>>,
			Where2<Where<Customer.bAccountID, IsNull,
			Or<Match<Customer, Current<AccessInfo.userName>>>>,
			And<Where<INSite.siteID, IsNull,
			Or<Match<INSite, Current<AccessInfo.userName>>>>>>> Document;
		public PXSelect<SOShipment, Where<SOShipment.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>> CurrentDocument;
		public PXSelect<SOShipLine, Where<SOShipLine.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>, OrderBy<Asc<SOShipLine.shipmentNbr, Asc<SOShipLine.lineNbr>>>> Transactions;
		public PXSelect<SOShipLineSplit, Where<SOShipLineSplit.shipmentNbr, Equal<Current<SOShipLine.shipmentNbr>>, And<SOShipLineSplit.lineNbr, Equal<Current<SOShipLine.lineNbr>>>>> splits;
		[PXViewName(Messages.ShippingAddress)]
		public PXSelect<SOShipmentAddress, Where<SOShipmentAddress.addressID, Equal<Current<SOShipment.shipAddressID>>>> Shipping_Address;
		[PXViewName(Messages.ShippingContact)]
		public PXSelect<SOShipmentContact, Where<SOShipmentContact.contactID, Equal<Current<SOShipment.shipContactID>>>> Shipping_Contact;
        [PXViewName(Messages.SOOrderShipment)]
        public PXSelectJoin<SOOrderShipment,
				InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOOrderShipment.orderType>, And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>,
				InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<SOOrder.curyInfoID>>,
				InnerJoin<SOAddress, On<SOAddress.addressID, Equal<SOOrder.billAddressID>>,
				InnerJoin<SOContact, On<SOContact.contactID, Equal<SOOrder.billContactID>>,
				InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrder.orderType>>>>>>>,
				Where<SOOrderShipment.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>, And<SOOrderShipment.shipmentType, Equal<Current<SOShipment.shipmentType>>>>> OrderList;
		public PXSelect<SOOrderShipment, Where<SOOrderShipment.shipmentNbr, Equal<Optional<SOOrderShipment.shipmentNbr>>, And<SOOrderShipment.shipmentType, Equal<Optional<SOOrderShipment.shipmentType>>, And<SOOrderShipment.orderType, Equal<Current<SOOrderShipment.orderType>>, And<SOOrderShipment.orderNbr, Equal<Current<SOOrderShipment.orderNbr>>>>>>> ordershipment;
		public PXSelect<SOShipmentDiscountDetail, Where<SOShipmentDiscountDetail.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>> DiscountDetails;
		public PXSelect<SOShipLine, Where<SOShipLine.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>, And<SOShipLine.isFree, Equal<boolTrue>>>, OrderBy<Asc<SOShipLine.shipmentNbr, Asc<SOShipLine.lineNbr>>>> FreeItems;
		public PXSelect<SOPackageDetail, Where<SOPackageDetail.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>> Packages;
		public PXSetup<Carrier, Where<Carrier.carrierID, Equal<Current<SOShipment.shipVia>>>> carrier;
		public PXSelect<CurrencyInfo> currencyinfo;
		public PXSelect<CurrencyInfo> DummyCuryInfo;
		public PXSetup<INSetup> insetup;
		public PXSetup<SOSetup> sosetup;
        public PXSetup<ARSetup> arsetup;
		public CMSetupSelect cmsetup;
		public PXSetup<GL.Branch, InnerJoin<INSite, On<INSite.branchID, Equal<GL.Branch.branchID>>>, Where<INSite.siteID, Equal<Optional<SOShipment.siteID>>>> Company;
		public PXSetup<Customer, Where<Customer.bAccountID, Equal<Optional<SOShipment.customerID>>>> customer;
		public PXSetup<Location, Where<Location.bAccountID, Equal<Current<SOShipment.customerID>>, And<Location.locationID, Equal<Optional<SOShipment.customerLocationID>>>>> location;

		public LSSOShipLine lsselect;
		public PXSelect<SOLine2> soline;
		public PXSelect<SOLineSplit2> solinesplit;
		public PXSelect<SOLine> dummy_soline; //will prevent collection was modified if no Select<SOLine> was executed prior to Persist()
		public PXSelect<SOOrder> soorder;

		public PXFilter<AddSOFilter> addsofilter;
        public PXSelectJoin<SOShipmentPlan, InnerJoin<SOLine, On<SOLine.planID, Equal<SOShipmentPlan.planID>>>> soshipmentplan;

		[CRBAccountReference(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<SOShipment.customerID>>>>))]
		[CRDefaultMailTo(typeof(Select<SOShipmentContact, Where<SOShipmentContact.contactID, Equal<Current<SOShipment.shipContactID>>>>))]
		public CRActivityList<SOShipment>
			Activity;

        [PXViewName(CR.Messages.MainContact)]
        public PXSelect<Contact> DefaultCompanyContact;
        protected virtual IEnumerable defaultCompanyContact()
        {
            List<int> branches = PXAccess.GetMasterBranchID(Accessinfo.BranchID);
            foreach (PXResult<GL.Branch, BAccountR, Contact> res in PXSelectJoin<GL.Branch,
                                        LeftJoin<BAccountR, On<GL.Branch.bAccountID, Equal<BAccountR.bAccountID>>,
                                        LeftJoin<Contact, On<BAccountR.defContactID, Equal<Contact.contactID>>>>,
                                        Where<GL.Branch.branchID, Equal<Required<GL.Branch.branchID>>>>.Select(this, branches != null ? (int?)branches[0] : null))
            {
                yield return (Contact)res;
                break;
            }
        }

        public PXAction<SOShipment> hold;
		[PXUIField(DisplayName = "Hold")]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		protected virtual IEnumerable Hold(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<SOShipment> flow;
		[PXUIField(DisplayName = "Flow")]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		protected virtual IEnumerable Flow(PXAdapter adapter,
			[PXInt]
			[PXIntList(new int[] { 5 }, new string[] { "OnConfirmation" })]
			int? actionID,
			[PXString(1)]
			[SOOrderStatus.List()]
			string orderStatus1,
			[PXString(1)]
			[SOOrderStatus.List()]
			string orderStatus2)
		{
			switch (actionID)
			{
				case 5: //OnConfirmation
					{
						List<SOShipment> list = new List<SOShipment>();
						foreach (SOShipment shipment in adapter.Get<SOShipment>())
						{
							list.Add(shipment);
						}

						Save.Press();

						PXLongOperation.StartOperation(this, delegate()
						{
							SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();

							foreach (SOShipment shipment in list)
							{
								docgraph.Document.Current = docgraph.Document.Search<SOShipment.shipmentNbr>(shipment.ShipmentNbr);

								foreach (PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact, SOOrderType> res in OrderList.Select())
								{
									SOOrder order = (SOOrder)res;

									if ((int)order.OpenShipmentCntr == 0)
									{
										order.Status = ((int)order.OpenLineCntr == 0) ? orderStatus1 : orderStatus2;

										docgraph.soorder.Cache.Update(order);
										docgraph.Save.Press();
									}
								}
							}
						});
					}
					break;
				default:
					Save.Press();
					break;
			}
			return adapter.Get();
		}

        public PXAction<SOShipment> notification;
        [PXUIField(DisplayName = "Notifications", Visible = false)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
        protected virtual IEnumerable Notification(PXAdapter adapter,
        [PXString]
		string notificationCD
        )
        {
            foreach (SOShipment shipment in adapter.Get<SOShipment>())
            {
                var parameters = new Dictionary<string, string>();
                parameters["SOShipment.ShipmentNbr"] = shipment.ShipmentNbr;
                Activity.SendNotification(ARNotificationSource.Customer, notificationCD, Accessinfo.BranchID, parameters);

                yield return shipment;
            }
        }

		public PXAction<SOShipment> action;
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual IEnumerable Action(PXAdapter adapter,
			[PXInt]
			[PXIntList(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new string[] { "Confirm Shipment", "Create Invoice", "Post Invoice to IN", "Apply Assignment Rules", "Correct Shipment", "Create Drop-Ship Invoice", "Print Labels", "Get Return Labels", "Cancel Return" })]
			int? actionID,
			[PXString()]
			string ActionName
			)
		{			
			if (!string.IsNullOrEmpty(ActionName))
			{
				PXAction action = this.Actions[ActionName];

				if (action != null)
				{
					Save.Press();
					List<object> result = new List<object>();
					foreach (object data in action.Press(adapter))
					{
						result.Add(data);
					}
					return result;
				}
			}

			List<SOShipment> list = new List<SOShipment>();
			foreach (SOShipment order in adapter.Get<SOShipment>())
			{
				list.Add(order);
			}

			switch (actionID)
			{
				case 1:
					{
						List<object> _persisted = new List<object>();
						foreach (SOOrder item in Caches[typeof(SOOrder)].Updated)
						{
							_persisted.Add(item);
						}

						if (!PXLongOperation.Exists(this.UID) && Document.Current != null && Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Updated)
						{
							Save.Press();
							PXAutomation.CompleteAction(this);
							PXLongOperation.WaitCompletion(this.UID);
							PXLongOperation.ClearStatus(this.UID);
						}
						else
						{
							Save.Press();
						}
						PXLongOperation.StartOperation(this, delegate()
						{
							SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();
							SOOrderEntry orderentry = PXGraph.CreateInstance<SOOrderEntry>();

							orderentry.Caches[typeof(SiteStatus)] = docgraph.Caches[typeof(SiteStatus)];
							orderentry.Caches[typeof(LocationStatus)] = docgraph.Caches[typeof(LocationStatus)];
							orderentry.Caches[typeof(LotSerialStatus)] = docgraph.Caches[typeof(LotSerialStatus)];
							orderentry.Caches[typeof(ItemLotSerial)] = docgraph.Caches[typeof(ItemLotSerial)];

							PXCache cache = orderentry.Caches[typeof(SOShipLineSplit)];
							cache = orderentry.Caches[typeof(INTranSplit)];

							orderentry.Views.Caches.Remove(typeof(SiteStatus));
							orderentry.Views.Caches.Remove(typeof(LocationStatus));
							orderentry.Views.Caches.Remove(typeof(LotSerialStatus));
							orderentry.Views.Caches.Remove(typeof(ItemLotSerial));

							PXAutomation.StorePersisted(docgraph, typeof(SOOrder), _persisted);
							foreach (SOOrder item in _persisted)
							{
								PXTimeStampScope.PutPersisted(orderentry.Document.Cache, item, item.tstamp);
							}

							foreach (SOShipment shipment in list)
							{
								if (shipment.Operation != SOOperation.Receipt )
									docgraph.ShipPackages(shipment);
								docgraph.ConfirmShipment(orderentry, shipment);
							}
						});
					}
					break;
				case 2:
					{
						Save.Press();

						PXLongOperation.StartOperation(this, delegate()
						{
							SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();
							SOInvoiceEntry ie = PXGraph.CreateInstance<SOInvoiceEntry>();

							DocumentList<ARInvoice, SOInvoice> created = new DocumentList<ARInvoice, SOInvoice>(docgraph);
							char[] a = typeof(SOShipmentFilter.invoiceDate).Name.ToCharArray();
							a[0] = char.ToUpper(a[0]);
							object invoiceDate;
							if( !adapter.Arguments.TryGetValue(new string(a), out invoiceDate))
							{
								invoiceDate = this.Accessinfo.BusinessDate;
							}
							foreach (SOShipment order in list)
							{
								PXProcessing<SOShipment>.SetCurrentItem(order);
								docgraph.InvoiceShipment(ie, order, (DateTime)invoiceDate, created);
							}

							if (!adapter.MassProcess && created.Count > 0)
							{
								using (new PXTimeStampScope(null))
								{
									ie.Clear();
									ie.Document.Current = ie.Document.Search<ARInvoice.docType, ARInvoice.refNbr>(((ARInvoice)created[0]).DocType, ((ARInvoice)created[0]).RefNbr, ((ARInvoice)created[0]).DocType);
									throw new PXRedirectRequiredException(ie, "Invoice");
								}
							}
						});
					}
					break;
				case 6: //Invoice Receipt
					{
						PXLongOperation.StartOperation(this, delegate()
						{
							SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();
							DocumentList<ARInvoice, SOInvoice> created = new DocumentList<ARInvoice, SOInvoice>(docgraph);

							SOShipmentEntry.InvoiceReceipt(adapter.Arguments, list, created);

							if (!adapter.MassProcess && created.Count > 0)
							{
								using (new PXTimeStampScope(null))
								{
									SOInvoiceEntry ie = PXGraph.CreateInstance<SOInvoiceEntry>();
									ie.Document.Current = (ARInvoice)created[0];
									throw new PXRedirectRequiredException(ie, "Invoice");
								}
							}
						});
					}
					break;
				case 3:
					{
						Save.Press();

						PXLongOperation.StartOperation(this, delegate()
						{
							INIssueEntry ie = PXGraph.CreateInstance<INIssueEntry>(); 
							SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();

							docgraph.Caches[typeof(SiteStatus)] = ie.Caches[typeof(SiteStatus)];
							docgraph.Caches[typeof(LocationStatus)] = ie.Caches[typeof(LocationStatus)];
							docgraph.Caches[typeof(LotSerialStatus)] = ie.Caches[typeof(LotSerialStatus)];
							docgraph.Caches[typeof(ItemLotSerial)] = ie.Caches[typeof(ItemLotSerial)];

							docgraph.Views.Caches.Remove(typeof(SiteStatus));
							docgraph.Views.Caches.Remove(typeof(LocationStatus));
							docgraph.Views.Caches.Remove(typeof(LotSerialStatus));
							docgraph.Views.Caches.Remove(typeof(ItemLotSerial));

							DocumentList<INRegister> created = new DocumentList<INRegister>(docgraph);

							ie.FieldVerifying.AddHandler<INTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
							ie.FieldVerifying.AddHandler<INTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
							foreach (SOShipment order in list)
							{
								docgraph.PostShipment(ie, order, created);
							}

							if (docgraph.sosetup.Current.AutoReleaseIN == true && created.Count > 0 && created[0].Hold == false)
							{
								INDocumentRelease.ReleaseDoc(created, false);
							}

						});
					}
					break;
				case 4:
					{
						if (sosetup.Current.DefaultShipmentAssignmentMapID == null)
						{
							throw new PXSetPropertyException(Messages.AssignNotSetup, "SO Setup");
						}
						EPAssignmentProcessHelper<SOShipment> aph = new EPAssignmentProcessHelper<SOShipment>();
						aph.Assign(Document.Current, sosetup.Current.DefaultShipmentAssignmentMapID);
						Document.Update(Document.Current);
					}
					break;
				case 5:
					{
						Save.Press();

						PXLongOperation.StartOperation(this, delegate()
						{
							SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();
							SOOrderEntry orderentry = PXGraph.CreateInstance<SOOrderEntry>();

							orderentry.Caches[typeof(SiteStatus)] = docgraph.Caches[typeof(SiteStatus)];
							orderentry.Caches[typeof(LocationStatus)] = docgraph.Caches[typeof(LocationStatus)];
							orderentry.Caches[typeof(LotSerialStatus)] = docgraph.Caches[typeof(LotSerialStatus)];
							orderentry.Caches[typeof(ItemLotSerial)] = docgraph.Caches[typeof(ItemLotSerial)];

							PXCache cache = orderentry.Caches[typeof(SOShipLineSplit)];
							cache = orderentry.Caches[typeof(INTranSplit)];

							orderentry.Views.Caches.Remove(typeof(SiteStatus));
							orderentry.Views.Caches.Remove(typeof(LocationStatus));
							orderentry.Views.Caches.Remove(typeof(LotSerialStatus));
							orderentry.Views.Caches.Remove(typeof(ItemLotSerial));

							foreach (SOShipment shipment in list)
							{
								docgraph.CancelPackages(shipment);
								docgraph.CorrectShipment(orderentry, shipment);
							}
						});
					}
					break;
				case 7: //Print Labels
					if (adapter.MassProcess)
					{
						Save.Press();

						PXLongOperation.StartOperation(this, delegate()
						{
							SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();
							PrintCarrierLabels(list);

						});
					}
					else
					{
						PrintCarrierLabels();
					}
					break;
				case 8: //Get Return Labels
						List<object> _persisted2 = new List<object>();
						foreach (SOOrder item in Caches[typeof(SOOrder)].Updated)
						{
							_persisted2.Add(item);
						}

						Save.Press();

						PXLongOperation.StartOperation(this, delegate()
						{
							SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();
							PXAutomation.StorePersisted(docgraph, typeof(SOOrder), _persisted2);

							foreach (SOShipment order in list)
							{
								docgraph.GetReturnLabels(order);
							}
						});
					break;
				case 9: //Cancel Return
					List<object> _persisted3 = new List<object>();
					foreach (SOOrder item in Caches[typeof(SOOrder)].Updated)
					{
						_persisted3.Add(item);
					}

					Save.Press();

					PXLongOperation.StartOperation(this, delegate()
					{
						SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();
						PXAutomation.StorePersisted(docgraph, typeof(SOOrder), _persisted3);

						foreach (SOShipment order in list)
						{
							docgraph.CancelPackages(order);
						}
					});
					break;
				default:
					Save.Press();
					break;
			}

			return list;
		}

		public PXAction<SOShipment> inquiry;
		[PXUIField(DisplayName = "Inquiries", MapEnableRights = PXCacheRights.Select)]
		[PXButton(MenuAutoOpen = true)]
		protected virtual IEnumerable Inquiry(PXAdapter adapter,
			[PXInt]
			[PXIntList(new int[] { }, new string[] { })]
			int? inquiryID,
			[PXString()]
			string ActionName
			)
		{
			if (!string.IsNullOrEmpty(ActionName))
			{
				PXAction action = this.Actions[ActionName];

				if (action != null)
				{
					Save.Press();
					foreach (object data in action.Press(adapter)) ;
				}
			}
			return adapter.Get();
		}

		//throw new PXReportRequiredException(parameters, "SO642000", "Shipment Confirmation");
		public PXAction<SOShipment> report;
		[PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual IEnumerable Report(PXAdapter adapter,
			[PXString(8, InputMask = "CC.CC.CC.CC")]
			string reportID
			)
		{
			if (!String.IsNullOrEmpty(reportID))
			{
				Save.Press();
				int i = 0;
				var cstmr = customer.Current;
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				string actualReportID = null;
				foreach (SOShipment order in adapter.Get<SOShipment>())
				{
					if (actualReportID == null)					
						actualReportID = new NotificationUtility(this).SearchReport(ARNotificationSource.Customer, cstmr, reportID, Company.Current.BranchID);					
					parameters["SOShipment.ShipmentNbr" + i.ToString()] = order.ShipmentNbr;					
					i++;
				}
				if (i > 0)
				{
					throw new PXReportRequiredException(parameters,
						reportID,
						"Report " + reportID);
				}
			}
			return adapter.Get();
		}

		public PXAction<SOShipment> recalculatePackages;
		[PXUIField(DisplayName = "Refresh Packages", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		protected virtual IEnumerable RecalculatePackages(PXAdapter adapter)
		{
			if (Document.Current != null)
			{
				if ( Document.Current.Released == true || Document.Current.Confirmed == true )
					throw new PXException("Packages cannot be recalculated on a confirmed / released document.");

				if (Document.Current.SiteID == null )
					throw new PXException("WarehouseID must be specified before packages can be recalculated.");
				
				foreach (SOPackageDetail package in Packages.Select())
				{
					Packages.Delete(package);
				}

				decimal weightTotal = 0;
				SOPackageEngine.PackSet manualPackSet;
				IList<SOPackageEngine.PackSet> packsets = CalculatePackages(Document.Current, out manualPackSet);
				foreach (SOPackageEngine.PackSet ps in packsets)
				{
					foreach (SOPackageInfoEx package in ps.Packages)
					{
						weightTotal += (package.Weight.GetValueOrDefault() + package.BoxWeight.GetValueOrDefault());

						SOPackageDetail detail = new SOPackageDetail();
						detail.PackageType = SOPackageType.Auto;
						detail.ShipmentNbr = Document.Current.ShipmentNbr;
						detail.BoxID = package.BoxID;
						detail.Weight = package.Weight;
						detail.WeightUOM = package.WeightUOM;
						detail.Qty = package.Qty;
						detail.QtyUOM = package.QtyUOM;
						detail.InventoryID = package.InventoryID;
						detail.DeclaredValue = package.DeclaredValue;

						detail = Packages.Insert(detail);
						detail.Confirmed = false;
					}
				}

				foreach (SOPackageInfoEx package in manualPackSet.Packages)
				{
					weightTotal += (package.Weight.GetValueOrDefault() + package.BoxWeight.GetValueOrDefault());

					SOPackageDetail detail = new SOPackageDetail();
					detail.PackageType = SOPackageType.Manual;
					detail.ShipmentNbr = Document.Current.ShipmentNbr;
					detail.BoxID = package.BoxID;
					detail.Weight = package.Weight;
					detail.WeightUOM = package.WeightUOM;
					detail.Qty = package.Qty;
					detail.QtyUOM = package.QtyUOM;
					detail.InventoryID = package.InventoryID;
					detail.DeclaredValue = package.DeclaredValue;

					detail = Packages.Insert(detail);
					detail.Confirmed = false;
				}

				Document.Current.PackageWeight = weightTotal;
				Document.Current.IsPackageValid = true;
			}

			return adapter.Get();
		}

		public virtual void SOShipmentPlan_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

			SOShipmentPlan plan = (SOShipmentPlan)e.Row;
			if (Document.Current.ShipDate < plan.PlanDate)
			{
				PXUIFieldAttribute.SetWarning<SOShipmentPlan.planDate>(sender, plan, Messages.PlanDateGreaterShipDate);
			}
		}

		public virtual IEnumerable sOshipmentplan()
		{
			PXSelectBase<SOShipLine> cmd =
				new PXSelect<SOShipLine,
					Where<SOShipLine.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>,
					And<SOShipLine.origOrderType, Equal<Current<SOLine.orderType>>,
					And<SOShipLine.origOrderNbr, Equal<Current<SOLine.orderNbr>>,
					And<SOShipLine.origLineNbr, Equal<Current<SOLine.lineNbr>>>>>>>(this);

			foreach (PXResult<SOShipmentPlan, SOLine, SOOrderShipment> res in
				PXSelectJoin<SOShipmentPlan,
					 InnerJoin<SOLine, On<SOLine.planID, Equal<SOShipmentPlan.planID>>,
					 LeftJoin<SOOrderShipment,
						   On<SOOrderShipment.orderType, Equal<SOShipmentPlan.orderType>,
						  And<SOOrderShipment.orderNbr, Equal<SOShipmentPlan.orderNbr>,
								And<SOOrderShipment.operation, Equal<SOLine.operation>,
								And<SOOrderShipment.siteID, Equal<SOShipmentPlan.siteID>,
								And<SOOrderShipment.confirmed, Equal<boolFalse>,
								And<SOOrderShipment.shipmentNbr, NotEqual<Current<SOShipment.shipmentNbr>>>>>>>>>>,
					 Where<SOShipmentPlan.orderType, Equal<Current<AddSOFilter.orderType>>,
					 And<SOShipmentPlan.orderNbr, Equal<Current<AddSOFilter.orderNbr>>,
					 And<SOShipmentPlan.siteID, Equal<Current<SOShipment.siteID>>,
					 And<SOOrderShipment.shipmentNbr, IsNull,
					 And<SOLine.operation, Equal<Current<AddSOFilter.operation>>,
					 And<Where<Current<SOShipment.destinationSiteID>, IsNull,
									Or<SOShipmentPlan.destinationSiteID, Equal<Current<SOShipment.destinationSiteID>>>>>>>>>>>
					.Select(this))
			{
				if (cmd.View.SelectSingleBound(new object[] { (SOLine)res }) == null)
				{
					yield return new PXResult<SOShipmentPlan, SOLine>((SOShipmentPlan)res, (SOLine)res);
				}
			}

			foreach (PXResult<SOShipmentPlan, SOLineSplit, SOLine, SOOrderShipment> res in
				PXSelectJoin<SOShipmentPlan,
					 InnerJoin<SOLineSplit, On<SOLineSplit.planID, Equal<SOShipmentPlan.planID>>,
					 InnerJoin<SOLine, On<SOLine.orderType, Equal<SOLineSplit.orderType>, And<SOLine.orderNbr, Equal<SOLineSplit.orderNbr>, And<SOLine.lineNbr, Equal<SOLineSplit.lineNbr>>>>,
					 LeftJoin<SOOrderShipment,
						   On<SOOrderShipment.orderType, Equal<SOShipmentPlan.orderType>,
								 And<SOOrderShipment.orderNbr, Equal<SOShipmentPlan.orderNbr>,
								 And<SOOrderShipment.siteID, Equal<SOShipmentPlan.siteID>,
								 And<SOOrderShipment.operation, Equal<SOLine.operation>,
								 And<SOOrderShipment.confirmed, Equal<boolFalse>,
								 And<SOOrderShipment.shipmentNbr, NotEqual<Current<SOShipment.shipmentNbr>>>>>>>>>>>,
					 Where<SOShipmentPlan.orderType, Equal<Current<AddSOFilter.orderType>>,
					 And<SOShipmentPlan.orderNbr, Equal<Current<AddSOFilter.orderNbr>>,
					 And<SOShipmentPlan.siteID, Equal<Current<SOShipment.siteID>>,
					 And<SOLine.operation, Equal<Current<AddSOFilter.operation>>,
					 And<SOOrderShipment.shipmentNbr, IsNull,
					 And<Where<SOShipmentPlan.inclQtySOShipping, Equal<True>, Or<SOShipmentPlan.inclQtySOShipped, Equal<True>, Or<SOShipmentPlan.requireAllocation, Equal<False>>>>>>>>>>>.Select(this))
			{
				if (cmd.View.SelectSingleBound(new object[] { (SOLine)(SOLineSplit)res }) == null)
				{
					yield return new PXResult<SOShipmentPlan, SOLine>((SOShipmentPlan)res, (SOLine)(SOLineSplit)res);
				}
			}
		}

		public PXAction<SOShipment> inventorySummary;
		[PXUIField(DisplayName = "Inventory Summary", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable InventorySummary(PXAdapter adapter)
		{
			PXCache tCache = Transactions.Cache;
			SOShipLine line = Transactions.Current;
			if (line == null) return adapter.Get();

			InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<SOShipLine.inventoryID>(tCache, line);
			if (item != null && item.StkItem == true)
			{
				INSubItem sbitem = (INSubItem)PXSelectorAttribute.Select<SOShipLine.subItemID>(tCache, line);
				InventorySummaryEnq.Redirect(item.InventoryID,
											 ((sbitem != null) ? sbitem.SubItemCD : null),
											 line.SiteID,
											 line.LocationID);
			}
			return adapter.Get();
		}

		public SOShipmentEntry()
		{
			{
				INSetup record = insetup.Current;
			}
			{
				SOSetup record = sosetup.Current;
			}
			CopyPaste.SetVisible(false);
			PXDBDefaultAttribute.SetDefaultForInsert<SOOrderShipment.shipAddressID>(OrderList.Cache, null, true);
			PXDBDefaultAttribute.SetDefaultForUpdate<SOOrderShipment.shipAddressID>(OrderList.Cache, null, true);

			PXDBDefaultAttribute.SetDefaultForInsert<SOOrderShipment.shipContactID>(OrderList.Cache, null, true);
			PXDBDefaultAttribute.SetDefaultForUpdate<SOOrderShipment.shipContactID>(OrderList.Cache, null, true);

			PXDBLiteDefaultAttribute.SetDefaultForInsert<SOOrderShipment.shipmentNbr>(OrderList.Cache, null, true);
			PXDBLiteDefaultAttribute.SetDefaultForUpdate<SOOrderShipment.shipmentNbr>(OrderList.Cache, null, true);

			//PXDimensionSelectorAttribute.SetValidCombo<SOShipLine.subItemID>(Transactions.Cache, true);
			//PXDimensionSelectorAttribute.SetValidCombo<SOShipLineSplit.subItemID>(splits.Cache, true);

			PXUIFieldAttribute.SetDisplayName(Caches[typeof(Contact)], typeof(Contact.salutation).Name, CR.Messages.Attention);
			this.Views.Caches.Add(typeof(SOLineSplit));

			FieldDefaulting.AddHandler<BAccountR.type>((sender, e) => { if (e.Row != null) e.NewValue = BAccountType.CustomerType; });
		}

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (viewName.ToLower() == "document" && values != null)
			{
				values["Hold"] = PXCache.NotSetValue;
			}
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		public PXAction<SOShipment> selectSO;
		[PXUIField(DisplayName = "Add Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable SelectSO(PXAdapter adapter)
		{
			if(this.Document.Cache.AllowDelete)
				addsofilter.AskExt();
			return adapter.Get();
		}

		public PXAction<SOShipment> addSO;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddSO(PXAdapter adapter)
		{
			SOOrder order = PXSelect<SOOrder,
				Where<SOOrder.orderType, Equal<Optional<AddSOFilter.orderType>>,
				  And<SOOrder.orderNbr, Equal<Optional<AddSOFilter.orderNbr>>>>>.Select(this);

			if (order != null)
			{
				CreateShipment(order, Document.Current.SiteID, Document.Current.ShipDate, false, addsofilter.Current.Operation, null);
			}

			if (addsofilter.Current != null)
			{
				try
				{
					addsofilter.Cache.SetDefaultExt<AddSOFilter.orderType>(addsofilter.Current);
					addsofilter.Current.OrderNbr = null;
				}
				catch {}
			}

			soshipmentplan.Cache.Clear();
			soshipmentplan.View.Clear();
            soshipmentplan.Cache.ClearQueryCache();

			return adapter.Get();
		}

		public PXAction<SOShipment> addSOCancel;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddSOCancel(PXAdapter adapter)
		{
			addsofilter.Cache.SetDefaultExt<AddSOFilter.orderType>(addsofilter.Current);
			addsofilter.Current.OrderNbr = null;
			soshipmentplan.Cache.Clear();
			soshipmentplan.View.Clear();

			return adapter.Get();
        }

		#region SOOrder Events
		protected virtual void SOOrder_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			SOOrder order = e.Row as SOOrder;

			if (e.Operation == PXDBOperation.Update)
			{
				if (order.ShipmentCntr < 0 || order.OpenShipmentCntr < 0 || (order.ShipmentCntr == 0 || order.OpenShipmentCntr == 0) && 
					((IEnumerable<SOOrderShipment>)OrderList.Cache.Inserted).Any(a => a.OrderType == order.OrderType && a.OrderNbr == order.OrderNbr) ||
					order.ShipmentCntr == 0 &&
					((IEnumerable<SOOrderShipment>)OrderList.Cache.Updated).Any(a => a.OrderType == order.OrderType && a.OrderNbr == order.OrderNbr))
				{
					throw new PXSetPropertyException(Messages.InvalidShipmentCounters);
				}
			}
		}
		#endregion

		#region SOLine2 Events

		[PXDBLong(BqlField = typeof(SOLine.planID), IsImmutable = true)]
        [SOLine2PlanID()]
        public virtual void SOLine2_PlanID_CacheAttached(PXCache sender)
        {
        }

        #endregion

		public PXAction<SOShipment> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton]
		public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			foreach (SOShipment current in adapter.Get<SOShipment>())
			{
				if (current != null)
				{
					bool needSave = false;
					Save.Press();
					SOShipmentAddress address = this.Shipping_Address.Select();
					if (address != null && address.IsDefaultAddress == false && address.IsValidated == false)
					{
						if (PXAddressValidator.Validate<SOShipmentAddress>(this, address, true))
							needSave = true;
					}
					
					if (needSave == true)
						this.Save.Press();
				}
				yield return current;
			}
		}

		#region CurrencyInfo events
	

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null)
			{
				e.NewValue = Document.Current.ShipDate;
				e.Cancel = true;
			}
		}

		protected virtual void CurrencyInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CurrencyInfo info = e.Row as CurrencyInfo;
			if (info != null)
			{
				bool curyenabled = info.AllowUpdate(this.Transactions.Cache);
				
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(sender, info, curyenabled);
			}
		}
		#endregion

		#region SOShipment Events
		protected virtual void SOShipment_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (sosetup.Current.RequireShipmentTotal == false)
			{
				if (PXCurrencyAttribute.IsNullOrEmpty(((SOShipment)e.Row).ShipmentQty) == false)
				{
					sender.SetValue<SOShipment.controlQty>(e.Row, ((SOShipment)e.Row).ShipmentQty);
				}
				else
				{
					sender.SetValue<SOShipment.controlQty>(e.Row, 0m);
				}
			}

			if (((SOShipment)e.Row).Hold == false && ((SOShipment)e.Row).Confirmed == false)
			{
				if ((bool)sosetup.Current.RequireShipmentTotal)
				{
					if (((SOShipment)e.Row).ShipmentQty != ((SOShipment)e.Row).ControlQty && ((SOShipment)e.Row).ControlQty != 0m)
					{
						sender.RaiseExceptionHandling<SOShipment.controlQty>(e.Row, ((SOShipment)e.Row).ControlQty, new PXSetPropertyException(Messages.DocumentOutOfBalance));
					}
					else
					{
						sender.RaiseExceptionHandling<SOShipment.controlQty>(e.Row, ((SOShipment)e.Row).ControlQty, null);
					}
				}
			}

			if (!sender.ObjectsEqual<SOShipment.lineTotal, SOShipment.shipmentWeight, SOShipment.shipmentVolume, SOShipment.shipTermsID, SOShipment.shipZoneID, SOShipment.shipVia>(e.OldRow, e.Row))
			{
				PXResultset<SOShipLine> res = Transactions.Select();
				if (res != null)
				{
					FreightCalculator fc = CreateFreightCalculator();
					fc.CalcFreight<SOShipment, SOShipment.curyFreightCost, SOShipment.curyFreightAmt>(sender, (SOShipment)e.Row, res.Count);
				}
			}

			if (!sender.ObjectsEqual<SOShipment.shipDate>(e.Row, e.OldRow))
			{
				foreach (SOOrderShipment item in OrderList.Select())
				{
					if (item.ShipmentType != SOShipmentType.DropShip)
					{
						item.ShipDate = ((SOShipment)e.Row).ShipDate;

						if (OrderList.Cache.GetStatus(item) == PXEntryStatus.Notchanged)
						{
							OrderList.Cache.SetStatus(item, PXEntryStatus.Updated);
						}
					}
				}
			}
		}

		protected virtual void SOShipment_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			PXUIFieldAttribute.SetVisible<SOShipment.curyID>(sender, e.Row, cmsetup.Current.MCActivated == true);

			bool curyenabled = true;

			PXUIFieldAttribute.SetEnabled(sender, e.Row, ((SOShipment)e.Row).Confirmed == false);
			PXUIFieldAttribute.SetEnabled<SOShipment.curyID>(sender, e.Row, ((SOShipment)e.Row).Confirmed == false && curyenabled);
			PXUIFieldAttribute.SetEnabled<SOShipment.status>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<SOShipment.shipmentQty>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<SOShipment.shipmentWeight>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<SOShipment.packageWeight>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<SOShipment.shipmentVolume>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<SOShipment.curyFreightCost>(sender, e.Row, false);

			sender.AllowInsert = true;
			sender.AllowUpdate = (((SOShipment)e.Row).Confirmed == false);
			sender.AllowDelete = (((SOShipment)e.Row).Confirmed == false);			
			selectSO.SetEnabled(((SOShipment)e.Row).SiteID != null && sender.AllowDelete);
			
			Transactions.Cache.AllowInsert = false;
			Transactions.Cache.AllowUpdate = (((SOShipment)e.Row).Confirmed == false);
			Transactions.Cache.AllowDelete = (((SOShipment)e.Row).Confirmed == false);

			splits.Cache.AllowInsert = (((SOShipment)e.Row).Confirmed == false);
			splits.Cache.AllowUpdate = (((SOShipment)e.Row).Confirmed == false);
			splits.Cache.AllowDelete = (((SOShipment)e.Row).Confirmed == false);

			Packages.Cache.AllowInsert = (((SOShipment)e.Row).Confirmed == false);
			Packages.Cache.AllowUpdate = (((SOShipment)e.Row).Confirmed == false);
			Packages.Cache.AllowDelete = (((SOShipment)e.Row).Confirmed == false);
			((SOShipment) e.Row).PackageCount = Packages.Select().Count;

			PXUIFieldAttribute.SetVisible<SOShipment.controlQty>(sender, e.Row, (bool)sosetup.Current.RequireShipmentTotal || (bool)((SOShipment)e.Row).Confirmed);

			PXUIFieldAttribute.SetEnabled<SOShipment.shipmentType>(sender, e.Row, sender.AllowUpdate && (Transactions.Select().Count == 0));
			PXUIFieldAttribute.SetEnabled<SOShipment.operation>(sender, e.Row, sender.AllowUpdate && (Transactions.Select().Count == 0));
			PXUIFieldAttribute.SetEnabled<SOShipment.customerID>(sender, e.Row, sender.AllowUpdate && (Transactions.Select().Count == 0));
			PXUIFieldAttribute.SetEnabled<SOShipment.customerLocationID>(sender, e.Row, sender.AllowUpdate && (Transactions.Select().Count == 0));
			PXUIFieldAttribute.SetEnabled<SOShipment.siteID>(sender, e.Row, sender.AllowUpdate && (Transactions.Select().Count == 0));
			PXUIFieldAttribute.SetEnabled<SOShipment.destinationSiteID>(sender, e.Row,
				sender.AllowUpdate && (Transactions.Select().Count == 0) && ((SOShipment)e.Row).ShipmentType == SOShipmentType.Transfer);

			SOShipmentAddress shipAddress = this.Shipping_Address.Select();
			bool enableAddressValidation = (((SOShipment)e.Row).Confirmed == false)
				&& ((shipAddress != null && shipAddress.IsDefaultAddress == false && shipAddress.IsValidated == false));
			this.validateAddresses.SetEnabled(enableAddressValidation);

			bool isGroundCollectVisible = false;

			if (((SOShipment)e.Row).ShipVia != null)
			{
				Carrier carrier = PXSelect<Carrier, Where<Carrier.carrierID, Equal<Required<Carrier.carrierID>>>>.Select(this, ((SOShipment)e.Row).ShipVia);

				if (carrier != null && carrier.IsExternal == true && !string.IsNullOrEmpty(carrier.CarrierPluginID))
				{
					ICarrierService service = CarrierPluginMaint.CreateCarrierService(this, carrier.CarrierPluginID);
					if (service != null)
						isGroundCollectVisible = service.Attributes.Contains("COLLECT");
				}
			}
			
			PXUIFieldAttribute.SetVisible<SOShipment.groundCollect>(sender, e.Row, isGroundCollectVisible);
		}

		protected virtual void SOShipment_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOShipment.customerLocationID>(e.Row);
		}

		protected virtual void SOShipment_CustomerLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOShipment.siteID>(e.Row);
			SOShipmentAddressAttribute.DefaultRecord<SOShipment.shipAddressID>(sender, e.Row);
			SOShipmentContactAttribute.DefaultRecord<SOShipment.shipContactID>(sender, e.Row);
		}

		protected virtual void SOShipment_DestinationSiteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOShipment shipment = e.Row as SOShipment;
			if (shipment == null || shipment.ShipmentType != SOShipmentType.Transfer)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void SOShipment_DestinationSiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOShipmentAddressAttribute.DefaultRecord<SOShipment.shipAddressID>(sender, e.Row);
			SOShipmentContactAttribute.DefaultRecord<SOShipment.shipContactID>(sender, e.Row);
		}

		protected virtual void SOShipment_ShipVia_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if ((bool)cmsetup.Current.MCActivated)
			{
				CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<SOShipment.curyInfoID>(sender, e.Row);

				string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);
				if (string.IsNullOrEmpty(message) == false)
				{
					sender.RaiseExceptionHandling<SOShipment.shipDate>(e.Row, ((SOShipment)e.Row).ShipDate, new PXSetPropertyException(message, PXErrorLevel.Warning));
				}

				if (info != null)
				{
					((SOShipment)e.Row).CuryID = info.CuryID;
				}
			}

			sender.SetDefaultExt<SOShipment.taxCategoryID>(e.Row);

			SOShipment row = e.Row as SOShipment;
			if (row != null)
			{
				row.UseCustomerAccount = CanUseCustomerAccount(row);
				row.IsPackageValid = false;
			}
		}
		

		protected virtual bool CanUseCustomerAccount(SOShipment row)
		{
			Carrier carrier = PXSelect<Carrier, Where<Carrier.carrierID, Equal<Required<Carrier.carrierID>>>>.Select(this, row.ShipVia);
			if (carrier != null && !string.IsNullOrEmpty(carrier.CarrierPluginID))
			{
				foreach (CarrierPluginCustomer cpc in PXSelect<CarrierPluginCustomer, 
						Where<CarrierPluginCustomer.carrierPluginID, Equal<Required<CarrierPluginCustomer.carrierPluginID>>,
						And<CarrierPluginCustomer.customerID, Equal<Required<CarrierPluginCustomer.customerID>>,
						And<CarrierPluginCustomer.isActive, Equal<True>>>>>.Select(this, carrier.CarrierPluginID, row.CustomerID))
				{
					if ( !string.IsNullOrEmpty(cpc.CarrierAccount) &&
						(cpc.CustomerLocationID == row.CustomerLocationID || cpc.CustomerLocationID == null )
						)
					{
						return true;
					}
				}								
			}

			return false;
		}

		protected virtual void SOShipment_UseCustomerAccount_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOShipment row = e.Row as SOShipment;
			if (row != null)
			{
				bool canBeTrue = CanUseCustomerAccount(row);

				if (e.NewValue != null && ((bool)e.NewValue) && !canBeTrue)
				{
					e.NewValue = false;
					throw new PXSetPropertyException(Messages.CustomeCarrierAccountIsNotSetup);
				}
			}
		}


		#endregion

		#region SOOrderShipment Events

		protected virtual void UpdateShipmentCntr(PXCache sender, object Row, short? Counter)
		{
			SOOrder order = PXSelect<SOOrder, Where<SOOrder.orderType, Equal<Current<SOOrderShipment.orderType>>, And<SOOrder.orderNbr, Equal<Current<SOOrderShipment.orderNbr>>>>>.SelectSingleBound(sender.Graph, new object[] { Row });
			if (order != null)
			{
				order.ShipmentDeleted = (Counter == -1) ? true : (bool?)null;
				order.ShipmentCntr += Counter;
				if (((SOOrderShipment)Row).Confirmed == false)
				{
					order.OpenShipmentCntr += Counter;
				}
				soorder.Cache.SetStatus(order, PXEntryStatus.Updated);
			}
		}

		protected virtual void SOOrderShipment_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			UpdateShipmentCntr(sender, e.Row, (short)1);
		}

		protected virtual void SOOrderShipment_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			//during correct shipment this will eliminate overwrite of SOOrder in SOShipmentEntry.Persist()
			if (!object.ReferenceEquals(e.Row, e.OldRow))
			{
				UpdateShipmentCntr(sender, e.OldRow, (short)-1);
				UpdateShipmentCntr(sender, e.Row, (short)1);
			}
		}

		protected virtual void SOOrderShipment_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			UpdateShipmentCntr(sender, e.Row, (short)-1);
		}

		protected virtual void SOOrderShipment_ShipmentNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void SOOrderShipment_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<SOOrderShipment.selected>(sender, e.Row, true);

		}

		#endregion

		#region SOShipLine Events
		protected virtual void SOShipLine_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			object oldValue = sender.GetValue<SOShipLine.inventoryID>(e.Row);
			if (oldValue != null)
			{
				e.NewValue = oldValue;
			}
		}

		protected virtual void SOShipLine_SubItemID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			object oldValue = sender.GetValue<SOShipLine.subItemID>(e.Row);
			if (oldValue != null && e.NewValue != null && e.ExternalCall)
			{
				e.NewValue = oldValue;
			}
		}

		protected virtual void SOShipLine_SiteID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			object oldValue = sender.GetValue<SOShipLine.siteID>(e.Row);
			if (oldValue != null && e.ExternalCall)
			{
				e.NewValue = oldValue;
			}
		}

		protected virtual void SOShipLine_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOShipLine.uOM>(e.Row);
			sender.SetDefaultExt<SOShipLine.unitWeigth>(e.Row);
			sender.SetDefaultExt<SOShipLine.unitVolume>(e.Row);

			SOShipLine tran = e.Row as SOShipLine;
			InventoryItem item = PXSelectorAttribute.Select<InventoryItem.inventoryID>(sender, tran) as InventoryItem;
			if (item != null && tran != null)
			{
				tran.TranDesc = item.Descr;
			}
		}

		protected virtual void SOShipLine_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		protected virtual void SOShipLine_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			SOShipLine row = e.Row as SOShipLine;

			if (row != null && !string.Equals(row.UOM, row.OrderUOM))
			{
				using (new PXConnectionScope())
				{
					decimal? unitprice = INUnitAttribute.ConvertFromTo<SOShipLine.inventoryID>(sender, row, row.UOM, row.OrderUOM, row.UnitPrice ?? 0, INPrecision.UNITCOST);
					sender.SetValueExt<SOShipLine.unitPrice>(row, unitprice);
				}
			}
		}

		protected virtual void DefaultUnitPrice(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			object UnitPrice;
			sender.RaiseFieldDefaulting<SOShipLine.unitPrice>(e.Row, out UnitPrice);

			if (UnitPrice != null && (decimal)UnitPrice != 0m)
			{
				decimal? unitprice = INUnitAttribute.ConvertFromTo<SOShipLine.inventoryID>(sender, e.Row, ((SOShipLine)e.Row).UOM, ((SOShipLine)e.Row).OrderUOM, (decimal)UnitPrice, INPrecision.UNITCOST);
				sender.SetValueExt<SOShipLine.unitPrice>(e.Row, unitprice);
			}
		}

		protected virtual void DefaultUnitCost(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			object UnitCost;
			sender.RaiseFieldDefaulting<SOShipLine.unitCost>(e.Row, out UnitCost);

			if (UnitCost != null && (decimal)UnitCost != 0m)
			{
				decimal? unitcost = INUnitAttribute.ConvertFromTo<SOShipLine.inventoryID>(sender, e.Row, ((SOShipLine)e.Row).UOM, ((SOShipLine)e.Row).OrderUOM, (decimal)UnitCost, INPrecision.UNITCOST);
				sender.SetValueExt<SOShipLine.unitCost>(e.Row, unitcost);
			}
		}
		
		protected virtual void SOShipLine_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOShipLine row = e.Row as SOShipLine;
			if (row != null)
			{
				DefaultUnitPrice(sender, e);
				DefaultUnitCost(sender, e);
			}
		}

		protected virtual void SOShipLine_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOShipLine row = e.Row as SOShipLine;
			if (row != null)
			{
				bool lineTypeInventory = row.LineType == SOLineType.Inventory;
				PXUIFieldAttribute.SetEnabled<SOShipLine.subItemID>(sender, row, lineTypeInventory);
				PXUIFieldAttribute.SetEnabled<SOShipLine.locationID>(sender, row, lineTypeInventory);
			}
		}

		protected virtual void SOShipLine_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Document.SetValueExt<SOShipment.isPackageValid>(Document.Current, false);
		}
		
		protected virtual void SOShipLine_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SOShipLine row = e.Row as SOShipLine;
			SOShipLine oldRow = e.OldRow as SOShipLine;
			if(row != null && sender.GetStatus(row) == PXEntryStatus.Inserted)
			{
				row.OriginalShippedQty = row.ShippedQty;
				row.BaseOriginalShippedQty = row.BaseShippedQty;
			}

			if (row != null && row.IsFree != true && !sender.ObjectsEqual<SOShipLine.shippedQty>(e.Row, e.OldRow))
			{
				PXSelectBase<SOShipmentDiscountDetail> selectDiscountDetailsByOrder = new PXSelect<SOShipmentDiscountDetail,
					Where<SOShipmentDiscountDetail.orderType, Equal<Required<SOShipmentDiscountDetail.orderType>>,
					And<SOShipmentDiscountDetail.orderNbr, Equal<Required<SOShipmentDiscountDetail.orderNbr>>,
					And<SOShipmentDiscountDetail.shipmentNbr, Equal<Required<SOShipmentDiscountDetail.shipmentNbr>>,
					And<SOShipmentDiscountDetail.type, Equal<DiscountType.LineDiscount>>>>>>(this);

				foreach (SOShipmentDiscountDetail sdd in selectDiscountDetailsByOrder.Select(row.OrigOrderType, row.OrigOrderNbr, row.ShipmentNbr))
				{
					DiscountDetails.Delete(sdd);
				}

				SOOrder order = PXSelect<SOOrder, Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>, And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>.Select(this, row.OrigOrderType, row.OrigOrderNbr);

                if (order != null)
                {
                    AllocateLineFreeItems(order);
                    AllocateGroupFreeItems(order);
                }
				
				AdjustFreeItemLines();
			}

			if (row != null && oldRow != null && ( row.BaseQty != oldRow.BaseQty))
			{
				Document.SetValueExt<SOShipment.isPackageValid>(Document.Current, false);
			}
		}

		protected virtual void SOShipLine_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			SOShipLine deleted = (SOShipLine) e.Row;
			if (deleted == null) return;

			SOShipLine line = PXSelect<SOShipLine, Where<SOShipLine.shipmentType, Equal<Current<SOShipLine.shipmentType>>, And<SOShipLine.shipmentNbr, Equal<Current<SOShipLine.shipmentNbr>>, And<SOShipLine.origOrderType, Equal<Current<SOShipLine.origOrderType>>, And<SOShipLine.origOrderNbr, Equal<Current<SOShipLine.origOrderNbr>>>>>>>.SelectSingleBound(this, new object[] { deleted });
			if(line == null)
			{
				SOOrderShipment oship = PXSelect<SOOrderShipment, Where<SOOrderShipment.shipmentType, Equal<Current<SOShipLine.shipmentType>>, And<SOOrderShipment.shipmentNbr, Equal<Current<SOShipLine.shipmentNbr>>, And<SOOrderShipment.orderType, Equal<Current<SOShipLine.origOrderType>>, And<SOOrderShipment.orderNbr, Equal<Current<SOShipLine.origOrderNbr>>>>>>>.SelectSingleBound(this, new object[] { deleted });
				ordershipment.Delete(oship);
			}

			Document.SetValueExt<SOShipment.isPackageValid>(Document.Current, false);
		}

		#endregion

		#region SOShipSplit Events
		#endregion

		#region AddSOFilter Events
		protected virtual void AddSOFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled<AddSOFilter.operation>(sender, e.Row,
				this.Document.Current == null || this.Document.Current.Operation == null);
		}
		#endregion
		
		#region SOPackageDetail Events
		
		protected virtual void SOPackageDetail_Weight_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOPackageDetail row = e.Row as SOPackageDetail;
			if (row != null)
			{
				row.Confirmed = true;
			}
		}

		protected virtual void SOPackageDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SOPackageDetail row = e.Row as SOPackageDetail;
			if (row != null)
			{
				CSBox box = PXSelect<CSBox, Where<CSBox.boxID, Equal<Required<CSBox.boxID>>>>.Select(this, row.BoxID);
				if (box != null && box.MaxWeight < row.Weight)
				{
					sender.RaiseExceptionHandling<SOPackageDetail.weight>(row, row.Weight, new PXSetPropertyException(Messages.WeightExceedsBoxSpecs));
				}
			}
		}

		protected virtual void SOPackageDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOPackageDetail row = e.Row as SOPackageDetail;
			if (row != null)
			{
				row.WeightUOM = insetup.Current.WeightUOM;
			}
		}

		#endregion
		
		#region Processing
		public decimal? ShipAvailableLots(SOShipmentPlan plan, SOShipLine newline, INLotSerClass lotserclass)
		{
			decimal? PlannedQty = plan.PlanQty;
			if (lotserclass.LotSerTrack == INLotSerTrack.SerialNumbered)
			{
				PlannedQty = Math.Floor((decimal)PlannedQty);
			}

			PXSelectBase<INLotSerialStatus> cmd = new PXSelectReadonly2<INLotSerialStatus,
				InnerJoin<INLocation, On<INLocation.locationID, Equal<INLotSerialStatus.locationID>>>,
				Where<INLotSerialStatus.inventoryID, Equal<Required<INLotSerialStatus.inventoryID>>,
					And<INLotSerialStatus.subItemID, Equal<Required<INLotSerialStatus.subItemID>>,
					And<INLotSerialStatus.siteID, Equal<Required<INLotSerialStatus.siteID>>,
					And<INLocation.salesValid, Equal<boolTrue>>>>>>(this);

			switch (lotserclass.LotSerIssueMethod)
			{
				case INLotSerIssueMethod.FIFO:
					cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Asc<INLotSerialStatus.receiptDate, Asc<INLotSerialStatus.lotSerialNbr>>>>>();
					break;
				case INLotSerIssueMethod.LIFO:
					cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Desc<INLotSerialStatus.receiptDate, Asc<INLotSerialStatus.lotSerialNbr>>>>>();
					break;
				case INLotSerIssueMethod.Expiration:
					cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Asc<INLotSerialStatus.expireDate, Asc<INLotSerialStatus.lotSerialNbr>>>>>();
					break;
				case INLotSerIssueMethod.Sequential:
					cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Asc<INLotSerialStatus.lotSerialNbr>>>>();
					break;
				case INLotSerIssueMethod.UserEnterable:
					{
						SOShipLine copy = PXCache<SOShipLine>.CreateCopy(newline);
						newline.UnassignedQty = PlannedQty;
						newline.BaseQty = PlannedQty;
						newline.Qty = INUnitAttribute.ConvertFromBase(Transactions.Cache, newline.InventoryID, newline.UOM, (decimal)newline.BaseQty, INPrecision.QUANTITY);

						Transactions.Cache.RaiseRowUpdated(newline, copy);
					}

					if (Document.Current.Hold == false)
					{
						SOShipment copy = PXCache<SOShipment>.CreateCopy(Document.Current);
						Document.Cache.SetValueExt<SOOrder.hold>(Document.Current, true);
						Document.Cache.RaiseRowUpdated(Document.Current, copy);
					}

					cmd.WhereAnd<Where<boolTrue, Equal<boolFalse>>>();
					break;
				default:
					throw new PXException();
			}

			foreach (INLotSerialStatus avail in cmd.Select(newline.InventoryID, newline.SubItemID, newline.SiteID))
			{
				LotSerialStatus accumavail = new LotSerialStatus();
				PXCache<INLotSerialStatus>.RestoreCopy(accumavail, avail);

				accumavail = (LotSerialStatus)this.Caches[typeof(LotSerialStatus)].Insert(accumavail);

				decimal? AvailableQty = avail.QtyHardAvail + accumavail.QtyHardAvail;

				if (AvailableQty <= 0m)
				{
					continue;
				}

				SOShipLineSplit newsplit = (SOShipLineSplit)newline;
				newsplit.UOM = null;
				newsplit.SplitLineNbr = null;
				newsplit.LocationID = avail.LocationID;
				newsplit.LotSerialNbr = avail.LotSerialNbr;
				newsplit.ExpireDate = avail.ExpireDate;

				if (AvailableQty < PlannedQty)
				{
					newsplit.Qty = AvailableQty;
					newsplit.BaseQty = null;
					splits.Insert(newsplit);

					PlannedQty -= AvailableQty;
				}
				else
				{
					newsplit.Qty = PlannedQty;
					newsplit.BaseQty = null;
					splits.Insert(newsplit);

					PlannedQty = 0m;
					break;
				}
			}

			return PlannedQty;
		}

		public decimal? ShipAvailableNonLots(SOShipmentPlan plan, SOShipLine newline, INLotSerClass lotserclass)
		{
			decimal? PlannedQty = plan.PlanQty;

			if (lotserclass.LotSerAssign == INLotSerAssign.WhenUsed && lotserclass.AutoNextNbr == false)
			{
				{
					SOShipLine copy = PXCache<SOShipLine>.CreateCopy(newline);
					newline.UnassignedQty = PlannedQty;
					newline.BaseQty = PlannedQty;
					newline.Qty = INUnitAttribute.ConvertFromBase(Transactions.Cache, newline.InventoryID, newline.UOM, (decimal)newline.BaseQty, INPrecision.QUANTITY);

					Transactions.Cache.RaiseRowUpdated(newline, copy);
				}

				if (Document.Current.Hold == false)
				{
					SOShipment copy = PXCache<SOShipment>.CreateCopy(Document.Current);
					Document.Cache.SetValueExt<SOOrder.hold>(Document.Current, true);
					Document.Cache.RaiseRowUpdated(Document.Current, copy);
				}

				return PlannedQty;
			}

			foreach (INLocationStatus avail in PXSelectReadonly2<INLocationStatus,
				InnerJoin<INLocation, On<INLocation.locationID, Equal<INLocationStatus.locationID>>>,
				Where<INLocationStatus.inventoryID, Equal<Required<INLocationStatus.inventoryID>>,
				And<INLocationStatus.subItemID, Equal<Required<INLocationStatus.subItemID>>,
				And<INLocationStatus.siteID, Equal<Required<INLocationStatus.siteID>>,
				And<INLocation.salesValid, Equal<boolTrue>>>>>,
				OrderBy<Asc<INLocation.pickPriority>>>.Select(this, newline.InventoryID, newline.SubItemID, newline.SiteID))
				{
				LocationStatus accumavail = new LocationStatus();
				PXCache<INLocationStatus>.RestoreCopy(accumavail, avail);

				accumavail = (LocationStatus)this.Caches[typeof(LocationStatus)].Insert(accumavail);

				decimal? AvailableQty = avail.QtyHardAvail + accumavail.QtyHardAvail;

				if (AvailableQty <= 0m)
				{
					continue;
				}

				SOShipLineSplit newsplit = (SOShipLineSplit)newline;
				newsplit.UOM = null;
				newsplit.SplitLineNbr = null;
				newsplit.LocationID = avail.LocationID;
				if (AvailableQty < PlannedQty)
				{
					if (lotserclass.LotSerTrack == INLotSerTrack.SerialNumbered)
					{
						newsplit.BaseQty = 1m;
						newsplit.Qty = 1m;

						for (int i = 0; i < (int)AvailableQty; i++)
						{
							splits.Insert(newsplit);
						}
					}
					else
					{
						newsplit.Qty = AvailableQty;
						newsplit.BaseQty = null;
						splits.Insert(newsplit);
					}
					PlannedQty -= AvailableQty;
				}
				else
				{
					if (lotserclass.LotSerTrack == INLotSerTrack.SerialNumbered)
					{
						newsplit.BaseQty = 1m;
						newsplit.Qty = 1m;

						for (int i = 0; i < (int)PlannedQty; i++)
						{
							splits.Insert(newsplit);
						}
					}
					else
					{
						newsplit.Qty = PlannedQty;
						newsplit.BaseQty = null;
						splits.Insert(newsplit);
					}
					PlannedQty = 0m;
					break;
				}
			}

			return PlannedQty;
		}

		public decimal? ShipNonStock(SOShipmentPlan plan, SOShipLine newline)
		{
			decimal? PlannedQty = plan.PlanQty;

			SOShipLineSplit newsplit = (SOShipLineSplit)newline;
			newsplit.UOM = null;
			newsplit.SplitLineNbr = null;
			newsplit.LocationID = null;
			newsplit.Qty = PlannedQty;
			newsplit.BaseQty = null;
			splits.Insert(newsplit);

			return 0m;
		}

		public decimal? ShipAvailable(SOShipmentPlan plan, SOShipLine newline, PXResult<InventoryItem, INLotSerClass> item)
		{
			INLotSerClass lotserclass = item;
			InventoryItem initem = item;

			if (initem.StkItem == false && initem.KitItem == true)
			{
				decimal? kitqty = plan.PlanQty;
				object lastComponentID = null;
				bool HasSerialComponents = false;
				SOShipLine copy;

				ShipNonStockKit(plan, newline, ref kitqty, ref lastComponentID, ref HasSerialComponents);

				copy = PXCache<SOShipLine>.CreateCopy(newline);
				copy.ShippedQty = INUnitAttribute.ConvertFromBase<SOShipLine.inventoryID>(Transactions.Cache, newline, newline.UOM, (decimal)kitqty, INPrecision.QUANTITY);
				lsselect.lastComponentID = (int?)lastComponentID;
				try
				{
					Transactions.Update(copy);
				}
				finally
				{
					lsselect.lastComponentID = null;
				}
				
				return 0m;
			}
			else if (lotserclass == null || lotserclass.LotSerTrack == null)
			{
				return ShipNonStock(plan, newline);
			}
			else if (lotserclass.LotSerTrack == INLotSerTrack.NotNumbered || lotserclass.LotSerAssign == INLotSerAssign.WhenUsed)
			{
				return ShipAvailableNonLots(plan, newline, lotserclass);
			}
			else
			{
				return ShipAvailableLots(plan, newline, lotserclass);
			}
		}

		public virtual void PromptReplenishment(PXCache sender, SOShipLine newline)
		{
			foreach (INLocationStatus stat in PXSelectReadonly2<INLocationStatus, InnerJoin<INLocation, On<INLocation.locationID, Equal<INLocationStatus.locationID>>>, Where<INLocationStatus.inventoryID, Equal<Current<SOShipLine.inventoryID>>, And<INLocationStatus.siteID, Equal<Current<SOShipLine.siteID>>, And<INLocation.inclQtyAvail, Equal<boolTrue>, And<INLocation.salesValid, Equal<boolFalse>, And<INLocationStatus.qtyHardAvail, Greater<decimal0>>>>>>>.SelectSingleBound(this, new object[] { newline }))
			{
				throw new PXException(Messages.PromptReplenishment, sender.GetValueExt<SOShipLine.inventoryID>(newline));
			}
		}

		public virtual void UpdateKitQtyFromUnshippedComponents(PXCache cache, SOShipLine newline)
		{
			if (newline.HasKitComponents == true)
			{
				decimal? KitQty = newline.BaseShippedQty;
                object lastComponentID = null;

				foreach (KeyValuePair<SOShipLine.KitComponentKey, decimal?> pair in newline.Unshipped)
				{
					decimal? PlannedQty = newline.Planned[pair.Key];
					decimal? UnshippedQty = pair.Value;

					if (PlannedQty != 0m)
					{
						if ((PlannedQty - UnshippedQty) / PlannedQty * newline.BaseShippedQty < KitQty)
						{
							KitQty = (PlannedQty - UnshippedQty) / PlannedQty * newline.BaseShippedQty;

                            lastComponentID = pair.Key.ItemID;
						}
					}
				}

				if (newline.HasSerialComponents == true)
				{
					KitQty = decimal.Floor((decimal)KitQty);
				}

                if (KitQty <= 0m && lastComponentID != null)
                {
                    Transactions.Cache.RaiseFieldSelecting<SOShipLine.inventoryID>(newline, ref lastComponentID, true);
                    PXTrace.WriteInformation(Messages.ItemNotAvailableTraced, lastComponentID, Transactions.GetValueExt<SOShipLine.siteID>(newline));
                }

				SOShipLine copy = PXCache<SOShipLine>.CreateCopy(newline);
				copy.ShippedQty = INUnitAttribute.ConvertFromBase<SOShipLine.inventoryID>(cache, copy, copy.UOM, (decimal)KitQty, INPrecision.QUANTITY);
				cache.Update(copy);
			}
		}

		public virtual void ShipNonStockKit(SOShipmentPlan plan, SOShipLine newline, ref decimal? kitqty, ref object lastComponentID, ref bool HasSerialComponents)
		{
			SOShipLine copy;
			foreach (PXResult<INKitSpecStkDet, InventoryItem, INLotSerClass> compres in 
				PXSelectJoin<INKitSpecStkDet, 
				InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INKitSpecStkDet.compInventoryID>>, 
				InnerJoin<INLotSerClass, On<INLotSerClass.lotSerClassID, Equal<InventoryItem.lotSerClassID>>>>, 
				Where<INKitSpecStkDet.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>>>.Select(this, newline.InventoryID))
			{
				INKitSpecStkDet compitem = (INKitSpecStkDet)compres;

				copy = PXCache<SOShipLine>.CreateCopy(newline);

				copy.InventoryID = compitem.CompInventoryID;
				copy.SubItemID = compitem.CompSubItemID;
				copy.UOM = compitem.UOM;
				copy.Qty = compitem.DfltCompQty * plan.PlanQty;

				//clear splits with correct ComponentID
				lsselect.RaiseRowDeleted(Transactions.Cache, copy);

				SOShipmentPlan plancopy = PXCache<SOShipmentPlan>.CreateCopy(plan);
				plancopy.PlanQty = INUnitAttribute.ConvertToBase<SOShipLine.inventoryID>(Transactions.Cache, copy, copy.UOM, (decimal)copy.Qty, INPrecision.QUANTITY);

				decimal? unshippedqty = ShipAvailable(plancopy, copy, new PXResult<InventoryItem, INLotSerClass>(compres, compres));

				if (plancopy.PlanQty != 0m && (plancopy.PlanQty - unshippedqty) / plancopy.PlanQty * plan.PlanQty < kitqty)
				{
					kitqty = (plancopy.PlanQty - unshippedqty) / plancopy.PlanQty * plan.PlanQty;
					lastComponentID = copy.InventoryID;
				}

				HasSerialComponents |= ((INLotSerClass)compres).LotSerTrack == INLotSerTrack.SerialNumbered;
			}

			foreach (PXResult<INKitSpecNonStkDet, InventoryItem> compres in PXSelectJoin<INKitSpecNonStkDet,
				InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INKitSpecNonStkDet.compInventoryID>>>,
				Where<INKitSpecNonStkDet.kitInventoryID, Equal<Required<INKitSpecNonStkDet.kitInventoryID>>,
					And<Where<InventoryItem.kitItem, Equal<True>, Or<InventoryItem.nonStockShip, Equal<True>>>>>>.Select(this, newline.InventoryID))
			{
				INKitSpecNonStkDet compitem = compres;
				InventoryItem item = compres;

				copy = PXCache<SOShipLine>.CreateCopy(newline);

				copy.InventoryID = compitem.CompInventoryID;
				copy.SubItemID = null;
				copy.UOM = compitem.UOM;
				copy.Qty = compitem.DfltCompQty * plan.PlanQty;

				//clear splits with correct ComponentID
				lsselect.RaiseRowDeleted(Transactions.Cache, copy);

				SOShipmentPlan plancopy = PXCache<SOShipmentPlan>.CreateCopy(plan);
				plancopy.PlanQty = INUnitAttribute.ConvertToBase<SOShipLine.inventoryID>(Transactions.Cache, copy, copy.UOM, (decimal)copy.Qty, INPrecision.QUANTITY);

				if (item.StkItem == false && item.KitItem == true)
				{
					decimal? subkitqty = plancopy.PlanQty;

					ShipNonStockKit(plancopy, copy, ref subkitqty, ref lastComponentID, ref HasSerialComponents);

					if (plancopy.PlanQty != 0m && subkitqty / plancopy.PlanQty * plan.PlanQty < kitqty)
					{
						kitqty = subkitqty / plancopy.PlanQty * plan.PlanQty;
					}
				}
				else
				{
					ShipAvailable(plancopy, copy, new PXResult<InventoryItem, INLotSerClass>(compres, null));
				}
			}

			if (HasSerialComponents)
			{
				kitqty = decimal.Floor((decimal)kitqty);
			}

			if (kitqty <= 0m && lastComponentID != null)
			{
				object lastComponentCD = lastComponentID;
				Transactions.Cache.RaiseFieldSelecting<SOShipLine.inventoryID>(newline, ref lastComponentCD, true);
				PXTrace.WriteInformation(Messages.ItemNotAvailableTraced, lastComponentCD, Transactions.GetValueExt<SOShipLine.siteID>(newline));
			}
		}

		public virtual void CreateShipmentFromSchedules(PXResult<SOShipmentPlan, SOLine, InventoryItem, INLotSerClass, INSite> res, ref SOShipLine newline, SOOrderType ordertype, string operation, DocumentList<SOShipment> list)
		{
			SOShipmentPlan plan = (SOShipmentPlan)res;
			SOLine line = (SOLine)res;
			INSite site = (INSite)res;

			if (plan.Selected == true || list != null && plan.InclQtySOBackOrdered == 0)
			{
				newline.OrigOrderType = line.OrderType;
				newline.OrigOrderNbr = line.OrderNbr;
				newline.OrigLineNbr = line.LineNbr;
				newline.InventoryID = line.InventoryID;
				newline.SubItemID = line.SubItemID;
				newline.SiteID = line.SiteID;
				newline.TranDesc = line.TranDesc;
				newline.CustomerID = line.CustomerID;
				newline.InvtMult = line.InvtMult;
				newline.Operation = line.Operation;
				newline.LineType = line.LineType;
				newline.ReasonCode = line.ReasonCode;
				newline.ProjectID = line.ProjectID;
				newline.TaskID = line.TaskID;
				newline.UOM = line.UOM;
				newline.IsFree = line.IsFree;
				newline.ManualDisc = line.ManualDisc;

                newline.DiscountID = line.DiscountID;
                newline.DiscountSequenceID = line.DiscountSequenceID;

				newline.DetDiscIDC1 = line.DetDiscIDC1;
				newline.DetDiscIDC2 = line.DetDiscIDC2;
				newline.DetDiscSeqIDC1 = line.DetDiscSeqIDC1;
				newline.DetDiscSeqIDC2 = line.DetDiscSeqIDC2;
				newline.DetDiscApp = line.DetDiscApp;
				newline.DocDiscIDC1 = line.DocDiscIDC1;
				newline.DocDiscIDC2 = line.DocDiscIDC2;
				newline.DocDiscSeqIDC1 = line.DocDiscSeqIDC1;
				newline.DocDiscSeqIDC2 = line.DocDiscSeqIDC2;

				if (((INLotSerClass)res).LotSerTrack == null)
				{
					newline.ShippedQty = INUnitAttribute.ConvertFromBase<SOShipLine.inventoryID>(Transactions.Cache, newline, newline.UOM, (decimal)plan.PlanQty, INPrecision.QUANTITY); ;
					newline = Transactions.Insert(newline);
					//clear splits
					lsselect.RaiseRowDeleted(Transactions.Cache, newline);

					ShipAvailable(plan, newline, new PXResult<InventoryItem, INLotSerClass>(res, res));
				}
				else if (operation == SOOperation.Receipt)
				{
					newline.ShippedQty = INUnitAttribute.ConvertFromBase(Transactions.Cache, newline.InventoryID, newline.UOM, (decimal)plan.PlanQty, INPrecision.QUANTITY);
					newline.LocationID = site.ReturnLocationID;
					if (newline.LocationID == null && list != null)
						throw new PXException(Messages.NoRMALocation, site.SiteCD);
					newline = Transactions.Insert(newline);
				}
				else
				{
					newline.ShippedQty = 0m;
					newline = Transactions.Insert(newline);
					//clear splits
					lsselect.RaiseRowDeleted(Transactions.Cache, newline);

					ShipAvailable(plan, newline, new PXResult<InventoryItem,INLotSerClass>(res, res));

				}

				if (newline.BaseShippedQty < plan.PlanQty)
				{
					PromptReplenishment(Transactions.Cache, newline);
				}

				if (ordertype.CopyLineNotesToShipment == true)
					PXNoteAttribute.SetNote(Caches[typeof(SOShipLine)], newline, PXNoteAttribute.GetNote(Caches[typeof(SOLine)], line));
				if (ordertype.CopyLineFilesToShipment == true)
					PXNoteAttribute.SetFileNotes(Caches[typeof(SOShipLine)], newline, PXNoteAttribute.GetFileNotes(Caches[typeof(SOLine)], line));

				if (newline.ShippedQty == 0m || newline.BaseShippedQty < plan.PlanQty * line.CompleteQtyMin / 100m && line.ShipComplete == SOShipComplete.ShipComplete)
				{
					if (list != null && sosetup.Current.AddAllToShipment == false)
					{
                        PXTrace.WriteInformation(Messages.ItemNotAvailableTraced, Transactions.GetValueExt<SOShipLine.inventoryID>(newline), Transactions.GetValueExt<SOShipLine.siteID>(newline));
						Transactions.Delete(newline);
					}
					else
					{
						Transactions.Cache.RaiseExceptionHandling<SOShipLine.shippedQty>(newline, null, new PXSetPropertyException(Messages.ItemNotAvailable, PXErrorLevel.RowWarning));
					}
				}
			}
		}
				
		public virtual void CreateShipment(SOOrder order, int? SiteID, DateTime? ShipDate, bool? useOptimalShipDate, string operation, DocumentList<SOShipment> list)
		{
			SOOrderType ordertype = PXSelect<SOOrderType, Where<SOOrderType.orderType, Equal<Required<SOOrder.orderType>>>>.Select(this, order.OrderType);
			SOShipment newdoc;
			if (operation == null)
				operation = ordertype.DefaultOperation;
			SOOrderTypeOperation orderoperation =
				PXSelect<SOOrderTypeOperation,
				Where<SOOrderTypeOperation.orderType, Equal<Required<SOOrderTypeOperation.orderType>>,
					And<SOOrderTypeOperation.operation, Equal<Required<SOOrderTypeOperation.operation>>>>>.Select(this, order.OrderType, operation);
			
			if(useOptimalShipDate == true)
			{
				SOShipmentPlan plan =
					order.ShipComplete == SOShipComplete.BackOrderAllowed
						? PXSelectJoinGroupBy<SOShipmentPlan,
						  	InnerJoin<SOLine, On<SOLine.planID, Equal<SOShipmentPlan.planID>>>,
							Where<SOShipmentPlan.siteID, Equal<Required<SOOrderFilter.siteID>>,
						  	And<SOShipmentPlan.orderType, Equal<Required<SOOrder.orderType>>,
						  	And<SOShipmentPlan.orderNbr, Equal<Required<SOOrder.orderNbr>>,
						  	And<SOLine.operation, Equal<Required<SOLine.operation>>>>>>,
						  Aggregate<Min<SOShipmentPlan.planDate>>>.Select(this, SiteID, order.OrderType, order.OrderNbr, operation)
						: PXSelectJoinGroupBy<SOShipmentPlan,
						  	InnerJoin<SOLine, On<SOLine.planID, Equal<SOShipmentPlan.planID>>>,
						  Where<SOShipmentPlan.siteID, Equal<Required<SOOrderFilter.siteID>>,
						  	And<SOShipmentPlan.orderType, Equal<Required<SOOrder.orderType>>,
						  	And<SOShipmentPlan.orderNbr, Equal<Required<SOOrder.orderNbr>>,
						  	And<SOLine.operation, Equal<Required<SOLine.operation>>>>>>,
						  Aggregate<Max<SOShipmentPlan.planDate>>>.Select(this, SiteID, order.OrderType, order.OrderNbr, operation);
				
				if (plan.PlanDate > ShipDate)
					ShipDate = plan.PlanDate;
			}
			if (list != null)
			{
				this.Clear(PXClearOption.PreserveTimeStamp);

				if (((SOOrder)order).ShipSeparately == false)
				{
					newdoc = list.Find
						<SOShipment.customerID, SOShipment.shipDate, SOShipment.shipAddressID, SOShipment.shipContactID, SOShipment.siteID, SOShipment.fOBPoint, SOShipment.shipVia, SOShipment.shipTermsID, SOShipment.shipZoneID, SOShipment.shipmentType, SOShipment.hidden>
						(order.CustomerID, ShipDate, order.ShipAddressID, order.ShipContactID, SiteID, order.FOBPoint, order.ShipVia, order.ShipTermsID, order.ShipZoneID, INTranType.DocType(orderoperation.INDocType), false) ?? new SOShipment();
				}
				else
				{
					newdoc = new SOShipment();
					newdoc.Hidden = true;
				}

				if (newdoc.ShipmentNbr != null)
				{
					Document.Current = Document.Search<SOShipment.shipmentNbr>(newdoc.ShipmentNbr);
				}
				else
				{

					newdoc = Document.Insert(newdoc);
					newdoc.CustomerID = order.CustomerID;
					newdoc.CustomerLocationID = order.CustomerLocationID;
					newdoc.SiteID = SiteID;
					newdoc.ShipVia = order.ShipVia;
					newdoc.UseCustomerAccount = order.UseCustomerAccount;
					newdoc.Resedential = order.Resedential;
					newdoc.SaturdayDelivery = order.SaturdayDelivery;
					newdoc.Insurance = order.Insurance;
					newdoc.GroundCollect = order.GroundCollect;
					newdoc.FOBPoint = order.FOBPoint;
					newdoc.ShipTermsID = order.ShipTermsID;
					newdoc.ShipZoneID = order.ShipZoneID;
					newdoc.TaxCategoryID = order.FreightTaxCategoryID;
					newdoc.Operation = operation;
					newdoc.ShipmentType = INTranType.DocType(orderoperation.INDocType);
					newdoc.DestinationSiteID = order.DestinationSiteID;
					newdoc.ShipDate = ShipDate;
					newdoc = Document.Update(newdoc);
					foreach (SOShipmentAddress address in this.Shipping_Address.Select())
						if (address.AddressID < 0)
							Shipping_Address.Delete(address);

					foreach (SOShipmentContact contact in this.Shipping_Contact.Select())
						if (contact.ContactID < 0)
							Shipping_Contact.Delete(contact);
					newdoc.ShipAddressID = order.ShipAddressID;
					newdoc.ShipContactID = order.ShipContactID;
					newdoc = Document.Update(newdoc);
					newdoc = Document.Search<SOShipment.shipmentNbr>(newdoc.ShipmentNbr);
				}			
			}
			else
			{
				newdoc = PXCache<SOShipment>.CreateCopy(Document.Current);
				newdoc.CustomerID = order.CustomerID;
				newdoc.CustomerLocationID = order.CustomerLocationID;
				newdoc.DestinationSiteID = order.DestinationSiteID;

				foreach (SOShipmentAddress address in this.Shipping_Address.Select())
				{
					if (address.AddressID < 0)
					{
						Shipping_Address.Delete(address);
					}
				}

				foreach (SOShipmentContact contact in this.Shipping_Contact.Select())
				{
					if (contact.ContactID < 0)
					{
						Shipping_Contact.Delete(contact);
					}
				}

				newdoc.ShipAddressID = order.ShipAddressID;
				newdoc.ShipContactID = order.ShipContactID;
				newdoc.SiteID = SiteID;
				newdoc.ShipVia = order.ShipVia;
				newdoc.UseCustomerAccount = order.UseCustomerAccount;
				newdoc.Resedential = order.Resedential;
				newdoc.SaturdayDelivery = order.SaturdayDelivery;
				newdoc.Insurance = order.Insurance;
				newdoc.GroundCollect = order.GroundCollect;
				newdoc.FOBPoint = order.FOBPoint;
				newdoc.ShipTermsID = order.ShipTermsID;
				newdoc.ShipZoneID = order.ShipZoneID;
				newdoc.TaxCategoryID = order.FreightTaxCategoryID;
				newdoc.Operation = operation;
				newdoc.ShipmentType = INTranType.DocType(orderoperation.INDocType);
				newdoc.DestinationSiteID = order.DestinationSiteID;
				newdoc.ShipDate = ShipDate;

				Document.Update(newdoc);
			}

			SOOrderShipment neworder = new SOOrderShipment();
			neworder.OrderType = order.OrderType;
			neworder.OrderNbr = order.OrderNbr;
			neworder.NoteID = order.NoteID;
			neworder.ShipmentNbr = Document.Current.ShipmentNbr;
			neworder.ShipmentType = Document.Current.ShipmentType;
			neworder.Operation = Document.Current.Operation;
			neworder.OrigPOType = order.OrigPOType;
			neworder.OrigPONbr = order.OrigPONbr;

			if (ordershipment.View.SelectSingleBound(new object[] { neworder }) == null)
			{
				neworder = OrderList.Insert(neworder);
			}

			PXRowDeleting SOOrderShipment_RowDeleting = delegate(PXCache sender, PXRowDeletingEventArgs e)
			{
				e.Cancel = true;
			};

			this.RowDeleting.AddHandler<SOOrderShipment>(SOOrderShipment_RowDeleting);

            bool anydeleted = false;
            PXRowDeleted SOShipLine_RowDeleted = delegate(PXCache sender, PXRowDeletedEventArgs e)
            {
                anydeleted = true;
            };

            this.RowDeleted.AddHandler<SOShipLine>(SOShipLine_RowDeleted);

			SOShipLine newline = null;

			foreach (PXResult<SOShipmentPlan, SOLine, InventoryItem, INLotSerClass, INSite, SOShipLine> res in
				PXSelectJoin<SOShipmentPlan,
			InnerJoin<SOLine, On<SOLine.planID, Equal<SOShipmentPlan.planID>>,
			InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<SOShipmentPlan.inventoryID>>,
			LeftJoin<INLotSerClass, On<INLotSerClass.lotSerClassID, Equal<InventoryItem.lotSerClassID>>,
			LeftJoin<INSite, On<INSite.siteID, Equal<SOLine.siteID>>,
			LeftJoin<SOShipLine,
						On<SOShipLine.origOrderType, Equal<SOLine.orderType>,
						And<SOShipLine.origOrderNbr, Equal<SOLine.orderNbr>,
						And<SOShipLine.origLineNbr, Equal<SOLine.lineNbr>,
						And<SOShipLine.confirmed, Equal<boolFalse>,
						And<SOShipLine.shipmentNbr, NotEqual<Current<SOShipment.shipmentNbr>>>>>>>>>>>>,
			Where<SOShipmentPlan.siteID, Equal<Optional<SOOrderFilter.siteID>>,
			And<SOShipmentPlan.planDate, LessEqual<Optional<SOOrderFilter.endDate>>,
			And<SOShipmentPlan.orderType, Equal<Required<SOOrder.orderType>>,
			And<SOShipmentPlan.orderNbr, Equal<Required<SOOrder.orderNbr>>,
			And<SOLine.operation, Equal<Required<SOLine.operation>>,
			And<SOShipLine.origOrderNbr, IsNull>>>>>>>.Select(this, SiteID, ShipDate, order.OrderType, order.OrderNbr, operation))
			{
				newline = new SOShipLine();
				CreateShipmentFromSchedules(res, ref newline, ordertype, operation, list);
			}

			newline = null;

			foreach (PXResult<SOShipmentPlan, SOLineSplit, SOLine, InventoryItem, INLotSerClass, INSite, SOShipLine> res in
				PXSelectJoin<SOShipmentPlan,
			InnerJoin<SOLineSplit, On<SOLineSplit.planID, Equal<SOShipmentPlan.planID>>,
			InnerJoin<SOLine, On<SOLine.orderType, Equal<SOLineSplit.orderType>, And<SOLine.orderNbr, Equal<SOLineSplit.orderNbr>, And<SOLine.lineNbr, Equal<SOLineSplit.lineNbr>>>>,
			InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<SOShipmentPlan.inventoryID>>,
			LeftJoin<INLotSerClass, On<INLotSerClass.lotSerClassID, Equal<InventoryItem.lotSerClassID>>,
			LeftJoin<INSite, On<INSite.siteID, Equal<SOLine.siteID>>,
			LeftJoin<SOShipLine,
						On<SOShipLine.origOrderType, Equal<SOLine.orderType>,
						And<SOShipLine.origOrderNbr, Equal<SOLine.orderNbr>,
						And<SOShipLine.origLineNbr, Equal<SOLine.lineNbr>,
						And<SOShipLine.confirmed, Equal<boolFalse>,
						And<SOShipLine.shipmentNbr, NotEqual<Current<SOShipment.shipmentNbr>>>>>>>>>>>>>,
			Where<SOShipmentPlan.siteID, Equal<Optional<SOOrderFilter.siteID>>,
			And<SOShipmentPlan.planDate, LessEqual<Optional<SOOrderFilter.endDate>>,
			And<SOShipmentPlan.orderType, Equal<Required<SOOrder.orderType>>,
			And<SOShipmentPlan.orderNbr, Equal<Required<SOOrder.orderNbr>>,
			And<SOLine.operation, Equal<Required<SOLine.operation>>,
			And<SOShipLine.origOrderNbr, IsNull,
			And<SOShipmentPlan.requireAllocation, Equal<True>, 
			And<Where<SOShipmentPlan.inclQtySOShipping, Equal<True>, Or<SOShipmentPlan.inclQtySOShipped, Equal<True>>>>>>>>>>>>.Select(this, SiteID, ShipDate, order.OrderType, order.OrderNbr, operation))
			{
				newline = new SOShipLine();
				newline.OrigSplitLineNbr = ((SOLineSplit)res).SplitLineNbr;
				CreateShipmentFromSchedules(new PXResult<SOShipmentPlan, SOLine, InventoryItem, INLotSerClass, INSite>(res, res, res, res, res), ref newline, ordertype, operation, list);
			}

			SOLine prev_line = null;
			newline = null;

			//if WhseLoc is required in OrderType no lines can have ShipComplete=BackOrderAllowed
			foreach (PXResult<SOShipmentPlan, SOLineSplit, SOLine, SOShipLine, InventoryItem, INLotSerClass> res in PXSelectJoin<SOShipmentPlan,
			InnerJoin<SOLineSplit, On<SOLineSplit.planID, Equal<SOShipmentPlan.planID>>,
			InnerJoin<SOLine, On<SOLine.orderType, Equal<SOLineSplit.orderType>, And<SOLine.orderNbr, Equal<SOLineSplit.orderNbr>, And<SOLine.lineNbr, Equal<SOLineSplit.lineNbr>>>>,
			LeftJoin<SOShipLine, On<SOShipLine.origOrderType, Equal<SOLine.orderType>, And<SOShipLine.origOrderNbr, Equal<SOLine.orderNbr>, And<SOShipLine.origLineNbr, Equal<SOLine.lineNbr>, And<SOShipLine.confirmed, Equal<boolFalse>, And<SOShipLine.shipmentNbr, NotEqual<Current<SOShipment.shipmentNbr>>>>>>>,
			InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<SOShipmentPlan.inventoryID>>,
			InnerJoin<INLotSerClass, On<INLotSerClass.lotSerClassID, Equal<InventoryItem.lotSerClassID>>>>>>>,
			Where<SOShipmentPlan.siteID, Equal<Optional<SOOrderFilter.siteID>>,
			And<SOShipmentPlan.planDate, LessEqual<Optional<SOOrderFilter.endDate>>,
			And<SOShipmentPlan.orderType, Equal<Required<SOOrder.orderType>>, 
			And<SOShipmentPlan.orderNbr, Equal<Required<SOOrder.orderNbr>>,
			And<SOShipmentPlan.requireAllocation, Equal<False>,
			And<SOLine.operation, Equal<Required<SOLine.operation>>,
			And<SOShipLine.origOrderNbr, IsNull>>>>>>>, 
			OrderBy<Asc<SOLineSplit.lineNbr>>>.Select(this, SiteID, ShipDate, order.OrderType, order.OrderNbr, operation))
			{
				SOShipmentPlan plan = (SOShipmentPlan)res;
				SOLineSplit split = (SOLineSplit)res;
				SOLine line = (SOLine)res;

				if (plan.Selected == true || list != null && plan.InclQtySOBackOrdered == 0)
				{
					if (Caches[typeof(SOLine)].ObjectsEqual(prev_line, line) == false)
					{
						if (newline != null)
						{
							UpdateKitQtyFromUnshippedComponents(Transactions.Cache, newline);

							if (newline.ShippedQty == 0m || newline.BaseShippedQty < prev_line.BaseOpenQty * prev_line.CompleteQtyMin / 100m && prev_line.ShipComplete == SOShipComplete.ShipComplete)
							{
								if (list != null && sosetup.Current.AddAllToShipment == false)
								{
                                    PXTrace.WriteInformation(Messages.ItemNotAvailableTraced, Transactions.GetValueExt<SOShipLine.inventoryID>(newline), Transactions.GetValueExt<SOShipLine.siteID>(newline));
                                    Transactions.Delete(newline);
								}
								else
								{
									Transactions.Cache.RaiseExceptionHandling<SOShipLine.shippedQty>(newline, null, new PXSetPropertyException(Messages.ItemNotAvailable, PXErrorLevel.RowWarning));
								}
							}
						}
						newline = new SOShipLine();
						newline.OrigOrderType = line.OrderType;
						newline.OrigOrderNbr = line.OrderNbr;
						newline.OrigLineNbr = line.LineNbr;
						newline.InventoryID = line.InventoryID;
						newline.SubItemID = line.SubItemID;
						newline.SiteID = line.SiteID;
						newline.TranDesc = line.TranDesc;
						newline.CustomerID = line.CustomerID;
						newline.InvtMult = line.InvtMult;
						newline.Operation = line.Operation;
						newline.LineType = line.LineType;
						newline.ReasonCode = line.ReasonCode;
						newline.ProjectID = line.ProjectID;
						newline.TaskID = line.TaskID;
						newline.UOM = line.UOM;
						newline.HasKitComponents = !object.Equals(line.InventoryID, split.InventoryID);
						newline.ShippedQty = newline.HasKitComponents == true ? line.OrderQty : 0m;
						newline.BaseShippedQty = newline.HasKitComponents == true ? line.BaseOrderQty : 0m;
						newline.IsFree = line.IsFree;
						newline.ManualDisc = line.ManualDisc;

                        newline.DiscountID = line.DiscountID;
                        newline.DiscountSequenceID = line.DiscountSequenceID;

						newline.DetDiscIDC1 = line.DetDiscIDC1;
						newline.DetDiscIDC2 = line.DetDiscIDC2;
						newline.DetDiscSeqIDC1 = line.DetDiscSeqIDC1;
						newline.DetDiscSeqIDC2 = line.DetDiscSeqIDC2;
						newline.DetDiscApp = line.DetDiscApp;
						newline.DocDiscIDC1 = line.DocDiscIDC1;
						newline.DocDiscIDC2 = line.DocDiscIDC2;
						newline.DocDiscSeqIDC1 = line.DocDiscSeqIDC1;
						newline.DocDiscSeqIDC2 = line.DocDiscSeqIDC2;

						newline = Transactions.Insert(newline);

						if (ordertype.CopyLineNotesToShipment == true)
							PXNoteAttribute.SetNote(Caches[typeof(SOShipLine)], newline, PXNoteAttribute.GetNote(Caches[typeof(SOLine)], line));
						if (ordertype.CopyLineFilesToShipment == true)
							PXNoteAttribute.SetFileNotes(Caches[typeof(SOShipLine)], newline, PXNoteAttribute.GetFileNotes(Caches[typeof(SOLine)], line));

						//clear splits
						lsselect.RaiseRowDeleted(Transactions.Cache, newline);
					}

					prev_line = line;

					SOShipLineSplit newsplit = (SOShipLineSplit)newline;
					newsplit.SplitLineNbr = null;
					newsplit.InventoryID = split.InventoryID;
					newsplit.SubItemID = split.SubItemID;
					newsplit.LocationID = split.LocationID;
					newsplit.LotSerialNbr = split.LotSerialNbr;
					newsplit.ExpireDate = split.ExpireDate;
					newsplit.UOM = split.UOM;

					decimal? PlannedQty = plan.PlanQty;
					SOShipLine.KitComponentKey PlannedKey = new SOShipLine.KitComponentKey(plan.InventoryID, plan.SubItemID);

					if (newline.Planned.ContainsKey(PlannedKey))
					{
						newline.Planned[PlannedKey] += PlannedQty;
					}
					else
					{
						newline.Planned[PlannedKey] = PlannedQty;

						if (newline.HasKitComponents == true)
						{
							//clear splits with correct ComponentID
							lsselect.RaiseRowDeleted(Transactions.Cache, newsplit);
							newline.HasSerialComponents |= ((INLotSerClass)res).LotSerTrack == INLotSerTrack.SerialNumbered;
						}
					}

					if (operation == SOOperation.Receipt || INItemPlanIDAttribute.GetInclQtyHardAvail<LocationStatus>(this.Caches[typeof(SOLineSplit)], split) != 0m)
					{
						newsplit.Qty = split.Qty;
						newsplit.BaseQty = null;

						splits.Insert(newsplit);
					}
					else if (((INLotSerClass)res).LotSerTrack == INLotSerTrack.NotNumbered || ((INLotSerClass)res).LotSerAssign == INLotSerAssign.WhenUsed)
					{
						foreach (INLocationStatus avail in PXSelectReadonly<INLocationStatus, Where<INLocationStatus.inventoryID, Equal<Required<INLocationStatus.inventoryID>>, And<INLocationStatus.subItemID, Equal<Required<INLocationStatus.subItemID>>, And<INLocationStatus.siteID, Equal<Required<INLocationStatus.siteID>>, And<INLocationStatus.locationID, Equal<Required<INLocationStatus.locationID>>>>>>>.Select(this, newsplit.InventoryID, newsplit.SubItemID, newsplit.SiteID, newsplit.LocationID))
						{
							LocationStatus accumavail = new LocationStatus();
							PXCache<INLocationStatus>.RestoreCopy(accumavail, avail);

							accumavail = (LocationStatus)this.Caches[typeof(LocationStatus)].Insert(accumavail);

							decimal? AvailableQty = avail.QtyHardAvail + accumavail.QtyHardAvail;

							if (AvailableQty <= 0m)
							{
								if (newline.Unshipped.ContainsKey(PlannedKey))
								{
									newline.Unshipped[PlannedKey] += PlannedQty;
								}
								else
								{
									newline.Unshipped[PlannedKey] = PlannedQty;
								}

								continue;
							}

							if (AvailableQty <= PlannedQty)
							{
								newsplit.Qty = INUnitAttribute.ConvertFromBase(Transactions.Cache, newline.InventoryID, newsplit.UOM, (decimal)AvailableQty, INPrecision.QUANTITY);

								if (newline.Unshipped.ContainsKey(PlannedKey))
								{
									newline.Unshipped[PlannedKey] += (PlannedQty - AvailableQty);
								}
								else
								{
									newline.Unshipped[PlannedKey] = (PlannedQty - AvailableQty);
								}
							}
							else
							{
								newsplit.Qty = INUnitAttribute.ConvertFromBase(Transactions.Cache, newline.InventoryID, newsplit.UOM, (decimal)PlannedQty, INPrecision.QUANTITY);
							}
							newsplit.BaseQty = null;

							splits.Insert(newsplit);
							break;
						}
					}
					else
					{
						foreach (INLotSerialStatus avail in PXSelectReadonly<INLotSerialStatus, Where<INLotSerialStatus.inventoryID, Equal<Required<INLotSerialStatus.inventoryID>>, And<INLotSerialStatus.subItemID, Equal<Required<INLotSerialStatus.subItemID>>, And<INLotSerialStatus.siteID, Equal<Required<INLotSerialStatus.siteID>>, And<INLotSerialStatus.locationID, Equal<Required<INLotSerialStatus.locationID>>, And<INLotSerialStatus.lotSerialNbr, Equal<Required<INLotSerialStatus.lotSerialNbr>>>>>>>>.Select(this, newsplit.InventoryID, newsplit.SubItemID, newsplit.SiteID, newsplit.LocationID, newsplit.LotSerialNbr))
						{
							LotSerialStatus accumavail = new LotSerialStatus();
							PXCache<INLotSerialStatus>.RestoreCopy(accumavail, avail);

							accumavail = (LotSerialStatus)this.Caches[typeof(LotSerialStatus)].Insert(accumavail);

							decimal? AvailableQty = avail.QtyHardAvail + accumavail.QtyHardAvail;

							if (AvailableQty <= 0m)
							{
								if (newline.Unshipped.ContainsKey(PlannedKey))
								{
									newline.Unshipped[PlannedKey] += PlannedQty;
								}
								else
								{
									newline.Unshipped[PlannedKey] = PlannedQty;
								}
								continue;
							}

							if (AvailableQty <= PlannedQty)
							{
								newsplit.Qty = INUnitAttribute.ConvertFromBase(Transactions.Cache, newline.InventoryID, newsplit.UOM, (decimal)AvailableQty, INPrecision.QUANTITY);

								if (newline.Unshipped.ContainsKey(PlannedKey))
								{
									newline.Unshipped[PlannedKey] += (PlannedQty - AvailableQty);
								}
								else
								{
									newline.Unshipped[PlannedKey] = (PlannedQty - AvailableQty);
								}
							}
							else
							{
								newsplit.Qty = INUnitAttribute.ConvertFromBase(Transactions.Cache, newline.InventoryID, newsplit.UOM, (decimal)PlannedQty, INPrecision.QUANTITY);
							}
							newsplit.BaseQty = null;

							splits.Insert(newsplit);
							break;
						}
					}
				}
			}

			if (newline != null)
			{
				UpdateKitQtyFromUnshippedComponents(Transactions.Cache, newline);

				if (newline.ShippedQty == 0m || newline.BaseShippedQty < prev_line.BaseOpenQty * prev_line.CompleteQtyMin / 100m && prev_line.ShipComplete == SOShipComplete.ShipComplete)
				{
					if (list != null && sosetup.Current.AddAllToShipment == false)
					{
                        PXTrace.WriteInformation(Messages.ItemNotAvailableTraced, Transactions.GetValueExt<SOShipLine.inventoryID>(newline), Transactions.GetValueExt<SOShipLine.siteID>(newline));
                        Transactions.Delete(newline);
					}
					else
					{
						Transactions.Cache.RaiseExceptionHandling<SOShipLine.shippedQty>(newline, null, new PXSetPropertyException(Messages.ItemNotAvailable, PXErrorLevel.RowWarning));
					}
				}
			}

			AllocateDocumentFreeItems(order);
            AllocateGroupFreeItems(order);
			AllocateLineFreeItems(order);
			AdjustFreeItemLines();
			
			this.RowDeleting.RemoveHandler<SOOrderShipment>(SOOrderShipment_RowDeleting);
            this.RowDeleted.RemoveHandler<SOShipLine>(SOShipLine_RowDeleted);

			foreach (SOOrderShipment item in OrderList.Cache.Inserted)
			{
				if (list != null && item.LineCntr > 0 && item.ShipmentQty == 0m && sosetup.Current.AddAllToShipment == true && sosetup.Current.CreateZeroShipments != true)
				{
					throw new SOShipmentException(Messages.CannotShipTraced, item.OrderType, item.OrderNbr);
				}

				if (list != null && item.LineCntr == 0)
				{
                    if (anydeleted)
                    {
                        throw new SOShipmentException(Messages.CannotShipTraced, item.OrderType, item.OrderNbr);
                    }
                    else if (operation == SOOperation.Issue)
                    {
                        throw new SOShipmentException(Messages.NothingToShipTraced, item.OrderType, item.OrderNbr, item.ShipDate);
                    }
                    else
                    {
                        throw new SOShipmentException(Messages.NothingToReceiveTraced, item.OrderType, item.OrderNbr, item.ShipDate);
                    }
				}
			}

			if (list != null)
			{
				foreach (SOLine2 line in PXSelect<SOLine2, Where<SOLine2.orderType, Equal<Current<SOOrder.orderType>>, And<SOLine2.orderNbr, Equal<Current<SOOrder.orderNbr>>, And<SOLine2.siteID, Equal<Required<SOLine2.siteID>>, And<SOLine2.shipDate, LessEqual<Required<SOLine2.shipDate>>>>>>>.SelectMultiBound(this, new object[] { order }, SiteID, ShipDate))
				{
					if (line.LineType == "GI" && line.Cancelled == false && order.ShipComplete == SOShipComplete.ShipComplete && line.ShippedQty == 0m)
					{
                        throw new SOShipmentException(Messages.CannotShipCompleteTraced, order.OrderType, order.OrderNbr);
					}
				}

				this.Caches[typeof(SOOrder)].SetStatus(order, PXEntryStatus.Updated);

				if (OrderList.Select().Count > 0)
				{
					this.Save.Press();

					if (list.Find(Document.Current) == null)
					{
						list.Add(Document.Current);
					}
				}
				else
				{
					List<object> failed = new List<object>();
					failed.Add(order);
					PXAutomation.StorePersisted(this, typeof(SOOrder), failed);
				}
			}
		}

		

		public virtual void CorrectShipment(SOOrderEntry docgraph, SOShipment shiporder)
		{
			this.Clear();

			Document.Current = Document.Search<SOShipment.shipmentNbr>(shiporder.ShipmentNbr);
			Document.Current.Confirmed = false;
			//support for delayed workflow fills
			Document.Current.Status = shiporder.Status;
			Document.Cache.SetStatus(Document.Current, PXEntryStatus.Updated);
			Document.Cache.IsDirty = true;			
			this.lsselect.OverrideAdvancedAvailCheck(false);

			PXFormulaAttribute.SetAggregate<SOLine.completed>(docgraph.Transactions.Cache, null);

			//docgraph.FieldDefaulting.AddHandler<IN.Overrides.INDocumentRelease.SiteStatus.negAvailQty>((sender, e) => 
			//{
			//    e.NewValue = true;
			//    e.Cancel = true;
			//});

			using (PXConnectionScope cs = new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					List<object> persisted = new List<object>();

					foreach (PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact, SOOrderType> ordres in OrderList.Select())
					{
						SOOrderShipment order = ordres;
						SOOrderType ordertype = ordres;

						if (!string.IsNullOrEmpty(order.InvoiceNbr) && ordertype.ARDocType != ARDocType.NoUpdate || !string.IsNullOrEmpty(order.InvtRefNbr))
						{
							throw new PXException(Messages.ShipmentInvoicedCannotReopen, order.OrderType, order.OrderNbr);
						}
						if (((SOOrder)ordres).Cancelled == true)
						{
							throw new PXException(Messages.ShipmentCancelledCannotReopen, order.OrderType, order.OrderNbr);
						}


						docgraph.Clear();

						docgraph.Document.Current = docgraph.Document.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderType);
						docgraph.Document.Current.OpenShipmentCntr++;
						docgraph.Document.Current.Completed = false;
						docgraph.Document.Cache.SetStatus(docgraph.Document.Current, PXEntryStatus.Updated);

						docgraph.soordertype.Current.RequireControlTotal = false;

						order.Confirmed = false;
						this.OrderList.Cache.Update(order);

						if (docgraph.Document.Current.OpenShipmentCntr > 1)
						{
							foreach (SOOrderShipment shipment2 in PXSelect<SOOrderShipment, Where<SOOrderShipment.orderType, Equal<Current<SOOrderShipment.orderType>>, And<SOOrderShipment.orderNbr, Equal<Current<SOOrderShipment.orderNbr>>, And<SOOrderShipment.siteID, Equal<Current<SOOrderShipment.siteID>>, And<SOOrderShipment.shipmentNbr, Greater<Current<SOOrderShipment.shipmentNbr>>, And<SOOrderShipment.shipmentType, NotEqual<SOShipmentType.dropShip>>>>>>>.SelectSingleBound(this, new object[] { order }))
							{
								throw new PXException(Messages.ShipmentExistsForSiteCannotReopen, order.OrderType, order.OrderNbr);
							}
						}

                    Dictionary<int?, List<INItemPlan>> demand = new Dictionary<int?, List<INItemPlan>>();

                    foreach (PXResult<SOShipLineSplit, INItemPlan> res in PXSelectReadonly2<SOShipLineSplit,
                        InnerJoin<INItemPlan, On<INItemPlan.supplyPlanID, Equal<SOShipLineSplit.planID>>>,
                        Where<SOShipLineSplit.shipmentNbr, Equal<Required<SOOrderShipment.shipmentNbr>>>>.Select(docgraph, order.OrderType, order.OrderNbr))
                    {
                        SOShipLineSplit line = res;
                        INItemPlan plan = res;

                        List<INItemPlan> ex;
                        if (!demand.TryGetValue(line.LineNbr, out ex))
                        {
                            demand[line.LineNbr] = ex = new List<INItemPlan>();
                        }
                        ex.Add(plan);
                    }

					//no Misc lines will be selected because of SiteID constraint
					foreach (PXResult<SOLine, SOShipLine> res in
						PXSelectJoin<SOLine,
							LeftJoin<SOShipLine,
								On<SOShipLine.origOrderType, Equal<SOLine.orderType>,
								And<SOShipLine.origOrderNbr, Equal<SOLine.orderNbr>,
								And<SOShipLine.origLineNbr, Equal<SOLine.lineNbr>,
								And<SOShipLine.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>>>>>>,
							Where<SOLine.orderType, Equal<Current<SOOrderShipment.orderType>>,
								And<SOLine.orderNbr, Equal<Current<SOOrderShipment.orderNbr>>,
								And<SOLine.siteID, Equal<Current<SOOrderShipment.siteID>>,
								And<SOLine.operation, Equal<Current<SOOrderShipment.operation>>,
								And<SOLine.shipDate, LessEqual<Current<SOOrderShipment.shipDate>>>>>>>>.SelectMultiBound(docgraph, new object[] { order }))
					{
						SOLine line = (SOLine)res;
						SOShipLine shipline = (SOShipLine)res;

							if ((line.Cancelled == false || line.ShippedQty > 0m) && shipline.ShipmentNbr == null)
							{
								continue;
							}

							//if it was never shipped or is included in the shipment
							if (line.Cancelled == true && line.ShippedQty == 0m || line.OpenQty == 0m && shipline.ShippedQty > 0m)
							{
								//skip auto free lines, must be consistent with OpenLineCalc<> and ConfirmShipment()
								if (line.IsFree == false || line.ManualDisc == true)
								{
									docgraph.Document.Current.OpenLineCntr++;
								}
							}

							line = PXCache<SOLine>.CreateCopy(line);
							line.Cancelled = false;
							line.UnbilledQty = line.OrderQty - line.BilledQty;

							if (shipline.ShippedQty == null)
							{
								line.OpenQty = line.OrderQty - line.ShippedQty;
								line.ClosedQty = line.ShippedQty;
								line.BaseClosedQty = line.BaseShippedQty;
							}
							else if (line.ShipComplete == SOShipComplete.CancelRemainder)
							{
								line.OpenQty = line.OrderQty;
								line.ClosedQty = 0m;
								line.BaseClosedQty = 0m;

							}
							else
							{
								line.BaseOpenQty += shipline.BaseShippedQty ?? 0m;
								PXDBQuantityAttribute.CalcTranQty<SOLine.openQty>(docgraph.Caches[typeof(SOLine)], line);

								line.ClosedQty = line.OrderQty - line.OpenQty;
								PXDBQuantityAttribute.CalcBaseQty<SOLine.closedQty>(docgraph.Caches[typeof(SOLine)], line);
							}

							line = (SOLine)docgraph.Transactions.Cache.Update(line);
							//perform dirty Update() for OpenLineCalc<>
							line.Completed = false;

                        if (line.PlanID != null && shipline.LineNbr != null)
                        {
                            List<INItemPlan> sp;
                            if (demand.TryGetValue(shipline.LineNbr, out sp))
                            {
                                foreach (INItemPlan item in sp)
                                {
                                    item.SupplyPlanID = line.PlanID;
                                    if (docgraph.Caches[typeof(INItemPlan)].GetStatus(item) == PXEntryStatus.Notchanged)
                                    {
                                        docgraph.Caches[typeof(INItemPlan)].SetStatus(item, PXEntryStatus.Updated);
                                    }
                                }
                            }
                        }
					}

						decimal? OpenQty = 0m;

						foreach (PXResult<SOLineSplit, SOShipLine> res in PXSelectJoin<SOLineSplit, 
							LeftJoin<SOShipLine, On<SOShipLine.origOrderType, Equal<SOLineSplit.orderType>, 
								And<SOShipLine.origOrderNbr, Equal<SOLineSplit.orderNbr>, 
								And<SOShipLine.origLineNbr, Equal<SOLineSplit.lineNbr>, 
								And<SOShipLine.origSplitLineNbr, Equal<SOLineSplit.splitLineNbr>, 
								And<SOShipLine.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>>>>>>>, 
							Where<SOLineSplit.orderType, Equal<Current<SOOrderShipment.orderType>>, 
								And<SOLineSplit.orderNbr, Equal<Current<SOOrderShipment.orderNbr>>, 
								And<SOLineSplit.siteID, Equal<Current<SOOrderShipment.siteID>>,
								And<SOLineSplit.operation, Equal<Current<SOOrderShipment.operation>>,
								And<Where<SOLineSplit.shipmentNbr, IsNull, Or<SOLineSplit.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>>>>>>>>,
								//And<Where<SOLineSplit.shipDate, LessEqual<Current<SOOrderShipment.shipDate>>, Or<SOLineSplit.isAllocated, Equal<False>>>>>>>,
							OrderBy<Desc<SOLineSplit.shipmentNbr,
								Asc<SOLineSplit.isAllocated,
								Desc<SOLineSplit.shipDate,
							    Desc<SOLineSplit.splitLineNbr>>>>>>.SelectMultiBound(docgraph, new object[] { order }))
						{
							SOLineSplit split = PXCache<SOLineSplit>.CreateCopy(res);

							if (object.Equals(split.ShipmentNbr, order.ShipmentNbr) && split.IsAllocated == true)
								{
									OpenQty += split.Qty - split.ShippedQty;
								}
								else if (split.Qty >= OpenQty)
								{
									split.Qty -= OpenQty;
									OpenQty = 0m;
								}
								else 
								{
									OpenQty -= split.Qty;
									split.Qty = 0m;
								}
								split.Cancelled = false;
							split.ShipmentNbr = null;

							if (split.Qty <= 0 || docgraph.splits.Cache.GetStatus(split) == PXEntryStatus.Inserted)
							{
								docgraph.splits.Delete(split);
							}
							else
							{
								docgraph.splits.Update(split);								
							}
						}

						SOOrder copy = PXCache<SOOrder>.CreateCopy(docgraph.Document.Current);
						PXFormulaAttribute.CalcAggregate<SOLine.orderQty>(docgraph.Transactions.Cache, copy);
						docgraph.Document.Update(copy);

						docgraph.Save.Press();

						persisted.Add(docgraph.Document.Current);
					}

				foreach (PXResult<INItemPlan, SOShipLineSplit, SOOrderType> res in PXSelectJoin<INItemPlan, 
                    InnerJoin<SOShipLineSplit, On<SOShipLineSplit.planID, Equal<INItemPlan.planID>>, 
                    InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOShipLineSplit.origOrderType>>>>, 
                Where<SOShipLineSplit.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>>.Select(this))
				{
					INItemPlan plan = res;
					SOOrderType ordertype = res;
                    SOShipLineSplit split = res;

                    PXSelect<SOShipLineSplit, Where<SOShipLineSplit.planID, Equal<Required<SOShipLineSplit.planID>>>>.StoreCached(this, new PXCommandKey(new object[] { split.PlanID }), new List<object> { split });
                    PXSelect<SOLine2, Where<SOLine2.planID, Equal<Required<SOLine2.planID>>>>.StoreCached(this, new PXCommandKey(new object[] { split.PlanID }), new List<object>());

					plan = PXCache<INItemPlan>.CreateCopy(plan);

						plan.PlanType = ordertype.ShipmentPlanType;

					this.Caches[typeof(INItemPlan)].Update(plan);

                    split.Confirmed = false;
                    Caches[typeof(SOShipLineSplit)].SetStatus(split, PXEntryStatus.Updated);
                    Caches[typeof(SOShipLineSplit)].IsDirty = true;
				}

				//this is done to reset BackOrder plans back to Order Plans because SOLinePlanIDAttribute does not initialize plans normally
				foreach (SOLine2 line2 in PXSelectJoin<SOLine2, InnerJoin<SOOrderShipment, On<SOOrderShipment.orderType, Equal<SOLine2.orderType>, And<SOOrderShipment.orderNbr, Equal<SOLine2.orderNbr>, And<SOOrderShipment.siteID, Equal<SOLine2.siteID>>>>>, Where<SOOrderShipment.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>, And<SOOrderShipment.shipmentType, Equal<Current<SOShipment.shipmentType>>>>>.Select(this))
				{
				  SOLine2 copy = PXCache<SOLine2>.CreateCopy(line2);
				  this.Caches[typeof(SOLine2)].RaiseRowUpdated(line2, copy);
				}

					PXAutomation.StorePersisted(this, typeof(SOOrder), persisted);

					this.Caches[typeof(SOOrder)].Clear();
					this.Caches[typeof(SOLine2)].Clear();

					this.Save.Press();
					ts.Complete();
				}
			}

		}

		public virtual void ConfirmShipment(SOOrderEntry docgraph, SOShipment shiporder)
		{
			this.Clear();

			Document.Current = Document.Search<SOShipment.shipmentNbr>(shiporder.ShipmentNbr);			
			this.lsselect.OverrideAdvancedAvailCheck(true);

			if ((bool)sosetup.Current.RequireShipmentTotal)
			{
				if (Document.Current.ShipmentQty != Document.Current.ControlQty)
				{
					throw new PXException(Messages.MissingShipmentControlTotal);
				}
			}

			if (Document.Current.ShipmentQty == 0)
				throw new PXException(Messages.UnableConfirmZeroShipment, Document.Current.ShipmentNbr);
			
			if ((SOOrderShipment)OrderList.SelectWindowed(0, 1) == null)
				throw new PXException(Messages.UnableConfirmShipment, Document.Current.ShipmentNbr);

			Carrier carrier = PXSelect<Carrier, Where<Carrier.carrierID, Equal<Required<Carrier.carrierID>>>>.Select(this, Document.Current.ShipVia);
			if (carrier != null && carrier.IsExternal == true && carrier.PackageRequired == true)
			{
				//check for atleast one package
				SOPackageDetail p = Packages.SelectSingle();
				if (p == null)
					throw new PXException(Messages.PackageIsRequired);

			}

			Document.Current.Confirmed = true;
			//support for delayed workflow fills
			Document.Current.Status = shiporder.Status;
			Document.Cache.SetStatus(Document.Current, PXEntryStatus.Updated);
			Document.Cache.IsDirty = true;

            foreach (PXResult<INItemPlan, SOShipLineSplit, INPlanType> res in PXSelectJoin<INItemPlan,
                InnerJoin<SOShipLineSplit, On<SOShipLineSplit.planID, Equal<INItemPlan.planID>>,
				InnerJoin<INPlanType, On<INPlanType.planType, Equal<INItemPlan.planType>>>>,
			Where<SOShipLineSplit.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>>.Select(this))
			{
				SOShipLineSplit split = (SOShipLineSplit)res;
				INItemPlan plan = PXCache<INItemPlan>.CreateCopy((INItemPlan)res);
				INPlanType plantype = (INPlanType)res;

                PXSelect<SOShipLineSplit, Where<SOShipLineSplit.planID, Equal<Required<SOShipLineSplit.planID>>>>.StoreCached(this, new PXCommandKey(new object[] { split.PlanID }), new List<object> { split });
                PXSelect<SOLine2, Where<SOLine2.planID, Equal<Required<SOLine2.planID>>>>.StoreCached(this, new PXCommandKey(new object[] { split.PlanID }), new List<object>());

				if ((bool)plantype.DeleteOnEvent)
				{
					Caches[typeof(INItemPlan)].Delete(plan);
					split.PlanID = null;
				}
				else if (string.IsNullOrEmpty(plantype.ReplanOnEvent) == false)
				{
					plan.PlanType = plantype.ReplanOnEvent;
					Caches[typeof(INItemPlan)].Update(plan);
				}

                split.Confirmed = true;
                Caches[typeof(SOShipLineSplit)].SetStatus(split, PXEntryStatus.Updated);
                Caches[typeof(SOShipLineSplit)].IsDirty = true;
			}

			PXRowUpdating cancel_handler = new PXRowUpdating((sender, e) => { e.Cancel = true; });
			PXFormulaAttribute.SetAggregate<SOLine.completed>(docgraph.Transactions.Cache, null);

			List<object> persisted = new List<object>();

			using (PXConnectionScope cs = new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					foreach (SOOrderShipment order in OrderList.Select())
					{
						order.Confirmed = true;
						OrderList.Cache.SetStatus(order, PXEntryStatus.Updated);
						OrderList.Cache.IsDirty = true;

						docgraph.Clear();

						docgraph.Document.Current = docgraph.Document.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderType);
						docgraph.Document.Current.OpenShipmentCntr--;
						docgraph.Document.Current.LastSiteID = order.SiteID;
						docgraph.Document.Current.LastShipDate = order.ShipDate;
						docgraph.Document.Cache.SetStatus(docgraph.Document.Current, PXEntryStatus.Updated);

						docgraph.soordertype.Current.RequireControlTotal = false;

					bool backorderExists = false;
					Dictionary<object, decimal?> SchedulesClosing = new Dictionary<object, decimal?>();
                    Dictionary<long?, List<INItemPlan>> demand = new Dictionary<long?, List<INItemPlan>>();

                    foreach (PXResult<SOLine, INItemPlan> res in PXSelectReadonly2<SOLine,
                        InnerJoin<INItemPlan, On<INItemPlan.supplyPlanID, Equal<SOLine.planID>>>,
                        Where<SOLine.orderType, Equal<Required<SOOrderShipment.orderType>>,
                            And<SOLine.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>>>>.Select(docgraph, order.OrderType, order.OrderNbr))
                    { 
                        SOLine line = res;
                        INItemPlan plan = res;

                        List<INItemPlan> ex;
                        if (!demand.TryGetValue(line.PlanID, out ex))
                        {
                            demand[line.PlanID] = ex = new List<INItemPlan>();
                        }
                        ex.Add(plan);
                    }

						foreach (PXResult<SOLine, InventoryItem, INItemPlan, SOShipLine> res in PXSelectJoin<SOLine,
							InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<SOLine.inventoryID>>,
							LeftJoin<INItemPlan, On<INItemPlan.planID, Equal<SOLine.planID>>,
							LeftJoin<SOShipLine, On<SOShipLine.origOrderType, Equal<SOLine.orderType>, And<SOShipLine.origOrderNbr, Equal<SOLine.orderNbr>, And<SOShipLine.origLineNbr, Equal<SOLine.lineNbr>, And<SOShipLine.shipmentNbr, Equal<Required<SOOrderShipment.shipmentNbr>>>>>>>>>,
							Where<SOLine.orderType, Equal<Required<SOOrderShipment.orderType>>,
								And<SOLine.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>,
								And<SOLine.siteID, Equal<Required<SOOrderShipment.siteID>>,
								And<SOLine.operation, Equal<Required<SOOrderShipment.operation>>,
								And2<Where<SOLine.openQty, Greater<decimal0>, Or<SOLine.planID, IsNotNull>>,
								And<Where<SOLine.shipDate, LessEqual<Required<SOOrderShipment.shipDate>>, Or<SOShipLine.shipmentNbr, IsNotNull>>>>>>>>,
							OrderBy<Asc<SOLine.isFree>>>.Select(docgraph, order.ShipmentNbr, order.OrderType, order.OrderNbr, order.SiteID, order.Operation, order.ShipDate))
						{
							SOLine line = (SOLine)res;
							INItemPlan plan = (INItemPlan)res;
							SOShipLine shipline = (SOShipLine)res;

							if (shipline.ShipmentNbr != null && line.ShipComplete == SOShipComplete.ShipComplete && line.BaseShippedQty < line.BaseOrderQty * line.CompleteQtyMin / 100m ||
								shipline.ShipmentNbr == null && order.ShipComplete == SOShipComplete.ShipComplete)
							{
								throw new PXException(Messages.CannotShipComplete);
							}

							line = PXCache<SOLine>.CreateCopy(line);

							if (docgraph.soordertype.Current.RequireAllocation == true && (shipline.ShipmentNbr != null || line.ShipComplete == SOShipComplete.CancelRemainder))
							{
								if (!SchedulesClosing.ContainsKey(line))
								{
									foreach (PXResult<SOLineSplit, INItemPlan, SOShipLine> schedres in PXSelectJoin<SOLineSplit,
										InnerJoin<INItemPlan, On<INItemPlan.planID, Equal<SOLineSplit.planID>>,
										LeftJoin<SOShipLine, On<SOShipLine.origOrderType, Equal<SOLineSplit.orderType>, And<SOShipLine.origOrderNbr, Equal<SOLineSplit.orderNbr>, And<SOShipLine.origLineNbr, Equal<SOLineSplit.lineNbr>, And<SOShipLine.origSplitLineNbr, Equal<SOLineSplit.splitLineNbr>, And<SOShipLine.shipmentNbr, Equal<Required<SOOrderShipment.shipmentNbr>>>>>>>>>,
										Where<SOLineSplit.orderType, Equal<Required<SOLineSplit.orderType>>,
											And<SOLineSplit.orderNbr, Equal<Required<SOLineSplit.orderNbr>>,
											And<SOLineSplit.lineNbr, Equal<Required<SOLineSplit.lineNbr>>,
											And<SOLineSplit.siteID, Equal<Required<SOLineSplit.siteID>>,
											And<Where<SOLineSplit.shipDate, LessEqual<Required<SOLineSplit.shipDate>>, 
												Or<SOShipLine.shipmentNbr, IsNotNull,
												Or<SOLineSplit.isAllocated, Equal<False>>>>>>>>>>.Select(docgraph, order.ShipmentNbr, line.OrderType, line.OrderNbr, line.LineNbr, order.SiteID, order.ShipDate))
									{
										SOLineSplit schedule = schedres;
										INItemPlan schedplan = schedres;

										docgraph.RowUpdating.AddHandler<SOLine>(cancel_handler);

										if (schedule.IsAllocated == true && line.ShipComplete != SOShipComplete.ShipComplete && line.ShipComplete != SOShipComplete.CancelRemainder)
										{
											SOLineSplit split = (SOLineSplit)line;
											split.SplitLineNbr = null;
											split.IsAllocated = false;
											split.BaseQty = (schedule.BaseQty - schedule.BaseShippedQty);
											split.Qty = INUnitAttribute.ConvertFromBase(docgraph.splits.Cache, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);

											if (split.BaseQty > 0m)
											{
												docgraph.splits.Insert(split);
											}
										}

										if (schedule.IsAllocated == true || line.ShipComplete == SOShipComplete.ShipComplete || line.ShipComplete == SOShipComplete.CancelRemainder)
										{
											schedule = PXCache<SOLineSplit>.CreateCopy(schedule);
											schedule.Cancelled = true;
											schedule.ShipmentNbr = (schedule.IsAllocated == true ? order.ShipmentNbr : null);											
											schedule = docgraph.splits.Update(schedule);
											docgraph.Caches[typeof(INItemPlan)].Delete(schedplan);

											schedule.PlanID = null;
										}

										docgraph.RowUpdating.RemoveHandler<SOLine>(cancel_handler);
									}
								}
								
								SchedulesClosing[line] = 0m;
							}

							//do not create issue lines if nothing is in the current shipment i.e. shipline.Operation == null
							if (line.AutoCreateIssueLine == true && shipline.Operation == SOOperation.Receipt)
							{
								SOLine newLine =
								PXSelect<SOLine,
									Where<SOLine.origOrderType, Equal<Required<SOLine.origOrderType>>,
										And<SOLine.origOrderNbr, Equal<Required<SOLine.origOrderNbr>>,
											And<SOLine.origLineNbr, Equal<Required<SOLine.origLineNbr>>>>>>.SelectWindowed(docgraph, 0, 1, line.OrderType,
																												   line.OrderNbr, line.LineNbr);
								if (newLine == null)
								{
									newLine = new SOLine();
									newLine.OrderType = line.OrderType;
									newLine.OrderNbr = line.OrderNbr;
									newLine.Operation = SOOperation.Issue;
									newLine = PXCache<SOLine>.CreateCopy(docgraph.Transactions.Insert(newLine));
									newLine.InventoryID = line.InventoryID;
									newLine.SubItemID = line.SubItemID;
									newLine.UOM = line.UOM;
									newLine.SiteID = line.SiteID;
									newLine.OrderQty = line.OrderQty;
									newLine.OrigOrderType = line.OrderType;
									newLine.OrigOrderNbr = line.OrderNbr;
									newLine.OrigLineNbr = line.LineNbr;
									newLine.ManualDisc = line.ManualDisc;
									newLine.ManualPrice = true;
									newLine.CuryUnitPrice = line.CuryUnitPrice;
									newLine.SalesPersonID = line.SalesPersonID;

									if (line.ManualDisc == true)
									{
										newLine.DiscPct = line.DiscPct;
										newLine.CuryDiscAmt = line.CuryDiscAmt;
										newLine.CuryLineAmt = line.CuryLineAmt;
									}

									docgraph.Transactions.Update(newLine);

									//Update manually
									docgraph.Document.Current.OpenLineCntr++;
								}
							}

							if (line.IsFree == true && line.ManualDisc == false)
							{
								if (!backorderExists)
								{
									line.OpenQty = 0m;
									line.Cancelled = true;
                                    line.ClosedQty = line.OrderQty;
                                    line.BaseClosedQty = line.BaseOrderQty;

									//Completed will be true for orders with locations enabled which requireshipping. check DefaultAttribute
									if (line.Completed != true)
									{
										line = (SOLine)docgraph.Caches[typeof(SOLine)].Update(line);

										docgraph.Caches[typeof(INItemPlan)].Delete(plan);

										//perform dirty Update() for OpenLineCalc<>
										line.PlanID = null;
										line.Completed = true;
									}
									else
									{
										docgraph.Caches[typeof(SOLine)].Update(line);
									}
								}
								else if (line.BaseShippedQty <= line.BaseOrderQty * line.CompleteQtyMin / 100m)
								{
									line.OpenQty = line.OrderQty - line.ShippedQty;
									line.ClosedQty = line.ShippedQty;
									line.BaseClosedQty = line.BaseShippedQty;

									docgraph.Caches[typeof(SOLine)].Update(line);
								}
							}
							else
							{
								if (line.ShipComplete == SOShipComplete.BackOrderAllowed && line.BaseShippedQty < line.BaseOrderQty * line.CompleteQtyMin / 100m)
								{
									line.OpenQty = line.OrderQty - line.ShippedQty;
									line.ClosedQty = line.ShippedQty;
									line.BaseClosedQty = line.BaseShippedQty;

									docgraph.Caches[typeof(SOLine)].Update(line);

									backorderExists = true;
								}
								else if (shipline.ShipmentNbr != null || line.ShipComplete != SOShipComplete.ShipComplete)
								{
									//Completed will be true for orders with locations enabled which requireshipping. check DefaultAttribute
									if (line.Completed != true)
									{
										docgraph.Document.Current.OpenLineCntr--;
									}

									if (docgraph.Document.Current.OpenLineCntr <= 0)
									{
										docgraph.Document.Current.Completed = true;
									}

									line.OpenQty = 0m;
									line.ClosedQty = line.OrderQty;
									line.BaseClosedQty = line.BaseOrderQty;

									if (line.ShipComplete == SOShipComplete.CancelRemainder)
									{
										line.UnbilledQty -= (line.OrderQty - line.ShippedQty);
										line.Cancelled = true;
									}
									else if (line.BaseShippedQty > line.BaseOrderQty * line.CompleteQtyMin / 100m)
									{
										line.UnbilledQty -= (line.OrderQty - line.ShippedQty);
									}

									//Completed will be true for orders with locations enabled which requireshipping. check DefaultAttribute
									if (line.Completed != true)
									{
                                        List<INItemPlan> sp;
                                        if (line.PlanID != null && demand.TryGetValue(line.PlanID, out sp))
                                        {
                                            INItemPlan shipPlan =
                                                PXSelectJoin<INItemPlan,
                                                    InnerJoin<SOShipLineSplit,
                                                                 On<SOShipLineSplit.planID, Equal<INItemPlan.planID>>>,
                                                    Where<SOShipLineSplit.shipmentNbr, Equal<Current2<SOShipLine.shipmentNbr>>,
                                                        And<SOShipLineSplit.lineNbr, Equal<Current2<SOShipLine.lineNbr>>>>>
                                                    .SelectSingleBound(this, new object[] { shipline });

                                            if (shipPlan != null)
                                            {
                                                foreach (INItemPlan item in sp)
                                                {
                                                    item.SupplyPlanID = shipPlan.PlanID;
                                                    if (docgraph.Caches[typeof(INItemPlan)].GetStatus(item) == PXEntryStatus.Notchanged)
                                                    {
                                                        docgraph.Caches[typeof(INItemPlan)].SetStatus(item, PXEntryStatus.Updated);
                                                    }
                                                }
                                            }
                                        }

                                        //current implementation of SOLinePlanIDAttr will remove INItemPlan and set PlanID to null in
                                        line = (SOLine)docgraph.Caches[typeof(SOLine)].Update(line);

                                        docgraph.Caches[typeof(INItemPlan)].Delete(plan);
                                        //perform dirty Update() for OpenLineCalc<>
                                        line.Completed = true;
                                        line.PlanID = null;
									}
									else
									{
										docgraph.Caches[typeof(SOLine)].Update(line);
									}
								}
							}

							if (shipline.ShipmentNbr != null)
							{
								object cached = Caches[typeof(SOShipLine)].Locate(shipline);
								if (cached != null)
								{
									shipline = (SOShipLine)cached;
								}

								if (Math.Abs((decimal)shipline.BaseQty) < 0.0000005m && this.sosetup.Current.AddAllToShipment == false)
								{
                                    Caches[typeof(SOShipLine)].SetStatus(shipline, PXEntryStatus.Deleted);
                                    Caches[typeof(SOShipLine)].ClearQueryCache();
                                }
								else
								{

                                    if (Math.Abs((decimal)shipline.BaseQty) < 0.0000005m)
                                    {
                                        lsselect.RaiseRowDeleted(Caches[typeof(SOShipLine)], shipline);
                                    }
                                    
                                        shipline.Confirmed = true;
                                        Caches[typeof(SOShipLine)].SetStatus(shipline, PXEntryStatus.Updated);
                                   
								}
								Caches[typeof(SOShipLine)].IsDirty = true;
							}
						}

						foreach (PXResult<SOLineSplit, INItemPlan, SOOrderType, SOShipLine> res in PXSelectJoin<SOLineSplit,
							InnerJoin<INItemPlan, On<INItemPlan.planID, Equal<SOLineSplit.planID>>,
							InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOLineSplit.orderType>>,
							LeftJoin<SOShipLine, On<SOShipLine.origOrderType, Equal<SOLineSplit.orderType>, And<SOShipLine.origOrderNbr, Equal<SOLineSplit.orderNbr>, And<SOShipLine.origLineNbr, Equal<SOLineSplit.lineNbr>, And<SOShipLine.shipmentNbr, Equal<Required<SOOrderShipment.shipmentNbr>>>>>>>>>,
							Where<SOLineSplit.orderType, Equal<Required<SOOrderShipment.orderType>>, 
							And<SOLineSplit.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>,
							And<SOLineSplit.siteID, Equal<Required<SOOrderShipment.siteID>>,
							And<SOLineSplit.operation, Equal<Required<SOOrderShipment.operation>>,
							And<SOOrderType.requireAllocation, Equal<False>,
							And<Where<SOLineSplit.shipDate, LessEqual<Required<SOOrderShipment.shipDate>>, Or<SOShipLine.shipmentNbr, IsNotNull>>>>>>>>>.Select(docgraph, order.ShipmentNbr, order.OrderType, order.OrderNbr, order.SiteID, order.Operation))
						{
							SOLineSplit split = (SOLineSplit)res;
							INItemPlan plan = (INItemPlan)res;

							if (order.ShipComplete == SOShipComplete.ShipComplete || ((SOShipLine)res).ShipmentNbr != null)
							{
								docgraph.Caches[typeof(INItemPlan)].Delete(plan);
							}
						}

						//apply automation step
						docgraph.Document.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderType);
						//docgraph.Document.Current.IsTaxValid = false; //force tax recalculation
						docgraph.Save.Press();

						persisted.Add(docgraph.Document.Current);
					}

					PXAutomation.StorePersisted(this, typeof(SOOrder), persisted);

					this.Caches[typeof(SOOrder)].Clear();
					this.Caches[typeof(SOLine2)].Clear();

					this.Save.Press();
					ts.Complete();
				}
			}
		}

		public virtual void InvoiceShipment(SOInvoiceEntry docgraph, SOShipment shiporder, DateTime invoiceDate, DocumentList<ARInvoice, SOInvoice> list)
		{
			this.Clear();

			Document.Current = Document.Search<SOShipment.shipmentNbr>(shiporder.ShipmentNbr);
			Document.Current.Status = shiporder.Status;

			if (Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Notchanged)
			{
				Document.Cache.SetStatus(Document.Current, PXEntryStatus.Updated);
			}

			using (PXConnectionScope cs = new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					this.Save.Press();

					foreach (PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact, SOOrderType> order in OrderList.Select())
					{
						docgraph.Clear(PXClearOption.PreserveTimeStamp);
						docgraph.ARSetup.Current.RequireControlTotal = false;
						docgraph.InvoiceOrder(invoiceDate, order, customer.Current, list);
					}
					ts.Complete();
				}
			}
		}

		public static void InvoiceReceipt(Dictionary<string, object> parameters, List<SOShipment> list, DocumentList<ARInvoice, SOInvoice> created)
		{
			SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();
			SOInvoiceEntry ie = PXGraph.CreateInstance<SOInvoiceEntry>();

			foreach (SOShipment poreceipt in list)
			{
				PXProcessing<SOShipment>.SetCurrentItem(poreceipt);

				ie.Clear();
				ie.ARSetup.Current.RequireControlTotal = false;

				char[] a = typeof(SOShipmentFilter.invoiceDate).Name.ToCharArray();
				a[0] = char.ToUpper(a[0]);
				object invoiceDate;
				if (!parameters.TryGetValue(new string(a), out invoiceDate))
				{
					invoiceDate = ie.Accessinfo.BusinessDate;
				}

				foreach (PXResult<SOOrderReceipt, SOOrder, SOOrderShipment, CurrencyInfo, SOAddress, SOContact, SOOrderType, SOOrderTypeOperation, Customer> res in PXSelectJoin<SOOrderReceipt, 
				InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOOrderReceipt.orderType>, And<SOOrder.orderNbr, Equal<SOOrderReceipt.orderNbr>>>,
				LeftJoin<SOOrderShipment, On<SOOrderShipment.orderType, Equal<SOOrderReceipt.orderType>, And<SOOrderShipment.orderNbr, Equal<SOOrderReceipt.orderNbr>, And<SOOrderShipment.shipmentNbr, Equal<SOOrderReceipt.receiptNbr>, And<SOOrderShipment.shipmentType, Equal<SOShipmentType.dropShip>>>>>,
				InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<SOOrder.curyInfoID>>,
				InnerJoin<SOAddress, On<SOAddress.addressID, Equal<SOOrder.billAddressID>>,
				InnerJoin<SOContact, On<SOContact.contactID, Equal<SOOrder.billContactID>>,
				InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrder.orderType>>,
				InnerJoin<SOOrderTypeOperation,
							 On<SOOrderTypeOperation.orderType, Equal<SOOrderType.orderType>,
									And<SOOrderTypeOperation.operation, Equal<SOOrderType.defaultOperation>>>,
				InnerJoin<Customer, On<Customer.bAccountID, Equal<SOOrder.customerID>>>>>>>>>>,
				Where<SOOrderReceipt.receiptNbr, Equal<Required<SOOrderReceipt.receiptNbr>>>>.Select(docgraph, poreceipt.ShipmentNbr))
				{
					SOOrderReceipt receipt = res;
					SOOrderShipment shipment = res;
					SOOrder order = res;

					PXResultset<SOShipLine, SOLine> details = new PXResultset<SOShipLine, SOLine>();

					foreach (PXResult<POReceiptLine, SOLine> line in PXSelectJoin<POReceiptLine, InnerJoin<SOLine, On<SOLine.pOType, Equal<POReceiptLine.pOType>, And<SOLine.pONbr, Equal<POReceiptLine.pONbr>, And<SOLine.pOLineNbr, Equal<POReceiptLine.pOLineNbr>>>>>, Where2<Where<POReceiptLine.lineType, Equal<POLineType.goodsForDropShip>, Or<POReceiptLine.lineType, Equal<POLineType.nonStockForDropShip>>>, And<POReceiptLine.receiptNbr, Equal<Current<SOOrderReceipt.receiptNbr>>, And<SOLine.orderType, Equal<Current<SOOrderReceipt.orderType>>, And<SOLine.orderNbr, Equal<Current<SOOrderReceipt.orderNbr>>>>>>>.SelectMultiBound(docgraph, new object[] { receipt }))
					{
						details.Add(new PXResult<SOShipLine, SOLine>((SOShipLine)line, line));
					}

					if (shipment.ShipmentNbr == null)
					{
						shipment = (PXResult<SOOrderReceipt, SOOrder>)res;
					}

					ie.InvoiceOrder((DateTime)invoiceDate, new PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact, SOOrderType, SOOrderTypeOperation>(shipment, order, (CurrencyInfo)res, (SOAddress)res, (SOContact)res, (SOOrderType)res, (SOOrderTypeOperation)res), details, (Customer)res, created);
				}
			}
		}

		public virtual void PostReceipt(INIssueEntry docgraph, PXResult<SOOrderShipment, SOOrder> sh, DocumentList<INRegister> list)
		{
			SOOrderShipment shiporder = sh;
			SOOrder order = sh;

			this.Clear();
			docgraph.Clear();

			docgraph.insetup.Current.HoldEntry = false;
			docgraph.insetup.Current.RequireControlTotal = false;

			INRegister newdoc = list.Find<INRegister.branchID, INRegister.docType, INRegister.siteID, INRegister.tranDate>(order.BranchID, INDocType.Issue, shiporder.SiteID, shiporder.ShipDate) ?? new INRegister();

			if (newdoc.RefNbr != null)
			{
				docgraph.issue.Current = docgraph.issue.Search<INRegister.refNbr>(newdoc.RefNbr);
			}
			else
			{
				newdoc.BranchID = order.BranchID;
				newdoc.DocType = INDocType.Issue;
				newdoc.SiteID = shiporder.SiteID;
				newdoc.TranDate = shiporder.ShipDate;
				newdoc.OrigModule = GL.BatchModule.SO;

				docgraph.issue.Insert(newdoc);
			}

			INTran newline = null;

			foreach (PXResult<POReceiptLine, SOLine, SOOrderType, SOOrderTypeOperation, InventoryItem, INLotSerClass, INPostClass, INSite, INLocation, ARTran, INTran> res in PXSelectJoin<POReceiptLine,
				InnerJoin<SOLine, On<SOLine.pOType, Equal<POReceiptLine.pOType>, And<SOLine.pONbr, Equal<POReceiptLine.pONbr>, And<SOLine.pOLineNbr, Equal<POReceiptLine.pOLineNbr>>>>,
				InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOLine.orderType>>,
				InnerJoin<SOOrderTypeOperation,
							On<SOOrderTypeOperation.orderType, Equal<SOLine.orderType>,
							And<SOOrderTypeOperation.operation, Equal<SOLine.operation>>>,
				InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<POReceiptLine.inventoryID>>,
				LeftJoin<INLotSerClass, On<INLotSerClass.lotSerClassID, Equal<InventoryItem.lotSerClassID>>,
				LeftJoin<INPostClass, On<INPostClass.postClassID, Equal<InventoryItem.postClassID>>,
				InnerJoin<INSite, On<INSite.siteID, Equal<POReceiptLine.siteID>>,
				LeftJoin<INLocation, On<INLocation.locationID, Equal<INSite.dropShipLocationID>>,
				LeftJoin<ARTran, On<ARTran.sOShipmentNbr, Equal<POReceiptLine.receiptNbr>, And<ARTran.sOShipmentType, Equal<SOShipmentType.dropShip>, And<ARTran.sOShipmentLineNbr, Equal<POReceiptLine.lineNbr>, And<ARTran.sOOrderType, Equal<SOLine.orderType>, And<ARTran.sOOrderNbr, Equal<SOLine.orderNbr>, And<ARTran.sOOrderLineNbr, Equal<SOLine.lineNbr>, And<ARTran.lineType, Equal<SOLine.lineType>>>>>>>>,
				LeftJoin<INTran, On<INTran.sOShipmentNbr, Equal<POReceiptLine.receiptNbr>, And<INTran.sOShipmentType, Equal<SOShipmentType.dropShip>, And<INTran.sOShipmentLineNbr, Equal<POReceiptLine.lineNbr>, And<INTran.sOOrderType, Equal<SOLine.orderType>, And<INTran.sOOrderNbr, Equal<SOLine.orderNbr>, And<INTran.sOOrderLineNbr, Equal<SOLine.lineNbr>>>>>>>>>>>>>>>>>,
			Where<POReceiptLine.receiptNbr, Equal<Current<SOOrderShipment.shipmentNbr>>, And<SOLine.orderType, Equal<Current<SOOrderShipment.orderType>>, And<SOLine.orderNbr, Equal<Current<SOOrderShipment.orderNbr>>, 
				And2<Where<POReceiptLine.lineType, Equal<POLineType.goodsForDropShip>, Or<POReceiptLine.lineType, Equal<POLineType.nonStockForDropShip>>>, 
				And2<Where<SOOrderType.aRDocType, Equal<ARDocType.noUpdate>, Or<ARTran.refNbr, IsNotNull>>, And<INTran.refNbr, IsNull>>>>>>>.SelectMultiBound(this, new object[] { shiporder }))
			{
				POReceiptLine line = res;
				SOLine soline = res;
				ARTran artran = res;
				SOOrderType ordertype = res;
				SOOrderTypeOperation orderoperation = res;
				INLocation loc = res;
				INLotSerClass lsclass = res;
				INPostClass postclass = res;

				if (line.LineType == POLineType.GoodsForDropShip && loc.LocationID == null)
				{
					throw new PXException(Messages.NoDropShipLocation, Caches[typeof(POReceiptLine)].GetValueExt<POReceiptLine.siteID>(line));
				}

				newline = new INTran();
				newline.BranchID = soline.BranchID;
				newline.TranType = orderoperation.INDocType;
				newline.POReceiptNbr = line.ReceiptNbr;
				newline.POReceiptLineNbr = line.LineNbr;
				newline.SOShipmentNbr = line.ReceiptNbr;
				newline.SOShipmentType = SOShipmentType.DropShip;
				newline.SOShipmentLineNbr = line.LineNbr;
				newline.SOOrderType = soline.OrderType;
				newline.SOOrderNbr = soline.OrderNbr;
				newline.SOOrderLineNbr = soline.LineNbr;
				newline.ARDocType = artran.TranType;
				newline.ARRefNbr = artran.RefNbr;
				newline.ARLineNbr = artran.LineNbr;

				newline.InventoryID = line.InventoryID;
				newline.SubItemID = line.SubItemID;
				newline.SiteID = line.SiteID;
				newline.LocationID = loc.LocationID;
				newline.BAccountID = soline.CustomerID;
				newline.InvtMult = (short)0;
				newline.UOM = line.UOM;
				newline.Qty = line.ReceiptQty;
				newline.UnitPrice = artran.UnitPrice ?? 0m;
				newline.TranAmt = artran.TranAmt ?? 0m;
				newline.UnitCost = line.UnitCost;
				newline.TranCost = line.ExtCost;
				newline.TranDesc = soline.TranDesc;
				newline.ReasonCode = soline.ReasonCode;
				newline.AcctID = line.POAccrualAcctID;
				newline.SubID = line.POAccrualSubID;
				newline.COGSAcctID = line.ExpenseAcctID;
				newline.COGSSubID = (postclass != null && postclass.COGSSubFromSales == true ? artran.SubID : null) ?? line.ExpenseSubID;

				newline = docgraph.transactions.Insert(newline);

				if (lsclass.LotSerAssign == "U" || lsclass.LotSerTrack == "N")
				{
					INTranSplit newsplit = (INTranSplit)newline;
					newsplit.SplitLineNbr = null;

					docgraph.splits.Insert(newsplit);
				}
			}

			INRegister copy = PXCache<INRegister>.CreateCopy(docgraph.issue.Current);
			PXFormulaAttribute.CalcAggregate<INTran.qty>(docgraph.transactions.Cache, copy);
			PXFormulaAttribute.CalcAggregate<INTran.tranAmt>(docgraph.transactions.Cache, copy);
			PXFormulaAttribute.CalcAggregate<INTran.tranCost>(docgraph.transactions.Cache, copy);
			docgraph.issue.Update(copy);

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				if (docgraph.transactions.Cache.IsDirty)
				{
					docgraph.Save.Press();

					{
						shiporder.InvtDocType = docgraph.issue.Current.DocType;
						shiporder.InvtRefNbr = docgraph.issue.Current.RefNbr;

						OrderList.Cache.Update(shiporder);

						SOOrder cached = soorder.Locate(order);
						cached.ShipmentCntr++;
					}

					PXDBDefaultAttribute.SetDefaultForUpdate<SOOrderShipment.shipAddressID>(OrderList.Cache, null, false);
					PXDBDefaultAttribute.SetDefaultForUpdate<SOOrderShipment.shipContactID>(OrderList.Cache, null, false);
					PXDBLiteDefaultAttribute.SetDefaultForUpdate<SOOrderShipment.shipmentNbr>(OrderList.Cache, null, false);

					this.Save.Press();

					if (list.Find(docgraph.issue.Current) == null)
					{
						list.Add(docgraph.issue.Current);
					}
				}
				ts.Complete();
			}
		}

		public virtual void PostShipment(INIssueEntry docgraph, SOShipment shiporder, DocumentList<INRegister> list)
		{
			this.Clear();
			docgraph.Clear();

			Document.Current = Document.Search<SOShipment.shipmentNbr>(shiporder.ShipmentNbr);
			Document.Current.Status = shiporder.Status;
			Document.Cache.SetStatus(Document.Current, PXEntryStatus.Updated);
			Document.Cache.IsDirty = true;

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				foreach (PXResult<SOOrderShipment, SOOrder> res in PXSelectJoin<SOOrderShipment, InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOOrderShipment.orderType>, And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>>, Where<SOOrderShipment.shipmentType, Equal<Current<SOShipment.shipmentType>>, And<SOOrderShipment.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>>>.SelectMultiBound(this, new object[] { shiporder }))
				{
					this.PostShipment(docgraph, res, list);
				}
				ts.Complete();
			}
		}

		public virtual void PostShipment(INIssueEntry docgraph, PXResult<SOOrderShipment, SOOrder> sh, DocumentList<INRegister> list)
		{
			SOOrderShipment shiporder = sh;
			SOOrder order = sh;

			if (!Document.Cache.IsDirty)
			{
				this.Clear();
				docgraph.Clear();

				Document.Current = Document.Search<SOShipment.shipmentNbr>(shiporder.ShipmentNbr);
			}

			docgraph.insetup.Current.HoldEntry = false;
			docgraph.insetup.Current.RequireControlTotal = false;

            INRegister newdoc =
                    (this.sosetup.Current.ConsolidateIN == false
                    ? list.Find<INRegister.origRefNbr>(shiporder.ShipmentNbr)
                    : shiporder.ShipmentType == SOShipmentType.Transfer
                    ? list.Find<INRegister.branchID, INRegister.docType, INRegister.siteID, INRegister.toSiteID, INRegister.tranDate>(Company.Current.BranchID, shiporder.ShipmentType, shiporder.SiteID, Document.Current.DestinationSiteID, shiporder.ShipDate)
                    : list.Find<INRegister.branchID, INRegister.docType, INRegister.siteID, INRegister.tranDate>(order.BranchID, shiporder.ShipmentType, shiporder.SiteID, shiporder.ShipDate))
                    ?? new INRegister();

			if (newdoc.RefNbr != null)
			{
				docgraph.issue.Current = PXSelect<INRegister>.Search<INRegister.refNbr>(docgraph, newdoc.RefNbr);
			}
			else
			{
				newdoc.BranchID = shiporder.ShipmentType == SOShipmentType.Transfer ? Company.Current.BranchID : order.BranchID;
				newdoc.DocType = shiporder.ShipmentType;
				newdoc.SiteID = shiporder.SiteID;
				newdoc.ToSiteID = Document.Current.DestinationSiteID;
				if (newdoc.DocType == SOShipmentType.Transfer)
				{
					newdoc.TransferType = INTransferType.TwoStep;
				}
				newdoc.TranDate = shiporder.ShipDate;
				newdoc.OrigModule = GL.BatchModule.SO;
                newdoc.OrigRefNbr = shiporder.ShipmentNbr;

				docgraph.issue.Insert(newdoc);
			}

			SOShipLine prev_line = null;
			ARTran prev_artran = null;
			INTran newline = null;

            Dictionary<long?, List<INItemPlan>> demand = new Dictionary<long?, List<INItemPlan>>();

            foreach (PXResult<SOShipLine, SOShipLineSplit, INItemPlan> res in PXSelectJoin<SOShipLine,
                InnerJoin<SOShipLineSplit, On<SOShipLineSplit.shipmentNbr, Equal<SOShipLine.shipmentNbr>, And<SOShipLineSplit.lineNbr, Equal<SOShipLine.lineNbr>>>,
                InnerJoin<INItemPlan, On<INItemPlan.supplyPlanID, Equal<SOShipLineSplit.planID>>>>,
            Where<SOShipLine.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>,
                And<SOShipLine.origOrderType, Equal<Current<SOOrderShipment.orderType>>,
                And<SOShipLine.origOrderNbr, Equal<Current<SOOrderShipment.orderNbr>>>>>>.SelectMultiBound(this, new object[] { shiporder }))
            { 
                SOShipLineSplit split = res;
                INItemPlan plan = res;

                List<INItemPlan> ex;
                if (!demand.TryGetValue(split.PlanID, out ex))
                {
                    demand[split.PlanID] = ex = new List<INItemPlan>();
                }
                ex.Add(plan);
            }

			foreach (PXResult<SOShipLine, SOShipLineSplit, SOOrderType, SOLine, ARTran, INTran, INItemPlan, INPlanType> res in PXSelectJoin<SOShipLine,
				InnerJoin<SOShipLineSplit, On<SOShipLineSplit.shipmentNbr, Equal<SOShipLine.shipmentNbr>, And<SOShipLineSplit.lineNbr, Equal<SOShipLine.lineNbr>>>,
				InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOShipLine.origOrderType>>,
				LeftJoin<SOLine, On<SOLine.orderType, Equal<SOShipLine.origOrderType>, And<SOLine.orderNbr, Equal<SOShipLine.origOrderNbr>, And<SOLine.lineNbr, Equal<SOShipLine.origLineNbr>>>>,
				LeftJoin<ARTran, On<ARTran.sOShipmentNbr, Equal<SOShipLine.shipmentNbr>, And<ARTran.sOShipmentType, NotEqual<SOShipmentType.dropShip>, And<ARTran.sOShipmentLineNbr, Equal<SOShipLine.lineNbr>, And<ARTran.lineType, Equal<SOShipLine.lineType>>>>>,
				LeftJoin<INTran, On<INTran.sOShipmentNbr, Equal<SOShipLine.shipmentNbr>, And<INTran.sOShipmentType, NotEqual<SOShipmentType.dropShip>, And<INTran.sOShipmentLineNbr, Equal<SOShipLine.lineNbr>>>>,
				LeftJoin<INItemPlan, On<INItemPlan.planID, Equal<SOShipLineSplit.planID>>,
				LeftJoin<INPlanType, On<INPlanType.planType, Equal<INItemPlan.planType>>>>>>>>>,
			Where<SOShipLine.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>, And<SOShipLine.origOrderType, Equal<Current<SOOrderShipment.orderType>>, And<SOShipLine.origOrderNbr, Equal<Current<SOOrderShipment.orderNbr>>, And<INTran.refNbr, IsNull>>>>, 
			OrderBy<Asc<SOShipLine.shipmentNbr, Asc<SOShipLine.lineNbr>>>>.SelectMultiBound(this, new object[] { shiporder }))
			{
				SOShipLine line = res;
				SOShipLineSplit split = res;
                INItemPlan plan = res;
				INPlanType plantype = res;
				SOLine soline = res;
				ARTran artran = res;
				SOOrderType ordertype = res;
                SOShipLineSplit splitcopy = PXCache<SOShipLineSplit>.CreateCopy(split);

                //avoid ReadItem()
                if (plan.PlanID != null)
                {
                    Caches[typeof(INItemPlan)].SetStatus(plan, PXEntryStatus.Notchanged);
                }
                PXSelect<SOShipLineSplit, Where<SOShipLineSplit.planID, Equal<Required<SOShipLineSplit.planID>>>>.StoreCached(this, new PXCommandKey(new object[] {split.PlanID }), new List<object> { split });
                PXSelect<SOLine2, Where<SOLine2.planID, Equal<Required<SOLine2.planID>>>>.StoreCached(this, new PXCommandKey(new object[] { split.PlanID }), new List<object>());

				if (plantype.DeleteOnEvent == true || line.BaseShippedQty < 0.0000005m)
				{
					Caches[typeof(INItemPlan)].Delete(plan);

					split.PlanID = null;
					split.Released = true;
					Caches[typeof(SOShipLineSplit)].SetStatus(split, PXEntryStatus.Updated);
					Caches[typeof(SOShipLineSplit)].IsDirty = true;

                    if (plantype.DeleteOnEvent != true)
                    {
                        continue;
                    }
				}
				else if (string.IsNullOrEmpty(plantype.ReplanOnEvent) == false)
				{
                    plan = PXCache<INItemPlan>.CreateCopy(plan);
                    plan.PlanType = plantype.ReplanOnEvent;
					Caches[typeof(INItemPlan)].Update(plan);

					Caches[typeof(SOShipLineSplit)].SetStatus(split, PXEntryStatus.Updated);
					Caches[typeof(SOShipLineSplit)].IsDirty = true;
				}

				if ((Caches[typeof(SOShipLine)].ObjectsEqual(prev_line, line) == false || object.Equals(line.InventoryID, split.InventoryID) == false) && split.IsStockItem == true)
				{
					line.Released = true;
					Caches[typeof(SOShipLine)].SetStatus(line, PXEntryStatus.Updated);
					Caches[typeof(SOShipLine)].IsDirty = true;

					newline = new INTran();
					newline.BranchID = shiporder.ShipmentType == SOShipmentType.Transfer ? Company.Current.BranchID : soline.BranchID;
					newline.DocType = newdoc.DocType;
					newline.TranType = line.TranType;
					newline.SOShipmentNbr = line.ShipmentNbr;
					newline.SOShipmentType = line.ShipmentType;
					newline.SOShipmentLineNbr = line.LineNbr;
					newline.SOOrderType = line.OrigOrderType;
					newline.SOOrderNbr = line.OrigOrderNbr;
					newline.SOOrderLineNbr = line.OrigLineNbr;
					newline.ARDocType = artran.TranType;
					newline.ARRefNbr = artran.RefNbr;
					newline.ARLineNbr = artran.LineNbr;
					newline.BAccountID = line.CustomerID;
					if (ordertype.ARDocType != ARDocType.NoUpdate)
					{
						newline.AcctID = artran.AccountID ?? soline.SalesAcctID;
						newline.SubID = artran.SubID ?? soline.SalesSubID;

						if (newline.AcctID == null)
						{ 
							throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<SOLine.salesAcctID>(Caches[typeof(SOLine)]));
						}

						if (newline.SubID == null)
						{
							throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<SOLine.salesSubID>(Caches[typeof(SOLine)]));
						}
					}
					newline.ProjectID = line.ProjectID;
					newline.TaskID = line.TaskID;

					newline.InventoryID = split.InventoryID;
					newline.SiteID = line.SiteID;
					newline.ToSiteID = Document.Current.DestinationSiteID;
					newline.InvtMult = line.InvtMult;
					newline.Qty = 0m;

					if (object.Equals(line.InventoryID, split.InventoryID) == false)
					{
						newline.SubItemID = split.SubItemID;
						newline.UOM = split.UOM;
						newline.UnitPrice = 0m;
						newline.UnitCost = 0m;
						newline.TranDesc = null;
					}
					else
					{
						newline.SubItemID = line.SubItemID;
						newline.UOM = line.UOM;
						newline.UnitPrice = artran.UnitPrice ?? 0m;
						newline.UnitCost = line.UnitCost;
						newline.TranDesc = line.TranDesc;
						newline.ReasonCode = line.ReasonCode;
					}

					newline = docgraph.lsselect.Insert(newline);
				}

				prev_line = line;
				prev_artran = artran;

				if (split.IsStockItem == true)
				{
					INTranSplit newsplit = (INTranSplit)newline;
					newsplit.SplitLineNbr = null;
					newsplit.SubItemID = split.SubItemID;
					newsplit.LocationID = split.LocationID;
					newsplit.LotSerialNbr = split.LotSerialNbr;
					newsplit.ExpireDate = split.ExpireDate;
					newsplit.UOM = split.UOM;
					newsplit.Qty = split.Qty;
					newsplit.BaseQty = null;
					if (line.ShipmentType == SOShipmentType.Transfer)
					{
						newsplit.TransferType = INTransferType.TwoStep;
					}

					newsplit = docgraph.splits.Insert(newsplit);

					List<INItemPlan> sp;
					if (splitcopy.PlanID != null && demand.TryGetValue(splitcopy.PlanID, out sp))
					{
						foreach (INItemPlan item in sp)
						{
							item.SupplyPlanID = newsplit.PlanID;
							if (docgraph.Caches[typeof(INItemPlan)].GetStatus(item) == PXEntryStatus.Notchanged)
							{
								docgraph.Caches[typeof(INItemPlan)].SetStatus(item, PXEntryStatus.Updated);
							}
						}
					}

					if (object.Equals(line.InventoryID, split.InventoryID))
					{
						newline.TranCost = prev_line.ExtCost;
						newline.TranAmt = string.Equals(ordertype.DefaultOperation, line.Operation) ? prev_artran.TranAmt ?? 0m : -prev_artran.TranAmt ?? 0m;
					}
				}
			}

			INRegister copy = PXCache<INRegister>.CreateCopy(docgraph.issue.Current);
			PXFormulaAttribute.CalcAggregate<INTran.qty>(docgraph.transactions.Cache, copy);
			PXFormulaAttribute.CalcAggregate<INTran.tranAmt>(docgraph.transactions.Cache, copy);
			PXFormulaAttribute.CalcAggregate<INTran.tranCost>(docgraph.transactions.Cache, copy);
			docgraph.issue.Update(copy);

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				if (docgraph.transactions.Cache.IsDirty)
				{
					docgraph.Save.Press();

					foreach (SOOrderShipment item in PXSelect<SOOrderShipment, Where<SOOrderShipment.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>, And<SOOrderShipment.shipmentType, Equal<Current<SOShipment.shipmentType>>>>>.SelectMultiBound(this, new object[] { shiporder }))
					{
						item.InvtDocType = docgraph.issue.Current.DocType;
						item.InvtRefNbr = docgraph.issue.Current.RefNbr;

						OrderList.Cache.Update(item);
					}

					this.Save.Press();

					INRegister existing;
					if ((existing = list.Find(docgraph.issue.Current)) == null)
					{
						list.Add(docgraph.issue.Current);
					}
					else
					{
						docgraph.issue.Cache.RestoreCopy(existing, docgraph.issue.Current);
					}
				}
				ts.Complete();
			}
		}
		#endregion

		#region Discount

		private void AllocateDocumentFreeItems(SOOrder order)
		{
			PXSelectBase<SOOrderDiscountDetail> select = new
				PXSelect<SOOrderDiscountDetail,
				Where<SOOrderDiscountDetail.orderType, Equal<Required<SOOrderDiscountDetail.orderType>>,
				And<SOOrderDiscountDetail.orderNbr, Equal<Required<SOOrderDiscountDetail.orderNbr>>,
				And<SOOrderDiscountDetail.type, Equal<DiscountType.DocumentDiscount>>>>>(this);

			foreach (SOOrderDiscountDetail detail in select.Select(order.OrderType, order.OrderNbr))
			{
				if (detail.FreeItemID != null && detail.FreeItemQty != null && detail.FreeItemQty.Value > 0)
				{
					SOLine freeItem = PXSelect<SOLine,
					Where<SOLine.orderType, Equal<Required<SOLine.orderType>>,
					And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
					And<SOLine.isFree, Equal<boolTrue>,
					And<SOLine.inventoryID, Equal<Required<SOLine.inventoryID>>>>>>>.Select(this, order.OrderType, order.OrderNbr, detail.FreeItemID);

					if (freeItem.ManualDisc == false && freeItem.ShippedQty <= freeItem.OrderQty)
					{
						PXSelectBase<SOLine> linesSelect = new
							PXSelect<SOLine,
							Where<SOLine.orderType, Equal<Required<SOLine.orderType>>,
							And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
							And<SOLine.lineType, NotEqual<SOLineType.miscCharge>,
							And<SOLine.docDiscIDC1, Equal<Required<SOLine.docDiscIDC1>>,
							And<SOLine.docDiscSeqIDC1, Equal<Required<SOLine.docDiscSeqIDC1>>,
							Or<SOLine.docDiscIDC2, Equal<Required<SOLine.docDiscIDC2>>,
							And<SOLine.docDiscSeqIDC2, Equal<Required<SOLine.docDiscSeqIDC2>>>>>>>>>>(this);

						bool hasBackorderedItems = false;
						foreach (SOLine line in linesSelect.Select(order.OrderType, order.OrderNbr, detail.DiscountID, detail.DiscountSequenceID, detail.DiscountID, detail.DiscountSequenceID))
						{
							SOShipLine shipLine = PXSelect<SOShipLine,
								Where<SOShipLine.origOrderType, Equal<Required<SOShipLine.origOrderType>>,
								And<SOShipLine.origOrderNbr, Equal<Required<SOShipLine.origOrderNbr>>,
								And<SOShipLine.origLineNbr, Equal<Required<SOShipLine.origLineNbr>>>>>>.Select(this, line.OrderType, line.OrderNbr, line.LineNbr);

							decimal totalShipedQty = (line.ShippedQty ?? 0) + (shipLine == null ? 0 : (shipLine.Qty ?? 0));
							if (line.ShipComplete == SOShipComplete.BackOrderAllowed && totalShipedQty < line.OrderQty)
							{
								hasBackorderedItems = true;
							}
						}

						if (!hasBackorderedItems)
						{
							PXSelectBase<SOShipLine> sourceLinesSelect = new
								PXSelect<SOShipLine,
								Where<SOShipLine.origOrderType, Equal<Required<SOShipLine.origOrderType>>,
								And<SOShipLine.origOrderNbr, Equal<Required<SOShipLine.origOrderNbr>>,
								And<SOShipLine.docDiscIDC1, Equal<Required<SOShipLine.docDiscIDC1>>,
								And<SOShipLine.docDiscSeqIDC1, Equal<Required<SOShipLine.docDiscSeqIDC1>>,
								Or<SOShipLine.docDiscIDC2, Equal<Required<SOShipLine.docDiscIDC2>>,
								And<SOShipLine.docDiscSeqIDC2, Equal<Required<SOShipLine.docDiscSeqIDC2>>>>>>>>>(this);

							List<SOShipLine> sourceLines = new List<SOShipLine>();
							foreach (SOShipLine ssl in sourceLinesSelect.Select(order.OrderType, order.OrderNbr, detail.DiscountID, detail.DiscountSequenceID, detail.DiscountID, detail.DiscountSequenceID))
							{
								sourceLines.Add(ssl);
							}

							SOShipmentDiscountDetail shipDiscDetail = SODiscountEngine<SOShipLine>.AggregateFreeItemDiscounts<SOShipmentDiscountDetail>(this.Caches[typeof(SOShipLine)], sourceLines, detail.DiscountID, detail.DiscountSequenceID, order.OrderDate.Value, ProrateDiscount);

							if (shipDiscDetail != null)
							{
								shipDiscDetail.OrderType = detail.OrderType;
								shipDiscDetail.OrderNbr = detail.OrderNbr;

								UpdateInsertDiscountTrace(shipDiscDetail);
							}
						}
					}
				}
			}
		}

        private void AllocateGroupFreeItems(SOOrder order)
        {
            Dictionary<DiscKey, decimal> freeItems = new Dictionary<DiscKey, decimal>();

            SOOrderEntry soOrder = (SOOrderEntry)PXGraph.CreateInstance(typeof(SOOrderEntry));
            soOrder.Document.Current = order;

            PXSelectBase<SOShipLine> lines = new PXSelectJoin<SOShipLine,
                InnerJoin<SOLine, On<SOLine.orderType, Equal<SOShipLine.origOrderType>,
                And<SOLine.orderNbr, Equal<SOShipLine.origOrderNbr>,
                And<SOLine.lineNbr, Equal<SOShipLine.origLineNbr>>>>>,
                Where<SOShipLine.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>,
                And<SOShipLine.origOrderType, Equal<Required<SOShipLine.origOrderType>>,
                And<SOShipLine.origOrderNbr, Equal<Required<SOShipLine.origOrderNbr>>,
                And<SOShipLine.isFree, Equal<boolFalse>>>>>>(this);

            Dictionary<DiscountSequenceKey, DiscountEngine<SOLine>.DiscountDetailToLineCorrelation<SOOrderDiscountDetail>> grLinesOrderCorrelation = DiscountEngine<SOLine>.CollectGroupDiscountToLineCorrelation(soOrder.Transactions.Cache, soOrder.Transactions, soOrder.DiscountDetails, order.CustomerLocationID, (DateTime)order.OrderDate, true);
            if (sosetup.Current.FreeItemShipping == FreeItemShipType.Proportional)
            {
                foreach (KeyValuePair<DiscountSequenceKey, DiscountEngine<SOLine>.DiscountDetailToLineCorrelation<SOOrderDiscountDetail>> dsGroup in grLinesOrderCorrelation)
                {
                    decimal shippedQty = 0m;
                    decimal shippedGroupQty = 0m;
                    foreach (SOLine soLine in dsGroup.Value.listOfApplicableLines)
                    {
                        foreach (PXResult<SOShipLine, SOLine> line in lines.Select(order.OrderType, order.OrderNbr))
                        {
                            SOShipLine shipLine = (SOShipLine)line;
                            SOLine sl = (SOLine)line;

                            if (soLine.LineNbr == shipLine.OrigLineNbr)
                            {
                                shippedGroupQty += (shipLine.ShippedQty ?? 0m);
                            }
                        }
                    }
                    if (dsGroup.Value.discountDetailLine.FreeItemQty > 0m)
                        shippedQty = (shippedGroupQty / (decimal)dsGroup.Value.discountDetailLine.DiscountableQty) * (decimal)dsGroup.Value.discountDetailLine.FreeItemQty;

                    DiscKey discKey = new DiscKey(dsGroup.Key.DiscountID, dsGroup.Key.DiscountSequenceID, (int)dsGroup.Value.discountDetailLine.FreeItemID);
                    freeItems.Add(discKey, Math.Floor(shippedQty));
                }
            }
            else
            {
                //Ship on last shipment
                foreach (KeyValuePair<DiscountSequenceKey, DiscountEngine<SOLine>.DiscountDetailToLineCorrelation<SOOrderDiscountDetail>> dsGroup in grLinesOrderCorrelation)
                {
                    decimal shippedBOGroupQty = 0m;
                    decimal orderBOGroupQty = 0m;
                    decimal shippedGroupQty = 0m;
                    decimal orderGroupQty = 0m;

                    decimal shippedQty = 0m;
                    foreach (SOLine soLine in dsGroup.Value.listOfApplicableLines)
                    {
                        SOLine2 keys = new SOLine2();
                        keys.OrderType = soLine.OrderType;
                        keys.OrderNbr = soLine.OrderNbr;
                        keys.LineNbr = soLine.LineNbr;

                        SOLine2 solineWithUpdatedShippedQty = (SOLine2)this.Caches[typeof(SOLine2)].Locate(keys);
                        if (solineWithUpdatedShippedQty != null)
                        {
                            orderGroupQty += soLine.Qty ?? 0m;
                            if (soLine.ShipComplete == SOShipComplete.BackOrderAllowed)
                            {
                                orderBOGroupQty += soLine.Qty ?? 0m;
                                if (solineWithUpdatedShippedQty.ShippedQty >= soLine.OrderQty)
                                {
                                    if (soLine.LineNbr == solineWithUpdatedShippedQty.LineNbr)
                                    {
                                        shippedBOGroupQty += (solineWithUpdatedShippedQty.ShippedQty ?? 0m);
                                    }
                                }
                            }
                            else
                            {
                                shippedGroupQty += solineWithUpdatedShippedQty.ShippedQty ?? 0m;
                            }
                        }
                    }

                    if (dsGroup.Value.discountDetailLine.FreeItemQty > 0m)
                        if (shippedGroupQty + shippedBOGroupQty < orderGroupQty)
                            shippedQty = ((shippedGroupQty + shippedBOGroupQty) / (decimal)dsGroup.Value.discountDetailLine.DiscountableQty) * (decimal)dsGroup.Value.discountDetailLine.FreeItemQty;
                        else
                            shippedQty = (decimal)dsGroup.Value.discountDetailLine.FreeItemQty;

                    DiscKey discKey = new DiscKey(dsGroup.Key.DiscountID, dsGroup.Key.DiscountSequenceID, (int)dsGroup.Value.discountDetailLine.FreeItemID);
                    freeItems.Add(discKey, shippedBOGroupQty >= orderBOGroupQty ? Math.Floor(shippedQty) : 0m);
                }
            }

            foreach (KeyValuePair<DiscKey, decimal> kv in freeItems)
            {
                SOShipmentDiscountDetail sdd = new SOShipmentDiscountDetail();
                sdd.Type = DiscountType.Line;
                sdd.OrderType = order.OrderType;
                sdd.OrderNbr = order.OrderNbr;
                sdd.DiscountID = kv.Key.DiscID;
                sdd.DiscountSequenceID = kv.Key.DiscSeqID;
                sdd.FreeItemID = kv.Key.FreeItemID;
                sdd.FreeItemQty = kv.Value;

                UpdateInsertDiscountTrace(sdd);
            }
        }

		private void AllocateLineFreeItems(SOOrder order)
		{
			PXSelectBase<SOShipLine> lines = new PXSelectJoin<SOShipLine,
				InnerJoin<SOLine, On<SOLine.orderType, Equal<SOShipLine.origOrderType>,
					And<SOLine.orderNbr, Equal<SOShipLine.origOrderNbr>,
					And<SOLine.lineNbr, Equal<SOShipLine.origLineNbr>>>>>,
				Where<SOShipLine.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>,
				And<SOShipLine.origOrderType, Equal<Required<SOShipLine.origOrderType>>,
				And<SOShipLine.origOrderNbr, Equal<Required<SOShipLine.origOrderNbr>>,
				And<SOShipLine.isFree, Equal<boolFalse>>>>>>(this);

			Dictionary<DiscKey, decimal> freeItems = new Dictionary<DiscKey, decimal>();
 
			foreach (PXResult<SOShipLine, SOLine> line in lines.Select(order.OrderType, order.OrderNbr))
			{
				SOShipLine shipLine = (SOShipLine)line;
				SOLine sl = (SOLine)line;

				if (sl.ShipComplete == SOShipComplete.BackOrderAllowed)
				{
					// NOTE: TODO
					// because of Math.Floor operator there always exist posibility of rounding off errors.
					// for now I have no idea how to handle such cases ((

					if (sosetup.Current.FreeItemShipping == FreeItemShipType.Proportional)
					{                       
                        if (!string.IsNullOrEmpty(sl.DiscountID))
						{
                            DiscountSequence ds = GetDiscountSequenceByID(sl.DiscountID, sl.DiscountSequenceID);
							if (ds != null)
							{
								SODiscountEngine<SOLine>.SingleDiscountResult dr =
									SODiscountEngine<SOLine>.CalculateDiscount(Caches[typeof(SOLine)], ds, sl, order.OrderDate.Value, ProrateDiscount);

								if (dr.FreeItemID != null && dr.FreeItemQty != null && dr.FreeItemQty.Value > 0 && sl.OrderQty != null && sl.OrderQty.Value > 0)
								{
									decimal portion = Math.Floor(dr.FreeItemQty.Value * (shipLine.ShippedQty ?? 0) / sl.OrderQty.Value);

									if (portion > 0)
									{
                                        DiscKey discKey = new DiscKey(sl.DiscountID, sl.DiscountSequenceID, dr.FreeItemID.Value);

										if (freeItems.ContainsKey(discKey))
										{
											freeItems[discKey] += portion;
										}
										else
										{
											freeItems.Add(discKey, portion);
										}
									}
								}
							}
						}
					}
					else
					{
						SOLine2 keys = new SOLine2();
						keys.OrderType = sl.OrderType;
						keys.OrderNbr = sl.OrderNbr;
						keys.LineNbr = sl.LineNbr;

						SOLine2 solineWithUpdatedShippedQty = (SOLine2) this.Caches[typeof(SOLine2)].Locate(keys);
						if (solineWithUpdatedShippedQty != null)
						{
							//Ship on last shipment
                            if (solineWithUpdatedShippedQty.ShippedQty >= sl.OrderQty)
							{
								CalcFreeItems(shipLine, freeItems, order.OrderDate.Value);
							}
						}
					}
				}
				else
				{
                    CalcFreeItems(shipLine, freeItems, order.OrderDate.Value);
				}
			}

			foreach (KeyValuePair<DiscKey, decimal> kv in freeItems)
			{
				SOShipmentDiscountDetail sdd = new SOShipmentDiscountDetail();
				sdd.Type = DiscountType.Line;
				sdd.OrderType = order.OrderType;
				sdd.OrderNbr = order.OrderNbr;
				sdd.DiscountID = kv.Key.DiscID;
				sdd.DiscountSequenceID = kv.Key.DiscSeqID;
				sdd.FreeItemID = kv.Key.FreeItemID;
				sdd.FreeItemQty = kv.Value;

				UpdateInsertDiscountTrace(sdd);
			}
		}

		private struct DiscKey
		{
			string discID;
			string discSeqID;
			int freeItemID;

			public string DiscID { get { return discID; } }
			public string DiscSeqID { get { return discSeqID; } }
			public int FreeItemID { get { return freeItemID; } }

			public DiscKey(string discID, string discSeqID, int freeItemID)
			{
				this.discID = discID;
				this.discSeqID = discSeqID;
				this.freeItemID = freeItemID;
			}
		}

		private void CalcFreeItems(SOShipLine shipLine, Dictionary<DiscKey, decimal> freeItems, DateTime date)
		{
			if (!string.IsNullOrEmpty(shipLine.DetDiscIDC1))
			{
				AddFreeItem(shipLine, freeItems, shipLine.DetDiscIDC1, shipLine.DetDiscSeqIDC1, date);
			}

			if (!string.IsNullOrEmpty(shipLine.DetDiscIDC2))
			{
				AddFreeItem(shipLine, freeItems, shipLine.DetDiscIDC2, shipLine.DetDiscSeqIDC2, date);
			}
		}

		private void AddFreeItem(SOShipLine sl, Dictionary<DiscKey, decimal> freeItems, string discID, string discSeqID, DateTime date)
		{
			DiscountSequence ds = GetDiscountSequenceByID(discID, discSeqID);
			if (ds != null)
			{
				SODiscountEngine<SOShipLine>.SingleDiscountResult dr =
								SODiscountEngine<SOShipLine>.CalculateDiscount(Caches[typeof(SOShipLine)], ds, sl, date, ProrateDiscount);

				SOLine manualFreeItem = PXSelect<SOLine,
					Where<SOLine.orderType, Equal<Required<SOLine.orderType>>,
					And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
					And<SOLine.isFree, Equal<boolTrue>,
					And<SOLine.manualDisc, Equal<boolTrue>,
					And<SOLine.inventoryID, Equal<Required<SOLine.inventoryID>>>>>>>>.Select(this, sl.OrigOrderType, sl.OrigOrderNbr, sl.InventoryID);

				if (manualFreeItem == null)
				{
					/*
					 Free Item should be calculated only if there is no manual override.
					 */

					if (dr.FreeItemID != null && dr.FreeItemQty != null && dr.FreeItemQty > 0)
					{
						DiscKey discKey = new DiscKey(discID, discSeqID, dr.FreeItemID.Value);
						if (freeItems.ContainsKey(discKey))
						{
							freeItems[discKey] += dr.FreeItemQty.Value;
						}
						else
						{
							freeItems.Add(discKey, dr.FreeItemQty.Value);
						}
					}
				}
			}
		}

		private DiscountSequence GetDiscountSequenceByID(string discountID, string discountSequenceID)
		{
			return PXSelect<DiscountSequence,
				Where<DiscountSequence.discountID, Equal<Required<DiscountSequence.discountID>>,
				And<DiscountSequence.discountSequenceID, Equal<Required<DiscountSequence.discountSequenceID>>>>>.Select(this, discountID, discountSequenceID);

		}

		private void RecalculateFreeItemQtyTotal()
		{
			if (Document.Current != null)
			{
				Document.Cache.SetValueExt<SOShipment.freeItemQtyTot>(Document.Current, SumFreeItemQtyTotal());
			}
		}

		private decimal SumFreeItemQtyTotal()
		{
			PXSelectBase<SOShipmentDiscountDetail> select =
					new PXSelect<SOShipmentDiscountDetail,
					Where<SOShipmentDiscountDetail.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>>(this);

			decimal total = 0;
			foreach (SOShipmentDiscountDetail record in select.Select())
			{
				total += record.FreeItemQty ?? 0;
			}

			return total;
		}

		private void AdjustFreeItemLines()
		{
			foreach (SOShipLine line in FreeItems.Select())
			{
				if (line.ManualDisc != true)
					AdjustFreeItemLines(line);
			}

			Transactions.View.RequestRefresh();
		}

		private void AdjustFreeItemLines(SOShipLine line)
		{
			PXSelectBase<SOShipmentDiscountDetail> select = new PXSelect<SOShipmentDiscountDetail,
				Where<SOShipmentDiscountDetail.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>,
				And<SOShipmentDiscountDetail.freeItemID, Equal<Required<SOShipmentDiscountDetail.freeItemID>>,
				And<SOShipmentDiscountDetail.orderType, Equal<Required<SOShipmentDiscountDetail.orderType>>,
				And<SOShipmentDiscountDetail.orderNbr, Equal<Required<SOShipmentDiscountDetail.orderNbr>>>>>>>(this);

			decimal? qtyTotal = 0;
			foreach (SOShipmentDiscountDetail item in select.Select(line.InventoryID, line.OrigOrderType, line.OrigOrderNbr))
			{
				if (item.FreeItemID != null && item.FreeItemQty != null && item.FreeItemQty.Value > 0)
				{
					qtyTotal += item.FreeItemQty.Value;
				}
			}

            SOShipLine oldLine = PXCache<SOShipLine>.CreateCopy(line);
            oldLine.ShippedQty = qtyTotal;
            FreeItems.Update(oldLine);

            //Note: Do not delete Free item line if its qty = 0. 
            //New free item is not inserted if the qty of the original line is increased.
		}

		private void UpdateInsertDiscountTrace(SOShipmentDiscountDetail newTrace)
		{
			SOShipmentDiscountDetail trace = PXSelect<SOShipmentDiscountDetail,
					Where<SOShipmentDiscountDetail.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>,
					And<SOShipmentDiscountDetail.orderType, Equal<Required<SOShipmentDiscountDetail.orderType>>,
					And<SOShipmentDiscountDetail.orderNbr, Equal<Required<SOShipmentDiscountDetail.orderNbr>>,
					And<SOShipmentDiscountDetail.type, Equal<Required<SOShipmentDiscountDetail.type>>,
					And<SOShipmentDiscountDetail.discountID, Equal<Required<SOShipmentDiscountDetail.discountID>>,
					And<SOShipmentDiscountDetail.discountSequenceID, Equal<Required<SOShipmentDiscountDetail.discountSequenceID>>>>>>>>>.Select(this, newTrace.OrderType, newTrace.OrderNbr, newTrace.Type, newTrace.DiscountID, newTrace.DiscountSequenceID);

			if (trace != null)
			{
				trace.DiscountableQty = newTrace.DiscountableQty;
				trace.DiscountPct = newTrace.DiscountPct;
				trace.FreeItemID = newTrace.FreeItemID;
				trace.FreeItemQty = newTrace.FreeItemQty;

				DiscountDetails.Update(trace);
			}
			else
				DiscountDetails.Insert(newTrace);
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

		#region Packaging into boxes

		protected virtual IList<SOPackageEngine.PackSet> CalculatePackages(SOShipment shipment, out SOPackageEngine.PackSet manualPackSet)
		{
			Dictionary<string, SOPackageEngine.ItemStats> stats = new Dictionary<string, SOPackageEngine.ItemStats>();

			PXSelectBase<SOShipLine> selectLines = new PXSelectJoin<SOShipLine,
						InnerJoin<InventoryItem, On<SOShipLine.inventoryID, Equal<InventoryItem.inventoryID>>,
						InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOShipLine.origOrderType>, And<SOOrder.orderNbr, Equal<SOShipLine.origOrderNbr>>>>>,
						Where<SOShipLine.shipmentType, Equal<Required<SOShipment.shipmentType>>, And<SOShipLine.shipmentNbr, Equal<Required<SOShipment.shipmentNbr>>>>>(this);

			PXSelectBase<SOPackageInfoEx> selectManual = new PXSelect<SOPackageInfoEx, 
					Where<SOPackageInfoEx.orderType, Equal<Required<SOOrder.orderType>>, 
					And<SOPackageInfoEx.orderNbr, Equal<Required<SOOrder.orderNbr>>,
					And<SOPackageInfoEx.siteID, Equal<Required<SOPackageInfoEx.siteID>>>>>>(this);

			SOPackageEngine.OrderInfo orderInfo = new SOPackageEngine.OrderInfo(shipment.ShipVia);

			manualPackSet = new SOPackageEngine.PackSet(shipment.SiteID.Value);
			foreach (PXResult<SOShipLine, InventoryItem, SOOrder> res in selectLines.Select(shipment.ShipmentType, shipment.ShipmentNbr))
			{
				SOShipLine line = (SOShipLine)res;
				InventoryItem item = (InventoryItem)res;
				SOOrder order = (SOOrder) res;

				if (order.IsManualPackage == true || PXAccess.FeatureInstalled<FeaturesSet.autoPackaging>() == false)
				{
					foreach (SOPackageInfoEx box in selectManual.Select(order.OrderType, order.OrderNbr, shipment.SiteID))
					{
						manualPackSet.Packages.Add(box);
					}
				}
				else if (item.PackageOption != INPackageOption.Manual)
				{
					orderInfo.AddLine(item, line.BaseQty);

					int inventoryID = SOPackageEngine.ItemStats.Mixed;
					if (item.PackSeparately == true)
					{
						inventoryID = line.InventoryID.Value;
					}

					string key = string.Format("{0}.{1}.{2}.{3}", line.SiteID, inventoryID, item.PackageOption, line.Operation);

					SOPackageEngine.ItemStats stat;
					if (stats.ContainsKey(key))
					{
						stat = stats[key];
						stat.BaseQty += line.BaseQty.GetValueOrDefault();
						stat.BaseWeight += line.ExtWeight.GetValueOrDefault();
						stat.DeclaredValue += line.CuryLineAmt.GetValueOrDefault();
						stat.AddLine(item, line.BaseQty);
					}
					else
					{
						stat = new SOPackageEngine.ItemStats();
						stat.SiteID = line.SiteID;
						stat.InventoryID = inventoryID;
						stat.Operation = line.Operation;
						stat.PackOption = item.PackageOption;
						stat.BaseQty += line.BaseQty.GetValueOrDefault();
						stat.BaseWeight += line.ExtWeight.GetValueOrDefault();
						stat.DeclaredValue += line.CuryLineAmt.GetValueOrDefault();
						stat.AddLine(item, line.BaseQty);
						stats.Add(key, stat);
					}
				}
			}
			orderInfo.Stats.AddRange(stats.Values);
			
			SOPackageEngine engine = CreatePackageEngine();
			IList<SOPackageEngine.PackSet> result = engine.Pack(orderInfo);
			
			return result;
		}
		
		protected virtual SOPackageEngine CreatePackageEngine()
		{
			return new SOPackageEngine(this);
		}

		#endregion

		public virtual void ShipPackages(SOShipment shiporder)
		{
			if (IsWithLabels(shiporder.ShipVia) && shiporder.ShippedViaCarrier != true)
			{
				ICarrierService cs = CarrierMaint.CreateCarrierService(this, shiporder.ShipVia);
				CarrierRequest cr = BuildRequest(shiporder);
				if (cr.Packages.Count > 0)
				{
					CarrierResult<ShipResult> result = cs.Ship(cr);

					if (result != null)
					{
						StringBuilder sb = new StringBuilder();
						foreach (Message message in result.Messages)
						{
							sb.AppendFormat("{0}:{1} ", message.Code, message.Description);
						}

						if (result.IsSuccess)
						{
							using (PXTransactionScope ts = new PXTransactionScope())
							{
                                //re-read document, do not use passed object because it contains fills from Automation that will be committed even 
                                //if shipment confirmation will fail later.
                                Document.Current = Document.Search<SOShipment.shipmentNbr>(shiporder.ShipmentNbr); 
                                SOShipment copy = PXCache<SOShipment>.CreateCopy(Document.Current);

								decimal freightCost = 0;

								if (shiporder.UseCustomerAccount != true)
								{
									freightCost = ConvertAmtToBaseCury(result.Result.Cost.Currency, arsetup.Current.DefaultRateTypeID, shiporder.ShipDate.Value, result.Result.Cost.Amount);
								}

								copy.FreightCost = freightCost;
								CM.PXCurrencyAttribute.CuryConvCury<SOShipment.curyFreightCost>(Document.Cache, copy);

								PXResultset<SOShipLine> res = Transactions.Select();
								FreightCalculator fc = CreateFreightCalculator();
								fc.ApplyFreightTerms<SOShipment, SOShipment.curyFreightAmt>(Document.Cache, copy, res.Count);

								copy.ShippedViaCarrier = true;
								
								UploadFileMaintenance upload = PXGraph.CreateInstance<UploadFileMaintenance>();

								if (result.Result.Image != null)
								{
									string fileName = string.Format("High Value Report.{0}", result.Result.Format);
									FileInfo file = new FileInfo(fileName, null, result.Result.Image);
									upload.SaveFile(file, FileExistsAction.CreateVersion);
									PXNoteAttribute.SetFileNotes(Document.Cache, copy, file.UID.Value);
								}
								Document.Update(copy);

								foreach (PackageData pd in result.Result.Data)
								{
									SOPackageDetail sdp = PXSelect<SOPackageDetail,
										Where<SOPackageDetail.shipmentNbr, Equal<Required<SOShipment.shipmentNbr>>,
										And<SOPackageDetail.lineNbr, Equal<Required<SOPackageDetail.lineNbr>>>>>.Select(this, shiporder.ShipmentNbr, pd.RefNbr);

									if (sdp != null)
									{
										string fileName = string.Format("Label #{0}.{1}", pd.TrackingNumber, pd.Format);
										FileInfo file = new FileInfo(fileName, null, pd.Image);
										upload.SaveFile(file);

										sdp.TrackNumber = pd.TrackingNumber;
										sdp.TrackData = pd.TrackingData;
										PXNoteAttribute.SetFileNotes(Packages.Cache, sdp, file.UID.Value);
										Packages.Update(sdp);
									}
								}

								this.Save.Press();
								ts.Complete();
							}

							//show warnings:
							if (result.Messages.Count > 0)
							{
								Document.Cache.RaiseExceptionHandling<SOShipment.curyFreightCost>(shiporder, shiporder.CuryFreightCost,
									new PXSetPropertyException(sb.ToString(), PXErrorLevel.Warning));
							}

						}
						else
						{
							Document.Cache.RaiseExceptionHandling<SOShipment.curyFreightCost>(shiporder, shiporder.CuryFreightCost,
									new PXSetPropertyException(Messages.CarrierServiceError, PXErrorLevel.Error, sb.ToString()));

							throw new PXException(Messages.CarrierServiceError, sb.ToString());
						}

					}
				}
			}
		}

		public virtual void GetReturnLabels(SOShipment shiporder)
		{
			if (IsWithLabels(shiporder.ShipVia))
			{
				ICarrierService cs = CarrierMaint.CreateCarrierService(this, shiporder.ShipVia);
				CarrierRequest cr = BuildRequest(shiporder);
				if (cr.Packages.Count > 0)
				{
					CarrierResult<ShipResult> result = cs.Return(cr);

					if (result != null)
					{
						StringBuilder sb = new StringBuilder();
						foreach (Message message in result.Messages)
						{
							sb.AppendFormat("{0}:{1} ", message.Code, message.Description);
						}

						if (result.IsSuccess)
						{
							foreach (SOPackageDetail pd in PXSelect<SOPackageDetail, Where<SOPackageDetail.shipmentNbr, Equal<Required<SOShipment.shipmentNbr>>>>.Select(this, shiporder.ShipmentNbr))
							{
								foreach (NoteDoc nd in PXSelect<NoteDoc, Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.Select(this, pd.NoteID))
								{
									UploadFileMaintenance.DeleteFile(nd.FileID);
								}
							}

							using (PXTransactionScope ts = new PXTransactionScope())
							{
								UploadFileMaintenance upload = PXGraph.CreateInstance<UploadFileMaintenance>();

								foreach (PackageData pd in result.Result.Data)
								{
									SOPackageDetail sdp = PXSelect<SOPackageDetail,
										Where<SOPackageDetail.shipmentNbr, Equal<Required<SOShipment.shipmentNbr>>,
										And<SOPackageDetail.lineNbr, Equal<Required<SOPackageDetail.lineNbr>>>>>.Select(this, shiporder.ShipmentNbr, pd.RefNbr);

									if (sdp != null)
									{
										string fileName = string.Format("ReturnLabel #{0}.{1}", pd.TrackingNumber, pd.Format);
										FileInfo file = new FileInfo(fileName, null, pd.Image);
										upload.SaveFile(file);

										sdp.TrackNumber = pd.TrackingNumber;
										sdp.TrackData = pd.TrackingData;
										PXNoteAttribute.SetFileNotes(Packages.Cache, sdp, file.UID.Value);
										Packages.Update(sdp);
									}
								}

								this.Save.Press();
								ts.Complete();
							}

							//show warnings:
							if (result.Messages.Count > 0)
							{
								Document.Cache.RaiseExceptionHandling<SOShipment.curyFreightCost>(shiporder, shiporder.CuryFreightCost,
									new PXSetPropertyException(sb.ToString(), PXErrorLevel.Warning));
							}

						}
						else
						{
							Document.Cache.RaiseExceptionHandling<SOShipment.curyFreightCost>(shiporder, shiporder.CuryFreightCost,
									new PXSetPropertyException(Messages.CarrierServiceError, PXErrorLevel.Error, sb.ToString()));

							throw new PXException(Messages.CarrierServiceError, sb.ToString());
						}

					}
				}
			}
		}

		protected virtual FreightCalculator CreateFreightCalculator()
		{
			return new FreightCalculator(this);
		}

		private void CancelPackages(SOShipment shiporder)
		{
			if (IsWithLabels(shiporder.ShipVia) && shiporder.ShippedViaCarrier == true)
			{
				ICarrierService cs = CarrierMaint.CreateCarrierService(this, shiporder.ShipVia);

				SOPackageDetail sdp = PXSelect<SOPackageDetail,
					Where<SOPackageDetail.shipmentNbr, Equal<Required<SOShipment.shipmentNbr>>>>.SelectWindowed(this, 0, 1, shiporder.ShipmentNbr);

				if (sdp != null && !string.IsNullOrEmpty(sdp.TrackNumber))
				{
					CarrierResult<string> result = cs.Cancel(sdp.TrackNumber, sdp.TrackData);

					if (result != null)
					{
						StringBuilder sb = new StringBuilder();
						foreach (Message message in result.Messages)
						{
							sb.AppendFormat("{0}:{1} ", message.Code, message.Description);
						}

						//Clear Tracking numbers no matter where the call to the carrier were successfull or not

						foreach (SOPackageDetail pd in PXSelect<SOPackageDetail, Where<SOPackageDetail.shipmentNbr, Equal<Required<SOShipment.shipmentNbr>>>>.Select(this, shiporder.ShipmentNbr))
						{
							pd.Confirmed = false;
							pd.TrackNumber = null;
							Packages.Update(pd);

							foreach (NoteDoc nd in PXSelect<NoteDoc, Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.Select(this, pd.NoteID))
							{
								UploadFileMaintenance.DeleteFile(nd.FileID);
							}
						}

						shiporder.CuryFreightCost = 0;
						shiporder.CuryFreightAmt = 0;
						shiporder.ShippedViaCarrier = false;
						Document.Update(shiporder);

						this.Save.Press();

						//Log errors if any: (Log Errors/Warnings to Trace do not return them - In processing warning are displayed as errors (( )
						//CancelPackages should not throw Exceptions since CorrectShipment follows it and must be executed.
						if (!result.IsSuccess)
						{
							//Document.Cache.RaiseExceptionHandling<SOPackageDetail.trackNumber>(shiporder, shiporder.CuryFreightCost,
							//        new PXSetPropertyException(Messages.CarrierServiceError, PXErrorLevel.Error, sb.ToString()));

							//throw new PXException(Messages.CarrierServiceError, sb.ToString());

							PXTrace.WriteWarning("Tracking Numbers and Labels for the shipment was succesfully cleared but Carrier Void Service Returned Error: " + sb.ToString());
						}
						else
						{
							//show warnings:
							if (result.Messages.Count > 0)
							{
								//Document.Cache.RaiseExceptionHandling<SOPackageDetail.trackNumber>(shiporder, shiporder.CuryFreightCost,
								//    new PXSetPropertyException(sb.ToString(), PXErrorLevel.Warning));

								PXTrace.WriteWarning("Tracking Numbers and Labels for the shipment was succesfully cleared but Carrier Void Service Returned Warnings: " + sb.ToString());
							}
						}
					}
				}
			}
		}

		private void PrintCarrierLabels()
		{
			if (Document.Current != null)
			{
				if (Document.Current.LabelsPrinted == true)
				{
					WebDialogResult result = Document.View.Ask(Document.Current, GL.Messages.Confirmation, Messages.ReprintLabels, MessageButtons.YesNo, MessageIcon.Question);
					if (result != WebDialogResult.Yes)
					{
						return;
					}
					else
					{
						PXTrace.WriteInformation("User Forced Labels Reprint for Shipment {0}", Document.Current.ShipmentNbr);
					}
				}

				PXResultset<SOPackageDetail> packagesResultset = Packages.Select();

				if (packagesResultset.Count > 0)
				{
					SOPackageDetail firstRecord = (SOPackageDetail)packagesResultset[0];
					UploadFileMaintenance upload = PXGraph.CreateInstance<UploadFileMaintenance>();

					Guid[] uids = PXNoteAttribute.GetFileNotes(Packages.Cache, firstRecord);
					if (uids.Length > 0)
					{
						FileInfo fileInfo = upload.GetFile(uids[0]);
						Document.Current.LabelsPrinted = true;
						Document.Update(Document.Current);
						this.Save.Press();

						if (IsThermalPrinter(fileInfo))
						{
							// merge and print
							IList<FileInfo> lableFiles = GetLabelFiles(packagesResultset);
							if (lableFiles.Count > 0)
							{
								FileInfo mergedFile = MergeFiles(lableFiles);
								if (upload.SaveFile(mergedFile))
								{
									string targetUrl = string.Format("~/Frames/GetFile.ashx?fileID={0}", mergedFile.UID);
									throw new PXRedirectToUrlException(targetUrl, "Print Labels");
								}
								else
								{
									PXTrace.WriteError("Failed to Save Merged file for Shipment {0}", Document.Current.ShipmentNbr);
								}
							}
							else
							{
								PXTrace.WriteWarning("No Label files to merge for Shipment {0}", Document.Current.ShipmentNbr);
							}
						}
						else
						{
							//open report

							Dictionary<string, string> parameters = new Dictionary<string, string>();
							parameters["shipmentNbr"] = Document.Current.ShipmentNbr;

							throw new PXReportRequiredException(parameters, "SO645000", "Report");
						}
					}
					else
					{
						PXTrace.WriteWarning("No Label files to print for Shipment {0}", Document.Current.ShipmentNbr);
					}
				}
			}
		}

		private void PrintCarrierLabels(List<SOShipment> list)
		{
			List<string> laserLabels = new List<string>();
			List<FileInfo> lableFiles = new List<FileInfo>();

			string targetUrl = null;
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				UploadFileMaintenance upload = PXGraph.CreateInstance<UploadFileMaintenance>();

				foreach (SOShipment shiporder in list)
				{
					PXResultset<SOPackageDetail> packagesResultset = PXSelect<SOPackageDetail, Where<SOPackageDetail.shipmentNbr, Equal<Required<SOShipment.shipmentNbr>>>>.Select(this, shiporder.ShipmentNbr);

					if (packagesResultset.Count > 0)
					{
						SOPackageDetail firstRecord = (SOPackageDetail)packagesResultset[0];

						Guid[] uids = PXNoteAttribute.GetFileNotes(Packages.Cache, firstRecord);
						if (uids.Length > 0)
						{
							FileInfo fileInfo = upload.GetFile(uids[0]);

							if (IsThermalPrinter(fileInfo))
							{
								lableFiles.AddRange(GetLabelFiles(packagesResultset));
							}
							else
							{
								laserLabels.Add(shiporder.ShipmentNbr);
							}
						}
						else
						{
							PXTrace.WriteWarning("No Label files to print for Shipment {0}", shiporder.ShipmentNbr);
						}
					}

					shiporder.LabelsPrinted = true;
					Document.Update(shiporder);
				}


				if (lableFiles.Count > 0 && laserLabels.Count > 0)
				{
					throw new PXException(Messages.MixedFormat, lableFiles.Count, laserLabels.Count);
				}

				if (lableFiles.Count > 0)
				{
					FileInfo mergedFile = MergeFiles(lableFiles);
					if (upload.SaveFile(mergedFile))
					{
						targetUrl = string.Format("~/Frames/GetFile.ashx?fileID={0}", mergedFile.UID);
					}
					else
					{
						throw new PXException(Messages.FailedToSaveMergedFile);
					}
				}

				this.Save.Press();
				ts.Complete();
			}

			if (targetUrl != null)
				throw new PXRedirectToUrlException(targetUrl, "Print Labels");

			if (laserLabels.Count > 0)
			{
				int i = 0;
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				foreach (string shipNbr in laserLabels)
				{
					parameters["SOShipment.ShipmentNbr" + i.ToString()] = shipNbr;
					i++;
				}
				if (i > 0)
				{
					throw new PXReportRequiredException(parameters, "SO645000", "Report");
				}
			}
		}

		private bool IsWithLabels(string shipVia)
		{
			Carrier carrier = PXSelect<Carrier, Where<Carrier.carrierID, Equal<Required<SOShipment.shipVia>>>>.Select(this, shipVia);
			if (carrier != null && carrier.IsExternal == true)
				return true;

			return false;
		}

		private bool IsThermalPrinter(FileInfo fileInfo)
		{
			if (System.IO.Path.HasExtension(fileInfo.Name))
			{
				string extension = System.IO.Path.GetExtension(fileInfo.Name).Substring(1).ToLower();
				if (extension.Length > 2)
				{
					string ext = extension.Substring(0, 3);
					if (ext == "zpl" || ext == "epl" || ext == "dpl" || ext == "spl" || extension == "starpl")
						return true;
					else
						return false;
				}
				else
					return false;
			}
			else
				return false;


		}

		private IList<FileInfo> GetLabelFiles(PXResultset<SOPackageDetail> resultset)
		{
			List<FileInfo> list = new List<FileInfo>(resultset.Count);
			UploadFileMaintenance upload = PXGraph.CreateInstance<UploadFileMaintenance>();

			foreach (SOPackageDetail pack in Packages.Select())
			{
				Guid[] ids = PXNoteAttribute.GetFileNotes(Packages.Cache, pack);
				if (ids.Length > 0)
				{
					if (ids.Length > 1)
					{
						PXTrace.WriteWarning("There are more then one file attached to the package. But only first will be processed for Shipment {0}/{1}", Document.Current.ShipmentNbr, pack.LineNbr);
					}


					FileInfo fileInfo = upload.GetFile(ids[0]);

					list.Add(fileInfo);

				}
			}

			return list;
		}

		private FileInfo MergeFiles(IList<FileInfo> files)
		{
			FileInfo result;
			using (System.IO.MemoryStream mem = new System.IO.MemoryStream())
			{
				string extension = null;

				foreach (FileInfo file in files)
				{
					string ext = System.IO.Path.GetExtension(file.Name);

					if (extension == null)
					{
						extension = ext;
					}
					else
					{
						if (ext != extension)
							throw new PXException("Cannot merge files with different formats (extensions)");
					}

					mem.Write(file.BinData, 0, file.BinData.Length);
				}

				//since we map file extensions with bat file will use .zpl for all thermal files:
				string fileName = Guid.NewGuid().ToString() + extension; // ".zpl"; //extension;

				result = new FileInfo(fileName, null, mem.ToArray());
			}

			return result;
		}

		private CarrierRequest BuildRequest(SOShipment shiporder)
		{
			INSite warehouse = PXSelect<INSite, Where<INSite.siteID, Equal<Required<SOShipment.siteID>>>>.Select(this, shiporder.SiteID);
			if (warehouse == null)
			{
				Document.Cache.RaiseExceptionHandling<SOShipment.siteID>(shiporder, shiporder.SiteID,
							new PXSetPropertyException(Messages.WarehouseIsRequired, PXErrorLevel.Error));

				throw new PXException(Messages.WarehouseIsRequired);
			}

			SOShipmentAddress shipAddress = PXSelect<SOShipmentAddress, Where<SOShipmentAddress.addressID, Equal<Required<SOShipment.shipAddressID>>>>.Select(this, shiporder.ShipAddressID);
			SOShipmentContact shipToContact = PXSelect<SOShipmentContact, Where<SOShipmentContact.contactID, Equal<Required<SOShipment.shipContactID>>>>.Select(this, shiporder.ShipContactID);
			Address warehouseAddress = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(this, warehouse.AddressID);
			Contact warehouseContact = PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.Select(this, warehouse.ContactID);


			BAccount companyAccount = PXSelectJoin<BAccountR, InnerJoin<GL.Branch, On<GL.Branch.bAccountID, Equal<BAccountR.bAccountID>>>, Where<GL.Branch.branchID, Equal<Required<GL.Branch.branchID>>>>.Select(this, warehouse.BranchID);
			Address shipperAddress = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(this, companyAccount.DefAddressID);
			Contact shipperContact = PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.Select(this, companyAccount.DefContactID);
			
			UnitsType unit = UnitsType.SI;
			Carrier carrier = PXSelect<Carrier, Where<Carrier.carrierID, Equal<Required<SOOrder.shipVia>>>>.Select(this, shiporder.ShipVia);
			CarrierPlugin plugin = PXSelect<CarrierPlugin, Where<CarrierPlugin.carrierPluginID, Equal<Required<CarrierPlugin.carrierPluginID>>>>.Select(this, carrier.CarrierPluginID);

			if (string.IsNullOrEmpty(plugin.UOM))
			{
				throw new PXException(Messages.CarrierUOMIsRequired);
			}

			if (plugin.UnitType == CarrierUnitsType.US)
			{
				unit = UnitsType.US;
			}

			CarrierRequest cr = new CarrierRequest(unit, shiporder.CuryID);

			//by customer and location
			CarrierPluginCustomer cpc = PXSelect<CarrierPluginCustomer, Where<CarrierPluginCustomer.carrierPluginID, Equal<Required<CarrierPluginCustomer.carrierPluginID>>,
				And<CarrierPluginCustomer.customerID, Equal<Required<CarrierPluginCustomer.customerID>>,
				And<CarrierPluginCustomer.customerLocationID, Equal<Required<CarrierPluginCustomer.customerLocationID>>,
				And<CarrierPluginCustomer.isActive, Equal<True>>>>>>.Select(this, plugin.CarrierPluginID, shiporder.CustomerID, shiporder.CustomerLocationID);

			if (cpc == null)
			{
				//only by cystomer
				cpc = PXSelect<CarrierPluginCustomer, Where<CarrierPluginCustomer.carrierPluginID, Equal<Required<CarrierPluginCustomer.carrierPluginID>>,
				And<CarrierPluginCustomer.customerID, Equal<Required<CarrierPluginCustomer.customerID>>,
				And<CarrierPluginCustomer.customerLocationID, IsNull,
				And<CarrierPluginCustomer.isActive, Equal<True>>>>>>.Select(this, plugin.CarrierPluginID, shiporder.CustomerID);
			}


			if (cpc != null && !string.IsNullOrEmpty(cpc.CarrierAccount) && shiporder.UseCustomerAccount == true )
			{
				cr.ThirdPartyAccountID = cpc.CarrierAccount;

				Location customerLocation = PXSelect<Location, Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,	And<Location.locationID, Equal<Required<Location.locationID>>>>>.Select(this, shiporder.CustomerID, shiporder.CustomerLocationID);
				Address customerAddress = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(this, customerLocation.DefAddressID);
				cr.ThirdPartyPostalCode = cpc.PostalCode ?? customerAddress.PostalCode;
				cr.ThirdPartyCountryCode = customerAddress.CountryID;
			}
			
			cr.Shipper = shipperAddress;
			cr.ShipperContact = shipperContact;
			cr.Origin = warehouseAddress;
			cr.OriginContact = warehouseContact;
			cr.Destination = shipAddress;
			cr.DestinationContact = shipToContact;
			cr.Packages = GetPackages(shiporder, carrier, plugin);
			cr.Resedential = shiporder.Resedential == true;
			cr.SaturdayDelivery = shiporder.SaturdayDelivery == true;
			cr.Insurance = shiporder.Insurance == true;
			if (DateTime.Now.Date < shiporder.ShipDate.Value.Date)
				cr.ShipDate = shiporder.ShipDate.Value;
			else
				cr.ShipDate = DateTime.Now.Date;

			cr.Attributes = new List<string>();

			if (Document.Current != null && Document.Current.GroundCollect == true)
			{
				cr.Attributes.Add("COLLECT");
			}
			cr.InvoiceLineTotal = shiporder.LineTotal.GetValueOrDefault();

			return cr;
		}

		public override void Persist()
		{
			if (Document.Current != null && Document.Current.IsPackageValid != true &&
				Document.Current.Released != true && Document.Current.Confirmed != true && Document.Current.SiteID != null)
			{
				recalculatePackages.Press();
			}

			base.Persist();
		}

		private IList<CarrierBox> GetPackages(SOShipment shiporder, Carrier carrier, CarrierPlugin plugin)
		{
			List<CarrierBox> list = new List<CarrierBox>();

			PXSelectBase<SOPackageDetail> select = new PXSelectJoin<SOPackageDetail,
			InnerJoin<PX.Objects.CS.CSBox, On<SOPackageDetail.boxID, Equal<PX.Objects.CS.CSBox.boxID>>>,
			Where<SOPackageDetail.shipmentNbr, Equal<Required<SOShipment.shipmentNbr>>>>(this);

			bool failed = false;
			foreach (PXResult<SOPackageDetail, PX.Objects.CS.CSBox> res in select.Select(shiporder.ShipmentNbr))
			{
				SOPackageDetail detail = (SOPackageDetail)res;
				PX.Objects.CS.CSBox package = (PX.Objects.CS.CSBox)res;

				if (carrier.ConfirmationRequired == true)
				{
					if (detail.Confirmed != true)
					{
						failed = true;

						Packages.Cache.RaiseExceptionHandling<SOPackageDetail.confirmed>(detail, detail.Confirmed,
							new PXSetPropertyException(Messages.ConfirmationIsRequired, PXErrorLevel.Error));
					}
				}

				decimal weightInStandardUnit = CarrierMaint.ConvertGlobalUnits(this, insetup.Current.WeightUOM, plugin.UOM, detail.Weight ?? 0);

				CarrierBox box = new CarrierBox(detail.LineNbr.Value, weightInStandardUnit);
				box.Description = detail.Description;
				box.DeclaredValue = detail.DeclaredValue ?? 0;
				box.COD = detail.COD ?? 0;
				box.Length = package.Length ?? 0;
				box.Width = package.Width ?? 0;
				box.Height = package.Height ?? 0;
				box.CarrierPackage = package.CarrierBox;
				box.CustomRefNbr1 = detail.CustomRefNbr1;
				box.CustomRefNbr2 = detail.CustomRefNbr2;

				list.Add(box);
			}

			if (failed)
			{
				throw new PXException(Messages.ConfirmationIsRequired);
			}

			return list;
		}

		private decimal ConvertAmtToBaseCury(string from, string rateType, DateTime effectiveDate, decimal amount)
		{
			decimal result = amount;

			using (ReadOnlyScope rs = new ReadOnlyScope(DummyCuryInfo.Cache))
			{
				CurrencyInfo ci = new CurrencyInfo();
				ci.CuryRateTypeID = rateType;
				ci.CuryID = from;
				ci = (CurrencyInfo)DummyCuryInfo.Cache.Insert(ci);
				ci.SetCuryEffDate(DummyCuryInfo.Cache, effectiveDate);
				DummyCuryInfo.Cache.Update(ci);
				PXCurrencyAttribute.CuryConvBase(DummyCuryInfo.Cache, ci, amount, out result);
				DummyCuryInfo.Cache.Delete(ci);
			}

			return result;
		}
	}

    public class SOShipmentException : PXException
    {
        public SOShipmentException(string message, params object[] args)
            :base(message, args)
        { 
        }

        public SOShipmentException(string message)
            :base(message)
        { 
        }


		public SOShipmentException(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
    }

	[PXProjection(typeof(Select5<POReceiptLine,
		InnerJoin<POReceipt, On<POReceipt.receiptNbr, Equal<POReceiptLine.receiptNbr>>,
		InnerJoin<SOLine, On<SOLine.pOType, Equal<POReceiptLine.pOType>, And<SOLine.pONbr, Equal<POReceiptLine.pONbr>, And<SOLine.pOLineNbr, Equal<POReceiptLine.pOLineNbr>>>>>>,
		Where2<Where<POReceiptLine.lineType, Equal<POLineType.goodsForDropShip>, Or<POReceiptLine.lineType, Equal<POLineType.nonStockForDropShip>>>, And<POReceipt.released, Equal<boolTrue>>>,
		Aggregate<
			GroupBy<POReceiptLine.receiptNbr,
			GroupBy<SOLine.orderType,
			GroupBy<SOLine.orderNbr,
			Sum<POReceiptLine.extWeight,
			Sum<POReceiptLine.extVolume,
			Sum<POReceiptLine.receiptQty>>>>>>>>))]
    [Serializable]
	public partial class SOOrderReceipt : IBqlTable
	{
		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.IBqlField
		{
		}
		protected String _ReceiptNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(POReceiptLine.receiptNbr))]
		public virtual String ReceiptNbr
		{
			get
			{
				return this._ReceiptNbr;
			}
			set
			{
				this._ReceiptNbr = value;
			}
		}
		#endregion
		#region ReceiptDate
		public abstract class receiptDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _ReceiptDate;
		[PXDBDate(BqlField = typeof(POReceipt.receiptDate))]
		public virtual DateTime? ReceiptDate
		{
			get
			{
				return this._ReceiptDate;
			}
			set
			{
				this._ReceiptDate = value;
			}
		}
		#endregion
		#region ReceiptQty
		public abstract class receiptQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _ReceiptQty;
		[PXDBQuantity(BqlField = typeof(POReceiptLine.receiptQty))]
		public virtual Decimal? ReceiptQty
		{
			get
			{
				return this._ReceiptQty;
			}
			set
			{
				this._ReceiptQty = value;
			}
		}
		#endregion
		#region ExtWeight
		public abstract class extWeight : PX.Data.IBqlField
		{
		}
		protected Decimal? _ExtWeight;
		[PXDBDecimal(6, BqlField = typeof(POReceiptLine.extWeight))]
		public virtual Decimal? ExtWeight
		{
			get
			{
				return this._ExtWeight;
			}
			set
			{
				this._ExtWeight = value;
			}
		}
		#endregion
		#region ExtVolume
		public abstract class extVolume : PX.Data.IBqlField
		{
		}
		protected Decimal? _ExtVolume;
		[PXDBDecimal(6, BqlField = typeof(POReceiptLine.extVolume))]
		public virtual Decimal? ExtVolume
		{
			get
			{
				return this._ExtVolume;
			}
			set
			{
				this._ExtVolume = value;
			}
		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.IBqlField
		{
		}
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(SOLine.orderType))]
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.IBqlField
		{
		}
		protected String _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(SOLine.orderNbr))]
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.IBqlField
		{
		}
		protected Int32? _CustomerID;
		[PXDBInt(BqlField = typeof(SOOrder.customerID))]
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
		#region CustomerLocationID
		public abstract class customerLocationID : PX.Data.IBqlField
		{
		}
		protected Int32? _CustomerLocationID;
		[PXDBInt(BqlField = typeof(SOOrder.customerLocationID))]
		public virtual Int32? CustomerLocationID
		{
			get
			{
				return this._CustomerLocationID;
			}
			set
			{
				this._CustomerLocationID = value;
			}
		}
		#endregion
	}

	[PXProjection(typeof(Select2<SOOrder,
		InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrder.orderType>>,
		InnerJoin<INItemPlan, On<INItemPlan.refNoteID, Equal<SOOrder.noteID>>,
		InnerJoin<INPlanType, On<INPlanType.planType, Equal<INItemPlan.planType>>>>>,
	Where<INItemPlan.hold, Equal<boolFalse>,
	  And<INItemPlan.planQty, Greater<decimal0>,
		And<INPlanType.isDemand, Equal<boolTrue>,
		And<INPlanType.isFixed, Equal<boolFalse>,
		And<INPlanType.isForDate, Equal<boolTrue>>>>>>>))]
    [Serializable]
	public partial class SOShipmentPlan : IBqlTable
	{
		#region Selected
		public abstract class selected : IBqlField
		{
		}
		protected bool? _Selected = false;
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.IBqlField
		{
		}
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, InputMask = ">LL", BqlField = typeof(SOOrder.orderType))]
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.IBqlField
		{
		}
		protected String _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(SOOrder.orderNbr))]
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
		#region DestinationSiteID
		public abstract class destinationSiteID : PX.Data.IBqlField
		{
		}
		protected Int32? _DestinationSiteID;
		[PXDefault()]
		[IN.Site(DisplayName = "Destination Warehouse", DescriptionField = typeof(INSite.descr), BqlField = typeof(SOOrder.destinationSiteID))]
		public virtual Int32? DestinationSiteID
		{
			get
			{
				return this._DestinationSiteID;
			}
			set
			{
				this._DestinationSiteID = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[PXDBInt(BqlField = typeof(INItemPlan.inventoryID))]
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
		#region SubItemID
		public abstract class subItemID : PX.Data.IBqlField
		{
		}
		protected Int32? _SubItemID;
		[PXDBInt(BqlField = typeof(INItemPlan.subItemID))]
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
		[PXDBInt(BqlField = typeof(INItemPlan.siteID))]
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
		#region PlanDate
		public abstract class planDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _PlanDate;
		[PXDBDate(BqlField = typeof(INItemPlan.planDate))]
		[PXUIField(DisplayName = "Sched. Ship. Date")]
		public virtual DateTime? PlanDate
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
		public abstract class planID : PX.Data.IBqlField
		{
		}
		protected Int64? _PlanID;
		[PXDBLong(IsKey = true, BqlField = typeof(INItemPlan.planID))]
		public virtual Int64? PlanID
		{
			get
			{
				return this._PlanID;
			}
			set
			{
				this._PlanID = value;
			}
		}
		#endregion
		#region PlanQty
		public abstract class planQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _PlanQty;
		[PXDBDecimal(6, BqlField = typeof(INItemPlan.planQty))]
		public virtual Decimal? PlanQty
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
		#region InclQtySOBackOrdered
		public abstract class inclQtySOBackOrdered : PX.Data.IBqlField
		{
		}
		protected Int16? _InclQtySOBackOrdered;
		[PXDBShort(BqlField=typeof(INPlanType.inclQtySOBackOrdered))]
		public virtual Int16? InclQtySOBackOrdered
		{
			get
			{
				return this._InclQtySOBackOrdered;
			}
			set
			{
				this._InclQtySOBackOrdered = value;
			}
		}
		#endregion
		#region InclQtySOShipping
		public abstract class inclQtySOShipping : PX.Data.IBqlField
		{
		}
		protected Int16? _InclQtySOShipping;
		[PXDBShort(BqlField = typeof(INPlanType.inclQtySOShipping))]
		public virtual Int16? InclQtySOShipping
		{
			get
			{
				return this._InclQtySOShipping;
			}
			set
			{
				this._InclQtySOShipping = value;
			}
		}
		#endregion
		#region InclQtySOShipped
		public abstract class inclQtySOShipped : PX.Data.IBqlField
		{
		}
		protected Int16? _InclQtySOShipped;
		[PXDBShort(BqlField = typeof(INPlanType.inclQtySOShipped))]
		public virtual Int16? InclQtySOShipped
		{
			get
			{
				return this._InclQtySOShipped;
			}
			set
			{
				this._InclQtySOShipped = value;
			}
		}
		#endregion
		#region RequireAllocation
		public abstract class requireAllocation : PX.Data.IBqlField
		{
		}
		protected Boolean? _RequireAllocation;
		[PXDBBool(BqlField = typeof(SOOrderType.requireAllocation))]
		public virtual Boolean? RequireAllocation
		{
			get
			{
				return this._RequireAllocation;
			}
			set
			{
				this._RequireAllocation = value;
			}
		}
		#endregion	
	}


	[PXProjection(typeof(Select2<SOLine, 
		InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOLine.orderType>>, 			
		InnerJoin<SOOrderTypeOperation,
		      On<SOOrderTypeOperation.orderType, Equal<SOLine.orderType>,
				And<SOOrderTypeOperation.operation, Equal<SOLine.operation>>>>>,
		Where<SOLine.lineType, NotEqual<SOLineType.miscCharge>>>), Persistent = true)]
    [Serializable]
	public partial class SOLine2 : IBqlTable, IItemPlanMaster
    {
		#region OrderType
		public abstract class orderType : PX.Data.IBqlField
		{
		}
		protected string _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(SOLine.orderType))]
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.IBqlField
		{
		}
		protected string _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(SOLine.orderNbr))]
		[PXParent(typeof(Select<SOOrder, Where<SOOrder.orderType, Equal<Current<SOLine2.orderType>>, And<SOOrder.orderNbr, Equal<Current<SOLine2.orderNbr>>>>>))]
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(SOLine.lineNbr))]
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
		#region LineType
		public abstract class lineType : PX.Data.IBqlField
		{
		}
		protected String _LineType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(SOLine.lineType))]
		public virtual String LineType
		{
			get
			{
				return this._LineType;
			}
			set
			{
				this._LineType = value;
			}
		}
		#endregion
		#region Operation
		public abstract class operation : PX.Data.IBqlField
		{
		}
		protected String _Operation;
		[PXDBString(1, IsFixed = true, InputMask = ">a", BqlField = typeof(SOLine.operation))]
		[PXUIField(DisplayName = "Operation")]		
		[SOOperation.List]
		public virtual String Operation
		{
			get
			{
				return this._Operation;
			}
			set
			{
				this._Operation = value;
			}
		}
		#endregion
		#region ShipComplete
		public abstract class shipComplete : PX.Data.IBqlField
		{
		}
		protected String _ShipComplete;
		[PXDBString(1, IsFixed = true, BqlField = typeof(SOLine.shipComplete))]
		public virtual String ShipComplete
		{
			get
			{
				return this._ShipComplete;
			}
			set
			{
				this._ShipComplete = value;
			}
		}
		#endregion
		#region Cancelled
		public abstract class cancelled : PX.Data.IBqlField
		{
		}
		protected Boolean? _Cancelled;
		[PXDBBool(BqlField = typeof(SOLine.cancelled))]
		public virtual Boolean? Cancelled
		{
			get
			{
				return this._Cancelled;
			}
			set
			{
				this._Cancelled = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[PXDBInt(BqlField = typeof(SOLine.inventoryID))]
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
		#region SiteID
		public abstract class siteID : PX.Data.IBqlField
		{
		}
		protected Int32? _SiteID;
		[PXDBInt(BqlField = typeof(SOLine.siteID))]
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
		#region SalesAcctID
		public abstract class salesAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _SalesAcctID;
		[PXDBInt(BqlField = typeof(SOLine.salesAcctID))]
		public virtual Int32? SalesAcctID
		{
			get
			{
				return this._SalesAcctID;
			}
			set
			{
				this._SalesAcctID = value;
			}
		}
		#endregion
		#region SalesSubID
		public abstract class salesSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _SalesSubID;
		[PXDBInt(BqlField = typeof(SOLine.salesSubID))]
		public virtual Int32? SalesSubID
		{
			get
			{
				return this._SalesSubID;
			}
			set
			{
				this._SalesSubID = value;
			}
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.IBqlField
		{
		}
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true, BqlField = typeof(SOLine.tranDesc))]
		public virtual String TranDesc
		{
			get
			{
				return this._TranDesc;
			}
			set
			{
				this._TranDesc = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.IBqlField
		{
		}
		protected String _UOM;
		[INUnit(typeof(SOLine2.inventoryID), BqlField = typeof(SOLine.uOM))]
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
		#region OrderQty
		public abstract class orderQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _OrderQty;
		[PXDBDecimal(6, BqlField = typeof(SOLine.orderQty))]
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
		#region ShippedQty
		public abstract class shippedQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _ShippedQty;
		[PXDBDecimal(6, BqlField = typeof(SOLine.shippedQty))]
		public virtual Decimal? ShippedQty
		{
			get
			{
				return this._ShippedQty;
			}
			set
			{
				this._ShippedQty = value;
			}
		}
		#endregion
		#region BaseShippedQty
		public abstract class baseShippedQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _BaseShippedQty;
		[PXDBBaseQuantity(typeof(SOLine2.uOM), typeof(SOLine2.shippedQty), BqlField = typeof(SOLine.baseShippedQty))]
		public virtual Decimal? BaseShippedQty
		{
			get
			{
				return this._BaseShippedQty;
			}
			set
			{
				this._BaseShippedQty = value;
			}
		}
		#endregion
		#region BilledQty
		public abstract class billedQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _BilledQty;
		[PXDBDecimal(6, BqlField = typeof(SOLine.billedQty))]
		public virtual Decimal? BilledQty
		{
			get
			{
				return this._BilledQty;
			}
			set
			{
				this._BilledQty = value;
			}
		}
		#endregion
		#region BaseBilledQty
		public abstract class baseBilledQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _BaseBilledQty;
		[PXDBBaseQuantity(typeof(SOLine2.uOM), typeof(SOLine2.billedQty), BqlField = typeof(SOLine.baseBilledQty))]
		public virtual Decimal? BaseBilledQty
		{
			get
			{
				return this._BaseBilledQty;
			}
			set
			{
				this._BaseBilledQty = value;
			}
		}
		#endregion
		#region UnbilledQty
		public abstract class unbilledQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _UnbilledQty;
		[PXDBQuantity(BqlField = typeof(SOLine.unbilledQty))]
		[PXFormula(null, typeof(SumCalc<SOOrder.unbilledOrderQty>))]
		public virtual Decimal? UnbilledQty
		{
			get
			{
				return this._UnbilledQty;
			}
			set
			{
				this._UnbilledQty = value;
			}
		}
		#endregion
		#region BaseUnbilledQty
		public abstract class baseUnbilledQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _BaseUnbilledQty;
		[PXDBBaseQuantity(typeof(SOLine2.uOM), typeof(SOLine2.unbilledQty), BqlField = typeof(SOLine.baseUnbilledQty))]
		public virtual Decimal? BaseUnbilledQty
		{
			get
			{
				return this._BaseUnbilledQty;
			}
			set
			{
				this._BaseUnbilledQty = value;
			}
		}
		#endregion
		#region CompleteQtyMin
		public abstract class completeQtyMin : PX.Data.IBqlField
		{
		}
		protected Decimal? _CompleteQtyMin;
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 99.0, BqlField = typeof(SOLine.completeQtyMin))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CompleteQtyMin
		{
			get
			{
				return this._CompleteQtyMin;
			}
			set
			{
				this._CompleteQtyMin = value;
			}
		}
		#endregion
		#region CompleteQtyMax
		public abstract class completeQtyMax : PX.Data.IBqlField
		{
		}
		protected Decimal? _CompleteQtyMax;
		[PXDBDecimal(2, MinValue = 100.0, MaxValue = 999.0, BqlField = typeof(SOLine.completeQtyMax))]
		[PXDefault(TypeCode.Decimal, "100.0")]
		public virtual Decimal? CompleteQtyMax
		{
			get
			{
				return this._CompleteQtyMax;
			}
			set
			{
				this._CompleteQtyMax = value;
			}
		}
		#endregion
		#region ShipDate
		public abstract class shipDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _ShipDate;
		[PXDBDate(BqlField = typeof(SOLine.shipDate))]
		public virtual DateTime? ShipDate
		{
			get
			{
				return this._ShipDate;
			}
			set
			{
				this._ShipDate = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		protected Int64? _CuryInfoID;
		[PXDBLong(BqlField = typeof(SOLine.curyInfoID))]
        [CurrencyInfo()]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region CuryUnitPrice
		public abstract class curyUnitPrice : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryUnitPrice;
		[PXDBDecimal(6, BqlField = typeof(SOLine.curyUnitPrice))]
		public virtual Decimal? CuryUnitPrice
		{
			get
			{
				return this._CuryUnitPrice;
			}
			set
			{
				this._CuryUnitPrice = value;
			}
		}
		#endregion
		#region DiscPct
		public abstract class discPct : PX.Data.IBqlField
		{
		}
		protected Decimal? _DiscPct;
		[PXDBDecimal(6, BqlField = typeof(SOLine.discPct))]
		public virtual Decimal? DiscPct
		{
			get
			{
				return this._DiscPct;
			}
			set
			{
				this._DiscPct = value;
			}
		}
		#endregion
		#region CuryUnbilledAmt
		public abstract class curyUnbilledAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryUnbilledAmt;
		[PXDBCurrency(typeof(SOLine2.curyInfoID), typeof(SOLine2.unbilledAmt), BqlField = typeof(SOLine.curyUnbilledAmt))]
		[PXFormula(typeof(Mult<Mult<SOLine2.unbilledQty, SOLine2.curyUnitPrice>, Sub<decimal1, Div<SOLine2.discPct, decimal100>>>))]
		public virtual Decimal? CuryUnbilledAmt
		{
			get
			{
				return this._CuryUnbilledAmt;
			}
			set
			{
				this._CuryUnbilledAmt = value;
			}
		}
		#endregion
		#region UnbilledAmt
		public abstract class unbilledAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _UnbilledAmt;
		[PXDBDecimal(4, BqlField = typeof(SOLine.unbilledAmt))]
		public virtual Decimal? UnbilledAmt
		{
			get
			{
				return this._UnbilledAmt;
			}
			set
			{
				this._UnbilledAmt = value;
			}
		}
		#endregion
		#region GroupDiscountRate
		public abstract class groupDiscountRate : PX.Data.IBqlField
		{
		}
		protected Decimal? _GroupDiscountRate;
		[PXDBDecimal(6, BqlField = typeof(SOLine.groupDiscountRate))]
		[PXDefault(TypeCode.Decimal, "1.0")]
		public virtual Decimal? GroupDiscountRate
		{
			get
			{
				return this._GroupDiscountRate;
			}
			set
			{
				this._GroupDiscountRate = value;
			}
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.IBqlField
		{
		}
		protected String _TaxCategoryID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(SOLine.taxCategoryID))]
		[SOUnbilledTax2(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran))]
		public virtual String TaxCategoryID
		{
			get
			{
				return this._TaxCategoryID;
			}
			set
			{
				this._TaxCategoryID = value;
			}
		}
		#endregion
		#region PlanType
		public abstract class planType : PX.Data.IBqlField
		{
		}
		protected String _PlanType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(SOOrderTypeOperation.orderPlanType))]
		public virtual String PlanType
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
		#region RequireLocation
		public abstract class requireLocation : PX.Data.IBqlField
		{
		}
		protected Boolean? _RequireLocation;
		[PXDBBool(BqlField = typeof(SOOrderType.requireLocation))]
		public virtual Boolean? RequireLocation
		{
			get
			{
				return this._RequireLocation;
			}
			set
			{
				this._RequireLocation = value;
			}
		}
		#endregion
		#region PlanID
		public abstract class planID : PX.Data.IBqlField
		{
		}
		protected Int64? _PlanID;
        [PXDBLong(BqlField = typeof(SOLine.planID), IsImmutable = true)]
		public virtual Int64? PlanID
		{
			get
			{
				return this._PlanID;
			}
			set
			{
				this._PlanID = value;
			}
		}
		#endregion
		#region SOOrderTypeOrderType
		public abstract class soOrderTypeOrderType : PX.Data.IBqlField
		{
		}
		protected string _SOOrderTypeOrderType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(SOOrderType.orderType))]
		[PXRestriction()]
		public virtual String SOOrderTypeOrderType
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		#endregion
		#region SOOrderTypeOperationOrderType
		public abstract class soOrderTypeOperationOrderType : PX.Data.IBqlField
		{
		}
		protected string _SOOrderTypeOperationOrderType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(SOOrderTypeOperation.orderType))]
		[PXRestriction()]
		public virtual String SOOrderTypeOperationOrderType
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		#endregion
	}

	[PXProjection(typeof(Select2<SOLineSplit,
		InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOLineSplit.orderType>>,
		InnerJoin<SOOrderTypeOperation,
					On<SOOrderTypeOperation.orderType, Equal<SOOrderType.orderType>,
				And<SOOrderTypeOperation.operation, Equal<SOOrderType.defaultOperation>>>>>>), Persistent = true)]
    [Serializable]
	public partial class SOLineSplit2 : IBqlTable
	{
		#region OrderType
		public abstract class orderType : PX.Data.IBqlField
		{
		}
		protected string _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(SOLineSplit.orderType))]
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.IBqlField
		{
		}
		protected string _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(SOLineSplit.orderNbr))]
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(SOLineSplit.lineNbr))]
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
		[PXDBInt(IsKey = true, BqlField = typeof(SOLineSplit.splitLineNbr))]
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
		#region Operation
		public abstract class operation : PX.Data.IBqlField
		{
		}
		protected String _Operation;
		[PXDBString(1, IsFixed = true, InputMask = ">a", BqlField = typeof(SOOrderTypeOperation.operation))]
		[PXUIField(DisplayName = "Operation")]
		[SOOperation.List]
		public virtual String Operation
		{
			get
			{
				return this._Operation;
			}
			set
			{
				this._Operation = value;
			}
		}
		#endregion
		#region Cancelled
		public abstract class cancelled : PX.Data.IBqlField
		{
		}
		protected Boolean? _Cancelled;
		[PXDBBool(BqlField = typeof(SOLineSplit.cancelled))]
		public virtual Boolean? Cancelled
		{
			get
			{
				return this._Cancelled;
			}
			set
			{
				this._Cancelled = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[PXDBInt(BqlField = typeof(SOLineSplit.inventoryID))]
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
		#region SiteID
		public abstract class siteID : PX.Data.IBqlField
		{
		}
		protected Int32? _SiteID;
		[PXDBInt(BqlField = typeof(SOLineSplit.siteID))]
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
		#region UOM
		public abstract class uOM : PX.Data.IBqlField
		{
		}
		protected String _UOM;
		[INUnit(typeof(SOLineSplit2.inventoryID), BqlField = typeof(SOLineSplit.uOM))]
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
		[PXDBDecimal(6, BqlField = typeof(SOLineSplit.qty))]
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
		#region ShippedQty
		public abstract class shippedQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _ShippedQty;
		[PXDBDecimal(6, BqlField = typeof(SOLineSplit.shippedQty))]
		public virtual Decimal? ShippedQty
		{
			get
			{
				return this._ShippedQty;
			}
			set
			{
				this._ShippedQty = value;
			}
		}
		#endregion
		#region BaseShippedQty
		public abstract class baseShippedQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _BaseShippedQty;
		[PXDBBaseQuantity(typeof(SOLineSplit2.uOM), typeof(SOLineSplit2.shippedQty), BqlField = typeof(SOLineSplit.baseShippedQty))]
		public virtual Decimal? BaseShippedQty
		{
			get
			{
				return this._BaseShippedQty;
			}
			set
			{
				this._BaseShippedQty = value;
			}
		}
		#endregion
		#region ShipDate
		public abstract class shipDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _ShipDate;
		[PXDBDate(BqlField = typeof(SOLineSplit.shipDate))]
		public virtual DateTime? ShipDate
		{
			get
			{
				return this._ShipDate;
			}
			set
			{
				this._ShipDate = value;
			}
		}
		#endregion
		#region PlanType
		public abstract class planType : PX.Data.IBqlField
		{
		}
		protected String _PlanType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(SOOrderTypeOperation.orderPlanType))]
		public virtual String PlanType
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
		#region RequireLocation
		public abstract class requireLocation : PX.Data.IBqlField
		{
		}
		protected Boolean? _RequireLocation;
		[PXDBBool(BqlField = typeof(SOOrderType.requireLocation))]
		public virtual Boolean? RequireLocation
		{
			get
			{
				return this._RequireLocation;
			}
			set
			{
				this._RequireLocation = value;
			}
		}
		#endregion

		#region SOOrderTypeOrderType
		public abstract class soOrderTypeOrderType : PX.Data.IBqlField
		{
		}
		protected string _SOOrderTypeOrderType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(SOOrderType.orderType))]
		[PXRestriction()]
		public virtual String SOOrderTypeOrderType
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		#endregion
		#region SOOrderTypeOperationOrderType
		public abstract class soOrderTypeOperationOrderType : PX.Data.IBqlField
		{
		}
		protected string _SOOrderTypeOperationOrderType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(SOOrderTypeOperation.orderType))]
		[PXRestriction()]
		public virtual String SOOrderTypeOperationOrderType
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		#endregion
	}

	[PXProjection(typeof(Select<SOLine, Where<SOLine.lineType, NotEqual<SOLineType.miscCharge>>>), Persistent = true)]
    [Serializable]
	public partial class SOLine4 : IBqlTable
	{
		#region OrderType
		public abstract class orderType : PX.Data.IBqlField
		{
		}
		protected string _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(SOLine.orderType))]
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.IBqlField
		{
		}
		protected string _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(SOLine.orderNbr))]
		[PXParent(typeof(Select<SOOrder, Where<SOOrder.orderType, Equal<Current<SOLine4.orderType>>, And<SOOrder.orderNbr, Equal<Current<SOLine4.orderNbr>>>>>))]
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(SOLine.lineNbr))]
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
		#region Operation
		public abstract class operation : PX.Data.IBqlField
		{
		}
		protected String _Operation;
		[PXDBString(1, IsFixed = true, InputMask = ">a", BqlField = typeof(SOLine.operation))]
		[PXUIField(DisplayName = "Operation")]
		[SOOperation.List]
		public virtual String Operation
		{
			get
			{
				return this._Operation;
			}
			set
			{
				this._Operation = value;
			}
		}
		#endregion
		#region ShipComplete
		public abstract class shipComplete : PX.Data.IBqlField
		{
		}
		protected String _ShipComplete;
		[PXDBString(1, IsFixed = true, BqlField = typeof(SOLine.shipComplete))]
		public virtual String ShipComplete
		{
			get
			{
				return this._ShipComplete;
			}
			set
			{
				this._ShipComplete = value;
			}
		}
		#endregion
		#region POType
		public abstract class pOType : PX.Data.IBqlField
		{
		}
		protected String _POType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(SOLine.pOType))]
		public virtual String POType
		{
			get
			{
				return this._POType;
			}
			set
			{
				this._POType = value;
			}
		}
		#endregion
		#region PONbr
		public abstract class pONbr : PX.Data.IBqlField
		{
		}
		protected String _PONbr;
		[PXDBString(15, IsUnicode = true, BqlField = typeof(SOLine.pONbr))]
		public virtual String PONbr
		{
			get
			{
				return this._PONbr;
			}
			set
			{
				this._PONbr = value;
			}
		}
		#endregion
		#region POLineNbr
		public abstract class pOLineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _POLineNbr;
		[PXDBInt(BqlField = typeof(SOLine.pOLineNbr))]
		public virtual Int32? POLineNbr
		{
			get
			{
				return this._POLineNbr;
			}
			set
			{
				this._POLineNbr = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[PXDBInt(BqlField = typeof(SOLine.inventoryID))]
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
		#region UOM
		public abstract class uOM : PX.Data.IBqlField
		{
		}
		protected String _UOM;
		[INUnit(typeof(SOLine4.inventoryID), BqlField = typeof(SOLine.uOM))]
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
		#region OrderQty
		public abstract class orderQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _OrderQty;
		[PXDBDecimal(6, BqlField = typeof(SOLine.orderQty))]
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
		#region BaseShippedQty
		public abstract class baseShippedQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _BaseShippedQty;
		[PXDBBaseQuantity(typeof(SOLine4.uOM), typeof(SOLine4.shippedQty), BqlField = typeof(SOLine.baseShippedQty))]
		public virtual Decimal? BaseShippedQty
		{
			get
			{
				return this._BaseShippedQty;
			}
			set
			{
				this._BaseShippedQty = value;
			}
		}
		#endregion
		#region ShippedQty
		public abstract class shippedQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _ShippedQty;
		[PXDBDecimal(6, BqlField = typeof(SOLine.shippedQty))]
		public virtual Decimal? ShippedQty
		{
			get
			{
				return this._ShippedQty;
			}
			set
			{
				this._ShippedQty = value;
			}
		}
		#endregion
		#region UnbilledQty
		public abstract class unbilledQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _UnbilledQty;
		[PXDBQuantity(BqlField = typeof(SOLine.unbilledQty))]
		[PXFormula(null, typeof(SumCalc<SOOrder.unbilledOrderQty>))]
		public virtual Decimal? UnbilledQty
		{
			get
			{
				return this._UnbilledQty;
			}
			set
			{
				this._UnbilledQty = value;
			}
		}
		#endregion
		#region OpenQty
		public abstract class openQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _OpenQty;
		[PXDBQuantity(typeof(SOLine4.uOM), typeof(SOLine4.baseOpenQty), BqlField = typeof(SOLine.openQty))]
		[PXFormula(typeof(Sub<SOLine4.orderQty, SOLine4.shippedQty>), typeof(SumCalc<SOOrder.openOrderQty>))]
		public virtual Decimal? OpenQty
		{
			get
			{
				return this._OpenQty;
			}
			set
			{
				this._OpenQty = value;
			}
		}
		#endregion
		#region BaseOpenQty
		public abstract class baseOpenQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _BaseOpenQty;
		[PXDBDecimal(6, MinValue = 0, BqlField = typeof(SOLine.baseOpenQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseOpenQty
		{
			get
			{
				return this._BaseOpenQty;
			}
			set
			{
				this._BaseOpenQty = value;
			}
		}
		#endregion
		#region CompleteQtyMin
		public abstract class completeQtyMin : PX.Data.IBqlField
		{
		}
		protected Decimal? _CompleteQtyMin;
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 99.0, BqlField = typeof(SOLine.completeQtyMin))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CompleteQtyMin
		{
			get
			{
				return this._CompleteQtyMin;
			}
			set
			{
				this._CompleteQtyMin = value;
			}
		}
		#endregion
		#region CompleteQtyMax
		public abstract class completeQtyMax : PX.Data.IBqlField
		{
		}
		protected Decimal? _CompleteQtyMax;
		[PXDBDecimal(2, MinValue = 100.0, MaxValue = 999.0, BqlField = typeof(SOLine.completeQtyMax))]
		[PXDefault(TypeCode.Decimal, "100.0")]
		public virtual Decimal? CompleteQtyMax
		{
			get
			{
				return this._CompleteQtyMax;
			}
			set
			{
				this._CompleteQtyMax = value;
			}
		}
		#endregion
		#region Cancelled
		public abstract class cancelled : PX.Data.IBqlField
		{
		}
		protected Boolean? _Cancelled;
		[PXDBBool(BqlField = typeof(SOLine.cancelled))]
		public virtual Boolean? Cancelled
		{
			get
			{
				return this._Cancelled;
			}
			set
			{
				this._Cancelled = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		protected Int64? _CuryInfoID;
		[PXDBLong(BqlField = typeof(SOLine.curyInfoID))]
        [CurrencyInfo()]
        public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region CuryUnitPrice
		public abstract class curyUnitPrice : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryUnitPrice;
		[PXDBDecimal(6, BqlField = typeof(SOLine.curyUnitPrice))]
		public virtual Decimal? CuryUnitPrice
		{
			get
			{
				return this._CuryUnitPrice;
			}
			set
			{
				this._CuryUnitPrice = value;
			}
		}
		#endregion
		#region DiscPct
		public abstract class discPct : PX.Data.IBqlField
		{
		}
		protected Decimal? _DiscPct;
		[PXDBDecimal(6, BqlField = typeof(SOLine.discPct))]
		public virtual Decimal? DiscPct
		{
			get
			{
				return this._DiscPct;
			}
			set
			{
				this._DiscPct = value;
			}
		}
		#endregion
		#region CuryOpenAmt
		public abstract class curyOpenAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryOpenAmt;
		[PXDBCurrency(typeof(SOLine4.curyInfoID), typeof(SOLine4.openAmt), BqlField = typeof(SOLine.curyOpenAmt))]
		[PXFormula(typeof(Mult<Mult<SOLine4.openQty, SOLine4.curyUnitPrice>, Sub<decimal1, Div<SOLine4.discPct, decimal100>>>))]
		public virtual Decimal? CuryOpenAmt
		{
			get
			{
				return this._CuryOpenAmt;
			}
			set
			{
				this._CuryOpenAmt = value;
			}
		}
		#endregion
		#region OpenAmt
		public abstract class openAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _OpenAmt;
		[PXDBDecimal(4, BqlField = typeof(SOLine.openAmt))]
		public virtual Decimal? OpenAmt
		{
			get
			{
				return this._OpenAmt;
			}
			set
			{
				this._OpenAmt = value;
			}
		}
		#endregion
		#region CuryUnbilledAmt
		public abstract class curyUnbilledAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryUnbilledAmt;
		[PXDBCurrency(typeof(SOLine4.curyInfoID), typeof(SOLine4.unbilledAmt), BqlField = typeof(SOLine.curyUnbilledAmt))]
		[PXFormula(typeof(Mult<Mult<SOLine4.unbilledQty, SOLine4.curyUnitPrice>, Sub<decimal1, Div<SOLine4.discPct, decimal100>>>))]
		public virtual Decimal? CuryUnbilledAmt
		{
			get
			{
				return this._CuryUnbilledAmt;
			}
			set
			{
				this._CuryUnbilledAmt = value;
			}
		}
		#endregion
		#region UnbilledAmt
		public abstract class unbilledAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _UnbilledAmt;
		[PXDBDecimal(4, BqlField = typeof(SOLine.unbilledAmt))]
		public virtual Decimal? UnbilledAmt
		{
			get
			{
				return this._UnbilledAmt;
			}
			set
			{
				this._UnbilledAmt = value;
			}
		}
		#endregion
		#region GroupDiscountRate
		public abstract class groupDiscountRate : PX.Data.IBqlField
		{
		}
		protected Decimal? _GroupDiscountRate;
		[PXDBDecimal(6, BqlField = typeof(SOLine.groupDiscountRate))]
		[PXDefault(TypeCode.Decimal, "1.0")]
		public virtual Decimal? GroupDiscountRate
		{
			get
			{
				return this._GroupDiscountRate;
			}
			set
			{
				this._GroupDiscountRate = value;
			}
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.IBqlField
		{
		}
		protected String _TaxCategoryID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(SOLine.taxCategoryID))]
		[SOOpenTax4(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran))]
		[SOUnbilledTax4(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran))]
		public virtual String TaxCategoryID
		{
			get
			{
				return this._TaxCategoryID;
			}
			set
			{
				this._TaxCategoryID = value;
			}
		}
		#endregion
		#region PlanID
		public abstract class planID : PX.Data.IBqlField
		{
		}
		protected Int64? _PlanID;
		[PXDBLong(BqlField = typeof(SOLine.planID))]
		public virtual Int64? PlanID
		{
			get
			{
				return this._PlanID;
			}
			set
			{
				this._PlanID = value;
			}
		}
		#endregion
	}

	[PXProjection(typeof(Select<SOLine, Where<SOLine.lineType, Equal<SOLineType.miscCharge>>>), Persistent = true)]
    [Serializable]
	public partial class SOMiscLine2 : IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.IBqlField
		{
		}
		protected Int32? _BranchID;
		[PXDBInt(BqlField = typeof(SOLine.branchID))]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.IBqlField
		{
		}
		protected string _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(SOLine.orderType))]
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.IBqlField
		{
		}
		protected string _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(SOLine.orderNbr))]
		[PXParent(typeof(Select<SOOrder, Where<SOOrder.orderType, Equal<Current<SOMiscLine2.orderType>>, And<SOOrder.orderNbr, Equal<Current<SOMiscLine2.orderNbr>>>>>))]
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(SOLine.lineNbr))]
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
		#region Operation
		public abstract class operation : PX.Data.IBqlField
		{
		}
		protected String _Operation;
		[PXDBString(1, IsFixed = true, InputMask = ">a", BqlField = typeof(SOLine.operation))]
		[PXUIField(DisplayName = "Operation")]
		[SOOperation.List]
		public virtual String Operation
		{
			get
			{
				return this._Operation;
			}
			set
			{
				this._Operation = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[NonStockItem(BqlField = typeof(SOLine.inventoryID))]
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
		#region ProjectID
		public abstract class projectID : PX.Data.IBqlField
		{
		}
		protected Int32? _ProjectID;
		[PXDBInt(BqlField=typeof(SOLine.projectID))]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		protected Int64? _CuryInfoID;
		[PXDBLong(BqlField = typeof(SOLine.curyInfoID))]
        [CurrencyInfo()]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.IBqlField
		{
		}
		protected String _UOM;
		[INUnit(typeof(SOMiscLine2.inventoryID), BqlField = typeof(SOLine.uOM))]
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
		#region BilledQty
		public abstract class billedQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _BilledQty;
		[PXDBDecimal(6, BqlField = typeof(SOLine.billedQty))]
		public virtual Decimal? BilledQty
		{
			get
			{
				return this._BilledQty;
			}
			set
			{
				this._BilledQty = value;
			}
		}
		#endregion
		#region BaseBilledQty
		public abstract class baseBilledQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _BaseBilledQty;
		[PXDBBaseQuantity(typeof(SOMiscLine2.uOM), typeof(SOMiscLine2.billedQty), BqlField = typeof(SOLine.baseBilledQty))]
		public virtual Decimal? BaseBilledQty
		{
			get
			{
				return this._BaseBilledQty;
			}
			set
			{
				this._BaseBilledQty = value;
			}
		}
		#endregion
		#region UnbilledQty
		public abstract class unbilledQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _UnbilledQty;
		[PXDBQuantity(BqlField = typeof(SOLine.unbilledQty))]
		[PXFormula(null, typeof(SumCalc<SOOrder.unbilledOrderQty>))]
		public virtual Decimal? UnbilledQty
		{
			get
			{
				return this._UnbilledQty;
			}
			set
			{
				this._UnbilledQty = value;
			}
		}
		#endregion
		#region BaseUnbilledQty
		public abstract class baseUnbilledQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _BaseUnbilledQty;
		[PXDBBaseQuantity(typeof(SOMiscLine2.uOM), typeof(SOMiscLine2.unbilledQty), BqlField = typeof(SOLine.baseUnbilledQty))]
		public virtual Decimal? BaseUnbilledQty
		{
			get
			{
				return this._BaseUnbilledQty;
			}
			set
			{
				this._BaseUnbilledQty = value;
			}
		}
		#endregion
		#region CuryUnitPrice
		public abstract class curyUnitPrice : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryUnitPrice;
		[PXDBDecimal(6, BqlField = typeof(SOLine.curyUnitPrice))]
		public virtual Decimal? CuryUnitPrice
		{
			get
			{
				return this._CuryUnitPrice;
			}
			set
			{
				this._CuryUnitPrice = value;
			}
		}
		#endregion
		#region CuryLineAmt
		public abstract class curyLineAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryLineAmt;
		[PXDBCurrency(typeof(SOMiscLine2.curyInfoID), typeof(SOMiscLine2.lineAmt), BqlField = typeof(SOLine.curyLineAmt))]
		[PXUIField(DisplayName = "Ext. Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryLineAmt
		{
			get
			{
				return this._CuryLineAmt;
			}
			set
			{
				this._CuryLineAmt = value;
			}
		}
		#endregion
		#region LineAmt
		public abstract class lineAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _LineAmt;
		[PXDBDecimal(4, BqlField = typeof(SOLine.lineAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LineAmt
		{
			get
			{
				return this._LineAmt;
			}
			set
			{
				this._LineAmt = value;
			}
		}
		#endregion
		#region CuryBilledAmt
		public abstract class curyBilledAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryBilledAmt;
		[PXDBCurrency(typeof(SOMiscLine2.curyInfoID), typeof(SOMiscLine2.billedAmt), BqlField = typeof(SOLine.curyBilledAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryBilledAmt
		{
			get
			{
				return this._CuryBilledAmt;
			}
			set
			{
				this._CuryBilledAmt = value;
			}
		}
		#endregion
		#region BilledAmt
		public abstract class billedAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _BilledAmt;
		[PXDBDecimal(4, BqlField = typeof(SOLine.billedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BilledAmt
		{
			get
			{
				return this._BilledAmt;
			}
			set
			{
				this._BilledAmt = value;
			}
		}
		#endregion
		#region CuryUnbilledAmt
		public abstract class curyUnbilledAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryUnbilledAmt;
		[PXDBCurrency(typeof(SOMiscLine2.curyInfoID), typeof(SOMiscLine2.unbilledAmt), BqlField = typeof(SOLine.curyUnbilledAmt))]
		public virtual Decimal? CuryUnbilledAmt
		{
			get
			{
				return this._CuryUnbilledAmt;
			}
			set
			{
				this._CuryUnbilledAmt = value;
			}
		}
		#endregion
		#region UnbilledAmt
		public abstract class unbilledAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _UnbilledAmt;
		[PXDBDecimal(4, BqlField = typeof(SOLine.unbilledAmt))]
		public virtual Decimal? UnbilledAmt
		{
			get
			{
				return this._UnbilledAmt;
			}
			set
			{
				this._UnbilledAmt = value;
			}
		}
		#endregion
        #region CuryDiscAmt
        public abstract class curyDiscAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryDiscAmt;
        [PXDBCurrency(typeof(SOMiscLine2.curyInfoID), typeof(SOMiscLine2.discAmt), BqlField = typeof(SOLine.curyDiscAmt))]
        [PXUIField(DisplayName = "Ext. Amount")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryDiscAmt
        {
            get
            {
                return this._CuryDiscAmt;
            }
            set
            {
                this._CuryDiscAmt = value;
            }
        }
        #endregion
        #region DiscAmt
        public abstract class discAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _DiscAmt;
        [PXDBDecimal(4, BqlField = typeof(SOLine.discAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? DiscAmt
        {
            get
            {
                return this._DiscAmt;
            }
            set
            {
                this._DiscAmt = value;
            }
        }
        #endregion
        #region DiscPct
        public abstract class discPct : PX.Data.IBqlField
        {
        }
        protected Decimal? _DiscPct;
        [PXDBDecimal(4, BqlField = typeof(SOLine.discPct))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? DiscPct
        {
            get
            {
                return this._DiscPct;
            }
            set
            {
                this._DiscPct = value;
            }
        }
        #endregion
		#region GroupDiscountRate
		public abstract class groupDiscountRate : PX.Data.IBqlField
		{
		}
		protected Decimal? _GroupDiscountRate;
		[PXDBDecimal(6, BqlField = typeof(SOLine.groupDiscountRate))]
		[PXDefault(TypeCode.Decimal, "1.0")]
		public virtual Decimal? GroupDiscountRate
		{
			get
			{
				return this._GroupDiscountRate;
			}
			set
			{
				this._GroupDiscountRate = value;
			}
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.IBqlField
		{
		}
		protected String _TaxCategoryID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(SOLine.taxCategoryID))]
		[SOUnbilledMiscTax2(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran))]
		public virtual String TaxCategoryID
		{
			get
			{
				return this._TaxCategoryID;
			}
			set
			{
				this._TaxCategoryID = value;
			}
		}
		#endregion
		#region SalesPersonID
		public abstract class salesPersonID : PX.Data.IBqlField
		{
		}
		protected Int32? _SalesPersonID;
		[SalesPerson(BqlField = typeof(SOLine.salesPersonID))]
		public virtual Int32? SalesPersonID
		{
			get
			{
				return this._SalesPersonID;
			}
			set
			{
				this._SalesPersonID = value;
			}
		}
		#endregion
		#region SalesAcctID
		public abstract class salesAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _SalesAcctID;
		[Account(Visible = false, BqlField = typeof(SOLine.salesAcctID))]
		public virtual Int32? SalesAcctID
		{
			get
			{
				return this._SalesAcctID;
			}
			set
			{
				this._SalesAcctID = value;
			}
		}
		#endregion
		#region SalesSubID
		public abstract class salesSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _SalesSubID;
		[SubAccount(typeof(SOMiscLine2.salesAcctID), Visible = false, BqlField = typeof(SOLine.salesSubID))]
		public virtual Int32? SalesSubID
		{
			get
			{
				return this._SalesSubID;
			}
			set
			{
				this._SalesSubID = value;
			}
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.IBqlField
		{
		}
		protected Int32? _TaskID;
		[PX.Objects.PM.ActiveProjectTask(typeof(SOLine.projectID), BatchModule.SO, BqlField=typeof(SOLine.taskID), DisplayName = "Project Task")]
		public virtual Int32? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.IBqlField
		{
		}
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true, BqlField = typeof(SOLine.tranDesc))]
		[PXUIField(DisplayName = "Line Description")]
		public virtual String TranDesc
		{
			get
			{
				return this._TranDesc;
			}
			set
			{
				this._TranDesc = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.IBqlField
		{
		}
		protected Int64? _NoteID;
		[PXNote(BqlField = typeof(SOLine.noteID))]
		public virtual Int64? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
        #region Commissionable
        public abstract class commissionable : IBqlField
        {
        }
        protected bool? _Commissionable;
        [PXDBBool(BqlField = typeof(SOLine.commissionable))]
        public bool? Commissionable
        {
            get
            {
                return _Commissionable;
            }
            set
            {
                _Commissionable = value;
            }
        }
        #endregion
        #region IsFree
		public abstract class isFree : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsFree;
		[PXDBBool(BqlField=typeof(SOLine.isFree))]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Free Item")]
		public virtual Boolean? IsFree
		{
			get
			{
				return this._IsFree;
			}
			set
			{
				this._IsFree = value;
			}
		}
		#endregion
		#region ManualDisc
		public abstract class manualDisc : PX.Data.IBqlField
		{
		}
		protected Boolean? _ManualDisc;
		[PXDBBool(BqlField = typeof(SOLine.manualDisc))]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Manual Discount", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? ManualDisc
		{
			get
			{
				return this._ManualDisc;
			}
			set
			{
				this._ManualDisc = value;
			}
		}
		#endregion

        #region DiscountID
        public abstract class discountID : PX.Data.IBqlField
        {
        }
        protected String _DiscountID;
        [PXDBString(10, IsUnicode = true)]
        [PXSelector(typeof(Search<ARDiscount.discountID, Where<ARDiscount.type, Equal<DiscountType.LineDiscount>>>))]
        [PXUIField(DisplayName = "Discount Code", Visible = true, Enabled = false)]
        public virtual String DiscountID
        {
            get
            {
                return this._DiscountID;
            }
            set
            {
                this._DiscountID = value;
            }
        }
        #endregion
        #region DiscountSequenceID
        public abstract class discountSequenceID : PX.Data.IBqlField
        {
        }
        protected String _DiscountSequenceID;
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Discount Sequence", Visible = true)]
        public virtual String DiscountSequenceID
        {
            get
            {
                return this._DiscountSequenceID;
            }
            set
            {
                this._DiscountSequenceID = value;
            }
        }
        #endregion

		#region DetDiscIDC1
		public abstract class detDiscIDC1 : PX.Data.IBqlField
		{
		}
		protected String _DetDiscIDC1;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(SOLine.detDiscIDC1))]
		public virtual String DetDiscIDC1
		{
			get
			{
				return this._DetDiscIDC1;
			}
			set
			{
				this._DetDiscIDC1 = value;
			}
		}
		#endregion
		#region DetDiscSeqIDC1
		public abstract class detDiscSeqIDC1 : PX.Data.IBqlField
		{
		}
		protected String _DetDiscSeqIDC1;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(SOLine.detDiscSeqIDC1))]
		public virtual String DetDiscSeqIDC1
		{
			get
			{
				return this._DetDiscSeqIDC1;
			}
			set
			{
				this._DetDiscSeqIDC1 = value;
			}
		}
		#endregion
		#region DetDiscIDC2
		public abstract class detDiscIDC2 : PX.Data.IBqlField
		{
		}
		protected String _DetDiscIDC2;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(SOLine.detDiscIDC2))]
		public virtual String DetDiscIDC2
		{
			get
			{
				return this._DetDiscIDC2;
			}
			set
			{
				this._DetDiscIDC2 = value;
			}
		}
		#endregion
		#region DetDiscSeqIDC2
		public abstract class detDiscSeqIDC2 : PX.Data.IBqlField
		{
		}
		protected String _DetDiscSeqIDC2;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(SOLine.detDiscSeqIDC2))]
		public virtual String DetDiscSeqIDC2
		{
			get
			{
				return this._DetDiscSeqIDC2;
			}
			set
			{
				this._DetDiscSeqIDC2 = value;
			}
		}
		#endregion
		#region DetDiscApp
		public abstract class detDiscApp : PX.Data.IBqlField
		{
		}
		protected Boolean? _DetDiscApp;
		[PXDBBool(BqlField = typeof(SOLine.detDiscApp))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? DetDiscApp
		{
			get
			{
				return this._DetDiscApp;
			}
			set
			{
				this._DetDiscApp = value;
			}
		}
		#endregion
		
		#region DocDiscIDC1
		public abstract class docDiscIDC1 : PX.Data.IBqlField
		{
		}
		protected String _DocDiscIDC1;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(SOLine.docDiscIDC1))]
		public virtual String DocDiscIDC1
		{
			get
			{
				return this._DocDiscIDC1;
			}
			set
			{
				this._DocDiscIDC1 = value;
			}
		}
		#endregion
		#region DocDiscSeqIDC1
		public abstract class docDiscSeqIDC1 : PX.Data.IBqlField
		{
		}
		protected String _DocDiscSeqIDC1;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(SOLine.docDiscSeqIDC1))]
		public virtual String DocDiscSeqIDC1
		{
			get
			{
				return this._DocDiscSeqIDC1;
			}
			set
			{
				this._DocDiscSeqIDC1 = value;
			}
		}
		#endregion
		#region DocDiscIDC2
		public abstract class docDiscIDC2 : PX.Data.IBqlField
		{
		}
		protected String _DocDiscIDC2;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(SOLine.docDiscIDC2))]
		public virtual String DocDiscIDC2
		{
			get
			{
				return this._DocDiscIDC2;
			}
			set
			{
				this._DocDiscIDC2 = value;
			}
		}
		#endregion
		#region DocDiscSeqIDC2
		public abstract class docDiscSeqIDC2 : PX.Data.IBqlField
		{
		}
		protected String _DocDiscSeqIDC2;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(SOLine.docDiscSeqIDC2))]
		public virtual String DocDiscSeqIDC2
		{
			get
			{
				return this._DocDiscSeqIDC2;
			}
			set
			{
				this._DocDiscSeqIDC2 = value;
			}
		}
		#endregion
	}

	[Serializable()]
	public partial class AddSOFilter : IBqlTable
	{
		#region Operation
		public abstract class operation : PX.Data.IBqlField
		{
		}
		protected String _Operation;
		[PXDBString(1, IsFixed = true, InputMask = ">a")]
		[PXUIField(DisplayName = "Operation")]
		[PXDefault(SOOperation.Issue, typeof(SOShipment.operation))]
		[SOOperation.List]
		public virtual String Operation
		{
			get
			{
				return this._Operation;
			}
			set
			{
				this._Operation = value;
			}
		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.IBqlField
		{
		}
		protected String _OrderType;
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXSelector(typeof(Search2<SOOrderType.orderType,
			InnerJoin<SOOrderTypeOperation, On<SOOrderTypeOperation.orderType, Equal<SOOrderType.orderType>>>,
			Where<SOOrderType.active, Equal<True>, And<SOOrderType.requireShipping, Equal<True>, And<SOOrderTypeOperation.active, Equal<True>, 
				And<SOOrderTypeOperation.operation, Equal<Current<AddSOFilter.operation>>,
				And<Where<SOOrderTypeOperation.iNDocType, Equal<INTranType.transfer>, And<Current<SOShipment.shipmentType>, Equal<SOShipmentType.transfer>,
				Or<SOOrderTypeOperation.iNDocType, NotEqual<INTranType.transfer>, And<Current<SOShipment.shipmentType>, Equal<SOShipmentType.issue>>>>>>>>>>>))]
		[PXDefault(typeof(Search2<SOOrderType.orderType,
			InnerJoin<SOOrderTypeOperation, On<SOOrderTypeOperation.orderType, Equal<SOOrderType.orderType>>>,
			Where<SOOrderType.active, Equal<True>, And<SOOrderType.requireShipping, Equal<True>, And<SOOrderTypeOperation.active, Equal<True>, 
				And<SOOrderTypeOperation.operation, Equal<Current<AddSOFilter.operation>>,
				And<Where<SOOrderTypeOperation.iNDocType, Equal<INTranType.transfer>, And<Current<SOShipment.shipmentType>, Equal<SOShipmentType.transfer>,
				Or<SOOrderTypeOperation.iNDocType, NotEqual<INTranType.transfer>, And<Current<SOShipment.shipmentType>, Equal<SOShipmentType.issue>>>>>>>>>>>))]
		[PXUIField(DisplayName = "Order Type")]
		[PXFormula(typeof(Default<AddSOFilter.operation>))]
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.IBqlField
		{
		}
		protected String _OrderNbr;
		[PXDBString(15, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Order Nbr.")]
		[PXDefault]
		[SO.RefNbr(typeof(Search<SOOrder.orderNbr,
			Where<SOOrder.orderType, Equal<Current<AddSOFilter.orderType>>,
			And<SOOrder.customerID, Equal<Current<SOShipment.customerID>>,
			And<SOOrder.cancelled, Equal<boolFalse>,
			And<SOOrder.completed, Equal<boolFalse>,
			And<SOOrder.hold, Equal<False>,
			And<SOOrder.creditHold, Equal<False>>>>>>>>), Filterable = true)]
		[PXFormula(typeof(Default<AddSOFilter.orderType>))]
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
	}
}
