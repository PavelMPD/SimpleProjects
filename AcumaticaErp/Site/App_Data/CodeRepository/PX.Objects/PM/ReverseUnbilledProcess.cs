using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using System.Collections;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.EP;

namespace PX.Objects.PM
{
    [Serializable]
	public class ReverseUnbilledProcess : PXGraph<ReverseUnbilledProcess>
	{
        [ProjectTask(typeof(PMTran.projectID))]
        protected virtual void PMTran_TaskID_CacheAttached(PXCache sender)
        {
        }

		public PXCancel<TranFilter> Cancel;
		public PXFilter<TranFilter> Filter;
		public PXFilteredProcessingJoin<PMTran, TranFilter,
			InnerJoin<PMRegister, On<PMRegister.module, Equal<PMTran.tranType>, And<PMRegister.refNbr, Equal<PMTran.refNbr>>>,
			InnerJoin<PMProject, On<PMProject.contractID, Equal<PMTran.projectID>>,
			InnerJoin<PMTask, On<PMTask.projectID, Equal<PMTran.projectID>, And<PMTask.taskID, Equal<PMTran.taskID>>>,
			LeftJoin<Customer, On<Customer.bAccountID, Equal<PMProject.customerID>>>>>>> Items;
		protected List<string> billable;

		protected virtual IEnumerable items()
		{
			
			PXSelectBase<PMTran> select = new PXSelectJoin<PMTran,
				InnerJoin<PMRegister, On<PMRegister.module, Equal<PMTran.tranType>, And<PMRegister.refNbr, Equal<PMTran.refNbr>>>,
				InnerJoin<PMProject, On<PMProject.contractID, Equal<PMTran.projectID>>,
				InnerJoin<PMTask, On<PMTask.projectID, Equal<PMTran.projectID>, And<PMTask.taskID, Equal<PMTran.taskID>>>,
				LeftJoin<Customer, On<Customer.bAccountID, Equal<PMProject.customerID>>>>>>,
				Where<PMTran.allocated, Equal<True>,
				And<PMTran.billed, Equal<False>,
				And<PMTran.released, Equal<True>,
				And<PMTran.reversed, Equal<False>>>>>>(this);

			TranFilter filter = this.Filter.Current;
			if (filter != null)
			{
				if (Filter.Current.BillingID != null)
				{
					select.WhereAnd<Where<PMTran.billingID, Equal<Current<TranFilter.billingID>>>>();
				}

				if (Filter.Current.ProjectID != null)
				{
					select.WhereAnd<Where<PMTran.projectID, Equal<Current<TranFilter.projectID>>>>();

					if (Filter.Current.ProjectTaskID != null)
					{
						select.WhereAnd<Where<PMTran.taskID, Equal<Current<TranFilter.projectTaskID>>>>();
					}
				}

				if (Filter.Current.CustomerID != null)
				{
					select.WhereAnd<Where<PMProject.customerID, Equal<Current<TranFilter.customerID>>>>();
				}

				if (Filter.Current.CustomerClassID != null)
				{
					select.WhereAnd<Where<Customer.customerClassID, Equal<Current<TranFilter.customerClassID>>>>();
				}
				if (filter.InventoryID != null)
				{
					select.WhereAnd<Where<PMTran.inventoryID, Equal<Current<TranFilter.inventoryID>>>>();
				}
				if (filter.ResourceID != null)
				{
					select.WhereAnd<Where<PMTran.resourceID, Equal<Current<TranFilter.resourceID>>>>();
				}



				if (filter.DateFrom != null && filter.DateTo != null)
				{
					if (filter.DateFrom == filter.DateTo)
					{
						select.WhereAnd<Where<PMTran.date, Equal<Current<TranFilter.dateFrom>>>>();
					}
					else
					{
						select.WhereAnd<Where<PMTran.date, Between<Current<TranFilter.dateFrom>, Current<TranFilter.dateTo>>>>();
					}
				}
				else if (filter.DateFrom != null)
				{
					select.WhereAnd<Where<PMTran.date, GreaterEqual<Current<TranFilter.dateFrom>>>>();
				}
				else if (filter.DateTo != null)
				{
					select.WhereAnd<Where<PMTran.date, LessEqual<Current<TranFilter.dateTo>>>>();
				}
			}

			foreach (PXResult<PMTran, PMRegister, PMProject, PMTask, Customer> res in select.Select())
			{
				PMTran tran = (PMTran)res;
				PMTask task = (PMTask)res;

				if (!IsUnbilled(tran, task.BillingID))
					continue;

				yield return res;
			}
		}

		public ReverseUnbilledProcess()
		{			
		}

		public PXAction<TranFilter> viewDocument;
		[PXUIField(DisplayName = SO.Messages.ViewDocument, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXButton]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			RegisterEntry graph = CreateInstance<RegisterEntry>();
			graph.Document.Current = graph.Document.Search<PMRegister.refNbr>(Items.Current.RefNbr, Items.Current.TranType);
			throw new PXRedirectRequiredException(graph, "PMTransactiosn") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}


		#region EventHandlers
		protected virtual void TranFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			Items.Cache.Clear();
		}
		protected virtual void TranFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			TranFilter filter = Filter.Current;

			Items.SetProcessDelegate(ReverseAllocatedTran);
		}
		#endregion

		public static void ReverseAllocatedTran(PMTran tran)
		{
			RegisterEntry pmEntry = PXGraph.CreateInstance<RegisterEntry>();
            pmEntry.FieldVerifying.AddHandler<PMTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
            pmEntry.FieldVerifying.AddHandler<PMTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
            pmEntry.FieldVerifying.AddHandler<PMTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

			PMRegister reversalDoc = (PMRegister)pmEntry.Document.Cache.Insert();
			reversalDoc.OrigDocType = PMOrigDocType.AllocationReversal;
			reversalDoc.OrigDocNbr = tran.RefNbr;
			reversalDoc.Description = "Allocation Reversal to Non-Billable";
			pmEntry.Document.Current = reversalDoc;

			PMBillEngine engine = PXGraph.CreateInstance<PMBillEngine>();
			foreach (PMTran reverse in engine.ReverseTran(tran))
			{
				reverse.Reversed = true;
				pmEntry.Transactions.Insert(reverse);
			}

			tran.Reversed = true;
			pmEntry.Transactions.Update(tran);

			pmEntry.Save.Press();

			PMSetup setup = PXSelect<PMSetup>.Select(pmEntry);
			if (setup.AutoReleaseAllocation == true)
			{
				RegisterRelease.Release(reversalDoc);
			}
		}

		protected virtual bool IsUnbilled(PMTran tran, string billingID)
		{
			if ( billable == null )
			{
				PXResultset<PMBillingRule> rules = PXSelect<PMBillingRule>.Select(this);
				billable = new List<string>(rules.Count);

				foreach ( PMBillingRule rule in rules )
				{
					billable.Add( string.Format("{0}.{1}", rule.BillingID, rule.AccountGroupID) );
				}
			}

			return billable.Contains(string.Format("{0}.{1}", billingID, tran.AccountGroupID));


		}

        [Serializable]
		public partial class TranFilter : IBqlTable
		{
			#region BillingID
			public abstract class billingID : PX.Data.IBqlField
			{
			}
			protected String _BillingID;
			[PXSelector(typeof(PMBilling.billingID))]
			[PXDBString(PMBilling.billingID.Length, IsUnicode = true)]
			[PXUIField(DisplayName = "Allocation Rule")]
			public virtual String BillingID
			{
				get
				{
					return this._BillingID;
				}
				set
				{
					this._BillingID = value;
				}
			}
			#endregion
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
			#region CustomerID
			public abstract class customerID : PX.Data.IBqlField
			{
			}
			protected Int32? _CustomerID;
			[Customer()]
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
			#region ProjectID
			public abstract class projectID : PX.Data.IBqlField
			{
			}
			protected Int32? _ProjectID;
			[Project()]
			public virtual Int32? ProjectID
			{
				get
				{
					return this._ProjectID;
				}
				set
				{
					this._ProjectID = value;
				}
			}
			#endregion
			#region ProjectTaskID
			public abstract class projectTaskID : PX.Data.IBqlField
			{
			}
			protected Int32? _ProjectTaskID;
			[ProjectTask(typeof(TranFilter.projectID))]
			public virtual Int32? ProjectTaskID
			{
				get
				{
					return this._ProjectTaskID;
				}
				set
				{
					this._ProjectTaskID = value;
				}
			}
			#endregion
			#region InventoryID
			public abstract class inventoryID : PX.Data.IBqlField
			{
			}
			protected Int32? _InventoryID;
			[Inventory]
			public virtual Int32? InventoryID
			{
				get
				{
					return this._InventoryID;
				}
				set
				{
					this._InventoryID = value;
				}
			}
			#endregion
			#region ResourceID
			public abstract class resourceID : PX.Data.IBqlField
			{
			}
			protected Int32? _ResourceID;
			[PXEPEmployeeSelector]
			[PXDBInt()]
			[PXUIField(DisplayName = "Employee")]
			public virtual Int32? ResourceID
			{
				get
				{
					return this._ResourceID;
				}
				set
				{
					this._ResourceID = value;
				}
			}
			#endregion
			#region DateFrom
			public abstract class dateFrom : PX.Data.IBqlField
			{
			}
			protected DateTime? _DateFrom;
			[PXDBDate()]
			[PXUIField(DisplayName = "From", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? DateFrom
			{
				get
				{
					return this._DateFrom;
				}
				set
				{
					this._DateFrom = value;
				}
			}
			#endregion
			#region DateTo
			public abstract class dateTo : PX.Data.IBqlField
			{
			}
			protected DateTime? _DateTo;
			[PXDBDate()]
			[PXUIField(DisplayName = "To", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? DateTo
			{
				get
				{
					return this._DateTo;
				}
				set
				{
					this._DateTo = value;
				}
			}
			#endregion
		}
				
	}
}
