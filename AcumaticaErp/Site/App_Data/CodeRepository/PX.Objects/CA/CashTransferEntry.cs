using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.Objects.CM;
using System.Collections;

namespace PX.Objects.CA
{
	public class CashTransferEntry : PXGraph<CashTransferEntry, CATransfer>
	{
		#region Selects
		public class TransferView : PXView
		{
			public bool JustPersisted;
			public TransferView(PXGraph graph)
				: base(graph, false, new Select<CATransfer>())
			{
			}
			public override List<object> Select(object[] currents, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
			{
				if (JustPersisted && Cache.Current != null)
				{
					List<object> ret = new List<object>();
					ret.Add(Cache.Current);
					return ret;
				}
				return base.Select(currents, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
			}
		}
		public class TransferSelect : PXSelect<CATransfer>
		{
			public TransferSelect(PXGraph graph)
				: base(graph)
			{
				_Graph = graph;
				View = new TransferView(graph);
			}
			public bool JustPersisted
			{
				set
				{
					((TransferView)View).JustPersisted = value;
				}
			}
		}
		public TransferSelect Transfer;
		public PXFilter<AddTrxFilter> AddFilter;
		public PXSelectJoin<CATran, LeftJoin<CAAdj, On<CAAdj.tranID,Equal<CATran.tranID>>, LeftJoin<CASplit, On<CASplit.adjTranType, Equal<CAAdj.adjTranType>, And<CASplit.adjRefNbr, Equal<CAAdj.adjRefNbr>>>>>, Where<CATran.origTranType, Equal<CATranType.cATransferExp>, And<CATran.origRefNbr, Equal<Current<CATransfer.transferNbr>>>>> TransferTran;

		public ToggleCurrency<CATransfer> CurrencyView;

		public PXSetup<ARSetup> ARSetup;
		public PXSetup<CASetup> CASetup;
		public CMSetupSelect cmsetup;

		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<CATransfer.outCuryInfoID>>>> CurInfoOUT;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<CATransfer.inCuryInfoID>>>>  CurInfoIN;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<AddTrxFilter.curyInfoID>>>> currencyinfo_addfilter;
		#endregion
		
		#region Functions

		public CashTransferEntry()
		{
			CASetup setup = CASetup.Current;
			Views["_AddTrxFilter.curyInfoID_CurrencyInfo.CuryInfoID_"] = new PXView(this, false, new Select<AddTrxFilter>(), new PXSelectDelegate(AddFilter.Get));

			this.FieldSelecting.AddHandler<CATransfer.tranIDOut_CATran_batchNbr>(CATransfer_TranIDOut_CATran_BatchNbr_FieldSelecting);
			this.FieldSelecting.AddHandler<CATransfer.tranIDIn_CATran_batchNbr>(CATransfer_TranIDIn_CATran_BatchNbr_FieldSelecting);

		}
    
		public override void Clear()
		{
			AddFilter.Current.TranDate = null;
			AddFilter.Current.FinPeriodID = null;
			AddFilter.Current.CuryInfoID = null;
			base.Clear();
		}

		public override void Persist()
		{
			using (new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					List<CATran> deleted = new List<CATran>();
					RowPersisting.AddHandler<CATran>((sender, e) =>
						{
							if (deleted.Contains((CATran)e.Row))
							{
								e.Cancel = true;
							}
						});
					CATranEntry graph = null;
					foreach (CATran catran in TransferTran.Cache.Deleted)
					{
						if (graph == null)
						{
							graph = PXGraph.CreateInstance<CATranEntry>();
						}
						CAAdj adj = PXSelect<CAAdj, Where<CAAdj.tranID, Equal<Required<CAAdj.tranID>>>>.Select(graph, catran.TranID);
						if (adj != null)
						{
							graph.TimeStamp = TimeStamp;
							graph.CAAdjRecords.Delete(adj);
							graph.Save.Press();
							graph.Clear();
							deleted.Add(catran);
						}
					}
					base.Persist();
					ts.Complete();
					foreach (CATran catran in deleted)
					{
						TransferTran.Cache.Remove(catran);
					}
					deleted.Clear();
				}
			}

			//Current should be the only one inserted
			PXCache currencycache = Caches[typeof(CM.CurrencyInfo)];
			currencycache.SetStatus(currencycache.Current, PXEntryStatus.Inserted);
		}
		private void CalculateTranIn(CATransfer transfer)
		{
			decimal resultValue		= decimal.Zero;
			decimal resultBaseValue = decimal.Zero;

			if (	(transfer.CuryTranOut != 0m)  &&
					(CurInfoIN.Current	  != null))
			{
				PXSelectBase<CurrencyInfo> currencyInfoStatement = new PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<CATransfer.inCuryInfoID>>>>(this);
				CurrencyInfo			   cinfo			     = (CurrencyInfo) currencyInfoStatement.Select();

				if (transfer.OutCuryID == transfer.InCuryID)
				{
					transfer.CuryTranIn = transfer.CuryTranOut;
					transfer.TranIn		= transfer.TranOut;
				}
				else 
				{
					PXDBCurrencyAttribute.CuryConvCury(currencyInfoStatement.Cache, cinfo, (transfer.TranOut ?? decimal.Zero), out resultValue);
					transfer.CuryTranIn = resultValue;
					PXDBCurrencyAttribute.CuryConvBase(currencyInfoStatement.Cache, cinfo, (transfer.CuryTranIn ?? decimal.Zero), out resultBaseValue);
					transfer.TranIn = resultBaseValue;
				}
			}
		}
		#endregion		

		#region CATran Envents
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Number", Enabled = false)]
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleCA>>>))]
		public virtual void CATran_BatchNbr_CacheAttached(PXCache sender)
		{ 
		}
		#endregion

		#region CurrencyInfo Events
		protected virtual void CurrencyInfo_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			if (AddFilter.Current != null && AddFilter.Current.CuryInfoID == ((CurrencyInfo)e.Row).CuryInfoID)
			{
				e.Cancel = true;
			} 
		}

		protected virtual void CurrencyInfo_RowUpdated(PXCache sender, PXRowUpdatedEventArgs  e)
		{
			foreach (CATransfer doc in PXSelect<CATransfer, Where<CATransfer.outCuryInfoID, Equal<Required<CATransfer.outCuryInfoID>>>>.Select(sender.Graph, ((CurrencyInfo)e.Row).CuryInfoID))
			{
				PXCurrencyAttribute.CalcBaseValues<CATransfer.curyTranOut>(Transfer.Cache, doc);
				CalculateTranIn(doc);

				if (Transfer.Cache.GetStatus(doc) == PXEntryStatus.Notchanged)
				{
					Transfer.Cache.SetStatus(doc, PXEntryStatus.Updated);
				}

			}

			foreach (CATransfer doc in PXSelect<CATransfer, Where<CATransfer.inCuryInfoID, Equal<Required<CATransfer.inCuryInfoID>>>>.Select(sender.Graph, ((CurrencyInfo)e.Row).CuryInfoID))
			{
				CalculateTranIn(doc);

				if (Transfer.Cache.GetStatus(doc) == PXEntryStatus.Notchanged)
				{
					Transfer.Cache.SetStatus(doc, PXEntryStatus.Updated);
				}
			}
		}

		#endregion

		#region CATransfer Events

		protected virtual void CATransfer_TranIDIn_CATran_BatchNbr_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row == null || e.IsAltered)
			{
				string ViewName = null;
				PXCache cache = sender.Graph.Caches[typeof(CATran)];
				PXFieldState state = cache.GetStateExt<CATran.batchNbr>(null) as PXFieldState;
				if (state != null)
				{
					ViewName = state.ViewName;
				}

				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, false, false, 0, 0, null, null, null, null, null, null, PXErrorLevel.Undefined, false, true, true, PXUIVisibility.Visible, ViewName, null, null);
			}
		}

		protected virtual void CATransfer_TranIDOut_CATran_BatchNbr_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row == null || e.IsAltered)
			{
				string ViewName = null;
				PXCache cache = sender.Graph.Caches[typeof(CATran)];
				PXFieldState state = cache.GetStateExt<CATran.batchNbr>(null) as PXFieldState;
				if (state != null)
				{
					ViewName = state.ViewName;
				}

				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, false, false, 0, 0, null, null, null, null, null, null, PXErrorLevel.Undefined, false, true, true, PXUIVisibility.Visible, ViewName, null, null);
			}
		}

		protected virtual void CATransfer_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			CATransfer transfer	   = (CATransfer)e.Row;
			CATransfer newTransfer = (CATransfer)e.NewRow;
			if (	(newTransfer.TranOut     != transfer.TranOut)  ||
					(newTransfer.InCuryID    != transfer.InCuryID)   ||
					(newTransfer.InAccountID != transfer.InAccountID))
			{
				CalculateTranIn(newTransfer);
			}
		}

        protected virtual void CATransfer_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            CATransfer transfer = (CATransfer)e.Row;
            if (transfer == null) return;
			CMSetup setup = cmsetup.Current;

			transfer.RGOLAmt = transfer.TranIn - transfer.TranOut;

			bool transferOnHold      = (transfer.Hold == true);
			bool transferNotReleased = (transfer.Released != true);
			bool transferReleased    = (transfer.Released == true);
			bool msActivated = (setup.MCActivated == true);
			PXUIFieldAttribute.SetVisible<CATransfer.inCuryID>			       (sender, transfer, msActivated);
			PXUIFieldAttribute.SetVisible<CATransfer.outCuryID>			       (sender, transfer, msActivated);
			PXUIFieldAttribute.SetVisible<CATransfer.rGOLAmt>			       (sender, transfer, msActivated);
            PXUIFieldAttribute.SetVisible<CATransfer.inGLBalance>			   (sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetVisible<CATransfer.outGLBalance>			   (sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetVisible<CATransfer.cashBalanceIn>			   (sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetVisible<CATransfer.cashBalanceOut>		   (sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetVisible<CATransfer.tranIDIn_CATran_batchNbr> (sender, transfer, transferReleased);
			PXUIFieldAttribute.SetVisible<CATransfer.tranIDOut_CATran_batchNbr>(sender, transfer, transferReleased);
			
			PXUIFieldAttribute.SetEnabled                        (sender, transfer, false);
            
            
            AddTrxFilter filter = AddFilter.Current;
            if (filter != null)
            {
			    PXUIFieldAttribute.SetEnabled(AddFilter.Cache, filter, transferNotReleased);
                AddFilter.Cache.AllowUpdate = transferNotReleased;
            }

			sender.AllowDelete = transferNotReleased;
			sender.AllowUpdate = transferNotReleased;
			TransferTran.Cache.AllowDelete = transferNotReleased;
			
			
            CashAccount cashaccountOut = (CashAccount)PXSelectorAttribute.Select<CATransfer.outAccountID>(sender, e.Row);
            CashAccount cashaccountIn  = (CashAccount)PXSelectorAttribute.Select<CATransfer.inAccountID>(sender, e.Row);

			bool clearEnabledIn  = transferNotReleased && (cashaccountIn != null)  && (cashaccountIn.Reconcile == true);
			bool clearEnabledOut = transferNotReleased && (cashaccountOut != null) && (cashaccountOut.Reconcile == true);
			
			PXUIFieldAttribute.SetEnabled<CATransfer.hold>        (sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetEnabled<CATransfer.transferNbr> (sender, transfer, true);
            PXUIFieldAttribute.SetEnabled<CATransfer.descr>       (sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetEnabled<CATransfer.curyTranIn>  (sender, transfer, transferNotReleased && (transfer.OutCuryID != transfer.InCuryID));
			PXUIFieldAttribute.SetEnabled<CATransfer.inAccountID> (sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetEnabled<CATransfer.inDate>      (sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetEnabled<CATransfer.inExtRefNbr> (sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetEnabled<CATransfer.curyTranOut> (sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetEnabled<CATransfer.outAccountID>(sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetEnabled<CATransfer.outDate>     (sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetEnabled<CATransfer.outExtRefNbr>(sender, transfer, transferNotReleased);
            PXUIFieldAttribute.SetEnabled<CATransfer.clearedOut>  (sender, transfer, clearEnabledOut);
            PXUIFieldAttribute.SetEnabled<CATransfer.clearDateOut>(sender, transfer, clearEnabledOut && transfer.ClearedOut == true);
            PXUIFieldAttribute.SetEnabled<CATransfer.clearedIn>   (sender, transfer, clearEnabledIn);
            PXUIFieldAttribute.SetEnabled<CATransfer.clearDateIn> (sender, transfer, clearEnabledIn && transfer.ClearedIn == true);
		

			TransferTran.Cache.AllowInsert = false;
			TransferTran.Cache.AllowUpdate = false;

			AddFilter.Cache.RaiseRowSelected(AddFilter.Current);

			Release.SetEnabled(transferNotReleased && !transferOnHold);
			PrepareAdd.SetEnabled(transferNotReleased);
		}

		public virtual void CATransfer_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			foreach (CATran item in TransferTran.Select())
			{
				TransferTran.Delete(item);
			}
		}

		protected virtual void CATransfer_OutAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{

            CATransfer transfer = e.Row as CATransfer;
            CashAccount  cashaccountOut = (CashAccount)PXSelectorAttribute.Select<CATransfer.outAccountID>(sender, e.Row);

			sender.SetDefaultExt<CATransfer.outCuryID>(transfer);
			CurrencyInfo currinfo = CurInfoOUT.Select();

            if ((cashaccountOut != null) && (cashaccountOut.CuryRateTypeID != null))
			{
                currinfo.CuryRateTypeID = cashaccountOut.CuryRateTypeID;
				CurInfoOUT.Cache.RaiseFieldUpdated<CurrencyInfo.curyRateTypeID>(currinfo, null);
			}
            transfer.ClearedOut   = false;
            transfer.ClearDateOut = null;

            if ((cashaccountOut != null) && (cashaccountOut.Reconcile != true))
            {
                transfer.ClearedOut   = true;
                transfer.ClearDateOut = transfer.OutDate;
            }
		}

		protected virtual void CATransfer_InAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CATransfer transfer = e.Row as CATransfer;

            CashAccount cashaccountIn = (CashAccount)PXSelectorAttribute.Select<CATransfer.inAccountID>(sender, transfer);

			sender.SetDefaultExt<CATransfer.inCuryID>(transfer);

			CurrencyInfo currinfo = CurInfoIN.Select();

            if ((cashaccountIn != null) && (cashaccountIn.CuryRateTypeID != null))
			{
                currinfo.CuryRateTypeID = cashaccountIn.CuryRateTypeID;
				CurInfoIN.Cache.RaiseFieldUpdated<CurrencyInfo.curyRateTypeID>(currinfo, null);
			}

			//CalculateCuryTranIn(transfer);

            transfer.ClearedIn   = false;
            transfer.ClearDateIn = null;

            if ((cashaccountIn != null) && (cashaccountIn.Reconcile != true))
            {
                transfer.ClearedIn   = true;
                transfer.ClearDateIn = transfer.InDate;
            }
		}

		protected virtual void CATransfer_InDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CurrencyInfoAttribute.SetEffectiveDate<CATransfer.inDate, CATransfer.inCuryInfoID>(sender, e);
		}

		protected virtual void CATransfer_OutDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CurrencyInfoAttribute.SetEffectiveDate<CATransfer.outDate, CATransfer.outCuryInfoID>(sender, e);
		}

		protected virtual void CATransfer_OutExtRefNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CATransfer transfer = (CATransfer)e.Row;

            if (string.IsNullOrEmpty(transfer.InExtRefNbr))
			{
                transfer.InExtRefNbr = transfer.OutExtRefNbr;
			}
		}
		

        protected virtual void CATransfer_Hold_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{

            CATransfer transfer = (CATransfer)e.Row;

            if ((bool)e.NewValue != true)
            {
                if (transfer.TranOut == 0)
                {
                    cache.RaiseExceptionHandling<CATransfer.curyTranOut>(transfer, transfer.CuryTranOut, new PXSetPropertyException(Messages.DocumentOutOfBalance));
                }
            }
        }

        protected virtual void CATransfer_ClearedIn_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CATransfer transfer = (CATransfer)e.Row;
            if (transfer.ClearedIn == true)
            {
                if (transfer.ClearDateIn == null)
                    transfer.ClearDateIn = transfer.InDate;
            }
            else
            {
                transfer.ClearDateIn = null;   
            }
        }
        protected virtual void CATransfer_ClearedOut_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CATransfer transfer = (CATransfer)e.Row;
            if (transfer.ClearedOut == true)
            {
                if (transfer.ClearDateOut == null)
                    transfer.ClearDateOut = transfer.OutDate;
            }
            else
            {
                transfer.ClearDateOut = null;
            }
        }

		protected virtual void CATransfer_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			CATransfer transfer = (CATransfer)e.Row;

			if (transfer.OutAccountID == null)
			{
                sender.RaiseExceptionHandling<CATransfer.outAccountID>(transfer, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(CATransfer.outAccountID).Name));
			}

			if (string.IsNullOrEmpty(transfer.OutExtRefNbr))
			{
                sender.RaiseExceptionHandling<CATransfer.outExtRefNbr>(transfer, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(CATransfer.outExtRefNbr).Name));
			}

			if (transfer.InAccountID == null)
			{
                sender.RaiseExceptionHandling<CATransfer.inAccountID>(transfer, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(CATransfer.inAccountID).Name));
			}

			if (string.IsNullOrEmpty(transfer.InExtRefNbr))
			{
                sender.RaiseExceptionHandling<CATransfer.inExtRefNbr>(transfer, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(CATransfer.inExtRefNbr).Name));
			}

			if (transfer.OutAccountID == transfer.InAccountID)
			{
                sender.RaiseExceptionHandling<CATransfer.inAccountID>(transfer, null, new PXSetPropertyException(Messages.TransferCAAreEquals));
			}
			if (transfer.Hold == false && (transfer.TranOut == null || transfer.TranOut == 0))
			{
                sender.RaiseExceptionHandling<CATransfer.curyTranOut>(transfer, transfer.CuryTranOut, new PXSetPropertyException(Messages.DocumentOutOfBalance));
			}			
		}
		#endregion
	
		#region AddTrxFilter Events
		public virtual void AddTrxFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			//controls will be enabled correctly in attribute
			PXUIFieldAttribute.SetEnabled<AddTrxFilter.cashAccountID>(sender, e.Row, true);
			PXUIFieldAttribute.SetEnabled<AddTrxFilter.subID>(sender, e.Row, true);
		}
		protected virtual void AddTrxFilter_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			AddTrxFilter filter = (AddTrxFilter)e.Row;
			filter.OnlyExpense = true;
		}
		#endregion

		#region Buttons
		public PXAction<CATransfer> ViewDoc;
		[PXUIField(DisplayName = Messages.ViewExpense, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable viewDoc(PXAdapter adapter)
		{

			CATran catran = (CATran)(TransferTran.Current);

			if (catran != null)
			{
				CATranEntry graph = PXGraph.CreateInstance<CATranEntry>();
				graph.Clear();
				Transfer.Cache.IsDirty = false;
				graph.CAAdjRecords.Current = PXSelect<CAAdj, Where<CAAdj.tranID, Equal<Required<CATran.tranID>>>>.Select(this, catran.TranID);
				throw new PXRedirectRequiredException(graph, true, "Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		public PXAction<CATransfer> PrepareAdd;
		[PXUIField(DisplayName = "Add Expense", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable prepareAdd(PXAdapter adapter)
		{
			if (Transfer.Current != null && Transfer.Current.Released != true)
			{
				Save.Press();
				Transfer.JustPersisted = true;
                
                //recreate currencyinfo on CurrencyInfoAttribute.RowUpdating
                AddFilter.Update(AddFilter.Current);

				if (this.Views.ContainsKey("AddFilter"))
				{
					WebDialogResult result = this.Views["AddFilter"].AskExt(true);
					if (result == WebDialogResult.OK)
					{
						using (new PXTimeStampScope(this.TimeStamp))
						{
							CATran catran = null;
							catran = (catran ?? AddTrxFilter.AddAPTransaction(this, AddFilter, currencyinfo_addfilter));
							catran = (catran ?? AddTrxFilter.AddARTransaction(this, AddFilter, currencyinfo_addfilter));
							catran = (catran ?? AddTrxFilter.AddCATransaction(this, AddFilter, currencyinfo_addfilter));
							AddTrxFilter.Clear(this, AddFilter);

							catran = TransferTran.Update(catran);
							Save.Press();
						}
					}
				}
			}
			else
			{
				throw new Exception(Messages.DocumentStatusInvalid);
			}
			return adapter.Get();
		}

		public PXAction<CATransfer> Release;
		[PXUIField(DisplayName = "Release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable release(PXAdapter adapter)
		{
			PXCache cache = Caches[typeof(CATransfer)];
			if (PXLongOperation.Exists(UID))
			{
				throw new ApplicationException(GL.Messages.PrevOperationNotCompleteYet);
			}
			Save.Press();
            CATransfer transfer = Transfer.Current;
            List<CARegister> list = new List<CARegister>();
            CATran tran = PXSelect<CATran, Where<CATran.tranID, Equal<Required<CATransfer.tranIn>>>>.Select(this, transfer.TranIDIn);
            if (tran != null)
                list.Add(CATrxRelease.CARegister(transfer, tran));
            else
                throw new PXException(Messages.TransactionNotFound);
            
			tran = PXSelect<CATran, Where<CATran.tranID, Equal<Required<CATransfer.tranIn>>>>.Select(this, transfer.TranIDOut);
            if (tran == null)
                throw new PXException(Messages.TransactionNotFound);

            PXLongOperation.StartOperation(this, delegate() {CATrxRelease.GroupRelease(list, false); });

			List<CATransfer> ret = new List<CATransfer>();
			ret.Add(transfer);
			return ret;
		}
		
		public PXAction<CATransfer> viewOutBatch;
		[PXUIField(DisplayName = "View Batch", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewOutBatch(PXAdapter adapter)
		{
			if (Transfer.Current != null)
			{
				string BatchNbr = (string)Transfer.GetValueExt<CATransfer.tranIDOut_CATran_batchNbr>(Transfer.Current);
				if (BatchNbr != null)
				{
					JournalEntry graph = PXGraph.CreateInstance<JournalEntry>();
					graph.Clear();
					Batch newBatch = new Batch();
					graph.BatchModule.Current = PXSelect<GL.Batch,
							Where<GL.Batch.module, Equal<GL.BatchModule.moduleCA>,
							And<GL.Batch.batchNbr, Equal<Required<GL.Batch.batchNbr>>>>>
							.Select(this, BatchNbr);
					throw new PXRedirectRequiredException(graph, "Batch Record");
				}
			}
			return adapter.Get();
		}

		public PXAction<CATransfer> viewInBatch;
		[PXUIField(DisplayName = "View Batch", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewInBatch(PXAdapter adapter)
		{
			if (Transfer.Current != null)
			{
				string BatchNbr = (string)Transfer.GetValueExt<CATransfer.tranIDIn_CATran_batchNbr>(Transfer.Current);
				if (BatchNbr != null)
				{
					JournalEntry graph = PXGraph.CreateInstance<JournalEntry>();
					graph.Clear();
					Batch newBatch = new Batch();
					graph.BatchModule.Current = PXSelect<GL.Batch,
							Where<GL.Batch.module, Equal<GL.BatchModule.moduleCA>,
							And<GL.Batch.batchNbr, Equal<Required<GL.Batch.batchNbr>>>>>
							.Select(this, BatchNbr);
					throw new PXRedirectRequiredException(graph, "Batch Record");
				}
			}
			return adapter.Get();
		}
		#endregion
	}
}
