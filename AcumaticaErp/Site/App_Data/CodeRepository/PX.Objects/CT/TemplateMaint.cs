using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CM;

namespace PX.Objects.CT
{
    public class TemplateMaint : PXGraph<TemplateMaint, ContractTemplate>
    {
		#region DAC Attributes override

		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(ContractTemplate.contractID))]
		[PXParent(typeof(Select<ContractTemplate, Where<ContractTemplate.contractID, Equal<Current<ContractBillingSchedule.contractID>>>>))]
		protected virtual void ContractBillingSchedule_ContractID_CacheAttached(PXCache sender) { }

        [PXDBInt()]
        [PXDBDefault(typeof(ContractTemplate.contractID))]
        [PXParent(typeof(Select<ContractTemplate, Where<ContractTemplate.contractID, Equal<Current<ContractSLAMapping.contractID>>>>))]
        protected virtual void ContractSLAMapping_ContractID_CacheAttached(PXCache sender) { }
		
		#region CSAttributeGroup
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		protected virtual void CSAttributeGroup_EntityClassID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(2, IsFixed = true, IsKey = true)]
		[PXDefault(CSAnswerType.Contract)]
		protected virtual void CSAttributeGroup_Type_CacheAttached(PXCache sender)
		{
		}
		#endregion
        #region ContractDetail

        [PXDBInt]
        [PXDefault()]
        [PXDimensionSelector(ContractItemAttribute.DimensionName, typeof(Search<ContractItem.contractItemID>),
                                                                    typeof(ContractItem.contractItemCD),
                                                                    typeof(ContractItem.contractItemCD), typeof(ContractItem.descr))]
        [PXUIField(DisplayName = "Item Code")]
		[PXRestrictor(typeof(Where<ContractItem.curyID, Equal<Current<ContractTemplate.curyID>>>), Messages.ItemHasAnotherCuryID)]
        protected virtual void ContractDetail_ContractItemID_CacheAttached(PXCache sender) { }

        [PXDBInt(IsKey = true)]
        [PXDBDefault(typeof(ContractTemplate.contractID))]
        [PXParent(typeof(Select<ContractTemplate, Where<ContractTemplate.contractID, Equal<Current<ContractDetail.contractID>>>>))]
        protected virtual void ContractDetail_ContractID_CacheAttached(PXCache sender) { }

		[PXDBInt(MinValue = 1)]
		[PXDefault(typeof(ContractTemplate.revID), PersistingCheck = PXPersistingCheck.Null)]
		protected virtual void ContractDetail_RevID_CacheAttached(PXCache sender) { }

		[PXDBInt(IsKey = true)]
		[ContractLineNbr(typeof(ContractTemplate.lineCtr))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		protected virtual void ContractDetail_LineNbr_CacheAttached(PXCache sender) { }
        #endregion
        #endregion

        public TemplateMaint()
        {
            bool crmInstalled = PXAccess.FeatureInstalled<CS.FeaturesSet.customerModule>();
            if (!crmInstalled)
            {
                PXUIFieldAttribute.SetVisible<ContractTemplate.caseItemID>(Templates.Cache, null, false);
                PXUIFieldAttribute.SetVisible<ContractTemplate.min>(Templates.Cache, null, false);
            }
        }

        #region Selects/Views
        public PXSelect<ContractTemplate, Where<ContractTemplate.baseType, Equal<ContractTemplate.ContractBaseType>, And<ContractTemplate.isTemplate, Equal<boolTrue>>>> Templates;
		public PXSelect<ContractTemplate, Where<ContractTemplate.contractID, Equal<Current<ContractTemplate.contractID>>>> CurrentTemplate;
        public PXSelect<Contract, Where<Contract.templateID, Equal<Current<ContractTemplate.contractID>>>> Contracts;
		public PXSelect<ContractBillingSchedule, Where<ContractBillingSchedule.contractID, Equal<Current<ContractTemplate.contractID>>>> Billing;
		public PXSelect<ContractSLAMapping, Where<ContractSLAMapping.contractID, Equal<Current<ContractTemplate.contractID>>>> SLAMapping;
        public PXSelectJoin<ContractDetail,
            LeftJoin<ContractItem, On<ContractDetail.contractItemID, Equal<ContractItem.contractItemID>>>,
            Where<ContractDetail.contractID, Equal<Current<ContractTemplate.contractID>>>> ContractDetails;	
		public PXSetup<Company> Company;
		public PXSelectJoin<CSAttributeGroup,
			InnerJoin<CSAttribute,
			On<CSAttributeGroup.attributeID, Equal<CSAttribute.attributeID>>>,
			Where<CSAttributeGroup.entityClassID, Equal<Current<ContractTemplate.contractCD>>,
			And<CSAttributeGroup.type, Equal<CSAnswerType.contractAnswerType>>>,
			OrderBy<Asc<CSAttributeGroup.sortOrder>>>  AttributeGroup;
		#endregion

		#region ContractTemplate Event handlers
        
		protected virtual void ContractTemplate_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;

            ContractTemplate row = e.Row as ContractTemplate;
			SetControlsState(row, sender);
		}
		
		protected virtual void ContractTemplate_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			ContractTemplate row = e.Row as ContractTemplate;
			if (row != null)
			{
				ContractBillingSchedule schedule = new ContractBillingSchedule();
				schedule.ContractID = row.ContractID;
				Billing.Insert(schedule);

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

				Billing.Cache.IsDirty = false;
				SLAMapping.Cache.IsDirty = false;
			}
		}

		protected virtual void ContractTemplate_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			ContractTemplate row = e.Row as ContractTemplate;
			if (row != null)
			{
				ContractTemplate refContract = PXSelect<ContractTemplate, Where<ContractTemplate.templateID, Equal<Current<ContractTemplate.contractID>>>>.Select(this);

				if (refContract != null)
				{
					e.Cancel = true;
					throw new PXException(Messages.ContractRefError);
				}
			}
		}
						
		protected virtual void ContractTemplate_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ContractTemplate row = e.Row as ContractTemplate;
			if (row != null && Company.Current != null)
			{
				row.CuryID = Company.Current.BaseCuryID;
			}
		}

		protected virtual void ContractTemplate_Type_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractTemplate row = e.Row as ContractTemplate;
			SetControlsState(row, sender);
		}

		private void SetControlsState(ContractTemplate row, PXCache sender)
		{
            Contracts.Cache.AllowInsert = false;
            Contracts.Cache.AllowUpdate = false;
            Contracts.Cache.AllowDelete = false;
			if (row != null)
			{
				#region Setup Controls for Multi-Currency
				PXUIFieldAttribute.SetVisible<ContractTemplate.curyID>(sender, row, IsMultyCurrency);
				PXUIFieldAttribute.SetVisible<ContractTemplate.rateTypeID>(sender, row, IsMultyCurrency);
				PXUIFieldAttribute.SetVisible<ContractTemplate.allowOverrideCury>(sender, row, IsMultyCurrency);
				PXUIFieldAttribute.SetVisible<ContractTemplate.allowOverrideRate>(sender, row, IsMultyCurrency);
				#endregion

				PXUIFieldAttribute.SetEnabled<ContractTemplate.isContinuous>(sender, row, row.Type == ContractType.Renewable);

				#region Contract Type

				PXUIFieldAttribute.SetEnabled<ContractTemplate.duration>(sender, row, row.Type != ContractType.Unlimited);
				PXUIFieldAttribute.SetEnabled<ContractTemplate.durationType>(sender, row, row.Type != ContractType.Unlimited);
				PXUIFieldAttribute.SetEnabled<ContractTemplate.expireDate>(sender, row, row.Type != ContractType.Unlimited);
				PXUIFieldAttribute.SetEnabled<ContractTemplate.gracePeriod>(sender, row, row.Type != ContractType.Unlimited);
				PXUIFieldAttribute.SetEnabled<ContractTemplate.isContinuous>(sender, row, row.Type != ContractType.Unlimited);
				PXUIFieldAttribute.SetEnabled<ContractTemplate.refundPeriod>(sender, row, row.Refundable == true);
				#endregion

                ContractDetail detail = PXSelect<ContractDetail, Where<ContractDetail.contractID, Equal<Current<ContractTemplate.contractID>>>>.SelectWindowed(this, 0, 1);
                PXUIFieldAttribute.SetEnabled<ContractTemplate.curyID>(sender, row, detail == null ? true : false);
			}

			AttributeGroup.AllowInsert = row.ContractID > 0;
		}

		#endregion

		#region ContractTemplateItem Event Handlers

        protected virtual void ContractDetail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            ContractDetail row = e.Row as ContractDetail;
            if (row != null)
            {
                row.ContractID = CurrentTemplate.Current.ContractID;
                ValidateUniqueness(sender, row, e);

                ContractItem contractItem = PXSelect<ContractItem, Where<ContractItem.contractItemID, Equal<Required<ContractDetail.contractItemID>>>>.Select(this, row.ContractItemID);
                if (contractItem!=null)
					row.Qty = contractItem.DefaultQty;
            }
        }

		protected virtual void ContractDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (!IsImport)
			{
				ContractDetail row = e.Row as ContractDetail;
				if (row != null)
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

        protected virtual void ContractDetail_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
        {
            ContractDetail row = e.NewRow as ContractDetail;
            if (row != null)
            {
                ValidateUniqueness(sender, row, e);
            }
        }

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
            }
        }

		protected virtual void ContractDetail_ContractItemID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractDetail row = e.Row as ContractDetail;

			ContractItem item = PXSelect<ContractItem, Where<ContractItem.contractItemID, Equal<Required<ContractItem.contractItemID>>>>.Select(this, row.ContractItemID);
			if (item != null && row != null)
			{
                InventoryItem nonstock = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, item.BaseItemID ?? item.RecurringItemID);
			    if (nonstock != null)
			    {
			        row.Description = item.Descr;
			        row.Qty = item.DefaultQty;
			    }
			}
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
		
		#endregion	        		

		#region CSAttributeGroup Event handler
		//Remove trailing spaces received from PXDimensionSelector
		protected virtual void CSAttributeGroup_EntityClassID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CSAttributeGroup row = e.Row as CSAttributeGroup;
			if (row != null)
			{
				e.NewValue = CurrentTemplate.Current.ContractCD.Trim();
			}
		}

		protected virtual void CSAttributeGroup_DefaultValue_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			string AnswerValueField = "DefaultValue";
			int AnswerValueLength = 60;
			CSAttributeGroup row = e.Row as CSAttributeGroup;
			if (row != null)
			{
				CSAttribute question = new PXSelect<CSAttribute>(this).Search<CSAttribute.attributeID>(row.AttributeID);
				PXResultset<CSAttributeDetail> options = PXSelect<CSAttributeDetail,
					Where<CSAttributeDetail.attributeID, Equal<Required<CSAttributeGroup.attributeID>>>,
					OrderBy<Asc<CSAttributeDetail.sortOrder>>>.Select(this, row.AttributeID);

				int required = -1;
				if ((bool)row.Required)
				{
					required = 1;
				}

				if (options.Count > 0)
				{
					//ComboBox:

					List<string> allowedValues = new List<string>();
					List<string> allowedLabels = new List<string>();

					foreach (CSAttributeDetail option in options)
					{
						allowedValues.Add(option.ValueID);
						allowedLabels.Add(option.Description);
					}

					string mask = question != null ? question.EntryMask : null;

					e.ReturnState = PXStringState.CreateInstance(e.ReturnState, CSAttributeDetail.ParameterIdLength, true, AnswerValueField, false, required, mask, allowedValues.ToArray(), allowedLabels.ToArray(), true, null);
					if (question.ControlType == CSAttribute.MultiSelectCombo)
					{
						((PXStringState)e.ReturnState).MultiSelect = true;
					}
				}
				else if (question != null)
				{
					if (question.ControlType.Value == CSAttribute.CheckBox)
					{
						e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(bool), false, false, required, null, null, false, AnswerValueField, null, null, null, PXErrorLevel.Undefined, true, true, null, PXUIVisibility.Visible, null, null, null);
					}
					else if (question.ControlType.Value == CSAttribute.Datetime)
					{
						string mask = question != null ? question.EntryMask : null;
						e.ReturnState = PXDateState.CreateInstance(e.ReturnState, AnswerValueField, false, required, mask, mask, null, null);
					}
					else
					{
						//TextBox:
						string mask = question != null ? question.EntryMask : null;

						e.ReturnState = PXStringState.CreateInstance(e.ReturnState, AnswerValueLength, null, AnswerValueField, false, required, mask, null, null, true, null);
					}
				}

			}
		}
		#endregion

        public override void Persist()
        {
			PXResultset<ContractDetail> list = ContractDetails.Select();
			foreach (ContractDetail detail in list)
            {
				Billing.Current = Billing.Select();
				if (Billing.Current != null && Billing.Current.Type == BillingType.OnDemand)
				{
					string itemCD;
					if (!ValidItemForOnDemand(this, detail, out itemCD))
					{
						ContractDetails.Cache.RaiseExceptionHandling<ContractDetail.contractItemID>(detail, itemCD, new PXException(Messages.ItemOnDemandRecurringItem));
						ContractDetails.Cache.SetStatus(detail, PXEntryStatus.Updated);
					}
				}
            }
			CheckContractOnDepositItems(list, Templates.Current);
			base.Persist();
        }

		private void ValidateUniqueness(PXCache sender, ContractDetail row, System.ComponentModel.CancelEventArgs e)
		{
			if (row.ContractItemID.HasValue)
			{
				PXSelectBase<ContractDetail> s = new PXSelect<ContractDetail,
                    Where<ContractDetail.contractItemID, Equal<Required<ContractDetail.contractItemID>>, 
					And<ContractDetail.contractID, Equal<Current<ContractTemplate.contractID>>,
                    And<ContractDetail.contractDetailID, NotEqual<Required<ContractDetail.contractDetailID>>>>>>(this);

                ContractDetail item = s.SelectWindowed(0, 1, row.ContractItemID, row.ContractDetailID);

				if (item != null)
				{
                    ContractItem cirow =(ContractItem)PXSelectorAttribute.Select<ContractDetail.contractItemID>(sender, row);
                    sender.RaiseExceptionHandling<ContractDetail.contractItemID>(row, cirow.ContractItemCD, new PXException(Messages.ItemNotUnique));					
					e.Cancel = true;
				}
			}
		}

		private bool IsMultyCurrency
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

		public static bool ValidItemForOnDemand(PXGraph graph, ContractDetail detail, out string itemCD)
		{
			ContractItem item = PXSelect<ContractItem, Where<ContractItem.contractItemID, Equal<Required<ContractDetail.contractItemID>>>>.Select(graph, detail.ContractItemID);
			if (item != null && item.RecurringItemID != null && item.DepositItemID == null)
			{
				itemCD = item.ContractItemCD;
				return false;
			}
			else
			{
				itemCD = null;
				return true;
			}

		}

		public static void CheckContractOnDepositItems(PXResultset<ContractDetail> list, Contract contract)
		{
			int qty = 0;
			foreach (ContractDetail det in list)
			{
				if (det.Deposit == true)
					qty++;
			}
			if (qty > 0 && contract.Type == ContractType.Renewable)
				throw new PXSetPropertyException(Messages.RenewableContractContainsDepositItem);
			if (qty > 1)
				throw new PXSetPropertyException(Messages.DepositItemGreaterThanOne);
		}
    }
}
