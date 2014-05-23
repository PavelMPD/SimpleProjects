using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using AccountTypeList = PX.Objects.GL.AccountType;

namespace PX.Objects.GL
{
	public class JournalEntryImport : PXGraph<JournalEntryImport, GLTrialBalanceImportMap>, PXImportAttribute.IPXPrepareItems
	{	
		#region OperationParam

		[Serializable]
		public partial class OperationParam : IBqlTable
		{
			public abstract class action : IBqlField { }

			protected String _Action;
			[PXDefault(_VALIDATE_ACTION)]
			public String Action
			{
				get { return _Action; }
				set { _Action = value; }
			}
		}

		#endregion

		#region TrialBalanceTemplate
        //Alias
		[Serializable]
		public partial class TrialBalanceTemplate : GLTrialBalanceImportDetails
		{
			public new abstract class mapNumber : IBqlField { }
			public new abstract class line : IBqlField { }
			public new abstract class importAccountCD : IBqlField { }
			public new abstract class mapAccountID : IBqlField { }
			public new abstract class importSubAccountCD : IBqlField { }
			public new abstract class mapSubAccountID : IBqlField { }
			public new abstract class selected : IBqlField { }
			public new abstract class status : IBqlField { }
			public new abstract class displayStatus : IBqlField { }
			public new abstract class description : IBqlField { }
		}

		#endregion

		#region GLHistoryEnquiryWithSubResult

		[Serializable]
		public partial class GLHistoryEnquiryWithSubResult : GLHistoryEnquiryResult
		{
			public new abstract class accountID : IBqlField { }

			#region SubID
			public abstract class subID : IBqlField { }

			protected Int32? _SubID;
			[SubAccount(typeof(accountID))]
			public virtual Int32? SubID
			{
				get { return _SubID; }
				set { _SubID = value; }
			}
			#endregion

			#region AccountType
			public abstract class accountType : IBqlField { }

			protected String _AccountType;
			[PXDBString(1)]
			[PXDefault(AccountTypeList.Asset)]
			[AccountTypeList.List()]
			[PXUIField(DisplayName = "Account Type")]
			public virtual String AccountType
			{
				get { return _AccountType; }
				set { _AccountType = value; }
			}

			#endregion
		}

		#endregion

		#region JournalEntryImportProcessing

		public class JournalEntryImportProcessing : PXGraph<JournalEntryImportProcessing>
		{
			#region Fields

			public PXSetup<Company> CompanySetup;

			public PXSetup<GLSetup> GLSetup;

			public PXSelect<Batch> Batch;

			public PXSelect<GLTran, Where<GLTran.batchNbr, Equal<Current<Batch.batchNbr>>>> GLTrans;

			public PXSelect<GLTrialBalanceImportMap> Map;

			public PXSelect<GLTrialBalanceImportDetails,
				Where<GLTrialBalanceImportDetails.mapNumber, Equal<Current<GLTrialBalanceImportMap.number>>>> MapDetails;

			#endregion

			#region ReleaseImport

			public static void ReleaseImport(object mapNumber, bool isReversedSign)
			{
				var graph = (JournalEntryImportProcessing)PXGraph.CreateInstance(typeof(JournalEntryImportProcessing));

				GLTrialBalanceImportMap map = graph.Map.Search<GLTrialBalanceImportMap.number>(mapNumber);
				if (map == null) return;

				Batch newBatch = new Batch();
				graph.Map.Current = map;
				using (new PXConnectionScope())
				{
					using (var ts = new PXTransactionScope())
					{
						List<GLTrialBalanceImportDetails> details = new List<GLTrialBalanceImportDetails>();
						foreach (GLTrialBalanceImportDetails item in graph.MapDetails.Select())
							details.Add(item);
						var refNumber = _TRAN_REFNUMBER_PREFIX + mapNumber;

						newBatch.BranchID = map.BranchID;
						newBatch = (Batch)graph.Batch.Cache.Insert(newBatch);
						newBatch.Description = map.Description;
						newBatch.DebitTotal = 0m;
						newBatch.CreditTotal = 0m;
						newBatch.BatchType = BatchTypeCode.TrialBalance;

						string baseCurrency = graph.CompanySetup.Current.BaseCuryID;

						foreach (var item in
							GetBalances(graph, isReversedSign, map.BranchID, map.LedgerID,
							            map.FinPeriodID, map.BegFinPeriod, baseCurrency))
						{
							GLTrialBalanceImportDetails importItem = null;
							for (int index = 0; index < details.Count; index++)
							{
								GLTrialBalanceImportDetails detail = details[index];
								if (detail.MapAccountID == item.AccountID && detail.MapSubAccountID == item.SubID)
								{
									importItem = detail;
									details.RemoveAt(index);
									break;
								}
							}
							decimal diff = (importItem == null ? 0m : (decimal)importItem.YtdBalance) - (decimal)item.EndBalance;
							decimal curyDiff = (importItem == null ? 0m : (decimal)importItem.CuryYtdBalance) - (decimal)item.CuryEndBalance;
							if (diff == 0m) continue;
							graph.GLTrans.Cache.Insert();
							GLTran tran = graph.GLTrans.Current;
							tran.AccountID = item.AccountID;
							tran.SubID = item.SubID;
							FillDebitAndCreditAmt(tran, diff, curyDiff, isReversedSign, item.AccountType);
							tran.Released = true;
							tran.RefNbr = refNumber;
							tran.CuryInfoID = SetCuryID(GetAccount(graph, tran.AccountID));
							graph.GLTrans.Update(tran);
						}
						foreach (var item in details)
						{
							decimal diff = (decimal)item.YtdBalance;
							decimal curyDiff = (decimal)item.CuryYtdBalance;
							if (diff == 0m) continue;
							graph.GLTrans.Cache.Insert();
							GLTran tran = graph.GLTrans.Current;
							tran.AccountID = item.MapAccountID;
							tran.SubID = item.MapSubAccountID;
							FillDebitAndCreditAmt(tran, diff, curyDiff, isReversedSign, GetAccount(graph, item.MapAccountID).Type);
							tran.Released = true;
							tran.RefNbr = refNumber;
							tran.CuryInfoID = SetCuryID(GetAccount(graph, tran.AccountID));
							graph.GLTrans.Update(tran);
						}
						newBatch.ControlTotal = (newBatch.DebitTotal == newBatch.CreditTotal) ? newBatch.DebitTotal : 0m;
						graph.Batch.Update(newBatch);

						map.Status = TrialBalanceImportMapStatusAttribute.RELEASED;
						graph.Map.Update(map);
						graph.Actions.PressSave();

						ts.Complete();
					}
				}
				
				using (new PXTimeStampScope(null))
				{
					graph.Clear();
					newBatch = graph.Batch.Search<Batch.batchNbr, Batch.module>(newBatch.BatchNbr, newBatch.Module);
					PXRedirectHelper.TryRedirect(graph.Batch.Cache, newBatch, Messages.ViewBatch);
				}
			}

			#endregion

			private static Int64? SetCuryID(Account account)
			{
				var currencyInfoGraph = (CurrencyInfoProcessing)PXGraph.CreateInstance(typeof(CurrencyInfoProcessing));
				if (account != null)
				{
					PX.Objects.CM.CurrencyInfo ci = new PX.Objects.CM.CurrencyInfo();
					ci.CuryID = account.CuryID;
					ci.BaseCalc = false;
					ci.CuryRate = 1;
					ci.RecipRate = 1;
					ci = currencyInfoGraph.currencyInfo.Insert(ci);
					currencyInfoGraph.Persist();
					return ci.CuryInfoID;
				}
				return null;
			}

			#region Batch

			protected virtual void Batch_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
			{
				if (Map.Current != null) Map.Current.BatchNbr = ((Batch)e.Row).BatchNbr;
			}

			protected virtual void Batch_Module_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
			{
				e.NewValue = BatchModule.GL;
			}

			protected virtual void Batch_Status_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
			{
				e.NewValue = BatchStatus.Balanced;
			}

			protected virtual void Batch_DateEntered_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
			{
				if (Map.Current != null) e.NewValue = Map.Current.ImportDate;
			}

			protected virtual void Batch_FinPeriodID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
			{
				if (Map.Current != null) e.NewValue = Map.Current.FinPeriodID;
				e.Cancel = true;
			}

			protected virtual void Batch_LedgerID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
			{
				if (Map.Current != null) e.NewValue = GetLedger(cache.Graph, Map.Current.LedgerID).LedgerCD;
			}

			#endregion
		}

		public class CurrencyInfoProcessing : PXGraph<CurrencyInfoProcessing>
		{
			public PXSelect<PX.Objects.CM.CurrencyInfo> currencyInfo;
		}

		#endregion

		#region Fields

		#region Constants

		private const string _VALIDATE_ACTION = "Validate";
		private const string _MERGE_DUPLICATES_ACTION = "Merge Duplicates";
		private const string _IMPORTTEMPLATE_VIEWNAME = "ImportTemplate";
		private const string _TRIALBALANCE_SIGN_STATUS_NORMAL = "N";
		private const string _TRAN_REFNUMBER_PREFIX = "";

		#endregion

		private readonly string _mapNumberFieldName;

		[PXHidden]
		public PXSetup<GLSetup> GLSetup;

		[PXHidden]
		public PXFilter<OperationParam> Operations;

		public PXSelect<GLTrialBalanceImportMap> Map;

		[PXFilterable]
		public PXSelect<GLTrialBalanceImportDetails,
			Where<GLTrialBalanceImportDetails.mapNumber, Equal<Current<GLTrialBalanceImportMap.number>>>> MapDetails;

		[PXImport(typeof(GLTrialBalanceImportMap))]
		public PXSelect<TrialBalanceTemplate> ImportTemplate;

		[PXFilterable]
        [PXCopyPasteHiddenView]
		public PXSelectOrderBy<GLHistoryEnquiryWithSubResult,
			OrderBy<Asc<GLHistoryEnquiryWithSubResult.accountID, Asc<GLHistoryEnquiryWithSubResult.subID>>>> Exceptions;

		#endregion

		#region Ctors

		public JournalEntryImport()
		{
			var setup = GLSetup.Current; //NOTE: check setup

			_mapNumberFieldName = ImportTemplate.Cache.GetField(typeof(TrialBalanceTemplate.mapNumber));

			MapDetails.Cache.AllowInsert = true;
			MapDetails.Cache.AllowUpdate = true;
			MapDetails.Cache.AllowDelete = true;
			PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportDetails.selected>(MapDetails.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportDetails.importAccountCD>(MapDetails.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportDetails.importSubAccountCD>(MapDetails.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportDetails.ytdBalance>(MapDetails.Cache, null, true);
			PXUIFieldAttribute.SetReadOnly<GLTrialBalanceImportDetails.status>(MapDetails.Cache, null);
			PXUIFieldAttribute.SetReadOnly<GLTrialBalanceImportDetails.displayStatus>(MapDetails.Cache, null);

			PXUIFieldAttribute.SetEnabled<TrialBalanceTemplate.mapNumber>(ImportTemplate.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<TrialBalanceTemplate.mapAccountID>(ImportTemplate.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<TrialBalanceTemplate.mapSubAccountID>(ImportTemplate.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<TrialBalanceTemplate.selected>(ImportTemplate.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<TrialBalanceTemplate.status>(ImportTemplate.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<TrialBalanceTemplate.displayStatus>(ImportTemplate.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<TrialBalanceTemplate.description>(ImportTemplate.Cache, null, false);
			PXUIFieldAttribute.SetDisplayName<TrialBalanceTemplate.importAccountCD>(ImportTemplate.Cache, "Account");
			PXUIFieldAttribute.SetDisplayName<TrialBalanceTemplate.importSubAccountCD>(ImportTemplate.Cache, "Subaccount");
		}

		#endregion

		#region Actions

		public PXAction<GLTrialBalanceImportMap> process;
		[PXUIField(DisplayName = Messages.Process)]
        [PXButton]
		protected virtual IEnumerable Process(PXAdapter adapter)
		{
			if (CanEdit)
			{
				ICollection<GLTrialBalanceImportDetails> list = new List<GLTrialBalanceImportDetails>();
				foreach (GLTrialBalanceImportDetails item in MapDetails.Select(Operations.Current.Action))
					if (item.Selected == true) list.Add(item);
				ProcessHandler(list);
			}
			return adapter.Get();
		}

		public PXAction<GLTrialBalanceImportMap> processAll;
		[PXUIField(DisplayName = Messages.ProcessAll)]
        [PXButton]
        protected virtual IEnumerable ProcessAll(PXAdapter adapter)
		{
			if (CanEdit)
			{
				ICollection<GLTrialBalanceImportDetails> list = new List<GLTrialBalanceImportDetails>();
				foreach (GLTrialBalanceImportDetails item in MapDetails.Select(Operations.Current.Action))
				{
					item.Selected = true;
					list.Add(item);
				}
				ProcessHandler(list);
			}
			return adapter.Get();
		}

		public PXAction<GLTrialBalanceImportMap> release;
		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			GLTrialBalanceImportMap map = Map.Current;
			if (map != null)
			{
				if (map.Status != TrialBalanceImportMapStatusAttribute.BALANCED)
					throw new PXException(Messages.ImportStatusInvalid);

				if (map.CreditTotalBalance != map.TotalBalance)
					throw new Exception(Messages.FailedControlTotalBalance);
				if (map.DebitTotalBalance != map.TotalBalance)
					throw new Exception(Messages.FailedControlTotalBalance);

				Save.Press();
				bool isUnsignOperations = IsUnsignOperations(this);
				object mapNumber = Map.Current.Number;
				PXLongOperation.StartOperation(this,
					delegate()
					{
						JournalEntryImportProcessing.ReleaseImport(mapNumber, isUnsignOperations);
					});
			}

			yield return Map.Current;
		}

		#endregion

		#region Select Handlers

		protected virtual void mapDetails([PXString] ref string action)
		{
			if (action != null) Operations.Current.Action = action;
		}

		protected virtual IEnumerable exceptions()
		{
			if (Map.Current == null) yield break;

			foreach (GLHistoryEnquiryWithSubResult item in
				GetBalances(this, Map.Current.BranchID, Map.Current.LedgerID, Map.Current.FinPeriodID, Map.Current.BegFinPeriod, null))
			{
				if (item.EndBalance == 0m) continue;

				if (PXSelect<GLTrialBalanceImportDetails,
					Where<GLTrialBalanceImportDetails.mapNumber, Equal<Required<GLTrialBalanceImportDetails.mapNumber>>,
						And<GLTrialBalanceImportDetails.mapAccountID, Equal<Required<GLTrialBalanceImportDetails.mapAccountID>>,
						And<GLTrialBalanceImportDetails.mapSubAccountID, Equal<Required<GLTrialBalanceImportDetails.mapSubAccountID>>>>>>.
					Select(this, Map.Current.Number, item.AccountID, item.SubID).Count > 0) continue;
				yield return item;
			}
		}

		#endregion

		#region Processing

		protected virtual void ProcessHandler(ICollection<GLTrialBalanceImportDetails> list)
		{
			if (Operations.Current == null) return;

			switch (Operations.Current.Action)
			{
				case _VALIDATE_ACTION:
					foreach (GLTrialBalanceImportDetails item in list)
					{
						bool validateSubAccountCD = SetValue(MapDetails.Cache, item, "ImportAccountCD", "MapAccountID",
															 Messages.ImportAccountCDNotFound, Messages.ImportAccountCDIsEmpty);
						if (!validateSubAccountCD)
						{
							item.MapSubAccountID = null;
							PersistErrorAttribute.ClearError(MapDetails.Cache, item, "ImportSubAccountCD");
						}
						else if (PXAccess.FeatureInstalled<CS.FeaturesSet.subAccount>() == true)
						{
							SetValue(MapDetails.Cache, item, "ImportSubAccountCD", "MapSubAccountID",
									  null, Messages.ImportSubAccountCDIsEmpty);
						}
						item.Status = TrialBalanceImportStatusAttribute.VALID;
					}
					foreach (GLTrialBalanceImportDetails item in list)
					{
						PXResultset<GLTrialBalanceImportDetails> duplicates1 = SearchDuplicates(item);
						if (duplicates1.Count < 2) continue;
						foreach (GLTrialBalanceImportDetails duplicate in duplicates1)
							if (duplicate.Status != TrialBalanceImportStatusAttribute.ERROR)
								duplicate.Status = TrialBalanceImportStatusAttribute.DUPLICATE;
					}
					foreach (GLTrialBalanceImportDetails item in list)
						MapDetails.Cache.Update(item);
					break;
				case _MERGE_DUPLICATES_ACTION:
					List<GLTrialBalanceImportDetails> revalidateItems = new List<GLTrialBalanceImportDetails>();
					foreach (GLTrialBalanceImportDetails item in list)
					{
						PXEntryStatus itemStatus = MapDetails.Cache.GetStatus(item);
						if (itemStatus != PXEntryStatus.Deleted && itemStatus != PXEntryStatus.InsertedDeleted)
						{
							foreach (GLTrialBalanceImportDetails duplicate in SearchDuplicates(item))
								if (duplicate.Line != item.Line)
								{
									if (list.Contains(duplicate))
									{
										item.YtdBalance += duplicate.YtdBalance;
                                        item.CuryYtdBalance += duplicate.CuryYtdBalance;
										MapDetails.Cache.Delete(duplicate);
									}
									else revalidateItems.Add(duplicate);
								}
							if (item.Status != TrialBalanceImportStatusAttribute.ERROR)
							{
								bool accountCDNotValidated = !string.IsNullOrEmpty(item.ImportAccountCD) &&
															 item.MapAccountID == null;
								bool subAccountCDNotValidated = !string.IsNullOrEmpty(item.ImportSubAccountCD) &&
																item.MapSubAccountID == null;

								item.Status = (accountCDNotValidated || subAccountCDNotValidated) ?
									TrialBalanceImportStatusAttribute.NEW : TrialBalanceImportStatusAttribute.VALID;
							}
							revalidateItems.Add(item);
						}
					}
					foreach (GLTrialBalanceImportDetails item in revalidateItems)
					{
						PXResultset<GLTrialBalanceImportDetails> duplicates = SearchDuplicates(item, revalidateItems);
						if (duplicates.Count < 2)
						{
							MapDetails.Cache.Update(item);
							continue;
						}
						foreach (GLTrialBalanceImportDetails duplicate in duplicates)
							if (duplicate.Status != TrialBalanceImportStatusAttribute.ERROR)
							{
								duplicate.Status = TrialBalanceImportStatusAttribute.DUPLICATE;
								MapDetails.Cache.Update(item);
							}
					}
					break;
			}
		}

		#endregion

		#region Event Handlers

		#region GLTrialBalanceImportMap

        protected virtual void GLTrialBalanceImportMap_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            if (e.Row == null) return;

            GLTrialBalanceImportMap row = (GLTrialBalanceImportMap)e.Row;
            bool isEditable = IsEditable(row);

            row.CreditTotalBalance = 0m;
            row.DebitTotalBalance = 0m;
            bool isAllMapped = true;
            bool isReversedSign = IsUnsignOperations(this);
            if (!this.IsImport)
                foreach (GLTrialBalanceImportDetails item in GetImportDetails(this, row.Number))
                {
                    if (item.YtdBalance != null && item.MapAccountID != null)
                    {
                        Account account = GetAccount(this, item.MapAccountID);
                        if (account != null)
                            switch (account.Type)
                            {
                                case AccountType.Asset:
                                case AccountType.Expense:
                                    row.DebitTotalBalance += item.YtdBalance;
                                    break;
                                case AccountType.Liability:
                                case AccountType.Income:
                                    if (isReversedSign)
                                        row.CreditTotalBalance -= item.YtdBalance;
                                    else row.CreditTotalBalance += item.YtdBalance;
                                    break;
                            }
                    }
                    isAllMapped &= item.MapAccountID != null && item.MapSubAccountID != null && item.YtdBalance != null;
                }
            if (isEditable)
            {
                int newStatus = TrialBalanceImportMapStatusAttribute.HOLD;
                bool creditIncorrect = false;
                bool debitIncorrect = false;
                if (row.IsHold != true)
                {
                    creditIncorrect = row.CreditTotalBalance != row.TotalBalance;
                    debitIncorrect = row.DebitTotalBalance != row.TotalBalance;

                    if (creditIncorrect)
                        PXUIFieldAttribute.SetError<GLTrialBalanceImportMap.creditTotalBalance>(sender, row,
                            Messages.FailedControlTotalBalance, row.CreditTotalBalance.ToString());
                    if (debitIncorrect)
                        PXUIFieldAttribute.SetError<GLTrialBalanceImportMap.debitTotalBalance>(sender, row,
                            Messages.FailedControlTotalBalance, row.DebitTotalBalance.ToString());
                    if (!creditIncorrect && !debitIncorrect && isAllMapped)
                        newStatus = TrialBalanceImportMapStatusAttribute.BALANCED;
                }
                if (!creditIncorrect)
                    PXUIFieldAttribute.SetError<GLTrialBalanceImportMap.creditTotalBalance>(sender, row, null);
                if (!debitIncorrect)
                    PXUIFieldAttribute.SetError<GLTrialBalanceImportMap.debitTotalBalance>(sender, row, null);
                row.Status = newStatus;
            }
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportMap.totalBalance>(sender, row, isEditable);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportMap.importDate>(sender, row, isEditable);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportMap.finPeriodID>(sender, row, isEditable);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportMap.description>(sender, row, isEditable);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportMap.ledgerID>(sender, row, isEditable);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportMap.branchID>(sender, row, isEditable);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportMap.isHold>(sender, row, isEditable);
            Map.Cache.AllowDelete = isEditable;
            Map.Cache.AllowUpdate = isEditable;
            MapDetails.Cache.AllowInsert = isEditable;
            MapDetails.Cache.AllowUpdate = isEditable;
            MapDetails.Cache.AllowDelete = isEditable;
            Actions["Release"].SetEnabled(row.Status == TrialBalanceImportMapStatusAttribute.BALANCED);
            Actions["Process"].SetEnabled(isEditable);
            Actions["ProcessAll"].SetEnabled(isEditable);
            PXImportAttribute.SetEnabled(this, "ImportTemplate", isEditable);
        }

        protected virtual void GLTrialBalanceImportMap_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            sender.SetDefaultExt<GLTrialBalanceImportMap.ledgerID>(e.Row);
        }

		#endregion

		#region TrialBalanceTemplate

		protected virtual void TrialBalanceTemplate_ImportAccountCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void TrialBalanceTemplate_ImportSubAccountCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void TrialBalanceTemplate_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			//MapDetails.Update(ConvertToImportDetails(sender, (TrialBalanceTemplate)e.Row));
			MapDetails.Update((TrialBalanceTemplate)e.Row);
		}

		protected virtual void TrialBalanceTemplate_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			//MapDetails.Update(ConvertToImportDetails(sender, (TrialBalanceTemplate)e.Row));
			MapDetails.Update((TrialBalanceTemplate)e.Row);
		}

		protected virtual void TrialBalanceTemplate_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		#endregion

		#region GLTrialBalanceImportDetails


		protected virtual void GLTrialBalanceImportDetails_ImportAccountCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void GLTrialBalanceImportDetails_ImportSubAccountCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void GLTrialBalanceImportDetails_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			GLTrialBalanceImportDetails oldRow = (GLTrialBalanceImportDetails) e.OldRow;
			GLTrialBalanceImportDetails row = (GLTrialBalanceImportDetails) e.Row;
			if (row.ImportSubAccountCD != oldRow.ImportSubAccountCD)
			{
				row.ImportSubAccountCDError = null;
				row.MapSubAccountID = null;
			}
			if (row.ImportAccountCD != oldRow.ImportAccountCD)
			{
				row.ImportAccountCDError = null;
				row.MapAccountID = null;
				row.ImportSubAccountCDError = null;
				row.MapSubAccountID = null;
			}
			CheckMappingAndBalance(sender, row);
		}

		protected virtual void GLTrialBalanceImportDetails_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			GLTrialBalanceImportDetails row = (GLTrialBalanceImportDetails)e.Row;
			if (row == null) return;

			Account account;
			if (row.MapAccountID != null && (account = GetAccount(sender.Graph, row.MapAccountID)) != null)
			{
				row.Description = account.Description;
				row.AccountType = account.Type;
			}
			else
			{
				row.Description = null;
				row.AccountType = null;
			}
			CheckMappingAndBalance(sender, row);
		}

		protected virtual void GLTrialBalanceImportDetails_MapSubAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var row = e.Row as GLTrialBalanceImportDetails;
			if (row == null || row.MapAccountID == null || e.NewValue == null) return;
            Account acc = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(sender.Graph, row.MapAccountID);
			if (acc.IsCashAccount != true)
            {
                return;
            }
            CA.CashAccount cashAccount = PXSelect<CA.CashAccount, Where<CA.CashAccount.accountID, Equal<Required<CA.CashAccount.accountID>>,
                And<CA.CashAccount.subID, Equal<Required<CA.CashAccount.subID>>>>>.Select(sender.Graph, row.MapAccountID, (int?)e.NewValue);
			if (cashAccount == null)
			{
				throw new PXSetPropertyException(Messages.InvalidCashAccountSub);
			}
		}

        /*protected virtual void GLTrialBalanceImportDetails_Line_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var row = e.Row as GLTrialBalanceImportDetails;
            if (row == null) return;

            var lastDetail = (GLTrialBalanceImportDetails)PXSelect<GLTrialBalanceImportDetails,
                Where<GLTrialBalanceImportDetails.mapNumber, Equal<Required<GLTrialBalanceImportDetails.mapNumber>>>,
                OrderBy<Desc<GLTrialBalanceImportDetails.line>>>.
                Select(this, row.MapNumber);
            var newLineNbr = lastDetail != null && lastDetail.Line != null ? (int)lastDetail.Line + 1 : 0;
            if (e.NewValue == null || newLineNbr > (int)e.NewValue)
                e.NewValue = newLineNbr;
        }*/

		#endregion

		#endregion

		#region Private Methods

		private static PXResultset<GLTrialBalanceImportDetails> SearchDuplicates(GLTrialBalanceImportDetails item, IEnumerable<GLTrialBalanceImportDetails> list)
		{
			PXResultset<GLTrialBalanceImportDetails> resultset = new PXResultset<GLTrialBalanceImportDetails>();
			foreach (GLTrialBalanceImportDetails listItem in list)
				if (listItem.MapNumber == item.MapNumber && listItem.ImportAccountCD == item.ImportAccountCD &&
					listItem.ImportSubAccountCD == item.ImportSubAccountCD)
					resultset.Add(new PXResult<GLTrialBalanceImportDetails>(listItem));
			return resultset;
		}

		private PXResultset<GLTrialBalanceImportDetails> SearchDuplicates(GLTrialBalanceImportDetails item)
		{
			return PXSelect<GLTrialBalanceImportDetails,
				Where<GLTrialBalanceImportDetails.mapNumber, Equal<Required<GLTrialBalanceImportDetails.mapNumber>>,
					And<GLTrialBalanceImportDetails.importAccountCD, Equal<Required<GLTrialBalanceImportDetails.importAccountCD>>,
						And<GLTrialBalanceImportDetails.importSubAccountCD, Equal<Required<GLTrialBalanceImportDetails.importSubAccountCD>>>>>>.
				Select(this, item.MapNumber, item.ImportAccountCD, item.ImportSubAccountCD);
		}

		private static bool SetValue(PXCache cache, GLTrialBalanceImportDetails item, string sourceFieldName, string fieldName, string alternativeError, string emptyError)
		{
			string error = null;
			PXUIFieldAttribute.SetError(cache, item, fieldName, null);
			object value = cache.GetValue(item, sourceFieldName);

			if (value == null || value is string && string.IsNullOrEmpty(value.ToString())) error = emptyError;
			else
				try
				{
					cache.SetValueExt(item, fieldName, value);
				}
				catch (PXSetPropertyException e)
				{
					error = e.Message;
				}
				finally
				{
					if (error == null) error = PXUIFieldAttribute.GetError(cache, item, fieldName);
				}

			if (!string.IsNullOrEmpty(error))
			{
				PersistErrorAttribute.SetError(cache, item, sourceFieldName,
					(error != emptyError && alternativeError != null) ? alternativeError : error);
				return false;
			}
			PersistErrorAttribute.ClearError(cache, item, sourceFieldName);
			return true;
		}

		private static bool IsUnsignOperations(JournalEntryImport graph)
		{
			return graph.GLSetup.Current.TrialBalanceSign != _TRIALBALANCE_SIGN_STATUS_NORMAL;
		}

		private static PXResultset<GLTrialBalanceImportDetails> GetImportDetails(PXGraph graph, string mapNumber)
		{
			return PXSelect<GLTrialBalanceImportDetails,
				Where<GLTrialBalanceImportDetails.mapNumber, Equal<Required<GLTrialBalanceImportDetails.mapNumber>>>>.
				Select(graph, mapNumber);
		}

		private static Account GetAccount(PXGraph graph, object accountID)
		{
			PXResultset<Account> result =
				PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.
				Select(graph, accountID);
			return result.Count > 0 ? (Account)result[0] : null;
		}

		private static Ledger GetLedger(PXGraph graph, int? ledgerID)
		{
			PXResultset<Ledger> resultset = PXSelect<Ledger,
				Where<Ledger.ledgerID, Equal<Required<Ledger.ledgerID>>>>.Select(graph, ledgerID);
			return resultset.Count > 0 ? resultset[0] : null;
		}

		private bool CanEdit
		{
			get
			{
				return Map.Current != null && IsEditable(Map.Current);
			}
		}

		private static bool IsEditable(GLTrialBalanceImportMap map)
		{
			return map.Status != TrialBalanceImportMapStatusAttribute.RELEASED;
		}

		private static void FillDebitAndCreditAmt(GLTran tran, decimal diff, decimal curyDiff, bool isReversedSign, string accountType)
		{
			if ((accountType == AccountType.Asset || accountType == AccountType.Expense) && diff > 0m ||
				(accountType == AccountType.Liability || accountType == AccountType.Income) &&
				(isReversedSign && diff > 0m || !isReversedSign && diff < 0m))
			{
				tran.DebitAmt = Math.Abs(diff);
				tran.CuryDebitAmt = Math.Abs(curyDiff);
				tran.CreditAmt = 0m;
			}
			else
			{
				tran.DebitAmt = 0m;
				tran.CreditAmt = Math.Abs(diff);
				tran.CuryCreditAmt = Math.Abs(curyDiff);
			}
		}

		private static IEnumerable<GLHistoryEnquiryWithSubResult> GetBalances(JournalEntryImport graph, int? branchID, int? ledgerID, string finPeriodID, string begFinPeriod, string baseCurrency)
		{
			return GetBalances(graph, IsUnsignOperations(graph), branchID, ledgerID, finPeriodID, begFinPeriod, baseCurrency);
		}

		private static IEnumerable<GLHistoryEnquiryWithSubResult> GetBalances(PXGraph graph, bool isReversedSign, int? branchID, int? ledgerID, string finPeriodID, string begFinPeriod, string baseCurrency)
		{
			if (ledgerID == null || finPeriodID == null) yield break;

			PXSelectBase<GLHistoryByPeriod> cmd = new PXSelectJoinGroupBy<GLHistoryByPeriod,
								InnerJoin<Account,
										On<GLHistoryByPeriod.accountID, Equal<Account.accountID>, And<Match<Account, Current<AccessInfo.userName>>>>,
								InnerJoin<Sub,
										On<GLHistoryByPeriod.subID, Equal<Sub.subID>, And<Match<Sub, Current<AccessInfo.userName>>>>,
								LeftJoin<GLHistory, On<GLHistoryByPeriod.accountID, Equal<GLHistory.accountID>,
										And<GLHistoryByPeriod.branchID, Equal<GLHistory.branchID>,
										And<GLHistoryByPeriod.ledgerID, Equal<GLHistory.ledgerID>,
										And<GLHistoryByPeriod.subID, Equal<GLHistory.subID>,
										And<GLHistoryByPeriod.finPeriodID, Equal<GLHistory.finPeriodID>>>>>>,
								LeftJoin<AH, On<GLHistoryByPeriod.ledgerID, Equal<AH.ledgerID>,
										And<GLHistoryByPeriod.branchID, Equal<AH.branchID>,
										And<GLHistoryByPeriod.accountID, Equal<AH.accountID>,
										And<GLHistoryByPeriod.subID, Equal<AH.subID>,
										And<GLHistoryByPeriod.lastActivityPeriod, Equal<AH.finPeriodID>>>>>>>>>>,
								Where<GLHistoryByPeriod.ledgerID, Equal<Required<GLHistoryByPeriod.ledgerID>>,
										And<GLHistoryByPeriod.finPeriodID, Equal<Required<GLHistoryByPeriod.finPeriodID>>,
										And2<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
											Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>,
										And<
											Where2<
												Where<Account.type, Equal<AccountType.asset>,
													Or<Account.type, Equal<AccountType.liability>>>,
											Or<Where<GLHistoryByPeriod.lastActivityPeriod, GreaterEqual<Required<GLHistoryByPeriod.lastActivityPeriod>>,
												And<Where<Account.type, Equal<AccountType.expense>,
												Or<Account.type, Equal<AccountType.income>>>>>>>>>>>,
								Aggregate<
										Sum<AH.finYtdBalance,
										Sum<AH.curyFinYtdBalance,
										Sum<GLHistory.finPtdDebit,
										Sum<GLHistory.finPtdCredit,
										Sum<GLHistory.finBegBalance,
										Sum<GLHistory.finYtdBalance,
										Sum<GLHistory.curyFinBegBalance,
										Sum<GLHistory.curyFinYtdBalance,
										Sum<GLHistory.curyFinPtdCredit,
										Sum<GLHistory.curyFinPtdDebit,
										GroupBy<GLHistoryByPeriod.ledgerID,
										GroupBy<GLHistoryByPeriod.accountID,
										GroupBy<GLHistoryByPeriod.subID,
										GroupBy<GLHistoryByPeriod.finPeriodID
								 >>>>>>>>>>>>>>>>(graph);

			Ledger ledger = PXSelect<Ledger, Where<Ledger.ledgerID, Equal<Required<Ledger.ledgerID>>>>.Select(graph, ledgerID);
			if (ledger.PostInterCompany == true)
			{
				cmd.WhereAnd<Where<GLHistoryByPeriod.branchID, Equal<Required<GLHistoryByPeriod.branchID>>>>();
			}

			foreach (PXResult<GLHistoryByPeriod, Account, Sub, GLHistory, AH> it in
				cmd.Select(ledgerID, finPeriodID, begFinPeriod, branchID))
			{
				GLHistoryByPeriod baseview = (GLHistoryByPeriod)it;
				Account acct = (Account)it;
				GLHistory ah = (GLHistory)it;
				AH ah1 = (AH)it;

				GLHistoryEnquiryWithSubResult item = new GLHistoryEnquiryWithSubResult();
				item.LedgerID = baseview.LedgerID;
				item.AccountID = baseview.AccountID;
				item.AccountType = acct.Type;
				item.SubID = baseview.SubID;
				item.Type = acct.Type;
				item.Description = acct.Description;
				item.CuryID = acct.CuryID;
				item.LastActivityPeriod = baseview.LastActivityPeriod;
				item.PtdCreditTotal = ah.FinPtdCredit;
				item.PtdDebitTotal = ah.FinPtdDebit;
				item.CuryPtdCreditTotal = ah.CuryFinPtdCredit;
				item.CuryPtdDebitTotal = ah.CuryFinPtdDebit;
				bool reverseBalance = isReversedSign &&
					(item.AccountType == AccountTypeList.Liability || item.AccountType == AccountTypeList.Income);
				item.EndBalance = reverseBalance ? -ah1.FinYtdBalance : ah1.FinYtdBalance;
				item.CuryEndBalance = reverseBalance ? -ah1.CuryFinYtdBalance : ah1.CuryFinYtdBalance;
				item.ConsolAccountCD = acct.GLConsolAccountCD;
				item.BegBalance = item.EndBalance + (reverseBalance ? item.PtdSaldo : -item.PtdSaldo);
				item.CuryBegBalance = item.CuryEndBalance + (reverseBalance ? item.CuryPtdSaldo : -item.CuryPtdSaldo);
				yield return item;
			}
		}

		private void CheckMappingAndBalance(PXCache sender, GLTrialBalanceImportDetails row)
		{
			if (PXUIFieldAttribute.GetError<GLTrialBalanceImportDetails.importAccountCD>(sender, row) == Messages.ImportAccountCDIsEmpty)
				PXUIFieldAttribute.SetError<GLTrialBalanceImportDetails.importAccountCD>(sender, row, null);
			PXUIFieldAttribute.SetError<GLTrialBalanceImportDetails.mapAccountID>(sender, row, null);
			if (PXUIFieldAttribute.GetError<GLTrialBalanceImportDetails.importSubAccountCD>(sender, row) == Messages.ImportSubAccountCDIsEmpty)
				PXUIFieldAttribute.SetError<GLTrialBalanceImportDetails.importSubAccountCD>(sender, row, null);
			PXUIFieldAttribute.SetError<GLTrialBalanceImportDetails.mapSubAccountID>(sender, row, null);
			PXUIFieldAttribute.SetError<GLTrialBalanceImportDetails.ytdBalance>(sender, row, null);
			if (Map.Current != null && IsEditable(Map.Current) && Map.Current.IsHold != true)
			{
				if (row.ImportAccountCD == null)
					PXUIFieldAttribute.SetError<GLTrialBalanceImportDetails.importAccountCD>(sender, row, Messages.ImportAccountCDIsEmpty, null);
				if (row.MapAccountID == null)
					PXUIFieldAttribute.SetError<GLTrialBalanceImportDetails.mapAccountID>(sender, row, Messages.ImportAccountIDIsEmpty, null);
				if (row.ImportSubAccountCD == null && PXAccess.FeatureInstalled<CS.FeaturesSet.subAccount>() == true)
					PXUIFieldAttribute.SetError<GLTrialBalanceImportDetails.importSubAccountCD>(sender, row, Messages.ImportSubAccountCDIsEmpty, null);
				if (row.MapSubAccountID == null)
					PXUIFieldAttribute.SetError<GLTrialBalanceImportDetails.mapSubAccountID>(sender, row, Messages.ImportSubAccountIDIsEmpty, null);
				if (row.YtdBalance == null)
					PXUIFieldAttribute.SetError<GLTrialBalanceImportDetails.ytdBalance>(sender, row, Messages.ImportYtdBalanceIsEmpty, null);
			}
		}

		#endregion

		#region Implementation of PXImportAttribute.IPXPrepareItems

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (viewName == _IMPORTTEMPLATE_VIEWNAME && Map.Current != null)
			{
				string value = Map.Current.Number;
				if (keys.Contains(_mapNumberFieldName)) keys[_mapNumberFieldName] = value;
				else keys.Add(_mapNumberFieldName, value);
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

		public virtual void PrepareItems(string viewName, IEnumerable items) { }

		#endregion
	}
}
