using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Linq;
using PX.Data;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.CR;
using PX.Objects.AR;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.SO;

namespace PX.Objects.CT
{
	public class ContractMaint : PXGraph<ContractMaint, Contract>
    {
		#region DAC Overrides
		[PXDBInt]
		[PXDefault()]
		[PXDimensionSelector(ContractItemAttribute.DimensionName, typeof(Search<ContractItem.contractItemID>),
																	typeof(ContractItem.contractItemCD),
																	typeof(ContractItem.contractItemCD), typeof(ContractItem.descr))]
		[PXUIField(DisplayName = "Item Code")]
		[PXRestrictor(typeof(Where<ContractItem.curyID, Equal<Current<Contract.curyID>>>), Messages.ItemHasAnotherCuryID)]
		protected virtual void ContractDetail_ContractItemID_CacheAttached(PXCache sender) { }

        [PXString(1, IsFixed = true)]
        [RecurringOption.ListForDeposits]
        [PXDefault(RecurringOption.None)]
        [PXUIField(DisplayName = "Billing Type")]
        [PXFormula(typeof(Switch<Case<Where<ContractItem.deposit, Equal<True>>, RecurringOption.deposits>, ContractItem.recurringType>))]
        protected virtual void ContractItem_RecurringTypeForDeposits_CacheAttached(PXCache sender) { }       
        
        [PXUIField(DisplayName = "UOM")]
        [PXString(10, IsFixed = true)]
        [PXFormula(typeof(Switch<
            Case<Where<ContractItem.deposit, Equal<True>>, ContractItem.curyID>,
                     Selector<ContractItem.recurringItemID, InventoryItem.salesUnit>>))]
        protected virtual void ContractItem_UOMForDeposits_CacheAttached(PXCache sender) { } 
        
        [PXBool()]
        [PXFormula(typeof(Switch<Case<Where<ContractDetail.deposit, Equal<True>>,Switch<
                          Case<Where<Div<Mult<ContractDetail.recurringIncluded, Selector<ContractDetail.contractItemID, ContractItem.retainRate>>, decimal100>, 
                          Greater<Sub<ContractDetail.recurringIncluded, ContractDetail.recurringUsedTotal>>>,
                          True>,False>>,False>))]
        protected virtual void ContractDetail_WarningAmountForDeposit_CacheAttached(PXCache sender) { }
        #endregion

        #region Selects/Views
        public PXSelect<Contract, Where<Contract.baseType, Equal<Contract.ContractBaseType>, And<Contract.isTemplate, Equal<False>>>> Contracts;
		public PXSelect<Contract, Where<Contract.contractID, Equal<Current<Contract.contractID>>>> CurrentContract;
        [PXCopyPasteHiddenFields(typeof(ContractBillingSchedule.lastDate), typeof(ContractBillingSchedule.nextDate), typeof(ContractBillingSchedule.chargeRenewalFeeOnNextBilling), typeof(ContractBillingSchedule.startBilling))]
        public PXSelect<ContractBillingSchedule, Where<ContractBillingSchedule.contractID, Equal<Current<Contract.contractID>>>> Billing;
        
		public PXSelect<ContractSLAMapping, Where<ContractSLAMapping.contractID, Equal<Current<Contract.contractID>>>> SLAMapping;
		public PXSelectJoin<ContractDetail, LeftJoin<ContractItem, On<ContractDetail.contractItemID, Equal<ContractItem.contractItemID>>>, Where<ContractDetail.contractID, Equal<Current<Contract.contractID>>>> ContractDetails;
		public PXSelectJoin<ContractDetail,
                InnerJoin<ContractItem, On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>,
                InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<ContractItem.recurringItemID>, Or<InventoryItem.inventoryID, Equal<ContractItem.baseItemID>, And<ContractItem.deposit, Equal<True>>>>>>,
                Where<ContractDetail.contractID, Equal<Current<Contract.contractID>>>> RecurringDetails;

        public PXSelect<ContractItem> RecurringDetailsContractItem;
		public PXSetup<Location, Where<Location.bAccountID, Equal<Optional<ContractBillingSchedule.accountID>>, And<Location.locationID, Equal<Optional<ContractBillingSchedule.locationID>>>>> BillingLocation;

		public PXSelect<SelContractWatcher, Where<SelContractWatcher.contractID, Equal<Current<Contract.contractID>>>> Watchers;
		[PXCopyPasteHiddenView]
		public PXSelect<ARInvoice, Where<ARInvoice.projectID, Equal<Current<Contract.contractID>>>> Invoices;
       [PXCopyPasteHiddenView]
		public PXSelect<ContractRenewalHistory, Where<ContractRenewalHistory.contractID, Equal<Current<Contract.contractID>>>> RenewalHistory;
		public PXSetup<Company> Company;
		public PXSetup<ContractTemplate, Where<ContractTemplate.contractID, Equal<Current<Contract.templateID>>>> CurrentTemplate;
        [PXCopyPasteHiddenView]
		public PXFilter<ActivationSettingsFilter> ActivationSettings;
        [PXCopyPasteHiddenView]
        public PXFilter<TerminationSettingsFilter> TerminationSettings;
		[PXCopyPasteHiddenView]
		public PXFilter<BillingOnDemandSettingsFilter> OnDemandSettings;
        public CRAttributeList<Contract> Answers;
		public PXSelectJoin<EPContractRate
				, LeftJoin<EPEmployee, On<EPContractRate.employeeID, Equal<EPEmployee.bAccountID>>
					, LeftJoin<EPEarningType, On<EPEarningType.typeCD, Equal<EPContractRate.earningType>>>
					>
				, Where<EPContractRate.contractID, Equal<Current<Contract.contractID>>>
				, OrderBy<Asc<EPContractRate.earningType, Asc<EPContractRate.employeeID>>>
				> ContractRates;

		#endregion        

		public ContractMaint()
		{
            this.CopyPaste.SetVisible(false);
			PXUIFieldAttribute.SetDisplayName(Caches[typeof(Contact)], typeof(Contact.salutation).Name, CR.Messages.Attention);
			PXUIFieldAttribute.SetDisplayName<ContractDetail.used>(RecurringDetails.Cache, "Used");
			PXUIFieldAttribute.SetDisplayName<ContractDetail.qty>(RecurringDetails.Cache, "Included");

			FieldDefaulting.AddHandler<BAccountR.type>((sender, e) => { if (e.Row != null) e.NewValue = BAccountType.CustomerType; });
			PXDefaultAttribute.SetPersistingCheck<ContractBillingSchedule.billTo>(Billing.Cache, null, PXPersistingCheck.Null);
			action.AddMenuAction(ChangeID);
		}
		#region Actions

		public PXAction<Contract> action;
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual IEnumerable Action(PXAdapter adapter,
			[PXString()]
			string ActionName
			)
		{
			if (!string.IsNullOrEmpty(ActionName))
			{
				PXAction action = this.Actions[ActionName];

				if (action != null)
				{
					Save.Press();
					List<object> result = new List<object>();
					foreach (object data in action.Press(adapter))
					{
						result.Add(data);
					}
					return result;
				}
			}
			return adapter.Get();
		}

		public PXAction<Contract> inquiry;
		[PXUIField(DisplayName = "Inquiries", MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual IEnumerable Inquiry(PXAdapter adapter,
			[PXString()]
			string ActionName
			)
		{
			if (!string.IsNullOrEmpty(ActionName))
			{
				PXAction action = this.Actions[ActionName];

				if (action != null)
				{
					Save.Press();
					foreach (object data in action.Press(adapter)) ;
				}
			}
			return adapter.Get();
		}


		public PXAction<Contract> viewUsage;
		[PXUIField(DisplayName = Messages.ViewUsage, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Inquiry)]
		public virtual IEnumerable ViewUsage(PXAdapter adapter)
		{
			if (Contracts.Current != null && Contracts.Cache.GetStatus(Contracts.Current) != PXEntryStatus.Inserted)
			{
				UsageMaint target = PXGraph.CreateInstance<UsageMaint>();
				target.Clear();
				target.Filter.Current.ContractID = Contracts.Current.ContractID;

				throw new PXRedirectRequiredException(target, Messages.ViewUsage);
			}

			return adapter.Get();
		}

		public PXAction<Contract> showContact;
		[PXUIField(DisplayName = "Show Contact", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ShowContact(PXAdapter adapter)
		{
			SelContractWatcher watcher = this.Watchers.Current;
			if (watcher != null && watcher.ContactID != null)
			{
				ContactMaint graph = PXGraph.CreateInstance<ContactMaint>();
				graph.Clear();
				Contact contact = graph.Contact.Search<Contact.contactID>(watcher.ContactID);
				if (contact != null)
				{
					graph.Contact.Current = contact;
					throw new PXRedirectRequiredException(graph, PX.Objects.CR.Messages.ContactMaint);
				}
			}
			return adapter.Get();
		}

		public PXAction<Contract> viewInvoice;
		[PXUIField(DisplayName = Messages.ViewInvoice, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Inquiry)]
		public virtual IEnumerable ViewInvoice(PXAdapter adapter)
		{
			if (Invoices.Current != null)
			{
				ARInvoiceEntry target = PXGraph.CreateInstance<ARInvoiceEntry>();
				target.Clear();
				target.Document.Current = Invoices.Current;
				throw new PXRedirectRequiredException(target, true, "ViewInvoice"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
			}

			return adapter.Get();
		}

		public PXAction<Contract> renew;
		[PXUIField(DisplayName = "Renew Contract", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable Renew(PXAdapter adapter)
		{
			RenewContract(true);

			return adapter.Get();
		}

		public virtual void RenewContract()
		{
			RenewContract(false);
		}

		private void RenewContract(bool redirect)
		{
			this.CurrentContract.Current.IsLastActionUndoable = true;
			this.CurrentContract.Update(this.CurrentContract.Current);
			this.Save.Press();
            if (Contracts.Current != null)
            {
				if (Contracts.Current.Type == ContractType.Expiring || (Contracts.Current.Type == ContractType.Renewable && IsExpired(Contracts.Current, Accessinfo.BusinessDate.Value)))
                {
                    ContractMaint target = PXGraph.CreateInstance<ContractMaint>();
                    target.Clear();
                    RenewExpiring(target);
					target.Save.Press();

                    CreateExpiringRenewalHistory(target.Contracts.Current);

					target.Save.Press();
					this.Save.Press();
                    if (redirect)
                        throw new PXRedirectRequiredException(target, "Navigate to New Contract");
                }
                else if (Contracts.Current.Type == ContractType.Renewable)
                {
                    CTBillEngine engine = PXGraph.CreateInstance<CTBillEngine>();
                    engine.Renew(Contracts.Current.ContractID);

                    this.Clear();//ContractBillingSchedule and Contract is changed by the engine.Renew()
                }
            }
		}

        private void CreateExpiringRenewalHistory(Contract child)
        {
            if (child.OriginalContractID != null)
            {
				Contract parent = PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.originalContractID>>>>.Select(this, child.OriginalContractID);

				parent.LastActiveRevID = parent.RevID;
				var history = new ContractRenewalHistory
                {
                    ContractID = parent.ContractID,
                    ChildContractID = child.ContractID,
                    Status = parent.Status,
                    Action = ContractAction.Renew,
                    RenewalDate = child.CreatedDateTime,
                    RenewedBy = PXAccess.GetUserID(),
					RevID=++parent.RevID
                };

				this.RenewalHistory.Insert(history);
				Contracts.Update(parent);
            }
        }

		public PXAction<Contract> viewContract;
		[PXUIField(DisplayName = Messages.ViewContract, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewContract(PXAdapter adapter)
		{
			if (RenewalHistory.Current != null && RenewalHistory.Current.ChildContractID != null)
			{
				ContractMaint target = PXGraph.CreateInstance<ContractMaint>();
				target.Clear();
                target.Contracts.Current = PXSelect<Contract, Where<Contract.contractID, Equal<Current<ContractRenewalHistory.childContractID>>>>.Select(this);
                throw new PXRedirectRequiredException(target, "ViewContract"){Mode = PXBaseRedirectException.WindowMode.NewWindow};              
			}

			return adapter.Get();
		}          

		public PXAction<Contract> bill;
		[PXUIField(DisplayName = Messages.Bill, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual void Bill()
		{
			CTBillEngine engine = PXGraph.CreateInstance<CTBillEngine>();
			Billing.Current = Billing.Select();
			if (Billing.Current.Type == BillingType.OnDemand)
			{
				if (OnDemandSettings.AskExt() == WebDialogResult.OK)
				{
					PXLongOperation.StartOperation(this, delegate()
					{
						engine.Bill(Contracts.Current.ContractID, OnDemandSettings.Current.BillingDate);
					});

				}
			}
			else
			{
				PXLongOperation.StartOperation(this, delegate()
				{
					engine.Bill(Contracts.Current.ContractID);
				});
			}

		}

		public virtual void fillFilter(PXGraph aGraph, string ViewName)
		{
			ActivationSettings.Cache.SetDefaultExt<ActivationSettingsFilter.activationDate>(ActivationSettings.Current);
			ActivationSettings.Cache.SetDefaultExt<ActivationSettingsFilter.startDate>(ActivationSettings.Current);
		}

		public PXAction<Contract> setup;
		[PXUIField(DisplayName = Messages.SetupContract, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXLookupButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable Setup(PXAdapter adapter)
		{
			this.Save.Press();
			if (Contracts.Current != null)
			{
				ActivationSettings.Current.CalledAction = ContractAction.Setup;
				if (ActivationSettings.AskExt(fillFilter) == WebDialogResult.OK)
				{
					if (Contracts.Current.StartDate != ActivationSettings.Current.StartDate)
					{
						Contracts.Current.StartDate = ActivationSettings.Current.StartDate;
						
					}
					if (Contracts.Current.ScheduleStartsOn==ScheduleStartOption.SetupDate)
						SetExpireDate(Contracts.Current);
					Contracts.Update(Contracts.Current);
					this.Save.Press();
				}
				PXLongOperation.StartOperation(this, delegate()
				{
					CTBillEngine engine = PXGraph.CreateInstance<CTBillEngine>();
					engine.Setup(Contracts.Current.ContractID, ActivationSettings.Current.StartDate);
				});

				return new object[] { Contracts.Current };
			}

			return adapter.Get();
		}

		public PXAction<Contract> activate;
		[PXUIField(DisplayName = Messages.ActivateContract, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXLookupButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
        public virtual IEnumerable Activate(PXAdapter adapter)
        {
            if (Contracts.Current != null)
            {
				PXDefaultAttribute.SetPersistingCheck<Contract.activationDate>(CurrentContract.Cache, CurrentContract.Current, PXPersistingCheck.NullOrBlank);
				ActivationSettings.Current.CalledAction = ContractAction.Activate;
				if (ActivationSettings.AskExt(fillFilter) == WebDialogResult.OK)
                {
					if (Contracts.Current.ActivationDate != ActivationSettings.Current.ActivationDate)
                    {
						Contracts.Current.ActivationDate = ActivationSettings.Current.ActivationDate;
                    }
					SetExpireDate(Contracts.Current);
					Contracts.Update(Contracts.Current);
                    this.Save.Press();
                }
                PXLongOperation.StartOperation(this, delegate()
                {
                    CTBillEngine engine = PXGraph.CreateInstance<CTBillEngine>();
                    engine.Activate(Contracts.Current.ContractID, ActivationSettings.Current.ActivationDate);
                });

                return new object[] { Contracts.Current };
            }

            return adapter.Get();
        }

		public PXAction<Contract> setupAndActivate;
		[PXUIField(DisplayName = Messages.SetupAndActivateContract, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXLookupButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
        public virtual IEnumerable SetupAndActivate(PXAdapter adapter)
        {
            if (Contracts.Current != null)
            {
				PXDefaultAttribute.SetPersistingCheck<Contract.activationDate>(CurrentContract.Cache, CurrentContract.Current, PXPersistingCheck.NullOrBlank);
				ActivationSettings.Current.CalledAction = ContractAction.SetupAndActivate;
				if (ActivationSettings.AskExt(fillFilter) == WebDialogResult.OK)
				{
					if (Contracts.Current.ActivationDate != ActivationSettings.Current.ActivationDate)
                    {
						Contracts.Current.ActivationDate = ActivationSettings.Current.ActivationDate;
					}
                    Contracts.Current.StartDate = ActivationSettings.Current.ActivationDate;

					SetExpireDate(Contracts.Current);
					Contracts.Update(Contracts.Current);
                    this.Save.Press();
                }
                PXLongOperation.StartOperation(this, delegate()
                {
                    CTBillEngine engine = PXGraph.CreateInstance<CTBillEngine>();
                    engine.SetupAndActivate(Contracts.Current.ContractID, ActivationSettings.Current.ActivationDate);
                });

                return new object[] { Contracts.Current };
            }

            return adapter.Get();
        }

		public PXAction<Contract> terminate;
		[PXUIField(DisplayName = Messages.Terminate, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual void Terminate()
		{
			if (Contracts.Current != null)
			{
				if (Contracts.Current.CustomerID != null)
				{
                    if (TerminationSettings.AskExt() == WebDialogResult.OK)
                    {
                        
                        PXLongOperation.StartOperation(this, delegate()
                        {
                            CTBillEngine engine = PXGraph.CreateInstance<CTBillEngine>();
                            engine.Terminate(Contracts.Current.ContractID, TerminationSettings.Current.TerminationDate);

                        });
                    }
				}
				else
				{
                    throw new PXException(Messages.VirtualContractCannotBeTerminated);
					//virtual contract
				}
				
			}
		}

        public PXAction<Contract> upgrade;
        [PXUIField(DisplayName = Messages.Upgrade, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
        [PXLookupButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
        public virtual void Upgrade()
        {
			this.Save.Press();
            if (Contracts.Current != null)
            {
				PXLongOperation.StartOperation(this, delegate()
				{
					CTBillEngine engine = PXGraph.CreateInstance<CTBillEngine>();
					engine.Upgrade(Contracts.Current.ContractID);
				});
            }
        }

		public PXAction<Contract> activateUpgrade;
		[PXUIField(DisplayName = Messages.ActivateUpgrade, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXLookupButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ActivateUpgrade(PXAdapter adapter)
		{
			this.Save.Press();
			if (Contracts.Current != null)
			{
				if (ActivationSettings.AskExt() == WebDialogResult.OK)
				{
					PXLongOperation.StartOperation(this, delegate()
					{
						CTBillEngine engine = PXGraph.CreateInstance<CTBillEngine>();
						engine.Activate(Contracts.Current.ContractID, ActivationSettings.Current.ActivationDate);
					});
				}

				return new object[] { Contracts.Current };
			}

			return adapter.Get();
		}

        public PXAction<Contract> undoBilling;
        [PXUIField(DisplayName = Messages.UndoBilling, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
        [PXLookupButton(ImageKey = PX.Web.UI.Sprite.Main.Process, Tooltip = Messages.UndoBillingTooltip)]
        public virtual void UndoBilling()
        {
            if (Contracts.Current == null)
                return;
            
            if(Contracts.Current.IsLastActionUndoable != true)
                throw new PXException(Messages.CannotUndoAction);

            PXLongOperation.StartOperation(this, delegate()
                {
                    CTBillEngine engine = PXGraph.CreateInstance<CTBillEngine>();
                    engine.UndoBilling(Contracts.Current.ContractID);
                });
        }

		public PXChangeID<Contract, Contract.contractCD> ChangeID;

		#endregion

		public ContractBillingSchedule contractBillingSchedule
		{
			get
			{
				return this.Billing.Select();
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

        #region Event Handlers

		#region Contract Event Handlers

		protected virtual void Contract_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Contract row = e.Row as Contract;
			if (row != null)
			{
				ContractBillingSchedule schedule = new ContractBillingSchedule();
				schedule.ContractID = row.ContractID;
				Billing.Insert(schedule);

				PXUIFieldAttribute.SetRequired<ContractBillingSchedule.nextDate>(sender, true);

				PXStringState state = SLAMapping.Cache.GetStateExt<ContractSLAMapping.severity>(null) as PXStringState;
				if (state != null && state.AllowedValues != null && state.AllowedValues.Length > 0)
				{
					foreach (string severity in state.AllowedValues)
					{
						ContractSLAMapping sla = new ContractSLAMapping();
						sla.ContractID = row.ContractID;
						sla.Severity = severity;
						SLAMapping.Insert(sla);
					}
				}

				ContractRenewalHistory history = new ContractRenewalHistory();
				history.RenewalDate = row.LastModifiedDateTime;
                history.RenewedBy = PXAccess.GetUserID();
				history.Status = row.Status;
				history.Action = ContractAction.Create;
				history.ChildContractID = row.OriginalContractID;

                CTBillEngine.UpdateContractHistoryEntry(history, row, schedule);

				RenewalHistory.Insert(history);

				Billing.Cache.IsDirty = false;
				SLAMapping.Cache.IsDirty = false;
				RenewalHistory.Cache.IsDirty = false;
			}
		}

		protected virtual void Contract_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Contract row = e.Row as Contract;
            if (row != null)
            {
                SetControlsState(row, sender);
				CalcDetail(row);

				if (!(row.TotalsCalculated == 1))
				{
					CalcSummary(sender, row);
					if (row.TotalUsage != null)
					{
						row.TotalsCalculated = 1;
					}
				}
            }

            undoBilling.SetEnabled(row.IsLastActionUndoable == true);
			renew.SetEnabled(row.Type != ContractType.Unlimited);
		}
        
		protected virtual void Contract_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			Contract row = e.Row as Contract;
			Contract oldRow = e.OldRow as Contract;
			if (row != null)
			{
				if (row.StartDate == null || row.ExpireDate == null)
					return;

				if (row.StartDate.Value > row.ExpireDate.Value)
				{
					if (row.ExpireDate.Value != oldRow.ExpireDate.Value)
					{
						sender.RaiseExceptionHandling<Contract.expireDate>(row, row.ExpireDate, new PXSetPropertyException(Messages.InvalidDate));
						return;
					}
					sender.RaiseExceptionHandling<Contract.startDate>(row, row.StartDate, new PXSetPropertyException(Messages.InvalidDate));
				}
			}
		}

		protected virtual void Contract_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(PM.PMTran.projectID));
			PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(CRCase.contractID));
			PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(ARTran.projectID));
			}

		protected virtual void Contract_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			Contract row = e.Row as Contract;
			if (row != null)
			{

				if (row.CustomerID == null)
				{

					if (sender.RaiseExceptionHandling<Contract.customerID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Contract.customerID).Name)))
					{
						throw new PXRowPersistingException(typeof(Contract.customerID).Name, null, ErrorMessages.FieldIsEmpty, typeof(Contract.customerID).Name);
					}

				}

			}
		}

		protected virtual void Contract_TemplateID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Contract row = e.Row as Contract;
			if (row != null)
			{
                if (row.TemplateID != null)
                {
                    if (Contracts.Cache.GetStatus(row) == PXEntryStatus.Inserted)
                    {
                        DefaultFromTemplate(row, sender);
                    }
                }
			}
		}

		protected virtual void Contract_Type_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Contract row = e.Row as Contract;
			if (row != null)
			{
				SetControlsState(row, sender);
			}
		}
        
		protected virtual void Contract_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Contract row = e.Row as Contract;
			if (row != null)
			{
				if (Contracts.Cache.GetStatus(row) == PXEntryStatus.Inserted)
				{
					#region Default CuryID and Rate type from Customer and CustomerClass

					PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>> cs = new PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>(this);
					Customer customer = cs.Select(row.CustomerID);

					string defaultCuryID = null;
					string defaultCuryRateType = null;
					if (customer != null)
					{
						if (!string.IsNullOrEmpty(customer.CuryID))
						{
							defaultCuryID = customer.CuryID;
						}

						if (!string.IsNullOrEmpty(customer.CuryRateTypeID))
						{
							defaultCuryRateType = customer.CuryRateTypeID;
						}

						if (string.IsNullOrEmpty(defaultCuryID) || string.IsNullOrEmpty(defaultCuryRateType))
						{
							PXSelect<CustomerClass, Where<CustomerClass.customerClassID, Equal<Required<CustomerClass.customerClassID>>>> ccs = new PXSelect<CustomerClass, Where<CustomerClass.customerClassID, Equal<Required<CustomerClass.customerClassID>>>>(this);
							CustomerClass customerClass = ccs.Select(customer.CustomerClassID);
							if (customerClass != null)
							{
								if (!string.IsNullOrEmpty(defaultCuryID))
								{
									defaultCuryID = customerClass.CuryID;
								}

								if (!string.IsNullOrEmpty(defaultCuryRateType))
								{
									defaultCuryRateType = customerClass.CuryRateTypeID;
								}
							}

						}
					}

                    //if (!string.IsNullOrEmpty(defaultCuryID))
                    //{
                    //    row.CuryID = defaultCuryID;
                    //}

                    if (!string.IsNullOrEmpty(defaultCuryRateType))
                    {
                        row.RateTypeID = defaultCuryRateType;
                    }
                    
					#endregion
				}

				sender.SetDefaultExt<Contract.locationID>(row);
                Billing.Cache.SetDefaultExt<ContractBillingSchedule.accountID>(Billing.Current);

				CheckBillingAccount(Billing.Current);
			}
		}

        protected virtual void Contract_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			Contract row = e.Row as Contract;
			if (row != null && Company.Current != null)
			{
				row.CuryID = Company.Current.BaseCuryID;
			}
		}

		protected virtual void ResetDiscounts(PXCache sender, ContractDetail line)
		{
			line.BaseDiscountID = null;
			line.BaseDiscountSeq = null;
			line.BaseDiscountPct = 0;
            line.BaseDiscountAmt = 0;
			line.RecurringDiscountID = null;
			line.RecurringDiscountSeq = null;
			line.RecurringDiscountPct = 0;
            line.RecurringDiscountAmt = 0;
            line.RenewalDiscountID = null;
			line.RenewalDiscountSeq = null;
			line.RenewalDiscountPct = 0;
            line.RenewalDiscountAmt = 0;
		}

        protected virtual void SetDiscounts(PXCache sender, ContractDetail line)
        {
            Contract contract = Contracts.Current;

            DiscountSequence seq1;
            DiscountSequence seq2;
            bool isCompoundDiscount;

            SODiscountEngine.DiscountSetup group = new SODiscountEngine.DiscountSetup();

            group.FirstDiscountID = contract.DiscountID;
            group.SecondDiscountID = null;
            group.IsCompoundDiscount = false;

            ContractItem item = PXSelect<ContractItem, Where<ContractItem.contractItemID, Equal<Required<ContractItem.contractItemID>>>>.Select(this, line.ContractItemID);

            if (item.IsBaseValid == true && SODiscountEngine.SearchDiscounts(sender, group, item.BaseItemID, contract.CustomerID, contract.LocationID, (DateTime)contract.StartDate, out seq1, out seq2, out isCompoundDiscount))
            {
                if (seq1 != null)
                {
                    line.BaseDiscountID = seq1.DiscountID;
                    line.BaseDiscountSeq = seq1.DiscountSequenceID;
                }
            }

            if ((item.IsFixedRecurringValid == true || item.IsUsageValid == true) && SODiscountEngine.SearchDiscounts(sender, group, item.RecurringItemID, contract.CustomerID, contract.LocationID, (DateTime)contract.StartDate, out seq1, out seq2, out isCompoundDiscount))
            {
                if (seq1 != null)
                {
                    line.RecurringDiscountID = seq1.DiscountID;
                    line.RecurringDiscountSeq = seq1.DiscountSequenceID;
                }
            }

            if (item.IsRenewalValid == true && SODiscountEngine.SearchDiscounts(sender, group, item.RenewalItemID, contract.CustomerID, contract.LocationID, (DateTime)contract.StartDate, out seq1, out seq2, out isCompoundDiscount))
            {
                if (seq1 != null)
                {
                    line.RenewalDiscountID = seq1.DiscountID;
                    line.RenewalDiscountSeq = seq1.DiscountSequenceID;
                }
            }
        }

		static public void CalculateDiscounts(PXCache sender, Contract contract, ContractDetail det)
		{
			ContractItem item = (new PXSelect<ContractItem, Where<ContractItem.contractItemID, Equal<Required<ContractItem.contractItemID>>>>(sender.Graph)).Select(det.ContractItemID);

			DiscountSequence sequence = SODiscountEngine.GetDiscountSequenceByID(sender, det.BaseDiscountID, det.BaseDiscountSeq);
			if (sequence != null)
			{
				InventoryItem inventory = (new PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>(sender.Graph)).Select(item.BaseItemID);
				if (inventory != null)
				{
					SODiscountEngine.DiscountResult res = SODiscountEngine.GetDiscount(sender, sequence, contract.EffectiveFrom ?? contract.StartDate.Value, item.BaseItemID, inventory.BaseUnit, det.Qty, det.Qty * det.BasePriceVal, false);
					SODiscountEngine.ApplyDiscountToLine(det.Qty, det.BasePriceVal, det.Qty * det.BasePriceVal, new DiscountedLine<ContractDetail.baseDiscountAmt, ContractDetail.baseDiscountPct>(sender, det), res, 1);
				}
			}

			sequence = SODiscountEngine.GetDiscountSequenceByID(sender, det.RecurringDiscountID, det.RecurringDiscountSeq);
			if (sequence != null)
			{
				InventoryItem inventory = (new PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>(sender.Graph)).Select(item.RecurringItemID);
				if (inventory != null)
				{
					SODiscountEngine.DiscountResult res = SODiscountEngine.GetDiscount(sender, sequence, contract.EffectiveFrom ?? contract.StartDate.Value, item.RecurringItemID, inventory.BaseUnit, det.Qty, det.Qty * det.FixedRecurringPriceVal, false);
					SODiscountEngine.ApplyDiscountToLine(det.Qty, det.BasePriceVal, det.Qty * det.FixedRecurringPriceVal, new DiscountedLine<ContractDetail.recurringDiscountAmt, ContractDetail.recurringDiscountPct>(sender, det), res, 1);
				}
			}

			sequence = SODiscountEngine.GetDiscountSequenceByID(sender, det.RenewalDiscountID, det.RenewalDiscountSeq);
			if (sequence != null)
			{
				InventoryItem inventory = (new PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>(sender.Graph)).Select(item.RenewalItemID);
				if (inventory != null)
				{
					SODiscountEngine.DiscountResult res = SODiscountEngine.GetDiscount(sender, sequence, contract.EffectiveFrom ?? contract.StartDate.Value, item.RenewalItemID, inventory.BaseUnit, det.Qty, det.Qty * det.RenewalPriceVal, false);
					SODiscountEngine.ApplyDiscountToLine(det.Qty, det.BasePriceVal, det.Qty * det.RenewalPriceVal, new DiscountedLine<ContractDetail.renewalDiscountAmt, ContractDetail.renewalDiscountPct>(sender, det), res, 1);
				}
			}
		}

		protected virtual void Contract_DiscountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Contract contract = (Contract)e.Row;

			foreach (ContractDetail det in ContractDetails.Select())
			{
				ResetDiscounts(ContractDetails.Cache, det);
				if (string.IsNullOrEmpty(contract.DiscountID) == false)
				{
					SetDiscounts(ContractDetails.Cache, det);
				}
				ContractDetails.Update(det);
			}
		}

        protected virtual void Contract_LocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            Contract contract = (Contract)e.Row;
            if (contract != null)
            {
                if (Billing.Current != null)
                {
                    if (Billing.Current.BillTo == ContractBillingSchedule.billTo.CustomerAccount)
                    {
                        Billing.Cache.SetValue<ContractBillingSchedule.locationID>(Billing.Current, contract.LocationID);
                    }
                }
            }
        }

	    protected virtual void Contract_EffectiveFrom_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
	    {
	        Contract row = e.Row as Contract;
	        if (row == null) return;

            ARInvoice doc = PXSelect<ARInvoice, Where<ARInvoice.projectID, Equal<Current<Contract.contractID>>, And<ARInvoice.docDate, Greater<Required<ARInvoice.docDate>>>>>.SelectWindowed(this, 0, 1, (DateTime?) e.NewValue);

	        if (doc != null)
	        {
	            sender.RaiseExceptionHandling<Contract.effectiveFrom>(row, e.NewValue, new PXSetPropertyException(Messages.InvoiceExistPostGivendate, PXErrorLevel.Warning));
	        }
	    }
				
		#endregion

		#region ContractDetail Event Handlers

        protected virtual void ContractDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            ContractDetail row = e.Row as ContractDetail;
            if (row != null)
            {
                ContractItem item = PXSelect<ContractItem, Where<ContractItem.contractItemID, Equal<Required<ContractDetail.contractItemID>>>>.Select(this, row.ContractItemID);
                if (item != null)
                {
                    if (!ContractItemMaint.IsValidItemPrice(this, item))
                    {
                        PXUIFieldAttribute.SetWarning<ContractDetail.contractItemID>(sender, row, Messages.ItemNotPrice);
                    }
                }
                if (row.WarningAmountForDeposit==true) 
                {
                    this.RecurringDetails.Cache.RaiseExceptionHandling<ContractDetail.recurringIncluded>(row, row.RecurringIncluded, new PXSetPropertyException(Messages.DepositBalanceIsBelowTheRetainingAmountThreshold, PXErrorLevel.RowWarning));
                }

                bool stateAllowsPriceEdit = row.LastQty == null;

				PXUIFieldAttribute.SetEnabled<ContractDetail.qty>(sender, row, CurrentContract.Current.Status == ContractStatus.Draft || CurrentContract.Current.Status == ContractStatus.InUpgrade);
				PXUIFieldAttribute.SetEnabled<ContractDetail.basePriceVal>(sender, row, row.BasePriceEditable == true && stateAllowsPriceEdit);
				PXUIFieldAttribute.SetEnabled<ContractDetail.renewalPriceVal>(sender, row, row.RenewalPriceEditable == true && stateAllowsPriceEdit);
				PXUIFieldAttribute.SetEnabled<ContractDetail.fixedRecurringPriceVal>(sender, row, row.FixedRecurringPriceEditable == true && stateAllowsPriceEdit);
				PXUIFieldAttribute.SetEnabled<ContractDetail.usagePriceVal>(sender, row, row.UsagePriceEditable == true && stateAllowsPriceEdit);
				PXUIFieldAttribute.SetEnabled<ContractDetail.contractItemID>(sender, row, (CurrentContract.Current.Status == ContractStatus.Draft || CurrentContract.Current.Status == ContractStatus.InUpgrade) && CurrentTemplate.Current.AllowOverride==true);
				PXUIFieldAttribute.SetEnabled<ContractDetail.description>(sender, row, (CurrentContract.Current.Status == ContractStatus.Draft || CurrentContract.Current.Status == ContractStatus.InUpgrade) && CurrentTemplate.Current.AllowOverride == true);
				PXUIFieldAttribute.SetVisible<ContractDetail.change>(sender, null, CurrentContract.Current.Status != ContractStatus.Activated);
            }
        }

		protected virtual void ContractDetail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			ContractDetail row = e.Row as ContractDetail;
			if (row != null)
			{
				try
				{
					ValidateUniqueness(this, row);
				}
				catch (PXException ex)
				{
                    ContractItem cItem = (ContractItem)PXSelectorAttribute.Select<ContractDetail.contractItemID>(this.Caches<ContractDetail>(), row);
                    sender.RaiseExceptionHandling<ContractDetail.contractItemID>(row, cItem.ContractItemCD, ex);
					e.Cancel = true;
				}

				int? revID = row.RevID;
				int? contractDetailID = row.ContractDetailID;
				ContractDetail detailExt = PXSelectReadonly<ContractDetailExt,
												Where<ContractDetailExt.contractID, Equal<Required<ContractDetail.contractID>>,
														And<ContractDetailExt.contractItemID, Equal<Required<ContractDetail.contractItemID>>,
														And<ContractDetailExt.revID, Equal<Required<ContractDetail.revID>>>>>>.Select(this, row.ContractID, row.ContractItemID, row.RevID - 1);
				if (detailExt != null)
				{
					sender.RestoreCopy(row, detailExt);
					row.RevID = revID;
					ContractDetail det = sender.Locate(row) as ContractDetail;
					if (det != null)
						row.ContractDetailID = det.ContractDetailID;
					else
						row.ContractDetailID = contractDetailID;
					row.LastQty = row.Qty;
					row.Change = 0;
					row.LastBaseDiscountPct = row.BaseDiscountPct;
					row.LastRecurringDiscountPct = row.RecurringDiscountPct;
					row.LastRenewalDiscountPct = row.RenewalDiscountPct;
				}
			}
		}

		protected virtual void ContractDetail_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			ContractDetail oldRow = e.Row as ContractDetail;
			ContractDetail row = e.NewRow as ContractDetail;
			if (!sender.ObjectsEqual<ContractDetail.contractItemID>(oldRow,row))
			{
				try
				{
					ValidateUniqueness(this, row);
				}
				catch (PXException ex)
				{
					sender.RaiseExceptionHandling<ContractDetail.contractItemID>(row, row.ContractItemID, ex);
					e.Cancel = true;
				}
			}
		}

		protected virtual void ContractDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			ContractDetail row = e.Row as ContractDetail;
			if (row != null)
			{
				CalculateDiscounts(sender, Contracts.Current, row);
				Contracts.Current.TotalsCalculated = null;

				if (!IsImport)
				{
					ContractItem contractItem = PXSelect<ContractItem, Where<ContractItem.contractItemID, Equal<Required<ContractDetail.contractItemID>>>>.Select(this, row.ContractItemID);
					if (contractItem != null && contractItem.Deposit == false && contractItem.DepositItemID != null)
					{
						ContractItem depositItem = PXSelect<ContractItem, Where<ContractItem.contractItemID, Equal<Required<ContractDetail.contractItemID>>>>.Select(this, contractItem.DepositItemID);
						ContractDetail newDetail = new ContractDetail();
						sender.SetValueExt<ContractDetail.contractItemID>(newDetail, depositItem.ContractItemID);
						ContractDetails.Insert(newDetail);
						ContractDetails.View.RequestRefresh();
					}
				}
			}
		}

		protected virtual void ContractDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			CalculateDiscounts(sender, Contracts.Current, (ContractDetail)e.Row);
			Contracts.Current.TotalsCalculated = null;
		}

		protected virtual void ContractDetail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			Contracts.Current.TotalsCalculated = null;
		}
		
		protected virtual void ContractDetail_ContractItemID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractDetail row = e.Row as ContractDetail;

            ContractItem item = PXSelect<ContractItem, Where<ContractItem.contractItemID, Equal<Required<ContractItem.contractItemID>>>>.Select(this, row.ContractItemID);
			if (item != null && row != null)
			{
				row.Qty = item.DefaultQty;
                InventoryItem nonstock = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, item.BaseItemID ?? item.RecurringItemID);
				row.Description = item.Descr;
			}

			ResetDiscounts(sender, (ContractDetail)e.Row);
			SetDiscounts(sender, (ContractDetail)e.Row);
		}

        protected virtual void ContractDetail_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            ContractDetail row = (ContractDetail)e.Row;
            ContractItem item = PXSelect<ContractItem, Where<ContractItem.contractItemID, Equal<Required<ContractDetail.contractItemID>>>>.Select(this, row.ContractItemID);
            if (item != null && (item.MaxQty < (decimal?)e.NewValue || item.MinQty > (decimal?)e.NewValue))
            {
                throw new PXSetPropertyException(Messages.QtyErrorWithParameters, item.MinQty, item.MaxQty);
            }
        }

		protected virtual void ContractDetail_BasePriceVal_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractDetail row = e.Row as ContractDetail;
			if (row.BasePriceOption == PriceOption.Manually)
			{
				sender.SetValue<ContractDetail.basePrice>(e.Row, sender.GetValue<ContractDetail.basePriceVal>(e.Row));
			}
		}

		protected virtual void ContractDetail_RenewalPriceVal_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractDetail row = e.Row as ContractDetail;
			if (row.RenewalPriceOption == PriceOption.Manually)
			{
				sender.SetValue<ContractDetail.renewalPrice>(e.Row, sender.GetValue<ContractDetail.renewalPriceVal>(e.Row));
			}
		}

		protected virtual void ContractDetail_FixedRecurringPriceVal_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractDetail row = e.Row as ContractDetail;
			if (row.FixedRecurringPriceOption == PriceOption.Manually)
			{
				sender.SetValue<ContractDetail.fixedRecurringPrice>(e.Row, sender.GetValue<ContractDetail.fixedRecurringPriceVal>(e.Row));
			}
		}

		protected virtual void ContractDetail_UsagePriceVal_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractDetail row = e.Row as ContractDetail;
			if (row.UsagePriceOption == PriceOption.Manually)
			{
				sender.SetValue<ContractDetail.usagePrice>(e.Row, sender.GetValue<ContractDetail.usagePriceVal>(e.Row));
			}
		}

		#endregion
		
		#region ContractBillingSchedule Event Handlers
		
		protected virtual void ContractBillingSchedule_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ContractBillingSchedule row = e.Row as ContractBillingSchedule;
			if (row != null)
			{
				SetControlsState(row, sender);
			}
		}

        protected virtual void ContractBillingSchedule_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            ContractBillingSchedule row = (ContractBillingSchedule)e.Row;
            if (row != null)
            {
				if (row.AccountID == null)
				{
					if (row.BillTo == ContractBillingSchedule.billTo.SpecificAccount)
					{
						if (sender.RaiseExceptionHandling<ContractBillingSchedule.accountID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ContractBillingSchedule.accountID).Name)))
						{
							throw new PXRowPersistingException(typeof(ContractBillingSchedule.accountID).Name, null, ErrorMessages.FieldIsEmpty, typeof(ContractBillingSchedule.accountID).Name);
						}
					}
				}
				CheckBillingAccount(row);
            }
        }

		protected virtual void ContractBillingSchedule_BillTo_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            ContractBillingSchedule schedule = (ContractBillingSchedule)e.Row;
            if (schedule != null)
            {
                if (CurrentContract.Current.CustomerID != null)
                {
                    sender.SetDefaultExt<ContractBillingSchedule.accountID>(schedule);
                    switch (schedule.BillTo)
                    {
                        case ContractBillingSchedule.billTo.ParentAccount:
                            sender.SetDefaultExt<ContractBillingSchedule.locationID>(schedule);
                            break;
                        case ContractBillingSchedule.billTo.CustomerAccount:
                            schedule.LocationID = CurrentContract.Current.LocationID;
                            break;
                        case ContractBillingSchedule.billTo.SpecificAccount:
                        default:
                            schedule.LocationID = null;
                            break;
                    }
					CheckBillingAccount(schedule);
                }
            }
        }

        protected virtual void ContractBillingSchedule_AccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            ContractBillingSchedule schedule = (ContractBillingSchedule)e.Row;
            if (schedule != null)
        {
                switch (schedule.BillTo)
            {
                    case ContractBillingSchedule.billTo.ParentAccount:
                        BAccount bAccount = PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, CurrentContract.Current.CustomerID);
                        Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<BAccount.parentBAccountID>>>>.Select(this, bAccount.ParentBAccountID);
                        //Customer customer = PXSelectJoin<Customer, InnerJoin<BAccount, On<BAccount.parentBAccountID, Equal<Customer.bAccountID>>>, Where<BAccount.bAccountID, Equal<Required<Contract.customerID>>>>.Select(this, CurrentContract.Current.CustomerID);
                        if (customer != null)
                {
                            e.NewValue = customer.BAccountID;
                }
                        break;
                    case ContractBillingSchedule.billTo.CustomerAccount:
                        e.NewValue = CurrentContract.Current.CustomerID;
                        break;
                    case ContractBillingSchedule.billTo.SpecificAccount:
                    default:
                        e.NewValue = null;
                        break;
                }
            }
        }

        protected virtual void ContractBillingSchedule_AccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            sender.SetDefaultExt<ContractBillingSchedule.locationID>(e.Row);
        }

		protected virtual void ContractBillingSchedule_LocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			BillingLocation.RaiseFieldUpdated(sender, e.Row);

			foreach (ContractDetail det in ContractDetails.Select())
			{
				ContractDetails.Cache.SetDefaultExt<ContractDetail.basePriceVal>(det);
				ContractDetails.Cache.SetDefaultExt<ContractDetail.renewalPriceVal>(det);
				ContractDetails.Cache.SetDefaultExt<ContractDetail.fixedRecurringPriceVal>(det);
				ContractDetails.Cache.SetDefaultExt<ContractDetail.usagePriceVal>(det);

				if (ContractDetails.Cache.GetStatus(det) == PXEntryStatus.Notchanged)
				{
					ContractDetails.Cache.SetStatus(det, PXEntryStatus.Updated);
				}
			}
		}

		#endregion

        #region SelContractWatcher Event Handlers

        protected virtual void SelContractWatcher_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			SelContractWatcher member = (SelContractWatcher)e.Row;
			if (member.ContactID != null)
			{
				Contact cont = PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>
					.Select(this, member.ContactID);
				member.FirstName = cont.FirstName;
				member.MidName = cont.MidName;
				member.LastName = cont.LastName;
				member.Title = cont.Title;
			}
		}

		protected virtual void SelContractWatcher_ContactID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SelContractWatcher row = e.Row as SelContractWatcher;
			if (row != null)
			{
				Contact contact = PXSelect<Contact>.Search<Contact.contactID>(this, row.ContactID);

				if (contact != null && string.IsNullOrEmpty(contact.EMail) == false && string.IsNullOrEmpty(row.EMail))
				{
					row.EMail = contact.EMail;
				}
				if (contact != null && string.IsNullOrEmpty(contact.Salutation) == false && string.IsNullOrEmpty(row.Salutation))
				{
					row.Salutation = contact.Salutation;
				}
			}
		}

		protected virtual void SelContractWatcher_WatchTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SelContractWatcher caseContact = e.Row as SelContractWatcher;
			if (caseContact != null)
			{
				e.NewValue = "A";
			}
		}

		#endregion

		#region ActivationSettingsFilter Event Handlers
		
		protected virtual void ActivationSettingsFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            ActivationSettingsFilter row = e.Row as ActivationSettingsFilter;
            if (row != null)
            {
				PXUIFieldAttribute.SetVisible<ActivationSettingsFilter.startDate>(sender, row, row.CalledAction == ContractAction.Setup);
				PXUIFieldAttribute.SetVisible<ActivationSettingsFilter.activationDate>(sender, row, row.CalledAction != ContractAction.Setup);

                if (row.ActivationDate < row.StartDate)
                {
                    PXUIFieldAttribute.SetError<ActivationSettingsFilter.startDate>(sender, row, Messages.ActivationDateError);
                }
                else
                {
					if (row.CalledAction != ContractAction.Setup && row.ActivationDate > row.StartDate && Contracts.Current.IsPendingUpdate != true)
                    {
                        PXUIFieldAttribute.SetWarning<ActivationSettingsFilter.startDate>(sender, row, Messages.StartDateNoMatchActivation);
                    }
                    else
                    {
                        PXUIFieldAttribute.SetError<ActivationSettingsFilter.startDate>(sender, row, null);
                    }
                }

            }
		}

		#endregion


		#region EPContractRate
		[PXDBInt()]
		[PXParent(typeof(Select<Contract, Where<Contract.contractID, Equal<Current<EPContractRate.contractID>>>>))]
		[PXDBDefault(typeof(Contract.contractID))]
		protected virtual void EPContractRate_ContractID_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		[PXEPEmployeeSelector()]
		[PXCheckUnique(IgnoreNulls = false, Where = typeof(Where<EPContractRate.earningType, Equal<Current<EPContractRate.earningType>>, And<EPContractRate.contractID, Equal<Current<EPContractRate.contractID>>>>))]
		[PXUIField(DisplayName = "Employee")]
		protected virtual void EPContractRate_EmployeeID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(2, IsFixed = true, IsUnicode = false, InputMask = ">LL")]
		[PXDefault()]
		[PXSelector(typeof(EP.EPEarningType.typeCD))]
		[PXUIField(DisplayName = "Earning Type")]
		protected virtual void EPContractRate_EarningType_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#endregion

        public override void Persist()
        {
			Billing.Current = Billing.Select();
			PXResultset<ContractDetail> list = ContractDetails.Select();
			foreach (ContractDetail detail in list)
            {
				if (Billing.Current != null && Billing.Current.Type == BillingType.OnDemand)
				{
					string itemCD;
					if (!TemplateMaint.ValidItemForOnDemand(this, detail, out itemCD))
					{
						ContractDetails.Cache.RaiseExceptionHandling<ContractDetail.contractItemID>(detail, itemCD, new PXException(Messages.ItemOnDemandRecurringItem));
						ContractDetails.Cache.SetStatus(detail, PXEntryStatus.Updated);
					}
				}
            }
			TemplateMaint.CheckContractOnDepositItems(list, Contracts.Current);
            base.Persist();
        }

		bool customerChanged;
		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (viewName == "Contracts" && values != null)
			{
				customerChanged = values.Contains("CustomerID") && values["CustomerID"] != PXCache.NotSetValue;
			}
			if (viewName.ToLower() == "billing" && values != null)
			{
				if (!this.IsImport && customerChanged)
				{
					values["BillTo"] = PXCache.NotSetValue;
				}
				if (Billing.Current != null && Billing.Current.BillTo != ContractBillingSchedule.billTo.SpecificAccount)
				{
					values["AccountID"] = PXCache.NotSetValue;
				}
			}
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		public static void ValidateUniqueness(PXGraph graph, ContractDetail row, bool validateRecurring = false)
		{
			//assuming that row is not in cache yet
			if (row.ContractItemID.HasValue)
			{
				PXSelectBase<ContractDetail> s = new PXSelect<ContractDetail,
                    Where<ContractDetail.contractItemID, Equal<Required<ContractDetail.contractItemID>>,
					And<ContractDetail.contractID, Equal<Required<Contract.contractID>>,
					And<ContractDetail.revID, Equal<Required<ContractDetail.revID>>,
					And<ContractDetail.lineNbr, NotEqual<Required<ContractDetail.lineNbr>>>>>>>(graph);

                ContractDetail item = s.SelectWindowed(0, 1, row.ContractItemID, row.ContractID, row.RevID, row.LineNbr);
				ContractItem cItem = (ContractItem)PXSelectorAttribute.Select<ContractDetail.contractItemID>(graph.Caches<ContractDetail>(), row);

				if (item != null)
				{
					throw new PXException(Messages.DuplicateItem, cItem.ContractItemCD);
				}

				if (validateRecurring && cItem.RecurringItemID != null)
				{
					InventoryItem invItem = (InventoryItem)PXSelectorAttribute.Select<ContractItem.recurringItemID>(graph.Caches<ContractItem>(), cItem);
					
					item = (new PXSelectJoin<ContractDetail, InnerJoin<ContractItem, On<ContractDetail.contractItemID, Equal<ContractItem.contractItemID>>>,
						Where<ContractDetail.contractID, Equal<Required<Contract.contractID>>, 
 						And<ContractDetail.revID, Equal<Required<ContractDetail.revID>>,
						And<ContractItem.recurringItemID, Equal<Required<ContractItem.recurringItemID>>>>>>(graph)).SelectWindowed(0, 1, row.ContractID, row.RevID, cItem.RecurringItemID);

					if (item != null)
					{
						throw new PXException(Messages.DuplicateRecurringItem, invItem.InventoryCD);
					}
				}
			}
		}

		protected virtual void SetControlsState(Contract row, PXCache cache)
		{
			if (row != null)
			{
				#region Reset Disabled fields
				PXUIFieldAttribute.SetEnabled<Contract.startDate>(cache, row, true);
				PXUIFieldAttribute.SetEnabled<Contract.gracePeriod>(cache, row, true);
				PXUIFieldAttribute.SetEnabled<Contract.calendarID>(cache, row, true);
                PXUIFieldAttribute.SetEnabled<Contract.curyID>(cache, row, false);
				PXUIFieldAttribute.SetEnabled<Contract.rateTypeID>(cache, row, true);
				PXUIFieldAttribute.SetEnabled<Contract.detailedBilling>(cache, row, true); 
                PXUIFieldAttribute.SetEnabled<Contract.effectiveFrom>(cache, row, row.IsPendingUpdate == true);
                #endregion

				#region Contract Type

				PXUIFieldAttribute.SetEnabled<Contract.gracePeriod>(cache, row, row.Type != ContractType.Unlimited);
				#endregion

				#region Template Settings

				PXUIFieldAttribute.SetEnabled<Contract.type>(cache, row, !row.TemplateID.HasValue);
				PXUIFieldAttribute.SetEnabled<Contract.caseItemID>(cache, row, !row.TemplateID.HasValue);
												
				SLAMapping.Cache.AllowUpdate = row.TemplateID.HasValue;
				SLAMapping.Cache.AllowInsert = row.TemplateID.HasValue;
				SLAMapping.Cache.AllowDelete = row.TemplateID.HasValue;

				bool enabled = false;
				if (row.Status != ContractStatus.Canceled)
				{
					if (CurrentTemplate.Current != null)
					{
						PXUIFieldAttribute.SetEnabled<Contract.curyID>(cache, row, CurrentTemplate.Current.AllowOverrideCury == true);
						//PXUIFieldAttribute.SetEnabled<Contract.rateTypeID>(cache, row, CurrentTemplate.Current.AllowOverrideRate == true);

						enabled = CurrentTemplate.Current.AllowOverride == true;
						PXUIFieldAttribute.SetEnabled<Contract.caseItemID>(cache, row, enabled);
						
					}
					else
						enabled = true;
				}

                RecurringDetails.Cache.AllowUpdate = false;
                RecurringDetails.Cache.AllowInsert = false;
                RecurringDetails.Cache.AllowDelete = false;
				ContractDetails.Cache.AllowInsert = (row != null && row.TemplateID != null && (row.Status == ContractStatus.Draft || row.Status == ContractStatus.InUpgrade) && enabled);
				ContractDetails.Cache.AllowUpdate = true;// (row != null && row.TemplateID != null && (row.Status == ContractStatus.Draft || row.Status == ContractStatus.InUpgrade) && enabled);
				ContractDetails.Cache.AllowDelete = (row != null && row.TemplateID != null && (row.Status == ContractStatus.Draft || row.Status == ContractStatus.InUpgrade) && enabled);

				#endregion
				
				PXUIFieldAttribute.SetEnabled<Contract.templateID>(cache, row, cache.GetStatus(row) == PXEntryStatus.Inserted);
				PXUIFieldAttribute.SetEnabled<Contract.customerID>(cache, row, cache.GetStatus(row) == PXEntryStatus.Inserted);

				#region Setup Controls for Multi-Currency
				
				PXUIFieldAttribute.SetVisible<Contract.curyID>(cache, row, IsMultyCurrency);
				PXUIFieldAttribute.SetVisible<Contract.rateTypeID>(cache, row, IsMultyCurrency);
				#endregion

				//viewUsage.SetEnabled(true);

				if (cache.GetStatus(row) == PXEntryStatus.Inserted)
				{
					#region Disable Buttons

					//terminate.SetEnabled(false);
					//viewUsage.SetEnabled(false);

					#endregion

				}
				else
				{
					SLAMapping.Cache.AllowUpdate = true;
					SLAMapping.Cache.AllowInsert = true;
					SLAMapping.Cache.AllowDelete = true;

					terminate.SetEnabled(row.CustomerID != null);

					if (row.Status == ContractStatus.Canceled)
					{
						#region Disable everything
						PXUIFieldAttribute.SetEnabled<Contract.startDate>(cache, row, false);
						PXUIFieldAttribute.SetEnabled<Contract.activationDate>(cache, row, false);
						PXUIFieldAttribute.SetEnabled<Contract.gracePeriod>(cache, row, false);
						PXUIFieldAttribute.SetEnabled<Contract.autoRenew>(cache, row, false);
						PXUIFieldAttribute.SetEnabled<Contract.autoRenewDays>(cache, row, false);
						PXUIFieldAttribute.SetEnabled<Contract.calendarID>(cache, row, false);
						//PXUIFieldAttribute.SetEnabled<Contract.curyID>(cache, row, false);
						PXUIFieldAttribute.SetEnabled<Contract.rateTypeID>(cache, row, false);
						PXUIFieldAttribute.SetEnabled<Contract.detailedBilling>(cache, row, false);
						
						SLAMapping.Cache.AllowUpdate = false;
						SLAMapping.Cache.AllowInsert = false;
						SLAMapping.Cache.AllowDelete = false;
						#endregion
					}					
				}

				Delete.SetEnabled(row.Status == ContractStatus.Draft);
				Contracts.Cache.AllowDelete = row.Status == ContractStatus.Draft;
			}
		}

		protected virtual void SetControlsState(ContractBillingSchedule row, PXCache cache)
		{
			if (row != null )
			{
				if (Contracts.Current != null && Contracts.Current.TemplateID.HasValue)
				{
					PXUIFieldAttribute.SetEnabled<ContractBillingSchedule.type>(cache, row, false);
				}

				if (Contracts.Current != null && Contracts.Current.Status != ContractStatus.Activated)
				{
					PXUIFieldAttribute.SetEnabled<ContractBillingSchedule.nextDate>(cache, row, false);
				}

                PXUIFieldAttribute.SetEnabled<ContractBillingSchedule.accountID>(cache, row, row.BillTo==ContractBillingSchedule.billTo.SpecificAccount ? true : false);
                PXUIFieldAttribute.SetEnabled<ContractBillingSchedule.locationID>(cache, row, row.BillTo == ContractBillingSchedule.billTo.SpecificAccount || row.BillTo == ContractBillingSchedule.billTo.ParentAccount ? true : false);
			}
		}
        
        protected virtual void DefaultFromTemplate(Contract contract, PXCache cache)
		{
			ContractTemplate template = PXSelectReadonly<ContractTemplate>.Search<ContractTemplate.contractID>(this, contract.TemplateID);
			
			if (template != null)
			{
				contract.AutoRenew = template.AutoRenew;
				contract.AutoRenewDays = template.AutoRenewDays;
				contract.ClassType = template.ClassType;
				contract.CuryID = template.CuryID;
				contract.GracePeriod = template.GracePeriod;
				contract.RateTypeID = template.RateTypeID;
				contract.Type = template.Type;
				contract.Description = template.Description;
				contract.DurationType = template.DurationType;
				contract.Duration = template.Duration;
				contract.CalendarID = template.CalendarID;
				contract.StartDate = Accessinfo.BusinessDate;
				contract.ActivationDate = Accessinfo.BusinessDate;
				contract.DetailedBilling = template.DetailedBilling;
				contract.CaseItemID = template.CaseItemID;
				contract.AutomaticReleaseAR = template.AutomaticReleaseAR;
				contract.ScheduleStartsOn = template.ScheduleStartsOn;
				
				ContractBillingSchedule templateBilling = PXSelectReadonly<ContractBillingSchedule, Where<ContractBillingSchedule.contractID, Equal<Current<Contract.templateID>>>>.Select(this);
				if (templateBilling != null && Billing.Current != null)
				{
					Billing.Current.Type = templateBilling.Type;
					Billing.Cache.SetValueExt<ContractBillingSchedule.billTo>(Billing.Current, templateBilling.BillTo);
				}

				SLAMapping.Cache.Clear();
				PXResultset<ContractSLAMapping> list = PXSelectReadonly<ContractSLAMapping, Where<ContractSLAMapping.contractID, Equal<Current<Contract.templateID>>>>.Select(this);
				foreach (ContractSLAMapping item in list)
				{
					ContractSLAMapping sla = new ContractSLAMapping();
					sla.ContractID = contract.ContractID;
					sla.Severity = item.Severity;
					sla.Period = item.Period;
					SLAMapping.Insert(sla);
				}
				
				foreach (ContractDetail item in ContractDetails.Select())
				{
					ContractDetails.Delete(item);
				}

				PXResultset<ContractDetail> items = PXSelectReadonly<ContractDetail,
						Where<ContractDetail.contractID, Equal<Current<Contract.templateID>>, And<ContractDetail.inventoryID, IsNull>>>.Select(this);

				foreach (ContractDetail item in items)
				{
					ContractDetail newitem = new ContractDetail();
                    newitem = (ContractDetail)ContractDetails.Cache.CreateCopy(ContractDetails.Insert(newitem));
					CopyTemplateDetail(item, newitem);
					ContractDetails.Update(newitem);
				}


                var history = RenewalHistory.Current;
                CTBillEngine.UpdateContractHistoryEntry(history, contract, Billing.Current);
                RenewalHistory.Update(history);
			}

		}

		protected virtual void CopyTemplateDetail(ContractDetail source, ContractDetail target)
		{
			target.CuryInfoID = source.CuryInfoID;
			target.CuryItemFee = source.CuryItemFee;
			target.Description = source.Description;
			target.Included = source.Included;
			target.InventoryID = source.InventoryID;
			target.ItemFee = source.ItemFee;
			target.ResetUsage = source.ResetUsage;
			target.AccountID = source.AccountID;
			target.SubID = source.SubID;
			target.UOM = source.UOM;
            target.ContractItemID = source.ContractItemID;
            target.Qty = source.Qty;
		}


		protected virtual void CopyContractDetail(ContractDetail source, ContractDetail target)
		{
			CopyTemplateDetail(source, target);

            target.BasePrice = source.BasePrice;
            target.BasePriceOption = source.BasePriceOption;

            target.RenewalPrice = source.RenewalPrice;
            target.RenewalPriceOption = source.RenewalPriceOption;

            target.FixedRecurringPrice = source.FixedRecurringPrice;
            target.FixedRecurringPriceOption = source.FixedRecurringPriceOption;

			target.FixedRecurringPrice = source.FixedRecurringPrice;
			target.FixedRecurringPriceOption = source.FixedRecurringPriceOption;

			target.UsagePrice = source.UsagePrice;
			target.UsagePriceOption = source.UsagePriceOption;
		}       

		private void CalcDetail(Contract row)
		{
			decimal pendingSetup = 0;
			decimal pendingRecurring = 0;
			decimal pendingRenewal = 0;
			decimal currentSetup = 0;
			decimal currentRecurring = 0;
			decimal currentRenewal = 0;
			foreach (PXResult<ContractDetail, ContractItem> res in ContractDetails.View.SelectMultiBound(new object[] { row }))
			{
				ContractDetail detail = (ContractDetail)res;
				ContractItem item = (ContractItem)res;
				pendingSetup += detail.Change.GetValueOrDefault() * detail.BasePriceVal.GetValueOrDefault() * (100 - detail.BaseDiscountPct.GetValueOrDefault()) / 100;
				pendingRecurring += detail.Qty.GetValueOrDefault() * detail.FixedRecurringPriceVal.GetValueOrDefault() * (100 - detail.RecurringDiscountPct.GetValueOrDefault()) / 100;
				pendingRenewal += detail.Qty.GetValueOrDefault() * detail.RenewalPriceVal.GetValueOrDefault() * (100 - detail.RenewalDiscountPct.GetValueOrDefault()) / 100;
				currentSetup += detail.LastQty.GetValueOrDefault() * detail.BasePriceVal.GetValueOrDefault() * (100 - detail.LastBaseDiscountPct.GetValueOrDefault()) / 100;
				currentRecurring += detail.LastQty.GetValueOrDefault() * detail.FixedRecurringPriceVal.GetValueOrDefault() * (100 - detail.LastRecurringDiscountPct.GetValueOrDefault()) / 100;
				currentRenewal += detail.LastQty.GetValueOrDefault() * detail.RenewalPriceVal.GetValueOrDefault() * (100 - detail.LastRenewalDiscountPct.GetValueOrDefault()) / 100;
			}
			row.PendingSetup = pendingSetup;
			row.PendingRecurring = pendingRecurring;
			row.PendingRenewal = pendingRenewal;
			row.CurrentSetup = row.Status == ContractStatus.Activated ? pendingSetup : currentSetup;
			row.CurrentRecurring = row.Status ==  ContractStatus.Activated ? pendingRecurring : currentRecurring;
			row.CurrentRenewal = row.Status ==  ContractStatus.Activated ? pendingRenewal : currentRenewal;
			row.TotalPending = row.PendingRecurring + row.PendingRenewal + row.PendingSetup;
		}

		private void CalcSummary(PXCache sender, Contract row)
		{
			//balance = Invoice + DebitMemo - CreditMemo
			PXSelectBase<ARInvoice> selectInvoices = new PXSelectGroupBy<ARInvoice, Where<ARInvoice.projectID, Equal<Required<Contract.contractID>>, And<ARInvoice.docType, NotEqual<ARDocType.creditMemo>, And<ARInvoice.released, Equal<True>>>>, Aggregate<Sum<ARInvoice.curyDocBal>>>(this);
			PXSelectBase<ARInvoice> selectCreditMemo = new PXSelectGroupBy<ARInvoice, Where<ARInvoice.projectID, Equal<Required<Contract.contractID>>, And<ARInvoice.docType, Equal<ARDocType.creditMemo>, And<ARInvoice.released, Equal<True>>>>, Aggregate<Sum<ARInvoice.curyDocBal>>>(this);
			ARInvoice aggregate = selectInvoices.SelectSingle(row.ContractID);
			ARInvoice aggregateCM = selectCreditMemo.SelectSingle(row.ContractID);
			row.Balance = (aggregate.CuryDocBal ?? 0m) - (aggregateCM.CuryDocBal ?? 0m);

			decimal? totalRecurring = 0m;
			decimal? totalOveruse = 0m;
			foreach (PXResult<ContractDetail, ContractItem, InventoryItem> res in RecurringDetails.View.SelectMultiBound(new object[] { row }))
			{
				ContractDetail detail = (ContractDetail)res;
				ContractItem item = (ContractItem)res;

				if (item.DepositItemID == null)
				{
					totalRecurring += detail.Qty.GetValueOrDefault() * detail.FixedRecurringPriceVal.GetValueOrDefault() * (100 - detail.RecurringDiscountPct.GetValueOrDefault()) * 0.01m;
					decimal overuse;
					if (item.ResetUsageOnBilling == true)
					{
						overuse = detail.Used.GetValueOrDefault() - detail.Qty.GetValueOrDefault();
					}
					else
					{
						decimal billedQty = detail.UsedTotal.GetValueOrDefault() - detail.Used.GetValueOrDefault();
						decimal available = detail.Qty.GetValueOrDefault() - billedQty;
						overuse = detail.Used.GetValueOrDefault() - Math.Max(available, 0);
					}
					if (overuse > 0)
						totalOveruse += overuse * detail.UsagePriceVal.GetValueOrDefault();
				}
			}
			CTBillEngine biller = PXGraph.CreateInstance<CTBillEngine>();
			try
			{
				totalOveruse += biller.RecalcDollarUsage(row);
			}
			catch (Exception)
			{
				totalOveruse = null;
				sender.RaiseExceptionHandling<Contract.totalUsage>(row, null, new PXSetPropertyException(Messages.CannotCalculateValue, PXErrorLevel.Warning));
			}
			row.TotalRecurring = totalRecurring;
			row.TotalUsage = totalOveruse;
			row.TotalDue = totalRecurring + totalOveruse;
		}
								
		public static void SetExpireDate(Contract contract)
		{
			if (contract.ScheduleStartsOn == ScheduleStartOption.SetupDate)
			{
				if (contract.StartDate.HasValue)
				{
					contract.ExpireDate =  GetExpireDate(contract, contract.StartDate.Value);
				}
				else
				{
					contract.ExpireDate = null;
				}
			}
			else
			{
				if (contract.ActivationDate.HasValue)
				{
					contract.ExpireDate =  GetExpireDate(contract, contract.ActivationDate.Value);
				}
				else
				{
					contract.ExpireDate = null;
				}
			}
		}

        public static DateTime? GetExpireDate(Contract contract, DateTime origin)
        {
			if (contract.Type != ContractType.Unlimited && contract.StartDate.HasValue)
            {
                if (contract.DurationType == null)
                    throw new PXException(Messages.ContractDurationIsNotConfigured);

                switch (contract.DurationType)
                {
                    case DurationType.Annual:
                        return origin.AddYears(contract.Duration.HasValue ? contract.Duration.Value : 1).AddDays(-1);
                    case DurationType.Monthly:
                        return origin.AddMonths(contract.Duration.HasValue ? contract.Duration.Value : 1).AddDays(-1);
                    case DurationType.Quarterly:
                        return origin.AddMonths(3 * (contract.Duration.HasValue ? contract.Duration.Value : 1)).AddDays(-1);
                    case DurationType.Custom:
                        return origin.AddDays(contract.Duration.HasValue ? contract.Duration.Value : 1).AddDays(-1);
                }
            }

            return null;
        }
        
		protected virtual void RenewExpiring(ContractMaint graph)
		{
			Contract renewed = PXSelect<Contract, Where<Contract.originalContractID, Equal<Current<Contract.contractID>>>>.Select(this);
			if (renewed != null)
				throw new PXException(Messages.ContractAlreadyRenewed, renewed.ContractCD);

		    if (IsExpired(Contracts.Current, Accessinfo.BusinessDate.Value))
		    {
		        Contracts.Current.IsCompleted = true;
		        Contracts.Current.Status = ContractStatus.Expired;
		        Contracts.Update(Contracts.Current);
		    }

			Contract newContract = (Contract)PXCache<Contract>.CreateCopy(Contracts.Current);
		    newContract.IsCompleted = false;
		    newContract.IsActive = false;
		    newContract.IsPendingUpdate = false;
			newContract.RevID = 1;
			newContract.LastActiveRevID = null;
            newContract.Status = ContractStatus.Draft;
			newContract.ContractID = null;
			newContract.ContractCD = null;
			newContract.OriginalContractID = Contracts.Current.ContractID;
			newContract.StartDate = GetNextStartDate();
			newContract.ActivationDate = newContract.StartDate;
			newContract.ExpireDate = null;
			newContract.LineCtr = 0;

            //We have to drop these fields so that RowInserted does its work before any call to CheckBillingAccount
            newContract.CustomerID = null;
            newContract.LocationID = null;
			newContract = graph.Contracts.Insert(newContract);
            
            //Now, with Billing.Current having been set up in the graph we can update Customer
            newContract.CustomerID = Contracts.Current.CustomerID;
            newContract.LocationID = Contracts.Current.LocationID;
            newContract = graph.Contracts.Update(newContract);
            
			#region Setup Billing

			ContractBillingSchedule billingCurrent = Billing.Select();
			graph.Billing.Current.Type = billingCurrent.Type;
			//never should be set true because contract will be created in draft status.
			graph.Billing.Current.ChargeRenewalFeeOnNextBilling = false;
            graph.Billing.Update(graph.Billing.Current);
			
			#endregion

			#region Copy SLA
			graph.SLAMapping.Cache.Clear();
			PXResultset<ContractSLAMapping> list = PXSelectReadonly<ContractSLAMapping, Where<ContractSLAMapping.contractID, Equal<Current<Contract.contractID>>>>.Select(this);
			foreach (ContractSLAMapping item in list)
			{
				ContractSLAMapping sla = new ContractSLAMapping();
				sla.ContractID = newContract.ContractID;
				sla.Severity = item.Severity;
				sla.Period = item.Period;
				graph.SLAMapping.Insert(sla);
			} 
			#endregion
			
			#region Copy Watchers
			PXResultset<SelContractWatcher> listWatchers = PXSelectReadonly<SelContractWatcher, Where<SelContractWatcher.contractID, Equal<Current<Contract.contractID>>>>.Select(this);
			foreach (SelContractWatcher item in listWatchers)
			{
				SelContractWatcher watcher = (SelContractWatcher) Watchers.Cache.CreateCopy(item);
				watcher.ContractID = newContract.ContractID;
				graph.Watchers.Insert(watcher);
			}  
			#endregion

			#region CSAnswers
			PXResultset<CSAnswers> cSAnswerList = PXSelectReadonly<CSAnswers, Where<CSAnswers.entityType, Equal<CSAnswerType.contractAnswerType>, And<CSAnswers.entityID, Equal<Current<Contract.contractID>>>>>.Select(this);
			foreach (CSAnswers cSAnswer in cSAnswerList)
			{
				CSAnswers newAnswer = (CSAnswers)Answers.Cache.CreateCopy(cSAnswer);
				newAnswer.EntityID = newContract.ContractID;
				graph.Answers.Insert(newAnswer);
			}
			#endregion

			PXResultset<ContractDetail> items = PXSelect<ContractDetail,
						Where<ContractDetail.contractID, Equal<Current<Contract.contractID>>>>.Select(this);

			foreach (ContractDetail item in items)
            {
				CopyContractDetail(item, graph.ContractDetails.Insert(new ContractDetail()));
				ContractDetail newItem = PXCache<ContractDetail>.CreateCopy(item) as ContractDetail;
				ContractDetails.Cache.Remove(newItem);
				newItem.RevID += 1;
                ContractDetails.Insert(newItem);
			}

			if (CurrentTemplate.Current.RefreshOnRenewal == true)
			{
				foreach (ContractDetail item in graph.ContractDetails.Select())
				{
					ContractDetail templateItem = PXSelect<ContractDetail,
						Where<ContractDetail.contractID, Equal<Current<Contract.templateID>>,
						And<ContractDetail.contractItemID, Equal<Required<ContractDetail.contractItemID>>>>>.Select(this, item.ContractItemID);

					if (templateItem != null)
					{
						graph.CopyTemplateDetail(templateItem, item);

						graph.ContractDetails.Cache.SetDefaultExt<ContractDetail.basePrice>(item);
						graph.ContractDetails.Cache.SetDefaultExt<ContractDetail.renewalPrice>(item);
						graph.ContractDetails.Cache.SetDefaultExt<ContractDetail.fixedRecurringPrice>(item);
						graph.ContractDetails.Cache.SetDefaultExt<ContractDetail.usagePrice>(item);

						graph.ContractDetails.Cache.SetDefaultExt<ContractDetail.basePriceOption>(item);
						graph.ContractDetails.Cache.SetDefaultExt<ContractDetail.renewalPriceOption>(item);
						graph.ContractDetails.Cache.SetDefaultExt<ContractDetail.fixedRecurringPriceOption>(item);
						graph.ContractDetails.Cache.SetDefaultExt<ContractDetail.usagePriceOption>(item);

						graph.ContractDetails.Update(item);
					}
				}
			}
		}
        
		

		protected virtual DateTime GetNextStartDate()
		{
			if (CurrentTemplate.Current != null)
			{
				if (CurrentTemplate.Current.IsContinuous == true || !IsExpired(Contracts.Current, Accessinfo.BusinessDate.Value))
				{
					return Contracts.Current.ExpireDate.Value.AddDays(1);
				}
				else
				{
					return Accessinfo.BusinessDate.Value;
				}
			}
			else
			{
				return Contracts.Current.ExpireDate.Value.AddDays(1);
			}
		}

		public static bool IsExpired(Contract row, DateTime businessDate)
		{
			if (row.ExpireDate != null)
			{
				int daysAfterExpire = ((TimeSpan)businessDate.Date.Subtract(row.ExpireDate.Value)).Days;

				if (daysAfterExpire > (row.GracePeriod ?? 0))
					return true;
				else
					return false;

			}
			else
				return false;
		}

		public static bool IsInGracePeriod(Contract row, DateTime businessDate, out int daysLeft)
		{
			daysLeft = 0;
			
			if (row.ExpireDate != null)
			{
                int daysAfterExpire = ((TimeSpan)businessDate.Subtract(row.ExpireDate.Value.Date)).Days;

				if (daysAfterExpire > 0 && daysAfterExpire < (row.GracePeriod ?? 0))
				{
					daysLeft = (row.GracePeriod ?? 0) - daysAfterExpire;
					return true;
				}
				else
					return false;

			}
			else
				return false;
		}

		protected bool IsMultyCurrency
		{
			get
			{
				CMSetup cmsetup = PXSelect<CMSetup>.Select(this);

				if (cmsetup == null)
				{
					return false;
				}
				else
				{
					return cmsetup.MCActivated == true;
				}

			}
		}

		private void CheckBillingAccount(ContractBillingSchedule schedule)
		{
			if (schedule.AccountID == null && schedule.BillTo == ContractBillingSchedule.billTo.ParentAccount)
			{
				Billing.Cache.RaiseExceptionHandling<ContractBillingSchedule.billTo>(schedule, ContractBillingSchedule.billTo.ParentAccount, new PXSetPropertyException(Messages.CustomerDoesNotHaveParentAccount));
			}
			if (schedule.AccountID != null)
			{
				if (CurrentContract.Current != null)
				{
					Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, schedule.AccountID);
					CustomerClass customerClass = PXSelect<CustomerClass, Where<CustomerClass.customerClassID, Equal<Required<Customer.customerClassID>>>>.Select(this, customer.CustomerClassID);
					string customerCury = customer.CuryID ?? customerClass.CuryID;
					if (CurrentContract.Current.CuryID != null && CurrentContract.Current.CuryID != customerCury && customer.AllowOverrideCury != true)
					{
						Billing.Cache.RaiseExceptionHandling<ContractBillingSchedule.accountID>(schedule, customer.AcctCD, new PXSetPropertyException(Messages.CustomerCuryNotMatchWithContractCury));
					}
				}
			}
		}

		#region Local Types
		[Serializable]
		public partial class ActivationSettingsFilter : IBqlTable
		{
			#region ActivationDate
			public abstract class activationDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _ActivationDate;
			[PXFormula(typeof(Switch<
				Case<Where<Current<Contract.status>, Equal<ContractStatus.ContractStatusUpgrade>, And<Current<Contract.effectiveFrom>, IsNotNull>>, Current<Contract.effectiveFrom>,
				Case<Where<ActivationSettingsFilter.startDate, Greater<Current<Contract.activationDate>>>, ActivationSettingsFilter.startDate>>,
				Current<Contract.activationDate>>))]
			[PXDBDate()]
			[PXUIField(DisplayName = "Activation Date", Required = true)]
			public virtual DateTime? ActivationDate
			{
				get
				{
					return this._ActivationDate;
				}
				set
				{
					this._ActivationDate = value;
				}
			}
			#endregion
            #region StartDate
            public abstract class startDate : PX.Data.IBqlField
            {
            }
            protected DateTime? _StartDate;
			[PXDefault(typeof(Contract.startDate))]
            [PXDBDate()]
            [PXUIField(DisplayName = "Setup Date", Required = true)]
            public virtual DateTime? StartDate
            {
                get
                {
                    return this._StartDate;
                }
                set
                {
                    this._StartDate = value;
                }
            }
            #endregion
			#region CalledAction
			public abstract class calledAction : PX.Data.IBqlField
			{
			}
			protected String _CalledAction;
			[PXDBString(2)]
			public virtual String CalledAction
			{
				get
				{
					return this._CalledAction;
				}
				set
				{
					this._CalledAction = value;
				}
			}
			#endregion
		}

        [Serializable]
		public partial class TerminationSettingsFilter : IBqlTable
        {
            #region TerminationDate
            public abstract class terminationDate : PX.Data.IBqlField
			{
			}
            protected DateTime? _TerminationDate;
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXDate()]
			[PXUIField(DisplayName = "Termination Date", Required = true)]
            public virtual DateTime? TerminationDate
			{
				get
				{
                    return this._TerminationDate;
				}
				set
				{
                    this._TerminationDate = value;
				}
			}
			#endregion
		}

		[Serializable]
		public partial class BillingOnDemandSettingsFilter : IBqlTable
		{
			#region BillingDate
			public abstract class billingDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _BillingDate;
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXDate()]
			[PXUIField(DisplayName = "Billing Date", Required = true)]
			public virtual DateTime? BillingDate
			{
				get
				{
					return this._BillingDate;
				}
				set
				{
					this._BillingDate = value;
				}
			}
			#endregion
		}

		#endregion
	}

	[PXProjection(typeof(Select2<ContractWatcher, RightJoin<Contact,
		   On<Contact.contactID, Equal<ContractWatcher.contactID>>>>), Persistent = true)]
    [Serializable]
	public partial class SelContractWatcher : ContractWatcher
	{
		#region DisplayName
		public abstract class displayName : PX.Data.IBqlField
		{
		}
		protected String _displayName;
		[PXUIField(Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[ContactDisplayName(typeof(SelContractWatcher.lastName), typeof(SelContractWatcher.firstName),
			typeof(SelContractWatcher.midName), typeof(SelContractWatcher.title), true, 
			BqlField = typeof(Contact.displayName))]
		public virtual String DisplayName
		{
			get { return _displayName; }
			set { _displayName = value; }
		}
		#endregion
		#region FirstName
		public abstract class firstName : PX.Data.IBqlField
		{
		}
		protected String _FirstName;
		[PXDBString(50, IsUnicode = true, BqlField = typeof(Contact.firstName))]
		[PXUIField(DisplayName = "First Name")]
		public virtual String FirstName
		{
			get
			{
				return this._FirstName;
			}
			set
			{
				this._FirstName = value;
			}
		}
		#endregion
		#region MidName
		public abstract class midName : PX.Data.IBqlField
		{
		}
		protected String _MidName;
		[PXDBString(50, IsUnicode = true, BqlField = typeof(Contact.midName))]
		[PXUIField(DisplayName = "Middle Name")]
		public virtual String MidName
		{
			get
			{
				return this._MidName;
			}
			set
			{
				this._MidName = value;
			}
		}
		#endregion
		#region LastName
		public abstract class lastName : PX.Data.IBqlField
		{
		}
		protected String _LastName;
		[PXDBString(100, IsUnicode = true, BqlField = typeof(Contact.lastName))]
		[PXUIField(DisplayName = "Last Name")]
		public virtual String LastName
		{
			get
			{
				return this._LastName;
			}
			set
			{
				this._LastName = value;
			}
		}
		#endregion
		#region Title
		public abstract class title : PX.Data.IBqlField
		{
		}
		protected String _Title;
		[PXDBString(50, IsUnicode = true, BqlField = typeof(Contact.title))]
		[Titles]
		[PXUIField(DisplayName = "Title")]
		public virtual String Title
		{
			get
			{
				return this._Title;
			}
			set
			{
				this._Title = value;
			}
		}
		#endregion
		#region Salutation
		public abstract class salutation : PX.Data.IBqlField
		{
		}
		protected String _Salutation;
		[PXDBString(255, IsUnicode = true, BqlField = typeof(Contact.salutation))]
		[PXUIField(DisplayName = "Attention", Visibility = PXUIVisibility.SelectorVisible)]
		//[PXParentSearch()]
		public virtual String Salutation
		{
			get
			{
				return this._Salutation;
			}
			set
			{
				this._Salutation = value;
			}
		}
		#endregion
		#region Phone1
		public abstract class phone1 : PX.Data.IBqlField
		{
		}
		protected String _Phone1;
		[PXDBString(50, BqlField = typeof(Contact.phone1))]
		[PXUIField(DisplayName = "Phone 1", Visibility = PXUIVisibility.SelectorVisible)]
		[PhoneValidation()]
		public virtual String Phone1
		{
			get
			{
				return this._Phone1;
			}
			set
			{
				this._Phone1 = value;
			}
		}
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _BAccountID;
		[PXDBInt(IsKey = false, BqlField = typeof(Contact.bAccountID))]
		//[PXDBLiteDefault(typeof(BAccount.bAccountID))]
		[PXDimensionSelector("BIZACCT", typeof(Search<BAccount.bAccountID>), typeof(BAccount.acctCD), DescriptionField = typeof(BAccount.acctName), DirtyRead = true)]
		[PXUIField(DisplayName = "Customer ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		#region ContactContactID
		public abstract class contactContactID : PX.Data.IBqlField
		{
		}
		protected Int32? _ContactContactID;
		[PXDBInt(BqlField = typeof(Contact.contactID))]
		[PXUIField(Visibility = PXUIVisibility.Invisible)]
		[PXExtraKey]
		public virtual Int32? ContactContactID
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		#endregion
	}
}
