using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using System.Collections;
using PX.Objects.CM;
using PX.Objects.CS;

namespace PX.Objects.GL
{
	[Serializable]
	public partial class GLConsolRead : GLConsolData 
	{
		#region AccountID
		public abstract class accountID : PX.Data.IBqlField
		{
		}
		protected Int32? _AccountID;
		//[Account(typeof(Account.curyID))]
		//[PXDefault(typeof(Search<Account.accountID, Where<Account.accountCD, Equal<Current<ConsolRead.accountCD>>>>))]
		public virtual Int32? AccountID
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
		public abstract class subID : PX.Data.IBqlField
		{
		}
		protected Int32? _SubID;
		//[SubAccount(typeof(GLTran.accountID))]
		//[PXDefault(typeof(Search<GLTran.subID, Where<GLTran.module, Equal<Current<GLTran.module>>, And<GLTran.batchNbr, Equal<Current<GLTran.batchNbr>>>>>))]
		public virtual Int32? SubID
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
	}

	[PX.Objects.GL.TableAndChartDashboardType]
	public class GLConsolReadMaint : PXGraph<GLConsolReadMaint>
	{
        
        public PXCancel<GLConsolSetup> Cancel;
		[PXFilterable]
		public PXProcessing<GLConsolSetup,
			Where<GLConsolSetup.isActive, Equal<boolTrue>>>
			ConsolSetupRecords;
		public PXSelectOrderBy<GLConsolData,
			OrderBy<Asc<GLConsolData.finPeriodID>>> ConsolRecords;

		protected virtual IEnumerable consolRecords(
			[PXDBString]
			string ledgerCD,
			[PXDBString]
			string branchCD
			)
		{
			Ledger ledger = PXSelect<Ledger,
				Where<Ledger.consolAllowed, Equal<boolTrue>,
				And<Ledger.ledgerCD, Equal<Required<Ledger.ledgerCD>>>>>.Select(this, ledgerCD);

			Branch branch = PXSelect<Branch,
				Where<Branch.branchCD, Equal<Required<Branch.branchCD>>>>.Select(this, branchCD);

			if (ledger == null)
			{
				throw new PXException(Messages.CantFindConsolidationLedger, ledgerCD);
			}
			
			if (!string.IsNullOrEmpty(branchCD) && branch == null)
			{
				throw new PXException(Messages.CantFindConsolidationBranch, branchCD);
			}

			if (true)
			{
				InitSegmentData(null);

				PXSelectBase<GLHistory> cmd = new PXSelectJoin<GLHistory,
					InnerJoin<Account, On<Account.accountID, Equal<GLHistory.accountID>>,
					InnerJoin<Sub, On<Sub.subID, Equal<GLHistory.subID>>,
					InnerJoin<Ledger, On<Ledger.ledgerID, Equal<GLHistory.ledgerID>>,
					InnerJoin<Branch, On<Branch.branchID, Equal<GLHistory.branchID>>>>>>,
					Where<Ledger.ledgerCD, Equal<Required<Ledger.ledgerCD>>,
					And<GLHistory.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>,
					OrderBy<Asc<GLHistory.finPeriodID, Asc<Account.accountCD, Asc<Sub.subCD>>>>>(this);

				if (!string.IsNullOrEmpty(branchCD))
				{
					cmd.WhereAnd<Where<Branch.branchCD, Equal<Required<Branch.branchCD>>>>();
				}

				foreach (PXResult<GLHistory, Account, Sub> result in cmd.Select(ledgerCD, branchCD))
				{
					GLHistory history = result;
					Account account = result;
					Sub sub = result;

					string accountCD = account.GLConsolAccountCD;
					string subCD = GetMappedValue(sub.SubCD);

					if (accountCD != null && accountCD.TrimEnd() != ""
						&& subCD != null && subCD.TrimEnd() != "")
					{
						GLConsolData consolData = new GLConsolData();
						consolData.MappedValue = subCD;
						consolData.AccountCD = accountCD;
						consolData.FinPeriodID = history.FinPeriodID;
						consolData = ConsolRecords.Locate(consolData);
						if (consolData != null)
						{
							consolData.ConsolAmtDebit += history.FinPtdDebit;
							consolData.ConsolAmtCredit += history.FinPtdCredit;
						}
						else
						{
							consolData = new GLConsolData();
							consolData.MappedValue = subCD;
							consolData.AccountCD = accountCD;
							consolData.FinPeriodID = history.FinPeriodID;
							consolData.ConsolAmtDebit = history.FinPtdDebit;
							consolData.ConsolAmtCredit = history.FinPtdCredit;
							ConsolRecords.Insert(consolData);
						}
					}
				}
			}

			return ConsolRecords.Cache.Inserted;
		}

		private short ErrorSegmentId = -1;
		private string ErrorSegmentValue = string.Empty;

		protected virtual string GetMappedValue(string subValue)
		{
			int startIndex = 0;
			string value = string.Empty;
			string result = string.Empty;
			bool thereWasAnError = false;
			ErrorSegmentId = -1;

			for (short segmentId = 1; segmentId <= segmentPairs.Count; segmentId++)
			{
				if (segmentNumChars[segmentId] <= 0) // if Length <= 0
				{
					startIndex += segmentPairs[segmentId];
					continue;
				}

				value = subValue.Substring(startIndex, segmentPairs[segmentId]);
				if (segmentValueTriple.ContainsKey(segmentId))
				{
					if (segmentValueTriple[segmentId].ContainsKey(value))
					{
						string mappedValue = segmentValueTriple[segmentId][value];
						if (mappedValue != null && mappedValue.Length == segmentNumChars[segmentId])
							result += mappedValue;
						else
							thereWasAnError = true;
					}
					else
						thereWasAnError = true;
				}
				else if (value.Length == segmentNumChars[segmentId])
					result += value;
				else
					thereWasAnError = true;

				if (thereWasAnError)
				{
					ErrorSegmentId = segmentId;
					ErrorSegmentValue = value;
					return string.Empty;
				}

				startIndex += segmentPairs[segmentId];
			}

			return result;
		}

		public GLConsolReadMaint()
		{
			GLSetup setup = GLSetup.Current;

			PXCache cache = ConsolSetupRecords.Cache;

			PXUIFieldAttribute.SetEnabled<GLConsolSetup.description>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.lastConsDate>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.lastPostPeriod>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.ledgerId>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.login>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.password>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.pasteFlag>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.segmentValue>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.url>(cache, null, false);

			cache.AllowInsert = false;
			cache.AllowDelete = false;

			ConsolSetupRecords.SetProcessDelegate<GLConsolReadMaint>(ProcessConsolidationRead);
			ConsolSetupRecords.SetProcessAllVisible(false);
		}

		protected static void ProcessConsolidationRead(GLConsolReadMaint processor, GLConsolSetup item)
		{
			processor.Clear();
			processor.listConsolRead.Clear();
			int cnt = processor.ConsolidationRead(item);
			if (cnt > 0)
			{
				throw new PXSetPropertyException(Messages.NumberRecodsProcessed, PXErrorLevel.RowInfo, cnt);
			}
			else
			{
				throw new PXSetPropertyException(Messages.NoRecordsToProcess, PXErrorLevel.RowInfo);
			}
		}

		protected Dictionary<short, short> segmentPairs = new Dictionary<short, short>(); // <segmentid, length>
		protected Dictionary<short, Dictionary<string, string>> segmentValueTriple = new Dictionary<short, Dictionary<string, string>>(); // <segmentid, <value, mapped_value>>
		public List<GLConsolRead> listConsolRead = new List<GLConsolRead>();
		protected List<GLConsolRead> listConsolReadAddition = new List<GLConsolRead>();
		protected GLConsolSetup consolSetup = null;

		protected Dictionary<short, short> segmentNumChars = new Dictionary<short, short>(); // <segmentid, NumChars>
		protected Dictionary<short, string> segmentNames = new Dictionary<short, string>(); // <segmentid, segmentName>

		protected virtual int ConsolidationRead(GLConsolSetup item)
		{
			int cnt = 0;

			string aFiscalPeriod = null;
			int? ledgerID = item.LedgerId;
			int? branchID = item.BranchID;
			string segmentValue = item.SegmentValue;

			InitSegmentData(item);

			PXSelect<GLConsolHistory,
				Where<GLConsolHistory.setupID, Equal<Required<GLConsolHistory.setupID>>>>
				.Select(this, item.SetupID);

			JournalEntry je = PXGraph.CreateInstance<JournalEntry>();
			if (item.BypassAccountSubValidation == true)
			{
				je.FieldVerifying.AddHandler<GLTran.accountID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
				je.FieldVerifying.AddHandler<GLTran.subID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			}

			using (PXSoapScope scope = new PXSoapScope(consolSetup.Url, consolSetup.Login, consolSetup.Password))
			{
				GLConsolReadMaint reader = PXGraph.CreateInstance<GLConsolReadMaint>();
				reader.ConsolRecords.Select(item.SourceLedgerCD, item.SourceBranchCD);
				scope.Process(reader);

				int min = 0;
				if (!String.IsNullOrEmpty(item.StartPeriod))
				{
					int.TryParse(item.StartPeriod, out min);
				}
				int max = 0;
				if (!String.IsNullOrEmpty(item.EndPeriod))
				{
					int.TryParse(item.EndPeriod, out max);
				}
				foreach (GLConsolData row in reader.ConsolRecords.Select())
				{
					if (min > 0 || max > 0)
					{
						if (!String.IsNullOrEmpty(row.FinPeriodID))
						{
							int p;
							if (int.TryParse(row.FinPeriodID, out p))
							{
								if (min > 0 && p < min || max > 0 && p > max)
								{
									continue;
								}
							}
						}
					}
					if (aFiscalPeriod == null)
					{
						aFiscalPeriod = row.FinPeriodID;
					}
					else if (aFiscalPeriod != row.FinPeriodID)
					{
						if (listConsolRead.Count > 0)
						{
							cnt += AppendRemapped(aFiscalPeriod, ledgerID, branchID, item.SetupID);
							CreateBatch(je, aFiscalPeriod, ledgerID, branchID, item);
						}
						aFiscalPeriod = row.FinPeriodID;
						listConsolRead.Clear();
					}

					GLConsolRead read = new GLConsolRead();
					int? accountId = ValidateAccountCD(row.AccountCD);
					string subCD = GetRegeneratedSubCD(row.MappedValue);
					int? subId = ValidateSubCD(subCD);

					read.AccountCD = row.AccountCD;
					read.AccountID = accountId;
					read.MappedValue = subCD;
					read.SubID = subId;
					read.FinPeriodID = row.FinPeriodID;

					GLConsolHistory history = new GLConsolHistory();
					history.SetupID = item.SetupID;
					history.FinPeriodID = read.FinPeriodID;
					history.AccountID = read.AccountID;
					history.SubID = read.SubID;
					history.LedgerID = item.LedgerId;
					history.BranchID = item.BranchID;
					history = (GLConsolHistory)Caches[typeof(GLConsolHistory)].Locate(history);
					if (history != null)
					{
						read.ConsolAmtCredit = (row.ConsolAmtCredit - history.PtdCredit);
						read.ConsolAmtDebit = (row.ConsolAmtDebit - history.PtdDebit);
						history.PtdCredit = 0m;
						history.PtdDebit = 0m;
					}
					else
					{
						read.ConsolAmtCredit = row.ConsolAmtCredit;
						read.ConsolAmtDebit = row.ConsolAmtDebit;
					}
					if (read.ConsolAmtCredit != 0m || read.ConsolAmtDebit != 0m)
					{
						listConsolRead.Add(read);
						cnt++;
					}
				}
			}

			if (listConsolRead.Count > 0)
			{
				cnt += AppendRemapped(aFiscalPeriod, ledgerID, branchID, item.SetupID);
				CreateBatch(je, aFiscalPeriod, ledgerID, branchID, item);
			}

			if (exception != null)
			{
				PXException ex = exception;
				exception = null;
				throw ex;
			}

			return cnt;
		}

		public void InitSegmentData(GLConsolSetup item)
		{
			foreach (SegmentValue segValue in PXSelect<SegmentValue,
				Where<SegmentValue.dimensionID, Equal<Required<SegmentValue.dimensionID>>>>.Select(this, "SUBACCOUNT"))
			{
				if (!segmentValueTriple.ContainsKey(segValue.SegmentID ?? 0))
					segmentValueTriple[segValue.SegmentID ?? 0] = new Dictionary<string, string>();
				segmentValueTriple[segValue.SegmentID ?? 0][segValue.Value] = segValue.MappedSegValue;
			}

			foreach (Segment segDetail in PXSelect<Segment,
				Where<Segment.dimensionID, Equal<Required<Segment.dimensionID>>>>.Select(this, "SUBACCOUNT"))
			{
				segmentPairs[segDetail.SegmentID ?? 0] = segDetail.Length ?? 0;
				segmentNames[segDetail.SegmentID ?? 0] = segDetail.Descr;
				segmentNumChars[segDetail.SegmentID ?? 0] = segDetail.ConsolNumChar ?? 0;
				if (segDetail.Validate != true && segmentValueTriple.ContainsKey(segDetail.SegmentID ?? 0))
					segmentValueTriple.Remove(segDetail.SegmentID ?? 0);
			}

			// Init Segment start index
			short consolSegmentId = GLSetup.Current.ConsolSegmentId ?? 0;

			int startIndex = 0;
			for (short segmentId = 1; segmentId < consolSegmentId; segmentId++)
			{
				startIndex += segmentPairs[segmentId];
			}
			segmentStartIndex = startIndex;

			PXCache cache = this.Caches[typeof(GLConsolSetup)];
			PXDBCryptStringAttribute.SetDecrypted<GLConsolSetup.password>(cache, true);

			if (item != null)
			{
				consolSetup = item;
				consolSetup.Password = cache.GetValueExt<GLConsolSetup.password>(consolSetup).ToString();
				PXDBCryptStringAttribute.SetDecrypted<GLConsolSetup.password>(cache, false);
				segmentInsertData = consolSetup.SegmentValue;
				pasteFlag = consolSetup.PasteFlag ?? false;
			}
		}

		public int? ValidateAccountCD(string accountCD)
		{
			Account account = PXSelect<Account,
				Where<Account.accountCD, Equal<Required<Account.accountCD>>>>.
				Select(this, accountCD);

			return account.AccountID;
		}

		public int? ValidateSubCD(string subCD)
		{
			System.Diagnostics.Debug.WriteLine(subCD);
			Sub sub = PXSelect<Sub,
				Where<Sub.subCD, Equal<Required<Sub.subCD>>>>.
				Select(this, subCD);
			
			if (sub == null)
				return null;

			return sub.SubID;
		}

		public PXSetup<GLSetup> GLSetup;

		private int segmentStartIndex = 0;
		private string segmentInsertData = string.Empty;
		private bool pasteFlag = false;
		public virtual string GetRegeneratedSubCD(string mappedValue)
		{
			if (pasteFlag)
				return mappedValue.Insert(segmentStartIndex, segmentInsertData);
			else
				return mappedValue;
		}

		public int AppendRemapped(string periodId, int? ledgerID, int? branchID, int? setupID)
		{
			int ret = 0;
			foreach (GLConsolHistory history in Caches[typeof(GLConsolHistory)].Cached)
			{
				if (history.SetupID == setupID
					&& history.FinPeriodID == periodId
					&& history.LedgerID == ledgerID
					&& history.BranchID == branchID
					&& (history.PtdCredit != 0m || history.PtdDebit != 0m))
				{
					GLConsolRead read = new GLConsolRead();
					read.AccountID = history.AccountID;
					read.SubID = history.SubID;
					read.FinPeriodID = history.FinPeriodID;
					read.ConsolAmtCredit = -history.PtdCredit;
					read.ConsolAmtDebit = -history.PtdDebit;
					listConsolRead.Add(read);
					ret++;
				}
			}
			return ret;
		}

		public void CreateBatch(JournalEntry je, string periodId, int? ledgerID, int? branchID, GLConsolSetup item)
		{
			je.Clear();

			je.glsetup.Current.RequireControlTotal = false;

			Ledger ledger = PXSelect<Ledger,
				Where<Ledger.ledgerID, Equal<Required<Ledger.ledgerID>>>>
				.Select(this, ledgerID);

			Branch branch = PXSelect<Branch,
				Where<Branch.branchID, Equal<Required<Branch.branchID>>>>
				.Select(this, branchID);

			FinPeriod fiscalPeriod = PXSelect<FinPeriod,
				Where<FinPeriod.finPeriodID, Equal<Required<FinPeriod.finPeriodID>>>>.
				Select(this, periodId);

			if (fiscalPeriod == null)
			{
				throw new FiscalPeriodInvalidException(periodId);
			}

			je.Accessinfo.BusinessDate = fiscalPeriod.EndDate.Value.AddDays(-1);

			CurrencyInfo info = new CurrencyInfo();
			info.CuryID = ledger.BaseCuryID;
			info.CuryEffDate = fiscalPeriod.EndDate.Value.AddDays(-1);
			info.CuryRate = (decimal)1.0;
			info = je.currencyinfo.Insert(info);

			Batch batch = new Batch();
			batch.TranPeriodID = periodId;
			batch.BranchID = branchID;
			batch.LedgerID = ledgerID;
			batch.Module = BatchModule.GL;
			batch.Hold = false;
			batch.Released = false;
			batch.CuryID = ledger.BaseCuryID;
			batch.CuryInfoID = info.CuryInfoID;
			batch.FinPeriodID = periodId;
			batch.CuryID = ledger.BaseCuryID;
			batch.BatchType = "C";
			batch.Description = PXMessages.LocalizeFormatNoPrefix(Messages.ConsolidationBatch, item.Description);
			batch = je.BatchModule.Insert(batch);

			foreach (GLConsolRead read in listConsolRead)
			{
				if (Math.Abs((decimal)read.ConsolAmtDebit) > 0)
				{
					GLTran tran = new GLTran();

					tran.AccountID = read.AccountID;
					tran.SubID = read.SubID;
					tran.CuryInfoID = info.CuryInfoID;
					tran.CuryCreditAmt = (decimal)0.0;
					tran.CuryDebitAmt = read.ConsolAmtDebit;
					tran.CreditAmt = (decimal)0.0;
					tran.DebitAmt = read.ConsolAmtDebit;
					tran.TranType = "CON";
					tran.TranClass = "C";
					tran.TranDate = fiscalPeriod.EndDate.Value.AddDays(-1);
					tran.TranDesc = Messages.ConsolidationDetail;
					tran.TranPeriodID = periodId;
					tran.RefNbr = "";
					tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(this);
					tran = je.GLTranModuleBatNbr.Insert(tran);

					if (tran.SubID == null && read.MappedValue != null)
					{
						je.GLTranModuleBatNbr.SetValueExt<GLTran.subID>(tran, read.MappedValue);
					}

					if (tran.AccountID == null || tran.SubID == null)
					{
						throw new PXException(Messages.AccountOrSubNotFound, read.AccountCD, read.MappedValue);
					}
				}

				if (Math.Abs((decimal)read.ConsolAmtCredit) > 0)
				{
					GLTran tran = new GLTran();

					tran.AccountID = read.AccountID;
					tran.SubID = read.SubID;
					tran.CuryInfoID = info.CuryInfoID;
					tran.CuryCreditAmt = read.ConsolAmtCredit;
					tran.CuryDebitAmt = (decimal)0.0;
					tran.CreditAmt = read.ConsolAmtCredit;
					tran.DebitAmt = (decimal)0.0;
					tran.TranType = "CON";
					tran.TranClass = "C";
					tran.TranDate = fiscalPeriod.EndDate.Value.AddDays(-1);
					tran.TranDesc = Messages.ConsolidationDetail;
					tran.TranPeriodID = periodId;
					tran.RefNbr = "";
					tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(this);
					tran = je.GLTranModuleBatNbr.Insert(tran);

					if (tran.SubID == null && read.MappedValue != null)
					{
						je.GLTranModuleBatNbr.SetValueExt<GLTran.subID>(tran, read.MappedValue);
					}

					if (tran.AccountID == null || tran.SubID == null)
					{
						throw new PXException(Messages.AccountOrSubNotFound, read.AccountCD, read.MappedValue);
					}
				}
			}

			item.LastPostPeriod = periodId;
			item.LastConsDate = DateTime.Now;
			je.Caches[typeof(GLConsolSetup)].Update(item);
			if (!je.Views.Caches.Contains(typeof(GLConsolSetup)))
			{
				je.Views.Caches.Add(typeof(GLConsolSetup));
			}
			GLConsolBatch cb = new GLConsolBatch();
			cb.SetupID = item.SetupID;
			je.Caches[typeof(GLConsolBatch)].Insert(cb);
			if (!je.Views.Caches.Contains(typeof(GLConsolBatch)))
			{
				je.Views.Caches.Add(typeof(GLConsolBatch));
			}

			try
			{
				je.Save.Press();
			}
			catch (PXException e)
			{
				try
				{
					if (!String.IsNullOrEmpty(PXUIFieldAttribute.GetError<Batch.curyCreditTotal>(je.BatchModule.Cache, je.BatchModule.Current))
						|| !String.IsNullOrEmpty(PXUIFieldAttribute.GetError<Batch.curyDebitTotal>(je.BatchModule.Cache, je.BatchModule.Current)))
					{
						je.BatchModule.Current.Hold = true;
						je.BatchModule.Update(je.BatchModule.Current);
					}
					je.Save.Press();
					if (exception == null)
					{
						exception = new PXException(Messages.ConsolidationBatchOutOfBalance, je.BatchModule.Current.BatchNbr);
					}
					else
					{
						exception = new PXException(exception.Message + Messages.ConsolidationBatchOutOfBalance, je.BatchModule.Current.BatchNbr);
					}
				}
				catch
				{
					throw e;
				}
			}
		}

		public PXException exception;
	}
}
