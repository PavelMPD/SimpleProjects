using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using System.Collections;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.AR
{
	[System.SerializableAttribute()]
	public partial class ARAutoApplyParameters: IBqlTable
	{
		#region ApplyCreditMemos
		public abstract class applyCreditMemos : PX.Data.IBqlField
		{
		}
		protected bool? _ApplyCreditMemos;
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Apply Credit Memos", Visibility = PXUIVisibility.Visible)]
		public virtual bool? ApplyCreditMemos
		{
			get
			{
				return this._ApplyCreditMemos;
			}
			set
			{
				this._ApplyCreditMemos = value;
			}
		}
		#endregion
		#region ReleaseBatchWhenFinished
		public abstract class releaseBatchWhenFinished : PX.Data.IBqlField
		{
		}
		protected bool? _ReleaseBatchWhenFinished;
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Release Batch When Finished", Visibility = PXUIVisibility.Visible)]
		public virtual bool? ReleaseBatchWhenFinished
		{
			get
			{
				return this._ReleaseBatchWhenFinished;
			}
			set
			{
				this._ReleaseBatchWhenFinished = value;
			}
		}
		#endregion
		#region ApplicationDate
		public abstract class applicationDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _ApplicationDate;
		[PXDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Application Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? ApplicationDate
		{
			get
			{
				return this._ApplicationDate;
			}
			set
			{
				this._ApplicationDate = value;
			}
		}
		#endregion
		#region FinPeriod
		public abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		protected String _FinPeriodID;
		[AROpenPeriod(typeof(ARAutoApplyParameters.applicationDate))]
		[PXUIField(DisplayName = "Application Period", Visibility = PXUIVisibility.Visible)]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
	}

	[TableAndChartDashboardType]
	public class ARAutoApplyPayments : PXGraph<ARAutoApplyPayments>
	{
		public PXCancel<ARAutoApplyParameters> Cancel;
		public PXFilter<ARAutoApplyParameters> Filter;
		[PXFilterable]
		public PXFilteredProcessing<ARStatementCycle, ARAutoApplyParameters> ARStatementCycleList;
		

		public ARAutoApplyPayments()
		{
			ARSetup setup = arsetup.Current;
		}

		public PXSetup<ARSetup> arsetup;

		protected virtual void ARAutoApplyParameters_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;

            ARAutoApplyParameters filter = Filter.Current;
			ARStatementCycleList.SetProcessDelegate<ARPaymentEntry>(
				delegate(ARPaymentEntry graph, ARStatementCycle cycle)
				{
					ProcessDoc(graph, cycle, filter);
				}
			);
		}
		
		public static void ProcessDoc(ARPaymentEntry graph, ARStatementCycle cycle, ARAutoApplyParameters filter)
		{
			List<ARRegister> toRelease = new List<ARRegister>();

			// Build Invoices List
			foreach (Customer customer in
				PXSelect<Customer,
					Where<Customer.statementCycleId, Equal<Required<Customer.statementCycleId>>,
					And<Match<Required<AccessInfo.userName>>>>>
					.Select(graph, cycle.StatementCycleId, graph.Accessinfo.UserName))
			{
				List<ARInvoice> arInvoiceList = new List<ARInvoice>();

				foreach (ARInvoice invoice in
					PXSelect<ARInvoice, Where<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>,
					And<ARInvoice.released, Equal<boolTrue>,
					And<ARInvoice.openDoc, Equal<boolTrue>,
					And<Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>,
					Or<ARInvoice.docType, Equal<Required<ARInvoice.docType>>,
					Or<ARInvoice.docType, Equal<Required<ARInvoice.docType>>>>>>>>>,
					OrderBy<Asc<ARInvoice.dueDate>>>
					.Select(graph,
					customer.BAccountID,
					ARDocType.Invoice, 
					ARDocType.FinCharge,
					filter.ApplyCreditMemos == true ? ARDocType.DebitMemo : ARDocType.Invoice))
				{
					arInvoiceList.Add(invoice);
				}

				arInvoiceList.Sort(new Comparison<ARInvoice>(delegate(ARInvoice a, ARInvoice b)
					{
						if ((bool)graph.arsetup.Current.FinChargeFirst)
						{
							int aSortOrder = (a.DocType == ARDocType.FinCharge ? 0 : 1);
							int bSortOrder = (b.DocType == ARDocType.FinCharge ? 0 : 1);
							int ret = ((IComparable)aSortOrder).CompareTo(bSortOrder);
							if (ret != 0)
							{
								return ret;
							}
						}

						{
							object aDueDate = a.DueDate;
							object bDueDate = b.DueDate;
							int ret = ((IComparable)aDueDate).CompareTo(bDueDate);

							return ret;
						}
					}
				));


				if (arInvoiceList.Count > 0)
				{
					int invoiceIndex = 0;

					// this foreach gets all payments [and CreditMemos if applyCreditMemos = true] sorted by docDate
					foreach (ARPayment payment in
						PXSelect<ARPayment,
							Where<ARPayment.customerID, Equal<Required<ARPayment.customerID>>,
							And<ARPayment.released, Equal<boolTrue>,
							And<ARPayment.openDoc, Equal<boolTrue>,
							And<Where<ARPayment.docType, Equal<Required<ARPayment.docType>>,
							Or<ARPayment.docType, Equal<Required<ARPayment.docType>>>>>>>>,
							OrderBy<Asc<ARPayment.docDate>>>
							.Select(graph,
							customer.BAccountID,
							ARDocType.Payment,
							filter.ApplyCreditMemos == true ? ARDocType.CreditMemo : ARDocType.Payment))
					{
						graph.Clear();
						graph.Document.Current = payment;

						if (graph.Adjustments.Select().Count == 0)
						{
							bool adjustmentAdded = false;
							while (graph.Document.Current.CuryUnappliedBal > 0)
							{
								if (graph.Document.Current.CuryApplAmt == null)
								{
									object curyapplamt = graph.Document.Cache.GetValueExt<ARPayment.curyApplAmt>(graph.Document.Current);
									if (curyapplamt is PXFieldState)
									{
										curyapplamt = ((PXFieldState)curyapplamt).Value;
									}
									graph.Document.Current.CuryApplAmt = (decimal?)curyapplamt;
								}
								graph.Document.Current.AdjDate = filter.ApplicationDate;
								graph.Document.Current.AdjFinPeriodID = filter.FinPeriodID;
								graph.Document.Cache.Update(graph.Document.Current);

								ARInvoice invoice = arInvoiceList[invoiceIndex];

								ARAdjust adj = new ARAdjust();
								adj.AdjdDocType = invoice.DocType;
								adj.AdjdRefNbr = invoice.RefNbr;

								graph.AutoPaymentApp = true;
								adj = graph.Adjustments.Insert(adj);
								if (adj == null)
								{
									throw new PXException(PXMessages.LocalizeFormat(Messages.ErrorAutoApply, invoice.DocType, invoice.RefNbr, payment.DocType, payment.RefNbr));
								}
								adjustmentAdded = true;
								if (adj.CuryDocBal <= 0m)
								{
									invoiceIndex++;
								}
								if (invoiceIndex >= arInvoiceList.Count)
									break;
							}
							if (adjustmentAdded)
							{
								graph.Save.Press();
								if (filter.ReleaseBatchWhenFinished == true)
								{
									toRelease.Add(graph.Document.Current);
								}
							}
						}
						if (invoiceIndex >= arInvoiceList.Count)
							break;
					}
				}
			}

			if (toRelease.Count > 0)
			{
				ARDocumentRelease.ReleaseDoc(toRelease, false);
			}
		}

		protected virtual void ARStatementCycle_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				ARStatementCycle row = e.Row as ARStatementCycle;

				row.NextStmtDate = ARStatementProcess.CalcStatementDateBefore(this.Accessinfo.BusinessDate.Value, row.PrepareOn, row.Day00, row.Day01);
				if (row.LastStmtDate.HasValue && row.NextStmtDate <= row.LastStmtDate)
					row.NextStmtDate = ARStatementProcess.CalcNextStatementDate(row.LastStmtDate.Value, row.PrepareOn, row.Day00, row.Day01);
			}
		}
	}
}
