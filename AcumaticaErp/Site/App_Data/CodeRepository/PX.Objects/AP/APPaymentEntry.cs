using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CR;

namespace PX.Objects.AP
{
	[Serializable]
	public class AdjustedNotFoundException : PXException
	{
		public AdjustedNotFoundException()
			: base(ErrorMessages.ElementDoesntExist, Messages.APInvoice)
		{ 
		}
		public AdjustedNotFoundException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			PXReflectionSerializer.RestoreObjectProps(this, info);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			PXReflectionSerializer.GetObjectData(this, info);
			base.GetObjectData(info, context);
		}

	}

	public class APPaymentEntry : APDataEntryGraph<APPaymentEntry, APPayment>
    {
        #region Cache Attached Events
        #region PTInstTran
        #region PTInstanceID
        
        [PXDBInt()]
        [PXDBDefault(typeof(APPayment.pTInstanceID))]
        protected virtual void PTInstTran_PTInstanceID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region OrigModule
        [PXDBString(2, IsFixed = true)]
        [PXDefault(BatchModule.AP)]
        [PXUIField(DisplayName = "Module")]        
        protected virtual void PTInstTran_OrigModule_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region OrigTranType        
        [PXDBString(3)]
        [PXDBDefault(typeof(APPayment.docType))]
        [PXUIField(DisplayName = "Tran. Type")]        
        protected virtual void PTInstTran_OrigTranType_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region OrigRefNbr    

        [PXDBString(15, IsUnicode = true)]
        [PXDBDefault(typeof(APPayment.refNbr))]
        [PXUIField(DisplayName = "Ref. Nbr.")]
        [PXParent(typeof(Select<APPayment,
                            Where<APPayment.docType, Equal<Current<PTInstTran.origTranType>>,
                            And<APPayment.refNbr, Equal<Current<PTInstTran.origRefNbr>>>>>))]        
        protected virtual void PTInstTran_OrigRefNbr_CacheAttached(PXCache sender)
        {
        }
        
        #endregion
        #region ExtRefNbr        
        [PXDBString(15, IsUnicode = true)]
        [PXDBDefault(typeof(APPayment.extRefNbr), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Ext. Ref. Nbr.")]        
        protected virtual void PTInstTran_ExtRefNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region CashAccountID
        
        [PXDBInt()]
        [PXDBDefault(typeof(APPayment.cashAccountID))]
        protected virtual void PTInstTran_CashAccountID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region TranDate
        
        [PXDBDate()]
        [PXDBDefault(typeof(APPayment.docDate))]
        [PXUIField(DisplayName = "Tran. Date")]        
        protected virtual void PTInstTran_TranDate_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region TranDesc        
        [PXDBString(60, IsUnicode = true)]
        [PXDBDefault(typeof(APPayment.docDesc), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Descr")]        
        protected virtual void PTInstTran_TranDesc_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region DrCr

        [PXDBString(1, IsFixed = true)]
        [PXDBDefault(typeof(APPayment.drCr))]        
        protected virtual void PTInstTran_DrCr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region CuryID

        [PXDBString(5, IsUnicode = true)]
        [PXDBDefault(typeof(APPayment.curyID))]
        [PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
        [PXUIField(DisplayName = "Currency ID")]        
        protected virtual void PTInstTran_CuryID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region CuryInfoID
        
        [PXDBLong]
        [PXDBDefault(typeof(APPayment.curyInfoID))]        
        protected virtual void PTInstTran_CuryInfoID_CacheAttached(PXCache sender)
        {
        }
        
        #endregion
        #region CuryTranAmt
        [PXDBDecimal(4)]
        [PXDBDefault(typeof(APPayment.curyOrigDocAmt))]
        protected virtual void PTInstTran_CuryTranAmt_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region TranAmt   
        [PXDBDecimal(4)]
        [PXDBDefault(typeof(APPayment.origDocAmt), DefaultForUpdate = true)]
        protected virtual void PTInstTran_TranAmt_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region Hold
        
        [PXDBBool()]
        [PXDBDefault(typeof(APPayment.hold))]
        [PXUIField(DisplayName = "Hold")]
        protected virtual void PTInstTran_Hold_CacheAttached(PXCache sender)
        {
        }

        #endregion
        #region VendorID

        [PXDBInt()]
        [PXDBDefault(typeof(APPayment.vendorID))]
        [PXUIField(DisplayName = "Vendor ID")]        
        protected virtual void PTInstTran_VendorID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region TranSource
        
        [PXDBString(2, IsFixed = true)]
        [PXDefault("AP")]
        [PXUIField(DisplayName = "Source")]
        protected virtual void PTInstTran_TranSource_CacheAttached(PXCache sender)
        {
        }
        #endregion
        
        #endregion
        #endregion
		public PXAction<APPayment> cancel;
		[PXCancelButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select)]
		protected new virtual IEnumerable Cancel(PXAdapter a)
		{
			string lastDocType = null;
			string lastRefNbr = null;
			if (this.Document.Current != null)
			{
				lastDocType = this.Document.Current.DocType;
				lastRefNbr = this.Document.Current.RefNbr;
			}
			PXResult<APPayment, Vendor> r = null;
			foreach (PXResult<APPayment, Vendor> e in (new PXCancel<APPayment>(this, "Cancel")).Press(a))
			{
				r = e;
			}
			if (Document.Cache.GetStatus((APPayment)r) == PXEntryStatus.Inserted)
			{
				if (lastRefNbr != ((APPayment)r).RefNbr)
				{
					if (((APPayment)r).DocType == APPaymentType.Check || ((APPayment)r).DocType == APPaymentType.Prepayment)
					{
						string docType = ((APPayment)r).DocType;
						string refNbr = ((APPayment)r).RefNbr;
						string searchDocType = docType == APPaymentType.Check ? APPaymentType.Prepayment : APPaymentType.Check;
						APPayment duplicatePayment = PXSelect<APPayment,
							Where<APPayment.docType, Equal<Required<APPayment.docType>>, And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>
							.Select(this, searchDocType, refNbr);
						APInvoice inv = null;
						if (searchDocType == APPaymentType.Prepayment)
						{
							inv = PXSelect<APInvoice, Where<APInvoice.docType, Equal<APInvoiceType.prepayment>, And<APInvoice.refNbr, Equal<Required<APPayment.refNbr>>>>>.Select(this, refNbr);
						}
						if (duplicatePayment != null && inv == null)
						{
							Document.Cache.RaiseExceptionHandling<APPayment.refNbr>((APPayment)r, refNbr,
								new PXSetPropertyException<APPayment.refNbr>(Messages.SameRefNbr, searchDocType == APPaymentType.Check ? Messages.Check : Messages.Prepayment, refNbr));
						}
					}

				}
			}

			yield return r;
		}

        public ToggleCurrency<APPayment> CurrencyView;

		[PXViewName(Messages.APPayment)]
        [PXCopyPasteHiddenFields(typeof(APPayment.extRefNbr))]
		public PXSelectJoin<APPayment,
			LeftJoin<Vendor, On<Vendor.bAccountID, Equal<APPayment.vendorID>>>,
			Where<APPayment.docType, Equal<Optional<APPayment.docType>>,
			And<Where<Vendor.bAccountID, IsNull,
			Or<Match<Vendor, Current<AccessInfo.userName>>>>>>> Document;
		public PXSelect<APPayment, Where<APPayment.docType, Equal<Current<APPayment.docType>>, And<APPayment.refNbr, Equal<Current<APPayment.refNbr>>>>> CurrentDocument;
		[PXViewName(Messages.APAdjust)]
        [PXCopyPasteHiddenView]
        public PXSelectJoin<APAdjust, LeftJoin<APInvoice, On<APInvoice.docType, Equal<APAdjust.adjdDocType>, And<APInvoice.refNbr, Equal<APAdjust.adjdRefNbr>>>>, Where<APAdjust.adjgDocType, Equal<Current<APPayment.docType>>, And<APAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>, And<APAdjust.adjNbr, Equal<Current<APPayment.lineCntr>>>>>> Adjustments;
		public PXSelect<APAdjust, Where<APAdjust.adjgDocType, Equal<Optional<APPayment.docType>>, And<APAdjust.adjgRefNbr, Equal<Optional<APPayment.refNbr>>, And<APAdjust.adjNbr, Equal<Optional<APPayment.lineCntr>>>>>> Adjustments_Raw;
		public PXSelectJoin<APAdjust, InnerJoin<APInvoice, On<APInvoice.docType, Equal<APAdjust.adjdDocType>, And<APInvoice.refNbr, Equal<APAdjust.adjdRefNbr>>>>, Where<APAdjust.adjgDocType, Equal<Current<APPayment.docType>>, And<APAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>, And<APAdjust.adjNbr, Equal<Current<APPayment.lineCntr>>>>>> Adjustments_Invoices;
		public PXSelectJoin<APAdjust, InnerJoin<APPayment, On<APPayment.docType, Equal<APAdjust.adjdDocType>, And<APPayment.refNbr, Equal<APAdjust.adjdRefNbr>>>, LeftJoin<APInvoice, On<APInvoice.docType, Equal<APAdjust.adjdDocType>, And<APInvoice.refNbr, Equal<APAdjust.adjdRefNbr>>>>>, Where<APAdjust.adjgDocType, Equal<Current<APPayment.docType>>, And<APAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>, And<APAdjust.adjNbr, Equal<Current<APPayment.lineCntr>>, And<APInvoice.refNbr, IsNull>>>>> Adjustments_Payments;
        [PXCopyPasteHiddenView]
        public PXSelectJoin<APAdjust, LeftJoin<APInvoice, On<APInvoice.docType, Equal<APAdjust.adjdDocType>, And<APInvoice.refNbr, Equal<APAdjust.adjdRefNbr>>>>, Where<APAdjust.adjgDocType, Equal<Current<APPayment.docType>>, And<APAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>, And<APAdjust.adjNbr, Less<Current<APPayment.lineCntr>>>>>> Adjustments_History;
		[PXViewName(Messages.APAdjustHistory)]
		public PXSelect<APAdjust> Adjustments_print;

        public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APPayment.curyInfoID>>>> currencyinfo;
		public PXSelect<APInvoice> dummy_APInvoice;
		public PXSelect<CATran> dummy_CATran;

		[PXViewName(Messages.APAddress)]
		public PXSelect<APAddress, Where<APAddress.addressID, Equal<Current<APPayment.remitAddressID>>>> Remittance_Address;
		[PXViewName(Messages.APContact)]
		public PXSelect<APContact, Where<APContact.contactID, Equal<Current<APPayment.remitContactID>>>> Remittance_Contact;

		//public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APPayment.curyInfoID>>>> CurrencyInfo_Document;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> CurrencyInfo_CuryInfoID;
		public PXSelectReadonly<APInvoice, Where<APInvoice.vendorID, Equal<Required<APInvoice.vendorID>>, And<APInvoice.docType, Equal<Required<APInvoice.docType>>, And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>> APInvoice_VendorID_DocType_RefNbr;
		public PXSelect<APPayment, Where<APPayment.vendorID, Equal<Required<APPayment.vendorID>>, And<APPayment.docType, Equal<Required<APPayment.docType>>, And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>> APPayment_VendorID_DocType_RefNbr;

        public PXSelect<APPaymentChargeTran,
                    Where<APPaymentChargeTran.docType, Equal<Current<APPayment.docType>>,
                    And<APPaymentChargeTran.refNbr, Equal<Current<APPayment.refNbr>>>>> PaymentCharges;

		[PXViewName(Messages.Vendor)]
		public PXSetup<Vendor, Where<Vendor.bAccountID, Equal<Optional<APPayment.vendorID>>>> vendor;
		public PXSetup<Location, Where<Location.bAccountID, Equal<Current<APPayment.vendorID>>, And<Location.locationID, Equal<Optional<APPayment.vendorLocationID>>>>> location;
		public PXSetup<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>> vendorclass;
		public PXSelect<PaymentMethodAccount, Where<PaymentMethodAccount.cashAccountID, Equal<Current<APPayment.cashAccountID>>>, OrderBy<Asc<PaymentMethodAccount.aPIsDefault>>> CashAcctDetail_AccountID;
		public PXSetup<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Optional<APPayment.paymentMethodID>>>> paymenttype;
		public PXSetup<CashAccount, Where<CashAccount.cashAccountID, Equal<Optional<APPayment.cashAccountID>>>> cashaccount;
		public PXSetup<PaymentMethodAccount, Where<PaymentMethodAccount.cashAccountID, Equal<Optional<APPayment.cashAccountID>>, And<PaymentMethodAccount.paymentMethodID, Equal<Current<APPayment.paymentMethodID>>>>> cashaccountdetail;
		public PXSelectReadonly<FinPeriod, Where<FinPeriod.finPeriodID, Equal<Current<APPayment.adjFinPeriodID>>>> finperiod;
		public PXSelect<PTInstTran, Where<PTInstTran.origTranType, Equal<Current<APPayment.docType>>,
											And<PTInstTran.origRefNbr, Equal<Current<APPayment.refNbr>>>>> ptInstanceTrans;
		public PXSetup<GLSetup> glsetup;

		protected bool _AutoPaymentApp = false;
		public bool AutoPaymentApp
		{
			get
			{
				return this._AutoPaymentApp;
			}
			set
			{
				this._AutoPaymentApp = value;
			}
		}

		public FinPeriod FINPERIOD
		{
			get
			{
				return finperiod.Select();
			}
		}

		#region Setups
		public PXSetup<APSetup> APSetup;
		public CMSetupSelect CMSetup;
		#endregion

		public PXAction<APPayment> printCheck;
		[PXUIField(DisplayName = "Print Check", MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable PrintCheck(PXAdapter adapter)
		{
			APPayment doc = Document.Current;
			APPrintChecks pp = PXGraph.CreateInstance<APPrintChecks>();
			PrintChecksFilter filter_copy = PXCache<PrintChecksFilter>.CreateCopy(pp.Filter.Current);
			filter_copy.PayAccountID = doc.CashAccountID;
			filter_copy.PayTypeID = doc.PaymentMethodID;
			pp.Filter.Cache.Update(filter_copy);
			doc.Selected = true;
			doc.Passed = true;
			pp.APPaymentList.Cache.Update(doc);
			pp.APPaymentList.Cache.SetStatus(doc, PXEntryStatus.Updated);
			pp.APPaymentList.Cache.IsDirty = false;
			throw new PXRedirectRequiredException(pp, "Preview");
		}

		public PXAction<APPayment> newVendor;
		[PXUIField(DisplayName = "New Vendor", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable NewVendor(PXAdapter adapter)
		{
			VendorMaint graph = PXGraph.CreateInstance<VendorMaint>();
			throw new PXRedirectRequiredException(graph, "New Vendor");
		}

		public PXAction<APPayment> editVendor;
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

		public PXAction<APPayment> vendorDocuments;
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
			foreach (APPayment apdoc in adapter.Get<APPayment>())
			{
				if (!(bool)apdoc.Hold)
				{
					cache.Update(apdoc);
					list.Add(apdoc);
				}
			}
			if (list.Count == 0)
			{
				throw new PXException(Messages.Document_Status_Invalid);
			}
			Save.Press();
			PXLongOperation.StartOperation(this, delegate() { APDocumentRelease.ReleaseDoc(list, false); });
			return list;
		}

		[PXUIField(DisplayName = "Void", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public override IEnumerable VoidCheck(PXAdapter adapter)
		{
			List<APPayment> rs = new List<APPayment>();

			if (Document.Current != null && Document.Current.Released == true && Document.Current.Voided == false && (Document.Current.DocType == APDocType.Check || Document.Current.DocType == APDocType.Prepayment))
			{
				APAdjust refund_adj = PXSelect<APAdjust, Where<APAdjust.adjdDocType, Equal<Required<APAdjust.adjdDocType>>, And<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>, And<APAdjust.adjgDocType, Equal<APDocType.check>>>>>.SelectWindowed(this, 0, 1, Document.Current.DocType, Document.Current.RefNbr);
				if (refund_adj != null)
				{
					throw new PXException(Messages.PaymentIsPayedByCheck, refund_adj.AdjgRefNbr);
				}

				refund_adj = PXSelect<APAdjust, Where<APAdjust.adjdDocType, Equal<Required<APAdjust.adjdDocType>>, And<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>, And<APAdjust.adjgDocType, NotEqual<APDocType.check>>>>>.SelectWindowed(this, 0, 1, Document.Current.DocType, Document.Current.RefNbr);
				if (refund_adj != null)
				{
					throw new PXException(Messages.PaymentIsRefunded, refund_adj.AdjgRefNbr);
				}

				APPayment voidcheck = Document.Search<APPayment.refNbr>(Document.Current.RefNbr, APDocType.VoidCheck);

				if (voidcheck != null)
				{
					rs.Add(Document.Current);
					return rs;
				}

				//delete unreleased applications
				foreach (APAdjust adj in Adjustments_Raw.Select())
				{
					Adjustments.Cache.Delete(adj);
				}
				this.Save.Press();

				APPayment doc = PXCache<APPayment>.CreateCopy(Document.Current);

				if (finperiod.Current != null && (finperiod.Current.Active == false || finperiod.Current.APClosed == true && glsetup.Current.PostClosedPeriods == false))
				{
					FinPeriod firstopen = PXSelect<FinPeriod, Where<FinPeriod.aPClosed, Equal<boolFalse>, And<FinPeriod.active, Equal<boolTrue>, And<FinPeriod.finPeriodID, Greater<Required<FinPeriod.finPeriodID>>>>>, OrderBy<Asc<FinPeriod.finPeriodID>>>.Select(this, doc.AdjFinPeriodID);
					if (firstopen == null)
					{
						Document.Cache.RaiseExceptionHandling<APPayment.finPeriodID>(Document.Current, Document.Current.FinPeriodID, new PXSetPropertyException(GL.Messages.NoOpenPeriod));
						return adapter.Get();
					}
					doc.AdjFinPeriodID = firstopen.FinPeriodID;
				}

				try
				{
					_IsVoidCheckInProgress = true;
					this.VoidCheckProc(doc);
				}
				catch (PXSetPropertyException)
				{
					this.Clear();
					Document.Current = doc;
					throw;
				}
				finally
				{
					_IsVoidCheckInProgress = false;
				}

				Document.Cache.RaiseExceptionHandling<APPayment.finPeriodID>(Document.Current, Document.Current.FinPeriodID, null);

				rs.Add(Document.Current);
				return rs;
			}
			return Document.Select();
		}

		private class PXLoadInvoiceException : Exception
		{
			public PXLoadInvoiceException(){}
				
			public PXLoadInvoiceException(SerializationInfo info, StreamingContext context)
			: base(info, context){}

		}

		private APAdjust AddAdjustment(APAdjust adj)
		{
			if (Document.Current.CuryUnappliedBal == 0m && Document.Current.CuryOrigDocAmt > 0m)
			{
				throw new PXLoadInvoiceException();
			}
			return this.Adjustments.Insert(adj);
		}

		private void LoadInvoicesProc(bool LoadExistingOnly)
		{
			Dictionary<string, APAdjust> existing = new Dictionary<string, APAdjust>();

			InternalCall = true;
			try
			{
				if (Document.Current == null || Document.Current.VendorID == null || Document.Current.OpenDoc == false || Document.Current.DocType != APDocType.Check && Document.Current.DocType != APDocType.Prepayment && Document.Current.DocType != APDocType.Refund)
				{
					throw new PXLoadInvoiceException();
				}

				foreach (PXResult<APAdjust> res in Adjustments_Raw.Select())
				{
					APAdjust old_adj = (APAdjust)res;

					if (LoadExistingOnly == false)
					{
						old_adj = PXCache<APAdjust>.CreateCopy(old_adj);
						old_adj.CuryAdjgAmt = null;
						old_adj.CuryAdjgDiscAmt = null;
						old_adj.CuryAdjgWhTaxAmt = null;
					}

					string s = string.Format("{0}_{1}", old_adj.AdjdDocType, old_adj.AdjdRefNbr);
					existing.Add(s, old_adj);
					Adjustments.Cache.Delete((APAdjust)res);
				}

				Document.Current.LineCntr++;
				if (Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Notchanged)
				{
					Document.Cache.SetStatus(Document.Current, PXEntryStatus.Updated);
				}
				Document.Cache.IsDirty = true;

				foreach (KeyValuePair<string, APAdjust> res in existing)
				{
					APAdjust adj = new APAdjust();
					adj.AdjdDocType = res.Value.AdjdDocType;
					adj.AdjdRefNbr = res.Value.AdjdRefNbr;

					try
					{
						adj = PXCache<APAdjust>.CreateCopy(AddAdjustment(adj));
						if (res.Value.CuryAdjgWhTaxAmt != null && res.Value.CuryAdjgWhTaxAmt < adj.CuryAdjgWhTaxAmt)
						{
							adj.CuryAdjgWhTaxAmt = res.Value.CuryAdjgWhTaxAmt;
							adj = PXCache<APAdjust>.CreateCopy((APAdjust)this.Adjustments.Cache.Update(adj));
						}

						if (res.Value.CuryAdjgDiscAmt != null && res.Value.CuryAdjgDiscAmt < adj.CuryAdjgDiscAmt)
						{
							adj.CuryAdjgDiscAmt = res.Value.CuryAdjgDiscAmt;
							adj = PXCache<APAdjust>.CreateCopy((APAdjust)this.Adjustments.Cache.Update(adj));
						}

						if (res.Value.CuryAdjgAmt != null && res.Value.CuryAdjgAmt < adj.CuryAdjgAmt)
						{
							adj.CuryAdjgAmt = res.Value.CuryAdjgAmt;
							this.Adjustments.Cache.Update(adj);
						}
					}
					catch (PXSetPropertyException) { }
				}

				if (LoadExistingOnly)
				{
					return;
				}

				PXSelectBase<APInvoice> cmd = new PXSelectReadonly2<APInvoice,
					LeftJoin<APAdjust, On<APAdjust.adjdDocType, Equal<APInvoice.docType>, And<APAdjust.adjdRefNbr, Equal<APInvoice.refNbr>, And<APAdjust.released, Equal<boolFalse>>>>,
                    LeftJoin<APAdjust2, On<APAdjust2.adjgDocType, Equal<APInvoice.docType>, And<APAdjust2.adjgRefNbr, Equal<APInvoice.refNbr>, And<APAdjust2.released, Equal<boolFalse>, And<APAdjust2.voided, Equal<boolFalse>>>>>,
					LeftJoin<APPayment, On<APPayment.docType, Equal<APInvoice.docType>, And<APPayment.refNbr, Equal<APInvoice.refNbr>, And<APPayment.docType, Equal<APDocType.prepayment>>>>>>>,
					Where<APInvoice.vendorID, Equal<Optional<APPayment.vendorID>>,
									And2<Where<APInvoice.released, Equal<True>,Or<APInvoice.prebooked,Equal<True>>>, 
									And<APInvoice.openDoc, Equal<boolTrue>,
									And<APAdjust.adjgRefNbr, IsNull, And<APAdjust2.adjdRefNbr, IsNull,
									And<APPayment.refNbr, IsNull,
									And<Where<APInvoice.docDate, LessEqual<Current<APPayment.adjDate>>,
									And<APInvoice.finPeriodID, LessEqual<Current<APPayment.adjFinPeriodID>>,
									Or<Current<APPayment.docType>, Equal<APDocType.check>, And<Current<APSetup.earlyChecks>, Equal<True>, Or<Current<APPayment.docType>, Equal<APDocType.voidCheck>, And<Current<APSetup.earlyChecks>, Equal<True>>>>>
									>>>>>>>>>, 
                    OrderBy<Asc<APInvoice.dueDate, Asc<APInvoice.refNbr>>>>(this);

				switch (Document.Current.DocType)
				{
					case APDocType.Refund:
						cmd.WhereAnd<Where<APInvoice.docType, Equal<APDocType.debitAdj>>>();
						break;
					case APDocType.Prepayment:
						cmd.WhereAnd<Where<APInvoice.docType, Equal<APDocType.invoice>, Or<APInvoice.docType, Equal<APDocType.creditAdj>>>>();
						break;
					case APDocType.Check:
						cmd.WhereAnd<Where<APInvoice.docType, Equal<APDocType.invoice>, Or<APInvoice.docType, Equal<APDocType.debitAdj>, Or<APInvoice.docType, Equal<APDocType.creditAdj>, Or<APInvoice.docType, Equal<APDocType.prepayment>, And<APInvoice.curyID, Equal<Current<APPayment.curyID>>>>>>>>();
						break;
					default:
						cmd.WhereAnd<Where<True, Equal<False>>>();
						break;
				}

                PXResultset<APInvoice> venddocs = cmd.Select();

                venddocs.Sort((a, b) =>
                {
                    int aSortOrder = 0;
                    int bSortOrder = 0;

                    if (Document.Current.CuryOrigDocAmt > 0m)
                    {
                        aSortOrder += (((APInvoice)a).DocType == APDocType.DebitAdj ? 0 : 1000);
                        bSortOrder += (((APInvoice)b).DocType == APDocType.DebitAdj ? 0 : 1000);
                    }
                    else
                    {
                        aSortOrder += (((APInvoice)a).DocType == APDocType.DebitAdj ? 1000 : 0);
                        bSortOrder += (((APInvoice)b).DocType == APDocType.DebitAdj ? 1000 : 0);
                    }

                    DateTime aDueDate = ((APInvoice)a).DueDate ?? DateTime.MinValue;
                    DateTime bDueDate = ((APInvoice)b).DueDate ?? DateTime.MinValue;

                    object aObj;
                    object bObj;

                    aSortOrder += (1 + aDueDate.CompareTo(bDueDate)) / 2 * 10;
                    bSortOrder += (1 - aDueDate.CompareTo(bDueDate)) / 2 * 10;

                    aObj = ((APInvoice)a).RefNbr;
                    bObj = ((APInvoice)b).RefNbr;
                    aSortOrder += (1 + ((IComparable)aObj).CompareTo(bObj)) / 2;
                    bSortOrder += (1 - ((IComparable)aObj).CompareTo(bObj)) / 2;

                    return aSortOrder.CompareTo(bSortOrder);
                });

				foreach (APInvoice invoice in venddocs)
				{
					string s = string.Format("{0}_{1}", invoice.DocType, invoice.RefNbr);
					if (existing.ContainsKey(s) == false)
					{
						APAdjust adj = new APAdjust();
						adj.AdjdDocType = invoice.DocType;
						adj.AdjdRefNbr = invoice.RefNbr;

						AddAdjustment(adj);
					}
				}

                if (Document.Current.CuryApplAmt < 0m)
                {
                    List<APAdjust> debits = new List<APAdjust>();

                    foreach (APAdjust adj in Adjustments_Raw.Select())
                    {
                        if (adj.AdjdDocType == APDocType.DebitAdj)
                        {
                            debits.Add(adj);
                        }
                    }

                    debits.Sort((a, b) =>
                        {
                            return ((IComparable)a.CuryAdjgAmt).CompareTo(b.CuryAdjgAmt);
                        });

                    foreach (APAdjust adj in debits)
                    {
                        if (adj.CuryAdjgAmt <= -Document.Current.CuryApplAmt)
                        {
                            Adjustments.Delete(adj);
                        }
                        else
                        {
                            APAdjust copy = PXCache<APAdjust>.CreateCopy(adj);
                            copy.CuryAdjgAmt += Document.Current.CuryApplAmt;
                            Adjustments.Update(copy);
                        }
                    }
                }
			}
			catch (PXLoadInvoiceException)
			{
			}
			finally
			{
				InternalCall = false;
			}
		}

		public PXAction<APPayment> loadInvoices;
		[PXUIField(DisplayName = "Load Documents", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Refresh)]
		public virtual IEnumerable LoadInvoices(PXAdapter adapter)
		{
			LoadInvoicesProc(false);
			return adapter.Get();
		}

		public PXAction<APPayment> reverseApplication;
		[PXUIField(DisplayName = "Reverse Application", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(Tooltip = Messages.ReverseApp, ImageKey = PX.Web.UI.Sprite.Main.Refresh)]
		public virtual IEnumerable ReverseApplication(PXAdapter adapter)
		{
            if (Adjustments_History.Current != null && (bool)Adjustments_History.Current.Voided == false && (APPaymentType.CanHaveBalance(Adjustments_History.Current.AdjgDocType) || Adjustments_History.Current.AdjgDocType == APDocType.Refund ||
                Adjustments_History.Current.AdjgDocType == APDocType.Check))
			{
				if ((bool)Document.Current.OpenDoc == false)
				{
					Document.Current.OpenDoc = true;
					Document.Cache.RaiseRowSelected(Document.Current);
				}

				APAdjust adj = PXCache<APAdjust>.CreateCopy(Adjustments_History.Current);

				adj.Voided = true;
				adj.VoidAdjNbr = adj.AdjNbr;
				adj.Released = false;
				adj.AdjNbr = Document.Current.LineCntr;
				adj.AdjBatchNbr = null;

				APAdjust adjnew = new APAdjust();
				adjnew.AdjgDocType = adj.AdjgDocType;
				adjnew.AdjgRefNbr = adj.AdjgRefNbr;
				adjnew.AdjgBranchID = adj.AdjgBranchID;
				adjnew.AdjdDocType = adj.AdjdDocType;
				adjnew.AdjdRefNbr = adj.AdjdRefNbr;
				adjnew.AdjdBranchID = adj.AdjdBranchID;
				adjnew.VendorID = adj.VendorID;
				adjnew.AdjNbr = adj.AdjNbr;
				adjnew.AdjdCuryInfoID = adj.AdjdCuryInfoID;

				this._AutoPaymentApp = true;
				adjnew = this.Adjustments.Insert(adjnew);

				if (adjnew == null)
				{
					return adapter.Get();
				}

				adj.CuryAdjgAmt = -1 * adj.CuryAdjgAmt;
				adj.CuryAdjgDiscAmt = -1 * adj.CuryAdjgDiscAmt;
				adj.AdjAmt = -1 * adj.AdjAmt;
				adj.AdjDiscAmt = -1 * adj.AdjDiscAmt;
				adj.CuryAdjdAmt = -1 * adj.CuryAdjdAmt;
				adj.CuryAdjdDiscAmt = -1 * adj.CuryAdjdDiscAmt;
				adj.RGOLAmt = -1 * adj.RGOLAmt;
				adj.AdjgCuryInfoID = this.Document.Current.CuryInfoID;

				this.Adjustments.Update(adj);
				this._AutoPaymentApp = false;
			}

			return adapter.Get();
		}

		public PXAction<APPayment> viewApplicationDocument;
		[PXUIField(DisplayName = "View Application Document", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton(Tooltip = Messages.ViewAppDoc)]
		public virtual IEnumerable ViewApplicationDocument(PXAdapter adapter)
		{
			if (Adjustments_History.Current != null)
			{
				APAdjust row = (APAdjust)this.Adjustments_History.Current;
				if (!(String.IsNullOrEmpty(row.AdjdDocType) || String.IsNullOrEmpty(row.AdjdRefNbr)))
				{
					APInvoiceEntry iegraph = PXGraph.CreateInstance<APInvoiceEntry>();
					iegraph.Document.Current = iegraph.Document.Search<APInvoice.refNbr>(row.AdjdRefNbr, row.AdjdDocType);
					if (iegraph.Document.Current != null)
					{
						throw new PXRedirectRequiredException(iegraph, true, "View Application Document"){ Mode = PXBaseRedirectException.WindowMode.NewWindow };
					}
				}
			}
			return adapter.Get();
		}

		public PXAction<APPayment> viewCurrentBatch;
		[PXUIField(DisplayName = "View Batch", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton(Tooltip = Messages.ViewBatch)]
		public virtual IEnumerable ViewCurrentBatch(PXAdapter adapter)
		{
			APAdjust row = Adjustments_History.Current;
			if (row != null && !String.IsNullOrEmpty(row.AdjBatchNbr))
			{
				JournalEntry graph = PXGraph.CreateInstance<JournalEntry>();
				graph.BatchModule.Current = PXSelect<Batch,
										Where<Batch.module, Equal<BatchModule.moduleAP>,
										And<Batch.batchNbr, Equal<Required<Batch.batchNbr>>>>>
										.Select(this, row.AdjBatchNbr);
				if (graph.BatchModule.Current != null)
				{
					throw new PXRedirectRequiredException(graph, true, "View Batch") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		#region Buttons
		public PXAction<APPayment> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddress, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton]
		public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			foreach (APPayment current in adapter.Get<APPayment>())
			{
				if (current != null)
				{
					bool needSave = false;
					Save.Press();
					APAddress address = this.Remittance_Address.Select();
					if (address != null && address.IsDefaultAddress == false && address.IsValidated == false)
					{
						if (PXAddressValidator.Validate<APAddress>(this, address, true))
							needSave = true;
					}
					if (needSave == true)
						this.Save.Press();
				}
				yield return current;
			}
		}
		#endregion

		protected virtual void CATran_CashAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CATran_FinPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CATran_TranPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CATran_ReferenceID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CATran_CuryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void APPayment_CuryID_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
		}

		protected virtual void APPayment_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = APDocType.Check;
		}

		protected virtual IEnumerable adjustments()
		{
			PXResultset<APAdjust, APInvoice> ret = new PXResultset<APAdjust, APInvoice>();


			foreach (PXResult<APAdjust, APInvoice> res in Adjustments_Invoices.Select())
			{
				if (Adjustments.Cache.GetStatus((APAdjust)res) == PXEntryStatus.Notchanged)
				{
					CalcBalances<APInvoice>((APAdjust)res, (APInvoice)res, true);
				}
				ret.Add(res);
			}

			foreach (PXResult<APAdjust, APPayment> res in Adjustments_Payments.Select())
			{
				if (Adjustments.Cache.GetStatus((APAdjust)res) == PXEntryStatus.Notchanged)
				{
					CalcBalances<APPayment>((APAdjust)res, (APPayment)res, true);
				}
				ret.Add(res);
			}

			return ret;
		}

		protected virtual IEnumerable adjustments_print()
		{
			if (Document.Current.DocType == APDocType.QuickCheck)
			{
				foreach (PXResult<APAdjust> res in Adjustments_Raw.Select())
				{
					Adjustments.Cache.Delete((APAdjust)res);
				}

				APPayment doc = Document.Current;
				doc.LineCntr++;

				APAdjust adj = new APAdjust();
				adj.AdjdDocType = doc.DocType;
				adj.AdjdRefNbr = doc.RefNbr;
				adj.AdjdBranchID = doc.BranchID;
				adj.AdjdOrigCuryInfoID = doc.CuryInfoID;
				adj.AdjgCuryInfoID = doc.CuryInfoID;
				adj.AdjdCuryInfoID = doc.CuryInfoID;

				adj = Adjustments.Insert(adj);

				adj.CuryDocBal = doc.CuryOrigDocAmt + doc.CuryOrigDiscAmt + doc.CuryOrigWhTaxAmt;
				adj.CuryDiscBal = doc.CuryOrigDiscAmt;
				adj.CuryWhTaxBal = doc.CuryOrigWhTaxAmt;

				adj = PXCache<APAdjust>.CreateCopy(adj);

				adj.AdjgDocType = doc.DocType;
				adj.AdjgRefNbr = doc.RefNbr;
				adj.AdjdAPAcct = doc.APAccountID;
				adj.AdjdAPSub = doc.APSubID;
				adj.AdjdCuryInfoID = doc.CuryInfoID;
				adj.AdjdDocDate = doc.DocDate;
				adj.AdjdFinPeriodID = doc.FinPeriodID;
				adj.AdjdTranPeriodID = doc.TranPeriodID;
				adj.AdjdOrigCuryInfoID = doc.CuryInfoID;
				adj.AdjgCuryInfoID = doc.CuryInfoID;
				adj.AdjgDocDate = doc.DocDate;
				adj.AdjgFinPeriodID = doc.FinPeriodID;
				adj.AdjgTranPeriodID = doc.TranPeriodID;
				adj.AdjNbr = doc.LineCntr;
				adj.AdjAmt = doc.OrigDocAmt;
				adj.AdjDiscAmt = doc.OrigDiscAmt;
				adj.AdjWhTaxAmt = doc.OrigWhTaxAmt;
				adj.RGOLAmt = 0m;
				adj.CuryAdjdAmt = doc.CuryOrigDocAmt;
				adj.CuryAdjdDiscAmt = doc.CuryOrigDiscAmt;
				adj.CuryAdjdWhTaxAmt = doc.CuryOrigWhTaxAmt;
				adj.CuryAdjgAmt = doc.CuryOrigDocAmt;
				adj.CuryAdjgDiscAmt = doc.CuryOrigDiscAmt;
				adj.CuryAdjgWhTaxAmt = doc.CuryOrigWhTaxAmt;
				adj.CuryDocBal = doc.CuryOrigDocAmt + doc.CuryOrigDiscAmt + doc.CuryOrigWhTaxAmt;
				adj.CuryDiscBal = doc.CuryOrigDiscAmt;
				adj.CuryWhTaxBal = doc.CuryOrigWhTaxAmt;
				adj.Released = false;
				adj.VendorID = doc.VendorID;
				Adjustments.Cache.Update(adj);
			}

			return Adjustments_Raw.Select();
		}

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (viewName.ToLower() == "document" && values != null)
			{
				values["CuryApplAmt"] = PXCache.NotSetValue;
				values["CuryUnappliedBal"] = PXCache.NotSetValue;
			}
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		protected virtual void ParentFieldUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<APPayment.adjDate, APPayment.adjFinPeriodID, APPayment.curyID, APPayment.branchID>(e.Row, e.OldRow))
			{
				foreach (APAdjust adj in Adjustments_Raw.Select())
				{
                    adj.AdjgBranchID = ((APPayment)e.Row).BranchID;
                    
                    if (Adjustments.Cache.GetStatus(adj) == PXEntryStatus.Notchanged)
					{
						Adjustments.Cache.SetStatus(adj, PXEntryStatus.Updated);
					}
				}
			}

			if (!sender.ObjectsEqual<APPayment.docDate, APPayment.curyOrigDocAmt, APPayment.curyID, APPayment.hold, APPayment.cashAccountID, APPayment.pTInstanceID, APPayment.extRefNbr, APPayment.vendorID>(e.Row, e.OldRow))
			{
				foreach (PTInstTran iTr in this.ptInstanceTrans.Select())
				{
					if (this.ptInstanceTrans.Cache.GetStatus(iTr) == PXEntryStatus.Notchanged)
					{
						this.ptInstanceTrans.Cache.SetStatus(iTr, PXEntryStatus.Updated);
					}
				}
			}
		}

		public APPaymentEntry()
			: base()
		{
			APSetup setup = APSetup.Current;
			OpenPeriodAttribute.SetValidatePeriod<APPayment.adjFinPeriodID>(Document.Cache, null, PeriodValidation.DefaultSelectUpdate);

			RowUpdated.AddHandler<APPayment>(ParentFieldUpdated);

			_created = new DocumentList<APPayment>(this);
        }

		DocumentList<APPayment> _created = null;
		public DocumentList<APPayment> created
		{
			get
			{
				return _created;
			}
		}

		public virtual void Segregate(APAdjust adj, CurrencyInfo info)
		{
			if (this.IsDirty)
			{
				Save.Press();
			}

            APInvoice apdoc = APInvoice_VendorID_DocType_RefNbr.Select(adj.VendorID, adj.AdjdDocType, adj.AdjdRefNbr);
			if (apdoc == null)
			{
				throw new AdjustedNotFoundException();
			}

			APPayment payment = created.Find<APPayment.vendorID, APPayment.vendorLocationID, APPayment.hidden>(apdoc.VendorID, apdoc.PayLocationID, false) ?? new APPayment();

			if ((adj.SeparateCheck != true || adj.AdjdDocType == APDocType.DebitAdj) && payment.RefNbr != null)
			{
				Document.Current = Document.Search<APPayment.refNbr>(payment.RefNbr, payment.DocType);
			}
			else if (adj.AdjdDocType == APDocType.DebitAdj)
			{
				throw new PXSetPropertyException(Messages.ZeroCheck_CannotPrint, PXErrorLevel.Warning);
			}
			else
			{
				Clear();
				this.Document.View.Answer = WebDialogResult.No;

				payment = new APPayment();
				info.CuryInfoID = null;

				info = PXCache<CurrencyInfo>.CreateCopy(this.currencyinfo.Insert(info));

				payment = new APPayment();
				payment.CuryInfoID = info.CuryInfoID;
				payment.DocType = APDocType.Check;
				payment.Hidden = adj.SeparateCheck;

				payment = PXCache<APPayment>.CreateCopy(this.Document.Insert(payment));

				payment.VendorID = apdoc.VendorID;
				payment.VendorLocationID = apdoc.PayLocationID;
				payment.AdjDate = adj.AdjgDocDate;
				payment.AdjFinPeriodID = adj.AdjgFinPeriodID;
				payment.CashAccountID = apdoc.PayAccountID;
				payment.PaymentMethodID = apdoc.PayTypeID;

				payment = PXCache<APPayment>.CreateCopy(this.Document.Update(payment));

				if (payment.ExtRefNbr == null)
				{
					payment.Hold = true;
					payment = this.Document.Update(payment);
				}

				CurrencyInfo b_info = (CurrencyInfo)PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APPayment.curyInfoID>>>>.Select(this, null);
				b_info.CuryID = info.CuryID;
				b_info.CuryEffDate = info.CuryEffDate;
				b_info.CuryRateTypeID = info.CuryRateTypeID;
				b_info.CuryRate = info.CuryRate;
				b_info.RecipRate = info.RecipRate;
				b_info.CuryMultDiv = info.CuryMultDiv;
				this.currencyinfo.Update(b_info);
			}
		}

		public override void Persist()
		{ 
            foreach (APPayment doc in Document.Cache.Updated)
            {
                if (doc.OpenDoc == true && (bool?)Document.Cache.GetValueOriginal<APPayment.openDoc>(doc) == false && Adjustments_Raw.SelectSingle(doc.DocType, doc.RefNbr, doc.LineCntr) == null)
                {
                    doc.OpenDoc = false;
                    Document.Cache.RaiseRowSelected(doc);
                }
            }
			base.Persist();

			if (Document.Current != null)
			{
				APPayment existed = created.Find(Document.Current);
				if (existed == null)
				{
					created.Add(Document.Current);
				}
				else
				{
					Document.Cache.RestoreCopy(existed, Document.Current);
				}
			}           
		}

		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if ((bool)CMSetup.Current.MCActivated)
			{
				if (cashaccount.Current != null && !string.IsNullOrEmpty(cashaccount.Current.CuryID))
				{
					e.NewValue = cashaccount.Current.CuryID;
					e.Cancel = true;
				}

				//if (vendor.Current != null && !string.IsNullOrEmpty(vendor.Current.CuryID))
				//{
				//  e.NewValue = vendor.Current.CuryID;
				//  e.Cancel = true;
				//}
			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if ((bool)CMSetup.Current.MCActivated)
			{
				if (cashaccount.Current != null && !string.IsNullOrEmpty(cashaccount.Current.CuryRateTypeID))
				{
					e.NewValue = cashaccount.Current.CuryRateTypeID;
					e.Cancel = true;
				}

				//if (vendor.Current != null && !string.IsNullOrEmpty(vendor.Current.CuryRateTypeID))
				//{
				//  e.NewValue = vendor.Current.CuryRateTypeID;
				//  e.Cancel = true;
				//}
			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Cache.Current != null)
			{
				e.NewValue = ((APPayment)Document.Cache.Current).DocDate;
				e.Cancel = true;
			}
		}

		protected virtual void APPayment_VendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			vendor.RaiseFieldUpdated(sender, e.Row);

			sender.SetDefaultExt<APInvoice.vendorLocationID>(e.Row);
		}

		protected virtual void APPayment_VendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			location.RaiseFieldUpdated(sender, e.Row);

    		sender.SetDefaultExt<APPayment.paymentMethodID>(e.Row);
			sender.SetDefaultExt<APPayment.aPAccountID>(e.Row);
			sender.SetDefaultExt<APPayment.aPSubID>(e.Row);

			try
			{
				APAddressAttribute.DefaultRecord<APPayment.remitAddressID>(sender, e.Row);
				APContactAttribute.DefaultRecord<APPayment.remitContactID>(sender, e.Row);
			}
			catch (PXFieldValueProcessingException ex)
			{
				ex.ErrorValue = location.Current.LocationCD;
				throw;
			}
		}

		protected virtual void APPayment_ExtRefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row != null && ((APPayment)e.Row).DocType == APDocType.VoidCheck)
			{
				//avoid webdialog in PaymentRef attribute
				e.Cancel = true;
			}
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

		protected virtual void APPayment_APAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (location.Current != null && e.Row != null)
			{
				e.NewValue = null;
				if (((APPayment)e.Row).DocType == APDocType.Prepayment)
				{
					e.NewValue = GetAcctSub<Vendor.prepaymentAcctID>(vendor.Cache, vendor.Current);
				}
				if (string.IsNullOrEmpty((string)e.NewValue))
				{
					e.NewValue = GetAcctSub<Location.aPAccountID>(location.Cache, location.Current);
				}
			}
		}

		protected virtual void APPayment_APSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (location.Current != null && e.Row != null)
			{
				e.NewValue = null;
				if (((APPayment)e.Row).DocType == APDocType.Prepayment)
				{
					e.NewValue = GetAcctSub<Vendor.prepaymentSubID>(vendor.Cache, vendor.Current);
				}
				if (string.IsNullOrEmpty((string)e.NewValue))
				{
					e.NewValue = GetAcctSub<Location.aPSubID>(location.Cache, location.Current);
				}
			}
		}

        protected virtual void APPayment_CashAccountID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            APPayment payment = (APPayment)e.Row;
			if (payment == null) return;
            bool wasFirstErrorCharge = false;
            foreach (APPaymentChargeTran charge in PaymentCharges.Select())
            {
                CashAccountETDetail eTDetail = (CashAccountETDetail)PXSelectJoin<CashAccountETDetail,
                                                                    InnerJoin<Account,On<Account.accountID, Equal<CashAccountETDetail.accountID>>>,
                                                                    Where<Account.accountCD, Equal<Required<APPayment.cashAccountID>>,
                                                                    And<CashAccountETDetail.entryTypeID, Equal<Required<APPaymentChargeTran.entryTypeID>>>>>.
                                                                    Select(this, e.NewValue, charge.EntryTypeID);
                if (eTDetail == null)
                {
                    if (!wasFirstErrorCharge)
                    {
                        if (Document.Ask(Messages.Warning, Messages.SomeChargeNotRelatedWithCashAccount, MessageButtons.YesNo) == WebDialogResult.No)
                        {
                            e.NewValue = payment.CashAccountID;
                            break;
                        }
                        wasFirstErrorCharge = true;
                    }
                    PaymentCharges.Cache.Delete(charge);
                }
            }
        }

		protected virtual void APPayment_CashAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APPayment payment = (APPayment)e.Row;
			cashaccount.RaiseFieldUpdated(sender, e.Row);

			if (_IsVoidCheckInProgress == false && CMSetup.Current.MCActivated == true)
			{
				CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<APPayment.curyInfoID>(sender, e.Row);

				string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);
				if (string.IsNullOrEmpty(message) == false)
				{
					sender.RaiseExceptionHandling<APPayment.adjDate>(e.Row, payment.DocDate, new PXSetPropertyException(message, PXErrorLevel.Warning));
				}

				if (info != null)
				{
					payment.CuryID = info.CuryID;
				}
			}

            sender.SetValueExt<APPayment.pTInstanceID>(e.Row, null); //??

			payment.Cleared = false;
			payment.ClearDate = null;

			if ((cashaccount.Current != null) && (cashaccount.Current.Reconcile == false))
			{
				payment.Cleared = true;
				payment.ClearDate = payment.DocDate;
			}

			sender.SetDefaultExt<APPayment.depositAsBatch>(e.Row);
			sender.SetDefaultExt<APPayment.depositAfter>(e.Row);
        }

		protected virtual void APPayment_PaymentMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			paymenttype.RaiseFieldUpdated(sender, e.Row);

			sender.SetDefaultExt<APPayment.cashAccountID>(e.Row);
			sender.SetDefaultExt<APPayment.pTInstanceID>(e.Row);			
			sender.SetDefaultExt<APPayment.printCheck>(e.Row);
		}
		

		protected virtual void APPayment_PrintCheck_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			switch (((APPayment)e.Row).DocType)
			{
				case APDocType.Refund:
				case APDocType.Prepayment:
					e.NewValue = false;
					e.Cancel = true;
					break;
			}
		}

	    protected virtual bool MustPrintCheck(APPayment payment)
	    {
	        if(paymenttype.Current != null && payment != null)
	        {
	            return payment.Printed == false && paymenttype.Current.PrintOrExport == true;
	        }
	        return false;
	    }

	    protected virtual void APPayment_PrintCheck_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
            if (MustPrintCheck((APPayment)e.Row))
			{
				sender.SetValueExt<APPayment.extRefNbr>(e.Row, null);
				sender.SetValueExt<APPayment.hold>(e.Row, true);
			}
		}

		protected virtual void APPayment_AdjDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if ((bool)((APPayment)e.Row).Released == false && (bool)((APPayment)e.Row).VoidAppl == false)
			{
				CurrencyInfoAttribute.SetEffectiveDate<APPayment.adjDate>(sender, e);
				sender.SetDefaultExt<APPayment.depositAfter>(e.Row);
				LoadInvoicesProc(true);
			}
		}

		protected virtual void APPayment_AdjDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ((bool)((APPayment)e.Row).VoidAppl == false)
			{
				//foreach (PXResult<APAdjust, APInvoice> res in Adjustments_raw.Select((object)null))
				//{
				//	throw new PXSetPropertyException(Messages.Cannot_Change_Details_Exists);
				//}

				if (vendor.Current != null && (bool)vendor.Current.Vendor1099)
				{
					string Year1099 = ((DateTime)e.NewValue).Year.ToString();
					AP1099Year year = PXSelect<AP1099Year, Where<AP1099Year.finYear, Equal<Required<AP1099Year.finYear>>>>.Select(this, Year1099);

					if (year != null && year.Status != "N")
					{
						throw new PXSetPropertyException(Messages.AP1099_PaymentDate_NotIn_OpenYear, PXUIFieldAttribute.GetDisplayName<APPayment.adjDate>(sender));
					}
				}
			}
		}

		protected virtual void APPayment_CuryOrigDocAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (!(bool)((APPayment)e.Row).Released)
			{
				sender.SetValueExt<APPayment.curyDocBal>(e.Row, ((APPayment)e.Row).CuryOrigDocAmt);
			}
		}

		protected virtual void APAdjust_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			string errmsg = PXUIFieldAttribute.GetError<APAdjust.adjdRefNbr>(sender, e.Row);

			e.Cancel = (((APAdjust)e.Row).AdjdRefNbr == null || string.IsNullOrEmpty(errmsg) == false);
		}

		protected virtual void APAdjust_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			//Prepayment requests have Check CuryInfoID in AdjdCuryInfoID should not be deleted
			if (((APAdjust)e.Row).AdjdCuryInfoID != ((APAdjust)e.Row).AdjgCuryInfoID && ((APAdjust)e.Row).AdjdCuryInfoID != ((APAdjust)e.Row).AdjdOrigCuryInfoID && ((APAdjust)e.Row).VoidAdjNbr == null)
			{
				foreach (CurrencyInfo info in CurrencyInfo_CuryInfoID.Select(((APAdjust)e.Row).AdjdCuryInfoID))
				{
					currencyinfo.Delete(info);
				}
			}
		}

		public virtual void APAdjust_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			APAdjust doc = (APAdjust)e.Row;

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete)
			{
				return;
			}

			if (((DateTime)doc.AdjdDocDate).CompareTo((DateTime)Document.Current.AdjDate) > 0 && (doc.AdjgDocType != APDocType.Check && doc.AdjgDocType != APDocType.VoidCheck || APSetup.Current.EarlyChecks == false))
			{
				if (sender.RaiseExceptionHandling<APAdjust.adjdRefNbr>(e.Row, doc.AdjdRefNbr, new PXSetPropertyException(Messages.ApplDate_Less_DocDate, PXErrorLevel.RowError, PXUIFieldAttribute.GetDisplayName<APPayment.adjDate>(Document.Cache))))
				{
					throw new PXRowPersistingException(PXDataUtils.FieldName<APAdjust.adjdDocDate>(), doc.AdjdDocDate, Messages.ApplDate_Less_DocDate, PXUIFieldAttribute.GetDisplayName<APPayment.adjDate>(Document.Cache));
				}
			}

			if (((string)doc.AdjdFinPeriodID).CompareTo((string)Document.Current.AdjFinPeriodID) > 0 && (doc.AdjgDocType != APDocType.Check && doc.AdjgDocType != APDocType.VoidCheck || APSetup.Current.EarlyChecks == false))
			{
				if (sender.RaiseExceptionHandling<APAdjust.adjdRefNbr>(e.Row, doc.AdjdRefNbr, new PXSetPropertyException(Messages.ApplPeriod_Less_DocPeriod, PXErrorLevel.RowError, PXUIFieldAttribute.GetDisplayName<APPayment.adjFinPeriodID>(Document.Cache))))
				{
					throw new PXRowPersistingException(PXDataUtils.FieldName<APAdjust.adjdFinPeriodID>(), doc.AdjdFinPeriodID, Messages.ApplPeriod_Less_DocPeriod, PXUIFieldAttribute.GetDisplayName<APPayment.adjFinPeriodID>(Document.Cache));
				}
			}

			if (doc.AdjdDocType == APDocType.Prepayment && (doc.AdjgDocType == APDocType.Check || doc.AdjgDocType == APDocType.VoidCheck))
			{
				doc.AdjdCuryInfoID = Document.Current.CuryInfoID;
				doc.AdjdOrigCuryInfoID = Document.Current.CuryInfoID;
			}

			if (doc.CuryDocBal < 0m)
			{
				sender.RaiseExceptionHandling<APAdjust.curyAdjgAmt>(e.Row, doc.CuryAdjgAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
			}

			if (doc.AdjgDocType != APDocType.QuickCheck && doc.CuryDiscBal < 0m)
			{
				sender.RaiseExceptionHandling<APAdjust.curyAdjgDiscAmt>(e.Row, doc.CuryAdjgDiscAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
			}

			if (doc.AdjgDocType != APDocType.QuickCheck && doc.CuryWhTaxBal < 0m)
			{
				sender.RaiseExceptionHandling<APAdjust.curyAdjgWhTaxAmt>(e.Row, doc.CuryAdjgWhTaxAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
			}
		}

		protected virtual void APAdjust_AdjdRefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = (Document.Current != null && (Document.Current.VoidAppl == true || Document.Current.DocType == APDocType.QuickCheck) || this._AutoPaymentApp);
		}

		protected virtual void APAdjust_AdjdRefNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			try
			{
				APAdjust adj = e.Row as APAdjust;
				if (adj.AdjdCuryInfoID == null)
				{
					foreach (PXResult<APInvoice, CurrencyInfo> res in PXSelectJoin<APInvoice, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APInvoice.curyInfoID>>>, Where<APInvoice.vendorID, Equal<Current<APPayment.vendorID>>, And<APInvoice.docType, Equal<Required<APInvoice.docType>>, And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>>.Select(this, adj.AdjdDocType, adj.AdjdRefNbr))
					{
						APAdjust_AdjdRefNbr_FieldUpdated<APInvoice>(res, adj);
						return;
					}
					foreach (PXResult<APPayment, CurrencyInfo> res in PXSelectJoin<APPayment, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APPayment.curyInfoID>>>, Where<APPayment.vendorID, Equal<Current<APPayment.vendorID>>, And<APPayment.docType, Equal<Required<APPayment.docType>>, And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>>.Select(this, adj.AdjdDocType, adj.AdjdRefNbr))
					{
						APAdjust_AdjdRefNbr_FieldUpdated<APPayment>(res, adj);
					}
				}
			}
			catch (PXSetPropertyException ex)
			{
				throw new PXException(ex.Message);
			}
		}

		private void APAdjust_AdjdRefNbr_FieldUpdated<T>(PXResult<T, CurrencyInfo> res, APAdjust adj)
			where T : APRegister, IInvoice, new()
		{
			CurrencyInfo info = (CurrencyInfo)res;
			CurrencyInfo info_copy = null;
			T invoice = (T)res;

			if (adj.AdjdDocType == APDocType.Prepayment && (adj.AdjgDocType == APDocType.Check || adj.AdjgDocType == APDocType.VoidCheck))
			{
				if (object.Equals(invoice.CuryID, Document.Current.CuryID) == false)
				{
					throw new PXSetPropertyException(Messages.CheckCuryNotPPCury);
				}

				//Prepayment cannot have RGOL
				info = new CurrencyInfo();
				info.CuryInfoID = Document.Current.CuryInfoID;
				info_copy = info;
			}
			else
			{
				info_copy = PXCache<CurrencyInfo>.CreateCopy((CurrencyInfo)res);
				info_copy.CuryInfoID = null;
				info_copy = (CurrencyInfo)currencyinfo.Cache.Insert(info_copy);

				//valid for future Bills payed with Checks & future bills payed with Prepayments
				currencyinfo.Cache.SetValueExt<CurrencyInfo.curyEffDate>(info_copy, Document.Current.DocDate);
			}
			adj.VendorID = invoice.VendorID;
			adj.AdjgDocDate = Document.Current.AdjDate;
			adj.AdjgCuryInfoID = Document.Current.CuryInfoID;
			adj.AdjdCuryInfoID = info_copy.CuryInfoID;
			adj.AdjdOrigCuryInfoID = info.CuryInfoID;
			adj.AdjdBranchID = invoice.BranchID;
			adj.AdjdAPAcct = invoice.APAccountID;
			adj.AdjdAPSub = invoice.APSubID;
			adj.AdjdDocDate = invoice.DocDate;
			adj.AdjdFinPeriodID = invoice.FinPeriodID;
			adj.Released = false;

			CalcBalances<T>(adj, invoice, false, true);

			if (adj.CuryWhTaxBal >= 0m &&
										adj.CuryDiscBal >= 0m &&
										adj.CuryDocBal - adj.CuryWhTaxBal - adj.CuryDiscBal <= 0m)
			{
				//no amount suggestion is possible
				return;
			}

			decimal? CuryApplDiscAmt = (adj.AdjgDocType == APDocType.DebitAdj) ? 0m : adj.CuryDiscBal;
			decimal? CuryApplAmt = adj.CuryDocBal - adj.CuryWhTaxBal - CuryApplDiscAmt;
			decimal? CuryUnappliedBal = Document.Current.CuryUnappliedBal;

            if (Document.Current != null && adj.AdjgBalSign < 0m)
            {
                if (CuryUnappliedBal < 0m)
                {
                    CuryApplAmt = Math.Min((decimal)CuryApplAmt, Math.Abs((decimal)CuryUnappliedBal));
                }
            }
            else if (Document.Current != null && CuryUnappliedBal > 0m && adj.AdjgBalSign > 0m && CuryUnappliedBal < CuryApplDiscAmt)
            {
				CuryApplAmt = CuryUnappliedBal;
				CuryApplDiscAmt = 0m;
			}
			else if (Document.Current != null && CuryUnappliedBal > 0m && adj.AdjgBalSign > 0m)
			{
				CuryApplAmt = Math.Min((decimal)CuryApplAmt, (decimal)CuryUnappliedBal);
			}
			else if (Document.Current != null && CuryUnappliedBal <= 0m && Document.Current.CuryOrigDocAmt > 0)
			{
				CuryApplAmt = 0m;
			}

			adj.CuryAdjgAmt = CuryApplAmt;
			adj.CuryAdjgDiscAmt = CuryApplDiscAmt;
			adj.CuryAdjgWhTaxAmt = adj.CuryWhTaxBal;

			CalcBalances<T>(adj, invoice, true, true);
		}

		private bool internalCall;

		protected virtual void APAdjust_CuryDocBal_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (!internalCall)
			{
				if (e.Row != null && ((APAdjust)e.Row).AdjdCuryInfoID != null && ((APAdjust)e.Row).CuryDocBal == null && sender.GetStatus(e.Row) != PXEntryStatus.Deleted)
				{
					CalcBalances(e.Row, false);
				}
				if (e.Row != null)
				{
					e.NewValue = ((APAdjust)e.Row).CuryDocBal;
				}
			}
			e.Cancel = true;
		}

		protected virtual void APAdjust_CuryDiscBal_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (!internalCall)
			{
				if (e.Row != null && ((APAdjust)e.Row).AdjdCuryInfoID != null && ((APAdjust)e.Row).CuryDiscBal == null && sender.GetStatus(e.Row) != PXEntryStatus.Deleted)
				{
					CalcBalances(e.Row, false);
				}
				if (e.Row != null)
				{
					e.NewValue = ((APAdjust)e.Row).CuryDiscBal;
				}
			}
			e.Cancel = true;
		}

		protected virtual void APAdjust_CuryWhTaxBal_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (!internalCall)
			{
				if (e.Row != null && ((APAdjust)e.Row).AdjdCuryInfoID != null && ((APAdjust)e.Row).CuryWhTaxBal == null && sender.GetStatus(e.Row) != PXEntryStatus.Deleted)
				{
					CalcBalances(e.Row, false);
				}
				if (e.Row != null)
				{
					e.NewValue = ((APAdjust)e.Row).CuryWhTaxBal;
				}
			}
			e.Cancel = true;
		}

		protected virtual void APAdjust_AdjdCuryRate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ((decimal)e.NewValue <= 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GT, ((int)0).ToString());
			}
		}

		protected virtual void APAdjust_AdjdCuryRate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APAdjust adj = (APAdjust)e.Row;

			CurrencyInfo pay_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APAdjust.adjgCuryInfoID>>>>.SelectSingleBound(this, new object[] { e.Row });
			CurrencyInfo vouch_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APAdjust.adjdCuryInfoID>>>>.SelectSingleBound(this, new object[] { e.Row });

            decimal payment_docbal = (decimal)adj.CuryAdjgAmt;

			if (string.Equals(pay_info.CuryID, vouch_info.CuryID) && adj.AdjdCuryRate != 1m)
			{
				adj.AdjdCuryRate = 1m;
				vouch_info.SetCuryEffDate(currencyinfo.Cache, Document.Current.DocDate);
			}
			else if (string.Equals(vouch_info.CuryID, vouch_info.BaseCuryID))
			{
				adj.AdjdCuryRate = pay_info.CuryMultDiv == "M" ? 1 / pay_info.CuryRate : pay_info.CuryRate;
			}
			else
			{
                vouch_info.CuryRate = adj.AdjdCuryRate;
                vouch_info.RecipRate = Math.Round(1m / (decimal)adj.AdjdCuryRate, 8, MidpointRounding.AwayFromZero);
                PXCurrencyAttribute.CuryConvBase(sender, vouch_info, (decimal)adj.CuryAdjdAmt, out payment_docbal);                
                
                vouch_info.CuryRate = Math.Round((decimal)adj.AdjdCuryRate * (pay_info.CuryMultDiv == "M" ? (decimal)pay_info.CuryRate : 1m / (decimal)pay_info.CuryRate), 8, MidpointRounding.AwayFromZero);
				vouch_info.RecipRate = Math.Round((pay_info.CuryMultDiv == "M" ? 1m / (decimal)pay_info.CuryRate : (decimal)pay_info.CuryRate) / (decimal)adj.AdjdCuryRate, 8, MidpointRounding.AwayFromZero);
				vouch_info.CuryMultDiv = "M";
			}

			if (Caches[typeof(CurrencyInfo)].GetStatus(vouch_info) == PXEntryStatus.Notchanged)
			{
				Caches[typeof(CurrencyInfo)].SetStatus(vouch_info, PXEntryStatus.Updated);
			}

            CalcBalances(e.Row, true);
            if (payment_docbal != (decimal)adj.CuryAdjgAmt) sender.SetValueExt<APAdjust.curyAdjgAmt>(e.Row, payment_docbal);
		}

		protected virtual void APAdjust_CuryAdjgAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APAdjust adj = (APAdjust)e.Row;

			if (adj.CuryDocBal == null || adj.CuryDiscBal == null || adj.CuryWhTaxBal == null)
			{
				CalcBalances(e.Row, false);
			}

			if (adj.CuryDocBal == null)
			{
				throw new PXSetPropertyException<APAdjust.adjdRefNbr>(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<APAdjust.adjdRefNbr>(sender));
			}

			if (adj.VoidAdjNbr == null && (decimal)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
			}

			if (adj.VoidAdjNbr != null && (decimal)e.NewValue > 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_LE, ((int)0).ToString());
			}

			if ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgAmt - (decimal)e.NewValue < 0)
			{
				throw new PXSetPropertyException(Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgAmt).ToString());
			}
		}

		protected virtual void APAdjust_CuryAdjgAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.OldValue != null && ((APAdjust)e.Row).CuryDocBal == 0m && ((APAdjust)e.Row).CuryAdjgAmt < (decimal)e.OldValue)
			{
				((APAdjust)e.Row).CuryAdjgDiscAmt = 0m;
			}
			CalcBalances(e.Row, true);
		}

		protected virtual void APAdjust_CuryAdjgDiscAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APAdjust adj = (APAdjust)e.Row;

			if (adj.CuryDocBal == null || adj.CuryDiscBal == null || adj.CuryWhTaxBal == null)
			{
				CalcBalances(e.Row, false);
			}

			if (adj.CuryDocBal == null || adj.CuryDiscBal == null)
			{
				throw new PXSetPropertyException<APAdjust.adjdRefNbr>(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<APAdjust.adjdRefNbr>(sender));
			}

			if (adj.VoidAdjNbr == null && (decimal)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
			}

			if (adj.VoidAdjNbr != null && (decimal)e.NewValue > 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_LE, ((int)0).ToString());
			}

            if ((decimal)adj.CuryDiscBal + (decimal)adj.CuryAdjgDiscAmt - (decimal)e.NewValue < 0)
            {
                throw new PXSetPropertyException((decimal)adj.CuryDiscBal + (decimal)adj.CuryAdjgDiscAmt == 0 ? CS.Messages.Entry_EQ : Messages.Entry_LE, ((decimal)adj.CuryDiscBal + (decimal)adj.CuryAdjgDiscAmt).ToString());
            }

			if (adj.CuryAdjgAmt != null && (sender.GetValuePending<APAdjust.curyAdjgAmt>(e.Row) == PXCache.NotSetValue || (Decimal?)sender.GetValuePending<APAdjust.curyAdjgAmt>(e.Row) == adj.CuryAdjgAmt))
            {
                if ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgDiscAmt - (decimal)e.NewValue < 0)
                {
                    throw new PXSetPropertyException((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgDiscAmt == 0 ? CS.Messages.Entry_EQ : Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgDiscAmt).ToString());
                }
			}
		}

		protected virtual void APAdjust_CuryAdjgDiscAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CalcBalances(e.Row, true);
		}

		protected virtual void APAdjust_CuryAdjgWhTaxAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APAdjust adj = (APAdjust)e.Row;

			if (adj.CuryDocBal == null || adj.CuryDiscBal == null || adj.CuryWhTaxBal == null)
			{
				CalcBalances(e.Row, false);
			}

			if (adj.CuryDocBal == null || adj.CuryWhTaxBal == null)
			{
				throw new PXSetPropertyException<APAdjust.adjdRefNbr>(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<APAdjust.adjdRefNbr>(sender));
			}

			if (adj.VoidAdjNbr == null && (decimal)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
			}

			if (adj.VoidAdjNbr != null && (decimal)e.NewValue > 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_LE, ((int)0).ToString());
			}

			if ((decimal)adj.CuryWhTaxBal + (decimal)adj.CuryAdjgWhTaxAmt - (decimal)e.NewValue < 0)
			{
				throw new PXSetPropertyException(Messages.Entry_LE, ((decimal)adj.CuryWhTaxBal + (decimal)adj.CuryAdjgWhTaxAmt).ToString());
			}

			if (adj.CuryAdjgAmt != null && (sender.GetValuePending<APAdjust.curyAdjgAmt>(e.Row) == PXCache.NotSetValue || (Decimal?)sender.GetValuePending<APAdjust.curyAdjgAmt>(e.Row) == adj.CuryAdjgAmt))
			{
				if ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgWhTaxAmt - (decimal)e.NewValue < 0)
				{
					throw new PXSetPropertyException(Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgWhTaxAmt).ToString());
				}
			}
		}

		protected virtual void APAdjust_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			APAdjust adj = (APAdjust)e.Row;

			if (adj == null || internalCall)
			{
				return;
			}

			bool adjNotReleased = (bool)(adj.Released == false);
            PXUIFieldAttribute.SetEnabled<APAdjust.adjdDocType>(cache, adj, adjNotReleased && (adj.Voided != true));
            PXUIFieldAttribute.SetEnabled<APAdjust.adjdRefNbr>(cache, adj, adjNotReleased && (adj.Voided != true));
            PXUIFieldAttribute.SetEnabled<APAdjust.curyAdjgAmt>(cache, adj, adjNotReleased && (adj.Voided != true));
            PXUIFieldAttribute.SetEnabled<APAdjust.curyAdjgDiscAmt>(cache, adj, adjNotReleased && (adj.Voided != true));
            PXUIFieldAttribute.SetEnabled<APAdjust.curyAdjgWhTaxAmt>(cache, adj, adjNotReleased && (adj.Voided != true));
			PXUIFieldAttribute.SetEnabled<APAdjust.adjBatchNbr>(cache, adj, false);
			//
			PXUIFieldAttribute.SetVisible<APAdjust.adjBatchNbr>(cache, adj, !adjNotReleased);

			bool EnableCrossRate = false;
			if (adj.Released == false)
			{
				CurrencyInfo pay_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APAdjust.adjgCuryInfoID>>>>.SelectSingleBound(this, new object[] { e.Row });
				CurrencyInfo vouch_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APAdjust.adjdCuryInfoID>>>>.SelectSingleBound(this, new object[] { e.Row });

				EnableCrossRate = string.Equals(pay_info.CuryID, vouch_info.CuryID) == false && string.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) == false;
			}
			PXUIFieldAttribute.SetEnabled<APAdjust.adjdCuryRate>(cache, adj, EnableCrossRate);
		}

        protected virtual void APAdjust_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
        {
            APAdjust adj = (APAdjust)e.Row;
						if (_IsVoidCheckInProgress == false && adj.Voided == true)
            {
                throw new PXSetPropertyException(ErrorMessages.CantUpdateRecord);
            }
        }

        protected virtual void APAdjust_CuryAdjgWhTaxAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CalcBalances(e.Row, true);
		}

        public bool TakeDiscAlways = false;

        private void CalcBalances(object row, bool isCalcRGOL)
		{
            CalcBalances(row, isCalcRGOL, !TakeDiscAlways);
		}

		private void CalcBalances(object row, bool isCalcRGOL, bool DiscOnDiscDate)
		{
			APAdjust adj = (APAdjust)row;
			foreach (APInvoice voucher in APInvoice_VendorID_DocType_RefNbr.Select(adj.VendorID, adj.AdjdDocType, adj.AdjdRefNbr))
			{
				CalcBalances<APInvoice>(adj, voucher, isCalcRGOL, DiscOnDiscDate);
				return;
			}

			foreach (APPayment payment in APPayment_VendorID_DocType_RefNbr.Select(adj.VendorID, adj.AdjdDocType, adj.AdjdRefNbr))
			{
				CalcBalances<APPayment>(adj, payment, isCalcRGOL, DiscOnDiscDate);
			}
		}

		private void CalcBalances<T>(APAdjust adj, T voucher, bool isCalcRGOL)
			where T : IInvoice
		{
            CalcBalances<T>(CurrencyInfo_CuryInfoID, adj, voucher, isCalcRGOL, !TakeDiscAlways);
		}

		private void CalcBalances<T>(APAdjust adj, T voucher, bool isCalcRGOL, bool DiscOnDiscDate)
			where T :  IInvoice
		{
			CalcBalances<T>(CurrencyInfo_CuryInfoID, adj, voucher, isCalcRGOL, DiscOnDiscDate);
		}

		public static void CalcBalances<T>(PXSelectBase<CurrencyInfo> CurrencyInfo_CuryInfoID, APAdjust adj, T voucher, bool isCalcRGOL, bool DiscOnDiscDate)
			where T : IInvoice
		{
			PaymentEntry.CalcBalances<T, APAdjust>(CurrencyInfo_CuryInfoID, adj.AdjgCuryInfoID, adj.AdjdCuryInfoID, voucher, adj);

			if (DiscOnDiscDate)
			{
				PaymentEntry.CalcDiscount<T, APAdjust>(adj.AdjgDocDate, voucher, adj);
			}

			CurrencyInfo pay_info = CurrencyInfo_CuryInfoID.Select(adj.AdjgCuryInfoID);
			CurrencyInfo vouch_info = CurrencyInfo_CuryInfoID.Select(adj.AdjdCuryInfoID);

			if (vouch_info != null && string.Equals(pay_info.CuryID, vouch_info.CuryID) == false)
			{
				adj.AdjdCuryRate = Math.Round((vouch_info.CuryMultDiv == "M" ? (decimal)vouch_info.CuryRate : 1 / (decimal)vouch_info.CuryRate) * (pay_info.CuryMultDiv == "M" ? 1 / (decimal)pay_info.CuryRate : (decimal)pay_info.CuryRate), 8, MidpointRounding.AwayFromZero);
			}
			else
			{
				adj.AdjdCuryRate = 1m;
			}

			PaymentEntry.AdjustBalance<APAdjust>(CurrencyInfo_CuryInfoID, adj);
			if (isCalcRGOL && (adj.Voided == null || adj.Voided == false))
			{
				PaymentEntry.CalcRGOL<T, APAdjust>(CurrencyInfo_CuryInfoID, voucher, adj);
				adj.RGOLAmt = (bool)adj.ReverseGainLoss ? -1m * adj.RGOLAmt : adj.RGOLAmt;

				if (adj.AdjdDocType == APDocType.Prepayment && (adj.AdjgDocType == APDocType.Check || adj.AdjgDocType == APDocType.VoidCheck))
				{
					adj.RGOLAmt = 0m;
				}
			}
		}

		protected virtual void CurrencyInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CurrencyInfo info = e.Row as CurrencyInfo;
			if (info != null)
			{
				bool curyenabled = info.AllowUpdate(this.Adjustments.Cache);

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

		protected virtual void CurrencyInfo_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<CurrencyInfo.curyID, CurrencyInfo.curyRate, CurrencyInfo.curyMultDiv>(e.Row, e.OldRow))
			{
				foreach (APAdjust adj in PXSelect<APAdjust, Where<APAdjust.adjgCuryInfoID, Equal<Required<APAdjust.adjgCuryInfoID>>>>.Select(sender.Graph, ((CurrencyInfo)e.Row).CuryInfoID))
				{
					if (Adjustments.Cache.GetStatus(adj) == PXEntryStatus.Notchanged)
					{
						Adjustments.Cache.SetStatus(adj, PXEntryStatus.Updated);
					}

					CalcBalances(adj, true);

					if (adj.CuryDocBal < 0m)
					{
						Adjustments.Cache.RaiseExceptionHandling<APAdjust.curyAdjgAmt>(adj, adj.CuryAdjgAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
					}

					if (adj.CuryDiscBal < 0m)
					{
						Adjustments.Cache.RaiseExceptionHandling<APAdjust.curyAdjgDiscAmt>(adj, adj.CuryAdjgDiscAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
					}

					if (adj.CuryWhTaxBal < 0m)
					{
						Adjustments.Cache.RaiseExceptionHandling<APAdjust.curyAdjgWhTaxAmt>(adj, adj.CuryAdjgWhTaxAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
					}
				}
			}
		}

		protected virtual void APPayment_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			APPayment doc = (APPayment)e.Row;

			//true for DebitAdj and Prepayment Requests
			if ((bool)doc.Released == false && doc.CashAccountID == null)
			{
				if (sender.RaiseExceptionHandling<APPayment.cashAccountID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APPayment.cashAccountID).Name)))
				{
					throw new PXRowPersistingException(typeof(APPayment.cashAccountID).Name, null, ErrorMessages.FieldIsEmpty, typeof(APPayment.cashAccountID).Name);
				}
			}

			//true for DebitAdj and Prepayment Requests
			if ((bool)doc.Released == false && string.IsNullOrEmpty(doc.PaymentMethodID))
			{
				if (sender.RaiseExceptionHandling<APPayment.paymentMethodID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APPayment.paymentMethodID).Name)))
				{
					throw new PXRowPersistingException(typeof(APPayment.paymentMethodID).Name, null, ErrorMessages.FieldIsEmpty, typeof(APPayment.paymentMethodID).Name);
				}
			}

			if ((bool)((APPayment)e.Row).Hold == false && (bool)((APPayment)e.Row).Released == false)
			{
				if (string.IsNullOrEmpty(doc.ExtRefNbr))
				{
					if (sender.RaiseExceptionHandling<APPayment.extRefNbr>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APPayment.extRefNbr).Name)))
					{
						throw new PXRowPersistingException(typeof(APPayment.extRefNbr).Name, null, ErrorMessages.FieldIsEmpty, typeof(APPayment.extRefNbr).Name);
					}
				}
			}

			PaymentRefAttribute.SetUpdateCashManager<APPayment.extRefNbr>(sender, e.Row, ((APPayment)e.Row).DocType != APDocType.VoidCheck && ((APPayment)e.Row).DocType != APDocType.Refund);
		}

		protected virtual void APPayment_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			APPayment row = (APPayment)e.Row;
			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
			{
				if (row.DocType == APPaymentType.Check || row.DocType == APPaymentType.Prepayment)
				{
					string searchDocType = row.DocType == APPaymentType.Check ? APPaymentType.Prepayment : APPaymentType.Check;
					APPayment duplicatePayment = PXSelect<APPayment,
						Where<APPayment.docType, Equal<Required<APPayment.docType>>, And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>
						.Select(this, searchDocType, row.RefNbr);
					APInvoice inv = null;
					if (searchDocType == APPaymentType.Prepayment)
					{
						inv = PXSelect<APInvoice, Where<APInvoice.docType, Equal<APInvoiceType.prepayment>, And<APInvoice.refNbr, Equal<Required<APPayment.refNbr>>>>>.Select(this, row.RefNbr);
					}
					if (duplicatePayment != null && inv == null)
					{
						throw new PXRowPersistedException(typeof(APPayment.refNbr).Name, row.RefNbr, Messages.SameRefNbr, searchDocType == APPaymentType.Check ? Messages.Check : Messages.Prepayment, row.RefNbr);
					}
				}
			}
		}

		protected bool InternalCall = false;

		protected virtual void APPayment_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			APPayment doc = e.Row as APPayment;

			if (doc == null || InternalCall)
			{
				return;
			}

			if (vendor.Current != null && doc.VendorID != vendor.Current.BAccountID)
			{
				vendor.Current = null;
			}

			if (finperiod.Current != null && object.Equals(finperiod.Current.FinPeriodID, doc.AdjFinPeriodID) == false)
			{
				finperiod.Current = null;
			}
			bool docTypeNotDebitAdj = (doc.DocType != APDocType.DebitAdj);
			PXUIFieldAttribute.SetVisible<APPayment.curyID>(cache, doc, (bool)CMSetup.Current.MCActivated);
			PXUIFieldAttribute.SetVisible<APPayment.cashAccountID>(cache, doc, docTypeNotDebitAdj);
			PXUIFieldAttribute.SetVisible<APPayment.cleared>(cache, doc, docTypeNotDebitAdj);
			PXUIFieldAttribute.SetVisible<APPayment.clearDate>(cache, doc, docTypeNotDebitAdj);
			PXUIFieldAttribute.SetVisible<APPayment.paymentMethodID>(cache, doc, docTypeNotDebitAdj);
			PXUIFieldAttribute.SetVisible<APPayment.extRefNbr>(cache, doc, docTypeNotDebitAdj);
			bool ptInstanceVisible = false;

			if (docTypeNotDebitAdj)
			{
				PaymentTypeInstance ptInstance = PXSelect<PaymentTypeInstance, Where<PaymentTypeInstance.cashAccountID, Equal<Required<APPayment.cashAccountID>>,
					And<PaymentTypeInstance.paymentMethodID, Equal<Required<APPayment.paymentMethodID>>>>>.Select(this, doc.CashAccountID, doc.PaymentMethodID);
				ptInstanceVisible = (ptInstance != null);
			}
			PXUIFieldAttribute.SetVisible<APPayment.pTInstanceID>(cache, doc, ptInstanceVisible);
			//true for DebitAdj and Prepayment Requests
			bool docNotReleased = (doc.Released == false);
			bool docReleased = (doc.Released == true);
			bool docNotOnHold = (doc.Hold == false);
			bool docOnHold = (doc.Hold == true);
			bool curyenabled = false;
			bool clearEnabled = docOnHold && (cashaccount.Current != null) && (cashaccount.Current.Reconcile == true);
			bool HoldAdj = false;

			PXUIFieldAttribute.SetRequired<APPayment.cashAccountID>(cache, docNotReleased);
			PXUIFieldAttribute.SetRequired<APPayment.paymentMethodID>(cache, docNotReleased);
			PXUIFieldAttribute.SetRequired<APPayment.extRefNbr>(cache, docNotReleased);

			PaymentRefAttribute.SetUpdateCashManager<APPayment.extRefNbr>(cache, e.Row, doc.DocType != APDocType.VoidCheck && doc.DocType != APDocType.Refund);


			bool allowDeposit = (doc.DocType == APDocType.Refund);
			PXUIFieldAttribute.SetVisible<APPayment.depositAfter>(cache, doc, allowDeposit && (doc.DepositAsBatch == true));
			PXUIFieldAttribute.SetEnabled<APPayment.depositAfter>(cache, doc, false);
			PXUIFieldAttribute.SetRequired<APPayment.depositAfter>(cache, allowDeposit && (doc.DepositAsBatch == true));
			PXDefaultAttribute.SetPersistingCheck<APPayment.depositAfter>(cache, doc, allowDeposit && (doc.DepositAsBatch == true) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			//if (vendor.Current != null && !(bool)vendor.Current.AllowOverrideCury)
			//{
			//  curyenabled = false;
			//}
			this.validateAddresses.SetEnabled(false);
			if (cache.GetStatus(doc) == PXEntryStatus.Notchanged && doc.Status == APDocStatus.Open && doc.VoidAppl == false && doc.AdjDate != null && ((DateTime)doc.AdjDate).CompareTo((DateTime)Accessinfo.BusinessDate) < 0)
			{
				if (Adjustments_Raw.View.SelectSingleBound(new object[] { e.Row }) == null)
				{
					doc.AdjDate = Accessinfo.BusinessDate;
					doc.AdjTranPeriodID = GL.FinPeriodIDAttribute.GetPeriod((DateTime)doc.AdjDate);
					doc.AdjFinPeriodID = doc.AdjTranPeriodID;
					cache.SetStatus(doc, PXEntryStatus.Held);
				}
			}
			bool isReclassified = false;
			if (doc.DocType == APDocType.DebitAdj && cache.GetStatus(doc) == PXEntryStatus.Inserted)
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				cache.AllowUpdate = false;
				cache.AllowDelete = false;
				Adjustments.Cache.AllowDelete = false;
				Adjustments.Cache.AllowUpdate = false;
				Adjustments.Cache.AllowInsert = false;
				release.SetEnabled(false);
			}
			//else if ((bool) doc.VoidAppl && cache.GetStatus(doc) == PXEntryStatus.Inserted)
			else if ((bool)doc.VoidAppl && docNotReleased)
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<APPayment.adjDate>(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<APPayment.adjFinPeriodID>(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<APPayment.docDesc>(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<APPayment.hold>(cache, doc, true);
				cache.AllowUpdate = true;
				cache.AllowDelete = true;
				Adjustments.Cache.AllowDelete = false;
				Adjustments.Cache.AllowUpdate = false;
				Adjustments.Cache.AllowInsert = false;
				release.SetEnabled(docNotOnHold);
			}
			else if ((bool)doc.Released && (bool)doc.OpenDoc)
			{
				//these to cases do not intersect, no need to evaluate complete
			    foreach (APAdjust adj in Adjustments_Raw.Select())
				{
					if (adj.Voided == true)
					{
					    break;
					}

					if (adj.Hold == true)
					{
						HoldAdj = true;
						break;
					}
				}

				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<APPayment.adjDate>(cache, doc, (HoldAdj == false));
				PXUIFieldAttribute.SetEnabled<APPayment.adjFinPeriodID>(cache, doc, (HoldAdj == false));
				PXUIFieldAttribute.SetEnabled<APPayment.hold>(cache, doc, (HoldAdj == false));

				cache.AllowDelete = false;
				cache.AllowUpdate = (HoldAdj == false);
				Adjustments.Cache.AllowDelete = (HoldAdj == false);
				Adjustments.Cache.AllowInsert = (HoldAdj == false);
				Adjustments.Cache.AllowUpdate = (HoldAdj == false);

				release.SetEnabled(docNotOnHold && (HoldAdj == false));
			}
			else if ((bool)doc.Released && !(bool)doc.OpenDoc)
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				cache.AllowDelete = false;
				cache.AllowUpdate = false;
				Adjustments.Cache.AllowDelete = false;
				Adjustments.Cache.AllowUpdate = false;
				Adjustments.Cache.AllowInsert = false;
				release.SetEnabled(false);
			}
			else if ((bool)doc.VoidAppl)
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				cache.AllowDelete = false;
				cache.AllowUpdate = false;
				Adjustments.Cache.AllowDelete = false;
				Adjustments.Cache.AllowUpdate = false;
				Adjustments.Cache.AllowInsert = false;
				release.SetEnabled(docNotOnHold);
			}
			else
			{
				CATran tran = PXSelect<CATran, Where<CATran.tranID, Equal<Required<CATran.tranID>>>>.Select(this, doc.CATranID);
				isReclassified = (tran != null) && (tran.RefTranID.HasValue);
				PXUIFieldAttribute.SetEnabled(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<APPayment.status>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<APPayment.curyID>(cache, doc, curyenabled);
				PXUIFieldAttribute.SetEnabled<APPayment.printCheck>(cache, doc, (doc.DocType != APDocType.Prepayment && doc.DocType != APDocType.Refund));


			    bool mustPrintCheck = MustPrintCheck(doc);
                PXUIFieldAttribute.SetEnabled<APPayment.hold>(cache, doc, !mustPrintCheck);
                PXUIFieldAttribute.SetEnabled<APPayment.extRefNbr>(cache, doc, !mustPrintCheck && !isReclassified);
			    cache.RaiseExceptionHandling<APPayment.hold>(doc, null,
			                                                 mustPrintCheck
			                                                     ? new PXSetPropertyException(
			                                                           Messages.Check_Cannot_Unhold_Until_Printed,
			                                                           PXErrorLevel.Warning)
			                                                     : null);

			    cache.AllowDelete = true;
				cache.AllowUpdate = true;
				Adjustments.Cache.AllowDelete = true;
				Adjustments.Cache.AllowUpdate = true;
				Adjustments.Cache.AllowInsert = true;
				release.SetEnabled(docNotOnHold);
				PXUIFieldAttribute.SetEnabled<APPayment.curyOrigDocAmt>(cache, doc, !isReclassified);
				PXUIFieldAttribute.SetEnabled<APPayment.vendorID>(cache, doc, !isReclassified);
			}

			bool prepaymentNotReleased = docNotReleased && doc.DocType == APDocType.Prepayment;
			APAddress address = this.Remittance_Address.Select();
			bool enableAddressValidation = docNotReleased && (address != null && address.IsDefaultAddress == false && address.IsValidated == false);
			this.validateAddresses.SetEnabled(enableAddressValidation);
			PXUIFieldAttribute.SetEnabled<APPayment.cashAccountID>(cache, doc, docNotReleased && doc.VoidAppl == false && !isReclassified);
			PXUIFieldAttribute.SetEnabled<APPayment.paymentMethodID>(cache, doc, docNotReleased && doc.VoidAppl == false && !isReclassified);
			PXUIFieldAttribute.SetEnabled<APPayment.pTInstanceID>(cache, doc, docNotReleased && doc.VoidAppl == false && !isReclassified);
			PXUIFieldAttribute.SetEnabled<APPayment.cleared>(cache, doc, clearEnabled);
			PXUIFieldAttribute.SetEnabled<APPayment.clearDate>(cache, doc, clearEnabled && doc.Cleared == true);
			PXUIFieldAttribute.SetEnabled<APPayment.docType>(cache, doc);
			PXUIFieldAttribute.SetEnabled<APPayment.refNbr>(cache, doc);
			PXUIFieldAttribute.SetEnabled<APPayment.curyUnappliedBal>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<APPayment.curyApplAmt>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<APPayment.batchNbr>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<APPayment.aPAccountID>(cache, doc, prepaymentNotReleased);
			PXUIFieldAttribute.SetEnabled<APPayment.aPSubID>(cache, doc, prepaymentNotReleased);

			voidCheck.SetEnabled(docReleased && doc.Voided == false && (doc.DocType == APDocType.Check || doc.DocType == APDocType.Prepayment) && (HoldAdj == false));
			loadInvoices.SetEnabled(doc.VendorID != null && (bool)doc.OpenDoc && (HoldAdj == false) && (doc.DocType == APDocType.Check 
				|| doc.DocType == APDocType.Prepayment || doc.DocType == APDocType.Refund));

			SetDocTypeList(e.Row);

			editVendor.SetEnabled(vendor != null && vendor.Current != null);
			if (doc.VendorID != null)
			{
				if (Adjustments_Raw.View.SelectSingleBound(new object[] { e.Row }) != null)
				{
					PXUIFieldAttribute.SetEnabled<APPayment.vendorID>(cache, doc, false);
				}
			}

			if (e.Row != null && ((APPayment)e.Row).CuryApplAmt == null)
			{
				bool IsReadOnly = (cache.GetStatus(e.Row) == PXEntryStatus.Notchanged);
				PXFormulaAttribute.CalcAggregate<APAdjust.curyAdjgAmt>(Adjustments.Cache, e.Row, IsReadOnly);
				cache.RaiseFieldUpdated<APPayment.curyApplAmt>(e.Row, null);

				PXDBCurrencyAttribute.CalcBaseValues<APPayment.curyApplAmt>(cache, e.Row);
				PXDBCurrencyAttribute.CalcBaseValues<APPayment.curyUnappliedBal>(cache, e.Row);
			}

			PXUIFieldAttribute.SetEnabled<APPayment.depositDate>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<APPayment.depositAsBatch>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<APPayment.deposited>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<APPayment.depositNbr>(cache, null, false);

			bool isDeposited = (string.IsNullOrEmpty(doc.DepositNbr) == false && string.IsNullOrEmpty(doc.DepositType) == false);
			CashAccount cashAccount = this.cashaccount.Current;
			bool isClearingAccount = (cashAccount != null && cashAccount.CashAccountID == doc.CashAccountID && cashAccount.ClearingAccount == true);
			bool enableDepositEdit = !isDeposited && cashAccount != null && (isClearingAccount || doc.DepositAsBatch != isClearingAccount);
			if (enableDepositEdit)
			{
				if (doc.DepositAsBatch != isClearingAccount)
					cache.RaiseExceptionHandling<APPayment.depositAsBatch>(doc, doc.DepositAsBatch, new PXSetPropertyException(AR.Messages.DocsDepositAsBatchSettingDoesNotMatchClearingAccountFlag, PXErrorLevel.Warning));
				else
					cache.RaiseExceptionHandling<APPayment.depositAsBatch>(doc, doc.DepositAsBatch, null);
			}
			PXUIFieldAttribute.SetEnabled<APPayment.depositAsBatch>(cache, doc, enableDepositEdit);
			PXUIFieldAttribute.SetEnabled<APPayment.depositAfter>(cache, doc, ((!isDeposited) && isClearingAccount && doc.DepositAsBatch == true));

			this.PaymentCharges.Cache.AllowInsert = ((doc.DocType == APDocType.Check || doc.DocType == APDocType.VoidCheck) && doc.Released != true);
			this.PaymentCharges.Cache.AllowUpdate = ((doc.DocType == APDocType.Check || doc.DocType == APDocType.VoidCheck) && doc.Released != true);
			this.PaymentCharges.Cache.AllowDelete = ((doc.DocType == APDocType.Check || doc.DocType == APDocType.VoidCheck) && doc.Released != true);
		}

		public virtual void APPayment_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			APPayment row = (APPayment)e.Row;
			if (row != null && row.CuryApplAmt == null)
			{
				using (new PXConnectionScope())
				{
					RecalcApplAmounts(sender, row,true);
				}
			}
		}

		public virtual void RecalcApplAmounts(PXCache sender, APPayment row, bool aReadOnly) 
		{
			internalCall = true;
			try
			{
				PXFormulaAttribute.CalcAggregate<APAdjust.curyAdjgAmt>(Adjustments.Cache, row, aReadOnly);
				PXFormulaAttribute.CalcAggregate<APPaymentChargeTran.curyTranAmt>(PaymentCharges.Cache, row, aReadOnly);
			}
			finally
			{
				internalCall = false;
			}
			sender.RaiseFieldUpdated<APPayment.curyApplAmt>(row, null);
			PXDBCurrencyAttribute.CalcBaseValues<APPayment.curyApplAmt>(sender, row);
			PXDBCurrencyAttribute.CalcBaseValues<APPayment.curyUnappliedBal>(sender, row);
		}

		public static void SetDocTypeList(PXCache cache,string docType)
		{
			List<string> AllowedValues = new List<string>();
			List<string> AllowedLabels = new List<string>();

			if (docType == APDocType.Refund)
			{
				PXDefaultAttribute.SetDefault<APAdjust.adjdDocType>(cache, APDocType.DebitAdj);
				PXStringListAttribute.SetList<APAdjust.adjdDocType>(cache, null, new string[] { APDocType.DebitAdj, APDocType.Prepayment }, new string[] { Messages.DebitAdj, Messages.Prepayment });
			}
			else if (docType == APDocType.Prepayment)
			{
				PXDefaultAttribute.SetDefault<APAdjust.adjdDocType>(cache, APDocType.Invoice);
				PXStringListAttribute.SetList<APAdjust.adjdDocType>(cache, null, new string[] { APDocType.Invoice, APDocType.CreditAdj }, new string[] { Messages.Invoice, Messages.CreditAdj });
			}
			else if (docType == APDocType.Check || docType == APDocType.VoidCheck)
			{
				PXDefaultAttribute.SetDefault<APAdjust.adjdDocType>(cache, APDocType.Invoice);
				PXStringListAttribute.SetList<APAdjust.adjdDocType>(cache, null, new string[] { APDocType.Invoice, APDocType.DebitAdj, APDocType.CreditAdj, APDocType.Prepayment }, new string[] { Messages.Invoice, Messages.DebitAdj, Messages.CreditAdj, Messages.Prepayment });
			}
			else
			{
				PXDefaultAttribute.SetDefault<APAdjust.adjdDocType>(cache, APDocType.Invoice);
				PXStringListAttribute.SetList<APAdjust.adjdDocType>(cache, null, new string[] { APDocType.Invoice, APDocType.CreditAdj, APDocType.Prepayment }, new string[] { Messages.Invoice, Messages.CreditAdj, Messages.Prepayment });
			}
		}

        private void SetDocTypeList(object Row) 
        {
            APPayment row = Row as APPayment;
            if (row != null)
            {                
                SetDocTypeList(Adjustments.Cache, row.DocType);
            }
        }
		protected virtual void APPayment_Cleared_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APPayment payment = (APPayment)e.Row;
			if (payment.Cleared == true)
			{
				if (payment.ClearDate == null)
				{
					payment.ClearDate = payment.DocDate;
				}
			}
			else
			{
				payment.ClearDate = null;
			}
		}

		protected virtual void APPayment_Hold_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
            PXBoolAttribute.ConvertValue(e);
			if (e.Row != null && e.NewValue != null)
			{
				if ((bool)e.NewValue && ((APPayment)e.Row).Status == "B")
				{
					sender.SetValue<APPayment.status>(e.Row, "H");
				}
				else if (!(bool)e.NewValue && ((APPayment)e.Row).Status == "H")
				{
					sender.SetValue<APPayment.status>(e.Row, "B");
				}
			}
		}

		protected virtual void APPayment_DocDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (((APPayment)e.Row).Released == false && ((APPayment)e.Row).VoidAppl == false)
			{
				e.NewValue = ((APPayment)e.Row).AdjDate;
				e.Cancel = true;
			}
		}

		protected virtual void APPayment_FinPeriodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if ((bool)((APPayment)e.Row).Released == false)
			{
				e.NewValue = ((APPayment)e.Row).AdjFinPeriodID;
				e.Cancel = true;
			}
		}

		protected virtual void APPayment_TranPeriodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if ((bool)((APPayment)e.Row).Released == false)
			{
				e.NewValue = ((APPayment)e.Row).AdjTranPeriodID;
				e.Cancel = true;
			}
		}

		protected virtual void APPayment_DepositAfter_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APPayment row = (APPayment)e.Row;
			if ((row.DocType == APDocType.Refund)
				&& row.DepositAsBatch == true)
			{
				e.NewValue = row.AdjDate;
				e.Cancel = true;
			}
		}

		protected virtual void APPayment_DepositAsBatch_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APPayment row = (APPayment)e.Row;
			if ((row.DocType == APDocType.Refund))
			{
				sender.SetDefaultExt<APPayment.depositAfter>(e.Row);
			}
		}
		

		protected virtual void APPayment_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (((APPayment)e.Row).Released == false)
			{
				//for Void application cannot change DocDate not to screw up DBDefault for AdjgDocDate 
				//which will cause incorrect voucher balance calculation
				if (((APPayment)e.Row).VoidAppl == false)
				{
					((APPayment)e.Row).DocDate = ((APPayment)e.Row).AdjDate;
				}
				((APPayment)e.Row).FinPeriodID = ((APPayment)e.Row).AdjFinPeriodID;
				((APPayment)e.Row).TranPeriodID = ((APPayment)e.Row).AdjTranPeriodID;

				sender.RaiseExceptionHandling<APPayment.finPeriodID>(e.Row, ((APPayment)e.Row).FinPeriodID, null);
			}
		}

		protected virtual void APPayment_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			APPayment row = (APPayment)e.Row;

			if (row.Released != true)
			{
				//for Void application cannot change DocDate not to screw up DBDefault for AdjgDocDate 
				//which will cause incorrect voucher balance calculation

				if (row.VoidAppl != true)
				{
					row.DocDate = row.AdjDate;
				}

				row.FinPeriodID  = row.AdjFinPeriodID;
				row.TranPeriodID = row.AdjTranPeriodID;

				sender.RaiseExceptionHandling<APPayment.finPeriodID>(row, row.FinPeriodID, null);
				if (row.PTInstanceID != null)
				{
					PTInstTran tran = this.ptInstanceTrans.Select();
					if (tran == null)
					{
						tran = new PTInstTran();
						tran = this.ptInstanceTrans.Insert(tran);
					}
				}
				else
				{
					foreach (PTInstTran it in this.ptInstanceTrans.Select())
					{
						this.ptInstanceTrans.Delete(it);
					}
				}
			}

			if (row.OpenDoc == true && row.Hold != true)
			{
                if (row.CanHaveBalance == true && row.VoidAppl != true && (row.CuryUnappliedBal < 0m || row.CuryApplAmt < 0m && row.CuryUnappliedBal > row.CuryOrigDocAmt || row.CuryOrigDocAmt < 0m) ||
                    row.CanHaveBalance != true && row.CuryUnappliedBal != 0m)
				{
					sender.RaiseExceptionHandling<APPayment.curyOrigDocAmt>(row, row.CuryOrigDocAmt, new PXSetPropertyException(Messages.DocumentOutOfBalance));
				}
				else
				{
					sender.RaiseExceptionHandling<APPayment.curyOrigDocAmt>(row, row.CuryOrigDocAmt, null);
				}
			}
		}

        public virtual void APPaymentChargeTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            APPaymentChargeTran charge = (APPaymentChargeTran)e.Row;
            if (charge.CuryTranAmt <= 0m && charge.DocType != APDocType.VoidCheck)
            {
                sender.RaiseExceptionHandling<APPaymentChargeTran.curyTranAmt>(e.Row, charge.CuryTranAmt, new PXSetPropertyException(CS.Messages.Entry_GT, "0"));
            }
            if (charge.AccountID == null)
            {
                sender.RaiseExceptionHandling<APPaymentChargeTran.accountID>(e.Row, charge.AccountID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APPaymentChargeTran.accountID).Name));
            }
            if (charge.SubID == null)
            {
                sender.RaiseExceptionHandling<APPaymentChargeTran.subID>(e.Row, charge.SubID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APPaymentChargeTran.subID).Name));
            }

        }


		#region BusinessProcs
		
		public virtual void CreatePayment(APAdjust apdoc, CurrencyInfo info)
		{
			Segregate(apdoc, info);

			APAdjust adj = new APAdjust();
			adj.AdjdDocType = apdoc.AdjdDocType;
			adj.AdjdRefNbr = apdoc.AdjdRefNbr;

			//set origamt to zero to apply "full" amounts to invoices.
			this.Document.Cache.SetValueExt<APPayment.curyOrigDocAmt>(this.Document.Current, 0m);

			adj = PXCache<APAdjust>.CreateCopy(this.Adjustments.Insert(adj));

			if (TakeDiscAlways == true)
			{
				adj.CuryAdjgAmt = 0m;
				adj.CuryAdjgDiscAmt = 0m;
				adj.CuryAdjgWhTaxAmt = 0m;

				CalcBalances((object)adj, true);
				adj = PXCache<APAdjust>.CreateCopy((APAdjust)this.Adjustments.Cache.Update(adj));
			}

			if (apdoc.CuryAdjgDiscAmt != null)
			{
				adj.CuryAdjgDiscAmt = apdoc.CuryAdjgDiscAmt;
				adj = PXCache<APAdjust>.CreateCopy((APAdjust)this.Adjustments.Cache.Update(adj));
			}

			if (apdoc.CuryAdjgAmt != null)
			{
				adj.CuryAdjgAmt = apdoc.CuryAdjgAmt;
                adj = PXCache<APAdjust>.CreateCopy((APAdjust)this.Adjustments.Cache.Update(adj));
			}

            if (Document.Current.CuryApplAmt < 0m)
            {
				if (adj.CuryAdjgAmt <= -Document.Current.CuryApplAmt)
				{
					Adjustments.Delete(adj);
				}
				else
				{
					adj.CuryAdjgAmt += Document.Current.CuryApplAmt;
					Adjustments.Update(adj);
				}
            }

			decimal? CuryApplAmt = Document.Current.CuryApplAmt;

			this.Document.Cache.SetValueExt<APPayment.curyOrigDocAmt>(this.Document.Current, CuryApplAmt);
			this.Document.Cache.Update(this.Document.Current);
			this.Save.Press();

			apdoc.AdjgDocType = this.Document.Current.DocType;
			apdoc.AdjgRefNbr = this.Document.Current.RefNbr;
		}

		public virtual void CreatePayment(APInvoice apdoc)
		{
			APPayment payment = this.Document.Current;

			if (apdoc.PayLocationID == null)
			{
				apdoc.PayLocationID = apdoc.VendorLocationID;
			}

			if (payment == null ||
				!object.Equals(payment.VendorID, apdoc.VendorID) ||
				!object.Equals(payment.VendorLocationID, apdoc.PayLocationID) ||
				(bool)apdoc.SeparateCheck)
			{
				this.Clear();

				Location vend = PXSelect<Location, Where<Location.bAccountID, Equal<Required<Location.bAccountID>>, And<Location.locationID, Equal<Required<Location.locationID>>>>>.Select(this, apdoc.VendorID, apdoc.PayLocationID);
				if (vend == null)
				{
					throw new PXException(Messages.InternalError, 502);
				}

				if (apdoc.PayAccountID == null || apdoc.PayTypeID == null || apdoc.PayDate == null)
				{
					apdoc.PayTypeID = vend.PaymentMethodID;
					apdoc.PayAccountID = vend.CashAccountID;
					apdoc.PayDate = this.Accessinfo.BusinessDate;
				}

				CashAccount ca = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, apdoc.PayAccountID);

				if (ca == null)
				{
					throw new PXException(Messages.VendorMissingCashAccount);
				}

				payment = new APPayment();
				switch (apdoc.DocType)
				{
					case APDocType.DebitAdj:
						payment.DocType = APDocType.Refund;
						break;
					default:
						payment.DocType = APDocType.Check;
						break;
				}
				payment = PXCache<APPayment>.CreateCopy(this.Document.Insert(payment));

				payment.VendorID = apdoc.VendorID;
				payment.VendorLocationID = apdoc.PayLocationID;
				payment.AdjDate = (DateTime.Compare((DateTime)this.Accessinfo.BusinessDate, (DateTime)apdoc.DocDate) < 0 ? apdoc.DocDate : this.Accessinfo.BusinessDate);
				payment.CashAccountID = apdoc.PayAccountID;
				payment.CuryID = ca.CuryID;
				payment.PaymentMethodID = apdoc.PayTypeID;
				payment.DocDesc = apdoc.DocDesc;

				payment = this.Document.Update(payment);
			}

			APAdjust adj = new APAdjust();
			adj.AdjdDocType = apdoc.DocType;
			adj.AdjdRefNbr = apdoc.RefNbr;

			//set origamt to zero to apply "full" amounts to invoices.
			this.Document.SetValueExt<APPayment.curyOrigDocAmt>(payment, 0m);

			try
			{
				this.Adjustments.Insert(adj);
			}
			catch (PXSetPropertyException)
			{
				throw new AdjustedNotFoundException();
			}

			decimal? CuryApplAmt = payment.CuryApplAmt;

			this.Document.SetValueExt<APPayment.curyOrigDocAmt>(payment, CuryApplAmt);
			this.Document.Current = this.Document.Update(payment);
		}

		private bool _IsVoidCheckInProgress = false;

		protected virtual void APPayment_RefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_IsVoidCheckInProgress)
			{
				e.Cancel = true;
			}
		}

		protected virtual void APPayment_FinPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_IsVoidCheckInProgress)
			{
				e.Cancel = true;
			}
		}

		protected virtual void APPayment_AdjFinPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_IsVoidCheckInProgress)
			{
				e.Cancel = true;
			}

			APPayment doc = (APPayment)e.Row;

			if (doc.Released == true && doc.FinPeriodID.CompareTo((string)e.NewValue) > 0)
			{
				e.NewValue = FinPeriodIDAttribute.FormatForDisplay((string)e.NewValue);
				throw new PXSetPropertyException(CS.Messages.Entry_GE, FinPeriodIDAttribute.FormatForError(doc.FinPeriodID));
			}

			if (doc.DocType == APDocType.VoidCheck)
			{
				APPayment orig_payment = PXSelect<APPayment, Where<APPayment.docType, Equal<APDocType.check>, And<APPayment.refNbr, Equal<Current<APPayment.refNbr>>>>>.SelectSingleBound(this, new object[] { e.Row });

				if (orig_payment != null && orig_payment.FinPeriodID.CompareTo((string)e.NewValue) > 0)
				{
					e.NewValue = FinPeriodIDAttribute.FormatForDisplay((string)e.NewValue);
					throw new PXSetPropertyException(CS.Messages.Entry_GE, FinPeriodIDAttribute.FormatForError(orig_payment.FinPeriodID));
				}
			}

			try
			{
				internalCall = true;
				foreach (APAdjust adjmax in PXSelect<APAdjust, Where<APAdjust.adjgDocType, Equal<Required<APAdjust.adjgDocType>>, And<APAdjust.adjgRefNbr, Equal<Required<APAdjust.adjgRefNbr>>, And<APAdjust.released, Equal<Required<APAdjust.released>>>>>, OrderBy<Desc<APAdjust.adjgFinPeriodID>>>.SelectWindowed(this, 0, 1, doc.DocType, doc.RefNbr, doc.DocType == APDocType.VoidCheck ? false : true))
				{
					if (adjmax.AdjgFinPeriodID.CompareTo((string)e.NewValue) > 0)
					{
						e.NewValue = FinPeriodIDAttribute.FormatForDisplay((string)e.NewValue);
						throw new PXSetPropertyException(CS.Messages.Entry_GE, FinPeriodIDAttribute.FormatForError(adjmax.AdjgFinPeriodID));
					}
				}
			}
			finally
			{
				internalCall = false;
			}
		}

		public virtual void VoidCheckProc(APPayment doc)
		{
			this.Clear(PXClearOption.PreserveTimeStamp);
			this.Document.View.Answer = WebDialogResult.No;

			foreach (PXResult<APPayment, CurrencyInfo, Currency, Vendor> res in APPayment_CurrencyInfo_Currency_Vendor.Select(this, (object)doc.DocType, doc.RefNbr, doc.VendorID))
			{
				Vendor vendor = (Vendor)res;

				CurrencyInfo info = PXCache<CurrencyInfo>.CreateCopy((CurrencyInfo)res);
				info.CuryInfoID = null;
				info.IsReadOnly = false;
				info = PXCache<CurrencyInfo>.CreateCopy(this.currencyinfo.Insert(info));

				APPayment payment = new APPayment();
				payment.DocType = ((APPayment)res).DocType;
				payment.RefNbr = ((APPayment)res).RefNbr;
				payment.CuryInfoID = info.CuryInfoID;
				payment.VoidAppl = true;

				Document.Insert(payment);

				payment = PXCache<APPayment>.CreateCopy((APPayment)res);

				payment.CuryInfoID = info.CuryInfoID;
				payment.VoidAppl = true;
				//must set for _RowSelected
				payment.CATranID = null;
				payment.Printed = true;
				payment.OpenDoc = true;
				payment.Released = false;
				payment.Hold = false;
				payment.LineCntr = 0;
				payment.BatchNbr = null;
				payment.CuryOrigDocAmt = -1 * payment.CuryOrigDocAmt;
				payment.OrigDocAmt = -1 * payment.OrigDocAmt;
				payment.CuryChargeAmt = 0;
				payment.CuryApplAmt = null;
				payment.CuryUnappliedBal = null;
				payment.AdjDate = doc.DocDate;
				payment.AdjFinPeriodID = doc.AdjFinPeriodID;
				payment.Cleared = true;
				payment.ClearDate = payment.DocDate;

				payment = Document.Update(payment);

				//this.Document.Cache.SetValueExt<APPayment.adjDate>(payment, doc.AdjDate);

				if (info != null)
				{
					CurrencyInfo b_info = (CurrencyInfo)PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APPayment.curyInfoID>>>>.Select(this, null);
					b_info.CuryID = info.CuryID;
					b_info.CuryEffDate = info.CuryEffDate;
					b_info.CuryRateTypeID = info.CuryRateTypeID;
					b_info.CuryRate = info.CuryRate;
					b_info.RecipRate = info.RecipRate;
					b_info.CuryMultDiv = info.CuryMultDiv;
					this.currencyinfo.Update(b_info);
				}
			}

			foreach (PXResult<APAdjust, CurrencyInfo> adjres in PXSelectJoin<APAdjust, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APAdjust.adjdCuryInfoID>>>, Where<APAdjust.adjgDocType, Equal<Required<APAdjust.adjgDocType>>, And<APAdjust.adjgRefNbr, Equal<Required<APAdjust.adjgRefNbr>>, And<APAdjust.voided, Equal<boolFalse>>>>>.Select(this, doc.DocType, doc.RefNbr))
			{
				APAdjust adj = PXCache<APAdjust>.CreateCopy((APAdjust)adjres);

				adj.VoidAppl = true;
				adj.Released = false;
				adj.VoidAdjNbr = adj.AdjNbr;
				adj.AdjNbr = 0;
				adj.AdjBatchNbr = null;

				APAdjust adjnew = new APAdjust();
				adjnew.AdjgDocType = adj.AdjgDocType;
				adjnew.AdjgRefNbr = adj.AdjgRefNbr;
				adjnew.AdjgBranchID = adj.AdjgBranchID;
				adjnew.AdjdDocType = adj.AdjdDocType;
				adjnew.AdjdRefNbr = adj.AdjdRefNbr;
				adjnew.AdjdBranchID = adj.AdjdBranchID;
				adjnew.VendorID = adj.VendorID;
				adjnew.AdjdCuryInfoID = adj.AdjdCuryInfoID;

				if (this.Adjustments.Insert(adjnew) == null)
				{
					adj = (APAdjust)adjres;
					this.Clear();
					adj = (APAdjust)Adjustments.Cache.Update(adj);
					Document.Current = Document.Search<APPayment.refNbr>(adj.AdjgRefNbr, adj.AdjgDocType);
					Adjustments.Cache.RaiseExceptionHandling<APAdjust.adjdRefNbr>(adj, adj.AdjdRefNbr, new PXSetPropertyException(Messages.MultipleApplicationError, PXErrorLevel.RowError));

					throw new PXException(Messages.MultipleApplicationError);
				}

				adj.CuryAdjgAmt = -1 * adj.CuryAdjgAmt;
				adj.CuryAdjgDiscAmt = -1 * adj.CuryAdjgDiscAmt;
				adj.CuryAdjgWhTaxAmt = -1 * adj.CuryAdjgWhTaxAmt;
				adj.AdjAmt = -1 * adj.AdjAmt;
				adj.AdjDiscAmt = -1 * adj.AdjDiscAmt;
				adj.AdjWhTaxAmt = -1 * adj.AdjWhTaxAmt;
				adj.CuryAdjdAmt = -1 * adj.CuryAdjdAmt;
				adj.CuryAdjdDiscAmt = -1 * adj.CuryAdjdDiscAmt;
				adj.CuryAdjdWhTaxAmt = -1 * adj.CuryAdjdWhTaxAmt;
				adj.RGOLAmt = -1 * adj.RGOLAmt;
				adj.AdjgCuryInfoID = Document.Current.CuryInfoID;

				Adjustments.Update(adj);
			}

			foreach (PXResult<APPaymentChargeTran> paycharge in PXSelect<APPaymentChargeTran, Where<APPaymentChargeTran.docType, Equal<Required<APPayment.docType>>, And<APPaymentChargeTran.refNbr, Equal<Required<APPayment.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
			{
				APPaymentChargeTran charge = PXCache<APPaymentChargeTran>.CreateCopy((APPaymentChargeTran)paycharge);
				charge.DocType = Document.Current.DocType;
				charge.CuryTranAmt = -1 * charge.CuryTranAmt;
				charge.Released = false;
				charge.CuryInfoID = Document.Current.CuryInfoID;
				PaymentCharges.Insert(charge);
			}
		}
		#endregion
	}
}
