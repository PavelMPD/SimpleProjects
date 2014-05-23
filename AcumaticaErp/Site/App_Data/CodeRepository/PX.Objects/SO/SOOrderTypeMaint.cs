using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.IN;

namespace PX.Objects.SO
{
	public class SOOrderTypeMaint : PXGraph<SOOrderTypeMaint, SOOrderType>
	{
		public PXSelect<SOOrderType> soordertype;
		public PXSelect<SOOrderType, Where<SOOrderType.orderType, Equal<Current<SOOrderType.orderType>>>> currentordertype;

		public PXSelect<SOOrderTypeOperation,
			Where<SOOrderTypeOperation.orderType, Equal<Current<SOOrderType.orderType>>>>
			operations;

		public PXSelect<SOOrderTypeOperation,
			Where<SOOrderTypeOperation.orderType, Equal<Current<SOOrderType.orderType>>,
			And<SOOrderTypeOperation.operation, Equal<Current<SOOrderType.defaultOperation>>>>>
			defaultOperation;

		public PXSelect<SOOrderType,
			Where<SOOrderType.template, Equal<Required<SOOrderType.orderType>>,
				And<SOOrderType.orderType, NotEqual<SOOrderType.template>>>> references;

		public SOOrderTypeMaint()
		{
			operations.Cache.AllowInsert = operations.Cache.AllowDelete = false;
			this.FieldVerifying.AddHandler<SOOrderType.salesSubMask>(SOOrderType_Mask_FieldVerifying);			
			this.FieldVerifying.AddHandler<SOOrderType.miscSubMask>(SOOrderType_Mask_FieldVerifying);
			this.FieldVerifying.AddHandler<SOOrderType.freightSubMask>(SOOrderType_Mask_FieldVerifying);
			this.FieldVerifying.AddHandler<SOOrderType.discSubMask>(SOOrderType_Mask_FieldVerifying);
			//this.FieldVerifying.AddHandler<SOOrderType.cOGSSubMask>(SOOrderType_Mask_FieldVerifying);
		}

		protected virtual void SOOrderType_Mask_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOOrderType row = (SOOrderType)e.Row;
			if(row == null || (row.Active != true && e.NewValue == null))
				e.Cancel = true;
		}
		protected virtual void SOOrderType_Template_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			SOOrderType row = (SOOrderType)e.Row;
			if(row == null) return;
			if(sender.GetStatus(row) == PXEntryStatus.Inserted && row.OrderType == (string)e.NewValue)
				e.NewValue = null;
		}
		protected virtual void SOOrderType_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}					
			SOOrderType ordertype = (SOOrderType)e.Row;
			SOOrderType prevtype = (SOOrderType)e.OldRow;			
			if(ordertype.Active == true)
			{
				if (prevtype.SalesSubMask == null && ordertype.SalesSubMask == null) sender.SetDefaultExt<SOOrderType.salesSubMask>(ordertype);
				if (prevtype.MiscSubMask == null && ordertype.MiscSubMask == null) sender.SetDefaultExt<SOOrderType.miscSubMask>(ordertype);
				if (prevtype.FreightSubMask == null && ordertype.FreightSubMask == null) sender.SetDefaultExt<SOOrderType.freightSubMask>(ordertype);
				if (prevtype.DiscSubMask == null && ordertype.DiscSubMask == null) sender.SetDefaultExt<SOOrderType.discSubMask>(ordertype);
				//if (prevtype.COGSSubMask == null && ordertype.COGSSubMask == null) sender.SetDefaultExt<SOOrderType.cOGSSubMask>(ordertype);
			}
			
			if (ordertype.Template != ((SOOrderType)e.OldRow).Template)
			{
				SOOrderType template =
					PXSelect<SOOrderType, Where<SOOrderType.orderType, Equal<Required<SOOrderType.orderType>>>>
					.SelectWindowed(this, 0,1,ordertype.Template);
				this.soordertype.Current = ordertype;

				if(template == null) return;
				ordertype.Behavior = template.Behavior;
				ordertype.DefaultOperation = template.DefaultOperation;
				ordertype.RequireShipping = template.RequireShipping;
				ordertype.RequireLocation = template.RequireLocation;
				ordertype.RequireAllocation = template.RequireAllocation;
				ordertype.ARDocType = template.ARDocType;
				ordertype.INDocType = template.INDocType;
				ordertype.ShipmentPlanType = template.ShipmentPlanType;
				ordertype.OrderPlanType = template.OrderPlanType;

				foreach (SOOrderTypeOperation o in this.operations.View.SelectMultiBound( new object[]{ordertype}))
				{
					this.operations.Delete(o);
				}

				foreach (SOOrderTypeOperation o in this.operations.View.SelectMultiBound(new object[] { template}))
				{
					SOOrderTypeOperation upd = new SOOrderTypeOperation();
					upd.OrderType = ordertype.OrderType;
					upd.Operation = o.Operation;
					upd.INDocType = o.INDocType;
					upd.ShipmentPlanType = o.ShipmentPlanType;
					upd.OrderPlanType = o.OrderPlanType;
					upd.AutoCreateIssueLine = o.AutoCreateIssueLine;
					upd.Active = o.Active;
					upd = this.operations.Insert(upd);
				}				
			}

			if (ordertype.Template == null && sender.ObjectsEqual<SOOrderType.behavior, SOOrderType.aRDocType>(e.Row, e.OldRow) == false)
			{
				foreach (SOOrderTypeOperation o in this.operations.View.SelectMultiBound(new object[] { ordertype }))
				{
					this.operations.Delete(o);
				}
				string defaultOp = SOBehavior.DefaultOperation(ordertype.Behavior, ordertype.ARDocType);

				if (defaultOp != null)
				{
					ordertype.DefaultOperation = defaultOp;
					SOOrderTypeOperation def = new SOOrderTypeOperation();
					def.OrderType = ordertype.OrderType;
					def.Operation = ordertype.DefaultOperation;
					this.operations.Insert(def);
					if (ordertype.Behavior == SOBehavior.RM)
					{
						def = new SOOrderTypeOperation();
						def.OrderType = ordertype.OrderType;
						def.Operation = SOOperation.Issue;
						this.operations.Insert(def);
					}
				}
			}
			
			if (ordertype.OrderType != ordertype.Template) return;
			if (ordertype.RequireShipping == false)
			{
				foreach (SOOrderTypeOperation op in operations.Select())
				{
					SOOrderTypeOperation upd = PXCache<SOOrderTypeOperation>.CreateCopy(op);
					upd.ShipmentPlanType = null;
					this.operations.Update(upd);					
				}				
			}
			if (ordertype.ARDocType == ARDocType.NoUpdate)
			{
				foreach (SOOrderTypeOperation op in operations.Select())
				{
					SOOrderTypeOperation upd = PXCache<SOOrderTypeOperation>.CreateCopy(op);
					if (upd.INDocType == INTranType.Invoice || upd.INDocType == INTranType.DebitMemo)
						upd.INDocType = INTranType.Issue;
					if (upd.INDocType == INTranType.CreditMemo)
						upd.INDocType = INTranType.Return;
					this.operations.Update(upd);
				}
			}
		}

		protected virtual void SOOrderTypeOperation_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SOOrderTypeOperation row = (SOOrderTypeOperation)e.Row;
			SOOrderType ordertype = this.soordertype.Current;
			if(row == null || ordertype == null || row.Operation != ordertype.DefaultOperation) return;

			ordertype.INDocType = row.INDocType;
			ordertype.OrderPlanType = row.OrderPlanType;
			ordertype.ShipmentPlanType = row.ShipmentPlanType;

			if (row.INDocType == INTranType.NoUpdate)
			{				
				row.OrderPlanType = null;
				row.ShipmentPlanType = null;
				ordertype.RequireShipping = false;
				ordertype.RequireLocation = false;
				ordertype.RequireAllocation = false;
			}
			if (row.INDocType == INTranType.Transfer)
			{
				ordertype.RequireShipping = true;
				ordertype.ARDocType = ARDocType.NoUpdate;
			}			

			if (row.INDocType == INTranType.NoUpdate)
			{
				sender.SetValue<SOOrderTypeOperation.orderPlanType>(e.Row, null);
				sender.SetValue<SOOrderTypeOperation.shipmentPlanType>(e.Row, null);
			}
		}
		
		protected virtual void SOOrderTypeOperation_INDocType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOOrderTypeOperation row = (SOOrderTypeOperation)e.Row;
			if(row == null) return;
			
			short? inInvtMult = INTranType.InvtMult((string)e.NewValue);
			if((row.Operation == SOOperation.Issue && inInvtMult > 0) ||
				(row.Operation == SOOperation.Receipt && inInvtMult < 0))
				throw new PXSetPropertyException(Messages.OrderTypeUnsupportedOperation);

		}

		protected virtual void SOOrderTypeOperation_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOOrderTypeOperation row = e.Row as SOOrderTypeOperation;

			if (row != null)
			{
				e.NewValue = INTranType.InvtMult(row.INDocType);
				e.Cancel = true;
			}
		}

		protected virtual void SOOrderTypeOperation_Active_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOOrderTypeOperation row = e.Row as SOOrderTypeOperation;

			if (row != null)
			{
				if (currentordertype.Current.Behavior == SOBehavior.RM || currentordertype.Current.Behavior == SOBehavior.IN)
				{
					if (row.Operation==SOOperation.Issue && row.Active==false)
					{
						foreach (SOOrderTypeOperation var in operations.Cache.Cached)
						{
							if (var.Operation == SOOperation.Receipt)
							{
								var.AutoCreateIssueLine = false;
								operations.Update(var);
								operations.View.RequestRefresh();
							}
						}
					}
				}
			}
		}

		protected virtual void SOOrderType_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}
			SOOrderType ordertype = (SOOrderType)e.Row;

			SOOrderType link = references.SelectWindowed(0, 1, ordertype.OrderType);
			PXUIFieldAttribute.SetEnabled<SOOrderType.template>(sender, e.Row, link == null);

			bool isTemplateUpdatable = 
				link == null && ordertype.OrderType != null &&
				(ordertype.OrderType == ordertype.Template || ordertype.Template == null);

			SOOrderTypeOperation def = this.defaultOperation.Select(ordertype.OrderType, ordertype.DefaultOperation);
			if(def == null) def = new SOOrderTypeOperation();
			PXUIFieldAttribute.SetEnabled<SOOrderType.billSeparately>(sender, e.Row, ordertype.ARDocType != ARDocType.NoUpdate);
			PXUIFieldAttribute.SetEnabled<SOOrderType.invoiceNumberingID>(sender, e.Row, ordertype.ARDocType != ARDocType.NoUpdate);

			
			if (ordertype.ARDocType == ARDocType.NoUpdate)
			{
				INTranType.CustomListAttribute listattr = new INTranType.SONonARListAttribute();
				PXStringListAttribute.SetList<SOOrderTypeOperation.iNDocType>(this.operations.Cache, null, listattr.AllowedValues, listattr.AllowedLabels);
			}
			else
			{
				INTranType.CustomListAttribute listattr = new INTranType.SOListAttribute();
				PXStringListAttribute.SetList<SOOrderTypeOperation.iNDocType>(this.operations.Cache, null, listattr.AllowedValues, listattr.AllowedLabels);
			}

			SOOrder order =
			PXSelectJoin<SOOrder,
				InnerJoin<INItemPlan, On<INItemPlan.refNoteID, Equal<SOOrder.noteID>>>,
			Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
				And<Where<INItemPlan.planType, Equal<INPlanConstants.plan60>,
							 Or<INItemPlan.planType, Equal<INPlanConstants.plan66>,
							 Or<INItemPlan.planType, Equal<INPlanConstants.plan68>>>>>>>
			.SelectWindowed(this, 0, 1, ordertype.OrderType);
			PXUIFieldAttribute.SetEnabled<SOOrderType.requireLocation>(sender, ordertype, order == null && isTemplateUpdatable);
			PXUIFieldAttribute.SetEnabled<SOOrderType.requireLocation>(sender, e.Row, def.INDocType != INTranType.NoUpdate && isTemplateUpdatable);
			PXUIFieldAttribute.SetEnabled<SOOrderType.requireShipping>(sender, e.Row, def.INDocType != INTranType.NoUpdate && def.INDocType != INTranType.Transfer && isTemplateUpdatable );
			PXUIFieldAttribute.SetEnabled<SOOrderType.aRDocType>(sender, e.Row, def.INDocType != INTranType.Transfer && isTemplateUpdatable);
			PXUIFieldAttribute.SetEnabled<SOOrderType.behavior>(sender, e.Row, isTemplateUpdatable);
			PXUIFieldAttribute.SetEnabled<SOOrderType.defaultOperation>(sender, e.Row, isTemplateUpdatable && ordertype.Behavior == SOBehavior.RM);
			PXUIFieldAttribute.SetVisible<SOOrderTypeOperation.active>(operations.Cache, null, ordertype.Behavior == SOBehavior.RM || ordertype.Behavior == SOBehavior.IN);
			PXUIFieldAttribute.SetVisible<SOOrderTypeOperation.autoCreateIssueLine>(operations.Cache, null, ordertype.Behavior == SOBehavior.RM);

			PXPersistingCheck activeCheck = ordertype.Active == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing;
			PXDefaultAttribute.SetPersistingCheck<SOOrderType.salesSubMask>(sender, ordertype, activeCheck);
			//PXDefaultAttribute.SetPersistingCheck<SOOrderType.cOGSSubMask>(sender, ordertype, activeCheck);
			PXDefaultAttribute.SetPersistingCheck<SOOrderType.miscSubMask>(sender, ordertype, activeCheck);
			PXDefaultAttribute.SetPersistingCheck<SOOrderType.freightSubMask>(sender, ordertype, activeCheck);
			PXDefaultAttribute.SetPersistingCheck<SOOrderType.discSubMask>(sender, ordertype, activeCheck);
			PXDefaultAttribute.SetPersistingCheck<SOOrderType.freightAcctID>(sender, ordertype, activeCheck);
			PXDefaultAttribute.SetPersistingCheck<SOOrderType.freightSubID>(sender, ordertype, activeCheck);
			PXDefaultAttribute.SetPersistingCheck<SOOrderType.discountAcctID>(sender, ordertype, activeCheck);
			PXDefaultAttribute.SetPersistingCheck<SOOrderType.discountSubID>(sender, ordertype, activeCheck);

			PXUIFieldAttribute.SetEnabled<SOOrderType.copyLineNotesToInvoiceOnlyNS>(sender, ordertype, ordertype.CopyLineNotesToInvoice == true);
			PXUIFieldAttribute.SetEnabled<SOOrderType.copyLineFilesToInvoiceOnlyNS>(sender, ordertype, ordertype.CopyLineFilesToInvoice == true);
			PXUIFieldAttribute.SetEnabled<SOOrderType.requireAllocation>(sender, ordertype, ordertype.RequireShipping == true);

            PXUIFieldAttribute.SetEnabled<SOOrderType.useDiscountSubFromSalesSub>(sender, ordertype, ordertype.PostLineDiscSeparately == true);
		}


		protected virtual void SOOrderType_CopyLineNotesToInvoice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOOrderType row = e.Row as SOOrderType;
			if (row != null)
			{
				if (row.CopyLineNotesToInvoice != true)
					row.CopyLineNotesToInvoiceOnlyNS = false;
			}
		}

		protected virtual void SOOrderType_CopyLineFilesToInvoice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOOrderType row = e.Row as SOOrderType;
			if (row != null)
			{
				if (row.CopyLineFilesToInvoice != true)
					row.CopyLineFilesToInvoiceOnlyNS = false;
			}
		}
		protected virtual void SOOrderType_RequireShipping_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOOrderType row = e.Row as SOOrderType;
			if (row != null && row.RequireShipping == false)							
				row.RequireAllocation = false;			
		}
		protected virtual void SOOrderType_RequireAllocation_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOOrderType row = e.Row as SOOrderType;
			if (row != null && row.RequireAllocation == true)
				row.RequireLocation = false;
		}
		protected virtual void SOOrderType_RequireLocation_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOOrderType row = e.Row as SOOrderType;
			if (row != null && row.RequireLocation == true)
				row.RequireAllocation = false;
		}
        protected virtual void SOOrderType_PostLineDiscSeparately_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            SOOrderType row = e.Row as SOOrderType;
            if (row != null && row.PostLineDiscSeparately != true)
                row.UseDiscountSubFromSalesSub = false;
        }

		protected virtual void SOOrderTypeOperation_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{			
			SOOrderType ordertype = this.soordertype.Current;
			SOOrderTypeOperation operation = (SOOrderTypeOperation)e.Row;

			if(ordertype == null || operation == null) return;

			SOOrderType link = references.SelectWindowed(0, 1, ordertype.OrderType);
			bool isTemplateUpdatable = link == null && ordertype.OrderType != null &&
				(ordertype.OrderType == ordertype.Template || ordertype.Template == null);

			PXUIFieldAttribute.SetEnabled<SOOrderTypeOperation.active>(sender, e.Row, operation.Operation != ordertype.DefaultOperation);
			PXUIFieldAttribute.SetEnabled<SOOrderTypeOperation.iNDocType>(sender, e.Row, isTemplateUpdatable);
			PXUIFieldAttribute.SetEnabled<SOOrderTypeOperation.shipmentPlanType>(sender, e.Row, operation.INDocType != INTranType.NoUpdate && (bool)ordertype.RequireShipping && isTemplateUpdatable);
			PXUIFieldAttribute.SetEnabled<SOOrderTypeOperation.orderPlanType>(sender, e.Row, operation.INDocType != INTranType.NoUpdate && isTemplateUpdatable);
			PXUIFieldAttribute.SetEnabled<SOOrderTypeOperation.autoCreateIssueLine>(sender, e.Row, 
				ordertype.Behavior == SOBehavior.RM && operation.Operation == SOOperation.Receipt && isTemplateUpdatable);
		}

		protected virtual void Validate(PXCache sender, SOOrderType row)
		{
			SOOrderTypeOperation def = this.defaultOperation.Select(row.OrderType, row.DefaultOperation);
			if(def == null) return;
			if (def.INDocType != INTranType.NoUpdate && row.RequireShipping != true && row.RequireLocation != true)
			{
				PXException ex = new PXSetPropertyException(Messages.OrderTypeShipmentOrLocation);
				sender.RaiseExceptionHandling<SOOrderType.requireShipping>(row, row.RequireShipping, ex);
				sender.RaiseExceptionHandling<SOOrderType.requireLocation>(row, row.RequireLocation, ex);
			}
			short? arInvtMult = 0;
			short? inInvtMult = INTranType.InvtMult(def.INDocType);
			switch (row.ARDocType)
			{
				case ARDocType.Invoice:
				case ARDocType.DebitMemo:
				case ARDocType.CashSale:
					arInvtMult = -1;
					break;
				case ARDocType.CreditMemo:
					arInvtMult = 1;
					break;
			}

			if (row.Behavior != SOBehavior.RM && inInvtMult != arInvtMult && inInvtMult != 0 && arInvtMult != 0)
			{
				PXException ex = new PXSetPropertyException(Messages.OrderTypeUnsupportedCombination);				
				sender.RaiseExceptionHandling<SOOrderType.aRDocType>(row, row.ARDocType, ex);
				this.operations.Cache.RaiseExceptionHandling<SOOrderTypeOperation.iNDocType>(def, def.INDocType, ex);
			}
		}

		protected virtual void SOOrderType_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			SOOrderType r = e.Row as SOOrderType;
			if(r == null) return;

			if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)			
				Validate(sender, r);
			
			if (e.Operation == PXDBOperation.Insert)			
				if (r.Template == null) r.Template = r.OrderType;			
		}		
		protected virtual void SOOrderType_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			SOOrderType r = e.Row as SOOrderType;
			if(e.Exception != null && e.Operation == PXDBOperation.Insert && r != null && r.Template == r.OrderType)
				r.Template = null;
		}

		protected virtual void SOOrderType_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			SOOrderType ordertype = (SOOrderType)e.Row;

			SOOrderType link = references.SelectWindowed(0, 1, ordertype.OrderType);
			if (link != null)
			{
				throw new PXSetPropertyException(Messages.CannotDeleteTemplateOrderType, link.OrderType);
			}
		}
	}
}
