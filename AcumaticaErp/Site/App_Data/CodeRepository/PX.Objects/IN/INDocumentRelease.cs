using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.IN.Overrides.INDocumentRelease;
using PX.Objects.SO;
using PX.Objects.AP;

namespace PX.Objects.IN
{
	public class PXMassProcessException : PXException
	{
		protected Exception _InnerException;
		protected int _ListIndex;

		public int ListIndex
		{
			get
			{
				return this._ListIndex;
			}
		}

		public PXMassProcessException(int ListIndex, Exception InnerException)
			: base(InnerException is PXOuterException ? InnerException.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)InnerException).InnerMessages) : InnerException.Message, InnerException)
		{
			this._ListIndex = ListIndex;
		}

		public PXMassProcessException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			PXReflectionSerializer.RestoreObjectProps(this, info);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			PXReflectionSerializer.GetObjectData(this, info);
			base.GetObjectData(info, context);
		}
	}

    public class PXQtyCostImbalanceException : PXException
    {
    }

	[PX.Objects.GL.TableAndChartDashboardType]
	public class INDocumentRelease : PXGraph<INDocumentRelease>
	{
		public PXCancel<INRegister> Cancel;
		[PXFilterable]
		public PXProcessing<INRegister, Where<INRegister.released, Equal<boolFalse>, And<INRegister.hold, Equal<boolFalse>>>> INDocumentList;
		public PXAction<INRegister> viewDocument;
		public PXSetup<INSetup> insetup;

		public INDocumentRelease()
		{
			INSetup record = insetup.Current;
			INDocumentList.SetProcessDelegate(
				delegate(List<INRegister> list)
				{
					ReleaseDoc(list, true);
				}
			);
			INDocumentList.SetProcessCaption("Release");
			INDocumentList.SetProcessAllCaption("Release All");
		}

		public static void ReleaseDoc(List<INRegister> list, bool isMassProcess)
		{
			bool failed = false;

			INReleaseProcess rg = PXGraph.CreateInstance<INReleaseProcess>();
			JournalEntry je = PXGraph.CreateInstance<JournalEntry>();
			//Field Verification can fail if GL module is not "Visible";therfore suppress it:
			je.FieldVerifying.AddHandler<GLTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			je.FieldVerifying.AddHandler<GLTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			PostGraph pg = PXGraph.CreateInstance<PostGraph>();
			Dictionary<int,int> batchbind = new Dictionary<int,int>();

			for (int i = 0; i < list.Count; i++)
			{
				INRegister doc = list[i];
				try
				{
					rg.Clear();

					rg.ReleaseDocProc(je, doc);
					int k;
					if ((k = je.created.IndexOf(je.BatchModule.Current)) >= 0 && batchbind.ContainsKey(k) == false)
					{
						batchbind.Add(k, i);
					}

					if (isMassProcess)
					{
						PXProcessing<INRegister>.SetInfo(i, ActionsMessages.RecordProcessed);
					}
				}
				catch (Exception e)
				{
					je.Clear();
					if (isMassProcess)
					{
						PXProcessing<INRegister>.SetError(i, e);
						failed = true;
					}
					else if (list.Count == 1)
					{
						throw new PXMassProcessException(i, e);
					}
					else
					{
						failed = true;
					}
				}
			}

			for (int i = 0; i < je.created.Count; i++)
			{
				Batch batch = je.created[i];
				try
				{
					if (rg.AutoPost)
					{
						pg.Clear();
						pg.PostBatchProc(batch);
					}
				}
				catch (Exception e)
				{
					if (isMassProcess)
					{
						failed = true;
						PXProcessing<INRegister>.SetError(batchbind[i], e);
					}
					else if (list.Count == 1)
					{
						throw new PXMassProcessException(batchbind[i], e);
					}
					else
					{
						failed = true;
					}
				}
			}
			if (failed)
			{
				throw new PXOperationCompletedException(ErrorMessages.SeveralItemsFailed);
			}
		}

		[PXUIField(DisplayName = Messages.ViewDocument)]
		[PXLookupButton]
		protected virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (this.INDocumentList.Current != null)
			{
				INRegister r = PXCache<INRegister>.CreateCopy(this.INDocumentList.Current);

				switch (r.DocType)
				{
					case INDocType.Issue:

						INIssueEntry i_graph = PXGraph.CreateInstance<INIssueEntry>();
						i_graph.issue.Current = r;
						throw new PXRedirectRequiredException(i_graph, true, "IN Issue") { Mode = PXBaseRedirectException.WindowMode.NewWindow };

					case INDocType.Receipt:

						INReceiptEntry r_graph = PXGraph.CreateInstance<INReceiptEntry>();
						r_graph.receipt.Current = r;
						throw new PXRedirectRequiredException(r_graph, true, "IN Receipt") { Mode = PXBaseRedirectException.WindowMode.NewWindow };

					case INDocType.Transfer:

						INTransferEntry t_graph = PXGraph.CreateInstance<INTransferEntry>();
						t_graph.transfer.Current = r;
						throw new PXRedirectRequiredException(t_graph, true, "IN Transfer") { Mode = PXBaseRedirectException.WindowMode.NewWindow };

					case INDocType.Adjustment:

						INAdjustmentEntry a_graph = PXGraph.CreateInstance<INAdjustmentEntry>();
						a_graph.adjustment.Current = r;
						throw new PXRedirectRequiredException(a_graph, true, "IN Transfer") { Mode = PXBaseRedirectException.WindowMode.NewWindow };

					case INDocType.Production:
					case INDocType.Disassembly:

						KitAssemblyEntry k_graph = PXGraph.CreateInstance<KitAssemblyEntry>();
						k_graph.Document.Current = PXSelect<INKitRegister, Where<INKitRegister.docType, Equal<Current<INRegister.docType>>, And<INKitRegister.refNbr, Equal<Current<INRegister.refNbr>>>>>.SelectSingleBound(this, new object[] { r });
						throw new PXRedirectRequiredException(k_graph, true, "IN Kit Assembly") { Mode = PXBaseRedirectException.WindowMode.NewWindow };

					default:
						throw new PXException("Unknown Document Type");
				}
			}
			return adapter.Get();
		}

	}

	public class PXAccumSelect<Table> : PXSelect<Table>
	where Table : class, IBqlTable, new()
	{
		public override Table Insert(Table item)
		{
			Table ret = base.Insert(item);
			if (ret == null)
			{
				return base.Locate(item);
			}
			return ret;
		}

		public PXAccumSelect(PXGraph graph)
			: base(graph)
		{
		}

		public PXAccumSelect(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
		}
	}

	public class PXNoEventsCache<TNode> : PXCache<TNode>
		where TNode : class, IBqlTable, new()
	{
		public PXNoEventsCache(PXGraph graph)
			: base(graph)
		{
			_EventsRowAttr.RowSelecting = null;
			_EventsRowAttr.RowSelected = null;
			_EventsRowAttr.RowInserting = null;
			_EventsRowAttr.RowInserted = null;
			_EventsRowAttr.RowUpdating = null;
			_EventsRowAttr.RowUpdated = null;
			_EventsRowAttr.RowDeleting = null;
			_EventsRowAttr.RowDeleted = null;
			_EventsRowAttr.RowPersisting = null;
			_EventsRowAttr.RowPersisted = null;
		}
	}

	public class INReleaseProcess : PXGraph<INReleaseProcess>
	{
		public PXSelect<INCostSubItemXRef> costsubitemxref;
		public PXSelect<INItemSite> initemsite;

		public PXSelect<OversoldCostStatus> oversoldcoststatus;
		public PXSelect<FIFOCostStatus> fifocoststatus;
		public PXSelect<AverageCostStatus> averagecoststatus;
		public PXSelect<StandardCostStatus> standardcoststatus;
		public PXSelect<SpecificCostStatus> specificcoststatus;
		public PXSelect<ReceiptStatus> receiptstatus;
		public PXSelect<ItemLotSerial> itemlotserial;
		public PXSelect<LotSerialStatus> lotnumberedstatus;
		public PXSelect<LocationStatus> locationstatus;
		public PXSelect<SiteStatus> sitestatus;

		public PXSelect<ItemStats> itemstats;

		public PXSelect<ItemSiteHist> itemsitehist;
        public PXSelect<ItemSiteHistD> itemsitehistd;
		public PXSelect<ItemCostHist> itemcosthist;
		public PXSelect<ItemSalesHist> itemsaleshist;
		public PXSelect<ItemCustSalesHist> itemcustsaleshist;
		public PXSelect<ItemCustSalesStats> itemcustsalesstats;
		public PXSelect<ItemSalesHistD> itemsaleshistd;

		public PXSelect<INRegister> inregister;
		public PXSelect<INTran> intranselect;
		public PXSelect<INTranSplit> intransplit;
		public PXSelect<INItemPlan> initemplan;
		public PXAccumSelect<INTranCost> intrancost;

		public PXSelect<ARTran> artran;

		public PXSetup<INSetup> insetup;
		public PXSetup<Company> companysetup;

		public string BaseCuryID
		{
			get
			{
				return companysetup.Current.BaseCuryID;
			}
		}

		public bool AutoPost
		{
			get
			{
				return (bool)insetup.Current.AutoPost;
			}
		}

		public bool UpdateGL
		{
			get
			{
				return (bool)insetup.Current.UpdateGL;
			}
		}

		public bool SummPost
		{
			get
			{
				return (bool)insetup.Current.SummPost;
			}
		}

		protected ReasonCode _ReceiptReasonCode;
		public ReasonCode ReceiptReasonCode
		{
			get
			{
				if (this._ReceiptReasonCode == null)
				{
					_ReceiptReasonCode = (ReasonCode)PXSelect<ReasonCode, Where<ReasonCode.reasonCodeID, Equal<Current<INSetup.receiptReasonCode>>>>.Select(this);
				}
				return _ReceiptReasonCode;
			}
		}

		protected ReasonCode _IssuesReasonCode;
		public ReasonCode IssuesReasonCode
		{
			get
			{
				if (this._IssuesReasonCode == null)
				{
					_IssuesReasonCode = (ReasonCode)PXSelect<ReasonCode, Where<ReasonCode.reasonCodeID, Equal<Current<INSetup.issuesReasonCode>>>>.Select(this);
				}
				return _IssuesReasonCode;
			}
		}

		protected ReasonCode _AdjustmentReasonCode;
		public ReasonCode AdjustmentReasonCode
		{
			get
			{
				if (this._AdjustmentReasonCode == null)
				{
					_AdjustmentReasonCode = (ReasonCode)PXSelect<ReasonCode, Where<ReasonCode.reasonCodeID, Equal<Current<INSetup.adjustmentReasonCode>>>>.Select(this);
				}
				return _AdjustmentReasonCode;
			}
		}

		public Int32? ARClearingAcctID
		{
			get
			{
				return insetup.Current.ARClearingAcctID;
			}
		}

		public Int32? ARClearingSubID
		{
			get
			{
				return insetup.Current.ARClearingSubID;
			}
		}

		public Int32? INTransitAcctID
		{
			get
			{
				return insetup.Current.INTransitAcctID;
			}
		}

		public Int32? INTransitSubID
		{
			get
			{
				return insetup.Current.INTransitSubID;
			}
		}

		public Int32? INProgressAcctID
		{
			get
			{
				return insetup.Current.INProgressAcctID;
			}
		}

		public Int32? INProgressSubID
		{
			get
			{
				return insetup.Current.INProgressSubID;
			}
		}

		protected PXCache<INTranCost> transfercosts;

		public override void Clear()
		{
			base.Clear();
			if (transfercosts != null)
			{
				transfercosts.Clear();
			}
			WIPCalculated = false;
			WIPVariance = 0m;
		}

		public INReleaseProcess()
		{
			INSetup setup = insetup.Current;

			transfercosts = new PXNoEventsCache<INTranCost>(this);

			PXDBDefaultAttribute.SetDefaultForInsert<INTran.docType>(intranselect.Cache, null, false);
			PXDBDefaultAttribute.SetDefaultForInsert<INTran.refNbr>(intranselect.Cache, null, false);
			PXDBDefaultAttribute.SetDefaultForInsert<INTran.tranDate>(intranselect.Cache, null, false);
			PXDBDefaultAttribute.SetDefaultForInsert<INTran.finPeriodID>(intranselect.Cache, null, false);

			PXDBDefaultAttribute.SetDefaultForInsert<INTranSplit.refNbr>(intransplit.Cache, null, false);
			PXDBDefaultAttribute.SetDefaultForInsert<INTranSplit.tranDate>(intransplit.Cache, null, false);

			PXDBDefaultAttribute.SetDefaultForUpdate<INTran.docType>(intranselect.Cache, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<INTran.refNbr>(intranselect.Cache, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<INTran.tranDate>(intranselect.Cache, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<INTran.finPeriodID>(intranselect.Cache, null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<INTran.tranPeriodID>(intranselect.Cache, null, false);

			PXDBDefaultAttribute.SetDefaultForUpdate<INTranSplit.refNbr>(intransplit.Cache, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<INTranSplit.tranDate>(intransplit.Cache, null, false);

			OpenPeriodAttribute.SetValidatePeriod<INRegister.finPeriodID>(inregister.Cache, null, PeriodValidation.Nothing);

			ParseSubItemSegKeys();

			this.Caches[typeof(InventoryItem)] = new PXCache<InventoryItem>(this);

			PXDimensionSelectorAttribute.SuppressViewCreation(intranselect.Cache);
			PXDimensionSelectorAttribute.SuppressViewCreation(intrancost.Cache);

            PXFormulaAttribute.SetAggregate<INTran.qty>(intranselect.Cache, null); 
            PXFormulaAttribute.SetAggregate<INTran.tranCost>(intranselect.Cache, null);
            PXFormulaAttribute.SetAggregate<INTran.tranAmt>(intranselect.Cache, null);
		}

        protected virtual void StandardCostStatus_UnitCost_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (e.Row != null)
            {
                StandardCostStatus tran = (StandardCostStatus)e.Row;

                INItemSite itemsite = new INItemSite();
                itemsite.InventoryID = tran.InventoryID;
                itemsite.SiteID = tran.CostSiteID;
                itemsite = initemsite.Locate(itemsite);

                if (itemsite == null)
                {
                    itemsite = PXSelectReadonly<INItemSite, Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>, And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>.Select(this, tran.InventoryID, tran.CostSiteID);
                }

                if (itemsite != null)
                {
                    e.NewValue = itemsite.StdCost;
                    e.Cancel = true;
                }
            }
        }

		//all descendants of INCostStatus should have this handler
		long _CostStatus_Identity = long.MinValue;
		protected virtual void StandardCostStatus_CostID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = _CostStatus_Identity++;
			e.Cancel = true;
		}

		protected virtual void AverageCostStatus_CostID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = _CostStatus_Identity++;
			e.Cancel = true;
		}

		protected virtual void FIFOCostStatus_CostID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = _CostStatus_Identity++;
			e.Cancel = true;
		}

		protected virtual void SpecificCostStatus_CostID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = _CostStatus_Identity++;
			e.Cancel = true;
		}

		protected virtual void OversoldCostStatus_CostID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = _CostStatus_Identity++;
			e.Cancel = true;
		}

		[IN.LocationAvail(typeof(INTranSplit.inventoryID), typeof(INTranSplit.subItemID), typeof(INTranSplit.siteID), typeof(INTranSplit.tranType), typeof(INTranSplit.invtMult))]
		public virtual void INTranSplit_LocationID_CacheAttached(PXCache sender)
		{
		}

		protected virtual void INTran_UnitCost_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update && (((INTran)e.Row).InvtMult == (short)0 || ((INTran)e.Row).InvtMult == (short)1 && ((INTran)e.Row).OrigLineNbr == null))
			{
				e.FieldName = string.Empty;
				e.Cancel = true;
			}
		}

		protected virtual void INTran_UnitPrice_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				e.FieldName = string.Empty;
				e.Cancel = true;
			}
		}

		protected virtual void INRegister_TotalQty_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				e.FieldName = string.Empty;
				e.Cancel = true;
			}
		}

		protected virtual void INRegister_TotalAmount_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				e.FieldName = string.Empty;
				e.Cancel = true;
			}
		}

		protected virtual void INRegister_TotalCost_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				e.FieldName = string.Empty;
				e.Cancel = true;
			}
		}

		protected virtual void INRegister_TransferNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		public virtual INSiteStatus UpdateSiteStatus(INTranSplit tran, INLocation whseloc)
		{
			SiteStatus item = new SiteStatus();
			item.InventoryID = tran.InventoryID;
			item.SubItemID = tran.SubItemID;
			item.SiteID = tran.SiteID;

			item = sitestatus.Insert(item);

			item.QtyOnHand += (decimal)tran.InvtMult * (decimal)tran.BaseQty;
			item.QtyAvail += whseloc.InclQtyAvail == true ? (decimal)tran.InvtMult * (decimal)tran.BaseQty : 0m;
			item.QtyHardAvail += whseloc.InclQtyAvail == true ? (decimal)tran.InvtMult * (decimal)tran.BaseQty : 0m;
			item.QtyNotAvail += whseloc.InclQtyAvail == true ? 0m : (decimal)tran.InvtMult * (decimal)tran.BaseQty;

			return item;
		}

        public virtual LocationStatus UpdateLocationStatus(INTranSplit tran)
		{
			INPIStatus pistatus_rec =
				PXSelect<INPIStatus,
					Where<INPIStatus.siteID, Equal<Required<INPIStatus.siteID>>,
						And<INPIStatus.active, Equal<boolTrue>,
					 And2<Where<INPIStatus.inventoryID, IsNull,
									 Or<INPIStatus.inventoryID, Equal<Required<INPIStatus.inventoryID>>>>,
						And<Where<INPIStatus.locationID, IsNull,
									 Or<INPIStatus.locationID, Equal<Required<INPIStatus.locationID>>>>>>>>>
				.Select(this, tran.SiteID, tran.InventoryID, tran.LocationID);

			if ((pistatus_rec != null) && (pistatus_rec.RecordID != null))
			{
                PXCache cache = this.Caches[typeof(INTranSplit)];
                throw new PXException(Messages.PICountInProgressDuringRelease,
                                      PXForeignSelectorAttribute.GetValueExt<INTranSplit.inventoryID>(cache, tran),
                                      PXForeignSelectorAttribute.GetValueExt<INTranSplit.siteID>(cache, tran),
                                      PXForeignSelectorAttribute.GetValueExt<INTranSplit.locationID>(cache, tran));
			}

			LocationStatus item = new LocationStatus();
			item.InventoryID = tran.InventoryID;
			item.SubItemID = tran.SubItemID;
			item.SiteID = tran.SiteID;
			item.LocationID = tran.LocationID;

			item = locationstatus.Insert(item);

			item.NegQty = (tran.TranType == INTranType.Adjustment) ? false : item.NegQty;
			item.QtyOnHand += (decimal)tran.InvtMult * (decimal)tran.BaseQty;
			item.QtyAvail += (decimal)tran.InvtMult * (decimal)tran.BaseQty;
			item.QtyHardAvail += (decimal)tran.InvtMult * (decimal)tran.BaseQty;

			return item;
		}

		public virtual LotSerialStatus AccumulatedLotSerialStatus(INTranSplit split, INLotSerClass lsclass)
		{
			LotSerialStatus ret = new LotSerialStatus();
			ret.InventoryID = split.InventoryID;
			ret.SubItemID = split.SubItemID;
			ret.SiteID = split.SiteID;
			ret.LocationID = split.LocationID;
			ret.LotSerialNbr = split.LotSerialNbr;
			ret = (LotSerialStatus)lotnumberedstatus.Cache.Insert(ret);
            if (ret.ExpireDate == null)
            {
                ret.ExpireDate = split.ExpireDate;
            }
            if (ret.ReceiptDate == null)
            {
                ret.ReceiptDate = split.TranDate;
            }
			ret.LotSerTrack = lsclass.LotSerTrack;

            return ret;
		}


		public virtual bool ReceiveLot(INTran tran, INTranSplit split, InventoryItem item, INLotSerClass lsclass, out LotSerialStatus lsitem)
		{
			if (split.InvtMult == (short)1 && tran.OrigLineNbr == null)
			{
				if (lsclass.LotSerTrack != "N" && lsclass.LotSerAssign == "R")
				{
					lsitem = AccumulatedLotSerialStatus(split, lsclass);

					if (item.ValMethod == "S")
					{
						//for specific Items cost layer can be accurately identified by LotSerialNbr, Account/Sub is not taken into consideration
						INCostStatus layer = AccumulatedCostStatus(tran, split, item);
						lsitem.CostID = layer.CostID;
					}

					lsitem.QtyOnHand += (decimal)split.InvtMult * (decimal)split.BaseQty;
					lsitem.QtyAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
					lsitem.QtyHardAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;

					return true;
				}
			}
			lsitem = null;
			return false;
		}

		public virtual bool IssueLot(INTran tran, INTranSplit split, InventoryItem item, INLotSerClass lsclass, out LotSerialStatus lsitem)
		{
			if (split.InvtMult == -1)
			{
				//for when used serial numbers numbers will mark processed numbers with trandate
				if (INLotSerialNbrAttribute.IsTrackSerial(lsclass, tran.TranType) || lsclass.LotSerTrack != "N" && lsclass.LotSerAssign == "R")
				{
					lsitem = AccumulatedLotSerialStatus(split, lsclass);

					if (lsclass.LotSerAssign == "R")
					{
						if (item.ValMethod == "S")
						{
							//for specific Items cost layer can be accurately identified by LotSerialNbr, Account/Sub is not taken into consideration
							INCostStatus layer = AccumulatedCostStatus(tran, split, item);
							lsitem.CostID = layer.CostID;
						}

						lsitem.QtyOnHand += (decimal)split.InvtMult * (decimal)split.BaseQty;
						lsitem.QtyAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
						lsitem.QtyHardAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
					}

					return true;
				}
			}

			lsitem = null;
			return false;
		}

		public virtual bool TransferLot(INTran tran, INTranSplit split, InventoryItem item, INLotSerClass lsclass, out LotSerialStatus lsitem)
		{
			if (split.InvtMult == (short)1 && tran.OrigLineNbr != null)
			{
				if (lsclass.LotSerTrack != "N" && lsclass.LotSerAssign == "R")
				{
					lsitem = AccumulatedLotSerialStatus(split, lsclass);

					PXResult res = PXSelectJoin<INTranSplit, InnerJoin<ReadOnlyLotSerialStatus, On<ReadOnlyLotSerialStatus.inventoryID, Equal<INTranSplit.inventoryID>, And<ReadOnlyLotSerialStatus.siteID, Equal<INTranSplit.siteID>, And<ReadOnlyLotSerialStatus.locationID, Equal<INTranSplit.locationID>, And<ReadOnlyLotSerialStatus.lotSerialNbr, Equal<INTranSplit.lotSerialNbr>>>>>>, Where<INTranSplit.tranType, Equal<Current<INTran.origTranType>>, And<INTranSplit.refNbr, Equal<Current<INTran.origRefNbr>>, And<INTranSplit.lineNbr, Equal<Current<INTran.origLineNbr>>, And<INTranSplit.lotSerialNbr, Equal<Current<INTranSplit.lotSerialNbr>>>>>>>.SelectSingleBound(this, new object[] { tran, split });

					if (res != null)
					{
						lsitem.ReceiptDate = ((ReadOnlyLotSerialStatus)res[typeof(ReadOnlyLotSerialStatus)]).ReceiptDate;
					}

					if (item.ValMethod == "S")
					{
						//for specific Items cost layer can be accurately identified by LotSerialNbr, Account/Sub is not taken into consideration
						INCostStatus layer = AccumulatedCostStatus(tran, split, item);
						lsitem.CostID = layer.CostID;
					}

					lsitem.QtyOnHand += (decimal)split.InvtMult * (decimal)split.BaseQty;
					lsitem.QtyAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
					lsitem.QtyHardAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;

					return true;
				}
			}

			lsitem = null;
			return false;
		}

		public virtual INItemLotSerial  UpdateItemLotSerial(INTran tran, INTranSplit split, InventoryItem item, INLotSerClass lsclass)
		{
			if (split.InvtMult == 1 &&
					lsclass.LotSerTrack != INLotSerTrack.NotNumbered &&
				  lsclass.LotSerAssign == INLotSerAssign.WhenReceived)
			{
				ItemLotSerial lsitem = AccumulatedItemLotSerial(split, lsclass);

				if (tran.OrigLineNbr == null && lsitem.ExpireDate != null && lsitem.UpdateExpireDate == null)
					lsitem.UpdateExpireDate = true;

				if (split.TranType == INTranType.Adjustment && split.InvtMult == 1 && split.ExpireDate != null)
				{
					lsitem.UpdateExpireDate = true;
					lsitem.ExpireDate = split.ExpireDate;
				}

				lsitem.QtyOnHand += (decimal)split.InvtMult * (decimal)split.BaseQty;
				lsitem.QtyAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
				lsitem.QtyHardAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;

				return lsitem;
			}

			if (split.InvtMult == -1 &&
					split.BaseQty != 0m &&
					!string.IsNullOrEmpty(split.LotSerialNbr) &&
					lsclass.LotSerTrack != INLotSerTrack.NotNumbered)
			{
				ItemLotSerial lsitem = AccumulatedItemLotSerial(split, lsclass);

				if (lsitem.ExpireDate != null && lsitem.UpdateExpireDate == null)
					lsitem.UpdateExpireDate = lsclass.LotSerAssign == INLotSerAssign.WhenUsed;

				if (lsclass.LotSerTrack == INLotSerTrack.SerialNumbered ||
						lsclass.LotSerAssign == INLotSerAssign.WhenReceived)
				{
					lsitem.QtyOnHand += (decimal)split.InvtMult * (decimal)split.BaseQty;
					lsitem.QtyAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
					lsitem.QtyHardAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
				}
				return lsitem;
			}

			return null;
		}

		public virtual INLotSerialStatus UpdateLotSerialStatus(INTran tran, INTranSplit split, InventoryItem item, INLotSerClass lsclass)
		{
			LotSerialStatus lsitem;

			if (ReceiveLot(tran, split, item, lsclass, out lsitem) || 
				IssueLot(tran, split, item, lsclass, out lsitem) || 	
				TransferLot(tran, split, item, lsclass, out lsitem))
			{
				return lsitem;
			}
			return null;
		}

		public virtual ItemLotSerial AccumulatedItemLotSerial(INTranSplit split, INLotSerClass lsclass)
		{
			ItemLotSerial lsitem = new ItemLotSerial();
			lsitem.InventoryID = split.InventoryID;
			lsitem.LotSerialNbr = split.LotSerialNbr;
            lsitem = (ItemLotSerial)itemlotserial.Insert(lsitem);
            if (lsitem.ExpireDate == null)
            {
                lsitem.ExpireDate = split.ExpireDate;
            }
            lsitem.LotSerTrack = lsclass.LotSerTrack; 

			return lsitem;
		}

		public virtual INCostStatus AccumulatedCostStatus(INTran tran, INTranSplit split, InventoryItem item)
		{
			INCostStatus layer = null;

			switch (item.ValMethod)
			{
				case INValMethod.Standard:

					if (tran.TranType == INTranType.NegativeCostAdjustment)
					{
						return AccumOversoldCostStatus(tran, split, item);
					}
					else
					{
						layer = new StandardCostStatus();
						layer.AccountID = tran.InvtAcctID;
						layer.SubID = tran.InvtSubID;
						layer.InventoryID = tran.InventoryID;
						layer.CostSiteID = split.CostSiteID;
						layer.CostSubItemID = split.CostSubItemID;
						layer.LayerType = INLayerType.Normal;

						return (StandardCostStatus)standardcoststatus.Cache.Insert(layer);
					}
				case INValMethod.Average:
					layer = new AverageCostStatus();
					layer.AccountID = tran.InvtAcctID;
					layer.SubID = tran.InvtSubID;
					layer.InventoryID = tran.InventoryID;
					layer.CostSiteID = split.CostSiteID;
					layer.CostSubItemID = split.CostSubItemID;
					layer.LayerType = INLayerType.Normal;

					return (AverageCostStatus)averagecoststatus.Cache.Insert(layer);
				case INValMethod.FIFO:
					layer = new FIFOCostStatus();
					layer.AccountID = tran.InvtAcctID;
					layer.SubID = tran.InvtSubID;
					layer.InventoryID = tran.InventoryID;
					layer.CostSiteID = split.CostSiteID;
					layer.CostSubItemID = split.CostSubItemID;
					layer.ReceiptDate = tran.TranDate;
					layer.ReceiptNbr = tran.OrigRefNbr ?? tran.RefNbr;
					layer.LayerType = INLayerType.Normal;

					return (FIFOCostStatus)fifocoststatus.Cache.Insert(layer);
				case INValMethod.Specific:
					layer = new SpecificCostStatus();
					layer.AccountID = tran.InvtAcctID;
					layer.SubID = tran.InvtSubID;
					layer.InventoryID = tran.InventoryID;
					layer.CostSiteID = split.CostSiteID;
					layer.CostSubItemID = split.CostSubItemID;
					layer.LotSerialNbr = split.LotSerialNbr;
					layer.ReceiptDate = tran.TranDate;
					layer.ReceiptNbr = tran.RefNbr;
					layer.LayerType = INLayerType.Normal;

					if (tran.InvtMult == 0 && (tran.TranType == INTranType.Invoice || tran.TranType == INTranType.DebitMemo || tran.TranType == INTranType.CreditMemo))
					{
						layer.LotSerialNbr = string.Empty;
					}

					return (SpecificCostStatus)specificcoststatus.Cache.Insert(layer);
				default:
					throw new PXException();
			}
		}

		public virtual INCostStatus AccumOversoldCostStatus(INTran tran, INTranSplit split, InventoryItem item)
		{
			INCostStatus layer = null;

			if (item.NegQty == false)
			{
				INSite warehouse = PXSelect<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>.Select(this, tran.SiteID);
				INLocation location = PXSelect<INLocation, Where<INLocation.siteID, Equal<Required<INLocation.siteID>>, And<INLocation.locationID, Equal<Required<INLocation.locationID>>>>>.Select(this, tran.SiteID, tran.LocationID);

				string siteCD = "";
				string locationCD = "";

				if (warehouse != null)
					siteCD = warehouse.SiteCD;

				if (location != null)
					locationCD = location.LocationCD;

				throw new PXException(Messages.Inventory_Negative, item.InventoryCD, siteCD, locationCD);
			}

			switch (item.ValMethod)
			{
				case INValMethod.Standard:
				case INValMethod.Average:
				case INValMethod.FIFO:
					layer = new OversoldCostStatus();
					layer.AccountID = tran.InvtAcctID;
					layer.SubID = tran.InvtSubID;
					layer.InventoryID = tran.InventoryID;
					layer.CostSiteID = split.CostSiteID;
					layer.CostSubItemID = split.CostSubItemID;
					layer.ReceiptDate = new DateTime(1900, 1, 1);
					layer.ReceiptNbr = "OVERSOLD";
					layer.LayerType = INLayerType.Oversold;
					layer.ValMethod = item.ValMethod;

					return (OversoldCostStatus)oversoldcoststatus.Cache.Insert(layer);
				case INValMethod.Specific:
					throw new PXException(Messages.Inventory_Negative);
				default:
					throw new PXException();
			}
		}

		public virtual INCostStatus AccumOversoldCostStatus(INCostStatus layer)
		{
			INCostStatus ret = new OversoldCostStatus();

			PXCache<INCostStatus>.RestoreCopy(ret, layer);
			ret.QtyOnHand = 0m;
			ret.TotalCost = 0m;

			return (OversoldCostStatus)oversoldcoststatus.Cache.Insert(ret);
		}

        public virtual PXView GetCostStatusCommand(InventoryItem item)
        {
            PXView cmd = null;
            switch (item.ValMethod)
            {
                case INValMethod.Average:
                case INValMethod.Standard:
                case INValMethod.FIFO:
                    cmd = this.TypedViews.GetView(new Select<ReadOnlyCostStatus, Where<ReadOnlyCostStatus.inventoryID, Equal<Required<ReadOnlyCostStatus.inventoryID>>, And<ReadOnlyCostStatus.costSiteID, Equal<Required<ReadOnlyCostStatus.costSiteID>>, And<ReadOnlyCostStatus.costSubItemID, Equal<Required<ReadOnlyCostStatus.costSubItemID>>>>>, OrderBy<Asc<ReadOnlyCostStatus.receiptDate, Asc<ReadOnlyCostStatus.receiptNbr>>>>(), false);
                    break;
                case INValMethod.Specific:
                    cmd = this.TypedViews.GetView(new Select<ReadOnlyCostStatus, Where<ReadOnlyCostStatus.inventoryID, Equal<Required<ReadOnlyCostStatus.inventoryID>>, And<ReadOnlyCostStatus.costSiteID, Equal<Required<ReadOnlyCostStatus.costSiteID>>, And<ReadOnlyCostStatus.costSubItemID, Equal<Required<ReadOnlyCostStatus.costSubItemID>>, And<ReadOnlyCostStatus.lotSerialNbr, Equal<Required<ReadOnlyCostStatus.lotSerialNbr>>>>>>>(), false);
                    break;
                default:
                    throw new PXException();
            }
            return cmd;
        }

		public virtual void ReceiveCost(INTran tran, INTranSplit split, InventoryItem item)
		{
            if (tran.InvtMult == (short)1 && tran.OrigLineNbr == null && (split.InvtMult == (short)1 || item.ValMethod != INValMethod.Standard) || tran.InvtMult == (short)0)
			{
				//!!!SPECIFIC, add Account Sub population from existing layer.
				INCostStatus layer = AccumulatedCostStatus(tran, split, item);

				INTranCost costtran = new INTranCost();
				costtran.InvtAcctID = layer.AccountID;
				costtran.InvtSubID = layer.SubID;
				costtran.COGSAcctID = tran.COGSAcctID;
				costtran.COGSSubID = tran.COGSSubID;
				costtran.CostID = layer.CostID;
				costtran.InventoryID = layer.InventoryID;
				costtran.CostSiteID = layer.CostSiteID;
				costtran.CostSubItemID = layer.CostSubItemID;
				costtran.TranType = tran.TranType;
				costtran.RefNbr = tran.RefNbr;
				costtran.LineNbr = tran.LineNbr;
				costtran.CostDocType = tran.DocType;
				costtran.CostRefNbr = tran.RefNbr;

				//for negative adjustments line InvtMult == 1 split/cost InvtMult == -1
				costtran.InvtMult = split.InvtMult;

				costtran.FinPeriodID = tran.FinPeriodID;
				costtran.TranPeriodID = tran.TranPeriodID;
				costtran.TranDate = tran.TranDate;
				costtran.TranAmt = 0m;

                PXParentAttribute.SetParent(intrancost.Cache, costtran, typeof(INTran), tran);
				INTranCost prev_tran = intrancost.Insert(costtran);
				costtran = PXCache<INTranCost>.CreateCopy(prev_tran);

				costtran.Qty += INUnitAttribute.ConvertToBase<INTran.inventoryID>(intranselect.Cache, tran, split.UOM, (decimal)split.Qty, INPrecision.QUANTITY);

				//cost only adjustment
				if (split.BaseQty == 0m && tran.BaseQty == 0m)
				{
					costtran.TranCost += tran.TranCost;
				}
				else if (item.ValMethod == INValMethod.Standard)
				{
					//do not add cost, recalculate
					costtran.TranCost = PXCurrencyAttribute.BaseRound(this, (decimal)layer.UnitCost * (decimal)costtran.Qty);
				}
				else
				{
					costtran.TranCost += PXCurrencyAttribute.BaseRound(this, (decimal)tran.UnitCost * (decimal)split.BaseQty);
				}

				costtran.TranAmt += PXCurrencyAttribute.BaseRound(this, (decimal)split.BaseQty * (decimal)tran.UnitPrice);

                INCostStatus unmodifiedLayer = PXCache<INCostStatus>.CreateCopy(layer);

                layer.QtyOnHand += (decimal)costtran.InvtMult * (costtran.Qty - prev_tran.Qty);
                layer.TotalCost += (decimal)costtran.InvtMult * (costtran.TranCost - prev_tran.TranCost);

                PXView cmd = GetCostStatusCommand(item);
                foreach (INCostStatus costStatus in cmd.SelectMulti(split.InventoryID, split.CostSiteID, split.CostSubItemID, split.LotSerialNbr))
                {
                    if (item.ValMethod == INValMethod.Specific ||
                        ((item.ValMethod == INValMethod.Average || item.ValMethod == INValMethod.FIFO) && costStatus.ReceiptNbr == layer.ReceiptNbr))
                    {
                        INCostStatus unmodifiedCostStatus = PXCache<INCostStatus>.CreateCopy(costStatus);
                        costStatus.QtyOnHand += (decimal)costtran.InvtMult * (costtran.Qty - prev_tran.Qty);
                        costStatus.TotalCost += (decimal)costtran.InvtMult * (costtran.TranCost - prev_tran.TranCost);
                        cmd.Cache.SetStatus(costStatus, PXEntryStatus.Held);
                        //throw exception if not cost only adjustment and quantity to cost imbalance detected
                        if (split.BaseQty != 0m && tran.BaseQty != 0m && costStatus.QtyOnHand == 0m && costStatus.TotalCost != 0m)
                        {
                            PXCache<INCostStatus>.RestoreCopy(layer, unmodifiedLayer);
                            PXCache<INCostStatus>.RestoreCopy(costStatus, unmodifiedCostStatus);
                            intrancost.Delete(costtran);
                            throw new PXQtyCostImbalanceException();
                        }
                    }
                }

				if (split.BaseQty == 0m && tran.BaseQty == 0m)
				{
					//avoid PXFormula
					PXCache<INTranCost>.RestoreCopy(prev_tran, costtran);
				}
				else
				{
					decimal diff;
					if (item.ValMethod != INValMethod.Standard && (diff = PXCurrencyAttribute.BaseRound(this, (tran.CostedQty + costtran.Qty - prev_tran.Qty) * tran.UnitCost - tran.TranCost - costtran.TranCost + prev_tran.TranCost)) != 0m)
					{
						costtran.TranCost += diff;
						layer.TotalCost += (decimal)costtran.InvtMult * diff;
					}

					//update after, otherwise objects.Equal(costtran, prev_tran)
					costtran = intrancost.Update(costtran);
				}

				//write-off cost remainder
				if (tran.BaseQty != 0m && tran.BaseQty == tran.CostedQty)
				{
					if (item.ValMethod == INValMethod.Standard)
					{
						costtran.VarCost += (tran.OrigTranCost - tran.TranCost);
						tran.TranCost = tran.OrigTranCost;
					}
					else
					{
						costtran.TranCost += (tran.OrigTranCost - tran.TranCost);
						layer.TotalCost += (tran.OrigTranCost - tran.TranCost);
						tran.TranCost = tran.OrigTranCost;
					}
				}
				//negative cost adjustment 1:1 INTran:INTranSplit
				else if (tran.BaseQty != 0m && tran.BaseQty == -tran.CostedQty)
				{
					if (item.ValMethod == INValMethod.Standard)
					{
						costtran.VarCost += (-1m * tran.OrigTranCost - tran.TranCost);
						tran.TranCost = tran.OrigTranCost;
					}
					else
					{
						costtran.TranCost += (-1m * tran.OrigTranCost - tran.TranCost);
						layer.TotalCost += (-1m * tran.OrigTranCost - tran.TranCost);
						tran.TranCost = tran.OrigTranCost;
					}
				}

				//write-off price remainder
				if (tran.BaseQty != 0m && (tran.BaseQty == tran.CostedQty || tran.BaseQty == -tran.CostedQty))
				{
					costtran.TranAmt += (tran.OrigTranAmt - tran.TranAmt);
					tran.TranAmt = tran.OrigTranAmt;
				}
			}
		}


		public class PXSelectOversold<InventoryID, CostSubItemID, CostSiteID> : PXSelectJoinGroupBy<INTranCost, InnerJoin<ReadOnlyCostStatus, On<ReadOnlyCostStatus.costID, Equal<INTranCost.costID>>>, Where<ReadOnlyCostStatus.layerType, Equal<INLayerType.oversold>, And<ReadOnlyCostStatus.inventoryID, Equal<Current<InventoryID>>, And<ReadOnlyCostStatus.costSubItemID, Equal<Current<CostSubItemID>>, And<ReadOnlyCostStatus.costSiteID, Equal<Current<CostSiteID>>, And<ReadOnlyCostStatus.qtyOnHand, Less<decimal0>>>>>>, Aggregate<GroupBy<INTranCost.tranType, GroupBy<INTranCost.refNbr, GroupBy<INTranCost.lineNbr, GroupBy<INTranCost.costID, Sum<INTranCost.qty, Sum<INTranCost.tranCost>>>>>>>>
			where InventoryID : IBqlField
			where CostSubItemID : IBqlField
			where CostSiteID : IBqlField
		{
			public PXSelectOversold(PXGraph graph)
				: base(graph)
			{
			}
		}

		public void ReceiveOverSold<TLayer, InventoryID, CostSubItemID, CostSiteID>(INRegister doc)
			where TLayer : INCostStatus
			where InventoryID : IBqlField
			where CostSubItemID : IBqlField
			where CostSiteID : IBqlField
		{
			foreach (TLayer accumlayer in this.Caches[typeof(TLayer)].Inserted)
			{
				if (accumlayer.QtyOnHand > 0m)
				{
					foreach (PXResult<INTranCost, ReadOnlyCostStatus> res in PXSelectOversold<InventoryID, CostSubItemID, CostSiteID>.SelectMultiBound(this, new object[] { accumlayer }))
					{
						INTranCost costtran = (INTranCost)res;

						foreach (INTranCost item in intrancost.Cache.Inserted)
						{
							if (string.Equals(item.TranType, costtran.TranType) && string.Equals(item.RefNbr, costtran.RefNbr) && object.Equals(item.LineNbr, costtran.LineNbr) && object.Equals(item.CostID, costtran.CostID))
							{
								if (object.ReferenceEquals(costtran, (INTranCost)res))
								{
									costtran = PXCache<INTranCost>.CreateCopy(costtran);
								}
								costtran.Qty += item.Qty;
								costtran.TranCost += item.TranCost;
							}
						}

						if (costtran.Qty <= 0m)
						{
							continue;
						}

						INCostStatus oversoldlayer = AccumOversoldCostStatus((ReadOnlyCostStatus)res);

						if (accumlayer.QtyOnHand != 0m)
						{
							accumlayer.AvgCost = accumlayer.TotalCost / accumlayer.QtyOnHand;
						}
						if ((((ReadOnlyCostStatus)res).QtyOnHand + oversoldlayer.QtyOnHand) != 0m)
						{
							oversoldlayer.AvgCost = (((ReadOnlyCostStatus)res).TotalCost + oversoldlayer.TotalCost) / (((ReadOnlyCostStatus)res).QtyOnHand + oversoldlayer.QtyOnHand);
						}

						if (costtran.Qty <= accumlayer.QtyOnHand)
						{
							{
								//reverse original cost
								INTranCost newtran = PXCache<INTranCost>.CreateCopy(costtran);

								newtran.TranDate = doc.TranDate;
								newtran.TranPeriodID = doc.TranPeriodID;
								newtran.FinPeriodID = doc.FinPeriodID;
								newtran.CostDocType = doc.DocType;
								newtran.CostRefNbr = doc.RefNbr;
								newtran.TranAmt = 0m;
								newtran.Qty = 0m;
								newtran.TranCost = 0m;
								newtran.VarCost = 0m;

								//count for multiply layers adjusting single oversold transactions
								INTranCost prev_tran = intrancost.Insert(newtran);
								newtran = PXCache<INTranCost>.CreateCopy(prev_tran);

								newtran.Qty -= costtran.Qty;
								if (oversoldlayer.ValMethod == INValMethod.Standard)
								{
									newtran.TranCost = PXCurrencyAttribute.BaseRound(this, newtran.Qty * oversoldlayer.AvgCost);
									newtran.VarCost = -PXCurrencyAttribute.BaseRound(this, newtran.Qty * oversoldlayer.AvgCost) + PXCurrencyAttribute.BaseRound(this, newtran.Qty * oversoldlayer.UnitCost);
								}
								else
								{
									newtran.TranCost -= costtran.TranCost;
								}

								oversoldlayer.TotalCost += -newtran.TranCost + prev_tran.TranCost;
								oversoldlayer.QtyOnHand += -newtran.Qty + prev_tran.Qty;

								intrancost.Update(newtran);
							}
							{
								INTranCost newtran = PXCache<INTranCost>.CreateCopy(costtran);
								newtran.CostID = accumlayer.CostID;
								newtran.InvtAcctID = accumlayer.AccountID;
								newtran.InvtSubID = accumlayer.SubID;
								newtran.TranDate = doc.TranDate;
								newtran.TranPeriodID = doc.TranPeriodID;
								newtran.FinPeriodID = doc.FinPeriodID;
								newtran.Qty = costtran.Qty;
								newtran.TranCost = PXCurrencyAttribute.BaseRound(this, newtran.Qty * accumlayer.AvgCost);
								if (accumlayer.ValMethod == INValMethod.Standard)
								{
									newtran.VarCost = -PXCurrencyAttribute.BaseRound(this, newtran.Qty * accumlayer.AvgCost) + PXCurrencyAttribute.BaseRound(this, newtran.Qty * accumlayer.UnitCost);
								}
								newtran.TranAmt = 0m;
								newtran.CostDocType = doc.DocType;
								newtran.CostRefNbr = doc.RefNbr;

								intrancost.Cache.Insert(newtran);

								accumlayer.TotalCost -= PXCurrencyAttribute.BaseRound(this, costtran.Qty * accumlayer.AvgCost);
								accumlayer.QtyOnHand -= costtran.Qty;
							}
						}
						else if (accumlayer.QtyOnHand > 0m)
						{
							{
								//reverse original cost
								INTranCost newtran = PXCache<INTranCost>.CreateCopy(costtran);
								newtran.TranDate = doc.TranDate;
								newtran.TranPeriodID = doc.TranPeriodID;
								newtran.FinPeriodID = doc.FinPeriodID;
								newtran.CostDocType = doc.DocType;
								newtran.CostRefNbr = doc.RefNbr;
								newtran.TranAmt = 0m;
								newtran.Qty = 0m;
								newtran.TranCost = 0m;
								newtran.VarCost = 0m;

								//count for multiply layers adjusting single oversold transactions
								INTranCost prev_tran = intrancost.Insert(newtran);
								newtran = PXCache<INTranCost>.CreateCopy(prev_tran);

								newtran.Qty -= accumlayer.QtyOnHand;
								if (oversoldlayer.ValMethod == INValMethod.Standard)
								{
									newtran.TranCost = PXCurrencyAttribute.BaseRound(this, newtran.Qty * oversoldlayer.AvgCost);
									newtran.VarCost = -PXCurrencyAttribute.BaseRound(this, newtran.Qty * oversoldlayer.UnitCost) + PXCurrencyAttribute.BaseRound(this, newtran.Qty * oversoldlayer.AvgCost);
								}
								else
								{
									newtran.TranCost += PXCurrencyAttribute.BaseRound(this, newtran.Qty * costtran.TranCost / costtran.Qty);
								}

								oversoldlayer.TotalCost += -newtran.TranCost + prev_tran.TranCost;
								oversoldlayer.QtyOnHand += -newtran.Qty + prev_tran.Qty;

								intrancost.Update(newtran);
							}
							{
								INTranCost newtran = PXCache<INTranCost>.CreateCopy(costtran);
								newtran.CostID = accumlayer.CostID;
								newtran.InvtAcctID = accumlayer.AccountID;
								newtran.InvtSubID = accumlayer.SubID;
								newtran.TranDate = doc.TranDate;
								newtran.TranPeriodID = doc.TranPeriodID;
								newtran.FinPeriodID = doc.FinPeriodID;
								newtran.Qty = accumlayer.QtyOnHand;
								newtran.TranCost = accumlayer.TotalCost;
								if (accumlayer.ValMethod == INValMethod.Standard)
								{
									newtran.VarCost = -accumlayer.TotalCost + PXCurrencyAttribute.BaseRound(this, newtran.Qty * accumlayer.UnitCost);
								}
								newtran.TranAmt = 0m;
								newtran.CostDocType = doc.DocType;
								newtran.CostRefNbr = doc.RefNbr;

								intrancost.Cache.Insert(newtran);

								accumlayer.TotalCost = 0m;
								accumlayer.QtyOnHand = 0m;
							}
						}
					}
				}
			}
		}

		public virtual void ReceiveOversold(INRegister doc)
		{
			ReceiveOverSold<FIFOCostStatus, FIFOCostStatus.inventoryID, FIFOCostStatus.costSubItemID, FIFOCostStatus.costSiteID>(doc);
			ReceiveOverSold<AverageCostStatus, AverageCostStatus.inventoryID, AverageCostStatus.costSubItemID, AverageCostStatus.costSiteID>(doc);
			ReceiveOverSold<StandardCostStatus, StandardCostStatus.inventoryID, StandardCostStatus.costSubItemID, StandardCostStatus.costSiteID>(doc);
		}

		public virtual INCostStatus AccumulatedCostStatus(INCostStatus layer, InventoryItem item)
		{
			INCostStatus ret = null;

			if (layer.LayerType == INLayerType.Oversold)
			{
				ret = new OversoldCostStatus();
				ret.AccountID = layer.AccountID;
				ret.SubID = layer.SubID;
				ret.InventoryID = layer.InventoryID;
				ret.CostSiteID = layer.CostSiteID;
				ret.CostSubItemID = layer.CostSubItemID;
				ret.ReceiptDate = layer.ReceiptDate;
				ret.ReceiptNbr = layer.ReceiptNbr;
				ret.LayerType = layer.LayerType;

				return (OversoldCostStatus)oversoldcoststatus.Cache.Insert(ret);
			}

			switch (item.ValMethod)
			{
				case INValMethod.Average:
					ret = new AverageCostStatus();
					ret.AccountID = layer.AccountID;
					ret.SubID = layer.SubID;
					ret.InventoryID = layer.InventoryID;
					ret.CostSiteID = layer.CostSiteID;
					ret.CostSubItemID = layer.CostSubItemID;
					ret.LayerType = layer.LayerType;

					return (AverageCostStatus)averagecoststatus.Cache.Insert(ret);
				case INValMethod.Standard:
					ret = new StandardCostStatus();
					ret.AccountID = layer.AccountID;
					ret.SubID = layer.SubID;
					ret.InventoryID = layer.InventoryID;
					ret.CostSiteID = layer.CostSiteID;
					ret.CostSubItemID = layer.CostSubItemID;
					ret.LayerType = layer.LayerType;

					return (StandardCostStatus)standardcoststatus.Cache.Insert(ret);
				case INValMethod.FIFO:
					ret = new FIFOCostStatus();
					ret.AccountID = layer.AccountID;
					ret.SubID = layer.SubID;
					ret.InventoryID = layer.InventoryID;
					ret.CostSiteID = layer.CostSiteID;
					ret.CostSubItemID = layer.CostSubItemID;
					ret.ReceiptDate = layer.ReceiptDate;
					ret.ReceiptNbr = layer.ReceiptNbr;
					ret.LayerType = layer.LayerType;

					return (FIFOCostStatus)fifocoststatus.Cache.Insert(ret);
				case INValMethod.Specific:
					ret = new SpecificCostStatus();
					ret.AccountID = layer.AccountID;
					ret.SubID = layer.SubID;
					ret.InventoryID = layer.InventoryID;
					ret.CostSiteID = layer.CostSiteID;
					ret.CostSubItemID = layer.CostSubItemID;
					ret.LotSerialNbr = layer.LotSerialNbr;
					ret.ReceiptDate = layer.ReceiptDate;
					ret.ReceiptNbr = layer.ReceiptNbr;
					ret.LayerType = layer.LayerType;

					return (SpecificCostStatus)specificcoststatus.Cache.Insert(ret);
				default:
					throw new PXException();
			}
		}

		public virtual INCostStatus AccumulatedCostStatus(INCostStatus layer, INTran tran, INTranSplit split, InventoryItem item)
		{
			INCostStatus ret = null;
			switch (item.ValMethod)
			{
				case INValMethod.Average:
					ret = new AverageCostStatus();
					ret.AccountID = tran.InvtAcctID;
					ret.SubID = tran.InvtSubID;
					ret.InventoryID = layer.InventoryID;
					ret.CostSiteID = split.CostSiteID;
					ret.CostSubItemID = layer.CostSubItemID;
					ret.LayerType = layer.LayerType;

					return (AverageCostStatus)averagecoststatus.Cache.Insert(ret);
				case INValMethod.Standard:
					ret = new StandardCostStatus();
					ret.AccountID = tran.InvtAcctID;
					ret.SubID = tran.InvtSubID;
					ret.InventoryID = layer.InventoryID;
					ret.CostSiteID = split.CostSiteID;
					ret.CostSubItemID = layer.CostSubItemID;
					ret.LayerType = layer.LayerType;

					return (StandardCostStatus)standardcoststatus.Cache.Insert(ret);
				case INValMethod.FIFO:
					ret = new FIFOCostStatus();
					ret.AccountID = tran.InvtAcctID;
					ret.SubID = tran.InvtSubID;
					ret.InventoryID = layer.InventoryID;
					ret.CostSiteID = split.CostSiteID;
					ret.CostSubItemID = layer.CostSubItemID;
					ret.ReceiptDate = layer.ReceiptDate;
					ret.ReceiptNbr = layer.ReceiptNbr;
					ret.LayerType = layer.LayerType;

					return (FIFOCostStatus)fifocoststatus.Cache.Insert(ret);
				case INValMethod.Specific:
					ret = new SpecificCostStatus();
					ret.AccountID = tran.InvtAcctID;
					ret.SubID = tran.InvtSubID;
					ret.InventoryID = layer.InventoryID;
					ret.CostSiteID = split.CostSiteID;
					ret.CostSubItemID = layer.CostSubItemID;
					ret.LotSerialNbr = layer.LotSerialNbr;
					ret.ReceiptDate = layer.ReceiptDate;
					ret.ReceiptNbr = layer.ReceiptNbr;
					ret.LayerType = layer.LayerType;

					return (SpecificCostStatus)specificcoststatus.Cache.Insert(ret);
				default:
					throw new PXException();
			}
		}

		public virtual void IssueCost(INCostStatus layer, INTran tran, INTranSplit split, InventoryItem item, ref decimal QtyUnCosted)
		{
			INTranCost costtran = new INTranCost();
			costtran.InvtAcctID = layer.AccountID;
			costtran.InvtSubID = layer.SubID;
			costtran.COGSAcctID = tran.COGSAcctID;
			costtran.COGSSubID = tran.COGSSubID;
			costtran.CostID = layer.CostID;
			costtran.InventoryID = layer.InventoryID;
			costtran.CostSiteID = layer.CostSiteID;
			costtran.CostSubItemID = layer.CostSubItemID;
			costtran.TranType = tran.TranType;
			costtran.RefNbr = tran.RefNbr;
			costtran.LineNbr = tran.LineNbr;
			costtran.CostDocType = tran.DocType;
			costtran.CostRefNbr = tran.RefNbr;
			//for negative adjustments line InvtMult == 1 split/cost InvtMult == -1
			costtran.InvtMult = split.InvtMult;
			costtran.FinPeriodID = tran.FinPeriodID;
			costtran.TranPeriodID = tran.TranPeriodID;
			costtran.TranDate = tran.TranDate;
			costtran.TranAmt = 0m;

            PXParentAttribute.SetParent(intrancost.Cache, costtran, typeof(INTran), tran);
            costtran = PXCache<INTranCost>.CreateCopy(intrancost.Insert(costtran));

			INCostStatus accumlayer = AccumulatedCostStatus(layer, item);
			accumlayer.QtyOnHand += costtran.Qty;
			accumlayer.TotalCost += costtran.TranCost;

			if (layer.QtyOnHand <= QtyUnCosted)
			{
				QtyUnCosted -= (decimal)layer.QtyOnHand;
				costtran.TranAmt += PXCurrencyAttribute.BaseRound(this, (decimal)layer.QtyOnHand * (decimal)tran.UnitPrice);
				costtran.Qty += layer.QtyOnHand;
				costtran.TranCost += layer.TotalCost;
				if (accumlayer.ValMethod == INValMethod.Standard)
				{
					costtran.VarCost += PXDBCurrencyAttribute.BaseRound(this, (decimal)layer.QtyOnHand * (decimal)layer.UnitCost) - layer.TotalCost;
				}
				layer.QtyOnHand = 0m;
				layer.TotalCost = 0m;
			}
			else
			{
				costtran.TranAmt += PXCurrencyAttribute.BaseRound(this, (decimal)QtyUnCosted * (decimal)tran.UnitPrice);
				if (PXCurrencyAttribute.IsNullOrEmpty(layer.UnitCost))
				{
					layer.UnitCost = (decimal)layer.TotalCost / (decimal)layer.QtyOnHand;
				}

				layer.QtyOnHand -= QtyUnCosted;
				layer.TotalCost += costtran.TranCost;
				layer.TotalCost -= PXCurrencyAttribute.BaseRound(this, (costtran.Qty + QtyUnCosted) * (decimal)layer.UnitCost);

				costtran.Qty += QtyUnCosted;
				costtran.TranCost = PXCurrencyAttribute.BaseRound(this, costtran.Qty * (decimal)layer.UnitCost);

				QtyUnCosted = 0m;
			}

			accumlayer.QtyOnHand -= costtran.Qty;
			accumlayer.TotalCost -= costtran.TranCost;

			//Accumulate cost issued via PXFormula for Issues only
			costtran = intrancost.Update(costtran);

			//negative cost adjustment 1:1 INTran:INTranSplit
			if (tran.InvtMult == 1m && tran.BaseQty == -tran.CostedQty)
			{
                if (item.ValMethod != INValMethod.Specific)
				{
					//reset variance to difference
					costtran.VarCost = (-1m * tran.OrigTranCost - tran.TranCost);
					tran.TranCost = tran.OrigTranCost;
				}
			}

			//write-off price remainder
			if (tran.BaseQty != 0m && (tran.BaseQty == tran.CostedQty || tran.BaseQty == -tran.CostedQty))
			{
				costtran.TranAmt += (tran.OrigTranAmt - tran.TranAmt);
				tran.TranAmt = tran.OrigTranAmt;
			}

		}

        public virtual void IssueCost(INTran tran, INTranSplit split, InventoryItem item, bool correctImbalance)
		{
			//costing is done in parallel, i.e. if 2 splits are all the same as transaction, one accumulated cost tran(cost&qty summarized) will be added, if varied in cost key fields, then variance
			//will be via LayerID which will be different

            if (tran.InvtMult == (short)-1 || tran.InvtMult == (short)1 && split.InvtMult == (short)-1 && (item.ValMethod == INValMethod.Standard || (item.ValMethod != INValMethod.Standard && correctImbalance)))
			{
                PXView cmd = GetCostStatusCommand(item);

				if (cmd != null)
				{
					decimal QtyUnCosted = INUnitAttribute.ConvertToBase<INTran.inventoryID>(intranselect.Cache, tran, split.UOM, (decimal)split.Qty, INPrecision.QUANTITY);
					foreach (INCostStatus layer in cmd.SelectMulti(split.InventoryID, split.CostSiteID, split.CostSubItemID, split.LotSerialNbr))
					{
						if (layer.QtyOnHand > 0m)
						{
							IssueCost(layer, tran, split, item, ref QtyUnCosted);

							cmd.Cache.SetStatus(layer, PXEntryStatus.Held);

							if (QtyUnCosted == 0m)
							{
								break;
							}
						}
					}

					//negative cost adjustment
					if (tran.InvtMult == (short)1 && QtyUnCosted > 0m)
					{
						object costSubItemID = split.CostSubItemID;
						object costSiteID = split.CostSiteID;
						intranselect.Cache.RaiseFieldSelecting<INTran.subItemID>(tran, ref costSubItemID, true);
						intranselect.Cache.RaiseFieldSelecting<INTran.siteID>(tran, ref costSiteID, true);

						throw new PXException(Messages.StatusCheck_QtyNegative, intranselect.Cache.GetValueExt<INTran.inventoryID>(tran), costSubItemID, costSiteID);
					}

					if (QtyUnCosted > 0m)
					{
						INCostStatus oversold = PXCache<INCostStatus>.CreateCopy(AccumOversoldCostStatus(tran, split, item));
						//qty and cost in this dummy layer must be set explicitly, not added.
						oversold.QtyOnHand = QtyUnCosted;
						oversold.TotalCost = PXCurrencyAttribute.BaseRound(this, QtyUnCosted * (decimal)oversold.UnitCost);

						IssueCost(oversold, tran, split, item, ref QtyUnCosted);
					}

					if (QtyUnCosted > 0m)
					{
						throw new PXException(Messages.InternalError, 500);
					}
				}
			}
		}

		public virtual void TransferCost(INTran tran, INTranSplit split, InventoryItem item)
		{
			if (tran.InvtMult == (short)1 && tran.OrigLineNbr != null)
			{
				PXView cmd = null;

				switch (item.ValMethod)
				{
					case INValMethod.Average:
					case INValMethod.Standard:
					case INValMethod.FIFO:
						cmd = this.TypedViews.GetView(new Select2<INTranCost, LeftJoin<ReadOnlyCostStatus, On<ReadOnlyCostStatus.costID, Equal<INTranCost.costID>>>, Where<INTranCost.tranType, Equal<Current<INTran.origTranType>>, And<INTranCost.refNbr, Equal<Current<INTran.origRefNbr>>, And<INTranCost.lineNbr, Equal<Current<INTran.origLineNbr>>>>>>(), false);
						break;
					case INValMethod.Specific:
						//per Location costing is ignored, CostSiteID <=> OrigLineNbr
						cmd = this.TypedViews.GetView(new Select2<INTranCost, InnerJoin<ReadOnlyCostStatus, On<ReadOnlyCostStatus.costID, Equal<INTranCost.costID>>>, Where<INTranCost.tranType, Equal<Current<INTran.origTranType>>, And<INTranCost.refNbr, Equal<Current<INTran.origRefNbr>>, And<INTranCost.lineNbr, Equal<Current<INTran.origLineNbr>>, And<ReadOnlyCostStatus.costSubItemID, Equal<Required<ReadOnlyCostStatus.costSubItemID>>, And<ReadOnlyCostStatus.lotSerialNbr, Equal<Required<ReadOnlyCostStatus.lotSerialNbr>>>>>>>>(), false);
						break;
					default:
						throw new PXException();
				}

				if (cmd != null)
				{
					decimal QtyUnCosted = INUnitAttribute.ConvertToBase<INTran.inventoryID>(intranselect.Cache, tran, split.UOM, (decimal)split.Qty, INPrecision.QUANTITY);

					foreach (PXResult<INTranCost, ReadOnlyCostStatus> res in cmd.SelectMultiBound(new object[] { tran }, split.CostSubItemID, split.LotSerialNbr))
					{
						INTranCost layer = (INTranCost)res;
						INCostStatus orig_layer = (ReadOnlyCostStatus)res;

						INTranCost receipted = PXSelectReadonly2<INTranCost, 
							InnerJoin<INTran, On<INTran.tranType, Equal<INTranCost.tranType>, And<INTran.refNbr, Equal<INTranCost.refNbr>, And<INTran.lineNbr, Equal<INTranCost.lineNbr>>>>>,
							Where<INTran.origTranType, Equal<Required<INTran.origTranType>>, And<INTran.origRefNbr, Equal<Required<INTran.origRefNbr>>, And<INTran.origLineNbr, Equal<Required<INTran.origLineNbr>>>>>>.SelectWindowed(this, 0, 1, tran.OrigTranType, tran.OrigRefNbr, tran.OrigLineNbr);

						if (receipted != null)
						{
							throw new PXException(Messages.Transfered_Item_Receipted, intranselect.Cache.GetValueExt<INTran.inventoryID>(tran), intrancost.Cache.GetValueExt<INTranCost.refNbr>(receipted));
						}

						if (orig_layer.InventoryID == null)
						{
							//this can be oversold from 1-step transfer not yet committed to the database
							foreach (INCostStatus oversold in oversoldcoststatus.Cache.Inserted)
							{
								if (oversold.CostID == layer.CostID)
								{
									orig_layer = oversold;
									break;
								}
							}
						}

						if (orig_layer.InventoryID == null)
						{
							throw new PXException();
						}

						INTranCost cached = (INTranCost)transfercosts.Locate(layer);
						if (cached == null)
						{
							layer = (INTranCost)transfercosts.Update(layer);
						}
						else
						{
							layer = cached;
						}

						if (layer.QtyOnHand > 0m)
						{
							if (orig_layer.LayerType == INLayerType.Oversold)
							{
								orig_layer = PXCache<INCostStatus>.CreateCopy(orig_layer);
								orig_layer.LayerType = INLayerType.Normal;
							}

							INCostStatus accumlayer = AccumulatedCostStatus(orig_layer, tran, split, item);

							if (accumlayer.ValMethod == "S")
							{
								//were garbaged by UpdateLotSerialStatus()
								accumlayer.ReceiptNbr = orig_layer.ReceiptNbr;
								accumlayer.ReceiptDate = orig_layer.ReceiptDate;
							}

							INTranCost costtran = new INTranCost();
							costtran.InvtAcctID = accumlayer.AccountID;
							costtran.InvtSubID = accumlayer.SubID;
							costtran.COGSAcctID = tran.COGSAcctID;
							costtran.COGSSubID = tran.COGSSubID;
							costtran.CostID = accumlayer.CostID;
							costtran.InventoryID = accumlayer.InventoryID;
							costtran.CostSiteID = accumlayer.CostSiteID;
							costtran.CostSubItemID = accumlayer.CostSubItemID;
							costtran.TranType = tran.TranType;
							costtran.RefNbr = tran.RefNbr;
							costtran.LineNbr = tran.LineNbr;
							costtran.CostDocType = tran.DocType;
							costtran.CostRefNbr = tran.RefNbr;
							costtran.InvtMult = tran.InvtMult;
							costtran.FinPeriodID = tran.FinPeriodID;
							costtran.TranPeriodID = tran.TranPeriodID;
							costtran.TranDate = tran.TranDate;
							costtran.TranAmt = 0m;

                            PXParentAttribute.SetParent(intrancost.Cache, costtran, typeof(INTran), tran);
                            costtran = PXCache<INTranCost>.CreateCopy(intrancost.Insert(costtran));

							accumlayer.QtyOnHand -= costtran.Qty;
							accumlayer.TotalCost -= costtran.TranCost;

							if (layer.QtyOnHand <= QtyUnCosted)
							{
								QtyUnCosted -= (decimal)layer.QtyOnHand;
								costtran.Qty += layer.QtyOnHand;
								if (item.ValMethod == INValMethod.Standard)
								{
									costtran.TranCost += PXCurrencyAttribute.BaseRound(this, (decimal)accumlayer.UnitCost * (decimal)layer.QtyOnHand);
									costtran.VarCost += layer.TotalCost - PXCurrencyAttribute.BaseRound(this, (decimal)accumlayer.UnitCost * (decimal)layer.QtyOnHand);
								}
								else
								{
									costtran.TranCost += layer.TotalCost;
								}
								layer.QtyOnHand = 0m;
								layer.TotalCost = 0m;

							}
							else
							{
								if (PXCurrencyAttribute.IsNullOrEmpty(layer.UnitCost))
								{
									layer.UnitCost = (decimal)layer.TotalCost / (decimal)layer.QtyOnHand;
								}
								costtran.Qty += QtyUnCosted;
								if (item.ValMethod == INValMethod.Standard)
								{
									costtran.TranCost += PXCurrencyAttribute.BaseRound(this, (decimal)accumlayer.UnitCost * QtyUnCosted);
									costtran.VarCost += PXCurrencyAttribute.BaseRound(this, (decimal)layer.UnitCost * QtyUnCosted) - PXCurrencyAttribute.BaseRound(this, (decimal)accumlayer.UnitCost * QtyUnCosted);
								}
								else
								{
									costtran.TranCost += PXCurrencyAttribute.BaseRound(this, QtyUnCosted * (decimal)layer.UnitCost);
								}
								layer.QtyOnHand -= QtyUnCosted;
								layer.TotalCost -= PXCurrencyAttribute.BaseRound(this, QtyUnCosted * (decimal)layer.UnitCost);
								QtyUnCosted = 0m;
							}

							//cmd.View.Cache.SetStatus(layer, PXEntryStatus.Held);

							accumlayer.QtyOnHand += costtran.Qty;
							accumlayer.TotalCost += costtran.TranCost;

							//Accumulate cost issued via PXFormula for Issues only
							intrancost.Update(costtran);

							if (QtyUnCosted == 0m)
							{
								break;
							}
						}
					}

					if (QtyUnCosted > 0m)
					{
						throw new PXException(Messages.Inventory_Negative);
					}
				}
			}
		}

		bool WIPCalculated = false;
		decimal? WIPVariance = 0m;

		public virtual void AssembleCost(INTran tran, INTranSplit split, InventoryItem item)
		{
			if (tran.TranType == INTranType.Assembly && tran.AssyType == INAssyType.KitTran && tran.InvtMult == (short)1)
			{
				tran.TranCost = 0m;

				//rollup stock components
				foreach (INTranCost costtran in intrancost.Cache.Inserted)
				{
					if (string.Equals(costtran.CostDocType, tran.DocType) && string.Equals(costtran.CostRefNbr, tran.RefNbr) && costtran.InvtMult == (short)-1)
					{
						tran.TranCost += costtran.TranCost;
					}
				}

				//rollup non-stock components
				foreach (INTran costtran in intranselect.Cache.Updated)
				{
					if (string.Equals(costtran.DocType, tran.DocType) && string.Equals(costtran.RefNbr, tran.RefNbr) && costtran.AssyType == INAssyType.OverheadTran && costtran.InvtMult == (short)-1)
					{
						tran.TranCost += costtran.TranCost;
					}
				}
			}

			if (tran.TranType == INTranType.Assembly && (tran.AssyType == INAssyType.CompTran || tran.AssyType == INAssyType.OverheadTran) && tran.InvtMult == (short)1)
			{
				if (WIPCalculated == false)
				{
					//rollup kit disassembled
					foreach (INTranCost costtran in intrancost.Cache.Inserted)
					{
						if (string.Equals(costtran.CostDocType, tran.DocType) && string.Equals(costtran.CostRefNbr, tran.RefNbr) && costtran.InvtMult == (short)-1)
						{
							WIPVariance += costtran.TranCost;
						}
					}
					WIPCalculated = true;
				}
				WIPVariance -= tran.TranCost;
			}
		}

		public virtual void UpdateCostStatus(INTran prev_tran, INTran tran, INTranSplit split, InventoryItem item)
		{
			if (object.Equals(prev_tran, tran) == false)
			{
				AssembleCost(tran, split, item);

				if (tran.BaseQty != 0m)
				{
					tran.CostedQty = 0m;
					tran.OrigTranCost = tran.TranCost;
					tran.OrigTranAmt = tran.TranAmt;
					tran.TranCost = 0m;
					tran.TranAmt = 0m;

					//CommandPreparing will prevent actual update
					tran.UnitCost = (decimal)tran.OrigTranCost / (decimal)tran.BaseQty;
					tran.UnitPrice = (decimal)tran.OrigTranAmt / (decimal)tran.BaseQty;
				}
				else
				{
                    //prevent SelectSiblings on null value.
                    tran.CostedQty = 0m;
					tran.UnitCost = 0m;
					tran.UnitPrice = 0m;
				}
			}

            try
            {
                ReceiveCost(tran, split, item);
                IssueCost(tran, split, item, false);
            }
            catch (PXQtyCostImbalanceException)
            {
                IssueCost(tran, split, item, true);
            }
			TransferCost(tran, split, item);
		}

		public class INHistBucket
		{
			public decimal SignReceived = 0m;
			public decimal SignIssued = 0m;
			public decimal SignSales = 0m;
			public decimal SignCreditMemos = 0m;
			public decimal SignDropShip = 0m;
			public decimal SignTransferIn = 0m;
			public decimal SignTransferOut = 0m;
			public decimal SignAdjusted = 0m;
			public decimal SignAssemblyIn = 0m;
			public decimal SignAssemblyOut = 0m;
			public decimal SignYtd = 0m;

			public INHistBucket(INTran tran)
				: this(tran.TranType, tran.InvtMult)
			{
			}

			public INHistBucket(INTranCost costtran, INTran intran)
				: this(costtran.TranType, costtran.InvtMult)
			{
				if ((costtran.TranType == INTranType.Transfer || costtran.TranType == INTranType.Assembly) && (costtran.CostDocType != intran.DocType || costtran.CostRefNbr != intran.RefNbr))
				{
					this.SignTransferOut = 0m;
					this.SignSales = 1m;
				}
			}

			public INHistBucket(INTranSplit tran)
				: this(tran.TranType, tran.InvtMult)
			{
			}

			public INHistBucket(string TranType, short? InvtMult)
			{
				SignYtd = (decimal)InvtMult;

				switch (TranType)
				{
					case INTranType.Receipt:
						SignReceived = 1m;
						break;
					case INTranType.Issue:
						SignIssued = 1m;
						break;
					case INTranType.Return:
						SignIssued = -1m;
						break;
					case INTranType.Invoice:
					case INTranType.DebitMemo:
						if (SignYtd == 0m)
						{
							SignDropShip = 1m;
						}
						else
						{
							SignSales = 1m;
						}
						break;
					case INTranType.CreditMemo:
						if (SignYtd == 0m)
						{
							SignDropShip = -1m;
						}
						else
						{
							SignCreditMemos = 1m;
						}
						break;
					case INTranType.Transfer:
						if (InvtMult == 1m)
						{
							SignTransferIn = 1m;
						}
						else
						{
							SignTransferOut = 1m;
						}
						break;
					case INTranType.Adjustment:
						if (InvtMult == 0m)
						{
							SignAdjusted = 1m;
							SignSales = 1m;
						}
						else
						{
							SignAdjusted = (decimal)InvtMult;
						}
						break;
					case INTranType.StandardCostAdjustment:
					case INTranType.NegativeCostAdjustment:
						SignAdjusted = 1m;
						break;
					case INTranType.Assembly:
						if (InvtMult == 1m)
						{
							SignAssemblyIn = 1m;
						}
						else
						{
							SignAssemblyOut = 1m;
						}
						break;
					default:
						throw new PXException();
				}
			}
		}

		protected static void UpdateHistoryField<FinHistoryField, TranHistoryField>(PXGraph graph, object data, decimal? value, bool IsFinField)
			where FinHistoryField : IBqlField
			where TranHistoryField : IBqlField
		{
			PXCache cache = graph.Caches[BqlCommand.GetItemType(typeof(FinHistoryField))];

			if (IsFinField)
			{
				decimal? oldvalue = (decimal?)cache.GetValue<FinHistoryField>(data);

				cache.SetValue<FinHistoryField>(data, (oldvalue ?? 0m) + (value ?? 0m));
			}
			else
			{
				decimal? oldvalue = (decimal?)cache.GetValue<TranHistoryField>(data);

				cache.SetValue<TranHistoryField>(data, (oldvalue ?? 0m) + (value ?? 0m));
			}
		}

		public static void UpdateCostHist(PXGraph graph, INHistBucket bucket, INTranCost tran, string PeriodID, bool FinFlag)
		{
			ItemCostHist hist = new ItemCostHist();
			hist.InventoryID = tran.InventoryID;
			hist.CostSiteID = tran.CostSiteID;
			hist.AccountID = tran.InvtAcctID;
			hist.SubID = tran.InvtSubID;
			hist.CostSubItemID = tran.CostSubItemID;
			hist.FinPeriodID = PeriodID;

			hist = (ItemCostHist)graph.Caches[typeof(ItemCostHist)].Insert(hist);

			UpdateHistoryField<ItemCostHist.finPtdCostReceived, ItemCostHist.tranPtdCostReceived>(graph, hist, tran.TranCost * bucket.SignReceived, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdCostIssued, ItemCostHist.tranPtdCostIssued>(graph, hist, tran.TranCost * bucket.SignIssued, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdCOGS, ItemCostHist.tranPtdCOGS>(graph, hist, tran.TranCost * bucket.SignSales, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdCOGSCredits, ItemCostHist.tranPtdCOGSCredits>(graph, hist, tran.TranCost * bucket.SignCreditMemos, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdCOGSDropShips, ItemCostHist.tranPtdCOGSDropShips>(graph, hist, tran.TranCost * bucket.SignDropShip, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdCostTransferIn, ItemCostHist.tranPtdCostTransferIn>(graph, hist, tran.TranCost * bucket.SignTransferIn, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdCostTransferOut, ItemCostHist.tranPtdCostTransferOut>(graph, hist, tran.TranCost * bucket.SignTransferOut, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdCostAdjusted, ItemCostHist.tranPtdCostAdjusted>(graph, hist, tran.TranCost * bucket.SignAdjusted, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdCostAssemblyIn, ItemCostHist.tranPtdCostAssemblyIn>(graph, hist, tran.TranCost * bucket.SignAssemblyIn, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdCostAssemblyOut, ItemCostHist.tranPtdCostAssemblyOut>(graph, hist, tran.TranCost * bucket.SignAssemblyOut, FinFlag);

			UpdateHistoryField<ItemCostHist.finPtdQtyReceived, ItemCostHist.tranPtdQtyReceived>(graph, hist, tran.Qty * bucket.SignReceived, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdQtyIssued, ItemCostHist.tranPtdQtyIssued>(graph, hist, tran.Qty * bucket.SignIssued, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdQtySales, ItemCostHist.tranPtdQtySales>(graph, hist, tran.Qty * bucket.SignSales, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdQtyCreditMemos, ItemCostHist.tranPtdQtyCreditMemos>(graph, hist, tran.Qty * bucket.SignCreditMemos, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdQtyDropShipSales, ItemCostHist.tranPtdQtyDropShipSales>(graph, hist, tran.Qty * bucket.SignDropShip, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdQtyTransferIn, ItemCostHist.tranPtdQtyTransferIn>(graph, hist, tran.Qty * bucket.SignTransferIn, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdQtyTransferOut, ItemCostHist.tranPtdQtyTransferOut>(graph, hist, tran.Qty * bucket.SignTransferOut, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdQtyAdjusted, ItemCostHist.tranPtdQtyAdjusted>(graph, hist, tran.Qty * bucket.SignAdjusted, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdQtyAssemblyIn, ItemCostHist.tranPtdQtyAssemblyIn>(graph, hist, tran.Qty * bucket.SignAssemblyIn, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdQtyAssemblyOut, ItemCostHist.tranPtdQtyAssemblyOut>(graph, hist, tran.Qty * bucket.SignAssemblyOut, FinFlag);

			UpdateHistoryField<ItemCostHist.finPtdSales, ItemCostHist.tranPtdSales>(graph, hist, tran.TranAmt * bucket.SignSales, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdCreditMemos, ItemCostHist.tranPtdCreditMemos>(graph, hist, tran.TranAmt * bucket.SignCreditMemos, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdDropShipSales, ItemCostHist.tranPtdDropShipSales>(graph, hist, tran.TranAmt * bucket.SignDropShip, FinFlag);

			UpdateHistoryField<ItemCostHist.finYtdQty, ItemCostHist.tranYtdQty>(graph, hist, tran.Qty * bucket.SignYtd, FinFlag);
			UpdateHistoryField<ItemCostHist.finYtdCost, ItemCostHist.tranYtdCost>(graph, hist, tran.TranCost * bucket.SignYtd, FinFlag);
		}

		public static void UpdateCostHist(PXGraph graph, INTranCost costtran, INTran intran)
		{
			INHistBucket bucket = new INHistBucket(costtran, intran);

			UpdateCostHist(graph, bucket, costtran, costtran.FinPeriodID, true);
			UpdateCostHist(graph, bucket, costtran, costtran.TranPeriodID, false);
		}

		protected virtual void UpdateCostHist(INTranCost costtran, INTran intran)
		{
			UpdateCostHist(this, costtran, intran);
		}

		protected virtual void UpdateSalesHist(INHistBucket bucket, INTranCost tran, string PeriodID, bool FinFlag)
		{
			ItemSalesHist hist = new ItemSalesHist();
			hist.InventoryID = tran.InventoryID;
			hist.CostSiteID = tran.CostSiteID;
			hist.CostSubItemID = tran.CostSubItemID;
			hist.FinPeriodID = PeriodID;

			hist = itemsaleshist.Insert(hist);

			UpdateHistoryField<ItemSalesHist.finPtdCOGS, ItemSalesHist.tranPtdCOGS>(this, hist, tran.TranCost * bucket.SignSales, FinFlag);
			UpdateHistoryField<ItemSalesHist.finPtdCOGSCredits, ItemSalesHist.tranPtdCOGSCredits>(this, hist, tran.TranCost * bucket.SignCreditMemos, FinFlag);
			UpdateHistoryField<ItemSalesHist.finPtdCOGSDropShips, ItemSalesHist.tranPtdCOGSDropShips>(this, hist, tran.TranCost * bucket.SignDropShip, FinFlag);

			UpdateHistoryField<ItemSalesHist.finPtdQtySales, ItemSalesHist.tranPtdQtySales>(this, hist, tran.Qty * bucket.SignSales, FinFlag);
			UpdateHistoryField<ItemSalesHist.finPtdQtyCreditMemos, ItemSalesHist.tranPtdQtyCreditMemos>(this, hist, tran.Qty * bucket.SignCreditMemos, FinFlag);
			UpdateHistoryField<ItemSalesHist.finPtdQtyDropShipSales, ItemSalesHist.tranPtdQtyDropShipSales>(this, hist, tran.Qty * bucket.SignDropShip, FinFlag);

			UpdateHistoryField<ItemSalesHist.finPtdSales, ItemSalesHist.tranPtdSales>(this, hist, tran.TranAmt * bucket.SignSales, FinFlag);
			UpdateHistoryField<ItemSalesHist.finPtdCreditMemos, ItemSalesHist.tranPtdCreditMemos>(this, hist, tran.TranAmt * bucket.SignCreditMemos, FinFlag);
			UpdateHistoryField<ItemSalesHist.finPtdDropShipSales, ItemSalesHist.tranPtdDropShipSales>(this, hist, tran.TranAmt * bucket.SignDropShip, FinFlag);

			UpdateHistoryField<ItemSalesHist.finYtdCOGS, ItemSalesHist.tranYtdCOGS>(this, hist, tran.TranCost * bucket.SignSales, FinFlag);
			UpdateHistoryField<ItemSalesHist.finYtdCOGSCredits, ItemSalesHist.tranYtdCOGSCredits>(this, hist, tran.TranCost * bucket.SignCreditMemos, FinFlag);
			UpdateHistoryField<ItemSalesHist.finYtdCOGSDropShips, ItemSalesHist.tranYtdCOGSDropShips>(this, hist, tran.TranCost * bucket.SignDropShip, FinFlag);

			UpdateHistoryField<ItemSalesHist.finYtdQtySales, ItemSalesHist.tranYtdQtySales>(this, hist, tran.Qty * bucket.SignSales, FinFlag);
			UpdateHistoryField<ItemSalesHist.finYtdQtyCreditMemos, ItemSalesHist.tranYtdQtyCreditMemos>(this, hist, tran.Qty * bucket.SignCreditMemos, FinFlag);
			UpdateHistoryField<ItemSalesHist.finYtdQtyDropShipSales, ItemSalesHist.tranYtdQtyDropShipSales>(this, hist, tran.Qty * bucket.SignDropShip, FinFlag);

			UpdateHistoryField<ItemSalesHist.finYtdSales, ItemSalesHist.tranYtdSales>(this, hist, tran.TranAmt * bucket.SignSales, FinFlag);
			UpdateHistoryField<ItemSalesHist.finYtdCreditMemos, ItemSalesHist.tranYtdCreditMemos>(this, hist, tran.TranAmt * bucket.SignCreditMemos, FinFlag);
			UpdateHistoryField<ItemSalesHist.finYtdDropShipSales, ItemSalesHist.tranYtdDropShipSales>(this, hist, tran.TranAmt * bucket.SignDropShip, FinFlag);
		}

		protected virtual void UpdateSalesHist(INTranCost costtran, INTran intran)
		{
			INHistBucket bucket = new INHistBucket(costtran, intran);

			UpdateSalesHist(bucket, costtran, costtran.FinPeriodID, true);
			UpdateSalesHist(bucket, costtran, costtran.TranPeriodID, false);
		}

		protected virtual void UpdateSalesHistD(INTran intran)
		{
			UpdateSalesHistD(this, intran);
		}
		public static void UpdateSalesHistD(PXGraph graph, INTran intran)
		{
			INHistBucket bucket = new INHistBucket(intran);
			if (intran.TranDate == null) return;
			DateTime date = (DateTime)intran.TranDate;
			ItemSalesHistD hist = new ItemSalesHistD();
			hist.InventoryID = intran.InventoryID;
			hist.SiteID = intran.SiteID;
			hist.SubItemID = intran.SubItemID;
			hist.SDate = date;

			if (intran.BaseQty * bucket.SignSales <= 0 || intran.SubItemID == null) return;

			hist = (ItemSalesHistD)graph.Caches[typeof(ItemSalesHistD)].Insert(hist);

			PXResult<CSCalendar, CSCalendarExceptions> result =
				(PXResult<CSCalendar, CSCalendarExceptions>)
				PXSelectJoin<CSCalendar,
					LeftJoin<CSCalendarExceptions,
					On<CSCalendarExceptions.calendarID, Equal<CSCalendar.calendarID>,
					And<CSCalendarExceptions.date, Equal<Required<CSCalendarExceptions.date>>>>,
					InnerJoin<INReplenishmentPolicy,
					On<INReplenishmentPolicy.calendarID, Equal<CSCalendar.calendarID>>,
					InnerJoin<INItemSite,
					On<INItemSite.replenishmentPolicyID, Equal<INReplenishmentPolicy.replenishmentPolicyID>,
					And<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
					And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>>>>>
					.SelectWindowed(graph, 0, 1, date, intran.InventoryID, intran.SiteID);
			if (result != null)
			{
				CSCalendar calendar = result;
				CSCalendarExceptions exc = result;
				if (exc.Date != null)
				{
					hist.DemandType1 = exc.WorkDay == true ? 1 : 0;
					hist.DemandType2 = exc.WorkDay != true ? 1 : 0;
				}
				else
				{
					hist.DemandType1 = calendar.IsWorkDay(date) ? 1 : 0;
					hist.DemandType2 = calendar.IsWorkDay(date) ? 0 : 1;
				}
			}
			hist.SYear = date.Year;
			hist.SMonth = date.Month;
			hist.SDay = date.Day;
			hist.SQuater = (date.Month + 2) / 3;
			hist.SDayOfWeek = (int)date.DayOfWeek;
			hist.QtyIssues += intran.BaseQty * bucket.SignSales;
		}

		protected virtual void UpdateCustSalesStats(INTran intran)
		{
			UpdateCustSalesStats(this, intran);
		}

		public static void UpdateCustSalesStats(PXGraph graph, INTran intran)
		{
			INHistBucket bucket = new INHistBucket(intran);

			if (intran.TranDate == null || intran.BaseQty == 0 ||
					intran.BAccountID == null || bucket.SignSales == 0 || intran.SubItemID == null) return;

			ItemCustSalesStats stats = new ItemCustSalesStats();
			stats.InventoryID = intran.InventoryID;
			stats.SubItemID = intran.SubItemID;
			stats.SiteID = intran.SiteID;
			stats.BAccountID = intran.BAccountID;
			stats = (ItemCustSalesStats)graph.Caches[typeof(ItemCustSalesStats)].Insert(stats);
			if (stats.LastDate == null || stats.LastDate < intran.TranDate)
			{
				stats.LastDate = intran.TranDate;
				stats.LastQty = intran.BaseQty;
				//during release process intran.UnitPrice is recalculated for base uom and discarded after
				stats.LastUnitPrice = intran.TranAmt / intran.BaseQty;
			}
		}

		protected virtual void UpdateCustSalesHist(INHistBucket bucket, INTranCost tran, string PeriodID, bool FinFlag, INTran intran)
		{
			if (intran.BAccountID == null) return;

			ItemCustSalesHist hist = new ItemCustSalesHist();
			hist.InventoryID = tran.InventoryID;
			hist.CostSiteID = tran.CostSiteID;
			hist.CostSubItemID = tran.CostSubItemID;
			hist.FinPeriodID = PeriodID;
			hist.BAccountID = intran.BAccountID;

			hist = itemcustsaleshist.Insert(hist);

			UpdateHistoryField<ItemCustSalesHist.finPtdCOGS, ItemCustSalesHist.tranPtdCOGS>(this, hist, tran.TranCost * bucket.SignSales, FinFlag);
			UpdateHistoryField<ItemCustSalesHist.finPtdCOGSCredits, ItemCustSalesHist.tranPtdCOGSCredits>(this, hist, tran.TranCost * bucket.SignCreditMemos, FinFlag);
			UpdateHistoryField<ItemCustSalesHist.finPtdCOGSDropShips, ItemCustSalesHist.tranPtdCOGSDropShips>(this, hist, tran.TranCost * bucket.SignDropShip, FinFlag);

			UpdateHistoryField<ItemCustSalesHist.finPtdQtySales, ItemCustSalesHist.tranPtdQtySales>(this, hist, tran.Qty * bucket.SignSales, FinFlag);
			UpdateHistoryField<ItemCustSalesHist.finPtdQtyCreditMemos, ItemCustSalesHist.tranPtdQtyCreditMemos>(this, hist, tran.Qty * bucket.SignCreditMemos, FinFlag);
			UpdateHistoryField<ItemCustSalesHist.finPtdQtyDropShipSales, ItemCustSalesHist.tranPtdQtyDropShipSales>(this, hist, tran.Qty * bucket.SignDropShip, FinFlag);

			UpdateHistoryField<ItemCustSalesHist.finPtdSales, ItemCustSalesHist.tranPtdSales>(this, hist, tran.TranAmt * bucket.SignSales, FinFlag);
			UpdateHistoryField<ItemCustSalesHist.finPtdCreditMemos, ItemCustSalesHist.tranPtdCreditMemos>(this, hist, tran.TranAmt * bucket.SignCreditMemos, FinFlag);
			UpdateHistoryField<ItemCustSalesHist.finPtdDropShipSales, ItemCustSalesHist.tranPtdDropShipSales>(this, hist, tran.TranAmt * bucket.SignDropShip, FinFlag);

			UpdateHistoryField<ItemCustSalesHist.finYtdCOGS, ItemCustSalesHist.tranYtdCOGS>(this, hist, tran.TranCost * bucket.SignSales, FinFlag);
			UpdateHistoryField<ItemCustSalesHist.finYtdCOGSCredits, ItemCustSalesHist.tranYtdCOGSCredits>(this, hist, tran.TranCost * bucket.SignCreditMemos, FinFlag);
			UpdateHistoryField<ItemCustSalesHist.finYtdCOGSDropShips, ItemCustSalesHist.tranYtdCOGSDropShips>(this, hist, tran.TranCost * bucket.SignDropShip, FinFlag);

			UpdateHistoryField<ItemCustSalesHist.finYtdQtySales, ItemCustSalesHist.tranYtdQtySales>(this, hist, tran.Qty * bucket.SignSales, FinFlag);
			UpdateHistoryField<ItemCustSalesHist.finYtdQtyCreditMemos, ItemCustSalesHist.tranYtdQtyCreditMemos>(this, hist, tran.Qty * bucket.SignCreditMemos, FinFlag);
			UpdateHistoryField<ItemCustSalesHist.finYtdQtyDropShipSales, ItemCustSalesHist.tranYtdQtyDropShipSales>(this, hist, tran.Qty * bucket.SignDropShip, FinFlag);

			UpdateHistoryField<ItemCustSalesHist.finYtdSales, ItemCustSalesHist.tranYtdSales>(this, hist, tran.TranAmt * bucket.SignSales, FinFlag);
			UpdateHistoryField<ItemCustSalesHist.finYtdCreditMemos, ItemCustSalesHist.tranYtdCreditMemos>(this, hist, tran.TranAmt * bucket.SignCreditMemos, FinFlag);
			UpdateHistoryField<ItemCustSalesHist.finYtdDropShipSales, ItemCustSalesHist.tranYtdDropShipSales>(this, hist, tran.TranAmt * bucket.SignDropShip, FinFlag);
		}

		protected virtual void UpdateCustSalesHist(INTranCost costtran, INTran intran)
		{
			INHistBucket bucket = new INHistBucket(costtran, intran);
			UpdateCustSalesHist(bucket, costtran, costtran.FinPeriodID, true, intran);
			UpdateCustSalesHist(bucket, costtran, costtran.TranPeriodID, false, intran);
		}

		protected static void UpdateSiteHist(PXGraph graph, INHistBucket bucket, INTranSplit tran, string PeriodID, bool FinFlag)
		{
			ItemSiteHist hist = new ItemSiteHist();
			hist.InventoryID = tran.InventoryID;
			hist.SiteID = tran.SiteID;
			hist.SubItemID = tran.SubItemID;
			hist.LocationID = tran.LocationID;
			hist.FinPeriodID = PeriodID;

			hist = (ItemSiteHist)graph.Caches[typeof(ItemSiteHist)].Insert(hist);

			UpdateHistoryField<ItemSiteHist.finPtdQtyReceived, ItemSiteHist.tranPtdQtyReceived>(graph, hist, tran.BaseQty * bucket.SignReceived, FinFlag);
			UpdateHistoryField<ItemSiteHist.finPtdQtyIssued, ItemSiteHist.tranPtdQtyIssued>(graph, hist, tran.BaseQty * bucket.SignIssued, FinFlag);
			UpdateHistoryField<ItemSiteHist.finPtdQtySales, ItemSiteHist.tranPtdQtySales>(graph, hist, tran.BaseQty * bucket.SignSales, FinFlag);
			UpdateHistoryField<ItemSiteHist.finPtdQtyCreditMemos, ItemSiteHist.tranPtdQtyCreditMemos>(graph, hist, tran.BaseQty * bucket.SignCreditMemos, FinFlag);
			UpdateHistoryField<ItemSiteHist.finPtdQtyDropShipSales, ItemSiteHist.tranPtdQtyDropShipSales>(graph, hist, tran.BaseQty * bucket.SignDropShip, FinFlag);
			UpdateHistoryField<ItemSiteHist.finPtdQtyTransferIn, ItemSiteHist.tranPtdQtyTransferIn>(graph, hist, tran.BaseQty * bucket.SignTransferIn, FinFlag);
			UpdateHistoryField<ItemSiteHist.finPtdQtyTransferOut, ItemSiteHist.tranPtdQtyTransferOut>(graph, hist, tran.BaseQty * bucket.SignTransferOut, FinFlag);
			UpdateHistoryField<ItemSiteHist.finPtdQtyAdjusted, ItemSiteHist.tranPtdQtyAdjusted>(graph, hist, tran.BaseQty * bucket.SignAdjusted, FinFlag);
			UpdateHistoryField<ItemSiteHist.finPtdQtyAssemblyIn, ItemSiteHist.tranPtdQtyAssemblyIn>(graph, hist, tran.BaseQty * bucket.SignAssemblyIn, FinFlag);
			UpdateHistoryField<ItemSiteHist.finPtdQtyAssemblyOut, ItemSiteHist.tranPtdQtyAssemblyOut>(graph, hist, tran.BaseQty * bucket.SignAssemblyOut, FinFlag);
			UpdateHistoryField<ItemSiteHist.finYtdQty, ItemSiteHist.tranYtdQty>(graph, hist, tran.BaseQty * bucket.SignYtd, FinFlag);
		}

		public static void UpdateSiteHist(PXGraph graph, INTran tran, INTranSplit split)
		{
			//for negative adjustments line InvtMult == 1 split/cost InvtMult == -1
			INHistBucket bucket = new INHistBucket(split);

			UpdateSiteHist(graph, bucket, split, tran.FinPeriodID, true);
			UpdateSiteHist(graph, bucket, split, tran.TranPeriodID, false);
		}

		protected virtual void UpdateSiteHist(INTran tran, INTranSplit split)
		{
			UpdateSiteHist(this, tran, split);
		}

        public static void UpdateSiteHistD(PXGraph graph, INTranSplit tran)
        {
            //for negative adjustments line InvtMult == 1 split/cost InvtMult == -1
            INHistBucket bucket = new INHistBucket(tran);

            ItemSiteHistD hist = new ItemSiteHistD();
            DateTime date = (DateTime) tran.TranDate;
            hist.InventoryID = tran.InventoryID;
            hist.SiteID = tran.SiteID;
            hist.SubItemID = tran.SubItemID;
            hist.SDate = date;

						if (tran.TranType == INTranType.Transfer && tran.InvtMult == -1 && tran.SiteID == tran.ToSiteID)
						{
							bucket.SignTransferIn =  -1;
							bucket.SignTransferOut = 0;
						}


	        hist = (ItemSiteHistD)graph.Caches[typeof(ItemSiteHistD)].Insert(hist);

            hist.SYear = date.Year;
            hist.SMonth = date.Month;
            hist.SDay = date.Day;
            hist.SQuater = (date.Month + 2) / 3;
            hist.SDayOfWeek = (int)date.DayOfWeek;

            UpdateHistoryField<ItemSiteHistD.qtyReceived, ItemSiteHistD.qtyReceived>(graph, hist, tran.BaseQty * bucket.SignReceived, true);
            UpdateHistoryField<ItemSiteHistD.qtyIssued, ItemSiteHistD.qtyIssued>(graph, hist, tran.BaseQty * bucket.SignIssued, true);
            UpdateHistoryField<ItemSiteHistD.qtySales, ItemSiteHistD.qtySales>(graph, hist, tran.BaseQty * bucket.SignSales, true);
            UpdateHistoryField<ItemSiteHistD.qtyCreditMemos, ItemSiteHistD.qtyCreditMemos>(graph, hist, tran.BaseQty * bucket.SignCreditMemos, true);
            UpdateHistoryField<ItemSiteHistD.qtyDropShipSales, ItemSiteHistD.qtyDropShipSales>(graph, hist, tran.BaseQty * bucket.SignDropShip, true);
            UpdateHistoryField<ItemSiteHistD.qtyTransferIn, ItemSiteHistD.qtyTransferIn>(graph, hist, tran.BaseQty * bucket.SignTransferIn, true);
            UpdateHistoryField<ItemSiteHistD.qtyTransferOut, ItemSiteHistD.qtyTransferOut>(graph, hist, tran.BaseQty * bucket.SignTransferOut, true);
            UpdateHistoryField<ItemSiteHistD.qtyAdjusted, ItemSiteHistD.qtyAdjusted>(graph, hist, tran.BaseQty * bucket.SignAdjusted, true);
            UpdateHistoryField<ItemSiteHistD.qtyAssemblyIn, ItemSiteHistD.qtyAssemblyIn>(graph, hist, tran.BaseQty * bucket.SignAssemblyIn, true);
            UpdateHistoryField<ItemSiteHistD.qtyAssemblyOut, ItemSiteHistD.qtyAssemblyOut>(graph, hist, tran.BaseQty * bucket.SignAssemblyOut, true);

        }

        protected virtual void UpdateSiteHistD(INTranSplit tran)
        {
            UpdateSiteHistD(this, tran);
        }


		public int? GetAcctID<Field>(string AcctDefault, InventoryItem item, INSite site, INPostClass postclass)
			where Field : IBqlField
		{
			return GetAcctID<Field>(this, AcctDefault, item, site, postclass);
		}

		public static int? GetAcctID<Field>(PXGraph graph, string AcctDefault, InventoryItem item, INSite site, INPostClass postclass)
			where Field : IBqlField
		{
			switch (AcctDefault)
			{
				case INAcctSubDefault.MaskItem:
				default:
					{
						PXCache cache = graph.Caches[typeof(InventoryItem)];
						try
						{
							return (int)cache.GetValue<Field>(item);
						}
						catch (NullReferenceException)
						{
							object keyval = cache.GetStateExt<InventoryItem.inventoryCD>(item);
							if (item.StkItem == true)
							{
								throw new PXMaskArgumentException(Messages.MaskItem, PXUIFieldAttribute.GetDisplayName<Field>(cache), keyval);
							}
							throw new PXMaskArgumentException(Messages.MaskItem, GetSubstFieldDesr<Field>(cache), keyval);
						}
					}
				case INAcctSubDefault.MaskSite:
					{
						PXCache cache = graph.Caches[typeof(INSite)];
						try
						{
							return (int)cache.GetValue<Field>(site);
						}
						catch (NullReferenceException)
						{
							object keyval = cache.GetStateExt<INSite.siteCD>(site);
							throw new PXMaskArgumentException(Messages.MaskSite, PXUIFieldAttribute.GetDisplayName<Field>(cache), keyval);
						}
					}
				case INAcctSubDefault.MaskClass:
					{
						PXCache cache = graph.Caches[typeof(INPostClass)];
						try
						{
							return (int)cache.GetValue<Field>(postclass);
						}
						catch (NullReferenceException)
						{
							object keyval = cache.GetStateExt<INPostClass.postClassID>(postclass);
							throw new PXMaskArgumentException(Messages.MaskClass, PXUIFieldAttribute.GetDisplayName<Field>(cache), keyval);
						}
					}
			}
		}

		public static string GetSubstFieldDesr<Field>(PXCache cache)
			where Field : IBqlField
		{
			if (typeof (Field) == typeof (INPostClass.invtAcctID))
			{
				return PXUIFieldAttribute.GetDisplayName<NonStockItem.invtAcctID>(cache);
			}
			if (typeof(Field) == typeof(INPostClass.cOGSAcctID))
			{
				return PXUIFieldAttribute.GetDisplayName<NonStockItem.cOGSAcctID>(cache);
			}
			return PXUIFieldAttribute.GetDisplayName<Field>(cache);
		}

		public int? GetSubID<Field>(string AcctDefault, string SubMask, InventoryItem item, INSite site, INPostClass postclass)
			where Field : IBqlField
		{
			return GetSubID<Field>(this, AcctDefault, SubMask, item, site, postclass);
		}

		public static int? GetSubID<Field>(PXGraph graph, string AcctDefault, string SubMask, InventoryItem item, INSite site, INPostClass postclass)
			where Field : IBqlField
		{
			return GetSubID<Field>(graph, AcctDefault, SubMask, item, site, postclass, null);
		}

		public int? GetSubID<Field>(string AcctDefault, string SubMask, InventoryItem item, INSite site, INPostClass postclass, INTran tran)
			where Field : IBqlField
		{
			return GetSubID<Field>(this, AcctDefault, SubMask, item, site, postclass);
		}

		public static int? GetSubID<Field>(PXGraph graph, string AcctDefault, string SubMask, InventoryItem item, INSite site, INPostClass postclass, INTran tran)
			where Field : IBqlField
		{
			if (typeof(Field) == typeof(INPostClass.cOGSSubID) && tran != null && postclass.COGSSubFromSales == true)
			{
				PXCache cache = graph.Caches[typeof(INTran)];

				object tran_SubID = cache.GetValueExt<INTran.subID>(tran);
				object value = (tran_SubID is PXFieldState) ? ((PXFieldState)tran_SubID).Value : tran_SubID;

				cache.RaiseFieldUpdating<Field>(tran, ref value);
				return (int?)value;
			}
			else
			{
				int? item_SubID = (int?)graph.Caches[typeof(InventoryItem)].GetValue<Field>(item);
				int? site_SubID = (int?)graph.Caches[typeof(INSite)].GetValue<Field>(site);
				int? class_SubID = (int?)graph.Caches[typeof(INPostClass)].GetValue<Field>(postclass);

				object value = null;

				try
				{
					if (item.StkItem == true && typeof(Field) == typeof(INPostClass.invtSubID))
						value = SubAccountMaskAttribute.MakeSub<INPostClass.invtSubMask>(graph, SubMask, item.StkItem, new object[] { item_SubID, site_SubID, class_SubID }, new Type[] { typeof(InventoryItem.invtSubID), typeof(INSite.invtSubID), typeof(INPostClass.invtSubID) });
					if (item.StkItem != true && typeof(Field) == typeof(INPostClass.invtSubID))
						value = SubAccountMaskAttribute.MakeSub<INPostClass.invtSubMask>(graph, SubMask, item.StkItem, new object[] { item_SubID, site_SubID, class_SubID }, new Type[] { typeof(NonStockItem.invtSubID), typeof(INSite.invtSubID), typeof(INPostClass.invtSubID) });
					if (item.StkItem == true && typeof(Field) == typeof(INPostClass.cOGSSubID))
						value = SubAccountMaskAttribute.MakeSub<INPostClass.cOGSSubMask>(graph, SubMask, item.StkItem, new object[] { item_SubID, site_SubID, class_SubID }, new Type[] { typeof(InventoryItem.cOGSSubID), typeof(INSite.cOGSSubID), typeof(INPostClass.cOGSSubID) });
					if (item.StkItem != true && typeof(Field) == typeof(INPostClass.cOGSSubID))
						value = SubAccountMaskAttribute.MakeSub<INPostClass.cOGSSubMask>(graph, SubMask, item.StkItem, new object[] { item_SubID, site_SubID, class_SubID }, new Type[] { typeof(NonStockItem.cOGSSubID), typeof(INSite.cOGSSubID), typeof(INPostClass.cOGSSubID) });
					if (typeof(Field) == typeof(INPostClass.salesSubID))
						value = SubAccountMaskAttribute.MakeSub<INPostClass.salesSubMask>(graph, SubMask, new object[] { item_SubID, site_SubID, class_SubID }, new Type[] { typeof(InventoryItem.salesSubID), typeof(INSite.salesSubID), typeof(INPostClass.salesSubID) });
					if (typeof(Field) == typeof(INPostClass.stdCstVarSubID))
						value = SubAccountMaskAttribute.MakeSub<INPostClass.stdCstVarSubMask>(graph, SubMask, new object[] { item_SubID, site_SubID, class_SubID }, new Type[] { typeof(InventoryItem.stdCstVarSubID), typeof(INSite.stdCstVarSubID), typeof(INPostClass.stdCstVarSubID) });
					if (typeof(Field) == typeof(INPostClass.stdCstRevSubID))
						value = SubAccountMaskAttribute.MakeSub<INPostClass.stdCstRevSubMask>(graph, SubMask, new object[] { item_SubID, site_SubID, class_SubID }, new Type[] { typeof(InventoryItem.stdCstRevSubID), typeof(INSite.stdCstRevSubID), typeof(INPostClass.stdCstRevSubID) });
                    if (typeof(Field) == typeof(INPostClass.pOAccrualSubID))
                        throw new NotImplementedException();
					if (typeof(Field) == typeof(INPostClass.pPVSubID))
						value = SubAccountMaskAttribute.MakeSub<INPostClass.pPVSubMask>(graph, SubMask, new object[] { item_SubID, site_SubID, class_SubID }, new Type[] { typeof(InventoryItem.pPVSubID), typeof(INSite.pPVSubID), typeof(INPostClass.pPVSubID) });
					if (typeof(Field) == typeof(INPostClass.lCVarianceSubID))
						value = SubAccountMaskAttribute.MakeSub<INPostClass.lCVarianceSubMask>(graph, SubMask, new object[] { item_SubID, site_SubID, class_SubID }, new Type[] { typeof(InventoryItem.lCVarianceSubID), typeof(INSite.lCVarianceSubID), typeof(INPostClass.lCVarianceSubID) });
				}
				catch (PXMaskArgumentException ex)
				{
					object keyval;
					switch (ex.SourceIdx)
					{
						case 0:
						default:
							keyval = graph.Caches[typeof(InventoryItem)].GetStateExt<InventoryItem.inventoryCD>(item);
							break;
						case 1:
							keyval = graph.Caches[typeof(INSite)].GetStateExt<INSite.siteCD>(site);
							break;
						case 2:
							keyval = graph.Caches[typeof(INPostClass)].GetStateExt<INPostClass.postClassID>(postclass);
							break;
					}
					throw new PXMaskArgumentException(ex, keyval);
				}

				switch (AcctDefault)
				{
					case INAcctSubDefault.MaskItem:
					default:
						RaiseFieldUpdating<Field>(graph.Caches[typeof(InventoryItem)], item, ref value);
						break;
					case INAcctSubDefault.MaskSite:
						RaiseFieldUpdating<Field>(graph.Caches[typeof(INSite)], site, ref value);
						break;
					case INAcctSubDefault.MaskClass:
						RaiseFieldUpdating<Field>(graph.Caches[typeof(INPostClass)], postclass, ref value);
						break;
				}
				return (int?)value;
			}
		}

        public static int? GetPOAccrualAcctID<Field>(PXGraph graph, string AcctDefault, InventoryItem item, INSite site, INPostClass postclass, Vendor vendor)
            where Field : IBqlField
        {
            switch (AcctDefault)
            {
                case INAcctSubDefault.MaskItem:
                default:
                    {
                        PXCache cache = graph.Caches[typeof(InventoryItem)];
                        try
                        {
                            return (int?)cache.GetValue<Field>(item);
                        }
                        catch (NullReferenceException)
                        {
                            object keyval = cache.GetStateExt<InventoryItem.inventoryCD>(item);
                            throw new PXMaskArgumentException(Messages.MaskItem, PXUIFieldAttribute.GetDisplayName<Field>(cache), keyval);
                        }
                    }
                case INAcctSubDefault.MaskSite:
                    {
                        PXCache cache = graph.Caches[typeof(INSite)];
                        try
                        {
                            return (int?)cache.GetValue<Field>(site);
                        }
                        catch (NullReferenceException)
                        {
                            object keyval = cache.GetStateExt<INSite.siteCD>(site);
                            throw new PXMaskArgumentException(Messages.MaskSite, PXUIFieldAttribute.GetDisplayName<Field>(cache), keyval);
                        }
                    }
                case INAcctSubDefault.MaskClass:
                    {
                        PXCache cache = graph.Caches[typeof(INPostClass)];
                        try
                        {
                            return (int?)cache.GetValue<Field>(postclass);
                        }
                        catch (NullReferenceException)
                        {
                            object keyval = cache.GetStateExt<INPostClass.postClassID>(postclass);
                            throw new PXMaskArgumentException(Messages.MaskClass, PXUIFieldAttribute.GetDisplayName<Field>(cache), keyval);
                        }
                    }
                case INAcctSubDefault.MaskVendor:
                    {
                        PXCache cache = graph.Caches[typeof(Vendor)];
                        try
                        {
                            return (int?)cache.GetValue<Field>(vendor);
                        }
                        catch (NullReferenceException)
                        {
                            object keyval = cache.GetStateExt<Vendor.bAccountID>(vendor);
                            throw new PXMaskArgumentException(Messages.MaskVendor, PXUIFieldAttribute.GetDisplayName<Field>(cache), keyval);
                        }
                    }
            }
        }

        public static int? GetPOAccrualSubID<Field>(PXGraph graph, string AcctDefault, string SubMask, InventoryItem item, INSite site, INPostClass postclass, Vendor vendor)
            where Field : IBqlField
        {
            int? item_SubID = (int?)graph.Caches[typeof(InventoryItem)].GetValue<Field>(item);
            int? site_SubID = (int?)graph.Caches[typeof(INSite)].GetValue<Field>(site);
            int? class_SubID = (int?)graph.Caches[typeof(INPostClass)].GetValue<Field>(postclass);
            int? vendor_SubID = (int?)graph.Caches[typeof(Vendor)].GetValue<Field>(vendor);

            object value = null;

            try
            {
                value = POAccrualSubAccountMaskAttribute.MakeSub<INPostClass.pOAccrualSubMask>(graph, SubMask, new object[] { item_SubID, site_SubID, class_SubID, vendor_SubID }, new Type[] { typeof(InventoryItem.pOAccrualSubID), typeof(INSite.pOAccrualSubID), typeof(INPostClass.pOAccrualSubID), typeof(Vendor.pOAccrualSubID) });
            }
            catch (PXMaskArgumentException ex)
            {
                object keyval;
                switch (ex.SourceIdx)
                {
                    case 0:
                    default:
                        keyval = graph.Caches[typeof(InventoryItem)].GetStateExt<InventoryItem.inventoryCD>(item);
                        break;
                    case 1:
                        keyval = graph.Caches[typeof(INSite)].GetStateExt<INSite.siteCD>(site);
                        break;
                    case 2:
                        keyval = graph.Caches[typeof(INPostClass)].GetStateExt<INPostClass.postClassID>(postclass);
                        break;
                    case 3:
                        keyval = graph.Caches[typeof(Vendor)].GetStateExt<Vendor.bAccountID>(vendor);
                        break;
                }
                throw new PXMaskArgumentException(ex, keyval);
            }

            switch (AcctDefault)
            {
                case INAcctSubDefault.MaskItem:
                default:
                    RaiseFieldUpdating<Field>(graph.Caches[typeof(InventoryItem)], item, ref value);
                    break;
                case INAcctSubDefault.MaskSite:
                    RaiseFieldUpdating<Field>(graph.Caches[typeof(INSite)], site, ref value);
                    break;
                case INAcctSubDefault.MaskClass:
                    RaiseFieldUpdating<Field>(graph.Caches[typeof(INPostClass)], postclass, ref value);
                    break;
                case INAcctSubDefault.MaskVendor:
                    RaiseFieldUpdating<Field>(graph.Caches[typeof(Vendor)], vendor, ref value);
                    break;
            }
            return (int?)value;
        }

		public int? GetReasonCodeSubID<Field>(ReasonCode tranreasoncode, ReasonCode defreasoncode, InventoryItem item, INSite site, INPostClass postclass)
			where Field : IBqlField
		{
			ReasonCode reasoncode = (tranreasoncode.AccountID == null) ? defreasoncode : tranreasoncode;

			int? reasoncode_SubID = (int?)Caches[typeof(ReasonCode)].GetValue<ReasonCode.subID>(reasoncode);
			int? item_SubID = (int?)Caches[typeof(InventoryItem)].GetValue<Field>(item);
			int? site_SubID = (int?)Caches[typeof(INSite)].GetValue<Field>(site);
			int? class_SubID = (int?)Caches[typeof(INPostClass)].GetValue<Field>(postclass);

			object value = ReasonCodeSubAccountMaskAttribute.MakeSub<ReasonCode.subMask>(this, reasoncode.SubMask,
				new object[] { reasoncode_SubID, item_SubID, site_SubID, class_SubID },
				new Type[] { typeof(ReasonCode.subID), typeof(InventoryItem.reasonCodeSubID), typeof(INSite.reasonCodeSubID), typeof(INPostClass.reasonCodeSubID) });

			RaiseFieldUpdating<ReasonCode.subID>(Caches[typeof(ReasonCode)], reasoncode, ref value);
			return (int?)value;
		}

		public int? GetReasonCodeSubID<Field>(ReasonCode reasoncode, InventoryItem item, INSite site, INPostClass postclass)
			where Field : IBqlField
		{
			return GetReasonCodeSubID<Field>(this, reasoncode, item, site, postclass);
		}

		public static int? GetReasonCodeSubID<Field>(PXGraph graph, ReasonCode reasoncode, InventoryItem item, INSite site, INPostClass postclass)
			where Field : IBqlField
		{
			if (reasoncode.AccountID != null)
			{
				int? reasoncode_SubID = (int?)graph.Caches[typeof(ReasonCode)].GetValue<ReasonCode.subID>(reasoncode);
				int? item_SubID = (int?)graph.Caches[typeof(InventoryItem)].GetValue<Field>(item);
				int? site_SubID = (int?)graph.Caches[typeof(INSite)].GetValue<Field>(site);
				int? class_SubID = (int?)graph.Caches[typeof(INPostClass)].GetValue<Field>(postclass);

				object value = ReasonCodeSubAccountMaskAttribute.MakeSub<ReasonCode.subMask>(graph, reasoncode.SubMask,
					new object[] { reasoncode_SubID, item_SubID, site_SubID, class_SubID },
					new Type[] { typeof(ReasonCode.subID), typeof(InventoryItem.reasonCodeSubID), typeof(INSite.reasonCodeSubID), typeof(INPostClass.reasonCodeSubID) });

				RaiseFieldUpdating<ReasonCode.subID>(graph.Caches[typeof(ReasonCode)], reasoncode, ref value);
				return (int?)value;
			}
			return null;
		}

		public int? GetAccountDefaults<Field>(InventoryItem item, INSite site, INPostClass postclass)
			where Field : IBqlField
		{
			return GetAccountDefaults<Field>(this, item, site, postclass);
		}

		public static int? GetAccountDefaults<Field>(PXGraph graph, InventoryItem item, INSite site, INPostClass postclass)
			where Field : IBqlField
		{
			return GetAccountDefaults<Field>(graph, item, site, postclass, null);
		}

		public int? GetAccountDefaults<Field>(InventoryItem item, INSite site, INPostClass postclass, INTran tran)
			where Field : IBqlField
		{
			return GetAccountDefaults<Field>(this, item, site, postclass, tran);
		}

		public static int? GetAccountDefaults<Field>(PXGraph graph, InventoryItem item, INSite site, INPostClass postclass, INTran tran)
			where Field : IBqlField
		{
			if (typeof(Field) == typeof(INPostClass.invtAcctID))
				return GetAcctID<Field>(graph, item.StkItem != true && postclass.InvtAcctDefault == INAcctSubDefault.MaskSite ? INAcctSubDefault.MaskItem : postclass.InvtAcctDefault, item, site, postclass);
			if (typeof(Field) == typeof(INPostClass.invtSubID))
				return GetSubID<Field>(graph, postclass.InvtAcctDefault, postclass.InvtSubMask, item, site, postclass);
			if (typeof(Field) == typeof(INPostClass.cOGSAcctID))
				return GetAcctID<Field>(graph, item.StkItem != true && postclass.InvtAcctDefault == INAcctSubDefault.MaskSite ? INAcctSubDefault.MaskItem : postclass.InvtAcctDefault, item, site, postclass);
			if (typeof(Field) == typeof(INPostClass.cOGSSubID))
				return GetSubID<Field>(graph, postclass.COGSAcctDefault, postclass.COGSSubMask, item, site, postclass, tran);
			if (typeof(Field) == typeof(INPostClass.salesAcctID))
				return GetAcctID<Field>(graph, postclass.SalesAcctDefault, item, site, postclass);
			if (typeof(Field) == typeof(INPostClass.salesSubID))
				return GetSubID<Field>(graph, postclass.SalesAcctDefault, postclass.SalesSubMask, item, site, postclass);
			if (typeof(Field) == typeof(INPostClass.stdCstVarAcctID))
				return GetAcctID<Field>(graph, postclass.StdCstVarAcctDefault, item, site, postclass);
			if (typeof(Field) == typeof(INPostClass.stdCstVarSubID))
				return GetSubID<Field>(graph, postclass.StdCstVarAcctDefault, postclass.StdCstVarSubMask, item, site, postclass);
			if (typeof(Field) == typeof(INPostClass.stdCstRevAcctID))
				return GetAcctID<Field>(graph, postclass.StdCstRevAcctDefault, item, site, postclass);
			if (typeof(Field) == typeof(INPostClass.stdCstRevSubID))
				return GetSubID<Field>(graph, postclass.StdCstRevAcctDefault, postclass.StdCstRevSubMask, item, site, postclass);

			throw new PXException();
		}

		public virtual void UpdateItemSite(INTran tran, InventoryItem item, INSite site, ReasonCode reasoncode, INPostClass postclass)
		{
			if (item.StkItem == true)
			{
				INItemSite itemsite = new INItemSite();
				itemsite.InventoryID = tran.InventoryID;
				itemsite.SiteID = tran.SiteID;
				itemsite = initemsite.Locate(itemsite);

				if (itemsite == null)
				{
					itemsite = PXSelectReadonly<INItemSite, Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>, And<INItemSite.siteID, Equal<Required<INItemSite.inventoryID>>>>>.Select(this, tran.InventoryID, tran.SiteID);
				}

				if (itemsite == null)
				{
					itemsite = new INItemSite();
					itemsite.InventoryID = tran.InventoryID;
					itemsite.SiteID = tran.SiteID;
					INItemSiteMaint.DefaultItemSiteByItem(this, itemsite, item, site, postclass);
					itemsite = initemsite.Insert(itemsite);
				}

				if (tran.InvtAcctID == null)
				{
					tran.InvtAcctID = itemsite.InvtAcctID;
					tran.InvtSubID = itemsite.InvtSubID;
				}
			}
			else
			{
				switch (tran.TranType)
				{
					case INTranType.Receipt:
					case INTranType.Issue:
						if (tran.InvtAcctID == null)
						{
							tran.InvtAcctID = GetAccountDefaults<INPostClass.cOGSAcctID>(item, null, postclass);
							tran.InvtSubID = GetAccountDefaults<INPostClass.cOGSSubID>(item, null, postclass);
						}
						break;
					case INTranType.Invoice:
					case INTranType.DebitMemo:
					case INTranType.CreditMemo:
					case INTranType.Adjustment:
					case INTranType.Assembly:
						if (tran.InvtAcctID == null)
						{
							tran.InvtAcctID = GetAccountDefaults<INPostClass.invtAcctID>(item, null, postclass);
							tran.InvtSubID = GetAccountDefaults<INPostClass.invtSubID>(item, null, postclass);
						}
						break;
					default:
						throw new PXException(Messages.TranType_Invalid);
				}
			}

			switch (tran.TranType)
			{
				case INTranType.Receipt:
					if (tran.AcctID == null)
					{
						tran.AcctID = reasoncode.AccountID ?? ReceiptReasonCode.AccountID;
						tran.SubID = GetReasonCodeSubID<INPostClass.reasonCodeSubID>(reasoncode, ReceiptReasonCode, item, site, postclass);
						tran.ReasonCode = tran.ReasonCode ?? ReceiptReasonCode.ReasonCodeID;
					}
					if (tran.COGSAcctID != null)
					{
						tran.COGSAcctID = null;
						tran.COGSSubID = null;
					}
					break;
				case INTranType.Issue:
				case INTranType.Return:
					if (tran.AcctID != null)
					{
						tran.AcctID = null;
						tran.SubID = null;
					}
					if (tran.COGSAcctID == null)
					{
						//some crazy guys manage to setup ordertype so that it will create return in inventory and will specify non-inventory reason code
						if (reasoncode.Usage == ReasonCodeUsages.Issue || string.IsNullOrEmpty(reasoncode.Usage))
						{
							tran.COGSAcctID = reasoncode.AccountID ?? IssuesReasonCode.AccountID;
							tran.COGSSubID = GetReasonCodeSubID<INPostClass.reasonCodeSubID>(reasoncode, IssuesReasonCode, item, site, postclass);
							tran.ReasonCode = tran.ReasonCode ?? IssuesReasonCode.ReasonCodeID;
						}
						else
						{
							tran.COGSAcctID = GetAccountDefaults<INPostClass.cOGSAcctID>(item, site, postclass);
							tran.COGSSubID = GetAccountDefaults<INPostClass.cOGSSubID>(item, site, postclass);
						}
					}
					break;
				case INTranType.Invoice:
				case INTranType.DebitMemo:
				case INTranType.CreditMemo:
					if (tran.AcctID == null)
					{
						tran.AcctID = GetAccountDefaults<INPostClass.salesAcctID>(item, site, postclass);
						tran.SubID = GetAccountDefaults<INPostClass.salesSubID>(item, site, postclass);
					}
					if (tran.COGSAcctID == null)
					{
						if (reasoncode.Usage == ReasonCodeUsages.Issue)
						{
							tran.COGSAcctID = reasoncode.AccountID;
							tran.COGSSubID = GetReasonCodeSubID<INPostClass.reasonCodeSubID>(reasoncode, item, site, postclass);
						}
						else
						{
							tran.COGSAcctID = GetAccountDefaults<INPostClass.cOGSAcctID>(item, site, postclass);
							tran.COGSSubID = GetAccountDefaults<INPostClass.cOGSSubID>(item, site, postclass, (tran.InvtMult == 0 ? null : tran));
						}
					}
					break;
				case INTranType.Adjustment:
				case INTranType.StandardCostAdjustment:
				case INTranType.NegativeCostAdjustment:
					if (tran.AcctID == null)
					{
						tran.AcctID = reasoncode.AccountID ?? AdjustmentReasonCode.AccountID;
						tran.SubID = GetReasonCodeSubID<INPostClass.reasonCodeSubID>(reasoncode, AdjustmentReasonCode, item, site, postclass);
						tran.ReasonCode = tran.ReasonCode ?? AdjustmentReasonCode.ReasonCodeID;
					}
					if (tran.COGSAcctID == null && tran.InvtMult == (short)0)
					{
						if (item.ValMethod == INValMethod.Standard)
						{
							tran.COGSAcctID = GetAccountDefaults<INPostClass.stdCstVarAcctID>(item, site, postclass);
							tran.COGSSubID = GetAccountDefaults<INPostClass.stdCstVarSubID>(item, site, postclass);
						}
						else
						{
							tran.COGSAcctID = GetAccountDefaults<INPostClass.cOGSAcctID>(item, site, postclass);
							tran.COGSSubID = GetAccountDefaults<INPostClass.cOGSSubID>(item, site, postclass, tran);
						}
					}
					if (tran.COGSAcctID != null && tran.InvtMult == (short)1)
					{
						tran.COGSAcctID = null;
						tran.COGSSubID = null;
					}
					break;
				case INTranType.Transfer:
					if (tran.AcctID == null)
					{
						tran.AcctID = INTransitAcctID;
						tran.SubID = INTransitSubID;
					}
					if (tran.COGSAcctID != null)
					{
						tran.COGSAcctID = null;
						tran.COGSSubID = null;
					}
					break;
				case INTranType.Assembly:
					if (tran.AcctID == null)
					{
						tran.AcctID = INProgressAcctID;
						tran.SubID = INProgressSubID;
					}
					if (tran.COGSAcctID != null)
					{
						tran.COGSAcctID = null;
						tran.COGSSubID = null;
					}
					break;
				default:
					throw new PXException(Messages.TranType_Invalid);
			}
		}

        private void SegregateBatch(JournalEntry je, int? branchID, DateTime? docDate, string finPeriodID, string description)
        {
            je.created.Consolidate = PXAccess.FeatureInstalled<FeaturesSet.consolidatedPosting>();
			je.Segregate(BatchModule.IN, branchID, this.BaseCuryID, docDate, finPeriodID, description, null, null);
		}

		public virtual void WriteGLSales(JournalEntry je, INTran intran)
		{
			if (UpdateGL && intran.SalesMult != null && string.IsNullOrEmpty(intran.SOOrderNbr) && string.IsNullOrEmpty(intran.ARRefNbr))
			{
				{
					GLTran tran = new GLTran();
					tran.SummPost = this.SummPost;
					tran.BranchID = intran.BranchID;
					tran.AccountID = ARClearingAcctID;
					tran.SubID = ARClearingSubID;

					tran.CuryDebitAmt = (intran.SalesMult == (short)1) ? intran.TranAmt : 0m;
					tran.DebitAmt = (intran.SalesMult == (short)1) ? intran.TranAmt : 0m;
					tran.CuryCreditAmt = (intran.SalesMult == (short)1) ? 0m : intran.TranAmt;
					tran.CreditAmt = (intran.SalesMult == (short)1) ? 0m : intran.TranAmt;

					tran.TranType = intran.TranType;
					tran.TranClass = "N";
					tran.RefNbr = intran.RefNbr;
					tran.InventoryID = intran.InventoryID;
					tran.Qty = (intran.SalesMult == (short)1) ? intran.Qty : -intran.Qty;
					tran.UOM = intran.UOM;
					tran.TranDesc = intran.TranDesc;
					tran.TranDate = intran.TranDate;
					tran.TranPeriodID = intran.TranPeriodID;
					tran.FinPeriodID = intran.FinPeriodID;
					tran.ProjectID = intran.ProjectID;
					tran.TaskID = intran.TaskID;
					tran.Released = true;

					je.GLTranModuleBatNbr.Insert(tran);
				}

				{
					GLTran tran = new GLTran();
					tran.SummPost = this.SummPost;
					tran.BranchID = intran.BranchID;
					tran.AccountID = intran.AcctID;
					tran.SubID = GetValueInt<INTran.subID>(je, intran);

					tran.CuryDebitAmt = (intran.SalesMult == (short)1) ? 0m : intran.TranAmt;
					tran.DebitAmt = (intran.SalesMult == (short)1) ? 0m : intran.TranAmt;
					tran.CuryCreditAmt = (intran.SalesMult == (short)1) ? intran.TranAmt : 0m;
					tran.CreditAmt = (intran.SalesMult == (short)1) ? intran.TranAmt : 0m;

					tran.TranType = intran.TranType;
					tran.TranClass = "N";
					tran.RefNbr = intran.RefNbr;
					tran.InventoryID = intran.InventoryID;
					tran.Qty = (intran.SalesMult == (short)1) ? -intran.Qty : intran.Qty;
					tran.UOM = intran.UOM;
					tran.TranDesc = intran.TranDesc;
					tran.TranDate = intran.TranDate;
					tran.TranPeriodID = intran.TranPeriodID;
					tran.FinPeriodID = intran.FinPeriodID;
					tran.ProjectID = intran.ProjectID;
					tran.TaskID = intran.TaskID;
					tran.Released = true;

					je.GLTranModuleBatNbr.Insert(tran);
				}
			}
		}

		public int? GetValueInt<SourceField>(PXGraph target, object item)
			where SourceField : IBqlField
		{
			PXCache source = this.Caches[BqlCommand.GetItemType(typeof(SourceField))];
			PXCache dest = target.Caches[BqlCommand.GetItemType(typeof(SourceField))];

			object value = source.GetValueExt<SourceField>(item);
			if (value is PXFieldState)
			{
				value = ((PXFieldState)value).Value;
			}

			if (value != null)
			{
				dest.RaiseFieldUpdating<SourceField>(item, ref value);
			}

			return (int?)value;
		}

		public static void RaiseFieldUpdating<Field>(PXCache cache, object item, ref object value)
			where Field : IBqlField
		{
			try
			{
				cache.RaiseFieldUpdating<Field>(item, ref value);
			}
			catch (PXSetPropertyException ex)
			{
				string fieldname = typeof(Field).Name;
				string itemname = PXUIFieldAttribute.GetItemName(cache);
				string dispname = PXUIFieldAttribute.GetDisplayName(cache, fieldname);
				string errortext = ex.Message;

				if (dispname != null && fieldname != dispname)
				{
					int fid = errortext.IndexOf(fieldname, StringComparison.OrdinalIgnoreCase);
					if (fid >= 0)
					{
						errortext = errortext.Remove(fid, fieldname.Length).Insert(fid, dispname);
					}
				}
				else
				{
					dispname = fieldname;
				}

				dispname = string.Format("{0} {1}", itemname, dispname);

				throw new PXSetPropertyException(ErrorMessages.ValueDoesntExist, dispname, value);
			}
		}

		public virtual void WriteGLNonStockCosts(JournalEntry je, INTran intran, InventoryItem item, INSite site)
		{
			if (item.StkItem == false && (intran.COGSAcctID != null || intran.AcctID != null))
			{
				GLTran tran = new GLTran();
				tran.SummPost = this.SummPost;
				tran.BranchID = intran.BranchID;
				tran.AccountID = (intran.InvtMult == (short)0) ? intran.AcctID : intran.InvtAcctID;
				tran.SubID = (intran.InvtMult == (short)0) ? intran.SubID : GetValueInt<INTran.invtSubID>(je, intran);

				tran.CuryDebitAmt = (intran.InvtMult == (short)1) ? intran.TranCost : 0m;
				tran.DebitAmt = (intran.InvtMult == (short)1) ? intran.TranCost : 0m;
				tran.CuryCreditAmt = (intran.InvtMult == (short)1) ? 0m : intran.TranCost;
				tran.CreditAmt = (intran.InvtMult == (short)1) ? 0m : intran.TranCost;

				tran.TranType = intran.TranType;
				tran.TranClass = "N";
				tran.RefNbr = intran.RefNbr;
				tran.InventoryID = intran.InventoryID;
				tran.Qty = (intran.InvtMult == (short)1) ? intran.Qty : -intran.Qty;
				tran.UOM = intran.UOM;
				tran.TranDesc = intran.TranDesc;
				tran.TranDate = intran.TranDate;
				tran.TranPeriodID = intran.TranPeriodID;
				tran.FinPeriodID = intran.FinPeriodID;
				tran.ProjectID = (intran.InvtMult == (short)0) ? PM.ProjectDefaultAttribute.NonProject(this) : intran.ProjectID;
				tran.TaskID = (intran.InvtMult == (short)0) ? null : intran.TaskID;
				tran.Released = true;

				je.GLTranModuleBatNbr.Insert(tran);
			}

			if (item.StkItem == false && (intran.COGSAcctID != null || intran.AcctID != null))
			{
				GLTran tran = new GLTran();
				tran.SummPost = this.SummPost;
				tran.BranchID = intran.BranchID;
				tran.AccountID = (intran.COGSAcctID ?? intran.AcctID);
				tran.SubID = (GetValueInt<INTran.cOGSSubID>(je, intran) ?? GetValueInt<INTran.subID>(je, intran));

				tran.CuryDebitAmt = (intran.InvtMult == (short)1) ? 0m : intran.TranCost;
				tran.DebitAmt = (intran.InvtMult == (short)1) ? 0m : intran.TranCost;
				tran.CuryCreditAmt = (intran.InvtMult == (short)1) ? intran.TranCost : 0m;
				tran.CreditAmt = (intran.InvtMult == (short)1) ? intran.TranCost : 0m;

				tran.TranType = intran.TranType;
				tran.TranClass = "N";
				tran.RefNbr = intran.RefNbr;
				tran.InventoryID = intran.InventoryID;
				tran.Qty = (intran.InvtMult == (short)1) ? -intran.Qty : intran.Qty;
				tran.UOM = intran.UOM;
				tran.TranDesc = intran.TranDesc;
				tran.TranDate = intran.TranDate;
				tran.TranPeriodID = intran.TranPeriodID;
				tran.FinPeriodID = intran.FinPeriodID;
				tran.ProjectID = (intran.InvtMult == (short)0) ? intran.ProjectID : PM.ProjectDefaultAttribute.NonProject(this);
				tran.TaskID = (intran.InvtMult == (short)0) ? intran.TaskID : null;
				tran.Released = true;

				je.GLTranModuleBatNbr.Insert(tran);
			}
		}

		public virtual void WriteGLCosts(JournalEntry je, INTranCost trancost, INTran intran, InventoryItem item, INSite site, INPostClass postclass, ReasonCode reasoncode)
		{
			if (trancost.COGSAcctID != null || intran.AcctID != null)
			{
				GLTran tran = new GLTran();
				tran.SummPost = trancost.TranType == INTranType.Transfer && intran.DocType == trancost.CostDocType ? true : this.SummPost;
				tran.BranchID = (trancost.InvtMult == (short)0) ? intran.BranchID : site.BranchID;
				tran.AccountID = (trancost.InvtMult == (short)0) ? intran.AcctID : trancost.InvtAcctID;
				tran.SubID = (trancost.InvtMult == (short)0) ? intran.SubID : GetValueInt<INTranCost.invtSubID>(je, trancost);

				tran.CuryDebitAmt = (trancost.InvtMult == (short)1) ? trancost.TranCost : 0m;
				tran.DebitAmt = (trancost.InvtMult == (short)1) ? trancost.TranCost : 0m;
				tran.CuryCreditAmt = (trancost.InvtMult == (short)1) ? 0m : trancost.TranCost;
				tran.CreditAmt = (trancost.InvtMult == (short)1) ? 0m : trancost.TranCost;

				tran.TranType = trancost.TranType;
				tran.TranClass = "N";
				tran.RefNbr = trancost.RefNbr;
				tran.InventoryID = trancost.InventoryID;
				tran.Qty = (trancost.InvtMult == (short)1) ?  trancost.Qty : -trancost.Qty;
				tran.UOM = item.BaseUnit;
				tran.TranDesc = intran.TranDesc;
				tran.TranDate = intran.TranDate;
				tran.TranPeriodID = intran.TranPeriodID;
				tran.FinPeriodID = intran.FinPeriodID;
				tran.ProjectID = (trancost.InvtMult == (short)0) ? intran.ProjectID : PM.ProjectDefaultAttribute.NonProject(this);
				tran.TaskID = (trancost.InvtMult == (short)0) ? intran.TaskID : null;
				tran.Released = true;

				je.GLTranModuleBatNbr.Insert(tran);
			}

			if (item.ValMethod == INValMethod.Standard && (trancost.COGSAcctID != null || intran.AcctID != null))
			{
				GLTran tran = new GLTran();
				tran.SummPost = this.SummPost;
				tran.BranchID = intran.BranchID;
				tran.AccountID = GetAccountDefaults<INPostClass.stdCstVarAcctID>(item, site, postclass);
				tran.SubID = GetAccountDefaults<INPostClass.stdCstVarSubID>(je, item, site, postclass);

				tran.CuryDebitAmt = (trancost.InvtMult == (short)1) ? trancost.VarCost : 0m;
				tran.DebitAmt = (trancost.InvtMult == (short)1) ? trancost.VarCost : 0m;
				tran.CuryCreditAmt = (trancost.InvtMult == (short)1) ? 0m : trancost.VarCost;
				tran.CreditAmt = (trancost.InvtMult == (short)1) ? 0m : trancost.VarCost;

				tran.TranType = trancost.TranType;
				tran.TranClass = "N";
				tran.RefNbr = trancost.RefNbr;
				tran.InventoryID = trancost.InventoryID;
				tran.Qty = (trancost.InvtMult == (short)1) ? trancost.Qty : -trancost.Qty;
				tran.UOM = item.BaseUnit;
				tran.TranDesc = intran.TranDesc;
				tran.TranDate = intran.TranDate;
				tran.TranPeriodID = intran.TranPeriodID;
				tran.FinPeriodID = intran.FinPeriodID;
				tran.ProjectID = intran.ProjectID;
				tran.TaskID = intran.TaskID;
				tran.Released = true;

				je.GLTranModuleBatNbr.Insert(tran);
			}

			if (trancost.COGSAcctID != null || intran.AcctID != null)
			{
				//oversold transfers go to COGS instead of GIT
				if (trancost.TranType == INTranType.Transfer && (trancost.CostDocType != intran.DocType || trancost.CostRefNbr != intran.RefNbr))
				{
					trancost.COGSAcctID = GetAccountDefaults<INPostClass.cOGSAcctID>(item, site, postclass);
					trancost.COGSSubID = GetAccountDefaults<INPostClass.cOGSSubID>(je, item, site, postclass, null);
				}
				//oversold Assemblies go to Variance instead of WIP
				if (trancost.TranType == INTranType.Assembly && reasoncode != null && reasoncode.AccountID != null && (trancost.CostDocType != intran.DocType || trancost.CostRefNbr != intran.RefNbr))
				{
					trancost.COGSAcctID = reasoncode.AccountID;
					trancost.COGSSubID = GetReasonCodeSubID<INPostClass.reasonCodeSubID>(je, reasoncode, item, site, postclass);
				}

				GLTran tran = new GLTran();
				tran.SummPost = (trancost.TranType == INTranType.Transfer || trancost.TranType == INTranType.Assembly) && intran.DocType == trancost.CostDocType ? true : this.SummPost;
				tran.BranchID = trancost.COGSAcctID == null ? intran.OrigBranchID ?? intran.BranchID : intran.BranchID;
				tran.AccountID = (trancost.COGSAcctID ?? intran.AcctID);
				tran.SubID = (GetValueInt<INTranCost.cOGSSubID>(je, trancost) ?? GetValueInt<INTran.subID>(je, intran));

                tran.CuryDebitAmt = (trancost.InvtMult == (short)1) ? 0m : trancost.TranCost + (item.ValMethod == INValMethod.Standard ? trancost.VarCost : 0m);
                tran.DebitAmt = (trancost.InvtMult == (short)1) ? 0m : trancost.TranCost + (item.ValMethod == INValMethod.Standard ? trancost.VarCost : 0m);
                tran.CuryCreditAmt = (trancost.InvtMult == (short)1) ? trancost.TranCost + (item.ValMethod == INValMethod.Standard ? trancost.VarCost : 0m) : 0m;
                tran.CreditAmt = (trancost.InvtMult == (short)1) ? trancost.TranCost + (item.ValMethod == INValMethod.Standard ? trancost.VarCost : 0m) : 0m;

				tran.TranType = trancost.TranType;
				tran.TranClass = "N";
				tran.RefNbr = trancost.RefNbr;
				tran.InventoryID = trancost.InventoryID;
				tran.Qty = (trancost.InvtMult == (short)1) ? -trancost.Qty : trancost.Qty;
				tran.UOM = item.BaseUnit;
				tran.TranDesc = intran.TranDesc;
				tran.TranDate = intran.TranDate;
				tran.TranPeriodID = intran.TranPeriodID;
				tran.FinPeriodID = intran.FinPeriodID;
				tran.ProjectID = intran.ProjectID;
				tran.TaskID = intran.TaskID;
				tran.Released = true;
				tran.TranLineNbr = (tran.SummPost == true) ? null : intran.LineNbr;

				je.GLTranModuleBatNbr.Insert(tran);
			}

			//Write off production variance from WIP
			if (WIPCalculated && intran.AssyType == INAssyType.KitTran && string.Equals(trancost.CostDocType, intran.DocType) && string.Equals(trancost.CostRefNbr, intran.RefNbr))
			{
				GLTran tran = new GLTran();
				tran.SummPost = true;
				tran.BranchID = intran.BranchID;
				tran.AccountID = INProgressAcctID;
				tran.SubID = INProgressSubID;

				tran.CuryDebitAmt = 0m;
				tran.DebitAmt = 0m;
				tran.CuryCreditAmt = WIPVariance;
				tran.CreditAmt = WIPVariance;

				tran.TranType = intran.TranType;
				tran.TranClass = "N";
				tran.RefNbr = intran.RefNbr;
				tran.InventoryID = intran.InventoryID;
				tran.TranDesc = PXMessages.LocalizeNoPrefix(Messages.ProductionVarianceTranDesc);
				tran.TranDate = intran.TranDate;
				tran.TranPeriodID = intran.TranPeriodID;
				tran.FinPeriodID = intran.FinPeriodID;
				tran.ProjectID = intran.ProjectID;
				tran.TaskID = intran.TaskID;
				tran.Released = true;
				tran.TranLineNbr = null;

				je.GLTranModuleBatNbr.Insert(tran);
			}

			if (WIPCalculated && intran.AssyType == INAssyType.KitTran && string.Equals(trancost.CostDocType, intran.DocType) && string.Equals(trancost.CostRefNbr, intran.RefNbr))
			{
				GLTran tran = new GLTran();
				tran.SummPost = this.SummPost;
				tran.BranchID = intran.BranchID;
				tran.AccountID = reasoncode.AccountID;
				tran.SubID = GetReasonCodeSubID<INPostClass.reasonCodeSubID>(je, reasoncode, item, site, postclass);

				tran.CuryDebitAmt = WIPVariance;
				tran.DebitAmt = WIPVariance;
				tran.CuryCreditAmt = 0m;
				tran.CreditAmt = 0m;

				tran.TranType = INTranType.Assembly;
				tran.TranClass = "N";
				tran.RefNbr = intran.RefNbr;
				tran.InventoryID = intran.InventoryID;
				tran.TranDesc = PXMessages.LocalizeNoPrefix(Messages.ProductionVarianceTranDesc); 
				tran.TranDate = intran.TranDate;
				tran.TranPeriodID = intran.TranPeriodID;
				tran.FinPeriodID = intran.FinPeriodID;
				tran.ProjectID = intran.ProjectID;
				tran.TaskID = intran.TaskID;
				tran.Released = true;
				tran.TranLineNbr = null;

				je.GLTranModuleBatNbr.Insert(tran);

				WIPCalculated = false;
				WIPVariance = 0m;
			}
		}

		public object GetValueExt<Field>(PXCache cache, object data)
			where Field : class, IBqlField
		{
			object val = cache.GetValueExt<Field>(data);

			if (val is PXFieldState)
			{
				return ((PXFieldState)val).Value;
			}
			else
			{
				return val;
			}
		}

		List<Segment> _SubItemSeg = null;
		Dictionary<short?, string> _SubItemSegVal = null;

		public virtual void ParseSubItemSegKeys()
		{
			if (_SubItemSeg == null)
			{
				_SubItemSeg = new List<Segment>();

				foreach (Segment seg in PXSelect<Segment, Where<Segment.dimensionID, Equal<IN.SubItemAttribute.dimensionName>>>.Select(this))
				{
					_SubItemSeg.Add(seg);
				}

				_SubItemSegVal = new Dictionary<short?, string>();

				foreach (SegmentValue val in PXSelectJoin<SegmentValue, InnerJoin<Segment, On<Segment.dimensionID, Equal<SegmentValue.dimensionID>, And<Segment.segmentID, Equal<SegmentValue.segmentID>>>>, Where<SegmentValue.dimensionID, Equal<IN.SubItemAttribute.dimensionName>, And<Segment.isCosted, Equal<boolFalse>, And<SegmentValue.isConsolidatedValue, Equal<boolTrue>>>>>.Select(this))
				{
					_SubItemSegVal.Add((short)val.SegmentID, val.Value);
				}
			}
		}

		public virtual string MakeCostSubItemCD(string SubItemCD)
		{
			StringBuilder sb = new StringBuilder();

			int offset = 0;

			foreach (Segment seg in _SubItemSeg)
			{
				string segval = SubItemCD.Substring(offset, (int)seg.Length);
				if (seg.IsCosted == true || segval.TrimEnd() == string.Empty)
				{
					sb.Append(segval);
				}
				else
				{
					if (_SubItemSegVal.TryGetValue(seg.SegmentID, out segval))
					{
						sb.Append(segval);
					}
					else
					{
						throw new PXException(Messages.SubItemSeg_Missing_ConsolidatedVal);
					}
				}
				offset += (int)seg.Length;
			}

			return sb.ToString();
		}

		public virtual void UpdateCrossReference(INTranSplit split, InventoryItem item, INLocation whseloc)
		{
			if (item.ValMethod != INValMethod.Standard && item.ValMethod != INValMethod.Specific && whseloc == null)
			{
				throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<INTran.locationID>(intranselect.Cache));
			}

			if (split.SubItemID == null)
			{
				throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<INTran.subItemID>(intranselect.Cache));
			}

			INCostSubItemXRef xref = new INCostSubItemXRef();

			xref.SubItemID = split.SubItemID;
			xref.CostSubItemID = split.SubItemID;

			string SubItemCD = (string)this.GetValueExt<INCostSubItemXRef.costSubItemID>(costsubitemxref.Cache, xref);

			xref.CostSubItemID = null;

			string CostSubItemCD = PXAccess.FeatureInstalled<FeaturesSet.subItem>() ? MakeCostSubItemCD(SubItemCD) : SubItemCD;

			costsubitemxref.Cache.SetValueExt<INCostSubItemXRef.costSubItemID>(xref, CostSubItemCD);
			xref = costsubitemxref.Update(xref);

			if (costsubitemxref.Cache.GetStatus(xref) == PXEntryStatus.Updated)
			{
				costsubitemxref.Cache.SetStatus(xref, PXEntryStatus.Notchanged);
			}

			split.CostSubItemID = xref.CostSubItemID;
			//Standard & Specific Cost items will ignore per location costing, Standard can have null location
			split.CostSiteID = (item.ValMethod != INValMethod.Standard && item.ValMethod != INValMethod.Specific && whseloc.IsCosted == true ? whseloc.LocationID : split.SiteID);
		}

		public virtual void ReleaseDocProc(JournalEntry je, INRegister doc)
		{
			if ((bool)doc.Hold)
			{
				throw new PXException(Messages.Document_OnHold_CannotRelease);
			}

			//planning requires document context.
			inregister.Current = doc;
			//mark as updated so that doc will not expire from cache, and totalcost will not be overwritten with old value
			inregister.Cache.SetStatus(doc, PXEntryStatus.Updated);

			INItemPlanIDAttribute.SetReleaseMode<INTranSplit.planID>(intransplit.Cache, true);

			using (new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					SegregateBatch(je, doc.BranchID, doc.TranDate, doc.FinPeriodID, doc.TranDesc);

					INTran prev_tran = null;
					int? prev_linenbr = null;

					foreach (PXResult<INTran, INTranSplit, INItemPlan, INSite> res in PXSelectJoin<INTran,
						InnerJoin<INTranSplit, On<INTranSplit.tranType, Equal<INTran.tranType>, And<INTranSplit.refNbr, Equal<INTran.refNbr>, And<INTranSplit.lineNbr, Equal<INTran.lineNbr>>>>,
						InnerJoin<INItemPlan, On<INItemPlan.planID, Equal<INTranSplit.planID>, And<INItemPlan.planType, Equal<INPlanConstants.plan40>>>,
						InnerJoin<INSite, On<INSite.siteID, Equal<INTran.toSiteID>>>>>,
						Where<INTran.docType, Equal<Required<INTran.docType>>, And<INTran.refNbr, Equal<Required<INTran.refNbr>>, And<INTran.docType, Equal<INDocType.transfer>, And<INTran.invtMult, Equal<shortMinus1>>>>>, OrderBy<Asc<INTran.tranType, Asc<INTran.refNbr, Asc<INTran.lineNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
					{
						INTran tran = res;
						INTranSplit split = res;
						INSite site = res;

						if (object.Equals(prev_tran, tran) == false)
						{
							INTran newtran = PXCache<INTran>.CreateCopy(tran);
							newtran.OrigBranchID = newtran.BranchID;
							newtran.OrigTranType = newtran.TranType;
							newtran.OrigRefNbr = newtran.RefNbr;
							newtran.OrigLineNbr = newtran.LineNbr;
							newtran.BranchID = site.BranchID;
							newtran.LineNbr = (int)PXLineNbrAttribute.NewLineNbr<INTran.lineNbr>(intranselect.Cache, doc);
							newtran.InvtMult = (short)1;
							newtran.SiteID = newtran.ToSiteID;
							newtran.LocationID = newtran.ToLocationID;
							newtran.ToSiteID = null;
							newtran.ToLocationID = null;
							newtran.InvtAcctID = null;
							newtran.InvtSubID = null;
							newtran.ARDocType = null;
							newtran.ARRefNbr = null;
							newtran.ARLineNbr = null;

							newtran = intranselect.Insert(newtran);
							//persist now for join right part 
							//intranselect.Cache.PersistInserted(newtran);

							prev_tran = tran;
							prev_linenbr = newtran.LineNbr;
						}

						INTranSplit newsplit = PXCache<INTranSplit>.CreateCopy(split);
						newsplit.LineNbr = prev_linenbr;
						newsplit.SplitLineNbr = (int)PXLineNbrAttribute.NewLineNbr<INTranSplit.splitLineNbr>(intransplit.Cache, doc);
						newsplit.InvtMult = (short)1;
						newsplit.SiteID = tran.ToSiteID;
						newsplit.LocationID = tran.ToLocationID;
						newsplit.PlanID = null;

						newsplit = intransplit.Insert(newsplit);
						//persist now for join right part 
						//intransplit.Cache.PersistInserted(newsplit);
					}

					foreach (PXResult<INTran, InventoryItem, INLocation, INLotSerClass> res in PXSelectJoin<INTran,
						InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INTran.inventoryID>>,
						LeftJoin<INLocation, On<INLocation.locationID, Equal<INTran.locationID>>,
						InnerJoin<INLotSerClass, On<INLotSerClass.lotSerClassID, Equal<InventoryItem.lotSerClassID>>>>>,
						Where<INTran.docType, Equal<Required<INTran.docType>>, And<INTran.refNbr, Equal<Required<INTran.refNbr>>, And<INTran.tranType, Equal<INTranType.receiptCostAdjustment>>>>,
						OrderBy<Asc<INTran.tranType, Asc<INTran.refNbr, Asc<INTran.lineNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
					{
						INTran tran = (INTran)res;
						InventoryItem item = (InventoryItem)res;
						INLotSerClass lotserclass = (INLotSerClass)res;
						INLocation whseloc = (INLocation)res;
						INTranSplit split = (INTranSplit)tran;

						UpdateCrossReference(split, item, whseloc);

						decimal? accu_TranCost = 0m;
						decimal? accu_Qty = 0m;
						ReadOnlyCostStatus prev_layer = null;

						//fifo, average, specific
						foreach (PXResult<ReadOnlyCostStatus, ReceiptStatus> layerres in PXSelectJoin<ReadOnlyCostStatus,
							LeftJoin<ReceiptStatus, On<ReceiptStatus.inventoryID, Equal<ReadOnlyCostStatus.inventoryID>, And<ReceiptStatus.costSubItemID, Equal<ReadOnlyCostStatus.costSubItemID>, And<ReceiptStatus.costSiteID, Equal<ReadOnlyCostStatus.costSiteID>>>>>,
							Where<ReadOnlyCostStatus.inventoryID, Equal<Current<INTranSplit.inventoryID>>, And<ReadOnlyCostStatus.costSubItemID, Equal<Current<INTranSplit.costSubItemID>>, And<ReadOnlyCostStatus.costSiteID, Equal<Current<INTranSplit.costSiteID>>,
							//FIFO
								And<Where<ReadOnlyCostStatus.receiptNbr, Equal<Current<INTran.origRefNbr>>, And<ReceiptStatus.receiptNbr, IsNull,
							//Specific
									Or<ReadOnlyCostStatus.receiptNbr, Equal<Current<INTran.origRefNbr>>, And<ReceiptStatus.receiptNbr, Equal<Current<INTran.origRefNbr>>,
							//Average
									Or<ReadOnlyCostStatus.receiptNbr, Equal<INLayerRef.zzz>, And<ReceiptStatus.receiptNbr, Equal<Current<INTran.origRefNbr>>>>>>>>>>>>>
							.SelectMultiBound(this, new object[] { tran, split }))
						{
							ReadOnlyCostStatus layer = (ReadOnlyCostStatus)layerres;
							ReceiptStatus receipt = (ReceiptStatus)layerres;

							decimal? tran_TranCost = tran.TranCost;

							if (layer.ValMethod == INValMethod.Average)
							{
								layer.OrigQty = receipt.OrigQty;
							}
							else if (layer.ValMethod == INValMethod.Specific)
							{
								tran_TranCost = tran.TranCost / receipt.OrigQty;
							}

							if (layer.QtyOnHand > 0m)
							{
								prev_layer = layer;
							}

							if (layer.QtyOnHand < layer.OrigQty && layer.OrigQty != 0m)
							{
								//inventory adjustment
								if (layer.QtyOnHand != 0m)
								{
									INTran newtran = new INTran();
									newtran.BranchID = tran.BranchID;
									newtran.DocType = tran.DocType;
									newtran.RefNbr = tran.RefNbr;
									newtran.TranType = INTranType.Adjustment;
									newtran.InventoryID = tran.InventoryID;
									newtran.SubItemID = tran.SubItemID;
									newtran.SiteID = tran.SiteID;
									newtran.LocationID = tran.LocationID;
									newtran.UOM = tran.UOM;
									newtran.Qty = 0m;
									newtran.InvtMult = (short)1;
									newtran.TranCost = PXDBCurrencyAttribute.BaseRound(this, (decimal)(layer.QtyOnHand * tran_TranCost / layer.OrigQty));
									newtran.AcctID = tran.AcctID;
									newtran.SubID = tran.SubID;
									newtran.COGSAcctID = tran.COGSAcctID;
									newtran.COGSSubID = tran.COGSSubID;
									newtran.InvtAcctID = layer.AccountID;
									newtran.InvtSubID = layer.SubID;
									newtran.OrigRefNbr = (item.ValMethod == INValMethod.FIFO || item.ValMethod == INValMethod.Specific) ? layer.ReceiptNbr : null;
									newtran.LotSerialNbr = (item.ValMethod == INValMethod.Specific) ? layer.LotSerialNbr : string.Empty;
									newtran.POReceiptNbr = tran.POReceiptNbr;
									newtran.POReceiptLineNbr = tran.POReceiptLineNbr;
									newtran.ReasonCode = tran.ReasonCode;

									if (item.ValMethod == INValMethod.Specific)
									{
										decimal diff;
										if ((diff = PXCurrencyAttribute.BaseRound(this, (accu_Qty + layer.OrigQty) * tran.TranCost / receipt.OrigQty - accu_TranCost - newtran.TranCost)) != 0m)
										{
											newtran.TranCost += diff;
										}
									}

									accu_TranCost += newtran.TranCost;

									intranselect.Insert(newtran);
								}

								//cogs adjustment
								{
									INTran newtran = new INTran();
									newtran.BranchID = tran.BranchID;
									newtran.DocType = tran.DocType;
									newtran.RefNbr = tran.RefNbr;
									newtran.TranType = INTranType.Adjustment;
									newtran.InventoryID = tran.InventoryID;
									newtran.SubItemID = tran.SubItemID;
									newtran.SiteID = tran.SiteID;
									newtran.LocationID = tran.LocationID;
									newtran.UOM = tran.UOM;
									newtran.Qty = 0m;
									newtran.InvtMult = (short)0;
									newtran.TranCost = PXDBCurrencyAttribute.BaseRound(this, (decimal)((layer.OrigQty - layer.QtyOnHand) * tran_TranCost / layer.OrigQty));
									newtran.AcctID = tran.AcctID;
									newtran.SubID = tran.SubID;
									newtran.COGSAcctID = tran.COGSAcctID;
									newtran.COGSSubID = tran.COGSSubID;
									newtran.InvtAcctID = layer.AccountID;
									newtran.InvtSubID = layer.SubID;
									newtran.OrigRefNbr = (item.ValMethod == INValMethod.FIFO || item.ValMethod == INValMethod.Specific) ? layer.ReceiptNbr : null;
									newtran.LotSerialNbr = (item.ValMethod == INValMethod.Specific) ? layer.LotSerialNbr : string.Empty;
									newtran.POReceiptNbr = tran.POReceiptNbr;
									newtran.POReceiptLineNbr = tran.POReceiptLineNbr;
									newtran.ReasonCode = tran.ReasonCode;

									accu_TranCost += newtran.TranCost;

									intranselect.Insert(newtran);
								}
							}
							else
							{
								//inventory adjustment only
								{
									INTran newtran = new INTran();
									newtran.BranchID = tran.BranchID;
									newtran.DocType = tran.DocType;
									newtran.RefNbr = tran.RefNbr;
									newtran.TranType = INTranType.Adjustment;
									newtran.InventoryID = tran.InventoryID;
									newtran.SubItemID = tran.SubItemID;
									newtran.SiteID = tran.SiteID;
									newtran.LocationID = tran.LocationID;
									newtran.UOM = tran.UOM;
									newtran.Qty = 0m;
									newtran.InvtMult = (short)1;
									newtran.TranCost = PXDBCurrencyAttribute.BaseRound(this, (decimal)tran_TranCost);
									newtran.AcctID = tran.AcctID;
									newtran.SubID = tran.SubID;
									newtran.COGSAcctID = tran.COGSAcctID;
									newtran.COGSSubID = tran.COGSSubID;
									newtran.InvtAcctID = layer.AccountID;
									newtran.InvtSubID = layer.SubID;
									newtran.OrigRefNbr = (item.ValMethod == INValMethod.FIFO || item.ValMethod == INValMethod.Specific) ? layer.ReceiptNbr : null;
									newtran.LotSerialNbr = (item.ValMethod == INValMethod.Specific) ? layer.LotSerialNbr : string.Empty;
									newtran.POReceiptNbr = tran.POReceiptNbr;
									newtran.POReceiptLineNbr = tran.POReceiptLineNbr;
									newtran.ReasonCode = tran.ReasonCode;

									if (item.ValMethod == INValMethod.Specific)
									{
										decimal diff;
										if ((diff = PXCurrencyAttribute.BaseRound(this, (accu_Qty + layer.OrigQty) * tran.TranCost / receipt.OrigQty - accu_TranCost - newtran.TranCost)) != 0m)
										{
											newtran.TranCost += diff;
										}
									}

									accu_TranCost += newtran.TranCost;

									intranselect.Insert(newtran);
								}
							}

							accu_Qty += layer.OrigQty;
						}

						//Standard, Specific rounding
						if (tran.TranCost - accu_TranCost != 0m)
						{
							//cogs adjustment
							{
								INTran newtran = new INTran();
								newtran.BranchID = tran.BranchID;
								newtran.DocType = tran.DocType;
								newtran.RefNbr = tran.RefNbr;
								newtran.TranType = INTranType.Adjustment;
								newtran.InventoryID = tran.InventoryID;
								newtran.SubItemID = tran.SubItemID;
								newtran.SiteID = tran.SiteID;
								newtran.LocationID = tran.LocationID;
								newtran.UOM = tran.UOM;
								newtran.Qty = 0m;
								newtran.InvtMult = (short)0;
								newtran.TranCost = PXDBCurrencyAttribute.BaseRound(this, (decimal)(tran.TranCost - accu_TranCost));
								newtran.AcctID = tran.AcctID;
								newtran.SubID = tran.SubID;
								newtran.COGSAcctID = tran.COGSAcctID;
								newtran.COGSSubID = tran.COGSSubID;
								newtran.InvtAcctID = (prev_layer != null ? prev_layer.AccountID : null);
								newtran.InvtSubID = (prev_layer != null ? prev_layer.SubID : null);
								newtran.OrigRefNbr = (item.ValMethod == INValMethod.FIFO || item.ValMethod == INValMethod.Specific) && prev_layer != null ? prev_layer.ReceiptNbr : null;
								newtran.LotSerialNbr = (item.ValMethod == INValMethod.Specific) && prev_layer != null ? prev_layer.LotSerialNbr : string.Empty;
								newtran.POReceiptNbr = tran.POReceiptNbr;
								newtran.POReceiptLineNbr = tran.POReceiptLineNbr;
								newtran.ReasonCode = tran.ReasonCode;

								intranselect.Insert(newtran);
							}
						}
						intranselect.Cache.SetStatus(tran, PXEntryStatus.Deleted);
					}

					if (intranselect.Cache.IsDirty)
					{
						this.Persist(typeof(INTran), PXDBOperation.Insert);
						this.Persist(typeof(INTranSplit), PXDBOperation.Insert);
						this.Persist(typeof(INItemPlan), PXDBOperation.Insert);
						byte[] timestamp = this.TimeStamp;
						try
						{
							this.TimeStamp = PXDatabase.SelectTimeStamp();
							intranselect.Cache.Persisted(false);
							intransplit.Cache.Persisted(false);
							initemplan.Cache.Persisted(false);
						}
						finally
						{
							this.TimeStamp = timestamp;
						}
					}

					foreach (PXResult<INTranSplit, INTran, INItemPlan, INPlanType, INItemSite, INSite> res in PXSelectJoin<INTranSplit,
						InnerJoin<INTran, On<INTran.tranType, Equal<INTranSplit.tranType>, And<INTran.refNbr, Equal<INTranSplit.refNbr>, And<INTran.lineNbr, Equal<INTranSplit.lineNbr>>>>,
						InnerJoin<INItemPlan, On<INItemPlan.planID, Equal<INTranSplit.planID>>,
						InnerJoin<INPlanType, On<INPlanType.planType, Equal<INItemPlan.planType>>,
						LeftJoin<INItemSite, On<INItemSite.inventoryID, Equal<INTran.inventoryID>, And<INItemSite.siteID, Equal<INTran.toSiteID>>>,
						LeftJoin<INSite, On<INSite.siteID, Equal<INTran.toSiteID>>>>>>>,
					Where<INTranSplit.docType, Equal<Required<INTranSplit.docType>>, And<INTranSplit.refNbr, Equal<Required<INTranSplit.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
					{
						INTranSplit split = res;
						INTran tran = res;
						INItemPlan plan = res;
						INPlanType plantype = res;
						INItemSite itemsite = res ?? new INItemSite();
						INSite site = res ?? new INSite();

                        //avoid ReadItem()
                        initemplan.Cache.SetStatus(plan, PXEntryStatus.Notchanged);

						if (plantype.DeleteOnEvent == true)
						{
							initemplan.Delete(plan);
							split.PlanID = null;
							intransplit.Cache.SetStatus(split, PXEntryStatus.Updated);
						}
						else if (string.IsNullOrEmpty(plantype.ReplanOnEvent) == false)
						{

							if (plantype.ReplanOnEvent == INPlanConstants.Plan42)
							{
								initemplan.Delete(plan);
								plan = PXCache<INItemPlan>.CreateCopy(plan);
								plan.PlanType = plantype.ReplanOnEvent;
								plan.PlanID = null;
								plan.SiteID = tran.ToSiteID;
								plan.LocationID = tran.ToLocationID ?? itemsite.DfltReceiptLocationID ?? site.ReceiptLocationID;
								plan = initemplan.Insert(plan);

								foreach (PXResult<INItemPlan, INPlanType> demand_res in PXSelectJoin<INItemPlan, 
									InnerJoin<INPlanType, On<INPlanType.planType, Equal<INItemPlan.planType>>>, 
									Where<INItemPlan.supplyPlanID, Equal<Required<INItemPlan.supplyPlanID>>>>.Select(this, split.PlanID))
								{
									INItemPlan demand_plan = PXCache<INItemPlan>.CreateCopy(demand_res);
									INPlanType demand_plantype = demand_res;

									//avoid ReadItem()
									initemplan.Cache.SetStatus(demand_plan, PXEntryStatus.Notchanged);

									demand_plan.SupplyPlanID = plan.PlanID;

									if (demand_plantype.ReplanOnEvent == INPlanConstants.Plan95)
									{
										demand_plan.PlanType = demand_plantype.ReplanOnEvent;
										initemplan.Cache.Update(demand_plan);
									}
									else
									{
										initemplan.Cache.SetStatus(demand_plan, PXEntryStatus.Updated);
									}
								}
								split.PlanID = plan.PlanID;
                                split.ToSiteID = tran.ToSiteID;
                                split.ToLocationID = tran.ToLocationID ?? itemsite.DfltReceiptLocationID ?? site.ReceiptLocationID;
								intransplit.Cache.SetStatus(split, PXEntryStatus.Updated);								
							}
							else
							{
								plan = PXCache<INItemPlan>.CreateCopy(plan);
								plan.PlanType = plantype.ReplanOnEvent;
								initemplan.Cache.Update(plan);
							}
						}
					}

					//remove intransit plans
					foreach (PXResult<INTranSplit, INItemPlan, INPlanType, INTran> res in
					PXSelectJoin<INTranSplit,
						InnerJoin<INItemPlan, On<INItemPlan.planID, Equal<INTranSplit.planID>>,
						InnerJoin<INPlanType, On<INPlanType.planType, Equal<INItemPlan.planType>>,
						InnerJoin<INTran, On<INTran.origTranType, Equal<INTranSplit.tranType>,
									And<INTran.origRefNbr, Equal<INTranSplit.refNbr>,
									And<INTran.origLineNbr, Equal<INTranSplit.lineNbr>>>>>>>,
					Where<INTran.docType, Equal<Required<INRegister.docType>>,
						And<INTran.refNbr, Equal<Required<INRegister.refNbr>>>>>
					.Select(this, doc.DocType, doc.RefNbr))
					{
						INTranSplit split = (INTranSplit)res;
						INItemPlan plan = (INItemPlan)res;
						INPlanType plantype = (INPlanType)res;

                        //avoid ReadItem()
                        initemplan.Cache.SetStatus(plan, PXEntryStatus.Notchanged);

						if (plantype.DeleteOnEvent == true)
						{
							initemplan.Delete(plan);
							split.PlanID = null;
							intransplit.Cache.SetStatus(split, PXEntryStatus.Updated);
						}
					}

					//replan fixed SO Demand
					foreach (PXResult<INItemPlan, INTranSplit, INPlanType> res in
						PXSelectJoin<INItemPlan,
						InnerJoin<INTranSplit, On<INTranSplit.planID, Equal<INItemPlan.supplyPlanID>>,
						InnerJoin<INPlanType, On<INPlanType.planType, Equal<INItemPlan.planType>>>>,
					Where<INTranSplit.docType, Equal<Required<INRegister.docType>>,
					  And<INTranSplit.refNbr, Equal<Required<INRegister.refNbr>>>>>
						.Select(this, doc.DocType, doc.RefNbr))
					{
						INItemPlan plan = PXCache<INItemPlan>.CreateCopy((INItemPlan)res);
						INPlanType plantype = (INPlanType)res;
						INTranSplit split = (INTranSplit)res;

                        //avoid ReadItem()
                        initemplan.Cache.SetStatus(plan, PXEntryStatus.Notchanged);

						if (plantype.ReplanOnEvent != null)
						{
							plan.PlanType = plantype.ReplanOnEvent;
							plan.SupplyPlanID = null;
							initemplan.Cache.Update(plan);
						}
						else if (plantype.DeleteOnEvent == true)
						{
							initemplan.Delete(plan);
							split.PlanID = null;
							intransplit.Cache.SetStatus(split, PXEntryStatus.Updated);
						}
					}

					prev_tran = null;

					foreach (PXResult<INTran, InventoryItem, INSite, INPostClass, INLotSerClass, INTranSplit, ReasonCode, INLocation> res in PXSelectJoin<INTran,
						InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INTran.inventoryID>>,
						InnerJoin<INSite, On<INSite.siteID, Equal<INTran.siteID>>,
						LeftJoin<INPostClass, On<INPostClass.postClassID, Equal<InventoryItem.postClassID>>,
						LeftJoin<INLotSerClass, On<INLotSerClass.lotSerClassID, Equal<InventoryItem.lotSerClassID>>,
						LeftJoin<INTranSplit, On<INTranSplit.tranType, Equal<INTran.tranType>, And<INTranSplit.refNbr, Equal<INTran.refNbr>, And<INTranSplit.lineNbr, Equal<INTran.lineNbr>>>>,
						LeftJoin<ReasonCode, On<ReasonCode.reasonCodeID, Equal<INTran.reasonCode>>,
						LeftJoin<INLocation, On<INLocation.locationID, Equal<INTranSplit.locationID>>
						>>>>>>>,
						Where<INTran.docType, Equal<Required<INTran.docType>>, And<INTran.refNbr, Equal<Required<INTran.refNbr>>>>,
						OrderBy<Asc<INTran.tranType, Asc<INTran.refNbr, Asc<INTran.invtMult, Asc<INTran.lineNbr>>>>>>.Select(this, doc.DocType, doc.RefNbr))
					{
						INTran tran = (INTran)res;
						INTranSplit split = (((INTranSplit)res).RefNbr != null) ? (INTranSplit)res : (INTranSplit)tran;
						InventoryItem item = (InventoryItem)res;
						INSite site = (INSite)res;
						ReasonCode reasoncode = (ReasonCode)res;
						INLocation whseloc = (((INLocation)res).LocationID != null) ? (INLocation)res : PXSelect<INLocation, Where<INLocation.locationID, Equal<Current<INTran.locationID>>>>.SelectSingleBound(this, new object[] { tran });
						INPostClass postclass = (INPostClass)res;
						INLotSerClass lotserclass = (INLotSerClass)res;

                        if(site.Active != true)
                            throw new PXException(Messages.InactiveWarehouse, site.SiteCD);

                        PXParentAttribute.SetParent(intranselect.Cache, tran, typeof(INRegister), inregister.Current);

                        PXSelectJoin<InventoryItem, LeftJoin<INPostClass, On<INPostClass.postClassID, Equal<InventoryItem.postClassID>>>, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.StoreCached(this, new PXCommandKey(new object[] { tran.InventoryID }), new List<object> { new PXResult<InventoryItem, INPostClass>((InventoryItem)res, (INPostClass)res) });
                        PXSelect<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>.StoreCached(this, new PXCommandKey(new object[] { tran.SiteID }), new List<object> { site });

						tran = PXCache<INTran>.CreateCopy(tran);
						tran.TranDate = doc.TranDate;
						tran.TranPeriodID = doc.TranPeriodID;
						tran.FinPeriodID = doc.FinPeriodID;
						tran.Released = true;
						tran = intranselect.Update(tran);

						//zero quantity auto added splits will have it null
						if (split.CreatedDateTime != null)
						{
							//locate split record not to erase PlanID
							split = intransplit.Locate(split) ?? split;
							split.TranDate = doc.TranDate;
							split.Released = true;
							split = intransplit.Update(split);
						}

						if (item.StkItem == true)
						{
							UpdateCrossReference(split, item, whseloc);
							UpdateItemSite(tran, item, site, reasoncode, postclass);

							if (split.BaseQty != 0m)
							{
								UpdateSiteStatus(split, whseloc);
                                UpdateLocationStatus(split);
								UpdateLotSerialStatus(tran, split, item, lotserclass);
								UpdateSiteHist(tran, split);
                                UpdateSiteHistD(split);
							}
							UpdateItemLotSerial(tran, split, item, lotserclass);

							UpdateCostStatus(prev_tran, tran, split, item);
							prev_tran = tran;
						}
						else
						{
							if (tran.AssyType == INAssyType.KitTran || tran.AssyType == INAssyType.CompTran)
							{
								throw new PXException(Messages.NonStockKitAssemblyNotAllowed);
							}

							UpdateItemSite(tran, item, site, reasoncode, postclass);
							AssembleCost(tran, split, item);
							WriteGLNonStockCosts(je, tran, item, site);
						}
					}

					//CreateReceiptLayers should precede Oversold
					foreach (AverageCostStatus layer in averagecoststatus.Cache.Inserted)
					{
						//ignore TransfersIn
						if (doc.DocType == INDocType.Receipt && layer.LayerType == INLayerType.Normal && layer.QtyOnHand > 0m)
						{
							layer.OrigQty = 0m;

							ReceiptStatus receipt = new ReceiptStatus();
							receipt.InventoryID = layer.InventoryID;
							receipt.CostSiteID = layer.CostSiteID;
							receipt.CostSubItemID = layer.CostSubItemID;
							receipt.ReceiptNbr = doc.RefNbr;
							receipt.ReceiptDate = doc.TranDate;
							receipt.OrigQty = layer.QtyOnHand;
							receipt.ValMethod = layer.ValMethod;

							receiptstatus.Insert(receipt);
						}
					}
                    if (this.insetup.Current.ReplanBackOrders == true)
                    {
						ReplanBackOrders();
                    }

					foreach (SpecificCostStatus layer in specificcoststatus.Cache.Inserted)
					{
						if (layer.QtyOnHand > 0m)
						{
							layer.OrigQty = layer.QtyOnHand;

							ReceiptStatus receipt = new ReceiptStatus();
							receipt.InventoryID = layer.InventoryID;
							receipt.CostSiteID = layer.CostSiteID;
							receipt.CostSubItemID = layer.CostSubItemID;
							receipt.ReceiptNbr = layer.ReceiptNbr;
							receipt.ReceiptDate = layer.ReceiptDate;
							receipt.ValMethod = layer.ValMethod;

							receipt = receiptstatus.Insert(receipt);

							receipt.OrigQty += layer.QtyOnHand;
						}
					}

					foreach (FIFOCostStatus layer in fifocoststatus.Cache.Inserted)
					{
						if (layer.LayerType == INLayerType.Normal && layer.QtyOnHand > 0m)
						{
							layer.OrigQty = layer.QtyOnHand;
						}
					}

					ReceiveOversold(doc);

					prev_tran = null;
                    //foreach (PXResult<INTranCost, INTran, InventoryItem, INSite, INPostClass, ReasonCode> res in PXSelectJoin<INTranCost,
                    //    InnerJoin<INTran, On<INTran.tranType, Equal<INTranCost.tranType>, And<INTran.refNbr, Equal<INTranCost.refNbr>, And<INTran.lineNbr, Equal<INTranCost.lineNbr>>>>,
                    //    InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INTranCost.inventoryID>>,
                    //    InnerJoin<INSite, On<INSite.siteID, Equal<INTran.siteID>>,
                    //    InnerJoin<INPostClass, On<INPostClass.postClassID, Equal<InventoryItem.postClassID>>,
                    //    LeftJoin<ReasonCode, On<ReasonCode.reasonCodeID, Equal<INTran.reasonCode>>>>>>>,
                    //    Where<INTranCost.costDocType, Equal<Required<INTranCost.costDocType>>, And<INTranCost.costRefNbr, Equal<Required<INTranCost.costRefNbr>>>>, OrderBy<Asc<INTranCost.tranType, Asc<INTranCost.refNbr, Asc<INTranCost.lineNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
					//{
					//	INTran tran = (INTran)intranselect.Cache.Locate((INTran)res);
					//	INTranCost costtran = (INTranCost)res;
                    //  InventoryItem item = (InventoryItem)res;
                    //  ReasonCode reasoncode = res;
                    foreach (INTranCost costtran in intrancost.Cache.Inserted)
                    {
                        INTran tran = (INTran)PXParentAttribute.SelectParent(intrancost.Cache, costtran, typeof(INTran));
                        if (tran != null)
                        {
                            PXResult<InventoryItem, INPostClass> res = (PXResult<InventoryItem, INPostClass>)PXSelectJoin<InventoryItem, LeftJoin<INPostClass, On<INPostClass.postClassID, Equal<InventoryItem.postClassID>>>, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, costtran.InventoryID);
                            INSite site = PXSelect<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>.Select(this, tran.SiteID);
                            ReasonCode reasoncode = PXSelect<ReasonCode, Where<ReasonCode.reasonCodeID, Equal<Required<ReasonCode.reasonCodeID>>>>.Select(this, tran.ReasonCode);

                            UpdateCostHist(costtran, tran);
                            UpdateSalesHist(costtran, tran);
                            UpdateSalesHistD(tran);
                            UpdateCustSalesHist(costtran, tran);
                            UpdateCustSalesStats(tran);

                            if (object.Equals(prev_tran, tran) == false)
                            {
                                if (tran.DocType == costtran.CostDocType && tran.RefNbr == costtran.CostRefNbr)
                                {
                                    WriteGLSales(je, tran);
                                }

                                if ((tran.InvtMult == (short)-1 || tran.InvtMult == (short)1 && tran.OrigLineNbr != null) && tran.Qty != 0m)
                                {
                                    tran.UnitCost = PXDBPriceCostAttribute.Round(intranselect.Cache, (decimal)tran.TranCost / (decimal)tran.Qty);
                                }
                            }

                            if (tran.ARRefNbr != null)
                            {
                                ARTran artran = new ARTran();
                                artran.TranType = tran.ARDocType;
                                artran.RefNbr = tran.ARRefNbr;
                                artran.LineNbr = tran.ARLineNbr;

                                artran = this.artran.Insert(artran);

                                artran.TranCost += costtran.TranCost;
                            }

                            prev_tran = tran;

                            WriteGLCosts(je, costtran, tran, (InventoryItem)res, site, (INPostClass)res, reasoncode);
                        }
					}

					foreach (ItemCostHist hist in itemcosthist.Cache.Inserted)
					{
						INSite insite = PXSelect<INSite, Where<INSite.siteID, Equal<Current<ItemCostHist.costSiteID>>>>.SelectSingleBound(this, new object[] { hist });

						if (insite != null)
						{
							ItemStats stats = new ItemStats();
							stats.InventoryID = hist.InventoryID;
							stats.SiteID = hist.CostSiteID;

							stats = itemstats.Insert(stats);

							stats.QtyOnHand += hist.FinYtdQty;
							stats.TotalCost += hist.FinYtdCost;
							stats.QtyReceived += hist.FinPtdQtyReceived + hist.FinPtdQtyTransferIn;
							stats.CostReceived += hist.FinPtdCostReceived + hist.FinPtdCostTransferIn;
						}
					}

					foreach (ItemStats stats in itemstats.Cache.Cached)
					{
						if (itemstats.Cache.GetStatus(stats) != PXEntryStatus.Notchanged)
						{
							if (stats.QtyReceived != 0m && stats.QtyReceived != null && stats.CostReceived != null)
							{
								stats.LastCostDate = DateTime.Now;
							}

							stats.LastCost = stats.QtyReceived == 0m || stats.QtyReceived == null || stats.CostReceived == null ? 0m : PXDBPriceCostAttribute.Round(itemstats.Cache, (decimal)(stats.CostReceived / stats.QtyReceived));
							stats.MaxCost = stats.LastCost;
							stats.MinCost = stats.LastCost;
						}
					}

					if (UpdateGL)
					{
						je.Save.Press();
						doc.BatchNbr = je.BatchModule.Current.BatchNbr;
					}

					doc.Released = true;
					inregister.Update(doc);

					this.Actions.PressSave();

					ts.Complete();
				}
			}
		}

		public virtual void ReplanBackOrders()
		{
			ReplanBackOrders(this, false);
		}
		
		public static void ReplanBackOrders(PXGraph graph)
		{
			ReplanBackOrders(graph, true);
		}

		public static void ReplanBackOrders(PXGraph graph, bool ForceReplan)
		{
			List<INItemPlan> replan = new List<INItemPlan>();
			foreach (SiteStatus layer in graph.Caches[typeof(SiteStatus)].Inserted)
			{
				if (layer.QtyOnHand <= 0m && !ForceReplan)
					continue;

				SiteStatus dbLayer = PXSelectReadonly<SiteStatus,
					Where<SiteStatus.inventoryID, Equal<Required<SiteStatus.inventoryID>>,
						And<SiteStatus.subItemID, Equal<Required<SiteStatus.subItemID>>,
							And<SiteStatus.siteID, Equal<Required<SiteStatus.siteID>>>>>>
					.Select(graph, layer.InventoryID, layer.SubItemID, layer.SiteID);

				decimal qtyAvail =
					layer.QtyOnHand.Value
					- layer.QtyNotAvail.Value
					- layer.QtySOShipped.Value
					- layer.QtySOShipping.Value
					- layer.QtySOBooked.Value
					- layer.QtyINIssues.Value
					- layer.QtyINAssemblyDemand.Value
					+
					(
						(dbLayer != null)
							? (dbLayer.QtyOnHand.Value
							   - dbLayer.QtyNotAvail.Value
							   - dbLayer.QtySOShipped.Value
							   - dbLayer.QtySOShipping.Value
							   - dbLayer.QtySOBooked.Value
							   - dbLayer.QtyINIssues.Value
							   - dbLayer.QtyINAssemblyDemand.Value)
							: 0m);

				if (qtyAvail > 0m)
				{
					foreach (INItemPlan plan in PXSelectJoin<INItemPlan,
						InnerJoin<SOOrder, On<SOOrder.noteID, Equal<INItemPlan.refNoteID>>,
							InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrder.orderType>>>>,
						Where<INItemPlan.planType, Equal<Required<INItemPlan.planType>>,
							And<INItemPlan.inventoryID, Equal<Required<INItemPlan.inventoryID>>,
								And<INItemPlan.subItemID, Equal<Required<INItemPlan.subItemID>>,
									And<INItemPlan.siteID, Equal<Required<INItemPlan.siteID>>,
										And<SOOrderType.requireAllocation, Equal<False>>>>>>,
						OrderBy<Asc<INItemPlan.planDate, Desc<INItemPlan.planQty>>>>
						.Select(graph, INPlanConstants.Plan68, layer.InventoryID, layer.SubItemID, layer.SiteID))
					{
						if (plan.PlanQty <= qtyAvail)
						{
							qtyAvail -= plan.PlanQty.Value;
							replan.Add(plan);
						}
						else if (qtyAvail > 0)
						{
							SOLine soLine = PXSelect<SOLine,
								Where<SOLine.planID, Equal<Required<SOLine.planID>>>>.Select(graph, plan.PlanID);
							if (soLine != null && soLine.ShipComplete != SOShipComplete.ShipComplete)
							{
								replan.Add(plan);
								qtyAvail = 0m;
							}
						}
					}
				}
			}
			PXCache plancache = graph.Caches[typeof(INItemPlan)];
			foreach (INItemPlan plan in replan)
			{
				INItemPlan upd = PXCache<INItemPlan>.CreateCopy(plan);
				upd.PlanType = INPlanConstants.Plan60;
				plancache.Update(upd);
			}
		}
	}
}
namespace PX.Objects.IN
{
	public interface ICostStatus
	{
		decimal? QtyOnHand { get; set; }
		decimal? TotalCost { get; set; }
	}

	public interface IStatus
	{
		decimal? QtyOnHand { get; set; }
		decimal? QtyAvail { get; set; }
		decimal? QtyNotAvail { get; set; }
		decimal? QtyExpired { get; set; }
		decimal? QtyHardAvail { get; set; }
		decimal? QtyINIssues { get; set; }
		decimal? QtyINReceipts { get; set; }
		decimal? QtyInTransit { get; set; }
		decimal? QtyPOPrepared { get; set; }
		decimal? QtyPOOrders { get; set; }
		decimal? QtyPOReceipts { get; set; }
		decimal? QtySOBackOrdered { get; set; }
		decimal? QtySOBooked { get; set; }
		decimal? QtySOShipped { get; set; }
		decimal? QtySOShipping { get; set; }
		decimal? QtyINAssemblySupply { get; set; }
		decimal? QtyINAssemblyDemand { get; set; }
		decimal? QtySOFixed { get; set; }
		decimal? QtyPOFixedOrders { get; set; }
		decimal? QtyPOFixedPrepared { get; set; }
		decimal? QtyPOFixedReceipts { get; set; }
		decimal? QtySODropShip { get; set; }
		decimal? QtyPODropShipOrders { get; set; }
		decimal? QtyPODropShipPrepared { get; set; }
		decimal? QtyPODropShipReceipts { get; set; }
	}
}

namespace PX.Objects.IN.Overrides.INDocumentRelease
{
	public interface IQtyAllocated : IQtyAllocatedBase
	{
		decimal? QtyINIssues { get; set; }
		decimal? QtyINReceipts { get; set; }
		decimal? QtyPOPrepared { get; set; }
		decimal? QtyPOOrders { get; set; }
		decimal? QtyPOReceipts { get; set; }
		decimal? QtySOBackOrdered { get; set; }
		decimal? QtySOBooked { get; set; }
		decimal? QtySOShipped { get; set; }
		decimal? QtySOShipping { get; set; }
		decimal? QtyINAssemblySupply { get; set; }
		decimal? QtyINAssemblyDemand { get; set; }
		decimal? QtyINReplaned { get; set; }
		decimal? QtySOFixed { get; set; }
		decimal? QtyPOFixedOrders { get; set; }
		decimal? QtyPOFixedPrepared { get; set; }
		decimal? QtyPOFixedReceipts { get; set; }
		decimal? QtySODropShip { get; set; }
		decimal? QtyPODropShipOrders { get; set; }
		decimal? QtyPODropShipPrepared { get; set; }
		decimal? QtyPODropShipReceipts { get; set; }
	}

    public interface IQtyAllocatedBase
    {
        bool? NegQty { get; }
        bool? InclQtyAvail { get; }
        bool? InclQtySOReverse { get; }
        bool? InclQtySOBackOrdered { get; }
        bool? InclQtySOBooked { get; }
        bool? InclQtySOShipped { get; }
        bool? InclQtySOShipping { get; }
        bool? InclQtyPOPrepared { get; }
        bool? InclQtyPOOrders { get; }
        bool? InclQtyPOReceipts { get; }
        bool? InclQtyInTransit { get; }
        bool? InclQtyINIssues { get; }
        bool? InclQtyINReceipts { get; }
        bool? InclQtyINAssemblySupply { get; }
        bool? InclQtyINAssemblyDemand { get; }
        decimal? QtyOnHand { get; set; }
        decimal? QtyAvail { get; set; }
        decimal? QtyHardAvail { get; set; }
        decimal? QtyNotAvail { get; set; }
        decimal? QtyInTransit { get; set; }
    }

	[ItemStatsAccumulator()]
    [PXDisableCloneAttributes()]
    [Serializable]
	public partial class ItemStats : INItemStats
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region SiteID
		public new abstract class siteID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region ValMethod
		public abstract new class valMethod : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(Search<InventoryItem.valMethod, Where<InventoryItem.inventoryID, Equal<Current<ItemStats.inventoryID>>>>))]
		public override String ValMethod
		{
			get
			{
				return this._ValMethod;
			}
			set
			{
				this._ValMethod = value;
			}
		}
		#endregion
		#region StdCost
		public new abstract class stdCost : PX.Data.IBqlField
		{
		}
		[PXDecimal(6)]
		public override Decimal? StdCost
		{
			get
			{
				return this._StdCost;
			}
			set
			{
				this._StdCost = value;
			}
		}
		#endregion
	}

	public class ARTranAccum : PXAccumulatorAttribute
	{
		public ARTranAccum()
		{
			this.SingleRecord = true;
		}

		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			columns.Update<ARTran.tranCost>(((ARTran)row).TranCost, PXDataFieldAssign.AssignBehavior.Summarize);

			return true;
		}
	}

	[ARTranAccum()]
    [Serializable]
	public partial class ARTran : IBqlTable
	{
		#region TranType
		public abstract class tranType : PX.Data.IBqlField
		{
		}
		protected String _TranType;
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		public virtual String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.IBqlField
		{
		}
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region TranCost
		public abstract class tranCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranCost;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranCost
		{
			get
			{
				return this._TranCost;
			}
			set
			{
				this._TranCost = value;
			}
		}
		#endregion
	}

	[SiteStatusAccumulator]
    [PXDisableCloneAttributes()]
    [Serializable]
	public partial class SiteStatus : INSiteStatus, IQtyAllocated
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXForeignSelector(typeof(INTran.inventoryID))]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region SubItemID
		public new abstract class subItemID : PX.Data.IBqlField
		{
		}
		[SubItem(IsKey = true)]
		[PXForeignSelector(typeof(INTran.subItemID))]
		[PXDefault()]
		public override Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SiteID
		public new abstract class siteID : PX.Data.IBqlField
		{
		}
		//[PXDBInt(IsKey = true)]
		[PXForeignSelector(typeof(INTran.siteID))]
		[IN.Site(IsKey = true)]
		[PXDefault()]
		public override Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region ItemClassID
		public abstract class itemClassID : PX.Data.IBqlField
		{
		}
		protected String _ItemClassID;
		[PXString(10, IsUnicode = true)]
		[PXDBScalar(typeof(Search<InventoryItem.itemClassID, Where<InventoryItem.inventoryID, Equal<INSiteStatus.inventoryID>>>))]
		[PXDefault(typeof(Search<InventoryItem.itemClassID, Where<InventoryItem.inventoryID, Equal<Current<SiteStatus.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String ItemClassID
		{
			get
			{
				return this._ItemClassID;
			}
			set
			{
				this._ItemClassID = value;
			}
		}
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.IBqlField
		{
		}
		#endregion
		#region NegQty
		public abstract class negQty : PX.Data.IBqlField
		{
		}
		protected bool? _NegQty;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<SiteStatus.itemClassID>>>>), SourceField = typeof(INItemClass.negQty), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBScalar(typeof(Search2<INItemClass.negQty, InnerJoin<InventoryItem, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>, Where<InventoryItem.inventoryID, Equal<INSiteStatus.inventoryID>>>))]
		public virtual bool? NegQty
		{
			get
			{
				return this._NegQty;
			}
			set
			{
				this._NegQty = value;
			}
		}
		#endregion
		#region NegAvailQty
		public abstract class negAvailQty : PX.Data.IBqlField
		{
		}
		protected bool? _NegAvailQty;
		[PXBool()]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? NegAvailQty
		{
			get
			{
				return this._NegAvailQty;
			}
			set
			{
				this._NegAvailQty = value;
			}
		}
		#endregion
		#region InclQtyAvail
		public abstract class inclQtyAvail : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyAvail;
		[PXBool()]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyAvail
		{
			get
			{
				return this._InclQtyAvail;
			}
			set
			{
				this._InclQtyAvail = value;
			}
		}
		#endregion
		#region InclQtySOReverse
		public abstract class inclQtySOReverse : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtySOReverse;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<SiteStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtySOReverse), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtySOReverse
		{
			get
			{
				return this._InclQtySOReverse;
			}
			set
			{
				this._InclQtySOReverse = value;
			}
		}
		#endregion
		#region InclQtySOBackOrdered
		public abstract class inclQtySOBackOrdered : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtySOBackOrdered;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<SiteStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtySOBackOrdered), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtySOBackOrdered
		{
			get
			{
				return this._InclQtySOBackOrdered;
			}
			set
			{
				this._InclQtySOBackOrdered = value;
			}
		}
		#endregion
		#region InclQtySOBooked
		public abstract class inclQtySOBooked : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtySOBooked;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<SiteStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtySOBooked), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtySOBooked
		{
			get
			{
				return this._InclQtySOBooked;
			}
			set
			{
				this._InclQtySOBooked = value;
			}
		}
		#endregion
		#region InclQtySOShipped
		public abstract class inclQtySOShipped : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtySOShipped;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<SiteStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtySOShipped), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtySOShipped
		{
			get
			{
				return this._InclQtySOShipped;
			}
			set
			{
				this._InclQtySOShipped = value;
			}
		}
		#endregion
		#region InclQtySOShipping
		public abstract class inclQtySOShipping : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtySOShipping;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<SiteStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtySOShipping), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtySOShipping
		{
			get
			{
				return this._InclQtySOShipping;
			}
			set
			{
				this._InclQtySOShipping = value;
			}
		}
		#endregion
		#region InclQtyInTransit
		public abstract class inclQtyInTransit : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyInTransit;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<SiteStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyInTransit), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyInTransit
		{
			get
			{
				return this._InclQtyInTransit;
			}
			set
			{
				this._InclQtyInTransit = value;
			}
		}
		#endregion
		#region InclQtyPOReceipts
		public abstract class inclQtyPOReceipts : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyPOReceipts;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<SiteStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyPOReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyPOReceipts
		{
			get
			{
				return this._InclQtyPOReceipts;
			}
			set
			{
				this._InclQtyPOReceipts = value;
			}
		}
		#endregion
		#region InclQtyPOPrepared
		public abstract class inclQtyPOPrepared : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyPOPrepared;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<SiteStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyPOPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyPOPrepared
		{
			get
			{
				return this._InclQtyPOPrepared;
			}
			set
			{
				this._InclQtyPOPrepared = value;
			}
		}
		#endregion
		#region InclQtyPOOrders
		public abstract class inclQtyPOOrders : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyPOOrders;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<SiteStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyPOOrders), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyPOOrders
		{
			get
			{
				return this._InclQtyPOOrders;
			}
			set
			{
				this._InclQtyPOOrders = value;
			}
		}
		#endregion
		#region InclQtyINIssues
		public abstract class inclQtyINIssues : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyINIssues;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<SiteStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyINIssues), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyINIssues
		{
			get
			{
				return this._InclQtyINIssues;
			}
			set
			{
				this._InclQtyINIssues = value;
			}
		}
		#endregion
		#region InclQtyINReceipts
		public abstract class inclQtyINReceipts : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyINReceipts;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<SiteStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyINReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyINReceipts
		{
			get
			{
				return this._InclQtyINReceipts;
			}
			set
			{
				this._InclQtyINReceipts = value;
			}
		}
		#endregion
		#region InclQtyINAssemblyDemand
		public abstract class inclQtyINAssemblyDemand : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyINAssemblyDemand;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<SiteStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyINAssemblyDemand), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyINAssemblyDemand
		{
			get
			{
				return this._InclQtyINAssemblyDemand;
			}
			set
			{
				this._InclQtyINAssemblyDemand = value;
			}
		}
		#endregion
		#region InclQtyINAssemblySupply
		public abstract class inclQtyINAssemblySupply : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyINAssemblySupply;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<SiteStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyINAssemblySupply), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyINAssemblySupply
		{
			get
			{
				return this._InclQtyINAssemblySupply;
			}
			set
			{
				this._InclQtyINAssemblySupply = value;
			}
		}
		#endregion
	}

	[LocationStatusAccumulator]
    [PXDisableCloneAttributes()]
    [Serializable]
	public partial class LocationStatus : INLocationStatus, IQtyAllocated, ICostStatus
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXForeignSelector(typeof(INTran.inventoryID))]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region SubItemID
		public new abstract class subItemID : PX.Data.IBqlField
		{
		}
		[SubItem(IsKey = true)]
		[PXForeignSelector(typeof(INTran.subItemID))]
		[PXDefault()]
		public override Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SiteID
		public new abstract class siteID : PX.Data.IBqlField
		{
		}
		//[PXDBInt(IsKey = true)]
		[PXForeignSelector(typeof(INTran.siteID))]
		[IN.Site(IsKey = true)]
		[PXDefault()]
		public override Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region LocationID
		public new abstract class locationID : PX.Data.IBqlField
		{
		}
		[IN.Location(IsKey = true, ValidComboRequired = false)]
		[PXForeignSelector(typeof(INTran.locationID))]
		[PXDefault()]
		public override Int32? LocationID
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
		#region ItemClassID
		public abstract class itemClassID : PX.Data.IBqlField
		{
		}
		protected String _ItemClassID;
		[PXString(10, IsUnicode = true)]
		[PXDBScalar(typeof(Search<InventoryItem.itemClassID, Where<InventoryItem.inventoryID, Equal<INLocationStatus.inventoryID>>>))]
		[PXDefault(typeof(Search<InventoryItem.itemClassID, Where<InventoryItem.inventoryID, Equal<Current<LocationStatus.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String ItemClassID
		{
			get
			{
				return this._ItemClassID;
			}
			set
			{
				this._ItemClassID = value;
			}
		}
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.IBqlField
		{
		}
		#endregion
		#region TotalCost
		public abstract class totalCost : PX.Data.IBqlField
		{
		}
		protected Decimal? _TotalCost;
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? TotalCost
		{
			get
			{
				return this._TotalCost;
			}
			set
			{
				this._TotalCost = value;
			}
		}
		#endregion
		#region QtyNotAvail
		public new abstract class qtyNotAvail : PX.Data.IBqlField
		{
		}
		#endregion
		#region NegQty
		public abstract class negQty : PX.Data.IBqlField
		{
		}
		protected bool? _NegQty;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LocationStatus.itemClassID>>>>), SourceField = typeof(INItemClass.negQty), PersistingCheck = PXPersistingCheck.Nothing)]
		//[PXDBScalar(typeof(Search2<INItemClass.negQty, InnerJoin<InventoryItem, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>, Where<InventoryItem.inventoryID, Equal<INLocationStatus.inventoryID>>>))]
		public virtual bool? NegQty
		{
			get
			{
				return this._NegQty;
			}
			set
			{
				this._NegQty = value;
			}
		}
		#endregion
		#region QtyINReplaned
		public decimal? QtyINReplaned
		{
			get { return null; }
			set { }
		}
		#endregion
		#region InclQtyAvail
		public abstract class inclQtyAvail : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyAvail;
		[PXBool()]
		[PXDefault(typeof(Search<INLocation.inclQtyAvail, Where<INLocation.locationID, Equal<Current<LocationStatus.locationID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyAvail
		{
			get
			{
				return this._InclQtyAvail;
			}
			set
			{
				this._InclQtyAvail = value;
			}
		}
		#endregion
		#region InclQtySOReverse
		public abstract class inclQtySOReverse : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtySOReverse;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LocationStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtySOReverse), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtySOReverse
		{
			get
			{
				return this._InclQtySOReverse;
			}
			set
			{
				this._InclQtySOReverse = value;
			}
		}
		#endregion
		#region InclQtySOBackOrdered
		public abstract class inclQtySOBackOrdered : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtySOBackOrdered;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LocationStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtySOBackOrdered), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtySOBackOrdered
		{
			get
			{
				return this._InclQtySOBackOrdered;
			}
			set
			{
				this._InclQtySOBackOrdered = value;
			}
		}
		#endregion
		#region InclQtySOBooked
		public abstract class inclQtySOBooked : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtySOBooked;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LocationStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtySOBooked), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtySOBooked
		{
			get
			{
				return this._InclQtySOBooked;
			}
			set
			{
				this._InclQtySOBooked = value;
			}
		}
		#endregion
		#region InclQtySOShipped
		public abstract class inclQtySOShipped : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtySOShipped;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LocationStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtySOShipped), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtySOShipped
		{
			get
			{
				return this._InclQtySOShipped;
			}
			set
			{
				this._InclQtySOShipped = value;
			}
		}
		#endregion
		#region InclQtySOShipping
		public abstract class inclQtySOShipping : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtySOShipping;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LocationStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtySOShipping), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtySOShipping
		{
			get
			{
				return this._InclQtySOShipping;
			}
			set
			{
				this._InclQtySOShipping = value;
			}
		}
		#endregion
		#region InclQtyInTransit
		public abstract class inclQtyInTransit : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyInTransit;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LocationStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyInTransit), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyInTransit
		{
			get
			{
				return this._InclQtyInTransit;
			}
			set
			{
				this._InclQtyInTransit = value;
			}
		}
		#endregion
		#region InclQtyPOReceipts
		public abstract class inclQtyPOReceipts : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyPOReceipts;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LocationStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyPOReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyPOReceipts
		{
			get
			{
				return this._InclQtyPOReceipts;
			}
			set
			{
				this._InclQtyPOReceipts = value;
			}
		}
		#endregion
		#region InclQtyPOPrepared
		public abstract class inclQtyPOPrepared : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyPOPrepared;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LocationStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyPOPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyPOPrepared
		{
			get
			{
				return this._InclQtyPOPrepared;
			}
			set
			{
				this._InclQtyPOPrepared = value;
			}
		}
		#endregion
		#region InclQtyPOOrders
		public abstract class inclQtyPOOrders : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyPOOrders;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LocationStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyPOOrders), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyPOOrders
		{
			get
			{
				return this._InclQtyPOOrders;
			}
			set
			{
				this._InclQtyPOOrders = value;
			}
		}
		#endregion
		#region InclQtyINIssues
		public abstract class inclQtyINIssues : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyINIssues;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LocationStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyINIssues), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyINIssues
		{
			get
			{
				return this._InclQtyINIssues;
			}
			set
			{
				this._InclQtyINIssues = value;
			}
		}
		#endregion
		#region InclQtyINReceipts
		public abstract class inclQtyINReceipts : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyINReceipts;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LocationStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyINReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyINReceipts
		{
			get
			{
				return this._InclQtyINReceipts;
			}
			set
			{
				this._InclQtyINReceipts = value;
			}
		}
		#endregion
		#region InclQtyINAssemblyDemand
		public abstract class inclQtyINAssemblyDemand : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyINAssemblyDemand;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LocationStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyINAssemblyDemand), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyINAssemblyDemand
		{
			get
			{
				return this._InclQtyINAssemblyDemand;
			}
			set
			{
				this._InclQtyINAssemblyDemand = value;
			}
		}
		#endregion
		#region InclQtyINAssemblySupply
		public abstract class inclQtyINAssemblySupply : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyINAssemblySupply;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LocationStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyINAssemblySupply), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyINAssemblySupply
		{
			get
			{
				return this._InclQtyINAssemblySupply;
			}
			set
			{
				this._InclQtyINAssemblySupply = value;
			}
		}
		#endregion
	}

	[LotSerialStatusAccumulator]
    [Serializable]
	public partial class LotSerialStatus : INLotSerialStatus, IQtyAllocated
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXForeignSelector(typeof(INTran.inventoryID))]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region SubItemID
		public new abstract class subItemID : PX.Data.IBqlField
		{
		}
		[SubItem(IsKey = true)]
		[PXForeignSelector(typeof(INTran.subItemID))]
		[PXDefault()]
		public override Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SiteID
		public new abstract class siteID : PX.Data.IBqlField
		{
		}
		//[PXDBInt(IsKey = true)]
		[PXForeignSelector(typeof(INTran.siteID))]
		[IN.Site(IsKey = true)]
		[PXDefault()]
		public override Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region LocationID
		public new abstract class locationID : PX.Data.IBqlField
		{
		}
		[IN.Location(IsKey = true, ValidComboRequired = false)]
		[PXForeignSelector(typeof(INTran.locationID))]
		[PXDefault()]
		public override Int32? LocationID
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
		#region LotSerialNbr
		public new abstract class lotSerialNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(100, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		public override String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region CostID
		public new abstract class costID : PX.Data.IBqlField
		{
		}
		[PXDBLong()]
		public override Int64? CostID
		{
			get
			{
				return this._CostID;
			}
			set
			{
				this._CostID = value;
			}
		}
		#endregion
		#region ItemClassID
		public abstract class itemClassID : PX.Data.IBqlField
		{
		}
		protected String _ItemClassID;
		[PXString(10, IsUnicode = true)]
		[PXDBScalar(typeof(Search<InventoryItem.itemClassID, Where<InventoryItem.inventoryID, Equal<INLotSerialStatus.inventoryID>>>))]
		[PXDefault(typeof(Search<InventoryItem.itemClassID, Where<InventoryItem.inventoryID, Equal<Current<LotSerialStatus.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String ItemClassID
		{
			get
			{
				return this._ItemClassID;
			}
			set
			{
				this._ItemClassID = value;
			}
		}
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.IBqlField
		{
		}
		#endregion
		#region LotSerTrack
		public new abstract class lotSerTrack : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(Select2<INLotSerClass, InnerJoin<InventoryItem, On<InventoryItem.lotSerClassID, Equal<INLotSerClass.lotSerClassID>>>, Where<InventoryItem.inventoryID, Equal<Current<LotSerialStatus.inventoryID>>>>), SourceField = typeof(INLotSerClass.lotSerTrack), PersistingCheck = PXPersistingCheck.Nothing)]
		public override String LotSerTrack
		{
			get
			{
				return this._LotSerTrack;
			}
			set
			{
				this._LotSerTrack = value;
			}
		}
		#endregion
		#region ExpireDate
		public new abstract class expireDate : PX.Data.IBqlField
		{
		}
		[PXDate()]
		public override DateTime? ExpireDate
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
		#region ReceiptDate
		public new abstract class receiptDate : PX.Data.IBqlField
		{
		}
		[PXDBDate()]
		public override DateTime? ReceiptDate
		{
			get
			{
				return this._ReceiptDate;
			}
			set
			{
				this._ReceiptDate = value;
			}
		}
		#endregion
		#region QtyNotAvail
		public new abstract class qtyNotAvail : PX.Data.IBqlField
		{
		}
		#endregion
		#region NegQty
		public abstract class negQty : PX.Data.IBqlField
		{
		}
		protected bool? _NegQty;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LotSerialStatus.itemClassID>>>>), SourceField = typeof(INItemClass.negQty), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBScalar(typeof(Search2<INItemClass.negQty, InnerJoin<InventoryItem, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>, Where<InventoryItem.inventoryID, Equal<INLotSerialStatus.inventoryID>>>))]
		public virtual bool? NegQty
		{
			get
			{
				return this._NegQty;
			}
			set
			{
				this._NegQty = value;
			}
		}
		#endregion
		#region QtyINReplaned
		public decimal? QtyINReplaned
		{
			get { return null; }
			set { }
		}
		#endregion
		#region InclQtyAvail
		public abstract class inclQtyAvail : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyAvail;
		[PXBool()]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyAvail
		{
			get
			{
				return this._InclQtyAvail;
			}
			set
			{
				this._InclQtyAvail = value;
			}
		}
		#endregion
		#region InclQtySOReverse
		public abstract class inclQtySOReverse : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtySOReverse;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LotSerialStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtySOReverse), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtySOReverse
		{
			get
			{
				return this._InclQtySOReverse;
			}
			set
			{
				this._InclQtySOReverse = value;
			}
		}
		#endregion
		#region InclQtySOBackOrdered
		public abstract class inclQtySOBackOrdered : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtySOBackOrdered;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LotSerialStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtySOBackOrdered), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtySOBackOrdered
		{
			get
			{
				return this._InclQtySOBackOrdered;
			}
			set
			{
				this._InclQtySOBackOrdered = value;
			}
		}
		#endregion
		#region InclQtySOBooked
		public abstract class inclQtySOBooked : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtySOBooked;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LotSerialStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtySOBooked), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtySOBooked
		{
			get
			{
				return this._InclQtySOBooked;
			}
			set
			{
				this._InclQtySOBooked = value;
			}
		}
		#endregion
		#region InclQtySOShipped
		public abstract class inclQtySOShipped : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtySOShipped;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LotSerialStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtySOShipped), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtySOShipped
		{
			get
			{
				return this._InclQtySOShipped;
			}
			set
			{
				this._InclQtySOShipped = value;
			}
		}
		#endregion
		#region InclQtySOShipping
		public abstract class inclQtySOShipping : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtySOShipping;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LotSerialStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtySOShipping), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtySOShipping
		{
			get
			{
				return this._InclQtySOShipping;
			}
			set
			{
				this._InclQtySOShipping = value;
			}
		}
		#endregion
		#region InclQtyInTransit
		public abstract class inclQtyInTransit : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyInTransit;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LotSerialStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyInTransit), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyInTransit
		{
			get
			{
				return this._InclQtyInTransit;
			}
			set
			{
				this._InclQtyInTransit = value;
			}
		}
		#endregion
		#region InclQtyPOReceipts
		public abstract class inclQtyPOReceipts : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyPOReceipts;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LotSerialStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyPOReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyPOReceipts
		{
			get
			{
				return this._InclQtyPOReceipts;
			}
			set
			{
				this._InclQtyPOReceipts = value;
			}
		}
		#endregion
		#region InclQtyPOPrepared
		public abstract class inclQtyPOPrepared : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyPOPrepared;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LotSerialStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyPOPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyPOPrepared
		{
			get
			{
				return this._InclQtyPOPrepared;
			}
			set
			{
				this._InclQtyPOPrepared = value;
			}
		}
		#endregion
		#region InclQtyPOOrders
		public abstract class inclQtyPOOrders : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyPOOrders;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LotSerialStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyPOOrders), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyPOOrders
		{
			get
			{
				return this._InclQtyPOOrders;
			}
			set
			{
				this._InclQtyPOOrders = value;
			}
		}
		#endregion
		#region InclQtyINIssues
		public abstract class inclQtyINIssues : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyINIssues;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LotSerialStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyINIssues), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyINIssues
		{
			get
			{
				return this._InclQtyINIssues;
			}
			set
			{
				this._InclQtyINIssues = value;
			}
		}
		#endregion
		#region InclQtyINReceipts
		public abstract class inclQtyINReceipts : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyINReceipts;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LotSerialStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyINReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyINReceipts
		{
			get
			{
				return this._InclQtyINReceipts;
			}
			set
			{
				this._InclQtyINReceipts = value;
			}
		}
		#endregion
		#region InclQtyINAssemblyDemand
		public abstract class inclQtyINAssemblyDemand : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyINAssemblyDemand;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LotSerialStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyINAssemblyDemand), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyINAssemblyDemand
		{
			get
			{
				return this._InclQtyINAssemblyDemand;
			}
			set
			{
				this._InclQtyINAssemblyDemand = value;
			}
		}
		#endregion
		#region InclQtyINAssemblySupply
		public abstract class inclQtyINAssemblySupply : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyINAssemblySupply;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LotSerialStatus.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyINAssemblySupply), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyINAssemblySupply
		{
			get
			{
				return this._InclQtyINAssemblySupply;
			}
			set
			{
				this._InclQtyINAssemblySupply = value;
			}
		}
		#endregion
	}

	[ItemLotSerialAccumulator]
    [Serializable]
	public partial class ItemLotSerial : INItemLotSerial, IQtyAllocatedBase
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXForeignSelector(typeof(INTran.inventoryID))]
        //[SerialNumberedStatusCheck(typeof(ItemLotSerial.qtyAvail), typeof(ItemLotSerial.inventoryID), typeof(ItemLotSerial.lotSerialNbr), typeof(ItemLotSerial.lotSerTrack), typeof(ItemLotSerial.lotSerAssign), typeof(ItemLotSerial.refNoteID), typeof(ItemLotSerial.isDuplicated))]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region LotSerialNbr
		public new abstract class lotSerialNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(100, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		public override String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
        #region RefNotID
        public abstract class refNoteID : PX.Data.IBqlField
        {
        }
        [PXLong()]
        public virtual Int64? RefNoteID
        {
            get;
            set;
        }
        #endregion
        #region IsDuplicated
        public abstract class isDuplicated : PX.Data.IBqlField
        {
        }
        [PXBool()]
        public virtual bool? IsDuplicated
        {
            get;
            set;
        }
        #endregion
		#region ItemClassID
		public abstract class itemClassID : PX.Data.IBqlField
		{
		}
		protected String _ItemClassID;
		[PXString(10, IsUnicode = true)]
		[PXDBScalar(typeof(Search<InventoryItem.itemClassID, Where<InventoryItem.inventoryID, Equal<INItemLotSerial.inventoryID>>>))]
		[PXDefault(typeof(Search<InventoryItem.itemClassID, Where<InventoryItem.inventoryID, Equal<Current<LotSerialStatus.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String ItemClassID
		{
			get
			{
				return this._ItemClassID;
			}
			set
			{
				this._ItemClassID = value;
			}
		}
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.IBqlField
		{
		}
		#endregion
		#region LotSerTrack
		public new abstract class lotSerTrack : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(Select2<INLotSerClass, InnerJoin<InventoryItem, On<InventoryItem.lotSerClassID, Equal<INLotSerClass.lotSerClassID>>>, Where<InventoryItem.inventoryID, Equal<Current<ItemLotSerial.inventoryID>>>>), SourceField = typeof(INLotSerClass.lotSerTrack), PersistingCheck = PXPersistingCheck.Nothing)]
		public override String LotSerTrack
		{
			get
			{
				return this._LotSerTrack;
			}
			set
			{
				this._LotSerTrack = value;
			}
		}
		#endregion
		#region LotSerAssign
		public new abstract class lotSerAssign : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(Select2<INLotSerClass, InnerJoin<InventoryItem, On<InventoryItem.lotSerClassID, Equal<INLotSerClass.lotSerClassID>>>, Where<InventoryItem.inventoryID, Equal<Current<ItemLotSerial.inventoryID>>>>), SourceField = typeof(INLotSerClass.lotSerAssign), PersistingCheck = PXPersistingCheck.Nothing)]
		public override String LotSerAssign
		{
			get
			{
				return this._LotSerAssign;
			}
			set
			{
				this._LotSerAssign = value;
			}
		}
		#endregion
		#region ExpireDate
		public new abstract class expireDate : PX.Data.IBqlField
		{
		}
		[PXDBDate()]
		public override DateTime? ExpireDate
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
		#region QtyNotAvail
		public abstract class qtyNotAvail : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyNotAvail;
		[PXDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? QtyNotAvail
		{
			get
			{
				return this._QtyNotAvail;
			}
			set
			{
				this._QtyNotAvail = value;
			}
		}
		#endregion
		#region NegQty
		public abstract class negQty : PX.Data.IBqlField
		{
		}
		protected bool? _NegQty;
		[PXBool()]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<LotSerialStatus.itemClassID>>>>), SourceField = typeof(INItemClass.negQty), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBScalar(typeof(Search2<INItemClass.negQty, InnerJoin<InventoryItem, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>, Where<InventoryItem.inventoryID, Equal<INItemLotSerial.inventoryID>>>))]
		public virtual bool? NegQty
		{
			get
			{
				return this._NegQty;
			}
			set
			{
				this._NegQty = value;
			}
		}
		#endregion
		#region InclQtyAvail
		public abstract class inclQtyAvail : PX.Data.IBqlField
		{
		}
		protected Boolean? _InclQtyAvail;
		[PXBool()]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? InclQtyAvail
		{
			get
			{
				return this._InclQtyAvail;
			}
			set
			{
				this._InclQtyAvail = value;
			}
		}
		#endregion
        #region InclQtySOReverse
        public abstract class inclQtySOReverse : PX.Data.IBqlField
        {
        }
        protected Boolean? _InclQtySOReverse;
        [PXBool()]
        [PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<ItemLotSerial.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtySOReverse), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOReverse
        {
            get
            {
                return this._InclQtySOReverse;
            }
            set
            {
                this._InclQtySOReverse = value;
            }
        }
        #endregion
        #region InclQtySOBackOrdered
        public abstract class inclQtySOBackOrdered : PX.Data.IBqlField
        {
        }
        protected Boolean? _InclQtySOBackOrdered;
        [PXBool()]
        [PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<ItemLotSerial.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtySOBackOrdered), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOBackOrdered
        {
            get
            {
                return this._InclQtySOBackOrdered;
            }
            set
            {
                this._InclQtySOBackOrdered = value;
            }
        }
        #endregion
        #region InclQtySOBooked
        public abstract class inclQtySOBooked : PX.Data.IBqlField
        {
        }
        protected Boolean? _InclQtySOBooked;
        [PXBool()]
        [PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<ItemLotSerial.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtySOBooked), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOBooked
        {
            get
            {
                return this._InclQtySOBooked;
            }
            set
            {
                this._InclQtySOBooked = value;
            }
        }
        #endregion
        #region InclQtySOShipped
        public abstract class inclQtySOShipped : PX.Data.IBqlField
        {
        }
        protected Boolean? _InclQtySOShipped;
        [PXBool()]
        [PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<ItemLotSerial.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtySOShipped), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOShipped
        {
            get
            {
                return this._InclQtySOShipped;
            }
            set
            {
                this._InclQtySOShipped = value;
            }
        }
        #endregion
        #region InclQtySOShipping
        public abstract class inclQtySOShipping : PX.Data.IBqlField
        {
        }
        protected Boolean? _InclQtySOShipping;
        [PXBool()]
        [PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<ItemLotSerial.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtySOShipping), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtySOShipping
        {
            get
            {
                return this._InclQtySOShipping;
            }
            set
            {
                this._InclQtySOShipping = value;
            }
        }
        #endregion
        #region InclQtyInTransit
        public abstract class inclQtyInTransit : PX.Data.IBqlField
        {
        }
        protected Boolean? _InclQtyInTransit;
        [PXBool()]
        [PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<ItemLotSerial.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyInTransit), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyInTransit
        {
            get
            {
                return this._InclQtyInTransit;
            }
            set
            {
                this._InclQtyInTransit = value;
            }
        }
        #endregion
        #region InclQtyPOReceipts
        public abstract class inclQtyPOReceipts : PX.Data.IBqlField
        {
        }
        protected Boolean? _InclQtyPOReceipts;
        [PXBool()]
        [PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<ItemLotSerial.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyPOReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOReceipts
        {
            get
            {
                return this._InclQtyPOReceipts;
            }
            set
            {
                this._InclQtyPOReceipts = value;
            }
        }
        #endregion
        #region InclQtyPOPrepared
        public abstract class inclQtyPOPrepared : PX.Data.IBqlField
        {
        }
        protected Boolean? _InclQtyPOPrepared;
        [PXBool()]
        [PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<ItemLotSerial.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyPOPrepared), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOPrepared
        {
            get
            {
                return this._InclQtyPOPrepared;
            }
            set
            {
                this._InclQtyPOPrepared = value;
            }
        }
        #endregion
        #region InclQtyPOOrders
        public abstract class inclQtyPOOrders : PX.Data.IBqlField
        {
        }
        protected Boolean? _InclQtyPOOrders;
        [PXBool()]
        [PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<ItemLotSerial.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyPOOrders), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyPOOrders
        {
            get
            {
                return this._InclQtyPOOrders;
            }
            set
            {
                this._InclQtyPOOrders = value;
            }
        }
        #endregion
        #region InclQtyINIssues
        public abstract class inclQtyINIssues : PX.Data.IBqlField
        {
        }
        protected Boolean? _InclQtyINIssues;
        [PXBool()]
        [PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<ItemLotSerial.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyINIssues), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINIssues
        {
            get
            {
                return this._InclQtyINIssues;
            }
            set
            {
                this._InclQtyINIssues = value;
            }
        }
        #endregion
        #region InclQtyINReceipts
        public abstract class inclQtyINReceipts : PX.Data.IBqlField
        {
        }
        protected Boolean? _InclQtyINReceipts;
        [PXBool()]
        [PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<ItemLotSerial.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyINReceipts), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINReceipts
        {
            get
            {
                return this._InclQtyINReceipts;
            }
            set
            {
                this._InclQtyINReceipts = value;
            }
        }
        #endregion
        #region InclQtyINAssemblyDemand
        public abstract class inclQtyINAssemblyDemand : PX.Data.IBqlField
        {
        }
        protected Boolean? _InclQtyINAssemblyDemand;
        [PXBool()]
        [PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<ItemLotSerial.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyINAssemblyDemand), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINAssemblyDemand
        {
            get
            {
                return this._InclQtyINAssemblyDemand;
            }
            set
            {
                this._InclQtyINAssemblyDemand = value;
            }
        }
        #endregion
        #region InclQtyINAssemblySupply
        public abstract class inclQtyINAssemblySupply : PX.Data.IBqlField
        {
        }
        protected Boolean? _InclQtyINAssemblySupply;
        [PXBool()]
        [PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<ItemLotSerial.itemClassID>>>>), SourceField = typeof(INItemClass.inclQtyINAssemblySupply), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? InclQtyINAssemblySupply
        {
            get
            {
                return this._InclQtyINAssemblySupply;
            }
            set
            {
                this._InclQtyINAssemblySupply = value;
            }
        }
        #endregion
    }


    [Serializable]
	public partial class ReadOnlyLotSerialStatus : INLotSerialStatus
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region SubItemID
		public new abstract class subItemID : PX.Data.IBqlField
		{
		}
		[SubItem(IsKey = true)]
		public override Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SiteID
		public new abstract class siteID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		public override Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region LocationID
		public new abstract class locationID : PX.Data.IBqlField
		{
		}
		[IN.Location(IsKey = true, ValidComboRequired = false)]
		[PXDefault()]
		public override Int32? LocationID
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
		#region LotSerialNbr
		public new abstract class lotSerialNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(100, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		public override String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region CostID
		public new abstract class costID : PX.Data.IBqlField
		{
		}
		[PXDBLong()]
		public override Int64? CostID
		{
			get
			{
				return this._CostID;
			}
			set
			{
				this._CostID = value;
			}
		}
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.IBqlField
		{
		}
		#endregion
		#region LotSerTrack
		public new abstract class lotSerTrack : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		public override String LotSerTrack
		{
			get
			{
				return this._LotSerTrack;
			}
			set
			{
				this._LotSerTrack = value;
			}
		}
		#endregion
		#region ExpireDate
		public new abstract class expireDate : PX.Data.IBqlField
		{
		}
		protected new DateTime? _ExpireDate;
		[PXDBDate(BqlField = typeof(INItemLotSerial.expireDate))]
		[PXUIField(DisplayName = "Expiry Date")]
		public override DateTime? ExpireDate
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
		#region ReceiptDate
		public new abstract class receiptDate : PX.Data.IBqlField
		{
		}
		[PXDBDate()]
		public override DateTime? ReceiptDate
		{
			get
			{
				return this._ReceiptDate;
			}
			set
			{
				this._ReceiptDate = value;
			}
		}
		#endregion
	}

    [Serializable]
	public partial class ReadOnlyCostStatus : INCostStatus
	{
		#region CostID
		public new abstract class costID : PX.Data.IBqlField
		{
		}
		[PXDBLong(IsKey = true)]
		[PXDefault()]
		public override Int64? CostID
		{
			get
			{
				return this._CostID;
			}
			set
			{
				this._CostID = value;
			}
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region CostSubItemID
		public new abstract class costSubItemID : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
		[PXDefault()]
		public override Int32? CostSubItemID
		{
			get
			{
				return this._CostSubItemID;
			}
			set
			{
				this._CostSubItemID = value;
			}
		}
		#endregion
		#region CostSiteID
		public new abstract class costSiteID : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
		[PXDefault()]
		public override Int32? CostSiteID
		{
			get
			{
				return this._CostSiteID;
			}
			set
			{
				this._CostSiteID = value;
			}
		}
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
		[PXDefault()]
		public override Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
		[PXDefault()]
		public override Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region LayerType
		public new abstract class layerType : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault()]
		public override String LayerType
		{
			get
			{
				return this._LayerType;
			}
			set
			{
				this._LayerType = value;
			}
		}
		#endregion
		#region ValMethod
		public new abstract class valMethod : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault()]
		public override String ValMethod
		{
			get
			{
				return this._ValMethod;
			}
			set
			{
				this._ValMethod = value;
			}
		}
		#endregion
		#region ReceiptNbr
		public new abstract class receiptNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true)]
		[PXDefault()]
		public override String ReceiptNbr
		{
			get
			{
				return this._ReceiptNbr;
			}
			set
			{
				this._ReceiptNbr = value;
			}
		}
		#endregion
		#region ReceiptDate
		public new abstract class receiptDate : PX.Data.IBqlField
		{
		}
		[PXDBDate()]
		[PXDefault()]
		public override DateTime? ReceiptDate
		{
			get
			{
				return this._ReceiptDate;
			}
			set
			{
				this._ReceiptDate = value;
			}
		}
		#endregion
		#region LotSerialNbr
		public new abstract class lotSerialNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(100, IsUnicode = true)]
		public override String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.IBqlField
		{
		}
		#endregion
	}

	[CostStatusAccumulator(typeof(OversoldCostStatus.qtyOnHand), typeof(OversoldCostStatus.totalCost), typeof(OversoldCostStatus.inventoryID), typeof(OversoldCostStatus.costSubItemID), typeof(OversoldCostStatus.costSiteID), typeof(OversoldCostStatus.layerType))]
    [Serializable]
	public partial class OversoldCostStatus : INCostStatus
	{
		#region CostID
		public new abstract class costID : PX.Data.IBqlField
		{
		}
		[CostIdentity(typeof(INTranCost.costID))]
		[PXDefault()]
		public override Int64? CostID
		{
			get
			{
				return this._CostID;
			}
			set
			{
				this._CostID = value;
			}
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXForeignSelector(typeof(INTran.inventoryID))]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region CostSubItemID
		public new abstract class costSubItemID : PX.Data.IBqlField
		{
		}
		[SubItem(IsKey = true)]
		[PXDefault()]
		public override Int32? CostSubItemID
		{
			get
			{
				return this._CostSubItemID;
			}
			set
			{
				this._CostSubItemID = value;
			}
		}
		#endregion
		#region CostSiteID
		public new abstract class costSiteID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[CostSiteID()]
		[PXDefault()]
		public override Int32? CostSiteID
		{
			get
			{
				return this._CostSiteID;
			}
			set
			{
				this._CostSiteID = value;
			}
		}
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
		[PXDefault()]
		public override Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.IBqlField
		{
		}
		[SubAccount()]
		[PXDefault()]
		public override Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region LayerType
		public new abstract class layerType : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true, IsKey = true)]
		[PXDefault(INLayerType.Oversold)]
		public override String LayerType
		{
			get
			{
				return this._LayerType;
			}
			set
			{
				this._LayerType = value;
			}
		}
		#endregion
		#region ValMethod
		public new abstract class valMethod : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault()]
		public override String ValMethod
		{
			get
			{
				return this._ValMethod;
			}
			set
			{
				this._ValMethod = value;
			}
		}
		#endregion
		#region ReceiptNbr
		public new abstract class receiptNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true)]
		[PXDefault("OVERSOLD")]
		public override String ReceiptNbr
		{
			get
			{
				return this._ReceiptNbr;
			}
			set
			{
				this._ReceiptNbr = value;
			}
		}
		#endregion
		#region ReceiptDate
		public new abstract class receiptDate : PX.Data.IBqlField
		{
		}
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		public override DateTime? ReceiptDate
		{
			get
			{
				return this._ReceiptDate;
			}
			set
			{
				this._ReceiptDate = value;
			}
		}
		#endregion
		#region LotSerialNbr
		public new abstract class lotSerialNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(100, IsUnicode = true)]
		public override String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region OrigQty
		public new abstract class origQty : PX.Data.IBqlField
		{
		}
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.IBqlField
		{
		}
		#endregion
		#region UnitCost
		public new abstract class unitCost : PX.Data.IBqlField
		{
		}
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search2<INItemSite.negativeCost, InnerJoin<INLocation, On<INLocation.siteID, Equal<INItemSite.siteID>>>, Where<INItemSite.inventoryID, Equal<Current<OversoldCostStatus.inventoryID>>, And<Where<INItemSite.siteID, Equal<Current<OversoldCostStatus.costSiteID>>, Or<INLocation.locationID, Equal<Current<OversoldCostStatus.costSiteID>>>>>>>))]
		public override Decimal? UnitCost
		{
			get
			{
				return this._UnitCost;
			}
			set
			{
				this._UnitCost = value;
			}
		}
		#endregion
	}

	[CostStatusAccumulator(typeof(AverageCostStatus.qtyOnHand), typeof(AverageCostStatus.totalCost), typeof(AverageCostStatus.inventoryID), typeof(AverageCostStatus.costSubItemID), typeof(AverageCostStatus.costSiteID), typeof(AverageCostStatus.layerType))]
    [Serializable]
	public partial class AverageCostStatus : INCostStatus
	{
		#region CostID
		public new abstract class costID : PX.Data.IBqlField
		{
		}
		[CostIdentity(typeof(INTranCost.costID))]
		[PXDefault()]
		public override Int64? CostID
		{
			get
			{
				return this._CostID;
			}
			set
			{
				this._CostID = value;
			}
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXForeignSelector(typeof(INTran.inventoryID))]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region CostSubItemID
		public new abstract class costSubItemID : PX.Data.IBqlField
		{
		}
		[SubItem(IsKey = true)]
		[PXDefault()]
		public override Int32? CostSubItemID
		{
			get
			{
				return this._CostSubItemID;
			}
			set
			{
				this._CostSubItemID = value;
			}
		}
		#endregion
		#region CostSiteID
		public new abstract class costSiteID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[CostSiteID()]
		[PXDefault()]
		public override Int32? CostSiteID
		{
			get
			{
				return this._CostSiteID;
			}
			set
			{
				this._CostSiteID = value;
			}
		}
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.IBqlField
		{
		}
		[SubAccount(IsKey = true)]
		[PXDefault()]
		public override Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region LayerType
		public new abstract class layerType : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true, IsKey = true)]
		[PXDefault()]
		public override String LayerType
		{
			get
			{
				return this._LayerType;
			}
			set
			{
				this._LayerType = value;
			}
		}
		#endregion
		#region ValMethod
		public new abstract class valMethod : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INValMethod.Average)]
		public override String ValMethod
		{
			get
			{
				return this._ValMethod;
			}
			set
			{
				this._ValMethod = value;
			}
		}
		#endregion
		#region ReceiptNbr
		public new abstract class receiptNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true)]
		[PXDefault("ZZZ")]
		public override String ReceiptNbr
		{
			get
			{
				return this._ReceiptNbr;
			}
			set
			{
				this._ReceiptNbr = value;
			}
		}
		#endregion
		#region ReceiptDate
		public new abstract class receiptDate : PX.Data.IBqlField
		{
		}
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		public override DateTime? ReceiptDate
		{
			get
			{
				return this._ReceiptDate;
			}
			set
			{
				this._ReceiptDate = value;
			}
		}
		#endregion
		#region LotSerialNbr
		public new abstract class lotSerialNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(100, IsUnicode = true)]
		public override String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region OrigQty
		public new abstract class origQty : PX.Data.IBqlField
		{
		}
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.IBqlField
		{
		}
		#endregion
	}

	[CostStatusAccumulator(typeof(StandardCostStatus.qtyOnHand), typeof(StandardCostStatus.totalCost), typeof(StandardCostStatus.inventoryID), typeof(StandardCostStatus.costSubItemID), typeof(StandardCostStatus.costSiteID), typeof(StandardCostStatus.costSiteID), typeof(StandardCostStatus.layerType))]
    [Serializable]
	public partial class StandardCostStatus : INCostStatus
	{
		#region CostID
		public new abstract class costID : PX.Data.IBqlField
		{
		}
		[CostIdentity(typeof(INTranCost.costID))]
		[PXDefault()]
		public override Int64? CostID
		{
			get
			{
				return this._CostID;
			}
			set
			{
				this._CostID = value;
			}
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXForeignSelector(typeof(INTran.inventoryID))]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region CostSubItemID
		public new abstract class costSubItemID : PX.Data.IBqlField
		{
		}
		[SubItem(IsKey = true)]
		[PXDefault()]
		public override Int32? CostSubItemID
		{
			get
			{
				return this._CostSubItemID;
			}
			set
			{
				this._CostSubItemID = value;
			}
		}
		#endregion
		#region CostSiteID
		public new abstract class costSiteID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[CostSiteID()]
		[PXDefault()]
		public override Int32? CostSiteID
		{
			get
			{
				return this._CostSiteID;
			}
			set
			{
				this._CostSiteID = value;
			}
		}
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.IBqlField
		{
		}
		[SubAccount(IsKey = true)]
		[PXDefault()]
		public override Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region LayerType
		public new abstract class layerType : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true, IsKey = true)]
		[PXDefault()]
		public override String LayerType
		{
			get
			{
				return this._LayerType;
			}
			set
			{
				this._LayerType = value;
			}
		}
		#endregion
		#region ValMethod
		public new abstract class valMethod : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INValMethod.Standard)]
		public override String ValMethod
		{
			get
			{
				return this._ValMethod;
			}
			set
			{
				this._ValMethod = value;
			}
		}
		#endregion
		#region ReceiptNbr
		public new abstract class receiptNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true)]
		[PXDefault("ZZZ")]
		public override String ReceiptNbr
		{
			get
			{
				return this._ReceiptNbr;
			}
			set
			{
				this._ReceiptNbr = value;
			}
		}
		#endregion
		#region ReceiptDate
		public new abstract class receiptDate : PX.Data.IBqlField
		{
		}
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		public override DateTime? ReceiptDate
		{
			get
			{
				return this._ReceiptDate;
			}
			set
			{
				this._ReceiptDate = value;
			}
		}
		#endregion
		#region LotSerialNbr
		public new abstract class lotSerialNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(100, IsUnicode = true)]
		public override String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region OrigQty
		public new abstract class origQty : PX.Data.IBqlField
		{
		}
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.IBqlField
		{
		}
		#endregion
		#region UnitCost
		public new abstract class unitCost : PX.Data.IBqlField
		{
		}
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? UnitCost
		{
			get
			{
				return this._UnitCost;
			}
			set
			{
				this._UnitCost = value;
			}
		}
		#endregion
	}

	[CostStatusAccumulator(typeof(FIFOCostStatus.qtyOnHand), typeof(FIFOCostStatus.totalCost), typeof(FIFOCostStatus.inventoryID), typeof(FIFOCostStatus.costSubItemID), typeof(FIFOCostStatus.costSiteID), typeof(FIFOCostStatus.receiptNbr), typeof(FIFOCostStatus.layerType))]
    [Serializable]
	public partial class FIFOCostStatus : INCostStatus
	{
		#region CostID
		public new abstract class costID : PX.Data.IBqlField
		{
		}
		[CostIdentity(typeof(INTranCost.costID))]
		[PXDefault()]
		public override Int64? CostID
		{
			get
			{
				return this._CostID;
			}
			set
			{
				this._CostID = value;
			}
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXForeignSelector(typeof(INTran.inventoryID))]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region CostSubItemID
		public new abstract class costSubItemID : PX.Data.IBqlField
		{
		}
		[SubItem(IsKey = true)]
		[PXDefault()]
		public override Int32? CostSubItemID
		{
			get
			{
				return this._CostSubItemID;
			}
			set
			{
				this._CostSubItemID = value;
			}
		}
		#endregion
		#region CostSiteID
		public new abstract class costSiteID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[CostSiteID()]
		[PXDefault()]
		public override Int32? CostSiteID
		{
			get
			{
				return this._CostSiteID;
			}
			set
			{
				this._CostSiteID = value;
			}
		}
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.IBqlField
		{
		}
		[SubAccount(IsKey = true)]
		[PXDefault()]
		public override Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region LayerType
		public new abstract class layerType : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true, IsKey = true)]
		[PXDefault()]
		public override String LayerType
		{
			get
			{
				return this._LayerType;
			}
			set
			{
				this._LayerType = value;
			}
		}
		#endregion
		#region ValMethod
		public new abstract class valMethod : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INValMethod.FIFO)]
		public override String ValMethod
		{
			get
			{
				return this._ValMethod;
			}
			set
			{
				this._ValMethod = value;
			}
		}
		#endregion
		#region ReceiptNbr
		public new abstract class receiptNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		public override String ReceiptNbr
		{
			get
			{
				return this._ReceiptNbr;
			}
			set
			{
				this._ReceiptNbr = value;
			}
		}
		#endregion
		#region ReceiptDate
		public new abstract class receiptDate : PX.Data.IBqlField
		{
		}
		[PXDBDate()]
		[PXDefault()]
		public override DateTime? ReceiptDate
		{
			get
			{
				return this._ReceiptDate;
			}
			set
			{
				this._ReceiptDate = value;
			}
		}
		#endregion
		#region LotSerialNbr
		public new abstract class lotSerialNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(100, IsUnicode = true)]
		public override String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region OrigQty
		public new abstract class origQty : PX.Data.IBqlField
		{
		}
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.IBqlField
		{
		}
		#endregion
	}

	[CostStatusAccumulator(typeof(SpecificCostStatus.qtyOnHand), typeof(SpecificCostStatus.totalCost), typeof(SpecificCostStatus.inventoryID), typeof(SpecificCostStatus.costSubItemID), typeof(SpecificCostStatus.costSiteID), typeof(SpecificCostStatus.lotSerialNbr), typeof(SpecificCostStatus.layerType))]
    [Serializable]
	public partial class SpecificCostStatus : INCostStatus
	{
		#region CostID
		public new abstract class costID : PX.Data.IBqlField
		{
		}
		[CostIdentity(typeof(INTranCost.costID), typeof(LotSerialStatus.costID))]
		[PXDefault()]
		public override Int64? CostID
		{
			get
			{
				return this._CostID;
			}
			set
			{
				this._CostID = value;
			}
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXForeignSelector(typeof(INTran.inventoryID))]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region CostSubItemID
		public new abstract class costSubItemID : PX.Data.IBqlField
		{
		}
		[SubItem(IsKey = true)]
		[PXDefault()]
		public override Int32? CostSubItemID
		{
			get
			{
				return this._CostSubItemID;
			}
			set
			{
				this._CostSubItemID = value;
			}
		}
		#endregion
		#region CostSiteID
		public new abstract class costSiteID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[CostSiteID()]
		[PXDefault()]
		public override Int32? CostSiteID
		{
			get
			{
				return this._CostSiteID;
			}
			set
			{
				this._CostSiteID = value;
			}
		}
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
		[PXDefault()]
		public override Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.IBqlField
		{
		}
		[SubAccount()]
		[PXDefault()]
		public override Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region LayerType
		public new abstract class layerType : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault()]
		public override String LayerType
		{
			get
			{
				return this._LayerType;
			}
			set
			{
				this._LayerType = value;
			}
		}
		#endregion
		#region ValMethod
		public new abstract class valMethod : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INValMethod.Specific)]
		public override String ValMethod
		{
			get
			{
				return this._ValMethod;
			}
			set
			{
				this._ValMethod = value;
			}
		}
		#endregion
		#region ReceiptNbr
		public new abstract class receiptNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true)]
		[PXDefault("ZZZ")]
		public override String ReceiptNbr
		{
			get
			{
				return this._ReceiptNbr;
			}
			set
			{
				this._ReceiptNbr = value;
			}
		}
		#endregion
		#region ReceiptDate
		public new abstract class receiptDate : PX.Data.IBqlField
		{
		}
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		public override DateTime? ReceiptDate
		{
			get
			{
				return this._ReceiptDate;
			}
			set
			{
				this._ReceiptDate = value;
			}
		}
		#endregion
		#region LotSerialNbr
		public new abstract class lotSerialNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(100, IsUnicode = true, IsKey = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Null)]
		public override String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region OrigQty
		public new abstract class origQty : PX.Data.IBqlField
		{
		}
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.IBqlField
		{
		}
		#endregion
	}

	[ReceiptStatusAccumulator()]
    [Serializable]
	public partial class ReceiptStatus : INReceiptStatus
	{
		#region ReceiptID
		public new abstract class receiptID : PX.Data.IBqlField
		{
		}
		[PXDBLongIdentity()]
		[PXDefault()]
		public override Int64? ReceiptID
		{
			get
			{
				return this._ReceiptID;
			}
			set
			{
				this._ReceiptID = value;
			}
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region CostSubItemID
		public new abstract class costSubItemID : PX.Data.IBqlField
		{
		}
		[SubItem(IsKey = true)]
		[PXDefault()]
		public override Int32? CostSubItemID
		{
			get
			{
				return this._CostSubItemID;
			}
			set
			{
				this._CostSubItemID = value;
			}
		}
		#endregion
		#region CostSiteID
		public new abstract class costSiteID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? CostSiteID
		{
			get
			{
				return this._CostSiteID;
			}
			set
			{
				this._CostSiteID = value;
			}
		}
		#endregion
		#region LayerType
		public new abstract class layerType : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INLayerType.Normal)]
		public override String LayerType
		{
			get
			{
				return this._LayerType;
			}
			set
			{
				this._LayerType = value;
			}
		}
		#endregion
		#region ValMethod
		public new abstract class valMethod : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INValMethod.FIFO)]
		public override String ValMethod
		{
			get
			{
				return this._ValMethod;
			}
			set
			{
				this._ValMethod = value;
			}
		}
		#endregion
		#region ReceiptNbr
		public new abstract class receiptNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		public override String ReceiptNbr
		{
			get
			{
				return this._ReceiptNbr;
			}
			set
			{
				this._ReceiptNbr = value;
			}
		}
		#endregion
		#region ReceiptDate
		public new abstract class receiptDate : PX.Data.IBqlField
		{
		}
		[PXDBDate()]
		[PXDefault()]
		public override DateTime? ReceiptDate
		{
			get
			{
				return this._ReceiptDate;
			}
			set
			{
				this._ReceiptDate = value;
			}
		}
		#endregion
		#region OrigQty
		public new abstract class origQty : PX.Data.IBqlField
		{
		}
		#endregion
	}

	[PXAccumulator(new Type[] {
				typeof(ItemSiteHist.finYtdQty),
				typeof(ItemSiteHist.finYtdQty),

				typeof(ItemSiteHist.tranYtdQty),
				typeof(ItemSiteHist.tranYtdQty)

				},
					new Type[] {
				typeof(ItemSiteHist.finBegQty),
				typeof(ItemSiteHist.finYtdQty),

				typeof(ItemSiteHist.tranBegQty),
				typeof(ItemSiteHist.tranYtdQty)

				}
			)]
    [PXDisableCloneAttributes()]
    [Serializable]
	public partial class ItemSiteHist : INItemSiteHist
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region SubItemID
		public new abstract class subItemID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SiteID
		public new abstract class siteID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region LocationID
		public new abstract class locationID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? LocationID
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
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		[PXDBString(6, IsKey = true, IsFixed = true)]
		[PXDefault()]
		public override String FinPeriodID
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

    public class ItemSiteHistDAccumAttribute : PXAccumulatorAttribute
    {
        public ItemSiteHistDAccumAttribute()
        {
            _SingleRecord = true;
        }

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            ItemSiteHistD bal = (ItemSiteHistD)row;
            columns.Update<ItemSiteHistD.qtyReceived>(bal.QtyReceived, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSiteHistD.qtyIssued>(bal.QtyIssued, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSiteHistD.qtySales>(bal.QtySales, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSiteHistD.qtyCreditMemos>(bal.QtyCreditMemos, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSiteHistD.qtyDropShipSales>(bal.QtyDropShipSales, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSiteHistD.qtyTransferIn>(bal.QtyTransferIn, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSiteHistD.qtyTransferOut>(bal.QtyTransferOut, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSiteHistD.qtyAssemblyIn>(bal.QtyAssemblyIn, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSiteHistD.qtyAssemblyOut>(bal.QtyAssemblyOut, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSiteHistD.qtyAdjusted>(bal.QtyAdjusted, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ItemSiteHistD.sDay>(bal.SDay, PXDataFieldAssign.AssignBehavior.Replace);
            columns.Update<ItemSiteHistD.sMonth>(bal.SMonth, PXDataFieldAssign.AssignBehavior.Replace);
            columns.Update<ItemSiteHistD.sYear>(bal.SYear, PXDataFieldAssign.AssignBehavior.Replace);
            columns.Update<ItemSiteHistD.sQuater>(bal.SQuater, PXDataFieldAssign.AssignBehavior.Replace);
            columns.Update<ItemSiteHistD.sDayOfWeek>(bal.SDayOfWeek, PXDataFieldAssign.AssignBehavior.Replace);

            return true;
        }
    }

    [ItemSiteHistDAccum(SingleRecord = true)]
    [Serializable]
    public partial class ItemSiteHistD : INItemSiteHistD
    {
        #region InventoryID
        public new abstract class inventoryID : PX.Data.IBqlField
        {
        }
        [Inventory(IsKey = true)]
        [PXDefault]
        public override Int32? InventoryID
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
        #region SubItemID
        public new abstract class subItemID : PX.Data.IBqlField
        {
        }
        [SubItem(IsKey = true)]
        [PXDefault]
        public override Int32? SubItemID
        {
            get
            {
                return this._SubItemID;
            }
            set
            {
                this._SubItemID = value;
            }
        }
        #endregion
        #region SiteID
        public new abstract class siteID : PX.Data.IBqlField
        {
        }
        [Site(IsKey = true)]
        [PXDefault]
        public override Int32? SiteID
        {
            get
            {
                return this._SiteID;
            }
            set
            {
                this._SiteID = value;
            }
        }
        #endregion
        #region SDate
        public new abstract class sDate : PX.Data.IBqlField
        {
        }
        [PXDBDate(IsKey = true)]
        public override DateTime? SDate
        {
            get
            {
                return this._SDate;
            }
            set
            {
                this._SDate = value;
            }
        }
        #endregion
    }

	[PXAccumulator(new Type[] 
		{
			typeof(ItemCostHist.finYtdQty),
			typeof(ItemCostHist.finYtdQty),
			typeof(ItemCostHist.finYtdCost),
			typeof(ItemCostHist.finYtdCost),

			typeof(ItemCostHist.tranYtdQty),
			typeof(ItemCostHist.tranYtdQty),
			typeof(ItemCostHist.tranYtdCost),
			typeof(ItemCostHist.tranYtdCost)

		}, new Type[] 
		{
			typeof(ItemCostHist.finBegQty),
			typeof(ItemCostHist.finYtdQty),
			typeof(ItemCostHist.finBegCost),
			typeof(ItemCostHist.finYtdCost),

			typeof(ItemCostHist.tranBegQty),
			typeof(ItemCostHist.tranYtdQty),
			typeof(ItemCostHist.tranBegCost),
			typeof(ItemCostHist.tranYtdCost)
		}
		)]
    [PXDisableCloneAttributes()]
    [Serializable]
	public partial class ItemCostHist : INItemCostHist
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region CostSubItemID
		public new abstract class costSubItemID : PX.Data.IBqlField
		{
		}
		[SubItem(IsKey = true)]
		[PXDefault()]
		public override Int32? CostSubItemID
		{
			get
			{
				return this._CostSubItemID;
			}
			set
			{
				this._CostSubItemID = value;
			}
		}
		#endregion
		#region CostSiteID
		public new abstract class costSiteID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? CostSiteID
		{
			get
			{
				return this._CostSiteID;
			}
			set
			{
				this._CostSiteID = value;
			}
		}
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.IBqlField
		{
		}
		[SubAccount(IsKey = true)]
		[PXDefault()]
		public override Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		[PXDBString(6, IsKey = true, IsFixed = true)]
		[PXDefault()]
		public override String FinPeriodID
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

	public class ItemSalesHistAccumAttribute : PXAccumulatorAttribute
	{
		public ItemSalesHistAccumAttribute()
			: base(new Type[] 
		{
			typeof(ItemSalesHist.finYtdSales),
			typeof(ItemSalesHist.finYtdCreditMemos),
			typeof(ItemSalesHist.finYtdDropShipSales),
			typeof(ItemSalesHist.finYtdCOGS),
			typeof(ItemSalesHist.finYtdCOGSCredits),
			typeof(ItemSalesHist.finYtdCOGSDropShips),
			typeof(ItemSalesHist.finYtdQtySales),
			typeof(ItemSalesHist.finYtdQtyCreditMemos),
			typeof(ItemSalesHist.finYtdQtyDropShipSales),

			typeof(ItemSalesHist.tranYtdSales),
			typeof(ItemSalesHist.tranYtdCreditMemos),
			typeof(ItemSalesHist.tranYtdDropShipSales),
			typeof(ItemSalesHist.tranYtdCOGS),
			typeof(ItemSalesHist.tranYtdCOGSCredits),
			typeof(ItemSalesHist.tranYtdCOGSDropShips),
			typeof(ItemSalesHist.tranYtdQtySales),
			typeof(ItemSalesHist.tranYtdQtyCreditMemos),
			typeof(ItemSalesHist.tranYtdQtyDropShipSales)
		}, new Type[] 
		{
			typeof(ItemSalesHist.finYtdSales),
			typeof(ItemSalesHist.finYtdCreditMemos),
			typeof(ItemSalesHist.finYtdDropShipSales),
			typeof(ItemSalesHist.finYtdCOGS),
			typeof(ItemSalesHist.finYtdCOGSCredits),
			typeof(ItemSalesHist.finYtdCOGSDropShips),
			typeof(ItemSalesHist.finYtdQtySales),
			typeof(ItemSalesHist.finYtdQtyCreditMemos),
			typeof(ItemSalesHist.finYtdQtyDropShipSales),

			typeof(ItemSalesHist.tranYtdSales),
			typeof(ItemSalesHist.tranYtdCreditMemos),
			typeof(ItemSalesHist.tranYtdDropShipSales),
			typeof(ItemSalesHist.tranYtdCOGS),
			typeof(ItemSalesHist.tranYtdCOGSCredits),
			typeof(ItemSalesHist.tranYtdCOGSDropShips),
			typeof(ItemSalesHist.tranYtdQtySales),
			typeof(ItemSalesHist.tranYtdQtyCreditMemos),
			typeof(ItemSalesHist.tranYtdQtyDropShipSales)
		}
		)
		{
		}

		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			ItemSalesHist hist = (ItemSalesHist)row;

			columns.RestrictPast<ItemSalesHist.finPeriodID>(PXComp.GE, hist.FinPeriodID.Substring(0, 4) + "01");
			columns.RestrictFuture<ItemSalesHist.finPeriodID>(PXComp.LE, hist.FinPeriodID.Substring(0, 4) + "99");

			return true;
		}
	}

	[ItemSalesHistAccum()]
    [PXDisableCloneAttributes()]

    [Serializable]
	public partial class ItemSalesHist : INItemSalesHist
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region CostSubItemID
		public new abstract class costSubItemID : PX.Data.IBqlField
		{
		}
		[SubItem(IsKey = true)]
		[PXDefault()]
		public override Int32? CostSubItemID
		{
			get
			{
				return this._CostSubItemID;
			}
			set
			{
				this._CostSubItemID = value;
			}
		}
		#endregion
		#region CostSiteID
		public new abstract class costSiteID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? CostSiteID
		{
			get
			{
				return this._CostSiteID;
			}
			set
			{
				this._CostSiteID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		[PXDBString(6, IsKey = true, IsFixed = true)]
		[PXDefault()]
		public override String FinPeriodID
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

	[ItemSalesHistDAccumAttribute(SingleRecord = true)]
    [PXDisableCloneAttributes()]
    [Serializable]
	public partial class ItemSalesHistD : INItemSalesHistD
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[Inventory(IsKey = true)]
		[PXDefault]
		public override Int32? InventoryID
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
		#region SubItemID
		public new abstract class subItemID : PX.Data.IBqlField
		{
		}
		[SubItem(IsKey = true)]
		[PXDefault]
		public override Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SiteID
		public new abstract class siteID : PX.Data.IBqlField
		{
		}
		[Site(IsKey = true)]
		[PXDefault]
		public override Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region SDate
		public new abstract class sDate : PX.Data.IBqlField
		{
		}
		[PXDBDate(IsKey = true)]
		public override DateTime? SDate
		{
			get
			{
				return this._SDate;
			}
			set
			{
				this._SDate = value;
			}
		}
		#endregion
		#region SYear
		public new abstract class sYear : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		public override int? SYear
		{
			get
			{
				return this._SYear;
			}
			set
			{
				this._SYear = value;
			}
		}
		#endregion
		#region SMonth
		public new abstract class sMonth : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		public override int? SMonth
		{
			get
			{
				return this._SMonth;
			}
			set
			{
				this._SMonth = value;
			}
		}
		#endregion
		#region SDay
		public new abstract class sDay : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		public override int? SDay
		{
			get
			{
				return this._SDay;
			}
			set
			{
				this._SDay = value;
			}
		}
		#endregion
	}
	public class ItemSalesHistDAccumAttribute : PXAccumulatorAttribute
	{
		public ItemSalesHistDAccumAttribute()
		{
			this._SingleRecord = true;
		}

		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			ItemSalesHistD bal = (ItemSalesHistD)row;
			columns.Update<ItemSalesHistD.qtyExcluded>(bal.QtyExcluded, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<ItemSalesHistD.qtyIssues>(bal.QtyIssues, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<ItemSalesHistD.qtyLostSales>(bal.QtyLostSales, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<ItemSalesHistD.qtyPlanSales>(bal.QtyPlanSales, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<ItemSalesHistD.demandType1>(bal.DemandType1, PXDataFieldAssign.AssignBehavior.Replace);
			columns.Update<ItemSalesHistD.demandType2>(bal.DemandType2, PXDataFieldAssign.AssignBehavior.Replace);
			columns.Update<ItemSalesHistD.sQuater>(bal.SQuater, PXDataFieldAssign.AssignBehavior.Replace);
			columns.Update<ItemSalesHistD.sDayOfWeek>(bal.SDayOfWeek, PXDataFieldAssign.AssignBehavior.Replace);

			return true;
		}
	}
	public class ItemCustSalesHistAccumAttribute : PXAccumulatorAttribute
	{
		public ItemCustSalesHistAccumAttribute()
			: base(new Type[] 
		{
			typeof(ItemCustSalesHist.finYtdSales),
			typeof(ItemCustSalesHist.finYtdCreditMemos),
			typeof(ItemCustSalesHist.finYtdDropShipSales),
			typeof(ItemCustSalesHist.finYtdCOGS),
			typeof(ItemCustSalesHist.finYtdCOGSCredits),
			typeof(ItemCustSalesHist.finYtdCOGSDropShips),
			typeof(ItemCustSalesHist.finYtdQtySales),
			typeof(ItemCustSalesHist.finYtdQtyCreditMemos),
			typeof(ItemCustSalesHist.finYtdQtyDropShipSales),

			typeof(ItemCustSalesHist.tranYtdSales),
			typeof(ItemCustSalesHist.tranYtdCreditMemos),
			typeof(ItemCustSalesHist.tranYtdDropShipSales),
			typeof(ItemCustSalesHist.tranYtdCOGS),
			typeof(ItemCustSalesHist.tranYtdCOGSCredits),
			typeof(ItemCustSalesHist.tranYtdCOGSDropShips),
			typeof(ItemCustSalesHist.tranYtdQtySales),
			typeof(ItemCustSalesHist.tranYtdQtyCreditMemos),
			typeof(ItemCustSalesHist.tranYtdQtyDropShipSales)
		}, new Type[] 
		{
			typeof(ItemCustSalesHist.finYtdSales),
			typeof(ItemCustSalesHist.finYtdCreditMemos),
			typeof(ItemCustSalesHist.finYtdDropShipSales),
			typeof(ItemCustSalesHist.finYtdCOGS),
			typeof(ItemCustSalesHist.finYtdCOGSCredits),
			typeof(ItemCustSalesHist.finYtdCOGSDropShips),
			typeof(ItemCustSalesHist.finYtdQtySales),
			typeof(ItemCustSalesHist.finYtdQtyCreditMemos),
			typeof(ItemCustSalesHist.finYtdQtyDropShipSales),

			typeof(ItemCustSalesHist.tranYtdSales),
			typeof(ItemCustSalesHist.tranYtdCreditMemos),
			typeof(ItemCustSalesHist.tranYtdDropShipSales),
			typeof(ItemCustSalesHist.tranYtdCOGS),
			typeof(ItemCustSalesHist.tranYtdCOGSCredits),
			typeof(ItemCustSalesHist.tranYtdCOGSDropShips),
			typeof(ItemCustSalesHist.tranYtdQtySales),
			typeof(ItemCustSalesHist.tranYtdQtyCreditMemos),
			typeof(ItemCustSalesHist.tranYtdQtyDropShipSales)
		}
		)
		{
		}

		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			ItemCustSalesHist hist = (ItemCustSalesHist)row;

			columns.RestrictPast<ItemCustSalesHist.finPeriodID>(PXComp.GE, hist.FinPeriodID.Substring(0, 4) + "01");
			columns.RestrictFuture<ItemCustSalesHist.finPeriodID>(PXComp.LE, hist.FinPeriodID.Substring(0, 4) + "99");

			return true;
		}
	}

	[ItemCustSalesStatsAccum()]
    [PXDisableCloneAttributes()]
    [Serializable]
	public partial class ItemCustSalesStats : INItemCustSalesStats
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region SubItemID
		public new abstract class subItemID : PX.Data.IBqlField
		{
		}
		[SubItem(IsKey = true)]
		[PXDefault()]
		public override Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SiteID
		public new abstract class siteID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region BAccountID
		public new abstract class bAccountID : PX.Data.IBqlField
		{
		}
		protected new Int32? _BAccountID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
	}

	public class ItemCustSalesStatsAccumAttribute : PXAccumulatorAttribute
	{
		public ItemCustSalesStatsAccumAttribute()
		{
			this._SingleRecord = true;
		}

		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			ItemCustSalesStats bal = (ItemCustSalesStats)row;
			columns.Update<ItemCustSalesStats.lastDate>(bal.LastDate, PXDataFieldAssign.AssignBehavior.Maximize);
			columns.Update<ItemCustSalesStats.lastUnitPrice>(bal.LastUnitPrice, PXDataFieldAssign.AssignBehavior.Replace);
			columns.Update<ItemCustSalesStats.lastQty>(bal.LastQty, PXDataFieldAssign.AssignBehavior.Replace);
			columns.Restrict<ItemCustSalesStats.lastDate>(PXComp.LE, bal.LastDate);

			return true;
		}

		public override bool PersistInserted(PXCache sender, object row)
		{
			try
			{
				return base.PersistInserted(sender, row);
			}
			catch (PXLockViolationException)
			{
				return false;
			}
		}
	}
	[ItemCustSalesHistAccum()]
    [PXDisableCloneAttributes()]
    [Serializable]
	public partial class ItemCustSalesHist : INItemCustSalesHist
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? InventoryID
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
		#region CostSubItemID
		public new abstract class costSubItemID : PX.Data.IBqlField
		{
		}
		[SubItem(IsKey = true)]
		[PXDefault()]
		public override Int32? CostSubItemID
		{
			get
			{
				return this._CostSubItemID;
			}
			set
			{
				this._CostSubItemID = value;
			}
		}
		#endregion
		#region CostSiteID
		public new abstract class costSiteID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? CostSiteID
		{
			get
			{
				return this._CostSiteID;
			}
			set
			{
				this._CostSiteID = value;
			}
		}
		#endregion
		#region BAccountID
		public new abstract class bAccountID : PX.Data.IBqlField
		{
		}
		protected new Int32? _BAccountID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		[PXDBString(6, IsKey = true, IsFixed = true)]
		[PXDefault()]
		public override String FinPeriodID
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

	public class StatusAccumulatorAttribute : PXAccumulatorAttribute, IPXRowPersistedSubscriber 
	{
		protected Dictionary<object, bool> _persisted;
        protected PXAccumulatorCollection _columns;

        protected virtual object Aggregate(PXCache cache, object a, object b)
        {
            object ret = cache.CreateCopy(a);

            foreach (KeyValuePair<string, PXAccumulatorItem> column in _columns)
            {
                if (column.Value.CurrentUpdateBehavior == PXDataFieldAssign.AssignBehavior.Summarize)
                {
                    object aVal = cache.GetValue(a, column.Key);
                    object bVal = cache.GetValue(b, column.Key);
                    object retVal = null;

                    if (aVal.GetType() == typeof(decimal))
                    {
                        retVal = (decimal)aVal + (decimal)bVal;
                    }

                    if (aVal.GetType() == typeof(double))
                    {
                        retVal = (double)aVal + (double)bVal;
                    }

                    if (aVal.GetType() == typeof(long))
                    {
                        retVal = (long)aVal + (long)bVal;
                    }

                    if (aVal.GetType() == typeof(int))
                    {
                        retVal = (int)aVal + (int)bVal;
                    }

                    if (aVal.GetType() == typeof(short))
                    {
                        retVal = (short)aVal + (short)bVal;
                    }

                    cache.SetValue(ret, column.Key, retVal);
                }
            }

            return ret;
        }

		public override object Insert(PXCache sender, object row)
		{
			object copy = sender.CreateCopy(row);

			PXAccumulatorCollection columns = new PXAccumulatorCollection();

			PrepareInsert(sender, row, columns);

			foreach (KeyValuePair<string, PXAccumulatorItem> column in columns)
			{
				if (column.Value.CurrentUpdateBehavior == PXDataFieldAssign.AssignBehavior.Summarize)
				{
					sender.SetValue(copy, column.Key, null);
				}
			}
			
			object item = base.Insert(sender, copy);

			if (item != null && _persisted.ContainsKey(item))
			{
				foreach (string field in sender.Fields)
				{
					if (sender.GetValue(copy, field) == null)
					{
						object newvalue;
						if (sender.RaiseFieldDefaulting(field, copy, out newvalue))
						{
							sender.RaiseFieldUpdating(field, copy, ref newvalue);
						}
						sender.SetValue(copy, field, newvalue);
					}
				}
				return copy;
			}
			return item;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_persisted = new Dictionary<object, bool>();
		}

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            _columns = columns;
			if (base.PrepareInsert(sender, row, columns))
			{
				foreach (string field in sender.Fields)
				{
					if (sender.Keys.IndexOf(field) < 0 && field.StartsWith("Usr", StringComparison.InvariantCultureIgnoreCase))
					{
						object val = sender.GetValue(row, field);
						columns.Update(field, val, (val != null) ? PXDataFieldAssign.AssignBehavior.Replace : PXDataFieldAssign.AssignBehavior.Initialize);
					}
				}
				return true;
			}
			return false;
        }

		public override bool PersistInserted(PXCache sender, object row)
		{
			if (base.PersistInserted(sender, row))
			{
				_persisted.Add(row, true);
				return true;
			}
			return false;
		}

		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && (e.TranStatus == PXTranStatus.Completed || e.TranStatus == PXTranStatus.Aborted))
			{
				if (_persisted.ContainsKey(e.Row))
				{
					_persisted.Remove(e.Row);
				}
			}
		}
	}

	public class ItemStatsAccumulatorAttribute : PXAccumulatorAttribute
	{
		public ItemStatsAccumulatorAttribute()
		{
			base._SingleRecord = true;
		}
		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			ItemStats bal = (ItemStats)row;

			if (bal.LastCostDate == null || bal.LastCost == null)
			{
				columns.Update<ItemStats.lastCost>(bal.LastCost, PXDataFieldAssign.AssignBehavior.Initialize);
				columns.Update<ItemStats.lastCostDate>(bal.LastCostDate ?? new DateTime(1900, 1, 1), PXDataFieldAssign.AssignBehavior.Initialize);
			}
			else
			{
				columns.Update<ItemStats.lastCost>(bal.LastCost, PXDataFieldAssign.AssignBehavior.Replace);
				columns.Update<ItemStats.lastCostDate>(bal.LastCostDate, PXDataFieldAssign.AssignBehavior.Replace);
			}

			if (bal.LastPurchaseDate == null)
			{
				columns.Update<ItemStats.lastPurchaseDate>(bal.LastPurchaseDate, PXDataFieldAssign.AssignBehavior.Initialize);
				columns.Update<ItemStats.lastPurchasePrice>(bal.LastPurchasePrice, PXDataFieldAssign.AssignBehavior.Initialize);
				columns.Update<ItemStats.lastVendorID>(bal.LastVendorID, PXDataFieldAssign.AssignBehavior.Initialize);
			}
			else
			{
				columns.Update<ItemStats.lastPurchaseDate>(bal.LastPurchaseDate, PXDataFieldAssign.AssignBehavior.Replace);
				columns.Update<ItemStats.lastPurchasePrice>(bal.LastPurchasePrice, PXDataFieldAssign.AssignBehavior.Replace);
				columns.Update<ItemStats.lastVendorID>(bal.LastVendorID, PXDataFieldAssign.AssignBehavior.Replace);
			}

			if (bal.MinCost == 0m)
			{
				columns.Update<ItemStats.minCost>(bal.MinCost, PXDataFieldAssign.AssignBehavior.Initialize);
			}
			else
			{
				columns.Update<ItemStats.minCost>(bal.MinCost, PXDataFieldAssign.AssignBehavior.Minimize);
			}

			columns.Update<ItemStats.maxCost>(bal.MaxCost, PXDataFieldAssign.AssignBehavior.Maximize);
			columns.Update<ItemStats.qtyOnHand>(bal.QtyOnHand, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<ItemStats.totalCost>(bal.TotalCost, PXDataFieldAssign.AssignBehavior.Summarize);

			return true;
		}
	}

	public class SiteStatusAccumulatorAttribute : StatusAccumulatorAttribute
	{
		public SiteStatusAccumulatorAttribute()
		{
			base._SingleRecord = true;
		}
		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			SiteStatus bal = (SiteStatus)row;

			columns.Update<SiteStatus.qtyOnHand>(bal.QtyOnHand, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<SiteStatus.qtyAvail>(bal.QtyAvail, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<SiteStatus.qtyHardAvail>(bal.QtyHardAvail, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<SiteStatus.qtyINIssues>(bal.QtyINIssues, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<SiteStatus.qtyINReceipts>(bal.QtyINReceipts, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<SiteStatus.qtyInTransit>(bal.QtyInTransit, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<SiteStatus.qtyPOReceipts>(bal.QtyPOReceipts, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<SiteStatus.qtyPOPrepared>(bal.QtyPOPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<SiteStatus.qtyPOOrders>(bal.QtyPOOrders, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<SiteStatus.qtySOBooked>(bal.QtySOBooked, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<SiteStatus.qtySOShipped>(bal.QtySOShipped, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<SiteStatus.qtySOShipping>(bal.QtySOShipping, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<SiteStatus.qtyINAssemblyDemand>(bal.QtyINAssemblyDemand, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<SiteStatus.qtyINAssemblySupply>(bal.QtyINAssemblySupply, PXDataFieldAssign.AssignBehavior.Summarize);

            //only in release process updates onhand.
            if (bal.NegQty == false && bal.QtyOnHand < 0m)
            {
                columns.Restrict<SiteStatus.qtyOnHand>(PXComp.GE, -bal.QtyOnHand);
            }
            else if (bal.NegAvailQty == false && bal.QtyHardAvail < 0m)
            {
                columns.Restrict<SiteStatus.qtyHardAvail>(PXComp.GE, -bal.QtyHardAvail);
            }

			return true;
		}

		public override bool PersistInserted(PXCache sender, object row)
		{
			try
			{
				return base.PersistInserted(sender, row);
			}
			catch (PXLockViolationException)
			{
                object inventoryID = sender.GetValue<SiteStatus.inventoryID>(row);
                object subItemID = sender.GetValue<SiteStatus.subItemID>(row);
                object siteID = sender.GetValue<SiteStatus.siteID>(row);

                SiteStatus item = PXSelectReadonly<SiteStatus, Where<SiteStatus.inventoryID, Equal<Required<SiteStatus.inventoryID>>, And<SiteStatus.subItemID, Equal<Required<SiteStatus.subItemID>>, And<SiteStatus.siteID, Equal<Required<SiteStatus.siteID>>>>>>.Select(sender.Graph, inventoryID, subItemID, siteID);

                item = (SiteStatus)this.Aggregate(sender, item, row);

                SiteStatus bal = (SiteStatus)row;
                string message = null;
                //only in release process updates onhand.
                if (bal.NegQty == false && bal.QtyOnHand < 0m)
                {
                    if (item.QtyOnHand < 0m)
                    {
                        message = Messages.StatusCheck_QtyOnHandNegative;
                    }
                }
                else if (bal.NegAvailQty == false && bal.QtyHardAvail < 0m)
                {
                    if (item.QtyHardAvail < 0)
                    {
                        message = Messages.StatusCheck_QtyAvailNegative;
                    }
                }

                if (message != null)
                {
                    throw new PXException(message,
					PXForeignSelectorAttribute.GetValueExt<SiteStatus.inventoryID>(sender, row),
					PXForeignSelectorAttribute.GetValueExt<SiteStatus.subItemID>(sender, row),
					PXForeignSelectorAttribute.GetValueExt<SiteStatus.siteID>(sender, row));
			}

                throw;
            }
        }

        public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
            {
                SiteStatus bal = (SiteStatus)e.Row;
                string message = null;
                //only in release process updates onhand.
                if (bal.NegQty == false && bal.QtyOnHand < 0m)
                {
                    message = Messages.StatusCheck_QtyOnHandNegative;
                }
                else if (bal.NegAvailQty == false && bal.QtyHardAvail < 0m)
                {
                    message = Messages.StatusCheck_QtyAvailNegative;
                }

                if (message != null)
                {
                    throw new PXException(message,
                        PXForeignSelectorAttribute.GetValueExt<SiteStatus.inventoryID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<SiteStatus.subItemID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<SiteStatus.siteID>(sender, e.Row));
                }
            }

            base.RowPersisted(sender, e);
		}
	}

	public class LocationStatusAccumulatorAttribute : StatusAccumulatorAttribute
	{
		public LocationStatusAccumulatorAttribute()
		{
			base._SingleRecord = true;
		}

		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			LocationStatus bal = (LocationStatus)row;

			columns.Update<LocationStatus.qtyOnHand>(bal.QtyOnHand, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LocationStatus.qtyAvail>(bal.QtyAvail, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LocationStatus.qtyHardAvail>(bal.QtyHardAvail, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LocationStatus.qtyINIssues>(bal.QtyINIssues, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LocationStatus.qtyINReceipts>(bal.QtyINReceipts, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LocationStatus.qtyInTransit>(bal.QtyInTransit, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LocationStatus.qtyPOReceipts>(bal.QtyPOReceipts, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LocationStatus.qtyPOPrepared>(bal.QtyPOPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LocationStatus.qtyPOOrders>(bal.QtyPOOrders, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LocationStatus.qtySOBooked>(bal.QtySOBooked, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LocationStatus.qtySOShipped>(bal.QtySOShipped, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LocationStatus.qtySOShipping>(bal.QtySOShipping, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LocationStatus.qtyINAssemblyDemand>(bal.QtyINAssemblyDemand, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LocationStatus.qtyINAssemblySupply>(bal.QtyINAssemblySupply, PXDataFieldAssign.AssignBehavior.Summarize);

			if (bal.QtyOnHand >= 0m)
			{
				bal.NegQty = true;
			}

            //only in release process updates onhand.
            if (bal.NegQty == false && bal.QtyOnHand < 0m)
            {
                columns.Restrict<LocationStatus.qtyOnHand>(PXComp.GE, -bal.QtyOnHand);
            }
            
			return true;
		}

        public override bool PersistInserted(PXCache sender, object row)
        {
            try
            {
                return base.PersistInserted(sender, row);
            }
            catch (PXLockViolationException)
            {
                object inventoryID = sender.GetValue<LocationStatus.inventoryID>(row);
                object subItemID = sender.GetValue<LocationStatus.subItemID>(row);
                object siteID = sender.GetValue<LocationStatus.siteID>(row);
                object locationID = sender.GetValue<LocationStatus.locationID>(row);

                LocationStatus item = PXSelectReadonly<LocationStatus, Where<LocationStatus.inventoryID, Equal<Required<LocationStatus.inventoryID>>, And<LocationStatus.subItemID, Equal<Required<LocationStatus.subItemID>>, And<LocationStatus.siteID, Equal<Required<LocationStatus.siteID>>, And<LocationStatus.locationID, Equal<Required<LocationStatus.locationID>>>>>>>.Select(sender.Graph, inventoryID, subItemID, siteID, locationID);

                item = (LocationStatus)this.Aggregate(sender, item, row);

                LocationStatus bal = (LocationStatus)row;

                string message = null;
                //only in release process updates onhand.
                if (bal.NegQty == false && bal.QtyOnHand < 0m)
                {
                    if (item.QtyOnHand < 0m)
                    {
                        message = Messages.StatusCheck_QtyLocationOnHandNegative;
                    }
                }
                
                if (message != null)
                {
                    throw new PXException(message,
                        PXForeignSelectorAttribute.GetValueExt<LocationStatus.inventoryID>(sender, row),
                        PXForeignSelectorAttribute.GetValueExt<LocationStatus.subItemID>(sender, row),
                        PXForeignSelectorAttribute.GetValueExt<LocationStatus.siteID>(sender, row),
                        PXForeignSelectorAttribute.GetValueExt<LocationStatus.locationID>(sender, row));
                }

                throw;
            }
        }

        public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
            {
                LocationStatus bal = (LocationStatus)e.Row;
                string message = null;
                //only in release process updates onhand.
                if (bal.NegQty == false && bal.QtyOnHand < 0m)
                {
                    message = Messages.StatusCheck_QtyLocationOnHandNegative;
                }

                if (message != null)
                {
                    throw new PXException(message,
                        PXForeignSelectorAttribute.GetValueExt<LocationStatus.inventoryID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<LocationStatus.subItemID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<LocationStatus.siteID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<LocationStatus.locationID>(sender, e.Row));
                }
            }

            base.RowPersisted(sender, e);
        }
	}

	public class ItemLotSerialAccumulatorAttribute : StatusAccumulatorAttribute
	{
        protected class AutoNumberedEntityHelper : EntityHelper
        {
            public AutoNumberedEntityHelper(PXGraph graph)
                : base(graph)
            {
            }

            public override string GetFieldString(object row, Type entityType, string fieldName, bool preferDescription)
            {
                PXCache cache = this.graph.Caches[entityType];

                if (cache.GetStatus(row) == PXEntryStatus.Inserted)
                {
                    object cached = cache.Locate(row);
                    string val = AutoNumberAttribute.GetKeyToAbort(cache, cached, fieldName);
                    if (val != null)
                    {
                        return val;
                    }
                }
                return base.GetFieldString(row, entityType, fieldName, preferDescription);
            }
        }

		public ItemLotSerialAccumulatorAttribute()
		{
			base._SingleRecord = true;
		}

        protected virtual void PrepareSingleField<Field>(PXCache sender, object row, PXAccumulatorCollection columns)
           where Field : IBqlField
        {
            string lotSerTrack = (string)sender.GetValue<ItemLotSerial.lotSerTrack>(row);
            string lotSerAssign = (string)sender.GetValue<ItemLotSerial.lotSerAssign>(row);
            decimal? qty = (decimal?)sender.GetValue<Field>(row);

            if (lotSerTrack == INLotSerTrack.SerialNumbered &&
                qty != null &&
                qty != 0m &&
                qty != -1m &&
                qty != 1m)
                throw new PXException(Messages.SerialNumberDuplicated,
                    PXForeignSelectorAttribute.GetValueExt<ItemLotSerial.inventoryID>(sender, row),
                    PXForeignSelectorAttribute.GetValueExt<ItemLotSerial.lotSerialNbr>(sender, row));

            if (lotSerTrack == INLotSerTrack.SerialNumbered && lotSerAssign == INLotSerAssign.WhenReceived)
            {
                columns.Restrict<Field>(PXComp.LE, 1m - qty);
                columns.Restrict<Field>(PXComp.GE, 0m - qty);
            }

            if (lotSerTrack == INLotSerTrack.SerialNumbered && lotSerAssign == INLotSerAssign.WhenUsed)
            {
                columns.Restrict<Field>(PXComp.LE, 0m - qty);
                columns.Restrict<Field>(PXComp.GE, -1m - qty);
            }
        }

        protected virtual void ValidateSingleField<Field>(PXCache sender, object row, long? refNoteID, ref string message)
            where Field : IBqlField 
        {
            string lotSerTrack = (string)sender.GetValue<ItemLotSerial.lotSerTrack>(row);
            string lotSerAssign = (string)sender.GetValue<ItemLotSerial.lotSerAssign>(row);
            decimal? qty = (decimal?)sender.GetValue<Field>(row);

            if (lotSerTrack == INLotSerTrack.SerialNumbered && lotSerAssign == INLotSerAssign.WhenReceived)
            {
                //consider reusing PXAccumulator rules
                if (qty < 0m)
                {
                    message = refNoteID == null ? Messages.SerialNumberAlreadyIssued : Messages.SerialNumberAlreadyIssuedIn;
                }
                if (qty > 1m)
                {
                    message = refNoteID == null ? Messages.SerialNumberAlreadyReceived : Messages.SerialNumberAlreadyReceivedIn;
                }
            }

            if (lotSerTrack == INLotSerTrack.SerialNumbered && lotSerAssign == INLotSerAssign.WhenUsed)
            {
                //consider reusing PXAccumulator rules
                if (qty > 0m)
                {
                    message = refNoteID == null ? Messages.SerialNumberAlreadyReceived : Messages.SerialNumberAlreadyReceivedIn;
                }
                if (qty < -1m)
                {
                    message = refNoteID == null ? Messages.SerialNumberAlreadyIssued : Messages.SerialNumberAlreadyIssuedIn;
                }
            }
        }

		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			ItemLotSerial bal = (ItemLotSerial)row;
			columns.Update<ItemLotSerial.qtyOnHand>(bal.QtyOnHand, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<ItemLotSerial.qtyAvail>(bal.QtyAvail, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<ItemLotSerial.qtyHardAvail>(bal.QtyHardAvail, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<ItemLotSerial.qtyInTransit>(bal.QtyInTransit, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<ItemLotSerial.lotSerTrack>(bal.LotSerTrack, PXDataFieldAssign.AssignBehavior.Initialize);
			columns.Update<ItemLotSerial.lotSerAssign>(bal.LotSerTrack, PXDataFieldAssign.AssignBehavior.Initialize);
			if (bal.UpdateExpireDate == true)
				columns.Update<ItemLotSerial.expireDate>(bal.ExpireDate, PXDataFieldAssign.AssignBehavior.Replace);
			else
				columns.Update<ItemLotSerial.expireDate>(bal.ExpireDate, PXDataFieldAssign.AssignBehavior.Initialize);

            //only in release process updates onhand.
            if (bal.QtyOnHand != 0m)
            {
                PrepareSingleField<ItemLotSerial.qtyOnHand>(sender, row, columns);
            }
            else if (!sender.Graph.UnattendedMode)
            {
                PrepareSingleField<ItemLotSerial.qtyAvail>(sender, row, columns);
            }

			return true;
		}

        public override bool PersistInserted(PXCache sender, object row)
        {
            try
            {
                return base.PersistInserted(sender, row);
            }
            catch (PXLockViolationException)
            {
                object inventoryID = sender.GetValue<ItemLotSerial.inventoryID>(row);
                string lotSerialNbr = (string)sender.GetValue<ItemLotSerial.lotSerialNbr>(row);

                ItemLotSerial item = PXSelectReadonly<ItemLotSerial, Where<ItemLotSerial.inventoryID, Equal<Required<ItemLotSerial.inventoryID>>, And<ItemLotSerial.lotSerialNbr, Equal<Required<ItemLotSerial.lotSerialNbr>>>>>.Select(sender.Graph, inventoryID, lotSerialNbr);

                item = (ItemLotSerial)this.Aggregate(sender, item, row);
                item.LotSerTrack = ((ItemLotSerial)row).LotSerTrack;
                item.LotSerAssign = ((ItemLotSerial)row).LotSerAssign;

                bool isDuplicated = false;
                long? refNoteID = null;

                {
                    PXResultset<INItemPlan> plans = PXSelect<INItemPlan, Where<INItemPlan.inventoryID, Equal<Required<INItemPlan.inventoryID>>, And<INItemPlan.lotSerialNbr, Equal<Required<INItemPlan.lotSerialNbr>>>>>.SelectWindowed(sender.Graph, 0, 10, inventoryID, lotSerialNbr);

                    List<long?> refs = new List<long?>();

                    if (plans.Count <= 1)
                    {
                        refs.Add(null);
                    }
                    else
                    {
                        Dictionary<long?, int> counts = new Dictionary<long?, int>();
                        PXCache cache = sender.Graph.Caches[typeof(INItemPlan)];
                        for (int i = 0; i < plans.Count; i++)
                        {
                            refNoteID = plans[i].GetItem<INItemPlan>().RefNoteID;
                            if (cache.GetStatus(plans[i].GetItem<INItemPlan>()) == PXEntryStatus.Notchanged)
                            {
                                refs.Insert(0, refNoteID);
                            }
                            else
                            {
                                refs.Add(refNoteID);
                            }

                            if (counts.ContainsKey(refNoteID))
                            {
                                counts[refNoteID]++;
                                isDuplicated = true;
                            }
                            else
                            {
                                counts[refNoteID] = 1;
                            }
                        }
                    }
                    refNoteID = refs[0];
                }

                string message = null;
                if (((ItemLotSerial)row).LotSerTrack == INLotSerTrack.SerialNumbered && isDuplicated)
                {
                    message = Messages.SerialNumberDuplicated;
                }

                ItemLotSerial bal = (ItemLotSerial)row;

                //only in release process updates onhand.
                if (bal.QtyOnHand != 0m)
                {
                    ValidateSingleField<ItemLotSerial.qtyOnHand>(sender, item, null, ref message);
                }
                else if (!sender.Graph.UnattendedMode)
                {
                    ValidateSingleField<ItemLotSerial.qtyAvail>(sender, item, refNoteID, ref message);
                }

                string refRowID = null;
                if (refNoteID != null)
                {
                    AutoNumberedEntityHelper hlp = new AutoNumberedEntityHelper(sender.Graph);
                    refRowID = hlp.GetEntityRowID(refNoteID);
                }

                if (message != null)
                {
                    throw new PXException(message,
                        PXForeignSelectorAttribute.GetValueExt<ItemLotSerial.inventoryID>(sender, row),
                        PXForeignSelectorAttribute.GetValueExt<ItemLotSerial.lotSerialNbr>(sender, row),
                        refRowID);
                }

                throw;
            }
        }
	}

	public class LotSerialStatusAccumulatorAttribute : StatusAccumulatorAttribute
	{
		public LotSerialStatusAccumulatorAttribute()
		{
			base._SingleRecord = true;
		}

		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			LotSerialStatus bal = (LotSerialStatus)row;

			columns.Update<LotSerialStatus.qtyOnHand>(bal.QtyOnHand, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LotSerialStatus.qtyAvail>(bal.QtyAvail, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LotSerialStatus.qtyHardAvail>(bal.QtyHardAvail, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LotSerialStatus.qtyINIssues>(bal.QtyINIssues, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LotSerialStatus.qtyINReceipts>(bal.QtyINReceipts, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LotSerialStatus.qtyInTransit>(bal.QtyInTransit, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LotSerialStatus.qtyPOReceipts>(bal.QtyPOReceipts, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LotSerialStatus.qtyPOPrepared>(bal.QtyPOPrepared, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LotSerialStatus.qtyPOOrders>(bal.QtyPOOrders, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LotSerialStatus.qtySOBooked>(bal.QtySOBooked, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LotSerialStatus.qtySOShipped>(bal.QtySOShipped, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LotSerialStatus.qtySOShipping>(bal.QtySOShipping, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LotSerialStatus.qtyINAssemblyDemand>(bal.QtyINAssemblyDemand, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LotSerialStatus.qtyINAssemblySupply>(bal.QtyINAssemblySupply, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<LotSerialStatus.lotSerTrack>(bal.LotSerTrack, PXDataFieldAssign.AssignBehavior.Initialize);
			columns.Update<LotSerialStatus.costID>(bal.CostID, PXDataFieldAssign.AssignBehavior.Initialize);
			columns.Update<LotSerialStatus.receiptDate>(bal.ReceiptDate, PXDataFieldAssign.AssignBehavior.Initialize);

			if (bal.CostID < 0)
			{
				throw new PXException(Messages.InternalError, Messages.LS);
			}

            //only in release process updates onhand.
            if (bal.QtyOnHand < 0m)
		{
                columns.Restrict<LotSerialStatus.qtyOnHand>(PXComp.GE, -bal.QtyOnHand);
		}

			return true;
		}

        public override bool PersistInserted(PXCache sender, object row)
	{
            try
		{
                return base.PersistInserted(sender, row);
		}
            catch (PXLockViolationException)
			{
                object inventoryID = sender.GetValue<LotSerialStatus.inventoryID>(row);
                object subItemID = sender.GetValue<LotSerialStatus.subItemID>(row);
                object siteID = sender.GetValue<LotSerialStatus.siteID>(row);
                object locationID = sender.GetValue<LotSerialStatus.locationID>(row);
                object lotSerialNbr = sender.GetValue<LotSerialStatus.lotSerialNbr>(row);

                LotSerialStatus item = PXSelectReadonly<LotSerialStatus, Where<LotSerialStatus.inventoryID, Equal<Required<LotSerialStatus.inventoryID>>, And<LotSerialStatus.subItemID, Equal<Required<LotSerialStatus.subItemID>>, And<LotSerialStatus.siteID, Equal<Required<LotSerialStatus.siteID>>, And<LotSerialStatus.locationID, Equal<Required<LotSerialStatus.locationID>>, And<LotSerialStatus.lotSerialNbr,Equal<Required<LotSerialStatus.lotSerialNbr>>>>>>>>.Select(sender.Graph, inventoryID, subItemID, siteID, locationID, lotSerialNbr);

                item = (LotSerialStatus)this.Aggregate(sender, item, row);

                LotSerialStatus bal = (LotSerialStatus)row;

                string message = null;
                //only in release process updates onhand.
                if (bal.QtyOnHand < 0m)
	{
                    if (item.QtyOnHand < 0m)
		{
                        message = Messages.StatusCheck_QtyLotSerialOnHandNegative;
		}
	}

                if (message != null)
	{
                    throw new PXException(message,
                        PXForeignSelectorAttribute.GetValueExt<LotSerialStatus.inventoryID>(sender, row),
                        PXForeignSelectorAttribute.GetValueExt<LotSerialStatus.subItemID>(sender, row),
                        PXForeignSelectorAttribute.GetValueExt<LotSerialStatus.siteID>(sender, row),
                        PXForeignSelectorAttribute.GetValueExt<LotSerialStatus.locationID>(sender, row),
                        PXForeignSelectorAttribute.GetValueExt<LotSerialStatus.lotSerialNbr>(sender, row));
                }

                throw;
		}
	}

        public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
	{
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
            {
                LotSerialStatus bal = (LotSerialStatus)e.Row;
                string message = null;
                //only in release process updates onhand.
                if (bal.QtyOnHand < 0m)
		{
                    message = Messages.StatusCheck_QtyLotSerialOnHandNegative;
	}

                if (message != null)
	{
                    throw new PXException(message,
                        PXForeignSelectorAttribute.GetValueExt<LotSerialStatus.inventoryID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<LotSerialStatus.subItemID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<LotSerialStatus.siteID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<LotSerialStatus.locationID>(sender, e.Row),
                        PXForeignSelectorAttribute.GetValueExt<LotSerialStatus.lotSerialNbr>(sender, e.Row));
        }
        }

            base.RowPersisted(sender, e);
        }
		}

	public class CostStatusAccumulatorAttribute : PXAccumulatorAttribute
	{
		protected Type _QuantityField;
		protected Type _CostField;
		protected Type _InventoryIDField;
		protected Type _SubItemIDField;
		protected Type _SiteIDField;
		protected Type _SpecificNumberField;
		protected Type _LayerTypeField;

		public CostStatusAccumulatorAttribute(Type quantityField, Type costField, Type inventoryIDField, Type subItemIDField, Type siteIDField, Type specificNumberField, Type layerTypeField)
			: this()
		{
			_QuantityField = quantityField;
			_CostField = costField;
			_InventoryIDField = inventoryIDField;
			_SubItemIDField = subItemIDField;
			_SiteIDField = siteIDField;
			_SpecificNumberField = specificNumberField;
			_LayerTypeField = layerTypeField;
		}
		public CostStatusAccumulatorAttribute(Type quantityField, Type costField, Type inventoryIDField, Type subItemIDField, Type siteIDField, Type layerTypeField)
			: this(quantityField, costField, inventoryIDField, subItemIDField, siteIDField, null, layerTypeField)
		{
		}
		public CostStatusAccumulatorAttribute()
		{
			base._SingleRecord = true;
		}

		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			columns.AppendException(_SpecificNumberField == null ? Messages.StatusCheck_QtyNegative : Messages.StatusCheck_QtyNegative2,
				new PXAccumulatorRestriction(_QuantityField.Name, PXComp.GE, 0m),
				new PXAccumulatorRestriction(_LayerTypeField.Name, PXComp.EQ, INLayerType.Oversold));
			columns.AppendException(_SpecificNumberField == null ? Messages.StatusCheck_QtyNegative : Messages.StatusCheck_QtyNegative2,
				new PXAccumulatorRestriction(_QuantityField.Name, PXComp.LE, 0m),
				new PXAccumulatorRestriction(_LayerTypeField.Name, PXComp.EQ, INLayerType.Normal));
			columns.AppendException(Messages.StatusCheck_QtyCostImblance,
				new PXAccumulatorRestriction(_QuantityField.Name, PXComp.NE, 0m),
				new PXAccumulatorRestriction(_CostField.Name, PXComp.EQ, 0m));
			columns.AppendException(Messages.StatusCheck_QtyCostImblance,
				new PXAccumulatorRestriction(_QuantityField.Name, PXComp.LE, 0m),
				new PXAccumulatorRestriction(_CostField.Name, PXComp.GE, 0m));
			columns.AppendException(Messages.StatusCheck_QtyCostImblance,
				new PXAccumulatorRestriction(_QuantityField.Name, PXComp.GE, 0m),
				new PXAccumulatorRestriction(_CostField.Name, PXComp.LE, 0m));

			INCostStatus bal = (INCostStatus)row;

			columns.Update<INCostStatus.unitCost>(bal.UnitCost, PXDataFieldAssign.AssignBehavior.Replace);
			columns.Update<INCostStatus.origQty>(bal.OrigQty, PXDataFieldAssign.AssignBehavior.Initialize);
			columns.Update<INCostStatus.qtyOnHand>(bal.QtyOnHand, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<INCostStatus.totalCost>(bal.TotalCost, PXDataFieldAssign.AssignBehavior.Summarize);

			return true;
		}

		public override bool PersistInserted(PXCache sender, object row)
		{
			try
			{
				return base.PersistInserted(sender, row);
			}
			catch (PXRestrictionViolationException e)
			{
				List<object> pars = new List<object>();
				if (sender.BqlKeys.Contains(_InventoryIDField))
				{
					pars.Add(PXForeignSelectorAttribute.GetValueExt(sender, row, _InventoryIDField.Name));
				}
				if (sender.BqlKeys.Contains(_SubItemIDField))
				{
					pars.Add(PXForeignSelectorAttribute.GetValueExt(sender, row, _SubItemIDField.Name));
				}
				if (sender.BqlKeys.Contains(_InventoryIDField))
				{
					pars.Add(PXForeignSelectorAttribute.GetValueExt(sender, row, _SiteIDField.Name));
				}
				if ((e.Index == 0 || e.Index == 1) && _SpecificNumberField != null && sender.BqlKeys.Contains(_SpecificNumberField))
				{
					pars.Add(PXForeignSelectorAttribute.GetValueExt(sender, row, _SpecificNumberField.Name));
				}
				throw new PXException((e.Index == 0 || e.Index == 1) ? (_SpecificNumberField == null ? Messages.StatusCheck_QtyNegative : Messages.StatusCheck_QtyNegative2) : Messages.StatusCheck_QtyCostImblance, pars.ToArray());
			}
		}
	}

	public class ReceiptStatusAccumulatorAttribute : PXAccumulatorAttribute
                {
		public ReceiptStatusAccumulatorAttribute()
                {
			base._SingleRecord = true;
                        }

		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
                        {
			if (!base.PrepareInsert(sender, row, columns))
                        {
				return false;
        }

			INReceiptStatus bal = (INReceiptStatus)row;

			columns.Update<INReceiptStatus.origQty>(bal.OrigQty, PXDataFieldAssign.AssignBehavior.Initialize);

			return true;
        }
	}

	public class CostSiteIDAttribute : PXForeignSelectorAttribute
	{
		public CostSiteIDAttribute()
			: base(typeof(INTran.locationID))
		{
		}

		protected override object GetValueExt(PXCache sender, object item)
		{
			object val = sender.GetValue(item, _FieldOrdinal);
			object copyval = val;
			string result = string.Empty;

			PXResult<INLocation, INSite> res = (PXResult<INLocation, INSite>)PXSelectJoin<INLocation, InnerJoin<INSite, On<INSite.siteID, Equal<INLocation.siteID>>>, Where<INLocation.siteID, Equal<Required<INLocation.siteID>>, Or<INLocation.locationID, Equal<Required<INLocation.locationID>>>>>.SelectSingleBound(sender.Graph, null, val, val);

			INLocation loc = (INLocation)res;
			INSite site = (INSite)res;

			val = site.SiteCD;
			sender.Graph.Caches[typeof(INSite)].RaiseFieldSelecting<INSite.siteCD>(loc, ref val, true);
			if (val is PXStringState && string.IsNullOrEmpty(((PXStringState)val).InputMask) == false)
			{
				result = PX.Common.Mask.Format(((PXStringState)val).InputMask, (string)((PXStringState)val).Value);
			}
			else if (val is PXFieldState && ((PXStringState)val).Value is string)
			{
				result = (string)((PXFieldState)val).Value;
			}

			if (loc.LocationID == (int?)copyval)
			{
				val = loc.LocationCD;
				sender.Graph.Caches[typeof(INLocation)].RaiseFieldSelecting<INLocation.locationCD>(loc, ref val, true);

				if (val is PXStringState && string.IsNullOrEmpty(((PXStringState)val).InputMask) == false)
				{
					result += ' '.ToString();
					result += PX.Common.Mask.Format(((PXStringState)val).InputMask, (string)((PXStringState)val).Value);
				}
				else if (val is PXFieldState && ((PXStringState)val).Value is string)
				{
					result += ' '.ToString();
					result += (string)((PXFieldState)val).Value;
				}
			}
			return result;
		}
	}

	public class CostIdentityAttribute : PXDBLongIdentityAttribute
	{
		#region State
		protected new long? _KeyToAbort = null;
		protected Type[] _ChildTypes = null;
		#endregion

		#region Ctor
		public CostIdentityAttribute(params Type[] ChildTypes)
			: base()
		{
			_ChildTypes = ChildTypes;
		}
		#endregion

		#region Implementation
		public long? SelectAccumIdentity(PXCache sender, object Row)
		{
			List<PXDataField> fields = new List<PXDataField>();

			fields.Add(new PXDataField(_FieldName));

			foreach (string key in sender.Keys)
			{
				fields.Add(new PXDataFieldValue(key, sender.GetValue(Row, key)));
			}

			using (PXDataRecord UpdatedRow = PXDatabase.SelectSingle(sender.BqlTable, fields.ToArray()))
			{
				if (UpdatedRow != null)
				{
					return UpdatedRow.GetInt64(0);
				}
			}
			return null; ;
		}

		public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				if (e.TranStatus == PXTranStatus.Open)
				{
					this._KeyToAbort = (long?)sender.GetValue(e.Row, _FieldOrdinal);

					base.RowPersisted(sender, e);

					if (_KeyToAbort < 0)
					{
						long? _NewKey = SelectAccumIdentity(sender, e.Row);
						for (int i = 0; i < _ChildTypes.Length; i++)
						{
							PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_ChildTypes[i])];
							foreach (object item in cache.Inserted)
							{
								if ((long?)cache.GetValue(item, _ChildTypes[i].Name) == _KeyToAbort)
								{
									cache.SetValue(item, _ChildTypes[i].Name, _NewKey);
								}
							}
						}
					}
				}
				else if (e.TranStatus == PXTranStatus.Aborted && _KeyToAbort != null)
				{
					long? _NewKey = (long?)sender.GetValue(e.Row, _FieldOrdinal);
					for (int i = 0; i < _ChildTypes.Length; i++)
					{
						PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_ChildTypes[i])];
						foreach (object item in cache.Inserted)
						{
							if ((long?)cache.GetValue(item, _ChildTypes[i].Name) == _NewKey)
							{
								cache.SetValue(item, _ChildTypes[i].Name, _KeyToAbort);
							}
						}
					}
					_KeyToAbort = null;
					base.RowPersisted(sender, e);
				}
			}

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				if (e.TranStatus == PXTranStatus.Open)
				{
					this._KeyToAbort = (long?)sender.GetValue(e.Row, _FieldOrdinal);

					base.RowPersisted(sender, e);

					if (_KeyToAbort < 0)
					{
						long? _NewKey = Convert.ToInt64(PXDatabase.SelectIdentity());
						for (int i = 0; i < _ChildTypes.Length; i++)
						{
							PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_ChildTypes[i])];
							foreach (object item in cache.Inserted)
							{
								if ((long?)cache.GetValue(item, _ChildTypes[i].Name) == _KeyToAbort)
								{
									cache.SetValue(item, _ChildTypes[i].Name, _NewKey);
								}
							}
						}
					}
				}
				else if (e.TranStatus == PXTranStatus.Aborted && _KeyToAbort != null)
				{
					long? _NewKey = (long?)sender.GetValue(e.Row, _FieldOrdinal);
					for (int i = 0; i < _ChildTypes.Length; i++)
					{
						PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_ChildTypes[i])];
						foreach (object item in cache.Inserted)
						{
							if ((long?)cache.GetValue(item, _ChildTypes[i].Name) == _NewKey)
							{
								cache.SetValue(item, _ChildTypes[i].Name, _KeyToAbort);
							}
						}
					}
					_KeyToAbort = null;
					base.RowPersisted(sender, e);
				}
			}
		}
		#endregion
	}

	public class NonStockItem : IBqlTable
	{
		#region InvtAcctID
		public abstract class invtAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _InvtAcctID;
		[PXUIField(DisplayName = "Expense Accrual Account")]
		public virtual Int32? InvtAcctID
		{
			get
			{
				return this._InvtAcctID;
			}
			set
			{
				this._InvtAcctID = value;
			}
		}
		#endregion
		#region InvtSubID
		public abstract class invtSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _InvtSubID;
		[PXUIField(DisplayName = "Expense Accrual Sub.")]
		public virtual Int32? InvtSubID
		{
			get
			{
				return this._InvtSubID;
			}
			set
			{
				this._InvtSubID = value;
			}
		}
		#endregion
		#region COGSAcctID
		public abstract class cOGSAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _COGSAcctID;
		[PXUIField(DisplayName = "Expense Account")]
		public virtual Int32? COGSAcctID
		{
			get
			{
				return this._COGSAcctID;
			}
			set
			{
				this._COGSAcctID = value;
			}
		}
		#endregion
		#region COGSSubID
		public abstract class cOGSSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _COGSSubID;
		[PXUIField(DisplayName = "Expense Sub.")]
		public virtual Int32? COGSSubID
		{
			get
			{
				return this._COGSSubID;
			}
			set
			{
				this._COGSSubID = value;
			}
		}
		#endregion
	}

}
