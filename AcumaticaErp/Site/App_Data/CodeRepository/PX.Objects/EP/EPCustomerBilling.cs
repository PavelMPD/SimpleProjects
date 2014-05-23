using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.BQLConstants;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.AR;
using PX.Objects.GL;
using PX.Objects.CT;
using PX.Objects.PM;
using PX.Objects.IN;

namespace PX.Objects.EP
{
	[TableDashboardType]
	public class EPCustomerBilling : PXGraph<EPCustomerBilling>
	{
		public PXCancel<BillingFilter> Cancel;
		public PXFilter<BillingFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessing<CustomersList, BillingFilter> Customers;


		public EPCustomerBilling()
		{
			Customers.SetProcessCaption(Messages.Process);
			Customers.SetProcessAllCaption(Messages.ProcessAll);
			Customers.SetSelected<CustomersList.selected>();
		}

		protected virtual IEnumerable customers()
		{
			BillingFilter filter = Filter.Current;
			if (filter == null)
			{
				yield break;
			}
			bool found = false;
			foreach (CustomersList item in Customers.Cache.Inserted)
			{
				found = true;
				yield return item;
			}
			if (found)
			{
				yield break;
			}

			PXSelectBase<EPExpenseClaimDetails> sel = new PXSelectJoinGroupBy<EPExpenseClaimDetails, InnerJoin<Customer, On<EPExpenseClaimDetails.customerID, Equal<Customer.bAccountID>>,
																									 LeftJoin<Contract, On<EPExpenseClaimDetails.contractID, Equal<Contract.contractID>>>>,
																									 Where<EPExpenseClaimDetails.released, Equal<boolTrue>,
																									 And<EPExpenseClaimDetails.billable, Equal<boolTrue>,
																									 And<EPExpenseClaimDetails.billed, Equal<boolFalse>,
																									 And<EPExpenseClaimDetails.expenseDate, LessEqual<Current<BillingFilter.endDate>>>>>>,
																									 Aggregate<GroupBy<EPExpenseClaimDetails.customerID,
																											   GroupBy<EPExpenseClaimDetails.customerLocationID>>>>(this);
			if (filter.CustomerClassID != null)
			{
				sel.WhereAnd<Where<Customer.customerClassID, Equal<Current<BillingFilter.customerClassID>>>>();
			}
			if (filter.CustomerID != null)
			{
				sel.WhereAnd<Where<Customer.bAccountID, Equal<Current<BillingFilter.customerID>>>>();
			}

			foreach (PXResult<EPExpenseClaimDetails, Customer, Contract> res in sel.Select())
			{
				CustomersList retitem = new CustomersList();
				Customer customer = res;
				EPExpenseClaimDetails claimdetaisl = res;
				Contract contract = (Contract)res;

				if (contract.BaseType == PMProject.ProjectBaseType.Project)
					continue;


				retitem.CustomerID = customer.BAccountID;
				retitem.LocationID = claimdetaisl.CustomerLocationID;
				retitem.CustomerClassID = customer.CustomerClassID;

				retitem.Selected = false;

				yield return Customers.Insert(retitem);

			}
		}

		public static void Bill(EPCustomerBillingProcess docgraph, CustomersList customer, BillingFilter filter)
		{
			docgraph.Bill(customer, filter);
		}

		#region EventHandlers
		protected virtual void BillingFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			Customers.Cache.Clear();
		}

		protected virtual void BillingFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			BillingFilter filter = Filter.Current;
			Customers.SetProcessDelegate<EPCustomerBillingProcess>(
					delegate(EPCustomerBillingProcess docgraph, CustomersList customer)
					{
						EPCustomerBilling.Bill(docgraph, customer, filter);
					});
		}
		#endregion


		#region Internal Types
		[Serializable]
		public partial class BillingFilter : PX.Data.IBqlTable
		{
			#region InvoiceDate
			public abstract class invoiceDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _InvoiceDate;
			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Invoice Date", Visibility = PXUIVisibility.Visible)]
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
			#region CustomerClassID
			public abstract class customerClassID : PX.Data.IBqlField
			{
			}
			protected String _CustomerClassID;
			[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
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
			#region EndDate
			public abstract class endDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _EndDate;
			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Load Claims Up to", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? EndDate
			{
				get
				{
					return this._EndDate;
				}
				set
				{
					this._EndDate = value;
				}
			}
			#endregion
		}
		

		#endregion
	}

	[Serializable]
	public partial class CustomersList : PX.Data.IBqlTable
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
		#region CustomerClassID
		public abstract class customerClassID : PX.Data.IBqlField
		{
		}
		protected String _CustomerClassID;
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
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
		[Customer(DescriptionField = typeof(Customer.acctName), IsKey = true)]
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
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<CustomersList.customerID>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, IsKey = true)]
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
		
	}

	public class EPCustomerBillingProcess : PXGraph<EPCustomerBillingProcess>
	{

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		protected virtual void EPExpenseClaimDetails_RefNbr_CacheAttached(PXCache sender)
		{ }


		public PXSelect<EPExpenseClaimDetails> Transactions;
		public PXSetup<EPSetup> Setup;

		public virtual void Bill(CustomersList customer, PX.Objects.EP.EPCustomerBilling.BillingFilter filter)
		{
			ARInvoiceEntry arGraph = PXGraph.CreateInstance<ARInvoiceEntry>();
			RegisterEntry pmGraph = PXGraph.CreateInstance<RegisterEntry>();

			arGraph.Clear();
			pmGraph.Clear();

			PMRegister pmDoc = null;
			ARInvoice arDoc = null;

			List<ARRegister> doclist = new List<ARRegister>();
			List<EPExpenseClaimDetails> listOfDirectBilledClaims = new List<EPExpenseClaimDetails>();

			PXSelectBase<EPExpenseClaimDetails> select = new PXSelectJoin<EPExpenseClaimDetails,
															InnerJoin<EPExpenseClaim, On<EPExpenseClaimDetails.refNbr, Equal<EPExpenseClaim.refNbr>>>,
															Where<EPExpenseClaimDetails.released, Equal<boolTrue>,
															And<EPExpenseClaimDetails.billable, Equal<boolTrue>,
															And<EPExpenseClaimDetails.billed, Equal<boolFalse>,
															And<EPExpenseClaimDetails.customerID, Equal<Required<EPExpenseClaimDetails.customerID>>,
															And<EPExpenseClaimDetails.customerLocationID, Equal<Required<EPExpenseClaimDetails.customerLocationID>>,
															And<EPExpenseClaimDetails.expenseDate, LessEqual<Required<EPExpenseClaimDetails.expenseDate>>>>>>>>,
															OrderBy<Asc<EPExpenseClaimDetails.customerID, Asc<EPExpenseClaimDetails.customerLocationID>>>>(this);

			foreach (PXResult<EPExpenseClaimDetails, EPExpenseClaim> res in select.Select(customer.CustomerID, customer.LocationID, filter.EndDate))
			{
				EPExpenseClaimDetails row = (EPExpenseClaimDetails)res;
				EPExpenseClaim doc = (EPExpenseClaim)res;

				if (row.ContractID != null && !ProjectDefaultAttribute.IsNonProject(this, row.ContractID))
				{
					if (pmDoc == null)
					{
						pmDoc = (PMRegister)pmGraph.Document.Cache.Insert();
						pmDoc.OrigDocType = PMOrigDocType.ExpenseClaim;
						pmDoc.OrigDocNbr = doc.RefNbr;

					}

					InsertPMTran(pmGraph, row, doc, Setup.Current.CopyNotesPM == true, Setup.Current.CopyFilesPM == true);
				}
				else
				{
					if (arDoc == null ||
								arDoc.CustomerID != row.CustomerID ||
								arDoc.CustomerLocationID != row.CustomerLocationID)
					{
						if (arDoc != null)
						{
							arDoc.CuryOrigDocAmt = arDoc.CuryDocBal;
							arGraph.Document.Update(arDoc);
							arGraph.Save.Press();
						}
						arDoc = (ARInvoice)arGraph.Document.Cache.Insert();
						arDoc.DocType = AR.ARDocType.Invoice;
						arDoc.CustomerID = row.CustomerID;
						arDoc.CustomerLocationID = row.CustomerLocationID;
						arDoc = arGraph.Document.Update(arDoc);
						arGraph.Document.Cache.RaiseFieldUpdated<AR.ARInvoice.customerID>(arDoc, null);
						if (Setup.Current.AutomaticReleaseAR == true)
							arDoc.Hold = false;
						doclist.Add(arDoc);
					}

					//insert ARTran
					InsertARTran(arGraph, row, Setup.Current.CopyNotesAR == true, Setup.Current.CopyFilesAR == true);
					listOfDirectBilledClaims.Add(row);
				}

				row.Billed = true;
				Transactions.Update(row);
			}

			if (arDoc != null)
			{
				arDoc.CuryOrigDocAmt = arDoc.CuryDocBal;
				arGraph.Document.Update(arDoc);

				arGraph.RowPersisted.AddHandler<ARInvoice>(
				delegate(PXCache sender, PXRowPersistedEventArgs e)
				{
					if (e.TranStatus == PXTranStatus.Open)
					{
						foreach (EPExpenseClaimDetails claimdetail in listOfDirectBilledClaims)
						{
							EPExpenseClaimDetails row = Transactions.Locate(claimdetail);

							row.ARDocType = ((ARInvoice)e.Row).DocType;
							row.ARRefNbr = ((ARInvoice)e.Row).RefNbr;
						}
					}
				});

				arGraph.Save.Press();
			}
			if (pmDoc != null)
				pmGraph.Save.Press();

			this.Persist(typeof(EPExpenseClaimDetails), PXDBOperation.Update);

			if (Setup.Current.AutomaticReleaseAR == true)
			{
				ARDocumentRelease.ReleaseDoc(doclist, false);
			}
		}

		protected virtual void InsertARTran(ARInvoiceEntry arGraph, EPExpenseClaimDetails row, bool copyNotes, bool copyFiles)
		{
			ARTran tran = (ARTran)arGraph.Transactions.Cache.CreateInstance();

			tran.InventoryID = row.InventoryID;
			tran.TranDesc = row.TranDesc;
			tran.TranCost = row.ExtCost;
			tran.Qty = row.Qty;
			tran.UOM = row.UOM;
			tran.CuryTranAmt = row.CuryTranAmt;
			tran.AccountID = row.SalesAccountID;
			tran.SubID = row.SalesSubID;
			tran.Date = row.ExpenseDate;
			tran = arGraph.Transactions.Insert(tran);

			if (copyNotes)
			{
				string note = PXNoteAttribute.GetNote(Caches[typeof(EPExpenseClaimDetails)], row);
				if (note != null)
					PXNoteAttribute.SetNote(arGraph.Transactions.Cache, tran, note);
			}
			if (copyFiles)
			{
				Guid[] files = PXNoteAttribute.GetFileNotes(Caches[typeof(EPExpenseClaimDetails)], row);
				if (files != null && files.Length > 0)
					PXNoteAttribute.SetFileNotes(arGraph.Transactions.Cache, tran, files);
			}
		}

		protected virtual void InsertPMTran(RegisterEntry pmGraph, EPExpenseClaimDetails row, EPExpenseClaim doc, bool copyNotes, bool copyFiles)
		{
			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(pmGraph, row.ContractID);
			Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, row.ExpenseAccountID);

			if (account.AccountGroupID == null && project.BaseType == PMProject.ProjectBaseType.Project)
				throw new PXException(Messages.AccountGroupIsNotAssignedForAccount, account.AccountCD);

			PMTran tran = (PMTran)pmGraph.Transactions.Cache.Insert();
			tran.AccountGroupID = account.AccountGroupID;
			tran.BAccountID = row.CustomerID;
			tran.LocationID = row.CustomerLocationID;
			tran.ProjectID = row.ContractID;
			tran.TaskID = row.TaskID;
			tran.InventoryID = row.InventoryID;
			tran.Qty = row.Qty;
			tran.Billable = true;
			tran.BillableQty = row.Qty;
			tran.UOM = row.UOM;
			tran.UnitRate = row.UnitCost;
			tran.Amount = row.TranAmt;
			tran.AccountID = row.ExpenseAccountID;
			tran.SubID = row.ExpenseSubID;
			tran.StartDate = row.ExpenseDate;
			tran.EndDate = row.ExpenseDate;
			tran.Date = row.ExpenseDate;
			tran.ResourceID = doc.EmployeeID;
			tran.Released = project.BaseType == PMProject.ContractBaseType.Contract;//contract trans are created as released

			pmGraph.Transactions.Update(tran);

			pmGraph.Document.Current.Released = project.BaseType == PMProject.ContractBaseType.Contract;

			if (copyNotes)
			{
				string note = PXNoteAttribute.GetNote(Caches[typeof(EPExpenseClaimDetails)], row);
				if (note != null)
					PXNoteAttribute.SetNote(pmGraph.Transactions.Cache, tran, note);
			}
			if (copyFiles)
			{
				Guid[] files = PXNoteAttribute.GetFileNotes(Caches[typeof(EPExpenseClaimDetails)], row);
				if (files != null && files.Length > 0)
					PXNoteAttribute.SetFileNotes(pmGraph.Transactions.Cache, tran, files);
			}
		}
	}
}
