using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Runtime.Serialization;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.TX;
using PX.Objects.IN;
using PX.Objects.BQLConstants;
using PX.Objects.EP;
using PX.TM;
using SOInvoice = PX.Objects.SO.SOInvoice;
using SOInvoiceEntry = PX.Objects.SO.SOInvoiceEntry;
using PX.Objects.SO;
using PX.Objects.DR;
using Avalara.AvaTax.Adapter;
using Avalara.AvaTax.Adapter.TaxService;
using AvaAddress = Avalara.AvaTax.Adapter.AddressService;
using AvaMessage = Avalara.AvaTax.Adapter.Message;
using System.Diagnostics;


namespace PX.Objects.AR 
{
    [Serializable]
	public class ARInvoiceEntry : ARDataEntryGraph<ARInvoiceEntry, ARInvoice>, PXImportAttribute.IPXPrepareItems
	{
		#region Cache Attached
        #region InventoryItem
        #region COGSSubID
        [PXDefault(typeof(Search<INPostClass.cOGSSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>))]
        [SubAccount(typeof(InventoryItem.cOGSAcctID), DisplayName = "Expense Sub.", DescriptionField = typeof(Sub.description))]
        public virtual void InventoryItem_COGSSubID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion
		#region ARSalesPerTran
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(ARInvoice.docType))]
		protected virtual void ARSalesPerTran_DocType_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(ARInvoice.refNbr))]
		[PXParent(typeof(Select<ARInvoice, Where<ARInvoice.docType, Equal<Current<ARSalesPerTran.docType>>,
						 And<ARInvoice.refNbr, Equal<Current<ARSalesPerTran.refNbr>>>>>))]
		protected virtual void ARSalesPerTran_RefNbr_CacheAttached(PXCache sender)
		{
		}

        [PXDBInt()]
        [PXDBDefault(typeof(ARInvoice.branchID),DefaultForInsert = true, DefaultForUpdate = true)]
        protected virtual void ARSalesPerTran_BranchID_CacheAttached(PXCache sender)
        {
        }

		[SalesPerson(DirtyRead = true, Enabled = false, IsKey = true, DescriptionField = typeof(Contact.displayName))]
		protected virtual void ARSalesPerTran_SalespersonID_CacheAttached(PXCache sender)
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault(0)]
		protected virtual void ARSalesPerTran_AdjNbr_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(3, IsFixed = true, IsKey = true)]
		[PXDefault(ARDocType.Undefined)]
		protected virtual void ARSalesPerTran_AdjdDocType_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault("")]
		protected virtual void ARSalesPerTran_AdjdRefNbr_CacheAttached(PXCache sender)
		{
		}
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Coalesce<Search<CustSalesPeople.commisionPct, Where<CustSalesPeople.bAccountID, Equal<Current<ARInvoice.customerID>>,
				And<CustSalesPeople.locationID, Equal<Current<ARInvoice.customerLocationID>>,
				And<CustSalesPeople.salesPersonID, Equal<Current<ARSalesPerTran.salespersonID>>>>>>,
			Search<SalesPerson.commnPct, Where<SalesPerson.salesPersonID, Equal<Current<ARSalesPerTran.salespersonID>>>>>))]
		[PXUIField(DisplayName = "Commission %")]
		protected virtual void ARSalesPerTran_CommnPct_CacheAttached(PXCache sender)
		{
		}
		[PXDBCurrency(typeof(ARSalesPerTran.curyInfoID), typeof(ARSalesPerTran.commnblAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Commissionable Amount", Enabled = false)]
		[PXFormula(null, typeof(SumCalc<ARInvoice.curyCommnblAmt>))]
		protected virtual void ARSalesPerTran_CuryCommnblAmt_CacheAttached(PXCache sender)
		{
		}
		[PXDBCurrency(typeof(ARSalesPerTran.curyInfoID), typeof(ARSalesPerTran.commnAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(typeof(Mult<ARSalesPerTran.curyCommnblAmt, Div<ARSalesPerTran.commnPct, decimal100>>), typeof(SumCalc<ARInvoice.curyCommnAmt>))]
		[PXUIField(DisplayName = "Commission Amt.", Enabled = false)]
		protected virtual void ARSalesPerTran_CuryCommnAmt_CacheAttached(PXCache sender)
		{
		}        
		#endregion
		#endregion

		public ToggleCurrency<ARInvoice> CurrencyView;

        public PXAction<ARInvoice> viewSchedule;
		[PXUIField(DisplayName = "View Schedule", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewSchedule(PXAdapter adapter)
		{
			PXSelectBase<DRSchedule> select = new PXSelect<DRSchedule,
				Where<DRSchedule.module, Equal<BatchModule.moduleAR>,
				And<DRSchedule.docType, Equal<Current<ARTran.tranType>>,
				And<DRSchedule.refNbr, Equal<Current<ARTran.refNbr>>,
				And<DRSchedule.lineNbr, Equal<Current<ARTran.lineNbr>>>>>>>(this);

			DRSchedule sc = select.Select();

			if (sc == null)
			{
				DRDeferredCode defCode = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Current<ARTran.deferredCode>>>>.Select(this);
				if (defCode != null)
				{
					DRProcess process = PXGraph.CreateInstance<DRProcess>();
					process.CreateSchedule(Transactions.Current, defCode, Document.Current, true);
					process.Actions.PressSave();

					select.View.Clear();
					sc = select.Select();
				}
			}
			else if (sc.IsDraft == true)
			{
				DRDeferredCode defCode = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Current<ARTran.deferredCode>>>>.Select(this);
				if (defCode != null)
				{
					DRProcess process = PXGraph.CreateInstance<DRProcess>();
					process.ReavaluateDraftSchedule(sc, Transactions.Current, defCode, Document.Current);
					process.Actions.PressSave();

					select.View.Clear();
					sc = select.Select();
				}
			}

			if (sc != null)
			{
				this.Save.Press();

				DraftScheduleMaint target = PXGraph.CreateInstance<DraftScheduleMaint>();
				target.Clear();
				target.Schedule.Current = sc;
				throw new PXPopupRedirectException(target, "ViewSchedule", true);
			}
			
			return adapter.Get();
		}


		public PXAction<ARInvoice> newCustomer;
		[PXUIField(DisplayName = "New Customer", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable NewCustomer(PXAdapter adapter)
		{
			CustomerMaint graph = PXGraph.CreateInstance<CustomerMaint>();
			throw new PXRedirectRequiredException(graph, "New Customer");
		}

		public PXAction<ARInvoice> editCustomer;
		[PXUIField(DisplayName = "Edit Customer", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable EditCustomer(PXAdapter adapter)
		{
			if (customer.Current != null)
			{
				CustomerMaint graph = PXGraph.CreateInstance<CustomerMaint>();
				graph.BAccount.Current = customer.Current;
				throw new PXRedirectRequiredException(graph, "Edit Customer");
			}
			return adapter.Get();
		}

		public PXAction<ARInvoice> customerDocuments;
		[PXUIField(DisplayName = "Customer Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable CustomerDocuments(PXAdapter adapter)
		{
			if (customer.Current != null)
			{
				ARDocumentEnq graph = PXGraph.CreateInstance<ARDocumentEnq>();
				graph.Filter.Current.CustomerID = customer.Current.BAccountID;
				graph.Filter.Select();
				throw new PXRedirectRequiredException(graph, "Customer Details");
			}
			return adapter.Get();
		}

        public PXAction<ARInvoice> sOInvoice;
        [PXUIField(DisplayName = "SO Invoice", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable SOInvoice(PXAdapter adapter)
        {
            ARInvoice invoice = Document.Current;
            SOInvoiceEntry graph = CreateInstance<SOInvoiceEntry>();
            graph.Document.Current = graph.Document.Search<ARInvoice.refNbr>(invoice.RefNbr, invoice.DocType);
            throw new PXRedirectRequiredException(graph, "SO Invoice");
        }
        
        public PXAction<ARInvoice> sendARInvoiceMemo;
		[PXUIField(DisplayName = "Send AR Invoice/Memo", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable SendARInvoiceMemo(PXAdapter adapter,
					[PXString]
					string reportID
			)
		{
			ARInvoice invoice = Document.Current;
			if (reportID == null) reportID = "AR641000";
			if (invoice != null)
			{
				Dictionary<string, string> mailParams = new Dictionary<string, string>();
				mailParams["DocType"] = invoice.DocType;
				mailParams["RefNbr"] = invoice.RefNbr;
				if (!ReportNotificationGenerator.Send(reportID, mailParams).Any())
				{
					throw new PXException(ErrorMessages.MailSendFailed);
				}
				Clear();
				Document.Current = Document.Search<ARInvoice.refNbr>(invoice.RefNbr, invoice.DocType);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = "Release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public override IEnumerable Release(PXAdapter adapter)
		{
			PXCache cache = Document.Cache;
			List<ARRegister> list = new List<ARRegister>();
			foreach (ARInvoice apdoc in adapter.Get<ARInvoice>())
			{
				if (!(bool)apdoc.Hold && !(bool)apdoc.Released)
				{
					cache.Update(apdoc);
					list.Add(apdoc);
				}
			}
			if (list.Count == 0)
			{
				throw new PXException(Messages.Document_Status_Invalid);
			}

			if (!IsExternalTax)
			{
				Save.Press();
				PXLongOperation.StartOperation(this, delegate() { ARDocumentRelease.ReleaseDoc(list, false); });
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
				
				PXLongOperation.StartOperation(this, delegate()
				                                     	{
															List<ARRegister> listWithTax = new List<ARRegister>();
															foreach (ARInvoice ardoc in list)
															{
																if (ardoc.IsTaxValid != true)
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

															ARDocumentRelease.ReleaseDoc(listWithTax, false);
				                                     	});
			}

			return list;
		}

		public PXAction<ARInvoice> writeOff;
		[PXUIField(DisplayName = "Write-Off", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable WriteOff(PXAdapter adapter)
		{
			if (Document.Current != null && (Document.Current.DocType == ARDocType.Invoice || Document.Current.DocType == ARDocType.DebitMemo || Document.Current.DocType == ARDocType.CreditMemo
				 || Document.Current.DocType == ARDocType.FinCharge || Document.Current.DocType == ARDocType.SmallCreditWO))
			{
				this.Save.Press();

				Customer c = customer.Select(Document.Current.CustomerID);
				if (c != null)
				{
					if (c.SmallBalanceAllow != true)
					{
						throw new PXException(Messages.WriteOffIsDisabled);
					}
					else if (c.SmallBalanceLimit < Document.Current.CuryDocBal)
					{
						throw new PXException(Messages.WriteOffIsOutOfLimit, c.SmallBalanceLimit);
					}
				}

				ARCreateWriteOff target = PXGraph.CreateInstance<ARCreateWriteOff>();
				if (Document.Current.DocType == ARDocType.CreditMemo)
					target.Filter.Cache.SetValueExt<ARWriteOffFilter.woType>(target.Filter.Current, ARWriteOffType.SmallCreditWO);
				target.Filter.Cache.SetValueExt<ARWriteOffFilter.branchID>(target.Filter.Current, Document.Current.BranchID);
				target.Filter.Cache.SetValueExt<ARWriteOffFilter.customerID>(target.Filter.Current, Document.Current.CustomerID);

				foreach (PX.Objects.AR.ARCreateWriteOff.ARRegisterEx doc in target.ARDocumentList.Select())
				{
					if (doc.DocType == Document.Current.DocType && doc.RefNbr == Document.Current.RefNbr)
					{
						doc.Selected = true;
						target.ARDocumentList.Update(doc);
					}
				}

				throw new PXRedirectRequiredException(target, "Create Write-Off");
			}

			return adapter.Get();
		}

		public PXAction<ARInvoice> reverseInvoice;
		[PXUIField(DisplayName = "Reverse Invoice", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ReverseInvoice(PXAdapter adapter)
		{
			if (Document.Current != null && (Document.Current.DocType == ARDocType.Invoice || Document.Current.DocType == ARDocType.DebitMemo || Document.Current.DocType == ARDocType.CreditMemo))
			{
				if (Document.Current.InstallmentNbr != null && string.IsNullOrEmpty(Document.Current.MasterRefNbr) == false)
				{
					throw new PXSetPropertyException(Messages.Multiply_Installments_Cannot_be_Reversed, Document.Current.MasterRefNbr);
				}

				this.Save.Press();
				ARInvoice doc = PXCache<ARInvoice>.CreateCopy(Document.Current);

				if (finperiod.Current != null && (finperiod.Current.Active == false || finperiod.Current.ARClosed == true && glsetup.Current.PostClosedPeriods == false))
				{
					FinPeriod firstopen = PXSelect<FinPeriod, Where<FinPeriod.aRClosed, Equal<boolFalse>, And<FinPeriod.active, Equal<boolTrue>, And<FinPeriod.finPeriodID, Greater<Required<FinPeriod.finPeriodID>>>>>, OrderBy<Asc<FinPeriod.finPeriodID>>>.Select(this, doc.FinPeriodID);
					if (firstopen == null)
					{
						Document.Cache.RaiseExceptionHandling<ARInvoice.finPeriodID>(Document.Current, Document.Current.FinPeriodID, new PXSetPropertyException(GL.Messages.NoOpenPeriod));
						return adapter.Get();
					} 
					doc.FinPeriodID = firstopen.FinPeriodID;
				}

				try
				{
					this.ReverseInvoiceProc(doc);

					Document.Cache.RaiseExceptionHandling<ARInvoice.finPeriodID>(Document.Current, Document.Current.FinPeriodID, null);

					return new List<ARInvoice> { Document.Current };
				}
				catch (PXException)
				{
					this.Clear(PXClearOption.PreserveTimeStamp);
					Document.Current = doc; 
					throw;
				}
			}

			return adapter.Get();
		}

		public PXAction<ARInvoice> payInvoice;
		[PXUIField(DisplayName = "Enter Payment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable PayInvoice(PXAdapter adapter)
		{
			if (Document.Current != null && Document.Current.Released == true)
			{
				ARPaymentEntry pe = PXGraph.CreateInstance<ARPaymentEntry>();

				if (Document.Current.Payable == true && Document.Current.OpenDoc == true)
				{
					ARAdjust adj = PXSelect<ARAdjust, Where<ARAdjust.adjdDocType, Equal<Current<ARInvoice.docType>>,
						And<ARAdjust.adjdRefNbr, Equal<Current<ARInvoice.refNbr>>, And<ARAdjust.released, Equal<boolFalse>, And<ARAdjust.voided, Equal<boolFalse>>>>>>.Select(this);
					if (adj != null)
					{
						pe.Document.Current = pe.Document.Search<ARPayment.refNbr>(adj.AdjgRefNbr, adj.AdjgDocType);
					}
					else
					{
						pe.CreatePayment(Document.Current);
					}
					throw new PXRedirectRequiredException(pe, "PayInvoice");
				}
				else if (Document.Current.DocType == ARDocType.CreditMemo)
				{
					pe.Document.Current = pe.Document.Search<ARPayment.refNbr>(Document.Current.RefNbr, Document.Current.DocType);
					throw new PXRedirectRequiredException(pe, "PayInvoice");
				}
			}
			return adapter.Get();
		}

		public PXAction<ARInvoice> createSchedule;
		[PXUIField(DisplayName = "Assign to Schedule", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton(ImageKey = PX.Web.UI.Sprite.Main.Shedule)]
		public virtual IEnumerable CreateSchedule(PXAdapter adapter)
		{
			this.Save.Press();
			if (Document.Current != null && (bool)Document.Current.Released == false && (bool)Document.Current.Hold == false)
			{
				PX.Objects.AR.ARScheduleMaint sm = PXGraph.CreateInstance<PX.Objects.AR.ARScheduleMaint>();
				if ((bool)Document.Current.Scheduled && Document.Current.ScheduleID != null)
				{
					sm.Schedule_Header.Current = sm.Schedule_Header.Search<Schedule.scheduleID>(Document.Current.ScheduleID);
				}
				else
				{
					sm.Schedule_Header.Cache.Insert();
					ARRegister doc = (ARRegister)sm.Document_Detail.Cache.CreateInstance();
					PXCache<ARRegister>.RestoreCopy(doc, Document.Current);
					doc = (ARRegister)sm.Document_Detail.Cache.Update(doc);
				}
				throw new PXRedirectRequiredException(sm, "create Schedule");
			}
			return adapter.Get();
		}

		public PXAction<ARInvoice> autoApply;
		[PXUIField(DisplayName = "Auto Apply", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable AutoApply(PXAdapter adapter)
		{
			if (Document.Current != null && Document.Current.DocType == ARDocType.Invoice && Document.Current.Released == false)
			{
				foreach (PXResult<ARAdjust, ARPayment, CurrencyInfo> res in Adjustments_Raw.Select())
				{
					ARAdjust adj = (ARAdjust)res;

					adj.CuryAdjdAmt = 0m;
				}

				decimal? CuryApplAmt = Document.Current.CuryDocBal;

				foreach (PXResult<ARAdjust, ARPayment> res in Adjustments.Select())
				{
					ARAdjust adj = (ARAdjust)res;

					if (adj.CuryDocBal > 0m)
					{
						if (adj.CuryDocBal > CuryApplAmt)
						{
							adj.CuryAdjdAmt = CuryApplAmt;
							CuryApplAmt = 0m;
							Adjustments.Cache.RaiseFieldUpdated<ARAdjust.curyAdjdAmt>(adj, 0m);
							if (Adjustments.Cache.GetStatus(adj) == PXEntryStatus.Notchanged)
							{
								Adjustments.Cache.SetStatus(adj, PXEntryStatus.Updated);
							}
							break;
						}
						else
						{
							adj.CuryAdjdAmt = adj.CuryDocBal;
							CuryApplAmt -= adj.CuryDocBal;
							Adjustments.Cache.RaiseFieldUpdated<ARAdjust.curyAdjdAmt>(adj, 0m);
							if (Adjustments.Cache.GetStatus(adj) == PXEntryStatus.Notchanged)
							{
								Adjustments.Cache.SetStatus(adj, PXEntryStatus.Updated);
							}
						}
					}
				}
				Adjustments.View.RequestRefresh();
			}
			return adapter.Get();
		}

		public PXAction<ARInvoice> viewPayment;
		[PXUIField(DisplayName = "View Payment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewPayment(PXAdapter adapter)
		{
			if (Document.Current != null && Adjustments.Current != null)
			{
				ARPaymentEntry pe = PXGraph.CreateInstance<ARPaymentEntry>();
				pe.Document.Current = pe.Document.Search<ARPayment.refNbr>(Adjustments.Current.AdjgRefNbr, Adjustments.Current.AdjgDocType);

				throw new PXRedirectRequiredException(pe, true, "Payment"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
			}
			return adapter.Get();
		}

	    public PXAction<ARInvoice> viewItem;
        [PXUIField(DisplayName = "View Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable ViewItem(PXAdapter adapter)
		{
			if (Transactions.Current != null)
			{
			    InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID,
			        Equal<Current<ARTran.inventoryID>>>>.SelectSingleBound(this, null);
                if(item != null)
                    PXRedirectHelper.TryRedirect(this.Caches[typeof(InventoryItem)], item, "View Item", PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
        }


		public PXAction<ARInvoice> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddress, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton]
		public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			foreach (ARInvoice current in adapter.Get<ARInvoice>())
			{
				if (current != null)
				{
					bool needSave = false;
					Save.Press();
					ARAddress address = this.Billing_Address.Select();
					if (address != null && address.IsDefaultAddress == false && address.IsValidated == false)
					{
						if (PXAddressValidator.Validate<ARAddress>(this, address, true))
							needSave = true;
					}

					if (needSave == true)
						this.Save.Press();
				}
				yield return current;
			}
		}

        public PXAction<ARInvoice> recalculateDiscountsAction;
        [PXUIField(DisplayName = "Recalculate Prices and Discounts", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
        [PXButton]
        public virtual IEnumerable RecalculateDiscountsAction(PXAdapter adapter)
        {
            if (recalcdiscountsfilter.AskExt() == WebDialogResult.OK)
            {
                DiscountEngine<ARTran>.RecalculatePricesAndDiscounts<ARInvoiceDiscountDetail>(Transactions.Cache, Transactions, Transactions.Current, ARDiscountDetails, Document.Current.CustomerLocationID, Document.Current.DocDate.Value, recalcdiscountsfilter.Current);
            }
            return adapter.Get();
        }

        public PXAction<ARInvoice> recalcOk;
        [PXUIField(DisplayName = "OK", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable RecalcOk(PXAdapter adapter)
        {
            return adapter.Get();
        }

		#region Override PXGraph.GetStateExt
        private object disableJoined(object val)
        {
            PXFieldState stat = val as PXFieldState;
            if (stat != null)
            {
                stat.Enabled = false;
            }
            return val;
        }
        public override object GetStateExt(string viewName, object data, string fieldName)
        {
            if (viewName == "Adjustments")
            {
                if (data == null)
                {
                    int pos = fieldName.IndexOf("__");
                    if (pos > 0 && pos < fieldName.Length - 2)
                    {
                        string s = fieldName.Substring(0, pos);
                        PXCache cache = null;
                        foreach (Type t in Views[viewName].GetItemTypes())
                        {
                            if (t.Name == s)
                            {
                                cache = Caches[t];
                            }
                        }
                        if (cache == null)
                        {
                            cache = Caches[s];
                        }
                        if (cache != null)
                        {
                            return disableJoined(cache.GetStateExt(null, fieldName.Substring(pos + 2)));
                        }
                        return null;
                    }
                    else
                    {
                        return Caches[GetItemType(viewName)].GetStateExt(null, fieldName);
                    }
                }
                else
                {
                    return base.GetStateExt(viewName, data, fieldName);
                }
            }
            else
            {
                return base.GetStateExt(viewName, data, fieldName);
            }
        }
        #endregion

		public PXSelect<InventoryItem> dummy_nonstockitem_for_redirect_newitem;

		[PXViewName(Messages.ARInvoice)]
		[PXCopyPasteHiddenFields(typeof(ARInvoice.invoiceNbr), FieldsToShowInSimpleImport = new[] { typeof(ARInvoice.invoiceNbr) })]
		public PXSelectJoin<ARInvoice,
			LeftJoin<Customer, On<Customer.bAccountID, Equal<ARInvoice.customerID>>>,
			Where<ARInvoice.docType, Equal<Optional<ARInvoice.docType>>,
			And2<Where<ARInvoice.origModule, Equal<BatchModule.moduleAR>, Or<ARInvoice.released, Equal<True>>>,
			And<Where<Customer.bAccountID, IsNull, Or<Match<Customer, Current<AccessInfo.userName>>>>>>>> Document;
		public PXSelect<ARInvoice, Where<ARInvoice.docType, Equal<Current<ARInvoice.docType>>, And<ARInvoice.refNbr, Equal<Current<ARInvoice.refNbr>>>>> CurrentDocument;
		[PXViewName(Messages.ARTran)]
		[PXImport(typeof(ARInvoice))]
		public PXSelectJoin<ARTran, LeftJoin<SOLine, 
				On<SOLine.orderType, Equal<ARTran.sOOrderType>, 
				And<SOLine.orderNbr, Equal<ARTran.sOOrderNbr>,
				And<SOLine.lineNbr, Equal<ARTran.sOOrderLineNbr>>>>>,
			Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>, 
				And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>>>, 
			OrderBy<Asc<ARTran.tranType, Asc<ARTran.refNbr, Asc<ARTran.lineNbr>>>>> 
			Transactions;

        public PXSelect<ARTran, Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>, And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>, And<ARTran.lineType, Equal<SOLineType.discount>>>>, OrderBy<Asc<ARTran.tranType, Asc<ARTran.refNbr, Asc<ARTran.lineNbr>>>>> Discount_Row;

		public PXSelect<ARTax, Where<ARTax.tranType, Equal<Current<ARInvoice.docType>>, And<ARTax.refNbr, Equal<Current<ARInvoice.refNbr>>>>, OrderBy<Asc<ARTax.tranType, Asc<ARTax.refNbr, Asc<ARTax.taxID>>>>> Tax_Rows;		
        public PXSelectJoin<ARTaxTran, InnerJoin<Tax, On<Tax.taxID, Equal<ARTaxTran.taxID>>>, Where<ARTaxTran.module, Equal<BatchModule.moduleAR>, And<ARTaxTran.tranType, Equal<Current<ARInvoice.docType>>, And<ARTaxTran.refNbr, Equal<Current<ARInvoice.refNbr>>>>>> Taxes;

		[PXCopyPasteHiddenView()]
		public PXSelectJoin<ARAdjust, InnerJoin<ARPayment, On<ARPayment.docType, Equal<ARAdjust.adjgDocType>, And<ARPayment.refNbr, Equal<ARAdjust.adjgRefNbr>>>>> Adjustments;
		public PXSelectJoin<ARAdjust, 
							InnerJoin<ARPayment, On<ARPayment.docType, Equal<ARAdjust.adjgDocType>, And<ARPayment.refNbr, Equal<ARAdjust.adjgRefNbr>>>, 
							InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARPayment.curyInfoID>>>>, 
							Where<ARAdjust.adjdDocType, Equal<Current<ARInvoice.docType>>, 
								And<ARAdjust.adjdRefNbr, Equal<Current<ARInvoice.refNbr>>, 
								And<Where<Current<ARInvoice.released>,Equal<boolTrue>, Or<ARAdjust.released, Equal<Current<ARInvoice.released>>>>>>>> Adjustments_Raw;
		public PXSelect<PM.PMTran, Where<PM.PMTran.tranID, Equal<Required<ARTran.pMTranID>>>> RefPMTran;
        public PXSelect<PM.PMTran, Where<PM.PMTran.aRTranType, Equal<Required<ARTran.tranType>>, And<PM.PMTran.aRRefNbr, Equal<Required<ARTran.refNbr>>, And<PM.PMTran.refLineNbr, Equal<Required<ARTran.lineNbr>>>>>> RefContractUsageTran;
		[PXViewName(Messages.ARAddress)]
		public PXSelect<ARAddress, Where<ARAddress.addressID, Equal<Current<ARInvoice.billAddressID>>>> Billing_Address;
		[PXViewName(Messages.ARContact)]
		public PXSelect<ARContact, Where<ARContact.contactID, Equal<Current<ARInvoice.billContactID>>>> Billing_Contact;
		
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<ARInvoice.curyInfoID>>>> currencyinfo;

		public CMSetupSelect CMSetup;
		[PXViewName(Messages.Customer)]
		public PXSetup<Customer, Where<Customer.bAccountID, Equal<Optional<ARInvoice.customerID>>>> customer;
		public PXSetup<CustomerClass, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>> customerclass;
		public PXSetup<TaxZone, Where<TaxZone.taxZoneID, Equal<Current<ARInvoice.taxZoneID>>>> taxzone;
		public PXSetup<Location, Where<Location.bAccountID, Equal<Current<ARInvoice.customerID>>, And<Location.locationID, Equal<Optional<ARInvoice.customerLocationID>>>>> location;
		public PXSelect<ARBalances> arbalances;
		public PXSetup<FinPeriod, Where<FinPeriod.finPeriodID, Equal<Current<ARInvoice.finPeriodID>>>> finperiod;
		public PXSetup<ARSetup> ARSetup;
		public PXSetup<GLSetup> glsetup;
		public PXSetupOptional<TXAvalaraSetup> avalaraSetup;
		public PXSetupOptional<SOSetup> soSetup;
        public PXFilter<RecalcDiscountsParamFilter> recalcdiscountsfilter;

        [PXCopyPasteHiddenView()]
        public PXSelect<ARInvoiceDiscountDetail, Where<ARInvoiceDiscountDetail.docType, Equal<Current<ARInvoice.docType>>, And<ARInvoiceDiscountDetail.refNbr, Equal<Current<ARInvoice.refNbr>>>>, OrderBy<Asc<ARInvoiceDiscountDetail.docType, Asc<ARInvoiceDiscountDetail.refNbr>>>> ARDiscountDetails;
		
        public PXSelect<CustSalesPeople, Where<CustSalesPeople.bAccountID, Equal<Current<ARInvoice.customerID>>,
												And<CustSalesPeople.locationID, Equal<Current<ARInvoice.customerLocationID>>>>> salesPerSettings;
		public PXSelectJoin<ARSalesPerTran, LeftJoin<ARSPCommissionPeriod,On<ARSPCommissionPeriod.commnPeriodID,Equal<ARSalesPerTran.commnPaymntPeriod>>>,
                                                Where<ARSalesPerTran.docType, Equal<Current<ARInvoice.docType>>,
												And<ARSalesPerTran.refNbr, Equal<Current<ARInvoice.refNbr>>,
												And<ARSalesPerTran.adjdDocType, Equal<ARDocType.undefined>,
												And2<Where<Current<ARSetup.sPCommnCalcType>, Equal<SPCommnCalcTypes.byInvoice>, Or<Current<ARInvoice.released>, Equal<boolFalse>>>,
												Or<ARSalesPerTran.adjdDocType, Equal<Current<ARInvoice.docType>>,
												And<ARSalesPerTran.adjdRefNbr, Equal<Current<ARInvoice.refNbr>>,
												And<Current<ARSetup.sPCommnCalcType>, Equal<SPCommnCalcTypes.byPayment>>>>>>>>> salesPerTrans;
		public PXSelect<ARFinChargeTran, Where<ARFinChargeTran.tranType, Equal<Current<ARInvoice.docType>>,
												And<ARFinChargeTran.refNbr, Equal<Current<ARInvoice.refNbr>>>>> finChargeTrans;

		[CRBAccountReference(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>>>))]
		[CRDefaultMailTo(typeof(Select<ARContact, Where<ARContact.contactID, Equal<Current<ARInvoice.billContactID>>>>))]
		public CRActivityList<ARInvoice>
			Activity;

		public PXSelect<DRSchedule> dummySchedule_forPXParent;
		public PXSelect<DRScheduleDetail> dummyScheduleDetail_forPXParent;
		public PXSelect<DRScheduleTran> dummyScheduleTran_forPXParent;

        public PXFilter<DuplicateFilter> duplicatefilter;

        [PXViewName(CR.Messages.MainContact)]
        public PXSelect<Contact> DefaultCompanyContact;
        protected virtual IEnumerable defaultCompanyContact()
        {
            List<int> branches = PXAccess.GetMasterBranchID(Accessinfo.BranchID);
            foreach (PXResult<Branch, BAccountR, Contact> res in PXSelectJoin<Branch,
                                        LeftJoin<BAccountR, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>,
                                        LeftJoin<Contact, On<BAccountR.defContactID, Equal<Contact.contactID>>>>,
                                        Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.Select(this, branches != null ? (int?)branches[0] : null))
            {
                yield return (Contact)res;
                break;
            }
        }

        public PXSelect<Branch, Where<Branch.branchID, Equal<Required<Branch.branchID>>>> CurrentBranch;
        public PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>> InventoryItem;
        public PXSelect<RUTROTDistribution,
                    Where<RUTROTDistribution.docType, Equal<Current<ARInvoice.docType>>,
                    And<RUTROTDistribution.refNbr, Equal<Current<ARInvoice.refNbr>>>>> RUTROTDistribution;

        public virtual IEnumerable transactions()
        {
            foreach (PXResult<ARTran, SOLine> tran in PXSelectJoin<ARTran, LeftJoin<SOLine,
                On<SOLine.orderType, Equal<ARTran.sOOrderType>,
                And<SOLine.orderNbr, Equal<ARTran.sOOrderNbr>,
                And<SOLine.lineNbr, Equal<ARTran.sOOrderLineNbr>>>>>,
            Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>,
                And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>>>,
            OrderBy<Asc<ARTran.tranType, Asc<ARTran.refNbr, Asc<ARTran.lineNbr>>>>>.Select(this))
            {
                if (((ARTran)tran).LineType != SOLineType.Discount)
                    yield return tran;
            }
        }

        public virtual void ReverseInvoiceProc(ARRegister doc)
		{
            DuplicateFilter filter = PXCache<DuplicateFilter>.CreateCopy(duplicatefilter.Current);
            WebDialogResult dialogRes = duplicatefilter.View.Answer;

			this.Clear(PXClearOption.PreserveTimeStamp);
            
            foreach (PXResult<ARInvoice, CurrencyInfo, Terms, Customer> res in ARInvoice_CurrencyInfo_Terms_Customer.Select(this, (object)doc.DocType, doc.RefNbr, doc.CustomerID))
			{
				CurrencyInfo info = PXCache<CurrencyInfo>.CreateCopy((CurrencyInfo)res);
				info.CuryInfoID = null;
				info.IsReadOnly = false;
				info = PXCache<CurrencyInfo>.CreateCopy(this.currencyinfo.Insert(info));

				ARInvoice invoice = PXCache<ARInvoice>.CreateCopy((ARInvoice)res);
				invoice.CuryInfoID = info.CuryInfoID;
				switch(invoice.DocType)
				{
					case ARDocType.Invoice:
					case ARDocType.DebitMemo:
						invoice.DocType = ARDocType.CreditMemo;
					break;
					case ARDocType.CreditMemo:
						invoice.DocType = ARDocType.DebitMemo;
					break;
				}
				invoice.OrigModule = GL.BatchModule.AR;
				invoice.RefNbr = null;
                invoice.OrigModule = GL.BatchModule.AR;

				//must set for _RowSelected
				invoice.OpenDoc = true;
				invoice.Released = false;
				invoice.Hold = false;
				invoice.BatchNbr = null;
				invoice.ScheduleID = null;
				invoice.Scheduled = false;
				invoice.NoteID = null;
				invoice.RefNoteID = null;

				invoice.TermsID = null;
				invoice.InstallmentCntr = null;
				invoice.InstallmentNbr = null;
				invoice.DueDate = null;
				invoice.DiscDate = null;
				invoice.OrigDocType = doc.DocType;
				invoice.OrigRefNbr = doc.RefNbr;
				invoice.CuryOrigDiscAmt = 0m;
				invoice.FinPeriodID = doc.FinPeriodID;
				invoice.CuryDocBal = invoice.CuryOrigDocAmt;
				invoice.OrigDocDate = invoice.DocDate;
				invoice.CuryLineTotal = 0m;
				invoice.IsTaxPosted = false;
				invoice.IsTaxValid = false;
                invoice.CuryVatTaxableTotal = 0m;
                invoice.CuryVatExemptTotal = 0m;
				
                invoice.IsRUTROTDeductible = false;
                invoice.CuryRUTROTUndistributedAmt= 0m;
                invoice.CuryRUTROTDistributedAmt = 0m;
                invoice.CuryRUTROTTotalAmt = 0m;
                invoice.RUTROTUndistributedAmt = 0m;
                invoice.RUTROTDistributedAmt = 0m;
                invoice.RUTROTTotalAmt = 0m;
                invoice.RUTROTClaimDate = null;
                invoice.RUTROTClaimFileName = null;
                invoice.RUTROTDistributionLineCntr = 0;
				
				if (String.IsNullOrEmpty(invoice.PaymentMethodID) == false)
				{
					CA.PaymentMethod pm = null;
					if (invoice.CashAccountID.HasValue)
					{
						CA.PaymentMethodAccount pmAccount = null;
						PXResult<CA.PaymentMethod, CA.PaymentMethodAccount> pmResult = (PXResult<CA.PaymentMethod, CA.PaymentMethodAccount>)PXSelectJoin<CA.PaymentMethod,
														LeftJoin<CA.PaymentMethodAccount,
															On<CA.PaymentMethod.paymentMethodID, Equal<CA.PaymentMethodAccount.paymentMethodID>>>,
														Where<CA.PaymentMethod.paymentMethodID, Equal<Required<CA.PaymentMethod.paymentMethodID>>,
															And<CA.PaymentMethodAccount.cashAccountID, Equal<Required<CA.PaymentMethodAccount.cashAccountID>>>>>.Select(this, invoice.PaymentMethodID, invoice.CashAccountID);
						pm = pmResult;
						pmAccount = pmResult;
						if (pm == null || pm.UseForAR == false || pm.IsActive == false)
						{
							invoice.PaymentMethodID = null;
							invoice.CashAccountID = null;
						}
						else if (pmAccount == null || pmAccount.CashAccountID == null || pmAccount.UseForAR != true)
						{
							invoice.CashAccountID = null;
						}
					}
					else
					{
						pm = PXSelect<CA.PaymentMethod,
								Where<CA.PaymentMethod.paymentMethodID, Equal<Required<CA.PaymentMethod.paymentMethodID>>>>.Select(this, invoice.PaymentMethodID);
						if (pm == null || pm.UseForAR == false || pm.IsActive == false)
						{
							invoice.PaymentMethodID = null;
							invoice.CashAccountID = null;
							invoice.PMInstanceID = null;
						}
					}
					if (invoice.PMInstanceID.HasValue)
					{
						CustomerPaymentMethod cpm = PXSelect<CustomerPaymentMethod,
												Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>.Select(this, invoice.PMInstanceID);
						if (string.IsNullOrEmpty(invoice.PaymentMethodID) || cpm == null || cpm.IsActive == false || cpm.PaymentMethodID != invoice.PaymentMethodID)
						{
							invoice.PMInstanceID = null;
						}
					}
				}
				else
				{
					invoice.CashAccountID = null;
					invoice.PMInstanceID = null;
				}
                isReverse = true;
                SalesPerson sp = (SalesPerson)PXSelectorAttribute.Select<ARInvoice.salesPersonID>(this.Document.Cache, invoice);
                if (sp == null || sp.IsActive == false)
                    invoice.SalesPersonID = null;
				invoice = this.Document.Insert(invoice);
                isReverse = false;
				if (invoice.RefNbr == null)
				{
					//manual numbering, check for occasional duplicate
					ARInvoice duplicate = PXSelect<ARInvoice>.Search<ARInvoice.docType, ARInvoice.refNbr>(this, invoice.DocType, invoice.OrigRefNbr);
					if (duplicate != null)
					{
                        PXCache<DuplicateFilter>.RestoreCopy(duplicatefilter.Current, filter);
                        duplicatefilter.View.Answer = dialogRes;
                        if (duplicatefilter.AskExt() == WebDialogResult.OK)
                        {
                            duplicatefilter.Cache.Clear();
                            if (duplicatefilter.Current.RefNbr == null)
                                throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(DuplicateFilter.refNbr).Name);
                            duplicate = PXSelect<ARInvoice>.Search<ARInvoice.docType, ARInvoice.refNbr>(this, invoice.DocType, duplicatefilter.Current.RefNbr);
                            if (duplicate != null)
						throw new PXException(ErrorMessages.RecordExists);
                            invoice.RefNbr = duplicatefilter.Current.RefNbr;
                        }
					}
                    else
                        invoice.RefNbr = invoice.OrigRefNbr;

					this.Document.Cache.Normalize();
					invoice = this.Document.Update(invoice);
				}
				
				if (info != null)
				{
					CurrencyInfo b_info = (CurrencyInfo)PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<ARInvoice.curyInfoID>>>>.Select(this, null);
					b_info.CuryID = info.CuryID;
					b_info.CuryEffDate = info.CuryEffDate;
					b_info.CuryRateTypeID = info.CuryRateTypeID;
					b_info.CuryRate = info.CuryRate;
					b_info.RecipRate = info.RecipRate;
					b_info.CuryMultDiv = info.CuryMultDiv;
					this.currencyinfo.Update(b_info);
				}				
			}

            TaxAttribute.SetTaxCalc<ARTran.taxCategoryID, ARTaxAttribute>(this.Transactions.Cache, null, TaxCalc.ManualCalc);

			this.FieldDefaulting.AddHandler<ARTran.salesPersonID>((sender, e) => { e.NewValue = null; e.Cancel = true; });

            foreach (ARTran tran in PXSelect<ARTran, Where<ARTran.tranType, Equal<Required<ARTran.tranType>>, And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
            {
                tran.TranType = null;
                tran.RefNbr = null;
                tran.DrCr = null;
                tran.Released = null;
                tran.CuryInfoID = null;
                tran.SOOrderNbr = null;
                tran.SOShipmentNbr = null;
                tran.OrigInvoiceDate = tran.TranDate;
                tran.IsRUTROTDeductible = false;
                tran.CuryRUTROTAvailableAmt = 0.0m;
                tran.CuryRUTROTTaxAmountDeductible = 0.0m;
                tran.RUTROTAvailableAmt = 0.0m;
                tran.RUTROTTaxAmountDeductible = 0.0m;

                if (!string.IsNullOrEmpty(tran.DeferredCode))
                {
                    DRSchedule schedule = PXSelect<DRSchedule,
                        Where<DRSchedule.module, Equal<moduleAR>,
                        And<DRSchedule.docType, Equal<Required<DRSchedule.docType>>,
                        And<DRSchedule.refNbr, Equal<Required<DRSchedule.refNbr>>,
                        And<DRSchedule.lineNbr, Equal<Required<DRSchedule.lineNbr>>>>>>>.Select(this, doc.DocType, doc.RefNbr, tran.LineNbr);

                    if (schedule != null)
                    {
                        tran.DefScheduleID = schedule.ScheduleID;
                    }
                }

                SalesPerson sp = (SalesPerson)PXSelectorAttribute.Select<ARTran.salesPersonID>(this.Transactions.Cache, tran);
                if (sp == null || sp.IsActive == false)
                    tran.SalesPersonID = null;

                this.Transactions.Insert(tran);
            }

			this.RowInserting.AddHandler<ARSalesPerTran>((sender, e) => { e.Cancel = true; });

			foreach (ARSalesPerTran salespertran in PXSelect<ARSalesPerTran, Where<ARSalesPerTran.docType, Equal<Required<ARSalesPerTran.docType>>, And<ARSalesPerTran.refNbr, Equal<Required<ARSalesPerTran.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
			{
				ARSalesPerTran newtran = PXCache<ARSalesPerTran>.CreateCopy(salespertran);

				newtran.DocType = Document.Current.DocType;
				newtran.RefNbr = Document.Current.RefNbr;
				newtran.Released = false;
				newtran.CuryInfoID = null;
				newtran.CuryCommnblAmt *= -1m;
				newtran.CuryCommnAmt *= -1m;

				SalesPerson sp = (SalesPerson)PXSelectorAttribute.Select<ARSalesPerTran.salespersonID>(this.salesPerTrans.Cache, newtran);
				if (!(sp == null || sp.IsActive == false))
				{
					this.salesPerTrans.Update(newtran);
				}
			}    

			if (IsExternalTax != true)
			{
				foreach (ARTaxTran tax in PXSelect<ARTaxTran, Where<ARTaxTran.tranType, Equal<Required<ARTaxTran.tranType>>, And<ARTaxTran.refNbr, Equal<Required<ARTaxTran.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
				{
					ARTaxTran new_artax = new ARTaxTran();
					new_artax.TaxID = tax.TaxID;

					new_artax = this.Taxes.Insert(new_artax);

					if (new_artax != null)
					{
						new_artax = PXCache<ARTaxTran>.CreateCopy(new_artax);
						new_artax.TaxRate = tax.TaxRate;
						new_artax.CuryTaxableAmt = tax.CuryTaxableAmt;
						new_artax.CuryTaxAmt = tax.CuryTaxAmt;
						new_artax = this.Taxes.Update(new_artax);
					}
				}
			}
		}

		protected string salesSubMask;

		public virtual string SalesSubMask
		{
			get
			{
				if (salesSubMask == null)
				{
					salesSubMask = ARSetup.Current.SalesSubMask;
				}

				return salesSubMask;
			}
			set
			{
				salesSubMask = value;
			}

		}


		#region CurrencyInfo events
		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if ((bool)CMSetup.Current.MCActivated)
			{
				if (customer.Current != null && !string.IsNullOrEmpty(customer.Current.CuryID))
				{
					e.NewValue = customer.Current.CuryID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if ((bool)CMSetup.Current.MCActivated)
			{
				if (customer.Current != null && !string.IsNullOrEmpty(customer.Current.CuryRateTypeID))
				{
					e.NewValue = customer.Current.CuryRateTypeID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Cache.Current != null)
			{
				e.NewValue = ((ARInvoice)Document.Cache.Current).DocDate;
				e.Cancel = true;
			}
		}

		protected virtual void CurrencyInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CurrencyInfo info = e.Row as CurrencyInfo;
			if (info != null)
			{
				bool curyenabled = info.AllowUpdate(this.Transactions.Cache);

				if (customer.Current != null && !(bool)customer.Current.AllowOverrideRate)
				{
					curyenabled = false;
				}

				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(sender, info, curyenabled);
			}
		}
		#endregion


		protected virtual void ParentFieldUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<ARInvoice.docDate, ARInvoice.finPeriodID, ARInvoice.curyID>(e.Row, e.OldRow))
			{
				foreach (ARTran tran in Transactions.Select())
				{
					if (Transactions.Cache.GetStatus(tran) == PXEntryStatus.Notchanged)
					{
						Transactions.Cache.SetStatus(tran, PXEntryStatus.Updated);
					}
				}

				foreach (ARAdjust tran in Adjustments.Select())
				{
					if (Adjustments.Cache.GetStatus(tran) == PXEntryStatus.Notchanged)
					{
						Adjustments.Cache.SetStatus(tran, PXEntryStatus.Updated);
					}
				}

				foreach (ARSalesPerTran tran in salesPerTrans.Select())
				{
					if (this.salesPerTrans.Cache.GetStatus(tran) == PXEntryStatus.Notchanged)
					{
						this.salesPerTrans.Cache.SetStatus(tran, PXEntryStatus.Updated);
					}
				}

				foreach (ARFinChargeTran tran in this.finChargeTrans.Select())
				{
					if (this.finChargeTrans.Cache.GetStatus(tran) == PXEntryStatus.Notchanged)
					{
						this.finChargeTrans.Cache.SetStatus(tran, PXEntryStatus.Updated);
					}
				}
			}

			if (!sender.ObjectsEqual<ARInvoice.branchID>(e.Row, e.OldRow))
			{
				foreach (ARSalesPerTran tran in salesPerTrans.Select())
				{
					if (this.salesPerTrans.Cache.GetStatus(tran) == PXEntryStatus.Notchanged)
					{
						this.salesPerTrans.Cache.SetStatus(tran, PXEntryStatus.Updated);
					}
				}
			}
		}

		public ARInvoiceEntry()
		{
			ARSetup setup = ARSetup.Current;
			RowUpdated.AddHandler<ARInvoice>(ParentFieldUpdated);

			TaxAttribute.SetTaxCalc<ARTran.taxCategoryID, ARTaxAttribute>(Transactions.Cache, null, TaxCalc.ManualLineCalc);

            FieldDefaulting.AddHandler<InventoryItem.stkItem>((sender, e) => { if (e.Row != null) e.NewValue = false; });
        }

		public override void Persist()
		{
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
					decimal? CuryApplAmt = 0m;

					foreach (PXResult<ARAdjust, ARPayment, CurrencyInfo> res in Adjustments_Raw.View.SelectMultiBound(new object[] { ardoc }))
					{						
						ARAdjust adj = (ARAdjust)res;
						if (adj == null) continue;

						CuryApplAmt += adj.CuryAdjdAmt;

						if (ardoc.CuryDocBal - CuryApplAmt < 0m)
						{
							if (Adjustments.Cache.GetStatus(adj) == PXEntryStatus.Notchanged)
							{
								Adjustments.Cache.SetStatus(adj, PXEntryStatus.Updated);
							}
							Adjustments.Cache.RaiseExceptionHandling<ARAdjust.curyAdjdAmt>(adj, adj.CuryAdjdAmt, new PXSetPropertyException(Messages.Application_Amount_Cannot_Exceed_Document_Amount));
							throw new PXException(Messages.Application_Amount_Cannot_Exceed_Document_Amount);
						}
					}
				}
			}

			base.Persist();

			if (Document.Current != null && IsExternalTax == true && Document.Current.IsTaxValid != true && !skipAvalaraCallOnSave)
			{
				if (PXLongOperation.GetCurrentItem() == null)
				{
					PXLongOperation.StartOperation(this, delegate()
					{
						ARInvoice doc = new ARInvoice();
						doc.DocType = Document.Current.DocType;
						doc.RefNbr = Document.Current.RefNbr;
						doc.OrigModule = Document.Current.OrigModule;
						doc.ApplyPaymentWhenTaxAvailable = Document.Current.ApplyPaymentWhenTaxAvailable;
						ARExternalTaxCalc.Process(doc);

						RecalcUnbilledTax();
					});
				}
				else
				{
						
						ARExternalTaxCalc.Process(this);

						RecalcUnbilledTax();
				}
			}
		}

		protected virtual void RecalcUnbilledTax()
		{

		}

		public PXAction<ARInvoice> notification;
		[PXUIField(DisplayName = "Notifications", Visible = false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		protected virtual IEnumerable Notification(PXAdapter adapter,
		[PXString]
		string notificationCD
		)
		{
			foreach (ARInvoice doc in adapter.Get())
			{
				var parameters = new Dictionary<string, string>();
				parameters["DocType"] = doc.DocType;
				parameters["RefNbr"] = doc.RefNbr;
				Activity.SendNotification(ARNotificationSource.Customer, notificationCD, doc.BranchID, parameters);
				yield return doc;
			}
		}
		protected override string GetCustomerReportID(string reportID, ARInvoice doc)
		{
			this.Document.Current = doc;
			return new NotificationUtility(this).SearchReport(ARNotificationSource.Customer, customer.SelectSingle(), reportID, doc.BranchID);
		}

		public virtual IEnumerable adjustments()
		{
			CurrencyInfo inv_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<ARInvoice.curyInfoID>>>>.Select(this);

			foreach (PXResult<ARAdjust, ARPayment, CurrencyInfo> res in Adjustments_Raw.Select())
			{
				ARPayment payment = res;
				ARAdjust adj = res;
				CurrencyInfo pay_info = res;

				decimal CuryDocBal;
				if (string.Equals(pay_info.CuryID, inv_info.CuryID))
				{
					CuryDocBal = (decimal)payment.CuryDocBal;
				}
				else
				{
					PXDBCurrencyAttribute.CuryConvCury(Adjustments.Cache, inv_info, (decimal)payment.DocBal, out CuryDocBal);
				}

				if (adj != null && adj.Released == false)
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
					foreach (PXResult<ARPayment, CurrencyInfo, ARAdjust> res in PXSelectReadonly2<ARPayment, 
						InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARPayment.curyInfoID>>, 
						LeftJoin<ARAdjust, 
							On<ARAdjust.adjgDocType, Equal<ARPayment.docType>, 
							And<ARAdjust.adjgRefNbr, Equal<ARPayment.refNbr>, 
							And<ARAdjust.adjNbr, Equal<ARPayment.lineCntr>, 
							And<ARAdjust.released, Equal<False>, 
							And<ARAdjust.voided, Equal<False>, 
							And<Where<ARAdjust.adjdDocType, NotEqual<Current<ARInvoice.docType>>, 
								Or<ARAdjust.adjdRefNbr, NotEqual<Current<ARInvoice.refNbr>>>>>>>>>>>>, 
						Where<ARPayment.customerID, Equal<Current<ARInvoice.customerID>>, 
							And2<Where<ARPayment.docType, Equal<ARDocType.payment>, 
								Or<ARPayment.docType, Equal<ARDocType.prepayment>, 
								Or<ARPayment.docType, Equal<ARDocType.creditMemo>>>>, 
							And<ARPayment.docDate, LessEqual<Current<ARInvoice.docDate>>, 
							And<ARPayment.finPeriodID, LessEqual<Current<ARInvoice.finPeriodID>>, 
							And<ARPayment.released, Equal<boolTrue>, 
							And<ARPayment.openDoc, Equal<boolTrue>, 
							And<ARAdjust.adjdRefNbr, IsNull>>>>>>>>.Select(this))
					{
						ARPayment payment = res;
						ARAdjust adj = new ARAdjust();
						CurrencyInfo pay_info = res;

						adj.CustomerID = Document.Current.CustomerID;
						adj.AdjdDocType = Document.Current.DocType;
						adj.AdjdRefNbr = Document.Current.RefNbr;
						adj.AdjdBranchID = Document.Current.BranchID;
						adj.AdjgDocType = payment.DocType;
						adj.AdjgRefNbr = payment.RefNbr;
						adj.AdjgBranchID = payment.BranchID;
						adj.AdjNbr = payment.LineCntr;

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

							yield return new PXResult<ARAdjust, ARPayment>(Adjustments.Insert(adj), payment);
						}
					}
				}
			}
		}

		private class PXLoadInvoiceException : Exception
		{
			public PXLoadInvoiceException() { }

			public PXLoadInvoiceException(SerializationInfo info, StreamingContext context)
				: base(info, context) { }
		}

		public virtual void LoadInvoicesProc()
		{
			try
			{
				if (Document.Current == null || Document.Current.CustomerID == null || Document.Current.OpenDoc == false || Document.Current.DocType != ARDocType.Invoice)
				{
					throw new PXLoadInvoiceException();
				}

				if (Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Notchanged || Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Held)
				{
					Document.Cache.SetStatus(Document.Current, PXEntryStatus.Updated);
				}
				Document.Cache.IsDirty = true;

				decimal? CuryUnappliedBal = Document.Current.CuryDocBal;

				foreach (ARAdjust adj in Adjustments.Select())
				{
					ARAdjust copy = PXCache<ARAdjust>.CreateCopy(adj);

					if (CuryUnappliedBal > copy.CuryDocBal)
					{
						copy.CuryAdjdAmt = copy.CuryDocBal;
						CuryUnappliedBal -= copy.CuryAdjdAmt;
					}
					else
					{
						copy.CuryAdjdAmt = CuryUnappliedBal;
						CuryUnappliedBal = 0m;
					}

					Adjustments.Cache.Update(copy);

					if (CuryUnappliedBal == 0m)
					{
						throw new PXLoadInvoiceException();
					}
				}
			}
			catch (PXLoadInvoiceException)
			{
			}
		}

		#region ARInvoince Events
        
		protected virtual void ARInvoice_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARInvoice row = e.Row as ARInvoice;
			if (row != null)
			{
				Location customerLocation = location.Select();
				if (customerLocation != null)
				{
					if (!string.IsNullOrEmpty(customerLocation.CTaxZoneID))
					{
						e.NewValue = customerLocation.CTaxZoneID;
					}
					else 
					{
                        BAccount companyAccount = PXSelectJoin<BAccountR, InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.Select(this, row.BranchID);
                        if (companyAccount != null)
                        {
                            Location companyLocation = PXSelect<Location, Where<Location.bAccountID, Equal<Required<Location.bAccountID>>, And<Location.locationID, Equal<Required<Location.locationID>>>>>.Select(this, companyAccount.BAccountID, companyAccount.DefLocationID);
                            if (companyLocation != null)
                                e.NewValue = companyLocation.VTaxZoneID;
                        }
					}
				}
		
			}
		}

        protected virtual void ARInvoice_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            sender.SetDefaultExt<ARInvoice.taxZoneID>(e.Row);
            
            if (PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>())
                UncheckRUTROTIfProhibited(e.Row as ARInvoice);
        }

		private bool IsTaxZoneDerivedFromCustomer()
		{
			Location customerLocation = location.Select();
			if (customerLocation != null)
			{
				if (!string.IsNullOrEmpty(customerLocation.CTaxZoneID))
				{
					return true;
				}
			}

			return false;
		}

		protected virtual void ARAddress_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARAddress row = e.Row as ARAddress;
            ARAddress oldRow = e.OldRow as ARAddress;
			if (row != null)
			{
                if (!IsTaxZoneDerivedFromCustomer() && !string.IsNullOrEmpty(row.PostalCode) && oldRow.PostalCode != row.PostalCode)
				{
					string taxZone = TaxBuilderEngine.GetTaxZoneByZip(this, row.PostalCode);
					Document.Cache.SetValueExt<ARInvoice.taxZoneID>(Document.Current, taxZone);
				}
			}
		}


		protected virtual void ARInvoice_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = ARDocType.Invoice;
		}
		protected virtual void ARInvoice_DontPrint_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if(this.customer.Current != null)
				e.NewValue =  !(this.customer.Current.PrintInvoices == true);			
		}
		protected virtual void ARInvoice_DontEmail_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.customer.Current != null)
				e.NewValue = !(this.customer.Current.MailInvoices == true);			
		}

		private object GetAcctSub<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			object NewValue = cache.GetValueExt<Field>(data);
			if (NewValue is PXFieldState)
			{
				return ((PXFieldState)NewValue).Value;
			}
			else
			{
				return NewValue;
			}
		}

		protected virtual void ARInvoice_ARAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (location.Current != null && e.Row != null)
			{
				e.NewValue = GetAcctSub<CR.Location.aRAccountID>(location.Cache, location.Current);
			}
		}

		protected virtual void ARInvoice_ARSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (location.Current != null && e.Row != null)
			{
				e.NewValue = GetAcctSub<CR.Location.aRSubID>(location.Cache, location.Current);
			}
		}

		protected virtual void ARInvoice_CustomerLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			location.RaiseFieldUpdated(sender, e.Row);

			sender.SetDefaultExt<ARInvoice.aRAccountID>(e.Row);
			sender.SetDefaultExt<ARInvoice.aRSubID>(e.Row);
			sender.SetDefaultExt<ARInvoice.branchID>(e.Row);
			sender.SetDefaultExt<ARInvoice.taxZoneID>(e.Row);
			sender.SetDefaultExt<ARInvoice.avalaraCustomerUsageType>(e.Row);
			sender.SetDefaultExt<ARInvoice.salesPersonID>(e.Row);
			sender.SetDefaultExt<ARInvoice.workgroupID>(e.Row);
			sender.SetDefaultExt<ARInvoice.ownerID>(e.Row);

			if (PM.ProjectAttribute.IsPMVisible(this, BatchModule.AR))
			{
				sender.SetDefaultExt<ARInvoice.projectID>(e.Row);
			}
		}

		protected virtual void ARInvoice_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARInvoice invoice = (ARInvoice)e.Row;
			customer.RaiseFieldUpdated(sender, e.Row);
	
			if (!e.ExternalCall)
			{
				customer.Current.CreditRule = null;
			}

			if ((bool)CMSetup.Current.MCActivated)
			{
				if (e.ExternalCall || sender.GetValuePending<ARInvoice.curyID>(e.Row) == null)
				{
				CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<ARInvoice.curyInfoID>(sender, e.Row);

				string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);
				if (string.IsNullOrEmpty(message) == false)
				{
					sender.RaiseExceptionHandling<ARInvoice.docDate>(e.Row, ((ARInvoice)e.Row).DocDate, new PXSetPropertyException(message, PXErrorLevel.Warning));
				}

				if (info != null)
				{
					((ARInvoice)e.Row).CuryID = info.CuryID;
				}
			}
			}

			{
				sender.SetDefaultExt<ARInvoice.customerLocationID>(e.Row);
				sender.SetDefaultExt<ARInvoice.dontPrint>(e.Row);
				sender.SetDefaultExt<ARInvoice.dontEmail>(e.Row);

				if (e.ExternalCall || sender.GetValuePending<ARInvoice.termsID>(e.Row) == null)
				{
				if (((ARInvoice)e.Row).DocType != ARDocType.CreditMemo)
				{
					sender.SetDefaultExt<ARInvoice.termsID>(e.Row);
				}
				else
				{
					sender.SetValueExt<ARInvoice.termsID>(e.Row, null);
				}
			}
			}

			try
			{
				ARAddressAttribute.DefaultRecord<ARInvoice.billAddressID>(sender, e.Row);
				ARContactAttribute.DefaultRecord<ARInvoice.billContactID>(sender, e.Row);
			}
			catch (PXFieldValueProcessingException ex)
			{
				ex.ErrorValue = customer.Current.AcctCD;
				throw;
			}

			sender.SetDefaultExt<ARInvoice.taxZoneID>(e.Row);
            sender.SetDefaultExt<ARInvoice.paymentMethodID>(e.Row);    

            if (PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>())
                UncheckRUTROTIfProhibited(invoice);
		}

		protected virtual void ARInvoice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ARInvoice doc = (ARInvoice)e.Row;

			if (doc.DocType != ARDocType.CreditMemo && string.IsNullOrEmpty(doc.TermsID))
			{
				if (sender.RaiseExceptionHandling<ARInvoice.termsID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARInvoice.termsID).Name)))
				{
					throw new PXRowPersistingException(typeof(ARInvoice.termsID).Name, null, ErrorMessages.FieldIsEmpty, typeof(ARInvoice.termsID).Name);
				}
			}

			if (doc.DocType != ARDocType.CreditMemo && doc.DueDate == null)
			{
				if (sender.RaiseExceptionHandling<ARInvoice.dueDate>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARInvoice.dueDate).Name)))
				{
					throw new PXRowPersistingException(typeof(ARInvoice.dueDate).Name, null, ErrorMessages.FieldIsEmpty, typeof(ARInvoice.dueDate).Name);
				}
			}

			if (doc.DocType != ARDocType.CreditMemo && doc.DiscDate == null)
			{
				if (sender.RaiseExceptionHandling<ARInvoice.discDate>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARInvoice.discDate).Name)))
				{
					throw new PXRowPersistingException(typeof(ARInvoice.discDate).Name, null, ErrorMessages.FieldIsEmpty, typeof(ARInvoice.discDate).Name);
				}
			}

			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert) && (doc.DocType == ARDocType.FinCharge))
			{
				if (this.Accessinfo.ScreenID == "AR.30.10.00")
				{
					throw new PXException(PX.Objects.AR.Messages.FinChargeCanNotBeDeleted);
				}
			}

            if (doc.CuryDiscTot > doc.CuryLineTotal)
            {
                if (sender.RaiseExceptionHandling<ARInvoice.curyDiscTot>(e.Row, doc.CuryDiscTot, new PXSetPropertyException(Messages.DiscountGreaterLineTotal, PXErrorLevel.Error)))
                {
                    throw new PXRowPersistingException(typeof(ARInvoice.curyDiscTot).Name, null, Messages.DiscountGreaterLineTotal);
                }
            }

			//Cancel tax if document is deleted
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete && doc.IsTaxSaved == true)
			{
				CancelTax(doc, CancelCode.DocDeleted);
			}

			//Cancel tax if last line in the document is deleted
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update) 
				&& doc.IsTaxSaved == true && Transactions.Select().Count == 0)
			{
				CancelTax(doc, CancelCode.DocDeleted);
			}

			//Cancel tax if IsExternalTax has changed to False (Document was changed from Avalara TaxEngine to Acumatica Tax Engine )
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
				&& IsExternalTax == false && doc.IsTaxSaved == true)
			{
				CancelTax(doc, CancelCode.DocDeleted);
			}

            if (PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>())
            {
                bool treatAsError = doc.Hold != true;
                var errorLevel = treatAsError ? PXErrorLevel.Error : PXErrorLevel.Warning;

                if (WarnOnDeductionExceedsAllowance(doc, errorLevel) && treatAsError)
                    throw new PXSetPropertyException(doc.CuryRUTROTAllowedAmt == 0.0m ? RUTROTMessages.PeopleAreRequiredForDeduction : RUTROTMessages.DeductibleExceedsAllowance, PXErrorLevel.Error);

                if (WarnUndistributedAmount(doc, errorLevel) && treatAsError)
                    throw new PXSetPropertyException(RUTROTMessages.UndistributedAmount, PXErrorLevel.Error);
		}
		}

		protected virtual void ARInvoice_DocDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CurrencyInfoAttribute.SetEffectiveDate<ARInvoice.docDate>(sender, e);
		}

		protected virtual void ARInvoice_TermsID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Terms terms = (Terms)PXSelectorAttribute.Select<ARInvoice.termsID>(sender, e.Row);

			if (terms != null && terms.InstallmentType != TermsInstallmentType.Single)
			{
				foreach (ARAdjust adj in Adjustments.Select())
				{
					Adjustments.Cache.Delete(adj);
				}
			}
		}

        protected virtual void ARInvoice_PaymentMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            sender.SetDefaultExt<ARInvoice.pMInstanceID>(e.Row);
            sender.SetDefaultExt<ARInvoice.cashAccountID>(e.Row);
        }

		protected virtual void ARInvoice_PMInstanceID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			//sender.SetDefaultExt<ARInvoice.paymentMethodID>(e.Row);
			sender.SetDefaultExt<ARInvoice.cashAccountID>(e.Row);
		}

		protected virtual void ARInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			ARInvoice doc = e.Row as ARInvoice;
		    if (doc == null) return;

			PXUIFieldAttribute.SetVisible<ARInvoice.curyID>(cache, doc, (bool)CMSetup.Current.MCActivated);
			
			PXUIFieldAttribute.SetRequired<ARInvoice.termsID>(cache, (doc.DocType != ARDocType.CreditMemo));
			PXUIFieldAttribute.SetRequired<ARInvoice.dueDate>(cache, (doc.DocType != ARDocType.CreditMemo));
			PXUIFieldAttribute.SetRequired<ARInvoice.discDate>(cache, (doc.DocType != ARDocType.CreditMemo));
			PXUIFieldAttribute.SetVisible<ARTran.origInvoiceDate>(Transactions.Cache, null, doc.DocType == ARInvoiceType.CreditMemo);

		    bool curyenabled = !(customer.Current != null && customer.Current.AllowOverrideCury != true);

            if (doc.Released == true || doc.Voided == true || doc.DocType == ARDocType.SmallCreditWO)
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.dueDate>(cache, doc, (doc.DocType != ARDocType.CreditMemo && doc.DocType != ARDocType.SmallCreditWO) && doc.OpenDoc == true);
				PXUIFieldAttribute.SetEnabled<ARInvoice.discDate>(cache, doc, (doc.DocType != ARDocType.CreditMemo && doc.DocType != ARDocType.SmallCreditWO) && doc.OpenDoc == true);
				PXUIFieldAttribute.SetEnabled<ARInvoice.emailed>(cache, doc, true);
				cache.AllowDelete = false;
				cache.AllowUpdate = true;
				Transactions.Cache.AllowDelete = false;
				Transactions.Cache.AllowUpdate = false;
				Transactions.Cache.AllowInsert = false;

                ARDiscountDetails.Cache.AllowDelete = false;
                ARDiscountDetails.Cache.AllowUpdate = false;
                ARDiscountDetails.Cache.AllowInsert = false;

				Taxes.Cache.AllowUpdate = false;

				release.SetEnabled(doc.DocType == ARDocType.SmallCreditWO && doc.Released == false);
				createSchedule.SetEnabled(false);
				payInvoice.SetEnabled(doc.OpenDoc == true && doc.Payable == true);
				bool enablePM = doc.DocType == ARDocType.Invoice && (doc.Scheduled == true);
				PXUIFieldAttribute.SetEnabled<ARInvoice.paymentMethodID>(cache, doc, enablePM);
				
				if (enablePM) 
				{
					bool hasPaymentMethod = (String.IsNullOrEmpty(doc.PaymentMethodID) == false);
					bool isPMInstanceRequired = false;
					if (hasPaymentMethod)
					{
						CA.PaymentMethod pm = PXSelect<CA.PaymentMethod, Where<CA.PaymentMethod.paymentMethodID, Equal<Required<CA.PaymentMethod.paymentMethodID>>>>.Select(this, doc.PaymentMethodID);
						isPMInstanceRequired = pm.IsAccountNumberRequired == true;
					}
					PXUIFieldAttribute.SetEnabled<ARInvoice.pMInstanceID>(cache, doc, enablePM && hasPaymentMethod && isPMInstanceRequired);
					PXUIFieldAttribute.SetEnabled<ARInvoice.cashAccountID>(cache, e.Row, enablePM && hasPaymentMethod);
				}
				
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<ARInvoice.status>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.curyDocBal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.curyLineTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.curyTaxTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.batchNbr>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.curyID>(cache, doc, curyenabled);
				PXUIFieldAttribute.SetEnabled<ARInvoice.hold>(cache, doc, (doc.Scheduled != true));
                PXUIFieldAttribute.SetEnabled<ARInvoice.curyVatExemptTotal>(cache, doc, false);
                PXUIFieldAttribute.SetEnabled<ARInvoice.curyVatTaxableTotal>(cache, doc, false);

                
                bool hasPaymentMethod = (String.IsNullOrEmpty(doc.PaymentMethodID) == false);
                bool isPMInstanceRequired = false;
                if (doc.DocType == ARDocType.Invoice && hasPaymentMethod) 
                {
                    CA.PaymentMethod pm = PXSelect<CA.PaymentMethod, Where<CA.PaymentMethod.paymentMethodID, Equal<Required<CA.PaymentMethod.paymentMethodID>>>>.Select(this, doc.PaymentMethodID);
                    isPMInstanceRequired = pm.IsAccountNumberRequired == true; 
                }
                PXUIFieldAttribute.SetEnabled<ARInvoice.paymentMethodID>(cache, e.Row, doc.DocType == ARDocType.Invoice);
                PXUIFieldAttribute.SetEnabled<ARInvoice.pMInstanceID>(cache, e.Row, doc.DocType == ARDocType.Invoice && isPMInstanceRequired);
                PXUIFieldAttribute.SetEnabled<ARInvoice.cashAccountID>(cache, e.Row, hasPaymentMethod);
                

				PXUIFieldAttribute.SetEnabled<ARInvoice.termsID>(cache, doc, (doc.DocType != ARDocType.CreditMemo));
				PXUIFieldAttribute.SetEnabled<ARInvoice.dueDate>(cache, doc, (doc.DocType != ARDocType.CreditMemo));
				PXUIFieldAttribute.SetEnabled<ARInvoice.discDate>(cache, doc, (doc.DocType != ARDocType.CreditMemo));
				PXUIFieldAttribute.SetEnabled<ARInvoice.curyOrigDiscAmt>(cache, doc, (doc.DocType != ARDocType.CreditMemo));

				//PXUIFieldAttribute.SetEnabled<ARTran.deferredCode>(Transactions.Cache, null, (doc.DocType == ARDocType.Invoice || doc.DocType == ARDocType.DebitMemo));
				PXUIFieldAttribute.SetEnabled<ARTran.defScheduleID>(Transactions.Cache, null, (doc.DocType == ARDocType.CreditMemo || doc.DocType == ARDocType.DebitMemo));

				//calculate only on data entry, differences from the applications will be moved to RGOL upon closure
				PXDBCurrencyAttribute.SetBaseCalc<ARInvoice.curyDocBal>(cache, doc, true);
				PXDBCurrencyAttribute.SetBaseCalc<ARInvoice.curyDiscBal>(cache, doc, true);

				cache.AllowDelete = true;
				cache.AllowUpdate = true;
				Transactions.Cache.AllowDelete = true;
				Transactions.Cache.AllowUpdate = true;
				Transactions.Cache.AllowInsert = (doc.CustomerID != null) && (doc.CustomerLocationID != null) && (doc.ProjectID != null || !PM.ProjectAttribute.IsPMVisible(this, BatchModule.AR));

                if (Document.Current != null && Document.Current.SkipDiscounts == false)
                {
                    ARDiscountDetails.Cache.AllowDelete = true;
                    ARDiscountDetails.Cache.AllowUpdate = true;
                    ARDiscountDetails.Cache.AllowInsert = true;
                }
                else
                {
                    ARDiscountDetails.Cache.AllowDelete = false;
                    ARDiscountDetails.Cache.AllowUpdate = false;
                    ARDiscountDetails.Cache.AllowInsert = false;
                }

				Taxes.Cache.AllowUpdate = true;
				release.SetEnabled(doc.Hold != true && doc.Scheduled != true);
				createSchedule.SetEnabled(doc.Hold != true && (doc.DocType == ARDocType.Invoice));
				payInvoice.SetEnabled(false);
			}
			Billing_Address.Cache.AllowUpdate = !(doc.Printed == true || doc.Emailed == true);
			Billing_Contact.Cache.AllowUpdate = doc.Emailed == false;
			
			PXUIFieldAttribute.SetEnabled<ARInvoice.docType>(cache, doc);
			PXUIFieldAttribute.SetEnabled<ARInvoice.refNbr>(cache, doc);

			Taxes.Cache.AllowDelete = Transactions.Cache.AllowDelete && !IsExternalTax;
			Taxes.Cache.AllowInsert = Transactions.Cache.AllowInsert && !IsExternalTax;
			Taxes.Cache.AllowUpdate = Taxes.Cache.AllowUpdate && !IsExternalTax;

			Adjustments.Cache.AllowInsert = false;
			Adjustments.Cache.AllowDelete = false;
			Adjustments.Cache.AllowUpdate = Transactions.Cache.AllowUpdate;

			if (doc == null || customer.Current == null)
			{
				editCustomer.SetEnabled(false);
			}
			else
			{
				editCustomer.SetEnabled(true);
			}
			reverseInvoice.SetEnabled(doc != null && ((doc.Released ?? false) == true));

			//aRInvoiceMemo.SetEnabled(doc != null && 
			//	((doc.Released ?? false) == true || 
			//	 (this.ARSetup.Current.PrintBeforeRelease ?? false)));

			//sendARInvoiceMemo.SetEnabled(doc != null &&
			//	((doc.Released ?? false) == true ||
			//	 (this.ARSetup.Current.EmailBeforeRelease ?? false)));

			if (doc.CustomerID != null)
			{
				if (Transactions.Select().Count > 0)
				{
					PXUIFieldAttribute.SetEnabled<ARInvoice.customerID>(cache, doc, false);
				}
			}

			if (ARSetup.Current != null)
			{
				PXUIFieldAttribute.SetVisible<ARInvoice.curyOrigDocAmt>(cache, e.Row, (bool)ARSetup.Current.RequireControlTotal || e.Row != null && (bool)((ARInvoice)e.Row).Released);
			}

			PXUIFieldAttribute.SetEnabled<ARInvoice.curyCommnblAmt>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<ARInvoice.curyCommnAmt>(cache, doc, false);

			if (ARSetup.Current != null)
			{
				PXUIFieldAttribute.SetVisible<ARInvoice.commnPct>(cache, e.Row, false);
				if ((bool)doc.Released || (bool)doc.Voided)
				{
					this.salesPerTrans.Cache.AllowInsert = false;
					this.salesPerTrans.Cache.AllowDelete = false;

                    PXResult<ARSalesPerTran,ARSPCommissionPeriod> sptRes = ( PXResult<ARSalesPerTran,ARSPCommissionPeriod>) this.salesPerTrans.Select();
                    bool isCommnPeriodClosed = false;
                    if (sptRes != null) 
                    {
                        ARSPCommissionPeriod commnPeriod = (ARSPCommissionPeriod)sptRes;
                        if (!String.IsNullOrEmpty(commnPeriod.CommnPeriodID) && commnPeriod.Status == ARSPCommissionPeriodStatus.Closed) 
                        {
                            isCommnPeriodClosed = true;
                        }
                    }
                    this.salesPerTrans.Cache.AllowUpdate = !isCommnPeriodClosed;

					PXUIFieldAttribute.SetEnabled<ARInvoice.workgroupID>(cache, e.Row, false);
					PXUIFieldAttribute.SetEnabled<ARInvoice.ownerID>(cache, e.Row, false);
				}
				else
				{
					PXUIFieldAttribute.SetEnabled<ARInvoice.workgroupID>(cache, e.Row, true);
					PXUIFieldAttribute.SetEnabled<ARInvoice.ownerID>(cache, e.Row, true);
				}

			}

			PXUIFieldAttribute.SetVisible<ARTran.taskID>(Transactions.Cache, null, PM.ProjectAttribute.IsPMVisible(this, BatchModule.AR));
			if (IsExternalTax == true && ((ARInvoice)e.Row).IsTaxValid != true)
				PXUIFieldAttribute.SetWarning<ARInvoice.curyTaxTotal>(cache, e.Row, AR.Messages.TaxIsNotUptodate);

			ARAddress address = this.Billing_Address.Select();
			bool enableAddressValidation = doc.Released == false && (address != null && address.IsDefaultAddress == false && address.IsValidated == false);
			this.validateAddresses.SetEnabled(enableAddressValidation);

			PXUIFieldAttribute.SetVisible<ARInvoice.avalaraCustomerUsageType>(cache, null, AvalaraMaint.IsActive(this));
            CT.ContractBillingTrace cbt = PXSelect<CT.ContractBillingTrace, Where<CT.ContractBillingTrace.contractID, Equal<Required<CT.ContractBillingTrace.contractID>>,
                And<CT.ContractBillingTrace.docType, Equal<Required<CT.ContractBillingTrace.docType>>, And<CT.ContractBillingTrace.refNbr, Equal<Required<CT.ContractBillingTrace.refNbr>>>>>>.SelectWindowed(this, 0, 1, doc.ProjectID, doc.DocType, doc.RefNbr);
            if (cbt != null)
            {
                //this invoice was created as a result of Contract/Project billing. Changing Project/Contract for this Invoice is not allowed.
                PXUIFieldAttribute.SetEnabled<ARInvoice.projectID>(cache, doc, false);
            }
            if (PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>())
            {
                var controlsManager = new RUTROTControlsStateManager(this);
                controlsManager.Update();

                SetPersistingChecks(doc);

                var errorLevel = doc.Hold == true ? PXErrorLevel.Warning : PXErrorLevel.Error;

                WarnOnDeductionExceedsAllowance(doc, errorLevel);
                WarnUndistributedAmount(doc, errorLevel);
		    }
		}

		protected virtual void ARInvoice_Hold_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row != null && ((ARInvoice)e.Row).Hold != null)
			{
				if ((bool)((ARInvoice)e.Row).Hold )
				{
					if ((((ARInvoice)e.Row).Status != "H"))
						sender.SetValue<ARInvoice.status>(e.Row, "H");

					sender.SetDefaultExt<ARInvoice.printInvoice>(e.Row);					
					sender.SetDefaultExt<ARInvoice.emailInvoice>(e.Row);					
				}
				else if (!(bool)((ARInvoice)e.Row).Hold && ((ARInvoice)e.Row).Status == "H")
				{
					sender.SetValue<ARInvoice.status>(e.Row, "B");
				}
			}
		}
		
		protected virtual void ARInvoice_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARInvoice row = e.Row as ARInvoice;
			if (row != null)
			{
				foreach (ARTran tran in Transactions.Select())
				{
					tran.ProjectID = row.ProjectID;
					Transactions.Update(tran);
				}

			}
		}

        protected virtual void ARInvoice_IsRUTROTDeductible_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            ARInvoice row = e.Row as ARInvoice;
            if (row != null && PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>() && row.IsRUTROTDeductible == false
                 && !String.IsNullOrWhiteSpace(row.ROTEstateAppartment) && !String.IsNullOrWhiteSpace(row.ROTOrganizationNbr))
            {
                row.ROTEstateAppartment = null;
                row.ROTOrganizationNbr = null;
                Document.Update(row);
            }
        }

        protected virtual void ARInvoice_RUTROTType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            ARInvoice row = e.Row as ARInvoice;
            if (row != null && PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>() && row.IsRUTROTDeductible == true)
            {
                if (row.RUTROTType == RUTROTTypes.RUT
                    && !String.IsNullOrWhiteSpace(row.ROTEstateAppartment) && !String.IsNullOrWhiteSpace(row.ROTOrganizationNbr))
                {
                    row.ROTEstateAppartment = null;
                    row.ROTOrganizationNbr = null;
                    Document.Update(row);
			}
		}
		}

        bool isReverse = false;
        protected virtual void ARInvoice_ProjectID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (isReverse)
                e.Cancel = true;
        }

		protected virtual void ARInvoice_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			ARReleaseProcess.UpdateARBalances(this, (ARInvoice)e.Row, -(((ARInvoice)e.Row).OrigDocAmt));
		}

		protected virtual void ARInvoice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARInvoice doc = (ARInvoice)e.Row;
			if (doc.Released != true)
			{
                if (e.ExternalCall && (!sender.ObjectsEqual<ARInvoice.docDate>(e.OldRow, e.Row) || !sender.ObjectsEqual<ARInvoice.skipDiscounts>(e.OldRow, e.Row)))
                {
                    DiscountEngine<ARTran>.AutoRecalculatePricesAndDiscounts<ARInvoiceDiscountDetail>(Transactions.Cache, Transactions, null, ARDiscountDetails, doc.CustomerLocationID, doc.DocDate.Value);
                }

				if (IsExternalTax == true && !sender.ObjectsEqual<ARInvoice.avalaraCustomerUsageType>(e.Row, e.OldRow))
				{
					doc.IsTaxValid = false;
					if (Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Notchanged)
						Document.Cache.SetStatus(Document.Current, PXEntryStatus.Updated);
				}

                if (sender.GetStatus(doc) != PXEntryStatus.Deleted && !sender.ObjectsEqual<ARInvoice.curyDiscTot>(e.OldRow, e.Row))
                {
                    AddDiscount(sender, doc);
                }
                
				if (ARSetup.Current.RequireControlTotal != true)
				{
					if (doc.CuryDocBal != doc.CuryOrigDocAmt)
					{
						if (doc.CuryDocBal != null && doc.CuryDocBal != 0)
							sender.SetValueExt<ARInvoice.curyOrigDocAmt>(e.Row, doc.CuryDocBal);
						else
							sender.SetValueExt<ARInvoice.curyOrigDocAmt>(e.Row, 0m);
					}
				}
				if (doc.Hold != true)
				{
					if (doc.CuryDocBal != doc.CuryOrigDocAmt)
					{
						sender.RaiseExceptionHandling<ARInvoice.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, new PXSetPropertyException(Messages.DocumentOutOfBalance));
					}
					else if (doc.CuryOrigDocAmt < 0m)
					{
						if (ARSetup.Current.RequireControlTotal == true)
						{
							sender.RaiseExceptionHandling<ARInvoice.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
						}
						else
						{
							sender.RaiseExceptionHandling<ARInvoice.curyDocBal>(doc, doc.CuryDocBal, new PXSetPropertyException(Messages.DocumentBalanceNegative));
						}
					}
					else
					{
						if (ARSetup.Current.RequireControlTotal == true)
						{
							sender.RaiseExceptionHandling<ARInvoice.curyOrigDocAmt>(doc, null, null);
						}
						else
						{
							sender.RaiseExceptionHandling<ARInvoice.curyDocBal>(doc, null, null);
						}
					}
				}
                if (doc != null && doc.CustomerID != null && doc.CuryDiscTot != null && doc.CuryDiscTot > 0 && doc.CuryLineTotal != null && doc.CuryLineTotal > 0)
                {
                    decimal discountLimit = DiscountEngine.GetDiscountLimit(sender, doc.CustomerID);
                    if ((doc.CuryLineTotal / 100 * discountLimit) < doc.CuryDiscTot)
                        PXUIFieldAttribute.SetWarning<ARInvoice.curyDiscTot>(sender, doc, string.Format(AR.Messages.DocDiscountExceedLimit, discountLimit));
			}
			}
			if (e.OldRow != null)
			{
				ARReleaseProcess.UpdateARBalances(this, (ARInvoice)e.OldRow, -(((ARInvoice)e.OldRow).OrigDocAmt));
			}
            if (PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>())
                RedistributeDeduction();
			ARReleaseProcess.UpdateARBalances(this, (ARInvoice)e.Row, (((ARInvoice)e.Row).OrigDocAmt));			
		}

		protected virtual void ARInvoice_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			ARReleaseProcess.UpdateARBalances(this, (ARInvoice)e.Row, (((ARInvoice)e.Row).OrigDocAmt));
		}
		#endregion

		#region ARTran events
		protected virtual void ARTran_AccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARTran tran = (ARTran)e.Row;

			if (tran != null && tran.InventoryID == null && location.Current != null)
			{
				e.NewValue = location.Current.CSalesAcctID;
				e.Cancel = true;
			}
		}

		protected virtual void ARTran_AccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARTran row = e.Row as ARTran;
            if (row != null && row.ProjectID != null && !PM.ProjectDefaultAttribute.IsNonProject(this, row.ProjectID) && row.TaskID != null)
			{
				Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, e.NewValue);
				if (account != null && account.AccountGroupID == null)
				{
					sender.RaiseExceptionHandling<ARTran.accountID>(e.Row, account.AccountCD, new PXSetPropertyException(PM.Messages.NoAccountGroup, PXErrorLevel.Warning, account.AccountCD));
				}
			}
		}
		
		protected virtual void ARTran_AccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARTran row = e.Row as ARTran;
			if (row != null && row.TaskID == null)
			{
				sender.SetDefaultExt<ARTran.taskID>(e.Row);
			}
		}

        protected virtual void ARTran_SubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARTran tran = (ARTran)e.Row;
			if (tran != null && tran.AccountID != null && location.Current != null)
			{
                InventoryItem item = InventoryItemGetByID(tran.InventoryID);
                EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<ARTran.employeeID>>>>.SelectSingleBound(this, new object[] { e.Row });
                
				Location companyloc =
					(Location)PXSelectJoin<Location, InnerJoin<BAccountR, On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>, InnerJoin<GL.Branch, On<BAccountR.bAccountID, Equal<GL.Branch.bAccountID>>>>, Where<GL.Branch.branchID, Equal<Required<ARTran.branchID>>>>.Select(this, tran.BranchID);
				SalesPerson salesperson = (SalesPerson)PXSelect<SalesPerson, Where<SalesPerson.salesPersonID, Equal<Current<ARTran.salesPersonID>>>>.SelectSingleBound(this, new object[] { e.Row });

				int? customer_SubID = (int?)Caches[typeof(Location)].GetValue<Location.cSalesSubID>(location.Current);
				int? item_SubID = (int?)Caches[typeof(InventoryItem)].GetValue<InventoryItem.salesSubID>(item);
				int? employee_SubID = (int?)Caches[typeof(EPEmployee)].GetValue<EPEmployee.salesSubID>(employee);
				int? company_SubID = (int?)Caches[typeof(Location)].GetValue<Location.cMPSalesSubID>(companyloc);
				int? salesperson_SubID = (int?)Caches[typeof(SalesPerson)].GetValue<SalesPerson.salesSubID>(salesperson);

				object value;
				try
				{
					value = SubAccountMaskAttribute.MakeSub<ARSetup.salesSubMask>(this, SalesSubMask,
						new object[] { customer_SubID, item_SubID, employee_SubID, company_SubID, salesperson_SubID },
						new Type[] { typeof(Location.cSalesSubID), typeof(InventoryItem.salesSubID), typeof(EPEmployee.salesSubID), typeof(Location.cMPSalesSubID), typeof(SalesPerson.salesSubID) });

					sender.RaiseFieldUpdating<ARTran.subID>(e.Row, ref value);
				}
				catch (PXException)
				{
					value = null;
				}

				e.NewValue = (int?)value;
				e.Cancel = true;
			}
		}

        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Category")]
        [ARTax(typeof(ARInvoice), typeof(ARTax), typeof(ARTaxTran))]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
        [PXDefault(typeof(Search<InventoryItem.taxCategoryID,
            Where<InventoryItem.inventoryID, Equal<Current<ARTran.inventoryID>>>>),
            PersistingCheck = PXPersistingCheck.Nothing, SearchOnDefault = false)]
        protected virtual void ARTran_TaxCategoryID_CacheAttached(PXCache sender)
        { 
        }

		protected virtual void ARTran_TaxCategoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (TaxAttribute.GetTaxCalc<ARTran.taxCategoryID>(sender, e.Row) == TaxCalc.Calc && taxzone.Current != null && !string.IsNullOrEmpty(taxzone.Current.DfltTaxCategoryID) && ((ARTran)e.Row).InventoryID == null)
			{
				e.NewValue = taxzone.Current.DfltTaxCategoryID;
			}
		}

		protected virtual void ARTran_UnitPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (((ARTran)e.Row).InventoryID == null)
			{
				e.NewValue = 0m;
			}
		}

		protected virtual void ARTran_CuryUnitPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARTran row = e.Row as ARTran;
			if (row == null || row.InventoryID == null || row.UOM == null)
			{
				e.NewValue = sender.GetValue<ARTran.curyUnitPrice>(e.Row);
				e.Cancel = e.NewValue != null;
				return;
			}

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
                e.NewValue = ARSalesPriceMaint.CalculateSalesPrice(sender, customerPriceClass, ((ARTran)e.Row).CustomerID, ((ARTran)e.Row).InventoryID, currencyinfo.Select(), ((ARTran)e.Row).UOM, ((ARTran)e.Row).Qty, date, row.CuryUnitPrice) ?? 0m;
				}
			}

		protected virtual void ARTran_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARTran.curyUnitPrice>(e.Row);
		}

		protected virtual void ARTran_OrigInvoiceDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARTran.curyUnitPrice>(e.Row);
		}

		protected virtual void ARTran_Qty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARTran row = e.Row as ARTran;
			if (row != null)
			{
                if (row.Qty == 0)
                {
                    sender.SetValueExt<ARTran.curyDiscAmt>(row, decimal.Zero);
                    sender.SetValueExt<ARTran.discPct>(row, decimal.Zero);
			}
                else
                {
                    sender.SetDefaultExt<ARTran.curyUnitPrice>(e.Row);
		}
            }
		}

		protected virtual void ARTran_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!e.ExternalCall)
			{
				e.Cancel = true;
			}
		}

		protected virtual void ARTran_SOShipmentNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void ARTran_SalesPersonID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARTran.subID>(e.Row);
		}

        protected virtual void ARTran_EmployeeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            sender.SetDefaultExt<ARTran.subID>(e.Row);
        }

		protected virtual void ARTran_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARTran.accountID>(e.Row);
			try
			{
				sender.SetDefaultExt<ARTran.subID>(e.Row);
			}
			catch (PXSetPropertyException)
			{
				sender.SetValue<ARTran.subID>(e.Row, null);
			}
			sender.SetDefaultExt<ARTran.taxCategoryID>(e.Row);
			sender.SetDefaultExt<ARTran.deferredCode>(e.Row);
			sender.SetDefaultExt<ARTran.uOM>(e.Row);

			//sender.SetDefaultExt<ARTran.unitPrice>(e.Row);
			sender.SetDefaultExt<ARTran.curyUnitPrice>(e.Row);


			ARTran tran = e.Row as ARTran;
			IN.InventoryItem item = PXSelectorAttribute.Select<IN.InventoryItem.inventoryID>(sender, tran) as IN.InventoryItem;
			if (item != null && tran != null)
			{
				tran.TranDesc = item.Descr;
			}

			SetDiscounts(sender, (ARTran)e.Row);

            if(PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>())
                UpdateTranDeductibleFromInventory(tran);
		}

        protected virtual void ARTran_SubID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>())
                UpdateTranDeductibleFromInventory(e.Row as ARTran);
        }

		protected virtual void ARTran_DefScheduleID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRSchedule sc = PXSelect<DRSchedule, Where<DRSchedule.scheduleID, Equal<Required<DRSchedule.scheduleID>>>>.Select(this, ((ARTran)e.Row).DefScheduleID);
			if (sc != null)
			{
				ARTran defertran = PXSelect<ARTran, Where<ARTran.tranType, Equal<Required<ARTran.tranType>>,
					And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
					And<ARTran.lineNbr, Equal<Required<ARTran.lineNbr>>>>>>.Select(this, sc.DocType, sc.RefNbr, sc.LineNbr);

				if (defertran != null)
				{
					((ARTran)e.Row).DeferredCode = defertran.DeferredCode;
				}
			}
		}

        protected virtual void ARTran_DiscountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            ARTran row = e.Row as ARTran;
            if (e.ExternalCall && row != null)
            {
                DiscountEngine<ARTran>.UpdateManualLineDiscount<ARInvoiceDiscountDetail>(sender, Transactions, row, ARDiscountDetails, Document.Current.CustomerLocationID, Document.Current.DocDate.Value, Document.Current.SkipDiscounts);
            }
        }

		protected virtual void ARTran_DeferredCode_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARTran row = e.Row as ARTran;
			if (row != null)
			{
				if (row.TranType == ARDocType.CreditMemo)
				{
					DRDeferredCode dc = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Required<DRDeferredCode.deferredCodeID>>>>.Select(this, e.NewValue);

					if (dc != null && dc.Method == DeferredMethodType.CashReceipt)
					{
						e.Cancel = true;
						if (sender.RaiseExceptionHandling<ARTran.deferredCode>(e.Row, e.NewValue, new PXSetPropertyException(Messages.InvalidCashReceiptDeferredCode)))
						{
							throw new PXSetPropertyException(Messages.InvalidCashReceiptDeferredCode);
						}
					}
				}
			}
		}
        
		protected virtual void ARTran_DiscPct_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARTran row = e.Row as ARTran;
			decimal? newValue = (decimal?)e.NewValue;
			SODiscountEngine<ARTran>.ValidateMinGrossProfitPct(sender, row, row.UnitPrice, ref newValue);
			e.NewValue = newValue;
		}

		protected virtual void ARTran_CuryDiscAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARTran row = e.Row as ARTran;
			decimal? newValue = (decimal?)e.NewValue;
			SODiscountEngine<ARTran>.ValidateMinGrossProfitAmt(sender, row, row.UnitPrice, ref newValue);
			e.NewValue = newValue;
		}
        
		protected virtual void ARTran_CuryUnitPrice_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARTran row = e.Row as ARTran;
			if (row != null)
			{
				decimal? newValue = (decimal?)e.NewValue;
				SODiscountEngine<ARTran>.ValidateMinGrossProfitUnitPrice(sender, row, ref newValue);
				e.NewValue = newValue;
			}
		}

		protected virtual void ARTran_CuryUnitPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARTran row = e.Row as ARTran;
			if (row != null && row.ManualDisc == true)
			{
				row.CuryDiscAmt = (row.CuryUnitPrice ?? 0) * (row.Qty ?? 0) * (row.DiscPct ?? 0) * 0.01m;
				row.CuryTranAmt = (row.CuryUnitPrice ?? 0) * (row.Qty ?? 0) - row.CuryDiscAmt;
			}
		}

		protected virtual void ARTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARTran row = (ARTran)e.Row;
			ARTran oldRow = (ARTran)e.OldRow;
			if (row != null)
			{
                if (!sender.ObjectsEqual<ARTran.branchID>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.inventoryID>(e.Row, e.OldRow) ||
                    !sender.ObjectsEqual<ARTran.qty>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.curyUnitPrice>(e.Row, e.OldRow) ||
                    !sender.ObjectsEqual<ARTran.curyExtPrice>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.curyDiscAmt>(e.Row, e.OldRow) ||
                    !sender.ObjectsEqual<ARTran.discPct>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.manualDisc>(e.Row, e.OldRow) ||
                    !sender.ObjectsEqual<ARTran.discountID>(e.Row, e.OldRow))
					RecalculateDiscounts(sender, row);

				TaxAttribute.Calculate<ARTran.taxCategoryID, ARTaxAttribute>(sender, e);

				//Validate that Sales Account <> Deferral Account:
				if (!sender.ObjectsEqual<ARTran.accountID, ARTran.deferredCode>(e.Row, e.OldRow))
				{
					if (!string.IsNullOrEmpty(row.DeferredCode))
					{
						DRDeferredCode defCode = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Required<DRDeferredCode.deferredCodeID>>>>.Select(this, row.DeferredCode);
						if (defCode != null)
						{
							if (defCode.AccountID == row.AccountID)
							{
								sender.RaiseExceptionHandling<ARTran.accountID>(e.Row, row.AccountID,
									new PXSetPropertyException(Messages.AccountIsSameAsDeferred, PXErrorLevel.Warning));
							}
						}
					}
				}

				//if any of the fields that was saved in avalara has changed mark doc as TaxInvalid.
				if (Document.Current != null && IsExternalTax == true &&
					!sender.ObjectsEqual<ARTran.accountID, ARTran.inventoryID, ARTran.tranDesc, ARTran.tranAmt, ARTran.tranDate, ARTran.taxCategoryID>(e.Row, e.OldRow))
				{
					Document.Current.IsTaxValid = false;
					if (Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Notchanged)
						Document.Cache.SetStatus(Document.Current, PXEntryStatus.Updated);
				}
			}
		}

		protected virtual void ARTran_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			TaxAttribute.Calculate<ARTran.taxCategoryID, ARTaxAttribute>(sender, e);

			if (Document.Current != null && IsExternalTax == true)
			{
				Document.Current.IsTaxValid = false;
				if (Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Notchanged)
					Document.Cache.SetStatus(Document.Current, PXEntryStatus.Updated);
			}
		}

		protected virtual void ARTran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			ARTran row = (ARTran)e.Row;

			if (row != null)
			{
				PM.PMTran reference = RefPMTran.Select(row.PMTranID);
				if (reference != null)
				{
					reference.Billed = false;
					RefPMTran.Update(reference);
				}

				foreach (PM.PMTran pMRef in RefContractUsageTran.Select(row.TranType, row.RefNbr, row.LineNbr))
				{
					if (pMRef != null)
					{
						pMRef.ARRefNbr = null;
						pMRef.ARTranType = null;
						pMRef.RefLineNbr = null;
						pMRef.Billed = false;
						RefContractUsageTran.Update(pMRef);
					}
				}
			}

			if (Document.Current != null)
			{
				Document.Current.IsTaxValid = false;
				if ( Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Notchanged)
					Document.Cache.SetStatus(Document.Current, PXEntryStatus.Updated);
			}

            if (Document.Current != null && Document.Cache.GetStatus(Document.Current) != PXEntryStatus.Deleted)
            {
                if (row.LineType != SOLineType.Discount)
                    DiscountEngine<ARTran>.RecalculateGroupAndDocumentDiscounts(sender, Transactions, ARDiscountDetails, Document.Current.CustomerLocationID, Document.Current.DocDate.Value, true);
                RecalculateTotalDiscount();
            }
		}

		protected virtual void ARTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARTran row = e.Row as ARTran;
			if (row != null)
			{
                PXUIFieldAttribute.SetEnabled<ARTran.defScheduleID>(sender, row, row.TranType == ARInvoiceType.CreditMemo || row.TranType == ARInvoiceType.DebitMemo );
				PXUIFieldAttribute.SetEnabled<ARTran.deferredCode>(sender, row, row.DefScheduleID == null);
				
				viewSchedule.SetEnabled( sender.GetStatus(row) != PXEntryStatus.Inserted );
			}
		}
        
        protected virtual void ARTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            ARTran row = e.Row as ARTran;
            if (row != null && row.PMTranID != null && row.TranType != ARDocType.CreditMemo)
            {

                PM.PMTran pmTran = PXSelect<PM.PMTran, Where<PM.PMTran.tranID, Equal<Required<PM.PMTran.tranID>>>>.Select(this, row.PMTranID);

	            if (row.PMDeltaOption == null)
	            {
                if (pmTran.Amount > row.TranAmt)
                {
					bool raiseError = true;

			            PX.Objects.PM.PMAccountGroup ag =
				            PXSelect
					            <PX.Objects.PM.PMAccountGroup,
						            Where<PX.Objects.PM.PMAccountGroup.groupID, Equal<Required<PX.Objects.PM.PMAccountGroup.groupID>>>>
					            .Select(this, pmTran.AccountGroupID);
					if (ag.Type == GL.AccountType.Liability || ag.Type == GL.AccountType.Income)
					{
						if (-pmTran.Amount <= row.TranAmt)
							raiseError = false;
					}


					if (raiseError)
					{
				            if (sender.RaiseExceptionHandling(typeof (ARTran.pMDeltaOption).Name, row, null,
				                                              new PXSetPropertyException(Messages.PMDeltaOptionRequired,
				                                                                         PXErrorLevel.Error)))
						{
					            throw new PXRowPersistingException(typeof (ARTran.pMDeltaOption).Name, null,
					                                               Messages.PMDeltaOptionRequired);
				            }
			            }
		            }
						}
	            else if (row.TranAmt == 0 && row.PMDeltaOption != ARTran.pMDeltaOption.CompleteLine && ARSetup.Current.ZeroPost != true)
	            {
					if (sender.RaiseExceptionHandling(typeof(ARTran.pMDeltaOption).Name, row, row.PMDeltaOption, new PXSetPropertyException(Messages.PMDeltaOptionNotValid, PXErrorLevel.Error)))
					{
						throw new PXRowPersistingException(typeof(ARTran.pMDeltaOption).Name, row.PMDeltaOption, Messages.PMDeltaOptionNotValid);
					}
                }
            }
        }


		#endregion

		#region ARTaxTran Events
		protected virtual void ARTaxTran_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null)
			{
				e.NewValue = Document.Current.TaxZoneID;
				e.Cancel = true;
			}
		}

		protected virtual void ARTaxTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (Document.Current != null && (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update))
			{
				((ARTaxTran)e.Row).TaxZoneID = Document.Current.TaxZoneID;
			}
		}
		#endregion

		#region ARSalesPerTran events

		protected virtual void ARSalesPerTran_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			ARSalesPerTran row = (ARSalesPerTran)e.Row;
			foreach (ARSalesPerTran iSpt in this.salesPerTrans.Select())
			{
				if (iSpt.SalespersonID == row.SalespersonID)
				{
					PXEntryStatus status = this.salesPerTrans.Cache.GetStatus(iSpt);
					if (!(status == PXEntryStatus.InsertedDeleted || status == PXEntryStatus.Deleted))
					{
						sender.RaiseExceptionHandling<ARSalesPerTran.salespersonID>(e.Row, null, new PXException(Messages.ERR_DuplicatedSalesPersonAdded));
						e.Cancel = true;
						break;
					}
				}
			}
		}
		#endregion

		#region ARAdjust Events
		protected virtual void ARAdjust_CuryAdjdAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARAdjust adj = (ARAdjust)e.Row;
			Terms terms = PXSelect<Terms, Where<Terms.termsID, Equal<Current<ARInvoice.termsID>>>>.Select(this);

			if (terms != null && terms.InstallmentType != TermsInstallmentType.Single && (decimal)e.NewValue > 0m)
			{
				throw new PXSetPropertyException(Messages.PrepaymentAppliedToMultiplyInstallments);
			}

			if (adj.CuryDocBal == null)
			{
				PXResult<ARPayment, CurrencyInfo> res = (PXResult<ARPayment, CurrencyInfo>)PXSelectReadonly2<ARPayment, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARPayment.curyInfoID>>>, Where<ARPayment.docType, Equal<Required<ARPayment.docType>>, And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>.Select(this, adj.AdjgDocType, adj.AdjgRefNbr);

				ARPayment payment = res;
				CurrencyInfo pay_info = res;
				CurrencyInfo inv_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<ARInvoice.curyInfoID>>>>.Select(this);

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
				throw new PXSetPropertyException(Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjdAmt).ToString());
			}
		}

		protected virtual void ARAdjust_CuryAdjdAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARAdjust adj = (ARAdjust)e.Row;
			decimal CuryAdjgAmt;
			decimal AdjdAmt;
			decimal AdjgAmt;

			PXDBCurrencyAttribute.CuryConvBase<ARAdjust.adjdCuryInfoID>(sender, e.Row, (decimal)adj.CuryAdjdAmt, out AdjdAmt);

			CurrencyInfo pay_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<ARAdjust.adjgCuryInfoID>>>>.SelectSingleBound(this, new object[] { adj });
			CurrencyInfo inv_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<ARAdjust.adjdCuryInfoID>>>>.SelectSingleBound(this, new object[] { adj });

			if (string.Equals(pay_info.CuryID, inv_info.CuryID))
			{
				CuryAdjgAmt = (decimal)adj.CuryAdjdAmt;
			}
			else
			{
				PXDBCurrencyAttribute.CuryConvCury<ARAdjust.adjgCuryInfoID>(sender, e.Row, AdjdAmt, out CuryAdjgAmt);
			}

			if (object.Equals(pay_info.CuryID, inv_info.CuryID) && object.Equals(pay_info.CuryRate, inv_info.CuryRate) && object.Equals(pay_info.CuryMultDiv, inv_info.CuryMultDiv))
			{
				AdjgAmt = AdjdAmt;
			}
			else
			{
				PXDBCurrencyAttribute.CuryConvBase<ARAdjust.adjgCuryInfoID>(sender, e.Row, CuryAdjgAmt, out AdjgAmt);
			}

			adj.CuryAdjgAmt = CuryAdjgAmt;
			adj.AdjAmt = AdjdAmt;
			adj.RGOLAmt = AdjgAmt - AdjdAmt;
			adj.CuryDocBal = adj.CuryDocBal + (decimal?)e.OldValue - adj.CuryAdjdAmt;
		}

		protected virtual void ARAdjust_Hold_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = true;
			e.Cancel = true;
		}
		#endregion

        #region ARInvoiceDiscountDetail events

        protected virtual void ARInvoiceDiscountDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            ARInvoiceDiscountDetail discountDetail = (ARInvoiceDiscountDetail)e.Row;
            if (e.ExternalCall == true && discountDetail != null && discountDetail.DiscountID != null)
            {
                discountDetail.IsManual = true;
                DiscountEngine<ARTran>.InsertDocGroupDiscount<ARInvoiceDiscountDetail>(Transactions.Cache, Transactions, ARDiscountDetails, discountDetail, discountDetail.DiscountID, null, Document.Current.CustomerLocationID, Document.Current.DocDate.Value);
                RecalculateTotalDiscount();
            }
        }
        
        protected virtual void ARInvoiceDiscountDetail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            ARInvoiceDiscountDetail discountDetail = (ARInvoiceDiscountDetail)e.Row;
            if (e.ExternalCall == true && discountDetail != null && discountDetail.Type != DiscountType.Document)
            {
                DiscountEngine<ARTran>.UpdateDocumentDiscount<ARInvoiceDiscountDetail>(Transactions.Cache, Transactions, ARDiscountDetails, Document.Current.CustomerLocationID, Document.Current.DocDate.Value, Document.Current.SkipDiscounts);
            }
            RecalculateTotalDiscount();
        }

		#endregion
				
		#region ARAdjust       
        [Serializable]
		public partial class ARAdjust : PX.Objects.AR.ARAdjust
		{
			#region CustomerID
			public new abstract class customerID : PX.Data.IBqlField
			{
			}
			[PXDBInt]
			[PXDBDefault(typeof(AR.ARInvoice.customerID))]
			[PXUIField(DisplayName = "CustomerID", Visibility = PXUIVisibility.Visible, Visible = false)]
			public override Int32? CustomerID
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
			#region AdjgDocType
			public new abstract class adjgDocType : PX.Data.IBqlField
			{
			}
			[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
			[ARPaymentType.List()]
			[PXDefault()]
			[PXUIField(DisplayName = "Doc. Type", Enabled = false)]
			public override String AdjgDocType
			{
				get
				{
					return this._AdjgDocType;
				}
				set
				{
					this._AdjgDocType = value;
				}
			}
			#endregion
			#region AdjgRefNbr
			public new abstract class adjgRefNbr : PX.Data.IBqlField
			{
			}
			[PXDBString(15, IsUnicode = true, IsKey = true)]
			[PXDefault()]
			[PXUIField(DisplayName = "Reference Nbr.", Enabled = false)]
			[ARPaymentType.AdjgRefNbr(typeof(Search<ARPayment.refNbr, Where<ARPayment.docType, Equal<Optional<ARAdjust.adjgDocType>>>>), Filterable = true)]
			public override String AdjgRefNbr
			{
				get
				{
					return this._AdjgRefNbr;
				}
				set
				{
					this._AdjgRefNbr = value;
				}
			}
			#endregion
			#region AdjgBranchID
			public new abstract class adjgBranchID : PX.Data.IBqlField
			{
			}
			[Branch(null)]
			public override Int32? AdjgBranchID
			{
				get
				{
					return this._AdjgBranchID;
				}
				set
				{
					this._AdjgBranchID = value;
				}
			}
			#endregion
			#region AdjdCuryInfoID
			public new abstract class adjdCuryInfoID : PX.Data.IBqlField
			{
			}
			[PXDBLong()]
			[CurrencyInfo(typeof(ARInvoice.curyInfoID), ModuleCode = "AR", CuryIDField = "AdjdCuryID")]
			public override Int64? AdjdCuryInfoID
			{
				get
				{
					return this._AdjdCuryInfoID;
				}
				set
				{
					this._AdjdCuryInfoID = value;
				}
			}
			#endregion
			#region AdjdDocType
			public new abstract class adjdDocType : PX.Data.IBqlField
			{
			}
			[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
			[PXDBDefault(typeof(AR.ARInvoice.docType))]
			[PXUIField(DisplayName = "Document Type", Visibility = PXUIVisibility.Invisible, Visible = false)]
			public override String AdjdDocType
			{
				get
				{
					return this._AdjdDocType;
				}
				set
				{
					this._AdjdDocType = value;
				}
			}
			#endregion
			#region AdjdRefNbr
			public new abstract class adjdRefNbr : PX.Data.IBqlField
			{
			}
			[PXDBString(15, IsUnicode = true, IsKey = true)]
			[PXDBDefault(typeof(AR.ARInvoice.refNbr))]
			[PXParent(typeof(Select<AR.ARInvoice, Where<AR.ARInvoice.docType, Equal<Current<ARAdjust.adjdDocType>>, And<AR.ARInvoice.refNbr, Equal<Current<ARAdjust.adjdRefNbr>>>>>))]
			[PXParent(typeof(Select<SOInvoice, Where<SOInvoice.docType, Equal<Current<ARAdjust.adjdDocType>>, And<SOInvoice.refNbr, Equal<Current<ARAdjust.adjdRefNbr>>>>>))]
			[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Invisible, Visible = false)]
			public override String AdjdRefNbr
			{
				get
				{
					return this._AdjdRefNbr;
				}
				set
				{
					this._AdjdRefNbr = value;
				}
			}
			#endregion
			#region AdjdBranchID
			public new abstract class adjdBranchID : PX.Data.IBqlField
			{
			}
			[Branch(typeof(AR.ARInvoice.branchID))]
			public override Int32? AdjdBranchID
			{
				get
				{
					return this._AdjdBranchID;
				}
				set
				{
					this._AdjdBranchID = value;
				}
			}
			#endregion
			#region AdjNbr
			public new abstract class adjNbr : PX.Data.IBqlField
			{
			}
			[PXDBInt(IsKey = true)]
			[PXUIField(DisplayName = "Adjustment Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
			[PXDefault()]
			public override Int32? AdjNbr
			{
				get
				{
					return this._AdjNbr;
				}
				set
				{
					this._AdjNbr = value;
				}
			}
			#endregion
			#region AdjBatchNbr
			public new abstract class adjBatchNbr : PX.Data.IBqlField
			{
			}
			[PXDBString(15, IsUnicode = true)]
			[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
			public override String AdjBatchNbr
			{
				get
				{
					return this._AdjBatchNbr;
				}
				set
				{
					this._AdjBatchNbr = value;
				}
			}
			#endregion
			#region AdjdOrigCuryInfoID
			public new abstract class adjdOrigCuryInfoID : PX.Data.IBqlField
			{
			}
			[PXDBLong()]
			[PXDBDefault(typeof(ARInvoice.curyInfoID))]
			public override Int64? AdjdOrigCuryInfoID
			{
				get
				{
					return this._AdjdOrigCuryInfoID;
				}
				set
				{
					this._AdjdOrigCuryInfoID = value;
				}
			}
			#endregion
			#region AdjgCuryInfoID
			public new abstract class adjgCuryInfoID : PX.Data.IBqlField
			{
			}
			[PXDBLong()]
			[CurrencyInfo(ModuleCode = "AR", CuryIDField = "AdjgCuryID")]
			public override Int64? AdjgCuryInfoID
			{
				get
				{
					return this._AdjgCuryInfoID;
				}
				set
				{
					this._AdjgCuryInfoID = value;
				}
			}
			#endregion
			#region AdjgDocDate
			public new abstract class adjgDocDate : PX.Data.IBqlField
			{
			}
			[PXDBDate()]
			[PXDBDefault(typeof(AR.ARInvoice.docDate))]
			public override DateTime? AdjgDocDate
			{
				get
				{
					return this._AdjgDocDate;
				}
				set
				{
					this._AdjgDocDate = value;
				}
			}
			#endregion
			#region AdjgFinPeriodID
			public new abstract class adjgFinPeriodID : PX.Data.IBqlField
			{
			}
			[GL.FinPeriodID()]
			[PXDBDefault(typeof(AR.ARInvoice.finPeriodID))]
			public override String AdjgFinPeriodID
			{
				get
				{
					return this._AdjgFinPeriodID;
				}
				set
				{
					this._AdjgFinPeriodID = value;
				}
			}
			#endregion
			#region AdjgTranPeriodID
			public new abstract class adjgTranPeriodID : PX.Data.IBqlField
			{
			}
			[GL.FinPeriodID()]
			[PXDBDefault(typeof(ARInvoice.tranPeriodID))]
			public override String AdjgTranPeriodID
			{
				get
				{
					return this._AdjgTranPeriodID;
				}
				set
				{
					this._AdjgTranPeriodID = value;
				}
			}
			#endregion
			#region AdjdDocDate
			public new abstract class adjdDocDate : PX.Data.IBqlField
			{
			}
			[PXDBDate()]
			[PXDBDefault(typeof(AR.ARInvoice.docDate))]
			[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public override DateTime? AdjdDocDate
			{
				get
				{
					return this._AdjdDocDate;
				}
				set
				{
					this._AdjdDocDate = value;
				}
			}
			#endregion
			#region AdjdFinPeriodID
			public new abstract class adjdFinPeriodID : PX.Data.IBqlField
			{
			}
			[FinPeriodID(typeof(ARAdjust.adjdDocDate))]
			[PXDBDefault(typeof(AR.ARInvoice.finPeriodID))]
			[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
			public override String AdjdFinPeriodID
			{
				get
				{
					return this._AdjdFinPeriodID;
				}
				set
				{
					this._AdjdFinPeriodID = value;
				}
			}
			#endregion
			#region AdjdTranPeriodID
			public new abstract class adjdTranPeriodID : PX.Data.IBqlField
			{
			}
			[FinPeriodID(typeof(ARAdjust.adjdDocDate))]
			[PXDBDefault(typeof(ARInvoice.tranPeriodID))]
			public override String AdjdTranPeriodID
			{
				get
				{
					return this._AdjdTranPeriodID;
				}
				set
				{
					this._AdjdTranPeriodID = value;
				}
			}
			#endregion
			#region CuryAdjdDiscAmt
			public new abstract class curyAdjdDiscAmt : PX.Data.IBqlField
			{
			}
			[PXDBCurrency(typeof(ARAdjust.adjdCuryInfoID), typeof(ARAdjust.adjDiscAmt))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Cash Discount Taken", Visibility = PXUIVisibility.Visible)]
			public override Decimal? CuryAdjdDiscAmt
			{
				get
				{
					return this._CuryAdjdDiscAmt;
				}
				set
				{
					this._CuryAdjdDiscAmt = value;
				}
			}
			#endregion
			#region CuryAdjdAmt
			public new abstract class curyAdjdAmt : PX.Data.IBqlField
			{
			}
			[PXDBCurrency(typeof(ARAdjust.adjdCuryInfoID), typeof(ARAdjust.adjAmt))]
			[PXUIField(DisplayName = "Amount Paid", Visibility = PXUIVisibility.Visible)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXFormula(null, typeof(SumCalc<SOInvoice.curyPaymentTotal>))]
			[PXFormula(null, typeof(SumCalc<SOAdjust.curyAdjdBilledAmt>))]
			public override Decimal? CuryAdjdAmt
			{
				get
				{
					return this._CuryAdjdAmt;
				}
				set
				{
					this._CuryAdjdAmt = value;
				}
			}
			#endregion
			#region AdjAmt
			public new abstract class adjAmt : PX.Data.IBqlField
			{
			}
			[PXDBDecimal(4)]
			[PXFormula(null, typeof(SumCalc<SOAdjust.adjBilledAmt>))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public override Decimal? AdjAmt
			{
				get
				{
					return this._AdjAmt;
				}
				set
				{
					this._AdjAmt = value;
				}
			}
			#endregion
			#region AdjDiscAmt
			public new abstract class adjDiscAmt : PX.Data.IBqlField
			{
			}
			[PXDBDecimal(4)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public override Decimal? AdjDiscAmt
			{
				get
				{
					return this._AdjDiscAmt;
				}
				set
				{
					this._AdjDiscAmt = value;
				}
			}
			#endregion
			#region CuryAdjgDiscAmt
			public new abstract class curyAdjgDiscAmt : PX.Data.IBqlField
			{
			}
			[PXDBDecimal(4)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public override Decimal? CuryAdjgDiscAmt
			{
				get
				{
					return this._CuryAdjgDiscAmt;
				}
				set
				{
					this._CuryAdjgDiscAmt = value;
				}
			}
			#endregion
			#region CuryAdjgAmt
			public new abstract class curyAdjgAmt : PX.Data.IBqlField
			{
			}
			[PXDBDecimal(4)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXFormula(null, typeof(SumCalc<SOAdjust.curyAdjgBilledAmt>))]
			public override Decimal? CuryAdjgAmt
			{
				get
				{
					return this._CuryAdjgAmt;
				}
				set
				{
					this._CuryAdjgAmt = value;
				}
			}
			#endregion
			#region RGOLAmt
			public new abstract class rGOLAmt : PX.Data.IBqlField
			{
			}
			[PXDBDecimal(4)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public override Decimal? RGOLAmt
			{
				get
				{
					return this._RGOLAmt;
				}
				set
				{
					this._RGOLAmt = value;
				}
			}
			#endregion
			#region Released
			public new abstract class released : PX.Data.IBqlField
			{
			}
			[PXDBBool()]
			[PXDefault(false)]
			public override Boolean? Released
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
			#region Voided
			public new abstract class voided : PX.Data.IBqlField
			{
			}
			[PXDBBool()]
			[PXDefault(false)]
			public override Boolean? Voided
			{
				get
				{
					return this._Voided;
				}
				set
				{
					this._Voided = value;
				}
			}
			#endregion
			#region VoidAdjNbr
			public new abstract class voidAdjNbr : PX.Data.IBqlField
			{
			}
			[PXDBInt()]
			public override Int32? VoidAdjNbr
			{
				get
				{
					return this._VoidAdjNbr;
				}
				set
				{
					this._VoidAdjNbr = value;
				}
			}
			#endregion
			#region AdjdARAcct
			public new abstract class adjdARAcct : PX.Data.IBqlField
			{
			}
			[Account()]
			[PXDBDefault(typeof(ARInvoice.aRAccountID))]
			public override Int32? AdjdARAcct
			{
				get
				{
					return this._AdjdARAcct;
				}
				set
				{
					this._AdjdARAcct = value;
				}
			}
			#endregion
			#region AdjdARSub
			public new abstract class adjdARSub : PX.Data.IBqlField
			{
			}
			[SubAccount()]
			[PXDBDefault(typeof(ARInvoice.aRSubID))]
			public override Int32? AdjdARSub
			{
				get
				{
					return this._AdjdARSub;
				}
				set
				{
					this._AdjdARSub = value;
				}
			}
			#endregion
			#region NoteID
			public new abstract class noteID : PX.Data.IBqlField
			{
			}
			[PXNote()]
			public override Int64? NoteID
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
			#region CuryDocBal
			public new abstract class curyDocBal : PX.Data.IBqlField
			{
			}
			[PXCurrency(typeof(ARAdjust.adjdCuryInfoID), typeof(ARAdjust.docBal), BaseCalc = false)]
			[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public override Decimal? CuryDocBal
			{
				get
				{
					return this._CuryDocBal;
				}
				set
				{
					this._CuryDocBal = value;
				}
			}
			public override Decimal? CuryPayDocBal
			{
				get
				{
					return this._CuryDocBal;
				}
				set
				{
					this._CuryDocBal = value;
				}
			}
			#endregion
			#region DocBal
			public new abstract class docBal : PX.Data.IBqlField
			{
			}
			[PXDecimal(4)]
			[PXUnboundDefault(TypeCode.Decimal, "0.0")]
			public override Decimal? DocBal
			{
				get
				{
					return this._DocBal;
				}
				set
				{
					this._DocBal = value;
				}
			}
			public override Decimal? PayDocBal
			{
				get
				{
					return this._DocBal;
				}
				set
				{
					this._DocBal = value;
				}
			}
			#endregion
			#region CuryDiscBal
			public new abstract class curyDiscBal : PX.Data.IBqlField
			{
			}
			[PXCurrency(typeof(ARAdjust.adjdCuryInfoID), typeof(ARAdjust.discBal), BaseCalc = false)]
			[PXUIField(DisplayName = "Cash Discount Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public override Decimal? CuryDiscBal
			{
				get
				{
					return this._CuryDiscBal;
				}
				set
				{
					this._CuryDiscBal = value;
				}
			}
			public override Decimal? CuryPayDiscBal
			{
				get
				{
					return this._CuryDiscBal;
				}
				set
				{
					this._CuryDiscBal = value;
				}
			}
			#endregion
			#region DiscBal
			public new abstract class discBal : PX.Data.IBqlField
			{
			}
			[PXDecimal(4)]
			[PXUnboundDefault()]
			public override Decimal? DiscBal
			{
				get
				{
					return this._DiscBal;
				}
				set
				{
					this._DiscBal = value;
				}
			}
			public override Decimal? PayDiscBal
			{
				get
				{
					return this._DiscBal;
				}
				set
				{
					this._DiscBal = value;
				}
			}
			#endregion
			#region VoidAppl
			public new abstract class voidAppl : PX.Data.IBqlField
			{
			}
			[PXBool()]
			[PXUIField(DisplayName = "Void Application", Visibility = PXUIVisibility.Visible)]
			[PXDefault(false)]
			public override Boolean? VoidAppl
			{
				[PXDependsOnFields(typeof(adjgDocType))]
				get
				{
					return (this._AdjgDocType == ARDocType.VoidPayment);
				}
				set
				{
					if ((bool)value)
					{
						this._AdjgDocType = ARDocType.VoidPayment;
						this.Voided = true;
					}
				}
			}
			#endregion
			#region ReverseGainLoss
			public new abstract class reverseGainLoss : PX.Data.IBqlField
			{
			}
			[PXBool()]
			public override Boolean? ReverseGainLoss
			{
				[PXDependsOnFields(typeof(adjgDocType))]
				get
				{
					return (ARPaymentType.DrCr(this._AdjgDocType) == "C");
				}
				set
				{
				}
			}
			#endregion
		}
		#endregion

		private InventoryItem InventoryItemGetByID(int? inventoryID)
		{
			return PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, inventoryID);
		}

		private DRDeferredCode DeferredCodeGetByID(string deferredCodeID)
		{
			return PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Required<DRDeferredCode.deferredCodeID>>>>.Select(this, deferredCodeID);
		}

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (string.Compare(viewName, "Transactions", true) == 0)
			{
				if (keys.Contains("tranType")) keys["tranType"] = Document.Current.DocType;
				else keys.Add("tranType", Document.Current.DocType);
				if (keys.Contains("tranType")) keys["refNbr"] = Document.Current.RefNbr;
				else keys.Add("refNbr", Document.Current.RefNbr);
				if (DontUpdateExistRecords)
				{
					if (keys.Contains("lineNbr")) keys["lineNbr"] = Document.Current.LineCntr;
					else keys.Add("lineNbr", Document.Current.LineCntr);
				}
			}
			return true;
		}

		private static bool DontUpdateExistRecords
		{
			get
			{
				object dontUpdateExistRecords;
				return
					PX.Common.PXExecutionContext.Current.Bag.TryGetValue(PXImportAttribute._DONT_UPDATE_EXIST_RECORDS,
																		 out dontUpdateExistRecords) &&
																		 true.Equals(dontUpdateExistRecords);
			}
		}

		public bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public virtual void PrepareItems(string viewName, IEnumerable items) { }

		#region Avalara Tax

		protected bool IsExternalTax
		{
			get
			{
				if (Document.Current == null)
					return false;

				return AvalaraMaint.IsExternalTax(this, Document.Current.TaxZoneID);
			}
		}

		public virtual ARInvoice CalculateAvalaraTax(ARInvoice invoice)
		{
			TaxSvc service = new TaxSvc();
			AvalaraMaint.SetupService(this, service);

			AvaAddress.AddressSvc addressService = new AvaAddress.AddressSvc();
			AvalaraMaint.SetupService(this, addressService);

			GetTaxRequest getRequest = BuildGetTaxRequest(invoice);

			if (getRequest.Lines.Count == 0)
			{
                Document.SetValueExt<ARInvoice.isTaxValid>(invoice, true);
				if (invoice.IsTaxSaved == true)
					Document.SetValueExt<ARInvoice.isTaxSaved>(invoice, false);
				try
				{
					skipAvalaraCallOnSave = true;
					this.Save.Press();
				}
				finally
				{
					skipAvalaraCallOnSave = false;
				}

			}

			GetTaxResult result = service.GetTax(getRequest);
			if (result.ResultCode == SeverityLevel.Success)
			{
				try
				{
					ApplyAvalaraTax(invoice, result);

					try
					{
						skipAvalaraCallOnSave = true;
						this.Save.Press();
					}
					finally
					{
						skipAvalaraCallOnSave = false;
					}

				}
				catch (Exception ex)
				{
					try
					{
						CancelTax(invoice, CancelCode.Unspecified);
					}
					catch (Exception)
					{
						throw new PXException(new PXException(ex, TX.Messages.FailedToApplyTaxes), TX.Messages.FailedToCancelTaxes);
					}

					throw new PXException(ex, TX.Messages.FailedToApplyTaxes);
				}

				PostTaxRequest request = new PostTaxRequest();
				request.CompanyCode = getRequest.CompanyCode;
				request.DocCode = getRequest.DocCode;
				request.DocDate = getRequest.DocDate;
				request.DocType = getRequest.DocType;
				request.TotalAmount = result.TotalAmount;
				request.TotalTax = result.TotalTax;
				PostTaxResult postResult = service.PostTax(request);
				if (postResult.ResultCode == SeverityLevel.Success)
				{
                    invoice.IsTaxValid = true;
                    invoice = Document.Update(invoice);
					try
					{
						skipAvalaraCallOnSave = true;
						this.Save.Press();
					}
					finally
					{
						skipAvalaraCallOnSave = false;
					}
				}
			}
			else
			{
				LogMessages(result);

				throw new PXException(TX.Messages.FailedToGetTaxes);
			}
			

			return invoice;
		}

		protected virtual GetTaxRequest BuildGetTaxRequest(ARInvoice invoice)
		{
			if (invoice == null)
				throw new PXArgumentException(ErrorMessages.ArgumentNullException);

			Customer cust = (Customer)customer.View.SelectSingleBound(new object[] { invoice });
			Location loc = (Location) location.View.SelectSingleBound(new object[] { invoice });
			
			GetTaxRequest request = new GetTaxRequest();
			request.CompanyCode = AvalaraMaint.CompanyCodeFromBranch(this, invoice.BranchID);
			request.CurrencyCode = invoice.CuryID;
			request.CustomerCode = cust.AcctCD;
			IAddressBase fromAddress = GetFromAddress(invoice);
			IAddressBase toAddress = GetToAddress(invoice);

			if ( fromAddress == null)
				throw new PXException("Failed to get the 'FROM' Address for the document");

			if (toAddress == null)
				throw new PXException("Failed to get the 'TO' Address for the document");
			
			request.OriginAddress = AvalaraMaint.FromAddress(fromAddress);
			request.DestinationAddress = AvalaraMaint.FromAddress(toAddress);
			request.DetailLevel = DetailLevel.Summary;
			request.DocCode = string.Format("AR.{0}.{1}", invoice.DocType, invoice.RefNbr);
			request.DocDate = invoice.DocDate.GetValueOrDefault();
			if ( !string.IsNullOrEmpty(invoice.AvalaraCustomerUsageType) )
			{
				request.CustomerUsageType = invoice.AvalaraCustomerUsageType;
			}
			if (!string.IsNullOrEmpty(loc.CAvalaraExemptionNumber))
			{
				request.ExemptionNo = loc.CAvalaraExemptionNumber;
			}
			
			request.DocType = DocumentType.SalesInvoice;
			int mult = invoice.DocType == ARDocType.CreditMemo ? -1 : 1;

			PXSelectBase<ARTran> select = new PXSelectJoin<ARTran,
				LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<ARTran.inventoryID>>,
					LeftJoin<Account, On<Account.accountID, Equal<ARTran.accountID>>>>,
				Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>,
					And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>>>,
				OrderBy<Asc<ARTran.tranType, Asc<ARTran.refNbr, Asc<ARTran.lineNbr>>>>>(this);

			request.Discount = GetDocDiscount().GetValueOrDefault();
			DateTime? taxDate = invoice.OrigDocDate;

			foreach (PXResult<ARTran, InventoryItem, Account> res in select.View.SelectMultiBound(new object[] { invoice }))
			{
				ARTran tran = (ARTran)res;
				InventoryItem item = (InventoryItem)res;
				Account salesAccount = (Account)res;

				Line line = new Line();
				line.No = Convert.ToString(tran.LineNbr);
				line.Amount = mult * tran.TranAmt.GetValueOrDefault();
				line.Description = tran.TranDesc;
				line.DestinationAddress = request.DestinationAddress;
				line.OriginAddress = request.OriginAddress;
				line.ItemCode = item.InventoryCD;
				line.Qty = Convert.ToDouble(tran.Qty.GetValueOrDefault());
				line.Discounted = request.Discount > 0;
				line.TaxIncluded = avalaraSetup.Current.IsInclusiveTax == true;
			
				if (avalaraSetup.Current != null && avalaraSetup.Current.SendRevenueAccount == true)
					line.RevAcct = salesAccount.AccountCD;

				line.TaxCode = tran.TaxCategoryID;

				if (tran.OrigInvoiceDate != null)
					taxDate = tran.OrigInvoiceDate;

				request.Lines.Add(line);
			}

			

			switch (invoice.DocType)
			{
				case ARDocType.Invoice:
				case ARDocType.DebitMemo:
				case ARDocType.FinCharge:
				case ARDocType.CashSale:
					request.DocType = DocumentType.SalesInvoice;
					break;
				case ARDocType.CreditMemo:
				case ARDocType.CashReturn:
					if (invoice.OrigDocDate != null)
					{
						request.TaxOverride.Reason = "Return";
						request.TaxOverride.TaxDate = taxDate.Value;
						request.TaxOverride.TaxOverrideType = TaxOverrideType.TaxDate;
						mult = -1;
					}
					request.DocType = DocumentType.ReturnInvoice;
					break;

				default:
					throw new PXException("DocType is not supported/implemented.");
			}

			return request;
		}

		protected bool skipAvalaraCallOnSave = false;
		protected virtual void ApplyAvalaraTax(ARInvoice invoice, GetTaxResult result)
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

			for (int i = 0; i < result.TaxSummary.Count; i++)
			{
				string taxID;
				if (result.TaxSummary[i].JurisType == JurisdictionType.Special)
				{
					if (result.TaxSummary[i].JurisName.Length > Tax.taxID.Length)
					{
						taxID = result.TaxSummary[i].JurisName.Substring(0, Tax.taxID.Length);
					}
					else
					{
						taxID = result.TaxSummary[i].JurisName;
					}
				}
				else
				{
					taxID = result.TaxSummary[i].TaxName.ToUpperInvariant();
				}

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
					tx.TaxCalcLevel = avalaraSetup.Current.IsInclusiveTax == true ? CSTaxCalcLevel.Inclusive : CSTaxCalcLevel.CalcOnItemAmt;
					tx.TaxApplyTermsDisc = CSTaxTermsDiscount.ToTaxableAmount;
					tx.SalesTaxAcctID = vendor.SalesTaxAcctID;
					tx.SalesTaxSubID = vendor.SalesTaxSubID;
					tx.ExpenseAccountID = vendor.TaxExpenseAcctID;
					tx.ExpenseSubID = vendor.TaxExpenseSubID;
					tx.TaxVendorID = taxZone.TaxVendorID;

					this.Caches[typeof (Tax)].Insert(tx);
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
					
					tax = Taxes.Insert(tax);
					if (tax == null)
						throw new PXException();
				}
			}

			foreach (ARTaxTran taxTran in existingRows.Values)
			{
				Taxes.Delete(taxTran);
			}

			bool requireControlTotal = ARSetup.Current.RequireControlTotal == true;

			if (invoice.Hold != true)
				ARSetup.Current.RequireControlTotal = false;


			try
			{
				invoice.CuryTaxTotal = Math.Abs(result.TotalTax);
				Document.Cache.SetValueExt<ARInvoice.isTaxSaved>(invoice, true);
			}
			finally
			{
				ARSetup.Current.RequireControlTotal = requireControlTotal;
			}
		} 
		
		protected virtual void CancelTax(ARInvoice invoice, CancelCode code)
		{
			CancelTaxRequest request = new CancelTaxRequest();
			request.CompanyCode = AvalaraMaint.CompanyCodeFromBranch(this, invoice.BranchID);
			request.CancelCode = code;
			request.DocCode = string.Format("AR.{0}.{1}", invoice.DocType, invoice.RefNbr);
			request.DocType = DocumentType.SalesInvoice;

			TaxSvc service = new TaxSvc();
			AvalaraMaint.SetupService(this, service);
			CancelTaxResult result = service.CancelTax(request);

			bool raiseError = false;
			if ( result.ResultCode != SeverityLevel.Success )
			{
				LogMessages(result);

				if (result.ResultCode == SeverityLevel.Error && result.Messages[0].Name == "DocumentNotFoundError")
				{
					//just ignore this error. There is no document to delete in avalara.
				}
				else
				{
					raiseError = true;
				}
			}

			if ( raiseError )
			{
				throw new PXException(TX.Messages.FailedToDeleteFromAvalara);
			}
			else
			{
				invoice.IsTaxSaved = false;
				invoice.IsTaxValid = false;
				if (Document.Cache.GetStatus(invoice) == PXEntryStatus.Notchanged)
					Document.Cache.SetStatus(invoice, PXEntryStatus.Updated);
			}
		}

		protected virtual void LogMessages(BaseResult result)
		{
			foreach (AvaMessage msg in result.Messages)
			{
				switch (result.ResultCode)
				{
					case SeverityLevel.Exception:
					case SeverityLevel.Error:
						PXTrace.WriteError(msg.Summary + ": " + msg.Details);
						break;
					case SeverityLevel.Warning:
						PXTrace.WriteWarning(msg.Summary + ": " + msg.Details);
						break;
				}
			}
		}

		protected virtual IAddressBase GetFromAddress(ARInvoice invoice)
		{
			PXSelectBase<Branch> select = new PXSelectJoin
				<Branch, InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>,
					InnerJoin<Address, On<Address.addressID, Equal<BAccountR.defAddressID>>>>,
					Where<Branch.branchID, Equal<Required<Branch.branchID>>>>(this);

			foreach (PXResult<Branch, BAccountR, Address> res in select.Select(invoice.BranchID))
				return (Address) res;

			return null;
		}

		protected virtual IAddressBase GetToAddress(ARInvoice invoice)
		{
			IAddressBase result;
			if (invoice.OrigModule == BatchModule.SO)
			{
				PXSelectBase<SOOrderShipment> orderShipments = new PXSelectJoin<SOOrderShipment,
					LeftJoin<SOShipment, On<SOShipment.shipmentType, Equal<SOOrderShipment.shipmentType>,
							And<SOShipment.shipmentNbr, Equal<SOOrderShipment.shipmentNbr>>>,
					LeftJoin<Carrier, On<Carrier.carrierID, Equal<SOShipment.shipVia>>>>,
					Where<SOOrderShipment.invoiceType, Equal<Required<SOOrderShipment.invoiceType>>,
						And<SOOrderShipment.invoiceNbr, Equal<Required<SOOrderShipment.invoiceNbr>>>>>(this);

				bool? isCommonCarrier = null;
				List<int> shipments = new List<int>();
				foreach (PXResult<SOOrderShipment, SOShipment, Carrier> res in orderShipments.Select(invoice.DocType, invoice.RefNbr))
				{
					Carrier carrier = (Carrier)res;

					if (carrier != null && carrier.CarrierID != null)
					{
						if (isCommonCarrier == null)
						{
							isCommonCarrier = carrier.IsCommonCarrier == true;
						}
						else
						{
							if (isCommonCarrier != carrier.IsCommonCarrier)
							{
								throw new PXException(Messages.CarriersCannotBeMixed);
							}
						}
					}



					SOShipment ship = (SOShipment)res;
					if (ship.ShipAddressID != null && !shipments.Contains(ship.ShipAddressID.Value))
						shipments.Add(ship.ShipAddressID.Value);
				}

				if (shipments.Count == 0)
				{
					//Sales order without actual shipment.
					var orders = new PXSelectJoin<SOOrderShipment,
						LeftJoin<SOOrder, On<SOOrder.orderType, Equal<SOOrderShipment.orderType>,
							And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>,
							LeftJoin<Carrier, On<Carrier.carrierID, Equal<SOOrder.shipVia>>>>,
						Where<SOOrderShipment.invoiceType, Equal<Required<SOOrderShipment.invoiceType>>,
							And<SOOrderShipment.invoiceNbr, Equal<Required<SOOrderShipment.invoiceNbr>>>>>(this);
					
					foreach (PXResult<SOOrderShipment, SOOrder, Carrier> res in orders.Select(invoice.DocType, invoice.RefNbr))
					{
						Carrier carrier = (Carrier)res;

						if (carrier != null && carrier.CarrierID != null)
						{
							if (isCommonCarrier == null)
							{
								isCommonCarrier = carrier.IsCommonCarrier == true;
							}
						}
					}

				}

				if (isCommonCarrier == true)
				{
					if (shipments.Count > 1)
					{
						throw new PXException(Messages.MultipleShipAddressOnInvoice);
					}

					PXSelectBase<SOAddress> select = new PXSelectJoin<SOAddress,
				InnerJoin<SOInvoice, On<SOInvoice.shipAddressID, Equal<SOAddress.addressID>>>,
				Where<SOInvoice.docType, Equal<Required<SOInvoice.docType>>, And<SOInvoice.refNbr, Equal<Required<SOInvoice.refNbr>>>>>(this);

					result = (SOAddress)select.SelectSingle(invoice.DocType, invoice.RefNbr);
				}
				else
				{
					//When this is not a common carrier, we calculate tax at brand where goods are sold, delivered from or picked up
					return GetFromAddress(invoice);
				}

			}
			else
			{
				result = (Address)PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(this, customer.Current.DefAddressID);
			}

			return result;
		}

		protected virtual decimal? GetDocDiscount()
		{
			return null;
		}

		#endregion


        public void RUTROTDistribution_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            if (PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>())
                WarnPersonAmount(e.Row as RUTROTDistribution);
        }

        public virtual void RUTROTDistribution_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>())
            {
                var error = WarnPersonAmount(e.Row as RUTROTDistribution);
                if (error != null)
                    throw error;
            }
        }

        #region RUTROT Helpers
        private void UncheckRUTROTIfProhibited(ARInvoice invoice)
        {
            var branch = CurrentBranch.SelectSingle(invoice.BranchID);

            if (branch == null || invoice == null)
                return;

            bool rutrotAllowed = (branch.AllowsRUTROT == true) && (branch.RUTROTCuryID == invoice.CuryID);

            if (rutrotAllowed == false && invoice.IsRUTROTDeductible == true)
            {
                invoice.IsRUTROTDeductible = false;
                Document.Cache.Update(invoice);
            }
        }

        private void SetPersistingChecks(ARInvoice invoice)
        {
            var check = invoice.IsRUTROTDeductible == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing;
            var rotCheck = invoice.IsRUTROTDeductible == true && invoice.RUTROTType == RUTROTTypes.ROT ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing;

            PXDefaultAttribute.SetPersistingCheck<ARInvoice.rUTROTType>(Document.Cache, invoice, check);
            PXDefaultAttribute.SetPersistingCheck<ARInvoice.rUTROTDeductionPct>(Document.Cache, invoice, check);
            PXDefaultAttribute.SetPersistingCheck<ARInvoice.curyRUTROTPersonalAllowance>(Document.Cache, invoice, check);

            PXDefaultAttribute.SetPersistingCheck<ARInvoice.rOTEstateAppartment>(Document.Cache, invoice, rotCheck);
	}	


        #region Validation
        private bool WarnOnDeductionExceedsAllowance(ARInvoice invoice, PXErrorLevel errorLevel)
        {
            if (invoice == null || invoice.IsRUTROTDeductible != true || invoice.CuryRUTROTAllowedAmt == null)
                return false;

            Document.Cache.RaiseExceptionHandling<ARInvoice.curyRUTROTTotalAmt>(invoice, invoice.CuryRUTROTUndistributedAmt,null);

            Action<string> setNotification = null;
            if (errorLevel == PXErrorLevel.Error)
                setNotification = m => PXUIFieldAttribute.SetError<ARInvoice.curyRUTROTTotalAmt>(Document.Cache, invoice, m);
            else
                setNotification = m => PXUIFieldAttribute.SetWarning<ARInvoice.curyRUTROTTotalAmt>(Document.Cache, invoice, m);

            if (invoice.CuryRUTROTTotalAmt <= invoice.CuryRUTROTAllowedAmt)
            {
                return false;
            }
            else if (invoice.CuryRUTROTAllowedAmt > 0.0m)
            {
                PXUIFieldAttribute.SetWarning<ARInvoice.curyRUTROTTotalAmt>(Document.Cache, invoice, RUTROTMessages.DeductibleExceedsAllowance);
                return false;
            }
            else
            {
                setNotification(RUTROTMessages.PeopleAreRequiredForDeduction);
                return true;
            }
        }

        private bool WarnUndistributedAmount(ARInvoice invoice, PXErrorLevel errorLevel)
        {
            if (invoice == null || invoice.IsRUTROTDeductible != true)
                return false;

            decimal maxDiff = 0.0m;

            if (PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.invoiceRounding>() && currencyinfo.Current != null)
            {
                var distributor = new DistributionRounding(ARSetup.Current, true) { CuryPlaces = currencyinfo.Current.CuryPrecision ?? 0 };
                maxDiff = distributor.FinishStep;
            }

            Action<string> setNotification = null;
            if (errorLevel == PXErrorLevel.Error)
                setNotification = m => PXUIFieldAttribute.SetError<ARInvoice.curyRUTROTUndistributedAmt>(Document.Cache, invoice, m);
            else
                setNotification = m => PXUIFieldAttribute.SetWarning<ARInvoice.curyRUTROTUndistributedAmt>(Document.Cache, invoice, m);

            if (invoice.CuryRUTROTUndistributedAmt > maxDiff)
            {
                if (invoice.RUTROTAutoDistribution == true)
                {
                    setNotification(RUTROTMessages.PositiveUndistributedAmount);
                    return true;
                }
                else
                {
                    PXUIFieldAttribute.SetWarning<ARInvoice.curyRUTROTUndistributedAmt>(Document.Cache, invoice, RUTROTMessages.PositiveUndistributedAmount);
                    return false;
                }
            }
            else if (invoice.CuryRUTROTUndistributedAmt < 0.0m)
            {
                setNotification(RUTROTMessages.NegativeUndistributedAmount);
                return true;
            }
            else
            {
                Document.Cache.RaiseExceptionHandling<ARInvoice.curyRUTROTUndistributedAmt>(invoice, invoice.CuryRUTROTUndistributedAmt, null);
                return false;
	}	
}

        private Exception WarnPersonAmount(RUTROTDistribution personalAmount)
        {
            var invoice = Document.Current;
            if (invoice == null || invoice.IsRUTROTDeductible != true || personalAmount == null)
                return null;

            var treatAsError = invoice.Hold != true;
            var errorLevel = treatAsError ? PXErrorLevel.Error : PXErrorLevel.Warning;

            PXSetPropertyException<RUTROTDistribution.curyAmount> error = null;

            if (personalAmount.CuryAmount <= 0.0m && invoice.CuryRUTROTTotalAmt != 0.0m)
            {
                error = new PXSetPropertyException<RUTROTDistribution.curyAmount>(RUTROTMessages.NonpositivePersonAmount, errorLevel);
            }
            else if (personalAmount.CuryAmount > personalAmount.CuryAllowance)
            {
                error = new PXSetPropertyException<RUTROTDistribution.curyAmount>(RUTROTMessages.PersonExceedsAllowance, errorLevel);
            }

            RUTROTDistribution.Cache.RaiseExceptionHandling<RUTROTDistribution.curyAmount>(personalAmount, personalAmount.CuryAmount, error);
            if (treatAsError)
                return error;
            else
                return null;
        }
        #endregion


        private void RedistributeDeduction()
        {
            var invoice = Document.Current;

            if (invoice == null || invoice.IsRUTROTDeductible != true)
                return;

            var persons = RUTROTDistribution.Select().ToList();
            int count = persons.Count;

            if (invoice.RUTROTAutoDistribution == true && count != 0)
            {
                decimal totalFromTrans = invoice.CuryRUTROTTotalAmt ?? 0.0m;


                var distributor = new DistributionRounding(ARSetup.Current, PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.invoiceRounding>()) 
                                        { PreventOverflow = true, CuryPlaces = currencyinfo.Current.CuryPrecision ?? 0 };
                var amts = distributor.DistributeEven(totalFromTrans, count);

                foreach (var p in persons.Zip(amts, (p, a) => new { DistributionItem = p, Amount = a }))
                {
                    var item = (RUTROTDistribution)RUTROTDistribution.Cache.CreateCopy((RUTROTDistribution)p.DistributionItem);
                    if (item.CuryAmount != p.Amount)
                    {
                        item.CuryAmount = p.Amount;
                        RUTROTDistribution.Update(item);
                    }
                }
            }

            RUTROTDistribution.View.RequestRefresh();
        }

        private void UpdateTranDeductibleFromInventory(ARTran tran)
        {
            if (tran == null)
                return;

            var invoice = Document.Current;

            if (invoice.IsRUTROTDeductible == true)
            {
                var item = InventoryItem.SelectSingle(tran.InventoryID);
                tran.IsRUTROTDeductible = item == null ? false : item.IsRUTROTDeductible == true;
            }
            else
            {
                tran.IsRUTROTDeductible = false;
            }
        }

        #endregion

        #region RUTROT Controls State Manager
        class RUTROTControlsStateManager
        {
            private readonly ARInvoiceEntry _graph;

            private readonly ARInvoice _invoice;
            private readonly GL.Branch _branch;

            private PXCache DocumentCache { get { return this._graph.Document.Cache; } }
            private PXCache DistributionCache { get { return this._graph.RUTROTDistribution.Cache; } }
            private PXCache LinesCache { get { return this._graph.Transactions.Cache; } }


            private bool ShowTick { get { return _branch != null && _branch.AllowsRUTROT == true && IsDocTypeSuitable; } }
            private bool ShowSection { get { return this._invoice.IsRUTROTDeductible == true; } }
            private bool ShowROTSection { get { return ShowSection && this._invoice.RUTROTType == RUTROTTypes.ROT; } }

            private bool IsDocTypeSuitable { get { return (this._invoice.DocType == ARDocType.Invoice) || (this._invoice.DocType == ARDocType.DebitMemo); } }

            private bool CurrenciesMatch { get { return _branch != null && _branch.RUTROTCuryID == this._invoice.CuryID; } }
            private bool EnableEdit { get { return this._invoice.Released != true && CurrenciesMatch; } }

            private bool IsAutoDistribution { get { return this._invoice.RUTROTAutoDistribution == true; } }

            private bool ShowDistributedAmt { get { return PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.invoiceRounding>(); } }

            public RUTROTControlsStateManager(ARInvoiceEntry graph)
            {
                this._graph = graph;

                this._invoice = graph.Document.Current;
                if (this._invoice != null)
                {
                    _branch = this._graph.CurrentBranch.SelectSingle(this._invoice.BranchID);
                }
            }

            public void Update()
            {
                if (this._invoice == null)
                    return;

                PXUIFieldAttribute.SetVisible<ARInvoice.isRUTROTDeductible>(DocumentCache, this._invoice, ShowTick);
                PXUIFieldAttribute.SetEnabled<ARInvoice.isRUTROTDeductible>(DocumentCache, this._invoice, EnableEdit);

                UpdateRUTROTSection();
                UpdateDistributionControls();
                UpdateLinesControls();
            }

            private void UpdateRUTROTSection()
            {
                PXUIFieldAttribute.SetVisible<ARInvoice.rUTROTType>(DocumentCache, this._invoice, ShowSection);
                PXUIFieldAttribute.SetVisible<ARInvoice.rUTROTAutoDistribution>(DocumentCache, this._invoice, ShowSection);
                PXUIFieldAttribute.SetVisible<ARInvoice.curyRUTROTTotalAmt>(DocumentCache, this._invoice, ShowSection);


                PXUIFieldAttribute.SetVisible<ARInvoice.rOTEstateAppartment>(DocumentCache, this._invoice, ShowROTSection);
                PXUIFieldAttribute.SetVisible<ARInvoice.rOTOrganizationNbr>(DocumentCache, this._invoice, ShowROTSection);
            }

            private void UpdateDistributionControls()
            {
                DistributionCache.AllowSelect = ShowSection;

                PXUIFieldAttribute.SetVisible<ARInvoice.curyRUTROTUndistributedAmt>(DocumentCache, this._invoice, ShowSection && IsAutoDistribution == false);
                PXUIFieldAttribute.SetVisible<ARInvoice.curyRUTROTDistributedAmt>(DocumentCache, this._invoice, ShowSection && ShowDistributedAmt && IsAutoDistribution);
                
                PXUIFieldAttribute.SetEnabled(DistributionCache, "CuryAmount", IsAutoDistribution == false);

                DistributionCache.AllowInsert = EnableEdit;
                DistributionCache.AllowDelete = EnableEdit;
                DistributionCache.AllowUpdate = EnableEdit;
            }

            private void UpdateLinesControls()
            {
                PXUIFieldAttribute.SetVisible(LinesCache, "IsRUTROTDeductible", ShowSection);
                PXUIFieldAttribute.SetEnabled(LinesCache, "IsRUTROTDeductible", ShowSection);
                PXUIFieldAttribute.SetVisible(LinesCache, "CuryRUTROTAvailableAmt", ShowSection);
            }
        }
        #endregion

        protected override void RecalculateDiscounts(PXCache sender, ARTran line)
        {
            if (line.InventoryID != null && line.Qty != null && line.CuryLineAmt != null && line.IsFree != true)
            {
                DiscountEngine<ARTran>.SetDiscounts<ARInvoiceDiscountDetail>(sender, Transactions, line, ARDiscountDetails, Document.Current.CustomerLocationID, Document.Current.CuryID, Document.Current.DocDate.Value, Document.Current.SkipDiscounts, true, recalcdiscountsfilter.Current);

                RecalculateTotalDiscount();
            }
        }

        private void RecalculateTotalDiscount()
        {
            if (Document.Current != null)
            {
                ARInvoice old_row = PXCache<ARInvoice>.CreateCopy(Document.Current);
                Document.Cache.SetValueExt<ARInvoice.curyDiscTot>(Document.Current, GetTotalGroupAndDocumentDiscount());
                Document.Cache.RaiseRowUpdated(Document.Current, old_row);
            }
        }

        private decimal GetTotalGroupAndDocumentDiscount()
        {
            if (Document.Current != null)
            {
                decimal total = 0;
                foreach (ARInvoiceDiscountDetail record in PXSelect<ARInvoiceDiscountDetail,
                        Where<ARInvoiceDiscountDetail.docType, Equal<Current<ARTran.tranType>>,
                        And<ARInvoiceDiscountDetail.refNbr, Equal<Current<ARTran.refNbr>>>>>.Select(this))
                {
                    total += record.CuryDiscountAmt ?? 0;
                }

                return total;
            }
            return 0;
        }

        protected virtual void AddDiscount(PXCache sender, ARInvoice row)
        {
            ARTran discount = (ARTran)Discount_Row.Cache.CreateInstance();
            discount.LineType = SOLineType.Discount;
            discount.DrCr = (Document.Current.DrCr == "D") ? "C" : "D";
            discount.FreezeManualDisc = true;
            discount = (ARTran)Discount_Row.Select() ?? (ARTran)Discount_Row.Cache.Insert(discount);

            ARTran old_row = (ARTran)Discount_Row.Cache.CreateCopy(discount);

            discount.CuryTranAmt = (decimal?)sender.GetValue<ARInvoice.curyDiscTot>(row);
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

            if (Discount_Row.Cache.GetStatus(discount) == PXEntryStatus.Notchanged)
            {
                Discount_Row.Cache.SetStatus(discount, PXEntryStatus.Updated);
            }

            discount.ManualDisc = true; //escape SOManualDiscMode.RowUpdated
            Discount_Row.Cache.RaiseRowUpdated(discount, old_row);

            decimal auotDocDisc = GetTotalGroupAndDocumentDiscount();
            if (auotDocDisc == discount.CuryTranAmt)
            {
                discount.ManualDisc = false;
            }
        }

        protected object GetValue<Field>(object data)
            where Field : IBqlField
        {
            return this.Caches[BqlCommand.GetItemType(typeof(Field))].GetValue(data, typeof(Field).Name);
        }

        private void DefaultDiscountAccountAndSubAccount(ARTran tran)
        {
            Location customerloc = location.Current;
            //Location companyloc = (Location)PXSelectJoin<Location, InnerJoin<BAccountR, On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>, InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>>, Where<Branch.branchID, Equal<Current<ARRegister.branchID>>>>.Select(this);

            object customer_LocationAcctID = GetValue<Location.cDiscountAcctID>(customerloc);
            //object company_LocationAcctID = GetValue<Location.cDiscountAcctID>(companyloc);


            if (customer_LocationAcctID != null)
            {
                tran.AccountID = (int?)customer_LocationAcctID;
                Discount_Row.Cache.RaiseFieldUpdated<ARTran.accountID>(tran, null);
            }

            if (tran.AccountID != null)
            {
                object customer_LocationSubID = GetValue<Location.cDiscountSubID>(customerloc);
                if (customer_LocationSubID != null)
                {
                    tran.SubID = (int?)customer_LocationSubID;
                    Discount_Row.Cache.RaiseFieldUpdated<ARTran.subID>(tran, null);
                }
            }
        }
	}
    [Serializable()]
    public partial class DuplicateFilter : IBqlTable
    {
        #region RefNbr
        public abstract class refNbr : PX.Data.IBqlField
        {
        }
        protected String _RefNbr;
        [PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "New Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
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
    }
}
