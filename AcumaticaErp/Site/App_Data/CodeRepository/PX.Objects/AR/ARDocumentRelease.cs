using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Avalara.AvaTax.Adapter;
using Avalara.AvaTax.Adapter.TaxService;
using PX.Data;
using PX.Objects.BQLConstants;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.TX;
using PX.Objects.AR.Overrides.ARDocumentRelease;
using PX.Objects.CA;
using PX.Objects.AR.Overrides.ScheduleMaint;
using SOOrder = PX.Objects.SO.SOOrder;
using SOInvoice = PX.Objects.SO.SOInvoice;
using SOOrderShipment = PX.Objects.SO.SOOrderShipment;
using INTran = PX.Objects.IN.INTran;
using PMTran = PX.Objects.PM.PMTran;
using PX.Objects.DR;
using PX.Objects.CR;
using PX.Common;

namespace PX.Objects.AR
{
	[System.SerializableAttribute()]
	public partial class BalancedARDocument : ARRegister
	{
		#region Selected
		public new abstract class selected : IBqlField
		{
		}
		#endregion
		#region DocType
		public new abstract class docType : PX.Data.IBqlField
		{
		}
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.IBqlField
		{
		}
		#endregion
		#region OrigModule
		public new abstract class origModule : PX.Data.IBqlField
		{
        }
        #endregion
        #region OpenDoc
        public new abstract class openDoc : PX.Data.IBqlField
		{
		}
		#endregion
		#region Released
		public new abstract class released : PX.Data.IBqlField
		{
		}
		#endregion
		#region Hold
		public new abstract class hold : PX.Data.IBqlField
		{
		}
		#endregion
		#region Scheduled
		public new abstract class scheduled : PX.Data.IBqlField
		{
		}
		#endregion
		#region Voided
		public new abstract class voided : PX.Data.IBqlField
		{
		}
		#endregion
		#region Status
		public new abstract class status : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[ARDocStatus.List()]
		public override String Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		#endregion
		#region CreatedByID
		public new abstract class createdByID : PX.Data.IBqlField
		{
		}
		#endregion
		#region LastModifiedByID
		public new abstract class lastModifiedByID : PX.Data.IBqlField
		{
		}
		#endregion
		#region CustomerRefNbr
		public abstract class customerRefNbr : IBqlField
		{
		}
		protected String _CustomerRefNbr;
		[PXString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Customer Ref. Nbr.")]
		public String CustomerRefNbr
		{
			get
			{
				return _CustomerRefNbr;
			}
			set
			{
				_CustomerRefNbr = value;
			}
		}
		#endregion
		#region Methods
		protected override void SetStatus()
		{
		}
		#endregion
	}

	public class PXMassProcessException : PXException
	{
		protected Exception _InnerException;
		protected int _ListIndex;

		public int ListIndex
		{
			get
			{
				return this._ListIndex;
			}
		}

		public PXMassProcessException(int ListIndex, Exception InnerException)
			: base(InnerException is PXOuterException ? InnerException.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)InnerException).InnerMessages) : InnerException.Message, InnerException)
		{
			this._ListIndex = ListIndex;
		}

		public PXMassProcessException(SerializationInfo info, StreamingContext context)
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

	[PX.Objects.GL.TableAndChartDashboardType]
	public class ARDocumentRelease : PXGraph<ARDocumentRelease>
	{
		public PXCancel<BalancedARDocument> Cancel;
		[PXFilterable]
		public PXProcessingJoin<BalancedARDocument,
		    LeftJoin<ARInvoice, On<ARInvoice.docType, Equal<BalancedARDocument.docType>,
					And<ARInvoice.refNbr, Equal<BalancedARDocument.refNbr>>>,
				LeftJoin<ARPayment, On<ARPayment.docType, Equal<BalancedARDocument.docType>,
					And<ARPayment.refNbr, Equal<BalancedARDocument.refNbr>>>,
				InnerJoin<Customer, On<Customer.bAccountID, Equal<BalancedARDocument.customerID>>,
				LeftJoin<ARAdjust, On<ARAdjust.adjgDocType, Equal<BalancedARDocument.docType>,
				And<ARAdjust.adjgRefNbr, Equal<BalancedARDocument.refNbr>,
				And<ARAdjust.adjNbr, Equal<BalancedARDocument.lineCntr>,
				And<ARAdjust.hold, Equal<boolFalse>>>>>>>>>,
				Where2<Match<Customer, Current<AccessInfo.userName>>, And<ARRegister.hold, Equal<boolFalse>, And<ARRegister.voided, Equal<boolFalse>, And<ARRegister.scheduled, Equal<boolFalse>, And<Where<BalancedARDocument.released, Equal<boolFalse>, And<BalancedARDocument.origModule, Equal<GL.BatchModule.moduleAR>, Or<BalancedARDocument.openDoc, Equal<boolTrue>, And<ARAdjust.adjdRefNbr, IsNotNull>>>>>>>>>> ARDocumentList;

		public ARDocumentRelease()
		{
			ARSetup setup = arsetup.Current;
			ARDocumentList.SetProcessDelegate(
				delegate(List<BalancedARDocument> list)
				{
					List<ARRegister> newlist = new List<ARRegister>(list.Count);
					foreach (BalancedARDocument doc in list)
					{
						newlist.Add(doc);
					}
					ReleaseDoc(newlist, true);
				}
			);
			ARDocumentList.SetProcessCaption("Release");
			ARDocumentList.SetProcessAllCaption("Release All");
		}

		public PXAction<BalancedARDocument> ViewDocument;
		[PXUIField(DisplayName = "View Document", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable viewDocument(PXAdapter adapter)
		{
			if (this.ARDocumentList.Current != null)
			{
				PXRedirectHelper.TryRedirect(ARDocumentList.Cache, ARDocumentList.Current, "Document", PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public delegate void ARMassProcessDelegate(ARRegister ardoc, bool isAborted);

		public static void ReleaseDoc(List<ARRegister> list, bool isMassProcess)
		{
			ReleaseDoc(list, isMassProcess, null, null);
		}

		/// <summary>
		/// Static function for release of AR documents and posting of the released batch.
		/// Released batches will be posted if the corresponded flag in ARSetup is set to true.
		/// SkipPost parameter is used to override this flag. 
		/// This function can not be called from inside of the covering DB transaction scope, unless skipPost is set to true.     
		/// </summary>
		/// <param name="list">List of the documents to be released</param>
		/// <param name="isMassProcess">Flag specifing if the function is called from mass process - affects error handling</param>
		/// <param name="skipPost"> Prevent Posting of the released batch(es). This parameter must be set to true if this function is called from "covering" DB transaction</param>
		public static void ReleaseDoc(List<ARRegister> list, bool isMassProcess, List<Batch> externalPostList, ARMassProcessDelegate onsuccess)
		{
			bool failed = false;
			ARReleaseProcess rg = PXGraph.CreateInstance<ARReleaseProcess>();
			JournalEntry je = PXGraph.CreateInstance<JournalEntry>();
			je.FieldVerifying.AddHandler<GLTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			je.FieldVerifying.AddHandler<GLTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			if (rg.ZeroPost)
			{
				je.RowInserting.AddHandler<GLTran>((sender, e) => { ((GLTran)e.Row).ZeroPost = ((GLTran)e.Row).ZeroPost ?? "DRB".IndexOf(((GLTran)e.Row).TranClass) < 0; });
			}

			PostGraph pg = PXGraph.CreateInstance<PostGraph>();
			Dictionary<int, int> batchbind = new Dictionary<int, int>();
			List<Batch> pmBatchList = new List<Batch>();
			bool isSkipPost = externalPostList != null;

			for (int i = 0; i < list.Count; i++)
			{
				ARRegister doc = list[i];
				try
				{
                    bool onefailed = false;
                    rg.Clear();

					try
					{
						List<ARRegister> childs = rg.ReleaseDocProc(je, doc, pmBatchList);
					    int k;
					    if ((k = je.created.IndexOf(je.BatchModule.Current)) >= 0 && batchbind.ContainsKey(k) == false)
					    {
						    batchbind.Add(k, i);
					    }


					    if (childs != null)
					    {
						    for (int j = 0; j < childs.Count; j++)
						    {
							    rg.Clear();
								rg.ReleaseDocProc(je, childs[j], pmBatchList);

                                object cached;
                                if ((cached = rg.ARDocument.Cache.Locate(doc)) != null)
                                {
                                    PXCache<ARRegister>.RestoreCopy(doc, (ARRegister)cached);
                                    doc.Selected = true;
                                }
                            }
                        }
                    }
                    catch
                    {
                        onefailed = true;
                        throw;
                    }
                    finally
                    {
                        if (onsuccess != null)
                        {
                            onsuccess(doc, onefailed);
                        }
                    }

					if (isMassProcess)
					{
						if (string.IsNullOrEmpty(doc.WarningMessage))
						PXProcessing<ARRegister>.SetInfo(i, ActionsMessages.RecordProcessed);
						else
						{
							PXProcessing<ARRegister>.SetWarning(i, doc.WarningMessage);
					}
				}

				}
				catch (Exception e)
				{
					if (isMassProcess)
					{
						PXProcessing<ARRegister>.SetError(i, e);
						failed = true;
					}
					else
					{
						throw new PXMassProcessException(i, e);
					}
				}
			}

			if (isSkipPost) 
			{
				if (rg.AutoPost)
					externalPostList.AddRange(je.created);
			}
			else
			{
				for (int i = 0; i < je.created.Count; i++)
				{
					Batch batch = je.created[i];
					try
					{
						if (rg.AutoPost)
						{
							pg.Clear();
							pg.PostBatchProc(batch);
						}
					}
					catch (Exception e)
					{
						if (isMassProcess)
						{
							failed = true;
							PXProcessing<ARRegister>.SetError(batchbind[i], e);
						}
						else
						{
							throw new PXMassProcessException(batchbind[i], e);
						}
					}
				}
			}
			if (failed)
			{
				throw new PXException(GL.Messages.DocumentsNotReleased);
			}

			List<PM.ProcessInfo<Batch>> infoList = new List<ProcessInfo<Batch>>();
			ProcessInfo<Batch> processInfo = new ProcessInfo<Batch>(0);
			processInfo.Batches.AddRange(pmBatchList);
			infoList.Add(processInfo);
			PM.RegisterRelease.Post(infoList, false);
		}

		protected virtual IEnumerable ardocumentlist()
		{
			foreach (PXResult<BalancedARDocument, ARInvoice, ARPayment, Customer, ARAdjust> res in 
				PXSelectJoinGroupBy<BalancedARDocument,
				LeftJoin<ARInvoice, On<ARInvoice.docType, Equal<BalancedARDocument.docType>,
					And<ARInvoice.refNbr, Equal<BalancedARDocument.refNbr>>>,
				LeftJoin<ARPayment, On<ARPayment.docType, Equal<BalancedARDocument.docType>,
					And<ARPayment.refNbr, Equal<BalancedARDocument.refNbr>>>,
				InnerJoin<Customer, On<Customer.bAccountID, Equal<BalancedARDocument.customerID>>,
				LeftJoin<ARAdjust, On<ARAdjust.adjgDocType, Equal<BalancedARDocument.docType>, 
				And<ARAdjust.adjgRefNbr, Equal<BalancedARDocument.refNbr>, 
				And<ARAdjust.adjNbr, Equal<BalancedARDocument.lineCntr>,
				And<ARAdjust.hold, Equal<boolFalse>>>>>>>>>,
				Where2<Match<Customer, Current<AccessInfo.userName>>, 
                    And<ARRegister.hold, Equal<boolFalse>, 
                    And<ARRegister.voided, Equal<boolFalse>, 
                    And<ARRegister.scheduled, Equal<boolFalse>,
					And2<Where<ARInvoice.refNbr, IsNotNull, Or<ARPayment.refNbr, IsNotNull>>,
                    And<Where<BalancedARDocument.released, Equal<boolFalse>, 
                        And<BalancedARDocument.origModule, Equal<GL.BatchModule.moduleAR>, 
                        Or<BalancedARDocument.openDoc, Equal<boolTrue>, And<ARAdjust.adjdRefNbr, IsNotNull>>>>>>>>>>,
				Aggregate<GroupBy<BalancedARDocument.docType, 
				GroupBy<BalancedARDocument.refNbr, 
				GroupBy<BalancedARDocument.released, 
				GroupBy<BalancedARDocument.openDoc, 
				GroupBy<BalancedARDocument.hold, 
				GroupBy<BalancedARDocument.scheduled, 
				GroupBy<BalancedARDocument.voided, 
				GroupBy<BalancedARDocument.createdByID, 
				GroupBy<BalancedARDocument.lastModifiedByID,
				GroupBy<Customer.printInvoices,
				GroupBy<ARInvoice.dontPrint,
				GroupBy<ARInvoice.printed,
				GroupBy<ARInvoice.dontEmail,
				GroupBy<ARInvoice.emailed,
				GroupBy<ARInvoice.creditHold,
				GroupBy<ARInvoice.approvedCredit>>>>>>>>>>>>>>>>>>
				.Select(this, null))
			{
				BalancedARDocument ardoc = (BalancedARDocument)res;
				ARInvoice invoice = (ARInvoice)res;
				ARPayment payment = (ARPayment)res;               

				if (invoice != null && string.IsNullOrEmpty(invoice.InvoiceNbr) == false)
				{
					ardoc.CustomerRefNbr = invoice.InvoiceNbr;
				}
				else if (payment != null && string.IsNullOrEmpty(payment.ExtRefNbr) == false)
				{
					ardoc.CustomerRefNbr = payment.ExtRefNbr;
				}

				if (payment != null && payment.PMInstanceID != null
						&& this.arsetup.Current.IntegratedCCProcessing == true)
				{
					//Filter out payments by CreditCard	 - they are processed separately
					PXResult<CustomerPaymentMethod, PaymentMethod> pmResult = (PXResult<CustomerPaymentMethod, PaymentMethod>)PXSelectJoin<CustomerPaymentMethod, InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>>>,
														Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>.Select(this, payment.PMInstanceID);
					PaymentMethod pm = pmResult;
					CustomerPaymentMethod cpm = pmResult;
					if (pm.PaymentType == CA.PaymentMethodType.CreditCard && pm.ARIsProcessingRequired == true)
						continue;
				}

				ARAdjust adj = res;
				if ((bool)ardoc.Released ||
					   ((ARInvoice)res).RefNbr == null || 
						 (
							 (arsetup.Current.PrintBeforeRelease == false ||
								((ARInvoice)res).PrintInvoice == false)
							 &&
							 (arsetup.Current.EmailBeforeRelease == false ||
								((ARInvoice)res).EmailInvoice == false)
						 ))
				{
					if (adj.AdjdRefNbr != null)
					{
						ardoc.DocDate = adj.AdjgDocDate;
						ardoc.TranPeriodID = adj.AdjgTranPeriodID;
						ardoc.FinPeriodID = adj.AdjgFinPeriodID;
					}
					yield return new PXResult<BalancedARDocument, ARInvoice, ARPayment, Customer, ARAdjust>(ardoc, res, res, res, res);
				}				
			}
		}
		
		public PXSetup<ARSetup> arsetup;
	}

	public class ARPayment_CurrencyInfo_Currency_Customer : PXSelectJoin<ARPayment, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARPayment.curyInfoID>>, InnerJoin<Currency, On<Currency.curyID, Equal<CurrencyInfo.curyID>>, LeftJoin<Customer, On<Customer.bAccountID, Equal<ARPayment.customerID>>, LeftJoin<CashAccount, On<CashAccount.cashAccountID, Equal<ARPayment.cashAccountID>>>>>>, Where<ARPayment.docType, Equal<Required<ARPayment.docType>>, And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>
	{
		public ARPayment_CurrencyInfo_Currency_Customer(PXGraph graph)
			: base(graph)
		{
		}
	}

	public class ARInvoice_CurrencyInfo_Terms_Customer : PXSelectJoin<ARInvoice, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARInvoice.curyInfoID>>, LeftJoin<Terms, On<Terms.termsID, Equal<ARInvoice.termsID>>, LeftJoin<Customer, On<Customer.bAccountID, Equal<ARInvoice.customerID>>, LeftJoin<SOInvoice, On<SOInvoice.docType, Equal<ARInvoice.docType>, And<SOInvoice.refNbr, Equal<ARInvoice.refNbr>>>>>>>, Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>, And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>
	{
		public ARInvoice_CurrencyInfo_Terms_Customer(PXGraph graph)
			: base(graph)
		{
		}
	}

	public class ARReleaseProcess : PXGraph<ARReleaseProcess>
	{
		public PXSelect<ARRegister> ARDocument;
		public PXSelectJoin<ARTran, LeftJoin<ARTax, On<ARTax.tranType, Equal<ARTran.tranType>, And<ARTax.refNbr, Equal<ARTran.refNbr>, And<ARTax.lineNbr, Equal<ARTran.lineNbr>>>>, LeftJoin<Tax, On<Tax.taxID, Equal<ARTax.taxID>>, LeftJoin<DRDeferredCode, On<DRDeferredCode.deferredCodeID, Equal<ARTran.deferredCode>>, LeftJoin<PMTran, On<PMTran.tranID, Equal<ARTran.pMTranID>>, LeftJoin<SO.SOOrderType, On<SO.SOOrderType.orderType, Equal<ARTran.sOOrderType>>>>>>>, Where<ARTran.tranType, Equal<Required<ARTran.tranType>>, And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>>>, OrderBy<Asc<ARTran.lineNbr, Asc<Tax.taxCalcLevel>>>> ARTran_TranType_RefNbr;
		public PXSelect<ARTaxTran, Where<ARTaxTran.module, Equal<BatchModule.moduleAR>, And<ARTaxTran.tranType, Equal<Required<ARTaxTran.tranType>>, And<ARTaxTran.refNbr, Equal<Required<ARTaxTran.refNbr>>>>>, OrderBy<Asc<Tax.taxCalcLevel>>> ARTaxTran_TranType_RefNbr;
		public PXSelect<Batch> Batch;

		public ARInvoice_CurrencyInfo_Terms_Customer ARInvoice_DocType_RefNbr;
		public ARPayment_CurrencyInfo_Currency_Customer ARPayment_DocType_RefNbr;
		public PXSelectJoin<ARAdjust, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARAdjust.adjdCuryInfoID>>, InnerJoin<Currency, On<Currency.curyID, Equal<CurrencyInfo.curyID>>, LeftJoin<ARInvoice, On<ARInvoice.docType, Equal<ARAdjust.adjdDocType>, And<ARInvoice.refNbr, Equal<ARAdjust.adjdRefNbr>>>, LeftJoin<ARPayment, On<ARPayment.docType, Equal<ARAdjust.adjdDocType>, And<ARPayment.refNbr, Equal<ARAdjust.adjdRefNbr>>>>>>>, Where<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>, And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>, And<ARAdjust.adjNbr, Equal<Required<ARAdjust.adjNbr>>>>>> ARAdjust_AdjgDocType_RefNbr_CustomerID;
		public PXSelectJoin<ARAdjust, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARAdjust.adjdCuryInfoID>>, InnerJoin<Currency, On<Currency.curyID, Equal<CurrencyInfo.curyID>>, LeftJoin<ARPayment, On<ARPayment.docType, Equal<ARAdjust.adjgDocType>, And<ARPayment.refNbr, Equal<ARAdjust.adjgRefNbr>>>>>>, Where<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>, And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>, And<ARAdjust.adjNbr, Equal<Required<ARAdjust.adjNbr>>>>>> ARAdjust_AdjdDocType_RefNbr_CustomerID;
        public PXSelect<ARPaymentChargeTran, Where<ARPaymentChargeTran.docType, Equal<Required<ARPaymentChargeTran.docType>>, And<ARPaymentChargeTran.refNbr, Equal<Required<ARPaymentChargeTran.refNbr>>>>> ARPaymentChargeTran_DocType_RefNbr;

		public PXSelect<ARSalesPerTran, Where<ARSalesPerTran.docType, Equal<Required<ARSalesPerTran.docType>>,
											And<ARSalesPerTran.refNbr, Equal<Required<ARSalesPerTran.refNbr>>>>> ARDoc_SalesPerTrans;

		public PXSelect<CATran> CashTran;
		public PXSelect<INTran> intranselect;

		private ARSetup _arsetup;

		public ARSetup arsetup
		{
			get
			{
				if (_arsetup == null)
				{
					_arsetup = PXSelect<ARSetup>.Select(this);
				}
				return _arsetup;
			}
		}

		public bool AutoPost
		{
			get
			{
				return (bool)arsetup.AutoPost;
			}
		}

		public bool ZeroPost
		{
			get
			{
				return (bool)arsetup.ZeroPost;
			}
		}

		public bool SummPost
		{
			get
			{
				return (arsetup.TransactionPosting == "S");
			}
		}

        public string InvoiceRounding
        {
            get
            {
                //blocks rounding in release process when performed during data entry
                return RoundingType.Currency;
            }
        }

        public decimal? InvoicePrecision
        {
            get
            {
                return arsetup.InvoicePrecision;
            }
        }

		protected ARInvoiceEntry _ie = null;
		public ARInvoiceEntry ie
		{
			get
			{
				if (_ie == null)
				{
					_ie = PXGraph.CreateInstance<ARInvoiceEntry>();
				}
				return _ie;
			}
		}

		protected ARPaymentEntry _pe = null;
		public ARPaymentEntry pe
		{
			get
			{
				if (_pe == null)
				{
					_pe = PXGraph.CreateInstance<ARPaymentEntry>();
				}
				return _pe;
			}
		}

		[PXDBString(6, IsFixed = true)]
		[PXDefault()]
		protected virtual void ARPayment_AdjFinPeriodID_CacheAttached(PXCache sender)
		{ }

		[PXDBString(6, IsFixed = true)]
		[PXDefault()]
		protected virtual void ARPayment_AdjTranPeriodID_CacheAttached(PXCache sender)
		{ }

		public ARReleaseProcess()
		{
			//Caches[typeof(ARRegister)] = new PXCache<ARRegister>(this);
			OpenPeriodAttribute.SetValidatePeriod<ARRegister.finPeriodID>(ARDocument.Cache, null, PeriodValidation.Nothing);
			OpenPeriodAttribute.SetValidatePeriod<ARPayment.adjFinPeriodID>(ARPayment_DocType_RefNbr.Cache, null, PeriodValidation.Nothing);

			PXDBDefaultAttribute.SetDefaultForUpdate<ARAdjust.customerID>(Caches[typeof(ARAdjust)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARAdjust.adjgDocType>(Caches[typeof(ARAdjust)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARAdjust.adjgRefNbr>(Caches[typeof(ARAdjust)], null, false);
			PXDBLiteDefaultAttribute.SetDefaultForUpdate<ARAdjust.adjgCuryInfoID>(Caches[typeof(ARAdjust)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARAdjust.adjgDocDate>(Caches[typeof(ARAdjust)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARAdjust.adjgFinPeriodID>(Caches[typeof(ARAdjust)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARAdjust.adjgTranPeriodID>(Caches[typeof(ARAdjust)], null, false);

			PXDBDefaultAttribute.SetDefaultForInsert<ARTran.tranType>(Caches[typeof(ARTran)], null, false);
			PXDBDefaultAttribute.SetDefaultForInsert<ARTran.refNbr>(Caches[typeof(ARTran)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARTran.tranType>(Caches[typeof(ARTran)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARTran.refNbr>(Caches[typeof(ARTran)], null, false);
			PXDBLiteDefaultAttribute.SetDefaultForUpdate<ARTran.curyInfoID>(Caches[typeof(ARTran)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARTran.tranDate>(Caches[typeof(ARTran)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARTran.finPeriodID>(Caches[typeof(ARTran)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARTran.customerID>(Caches[typeof(ARTran)], null, false);

			PXDBDefaultAttribute.SetDefaultForUpdate<ARTaxTran.tranType>(Caches[typeof(ARTaxTran)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARTaxTran.refNbr>(Caches[typeof(ARTaxTran)], null, false);
			PXDBLiteDefaultAttribute.SetDefaultForUpdate<ARTaxTran.curyInfoID>(Caches[typeof(ARTaxTran)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARTaxTran.tranDate>(Caches[typeof(ARTaxTran)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARTaxTran.taxZoneID>(Caches[typeof(ARTaxTran)], null, false);

			PXDBDefaultAttribute.SetDefaultForUpdate<INTran.refNbr>(intranselect.Cache, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<INTran.tranDate>(intranselect.Cache, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<INTran.finPeriodID>(intranselect.Cache, null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<INTran.tranPeriodID>(intranselect.Cache, null, false);

			PXFormulaAttribute.SetAggregate<ARAdjust.curyAdjgAmt>(Caches[typeof(ARAdjust)], null);
			PXFormulaAttribute.SetAggregate<ARAdjust.curyAdjdAmt>(Caches[typeof(ARAdjust)], null);
			PXFormulaAttribute.SetAggregate<ARAdjust.adjAmt>(Caches[typeof(ARAdjust)], null);
        }

		protected virtual void ARPayment_CashAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void ARPayment_PMInstanceID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void ARPayment_PaymentMethodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void ARPayment_ExtRefNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void ARRegister_FinPeriodID_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				e.FieldName = string.Empty;
				e.Cancel = true;
			}
		}

		protected virtual void ARRegister_TranPeriodID_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				e.FieldName = string.Empty;
				e.Cancel = true;
			}
		}

		protected virtual void ARRegister_DocDate_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				e.FieldName = string.Empty;
				e.Cancel = true;
			}
		}

		protected virtual void ARAdjust_AdjdRefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void ARTran_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			e.Cancel = _IsIntegrityCheck;
		}

		protected virtual void ARTran_TaxCategoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		protected virtual void ARTran_SalesPersonID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		private ARHist CreateHistory(int? BranchID, int? AccountID, int? SubID, int? CustomerID, string PeriodID)
		{
			ARHist accthist = new ARHist();
			accthist.BranchID = BranchID;
			accthist.AccountID = AccountID;
			accthist.SubID = SubID;
			accthist.CustomerID = CustomerID;
			accthist.FinPeriodID = PeriodID;
			return (ARHist)Caches[typeof(ARHist)].Insert(accthist);
		}

		private CuryARHist CreateHistory(int? BranchID, int? AccountID, int? SubID, int? CustomerID, string CuryID, string PeriodID)
		{
			CuryARHist accthist = new CuryARHist();
			accthist.BranchID = BranchID;
			accthist.AccountID = AccountID;
			accthist.SubID = SubID;
			accthist.CustomerID = CustomerID;
			accthist.CuryID = CuryID;
			accthist.FinPeriodID = PeriodID;
			return (CuryARHist)Caches[typeof(CuryARHist)].Insert(accthist);
		}

        private class ARHistItemDiscountsBucket : ARHistBucket
        {
            public ARHistItemDiscountsBucket(ARTran tran)
				: base()
            {
                switch (tran.TranType)
                {
                    case ARDocType.Invoice:
                    case ARDocType.DebitMemo:
                    case ARDocType.CashSale:
                        SignPtdItemDiscounts = 1m;
                        break;
                    case ARDocType.CreditMemo:
                    case ARDocType.CashReturn:
                        SignPtdItemDiscounts = -1m;
                        break;
                }
            }
        }

		private class ARHistBucket
		{
			public int? arAccountID = null;
			public int? arSubID = null;
			public decimal SignPayments = 0m;
			public decimal SignDeposits = 0m;
			public decimal SignSales = 0m;
			public decimal SignFinCharges = 0m;
			public decimal SignCrMemos = 0m;
			public decimal SignDrMemos = 0m;
			public decimal SignDiscTaken = 0m;
			public decimal SignRGOL = 0m;
			public decimal SignPtd = 0m;
            public decimal SignPtdItemDiscounts = 0m;

			public ARHistBucket(GLTran tran, string TranType)
			{
				arAccountID = tran.AccountID;
				arSubID = tran.SubID;

				switch (TranType + tran.TranClass)
				{
					case "CSLN":
						SignSales = -1m;
						SignPayments = -1m;
						SignPtd = 0m;
						break;
					case "RCSN":
						SignSales = -1m;
						SignPayments = -1m;
						SignPtd = 0m;
						break;
					case "INVN":
						SignSales = 1m;
						SignPtd = 1m;
						break;
					case "DRMN":
						SignDrMemos = 1m;
						SignPtd = 1m;
						break;
					case "FCHN":
						SignFinCharges = 1m;
						SignPtd = 1m;
						break;
					case "CRMP":
					case "CRMN":
						SignCrMemos = -1m;
						SignPtd = 1m;
						break;
					case "CRMR":
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						SignCrMemos = -1m;
						SignRGOL = 1m;
						break;
					case "CRMD":
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						SignCrMemos = -1m;
						SignDiscTaken = 1m;
						break;
					case "CRMB":
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						SignCrMemos = 0m;
						break;
					case "PPMP":
						SignDeposits = -1m;
						break;
					case "PPMU":
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
					    SignDeposits = -1m;
						SignDrMemos = -1m;
						SignPtd = -1m;
					    break;
					case "PMTU":
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						SignPayments = -1m;
						SignDrMemos = -1m;
						break;
					case "RPMP":						
					case "RPMN":						
					case "PMTP":						
					case "PMTN":
					case "PPMN":
					case "REFP":
					case "REFN":
						SignPayments = -1m;
						SignPtd = 1m;
						break;
					case "REFU":
						SignDeposits = -1m;
						break;
					case "RPMR":
					case "PPMR":
					case "PMTR":
					case "REFR":
					case "CSLR":
					case "RCSR":
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						SignPayments = -1m;
						SignRGOL = 1m;
						break;
					case "RPMD":
					case "PPMD":
					case "PMTD":
					case "REFD": //not really happens
					case "CSLD":
					case "RCSD":
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						SignPayments = -1m;
						SignDiscTaken = 1m;
						break;
					case "SMCP":
						//Zero Update
						//will insert SCWO Account in ARHistory for trial balance report
						//arAccountID = tran.OrigAccountID;
						//arSubID = tran.OrigSubID;
						break;
					case "SMCN":
						SignDrMemos = 1m;
						SignPtd = 1m;
						break;
					case "SMBP":
						//Zero Update
						//will insert SBWO Account in ARHistory for trial balance report
						break;
					case "SMBD": //not really happens
						//Zero Update
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						break;
					case "SMBN":
						SignCrMemos = -1m;
						SignPtd = 1m;
						break;
					case "SMBR":
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						SignCrMemos = -1m;
						SignRGOL = 1m;
						break;
					case "RPMB":
					case "PPMB":
					case "REFB": //not really happens
					case "CSLB":
					case "RCSB":
					case "PMTB":
					case "SMBB":
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						SignPayments = -1m;
						SignCrMemos = 1m;
						break;
				}
			}

            protected ARHistBucket()
            {
            }
		}

		private void UpdateHist<History>(History accthist, ARHistBucket bucket, bool FinFlag, GLTran tran)
			where History : class, IBaseARHist
		{
			if (_IsIntegrityCheck == false || accthist.DetDeleted == false)
			{
				accthist.FinFlag = FinFlag;
				accthist.PtdPayments += bucket.SignPayments * (tran.DebitAmt - tran.CreditAmt);
				accthist.PtdSales += bucket.SignSales * (tran.DebitAmt - tran.CreditAmt);
				accthist.PtdDrAdjustments += bucket.SignDrMemos * (tran.DebitAmt - tran.CreditAmt);
				accthist.PtdCrAdjustments += bucket.SignCrMemos * (tran.DebitAmt - tran.CreditAmt);
				accthist.PtdFinCharges += bucket.SignFinCharges * (tran.DebitAmt - tran.CreditAmt);
				accthist.PtdDiscounts += bucket.SignDiscTaken * (tran.DebitAmt - tran.CreditAmt);
				accthist.PtdRGOL += bucket.SignRGOL * (tran.DebitAmt - tran.CreditAmt);
				accthist.YtdBalance += bucket.SignPtd * (tran.DebitAmt - tran.CreditAmt);
				accthist.PtdDeposits += bucket.SignDeposits * (tran.DebitAmt - tran.CreditAmt);
				accthist.YtdDeposits += bucket.SignDeposits * (tran.DebitAmt - tran.CreditAmt);
                accthist.PtdItemDiscounts += bucket.SignPtdItemDiscounts * (tran.DebitAmt - tran.CreditAmt);
            }
		}

		private void UpdateFinHist<History>(History accthist, ARHistBucket bucket, GLTran tran)
			where History : class, IBaseARHist
		{
			UpdateHist<History>(accthist, bucket, true, tran);
		}

		private void UpdateTranHist<History>(History accthist, ARHistBucket bucket, GLTran tran)
			where History : class, IBaseARHist
		{
			UpdateHist<History>(accthist, bucket, false, tran);
		}

		private void CuryUpdateHist<History>(History accthist, ARHistBucket bucket, bool FinFlag, GLTran tran)
			where History : class, ICuryARHist, IBaseARHist
		{
			if (_IsIntegrityCheck == false || accthist.DetDeleted == false)
			{
				UpdateHist<History>(accthist, bucket, FinFlag, tran);

				accthist.FinFlag = FinFlag;
				accthist.CuryPtdPayments += bucket.SignPayments * (tran.CuryDebitAmt - tran.CuryCreditAmt);
				accthist.CuryPtdSales += bucket.SignSales * (tran.CuryDebitAmt - tran.CuryCreditAmt);
				accthist.CuryPtdDrAdjustments += bucket.SignDrMemos * (tran.CuryDebitAmt - tran.CuryCreditAmt);
				accthist.CuryPtdCrAdjustments += bucket.SignCrMemos * (tran.CuryDebitAmt - tran.CuryCreditAmt);
				accthist.CuryPtdFinCharges += bucket.SignFinCharges * (tran.CuryDebitAmt - tran.CuryCreditAmt);
				accthist.CuryPtdDiscounts += bucket.SignDiscTaken * (tran.CuryDebitAmt - tran.CuryCreditAmt);
				accthist.CuryYtdBalance += bucket.SignPtd * (tran.CuryDebitAmt - tran.CuryCreditAmt);
				accthist.CuryPtdDeposits += bucket.SignDeposits * (tran.CuryDebitAmt - tran.CuryCreditAmt);
				accthist.CuryYtdDeposits += bucket.SignDeposits * (tran.CuryDebitAmt - tran.CuryCreditAmt);
			}
		}

		private void CuryUpdateFinHist<History>(History accthist, ARHistBucket bucket, GLTran tran)
			where History : class, ICuryARHist, IBaseARHist
		{
			CuryUpdateHist<History>(accthist, bucket, true, tran);
		}

		private void CuryUpdateTranHist<History>(History accthist, ARHistBucket bucket, GLTran tran)
			where History : class, ICuryARHist, IBaseARHist
		{
			CuryUpdateHist<History>(accthist, bucket, false, tran);
		}


        protected void UpdateItemDiscountsHistory(ARTran tran, ARRegister ardoc)
        {
            ARHistBucket bucket = new ARHistItemDiscountsBucket(tran);
            {
                ARHist accthist = CreateHistory(tran.BranchID, ardoc.ARAccountID, ardoc.ARSubID, ardoc.CustomerID, ardoc.FinPeriodID);
                if (accthist != null)
                {
                    UpdateFinHist<ARHist>(accthist, bucket, new GLTran { DebitAmt = tran.DiscAmt, CreditAmt = 0m });
                }
            }

            {
                ARHist accthist = CreateHistory(tran.BranchID, ardoc.ARAccountID, ardoc.ARSubID, ardoc.CustomerID, ardoc.TranPeriodID);
                if (accthist != null)
                {
                    UpdateTranHist<ARHist>(accthist, bucket, new GLTran { DebitAmt = tran.DiscAmt, CreditAmt = 0m });
                }
            }
        }

		private void UpdateHistory(GLTran tran, Customer cust)
		{
            string HistTranType = tran.TranType;
            if (tran.TranType == ARDocType.VoidPayment)
            {
                ARRegister doc = PXSelect<ARRegister, Where<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>, And<Where<ARRegister.docType, Equal<ARDocType.payment>, Or<ARRegister.docType, Equal<ARDocType.prepayment>>>>>, OrderBy<Asc<Switch<Case<Where<ARRegister.docType, Equal<ARDocType.payment>>, int0>, int1>, Asc<ARRegister.docType, Asc<ARRegister.refNbr>>>>>.Select(this, tran.RefNbr);
                if (doc != null)
                {
                    HistTranType = doc.DocType;
                }
            }

            ARHistBucket bucket = new ARHistBucket(tran, HistTranType);
			{
				ARHist accthist = CreateHistory(tran.BranchID, bucket.arAccountID, bucket.arSubID, cust.BAccountID, tran.FinPeriodID);
				if (accthist != null)
				{
					UpdateFinHist<ARHist>(accthist, bucket, tran);
				}
			}

			{
				ARHist accthist = CreateHistory(tran.BranchID, bucket.arAccountID, bucket.arSubID, cust.BAccountID, tran.TranPeriodID);
				if (accthist != null)
				{
					UpdateTranHist<ARHist>(accthist, bucket, tran);
				}
			}
		}

		private void UpdateHistory(GLTran tran, Customer cust, CurrencyInfo info)
		{
            string HistTranType = tran.TranType;
            if (tran.TranType == ARDocType.VoidPayment)
            {
                ARRegister doc = PXSelect<ARRegister, Where<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>, And<Where<ARRegister.docType, Equal<ARDocType.payment>, Or<ARRegister.docType, Equal<ARDocType.prepayment>>>>>, OrderBy<Asc<Switch<Case<Where<ARRegister.docType, Equal<ARDocType.payment>>, int0>, int1>, Asc<ARRegister.docType, Asc<ARRegister.refNbr>>>>>.Select(this, tran.RefNbr);
                if (doc != null)
                {
                    HistTranType = doc.DocType;
                }
            }

            ARHistBucket bucket = new ARHistBucket(tran, HistTranType);
			{
				CuryARHist accthist = CreateHistory(tran.BranchID, bucket.arAccountID, bucket.arSubID, cust.BAccountID, info.CuryID, tran.FinPeriodID);
				if (accthist != null)
				{
					CuryUpdateFinHist<CuryARHist>(accthist, bucket, tran);
				}
			}

			{
				CuryARHist accthist = CreateHistory(tran.BranchID, bucket.arAccountID, bucket.arSubID, cust.BAccountID, info.CuryID, tran.TranPeriodID);
				if (accthist != null)
				{
					CuryUpdateTranHist<CuryARHist>(accthist, bucket, tran);
				}
			}
		}

		private List<ARRegister> CreateInstallments(PXResult<ARInvoice, CurrencyInfo, Terms, Customer> res)
		{
			ARInvoice ardoc = (ARInvoice)res;
			CurrencyInfo info = (CurrencyInfo)res;
			Terms terms = (Terms)res;
			Customer customer = (Customer)res;
			List<ARRegister> ret = new List<ARRegister>();

			decimal CuryTotalInstallments = 0m;

			ARInvoiceEntry docgraph = PXGraph.CreateInstance<ARInvoiceEntry>();

			PXResultset<TermsInstallments> installments = TermsAttribute.SelectInstallments(this, terms, (DateTime)ardoc.DueDate);
			foreach (TermsInstallments inst in installments)
			{
				docgraph.customer.Current = customer;
				PXCache sender = ARInvoice_DocType_RefNbr.Cache;
				//force precision population
				object CuryOrigDocAmt = sender.GetValueExt(ardoc, "CuryOrigDocAmt");

				CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(info);
				new_info.CuryInfoID = null;
				new_info = docgraph.currencyinfo.Insert(new_info);

				ARInvoice new_ardoc = PXCache<ARInvoice>.CreateCopy(ardoc);
				new_ardoc.CuryInfoID = new_info.CuryInfoID;
				new_ardoc.DueDate = ((DateTime)new_ardoc.DueDate).AddDays((double)inst.InstDays);
				new_ardoc.DiscDate = new_ardoc.DueDate;
				new_ardoc.InstallmentNbr = inst.InstallmentNbr;
				new_ardoc.MasterRefNbr = new_ardoc.RefNbr;
				new_ardoc.RefNbr = null;
				new_ardoc.ProjectID = ProjectDefaultAttribute.NonProject(docgraph);
				TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(docgraph.Transactions.Cache, null, TaxCalc.NoCalc);

				if (inst.InstallmentNbr == installments.Count)
				{
					new_ardoc.CuryOrigDocAmt = new_ardoc.CuryOrigDocAmt - CuryTotalInstallments;
				}
				else
				{
					if (terms.InstallmentMthd == TermsInstallmentMethod.AllTaxInFirst)
					{
						new_ardoc.CuryOrigDocAmt = PXDBCurrencyAttribute.Round(sender, ardoc, (decimal)((ardoc.CuryOrigDocAmt - ardoc.CuryTaxTotal) * inst.InstPercent / 100m), CMPrecision.TRANCURY);
						if (inst.InstallmentNbr == 1)
						{
							new_ardoc.CuryOrigDocAmt += (decimal)ardoc.CuryTaxTotal;
						}
					}
					else
					{
						new_ardoc.CuryOrigDocAmt = PXDBCurrencyAttribute.Round(sender, ardoc, (decimal)(ardoc.CuryOrigDocAmt * inst.InstPercent / 100m), CMPrecision.TRANCURY);
					}
				}
				new_ardoc.CuryDocBal = new_ardoc.CuryOrigDocAmt;
				new_ardoc.CuryLineTotal = new_ardoc.CuryOrigDocAmt;
				new_ardoc.CuryTaxTotal = 0m;
				new_ardoc.CuryOrigDiscAmt = 0m;
				new_ardoc = docgraph.Document.Insert(new_ardoc);
				CuryTotalInstallments += (decimal)new_ardoc.CuryOrigDocAmt;

				ARTran new_artran = new ARTran();
				new_artran.AccountID = new_ardoc.ARAccountID;
				new_artran.SubID = new_ardoc.ARSubID;
				new_artran.CuryTranAmt = new_ardoc.CuryOrigDocAmt;
				new_artran.TranDesc = Messages.MultiplyInstallmentsTranDesc;

				docgraph.Transactions.Insert(new_artran);

				foreach (ARSalesPerTran sptran in docgraph.salesPerTrans.Select())
				{
					docgraph.salesPerTrans.Delete(sptran);
				}

				foreach (ARSalesPerTran sptran in PXSelect<ARSalesPerTran, Where<ARSalesPerTran.docType, Equal<Required<ARSalesPerTran.docType>>, And<ARSalesPerTran.refNbr, Equal<Required<ARSalesPerTran.refNbr>>>>>.Select(this, ardoc.DocType, ardoc.RefNbr))
				{
					ARSalesPerTran new_sptran = PXCache<ARSalesPerTran>.CreateCopy(sptran);
					new_sptran.RefNbr = null;
					new_sptran.CuryInfoID = new_info.CuryInfoID;                    

					new_sptran.RefCntr = 999;
					new_sptran.CuryCommnblAmt = PXDBCurrencyAttribute.Round(sender, ardoc, (decimal)(sptran.CuryCommnblAmt * inst.InstPercent / 100m), CMPrecision.TRANCURY);
					new_sptran.CuryCommnAmt = PXDBCurrencyAttribute.Round(sender, ardoc, (decimal)(sptran.CuryCommnAmt * inst.InstPercent / 100m), CMPrecision.TRANCURY);
					new_sptran = docgraph.salesPerTrans.Insert(new_sptran);
				}

				docgraph.Save.Press();

				ret.Add((ARRegister)docgraph.Document.Current);

				docgraph.Clear();
			}

			if (installments.Count > 0)
			{
                docgraph.Document.WhereNew<Where<ARInvoice.docType, Equal<Optional<ARInvoice.docType>>>>();
				docgraph.Document.Search<ARInvoice.refNbr>(ardoc.RefNbr, ardoc.DocType);
				docgraph.Document.Current.InstallmentCntr = Convert.ToInt16(installments.Count);
				docgraph.Document.Cache.SetStatus(docgraph.Document.Current, PXEntryStatus.Updated);

				docgraph.Save.Press();
				docgraph.Clear();
			}

			return ret;
		}

		private void CreatePayment(PXResult<ARInvoice, CurrencyInfo, Terms, Customer> res, ref List<ARRegister> ret)
		{
			if (ret == null)
			{
				ret = new List<ARRegister>();
			}

			ARPaymentEntry docgraph = PXGraph.CreateInstance<ARPaymentEntry>();
			docgraph.AutoPaymentApp = true;
			docgraph.arsetup.Current.HoldEntry = false;
			docgraph.arsetup.Current.RequireControlTotal = false;

			ARInvoice invoice = PXCache<ARInvoice>.CreateCopy(res);
			
			PXResultset<SOInvoice> invoicesNotCSL = PXSelectJoin<SOInvoice,
				InnerJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<SOInvoice.pMInstanceID>>,
					InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>>,
						LeftJoin<ARPayment, On<ARPayment.docType, Equal<SOInvoice.docType>,
							And<ARPayment.refNbr, Equal<SOInvoice.refNbr>>>>>>,
				Where<SOInvoice.docType, Equal<Required<SOInvoice.docType>>,
					And<SOInvoice.refNbr, Equal<Required<SOInvoice.refNbr>>,
						And<PaymentMethod.paymentType, Equal<PaymentMethodType.creditCard>,
							And<ARPayment.refNbr, IsNull>>>>>.Select(docgraph, invoice.DocType, invoice.RefNbr);

			if (invoicesNotCSL.Count() > 0)
			{
				foreach (SOInvoice soinvoice in invoicesNotCSL)
				{
					PXSelectBase<CCProcTran> cmd = new PXSelectJoin<CCProcTran,
						LeftJoin<SOOrderShipment, On<CCProcTran.origDocType, Equal<SOOrderShipment.orderType>,
							And<CCProcTran.origRefNbr, Equal<SOOrderShipment.orderNbr>>>>,
						Where<CCProcTran.docType, Equal<Required<CCProcTran.docType>>,
							And<CCProcTran.refNbr, Equal<Required<CCProcTran.refNbr>>,
								Or<Where<SOOrderShipment.invoiceType, Equal<Required<SOOrderShipment.invoiceType>>,
									And<SOOrderShipment.invoiceNbr, Equal<Required<SOOrderShipment.invoiceNbr>>,
										And<CCProcTran.refNbr, IsNull,
											And<CCProcTran.docType, IsNull>>>>>>>,
						OrderBy<Desc<CCProcTran.tranNbr>>>(this);

					CCPaymentEntry.UpdateCapturedState<SOInvoice>(soinvoice, cmd.Select(soinvoice.DocType, soinvoice.RefNbr, soinvoice.DocType, soinvoice.RefNbr));

					if (soinvoice.IsCCCaptured == true)
					{
						if (((Terms) res).InstallmentType != TermsInstallmentType.Single)
						{
							throw new PXException(Messages.PrepaymentAppliedToMultiplyInstallments);
						}

						ARPayment payment = new ARPayment()
							{
								DocType = ARDocType.Payment,
								AdjDate = soinvoice.AdjDate,
								AdjFinPeriodID = soinvoice.AdjFinPeriodID
							};

						payment = PXCache<ARPayment>.CreateCopy(docgraph.Document.Insert(payment));
						payment.CustomerID = invoice.CustomerID;
						payment.CustomerLocationID = invoice.CustomerLocationID;
						payment.ARAccountID = invoice.ARAccountID;
						payment.ARSubID = invoice.ARSubID;

						payment.PaymentMethodID = soinvoice.PaymentMethodID;
						payment.PMInstanceID = soinvoice.PMInstanceID;
						payment.CashAccountID = soinvoice.CashAccountID;
						payment.ExtRefNbr = soinvoice.ExtRefNbr ?? string.Format(Messages.ARAutoPaymentRefNbrFormat, payment.DocDate);
						payment.CuryOrigDocAmt = soinvoice.CuryCCCapturedAmt;

						docgraph.Document.Update(payment);

						invoice.Released = true;
						invoice.OpenDoc = true;
						invoice.CuryDocBal = invoice.CuryOrigDocAmt;
						invoice.DocBal = invoice.OrigDocAmt;
						invoice.CuryDiscBal = invoice.CuryOrigDiscAmt;
						invoice.DiscBal = invoice.OrigDiscAmt;

						docgraph.Caches[typeof (ARInvoice)].SetStatus(invoice, PXEntryStatus.Held);


						decimal? _CuryAdjgAmt = payment.CuryOrigDocAmt > invoice.CuryDocBal ? invoice.CuryDocBal : payment.CuryOrigDocAmt;
						decimal? _CuryAdjgDiscAmt = payment.CuryOrigDocAmt > invoice.CuryDocBal ? 0m : invoice.CuryDiscBal;

						if (_CuryAdjgDiscAmt + _CuryAdjgAmt > invoice.CuryDocBal)
						{
							_CuryAdjgDiscAmt = invoice.CuryDocBal - _CuryAdjgAmt;
						}

						ARAdjust adj = new ARAdjust()
							{
								AdjdDocType = soinvoice.DocType,
								AdjdRefNbr = soinvoice.RefNbr,
								CuryAdjgAmt = _CuryAdjgAmt,
								CuryAdjgDiscAmt = _CuryAdjgDiscAmt
							};

						docgraph.Adjustments.Insert(adj);

						using (PXTransactionScope ts = new PXTransactionScope())
						{
							docgraph.Save.Press();

							PXDatabase.Update<CCProcTran>(
								new PXDataFieldAssign("DocType", docgraph.Document.Current.DocType),
								new PXDataFieldAssign("RefNbr", docgraph.Document.Current.RefNbr),
								new PXDataFieldRestrict("DocType", PXDbType.Char, 3, soinvoice.DocType, PXComp.EQ),
								new PXDataFieldRestrict("RefNbr", PXDbType.NVarChar, 15, soinvoice.RefNbr, PXComp.EQ)
								);

							int i = 0;
							bool ccproctranupdated = false;

							foreach (SOOrderShipment order in PXSelect<SOOrderShipment,
								Where<SOOrderShipment.invoiceType, Equal<Required<SOOrderShipment.invoiceType>>,
									And<SOOrderShipment.invoiceNbr, Equal<Required<SOOrderShipment.invoiceNbr>>>>>.Select(docgraph, soinvoice.DocType, soinvoice.RefNbr))
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
									throw new PXException(Messages.ERR_CCMultiplyPreauthCombined);
								}

								i++;
							}

							ts.Complete();
						}

						ret.Add(docgraph.Document.Current);

						docgraph.Clear();
					}
				}
			}
			else
			{
				PXResultset<SOInvoice> invoicesCSL = PXSelectJoin<SOInvoice,
				InnerJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<SOInvoice.pMInstanceID>>,
					InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>>,
						LeftJoin<ARPayment, On<ARPayment.docType, Equal<SOInvoice.docType>,
							And<ARPayment.refNbr, Equal<SOInvoice.refNbr>>>>>>,
				Where<SOInvoice.docType, Equal<Required<SOInvoice.docType>>,
					And<SOInvoice.refNbr, Equal<Required<SOInvoice.refNbr>>,
						And<PaymentMethod.paymentType, Equal<PaymentMethodType.creditCard>,
							And<ARPayment.refNbr, IsNotNull>>>>>.Select(docgraph, invoice.DocType, invoice.RefNbr);

				foreach (PXResult<SOInvoice, CustomerPaymentMethod, PaymentMethod, ARPayment> csls in invoicesCSL)
				{
					SOInvoice currInvoice = csls;
					ARPayment currPayment = csls;

					using (PXTransactionScope ts = new PXTransactionScope())
					{
						PXDatabase.Update<CCProcTran>(
							new PXDataFieldAssign("DocType", currPayment.DocType),
							new PXDataFieldAssign("RefNbr", currPayment.RefNbr),
							new PXDataFieldRestrict("DocType", PXDbType.Char, 3, currInvoice.DocType, PXComp.EQ),
							new PXDataFieldRestrict("RefNbr", PXDbType.NVarChar, 15, currInvoice.RefNbr, PXComp.EQ)
							);

						int i = 0;
						bool ccproctranupdated = false;

						foreach (SOOrderShipment order in PXSelect<SOOrderShipment,
							Where<SOOrderShipment.invoiceType, Equal<Required<SOOrderShipment.invoiceType>>,
								And<SOOrderShipment.invoiceNbr, Equal<Required<SOOrderShipment.invoiceNbr>>>>>.Select(docgraph, currInvoice.DocType, currInvoice.RefNbr))
						{
							ccproctranupdated |= PXDatabase.Update<CCProcTran>(
								new PXDataFieldAssign("DocType", currPayment.DocType),
								new PXDataFieldAssign("RefNbr", currPayment.RefNbr),
								new PXDataFieldRestrict("OrigDocType", PXDbType.Char, 3, order.OrderType, PXComp.EQ),
								new PXDataFieldRestrict("OrigRefNbr", PXDbType.NVarChar, 15, order.OrderNbr, PXComp.EQ),
								new PXDataFieldRestrict("RefNbr", PXDbType.NVarChar, 15, null, PXComp.ISNULL)
								);

							if (ccproctranupdated && i > 0)
							{
								throw new PXException(Messages.ERR_CCMultiplyPreauthCombined);
							}

							i++;
						}

						ts.Complete();
					}
				}

			}
		}
			
		private void UpdateARBalancesDates(ARRegister ardoc)
		{
			ARBalances arbal = new ARBalances();
			arbal.BranchID = ardoc.BranchID;
			arbal.CustomerID = ardoc.CustomerID;
			arbal.CustomerLocationID = ardoc.CustomerLocationID;
			arbal = (ARBalances)Caches[typeof(ARBalances)].Insert(arbal);

			if (ardoc.DueDate != null && (arbal.OldInvoiceDate == null || ((DateTime)ardoc.DueDate) <= ((DateTime)arbal.OldInvoiceDate)))
			{
				if (ardoc.OpenDoc == true)
				{
					arbal.OldInvoiceDate = ardoc.DueDate;
				}
			}
		}

		private void UpdateARBalancesDates(ARInvoice ardoc, Int32 rowcount)
		{
			ARBalances arbal = new ARBalances();
			arbal.BranchID = ardoc.BranchID;
			arbal.CustomerID = ardoc.CustomerID;
			arbal.CustomerLocationID = ardoc.CustomerLocationID;
			arbal = (ARBalances)Caches[typeof(ARBalances)].Insert(arbal);

			if (ardoc.DueDate != null && (arbal.OldInvoiceDate == null || ((DateTime)ardoc.DueDate) <= ((DateTime)arbal.OldInvoiceDate)))
			{
				if ((bool)ardoc.OpenDoc)
				{
					arbal.OldInvoiceDate = ardoc.DueDate;
				}
				else
				{
					if (arbal.DatesUpdated != true)
					{
						arbal.tstamp = PXDatabase.SelectTimeStamp();
						arbal.DatesUpdated = true;
					}
					arbal.OldInvoiceDate = null;

					foreach (ARInvoice olddoc in PXSelectReadonly<ARInvoice,
						Where<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>,
						And<ARInvoice.customerLocationID, Equal<Required<ARInvoice.customerLocationID>>,
						And<ARInvoice.branchID, Equal<Required<ARInvoice.branchID>>,
						And<ARInvoice.released, Equal<boolTrue>,
						And<ARInvoice.openDoc, Equal<boolTrue>,
						And<ARInvoice.dueDate, IsNotNull>>>>>>,
						OrderBy<Asc<ARInvoice.dueDate>>>
						.SelectWindowed(this, 0, rowcount + 1, ardoc.CustomerID, ardoc.CustomerLocationID, ardoc.BranchID))
					{
						ARRegister cached = (ARRegister)ARDocument.Cache.Locate(olddoc);
						if (cached == null || (bool)cached.OpenDoc)
						{
							arbal.OldInvoiceDate = olddoc.DueDate;
							break;
						}
					}
				}
			}
		}

        public static decimal? RoundAmount(decimal? amount, string RoundType, decimal? precision)
        {
            decimal? toround = amount / precision;

            switch (RoundType)
            {
                case "F":
                    return Math.Floor((decimal)toround) * precision;
                case "C":
                    return Math.Ceiling((decimal)toround) * precision;
                case "R":
                    return Math.Round((decimal)toround, 0, MidpointRounding.AwayFromZero) * precision;
                default:
                    return amount;
            }
        }

        protected virtual decimal? RoundAmount(decimal? amount)
        {
            return RoundAmount(amount, this.InvoiceRounding, this.InvoicePrecision);
        }

		public virtual List<ARRegister> ReleaseDocProc(JournalEntry je, ref ARRegister doc, PXResult<ARInvoice, CurrencyInfo, Terms, Customer> res, out PM.PMRegister pmDoc)
		{
			pmDoc = null;
			ARInvoice ardoc = (ARInvoice)res;
			CurrencyInfo info = (CurrencyInfo)res;
			Terms terms = (Terms)res;
			Customer vend = (Customer)res;

			List<ARRegister> ret = null;

			if (_IsIntegrityCheck == false && ardoc.DocType == ARDocType.FinCharge)
			{
				foreach (ARTran fcARTran in ARTran_TranType_RefNbr.Select((object)ardoc.DocType, ardoc.RefNbr))
				{
					String doc_type = fcARTran.TranDesc.Substring(0, 3);
					String reference = fcARTran.TranDesc.Substring(4, fcARTran.TranDesc.Length - 4);

					ARInvoiceEntry ie = PXGraph.CreateInstance<ARInvoiceEntry>();
					ie.Clear();


					ARInvoice inv_to_charge = PXSelect<ARInvoice,
																			Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>,
																				And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>
																	 .Select(ie, doc_type, reference);
					if (inv_to_charge != null)
					{
						inv_to_charge.LastFinChargeDate = ardoc.DocDate;
						ie.Document.Cache.Update(inv_to_charge);
						ie.Save.Press();
					}
				}

			}

			if ((bool)doc.Released == false)
			{
				if (_IsIntegrityCheck == false)
				{
					if ((bool)arsetup.PrintBeforeRelease && ardoc.PrintInvoice == true)
						throw new PXException(Messages.Invoice_NotPrinted_CannotRelease);				
					if ((bool)arsetup.EmailBeforeRelease && ardoc.EmailInvoice == true)
						throw new PXException(Messages.Invoice_NotEmailed_CannotRelease);
				}

				string _InstallmentType = terms.InstallmentType;

				if (_IsIntegrityCheck && ardoc.InstallmentNbr == null)
				{
					//ARInvoice instdoc = PXSelect<ARInvoice, Where<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>, And<ARInvoice.masterRefNbr, Equal<Required<ARInvoice.masterRefNbr>>, And<ARInvoice.installmentNbr, IsNotNull>>>>.Select(this, doc.CustomerID, doc.RefNbr);
					//if (instdoc != null)
					if (ardoc.InstallmentCntr != null)
					{
						_InstallmentType = TermsInstallmentType.Multiple;
					}
					else
					{
						_InstallmentType = TermsInstallmentType.Single;
					}
				}

				if (_InstallmentType == TermsInstallmentType.Multiple && (ardoc.DocType == ARDocType.CashSale || ardoc.DocType == ARDocType.CashReturn))
				{
					throw new PXException(Messages.Cash_Sale_Cannot_Have_Multiply_Installments);
				}

				if (_InstallmentType == TermsInstallmentType.Multiple && ardoc.InstallmentNbr == null)
				{
					if (_IsIntegrityCheck == false)
					{
						ret = CreateInstallments(res);
					}
					doc.CuryDocBal = 0m;
					doc.DocBal = 0m;
					doc.CuryDiscBal = 0m;
					doc.DiscBal = 0m;
					doc.CuryDiscTaken = 0m;
					doc.DiscTaken = 0m;

					doc.OpenDoc = false;
					doc.ClosedFinPeriodID = doc.FinPeriodID;
					doc.ClosedTranPeriodID = doc.TranPeriodID;

					UpdateARBalances(this, doc, -1m * doc.OrigDocAmt, 0m);
				}
				else
				{
                    if (this.InvoiceRounding != RoundingType.Currency)
                    {
                        doc.CuryRoundDiff = doc.CuryOrigDocAmt;
                        doc.RoundDiff = doc.OrigDocAmt;

                        doc.CuryOrigDocAmt = RoundAmount(doc.CuryOrigDocAmt);
                        PXDBCurrencyAttribute.CalcBaseValues<ARRegister.curyOrigDocAmt>(this.ARDocument.Cache, doc);

                        doc.CuryRoundDiff -= doc.CuryOrigDocAmt;
                        doc.RoundDiff -= doc.OrigDocAmt;
                    }

					doc.CuryDocBal = doc.CuryOrigDocAmt;
					doc.DocBal = doc.OrigDocAmt;
					doc.CuryDiscBal = doc.CuryOrigDiscAmt;
					doc.DiscBal = doc.OrigDiscAmt;
					doc.CuryDiscTaken = 0m;
					doc.DiscTaken = 0m;
					doc.RGOLAmt = 0m;

					doc.OpenDoc = true;
					doc.ClosedFinPeriodID = null;
					doc.ClosedTranPeriodID = null;

					UpdateARBalancesDates(ardoc);
				}

				//should always restore ARRegister to ARInvoice after above assignments
				PXCache<ARRegister>.RestoreCopy(ardoc, doc);

				CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(info);
				new_info.CuryInfoID = null;
				new_info.ModuleCode = "GL";
				new_info.BaseCalc = false;
				new_info = je.currencyinfo.Insert(new_info) ?? new_info;

				if (ardoc.DocType == ARDocType.CashSale || ardoc.DocType == ARDocType.CashReturn)
				{
					GLTran tran = new GLTran();
					tran.SummPost = true;
					tran.ZeroPost = false;
					tran.BranchID = ardoc.BranchID;
					tran.AccountID = ardoc.ARAccountID;
					tran.SubID = ardoc.ARSubID;
					tran.CuryDebitAmt = (ardoc.DrCr == "D") ? 0m : ardoc.CuryOrigDocAmt + ardoc.CuryOrigDiscAmt;
					tran.DebitAmt = (ardoc.DrCr == "D") ? 0m : ardoc.OrigDocAmt + ardoc.OrigDiscAmt;
					tran.CuryCreditAmt = (ardoc.DrCr == "D") ? ardoc.CuryOrigDocAmt + ardoc.CuryOrigDiscAmt : 0m;
					tran.CreditAmt = (ardoc.DrCr == "D") ? ardoc.OrigDocAmt + ardoc.OrigDiscAmt : 0m;
					tran.TranType = ardoc.DocType;
					tran.TranClass = "N";
					tran.RefNbr = ardoc.RefNbr;
					tran.TranDesc = ardoc.DocDesc;
					tran.TranPeriodID = ardoc.TranPeriodID;
					tran.FinPeriodID = ardoc.FinPeriodID;
					tran.TranDate = ardoc.DocDate;
					tran.CuryInfoID = new_info.CuryInfoID;
					tran.Released = true;
					tran.ReferenceID = ardoc.CustomerID;

					//no history update should take place
					je.GLTranModuleBatNbr.Insert(tran);
				}
				else
				{
					GLTran tran = new GLTran();
					tran.SummPost = true;
					tran.BranchID = ardoc.BranchID;
					tran.AccountID = ardoc.ARAccountID;
					tran.SubID = ardoc.ARSubID;
					tran.CuryDebitAmt = (ardoc.DrCr == "D") ? 0m : ardoc.CuryOrigDocAmt;
					tran.DebitAmt = (ardoc.DrCr == "D") ? 0m : ardoc.OrigDocAmt - ardoc.RGOLAmt;
					tran.CuryCreditAmt = (ardoc.DrCr == "D") ? ardoc.CuryOrigDocAmt : 0m;
					tran.CreditAmt = (ardoc.DrCr == "D") ? ardoc.OrigDocAmt - ardoc.RGOLAmt : 0m;
					tran.TranType = ardoc.DocType;
					tran.TranClass = "N";
					tran.RefNbr = ardoc.RefNbr;
					tran.TranDesc = ardoc.DocDesc;
					tran.TranPeriodID = ardoc.TranPeriodID;
					tran.FinPeriodID = ardoc.FinPeriodID;
					tran.TranDate = ardoc.DocDate;
					tran.CuryInfoID = new_info.CuryInfoID;
					tran.Released = true;
					tran.ReferenceID = ardoc.CustomerID;

					//if (ardoc.InstallmentNbr == null || ardoc.InstallmentNbr == 0)
					if ((bool)doc.OpenDoc)
					{
						UpdateHistory(tran, vend);
						UpdateHistory(tran, vend, info);
					}

					je.GLTranModuleBatNbr.Insert(tran);
				}
				ARTran prev_n = new ARTran();

				foreach (PXResult<ARTran, ARTax, Tax, DRDeferredCode, PMTran, SO.SOOrderType> r in ARTran_TranType_RefNbr.Select((object)ardoc.DocType, ardoc.RefNbr))
				{
					ARTran n = r;
					SO.SOOrderType sotype = r;

					if (_IsIntegrityCheck == false && sotype.PostLineDiscSeparately == true && sotype.DiscountAcctID != null && n.CuryDiscAmt > 0.00005m)
					{
						ARTran new_tran = PXCache<ARTran>.CreateCopy(n);
						new_tran.InventoryID = null;
						new_tran.TaxCategoryID = null;
						new_tran.SalesPersonID = null;
						new_tran.UOM = null;
						new_tran.LineType = SO.SOLineType.Discount;
						new_tran.TranDesc = PXMessages.LocalizeNoPrefix(SO.Messages.LineDiscDescr);
						new_tran.LineNbr = (int?)PXLineNbrAttribute.NewLineNbr<ARTran.lineNbr>(ARTran_TranType_RefNbr.Cache, doc);
                        new_tran.CuryTranAmt = PXDBCurrencyAttribute.RoundCury<ARTran.curyInfoID>(ARTran_TranType_RefNbr.Cache, new_tran, (decimal)new_tran.CuryDiscAmt * (decimal)(n.CuryTranAmt != 0m && n.CuryTaxableAmt.GetValueOrDefault() != 0 ? n.CuryTaxableAmt / n.CuryTranAmt : 1m));
                        new_tran.CuryExtPrice = PXDBCurrencyAttribute.RoundCury<ARTran.curyInfoID>(ARTran_TranType_RefNbr.Cache, new_tran, (decimal)new_tran.CuryDiscAmt * (decimal)(n.CuryTranAmt != 0m && n.CuryTaxableAmt.GetValueOrDefault() != 0 ? n.CuryTaxableAmt / n.CuryTranAmt : 1m));
						PXDBCurrencyAttribute.CalcBaseValues<ARTran.curyTranAmt>(ARTran_TranType_RefNbr.Cache, new_tran);
						PXDBCurrencyAttribute.CalcBaseValues<ARTran.curyExtPrice>(ARTran_TranType_RefNbr.Cache, new_tran);
						new_tran.CuryDiscAmt = 0m;
						new_tran.Qty = 0m;
						new_tran.DiscPct = 0m;
						new_tran.CuryUnitPrice = 0m;
						new_tran.DetDiscIDC1 = null;
						new_tran.DetDiscIDC2 = null;
						new_tran.DetDiscSeqIDC1 = null;
						new_tran.DetDiscSeqIDC2 = null;
						new_tran.DocDiscIDC1 = null;
						new_tran.DocDiscIDC2 = null;
						new_tran.DocDiscSeqIDC1 = null;
						new_tran.DocDiscSeqIDC2 = null;

						ARTran_TranType_RefNbr.Cache.Insert(new_tran);

						new_tran = PXCache<ARTran>.CreateCopy(n);
						new_tran.InventoryID = null;
						new_tran.TaxCategoryID = null;
						new_tran.SalesPersonID = null;
						new_tran.UOM = null;
						new_tran.LineType = SO.SOLineType.Discount;
						new_tran.TranDesc = PXMessages.LocalizeNoPrefix(SO.Messages.LineDiscDescr);
						new_tran.AccountID = sotype.DiscountAcctID;
						new_tran.LineNbr = (int?)PXLineNbrAttribute.NewLineNbr<ARTran.lineNbr>(ARTran_TranType_RefNbr.Cache, doc);
                        new_tran.CuryTranAmt = -PXDBCurrencyAttribute.RoundCury<ARTran.curyInfoID>(ARTran_TranType_RefNbr.Cache, new_tran, (decimal)new_tran.CuryDiscAmt * (decimal)(n.CuryTranAmt != 0m && n.CuryTaxableAmt.GetValueOrDefault() != 0 ? n.CuryTaxableAmt / n.CuryTranAmt : 1m));
                        new_tran.CuryExtPrice = -PXDBCurrencyAttribute.RoundCury<ARTran.curyInfoID>(ARTran_TranType_RefNbr.Cache, new_tran, (decimal)new_tran.CuryDiscAmt * (decimal)(n.CuryTranAmt != 0m && n.CuryTaxableAmt.GetValueOrDefault() != 0 ? n.CuryTaxableAmt / n.CuryTranAmt : 1m));
						PXDBCurrencyAttribute.CalcBaseValues<ARTran.curyTranAmt>(ARTran_TranType_RefNbr.Cache, new_tran);
						PXDBCurrencyAttribute.CalcBaseValues<ARTran.curyExtPrice>(ARTran_TranType_RefNbr.Cache, new_tran);
						new_tran.CuryDiscAmt = 0m;
						new_tran.DiscPct = 0m;
						new_tran.Qty = 0m;
						new_tran.CuryUnitPrice = 0m;
						new_tran.DetDiscIDC1 = null;
						new_tran.DetDiscIDC2 = null;
						new_tran.DetDiscSeqIDC1 = null;
						new_tran.DetDiscSeqIDC2 = null;
						new_tran.DocDiscIDC1 = null;
						new_tran.DocDiscIDC2 = null;
						new_tran.DocDiscSeqIDC1 = null;
						new_tran.DocDiscSeqIDC2 = null;

                        if (sotype.UseDiscountSubFromSalesSub == false)
						{
                            Location customerloc = PXSelect<Location,
                                                            Where<Location.bAccountID, Equal<Required<ARInvoice.customerID>>,
                                                                And<Location.locationID, Equal<Required<ARInvoice.customerLocationID>>>>>
                                                       .Select(this, ardoc.CustomerID, ardoc.CustomerLocationID);
                            Location companyloc = (Location)PXSelectJoin<Location, 
                                                                         InnerJoin<BAccountR, On<Location.bAccountID, Equal<BAccountR.bAccountID>, 
                                                                             And<Location.locationID, Equal<BAccountR.defLocationID>>>, 
                                                                         InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>>, 
                                                                         Where<Branch.branchID, Equal<Required<ARRegister.branchID>>>>
                                                      .Select(this, ardoc.BranchID);
                            object ordertype_SubID = GetValue<SO.SOOrderType.discountSubID>(sotype);
                            object customer_Location = GetValue<Location.cDiscountSubID>(customerloc);
                            object company_Location = GetValue<Location.cMPDiscountSubID>(companyloc);

                            //if (customer_Location != null && company_Location != null)
                            {
                                object value = SO.SODiscSubAccountMaskAttribute.MakeSub<SO.SOOrderType.discSubMask>(this,
                                                                                                                    sotype.DiscSubMask,
                                                                                                                    new object[] 
                                                                                                                    { 
                                                                                                                        ordertype_SubID, 
                                                                                                                        customer_Location, 
                                                                                                                        company_Location 
                                                                                                                    },
                                                                                                                    new Type[] 
                                                                                                                    { 
                                                                                                                        typeof(SO.SOOrderType.discountSubID), 
                                                                                                                        typeof(Location.cDiscountSubID), 
                                                                                                                        typeof(Location.cMPDiscountSubID) 
                                                                                                                    });
                                ARTran_TranType_RefNbr.Cache.RaiseFieldUpdating<ARTran.subID>(new_tran, ref value);
                                new_tran.SubID = (int?)value;
                            }
						}

						ARTran_TranType_RefNbr.Cache.Insert(new_tran);
					}
				}

				List<PXResult<ARTran, PMTran>> creditMemoPMReversal = new List<PXResult<ARTran, PMTran>>();
				List<PXResult<ARTran, PMTran>> nonglBillLater = new List<PXResult<ARTran, PMTran>>();
				foreach (PXResult<ARTran, ARTax, Tax, DRDeferredCode, PMTran> r in ARTran_TranType_RefNbr.Select((object)ardoc.DocType, ardoc.RefNbr))
				{
					ARTran n = (ARTran)r;
					ARTax x = (ARTax)r;
					Tax salestax = (Tax)r;
					DRDeferredCode defcode = (DRDeferredCode)r;
					PMTran pmtran = r;

					if (!object.Equals(prev_n, n) && _IsIntegrityCheck == false && n.Released == true)
					{
						throw new PXException(Messages.Document_Status_Invalid);
					}

					if (!object.Equals(prev_n, n))
					{
						GLTran tran = new GLTran();
						tran.SummPost = this.SummPost;
						tran.BranchID = n.BranchID;
						tran.CuryInfoID = new_info.CuryInfoID;
						tran.TranType = n.TranType;
						tran.TranClass = ardoc.DocClass;
						tran.RefNbr = n.RefNbr;
						tran.InventoryID = n.InventoryID;
						tran.UOM = n.UOM;
						tran.Qty = (n.DrCr == "C") ? n.Qty : -1 * n.Qty;
						tran.TranDate = n.TranDate;
						tran.ProjectID = n.ProjectID;
						tran.TaskID = n.TaskID;
						tran.AccountID = n.AccountID;
                        tran.SubID = GetValueInt<ARTran.subID>(je, n);
						tran.TranDesc = n.TranDesc;
						tran.Released = true;
						tran.ReferenceID = ardoc.CustomerID;
						tran.TranLineNbr = (tran.SummPost == true) ? null : n.LineNbr;
						
						if (x != null && x.TaxID != null && salestax != null && salestax.TaxCalcLevel == "0")
						{
							//not supported yet, must change tax calculation
							tran.CuryDebitAmt = (n.DrCr == "D") ? x.CuryTaxableAmt : 0m;
							tran.DebitAmt = (n.DrCr == "D") ? x.TaxableAmt : 0m;
							tran.CuryCreditAmt = (n.DrCr == "D") ? 0m : x.CuryTaxableAmt;
							tran.CreditAmt = (n.DrCr == "D") ? 0m : x.TaxableAmt;
						}
						else
						{
							tran.CuryDebitAmt = (n.DrCr == "D") ? n.CuryTranAmt : 0m;
							tran.DebitAmt = (n.DrCr == "D") ? n.TranAmt : 0m;
							tran.CuryCreditAmt = (n.DrCr == "D") ? 0m : n.CuryTranAmt;
							tran.CreditAmt = (n.DrCr == "D") ? 0m : n.TranAmt;
						}

						if (_IsIntegrityCheck == false && (defcode != null && defcode.DeferredCodeID != null))
						{
							tran.AccountID = defcode.AccountID;
							tran.SubID = defcode.SubID;

							DRProcess dr = PXGraph.CreateInstance<DRProcess>();
							dr.CreateSchedule(n, defcode, ardoc);
							dr.Actions.PressSave();
						}

						je.GLTranModuleBatNbr.Insert(tran);

                        UpdateItemDiscountsHistory(n, ardoc);

						if (!_IsIntegrityCheck)
						{
							if (n.LineType == SO.SOLineType.Inventory || n.LineType == SO.SOLineType.NonInventory)
							{
								INTran intran = PXSelect<INTran,
									Where<INTran.sOShipmentType, Equal<Current<ARTran.sOShipmentType>>, And<INTran.sOShipmentNbr, Equal<Current<ARTran.sOShipmentNbr>>, And<INTran.sOShipmentLineNbr, Equal<Current<ARTran.sOShipmentLineNbr>>,
									And<INTran.sOOrderType, Equal<Current<ARTran.sOOrderType>>, And<INTran.sOOrderNbr, Equal<Current<ARTran.sOOrderNbr>>, And<INTran.sOOrderLineNbr, Equal<Current<ARTran.sOOrderLineNbr>>>>>>>>>.SelectMultiBound(this, new object[] { n });

								if (intran != null)
								{
									intran.ARDocType = n.TranType;
									intran.ARRefNbr = n.RefNbr;
									intran.ARLineNbr = n.LineNbr;
                                    intran.UnitPrice = n.UnitPrice;
                                    intran.TranAmt = n.TranAmt;

									this.intranselect.Cache.SetStatus(intran, PXEntryStatus.Updated);

									if (intran.Released == true)
									{
										n.TranCost = intran.TranCost;
									}
								}
							} 
						}

						if (pmtran.TranID != null)
						{
							if (pmtran.IsNonGL != true && pmtran.Reverse == PM.PMReverse.OnInvoice && pmtran.OffsetAccountID != null)
							{
								if (doc.DocType == ARDocType.CreditMemo)
								{
									creditMemoPMReversal.Add(new PXResult<ARTran, PMTran>(n, pmtran));
								}
								else if (pmtran.AccountID != pmtran.OffsetAccountID)
								{
									tran.PMTranID = null;
									tran.OrigPMTranID = pmtran.TranID;
									tran.TranLineNbr = null;

									decimal amount = pmtran.Amount.Value;
									if (n.TranAmt < pmtran.Amount && n.PMDeltaOption != ARTran.pMDeltaOption.CompleteLine)
									{
										amount = n.TranAmt.Value;
									}

									decimal curyval;
									PXDBCurrencyAttribute.CuryConvCury(je.GLTranModuleBatNbr.Cache, new_info, amount, out curyval);

									tran.AccountID = pmtran.OffsetAccountID;
									tran.SubID = pmtran.OffsetSubID;
									tran.CuryDebitAmt = curyval;
									tran.DebitAmt = amount;
									tran.CuryCreditAmt = 0m;
									tran.CreditAmt = 0m;

									je.GLTranModuleBatNbr.Insert(tran);

									tran.AccountID = pmtran.AccountID;
									tran.SubID = pmtran.SubID;
									tran.CuryDebitAmt = 0m;
									tran.DebitAmt = 0m;
									tran.CuryCreditAmt = curyval;
									tran.CreditAmt = amount;

									je.GLTranModuleBatNbr.Insert(tran);
								}
							}
							else if (pmtran.IsNonGL == true)
							{
								//NON-GL 
								if (n.TranAmt < pmtran.Amount && n.PMDeltaOption != ARTran.pMDeltaOption.CompleteLine)
								{
									nonglBillLater.Add(new PXResult<ARTran, PMTran>(n, pmtran));
								}
							}
						}

						n.Released = true;
						if (ARTran_TranType_RefNbr.Cache.GetStatus(n) == PXEntryStatus.Notchanged)
						{
							ARTran_TranType_RefNbr.Cache.SetStatus(n, PXEntryStatus.Updated);
						}
					}
					prev_n = n;
				}

				if (_IsIntegrityCheck == false)
				{
					if (creditMemoPMReversal.Count > 0)
					{
						PM.PMAllocator allocator = PXGraph.CreateInstance<PM.PMAllocator>();
						allocator.ReverseCreditMemo(doc.RefNbr, creditMemoPMReversal);
						allocator.Actions.PressSave();
						pmDoc = allocator.Document.Current;
					}
					else if (nonglBillLater.Count > 0)
					{
						PM.PMAllocator allocator = PXGraph.CreateInstance<PM.PMAllocator>();
						allocator.NonGlBillLater(doc.RefNbr, nonglBillLater);
						allocator.Actions.PressSave();
						pmDoc = allocator.Document.Current;
					}
				}
				foreach (ARTaxTran x in ARTaxTran_TranType_RefNbr.Select(ardoc.DocType, ardoc.RefNbr))
				{
					
					GLTran tran = new GLTran();
					tran.SummPost = this.SummPost;
					tran.BranchID = x.BranchID;
					tran.CuryInfoID = new_info.CuryInfoID;
					tran.TranType = x.TranType;
					tran.TranClass = "T";
					tran.RefNbr = x.RefNbr;
					tran.TranDate = x.TranDate;
					tran.AccountID = x.AccountID;
					tran.SubID = x.SubID;
					tran.TranDesc = x.TaxID;
					tran.CuryDebitAmt = (ardoc.DrCr == "D") ? x.CuryTaxAmt : 0m;
					tran.DebitAmt = (ardoc.DrCr == "D") ? x.TaxAmt : 0m;
					tran.CuryCreditAmt = (ardoc.DrCr == "D") ? 0m : x.CuryTaxAmt;
					tran.CreditAmt = (ardoc.DrCr == "D") ? 0m : x.TaxAmt;
					tran.Released = true;
					tran.ReferenceID = ardoc.CustomerID;

					je.GLTranModuleBatNbr.Insert(tran);
					
					x.Released = true;
					ARTaxTran_TranType_RefNbr.Cache.SetStatus(x, PXEntryStatus.Updated);
				}

				foreach (ARSalesPerTran n in ARDoc_SalesPerTrans.Select(doc.DocType, doc.RefNbr))
				{
					//multiply installments master and deferred revenue should not have commission
					n.Released = doc.OpenDoc;
					ARDoc_SalesPerTrans.Cache.Update(n);

					PXTimeStampScope.DuplicatePersisted(ARDoc_SalesPerTrans.Cache, n, typeof(ARSalesPerTran));
				}

				if (_IsIntegrityCheck == false)
				{
					foreach (PXResult<ARAdjust, ARPayment> appres in PXSelectJoin<ARAdjust, InnerJoin<ARPayment, On<ARPayment.docType, Equal<ARAdjust.adjgDocType>, And<ARPayment.refNbr, Equal<ARAdjust.adjgRefNbr>>>>, Where<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>, And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>, And<ARAdjust.released, Equal<boolFalse>, And<ARAdjust.voided, Equal<boolFalse>>>>>>.Select(this, doc.DocType, doc.RefNbr))
					{
						ARAdjust adj = (ARAdjust)appres;
						ARPayment payment = (ARPayment)appres;

						if (((ARAdjust)appres).CuryAdjdAmt > 0m)
						{
							if (_InstallmentType != TermsInstallmentType.Single)
							{
								throw new PXException(Messages.PrepaymentAppliedToMultiplyInstallments);
							}

							if (ret == null)
							{
								ret = new List<ARRegister>();
							}

							//are always greater then payments period
							if (DateTime.Compare((DateTime)payment.AdjDate, (DateTime)adj.AdjdDocDate) < 0)
							{
								payment.AdjDate = adj.AdjdDocDate;
								payment.AdjFinPeriodID = adj.AdjdFinPeriodID;
								payment.AdjTranPeriodID = adj.AdjdTranPeriodID;
							}

							if (payment.Released == true)
							{
								ret.Add(payment);
							}

							ARPayment_DocType_RefNbr.Cache.Update(payment);

							adj.AdjAmt += adj.RGOLAmt;
							adj.RGOLAmt = -adj.RGOLAmt;
							adj.Hold = false;

							ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.SetStatus(adj, PXEntryStatus.Updated);
						}
					}

					CreatePayment(res, ref ret);
				}

				Batch arbatch = je.BatchModule.Current;

                if (Math.Abs(Math.Round((decimal)(arbatch.CuryDebitTotal - arbatch.CuryCreditTotal), 4)) >= 0.00005m)
                {
                    GLTran tran = new GLTran();
                    tran.SummPost = true;
                    tran.BranchID = ardoc.BranchID;
                    Currency c = PXSelect<Currency, Where<Currency.curyID, Equal<Required<CurrencyInfo.curyID>>>>.Select(this, doc.CuryID);

                    if (Math.Sign((decimal)(arbatch.CuryDebitTotal - arbatch.CuryCreditTotal)) == 1)
                    {
                        tran.AccountID = c.RoundingGainAcctID;
                        tran.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingGainSubID>(je, tran.BranchID, c);
                        tran.CuryCreditAmt = Math.Round((decimal)(arbatch.CuryDebitTotal - arbatch.CuryCreditTotal), 4);
                        tran.CuryDebitAmt = 0m;
                    }
                    else
                    {
                        tran.AccountID = c.RoundingLossAcctID;
                        tran.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingLossSubID>(je, tran.BranchID, c);
                        tran.CuryCreditAmt = 0m;
                        tran.CuryDebitAmt = Math.Round((decimal)(arbatch.CuryCreditTotal - arbatch.CuryDebitTotal), 4);
                    }
                    tran.CreditAmt = 0m;
                    tran.DebitAmt = 0m;
                    tran.TranType = doc.DocType;
                    tran.RefNbr = doc.RefNbr;
                    tran.TranClass = "N";
                    tran.TranDesc = GL.Messages.RoundingDiff;
                    tran.LedgerID = arbatch.LedgerID;
                    tran.FinPeriodID = arbatch.FinPeriodID;
                    tran.TranDate = arbatch.DateEntered;
                    tran.Released = true;
                    tran.ReferenceID = ardoc.CustomerID;

                    CurrencyInfo infocopy = new CurrencyInfo();
                    infocopy = je.currencyinfo.Insert(infocopy) ?? infocopy;

                    tran.CuryInfoID = infocopy.CuryInfoID;
                    je.GLTranModuleBatNbr.Insert(tran);
                }

				if (Math.Abs(Math.Round((decimal)(arbatch.DebitTotal - arbatch.CreditTotal), 4)) >= 0.00005m)
				{
					GLTran tran = new GLTran();
					tran.SummPost = true;
					tran.BranchID = ardoc.BranchID;
					Currency c = PXSelect<Currency, Where<Currency.curyID, Equal<Required<CurrencyInfo.curyID>>>>.Select(this, doc.CuryID);

					if (Math.Sign((decimal)(arbatch.DebitTotal - arbatch.CreditTotal)) == 1)
					{
						tran.AccountID = c.RoundingGainAcctID;
                        tran.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingGainSubID>(je, tran.BranchID, c);
                        tran.CreditAmt = Math.Round((decimal)(arbatch.DebitTotal - arbatch.CreditTotal), 4);
						tran.DebitAmt = 0m;
					}
					else
					{
						tran.AccountID = c.RoundingLossAcctID;
                        tran.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingLossSubID>(je, tran.BranchID, c);
                        tran.CreditAmt = 0m;
						tran.DebitAmt = Math.Round((decimal)(arbatch.CreditTotal - arbatch.DebitTotal), 4);
					}
					tran.CuryCreditAmt = 0m;
					tran.CuryDebitAmt = 0m;
					tran.TranType = doc.DocType;
					tran.RefNbr = doc.RefNbr;
					tran.TranClass = "N";
					tran.TranDesc = GL.Messages.RoundingDiff;
					tran.LedgerID = arbatch.LedgerID;
					tran.FinPeriodID = arbatch.FinPeriodID;
					tran.TranDate = arbatch.DateEntered;
					tran.Released = true;
					tran.ReferenceID = ardoc.CustomerID;

					CurrencyInfo infocopy = new CurrencyInfo();
					infocopy = je.currencyinfo.Insert(infocopy) ?? infocopy;

					tran.CuryInfoID = infocopy.CuryInfoID;
					je.GLTranModuleBatNbr.Insert(tran);
				}

				if (doc.CuryDocBal == 0m)
				{
					doc.DocBal = 0m;
					doc.CuryDiscBal = 0m;
					doc.DiscBal = 0m;

					doc.OpenDoc = false;
					doc.ClosedFinPeriodID = doc.FinPeriodID;
					doc.ClosedTranPeriodID = doc.TranPeriodID;
				}
			}
			return ret;
		}

        protected object GetValue<Field>(object data)
            where Field : IBqlField
        {
            return this.Caches[typeof(Field).DeclaringType].GetValue(data, typeof(Field).Name);
        }

        public int? GetValueInt<SourceField>(PXGraph target, object item)
            where SourceField : IBqlField
        {
            PXCache source = this.Caches[typeof(SourceField).DeclaringType];
            PXCache dest = target.Caches[typeof(SourceField).DeclaringType];

            object value = source.GetValueExt<SourceField>(item);
            if (value is PXFieldState)
            {
                value = ((PXFieldState)value).Value;
            }

            if (value != null)
            {
                dest.RaiseFieldUpdating<SourceField>(item, ref value);
            }

            return (int?)value;
        }

		public static void UpdateARBalances(PXGraph graph, ARRegister ardoc, decimal? BalanceAmt)
		{
			//voided payment is both released and voided
			//voided invoice(previously scheduled) is not released and voided
			if ((bool)ardoc.Released && ardoc.Voided == false && ardoc.SignBalance != 0m)
			{
				UpdateARBalances(graph, ardoc, ardoc.SignBalance == 1m ? BalanceAmt : -BalanceAmt, 0m);
			}
			else if (ardoc.Hold == false && ardoc.Scheduled == false && ardoc.Voided == false && ardoc.SignBalance != 0m)
			{
				UpdateARBalances(graph, ardoc, 0m, ardoc.SignBalance == 1m ? BalanceAmt : -BalanceAmt);
			}
		}

		public static void UpdateARBalances(PXGraph graph, ARRegister ardoc, decimal? CurrentBal, decimal? UnreleasedBal)
		{
			if (ardoc.CustomerID != null && ardoc.CustomerLocationID != null)
			{
				ARBalances arbal = new ARBalances();
				arbal.BranchID = ardoc.BranchID;
				arbal.CustomerID = ardoc.CustomerID;
				arbal.CustomerLocationID = ardoc.CustomerLocationID;
				arbal = (ARBalances)graph.Caches[typeof(ARBalances)].Insert(arbal);

				arbal.CurrentBal += CurrentBal;
				arbal.UnreleasedBal += UnreleasedBal;
			}
		}

		public static void UpdateARBalances(PXGraph graph, ARInvoice ardoc, decimal? BalanceAmt)
		{
			//voided payment is both released and voided
			//voided invoice(previously scheduled) is not released and voided
			if ((bool)ardoc.Released && ardoc.Voided == false && ardoc.SignBalance != 0m)
			{
				UpdateARBalances(graph, ardoc, ardoc.SignBalance == 1m ? BalanceAmt : -BalanceAmt, 0m);
			}
			else if (ardoc.Hold == false && ardoc.CreditHold == false && ardoc.Scheduled == false && ardoc.Voided == false && ardoc.SignBalance != 0m)
			{
				UpdateARBalances(graph, ardoc, 0m, ardoc.SignBalance == 1m ? BalanceAmt : -BalanceAmt);
			}
		}

		public static void UpdateARBalances(PXGraph graph, SOOrder order, decimal? UnbilledAmount, decimal? UnshippedAmount)
		{
			if (order.CustomerID != null && order.CustomerLocationID != null)
			{
				ARBalances arbal = new ARBalances();
				arbal.BranchID = order.BranchID;
				arbal.CustomerID = order.CustomerID;
				arbal.CustomerLocationID = order.CustomerLocationID;
				arbal = (ARBalances)graph.Caches[typeof(ARBalances)].Insert(arbal);

				if (ARDocType.SignBalance(order.ARDocType) != 0m)
				{
					decimal? BalanceAmt;
					if (order.ShipmentCntr == 0)
					{
						BalanceAmt = UnbilledAmount;
					}
					else
					{
						BalanceAmt = UnshippedAmount;
						arbal.TotalOpenOrders += ARDocType.SignBalance(order.ARDocType) == 1m ? (UnbilledAmount - UnshippedAmount) : -(UnbilledAmount - UnshippedAmount);
					}

					if (order.Completed == false && order.Cancelled == false && order.Hold == false && order.CreditHold == false && order.InclCustOpenOrders == true)
					{
						arbal.TotalOpenOrders += ARDocType.SignBalance(order.ARDocType) == 1m ? BalanceAmt : -BalanceAmt;
					}
				}
			}
		}

		private void UpdateARBalances(ARAdjust adj, ARRegister ardoc)
		{
			if (object.Equals(ardoc.DocType, adj.AdjdDocType) && string.Equals(ardoc.RefNbr, adj.AdjdRefNbr, StringComparison.OrdinalIgnoreCase))
			{
				if (ardoc.CustomerID != null && ardoc.CustomerLocationID != null)
				{
					ARBalances arbal = new ARBalances();
					arbal.BranchID = ardoc.BranchID;
					arbal.CustomerID = ardoc.CustomerID;
					arbal.CustomerLocationID = ardoc.CustomerLocationID;
					arbal = (ARBalances)this.Caches[typeof(ARBalances)].Insert(arbal);

					arbal.CurrentBal += adj.AdjdTBSign * (adj.AdjAmt + adj.AdjDiscAmt + adj.AdjWOAmt) - adj.RGOLAmt;
				}
			}
			else if (object.Equals(ardoc.DocType, adj.AdjgDocType) && string.Equals(ardoc.RefNbr, adj.AdjgRefNbr, StringComparison.OrdinalIgnoreCase))
			{
				if (ardoc.CustomerID != null && ardoc.CustomerLocationID != null)
				{
					ARBalances arbal = new ARBalances();
					arbal.BranchID = ardoc.BranchID;
					arbal.CustomerID = ardoc.CustomerID;
					arbal.CustomerLocationID = ardoc.CustomerLocationID;
					arbal = (ARBalances)this.Caches[typeof(ARBalances)].Insert(arbal);

					arbal.CurrentBal += adj.AdjgTBSign * (adj.AdjAmt);
				}
			}
			else
			{
				throw new PXException();
			}
		}

		private void UpdateARBalances(ARRegister ardoc)
		{
			UpdateARBalances(this, ardoc, ardoc.OrigDocAmt);
		}

		private void UpdateARBalances(ARRegister ardoc, decimal? BalanceAmt)
		{
			UpdateARBalances(this, ardoc, BalanceAmt);
		}

		public virtual void UpdateBalances(ARAdjust adj, ARRegister adjddoc)
		{
			ARRegister ardoc = (ARRegister)adjddoc;
			ARRegister cached = (ARRegister)ARDocument.Cache.Locate(ardoc);
			if (cached != null)
			{
				ardoc = cached;
			}
			else if (_IsIntegrityCheck == true)
			{
				return;
			}

			if (_IsIntegrityCheck == false && adj.VoidAdjNbr != null)
			{
				ARAdjust voidadj = PXSelect<ARAdjust, Where2<Where<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>, Or<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>>>, And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>, And<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>, And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>, And<ARAdjust.adjNbr, Equal<Required<ARAdjust.adjNbr>>>>>>>>.Select(this, (adj.AdjgDocType == ARDocType.VoidPayment ? ARDocType.Prepayment : adj.AdjgDocType), (adj.AdjgDocType == ARDocType.VoidPayment ? ARDocType.Payment : adj.AdjgDocType), adj.AdjgRefNbr, adj.AdjdDocType, adj.AdjdRefNbr, adj.VoidAdjNbr);

				if (voidadj != null)
				{
					if ((bool)voidadj.Voided)
					{
						throw new PXException(Messages.DocumentApplicationAlreadyVoided);
					}

					voidadj.Voided = true;
					Caches[typeof(ARAdjust)].Update(voidadj);

					adj.AdjAmt = -voidadj.AdjAmt;
					adj.AdjDiscAmt = -voidadj.AdjDiscAmt;
					adj.AdjWOAmt = -voidadj.AdjWOAmt;
					adj.RGOLAmt = -voidadj.RGOLAmt;
					adj.CuryAdjdAmt = -voidadj.CuryAdjdAmt;
					adj.CuryAdjdDiscAmt = -voidadj.CuryAdjdDiscAmt;
					adj.CuryAdjdWOAmt = -voidadj.CuryAdjdWOAmt;
					adj.CuryAdjgAmt = -voidadj.CuryAdjgAmt;
					adj.CuryAdjgDiscAmt = -voidadj.CuryAdjgDiscAmt;
					adj.CuryAdjgWOAmt = -voidadj.CuryAdjgWOAmt;

					Caches[typeof(ARAdjust)].Update(adj);
				}
			}

			if (string.Equals(adj.AdjdDocType, ardoc.DocType) && string.Equals(adj.AdjdRefNbr, ardoc.RefNbr, StringComparison.OrdinalIgnoreCase))
			{
				ardoc.CuryDocBal -= (adj.CuryAdjdAmt + adj.CuryAdjdDiscAmt + adj.CuryAdjdWOAmt);
				ardoc.DocBal -= (adj.AdjAmt + adj.AdjDiscAmt + adj.AdjWOAmt + (adj.ReverseGainLoss == false ? adj.RGOLAmt : -adj.RGOLAmt));
				ardoc.CuryDiscBal -= adj.CuryAdjdDiscAmt;
				ardoc.DiscBal -= adj.AdjDiscAmt;
				ardoc.CuryDiscTaken += adj.CuryAdjdDiscAmt;
				ardoc.DiscTaken += adj.AdjDiscAmt;
				ardoc.RGOLAmt += adj.RGOLAmt;
			} 
			else if (string.Equals(adj.AdjgDocType, ardoc.DocType) && string.Equals(adj.AdjgRefNbr, ardoc.RefNbr, StringComparison.OrdinalIgnoreCase))
			{
				ardoc.CuryDocBal -= adj.CuryAdjgAmt;
				ardoc.DocBal -= adj.AdjAmt;
			}

			if (ardoc.CuryDiscBal == 0m)
			{
				ardoc.DiscBal = 0m;
			}

			if (_IsIntegrityCheck == false && ardoc.CuryDocBal < 0m)
			{
				throw new PXException(Messages.DocumentBalanceNegative);
			}

			if (_IsIntegrityCheck == false && adj.AdjgDocDate < adjddoc.DocDate)
			{
				throw new PXException(Messages.ApplDate_Less_DocDate, PXUIFieldAttribute.GetDisplayName<ARPayment.adjDate>(ARPayment_DocType_RefNbr.Cache));
			}

			if (_IsIntegrityCheck == false && string.Compare(adj.AdjgFinPeriodID, adjddoc.FinPeriodID) < 0)
			{
				throw new PXException(Messages.ApplPeriod_Less_DocPeriod, PXUIFieldAttribute.GetDisplayName<ARPayment.adjFinPeriodID>(ARPayment_DocType_RefNbr.Cache));
			}

			if (ardoc.CuryDocBal == 0m)
			{
				ardoc.CuryDiscBal = 0m;
				ardoc.DiscBal = 0m;
				ardoc.DocBal = 0m;
				ardoc.OpenDoc = false;
				string closedFinPeriodID = string.Empty;
				string closedTranPeriodID = string.Empty;

				foreach (ARAdjust adjmax in PXSelectGroupBy<ARAdjust, Where<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>, And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>, And<Where<ARAdjust.released, Equal<boolTrue>, Or<ARAdjust.adjNbr, Equal<Required<ARAdjust.adjNbr>>>>>>>, Aggregate<GroupBy<ARAdjust.adjdDocType, GroupBy<ARAdjust.adjdRefNbr, Max<ARAdjust.adjgFinPeriodID, Max<ARAdjust.adjgTranPeriodID>>>>>>.Select(this, ardoc.DocType, ardoc.RefNbr, adj.AdjNbr))
				{
					ardoc.ClosedFinPeriodID = closedFinPeriodID = adjmax.AdjgFinPeriodID;
					ardoc.ClosedTranPeriodID = closedTranPeriodID = adjmax.AdjgTranPeriodID;
				}

				foreach (ARAdjust adjmax in PXSelectGroupBy<ARAdjust, Where<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>, And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>, And<Where<ARAdjust.released, Equal<boolTrue>, Or<ARAdjust.adjNbr, Equal<Required<ARAdjust.adjNbr>>>>>>>, Aggregate<GroupBy<ARAdjust.adjgDocType, GroupBy<ARAdjust.adjgRefNbr, Max<ARAdjust.adjgFinPeriodID, Max<ARAdjust.adjgTranPeriodID>>>>>>.Select(this, ardoc.DocType, ardoc.RefNbr, adj.AdjNbr))
				{
					ardoc.ClosedFinPeriodID = string.Compare(closedFinPeriodID, adjmax.AdjgFinPeriodID) < 0 ? adjmax.AdjgFinPeriodID : closedFinPeriodID;
					ardoc.ClosedTranPeriodID = string.Compare(closedTranPeriodID, adjmax.AdjgTranPeriodID) < 0 ? adjmax.AdjgTranPeriodID : closedTranPeriodID;
				}
			}
			else
			{
				if (ardoc.CuryDocBal == ardoc.CuryOrigDocAmt)
				{
					ardoc.CuryDiscBal = ardoc.CuryOrigDiscAmt;
					ardoc.DiscBal = ardoc.OrigDiscAmt;
					ardoc.CuryDiscTaken = 0m;
					ardoc.DiscTaken = 0m;
				}

				ardoc.OpenDoc = true;
				ardoc.ClosedTranPeriodID = null;
				ardoc.ClosedFinPeriodID = null;
			}

			ARDocument.Cache.Update(ardoc);
		}

		private void UpdateVoidedCheck(ARPayment voidcheck, string OrigDocType)
		{
			foreach (PXResult<ARPayment, CurrencyInfo, Currency, Customer> res in ARPayment_DocType_RefNbr.Select(OrigDocType, voidcheck.RefNbr, voidcheck.CustomerID))
			{
				ARPayment payment = (ARPayment)res;
				if (_IsIntegrityCheck == false && string.Equals(voidcheck.ExtRefNbr, payment.ExtRefNbr, StringComparison.OrdinalIgnoreCase) == false)
				{
					throw new PXException(Messages.VoidAppl_CheckNbr_NotMatchOrigPayment);
				}

				ARRegister ardoc = (ARRegister)payment;
				ARRegister cached = (ARRegister)ARDocument.Cache.Locate(ardoc);
				if (cached != null)
				{
					ardoc = cached;
				}

				ardoc.Voided = true;
				ardoc.OpenDoc = false;
				ardoc.CuryDocBal = 0m;
				ardoc.DocBal = 0m;
				ardoc.ClosedTranPeriodID = voidcheck.TranPeriodID;
				ardoc.ClosedFinPeriodID = voidcheck.FinPeriodID;
				ARDocument.Cache.Update(ardoc);
			}
		}


		public virtual void ReleaseDocProc(JournalEntry je, ref ARRegister doc, PXResult<ARPayment, CurrencyInfo, Currency, Customer, CashAccount> res)
		{
			ARPayment ardoc = (ARPayment)res;
			CurrencyInfo info = (CurrencyInfo)res;
			Customer vend = (Customer)res;
			Currency paycury = (Currency)res;
			CashAccount cashacct = (CashAccount)res;

			CustomerClass custclass = (CustomerClass)PXSelectJoin<CustomerClass, InnerJoin<ARSetup, On<ARSetup.dfltCustomerClassID, Equal<CustomerClass.customerClassID>>>>.Select(this, null);

			CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(info);
			new_info.CuryInfoID = null;
			new_info.ModuleCode = "GL";
			new_info = je.currencyinfo.Insert(new_info) ?? new_info;

			if (doc.Released == false)
			{
                //should always restore ARRegister to ARPayment after invoice part release of cash sale
                PXCache<ARRegister>.RestoreCopy(ardoc, doc);

				if (doc.DocType != ARDocType.SmallBalanceWO)
				{
					GLTran tran = new GLTran();
					tran.SummPost = true;
					tran.BranchID = cashacct.BranchID;
                    tran.AccountID = cashacct.AccountID;
                    tran.SubID = cashacct.SubID;
					tran.CuryDebitAmt = (ardoc.DrCr == "D") ? ardoc.CuryOrigDocAmt : 0m;
					tran.DebitAmt = (ardoc.DrCr == "D") ? ardoc.OrigDocAmt : 0m;
					tran.CuryCreditAmt = (ardoc.DrCr == "D") ? 0m : ardoc.CuryOrigDocAmt;
					tran.CreditAmt = (ardoc.DrCr == "D") ? 0m : ardoc.OrigDocAmt;
					tran.TranType = ardoc.DocType;
					tran.TranClass = ardoc.DocClass;
					tran.RefNbr = ardoc.RefNbr;
					tran.TranDesc = ardoc.DocDesc;
					tran.TranDate = ardoc.DocDate;
					tran.TranPeriodID = ardoc.TranPeriodID;
					tran.FinPeriodID = ardoc.FinPeriodID;
					tran.CuryInfoID = new_info.CuryInfoID;
					tran.Released = true;
					tran.CATranID = ardoc.CATranID;
					tran.ReferenceID = ardoc.CustomerID;
				    tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(this);
					
					je.GLTranModuleBatNbr.Insert(tran);

					//Debit Payment AR Account
					tran = new GLTran();
					tran.SummPost = true;
					if (ardoc.DocType == ARDocType.CashSale || ardoc.DocType == ARDocType.CashReturn)
					{
						tran.ZeroPost = false;
					}
					tran.BranchID = ardoc.BranchID;
					tran.AccountID = ardoc.ARAccountID;
					tran.SubID = ardoc.ARSubID;
					tran.CuryDebitAmt = (ardoc.DrCr == "D") ? 0m : ardoc.CuryOrigDocAmt;
					tran.DebitAmt = (ardoc.DrCr == "D") ? 0m : ardoc.OrigDocAmt;
					tran.CuryCreditAmt = (ardoc.DrCr == "D") ? ardoc.CuryOrigDocAmt : 0m;
					tran.CreditAmt = (ardoc.DrCr == "D") ? ardoc.OrigDocAmt : 0m;
					tran.TranType = ardoc.DocType;
					tran.TranClass = "P";
					tran.RefNbr = ardoc.RefNbr;
					tran.TranDesc = ardoc.DocDesc;
					tran.TranDate = ardoc.DocDate;
					tran.TranPeriodID = ardoc.TranPeriodID;
					tran.FinPeriodID = ardoc.FinPeriodID;
					tran.CuryInfoID = new_info.CuryInfoID;
					tran.Released = true;
					tran.ReferenceID = ardoc.CustomerID;
					tran.ProjectID = ardoc.ProjectID;
					tran.TaskID = ardoc.TaskID;

					UpdateHistory(tran, vend);
					UpdateHistory(tran, vend, new_info);

					je.GLTranModuleBatNbr.Insert(tran);
				}

                foreach (ARPaymentChargeTran charge in ARPaymentChargeTran_DocType_RefNbr.Select(doc.DocType, doc.RefNbr))
                {
					bool isCADebit = charge.DrCr == "D" || doc.DocType == ARDocType.CashReturn;
					
					GLTran tran = new GLTran();
                    tran.SummPost = true;
                    tran.BranchID = cashacct.BranchID;
                    tran.AccountID = cashacct.AccountID;
                    tran.SubID = cashacct.SubID;
					tran.CuryDebitAmt = isCADebit ? charge.CuryTranAmt : 0m;
					tran.DebitAmt = isCADebit ? charge.TranAmt : 0m;
					tran.CuryCreditAmt = isCADebit ? 0m : charge.CuryTranAmt;
					tran.CreditAmt = isCADebit ? 0m : charge.TranAmt;
                    tran.TranType = charge.DocType;
                    tran.TranClass = ardoc.DocClass;
                    tran.RefNbr = charge.RefNbr;
                    tran.TranDesc = charge.TranDesc;
                    tran.TranDate = charge.TranDate;
                    tran.TranPeriodID = charge.TranPeriodID;
                    tran.FinPeriodID = charge.FinPeriodID;
                    tran.CuryInfoID = new_info.CuryInfoID;
                    tran.Released = true;
                    tran.CATranID = charge.CashTranID ?? ardoc.CATranID;
                    tran.ReferenceID = ardoc.CustomerID;

                    je.GLTranModuleBatNbr.Insert(tran);

                    tran = new GLTran();
                    tran.SummPost = true;
                    tran.ZeroPost = false;
                    tran.BranchID = ardoc.BranchID;
                    tran.AccountID = charge.AccountID;
                    tran.SubID = charge.SubID;
					tran.CuryDebitAmt = isCADebit ? 0m : charge.CuryTranAmt;
					tran.DebitAmt = isCADebit ? 0m : charge.TranAmt;
					tran.CuryCreditAmt = isCADebit ? charge.CuryTranAmt : 0m;
					tran.CreditAmt = isCADebit ? charge.TranAmt : 0m;
                    tran.TranType = charge.DocType;
                    tran.TranClass = "U";
                    tran.RefNbr = charge.RefNbr;
                    tran.TranDesc = charge.TranDesc;
                    tran.TranDate = charge.TranDate;
                    tran.TranPeriodID = charge.TranPeriodID;
                    tran.FinPeriodID = charge.FinPeriodID;
                    tran.CuryInfoID = new_info.CuryInfoID;
                    tran.Released = true;
                    tran.ReferenceID = ardoc.CustomerID;

                    je.GLTranModuleBatNbr.Insert(tran);
                }

				doc.CuryDocBal = doc.CuryOrigDocAmt;
				doc.DocBal = doc.OrigDocAmt;

				doc.Voided = false;
				doc.OpenDoc = true;
				doc.ClosedFinPeriodID = null;
				doc.ClosedTranPeriodID = null;

				if (ardoc.VoidAppl == true)
				{
					doc.OpenDoc = false;
					doc.ClosedFinPeriodID = doc.FinPeriodID;
					doc.ClosedTranPeriodID = doc.TranPeriodID;

					UpdateVoidedCheck(ardoc, ARDocType.Payment);
					UpdateVoidedCheck(ardoc, ARDocType.Prepayment);
				}
			}

			if (doc.DocType == ARDocType.CashSale || doc.DocType == ARDocType.CashReturn)
			{
				if (_IsIntegrityCheck == false)
				{
					ARAdjust adj = new ARAdjust();
					adj.AdjdDocType = doc.DocType;
					adj.AdjdRefNbr = doc.RefNbr;
					adj.AdjdBranchID = doc.BranchID;
					adj.AdjgDocType = doc.DocType;
					adj.AdjgRefNbr = doc.RefNbr;
					adj.AdjgBranchID = doc.BranchID;
					adj.AdjdARAcct = doc.ARAccountID;
					adj.AdjdARSub = doc.ARSubID;
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
					adj.RGOLAmt = 0m;
					adj.CuryAdjdAmt = doc.CuryOrigDocAmt;
					adj.CuryAdjdDiscAmt = doc.CuryOrigDiscAmt;
					adj.CuryAdjgAmt = doc.CuryOrigDocAmt;
					adj.CuryAdjgDiscAmt = doc.CuryOrigDiscAmt;
					adj.Released = false;
					adj.CustomerID = doc.CustomerID;
					ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.Insert(adj);
				}

				if (doc.DocType == ARDocType.CashReturn)
				{
					//UpdateVoidedCheck(ardoc, ARDocType.CashSale);
				}

				doc.CuryDocBal += doc.CuryOrigDiscAmt;
				doc.DocBal += doc.OrigDiscAmt;
				doc.ClosedFinPeriodID = doc.FinPeriodID;
				doc.ClosedTranPeriodID = doc.TranPeriodID;
			}

			doc.Released = true;

			ARAdjust prev_adj = new ARAdjust();
			CurrencyInfo prev_info = new CurrencyInfo();

			PXResultset<ARAdjust> adjustments = ARAdjust_AdjgDocType_RefNbr_CustomerID.Select(doc.DocType, doc.RefNbr, doc.LineCntr);

			foreach (PXResult<ARAdjust, CurrencyInfo, Currency, ARInvoice, ARPayment> adjres in adjustments)
			{
				ARAdjust adj = (ARAdjust)adjres;
				CurrencyInfo vouch_info = (CurrencyInfo)adjres;
				prev_info = (CurrencyInfo)adjres;
				Currency cury = (Currency)adjres;
				ARInvoice adjddoc = (ARInvoice)adjres; 
				ARPayment adjgdoc = (ARPayment)adjres;

				if (adj.CuryAdjgAmt == 0m && adj.CuryAdjgDiscAmt == 0m)
				{
					ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.Delete(adj);
					continue;
				}

				if (adj.Hold == true)
				{
					throw new PXException(Messages.Document_OnHold_CannotRelease);
				}

				List<ARSalesPerTran> spTrans = new List<ARSalesPerTran>();
				if (ardoc.DocType != ARDocType.CreditMemo) //Credit memos are treates as negative invoice
				{
					foreach (ARSalesPerTran iSPT in this.ARDoc_SalesPerTrans.Select(adjddoc.DocType, adjddoc.RefNbr))
					{
						ARSalesPerTran paySPT = new ARSalesPerTran();
						Copy(paySPT, ardoc);
						Copy(paySPT, adj);
						decimal applRatio = ((adj.CuryAdjdAmt + adj.CuryAdjdDiscAmt) / adjddoc.CuryOrigDocAmt).Value;
						if (ardoc.DocType == ARDocType.CashSale || ardoc.DocType == ARDocType.CashReturn)
						{
							applRatio = 1m;
						}
						else if (ardoc.DocType == ARDocType.CreditMemo)
						{
							applRatio = -applRatio;
						}
						CopyShare(paySPT, iSPT, applRatio, (cury.DecimalPlaces ?? 2));
						paySPT = this.ARDoc_SalesPerTrans.Insert(paySPT);
					}
				}

				if (adjddoc.RefNbr != null)
				{
					//Void Payment is processed after SC, avoid balance update since SC does not hold any balance even after application reversal and has WO Account in AR Account
					if (adjddoc.DocType != ARDocType.SmallCreditWO)
					{
						UpdateBalances(adj, adjddoc);
					}
					UpdateARBalances(adj, doc);
					UpdateARBalances(adj, adjddoc);
					UpdateARBalancesDates(adjddoc, adjustments.Count);
				} 
				else
				{
					UpdateBalances(adj, adjgdoc);
					UpdateARBalances(adj, doc);
					UpdateARBalances(adj, adjgdoc);
				}


				//Debit Payment AR Account
				GLTran tran = new GLTran();
				tran.SummPost = true;
				tran.ZeroPost = false;
				tran.BranchID = adj.AdjgBranchID;
				tran.AccountID = ardoc.ARAccountID;
				tran.SubID = ardoc.ARSubID;
				tran.DebitAmt = (adj.AdjgGLSign == 1m) ? adj.AdjAmt : 0m;
                tran.CuryDebitAmt = (adj.AdjgGLSign == 1m) ? adj.CuryAdjgAmt : 0m;
                tran.CreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.AdjAmt;
                tran.CuryCreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.CuryAdjgAmt;
				tran.TranType = adj.AdjgDocType;
				tran.TranClass = "P";
				tran.RefNbr = adj.AdjgRefNbr;
				tran.TranDesc = ardoc.DocDesc;
				tran.TranDate = adj.AdjgDocDate;
				tran.TranPeriodID = adj.AdjgTranPeriodID;
				tran.FinPeriodID = adj.AdjgFinPeriodID;
				tran.CuryInfoID = new_info.CuryInfoID;
				tran.Released = true;
				tran.ReferenceID = ardoc.CustomerID;
				tran.OrigAccountID = adj.AdjdARAcct;
				tran.OrigSubID = adj.AdjdARSub;
				tran.ProjectID = ardoc.ProjectID;
				tran.TaskID = ardoc.TaskID;


				UpdateHistory(tran, vend);
				UpdateHistory(tran, vend, new_info);

				je.GLTranModuleBatNbr.Insert(tran);

				//Credit Voucher AR Account/minus RGOL for refund
				tran = new GLTran();
				tran.SummPost = true;
				tran.ZeroPost = false;
				tran.BranchID = adj.AdjdBranchID;
				//Small-Credit has Payment AR Account in AdjdARAcct  and WO Account in ARAccountID
				tran.AccountID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjddoc.ARAccountID : adj.AdjdARAcct;
				tran.SubID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjddoc.ARSubID : adj.AdjdARSub;
				//Small-Credit reversal should update history for payment AR Account(AdjdARAcct)
				tran.OrigAccountID = adj.AdjdARAcct;
				tran.OrigSubID = adj.AdjdARSub;
                tran.CreditAmt = (adj.AdjgGLSign == 1m) ? adj.AdjAmt + adj.AdjDiscAmt + adj.AdjWOAmt + adj.RGOLAmt : 0m;
                tran.CuryCreditAmt = (adj.AdjgGLSign == 1m) ? (object.Equals(new_info.CuryID, new_info.BaseCuryID) ? tran.CreditAmt : adj.CuryAdjgAmt + adj.CuryAdjgDiscAmt + adj.CuryAdjgWOAmt) : 0m;
                tran.DebitAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.AdjAmt + adj.AdjDiscAmt + adj.AdjWOAmt - adj.RGOLAmt;
                tran.CuryDebitAmt = (adj.AdjgGLSign == 1m) ? 0m : (object.Equals(new_info.CuryID, new_info.BaseCuryID) ? tran.DebitAmt : adj.CuryAdjgAmt + adj.CuryAdjgDiscAmt + adj.CuryAdjgWOAmt);
				tran.TranType = adj.AdjgDocType;
				//always N for AdjdDocs except Prepayment
				tran.TranClass = ARDocType.DocClass(adj.AdjdDocType);
				tran.RefNbr = adj.AdjgRefNbr;
				tran.TranDesc = ardoc.DocDesc;
				tran.TranDate = adj.AdjgDocDate;
				tran.TranPeriodID = adj.AdjgTranPeriodID;
				tran.FinPeriodID = adj.AdjgFinPeriodID;
				tran.CuryInfoID = new_info.CuryInfoID;
				tran.Released = true;
				tran.ReferenceID = ardoc.CustomerID;
				tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(je);
				
				UpdateHistory(tran, vend);

				je.GLTranModuleBatNbr.Insert(tran);

				//Update CuryHistory in Voucher currency
                tran.CuryCreditAmt = (adj.AdjgGLSign == 1m) ? (object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? tran.CreditAmt : adj.CuryAdjdAmt + adj.CuryAdjdDiscAmt) : 0m;
                tran.CuryDebitAmt = (adj.AdjgGLSign == 1m) ? 0m : (object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? tran.DebitAmt : adj.CuryAdjdAmt + adj.CuryAdjdDiscAmt);
				UpdateHistory(tran, vend, vouch_info);

				//Credit Discount Taken/does not apply to refund, since no disc in AD
				tran = new GLTran();
				tran.SummPost = this.SummPost;
				tran.BranchID = adj.AdjdBranchID;
				tran.AccountID = vend.DiscTakenAcctID;
				tran.SubID = vend.DiscTakenSubID;
				tran.OrigAccountID = adj.AdjdARAcct;
				tran.OrigSubID = adj.AdjdARSub;
                tran.DebitAmt = (adj.AdjgGLSign == 1m) ? adj.AdjDiscAmt : 0m;
                tran.CuryDebitAmt = (adj.AdjgGLSign == 1m) ? adj.CuryAdjgDiscAmt : 0m;
                tran.CreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.AdjDiscAmt;
                tran.CuryCreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.CuryAdjgDiscAmt;
				tran.TranType = adj.AdjgDocType;
				tran.TranClass = "D";
				tran.RefNbr = adj.AdjgRefNbr;
				tran.TranDesc = ardoc.DocDesc;
				tran.TranDate = adj.AdjgDocDate;
				tran.TranPeriodID = adj.AdjgTranPeriodID;
				tran.FinPeriodID = adj.AdjgFinPeriodID;
				tran.CuryInfoID = new_info.CuryInfoID;
				tran.Released = true;
				tran.ReferenceID = ardoc.CustomerID;
				tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(je);

				UpdateHistory(tran, vend);

				je.GLTranModuleBatNbr.Insert(tran);

				//Update CuryHistory in Voucher currency
                tran.CuryDebitAmt = (adj.AdjgGLSign == 1m) ? adj.CuryAdjdDiscAmt : 0m;
                tran.CuryCreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.CuryAdjdDiscAmt;
				UpdateHistory(tran, vend, vouch_info);

				//Credit WO Account
				if (adj.AdjWOAmt != 0)
				{
					ARInvoice adjusted = PXSelect<ARInvoice, Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>, And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>.Select(pe, adj.AdjdDocType, adj.AdjdRefNbr);

					ReasonCode reasonCode = PXSelect<ReasonCode, Where<ReasonCode.reasonCodeID, Equal<Required<ReasonCode.reasonCodeID>>>>.Select(this, adj.WriteOffReasonCode);

					if (reasonCode == null)
						throw new PXException("Reason Code with the given id was not found in the system. Code: " + adj.WriteOffReasonCode);

					Location customerLocation = PXSelect<Location, Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,
						And<Location.locationID, Equal<Required<Location.locationID>>>>>.Select((PXGraph)pe, adjusted.CustomerID, adjusted.CustomerLocationID);

					Location companyLocation = (Location)PXSelectJoin<Location,
						InnerJoin<BAccountR, On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>,
						InnerJoin<GL.Branch, On<BAccountR.bAccountID, Equal<GL.Branch.bAccountID>>>>, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.Select((PXGraph)pe, adjusted.BranchID);

					object value = null;
					if (reasonCode.Usage == ReasonCodeUsages.BalanceWriteOff || reasonCode.Usage == ReasonCodeUsages.CreditWriteOff)
					{
						value = ReasonCodeSubAccountMaskAttribute.MakeSub<ReasonCode.subMask>((PXGraph)pe, reasonCode.SubMask,
							new object[] { reasonCode.SubID, customerLocation.CSalesSubID, companyLocation.CMPSalesSubID },
							new Type[] { typeof(ReasonCode.subID), typeof(Location.cSalesSubID), typeof(Location.cMPSalesSubID) });
					}
					else
					{
						throw new PXException("Invalid Reason Code Usage. Only Balance Write-Off or Credit Write-Off codes are expected.");
					}

				tran = new GLTran();
				tran.SummPost = this.SummPost;
				tran.BranchID = adj.AdjdBranchID;
					tran.AccountID = reasonCode.AccountID;
					tran.SubID = reasonCode.SubID;
				tran.OrigAccountID = adj.AdjdARAcct;
				tran.OrigSubID = adj.AdjdARSub;
                tran.DebitAmt = (adj.AdjgGLSign == 1m) ? adj.AdjWOAmt : 0m;
                tran.CuryDebitAmt = (adj.AdjgGLSign == 1m) ? adj.CuryAdjgWOAmt : 0m;
                tran.CreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.AdjWOAmt;
                tran.CuryCreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.CuryAdjgWOAmt;
				tran.TranType = adj.AdjgDocType;
				tran.TranClass = "B";
				tran.RefNbr = adj.AdjgRefNbr;
				tran.TranDesc = ardoc.DocDesc;
				tran.TranDate = adj.AdjgDocDate;
				tran.TranPeriodID = adj.AdjgTranPeriodID;
				tran.FinPeriodID = adj.AdjgFinPeriodID;
				tran.CuryInfoID = new_info.CuryInfoID;
				tran.Released = true;
				tran.ReferenceID = ardoc.CustomerID;
				tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(je);

				UpdateHistory(tran, vend);

					tran = je.GLTranModuleBatNbr.Insert(tran);
					je.GLTranModuleBatNbr.SetValueExt<GLTran.subID>(tran, value);

				//Update CuryHistory in Voucher currency
                tran.CuryDebitAmt = (adj.AdjgGLSign == 1m) ? adj.CuryAdjdWOAmt : 0m;
                tran.CuryCreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.CuryAdjdWOAmt;
				UpdateHistory(tran, vend, vouch_info);
				}

				//Debit/Credit RGOL Account
				if (cury.RealGainAcctID != null && cury.RealLossAcctID != null) 
				{
					tran = new GLTran();
					tran.SummPost = this.SummPost;
					tran.BranchID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjddoc.BranchID : adj.AdjdBranchID;
					tran.AccountID = (adj.RGOLAmt > 0m && !(bool)adj.VoidAppl || adj.RGOLAmt < 0m && (bool)adj.VoidAppl) 
                        ? cury.RealLossAcctID 
                        : cury.RealGainAcctID;
					tran.SubID = (adj.RGOLAmt > 0m && !(bool)adj.VoidAppl || adj.RGOLAmt < 0m && (bool)adj.VoidAppl) 
                        ? GainLossSubAccountMaskAttribute.GetSubID<Currency.realLossSubID>(je, tran.BranchID, cury)
                        : GainLossSubAccountMaskAttribute.GetSubID<Currency.realGainSubID>(je, tran.BranchID, cury);
					//SC has Payment AR Account in AdjdARAcct  and WO Account in ARAccountID
					tran.OrigAccountID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjddoc.ARAccountID : adj.AdjdARAcct;
					tran.OrigSubID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjddoc.ARSubID : adj.AdjdARSub;
					tran.CreditAmt = (adj.RGOLAmt < 0m) ? -1m * adj.RGOLAmt : 0m;
					//!object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) === precision alteration before payment application
					tran.CuryCreditAmt = object.Equals(new_info.CuryID, new_info.BaseCuryID) && !object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? tran.CreditAmt : 0m;
					tran.DebitAmt = (adj.RGOLAmt > 0m) ? adj.RGOLAmt : 0m;
					tran.CuryDebitAmt = object.Equals(new_info.CuryID, new_info.BaseCuryID) && !object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? tran.DebitAmt : 0m;
					tran.TranType = adj.AdjgDocType;
					tran.TranClass = "R";
					tran.RefNbr = adj.AdjgRefNbr;
					tran.TranDesc = ardoc.DocDesc;
					tran.TranDate = adj.AdjgDocDate;
					tran.TranPeriodID = adj.AdjgTranPeriodID;
					tran.FinPeriodID = adj.AdjgFinPeriodID;
					tran.CuryInfoID = new_info.CuryInfoID;
					tran.Released = true;
					tran.ReferenceID = ardoc.CustomerID;
					tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(je);

					UpdateHistory(tran, vend);

					je.GLTranModuleBatNbr.Insert(tran);

					//Update CuryHistory in Voucher currency
					tran.CuryDebitAmt = 0m;
					tran.CuryCreditAmt = 0m;
					UpdateHistory(tran, vend, vouch_info);

				}
				//Debit/Credit Rounding Gain-Loss Account
				else if (paycury.RoundingGainAcctID != null && paycury.RoundingLossAcctID != null)
				{
					tran = new GLTran();
					tran.SummPost = this.SummPost;
					tran.BranchID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjddoc.BranchID : adj.AdjdBranchID;
                    tran.AccountID = (adj.RGOLAmt > 0m && !(bool)adj.VoidAppl || adj.RGOLAmt < 0m && (bool)adj.VoidAppl) 
                        ? paycury.RoundingLossSubID 
                        : paycury.RoundingGainSubID;
					tran.SubID = (adj.RGOLAmt > 0m && !(bool)adj.VoidAppl || adj.RGOLAmt < 0m && (bool)adj.VoidAppl) 
                        ? GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingLossSubID>(je, tran.BranchID, paycury)
                        : GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingGainSubID>(je, tran.BranchID, paycury);                        

					//SC has Payment AR Account in AdjdARAcct  and WO Account in ARAccountID
					tran.OrigAccountID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjddoc.ARAccountID : adj.AdjdARAcct;
					tran.OrigSubID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjddoc.ARSubID : adj.AdjdARSub;
					tran.CreditAmt = (adj.RGOLAmt < 0m) ? -1m * adj.RGOLAmt : 0m;
					//!object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) === precision alteration before payment application
					tran.CuryCreditAmt = object.Equals(new_info.CuryID, new_info.BaseCuryID) && !object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? tran.CreditAmt : 0m;
					tran.DebitAmt = (adj.RGOLAmt > 0m) ? adj.RGOLAmt : 0m;
					tran.CuryDebitAmt = object.Equals(new_info.CuryID, new_info.BaseCuryID) && !object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? tran.DebitAmt : 0m;
					tran.TranType = adj.AdjgDocType;
					tran.TranClass = "R";
					tran.RefNbr = adj.AdjgRefNbr;
					tran.TranDesc = ardoc.DocDesc;
					tran.TranDate = adj.AdjgDocDate;
					tran.TranPeriodID = adj.AdjgTranPeriodID;
					tran.FinPeriodID = adj.AdjgFinPeriodID;
					tran.CuryInfoID = new_info.CuryInfoID;
					tran.Released = true;
					tran.ReferenceID = ardoc.CustomerID;
					tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(je);

					UpdateHistory(tran, vend);

					je.GLTranModuleBatNbr.Insert(tran);

					//Update CuryHistory in Voucher currency
					tran.CuryDebitAmt = 0m;
					tran.CuryCreditAmt = 0m;
					UpdateHistory(tran, vend, vouch_info);
				}

                

				//true for Quick Check and Void Quick Check
				if (adj.AdjgDocType != adj.AdjdDocType || adj.AdjgRefNbr != adj.AdjdRefNbr)
				{
					doc.CuryDocBal -= adj.AdjgBalSign * adj.CuryAdjgAmt;
                    doc.DocBal -= adj.AdjgBalSign * adj.AdjAmt;
				}

				if (_IsIntegrityCheck == false)
				{
                    if (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted)
					{
						je.Save.Press();
					}

					if (!je.BatchModule.Cache.IsDirty)
					{
						adj.AdjBatchNbr = ((Batch)je.BatchModule.Current).BatchNbr;
					}
					adj.Released = true;
					prev_adj = (ARAdjust)Caches[typeof(ARAdjust)].Update(adj);
				}
			}

            if (_IsIntegrityCheck == false && (bool)ardoc.VoidAppl == false && doc.CuryDocBal < 0m)
            {
                throw new PXException(Messages.DocumentBalanceNegative);
            }

			if (doc.CuryDocBal == 0m && doc.DocBal != 0m && prev_adj.AdjdRefNbr != null)
			{
				if ((bool)prev_adj.VoidAppl || object.Equals(new_info.CuryID, new_info.BaseCuryID))
				{
					throw new PXException();
				}

				//BaseCalc should be false
				prev_adj.AdjAmt += doc.DocBal;
				prev_adj.RGOLAmt -= (prev_adj.ReverseGainLoss == false ? doc.DocBal : -doc.DocBal);

				prev_adj = (ARAdjust)Caches[typeof(ARAdjust)].Update(prev_adj);

				//signs are reversed to RGOL
				GLTran tran = new GLTran();
				tran.SummPost = this.SummPost;
				tran.BranchID = ardoc.BranchID; ;
				tran.AccountID = (doc.DocBal < 0m) 
                    ? paycury.RoundingLossAcctID 
                    : paycury.RoundingGainAcctID;
                tran.SubID = (doc.DocBal < 0m)
                    ? GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingLossSubID>(je, tran.BranchID, paycury)
                    : GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingGainSubID>(je, tran.BranchID, paycury);
				tran.OrigAccountID = prev_adj.AdjdARAcct;
				tran.OrigSubID = prev_adj.AdjdARSub;
				tran.CreditAmt = (doc.DocBal > 0m) ? doc.DocBal : 0m;
				tran.CuryCreditAmt = 0m;
				tran.DebitAmt = (doc.DocBal < 0m) ? -1 * doc.DocBal : 0m;
				tran.CuryDebitAmt = 0m;
				tran.TranType = prev_adj.AdjgDocType;
				tran.TranClass = "R";
				tran.RefNbr = prev_adj.AdjgRefNbr;
				tran.TranDesc = ardoc.DocDesc;
				tran.TranDate = prev_adj.AdjgDocDate;
				tran.TranPeriodID = prev_adj.AdjgTranPeriodID;
				tran.FinPeriodID = prev_adj.AdjgFinPeriodID;
				tran.CuryInfoID = new_info.CuryInfoID;
				tran.Released = true;
				tran.ReferenceID = ardoc.CustomerID;
				tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(je);

				UpdateHistory(tran, vend);
				//Update CuryHistory in Voucher currency
				UpdateHistory(tran, vend, prev_info);

				je.GLTranModuleBatNbr.Insert(tran);

				//Credit Payment AR Account
				tran = new GLTran();
				tran.SummPost = true;
				tran.ZeroPost = false;
				tran.BranchID = ardoc.BranchID;
				tran.AccountID = ardoc.ARAccountID;
				tran.SubID = ardoc.ARSubID;
				tran.DebitAmt = (doc.DocBal > 0m) ? doc.DocBal : 0m;
				tran.CuryDebitAmt = 0m;
				tran.CreditAmt = (doc.DocBal < 0m) ? -1 * doc.DocBal : 0m;
				tran.CuryCreditAmt = 0m;
				tran.TranType = prev_adj.AdjgDocType;
				tran.TranClass = "P";
				tran.RefNbr = prev_adj.AdjgRefNbr;
				tran.TranDesc = ardoc.DocDesc;
				tran.TranDate = prev_adj.AdjgDocDate;
				tran.TranPeriodID = prev_adj.AdjgTranPeriodID;
				tran.FinPeriodID = prev_adj.AdjgFinPeriodID;
				tran.CuryInfoID = new_info.CuryInfoID;
				tran.Released = true;
				tran.ReferenceID = ardoc.CustomerID;
				tran.OrigAccountID = prev_adj.AdjdARAcct;
				tran.OrigSubID = prev_adj.AdjdARSub;
				tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(je);

				UpdateHistory(tran, vend);
				//Update CuryHistory in Payment currency
				UpdateHistory(tran, vend, new_info);

				je.GLTranModuleBatNbr.Insert(tran);

			}

			if ((bool)ardoc.VoidAppl || doc.CuryDocBal == 0m)
			{
				doc.CuryDocBal = 0m;
				doc.DocBal = 0m;
				doc.OpenDoc = false;
				foreach (ARAdjust adjmax in PXSelectGroupBy<ARAdjust, Where<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>, And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>, And<Where<ARAdjust.released, Equal<boolTrue>, Or<ARAdjust.adjNbr, Equal<Required<ARAdjust.adjNbr>>>>>>>, Aggregate<GroupBy<ARAdjust.adjgDocType, GroupBy<ARAdjust.adjgRefNbr, Max<ARAdjust.adjgFinPeriodID, Max<ARAdjust.adjgTranPeriodID>>>>>>.Select(this, doc.DocType, doc.RefNbr, doc.LineCntr))
				{
					doc.ClosedFinPeriodID = adjmax.AdjgFinPeriodID;
					doc.ClosedTranPeriodID = adjmax.AdjgTranPeriodID;
				}
			}
			else
			{
				doc.OpenDoc = true;
				doc.ClosedTranPeriodID = null;
				doc.ClosedFinPeriodID = null;
			}

			//increment default for AdjNbr
			doc.LineCntr++;
		}

		public virtual void ReleaseSmallCreditProc(JournalEntry je, ref ARRegister doc, PXResult<ARInvoice, CurrencyInfo, Terms, Customer> res)
		{
			ARInvoice ardoc = (ARInvoice)res;
			CurrencyInfo info = (CurrencyInfo)res;
			Customer vend = (Customer)res;

			CustomerClass custclass = (CustomerClass)PXSelectJoin<CustomerClass, InnerJoin<ARSetup, On<ARSetup.dfltCustomerClassID, Equal<CustomerClass.customerClassID>>>>.Select(this, null);

			CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(info);
			new_info.CuryInfoID = null;
			new_info.ModuleCode = "GL";
			new_info = je.currencyinfo.Insert(new_info) ?? new_info;

			if (doc.Released == false)
			{
				doc.CuryDocBal = doc.CuryOrigDocAmt;
				doc.DocBal = doc.OrigDocAmt;

				doc.OpenDoc = true;
				doc.ClosedFinPeriodID = null;
				doc.ClosedTranPeriodID = null;
			}

			doc.Released = true;
			ARAdjust prev_adj = new ARAdjust();

			foreach (PXResult<ARAdjust, CurrencyInfo, Currency, ARPayment> adjres in ARAdjust_AdjdDocType_RefNbr_CustomerID.Select(doc.DocType, doc.RefNbr, doc.LineCntr))
			{
				ARAdjust adj = (ARAdjust)adjres;
				CurrencyInfo vouch_info = (CurrencyInfo)adjres;
				Currency cury = (Currency)adjres;
				ARPayment adjddoc = (ARPayment)adjres;

				UpdateBalances(adj, adjddoc);

				UpdateARBalances(adj, doc);
				UpdateARBalances(adj, adjddoc);

				//Credit WO Account
				GLTran tran = new GLTran();
				tran.SummPost = true;
				tran.BranchID = ardoc.BranchID;
				tran.AccountID = ardoc.ARAccountID;
				tran.SubID = ardoc.ARSubID;
				tran.DebitAmt = (ardoc.DrCr == "D") ? adj.AdjAmt : 0m;
				tran.CuryDebitAmt = (ardoc.DrCr == "D") ? adj.CuryAdjgAmt : 0m;
				tran.CreditAmt = (ardoc.DrCr == "D") ? 0m : adj.AdjAmt;
				tran.CuryCreditAmt = (ardoc.DrCr == "D") ? 0m : adj.CuryAdjgAmt;
				tran.TranType = adj.AdjdDocType;
				tran.TranClass = "P";
				tran.RefNbr = adj.AdjdRefNbr;
				tran.TranDesc = ardoc.DocDesc;
				tran.TranDate = adj.AdjdDocDate;
				tran.TranPeriodID = ardoc.TranPeriodID;
				tran.FinPeriodID = ardoc.FinPeriodID;
				tran.CuryInfoID = new_info.CuryInfoID;
				tran.Released = true;
				tran.ReferenceID = ardoc.CustomerID;
				tran.OrigAccountID = adjddoc.ARAccountID;
				tran.OrigSubID = adjddoc.ARSubID;
				tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(je);

				//Create history for SCWO Account
				UpdateHistory(tran, vend);

				je.GLTranModuleBatNbr.Insert(tran);

				tran.CuryDebitAmt = (ardoc.DrCr == "D") ? (object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? adj.AdjAmt : adj.CuryAdjdAmt) : 0m;
				tran.CuryCreditAmt = (ardoc.DrCr == "D") ? 0m : (object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? adj.AdjAmt : adj.CuryAdjdAmt);

				UpdateHistory(tran, vend, vouch_info);

				//Debit Payment AR Account
				tran = new GLTran();
				tran.SummPost = true;
				tran.BranchID = adjddoc.BranchID;
				tran.AccountID = adjddoc.ARAccountID;
				tran.SubID = adjddoc.ARSubID;
				tran.CreditAmt = (ardoc.DrCr == "D") ? adj.AdjAmt : 0m;
				tran.CuryCreditAmt = (ardoc.DrCr == "D") ? (object.Equals(new_info.CuryID, new_info.BaseCuryID) ? adj.AdjAmt : adj.CuryAdjgAmt) : 0m;
				tran.DebitAmt = (ardoc.DrCr == "D") ? 0m : adj.AdjAmt;
				tran.CuryDebitAmt = (ardoc.DrCr == "D") ? 0m : (object.Equals(new_info.CuryID, new_info.BaseCuryID) ? adj.AdjAmt : adj.CuryAdjgAmt);
				tran.TranType = adj.AdjdDocType;
				tran.TranClass = "N";
				tran.RefNbr = adj.AdjdRefNbr;
				tran.TranDesc = ardoc.DocDesc;
				tran.TranDate = adj.AdjdDocDate;
				tran.TranPeriodID = ardoc.TranPeriodID;
				tran.FinPeriodID = ardoc.FinPeriodID;
				tran.CuryInfoID = new_info.CuryInfoID;
				tran.Released = true;
				tran.ReferenceID = ardoc.CustomerID;
				tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(je);

				UpdateHistory(tran, vend);

				je.GLTranModuleBatNbr.Insert(tran);

				//Update CuryHistory in Voucher currency
				tran.CuryCreditAmt = (ardoc.DrCr == "D") ? (object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? adj.AdjAmt : adj.CuryAdjdAmt) : 0m;
				tran.CuryDebitAmt = (ardoc.DrCr == "D") ? 0m : (object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? adj.AdjAmt : adj.CuryAdjdAmt);
				UpdateHistory(tran, vend, vouch_info);

				//No Discount should take place
				//No RGOL should take place

				doc.CuryDocBal -= adj.CuryAdjgAmt;
				doc.DocBal -= adj.AdjAmt;

                if (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted)
				{
					je.Save.Press();
				}

				if (!je.BatchModule.Cache.IsDirty)
				{
					adj.AdjBatchNbr = ((Batch)je.BatchModule.Current).BatchNbr;
				}
				adj.Released = true;
				prev_adj = (ARAdjust)Caches[typeof(ARAdjust)].Update(adj);
			}

			if (doc.CuryDocBal == 0m && doc.DocBal != 0m)
			{
				throw new PXException(Messages.DocumentBalanceNegative);
			}

			if ((bool)doc.OpenDoc == false || doc.CuryDocBal == 0m)
			{
				doc.CuryDocBal = 0m;
				doc.DocBal = 0m;
				doc.OpenDoc = false;

				doc.ClosedFinPeriodID = doc.FinPeriodID;
				doc.ClosedTranPeriodID = doc.TranPeriodID;
			}

			//increment default for AdjNbr
			doc.LineCntr++;
		}


        private void SegregateBatch(JournalEntry je, int? branchID, string curyID, DateTime? docDate, string finPeriodID, string description, CurrencyInfo curyInfo)
		{
            JournalEntry.SegregateBatch(je, BatchModule.AR, branchID, curyID, docDate, finPeriodID, description, curyInfo, null);
		}

		public virtual List<ARRegister> ReleaseDocProc(JournalEntry je, ARRegister ardoc, List<Batch> pmBatchList)
		{
			List<ARRegister> ret = null;

			if ((bool)ardoc.Hold)
			{
				throw new PXException(Messages.Document_OnHold_CannotRelease);
			}

			ARRegister doc = PXCache<ARRegister>.CreateCopy(ardoc);

			//using (new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					//mark as updated so that doc will not expire from cache and update with Released = 1 will not override balances/amount in document
					ARDocument.Cache.SetStatus(doc, PXEntryStatus.Updated);

					UpdateARBalances(doc, -doc.OrigDocAmt);

					bool Released = (bool)doc.Released;
					List<PM.PMRegister> pmDocList = new List<PM.PMRegister>();
					
					foreach (PXResult<ARInvoice, CurrencyInfo, Terms, Customer, SOInvoice> res in ARInvoice_DocType_RefNbr.Select((object)doc.DocType, doc.RefNbr))
					{
                        Customer c = res;
                        switch (c.Status)
                        {
                            case Customer.status.Inactive:
                            case Customer.status.Hold:
                            case Customer.status.CreditHold:
                                throw new PXSetPropertyException(Messages.CustomerIsInStatus, new Customer.status.ListAttribute().ValueLabelDic[c.Status]);
                        }

                        PM.PMRegister pmDoc = null;
						ARInvoice invoice = res;

						//must check for CM application in different period
						if (doc.Released == false)
                        {
                            SegregateBatch(je, doc.BranchID, doc.CuryID, doc.DocDate, doc.FinPeriodID, doc.DocDesc, (CurrencyInfo)res);
						}
						if (doc.DocType == ARDocType.SmallCreditWO)
						{
							ReleaseSmallCreditProc(je, ref doc, res);
						}
						else if (_IsIntegrityCheck == false && ((Customer)res).AutoApplyPayments == true && invoice.Released == false && ((SOInvoice)res).IsCCCaptured == false)
						{
							ie.Clear();
							ie.Document.Current = invoice;

							if (ie.Adjustments_Raw.View.SelectSingle() == null)
							{
								ie.LoadInvoicesProc();
							}
							ie.Save.Press();
							doc = (ARRegister)ie.Document.Current;
							ret = ReleaseDocProc(je, ref doc, new PXResult<ARInvoice, CurrencyInfo, Terms, Customer>(ie.Document.Current, (CurrencyInfo)res, (Terms)res, (Customer)res), out pmDoc);
						}
						else
						{
							ret = ReleaseDocProc(je, ref doc, res, out pmDoc);
						}
						//ensure correct PXDBDefault behaviour on ARTran persisting
						ARInvoice_DocType_RefNbr.Current = (ARInvoice)res;
						if (pmDoc != null)
							pmDocList.Add(pmDoc);
					}

					foreach (PXResult<ARPayment, CurrencyInfo, Currency, Customer, CashAccount> res in ARPayment_DocType_RefNbr.Select((object)doc.DocType, doc.RefNbr, doc.CustomerID))
					{
                        Customer c = res;
                        switch (c.Status)
                        {
                            case Customer.status.Inactive:
                            case Customer.status.Hold:
                                throw new PXSetPropertyException(Messages.CustomerIsInStatus, new Customer.status.ListAttribute().ValueLabelDic[c.Status]);
                        }

                        ARPayment payment = res;
						if (doc.DocType != ARDocType.CashSale && doc.DocType != ARDocType.CashReturn)
                        {
                            SegregateBatch(je, doc.BranchID, doc.CuryID, payment.AdjDate, payment.AdjFinPeriodID, payment.DocDesc, (CurrencyInfo)res);
						}
						if (_IsIntegrityCheck == false && ((Customer)res).AutoApplyPayments == true && payment.Released == false)
						{
							pe.Clear();
							pe.Document.Current = payment;
							if (pe.Adjustments_Raw.View.SelectSingle() == null)
							{
								pe.LoadInvoicesProc(false);
							}
							pe.Save.Press();
							doc = (ARRegister)pe.Document.Current;
							ReleaseDocProc(je, ref doc, new PXResult<ARPayment, CurrencyInfo, Currency, Customer, CashAccount>(pe.Document.Current, (CurrencyInfo)res, (Currency)res, (Customer)res, (CashAccount)res));
						}
						else
						{
							ReleaseDocProc(je, ref doc, res);
						}
						//ensure correct PXDBDefault behaviour on ARAdjust persisting
						ARPayment_DocType_RefNbr.Current = (ARPayment)res;
					}

					doc.Released = true;
                    //when doc is loaded in ARInvoiceEntry, ARPaymentEntry it will set Selected = 0 and document will dissappear from 
                    //list of processing items.
                    doc.Selected = true;

					UpdateARBalances(doc);

                    if (_IsIntegrityCheck == false)
					{
                        if (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted)
                        {
                            je.Save.Press();
                        }

                        if (!je.BatchModule.Cache.IsDirty && string.IsNullOrEmpty(doc.BatchNbr))
                        {
							doc.BatchNbr = ((Batch)je.BatchModule.Current).BatchNbr;
                        }
					}

					#region Auto Commit/Post document to avalara.

					ARInvoice arInvoice = PXSelect<ARInvoice, Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>, And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr);
					if (arInvoice != null && AvalaraMaint.IsExternalTax(this, arInvoice.TaxZoneID) && doc.IsTaxValid == true)
					{
						TXAvalaraSetup avalaraSetup = PXSelect<TXAvalaraSetup>.Select(this);
						if (avalaraSetup != null)
						{
							TaxSvc service = new TaxSvc();
							AvalaraMaint.SetupService(this, service);

							CommitTaxRequest request = new CommitTaxRequest();
							request.CompanyCode = AvalaraMaint.CompanyCodeFromBranch(this, doc.BranchID);
							request.DocCode = string.Format("AR.{0}.{1}", doc.DocType, doc.RefNbr);

							if (doc.DocType == ARDocType.CreditMemo)
								request.DocType = DocumentType.ReturnInvoice;
							else
								request.DocType = DocumentType.SalesInvoice;


							CommitTaxResult result = service.CommitTax(request);
							if (result.ResultCode == SeverityLevel.Success)
							{
								doc.IsTaxPosted = true;
							}
							else
							{
								//Avalara retuned an error - The given document is already marked as posted on the avalara side.
								if (result.ResultCode == SeverityLevel.Error && result.Messages.Count == 1 &&
									result.Messages[0].Details == "Expected Posted")
								{
									//ignore this error - everything is cool
								}
								else
								{
									//show as warning.
									StringBuilder sb = new StringBuilder();
									foreach (Avalara.AvaTax.Adapter.Message msg in result.Messages)
									{
										sb.AppendLine(msg.Name + ": " + msg.Details);
									}

									if (sb.Length > 0)
									{
										ardoc.WarningMessage = string.Format(Messages.PostingToAvalaraFailed, sb.ToString());
									}
								}
							}

						}
					} 
					#endregion
					
					doc = (ARRegister)ARDocument.Cache.Update(doc);

					PXCache<ARRegister>.RestoreCopy(ardoc, doc);

					PXTimeStampScope.DuplicatePersisted(ARDocument.Cache, doc, typeof(ARInvoice));
					PXTimeStampScope.DuplicatePersisted(ARDocument.Cache, doc, typeof(ARPayment));

					if (doc.DocType == ARDocType.CreditMemo)
					{
						if (Released)
						{
							ARPayment_DocType_RefNbr.Cache.SetStatus(ARPayment_DocType_RefNbr.Current, PXEntryStatus.Notchanged);
						}
						else
						{
							ARPayment crmemo = (ARPayment)ARPayment_DocType_RefNbr.Cache.Extend<ARRegister>(doc);
							crmemo.CreatedByID = doc.CreatedByID;
							crmemo.CreatedByScreenID = doc.CreatedByScreenID;
							crmemo.CreatedDateTime = doc.CreatedDateTime;
							crmemo.CashAccountID = null;
							crmemo.AdjDate = crmemo.DocDate;
							crmemo.AdjTranPeriodID = crmemo.TranPeriodID;
							crmemo.AdjFinPeriodID = crmemo.FinPeriodID;
							ARPayment_DocType_RefNbr.Cache.Update(crmemo);
							ARDocument.Cache.SetStatus(doc, PXEntryStatus.Notchanged);
						}
					}
					else
					{
						if (ARDocument.Cache.ObjectsEqual(doc, ARPayment_DocType_RefNbr.Current))
						{
							ARPayment_DocType_RefNbr.Cache.SetStatus(ARPayment_DocType_RefNbr.Current, PXEntryStatus.Notchanged);
						}
					}

                    foreach (ARPayment item in ARPayment_DocType_RefNbr.Cache.Updated)
                    {
                        PXTimeStampScope.DuplicatePersisted(ARPayment_DocType_RefNbr.Cache, item, typeof(ARRegister));
                    }

					List<ProcessInfo<Batch>> batchList;
					PM.RegisterRelease.ReleaseWithoutPost(pmDocList, false, out batchList);
					foreach (ProcessInfo<Batch> processInfo in batchList)
					{
						pmBatchList.AddRange(processInfo.Batches);
					}
					
					this.Actions.PressSave();
					ts.Complete(this);
				}
			}
			return ret;
		}

        public override void Persist()
        {
            using (PXTransactionScope ts = new PXTransactionScope())
            {
                ARPayment_DocType_RefNbr.Cache.Persist(PXDBOperation.Insert);
                ARPayment_DocType_RefNbr.Cache.Persist(PXDBOperation.Update);

                ARDocument.Cache.Persist(PXDBOperation.Update);
                ARTran_TranType_RefNbr.Cache.Persist(PXDBOperation.Insert);
                ARTran_TranType_RefNbr.Cache.Persist(PXDBOperation.Update);
                ARTaxTran_TranType_RefNbr.Cache.Persist(PXDBOperation.Update);
                intranselect.Cache.Persist(PXDBOperation.Update);

                ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.Persist(PXDBOperation.Insert);
                ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.Persist(PXDBOperation.Update);
                ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.Persist(PXDBOperation.Delete);

                ARDoc_SalesPerTrans.Cache.Persist(PXDBOperation.Insert);
                ARDoc_SalesPerTrans.Cache.Persist(PXDBOperation.Update);

                Caches[typeof(ARHist)].Persist(PXDBOperation.Insert);

                Caches[typeof(CuryARHist)].Persist(PXDBOperation.Insert);

                Caches[typeof(ARBalances)].Persist(PXDBOperation.Insert);

				Caches[typeof(CADailySummary)].Persist(PXDBOperation.Insert);

                ts.Complete(this);
            }

            ARPayment_DocType_RefNbr.Cache.Persisted(false);
            ARDocument.Cache.Persisted(false);
            ARTran_TranType_RefNbr.Cache.Persisted(false);
            ARTaxTran_TranType_RefNbr.Cache.Persisted(false);
            intranselect.Cache.Persisted(false);
            ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.Persisted(false);

            Caches[typeof(ARHist)].Persisted(false);

            Caches[typeof(CuryARHist)].Persisted(false);

            Caches[typeof(ARBalances)].Persisted(false);

            ARDoc_SalesPerTrans.Cache.Persisted(false);

			Caches[typeof(CADailySummary)].Persisted(false);
        }

		protected bool _IsIntegrityCheck = false;

		protected virtual int SortCustDocs(PXResult<ARRegister> a, PXResult<ARRegister> b)
		{
			return ((IComparable)((ARRegister)a).SortOrder).CompareTo(((ARRegister)b).SortOrder);
		}

		public virtual void IntegrityCheckProc(Customer cust, string startPeriod)
		{
			_IsIntegrityCheck = true;
			JournalEntry je = PXGraph.CreateInstance<JournalEntry>();
			je.SetOffline();

			Caches[typeof(Customer)].Current = cust;

			using (new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					string minPeriod = "190001";

					ARHistory maxHist = (ARHistory)PXSelectGroupBy<ARHistory, Where<ARHistory.customerID, Equal<Current<Customer.bAccountID>>, And<ARHistory.detDeleted, Equal<boolTrue>>>, Aggregate<Max<ARHistory.finPeriodID>>>.Select(this);

					if (maxHist != null && maxHist.FinPeriodID != null)
					{
						minPeriod = FinPeriodIDAttribute.PeriodPlusPeriod(this, maxHist.FinPeriodID, 1);
					}

					if (string.IsNullOrEmpty(startPeriod) == false && string.Compare(startPeriod, minPeriod) > 0)
					{
						minPeriod = startPeriod;
					}

					foreach (CuryARHist old_hist in PXSelectReadonly<CuryARHist, Where<CuryARHist.customerID, Equal<Required<CuryARHist.customerID>>, And<CuryARHist.finPeriodID, GreaterEqual<Required<CuryARHist.finPeriodID>>>>>.Select(this, cust.BAccountID, minPeriod))
					{
						CuryARHist hist = new CuryARHist();
						hist.BranchID = old_hist.BranchID;
						hist.AccountID = old_hist.AccountID;
						hist.SubID = old_hist.SubID;
						hist.CustomerID = old_hist.CustomerID;
						hist.FinPeriodID = old_hist.FinPeriodID;
						hist.CuryID = old_hist.CuryID;

						hist = (CuryARHist)Caches[typeof(CuryARHist)].Insert(hist);

						hist.FinPtdRevalued += old_hist.FinPtdRevalued;

						ARHist basehist = new ARHist();
						basehist.BranchID = old_hist.BranchID;
						basehist.AccountID = old_hist.AccountID;
						basehist.SubID = old_hist.SubID;
						basehist.CustomerID = old_hist.CustomerID;
						basehist.FinPeriodID = old_hist.FinPeriodID;

						basehist = (ARHist)Caches[typeof(ARHist)].Insert(basehist);

						basehist.FinPtdRevalued += old_hist.FinPtdRevalued;
					}

					PXDatabase.Delete<ARHistory>(
						new PXDataFieldRestrict("CustomerID", PXDbType.Int, 4, cust.BAccountID, PXComp.EQ),
						new PXDataFieldRestrict("FinPeriodID", PXDbType.Char, 6, minPeriod, PXComp.GE)
						);

					PXDatabase.Delete<CuryARHistory>(
						new PXDataFieldRestrict("CustomerID", PXDbType.Int, 4, cust.BAccountID, PXComp.EQ),
						new PXDataFieldRestrict("FinPeriodID", PXDbType.Char, 6, minPeriod, PXComp.GE)
						);

					PXDatabase.Update<ARBalances>(
						new PXDataFieldAssign<ARBalances.totalOpenOrders>(0m),
						new PXDataFieldAssign<ARBalances.unreleasedBal>(0m),
						new PXDataFieldAssign("CurrentBal", 0m),
						new PXDataFieldAssign("OldInvoiceDate", null),
						new PXDataFieldRestrict("CustomerID", PXDbType.Int, 4, cust.BAccountID, PXComp.EQ)
					);

					PXRowInserting ARHist_RowInserting = delegate(PXCache sender, PXRowInsertingEventArgs e)
					{
						if (string.Compare(((ARHist)e.Row).FinPeriodID, minPeriod) < 0)
						{
							e.Cancel = true;
						}
					};

					PXRowInserting CuryARHist_RowInserting = delegate(PXCache sender, PXRowInsertingEventArgs e)
					{
						if (string.Compare(((CuryARHist)e.Row).FinPeriodID, minPeriod) < 0)
						{
							e.Cancel = true;
						}
					};

					this.RowInserting.AddHandler<ARHist>(ARHist_RowInserting);
					this.RowInserting.AddHandler<CuryARHist>(CuryARHist_RowInserting);

					PXResultset<ARRegister> custdocs = PXSelect<ARRegister, Where<ARRegister.customerID, Equal<Current<Customer.bAccountID>>, And<ARRegister.released, Equal<boolTrue>, And<Where<ARRegister.finPeriodID, GreaterEqual<Required<ARRegister.finPeriodID>>, Or<ARRegister.closedFinPeriodID, GreaterEqual<Required<ARRegister.finPeriodID>>>>>>>>.Select(this, minPeriod, minPeriod);
					PXResultset<ARRegister> other = PXSelectJoinGroupBy<ARRegister,
						InnerJoin<ARAdjust, On2<
                            Where<ARAdjust.adjgDocType, Equal<ARRegister.docType>, 
                                Or<ARAdjust.adjgDocType, Equal<ARDocType.payment>, And<ARRegister.docType, Equal<ARDocType.voidPayment>, 
                                Or<ARAdjust.adjgDocType, Equal<ARDocType.prepayment>, And<ARRegister.docType, Equal<ARDocType.voidPayment>>>>>>, 
                            And<ARAdjust.adjgRefNbr, Equal<ARRegister.refNbr>>>>,
						Where<ARRegister.customerID, Equal<Current<Customer.bAccountID>>,
							And2<Where<ARAdjust.adjdClosedFinPeriodID, GreaterEqual<Required<ARAdjust.adjdClosedFinPeriodID>>,
								Or<ARAdjust.adjdFinPeriodID, GreaterEqual<Required<ARAdjust.adjdFinPeriodID>>>>,
							And<ARAdjust.released, Equal<boolTrue>,
							And<ARRegister.finPeriodID, Less<Required<ARRegister.finPeriodID>>,
							And<ARRegister.released, Equal<boolTrue>, 
							And<Where<ARRegister.closedFinPeriodID, Less<Required<ARRegister.closedFinPeriodID>>, Or<ARRegister.closedFinPeriodID, IsNull>>>>>>>>,
						Aggregate<GroupBy<ARRegister.docType,
							GroupBy<ARRegister.refNbr,
							GroupBy<ARRegister.createdByID,
							GroupBy<ARRegister.lastModifiedByID,
							GroupBy<ARRegister.released,
							GroupBy<ARRegister.openDoc,
                            GroupBy<ARRegister.hold,
                            GroupBy<ARRegister.scheduled,
                            GroupBy<ARRegister.isTaxValid,
                            GroupBy<ARRegister.isTaxSaved,
                            GroupBy<ARRegister.isTaxPosted,
                            GroupBy<ARRegister.voided>>>>>>>>>>>>>>.Select(this, minPeriod, minPeriod, minPeriod, minPeriod);

					custdocs.AddRange(other); 
					custdocs.Sort(SortCustDocs);

					foreach (ARRegister custdoc in custdocs)
					{
						je.Clear();

						ARRegister doc = custdoc;

						//mark as updated so that doc will not expire from cache and update with Released = 1 will not override balances/amount in document
						ARDocument.Cache.SetStatus(doc, PXEntryStatus.Updated);

						doc.Released = false;

						foreach (PXResult<ARInvoice, CurrencyInfo, Terms, Customer> res in ARInvoice_DocType_RefNbr.Select((object)doc.DocType, doc.RefNbr))
						{
							//must check for CM application in different period
							if ((bool)doc.Released == false)
                            {
								SegregateBatch(je, doc.BranchID, doc.CuryID, doc.DocDate, doc.FinPeriodID, doc.DocDesc, (CurrencyInfo)res);
							}
							if (doc.DocType == ARDocType.SmallCreditWO)
							{
								doc.LineCntr = -1;
								ReleaseSmallCreditProc(je, ref doc, res);
							}
							else
							{
								PM.PMRegister pmDoc;
								ReleaseDocProc(je, ref doc, res, out pmDoc);
							}
							doc.Released = true;
						}

						foreach (PXResult<ARPayment, CurrencyInfo, Currency, Customer, CashAccount> res in ARPayment_DocType_RefNbr.Select((object)doc.DocType, doc.RefNbr, doc.CustomerID))
						{
                            ARPayment payment = (ARPayment)res;
                            SegregateBatch(je, doc.BranchID, doc.CuryID, payment.AdjDate, payment.AdjFinPeriodID, payment.DocDesc, (CurrencyInfo)res);

							int OrigLineCntr = (int)doc.LineCntr;
							doc.LineCntr = 0;

							while (doc.LineCntr < OrigLineCntr)
							{
								ReleaseDocProc(je, ref doc, res);
								doc.Released = true;
							}
							ARAdjust reversal = ARAdjust_AdjgDocType_RefNbr_CustomerID.Select(doc.DocType, doc.RefNbr, OrigLineCntr);
							if (reversal != null)
							{
								doc.OpenDoc = true;
							}
						}

						ARDocument.Cache.Update(doc);
					}
					Caches[typeof(ARBalances)].Clear();

					foreach (ARRegister ardoc in ARDocument.Cache.Updated)
					{
						ARDocument.Cache.PersistUpdated(ardoc);
					}

					foreach (SOOrder order in PXSelectReadonly<SOOrder, Where<SOOrder.customerID, Equal<Required<SOOrder.customerID>>, And<SOOrder.completed, Equal<False>, And<SOOrder.cancelled, Equal<False>>>>>.Select(this, cust.BAccountID))
					{
						ARReleaseProcess.UpdateARBalances(this, order, order.UnbilledOrderTotal, order.OpenOrderTotal); 
					}

					foreach (ARRegister ardoc in PXSelectReadonly<ARRegister, Where<ARRegister.customerID, Equal<Required<ARRegister.customerID>>, And<ARRegister.released, Equal<True>, And<ARRegister.openDoc, Equal<True>>>>>.Select(this, cust.BAccountID))
					{
						ARReleaseProcess.UpdateARBalances(this, ardoc, ardoc.DocBal);
						UpdateARBalancesDates(ardoc);
					}

					foreach (ARRegister ardoc in PXSelectReadonly<ARRegister, Where<ARRegister.customerID, Equal<Required<ARRegister.customerID>>, And<ARRegister.released, Equal<False>, And<ARRegister.hold, Equal<False>, And<ARRegister.voided, Equal<False>, And<ARRegister.scheduled, Equal<False>>>>>>>.Select(this, cust.BAccountID))
					{
						ARReleaseProcess.UpdateARBalances(this, ardoc, ardoc.OrigDocAmt);
					}

					foreach (ARInvoice ardoc in PXSelectReadonly<ARInvoice, Where<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>, And<ARInvoice.creditHold, Equal<True>, And<ARInvoice.released, Equal<False>, And<ARInvoice.hold, Equal<False>, And<ARInvoice.voided, Equal<False>, And<ARInvoice.scheduled, Equal<False>>>>>>>>.Select(this, cust.BAccountID))
					{
						ardoc.CreditHold = false;
						ARReleaseProcess.UpdateARBalances(this, ardoc, -ardoc.OrigDocAmt);
					}

					this.RowInserting.RemoveHandler<ARHist>(ARHist_RowInserting);
					this.RowInserting.RemoveHandler<CuryARHist>(CuryARHist_RowInserting);

					Caches[typeof(ARHist)].Persist(PXDBOperation.Insert);

					Caches[typeof(CuryARHist)].Persist(PXDBOperation.Insert);

					Caches[typeof(ARBalances)].Persist(PXDBOperation.Insert);

					ts.Complete(this);
				}

				ARDocument.Cache.Persisted(false);

				Caches[typeof(ARHist)].Persisted(false);

				Caches[typeof(CuryARHist)].Persisted(false);

				Caches[typeof(ARBalances)].Persisted(false);
			}
		}

		protected static void Copy(ARSalesPerTran aDest, ARAdjust aAdj) 
		{
			aDest.AdjdDocType = aAdj.AdjdDocType;
			aDest.AdjdRefNbr = aAdj.AdjdRefNbr;
			aDest.AdjNbr = aAdj.AdjNbr;
            aDest.BranchID = aAdj.AdjdBranchID;
			aDest.Released = true;
		}

		protected static void Copy(ARSalesPerTran aDest, ARRegister aReg)
		{
			aDest.DocType = aReg.DocType;
			aDest.RefNbr = aReg.RefNbr;
		}

		protected static void CopyShare(ARSalesPerTran aDest, ARSalesPerTran aSrc, decimal aRatio, short aPrecision) 
		{
			aDest.SalespersonID = aSrc.SalespersonID;
			aDest.CuryInfoID = aSrc.CuryInfoID; //We will use currency Info of the orifginal invoice for the commission calculations
			aDest.CommnPct = aSrc.CommnPct;
			aDest.CuryCommnblAmt = Math.Round((decimal)(aRatio * aSrc.CuryCommnblAmt), aPrecision);
			aDest.CuryCommnAmt = Math.Round((decimal)(aRatio * aSrc.CuryCommnAmt), aPrecision);
		} 


	}
}

namespace PX.Objects.AR.Overrides.ARDocumentRelease
{
	public interface IBaseARHist
	{
		Boolean? DetDeleted
		{
			get;
			set;
		}

        Boolean? FinFlag
		{
			get;
			set;
		}
		Decimal? PtdCrAdjustments
		{
			get;
			set;
		}
		Decimal? PtdDrAdjustments
		{
			get;
			set;
		}
		Decimal? PtdSales
		{
			get;
			set;
		}
		Decimal? PtdPayments
		{
			get;
			set;
		}
		Decimal? PtdDiscounts
		{
			get;
			set;
		}
		Decimal? YtdBalance
		{
			get;
			set;
		}
		Decimal? BegBalance
		{
			get;
			set;
		}
		Decimal? PtdCOGS
		{
			get;
			set;
		}
		Decimal? PtdRGOL
		{
			get;
			set;
		}
		Decimal? PtdFinCharges
		{
			get;
			set;
		}
		Decimal? PtdDeposits
		{
			get;
			set;
		}
		Decimal? YtdDeposits
		{
			get;
			set;
		}
        Decimal? PtdItemDiscounts
        {
            get;
            set;
        }
	}

	public interface ICuryARHist
	{
		Decimal? CuryPtdCrAdjustments
		{
			get;
			set;
		}
		Decimal? CuryPtdDrAdjustments
		{
			get;
			set;
		}
		Decimal? CuryPtdSales
		{
			get;
			set;
		}
		Decimal? CuryPtdPayments
		{
			get;
			set;
		}
		Decimal? CuryPtdDiscounts
		{
			get;
			set;
		}
		Decimal? CuryPtdFinCharges
		{
			get;
			set;
		}
		Decimal? CuryYtdBalance
		{
			get;
			set;
		}
		Decimal? CuryBegBalance
		{
			get;
			set;
		}
		Decimal? CuryPtdDeposits
		{
			get;
			set;
		}
		Decimal? CuryYtdDeposits
		{
			get;
			set;
		}
	}

	[PXAccumulator(new Type[] {
				typeof(CuryARHistory.finYtdBalance),
				typeof(CuryARHistory.tranYtdBalance),
				typeof(CuryARHistory.curyFinYtdBalance),
				typeof(CuryARHistory.curyTranYtdBalance),
				typeof(CuryARHistory.finYtdBalance),
				typeof(CuryARHistory.tranYtdBalance),
				typeof(CuryARHistory.curyFinYtdBalance),
				typeof(CuryARHistory.curyTranYtdBalance),
				typeof(CuryARHistory.finYtdDeposits),
				typeof(CuryARHistory.tranYtdDeposits),
				typeof(CuryARHistory.curyFinYtdDeposits),
				typeof(CuryARHistory.curyTranYtdDeposits)
				},
					new Type[] {
				typeof(CuryARHistory.finBegBalance),
				typeof(CuryARHistory.tranBegBalance),
				typeof(CuryARHistory.curyFinBegBalance),
				typeof(CuryARHistory.curyTranBegBalance),
				typeof(CuryARHistory.finYtdBalance),
				typeof(CuryARHistory.tranYtdBalance),
				typeof(CuryARHistory.curyFinYtdBalance),
				typeof(CuryARHistory.curyTranYtdBalance),
				typeof(CuryARHistory.finYtdDeposits),
				typeof(CuryARHistory.tranYtdDeposits),
				typeof(CuryARHistory.curyFinYtdDeposits),
				typeof(CuryARHistory.curyTranYtdDeposits)
				}
			)]
    [Serializable]
	public partial class CuryARHist : CuryARHistory, ICuryARHist, IBaseARHist
	{
		#region BranchID
		public new abstract class branchID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		public override Int32? BranchID
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
		#region AccountID
		public new abstract class accountID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region CustomerID
		public new abstract class customerID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
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
		#region CuryID
		public new abstract class curyID : PX.Data.IBqlField
		{
		}
		[PXDBString(5, IsUnicode = true, IsKey = true, InputMask = ">LLLLL")]
		[PXDefault()]
		public override String CuryID
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
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		[PXDBString(6, IsKey = true, IsFixed = true)]
		[PXDefault()]
		public override String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
	}

	[PXAccumulator(new Type[] {
				typeof(ARHistory.finYtdBalance),
				typeof(ARHistory.tranYtdBalance),
				typeof(ARHistory.finYtdBalance),
				typeof(ARHistory.tranYtdBalance),
				typeof(ARHistory.finYtdDeposits),
				typeof(ARHistory.tranYtdDeposits)
				},
					new Type[] {
				typeof(ARHistory.finBegBalance),
				typeof(ARHistory.tranBegBalance),
				typeof(ARHistory.finYtdBalance),
				typeof(ARHistory.tranYtdBalance),
				typeof(ARHistory.finYtdDeposits),
				typeof(ARHistory.tranYtdDeposits)
				}
			)]
    [Serializable]
	public partial class ARHist : ARHistory, IBaseARHist
	{
		#region BranchID
		public new abstract class branchID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		public override Int32? BranchID
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
		#region AccountID
		public new abstract class accountID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region CustomerID
		public new abstract class customerID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
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
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		[PXDBString(6, IsKey = true, IsFixed = true)]
		[PXDefault()]
		public override String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
	}

	public class ARBalAccumAttribute : PXAccumulatorAttribute
	{
		public ARBalAccumAttribute()
		{
			base._SingleRecord = true;
		}
		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			ARBalances bal = (ARBalances)row;

			columns.Update<ARBalances.lastInvoiceDate>(bal.LastInvoiceDate, PXDataFieldAssign.AssignBehavior.Maximize);
			columns.Update<ARBalances.numberInvoicePaid>(bal.NumberInvoicePaid, PXDataFieldAssign.AssignBehavior.Summarize);
			if (bal.DatesUpdated == true)
			{
				columns.Update<ARBalances.oldInvoiceDate>(bal.OldInvoiceDate, PXDataFieldAssign.AssignBehavior.Replace);
				columns.Restrict<ARBalances.Tstamp>(PXComp.LE, bal.tstamp ?? sender.Graph.TimeStamp);
			}
			else
			{
				columns.Update<ARBalances.oldInvoiceDate>(bal.OldInvoiceDate, PXDataFieldAssign.AssignBehavior.Minimize);
			}
			columns.Update<ARBalances.paidInvoiceDays>(bal.PaidInvoiceDays, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<ARBalances.lastModifiedByID>(bal.LastModifiedByID, PXDataFieldAssign.AssignBehavior.Replace);
			columns.Update<ARBalances.lastModifiedByScreenID>(bal.LastModifiedByScreenID, PXDataFieldAssign.AssignBehavior.Replace);
			columns.Update<ARBalances.lastModifiedDateTime>(bal.LastModifiedDateTime, PXDataFieldAssign.AssignBehavior.Replace);

			return bal.LastInvoiceDate != null
				|| bal.NumberInvoicePaid != null
				|| bal.DatesUpdated == true
				|| bal.OldInvoiceDate != null
				|| bal.PaidInvoiceDays != null
				|| bal.CuryID != null
				|| bal.CurrentBal != 0m
				|| bal.UnreleasedBal != 0m
				|| bal.TotalOpenOrders != 0
				|| bal.TotalPrepayments != 0
				|| bal.TotalQuotations != 0
				|| bal.TotalShipped != 0;
		}
	}
}
