using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.IN;

using PX.Reports.ARm;
using PX.Reports.ARm.Data;
using System.Drawing;
using PX.CS;


namespace PX.Objects.CS
{
	public class RMReportReaderGL : PXGraphExtension<RMReportReader>
	{
		#region report
		public PXSetup<GLSetup> setup;

		protected string _perWildcard = "______";
		public virtual string perWildcard
		{
			get
			{
				return this._perWildcard;
			}
			set
			{
				this._perWildcard = value;
			}
		}

		protected string _acctWildcard;
		public virtual string acctWildcard
		{
			get
			{
				return this._acctWildcard;
			}
			set
			{
				this._acctWildcard = value;
			}
		}

		protected string _branchWildcard;
		public virtual string branchWildcard
		{
			get
			{
				return this._branchWildcard;
			}
			set
			{
				this._branchWildcard = value;
			}
		}

		public List<FinPeriod> finPeriods;

		protected PXCache _finPeriodCache;
		public virtual PXCache finPeriodCache
		{
			get
			{
				return this._finPeriodCache;
			}
			set
			{
				this._finPeriodCache = value;
			}
		}

		protected string _taskWildcard;
		public virtual string taskWildcard
		{
			get
			{
				return this._taskWildcard;
			}
			set
			{
				this._taskWildcard = value;
			}
		}

		protected GLHistory glhistoryInstance;
		protected Account accountInstance;
		protected Sub subInstance;
		protected Branch branch;

		protected string subWildcard;
		
		protected int? ytdNetIncomeAccountID;

		protected PXCache glhistoryCache;
		protected PXCache bAccountCache;
		protected PXCache finPeriodsCache;
		protected List<CR.BAccount> bAccounts;
		protected HashSet<string> glhistoryPeriods;


		protected PXCache accountCache;
		protected PXCache subCache;
		protected PXCache Branches;

		[PXOverride]
		public void Clear(Action del)
		{
			del();
			acctWildcard = null;
			taskWildcard = null;
		}

		public virtual void GLEnsureInitialized()
		{
			if (acctWildcard == null)
			{
				Dimension dim = PXSelect<Dimension,
					Where<Dimension.dimensionID, Equal<Required<Dimension.dimensionID>>>>
					.Select(this.Base, GL.AccountAttribute.DimensionName);
				if (dim != null && dim.Length != null)
				{
					acctWildcard = new String('_', (int)dim.Length);
				}
				else
				{
					acctWildcard = "";
				}
				dim = PXSelect<Dimension,
					Where<Dimension.dimensionID, Equal<Required<Dimension.dimensionID>>>>
					.Select(this.Base, GL.SubAccountAttribute.DimensionName);
				if (dim != null && dim.Length != null)
				{
					subWildcard = new String('_', (int)dim.Length);
				}
				else
				{
					subWildcard = "";
				}
				dim = PXSelect<Dimension,
					Where<Dimension.dimensionID, Equal<Required<Dimension.dimensionID>>>>
					.Select(this.Base, GL.BranchAttribute._DimensionName);
				if (dim != null && dim.Length != null)
				{
					branchWildcard = new String('_', (int)dim.Length);
				}
				else
				{
					branchWildcard = "";
				}

				PXSelectOrderBy<FinPeriod, OrderBy<Asc<FinPeriod.startDate>>>.Select(this.Base);
				finPeriodCache=Base.Caches[typeof(FinPeriod)];
				finPeriods = new List<FinPeriod>();
				foreach (FinPeriod per in finPeriodCache.Cached)
				{
					finPeriods.Add(per);
				}

				// netincom - only one account
				if (setup != null && setup.Current != null)
				{
					ytdNetIncomeAccountID = setup.Current.YtdNetIncAccountID;
				}

				PXSelect<CR.BAccount>.Select(this.Base);
				bAccountCache = Base.Caches[typeof(CR.BAccount)];
				bAccounts = new List<CR.BAccount>();
				foreach(CR.BAccount bacc in bAccountCache.Cached)
				{
					bAccounts.Add(bacc);
				}

				PXSelect<GLHistory>.Select(this.Base);
				glhistoryInstance = new GLHistory();
				glhistoryCache = Base.Caches[typeof(GLHistory)];
				glhistoryPeriods = new HashSet<string>();
				foreach (GLHistory hist in glhistoryCache.Cached)
				{
					glhistoryPeriods.Add(hist.FinPeriodID);
				}
				accountCache = Base.Caches[typeof(Account)];
				accountCache.Clear();
				subCache = Base.Caches[typeof(Sub)];
				subCache.Clear();
				Branches = Base.Caches[typeof(Branch)];
				Branches.Clear();
				if (Base.Report.Current.ApplyRestrictionGroups == false)
				{
					PXSelect<Account>.Clear(this.Base);
					PXSelect<Account>.Select(this.Base);

					PXSelect<Sub>.Clear(this.Base);
					PXSelect<Sub>.Select(this.Base);

					PXSelect<Branch>.Clear(this.Base);
					PXSelect<Branch>.Select(this.Base);
				}
				else
				{
					PXSelect<Account, Where<Match<Current<AccessInfo.userName>>>>.Clear(this.Base);
					PXSelect<Account, Where<Match<Current<AccessInfo.userName>>>>.Select(this.Base);

					PXSelect<Sub, Where<Match<Current<AccessInfo.userName>>>>.Clear(this.Base);
					PXSelect<Sub, Where<Match<Current<AccessInfo.userName>>>>.Select(this.Base);

					PXSelect<Branch, Where<Match<Current<AccessInfo.userName>>>>.Clear(this.Base);
					PXSelect<Branch, Where<Match<Current<AccessInfo.userName>>>>.Select(this.Base);
				}
				accountInstance = new Account();
				subInstance = new Sub();
				PXSelect<FinPeriod>.Select(this.Base);
				finPeriodCache = Base.Caches[typeof(FinPeriod)];
				branch = new Branch();
			}
		}

		protected virtual void NormalizeDataSource(RMDataSourceGL dsGL)
		{
			if (dsGL.StartBranch != null && dsGL.StartBranch.TrimEnd() == "")
			{
				dsGL.StartBranch = null;
			}
			if (dsGL.EndBranch != null && dsGL.EndBranch.TrimEnd() == "")
			{
				dsGL.EndBranch = null;
			}
			if (dsGL.AccountClassID != null && dsGL.AccountClassID.TrimEnd() == "")
			{
				dsGL.AccountClassID = null;
			}
			if (dsGL.StartAccount != null && dsGL.StartAccount.TrimEnd() == "")
			{
				dsGL.StartAccount = null;
			}
			if (dsGL.EndAccount != null && dsGL.EndAccount.TrimEnd() == "")
			{
				dsGL.EndAccount = null;
			}
			if (dsGL.StartSub != null && dsGL.StartSub.TrimEnd() == "")
			{
				dsGL.StartSub = null;
			}
			if (dsGL.EndSub != null && dsGL.EndSub.TrimEnd() == "")
			{
				dsGL.EndSub = null;
			}
			if (dsGL.StartPeriod != null && dsGL.StartPeriod.TrimEnd() == "")
			{
				dsGL.StartPeriod = null;
			}
			if (dsGL.EndPeriod != null && dsGL.EndPeriod.TrimEnd() == "")
			{
				dsGL.EndPeriod = null;
			}
			if (dsGL.StartPeriodOffset == null)
			{
				dsGL.StartPeriodOffset = 0;
			}
			if (dsGL.StartPeriodYearOffset == null)
			{
				dsGL.StartPeriodYearOffset = 0;
			}
			if (dsGL.EndPeriodOffset == null)
			{
				dsGL.EndPeriodOffset = 0;
			}
			if (dsGL.EndPeriodYearOffset == null)
			{
				dsGL.EndPeriodYearOffset = 0;
			}
		}

		/// <summary>
		/// Returns the financial period("{0:0000}{1:00}") by the current period("{0:0000}{1:00}") and offsets.
		/// Note: In different years can be a different number of financial periods.
		/// </summary>		
		public string GetFinPeriod(string period, short? yearOffset, short? periodOffset)
		{
			string result = period;
			if (!string.IsNullOrEmpty(period) && period.Length == 6)
			{
				string year = period.Substring(0, 4);
				string periodNbr = period.Substring(4);
				// apply year offset
				if (yearOffset != null && yearOffset != 0)
				{
					string resYear = (int.Parse(year) + yearOffset).ToString();
					List<FinPeriod> finPeriodsResYear = finPeriods.Where(f => string.Compare(f.FinYear, resYear.ToString()) == 0).ToList();
					if (finPeriodsResYear != null && finPeriodsResYear.Count > 0)
					{
						string resPeriodNbr = finPeriodsResYear.Last().PeriodNbr;
						if (string.Compare(resPeriodNbr, periodNbr) < 0)
							result=String.Format("{0:0000}{1:00}",resYear, resPeriodNbr);
						else
							result = String.Format("{0:0000}{1:00}", resYear, periodNbr);
					}
					else // if there is no year
					{
						result = String.Format("{0:0000}{1:00}",resYear, 1);
					}
				}
				// apply period offset
				if (periodOffset != null && periodOffset != 0)
				{
					short currentIndex = 0;
					foreach (FinPeriod p in finPeriods)
					{
						if (!string.Equals(p.FinPeriodID, result))
							currentIndex++;
						else
							break;
					}
					currentIndex = (short)(currentIndex + periodOffset);
					if (currentIndex < 0) // if there is no year
					{
						result = String.Format("{0:0000}{1:00}", int.Parse(year) - 1, periodNbr);
					}
					else
					{
						short i = 0;
						foreach (FinPeriod p in finPeriods)
						{
							if (i == currentIndex)
							{
								result = p.FinPeriodID;
								break;
							}
							else
								i++;
						}
					}
				}
			}
			return result;
		}

		[PXOverride]
		public virtual object GetHistoryValue(ARmDataSet dataSet, bool drilldown, Func<ARmDataSet, bool, object> del)
		{
			string rmType = Base.Report.Current.Type;
			if (rmType == ARmReport.GL)
			{
				#region GL
				if (((!string.IsNullOrEmpty(dataSet[Keys.BookCode] as string ?? "") && (!string.IsNullOrEmpty(dataSet[Keys.StartAccount] as string ?? "") ||
				!string.IsNullOrEmpty(dataSet[Keys.AccountClass] as string ?? "")) && !string.IsNullOrEmpty(dataSet[Keys.StartPeriod] as string ?? "") && (short?)dataSet[Keys.AmountType] != (short?)BalanceType.NotSet)))
				{
					ARmDataSet dataSetCopy = new ARmDataSet(dataSet);

					RMDataSource ds = Base.DataSourceByID.Current;

					RMDataSourceGL dsGL = Base.Caches[typeof(RMDataSource)].GetExtension<RMDataSourceGL>(ds);

					ds.AmountType = (short?)dataSet[Keys.AmountType];
					Base.DataSourceByID.SetValueExt<RMDataSourceGL.ledgerID>(ds, dataSet[Keys.BookCode] as string ?? "");
					dsGL.AccountClassID = dataSet[Keys.AccountClass] as string ?? "";
					dsGL.StartAccount = dataSet[Keys.StartAccount] as string ?? "";
					dsGL.EndAccount = dataSet[Keys.EndAccount] as string ?? "";
					dsGL.StartSub = dataSet[Keys.StartSub] as string ?? "";
					dsGL.EndSub = dataSet[Keys.EndSub] as string ?? "";

					dsGL.StartBranch = dataSet[Keys.StartBranch] as string ?? "";
					dsGL.EndBranch = dataSet[Keys.EndBranch] as string ?? "";
					dsGL.EndPeriod = ((dataSet[Keys.EndPeriod] as string ?? "").Length > 2 ? ((dataSet[Keys.EndPeriod] as string ?? "").Substring(2) + "    ").Substring(0, 4) : "    ") + ((dataSet[Keys.EndPeriod] as string ?? "").Length > 2 ? (dataSet[Keys.EndPeriod] as string ?? "").Substring(0, 2) : dataSet[Keys.EndPeriod] as string ?? "");
					dsGL.EndPeriodOffset = (short?)(int?)dataSet[Keys.EndOffset];
					dsGL.EndPeriodYearOffset = (short?)(int?)dataSet[Keys.EndYearOffset];
					dsGL.StartPeriod = ((dataSet[Keys.StartPeriod] as string ?? "").Length > 2 ? ((dataSet[Keys.StartPeriod] as string ?? "").Substring(2) + "    ").Substring(0, 4) : "    ") + ((dataSet[Keys.StartPeriod] as string ?? "").Length > 2 ? (dataSet[Keys.StartPeriod] as string ?? "").Substring(0, 2) : dataSet[Keys.StartPeriod] as string ?? "");
					dsGL.StartPeriodOffset = (short?)(int?)dataSet[Keys.StartOffset];
					dsGL.StartPeriodYearOffset = (short?)(int?)dataSet[Keys.StartYearOffset];

					GLEnsureInitialized();

					if (drilldown)
					{
						Base.DrilldownNumber++;
					}

					List<Account> Accounts = new List<Account>();
					List<Sub> Subs = new List<Sub>();
					List<string> Periods = new List<string>();
					List<bool> BypassPL = new List<bool>();
					List<Branch> branchList = new List<Branch>();

					List<object[]> splitret = null;

					if (ds.Expand != ExpandType.Nothing)
					{
						splitret = new List<object[]>();
					}

					if (dsGL.LedgerID == null || ds.AmountType == null || ds.AmountType == 0)
					{
						return 0m;
					}
					glhistoryInstance.LedgerID = dsGL.LedgerID;

					NormalizeDataSource(dsGL);

					if (dsGL.StartBranch != null)
					{
						#region StartBranch
						string br = (dsGL.StartBranch.Replace(' ', '_').Replace('?', '_') + branchWildcard).Substring(0, branchWildcard.Length);
						if (dsGL.EndBranch == null || dsGL.EndBranch == dsGL.StartBranch)
						{
							if (!br.Contains("_"))
							{
								//branch.BranchCD = br;
								branch.BranchCD = br.Trim('_');
								Branch b = (Branch)Branches.Locate(branch);
								if (b != null)
								{
									branchList.Add(b);
								}
							}
							else
							{
								foreach (Branch b in Branches.Cached)
								{
									//if (Base.IsLike(br, b.BranchCD))
									if (string.Equals(br, b.BranchCD.Replace(' ', '_')))
									{
										branchList.Add(b);
									}
								}
							}
						}
						else
						{
							br = br.Replace('_', ' ');
							string tobr = (dsGL.EndBranch.Replace(' ', 'z').Replace('?', 'z') + new String('z', branchWildcard.Length)).Substring(0, branchWildcard.Length);
							foreach (Branch b in Branches.Cached)
							{
								if (Base.IsBetween(br, tobr, b.BranchCD))
								{
									branchList.Add(b);
								}
							}
						}
						#endregion
					}
					else
					{
						foreach (Branch b in Branches.Cached)
						{
							branchList.Add(b);
						}
					}

					if (dsGL.StartAccount != null)
					{
						#region StartAccount
						string acct = (dsGL.StartAccount.Replace(' ', '_').Replace('?', '_') + acctWildcard).Substring(0, acctWildcard.Length);
						if (dsGL.EndAccount == null || dsGL.EndAccount == dsGL.StartAccount)
						{
							if (!acct.Contains("_"))
							{
								accountInstance.AccountCD = acct;
								Account a = (Account)accountCache.Locate(accountInstance);
								if (a != null && (dsGL.AccountClassID == null || dsGL.AccountClassID == a.AccountClassID))
								{
									Accounts.Add(a);
									if (ds.Expand == ExpandType.Account)
									{
										splitret.Add(new object[] { a.AccountCD, a.Description, 0m, null, string.Empty });
									}
								}
							}
							else
							{
								foreach (Account a in accountCache.Cached)
								{
									if (Base.IsLike(acct, a.AccountCD)
										&& (dsGL.AccountClassID == null || dsGL.AccountClassID == a.AccountClassID))
									{
										Accounts.Add(a);
										if (ds.Expand == ExpandType.Account)
										{
											splitret.Add(new object[] { a.AccountCD, a.Description, 0m, null, string.Empty });
										}
									}
								}
							}
						}
						else
						{
							acct = acct.Replace('_', ' ');
							string toacct = (dsGL.EndAccount.Replace(' ', 'z').Replace('?', 'z') + new String('z', acctWildcard.Length)).Substring(0, acctWildcard.Length);
							foreach (Account a in accountCache.Cached)
							{
								if (Base.IsBetween(acct, toacct, a.AccountCD)
									&& (dsGL.AccountClassID == null || dsGL.AccountClassID == a.AccountClassID))
								{
									Accounts.Add(a);
									if (ds.Expand == ExpandType.Account)
									{
										splitret.Add(new object[] { a.AccountCD, a.Description, 0m, null, string.Empty });
									}
								}
							}
						}
						#endregion
					}
					else
					{
						foreach (Account a in accountCache.Cached)
						{
							if ((dsGL.AccountClassID == null || dsGL.AccountClassID == a.AccountClassID))
							{
								Accounts.Add(a);
								if (ds.Expand == ExpandType.Account)
								{
									splitret.Add(new object[] { a.AccountCD, a.Description, 0m, null, string.Empty });
								}
							}
						}
					}
					if (dsGL.StartSub != null)
					{
						#region startsub
						string sub = (dsGL.StartSub.Replace(' ', '_').Replace('?', '_') + subWildcard).Substring(0, subWildcard.Length);
						if (dsGL.EndSub == null || dsGL.EndSub == dsGL.StartSub)
						{
							if (!sub.Contains("_"))
							{
								subInstance.SubCD = sub;
								Sub s = (Sub)subCache.Locate(subInstance);
								if (s != null)
								{
									Subs.Add(s);
								}
							}
							else
							{
								foreach (Sub s in subCache.Cached)
								{
									if (Base.IsLike(sub, s.SubCD))
									{
										Subs.Add(s);
										if (ds.Expand == ExpandType.Sub)
										{
											splitret.Add(new object[] { s.SubCD, s.Description, 0m, null, string.Empty });
										}
									}
								}
							}
						}
						else
						{
							foreach (Sub s in subCache.Cached)
							{
								if (IsBetweenByChar(dsGL.StartSub, dsGL.EndSub, s.SubCD))
								{
									Subs.Add(s);
									if (ds.Expand == ExpandType.Sub)
									{
										splitret.Add(new object[] { s.SubCD, s.Description, 0m, null, string.Empty });
									}
								}
							}
						}
						#endregion
					}
					else
					{
						foreach (Sub s in subCache.Cached)
						{
							Subs.Add(s);
							if (ds.Expand == ExpandType.Sub)
							{
								splitret.Add(new object[] { s.SubCD, s.Description, 0m, null, string.Empty });
							}
						}
					}
					bool takeLast = false;

					if (ds.AmountType == 4)
					{
						#region AmountType 4
						if (dsGL.StartPeriod != null)
						{
							string per = (dsGL.StartPeriod.Replace(' ', '_').Replace('?', '_') + perWildcard).Substring(0, perWildcard.Length);
							if (!per.Contains("_"))
							{
								per = GetFinPeriod(per, dsGL.StartPeriodYearOffset, dsGL.StartPeriodOffset);
								takeLast = true;
								foreach (FinPeriod p in finPeriodCache.Cached)
								{
									if (glhistoryPeriods.Contains(p.FinPeriodID) && String.Compare(p.FinPeriodID, per) < 0 || String.Equals(p.FinPeriodID, per))
									{
										Periods.Add(p.FinPeriodID);
										BypassPL.Add(!String.Equals(p.FinPeriodID.Substring(0, 4), per.Substring(0, 4)));
									}
								}
							}
							else
							{
								foreach (FinPeriod p in finPeriodCache.Cached)
								{
									if (glhistoryPeriods.Contains(p.FinPeriodID))
									{
										Periods.Add(p.FinPeriodID);
										BypassPL.Add(false);
									}
								}
							}
						}
						else
						{
							foreach (FinPeriod p in finPeriodCache.Cached)
							{
								if (glhistoryPeriods.Contains(p.FinPeriodID))
								{
									Periods.Add(p.FinPeriodID);
									BypassPL.Add(false);
								}
							}
						}
						#endregion
					}
					else if (ds.AmountType == 5)
					{
						#region AmountType 5
						if (dsGL.EndPeriod != null)
						{
							string per = (dsGL.EndPeriod.Replace(' ', '_').Replace('?', '_') + perWildcard).Substring(0, perWildcard.Length);
							if (!per.Contains("_"))
							{
								per = GetFinPeriod(per, dsGL.EndPeriodYearOffset, dsGL.EndPeriodOffset);
								foreach (FinPeriod p in finPeriodCache.Cached)
								{
									if (glhistoryPeriods.Contains(p.FinPeriodID) && String.Compare(p.FinPeriodID, per) <= 0)
									{
										Periods.Add(p.FinPeriodID);
										BypassPL.Add(!String.Equals(p.FinPeriodID.Substring(0, 4), per.Substring(0, 4)));
									}
								}
							}
							else
							{
								foreach (FinPeriod p in finPeriodCache.Cached)
								{
									if (glhistoryPeriods.Contains(p.FinPeriodID))
									{
										Periods.Add(p.FinPeriodID);
										BypassPL.Add(false);
									}
								}
							}
						}
						else if (dsGL.StartPeriod != null)
						{
							string per = (dsGL.StartPeriod.Replace(' ', '_').Replace('?', '_') + perWildcard).Substring(0, perWildcard.Length);
							if (!per.Contains("_"))
							{
								per = GetFinPeriod(per, dsGL.StartPeriodYearOffset, dsGL.StartPeriodOffset);
								foreach (FinPeriod p in finPeriodCache.Cached)
								{
									if (glhistoryPeriods.Contains(p.FinPeriodID) && String.Compare(p.FinPeriodID, per) <= 0)
									{
										Periods.Add(p.FinPeriodID);
										BypassPL.Add(!String.Equals(p.FinPeriodID.Substring(0, 4), per.Substring(0, 4)));
									}
								}
							}
							else
							{
								foreach (FinPeriod p in finPeriodCache.Cached)
								{
									if (glhistoryPeriods.Contains(p.FinPeriodID))
									{
										Periods.Add(p.FinPeriodID);
										BypassPL.Add(false);
									}
								}
							}
						}
						else
						{
							foreach (FinPeriod p in finPeriodCache.Cached)
							{
								if (glhistoryPeriods.Contains(p.FinPeriodID))
								{
									Periods.Add(p.FinPeriodID);
									BypassPL.Add(false);
								}
							}
						}
						#endregion
					}
					else
					{
						#region other
						if (dsGL.StartPeriod != null)
						{
							string per = (dsGL.StartPeriod.Replace(' ', '_').Replace('?', '_') + perWildcard).Substring(0, perWildcard.Length);
							if (!per.Contains("_") && dsGL.StartPeriodOffset != null && dsGL.StartPeriodOffset != 0 || dsGL.StartPeriodYearOffset != 0)
							{
								per = GetFinPeriod(per, dsGL.StartPeriodYearOffset, dsGL.StartPeriodOffset);
							}
							if (dsGL.EndPeriod == null)
							{
								if (!per.Contains("_"))
								{
									Periods.Add(per);
									BypassPL.Add(false);
								}
								else
								{
									foreach (FinPeriod p in finPeriodCache.Cached)
									{
										if (glhistoryPeriods.Contains(p.FinPeriodID) && Base.IsLike(per, p.FinPeriodID))
										{
											Periods.Add(p.FinPeriodID);
											BypassPL.Add(false);
										}
									}
								}
							}
							else
							{
								per = per.Replace('_', '0');
								string toper = (dsGL.EndPeriod.Replace(' ', '_').Replace('?', '_') + perWildcard).Substring(0, perWildcard.Length);
								if (!toper.Contains("_") && dsGL.EndPeriodOffset != null && dsGL.EndPeriodOffset != 0 || dsGL.EndPeriodYearOffset != 0)
								{
									toper = GetFinPeriod(toper, dsGL.EndPeriodYearOffset, dsGL.EndPeriodOffset);
								}
								toper = toper.Replace('_', '9');
								foreach (FinPeriod p in finPeriodCache.Cached)
								{
									if (glhistoryPeriods.Contains(p.FinPeriodID) && Base.IsBetween(per, toper, p.FinPeriodID))
									{
										Periods.Add(p.FinPeriodID);
										BypassPL.Add(false);
									}
								}
							}
						}
						else
						{
							foreach (FinPeriod p in finPeriodCache.Cached)
							{
								if (glhistoryPeriods.Contains(p.FinPeriodID))
								{
									Periods.Add(p.FinPeriodID);
									BypassPL.Add(false);
								}
							}
						}
						#endregion
					}

					#region NetIncome
					//end period datasource
					int endYear;
					//periods for all accounts
					List<string> PeriodsAll = new List<string>(Periods);
					//periods for NetIncome accounts
					List<string> PeriodsNetIncome = new List<string>();
					if (dsGL.EndPeriod != null)
					{
						string perEnd = (dsGL.EndPeriod.Replace(' ', '_').Replace('?', '_') + perWildcard).Substring(0, perWildcard.Length);
						if (!perEnd.Contains("_") && dsGL.EndPeriodOffset != null && dsGL.EndPeriodOffset != 0 || dsGL.EndPeriodYearOffset != 0)
						{
							perEnd = GetFinPeriod(perEnd, dsGL.EndPeriodYearOffset, dsGL.EndPeriodOffset);
						}						
						if (Int32.TryParse(perEnd.Substring(0, 4), out endYear))
						{
							PeriodsNetIncome.AddRange(PeriodsAll.Where(p => p.StartsWith(endYear.ToString())).ToList());
						}
						else
						{
							PeriodsNetIncome = PeriodsAll;
						}
					}
					#endregion

					if (ds.Expand == null || ds.Expand == ExpandType.Account || ds.Expand == ExpandType.Nothing)
					{
						#region Expand : null, A, N
						PXResultset<GLHistoryByPeriod, Account, Sub, GLHistory, GLSetup> resultset = null;
						GLHistory resulthist = null;
						if (drilldown)
						{
							resultset = new PXResultset<GLHistoryByPeriod, Account, Sub, GLHistory, GLSetup>();
							resulthist = new GLHistory();
							Accounts.Sort((Account a, Account b) =>
							{
								if (a.COAOrder < b.COAOrder)
								{
									return -1;
								}
								if (a.COAOrder > b.COAOrder)
								{
									return 1;
								}

								if (a.Type == b.Type)
								{
									return String.Compare(a.AccountCD, b.AccountCD, StringComparison.OrdinalIgnoreCase);
								}

								return String.Compare(a.Type, b.Type, StringComparison.OrdinalIgnoreCase);
							});
						}
						Account lastAcct = null;
						Sub lastSub = null;
						Branch lastBranch = null;
						decimal? lastYtd = 0m;
						decimal amt = 0m;
						int? branchID = null;
						for (int a = 0; a < Accounts.Count; a++)
						{
							#region NetIncome
							// if NetIncome account
							if (ytdNetIncomeAccountID != null && Accounts[a].AccountID == ytdNetIncomeAccountID)
							{
								Periods = PeriodsNetIncome;
							}
							else
							{
								Periods = PeriodsAll;
							}
							#endregion
							for (int s = 0; s < Subs.Count; s++)
							{
								for (int b = 0; b < branchList.Count; b++)
								{
									for (int p = 0; p < Periods.Count; p++)
									{
										if (BypassPL[p]
											&& (Accounts[a].Type == AccountType.Expense
											|| Accounts[a].Type == AccountType.Income))
										{
											continue;
										}
										glhistoryInstance.AccountID = Accounts[a].AccountID;
										glhistoryInstance.SubID = Subs[s].SubID;
										glhistoryInstance.FinPeriodID = Periods[p];
										glhistoryInstance.BranchID = branchList[b].BranchID;
										
										GLHistory hist = (GLHistory)glhistoryCache.Locate(glhistoryInstance);
										if (hist == null)
										{
											continue;
										}
										branchID = branchList[b].BranchID;
										if (ds.Expand == ExpandType.Account && lastAcct != null && lastAcct.AccountID != hist.AccountID)
										{
											amt += (decimal)lastYtd;
											for (int k = 0; k < Accounts.Count; k++)
											{
												if (lastAcct != null && Accounts[k].AccountID == lastAcct.AccountID)
												{
													dataSetCopy = new ARmDataSet(dataSetCopy);
													dataSetCopy[Keys.StartAccount] = dataSetCopy[Keys.EndAccount] = Accounts[k].AccountCD;
													splitret[k][2] = amt;
													splitret[k][3] = dataSetCopy;
													splitret[k][4] = bAccounts.FirstOrDefault(ba => ba.BAccountID == branchList[b].BAccountID).AcctName;
													break;
												}
											}
											amt = 0m;
											lastYtd = 0m;
										}
										switch (ds.AmountType.Value)
										{
											case 1: //Turnover
												if (Accounts[a].Type == AccountType.Asset || Accounts[a].Type == AccountType.Expense)
												{
													amt += (decimal)(hist.PtdDebit - hist.PtdCredit);
												}
												else
												{
													amt += (decimal)(hist.PtdCredit - hist.PtdDebit);
												}
												break;
											case 2: //Credit
												amt += (decimal)hist.PtdCredit;
												break;
											case 3: //Debit
												amt += (decimal)hist.PtdDebit;
												break;
											case 4: //Beg Balance
												if (lastAcct == null || lastAcct.AccountID != hist.AccountID || lastSub == null || lastSub.SubID != hist.SubID || lastBranch == null || lastBranch.BranchID != hist.BranchID)
												{
													if (!takeLast)
													{
														amt += (decimal)hist.FinBegBalance;
													}
													else
													{
														amt += (decimal)lastYtd;
													}
												}
												if (takeLast)
												{
													if (p < Periods.Count - 1)
													{
														lastYtd = hist.FinYtdBalance;
													}
													else
													{
														lastYtd = hist.FinBegBalance;
													}
												}
												break;
											case 5: //End Balance
												if (lastAcct == null || lastAcct.AccountID != hist.AccountID || lastSub == null || lastSub.SubID != hist.SubID || lastBranch == null || lastBranch.BranchID != hist.BranchID)
												{
													amt += (decimal)lastYtd;
												}
												lastYtd = hist.FinYtdBalance;
												break;
										}
										if (drilldown)
										{
											if ((lastAcct != null && lastAcct.AccountID != hist.AccountID || lastSub != null && lastSub.SubID != hist.SubID || lastBranch == null || lastBranch.BranchID != hist.BranchID) &&
												(resulthist != null && resulthist.BranchID != null && resulthist.LedgerID != null && resulthist.AccountID != null && resulthist.SubID != null && resulthist.FinPeriodID != null))
											{
												GLHistoryByPeriod period = new GLHistoryByPeriod();
												period.LedgerID = resulthist.LedgerID;
												period.BranchID = resulthist.BranchID;
												period.AccountID = resulthist.AccountID;
												period.SubID = resulthist.SubID;
												period.FinPeriodID = resulthist.FinPeriodID;
												period.LastActivityPeriod = ds.AmountType.ToString() + ":" + Base.DrilldownNumber.ToString();
												resultset.Add(new PXResult<GLHistoryByPeriod, Account, Sub, GLHistory, GLSetup>(period, lastAcct, lastSub, resulthist, PXSelect<GLSetup>.Select(this.Base)));
												resulthist = new GLHistory();
											}
											resulthist.LedgerID = hist.LedgerID;
											resulthist.BranchID = hist.BranchID;
											resulthist.AccountID = hist.AccountID;
											resulthist.SubID = hist.SubID;
											resulthist.FinPeriodID = hist.FinPeriodID;
											if (resulthist.BegBalance == null)
											{
												resulthist.BegBalance = hist.BegBalance;
												resulthist.PtdDebit = 0m;
												resulthist.PtdCredit = 0m;
											}
											resulthist.PtdDebit += hist.PtdDebit;
											resulthist.PtdCredit += hist.PtdCredit;
											resulthist.YtdBalance = hist.YtdBalance;
										}
										lastAcct = Accounts[a];
										lastSub = Subs[s];
										lastBranch = branchList[b];
									}
								}
							}
						}
						amt += (decimal)lastYtd;
						if (drilldown && resulthist.LedgerID != null)
						{
							GLHistoryByPeriod period = new GLHistoryByPeriod();
							period.LedgerID = resulthist.LedgerID;
							period.BranchID = resulthist.BranchID;
							period.AccountID = resulthist.AccountID;
							period.SubID = resulthist.SubID;
							period.FinPeriodID = resulthist.FinPeriodID;
							period.LastActivityPeriod = ds.AmountType.ToString() + ":" + Base.DrilldownNumber.ToString();
							resultset.Add(new PXResult<GLHistoryByPeriod, Account, Sub, GLHistory, GLSetup>(period, lastAcct, lastSub, resulthist, PXSelect<GLSetup>.Select(this.Base)));
							resulthist = (GLHistory)resultset;
							resulthist.CuryYtdBalance = amt;
							return resultset;
						}
						if (ds.Expand == ExpandType.Account)
						{
							if (lastAcct != null)
							{
								for (int k = 0; k < Accounts.Count; k++)
								{
									if (Accounts[k].AccountID == lastAcct.AccountID)
									{
										dataSetCopy = new ARmDataSet(dataSetCopy);
										dataSetCopy[Keys.StartAccount] = dataSetCopy[Keys.EndAccount] = Accounts[k].AccountCD;
										splitret[k][2] = amt;
										splitret[k][3] = dataSetCopy;
										splitret[k][4] = bAccounts.FirstOrDefault(ba => ba.BAccountID == branchList.First(b => b.BranchID == branchID).BAccountID).AcctName;
										break;
									}
								}
							}
							return splitret;
						}
						return amt;
						#endregion
					}
					else
					{
						#region otherExpand
						PXResultset<GLHistoryByPeriod, Account, Sub, GLHistory, GLSetup> resultset = null;
						GLHistory resulthist = null;
						if (drilldown)
						{
							resultset = new PXResultset<GLHistoryByPeriod, Account, Sub, GLHistory, GLSetup>();
							resulthist = new GLHistory();
						}

						Account lastAcct = null;
						Sub lastSub = null;
						Branch lastBranch = null;
						decimal? lastYtd = 0m;
						decimal amt = 0m;
						int? branchID = null;
						for (int s = 0; s < Subs.Count; s++)
						{
							for (int a = 0; a < Accounts.Count; a++)
							{
								#region NetIncome
								// if NetIncome account
								if (ytdNetIncomeAccountID != null && Accounts[a].AccountID == ytdNetIncomeAccountID)
								{
									Periods = PeriodsNetIncome;
								}
								else
								{
									Periods = PeriodsAll;
								}
								#endregion
								for (int b = 0; b < branchList.Count; b++)
								{
									for (int p = 0; p < Periods.Count; p++)
									{
										if (BypassPL[p]
											&& (Accounts[a].Type == AccountType.Expense
											|| Accounts[a].Type == AccountType.Income))
										{
											continue;
										}
										glhistoryInstance.AccountID = Accounts[a].AccountID;
										glhistoryInstance.SubID = Subs[s].SubID;
										glhistoryInstance.FinPeriodID = Periods[p];
										glhistoryInstance.BranchID = branchList[b].BranchID;
										GLHistory hist = (GLHistory)glhistoryCache.Locate(glhistoryInstance);
										if (hist == null)
										{
											continue;
										}
										branchID = branchList[b].BranchID;
										if (ds.Expand == ExpandType.Sub && lastSub != null && lastSub.SubID != hist.SubID)
										{
											amt += (decimal)lastYtd;
											for (int k = 0; k < Subs.Count; k++)
											{
												if (Subs[k] == lastSub)
												{
													dataSetCopy = new ARmDataSet(dataSetCopy);
													dataSetCopy[Keys.StartSub] = dataSetCopy[Keys.EndSub] = Subs[k].SubCD;
													splitret[k][2] = amt;
													splitret[k][3] = dataSetCopy;
													splitret[k][4] = bAccounts.FirstOrDefault(ba => ba.BAccountID == branchList[b].BAccountID).AcctName;
													break;
												}
											}
											amt = 0m;
											lastYtd = 0m;
										}
										switch (ds.AmountType.Value)
										{
											case 1: //Turnover
												if (Accounts[a].Type == AccountType.Asset || Accounts[a].Type == AccountType.Expense)
												{
													amt += (decimal)(hist.PtdDebit - hist.PtdCredit);
												}
												else
												{
													amt += (decimal)(hist.PtdCredit - hist.PtdDebit);
												}
												break;
											case 2: //Credit
												amt += (decimal)hist.PtdCredit;
												break;
											case 3: //Debit
												amt += (decimal)hist.PtdDebit;
												break;
											case 4: //Beg Balance
												if (lastAcct == null || lastAcct.AccountID != hist.AccountID || lastSub == null || lastSub.SubID != hist.SubID || lastBranch == null || lastBranch.BranchID != hist.BranchID)
												{
													if (!takeLast)
													{
														amt += (decimal)hist.FinBegBalance;
													}
													else
													{
														amt += (decimal)lastYtd;
													}
												}
												if (takeLast)
												{
													if (p < Periods.Count - 1)
													{
														lastYtd = hist.FinYtdBalance;
													}
													else
													{
														lastYtd = hist.FinBegBalance;
													}
												}
												break;
											case 5: //End Balance
												if (lastAcct == null || lastAcct.AccountID != hist.AccountID || lastSub == null || lastSub.SubID != hist.SubID || lastBranch == null || lastBranch.BranchID != hist.BranchID)
												{
													amt += (decimal)lastYtd;
												}
												lastYtd = hist.FinYtdBalance;
												break;
										}

										if (drilldown)
										{
											if ((lastAcct != null && lastAcct.AccountID != hist.AccountID || lastSub != null && lastSub.SubID != hist.SubID || lastBranch == null || lastBranch.BranchID != hist.BranchID) &&
												(resulthist != null && resulthist.BranchID != null && resulthist.LedgerID != null && resulthist.AccountID != null && resulthist.SubID != null && resulthist.FinPeriodID != null))
											{
												GLHistoryByPeriod period = new GLHistoryByPeriod();
												period.LedgerID = resulthist.LedgerID;
												period.BranchID = resulthist.BranchID;
												period.AccountID = resulthist.AccountID;
												period.SubID = resulthist.SubID;
												period.FinPeriodID = resulthist.FinPeriodID;
												period.LastActivityPeriod = ds.AmountType.ToString() + ":" + Base.DrilldownNumber.ToString();
												resultset.Add(new PXResult<GLHistoryByPeriod, Account, Sub, GLHistory, GLSetup>(period, lastAcct, lastSub, resulthist, PXSelect<GLSetup>.Select(this.Base)));
												resulthist = new GLHistory();
											}
											resulthist.LedgerID = hist.LedgerID;
											resulthist.BranchID = hist.BranchID;
											resulthist.AccountID = hist.AccountID;
											resulthist.SubID = hist.SubID;
											resulthist.FinPeriodID = hist.FinPeriodID;
											if (resulthist.BegBalance == null)
											{
												resulthist.BegBalance = hist.BegBalance;
												resulthist.PtdDebit = 0m;
												resulthist.PtdCredit = 0m;
											}
											resulthist.PtdDebit += hist.PtdDebit;
											resulthist.PtdCredit += hist.PtdCredit;
											resulthist.YtdBalance = hist.YtdBalance;
										}
										lastAcct = Accounts[a];
										lastSub = Subs[s];
										lastBranch = branchList[b];
									}
								}
							}
						}
						amt += (decimal)lastYtd;

						if (drilldown && resulthist.LedgerID != null)
						{
							GLHistoryByPeriod period = new GLHistoryByPeriod();
							period.LedgerID = resulthist.LedgerID;
							period.BranchID = resulthist.BranchID;
							period.AccountID = resulthist.AccountID;
							period.SubID = resulthist.SubID;
							period.FinPeriodID = resulthist.FinPeriodID;
							period.LastActivityPeriod = ds.AmountType.ToString() + ":" + Base.DrilldownNumber.ToString();
							resultset.Add(new PXResult<GLHistoryByPeriod, Account, Sub, GLHistory, GLSetup>(period, lastAcct, lastSub, resulthist, PXSelect<GLSetup>.Select(this.Base)));
							resulthist = (GLHistory)resultset;
							resulthist.CuryYtdBalance = amt;
							return resultset;
						}

						if (ds.Expand == ExpandType.Sub)
						{
							if (lastSub != null)
							{
								for (int k = 0; k < Subs.Count; k++)
								{
									if (Subs[k].SubID == lastSub.SubID)
									{
										dataSetCopy = new ARmDataSet(dataSetCopy);
										dataSetCopy[Keys.StartSub] = dataSetCopy[Keys.EndSub] = Subs[k].SubCD;
										splitret[k][2] = amt;
										splitret[k][3] = dataSetCopy;
										splitret[k][4] = bAccounts.FirstOrDefault(ba => ba.BAccountID == branchList.First(b => b.BranchID == branchID).BAccountID).AcctName;
										break;
									}
								}
							}
							return splitret;
						}
						return amt;
						#endregion
					}
				}
				else
				{
					return Decimal.MinValue;
				}
				#endregion
			}
			else
			{
				return del(dataSet, drilldown);
			}
		}

		[PXOverride]
		public string GetUrl(Func<string> del)
		{
			string rmType = Base.Report.Current.Type;
			if (rmType == ARmReport.GL)
			{
				PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNodeByScreenID("CS600000");
				if (node != null)
				{
					return PX.Common.PXUrl.TrimUrl(node.Url);
				}
				throw new PXException(string.Format(ErrorMessages.GetLocal(ErrorMessages.NotEnoughRightsToAccessObject), "CS600000"));
			}
			else
			{
				return del();
			}
		}

		protected bool IsBetweenByChar(string from, string to, string value)
		{
			bool skipfrom = false;
			bool skipto = false;
			for (int i = 0; i < value.Length - 1; i++)
			{
				if (!skipfrom && i <= from.Length - 1 && !char.IsWhiteSpace(from[i]))
				{
					if (value[i] < from[i])
						return false;
					else if (value[i] > from[i])
						skipfrom = true;
				}
				if (!skipto && i <= to.Length - 1 && !char.IsWhiteSpace(to[i]))
				{
					if (value[i] > to[i])
						return false;
					else if (value[i] < to[i])
						skipto = true;
				}
			}
			return true;
		}
		#endregion

		#region IARmDataSource

		public enum Keys
		{
			AmountType,
			StartBranch,
			EndBranch,
			BookCode,
			EndAccount,
			EndSub,
			StartAccount,
			StartSub,
			AccountClass,
			EndOffset,
			EndYearOffset,
			EndPeriod,
			StartOffset,
			StartYearOffset,
			StartPeriod,
		}

		[PXOverride]
		public bool IsParameter(ARmDataSet ds, string name, ValueBucket value)
		{
			Keys key;
			if (Enum.TryParse<Keys>(name, true, out key) && Enum.IsDefined(typeof(Keys), key))
			{
				value.Value = ds[key];
				return true;
			}
			return false;
		}

		[PXOverride]
		public ARmDataSet MergeDataSet(IEnumerable<ARmDataSet> list, string expand, Func<IEnumerable<ARmDataSet>, string, ARmDataSet> del)
		{
			ARmDataSet dataSet = del(list, expand);

			bool isFirst = true;
			foreach (ARmDataSet set in list)
			{
				if (set == null) continue;
				if (string.IsNullOrEmpty(dataSet[Keys.StartBranch] as string ?? "")) dataSet[Keys.StartBranch] = set[Keys.StartBranch] as string ?? "";
				if (string.IsNullOrEmpty(dataSet[Keys.EndBranch] as string ?? "")) dataSet[Keys.EndBranch] = set[Keys.EndBranch] as string ?? "";
				if (string.IsNullOrEmpty(dataSet[Keys.BookCode] as string ?? "")) dataSet[Keys.BookCode] = set[Keys.BookCode] as string ?? "";

				if (!(dataSet[Keys.StartOffset] as int?).HasValue) dataSet[Keys.StartOffset] = (int?)set[Keys.StartOffset];
				if (!(dataSet[Keys.StartYearOffset] as int?).HasValue) dataSet[Keys.StartYearOffset] = (int?)set[Keys.StartYearOffset];
				if (!(dataSet[Keys.EndOffset] as int?).HasValue) dataSet[Keys.EndOffset] = (int?)set[Keys.EndOffset];
				if (!(dataSet[Keys.EndYearOffset] as int?).HasValue) dataSet[Keys.EndYearOffset] = (int?)set[Keys.EndYearOffset];
				if ((dataSet[Keys.AmountType] as short? ?? 0) == (short?)BalanceType.NotSet)
					dataSet[Keys.AmountType] = set[Keys.AmountType];

				dataSet[Keys.StartPeriod] = Base.MergeMask(dataSet[Keys.StartPeriod] as string ?? "", set[Keys.StartPeriod] as string ?? "");
				dataSet[Keys.EndPeriod] = Base.MergeMask(dataSet[Keys.EndPeriod] as string ?? "", set[Keys.EndPeriod] as string ?? "");

				if (expand == ExpandType.Account)
				{
					if (isFirst)
					{
						dataSet[Keys.StartAccount] = set[Keys.StartAccount] as string ?? "";
						dataSet[Keys.EndAccount] = set[Keys.EndAccount] as string ?? "";
						dataSet[Keys.AccountClass] = set[Keys.AccountClass] as string ?? "";
					}
				}
				else
				{
					dataSet[Keys.StartAccount] = Base.MergeMask(dataSet[Keys.StartAccount] as string ?? "", set[Keys.StartAccount] as string ?? "");
					dataSet[Keys.EndAccount] = Base.MergeMask(dataSet[Keys.EndAccount] as string ?? "", set[Keys.EndAccount] as string ?? "");
					if (string.IsNullOrEmpty(dataSet[Keys.AccountClass] as string ?? "")) dataSet[Keys.AccountClass] = set[Keys.AccountClass] as string ?? "";
				}

				if (expand == ExpandType.Sub)
				{
					if (isFirst)
					{
						dataSet[Keys.StartSub] = set[Keys.StartSub] as string ?? "";
						dataSet[Keys.EndSub] = set[Keys.EndSub] as string ?? "";
					}
				}
				else
				{
					dataSet[Keys.StartSub] = Base.MergeMask(dataSet[Keys.StartSub] as string ?? "", set[Keys.StartSub] as string ?? "");
					dataSet[Keys.EndSub] = Base.MergeMask(dataSet[Keys.EndSub] as string ?? "", set[Keys.EndSub] as string ?? "");
				}
				if (string.IsNullOrEmpty(dataSet.RowDescription)) dataSet.RowDescription = set.RowDescription;
				isFirst = false;
			}

			string Expand = list.First().Expand;
			if ((Expand == ExpandType.Account || Expand == ExpandType.Sub) && !(Expand == ExpandType.Account && ((!string.IsNullOrEmpty(dataSet[Keys.StartAccount] as string ?? "") && !string.IsNullOrEmpty(dataSet[Keys.EndAccount] as string ?? "")) ||
				!string.IsNullOrEmpty(dataSet[Keys.AccountClass] as string ?? "")) ||
				Expand == ExpandType.Sub && (!string.IsNullOrEmpty(dataSet[Keys.StartSub] as string ?? "") && !string.IsNullOrEmpty(dataSet[Keys.EndSub] as string ?? "")))
				)
				dataSet.Expand = ExpandType.Nothing;
			else
				dataSet.Expand = Expand;

			return dataSet;
		}

		[PXOverride]
		public void FillDataSource(RMDataSource ds, ARmDataSet dst, string rmType, Action<RMDataSource, ARmDataSet, string> del)
		{
			del(ds, dst, rmType);
			Filldatasource(ds, dst, rmType);
		}

		private void Filldatasource(RMDataSource ds, ARmDataSet dst, string rmType)
		{
			if (ds != null && ds.DataSourceID != null)
			{
				RMDataSourceGL dsGL = Base.Caches[typeof(RMDataSource)].GetExtension<RMDataSourceGL>(ds);
				dst[Keys.AmountType] = ds.AmountType;
				dst[Keys.StartBranch] = dsGL.StartBranch;
				dst[Keys.EndBranch] = dsGL.EndBranch;

				if (rmType == ARmReport.GL)
				{
					object ledger = Base.DataSourceByID.Cache.GetValueExt(ds, "LedgerID");
					if (ledger != null)
					{
						if (ledger is PXFieldState)
						{
							ledger = ((PXFieldState)ledger).Value;
							if (ledger is string)
							{
								dst[Keys.BookCode] = (string)ledger;
							}
							else if (ledger != null)
							{
								throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ElementDoesntExist, "LedgerID"));
							}
						}
						else if (ledger is string)
						{
							dst[Keys.BookCode] = (string)ledger;
						}
						else
						{
							throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ElementDoesntExist, "LedgerID"));
						}
					}
					dst[Keys.EndAccount] = dsGL.EndAccount;
					dst[Keys.EndSub] = dsGL.EndSub;
					dst[Keys.StartAccount] = dsGL.StartAccount;
					dst[Keys.StartSub] = dsGL.StartSub;
					dst[Keys.AccountClass] = dsGL.AccountClassID;

				}
				dst.Expand = ds.Expand;
				
				dst.RowDescription = ds.RowDescription;
				dst[Keys.EndOffset] = (int?)dsGL.EndPeriodOffset;
				dst[Keys.EndYearOffset] = (int?)dsGL.EndPeriodYearOffset;
				string per = dsGL.EndPeriod;

				if (!String.IsNullOrEmpty(per))
				{
					dst[Keys.EndPeriod] = (per.Length > 4 ? (per.Substring(4) + "  ").Substring(0, 2) : "  ") + (per.Length > 4 ? per.Substring(0, 4) : per);
				}

				dst[Keys.StartOffset] = (int?)dsGL.StartPeriodOffset;
				dst[Keys.StartYearOffset] = (int?)dsGL.StartPeriodYearOffset;
				per = dsGL.StartPeriod;

				if (!String.IsNullOrEmpty(per))
				{
					dst[Keys.StartPeriod] = (per.Length > 4 ? (per.Substring(4) + "  ").Substring(0, 2) : "  ") + (per.Length > 4 ? per.Substring(0, 4) : per);
				}
			}
		}

		[PXOverride]
		public ARmReport GetReport(Func<ARmReport> del)
		{
			ARmReport ar = del();

			int? id = Base.Report.Current.StyleID;
			if (id != null)
			{
				RMStyle st = Base.StyleByID.SelectSingle(id);
				Base.fillStyle(st, ar.Style);
			}

			id = Base.Report.Current.DataSourceID;
			if (id != null)
			{
				RMDataSource ds = Base.DataSourceByID.SelectSingle(id);
				Filldatasource(ds, ar.DataSet, ar.Type);
			}

			List<ARmReport.ARmReportParameter> aRp = ar.ARmParams;
			PXFieldState state;
			RMReportPM rPM = Base.Report.Cache.GetExtension<RMReportPM>(Base.Report.Current);
			string sViewName = string.Empty;
			string sInputMask = string.Empty;

			//StartBranch, EndBranch
			state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourceGL.startBranch>(null) as PXFieldState;
			if (state != null && !String.IsNullOrEmpty(state.ViewName))
			{
				sViewName = state.ViewName;
				if (state is PXStringState)
				{
					sInputMask = ((PXStringState)state).InputMask;
				}
			}
			//int colSpan = (rPM.RequestEndBranch ?? false) ? 1 : 2;
			int colSpan = 2;
			Base.CreateParameter(Keys.StartBranch, "StartBranch", Messages.GetLocal(Messages.StartBranchTitle), ar.DataSet[Keys.StartBranch] as string, rPM.RequestStartBranch ?? false, colSpan, sViewName, sInputMask, aRp);
			Base.CreateParameter(Keys.EndBranch, "EndBranch", Messages.GetLocal(Messages.EndBranchTitle), ar.DataSet[Keys.EndBranch] as string, rPM.RequestEndBranch ?? false, colSpan, sViewName, sInputMask, aRp);

			bool bSinglePeriod = false;
			bool bRequestEndPeriod = false;
			short? endperreq = rPM.RequestEndPeriod ?? 0;
			if (endperreq > 0)
			{
				bRequestEndPeriod = true;
				if (endperreq == 2)
				{
					bSinglePeriod = true;
				}
			}

			bool bRequestStartPeriod = rPM.RequestStartPeriod ?? false;

			if (bRequestStartPeriod && (ar.DataSet[RMReportReaderGL.Keys.StartPeriod] as string ?? "").TrimEnd() == "")
			{
				try
				{
					ar.DataSet[RMReportReaderGL.Keys.StartPeriod] = (string)((ArmDATA)Base.GetExprContext()).GetDefExt("RowBatch.FinPeriodID");
				}
				catch
				{
				}
			}
			if (bRequestEndPeriod && (ar.DataSet[RMReportReaderGL.Keys.EndPeriod] as string ?? "").TrimEnd() == "")
			{
				try
				{
					ar.DataSet[RMReportReaderGL.Keys.EndPeriod] = (string)((ArmDATA)Base.GetExprContext()).GetDefExt("RowBatch.FinPeriodID");
				}
				catch
				{
				}
			}

			string sViewNameStartEndPeriod = string.Empty;
			string sInputMaskStartEndPeriod = string.Empty;

			state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourceGL.startPeriod>(null) as PXFieldState;
			if (state != null && !String.IsNullOrEmpty(state.ViewName))
			{
				sViewNameStartEndPeriod = state.ViewName;
				if (state is PXStringState)
				{
					sInputMaskStartEndPeriod = ((PXStringState)state).InputMask;
				}
			}

			if (ar.Type == ARmReport.GL)
			{
				RMReportGL rGL = Base.Report.Cache.GetExtension<RMReportGL>(Base.Report.Current);

				//BookCode
				sViewName = sInputMask = string.Empty;
				state = Base.DataSourceByID.Cache.GetStateExt(null, "LedgerID") as PXFieldState;
				if (state != null && !String.IsNullOrEmpty(state.ViewName))
				{
					sViewName = state.ViewName;
					if (state is PXStringState)
					{
						sInputMask = ((PXStringState)state).InputMask;
					}
				}
				Base.CreateParameter(Keys.BookCode, "BookCode", Messages.GetLocal(Messages.BookCodeTitle), ar.DataSet[Keys.BookCode] as string, rGL.RequestLedgerID ?? false, 2, sViewName, sInputMask, aRp);

				//StartPeriod, EndPeriod 																		
				bool endPeriod = (!bSinglePeriod && bRequestEndPeriod);
				string startLabel = endPeriod ? Messages.StartPeriodTitle : Messages.PeriodTitle;
				//colSpan = endPeriod ? 1 : 2;
				colSpan = 2;
				if (endPeriod)
				{
					Base.CreateParameter(Keys.StartPeriod, "StartPeriod", Messages.GetLocal(startLabel), ar.DataSet[Keys.StartPeriod] as string, bRequestStartPeriod, colSpan, sViewNameStartEndPeriod, sInputMaskStartEndPeriod, aRp);
					Base.CreateParameter(Keys.EndPeriod, "EndPeriod", Messages.GetLocal(Messages.EndPeriodTitle), ar.DataSet[Keys.EndPeriod] as string, bRequestEndPeriod, colSpan, sViewNameStartEndPeriod, sInputMaskStartEndPeriod, aRp);
				}
				else
				{
					Base.CreateParameter(new object[] { Keys.StartPeriod, Keys.EndPeriod }, "StartPeriod", Messages.GetLocal(startLabel), ar.DataSet[Keys.StartPeriod] as string, bRequestStartPeriod, colSpan, sViewNameStartEndPeriod, sInputMaskStartEndPeriod, aRp);
				}

				// StartAccount, EndAccount
				bool bRequestEndAccount = rGL.RequestEndAccount ?? false;
				sViewName = sInputMask = string.Empty;
				state = Base.DataSourceByID.Cache.GetStateExt(null, "StartAccount") as PXFieldState;
				if (state != null && !String.IsNullOrEmpty(state.ViewName))
				{
					sViewName = state.ViewName;
					if (state is PXStringState)
					{
						sInputMask = ((PXStringState)state).InputMask;
					}
				}
				//colSpan = bRequestEndAccount ? 1 : 2;
				colSpan = 2;
				Base.CreateParameter(Keys.StartAccount, "StartAccount", Messages.GetLocal(Messages.StartAccTitle), ar.DataSet[Keys.StartAccount] as string, rGL.RequestStartAccount ?? false, colSpan, sViewName, sInputMask, aRp);
				Base.CreateParameter(Keys.EndAccount, "EndAccount", Messages.GetLocal(Messages.EndAccTitle), ar.DataSet[Keys.EndAccount] as string, bRequestEndAccount, colSpan, sViewName, sInputMask, aRp);

				// StartSub, EndSub
				bool bRequestEndSub = rGL.RequestEndSub ?? false;
				sViewName = sInputMask = string.Empty;
				state = Base.Report.Cache.GetStateExt(null, "SubCD") as PXFieldState;
				if (state != null && !String.IsNullOrEmpty(state.ViewName))
				{
					sViewName = state.ViewName;
					if (state is PXStringState)
					{
						sInputMask = ((PXStringState)state).InputMask;
					}
				}
				//colSpan = bRequestEndSub ? 1 : 2;
				colSpan = 2;
				Base.CreateParameter(Keys.StartSub, "StartSub", Messages.GetLocal(Messages.StartSubTitle), ar.DataSet[Keys.StartSub] as string, rGL.RequestStartSub ?? false, colSpan, sViewName, sInputMask, aRp);
				Base.CreateParameter(Keys.EndSub, "EndSub", Messages.GetLocal(Messages.EndSubTitle), ar.DataSet[Keys.EndSub] as string, bRequestEndSub, colSpan, sViewName, sInputMask, aRp);

				// AccountClass
				bool bRequestAccountClassID = rGL.RequestAccountClassID ?? false;
				sViewName = sInputMask = string.Empty;
				state = Base.DataSourceByID.Cache.GetStateExt(null, "AccountClassID") as PXFieldState;
				if (state != null && !String.IsNullOrEmpty(state.ViewName))
				{
					sViewName = state.ViewName;
					if (state is PXStringState)
					{
						sInputMask = ((PXStringState)state).InputMask;
					}
				}
				//colSpan = bRequestAccountClassID ? 1 : 2;
				colSpan = 2;
				Base.CreateParameter(Keys.AccountClass, "AccountClass", Messages.GetLocal(Messages.AccountClassTitle), ar.DataSet[Keys.AccountClass] as string, bRequestAccountClassID, colSpan, sViewName, sInputMask, aRp);
			}
			else
			{
				// StartPeriod, EndPeriod
				bool endPeriod = (!bSinglePeriod && bRequestEndPeriod);
				string startLabel = endPeriod ? Messages.StartPeriodTitle : Messages.PeriodTitle;
				//colSpan = endPeriod ? 1 : 2;
				colSpan = 2;
				if (endPeriod)
				{
					Base.CreateParameter(Keys.StartPeriod, "StartPeriod", Messages.GetLocal(startLabel), ar.DataSet[Keys.StartPeriod] as string, bRequestStartPeriod, colSpan, sViewNameStartEndPeriod, sInputMaskStartEndPeriod, aRp);
					Base.CreateParameter(Keys.EndPeriod, "EndPeriod", Messages.GetLocal(Messages.EndPeriodTitle), ar.DataSet[Keys.EndPeriod] as string, bRequestEndPeriod, colSpan, sViewNameStartEndPeriod, sInputMaskStartEndPeriod, aRp);
				}
				else
				{
					Base.CreateParameter(new object[] { Keys.StartPeriod, Keys.EndPeriod }, "StartPeriod", Messages.GetLocal(startLabel), ar.DataSet[Keys.StartPeriod] as string, bRequestStartPeriod, colSpan, sViewNameStartEndPeriod, sInputMaskStartEndPeriod, aRp);
				}
			}
			return ar;
		}
		#endregion
	}

	public class ArmDATA : PX.Data.Reports.SoapNavigator.DATA
	{
		public object FormatPeriod(object period)
		{
			if (period is string)
			{
				period = ((string)period).Replace("-", "");
			}
			return ExtToUI("RowBatch.FinPeriodID", period);
		}
		public object FormatPeriod(object period, object shift)
		{
			try
			{
				string per = ((string)period).Replace("-", "");
				int j = Convert.ToInt32(shift);
				int k = (int)((FinYearSetup)PXSelect<FinYearSetup>.Select(_Graph)).FinPeriods;
				int res = int.Parse(per.Substring(2)) * k + int.Parse(per.Substring(0, 2)) + j;
				int resMonth = res % k;
				int resYear;
				if (resMonth == 0)
				{
					if (j > 0)
					{
						resMonth = 1;
						resYear = res / k;
					}
					else
					{
						resMonth = k;
						resYear = res / k - 1;
					}
				}
				else
				{
					resYear = res / k;
				}
				per = String.Format("{0:00}{1:0000}", resMonth, resYear);
				return FormatPeriod(per);
			}
			catch
			{
				return FormatPeriod(period);
			}
		}
		public object FormatPeriod(object period, object shiftYear, object shiftPeriod)
		{
			try
			{
				string per = ((string)period).Replace("-", "");
				//shift year
				int year = int.Parse(per.Substring(2));
				if (shiftYear != null && (int)shiftYear != 0)
				{
					year += (int)shiftYear;
				}
				//shift period
				per = per.Substring(0, 2) + year.ToString();
				int j = Convert.ToInt32(shiftPeriod);
				int k = (int)((FinYearSetup)PXSelect<FinYearSetup>.Select(_Graph)).FinPeriods;
				int res = int.Parse(per.Substring(2)) * k + int.Parse(per.Substring(0, 2)) + j;
				int resMonth = res % k;
				int resYear;
				if (resMonth == 0)
				{
					if (j > 0)
					{
						resMonth = 1;
						resYear = res / k;
					}
					else
					{
						resMonth = k;
						resYear = res / k - 1;
					}
				}
				else
				{
					resYear = res / k;
				}
				per = String.Format("{0:00}{1:0000}", resMonth, resYear);
				//Asked to no formatting
				return per;
			}
			catch
			{
				return FormatPeriod(period);
			}
		}		
		public object FormatYear(object period)
		{
			string per = FormatPeriod(period) as string;
			if (!String.IsNullOrEmpty(per))
			{
				int i = per.IndexOf("-");
				if (i >= 0 && i < per.Length - 1)
				{
					return per.Substring(i + 1);
				}
			}

			return per;
		}
		public object FormatYear(object period, object shift)
		{
			try
			{
				int j = Convert.ToInt32(shift);
				int i = int.Parse((string)FormatYear(period));
				return String.Format("{0:0000}", i + j);
			}
			catch
			{
				return FormatYear(period);
			}
		}
		public object GetNumberOfPeriods(object startPeriod, object endPeriod)
		{
			try
			{
				IEnumerable<PXDataRecord> record = PXDatabase.SelectMulti<FinPeriod>(
						new PXDataField<FinPeriod.finPeriodID>(),
						new PXDataFieldValue<FinPeriod.finPeriodID>(PXDbType.Char, 6, FormatForStoring(startPeriod as string), PXComp.GE),
						new PXDataFieldValue<FinPeriod.finPeriodID>(PXDbType.Char, 6, FormatForStoring(endPeriod as string), PXComp.LE),
						new PXDataFieldOrder<FinPeriod.finPeriodID>());
				{
					if (record != null)
						return record.Count<PXDataRecord>() - 1;
				}
			}
			catch
			{ }
			return null;
		}
		public object GetPeriodStartDate(object period)
		{
			try
			{
				var p = FormatForStoring(period as string);
				if (p != null)
				{
					using (PXDataRecord record = PXDatabase.SelectSingle<FinPeriod>(
						new PXDataField<FinPeriod.startDate>(),
						new PXDataFieldValue<FinPeriod.finPeriodID>(PXDbType.Char, 6, p)
						))
					{
						if (record != null)
						{
							return record.GetDateTime(0);
						}
					}
				}
			}
			catch
			{
			}
			return null;
		}
		public object GetPeriodEndDate(object period)
		{
			try
			{
				var p = FormatForStoring(period as string);
				if (p != null)
				{
					using (PXDataRecord record = PXDatabase.SelectSingle<FinPeriod>(
						new PXDataField<FinPeriod.endDate>(),
						new PXDataField<FinPeriod.startDate>(),
						new PXDataFieldValue<FinPeriod.finPeriodID>(PXDbType.Char, 6, p)
						))
					{
						if (record != null)
						{
							bool isEmpty = (record.GetDateTime(0).HasValue && record.GetDateTime(1) == record.GetDateTime(0));
							return ((record.GetDateTime(0).HasValue && !isEmpty) ? record.GetDateTime(0).Value.AddDays(-1) : record.GetDateTime(0));
						}
					}
				}
			}
			catch
			{
			}
			return null;
		}
		public object GetPeriodDescription(object period)
		{
			try
			{
				var p = FormatForStoring(period as string);
				if (p != null)
				{
					using (PXDataRecord record = PXDatabase.SelectSingle<FinPeriod>(
						new PXDataField<FinPeriod.descr>(),
						new PXDataFieldValue<FinPeriod.finPeriodID>(PXDbType.Char, 6, p)
						))
					{
						if (record != null)
						{
							return record.GetString(0);
						}
					}
				}
			}
			catch
			{
			}
			return null;
		}
		private static string FormatForStoring(string period)
		{
			if (string.IsNullOrEmpty(period))
				return null;
			if (period.Trim().Length != FiscalPeriodUtils.PERIOD_LENGTH + FiscalPeriodUtils.YEAR_LENGTH)
				return period;
			return period.Substring(FiscalPeriodUtils.PERIOD_LENGTH, FiscalPeriodUtils.YEAR_LENGTH) + period.Substring(0, FiscalPeriodUtils.PERIOD_LENGTH);
		}

	}
}
