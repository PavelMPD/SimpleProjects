using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.BQLConstants;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.AP;
using PX.Objects.AR;


namespace PX.Objects.CA
{
	[TableAndChartDashboardType]
	public class CABalValidate : PXGraph<CABalValidate>
	{
		public CABalValidate()
		{
			CASetup setup = CASetup.Current;

			CABalValidateList.SetProcessDelegate<CATranEntry>(Validate);

			CABalValidateList.SetProcessCaption("Validate");
			CABalValidateList.SetProcessAllCaption("Validate All");

			PXUIFieldAttribute.SetEnabled<CashAccount.selected>(CABalValidateList.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<CashAccount.cashAccountCD>(CABalValidateList.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<CashAccount.descr>(CABalValidateList.Cache, null, false);
		}

		public PXAction<CashAccount> cancel;
		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
		[PXCancelButton]
		protected virtual IEnumerable Cancel(PXAdapter adapter)
		{
			CABalValidateList.Cache.Clear();
			TimeStamp = null;
			return adapter.Get();
		}

		[PXFilterable]
		public PXProcessing<CashAccount, Where<CashAccount.active, Equal<boolTrue>>> CABalValidateList;
		public PXSetup<CASetup> CASetup;

		protected virtual IEnumerable cABalValidateList()
		{
			bool anyFound = false;
			foreach (CashAccount tlist in CABalValidateList.Cache.Inserted)
			{
				anyFound = true;
				yield return tlist;
			}
			if (anyFound)
			{
				yield break;
			}

			foreach (CashAccount cash in PXSelect<CashAccount>.Select(this))
			{
				yield return cash;
			}
			CABalValidateList.Cache.IsDirty = false;
		}

		private static void Validate(CATranEntry te, CashAccount tlist)
		{
			if (tlist.Reconcile != true)
			{ 
				te.Clear();
				using (new PXConnectionScope())
				{
					using (PXTransactionScope ts = new PXTransactionScope())
					{
						PXCache adjcache = te.Caches[typeof(CATran)];
						foreach (CATran catran in PXSelect<CATran, Where<CATran.cashAccountID, Equal<Required<CATran.cashAccountID>>>>.Select(te, tlist.CashAccountID))
						{
							if (tlist.Reconcile != true && (catran.Cleared != true || catran.TranDate == null))
							{
								catran.Cleared   = true;
								catran.ClearDate = catran.TranDate;
							}
							te.catrancache.Update(catran);
						}
						te.catrancache.Persist(PXDBOperation.Update);
						ts.Complete(te);
					}
					te.catrancache.Persisted(false);
				}
			}

			te.Clear();

			using (new PXConnectionScope())
			{
				PXCache adjcache = te.Caches[typeof(CAAdj)];

				using (PXTransactionScope ts = new PXTransactionScope())
				{
					foreach (PXResult<CAAdj, CATran> res in PXSelectJoin<CAAdj, LeftJoin<CATran, On<CATran.tranID, Equal<CAAdj.tranID>>>, Where<CAAdj.cashAccountID, Equal<Required<CAAdj.cashAccountID>>, And<CATran.tranID, IsNull>>>.Select(te, tlist.CashAccountID))
					{
						CAAdj caadj = (CAAdj)res;

						adjcache.SetValue<CAAdj.tranID>(caadj, null);
						adjcache.SetValue<CAAdj.cleared>(caadj, false);

						CATran catran = AdjCashTranIDAttribute.DefaultValues<CAAdj.tranID>(adjcache, caadj);

						if (catran != null)
						{
							catran = (CATran)te.catrancache.Insert(catran);
							te.catrancache.PersistInserted(catran);
							long id = Convert.ToInt64(PXDatabase.SelectIdentity());

							adjcache.SetValue<CAAdj.tranID>(caadj, id);
							adjcache.Update(caadj);
						}
					}

					adjcache.Persist(PXDBOperation.Update);

					ts.Complete(te);
				}

				adjcache.Persisted(false);
				te.catrancache.Persisted(false);
			}

			te.Clear();

			using (new PXConnectionScope())
			{
				PXCache transfercache = te.Caches[typeof(CATransfer)];

				using (PXTransactionScope ts = new PXTransactionScope())
				{
					foreach (PXResult<CATransfer, CATran> res in PXSelectJoin<CATransfer, LeftJoin<CATran, On<CATran.tranID, Equal<CATransfer.tranIDIn>>>, Where<CATransfer.inAccountID, Equal<Required<CATransfer.inAccountID>>, And<CATran.tranID, IsNull>>>.Select(te, tlist.CashAccountID))
					{
						CATransfer catransfer = (CATransfer)res;

						transfercache.SetValue<CATransfer.tranIDIn>(catransfer, null);
						transfercache.SetValue<CATransfer.clearedIn>(catransfer, false);
						if (transfercache.GetValue<CATransfer.clearedOut>(catransfer) == null)
						{
							transfercache.SetValue<CATransfer.clearedOut>(catransfer, false);
						}

						CATran catran = TransferCashTranIDAttribute.DefaultValues<CATransfer.tranIDIn>(transfercache, catransfer);

						if (catran != null)
						{
							catran = (CATran)te.catrancache.Insert(catran);
							te.catrancache.PersistInserted(catran);
							long id = Convert.ToInt64(PXDatabase.SelectIdentity());

							transfercache.SetValue<CATransfer.tranIDIn>(catransfer, id);
							transfercache.Update(catransfer);
						}
					}

					foreach (PXResult<CATransfer, CATran> res in PXSelectJoin<CATransfer, LeftJoin<CATran, On<CATran.tranID, Equal<CATransfer.tranIDOut>>>, Where<CATransfer.outAccountID, Equal<Required<CATransfer.outAccountID>>, And<CATran.tranID, IsNull>>>.Select(te, tlist.CashAccountID))
					{
						CATransfer catransfer = (CATransfer)res;

						transfercache.SetValue<CATransfer.tranIDOut>(catransfer, null);
						transfercache.SetValue<CATransfer.clearedOut>(catransfer, false);
						if (transfercache.GetValue<CATransfer.clearedIn>(catransfer) == null)
						{
							transfercache.SetValue<CATransfer.clearedIn>(catransfer, false);
						}

						CATran catran = TransferCashTranIDAttribute.DefaultValues<CATransfer.tranIDOut>(transfercache, catransfer);

						if (catran != null)
						{
							catran = (CATran)te.catrancache.Insert(catran);
							te.catrancache.PersistInserted(catran);
							long id = Convert.ToInt64(PXDatabase.SelectIdentity());

							transfercache.SetValue<CATransfer.tranIDOut>(catransfer, id);
							transfercache.Update(catransfer);
						}
					}

					transfercache.Persist(PXDBOperation.Update);

					ts.Complete(te);
				}

				transfercache.Persisted(false);
				te.catrancache.Persisted(false);
			}

			te.Clear();

			PXDBDefaultAttribute.SetDefaultForUpdate<GLTran.module>(te.Caches[typeof(GLTran)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<GLTran.batchNbr>(te.Caches[typeof(GLTran)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<GLTran.ledgerID>(te.Caches[typeof(GLTran)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<GLTran.finPeriodID>(te.Caches[typeof(GLTran)], null, false);

			using (new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					foreach (PXResult<GLTran, Ledger, Batch> res in PXSelectJoin<GLTran, InnerJoin<Ledger, On<Ledger.ledgerID, Equal<GLTran.ledgerID>>,
									InnerJoin<Batch, On<Batch.module,Equal<GLTran.module>, And<Batch.batchNbr,Equal<GLTran.batchNbr>,
										And<Batch.scheduled, Equal<False>>>>>>, 
									Where<GLTran.accountID, Equal<Required<GLTran.accountID>>, And<GLTran.subID, Equal<Required<GLTran.subID>>, 
									And<Ledger.balanceType, Equal<LedgerBalanceType.actual>, And<GLTran.cATranID, IsNull>>>>>.Select(te, tlist.AccountID, tlist.SubID))
					{
						GLTran gltran = (GLTran)res;
						CATran catran = GLCashTranIDAttribute.DefaultValues<GLTran.cATranID>(te.gltrancache, gltran);

						if (catran != null)
						{
							catran = (CATran) te.catrancache.Insert(catran);
							te.catrancache.PersistInserted(catran);
							long id = Convert.ToInt64(PXDatabase.SelectIdentity());

							gltran.CATranID = id;
							te.gltrancache.Update(gltran);
						}
					}

					te.gltrancache.Persist(PXDBOperation.Update);

					te.dailycache.Clear();

					PXDatabase.Delete<CADailySummary>(
							new PXDataFieldRestrict("CashAccountID", PXDbType.Int, 4, tlist.CashAccountID, PXComp.EQ)
						);

					foreach (CATran tran in PXSelect<CATran, Where<CATran.cashAccountID, Equal<Required<CATran.cashAccountID>>>>.Select(te, tlist.CashAccountID))
					{
						CADailyAccumulatorAttribute.RowInserted<CATran.tranDate>(te.catrancache, tran);
					}

					te.dailycache.Persist(PXDBOperation.Insert);
					te.dailycache.Persist(PXDBOperation.Update);

					ts.Complete(te);
				}

				te.gltrancache.Persisted(false);
				te.catrancache.Persisted(false);
				te.dailycache.Persisted(false);
			}
		}
	}
}
