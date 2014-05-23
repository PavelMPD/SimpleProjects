using PX.Objects.CT;
using PX.SM;
using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.IN;
using PX.Objects.BQLConstants;
using PX.Objects.TX;
using PX.Objects.CR;
using PX.Objects.AP;
using PX.TM;
using PX.Objects.PM;
using PX.Common;

namespace PX.Objects.EP
{
	public class ExpenseClaimEntry : PXGraph<ExpenseClaimEntry, EPExpenseClaim>, PXImportAttribute.IPXPrepareItems
	{
		#region Buttons declaration

		public PXAction<EPExpenseClaim> release;
		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			foreach (PXResult<EPExpenseClaim> rec in adapter.Get())
			{
				EPExpenseClaim claim = rec;
				if (claim.Approved == false || claim.Released == true)
				{
					throw new PXException(Messages.Document_Status_Invalid);
				}
				Save.Press();
				PXLongOperation.StartOperation(this, () => EPDocumentRelease.ReleaseDoc(claim));
			}
			return adapter.Get();
		}

		public ToggleCurrency<EPExpenseClaim> CurrencyView;

		public PXAction<EPExpenseClaim> expenseClaimPrint;
		[PXUIField(DisplayName = Messages.PrintExpenseClaims, MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.Report)]
		protected virtual IEnumerable ExpenseClaimPrint(PXAdapter adapter)
		{
			if (ExpenseClaim.Current != null)
			{
				var parameters = new Dictionary<string, string>();
				parameters["RefNbr"] = ExpenseClaim.Current.RefNbr;
				throw new PXReportRequiredException(parameters, "EP612000", Messages.PrintExpenseClaims);
			}

			return adapter.Get();
		}

		#endregion

		#region Selects Declartion

        [PXHidden]
	    public PXSelect<Contract> Dummy;
            
        [PXViewName(Messages.ExpenseClaim)]
		public PXSelectJoin<EPExpenseClaim,
			InnerJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<EPExpenseClaim.employeeID>>>,
			Where<EPExpenseClaim.createdByID, Equal<Current<AccessInfo.userID>>,
						 Or<EPEmployee.userID, Equal<Current<AccessInfo.userID>>,
						 Or<EPEmployee.userID, OwnedUser<Current<AccessInfo.userID>>,
						 Or<EPExpenseClaim.noteID, Approver<Current<AccessInfo.userID>>>>>>> ExpenseClaim;

		public PXSelectJoin<EPExpenseClaim, LeftJoin<APInvoice, On<APInvoice.refNbr, Equal<EPExpenseClaim.aPRefNbr>>>, 
            Where<EPExpenseClaim.refNbr, Equal<Current<EPExpenseClaim.refNbr>>>> ExpenseClaimCurrent;

        [PXImport(typeof(EPExpenseClaim))]
		public PXSelect<EPExpenseClaimDetails, Where<EPExpenseClaimDetails.refNbr, Equal<Current<EPExpenseClaim.refNbr>>>> ExpenseClaimDetails;
		public PXSelect<CurrencyInfo> currencyinfo;
		public CMSetupSelect cmsetup;
		public PXSelectReadonly<EPSetup> epsetup;
		[PXViewName(Messages.Employee)]
		public PXSetup<EPEmployee, Where<EPEmployee.bAccountID, Equal<Optional<EPExpenseClaim.employeeID>>>> EPEmployee;

		public PXSelect<EPTax, Where<EPTax.refNbr, Equal<Current<EPExpenseClaim.refNbr>>>, OrderBy<Asc<EPTax.refNbr, Asc<EPTax.taxID>>>> Tax_Rows;
		public PXSelectJoin<EPTaxTran, InnerJoin<Tax, On<Tax.taxID, Equal<EPTaxTran.taxID>>>, Where<EPTaxTran.refNbr, Equal<Current<EPExpenseClaim.refNbr>>>> Taxes;

		public PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Current<EPExpenseClaim.taxZoneID>>>> taxzone;
		public PXSetup<Location, Where<Location.bAccountID, Equal<Current<EPExpenseClaim.employeeID>>, And<Location.locationID, Equal<Optional<EPExpenseClaim.locationID>>>>> location;
		
		public EPApprovalAutomation<EPExpenseClaim, EPExpenseClaim.approved, EPExpenseClaim.rejected, EPExpenseClaim.hold> Approval;

		#endregion

		public ExpenseClaimEntry ()
		{
			if (epsetup.Current == null)
				throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(EPSetup), Messages.EPSetup);

			EPEmployee employeeByUserID = PXSelect<EPEmployee, Where<EP.EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>.Select(this);
			if (employeeByUserID == null)
			{
				//throw new PXException(Messages.MustBeEmployee);
				Redirector.Redirect(System.Web.HttpContext.Current, string.Format("~/Frames/Error.aspx?exceptionID={0}&typeID={1}", Messages.MustBeEmployee, "error"));
			}
		}

		#region Default Instance Accessors

		public EPSetup EPSETUP
		{
			get
			{
				return epsetup.Select();
			}

		}

		public TaxZone TAXZONE
		{
			get
			{
				return taxzone.Select();
			}
		}

		#endregion

		#region CurrencyInfo events
		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			if ((bool)cmsetup.Current.MCActivated)
			{
				if (EPEmployee.Current != null && !string.IsNullOrEmpty(EPEmployee.Current.CuryID))
				{
					e.NewValue = EPEmployee.Current.CuryID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			if ((bool)cmsetup.Current.MCActivated)
			{
				if (EPEmployee.Current != null && !string.IsNullOrEmpty(EPEmployee.Current.CuryRateTypeID))
				{
					e.NewValue = EPEmployee.Current.CuryRateTypeID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			if (ExpenseClaim.Cache.Current != null)
			{
				e.NewValue = ((EPExpenseClaim)ExpenseClaim.Cache.Current).DocDate;
				e.Cancel = true;
			}
		}

		protected virtual void CurrencyInfo_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CurrencyInfo info = e.Row as CurrencyInfo;
			if (info != null)
			{
				bool curyenabled = info.AllowUpdate(this.ExpenseClaimDetails.Cache);
				if (ExpenseClaim.Current != null && EPEmployee.Current != null && !(bool)EPEmployee.Current.AllowOverrideRate)
				{
					curyenabled = false;
				}

				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(cache, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(cache, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(cache, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(cache, info, curyenabled);
			}
		}
		#endregion

		#region Expense Claim Events		
		protected virtual void EPExpenseClaim_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			EPExpenseClaim doc = e.Row as EPExpenseClaim;
			if (doc == null)
			{
				return;
			}

		    APInvoice apdoc =
                PXSelect<APInvoice, Where<APInvoice.refNbr, Equal<Current<EPExpenseClaim.aPRefNbr>>>>.Select(this);
            if(apdoc != null)
            {
                doc.APStatus = apdoc.Status;    
            }
			ExpenseClaimDetails.Cache.AllowDelete = true;
			ExpenseClaimDetails.Cache.AllowUpdate = true;
			ExpenseClaimDetails.Cache.AllowInsert = true;
			Taxes.Cache.AllowDelete = true;
			Taxes.Cache.AllowUpdate = true;
			Taxes.Cache.AllowInsert = true;

			if (taxzone.Current != null && doc.TaxZoneID != taxzone.Current.TaxZoneID)
			{
				taxzone.Current = null;
			}

			PXUIFieldAttribute.SetVisible<EPExpenseClaim.curyID>(cache, doc, (bool)cmsetup.Current.MCActivated);

			bool curyenabled = true;

			if (EPEmployee.Current != null && (bool)EPEmployee.Current.AllowOverrideCury == false)
			{
				curyenabled = false;
			}

			if (doc.Released == true)
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				cache.AllowDelete = false;
				cache.AllowUpdate = false;
				ExpenseClaimDetails.Cache.AllowDelete = false;
				ExpenseClaimDetails.Cache.AllowUpdate = false;
				ExpenseClaimDetails.Cache.AllowInsert = false;
				Taxes.Cache.AllowDelete = false;
				Taxes.Cache.AllowUpdate = false;
				Taxes.Cache.AllowInsert = false;
				release.SetEnabled(false);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaim.refNbr>(cache, doc, true);

			}
			else
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaim.status>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaim.aPRefNbr>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaim.aPDocType>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaim.approverID>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaim.workgroupID>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaim.curyDocBal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaim.curyVatExemptTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaim.curyVatTaxableTotal>(cache, doc, false);

				PXUIFieldAttribute.SetEnabled<EPExpenseClaim.curyID>(cache, doc, curyenabled);

				OpenPeriodAttribute.SetValidatePeriod<EPExpenseClaim.finPeriodID>(cache, e.Row, PeriodValidation.DefaultUpdate);

				if (doc.Approved == true)
				{	
					PXUIFieldAttribute.SetEnabled(cache, doc, false);
					PXUIFieldAttribute.SetEnabled<EPExpenseClaim.refNbr>(cache, doc, true);
					PXUIFieldAttribute.SetEnabled<EPExpenseClaim.docDesc>(cache, doc, cache.GetStatus(e.Row) == PXEntryStatus.Inserted); 
					ExpenseClaimDetails.Cache.AllowUpdate = false;
					ExpenseClaimDetails.Cache.AllowInsert = false;
					ExpenseClaimDetails.Cache.AllowDelete = false;
					Taxes.Cache.AllowDelete = false;
					Taxes.Cache.AllowUpdate = false;
					Taxes.Cache.AllowInsert = false;
				}

				PXUIFieldAttribute.SetEnabled<EPExpenseClaim.hold>(cache, e.Row, doc.Released != true);
				bool isApprover = doc.Status == EPClaimStatus.Balanced && Approval.IsApprover(doc);

				PXUIFieldAttribute.SetEnabled<EPExpenseClaim.rejected>(cache, e.Row, isApprover);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaim.approved>(cache, e.Row, isApprover);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaim.approveDate>(cache, e.Row, isApprover);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaim.finPeriodID>(cache, e.Row, isApprover || doc.Status == EPClaimStatus.Approved);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaim.approverID>(cache, doc, isApprover && doc.Status == EPClaimStatus.Balanced);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaim.workgroupID>(cache, doc, isApprover && doc.Status == EPClaimStatus.Balanced);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaim.approvedByID>(cache, doc, false);

				cache.AllowDelete = true;
				cache.AllowUpdate = true;
				release.SetEnabled(doc.Approved == true);
			}

			if (epsetup.Current != null)
			{
				PXUIFieldAttribute.SetVisible<EPExpenseClaim.curyOrigDocAmt>(cache, e.Row, (bool)epsetup.Current.RequireControlTotal);
			}
		}

		protected virtual void EPExpenseClaim_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			if (((EPExpenseClaim)e.Row).Released != null && (bool)((EPExpenseClaim)e.Row).Released == false)
			{
				if (epsetup.Current != null && !(bool)epsetup.Current.RequireControlTotal)
				{
					if (((EPExpenseClaim)e.Row).CuryDocBal != null && ((EPExpenseClaim)e.Row).CuryDocBal != 0)
						((EPExpenseClaim)e.Row).CuryOrigDocAmt = ((EPExpenseClaim)e.Row).CuryDocBal;
					else
						((EPExpenseClaim)e.Row).CuryOrigDocAmt = 0m;
				}
			}

			if (((EPExpenseClaim)e.Row).Status == EPClaimStatus.Balanced || ((EPExpenseClaim)e.Row).Status == EPClaimStatus.Approved || epsetup.Current.HoldEntry != true)
			{
				if (((EPExpenseClaim)e.Row).CuryDocBal != ((EPExpenseClaim)e.Row).CuryOrigDocAmt)
				{
					cache.RaiseExceptionHandling<EPExpenseClaim.curyOrigDocAmt>(e.Row, ((EPExpenseClaim)e.Row).CuryOrigDocAmt, new PXSetPropertyException(Messages.DocumentOutOfBalance));
				}
				else
				{
					cache.RaiseExceptionHandling<EPExpenseClaim.curyOrigDocAmt>(e.Row, ((EPExpenseClaim)e.Row).CuryOrigDocAmt, null);
				}
			}

		}

		protected virtual void EPExpenseClaim_Hold_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			EPExpenseClaim row = e.Row as EPExpenseClaim;
			
			if (e.Row != null && ((EPExpenseClaim)e.Row).Hold != null)
			{
				EPExpenseClaim claim = (EPExpenseClaim)e.Row;
				if ((bool)((EPExpenseClaim)e.Row).Hold)
				{
					((EPExpenseClaim)e.Row).Rejected = false;

					if (((EPExpenseClaim)e.Row).Status != EPClaimStatus.Voided)
					{
						((EPExpenseClaim)e.Row).Status = EPClaimStatus.Hold;
					}
					((EPExpenseClaim)e.Row).Approved = false;
					PXUIFieldAttribute.SetEnabled<EPExpenseClaim.rejected>(cache, e.Row, false);
					PXUIFieldAttribute.SetEnabled<EPExpenseClaim.approved>(cache, e.Row, false);
					PXUIFieldAttribute.SetEnabled<EPExpenseClaim.approveDate>(cache, e.Row, false);
					PXUIFieldAttribute.SetEnabled<EPExpenseClaim.finPeriodID>(cache, e.Row, false);
				}
				else if (!(bool)((EPExpenseClaim)e.Row).Hold)
				{
					((EPExpenseClaim)e.Row).Rejected = false;
					((EPExpenseClaim)e.Row).Status = EPClaimStatus.Balanced;
					PXUIFieldAttribute.SetEnabled<EPExpenseClaim.rejected>(cache, e.Row, true);
					PXUIFieldAttribute.SetEnabled<EPExpenseClaim.approved>(cache, e.Row, true);
					PXUIFieldAttribute.SetEnabled<EPExpenseClaim.approveDate>(cache, e.Row, true);
					PXUIFieldAttribute.SetEnabled<EPExpenseClaim.finPeriodID>(cache, e.Row, true);
				}
				if (claim.Hold != true && claim.Approved != true)
				{
					/*
					PXResultset<EPSetupApproval> setups = PXSelect<EPSetupApproval,
						Where<EPSetupApproval.claimType, Equal<Required<POSetupApproval.claimType>>>>.Select(this, claim.claimType);					
					int?[] maps = new int?[setups.Count];
					int i = 0;
					foreach (POSetupApproval item in setups)
						maps[i++] = EPSetup.claimAssignmentMapID;
					*/
					claim.WorkgroupID = null;
					claim.OwnerID = null;
					if (!Approval.Assign(claim, epsetup.Current.ClaimAssignmentMapID))
					{
						claim.Approved = true;
						claim.Status = EPClaimStatus.Approved;
						if (claim.ApproveDate == null)
							claim.ApproveDate = PXTimeZoneInfo.Today;
						cache.SetDefaultExt<EPExpenseClaim.finPeriodID>(claim);
					}
				}
			}
		}

		protected virtual void EPExpenseClaim_LocationID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			cache.SetDefaultExt<EPExpenseClaim.taxZoneID>(e.Row);
		}

		protected virtual void EPExpenseClaim_Approved_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			EPExpenseClaim doc = (EPExpenseClaim)e.Row;
			if (doc.Approved == true)
			{
				if (doc.ApproveDate == null)
					doc.ApproveDate = PXTimeZoneInfo.Today;
				doc.Status = EPClaimStatus.Approved;
				cache.SetDefaultExt<EPExpenseClaim.finPeriodID>(doc);
			}
		}
		protected virtual void EPExpenseClaim_Rejected_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			EPExpenseClaim doc = (EPExpenseClaim)e.Row;
			if (doc.Rejected == true)
			{
				doc.Approved = false;
				doc.Hold = true;
				((EPExpenseClaim)e.Row).FinPeriodID = null;
				((EPExpenseClaim)e.Row).TranPeriodID = null;
				doc.Status = EPClaimStatus.Voided;
				cache.RaiseFieldUpdated<EPExpenseClaim.hold>(e.Row, null);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaim.hold>(cache, e.Row, true);
			}
		}


		protected virtual void EPExpenseClaim_DocDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CurrencyInfoAttribute.SetEffectiveDate<EPExpenseClaim.docDate>(cache, e);
		}

		protected virtual void EPExpenseClaim_EmployeeID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			EPEmployee.RaiseFieldUpdated(cache, e.Row);

			if (cmsetup.Current.MCActivated == true)
			{
				CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<EPExpenseClaim.curyInfoID>(cache, e.Row);

				string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);
				if (string.IsNullOrEmpty(message) == false)
				{
					cache.RaiseExceptionHandling<EPExpenseClaim.docDate>(e.Row, ((EPExpenseClaim)e.Row).DocDate, new PXSetPropertyException(message, PXErrorLevel.Warning));
				}
				if (info != null)
				{
					((EPExpenseClaim)e.Row).CuryID = info.CuryID;
				}
			}
			cache.SetDefaultExt<EPExpenseClaim.locationID>(e.Row);
			cache.SetDefaultExt<EPExpenseClaim.departmentID>(e.Row);
		}

		protected virtual void EPExpenseClaimDetails_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			EPExpenseClaimDetails row = (EPExpenseClaimDetails)e.Row;
			if (row != null)
			{
				if ((bool)((EPExpenseClaimDetails)e.Row).Billable == true)
				{
					PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.salesAccountID>(cache, e.Row, true);
					PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.salesSubID>(cache, e.Row, true);
				}
				else
				{
					PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.salesAccountID>(cache, e.Row, false);
					PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.salesSubID>(cache, e.Row, false);
				}

			}

		}


		protected virtual void EPExpenseClaimDetails_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			EPExpenseClaimDetails line = (EPExpenseClaimDetails)e.Row;

			if (EPEmployee.Current != null)
			{
				InventoryItem item = PXSelectorAttribute.Select<InventoryItem.inventoryID>(cache, line) as InventoryItem;

				if (item == null)
				{
					if (line.ExpenseDate == null)
					{
						line.ExpenseDate = ExpenseClaim.Current.DocDate;
					}

					line.ExpenseAccountID = EPEmployee.Current.ExpenseAcctID;
					line.SalesAccountID = EPEmployee.Current.SalesAcctID;
				}
				else
				{
					line.ExpenseAccountID = item.COGSAcctID;
					if (line.Billable == true)
					{
						line.SalesAccountID = item.SalesAcctID;
					}
				}

                cache.SetDefaultExt<EPExpenseClaimDetails.expenseSubID>(line);
                cache.SetDefaultExt<EPExpenseClaimDetails.salesSubID>(line);
			}
		}

		protected virtual void EPExpenseClaimDetails_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			EPExpenseClaimDetails row = e.Row as EPExpenseClaimDetails;

			if ((bool)((EPExpenseClaimDetails)e.Row).Billable == true & ((EPExpenseClaimDetails)e.Row).CustomerID == null)
			{
				cache.RaiseExceptionHandling<EPExpenseClaimDetails.customerID>(e.Row, null, new PXSetPropertyException(Messages.CustomerRequired));
			}
			if (((EPExpenseClaimDetails)e.Row).CustomerID != null & ((EPExpenseClaimDetails)e.Row).CustomerLocationID == null)
			{
				cache.RaiseExceptionHandling<EPExpenseClaimDetails.customerLocationID>(e.Row, null, new PXSetPropertyException(Messages.CustomerLocationRequired));
			}

			if (row.ContractID != null)
			{
				PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, row.ContractID);
				if (project.CustomerID != null && row.CustomerID !=null)
				{
                    if (project.CustomerID != row.CustomerID)
					    cache.RaiseExceptionHandling<EPExpenseClaimDetails.contractID>(e.Row, null, new PXSetPropertyException(Messages.CustomerDoesNotMatchProject));
				}
				if ((bool) ((EPExpenseClaimDetails) e.Row).Billable && row.TaskID != null)
				{
					PMTask task = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<PMTask.taskID>>>>.Select(this, row.TaskID);
					if(task != null && !(bool)task.VisibleInAP)
						cache.RaiseExceptionHandling<EPExpenseClaimDetails.taskID>(e.Row, task.TaskCD, 
							new PXSetPropertyException(PM.Messages.TaskInvisibleInModule, task.TaskCD, BatchModule.AP));
				}
			}
		}

		public override void Persist()
		{
			List<EPExpenseClaim> inserted = null;
			if (epsetup.Current.HoldEntry != true)
			{
				inserted = new List<EPExpenseClaim>();
				foreach (EPExpenseClaim item in this.ExpenseClaim.Cache.Inserted)
					inserted.Add(item);
			}
			base.Persist();
			if (inserted != null)
			{
				foreach (EPExpenseClaim item in inserted)
				{
					if (this.ExpenseClaim.Cache.GetStatus(item) != PXEntryStatus.Inserted)
					{
						EPExpenseClaim row = PXCache<EPExpenseClaim>.CreateCopy(item);
						row.Hold = false;
						this.ExpenseClaim.Update(row);
					}
				}
				base.Persist();
			}
		}

        protected virtual void EPExpenseClaimDetails_ContractID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            EPExpenseClaimDetails row = e.Row as EPExpenseClaimDetails;
            PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, row.ContractID);
            if (project.CustomerID != null && row.CustomerID !=null && row.CustomerID != project.CustomerID)
                cache.RaiseExceptionHandling<EPExpenseClaimDetails.contractID>(e.Row, null, 
                    new PXSetPropertyException(Messages.CustomerDoesNotMatchProject, PXErrorLevel.Warning));

            cache.SetDefaultExt<EPExpenseClaimDetails.expenseSubID>(e.Row);
            cache.SetDefaultExt<EPExpenseClaimDetails.salesSubID>(e.Row);
        }

		protected virtual void EPExpenseClaimDetails_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			EPExpenseClaimDetails tran = e.Row as EPExpenseClaimDetails;

			cache.SetDefaultExt<EPExpenseClaimDetails.taxCategoryID>(e.Row);

			InventoryItem item = PXSelectorAttribute.Select<InventoryItem.inventoryID>(cache, tran) as InventoryItem;
			if (item != null && tran != null)
			{
				decimal curyStdCost;
				PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.curyInfoID>(currencyinfo.Cache, e.Row, item.StdCost.Value, out curyStdCost);

				tran.ExpenseAccountID = item.COGSAcctID;
				tran.UOM = item.PurchaseUnit;
				tran.CuryUnitCost = curyStdCost;
				tran.TranDesc = item.Descr;

				if (tran.Billable == true)
				{
					tran.SalesAccountID = item.SalesAcctID;
				}
			}
		}

		protected virtual void EPExpenseClaimDetails_TaxCategoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (TaxAttribute.GetTaxCalc<EPExpenseClaimDetails.taxCategoryID>(sender, e.Row) == TaxCalc.Calc && taxzone.Current != null && !string.IsNullOrEmpty(taxzone.Current.DfltTaxCategoryID) && ((EPExpenseClaimDetails)e.Row).InventoryID == null)
			{
				e.NewValue = taxzone.Current.DfltTaxCategoryID;
			}
		}

		protected virtual void EPExpenseClaimDetails_Billable_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			if ((bool)((EPExpenseClaimDetails)e.Row).Billable == true)
			{
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.salesAccountID>(cache, e.Row, true);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.salesSubID>(cache, e.Row, true);
				if (((EPExpenseClaimDetails)e.Row).CustomerID == null)
				{
					cache.RaiseExceptionHandling<EPExpenseClaimDetails.customerID>(e.Row, null, new PXSetPropertyException(Messages.CustomerRequired, PXErrorLevel.Warning));
				}

				InventoryItem item = PXSelectorAttribute.Select<InventoryItem.inventoryID>(cache, e.Row) as InventoryItem;
				if (item != null)
				{
					((EPExpenseClaimDetails)e.Row).SalesAccountID = item.SalesAcctID;
				}

			}
			else
			{
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.salesAccountID>(cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.salesSubID>(cache, e.Row, false);
			}
		}

		protected virtual void EPExpenseClaimDetails_CustomerID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
            EPExpenseClaimDetails row = e.Row as EPExpenseClaimDetails;
            
			if (row != null && row.CustomerID != null)
			{
                if (row.ContractID != null)
                {
                    PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, row.ContractID);

                    if (project.CustomerID != null && row.CustomerID != project.CustomerID)
                        cache.RaiseExceptionHandling<EPExpenseClaimDetails.customerID>(e.Row, null,
                                                                                       new PXSetPropertyException(Messages.CustomerDoesNotMatchProject,
                                                                                                                  PXErrorLevel.Warning));
                }
			    cache.SetDefaultExt<EPExpenseClaimDetails.customerLocationID>(e.Row);
			}
		}

		protected virtual void EPExpenseClaimDetails_CustomerLocationID_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
		{
			if (e.Row != null && ((EPExpenseClaimDetails)e.Row).CustomerID == null)
			{
				e.NewValue = null;
			}
		}

		protected virtual void EPExpenseClaimDetails_UOM_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			cache.SetDefaultExt<EPExpenseClaimDetails.unitCost>(e.Row);
			cache.SetDefaultExt<EPExpenseClaimDetails.curyUnitCost>(e.Row);
		}

		protected virtual void EPExpenseClaimDetails_ExpenseSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (location.Current != null && ((EPExpenseClaimDetails)e.Row).ExpenseAccountID != null)
			{
				InventoryItem item = (InventoryItem)PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, ((EPExpenseClaimDetails)e.Row).InventoryID);
				Location companyloc =
					(Location)PXSelectJoin<Location, InnerJoin<BAccountR, On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>, InnerJoin<GL.Branch, On<BAccountR.bAccountID, Equal<GL.Branch.bAccountID>>>>, Where<GL.Branch.branchID, Equal<Current<EPExpenseClaim.branchID>>>>.Select(this);
                Contract contract = PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(this, ((EPExpenseClaimDetails)e.Row).ContractID);
                PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(this, ((EPExpenseClaimDetails)e.Row).ContractID, ((EPExpenseClaimDetails)e.Row).TaskID);

				int? employee_SubID = (int?)Caches[typeof(EPEmployee)].GetValue<EPEmployee.expenseSubID>(EPEmployee.Current);
				int? item_SubID = (int?)Caches[typeof(InventoryItem)].GetValue<InventoryItem.cOGSSubID>(item);
				int? company_SubID = (int?)Caches[typeof(Location)].GetValue<Location.cMPExpenseSubID>(companyloc);
                int? project_SubID = (int?)Caches[typeof(Contract)].GetValue<Contract.defaultSubID>(contract);
                int? task_SubID = (int?)Caches[typeof(PMTask)].GetValue<PMTask.defaultSubID>(task);


				object value = SubAccountExpenseMaskAttribute.MakeSub<EPSetup.expenseSubMask>(this, EPSETUP.ExpenseSubMask,
					new object[] { employee_SubID, item_SubID, company_SubID, project_SubID, task_SubID },
                    new Type[] { typeof(EPEmployee.expenseSubID), typeof(InventoryItem.cOGSSubID), typeof(Location.cMPExpenseSubID), typeof(Contract.defaultSubID), typeof(PMTask.defaultSubID)});

				sender.RaiseFieldUpdating<EPExpenseClaimDetails.expenseSubID>(e.Row, ref value);

				e.NewValue = (int?)value;
				e.Cancel = true;
			}
		}

		protected virtual void EPExpenseClaimDetails_SalesSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (location.Current != null && ((EPExpenseClaimDetails)e.Row).SalesAccountID != null)
			{
				InventoryItem item = (InventoryItem)PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, ((EPExpenseClaimDetails)e.Row).InventoryID);
				Location companyloc =
					(Location)PXSelectJoin<Location, InnerJoin<BAccountR, On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>, InnerJoin<GL.Branch, On<BAccountR.bAccountID, Equal<GL.Branch.bAccountID>>>>, Where<GL.Branch.branchID, Equal<Current<EPExpenseClaim.branchID>>>>.Select(this);
                Contract contract = PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(this, ((EPExpenseClaimDetails)e.Row).ContractID);
                PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(this, ((EPExpenseClaimDetails)e.Row).ContractID, ((EPExpenseClaimDetails)e.Row).TaskID);
				Location customerLocation = (Location)PXSelectorAttribute.Select<EPExpenseClaimDetails.customerLocationID>(sender, e.Row);

				int? employee_SubID = (int?)Caches[typeof(EPEmployee)].GetValue<EPEmployee.salesSubID>(EPEmployee.Current);
				int? item_SubID = (int?)Caches[typeof(InventoryItem)].GetValue<InventoryItem.salesSubID>(item);
				int? company_SubID = (int?)Caches[typeof(Location)].GetValue<Location.cMPSalesSubID>(companyloc);
                int? project_SubID = (int?)Caches[typeof(Contract)].GetValue<Contract.defaultSubID>(contract);
                int? task_SubID = (int?)Caches[typeof(Location)].GetValue<PMTask.defaultSubID>(task);
				int? location_SubID = (int?)Caches[typeof(Location)].GetValue<Location.cSalesSubID>(customerLocation);

				object value = SubAccountSalesMaskAttribute.MakeSub<EPSetup.salesSubMask>(this, EPSETUP.SalesSubMask,
					new object[] { employee_SubID, item_SubID, company_SubID, project_SubID, task_SubID, location_SubID },
					new Type[] { typeof(EPEmployee.salesSubID), typeof(InventoryItem.salesSubID), typeof(Location.cMPSalesSubID), typeof(Contract.defaultSubID), typeof(PMTask.defaultSubID), typeof(Location.cSalesSubID) });

				sender.RaiseFieldUpdating<EPExpenseClaimDetails.salesSubID>(e.Row, ref value);

				e.NewValue = (int?)value;
				e.Cancel = true;
			}
		}

        protected virtual void EPExpenseClaimDetails_ExpenseAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            EPExpenseClaimDetails row = e.Row as EPExpenseClaimDetails;

            if(row != null && row.ContractID != null)
            {
                Contract contract = PXSelect<Contract>.Search<Contract.contractID>(this, row.ContractID);
                if (contract != null && contract.BaseType == PMProject.ProjectBaseType.Project && !ProjectDefaultAttribute.IsNonProject(this, row.ContractID))
                {
                    Account account = PXSelect<Account>.Search<Account.accountID>(this, row.ExpenseAccountID);
                    if (account.AccountGroupID == null)
                    {
                        sender.RaiseExceptionHandling<EPExpenseClaimDetails.expenseAccountID>(e.Row, account.AccountCD, 
                            new PXSetPropertyException(Messages.AccountGroupIsNotAssignedForAccount, account.AccountCD.Trim(), PXErrorLevel.Error));
                    }

                }    
            }
        }

	    protected virtual void EPExpenseClaimDetails_TaskID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
	    {
	        EPExpenseClaimDetails row = e.Row as EPExpenseClaimDetails;
	        if (row == null) return;

            sender.SetDefaultExt<EPExpenseClaimDetails.expenseSubID>(e.Row);
            sender.SetDefaultExt<EPExpenseClaimDetails.salesSubID>(e.Row);
	        
	    }

		#endregion

		#region EPApproval Cahce Attached
		[PXDBDate()]
		[PXDefault(typeof(EPExpenseClaim.docDate), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		[PXDefault(typeof(EPExpenseClaim.employeeID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_BAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(60, IsUnicode = true)]
		[PXDefault(typeof(EPExpenseClaim.docDesc), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_Descr_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong()]
		[CurrencyInfo(typeof(EPExpenseClaim.curyInfoID))]
		protected virtual void EPApproval_CuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal(4)]
		[PXDefault(typeof(EPExpenseClaim.curyDocBal), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_CuryTotalAmount_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal(4)]
		[PXDefault(typeof(EPExpenseClaim.docBal), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_TotalAmount_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region Function

		public void SubmitDetail(EPExpenseClaimDetails details)
		{
			details.RefNbr = ExpenseClaim.Current.RefNbr;
			ExpenseClaimDetails.Update(details);
		}

		#endregion

		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
	    {
            return true;
	    }

	    public bool RowImporting(string viewName, object row)
	    {
            return row == null;
	    }

	    public bool RowImported(string viewName, object row, object oldRow)
	    {
            return oldRow == null;
	    }

	    public void PrepareItems(string viewName, IEnumerable items) { }
	}
}
