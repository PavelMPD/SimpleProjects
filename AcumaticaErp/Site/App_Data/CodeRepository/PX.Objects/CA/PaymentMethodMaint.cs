
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.AR;

namespace PX.Objects.CA
{
	public class PaymentMethodMaint : PXGraph<PaymentMethodMaint, PaymentMethod>
	{
		#region Type Override events
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXSelector(typeof(Search<CCProcessingCenter.processingCenterID>))]
		[PXParent(typeof(Select<CCProcessingCenter, Where<CCProcessingCenter.processingCenterID, Equal<Current<CCProcessingCenterPmntMethod.processingCenterID>>>>))]
		[PXUIField(DisplayName = "Proc. Center ID")]
		protected virtual void CCProcessingCenterPmntMethod_ProcessingCenterID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(CA.PaymentMethod.paymentMethodID))]
		[PXSelector(typeof(Search<CA.PaymentMethod.paymentMethodID>))]
		[PXUIField(DisplayName = "Payment Method")]
		[PXParent(typeof(Select<CA.PaymentMethod, Where<CA.PaymentMethod.paymentMethodID, Equal<Current<CCProcessingCenterPmntMethod.paymentMethodID>>>>))]
		protected virtual void CCProcessingCenterPmntMethod_PaymentMethodID_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region Public Selects
		public PXSelect<PaymentMethod> PaymentMethod;
		public PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>>
			> PaymentMethodCurrent;
		public PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>>> Details;
		public PXSelect<PaymentMethodDetail,
			Where<PaymentMethodDetail.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
				And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>
			> DetailsForReceivable;
		public PXSelect<PaymentMethodDetail,
			Where<PaymentMethodDetail.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
				And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
				  Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>
			> DetailsForCashAccount;
		public PXSelect<PaymentMethodDetail,
			Where<PaymentMethodDetail.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
				And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
				  Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>
			> DetailsForVendor;
		public PXSelectJoin<PaymentMethodAccount,
			InnerJoin<CashAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
			Where<PaymentMethodAccount.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>>
			> CashAccounts;
		public PXSelect<CCProcessingCenterPmntMethod, Where<CCProcessingCenterPmntMethod.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>>
			> ProcessingCenters;

		public PXSelect<CCProcessingCenterPmntMethod, Where<CCProcessingCenterPmntMethod.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
			And<CCProcessingCenterPmntMethod.isDefault, Equal<True>>>> DefaultProcCenter;

		#endregion
		public PaymentMethodMaint()
		{
			GLSetup setup = GLSetup.Current;
		}
		public PXSetup<GLSetup> GLSetup;

		public override void Persist()
		{
			base.Persist();
		}
		#region Header Events
		protected virtual void PaymentMethod_PaymentType_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{

			bool found = false;
			foreach (PaymentMethodDetail iDet in this.Details.Select())
			{
				found = true; break;
			}
			if (found)
			{

				WebDialogResult res = this.PaymentMethod.Ask("Confirmation", "Details for the payment method will be reset. Continue?", MessageButtons.YesNo);
				if (res != WebDialogResult.Yes)
				{
					PaymentMethod row = (PaymentMethod)e.Row;
					e.Cancel = true;
					e.NewValue = row.PaymentType;
				}
			}
		}
		protected virtual void PaymentMethod_PaymentType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			PaymentMethod row = (PaymentMethod)e.Row;
			cache.SetDefaultExt<PaymentMethod.aRHasBillingInfo>(row);
		}

		protected virtual void PaymentMethod_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			PaymentMethod row = (PaymentMethod)e.Row;
			CustomerPaymentMethod cpm = PXSelect<CustomerPaymentMethod,
								Where<CustomerPaymentMethod.paymentMethodID,
								 Equal<Required<CustomerPaymentMethod.paymentMethodID>>>>.SelectWindowed(this, 0, 1, row.PaymentMethodID);
			if (cpm != null)
			{
				throw new PXException(Messages.PaymentMethodIsInUseAndCantBeDeleted);
			}
			else
			{
				isDeleting = true;
			}
		}
		protected virtual void PaymentMethod_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

			PaymentMethod row = (PaymentMethod)e.Row;
			bool allowInstances = (row.APAllowInstances == true);

			bool isCreditCard = (row.PaymentType == PaymentMethodType.CreditCard);
			bool printChecks = (row.APPrintChecks == true);
			bool createBatch = (row.APCreateBatchPayment == true);

			bool useForAP = row.UseForAP.GetValueOrDefault(false);
			bool useForAR = row.UseForAR.GetValueOrDefault(false);

			PXUIFieldAttribute.SetEnabled<PaymentMethod.aPPrintRemittance>(cache, row, printChecks);
			PXUIFieldAttribute.SetEnabled<PaymentMethod.aPRemittanceReportID>(cache, row, printChecks && (row.APPrintRemittance == true));
			PXUIFieldAttribute.SetEnabled<PaymentMethod.aPCheckReportID>(cache, row, printChecks);
			PXUIFieldAttribute.SetRequired<PaymentMethod.aPCheckReportID>(cache, printChecks);

			PXUIFieldAttribute.SetEnabled<PaymentMethod.aPPrintChecks>(cache, row, !allowInstances);
			PXUIFieldAttribute.SetEnabled<PaymentMethod.aPStubLines>(cache, row, !allowInstances && printChecks);

			PXUIFieldAttribute.SetEnabled<PaymentMethod.aPCreateBatchPayment>(cache, row, !allowInstances && !printChecks);
			PXUIFieldAttribute.SetEnabled<PaymentMethod.aPBatchExportSYMappingID>(cache, row, createBatch);
			PXUIFieldAttribute.SetRequired<PaymentMethod.aPBatchExportSYMappingID>(cache, createBatch);

			PXUIFieldAttribute.SetVisible<PaymentMethodDetail.isExpirationDate>(this.Details.Cache, null, isCreditCard);
			PXUIFieldAttribute.SetVisible<PaymentMethodDetail.isIdentifier>(this.Details.Cache, null, isCreditCard);
			PXUIFieldAttribute.SetVisible<PaymentMethodDetail.isOwnerName>(this.Details.Cache, null, isCreditCard);
			PXUIFieldAttribute.SetVisible<PaymentMethodDetail.displayMask>(this.Details.Cache, null, isCreditCard);
			PXUIFieldAttribute.SetVisible<PaymentMethodDetail.isCCProcessingID>(this.Details.Cache, null, isCreditCard);

			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.isExpirationDate>(this.Details.Cache, null, isCreditCard);
			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.isIdentifier>(this.Details.Cache, null, isCreditCard);
			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.isIdentifier>(this.Details.Cache, null, isCreditCard);
			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.displayMask>(this.Details.Cache, null, isCreditCard);
			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.isCCProcessingID>(this.Details.Cache, null, isCreditCard);
			PXUIFieldAttribute.SetVisible<PaymentMethod.aRDepositAsBatch>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<PaymentMethod.aRDepositAsBatch>(cache, null, false);

			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.useForAP>(this.CashAccounts.Cache, null, useForAP);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aPIsDefault>(this.CashAccounts.Cache, null, useForAP);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aPAutoNextNbr>(this.CashAccounts.Cache, null, useForAP);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aPLastRefNbr>(this.CashAccounts.Cache, null, useForAP);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aPBatchLastRefNbr>(this.CashAccounts.Cache, null, useForAP);

			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.useForAR>(this.CashAccounts.Cache, null, useForAR);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aRIsDefault>(this.CashAccounts.Cache, null, useForAR);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aRAutoNextNbr>(this.CashAccounts.Cache, null, useForAR);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aRLastRefNbr>(this.CashAccounts.Cache, null, useForAR);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aRIsDefaultForRefund>(this.CashAccounts.Cache, null, useForAR);

			bool showProcCenters = (row.ARIsProcessingRequired == true);
			this.ProcessingCenters.Cache.AllowDelete = showProcCenters;
			this.ProcessingCenters.Cache.AllowUpdate = showProcCenters;
			this.ProcessingCenters.Cache.AllowInsert = showProcCenters;

			PXResultset<CCProcessingCenterPmntMethod> currDefaultProcCenter = DefaultProcCenter.Select();

			if (row.ARIsProcessingRequired == true && currDefaultProcCenter.Count == 0)
			{
				PaymentMethod.Cache.RaiseExceptionHandling<PaymentMethod.aRIsProcessingRequired>(row, row.ARIsProcessingRequired,
					new PXSetPropertyException(Messages.NoProcCenterSetAsDefault, PXErrorLevel.Warning));
			}
			else
			{
				PXFieldState state = (PXFieldState)cache.GetStateExt<PaymentMethod.aRIsProcessingRequired>(row);
				if (state.IsWarning && String.Equals(state.Error, Messages.NoProcCenterSetAsDefault))
				{
					PaymentMethod.Cache.RaiseExceptionHandling<PaymentMethod.aRIsProcessingRequired>(row, null, null);
				}
			}

		}

		protected virtual void PaymentMethod_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			PaymentMethod row = (PaymentMethod)e.Row;
			PaymentMethod oldRow = (PaymentMethod)e.OldRow;
			if (oldRow.PaymentType != row.PaymentType)
			{
				foreach (PaymentMethodDetail iDet in this.Details.Select())
				{
					this.Details.Cache.Delete(iDet);
				}
				if (row.PaymentType == PaymentMethodType.CreditCard)
				{
					this.fillCreditCardDefaults();
				}
				row.ARIsOnePerCustomer = row.PaymentType == PaymentMethodType.CashOrCheck;
			}

			if ((oldRow.UseForAR != row.UseForAR) && row.UseForAR.GetValueOrDefault(false) == false)
			{
				row.ARIsProcessingRequired = false;
			}

		}
		protected virtual void PaymentMethod_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			PaymentMethod row = (PaymentMethod)e.Row;
			foreach (PaymentMethodDetail iDet in this.Details.Select())
			{
				this.Details.Cache.Delete(iDet);
			}
			if (row.PaymentType == PaymentMethodType.CreditCard)
			{
				this.fillCreditCardDefaults();
				row.ARIsOnePerCustomer = false;
			}
			row.ARIsOnePerCustomer = row.PaymentType == PaymentMethodType.CashOrCheck;
		}
		protected virtual void PaymentMethod_ARHasBillingInfo_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			PaymentMethod row = (PaymentMethod)e.Row;
			if (row.PaymentType == PaymentMethodType.CreditCard)
			{
				e.NewValue = true;
				e.Cancel = true;
			}


		}

		protected virtual void PaymentMethod_APPrintChecks_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PaymentMethod row = (PaymentMethod)e.Row;
			if ((bool)row.APPrintChecks)
			{
				row.APCreateBatchPayment = false;
				row.APCheckReportID = null;
			}
			else
			{
				sender.SetDefaultExt<PaymentMethod.aPCreateBatchPayment>(row);
			}
		}

		public override int ExecuteInsert(string viewName, IDictionary values, params object[] parameters)
		{
			switch (viewName)
			{
				case "DetailsForCashAccount":
					values[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForCashAccount;
					break;
				case "DetailsForVendor":
					values[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForVendor;
					break;
				case "DetailsForReceivable":
					values[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForARCards;
					break;
			}
			return base.ExecuteInsert(viewName, values, parameters);
		}

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			string value = (String)values[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()];
			if (string.IsNullOrEmpty(value) || value == PaymentMethodDetailUsage.UseForAll)
			{
				switch (viewName)
				{
					case "DetailsForCashAccount":
						keys[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForCashAccount;
						values[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForCashAccount;
						break;
					case "DetailsForVendor":
						keys[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForVendor;
						values[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForVendor;
						break;
					case "DetailsForReceivable":
						keys[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForARCards;
						values[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForARCards;
						break;
				}
			}
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		#endregion
		#region Detail Events
		protected virtual void PaymentMethodDetail_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			bool enableID = false;
			PaymentMethodDetail row = e.Row as PaymentMethodDetail;
			if (row == null ||(row!=null && string.IsNullOrEmpty(row.DetailID))) enableID = true;
			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.detailID>(cache, e.Row, enableID);			
			
			bool isID = (row!= null) && (row.IsIdentifier ?? false);
			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.displayMask>(cache, e.Row, isID);
		}
		protected virtual void PaymentMethodDetail_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			if (errorKey)
			{
				errorKey = false;
				e.Cancel = true;
			}
			else
			{
				PaymentMethodDetail row = (PaymentMethodDetail)e.Row;
				string detID = row.DetailID;
				string UseFor = row.UseFor;

				bool isExist = false;
				foreach (PaymentMethodDetail it in this.Details.Select())
				{
					if ((it.DetailID == detID) && (UseFor == it.UseFor))
					{
						isExist = true;
					}
				}

				if (isExist)
				{
					cache.RaiseExceptionHandling<PaymentMethodDetail.detailID>(e.Row, detID, new PXException(Messages.DuplicatedPaymentMethodDetail));
					e.Cancel = true;
				}
			}
		}
		protected virtual void PaymentMethodDetail_DetailID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			PaymentMethodDetail a = e.Row as PaymentMethodDetail;
			if (a.DetailID != null)
			{
				errorKey = true;
			}
		}
		protected virtual void PaymentMethodDetail_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			PaymentMethodDetail row = (PaymentMethodDetail)e.Row;
			PaymentMethodDetail oldRow = (PaymentMethodDetail)e.OldRow;
			if (row.IsIdentifier != oldRow.IsIdentifier || row.IsExpirationDate != oldRow.IsExpirationDate || row.IsOwnerName != oldRow.IsOwnerName)
			{
				bool needRefresh;
				ResetUniqueFlags(row, out needRefresh);
				if (needRefresh)
					this.Details.View.RequestRefresh();
			}
		}
		protected virtual void PaymentMethodDetail_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			PaymentMethodDetail row = (PaymentMethodDetail)e.Row;
			if ((bool)row.IsIdentifier || (bool)row.IsExpirationDate || (bool)row.IsOwnerName)
			{
				bool needRefresh;
				ResetUniqueFlags(row, out needRefresh);
				if (needRefresh)
					this.Details.View.RequestRefresh();
			}

		}
		#endregion

		#region Account Events

		protected virtual void PaymentMethodAccount_PaymentMethodID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
		protected virtual void PaymentMethodAccount_CashAccountID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
			cache.SetDefaultExt<PaymentMethodAccount.useForAP>(row);
		}

		protected virtual void PaymentMethodAccount_UseForAP_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
			CA.PaymentMethod pm = this.PaymentMethod.Current;
			if (row != null && pm != null)
			{
				e.NewValue = (pm.UseForAP == true);
				if (pm.UseForAP == true && row.CashAccountID.HasValue)
				{
					CashAccount c = PXSelectReadonly<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, row.CashAccountID);					
					e.NewValue = (c != null);
				}
				e.Cancel = true;

			}
		}

		protected virtual void PaymentMethodAccount_UseForAR_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
			CA.PaymentMethod pm = this.PaymentMethod.Current;
			e.NewValue = (pm != null) && pm.UseForAR == true;
			e.Cancel = true;
		}

		protected virtual void PaymentMethodAccount_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
			if (row != null)
			{
				if (String.IsNullOrEmpty(row.PaymentMethodID) == false)
				{
					PaymentMethod pt = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, row.PaymentMethodID);
					bool enabled = (pt != null) && pt.APCreateBatchPayment == true;
					PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPBatchLastRefNbr>(cache, row, enabled);
				}

				if (row.CashAccountID.HasValue)
				{
					CashAccount c = PXSelectReadonly<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, row.CashAccountID);
					PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.useForAP>(cache, row, (c != null));
				}
				bool EnableAP = row.UseForAP.Value;
				bool EnableAR = row.UseForAR.Value;
				PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPIsDefault>(cache, e.Row, EnableAP);
				PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPAutoNextNbr>(cache, e.Row, EnableAP);
				PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPLastRefNbr>(cache, e.Row, EnableAP);
				PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aRIsDefault>(cache, e.Row, EnableAR);
				PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aRIsDefaultForRefund>(cache, e.Row, EnableAR);
				PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aRAutoNextNbr>(cache, e.Row, EnableAR);
				PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aRLastRefNbr>(cache, e.Row, EnableAR);
			}
		}
		protected virtual void PaymentMethodAccount_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.NewRow;
			if (row != null)
			{
				PaymentMethodAccount oldrow = (PaymentMethodAccount)e.Row;
				if ((row.UseForAP != oldrow.UseForAP) && !row.UseForAP.GetValueOrDefault(false))
				{
					row.APIsDefault = false;
				}
				if ((row.UseForAR != oldrow.UseForAR) && !row.UseForAR.GetValueOrDefault(false))
				{
					row.ARIsDefault = false;
				}
			}

		}
		protected virtual void PaymentMethodAccount_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
			bool needRefresh = false;
			if (row != null)
			{
				PaymentMethodAccount oldrow = (PaymentMethodAccount)e.OldRow;
				if ((row.ARIsDefault != oldrow.ARIsDefault) && row.ARIsDefault.GetValueOrDefault(false))
				{
					CashAccount rowc = PXSelectReadonly<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, row.CashAccountID);
					foreach (PXResult<PaymentMethodAccount, CashAccount> rr in this.CashAccounts.Select())
					{
						PaymentMethodAccount it = rr;
						CashAccount ic = rr;

						if (row.ARIsDefault == true && rowc.BranchID == ic.BranchID
								&& !(row.PaymentMethodID == it.PaymentMethodID
								&& row.CashAccountID == it.CashAccountID))
						{
							it.ARIsDefault = false;
							this.CashAccounts.Update(it);
						}
					}
					needRefresh = true;
				}
				if ((row.ARIsDefaultForRefund != oldrow.ARIsDefaultForRefund) && row.ARIsDefaultForRefund.GetValueOrDefault(false))
				{
					CashAccount rowc = PXSelectReadonly<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, row.CashAccountID);
					foreach (PXResult<PaymentMethodAccount, CashAccount> rr in this.CashAccounts.Select())
					{
						PaymentMethodAccount it = rr;
						CashAccount ic = rr;

						if (row.ARIsDefaultForRefund == true && rowc.BranchID == ic.BranchID
								&& !(row.PaymentMethodID == it.PaymentMethodID
								&& row.CashAccountID == it.CashAccountID))
						{
							it.ARIsDefaultForRefund = false;
							this.CashAccounts.Update(it);
						}
					}
					needRefresh = true;
				}

				if ((row.APIsDefault != oldrow.APIsDefault) && row.APIsDefault.GetValueOrDefault(false))
				{
					CashAccount rowc = PXSelectReadonly<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, row.CashAccountID);
					foreach (PXResult<PaymentMethodAccount, CashAccount> rr in this.CashAccounts.Select())
					{
						PaymentMethodAccount it = rr;
						CashAccount ic = rr;
						if (it.APIsDefault == true
								&& rowc.BranchID == ic.BranchID
								&& !(row.PaymentMethodID == it.PaymentMethodID
								&& row.CashAccountID == it.CashAccountID))
						{
							it.APIsDefault = false;
							this.CashAccounts.Update(it);
						}
					}
					needRefresh = true;
				}
				if (needRefresh)
				{
					this.CashAccounts.View.RequestRefresh();
				}
			}
		}

		protected virtual void PaymentMethodAccount_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
			PXEntryStatus status = cache.GetStatus(e.Row);
			if (!isDeleting && row.CashAccountID.HasValue && (status != PXEntryStatus.Inserted && status != PXEntryStatus.InsertedDeleted))
			{

				CustomerPaymentMethod cpm = PXSelect<CustomerPaymentMethod, Where<CustomerPaymentMethod.paymentMethodID, Equal<Required<CustomerPaymentMethod.paymentMethodID>>,
														And<CustomerPaymentMethod.cashAccountID, Equal<Required<CustomerPaymentMethod.cashAccountID>>>>>.SelectWindowed(this, 0, 1, row.PaymentMethodID, row.CashAccountID);
				if (cpm != null)
				{
					throw new PXException(Messages.PaymentMethodAccountIsInUseAndCantBeDeleted);
				}
			}
		}

		protected virtual void PaymentMethodAccount_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
			if (row == null) return;
			if (String.IsNullOrEmpty(row.PaymentMethodID) == false && row.CashAccountID.HasValue)
			{
				foreach (PXResult<PaymentMethodAccount, CashAccount> iRes in this.CashAccounts.Select())
				{
					PaymentMethodAccount it = iRes;
					if (!object.ReferenceEquals(row, it) && it.PaymentMethodID == row.PaymentMethodID && row.CashAccountID == it.CashAccountID)
					{
						CashAccount acct = iRes;
						throw new PXSetPropertyException(Messages.DuplicatedCashAccountForPaymentMethod, acct.CashAccountCD);
					}
				}
			}
		}
		#endregion
		
		#region ProcessingCenter Events
		protected virtual void CCProcessingCenterPmntMethod_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			if (errorKey)
			{
				errorKey = false;
				e.Cancel = true;
			}
			else
			{
				CCProcessingCenterPmntMethod row = e.Row as CCProcessingCenterPmntMethod;
				string detID = row.ProcessingCenterID;
				bool isExist = false;
				foreach (CCProcessingCenterPmntMethod it in this.ProcessingCenters.Select())
				{
					if (!Object.ReferenceEquals(it, row) && it.ProcessingCenterID == row.ProcessingCenterID)
					{
						isExist = true;
					}
				}

				if (isExist)
				{
					cache.RaiseExceptionHandling<CCProcessingCenterPmntMethod.processingCenterID>(e.Row, detID, new PXException(Messages.ProcessingCenterIsAlreadyAssignedToTheCard));
					e.Cancel = true;
				}
			}
		}
		protected virtual void CCProcessingCenterPmntMethod_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			CCProcessingCenterPmntMethod row = (CCProcessingCenterPmntMethod)e.Row;
			CCProcessingCenterPmntMethod oldRow = (CCProcessingCenterPmntMethod)e.OldRow;

			if (row.IsDefault != oldRow.IsDefault)
			{
				CheckDefaultFlag(cache, row);
			}
		}

		protected virtual void CCProcessingCenterPmntMethod_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			CCProcessingCenterPmntMethod row = (CCProcessingCenterPmntMethod)e.Row;

			if (row.IsDefault == true)
			{
				CheckDefaultFlag(cache, row);
			}
		}

		protected virtual void CCProcessingCenterPmntMethod_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			CCProcessingCenterPmntMethod row = (CCProcessingCenterPmntMethod)e.Row;
			if (row.IsDefault == true)
			{
				CheckDefaultFlag(cache, null);
			}
		}


		private void CheckDefaultFlag(PXCache cache, CCProcessingCenterPmntMethod row)
		{
			PXResultset<CCProcessingCenterPmntMethod> currDefaultProcCenter = DefaultProcCenter.Select();

			if (row != null && row.IsDefault == true)
			{
				foreach (CCProcessingCenterPmntMethod procPmtMethod in currDefaultProcCenter)
				{
					if (Object.ReferenceEquals(procPmtMethod, row)) continue;
					ProcessingCenters.SetValueExt<CCProcessingCenterPmntMethod.isDefault>(procPmtMethod, false);
				}
				ProcessingCenters.View.RequestRefresh();
			}
		}

		protected virtual void CCProcessingCenterPmntMethod_ProcessingCenterID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			CCProcessingCenterPmntMethod row = e.Row as CCProcessingCenterPmntMethod;
			if (row.ProcessingCenterID != null)
			{
				errorKey = true;
			}
		}
		protected virtual void CCProcessingCenterPmntMethod_IsDefault_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			if (this.ProcessingCenters.Select().Count == 0)
			{
				e.NewValue = true;
			}
		}

		#endregion

		#region Internal Auxillary Functions
		protected virtual void ResetUniqueFlags(PaymentMethodDetail aRow, out bool needRefresh)
		{
			needRefresh = false;
			const int IDFlag = 0x1;
			const int ExpDateFlag = 0x2;
			const int NameOnCardFlag = 0x4;
			int state = 0;
			state |= (bool)aRow.IsIdentifier ? IDFlag : 0;
			state |= (bool)aRow.IsExpirationDate ? ExpDateFlag : 0;
			state |= (bool)aRow.IsOwnerName ? NameOnCardFlag : 0;
			if (state != 0)
			{
				foreach (PaymentMethodDetail iDet in this.Details.Select())
				{
					if (Object.ReferenceEquals(aRow, iDet)) continue;
					if (aRow.UseFor != iDet.UseFor) continue;
					bool needUpdate = false;
					if ((bool)iDet.IsIdentifier && ((state & IDFlag) != 0))
					{
						iDet.IsIdentifier = false;
						needUpdate = true;
					}
					if ((state & ExpDateFlag) != 0 && (bool)iDet.IsExpirationDate)
					{
						iDet.IsExpirationDate = false;
						needUpdate = true;
					}
					if ((state & NameOnCardFlag) != 0 && (bool)iDet.IsOwnerName)
					{
						iDet.IsOwnerName = false;
						needUpdate = true;
					}
					if (needUpdate)
					{
						this.Details.Update(iDet);
						needRefresh = true;
					}
				}
			}
		}



		protected virtual void fillCreditCardDefaults()
		{
			this.addDefaultsToDetails(CreditCardAttributes.AttributeName.CCPID, Messages.CCPID);
			PaymentMethodDetail det = this.addDefaultsToDetails(CreditCardAttributes.AttributeName.CardNumber, Messages.CardNumber);
			det.DisplayMask = CreditCardAttributes.MaskDefaults.DefaultIdentifier;
			this.addDefaultsToDetails(CreditCardAttributes.AttributeName.ExpirationDate, Messages.ExpirationDate);
			this.addDefaultsToDetails(CreditCardAttributes.AttributeName.NameOnCard, Messages.NameOnCard);
			this.addDefaultsToDetails(CreditCardAttributes.AttributeName.CCVCode, Messages.CCVCode);
		}

		private PaymentMethodDetail addDefaultsToDetails(CreditCardAttributes.AttributeName aAttr, string aDescr)
		{
			PaymentMethodDetail det = new PaymentMethodDetail();
			ImportDefaults(det, aAttr);
			det.Descr = aDescr;
			det.UseFor = PaymentMethodDetailUsage.UseForARCards;
			det = (PaymentMethodDetail)this.Details.Cache.Insert(det);
			return det;
		}

		private static void ImportDefaults(PaymentMethodDetail aData, CreditCardAttributes.AttributeName aAttr)
		{
			aData.DetailID = CreditCardAttributes.GetID(aAttr);
			aData.EntryMask = CreditCardAttributes.GetMask(aAttr);
			aData.ValidRegexp = CreditCardAttributes.GetValidationRegexp(aAttr);
			aData.IsIdentifier = aAttr == CreditCardAttributes.AttributeName.CardNumber;
			aData.IsExpirationDate = aAttr == CreditCardAttributes.AttributeName.ExpirationDate;
			aData.IsOwnerName = (aAttr == CreditCardAttributes.AttributeName.NameOnCard);
			aData.IsRequired = (aAttr == CreditCardAttributes.AttributeName.CCPID);
			aData.IsEncrypted = (aAttr == CreditCardAttributes.AttributeName.ExpirationDate) || (aAttr == CreditCardAttributes.AttributeName.CardNumber) || (aAttr == CreditCardAttributes.AttributeName.CCVCode);
			aData.IsCCProcessingID = (aAttr == CreditCardAttributes.AttributeName.CCPID);
			aData.OrderIndex = (short)((int)aAttr + 1);
		}
		#endregion
		#region Private members
		private bool errorKey;
		#endregion
		private bool isDeleting = false;
	}

	public static class CreditCardAttributes
	{
		public enum AttributeName
		{
			CardNumber = 0,
			ExpirationDate,
			NameOnCard,
			CCVCode,
			CCPID
		}

		public static string GetID(AttributeName aID)
		{
			return IDS[(int)aID];
		}

		public static string GetMask(AttributeName aID)
		{
			return EntryMasks[(int)aID];
		}

		public static string GetValidationRegexp(AttributeName aID)
		{
			return ValidationRegexps[(int)aID];
		}

		public const string CardNumber = "CCDNUM";
		public const string ExpirationDate = "EXPDATE";
		public const string NameOnCard = "NAMEONCC";
		public const string CVV = "CVV";
		public const string CCPID = "CCPID";

		public static class MaskDefaults
		{
			public const string CardNumber = "0000-0000-0000-0000";
			public const string ExpirationDate = "00/0000";
			public const string DefaultIdentifier = "****-****-****-0000";
			public const string CVV = "000";
			public const string CCPID = "";
		}

		public static class ValidationRegexp
		{
			public const string CardNumber = "";
			public const string ExpirationDate = "";
			public const string DefaultIdentifier = "";
			public const string CVV = "";
			public const string CCPID = "";
		}

		#region Private Members
		private static string[] IDS = { CardNumber, ExpirationDate, NameOnCard, CVV, CCPID };
		private static string[] EntryMasks = { MaskDefaults.CardNumber, MaskDefaults.ExpirationDate, String.Empty, MaskDefaults.CVV, MaskDefaults.CCPID };
		private static string[] ValidationRegexps = { ValidationRegexp.CardNumber, ValidationRegexp.ExpirationDate, String.Empty, ValidationRegexp.CVV, ValidationRegexp.CCPID };

		#endregion
	}

}


