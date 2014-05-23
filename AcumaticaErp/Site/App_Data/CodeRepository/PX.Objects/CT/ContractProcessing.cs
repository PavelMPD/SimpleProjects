using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.SO;

namespace PX.Objects.CT
{
    public class ContractProcessing
    {
        protected ARInvoiceEntry graph;
        protected Contract contract;
        protected Contract template;
        protected ContractBillingSchedule schedule;
        protected Customer customer;
        protected INSetup insetup;
        protected Location location;

        protected ContractProcessing() { }

        protected ContractProcessing(int? contractID)
        {
            if (contractID > 0)
            {
                this.graph = PXGraph.CreateInstance<ARInvoiceEntry>();
                graph.FieldVerifying.AddHandler<ARInvoice.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
                this.contract = PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(graph, contractID);

                this.insetup = PXSelect<INSetup>.Select(graph);
                this.template = PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(graph, contract.TemplateID);
                this.schedule = PXSelect<ContractBillingSchedule>.Search<ContractBillingSchedule.contractID>(graph, contract.ContractID);
                if (contract.CustomerID != null)
                {
                    if (schedule != null && schedule.AccountID != null)
                    {
                        customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<ContractBillingSchedule.accountID>>>>.Select(graph, schedule.AccountID);
                        if (schedule.LocationID != null)
                        {
                            location = PXSelect<Location, Where<Location.bAccountID, Equal<Required<ContractBillingSchedule.accountID>>, And<Location.locationID, Equal<Required<ContractBillingSchedule.locationID>>>>>.Select(graph, customer.BAccountID, schedule.LocationID);
                        }
                        else
                        {
                            location = PXSelect<Location, Where<Location.locationID, Equal<Required<Customer.defLocationID>>>>.Select(graph, customer.DefLocationID);
                        }
                    }
                    else
                    {
                        customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(graph, contract.CustomerID);
                        if (contract.LocationID != null)
                        {
                            location = PXSelect<Location, Where<Location.bAccountID, Equal<Required<ContractBillingSchedule.accountID>>, And<Location.locationID, Equal<Required<ContractBillingSchedule.locationID>>>>>.Select(graph, customer.BAccountID, contract.LocationID);
                        }
                        else
                        {
                            location = PXSelect<Location, Where<Location.locationID, Equal<Required<Customer.defLocationID>>>>.Select(graph, customer.DefLocationID);
                        }
                    }
                }

                SetupGraph();
            }
        }

		protected int DecPlQty
		{
			get
			{
				int decimals = 4;
				if (insetup != null && insetup.DecPlQty != null)
				{
					decimals = Convert.ToInt32(insetup.DecPlQty);
				}

				return decimals;
			}
		}

		protected int DecPlPrcCst
		{
			get
			{
				int decimals = 2;
				if (insetup != null && insetup.DecPlPrcCst != null)
				{
					decimals = Convert.ToInt32(insetup.DecPlPrcCst);
				}

				return decimals;
			}
		}
		      
        protected virtual void SetupGraph()
        {
            graph.Clear();
            graph.Views.Caches.Add(typeof(Contract));
            graph.Views.Caches.Add(typeof(ContractDetail));
            graph.Views.Caches.Add(typeof(ContractBillingSchedule));
            
            PXCache<ContractBillingSchedule> scheduleCache = (PXCache<ContractBillingSchedule>)graph.Caches[typeof(ContractBillingSchedule)];
            PXDBDefaultAttribute.SetDefaultForUpdate<ContractBillingSchedule.contractID>(scheduleCache, null, false);

            PXCache<ContractDetail> contractItemCache = (PXCache<ContractDetail>)graph.Caches[typeof(ContractDetail)];
            PXDBDefaultAttribute.SetDefaultForUpdate<ContractDetail.contractID>(contractItemCache, null, false);
        }
    }

    public class TranNotePair
    {
        public PMTran Tran { get; private set; }
        public Note Note { get; private set; }

        public TranNotePair(PMTran tran, Note note)
        {
            this.Tran = tran;
            this.Note = note;
        }
    }
}
