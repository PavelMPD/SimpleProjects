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
using PX.Objects.CA;

namespace PX.Objects.TX
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class ProcessPendingVAT : PXGraph<ProcessPendingVAT>
	{
		public PXCancel<TaxTran> Cancel;
		[PXFilterable]
		public PXProcessing<TaxTran, Where<TaxTran.taxType, Equal<TaxType.pendingSales>, And<TaxTran.released, Equal<boolTrue>, And<TaxTran.voided, Equal<False>, And<TaxTran.taxInvoiceNbr, IsNull>>>>> CustomerSVAT;

		[PXFilterable]
		public PXProcessing<TaxTran, Where<TaxTran.taxType, Equal<TaxType.pendingPurchase>, And<TaxTran.released, Equal<boolTrue>, And<TaxTran.voided, Equal<False>, And<TaxTran.taxInvoiceNbr, IsNull>>>>> VendorSVAT;

		protected virtual void TaxTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			switch (((TaxTran)e.Row).Module)
			{
				case "AR":
					PXStringListAttribute.SetList<TaxTran.tranType>(CustomerSVAT.Cache, null,
						new string[] { ARInvoiceType.Invoice, ARInvoiceType.DebitMemo, ARInvoiceType.CreditMemo, ARInvoiceType.CashSale, ARInvoiceType.CashReturn },
						new string[] { AR.Messages.Invoice, AR.Messages.DebitMemo, AR.Messages.CreditMemo, AR.Messages.CashSale, AR.Messages.CashReturn });
					break;
				case "AP":
				case "CA":
					PXStringListAttribute.SetList<TaxTran.tranType>(VendorSVAT.Cache, null,
						new string[] { APInvoiceType.Invoice, APInvoiceType.DebitAdj, APInvoiceType.CreditAdj, APInvoiceType.QuickCheck, APInvoiceType.VoidQuickCheck, CAAPARTranType.CAAdjustment, CAAPARTranType.CATransferExp },
						new string[] { AP.Messages.Invoice, AP.Messages.DebitAdj, AP.Messages.CreditAdj, AP.Messages.QuickCheck, AP.Messages.VoidQuickCheck, CA.Messages.CAAdjustment, CA.Messages.CATransferExp });
					break;
			}
		}

		public ProcessPendingVAT()
		{
			CustomerSVAT.SetProcessDelegate(ProcessPendingVATProc);
			CustomerSVAT.SetProcessAllVisible(false);

			VendorSVAT.SetProcessDelegate(ProcessPendingVATProc);
			VendorSVAT.SetProcessAllVisible(false);

			PXUIFieldAttribute.SetEnabled<TaxTran.taxInvoiceDate>(CustomerSVAT.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<TaxTran.taxInvoiceNbr>(CustomerSVAT.Cache, null, true);
		}

		public static void ProcessPendingVATProc(List<TaxTran> list)
		{
			JournalEntry je = PXGraph.CreateInstance<JournalEntry>();

			PXCache dummycache = je.Caches[typeof(TaxTran)];
			dummycache = je.Caches[typeof(VATTaxTran)];
			je.Views.Caches.Add(typeof(VATTaxTran));

			DocumentList<Batch> created = new DocumentList<Batch>(je);

			foreach (TaxTran taxtran in list)
			{
                PXProcessing<TaxTran>.SetCurrentItem(taxtran);
                if (string.IsNullOrEmpty(taxtran.TaxInvoiceNbr) == true || taxtran.TaxInvoiceDate == null)
                {
                    //PXProcessing<TaxTran>.SetWarning(Messages.CannotProcessW);
                    PXProcessing<TaxTran>.SetError(Messages.CannotProcessW);
                }
                else
				{
					using (PXTransactionScope ts = new PXTransactionScope())
					{
						foreach (PXResult<VATTaxTran, CurrencyInfo, Tax> res  in PXSelectJoin<VATTaxTran, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<VATTaxTran.curyInfoID>>, InnerJoin<Tax, On<Tax.taxID, Equal<VATTaxTran.taxID>>>>, Where<VATTaxTran.module, Equal<Current<TaxTran.module>>, And<VATTaxTran.tranType, Equal<Current<TaxTran.tranType>>, And<VATTaxTran.refNbr, Equal<Current<TaxTran.refNbr>>, And<VATTaxTran.taxID, Equal<Current<TaxTran.taxID>>>>>>>.SelectSingleBound(je, new object[] { taxtran }))
						{
							VATTaxTran n = (VATTaxTran)res;
							CurrencyInfo info = (CurrencyInfo)res;
							Tax x = (Tax)res;

							string strFinPeriodID = FinPeriodSelectorAttribute.PeriodFromDate(taxtran.TaxInvoiceDate);
							SegregateBatch(je, info.CuryID, taxtran.TaxInvoiceDate, strFinPeriodID, created);

							n.TaxInvoiceNbr = taxtran.TaxInvoiceNbr;
							n.TaxInvoiceDate = taxtran.TaxInvoiceDate;

							je.Caches[typeof(VATTaxTran)].Update(n);

                            CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(info);
                            new_info.CuryInfoID = null;
                            new_info.ModuleCode = "GL";
                            new_info.BaseCalc = false;
                            new_info = je.currencyinfo.Insert(new_info) ?? new_info;

							//reverse original transaction
							{
								GLTran tran = new GLTran();
								tran.AccountID = n.AccountID;
								tran.SubID = n.SubID;
								tran.CuryDebitAmt = (n.TaxType == TaxType.PendingSales && ReportTaxProcess.GetMult(n) == 1m) ? n.CuryTaxAmt : 0m;
								tran.DebitAmt = (n.TaxType == TaxType.PendingSales && ReportTaxProcess.GetMult(n) == 1m) ? n.TaxAmt : 0m;
								tran.CuryCreditAmt = (n.TaxType == TaxType.PendingSales && ReportTaxProcess.GetMult(n) == 1m) ? 0m : n.CuryTaxAmt;
								tran.CreditAmt = (n.TaxType == TaxType.PendingSales && ReportTaxProcess.GetMult(n) == 1m) ? 0m : n.TaxAmt;
								tran.TranType = n.TranType;
								tran.TranClass = "N";
								tran.RefNbr = n.RefNbr;
								tran.TranDesc = n.TaxInvoiceNbr;
								tran.TranPeriodID = strFinPeriodID;
								tran.FinPeriodID = strFinPeriodID;
								tran.TranDate = n.TaxInvoiceDate;
								tran.CuryInfoID = new_info.CuryInfoID;
								tran.Released = true;
								
								je.GLTranModuleBatNbr.Insert(tran);

								VATTaxTran newtran = PXCache<VATTaxTran>.CreateCopy(n);
								newtran.Module = "GL";
								newtran.TranType = (n.TaxType == TaxType.PendingSales) ? TaxAdjustmentType.ReverseOutputVAT : TaxAdjustmentType.ReverseInputVAT;
								newtran.RefNbr = newtran.TaxInvoiceNbr;
								newtran.TranDate = newtran.TaxInvoiceDate;
								newtran.CuryTaxableAmt = -ReportTaxProcess.GetMult(n.Module, n.TranType, (n.TaxType == TaxType.PendingSales) ? TaxType.Sales : TaxType.Purchase, (short)1) * newtran.CuryTaxableAmt;
								newtran.TaxableAmt = -ReportTaxProcess.GetMult(n.Module, n.TranType, (n.TaxType == TaxType.PendingSales) ? TaxType.Sales : TaxType.Purchase, (short)1) * newtran.TaxableAmt;
								newtran.CuryTaxAmt = -ReportTaxProcess.GetMult(n.Module, n.TranType, (n.TaxType == TaxType.PendingSales) ? TaxType.Sales : TaxType.Purchase, (short)1) * newtran.CuryTaxAmt;
								newtran.TaxAmt = -ReportTaxProcess.GetMult(n.Module, n.TranType, (n.TaxType == TaxType.PendingSales) ? TaxType.Sales : TaxType.Purchase, (short)1) * newtran.TaxAmt;

								je.Caches[typeof(VATTaxTran)].Insert(newtran);
							}

							//reclassify to VAT account
							{
								GLTran tran = new GLTran();
								tran.AccountID = (n.TaxType == TaxType.PendingSales) ? x.SalesTaxAcctID : x.PurchTaxAcctID;
								tran.SubID = (n.TaxType == TaxType.PendingSales) ? x.SalesTaxSubID : x.PurchTaxSubID;
								tran.CuryDebitAmt = (n.TaxType == TaxType.PendingSales && ReportTaxProcess.GetMult(n) == 1m) ? 0m : n.CuryTaxAmt;
								tran.DebitAmt = (n.TaxType == TaxType.PendingSales && ReportTaxProcess.GetMult(n) == 1m) ? 0m : n.TaxAmt;
								tran.CuryCreditAmt = (n.TaxType == TaxType.PendingSales && ReportTaxProcess.GetMult(n) == 1m) ? n.CuryTaxAmt : 0m;
								tran.CreditAmt = (n.TaxType == TaxType.PendingSales && ReportTaxProcess.GetMult(n) == 1m) ? n.TaxAmt : 0m;
								tran.TranType = n.TranType;
								tran.TranClass = "N";
								tran.RefNbr = n.RefNbr;
								tran.TranDesc = n.TaxInvoiceNbr;
								tran.TranPeriodID = strFinPeriodID;
								tran.FinPeriodID = strFinPeriodID;
								tran.TranDate = n.TaxInvoiceDate;
								tran.CuryInfoID = new_info.CuryInfoID;
								tran.Released = true;
								
								je.GLTranModuleBatNbr.Insert(tran);

								VATTaxTran newtran = PXCache<VATTaxTran>.CreateCopy(n);
								newtran.Module = "GL";
								newtran.TranType = (n.TaxType == TaxType.PendingSales) ? TaxAdjustmentType.OutputVAT : TaxAdjustmentType.InputVAT;
								newtran.TaxType = (n.TaxType == TaxType.PendingSales) ? TaxType.Sales : TaxType.Purchase;
								newtran.AccountID = (n.TaxType == TaxType.PendingSales) ? x.SalesTaxAcctID : x.PurchTaxAcctID;
								newtran.SubID = (n.TaxType == TaxType.PendingSales) ? x.SalesTaxSubID : x.PurchTaxSubID;
								newtran.RefNbr = newtran.TaxInvoiceNbr;
								newtran.TranDate = newtran.TaxInvoiceDate;
								newtran.CuryTaxableAmt = ReportTaxProcess.GetMult(n.Module, n.TranType, (n.TaxType == TaxType.PendingSales) ? TaxType.Sales : TaxType.Purchase, (short)1) * newtran.CuryTaxableAmt;
								newtran.TaxableAmt = ReportTaxProcess.GetMult(n.Module, n.TranType, (n.TaxType == TaxType.PendingSales) ? TaxType.Sales : TaxType.Purchase, (short)1) * newtran.TaxableAmt;
								newtran.CuryTaxAmt = ReportTaxProcess.GetMult(n.Module, n.TranType, (n.TaxType == TaxType.PendingSales) ? TaxType.Sales : TaxType.Purchase, (short)1) * newtran.CuryTaxAmt;
								newtran.TaxAmt = ReportTaxProcess.GetMult(n.Module, n.TranType, (n.TaxType == TaxType.PendingSales) ? TaxType.Sales : TaxType.Purchase, (short)1) * newtran.TaxAmt;

								je.Caches[typeof(VATTaxTran)].Insert(newtran);
							}
						}

						je.Save.Press();

						ts.Complete();
					}
                    PXProcessing<TaxTran>.SetProcessed();
				}
			}
		}

		private static void SegregateBatch(JournalEntry je, string CuryID, DateTime? DocDate, string FinPeriodID, DocumentList<Batch> created)
		{
			if (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted)
			{
				je.Save.Press();

				if (created.Find(je.BatchModule.Current) == null)
				{
					created.Add(je.BatchModule.Current);
				}
			}

			Batch arbatch = created.Find<Batch.curyID, Batch.finPeriodID>(CuryID, FinPeriodID) ?? new Batch();

			if (arbatch.BatchNbr != null)
			{
				je.BatchModule.Current = je.BatchModule.Search<Batch.batchNbr>(arbatch.BatchNbr, arbatch.Module);
				je.TimeStamp = arbatch.tstamp;
			}
			else
			{
				je.Clear();

				CurrencyInfo info = new CurrencyInfo();
				info.CuryID = CuryID;
				info.CuryEffDate = DocDate;
				info = je.currencyinfo.Insert(info) ?? info;

				arbatch = new Batch();
				arbatch.Module = "GL";
				arbatch.Status = "U";
				arbatch.Released = true;
				arbatch.Hold = false;
				arbatch.DateEntered = DocDate;
				arbatch.FinPeriodID = FinPeriodID;
				arbatch.TranPeriodID = FinPeriodID;
				arbatch.CuryID = CuryID;
				arbatch.CuryInfoID = info.CuryInfoID;
				arbatch.DebitTotal = 0m;
				arbatch.CreditTotal = 0m;
				je.BatchModule.Insert(arbatch);

				CurrencyInfo b_info = je.currencyinfo.Select();
				if (b_info != null)
				{
					b_info.CuryID = CuryID;
					b_info.CuryEffDate = DocDate;
					je.currencyinfo.Update(b_info);
				}
			}
		}
	}


    [Serializable]
	public partial class VATTaxTran : TaxTran
	{
		#region Selected
		public new abstract class selected : IBqlField
		{
		}
		#endregion
		#region Module
		public new abstract class module : PX.Data.IBqlField
		{
		}
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault()]
		public override String Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;
			}
		}
		#endregion
		#region TranType
		public new abstract class tranType : PX.Data.IBqlField
		{
		}
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		public override String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		public override String RefNbr
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
		#region Released
		public new abstract class released : PX.Data.IBqlField
		{
		}
		#endregion
		#region Voided
		public new abstract class voided : PX.Data.IBqlField
		{
		}
		#endregion
		#region TaxPeriodID
		public new abstract class taxPeriodID : PX.Data.IBqlField
		{
		}
		[GL.FinPeriodID()]
		public override String TaxPeriodID
		{
			get
			{
				return this._TaxPeriodID;
			}
			set
			{
				this._TaxPeriodID = value;
			}
		}
		#endregion
        #region FinPeriodID
        public new abstract class finPeriodID : PX.Data.IBqlField
        {
        }
        [GL.FinPeriodID()]
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
		#region TaxID
		public new abstract class taxID : PX.Data.IBqlField
		{
		}
		[PXDBString(Tax.taxID.Length, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		public override String TaxID
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
		#region VendorID
		public new abstract class vendorID : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
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
		#region TaxZoneID
		public new abstract class taxZoneID : PX.Data.IBqlField
		{
		}
		[PXDBString(10, IsUnicode = true)]
		[PXDefault()]
		public override String TaxZoneID
		{
			get
			{
				return this._TaxZoneID;
			}
			set
			{
				this._TaxZoneID = value;
			}
		}
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
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
		[PXDBInt()]
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
		#region TranDate
		public new abstract class tranDate : PX.Data.IBqlField
		{
		}
		[PXDBDate()]
		[PXDefault()]
		public override DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		#region TaxType
		public new abstract class taxType : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault()]
		public override String TaxType
		{
			get
			{
				return this._TaxType;
			}
			set
			{
				this._TaxType = value;
			}
		}
		#endregion
		#region TaxBucketID
		public new abstract class taxBucketID : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
		[PXDefault()]
		public override Int32? TaxBucketID
		{
			get
			{
				return this._TaxBucketID;
			}
			set
			{
				this._TaxBucketID = value;
			}
		}
		#endregion
		#region TaxInvoiceNbr
		public new abstract class taxInvoiceNbr : PX.Data.IBqlField
		{
		}
		#endregion
		#region TaxInvoiceDate
		public new abstract class taxInvoiceDate : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		[PXDBLong()]
		[PXDefault()]
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
		#region CuryTaxableAmt
		public new abstract class curyTaxableAmt : PX.Data.IBqlField
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? CuryTaxableAmt
		{
			get
			{
				return this._CuryTaxableAmt;
			}
			set
			{
				this._CuryTaxableAmt = value;
			}
		}
		#endregion
		#region TaxableAmt
		public new abstract class taxableAmt : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryTaxAmt
		public new abstract class curyTaxAmt : PX.Data.IBqlField
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? CuryTaxAmt
		{
			get
			{
				return this._CuryTaxAmt;
			}
			set
			{
				this._CuryTaxAmt = value;
			}
		}
		#endregion
		#region TaxAmt
		public new abstract class taxAmt : PX.Data.IBqlField
		{
		}
		#endregion
	}

}
