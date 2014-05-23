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
using PX.Objects.CT;
using PX.Objects.GL;

namespace PX.Objects.PM
{
	public class BillingProcess : PXGraph<BillingProcess>
	{
		public PXCancel<BillingFilter> Cancel;
		public PXFilter<BillingFilter> Filter;
	    public PXFilteredProcessing<ProjectsList, BillingFilter> Items;
        		
        protected virtual IEnumerable items()
        {
            BillingFilter filter = Filter.Current;
            if (filter == null)
            {
                yield break;
            }
            bool found = false;
            foreach (ProjectsList item in Items.Cache.Inserted)
            {
                found = true;
                yield return item;
            }
            if (found)
                yield break;

            PXSelectBase<PMProject> select = new PXSelectJoin<PMProject,
                InnerJoin<ContractBillingSchedule, On<PMProject.contractID, Equal<ContractBillingSchedule.contractID>>,
                InnerJoin<Customer, On<PMProject.customerID, Equal<Customer.bAccountID>>>>,
                Where2<Where<ContractBillingSchedule.nextDate, LessEqual<Current<BillingFilter.invoiceDate>>, Or<ContractBillingSchedule.type, Equal<BillingType.BillingOnDemand>>>,
                And<PMProject.baseType, Equal<PMProject.ProjectBaseType>,
                And<PMProject.nonProject, Equal<False>,
                And<PMProject.isTemplate, Equal<False>>>>>>(this);

            if (filter.StatementCycleId != null)
            {
                select.WhereAnd<Where<Customer.statementCycleId, Equal<Current<BillingFilter.statementCycleId>>>>();
            }
            if (filter.CustomerClassID != null)
            {
                select.WhereAnd<Where<Customer.customerClassID, Equal<Current<BillingFilter.customerClassID>>>>();
            }
            if (filter.CustomerID != null)
            {
                select.WhereAnd<Where<Customer.bAccountID, Equal<Current<BillingFilter.customerID>>>>();
            }
            if (filter.TemplateID != null)
            {
                select.WhereAnd<Where<PMProject.templateID, Equal<Current<BillingFilter.templateID>>>>();
            }

            foreach (PXResult<PMProject, ContractBillingSchedule, Customer> item in select.Select())
            {
                PMProject project = (PMProject)item;
                ContractBillingSchedule schedule = (ContractBillingSchedule)item;
                Customer customer = (Customer)item;

                ProjectsList result = new ProjectsList();
				result.ProjectID = project.ContractID;
				result.ProjectCD = project.ContractCD;
                result.Description = project.Description;
                result.CustomerID = project.CustomerID;
                result.CustomerName = customer.AcctName;
                result.LastDate = schedule.LastDate;

                DateTime? fromDate = null;

                if (schedule.NextDate != null)
                {
                    switch (schedule.Type)
                    {
                        case BillingType.Annual:
                            fromDate = schedule.NextDate.Value.AddYears(-1);
                            break;
                        case BillingType.Monthly:
                            fromDate = schedule.NextDate.Value.AddMonths(-1);
                            break;
						case BillingType.Weekly:
							fromDate = schedule.NextDate.Value.AddDays(-7);
							break;
                        case BillingType.Quarterly:
                            fromDate = schedule.NextDate.Value.AddMonths(-3);
                            break;
                    }
                }

                result.FromDate = fromDate;
                result.NextDate = schedule.NextDate;

                yield return Items.Insert(result);


            }

            Items.Cache.IsDirty = false;
        }
        
		public BillingProcess()
		{
          	Items.SetProcessCaption(PM.Messages.ProcBill);
			Items.SetProcessAllCaption(PM.Messages.ProcBillAll);
		}
		        
		#region EventHandlers
		protected virtual void BillingFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			if ( !cache.ObjectsEqual<BillingFilter.invoiceDate, BillingFilter.invFinPeriodID, BillingFilter.statementCycleId, BillingFilter.customerClassID, BillingFilter.customerID, BillingFilter.templateID>(e.Row, e.OldRow) )
				Items.Cache.Clear();
		}
        protected virtual void BillingFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            BillingFilter filter = Filter.Current;

            Items.SetProcessDelegate(
                    delegate(ProjectsList item)
                    	{
                    		PMBillEngine engine = PXGraph.CreateInstance<PMBillEngine>();
							if (!engine.Bill(item.ProjectID, filter.InvoiceDate))
							{
								throw new PXSetPropertyException(Warnings.NothingToBill, PXErrorLevel.RowWarning);
							}
                    });
        }
		#endregion

				
        [Serializable]
        public partial class BillingFilter : IBqlTable
        {
            #region InvoiceDate
            public abstract class invoiceDate : PX.Data.IBqlField
            {
            }
            protected DateTime? _InvoiceDate;
            [PXDBDate()]
            [PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "Invoice Date", Visibility = PXUIVisibility.Visible, Required = true)]
            public virtual DateTime? InvoiceDate
            {
                get
                {
                    return this._InvoiceDate;
                }
                set
                {
                    this._InvoiceDate = value;
                }
            }
            #endregion

            #region InvFinPeriodID
            public abstract class invFinPeriodID : PX.Data.IBqlField
            {
            }
            protected string _InvFinPeriodID;
            [OpenPeriod(typeof(BillingFilter.invoiceDate))]
            [PXUIField(DisplayName = "Period to Post", Visibility = PXUIVisibility.Visible, Required = true)]
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

            #region StatementCycleId
            public abstract class statementCycleId : PX.Data.IBqlField
            {
            }
            protected String _StatementCycleId;
            [PXDBString(10, IsUnicode = true)]
            [PXUIField(DisplayName = "Statement Cycle ID")]
            [PXSelector(typeof(ARStatementCycle.statementCycleId))]
            public virtual String StatementCycleId
            {
                get
                {
                    return this._StatementCycleId;
                }
                set
                {
                    this._StatementCycleId = value;
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

            #region TemplateID
            public abstract class templateID : PX.Data.IBqlField
            {
            }
            protected Int32? _TemplateID;
            [ActiveProject(typeof(Where<PMProject.isTemplate, Equal<True>>), DisplayName = "Template ID")]
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
        }

		[Serializable]
        public partial class ProjectsList : IBqlTable
        {
            #region Selected
            public abstract class selected : IBqlField
            {
            }
            protected bool? _Selected = false;
            [PXBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Selected")]
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

			#region ProjectID
			public abstract class projectID : PX.Data.IBqlField
			{
			}
			protected int? _ProjectID;
			[PXDBInt(IsKey = true)]
			public virtual int? ProjectID
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

			#region ProjectCD
			public abstract class projectCD : PX.Data.IBqlField
			{
			}
			protected string _ProjectCD;
			[PXDBString]
			[PXUIField(DisplayName = "Project")]
			public virtual string ProjectCD
			{
				get
				{
					return this._ProjectCD;
				}
				set
				{
					this._ProjectCD = value;
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
            [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
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
            [PXUIField(DisplayName = "Last Billing Date", Enabled = false)]
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

            #region FromDate
            public abstract class fromDate : PX.Data.IBqlField
            {
            }
            protected DateTime? _FromDate;
            [PXDBDate()]
            [PXUIField(DisplayName = "From")]
            public virtual DateTime? FromDate
            {
                get
                {
                    return this._FromDate;
                }
                set
                {
                    this._FromDate = value;
                }
            }
            #endregion

            #region NextDate
            public abstract class nextDate : PX.Data.IBqlField
            {
            }
            protected DateTime? _NextDate;
            [PXDBDate()]
            [PXUIField(DisplayName = "To")]
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
        }
	}
}
