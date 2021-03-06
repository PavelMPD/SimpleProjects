using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.IN
{
	public class INIssueEntry : PXGraph<INIssueEntry, INRegister>
	{
		public PXSelect<INRegister, Where<INRegister.docType, Equal<INDocType.issue>>> issue;
		public PXSelect<INRegister, Where<INRegister.docType, Equal<Current<INRegister.docType>>, And<INRegister.refNbr, Equal<Current<INRegister.refNbr>>>>> CurrentDocument;
        [PXImport(typeof(INRegister))]
		public PXSelect<INTran, Where<INTran.docType, Equal<Current<INRegister.docType>>, And<INTran.refNbr, Equal<Current<INRegister.refNbr>>>>> transactions;

		[PXCopyPasteHiddenView()]
		public PXSelect<INTranSplit, Where<INTranSplit.tranType, Equal<Current<INTran.tranType>>, And<INTranSplit.refNbr, Equal<Current<INTran.refNbr>>, And<INTranSplit.lineNbr, Equal<Current<INTran.lineNbr>>>>>> splits;
		public PXSetup<INSetup> insetup;

		public LSINTran lsselect;

		[PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<INTran.inventoryID>>>>))]
		[INUnit(typeof(INTran.inventoryID))]
		public virtual void INTran_UOM_CacheAttached(PXCache sender)
		{
		}
        [PXString(2)]
        [PXFormula(typeof(Parent<INRegister.origModule>))]
        public virtual void INTran_OrigModule_CacheAttached(PXCache sender)
        {
        }
        [PXString(2)]
        [PXFormula(typeof(Parent<INRegister.origModule>))]
        public virtual void INTranSplit_OrigModule_CacheAttached(PXCache sender)
        {
        }
        [IN.LocationAvail(typeof(INTran.inventoryID),
            typeof(INTran.subItemID),
            typeof(INTran.siteID),
            typeof(Where<INTran.tranType, 
                            Equal<INTranType.invoice>, 
                        Or<INTran.tranType, 
                            Equal<INTranType.debitMemo>, 
                        Or<INTran.origModule, 
                            NotEqual<GL.BatchModule.modulePO>, 
                        And<INTran.tranType, 
                            Equal<INTranType.issue>>>>>),
            typeof(Where<INTran.tranType, 
                            Equal<INTranType.receipt>, 
                         Or<INTran.tranType, 
                            Equal<INTranType.return_>, 
                         Or<INTran.tranType, 
                            Equal<INTranType.creditMemo>, 
                         Or<INTran.origModule, 
                            Equal<GL.BatchModule.modulePO>, 
                         And<INTran.tranType, 
                            Equal<INTranType.issue>>>>>>),
            typeof(Where<INTran.tranType, 
                            Equal<INTranType.transfer>, 
                         And<INTran.invtMult, 
                            Equal<short1>, 
                         Or<INTran.tranType, 
                            Equal<INTranType.transfer>, 
                         And<INTran.invtMult, 
                            Equal<shortMinus1>>>>>))]
        public virtual void INTran_LocationID_CacheAttached(PXCache sender)
        {
        }        
        [IN.LocationAvail(typeof(INTranSplit.inventoryID), 
            typeof(INTranSplit.subItemID),
            typeof(INTranSplit.siteID),
            typeof(Where<INTranSplit.tranType, 
                            Equal<INTranType.invoice>, 
                         Or<INTranSplit.tranType, 
                            Equal<INTranType.debitMemo>, 
                         Or<INTranSplit.origModule, 
                            NotEqual<GL.BatchModule.modulePO>, 
                         And<INTranSplit.tranType, 
                            Equal<INTranType.issue>>>>>),
            typeof(Where<INTranSplit.tranType, 
                            Equal<INTranType.receipt>, 
                         Or<INTranSplit.tranType, 
                            Equal<INTranType.return_>, 
                         Or<INTranSplit.tranType, 
                            Equal<INTranType.creditMemo>, 
                         Or<INTranSplit.origModule, 
                            Equal<GL.BatchModule.modulePO>, 
                         And<INTranSplit.tranType, 
                            Equal<INTranType.issue>>>>>>),
            typeof(Where<INTranSplit.tranType, 
                            Equal<INTranType.transfer>, 
                         And<INTranSplit.invtMult, 
                            Equal<short1>, 
                         Or<INTranSplit.tranType, 
                            Equal<INTranType.transfer>, 
                         And<INTranSplit.invtMult,
                            Equal<shortMinus1>>>>>))]
        [PXDefault()]
        public virtual void INTranSplit_LocationID_CacheAttached(PXCache sender)
        {
        }

		public PXAction<INRegister> viewBatch;
		[PXUIField(DisplayName = "Review Batch", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewBatch(PXAdapter adapter)
		{
			if (issue.Current != null && !String.IsNullOrEmpty(issue.Current.BatchNbr))
			{
				GL.JournalEntry graph = PXGraph.CreateInstance<GL.JournalEntry>();
				graph.BatchModule.Current = graph.BatchModule.Search<GL.Batch.batchNbr>(issue.Current.BatchNbr, "IN");
				throw new PXRedirectRequiredException(graph, "Current batch record");
			}
			return adapter.Get();
		}
		
		public PXAction<INRegister> release;
		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			PXCache cache =issue.Cache;
			List<INRegister> list = new List<INRegister>();
			foreach (INRegister indoc in adapter.Get<INRegister>())
			{
				if (indoc.Hold == false && indoc.Released == false)
				{
					cache.Update(indoc);
					list.Add(indoc);
				}
			}
			if (list.Count == 0)
			{
				throw new PXException(Messages.Document_Status_Invalid);
			}
			Save.Press();
			PXLongOperation.StartOperation(this, delegate() { INDocumentRelease.ReleaseDoc(list, false); });
			return list;
		}

        #region MyButtons (MMK)
        public PXAction<INRegister> report;
        [PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable Report(PXAdapter adapter)
        {
            return adapter.Get();
        }
        #endregion

		public PXAction<INRegister> iNEdit;
		[PXUIField(DisplayName = Messages.INEditDetails, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable INEdit(PXAdapter adapter)
		{
			if (issue.Current != null)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["DocType"] = issue.Current.DocType;
				parameters["RefNbr"] = issue.Current.RefNbr;
				parameters["PeriodTo"] = null;
				parameters["PeriodFrom"] = null;
				throw new PXReportRequiredException(parameters, "IN611000", Messages.INEditDetails);
			}
			return adapter.Get();
		}
		
		public PXAction<INRegister> inventorySummary;
		[PXUIField(DisplayName = "Inventory Summary", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable InventorySummary(PXAdapter adapter)
		{
			PXCache		tCache = transactions.Cache;
			INTran		line   = transactions.Current;
			if (line == null) return adapter.Get();

			InventoryItem item = (InventoryItem) PXSelectorAttribute.Select<INTran.inventoryID>(tCache, line);
			if (item != null && item.StkItem == true)
			{
				INSubItem	sbitem = (INSubItem) PXSelectorAttribute.Select<INTran.subItemID>(tCache, line);
				InventorySummaryEnq.Redirect(item.InventoryID, 
										     ((sbitem != null) ? sbitem.SubItemCD : null), 
										     line.SiteID, 
										     line.LocationID);
			}
			return adapter.Get();
		}

		public PXAction<INRegister> iNRegisterDetails;
		[PXUIField(DisplayName = Messages.INRegisterDetails, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable INRegisterDetails(PXAdapter adapter)
		{
			if (issue.Current != null)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["PeriodID"] = (string)issue.GetValueExt<INRegister.finPeriodID>(issue.Current);
				parameters["DocType"] = issue.Current.DocType;
				parameters["RefNbr"] = issue.Current.RefNbr;
				throw new PXReportRequiredException(parameters, "IN614000", Messages.INRegisterDetails);
			}
			return adapter.Get();
		}

		#region SiteStatus Lookup
		public PXFilter<INSiteStatusFilter> sitestatusfilter;
		[PXFilterable]
		[PXCopyPasteHiddenView]
		public INSiteStatusLookup<INSiteStatusSelected, INSiteStatusFilter> sitestatus;

		public PXAction<INRegister> addInvBySite;
		[PXUIField(DisplayName = "Add Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable AddInvBySite(PXAdapter adapter)
		{
			sitestatusfilter.Cache.Clear();
			if (sitestatus.AskExt() == WebDialogResult.OK)
			{
				return AddInvSelBySite(adapter);
			}
			sitestatusfilter.Cache.Clear();
			sitestatus.Cache.Clear();
			return adapter.Get();
		}

		public PXAction<INRegister> addInvSelBySite;
		[PXUIField(DisplayName = "Add", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddInvSelBySite(PXAdapter adapter)
		{
			foreach (INSiteStatusSelected line in sitestatus.Cache.Cached)
			{
				if (line.Selected == true && line.QtySelected > 0)
				{
					INTran newline = PXCache<INTran>.CreateCopy(this.transactions.Insert(new INTran()));
					newline.SiteID = line.SiteID;
					newline.InventoryID = line.InventoryID;
					newline.SubItemID = line.SubItemID;
					newline.UOM = line.BaseUnit;
					newline = PXCache<INTran>.CreateCopy(transactions.Update(newline));
					newline.LocationID = line.LocationID;
					newline = PXCache<INTran>.CreateCopy(transactions.Update(newline));
					newline.Qty = line.QtySelected;
					transactions.Update(newline);
				}
			}
			sitestatus.Cache.Clear();
			return adapter.Get();
		}

		protected virtual void INSiteStatusFilter_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			INSiteStatusFilter row = (INSiteStatusFilter)e.Row;
			if (row != null && issue.Current != null)
				row.SiteID = issue.Current.SiteID;
		}
		#endregion

		public INIssueEntry()
		{
			INSetup record = insetup.Current;

			PXStringListAttribute.SetList<INTran.tranType>(transactions.Cache, null, new INTranType.IssueListAttribute().AllowedValues, new INTranType.IssueListAttribute().AllowedLabels);
			//PXDimensionSelectorAttribute.SetValidCombo<INTran.subItemID>(transactions.Cache, true);
			//PXDimensionSelectorAttribute.SetValidCombo<INTranSplit.subItemID>(splits.Cache, true);
		}

		protected virtual void INRegister_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = INDocType.Issue;
		}

		protected virtual void INRegister_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (((INRegister)e.Row).DocType == INDocType.Undefined)
			{
				e.Cancel = true;
			}
		}

		protected virtual void INRegister_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (insetup.Current.RequireControlTotal == false)
			{
				if (PXCurrencyAttribute.IsNullOrEmpty(((INRegister)e.Row).TotalAmount) == false)
				{
					sender.SetValue<INRegister.controlAmount>(e.Row, ((INRegister)e.Row).TotalAmount);
				}
				else
				{
					sender.SetValue<INRegister.controlAmount>(e.Row, 0m);
				}

				if (PXCurrencyAttribute.IsNullOrEmpty(((INRegister)e.Row).TotalQty) == false)
				{
					sender.SetValue<INRegister.controlQty>(e.Row, ((INRegister)e.Row).TotalQty);
				}
				else
				{
					sender.SetValue<INRegister.controlQty>(e.Row, 0m);
				}
			}

			if (((INRegister)e.Row).Hold == false && ((INRegister)e.Row).Released == false)
			{
				if ((bool)insetup.Current.RequireControlTotal)
				{
					if (((INRegister)e.Row).TotalAmount != ((INRegister)e.Row).ControlAmount)
					{
						sender.RaiseExceptionHandling<INRegister.controlAmount>(e.Row, ((INRegister)e.Row).ControlAmount, new PXSetPropertyException(Messages.DocumentOutOfBalance));
					}
					else
					{
						sender.RaiseExceptionHandling<INRegister.controlAmount>(e.Row, ((INRegister)e.Row).ControlAmount, null);
					}

					if (((INRegister)e.Row).TotalQty != ((INRegister)e.Row).ControlQty)
					{
						sender.RaiseExceptionHandling<INRegister.controlQty>(e.Row, ((INRegister)e.Row).ControlQty, new PXSetPropertyException(Messages.DocumentOutOfBalance));
					}
					else
					{
						sender.RaiseExceptionHandling<INRegister.controlQty>(e.Row, ((INRegister)e.Row).ControlQty, null);
					}
				}
			}
		}

		protected virtual void INRegister_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			release.SetEnabled(e.Row != null && ((INRegister)e.Row).Hold == false && ((INRegister)e.Row).Released == false);
			iNEdit.SetEnabled(e.Row != null && ((INRegister)e.Row).Hold == false && ((INRegister)e.Row).Released == false);
			iNRegisterDetails.SetEnabled(e.Row != null && ((INRegister)e.Row).Released == true);

			PXUIFieldAttribute.SetEnabled(sender, e.Row, ((INRegister)e.Row).Released == false && ((INRegister)e.Row).OrigModule == GL.BatchModule.IN);
			PXUIFieldAttribute.SetEnabled<INRegister.refNbr>(sender, e.Row, true);
			PXUIFieldAttribute.SetEnabled<INRegister.totalQty>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<INRegister.totalAmount>(sender, e.Row, false);
            PXUIFieldAttribute.SetEnabled<INRegister.totalCost>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<INRegister.status>(sender, e.Row, false);

			sender.AllowInsert = true;
			sender.AllowUpdate = (((INRegister)e.Row).Released == false);
			sender.AllowDelete = (((INRegister)e.Row).Released == false && ((INRegister)e.Row).OrigModule == GL.BatchModule.IN);

			lsselect.AllowInsert = (((INRegister)e.Row).Released == false && ((INRegister)e.Row).OrigModule == GL.BatchModule.IN);
			lsselect.AllowUpdate = (((INRegister)e.Row).Released == false);
			lsselect.AllowDelete = (((INRegister)e.Row).Released == false && ((INRegister)e.Row).OrigModule == GL.BatchModule.IN);

			PXUIFieldAttribute.SetVisible<INRegister.controlQty>(sender, e.Row, (bool)insetup.Current.RequireControlTotal);
			PXUIFieldAttribute.SetVisible<INRegister.controlAmount>(sender, e.Row, (bool)insetup.Current.RequireControlTotal);
			PXUIFieldAttribute.SetVisible<INTran.projectID>(transactions.Cache, null, IsPMVisible);
			PXUIFieldAttribute.SetVisible<INTran.taskID>(transactions.Cache, null, IsPMVisible);
            PXUIFieldAttribute.SetVisible<INRegister.totalCost>(sender, e.Row, ((INRegister)e.Row).Released == true);
		}

		protected virtual void INTran_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = INDocType.Issue;
		}

		protected virtual void INTran_TranType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = INTranType.Issue;
		}

		protected virtual void INTran_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = INTranType.InvtMult(((INTran)e.Row).TranType);
		}

		protected virtual void INTran_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<INTran.uOM>(e.Row);
			sender.SetDefaultExt<INTran.tranDesc>(e.Row);
		}

		protected virtual void INTran_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DefaultUnitPrice(sender, e);
			DefaultUnitCost(sender, e);
		}

		protected virtual void INTran_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DefaultUnitPrice(sender, e);
			DefaultUnitCost(sender, e);
		}

		protected virtual void INTran_SOOrderNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void INTran_SOShipmentNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void INTran_ReasonCode_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INTran row = e.Row as INTran;
			if (row != null)
			{
				ReasonCode reasoncd = PXSelect<ReasonCode, Where<ReasonCode.reasonCodeID, Equal<Optional<ReasonCode.reasonCodeID>>>>.Select(this, e.NewValue);

				if (reasoncd != null && row.ProjectID != null && !ProjectDefaultAttribute.IsNonProject(this, row.ProjectID))
				{
					PX.Objects.GL.Account account = PXSelect<PX.Objects.GL.Account, Where<PX.Objects.GL.Account.accountID, Equal<Required<PX.Objects.GL.Account.accountID>>>>.Select(this, reasoncd.AccountID);
					if (account != null && account.AccountGroupID == null)
					{
						sender.RaiseExceptionHandling<INTran.reasonCode>(e.Row, account.AccountCD, new PXSetPropertyException(PM.Messages.NoAccountGroup, PXErrorLevel.Warning, account.AccountCD));
					}
				}
				
				e.Cancel = (reasoncd != null) &&
					(row.TranType != INTranType.Issue && row.TranType != INTranType.Receipt && reasoncd.Usage == ReasonCodeUsages.Sales || reasoncd.Usage == row.DocType);

				
			}
		}

		protected virtual void INTran_LocationID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (issue.Current != null && issue.Current.OrigModule != GL.BatchModule.IN)
			{
				e.Cancel = true;
			}
		}

		protected virtual void INTranSplit_LocationID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (issue.Current != null && issue.Current.OrigModule != GL.BatchModule.IN)
			{
				e.Cancel = true;
			}
		}

		protected virtual void INTran_LotSerialNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (issue.Current != null && issue.Current.OrigModule != GL.BatchModule.IN)
			{
				e.Cancel = true;
			}
		}

		protected virtual void INTranSplit_LotSerialNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (issue.Current != null && issue.Current.OrigModule != GL.BatchModule.IN)
			{
				e.Cancel = true;
			}
		}

		protected virtual void INTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<INTran.unitCost>(sender, e.Row, ((INTran)e.Row).InvtMult == 1);
				PXUIFieldAttribute.SetEnabled<INTran.tranCost>(sender, e.Row, ((INTran)e.Row).InvtMult == 1);

			
			}
		}

		protected virtual void INTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			INTran row = (INTran)e.Row;
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				if (!string.IsNullOrEmpty(row.SOShipmentNbr))
				{
					if (PXDBQuantityAttribute.Round(sender, (decimal)(row.Qty + row.OrigQty)) > 0m)
					{
						sender.RaiseExceptionHandling<INTran.qty>(row, row.Qty, new PXSetPropertyException(CS.Messages.Entry_LE, -row.OrigQty));
					}
					else if (PXDBQuantityAttribute.Round(sender, (decimal)(row.Qty + row.OrigQty)) < 0m)
					{
						sender.RaiseExceptionHandling<INTran.qty>(row, row.Qty, new PXSetPropertyException(CS.Messages.Entry_GE, -row.OrigQty));
					}
				}
			}
		}

		protected virtual void DefaultUnitPrice(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			object UnitPrice;
			sender.RaiseFieldDefaulting<INTran.unitPrice>(e.Row, out UnitPrice);

			if (UnitPrice != null && (decimal)UnitPrice != 0m)
			{
				decimal? unitprice = INUnitAttribute.ConvertToBase<INTran.inventoryID>(sender, e.Row, ((INTran)e.Row).UOM, (decimal)UnitPrice, INPrecision.UNITCOST);
				sender.SetValueExt<INTran.unitPrice>(e.Row, unitprice);
			}
		}

		protected virtual void DefaultUnitCost(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			object UnitCost;
			sender.RaiseFieldDefaulting<INTran.unitCost>(e.Row, out UnitCost);

			if (UnitCost != null && (decimal)UnitCost != 0m)
			{
				decimal? unitcost = INUnitAttribute.ConvertToBase<INTran.inventoryID>(sender, e.Row, ((INTran)e.Row).UOM, (decimal)UnitCost, INPrecision.UNITCOST);
				sender.SetValueExt<INTran.unitCost>(e.Row, unitcost);
			}
		}

		protected virtual bool IsPMVisible
		{
			get
			{
				PM.PMSetup setup = PXSelect<PM.PMSetup>.Select(this);
				if (setup == null)
				{
					return false;
				}
				else
				{
					if (setup.IsActive != true)
						return false;
					else
						return setup.VisibleInIN == true;
				}
			}
		}
	}
}
