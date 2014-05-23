using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.TX;
using PX.Objects.IN;
using PX.Objects.CA;
using PX.Objects.BQLConstants;
using PX.Objects.EP;
using PX.Objects.PO;
using PX.Objects.SO;
using PX.Objects.DR;
using AP1099Hist = PX.Objects.AP.Overrides.APDocumentRelease.AP1099Hist;
using AP1099Yr = PX.Objects.AP.Overrides.APDocumentRelease.AP1099Yr;
using Avalara.AvaTax.Adapter;
using Avalara.AvaTax.Adapter.TaxService;
using AvaAddress = Avalara.AvaTax.Adapter.AddressService;
using AvaMessage = Avalara.AvaTax.Adapter.Message;

namespace PX.Objects.AP
{ 
    [Serializable]
	public class APInvoiceEntry : APDataEntryGraph<APInvoiceEntry, APInvoice>, PXImportAttribute.IPXPrepareItems
	{
        #region Internal Definitions + Cache Attached Events 
        #region InventoryItem
        #region COGSSubID
        [PXDefault(typeof(Search<INPostClass.cOGSSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>))]
        [SubAccount(typeof(InventoryItem.cOGSAcctID), DisplayName = "Expense Sub.", DescriptionField = typeof(Sub.description))]
        public virtual void InventoryItem_COGSSubID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion
        #region LandedCostTran
        #region Source

        [PXDBString(2, IsFixed = true)]
        [PXDefault(LandedCostTranSource.FromAP)]
        [PXUIField(DisplayName = "PO Receipt Type")]
        protected virtual void LandedCostTran_Source_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region VendorID

        [AP.VendorNonEmployeeActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true)]
        [PXDefault(typeof(APInvoice.vendorID))]
        [PXUIField(DisplayName = "Vendor ID", Visible = false)]
        protected virtual void LandedCostTran_VendorID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region VendorLocationID

        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<LandedCostTran.vendorID>>>), DescriptionField = typeof(Location.descr), Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(typeof(APInvoice.vendorLocationID))]        
        protected virtual void LandedCostTran_VendorLocationID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region LandedCostCodeID

        [PXDBString(15, IsUnicode = true, IsFixed = false)]
        [PXDefault()]
        [PXUIField(DisplayName = "Landed Cost Code")]
        [PXSelector(typeof(Search<LandedCostCode.landedCostCodeID,
                                    Where<LandedCostCode.applicationMethod, Equal<LandedCostApplicationMethod.fromAP>,
                                                Or<LandedCostCode.applicationMethod, Equal<LandedCostApplicationMethod.fromBoth>>>>))]
        protected virtual void LandedCostTran_LandedCostCodeID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region POReceiptType

        [PXDBString(3, IsFixed = true)]
        [PXDefault(PO.POReceiptType.POReceipt)]
        [POReceiptType.List()]
        [PXUIField(DisplayName = "PO Receipt Type")]
        protected virtual void LandedCostTran_POReceiptType_CacheAttached(PXCache sender)
        {
        }

        #endregion
        #region POReceiptNbr

        [PXDBString(15, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search<POReceipt.receiptNbr, Where<POReceipt.released, Equal<boolTrue>,
                        And<POReceipt.receiptType, Equal<Current<LandedCostTran.pOReceiptType>>>>, OrderBy<Desc<POReceipt.receiptNbr>>>))]
        [PXUIField(DisplayName = "PO Receipt Nbr.")]
        protected virtual void LandedCostTran_POReceiptNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region POReceiptLineNbr

        [PXDBInt()]
        [PXUIField(DisplayName = "PO Receipt Line Nbr.")]
        protected virtual void LandedCostTran_POReceiptLineNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region APDocType

        [PXDBString(3, IsFixed = true)]
        [PXDBDefault(typeof(APInvoice.docType))]
        [PXUIField(DisplayName = "AP Doc. Type")]
        protected virtual void LandedCostTran_APDocType_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region APRefNbr

        [PXDBString(15, IsUnicode = true)]
        [PXDBDefault(typeof(APInvoice.refNbr))]
        [PXParent(typeof(Select<APInvoice, Where<APInvoice.docType, Equal<Current<LandedCostTran.aPDocType>>,
                            And<APInvoice.refNbr, Equal<Current<LandedCostTran.aPRefNbr>>>>>))]
        [PXUIField(DisplayName = "AP Ref. Nbr.")]
        protected virtual void LandedCostTran_APRefNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region INDocType

        [PXDBString(3, IsFixed = true)]
        [PXUIField(DisplayName = "IN Doc. Type")]
        protected virtual void LandedCostTran_INDocType_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region CuryID

        [PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
        [PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDBDefault(typeof(APInvoice.curyID))]
        protected virtual void LandedCostTran_CuryID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region APCuryInfoID

        [PXDBLong()]
        [CurrencyInfo(typeof(APInvoice.curyInfoID), ModuleCode = "AP")]        
        protected virtual void LandedCostTran_APCuryInfoID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region CuryInfoID

        [PXDBLong()]
        [PXDBLiteDefault(typeof(APInvoice.curyInfoID))]
        protected virtual void LandedCostTran_CuryInfoID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region InventoryID

        [IN.Inventory(typeof(InnerJoin<POReceiptLine, On<POReceiptLine.inventoryID, Equal<InventoryItem.inventoryID>>>),
                        typeof(Where<POReceiptLine.receiptNbr, Equal<Current<LandedCostTran.pOReceiptNbr>>>), DisplayName = "Inventory ID")]
        protected virtual void LandedCostTran_InventoryID_CacheAttached(PXCache sender)
        {
        }

        #endregion
        #region CuryLCAPAmount

        [PXDBCurrency(typeof(LandedCostTran.aPCuryInfoID), typeof(LandedCostTran.lCAPAmount), MinValue = 0)]
        [PXFormula(typeof(Mult<LandedCostTran.curyLCAPEffAmount, LandedCostTran.amountSign>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Amount")]
        protected virtual void LandedCostTran_CuryLCAPAmount_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region LCAPAmount

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        protected virtual void LandedCostTran_LCAPAmount_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region CuryLCAmount

        [PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(typeof(Switch<Case<Where<LandedCostTran.iNDocCreated, Equal<True>>, LandedCostTran.curyLCAmount>, LandedCostTran.curyLCAPAmount>))]
        [PXDBCurrency(typeof(LandedCostTran.curyInfoID), typeof(LandedCostTran.lCAmount))]
        [PXUIField(DisplayName = "Amount")]
        protected virtual void LandedCostTran_CuryLCAmount_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region LCAmount

        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        protected virtual void LandedCostTran_LCAmount_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region InvoiceDate

        [PXDBDate()]
        [PXDBDefault(typeof(APInvoice.docDate))]
        protected virtual void LandedCostTran_InvoiceDate_CacheAttached(PXCache sender)
        {
        }
        #endregion
        
        #endregion        
        #region APTran
        #region LCTranID

        [PXDBInt()]
        [PXDBLiteDefault(typeof(LandedCostTran.lCTranID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(Enabled = false, Visible = false)]
        protected virtual void APTran_LCTranID_CacheAttached(PXCache sender)
        {
        }        
        #endregion
        #endregion
        #region LandedCostTranR

        [PXProjection(typeof(Select<LandedCostTran, Where<LandedCostTran.postponeAP, Equal<True>, And<LandedCostTran.aPRefNbr, IsNull>>>), Persistent = false)]
        [PXCacheName(Messages.LandedCostTranR)]
        [Serializable]
        public partial class LandedCostTranR : LandedCostTran
        {
            #region Selected
            public abstract class selected : IBqlField
            {
            }
            protected bool? _Selected = false;
            [PXBool]
            [PXUIField(DisplayName = "Selected")]
            public bool? Selected
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
        }
        #endregion
        #endregion

        # region Buttons
        public ToggleCurrency<APInvoice> CurrencyView;

		public PXAction<APInvoice> viewSchedule;
		[PXUIField(DisplayName = "View Schedule")]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Settings)]
		public virtual IEnumerable ViewSchedule(PXAdapter adapter)
		{
			PXSelectBase<DRSchedule> select = new PXSelect<DRSchedule,
				Where<DRSchedule.module, Equal<BatchModule.moduleAP>,
				And<DRSchedule.docType, Equal<Current<APTran.tranType>>,
				And<DRSchedule.refNbr, Equal<Current<APTran.refNbr>>,
				And<DRSchedule.lineNbr, Equal<Current<APTran.lineNbr>>>>>>>(this);

			DRSchedule sc = select.Select();

			if (sc == null)
			{
				DRDeferredCode defCode = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Current<APTran.deferredCode>>>>.Select(this);
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
				DRDeferredCode defCode = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Current<APTran.deferredCode>>>>.Select(this);
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
				throw new PXRedirectRequiredException(target, true, "ViewSchedule"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
			}

			return adapter.Get();
		}


		public PXAction<APInvoice> newVendor;
		[PXUIField(DisplayName = "New Vendor", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable NewVendor(PXAdapter adapter)
		{
			VendorMaint graph = PXGraph.CreateInstance<VendorMaint>();
			throw new PXRedirectRequiredException(graph, "New Vendor");
		}

		public PXAction<APInvoice> editVendor;
		[PXUIField(DisplayName = "Edit Vendor", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable EditVendor(PXAdapter adapter)
		{
			if (vendor.Current != null)
			{
				VendorMaint graph = PXGraph.CreateInstance<VendorMaint>();
				graph.BAccount.Current = (VendorR)vendor.Current;
				throw new PXRedirectRequiredException(graph, "Edit Vendor");
			}
			return adapter.Get();
		}

		public PXAction<APInvoice> vendorDocuments;
		[PXUIField(DisplayName = "Vendor Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable VendorDocuments(PXAdapter adapter)
		{
			if (vendor.Current != null)
			{
				APDocumentEnq graph = PXGraph.CreateInstance<APDocumentEnq>();
				graph.Filter.Current.VendorID = vendor.Current.BAccountID;
				graph.Filter.Select();
				throw new PXRedirectRequiredException(graph, "Vendor Details");
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = "Release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public override IEnumerable Release(PXAdapter adapter)
		{
			PXCache cache = Document.Cache;
			List<APRegister> list = new List<APRegister>();
			foreach (APInvoice apdoc in adapter.Get<APInvoice>())
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
				PXLongOperation.StartOperation(this, delegate() { APDocumentRelease.ReleaseDoc(list, false); });
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
					List<APRegister> listWithTax = new List<APRegister>();
					foreach (APInvoice apdoc in list)
					{
						if (apdoc.IsTaxValid != true && AvalaraMaint.IsExternalTax(this,apdoc.TaxZoneID))
						{
							APInvoice doc = new APInvoice();
							doc.DocType = apdoc.DocType;
							doc.RefNbr = apdoc.RefNbr;
							doc.OrigModule = apdoc.OrigModule;
							listWithTax.Add(APExternalTaxCalc.Process(doc));
						}
						else
						{
							listWithTax.Add(apdoc);
						}
					}

					APDocumentRelease.ReleaseDoc(listWithTax, false);
				});
			}
			
			return list;
		}

		public PXAction<APInvoice> prebook;
        [PXUIField(DisplayName = "Pre-release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable Prebook(PXAdapter adapter)
		{
			PXCache cache = Document.Cache;
			List<APRegister> list = new List<APRegister>();
			foreach (APInvoice apdoc in adapter.Get<APInvoice>())
			{
				if (!(bool)apdoc.Hold && !(bool)apdoc.Released && apdoc.Prebooked != true)
				{
					if (apdoc.PrebookAcctID == null)
					{
						cache.RaiseExceptionHandling<APInvoice.prebookAcctID>(apdoc, apdoc.PrebookAcctID, new PXSetPropertyException(Messages.PrebookingAccountIsRequiredForPrebooking)); 
						continue;
					}
					if (apdoc.PrebookSubID == null)
					{
						cache.RaiseExceptionHandling<APInvoice.prebookSubID>(apdoc, apdoc.PrebookSubID, new PXSetPropertyException(Messages.PrebookingAccountIsRequiredForPrebooking));
						continue;
					}						
					cache.Update(apdoc);
					list.Add(apdoc);
				}
			}
			if (list.Count == 0)
			{
				throw new PXException(ErrorMessages.RecordRaisedErrors, "Updating", this.Document.Cache.GetItemType().Name);
			}
			Persist();
			PXLongOperation.StartOperation(this, delegate() { APDocumentRelease.ReleaseDoc(list, false, true); });
			return list;
		}

		public PXAction<APInvoice> voidInvoice;
		[PXUIField(DisplayName = "Void", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable VoidInvoice(PXAdapter adapter)
		{
			PXCache cache = Document.Cache;
			List<APRegister> list = new List<APRegister>();
			foreach (APInvoice apdoc in adapter.Get<APInvoice>())
			{
				if (apdoc.Released == true || apdoc.Prebooked == true)
				{
					cache.Update(apdoc);
					list.Add(apdoc);
				}
			}

			if (list.Count == 0)
			{
				throw new PXException(Messages.Document_Status_Invalid);
			}
			Persist();
			PXLongOperation.StartOperation(this, delegate() { APDocumentRelease.VoidDoc(list); });
			return list;
		}

		public PXAction<APInvoice> vendorRefund;
		[PXUIField(DisplayName = "Vendor Refund", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable VendorRefund(PXAdapter adapter)
		{
			if (Document.Current != null && (bool)Document.Current.Released && Document.Current.DocType == APDocType.DebitAdj && (bool)Document.Current.OpenDoc)
			{
				APPaymentEntry pe = PXGraph.CreateInstance<APPaymentEntry>();

				APAdjust adj = PXSelect<APAdjust, Where<APAdjust.adjdDocType, Equal<Current<APInvoice.docType>>,
					And<APAdjust.adjdRefNbr, Equal<Current<APInvoice.refNbr>>, And<APAdjust.released, Equal<boolFalse>>>>>.Select(this);

				if (adj != null)
				{
					pe.Document.Current = pe.Document.Search<APPayment.refNbr>(adj.AdjgRefNbr, adj.AdjgDocType);
				}
				else
				{
					pe.Clear();
					pe.CreatePayment(Document.Current);
				}
				throw new PXRedirectRequiredException(pe, "PayInvoice");
			}
			return adapter.Get();
		}

		public PXAction<APInvoice> reverseInvoice;
		[PXUIField(DisplayName = Messages.APReverseBill, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ReverseInvoice(PXAdapter adapter)
		{
			if (Document.Current != null && (Document.Current.DocType == APDocType.Invoice ||
											 Document.Current.DocType == APDocType.CreditAdj))
			{
				if (Document.Current.InstallmentNbr != null && string.IsNullOrEmpty(Document.Current.MasterRefNbr) == false)
				{
					throw new PXSetPropertyException(Messages.Multiply_Installments_Cannot_be_Reversed, Document.Current.MasterRefNbr);
				}

				this.Save.Press();
				bool isPOReferenced = false;
				foreach (APTran itr in this.Transactions.Select())
				{
					if (!string.IsNullOrEmpty(itr.ReceiptNbr) || !string.IsNullOrEmpty(itr.PONbr))
					{
						isPOReferenced = true;
						break;
					}
				}
				if (isPOReferenced)
				{
					this.Document.Ask(Messages.Warning, Messages.DebitAdjustmentRowReferecesPOOrderOrPOReceipt, MessageButtons.OK, MessageIcon.Warning);
				}
				APInvoice doc = PXCache<APInvoice>.CreateCopy(Document.Current);

				if (finperiod.Current != null && (finperiod.Current.Active == false || finperiod.Current.APClosed == true && glsetup.Current.PostClosedPeriods == false))
				{
					FinPeriod firstopen = PXSelect<FinPeriod, Where<FinPeriod.aPClosed, Equal<boolFalse>, And<FinPeriod.active, Equal<boolTrue>, And<FinPeriod.finPeriodID, Greater<Required<FinPeriod.finPeriodID>>>>>, OrderBy<Asc<FinPeriod.finPeriodID>>>.Select(this, doc.FinPeriodID);
					if (firstopen == null)
					{
						Document.Cache.RaiseExceptionHandling<APInvoice.finPeriodID>(Document.Current, Document.Current.FinPeriodID, new PXSetPropertyException(GL.Messages.NoOpenPeriod));
						return adapter.Get();
					}
					doc.FinPeriodID = firstopen.FinPeriodID;
				}

				try
				{
					this.ReverseInvoiceProc(doc);

					Document.Cache.RaiseExceptionHandling<APInvoice.finPeriodID>(Document.Current, Document.Current.FinPeriodID, null);

					return new List<APInvoice> { Document.Current };
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

		public PXAction<APInvoice> payInvoice;
		[PXUIField(DisplayName = Messages.APPayBill, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable PayInvoice(PXAdapter adapter)
		{
			if (Document.Current != null && (Document.Current.Released == true || Document.Current.Prebooked == true))
			{
				APPaymentEntry pe = PXGraph.CreateInstance<APPaymentEntry>();
				if (Document.Current.OpenDoc == true && (Document.Current.Payable == true || Document.Current.DocType == APInvoiceType.Prepayment))
				{
					APAdjust adj = PXSelect<APAdjust, Where<APAdjust.adjdDocType, Equal<Current<APInvoice.docType>>,
						And<APAdjust.adjdRefNbr, Equal<Current<APInvoice.refNbr>>, And<APAdjust.released, Equal<boolFalse>>>>>.Select(this);

					if (adj != null)
					{
						pe.Document.Current = pe.Document.Search<APPayment.refNbr>(adj.AdjgRefNbr, adj.AdjgDocType);
					}
					else
					{
						pe.Clear();
						pe.CreatePayment(Document.Current);
					}
					throw new PXRedirectRequiredException(pe, "PayInvoice");
				}
				else if (Document.Current.DocType == APDocType.DebitAdj)
				{
					pe.Document.Current = pe.Document.Search<APPayment.refNbr>(Document.Current.RefNbr, Document.Current.DocType);
					throw new PXRedirectRequiredException(pe, "PayInvoice");
				}
			}
			return adapter.Get();
		}

		public PXAction<APInvoice> createSchedule;
		[PXUIField(DisplayName = "Assign to Schedule", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton(ImageKey = PX.Web.UI.Sprite.Main.Shedule)]
		public virtual IEnumerable CreateSchedule(PXAdapter adapter)
		{
			this.Save.Press();
			if (Document.Current != null && (bool)Document.Current.Released == false && (bool)Document.Current.Hold == false)
			{
				PX.Objects.AP.APScheduleMaint sm = PXGraph.CreateInstance<PX.Objects.AP.APScheduleMaint>();
				if ((bool)Document.Current.Scheduled && Document.Current.ScheduleID != null)
				{
					sm.Schedule_Header.Current = sm.Schedule_Header.Search<Schedule.scheduleID>(Document.Current.ScheduleID);
				}
				else
				{
					sm.Schedule_Header.Cache.Insert();
					APRegister doc = (APRegister)sm.Document_Detail.Cache.CreateInstance();
					PXCache<APRegister>.RestoreCopy(doc, Document.Current);
					doc = (APRegister)sm.Document_Detail.Cache.Update(doc);
				}
				throw new PXRedirectRequiredException(sm, "create Schedule");
			}
			return adapter.Get();
		}

		public PXAction<APInvoice> addPOReceipt;
        [PXUIField(DisplayName = PO.Messages.AddPOReceipt, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true, FieldClass = "INSITE")]
		[PXLookupButton]
		public virtual IEnumerable AddPOReceipt(PXAdapter adapter)
		{
			if (this.Document.Current != null &&
					this.Document.Current.Released == false &&
					this.Document.Current.Prebooked == false &&
				this.poreceiptslist.AskExt(true) == WebDialogResult.OK)
			{
				AddPOReceipt2(adapter);
			}
			poreceiptslist.View.Clear();
			poreceiptslist.Cache.Clear();
			return adapter.Get();
		}
		public PXAction<APInvoice> addPOReceipt2;
		[PXUIField(DisplayName = PO.Messages.AddPOReceipt, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddPOReceipt2(PXAdapter adapter)
		{
			if (this.Document.Current != null &&
					this.Document.Current.Released == false &&
					this.Document.Current.Prebooked == false )
			{
				foreach (POReceipt rc in poreceiptslist.Cache.Updated)
				{
					if (rc.Selected == true)
					{
						this.InvoicePOReceipt(rc, null, null, false, false);
					}
				}
			}
			poreceiptslist.View.Clear();
			poreceiptslist.Cache.Clear();
			return adapter.Get();
		}

		public PXAction<APInvoice> addReceiptLine;
        [PXUIField(DisplayName = PO.Messages.AddPOReceiptLine, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true, FieldClass = "INSITE")]
		[PXLookupButton]
		public virtual IEnumerable AddReceiptLine(PXAdapter adapter)
		{
			if (this.Document.Current != null &&
					this.Document.Current.Released == false &&
					this.Document.Current.Prebooked == false &&
					this.poReceiptLinesSelection.AskExt(true) == WebDialogResult.OK)
				return AddReceiptLine2(adapter);
			return adapter.Get();
		}
		public PXAction<APInvoice> addReceiptLine2;
		[PXUIField(DisplayName = PO.Messages.AddPOReceiptLine, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddReceiptLine2(PXAdapter adapter)
		{
			if (this.Document.Current != null &&
				this.Document.Current.Released == false &&
				this.Document.Current.Prebooked == false)
			{
				HashSet<APTran> duplicates = new HashSet<APTran>(new POReceiptLineComparer());
				foreach (APTran tran in this.Transactions.Select())
				{
					try
					{
						duplicates.Add(tran);
					}
					catch (NullReferenceException) { }
				}

				foreach (POReceiptLineS it in poReceiptLinesSelection.Select())
				{
					if (it.Selected == true)
					{
						this.AddPOReceiptLine(it, duplicates);
					}
				}
			}
			poReceiptLinesSelection.View.Clear();
			poReceiptLinesSelection.Cache.Clear();
			return adapter.Get();
		}

		public PXAction<APInvoice> addPOOrder;
		[PXUIField(DisplayName = PO.Messages.AddPOOrder, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true, FieldClass="INSITE")]
		[PXLookupButton]
		public virtual IEnumerable AddPOOrder(PXAdapter adapter)
		{
			if (this.Document.Current != null &&
				this.Document.Current.DocType == APDocType.Invoice &&
				this.Document.Current.Released == false &&
				this.Document.Current.Prebooked == false &&
				this.poorderslist.AskExt(true) == WebDialogResult.OK)
			{
				return AddPOOrder2(adapter);
			}
			return adapter.Get();
		}
		public PXAction<APInvoice> addPOOrder2;
		[PXUIField(DisplayName = PO.Messages.AddPOOrder, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddPOOrder2(PXAdapter adapter)
		{
			if (this.Document.Current != null &&
				this.Document.Current.DocType == APDocType.Invoice &&
				this.Document.Current.Released == false &&
				this.Document.Current.Prebooked == false)
			{
				foreach (POOrder rc in poorderslist.Cache.Updated)
				{
					if (rc.Selected == true)
					{
						this.InvoicePOOrder(rc, false);
					}
				}
			}
			poorderslist.View.Clear();
			poorderslist.Cache.Clear();
			return adapter.Get();
		}


		public PXAction<APInvoice> viewPODocument;
		[PXUIField(DisplayName = PO.Messages.ViewPODocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable ViewPODocument(PXAdapter adapter)
		{
			if (this.Transactions.Current != null)
			{
				APTran row = this.Transactions.Current;
				if (!String.IsNullOrEmpty(row.ReceiptNbr))
				{
					POReceiptEntry docGraph = PXGraph.CreateInstance<POReceiptEntry>();
					docGraph.Document.Current = docGraph.Document.Search<POReceipt.receiptNbr>(row.ReceiptNbr);
					if (docGraph.Document.Current != null)
						throw new PXRedirectRequiredException(docGraph, "View PO Receipt");
				}
				else
					if (!(String.IsNullOrEmpty(row.POOrderType) || String.IsNullOrEmpty(row.PONbr)))
					{
						POOrderEntry docGraph = PXGraph.CreateInstance<POOrderEntry>();
						docGraph.Document.Current = docGraph.Document.Search<POOrder.orderNbr>(row.PONbr, row.POOrderType);
						if (docGraph.Document.Current != null)
							throw new PXRedirectRequiredException(docGraph, "View PO Order");
					}
			}
			return adapter.Get();
		}

		public PXAction<APInvoice> autoApply;
		[PXUIField(DisplayName = "Auto Apply", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable AutoApply(PXAdapter adapter)
		{
			if (Document.Current != null && Document.Current.DocType == APDocType.Invoice
				&& Document.Current.Released == false &&
				this.Document.Current.Prebooked == false)
			{
				foreach (PXResult<APAdjust, APPayment, CurrencyInfo> res in Adjustments_Raw.Select())
				{
					APAdjust adj = (APAdjust)res;

					adj.CuryAdjdAmt = 0m;
				}

				decimal? CuryApplAmt = Document.Current.CuryDocBal;

				foreach (PXResult<APAdjust, APPayment> res in Adjustments.Select())
				{
					APAdjust adj = (APAdjust)res;

					if (adj.CuryDocBal > 0m)
					{
						if (adj.CuryDocBal > CuryApplAmt)
						{
							adj.CuryAdjdAmt = CuryApplAmt;
							CuryApplAmt = 0m;
							Adjustments.Cache.RaiseFieldUpdated<APAdjust.curyAdjdAmt>(adj, 0m);
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
							Adjustments.Cache.RaiseFieldUpdated<APAdjust.curyAdjdAmt>(adj, 0m);
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

		public PXAction<APInvoice> voidDocument;
		[PXUIField(DisplayName = Messages.Void, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable VoidDocument(PXAdapter adapter)
		{
			APRegister doc = (APRegister)Document.Current;
			if (doc != null && doc.DocType == APInvoiceType.Prepayment)
			{
				APAdjust adj = PXSelect<APAdjust, Where<APAdjust.adjdDocType, Equal<Required<APAdjust.adjdDocType>>,
													And<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>>>>
							   .Select(this, doc.DocType, doc.RefNbr);

				if (adj != null)
					throw new PXException(Messages.PrepaymentCannotBeVoided3, adj.AdjgRefNbr);

                this.VoidPrepayment(doc);

				List<APInvoice> rs = new List<APInvoice>();
				rs.Add(Document.Current);
				return rs;
			}
			return adapter.Get();
		}

		public PXAction<APInvoice> viewPayment;
		[PXUIField(DisplayName = "View Payment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewPayment(PXAdapter adapter)
		{
			if (Document.Current != null && Adjustments.Current != null)
			{
				APPaymentEntry pe = PXGraph.CreateInstance<APPaymentEntry>();
				pe.Document.Current = pe.Document.Search<APPayment.refNbr>(Adjustments.Current.AdjgRefNbr, Adjustments.Current.AdjgDocType);

				throw new PXRedirectRequiredException(pe, true, "Payment"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
			}
			return adapter.Get();
		}

        public PXAction<APInvoice> recalculateDiscountsAction;
        [PXUIField(DisplayName = "Recalculate Prices", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
        [PXButton]
        public virtual IEnumerable RecalculateDiscountsAction(PXAdapter adapter)
        {
            if (recalcdiscountsfilter.AskExt() == WebDialogResult.OK)
            {
                DiscountEngine<APTran>.RecalculatePricesAndDiscounts<APInvoiceDiscountDetail>(Transactions.Cache, Transactions, Transactions.Current, DiscountDetails, Document.Current.VendorLocationID, Document.Current.DocDate.Value, recalcdiscountsfilter.Current);
            }
            return adapter.Get();
        }

        public PXAction<APInvoice> recalcOk;
        [PXUIField(DisplayName = "OK", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable RecalcOk(PXAdapter adapter)
        {
            return adapter.Get();
        }

		#region Landed Cost
		public PXAction<APInvoice> viewLCPOReceipt;
		[PXUIField(DisplayName = PO.Messages.ViewPODocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable ViewLCPOReceipt(PXAdapter adapter)
		{
			if (this.landedCostTrans.Current != null)
			{
				LandedCostTran row = this.landedCostTrans.Current;
				if (!String.IsNullOrEmpty(row.POReceiptNbr))
				{
					POReceiptEntry docGraph = PXGraph.CreateInstance<POReceiptEntry>();
					docGraph.Document.Current = docGraph.Document.Search<POReceipt.receiptNbr>(row.POReceiptNbr);
					if (docGraph.Document.Current != null)
						throw new PXRedirectRequiredException(docGraph, true, "View PO Receipt"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
				}
			}
			return adapter.Get();
		}

		public PXAction<APInvoice> viewLCINDocument;
		[PXUIField(DisplayName = PO.Messages.ViewINDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable ViewLCINDocument(PXAdapter adapter)
		{
			LandedCostTran doc = this.landedCostTrans.Current;
			if (doc != null && String.IsNullOrEmpty(doc.INRefNbr) != true)
			{
				INAdjustmentEntry inReceiptGraph = PXGraph.CreateInstance<INAdjustmentEntry>();
				inReceiptGraph.adjustment.Current = inReceiptGraph.adjustment.Search<INRegister.refNbr>(doc.INRefNbr);
				throw new PXRedirectRequiredException(inReceiptGraph, true, PO.Messages.Document) { Mode = PXBaseRedirectException.WindowMode.NewWindow };

			}
			return adapter.Get();
		}


		public PXAction<APInvoice> addPostLandedCostTran;
		[PXUIField(DisplayName = Messages.AddPostponedLandedCost, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable AddPostLandedCostTran(PXAdapter adapter)
		{
			if (this.Document.Current != null &&
					this.Document.Current.Released == false &&
				this.landedCostTranSelection.AskExt() == WebDialogResult.OK)
			{
				foreach (LandedCostTranR rc in landedCostTranSelection.Cache.Updated)
				{
					if (rc.Selected == true)
					{
						LandedCostTran newTran = (LandedCostTran) this.landedCostTrans.Cache.CreateCopy(rc);
						newTran.APDocType = this.Document.Current.DocType;
						newTran.APRefNbr = this.Document.Current.RefNbr;
						newTran.APCuryInfoID = this.Document.Current.CuryInfoID;
						newTran = this.landedCostTrans.Update(newTran);	
					}
				}
			}
			poreceiptslist.View.Clear();
			poreceiptslist.Cache.Clear();
			return adapter.Get();
		}


		public PXAction<APInvoice> lsLCSplits;
		[PXUIField(DisplayName = Messages.LandedCostSplit, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]

		public virtual IEnumerable LsLCSplits(PXAdapter adapter)
		{
			LandedCostTran doc = this.landedCostTrans.Current;
			if (doc != null)
			{
				this.landedCostTrans.View.AskExt(true);
			}
			return adapter.Get();
		}
		#endregion


		#endregion

		#region Selects
		public PXSelect<InventoryItem, Where<InventoryItem.stkItem, Equal<False>, And<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>> nonStockItem;
        [PXCopyPasteHiddenFields(typeof(APInvoice.invoiceNbr), typeof(APInvoice.curyDiscTot))]
        public PXSelectJoin<APInvoice,
					LeftJoin<Vendor, On<Vendor.bAccountID, Equal<APInvoice.vendorID>>>,
					Where<APInvoice.docType, Equal<Optional<APInvoice.docType>>,
							And2<Where<APInvoice.origModule, NotEqual<BatchModule.moduleTX>, Or<APInvoice.released, Equal<True>>>,
							And<Where<Vendor.bAccountID, IsNull, Or<Match<Vendor, Current<AccessInfo.userName>>>>>>>> Document;
		[PXCopyPasteHiddenFields(typeof(APInvoice.paySel), typeof(APInvoice.payDate))]
		public PXSelect<APInvoice, Where<APInvoice.docType, Equal<Current<APInvoice.docType>>, And<APInvoice.refNbr, Equal<Current<APInvoice.refNbr>>>>> CurrentDocument;
		//Landed Cost must be higher then APTran for the correct work of DBLiteDefault attribute in APTran
		public PXSelect<LandedCostTran, Where<LandedCostTran.aPDocType, Equal<Current<APInvoice.docType>>,
											And<LandedCostTran.aPRefNbr, Equal<Current<APInvoice.refNbr>>,
											And<Where<LandedCostTran.source, Equal<LandedCostTranSource.fromAP>,
											Or<LandedCostTran.postponeAP, Equal<True>>>>>>, OrderBy<Asc<LandedCostTran.lCTranID>>> landedCostTrans;


		[PXImport(typeof(APInvoice))]
		public PXSelectJoin<APTran, LeftJoin<POReceiptLine, On<POReceiptLine.receiptNbr, Equal<APTran.receiptNbr>, 
										And<POReceiptLine.lineNbr, Equal<APTran.receiptLineNbr>>>>,
			Where<APTran.tranType, Equal<Current<APInvoice.docType>>,
				And<APTran.refNbr, Equal<Current<APInvoice.refNbr>>>>,
			OrderBy<Asc<APTran.tranType, Asc<APTran.refNbr, Asc<APTran.lineNbr>>>>>
			Transactions;

        public PXSelect<APTran, Where<APTran.tranType, Equal<Current<APInvoice.docType>>, And<APTran.refNbr, Equal<Current<APInvoice.refNbr>>, And<APTran.lineType, Equal<SOLineType.discount>>>>, OrderBy<Asc<APTran.tranType, Asc<APTran.refNbr, Asc<APTran.lineNbr>>>>> Discount_Row;

		public PXSelect<APTax, Where<APTax.tranType, Equal<Current<APInvoice.docType>>, And<APTax.refNbr, Equal<Current<APInvoice.refNbr>>>>, OrderBy<Asc<APTax.tranType, Asc<APTax.refNbr, Asc<APTax.taxID>>>>> Tax_Rows;		
        [PXCopyPasteHiddenFields(typeof(APTaxTran.refNbr))]
        public PXSelectJoin<APTaxTran, InnerJoin<Tax, On<Tax.taxID, Equal<APTaxTran.taxID>>>, Where<APTaxTran.module, Equal<BatchModule.moduleAP>, And<APTaxTran.tranType, Equal<Current<APInvoice.docType>>, And<APTaxTran.refNbr, Equal<Current<APInvoice.refNbr>>>>>> Taxes;
		[PXCopyPasteHiddenView()]
		public PXSelectJoin<APAdjust, InnerJoin<APPayment, On<APPayment.docType, Equal<APAdjust.adjgDocType>, And<APPayment.refNbr, Equal<APAdjust.adjgRefNbr>>>>> Adjustments;
		public PXSelectJoin<APAdjust, InnerJoin<APPayment, On<APPayment.docType, Equal<APAdjust.adjgDocType>, And<APPayment.refNbr, Equal<APAdjust.adjgRefNbr>>>, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APPayment.curyInfoID>>>>,
						Where<APAdjust.adjdDocType, Equal<Current<APInvoice.docType>>, And<APAdjust.adjdRefNbr, Equal<Current<APInvoice.refNbr>>,
							And<Where<Current<APInvoice.released>, Equal<boolTrue>, 
								Or<Current<APInvoice.prebooked>, Equal<boolTrue>, 
								Or<APAdjust.released, Equal<Current<APInvoice.released>>>>>>>>> Adjustments_Raw;

        public PXSelect<APInvoiceDiscountDetail, Where<APInvoiceDiscountDetail.docType, Equal<Current<APInvoice.docType>>, And<APInvoiceDiscountDetail.refNbr, Equal<Current<APInvoice.refNbr>>>>, OrderBy<Asc<APInvoiceDiscountDetail.docType, Asc<APInvoiceDiscountDetail.refNbr>>>> DiscountDetails;

		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APInvoice.curyInfoID>>>> currencyinfo;


		public CMSetupSelect CMSetup;
		public PXSetup<APSetup> APSetup;
		public PXSetup<Vendor, Where<Vendor.bAccountID, Equal<Optional<APInvoice.vendorID>>>> vendor;
		public PXSetup<EPEmployee, Where<EPEmployee.bAccountID, Equal<Optional<APInvoice.vendorID>>>> employee;
		public PXSetup<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>> vendorclass;
		public PXSetup<TaxZone, Where<TaxZone.taxZoneID, Equal<Current<APInvoice.taxZoneID>>>> taxzone;
		public PXSetup<Location, Where<Location.bAccountID, Equal<Current<APInvoice.vendorID>>, And<Location.locationID, Equal<Optional<APInvoice.vendorLocationID>>>>> location;
		public PXSetup<FinPeriod, Where<FinPeriod.finPeriodID, Equal<Current<APInvoice.finPeriodID>>>> finperiod;

		public PXSelect<POOrderRS> poorderslist;
		public PXSelect<POReceipt> poreceiptslist;
		public PXSelect<POReceiptLineR> receiptLinesUPD;
		public PXSelect<POLineAP> orderLinesUPD;
		

		public PXFilter<POReceiptFilter> filter;
        public PXFilter<POOrderFilter> pOrderFilter;
		public PXSelectJoin<POReceiptLineS, LeftJoin<APTran, On<APTran.receiptNbr, Equal<POReceiptLineS.receiptNbr>,
												And<APTran.receiptLineNbr, Equal<POReceiptLineS.lineNbr>,
												And<APTran.released, Equal<boolFalse>>>>,
                                            InnerJoin<POReceipt, On<POReceiptLineS.receiptNbr, Equal<POReceipt.receiptNbr>>>>,
								Where<POReceiptLineS.receiptNbr, Equal<Current<POReceiptFilter.receiptNbr>>,
								And<APTran.refNbr, IsNull,
								And<Where<POReceiptLineS.unbilledQty, Greater<decimal0>,
									Or<POReceiptLineS.curyUnbilledAmt, NotEqual<decimal0>>>>>>> poReceiptLinesSelection;

		public PXSelect<LandedCostTranR,Where<LandedCostTranR.vendorID,Equal<Current<APInvoice.vendorID>>,
			And<LandedCostTran.vendorLocationID,Equal<Current<APInvoice.vendorLocationID>>,
			And<LandedCostTran.curyID,Equal<Current<APInvoice.curyID>>>>>> landedCostTranSelection;

        public PXFilter<RecalcDiscountsParamFilter> recalcdiscountsfilter;

		public PXSelect<LandedCostTranSplit, Where<LandedCostTranSplit.lCTranID, Equal<Optional<LandedCostTran.lCTranID>>>> LCTranSplit;
		public PXSelect<AP1099Hist> ap1099hist;
		public PXSelect<AP1099Yr> ap1099year;

		public PXSetup<GLSetup> glsetup;
		public PXSetupOptional<TXAvalaraSetup> avalaraSetup;
		public PXSetupOptional<INSetup> insetup;

		public PXSelect<DRSchedule> dummySchedule_forPXParent;
		public PXSelect<DRScheduleDetail> dummyScheduleDetail_forPXParent;
		public PXSelect<DRScheduleTran> dummyScheduleTran_forPXParent;

		public PXSelect<EPExpenseClaim, Where<EPExpenseClaim.aPRefNbr, Equal<Current<APInvoice.refNbr>>>> expenseclaim;

        public PXFilter<AR.DuplicateFilter> duplicatefilter;

        public virtual IEnumerable transactions()
        {
            foreach (PXResult<APTran, POReceiptLine> tran in PXSelectJoin<APTran, LeftJoin<POReceiptLine, On<POReceiptLine.receiptNbr, Equal<APTran.receiptNbr>,
                                        And<POReceiptLine.lineNbr, Equal<APTran.receiptLineNbr>>>>,
            Where<APTran.tranType, Equal<Current<APInvoice.docType>>,
                And<APTran.refNbr, Equal<Current<APInvoice.refNbr>>>>,
            OrderBy<Asc<APTran.tranType, Asc<APTran.refNbr, Asc<APTran.lineNbr>>>>>.Select(this))
            {
                if (((APTran)tran).LineType != SOLineType.Discount)
                    yield return tran;
            }
        }

		#endregion

		#region Function
		public virtual void VoidPrepayment(APRegister doc)
		{
			APInvoice invoice = PXCache<APInvoice>.CreateCopy((APInvoice)doc);
			invoice.OpenDoc = false;
			invoice.Voided = true;
			Document.Update(invoice);
			Save.Press();
		}
		public virtual void ReverseInvoiceProc(APRegister doc)
		{
            AR.DuplicateFilter dfilter = PXCache<AR.DuplicateFilter>.CreateCopy(duplicatefilter.Current);
            WebDialogResult dialogRes = duplicatefilter.View.Answer;

			this.Clear(PXClearOption.PreserveTimeStamp);

			foreach (PXResult<APInvoice, CurrencyInfo, Terms, Vendor> res in APInvoice_CurrencyInfo_Terms_Vendor.Select(this, (object)doc.DocType, doc.RefNbr, doc.VendorID))
			{
				CurrencyInfo info = PXCache<CurrencyInfo>.CreateCopy((CurrencyInfo)res);
				info.CuryInfoID = null;
				info.IsReadOnly = false;
				info = PXCache<CurrencyInfo>.CreateCopy(this.currencyinfo.Insert(info));

				APInvoice invoice = PXCache<APInvoice>.CreateCopy((APInvoice)res);
				invoice.CuryInfoID = info.CuryInfoID;
				invoice.DocType = APDocType.DebitAdj;
				invoice.RefNbr = null;

				//must set for _RowSelected
				invoice.OpenDoc = true;
				invoice.Released = false;
				invoice.Hold = false;
				invoice.BatchNbr = null;
				invoice.PrebookBatchNbr = null;
				invoice.Prebooked = false;
				invoice.ScheduleID = null;
				invoice.Scheduled = false;
				invoice.NoteID = null;

				invoice.TermsID = null;
				invoice.InstallmentCntr = null;
				invoice.InstallmentNbr = null;
				invoice.DueDate = null;
				invoice.DiscDate = null;
				invoice.CuryOrigDiscAmt = 0m;
				invoice.FinPeriodID = doc.FinPeriodID;
				invoice.PaySel = false;
				invoice.PayTypeID = null;
				invoice.PayDate = null;
				invoice.PayAccountID = null;
				invoice.CuryDocBal = invoice.CuryOrigDocAmt;
				invoice.CuryLineTotal = 0m;
				invoice.IsTaxPosted = false;
				invoice.IsTaxValid = false;
                invoice.CuryVatTaxableTotal = 0m;
                invoice.CuryVatExemptTotal = 0m;

				invoice = this.Document.Insert(invoice);

				if (invoice.RefNbr == null)
				{
					//manual numbering, check for occasional duplicate
					APInvoice duplicate = PXSelect<APInvoice>.Search<APInvoice.docType, APInvoice.refNbr>(this, invoice.DocType, invoice.OrigRefNbr);
					if (duplicate != null)
					{
                        PXCache<AR.DuplicateFilter>.RestoreCopy(duplicatefilter.Current, dfilter);
                        duplicatefilter.View.Answer = dialogRes;
                        if (duplicatefilter.AskExt() == WebDialogResult.OK)
                        {
                            duplicatefilter.Cache.Clear();
                            if (duplicatefilter.Current.RefNbr == null)
                                throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AR.DuplicateFilter.refNbr).Name);
                            duplicate = PXSelect<APInvoice>.Search<APInvoice.docType, APInvoice.refNbr>(this, invoice.DocType, duplicatefilter.Current.RefNbr);
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
					CurrencyInfo b_info = (CurrencyInfo)PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APInvoice.curyInfoID>>>>.Select(this, null);
					b_info.CuryID = info.CuryID;
					b_info.CuryEffDate = info.CuryEffDate;
					b_info.CuryRateTypeID = info.CuryRateTypeID;
					b_info.CuryRate = info.CuryRate;
					b_info.RecipRate = info.RecipRate;
					b_info.CuryMultDiv = info.CuryMultDiv;
					this.currencyinfo.Update(b_info);
				}
			}

			TaxAttribute.SetTaxCalc<APTran.taxCategoryID, APTaxAttribute>(this.Transactions.Cache, null, TaxCalc.ManualCalc);

			foreach (APTran tran in PXSelect<APTran, Where<APTran.tranType, Equal<Required<APTran.tranType>>, And<APTran.refNbr, Equal<Required<APTran.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
			{
				tran.TranType = null;
				tran.RefNbr = null;
				tran.DrCr = null;
				tran.Released = null;
				tran.CuryInfoID = null;

				if (!string.IsNullOrEmpty(tran.DeferredCode))
				{
					DRSchedule schedule = PXSelect<DRSchedule,
						Where<DRSchedule.module, Equal<moduleAP>,
						And<DRSchedule.docType, Equal<Required<DRSchedule.docType>>,
						And<DRSchedule.refNbr, Equal<Required<DRSchedule.refNbr>>,
						And<DRSchedule.lineNbr, Equal<Required<DRSchedule.lineNbr>>>>>>>.Select(this, doc.DocType, doc.RefNbr, tran.LineNbr);

					if (schedule != null)
					{
						tran.DefScheduleID = schedule.ScheduleID;
					}
				}

				this.Transactions.Insert(tran);
			}

			if (!IsExternalTax)
			{
				foreach (APTaxTran tax in PXSelect<APTaxTran, Where<APTaxTran.tranType, Equal<Required<APTaxTran.tranType>>, And<APTaxTran.refNbr, Equal<Required<APTaxTran.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
				{
					APTaxTran new_aptax = new APTaxTran();
					new_aptax.TaxID = tax.TaxID;

					new_aptax = this.Taxes.Insert(new_aptax);

					if (new_aptax != null)
					{
						new_aptax = PXCache<APTaxTran>.CreateCopy(new_aptax);
						new_aptax.TaxRate = tax.TaxRate;
						new_aptax.CuryTaxableAmt = tax.CuryTaxableAmt;
						new_aptax.CuryTaxAmt = tax.CuryTaxAmt;
						new_aptax = this.Taxes.Update(new_aptax);
					}
				}
			}
		}

		private void PopulateBoxList()
		{
			List<int> AllowedValues = new List<int>();
			List<string> AllowedLabels = new List<string>();

			foreach (AP1099Box box in PXSelectReadonly<AP1099Box>.Select(this, null))
			{
				AllowedValues.Add((int)box.BoxNbr);
				StringBuilder bld = new StringBuilder(box.BoxNbr.ToString());
				bld.Append("-");
				bld.Append(box.Descr);
				AllowedLabels.Add(bld.ToString());
			}

			if (AllowedValues.Count > 0)
			{
				PXIntListAttribute.SetList<APTran.box1099>(Transactions.Cache, null, AllowedValues.ToArray(), AllowedLabels.ToArray());
			}
		}

		protected virtual void ParentFieldUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<APInvoice.docDate, APInvoice.finPeriodID, APInvoice.curyID>(e.Row, e.OldRow))
			{
				foreach (APTran tran in Transactions.Select())
				{
					if (Transactions.Cache.GetStatus(tran) == PXEntryStatus.Notchanged)
					{
						Transactions.Cache.SetStatus(tran, PXEntryStatus.Updated);
					}
				}

				foreach (APAdjust tran in Adjustments.Select())
				{
					if (Adjustments.Cache.GetStatus(tran) == PXEntryStatus.Notchanged)
					{
						Adjustments.Cache.SetStatus(tran, PXEntryStatus.Updated);
					}
				}
			}

			if (!sender.ObjectsEqual<APInvoice.docDate, APInvoice.curyID>(e.Row, e.OldRow))
			{
				foreach (LandedCostTran lcTran in this.landedCostTrans.Select())
				{
					if (this.landedCostTrans.Cache.GetStatus(lcTran) == PXEntryStatus.Notchanged)
					{
						this.landedCostTrans.Cache.SetStatus(lcTran, PXEntryStatus.Updated);
					}
				}
			}

		}

		private object GetAcctSub<Field>(PXCache cache, object data) where Field : IBqlField
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

		
		public APInvoiceEntry()
		{
			APSetup setup = APSetup.Current;
			PopulateBoxList();

			RowUpdated.AddHandler<APInvoice>(ParentFieldUpdated);

			this.poorderslist.Cache.AllowDelete = false;
			this.poorderslist.Cache.AllowInsert = false;
			this.poreceiptslist.Cache.AllowInsert = false;
			this.poreceiptslist.Cache.AllowDelete = false;
			this.poReceiptLinesSelection.Cache.AllowDelete = false;
			this.poReceiptLinesSelection.Cache.AllowInsert = false;
			this.landedCostTranSelection.Cache.AllowDelete = false;
			this.landedCostTranSelection.Cache.AllowInsert = false;
			PXUIFieldAttribute.SetEnabled(this.landedCostTranSelection.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<LandedCostTranR.selected>(this.landedCostTranSelection.Cache, null, true);

			PXUIFieldAttribute.SetDisplayName<APInvoice.paySel>(Document.Cache, Messages.ApprovedForPayment);
			PXUIFieldAttribute.SetVisible<APTran.projectID>(Transactions.Cache, null, PM.ProjectAttribute.IsPMVisible(this, BatchModule.AP));
			PXUIFieldAttribute.SetVisible<APTran.taskID>(Transactions.Cache, null, PM.ProjectAttribute.IsPMVisible(this, BatchModule.AP));
            PXUIFieldAttribute.SetVisible<APTran.nonBillable>(Transactions.Cache, null, PM.ProjectAttribute.IsPMVisible(this, BatchModule.AP));

            TaxAttribute.SetTaxCalc<APTran.taxCategoryID, APTaxAttribute>(Transactions.Cache, null, TaxCalc.ManualLineCalc);

			FieldDefaulting.AddHandler<InventoryItem.stkItem>((sender, e) => { if (e.Row != null) e.NewValue = false; });

            
		}

		public override void Persist()
		{
			foreach (APAdjust adj in Adjustments.Cache.Inserted)
			{
				if (adj.CuryAdjdAmt == 0m)
				{
					Adjustments.Cache.SetStatus(adj, PXEntryStatus.InsertedDeleted);
				}
			}

			foreach (APAdjust adj in Adjustments.Cache.Updated)
			{
				if (adj.CuryAdjdAmt == 0m)
				{
					Adjustments.Cache.SetStatus(adj, PXEntryStatus.Deleted);
				}
			}

			Adjustments.Cache.ClearQueryCache();

			foreach (APInvoice apdoc in Document.Cache.Cached)
			{
				if ((Document.Cache.GetStatus(apdoc) == PXEntryStatus.Inserted || Document.Cache.GetStatus(apdoc) == PXEntryStatus.Updated) && apdoc.DocType == APDocType.Invoice && (apdoc.Released == false && apdoc.Prebooked ==false))
				{
					decimal? CuryApplAmt = 0m;

					foreach (PXResult<APAdjust, APPayment, CurrencyInfo> res in Adjustments_Raw.View.SelectMultiBound(new object[] { apdoc }))
					{
						APAdjust adj = (APAdjust)res;

						if (adj != null)
						{
							CuryApplAmt += adj.CuryAdjdAmt;

							if (apdoc.CuryDocBal - CuryApplAmt < 0m)
							{
								if (Adjustments.Cache.GetStatus(adj) == PXEntryStatus.Notchanged)
								{
									Adjustments.Cache.SetStatus(adj, PXEntryStatus.Updated);
								}
								Adjustments.Cache.RaiseExceptionHandling<APAdjust.curyAdjdAmt>(adj, adj.CuryAdjdAmt, new PXSetPropertyException(Messages.Application_Amount_Cannot_Exceed_Document_Amount));
								throw new PXException(Messages.Application_Amount_Cannot_Exceed_Document_Amount);
							}
						}
					}
				}
			}

			base.Persist();

			if (Document.Current != null && IsExternalTax == true && Document.Current.IsTaxValid != true && !skipAvalaraCallOnSave)
			{
				PXLongOperation.StartOperation(this, delegate()
				{
					APInvoice doc = new APInvoice();
					doc.DocType = Document.Current.DocType;
					doc.RefNbr = Document.Current.RefNbr;
					doc.OrigModule = Document.Current.OrigModule;
					APExternalTaxCalc.Process(doc);
				});

			}
		}

		public virtual IEnumerable adjustments()
		{
			CurrencyInfo inv_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APInvoice.curyInfoID>>>>.Select(this);

			foreach (PXResult<APAdjust, APPayment, CurrencyInfo> res in Adjustments_Raw.Select())
			{
				APPayment payment = (APPayment)res;
				APAdjust adj = (APAdjust)res;
				CurrencyInfo pay_info = (CurrencyInfo)res;

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

			if (Document.Current != null && (Document.Current.DocType == APDocType.Invoice || Document.Current.DocType == APDocType.CreditAdj) && Document.Current.Released == false && Document.Current.Prebooked == false)
			{
				using (ReadOnlyScope rs = new ReadOnlyScope(Adjustments.Cache, Document.Cache))
				{
					foreach (PXResult<APPayment, CurrencyInfo, APAdjust> res in PXSelectJoin<APPayment, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APPayment.curyInfoID>>, LeftJoin<APAdjust, On<APAdjust.adjgDocType, Equal<APPayment.docType>, And<APAdjust.adjgRefNbr, Equal<APPayment.refNbr>, And<APAdjust.adjNbr, Equal<APPayment.lineCntr>, And<APAdjust.released, Equal<boolFalse>, And<Where<APAdjust.adjdDocType, NotEqual<Current<APInvoice.docType>>, Or<APAdjust.adjdRefNbr, NotEqual<Current<APInvoice.refNbr>>>>>>>>>>>, Where<APPayment.vendorID, Equal<Current<APInvoice.vendorID>>, And2<Where<APPayment.docType, Equal<APDocType.prepayment>, Or<APPayment.docType, Equal<APDocType.debitAdj>>>, And<APPayment.docDate, LessEqual<Current<APInvoice.docDate>>, And<APPayment.finPeriodID, LessEqual<Current<APInvoice.finPeriodID>>, And<APPayment.released, Equal<boolTrue>, And<APPayment.openDoc, Equal<boolTrue>, And<APAdjust.adjdRefNbr, IsNull>>>>>>>>.Select(this))
					{
						APPayment payment = (APPayment)res;
						APAdjust adj = new APAdjust();
						CurrencyInfo pay_info = (CurrencyInfo)res;

						adj.VendorID = Document.Current.VendorID;
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

							yield return new PXResult<APAdjust, APPayment>(Adjustments.Insert(adj), payment);
						}
					}
				}
			}
		}
		#endregion

		#region APInvoice Events

		protected virtual void APInvoice_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = APDocType.Invoice;
		}

		protected virtual void APInvoice_APAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (location.Current != null && e.Row != null)
			{
				e.NewValue = null;
				if (((APInvoice)e.Row).DocType == APDocType.Prepayment)
				{
					e.NewValue = GetAcctSub<Vendor.prepaymentAcctID>(vendor.Cache, vendor.Current);
				}
				if (string.IsNullOrEmpty((string)e.NewValue))
				{
					e.NewValue = GetAcctSub<Location.aPAccountID>(location.Cache, location.Current);
				}
			}
		}

		protected virtual void APInvoice_APSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (location.Current != null && e.Row != null)
			{
				e.NewValue = null;
				if (((APInvoice)e.Row).DocType == APDocType.Prepayment)
				{
					e.NewValue = GetAcctSub<Vendor.prepaymentSubID>(vendor.Cache, vendor.Current);
				}
				if (string.IsNullOrEmpty((string)e.NewValue))
				{
					e.NewValue = GetAcctSub<Location.aPSubID>(location.Cache, location.Current);
				}
			}
		}

		protected virtual void APInvoice_VendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			location.RaiseFieldUpdated(sender, e.Row);

			sender.SetDefaultExt<APInvoice.aPAccountID>(e.Row);
			sender.SetDefaultExt<APInvoice.aPSubID>(e.Row);
			sender.SetDefaultExt<APInvoice.termsID>(e.Row);
			sender.SetDefaultExt<APInvoice.branchID>(e.Row);
			sender.SetValue<APInvoice.payLocationID>(e.Row, sender.GetValue<APInvoice.vendorLocationID>(e.Row));
			sender.SetDefaultExt<APInvoice.payTypeID>(e.Row);
			sender.SetDefaultExt<APInvoice.separateCheck>(e.Row);
			sender.SetDefaultExt<APInvoice.taxZoneID>(e.Row);
			sender.SetDefaultExt<APInvoice.prebookAcctID>(e.Row);
			sender.SetDefaultExt<APInvoice.prebookSubID>(e.Row);
		}

		protected virtual void APInvoice_PayLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<APInvoice.separateCheck>(e.Row);
			sender.SetDefaultExt<APInvoice.payTypeID>(e.Row);

		}

		protected virtual void APInvoice_TermsID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APInvoice invoice = (APInvoice)e.Row;

			if (invoice != null && (invoice.DocType == APDocType.DebitAdj || invoice.DocType == APDocType.Prepayment))
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void APInvoice_VendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APInvoice invoice = (APInvoice)e.Row;

			vendor.RaiseFieldUpdated(sender, e.Row);

			if ((bool)CMSetup.Current.MCActivated)
			{
				CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<APInvoice.curyInfoID>(sender, invoice);

				string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);
				if (string.IsNullOrEmpty(message) == false)
				{
					sender.RaiseExceptionHandling<APInvoice.docDate>(invoice, invoice.DocDate, new PXSetPropertyException(message, PXErrorLevel.Warning));
				}

				if (info != null)
				{
					invoice.CuryID = info.CuryID;
				}
			}
           
			sender.SetDefaultExt<APInvoice.vendorLocationID>(invoice);
			sender.SetDefaultExt<APInvoice.termsID>(invoice);

			object InvoiceNbr = invoice.InvoiceNbr;
			sender.RaiseFieldVerifying<APInvoice.invoiceNbr>(invoice, ref InvoiceNbr);
		}

		protected virtual void APInvoice_InvoiceNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (string.IsNullOrEmpty((string)e.NewValue) == false)
			{
				APInvoice dup = PXSelectReadonly<APInvoice, Where<APInvoice.vendorID, Equal<Current<APInvoice.vendorID>>, And<APInvoice.invoiceNbr, Equal<Required<APInvoice.invoiceNbr>>, And<APInvoice.voided, Equal<boolFalse>, And<Where<APInvoice.docType, NotEqual<Current<APInvoice.docType>>, Or<APInvoice.refNbr, NotEqual<Current<APInvoice.refNbr>>>>>>>>>.Select(this, e.NewValue);
				if (dup != null)
				{
					if (APSetup.Current.RaiseErrorOnDoubleInvoiceNbr == true)
					{
						//throw new PXSetPropertyException(Messages.DuplicateVendorRefDoc, dup.RefNbr, dup.DocDate);
						sender.RaiseExceptionHandling<APInvoice.invoiceNbr>(e.Row, e.NewValue, new PXSetPropertyException(Messages.DuplicateVendorRefDoc, PXErrorLevel.Error, dup.RefNbr, dup.DocDate));
						bool thesame = (string)e.NewValue == (string)sender.GetValue<APInvoice.invoiceNbr>(e.Row);
						e.NewValue = sender.GetValue<APInvoice.invoiceNbr>(e.Row);
						if (thesame)
						{
							sender.SetValue<APInvoice.invoiceNbr>(e.Row, null);
						}
					}
					else
					{
						sender.RaiseExceptionHandling<APInvoice.invoiceNbr>(e.Row, e.NewValue, new PXSetPropertyException(Messages.DuplicateVendorRefDoc, PXErrorLevel.Warning, dup.RefNbr, dup.DocDate));
					}
				}
			}
		}

		protected virtual void APInvoice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			APInvoice doc = (APInvoice)e.Row;

			if (doc.DocType != APDocType.DebitAdj && doc.DocType != APDocType.Prepayment && string.IsNullOrEmpty(doc.TermsID))
			{
				sender.RaiseExceptionHandling<APInvoice.termsID>(doc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
			}

			Terms terms = (Terms)PXSelectorAttribute.Select<APInvoice.termsID>(Document.Cache, doc);

			if (vendor.Current != null && (bool)vendor.Current.Vendor1099 && terms != null)
			{

				if (terms.InstallmentType == CS.TermsInstallmentType.Multiple)
				{
					sender.RaiseExceptionHandling<APInvoice.termsID>(doc, doc.TermsID, new PXSetPropertyException(Messages.AP1099_Vendor_Cannot_Have_Multiply_Installments));
				}
			}

			EPEmployee emp = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<APInvoice.vendorID>>>>.Select(this);
			if (emp != null && terms != null)
			{
				if (PXCurrencyAttribute.IsNullOrEmpty(terms.DiscPercent) == false)
				{
					sender.RaiseExceptionHandling<APInvoice.termsID>(doc, doc.TermsID, new PXSetPropertyException(Messages.Employee_Cannot_Have_Discounts));
				}

				if (terms.InstallmentType == TermsInstallmentType.Multiple)
				{
					sender.RaiseExceptionHandling<APInvoice.termsID>(e.Row, doc.TermsID, new PXSetPropertyException(Messages.Employee_Cannot_Have_Multiply_Installments));
				}
			}

			if (doc.DocType != APDocType.DebitAdj && doc.DueDate == null)
			{
				sender.RaiseExceptionHandling<APInvoice.dueDate>(doc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
			}

			if (doc.DocType != APDocType.DebitAdj && doc.PaySel == true && doc.PayDate == null)
			{
				sender.RaiseExceptionHandling<APInvoice.payDate>(doc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
			}

			if (doc.DocType != APDocType.DebitAdj && doc.PaySel == true && doc.PayDate != null && ((DateTime)doc.DocDate).CompareTo((DateTime)doc.PayDate) > 0)
			{
				sender.RaiseExceptionHandling<APInvoice.payDate>(e.Row, doc.PayDate, new PXSetPropertyException(Messages.ApplDate_Less_DocDate, PXErrorLevel.RowError));
			}

			if (doc.DocType != APDocType.DebitAdj && doc.PaySel == true && doc.PayLocationID == null)
			{
				sender.RaiseExceptionHandling<APInvoice.payLocationID>(doc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
			}

			if (doc.DocType != APDocType.DebitAdj && doc.PaySel == true && doc.PayAccountID == null)
			{
				sender.RaiseExceptionHandling<APInvoice.payAccountID>(doc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
			}

			if (doc.DocType != APDocType.DebitAdj && doc.PaySel == true && doc.PayTypeID == null)
			{
				sender.RaiseExceptionHandling<APInvoice.payTypeID>(doc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
			}

			if (doc.DocType == APDocType.Prepayment && doc.PaySel == true && doc.PayAccountID != null)
			{
				object PayAccountID = doc.PayAccountID;

				try
				{
					sender.RaiseFieldVerifying<APInvoice.payAccountID>(doc, ref PayAccountID);
				}
				catch (PXSetPropertyException ex)
				{
					sender.RaiseExceptionHandling<APInvoice.payAccountID>(doc, PayAccountID, ex);
				}
			}

			if (doc.DocType != APDocType.DebitAdj && doc.DocType != APDocType.Prepayment && doc.DiscDate == null)
			{
				sender.RaiseExceptionHandling<APInvoice.discDate>(doc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
			}

			if (doc.DocType != APDocType.DebitAdj && doc.DocType != APDocType.CreditAdj && doc.DocType != APDocType.Prepayment && (vendor.Current == null || (bool)vendor.Current.TaxAgency == false) && string.IsNullOrEmpty(doc.InvoiceNbr) && (bool)APSetup.Current.RequireVendorRef)
			{
				sender.RaiseExceptionHandling<APInvoice.invoiceNbr>(doc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
			}

			if (doc.DocType == APDocType.Prepayment && doc.OpenDoc == true && doc.Voided == true)
			{
				doc.OpenDoc = false;
				doc.ClosedFinPeriodID = doc.FinPeriodID;
				doc.ClosedTranPeriodID = doc.TranPeriodID;
			}

            if (doc.CuryDiscTot > doc.CuryLineTotal)
            {
                sender.RaiseExceptionHandling<APInvoice.curyDiscTot>(e.Row, doc.CuryDiscTot, new PXSetPropertyException(Messages.DiscountGreaterLineTotal, PXErrorLevel.Error));
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
		}

		protected virtual void APInvoice_DocDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APInvoice doc = (APInvoice)e.Row;

			CurrencyInfoAttribute.SetEffectiveDate<APInvoice.docDate>(sender, e);

            if (doc.DocType == APDocType.Prepayment)// && doc.DueDate != null && DateTime.Compare((DateTime)doc.DocDate, (DateTime)doc.DueDate) > 0)
            {
                sender.SetDefaultExt<APInvoice.dueDate>(doc);
            }
		}

		protected virtual void APInvoice_DueDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APInvoice invoice = (APInvoice)e.Row;
			if (invoice.DocType == APDocType.Prepayment)
			{
				e.NewValue = invoice.DocDate;
			}
		}

		protected virtual void APInvoice_TermsID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Terms terms = (Terms)PXSelectorAttribute.Select<APInvoice.termsID>(sender, e.Row);

			if (terms != null && terms.InstallmentType != TermsInstallmentType.Single)
			{
				foreach (APAdjust adj in Adjustments.Select())
				{
					Adjustments.Cache.Delete(adj);
				}
			}
		}

		protected virtual void APInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			APInvoice doc = e.Row as APInvoice;

			if (doc == null) return;
			PXUIFieldAttribute.SetRequired<APInvoice.invoiceNbr>(cache, (bool)this.APSetup.Current.RequireVendorRef);
			PXUIFieldAttribute.SetVisible<APInvoice.curyID>(cache, doc, (bool)CMSetup.Current.MCActivated);

			bool docTypeNotDebitAdj = (doc.DocType != APDocType.DebitAdj);
			bool docTypePrepayment = (doc.DocType == APDocType.Prepayment);

			PXUIFieldAttribute.SetRequired<APInvoice.termsID>(cache, (docTypeNotDebitAdj && !docTypePrepayment));
			PXUIFieldAttribute.SetRequired<APInvoice.dueDate>(cache, (docTypeNotDebitAdj));
			PXUIFieldAttribute.SetRequired<APInvoice.discDate>(cache, (docTypeNotDebitAdj && !docTypePrepayment));

			bool isInvoice = (doc.DocType == APInvoiceType.Invoice);
			bool isDebitAdj = (doc.DocType == APInvoiceType.DebitAdj);

			this.addPOOrder.SetVisible(isInvoice);
			this.addPOReceipt.SetVisible(isInvoice || isDebitAdj);
			this.addReceiptLine.SetVisible(isInvoice);
			this.viewPODocument.SetVisible(isInvoice || isDebitAdj);
			this.addPostLandedCostTran.SetVisible(isInvoice);
			this.lsLCSplits.SetVisible(isInvoice);

			this.prebook.SetVisible(doc.DocType != APDocType.Prepayment);
			this.voidInvoice.SetVisible(doc.DocType != APDocType.Prepayment); 

			this.addPOReceipt.SetEnabled(false);
			this.addPOOrder.SetEnabled(false);
			this.addReceiptLine.SetEnabled(false);
			this.prebook.SetEnabled(false);
			this.voidInvoice.SetEnabled(false);

			this.addPostLandedCostTran.SetEnabled(false);
			this.lsLCSplits.SetEnabled(false);

			bool curyenabled = true;
			bool vendor1099 = false;

			PXUIFieldAttribute.SetEnabled(this.poReceiptLinesSelection.Cache, null, false);
			PXUIFieldAttribute.SetEnabled(this.poreceiptslist.Cache, null, false);
			PXUIFieldAttribute.SetEnabled(this.poorderslist.Cache, null, false);

			if (vendor.Current != null)
			{
				if (vendor.Current.AllowOverrideCury != true)
					curyenabled = false;
				if (vendor.Current.Vendor1099 == true)
					vendor1099 = true;
				if (vendor.Current.TaxAgency == true)
				{
					PXUIFieldAttribute.SetEnabled<APInvoice.taxZoneID>(cache, doc, false);
					PXUIFieldAttribute.SetEnabled<APTran.taxCategoryID>(Transactions.Cache, null, false);
				}
			}
			bool landedCostEnabled = false;
			if (doc.VendorID.HasValue && doc.VendorLocationID.HasValue && (isInvoice || isDebitAdj))
			{
				if (this.vendor.Current != null)
				{
					landedCostEnabled = ((bool)this.vendor.Current.LandedCostVendor) || (this.landedCostTrans.Select().Count > 0);
				}
			}
			doc.LCEnabled = landedCostEnabled;
			bool isPrebookedNotCompleted = (doc.Prebooked == true && doc.Released == false && doc.Voided==false);
			if ((bool)doc.Released || (bool)doc.Voided || doc.Prebooked == true)
			{
				bool Enable1099 = (vendor.Current != null && vendor.Current.Vendor1099 == true && doc.Voided == false);
				bool hasAdjustments = false;
				foreach (APAdjust adj in Adjustments.Select())
				{
					string Year1099 = ((DateTime)adj.AdjgDocDate).Year.ToString();

					AP1099Year year = PXSelect<AP1099Year, Where<AP1099Year.finYear, Equal<Required<AP1099Year.finYear>>>>.Select(this, Year1099);

					if (year != null && year.Status != "N" || adj.AdjDiscAmt != 0m)
					{
						Enable1099 = false;
					}
					hasAdjustments = true;
				}
				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<APInvoice.dueDate>(cache, doc, (docTypeNotDebitAdj) && (bool)doc.OpenDoc);
				PXUIFieldAttribute.SetEnabled<APInvoice.paySel>(cache, doc, (docTypeNotDebitAdj) && (bool)doc.OpenDoc);
				PXUIFieldAttribute.SetEnabled<APInvoice.payLocationID>(cache, doc, (docTypeNotDebitAdj) && (bool)doc.OpenDoc);
				PXUIFieldAttribute.SetEnabled<APInvoice.payAccountID>(cache, doc, (docTypeNotDebitAdj) && (bool)doc.OpenDoc);
				PXUIFieldAttribute.SetEnabled<APInvoice.payTypeID>(cache, doc, (docTypeNotDebitAdj) && (bool)doc.OpenDoc);
				PXUIFieldAttribute.SetEnabled<APInvoice.payDate>(cache, doc, (docTypeNotDebitAdj) && (bool)doc.OpenDoc);
				PXUIFieldAttribute.SetEnabled<APInvoice.discDate>(cache, doc, (docTypeNotDebitAdj && !docTypePrepayment) && (bool)doc.OpenDoc);

				cache.AllowDelete = false;
				cache.AllowUpdate = (bool)doc.OpenDoc || Enable1099 || isPrebookedNotCompleted;
				Transactions.Cache.AllowDelete = false;
				Transactions.Cache.AllowUpdate = Enable1099 || isPrebookedNotCompleted;
				Transactions.Cache.AllowInsert = false;

                DiscountDetails.Cache.AllowDelete = false;
                DiscountDetails.Cache.AllowUpdate = false;
                DiscountDetails.Cache.AllowInsert = false;

				Taxes.Cache.AllowUpdate = false;
				release.SetEnabled(isPrebookedNotCompleted);
				bool hasPOLinks = false;
				if (!hasAdjustments)
				{
					APTran tran = PXSelectReadonly<APTran, Where<APTran.tranType, Equal<Required<APTran.tranType>>, And<APTran.refNbr, Equal<Required<APTran.refNbr>>,
													And<Where<APTran.lCTranID, IsNotNull, Or<APTran.pONbr, IsNotNull, Or<APTran.receiptNbr, IsNotNull>>>>>>>.Select(this, doc.DocType, doc.RefNbr);
					hasPOLinks = (tran != null);
				}
				if (this._allowToVoidReleased)
				{
					voidInvoice.SetEnabled((doc.Released == true || doc.Prebooked == true) && !hasPOLinks && doc.Voided == false && !hasAdjustments);
				}
				else
				{
					voidInvoice.SetEnabled(isPrebookedNotCompleted && !hasPOLinks);
				}
				
				createSchedule.SetEnabled(false);
                payInvoice.SetEnabled((bool)doc.OpenDoc && ((bool)doc.Payable || doc.DocType == APInvoiceType.Prepayment));
				this.landedCostTrans.Cache.AllowDelete = false;
				this.landedCostTrans.Cache.AllowUpdate = false;
				this.landedCostTrans.Cache.AllowInsert = false;

				if (Enable1099 || isPrebookedNotCompleted)
				{
					PXUIFieldAttribute.SetEnabled(Transactions.Cache, null, false);
					PXUIFieldAttribute.SetEnabled<APTran.box1099>(Transactions.Cache, null, Enable1099);

					PXUIFieldAttribute.SetEnabled<APTran.accountID>(Transactions.Cache, null, isPrebookedNotCompleted);
					PXUIFieldAttribute.SetEnabled<APTran.subID>(Transactions.Cache, null, isPrebookedNotCompleted);
					PXUIFieldAttribute.SetEnabled<APTran.branchID>(Transactions.Cache, null, isPrebookedNotCompleted);
                    PXUIFieldAttribute.SetEnabled<APTran.projectID>(Transactions.Cache, null, isPrebookedNotCompleted);
                    PXUIFieldAttribute.SetEnabled<APTran.taskID>(Transactions.Cache, null, isPrebookedNotCompleted);
				}
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<APInvoice.status>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<APInvoice.curyDocBal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<APInvoice.curyLineTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<APInvoice.curyTaxTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<APInvoice.curyOrigWhTaxAmt>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<APInvoice.curyVatExemptTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<APInvoice.curyVatTaxableTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<APInvoice.batchNbr>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<APInvoice.curyID>(cache, doc, curyenabled);
				PXUIFieldAttribute.SetEnabled<APInvoice.hold>(cache, doc, ((bool)doc.Scheduled == false));

				PXUIFieldAttribute.SetEnabled<APInvoice.termsID>(cache, doc, (docTypeNotDebitAdj && !docTypePrepayment));
				PXUIFieldAttribute.SetEnabled<APInvoice.dueDate>(cache, doc, (docTypeNotDebitAdj));
				PXUIFieldAttribute.SetEnabled<APInvoice.discDate>(cache, doc, (docTypeNotDebitAdj && !docTypePrepayment));
				PXUIFieldAttribute.SetEnabled<APInvoice.curyOrigDiscAmt>(cache, doc, (docTypeNotDebitAdj && !docTypePrepayment));

				//PXUIFieldAttribute.SetEnabled<APTran.deferredCode>(Transactions.Cache, null, (doc.DocType == APDocType.Invoice || doc.DocType == APDocType.CreditAdj));
				PXUIFieldAttribute.SetEnabled<APTran.defScheduleID>(Transactions.Cache, null, (doc.DocType == APDocType.DebitAdj));

				//calculate only on data entry, differences from the applications will be moved to RGOL upon closure
				PXDBCurrencyAttribute.SetBaseCalc<APInvoice.curyDocBal>(cache, null, true);
				PXDBCurrencyAttribute.SetBaseCalc<APInvoice.curyDiscBal>(cache, null, true);

				PXUIFieldAttribute.SetEnabled<APInvoice.payAccountID>(cache, doc, (docTypeNotDebitAdj) && (bool)doc.OpenDoc);
				PXUIFieldAttribute.SetEnabled<APInvoice.payTypeID>(cache, doc, (docTypeNotDebitAdj) && (bool)doc.OpenDoc);
				PXUIFieldAttribute.SetEnabled<APInvoice.payDate>(cache, doc, (docTypeNotDebitAdj));

				bool hasLCTrans = this.landedCostTrans.Select().Count > 0;
				PXUIFieldAttribute.SetEnabled<APInvoice.vendorID>(cache, doc, !hasLCTrans);
				PXUIFieldAttribute.SetEnabled<APInvoice.vendorLocationID>(cache, doc, !hasLCTrans);


				cache.AllowDelete = true;
				cache.AllowUpdate = true;
				Transactions.Cache.AllowDelete = true;
				Transactions.Cache.AllowUpdate = true;
				Transactions.Cache.AllowInsert = (doc.VendorID != null) && (doc.VendorLocationID != null);

                if (Document.Current != null && Document.Current.SkipDiscounts == true)
                {
                    DiscountDetails.Cache.AllowDelete = false;
                    DiscountDetails.Cache.AllowUpdate = false;
                    DiscountDetails.Cache.AllowInsert = false;
                }
                else
                {
                    DiscountDetails.Cache.AllowDelete = true;
                    DiscountDetails.Cache.AllowUpdate = true;
                    DiscountDetails.Cache.AllowInsert = true;
                }

				this.landedCostTrans.Cache.AllowDelete = true;
				Vendor vnd = this.vendor.Select();

				if (vnd != null)
				{
					bool isLCVendor = (bool)vnd.LandedCostVendor;
					this.landedCostTrans.Cache.AllowUpdate = isLCVendor;
					this.landedCostTrans.Cache.AllowInsert = isLCVendor;
					if (hasLCTrans && !isLCVendor)
					{
						cache.RaiseExceptionHandling<APInvoice.vendorID>(doc, vnd.AcctCD, new PXSetPropertyKeepPreviousException(Messages.APLanededCostTranForNonLCVendor, PXErrorLevel.Warning));
					}
					this.addPostLandedCostTran.SetEnabled(isLCVendor);
                    if (vnd.Status == Vendor.status.Inactive)
                    {
                        cache.RaiseExceptionHandling<APInvoice.vendorID>(doc, vnd.AcctCD, new PXSetPropertyException(Messages.VendorIsInStatus, PXErrorLevel.Warning, CR.Messages.Inactive));
                    }
				}
				


				bool docNotHold = (doc.Hold != true);
				bool docNotScheduled = (doc.Scheduled != true);

				Taxes.Cache.AllowUpdate = true;
				release.SetEnabled(docNotHold && docNotScheduled);
				this.prebook.SetEnabled(doc.DocType != APDocType.Prepayment && docNotHold && docNotScheduled);
				createSchedule.SetEnabled(docNotHold && doc.DocType == APDocType.Invoice);
				payInvoice.SetEnabled(false);

				this.addPOReceipt.SetEnabled(true);
				this.addPOOrder.SetEnabled(true);
				this.addReceiptLine.SetEnabled(true);

				PXUIFieldAttribute.SetEnabled<POReceiptLineS.selected>(this.poReceiptLinesSelection.Cache, null, true);
				PXUIFieldAttribute.SetEnabled<POReceipt.selected>(this.poreceiptslist.Cache, null, true);
				PXUIFieldAttribute.SetEnabled<POOrderRS.selected>(this.poorderslist.Cache, null, true);
			}
			bool docReleased = (doc.Released == true) || (doc.Prebooked == true);
			
			PXUIFieldAttribute.SetEnabled<APInvoice.docType>(cache, doc);
			PXUIFieldAttribute.SetEnabled<APInvoice.refNbr>(cache, doc);

			Taxes.Cache.AllowDelete = Transactions.Cache.AllowDelete;
			Taxes.Cache.AllowInsert = Transactions.Cache.AllowInsert;

			Adjustments.Cache.AllowInsert = false;
			Adjustments.Cache.AllowDelete = false;
			Adjustments.Cache.AllowUpdate = Transactions.Cache.AllowUpdate && !isPrebookedNotCompleted;

			editVendor.SetEnabled(vendor != null && vendor.Current != null);

			reverseInvoice.SetEnabled(docTypeNotDebitAdj && docReleased && !docTypePrepayment);
			vendorRefund.SetEnabled(!docTypeNotDebitAdj && docReleased && !docTypePrepayment);

			if (doc.VendorID != null)
			{
				if (Transactions.Select().Count > 0)
				{
					PXUIFieldAttribute.SetEnabled<APInvoice.vendorID>(cache, doc, false);
				}               
			}
			if (doc.VendorLocationID != null)
			{
				bool hasLCTrans = this.landedCostTrans.Select().Count > 0;
				PXUIFieldAttribute.SetEnabled<APInvoice.vendorLocationID>(cache, doc, !(hasLCTrans || (bool)doc.Released || (bool)doc.Voided|| (bool) doc.Prebooked));
			}
			PXUIFieldAttribute.SetVisible<APInvoice.curyOrigDocAmt>(cache, doc, (bool)APSetup.Current.RequireControlTotal || (docReleased));
			PXUIFieldAttribute.SetVisible<APTran.box1099>(Transactions.Cache, null, vendor1099);
			PXUIFieldAttribute.SetVisible<APInvoice.prebookBatchNbr>(cache, doc, doc.Prebooked == true);
			PXUIFieldAttribute.SetVisible<APInvoice.voidBatchNbr>(cache, doc, doc.Voided == true);
			PXUIFieldAttribute.SetVisible<APInvoice.batchNbr>(cache, doc, doc.Voided != true);

			PXUIFieldAttribute.SetVisible<APInvoice.curyOrigDocAmt>(cache, doc, (bool)APSetup.Current.RequireControlTotal || docReleased);
			PXUIFieldAttribute.SetVisible<APTran.box1099>(Transactions.Cache, null, vendor1099);

            //this.Actions["addPOOrder"].SetEnabled(!docReleased && this.vendor.Current != null);
            this.addPOOrder.SetEnabled(!docReleased && this.vendor.Current != null);
            this.addPOReceipt.SetEnabled(!docReleased && this.vendor.Current != null);
            this.addReceiptLine.SetEnabled(!docReleased && this.vendor.Current != null);
			this.lsLCSplits.SetEnabled(landedCostEnabled);
			this.LCTranSplit.Cache.AllowInsert = this.LCTranSplit.Cache.AllowUpdate = this.LCTranSplit.Cache.AllowDelete = (landedCostEnabled && !docReleased);
			PXUIFieldAttribute.SetEnabled(this.LCTranSplit.Cache, null, (landedCostEnabled && !docReleased));

			if (IsExternalTax == true && ((APInvoice)e.Row).IsTaxValid != true)
				PXUIFieldAttribute.SetWarning<APInvoice.curyTaxTotal>(cache, e.Row, AR.Messages.TaxIsNotUptodate);
		}

		protected virtual void APInvoice_PayTypeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<APInvoice.payAccountID>(e.Row);
		}

		protected virtual void APInvoice_PayAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APInvoice doc = e.Row as APInvoice;
			if (doc == null) return;
			CashAccount cashacct = (CashAccount)PXSelectorAttribute.Select<APInvoice.payAccountID>(sender, doc, e.NewValue);

			if (cashacct != null && doc.DocType == APDocType.Prepayment && object.Equals(cashacct.CuryID, doc.CuryID) == false)
			{
                e.NewValue = cashacct.CashAccountCD;
				throw new PXSetPropertyException(Messages.CashCuryNotPPCury);
			}
		}

		protected virtual void APInvoice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			APInvoice doc = e.Row as APInvoice;
			if (doc == null) return;
            if (doc.Released != true)
            {
                if (e.ExternalCall && (!sender.ObjectsEqual<APInvoice.docDate>(e.OldRow, e.Row) || !sender.ObjectsEqual<APInvoice.skipDiscounts>(e.OldRow, e.Row)))
                {
                    DiscountEngine<APTran>.AutoRecalculatePricesAndDiscounts<APInvoiceDiscountDetail>(Transactions.Cache, Transactions, null, DiscountDetails, doc.VendorLocationID, doc.DocDate.Value);
                }
                if (sender.GetStatus(doc) != PXEntryStatus.Deleted && !sender.ObjectsEqual<APInvoice.curyDiscTot>(e.OldRow, e.Row))
                {
                    AddDiscount(sender, doc);
                }
                if (doc.Released != true && doc.Prebooked != true)
                {
                    if (APSetup.Current.RequireControlTotal != true)
                    {
                        if (doc.CuryDocBal != doc.CuryOrigDocAmt)
                        {
                            if (doc.CuryDocBal != null && doc.CuryDocBal != 0)
                                sender.SetValueExt<APInvoice.curyOrigDocAmt>(doc, doc.CuryDocBal);
                            else
                                sender.SetValueExt<APInvoice.curyOrigDocAmt>(doc, 0m);
                        }
                    }

                    if (doc.DocType == APDocType.Prepayment && doc.DueDate == null)
                    {
                        sender.SetValue<APInvoice.dueDate>(e.Row, this.Accessinfo.BusinessDate);
                    }
                }

                if (doc.Hold != true && doc.Released != true && doc.Prebooked != true)
                {
                    if (doc.CuryDocBal != doc.CuryOrigDocAmt)
                    {
                        sender.RaiseExceptionHandling<APInvoice.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, new PXSetPropertyException(Messages.DocumentOutOfBalance));
                    }
                    else if (doc.CuryOrigDocAmt < 0m)
                    {
                        if (APSetup.Current.RequireControlTotal == true)
                        {
                            sender.RaiseExceptionHandling<APInvoice.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
                        }
                        else
                        {
                            sender.RaiseExceptionHandling<APInvoice.curyDocBal>(doc, doc.CuryDocBal, new PXSetPropertyException(Messages.DocumentBalanceNegative));
                        }
                    }
                    else
                    {
                        if (APSetup.Current.RequireControlTotal == true)
                        {
                            sender.RaiseExceptionHandling<APInvoice.curyOrigDocAmt>(doc, null, null);
                        }
                        else
                        {
                            sender.RaiseExceptionHandling<APInvoice.curyDocBal>(doc, null, null);
                        }
                    }
                }
            }
		}

		protected virtual void APInvoice_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			EPExpenseClaim claim = expenseclaim.Select();
			if (claim != null)
			{
				if (claim.Status == EPClaimStatus.Released)
				{
					claim.Status = EPClaimStatus.Approved;
					claim.Released = false;
					claim.APDocType = null;
					claim.APRefNbr = null;
					expenseclaim.Update(claim);
				}
			}
		}
		
		#endregion

		#region APTran Events
		protected virtual void APTran_AccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APTran row = (APTran)e.Row;

			
			if (vendor.Current == null || row == null || row.InventoryID != null) return;
			
			switch (vendor.Current.Type)
			{
				case BAccountType.VendorType:
				case BAccountType.CombinedType:
					if (location.Current.VExpenseAcctID != null)
					{
						e.NewValue = location.Current.VExpenseAcctID;
					}
					break;
				case BAccountType.EmployeeType:
					if (employee.Current.ExpenseAcctID != null)
					{
						e.NewValue = employee.Current.ExpenseAcctID;
					}
					break;
			}
			e.Cancel = true;
		}

		protected virtual void APTran_AccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (vendor.Current != null && (bool)vendor.Current.Vendor1099)
			{
				sender.SetDefaultExt<APTran.box1099>(e.Row);				
			}
			sender.SetDefaultExt<APTran.projectID>(e.Row);
		}


		protected virtual void APTran_AccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APTran row = e.Row as APTran;
            if (row != null && row.ProjectID != null && !PM.ProjectDefaultAttribute.IsNonProject(this, row.ProjectID) && row.TaskID != null)
			{
				Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, e.NewValue);
				if (account != null && account.AccountGroupID == null)
				{
					sender.RaiseExceptionHandling<APTran.accountID>(e.Row, account.AccountCD, new PXSetPropertyException(PM.Messages.NoAccountGroup, PXErrorLevel.Warning, account.AccountCD));
				}
			}
		}


		protected virtual void APTran_SubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APTran row = (APTran)e.Row;
			if (row == null || row.AccountID == null || vendor.Current == null || vendor.Current.Type == null) return;
			if (!String.IsNullOrEmpty(row.PONbr) || !String.IsNullOrEmpty(row.ReceiptNbr)) return;
			InventoryItem item = nonStockItem.Select(row.InventoryID);
			EPEmployee employeeByUser = (EPEmployee)PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.Select(this, PXAccess.GetUserID());
			Location companyloc =
				(Location)PXSelectJoin<Location, InnerJoin<BAccountR, On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>, InnerJoin<Branch, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>>>, Where<Branch.branchID, Equal<Required<APTran.branchID>>>>.Select(this, row.BranchID);
			CT.Contract project = PXSelect<CT.Contract, Where<CT.Contract.contractID, Equal<Required<CT.Contract.contractID>>>>.Select(this, row.ProjectID);
			if (project == null || project.BaseType == CT.Contract.ContractBaseType.Contract)
			{
				project = PXSelect<CT.Contract, Where<CT.Contract.nonProject, Equal<True>>>.Select(this);
			}


			int? vendor_SubID = null;
			switch (vendor.Current.Type)
			{
				case BAccountType.VendorType:
				case BAccountType.CombinedType:
					if (location.Current.VExpenseAcctID != null)
					{
						vendor_SubID = (int?)Caches[typeof(Location)].GetValue<Location.vExpenseSubID>(location.Current);
					}
					break;
				case BAccountType.EmployeeType:
					if (employee.Current.ExpenseAcctID != null)
					{
						vendor_SubID = (int?)Caches[typeof(EPEmployee)].GetValue<EPEmployee.expenseSubID>(employee.Current);
					}
					break;
			}


			int? item_SubID = (int?)Caches[typeof(InventoryItem)].GetValue<InventoryItem.cOGSSubID>(item);
			int? employeeByUser_SubID = (int?)Caches[typeof(EPEmployee)].GetValue<EPEmployee.expenseSubID>(employeeByUser);
			int? company_SubID = (int?)Caches[typeof(Location)].GetValue<Location.cMPExpenseSubID>(companyloc);
			int? project_SubID = project.DefaultSubID;

			object value = SubAccountMaskAttribute.MakeSub<APSetup.expenseSubMask>(this, APSetup.Current.ExpenseSubMask,
				new object[] { vendor_SubID, item_SubID, employeeByUser_SubID, company_SubID, project_SubID },
								new Type[] { typeof(Location.vExpenseSubID), typeof(InventoryItem.cOGSSubID), typeof(EPEmployee.expenseSubID), typeof(Location.cMPExpenseSubID), typeof(PM.PMProject.defaultSubID) });

			sender.RaiseFieldUpdating<APTran.subID>(row, ref value);

			e.NewValue = (int?)value;
			e.Cancel = true;

		}

		protected virtual void APTran_LCTranID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.Cancel = true;
		}

        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
        [APTax(typeof(APRegister), typeof(APTax), typeof(APTaxTran))]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
        [PXDefault(typeof(Search<InventoryItem.taxCategoryID,
            Where<InventoryItem.inventoryID, Equal<Current<APTran.inventoryID>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        protected virtual void APTran_TaxCategoryID_CacheAttached(PXCache sender)
        {
        }

		protected virtual void APTran_TaxCategoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APTran row = (APTran)e.Row;
			if (row == null || row.InventoryID != null || vendor == null || vendor.Current == null || vendor.Current.TaxAgency == true || row.LCTranID!=null) return;

			if (TaxAttribute.GetTaxCalc<APTran.taxCategoryID>(sender, row) == TaxCalc.Calc &&
			 taxzone.Current != null &&
			 !string.IsNullOrEmpty(taxzone.Current.DfltTaxCategoryID))
			{
				e.NewValue = taxzone.Current.DfltTaxCategoryID;
				e.Cancel = true;
			}
		}

        protected virtual void APTran_UnitCost_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            APTran row = (APTran)e.Row;
            if (row == null || row.InventoryID != null) return;
            e.NewValue = 0m;
            e.Cancel = true;
        }

		protected virtual void APTran_CuryUnitCost_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APTran tran = (APTran)e.Row;
			if (tran == null || tran.InventoryID == null || !string.IsNullOrEmpty(tran.PONbr))
			{
				e.NewValue = sender.GetValue<APTran.curyUnitCost>(e.Row);
				e.Cancel = e.NewValue != null;
				return;
			}
						
			APInvoice doc = this.Document.Current;
			if (doc != null && doc.VendorID != null)
			{
                decimal? vendorUnitCost = null;
                if (tran != null && tran.InventoryID != null && tran.UOM != null)
                {
                    DateTime date = Document.Current.DocDate.Value;

                    vendorUnitCost = APVendorSalesPriceMaint.CalculateUnitCost(sender, tran.VendorID, doc.VendorLocationID, tran.InventoryID, currencyinfo.Select(), tran.UOM, tran.Qty, date, tran.CuryUnitCost);
                    e.NewValue = vendorUnitCost;
                }
                if (vendorUnitCost == null)
                e.NewValue = POItemCostManager.Fetch<APTran.inventoryID, APTran.curyInfoID>(sender.Graph, tran,
					doc.VendorID, doc.VendorLocationID, doc.DocDate, doc.CuryID, tran.InventoryID, null, null, tran.UOM);
				e.Cancel = true;
			}						
		}

		protected virtual void APTran_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APTran tran = (APTran)e.Row;
			sender.SetDefaultExt<APTran.unitCost>(tran);
			sender.SetDefaultExt<APTran.curyUnitCost>(tran);
			sender.SetValue<APTran.unitCost>(tran, null);
		}

        protected virtual void APTran_Qty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            APTran row = e.Row as APTran;
            if (row != null)
            {
                if (row.Qty == 0)
                {
                    sender.SetValueExt<APTran.curyDiscAmt>(row, decimal.Zero);
                    sender.SetValueExt<APTran.discPct>(row, decimal.Zero);
                }
                else
                {
                    sender.SetDefaultExt<APTran.curyUnitCost>(e.Row);
                }
            }
        }

		protected virtual void APTran_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APTran tran = e.Row as APTran;
			if (tran != null)
			{
				if (String.IsNullOrEmpty(tran.ReceiptNbr) && string.IsNullOrEmpty(tran.PONbr))
				{
					sender.SetDefaultExt<APTran.accountID>(tran);
					sender.SetDefaultExt<APTran.subID>(tran);
					sender.SetDefaultExt<APTran.taxCategoryID>(tran);
					sender.SetDefaultExt<APTran.deferredCode>(tran);
					sender.SetDefaultExt<APTran.uOM>(tran);

					sender.SetDefaultExt<APTran.unitCost>(tran);
					sender.SetDefaultExt<APTran.curyUnitCost>(tran);
					sender.SetValue<APTran.unitCost>(tran, null);

					InventoryItem item = nonStockItem.Select(tran.InventoryID);

					if (item != null)
					{
						tran.TranDesc = item.Descr;
					}
				}
			}
		}

		protected virtual void APTran_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APTran row = e.Row as APTran;
			if (row == null) return;

			sender.SetDefaultExt<APTran.subID>(row);
		}

		protected virtual void APTran_DefScheduleID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRSchedule sc = PXSelect<DRSchedule, Where<DRSchedule.scheduleID, Equal<Required<DRSchedule.scheduleID>>>>.Select(this, ((APTran)e.Row).DefScheduleID);
			if (sc != null)
			{
				APTran defertran = PXSelect<APTran, Where<APTran.tranType, Equal<Required<APTran.tranType>>,
					And<APTran.refNbr, Equal<Required<APTran.refNbr>>,
					And<APTran.lineNbr, Equal<Required<APTran.lineNbr>>>>>>.Select(this, sc.DocType, sc.RefNbr, sc.LineNbr);

				if (defertran != null)
				{
					((APTran)e.Row).DeferredCode = defertran.DeferredCode;
				}
			}
		}

        protected virtual void APTran_DiscountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            APTran row = e.Row as APTran;
            if (row != null && e.ExternalCall)
            {
                DiscountEngine<APTran>.UpdateManualLineDiscount<APInvoiceDiscountDetail>(sender, Transactions, row, DiscountDetails, Document.Current.VendorLocationID, Document.Current.DocDate.Value, Document.Current.SkipDiscounts);
            }
        }

		protected virtual void APTran_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			APTran row = e.Row as APTran;			
			if (row != null)
			{
				APInvoice doc = this.Document.Current;
				
				bool isPrebookedNotCompleted = (doc != null) &&  (doc.Prebooked == true && doc.Released == false && doc.Voided == false);
				bool isLCBased = (row.LCTranID != null);
				bool isPOReceiptRelated = string.IsNullOrEmpty(row.ReceiptNbr)==false;				
				bool isLCBasedTranAP = false;
				if (row.LCTranID != null)
				{
					LandedCostTran origin = null;
					foreach (LandedCostTran it in this.landedCostTrans.Select())
					{
						if (it.LCTranID == row.LCTranID)
						{
							origin = it;
							break;
						}
					}
					isLCBasedTranAP = (origin != null && (origin.Source == LandedCostTranSource.FromAP || origin.PostponeAP == true));					
				}
				if (!isLCBasedTranAP)
				{
					if (!(String.IsNullOrEmpty(row.PONbr) && String.IsNullOrEmpty(row.ReceiptNbr)))
					{
						PXUIFieldAttribute.SetEnabled<APTran.inventoryID>(cache, row, false);
						PXUIFieldAttribute.SetEnabled<APTran.uOM>(cache, row, false);
						bool isStockItem = false;
						if (row.InventoryID != null)
						{
							InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, row.InventoryID);
							isStockItem = (item != null && (item.StkItem == true));	
						}
						PXUIFieldAttribute.SetEnabled<APTran.accountID>(cache, row, !isStockItem);
						PXUIFieldAttribute.SetEnabled<APTran.subID>(cache, row, !isStockItem);
					}					
					bool allowEdit = (doc!= null) && (doc.Prebooked == false && doc.Released == false && doc.Voided == false);
					PXUIFieldAttribute.SetEnabled<APTran.defScheduleID>(cache, row, allowEdit && row.TranType == APInvoiceType.DebitAdj);
					PXUIFieldAttribute.SetEnabled<APTran.deferredCode>(cache, row, allowEdit && row.DefScheduleID == null);
					if (isPrebookedNotCompleted)
					{
						PXUIFieldAttribute.SetEnabled<APTran.accountID>(cache, row, (!isLCBased) && (!isPOReceiptRelated));
						PXUIFieldAttribute.SetEnabled<APTran.subID>(cache, row, (!isLCBased) && (!isPOReceiptRelated));
						PXUIFieldAttribute.SetEnabled<APTran.branchID>(cache, row, (!isLCBased) && (!isPOReceiptRelated));
					}
				}
				else
				{
					PXUIFieldAttribute.SetEnabled(cache, row, false);
				}

				if (row.TranType != APDocType.Invoice &&
					!(string.IsNullOrEmpty(row.PONbr) && string.IsNullOrEmpty(row.ReceiptNbr)))
				{
					PXUIFieldAttribute.SetEnabled<APTran.accountID>(cache, row, false);
					PXUIFieldAttribute.SetEnabled<APTran.subID>(cache, row, false);
				}
				viewSchedule.SetEnabled(cache.GetStatus(row) != PXEntryStatus.Inserted);				
			}
		}

        protected virtual void APTran_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            if (e.Row == null) return;
            RecalculateDiscounts(sender, (APTran)e.Row);
            TaxAttribute.Calculate<APTran.taxCategoryID, APTaxAttribute>(sender, e);
        }

        protected virtual void APTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            APTran row = e.Row as APTran;
            if (row != null)
            {
                //Validate that Expense Account <> Deferral Account:
                if (!sender.ObjectsEqual<APTran.accountID, APTran.deferredCode>(e.Row, e.OldRow))
                {
                    if (!string.IsNullOrEmpty(row.DeferredCode))
                    {
                        DRDeferredCode defCode = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Required<DRDeferredCode.deferredCodeID>>>>.Select(this, row.DeferredCode);
                        if (defCode != null)
                        {
                            if (defCode.AccountID == row.AccountID)
                            {
                                sender.RaiseExceptionHandling<APTran.accountID>(e.Row, row.AccountID,
                                    new PXSetPropertyException(Messages.AccountIsSameAsDeferred, PXErrorLevel.Warning));
                            }
                        }
                    }
                }
            }

            if (!sender.ObjectsEqual<APTran.branchID>(e.Row, e.OldRow) || !sender.ObjectsEqual<APTran.inventoryID>(e.Row, e.OldRow) ||
                    !sender.ObjectsEqual<APTran.qty>(e.Row, e.OldRow) || !sender.ObjectsEqual<APTran.curyUnitCost>(e.Row, e.OldRow) ||
                    !sender.ObjectsEqual<APTran.curyLineAmt>(e.Row, e.OldRow) || !sender.ObjectsEqual<APTran.curyDiscAmt>(e.Row, e.OldRow) ||
                    !sender.ObjectsEqual<APTran.discPct>(e.Row, e.OldRow) || !sender.ObjectsEqual<APTran.manualDisc>(e.Row, e.OldRow) ||
                    !sender.ObjectsEqual<APTran.discountID>(e.Row, e.OldRow))
                RecalculateDiscounts(sender, row);

            TaxAttribute.Calculate<APTran.taxCategoryID, APTaxAttribute>(sender, e);

            if (!sender.ObjectsEqual<APTran.box1099>(e.Row, e.OldRow))
            {
                foreach (APAdjust adj in PXSelect<APAdjust, Where<APAdjust.adjdDocType, Equal<Current<APInvoice.docType>>, And<APAdjust.adjdRefNbr, Equal<Current<APInvoice.refNbr>>, And<APAdjust.released, Equal<boolTrue>>>>>.Select(this))
                {
                    APReleaseProcess.Update1099Hist(this, -1m, adj, (APTran)e.OldRow, Document.Current);
                    APReleaseProcess.Update1099Hist(this, 1m, adj, (APTran)e.Row, Document.Current);
                }
            }

            //if any of the fields that was saved in avalara has changed mark doc as TaxInvalid.
            if (Document.Current != null && IsExternalTax == true &&
                !sender.ObjectsEqual<APTran.accountID, APTran.inventoryID, APTran.tranDesc, APTran.tranAmt, APTran.tranDate, APTran.taxCategoryID>(e.Row, e.OldRow))
            {
                Document.Current.IsTaxValid = false;
                Document.Update(Document.Current);
            }
            //}
        }

        protected virtual void APTran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            APTran row = (APTran)e.Row;

            if (Document.Current != null && Document.Cache.GetStatus(Document.Current) != PXEntryStatus.Deleted)
            {
                if (row.LineType != SOLineType.Discount)
                    DiscountEngine<APTran>.RecalculateGroupAndDocumentDiscounts(sender, Transactions, DiscountDetails, Document.Current.VendorLocationID, Document.Current.DocDate.Value, Document.Current.SkipDiscounts);
                RecalculateTotalDiscount();
            }
        }

		protected virtual void APTran_Box1099_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (vendor.Current == null || vendor.Current.Vendor1099 == false)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void APTran_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			object existing;
			if ((existing = sender.Locate(e.Row)) != null && sender.GetStatus(existing) == PXEntryStatus.Deleted)
			{
				sender.SetValue<APTran.tranID>(e.Row, sender.GetValue<APTran.tranID>(existing));
			}
		}

		protected virtual void APTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			APTran row = (APTran)e.Row;
			if (row.TranType == APDocType.Invoice &&
				(!String.IsNullOrEmpty(row.ReceiptNbr) && row.ReceiptLineNbr.HasValue))
			{
				PXResult<POReceiptLineR, POReceiptLineR1> source = (PXResult<POReceiptLineR, POReceiptLineR1>)PXSelectJoin<POReceiptLineR, InnerJoin<POReceiptLineR1, On<POReceiptLineR1.receiptNbr, Equal<POReceiptLineR.receiptNbr>,
														And<POReceiptLineR1.lineNbr, Equal<POReceiptLineR.lineNbr>>>>,
														Where<POReceiptLineR.receiptNbr, Equal<Required<POReceiptLineR.receiptNbr>>,
													And<POReceiptLineR.lineNbr, Equal<POReceiptLineR.lineNbr>>>>.Select(this, row.ReceiptNbr, row.ReceiptLineNbr);
				if (source != null)
				{
					if (((POReceiptLineR)source).VoucheredQty > ((POReceiptLineR1)source).ReceiptQty)
					{
						sender.RaiseExceptionHandling<APTran.qty>(e.Row, row.Qty,
									new PXSetPropertyException(Messages.QuantityBilledIsGreaterThenPOReceiptQuantity, PXErrorLevel.Error));
					}
				}
			}
		}

		protected virtual void APTran_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			APTran row = e.Row as APTran;
			if (row != null)
			{
				bool isLCBasedTranAP = false;
				if (row.LCTranID != null)
				{
					LandedCostTran origin = null;
					foreach (LandedCostTran it in this.landedCostTrans.Select())
					{
						if (it.LCTranID == row.LCTranID)
						{
							origin = it;
							break;
						}
					}
					isLCBasedTranAP = (origin != null && (origin.Source == LandedCostTranSource.FromAP || origin.PostponeAP == true));
				}
				if (isLCBasedTranAP && !this._isLCSync)
					e.Cancel = true;
			}
		}
		private bool _isLCSync = false;

		protected virtual void AP1099Hist_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (((AP1099Hist)e.Row).BoxNbr == null)
			{
				e.Cancel = true;
			}
		}

		#endregion

		#region CurrencyInfo Events
		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if ((bool)CMSetup.Current.MCActivated)
			{
				if (vendor.Current != null && !string.IsNullOrEmpty(vendor.Current.CuryID))
				{
					e.NewValue = vendor.Current.CuryID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if ((bool)CMSetup.Current.MCActivated)
			{
				if (vendor.Current != null && !string.IsNullOrEmpty(vendor.Current.CuryRateTypeID))
				{
					e.NewValue = vendor.Current.CuryRateTypeID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Cache.Current != null)
			{
				e.NewValue = ((APInvoice)Document.Cache.Current).DocDate;
				e.Cancel = true;
			}
		}
		protected virtual void CurrencyInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CurrencyInfo info = e.Row as CurrencyInfo;
			if (info != null)
			{
				bool curyenabled = info.AllowUpdate(this.Transactions.Cache);

				if (vendor.Current != null && !(bool)vendor.Current.AllowOverrideRate)
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

		#region APTaxTran Events
		protected virtual void APTaxTran_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null)
			{
				e.NewValue = Document.Current.TaxZoneID;
				e.Cancel = true;
			}
		}

		protected virtual void APTaxTran_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			PXParentAttribute.SetParent(sender, e.Row, typeof(APRegister), this.Document.Current);
		}

		protected virtual void APTaxTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (Document.Current != null && (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update))
			{
				((APTaxTran)e.Row).TaxZoneID = Document.Current.TaxZoneID;
			}
		}

		protected virtual void APTaxTran_TaxZoneID_ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e)
		{
			if (e.Exception is PXSetPropertyException)
			{
				Document.Cache.RaiseExceptionHandling<APInvoice.taxZoneID>(Document.Current, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
				e.Cancel = false;
			}
		}

		#endregion     

		#region APAdjust Events
		protected virtual void APAdjust_CuryAdjdAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APAdjust adj = (APAdjust)e.Row;
			Terms terms = PXSelect<Terms, Where<Terms.termsID, Equal<Current<APInvoice.termsID>>>>.Select(this);

			if (terms != null && terms.InstallmentType != TermsInstallmentType.Single && (decimal)e.NewValue > 0m)
			{
				throw new PXSetPropertyException(Messages.PrepaymentAppliedToMultiplyInstallments);
			}

			if (adj.CuryDocBal == null)
			{
				PXResult<APPayment, CurrencyInfo> res = (PXResult<APPayment, CurrencyInfo>)PXSelectJoin<APPayment, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APPayment.curyInfoID>>>, Where<APPayment.docType, Equal<Required<APPayment.docType>>, And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>.Select(this, adj.AdjgDocType, adj.AdjgRefNbr);

				APPayment payment = (APPayment)res;
				CurrencyInfo pay_info = (CurrencyInfo)res;
				CurrencyInfo inv_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APInvoice.curyInfoID>>>>.Select(this);

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

		protected virtual void APAdjust_CuryAdjdAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APAdjust adj = (APAdjust)e.Row;
			decimal CuryAdjgAmt;
			decimal AdjdAmt;
			decimal AdjgAmt;

			PXDBCurrencyAttribute.CuryConvBase<APAdjust.adjdCuryInfoID>(sender, e.Row, (decimal)adj.CuryAdjdAmt, out AdjdAmt);

			CurrencyInfo pay_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APAdjust.adjgCuryInfoID>>>>.SelectSingleBound(this, new object[] { adj });
			CurrencyInfo inv_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APAdjust.adjdCuryInfoID>>>>.SelectSingleBound(this, new object[] { adj });

			if (string.Equals(pay_info.CuryID, inv_info.CuryID))
			{
				CuryAdjgAmt = (decimal)adj.CuryAdjdAmt;
			}
			else
			{
				PXDBCurrencyAttribute.CuryConvCury<APAdjust.adjgCuryInfoID>(sender, e.Row, AdjdAmt, out CuryAdjgAmt);
			}

			if (object.Equals(pay_info.CuryID, inv_info.CuryID) && object.Equals(pay_info.CuryRate, inv_info.CuryRate) && object.Equals(pay_info.CuryMultDiv, inv_info.CuryMultDiv))
			{
				AdjgAmt = AdjdAmt;
			}
			else
			{
				PXDBCurrencyAttribute.CuryConvBase<APAdjust.adjgCuryInfoID>(sender, e.Row, CuryAdjgAmt, out AdjgAmt);
			}

			adj.CuryAdjgAmt = CuryAdjgAmt;
			adj.AdjAmt = AdjdAmt;
			adj.RGOLAmt = AdjgAmt - AdjdAmt;
			adj.CuryDocBal = adj.CuryDocBal + (decimal?)e.OldValue - adj.CuryAdjdAmt;
		}

		protected virtual void APAdjust_Hold_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = true;
			e.Cancel = true;
		}
		#endregion

		#region Landed Cost Trans Events
		protected virtual void LandedCostTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			LandedCostTran row = (LandedCostTran)e.Row;
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete)
			{
				if (row != null && row.Source == LandedCostTranSource.FromAP && String.IsNullOrEmpty(row.LandedCostCodeID) == false)
				{
					bool hasReceipt = String.IsNullOrEmpty(row.POReceiptNbr) == false;
					if (!hasReceipt) 
					{
						foreach (LandedCostTranSplit iSplit in this.LCTranSplit.Select(row.LCTranID)) 
						{
							hasReceipt = String.IsNullOrEmpty(iSplit.POReceiptNbr) == false;
							if (hasReceipt) break; 
						}
					}
					if (!hasReceipt)
					{
						if (sender.RaiseExceptionHandling<LandedCostTran.pOReceiptNbr>(e.Row, row.POReceiptNbr, new PXSetPropertyException(Messages.APLandedCost_NoPOReceiptNumberSpecified, PXErrorLevel.RowError)))
						{
							throw new PXRowPersistingException(typeof(LandedCostTran.pOReceiptNbr).Name, row.POReceiptNbr, Messages.APLandedCost_NoPOReceiptNumberSpecified);
						}
					}						
				}
				if (row != null && !String.IsNullOrEmpty(row.LandedCostCodeID) && (row.Source == LandedCostTranSource.FromAP))
				{
					LandedCostCode code = PXSelect<LandedCostCode, Where<LandedCostCode.landedCostCodeID, Equal<Required<LandedCostCode.landedCostCodeID>>>>.Select(this, row.LandedCostCodeID);
					decimal valueToCompare = decimal.Zero;
					int count = 0;

					List<POReceiptLine> allocateOn = new List<POReceiptLine>();
					List<LandedCostTranSplit> splits = new List<LandedCostTranSplit>();
					Dictionary<string, int> used = new Dictionary<string, int>();
					PXSelectBase<POReceiptLine> rctSelect = new PXSelect<POReceiptLine, Where<POReceiptLine.receiptNbr, Equal<Required<POReceiptLine.receiptNbr>>>>(this);
					if (String.IsNullOrEmpty(row.POReceiptNbr) == false)
					{
						used[row.POReceiptNbr] = 1;
						foreach (POReceiptLine it in rctSelect.Select(row.POReceiptNbr, row.InventoryID))
						{
							allocateOn.Add(it);							
						}
					}

					foreach (LandedCostTranSplit iSplit in this.LCTranSplit.Select(row.LCTranID))
					{
						splits.Add(iSplit);
						if(!used.ContainsKey(iSplit.POReceiptNbr))
						{
							used[iSplit.POReceiptNbr] = 1;
							foreach (POReceiptLine iLn in rctSelect.Select(iSplit.POReceiptNbr, row.InventoryID))
							{
								allocateOn.Add(iLn);							
							}
						}
					}

					foreach (POReceiptLine it in allocateOn)
					{
						if (LandedCostUtils.IsApplicable(row,splits, code, it))
						{
							valueToCompare += LandedCostUtils.GetBaseValue(code, it);
							count++;
						}						
					}

					string message = String.Empty;
					switch (code.AllocationMethod)
					{
						case LandedCostAllocationMethod.ByCost:
							message = PO.Messages.LandedCostReceiptTotalCostIsZero;
							break;
						case LandedCostAllocationMethod.ByWeight:
							message = PO.Messages.LandedCostReceiptTotalWeightIsZero;
							break;
						case LandedCostAllocationMethod.ByVolume:
							message = PO.Messages.LandedCostReceiptTotalVolumeIsZero;
							break;
						case LandedCostAllocationMethod.ByQuantity:
							message = PO.Messages.LandedCostReceiptTotalQuantityIsZero;
							break;
						case LandedCostAllocationMethod.None:
							message = PO.Messages.LandedCostReceiptNoReceiptLines;
							valueToCompare = count;
							break;
						default:
							message = PO.Messages.LandedCostUnknownAllocationType;
							break;
					}
					if (valueToCompare == Decimal.Zero)
					{
						if (sender.RaiseExceptionHandling<LandedCostTran.pOReceiptNbr>(e.Row, row.POReceiptNbr, new PXSetPropertyException(message, PXErrorLevel.RowError)))
						{
							throw new PXRowPersistingException(typeof(LandedCostTran.pOReceiptNbr).Name, row.POReceiptNbr, message);
						}
					}
					if (this.Document.Current != null)
						row.InvoiceNbr = this.Document.Current.InvoiceNbr;
				}
				if (row != null && row.Source == LandedCostTranSource.FromPO && String.IsNullOrEmpty(row.APDocType)) 
				{
					PXDBDefaultAttribute.SetDefaultForUpdate<LandedCostTran.aPDocType>(sender, e.Row, false);
					PXDBDefaultAttribute.SetDefaultForUpdate<LandedCostTran.aPRefNbr>(sender, e.Row, false);
					PXDBDefaultAttribute.SetDefaultForInsert<LandedCostTran.aPDocType>(sender, e.Row, false);
					PXDBDefaultAttribute.SetDefaultForInsert<LandedCostTran.aPRefNbr>(sender, e.Row, false);				
				}

			}
		}

		protected virtual void LandedCostTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			LandedCostTran row = (LandedCostTran)e.Row;
			
			PXUIFieldAttribute.SetEnabled<LandedCostTran.curyID>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<LandedCostTran.vendorID>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<LandedCostTran.vendorLocationID>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<LandedCostTran.pOReceiptType>(sender, null, false);
			if(row!= null)
			{
				bool hasINDoc = (row.INDocCreated == true);
				PXUIFieldAttribute.SetEnabled<LandedCostTran.landedCostCodeID>(sender, e.Row, !hasINDoc);
				PXUIFieldAttribute.SetEnabled<LandedCostTran.pOReceiptNbr>(sender, e.Row, !hasINDoc);				
				PXUIFieldAttribute.SetEnabled<LandedCostTran.inventoryID>(sender, e.Row, !hasINDoc);			
			}
		}

		protected virtual void LandedCostTran_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			LandedCostTran row = (LandedCostTran)e.Row;
			if (row != null && !String.IsNullOrEmpty(row.LandedCostCodeID))
			{
				LandedCostCode code = PXSelect<LandedCostCode, Where<LandedCostCode.landedCostCodeID, Equal<Required<LandedCostCode.landedCostCodeID>>>>.Select(this, row.LandedCostCodeID);
				this.AddLandedCostTran(row, code);
			}
		}

		protected virtual void LandedCostTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			LandedCostTran row = (LandedCostTran)e.Row;
			LandedCostTran oldRow = (LandedCostTran)e.OldRow;
			APTran tran = null;
			foreach (APTran it in this.Transactions.Select())
			{
				if(it.LCTranID == row.LCTranID)
				{
					tran = it; break;					
				}
			}
			LandedCostCode code = PXSelect<LandedCostCode, Where<LandedCostCode.landedCostCodeID, Equal<Required<LandedCostCode.landedCostCodeID>>>>.Select(this, row.LandedCostCodeID);
			if (tran == null)
			{
				this.AddLandedCostTran(row, code);
			}
			else
			{
				APTran copy = (APTran)this.Transactions.Cache.CreateCopy(tran);
				Copy(copy, row, code);
				tran = (APTran)this.Transactions.Cache.Update(copy);				
			}
		}

		protected virtual void LandedCostTran_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			LandedCostTran row = (LandedCostTran)e.Row;
			if (row.PostponeAP == true) 
			{
				row.APDocType = null;
				row.APRefNbr = null;				
				sender.RaiseRowDeleted(e.Row);
				PXDBDefaultAttribute.SetDefaultForUpdate<LandedCostTran.aPDocType>(sender, e.Row, false);
				PXDBDefaultAttribute.SetDefaultForUpdate<LandedCostTran.aPRefNbr>(sender, e.Row, false);
				PXDBDefaultAttribute.SetDefaultForInsert<LandedCostTran.aPDocType>(sender, e.Row, false);
				PXDBDefaultAttribute.SetDefaultForInsert<LandedCostTran.aPRefNbr>(sender, e.Row, false);
				
				this.landedCostTrans.View.RequestRefresh();
				e.Cancel = true;
			}
		}

		protected virtual void LandedCostTran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			LandedCostTran row = (LandedCostTran)e.Row;
			//APTran tran = null;
			List<APTran> trans = new List<APTran>(3);
			foreach (APTran it in this.Transactions.Select())
			{
				if (it.LCTranID == row.LCTranID)
				{
					trans.Add(it);					
				}
			}
			this._isLCSync = true;
			try
			{
				foreach (APTran itr in trans)
				{
					this.Transactions.Delete(itr);
				}
			}
			finally 
			{
				this._isLCSync = false;
			}
		}

		protected virtual void LandedCostTran_POReceiptNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			LandedCostTran row = (LandedCostTran)e.Row;
			sender.SetDefaultExt<LandedCostTran.inventoryID>(row);
			sender.SetValuePending<LandedCostTran.inventoryID>(row, null);

		}

		protected virtual void LandedCostTran_LandedCostCodeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			LandedCostTran row = (LandedCostTran)e.Row;
			sender.SetDefaultExt<LandedCostTran.descr>(e.Row);
			sender.SetValuePending<LandedCostTran.descr>(e.Row, row.Descr);
			sender.SetDefaultExt<LandedCostTran.taxCategoryID>(e.Row);
			sender.SetDefaultExt<LandedCostTran.lCAccrualAcct>(e.Row);
			sender.SetDefaultExt<LandedCostTran.lCAccrualSub>(e.Row);			
		}

		protected virtual void LandedCostTranR_RowPersisting(PXCache sender, PXRowPersistingEventArgs e) 
		{
			e.Cancel = true;
		}

		#endregion

        #region APInvoiceDiscountDetail events

        protected virtual void APInvoiceDiscountDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            APInvoiceDiscountDetail discountDetail = (APInvoiceDiscountDetail)e.Row;
            if (e.ExternalCall == true && discountDetail != null && discountDetail.DiscountID != null)
            {
                discountDetail.IsManual = true;

                DiscountEngine<APTran>.InsertDocGroupDiscount<APInvoiceDiscountDetail>(Transactions.Cache, Transactions, DiscountDetails, discountDetail, discountDetail.DiscountID, null, Document.Current.VendorLocationID, Document.Current.DocDate.Value);
                RecalculateTotalDiscount();
            }
        }

        protected virtual void APInvoiceDiscountDetail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            APInvoiceDiscountDetail discountDetail = (APInvoiceDiscountDetail)e.Row;
            if (e.ExternalCall == true && discountDetail != null && discountDetail.Type != DiscountType.Document)
            {
                DiscountEngine<APTran>.UpdateDocumentDiscount<APInvoiceDiscountDetail>(Transactions.Cache, Transactions, DiscountDetails, Document.Current.VendorLocationID, Document.Current.DocDate.Value, Document.Current.SkipDiscounts);
            }
            RecalculateTotalDiscount();
        }
        #endregion

		#region Select Overrides

		public virtual IEnumerable pOreceiptslist()
		{
			APInvoice doc = this.Document.Current;
			if (doc == null || doc.VendorID == null || doc.VendorLocationID == null) yield break;
			if( doc.DocType != APDocType.Invoice && doc.DocType != APDocType.DebitAdj) 
					yield break;
			foreach (POReceipt iDoc in poreceiptslist.Cache.Updated)
			{
				yield return iDoc;
			}

			string poReceiptType = (doc.DocType == APDocType.Invoice) ? POReceiptType.POReceipt: POReceiptType.POReturn;

            List<String> validPOReceipts = new List<string>();
            if (pOrderFilter.Current.OrderNbr != null)
            {
                foreach (PXResult<POReceipt, POReceiptLineS, APTran> order in PXSelectJoin<POReceipt,
                InnerJoin<POReceiptLineS, On<POReceiptLineS.receiptNbr, Equal<POReceipt.receiptNbr>>,
                LeftJoin<APTran, On<APTran.receiptNbr, Equal<POReceiptLineS.receiptNbr>,
                And<APTran.receiptLineNbr, Equal<POReceiptLineS.lineNbr>,
                And<APTran.released, Equal<boolFalse>>>>>>,
                Where<POReceipt.vendorID, Equal<Required<APInvoice.vendorID>>,
                And<POReceipt.vendorLocationID, Equal<Required<APInvoice.vendorLocationID>>,
                And<POReceipt.curyID, Equal<Required<APInvoice.curyID>>,
                And<POReceipt.hold, Equal<CS.boolFalse>,
                And<POReceipt.released, Equal<CS.boolTrue>,
                And<POReceipt.receiptType, Equal<Required<POReceipt.receiptType>>,
                And<APTran.refNbr, IsNull,
                And<Where<POReceiptLineS.unbilledQty, Greater<decimal0>,
                Or<POReceiptLineS.curyUnbilledAmt, NotEqual<decimal0>
                >>>>>>>>>>>.Select(this, doc.VendorID, doc.VendorLocationID, doc.CuryID, poReceiptType))
                {
                    if (((POReceiptLineS)order).PONbr == pOrderFilter.Current.OrderNbr)
                    {
                        validPOReceipts.Add(((POReceiptLineS)order).ReceiptNbr);
                    }
                }
            }

			foreach (PXResult<POReceipt, POReceiptLineS, APTran> order in PXSelectJoinGroupBy<POReceipt,
				InnerJoin<POReceiptLineS, On<POReceiptLineS.receiptNbr, Equal<POReceipt.receiptNbr>>,
				LeftJoin<APTran, On<APTran.receiptNbr, Equal<POReceiptLineS.receiptNbr>,
					And<APTran.receiptLineNbr, Equal<POReceiptLineS.lineNbr>,
					And<APTran.released, Equal<boolFalse>>>>>>,
				Where<POReceipt.vendorID, Equal<Required<APInvoice.vendorID>>,
					 And<POReceipt.vendorLocationID, Equal<Required<APInvoice.vendorLocationID>>,
					 And<POReceipt.curyID, Equal<Required<APInvoice.curyID>>,
					 And<POReceipt.hold, Equal<CS.boolFalse>,
					 And<POReceipt.released, Equal<CS.boolTrue>,
					 And<POReceipt.receiptType, Equal<Required<POReceipt.receiptType>>,
					 And<APTran.refNbr, IsNull,
					 And<Where<POReceiptLineS.unbilledQty, Greater<decimal0>,
						 Or<POReceiptLineS.curyUnbilledAmt, NotEqual<decimal0>
					 >>>>>>>>>>,
				Aggregate<GroupBy<POReceipt.receiptNbr, Sum<POReceiptLineS.receiptQty, Sum<POReceiptLineS.unbilledQty, Sum<POReceiptLineS.curyUnbilledAmt, GroupBy<POReceipt.receiptType>>>>>>>.Select(this, doc.VendorID, doc.VendorLocationID, doc.CuryID, poReceiptType))
			{
				//if (poreceiptslist.Cache.Locate((POReceipt)order) == null) //&& cmd.View.SelectSingleBound(new object[] { (POReceipt)order }) == null)
				//{
                if (pOrderFilter.Current.OrderNbr != null)
                {
                    if (validPOReceipts.Contains(((POReceipt)order).ReceiptNbr)) yield return (POReceipt)order;
                }
                else
                {
                    yield return (POReceipt)order;
                }
				//}
			}
		}

		public virtual IEnumerable pOOrderslist()
		{
			APInvoice doc = this.Document.Current;
			if (doc == null || doc.VendorID == null || doc.VendorLocationID == null) yield break;

			foreach (POOrder iDoc in this.poorderslist.Cache.Updated)
			{
				yield return iDoc;
			}

			foreach (POOrderRS order in PXSelect<POOrderRS,
												Where<POOrderRS.vendorID, Equal<Required<APInvoice.vendorID>>,
												And<POOrderRS.vendorLocationID, Equal<Required<APInvoice.vendorLocationID>>,
												And<POOrderRS.curyID, Equal<Required<APInvoice.curyID>>,
												And<POOrderRS.orderType, Equal<POOrderType.regularOrder>,
												And<POOrderRS.hold, Equal<CS.boolFalse>>>>>>>.Select(this, doc.VendorID, doc.VendorLocationID, doc.CuryID))
			{
				if ((order.LeftToBillQty ?? Decimal.Zero) > Decimal.Zero
						|| (order.CuryLeftToBillCost ?? Decimal.Zero) > Decimal.Zero)
				{
					yield return (POOrderRS)order;
				}
			}
		}

		#endregion

		#region Source-Specific  functions

		public class POReceiptLineComparer : IEqualityComparer<APTran>
		{
			public POReceiptLineComparer()
			{ 
			}

			#region IEqualityComparer<APTran> Members

			public bool Equals(APTran x, APTran y)
			{
				return x.ReceiptNbr == y.ReceiptNbr && x.ReceiptLineNbr == y.ReceiptLineNbr;
			}

			public int GetHashCode(APTran obj)
			{
				return obj.ReceiptNbr.GetHashCode() + 109 * obj.ReceiptLineNbr.GetHashCode();
			}

			#endregion
		}

		public class POLineComparer : IEqualityComparer<APTran>
		{
			public POLineComparer()
			{
			}

			#region IEqualityComparer<APTran> Members

			public bool Equals(APTran x, APTran y)
			{
				return x.POOrderType == y.POOrderType && x.PONbr == y.PONbr && x.POLineNbr == y.POLineNbr;
			}

			public int GetHashCode(APTran obj)
			{
				return obj.POOrderType.GetHashCode() + 109 * obj.PONbr.GetHashCode() + 37 * obj.POLineNbr.GetHashCode();
			}

			#endregion
		}

		public virtual void InvoicePOReceipt(POReceipt order, CurrencyInfo aCuryInfo, DocumentList<APInvoice> list, bool saveAndAdd, bool keepReceiptTaxes)
		{
			APInvoice newdoc;

			if (list != null)
			{
				bool billSeparately = false;
				if (billSeparately == false)
				{
					newdoc = list.Find<APInvoice.docType, APInvoice.branchID, APInvoice.vendorID, APInvoice.vendorLocationID, APInvoice.curyID, APInvoice.termsID, APInvoice.invoiceDate>(((POReceipt)order).GetAPDocType(), ((POReceipt)order).BranchID, ((POReceipt)order).VendorID, ((POReceipt)order).VendorLocationID, ((POReceipt)order).CuryID, ((POReceipt)order).TermsID, ((POReceipt)order).ReceiptDate, false) ?? new APInvoice();
				}
				else
				{
					newdoc = new APInvoice();					
				}

				if (newdoc.RefNbr != null)
				{
					this.Document.Current = this.Document.Search<APInvoice.refNbr>(newdoc.RefNbr, newdoc.DocType);
				}
				else
				{
					newdoc.DocType = ((POReceipt)order).GetAPDocType();
					newdoc.DocDate = order.InvoiceDate;
                    if (DateTime.Compare((DateTime)order.ReceiptDate, (DateTime)order.InvoiceDate) == 0)
                    {
                        newdoc.FinPeriodID = order.FinPeriodID;
                    }
					newdoc = PXCache<APInvoice>.CreateCopy(this.Document.Insert(newdoc));
					newdoc.BranchID = order.BranchID;
					newdoc.VendorID = ((POReceipt)order).VendorID;
					newdoc.VendorLocationID = ((POReceipt)order).VendorLocationID;

					newdoc.TermsID = ((POReceipt)order).TermsID;
					newdoc.InvoiceNbr = order.InvoiceNbr;
					newdoc.DueDate = order.DueDate;
					newdoc.DiscDate = order.DiscDate;
					newdoc.CuryOrigDiscAmt = order.CuryDiscAmt;
					newdoc.TaxZoneID = order.TaxZoneID;
					newdoc.CuryOrigDocAmt = order.CuryControlTotal;
					if (aCuryInfo == null)
						newdoc.CuryID = order.CuryID;
					newdoc = this.Document.Update(newdoc);
					if (aCuryInfo != null)
					{
						foreach (CurrencyInfo info in this.currencyinfo.Select())
						{
							PXCache<CurrencyInfo>.RestoreCopy(info, aCuryInfo);
							info.CuryInfoID = newdoc.CuryInfoID;
							newdoc.CuryID = info.CuryID;
						}
					}
				}
			}
			else
			{
				newdoc = PXCache<APInvoice>.CreateCopy(Document.Current);
				if (newdoc.VendorID == null && newdoc.VendorLocationID == null)
				{
					newdoc.VendorID = ((POReceipt)order).VendorID;
					newdoc.VendorLocationID = ((POReceipt)order).VendorLocationID;

					newdoc.DocDate = order.InvoiceDate;
					if (String.IsNullOrEmpty(newdoc.InvoiceNbr))
						newdoc.InvoiceNbr = order.InvoiceNbr;
					if (String.IsNullOrEmpty(newdoc.TermsID))
						newdoc.TermsID = ((POReceipt)order).TermsID;
					newdoc.DueDate = order.DueDate;
					newdoc.DiscDate = order.DiscDate;
					newdoc.TaxZoneID = order.TaxZoneID;
					newdoc.CuryOrigDocAmt = order.CuryControlTotal;
					//newdoc.CuryOrigDiscAmt = order.CuryDiscAmt;					
					if (aCuryInfo == null)
						newdoc.CuryID = order.CuryID;
					newdoc = this.Document.Update(newdoc);
				}
				else
				{
					if (newdoc.VendorID != ((POReceipt)order).VendorID || newdoc.VendorID != ((POReceipt)order).VendorID)
						throw new PXException("PO Receipt {0} belongs to another vendor", ((POReceipt)order).ReceiptNbr);
				}
				if (aCuryInfo != null)
				{
					foreach (CurrencyInfo info in this.currencyinfo.Select())
					{
						PXCache<CurrencyInfo>.RestoreCopy(info, aCuryInfo);
						info.CuryInfoID = newdoc.CuryInfoID;
						newdoc.CuryID = info.CuryID;
					}
				}
			}
			newdoc = this.Document.Current;
			if (String.IsNullOrEmpty(newdoc.InvoiceNbr))
				newdoc.InvoiceNbr = ((POReceipt)order).InvoiceNbr;
			if (keepReceiptTaxes)
			{
				//This is required to transfer taxes from the original document(possibly modified there) instead of counting them by default rules
				TaxAttribute.SetTaxCalc<APTran.taxCategoryID, APTaxAttribute>(this.Transactions.Cache, null, TaxCalc.ManualCalc);
			}

			HashSet<APTran> duplicates = new HashSet<APTran>(new POReceiptLineComparer());
			foreach (APTran tran in this.Transactions.Select())
			{
				try
				{
					duplicates.Add(tran);
				}
				catch (NullReferenceException) { }
			}

			foreach (PXResult<POReceiptLineR, POReceiptLineS> res in
								PXSelectJoin<POReceiptLineR,
											InnerJoin<POReceiptLineS, On<POReceiptLineS.receiptNbr, Equal<POReceiptLineR.receiptNbr>, And<POReceiptLineS.lineNbr, Equal<POReceiptLineR.lineNbr>>>,
											LeftJoin<APTran, On<APTran.receiptNbr, Equal<POReceiptLineS.receiptNbr>,
												And<APTran.receiptLineNbr, Equal<POReceiptLineS.lineNbr>,
													And<APTran.released, Equal<boolFalse>>>>>>,
								Where<POReceiptLineS.receiptNbr, Equal<Required<POReceiptLineS.receiptNbr>>,
										And<APTran.refNbr, IsNull,
										And<Where<POReceiptLineS.unbilledQty, Greater<decimal0>,
											Or<POReceiptLineS.curyUnbilledAmt, NotEqual<decimal0>>
											>>>>>.Select(this, ((POReceipt)order).ReceiptNbr))
			{
				POReceiptLineS orderline = res;
				POReceiptLineR aggregate = res;

				PXRowInserting handler = new PXRowInserting((sender, e) =>
				{ 
					PXParentAttribute.SetParent(sender, e.Row, typeof(APRegister), Document.Current);
					PXParentAttribute.SetParent(sender, e.Row, typeof(POReceiptLineR), aggregate);
				});

				this.RowInserting.AddHandler<APTran>(handler);

				APTran tran = AddPOReceiptLine(orderline, duplicates);

				this.RowInserting.RemoveHandler<APTran>(handler);

			}

			foreach (POReceiptTaxTran tax in PXSelect<POReceiptTaxTran, Where<POReceiptTaxTran.receiptNbr, Equal<Required<POReceiptTaxTran.receiptNbr>>,
													And<POReceiptTaxTran.detailType, Equal<POReceiptTaxDetailType.receiptTax>>>>.Select(this, ((POReceipt)order).ReceiptNbr))
			{
				APTaxTran newtax = new APTaxTran();
				newtax.TaxID = tax.TaxID;
				newtax.TaxRate = 0m;
				newtax = this.Taxes.Insert(newtax);
			}
			newdoc.CuryOrigDocAmt = newdoc.CuryDocBal;
			this.Document.Cache.RaiseFieldUpdated<APInvoice.curyOrigDocAmt>(newdoc, 0m);
            newdoc.CuryOrigDiscAmt = order.CuryDiscAmt;
			if (list != null && saveAndAdd)
			{

				this.Save.Press();

				if (list.Find(this.Document.Current) == null)
				{
					list.Add(this.Document.Current);
				}
			}
		}

		public virtual void InvoicePOOrder(POOrder order, CurrencyInfo aCuryInfo, DocumentList<APInvoice> list)
		{
			APInvoice newdoc;

			if (list != null)
			{
				bool billSeparately = false;
				if (billSeparately == false)
				{
					newdoc = list.Find<APInvoice.docType, APInvoice.branchID, APInvoice.vendorID, APInvoice.vendorLocationID, APInvoice.curyID, APInvoice.termsID, APInvoice.invoiceDate>(APDocType.Invoice, order.BranchID, order.VendorID, order.VendorLocationID, order.CuryID, order.TermsID, order.OrderDate, false) ?? new APInvoice();
				}
				else
				{
					newdoc = new APInvoice();
				}

				if (newdoc.RefNbr != null)
				{
					this.Document.Current = this.Document.Search<APInvoice.refNbr>(newdoc.RefNbr, newdoc.DocType);
				}
				else
				{
					newdoc.DocType = APDocType.Invoice;
					newdoc.DocDate = order.OrderDate;
					newdoc.BranchID = order.BranchID;
					newdoc = PXCache<APInvoice>.CreateCopy(this.Document.Insert(newdoc));

					newdoc.VendorID = order.VendorID;
					newdoc.VendorLocationID = order.VendorLocationID;

					newdoc.TermsID = order.TermsID;
					newdoc.InvoiceNbr = order.VendorRefNbr;

					if (aCuryInfo == null)
						newdoc.CuryID = order.CuryID;
					newdoc = this.Document.Update(newdoc);
					if (aCuryInfo != null)
					{
						foreach (CurrencyInfo info in this.currencyinfo.Select())
						{
							PXCache<CurrencyInfo>.RestoreCopy(info, aCuryInfo);
							info.CuryInfoID = newdoc.CuryInfoID;
							newdoc.CuryID = info.CuryID;
						}
					}
				}
			}
			else
			{
				newdoc = PXCache<APInvoice>.CreateCopy(Document.Current);
				if (!newdoc.VendorID.HasValue || !newdoc.VendorLocationID.HasValue)
				{
					newdoc.VendorID = order.VendorID;
					newdoc.VendorLocationID = order.VendorLocationID;
					if (String.IsNullOrEmpty(newdoc.TermsID))
						newdoc.TermsID = order.TermsID;
					if (string.IsNullOrEmpty(newdoc.DocDesc))
						newdoc.DocDesc = order.OrderDesc;
					if (aCuryInfo == null)
						newdoc.CuryID = order.CuryID;
					newdoc = this.Document.Update(newdoc);
					if (aCuryInfo != null)
					{
						foreach (CurrencyInfo info in this.currencyinfo.Select())
						{
							PXCache<CurrencyInfo>.RestoreCopy(info, aCuryInfo);
							info.CuryInfoID = newdoc.CuryInfoID;
							newdoc.CuryID = info.CuryID;
						}
					}
				}
			}
			newdoc = this.Document.Current;
			if (String.IsNullOrEmpty(newdoc.InvoiceNbr))
				newdoc.InvoiceNbr = order.VendorRefNbr;

			HashSet<APTran> duplicates = new HashSet<APTran>(new POLineComparer());
			foreach (APTran tran in this.Transactions.Select())
			{
				try
				{
					duplicates.Add(tran);
				}
				catch (NullReferenceException) { }
			}

			foreach (PXResult<POLine> res in
								PXSelect<POLine, Where<POLine.orderType, Equal<Required<POLine.orderType>>,
										And<POLine.orderNbr, Equal<Required<POLine.orderNbr>>,
										And<POLine.lineType, Equal<POLineType.service>,
										And<POLine.cancelled, Equal<boolFalse>,
										And<POLine.completed, Equal<boolFalse>,
										And<POLine.voucheredQty, LessEqual<POLine.orderQty>>>>>>>>.Select(this, order.OrderType, order.OrderNbr))
			{
				POLine orderline = (POLine)res;
				APTran tran = AddPOReceiptLine(orderline, duplicates);
			}
			//Copying of taxes from original PO Order doesn't have much sense - Invoice is not created from the PO Order directly 
			//and PO Order most likely has  items of other types

			newdoc.CuryOrigDocAmt = newdoc.CuryDocBal;
			this.Document.Cache.RaiseFieldUpdated<APInvoice.curyOrigDocAmt>(newdoc, 0m);

			if (list != null)
			{
				this.Save.Press();
				if (list.Find(this.Document.Current) == null)
				{
					list.Add(this.Document.Current);
				}
			}
		}

		public virtual void InvoicePOOrder(POOrder order, bool createNew)
		{
			APInvoice doc;
			if (createNew)
			{
				doc = new APInvoice();
				doc.DocType = APDocType.Invoice;
				doc.BranchID = order.BranchID;
				doc = PXCache<APInvoice>.CreateCopy(this.Document.Insert(doc));
				doc.VendorID = order.VendorID;
				doc.VendorLocationID = order.VendorLocationID;
				doc.TermsID = order.TermsID;
				doc.InvoiceNbr = order.VendorRefNbr;
				doc.CuryID = order.CuryID;
				doc.DocDesc = order.OrderDesc;
				doc = this.Document.Update(doc);
			}
			else
			{
				doc = PXCache<APInvoice>.CreateCopy(Document.Current);
				if (string.IsNullOrEmpty(doc.DocDesc))
					doc.DocDesc = order.OrderDesc;
				if (string.IsNullOrEmpty(doc.InvoiceNbr))
					doc.InvoiceNbr = order.VendorRefNbr;
				doc = this.Document.Update(doc);
			}
			if ((doc.VendorID.HasValue && doc.VendorID != order.VendorID)
				|| (doc.VendorLocationID.HasValue && doc.VendorLocationID != order.VendorLocationID)
				|| doc.CuryID != order.CuryID)
				throw new PXException("Canot add Purchase Order - it belong to another Vendor, Vendor Location or has different Currency");

			HashSet<APTran> duplicates = new HashSet<APTran>(new POLineComparer());
			foreach (APTran tran in this.Transactions.Select())
			{
				try
				{
					duplicates.Add(tran);
				}
				catch (NullReferenceException) { }
			}

			foreach (PXResult<POLine> res in
								PXSelect<POLine, Where<POLine.orderType, Equal<Required<POLine.orderType>>,
										And<POLine.orderNbr, Equal<Required<POLine.orderNbr>>,
										And<POLine.lineType, Equal<POLineType.service>,
										And<POLine.cancelled, Equal<boolFalse>,
										And<POLine.completed, Equal<boolFalse>,
										And<POLine.voucheredQty, LessEqual<POLine.orderQty>>>>>>>>.Select(this, order.OrderType, order.OrderNbr))
			{
				POLine orderline = (POLine)res;
				APTran tran = AddPOReceiptLine(orderline, duplicates);
			}
			//Copying of taxes from original PO Order doesn't have much sense - Invoice is not created from the PO Order directly 
			//and PO Order most likely has  items of other types			
			this.Document.Cache.RaiseFieldUpdated<APInvoice.curyOrigDocAmt>(doc, 0m);
		}

		public virtual void InvoiceLandedCost(LandedCostTran aTran, DocumentList<APInvoice> list, bool saveAndAdd)
		{
			APInvoice newdoc;
			if (!(String.IsNullOrEmpty(aTran.APDocType) || string.IsNullOrEmpty(aTran.APRefNbr))) return; // Invoice is already created
			LandedCostCode LCCode = PXSelect<LandedCostCode, Where<LandedCostCode.landedCostCodeID, Equal<Required<LandedCostCode.landedCostCodeID>>>>.Select(this, aTran.LandedCostCodeID);
			CurrencyInfo tranCuryInfo = null;
			if (aTran.CuryInfoID != null)
				tranCuryInfo = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<LandedCostTran.curyInfoID>>>>.Select(this, aTran.CuryInfoID);

            string apDocType = aTran.CuryLCAmount > Decimal.Zero ? APDocType.Invoice : APDocType.DebitAdj;
			string terms = String.IsNullOrEmpty(aTran.TermsID) ? LCCode.TermsID : aTran.TermsID;
			if (list != null)
			{
				bool billSeparately = false;
				if (billSeparately == false)
				{
					
					newdoc = list.Find<APInvoice.docType, APInvoice.vendorID, APInvoice.vendorLocationID, APInvoice.curyID, APInvoice.invoiceNbr, APInvoice.docDate, APInvoice.termsID>(apDocType, aTran.VendorID, aTran.VendorLocationID, aTran.CuryID, aTran.InvoiceNbr,aTran.InvoiceDate, aTran.TermsID)?? new APInvoice();
				}
				else
				{
					newdoc = new APInvoice();
				}

				if (newdoc.RefNbr != null)
				{
					this.Document.Current = this.Document.Search<APInvoice.refNbr>(newdoc.RefNbr, newdoc.DocType);
				}
				else
				{
					newdoc.DocType = apDocType;
					newdoc.DocDate = aTran.InvoiceDate;
					newdoc = PXCache<APInvoice>.CreateCopy(this.Document.Insert(newdoc));
					newdoc.VendorID = aTran.VendorID;
					newdoc.VendorLocationID = aTran.VendorLocationID;
                    if(apDocType!= AP.APDocType.DebitAdj)
					    newdoc.TermsID = terms;
					newdoc.InvoiceNbr = aTran.InvoiceNbr;
					newdoc.CuryOrigDocAmt = Math.Abs(aTran.CuryLCAmount.Value);
					if (tranCuryInfo == null)
						newdoc.CuryID = aTran.CuryID;
					newdoc = this.Document.Update(newdoc);
					if (tranCuryInfo != null)
					{
						foreach (CurrencyInfo info in this.currencyinfo.Select())
						{
							PXCache<CurrencyInfo>.RestoreCopy(info, tranCuryInfo);
							info.CuryInfoID = newdoc.CuryInfoID;
							newdoc.CuryID = info.CuryID;
						}
					}
				}
			}
			else
			{
                if(this.Document.Current != null && this.Document.Current.DocType == apDocType )
                {
                    newdoc = PXCache<APInvoice>.CreateCopy(this.Document.Current);
                }
                else
                {
                    newdoc = new APInvoice();
                    newdoc.DocType = apDocType;
                    newdoc.DocDate = aTran.InvoiceDate;
                    newdoc = PXCache<APInvoice>.CreateCopy(this.Document.Insert(newdoc));					
                }

				if (newdoc.VendorID == null && newdoc.VendorLocationID == null)
				{
					newdoc.DocDate = aTran.InvoiceDate;
					newdoc.VendorID = aTran.VendorID;
					newdoc.VendorLocationID = aTran.VendorLocationID;
                    if (apDocType != AP.APDocType.DebitAdj)
					    newdoc.TermsID = terms;
					newdoc.InvoiceNbr = aTran.InvoiceNbr;

					newdoc.CuryOrigDocAmt = Math.Abs(aTran.CuryLCAmount.Value);
					if (tranCuryInfo == null)
						newdoc.CuryID = aTran.CuryID;
					newdoc = this.Document.Update(newdoc);
					if (tranCuryInfo != null)
					{
						foreach (CurrencyInfo info in this.currencyinfo.Select())
						{
							PXCache<CurrencyInfo>.RestoreCopy(info, tranCuryInfo);
							info.CuryInfoID = newdoc.CuryInfoID;
							newdoc.CuryID = info.CuryID;
						}
					}
				}
			}
			newdoc = this.Document.Current;
			newdoc.InvoiceNbr = aTran.InvoiceNbr;
			APTran tran = this.AddLandedCostTran(aTran, LCCode);
			newdoc.CuryOrigDocAmt = newdoc.CuryDocBal;
			this.Document.Cache.RaiseFieldUpdated<APInvoice.curyOrigDocAmt>(newdoc, 0m);

			if (list != null && saveAndAdd)
			{
				this.Save.Press();
				aTran.APDocType = this.Document.Current.DocType;
				aTran.APRefNbr = this.Document.Current.RefNbr;
				if(!aTran.APCuryInfoID.HasValue)
					aTran.APCuryInfoID = this.Document.Current.CuryInfoID;
				if (list.Find(this.Document.Current) == null)
				{
					list.Add(this.Document.Current);
				}
			}
		}

		#endregion

		#region Utility Functions
		public virtual APTran AddPOReceiptLine(IAPTranSource aLine, HashSet<APTran> checkForDuplicates)
		{
			APTran newtran = new APTran();
			aLine.SetReferenceKeyTo(newtran);

			if (checkForDuplicates != null)
			{
				if (checkForDuplicates.Contains(newtran))
				{
					return null;
				}
			}
			newtran.BranchID = aLine.BranchID;
			newtran.AccountID = aLine.POAccrualAcctID ?? aLine.ExpenseAcctID;
			newtran.SubID = aLine.POAccrualSubID ?? aLine.ExpenseSubID;
			newtran.LineType = aLine.LineType;
			newtran.InventoryID = aLine.InventoryID;
			newtran.UOM = string.IsNullOrEmpty(aLine.UOM) ? null : aLine.UOM;
			newtran.Qty = aLine.BillQty;
			newtran.CuryUnitCost = aLine.CuryUnitCost;
			newtran.CuryLineAmt = aLine.CuryLineAmt;
			newtran.TranDesc = aLine.TranDesc;
			newtran.TaxCategoryID = aLine.TaxCategoryID;
			newtran.TaxID = aLine.TaxID;
			if (insetup.Current.UpdateGL != true || 
				aLine.LineType == POLineType.GoodsForDropShip || 
				aLine.LineType == POLineType.NonStockForDropShip)
			{
				if (aLine.ProjectID != null)
				{
					newtran.ProjectID = aLine.ProjectID;
					newtran.TaskID = aLine.TaskID;
				}
				else
				{
					newtran.ProjectID = PM.GLProjectDefaultAttribute.NonProject(this);
				}
			}
			else
			{
				newtran.ProjectID = PM.GLProjectDefaultAttribute.NonProject(this);
			}
			newtran = this.Transactions.Insert(newtran);
			//Special handling for the TaxCategoryID intentionally cleared in PO Receipt to prevent it's defaulting in AP
			if (aLine.TaxCategoryID == null && newtran.TaxCategoryID != null)
			{
				//APTran copy =(APTran) this.Transactions.Cache.CreateCopy(newtran);
				//copy.TaxCategoryID = aLine.TaxCategoryID;
				//copy = this.Transactions.Update(copy);
				//return copy;
			}
			return newtran;
		}

		public virtual APTran AddLandedCostTran(LandedCostTran aLine, LandedCostCode aLCCode)
		{

			APTran newtran = new APTran();
			Copy(newtran, aLine, aLCCode);
			newtran = this.Transactions.Insert(newtran);
			return newtran;
		}

		public static void Copy(APTran aDest, LandedCostTran aSrc, LandedCostCode aLCCode)
		{
			aDest.AccountID = aSrc.LCAccrualAcct.HasValue? aSrc.LCAccrualAcct: aLCCode.LCAccrualAcct;
			aDest.SubID = aSrc.LCAccrualAcct.HasValue ? aSrc.LCAccrualSub : aLCCode.LCAccrualSub;
			aDest.UOM = null;
			aDest.Qty = Decimal.One;
			aDest.CuryUnitCost = Math.Abs(aSrc.CuryLCAPAmount.Value);
			aDest.CuryTranAmt = Math.Abs(aSrc.CuryLCAPAmount.Value);
			aDest.TranDesc = aSrc.Descr;
			aDest.InventoryID = null;
			aDest.TaxCategoryID = aSrc.TaxCategoryID;
			aDest.TaxID = aSrc.TaxID;
			aDest.ReceiptNbr = aSrc.POReceiptNbr;
			aDest.ReceiptLineNbr = null;
			aDest.LCTranID = aSrc.LCTranID;
			aDest.LandedCostCodeID = aSrc.LandedCostCodeID;
		}


#if false
		public static void CopyCorrection(APTran aDest, LandedCostTran aSrc, Decimal aCuryAmount, Decimal aBaseAmount)
		{
			aDest.UOM = null;
			aDest.Qty = Decimal.One;
			aDest.CuryUnitCost = aCuryAmount;
			aDest.CuryTranAmt = aCuryAmount;
			aDest.UnitCost = aBaseAmount;
			aDest.TranAmt = aBaseAmount;
			aDest.TranDesc = "Correction";
			aDest.InventoryID = null;
			aDest.TaxCategoryID = null;
			aDest.ReceiptNbr = aSrc.POReceiptNbr;
			aDest.ReceiptLineNbr = null;
			aDest.LCTranID = aSrc.LCTranID;
			aDest.LandedCostCodeID = aSrc.LandedCostCodeID;
			aDest.LCAdjustment = true;
		}

		public static void CopyCorrections(CurrencyInfo aCuryInfo, LandedCostTran aSrc, LandedCostCode aLCCode, ref APTran aDest1, ref APTran aDest2)
		{
			decimal baseAmount = (aSrc.LCAPAmount ?? 0m) - (aSrc.LCAmount ?? 0m);
			decimal curyAmount;
			PXCurrencyAttribute.CuryConvCury(aCuryInfo, baseAmount, out curyAmount);
			if (curyAmount != Decimal.Zero || baseAmount != decimal.Zero || aDest1 != null || aDest2 != null)
			{
				if (aDest1 == null)
				{
					aDest1 = new APTran();
					aDest1.AccountID = aSrc.LCAccrualAcct.HasValue ? aSrc.LCAccrualAcct : aLCCode.LCAccrualAcct;
					aDest1.SubID = aSrc.LCAccrualAcct.HasValue ? aSrc.LCAccrualSub : aLCCode.LCAccrualSub;
				}
				if (aDest2 == null)
				{
					aDest2 = new APTran();
					aDest2.AccountID = aLCCode.LCVarianceAcct; // Variance here
					aDest2.SubID = aLCCode.LCVarianceSub;		 // Variance here	
				}
				CopyCorrection(aDest1, aSrc, -curyAmount, -baseAmount);
				CopyCorrection(aDest2, aSrc, curyAmount, baseAmount);
			}
		}
		
#endif
		private InventoryItem InventoryItemGetByID(int? inventoryID)
		{
			return PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, inventoryID);
		}

		private DRDeferredCode DeferredCodeGetByID(string deferredCodeID)
		{
			return PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Required<DRDeferredCode.deferredCodeID>>>>.Select(this, deferredCodeID);
		}
		#endregion

        protected virtual void RecalculateDiscounts(PXCache sender, APTran line)
        {
            if (PXAccess.FeatureInstalled<FeaturesSet.vendorDiscounts>() && line.InventoryID != null && line.Qty != null && line.CuryLineAmt != null)
            {
                DiscountEngine<APTran>.SetDiscounts<APInvoiceDiscountDetail>(sender, Transactions, line, DiscountDetails, Document.Current.VendorLocationID, Document.Current.CuryID, Document.Current.DocDate.Value, Document.Current.SkipDiscounts, true, recalcdiscountsfilter.Current);

                RecalculateTotalDiscount();
            }
        }

        private void RecalculateTotalDiscount()
        {
            if (Document.Current != null)
            {
                APInvoice old_row = PXCache<APInvoice>.CreateCopy(Document.Current);
                Document.Cache.SetValueExt<APInvoice.curyDiscTot>(Document.Current, GetTotalGroupAndDocumentDiscount());
                Document.Cache.RaiseRowUpdated(Document.Current, old_row);
            }
        }

        private decimal GetTotalGroupAndDocumentDiscount()
        {
            decimal total = 0;
            if (Document.Current != null)
            {
                foreach (APInvoiceDiscountDetail record in PXSelect<APInvoiceDiscountDetail,
                        Where<APInvoiceDiscountDetail.docType, Equal<Current<APTran.tranType>>,
                        And<APInvoiceDiscountDetail.refNbr, Equal<Current<APTran.refNbr>>>>>.Select(this))
                {
                    total += record.CuryDiscountAmt ?? 0;
                }
            }
            return total;
        }

        protected virtual void AddDiscount(PXCache sender, APInvoice row)
        {
            APTran discount = (APTran)Discount_Row.Cache.CreateInstance();
            discount.LineType = SOLineType.Discount;
            discount.DrCr = (Document.Current.DrCr == "D") ? "C" : "D";
            discount = (APTran)Discount_Row.Select() ?? (APTran)Discount_Row.Cache.Insert(discount);

            APTran old_row = (APTran)Discount_Row.Cache.CreateCopy(discount);

            discount.CuryTranAmt = (decimal?)sender.GetValue<APInvoice.curyDiscTot>(row);
            discount.TaxCategoryID = null;
            discount.TranDesc = PXMessages.LocalizeNoPrefix(Messages.DocDiscDescr);

            DefaultDiscountAccountAndSubAccount(discount);

            //ToDo: create separate discount lines for different projects 
            if (discount.ProjectID == null)
            {
                foreach (APTran tran in Transactions.Select())
                {
                    if (tran.ProjectID != null && !PM.ProjectDefaultAttribute.IsNonProject(this, tran.ProjectID))
                        discount.ProjectID = tran.ProjectID;
                }
            }

            if (discount.ProjectID == null && old_row.ProjectID != null)
                discount.ProjectID = old_row.ProjectID;

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

        private void DefaultDiscountAccountAndSubAccount(APTran tran)
        {
            Location vendorloc = location.Current;

            object vendor_LocationAcctID = GetValue<Location.vDiscountAcctID>(vendorloc);

            if (vendor_LocationAcctID != null)
            {
                tran.AccountID = (int?)vendor_LocationAcctID;
                Discount_Row.Cache.RaiseFieldUpdated<APTran.accountID>(tran, null);
            }

            if (tran.AccountID != null)
            {
                object vendor_LocationSubID = GetValue<Location.vDiscountSubID>(vendorloc);
                if (vendor_LocationSubID != null)
                {
                    tran.SubID = (int?)vendor_LocationSubID;
                    Discount_Row.Cache.RaiseFieldUpdated<APTran.subID>(tran, null);
                }
            }

        }

#if false
		public virtual APTran AddPOOrderLine(POLine aLine, bool checkForDuplicates)
		{
			if (checkForDuplicates)
			{
				foreach (APTran iTr in this.Transactions.Select())
				{
					if (CompareSourceKey(iTr, aLine))
						return iTr;
				}
			}

			APTran newtran = new APTran();
			newtran.AccountID = aLine.ExpenseAcctID;
			newtran.SubID = aLine.ExpenseSubID;
			newtran.POOrderType = aLine.OrderType;
			newtran.PONbr = aLine.OrderNbr;
			newtran.POLineNbr = aLine.LineNbr;
			newtran.LineType = aLine.LineType;
			newtran.InventoryID = aLine.InventoryID;
			newtran.UOM = aLine.UOM;
			newtran.Qty = aLine.OrderQty - aLine.VoucheredQty;
			newtran.CuryUnitCost = aLine.CuryUnitCost;
			newtran.CuryTranAmt = (aLine.CuryExtCost - aLine.CuryReceivedCost);
			newtran.TranDesc = aLine.TranDesc;
			newtran.TaxCategoryID = aLine.TaxCategoryID;

			newtran = this.Transactions.Insert(newtran);
			return newtran;
		}

		static bool CompareSourceKey(APTran aTran, POLine aLine)
		{
			return (aTran.PONbr == aLine.OrderNbr && aTran.POOrderType == aLine.OrderType && aTran.POLineNbr == aLine.LineNbr);
		}

		static bool CompareSourceKey(APTran aTran, POReceiptLine aLine)
		{
			return (aTran.ReceiptNbr == aLine.ReceiptNbr && aTran.ReceiptLineNbr == aLine.LineNbr);
		} 
#endif	

		#region Private Variables
		private bool _allowToVoidReleased = false; 
		#endregion

		#region Internal Member Definitions
        #region Order Selection
        [Serializable]
        public partial class POOrderFilter : IBqlTable
        {
            #region OrderNbr
            public abstract class orderNbr : PX.Data.IBqlField
            {
            }
            protected String _OrderNbr;
            [PXDBString(15, IsUnicode = true, InputMask = "")]
            [PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
            [PXSelector(typeof(Search5<POReceiptLineS.pONbr,
                InnerJoin<POReceipt, On<POReceipt.receiptNbr, Equal<POReceiptLineS.receiptNbr>>,
                InnerJoin<POOrder, On<POOrder.orderNbr, Equal<POReceiptLineS.pONbr>>,
                LeftJoin<APTran, On<APTran.receiptNbr, Equal<POReceiptLineS.receiptNbr>,
                    And<APTran.receiptLineNbr, Equal<POReceiptLineS.lineNbr>,
                    And<APTran.released, Equal<boolFalse>>>>>>>,
                Where<POReceipt.vendorID, Equal<Current<APInvoice.vendorID>>,
                     And<POReceipt.vendorLocationID, Equal<Optional<APInvoice.vendorLocationID>>,
                     And<POReceipt.curyID, Equal<Optional<APInvoice.curyID>>,
                     And<POReceipt.hold, Equal<CS.boolFalse>,
                     And<POReceipt.released, Equal<CS.boolTrue>,
                     And<APTran.refNbr, IsNull,
                     And<Where<POReceiptLineS.unbilledQty, Greater<decimal0>,
                     Or<POReceiptLineS.curyUnbilledAmt, NotEqual<decimal0>>>>>>>>>>,
                Aggregate<GroupBy<POReceiptLineS.pONbr>>>),
               typeof(POReceiptLineS.pONbr),
               typeof(POOrder.orderDate),
               typeof(POOrder.vendorID),
               typeof(POOrder.vendorID_Vendor_acctName),
               typeof(POOrder.vendorLocationID),
               typeof(POOrder.curyID),
               typeof(POOrder.curyOrderTotal), Filterable = true)]
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
        #endregion
        #region Receipt +Receipt Line Selection

        [Serializable]
		public partial class POReceiptFilter : IBqlTable
		{
			#region VendorID
			public abstract class vendorID : PX.Data.IBqlField
			{
			}

			protected Int32? _VendorID;
			[VendorActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true)]
			[PXDefault(typeof(APInvoice.vendorID))]
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

			#region ReceiptType
			public abstract class receiptType : PX.Data.IBqlField
			{
			}
			protected String _ReceiptType;
			[PXDBString(2, IsFixed = true)]
			[PXDefault(POReceiptType.POReceipt)]			
			[POReceiptType.List]
			[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual String ReceiptType
			{
				get
				{
					return this._ReceiptType;
				}
				set
				{
					this._ReceiptType = value;
				}
			}
			#endregion
			#region ReceiptNbr
			public abstract class receiptNbr : PX.Data.IBqlField
			{
			}
			protected String _ReceiptNbr;
			[PXDBString(15, IsUnicode = true, InputMask = "")]
			[PXDefault()]
			[PXUIField(DisplayName = "Receipt Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
			[PXSelector(typeof(Search2<POReceiptS.receiptNbr,
			InnerJoin<Vendor, On<POReceiptS.vendorID, Equal<Vendor.bAccountID>>>,
			Where<POReceiptS.vendorID, Equal<Current<APInvoice.vendorID>>,
				And<POReceiptS.vendorLocationID, Equal<Optional<APInvoice.vendorLocationID>>,				
				And<POReceiptS.curyID, Equal<Optional<APInvoice.curyID>>,
				And<Where2<Where<POReceiptS.receiptType, Equal<POReceiptType.poreceipt>,And<Optional<APInvoice.docType>, Equal<APInvoiceType.invoice>>>,
				   Or<Where<POReceiptS.receiptType, Equal<POReceiptType.poreturn>, 
				 And<Optional<APInvoice.docType>, Equal<APInvoiceType.debitAdj>>>>>
				>>>>>),
				typeof(POReceiptS.receiptNbr),
				typeof(POReceiptS.receiptDate),
				typeof(POReceiptS.vendorID),
				typeof(POReceiptS.vendorID_Vendor_acctName),
				typeof(POReceiptS.vendorLocationID),
				typeof(POReceiptS.curyID),
				typeof(POReceiptS.curyUnbilledAmtL), Filterable = true)]
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
		}

		[PXProjection(typeof(Select5<POReceipt,
										InnerJoin<POReceiptLineS, On<POReceiptLineS.receiptNbr, Equal<POReceipt.receiptNbr>,
												And<Where<POReceiptLineS.unbilledQty, Greater<decimal0>,
													Or<POReceiptLineS.curyUnbilledAmt, NotEqual<decimal0>>>>>,
										LeftJoin<APTran, On<APTran.receiptNbr, Equal<POReceiptLineS.receiptNbr>,
														And<APTran.receiptLineNbr, Equal<POReceiptLineS.lineNbr>,
														And<APTran.released, Equal<boolFalse>>>>>>,
										Where<POReceipt.released, Equal<boolTrue>,
											And<APTran.refNbr, IsNull>>,
											Aggregate
												<GroupBy<POReceipt.receiptNbr,
												GroupBy<POReceipt.receiptDate,
												GroupBy<POReceipt.curyID,
												GroupBy<POReceipt.curyOrderTotal,
												GroupBy<POReceipt.hold,
												Sum<POReceiptLineS.receiptQty,
												Sum<POReceiptLineS.curyUnbilledAmt,
												Sum<POReceiptLineS.unbilledQty,
												Sum<POReceiptLineS.curyExtCost
												>>>>>>>>>>>), Persistent = false)]
        [Serializable]
		public partial class POReceiptS : IBqlTable
		{
			#region Selected
			public abstract class selected : PX.Data.IBqlField
			{
			}
			protected bool? _Selected = false;
			[PXBool]
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
			#region ReceiptNbr
			public abstract class receiptNbr : PX.Data.IBqlField
			{
			}
			protected String _ReceiptNbr;
			[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(POReceipt.receiptNbr))]
			[POReceiptType.RefNbr(typeof(Search<POReceiptS.receiptNbr>), Filterable = true)]
			[PXUIField(DisplayName = "Receipt Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
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
			#region ReceiptType
			public abstract class receiptType : PX.Data.IBqlField
			{
			}
			protected String _ReceiptType;
			[PXDBString(2, IsFixed = true, BqlField = typeof(POReceipt.receiptType))]
			[POReceiptType.List()]
			public virtual String ReceiptType
			{
				get
				{
					return this._ReceiptType;
				}
				set
				{
					this._ReceiptType = value;
				}
			}
			#endregion
			#region VendorID
			public abstract class vendorID : PX.Data.IBqlField
			{
			}
			protected Int32? _VendorID;
			[VendorActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true, BqlField = typeof(POReceipt.vendorID))]
			[PXDefault()]
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
			#region VendorLocationID
			public abstract class vendorLocationID : PX.Data.IBqlField
			{
			}
			protected Int32? _VendorLocationID;
			[LocationID(typeof(Where<Location.bAccountID, Equal<Current<POReceipt.vendorID>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, BqlField = typeof(POReceipt.vendorLocationID))]


			public virtual Int32? VendorLocationID
			{
				get
				{
					return this._VendorLocationID;
				}
				set
				{
					this._VendorLocationID = value;
				}
			}
			#endregion
			#region ReceiptDate
			public abstract class receiptDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _ReceiptDate;
			[PXDBDate(BqlField = typeof(POReceipt.receiptDate))]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
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
			#region Hold
			public abstract class hold : PX.Data.IBqlField
			{
			}
			protected Boolean? _Hold;
			[PXDBBool(BqlField = typeof(POReceipt.hold))]
			[PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.Visible)]

			public virtual Boolean? Hold
			{
				get
				{
					return this._Hold;
				}
				set
				{
					this._Hold = value;

				}
			}
			#endregion
			#region Released
			public abstract class released : PX.Data.IBqlField
			{
			}
			protected Boolean? _Released;
			[PXDBBool(BqlField = typeof(POReceipt.released))]
			[PXUIField(DisplayName = "Released")]
			[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual Boolean? Released
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
			#region CuryID
			public abstract class curyID : PX.Data.IBqlField
			{
			}
			protected String _CuryID;
			[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(POReceipt.curyID))]
			[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
			[PXSelector(typeof(Currency.curyID))]
			public virtual String CuryID
			{
				get
				{
					return this._CuryID;
				}
				set
				{
					this._CuryID = value;
				}
			}
			#endregion

			#region VoucheredQty
			public abstract class voucheredQty : PX.Data.IBqlField
			{
			}
			protected Decimal? _VoucheredQty;
			[PXDBQuantity(BqlField = typeof(POReceiptLine.voucheredQty))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Vouchered Qty.", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Decimal? VoucheredQty
			{
				get
				{
					return this._VoucheredQty;
				}
				set
				{
					this._VoucheredQty = value;
				}
			}
			#endregion
			#region CuryInfoID
			public abstract class curyInfoID : PX.Data.IBqlField
			{
			}
			protected Int64? _CuryInfoID;
			[PXDBLong(BqlField = typeof(POReceiptLine.curyInfoID))]
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
			#region CuryVoucheredCost
			public abstract class curyVoucheredCost : PX.Data.IBqlField
			{
			}
			protected Decimal? _CuryVoucheredCost;
			[PXDBDecimal(6, BqlField = typeof(POReceiptLine.curyVoucheredCost))]
			[PXUIField(DisplayName = "Vouchered Amt", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? CuryVoucheredCost
			{
				get
				{
					return this._CuryVoucheredCost;
				}
				set
				{
					this._CuryVoucheredCost = value;
				}
			}
			#endregion
			#region VoucheredCost
			public abstract class voucheredCost : PX.Data.IBqlField
			{
			}
			protected Decimal? _VoucheredCost;
			[PXDBDecimal(6, BqlField = typeof(POReceiptLine.voucheredCost))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Vouchered Cost")]
			public virtual Decimal? VoucheredCost
			{
				get
				{
					return this._VoucheredCost;
				}
				set
				{
					this._VoucheredCost = value;
				}
			}
			#endregion

			#region UnbilledQty
			public abstract class unbilledQtyL : PX.Data.IBqlField
			{
			}
			protected Decimal? _UnbilledQtyL;
			[PXDBQuantity(BqlField = typeof(POReceiptLineS.unbilledQty))]
			[PXUIField(DisplayName = "Unbilled Qty.", Enabled = false)]
			public virtual Decimal? UnbilledQtyL
			{
				get
				{
					return this._UnbilledQtyL;
				}
				set
				{
					this._UnbilledQtyL = value;
				}
			}
			#endregion
			#region CuryUnbilledAmt
			public abstract class curyUnbilledAmtL : PX.Data.IBqlField
			{
			}
			protected Decimal? _CuryUnbilledAmtL;
			[PXDBCurrency(typeof(POReceiptS.curyInfoID), typeof(POReceiptS.unbilledAmt), BqlField = typeof(POReceiptLineS.curyUnbilledAmt))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Unbilled Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Decimal? CuryUnbilledAmtL
			{
				get
				{
					return this._CuryUnbilledAmtL;
				}
				set
				{
					this._CuryUnbilledAmtL = value;
				}
			}
			#endregion
			#region UnbilledAmt
			public abstract class unbilledAmt : PX.Data.IBqlField
			{
			}
			protected Decimal? _UnbilledAmt;
			[PXDBBaseCury(BqlField = typeof(POReceiptLine.unbilledAmt))]
			[PXDefault(TypeCode.Decimal, "0.0")]
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
			#region VendorID_Vendor_acctName
			public abstract class vendorID_Vendor_acctName : PX.Data.IBqlField
			{
			}
			#endregion
			#region CuryOrderTotal
			public abstract class curyOrderTotal : PX.Data.IBqlField
			{
			}
			protected Decimal? _CuryOrderTotal;

			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXDBCurrency(typeof(POReceiptS.curyInfoID), typeof(POReceiptS.orderTotal), BqlField = typeof(POReceipt.curyOrderTotal))]
			[PXUIField(DisplayName = "Total Amt.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual Decimal? CuryOrderTotal
			{
				get
				{
					return this._CuryOrderTotal;
				}
				set
				{
					this._CuryOrderTotal = value;
				}
			}
			#endregion
			#region OrderTotal
			public abstract class orderTotal : PX.Data.IBqlField
			{
			}
			protected Decimal? _OrderTotal;

			[PXDBBaseCury(BqlField = typeof(POReceipt.orderTotal))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? OrderTotal
			{
				get
				{
					return this._OrderTotal;
				}
				set
				{
					this._OrderTotal = value;
				}
			}
			#endregion
			#region OrderQty
			public abstract class orderQty : PX.Data.IBqlField
			{
			}
			protected Decimal? _OrderQty;
			[PXDBQuantity(BqlField = typeof(POReceipt.orderQty))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Total Qty.")]
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
			#region CuryLineTotal
			public abstract class curyLineTotal : PX.Data.IBqlField
			{
			}
			protected Decimal? _CuryLineTotal;

			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXDBCurrency(typeof(POReceiptS.curyInfoID), typeof(POReceiptS.lineTotal), BqlField = typeof(POReceipt.curyLineTotal))]
			[PXUIField(DisplayName = "Lines Total", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual Decimal? CuryLineTotal
			{
				get
				{
					return this._CuryLineTotal;
				}
				set
				{
					this._CuryLineTotal = value;
				}
			}
			#endregion
			#region LineTotal
			public abstract class lineTotal : PX.Data.IBqlField
			{
			}
			protected Decimal? _LineTotal;
			[PXDBBaseCury(BqlField = typeof(POReceipt.lineTotal))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? LineTotal
			{
				get
				{
					return this._LineTotal;
				}
				set
				{
					this._LineTotal = value;
				}
			}
			#endregion

		}

		//Read-only class for selector
		[PXProjection(typeof(Select<POReceiptLine>), Persistent = false)]
        [Serializable]
		public partial class POReceiptLineS : IBqlTable, IAPTranSource
		{
			#region Selected
			public abstract class selected : PX.Data.IBqlField
			{
			}
			protected bool? _Selected = false;
			[PXBool]
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
			#region BranchID
			public abstract class branchID : PX.Data.IBqlField
			{
			}
			protected Int32? _BranchID;
			[Branch(BqlField = typeof(POReceiptLine.branchID))]
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
			#region ReceiptNbr
			public abstract class receiptNbr : PX.Data.IBqlField
			{
			}
			protected String _ReceiptNbr;
			[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(POReceiptLine.receiptNbr))]
			[PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.Invisible, Visible = false)]
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
			#region ReceiptType
			public abstract class receiptType : PX.Data.IBqlField
			{
			}
			protected String _ReceiptType;
			[PXDBString(2, IsFixed = true, BqlField = typeof(POReceiptLine.receiptType))]
			public virtual String ReceiptType
			{
				get
				{
					return this._ReceiptType;
				}
				set
				{
					this._ReceiptType = value;
				}
			}
			#endregion
			#region LineNbr
			public abstract class lineNbr : PX.Data.IBqlField
			{
			}
			protected Int32? _LineNbr;
			[PXDBInt(IsKey = true, BqlField = typeof(POReceiptLine.lineNbr))]
			[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
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
			[PXDBString(2, IsFixed = true, BqlField = typeof(POReceiptLine.lineType))]
			[PXDefault(POLineType.GoodsForInventory)]
			[POLineType.List()]
			[PXUIField(DisplayName = "Line Type")]
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
			#region InventoryID
			public abstract class inventoryID : PX.Data.IBqlField
			{
			}
			protected Int32? _InventoryID;
			[Inventory(Filterable = true, BqlField = typeof(POReceiptLine.inventoryID))]
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
			#region VendorID
			public abstract class vendorID : PX.Data.IBqlField
			{
			}
			protected Int32? _VendorID;
			[Vendor(BqlField = typeof(POReceiptLine.vendorID))]
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
			#region ReceiptDate
			public abstract class receiptDate : PX.Data.IBqlField
			{
			}

			protected DateTime? _ReceiptDate;
			[PXDBDate(BqlField = typeof(POReceiptLine.receiptDate))]
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
			#region SubItemID
			public abstract class subItemID : PX.Data.IBqlField
			{
			}
			protected Int32? _SubItemID;
			[SubItem(typeof(POReceiptLineS.inventoryID), BqlField = typeof(POReceiptLine.subItemID))]
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
			[IN.SiteAvail(typeof(POReceiptLineS.inventoryID), typeof(POReceiptLineS.subItemID), BqlField = typeof(POReceiptLine.siteID))]
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
			#region UOM
			public abstract class uOM : PX.Data.IBqlField
			{
			}
			protected String _UOM;
			[INUnit(typeof(POReceiptLineS.inventoryID), BqlField = typeof(POReceiptLine.uOM))]
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
			#region ReceiptQty
			public abstract class receiptQty : PX.Data.IBqlField
			{
			}

			protected Decimal? _ReceiptQty;
			[PXDBQuantity(typeof(POReceiptLineS.uOM), typeof(POReceiptLineS.baseReceiptQty), HandleEmptyKey = true, BqlField = typeof(POReceiptLine.receiptQty))]
			[PXUIField(DisplayName = "Receipt Qty.", Visibility = PXUIVisibility.Visible)]
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
			#region BaseReceiptQty
			public abstract class baseReceiptQty : PX.Data.IBqlField
			{
			}

			protected Decimal? _BaseReceiptQty;
			[PXDBDecimal(6, BqlField = typeof(POReceiptLine.baseReceiptQty))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? BaseReceiptQty
			{
				get
				{
					return this._BaseReceiptQty;
				}
				set
				{
					this._BaseReceiptQty = value;
				}
			}

			public virtual Decimal? BaseQty
			{
				get
				{
					return this._BaseReceiptQty;
				}
				set
				{
					this._BaseReceiptQty = value;
				}
			}
			#endregion

			#region CuryInfoID
			public abstract class curyInfoID : PX.Data.IBqlField
			{
			}
			protected Int64? _CuryInfoID;
			[PXDBLong(BqlField = typeof(POReceiptLine.curyInfoID))]
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
			#region CuryUnitCost
			public abstract class curyUnitCost : PX.Data.IBqlField
			{
			}
			protected Decimal? _CuryUnitCost;

			[PXDBDecimal(6, BqlField = typeof(POReceiptLine.curyUnitCost))]
			[PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.SelectorVisible)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? CuryUnitCost
			{
				get
				{
					return this._CuryUnitCost;
				}
				set
				{
					this._CuryUnitCost = value;
				}
			}
			#endregion
			#region CuryExtCost
			public abstract class curyExtCost : PX.Data.IBqlField
			{
			}
			protected Decimal? _CuryExtCost;
			[PXDBCurrency(typeof(POReceiptLineS.curyInfoID), typeof(POReceiptLineS.extCost), BqlField = typeof(POReceiptLine.curyExtCost))]
			[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? CuryExtCost
			{
				get
				{
					return this._CuryExtCost;
				}
				set
				{
					this._CuryExtCost = value;
				}
			}



			#endregion
			#region ExtCost
			public abstract class extCost : PX.Data.IBqlField
			{
			}

			protected Decimal? _ExtCost;

			[PXDBBaseCury(BqlField = typeof(POReceiptLine.extCost))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? ExtCost
			{
				get
				{
					return this._ExtCost;
				}
				set
				{
					this._ExtCost = value;
				}
			}
			#endregion

			#region UnbilledQty
			public abstract class unbilledQty : PX.Data.IBqlField
			{
			}
			protected Decimal? _UnbilledQty;
			[PXDBQuantity(typeof(POReceiptLineS.uOM), typeof(POReceiptLineS.baseUnbilledQty), HandleEmptyKey = true, BqlField = typeof(POReceiptLine.unbilledQty))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Unbilled Qty.", Enabled = false)]
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

			public virtual Decimal? BillQty
			{
				get
				{
					return this._UnbilledQty;
				}
			}
			#endregion
			#region BaseUnbilledQty
			public abstract class baseUnbilledQty : PX.Data.IBqlField
			{
			}
			protected Decimal? _BaseUnbilledQty;
			[PXDBDecimal(6, BqlField = typeof(POReceiptLine.baseUnbilledQty))]
			[PXDefault(TypeCode.Decimal, "0.0")]
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
			#region CuryUnbilledAmt
			public abstract class curyUnbilledAmt : PX.Data.IBqlField
			{
			}
			protected Decimal? _CuryUnbilledAmt;
			[PXDBCurrency(typeof(POReceiptLineS.curyInfoID), typeof(POReceiptLineS.unbilledAmt), BqlField = typeof(POReceiptLine.curyUnbilledAmt))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Unbilled Amount", Enabled = false)]
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

			public virtual Decimal? CuryLineAmt
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
			[PXDBBaseCury(BqlField = typeof(POReceiptLine.unbilledAmt))]
			[PXDefault(TypeCode.Decimal, "0.0")]
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

			#region TaxCategoryID
			public abstract class taxCategoryID : PX.Data.IBqlField
			{
			}
			protected String _TaxCategoryID;

			[PXDBString(10, IsUnicode = true, BqlField = typeof(POReceiptLine.taxCategoryID))]
			[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
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
			#region ExpenseAcctID
			public abstract class expenseAcctID : PX.Data.IBqlField
			{
			}
			protected Int32? _ExpenseAcctID;

			[PXDBInt(BqlField = typeof(POReceiptLine.expenseAcctID))]
			public virtual Int32? ExpenseAcctID
			{
				get
				{
					return this._ExpenseAcctID;
				}
				set
				{
					this._ExpenseAcctID = value;
				}
			}
			#endregion
			#region ExpenseSubID
			public abstract class expenseSubID : PX.Data.IBqlField
			{
			}
			protected Int32? _ExpenseSubID;

			[PXDBInt(BqlField = typeof(POReceiptLine.expenseSubID))]
			public virtual Int32? ExpenseSubID
			{
				get
				{
					return this._ExpenseSubID;
				}
				set
				{
					this._ExpenseSubID = value;
				}
			}
			#endregion
			#region POAccrualAcctID
			public abstract class pOAccrualAcctID : PX.Data.IBqlField
			{
			}
			protected Int32? _POAccrualAcctID;
			[PXDBInt(BqlField = typeof(POReceiptLine.pOAccrualAcctID))]
			public virtual Int32? POAccrualAcctID
			{
				get
				{
					return this._POAccrualAcctID;
				}
				set
				{
					this._POAccrualAcctID = value;
				}
			}
			#endregion
			#region POAccrualSubID
			public abstract class pOAccrualSubID : PX.Data.IBqlField
			{
			}
			protected Int32? _POAccrualSubID;
			[PXDBInt(BqlField = typeof(POReceiptLine.pOAccrualSubID))]
			public virtual Int32? POAccrualSubID
			{
				get
				{
					return this._POAccrualSubID;
				}
				set
				{
					this._POAccrualSubID = value;
				}
			}
			#endregion
			#region TranDesc
			public abstract class tranDesc : PX.Data.IBqlField
			{
			}
			protected String _TranDesc;
			[PXDBString(256, IsUnicode = true, BqlField = typeof(POReceiptLine.tranDesc))]
			[PXUIField(DisplayName = "Transaction Descr.", Visibility = PXUIVisibility.Visible)]
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
			#region TaxID
			public abstract class taxID : PX.Data.IBqlField
			{
			}
			protected String _TaxID;
			[PXDBString(Tax.taxID.Length, IsUnicode = true, BqlField = typeof(POReceiptLine.taxID))]
			[PXUIField(DisplayName = "Tax ID", Visible = false)]			
			public virtual String TaxID
			{
				get
				{
					return this._TaxID;
				}
				set
				{
					this._TaxID = value;
				}
			}
			#endregion

			#region ProjectID
			public abstract class projectID : PX.Data.IBqlField
			{
			}
			protected int? _ProjectID;
			[PXDBInt(BqlField = typeof(POReceiptLine.projectID))]
			public virtual int? ProjectID
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
			#region TaskID
			public abstract class taskID : PX.Data.IBqlField
			{
			}
			protected int? _TaskID;
			[PXDBInt(BqlField = typeof(POReceiptLine.taskID))]
			public virtual int? TaskID
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
		

			#region POType
			public abstract class pOType : PX.Data.IBqlField
			{
			}
			protected String _POType;
			[PXDBString(2, IsFixed = true, BqlField = typeof(POReceiptLine.pOType))]
			[PXUIField(DisplayName = "Order Type")]
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
			[PXDBString(15, IsUnicode = true, BqlField = typeof(POReceiptLine.pONbr))]
			[PXUIField(DisplayName = "Order Nbr.")]
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
			[PXDBInt(BqlField = typeof(POReceiptLine.pOLineNbr))]
			[PXUIField(DisplayName = "PO Line Nbr.")]
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

			public virtual bool CompareReferenceKey(APTran aTran)
			{
				return (aTran.ReceiptNbr == this.ReceiptNbr && aTran.ReceiptLineNbr == this.LineNbr);
			}
			public virtual void SetReferenceKeyTo(APTran aTran)
			{
				aTran.ReceiptNbr = this.ReceiptNbr;
				aTran.ReceiptLineNbr = this.LineNbr;
				aTran.POOrderType = this.POType;
				aTran.PONbr = this.PONbr;
				aTran.POLineNbr = this.POLineNbr;
			}

		}
		#endregion

		#region POOrder + Unbilled Service Items Projection
		[PXBreakInheritance]
		[PXProjection(typeof(Select5<POOrder,
									InnerJoin<POLine, On<POLine.orderType, Equal<POOrder.orderType>,
										And<POLine.orderNbr, Equal<POOrder.orderNbr>,
										And<POLine.lineType, Equal<POLineType.service>,
										And<POLine.cancelled, NotEqual<boolTrue>,
										And<POLine.completed, NotEqual<boolTrue>
										>>>>>,
										LeftJoin<APTran, On<APTran.pOOrderType, Equal<POLine.orderType>,
																	And<APTran.pONbr, Equal<POLine.orderNbr>,
																	And<APTran.pOLineNbr, Equal<POLine.lineNbr>,
																	And<APTran.released, Equal<boolFalse>>>>>>>,
										Where<APTran.refNbr, IsNull>,
										Aggregate
											<GroupBy<POOrder.orderType,
											GroupBy<POOrder.orderNbr,
											GroupBy<POOrder.orderDate,
											GroupBy<POOrder.curyID,
											GroupBy<POOrder.curyOrderTotal,
											GroupBy<POOrder.hold,
											GroupBy<POOrder.receipt,
											GroupBy<POOrder.cancelled,
											Sum<POLine.orderQty,
											Sum<POLine.voucheredQty,
											Sum<POLine.curyVoucheredCost,
											Sum<POLine.voucheredCost,
											Sum<POLine.curyExtCost,
											Sum<POLine.extCost>>>>>>>>>>>>>>>>), Persistent = false)]
        [Serializable]
		public partial class POOrderRS : POOrder
		{
			#region Selected
			public new abstract class selected : PX.Data.IBqlField
			{
			}
			#endregion
			#region OrderType
			public new abstract class orderType : PX.Data.IBqlField
			{
			}
			#endregion
			#region OrderNbr
			public new abstract class orderNbr : PX.Data.IBqlField
			{
			}
			#endregion

			#region LineQty
			public abstract class lineQty : PX.Data.IBqlField
			{
			}
			protected Decimal? _LineQty;
			[PXDBQuantity(HandleEmptyKey = true, BqlField = typeof(POLine.orderQty))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "PO Qty.", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Decimal? LineQty
			{
				get
				{
					return this._LineQty;
				}
				set
				{
					this._LineQty = value;
				}
			}
			#endregion
			#region CuryExtCost
			public abstract class curyExtCost : PX.Data.IBqlField
			{
			}
			protected Decimal? _CuryExtCost;
			[PXDBCurrency(typeof(POOrderRS.curyInfoID), typeof(POOrderRS.extCost), BqlField = typeof(POLine.curyExtCost))]
			[PXUIField(DisplayName = "Ext. Amt.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? CuryExtCost
			{
				get
				{
					return this._CuryExtCost;
				}
				set
				{
					this._CuryExtCost = value;
				}
			}
			#endregion
			#region ExtCost
			public abstract class extCost : PX.Data.IBqlField
			{
			}
			protected Decimal? _ExtCost;
			[PXDBDecimal(6, BqlField = typeof(POLine.extCost))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Ext. Cost")]
			public virtual Decimal? ExtCost
			{
				get
				{
					return this._ExtCost;
				}
				set
				{
					this._ExtCost = value;
				}
			}
			#endregion

			#region VoucheredQty
			public abstract class voucheredQty : PX.Data.IBqlField
			{
			}
			protected Decimal? _VoucheredQty;
			[PXDBQuantity(HandleEmptyKey = true, BqlField = typeof(POLine.voucheredQty))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Billed Qty.", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Decimal? VoucheredQty
			{
				get
				{
					return this._VoucheredQty;
				}
				set
				{
					this._VoucheredQty = value;
				}
			}
			#endregion
			#region CuryVoucheredCost
			public abstract class curyVoucheredCost : PX.Data.IBqlField
			{
			}
			protected Decimal? _CuryVoucheredCost;
			[PXDBCurrency(typeof(POOrderRS.curyInfoID), typeof(POOrderRS.voucheredCost), BqlField = typeof(POLine.curyVoucheredCost))]
			[PXUIField(DisplayName = "Billed Amt", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? CuryVoucheredCost
			{
				get
				{
					return this._CuryVoucheredCost;
				}
				set
				{
					this._CuryVoucheredCost = value;
				}
			}
			#endregion
			#region VoucheredCost
			public abstract class voucheredCost : PX.Data.IBqlField
			{
			}
			protected Decimal? _VoucheredCost;
			[PXDBDecimal(6, BqlField = typeof(POLine.voucheredCost))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? VoucheredCost
			{
				get
				{
					return this._VoucheredCost;
				}
				set
				{
					this._VoucheredCost = value;
				}
			}
			#endregion
			#region Hold
			public new abstract class hold : PX.Data.IBqlField
			{
			}
			#endregion
			#region Cancelled
			public new abstract class cancelled : PX.Data.IBqlField
			{
			}
			#endregion
			#region Receipt
			public new abstract class receipt : PX.Data.IBqlField
			{
			}
			#endregion
			#region CuryLeftToReceiveCost
			public abstract class curyLeftToBillCost : PX.Data.IBqlField
			{
			}

			[PXDecimal(6)]
			[PXUIField(DisplayName = "Unbilled Amt.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? CuryLeftToBillCost
			{
				[PXDependsOnFields(typeof(curyExtCost),typeof(curyVoucheredCost))]
				get
				{
					return (this.CuryExtCost - this.CuryVoucheredCost);
				}
			}
			#endregion
			#region LeftToBillQty
			public abstract class leftToReceiveQty : PX.Data.IBqlField
			{
			}
			[PXQuantity()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Unbilled Qty.", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Decimal? LeftToBillQty
			{
				[PXDependsOnFields(typeof(lineQty),typeof(voucheredQty))]
				get
				{
					return (this.LineQty - this.VoucheredQty);
				}
			}
			#endregion
			#region CuryInfoID
			public new abstract class curyInfoID : PX.Data.IBqlField
			{
			}
			[PXDBLong()]
			public override Int64? CuryInfoID
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

		}
		#endregion
		#endregion

		#region APAdjust
        [Serializable]        
		public partial class APAdjust : PX.Objects.AP.APAdjust
		{
			#region VendorID
			public new abstract class vendorID : PX.Data.IBqlField
			{
			}
			[PXDBInt]
			[PXDBDefault(typeof(AP.APInvoice.vendorID))]
			[PXUIField(DisplayName = "Vendor ID", Visibility = PXUIVisibility.Visible, Visible = false)]
			public override Int32? VendorID
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
			#region AdjgDocType
			public new abstract class adjgDocType : PX.Data.IBqlField
			{
			}
			[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
			[APPaymentType.List()]
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
			[APPaymentType.AdjgRefNbr(typeof(Search<APPayment.refNbr, Where<APPayment.docType, Equal<Optional<APAdjust.adjgDocType>>>>), Filterable = true)]
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
			[CurrencyInfo(typeof(APInvoice.curyInfoID), ModuleCode = "AP", CuryIDField = "AdjdCuryID")]
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
			[PXDBDefault(typeof(AP.APInvoice.docType))]
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
			[PXDBDefault(typeof(AP.APInvoice.refNbr))]
			[PXParent(typeof(Select<AP.APInvoice, Where<AP.APInvoice.docType, Equal<Current<APAdjust.adjdDocType>>, And<AP.APInvoice.refNbr, Equal<Current<APAdjust.adjdRefNbr>>>>>))]
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
			[Branch(typeof(AP.APInvoice.branchID))]
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
			#region StubNbr
			public new abstract class stubNbr : PX.Data.IBqlField
			{
			}
			[PXDBString(15, IsUnicode = true)]
			public override String StubNbr
			{
				get
				{
					return this._StubNbr;
				}
				set
				{
					this._StubNbr = value;
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
			[PXDBDefault(typeof(APInvoice.curyInfoID))]
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
			[CurrencyInfo(ModuleCode = "AP", CuryIDField = "AdjgCuryID")]
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
			[PXDBDefault(typeof(AP.APInvoice.docDate))]
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
			[PXDBDefault(typeof(AP.APInvoice.finPeriodID))]
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
			[PXDBDefault(typeof(APInvoice.tranPeriodID))]
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
			[PXDBDefault(typeof(AP.APInvoice.docDate))]
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
			[FinPeriodID(typeof(APAdjust.adjdDocDate))]
			[PXDBDefault(typeof(AP.APInvoice.finPeriodID))]
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
			[FinPeriodID(typeof(APAdjust.adjdDocDate))]
			[PXDBDefault(typeof(APInvoice.tranPeriodID))]
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
			[PXDBCurrency(typeof(APAdjust.adjdCuryInfoID), typeof(APAdjust.adjDiscAmt))]
			[PXDefault(TypeCode.Decimal, "0.0")]
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
			[PXDBCurrency(typeof(APAdjust.adjdCuryInfoID), typeof(APAdjust.adjAmt))]
			[PXUIField(DisplayName = "Amount Paid", Visibility = PXUIVisibility.Visible)]
			[PXDefault(TypeCode.Decimal, "0.0")]
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
			#region CuryAdjdWhTaxAmt
			public new abstract class curyAdjdWhTaxAmt : PX.Data.IBqlField
			{
			}
			[PXDBCurrency(typeof(APAdjust.adjdCuryInfoID), typeof(APAdjust.adjWhTaxAmt))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public override Decimal? CuryAdjdWhTaxAmt
			{
				get
				{
					return this._CuryAdjdWhTaxAmt;
				}
				set
				{
					this._CuryAdjdWhTaxAmt = value;
				}
			}
			#endregion
			#region AdjAmt
			public new abstract class adjAmt : PX.Data.IBqlField
			{
			}
			[PXDBDecimal(4)]
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
			#region AdjWhTaxAmt
			public new abstract class adjWhTaxAmt : PX.Data.IBqlField
			{
			}
			[PXDBDecimal(4)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public override Decimal? AdjWhTaxAmt
			{
				get
				{
					return this._AdjWhTaxAmt;
				}
				set
				{
					this._AdjWhTaxAmt = value;
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
			#region CuryAdjgWhTaxAmt
			public new abstract class curyAdjgWhTaxAmt : PX.Data.IBqlField
			{
			}
			[PXDBDecimal(4)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public override Decimal? CuryAdjgWhTaxAmt
			{
				get
				{
					return this._CuryAdjgWhTaxAmt;
				}
				set
				{
					this._CuryAdjgWhTaxAmt = value;
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
			#region AdjdAPAcct
			public new abstract class adjdAPAcct : PX.Data.IBqlField
			{
			}
			[Account()]
			[PXDBDefault(typeof(APInvoice.aPAccountID))]
			public override Int32? AdjdAPAcct
			{
				get
				{
					return this._AdjdAPAcct;
				}
				set
				{
					this._AdjdAPAcct = value;
				}
			}
			#endregion
			#region AdjdAPSub
			public new abstract class adjdAPSub : PX.Data.IBqlField
			{
			}
			[SubAccount()]
			[PXDBDefault(typeof(APInvoice.aPSubID))]
			public override Int32? AdjdAPSub
			{
				get
				{
					return this._AdjdAPSub;
				}
				set
				{
					this._AdjdAPSub = value;
				}
			}
			#endregion
			#region AdjdWhTaxAcctID
			public new abstract class adjdWhTaxAcctID : PX.Data.IBqlField
			{
			}
			[Account()]
			[PXDefault(typeof(Search2<APTaxTran.accountID, InnerJoin<Tax, On<Tax.taxID, Equal<APTaxTran.taxID>>>, Where<APTaxTran.tranType, Equal<Current<APAdjust.adjdDocType>>, And<APTaxTran.refNbr, Equal<Current<APAdjust.adjdRefNbr>>, And<Tax.taxType, Equal<CSTaxType.withholding>>>>, OrderBy<Asc<APTaxTran.taxID>>>), PersistingCheck = PXPersistingCheck.Nothing)]
			public override Int32? AdjdWhTaxAcctID
			{
				get
				{
					return this._AdjdWhTaxAcctID;
				}
				set
				{
					this._AdjdWhTaxAcctID = value;
				}
			}
			#endregion
			#region AdjdWhTaxSubID
			public new abstract class adjdWhTaxSubID : PX.Data.IBqlField
			{
			}
			[SubAccount()]
			[PXDefault(typeof(Search2<APTaxTran.subID, InnerJoin<Tax, On<Tax.taxID, Equal<APTaxTran.taxID>>>, Where<APTaxTran.tranType, Equal<Current<APAdjust.adjdDocType>>, And<APTaxTran.refNbr, Equal<Current<APAdjust.adjdRefNbr>>, And<Tax.taxType, Equal<CSTaxType.withholding>>>>, OrderBy<Asc<APTaxTran.taxID>>>), PersistingCheck = PXPersistingCheck.Nothing)]
			public override Int32? AdjdWhTaxSubID
			{
				get
				{
					return this._AdjdWhTaxSubID;
				}
				set
				{
					this._AdjdWhTaxSubID = value;
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
			[PXCurrency(typeof(APAdjust.adjdCuryInfoID), typeof(APAdjust.docBal), BaseCalc = false)]
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
			#endregion
			#region CuryDiscBal
			public new abstract class curyDiscBal : PX.Data.IBqlField
			{
			}
			[PXCurrency(typeof(APAdjust.adjdCuryInfoID), typeof(APAdjust.discBal), BaseCalc = false)]
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
			#endregion
			#region CuryWhTaxBal
			public new abstract class curyWhTaxBal : PX.Data.IBqlField
			{
			}
			[PXCurrency(typeof(APAdjust.adjdCuryInfoID), typeof(APAdjust.whTaxBal), BaseCalc = false)]
			[PXUIField(DisplayName = "With. Tax Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public override Decimal? CuryWhTaxBal
			{
				get
				{
					return this._CuryWhTaxBal;
				}
				set
				{
					this._CuryWhTaxBal = value;
				}
			}
			#endregion
			#region WhTaxBal
			public new abstract class whTaxBal : PX.Data.IBqlField
			{
			}
			[PXDecimal(4)]
			public override Decimal? WhTaxBal
			{
				get
				{
					return this._WhTaxBal;
				}
				set
				{
					this._WhTaxBal = value;
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
					return (this._AdjgDocType == APDocType.VoidCheck);
				}
				set
				{
					if ((bool)value)
					{
						this._AdjgDocType = APDocType.VoidCheck;
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
					return (APPaymentType.DrCr(this._AdjgDocType) == "D");
				}
				set
				{
				}
			}
			#endregion
		}
		#endregion

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (string.Compare(viewName, "Transactions", true) == 0)
			{
				if (values.Contains("tranType")) values["tranType"] = Document.Current.DocType;
				else values.Add("tranType", Document.Current.DocType);
				if (values.Contains("refNbr")) values["refNbr"] = Document.Current.RefNbr;
				else values.Add("refNbr", Document.Current.RefNbr);
			}
			return true;
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

		public virtual APInvoice CalculateAvalaraTax(APInvoice invoice)
		{
			TaxSvc service = new TaxSvc();
			AvalaraMaint.SetupService(this, service);

			AvaAddress.AddressSvc addressService = new AvaAddress.AddressSvc();
			AvalaraMaint.SetupService(this, addressService);

			GetTaxRequest getRequest = BuildGetTaxRequest(invoice);

			if (getRequest.Lines.Count == 0)
			{
                Document.SetValueExt<APInvoice.isTaxValid>(invoice, true);
				if (invoice.IsTaxSaved == true)
					Document.SetValueExt<APInvoice.isTaxSaved>(invoice, false);

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
					catch (Exception)
					{
						throw;
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

		protected virtual GetTaxRequest BuildGetTaxRequest(APInvoice invoice)
		{
			if (invoice == null)
				throw new PXArgumentException(ErrorMessages.ArgumentNullException);

			Vendor vend = (Vendor)vendor.View.SelectSingleBound(new object[] { invoice });
			
			GetTaxRequest request = new GetTaxRequest();
			request.CompanyCode = AvalaraMaint.CompanyCodeFromBranch(this, invoice.BranchID);
			request.CurrencyCode = invoice.CuryID;
			request.CustomerCode = vend.AcctCD;
			IAddressBase fromAddress = GetFromAddress(invoice);
			IAddressBase toAddress = GetToAddress(invoice);

			if (fromAddress == null)
				throw new PXException("Failed to get the 'FROM' Address for the document");

			if (toAddress == null)
				throw new PXException("Failed to get the 'TO' Address for the document");

			request.OriginAddress = AvalaraMaint.FromAddress(fromAddress);
			request.DestinationAddress = AvalaraMaint.FromAddress(toAddress);
			request.DetailLevel = DetailLevel.Summary;
			request.DocCode = string.Format("AP.{0}.{1}", invoice.DocType, invoice.RefNbr);
			request.DocDate = invoice.DocDate.GetValueOrDefault();

			Location branchLoc = GetBranchLocation(invoice);

			if (branchLoc != null)
			{
				request.CustomerUsageType = branchLoc.CAvalaraCustomerUsageType;
				request.ExemptionNo = branchLoc.CAvalaraExemptionNumber;
			}

			request.DocType = DocumentType.PurchaseInvoice;
			int mult = 1;
			switch (invoice.DocType)
			{
				case APDocType.Invoice:
				case APDocType.CreditAdj:
					request.DocType = DocumentType.PurchaseInvoice;
					break;
				case APDocType.DebitAdj:
					if (invoice.OrigDocDate != null)
					{
						request.TaxOverride.Reason = "Debit Adjustment";
						request.TaxOverride.TaxDate = invoice.OrigDocDate.Value;
						request.TaxOverride.TaxOverrideType = TaxOverrideType.TaxDate;
						mult = -1;
					}
					request.DocType = DocumentType.ReturnInvoice;
					break;

				default:
					throw new PXException("DocType is not supported/implemented.");
			}

			PXSelectBase<APTran> select = new PXSelectJoin<APTran,
				LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<APTran.inventoryID>>,
					LeftJoin<Account, On<Account.accountID, Equal<APTran.accountID>>>>,
				Where<APTran.tranType, Equal<Current<APInvoice.docType>>,
					And<APTran.refNbr, Equal<Current<APInvoice.refNbr>>>>,
				OrderBy<Asc<APTran.tranType, Asc<APTran.refNbr, Asc<APTran.lineNbr>>>>>(this);

			request.Discount = GetDocDiscount().GetValueOrDefault();
			foreach (PXResult<APTran, InventoryItem, Account> res in select.View.SelectMultiBound(new object[] { invoice }))
			{
				APTran tran = (APTran)res;
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

				request.Lines.Add(line);
			}

			return request;
		}

		protected bool skipAvalaraCallOnSave = false;
		protected virtual void ApplyAvalaraTax(APInvoice invoice, GetTaxResult result)
		{
			TaxZone taxZone = (TaxZone)taxzone.View.SelectSingleBound(new object[] { invoice });
			AP.Vendor vendor = PXSelect<AP.Vendor, Where<AP.Vendor.bAccountID, Equal<Required<AP.Vendor.bAccountID>>>>.Select(this, taxZone.TaxVendorID);

			if (vendor == null)
				throw new PXException("Tax Vendor is required but not found for the External TaxZone.");

			Dictionary<string, APTaxTran> existingRows = new Dictionary<string, APTaxTran>();
			foreach (PXResult<APTaxTran, Tax> res in Taxes.View.SelectMultiBound(new object[] { invoice }))
			{
				APTaxTran taxTran = (APTaxTran)res;
				existingRows.Add(taxTran.TaxID.Trim().ToUpperInvariant(), taxTran);
			}

			this.Views.Caches.Add(typeof(Tax));

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
					tx.TaxCalcLevel = avalaraSetup.Current.IsInclusiveTax == true ? CSTaxCalcLevel.Inclusive : CSTaxCalcLevel.CalcOnItemAmt;
					tx.TaxApplyTermsDisc = CSTaxTermsDiscount.ToTaxableAmount;
					tx.SalesTaxAcctID = vendor.SalesTaxAcctID;
					tx.SalesTaxSubID = vendor.SalesTaxSubID;
					tx.ExpenseAccountID = vendor.TaxExpenseAcctID;
					tx.ExpenseSubID = vendor.TaxExpenseSubID;
					tx.TaxVendorID = taxZone.TaxVendorID;

					this.Caches[typeof(Tax)].Insert(tx);
				}

				APTaxTran existing = null;
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
					APTaxTran tax = new APTaxTran();
					tax.Module = BatchModule.AP;
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

			foreach (APTaxTran taxTran in existingRows.Values)
			{
				Taxes.Delete(taxTran);
			}

			bool requireControlTotal = APSetup.Current.RequireControlTotal == true;
			if (invoice.Hold != true)
				APSetup.Current.RequireControlTotal = false;

			try
			{
				invoice.CuryTaxTotal = Math.Abs(result.TotalTax);
				Document.Cache.SetValueExt<APInvoice.isTaxSaved>(invoice, true);
			}
			finally
			{
				APSetup.Current.RequireControlTotal = requireControlTotal;
			}
		}

		protected virtual void CancelTax(APInvoice invoice, CancelCode code)
		{
			CancelTaxRequest request = new CancelTaxRequest();
			request.CompanyCode = AvalaraMaint.CompanyCodeFromBranch(this, invoice.BranchID);
			request.CancelCode = code;
			request.DocCode = string.Format("AP.{0}.{1}", invoice.DocType, invoice.RefNbr);
			request.DocType = DocumentType.PurchaseInvoice;

			TaxSvc service = new TaxSvc();
			AvalaraMaint.SetupService(this, service);
			CancelTaxResult result = service.CancelTax(request);

			bool raiseError = false;
			if (result.ResultCode != SeverityLevel.Success)
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

			if (raiseError)
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

		protected virtual IAddressBase GetToAddress(APInvoice invoice)
		{
			PXSelectBase<Branch> select = new PXSelectJoin
				<Branch, InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>,
					InnerJoin<Address, On<Address.addressID, Equal<BAccountR.defAddressID>>>>,
					Where<Branch.branchID, Equal<Required<Branch.branchID>>>>(this);

			foreach (PXResult<Branch, BAccountR, Address> res in select.Select(invoice.BranchID))
				return (Address)res;

			return null;
		}

		protected virtual Location GetBranchLocation(APInvoice invoice)
		{
			PXSelectBase<Branch> select = new PXSelectJoin
				<Branch, InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>,
					InnerJoin<Location, On<Location.locationID, Equal<BAccountR.defLocationID>>>>,
					Where<Branch.branchID, Equal<Required<Branch.branchID>>>>(this);

			foreach (PXResult<Branch, BAccountR, Location> res in select.Select(invoice.BranchID))
				return (Location)res;

			return null;
		}

		protected virtual IAddressBase GetFromAddress(APInvoice invoice)
		{
			Address vendorAddress = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(this, vendor.Current.DefAddressID);

			return vendorAddress;
		}

		protected virtual decimal? GetDocDiscount()
		{
			return null;
		}

		#endregion
	}
}
namespace PX.Objects.PO
{
	//This class is used for Update of vouchered (billed) amounts in POReceiptLine
	[PXProjection(typeof(Select<POReceiptLine>), Persistent = true)]
    [Serializable]
	public partial class POReceiptLineR : IBqlTable
	{
		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.IBqlField
		{
		}
		protected String _ReceiptNbr;

		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(POReceiptLine.receiptNbr))]
		[PXDefault()]
		[PXParent(typeof(Select<POReceipt, Where<POReceipt.receiptNbr, Equal<Current<POReceiptLineR1.receiptNbr>>>>))]
		[PXUIField(DisplayName = "Receipt Nbr.", Visibility = PXUIVisibility.Invisible, Visible = false)]
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
		#region LineNbr
		public abstract class lineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(POReceiptLine.lineNbr))]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[Inventory(Filterable = true, BqlField = typeof(POReceiptLine.inventoryID))]
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
		#region InvtMult
		public abstract class invtMult : PX.Data.IBqlField
		{
		}
		protected Int16? _InvtMult;
		[PXDBShort(BqlField = typeof(POReceiptLine.invtMult))]
		[PXDefault()]
		public virtual Int16? InvtMult
		{
			get
			{
				return this._InvtMult;
			}
			set
			{
				this._InvtMult = value;
			}
		}
		#endregion	

		#region VoucheredQty
		public abstract class voucheredQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _VoucheredQty;

		[PXDBDecimal(6, BqlField = typeof(POReceiptLine.voucheredQty))]
		public virtual Decimal? VoucheredQty
		{
			get
			{
				return this._VoucheredQty;
			}
			set
			{
				this._VoucheredQty = value;
			}
		}
		#endregion
		#region BaseVoucheredQty
		public abstract class baseVoucheredQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _BaseVoucheredQty;

		[PXDBDecimal(6, BqlField = typeof(POReceiptLine.baseVoucheredQty))]
		public virtual Decimal? BaseVoucheredQty
		{
			get
			{
				return this._BaseVoucheredQty;
			}
			set
			{
				this._BaseVoucheredQty = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		protected Int64? _CuryInfoID;
		[PXDBLong(BqlField = typeof(POReceiptLine.curyInfoID))]
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
		#region CuryVoucheredCost
		public abstract class curyVoucheredCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryVoucheredCost;

		[PXDBDecimal(6, BqlField = typeof(POReceiptLine.curyVoucheredCost))]
		public virtual Decimal? CuryVoucheredCost
		{
			get
			{
				return this._CuryVoucheredCost;
			}
			set
			{
				this._CuryVoucheredCost = value;
			}
		}
		#endregion
		#region VoucheredCost
		public abstract class voucheredCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _VoucheredCost;
		[PXDBDecimal(6, BqlField = typeof(POReceiptLine.voucheredCost))]		
		public virtual Decimal? VoucheredCost
		{
			get
			{
				return this._VoucheredCost;
			}
			set
			{
				this._VoucheredCost = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.IBqlField
		{
		}
		protected String _UOM;
		[INUnit(typeof(POReceiptLineR.inventoryID), DisplayName = "UOM", BqlField = typeof(POReceiptLine.uOM))]
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

		#region SignedVoucheredQty
		public abstract class signedVoucheredQty : PX.Data.IBqlField
		{
		}
		
		
		[PXQuantity(typeof(POReceiptLineR.uOM), typeof(POReceiptLineR.signedBaseVoucheredQty), HandleEmptyKey = true)]
		public virtual Decimal? SignedVoucheredQty
		{
			[PXDependsOnFields(typeof(voucheredQty))]
			get
			{
				return this._VoucheredQty * this.Sign;
			}
			set
			{
				this._VoucheredQty = value * this.Sign;
			}
		}
		#endregion
		#region SignedBaseVoucheredQty
		public abstract class signedBaseVoucheredQty : PX.Data.IBqlField
		{
		}
	

		[PXDecimal(6)]
		public virtual Decimal? SignedBaseVoucheredQty
		{
			[PXDependsOnFields(typeof(baseVoucheredQty))]
			get
			{
				return this._BaseVoucheredQty * this.Sign;
			}
			set
			{
				this._BaseVoucheredQty = value * this.Sign;
			}
		}
		#endregion
		#region SignedCuryVoucheredCost
		public abstract class signedCuryVoucheredCost : PX.Data.IBqlField
		{
		}

		[PXCurrency(typeof(POReceiptLineR.curyInfoID), typeof(POReceiptLineR.signedVoucheredCost))]
		public virtual Decimal? SignedCuryVoucheredCost
		{
			[PXDependsOnFields(typeof(curyVoucheredCost))]
			get
			{
				return this._CuryVoucheredCost * this.Sign;
			}
			set
			{
				this._CuryVoucheredCost = value * this.Sign;
			}
		}
		#endregion
		#region SignedVoucheredCost
		public abstract class signedVoucheredCost : PX.Data.IBqlField
		{
		}
		
		[PXDecimal(6)]
		public virtual Decimal? SignedVoucheredCost
		{
			[PXDependsOnFields(typeof(voucheredCost))]
			get
			{
				return this._VoucheredCost * this.Sign;
			}
			set
			{
				this._VoucheredCost = value * this.Sign;
			}
		}
		#endregion

		protected decimal Sign
		{
			get { return this._InvtMult < 0 ? Decimal.MinusOne : Decimal.One; }
		}
	}

	//This class is used for Update of vouchered (billed amounts) in POLine
	[PXProjection(typeof(Select<POLine>), Persistent = true)]
    [Serializable]
	public partial class POLineAP : IBqlTable
	{
		#region OrderType
		public abstract class orderType : PX.Data.IBqlField
		{
		}
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(POLine.orderType))]
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
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(POLine.orderNbr))]
		[PXDefault()]
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
		[PXDBInt(IsKey = true, BqlField = typeof(POLine.lineNbr))]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[Inventory(Filterable = true, BqlField = typeof(POLine.inventoryID))]
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
		#region VoucheredQty
		public abstract class voucheredQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _VoucheredQty;

		[PXDBQuantity(typeof(POLineAP.uOM), typeof(POLineAP.baseVoucheredQty), HandleEmptyKey = true, BqlField = typeof(POLine.voucheredQty))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Vouchered Qty.", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? VoucheredQty
		{
			get
			{
				return this._VoucheredQty;
			}
			set
			{
				this._VoucheredQty = value;
			}
		}
		#endregion
		#region BaseVoucheredQty
		public abstract class baseVoucheredQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _BaseVoucheredQty;

		[PXDBDecimal(6, BqlField = typeof(POLine.baseVoucheredQty))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Base Vouchered Qty.", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? BaseVoucheredQty
		{
			get
			{
				return this._BaseVoucheredQty;
			}
			set
			{
				this._BaseVoucheredQty = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		protected Int64? _CuryInfoID;
		[PXDBLong(BqlField = typeof(POLine.curyInfoID))]
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
		#region CuryVoucheredCost
		public abstract class curyVoucheredCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryVoucheredCost;

		[PXDBCurrency(typeof(POLineAP.curyInfoID), typeof(POLineAP.voucheredCost), BqlField = typeof(POLine.curyVoucheredCost))]
		[PXUIField(DisplayName = "Vouchered Cost", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryVoucheredCost
		{
			get
			{
				return this._CuryVoucheredCost;
			}
			set
			{
				this._CuryVoucheredCost = value;
			}
		}
		#endregion
		#region VoucheredCost
		public abstract class voucheredCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _VoucheredCost;
		[PXDBDecimal(6, BqlField = typeof(POLine.voucheredCost))]
		[PXUIField(DisplayName = "Vouchered Cost")]
		public virtual Decimal? VoucheredCost
		{
			get
			{
				return this._VoucheredCost;
			}
			set
			{
				this._VoucheredCost = value;
			}
		}
		#endregion
		#region ReceivedQty
		public abstract class receivedQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _ReceivedQty;
		[PXDBQuantity(typeof(POLineAP.uOM), typeof(POLineAP.baseReceivedQty), HandleEmptyKey = true, BqlField = typeof(POLine.receivedQty))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Received Qty.", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? ReceivedQty
		{
			get
			{
				return this._ReceivedQty;
			}
			set
			{
				this._ReceivedQty = value;
			}
		}
		#endregion
		#region BaseReceivedQty
		public abstract class baseReceivedQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _BaseReceivedQty;

		[PXDBDecimal(6, BqlField = typeof(POLine.baseReceivedQty))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Base Received Qty.", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? BaseReceivedQty
		{
			get
			{
				return this._BaseReceivedQty;
			}
			set
			{
				this._BaseReceivedQty = value;
			}
		}
		#endregion
		#region CuryReceivedCost
		public abstract class curyReceivedCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryReceivedCost;

		[PXDBCurrency(typeof(POLineAP.curyInfoID), typeof(POLineAP.receivedCost), BqlField = typeof(POLine.curyReceivedCost))]
		[PXUIField(DisplayName = "Received Cost", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryReceivedCost
		{
			get
			{
				return this._CuryReceivedCost;
			}
			set
			{
				this._CuryReceivedCost = value;
			}
		}
		#endregion
		#region ReceivedCost
		public abstract class receivedCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _ReceivedCost;
		[PXDBDecimal(6, BqlField = typeof(POLine.receivedCost))]
		[PXUIField(DisplayName = "Received Cost")]
		public virtual Decimal? ReceivedCost
		{
			get
			{
				return this._ReceivedCost;
			}
			set
			{
				this._ReceivedCost = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.IBqlField
		{
		}
		protected String _UOM;
		[INUnit(typeof(POLineAP.inventoryID), DisplayName = "UOM", BqlField = typeof(POLine.uOM))]
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
		#region tstamp
		public abstract class Tstamp : PX.Data.IBqlField
		{
		}
		protected Byte[] _tstamp;
		[PXDBTimestamp(BqlField = typeof(POLine.Tstamp))]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
	}

	//This class is used for Update of unbilled amounts in POReceiptLine ( during Release AP Document process)
	[PXProjection(typeof(Select<POReceiptLine>), Persistent = true)]
    [Serializable]
	public partial class POReceiptLineR1 : IBqlTable
	{
		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.IBqlField
		{
		}
		protected String _ReceiptNbr;

		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(POReceiptLine.receiptNbr))]
		[PXDefault()]
		[PXParent(typeof(Select<POReceipt, Where<POReceipt.receiptNbr, Equal<Current<POReceiptLineR1.receiptNbr>>>>))]
		[PXUIField(DisplayName = "Receipt Nbr.", Visibility = PXUIVisibility.Invisible, Visible = false)]
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

		#region LineNbr
		public abstract class lineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(POReceiptLine.lineNbr))]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
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
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		protected Int64? _CuryInfoID;
		[PXDBLong(BqlField = typeof(POReceiptLine.curyInfoID))]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[Inventory(Filterable = true, BqlField = typeof(POReceiptLine.inventoryID))]
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
		[SubItem(BqlField = typeof(POReceiptLine.subItemID))]
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
		#region UOM
		public abstract class uOM : PX.Data.IBqlField
		{
		}
		protected String _UOM;
		[INUnit(typeof(POReceiptLineR1.inventoryID), DisplayName = "UOM", BqlField = typeof(POReceiptLine.uOM))]
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
		#region ReceiptQty
		public abstract class receiptQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _ReceiptQty;

		[PXDBQuantity(MinValue = 0, BqlField = typeof(POReceiptLine.receiptQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity", Visibility = PXUIVisibility.Visible)]
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

		public virtual Decimal? Qty
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
		#region CuryExtCost
		public abstract class curyExtCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryExtCost;
		[PXDBDecimal(4, BqlField = typeof(POReceiptLine.curyExtCost))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryExtCost
		{
			get
			{
				return this._CuryExtCost;
			}
			set
			{
				this._CuryExtCost = value;
			}
		}
		#endregion
		#region InvtMult
		public abstract class invtMult : PX.Data.IBqlField
		{
		}
		protected Int16? _InvtMult;
		[PXDBShort(BqlField = typeof(POReceiptLine.invtMult))]		
		public virtual Int16? InvtMult
		{
			get
			{
				return this._InvtMult;
			}
			set
			{
				this._InvtMult = value;
			}
		}
		#endregion
		#region UnbilledQty
		public abstract class unbilledQty : PX.Data.IBqlField
		{
		}
		protected Decimal? _UnbilledQty;
		[PXDBQuantity(typeof(POReceiptLineR1.uOM), typeof(POReceiptLineR1.baseUnbilledQty), HandleEmptyKey = true, BqlField = typeof(POReceiptLine.unbilledQty))]
		[PXFormula(null, typeof(SumCalc<POReceipt.unbilledQty>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Qty.", Enabled = false)]
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
		[PXDBDecimal(6, BqlField = typeof(POReceiptLine.baseUnbilledQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		#region CuryUnbilledAmt
		public abstract class curyUnbilledAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryUnbilledAmt;
		[PXDBCurrency(typeof(POReceiptLineR1.curyInfoID), typeof(POReceiptLineR1.unbilledAmt), BqlField = typeof(POReceiptLine.curyUnbilledAmt))]
		[PXFormula(typeof(Mult<POReceiptLineR1.unbilledQty, POReceiptLineR1.curyUnitCost>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		[PXDBDecimal(4, BqlField = typeof(POReceiptLine.unbilledAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		#region CuryUnitCost
		public abstract class curyUnitCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryUnitCost;

		[PXDBDecimal(4, BqlField = typeof(POReceiptLine.curyUnitCost))]
		[PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryUnitCost
		{
			get
			{
				return this._CuryUnitCost;
			}
			set
			{
				this._CuryUnitCost = value;
			}
		}
		#endregion
		#region BillPPVAmt
		public abstract class billPPVAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _BillPPVAmt;
		[PXDBBaseCury(BqlField = typeof(POReceiptLine.billPPVAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BillPPVAmt
		{
			get
			{
				return this._BillPPVAmt;
			}
			set
			{
				this._BillPPVAmt = value;
			}
		}
		#endregion

		#region UnitCost
		public abstract class unitCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _UnitCost;

		[PXDBDecimal(4, BqlField = typeof(POReceiptLine.unitCost))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnitCost
		{
			get
			{
				return this._UnitCost;
			}
			set
			{
				this._UnitCost = value;
			}
		}
		#endregion
		#region GroupDiscountRate
		public abstract class groupDiscountRate : PX.Data.IBqlField
		{
		}
		protected Decimal? _GroupDiscountRate;
		[PXDBDecimal(6, BqlField = typeof(POReceiptLine.groupDiscountRate))]
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
		[PXDBString(10, IsUnicode = true, BqlField = typeof(POReceiptLine.taxCategoryID))]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
		[POReceiptUnbilledTaxR(typeof(POReceipt), typeof(POReceiptTax), typeof(POReceiptTaxTran))]
		//[POUnbilledTaxR(typeof(POReceipt), typeof(POTax), typeof(POTaxTran))]
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
		#region LineType
		public abstract class lineType : PX.Data.IBqlField
		{
		}
		protected String _LineType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(POReceiptLine.lineType))]
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

		public decimal APSign
		{
			[PXDependsOnFields(typeof(invtMult))]
			get { return this._InvtMult < 0 ? Decimal.MinusOne : Decimal.One; }
		}
	}

	public interface IAPTranSource
	{
		Int32? BranchID { get; }
		Int32? ExpenseAcctID { get; }
		Int32? ExpenseSubID { get; }
		Int32? POAccrualAcctID { get; }
		Int32? POAccrualSubID { get; }
		String LineType { get; }
		Int32? InventoryID { get; }
		String UOM { get; }
		decimal? BillQty { get; }
		decimal? CuryUnitCost { get; }
		decimal? CuryLineAmt { get; }
		String TaxCategoryID { get; }
		String TranDesc { get; }
		String TaxID { get; }
		int? ProjectID { get; }
		int? TaskID { get; }
		//String ReceiptNbr { get; }
		//Int16? LineNbr { get; }


		bool CompareReferenceKey(AP.APTran aTran);
		void SetReferenceKeyTo(AP.APTran aTran);

	}
}
  
