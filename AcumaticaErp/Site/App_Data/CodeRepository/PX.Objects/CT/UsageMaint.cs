using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.CR;
using System.Diagnostics;
using PX.Objects.GL;
using System.Collections;
using PX.Objects.PM;
using PX.Data.EP;

namespace PX.Objects.CT
{
	public class UsageMaint : PXGraph<UsageMaint, PX.Objects.CT.UsageMaint.UsageFilter>
    {
		#region DAC Attributes Override

		[NonStockItem]
		protected virtual void PMTran_InventoryID_CacheAttached(PXCache sender) { }

		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		[PXFieldDescription]
		[PXFormula(typeof(Selector<PMTran.inventoryID, InventoryItem.descr>))]
		protected virtual void PMTran_Description_CacheAttached(PXCache sender) { }

		[PXFormula(typeof(Selector<PMTran.inventoryID, InventoryItem.baseUnit>))]
		[PMUnit(typeof(PMTran.inventoryID))]
		protected virtual void PMTran_UOM_CacheAttached(PXCache sender) { }

		[CustomerAndProspect]
		[PXDefault(typeof(Search<Contract.customerID, Where<Contract.contractID, Equal<Current<PMTran.projectID>>>>))]
		protected virtual void PMTran_BAccountID_CacheAttached(PXCache sender)
		{ }

		[PXDBQuantity]
        [PXUIField(DisplayName = "Quantity")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		protected virtual void PMTran_BillableQty_CacheAttached(PXCache sender)
		{ }

		[PXDBInt]
		[PXDefault(typeof(UsageFilter.contractID))]
		protected virtual void PMTran_ProjectID_CacheAttached(PXCache sender)
		{ }
        
		[Branch(typeof(Search<GL.Branch.branchID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>), IsDetail = false)]
		protected virtual void PMTran_BranchID_CacheAttached(PXCache sender)
		{ }

		[PXDBBool()]
		[PXDefault(true)]
		protected virtual void PMTran_IsQtyOnly_CacheAttached(PXCache sender)
		{ }

        [PXDBString(3, IsFixed = true)]
        [PXUIField(DisplayName="Doc Type")]
        protected virtual void PMTran_ARTranType_CacheAttached(PXCache sender)
        { }

		#endregion
		
		#region Selects/Views

		public PXSelect<PMRegister> Document;
		public PXFilter<UsageFilter> Filter;
		public PXSelect<Contract, Where<Contract.contractID, Equal<Current<UsageFilter.contractID>>>> CurrentContract;
		public PXSelectJoin<
			PMTran
			, LeftJoin<EPActivity, On<EPActivity.noteID, Equal<PMTran.origRefID>>, LeftJoin<CRCase, On<CRCase.noteID, Equal<PMTran.origRefID>, Or<CRCase.noteID, Equal<EPActivity.refNoteID>>>>>
			, Where<PMTran.billed, Equal<False>, And<PMTran.projectID, Equal<Current<Contract.contractID>>>>
			> UnBilled;
		public PXSelectJoin<
			PMTran
			, LeftJoin<EPActivity, On<EPActivity.noteID, Equal<PMTran.origRefID>>, LeftJoin<CRCase, On<CRCase.noteID, Equal<PMTran.origRefID>, Or<CRCase.noteID, Equal<EPActivity.refNoteID>>>>>
			, Where<PMTran.billed, Equal<True>, And<PMTran.projectID, Equal<Current<UsageFilter.contractID>>>>
			> Billed;
		public PXSelect<ContractDetailAcum> ContractDetails;
		public PXSetup<ARSetup> arsetup;

		protected virtual IEnumerable billed()
		{
			UsageFilter filter = Filter.Current;
			if (filter == null)
			{
				return new List<PMTran>();
			}

			PXSelectBase<PMTran> select = new  PXSelectJoin<
				PMTran
				, LeftJoin<EPActivity, On<EPActivity.noteID, Equal<PMTran.origRefID>>, LeftJoin<CRCase, On<CRCase.noteID, Equal<PMTran.origRefID>, Or<CRCase.noteID, Equal<EPActivity.refNoteID>>>>>
				, Where<PMTran.billed, Equal<True>, And<PMTran.projectID, Equal<Current<UsageFilter.contractID>>>>
				>(this);

			if (filter.CustomerID != null)
			{
				select.WhereAnd<Where<PMTran.bAccountID, Equal<Current<UsageFilter.customerID>>>>();
			}
			if (filter.LocationID != null)
			{
				select.WhereAnd<Where<PMTran.locationID, Equal<Current<UsageFilter.locationID>>>>();
			}
			if (!string.IsNullOrEmpty(filter.InvFinPeriodID))
			{
				FinPeriod finPeriod = PXSelect<FinPeriod, Where<FinPeriod.finPeriodID, Equal<Current<UsageFilter.invFinPeriodID>>>>.Select(this);

				if (finPeriod != null)
					select.WhereAnd<Where<PMTran.billedDate, Between<Required<FinPeriod.startDate>, Required<FinPeriod.endDate>>>>();
			}

			return select.Select();
		}

		#endregion
        		
		public UsageMaint()
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.projectModule>())
			{
				AutoNumberAttribute.SetNumberingId<PMRegister.refNbr>(Document.Cache, arsetup.Current.UsageNumberingID);
			}
			Document.Cache.Insert();
		}
		
		#region Event Handlers
		
		protected virtual void UsageFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			UsageFilter row = e.Row as UsageFilter;
			if (row != null)
			{
				UnBilled.Cache.AllowInsert = Filter.Current.ContractID != null && (Filter.Current.ContractStatus==ContractStatus.Activated || Filter.Current.ContractStatus==ContractStatus.InUpgrade);
			}
		}
		
		protected virtual void UsageFilter_ContractID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue != null)
			{
				Contract contract = PXSelectReadonly<Contract,
					Where<Contract.contractID, Equal<Required<CRCase.contractID>>>>.Select(this, e.NewValue);

				if (contract != null)
				{
					#region Set Warnings for expired and 'In Grace Period' contracts

					int daysLeft;
					if (ContractMaint.IsExpired(contract, Accessinfo.BusinessDate.Value))
					{
						sender.RaiseExceptionHandling<UsageFilter.contractID>(e.Row, contract.ContractCD, new PXSetPropertyException(PX.Objects.CR.Messages.ContractExpired, PXErrorLevel.Warning));

					}
					else if (ContractMaint.IsInGracePeriod(contract, Accessinfo.BusinessDate.Value, out daysLeft))
					{
						sender.RaiseExceptionHandling<UsageFilter.contractID>(e.Row, contract.ContractCD, new PXSetPropertyException(PX.Objects.CR.Messages.ContractInGracePeriod, PXErrorLevel.Warning, daysLeft));
					}
					#endregion
				}

			}
		}

		
		protected virtual void PMTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Contract contract = CurrentContract.Select();
            if (contract == null) return;

            PXUIFieldAttribute.SetEnabled<PMTran.bAccountID>(sender, e.Row, contract.CustomerID == null);
		}
		
		protected virtual void PMTran_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<PMTran.locationID>(e.Row);
		}
		
		protected virtual void PMTran_BillableQty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMTran row = e.Row as PMTran;
			if (row != null && row.BillableQty != 0)
			{
				SubtractUsage(sender, row.ProjectID, row.InventoryID, (decimal?)e.OldValue, row.UOM);
				AddUsage(sender, row.ProjectID, row.InventoryID, row.BillableQty, row.UOM);
			}
		}

		protected virtual void PMTran_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMTran row = e.Row as PMTran;
			if (row != null && row.BillableQty != 0)
			{
				SubtractUsage(sender, row.ProjectID, row.InventoryID, row.BillableQty, (string)e.OldValue);
				AddUsage(sender, row.ProjectID, row.InventoryID, row.BillableQty, row.UOM);
			}
		}

        		
		#endregion

		protected void AddUsage(PXCache sender, int? contractID, int? inventoryID, decimal? used, string UOM)
		{
			if (contractID != null && inventoryID != null)
			{
				Contract contract = PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(this, contractID);

				//update all revisions starting from last active
                foreach(ContractDetailExt targetItem in PXSelectJoin<ContractDetailExt,
                    InnerJoin<ContractItem, On<ContractItem.contractItemID, Equal<ContractDetailExt.contractItemID>>,
					InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<ContractItem.recurringItemID>>>>,
                    Where<ContractDetailExt.contractID, Equal<Required<ContractDetailExt.contractID>>, And<ContractDetailExt.revID, GreaterEqual<Required<ContractDetailExt.revID>>,
					And<ContractItem.recurringItemID, Equal<Required<ContractItem.recurringItemID>>>>>>.Select(this, contractID, contract.LastActiveRevID, inventoryID))
				{
					decimal inTargetUnit = used ?? 0;
					if (!string.IsNullOrEmpty(UOM))
					{
                        inTargetUnit = INUnitAttribute.ConvertToBase(sender, inventoryID, UOM, used ?? 0, INPrecision.QUANTITY);
					}

					ContractDetailAcum item = new ContractDetailAcum();
					item.ContractDetailID = targetItem.ContractDetailID;

					item = ContractDetails.Insert(item);
					item.Used += inTargetUnit;
                    item.UsedTotal += inTargetUnit;
				}
			}
		}

		protected void SubtractUsage(PXCache sender, int? contractID, int? inventoryID, decimal? used, string UOM)
		{
			if (used != 0)
				AddUsage(sender, contractID, inventoryID, -used, UOM);
		}

		public override void Persist()
		{
			bool hasInserted = false;
			foreach (PMTran tran in UnBilled.Cache.Inserted)
			{
				hasInserted = true;
				break;
			}

			if (!hasInserted)
			{
				Document.Cache.Clear();
			}


			base.Persist();
		}
				
		#region Local Types

		[Serializable]
		public partial class UsageFilter : IBqlTable
		{
			#region ContractID
			public abstract class contractID : PX.Data.IBqlField
			{
			}
			protected Int32? _ContractID;
			[PXDBInt]
			[PXSelector(typeof(Search<Contract.contractID, Where<Contract.isTemplate, Equal<boolFalse>, And<Contract.baseType, Equal<Contract.ContractBaseType>, And<Contract.status, NotEqual<ContractStatus.ContractStatusDraft>, And<Contract.status, NotEqual<ContractStatus.ContractStatusInApproval>>>>>>), SubstituteKey = typeof(Contract.contractCD), DescriptionField = typeof(Contract.description))]
			[PXUIField(DisplayName = "Contract ID")]
			public virtual Int32? ContractID
			{
				get
				{
					return this._ContractID;
				}
				set
				{
					this._ContractID = value;
				}
			}
			#endregion

			#region InvFinPeriodID
			public abstract class invFinPeriodID : PX.Data.IBqlField
			{
			}
			protected string _InvFinPeriodID;
			[FinPeriodSelector]
			[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.Visible)]
			public virtual String InvFinPeriodID
			{
				get
				{
					return this._InvFinPeriodID;
				}
				set
				{
					this._InvFinPeriodID = value;
				}
			}
			#endregion

			#region CustomerID
			public abstract class customerID : PX.Data.IBqlField
			{
			}
			protected Int32? _CustomerID;
			[PXUIField(DisplayName = "Customer ID")]
			[Customer(DescriptionField = typeof(Customer.acctName))]
			public virtual Int32? CustomerID
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

			#region LocationID
			public abstract class locationID : PX.Data.IBqlField
			{
			}
			protected Int32? _LocationID;
			[LocationID(typeof(Where<Location.bAccountID, Equal<Current<UsageFilter.customerID>>>), DescriptionField = typeof(Location.descr))]
			[PXUIField(DisplayName = "Location ID")]
			public virtual Int32? LocationID
			{
				get
				{
					return this._LocationID;
				}
				set
				{
					this._LocationID = value;
				}
			}
			#endregion

			#region ContractStatus
			public abstract class contractStatus : PX.Data.IBqlField
			{
			}
			protected String _ContractStatus;
			[PXDBString]
			[PXFormula(typeof(Selector<UsageFilter.contractID, Contract.status>))]
			public virtual String ContractStatus
			{
				get
				{
					return this._ContractStatus;
				}
				set
				{
					this._ContractStatus = value;
				}
			}
			#endregion

		}

		#endregion
    }

	


}
