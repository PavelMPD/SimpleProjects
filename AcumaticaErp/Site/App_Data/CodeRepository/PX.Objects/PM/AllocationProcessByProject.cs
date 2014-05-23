using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using System.Diagnostics;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.AP;
using PX.Reports.Parser;
using PX.Objects.IN;
using PX.Objects.CS;
using System.Collections;
using PX.Objects.GL;
using System.Collections.Specialized;

namespace PX.Objects.PM
{
	public class AllocationProcessByProject : PXGraph<AllocationProcessByProject>
	{
		public PXCancel<AllocationFilter> Cancel;
		public PXFilter<AllocationFilter> Filter;
		public PXFilteredProcessingJoin<PMProject,
			AllocationFilter,
			LeftJoin<Customer, On<PMProject.customerID, Equal<Customer.bAccountID>>>,
			Where<PMProject.baseType, Equal<PMProject.ProjectBaseType>,  
			And<PMProject.isTemplate, Equal<False>, 
			And<PMProject.isActive, Equal<True>, 
			And<PMProject.nonProject, Equal<False>, 
			And2<Where<Current<AllocationFilter.allocationID>, IsNull, Or<PMProject.allocationID, Equal<Current<AllocationFilter.allocationID>>>>,
			And2<Where<Current<AllocationFilter.projectID>, IsNull, Or<PMProject.contractID, Equal<Current<AllocationFilter.projectID>>>>,
			And2<Where<Current<AllocationFilter.customerID>, IsNull, Or<PMProject.customerID, Equal<Current<AllocationFilter.customerID>>>>,
			And<Where<Current<AllocationFilter.customerClassID>, IsNull, Or<Customer.customerClassID, Equal<Current<AllocationFilter.customerClassID>>>>>>>>>>>>> Items;


		public AllocationProcessByProject()
		{
			Items.SetProcessCaption(PM.Messages.ProcAllocate);
			Items.SetProcessAllCaption(PM.Messages.ProcAllocateAll);
		}

		#region EventHandlers
		protected virtual void AllocationFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			if (!cache.ObjectsEqual<AllocationFilter.date, AllocationFilter.allocationID, AllocationFilter.customerClassID, AllocationFilter.customerID, AllocationFilter.projectID, AllocationFilter.taskID>(e.Row, e.OldRow))
				Items.Cache.Clear();
		}
		protected virtual void AllocationFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			AllocationFilter filter = Filter.Current;

			Items.SetProcessDelegate(
					delegate(PMProject item)
					{
						PMAllocator engine = PXGraph.CreateInstance<PMAllocator>();
						Run(engine, item, filter.Date, filter.DateFrom, filter.DateTo);
					});
		}
		#endregion       


		private PMSetup setup;

		public bool AutoReleaseAllocation
		{
			get
			{
				if (setup == null)
				{
					setup = PXSelect<PMSetup>.Select(this);
				}

				return setup.AutoReleaseAllocation == true;
			}
		}

		public static void Run(PMAllocator graph, PMProject item, DateTime? date, DateTime? fromDate, DateTime? toDate)
		{
			PXSelectBase<PMTask> select = new PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
				And<PMTask.allocationID, IsNotNull,
				And<PMTask.isActive, Equal<True>>>>>(graph);

			List<PMTask> tasks = new List<PMTask>();
			foreach (PMTask pmTask in select.Select(item.ContractID))
			{
				tasks.Add(pmTask);
			}

			graph.Clear();
			graph.OverrideAllocationDate = date;
		    graph.FilterStartDate = fromDate;
		    graph.FilterEndDate = toDate;
			graph.Execute(tasks);
			if (graph.Document.Current != null)
			{
                graph.Actions.PressSave();
				PMSetup setup = PXSelect<PMSetup>.Select(graph);
				PMRegister doc = graph.Caches[typeof(PMRegister)].Current as PMRegister;
				if (doc != null && setup.AutoReleaseAllocation == true)
				{
					RegisterRelease.Release(doc);
				}

			}
			else
			{
				throw new PXSetPropertyException(Warnings.NothingToAllocate, PXErrorLevel.RowWarning);
			}

		}

		[Serializable]
		public partial class AllocationFilter : IBqlTable
		{
			#region Date
			public abstract class date : PX.Data.IBqlField
			{
			}
			protected DateTime? _Date;
			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Posting Date", Visibility = PXUIVisibility.Visible, Required = true)]
			public virtual DateTime? Date
			{
				get
				{
					return this._Date;
				}
				set
				{
					this._Date = value;
				}
			}
			#endregion
			#region AllocationID
			public abstract class allocationID : PX.Data.IBqlField
			{
			}
			protected String _AllocationID;
			[PXSelector(typeof(PMAllocation.allocationID))]
			[PXDBString(15, IsUnicode = true)]
			[PXUIField(DisplayName = "Allocation Rule")]
			public virtual String AllocationID
			{
				get
				{
					return this._AllocationID;
				}
				set
				{
					this._AllocationID = value;
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
			#region TaskID
			public abstract class taskID : PX.Data.IBqlField
			{
			}
			protected Int32? _TaskID;
			[ProjectTask(typeof(AllocationFilter.projectID))]
			public virtual Int32? TaskID
			{
				get
				{
					return this._TaskID;
				}
				set
				{
					this._TaskID = value;
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
