using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using PX.Data;
using PX.Common;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.CA;
using PX.Objects.GL.Overrides.PostGraph;
using PX.Objects.PM;
using PX.Objects.TX;

namespace PX.Objects.GL
{
    public class JournalEntry : PXGraph<JournalEntry, Batch>, PXImportAttribute.IPXPrepareItems
    {
        #region Cache Attached Events
		#region PMTran
		#region TranID

		[PXDBLongIdentity(IsKey = true)]
        protected virtual void PMTran_TranID_CacheAttached(PXCache sender)
        {
        }
        #endregion
		#region RefNbr
		//[PXDBLiteDefault(typeof(PMRegister.refNbr))]// is handled by the graph see PMRegister_RowPersisted
		[PXDBString(15, IsUnicode = true)]
		protected virtual void PMTran_RefNbr_CacheAttached(PXCache sender)
		{
		}
		#endregion
        #region BatchNbr
        [PXDBLiteDefault(typeof(Batch.batchNbr), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "BatchNbr")]
        protected virtual void PMTran_BatchNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
		#region Date
		[PXDBDate()]
		[PXDefault(typeof(PMRegister.date))]
		public virtual void PMTran_Date_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion
        #endregion

        public ToggleCurrency<Batch> CurrencyView;

        /*
        public class TranslationHistoryRev : TranslationHistory
        { 
        }
        */

        public PXSelect<Batch, Where<Batch.module, Equal<Optional<Batch.module>>, And<Batch.draft, Equal<False>>>> BatchModule;
        public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<Batch.curyInfoID>>>> currencyinfo;
        [PXImport(typeof(Batch))]
        public PXSelect<GLTran, Where<GLTran.module, Equal<Current<Batch.module>>, And<GLTran.batchNbr, Equal<Current<Batch.batchNbr>>>>> GLTranModuleBatNbr;
        public PXSelect<GLAllocationHistory, Where<GLAllocationHistory.batchNbr, Equal<Current<Batch.batchNbr>>, And<GLAllocationHistory.module, Equal<Current<Batch.module>>>>> AllocationHistory;
        public PXSelect<GLAllocationAccountHistory, Where<GLAllocationAccountHistory.batchNbr, Equal<Current<Batch.batchNbr>>, And<GLAllocationAccountHistory.module, Equal<Current<Batch.module>>>>> AllocationAccountHistory;
        public PXSelect<CATran> catran;
        public PXSelectReadonly<FinPeriod, Where<FinPeriod.finPeriodID, Equal<Current<Batch.finPeriodID>>>> finperiod;
        //public PXSetup<TranslationHistory, Where<TranslationHistory.batchNbr, Equal<Current<Batch.batchNbr>>>> translationhistory;
        //public PXSetup<TranslationHistoryRev, Where<TranslationHistoryRev.batchNbr, Equal<Current<Batch.origBatchNbr>>>> translationhistory_reversing;
        public PXSelect<PMRegister> ProjectDocs;
        public PXSelect<PMTran> ProjectTrans;
        public PXSelect<PMTaskTotal> ProjectTaskTotals;
        public PXSetup<Branch, Where<Branch.branchID, Equal<Optional<Batch.branchID>>>> branch;
        public PXSetup<Ledger, Where<Ledger.ledgerID, Equal<Optional<Batch.ledgerID>>>> ledger;
        public PXSetup<Company> company; 

        #region Properties
        public PXSetup<GLSetup> glsetup;

        public bool AutoRevEntry
        {
            get
            {
                return glsetup.Current.AutoRevEntry == true;
            }
        }

        public CMSetupSelect CMSetup;

        protected CurrencyInfo _CurrencyInfo;
        public CurrencyInfo currencyInfo
        {
            get
            {
                return currencyinfo.Select();
            }
        }
        public FinPeriod FINPERIOD
        {
            get
            {
                return finperiod.Select();
            }
        }
        #endregion

        #region Buttons

        public PXAction<Batch> batchRegisterDetails;
        [PXUIField(DisplayName = Messages.BatchRegisterDetails, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable BatchRegisterDetails(PXAdapter adapter)
        {
            if (BatchModule.Current != null && (bool)BatchModule.Current.Released == true)
            {
                throw new PXReportRequiredException(CreateBatchReportParams(), "GL621000", "Batch Register Details");
            }
            return adapter.Get();
        }

        public PXAction<Batch> glEditDetails;
        [PXUIField(DisplayName = Messages.GLEditDetails, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable GLEditDetails(PXAdapter adapter)
        {
            if (BatchModule.Current != null && (bool)BatchModule.Current.Released == false && (bool)BatchModule.Current.Posted == false)
            {
                throw new PXReportRequiredException(CreateBatchReportParams(), "GL610500", "GL Edit Details");
            }
            return adapter.Get();
        }

        private Dictionary<string, string> CreateBatchReportParams()
        {
            var parameters = new Dictionary<string, string>();
            parameters["LedgerID"] = ledger.Current.LedgerCD;
            parameters["BranchID"] = branch.Current.BranchCD;
            parameters["Batch.BatchNbr"] = BatchModule.Current.BatchNbr;

            var period = BatchModule.Current.FinPeriodID.Substring(4, 2) + BatchModule.Current.FinPeriodID.Substring(0, 4);
            parameters["PeriodFrom"] = period;
            parameters["PeriodTo"] = period;

            return parameters;
        }

        public PXAction<Batch> glReversingBatches;
        [PXUIField(DisplayName = "GL Reversing Batches", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable GLReversingBatches(PXAdapter adapter)
        {
            if (BatchModule.Current != null)
            {
                var reportParams = new Dictionary<string, string>();
                reportParams["Module"] = BatchModule.Current.Module;
                reportParams["OrigBatchNbr"] = BatchModule.Current.BatchNbr;

                throw new PXReportRequiredException(reportParams, "GL690010", "GL Reversing Batches");
            }
            return adapter.Get();
        }

        public PXAction<Batch> release;
        [PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton]
        public virtual IEnumerable Release(PXAdapter adapter)
        {
            PXCache cache = Caches[typeof(Batch)];
            List<Batch> list = new List<Batch>();
            foreach (Batch batch in adapter.Get())
            {
                if (batch.Status == BatchStatus.Balanced)
                {
                    cache.Update(batch);
                    list.Add(batch);
                }
            }
            if (list.Count == 0)
            {
                throw new PXException(Messages.BatchStatusInvalid);
            }
            Save.Press();
            if (list.Count > 0)
            {
                PXLongOperation.StartOperation(this, delegate() { ReleaseBatch(list); });
            }
            return list;
        }

        #region MyButtons (MMK)
        public PXAction<Batch> action;
        [PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable Action(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<Batch> report;
        [PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
        [PXButton(SpecialType = PXSpecialButtonType.Report)]
        protected virtual IEnumerable Report(PXAdapter adapter)
        {
            return adapter.Get();
        }
        #endregion

        public PXAction<Batch> createSchedule;
        [PXUIField(DisplayName = Messages.AddToRepeatingTasks, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXProcessButton(ImageKey = PX.Web.UI.Sprite.Main.Shedule)]
        public virtual IEnumerable CreateSchedule(PXAdapter adapter)
        {
            this.Save.Press();
            if (BatchModule.Current != null && (bool)BatchModule.Current.Released == false && (bool)BatchModule.Current.Hold == false)
            {
                ScheduleMaint sm = PXGraph.CreateInstance<ScheduleMaint>();
                if ((bool)BatchModule.Current.Scheduled && BatchModule.Current.ScheduleID != null)
                {
                    sm.Schedule_Header.Current = PXSelect<Schedule,
                                        Where<Schedule.scheduleID, Equal<Required<Schedule.scheduleID>>>>
                                        .Select(this, BatchModule.Current.ScheduleID);
                }
                else
                {
                    sm.Schedule_Header.Cache.Insert(new Schedule());
                    Batch doc = (Batch)sm.Batch_Detail.Cache.CreateInstance();
                    PXCache<Batch>.RestoreCopy(doc, BatchModule.Current);
                    doc = (Batch)sm.Batch_Detail.Cache.Update(doc);
                }
                throw new PXRedirectRequiredException(sm, "Schedule");
            }
            return adapter.Get();
        }

        public PXAction<Batch> reverseBatch;
        [PXUIField(DisplayName = Messages.ReverseBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable ReverseBatch(PXAdapter adapter)
        {
            if (BatchModule.Current != null)
            {
                this.Save.Press();
                Batch batch = PXCache<Batch>.CreateCopy(BatchModule.Current);

                if (finperiod.Current != null && (finperiod.Current.Active == false || finperiod.Current.Closed == true && glsetup.Current.PostClosedPeriods == false))
                {
                    FinPeriod firstopen = PXSelect<FinPeriod, Where<FinPeriod.closed, Equal<boolFalse>, And<FinPeriod.active, Equal<boolTrue>, And<FinPeriod.finPeriodID, Greater<Required<FinPeriod.finPeriodID>>>>>, OrderBy<Asc<FinPeriod.finPeriodID>>>.Select(this, batch.FinPeriodID);
                    if (firstopen == null)
                    {
                        BatchModule.Cache.RaiseExceptionHandling<Batch.finPeriodID>(BatchModule.Current, BatchModule.Current.FinPeriodID, new PXSetPropertyException(Messages.NoOpenPeriod));
                        return adapter.Get();
                    }
                    batch.FinPeriodID = firstopen.FinPeriodID;
                }

                this.ReverseBatchProc(batch);

                if (BatchModule.Current != null)
                {
                    BatchModule.Cache.RaiseExceptionHandling<Batch.finPeriodID>(BatchModule.Current, BatchModule.Current.FinPeriodID, null);

                    List<Batch> rs = new List<Batch>();
                    rs.Add(BatchModule.Current);
                    return rs;
                }
            }
            return adapter.Get();
        }

        public PXAction<Batch> viewDocument;
        [PXUIField(DisplayName = Messages.ViewSourceDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton()]
        public virtual IEnumerable ViewDocument(PXAdapter adapter)
        {
            if (this.GLTranModuleBatNbr.Current != null)
            {
                GLTran tran = (GLTran)this.GLTranModuleBatNbr.Current;

                IDocGraphCreator creator = null;
                switch (tran.Module)
                {
                    case PX.Objects.GL.BatchModule.AP:
                        creator = new APDocGraphCreator(); break;
                    case PX.Objects.GL.BatchModule.AR:
                        creator = new ARDocGraphCreator(); break;
                    case PX.Objects.GL.BatchModule.CA:
                        creator = new CADocGraphCreator(); break;
                    case PX.Objects.GL.BatchModule.DR:
                        creator = new DRDocGraphCreator(); break;
                    case PX.Objects.GL.BatchModule.IN:
                        creator = new INDocGraphCreator(); break;
					case PX.Objects.GL.BatchModule.PM:
						creator = new PMDocGraphCreator(); break;
                }
                if (creator != null)
                {
                    PXGraph graph = creator.Create(tran);
                    if (graph != null)
                    {
                        throw new PXRedirectRequiredException(graph, true, "ViewDocument") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                    }
                }
                throw new PXException(Messages.SourceDocumentCanNotBeFound);
            }

            return adapter.Get();
        }
        #endregion

        #region Functions
		public virtual void ReverseBatchProc(Batch batch) 
		{
			ReverseBatchProc(batch, null, null);
		}

        public virtual void ReverseBatchProc(Batch batch, string aTranType, string aRefNbr)
        {
            Clear(PXClearOption.PreserveTimeStamp);

            Batch copy = null;
            CurrencyInfo info = null;
            CurrencyInfo prev_info = null;
            bool isDocumentReversal = string.IsNullOrEmpty(aTranType) == false && string.IsNullOrEmpty(aRefNbr) == false;
			PXSelectBase<Batch> batchTransSelect = new PXSelectJoin<Batch, InnerJoin<GLTran, On<GLTran.module, Equal<Batch.module>, And<GLTran.batchNbr, Equal<Batch.batchNbr>>>, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<GLTran.curyInfoID>>>>, Where<Batch.module, Equal<Required<Batch.module>>, And<Batch.batchNbr, Equal<Required<Batch.batchNbr>>, And<GLTran.isInterCompany, Equal<False>>>>>(this);
			if (isDocumentReversal)
				batchTransSelect.WhereAnd<Where<GLTran.tranType, Equal<Required<GLTran.refNbr>>, And<GLTran.tranType, Equal<Required<GLTran.refNbr>>>>>();
            foreach (PXResult<Batch, GLTran, CurrencyInfo> res in PXSelectJoin<Batch, InnerJoin<GLTran, On<GLTran.module, Equal<Batch.module>, And<GLTran.batchNbr, Equal<Batch.batchNbr>>>, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<GLTran.curyInfoID>>>>, Where<Batch.module, Equal<Required<Batch.module>>, And<Batch.batchNbr, Equal<Required<Batch.batchNbr>>, And<GLTran.isInterCompany, Equal<False>>>>>.Select(this, batch.Module, batch.BatchNbr, aTranType, aRefNbr))
            {
                CurrencyInfo traninfo = (CurrencyInfo)res;

                if (copy == null)
                {
                    copy = PXCache<Batch>.CreateCopy((Batch)res);
					if (!isDocumentReversal)
					{
						copy.OrigBatchNbr = copy.BatchNbr;
						copy.OrigModule = copy.Module;

						if (copy.Module != GL.BatchModule.CM)
						{
							copy.Module = GL.BatchModule.GL;
						}
					}
                    copy.BatchNbr = null;
                    info = PXCache<CurrencyInfo>.CreateCopy(traninfo);
                    info.CuryInfoID = null;
                    info.IsReadOnly = false;
                    info.BaseCalc = true;
                    info = PXCache<CurrencyInfo>.CreateCopy(currencyinfo.Insert(info));

                    copy.CuryInfoID = info.CuryInfoID;
                    copy.Posted = false;
					copy.Voided = false;
					copy.Scheduled = false;
					if (isDocumentReversal == false)
					{
						copy.Released = false;
						copy.Status = BatchStatus.Balanced;
					}                    
                    copy.AutoReverseCopy = (isDocumentReversal == false) ? true : false;
                    copy.CuryDebitTotal = 0m;
                    copy.CuryCreditTotal = 0m;
                    copy.CuryControlTotal = 0m;
                    copy.FinPeriodID = batch.FinPeriodID;

                    copy = (Batch)BatchModule.Insert(copy);

                    if (info != null)
                    {
                        CurrencyInfo b_info = (CurrencyInfo)PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<Batch.curyInfoID>>>>.Select(this, null);
                        b_info.CuryID = info.CuryID;
                        b_info.CuryEffDate = info.CuryEffDate;
                        b_info.CuryRateTypeID = info.CuryRateTypeID;
                        b_info.CuryRate = info.CuryRate;
                        b_info.RecipRate = info.RecipRate;
                        b_info.CuryMultDiv = info.CuryMultDiv;
                        this.currencyinfo.Update(b_info);
                    }

                    foreach (GLAllocationAccountHistory alloc in PXSelect<GLAllocationAccountHistory, Where<GLAllocationAccountHistory.module, Equal<Required<Batch.module>>, And<GLAllocationAccountHistory.batchNbr, Equal<Required<Batch.batchNbr>>>>>.Select(this, ((Batch)res).Module, ((Batch)res).BatchNbr))
                    {
                        GLAllocationAccountHistory alloccopy = PXCache<GLAllocationAccountHistory>.CreateCopy(alloc);
                        alloccopy.BatchNbr = null;
                        alloccopy.AllocatedAmount = -1m * alloccopy.AllocatedAmount;
                        alloccopy.PriorPeriodsAllocAmount = -1m * alloccopy.PriorPeriodsAllocAmount;
                        AllocationAccountHistory.Insert(alloccopy);
                    }

                    foreach (GLAllocationHistory alloc in PXSelect<GLAllocationHistory, Where<GLAllocationHistory.module, Equal<Required<Batch.module>>, And<GLAllocationHistory.batchNbr, Equal<Required<Batch.batchNbr>>>>>.Select(this, ((Batch)res).Module, ((Batch)res).BatchNbr))
                    {
                        GLAllocationHistory alloccopy = PXCache<GLAllocationHistory>.CreateCopy(alloc);
                        alloccopy.BatchNbr = null;
                        AllocationHistory.Insert(alloccopy);
                    }
                }

                if (prev_info != null && traninfo.CuryInfoID != prev_info.CuryInfoID)
                {
                    BatchModule.Cache.RaiseExceptionHandling<Batch.origBatchNbr>(copy, null, new PXSetPropertyException(Messages.MultiplyCurrencyInfo, PXErrorLevel.Warning));
                }
                prev_info = traninfo;

                GLTran trancopy = PXCache<GLTran>.CreateCopy((GLTran)res);
				trancopy.LedgerID = null;
				if (!isDocumentReversal)
				{
					trancopy.OrigBatchNbr = trancopy.BatchNbr;
					trancopy.OrigModule = trancopy.Module;
					if (trancopy.Module != GL.BatchModule.CM)
					{
						trancopy.Module = GL.BatchModule.GL;
					}
				}
                trancopy.BatchNbr = null;
                trancopy.CATranID = null;
                trancopy.TranID = null;
                trancopy.CuryInfoID = info.CuryInfoID;

                trancopy.Qty = -1m * trancopy.Qty;
                {
                    Decimal? amount = trancopy.CuryCreditAmt;
                    trancopy.CuryCreditAmt = trancopy.CuryDebitAmt;
                    trancopy.CuryDebitAmt = amount;
                }

                {
                    Decimal? amount = trancopy.CreditAmt;
                    trancopy.CreditAmt = trancopy.DebitAmt;
                    trancopy.DebitAmt = amount;
                }

                trancopy.TranDate = null;
                trancopy.FinPeriodID = null;
                trancopy.TranPeriodID = null;
                trancopy.Released = false;
                trancopy.Posted = false;
                if (trancopy.ProjectID != null && PM.ProjectDefaultAttribute.IsNonProject(this, trancopy.ProjectID))
                {
                    trancopy.TaskID = null;
                }

                if (trancopy.CuryDebitAmt != 0m || trancopy.CuryCreditAmt != 0m)
                {
                    GLTranModuleBatNbr.Insert(trancopy);
                }
            }
        }

		protected bool _IsOffline = false;
		protected DocumentList<Batch> _created = null;
		public DocumentList<Batch> created
		{
			get
			{
				return _created;
			}
		}

		public bool IsOffLine
		{
			get
			{
				return _IsOffline;
			}
		}

        public JournalEntry()
        {
            GLSetup setup = glsetup.Current;
            PXUIFieldAttribute.SetVisible<GLTran.projectID>(GLTranModuleBatNbr.Cache, null, PM.ProjectAttribute.IsPMVisible(this, GL.BatchModule.GL));
            PXUIFieldAttribute.SetVisible<GLTran.taskID>(GLTranModuleBatNbr.Cache, null, PM.ProjectAttribute.IsPMVisible(this, GL.BatchModule.GL));
			PXUIFieldAttribute.SetVisible<GLTran.nonBillable>(GLTranModuleBatNbr.Cache, null, PM.ProjectAttribute.IsPMVisible(this, GL.BatchModule.GL));

			_created = new DocumentList<Batch>(this);
        }

		public override void SetOffline()
		{
			base.SetOffline();
			this._IsOffline = true;
		}

        public static void SegregateBatch(JournalEntry graph, string module, int? branchID, string curyID, DateTime? docDate, string finPeriodID, string description, CurrencyInfo curyInfo, byte[] tstamp)
        {
            bool consolidatedPosting = PXAccess.FeatureInstalled<FeaturesSet.consolidatedPosting>();
            SegregateBatch(graph, consolidatedPosting, module, branchID, curyID, docDate, finPeriodID, description, curyInfo, tstamp);
        }

        public static void SegregateBatch(JournalEntry graph, bool consolidatedPosting, string module, int? branchID, string curyID, DateTime? docDate, string finPeriodID, string description, CurrencyInfo curyInfo, byte[] tstamp)
        {
            graph.created.Consolidate = consolidatedPosting;
            var effectiveDate = docDate ?? (curyInfo != null ? curyInfo.CuryEffDate : null);
            var rate = consolidatedPosting == false && curyInfo != null ? curyInfo.SampleCuryRate : null;
            var rateType = curyInfo == null ? null : curyInfo.CuryRateTypeID;
            graph.Segregate(module, branchID, curyID, effectiveDate, finPeriodID, description, rate, rateType);
        }

        public virtual void Segregate(string Module, int? BranchID, string CuryID, DateTime? DocDate, string FinPeriodID, string Descr, decimal? curyRate, string curyRateType)
        {
            Segregate(Module, BranchID, CuryID, DocDate, FinPeriodID, Descr, curyRate, curyRateType, null);
        }

        public virtual void Segregate(string Module, int? BranchID, string CuryID, DateTime? DocDate, string FinPeriodID, string Descr, decimal? curyRate, string curyRateType, byte[] tstamp)
		{
			if (this.IsOffLine == false && this.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted)
			{
				this.Save.Press();
			}

			Batch existing = created.Find<Batch.module, Batch.branchID, Batch.curyID, Batch.finPeriodID>(Module, BranchID, CuryID, FinPeriodID);
			if (existing != null)
			{
				Batch newbatch = this.BatchModule.Search<Batch.batchNbr>(existing.BatchNbr, existing.Module);
                if (newbatch != null)
                {
                    if (newbatch.Description != Descr)
                    {
                        newbatch.Description = "";
                        this.BatchModule.Update(newbatch);
                    }
                    this.BatchModule.Current = newbatch;
                }
                else
                {
                    created.Remove(existing);
                    existing = null;
                }
			}
			
            if (existing == null)
			{
				this.Clear();

                if (tstamp != null)
                {
                    this.TimeStamp = tstamp;
                }


                Ledger l = PXSelectJoin<Ledger, LeftJoin<Branch, On<Ledger.ledgerID, Equal<Branch.ledgerID>>>, Where<Branch.branchID, Equal<Optional<Branch.branchID>>>>.Select(this, BranchID);
                if (l != null && Module != GL.BatchModule.GL && Module != GL.BatchModule.CM && company.Current.BaseCuryID != l.BaseCuryID)
                {
                    throw new PXException(Messages.ActualLedgerInBaseCurrency, l.LedgerCD, company.Current.BaseCuryID);
                }

				CurrencyInfo info = new CurrencyInfo();
				info.CuryID = CuryID;
				info.CuryEffDate = DocDate;
                info.CuryRateTypeID = curyRateType ?? info.CuryRateTypeID;
                info.SampleCuryRate = curyRate ?? info.SampleCuryRate;
				info = this.currencyinfo.Insert(info) ?? info;

				Batch newbatch = new Batch();
				newbatch.BranchID = BranchID;
				newbatch.Module = Module;
				newbatch.Status = "U";
				newbatch.Released = true;
				newbatch.Hold = false; 
				newbatch.DateEntered = DocDate;
				newbatch.FinPeriodID = FinPeriodID;
				newbatch.TranPeriodID = FinPeriodID;
				newbatch.CuryID = CuryID;
				newbatch.CuryInfoID = info.CuryInfoID;
				newbatch.CuryDebitTotal = 0m;
				newbatch.CuryCreditTotal = 0m;
                newbatch.DebitTotal = 0m;
                newbatch.CreditTotal = 0m;
				newbatch.Description = Descr;
				this.BatchModule.Insert(newbatch);

				CurrencyInfo b_info = this.currencyinfo.Select();
				if (b_info != null)
				{
					b_info.CuryID = CuryID;
                    this.currencyinfo.SetValueExt<CurrencyInfo.curyEffDate>(b_info, DocDate);
                    b_info.SampleCuryRate = curyRate ?? b_info.SampleCuryRate;
                    b_info.CuryRateTypeID = curyRateType ?? b_info.CuryRateTypeID;
                    this.currencyinfo.Update(b_info);
				}
			}
		}

		public static void ReleaseBatch(List<Batch> list) 
		{
			ReleaseBatch(list, null);
		}

        public static void ReleaseBatch(List<Batch> list, List<Batch> externalPostList)
        {
            PostGraph pg = PXGraph.CreateInstance<PostGraph>();
			bool doPost = (externalPostList == null);
            for (int i = 0; i < list.Count; i++)
            {
                pg.Clear(PXClearOption.PreserveData);

                Batch batch = list[i];
                pg.ReleaseBatchProc(batch);

                if ((bool)batch.AutoReverse && pg.glsetup.Current.AutoRevOption == "R")
                {
                    Batch copy = pg.ReverseBatchProc(batch);
                    list.Add(copy);
                }

                if (pg.AutoPost)
                {
					if (doPost)
					{
						pg.PostBatchProc(batch);
					}
					else 
					{
						externalPostList.Add(batch);
					}
                }
            }
        }

        public static void PostBatch(List<Batch> list)
        {
            PostGraph pg = PXGraph.CreateInstance<PostGraph>();

            for (int i = 0; i < list.Count; i++)
            {
                pg.Clear(PXClearOption.PreserveData);

                Batch batch = list[i];
                pg.PostBatchProc(batch);
            }
        }

        private void SetTransactionsChanged()
        {
            foreach (GLTran tran in GLTranModuleBatNbr.Select())
            {
                if (Caches[typeof(GLTran)].GetStatus(tran) == PXEntryStatus.Notchanged)
                {
                    Caches[typeof(GLTran)].SetStatus(tran, PXEntryStatus.Updated);
                }
            }
        }

        private void SetTransactionsChanged<Field>()
                where Field : class, IBqlField
        {
            foreach (GLTran tran in GLTranModuleBatNbr.Select())
            {
                GLTranModuleBatNbr.Cache.SetDefaultExt<Field>(tran);
                if (Caches[typeof(GLTran)].GetStatus(tran) == PXEntryStatus.Notchanged)
                {
                    Caches[typeof(GLTran)].SetStatus(tran, PXEntryStatus.Updated);
                }
            }
        }

        protected virtual void PopulateSubDescr(PXCache sender, GLTran Row, bool ExternalCall)
        {
            GLTran prevTran = null;
            foreach (GLTran tran in GLTranModuleBatNbr.Select())
            {
                if (Row == tran)
                {
                    break;
                }
                prevTran = tran;
            }

            PXResultset<CashAccount> cashAccSet = PXSelect<CashAccount, Where<CashAccount.branchID, Equal<Required<CashAccount.branchID>>,
                And<CashAccount.accountID, Equal<Required<CashAccount.accountID>>>>>.SelectWindowed(this, 0, 2, Row.BranchID, Row.AccountID);
            if (cashAccSet != null && cashAccSet.Count == 1)
            {
                CashAccount cashacc = (CashAccount)cashAccSet;
                sender.SetValue<GLTran.subID>(Row, cashacc.SubID);
            }
            else if (cashAccSet.Count == 0)
                {
                if (prevTran != null && prevTran.SubID != null)
                {
                    Sub sub = (Sub)PXSelectorAttribute.Select<GLTran.subID>(sender, prevTran);
                    if (sub != null)
                    {
                        sender.SetValueExt<GLTran.subID>(Row, sub.SubCD);
                        PXUIFieldAttribute.SetError<GLTran.subID>(sender, Row, null);
                    }
                }
                Account account = (Account)PXSelectorAttribute.Select<GLTran.accountID>(sender, Row);
                //if (account != null && account.CashSubID != null)
                //{
                //    Row.SubID = account.CashSubID;
                //}
                if (account != null && (bool)account.NoSubDetail && glsetup.Current.DefaultSubID != null)
                {
                    Row.SubID = glsetup.Current.DefaultSubID;
                }
            }

            if (prevTran != null)
            {
                Row.TranDesc = prevTran.TranDesc;
                Row.RefNbr = prevTran.RefNbr;
            }
            else
            {
                Row.TranDesc = BatchModule.Current.Description;
            }

            decimal difference = (BatchModule.Current.CuryCreditTotal ?? decimal.Zero) -
                (BatchModule.Current.CuryDebitTotal ?? decimal.Zero);
            if (PXCurrencyAttribute.IsNullOrEmpty(Row.CuryDebitAmt) && PXCurrencyAttribute.IsNullOrEmpty(Row.CuryCreditAmt))
            {
                if (difference < decimal.Zero)
                {
                    Row.CuryCreditAmt = Math.Abs(difference);
                }
                else
                {
                    Row.CuryDebitAmt = Math.Abs(difference);
                }
            }
        }

        public override void Persist()
        {
			List<PMTask> autoAllocateTasks = CreateProjectTrans();

            if (BatchModule.Current != null && BatchModule.Cache.GetStatus(BatchModule.Current) == PXEntryStatus.Inserted)
            {
                foreach (GLTran tran in GLTranModuleBatNbr.Cache.Inserted)
                {
                    if (string.Equals(tran.RefNbr, BatchModule.Current.RefNbr))
                    {
                        PXDBDefaultAttribute.SetDefaultForInsert<GLTran.refNbr>(GLTranModuleBatNbr.Cache, tran, true);
                    }
                }
            }

            base.Persist();

			if (BatchModule.Current != null)
			{
				Batch existing = created.Find(BatchModule.Current);
				if (existing == null)
				{
					created.Add(BatchModule.Current);
				}
				else
				{
					BatchModule.Cache.RestoreCopy(existing, BatchModule.Current);
				}
			}

            if (autoAllocateTasks.Count > 0)
			{
				try
				{
					AutoAllocateTasks(autoAllocateTasks);
				}
				catch (Exception ex)
				{
					throw new PXException(ex, PM.Messages.AutoAllocationFailed);
				}
			}
		}

        protected virtual void AutoAllocateTasks(List<PMTask> tasks)
		{
			PMSetup setup = PXSelect<PMSetup>.Select(this);
			bool autoreleaseAllocation = setup.AutoReleaseAllocation == true;

			PMAllocator allocator = PXGraph.CreateInstance<PMAllocator>();
			allocator.Clear();
			allocator.TimeStamp = this.TimeStamp;
			allocator.Execute(tasks);
			allocator.Actions.PressSave();

			if (allocator.Document.Current != null && autoreleaseAllocation)
			{
				List<PMRegister> list = new List<PMRegister>();
				list.Add(allocator.Document.Current);
				List<ProcessInfo<Batch>> batchList;
				bool releaseSuccess = RegisterRelease.ReleaseWithoutPost(list, false, out batchList);
				if (!releaseSuccess)
				{
					throw new PXException(PM.Messages.AutoReleaseFailed);
				}
			}
		}
		
		protected virtual List<PMTask> CreateProjectTrans()
        {
			List<PMTask> autoAllocateTasks = new List<PMTask>();

            if (BatchModule.Current != null && BatchModule.Current.Module != GL.BatchModule.GL)
            {
					PXSelectBase<GLTran> select = new PXSelectJoin<GLTran,
                        InnerJoin<Account, On<GLTran.accountID, Equal<Account.accountID>>,
                        InnerJoin<PMAccountGroup, On<PMAccountGroup.groupID, Equal<Account.accountGroupID>>,
                        InnerJoin<PMProject, On<PMProject.contractID, Equal<GLTran.projectID>, And<PMProject.nonProject, Equal<False>>>,
						InnerJoin<PMTask, On<PMTask.projectID, Equal<GLTran.projectID>, And<PMTask.taskID, Equal<GLTran.taskID>>>,
                        LeftJoin<PMTran, On<GLTran.origPMTranID, Equal<PMTran.tranID>>>>>>>,
                        Where<GLTran.module, Equal<Current<Batch.module>>,
                        And<GLTran.batchNbr, Equal<Current<Batch.batchNbr>>,
                        And<Account.accountGroupID, IsNotNull,
                        And<GLTran.pMTranID, IsNull,
                        And<PMAccountGroup.type, NotEqual<PMAccountType.offBalance>>>>>>>(this);

                if (BatchModule.Current.Module == GL.BatchModule.AP)
				{
					select.Join<LeftJoin<AP.APTran, On<AP.APTran.refNbr, Equal<GLTran.refNbr>, And<AP.APTran.lineNbr, Equal<GLTran.tranLineNbr>, And<AP.APTran.tranType, Equal<GLTran.tranType>>>>>>();
				}
				else if (BatchModule.Current.Module == GL.BatchModule.AR)
				{
					select.Join<LeftJoin<AR.ARTran, On<AR.ARTran.refNbr, Equal<GLTran.refNbr>, And<AR.ARTran.lineNbr, Equal<GLTran.tranLineNbr>, And<AR.ARTran.tranType, Equal<GLTran.tranType>>>>>>();
				}

				
                PXResultset<GLTran> resultset = select.Select();

				PMRegister doc = null;
                if (resultset.Count > 0)
                {
                    doc = new PMRegister();
                    doc.Module = BatchModule.Current.Module;
					doc.Date = BatchModule.Current.DateEntered;
					doc.Description = BatchModule.Current.Description;
					doc.Released = true;
                    ProjectDocs.Insert(doc);
                }


				Dictionary<string, PMTask> tasksToAutoAllocate = new Dictionary<string, PMTask>();
				List<PMTran> sourceForAllocation = new List<PMTran>();

                foreach (PXResult res in resultset)
                {
                    GLTran tran = (GLTran)res[0];
					Account acc = (Account)res[1];
					PMAccountGroup ag = (PMAccountGroup)res[2];
					PMProject project = (PMProject)res[3];
					PMTask task = (PMTask)res[4];
					PMTran origTran = (PMTran)res[5];
                	AP.APTran apTran = null;
					AR.ARTran arTran = null;

                    if (BatchModule.Current.Module == GL.BatchModule.AP)
					{
                        apTran = (AP.APTran)res[6];
					}
					else if (BatchModule.Current.Module == GL.BatchModule.AR)
					{
						arTran = (AR.ARTran)res[6];
					}



					PMTran pmt = (PMTran)ProjectTrans.Cache.Insert();
					pmt.BranchID = tran.BranchID;
                    pmt.AccountGroupID = acc.AccountGroupID;
                    pmt.AccountID = tran.AccountID;
                    pmt.SubID = tran.SubID;
                    pmt.BAccountID = tran.ReferenceID; //CustomerLocation is lost.
                    //pmt.BatchNbr = tran.BatchNbr;
					pmt.Date = tran.TranDate;
                    pmt.TranDate = tran.TranDate;
                    pmt.Description = tran.TranDesc;
                    pmt.FinPeriodID = tran.FinPeriodID;
                    pmt.TranPeriodID = tran.TranPeriodID;
                    pmt.InventoryID = tran.InventoryID ?? PM.PMInventorySelectorAttribute.EmptyInventoryID;
                    pmt.OrigLineNbr = tran.LineNbr;
                    pmt.OrigModule = tran.Module;
                    pmt.OrigRefNbr = tran.RefNbr;
                    pmt.OrigTranType = tran.TranType;
                    pmt.ProjectID = tran.ProjectID;
                    pmt.TaskID = tran.TaskID;
                    if (arTran != null)
                    {
                        //if this is an invoice transaction force it to be non-billable 
                        //so that it is not billed again even if the billing rule is configured to bill this account group.
                        pmt.Billable = false;
                        
                    }
                    else
						pmt.Billable = tran.NonBillable != true;

	                if (apTran != null && apTran.Date != null)
	                {
		                pmt.Date = apTran.Date;
	                }

                	pmt.UseBillableQty = true;
                    pmt.UOM = tran.UOM;
					pmt.Amount = tran.DebitAmt - tran.CreditAmt;
                    pmt.Qty = tran.Qty;//pmt.Amount >= 0 ? tran.Qty : (tran.Qty * -1);
					int sign = 1;
                    if (acc.Type == AccountType.Income || acc.Type == AccountType.Liability)
                    {
						sign = -1;
                    }
                    
                    if (acc.Type != ag.Type)
                    {
                        pmt.Amount = -pmt.Amount;
                        pmt.Qty = -pmt.Qty;
                    }
                    pmt.BillableQty = pmt.Qty;
                    pmt.Released = true;
                    pmt.Allocated = tran.OrigPMTranID != null;// do not allocate or bill reverse transaction
                    pmt.Billed = tran.OrigPMTranID != null;// do not allocate or bill reverse transaction

                    tran.PMTranID = pmt.TranID;
                    GLTranModuleBatNbr.Update(tran);

					if (apTran != null && apTran.NoteID != null)
					{
						string note = PXNoteAttribute.GetNote(Caches[typeof(AP.APTran)], apTran);
						if (note != null)
							PXNoteAttribute.SetNote(ProjectTrans.Cache, pmt, note);
						Guid[] files = PXNoteAttribute.GetFileNotes(Caches[typeof(AP.APTran)], apTran);
						if (files != null && files.Length > 0)
							PXNoteAttribute.SetFileNotes(ProjectTrans.Cache, pmt, files);
					}
					else if (arTran != null && arTran.NoteID != null)
					{
						string note = PXNoteAttribute.GetNote(Caches[typeof(AR.ARTran)], arTran);
						if (note != null)
							PXNoteAttribute.SetNote(ProjectTrans.Cache, pmt, note);
						Guid[] files = PXNoteAttribute.GetFileNotes(Caches[typeof(AR.ARTran)], arTran);
						if (files != null && files.Length > 0)
							PXNoteAttribute.SetFileNotes(ProjectTrans.Cache, pmt, files);
					}

                    PM.RegisterReleaseProcess.UpdateProjectBalance(this, pmt, ag, acc, sign);
                                        
                    //Create unbilled remainder if any.
                    if (origTran != null && origTran.AccountID == tran.AccountID && origTran.Amount > tran.CreditAmt)
                    {
                        PMTran remainder = (PMTran)ProjectTrans.Cache.CreateCopy(origTran);
                        remainder.TranID = null;
						remainder.Billed = false;
						remainder.TranType = null;
                        remainder.RefNbr = null;
                        remainder.BatchNbr = null;
                        remainder.Amount = origTran.Amount - tran.CreditAmt;
                        remainder.Qty = origTran.Qty - tran.Qty;
                        remainder.BillableQty = remainder.Qty; 
                        ProjectTrans.Insert(remainder);
                    }

					sourceForAllocation.Add(pmt);
					if (pmt.Allocated != true && project.AutoAllocate == true)
					{
						if (!tasksToAutoAllocate.ContainsKey(string.Format("{0}.{1}", task.ProjectID, task.TaskID)))
						{
							tasksToAutoAllocate.Add(string.Format("{0}.{1}", task.ProjectID, task.TaskID), task);
						}
					}
                }

				autoAllocateTasks.AddRange(tasksToAutoAllocate.Values);
            }

			return autoAllocateTasks;
        }
		
        #endregion

        #region CATran Events

        protected virtual void CATran_CashAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void CATran_FinPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void CATran_TranPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void CATran_ReferenceID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void CATran_CuryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }
        #endregion

        #region CurrencyInfo Events
        protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (ledger.Current != null)
            {
                e.NewValue = ledger.Current.BaseCuryID;
                e.Cancel = true;
            }
        }

        protected virtual void CurrencyInfo_BaseCuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (ledger.Current != null)
            {
                e.NewValue = ledger.Current.BaseCuryID;
                e.Cancel = true;
            }
        }

        protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if ((bool)CMSetup.Current.MCActivated)
            {
                e.NewValue = CMSetup.Current.GLRateTypeDflt;
            }
        }

        protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            CurrencyInfo currencyInfo = (CurrencyInfo)e.Row;
            if (currencyInfo == null || BatchModule.Current == null || BatchModule.Current.DateEntered == null) return;
            e.NewValue = BatchModule.Current.DateEntered;
            e.Cancel = true;
        }

        protected virtual void CurrencyInfo_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            CurrencyInfo info = (CurrencyInfo)e.Row;
            object CuryID = info.CuryID;
            object CuryRateTypeID = info.CuryRateTypeID;
            object CuryMultDiv = info.CuryMultDiv;
            object CuryRate = info.CuryRate;

            if (BatchModule.Current == null || BatchModule.Current.Module != GL.BatchModule.GL)
            {
                BqlCommand sel = new Select<CurrencyInfo, Where<CurrencyInfo.curyID, Equal<Required<CurrencyInfo.curyID>>, And<CurrencyInfo.curyRateTypeID, Equal<Required<CurrencyInfo.curyRateTypeID>>, And<CurrencyInfo.curyMultDiv, Equal<Required<CurrencyInfo.curyMultDiv>>, And<CurrencyInfo.curyRate, Equal<Required<CurrencyInfo.curyRate>>>>>>>();
                foreach (CurrencyInfo summ_info in sender.Cached)
                {
                    if (sender.GetStatus(summ_info) != PXEntryStatus.Deleted &&
                        sender.GetStatus(summ_info) != PXEntryStatus.InsertedDeleted)
                    {
                        if (sel.Meet(sender, summ_info, CuryID, CuryRateTypeID, CuryMultDiv, CuryRate))
                        {
                            sender.SetValue(e.Row, "CuryInfoID", summ_info.CuryInfoID);
                            sender.Delete(summ_info);
                            return;
                        }
                    }
                }
            }
        }
        #endregion

        #region Batch Events

        protected virtual void Batch_LedgerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            Batch batch = (Batch)e.Row;
            ledger.RaiseFieldUpdated(sender, e.Row);

            currencyinfo.Cache.SetDefaultExt<CurrencyInfo.baseCuryID>(currencyinfo.Current);
            currencyinfo.Cache.SetDefaultExt<CurrencyInfo.curyID>(currencyinfo.Current);
            sender.SetDefaultExt<Batch.curyID>(batch);

            foreach (GLTran tran in GLTranModuleBatNbr.Select())
            {
                GLTran newtran = PXCache<GLTran>.CreateCopy(tran);
                newtran.LedgerID = batch.LedgerID;

                GLTranModuleBatNbr.Cache.Update(newtran);
            }
        }

        protected virtual void Batch_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (ledger.Current != null)
            {
                e.NewValue = ledger.Current.BaseCuryID;
                e.Cancel = true;
            }
        }

        protected virtual void Batch_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            Batch batch = (Batch)e.Row;
            Batch oldBatch = (Batch)e.OldRow;
            if ((bool)glsetup.Current.RequireControlTotal == false || batch.Status == BatchStatus.Unposted)
            {
                if (batch.CuryCreditTotal != null && batch.CuryCreditTotal != 0)
                    cache.SetValue<Batch.curyControlTotal>(batch, batch.CuryCreditTotal);
                else if (batch.CuryDebitTotal != null && batch.CuryDebitTotal != 0)
                    cache.SetValue<Batch.curyControlTotal>(batch, batch.CuryDebitTotal);
                else
                    cache.SetValue<Batch.curyControlTotal>(batch, 0m);

                //set control total explicitly
                if (batch.CreditTotal != null && batch.CreditTotal != 0)
                    cache.SetValue<Batch.controlTotal>(batch, batch.CreditTotal);
                else if (batch.DebitTotal != null && batch.DebitTotal != 0)
                    cache.SetValue<Batch.controlTotal>(batch, batch.DebitTotal);
                else
                    cache.SetValue<Batch.controlTotal>(batch, 0m);
            }

            bool isOutOfBalance = false;

            if (batch.Status == BatchStatus.Balanced || batch.Status == BatchStatus.Scheduled || batch.Status == BatchStatus.Unposted)
            {
				if (batch.CuryDebitTotal != batch.CuryCreditTotal && batch.BatchType != BatchTypeCode.TrialBalance)
                {
                    isOutOfBalance = true;
                }

                if (glsetup.Current.RequireControlTotal == true)
                {
					if (batch.CuryCreditTotal != batch.CuryControlTotal && (ledger.Current == null || ledger.Current.BalanceType != LedgerBalanceType.Statistical) && batch.BatchType != BatchTypeCode.TrialBalance)
                    {
                        cache.RaiseExceptionHandling<Batch.curyControlTotal>(batch, batch.CuryControlTotal, new PXSetPropertyException(Messages.BatchOutOfBalance));
                    }
                    else
                    {
                        cache.RaiseExceptionHandling<Batch.curyControlTotal>(batch, batch.CuryControlTotal, null);
                    }
                }
            }

            if (batch.Status == BatchStatus.Unposted)
            {
                if (batch.DebitTotal != batch.CreditTotal)
                {
                    isOutOfBalance = true;
                }
            }

            if (isOutOfBalance && (ledger.Current == null || ledger.Current.BalanceType != LedgerBalanceType.Statistical))
            {
                cache.RaiseExceptionHandling<Batch.curyDebitTotal>(batch, batch.CuryDebitTotal, new PXSetPropertyException(Messages.BatchOutOfBalance));
            }
            else
            {
                cache.RaiseExceptionHandling<Batch.curyDebitTotal>(batch, batch.CuryDebitTotal, null);
            }

            if (batch.Status == BatchStatus.Balanced || batch.Status == BatchStatus.Hold)
            {
                if ((batch.Module == GL.BatchModule.GL && PXAccess.FeatureInstalled<FeaturesSet.taxEntryFromGL>()) 
                        && batch.CreateTaxTrans != oldBatch.CreateTaxTrans)
                {
                    if (batch.CreateTaxTrans == false)
                    {
                        foreach (GLTran iTran in this.GLTranModuleBatNbr.Select())
                        {
                            bool needUpdate = false;
                            if (String.IsNullOrEmpty(iTran.TaxID) == false)
                            {
                                iTran.TaxID = null;
                                needUpdate = true;
                            }
                            if (String.IsNullOrEmpty(iTran.TaxCategoryID) == false)
                            {
                                iTran.TaxCategoryID = null;
                                needUpdate = true;
                            }
                            if (needUpdate)
                                this.GLTranModuleBatNbr.Update(iTran);
                        }
                    }
                    else
                    {
                        foreach (GLTran iTran in this.GLTranModuleBatNbr.Select())
                        {
                            GLTranModuleBatNbr.Cache.SetDefaultExt<GLTran.taxID>(iTran);
                            GLTranModuleBatNbr.Cache.SetDefaultExt<GLTran.taxCategoryID>(iTran);
                            this.GLTranModuleBatNbr.Update(iTran);
                        }
                    }
                }
            }
        }

        protected virtual void Batch_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            Batch batch = e.Row as Batch;

            if (batch == null)
                return;

            if (currencyinfo.Current != null && object.Equals(currencyinfo.Current.CuryInfoID, batch.CuryInfoID) == false)
            {
                currencyinfo.Current = null;
            }

            if (finperiod.Current != null && object.Equals(finperiod.Current.FinPeriodID, batch.FinPeriodID) == false)
            {
                finperiod.Current = null;
            }

            bool batchNotReleased = (batch.Released != true);
            bool batchPosted = (batch.Posted == true);
            bool batchVoided = (batch.Voided == true);
            bool batchScheduled = (batch.Scheduled == true);
            bool batchModuleGL = (batch.Module == GL.BatchModule.GL);
            bool batchModuleCM = (batch.Module == GL.BatchModule.CM);
            bool batchStatusInserted = (cache.GetStatus(e.Row) == PXEntryStatus.Inserted);

            bool allowCreateTaxTrans  = PXAccess.FeatureInstalled<FeaturesSet.taxEntryFromGL>();
            allowCreateTaxTrans = allowCreateTaxTrans && batch!= null && batch.Module==GL.BatchModule.GL; 
            bool isViewSourceSupported = true;
            if (batch.Module == GL.BatchModule.GL
                || batch.Module == GL.BatchModule.CM
                || batch.Module == GL.BatchModule.FA)
                isViewSourceSupported = false;


            viewDocument.SetEnabled(isViewSourceSupported);

            PXUIFieldAttribute.SetVisible<Batch.curyID>(cache, batch, (bool)CMSetup.Current.MCActivated);
            PXUIFieldAttribute.SetVisible<Batch.createTaxTrans>(cache, batch,allowCreateTaxTrans);
            PXUIFieldAttribute.SetEnabled<Batch.createTaxTrans>(cache, batch, false);
            PXUIFieldAttribute.SetEnabled<Batch.autoReverseCopy>(cache, batch, false);

            PXDBCurrencyAttribute.SetBaseCalc<Batch.curyCreditTotal>(cache, batch, batchNotReleased);
            PXDBCurrencyAttribute.SetBaseCalc<Batch.curyDebitTotal>(cache, batch, batchNotReleased);
            PXDBCurrencyAttribute.SetBaseCalc<Batch.curyControlTotal>(cache, batch, batchNotReleased);
            PXDBCurrencyAttribute.SetBaseCalc<GLTran.curyCreditAmt>(GLTranModuleBatNbr.Cache, null, batchNotReleased);
            PXDBCurrencyAttribute.SetBaseCalc<GLTran.curyDebitAmt>(GLTranModuleBatNbr.Cache, null, batchNotReleased);

            release.SetEnabled(false);
            if (!batchModuleGL && batchStatusInserted) // || translationhistory_reversing.Current == null           /*(!batchModuleCM) &&*/
            {
                PXUIFieldAttribute.SetEnabled(cache, batch, false);
                cache.AllowUpdate = false;
                cache.AllowDelete = false;
                GLTranModuleBatNbr.Cache.AllowDelete = false;
                GLTranModuleBatNbr.Cache.AllowUpdate = false;
                GLTranModuleBatNbr.Cache.AllowInsert = false;
            }
            else if (batchVoided || !batchNotReleased)
            {
                PXUIFieldAttribute.SetEnabled(cache, batch, false);
                cache.AllowDelete = false;
                cache.AllowUpdate = false;
                GLTranModuleBatNbr.Cache.AllowDelete = false;
                GLTranModuleBatNbr.Cache.AllowUpdate = false;
                GLTranModuleBatNbr.Cache.AllowInsert = false;
            }
            else
            {
                PXUIFieldAttribute.SetEnabled(cache, batch, true);
                PXUIFieldAttribute.SetEnabled<Batch.status>(cache, batch, false);
                PXUIFieldAttribute.SetEnabled<Batch.curyCreditTotal>(cache, batch, false);
                PXUIFieldAttribute.SetEnabled<Batch.curyDebitTotal>(cache, batch, false);
                PXUIFieldAttribute.SetEnabled<Batch.origBatchNbr>(cache, batch, false);
                PXUIFieldAttribute.SetEnabled<Batch.hold>(cache, batch, (batch.Scheduled != true));
                PXUIFieldAttribute.SetEnabled<Batch.autoReverse>(cache, batch, (batch.AutoReverseCopy != true));
                PXUIFieldAttribute.SetEnabled<Batch.autoReverseCopy>(cache, batch, false);
                cache.AllowDelete = true;
                cache.AllowUpdate = true;
                GLTranModuleBatNbr.Cache.AllowDelete = true;
                GLTranModuleBatNbr.Cache.AllowUpdate = true;
                GLTranModuleBatNbr.Cache.AllowInsert = true;
                bool isAutoReverse = ((batch.AutoReverseCopy == true) || (batch.AutoReverse == true));
                PXUIFieldAttribute.SetEnabled<Batch.createTaxTrans>(cache, batch, allowCreateTaxTrans && (isAutoReverse != true));
            }
            release.SetEnabled(batchNotReleased && batch.Scheduled != true && batch.Status != BatchStatus.Hold && !batchVoided);
            createSchedule.SetEnabled(batchModuleGL && !batchVoided && !batchStatusInserted && (batchNotReleased || batch.Scheduled == true));

            PXUIFieldAttribute.SetEnabled<Batch.module>(cache, batch);
            PXUIFieldAttribute.SetEnabled<Batch.batchNbr>(cache, batch);
            PXUIFieldAttribute.SetVisible<Batch.scheduleID>(cache, batch, false);
            PXUIFieldAttribute.SetVisible<Batch.curyControlTotal>(cache, batch, (bool)glsetup.Current.RequireControlTotal);

			if (!this.IsImport)
			{
				PXUIFieldAttribute.SetVisible<GLTran.tranDate>(GLTranModuleBatNbr.Cache, null, !batchModuleGL);

				bool createTaxTran = ShouldCreateTaxTrans();
				PXUIFieldAttribute.SetVisible<GLTran.taxID>(GLTranModuleBatNbr.Cache, null, createTaxTran);
				PXUIFieldAttribute.SetVisible<GLTran.taxCategoryID>(GLTranModuleBatNbr.Cache, null, createTaxTran);
			}

	        PXUIFieldAttribute.SetVisible<Batch.skipTaxValidation>(cache, batch, batch.CreateTaxTrans == true);

            batchRegisterDetails.SetEnabled(!batchNotReleased);
            glEditDetails.SetEnabled(batchNotReleased && !batchPosted && !batchStatusInserted);
            
            bool canReverse = (batch.Released == true) && (batch.Module != GL.BatchModule.CM);
            this.reverseBatch.SetEnabled(canReverse);
            this.glReversingBatches.SetEnabled(canReverse);
        }

        protected virtual void Batch_DateEntered_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            Batch batch = (Batch)e.Row;

            SetTransactionsChanged<GLTran.tranDate>();

            CurrencyInfoAttribute.SetEffectiveDate<Batch.dateEntered>(cache, e);
        }

        protected virtual void Batch_FinPeriodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            SetTransactionsChanged();
        }

        protected virtual void Batch_Module_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = GL.BatchModule.GL;
        }

        protected virtual void Batch_AutoReverse_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            Batch row = (Batch) e.Row;
            if(row.AutoReverse == true )
            {
                cache.SetValueExt<Batch.createTaxTrans>(row, false);
            } 
        }

        protected virtual void Batch_AutoReverseCopy_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            Batch row = (Batch)e.Row;
            if (row.AutoReverseCopy == true)
            {
                cache.SetValueExt<Batch.createTaxTrans>(row, false);
            }
        }

        protected virtual void Batch_BatchNbr_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
        {
            Batch batch = e.Row as Batch;

            if (batch == null)
                return;
            bool batchNotReleased = (batch.Released != true);
            PXDBCurrencyAttribute.SetBaseCalc<Batch.curyCreditTotal>(cache, batch, batchNotReleased);
            PXDBCurrencyAttribute.SetBaseCalc<Batch.curyDebitTotal>(cache, batch, batchNotReleased);
        }
        #endregion

        #region GLTran Events
        private bool _importing;

        protected virtual void GLTran_AccountID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            if (e.Row != null) _importing = sender.GetValuePending(e.Row, PXImportAttribute.ImportFlag) != null;
		}

        protected virtual void GLTran_AccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            GLTran row = e.Row as GLTran;
            if (row != null && row.ProjectID != null && !ProjectDefaultAttribute.IsNonProject(this, row.ProjectID) && row.TaskID != null)//taskID for Contract is null.
            {
                Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, e.NewValue);
                if (account != null && account.AccountGroupID == null)
                {
                    sender.RaiseExceptionHandling<GLTran.accountID>(e.Row, account.AccountCD, new PXSetPropertyException(PM.Messages.NoAccountGroup, PXErrorLevel.Error, account.AccountCD));
                }
            }
        }

        protected virtual void GLTran_LedgerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            sender.SetDefaultExt<GLTran.projectID>(e.Row);
        }

        protected virtual void GLTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            GLTran tran = (GLTran)e.Row;
            if (e.ExternalCall && (e.Row == null || !_importing) && sender.GetStatus(tran) == PXEntryStatus.Inserted && ((GLTran)e.OldRow).AccountID == null && tran.AccountID != null)
            {
                GLTran oldrow = PXCache<GLTran>.CreateCopy(tran);
                PopulateSubDescr(sender, tran, e.ExternalCall);
                sender.RaiseRowUpdated(tran, oldrow);
            }
        }

        protected virtual void GLTran_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            GLTran tran = (GLTran)e.Row;
            if (e.ExternalCall && (e.Row == null || !_importing) && sender.GetStatus(tran) == PXEntryStatus.Inserted && tran.AccountID != null)
            {
                GLTran oldrow = PXCache<GLTran>.CreateCopy(tran);
                PopulateSubDescr(sender, tran, e.ExternalCall);
                sender.RaiseRowUpdated(tran, oldrow);
            }
        }

        protected virtual void GLTran_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            GLTran tran = (GLTran)e.Row;
            object BranchID = tran.BranchID;
            object AccountID = tran.AccountID;
            object SubID = tran.SubID;
            object CuryInfoID = tran.CuryInfoID;
            object ProjectID = tran.ProjectID;
            object TaskID = tran.TaskID;
            object CATranID = tran.CATranID;

            if (tran.RefNbr == null)
            {
                tran.RefNbr = string.Empty;
            }

            if (tran.TranDesc == null)
            {
                tran.TranDesc = string.Empty;
            }

            object RefNbr = tran.RefNbr;
            object TranDesc = tran.TranDesc;


            if (tran.Module != GL.BatchModule.GL)
            {
                if ((bool)tran.SummPost)
                {
                    e.Cancel = PostGraph.UpdateTran(sender, (GLTran)e.Row,
                        PXSelect<GLTran,
                            Where<GLTran.module, Equal<Current<Batch.module>>,
                            And<GLTran.batchNbr, Equal<Current<Batch.batchNbr>>,
                            And<GLTran.curyInfoID, Equal<Required<GLTran.curyInfoID>>,
                            And<GLTran.branchID, Equal<Required<GLTran.branchID>>,
                            And<GLTran.accountID, Equal<Required<GLTran.accountID>>,
                            And<GLTran.subID, Equal<Required<GLTran.subID>>,
                            And<GLTran.refNbr, Equal<Required<GLTran.refNbr>>,
                            And<GLTran.summPost, Equal<True>,
                            And2<Where<GLTran.cATranID, Equal<Required<GLTran.cATranID>>, Or<GLTran.cATranID, IsNull, And<Required<GLTran.cATranID>, IsNull>>>,
                            And2<Where<GLTran.projectID, Equal<Required<GLTran.projectID>>, Or<GLTran.projectID, IsNull, And<Required<GLTran.projectID>, IsNull>>>,
                            And<Where<GLTran.taskID, Equal<Required<GLTran.taskID>>, Or<GLTran.taskID, IsNull, And<Required<GLTran.taskID>, IsNull>>>>>>>>>>>>>>>.Select(this, CuryInfoID, BranchID, AccountID, SubID, RefNbr, CATranID, CATranID, ProjectID, ProjectID, TaskID, TaskID));
                    if (e.Cancel == false)
                    {
						PostGraph.NormalizeAmounts(tran);
                    }
                }
                else
                {
                    PostGraph.NormalizeAmounts(tran);
                }

                if (e.Cancel == false)
                {
                    e.Cancel = (tran.CuryDebitAmt == 0 &&
                                tran.CuryCreditAmt == 0 &&
                                tran.DebitAmt == 0 &&
								tran.CreditAmt == 0 &&
								tran.ZeroPost != true);
                }

				if (e.Cancel == false)
				{
					BranchAcctMapFrom mapfrom;
					BranchAcctMapTo mapto;

					if (!PostGraph.GetAccountMapping(this, BatchModule.Current, tran, out mapfrom, out mapto))
					{
						Branch branchfrom = (Branch)PXSelectorAttribute.Select<Batch.branchID>(BatchModule.Cache, BatchModule.Current, BatchModule.Current.BranchID);
						Branch branchto = (Branch)PXSelectorAttribute.Select<GLTran.branchID>(sender, tran, tran.BranchID);

						throw new PXException(Messages.BrachAcctMapMissing, (branchfrom != null ? branchfrom.BranchCD : "Undefined"), (branchto != null ? branchto.BranchCD : "Undefined"));
					}
				}
            }
        }

		protected virtual void Batch_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<Batch.ledgerID>(e.Row);
		}


		protected virtual void GLTran_AccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Account a = (Account)PXSelectorAttribute.Select<GLTran.accountID>(sender, e.Row);
			if (a != null)
			{
				sender.SetDefaultExt<GLTran.projectID>(e.Row);
                sender.SetDefaultExt<GLTran.taxID>(e.Row);
                sender.SetDefaultExt<GLTran.taxCategoryID>(e.Row);
			}
		}

        protected virtual void GLTran_SubID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            GLTran row = (GLTran)e.Row;
            if (row!= null)
            {                
                sender.SetDefaultExt<GLTran.taxID>(e.Row);
            }
        }

		protected virtual void GLTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			GLTran row = e.Row as GLTran;
			if (row != null && row.ProjectID == null)
			{
				Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, row.AccountID);
				if (account != null && account.AccountGroupID != null)
				{
					sender.RaiseExceptionHandling<GLTran.projectID>(e.Row, row.ProjectID, new PXSetPropertyException(Messages.ProjectIsRequired, account.AccountCD));
				}
			}

			if (row != null && row.ProjectID != null && !ProjectDefaultAttribute.IsNonProject(this, row.ProjectID) && row.TaskID != null)//taskID for Contract is null.
			{
				Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, row.AccountID);
				if (account != null && account.AccountGroupID == null)
				{
					sender.RaiseExceptionHandling<GLTran.accountID>(e.Row, account.AccountCD, new PXSetPropertyException(PM.Messages.NoAccountGroup, PXErrorLevel.Error, account.AccountCD));
				}
			}

			if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
			{
				if (((GLTran)e.Row).AccountBranchID != null)
				{
					sender.SetValue<GLTran.branchID>(e.Row, ((GLTran)e.Row).AccountBranchID);
				}
			}
		}

        protected virtual void GLTran_CuryCreditAmt_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            GLTran row = (GLTran)e.Row;

            if (row != null)
            {
                if (row.CuryDebitAmt != null && row.CuryDebitAmt != 0 && row.CuryCreditAmt != null && row.CuryCreditAmt != 0)
                {
                    row.CuryDebitAmt = 0.0m;
                    row.DebitAmt = 0.0m;
                }
                //row.CreditAmt = row.CuryCreditAmt;
            }
        }

        protected virtual void GLTran_CuryDebitAmt_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            GLTran row = (GLTran)e.Row;

            if (row != null)
            {
                if (row.CuryCreditAmt != null && row.CuryCreditAmt != 0 && row.CuryDebitAmt != null && row.CuryDebitAmt != 0)
                {
                    row.CuryCreditAmt = 0.0m;
                    row.CreditAmt = 0.0m;
                }
                //row.DebitAmt = row.CuryDebitAmt;
            }
        }

		protected virtual void GLTran_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			GLTran row = e.Row as GLTran;
			if (row == null) return;

			row.TaskID = null;
		}

        protected virtual void GLTran_TaxID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            GLTran row = e.Row as GLTran;
            Batch batch = this.BatchModule.Current;
            
            if (batch != null && batch.CreateTaxTrans == true)
            {
                e.NewValue = null;
                if(row.AccountID !=null && row.SubID != null) 
                {
                    PXResultset<TX.Tax> taxset = PXSelect<TX.Tax, Where2<Where<TX.Tax.purchTaxAcctID, Equal<Required<GLTran.accountID>>,
                                And<TX.Tax.purchTaxSubID, Equal<Required<GLTran.subID>>>>,
                                Or<Where<TX.Tax.salesTaxAcctID, Equal<Required<GLTran.accountID>>,
                                And<TX.Tax.salesTaxSubID, Equal<Required<GLTran.subID>>>>>>>.Select(this, row.AccountID, row.SubID, row.AccountID, row.SubID);
                    if (taxset.Count == 1) 
                    {
                        e.NewValue = ((Tax)taxset[0]).TaxID;
                    }
                    else if (taxset.Count > 1)
                    {
                        this.GLTranModuleBatNbr.Cache.RaiseExceptionHandling<GLTran.taxID>(row, null, new PXSetPropertyException(Messages.TaxIDMissingForAccountAssociatedWithTaxes, PXErrorLevel.Warning));
                        //MS This is needed because of the usage PopulateSubDescr makes last RowSelected call before SubID is initialized - so the warning does not appear for the new records with  "right" default values.
                    }
                }
                e.Cancel = true;
            }
            
        }

        protected virtual void GLTran_TaxCategoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (ShouldCreateTaxTrans() == false)
            {
                e.Cancel = true;
            }
        }

		protected virtual void GLTran_TaxID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			GLTran tran = e.Row as GLTran;
			if (tran == null) return;
			GLTran newtran = sender.CreateCopy(tran) as GLTran;
			newtran.TaxID = e.NewValue as string;
			if (newtran.TaxID != null)
			{
				Tax tax = PXSelectorAttribute.Select<GLTran.taxID>(sender, newtran) as Tax;
				if (tax != null)
				{
					if (tax.PurchTaxAcctID == tax.SalesTaxAcctID && tax.PurchTaxSubID == tax.SalesTaxSubID)
                    {
                        sender.RaiseExceptionHandling<GLTran.taxID>(tran, tax.TaxID, new PXSetPropertyException(TX.Messages.ClaimableAndPayableAccountsAreTheSame, tax.TaxID));
                        e.NewValue = tran.TaxID;
                        e.Cancel = true;
                        return;
                    }
					string taxType = (tax.PurchTaxAcctID == tran.AccountID && tax.PurchTaxSubID == tran.SubID) ? TaxType.Purchase :
						                 (tax.SalesTaxAcctID == tran.AccountID && tax.SalesTaxSubID == tran.SubID) ? TaxType.Sales : null;
					if (taxType != null)
					{
						TaxRev taxrev = PXSelectReadonly<TaxRev, Where<TaxRev.taxID, Equal<Required<TaxRev.taxID>>,
							And<TaxRev.outdated, Equal<boolFalse>,
								And<TaxRev.taxType, Equal<Required<TaxRev.taxType>>>>>>.SelectWindowed(sender.Graph, 0, 1, tax.TaxID, taxType);
						if (taxrev == null)
                        {
                            sender.RaiseExceptionHandling<GLTran.taxID>(tran, tax.TaxID, new PXSetPropertyException(TX.Messages.TaxRevNotFound, tax.TaxID, taxType));
                            e.NewValue = tran.TaxID;
                            e.Cancel = true;
                            return;
                        }
					}
				}
			}
		}
		
		protected virtual void GLTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            bool createTaxTran = ShouldCreateTaxTrans();

            if (createTaxTran && e.Row!= null)
            {
                PXFieldState state = (PXFieldState) sender.GetStateExt<GLTran.taxID>(e.Row); //Needed to prevent error from being overriden by warning
                if (String.IsNullOrEmpty(state.Error) || state.IsWarning == true)
                {
                    WarnIfMissingTaxID(e.Row as GLTran);
                }
            }
        }

        private bool ShouldCreateTaxTrans()
        {
            Batch batch = this.BatchModule.Current;
            bool allowCreateTaxTrans = PXAccess.FeatureInstalled<FeaturesSet.taxEntryFromGL>();
            allowCreateTaxTrans = allowCreateTaxTrans && batch != null && batch.Module == GL.BatchModule.GL; 
            return allowCreateTaxTrans && (batch.CreateTaxTrans ?? false);
        }

        private void WarnIfMissingTaxID(GLTran tran)
        {
            bool needWarning = false;
            if (tran != null && tran.AccountID != null && tran.SubID != null && tran.TaxID == null)
            {
                PXResultset<TX.Tax> taxset = PXSelect<TX.Tax, Where2<Where<TX.Tax.purchTaxAcctID, Equal<Required<GLTran.accountID>>,
                            And<TX.Tax.purchTaxSubID, Equal<Required<GLTran.subID>>>>,
                            Or<Where<TX.Tax.salesTaxAcctID, Equal<Required<GLTran.accountID>>,
                            And<TX.Tax.salesTaxSubID, Equal<Required<GLTran.subID>>>>>>>.Select(this, tran.AccountID, tran.SubID, tran.AccountID, tran.SubID);

                if (taxset.Count > 0 && tran.TaxID == null)
                {
                    needWarning = true;                    
                }
            }
            if (tran != null)
            {                
                if (needWarning)
                {
                    this.GLTranModuleBatNbr.Cache.RaiseExceptionHandling<GLTran.taxID>(tran, null, new PXSetPropertyException(Messages.TaxIDMissingForAccountAssociatedWithTaxes, PXErrorLevel.Warning));
                }
                else
                {
                    this.GLTranModuleBatNbr.Cache.RaiseExceptionHandling<GLTran.taxID>(tran, null, null);
                }
            }
        }
        #endregion

		#region PMRegister Events

		protected virtual void PMRegister_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			//CreateProjectTrans() can create more then one PMRegister because of autoallocation
			//hense set the RefNbr Manualy for all child transactios.

			PMRegister row = (PMRegister)e.Row;

			if (e.Operation == PXDBOperation.Insert)
			{
				if (e.TranStatus == PXTranStatus.Open)
				{
					foreach (PMTran tran in ProjectTrans.Cache.Inserted)
					{
						if (tran.TranType == row.Module)
						{
							tran.RefNbr = row.RefNbr;
						}

					}
				}
			}
		}
		
		#endregion

        #region Implementation of IPXPrepareItems

        public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
        {
            if (viewName == "GLTranModuleBatNbr")
            {
                var creditAmt = CorrectImportValue(values, "CreditAmt", "0");
                CorrectImportValue(values, "CuryCreditAmt", creditAmt);
                var debitAmt = CorrectImportValue(values, "DebitAmt", "0");
                CorrectImportValue(values, "CuryDebitAmt", debitAmt);
            }
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

        public virtual void PrepareItems(string viewName, IEnumerable items)
        {
        }

        private static string CorrectImportValue(IDictionary dic, string fieldName, string defValue)
        {
            var result = defValue;
            if (!dic.Contains(fieldName)) dic.Add(fieldName, defValue);
            else
            {
                var val = dic[fieldName];
                Decimal mVal;
                string sVal;
                if (val == null ||
                    string.IsNullOrEmpty(sVal = val.ToString()) ||
                    !decimal.TryParse(sVal, out mVal))
                {
                    dic[fieldName] = defValue;
                }
                else result = sVal;
            }
            return result;
        }

        #endregion
    }

    [Serializable]
    public class PostGraph : PXGraph<PostGraph>
    {
		#region Cache Attached Events
		#region GLTran
		#region BranchID
		[PXDBInt()]
		[PXSelector(typeof(Search<Branch.branchID>), SubstituteKey = typeof(Branch.branchCD), CacheGlobal = true)]
		protected virtual void GLTran_BranchID_CacheAttached(PXCache cache)
		{
		}
		#endregion
		#region LedgerID
		[PXDBInt()]
		protected virtual void GLTran_LedgerID_CacheAttached(PXCache cache)
		{
		}
		#endregion
		#region AccountID
		[PXDBInt()]
		[PXSelector(typeof(Search<Account.accountID>), SubstituteKey = typeof(Account.accountCD), CacheGlobal = true)]
		protected virtual void GLTran_AccountID_CacheAttached(PXCache cache)
		{
		}
		#endregion
		#region SubID
		[PXDBInt()]
		protected virtual void GLTran_SubID_CacheAttached(PXCache cache)
		{
		}
		#endregion
		#region OrigAccountID
		[PXDBInt()]
		protected virtual void GLTran_OrigAccountID_CacheAttached(PXCache cache)
		{
		}
		#endregion
		#region OrigSubID
		[PXDBInt()]
		protected virtual void GLTran_OrigSubID_CacheAttached(PXCache cache)
		{
		}
		#endregion
		#region RefNbr
		[PXDBString(15, IsUnicode = true)]
		protected virtual void GLTran_RefNbr_CacheAttached(PXCache cache)
		{
		}
		#endregion
		#region TranDesc
		[PXDBString(255, IsUnicode = true)]
		protected virtual void GLTran_TranDesc_CacheAttached(PXCache cache)
		{
		}
		#endregion
		#region DebitAmt
		[PXDBDecimal(4)]
		protected virtual void GLTran_DebitAmt_CacheAttached(PXCache cache)
		{
		}
		#endregion
		#region CreditAmt
		[PXDBDecimal(4)]
		protected virtual void GLTran_CreditAmt_CacheAttached(PXCache cache)
		{
		}
		#endregion
		#region CuryDebitAmt
		[PXDBDecimal(4)]
		protected virtual void GLTran_CuryDebitAmt_CacheAttached(PXCache cache)
		{
		}
		#endregion
		#region CuryCreditAmt
		[PXDBDecimal(4)]
		protected virtual void GLTran_CuryCreditAmt_CacheAttached(PXCache cache)
		{
		}
		#endregion
		#region CuryInfoID
		[PXDBLong()]
		protected virtual void GLTran_CuryInfoID_CacheAttached(PXCache cache)
		{
		}
		#endregion
		#region CATranID
		[PXDBLong()]
		protected virtual void GLTran_CATranID_CacheAttached(PXCache cache)
		{
		}
		#endregion
		#region ProjectID
		[PXDBInt()]
		protected virtual void GLTran_ProjectID_CacheAttached(PXCache cache)
		{
		}
		#endregion
		#region TaskID
		[PXDBInt()]
		protected virtual void GLTran_TaskID_CacheAttached(PXCache cache)
		{
		}
		#endregion
		#region PMTranID
		[PXDBLong()]
		protected virtual void GLTran_PMTranID_CacheAttached(PXCache cache)
		{
		}
		#endregion
		#endregion
		#region PMRegister
		#region BatchNbr
		[PXDBLiteDefault(typeof(Batch.batchNbr))]
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "BatchNbr")]
		protected virtual void PMRegister_BatchNbr_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion
		#region PMTran
		#region TranID
		[PXDBLongIdentity(IsKey = true)]
		protected virtual void PMTran_TranID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RefNbr
		[PXDBLiteDefault(typeof(PMRegister.refNbr))]// is handled by the graph
		[PXDBString(15, IsUnicode = true)]
		protected virtual void PMTran_RefNbr_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region BatchNbr
        [PXDBLiteDefault(typeof(Batch.batchNbr), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "BatchNbr")]
		protected virtual void PMTran_BatchNbr_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region Date
		[PXDBDate()]
		[PXDefault(typeof(PMRegister.date))]
		public virtual void PMTran_Date_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion

        #region TaxTran
        #region FinPeriodID        
        [GL.FinPeriodID()]
        [PXDefault()]		
        protected virtual void TaxTran_FinPeriodID_CacheAttached(PXCache cache)
        {
        }
        #endregion
        #region TaxPeriodID
        [GL.FinPeriodID()]        
        protected virtual void TaxTran_TaxPeriodID_CacheAttached(PXCache cache)
        {
        }
        #endregion
        #region VendorID
        [PXDBInt()]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]        
        protected virtual void TaxTran_VendorID_CacheAttached(PXCache cache)
        {
        }
        #endregion
        #region RefNbr
        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Ref. Nbr.", Enabled = false, Visible = false)]
        protected virtual void TaxTran_RefNbr_CacheAttached(PXCache cache)
        {
        }
        #endregion
        #region TranDate
        [PXDBDate()]
        [PXDefault()]
        protected virtual void TaxTran_TranDate_CacheAttached(PXCache cache)
        {
        }
        #endregion
        #region TranType
        [PXDBString(3, IsKey = true, IsFixed = true)]
        [PXDefault(" ")]
        protected virtual void TaxTran_TranType_CacheAttached(PXCache cache)
        {
        }
        #endregion
        #region TaxZoneID
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Zone")]		        
        protected virtual void TaxTran_TaxZoneID_CacheAttached(PXCache cache)
        {
        }
        #endregion
        #endregion
		#endregion

        public PXSelectJoin<GLTran,
            LeftJoin<CurrencyInfo, On<GLTran.curyInfoID, Equal<CurrencyInfo.curyInfoID>>,
            LeftJoin<Account, On<GLTran.accountID, Equal<Account.accountID>>,
            LeftJoin<Ledger, On<GLTran.ledgerID, Equal<Ledger.ledgerID>>>>>,
            Where<GLTran.module, Equal<Optional<Batch.module>>,
            And<GLTran.batchNbr, Equal<Optional<Batch.batchNbr>>>>> GLTran_Module_BatNbr;
        public PXSelectReadonly2<GLTran,
            LeftJoin<CATran, On<CATran.tranID, Equal<GLTran.cATranID>>,
            LeftJoin<CurrencyInfo, On<GLTran.curyInfoID, Equal<CurrencyInfo.curyInfoID>>,
            LeftJoin<Account, On<GLTran.accountID, Equal<Account.accountID>>,
            LeftJoin<Ledger, On<GLTran.ledgerID, Equal<Ledger.ledgerID>>>>>>,
            Where<GLTran.module, Equal<Optional<Batch.module>>,
            And<GLTran.batchNbr, Equal<Optional<Batch.batchNbr>>>>> GLTran_CATran_Module_BatNbr;

        public PXSelect<Batch, Where<Batch.module, Equal<Optional<Batch.module>>>> BatchModule;
        public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Optional<GLTran.curyInfoID>>>, OrderBy<Asc<CurrencyInfo.curyInfoID>>> CurrencyInfo_ID;
        public PXSelectReadonly<Account, Where<Account.accountID, Equal<Required<GLTran.accountID>>>, OrderBy<Asc<Account.accountCD>>> Account_AccountID;
        public PXSelectReadonly<Ledger, Where<Ledger.ledgerID, Equal<Optional<GLTran.ledgerID>>>, OrderBy<Asc<Ledger.ledgerCD>>> Ledger_LedgerID;

        public PXSelectJoin<GLAllocationAccountHistory,
            InnerJoin<Account, On<Account.accountID, Equal<GLAllocationAccountHistory.accountID>>>,
            Where<GLAllocationAccountHistory.batchNbr, Equal<Required<GLAllocationAccountHistory.batchNbr>>,
            And<GLAllocationAccountHistory.module, Equal<Required<GLAllocationAccountHistory.module>>>>>
            BatchAllocHistory;

        public PXSelect<TaxTran,Where<TaxTran.module,Equal<GL.BatchModule.moduleGL>,
                            And<TaxTran.module,Equal<Optional<Batch.module>>,
                            And<TaxTran.refNbr,Equal<Optional<Batch.batchNbr>>>>>> GL_GLTran_Taxes;

        public PXSelect<FinPeriod, Where<FinPeriod.finPeriodID, Less<Required<FinPeriod.finPeriodID>>>, OrderBy<Desc<FinPeriod.finPeriodID>>> ClosedCheck;
        public PXSelectReadonly<FinPeriod, Where<FinPeriod.finYear, Greater<Optional<FinPeriod.finYear>>>, OrderBy<Asc<FinPeriod.finPeriodID>>> NextFiscalYear;
        public PXSelect<CATran> catran;
        public PXSelect<PMRegister> ProjectDocs;
        public PXSelect<PMTran> ProjectTrans;

        public PXSetup<GLSetup> glsetup;
        Account niacct = null;
        Account reacct = null;

        public bool AutoPost
        {
            get
            {
                return glsetup.Current.AutoPostOption == true;
            }
        }

        public bool AutoRevEntry
        {
            get
            {
                return glsetup.Current.AutoRevEntry == true;
            }
        }

        protected bool _IsIntegrityCheck = false;

        public PostGraph()
        {
            PXDBCurrencyAttribute.SetBaseCalc<GLTran.curyCreditAmt>(GLTran_Module_BatNbr.Cache, null, false);
            PXDBCurrencyAttribute.SetBaseCalc<GLTran.curyDebitAmt>(GLTran_Module_BatNbr.Cache, null, false);

            PXDBCurrencyAttribute.SetBaseCalc<Batch.curyCreditTotal>(BatchModule.Cache, null, false);
            PXDBCurrencyAttribute.SetBaseCalc<Batch.curyDebitTotal>(BatchModule.Cache, null, false);
            PXDBCurrencyAttribute.SetBaseCalc<Batch.curyControlTotal>(BatchModule.Cache, null, false);
        }

		public static void NormalizeAmounts(GLTran tran)
		{
			if ((tran.CuryDebitAmt - tran.CuryCreditAmt) > 0m && (tran.DebitAmt - tran.CreditAmt) < 0m ||
				(tran.CuryDebitAmt - tran.CuryCreditAmt) < 0m && (tran.DebitAmt - tran.CreditAmt) > 0m)
			{
				return;
			}

			if ((tran.CuryDebitAmt - tran.CuryCreditAmt) != decimal.Zero)
			{
				if ((tran.CuryDebitAmt - tran.CuryCreditAmt) < 0m)
				{
					tran.CuryCreditAmt = Math.Abs((decimal)tran.CuryDebitAmt - (decimal)tran.CuryCreditAmt);
					tran.CreditAmt = Math.Abs((decimal)tran.DebitAmt - (decimal)tran.CreditAmt);
					tran.CuryDebitAmt = 0m;
					tran.DebitAmt = 0m;
				}
				else
				{
					tran.CuryDebitAmt = Math.Abs((decimal)tran.CuryDebitAmt - (decimal)tran.CuryCreditAmt);
					tran.DebitAmt = Math.Abs((decimal)tran.DebitAmt - (decimal)tran.CreditAmt);
					tran.CuryCreditAmt = 0m;
					tran.CreditAmt = 0m;
				}
			}
			else
			{
				if ((tran.DebitAmt - tran.CreditAmt) < 0m)
				{
					tran.CuryCreditAmt = Math.Abs((decimal)tran.CuryDebitAmt - (decimal)tran.CuryCreditAmt);
					tran.CreditAmt = Math.Abs((decimal)tran.DebitAmt - (decimal)tran.CreditAmt);
					tran.CuryDebitAmt = 0m;
					tran.DebitAmt = 0m;
				}
				else
				{
					tran.CuryDebitAmt = Math.Abs((decimal)tran.CuryDebitAmt - (decimal)tran.CuryCreditAmt);
					tran.DebitAmt = Math.Abs((decimal)tran.DebitAmt - (decimal)tran.CreditAmt);
					tran.CuryCreditAmt = 0m;
					tran.CreditAmt = 0m;
				}
			}
		}

		public static bool UpdateTran(PXCache sender, GLTran tran, PXResultset<GLTran> summary)
		{
			foreach (GLTran summ_tran in summary)
			{
                PXParentAttribute.SetParent(sender, summ_tran, typeof(Batch), sender.Graph.Caches[typeof(Batch)].Current);

				GLTran copy_tran = PXCache<GLTran>.CreateCopy(summ_tran);
				copy_tran.CuryCreditAmt += tran.CuryCreditAmt;
				copy_tran.CreditAmt += tran.CreditAmt;
				copy_tran.CuryDebitAmt += tran.CuryDebitAmt;
				copy_tran.DebitAmt += tran.DebitAmt;

				if ((copy_tran.CuryDebitAmt - copy_tran.CuryCreditAmt) > 0m && (copy_tran.DebitAmt - copy_tran.CreditAmt) < 0m ||
					(copy_tran.CuryDebitAmt - copy_tran.CuryCreditAmt) < 0m && (copy_tran.DebitAmt - copy_tran.CreditAmt) > 0m)
				{
					continue;
				}

				NormalizeAmounts(copy_tran);

				if (copy_tran.CuryDebitAmt == 0m &&
					copy_tran.CuryCreditAmt == 0m &&
					copy_tran.DebitAmt == 0m &&
					copy_tran.CreditAmt == 0m &&
					copy_tran.ZeroPost != true)
				{
					sender.Delete(copy_tran);
				}
				else
				{
					if (!object.Equals(copy_tran.TranDesc, tran.TranDesc))
					{
						copy_tran.TranDesc = Messages.SummaryTranDesc;
					}

					copy_tran.Qty = 0m;
					copy_tran.UOM = null;
					copy_tran.InventoryID = null;
					copy_tran.TranLineNbr = null;

					sender.Update(copy_tran);
				}
				return true;
			}
			return false;
		}

        private void UpdateHistory<TAcctHist>(GLTran tran, Account acct, string curyID, string baseCuryID, string PeriodID, bool FinFlag, bool REFlag)
            where TAcctHist : BaseGLHistory, new()
        {
            TAcctHist accthist = new TAcctHist();
            accthist.AccountID = acct.AccountID;
            accthist.FinPeriodID = PeriodID;
            accthist.LedgerID = tran.LedgerID;
            accthist.BranchID = tran.BranchID;
            accthist.SubID = tran.SubID;
            accthist.CuryID = curyID;
            if (baseCuryID != null)
            {
                (accthist as CuryAcctHist).BaseCuryID = baseCuryID;
            }
            accthist = (TAcctHist)Caches[typeof(TAcctHist)].Insert(accthist);
            if (accthist != null)
            {
                accthist.FinFlag = FinFlag;
				accthist.REFlag = REFlag;

                if (tran.CuryDebitAmt != 0m && tran.CuryCreditAmt != 0m || tran.DebitAmt != 0m && tran.CreditAmt != 0m)
                {
                    throw new PXException(Messages.TranAmountsDenormalized);
                }

                if (!REFlag)
                {
                    accthist.PtdDebit += (tran.DebitAmt != 0m && tran.CreditAmt == 0m) ? (tran.DebitAmt - tran.CreditAmt) : 0m;
                    accthist.PtdCredit += (tran.CreditAmt != 0m && tran.DebitAmt == 0m) ? (tran.CreditAmt - tran.DebitAmt) : 0m;

                    if (accthist.CuryID != null)
                    {
                        accthist.CuryPtdDebit += (tran.CuryDebitAmt != 0m && tran.CuryCreditAmt == 0m) ? (tran.CuryDebitAmt - tran.CuryCreditAmt) : 0m;
                        accthist.CuryPtdCredit += (tran.CuryCreditAmt != 0m && tran.CuryDebitAmt == 0m) ? (tran.CuryCreditAmt - tran.CuryDebitAmt) : 0m;
                    }
                    else
                    {
                        accthist.CuryPtdDebit = accthist.PtdDebit;
                        accthist.CuryPtdCredit = accthist.PtdCredit;
                    }
                }

                if (acct.Type == AccountType.Income || acct.Type == AccountType.Liability)
                {
                    accthist.YtdBalance += (tran.CreditAmt - tran.DebitAmt);
                    if (REFlag)
                    {
                        accthist.BegBalance += (tran.CreditAmt - tran.DebitAmt);
                    }
                    if (accthist.CuryID != null)
                    {
                        if (tran.Module == GL.BatchModule.CM && tran.TranType == "REV" && tran.TranClass == acct.Type)
                        {
                            accthist.PtdRevalued -= (tran.CreditAmt - tran.DebitAmt);
                        }
                        accthist.CuryYtdBalance += (tran.CuryCreditAmt - tran.CuryDebitAmt);
                        if (REFlag)
                        {
                            accthist.CuryBegBalance += (tran.CuryCreditAmt - tran.CuryDebitAmt);
                        }
                    }
                    else
                    {
                        accthist.CuryYtdBalance = accthist.YtdBalance;
                        if (REFlag)
                        {
                            accthist.CuryBegBalance = accthist.BegBalance;
                        }
                    }
                }
                else
                {
                    accthist.YtdBalance += (tran.DebitAmt - tran.CreditAmt);
                    if (REFlag)
                    {
                        accthist.BegBalance += (tran.DebitAmt - tran.CreditAmt);
                    }
                    if (accthist.CuryID != null)
                    {
                        if (tran.Module == GL.BatchModule.CM && tran.TranType == "REV" && tran.TranClass == acct.Type)
                        {
                            accthist.PtdRevalued -= (tran.DebitAmt - tran.CreditAmt);
                        }
                        accthist.CuryYtdBalance += (tran.CuryDebitAmt - tran.CuryCreditAmt);
                        if (REFlag)
                        {
                            accthist.CuryBegBalance += (tran.CuryDebitAmt - tran.CuryCreditAmt);
                        }
                    }
                    else
                    {
                        accthist.CuryYtdBalance = accthist.YtdBalance;
                        if (REFlag)
                        {
                            accthist.CuryBegBalance = accthist.BegBalance;
                        }
                    }
                }
            }
        }

        private void UpdateHistory(GLTran tran, Account acct, string PeriodID, bool FinFlag, bool REFlag, string curyID, string baseCuryID)
        {
            UpdateHistory<AcctHist>(tran, acct, acct.CuryID, null, PeriodID, FinFlag, REFlag);

            if (curyID != null)
            {
                UpdateHistory<CuryAcctHist>(tran, acct, curyID, baseCuryID, PeriodID, FinFlag, REFlag);
            }
        }

        private void UpdateAllocationBalance(Batch b)
        {
            foreach (PXResult<GLAllocationAccountHistory, Account> res in this.BatchAllocHistory.Select(b.BatchNbr, b.Module))
            {
                GLAllocationAccountHistory iAH = res;
                Account acct = res;
                if (Math.Round(Math.Abs(iAH.AllocatedAmount ?? 0.0m), 4) < 0.00005m) continue;
                AcctHist accthist = new AcctHist();
                accthist.AccountID = iAH.AccountID;
                accthist.FinPeriodID = b.TranPeriodID;
                accthist.LedgerID = b.LedgerID;
                accthist.SubID = iAH.SubID;
                accthist.CuryID = acct.CuryID;
                accthist.BranchID = iAH.BranchID;
                accthist = (AcctHist)Caches[typeof(AcctHist)].Insert(accthist);
                if (accthist != null)
                {
                    accthist.AllocPtdBalance = (accthist.AllocPtdBalance ?? 0.0m) + iAH.AllocatedAmount;
                    accthist.AllocBegBalance = (accthist.AllocBegBalance ?? 0.0m) + iAH.PriorPeriodsAllocAmount;
                }
            }
        }

        private bool UpdateConsolidationBalance(Batch b)
        {
            bool anychanges = false;
            if (b.BatchType == BatchTypeCode.Consolidation)
            {
                GLConsolBatch cb = PXSelect<GLConsolBatch,
                    Where<GLConsolBatch.batchNbr, Equal<Required<GLConsolBatch.batchNbr>>>>
                    .Select(this, b.BatchNbr);
                if (cb == null && b.AutoReverseCopy == true)
                {
                    cb = PXSelect<GLConsolBatch,
                        Where<GLConsolBatch.batchNbr, Equal<Required<GLConsolBatch.batchNbr>>>>
                        .Select(this, b.OrigBatchNbr);
                }
                if (cb != null)
                {
                    PXCache cache = Caches[typeof(ConsolHist)];
                    foreach (AcctHist hist in Caches[typeof(AcctHist)].Inserted)
                    {
                        if (hist.AccountID == glsetup.Current.YtdNetIncAccountID
                            || hist.AccountID == glsetup.Current.RetEarnAccountID
                            && hist.FinPeriodID != b.FinPeriodID)
                        {
                            continue;
                        }
                        ConsolHist ch = new ConsolHist();
                        ch.SetupID = cb.SetupID;
						ch.BranchID = hist.BranchID;
                        ch.LedgerID = hist.LedgerID;
                        ch.AccountID = hist.AccountID;
                        ch.SubID = hist.SubID;
                        ch.FinPeriodID = hist.FinPeriodID;
                        ch = (ConsolHist)cache.Insert(ch);
                        if (ch != null)
                        {
                            ch.PtdCredit += hist.FinPtdCredit;
                            ch.PtdDebit += hist.FinPtdDebit;
                            anychanges = true;
                        }
                    }
                }
            }
            return anychanges;
        }

        private void DecimalSwap(ref decimal? d1, ref decimal? d2)
        {
            decimal? swap = d1;
            d1 = d2;
            d2 = swap;
        }

        public virtual Batch ReverseBatchProc(Batch b)
        {
            Batch copy = PXCache<Batch>.CreateCopy(b);
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    copy.OrigBatchNbr = copy.BatchNbr;
                    copy.OrigModule = copy.Module;
                    copy.BatchNbr = null;
					try
					{
						copy.FinPeriodID = FinPeriodIDAttribute.NextPeriod(this, copy.FinPeriodID);
					}
					catch(PXFinPeriodException)
					{
						throw new PXFinPeriodException(Messages.NoOpenPeriodAfter, FinPeriodIDFormattingAttribute.FormatForError(copy.FinPeriodID));
					}
                    if (copy.FinPeriodID == null)
                    {
                        throw new PXException(Messages.NoOpenPeriod);
                    }
                    copy.DateEntered = FinPeriodIDAttribute.PeriodStartDate(this, copy.FinPeriodID);
                    copy.AutoReverse = false;
                    copy.AutoReverseCopy = true;
                    copy.CuryInfoID = null;

                    CurrencyInfo info = (CurrencyInfo)PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(this, b.CuryInfoID);

                    if (info != null)
                    {
                        CurrencyInfo infocopy = PXCache<CurrencyInfo>.CreateCopy(info);
                        infocopy.CuryInfoID = -1;
                        copy.CuryInfoID = -1;
                        infocopy = (CurrencyInfo)CurrencyInfo_ID.Cache.Insert(infocopy);
                    }

                    copy.Posted = false;
                    copy.Status = BatchStatus.Unposted;
                    copy = (Batch)Caches[typeof(Batch)].Insert(copy);

                    foreach (GLTran tran in GLTran_Module_BatNbr.Select(b.Module, b.BatchNbr))
                    {
                        GLTran trancopy = PXCache<GLTran>.CreateCopy(tran);
                        trancopy.OrigBatchNbr = trancopy.BatchNbr;
                        trancopy.OrigModule = trancopy.Module;
                        trancopy.BatchNbr = null;
                        trancopy.CuryInfoID = copy.CuryInfoID;
                        trancopy.CATranID = null;
                        trancopy.TranID = null;
                        trancopy.Posted = false;
						trancopy.TranDate = copy.DateEntered;

                        {
                            Decimal? amount = trancopy.CuryDebitAmt;
                            trancopy.CuryDebitAmt = trancopy.CuryCreditAmt;
                            trancopy.CuryCreditAmt = amount;
                        }

                        {
                            Decimal? amount = trancopy.DebitAmt;
                            trancopy.DebitAmt = trancopy.CreditAmt;
                            trancopy.CreditAmt = amount;
                        }

                        trancopy = (GLTran)Caches[typeof(GLTran)].Insert(trancopy);
                    }

                    Caches[typeof(Batch)].Persist(PXDBOperation.Insert);

                    foreach (GLTran tran in Caches[typeof(GLTran)].Inserted)
                    {
                        foreach (Batch batch in Caches[typeof(Batch)].Cached)
                        {
                            if (object.Equals(tran.OrigBatchNbr, batch.OrigBatchNbr))
                            {
                                tran.BatchNbr = batch.BatchNbr;
                                tran.CuryInfoID = batch.CuryInfoID;
                                break;
                            }
                        }

                        CATran catran = GLCashTranIDAttribute.DefaultValues(Caches[typeof(GLTran)], tran);
                        if (catran != null)
                        {
                            catran = (CATran)Caches[typeof(CATran)].Insert(catran);
                            Caches[typeof(CATran)].PersistInserted(catran);
                            long id = Convert.ToInt64(PXDatabase.SelectIdentity());

                            tran.CATranID = id;
                            catran.TranID = id;

                            Caches[typeof(CATran)].Normalize();
                        }
                    }

                    Caches[typeof(GLTran)].Persist(PXDBOperation.Insert);
					Caches[typeof(CADailySummary)].Persist(PXDBOperation.Insert);

                    ts.Complete(this);
                }
                Caches[typeof(Batch)].Persisted(false);
                Caches[typeof(GLTran)].Persisted(false);
                Caches[typeof(CATran)].Persisted(false);
				Caches[typeof(CADailySummary)].Persisted(false);

            }
            return copy;
        }

        private bool IsClosed(string PeriodID)
        {
            FinPeriod fp = ClosedCheck.Select(PeriodID);
            if (fp != null)
            {
                return (bool)fp.Closed;
            }
            else
            {
                return false;
            }
        }

        public virtual void IntegrityCheckProc(Ledger ledger)
        {
            this._IsIntegrityCheck = true;

            string minPeriod = "190001";

            GLHistory maxHist = (GLHistory)PXSelectGroupBy<GLHistory, Where<GLHistory.ledgerID, Equal<Required<GLHistory.ledgerID>>, And<GLHistory.detDeleted, Equal<boolTrue>>>, Aggregate<Max<GLHistory.finPeriodID>>>.Select(this, ledger.LedgerID);

            if (maxHist != null && maxHist.FinPeriodID != null)
            {
                minPeriod = maxHist.FinPeriodID;
            }

            //foreach (Batch batch in PXSelect<Batch, Where<Batch.posted, Equal<True>>>.Select(this))
            foreach (FinPeriod period in PXSelectReadonly<FinPeriod, Where<FinPeriod.finPeriodID, Greater<Required<FinPeriod.finPeriodID>>>>.Select(this, minPeriod))
            {
                GLTran_CATran_Module_BatNbr.View.WhereNew<Where<GLTran.finPeriodID, Equal<Required<GLTran.finPeriodID>>, And<GLTran.ledgerID, Equal<Required<GLTran.ledgerID>>, And<GLTran.posted, Equal<True>>>>>();
                UpdateHistoryProc(period.FinPeriodID, ledger.LedgerID);
            }

            using (new PXConnectionScope())
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    PXDatabase.Delete<GLHistory>(
                        new PXDataFieldRestrict("LedgerID", PXDbType.Int, 4, ledger.LedgerID, PXComp.EQ),
                        new PXDataFieldRestrict("FinPeriodID", PXDbType.Char, 6, minPeriod, PXComp.GT)
                        );

                    PXDatabase.Delete<CuryGLHistory>(
                        new PXDataFieldRestrict("LedgerID", PXDbType.Int, 4, ledger.LedgerID, PXComp.EQ),
                        new PXDataFieldRestrict("FinPeriodID", PXDbType.Char, 6, minPeriod, PXComp.GT)
                        );

                    Caches[typeof(AcctHist)].Persist(PXDBOperation.Insert);
                    Caches[typeof(CuryAcctHist)].Persist(PXDBOperation.Insert);

                    ts.Complete(this);
                }
                Caches[typeof(AcctHist)].Persisted(false);
                Caches[typeof(CuryAcctHist)].Persisted(false);
            }
        }

        public virtual void UpdateHistoryProc(params object[] pars)
        {
            GLTran_CATran_Module_BatNbr.View.Clear();
            foreach (PXResult<GLTran, CATran, CurrencyInfo, Account, Ledger> res in GLTran_CATran_Module_BatNbr.Select(pars))
            {
                string curyID = null;
                string baseCuryID = null;
                CurrencyInfo ci = res;
                if (ci != null)
                {
                    curyID = ci.CuryID;
                    baseCuryID = ci.BaseCuryID;
                }

                GLTran tran = res;
                if (!_IsIntegrityCheck)
                {
                    GLTran cached = (GLTran)GLTran_Module_BatNbr.Cache.Locate(tran);
                    tran = cached ?? tran;
                }
                CATran cashtran = (CATran)res;

				Account acct = Account_AccountID.Current = res;
				Ledger ledger = Ledger_LedgerID.Current = res;

                Account_AccountID.Cache.SetStatus(acct, PXEntryStatus.Held);

                if (acct.AccountID == null)
                {
                    throw new PXException(Messages.AccountMissing);
                }

                if (ledger.LedgerID == null)
                {
                    throw new PXException(Messages.LedgerMissing);
                }

                if (ci.CuryInfoID == null)
                {
                    throw new PXException(Messages.CuryInfoMissing);
                }

				if (reacct == null)
				{
					reacct = Account_AccountID.Select(glsetup.Current.RetEarnAccountID);
					if (reacct == null)
					{
						throw new PXException(Messages.InvalidRetEarnings);
					}
				}

                //Incomes are treated like Liabilities, Expenses like Assets in statistical ledgers
                if ((acct.Type == AccountType.Income || acct.Type == AccountType.Expense) && ledger.BalanceType != LedgerBalanceType.Statistical)
                {
                    if (object.Equals(tran.AccountID, glsetup.Current.YtdNetIncAccountID))
                    {
                        throw new PXException(Messages.NoPostNetIncome);
                    }

                    if (niacct == null)
                    {
                        niacct = Account_AccountID.Select(glsetup.Current.YtdNetIncAccountID);
                        if (niacct == null)
                        {
                            throw new PXException(Messages.InvalidNetIncome);
                        }
                    }

					GLTran retran = PXCache<GLTran>.CreateCopy(tran);
					retran.CuryDebitAmt = 0m;
					retran.CuryCreditAmt = 0m;
					retran.DebitAmt = 0m;
					retran.CreditAmt = 0m;

                    UpdateHistory(tran, acct, tran.FinPeriodID, true, false, curyID, baseCuryID);
					if (ledger.BalanceType == LedgerBalanceType.Actual || ledger.BalanceType == LedgerBalanceType.Report)
					{
						UpdateHistory(tran, niacct, tran.FinPeriodID, true, false, null, baseCuryID);
						UpdateHistory(retran, reacct, tran.FinPeriodID.Substring(0, 4) + "01", true, false, null, baseCuryID);

						FinPeriod next = NextFiscalYear.Select(tran.FinPeriodID);
                        if (next != null)
                        {
                            UpdateHistory(tran, reacct, next.FinPeriodID, true, true, null, baseCuryID);
                        }
                    }

                    UpdateHistory(tran, acct, tran.TranPeriodID, false, false, curyID, baseCuryID);
					if (ledger.BalanceType == LedgerBalanceType.Actual || ledger.BalanceType == LedgerBalanceType.Report)
					{
						UpdateHistory(tran, niacct, tran.TranPeriodID, false, false, null, baseCuryID);
						UpdateHistory(retran, reacct, tran.TranPeriodID.Substring(0, 4) + "01", false, false, null, baseCuryID);

						FinPeriod next = NextFiscalYear.Select(tran.TranPeriodID);
						if (next != null)
						{
							UpdateHistory(tran, reacct, next.FinPeriodID, false, true, null, baseCuryID);
						}
					}
                }
                else
                {
                    UpdateHistory(tran, acct, tran.FinPeriodID, true, false, curyID, baseCuryID);
                    UpdateHistory(tran, acct, tran.TranPeriodID, false, false, curyID, baseCuryID);

					if ((ledger.BalanceType == LedgerBalanceType.Actual || ledger.BalanceType == LedgerBalanceType.Report) && acct.AccountID == reacct.AccountID)
					{
						GLTran retran = PXCache<GLTran>.CreateCopy(tran);
						retran.CuryDebitAmt = 0m;
						retran.CuryCreditAmt = 0m;
						retran.DebitAmt = 0m;
						retran.CreditAmt = 0m;

						UpdateHistory(retran, reacct, tran.FinPeriodID.Substring(0, 4) + "01", true, false, null, baseCuryID);
					}
                }

                if (_IsIntegrityCheck == false)
                {
                    tran.Posted = true;
                    GLTran_Module_BatNbr.Cache.SetStatus(tran, PXEntryStatus.Updated);
                    if (cashtran.TranID != null)
                    {
                        cashtran = PXCache<CATran>.CreateCopy(cashtran);
                        cashtran.Released = true;
                        cashtran.Posted = true;
                        cashtran.BatchNbr = tran.BatchNbr;
                        catran.Update(cashtran);
                    }
                }
            }
        }

		public static bool GetAccountMapping(PXGraph graph, Batch batch, GLTran tran, out BranchAcctMapFrom mapfrom, out BranchAcctMapTo mapto)
		{
			mapfrom = null;
			mapto = null;

			if (batch.BranchID == tran.BranchID || tran.BranchID == null || tran.AccountID == null) return true;

			Ledger ledger = (Ledger)PXSelectorAttribute.Select<Batch.ledgerID>(graph.Caches[typeof(Batch)], batch, batch.LedgerID);

			if (ledger == null)
			{
				throw new PXException(Messages.LedgerMissing);
			}

			if (ledger.BalanceType != LedgerBalanceType.Actual) return true;

			Branch detailbranch = (Branch)PXSelectorAttribute.Select<GLTran.branchID>(graph.Caches[typeof(GLTran)], tran, tran.BranchID);

			if (detailbranch == null)
			{
				throw new PXException(Messages.BranchMissing);
			}

			if (detailbranch.LedgerID == batch.LedgerID && ledger.PostInterCompany == false) return true;
			if (!PXAccess.FeatureInstalled<FeaturesSet.interBranch>()) return false;

			Account account = (Account)PXSelectorAttribute.Select<GLTran.accountID>(graph.Caches[typeof(GLTran)], tran, tran.AccountID);

			if (account == null)
			{
				throw new PXException(Messages.AccountMissing);
			}

			mapfrom = PXSelectReadonly<BranchAcctMapFrom, Where<BranchAcctMapFrom.fromBranchID, Equal<Required<Batch.branchID>>, And<BranchAcctMapFrom.toBranchID, Equal<Required<GLTran.branchID>>, And<Required<Account.accountCD>, Between<BranchAcctMapFrom.fromAccountCD, BranchAcctMapFrom.toAccountCD>>>>>.Select(graph, batch.BranchID, tran.BranchID, account.AccountCD);
			if (mapfrom == null)
			{
				mapfrom = PXSelectReadonly<BranchAcctMapFrom, Where<BranchAcctMapFrom.fromBranchID, Equal<Required<Batch.branchID>>, And<BranchAcctMapFrom.toBranchID, IsNull, And<Required<Account.accountCD>, Between<BranchAcctMapFrom.fromAccountCD, BranchAcctMapFrom.toAccountCD>>>>>.Select(graph, batch.BranchID, account.AccountCD);

				if (mapfrom == null || mapfrom.MapSubID == null)
				{
					return false;
				}
			}

			mapto = PXSelectReadonly<BranchAcctMapTo, Where<BranchAcctMapTo.toBranchID, Equal<Required<Batch.branchID>>, And<BranchAcctMapTo.fromBranchID, Equal<Required<GLTran.branchID>>, And<Required<Account.accountCD>, Between<BranchAcctMapTo.fromAccountCD, BranchAcctMapTo.toAccountCD>>>>>.Select(graph, batch.BranchID, tran.BranchID, account.AccountCD);
			if (mapto == null)
			{
				mapto = PXSelectReadonly<BranchAcctMapTo, Where<BranchAcctMapTo.toBranchID, Equal<Required<Batch.branchID>>, And<BranchAcctMapTo.fromBranchID, IsNull, And<Required<Account.accountCD>, Between<BranchAcctMapTo.fromAccountCD, BranchAcctMapTo.toAccountCD>>>>>.Select(graph, batch.BranchID, account.AccountCD);

				if (mapto == null || mapto.MapSubID == null)
				{
					return false;
				}
			}
			return true;
		}

		protected virtual Batch CreateInterCompany(Batch b)
		{
			this.Caches[typeof(Batch)].Current = b;

			PXRowInserting inserting_handler = new PXRowInserting((sender, e) =>
			{
				GLTran tran = (GLTran)e.Row;
				object BranchID = tran.BranchID;
				object AccountID = tran.AccountID;
				object SubID = tran.SubID;
				object CuryInfoID = tran.CuryInfoID;

				if (tran.IsInterCompany == true)
				{
					e.Cancel = PostGraph.UpdateTran(sender, (GLTran)e.Row,
						PXSelect<GLTran,
							Where<GLTran.module, Equal<Current<Batch.module>>,
							And<GLTran.batchNbr, Equal<Current<Batch.batchNbr>>,
							And<GLTran.curyInfoID, Equal<Required<GLTran.curyInfoID>>,
							And<GLTran.branchID, Equal<Required<GLTran.branchID>>,
							And<GLTran.accountID, Equal<Required<GLTran.accountID>>,
							And<GLTran.subID, Equal<Required<GLTran.subID>>,
							And<GLTran.isInterCompany, Equal<True>>>>>>>>>.Select(this, CuryInfoID, BranchID, AccountID, SubID));
					if (e.Cancel == false)
					{
						PostGraph.NormalizeAmounts(tran);
					}
				}
				else
				{
					PostGraph.NormalizeAmounts(tran);
				}

				if (e.Cancel == false)
				{
					e.Cancel = (tran.CuryDebitAmt == 0 &&
								tran.CuryCreditAmt == 0 &&
								tran.DebitAmt == 0 &&
								tran.CreditAmt == 0 &&
								tran.ZeroPost != true);
				}
			});

			PXRowInserted inserted_handler = new PXRowInserted((sender, e) =>
			{
				b.CuryCreditTotal += ((GLTran)e.Row).CuryCreditAmt;
				b.CuryDebitTotal += ((GLTran)e.Row).CuryDebitAmt;
				b.CuryControlTotal += ((GLTran)e.Row).CuryDebitAmt;
				b.CreditTotal += ((GLTran)e.Row).CreditAmt;
				b.DebitTotal += ((GLTran)e.Row).DebitAmt;
				b.ControlTotal += ((GLTran)e.Row).DebitAmt;
			});
			PXRowUpdated updated_handler = new PXRowUpdated((sender, e) =>
			{
				b.CuryCreditTotal += ((GLTran)e.Row).CuryCreditAmt;
				b.CuryDebitTotal += ((GLTran)e.Row).CuryDebitAmt;
				b.CuryControlTotal += ((GLTran)e.Row).CuryDebitAmt;
				b.CreditTotal += ((GLTran)e.Row).CreditAmt;
				b.DebitTotal += ((GLTran)e.Row).DebitAmt;
				b.ControlTotal += ((GLTran)e.Row).DebitAmt;

				b.CuryCreditTotal -= ((GLTran)e.OldRow).CuryCreditAmt;
				b.CuryDebitTotal -= ((GLTran)e.OldRow).CuryDebitAmt;
				b.CuryControlTotal -= ((GLTran)e.OldRow).CuryDebitAmt;
				b.CreditTotal -= ((GLTran)e.OldRow).CreditAmt;
				b.DebitTotal -= ((GLTran)e.OldRow).DebitAmt;
				b.ControlTotal -= ((GLTran)e.OldRow).DebitAmt;
			});
            PXRowDeleted deleted_handler = new PXRowDeleted((sender, e) =>
            {
                b.CuryCreditTotal -= ((GLTran)e.Row).CuryCreditAmt;
                b.CuryDebitTotal -= ((GLTran)e.Row).CuryDebitAmt;
                b.CuryControlTotal -= ((GLTran)e.Row).CuryDebitAmt;
                b.CreditTotal -= ((GLTran)e.Row).CreditAmt;
                b.DebitTotal -= ((GLTran)e.Row).DebitAmt;
                b.ControlTotal -= ((GLTran)e.Row).DebitAmt;
            });


			this.RowInserting.AddHandler<GLTran>(inserting_handler);
			this.RowInserted.AddHandler<GLTran>(inserted_handler);
			this.RowUpdated.AddHandler<GLTran>(updated_handler);
            this.RowDeleted.AddHandler<GLTran>(deleted_handler);

			bool isex = false;
			foreach (PXResult<GLTran, Account, Branch, Ledger> res in PXSelectJoin<GLTran, LeftJoin<Account, On<Account.accountID, Equal<GLTran.accountID>>, LeftJoin<Branch, On<Branch.branchID, Equal<GLTran.branchID>>, LeftJoin<Ledger, On<Ledger.ledgerID, Equal<Optional<Batch.ledgerID>>>>>>, Where<GLTran.module, Equal<Optional<Batch.module>>, And<GLTran.batchNbr, Equal<Optional<Batch.batchNbr>>, And<GLTran.branchID, NotEqual<Optional<Batch.branchID>>, And<Ledger.balanceType, Equal<LedgerBalanceType.actual>, And<Where<Branch.ledgerID, NotEqual<Optional<Batch.ledgerID>>, Or<Ledger.postInterCompany, Equal<True>>>>>>>>>.SelectMultiBound(this, new object[] { b }))
			{
				GLTran tran = res;
				Account acct = res;
				Branch branchto = res;
				Ledger ledger = res;

				if (acct.AccountID == null)
				{
					throw new PXException(Messages.AccountMissing);
				}

				if (ledger.LedgerID == null)
				{
					throw new PXException(Messages.LedgerMissing);
				}

				if (branchto.BranchID == null)
				{
					throw new PXException(Messages.BranchMissing);
				}

				PXSelectorAttribute.StoreCached<GLTran.accountID>(this.Caches[typeof(GLTran)], tran, acct);
				PXSelectorAttribute.StoreCached<GLTran.branchID>(this.Caches[typeof(GLTran)], tran, branchto);

				BranchAcctMapFrom mapfrom;
				BranchAcctMapTo mapto;
				if (!GetAccountMapping(this, b, tran, out mapfrom, out mapto))
				{
					Branch branchfrom = PXSelect<Branch, Where<Branch.branchID, Equal<Optional<Batch.branchID>>>>.SelectSingleBound(this, new object[] { b });
					throw new PXException(Messages.BrachAcctMapMissing, (branchfrom != null ? branchfrom.BranchCD : "Undefined"), (branchto != null ? branchto.BranchCD : "Undefined"));
				}

				GLTran copy = PXCache<GLTran>.CreateCopy(tran);
				copy.AccountID = mapfrom.MapAccountID;
				copy.SubID = mapfrom.MapSubID;
				copy.BranchID = b.BranchID;
				copy.LedgerID = b.LedgerID;
				copy.TranLineNbr = null;
				copy.LineNbr = null;
				copy.TranID = null;
				copy.CATranID = null;
				copy.ProjectID = null;
				copy.TaskID = null;
				copy.RefNbr = null;
				copy.IsInterCompany = true;
                copy.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.InterCoTranDesc, Caches[typeof(Batch)].GetValueExt<Batch.branchID>(b));

				Caches[typeof(GLTran)].Insert(copy);

				copy = PXCache<GLTran>.CreateCopy(tran);
				copy.AccountID = mapto.MapAccountID;
				copy.SubID = mapto.MapSubID;
				copy.LedgerID = branchto.LedgerID;
				copy.TranLineNbr = null;
				copy.LineNbr = null;
				copy.TranID = null;
				copy.CATranID = null;
				copy.ProjectID = null;
				copy.TaskID = null;
				copy.RefNbr = null;
				copy.IsInterCompany = true;
                copy.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.InterCoTranDesc, branchto.BranchCD);

				decimal? amt = copy.CuryCreditAmt;
				copy.CuryCreditAmt = copy.CuryDebitAmt;
				copy.CuryDebitAmt = amt;

				amt = copy.CreditAmt;
				copy.CreditAmt = copy.DebitAmt;
				copy.DebitAmt = amt;

				Caches[typeof(GLTran)].Insert(copy);

				copy = PXCache<GLTran>.CreateCopy(tran);
				copy.LedgerID = branchto.LedgerID;

				Caches[typeof(GLTran)].Update(copy);

				isex = true;
			}

			this.RowInserting.RemoveHandler<GLTran>(inserting_handler);
			this.RowInserted.RemoveHandler<GLTran>(inserted_handler);
			this.RowUpdated.RemoveHandler<GLTran>(updated_handler);
            this.RowDeleted.RemoveHandler<GLTran>(deleted_handler);

			Caches[typeof(GLTran)].Persist(PXDBOperation.Insert);
			Caches[typeof(GLTran)].Persist(PXDBOperation.Update);

			this.SelectTimeStamp();

			Caches[typeof(GLTran)].Persisted(false);

			return (isex ? b : null);
		}


        public virtual void PostBatchProc(Batch b)
        {
            if (RunningFlagScope<GLHistoryValidate>.IsRunning)
                throw new PXSetPropertyException(Messages.GLHistoryValidationRunning, PXErrorLevel.Warning);

            using (new RunningFlagScope<PostGraph>())
            {
                PostBatchProc(b, true);
            }
        }

        public virtual void PostBatchProc(Batch b, bool createintercompany)
        {
            if (b.Status != BatchStatus.Unposted || b.Released == false)
            {
                throw new PXException(Messages.BatchStatusInvalid);
            }

            if (glsetup.Current.AutoRevOption == "P")
            {
                PostGraph pg = PXGraph.CreateInstance<PostGraph>();

                foreach (PXResult<Batch, BatchCopy> res in PXSelectJoin<Batch, LeftJoin<BatchCopy, On<BatchCopy.origModule, Equal<Batch.module>, And<BatchCopy.origBatchNbr, Equal<Batch.batchNbr>, And<BatchCopy.autoReverseCopy, Equal<boolTrue>>>>>, Where<Batch.module, Equal<Required<Batch.module>>, And<Batch.batchNbr, Equal<Required<Batch.batchNbr>>, And<Batch.autoReverse, Equal<boolTrue>, And<Batch.released, Equal<boolTrue>, And<BatchCopy.origBatchNbr, IsNull>>>>>>.Select(pg, b.Module, b.BatchNbr))
                {
                    pg.Clear();

                    Batch copy = pg.ReverseBatchProc((Batch)res);
                    pg.ReleaseBatchProc(copy);

                    if (glsetup.Current.AutoPostOption == true)
                    {
                        pg.PostBatchProc(copy);
                    }
                }
            }

            if (createintercompany)
            {
                PostGraph pg = PXGraph.CreateInstance<PostGraph>();
                pg.Clear();
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    if (pg.CreateInterCompany(b) != null)
                    {
                        pg.PostBatchProc(b, false);
                        ts.Complete();
                        return;
                    }
                }
            }

            this.UpdateHistoryProc(b.Module, b.BatchNbr);
            this.UpdateAllocationBalance(b);
            bool isconsol = this.UpdateConsolidationBalance(b);

            this.UpdateProjectBalance(b);

            b.Posted = true;
            b.Status = BatchStatus.Posted;
            BatchModule.Update(b);

            using (PXTransactionScope ts = new PXTransactionScope())
            {
                BatchModule.Cache.Persist(PXDBOperation.Update);
                    GLTran_Module_BatNbr.Cache.Persist(PXDBOperation.Update);

                catran.Cache.Persist(PXDBOperation.Update);
                Caches[typeof(AcctHist)].Persist(PXDBOperation.Insert);
                Caches[typeof(CuryAcctHist)].Persist(PXDBOperation.Insert);
                Caches[typeof(PMHistory)].Persist(PXDBOperation.Insert);
				Caches[typeof(CADailySummary)].Persist(PXDBOperation.Insert);


                if (isconsol)
                {
                    Caches[typeof(ConsolHist)].Persist(PXDBOperation.Insert);
                }

                ts.Complete(this);
            }
            BatchModule.Cache.Persisted(false);
            GLTran_Module_BatNbr.Cache.Persisted(false);
            catran.Cache.Persisted(false);
            Caches[typeof(AcctHist)].Persisted(false);
            Caches[typeof(CuryAcctHist)].Persisted(false);
			Caches[typeof(CADailySummary)].Persisted(false);

            if (isconsol)
            {
                Caches[typeof(ConsolHist)].Persisted(false);
            }
            BatchModule.Cache.RestoreCopy(b, BatchModule.Current);
        }

        public virtual void ReleaseBatchProc(Batch b)
        {
            Dictionary<string, PMTask> tasksToAutoAllocate = new Dictionary<string, PMTask>();
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    if (string.IsNullOrEmpty(b.OrigBatchNbr) && (b.Status != BatchStatus.Balanced || (bool)b.Released))
                    {
                        throw new PXException(Messages.BatchStatusInvalid);
                    }

                    b.CreditTotal = 0.0m;
                    b.DebitTotal = 0.0m;
                    b.CuryCreditTotal = 0.0m;
                    b.CuryDebitTotal = 0.0m;

                    CurrencyInfo info = null;
                    /*
                    foreach (TranslationHistory hist in PXSelect<TranslationHistory, Where<TranslationHistory.batchNbr, Equal<Required<Batch.origBatchNbr>>, And<Required<Batch.origModule>, Equal<GL.BatchModule.moduleCM>>>>.Select(this, b.OrigBatchNbr, b.OrigModule))
                    {
                        if (string.IsNullOrEmpty(hist.ReversedBatchNbr))
                        {
                            hist.ReversedBatchNbr = b.BatchNbr;
                            hist.Status = TranslationStatus.Voided;
                            Caches[typeof(TranslationHistory)].SetStatus(hist, PXEntryStatus.Updated);
                        }
                        else
                        {
                            throw new PXException(Messages.TranslationAlreadyReversed, hist.ReferenceNbr);
                        }
                    }
                    */

                    Ledger ledger = null;                    
                    bool createTaxTrans = (PXAccess.FeatureInstalled<FeaturesSet.taxEntryFromGL>() && b.CreateTaxTrans == true);
                    TaxTranCreationProcessor taxCreationProc = null;
                    if (createTaxTrans)
                        taxCreationProc = new TaxTranCreationProcessor(this.GLTran_CATran_Module_BatNbr.Cache, this.GL_GLTran_Taxes.Cache, (b.SkipTaxValidation??false));
                    foreach (PXResult<GLTran, CATran, CurrencyInfo, Account, Ledger> res in GLTran_CATran_Module_BatNbr.Select(b.Module, b.BatchNbr))
                    {
                        GLTran tran = res;
                        CATran cashtran = res;
						Account acct = res;
                        
                        ledger = res;
                        info = res;

                        if (acct.AccountID == null)
                        {
                            throw new PXException(Messages.AccountMissing);
                        }

                        if (ledger.LedgerID == null)
                        {
                            throw new PXException(Messages.LedgerMissing);
                        }

						PXSelectorAttribute.StoreCached<GLTran.accountID>(this.Caches[typeof(GLTran)], tran, acct);

						BranchAcctMapFrom mapfrom;
						BranchAcctMapTo mapto;
						if (!PostGraph.GetAccountMapping(this, b, tran, out mapfrom, out mapto))
						{
							Branch branchfrom = (Branch)PXSelectorAttribute.Select<Batch.branchID>(BatchModule.Cache, b, b.BranchID);
							Branch branchto = (Branch)PXSelectorAttribute.Select<GLTran.branchID>(GLTran_CATran_Module_BatNbr.Cache, tran, tran.BranchID);

							throw new PXException(Messages.BrachAcctMapMissing, (branchfrom != null ? branchfrom.BranchCD : "Undefined"), (branchto != null ? branchto.BranchCD : "Undefined"));
						}

                        if (tran.CuryDebitAmt != 0m && tran.CuryCreditAmt != 0m || tran.DebitAmt != 0m && tran.CreditAmt != 0m)
                        {
                            throw new PXException(Messages.TranAmountsDenormalized);
                        }

                        if (createTaxTrans) 
                        {                            
                            taxCreationProc.AddToDocuments(res);                            
                        }                        


                        if (b.AutoReverseCopy == true && this.AutoRevEntry)
                        {
                            if (tran.CuryDebitAmt != 0m && tran.CuryCreditAmt == 0m || tran.DebitAmt != 0m && tran.CreditAmt == 0m)
                            {
                                tran.CuryCreditAmt = -1m * tran.CuryDebitAmt;
                                tran.CreditAmt = -1m * tran.DebitAmt;
                                tran.CuryDebitAmt = 0m;
                                tran.DebitAmt = 0m;
                            }
                            else if (tran.CuryCreditAmt != 0m && tran.CuryDebitAmt == 0m || tran.CreditAmt != 0m && tran.DebitAmt == 0m)
                            {
                                tran.CuryDebitAmt = -1m * tran.CuryCreditAmt;
                                tran.DebitAmt = -1m * tran.CreditAmt;
                                tran.CuryCreditAmt = 0m;
                                tran.CreditAmt = 0m;
                            }
                        }

                        b.CreditTotal += tran.CreditAmt;
                        b.DebitTotal += tran.DebitAmt;
                        b.CuryCreditTotal += tran.CuryCreditAmt;
                        b.CuryDebitTotal += tran.CuryDebitAmt;
                        b.CuryControlTotal = b.CuryDebitTotal;
                        b.ControlTotal = b.DebitTotal;

                        tran.Released = true;
                        GLTran_Module_BatNbr.Cache.Update(tran);

                        if (cashtran.TranID != null)
                        {
                            cashtran = PXCache<CATran>.CreateCopy(cashtran);
                            cashtran.Released = true;
                            cashtran.BatchNbr = tran.BatchNbr;
                            cashtran.Hold = b.Hold;
                            catran.Update(cashtran);
                        }
                    }

					if (Math.Abs(Math.Round((decimal)(b.CuryDebitTotal - b.CuryCreditTotal), 4)) >= 0.00005m && ledger.BalanceType != LedgerBalanceType.Statistical && b.BatchType != BatchTypeCode.TrialBalance)
                    {
                        throw new PXException(Messages.BatchOutOfBalance);
                    }

                    GLTran newtran = null;

                    if (Math.Abs(Math.Round((decimal)(b.DebitTotal - b.CreditTotal), 4)) >= 0.00005m && ledger.BalanceType != LedgerBalanceType.Statistical)
                    {
						BatchModule.Current = b;

                        GLTran tran = new GLTran();
                        Currency c = PXSelect<Currency, Where<Currency.curyID, Equal<Required<CurrencyInfo.curyID>>>>.Select(this, info.CuryID);

                        if (Math.Sign((decimal)(b.DebitTotal - b.CreditTotal)) == 1)
                        {
                            tran.AccountID = c.RoundingGainAcctID;
                            tran.SubID = c.RoundingGainSubID;
                            tran.CreditAmt = Math.Round((decimal)(b.DebitTotal - b.CreditTotal), 4);
                            tran.DebitAmt = 0;

							b.ControlTotal = b.DebitTotal;
							b.CreditTotal = b.DebitTotal;
                        }
                        else
                        {
                            tran.AccountID = c.RoundingLossAcctID;
                            tran.SubID = c.RoundingLossSubID;
                            tran.CreditAmt = 0;
                            tran.DebitAmt = Math.Round((decimal)(b.CreditTotal - b.DebitTotal), 4);

							b.ControlTotal = b.CreditTotal;
							b.DebitTotal = b.CreditTotal;
                        }
						tran.BranchID = b.BranchID;
                        tran.CuryCreditAmt = 0;
                        tran.CuryDebitAmt = 0;
						tran.TranDesc = PXMessages.LocalizeNoPrefix(Messages.RoundingDiff);
                        tran.LedgerID = b.LedgerID;
                        tran.FinPeriodID = b.FinPeriodID;
                        tran.TranDate = b.DateEntered;
                        tran.Released = true;

                        CurrencyInfo infocopy = new CurrencyInfo();
                        infocopy.CuryInfoID = -1;
                        tran.CuryInfoID = -1;
                        infocopy = (CurrencyInfo)CurrencyInfo_ID.Cache.Insert(infocopy);

                        tran = (GLTran)GLTran_Module_BatNbr.Cache.Insert(tran);

                        newtran = tran;
                    }

                    b.Released = true;
                    b.Status = BatchStatus.Unposted;
                    BatchModule.Update(b);

                    BatchModule.Cache.Persist(PXDBOperation.Update);

                    #region Project Management

                    if (b.Module == GL.BatchModule.GL)
                    {
                        PXSelectBase<GLTran> select = new PXSelectJoin<GLTran,
                        InnerJoin<Account, On<GLTran.accountID, Equal<Account.accountID>>,
                        InnerJoin<PMAccountGroup, On<PMAccountGroup.groupID, Equal<Account.accountGroupID>>,
                        InnerJoin<PMProject, On<PMProject.contractID, Equal<GLTran.projectID>, And<PMProject.nonProject, Equal<False>>>,
						InnerJoin<PMTask, On<PMTask.projectID, Equal<GLTran.projectID>, And<PMTask.taskID, Equal<GLTran.taskID>>>>>>>,
                        Where<GLTran.module, Equal<BatchModule.moduleGL>,
                        And<GLTran.batchNbr, Equal<Required<GLTran.batchNbr>>,
                        And<Account.accountGroupID, IsNotNull>>>>(this);

                        PXResultset<GLTran> resultset = select.Select(b.BatchNbr);

                        if (resultset.Count > 0)
                        {
                            PMRegister doc = new PMRegister();
                            doc.Module = b.Module;
                            doc.Date = b.DateEntered;
                            doc.Description = b.Description;
                            doc.Released = true;
                            ProjectDocs.Insert(doc);
							ProjectDocs.Cache.Persist(PXDBOperation.Insert);
                        }

						List<PMTran> sourceForAllocation = new List<PMTran>();

                        foreach (PXResult<GLTran, Account, PMAccountGroup, PMProject, PMTask> res in resultset)
                        {
                            GLTran tran = (GLTran)res;
                            Account acc = (Account)res;
                            PMAccountGroup ag = (PMAccountGroup)res;
							PMProject project = (PMProject)res;
							PMTask task = (PMTask)res;

							PMTran pmt = (PMTran)ProjectTrans.Cache.Insert();
							pmt.BranchID = tran.BranchID;
                            pmt.AccountGroupID = acc.AccountGroupID;
                            pmt.AccountID = tran.AccountID;
                            pmt.SubID = tran.SubID;
                            pmt.BAccountID = tran.ReferenceID;
                            pmt.BatchNbr = tran.BatchNbr;
                            pmt.Date = tran.TranDate;
                            pmt.Description = tran.TranDesc;
                            pmt.FinPeriodID = tran.FinPeriodID;
                            pmt.TranPeriodID = tran.TranPeriodID;
                            pmt.InventoryID = tran.InventoryID ?? PM.PMInventorySelectorAttribute.EmptyInventoryID;
                            pmt.OrigLineNbr = tran.LineNbr;
                            pmt.OrigModule = tran.Module;
                            pmt.OrigRefNbr = tran.RefNbr;
                            pmt.OrigTranType = tran.TranType;
                            pmt.ProjectID = tran.ProjectID;
                            pmt.TaskID = tran.TaskID;
                            pmt.Billable = tran.NonBillable != true;
							pmt.UseBillableQty = true;
                            pmt.UOM = tran.UOM;

							pmt.Amount = tran.DebitAmt - tran.CreditAmt;
                        pmt.Qty = tran.Qty;// pmt.Amount >= 0 ? tran.Qty : (tran.Qty * -1);
							int sign = 1;
							if (acc.Type == AccountType.Income || acc.Type == AccountType.Liability)
							{
								sign = -1;
							}

                            if (acc.Type != ag.Type)
                            {
                                pmt.Amount = -pmt.Amount;
                                pmt.Qty = -pmt.Qty;
                            }
                            pmt.BillableQty = tran.Qty;
                            pmt.Released = true;

                            string note = PXNoteAttribute.GetNote(GLTran_Module_BatNbr.Cache, tran);
							if (note != null)
								PXNoteAttribute.SetNote(ProjectTrans.Cache, pmt, note);
							Guid[] files = PXNoteAttribute.GetFileNotes(GLTran_Module_BatNbr.Cache, tran);
							if (files != null && files.Length > 0)
								PXNoteAttribute.SetFileNotes(ProjectTrans.Cache, pmt, files);

							ProjectTrans.Update(pmt);
							ProjectTrans.Cache.Persist(pmt, PXDBOperation.Insert);

                            tran.PMTranID = pmt.TranID;
                            this.Caches[typeof(GLTran)].Update(tran);

							if (pmt.TaskID != null && (pmt.Qty != 0 || pmt.Amount != 0)) //TaskID will be null for Contract
							{
								#region Project Status Update
								//Note: This is a special case of RegisterReleaseProcess.UpdateProjectBalance()
								//Note: GLTran.InventoryID will always be null here and thus can be simplified:  
								PMProjectStatus otherStatus = PXSelect<PMProjectStatus,
									Where<PMProjectStatus.accountGroupID, Equal<Required<PMProjectStatus.accountGroupID>>,
									And<PMProjectStatus.projectID, Equal<Required<PMProjectStatus.projectID>>,
									And<PMProjectStatus.projectTaskID, Equal<Required<PMProjectStatus.projectTaskID>>,
									And<PMProjectStatus.inventoryID, Equal<Required<PMProjectStatus.inventoryID>>>>>>>.Select(this, pmt.AccountGroupID, pmt.ProjectID, pmt.TaskID, PMProjectStatus.EmptyInventoryID);//do not restrict by FinPeriod

								decimal rollupQty = 0;
								string UOM = null;
								if (otherStatus != null && !string.IsNullOrEmpty(otherStatus.UOM))
								{
									UOM = otherStatus.UOM;

									if (otherStatus.UOM == pmt.UOM)
									{
										rollupQty = pmt.Qty.GetValueOrDefault();
									}
									else
									{
										//convert if possible:
										PX.Objects.IN.INUnitAttribute.TryConvertGlobalUnits(this, pmt.UOM, UOM, pmt.Qty.GetValueOrDefault(), PX.Objects.IN.INPrecision.QUANTITY, out rollupQty);
									}
								}

								decimal amount = pmt.Amount.GetValueOrDefault() * sign;

								PMProjectStatusAccum ps = new PMProjectStatusAccum();
								ps.PeriodID = pmt.FinPeriodID;
								ps.ProjectID = pmt.ProjectID;
								ps.ProjectTaskID = pmt.TaskID;
								ps.AccountGroupID = pmt.AccountGroupID;
								ps.InventoryID = PMProjectStatus.EmptyInventoryID;
								ps.UOM = UOM;

								ps = (PMProjectStatusAccum)this.Caches[typeof(PMProjectStatusAccum)].Insert(ps);

								ps.ActualQty += rollupQty;
								ps.ActualAmount += amount;
								this.Views.Caches.Add(typeof(PMProjectStatusAccum));

								#endregion

								#region PMTask Totals Update

								PMTaskTotal ta = new PMTaskTotal();
								ta.ProjectID = pmt.ProjectID;
								ta.TaskID = pmt.TaskID;

								ta = (PMTaskTotal)this.Caches[typeof(PMTaskTotal)].Insert(ta);

								switch (acc.Type)
								{
									case AccountType.Asset:
										ta.Asset += amount;
										break;
									case AccountType.Liability:
										ta.Liability += amount;
										break;
									case AccountType.Income:
										ta.Income += amount;
										break;
									case AccountType.Expense:
										ta.Expense += amount;
										break;
								}
								this.Views.Caches.Add(typeof(PMTaskTotal));

								#endregion

								sourceForAllocation.Add(pmt);
								if (pmt.Allocated != true && project.AutoAllocate == true)
								{
									if (!tasksToAutoAllocate.ContainsKey(string.Format("{0}.{1}", task.ProjectID, task.TaskID)))
									{
										tasksToAutoAllocate.Add(string.Format("{0}.{1}", task.ProjectID, task.TaskID), task);
									}
								}
							}
                        }

						
						Caches[typeof(PMProjectStatusAccum)].Persist(PXDBOperation.Insert);
						Caches[typeof(PMTaskTotal)].Persist(PXDBOperation.Insert);
                    }
                    #endregion

                    #region TaxEntryFromGL
                    bool taxTransCreated = false;
                    if (createTaxTrans && ledger.BalanceType == LedgerBalanceType.Actual) 
                    {
                        taxTransCreated = taxCreationProc.CreateTaxTransactions();                        
                    }

                    #endregion

                    //Caches[typeof(TranslationHistory)].Persist(PXDBOperation.Update);

                    if (newtran != null)
                    {
                        CurrencyInfo_ID.Cache.Persist(PXDBOperation.Insert);
                        newtran.CuryInfoID = Convert.ToInt64(PXDatabase.SelectIdentity());
                        GLTran_Module_BatNbr.Cache.Persist(PXDBOperation.Insert);
                    }

                        GLTran_Module_BatNbr.Cache.Persist(PXDBOperation.Update);

                    catran.Cache.Persist(PXDBOperation.Update);
                    //Caches[typeof(GLTran)].Persist(PXDBOperation.Update);
					Caches[typeof(CADailySummary)].Persist(PXDBOperation.Insert);
                    if (taxTransCreated)
                        this.GL_GLTran_Taxes.Cache.Persist(PXDBOperation.Insert);
                    ts.Complete(this);
                }
                BatchModule.Cache.Persisted(false);
                //Caches[typeof(TranslationHistory)].Persisted(false);
                CurrencyInfo_ID.Cache.Persisted(false);
                GLTran_Module_BatNbr.Cache.Persisted(false);
                catran.Cache.Persisted(false);
				Caches[typeof(CADailySummary)].Persisted(false);
                this.GL_GLTran_Taxes.Cache.Persisted(false);

            BatchModule.Cache.RestoreCopy(b, BatchModule.Current);

			if (tasksToAutoAllocate.Count > 0)
			{
				try
				{
					AutoAllocateTasks(new List<PMTask>(tasksToAutoAllocate.Values));
				}
				catch (Exception ex)
				{
					throw new PXException(ex, PM.Messages.AutoAllocationFailed);
				}
				
			}
        }

		public class TaxTranCreationProcessor
		{
			public const string TranForward = "TFW";
			public const string TranReversed = "TRV";

			protected PXCache GLTranCache = null;
			protected PXCache TaxTranCache = null;
			protected PXGraph graph = null;
			protected Dictionary<string, List<PXResult<GLTran, CATran, CurrencyInfo, Account, Ledger>>> Documents;
            protected bool skipTaxValidation = false;

			public TaxTranCreationProcessor(PXCache aGLTranCache, PXCache aTaxTranCache, bool aSkipTaxValidation)
			{
				this.GLTranCache = aGLTranCache;
				this.TaxTranCache = aTaxTranCache;
				this.graph = this.GLTranCache.Graph;
                this.skipTaxValidation = aSkipTaxValidation;
				this.Documents = new Dictionary<string, List<PXResult<GLTran, CATran, CurrencyInfo, Account, Ledger>>>();
			}
			public void AddToDocuments(PXResult<GLTran, CATran, CurrencyInfo, Account, Ledger> aTran)
			{
				GLTran tran = aTran;
				if (!String.IsNullOrEmpty(tran.RefNbr))
				{
					List<PXResult<GLTran, CATran, CurrencyInfo, Account, Ledger>> lines;
					if (!this.Documents.TryGetValue(tran.RefNbr, out lines))
					{
						lines = new List<PXResult<GLTran, CATran, CurrencyInfo, Account, Ledger>>(1);
						this.Documents.Add(tran.RefNbr, lines);
					}
					lines.Add(aTran);
				}
			}
			public virtual bool CreateTaxTransactions()
			{
				bool taxTransCreated = false;
				if (this.Documents.Count > 0)
				{
					//bool skipTaxValidation = ((Batch) graph.Caches[typeof (Batch)].Current).SkipTaxValidation == true;
					foreach (KeyValuePair<string, List<PXResult<GLTran, CATran, CurrencyInfo, Account, Ledger>>> iDoc in this.Documents)
					{
						List<PXResult<GLTran, CATran, CurrencyInfo, Account, Ledger>> iLines = iDoc.Value;
						string refNumber = iDoc.Key;
						Decimal curyCreditTotal = Decimal.Zero;
						Decimal curyDebitTotal = Decimal.Zero;
						List<GLTran> assetTrans = new List<GLTran>();
						List<GLTran> liabilityTrans = new List<GLTran>();
						List<GLTran> purchaseTaxTrans = new List<GLTran>();                        
						List<GLTran> salesTaxTrans = new List<GLTran>();
						List<GLTran> incomeTrans = new List<GLTran>();
						List<GLTran> expenseTrans = new List<GLTran>();
						CurrencyInfo docInfo = null;
						Dictionary<string, Tax> taxes = new Dictionary<string, Tax>();
						Dictionary<string, List<GLTran>> taxCategories = new Dictionary<string, List<GLTran>>();
						Dictionary<string, HashSet<GLTran>> taxGroups = new Dictionary<string, HashSet<GLTran>>();
						foreach (PXResult<GLTran, CATran, CurrencyInfo, Account, Ledger> iLn in iLines)
						{
							GLTran trn = iLn;
							Account acct = iLn;
							if (docInfo == null) docInfo = iLn;
							curyCreditTotal += trn.CuryCreditAmt.Value;
							curyDebitTotal += trn.CuryDebitAmt.Value;

							if (String.IsNullOrEmpty(trn.TaxID) == false)
							{
								Tax tax;
								if (!taxes.TryGetValue(trn.TaxID, out tax))
								{
									tax = PXSelect<TX.Tax, Where<TX.Tax.taxID, Equal<Required<TX.Tax.taxID>>>>.Select(this.graph, trn.TaxID);
									if (tax == null)
										throw new PXException(Messages.UnrecognizedTaxFoundUsedInDocument, trn.TaxID, refNumber);
									taxes.Add(tax.TaxID, tax);
								}

								if (tax.PurchTaxAcctID == tax.SalesTaxAcctID && tax.PurchTaxSubID == tax.SalesTaxSubID)
								{
									throw new PXException(TX.Messages.ClaimableAndPayableAccountsAreTheSame, trn.TaxID);
								}

								bool isPurchaseTaxTran = (tax.PurchTaxAcctID == trn.AccountID && tax.PurchTaxSubID == trn.SubID && (tax.ReverseTax !=true));
                                bool isSalesTaxTran = (tax.SalesTaxAcctID == trn.AccountID && tax.SalesTaxSubID == trn.SubID && (tax.ReverseTax != true) );
                                bool isReversedPurchaseTaxTran = (tax.PurchTaxAcctID == trn.AccountID && tax.PurchTaxSubID == trn.SubID && (tax.ReverseTax == true));
                                bool isReversedSalesTaxTran = (tax.SalesTaxAcctID == trn.AccountID && tax.SalesTaxSubID == trn.SubID && (tax.ReverseTax == true));
                                if (isSalesTaxTran || isReversedPurchaseTaxTran)
                                {
                                    salesTaxTrans.Add(trn);
                                }
                                else if (isPurchaseTaxTran || isReversedSalesTaxTran)
                                {
                                    purchaseTaxTrans.Add(trn);
                                }								
							}
							else
							{
                                if (!String.IsNullOrEmpty(trn.TaxCategoryID))
                                {
                                    List<GLTran> catList;
                                    if (!taxCategories.TryGetValue(trn.TaxCategoryID, out catList))
                                    {
                                        catList = new List<GLTran>();
                                        taxCategories.Add(trn.TaxCategoryID, catList);
                                    }
                                    catList.Add(trn);

                                    if (acct.Type == AccountType.Asset)
                                        assetTrans.Add(trn);
                                    if (acct.Type == AccountType.Liability)
                                        liabilityTrans.Add(trn);
                                    if (acct.Type == AccountType.Expense)
                                        expenseTrans.Add(trn);
                                    if (acct.Type == AccountType.Income)
                                        incomeTrans.Add(trn);
                                }
							}
						}

						if (salesTaxTrans.Count > 0)
						{
							TaxTranCreationHelper.SegregateTaxGroup(GLTranCache, salesTaxTrans, taxes, taxCategories, taxGroups);
						}
						if (purchaseTaxTrans.Count > 0)
						{
							TaxTranCreationHelper.SegregateTaxGroup(GLTranCache, purchaseTaxTrans, taxes, taxCategories, taxGroups);
						}                        

						bool hasTaxes = (salesTaxTrans.Count > 0 || purchaseTaxTrans.Count > 0); //There is no need to analize doc's withount taxes.
						if (hasTaxes)
						{
                            if (curyCreditTotal != curyDebitTotal)
                            {
                                throw new PXException(Messages.DocumentWithTaxIsNotBalanced, refNumber);
                            }

                            if (salesTaxTrans.Count > 0 && purchaseTaxTrans.Count > 0)
                            {
                                throw new PXException(Messages.DocumentWithTaxContainsBothSalesAndPurchaseTransactions, refNumber);
                            }

                            if (salesTaxTrans.Count > 0 && incomeTrans.Count == 0 && assetTrans.Count == 0)
                            {
                                throw new PXException(Messages.DocumentContainsSalesTaxTransactionsButNoIncomeAccounts, refNumber);
                            }

                            if (purchaseTaxTrans.Count > 0 && expenseTrans.Count == 0 && assetTrans.Count == 0)
                            {
                                throw new PXException(Messages.DocumentContainsPurchaseTaxTransactionsButNoExpenseAccounts, refNumber);
                            }

                            List<GLTran> taxTrans = salesTaxTrans.Count > 0 ? salesTaxTrans: purchaseTaxTrans;
                            string groupTaxType = salesTaxTrans.Count > 0 ? TaxType.Sales : TaxType.Purchase;
							Dictionary<string, GLTran> taxTransSummary = new Dictionary<string, GLTran>();
							PXCache glTranCache = this.GLTranCache;

							foreach (GLTran iTaxTran in taxTrans)
							{
								GLTran summary;
								if (!taxTransSummary.TryGetValue(iTaxTran.TaxID, out summary))
								{
									summary = (GLTran)glTranCache.CreateCopy(iTaxTran);
									taxTransSummary.Add(summary.TaxID, summary);
								}
								else
									TaxTranCreationHelper.Aggregate(summary, iTaxTran);
							}

							foreach (GLTran iTaxTran in taxTransSummary.Values)
							{
								List<GLTran> taxableTrans = new List<GLTran>();
								HashSet<GLTran> taxgroup;
								string taxID = iTaxTran.TaxID;
								TaxTran taxTran = new TaxTran();
								taxTran.TaxID = taxID;
								Tax tax = taxes[taxID];
                                string taxType = groupTaxType;
                                if (tax.ReverseTax == true) 
                                {
                                    taxType = (groupTaxType == TaxType.Sales) ? TaxType.Purchase : TaxType.Sales;
                                } 
								TaxTranCreationHelper.Copy(taxTran, iTaxTran, tax);

								if (taxGroups.TryGetValue(taxID, out taxgroup))
								{
									taxableTrans.AddRange(taxgroup.ToArray<GLTran>());
								}
								if (taxgroup == null || taxableTrans.Count == 0)
								{
									throw new PXException(Messages.NoTaxableLinesForTaxID, refNumber, taxID);
								}
								taxTran.CuryTaxableAmt = taxTran.TaxableAmt = Decimal.Zero;
								foreach (GLTran iTaxable in taxableTrans)
								{
									Account acct = PXSelectReadonly<Account, Where<Account.accountID,
										Equal<Required<Account.accountID>>>>.Select(glTranCache.Graph, iTaxable.AccountID);
									int accTypeSign = (acct.Type == AccountType.Income || acct.Type == AccountType.Liability) && taxType == TaxType.Sales ||
													  (acct.Type == AccountType.Asset || acct.Type == AccountType.Expense) && taxType == TaxType.Purchase ? 1 : -1;
									taxTran.CuryTaxableAmt += (iTaxable.CuryDebitAmt - iTaxable.CuryCreditAmt) * accTypeSign;
									taxTran.TaxableAmt += (iTaxable.DebitAmt - iTaxable.CreditAmt) * accTypeSign;
								}
								int sign = Math.Sign(taxTran.CuryTaxableAmt.Value);
								taxTran.CuryTaxableAmt *= sign;
								taxTran.TaxableAmt *= sign;
								taxTran.CuryTaxAmt = ((iTaxTran.CuryDebitAmt ?? Decimal.Zero) - (iTaxTran.CuryCreditAmt ?? Decimal.Zero)) * sign;
								taxTran.TaxAmt = ((iTaxTran.DebitAmt ?? Decimal.Zero) - (iTaxTran.CreditAmt ?? Decimal.Zero)) * sign;
								taxTran.TranType = GetTranType(taxType, sign);
								TaxRev taxRev = null;
								foreach (TaxRev iRev in PXSelectReadonly<TaxRev, Where<TaxRev.taxID, Equal<Required<TaxRev.taxID>>,
												And<TaxRev.outdated, Equal<boolFalse>,
												And<TaxRev.taxType, Equal<Required<TaxRev.taxType>>,
												And<Required<GLTran.tranDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>.Select(this.graph, taxID, taxType, taxTran.TranDate))
								{
									if (taxRev != null && iRev.TaxID == taxID)
									{
										throw new PXException(Messages.SeveralTaxRevisionFoundForTheTax, taxID, taxTran.TranDate);
									}
									taxRev = (TaxRev)this.graph.Caches[typeof(TaxRev)].CreateCopy(iRev);
									if (taxRev != null && tax.DeductibleVAT != true)
									{
										taxRev.NonDeductibleTaxRate = 100m;
									}
								}
								if (taxRev != null)
								{
									TaxTranCreationHelper.Copy(taxTran, taxRev);
									TaxTranCreationHelper.AdjustExpenseAmt(this.TaxTranCache, taxTran);
									if (!this.skipTaxValidation)
									{
										TaxTranCreationHelper.Validate(this.TaxTranCache, taxTran, taxRev, iTaxTran);
									}
									taxTran.Released = true;
									TaxTran copy = (TaxTran)this.TaxTranCache.Insert(taxTran);
									taxTransCreated = true;
								}
								else
								{
									throw new PXException(TX.Messages.TaxRevNotFound, tax.TaxID, taxType);
								}
							}
						}
					}
				}
				return taxTransCreated;
			}
			public static string GetTranType(string taxType, decimal sign)
			{
				string result = string.Empty;
				if (taxType == TaxType.Sales)
					result = sign > Decimal.Zero ? TranReversed : TranForward;
				if (taxType == TaxType.Purchase)
					result = sign > Decimal.Zero ? TranForward : TranReversed;
				if (string.IsNullOrEmpty(result))
				{
					throw new PXException(Messages.TypeForTheTaxTransactionIsNoRegonized, taxType, sign);
				}
				return result;
			}
			public static class TaxTranCreationHelper
			{
				public static void Copy(TaxTran aDest, TaxRev aSrc)
				{
					aDest.TaxRate = aSrc.TaxRate;
					aDest.TaxType = aSrc.TaxType;
					aDest.TaxBucketID = aSrc.TaxBucketID;
					aDest.NonDeductibleTaxRate = aSrc.NonDeductibleTaxRate;
				}
				public static void Copy(TaxTran aDest, GLTran aTaxTran, Tax taxDef)
				{
					aDest.TaxID = aTaxTran.TaxID;
					aDest.AccountID = aTaxTran.AccountID;
					aDest.SubID = aTaxTran.SubID;
					aDest.CuryInfoID = aTaxTran.CuryInfoID;
					aDest.Module = aTaxTran.Module;
					aDest.TranDate = aTaxTran.TranDate;
					aDest.BranchID = aTaxTran.BranchID;
					aDest.TranType = aTaxTran.TranType;
					aDest.RefNbr = aTaxTran.BatchNbr;
					aDest.FinPeriodID = aTaxTran.FinPeriodID;
					aDest.LineRefNbr = aTaxTran.RefNbr;
					aDest.VendorID = taxDef.TaxVendorID;
				}
				public static void Aggregate(GLTran aDest, GLTran aSrc)
				{
					aDest.CuryCreditAmt += aSrc.CuryCreditAmt ?? Decimal.Zero;
					aDest.CuryDebitAmt += aSrc.CuryDebitAmt ?? Decimal.Zero;
					aDest.CreditAmt += aSrc.CreditAmt ?? Decimal.Zero;
					aDest.DebitAmt += aSrc.DebitAmt ?? Decimal.Zero;
				}
				public static void Validate(PXCache cache, TaxTran tran, TaxRev taxRev, GLTran aDocTran)
				{

					if (Math.Sign(tran.CuryTaxAmt.Value) != 0 && Math.Sign(tran.CuryTaxableAmt.Value) != Math.Sign(tran.CuryTaxAmt.Value))
						throw new PXException(Messages.TaxableAndTaxAmountsHaveDifferentSignsForDocument, aDocTran.TaxID, aDocTran.RefNbr);
					if (tran.CuryTaxAmt.Value < 0)
						throw new PXException(Messages.TaxAmountIsNegativeForTheDocument, aDocTran.TaxID, aDocTran.RefNbr);

					TaxTran calculatedTax = CalculateTax(cache, tran, taxRev);
					if (calculatedTax.CuryTaxAmt != tran.CuryTaxAmt)
						if (decimal.Compare((decimal)tran.NonDeductibleTaxRate, 100m) < 0)
						{
							throw new PXException(Messages.DeductedTaxAmountEnteredDoesNotMatchToAmountCalculatedFromTaxableForTheDocument, tran.CuryTaxAmt.Value,
								calculatedTax.CuryTaxAmt, tran.CuryTaxableAmt.Value, tran.TaxRate, tran.NonDeductibleTaxRate, aDocTran.TaxID, aDocTran.RefNbr);
						}
						else
						{
							throw new PXException(Messages.TaxAmountEnteredDoesNotMatchToAmountCalculatedFromTaxableForTheDocument, tran.CuryTaxAmt.Value,
								calculatedTax.CuryTaxAmt, tran.CuryTaxableAmt.Value, tran.TaxRate, aDocTran.TaxID, aDocTran.RefNbr);
						}

					if (tran.CuryTaxableAmt != calculatedTax.CuryTaxableAmt)
					{
						tran.CuryTaxableAmt = calculatedTax.CuryTaxableAmt; //May happen due to min/max taxable limitations.
					}
				}
				public static TaxTran CalculateTax(PXCache cache, TaxTran aTran, TaxRev aTaxRev)
				{
					Decimal curyTaxableAmt = aTran.CuryTaxableAmt ?? Decimal.Zero;
					Decimal taxableAmt = aTran.TaxableAmt ?? Decimal.Zero;
					if (aTaxRev.TaxableMin != 0.0m)
					{
						if (taxableAmt < (decimal)aTaxRev.TaxableMin)
						{
							curyTaxableAmt = 0.0m;
							taxableAmt = 0.0m;
						}
					}
					if (aTaxRev.TaxableMax != 0.0m)
					{
						if (taxableAmt > (decimal)aTaxRev.TaxableMax)
						{
							PXDBCurrencyAttribute.CuryConvCury(cache, aTran, (decimal)aTaxRev.TaxableMax, out curyTaxableAmt);
							taxableAmt = (decimal)aTaxRev.TaxableMax;
						}
					}
					Decimal curyExpenseAmt = 0m;
					Decimal curyTaxAmount = (curyTaxableAmt * (decimal)aTaxRev.TaxRate / 100);
					if ((decimal)aTaxRev.NonDeductibleTaxRate < 100m)
					{
						curyExpenseAmt = curyTaxAmount * (1 - (decimal)aTaxRev.NonDeductibleTaxRate / 100);
						curyTaxAmount -= curyExpenseAmt;
					}
					TaxTran result = (TaxTran)cache.CreateCopy(aTran);
					result.CuryTaxableAmt = PXDBCurrencyAttribute.RoundCury(cache, aTran, curyTaxableAmt);
					result.CuryTaxAmt = PXDBCurrencyAttribute.RoundCury(cache, aTran, curyTaxAmount);//MS TaxAmount and Taxable account will be recalculated on insert anyway;
					result.CuryExpenseAmt = PXDBCurrencyAttribute.RoundCury(cache, aTran, curyExpenseAmt);
					return result;
				}
				public static void AdjustExpenseAmt(PXCache cache, TaxTran tran)
				{
					if (tran != null && (decimal)tran.NonDeductibleTaxRate < 100m)
					{
						decimal ExpenseRate = (decimal)tran.TaxRate * (100 - (decimal)tran.NonDeductibleTaxRate) / 100;
						Decimal curyTaxableAmount = (decimal)tran.CuryTaxableAmt / ((100 + ExpenseRate) / 100);
						Decimal curyExpenseAmt = curyTaxableAmount * ExpenseRate / 100;
						tran.CuryExpenseAmt = PXDBCurrencyAttribute.RoundCury(cache, tran, curyExpenseAmt);
						tran.CuryTaxableAmt = PXDBCurrencyAttribute.RoundCury(cache, tran, curyTaxableAmount);
					}
				}
				public static void SegregateTaxGroup(PXCache cache, List<GLTran> taxLines, Dictionary<string, Tax> taxes,
					Dictionary<string, List<GLTran>> taxCategories, Dictionary<string, HashSet<GLTran>> taxGroups)
				{
					foreach (var taxLine in taxLines)
					{
						if (taxLine.TaxID == null && !taxes.ContainsKey(taxLine.TaxID)) continue;
						PXResultset<TaxCategory> taxCategoryDetails = (new PXSelectJoin<TaxCategory, LeftJoin<TaxCategoryDet, 
							On<TaxCategory.taxCategoryID, Equal<TaxCategoryDet.taxCategoryID>>>,
								Where2<Where<TaxCategory.taxCatFlag, Equal<True>,
								And<Where<TaxCategoryDet.taxID, NotEqual<Required<TaxCategoryDet.taxID>>, Or<TaxCategoryDet.taxID, IsNull>>>>,
								Or<Where<TaxCategory.taxCatFlag, Equal<False>,
								And<TaxCategoryDet.taxID, Equal<Required<TaxCategoryDet.taxID>>>>>>>(cache.Graph)).Select(taxLine.TaxID, taxLine.TaxID);
						if (taxCategoryDetails.Count == 0) continue;
						foreach (TaxCategory tcd in taxCategoryDetails)
						{
							List<GLTran> catgroup;
							if (taxCategories.TryGetValue(tcd.TaxCategoryID, out catgroup))
							{
								HashSet<GLTran> taxgroup;
								if (!taxGroups.TryGetValue(taxLine.TaxID, out taxgroup))
								{
									taxgroup = new HashSet<GLTran>();
									taxGroups.Add(taxLine.TaxID, taxgroup);
								}
								taxgroup.UnionWith(catgroup);
							}
						}
					}
				}
			}
		}
        
        
        protected virtual void UpdateProjectBalance(Batch b)
        {
            PXSelectBase<GLTran> select = new PXSelectJoin<GLTran,
                InnerJoin<PMTran, On<GLTran.pMTranID, Equal<PMTran.tranID>>,
				InnerJoin<PMAccountGroup, On<PMTran.accountGroupID, Equal<PMAccountGroup.groupID>>,
				LeftJoin<Account, On<PMTran.offsetAccountID, Equal<Account.accountID>>,
				LeftJoin<OffsetPMAccountGroup, On<Account.accountGroupID, Equal<OffsetPMAccountGroup.groupID>>>>>>,
				Where<GLTran.module, Equal<Required<GLTran.module>>,
                And<GLTran.batchNbr, Equal<Required<GLTran.batchNbr>>,
                And<GLTran.pMTranID, IsNotNull,
				And<GLTran.projectID, NotEqual<Required<GLTran.projectID>>>>>>>(this);

			foreach (PXResult<GLTran, PMTran, PMAccountGroup, Account, OffsetPMAccountGroup> res in select.Select(b.Module, b.BatchNbr, ProjectDefaultAttribute.NonProject(this)))
			{
                GLTran tran = (GLTran)res;
				PMTran pmt = (PMTran)res;
				PMAccountGroup ag = (PMAccountGroup)res;
				Account offsetAccount = (Account)res;
				OffsetPMAccountGroup offsetAg = (OffsetPMAccountGroup)res;
				
				#region History Update

				bool isOffsetTran = pmt.OffsetAccountID == tran.AccountID;
				decimal amt = tran.DebitAmt.GetValueOrDefault() - tran.CreditAmt.GetValueOrDefault();

				if (isOffsetTran)
				{
					if (offsetAg.Type == AccountType.Income || offsetAg.Type == AccountType.Liability)
					{
						amt *= -1;
					}
				}
				else
				{
					if (ag.Type == AccountType.Income || ag.Type == AccountType.Liability)
					{
						amt *= -1;
					}
				}


				PMHistoryAccum hist = new PMHistoryAccum();
				hist.ProjectID = pmt.ProjectID;
				hist.ProjectTaskID = pmt.TaskID;
				hist.AccountGroupID = isOffsetTran ? offsetAccount.AccountGroupID : pmt.AccountGroupID;
				hist.InventoryID = pmt.InventoryID ?? PMProjectStatus.EmptyInventoryID;
				hist.PeriodID = pmt.FinPeriodID;

				hist = (PMHistoryAccum)this.Caches[typeof(PMHistoryAccum)].Insert(hist);

				decimal baseQty = 0;

				if (pmt.InventoryID != null && pmt.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID && pmt.Qty != null && pmt.Qty != 0 && !string.IsNullOrEmpty(pmt.UOM))
				{
					baseQty = PX.Objects.IN.INUnitAttribute.ConvertToBase(this.Caches[typeof(PMHistory)], pmt.InventoryID, pmt.UOM, pmt.Qty ?? 0, PX.Objects.IN.INPrecision.QUANTITY);
				}

				hist.FinPTDAmount += amt;
				hist.FinYTDAmount += amt;
				hist.FinPTDQty += baseQty;
				hist.FinYTDQty += baseQty;

				if (pmt.FinPeriodID == pmt.TranPeriodID)
				{
					hist.TranPTDAmount += amt;
					hist.TranYTDAmount += amt;
					hist.TranPTDQty += baseQty;
					hist.TranYTDQty += baseQty;
				}
				else
				{
					PMHistoryAccum tranHist = new PMHistoryAccum();
					tranHist.ProjectID = pmt.ProjectID;
					tranHist.ProjectTaskID = pmt.TaskID;
					tranHist.AccountGroupID = isOffsetTran ? offsetAccount.AccountGroupID : pmt.AccountGroupID;
					tranHist.InventoryID = pmt.InventoryID ?? PM.PMProjectStatus.EmptyInventoryID;
					tranHist.PeriodID = pmt.TranPeriodID;

					tranHist = (PMHistoryAccum)this.Caches[typeof(PMHistoryAccum)].Insert(tranHist);
					tranHist.TranPTDAmount += amt;
					tranHist.TranYTDAmount += amt;
					tranHist.TranPTDQty += baseQty;
					tranHist.TranYTDQty += baseQty;

				}
				this.Views.Caches.Add(typeof(PMHistoryAccum));

				#endregion
			}
        }

		protected virtual void AutoAllocateTasks(List<PMTask> tasks)
		{
			PMSetup setup = PXSelect<PMSetup>.Select(this);
			bool autoreleaseAllocation = setup.AutoReleaseAllocation == true;

			PMAllocator allocator = PXGraph.CreateInstance<PMAllocator>();
			allocator.Clear();
			allocator.TimeStamp = this.TimeStamp;
			allocator.Execute(tasks);
			allocator.Actions.PressSave();

			if (allocator.Document.Current != null && autoreleaseAllocation)
			{
				List<PMRegister> list = new List<PMRegister>();
				list.Add(allocator.Document.Current);
				List<ProcessInfo<Batch>> batchList;
				bool releaseSuccess = RegisterRelease.ReleaseWithoutPost(list, false, out batchList);
				if (!releaseSuccess)
				{
					throw new PXException(PM.Messages.AutoReleaseFailed);
				}
			}
		}

		[PXHidden]
        [Serializable]
		public partial class OffsetPMAccountGroup : PMAccountGroup
		{
			#region GroupID
			public new abstract class groupID : PX.Data.IBqlField
			{
			}

			#endregion
			#region GroupCD
			public new abstract class groupCD : PX.Data.IBqlField
			{
			}
			#endregion
			#region Type
			public new abstract class type : PX.Data.IBqlField
			{
			}
			#endregion
		}
    }
}



namespace PX.Objects.GL.Overrides.PostGraph
{
    [Serializable]
    public partial class BatchCopy : Batch
    {
        public new abstract class origBatchNbr : PX.Data.IBqlField { }
        public new abstract class origModule : PX.Data.IBqlField { }
        public new abstract class autoReverseCopy : PX.Data.IBqlField { }
    }

    public class AHAccumAttribute : PXAccumulatorAttribute
    {
        protected int? reacct;
        protected int? niacct;

        public AHAccumAttribute()
            : base(new Type[] {
				typeof(GLHistory.finYtdBalance),
				typeof(GLHistory.tranYtdBalance),
				typeof(GLHistory.curyFinYtdBalance),
				typeof(GLHistory.curyTranYtdBalance),
				typeof(GLHistory.finYtdBalance),
				typeof(GLHistory.tranYtdBalance),
				typeof(GLHistory.curyFinYtdBalance),
				typeof(GLHistory.curyTranYtdBalance)
				},
                    new Type[] {
				typeof(GLHistory.finBegBalance),
				typeof(GLHistory.tranBegBalance),
				typeof(GLHistory.curyFinBegBalance),
				typeof(GLHistory.curyTranBegBalance),
				typeof(GLHistory.finYtdBalance),
				typeof(GLHistory.tranYtdBalance),
				typeof(GLHistory.curyFinYtdBalance),
				typeof(GLHistory.curyTranYtdBalance)
				}
            )
        {
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            GLSetup setup = (GLSetup)sender.Graph.Caches[typeof(GLSetup)].Current;
            if (setup == null)
            {
                setup = PXSelect<GLSetup>.Select(sender.Graph);
            }
            if (setup != null)
            {
                reacct = setup.RetEarnAccountID;
                niacct = setup.YtdNetIncAccountID;
            }

			sender.Graph.RowPersisted.AddHandler(sender.GetItemType(), RowPersisted);
        }

        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            columns.InitializeFrom<GLHistory.finBegBalance, GLHistory.finYtdBalance>();
            columns.InitializeFrom<GLHistory.tranBegBalance, GLHistory.tranYtdBalance>();
            columns.InitializeFrom<GLHistory.curyFinBegBalance, GLHistory.curyFinYtdBalance>();
            columns.InitializeFrom<GLHistory.curyTranBegBalance, GLHistory.curyTranYtdBalance>();
            columns.InitializeFrom<GLHistory.finYtdBalance, GLHistory.finYtdBalance>();
            columns.InitializeFrom<GLHistory.tranYtdBalance, GLHistory.tranYtdBalance>();
            columns.InitializeFrom<GLHistory.curyFinYtdBalance, GLHistory.curyFinYtdBalance>();
            columns.InitializeFrom<GLHistory.curyTranYtdBalance, GLHistory.curyTranYtdBalance>();

            BaseGLHistory hist = (BaseGLHistory)row;

            columns.UpdateFuture<GLHistory.finBegBalance>(hist.FinYtdBalance);
            columns.UpdateFuture<GLHistory.tranBegBalance>(hist.TranYtdBalance);
            columns.UpdateFuture<GLHistory.curyFinBegBalance>(hist.CuryFinYtdBalance);
            columns.UpdateFuture<GLHistory.curyTranBegBalance>(hist.CuryTranYtdBalance);
            columns.UpdateFuture<GLHistory.finYtdBalance>(hist.FinYtdBalance);
            columns.UpdateFuture<GLHistory.tranYtdBalance>(hist.TranYtdBalance);
            columns.UpdateFuture<GLHistory.curyFinYtdBalance>(hist.CuryFinYtdBalance);
            columns.UpdateFuture<GLHistory.curyTranYtdBalance>(hist.CuryTranYtdBalance);

            bool? year = (hist.AccountID == niacct);
            if (year == false && hist.AccountID != reacct)
            {
                PXCache cache = sender.Graph.Caches[typeof(Account)];
                year = null;
                foreach (Account acct in cache.Cached)
                {
                    if (acct.AccountID == hist.AccountID)
                    {
                        year = (acct.Type == AccountType.Income || acct.Type == AccountType.Expense);
                        break;
                    }
                }
                if (year == null)
                {
                    Account acct = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(sender.Graph, hist.AccountID);
                    if (acct != null)
                    {
                        year = (acct.Type == AccountType.Income || acct.Type == AccountType.Expense);
                    }
                    else
                    {
                        year = false;
                    }
                }
            }
            if (year == true)
            {
                columns.RestrictPast<GLHistory.finPeriodID>(PXComp.GE, hist.FinPeriodID.Substring(0, 4) + "01");
                columns.RestrictFuture<GLHistory.finPeriodID>(PXComp.LE, hist.FinPeriodID.Substring(0, 4) + "99");
            }

			if (hist.REFlag == true)
			{
				columns.UpdateOnly = true;
			}

            return true;
        }

		protected string PreviousPeriod(string FinPeriodID)
		{
 			using (PXDataRecord rec = PXDatabase.SelectSingle<FinPeriod>(
				new PXDataField<FinPeriod.finPeriodID>(),
				new PXDataFieldValue<FinPeriod.finPeriodID>(PXDbType.Char, 6, FinPeriodID, PXComp.LT),
				new PXDataFieldOrder<FinPeriod.finPeriodID>(true)))
			{
				return rec != null ? rec.GetString(0) : null;
			}
		}

		public override bool PersistInserted(PXCache sender, object row)
		{
			try
			{
				return base.PersistInserted(sender, row);
			}
			catch (PXLockViolationException)
			{
				if (((BaseGLHistory)row).REFlag != true)
				{
					throw;
				}

				return true;
			}
		}

		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
			{
				BaseGLHistory hist = e.Row as GLHistory;

				if (hist != null && hist.AccountID == reacct && hist.FinPeriodID.Substring(4, 2) == "01")
				{
					string PrevPeriodID = PreviousPeriod(hist.FinPeriodID);

					PXUpdateJoin<
						Set<AcctHist.yearClosed, True,
						Set<AcctHist.curyFinBegBalance, Add<AcctHist.curyFinBegBalance, IsNull<AcctHist2.curyFinYtdBalance, decimal0>>,
						Set<AcctHist.curyFinYtdBalance, Add<AcctHist.curyFinYtdBalance, IsNull<AcctHist2.curyFinYtdBalance, decimal0>>,
						Set<AcctHist.curyTranBegBalance, Add<AcctHist.curyTranBegBalance, IsNull<AcctHist2.curyTranYtdBalance, decimal0>>,
						Set<AcctHist.curyTranYtdBalance, Add<AcctHist.curyTranYtdBalance, IsNull<AcctHist2.curyTranYtdBalance, decimal0>>,
						Set<AcctHist.finBegBalance, Add<AcctHist.finBegBalance, IsNull<AcctHist2.finYtdBalance, decimal0>>,
						Set<AcctHist.finYtdBalance, Add<AcctHist.finYtdBalance, IsNull<AcctHist2.finYtdBalance, decimal0>>,
						Set<AcctHist.tranBegBalance, Add<AcctHist.tranBegBalance, IsNull<AcctHist2.tranYtdBalance, decimal0>>,
						Set<AcctHist.tranYtdBalance, Add<AcctHist.tranYtdBalance, IsNull<AcctHist2.tranYtdBalance, decimal0>>>>>>>>>>>,
					AcctHist,
					InnerJoin<GLHistoryByPeriod,
						On<GLHistoryByPeriod.ledgerID, Equal<AcctHist.ledgerID>,
						And<GLHistoryByPeriod.branchID, Equal<AcctHist.branchID>,
						And<GLHistoryByPeriod.accountID, Equal<Required<Account.accountID>>,
						And<GLHistoryByPeriod.subID, Equal<AcctHist.subID>,
						And<GLHistoryByPeriod.finPeriodID, Equal<Required<FinPeriod.finPeriodID>>>>>>>,
					LeftJoin<AcctHist3, //Retained Earnings 
						On<AcctHist3.ledgerID, Equal<AcctHist.ledgerID>,
						And<AcctHist3.branchID, Equal<AcctHist.branchID>, 
						And<AcctHist3.accountID, Equal<AcctHist.accountID>,
						And<AcctHist3.subID, Equal<AcctHist.subID>,
						And<Substring<AcctHist3.finPeriodID, int1, int4>, Greater<Substring<GLHistoryByPeriod.lastActivityPeriod,int1, int4>>, 
						And<AcctHist3.finPeriodID, LessEqual<Required<FinPeriod.finPeriodID>>>>>>>>,
					LeftJoin<AcctHist2, //Net Income
						On<AcctHist2.ledgerID, Equal<GLHistoryByPeriod.ledgerID>,
						And<AcctHist2.branchID, Equal<GLHistoryByPeriod.branchID>,
						And<AcctHist2.accountID, Equal<GLHistoryByPeriod.accountID>,
						And<AcctHist2.subID, Equal<GLHistoryByPeriod.subID>,
						And<AcctHist2.finPeriodID, Equal<GLHistoryByPeriod.lastActivityPeriod>,
						And<AcctHist3.accountID, IsNull>>>>>>>>>,
					Where<AcctHist.accountID, Equal<Required<AcctHist.accountID>>,
						And<AcctHist.subID, Equal<Required<AcctHist.subID>>,
						And<AcctHist.ledgerID, Equal<Required<AcctHist.subID>>,
						And<AcctHist.branchID, Equal<Required<AcctHist.branchID>>,
						And<AcctHist.finPeriodID, Equal<Required<AcctHist.finPeriodID>>,
						And<AcctHist.yearClosed, Equal<False>>>>>>>>.Update(sender.Graph, niacct, PrevPeriodID, PrevPeriodID, reacct, hist.SubID, hist.LedgerID, hist.BranchID, hist.FinPeriodID);
				}
			}
		}
    }

    [Serializable]
    [PXAccumulator]
    public partial class ConsolHist : GLConsolHistory
    {
    }

	public class AcctHistDefaultAttribute : PXDefaultAttribute
	{
		public override PXEventSubscriberAttribute Clone(PXAttributeLevel attributeLevel)
		{
			if (attributeLevel == PXAttributeLevel.Item)
			{
				return this;
			}
			return base.Clone(attributeLevel);
		}
		public AcctHistDefaultAttribute(Type sourceType)
			: base(sourceType)
		{
		}
	}

	public class AcctHistDBDecimalAttribute : PXDBDecimalAttribute, IPXFieldDefaultingSubscriber
	{
		public override PXEventSubscriberAttribute Clone(PXAttributeLevel attributeLevel)
		{
			if (attributeLevel == PXAttributeLevel.Item)
			{
				return this;
			}
			return base.Clone(attributeLevel);
		}
		public AcctHistDBDecimalAttribute()
			: base(4)
		{
		}
		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = 0m;
		}
	}

	public class AcctHistDBIntAttribute : PXDBIntAttribute
	{
		public override PXEventSubscriberAttribute Clone(PXAttributeLevel attributeLevel)
		{
			if (attributeLevel == PXAttributeLevel.Item)
			{
				return this;
			}
			return base.Clone(attributeLevel);
		}
		public AcctHistDBIntAttribute()
		{
		}
	}

	public class AcctHistDBStringAttribute : PXDBStringAttribute
	{
		public override PXEventSubscriberAttribute Clone(PXAttributeLevel attributeLevel)
		{
			if (attributeLevel == PXAttributeLevel.Item)
			{
				return this;
			}
			return base.Clone(attributeLevel);
		}
		public AcctHistDBStringAttribute(int length)
			: base(length)
		{
		}
	}

	public class AcctHistDBTimestamp : PXDBTimestampAttribute
	{
		public override PXEventSubscriberAttribute Clone(PXAttributeLevel attributeLevel)
		{
			if (attributeLevel == PXAttributeLevel.Item)
			{
				return this;
			}
			return base.Clone(attributeLevel);
		}
		public AcctHistDBTimestamp()
		{
		}
	}

	[System.SerializableAttribute()]
    [AHAccum]
    [PXDisableCloneAttributes()]
	[PXBreakInheritance()]
    public partial class AcctHist : GLHistory
	{
		#region BranchID
		[AcctHistDBInt(IsKey = true)]
		public override Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region LedgerID
        [AcctHistDBInt(IsKey = true)]
        public override Int32? LedgerID
        {
            get
            {
                return this._LedgerID;
            }
            set
            {
                this._LedgerID = value;
            }
        }
        #endregion
        #region AccountID
        [AcctHistDBInt(IsKey = true)]
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
        [AcctHistDBInt(IsKey = true)]
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
        #region FinPeriod
        [AcctHistDBString(6, IsFixed = true, IsKey = true)]
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
        #region BalanceType
        [AcctHistDBString(1, IsFixed = true)]
        [AcctHistDefault(typeof(Ledger.balanceType))]
        public override String BalanceType
        {
            get
            {
                return this._BalanceType;
            }
            set
            {
                this._BalanceType = value;
            }
        }
        #endregion
        #region CuryID
        [AcctHistDBString(5, IsUnicode = true)]
        public override String CuryID
        {
            get
            {
                return this._CuryID;
            }
            set
            {
                this._CuryID = value;
            }
        }
        #endregion
        #region FinYear
        public new abstract class finYear : PX.Data.IBqlField
        {
        }
        public override String FinYear
        {
            [PXDependsOnFields(typeof(finPeriodID))]
            get
            {
                return FiscalPeriodUtils.FiscalYear(this._FinPeriodID);
            }
        }
        #endregion
        #region DetDeleted
        public new abstract class detDeleted : PX.Data.IBqlField
        {
        }
        #endregion
		#region YearClosed
		public new abstract class yearClosed : PX.Data.IBqlField { }
		#endregion
		#region FinPtdCredit
		[AcctHistDBDecimal]
		public override Decimal? FinPtdCredit
		{
			get
			{
				return this._FinPtdCredit;
			}
			set
			{
				this._FinPtdCredit = value;
			}
		}
		#endregion
		#region FinPtdDebit
		[AcctHistDBDecimal]
		public override Decimal? FinPtdDebit
		{
			get
			{
				return this._FinPtdDebit;
			}
			set
			{
				this._FinPtdDebit = value;
			}
		}
		#endregion
		#region FinYtdBalance
		public new abstract class finYtdBalance : IBqlField { }
		[AcctHistDBDecimal]
		public override Decimal? FinYtdBalance
		{
			get
			{
				return this._FinYtdBalance;
			}
			set
			{
				this._FinYtdBalance = value;
			}
		}
		#endregion
		#region FinBegBalance
		public new abstract class finBegBalance : IBqlField { }
		[AcctHistDBDecimal]
		public override Decimal? FinBegBalance
		{
			get
			{
				return this._FinBegBalance;
			}
			set
			{
				this._FinBegBalance = value;
			}
		}
		#endregion
		#region FinPtdRevalued
		[AcctHistDBDecimal]
		public override Decimal? FinPtdRevalued
		{
			get
			{
				return this._FinPtdRevalued;
			}
			set
			{
				this._FinPtdRevalued = value;
			}
		}
		#endregion
		#region TranPtdCredit
		[AcctHistDBDecimal]
		public override Decimal? TranPtdCredit
		{
			get
			{
				return this._TranPtdCredit;
			}
			set
			{
				this._TranPtdCredit = value;
			}
		}
		#endregion
		#region TranPtdDebit
		[AcctHistDBDecimal]
		public override Decimal? TranPtdDebit
		{
			get
			{
				return this._TranPtdDebit;
			}
			set
			{
				this._TranPtdDebit = value;
			}
		}
		#endregion
		#region TranYtdBalance
		public new abstract class tranYtdBalance : IBqlField { }
		[AcctHistDBDecimal]
		public override Decimal? TranYtdBalance
		{
			get
			{
				return this._TranYtdBalance;
			}
			set
			{
				this._TranYtdBalance = value;
			}
		}
		#endregion
		#region TranBegBalance
		public new abstract class tranBegBalance : IBqlField { }
		[AcctHistDBDecimal]
		public override Decimal? TranBegBalance
		{
			get
			{
				return this._TranBegBalance;
			}
			set
			{
				this._TranBegBalance = value;
			}
		}
		#endregion
		#region CuryFinPtdCredit
		[AcctHistDBDecimal]
		public override Decimal? CuryFinPtdCredit
		{
			get
			{
				return this._CuryFinPtdCredit;
			}
			set
			{
				this._CuryFinPtdCredit = value;
			}
		}
		#endregion
		#region CuryFinPtdDebit
		[AcctHistDBDecimal]
		public override Decimal? CuryFinPtdDebit
		{
			get
			{
				return this._CuryFinPtdDebit;
			}
			set
			{
				this._CuryFinPtdDebit = value;
			}
		}
		#endregion
		#region CuryFinYtdBalance
		public new abstract class curyFinYtdBalance : IBqlField { }
		[AcctHistDBDecimal]
		public override Decimal? CuryFinYtdBalance
		{
			get
			{
				return this._CuryFinYtdBalance;
			}
			set
			{
				this._CuryFinYtdBalance = value;
			}
		}
		#endregion
		#region CuryFinBegBalance
		public new abstract class curyFinBegBalance : IBqlField { }
		[AcctHistDBDecimal]
		public override Decimal? CuryFinBegBalance
		{
			get
			{
				return this._CuryFinBegBalance;
			}
			set
			{
				this._CuryFinBegBalance = value;
			}
		}
		#endregion
		#region CuryTranPtdCredit
		[AcctHistDBDecimal]
		public override Decimal? CuryTranPtdCredit
		{
			get
			{
				return this._CuryTranPtdCredit;
			}
			set
			{
				this._CuryTranPtdCredit = value;
			}
		}
		#endregion
		#region CuryTranPtdDebit
		[AcctHistDBDecimal]
		public override Decimal? CuryTranPtdDebit
		{
			get
			{
				return this._CuryTranPtdDebit;
			}
			set
			{
				this._CuryTranPtdDebit = value;
			}
		}
		#endregion
		#region CuryTranYtdBalance
		public new abstract class curyTranYtdBalance : IBqlField { }
		[AcctHistDBDecimal]
		public override Decimal? CuryTranYtdBalance
		{
			get
			{
				return this._CuryTranYtdBalance;
			}
			set
			{
				this._CuryTranYtdBalance = value;
			}
		}
		#endregion
		#region CuryTranBegBalance
		public new abstract class curyTranBegBalance : IBqlField { }
		[AcctHistDBDecimal]
		public override Decimal? CuryTranBegBalance
		{
			get
			{
				return this._CuryTranBegBalance;
			}
			set
			{
				this._CuryTranBegBalance = value;
			}
		}
		#endregion
		#region FinFlag
		public override bool? FinFlag
		{
			get
			{
				return this._FinFlag;
			}
			set
			{
				this._FinFlag = value;
			}
		}
		#endregion
		#region PtdCredit
		public override Decimal? PtdCredit
		{
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdCredit : this._TranPtdCredit;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdCredit = value;
				}
				else
				{
					this._TranPtdCredit = value;
				}
			}
		}
		#endregion
		#region PtdDebit
		public override Decimal? PtdDebit
		{
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdDebit : this._TranPtdDebit;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdDebit = value;
				}
				else
				{
					this._TranPtdDebit = value;
				}
			}
		}
		#endregion
		#region YtdBalance
		public override Decimal? YtdBalance
		{
			get
			{
				return ((bool)_FinFlag) ? this._FinYtdBalance : this._TranYtdBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinYtdBalance = value;
				}
				else
				{
					this._TranYtdBalance = value;
				}
			}
		}
		#endregion
		#region BegBalance
		public override Decimal? BegBalance
		{
			get
			{
				return ((bool)_FinFlag) ? this._FinBegBalance : this._TranBegBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinBegBalance = value;
				}
				else
				{
					this._TranBegBalance = value;
				}
			}
		}
		#endregion
		#region PtdRevalued
		public override Decimal? PtdRevalued
		{
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdRevalued : null;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdRevalued = value;
				}
			}
		}
		#endregion
		#region CuryPtdCredit
		public override Decimal? CuryPtdCredit
		{
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdCredit : this._CuryTranPtdCredit;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdCredit = value;
				}
				else
				{
					this._CuryTranPtdCredit = value;
				}
			}
		}
		#endregion
		#region CuryPtdDebit
		public override Decimal? CuryPtdDebit
		{
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdDebit : this._CuryTranPtdDebit;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdDebit = value;
				}
				else
				{
					this._CuryTranPtdDebit = value;
				}
			}
		}
		#endregion
		#region CuryYtdBalance
		public override Decimal? CuryYtdBalance
		{
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinYtdBalance : this._CuryTranYtdBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinYtdBalance = value;
				}
				else
				{
					this._CuryTranYtdBalance = value;
				}
			}
		}
		#endregion
		#region CuryBegBalance
		public override Decimal? CuryBegBalance
		{
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinBegBalance : this._CuryTranBegBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinBegBalance = value;
				}
				else
				{
					this._CuryTranBegBalance = value;
				}
			}
		}
		#endregion
		#region tstamp
		[AcctHistDBTimestamp]
		public override Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
	}

	[System.SerializableAttribute()]
	[PXBreakInheritance()]
	public partial class AcctHist2 : GLHistory
	{
		#region LedgerID
		public new abstract class ledgerID : PX.Data.IBqlField
		{
		}
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.IBqlField
		{
		}
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.IBqlField
		{
		}
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.IBqlField
		{
		}
		#endregion
		#region FinPeriod
		public new abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryID
		public new abstract class curyID : PX.Data.IBqlField
		{
		}
		#endregion
		#region DetDeleted
		public new abstract class detDeleted : PX.Data.IBqlField
		{
		}
		#endregion
		#region YearClosed
		public new abstract class yearClosed : PX.Data.IBqlField
		{
		}
		#endregion
		#region FinPtdCredit
		public new abstract class finPtdCredit : PX.Data.IBqlField
		{
		}
		#endregion
		#region FinPtdDebit
		public new abstract class finPtdDebit : PX.Data.IBqlField
		{
		}
		#endregion
		#region FinYtdBalance
		public new abstract class finYtdBalance : PX.Data.IBqlField
		{
		}
		#endregion
		#region FinBegBalance
		public new abstract class finBegBalance : PX.Data.IBqlField
		{
		}
		#endregion
		#region FinPtdRevalued
		public new abstract class finPtdRevalued : PX.Data.IBqlField
		{
		}
		#endregion
		#region TranPtdCredit
		public new abstract class tranPtdCredit : PX.Data.IBqlField
		{
		}
		#endregion
		#region TranPtdDebit
		public new abstract class tranPtdDebit : PX.Data.IBqlField
		{
		}
		#endregion
		#region TranYtdBalance
		public new abstract class tranYtdBalance : PX.Data.IBqlField
		{
		}
		#endregion
		#region TranBegBalance
		public new abstract class tranBegBalance : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryFinPtdCredit
		public new abstract class curyFinPtdCredit : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryFinPtdDebit
		public new abstract class curyFinPtdDebit : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryFinYtdBalance
		public new abstract class curyFinYtdBalance : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryFinBegBalance
		public new abstract class curyFinBegBalance : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryTranPtdCredit
		public new abstract class curyTranPtdCredit : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryTranPtdDebit
		public new abstract class curyTranPtdDebit : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryTranYtdBalance
		public new abstract class curyTranYtdBalance : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryTranBegBalance
		public new abstract class curyTranBegBalance : PX.Data.IBqlField
		{
		}
		#endregion
	}

	[System.SerializableAttribute()]
	[PXBreakInheritance()]
	public partial class AcctHist3 : GLHistory
	{
		#region LedgerID
		public new abstract class ledgerID : PX.Data.IBqlField
		{
		}
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.IBqlField
		{
		}
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.IBqlField
		{
		}
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.IBqlField
		{
		}
		#endregion
		#region FinPeriod
		public new abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryID
		public new abstract class curyID : PX.Data.IBqlField
		{
		}
		#endregion
		#region DetDeleted
		public new abstract class detDeleted : PX.Data.IBqlField
		{
		}
		#endregion
		#region YearClosed
		public new abstract class yearClosed : PX.Data.IBqlField
		{
		}
		#endregion
		#region FinPtdCredit
		public new abstract class finPtdCredit : PX.Data.IBqlField
		{
		}
		#endregion
		#region FinPtdDebit
		public new abstract class finPtdDebit : PX.Data.IBqlField
		{
		}
		#endregion
		#region FinYtdBalance
		public new abstract class finYtdBalance : PX.Data.IBqlField
		{
		}
		#endregion
		#region FinBegBalance
		public new abstract class finBegBalance : PX.Data.IBqlField
		{
		}
		#endregion
		#region FinPtdRevalued
		public new abstract class finPtdRevalued : PX.Data.IBqlField
		{
		}
		#endregion
		#region TranPtdCredit
		public new abstract class tranPtdCredit : PX.Data.IBqlField
		{
		}
		#endregion
		#region TranPtdDebit
		public new abstract class tranPtdDebit : PX.Data.IBqlField
		{
		}
		#endregion
		#region TranYtdBalance
		public new abstract class tranYtdBalance : PX.Data.IBqlField
		{
		}
		#endregion
		#region TranBegBalance
		public new abstract class tranBegBalance : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryFinPtdCredit
		public new abstract class curyFinPtdCredit : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryFinPtdDebit
		public new abstract class curyFinPtdDebit : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryFinYtdBalance
		public new abstract class curyFinYtdBalance : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryFinBegBalance
		public new abstract class curyFinBegBalance : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryTranPtdCredit
		public new abstract class curyTranPtdCredit : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryTranPtdDebit
		public new abstract class curyTranPtdDebit : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryTranYtdBalance
		public new abstract class curyTranYtdBalance : PX.Data.IBqlField
		{
		}
		#endregion
		#region CuryTranBegBalance
		public new abstract class curyTranBegBalance : PX.Data.IBqlField
		{
		}
		#endregion
	}	

    [System.SerializableAttribute()]
    [AHAccum]
    [PXDisableCloneAttributes()]
	[PXBreakInheritance()]
    public partial class CuryAcctHist : CuryGLHistory
    {
		#region BranchID
		[AcctHistDBInt(IsKey = true)]
		public override Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region LedgerID
        [AcctHistDBInt(IsKey = true)]
        public override Int32? LedgerID
        {
            get
            {
                return this._LedgerID;
            }
            set
            {
                this._LedgerID = value;
            }
        }
        #endregion
        #region AccountID
        [AcctHistDBInt(IsKey = true)]
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
        [AcctHistDBInt(IsKey = true)]
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
        #region CuryID
        [AcctHistDBString(5, IsUnicode = true, IsKey = true)]
        public override String CuryID
        {
            get
            {
                return this._CuryID;
            }
            set
            {
                this._CuryID = value;
            }
        }
        #endregion
        #region FinPeriod
        [AcctHistDBString(6, IsFixed = true, IsKey = true)]
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
        #region BalanceType
        [AcctHistDBString(1, IsFixed = true)]
        [AcctHistDefault(typeof(Ledger.balanceType))]
        public override String BalanceType
        {
            get
            {
                return this._BalanceType;
            }
            set
            {
                this._BalanceType = value;
            }
        }
        #endregion
        #region BaseCuryID
        [AcctHistDBString(5, IsUnicode = true)]
        public override String BaseCuryID
        {
            get
            {
                return this._BaseCuryID;
            }
            set
            {
                this._BaseCuryID = value;
            }
        }
        #endregion
        #region FinYear
        public new abstract class finYear : PX.Data.IBqlField
        {
        }
        public override String FinYear
        {
            [PXDependsOnFields(typeof(finPeriodID))]
            get
            {
                return (_FinPeriodID == null) ? null : FiscalPeriodUtils.FiscalYear(this._FinPeriodID);
            }
        }
        #endregion
        #region DetDeleted
        public new abstract class detDeleted : PX.Data.IBqlField
        {
        }
        #endregion
		#region FinPtdCredit
		[AcctHistDBDecimal]
		public override Decimal? FinPtdCredit
		{
			get
			{
				return this._FinPtdCredit;
			}
			set
			{
				this._FinPtdCredit = value;
			}
		}
		#endregion
		#region FinPtdDebit
		[AcctHistDBDecimal]
		public override Decimal? FinPtdDebit
		{
			get
			{
				return this._FinPtdDebit;
			}
			set
			{
				this._FinPtdDebit = value;
			}
		}
		#endregion
		#region FinYtdBalance
		[AcctHistDBDecimal]
		public override Decimal? FinYtdBalance
		{
			get
			{
				return this._FinYtdBalance;
			}
			set
			{
				this._FinYtdBalance = value;
			}
		}
		#endregion
		#region FinBegBalance
		[AcctHistDBDecimal]
		public override Decimal? FinBegBalance
		{
			get
			{
				return this._FinBegBalance;
			}
			set
			{
				this._FinBegBalance = value;
			}
		}
		#endregion
		#region FinPtdRevalued
		public override Decimal? FinPtdRevalued
		{
			get
			{
				return this._FinPtdRevalued;
			}
			set
			{
				this._FinPtdRevalued = value;
			}
		}
		#endregion
		#region TranPtdCredit
		[AcctHistDBDecimal]
		public override Decimal? TranPtdCredit
		{
			get
			{
				return this._TranPtdCredit;
			}
			set
			{
				this._TranPtdCredit = value;
			}
		}
		#endregion
		#region TranPtdDebit
		[AcctHistDBDecimal]
		public override Decimal? TranPtdDebit
		{
			get
			{
				return this._TranPtdDebit;
			}
			set
			{
				this._TranPtdDebit = value;
			}
		}
		#endregion
		#region TranYtdBalance
		[AcctHistDBDecimal]
		public override Decimal? TranYtdBalance
		{
			get
			{
				return this._TranYtdBalance;
			}
			set
			{
				this._TranYtdBalance = value;
			}
		}
		#endregion
		#region TranBegBalance
		[AcctHistDBDecimal]
		public override Decimal? TranBegBalance
		{
			get
			{
				return this._TranBegBalance;
			}
			set
			{
				this._TranBegBalance = value;
			}
		}
		#endregion
		#region CuryFinPtdCredit
		[AcctHistDBDecimal]
		public override Decimal? CuryFinPtdCredit
		{
			get
			{
				return this._CuryFinPtdCredit;
			}
			set
			{
				this._CuryFinPtdCredit = value;
			}
		}
		#endregion
		#region CuryFinPtdDebit
		[AcctHistDBDecimal]
		public override Decimal? CuryFinPtdDebit
		{
			get
			{
				return this._CuryFinPtdDebit;
			}
			set
			{
				this._CuryFinPtdDebit = value;
			}
		}
		#endregion
		#region CuryFinYtdBalance
		[AcctHistDBDecimal]
		public override Decimal? CuryFinYtdBalance
		{
			get
			{
				return this._CuryFinYtdBalance;
			}
			set
			{
				this._CuryFinYtdBalance = value;
			}
		}
		#endregion
		#region CuryFinBegBalance
		[AcctHistDBDecimal]
		public override Decimal? CuryFinBegBalance
		{
			get
			{
				return this._CuryFinBegBalance;
			}
			set
			{
				this._CuryFinBegBalance = value;
			}
		}
		#endregion
		#region CuryTranPtdCredit
		[AcctHistDBDecimal]
		public override Decimal? CuryTranPtdCredit
		{
			get
			{
				return this._CuryTranPtdCredit;
			}
			set
			{
				this._CuryTranPtdCredit = value;
			}
		}
		#endregion
		#region CuryTranPtdDebit
		[AcctHistDBDecimal]
		public override Decimal? CuryTranPtdDebit
		{
			get
			{
				return this._CuryTranPtdDebit;
			}
			set
			{
				this._CuryTranPtdDebit = value;
			}
		}
		#endregion
		#region CuryTranYtdBalance
		[AcctHistDBDecimal]
		public override Decimal? CuryTranYtdBalance
		{
			get
			{
				return this._CuryTranYtdBalance;
			}
			set
			{
				this._CuryTranYtdBalance = value;
			}
		}
		#endregion
		#region CuryTranBegBalance
		[AcctHistDBDecimal]
		public override Decimal? CuryTranBegBalance
		{
			get
			{
				return this._CuryTranBegBalance;
			}
			set
			{
				this._CuryTranBegBalance = value;
			}
		}
		#endregion
		#region FinFlag
		public override bool? FinFlag
		{
			get
			{
				return this._FinFlag;
			}
			set
			{
				this._FinFlag = value;
			}
		}
		#endregion
		#region PtdCredit
		public override Decimal? PtdCredit
		{
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdCredit : this._TranPtdCredit;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdCredit = value;
				}
				else
				{
					this._TranPtdCredit = value;
				}
			}
		}
		#endregion
		#region PtdDebit
		public override Decimal? PtdDebit
		{
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdDebit : this._TranPtdDebit;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdDebit = value;
				}
				else
				{
					this._TranPtdDebit = value;
				}
			}
		}
		#endregion
		#region YtdBalance
		public override Decimal? YtdBalance
		{
			get
			{
				return ((bool)_FinFlag) ? this._FinYtdBalance : this._TranYtdBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinYtdBalance = value;
				}
				else
				{
					this._TranYtdBalance = value;
				}
			}
		}
		#endregion
		#region BegBalance
		public override Decimal? BegBalance
		{
			get
			{
				return ((bool)_FinFlag) ? this._FinBegBalance : this._TranBegBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinBegBalance = value;
				}
				else
				{
					this._TranBegBalance = value;
				}
			}
		}
		#endregion
		#region PtdRevalued
		public override Decimal? PtdRevalued
		{
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdRevalued : null;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdRevalued = value;
				}
			}
		}
		#endregion
		#region CuryPtdCredit
		public override Decimal? CuryPtdCredit
		{
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdCredit : this._CuryTranPtdCredit;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdCredit = value;
				}
				else
				{
					this._CuryTranPtdCredit = value;
				}
			}
		}
		#endregion
		#region CuryPtdDebit
		public override Decimal? CuryPtdDebit
		{
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdDebit : this._CuryTranPtdDebit;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdDebit = value;
				}
				else
				{
					this._CuryTranPtdDebit = value;
				}
			}
		}
		#endregion
		#region CuryYtdBalance
		public override Decimal? CuryYtdBalance
		{
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinYtdBalance : this._CuryTranYtdBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinYtdBalance = value;
				}
				else
				{
					this._CuryTranYtdBalance = value;
				}
			}
		}
		#endregion
		#region CuryBegBalance
		public override Decimal? CuryBegBalance
		{
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinBegBalance : this._CuryTranBegBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinBegBalance = value;
				}
				else
				{
					this._CuryTranBegBalance = value;
				}
			}
		}
		#endregion
		#region tstamp
		[AcctHistDBTimestamp]
		public override Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
	}
}
