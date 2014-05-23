using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Api;

namespace PX.Objects.CA
{
	public class CABatchEntry : PXGraph<CABatchEntry, CABatch>
	{
		#region Internal defintions
#if false
		public partial class PaymentFilter : IBqlTable
		{
			#region PaymentMethodID
			public abstract class paymentMethodID : PX.Data.IBqlField
			{
			}
			protected String _PaymentMethodID;
			[PXDBString(10)]
			[PXSelector(typeof(Search<PaymentMethod.paymentMethodID>))]
			[PXUIField(DisplayName = "Payment Method", Enabled = true)]
			public virtual String PaymentMethodID
			{
				get
				{
					return this._PaymentMethodID;
				}
				set
				{
					this._PaymentMethodID = value;
				}
			}
			#endregion
			#region CashAccountID
			public abstract class cashAccountID : PX.Data.IBqlField
			{
			}
			protected Int32? _CashAccountID;

			[CashAccount(null, typeof(Search2<CashAccount.cashAccountID,
										InnerJoin<CashAccountDeposit, On<CashAccountDeposit.depositAcctID, Equal<CashAccount.cashAccountID>>>,
										Where<CashAccountDeposit.accountID, Equal<Current<CABatch.cashAccountID>>,
										And<Match<Current<AccessInfo.userName>>>>>), DisplayName = "Clearing Account", DescriptionField = typeof(CashAccount.descr))]
			public virtual Int32? CashAccountID
			{
				get
				{
					return this._CashAccountID;
				}
				set
				{
					this._CashAccountID = value;
				}
			}
			#endregion
		}
#endif
		public static class ExportProviderParams
		{
			public const string FileName ="FileName";
			public const string BatchNbr ="BatchNbr";
			public const string BatchSequenceStartingNbr = "BatchStartNumber";
		}

		#endregion
        
		#region Buttons
		public PXAction<CABatch> Release;
		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable release(PXAdapter adapter)
		{
			if (PXLongOperation.Exists(UID))
			{
				throw new ApplicationException(GL.Messages.PrevOperationNotCompleteYet);
			}
            Save.Press();
			CABatch doc = this.Document.Current;
			if (doc.Released == false && doc.Hold == false) 
			{
				PXLongOperation.StartOperation(this, delegate() { CABatchEntry.ReleaseDoc(doc);});
			}		
            
            return adapter.Get();
		}

		public PXAction<CABatch> Export;
		[PXUIField(DisplayName = Messages.Export, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable export(PXAdapter adapter)
		{
			if (PXLongOperation.Exists(UID))
			{
				throw new ApplicationException(GL.Messages.PrevOperationNotCompleteYet);
			}
			CABatch doc = this.Document.Current;			
			if (doc!=null && doc.Released == true && doc.Hold == false)
			{
				PXResult<PaymentMethod,SYMapping> res =(PXResult<PaymentMethod,SYMapping>) PXSelectJoin<PaymentMethod, 
									LeftJoin<SYMapping,On<SYMapping.mappingID,Equal<PaymentMethod.aPBatchExportSYMappingID>>>,
										Where<PaymentMethod.paymentMethodID, Equal<Optional<CABatch.paymentMethodID>>>>.Select(this,doc.PaymentMethodID);
				PaymentMethod pt = res;
				SYMapping map =res;
				if (pt != null && pt.APCreateBatchPayment == true && pt.APBatchExportSYMappingID != null && map!=null)
				{
					string defaultFileName = this.GenerateFileName(doc);
					PXLongOperation.StartOperation(this, delegate()
					{

						PX.Api.SYExportProcess.RunScenario(map.Name, SYMapping.RepeatingOption.All,true,true,
							new PX.Api.PXSYParameter(ExportProviderParams.FileName, defaultFileName),
							new PX.Api.PXSYParameter(ExportProviderParams.BatchNbr, doc.BatchNbr)
                            
							//,
							//new PX.Api.PXSYParameter(ExportProviderParams.BatchSequenceStartingNbr, "0000")
							);
					});
				}
				else
				{
					throw new PXException(Messages.CABatchExportProviderIsNotConfigured);				
				}
			}

			return adapter.Get();
		}


#if false
        public PXAction<CABatch> addExtPayment;
        [PXUIField(DisplayName = Messages.AddARPayment, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
        [PXLookupButton]
        public virtual IEnumerable AddExtPayment(PXAdapter adapter)
        {
            if (this.Document.Current != null &&
                  this.Document.Current.Released != true)
            {

                if (this.AvailablePayments.AskExt() == WebDialogResult.OK)
                {
                    this.AvailablePayments.Cache.AllowInsert = true;
                    foreach (APPayment it in this.AvailablePayments.Cache.Updated)
                    {
                        APPayment pm = (APPayment)it;
                        if ((bool)pm.Selected)
                        {
                            this.AddPayment(pm, false);
                            pm.Selected = false;
                        }
                    }
                }
                else
                {
                    foreach (APPayment it in this.AvailablePayments.Cache.Updated)
                        it.Selected = false;
                }
                this.AvailablePayments.Cache.AllowInsert = false;
            }
            return adapter.Get();
        }
        
#endif


		public PXAction<CABatch> viewAPDocument;
		[PXUIField(DisplayName = PO.Messages.ViewAPDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable ViewAPDocument(PXAdapter adapter)
		{
			CABatchDetail doc = this.BatchPayments.Current;
			if (doc != null)
			{
				APRegister apDoc = PXSelect<APRegister, 
								Where<APRegister.docType, Equal<Required<APRegister.docType>>,
								And<APRegister.refNbr, Equal<Required<APRegister.refNbr>>>>>.Select(this, doc.OrigDocType,doc.OrigRefNbr);
				if (apDoc != null)
				{
					if (apDoc.DocType == APDocType.Check)
					{
						APPaymentEntry apGraph = PXGraph.CreateInstance<APPaymentEntry>();
						apGraph.Document.Current = apGraph.Document.Search<APRegister.refNbr>(apDoc.RefNbr, apDoc.DocType);
						if (apGraph.Document.Current != null)
							throw new PXRedirectRequiredException(apGraph, true, ""){ Mode = PXBaseRedirectException.WindowMode.NewWindow};
					}
					else if (apDoc.DocType == APDocType.QuickCheck)
					{
						APQuickCheckEntry apGraph = PXGraph.CreateInstance<APQuickCheckEntry>();
						apGraph.Document.Current = apGraph.Document.Search<APRegister.refNbr>(apDoc.RefNbr, apDoc.DocType);
						if (apGraph.Document.Current != null)
							throw new PXRedirectRequiredException(apGraph, true, "") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
					}					
				}
			}
			return adapter.Get();
		}
	

	    #endregion

        #region Ctor + Selects
		
		public CABatchEntry()
		{
			CASetup setup = casetup.Current;			
			RowUpdated.AddHandler<CABatch>(ParentFieldUpdated);
            this.APPaymentApplications.Cache.AllowInsert = false;
            this.APPaymentApplications.Cache.AllowUpdate = false;
            this.APPaymentApplications.Cache.AllowDelete = false;

		}
        
		public PXSelect<CABatch> Document; 	
		
		public PXSelect<CABatchDetail, Where<CABatchDetail.batchNbr, Equal<Current<CABatch.batchNbr>>>> Details;
		public PXSelectJoin<CABatchDetail,
							LeftJoin<APPayment, On<CABatchDetail.origDocType, Equal<APPayment.docType>,
							And<CABatchDetail.origRefNbr, Equal<APPayment.refNbr>>>>,
							Where<CABatchDetail.batchNbr, Equal<Current<CABatch.batchNbr>>>> BatchPayments;

		public PXSelectJoin<APPayment,
							InnerJoin<CABatchDetail,On<CABatchDetail.origDocType, Equal<APPayment.docType>,
							And<CABatchDetail.origRefNbr, Equal<APPayment.refNbr>,
							And<CABatchDetail.origModule,Equal<GL.BatchModule.moduleAP>>>>>,
							Where<CABatchDetail.batchNbr, Equal<Current<CABatch.batchNbr>>>> APPaymentList;

        public PXSelectJoin<Address, InnerJoin<Location,On<Location.remitAddressID,Equal<Address.addressID>>>,
                                    Where<Location.bAccountID,Equal<Current<APPayment.vendorID>>,
                                       And<Location.locationID, Equal<Current<APPayment.vendorLocationID>>>>> VendorRemitAddress;

        public PXSelectJoin<Contact, InnerJoin<Location, On<Location.remitContactID, Equal<Contact.contactID>>>,                                    
                                    Where<Location.bAccountID, Equal<Current<APPayment.vendorID>>,
                                       And<Location.locationID, Equal<Current<APPayment.vendorLocationID>>>>> VendorRemitContact;
                    
        

        //public PXSelectJoin<APPayment, LeftJoin<CABatchDetail,On<CABatchDetail.origDocType, Equal<APPayment.docType>,
        //                                    And<CABatchDetail.origRefNbr, Equal<APPayment.refNbr>>>>,
        //                        Where<APPayment.cashAccountID, Equal<Current<CABatch.cashAccountID>>,
        //                        And<APPayment.paymentMethodID, Equal<Current<CABatch.paymentMethodID>>,
        //                        And<APPayment.released,Equal<boolTrue>,
        //                        And<APPayment.voided,Equal<boolFalse>,								
        //                        And<APPayment.docDate,LessEqual<Current<CABatch.tranDate>>,
        //                        And<CABatchDetail.batchNbr,IsNull>>>>>>> AvailablePayments;
        public PXSelectJoin<APInvoice, InnerJoin<APAdjust, On<APInvoice.docType, Equal<APAdjust.adjdDocType>,
                            And<APInvoice.refNbr,Equal<APAdjust.adjdRefNbr>>>,
                            InnerJoin<APPayment, On<APAdjust.adjgDocType, Equal<APPayment.docType>,
                                And<APAdjust.adjgRefNbr, Equal<APPayment.refNbr>>>>>,
                    Where<APAdjust.adjgDocType, Equal<Current<APPayment.docType>>,
                    And<APAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>>>> APPaymentApplications;
		
				
		public PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Current<CABatch.cashAccountID>>>> cashAccount;   
    	public PXSetup<CASetup> casetup;

		public PXSelectJoin<CABatchDetail,
							InnerJoin<APPayment, On<APPayment.docType, Equal<CABatchDetail.origDocType>,
							And<APPayment.refNbr, Equal<CABatchDetail.origRefNbr>,
							And<APPayment.released,Equal<True>>>>>,
							Where<CABatchDetail.batchNbr, Equal<Current<CABatch.batchNbr>>>> ReleasedPayments;


		#region Selects, used in export
		public PXSelectReadonly<CashAccountPaymentMethodDetail,
		Where<CashAccountPaymentMethodDetail.paymentMethodID, Equal<Current<CABatch.paymentMethodID>>,
		And<Current<APPayment.docType>, IsNotNull,
		And<Current<APPayment.refNbr>, IsNotNull,
		And<CashAccountPaymentMethodDetail.accountID, Equal<Current<CABatch.cashAccountID>>,
		And<CashAccountPaymentMethodDetail.detailID, Equal<Required<CashAccountPaymentMethodDetail.detailID>>>>>>>> cashAccountSettings;

		public PXSelectReadonly2<VendorPaymentMethodDetail,
				InnerJoin<Location, On<Location.bAccountID, Equal<VendorPaymentMethodDetail.bAccountID>,
                    And<Location.locationID, Equal<VendorPaymentMethodDetail.locationID>>>>,
				Where<VendorPaymentMethodDetail.paymentMethodID, Equal<Current<CABatch.paymentMethodID>>,
					And<Current<APPayment.docType>, IsNotNull,
					And<Current<APPayment.refNbr>, IsNotNull,
					And<Location.bAccountID, Equal<Current<APPayment.vendorID>>,
                    And<Location.locationID, Equal<Current<APPayment.vendorLocationID>>,
					And<VendorPaymentMethodDetail.detailID, Equal<Required<VendorPaymentMethodDetail.detailID>>>>>>>>> vendorPaymentSettings;


		
		#endregion
        #endregion

		#region Select Delegate

#if false
		public virtual IEnumerable availablePayments()
		{
			PaymentFilter flt = this.filter.Current;
			CABatch doc = this.Document.Current;
			if (doc == null || doc.CashAccountID == null || doc.Released == true)
				yield break;
			PXSelectBase<ARPayment> paymentSelect = new PXSelectJoin<ARPayment,
								LeftJoin<CADepositDetail, On<CADepositDetail.origDocType, Equal<ARPayment.docType>,
									And<CADepositDetail.origRefNbr, Equal<ARPayment.refNbr>,
									And<CADepositDetail.tranType, Equal<CATranType.cADeposit>>>>,
								LeftJoin<CABatch, On<CABatch.tranType, Equal<CADepositDetail.tranType>,
									And<CABatch.refNbr, Equal<CADepositDetail.refNbr>>>,
								InnerJoin<CashAccountDeposit, On<CashAccountDeposit.depositAcctID, Equal<ARPayment.cashAccountID>,
								And<Where<CashAccountDeposit.paymentMethodID, Equal<ARPayment.paymentMethodID>,
									Or<CashAccountDeposit.paymentMethodID, Equal<BQLConstants.EmptyString>>>>>>>>,
								Where<CashAccountDeposit.accountID, Equal<Current<CABatch.cashAccountID>>,
								And<ARPayment.docType, NotEqual<ARPaymentType.voidPayment>,
								And<ARPayment.docType, NotEqual<ARPaymentType.cashReturn>,
								And<ARPayment.released, Equal<boolTrue>,
								And<ARPayment.voided, Equal<boolFalse>,
								And<ARPayment.depositAsBatch, Equal<boolTrue>,
								And<ARPayment.depositNbr, IsNull,
								And<ARPayment.depositAfter, LessEqual<Current<CABatch.tranDate>>,
								And<Where<CADepositDetail.refNbr, IsNull, Or<CABatch.voided, Equal<boolTrue>>>>>>>>>>>>,
								OrderBy<Asc<ARPayment.docType, Asc<ARPayment.refNbr, Desc<CashAccountDeposit.paymentMethodID>>>>>(this);
			if (flt.CashAccountID.HasValue)
				paymentSelect.WhereAnd<Where<ARPayment.cashAccountID, Equal<Current<PaymentFilter.cashAccountID>>>>();
			if (!String.IsNullOrEmpty(flt.PaymentMethodID))
				paymentSelect.WhereAnd<Where<ARPayment.paymentMethodID, Equal<Current<PaymentFilter.paymentMethodID>>>>();
			ARPayment last = null;
			foreach (PXResult<ARPayment, CADepositDetail, CABatch, CashAccountDeposit> it in paymentSelect.Select())
			{
				ARPayment payment = it;
				CADepositDetail detail = it;
				bool exist = false;
				if (last != null && last.DocType == payment.DocType && last.RefNbr == payment.RefNbr) continue; //Skip duplicates 
				last = payment;
				//Add filter for CashAccountDeposit.paymentMethodID according to priorities
				foreach (CADepositDetail iDet in this.Details.Select())
				{
					if (iDet.OrigDocType == payment.DocType && iDet.OrigRefNbr == payment.RefNbr)
					{
						exist = true;
						break;
					}
				}
				if (exist) continue;

				yield return it;
			}
		} 
#endif

		#endregion

		#region Events

		#region CABatch Events
		protected virtual void CABatch_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CABatch row = e.Row as CABatch;
			if (row == null) return;			
			
			bool isReleased = (row.Released == true);
					
			
			PXUIFieldAttribute.SetEnabled(sender, row, false);
			PXUIFieldAttribute.SetEnabled<CABatch.batchNbr>(sender, row, true);		

			bool isProcessing = row.Processing ?? false;
			PXUIFieldAttribute.SetEnabled<CABatch.processing>(sender, row, true);

			bool allowDelete = !isReleased;
			if (allowDelete)
			{
				allowDelete = !(this.ReleasedPayments.Select(row.BatchNbr).Count > 0);
			}
			sender.AllowDelete = allowDelete;

			CashAccount cashaccount = (CashAccount)PXSelectorAttribute.Select<CABatch.cashAccountID>(sender, row);
			bool clearEnabled = (row.Released != true) && (cashaccount != null) && (cashaccount.Reconcile == true);
		
			if (!isReleased)
			{				
				PXUIFieldAttribute.SetEnabled<CABatch.hold>(sender, row, !isReleased);
				PXUIFieldAttribute.SetEnabled<CABatch.tranDesc>(sender, row, !isReleased);
				PXUIFieldAttribute.SetEnabled<CABatch.tranDate>(sender, row, !isReleased);
				PXUIFieldAttribute.SetEnabled<CABatch.batchSeqNbr>(sender, row, !isReleased);
				PXUIFieldAttribute.SetEnabled<CABatch.extRefNbr>(sender, row, !isReleased);
				PXUIFieldAttribute.SetEnabled<CABatch.released>(sender, row, true);
				
				bool hasDetails =this.BatchPayments.Select().Count >0;
				PXUIFieldAttribute.SetEnabled<CABatch.paymentMethodID>(sender, row, !hasDetails && !isReleased);
				PXUIFieldAttribute.SetEnabled<CABatch.cashAccountID>(sender, row, !hasDetails && !isReleased);
				if (hasDetails) 
				{
					decimal? curyTotal=Decimal.Zero, total=Decimal.Zero;
					this.CalcDetailsTotal(ref curyTotal, ref total);
					row.DetailTotal = total;
					row.CuryTotal = curyTotal;
				} 
				
			}
			PXUIFieldAttribute.SetVisible<CABatch.curyDetailTotal>(sender, row, isReleased);
			PXUIFieldAttribute.SetVisible<CABatch.curyTotal>(sender, row, !isReleased);
			PXUIFieldAttribute.SetEnabled<CABatch.exportFileName>(sender, row, isProcessing);
			PXUIFieldAttribute.SetEnabled<CABatch.exportTime>(sender, row, isProcessing);
			PXUIFieldAttribute.SetVisible<CABatch.dateSeqNbr>(sender, row,isReleased);

			this.Release.SetEnabled(!isReleased && (row.Hold == false));
			this.Export.SetEnabled(isReleased);
			//this.addExtPayment.SetEnabled(!isReleased);		
		}
	
		protected virtual void CABatch_CashAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABatch row = (CABatch)e.Row;
			row.Cleared = false;
			row.ClearDate = null;
			if (cashAccount.Current == null || cashAccount.Current.CashAccountID != row.CashAccountID)
			{
				cashAccount.Current = (CashAccount)PXSelectorAttribute.Select<CABatch.cashAccountID>(sender, row);
			}
			if (cashAccount.Current.Reconcile != true)
			{
				row.Cleared = true;
				row.ClearDate = row.TranDate;			
			}
			sender.SetDefaultExt<CABatch.referenceID>(e.Row);
			sender.SetDefaultExt<CABatch.paymentMethodID>(e.Row);			
		}

        protected virtual void CABatch_PaymentMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CABatch row = (CABatch)e.Row;
            sender.SetDefaultExt<CABatch.batchSeqNbr>(e.Row);			
        }

        protected virtual void CABatch_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        { 
            this._isMassDelete = false;
        }

        private bool _isMassDelete = false;
        protected virtual void CABatch_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            this._isMassDelete = true;
        }

		#endregion

		#region CABatch Detail events

		protected virtual void CABatchDetail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			CABatchDetail row = (CABatchDetail)e.Row;
			bool isReleased = false;
			if (row.OrigModule == GL.BatchModule.AP)
			{
				APRegister doc = PXSelect<APRegister, Where<APRegister.docType, Equal<Required<APRegister.docType>>,
											And<APRegister.refNbr, Equal<Required<APRegister.refNbr>>>>>.Select(this, row.OrigDocType, row.OrigRefNbr);
				isReleased = (bool)doc.Released;
			}
			if (row.OrigModule == GL.BatchModule.AR)
			{
				ARRegister doc = PXSelect<ARRegister, Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
											And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>.Select(this, row.OrigDocType, row.OrigRefNbr);
				isReleased = (bool)doc.Released;
			}
			if (isReleased)
				throw new PXException(Messages.ReleasedDocumentMayNotBeAddedToCABatch);
		}
		
		protected virtual void CABatchDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			CABatchDetail row = (CABatchDetail)e.Row;
			UpdateDocAmount(row, false);			
		}

		protected virtual void CABatchDetail_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			CABatchDetail row = (CABatchDetail)e.Row;
			bool isReleased =false;
			bool isVoided = false;
			if(row.OrigModule == GL.BatchModule.AP)
			{
				APRegister doc = PXSelect<APRegister, Where<APRegister.docType, Equal<Required<APRegister.docType>>,
											And<APRegister.refNbr, Equal<Required<APRegister.refNbr>>>>>.Select(this, row.OrigDocType, row.OrigRefNbr);
				isReleased = (bool) doc.Released;
				isVoided = (bool)doc.Voided; 
			}
			if (row.OrigModule == GL.BatchModule.AR)
			{
				ARRegister doc = PXSelect<ARRegister,Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
											And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>.Select(this, row.OrigDocType, row.OrigRefNbr);
				isReleased = (bool)doc.Released;
				isVoided = (bool)doc.Voided; 
			}
			if (isReleased && !isVoided)
				throw new PXException(Messages.ReleasedDocumentMayNotBeDeletedFromCABatch);
		}

		protected virtual void CABatchDetail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			CABatchDetail row = (CABatchDetail)e.Row;
            if (!this._isMassDelete)
            {
                UpdateDocAmount(row, true);
            }
		}

		private CABatch UpdateDocAmount(CABatchDetail row, bool negative)
		{
			CABatch doc = this.Document.Current;
			if (row.OrigDocType != null && row.OrigRefNbr != null)
			{
				decimal? curyAmount = null, amount = null;
				if (row.OrigModule == GL.BatchModule.AP)
				{
					APPayment pmt = PXSelect<APPayment,
							Where<APPayment.docType, Equal<Required<APPayment.docType>>,
							And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>.Select(this, row.OrigDocType, row.OrigRefNbr);
					if (pmt != null)
					{
						curyAmount = pmt.CuryOrigDocAmt;
						amount = pmt.OrigDocAmt;
					}
				}
				else
				{
					ARPayment pmt = PXSelect<ARPayment,
							Where<ARPayment.docType, Equal<Required<ARPayment.docType>>,
							And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>.Select(this, row.OrigDocType, row.OrigRefNbr);
					if (pmt != null)
					{
						curyAmount = pmt.CuryOrigDocAmt;
						amount = pmt.OrigDocAmt;
					}
				}
				CABatch copy = (CABatch)this.Document.Cache.CreateCopy(doc);
				if (curyAmount.HasValue)
					doc.CuryDetailTotal += negative ? -curyAmount : curyAmount;
				if (amount.HasValue)
					doc.DetailTotal += negative ? -amount: amount;
				doc = this.Document.Update(doc);
			}
			return doc;
		}
		
		
		#endregion
		
		#endregion

		#region Methods
		
		public virtual CABatchDetail AddPayment(APPayment aPayment,  bool skipCheck)
		{
			if (!skipCheck)
			{
				foreach (CABatchDetail iDel in this.BatchPayments.Select())
				{
					if(IsKeyEqual(aPayment,iDel)) return iDel;					
				}
			}
			CABatchDetail detail = new CABatchDetail();
			detail.Copy(aPayment);
			detail = this.BatchPayments.Insert(detail);
			return detail; 			
		}

		public virtual CABatchDetail AddPayment(ARPayment aPayment, bool skipCheck)
		{
			if (!skipCheck)
			{
				foreach (CABatchDetail iDel in this.BatchPayments.Select())
				{
					if (IsKeyEqual(aPayment, iDel)) return iDel;
				}
			}
			CABatchDetail detail = new CABatchDetail();
			detail.Copy(aPayment);
			detail = this.BatchPayments.Insert(detail);
			return detail;
		}

		protected virtual void ParentFieldUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<CABatch.tranDate>(e.Row, e.OldRow))
			{
				foreach (CABatchDetail tran in this.Details.Select())
				{
					if (this.Details.Cache.GetStatus(tran) == PXEntryStatus.Notchanged)
					{
						this.Details.Cache.SetStatus(tran, PXEntryStatus.Updated);
					}
				}
			}
		}


		#endregion

		#region Internal Utilities
		public virtual string GenerateFileName(CABatch aBatch)
		{
			if (aBatch.CashAccountID != null && !string.IsNullOrEmpty(aBatch.PaymentMethodID))
			{
				CashAccount acct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, aBatch.CashAccountID);
				if (acct != null)
				{
                    return String.Format(Messages.CABatchDefaultExportFilenameTemplate, aBatch.PaymentMethodID, acct.CashAccountCD, aBatch.TranDate.Value, aBatch.DateSeqNbr);
				}
			}
			return string.Empty;
		}

		public virtual void CalcDetailsTotal(ref decimal? aCuryTotal, ref decimal? aTotal) 
		{
			aCuryTotal = Decimal.Zero;
			aTotal = Decimal.Zero;
			foreach (PXResult<CABatchDetail,APPayment> it in this.BatchPayments.Select()) 
			{
				APPayment pmt = it;
				if (!String.IsNullOrEmpty(pmt.RefNbr)) 
				{
					aCuryTotal += pmt.CuryOrigDocAmt;
					aTotal += pmt.OrigDocAmt;
				}
			}
		}
		#endregion
		
		#region Static Methods
		public static void ReleaseDoc(CABatch aDoc) 
		{
			if ((bool)aDoc.Released || (bool)aDoc.Hold)
				throw new PXException(Messages.CABatchStatusIsNotValidForProcessing);
			CABatchUpdate be = PXGraph.CreateInstance<CABatchUpdate>();
			CABatch doc = be.Document.Select(aDoc.BatchNbr);
			be.Document.Current = doc;
			if((bool)doc.Released || (bool)doc.Hold)
				throw new PXException(Messages.CABatchStatusIsNotValidForProcessing);

			APPayment voided = PXSelectReadonly2<APPayment,
							InnerJoin<CABatchDetail, On<CABatchDetail.origDocType, Equal<APPayment.docType>,
							And<CABatchDetail.origRefNbr, Equal<APPayment.refNbr>,
							And<CABatchDetail.origModule, Equal<GL.BatchModule.moduleAP>>>>>,
							Where<CABatchDetail.batchNbr, Equal<Required<CABatch.batchNbr>>,
								And<APPayment.voided, Equal<True>>>>.Select(be,doc.BatchNbr);
			if (voided != null && String.IsNullOrEmpty(voided.RefNbr) == false) 
			{
				throw new PXException(Messages.CABatchContainsVoidedPaymentsAndConnotBeReleased);
			}

            List<APRegister> unreleasedList = new List<APRegister>();
            PXSelectBase<APPayment> selectUnreleased = new PXSelectReadonly2<APPayment,
                            InnerJoin<CABatchDetail, On<CABatchDetail.origDocType, Equal<APPayment.docType>,
                            And<CABatchDetail.origRefNbr, Equal<APPayment.refNbr>,
                            And<CABatchDetail.origModule, Equal<GL.BatchModule.moduleAP>>>>>,
                            Where<CABatchDetail.batchNbr, Equal<Optional<CABatch.batchNbr>>,
                                And<APPayment.released, Equal<boolFalse>>>>(be);
            foreach (APPayment iPmt in selectUnreleased.Select(doc.BatchNbr)) 
			{
				if (iPmt.Released != true) 
				{
					unreleasedList.Add(iPmt);
				}
			}
			if (unreleasedList.Count > 0)
			{
				APDocumentRelease.ReleaseDoc(unreleasedList, true);
			}

            selectUnreleased.View.Clear();
            APPayment pmt = selectUnreleased.Select(doc.BatchNbr);
			if (pmt != null) 
			{
				throw new PXException(Messages.CABatchContainsUnreleasedPaymentsAndCannotBeReleased);
			}
			doc.Released = true;
			doc.DateSeqNbr = GetNextDateSeqNbr(be, aDoc);
			be.RecalcTotals();
			doc = be.Document.Update(doc);
			be.Actions.PressSave();
		}

		public static bool IsKeyEqual(APPayment op1, CABatchDetail op2)
		{
			return (op2.OrigModule == BatchModule.AP && op1.DocType == op2.OrigDocType && op1.RefNbr == op2.OrigRefNbr);
		}

		public static bool IsKeyEqual(AR.ARPayment op1, CABatchDetail op2)
		{
			return (op2.OrigModule == BatchModule.AR && op1.DocType == op2.OrigDocType && op1.RefNbr == op2.OrigRefNbr);
		}

		public static Int16 GetNextDateSeqNbr(PXGraph graph, CABatch aDoc)
		{
			Int16 result = 0;
			CABatch last = PXSelectReadonly<CABatch, 
							Where<CABatch.cashAccountID, Equal<Required<CABatch.cashAccountID>>,
							And<CABatch.paymentMethodID, Equal<Required<CABatch.paymentMethodID>>,
							And<CABatch.released,Equal<True>,
							And<CABatch.tranDate, Equal<Required<CABatch.tranDate>>>>>>,
							OrderBy<Desc<CABatch.dateSeqNbr>>>.Select(graph, aDoc.CashAccountID, aDoc.PaymentMethodID, aDoc.TranDate);
			if (last != null) 
			{
				result = last.DateSeqNbr ?? (short)0;
				if (result >= short.MaxValue || result < short.MinValue)
					throw new PXException(Messages.DateSeqNumberIsOutOfRange);
				result++;
			}
			return result;
		}	
		#endregion

		#region Processing Grpah Definition
		[PXHidden]
		public class CABatchUpdate : PXGraph<CABatchUpdate>
		{
			public PXSelect<CABatch, Where<CABatch.batchNbr, Equal<Required<CABatch.batchNbr>>>> Document;
			public PXSelectJoin<APPayment,
							InnerJoin<CABatchDetail, On<CABatchDetail.origDocType, Equal<APPayment.docType>,
							And<CABatchDetail.origRefNbr, Equal<APPayment.refNbr>,
							And<CABatchDetail.origModule, Equal<GL.BatchModule.moduleAP>>>>>,
							Where<CABatchDetail.batchNbr, Equal<Optional<CABatch.batchNbr>>>> APPaymentList;
			public virtual void RecalcTotals()
			{
				CABatch row = this.Document.Current;
				if (row != null)
				{
					row.DetailTotal = row.CuryDetailTotal = row.CuryTotal = row.Total = decimal.Zero;
					foreach (PXResult<APPayment, CABatchDetail> it in this.APPaymentList.Select())
					{
						APPayment pmt = it;
						if (!String.IsNullOrEmpty(pmt.RefNbr))
						{
							row.CuryDetailTotal += pmt.CuryOrigDocAmt;
							row.DetailTotal += pmt.OrigDocAmt;
						}
					}

				}
			}
		}
		
		#endregion
	}
	
}
