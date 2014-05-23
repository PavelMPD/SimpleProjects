using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using System.Collections;
using PX.Objects.GL;

namespace PX.Objects.CT
{
	[TableAndChartDashboardType]
	public class ExpiringContractsEng : PXGraph<ExpiringContractsEng>
	{
		public PXFilter<ExpiringContractFilter> Filter;
		public PXCancel<ExpiringContractFilter> Cancel;
		public PXSelectJoin<Contract, InnerJoin<ContractBillingSchedule, On<ContractBillingSchedule.contractID, Equal<Contract.contractID>>
			, InnerJoin<Customer, On<Customer.bAccountID, Equal<Contract.customerID>>>>,
			Where<Contract.isTemplate, Equal<boolFalse>,
			And<Contract.baseType, Equal<Contract.ContractBaseType>,
			And<Contract.type, Equal<ContractType.ContractExpiring>>>>> Contracts;

		public ExpiringContractsEng()
		{
			Contracts.Cache.AllowDelete = false;
			Contracts.Cache.AllowUpdate = false;
			Contracts.Cache.AllowInsert = false;
		}

		public virtual IEnumerable contracts()
		{
			ExpiringContractFilter filter = this.Filter.Current;
			if (filter != null)
			{
				PXSelectBase<Contract> select = new PXSelectJoin<Contract
					, InnerJoin<ContractBillingSchedule, On<Contract.contractID, Equal<ContractBillingSchedule.contractID>>
					, InnerJoin<Customer, On<Customer.bAccountID, Equal<Contract.customerID>>>>,
					Where<Contract.isTemplate, Equal<boolFalse>,
					And<Contract.baseType, Equal<Contract.ContractBaseType>,
					And<Contract.expireDate, LessEqual<Current<ExpiringContractFilter.endDate>>,
					And<Contract.status, NotEqual<ContractStatus.ContractStatusCanceled>>>>>>(this);
				
				if (filter.ShowAutoRenewable == false)
				{
					select.WhereAnd<Where<Contract.autoRenew, Equal<boolFalse>>>();
				}
				
				if (!string.IsNullOrEmpty(filter.CustomerClassID))
				{
					select.WhereAnd<Where<Customer.customerClassID, Equal<Current<ExpiringContractFilter.customerClassID>>>>();
				}

				if (filter.TemplateID != null)
				{
					select.WhereAnd<Where<Contract.templateID, Equal<Current<ExpiringContractFilter.templateID>>>>();
				}

				/*
				 * Expiring Contracts has a hierarchical structure and we need to show only the latest expiring node hidding
				 * all of its original contracts
				*/
				foreach ( PXResult<Contract> result in select.Select())
				{
					bool skipItem = false;
					if (((Contract)result).Type == ContractType.Expiring)
					{
						Contract child = PXSelect<Contract, Where<Contract.originalContractID, Equal<Required<Contract.originalContractID>>>>.Select(this, ((Contract)result).ContractID);
						skipItem = child != null;
					}

					if (!skipItem)
						yield return result;
				}
			}
			else
				yield break;

		}

		#region Actions

		public PXAction<ExpiringContractFilter> viewContract;
		[PXUIField(DisplayName = Messages.ViewContract, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewContract(PXAdapter adapter)
		{
			if (Contracts.Current != null)
			{
				ContractMaint target = PXGraph.CreateInstance<ContractMaint>();
				target.Clear();
				target.Contracts.Current = Contracts.Current;
				throw new PXRedirectRequiredException(target, true, "ViewContract"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
			}

			return adapter.Get();
		}

		#endregion

		public virtual void ExpiringContractFilter_BeginDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Accessinfo.BusinessDate;
			e.Cancel = true;
		}

		#region Local Types
		[Serializable]
		public partial class ExpiringContractFilter : IBqlTable
		{
			#region CustomerClassID
			public abstract class customerClassID : PX.Data.IBqlField
			{
			}
			protected String _CustomerClassID;
			[PXDBString(10, IsUnicode = true)]
			[PXSelector(typeof(CustomerClass.customerClassID), DescriptionField = typeof(CustomerClass.descr), CacheGlobal = true)]
			[PXUIField(DisplayName = "Customer Class")]
			public virtual String CustomerClassID
			{
				get
				{
					return this._CustomerClassID;
				}
				set
				{
					this._CustomerClassID = value;
				}
			}
			#endregion
			#region TemplateID
			public abstract class templateID : PX.Data.IBqlField
			{
			}
			protected Int32? _TemplateID;
			[ContractTemplate]
			public virtual Int32? TemplateID
			{
				get
				{
					return this._TemplateID;
				}
				set
				{
					this._TemplateID = value;
				}
			}
			#endregion
			#region BeginDate
			public abstract class beginDate : PX.Data.IBqlField
			{
			}

			protected DateTime? _BeginDate;

			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Start Date")]
			public virtual DateTime? BeginDate
			{
				get
				{
					return this._BeginDate;
				}
				set
				{
					this._BeginDate = value;
				}
			}
			#endregion
			#region EndDate
			public abstract class endDate : PX.Data.IBqlField
			{
			}
			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			public virtual DateTime? EndDate
			{
				get
				{
					if (this._BeginDate.HasValue)
						return _BeginDate.Value.AddDays(_ExpireXDays ?? 0);
					return this._BeginDate;
				}
				set
				{
				}
			}
			#endregion
			#region ExpireXDays
			public abstract class expireXDays : PX.Data.IBqlField
			{
			}

			protected Int32? _ExpireXDays;

			[PXDBInt()]
			[PXDefault(30)]
			[PXUIField(DisplayName = "Duration")]
			public virtual Int32? ExpireXDays
			{
				get
				{
					return this._ExpireXDays;
				}
				set
				{
					this._ExpireXDays = value;
				}
			}
			#endregion
			#region ShowAutoRenewable
			public abstract class showAutoRenewable : PX.Data.IBqlField
			{
			}
			protected Boolean? _ShowAutoRenewable;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Show Auto-Renewable Contracts")]
			public virtual Boolean? ShowAutoRenewable
			{
				get
				{
					return this._ShowAutoRenewable;
				}
				set
				{
					this._ShowAutoRenewable = value;
				}
			}
			#endregion
			#region ActiveOnly
			public abstract class activeOnly : PX.Data.IBqlField
			{
			}
			protected Boolean? _ActiveOnly;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Show Active Only")]
			public virtual Boolean? ActiveOnly
			{
				get
				{
					return this._ActiveOnly;
				}
				set
				{
					this._ActiveOnly = value;
				}
			}
			#endregion

		}
				
		#endregion
	}

	
}
