using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.BQLConstants;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.TX;

namespace PX.Objects.AR
{
	[TableAndChartDashboardType]
	public class ARScheduleRun : PXGraph<ARScheduleRun>
	{
        #region Cache Attached Events
        #region Schedule
        #region ScheduleID
        [PXDBString(10, IsUnicode = true, IsKey = true)]
        [PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.Visible, Enabled = false)]
        protected virtual void Schedule_ScheduleID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion
        #endregion

		public PXFilter<ScheduleRun.Parameters> Filter;
		public PXCancel<ScheduleRun.Parameters> Cancel;
		public PXFilteredProcessingJoin<Schedule, ScheduleRun.Parameters,
			LeftJoin<ARRegisterAccess, On<ARRegisterAccess.scheduleID, Equal<Schedule.scheduleID>,
			And<ARRegisterAccess.scheduled, Equal<boolTrue>,
			And<Not<Match<ARRegisterAccess, Current<AccessInfo.userName>>>>>>>,
			Where<Schedule.active, Equal<boolTrue>,
			And<Schedule.module, Equal<BatchModule.moduleAR>,
			And<Schedule.scheduleType, NotEqual<GLScheduleType.deferredRevenue>, 
			And<Schedule.nextRunDate, LessEqual<Current<ScheduleRun.Parameters.endDate>>,
			And2<Where<Current<ScheduleRun.Parameters.startDate>, IsNull, Or<Schedule.nextRunDate, GreaterEqual<Current<ScheduleRun.Parameters.startDate>>>>,
			And<ARRegisterAccess.docType, IsNull>>>>>>>
			Schedule_List;

		public ARScheduleRun()
		{
			ARSetup setup = ARSetup.Current;
			Schedule_List.SetProcessCaption("Run Selected");
			Schedule_List.SetProcessAllCaption("Run All");
		}

		public PXAction<ScheduleRun.Parameters> ViewSchedule;
		[PXUIField(DisplayName = Messages.ViewSchedule, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable viewSchedule(PXAdapter adapter)
		{
			if (Schedule_List.Current != null)
			{
				PXRedirectHelper.TryRedirect(Schedule_List.Cache, Schedule_List.Current, "Schedule", PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXAction<ScheduleRun.Parameters> NewSchedule;
		[PXUIField(DisplayName = Messages.NewSchedule, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable newSchedule(PXAdapter adapter)
		{
			ARScheduleMaint graph = CreateInstance<ARScheduleMaint>();
			graph.Schedule_Header.Insert(new Schedule());
			graph.Schedule_Header.Cache.IsDirty = false;
			throw new PXRedirectRequiredException(graph, true, "New Schedule") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

        protected virtual void Parameters_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            ScheduleRun.SetProcessDelegate<ARScheduleProcess>(this, (ScheduleRun.Parameters)e.Row, Schedule_List);
        }

		protected virtual void Parameters_StartDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ScheduleRun.Parameters filterData = (ScheduleRun.Parameters)e.Row;
			if (filterData.EndDate != null && filterData.StartDate > filterData.EndDate)
			{
				filterData.EndDate = filterData.StartDate;
			}
		}

		public PXSetup<ARSetup> ARSetup;

		protected virtual void Schedule_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			Schedule task = (Schedule)e.Row;
			if (task == null || PXLongOperation.Exists(UID)) return;

			if ((task.NoRunLimit ?? false) == false && task.RunCntr == task.RunLimit)
			{
				cache.RaiseExceptionHandling<Schedule.scheduleID>(e.Row, task.ScheduleID, new PXSetPropertyException(Messages.SheduleExecutionLimitExceeded, PXErrorLevel.RowError));
			}
			else if ((task.NoEndDate ?? false) == false && task.EndDate < Accessinfo.BusinessDate)
			{
				cache.RaiseExceptionHandling<Schedule.scheduleID>(e.Row, task.ScheduleID, new PXSetPropertyException(Messages.SheduleHasExpired, PXErrorLevel.RowError));
			}
			else if (task.NextRunDate > Accessinfo.BusinessDate)
			{
				cache.RaiseExceptionHandling<Schedule.scheduleID>(e.Row, task.ScheduleID, new PXSetPropertyException(Messages.SheduleNextExecutionDateExceeded, PXErrorLevel.RowWarning));
			}
		}
	}

	public class ARScheduleProcess : PXGraph<ARScheduleProcess>, IScheduleProcessing
	{
		public PXSelect<Schedule> Running_Schedule;
		public PXSelect<ARTran> Tran_Created;
		public PXSelect<CurrencyInfo> CuryInfo_Created;

		public ARScheduleProcess()
		{
		}

        public virtual void GenerateProc(Schedule s)
        {
            GenerateProc(s, 1, Accessinfo.BusinessDate.Value);
        }

        public virtual void GenerateProc(Schedule s, short Times, DateTime runDate)
        {
            List<ScheduleDet> sd = GL.ScheduleProcess.MakeSchedule(this, s, Times, runDate);

			ARInvoiceEntry docgraph = CreateGraph();

			foreach (ScheduleDet sdet in sd)
			{
				foreach (PXResult<ARInvoice, Customer, CurrencyInfo> res in PXSelectJoin<ARInvoice, InnerJoin<Customer, On<Customer.bAccountID, Equal<ARInvoice.customerID>>, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARInvoice.curyInfoID>>>>, Where<ARInvoice.scheduleID, Equal<Required<ARInvoice.scheduleID>>, And<ARInvoice.scheduled,Equal<boolTrue>>>>.Select(this, s.ScheduleID))
				{
					docgraph.Clear();
					docgraph.customer.Current = (Customer)res;
					ARInvoice apdoc = (ARInvoice)res;
					CurrencyInfo info = (CurrencyInfo)res;

					CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(info);
					new_info.CuryInfoID = null;
					new_info = docgraph.currencyinfo.Insert(new_info);

					ARInvoice new_ardoc = PXCache<ARInvoice>.CreateCopy(apdoc);
					new_ardoc.CuryInfoID = new_info.CuryInfoID;
					new_ardoc.DocDate = sdet.ScheduledDate;
					new_ardoc.FinPeriodID = sdet.ScheduledPeriod;
					new_ardoc.TranPeriodID = null;
					new_ardoc.DueDate = null;
					new_ardoc.DiscDate = null;
					new_ardoc.CuryOrigDiscAmt = null;
					new_ardoc.OrigDiscAmt = null;
					new_ardoc.RefNbr = null;
					new_ardoc.Scheduled = false;
					new_ardoc.CuryLineTotal = 0m;
					new_ardoc.CuryVatTaxableTotal = 0m;
					new_ardoc.CuryVatExemptTotal = 0m;
					new_ardoc.NoteID = null;
					bool forceClear = false;
					bool clearPM = false;
					if (new_ardoc.PMInstanceID.HasValue)
					{
						PXResult<CustomerPaymentMethod, CA.PaymentMethod> pmiResult = (PXResult<CustomerPaymentMethod, CA.PaymentMethod>)PXSelectJoin<CustomerPaymentMethod, InnerJoin<CA.PaymentMethod, On<CA.PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>>>, Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>.Select(docgraph, new_ardoc.PMInstanceID);
						
						if (pmiResult != null)
						{
							CustomerPaymentMethod pmInstance = pmiResult;
							CA.PaymentMethod paymentMethod = pmiResult;
							if (pmInstance == null || pmInstance.IsActive != true || paymentMethod.IsActive != true || paymentMethod.UseForAR != true)
							{
								clearPM = true;
								forceClear = true;
							}
						}
						else
						{
							clearPM = true;
							forceClear = true;
						}
						
					}
					else
					{
						if (string.IsNullOrEmpty(new_ardoc.PaymentMethodID) == false) 
						{
							CA.PaymentMethod pm = PXSelect<CA.PaymentMethod, Where<CA.PaymentMethod.paymentMethodID, Equal<Required<CA.PaymentMethod.paymentMethodID>>>>.Select(docgraph, new_ardoc.PaymentMethodID);
							if (pm == null || pm.IsActive != true || pm.UseForAR != true) 
							{
								clearPM = true;
								forceClear = true;
							}
						}
					}

					if (clearPM) 
					{
						new_ardoc.PMInstanceID = null;
						new_ardoc.PaymentMethodID = null;
						new_ardoc.CashAccountID = null;
					}

					new_ardoc = docgraph.Document.Insert(new_ardoc);
                    
                    //force creditrule back
                    docgraph.customer.Current = (Customer)res;

					if (forceClear == true)
					{
						ARInvoice copy = PXCache<ARInvoice>.CreateCopy(new_ardoc);
						copy.PMInstanceID = null;
						copy.PaymentMethodID = null;
						copy.CashAccountID = null;
						new_ardoc = docgraph.Document.Update(copy);
					}
					AddressAttribute.CopyRecord<ARInvoice.billAddressID>(docgraph.Document.Cache, new_ardoc, apdoc, false);
					ContactAttribute.CopyRecord<ARInvoice.billContactID>(docgraph.Document.Cache, new_ardoc, apdoc, false);

					TaxAttribute.SetTaxCalc<ARTran.taxCategoryID, ARTaxAttribute>(docgraph.Transactions.Cache, null, TaxCalc.ManualCalc);
					PXNoteAttribute.SetNote(docgraph.Document.Cache, new_ardoc, PXNoteAttribute.GetNote(Caches[typeof(ARInvoice)], apdoc));
					PXNoteAttribute.SetFileNotes(docgraph.Document.Cache, new_ardoc, PXNoteAttribute.GetFileNotes(Caches[typeof(ARInvoice)], apdoc));

					foreach (ARTran aptran in PXSelect<ARTran, Where<ARTran.tranType, Equal<Required<ARTran.tranType>>, And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>>>>.Select(docgraph, apdoc.DocType, apdoc.RefNbr))
					{
						ARTran new_aptran = PXCache<ARTran>.CreateCopy(aptran);
						new_aptran.RefNbr = null;
					    new_aptran.CuryInfoID = null;
						docgraph.Transactions.Insert(new_aptran);
					}

					foreach (ARTaxTran tax in PXSelect<ARTaxTran, Where<ARTaxTran.tranType, Equal<Required<ARTaxTran.tranType>>, And<ARTaxTran.refNbr, Equal<Required<ARTaxTran.refNbr>>>>>.Select(docgraph, apdoc.DocType, apdoc.RefNbr))
					{
						ARTaxTran new_artax = new ARTaxTran();
						new_artax.TaxID = tax.TaxID;

						new_artax = docgraph.Taxes.Insert(new_artax);

						if (new_artax != null)
						{
							new_artax = PXCache<ARTaxTran>.CreateCopy(new_artax);
							new_artax.TaxRate = tax.TaxRate;
							new_artax.CuryTaxableAmt = tax.CuryTaxableAmt;
							new_artax.CuryTaxAmt = tax.CuryTaxAmt;
							new_artax = docgraph.Taxes.Update(new_artax);
						}
					}
					
					docgraph.Save.Press();
				}
				s.LastRunDate = sdet.ScheduledDate;
				Running_Schedule.Cache.Update(s);
			}

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				Running_Schedule.Cache.Persist(PXDBOperation.Update);
				ts.Complete(this);
			}
			Running_Schedule.Cache.Persisted(false);
		}

		public virtual ARInvoiceEntry CreateGraph()
		{
			return PXGraph.CreateInstance<ARInvoiceEntry>();
		}
	}
}


