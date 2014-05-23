using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using System.Collections;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.CT
{	
	[TableAndChartDashboardType]
	public class RenewContracts : PXGraph<RenewContracts>
	{
		public PXCancel<ExpiringContractFilter> Cancel;
		public PXFilter<ExpiringContractFilter> Filter;
		public PXFilteredProcessing<ContractsList, ExpiringContractFilter> Items;

		public RenewContracts()
		{
			Items.SetSelected<ContractsList.selected>();
		}

		protected virtual IEnumerable items()
		{
			ExpiringContractFilter filter = Filter.Current;
			if (filter == null)
			{
				yield break;
			}
			bool found = false;
			foreach (ContractsList item in Items.Cache.Inserted)
			{
				found = true;
				yield return item;
			}
			if (found)
				yield break;


			PXSelectBase<Contract> select = new PXSelectJoin<Contract
								, InnerJoin<ContractBillingSchedule, On<Contract.contractID, Equal<ContractBillingSchedule.contractID>>
								, InnerJoin<Customer, On<Customer.bAccountID, Equal<Contract.customerID>>>>,
								Where<Contract.isTemplate, Equal<boolFalse>,
								And<Contract.baseType, Equal<Contract.ContractBaseType>,
								And<Contract.expireDate, LessEqual<Current<ExpiringContractFilter.endDate>>,
								And<Contract.type, NotEqual<ContractType.ContractUnlimited>,
								And<Contract.autoRenew, Equal<boolTrue>,
                                And<Where<Contract.status, Equal<ContractStatus.ContractStatusActivated>, Or<Contract.status, Equal<ContractStatus.ContractStatusExpired>>>>>>>>>>(this);

			
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
			foreach (PXResult<Contract, ContractBillingSchedule, Customer> resultSet in select.Select())
			{
				Contract contract = (Contract)resultSet;
				ContractBillingSchedule schedule = (ContractBillingSchedule)resultSet;
				Customer customer = (Customer)resultSet;

				bool skipItem = false;
				if (contract.Type == ContractType.Expiring)
				{
					Contract child = PXSelect<Contract, Where<Contract.originalContractID, Equal<Required<Contract.originalContractID>>>>.Select(this, contract.ContractID);
					skipItem = child != null;
				}

				if (!skipItem)
				{
					ContractsList result = new ContractsList();
					result.ContractID = contract.ContractID;
					result.Description = contract.Description;
					result.Type = contract.Type;
					result.ExpireDate = contract.ExpireDate;
					result.CustomerID = contract.CustomerID;
					result.CustomerName = customer.AcctName;
					result.LastDate = schedule.LastDate;
					result.NextDate = schedule.NextDate;
					result.ExpireDate = contract.ExpireDate;
					result.TemplateID = contract.TemplateID;
					result.Status = contract.Status;
					result.StartDate = contract.StartDate;

					yield return Items.Insert(result);
				}
			}

			Items.Cache.IsDirty = false;
		}

		#region Actions

		public PXAction<ExpiringContractFilter> viewContract;
		[PXUIField(DisplayName = Messages.ViewContract, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Inquiry)]
		public virtual IEnumerable ViewContract(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				ContractMaint target = PXGraph.CreateInstance<ContractMaint>();
				target.Clear();
				target.Contracts.Current = PXSelect<CT.Contract, Where<CT.Contract.contractID, Equal<Current<ContractsList.contractID>>>>.Select(this);
				throw new PXRedirectRequiredException(target, true, "ViewContract"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
			}

			return adapter.Get();
		}

		#endregion
			
		#region EventHandlers
		protected virtual void ExpiringContractFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			Items.Cache.Clear();
		}

		protected virtual void ExpiringContractFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			ExpiringContractFilter filter = Filter.Current;

			Items.SetProcessDelegate<ContractMaint>(RenewContract);
		}

		public virtual void ExpiringContractFilter_BeginDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Accessinfo.BusinessDate;
			e.Cancel = true;
		}

        [PXDBDate()]
        [PXUIField(DisplayName = "Setup Date", Visibility = PXUIVisibility.Visible)]
        protected virtual void ContractsList_StartDate_CacheAttached(PXCache sender)
        {
        }
		
		#endregion

		public static void RenewContract(ContractMaint docgraph, ContractsList item)
		{
			docgraph.Contracts.Current = PXSelect<CT.Contract, Where<CT.Contract.contractID, Equal<Required<CT.Contract.contractID>>>>.Select(docgraph, item.ContractID);
			docgraph.Billing.Current = docgraph.Billing.Select();
			docgraph.RenewContract();
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
			[PXUIField(DisplayName = "Show Auto Renewable Contracts")]
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
				
		[Serializable]
		public partial class ContractsList : IBqlTable
		{
			#region Selected
			public abstract class selected : IBqlField
			{
			}
			protected bool? _Selected = false;
			[PXBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Visible)]
			public bool? Selected
			{
				get
				{
					return _Selected;
				}
				set
				{
					_Selected = value;
				}
			}
			#endregion

			#region ContractID
			public abstract class contractID : PX.Data.IBqlField
			{
			}
			protected Int32? _ContractID;
			[Contract(IsKey = true)]
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

			#region CustomerID
			public abstract class customerID : PX.Data.IBqlField
			{
			}
			protected Int32? _CustomerID;
			[PXDBInt()]
			[PXUIField(DisplayName = "Customer ID", Visibility = PXUIVisibility.Visible)]
			[PXSelector(typeof(Customer.bAccountID), SubstituteKey = typeof(Customer.acctCD))]
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

			#region CustomerName
			public abstract class customerName : PX.Data.IBqlField
			{
			}
			protected string _CustomerName;
			[PXDBString(60, IsUnicode = true)]
			[PXUIField(DisplayName = "Customer Name", Visibility = PXUIVisibility.Visible)]
			public virtual string CustomerName
			{
				get
				{
					return this._CustomerName;
				}
				set
				{
					this._CustomerName = value;
				}
			}
			#endregion

			#region Description
			public abstract class description : PX.Data.IBqlField
			{
			}
			protected String _Description;
			[PXDBString(60, IsUnicode = true)]
			[PXDefault()]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible)]
			public virtual String Description
			{
				get
				{
					return this._Description;
				}
				set
				{
					this._Description = value;
				}
			}
			#endregion

			#region LastDate
			public abstract class lastDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _LastDate;
			[PXDBDate()]
			[PXUIField(DisplayName = "Last Billing Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? LastDate
			{
				get
				{
					return this._LastDate;
				}
				set
				{
					this._LastDate = value;
				}
			}
			#endregion

			#region NextDate
			public abstract class nextDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _NextDate;
			[PXDBDate()]
			[PXUIField(DisplayName = "Next Billing Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? NextDate
			{
				get
				{
					return this._NextDate;
				}
				set
				{
					this._NextDate = value;
				}
			}
			#endregion

			#region ExpireDate
			public abstract class expireDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _ExpireDate;
			[PXDBDate()]
			[PXUIField(DisplayName = "Expiration Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? ExpireDate
			{
				get
				{
					return this._ExpireDate;
				}
				set
				{
					this._ExpireDate = value;
				}
			}
			#endregion

			#region Type
			public abstract class type : PX.Data.IBqlField
			{
			}
			protected String _Type;
			[PXDBString(1, IsFixed = true)]
			[PXUIField(DisplayName = "Contract Type", Visibility = PXUIVisibility.Visible)]
			[ContractType.List()]
			[PXDefault(ContractType.Renewable)]
			public virtual String Type
			{
				get
				{
					return this._Type;
				}
				set
				{
					this._Type = value;
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

			#region Status
			public abstract class status : PX.Data.IBqlField
			{
			}
			protected String _Status;
			[PXDBString(1, IsFixed = true)]
			[ContractStatus.List()]
			[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.Visible)]
			public virtual String Status
			{
				get
				{
					return this._Status;
				}
				set
				{
					this._Status = value;
				}
			}
			#endregion

			#region StartDate
			public abstract class startDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _StartDate;
			[PXDBDate()]
			[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible)]
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

		}

		#endregion
	}
}
