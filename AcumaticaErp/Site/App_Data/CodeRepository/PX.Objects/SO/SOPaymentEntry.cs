using System;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Data;

namespace PX.Objects.SO
{
	public class SOPaymentEntry : ARPaymentEntry
	{
		public SOPaymentEntry()
		{ }
	}
#if false
	public class SOPaymentEntry : ARPaymentEntry
	{
		public PXSelectJoin<SOAdjust, LeftJoin<SOOrder, On<SOOrder.orderType, Equal<SOAdjust.adjdOrderType>, And<SOOrder.orderNbr, Equal<SOAdjust.adjdOrderNbr>>>>, Where<SOAdjust.adjgDocType, Equal<Current<ARPayment.docType>>, And<SOAdjust.adjgRefNbr, Equal<Current<ARPayment.refNbr>>>>> SOAdjustments;
		//public PXSelectJoin<SOAdjust, LeftJoin<SOOrder, On<SOOrder.orderType, Equal<SOAdjust.adjdOrderType>, And<SOOrder.orderNbr, Equal<SOAdjust.adjdOrderNbr>>>>, Where<SOAdjust.adjgDocType, Equal<Current<ARPayment.docType>>, And<SOAdjust.adjgRefNbr, Equal<Current<ARPayment.refNbr>>, And<SOAdjust.billed, Equal<True>>>>> SOAdjustments_History;
		public PXSelectJoin<SOAdjust, InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOAdjust.adjdOrderType>, And<SOOrder.orderNbr, Equal<SOAdjust.adjdOrderNbr>>>, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<SOOrder.curyInfoID>>>>, Where<SOAdjust.adjgDocType, Equal<Current<ARPayment.docType>>, And<SOAdjust.adjgRefNbr, Equal<Current<ARPayment.refNbr>>>>> SOAdjustments_Orders;
		public PXSelect<SOAdjust, Where<SOAdjust.adjgDocType, Equal<Current<ARPayment.docType>>, And<SOAdjust.adjgRefNbr, Equal<Current<ARPayment.refNbr>>>>> SOAdjustments_Raw;
		public PXSelect<SOOrder, Where<SOOrder.customerID, Equal<Required<SOOrder.customerID>>, And<SOOrder.orderType, Equal<Required<SOOrder.orderType>>, And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>> SOOrder_CustomerID_OrderType_RefNbr;


		public SOPaymentEntry()
		{
			this.Views.Caches.Remove(typeof (CurrencyInfo));
			this.Views.Caches.Add(typeof(CurrencyInfo));
        }

		[PXUIField(DisplayName = "Void", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = false, Visible = false)]
		[PXProcessButton]
		public override IEnumerable VoidCheck(PXAdapter adapter)
		{
			return adapter.Get();
		}

		protected virtual IEnumerable soadjustments()
		{
			PXResultset<SOAdjust, SOOrder> ret = new PXResultset<SOAdjust, SOOrder>();

			int startRow = PXView.StartRow;
			int totalRows = 0;

			if (Document.Current == null || Document.Current.DocType != ARDocType.Refund)
			{
				foreach (PXResult<SOAdjust, SOOrder> res in SOAdjustments_Orders.View.Select(PXView.Currents, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
				{
					if (SOAdjustments.Cache.GetStatus((SOAdjust)res) == PXEntryStatus.Notchanged)
					{
						SOOrder invoice = PXCache<SOOrder>.CreateCopy(res);
						SOAdjust adj = res;

						SOAdjust other = PXSelectGroupBy<SOAdjust, Where<SOAdjust.adjdOrderType, Equal<Required<SOAdjust.adjdOrderType>>, And<SOAdjust.adjdOrderNbr, Equal<Required<SOAdjust.adjdOrderNbr>>, And<Where<SOAdjust.adjgDocType, NotEqual<Required<SOAdjust.adjgDocType>>, Or<SOAdjust.adjgRefNbr, NotEqual<Required<SOAdjust.adjgRefNbr>>>>>>>, Aggregate<GroupBy<SOAdjust.adjdOrderType, GroupBy<SOAdjust.adjdOrderNbr, Sum<SOAdjust.curyAdjdAmt, Sum<SOAdjust.adjAmt>>>>>>.Select(this, adj.AdjdOrderType, adj.AdjdOrderNbr, adj.AdjgDocType, adj.AdjgRefNbr);
						if (other != null && other.AdjdOrderNbr != null)
						{
							invoice.CuryDocBal -= other.CuryAdjdAmt;
							invoice.DocBal -= other.AdjAmt;
						}

						CalcBalances<SOOrder>(adj, invoice, true, false);
					}
					ret.Add(res);
				}
			}

			PXView.StartRow = 0;

			return ret;
		}

		protected void CalcBalances<T>(SOAdjust adj, T invoice, bool isCalcRGOL)
			where T : class, IBqlTable, IInvoice
		{
			CalcBalances<T>(adj, invoice, isCalcRGOL, true);
		}

		protected void CalcBalances<T>(SOAdjust adj, T invoice, bool isCalcRGOL, bool DiscOnDiscDate)
			where T : class, IBqlTable, IInvoice
		{
			PaymentEntry.CalcBalances<T, SOAdjust>(CurrencyInfo_CuryInfoID, adj.AdjgCuryInfoID, adj.AdjdCuryInfoID, invoice, adj);
			if (DiscOnDiscDate)
			{
				PaymentEntry.CalcDiscount<T, SOAdjust>(adj.AdjgDocDate, invoice, adj);
			}
			PaymentEntry.WarnDiscount<T, SOAdjust>(this, adj.AdjgDocDate, invoice, adj);

			PaymentEntry.AdjustBalance<SOAdjust>(CurrencyInfo_CuryInfoID, adj);
			if (isCalcRGOL && (adj.Voided != true))
			{
				PaymentEntry.CalcRGOL<T, SOAdjust>(CurrencyInfo_CuryInfoID, invoice, adj);
				adj.RGOLAmt = (bool)adj.ReverseGainLoss ? -adj.RGOLAmt : adj.RGOLAmt;
			}
		}
		protected void CalcBalances(SOAdjust row, bool isCalcRGOL)
		{
			CalcBalances(row, isCalcRGOL, true);
		}

		protected void CalcBalances(SOAdjust adj, bool isCalcRGOL, bool DiscOnDiscDate)
		{
			foreach (PXResult<SOOrder> res in SOOrder_CustomerID_OrderType_RefNbr.Select(adj.CustomerID, adj.AdjdOrderType, adj.AdjdOrderNbr))
			{
				SOOrder invoice = PXCache<SOOrder>.CreateCopy(res);

				internalCall = true;
				SOAdjust other = PXSelectGroupBy<SOAdjust, Where<SOAdjust.adjdOrderType, Equal<Required<SOAdjust.adjdOrderType>>, And<SOAdjust.adjdOrderNbr, Equal<Required<SOAdjust.adjdOrderNbr>>, And<Where<SOAdjust.adjgDocType, NotEqual<Required<SOAdjust.adjgDocType>>, Or<SOAdjust.adjgRefNbr, NotEqual<Required<SOAdjust.adjgRefNbr>>>>>>>, Aggregate<GroupBy<SOAdjust.adjdOrderType, GroupBy<SOAdjust.adjdOrderNbr, Sum<SOAdjust.curyAdjdAmt, Sum<SOAdjust.adjAmt>>>>>>.Select(this, adj.AdjdOrderType, adj.AdjdOrderNbr, adj.AdjgDocType, adj.AdjgRefNbr);
				if (other != null && other.AdjdOrderNbr != null)
				{
					invoice.CuryDocBal -= other.CuryAdjdAmt;
					invoice.DocBal -= other.AdjAmt;
				}
				internalCall = false;
				
				CalcBalances<SOOrder>(adj, invoice, isCalcRGOL, DiscOnDiscDate);
				return;
			}
		}

		#region ARPayment Events

		protected override void ARPayment_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			bool calcFlag = (e.Row != null && ((ARPayment)e.Row).CuryApplAmt == null);

			base.ARPayment_RowSelected(cache, e);

			if (e.Row == null) return;

			if (calcFlag)
			{
				bool IsReadOnly = (cache.GetStatus(e.Row) == PXEntryStatus.Notchanged);
				PXFormulaAttribute.CalcAggregate<SOAdjust.curyAdjgAmt>(SOAdjustments.Cache, e.Row, IsReadOnly);
				cache.RaiseFieldUpdated<ARPayment.curySOApplAmt>(e.Row, null);

				PXDBCurrencyAttribute.CalcBaseValues<ARPayment.curySOApplAmt>(cache, e.Row);
				PXDBCurrencyAttribute.CalcBaseValues<ARPayment.curyUnappliedBal>(cache, e.Row);
			}

			if (e.Row != null && ((ARPayment)e.Row).CuryApplAmt > 0m)
			{
				cache.RaiseExceptionHandling<ARPayment.curyUnappliedBal>(e.Row, null, new PXSetPropertyException(Messages.UnappliedBalanceIncludesAR, PXErrorLevel.Warning));
			}

			PXUIFieldAttribute.SetEnabled<ARPayment.curySOApplAmt>(cache, e.Row, false);

			if (((ARPayment)e.Row).OpenDoc == true)
			{
				bool HoldAdj = false;
				foreach (SOAdjust adj in SOAdjustments_Orders.Select())
				{
					if (adj.Hold == true)
					{
						HoldAdj = true;
						break;
					}
				}

				PXUIFieldAttribute.SetEnabled<ARPayment.adjDate>(cache, e.Row, (HoldAdj == false));
				PXUIFieldAttribute.SetEnabled<ARPayment.adjFinPeriodID>(cache, e.Row, (HoldAdj == false));
				PXUIFieldAttribute.SetEnabled<ARPayment.hold>(cache, e.Row, (HoldAdj == false));

				cache.AllowUpdate &= (HoldAdj == false);
				Adjustments.Cache.AllowDelete &= (HoldAdj == false);
				Adjustments.Cache.AllowInsert &= (HoldAdj == false);
				Adjustments.Cache.AllowUpdate &= (HoldAdj == false);
			}

			bool hasAdjustments = (Adjustments_Raw.View.SelectSingleBound(new object[] { e.Row }) != null);
			bool hasSOAdjustments = (SOAdjustments_Raw.View.SelectSingleBound(new object[] { e.Row }) != null);
			release.SetEnabled(((ARPayment)e.Row).Released == false && ((ARPayment)e.Row).OpenDoc == true && hasAdjustments == false && ((ARPayment)e.Row).Hold == false);
			PXUIFieldAttribute.SetEnabled<ARPayment.customerID>(cache, e.Row, ((ARPayment)e.Row).Released == false && ((ARPayment)e.Row).OpenDoc == true && !hasSOAdjustments);
		}

		protected override void ARPayment_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			bool calcFlag = (e.Row != null && ((ARPayment)e.Row).CuryApplAmt == null && !e.IsReadOnly);

			base.ARPayment_RowSelecting(sender, e);

			if (calcFlag)
			{
				using (new PXConnectionScope())
				{
					//Should be exactly duplicated in _RowSelected for Redirects
					bool IsReadOnly = (sender.GetStatus(e.Row) == PXEntryStatus.Notchanged);
					PXFormulaAttribute.CalcAggregate<SOAdjust.curyAdjgAmt>(SOAdjustments.Cache, e.Row, IsReadOnly);
					sender.RaiseFieldUpdated<ARPayment.curySOApplAmt>(e.Row, null);

					PXDBCurrencyAttribute.CalcBaseValues<ARPayment.curySOApplAmt>(sender, e.Row);
					PXDBCurrencyAttribute.CalcBaseValues<ARPayment.curyUnappliedBal>(sender, e.Row);
				}
			}
		}
		#endregion

		#region SOAdjust Events
		protected virtual void SOAdjust_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			SOAdjust adj = (SOAdjust)e.Row;
			if (adj == null)
			{
				return;
			}

			bool adjNotReleased = (adj.Released != true);

			PXUIFieldAttribute.SetEnabled<SOAdjust.adjdOrderType>(cache, adj, adjNotReleased);
			PXUIFieldAttribute.SetEnabled<SOAdjust.adjdOrderNbr>(cache, adj, adjNotReleased);
			PXUIFieldAttribute.SetEnabled<SOAdjust.curyAdjgAmt>(cache, adj, adjNotReleased);
		}

		protected virtual void SOAdjust_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			string errmsg = PXUIFieldAttribute.GetError<SOAdjust.adjdOrderNbr>(sender, e.Row);

			e.Cancel = (((SOAdjust)e.Row).AdjdOrderNbr == null || string.IsNullOrEmpty(errmsg) == false);
		}

		protected virtual void SOAdjust_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			if (((SOAdjust)e.Row).CuryAdjdBilledAmt > 0m)
			{
				throw new PXSetPropertyException(ErrorMessages.CantDeleteRecord);
			}
		}

		protected virtual void SOAdjust_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (((SOAdjust)e.Row).AdjdCuryInfoID != ((SOAdjust)e.Row).AdjgCuryInfoID && ((SOAdjust)e.Row).AdjdCuryInfoID != ((SOAdjust)e.Row).AdjdOrigCuryInfoID)
			{
				foreach (CurrencyInfo info in CurrencyInfo_CuryInfoID.Select(((SOAdjust)e.Row).AdjdCuryInfoID))
				{
					currencyinfo.Delete(info);
				}
			}
		}

		protected virtual void SOAdjust_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			SOAdjust doc = (SOAdjust)e.Row;

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete)
			{
				return;
			}

			if (((DateTime)doc.AdjdOrderDate).CompareTo((DateTime)Document.Current.AdjDate) > 0)
			{
				if (sender.RaiseExceptionHandling<SOAdjust.adjdOrderNbr>(e.Row, doc.AdjdOrderNbr, new PXSetPropertyException(AR.Messages.ApplDate_Less_DocDate, PXErrorLevel.RowError, PXUIFieldAttribute.GetDisplayName<ARPayment.adjDate>(Document.Cache))))
				{
					throw new PXRowPersistingException(PXDataUtils.FieldName<SOAdjust.adjdOrderDate>(), doc.AdjdOrderDate, AR.Messages.ApplDate_Less_DocDate, PXUIFieldAttribute.GetDisplayName<ARPayment.adjDate>(Document.Cache));
				}
			}
			/*
			if (((string)doc.AdjdFinPeriodID).CompareTo((string)Document.Current.AdjFinPeriodID) > 0)
			{
				if (sender.RaiseExceptionHandling<SOAdjust.adjdOrderNbr>(e.Row, doc.AdjdOrderNbr, new PXSetPropertyException(AR.Messages.ApplPeriod_Less_DocPeriod, PXErrorLevel.RowError, PXUIFieldAttribute.GetDisplayName<ARPayment.adjFinPeriodID>(Document.Cache))))
				{
					throw new PXRowPersistingException(PXDataUtils.FieldName<SOAdjust.adjdFinPeriodID>(), doc.AdjdFinPeriodID, AR.Messages.ApplPeriod_Less_DocPeriod, PXUIFieldAttribute.GetDisplayName<ARPayment.adjFinPeriodID>(Document.Cache));
				}
			}
			*/
			if (doc.CuryDocBal < 0m)
			{
				sender.RaiseExceptionHandling<SOAdjust.curyAdjgAmt>(e.Row, doc.CuryAdjgAmt, new PXSetPropertyException(AR.Messages.DocumentBalanceNegative));
			}


			/*
			if (Document.Current.DocType == ARDocType.Prepayment)
			{
				if (((DateTime)doc.AdjdOrderDate).CompareTo((DateTime)Document.Current.DocDate) < 0)
				{
					if (sender.RaiseExceptionHandling<SOAdjust.adjdOrderNbr>(e.Row, doc.AdjdOrderNbr, new PXSetPropertyException(AR.Messages.ApplDate_Less_DocDate, PXErrorLevel.RowError, PXUIFieldAttribute.GetDisplayName<ARPayment.docDate>(Document.Cache))))
					{
						throw new PXRowPersistingException(PXDataUtils.FieldName<SOAdjust.adjdDocDate>(), doc.AdjdOrderDate, AR.Messages.ApplDate_Less_DocDate, PXUIFieldAttribute.GetDisplayName<ARPayment.docDate>(Document.Cache));
					}
				}

				if (((string)doc.AdjdFinPeriodID).CompareTo((string)Document.Current.FinPeriodID) < 0)
				{
					if (sender.RaiseExceptionHandling<SOAdjust.adjdOrderNbr>(e.Row, doc.AdjdOrderNbr, new PXSetPropertyException(AR.Messages.ApplPeriod_Greater_DocPeriod, PXErrorLevel.RowError, PXUIFieldAttribute.GetDisplayName<ARPayment.finPeriodID>(Document.Cache))))
					{
						throw new PXRowPersistingException(PXDataUtils.FieldName<SOAdjust.adjdFinPeriodID>(), doc.AdjdFinPeriodID, AR.Messages.ApplPeriod_Greater_DocPeriod, PXUIFieldAttribute.GetDisplayName<ARPayment.finPeriodID>(Document.Cache));
					}
				}
			}
			*/
		}

		protected virtual void SOAdjust_AdjdOrderNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = (Document.Current != null && Document.Current.VoidAppl == true || this._AutoPaymentApp);
		}

		protected virtual void SOAdjust_AdjdOrderNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			try
			{
				SOAdjust adj = (SOAdjust)e.Row;
				if (adj.AdjdCuryInfoID == null)
				{
					foreach (PXResult<SOOrder, CurrencyInfo> res in PXSelectJoin<SOOrder, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<SOOrder.curyInfoID>>>, Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>, And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>.Select(this, adj.AdjdOrderType, adj.AdjdOrderNbr))
					{
						SOAdjust_AdjdOrderNbr_FieldUpdated<SOOrder>(res, adj);
						return;
					}
				}
			}
			catch (PXSetPropertyException ex)
			{
				throw new PXException(ex.Message);
			}
		}

		private void SOAdjust_AdjdOrderNbr_FieldUpdated<T>(PXResult<T, CurrencyInfo> res, SOAdjust adj)
			where T : SOOrder, IInvoice, new()
		{
			CurrencyInfo info_copy = PXCache<CurrencyInfo>.CreateCopy((CurrencyInfo)res);
			info_copy.CuryInfoID = null;
			info_copy = (CurrencyInfo)currencyinfo.Cache.Insert(info_copy);
			T invoice = PXCache<T>.CreateCopy((T)res);

			//currencyinfo.Cache.SetValueExt<CurrencyInfo.curyEffDate>(info_copy, Document.Current.DocDate);
			info_copy.SetCuryEffDate(currencyinfo.Cache, Document.Current.DocDate);

			adj.CustomerID = Document.Current.CustomerID;
			adj.AdjgDocDate = Document.Current.AdjDate;
			adj.AdjgCuryInfoID = Document.Current.CuryInfoID;
			adj.AdjdCuryInfoID = info_copy.CuryInfoID;
			adj.AdjdOrigCuryInfoID = invoice.CuryInfoID;
			adj.AdjdOrderDate = invoice.OrderDate > Document.Current.AdjDate 
				? Document.Current.AdjDate 
				: invoice.OrderDate;
			adj.Released = false;

			SOAdjust other = PXSelectGroupBy<SOAdjust, Where<SOAdjust.adjdOrderType, Equal<Required<SOAdjust.adjdOrderType>>, And<SOAdjust.adjdOrderNbr, Equal<Required<SOAdjust.adjdOrderNbr>>, And<Where<SOAdjust.adjgDocType, NotEqual<Required<SOAdjust.adjgDocType>>, Or<SOAdjust.adjgRefNbr, NotEqual<Required<SOAdjust.adjgRefNbr>>>>>>>, Aggregate<GroupBy<SOAdjust.adjdOrderType, GroupBy<SOAdjust.adjdOrderNbr, Sum<SOAdjust.curyAdjdAmt, Sum<SOAdjust.adjAmt>>>>>>.Select(this, adj.AdjdOrderType, adj.AdjdOrderNbr, adj.AdjgDocType, adj.AdjgRefNbr);
			if (other != null && other.AdjdOrderNbr != null)
			{
				invoice.CuryDocBal -= other.CuryAdjdAmt;
				invoice.DocBal -= other.AdjAmt;
			}

			CalcBalances<T>(adj, invoice, false);

			decimal? CuryApplAmt = adj.CuryDocBal - adj.CuryDiscBal;
			decimal? CuryApplDiscAmt = adj.CuryDiscBal;
			decimal? CuryUnappliedBal = Document.Current.CuryUnappliedBal;

			if (adj.CuryDiscBal >= 0m && adj.CuryDocBal - adj.CuryDiscBal <= 0m)
			{
				//no amount suggestion is possible
				return;
			}

			if (Document.Current != null && string.IsNullOrEmpty(Document.Current.DocDesc))
			{
				Document.Current.DocDesc = invoice.OrderDesc;
			}
			if (Document.Current != null && CuryUnappliedBal > 0m)
			{
				CuryApplAmt = Math.Min((decimal)CuryApplAmt, (decimal)CuryUnappliedBal);

				if (CuryApplAmt + CuryApplDiscAmt < adj.CuryDocBal)
				{
					CuryApplDiscAmt = 0m;
				}
			}
			else if (Document.Current != null && CuryUnappliedBal <= 0m && ((ARPayment)Document.Current).CuryOrigDocAmt > 0)
			{
				CuryApplAmt = 0m;
				CuryApplDiscAmt = 0m;
			}

			SOAdjustments.Cache.SetValue<SOAdjust.curyAdjgAmt>(adj, 0m);
			SOAdjustments.Cache.SetValue<SOAdjust.curyAdjgDiscAmt>(adj, 0m);
			SOAdjustments.Cache.SetValueExt<SOAdjust.curyAdjgAmt>(adj, CuryApplAmt);
			SOAdjustments.Cache.SetValueExt<SOAdjust.curyAdjgDiscAmt>(adj, CuryApplDiscAmt);

			CalcBalances<T>(adj, invoice, true);
		}


		protected virtual void SOAdjust_CuryDocBal_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (!internalCall)
			{
				if (e.Row != null && ((SOAdjust)e.Row).AdjdCuryInfoID != null && ((SOAdjust)e.Row).CuryDocBal == null && sender.GetStatus(e.Row) != PXEntryStatus.Deleted)
				{
					CalcBalances((SOAdjust)e.Row, false, false);
				}
				if (e.Row != null)
				{
					e.NewValue = ((SOAdjust)e.Row).CuryDocBal;
				}
			}
			e.Cancel = true;
		}

		protected virtual void SOAdjust_CuryDiscBal_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (!internalCall)
			{
				if (e.Row != null && ((SOAdjust)e.Row).AdjdCuryInfoID != null && ((SOAdjust)e.Row).CuryDiscBal == null && sender.GetStatus(e.Row) != PXEntryStatus.Deleted)
				{
					CalcBalances((SOAdjust)e.Row, false, false);
				}
				if (e.Row != null)
				{
					e.NewValue = ((SOAdjust)e.Row).CuryDiscBal;
				}
			}
			e.Cancel = true;
		}
		protected virtual void SOAdjust_CuryAdjgAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOAdjust adj = (SOAdjust)e.Row;

			if (adj.CuryDocBal == null || adj.CuryDiscBal == null)
			{
				CalcBalances((SOAdjust)e.Row, false, false);
			}

			if (adj.CuryDocBal == null)
			{
				throw new PXSetPropertyException<SOAdjust.adjdOrderNbr>(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<SOAdjust.adjdOrderNbr>(sender));
			}

			if ((decimal)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(AR.Messages.Entry_GE, ((int)0).ToString());
			}


			if ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgAmt - (decimal)e.NewValue < 0)
			{
				throw new PXSetPropertyException(AR.Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgAmt).ToString());
			}
		}

		protected virtual void SOAdjust_CuryAdjgAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CalcBalances((SOAdjust)e.Row, true, false);
		}

		protected virtual void SOAdjust_CuryAdjgDiscAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOAdjust adj = (SOAdjust)e.Row;

			if (adj.CuryDocBal == null || adj.CuryDiscBal == null)
			{
				CalcBalances((SOAdjust)e.Row, false, false);
			}

			if (adj.CuryDocBal == null || adj.CuryDiscBal == null)
			{
				throw new PXSetPropertyException<SOAdjust.adjdOrderNbr>(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<SOAdjust.adjdOrderNbr>(sender));
			}

			if ((decimal)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(AR.Messages.Entry_GE, ((int)0).ToString());
			}

			if ((decimal)adj.CuryDiscBal + (decimal)adj.CuryAdjgDiscAmt - (decimal)e.NewValue < 0)
			{
				throw new PXSetPropertyException(AR.Messages.Entry_LE, ((decimal)adj.CuryDiscBal + (decimal)adj.CuryAdjgDiscAmt).ToString());
			}

			if (adj.CuryAdjgAmt != null && (sender.GetValuePending<SOAdjust.curyAdjgAmt>(e.Row) == PXCache.NotSetValue || (Decimal?)sender.GetValuePending<SOAdjust.curyAdjgAmt>(e.Row) == adj.CuryAdjgAmt))
			{
				if ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgDiscAmt - (decimal)e.NewValue < 0)
				{
					throw new PXSetPropertyException(AR.Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgDiscAmt).ToString());
				}
			}
		}

		protected virtual void SOAdjust_CuryAdjgDiscAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CalcBalances((SOAdjust)e.Row, true, false);
		}

		#endregion

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (viewName.ToLower() == "document" && values != null)
			{
				values["CurySOApplAmt"] = PXCache.NotSetValue;
				values["CuryApplAmt"] = PXCache.NotSetValue;
				values["CuryWOAmt"] = PXCache.NotSetValue;
				values["CuryUnappliedBal"] = PXCache.NotSetValue;
			}
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		public override void LoadInvoicesProc(bool LoadExistingOnly, LoadOptions opts)
		{
			Dictionary<string, SOAdjust> existing = new Dictionary<string, SOAdjust>();

			InternalCall = true;
			try
			{
				if (Document.Current == null || Document.Current.CustomerID == null || Document.Current.OpenDoc == false || Document.Current.DocType != ARDocType.Payment)
				{
					throw new PXLoadInvoiceException();
				}

				foreach (PXResult<SOAdjust> res in SOAdjustments_Raw.Select())
				{
					SOAdjust old_adj = (SOAdjust)res;

					if (LoadExistingOnly == false)
					{
						old_adj = PXCache<SOAdjust>.CreateCopy(old_adj);
						old_adj.CuryAdjgAmt = null;
						old_adj.CuryAdjgDiscAmt = null;
					}

					string s = string.Format("{0}_{1}", old_adj.AdjdOrderType, old_adj.AdjdOrderNbr);
					existing.Add(s, old_adj);
					Adjustments.Cache.Delete((SOAdjust)res);
				}

				Document.Current.LineCntr++;
				if (Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Notchanged || Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Held)
				{
					Document.Cache.SetStatus(Document.Current, PXEntryStatus.Updated);
				}
				Document.Cache.IsDirty = true;

				foreach (KeyValuePair<string, SOAdjust> res in existing)
				{
					SOAdjust adj = new SOAdjust();
					adj.AdjdOrderType = res.Value.AdjdOrderType;
					adj.AdjdOrderNbr = res.Value.AdjdOrderNbr;

					try
					{
						adj = PXCache<SOAdjust>.CreateCopy(AddSOAdjustment(adj) ?? adj);
						if (res.Value.CuryAdjgDiscAmt != null && res.Value.CuryAdjgDiscAmt < adj.CuryAdjgDiscAmt)
						{
							adj.CuryAdjgDiscAmt = res.Value.CuryAdjgDiscAmt;
							adj = PXCache<SOAdjust>.CreateCopy((SOAdjust)this.Adjustments.Cache.Update(adj));
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

				PXSelectBase<SOOrder> cmd = new PXSelectJoin<SOOrder, 
					 LeftJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrder.orderType>>>,
				     Where<SOOrder.customerID, Equal<Optional<ARPayment.customerID>>,
					   And<SOOrder.openDoc, Equal<boolTrue>,
					   And<SOOrder.orderDate, LessEqual<Current<ARPayment.adjDate>>,
					   And<Where<SOOrderType.aRDocType, Equal<ARDocType.invoice>, Or<SOOrderType.aRDocType, Equal<ARDocType.debitMemo>>>>>>>, 
				 OrderBy<Asc<SOOrder.dueDate, 
				         Asc<SOOrder.orderNbr>>>>(this);

				if (opts != null)
				{
					if (opts.FromDate != null)
					{
						cmd.WhereAnd<Where<SOOrder.orderDate, GreaterEqual<Current<LoadOptions.fromDate>>>>();
					}
					if (opts.TillDate != null)
					{
						cmd.WhereAnd<Where<SOOrder.orderDate, LessEqual<Current<LoadOptions.tillDate>>>>();
					}					
				}

				PXResultset<SOOrder> custdocs = opts == null || opts.MaxDocs == null ? cmd.Select() : cmd.SelectWindowed(0, (int)opts.MaxDocs);

				custdocs.Sort(new Comparison<PXResult<SOOrder>>(delegate(PXResult<SOOrder> a, PXResult<SOOrder> b)
				{
					if (arsetup.Current.FinChargeFirst == true)
					{
						int aSortOrder = (((SOOrder)a).DocType == ARDocType.FinCharge ? 0 : 1);
						int bSortOrder = (((SOOrder)b).DocType == ARDocType.FinCharge ? 0 : 1);
						int ret = ((IComparable)aSortOrder).CompareTo(bSortOrder);
						if (ret != 0) return ret;
					}

					if (opts == null)
					{
						object aDueDate = ((SOOrder)a).DueDate ?? DateTime.MinValue;
						object bDueDate = ((SOOrder)b).DueDate ?? DateTime.MinValue;
						return ((IComparable)aDueDate).CompareTo(bDueDate);
					}
					else
					{
						object aObj;
						object bObj;
						int ret;
						switch (opts.OrderBy)
						{
							case LoadOptions.orderBy.RefNbr:

								aObj = ((SOOrder)a).OrderNbr;
								bObj = ((SOOrder)b).OrderNbr;
								return ((IComparable)aObj).CompareTo(bObj);

							case LoadOptions.orderBy.DocDateRefNbr:

								aObj = ((SOOrder)a).OrderDate ?? DateTime.MinValue;
								bObj = ((SOOrder)b).OrderDate ?? DateTime.MinValue;
								ret = ((IComparable)aObj).CompareTo(bObj);
								if (ret != 0) return ret;

								aObj = ((SOOrder)a).OrderNbr;
								bObj = ((SOOrder)b).OrderNbr;
								return ((IComparable)aObj).CompareTo(bObj);

							default:

								aObj = ((SOOrder)a).DueDate ?? DateTime.MinValue;
								bObj = ((SOOrder)b).DueDate ?? DateTime.MinValue;
								ret = ((IComparable)aObj).CompareTo(bObj);
								if (ret != 0) return ret;

								aObj = ((SOOrder)a).OrderNbr;
								bObj = ((SOOrder)b).OrderNbr;
								return ((IComparable)aObj).CompareTo(bObj);
						}
					}
				}));

				foreach (SOOrder invoice in custdocs)
				{
					string s = string.Format("{0}_{1}", invoice.OrderType, invoice.OrderNbr);
					if (existing.ContainsKey(s) == false)
					{
						SOAdjust adj = new SOAdjust();
						adj.AdjdOrderType = invoice.OrderType;
						adj.AdjdOrderNbr = invoice.OrderNbr;
						AddSOAdjustment(adj);						
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

		protected SOAdjust AddSOAdjustment(SOAdjust adj)
		{
			if (Document.Current.CuryUnappliedBal == 0m && Document.Current.CuryOrigDocAmt > 0m)
			{
				throw new PXLoadInvoiceException();
			}
			return this.SOAdjustments.Insert(adj);
		}

		[PXUIField(DisplayName = "View Document", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton()]
		public override IEnumerable ViewDocumentToApply(PXAdapter adapter)
		{
			SOAdjust row = SOAdjustments.Current;
			if (row != null && !(String.IsNullOrEmpty(row.AdjdOrderType) || String.IsNullOrEmpty(row.AdjdOrderNbr)))
			{
				SOOrderEntry iegraph = PXGraph.CreateInstance<SOOrderEntry>();
				iegraph.Document.Current = iegraph.Document.Search<SOOrder.orderNbr>(row.AdjdOrderNbr, row.AdjdOrderType);
				if (iegraph.Document.Current != null)
				{
					throw new PXRedirectRequiredException(iegraph, true, "View Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = "View Application Document", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton()]
		public override IEnumerable ViewApplicationDocument(PXAdapter adapter)
		{
			SOAdjust row = SOAdjustments.Current;
			if (row != null && !(String.IsNullOrEmpty(row.AdjdOrderType) || String.IsNullOrEmpty(row.AdjdOrderNbr)))
			{
				SOOrderEntry iegraph = PXGraph.CreateInstance<SOOrderEntry>();
				iegraph.Document.Current = iegraph.Document.Search<SOOrder.orderNbr>(row.AdjdOrderNbr, row.AdjdOrderType);
				if (iegraph.Document.Current != null)
				{
					throw new PXRedirectRequiredException(iegraph, true, "View Application Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}
	}
#endif
}
