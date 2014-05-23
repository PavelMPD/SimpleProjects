using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.SO;

namespace PX.Objects.CT
{
    public class CTBillEngine :PXGraph<CTBillEngine>
    {
        public PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>> Contracts;
        public PXSelect<ContractBillingSchedule, Where<ContractBillingSchedule.contractID, Equal<Optional<Contract.contractID>>>> BillingSchedule;
        public PXSelect<ContractDetail, Where<ContractDetail.contractID, Equal<Required<Contract.contractID>>>> ContractDetails;
        public PXSelect<PMTran> Transactions;
        public PXSelect<ContractRenewalHistory> RenewalHistory;
		public PXSelect<ContractRenewalHistory, Where<ContractRenewalHistory.contractID, Equal<Required<Contract.contractID>>, 
			And<ContractRenewalHistory.revID, Equal<Required<Contract.revID>>>>> CurrentRenewalHistory;
        public PXSelect<ContractBillingTrace,  Where<ContractBillingTrace.contractID, Equal<Required<ContractBillingTrace.contractID>>>, OrderBy<Desc<ContractBillingTrace.recordID>>> BillingTrace;
		public PXSetupOptional<INSetup> insetup;
		public PXSelect<ContractItem> contractItem;

		public PXSetup<Location, Where<Location.bAccountID, Equal<Current<ContractBillingSchedule.accountID>>, And<Location.locationID, Equal<Current<ContractBillingSchedule.locationID>>>>> BillingLocation;

        protected List<ARRegister> doclist = new List<ARRegister>();
        protected Dictionary<int?, decimal?> availableQty = new Dictionary<int?, decimal?>();
		protected Dictionary<int?, decimal?> availableDeposit = new Dictionary<int?, decimal?>();
		protected Dictionary<int?, UsageData> depositUsage = new Dictionary<int?, UsageData>();
        protected List<UsageData> nonRefundableDepositedUsage = new List<UsageData>();
        protected Dictionary<int?, ContractItem> nonRefundableDeposits = new Dictionary<int?, ContractItem>();
        protected Dictionary<int?, ContractItem> refundableDeposits = new Dictionary<int?, ContractItem>();

		[PXBool()]
		[PXDBScalar(typeof(Search<ContractTemplate.detailedBilling, Where<ContractTemplate.contractID, Equal<Contract.templateID>>>))]
		protected virtual void Contract_DetailedBilling_CacheAttached(PXCache sender)
		{ 
		}

		[PXDBInt(IsKey = true)]
		protected virtual void ContractDetail_ContractID_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt(IsKey = true)]
		[PXDimensionSelector(ContractItemAttribute.DimensionName, typeof(Search<ContractItem.contractItemID>),
																	typeof(ContractItem.contractItemCD),
																	typeof(ContractItem.contractItemCD), typeof(ContractItem.descr))]
		protected virtual void ContractDetail_ContractItemID_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		protected virtual void ContractDetail_RevID_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		protected virtual void ContractDetail_LineNbr_CacheAttached(PXCache sender)
		{
		}

		public ContractBillingSchedule contractBillingSchedule
		{
			get
			{
				return this.BillingSchedule.Select();
			}
		}

		[PXDBInt()]
		[PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search2<InventoryItem.inventoryID,
			LeftJoin<ARSalesPrice, On<ARSalesPrice.inventoryID, Equal<InventoryItem.inventoryID>, And<ARSalesPrice.uOM, Equal<InventoryItem.baseUnit>, And<ARSalesPrice.custPriceClassID, Equal<ARPriceClass.emptyPriceClass>, And<ARSalesPrice.curyID, Equal<Current<ContractItem.curyID>>>>>>,
			LeftJoin<ARSalesPrice2, On<ARSalesPrice2.inventoryID, Equal<InventoryItem.inventoryID>, And<ARSalesPrice2.uOM, Equal<InventoryItem.baseUnit>, And<ARSalesPrice2.custPriceClassID, Equal<Current<Location.cPriceClassID>>, And<ARSalesPrice2.curyID, Equal<Current<ContractItem.curyID>>>>>>>>,
			Where<InventoryItem.stkItem, Equal<False>>>), typeof(InventoryItem.inventoryCD))]
		[PXUIField(DisplayName = "Setup Item")]
		public void ContractItem_BaseItemID_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		[PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search2<InventoryItem.inventoryID,
			LeftJoin<ARSalesPrice, On<ARSalesPrice.inventoryID, Equal<InventoryItem.inventoryID>, And<ARSalesPrice.uOM, Equal<InventoryItem.baseUnit>, And<ARSalesPrice.custPriceClassID, Equal<ARPriceClass.emptyPriceClass>, And<ARSalesPrice.curyID, Equal<Current<ContractItem.curyID>>>>>>,
			LeftJoin<ARSalesPrice2, On<ARSalesPrice2.inventoryID, Equal<InventoryItem.inventoryID>, And<ARSalesPrice2.uOM, Equal<InventoryItem.baseUnit>, And<ARSalesPrice2.custPriceClassID, Equal<Current<Location.cPriceClassID>>, And<ARSalesPrice2.curyID, Equal<Current<ContractItem.curyID>>>>>>>>,
			Where<InventoryItem.stkItem, Equal<False>>>), typeof(InventoryItem.inventoryCD))]
		[PXUIField(DisplayName = "Renewal Item")]
		public void ContractItem_RenewalItemID_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		[PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search2<InventoryItem.inventoryID,
			LeftJoin<ARSalesPrice, On<ARSalesPrice.inventoryID, Equal<InventoryItem.inventoryID>, And<ARSalesPrice.uOM, Equal<InventoryItem.baseUnit>, And<ARSalesPrice.custPriceClassID, Equal<ARPriceClass.emptyPriceClass>, And<ARSalesPrice.curyID, Equal<Current<ContractItem.curyID>>>>>>,
			LeftJoin<ARSalesPrice2, On<ARSalesPrice2.inventoryID, Equal<InventoryItem.inventoryID>, And<ARSalesPrice2.uOM, Equal<InventoryItem.baseUnit>, And<ARSalesPrice2.custPriceClassID, Equal<Current<Location.cPriceClassID>>, And<ARSalesPrice2.curyID, Equal<Current<ContractItem.curyID>>>>>>>>,
			Where<InventoryItem.stkItem, Equal<False>>>), typeof(InventoryItem.inventoryCD))]
		[PXUIField(DisplayName = "Recurring Item")]
		public void ContractItem_RecurringItemID_CacheAttached(PXCache sender)
		{
		}

		public CTBillEngine()
			:base()
		{
			this.AutomationView = "Contracts";
		}

        protected int DecPlPrcCst
        {
            get
            {
                int decimals = 2;
                INSetup insetup = PXSelect<INSetup>.Select(this);
                if (insetup != null && insetup.DecPlPrcCst != null)
                {
                    decimals = Convert.ToInt32(insetup.DecPlPrcCst);
                }

                return decimals;
            }
        }

		private void CreateNewRevision(int? contractID, string action, string newStatus)
		{
			Contract contract = (Contract)Contracts.Cache.CreateCopy(Contracts.SelectSingle(contractID));

			foreach (ContractDetail detail in ContractDetails.Select(contract.ContractID))
			{
				ContractDetail newdet = ContractDetails.Cache.CreateCopy(detail) as ContractDetail;
				ContractDetails.Cache.Remove(newdet);
				newdet.RevID += 1;
				if (action == ContractAction.Setup || action == ContractAction.SetupAndActivate || action == ContractAction.Activate)
				{
					ContractMaint.ValidateUniqueness(this, newdet, true);
				}
				ContractDetails.Insert(newdet);
				if (action == ContractAction.Upgrade)
				{
					ContractMaint.CalculateDiscounts(ContractDetails.Cache, contract, newdet);
					ContractDetails.Update(newdet);
				}
			}
			ContractRenewalHistory history = CurrentRenewalHistory.SelectSingle(contract.ContractID, contract.RevID);
			ContractRenewalHistory newHistory = CurrentRenewalHistory.Cache.CreateCopy(history) as ContractRenewalHistory;
			newHistory.Status = newStatus;
			newHistory.Action = action;
			newHistory.RevID += 1;
            newHistory.RenewedBy = PXAccess.GetUserID();

			CurrentRenewalHistory.Insert(newHistory);

			if (history.Status == ContractStatus.Activated)
			{
				contract.LastActiveRevID = history.RevID;
			}
			if (newHistory.Status == ContractStatus.Activated)
			{
				contract.LastActiveRevID = newHistory.RevID;
			}

			contract.RevID = newHistory.RevID;

            contract.IsLastActionUndoable = true;
			Contracts.Update(contract);
		}

        public static void UpdateContractHistoryEntry(ContractRenewalHistory history, Contract contract, ContractBillingSchedule schedule)
        {
            history.IsActive = contract.IsActive;
            history.IsCancelled = contract.IsCancelled;
            history.IsCompleted = contract.IsCompleted;
            history.IsPendingUpdate = contract.IsPendingUpdate;

            history.ExpireDate = contract.ExpireDate;
            history.EffectiveFrom = contract.EffectiveFrom;
            history.ActivationDate = contract.ActivationDate;
            history.StartDate = contract.StartDate;

            history.DiscountID = contract.DiscountID;

            history.LastDate = schedule.LastDate;
            history.NextDate = schedule.NextDate;
            history.StartBilling = schedule.StartBilling;
			if (contract.Status != ContractStatus.Draft)
				history.ChildContractID = null;
        }

        private void UpdateHistory(Contract contract)
        {
            var updated = Contracts.SelectSingle(contract.ContractID);
            var schedule = BillingSchedule.SelectSingle(contract.ContractID);
            var history = CurrentRenewalHistory.SelectSingle(updated.ContractID, updated.RevID);

            UpdateContractHistoryEntry(history, updated, schedule);

            CurrentRenewalHistory.Update(history);
        }

        private void ClearBillingTrace(int? contractID)
        {
            PXDatabase.Delete<ContractBillingTrace>(new PXDataFieldRestrict("ContractID",PXDbType.Int,contractID));
        }

        private void ClearFuture(Contract contract)
        {
            if (contract == null || contract.ContractID == null || contract.RevID == null)
                return;

            ClearFutureDetails(contract);
            ClearFutureHistory(contract);
        }

        private void ClearFutureHistory(Contract contract)
        {
            PXDatabase.Delete<ContractRenewalHistory>(
                new PXDataFieldRestrict("ContractID", PXDbType.Int, 4, contract.ContractID, PXComp.EQ),
                new PXDataFieldRestrict("RevID", PXDbType.Int, 4, contract.RevID, PXComp.GT));
        }

        private void ClearFutureDetails(Contract contract)
        {
            PXDatabase.Delete<ContractDetail>(
                new PXDataFieldRestrict("ContractID", PXDbType.Int, 4, contract.ContractID, PXComp.EQ),
                new PXDataFieldRestrict("RevID", PXDbType.Int, 4, contract.RevID, PXComp.GT));
        }

		private void ClearState()
		{
			availableQty.Clear();
			availableDeposit.Clear();
			depositUsage.Clear();
			doclist.Clear();
            nonRefundableDepositedUsage.Clear();
            nonRefundableDeposits.Clear();
            refundableDeposits.Clear();
        }
        
		public virtual void Setup(int? contractID, DateTime? date)
		{
			#region Parameter and State Verification

			if (contractID == null)
				throw new ArgumentNullException("contractID");

			if (date == null)
				throw new ArgumentNullException("date");

			Contract contract = Contracts.Select(contractID);
			ContractBillingSchedule schedule = (ContractBillingSchedule)BillingSchedule.Select(contractID);

			if (contract.IsCompleted == true)
				throw new PXException("Contract is Completed/Expired and cannot be Setted Up.");

			if (contract.Status!= ContractStatus.Draft)
				throw new PXException("Contract is already Setup.");

			if (contract.IsCancelled == true)
				throw new PXException("Contract is Cancelled/Terminated and cannot be Setted Up.");

			if (contract.ExpireDate != null && date.Value > contract.ExpireDate.Value)
			{
				throw new PXException("Activation date of a contract cannot be greater then the Expire Date of a Contract");
			}

			if (contract.IsPendingUpdate == true)
			{
				DateTime minDate = schedule.LastDate ?? contract.StartDate.Value;

				if (date < minDate || date > schedule.NextDate.Value)
				{
					throw new PXException("Update can only be activated if the effective date is between the Last Billing Date and Next Billing Date");
				}
			}
			foreach (ContractItem item in PXSelectJoin<ContractItem, InnerJoin<ContractDetail, On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>>, Where<ContractDetail.contractID, Equal<Required<Contract.contractID>>>>.Select(this, contract.ContractID))
			{
				string message;
				if (!ContractItemMaint.IsValidItemPrice(this, item, out message))
				{
					throw new PXException(Messages.SpecificItemNotSpecificPrice, item.ContractItemCD, message);
				}
			}

			#endregion

			ClearState();
            ClearBillingTrace(contractID);

			CreateNewRevision(contractID, ContractAction.Setup, ContractStatus.PendingActivation);

			Contract template = Contracts.SelectSingle(contract.TemplateID);
			contract.ScheduleStartsOn = template.ScheduleStartsOn;

			List<InvoiceData> invoices = new List<InvoiceData>();
			InvoiceData data = new InvoiceData(date.Value);
			data.UsageData.AddRange(GetSetupFee(contract));
			if (contract.ScheduleStartsOn == ScheduleStartOption.SetupDate)
			{
				data.UsageData.AddRange(GetActivationFee(contract, date));
			}
			if (data.UsageData.Count > 0)
				invoices.Add(data);

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				if (invoices.Count > 0)
				{
					Customer customer;
					Location location;
					SetBillingTarget(contract, out customer, out location);

					CreateInvoice(contract, template, invoices, customer, location, ContractAction.Setup);
				}

				if (contract.ScheduleStartsOn == ScheduleStartOption.SetupDate)
				{
					schedule.LastDate = date.Value;
				}

				foreach (ARInvoice invoice in doclist)
				{
					ContractBillingTrace trace = new ContractBillingTrace();
					trace.ContractID = contractID;
					trace.DocType = invoice.DocType;
					trace.RefNbr = invoice.RefNbr;
					trace.LastDate = schedule.LastDate;
					trace.NextDate = schedule.NextDate;

					BillingTrace.Insert(trace);
				}

				if (contract.ScheduleStartsOn == ScheduleStartOption.SetupDate)
				{
					schedule.StartBilling = date.Value;
					if (schedule.Type != BillingType.OnDemand)
						schedule.NextDate = GetNextBillingDate(schedule.Type, contract.CustomerID, date, date);
				}
				contract.EffectiveFrom = contract.StartDate;
				BillingSchedule.Update(schedule);
				contract.Status = ContractStatus.PendingActivation;
				Contracts.Update(contract);

                UpdateHistory(contract);

				this.Actions.PressSave();

				ts.Complete();
			}
			AutoReleaseInvoice(contract);
		}

		public virtual void Activate(int? contractID, DateTime? date)
		{
			#region Parameter and State Verification

			if (contractID == null)
				throw new ArgumentNullException("contractID");

			if (date == null)
				throw new ArgumentNullException("date");

			Contract contract = Contracts.Select(contractID);
			ContractBillingSchedule schedule = (ContractBillingSchedule) BillingSchedule.Select(contractID);

			if (contract.IsCompleted == true)
				throw new PXException("Contract is Completed/Expired and cannot be Activated.");

            if (contract.IsActive == true && contract.IsPendingUpdate != true)
				throw new PXException("Contract is already Active.");

			if (contract.IsCancelled == true)
				throw new PXException("Contract is Cancelled/Terminated and cannot be Activated.");

			if (date.Value < contract.StartDate.Value)
			{
				throw new PXException(Messages.ActivationDateError);
			}

			if (contract.ExpireDate != null && date.Value > contract.ExpireDate.Value)
			{
				throw new PXException("Activation date of a contract cannot be greater then the Expire Date of a Contract");
			}

			if (contract.IsPendingUpdate == true)
			{
				DateTime minDate = schedule.LastDate ?? contract.StartDate.Value;

				if (date < minDate ||  (schedule.Type!=BillingType.OnDemand && date > schedule.NextDate.Value))
				{
					throw new PXException("Update can only be activated if the effective date is between the Last Billing Date and Next Billing Date");
				}
			}
			foreach (ContractItem item in PXSelectJoin<ContractItem, InnerJoin<ContractDetail, On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>>, Where<ContractDetail.contractID, Equal<Required<Contract.contractID>>>>.Select(this, contract.ContractID))
			{
				string message;
				if (!ContractItemMaint.IsValidItemPrice(this, item, out message))
				{
					throw new PXException(Messages.SpecificItemNotSpecificPrice, item.ContractItemCD, message);
				}
			}

			#endregion

			ClearState();
            ClearBillingTrace(contractID);

			CreateNewRevision(contractID, ContractAction.Activate, ContractStatus.Activated);

			Contract template = Contracts.SelectSingle(contract.TemplateID);

			List<InvoiceData> invoices = new List<InvoiceData>();

			InvoiceData data = new InvoiceData(date.Value);
			if (contract.IsPendingUpdate != true)
			{
				if (contract.ScheduleStartsOn == ScheduleStartOption.SetupDate)
				{
					if (schedule.Type != BillingType.OnDemand)
					{
						while (schedule.NextDate < date)
						{
							schedule.LastDate = schedule.NextDate;
							schedule.NextDate = GetNextBillingDate(schedule.Type, contract.CustomerID, schedule.LastDate, schedule.StartBilling);
						}
					}
					else
					{
						schedule.LastDate = date;
						schedule.NextDate = null;
					}
				}

				if (contract.ScheduleStartsOn == ScheduleStartOption.ActivationDate)
				{
					data.UsageData.AddRange(GetActivationFee(contract, date));
				}
				data.UsageData.AddRange(GetPrepayment(contract, date, schedule.LastDate, schedule.NextDate));
			}
			else
			{
				data.UsageData.AddRange(GetUpgradeFee(contract, schedule.LastDate, schedule.NextDate, date));
			}

			if (data.UsageData.Count > 0)
				invoices.Add(data);
			

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				if (invoices.Count > 0)
				{

					Customer customer;
					Location location;
					SetBillingTarget(contract, out customer, out location);

					CreateInvoice(contract, template, invoices, customer, location, ContractAction.Activate);
				}

				foreach (ARInvoice invoice in doclist)
				{
					ContractBillingTrace trace = new ContractBillingTrace();
					trace.ContractID = contractID;
					trace.DocType = invoice.DocType;
					trace.RefNbr = invoice.RefNbr;
					trace.LastDate = schedule.LastDate;
					trace.NextDate = schedule.NextDate;

					BillingTrace.Insert(trace);
				}

				if (contract.IsPendingUpdate != true && contract.ScheduleStartsOn == ScheduleStartOption.ActivationDate)
				{
					if (schedule.Type != BillingType.OnDemand)
						schedule.NextDate = GetNextBillingDate(schedule.Type, contract.CustomerID, date, date);
                    schedule.LastDate = date.Value;
				}

				if (contract.IsPendingUpdate != true)
				{
					contract.EffectiveFrom = contract.StartDate;
					if (contract.ScheduleStartsOn == ScheduleStartOption.ActivationDate)
						schedule.StartBilling = date;
				}

				BillingSchedule.Update(schedule);

				contract.Status = ContractStatus.Activated;
                UpdateStatusFlags(contract);
				contract.ServiceActivate = true;
				Contracts.Update(contract);

                UpdateHistory(contract);

				this.Actions.PressSave();

				ts.Complete();
			}//ts

			AutoReleaseInvoice(contract);
		}
        
        public virtual void SetupAndActivate(int? contractID, DateTime? date)
        {
            #region Parameter and State Verification
            
            if (contractID == null)
                throw new ArgumentNullException("contractID");

            if (date == null)
                throw new ArgumentNullException("date");

            Contract contract = Contracts.Select(contractID);
            ContractBillingSchedule schedule = (ContractBillingSchedule) BillingSchedule.Select(contractID);

            if (contract.IsCompleted == true)
                throw new PXException("Contract is Completed/Expired and cannot be Activated.");

            if (contract.IsActive == true && contract.IsPendingUpdate != true)
                throw new PXException("Contract is already Active.");

            if (contract.IsCancelled == true)
                throw new PXException("Contract is Cancelled/Terminated and cannot be Activated.");
            
            if (date.Value < contract.StartDate.Value)
            {
                throw new PXException(Messages.ActivationDateError);
            }

            if (contract.ExpireDate != null && date.Value > contract.ExpireDate.Value)
            {
                throw new PXException("Activation date of a contract cannot be greater then the Expire Date of a Contract");
            }

            if (contract.IsPendingUpdate == true)
            {
                DateTime minDate = schedule.LastDate ?? contract.StartDate.Value;

                if (date < minDate || date > schedule.NextDate.Value)
                {
                    throw new PXException("Update can only be activated if the effective date is between the Last Billing Date and Next Billing Date");
                }
            }
			foreach (ContractItem item in PXSelectJoin<ContractItem, InnerJoin<ContractDetail, On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>>, Where<ContractDetail.contractID, Equal<Required<Contract.contractID>>>>.Select(this, contract.ContractID))
			{
				string message;
				if (!ContractItemMaint.IsValidItemPrice(this, item, out message))
				{
					throw new PXException(Messages.SpecificItemNotSpecificPrice, item.ContractItemCD, message);
				}
			}

            #endregion

	        ClearState();
            ClearBillingTrace(contractID);

			CreateNewRevision(contractID, ContractAction.SetupAndActivate, ContractStatus.Activated);

            Contract template = Contracts.SelectSingle(contract.TemplateID);
			contract.ScheduleStartsOn = template.ScheduleStartsOn;

            List<InvoiceData> invoices = new List<InvoiceData>();

			InvoiceData data = new InvoiceData(date.Value);
			data.UsageData.AddRange(GetSetupFee(contract));
			data.UsageData.AddRange(GetActivationFee(contract, date));
			data.UsageData.AddRange(GetPrepayment(contract, date, null, null));

			if (data.UsageData.Count > 0)
				invoices.Add(data);

            
            using (PXTransactionScope ts = new PXTransactionScope())
            {
                if (invoices.Count > 0)
                {
					Customer customer;
					Location location;
					SetBillingTarget(contract, out customer, out location);

					CreateInvoice(contract, template, invoices, customer, location, ContractAction.SetupAndActivate);
                }

					schedule.LastDate = date;

                foreach (ARInvoice invoice in doclist)
                {
                    ContractBillingTrace trace = new ContractBillingTrace();
                    trace.ContractID = contractID;
                    trace.DocType = invoice.DocType;
                    trace.RefNbr = invoice.RefNbr;
                    trace.LastDate = schedule.LastDate;
                    trace.NextDate = schedule.NextDate;
                   
                    BillingTrace.Insert(trace);
                }
				if (schedule.Type != BillingType.OnDemand)
					schedule.NextDate = GetNextBillingDate(schedule.Type, contract.CustomerID, date.Value, date);
                contract.EffectiveFrom = contract.StartDate;
                schedule.StartBilling = date;

                BillingSchedule.Update(schedule);
                contract.IsActive = true;
                contract.Status = ContractStatus.Activated;
				contract.ServiceActivate = true;
                Contracts.Update(contract);

                UpdateHistory(contract);

                this.Actions.PressSave();

                ts.Complete();
            }//ts

            AutoReleaseInvoice(contract);
        }

		private void CreateInvoice(Contract contract, Contract template, List<InvoiceData> invoices, Customer customer, Location location, string action)
		{
			ARInvoiceEntry invoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
			AROpenPeriodAttribute.DefaultFirstOpenPeriod<ARInvoice.finPeriodID>(invoiceEntry.Document.Cache);
			invoiceEntry.ARSetup.Current.RequireControlTotal = false;
			invoiceEntry.ARSetup.Current.LineDiscountTarget = LineDiscountTargetType.ExtendedPrice;
			invoiceEntry.FieldVerifying.AddHandler<ARInvoice.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			invoiceEntry.FieldVerifying.AddHandler<ARTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

			foreach (InvoiceData invData in invoices)
			{
				invoiceEntry.Clear(PXClearOption.ClearAll);

				ARInvoice invoice = (ARInvoice)invoiceEntry.Document.Cache.CreateInstance();
				invoice.DocType = invData.GetDocType();
				invoice.DocDate = invData.InvoiceDate;
				int mult = invoice.DocType == ARDocType.CreditMemo ? -1 : 1;
				invoice = invoiceEntry.Document.Insert(invoice);

				invoice.CustomerID = customer.BAccountID;
				invoice.CustomerLocationID = location.LocationID;
				switch (action)
				{
					case ContractAction.Setup:
						invoice.DocDesc = string.Format(PXMessages.LocalizeNoPrefix(Messages.SettingUpContract), contract.ContractCD, contract.Description);
						break;
					case ContractAction.Activate:
						invoice.DocDesc = string.Format(PXMessages.LocalizeNoPrefix(contract.IsPendingUpdate == true ? Messages.UpgradingContract : Messages.ActivatingContract), contract.ContractCD, contract.Description);
						break;
					case ContractAction.SetupAndActivate:
					default:
						invoice.DocDesc = string.Format(PXMessages.LocalizeNoPrefix(Messages.ActivatingContract), contract.ContractCD, contract.Description);
						break;
				}

				invoice = invoiceEntry.Document.Update(invoice);
				invoiceEntry.customer.Current.CreditRule = customer.CreditRule;

				invoice.ProjectID = contract.ContractID;
				invoice.CuryID = contract.CuryID;

				invoice = invoiceEntry.Document.Update(invoice);

				foreach (UsageData item in invData.UsageData)
				{
					//Note: The transactions is first inserted and then updated - this pattern is required so that Discounts are not reseted at the ARInvoice level.
					ARTran tran = invoiceEntry.Transactions.Insert();
					if (item.TranDate != null)
						tran.TranDate = item.TranDate;
					tran.InventoryID = item.InventoryID;
					tran.TranDesc = string.IsNullOrEmpty(item.Prefix)
										? item.Description
										: item.Prefix + ": " + item.Description;
					tran.UOM = item.UOM;
					tran.SalesPersonID = contract.SalesPersonID;
					tran = invoiceEntry.Transactions.Update(tran); //Discounts are set;

					//Set ARInvoice discounts:
                    tran.ManualDisc = true;
                    tran.DiscountID = item.DiscountID;
                    tran.DiscountSequenceID = item.DiscountSeq;

					tran.Qty = item.Qty * mult;
					if (item.IsTranData == null || item.IsTranData == true)
					{
						if (item.IsFree == true)
						{
							tran.CuryUnitPrice = 0;
							tran.CuryExtPrice = 0;
						}
						else if (item.PriceOverride != null)
						{
							tran.CuryUnitPrice = item.PriceOverride.Value;
							tran = invoiceEntry.Transactions.Update(tran);

							decimal extPriceRaw = tran.CuryUnitPrice.GetValueOrDefault() * tran.Qty.GetValueOrDefault() * item.Proportion.GetValueOrDefault(1);
							tran.CuryExtPrice = PXDBCurrencyAttribute.RoundCury(invoiceEntry.Transactions.Cache, tran, extPriceRaw);
						}
					}
					else
					{
						tran.Qty = 0m;
						tran.UOM = null;
						tran.CuryUnitPrice = 0m;
						tran.CuryExtPrice = item.ExtPrice * mult;
					}
					tran = invoiceEntry.Transactions.Update(tran);// price default is handled by ARInvoiceEntry
				}


				if (template.AutomaticReleaseAR == true)
				{
					invoiceEntry.Caches[typeof(ARInvoice)].SetValueExt<ARInvoice.hold>(invoice, false);
					invoice = invoiceEntry.Document.Update(invoice);
				}

				doclist.Add((ARInvoice)invoiceEntry.Caches[typeof(ARInvoice)].Current);
				invoiceEntry.Save.Press();
			}
		}

        private void UndoInvoices(Contract contract)
        {
	        var toRemove = new List<ARInvoice>();

	        foreach(var d in GetDocuments(contract))
	        {
                if (d.Released != true)
                {
                    toRemove.Add(d);
                }
                else if (CanIgnoreInvoice(d) == false)
                {
                    throw new PXException(Messages.CannotUndoActionDueToReleasedDocument);
                }
	        }

            if (toRemove.Any())
            {
                var invoiceEntry = CreateInvoiceGraph();
                Remove(toRemove);
            }
        }

        private IEnumerable<ARInvoice> GetDocuments(Contract contract)
        {
            var docs = new List<ARInvoice>();
            foreach (ARInvoice invoice in PXSelectJoin<ARInvoice,
                     InnerJoin<ContractBillingTrace, On<ContractBillingTrace.docType, Equal<ARInvoice.docType>,
                         And<ContractBillingTrace.refNbr,Equal<ARInvoice.refNbr>>>>,
                     Where<ContractBillingTrace.contractID, Equal<Required<Contract.contractID>>>>.Select(this, contract.ContractID))
            {
                docs.Add(invoice);
            }
            return docs;
        }

        private void Remove(IEnumerable<ARInvoice> documents)
        {
            var invoiceEntry = CreateInvoiceGraph();
            foreach (var d in documents)
            {
                invoiceEntry.Document.Current = d;
                invoiceEntry.Document.Delete(d);
                invoiceEntry.Save.Press();
            }
        }

        private bool CanIgnoreInvoice(ARInvoice invoice)
        {
            if (invoice.Released != true || invoice.OpenDoc == true)
                return false;

            if(invoice.DocType != ARDocType.Invoice && invoice.DocType != ARDocType.CreditMemo)
                return false;

            string reversingDocType = invoice.DocType == ARDocType.Invoice ? ARDocType.CreditMemo : ARDocType.DebitMemo;
            List<PXResult<ARAdjust>> items = null;

            if (invoice.DocType == ARDocType.Invoice)
            {
                var applications = new PXSelectJoin<ARAdjust,
                                        InnerJoin<ARRegister, On<ARRegister.docType, Equal<ARAdjust.adjgDocType>, And<ARRegister.refNbr, Equal<ARAdjust.adjgRefNbr>>>>,
                                        Where<ARAdjust.adjdDocType, Equal<Required<ARInvoice.docType>>,
                                          And<ARAdjust.adjdRefNbr, Equal<Required<ARInvoice.refNbr>>,
                                          And<ARAdjust.released, Equal<CS.boolTrue>,
                                          And<ARAdjust.voided, Equal<CS.boolFalse>>>>>>(this);

                items = applications.Select(invoice.DocType, invoice.RefNbr).ToList();
            }
            else if (invoice.DocType == ARDocType.CreditMemo)
            {
                var applications = new PXSelectJoin<ARAdjust,
                                        InnerJoin<ARRegister, On<ARRegister.docType, Equal<ARAdjust.adjdDocType>, And<ARRegister.refNbr, Equal<ARAdjust.adjdRefNbr>>>>,
                                        Where<ARAdjust.adjgDocType, Equal<Required<ARInvoice.docType>>,
                                          And<ARAdjust.adjgRefNbr, Equal<Required<ARInvoice.refNbr>>,
                                          And<ARAdjust.released, Equal<CS.boolTrue>,
                                          And<ARAdjust.voided, Equal<CS.boolFalse>>>>>>(this);

                items = applications.Select(invoice.DocType, invoice.RefNbr).ToList();
            }

            if (items.Count != 1)
                return false;

            var reversal = items[0].GetItem<ARRegister>();

            return reversal.DocType == reversingDocType &&
                   reversal.OrigDocType == invoice.DocType &&
                   reversal.OrigRefNbr == invoice.RefNbr;
        }

        private ARInvoiceEntry CreateInvoiceGraph()
        {
            ARInvoiceEntry invoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
			AROpenPeriodAttribute.DefaultFirstOpenPeriod<ARInvoice.finPeriodID>(invoiceEntry.Document.Cache);
            invoiceEntry.ARSetup.Current.RequireControlTotal = false;
			invoiceEntry.ARSetup.Current.LineDiscountTarget = LineDiscountTargetType.ExtendedPrice;
            invoiceEntry.Views.Caches.Add(typeof(PMTranEx));//Trans are saved via InvoiceEntry to correctly save the reference to ARTran.
            invoiceEntry.FieldVerifying.AddHandler<ARInvoice.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
            invoiceEntry.FieldVerifying.AddHandler<ARTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

            return invoiceEntry;
        }
        
        public virtual void Bill(int? contractID, DateTime? date=null)
        {
            #region Parameter and State Verification

            if (contractID == null)
                throw new ArgumentNullException("contractID");

            Contract contract = Contracts.Select(contractID);
            ContractBillingSchedule schedule = (ContractBillingSchedule)BillingSchedule.Select(contractID);

            if (contract.IsActive != true)
                throw new PXException("Contract must be Active.");

            if (contract.IsCancelled == true)
                throw new PXException("Contract is Cancelled/Terminated and cannot be Billed.");

            if (contract.IsCompleted == true)
                throw new PXException("Contract is Completed/Expired and cannot be Billed.");

			if (schedule.Type == BillingType.OnDemand && date == null)
				throw new PXException("Billing Date must be Set");

			if (date > contract.ExpireDate)
				throw new PXException("Billing Date is greater then Expiration Date");

			if (date < schedule.LastDate)
				throw new PXException("Billing Date is less then Last Billing Date");

			foreach (ContractItem item in PXSelectJoin<ContractItem, InnerJoin<ContractDetail, On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>>, Where<ContractDetail.contractID, Equal<Required<Contract.contractID>>>>.Select(this, contract.ContractID))
			{
				string message;
				if (!ContractItemMaint.IsValidItemPrice(this, item, out message))
				{
					throw new PXException(Messages.SpecificItemNotSpecificPrice, item.ContractItemCD, message);
				}
			}
            #endregion

            ContractBillingTrace trace = new ContractBillingTrace();
            trace.ContractID = contractID;
            trace.LastDate = schedule.LastDate;
            trace.NextDate = schedule.NextDate;

			ClearState();
            ClearBillingTrace(contractID);

			if (schedule.Type == BillingType.OnDemand)
			{
				schedule.NextDate = date;
				BillingSchedule.Update(schedule);
			}

            Contract template = Contracts.SelectSingle(contract.TemplateID);

            availableQty = new Dictionary<int?, decimal?>();
            List<UsageData> data;
            Dictionary<int, List<TranNotePair>> sourceTran;
            List<UsageData> tranData;

            if (IsLastBillBeforeExpiration(contract))
            {
	            RaiseErrorIfUnreleasedUsageExist(contract);

                contract.IsCompleted = true;
                contract.Status = ContractStatus.Expired;
				contract.ServiceActivate = false;
                Contracts.Update(contract);
            }

			CreateNewRevision(contract.ContractID, ContractAction.Bill, contract.Status);

			RecalcUsage(contract, out data, out sourceTran, out tranData);

			DateTime? billingDate;
			DateTime? nextBillingDate;
			if (schedule.Type == BillingType.OnDemand)
			{
				billingDate = date;
				nextBillingDate = null;
			}
			else
			{
				billingDate = schedule.NextDate;

				nextBillingDate = GetNextBillingDate(schedule.Type, contract.CustomerID, schedule.NextDate, schedule.StartBilling);
				if (contract.ExpireDate != null && contract.ExpireDate < nextBillingDate)
				{
					nextBillingDate = contract.ExpireDate;
				}
				schedule.NextDate = nextBillingDate;
			}
            schedule.ChargeRenewalFeeOnNextBilling = false;
            BillingSchedule.Update(schedule);

			using (PXTransactionScope ts = new PXTransactionScope())
            {
                if (data.Count > 0)
                {
                    Customer customer;
                    Location location;
                    SetBillingTarget(contract, out customer, out location);

                    ARInvoiceEntry invoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
					AROpenPeriodAttribute.DefaultFirstOpenPeriod<ARInvoice.finPeriodID>(invoiceEntry.Document.Cache);
					invoiceEntry.ARSetup.Current.RequireControlTotal = false;
					invoiceEntry.ARSetup.Current.LineDiscountTarget = LineDiscountTargetType.ExtendedPrice;
                    invoiceEntry.Views.Caches.Add(typeof(PMTranEx));//Trans are saved via InvoiceEntry to correctly save the reference to ARTran.
                    invoiceEntry.FieldVerifying.AddHandler<ARInvoice.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
                    invoiceEntry.FieldVerifying.AddHandler<ARTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

					ARInvoice invoice = (ARInvoice)invoiceEntry.Document.Cache.CreateInstance();
					invoice.DocType = ARDocType.Invoice;
					invoice.DocDate = billingDate;
					invoice = invoiceEntry.Document.Insert(invoice);

					invoice.CustomerID = customer.BAccountID;
					invoice.CustomerLocationID = location.LocationID;
					invoice.DocDesc = string.Format(PXMessages.LocalizeNoPrefix(Messages.BillingContract), contract.ContractCD, contract.Description);

					invoice = invoiceEntry.Document.Update(invoice);

					invoiceEntry.customer.Current.CreditRule = customer.CreditRule;

					invoice.ProjectID = contract.ContractID;
					invoice.CuryID = contract.CuryID;

					invoice = invoiceEntry.Document.Update(invoice);
                    
                    foreach (UsageData item in data)
                    {
                        if (item.Qty == 0 && item.ExtPrice == 0)
                            continue;

                        //Note: The transactions is first inserted and then updated - this pattern is required so that Discounts are not reseted at the ARInvoice level.
                        ARTran tran = invoiceEntry.Transactions.Insert();
                        tran.InventoryID = item.InventoryID;
                        tran.TranDesc = string.IsNullOrEmpty(item.Prefix) ? item.Description : item.Prefix + ": " + item.Description;
                        tran.UOM = item.UOM;
                        //tran.DeferredCode = item.DefCode;
                        if (item.BranchID != null)
                            tran.BranchID = item.BranchID;
                        tran.EmployeeID = item.EmployeeID;
                        tran.SalesPersonID = contract.SalesPersonID;

                        tran = invoiceEntry.Transactions.Update(tran); //Discounts are set;

                        //Set ARInvoice discounts:
                        tran.ManualDisc = true;
                        tran.DiscountID = item.DiscountID;
                        tran.DiscountSequenceID = item.DiscountSeq;
                        
                        tran.Qty = item.Qty;
						if (item.IsTranData == null || item.IsTranData == true)
						{
							if (item.IsFree == true)
							{
								tran.CuryUnitPrice = 0;
								tran.CuryExtPrice = 0;
							}
							else if (item.PriceOverride != null)
							{
								tran.CuryUnitPrice = item.PriceOverride.Value;
								tran = invoiceEntry.Transactions.Update(tran);

								decimal extPriceRaw = tran.CuryUnitPrice.GetValueOrDefault() * tran.Qty.GetValueOrDefault() * item.Proportion.GetValueOrDefault(1);
								tran.CuryExtPrice = PXDBCurrencyAttribute.RoundCury(invoiceEntry.Transactions.Cache, tran, extPriceRaw);
							}
						}
						else
						{
							tran.Qty = 0m;
							tran.UOM = null;
							tran.CuryUnitPrice = 0m;
							tran.CuryExtPrice = item.ExtPrice;
						}
						tran = invoiceEntry.Transactions.Update(tran);// price default is handled by ARInvoiceEntry

                        item.RefLineNbr = tran.LineNbr;
                    }

                    UpdateReferencePMTran2ARTran(invoiceEntry, sourceTran, tranData);

					if (template.AutomaticReleaseAR == true)
					{
						invoiceEntry.Caches[typeof(ARInvoice)].SetValueExt<ARInvoice.hold>(invoice, false);
						invoice = invoiceEntry.Document.Update(invoice);
					}

                    doclist.Add((ARInvoice)invoiceEntry.Caches[typeof(ARInvoice)].Current);
                    invoiceEntry.Actions.PressSave();


                    trace.DocType = doclist[0].DocType;
                    trace.RefNbr = doclist[0].RefNbr;
                    BillingTrace.Insert(trace);
                }
				if (schedule.Type == BillingType.OnDemand)
					schedule.NextDate = null;
				schedule.LastDate = billingDate;
				BillingSchedule.Update(schedule);

                UpdateHistory(contract);
                
                this.Actions.PressSave();
                ts.Complete();
            }

            AutoReleaseInvoice(contract);
        }

        public virtual void Renew(int? contractID)
        {
            #region Parameter and State Verification

            if (contractID == null)
                throw new ArgumentNullException("contractID");

            Contract contract = Contracts.Select(contractID);
            ContractBillingSchedule schedule = (ContractBillingSchedule)BillingSchedule.Select(contractID);

            if (contract.IsCancelled == true)
                throw new PXException("Contract is Cancelled/Terminated and cannot be Activated.");
            
            if (contract.ExpireDate == null)
                throw new PXException("Contract Expire date must be not null for a Contract that is Renewed.");

            if (schedule.ChargeRenewalFeeOnNextBilling == true)
            {
                throw new PXException(Messages.RenewFeeNotCollected);
            }

			foreach (ContractItem item in PXSelectJoin<ContractItem, InnerJoin<ContractDetail, On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>>, Where<ContractDetail.contractID, Equal<Required<Contract.contractID>>>>.Select(this, contract.ContractID))
			{
				string message;
				if (!ContractItemMaint.IsValidItemPrice(this, item, out message))
				{
					throw new PXException(Messages.SpecificItemNotSpecificPrice, item.ContractItemCD, message);
				}
			}

            #endregion

            ContractBillingTrace trace = new ContractBillingTrace();
            trace.ContractID = contractID;
            trace.LastDate = schedule.LastDate;
            trace.NextDate = schedule.NextDate;

			ClearState();
            ClearBillingTrace(contractID);

            DateTime newStartDate = contract.ExpireDate.Value.Date.AddDays(1);
            contract.ExpireDate = ContractMaint.GetExpireDate(contract, newStartDate);
            contract.IsActive = true;
            contract.IsCompleted = false;
            contract.Status = ContractStatus.Activated;
			contract.ServiceActivate = true;
            Contracts.Update(contract);
            
			CreateNewRevision(contractID, ContractAction.Renew, ContractStatus.Activated);
            
            schedule.ChargeRenewalFeeOnNextBilling = true;
            schedule.NextDate = GetNextBillingDate(schedule.Type, contract.CustomerID, newStartDate, newStartDate);
            BillingSchedule.Update(schedule);

			contract = Contracts.Select(contractID);

	        ContractRenewalHistory crh = CurrentRenewalHistory.SelectSingle(contract.ContractID, contract.RevID);
            crh.RenewalDate = newStartDate;
            crh.RenewedBy = this.Accessinfo.UserID;
			crh = CurrentRenewalHistory.Update(crh);

            Contract template = Contracts.SelectSingle(contract.TemplateID);

            availableQty = new Dictionary<int?, decimal?>();
            List<UsageData> data;
            Dictionary<int, List<TranNotePair>> sourceTran;
            List<UsageData> tranData;
            RecalcUsage(contract, out data, out sourceTran, out tranData);

            using (PXTransactionScope ts = new PXTransactionScope())
            {
                if (data.Count > 0)
                {
                    Customer customer;
                    Location location;
                    SetBillingTarget(contract, out customer, out location);

                    ARInvoiceEntry invoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
					AROpenPeriodAttribute.DefaultFirstOpenPeriod<ARInvoice.finPeriodID>(invoiceEntry.Document.Cache);
					invoiceEntry.ARSetup.Current.RequireControlTotal = false;
					invoiceEntry.ARSetup.Current.LineDiscountTarget = LineDiscountTargetType.ExtendedPrice;
                    invoiceEntry.Views.Caches.Add(typeof(PMTranEx));//Trans are saved via InvoiceEntry to correctly save the reference to ARTran.
                    invoiceEntry.FieldVerifying.AddHandler<ARInvoice.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
                    invoiceEntry.FieldVerifying.AddHandler<ARTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

					ARInvoice invoice = (ARInvoice)invoiceEntry.Document.Cache.CreateInstance();
					invoice.DocType = ARDocType.Invoice;
					invoice.DocDate = newStartDate;
					invoice = invoiceEntry.Document.Insert(invoice);

					invoice.CustomerID = customer.BAccountID;
					invoice.CustomerLocationID = location.LocationID;
					invoice.DocDesc = string.Format(PXMessages.LocalizeNoPrefix(Messages.RenewingContract), contract.ContractCD, contract.Description);

					invoice = invoiceEntry.Document.Update(invoice);

					invoiceEntry.customer.Current.CreditRule = customer.CreditRule;

					invoice.ProjectID = contract.ContractID;
					invoice.CuryID = contract.CuryID;

					invoice = invoiceEntry.Document.Update(invoice);

					CurrencyInfo curyinfo = (CurrencyInfo)invoiceEntry.Caches[typeof(CurrencyInfo)].Current;

                    foreach (UsageData item in data)
                    {
                        if (item.Qty == 0 && item.ExtPrice == 0)
                            continue;

                        //Note: The transactions is first inserted and then updated - this pattern is required so that Discounts are not reseted at the ARInvoice level.
                        ARTran tran = invoiceEntry.Transactions.Insert();
                        tran.InventoryID = item.InventoryID;
                        tran.TranDesc = string.IsNullOrEmpty(item.Prefix) ? item.Description : item.Prefix + ": " + item.Description;
                        tran.Qty = item.Qty;
                        tran.UOM = item.UOM;
                        //tran.DeferredCode = item.DefCode;
                        tran.ProjectID = contract.ContractID;
                        if (item.BranchID != null)
                            tran.BranchID = item.BranchID;
                        tran.EmployeeID = item.EmployeeID;
                        tran.SalesPersonID = contract.SalesPersonID;

                        tran = invoiceEntry.Transactions.Update(tran); //Discounts are set;

                        //Set ARInvoice discounts:
                        tran.ManualDisc = true;
                        tran.DiscountID = item.DiscountID;
                        tran.DiscountSequenceID = item.DiscountSeq;

                        tran.Qty = item.Qty;

						if (item.IsTranData == null || item.IsTranData == true)
                        {
                            if (item.IsFree == true)
                            {
                                tran.CuryUnitPrice = 0;
                                tran.CuryExtPrice = 0;

                                tran = invoiceEntry.Transactions.Update(tran);
                            }
                            else if (item.PriceOverride != null)
                            {
                                tran.CuryUnitPrice = item.PriceOverride;
								tran = invoiceEntry.Transactions.Update(tran);

                                decimal extPriceRaw = tran.CuryUnitPrice.GetValueOrDefault() * tran.Qty.GetValueOrDefault() * item.Proportion.GetValueOrDefault(1);
                                tran.CuryExtPrice = PXDBCurrencyAttribute.RoundCury(invoiceEntry.Transactions.Cache, tran, extPriceRaw);

                                tran = invoiceEntry.Transactions.Update(tran);
                            }
                            else
                            {
                                tran = invoiceEntry.Transactions.Update(tran);// price default is handled by ARInvoiceEntry
                            }
                        }
                        else
                        {
							tran.Qty = 0m;
							tran.UOM = null;
							tran.CuryUnitPrice = 0m;
							decimal extPrice = (item.Proportion ?? 1) * item.ExtPrice.Value;
							tran.CuryExtPrice = PXDBCurrencyAttribute.RoundCury(invoiceEntry.Transactions.Cache, tran, extPrice);;

                            tran = invoiceEntry.Transactions.Update(tran);

                        }

                        item.RefLineNbr = tran.LineNbr;
                    }

                    UpdateReferencePMTran2ARTran(invoiceEntry, sourceTran, tranData);

					if (template.AutomaticReleaseAR == true)
					{
						invoiceEntry.Caches[typeof(ARInvoice)].SetValueExt<ARInvoice.hold>(invoice, false);
						invoice = invoiceEntry.Document.Update(invoice);
					}

                    doclist.Add((ARInvoice)invoiceEntry.Caches[typeof(ARInvoice)].Current);
                    invoiceEntry.Actions.PressSave();
                    
                    schedule.LastDate = newStartDate;

                    trace.DocType = doclist[0].DocType;
                    trace.RefNbr = doclist[0].RefNbr;
                    BillingTrace.Insert(trace);
                }

                schedule.ChargeRenewalFeeOnNextBilling = false;
                BillingSchedule.Update(schedule);

                UpdateHistory(contract);

                this.Actions.PressSave();
                ts.Complete();
            }

            AutoReleaseInvoice(contract);
        }

        public virtual void Terminate(int? contractID, DateTime? date)
        {
            #region Parameter and State Verification

            if (contractID == null)
                throw new ArgumentNullException("contractID");

            if (date == null)
                throw new ArgumentNullException("date");

            Contract contract = Contracts.Select(contractID);
            ContractBillingSchedule schedule = (ContractBillingSchedule)BillingSchedule.Select(contractID);

            if (contract.IsCancelled == true)
                throw new PXException("Contract is already Terminated");

            DateTime minDate = schedule.LastDate ?? contract.StartDate.Value;
            if (date.Value < minDate)
            {
                throw new PXException("Termination date of a Contract cannot be earlier than the Last Billing Date of the Contract");
            }

            if (schedule.Type != BillingType.OnDemand && schedule.NextDate.HasValue && date.Value > schedule.NextDate.Value)
            {
                throw new PXException("Termination date of a Contract cannot be later than the Next Billing Date of the Contract");
            }

			foreach (ContractItem item in PXSelectJoin<ContractItem, InnerJoin<ContractDetail, On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>>, Where<ContractDetail.contractID, Equal<Required<Contract.contractID>>>>.Select(this, contract.ContractID))
			{
				string message;
				if (!ContractItemMaint.IsValidItemPrice(this, item, out message))
				{
					throw new PXException(Messages.SpecificItemNotSpecificPrice, item.ContractItemCD, message);
				}
			}

			RaiseErrorIfUnreleasedUsageExist(contract);

            #endregion

            ContractBillingTrace baseTrace = new ContractBillingTrace();
            baseTrace.ContractID = contractID;
            baseTrace.LastDate = schedule.LastDate;
            baseTrace.NextDate = schedule.NextDate;

			ClearState();
            ClearBillingTrace(contractID);

			Contract template = Contracts.SelectSingle(contract.TemplateID);

            contract.Status = ContractStatus.Canceled;
            contract.IsCancelled = true;
            contract.IsActive = false;
            contract.TerminationDate = date;
			contract.ServiceActivate = false;
            Contracts.Update(contract);

			CreateNewRevision(contractID, ContractAction.Terminate, ContractStatus.Canceled);

            List<InvoiceData> invoices = new List<InvoiceData>();

            InvoiceData data = new InvoiceData(date.Value);
            var fee = GetTerminationFee(contract, schedule.LastDate, schedule.NextDate, date.Value);
            data.UsageData.AddRange(fee);

            //data.UsageData.AddRange(GetPrepayment(contract, date, schedule.NextDate));

            if (data.UsageData.Count > 0)
                invoices.Add(data);


            using (PXTransactionScope ts = new PXTransactionScope())
            {
                if (invoices.Count > 0)
                {
                    Customer customer;
                    Location location;
                    SetBillingTarget(contract, out customer, out location);

                    CreateInvoice(contract, template, invoices, customer, location, ContractAction.Activate);
                        }

                foreach (ARInvoice invoice in doclist)
                        {
                    ContractBillingTrace trace = new ContractBillingTrace();
                    trace.ContractID = contractID;
                    trace.DocType = invoice.DocType;
                    trace.RefNbr = invoice.RefNbr;
                    trace.LastDate = baseTrace.LastDate;
                    trace.NextDate = baseTrace.NextDate;

                    BillingTrace.Insert(trace);
                }

                schedule.LastDate = date;
                schedule.NextDate = null;
                BillingSchedule.Update(schedule);

                UpdateHistory(contract);

                this.Actions.PressSave();
                ts.Complete();

            }//ts

            AutoReleaseInvoice(contract);
        }

        public virtual void UndoBilling(int? contractID)
        {
            if (contractID == null)
                throw new ArgumentNullException("contractID");

            Contract contract = Contracts.Select(contractID);

			if (contract.IsLastActionUndoable != true)
                throw new PXException(Messages.CannotUndoAction);

            var previousHistory = new PXSelect<ContractRenewalHistory, 
                Where<ContractRenewalHistory.contractID, Equal<Required<Contract.contractID>>,
                And<ContractRenewalHistory.revID, Equal<Required<Contract.revID>>>>>(this).SelectSingle(contract.ContractID, (contract.RevID ?? 0) - 1);

            if (previousHistory == null)
                throw new PXException(Messages.CannotUndoAction);

            ContractBillingSchedule schedule = (ContractBillingSchedule)BillingSchedule.Select(contractID);
            ContractBillingTrace lastBilling = BillingTrace.SelectWindowed(0, 1, contractID);
            ContractRenewalHistory toUndo = CurrentRenewalHistory.SelectSingle(contract.ContractID, contract.RevID);

            using (PXTransactionScope ts = new PXTransactionScope())
            {
				contract.LastActiveRevID = GetLastActiveRevisionID(contract);
				contract.RevID = previousHistory.RevID;

                contract.Status = previousHistory.Status;

                contract.IsLastActionUndoable = false;

                Contracts.Update(contract);

                UndoInvoices(contract);

				RestoreScheduleFromHistory(schedule, previousHistory);

				if (toUndo.Action == ContractAction.Terminate)
                {
					contract.TerminationDate = null;
                }
				if (toUndo.Action == ContractAction.Renew && contract.Type == ContractType.Expiring)
                {
					ContractMaint graph = new ContractMaint();
					graph.Clear();
					graph.Contracts.Current = PXSelect<CT.Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(graph, toUndo.ChildContractID);
					graph.Billing.Current = graph.Billing.Select();
					try
                {
						graph.Delete.Press();
                }
					catch (Exception)
                {
						throw new PXException("Cannot to delete Renewing Contract");
                }
                }

				RestoreFieldsFromHistory(contract, previousHistory);

                ClearBillingTrace(contract.ContractID);
                ClearFuture(contract);

                this.Actions.PressSave();
                ts.Complete();
            }
		}

        private void RestoreFieldsFromHistory(Contract contract, ContractRenewalHistory history)
                {
            contract.EffectiveFrom = history.EffectiveFrom;
            contract.ExpireDate = history.ExpireDate;
            contract.ActivationDate = history.ActivationDate;
            contract.StartDate = history.StartDate;

            contract.IsActive = history.IsActive;
            contract.IsCancelled = history.IsCancelled;
            contract.IsCompleted = history.IsCompleted;
            contract.IsPendingUpdate = history.IsPendingUpdate;

            contract.DiscountID = history.DiscountID;

            Contracts.Update(contract);
                }

        private void RestoreScheduleFromHistory(ContractBillingSchedule schedule, ContractRenewalHistory history)
                {
            schedule.LastDate = history.LastDate;
            schedule.NextDate = history.NextDate;
            schedule.StartBilling = history.StartBilling;
                    BillingSchedule.Update(schedule);
        }

        private int? GetLastActiveRevisionID(Contract contract)
        {
            if (contract == null)
                return null;

            ContractRenewalHistory last = new PXSelect<ContractRenewalHistory,
                Where<ContractRenewalHistory.contractID, Equal<Required<Contract.contractID>>,
                And<ContractRenewalHistory.status, Equal<ContractStatus.ContractStatusActivated>,
                And<ContractRenewalHistory.revID,Less<Required<Contract.revID>>>>>,
                OrderBy<Desc<ContractRenewalHistory.revID>>>(this).SelectSingle(contract.ContractID,contract.RevID);
            
            return last == null ? null : last.RevID;
        }

        private void UpdateStatusFlags(Contract contract)
        {
            contract.IsActive = contract.Status == ContractStatus.Activated || contract.Status == ContractStatus.InUpgrade;
            contract.IsCancelled = contract.Status == ContractStatus.Canceled;
            contract.IsCompleted = contract.Status == ContractStatus.Completed;
            contract.IsPendingUpdate = contract.Status == ContractStatus.InUpgrade;
        }

		public virtual void Upgrade(int? contractID)
		{
			#region Parameter and State Verification

			Contract contract = Contracts.Select(contractID);

			contract = Contracts.Cache.CreateCopy(contract) as Contract;

			if (contract == null)
				throw new ArgumentNullException("contractID");

			if (contract.IsActive != true)
				throw new PXException("Contract must be Active.");

			if (contract.IsCancelled == true)
				throw new PXException("Contract is Cancelled/Terminated and cannot be Upgraded.");

			if (contract.IsCompleted == true)
				throw new PXException("Contract is Completed/Expired and cannot be Upgraded.");

			foreach (ContractItem item in PXSelectJoin<ContractItem, InnerJoin<ContractDetail, On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>>, Where<ContractDetail.contractID, Equal<Required<Contract.contractID>>>>.Select(this, contract.ContractID))
			{
				string message;
				if (!ContractItemMaint.IsValidItemPrice(this, item, out message))
				{
					throw new PXException(Messages.SpecificItemNotSpecificPrice, item.ContractItemCD, message);
				}
			}
			#endregion

			ContractBillingSchedule schedule = (ContractBillingSchedule)BillingSchedule.Select(contractID);

            if (schedule.Type == BillingType.Statement)
                return;

			contract.Status = ContractStatus.InUpgrade;
			contract.IsPendingUpdate = true;
			contract.EffectiveFrom = schedule.NextDate ?? Accessinfo.BusinessDate;
			Contracts.Update(contract);

			CreateNewRevision(contractID, ContractAction.Upgrade, ContractStatus.InUpgrade);

            UpdateHistory(contract);

			this.Actions.PressSave();
		}

		protected virtual List<UsageData> GetSetupFee(Contract contract)
		{
            List<UsageData> list = new List<UsageData>();

            foreach (PXResult<ContractDetail, ContractItem, InventoryItem> res in PXSelectJoin<ContractDetail,
            InnerJoin<ContractItem, On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>,
                InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<ContractItem.baseItemID>>>>,
            Where<ContractDetail.contractID, Equal<Required<Contract.contractID>>,
                And<ContractItem.isBaseValid, Equal<True>>>>.Select(this, contract.ContractID))
            {
                ContractDetail det = res;
                ContractItem item = res;
                InventoryItem inventory = res;

                UsageData data = new UsageData();
                data.InventoryID = item.BaseItemID;
                data.Description = "Setup: " + inventory.Descr;
                data.UOM = inventory.BaseUnit;
                data.Qty = det.Qty;
                data.PriceOverride = det.BasePriceVal;
                data.ExtPrice = data.Qty * det.BasePriceVal;
                data.DiscountID = det.BaseDiscountID;
                data.DiscountSeq = det.BaseDiscountSeq;

                list.Add(data);

				if (item.Deposit == true)
				{
					det.DepositAmt = data.ExtPrice;
					ContractDetails.Update(det);
				}
            }
			return list;
            }

		protected virtual List<UsageData> GetActivationFee(Contract contract, DateTime? date)
		{
			List<UsageData> list = new List<UsageData>();
            PXSelectBase<ContractDetail> renewalItemsSelect = new
                    PXSelectJoin<ContractDetail,
                    InnerJoin<ContractItem, On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>,
                    InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<ContractItem.renewalItemID>>>>,
                    Where<ContractDetail.contractID, Equal<Required<Contract.contractID>>,
                    And<ContractItem.isRenewalValid, Equal<True>>>>(this);

            foreach (PXResult<ContractDetail, ContractItem, InventoryItem> res in renewalItemsSelect.Select(contract.ContractID))
            {
                ContractDetail det = res;
                ContractItem item = res;
                InventoryItem inventory = res;

                if (item.CollectRenewFeeOnActivation == true)
                {
                    UsageData data = new UsageData();
                    data.InventoryID = inventory.InventoryID;
                    data.Description = "Activate/Renew: " + inventory.Descr;
                    data.UOM = inventory.BaseUnit;
                    data.Qty = det.Qty;
                    data.PriceOverride = det.RenewalPriceVal;
                    data.ExtPrice = det.Qty * det.RenewalPriceVal;
                    data.DiscountID = det.RenewalDiscountID;
                    data.DiscountSeq = det.RenewalDiscountSeq;
                    list.Add(data);
                }
            }

            return list;
        }

		protected virtual void GetUpgradeSetup(IEnumerable<PXResult<ContractDetail, ContractItem>> details, List<UsageData> usagedata, decimal? prorate)
		{
			foreach (PXResult<ContractDetail, ContractItem> res in details)
			{
				ContractDetail det = res;
				ContractItem item = res;

				decimal change = det.Change ?? 0m;

				//only refundable items can be decreased.
				if (item.BaseItemID != null && (change != 0 && (change > 0 || item.Refundable == true)))
				{
					InventoryItem inventory = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, item.BaseItemID);
					InventoryItem inventoryRenew = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, item.RenewalItemID);

					UsageData data = new UsageData();
					data.InventoryID = item.BaseItemID;
					data.Description = "Setup upgrade: " + inventory.Descr;
					data.UOM = inventory.BaseUnit;
					data.Qty = change;
					data.PriceOverride = det.BasePriceVal;
					data.ExtPrice = data.Qty * det.BasePriceVal;
					data.DiscountID = det.BaseDiscountID;
					data.DiscountSeq = det.BaseDiscountSeq;
					if (item.ProrateSetup == true)
						data.Proportion = prorate;
					else
						data.Proportion = 1;

                    if (usagedata != null)
                    {
					usagedata.Add(data);
                    }

					if (item.RenewalItemID != null && item.CollectRenewFeeOnActivation == true)
					{
						UsageData renew = new UsageData();
						renew.InventoryID = item.RenewalItemID;
						renew.Description = "Upgrade activation: " + inventoryRenew.Descr;
						renew.UOM = inventoryRenew.BaseUnit;
						renew.Qty = change;
						renew.PriceOverride = det.RenewalPriceVal;
						renew.ExtPrice = renew.Qty * det.RenewalPriceVal;
						renew.DiscountID = det.RenewalDiscountID;
						renew.DiscountSeq = det.RenewalDiscountSeq;
						renew.Proportion = prorate;

                        if (usagedata != null)
                        {
						usagedata.Add(renew);
					}
				}
			}
		}
		}

		protected virtual void GetUpgradeRecurring(IEnumerable<PXResult<ContractDetail, ContractItem>> details, List<UsageData> usagedata, decimal? prorate)
		{
			foreach (PXResult<ContractDetail, ContractItem> res in details)
			{
				ContractDetail det = res;
				ContractItem item = res;

				decimal change = det.Change ?? 0;

				if (item.RecurringItemID != null && item.DepositItemID == null && (item.RecurringType == RecurringOption.Prepay || change < 0m))
				{
					InventoryItem inventory = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, item.RecurringItemID); 

					string prefix = PXMessages.LocalizeNoPrefix(item.RecurringType == RecurringOption.Prepay ? Messages.PrefixPrepaid : Messages.PrefixIncluded);

					UsageData data = new UsageData();
					data.InventoryID = item.RecurringItemID;
					data.Description = inventory.Descr;
					data.UOM = inventory.BaseUnit;
					data.Qty = (item.RecurringType == RecurringOption.Prepay ? change : -change);
					data.PriceOverride = det.FixedRecurringPriceVal;
					data.ExtPrice = (item.RecurringType == RecurringOption.Prepay ? change : -change)  * det.FixedRecurringPriceVal;
					data.Prefix = prefix;
					data.Proportion = (item.RecurringType == RecurringOption.Prepay ? prorate : 1 - prorate);
					data.DiscountID = det.RecurringDiscountID;
					data.DiscountSeq = det.RecurringDiscountSeq;

					usagedata.Add(data);
				}

                if (item.RecurringItemID != null && item.DepositItemID == null && item.RecurringType == RecurringOption.Usage && change > 0)
                {
                    InventoryItem inventory = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, item.RecurringItemID);

                    string prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixIncluded);

                    UsageData data = new UsageData();
                    data.InventoryID = item.RecurringItemID;
                    data.Description = inventory.Descr;
                    data.UOM = inventory.BaseUnit;
                    data.Qty = -change;
                    data.PriceOverride = det.FixedRecurringPriceVal;
                    data.ExtPrice = -change * det.FixedRecurringPriceVal;
                    data.Prefix = prefix;
                    data.Proportion = 1 - prorate;
                    data.DiscountID = det.RecurringDiscountID;
                    data.DiscountSeq = det.RecurringDiscountSeq;

                    usagedata.Add(data);
                }

                UpdateAvailableQty(det, item);
                AddDepositUsage(det, item, usagedata); 
			}
		}

        protected virtual void GetTerminateRecurring(IEnumerable<PXResult<ContractDetail, ContractItem>> details, List<UsageData> usagedata, decimal? prorate)
        {
            foreach (PXResult<ContractDetail, ContractItem> res in details)
            {
                ContractDetail det = res;
                ContractItem item = res;

                if (item.RecurringItemID != null && item.DepositItemID == null)
                {
                    InventoryItem inventory = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, item.RecurringItemID);

                    string prefix = PXMessages.LocalizeNoPrefix(item.RecurringType == RecurringOption.Prepay ? Messages.PrefixPrepaid : Messages.PrefixIncluded);
                                        
                    decimal unusedQty = Math.Max(det.Qty.GetValueOrDefault(0) - det.Used.GetValueOrDefault(0), 0.0m);

                    if (unusedQty > 0)
                    {
                        UsageData unusedData = new UsageData();
                        unusedData.InventoryID = item.RecurringItemID;
                        unusedData.Description = inventory.Descr;
                        unusedData.UOM = inventory.BaseUnit;
                        unusedData.Qty = (item.RecurringType == RecurringOption.Prepay ? -unusedQty : unusedQty);
                        unusedData.PriceOverride = det.FixedRecurringPriceVal;
                        unusedData.ExtPrice = unusedData.Qty * det.FixedRecurringPriceVal;
                        unusedData.Prefix = prefix;
                        unusedData.Proportion = (item.RecurringType == RecurringOption.Prepay ? prorate : 1 - prorate);
                        unusedData.DiscountID = det.RecurringDiscountID;
                        unusedData.DiscountSeq = det.RecurringDiscountSeq;

                        usagedata.Add(unusedData);
                    }

                    var usedQty = det.Qty - unusedQty;

                    if (usedQty > 0 && item.RecurringType != RecurringOption.Prepay)
                    {
                        UsageData usedData = new UsageData();
                        usedData.InventoryID = item.RecurringItemID;
                        usedData.Description = inventory.Descr;
                        usedData.UOM = inventory.BaseUnit;
                        usedData.Qty = usedQty;
                        usedData.PriceOverride = det.FixedRecurringPriceVal;
                        usedData.ExtPrice = usedData.Qty * det.FixedRecurringPriceVal;
                        usedData.Prefix = prefix;
                        usedData.Proportion = 1.0m;
                        usedData.DiscountID = det.RecurringDiscountID;
                        usedData.DiscountSeq = det.RecurringDiscountSeq;

                        usagedata.Add(usedData);
                    }
                }
                
                UpdateAvailableQty(det, item);
                AddDepositUsage(det, item, usagedata); 
            }
        }

        protected virtual void UpdateAvailableQty(ContractDetail det, ContractItem item)
        {
            decimal change = det.Change ?? 0;

            if (item.RecurringItemID != null && item.DepositItemID == null && (item.RecurringType == RecurringOption.Prepay || change < 0m))
            {
					decimal? avail;
					if (item.ResetUsageOnBilling == true)
					{
						//either last for active details or qty for deleted
						avail = det.LastQty ?? det.Qty;
					}
					else
					{
						decimal billedQty = (det.UsedTotal ?? 0m) - (det.Used ?? 0m);
						avail = (det.LastQty ?? det.Qty) - billedQty;
					}
					availableQty.Add(item.RecurringItemID, Math.Max(0, (decimal)avail));
				}
        }

        protected virtual void AddDepositUsage(ContractDetail det, ContractItem item, List<UsageData> usagedata)
        {
				if (item.BaseItemID != null && item.Deposit == true)
				{
					//TODO: add LastDepositAmt
					ContractDetail billing = det;
					InventoryItem inventory = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, item.BaseItemID); 

					availableDeposit[billing.ContractItemID] = billing.DepositAmt - billing.DepositUsedTotal;

					UsageData tranData = new UsageData();
					tranData.InventoryID = inventory.InventoryID;
					tranData.Description = billing.Description;
					tranData.Qty = 0m;
					tranData.UOM = inventory.BaseUnit;
					tranData.IsTranData = false;
					tranData.PriceOverride = 0m;
					tranData.ExtPrice = 0m;
					tranData.IsFree = false;
					tranData.Prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixPrepaidUsage);
					tranData.IsDollarUsage = true;

					usagedata.Add(tranData);
					depositUsage[billing.ContractItemID] = tranData;
				}
			}

		protected virtual void GetUpgradeUsage(IEnumerable<PXResult<ContractDetail, ContractItem>> details, List<UsageData> list, Contract contract)
		{
			Dictionary<int, List<TranNotePair>> transactions = GetTransactions(contract);

			foreach (KeyValuePair<int, List<TranNotePair>> kv in transactions)
			{
				PXResult<ContractDetail, ContractItem, InventoryItem> billing = null;

				foreach (PXResult<ContractDetail, ContractItem> res in details)
				{
					ContractItem item = res;
					if (item.RecurringItemID == kv.Key)
					{
						InventoryItem inventory = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, item.RecurringItemID);
						billing = new PXResult<ContractDetail, ContractItem, InventoryItem>(res, res, inventory);

						break;
					}
				}

				if (billing != null)
				{
					list.AddRange(ProcessTransactions(contract, billing, kv.Value));

					foreach (TranNotePair tran in kv.Value)
					{
						tran.Tran.Billed = true;
						tran.Tran.BilledDate = Accessinfo.BusinessDate;
						Transactions.Update(tran.Tran);//only Billed field is updated.
					}
				}
			} 
		}

        protected virtual List<UsageData> GetUpgradeFee(Contract contract, DateTime? lastBillingDate, DateTime? nextBillingDate, DateTime? activationDate)
        {
            List<UsageData> list = new List<UsageData>();

            decimal prorate = 1;
			if (contract.ExpireDate != null)
			{
				DateTime? startDate = contract.ScheduleStartsOn == ScheduleStartOption.SetupDate ? contract.StartDate : contract.ActivationDate;
				decimal totalDays = contract.ExpireDate.Value.Date.Subtract(startDate.Value.Date).Days + 1;
				decimal daysLeft = contract.ExpireDate.Value.Date.Subtract(activationDate.Value.Date).Days + 1;
				prorate = daysLeft / totalDays;
			}

			Dictionary<int?, PXResult<ContractDetail, ContractItem>> details = new Dictionary<int?, PXResult<ContractDetail, ContractItem>>();
			Dictionary<int?, PXResult<ContractDetail, ContractItem>> deleted = new Dictionary<int?, PXResult<ContractDetail, ContractItem>>();

			foreach (PXResult<ContractDetailExt, ContractItem> res in PXSelectReadonly2<ContractDetailExt, 
				InnerJoin<ContractItem, On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>,
				InnerJoin<Contract, On<Contract.contractID, Equal<ContractDetailExt.contractID>, And<Contract.lastActiveRevID, Equal<ContractDetailExt.revID>>>>>, 
				Where<ContractDetailExt.contractID, Equal<Required<ContractDetailExt.contractID>>>>.Select(this, contract.ContractID))
			{ 
				ContractDetailExt det = res;
				ContractDetail del = PXCache<ContractDetail>.CreateCopy(res);
				ContractItem item = res;

				det.Change = -det.Qty;

				details[det.LineNbr] = new PXResult<ContractDetail, ContractItem>(det, item);
				deleted[del.LineNbr] = new PXResult<ContractDetail, ContractItem>(del, item);
			}
            
            foreach (PXResult<ContractDetail, ContractItem> res in PXSelectReadonly2<ContractDetail,
				InnerJoin<ContractItem, On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>>,
				Where<ContractDetail.contractID, Equal<Required<Contract.contractID>>>>.Select(this, contract.ContractID))
            {
                ContractDetail det = res;
				ContractDetail del = PXCache<ContractDetail>.CreateCopy(res);
                ContractItem item = res;

				if (details.ContainsKey(det.LineNbr))
				{
					deleted.Remove(det.LineNbr);
				}

				ContractDetail detail = (ContractDetail)Caches[typeof(ContractDetail)].Locate(det);
				if (detail != null && item.Deposit == true)
				{
					detail.DepositAmt = detail.Qty * detail.BasePriceVal;
					Caches[typeof(ContractDetail)].Update(detail);
				}
				
				details[det.LineNbr] = res;

				if (det.Change < 0m)
				{
					del.Qty = del.LastQty;
					deleted[del.LineNbr] = new PXResult<ContractDetail, ContractItem>(del, item);
				}
            }

			GetUpgradeSetup(details.Values, list, prorate);

			prorate = 1;
			if (nextBillingDate != null && lastBillingDate != null)
			{
				decimal totalDays = nextBillingDate.Value.Date.Subtract(lastBillingDate.Value.Date).Days;
				decimal daysLeft = nextBillingDate.Value.Date.Subtract(activationDate.Value.Date).Days;
				prorate = totalDays == 0m ? 1m : daysLeft / totalDays;
			}

			GetUpgradeRecurring(details.Values, list, prorate);
			GetUpgradeUsage(deleted.Values, list, contract);

            return list;
        }

        protected virtual List<UsageData> GetTerminationFee(Contract contract, DateTime? lastBillingDate, DateTime? nextBillingDate, DateTime terminationDate)
        {
            List<UsageData> list = new List<UsageData>();

            Dictionary<int?, PXResult<ContractDetail, ContractItem>> details = new Dictionary<int?, PXResult<ContractDetail, ContractItem>>();
            Dictionary<int?, PXResult<ContractDetail, ContractItem>> deleted = new Dictionary<int?, PXResult<ContractDetail, ContractItem>>();

            foreach (PXResult<ContractDetail, ContractItem> res in PXSelectReadonly2<ContractDetail,
                InnerJoin<ContractItem, On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>>,
                Where<ContractDetail.contractID, Equal<Required<Contract.contractID>>>>.Select(this, contract.ContractID))
            {
                ContractDetail det = res;
                ContractDetail del = PXCache<ContractDetail>.CreateCopy(res);
                ContractItem item = res;
                det.Change = -det.Qty;

                details[det.LineNbr] = new PXResult<ContractDetail, ContractItem>(det, item);
                deleted[del.LineNbr] = new PXResult<ContractDetail, ContractItem>(del, item);

                ContractDetail detail = (ContractDetail)Caches[typeof(ContractDetail)].Locate(det);
                if (detail != null && item.Deposit == true)
                {
                    detail.DepositAmt = detail.Qty * detail.BasePriceVal;
                    Caches[typeof(ContractDetail)].Update(detail);
                }

                details[det.LineNbr] = res;

                if (det.Change < 0m)
                {
                    del.Qty = del.LastQty;
                    deleted[del.LineNbr] = new PXResult<ContractDetail, ContractItem>(del, item);
                }
            }

            MemorizeDeposits(details.Values);

            decimal setupProrate = 1;
            if (contract.ExpireDate != null)
            {
                DateTime? startDate = contract.ScheduleStartsOn == ScheduleStartOption.SetupDate ? contract.StartDate : contract.ActivationDate;
                decimal totalDays = contract.ExpireDate.Value.Date.Subtract(startDate.Value.Date).Days;
                decimal daysLeft = contract.ExpireDate.Value.Date.Subtract(terminationDate.Date).Days;
                setupProrate = daysLeft / totalDays;
            }

            Contract template = Contracts.SelectSingle(contract.TemplateID);
            bool isRefundAllowed = template.Refundable == true && contract.StartDate.Value.Date.AddDays(template.RefundPeriod.GetValueOrDefault()) >= terminationDate;
            GetUpgradeSetup(details.Values, isRefundAllowed ? list : null, setupProrate);

            decimal recurringProrate = 1;
            if (nextBillingDate != null && lastBillingDate != null)
            {
                decimal totalDays = nextBillingDate.Value.Date.Subtract(lastBillingDate.Value.Date).Days;
                decimal daysLeft = nextBillingDate.Value.Date.Subtract(terminationDate.Date).Days;
                recurringProrate = totalDays == 0m ? 1m : daysLeft / totalDays;
            }

            GetTerminateRecurring(details.Values, list, recurringProrate);

            GetUpgradeUsage(deleted.Values, list, contract);

            foreach (var usage in depositUsage.Values)
                list.Remove(usage);

            foreach (var usage in nonRefundableDepositedUsage)
                list.Remove(usage);

            return list;
        }

        protected virtual void MemorizeDeposits(IEnumerable<PXResult<ContractDetail, ContractItem>> details)
        {
            foreach (var detail in details)
            {
                ContractItem item = detail;
                if (item.BaseItemID != null && item.Deposit == true)
                {
                    if (item.Refundable == true)
                    {
                        refundableDeposits.Add(item.ContractItemID, item);
                    }
                    else
                    {
                        nonRefundableDeposits.Add(item.ContractItemID, item);
                    }
                }
            }
        }

        protected virtual List<UsageData> GetRenewalUsage(Contract contract)
        {
            
            List<UsageData> list = new List<UsageData>();

            PXSelectBase<ContractDetail> renewalItemsSelect = new
                     PXSelectJoin<ContractDetail,
                     InnerJoin<ContractItem, On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>,
                     InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<ContractItem.renewalItemID>>>>,
                     Where<ContractDetail.contractID, Equal<Required<Contract.contractID>>,
                     And<ContractItem.isRenewalValid, Equal<True>>>>(this);

            foreach (PXResult<ContractDetail, ContractItem, InventoryItem> res in renewalItemsSelect.Select(contract.ContractID))
            {
                ContractDetail det = res;
                ContractItem item = res;
                InventoryItem inventory = res;

                UsageData data = new UsageData();
                data.InventoryID = inventory.InventoryID;
                data.Description = inventory.Descr;
                data.UOM = inventory.BaseUnit;
                data.Qty = det.Qty;
                data.PriceOverride = det.RenewalPriceVal;
                data.ExtPrice = det.Qty * det.RenewalPriceVal;
                data.DiscountID = det.RenewalDiscountID;
                data.DiscountSeq = det.RenewalDiscountSeq;
                list.Add(data);
            }


            return list;
        }

        protected static decimal Prorate(DateTime? date, DateTime? startDate, DateTime? endDate)
        {
            var actual = date.Value.Date;
            var start = startDate.Value.Date;
            var end = endDate.Value.Date;

            decimal totalDays = end.Subtract(start).Days;
            decimal daysLeft = end.Subtract(actual).Days;
            return totalDays == 0.0m ? 1.0m : daysLeft / totalDays;
        }

        protected static decimal Prorate(DateTime? date, DatePair period)
        {
            return Prorate(date, period.Start, period.End);
        }

        protected virtual List<UsageData> GetPrepayment(Contract contract, DateTime? activationDate, DateTime? lastBillingDate, DateTime? nextBillingDate)
        {
            List<UsageData> list = new List<UsageData>();

			decimal prorate = 1;

            ContractBillingSchedule schedule = BillingSchedule.SelectSingle(contract.ContractID);
            if (schedule != null && schedule.Type == BillingType.Statement)
            {
                var statementDates = GetStatementBillDates(contract.CustomerID, activationDate);
                prorate = Prorate(activationDate, statementDates);
            }
			else if (nextBillingDate != null && contract.ScheduleStartsOn == ScheduleStartOption.SetupDate)
			{
                prorate = Prorate(activationDate, lastBillingDate, nextBillingDate);
			}

            if (contract.IsPendingUpdate == true)
            {
                return list;//do not collect prepayment. it will be collected on next billing.
            }

            foreach (PXResult<ContractDetail, ContractItem, InventoryItem> res in PXSelectJoin<ContractDetail,
                InnerJoin<ContractItem, On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>,
                InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<ContractItem.recurringItemID>>>>,
                Where<ContractDetail.contractID, Equal<Required<ContractDetail.contractID>>,
                     And<ContractItem.recurringType, Equal<RecurringOption.prepay>,
                     And<ContractItem.depositItemID, IsNull>>>>.Select(this, contract.ContractID))
            {
                ContractDetail det = res;
                ContractItem item = res;
                InventoryItem inventory = res;
                System.Diagnostics.Debug.WriteLine((item.RecurringType == RecurringOption.Prepay) + "\t" + (item.DepositItemID == null));


                decimal included = det.Qty ?? 0;
                string prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixPrepaid);
                
                UsageData data = new UsageData();
                data.InventoryID = item.RecurringItemID;
                data.Description = inventory.Descr;
                data.UOM = inventory.BaseUnit;
                data.Qty = included;
                data.PriceOverride = det.FixedRecurringPriceVal;
                data.ExtPrice = included * det.FixedRecurringPriceVal;
                data.Prefix = prefix;
                data.Proportion = prorate;
                data.DiscountID = det.RecurringDiscountID;
                data.DiscountSeq = det.RecurringDiscountSeq;
                list.Add(data);
            }
            
            return list;
        }

        protected virtual List<UsageData> GetRecurringBilling(IEnumerable<PXResult<ContractDetail, ContractItem>> details, Contract contract)
        {
            ContractBillingSchedule schedule = BillingSchedule.Select(contract.ContractID);

            List<UsageData> list = new List<UsageData>();

            decimal prepaidProrate = 1.0m;
            decimal postpaidProrate = 0.0m;

            if (schedule != null && schedule.Type == BillingType.Statement && contract.CustomerID != null)
            {
                var proportions = CalculateStatementBasedRecurringProportions(contract, schedule);
                prepaidProrate = proportions.Item1;
                postpaidProrate = proportions.Item2;
            }

			foreach(PXResult<ContractDetail, ContractItem> res in details)
			{
				ContractDetail det = res;
				ContractItem item = res;

				if (item.RecurringItemID != null && item.DepositItemID == null)
				{
					InventoryItem inventory = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, item.RecurringItemID);

					decimal included = 0;

					#region Add Post Payment Usage
					if (item.RecurringType == RecurringOption.Usage)
					{
						UsageData data = new UsageData();
						data.InventoryID = inventory.InventoryID;
						data.Description = inventory.Descr;
						data.UOM = inventory.BaseUnit;
						data.Qty = det.Qty;
						data.PriceOverride = det.FixedRecurringPriceVal;
                        data.Proportion = 1.0m - postpaidProrate;
						data.ExtPrice = det.Qty * det.FixedRecurringPriceVal;
						data.Prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixIncluded);
						data.DiscountID = det.RecurringDiscountID;
						data.DiscountSeq = det.RecurringDiscountSeq;
						list.Add(data);

						included = det.Qty ?? 0;
					}

					#endregion

					#region Add Pre Payment Usage
					if (item.RecurringType == RecurringOption.Prepay)
					{
						if (contract.IsCancelled != true && !IsLastBillBeforeExpiration(contract))
						{
							UsageData data = new UsageData();
							data.InventoryID = inventory.InventoryID;
							data.Description = inventory.Descr;
							data.UOM = inventory.BaseUnit;
							data.Qty = det.Qty;
							data.PriceOverride = det.FixedRecurringPriceVal;
                            data.Proportion = prepaidProrate;
							data.ExtPrice = det.Qty * det.FixedRecurringPriceVal;
							data.Prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixPrepaid);
							data.DiscountID = det.RecurringDiscountID;
							data.DiscountSeq = det.RecurringDiscountSeq;
							list.Add(data);
						}

						included = det.Qty ?? 0;
					}
					#endregion

					decimal avail;
					if (item.ResetUsageOnBilling == true)
					{
						avail = included;
					}
					else
					{
						decimal billedQty = det.UsedTotal.GetValueOrDefault() - det.Used.GetValueOrDefault();
						avail = included - billedQty;
					}
					availableQty.Add(item.RecurringItemID, Math.Max(0, avail));
				}

				if (item.BaseItemID != null && item.Deposit == true)
				{ 
					ContractDetail billing = res;
					InventoryItem inventory = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, item.BaseItemID);

					availableDeposit[billing.ContractItemID] = billing.DepositAmt - billing.DepositUsedTotal;

					UsageData tranData = new UsageData();
					tranData.InventoryID = inventory.InventoryID;
					tranData.Description = billing.Description;
					tranData.Qty = 0m;
					tranData.UOM = inventory.BaseUnit;
					tranData.IsTranData = false;
					tranData.PriceOverride = 0m;
					tranData.ExtPrice = 0m;
					tranData.IsFree = false;
					tranData.Prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixPrepaidUsage);
					tranData.IsDollarUsage = true;

					list.Add(tranData);
					depositUsage[billing.ContractItemID] = tranData;
				}
			}

            return list;
        }

        protected virtual Tuple<decimal, decimal> CalculateStatementBasedRecurringProportions(Contract contract, ContractBillingSchedule schedule)
        {
            decimal prepaidProrate = 1.0m;
            decimal postpaidProrate = 0.0m;

            bool isFirstRegularBill = IsFirstRegularBill(contract);
            bool isPrevToLastBill = IsPrevToLastBillBeforeExpiration(contract);
            bool isLastBill = IsLastBillBeforeExpiration(contract);

            if (isFirstRegularBill && contract.ActivationDate.HasValue)
            {
                var statementDates = GetStatementBillDates(contract.CustomerID, contract.ActivationDate);
                postpaidProrate = 1.0m - Prorate(contract.ActivationDate, statementDates);
            }

            if (isPrevToLastBill)
            {
                var statementDates = GetStatementBillDates(contract.CustomerID, schedule.NextDate);
                prepaidProrate = 1.0m - Prorate(contract.ExpireDate.Value.AddDays(1), statementDates);
            }

            if (isLastBill && contract.ExpireDate.HasValue)
            {
                var statementDates = GetStatementBillDates(contract.CustomerID, schedule.NextDate);
                postpaidProrate = Prorate(contract.ExpireDate.Value.AddDays(1), statementDates);
            }

            return new Tuple<decimal, decimal>(prepaidProrate, postpaidProrate);
        }

		protected virtual DateTime DateSetDay(DateTime date, int day)
		{
			int days = DateTime.DaysInMonth(date.Year, date.Month);
			if (date.Day < day && days > date.Day)
			{
				return (days >= day) ? new DateTime(date.Year, date.Month, day) : new DateTime(date.Year, date.Month, days);
			}
			return date;
		}
        
        protected virtual DateTime? GetNextBillingDate(string scheduleType, int? customerID, DateTime? date, DateTime? startDate)
        {
            if (date == null)
                return null;

            switch (scheduleType)
            {
                case BillingType.Annual:
					return DateSetDay(date.Value.AddYears(1), startDate.Value.Day);
                case BillingType.Monthly:
					return DateSetDay(date.Value.AddMonths(1), startDate.Value.Day);
                case BillingType.Weekly:
					return date.Value.AddDays(7);
                case BillingType.Quarterly:
					return DateSetDay(date.Value.AddMonths(3), startDate.Value.Day);
				case BillingType.OnDemand:
					return null;
                case BillingType.Statement:
                    return GetStatementBillDates(customerID, date).End;
                default:
                    throw new ArgumentException(String.Format("Invalid Schedule type ({0})", scheduleType ?? "null"), "scheduleType");
            }
        }

        public class DatePair : Tuple<DateTime?, DateTime?>
        {
            public DatePair(DateTime? last, DateTime? next) : base(last, next) { }

            public DateTime? Start { get { return Item1; } }
            public DateTime? End { get { return Item2; } }
        }

        protected virtual DatePair GetStatementBillDates(int? customerID, DateTime? dateInside)
        {
            if (dateInside == null)
                return new DatePair(null,null);
            DateTime date = dateInside.Value.AddDays(1);

            ARStatementCycle cycle = PXSelectJoin<ARStatementCycle,
                                        LeftJoin<Customer, On<ARStatementCycle.statementCycleId, Equal<Customer.statementCycleId>>>,
                                        Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.SelectWindowed(this, 0, 1, customerID);
            if (cycle == null)
            {
                throw new PXSetPropertyException(Messages.StatementCycleIsNull, PXErrorLevel.Error);
            }
            else
            {
                return new DatePair(
                    ARStatementProcess.CalcStatementDateBefore(date, cycle.PrepareOn, cycle.Day00, cycle.Day01),
                    ARStatementProcess.FindNextStatementDateAfter(date, cycle));
            }
        }

        protected virtual void SetBillingTarget(Contract contract, out Customer customer, out Location location)
        {
            customer = null;
            location = null;
            if (contract.CustomerID != null)
            {
                ContractBillingSchedule schedule = BillingSchedule.Select(contract.ContractID);

                if (schedule != null && schedule.AccountID != null)
                {
                    customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<ContractBillingSchedule.accountID>>>>.Select(this, schedule.AccountID);
                    if (schedule.LocationID != null)
                    {
                        location = PXSelect<Location, Where<Location.bAccountID, Equal<Required<ContractBillingSchedule.accountID>>, And<Location.locationID, Equal<Required<ContractBillingSchedule.locationID>>>>>.Select(this, customer.BAccountID, schedule.LocationID);
                    }
                    else
                    {
                        location = PXSelect<Location, Where<Location.locationID, Equal<Required<Customer.defLocationID>>>>.Select(this, customer.DefLocationID);
                    }
                }
                else
                {
                    customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, contract.CustomerID);
                    if (contract.LocationID != null)
                    {
                        location = PXSelect<Location, Where<Location.bAccountID, Equal<Required<ContractBillingSchedule.accountID>>, And<Location.locationID, Equal<Required<ContractBillingSchedule.locationID>>>>>.Select(this, customer.BAccountID, contract.LocationID);
                    }
                    else
                    {
                        location = PXSelect<Location, Where<Location.locationID, Equal<Required<Customer.defLocationID>>>>.Select(this, customer.DefLocationID);
                    }
                }
            }
        }

        public virtual void RecalcUsage(Contract contract, out List<UsageData> data, out Dictionary<int, List<TranNotePair>> sourceTran, out List<UsageData> tranData)
        {
            ContractBillingSchedule schedule = BillingSchedule.Select(contract.ContractID);
            data = new List<UsageData>();

            if (schedule != null && schedule.ChargeRenewalFeeOnNextBilling == true)
                data.AddRange(GetRenewalUsage(contract));

			Dictionary<int?, PXResult<ContractDetail, ContractItem>> details = new Dictionary<int?, PXResult<ContractDetail, ContractItem>>();

			foreach (PXResult<ContractDetailExt, ContractItem> res in PXSelectReadonly2<ContractDetailExt,
				InnerJoin<ContractItem, On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>,
				InnerJoin<Contract, On<Contract.contractID, Equal<ContractDetailExt.contractID>, And<Contract.lastActiveRevID, Equal<ContractDetailExt.revID>>>>>,
				Where<ContractDetailExt.contractID, Equal<Required<ContractDetailExt.contractID>>>>.Select(this, contract.ContractID))
			{
				ContractDetailExt det = res;
				ContractItem item = res;

				details[det.LineNbr] = new PXResult<ContractDetail, ContractItem>(det, item);
			}
            
            data.AddRange(GetRecurringBilling(details.Values, contract));

            sourceTran = GetTransactions(contract);
            tranData = ProcessTransactions(contract, sourceTran);
            data.AddRange(tranData);
        }

		public virtual decimal? RecalcDollarUsage(Contract contract)
		{
			List<UsageData> data;
			List<UsageData> tranData;
			Dictionary<int, List<TranNotePair>> sourceTran;

			RecalcUsage(contract, out data, out sourceTran, out tranData);

			decimal? dollarUsage = 0m;

			foreach (UsageData item in data)
			{
				if (item.IsDollarUsage == true)
				{
					if (item.IsTranData == true)
					{
						if (item.IsFree == true)
						{
							//
						}
						else if (item.PriceOverride != null)
						{
							dollarUsage += item.Qty * item.PriceOverride;
						}
					}
					else
					{
						dollarUsage += item.ExtPrice;
					}
				}
			}

			return dollarUsage;
		}

        protected virtual Dictionary<int, List<TranNotePair>> GetTransactions(Contract contract)
        {
            //Returns Transactions grouped by InvetoryID for the billing period (all transactions up to billing date) for customer base contract.
            ContractBillingSchedule schedule = BillingSchedule.Select(contract.ContractID);

            Dictionary<int, List<TranNotePair>> dict = new Dictionary<int, List<TranNotePair>>();
            PXResultset<PMTranEx> resultset = null;
            if (contract.IsCancelled == true || schedule == null || schedule.NextDate == null ||  IsLastBillBeforeExpiration(contract) )
            {
                resultset = PXSelectJoin<PMTranEx,
                       LeftJoin<Note, On<PMTranEx.origRefID, Equal<Note.noteID>>>,
                       Where<PMTranEx.projectID, Equal<Required<PMTranEx.projectID>>,
                       And<PMTranEx.billed, Equal<False>>>>.Select(this, contract.ContractID);
            }
            else
            {
                
                //To select all usage that fall under billing date we must select all usage that is less then billingdate + 1!
                DateTime includeDate = schedule.NextDate.Value.Date.AddDays(1);

                resultset = PXSelectJoin<PMTranEx,
                       LeftJoin<Note, On<PMTranEx.origRefID, Equal<Note.noteID>>>,
                       Where<PMTranEx.projectID, Equal<Required<PMTranEx.projectID>>,
                       And<PMTranEx.billed, Equal<False>,
                       And<PMTranEx.date, Less<Required<PMTranEx.date>>>>>>.Select(this, contract.ContractID, includeDate);
            }
            foreach (PXResult<PMTranEx, Note> res in resultset)
            {
                PMTranEx tran = (PMTranEx)res;
                Note note = (Note)res;

                if (dict.ContainsKey(tran.InventoryID.Value))
                {
                    dict[tran.InventoryID.Value].Add(new TranNotePair(tran, note));
                }
                else
                {
                    List<TranNotePair> list = new List<TranNotePair>();
                    list.Add(new TranNotePair(tran, note));
                    dict.Add(tran.InventoryID.Value, list);
                }
            }

            return dict;
        }

        protected virtual List<UsageData> ProcessTransactions(Contract contract, Dictionary<int, List<TranNotePair>> transactions)
        {
            List<UsageData> list = new List<UsageData>();

            foreach (KeyValuePair<int, List<TranNotePair>> kv in transactions)
            {
				ContractBillingSchedule schedule = BillingSchedule.Select(contract.ContractID);
                PXResult<ContractDetail> billing = null;
                if (contract.CustomerID != null)//for Virtual Contract Billing items are not applicable.
                {
                    billing = PXSelectJoin<ContractDetail,
                    InnerJoin<ContractItem, On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>,
                    InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<ContractItem.recurringItemID>>>>,
                    Where<ContractDetail.contractID, Equal<Required<Contract.contractID>>,
                    And<ContractItem.recurringItemID, Equal<Required<ContractDetail.inventoryID>>>>>.Select(this, contract.ContractID, kv.Key);
                }

                if (billing == null)
                {
                    InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, kv.Key);
                    billing = new PXResult<ContractDetail, ContractItem, InventoryItem>(null, null, item);
                }

                list.AddRange(ProcessTransactions(contract, (PXResult<ContractDetail, ContractItem, InventoryItem>)billing, kv.Value));

                foreach (TranNotePair tran in kv.Value)
                {
                    tran.Tran.Billed = true;
					tran.Tran.BilledDate = schedule.NextDate;
                    Transactions.Update(tran.Tran);//only Billed field is updated.
                }
            }

            return list;
        }

        protected virtual List<UsageData> ProcessTransactions(Contract contract, PXResult<ContractDetail, ContractItem, InventoryItem> res, List<TranNotePair> list)
        {
            ContractDetail billing = res;
            ContractItem item = res;
            InventoryItem inventory = res;

            List<UsageData> data = new List<UsageData>();

            if (list.Count > 1)
            {
                if (contract.DetailedBilling == Contract.detailedBilling.Detail)
                {
                    foreach (TranNotePair tran in list)
                    {
                        ProcessSingleRecord(contract, tran, res, data);
                    }

                }
                else
                {
                    //Note: When expense claims are linked to Contract. The cost/price is specified int the transaction, and can vary from one tran to another within the same
                    //contract. if billing or usagePrice is supplied we can take those; otherwise individual unitprice must be used.

                    List<TranNotePair> commonPrice = new List<TranNotePair>();
                    foreach (TranNotePair tran in list)
                    {
                        if (billing != null || tran.Tran.IsQtyOnly == true)
                            commonPrice.Add(tran);
                        else
                        {
                            ProcessSingleRecord(contract, tran, res, data);
                        }
                    }

                    ProcessSummaryUsageItems(contract, commonPrice, res, data);
                }
            }
            else if (list.Count == 1)
            {
                ProcessSingleRecord(contract, list[0], res, data);
            }

            return data;
        }

		protected virtual bool ProcessDollarUsage(Contract contract, TranNotePair tran, PXResult<ContractDetail, ContractItem, InventoryItem> res, List<UsageData> addedData)
		{
			return ProcessDollarUsage(contract, tran, tran.Tran.UOM, tran.Tran.BillableQty, tran.Tran.ResourceID, res, addedData); 
		}

		protected virtual bool ProcessDollarUsage(Contract contract, TranNotePair tran, string UOM, decimal? BillableQty, int? EmployeeID, PXResult<ContractDetail, ContractItem, InventoryItem> res, List<UsageData> addedData)
		{
			ContractDetail billing = res;
			ContractItem item = res;
			InventoryItem inventory = res;

			UsageData tranData = addedData[addedData.Count - 1];

			if (billing != null && item != null && item.DepositItemID != null)
			{
                if (nonRefundableDeposits.ContainsKey(item.DepositItemID))
                {
                    nonRefundableDepositedUsage.Add(tranData);
                }

				decimal? available;
				if (availableDeposit.TryGetValue(item.DepositItemID, out available))
				{
					decimal? baseQty = ConvertUsage(Transactions.Cache, tran.Tran.InventoryID, UOM, inventory.BaseUnit, BillableQty);
					decimal? amount = PXDBCurrencyAttribute.BaseRound(this, (decimal)(baseQty * billing.FixedRecurringPriceVal));
					if (available > 0m && amount <= available)
					{
						tranData.PriceOverride = billing.FixedRecurringPriceVal;
						tranData.IsFree = false;
						tranData.Prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixPrepaidUsage);
						tranData.IsDollarUsage = true;

						available -= amount;
					}
					else if (available > 0m && baseQty > 0m)
					{
						decimal? fixedBaseQty = IN.PXDBQuantityAttribute.Round(this, (decimal)(available / billing.FixedRecurringPriceVal));
						tranData.UOM = inventory.BaseUnit;
						tranData.Qty = fixedBaseQty;
						tranData.PriceOverride = billing.FixedRecurringPriceVal;
						tranData.IsFree = false;
						tranData.Prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixPrepaidUsage);
						tranData.IsDollarUsage = true;

						tranData = new UsageData();
						tranData.InventoryID = tran.Tran.InventoryID;
						tranData.Description = tran.Tran.Description;
						tranData.Qty = baseQty - fixedBaseQty;
						tranData.UOM = inventory.BaseUnit;
						tranData.EmployeeID = tran.Tran.ResourceID;
						tranData.IsTranData = true;
						tranData.PriceOverride = billing.UsagePriceVal;
						tranData.IsFree = false;
						tranData.Prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixOverused);
						tranData.BranchID = tran.Tran.BranchID;
						tranData.IsDollarUsage = true;

						addedData.Add(tranData);

						amount = PXDBCurrencyAttribute.BaseRound(this, (decimal)((baseQty - fixedBaseQty) * billing.UsagePriceVal));
						available = -amount;
					}
					else
					{
						tranData.PriceOverride = billing.UsagePriceVal;
						tranData.IsFree = false;
						tranData.Prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixOverused);
						tranData.IsDollarUsage = true;

						amount = PXDBCurrencyAttribute.BaseRound(this, (decimal)(baseQty * billing.UsagePriceVal));
						available -= amount;
					}

					ContractDetail deposit = (ContractDetail)ContractDetails.Cache.Locate(new ContractDetail { ContractID = contract.ContractID, ContractItemID = item.DepositItemID, RevID = contract.RevID});
					if (deposit != null)
					{
						//deposit.DepositUsed += PXDBCurrencyAttribute.BaseRound(this, (decimal)(availableDeposit[item.DepositItemID] - available));
						deposit.DepositUsedTotal += PXDBCurrencyAttribute.BaseRound(this, (decimal)(availableDeposit[item.DepositItemID] - available));
						ContractDetails.Update(deposit);

						if (availableDeposit[item.DepositItemID] > 0m)
						{
							if (depositUsage.TryGetValue(item.DepositItemID, out tranData))
							{
								tranData.ExtPrice -= (availableDeposit[item.DepositItemID] - (available > 0m ? available : 0m));
							}
						}

						availableDeposit[item.DepositItemID] = available;
					}
					return true;
				}
			}

			return false;
		}

        protected virtual void ProcessSingleRecord(Contract contract, TranNotePair tran, PXResult<ContractDetail, ContractItem, InventoryItem> res, List<UsageData> addedData)
        {
            ContractDetail billing = res;
            ContractItem item = res;
            InventoryItem inventory = res;

            //Prepayment and Postpayment items already added.
            //if used is greater then available then add 1 more line.

            UsageData tranData = new UsageData();
            tranData.TranIDs.Add(tran.Tran.TranID);
            tranData.InventoryID = tran.Tran.InventoryID;
            tranData.Description = tran.Tran.Description;
            tranData.Qty = tran.Tran.BillableQty;
            tranData.UOM = tran.Tran.UOM;
            tranData.EmployeeID = tran.Tran.ResourceID;
            tranData.IsTranData = true;
			tranData.BranchID = tran.Tran.BranchID;
			tranData.TranDate = tran.Tran.Date;

            /*
             CRM - records only quantities and no prices. UnitPrice=0.0
             EP (ExpenseClaims) - record unitprice also.
             */

			addedData.Add(tranData);

			if (ProcessDollarUsage(contract, tran, res, addedData))
			{
				return;
			}

            if (billing != null && item != null)
                tranData.PriceOverride = billing.UsagePriceVal;
			else
			{
				if (tran.Tran.IsQtyOnly != true)
					tranData.PriceOverride = tran.Tran.UnitRate;
				else
				{
					Location loc = PXSelect<Location, Where<Location.locationID, Equal<Required<Contract.locationID>>>>.Select(this, contract.LocationID);
					ARSalesPriceMaint.SalesPriceItem spItem = ARSalesPriceMaint.FindSalesPrice(this.ContractDetails.Cache, loc.CPriceClassID, tranData.InventoryID, contract.CuryID, tranData.UOM, tranData.TranDate.HasValue ? tranData.TranDate.Value : Accessinfo.BusinessDate.Value);
					if (spItem != null)
					{

						decimal salesPrice = spItem.Price;
						if (spItem.UOM != tranData.UOM)
					{
							decimal salesPriceInBase = INUnitAttribute.ConvertFromBase(this.ContractDetails.Cache, tranData.InventoryID, spItem.UOM, salesPrice, INPrecision.UNITCOST);
							salesPrice = INUnitAttribute.ConvertToBase(this.ContractDetails.Cache, tranData.InventoryID, tranData.UOM, salesPriceInBase, INPrecision.UNITCOST);
						}
						tranData.PriceOverride = salesPrice;
					}
					else
					{
						throw new PXException(Messages.SpecificItemNotPrice, inventory.InventoryCD);
					}
				}
			}

            if (billing != null)
            {
				//billing can be from previous active revision, relookup in cache for correct revision
				ContractDetail cached;
				if ((cached = ContractDetails.Locate(billing)) != null && !object.ReferenceEquals(cached, billing))
				{
					billing = cached;
				}

                decimal? available = availableQty[tran.Tran.InventoryID.Value];

                if (tran.Tran.BillableQty <= available)
                {
                    //Transaction is already payed for either ar prepayed incuded or a a post payment included. Thus it should be free.
                    tranData.IsFree = true;
                    tranData.Prefix = item.RecurringType == RecurringOption.Prepay ? PXMessages.LocalizeNoPrefix(Messages.PrefixPrepaidUsage) : PXMessages.LocalizeNoPrefix(Messages.PrefixIncludedUsage);

                    availableQty[tran.Tran.InventoryID.Value] -= tran.Tran.BillableQty.Value;//decrease available qty

                }
                else
                {
					if (available > 0m)
					{
						//Transaction is already payed for either ar prepayed incuded or a a post payment included. Thus it should be free.
						tranData.Qty = available;
						tranData.IsFree = true;
						tranData.Prefix = item.RecurringType == RecurringOption.Prepay ? PXMessages.LocalizeNoPrefix(Messages.PrefixPrepaidUsage) : PXMessages.LocalizeNoPrefix(Messages.PrefixIncludedUsage);

						tranData = new UsageData();
						tranData.InventoryID = tran.Tran.InventoryID;
						tranData.Description = tran.Tran.Description;
						tranData.UOM = inventory.BaseUnit;
						tranData.EmployeeID = tran.Tran.ResourceID;
						tranData.IsTranData = true;
						tranData.PriceOverride = billing.UsagePriceVal;
						tranData.BranchID = tran.Tran.BranchID;

						addedData.Add(tranData);
					}

					tranData.Qty = tran.Tran.BillableQty - available;
					tranData.IsFree = false;
					tranData.Prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixOverused);

                    availableQty[tran.Tran.InventoryID.Value] = 0;//all available qty was used.
                }

				billing.Used -= tran.Tran.BillableQty;

				ContractDetails.Update(billing);
            }
        }

        protected virtual void ProcessSummaryUsageItems(Contract contract, List<TranNotePair> trans, PXResult<ContractDetail, ContractItem, InventoryItem> res, List<UsageData> addedData)
        {
            ContractDetail billing = res;
            ContractItem item = res;
            InventoryItem inventory = res;

			string targetUOM = inventory.BaseUnit;
            decimal used = SumUsage(trans, targetUOM);

            if (used > 0)
            {
                UsageData tranData = new UsageData();

                int? lastEmployeeID = null;
				DateTime? lastDate = null;
                foreach (TranNotePair tnp in trans)
                {
                    tranData.TranIDs.Add(tnp.Tran.TranID.Value);

                    if (lastEmployeeID == null)
                    {
                        lastEmployeeID = tnp.Tran.ResourceID;
                    }
                    else if (tnp.Tran.ResourceID != null && lastEmployeeID != tnp.Tran.ResourceID)
                    {
						lastEmployeeID = null;
						break;
                    }

					if (lastDate == null)
					{
						lastDate = tnp.Tran.Date;
					}
					else if (tnp.Tran.Date != null && lastDate != tnp.Tran.Date)
					{
						lastDate = null;
						break;
					}
                }

				tranData.EmployeeID = lastEmployeeID;
                tranData.InventoryID = trans[0].Tran.InventoryID.Value;
				tranData.Description = billing != null ? billing.Description : null;
                tranData.Qty = used;
                tranData.UOM = targetUOM;
                tranData.IsTranData = true;
				tranData.TranDate = lastDate;

				addedData.Add(tranData);

				if (ProcessDollarUsage(contract, trans[0], targetUOM, used, lastEmployeeID, res, addedData))
				{
					return;
				}

				if (item != null)
					tranData.PriceOverride = billing.UsagePriceVal;
				else
				{
					Location loc = PXSelect<Location, Where<Location.locationID, Equal<Required<Contract.locationID>>>>.Select(this, contract.LocationID);
					ARSalesPriceMaint.SalesPriceItem spItem = ARSalesPriceMaint.FindSalesPrice(this.ContractDetails.Cache, loc.CPriceClassID, tranData.InventoryID, contract.CuryID, tranData.UOM, tranData.TranDate.HasValue ? tranData.TranDate.Value : Accessinfo.BusinessDate.Value);
					if (spItem != null)
					{
						tranData.PriceOverride = spItem.Price;
					}
					else
					{
						throw new PXException(Messages.SpecificItemNotPrice, inventory.InventoryCD);
					}
				}
                if (billing != null)
                {
                    tranData.Description = billing.Description;
                    decimal? available = availableQty[tranData.InventoryID];

                    if (used <= available)
                    {
                        //Transaction is already payed for either ar prepayed incuded or a a post payment included. Thus it should be free.
                        tranData.IsFree = true;
                        tranData.Prefix = item.RecurringType == RecurringOption.Prepay ? PXMessages.LocalizeNoPrefix(Messages.PrefixPrepaidUsage) : PXMessages.LocalizeNoPrefix(Messages.PrefixIncludedUsage);

                        availableQty[tranData.InventoryID] -= used;//decrease available qty
                    }
                    else
                    {
						if (available > 0m)
						{
							//Transaction is already payed for either ar prepayed incuded or a a post payment included. Thus it should be free.
							tranData.Qty = available;
							tranData.IsFree = true;
							tranData.Prefix = item.RecurringType == RecurringOption.Prepay ? PXMessages.LocalizeNoPrefix(Messages.PrefixPrepaidUsage) : PXMessages.LocalizeNoPrefix(Messages.PrefixIncludedUsage);

							tranData = new UsageData();
							tranData.InventoryID = trans[0].Tran.InventoryID;
							tranData.Description = billing.Description;
							tranData.UOM = targetUOM;
							tranData.EmployeeID = lastEmployeeID;
							tranData.IsTranData = true;
							tranData.PriceOverride = billing.UsagePriceVal;

							addedData.Add(tranData);
						}
						tranData.Qty = used - available;
						tranData.IsFree = false;
						tranData.Prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixOverused);

                        availableQty[tranData.InventoryID] = 0;//all available qty was used.
                    }

					billing.Used -= used;
					ContractDetails.Update(billing);
                }
            }
        }

        protected virtual decimal SumUsage(List<TranNotePair> trans, string targetUOM)
        {
            decimal used = 0;

            foreach (TranNotePair item in trans)
            {
                used += ConvertUsage(Transactions.Cache, item.Tran.InventoryID, item.Tran.UOM, targetUOM, item.Tran.BillableQty);
            }

            return used;
        }

        protected virtual decimal SumUnbilledAmt(Contract contract, List<UsageData> data)
        {
            Customer customer;
            Location location;
            SetBillingTarget(contract, out customer, out location);

            ARInvoiceEntry emulator = null;

            decimal total = 0;
            foreach (UsageData item in data)
            {
				if (item.IsTranData == null || item.IsTranData == true)
                {
                    decimal unitPrice = 0;
                    if (item.IsFree != true)
                    {
                        if (item.PriceOverride != null)
                        {
                            unitPrice = item.PriceOverride.Value;
                        }
                        else
                        {
                            //we don't know exact price untill we create the invoice so we'll try to emulate this fuctionality.
                            //ARInvoice will figure out sales price based on several settings.
                            if (emulator == null)
                            {
                                emulator = PXGraph.CreateInstance<ARInvoiceEntry>();
								emulator.ARSetup.Current.LineDiscountTarget = LineDiscountTargetType.ExtendedPrice;

                                emulator.Clear();
                                ARInvoice doc = emulator.Document.Insert();
                                doc.CustomerID = customer.BAccountID;
                                doc.CustomerLocationID = contract.LocationID ?? customer.DefLocationID;
                                doc.ProjectID = contract.ContractID;
                                emulator.Document.Update(doc);
                            }

                            ARTran tran = (ARTran)emulator.Transactions.Cache.CreateInstance();
                            tran.InventoryID = item.InventoryID;
                            tran.Qty = item.Qty;
                            tran.UOM = item.UOM;
                            tran.FreezeManualDisc = true;
                            tran.ManualDisc = true;
                            tran.DiscountID = item.DiscountID;
                            tran.DiscountSequenceID = item.DiscountSeq;

                            tran = (ARTran)emulator.Caches[typeof(ARTran)].Update(tran);

                            unitPrice = tran.UnitPrice ?? 0;
                        }
                    }

                    total += item.Qty.Value * unitPrice;
                }
                else
                {
					//TODO: Add precision handling
                    decimal extPrice = Math.Round((item.Proportion ?? 1) * item.ExtPrice.Value, 4, MidpointRounding.AwayFromZero); //used for proportional prepayment in statement-based contracts.
                    total += extPrice;
                }
            }

            return total;
        }

        protected virtual decimal ConvertUsage(PXCache sender, int? inventoryID, string fromUOM, string toUOM, decimal? value)
        {
            if (value == null)
                return 0;

            if (fromUOM == toUOM)
                return value.Value;

            decimal inBase = INUnitAttribute.ConvertToBase(sender, inventoryID, fromUOM, value.Value, INPrecision.QUANTITY);
            return INUnitAttribute.ConvertFromBase(sender, inventoryID, toUOM, inBase, INPrecision.QUANTITY);
        }

        protected virtual bool IsLastBillBeforeExpiration(Contract contract)
        {
			if (contract.ExpireDate == null)
				return false;

            ContractBillingSchedule schedule = BillingSchedule.Select(contract.ContractID);

            return schedule.NextDate.HasValue && schedule.NextDate.Value.Date >= contract.ExpireDate.Value.Date;
        }

        protected virtual bool IsPrevToLastBillBeforeExpiration(Contract contract)
        {
            if (contract.ExpireDate == null)
                return false;

            ContractBillingSchedule schedule = BillingSchedule.Select(contract.ContractID);

            if (schedule.NextDate.HasValue && schedule.NextDate.Value.Date < contract.ExpireDate.Value.Date)
            {
                var nextBilling = GetNextBillingDate(schedule.Type, contract.CustomerID, schedule.NextDate.Value.Date.AddDays(1), schedule.StartBilling);
                return nextBilling.HasValue && nextBilling.Value.Date >= contract.ExpireDate.Value.Date;
            }
            else
                return false;
        }

        protected virtual bool IsFirstRegularBill(Contract contract)
        {
            if (contract.ActivationDate == null)
                return false;

            ContractBillingSchedule schedule = BillingSchedule.Select(contract.ContractID);

            return schedule.LastDate == null || schedule.LastDate == contract.ActivationDate;
        }

        protected virtual void UpdateReferencePMTran2ARTran(PXGraph graph, Dictionary<int, List<TranNotePair>> sourceTran, List<UsageData> tranData)
        {
            Dictionary<long, PMTran> sourceTranDict = new Dictionary<long, PMTran>();
            foreach (List<TranNotePair> list in sourceTran.Values)
            {
                foreach (TranNotePair pair in list)
                {
                    sourceTranDict.Add(pair.Tran.TranID.Value, pair.Tran);
                }
            }

            foreach (UsageData ud in tranData)
            {
                foreach (long tranID in ud.TranIDs)
                {
                    PMTran source = sourceTranDict[tranID];
                    source.ARTranType = ((ARInvoice)graph.Caches[typeof(ARInvoice)].Current).DocType;
                    source.ARRefNbr = ((ARInvoice)graph.Caches[typeof(ARInvoice)].Current).RefNbr;
                    source.RefLineNbr = ud.RefLineNbr;
                    graph.Caches[typeof(PMTranEx)].Update(source);//Note: PMTranEx should be saved via InvoiceEntry so that reference is updated correctly.
                }
            }
        }

        protected virtual void AutoReleaseInvoice(Contract contract)
        {
            if (doclist.Count > 0 )
            {
                Contract template = Contracts.SelectSingle(contract.TemplateID);
                if (template.AutomaticReleaseAR == true)
                {
                    try
                    {
                        ARDocumentRelease.ReleaseDoc(doclist, false);
                    }
                    catch (Exception ex)
                    {
                        throw new PXException(PM.Messages.AutoReleaseARFailed, ex);
                    }
                }
            }
        }

		protected virtual void RaiseErrorIfUnreleasedUsageExist(Contract contract)
		{
			bool failed = false;

			StringBuilder sb = new StringBuilder();
			sb.AppendLine("List of unreleased billable cases:");

			PXSelectBase<CRCase> selectCases = new PXSelectJoin<CRCase,
				InnerJoin<CRCaseClass, On<CRCaseClass.caseClassID, Equal<CRCase.caseClassID>>>,
				Where<CRCase.contractID, Equal<Required<CRCase.contractID>>,
				And<CRCase.released, Equal<False>,
				And<CRCase.isBillable, Equal<True>,
				And<CRCaseClass.perItemBilling, Equal<BillingTypeListAttribute.perCase>>>>>>(this);
			foreach (CRCase c in selectCases.Select(contract.ContractID))
			{
				failed = true;
				sb.AppendLine(c.CaseCD);
			}

			sb.AppendLine("List of unreleased billable activities:");
			PXSelectBase<EPActivity> selectActivities = new PXSelectJoin<EPActivity,
				InnerJoin<CRCase, On<EPActivity.refNoteID, Equal<CRCase.noteID>>,
				InnerJoin<CRCaseClass, On<CRCaseClass.caseClassID, Equal<CRCase.caseClassID>>>>,
				Where<CRCase.contractID, Equal<Required<CRCase.contractID>>,
				And<CRCaseClass.perItemBilling, Equal<BillingTypeListAttribute.perActivity>,
				And<EPActivity.isBillable, Equal<True>,
				And<EPActivity.billed, Equal<False>>>>>>(this);

			foreach (PXResult<EPActivity, CRCase, CRCaseClass> res in selectActivities.Select(contract.ContractID))
			{
				failed = true;
				sb.AppendFormat("Case: {0} Activity: {1}{2}", ((CRCase)res).CaseCD, ((EPActivity)res).Subject, Environment.NewLine);
			}


			if (failed)
			{
				PXTrace.WriteInformation(sb.ToString());
				throw new PXException(Messages.UnreasedActivityExists);
			}
		}

        protected class InvoiceData
        {
            public DateTime InvoiceDate { get; private set; }
            public List<UsageData> UsageData { get; private set; }

            public InvoiceData(DateTime date)
            {
                this.InvoiceDate = date;
                this.UsageData = new List<UsageData>();

            }

            public string GetDocType()
            {
                decimal sum = 0m;
                foreach (UsageData data in UsageData)
                {
					if (data.IsTranData == null || data.IsTranData == true)
					{
						if (data.IsFree == true)
						{
							//
						}
						else if (data.PriceOverride != null)
						{
							//TODO: add precision handling
							sum += Math.Round((data.Qty * data.PriceOverride * data.Proportion.GetValueOrDefault(1.0m)) ?? 0m, 4, MidpointRounding.AwayFromZero);
						}
					}
					else
					{
						sum += data.ExtPrice ?? 0m;
					}
                }

                if (sum < 0m)
                {
                    return ARDocType.CreditMemo;
                }
                else
                {
                    return ARDocType.Invoice;
                }
            }
        }

        protected class Refund
        {
            public readonly decimal? Amount;
            public readonly PXResult<ContractDetail, ContractItem, InventoryItem> Item;
            public readonly bool IsRenew;

            public Refund(decimal? amount, PXResult<ContractDetail, ContractItem, InventoryItem> item, bool isRenew)
            {
                this.Amount = amount;
                this.Item = item;
                this.IsRenew = isRenew;
            }

        }


        [Serializable]
        public partial class PMTranEx : PMTran
        {
            #region TranID
            public new abstract class tranID : PX.Data.IBqlField
            {
            }
            #endregion
            #region ProjectID
            public new abstract class projectID : PX.Data.IBqlField
            {
            }
            #endregion
            #region Billed
            public new abstract class billed : PX.Data.IBqlField
            {
            }
            #endregion
            #region StartDate
            public new abstract class startDate : PX.Data.IBqlField
            {
            }
            #endregion
            #region Date
            public new abstract class date : PX.Data.IBqlField
            {
            }
            #endregion
            #region EndDate
            public new abstract class endDate : PX.Data.IBqlField
            {
            }
            #endregion
            #region OrigRefID
            public new abstract class origRefID : PX.Data.IBqlField
            {
            }
            #endregion
            #region ARTranType
            public new abstract class aRTranType : PX.Data.IBqlField
            {
            }
            [PXDBString(3, IsFixed = true)]
            [PXDBLiteDefault(typeof(ARTran.tranType), PersistingCheck = PXPersistingCheck.Nothing)]
            public override String ARTranType
            {
                get
                {
                    return this._ARTranType;
                }
                set
                {
                    this._ARTranType = value;
                }
            }
            #endregion
            #region ARRefNbr
            public new abstract class aRRefNbr : PX.Data.IBqlField
            {
            }
            [PXDBString(15, IsUnicode = true)]
            [PXUIField(DisplayName = "AR Reference Nbr.")]
            [PXDBLiteDefault(typeof(ARTran.refNbr), PersistingCheck = PXPersistingCheck.Nothing)]
            public override String ARRefNbr
            {
                get
                {
                    return this._ARRefNbr;
                }
                set
                {
                    this._ARRefNbr = value;
                }
            }
            #endregion
            #region RefLineNbr
            public new abstract class refLineNbr : PX.Data.IBqlField
            {
            }
            [PXDBInt()]
            [PXDBLiteDefault(typeof(ARTran.lineNbr), PersistingCheck = PXPersistingCheck.Nothing)]
            public override Int32? RefLineNbr
            {
                get
                {
                    return this._RefLineNbr;
                }
                set
                {
                    this._RefLineNbr = value;
                }
            }
            #endregion

        }
        
        public partial class InventoryItemEx : InventoryItem
        {
            public new abstract class inventoryID : IBqlField { }
        }
    }
}
