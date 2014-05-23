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
using PX.Reports.Drawing;
using PX.CS;


namespace PX.Objects.CS
{
	public class RMReportReaderPM : PXGraphExtension<RMReportReaderGL, RMReportReader>
	{
		#region report
		protected PMHistory pmhistoryInstance;
		protected PMAccountGroup accountGroup;
		protected PMProject project;
		protected PMTask task;
		protected InventoryItem item;

		protected string accountGroupWildcard;
		protected string projectWildcard;
		protected string itemWildcard;
		protected PXCache pmhistoryCache;
		protected PXCache AccountGroups;
		protected PXCache Projects;
		protected PXCache ProjectTasks;
		protected PXCache Items;

		public virtual void PMEnsureInitialized()
		{
			if (Base1.taskWildcard == null)
			{
				Dimension dim = PXSelect<Dimension,
					Where<Dimension.dimensionID, Equal<Required<Dimension.dimensionID>>>>
					.Select(this.Base, PM.ProjectTaskAttribute.DimensionName);
				if (dim != null && dim.Length != null)
				{
					Base1.taskWildcard = new String('_', (int)dim.Length);
				}
				else
				{
					Base1.taskWildcard = "";
				}

				dim = PXSelect<Dimension,
					Where<Dimension.dimensionID, Equal<Required<Dimension.dimensionID>>>>
					.Select(this.Base, GL.BranchAttribute._DimensionName);
				if (dim != null && dim.Length != null)
				{
					Base1.branchWildcard = new String('_', (int)dim.Length);
				}
				else
				{
					Base1.branchWildcard = "";
				}

				dim = PXSelect<Dimension,
					Where<Dimension.dimensionID, Equal<Required<Dimension.dimensionID>>>>
					.Select(this.Base, PM.AccountGroupAttribute.DimensionName);
				if (dim != null && dim.Length != null)
				{
					accountGroupWildcard = new String('_', (int)dim.Length);
				}
				else
				{
					accountGroupWildcard = "";
				}

				dim = PXSelect<Dimension,
					Where<Dimension.dimensionID, Equal<Required<Dimension.dimensionID>>>>
					.Select(this.Base, PM.ProjectAttribute.DimensionName);
				if (dim != null && dim.Length != null)
				{
					projectWildcard = new String('_', (int)dim.Length);
				}
				else
				{
					projectWildcard = "";
				}

				dim = PXSelect<Dimension,
					Where<Dimension.dimensionID, Equal<Required<Dimension.dimensionID>>>>
					.Select(this.Base, IN.InventoryAttribute.DimensionName);
				if (dim != null && dim.Length != null)
				{
					itemWildcard = new String('_', (int)dim.Length);
				}
				else
				{
					itemWildcard = "";
				}

				PXSelectOrderBy<FinPeriod, OrderBy<Asc<FinPeriod.startDate>>>.Select(this.Base);
				Base1.finPeriodCache = Base.Caches[typeof(FinPeriod)];
				Base1.finPeriods = new List<FinPeriod>();
				foreach (FinPeriod per in Base1.finPeriodCache.Cached)
				{
					Base1.finPeriods.Add(per);
				}

				PXSelect<PMHistory>.Select(this.Base);
				pmhistoryInstance = new PMHistory();
				pmhistoryCache = Base.Caches[typeof(PMHistory)];
				PXSelect<PMAccountGroup>.Select(this.Base);
				accountGroup = new PMAccountGroup();
				AccountGroups = Base.Caches[typeof(PMAccountGroup)];
				PXSelect<PMProject, Where<PMProject.isTemplate, Equal<False>, And<PMProject.nonProject, Equal<False>, And<PMProject.baseType, Equal<PMProject.ProjectBaseType>>>>>.Select(this.Base);
				project = new PMProject();
				Projects = Base.Caches[typeof(PMProject)];
				PXSelectJoin<PMTask, InnerJoin<PMProject, On<PMTask.projectID, Equal<PMProject.contractID>>>, Where<PMProject.isTemplate, Equal<False>, And<PMProject.nonProject, Equal<False>>>>.Select(this.Base);
				task = new PMTask();
				ProjectTasks = Base.Caches[typeof(PMTask)];
				PXSelect<InventoryItem>.Select(this.Base);
				item = new InventoryItem();
				Items = Base.Caches[typeof(InventoryItem)];
				PXSelect<FinPeriod>.Select(this.Base);
				Base1.finPeriodCache = Base.Caches[typeof(FinPeriod)];

				//Add Inventory <OTHER> with InventoryID=0
				InventoryItem other = new InventoryItem();
				other.InventoryCD = "<OTHER>";
				other.InventoryID = PMInventorySelectorAttribute.EmptyInventoryID;
				other.Descr = "Unspecified items";
				Items.Insert(other);
			}
		}

		protected virtual void NormalizeDataSource(RMDataSourcePM dsPM)
		{
			if (dsPM.StartAccountGroup != null && dsPM.StartAccountGroup.TrimEnd() == "")
			{
				dsPM.StartAccountGroup = null;
			}
			if (dsPM.EndAccountGroup != null && dsPM.EndAccountGroup.TrimEnd() == "")
			{
				dsPM.EndAccountGroup = null;
			}
			if (dsPM.StartProject != null && dsPM.StartProject.TrimEnd() == "")
			{
				dsPM.StartProject = null;
			}
			if (dsPM.EndProject != null && dsPM.EndProject.TrimEnd() == "")
			{
				dsPM.EndProject = null;
			}
			if (dsPM.StartProjectTask != null && dsPM.StartProjectTask.TrimEnd() == "")
			{
				dsPM.StartProjectTask = null;
			}
			if (dsPM.EndProjectTask != null && dsPM.EndProjectTask.TrimEnd() == "")
			{
				dsPM.EndProjectTask = null;
			}
			if (dsPM.StartInventory != null && dsPM.StartInventory.TrimEnd() == "")
			{
				dsPM.StartInventory = null;
			}
			if (dsPM.EndInventory != null && dsPM.EndInventory.TrimEnd() == "")
			{
				dsPM.EndInventory = null;
			}
		}

		[PXOverride]
		public virtual object GetHistoryValue(ARmDataSet dataSet, bool drilldown, Func<ARmDataSet, bool, object> del)
		{
			string rmType = Base.Report.Current.Type;
			if (rmType == ARmReport.PM)
			{
				ARmDataSet dataSetCopy = new ARmDataSet(dataSet);

				RMDataSource ds = Base.DataSourceByID.Current;

				RMDataSourcePM dsPM = Base.Caches[typeof(RMDataSource)].GetExtension<RMDataSourcePM>(ds);

				ds.AmountType = (short?)dataSet[RMReportReaderGL.Keys.AmountType];

				dsPM.StartAccountGroup = dataSet[Keys.StartAccountGroup] as string ?? "";
				dsPM.EndAccountGroup = dataSet[Keys.EndAccountGroup] as string ?? "";
				dsPM.StartProject = dataSet[Keys.StartProject] as string ?? "";
				dsPM.EndProject = dataSet[Keys.EndProject] as string ?? "";
				dsPM.StartProjectTask = dataSet[Keys.StartProjectTask] as string ?? "";
				dsPM.EndProjectTask = dataSet[Keys.EndProjectTask] as string ?? "";
				dsPM.StartInventory = dataSet[Keys.StartInventory] as string ?? "";
				dsPM.EndInventory = dataSet[Keys.EndInventory] as string ?? "";

				RMDataSourceGL dsGL = Base.Caches[typeof(RMDataSource)].GetExtension<RMDataSourceGL>(ds);

				dsGL.StartBranch = dataSet[RMReportReaderGL.Keys.StartBranch] as string ?? "";
				dsGL.EndBranch = dataSet[RMReportReaderGL.Keys.EndBranch] as string ?? "";
				dsGL.EndPeriod = ((dataSet[RMReportReaderGL.Keys.EndPeriod] as string ?? "").Length > 2 ? ((dataSet[RMReportReaderGL.Keys.EndPeriod] as string ?? "").Substring(2) + "    ").Substring(0, 4) : "    ") + ((dataSet[RMReportReaderGL.Keys.EndPeriod] as string ?? "").Length > 2 ? (dataSet[RMReportReaderGL.Keys.EndPeriod] as string ?? "").Substring(0, 2) : dataSet[RMReportReaderGL.Keys.EndPeriod] as string ?? "");
				dsGL.EndPeriodOffset = (short?)(int?)dataSet[RMReportReaderGL.Keys.EndOffset];
				dsGL.EndPeriodYearOffset = (short?)(int?)dataSet[RMReportReaderGL.Keys.EndYearOffset];
				dsGL.StartPeriod = ((dataSet[RMReportReaderGL.Keys.StartPeriod] as string ?? "").Length > 2 ? ((dataSet[RMReportReaderGL.Keys.StartPeriod] as string ?? "").Substring(2) + "    ").Substring(0, 4) : "    ") + ((dataSet[RMReportReaderGL.Keys.StartPeriod] as string ?? "").Length > 2 ? (dataSet[RMReportReaderGL.Keys.StartPeriod] as string ?? "").Substring(0, 2) : dataSet[RMReportReaderGL.Keys.StartPeriod] as string ?? "");
				dsGL.StartPeriodOffset = (short?)(int?)dataSet[RMReportReaderGL.Keys.StartOffset];
				dsGL.StartPeriodYearOffset = (short?)(int?)dataSet[RMReportReaderGL.Keys.StartYearOffset];

				PMEnsureInitialized();

				List<PMAccountGroup> agList = new List<PMAccountGroup>();
				List<PMProject> projectList = new List<PMProject>();
				int[] projectIdsArray;

				List<PMTask> taskList = new List<PMTask>();
				List<InventoryItem> itemList = new List<InventoryItem>();
				List<string> periodList = new List<string>();

				if (ds.AmountType == null || ds.AmountType == 0)
				{
					return 0m;
				}

				List<object[]> splitret = null;
				if (ds.Expand != ExpandType.Nothing)
				{
					splitret = new List<object[]>();
				}

				NormalizeDataSource(dsPM);

				#region Filter Account Groups
				if (dsPM.StartAccountGroup != null)
				{
					string ag = (dsPM.StartAccountGroup.Replace(' ', '_').Replace('?', '_') + accountGroupWildcard).Substring(0, accountGroupWildcard.Length);
					if (dsPM.EndAccountGroup == null || dsPM.EndAccountGroup == dsPM.StartAccountGroup)
					{
						if (!ag.Contains("_"))
						{
							accountGroup.GroupCD = ag;
							PMAccountGroup a = (PMAccountGroup)AccountGroups.Locate(accountGroup);
							if (a != null)
							{
								agList.Add(a);
								if (ds.Expand == ExpandType.AccountGroup)
								{
									splitret.Add(new object[] { a.GroupCD, a.Description, 0m, null, string.Empty });
								}
							}
						}
						else
						{
							foreach (PMAccountGroup a in AccountGroups.Cached)
							{
								if (Base.IsLike(ag, a.GroupCD))
								{
									agList.Add(a);
									if (ds.Expand == ExpandType.AccountGroup)
									{
										splitret.Add(new object[] { a.GroupCD, a.Description, 0m, null, string.Empty });
									}
								}
							}
						}
					}
					else
					{
						ag = ag.Replace('_', ' ');
						string toag = (dsPM.EndAccountGroup.Replace(' ', 'z').Replace('?', 'z') + new String('z', accountGroupWildcard.Length)).Substring(0, accountGroupWildcard.Length);
						foreach (PMAccountGroup a in AccountGroups.Cached)
						{
							if (Base.IsBetween(ag, toag, a.GroupCD))
							{
								agList.Add(a);
								if (ds.Expand == ExpandType.AccountGroup)
								{
									splitret.Add(new object[] { a.GroupCD, a.Description, 0m, null, string.Empty });
								}
							}
						}
					}
				}
				else
				{
					foreach (PMAccountGroup a in AccountGroups.Cached)
					{
						agList.Add(a);
						if (ds.Expand == ExpandType.AccountGroup)
						{
							splitret.Add(new object[] { a.GroupCD, a.Description, 0m, null, string.Empty });
						}
					}
				}
				#endregion
				#region Filter Projects

				List<int> projectListIds = new List<int>();//filled only if subset. If all projects are selected this list is empty. 

				if (dsPM.StartProject != null)
				{
					string proj = (dsPM.StartProject.Replace(' ', '_').Replace('?', '_') + projectWildcard).Substring(0, projectWildcard.Length);
					if (dsPM.EndProject == null || dsPM.EndProject == dsPM.StartProject)
					{
						if (!proj.Contains("_"))
						{
							project.ContractCD = proj;
							PMProject p = (PMProject)Projects.Locate(project);
							if (p != null)
							{
								projectList.Add(p);
								projectListIds.Add(p.ContractID.Value);
								if (ds.Expand == ExpandType.Project)
								{
									splitret.Add(new object[] { p.ContractCD, p.Description, 0m, null, string.Empty });
								}
							}
						}
						else
						{
							foreach (PMProject p in Projects.Cached)
							{
								if (Base.IsLike(proj, p.ContractCD))
								{
									projectList.Add(p);
									projectListIds.Add(p.ContractID.Value);
									if (ds.Expand == ExpandType.Project)
									{
										splitret.Add(new object[] { p.ContractCD, p.Description, 0m, null, string.Empty });
									}
								}
							}
						}
					}
					else
					{
						proj = proj.Replace('_', ' ');
						string toproj = (dsPM.EndProject.Replace(' ', 'z').Replace('?', 'z') + new String('z', projectWildcard.Length)).Substring(0, projectWildcard.Length);
						foreach (PMProject p in Projects.Cached)
						{
							if (Base.IsBetween(proj, toproj, p.ContractCD))
							{
								projectList.Add(p);
								projectListIds.Add(p.ContractID.Value);
								if (ds.Expand == ExpandType.Project)
								{
									splitret.Add(new object[] { p.ContractCD, p.Description, 0m, null, string.Empty });
								}
							}
						}
					}
				}
				else
				{
					foreach (PMProject p in Projects.Cached)
					{
						projectList.Add(p);
						if (ds.Expand == ExpandType.Project)
						{
							splitret.Add(new object[] { p.ContractCD, p.Description, 0m, null, string.Empty });
						}
					}
				}

				projectIdsArray = projectListIds.ToArray();
				Comparer<int> comparer = Comparer<int>.Default;
				Array.Sort<int>(projectIdsArray, comparer);

				#endregion
				#region Filter Project Tasks
				if (dsPM.StartProjectTask != null)
				{
					string pt = (dsPM.StartProjectTask.Replace(' ', '_').Replace('?', '_') + Base1.taskWildcard).Substring(0, Base1.taskWildcard.Length);
					if (dsPM.EndProjectTask == null || dsPM.EndProjectTask == dsPM.StartProjectTask)
					{
						if (!pt.Contains("_"))
						{
							//foreach (PMProject p in Projects.Cached)
							foreach (PMProject p in projectList)
							{
								task.ProjectID = p.ContractID;
								task.TaskCD = pt;
								PMTask t = (PMTask)ProjectTasks.Locate(task);
								if (t != null)
								{
									taskList.Add(t);
									if (ds.Expand == ExpandType.ProjectTask)
									{
										splitret.Add(new object[] { t.TaskCD, t.Description, 0m, null, string.Empty });
									}
								}
							}
						}
						else
						{
							foreach (PMTask t in ProjectTasks.Cached)
							{
								if (Base.IsLike(pt, t.TaskCD))
								{
									bool addToList = false;

									if (projectIdsArray.Length == 0 && projectList.Count > 0)
										addToList = true;
									else if (Array.BinarySearch<int>(projectIdsArray, t.ProjectID.Value, comparer) >= 0)
										addToList = true;

									if (addToList)
									{
										taskList.Add(t);
										if (ds.Expand == ExpandType.ProjectTask)
										{
											splitret.Add(new object[] { t.TaskCD, t.Description, 0m, null, string.Empty });
										}
									}
								}

							}
						}
					}
					else
					{
						pt = pt.Replace('_', ' ');
						string topt = (dsPM.EndProjectTask.Replace(' ', 'z').Replace('?', 'z') + new String('z', Base1.taskWildcard.Length)).Substring(0, Base1.taskWildcard.Length);
						foreach (PMTask t in ProjectTasks.Cached)
						{
							if (Base.IsBetween(pt, topt, t.TaskCD))
							{
								bool addToList = false;

								if (projectIdsArray.Length == 0 && projectList.Count > 0)
									addToList = true;
								else if (Array.BinarySearch<int>(projectIdsArray, t.ProjectID.Value, comparer) >= 0)
									addToList = true;

								if (addToList)
								{
									taskList.Add(t);
									if (ds.Expand == ExpandType.ProjectTask)
									{
										splitret.Add(new object[] { t.TaskCD, t.Description, 0m, null, string.Empty });
									}
								}
							}
						}
					}
				}
				else
				{
					foreach (PMTask t in ProjectTasks.Cached)
					{
						bool addToList = false;

						if (projectIdsArray.Length == 0 && projectList.Count > 0)
							addToList = true;
						else if (Array.BinarySearch<int>(projectIdsArray, t.ProjectID.Value, comparer) >= 0)
							addToList = true;

						if (addToList)
						{
							taskList.Add(t);
							if (ds.Expand == ExpandType.ProjectTask)
							{
								splitret.Add(new object[] { t.TaskCD, t.Description, 0m, null, string.Empty });
							}
						}
					}
				}
				#endregion
				#region Filter Items
				if (dsPM.StartInventory != null)
				{
					string inv = (dsPM.StartInventory.Replace(' ', '_').Replace('?', '_') + itemWildcard).Substring(0, itemWildcard.Length);
					if (dsPM.EndInventory == null || dsPM.EndInventory == dsPM.StartInventory)
					{
						if (!inv.Contains("_"))
						{
							item.InventoryCD = inv;
							InventoryItem i = (InventoryItem)Items.Locate(item);
							if (i != null)
							{
								itemList.Add(i);
								if (ds.Expand == ExpandType.Inventory)
								{
									splitret.Add(new object[] { i.InventoryCD, i.Descr, 0m, null, string.Empty });
								}
							}
						}
						else
						{
							foreach (InventoryItem i in Items.Cached)
							{
								if (Base.IsLike(inv, i.InventoryCD))
								{
									itemList.Add(i);
									if (ds.Expand == ExpandType.Inventory)
									{
										splitret.Add(new object[] { i.InventoryCD, i.Descr, 0m, null, string.Empty });
									}
								}
							}
						}
					}
					else
					{
						inv = inv.Replace('_', ' ');
						string toinv = (dsPM.EndInventory.Replace(' ', 'z').Replace('?', 'z') + new String('z', itemWildcard.Length)).Substring(0, itemWildcard.Length);
						foreach (InventoryItem i in Items.Cached)
						{
							if (Base.IsBetween(inv, toinv, i.InventoryCD))
							{
								itemList.Add(i);
								if (ds.Expand == ExpandType.Inventory)
								{
									splitret.Add(new object[] { i.InventoryCD, i.Descr, 0m, null, string.Empty });
								}
							}
						}
					}
				}
				else
				{
					foreach (InventoryItem i in Items.Cached)
					{
						itemList.Add(i);
						if (ds.Expand == ExpandType.Inventory)
						{
							splitret.Add(new object[] { i.InventoryCD, i.Descr, 0m, null, string.Empty });
						}
					}
				}
				#endregion
				#region Filter FinPeriods
				if (dsGL.StartPeriod != null)
				{
					string per = (dsGL.StartPeriod.Replace(' ', '_').Replace('?', '_') + Base1.perWildcard).Substring(0, Base1.perWildcard.Length);
					if (!per.Contains("_") && dsGL.StartPeriodOffset != null && dsGL.StartPeriodOffset != 0)
					{
						per = Base1.GetFinPeriod(per, dsGL.StartPeriodYearOffset, dsGL.StartPeriodOffset);
					}
					if (dsGL.EndPeriod == null)
					{
						if (!per.Contains("_"))
						{
							periodList.Add(per);
						}
						else
						{
							foreach (FinPeriod p in Base1.finPeriodCache.Cached)
							{
								if (Base.IsLike(per, p.FinPeriodID))
								{
									periodList.Add(p.FinPeriodID);
								}
							}
						}
					}
					else
					{
						per = per.Replace('_', '0');
						string toper = (dsGL.EndPeriod.Replace(' ', '_').Replace('?', '_') + Base1.perWildcard).Substring(0, Base1.perWildcard.Length);
						if (!toper.Contains("_") && dsGL.EndPeriodOffset != null && dsGL.EndPeriodOffset != 0)
						{
							toper = Base1.GetFinPeriod(toper, 0, dsGL.EndPeriodOffset);
						}
						toper = toper.Replace('_', '9');
						foreach (FinPeriod p in Base1.finPeriodCache.Cached)
						{
							if (Base.IsBetween(per, toper, p.FinPeriodID))
							{
								periodList.Add(p.FinPeriodID);
							}
						}
					}
				}
				else
				{
					foreach (FinPeriod p in Base1.finPeriodCache.Cached)
					{
						periodList.Add(p.FinPeriodID);
					}
				}
				#endregion

				decimal val = 0m;
				PMAccountGroup lastAccountGroup = null;
				PMProject lastProject = null;
				PMTask lastTask = null;
				InventoryItem lastItem = null;
				PXReportResultset resultset = new PXReportResultset(typeof(PMHistory), typeof(PMProject), typeof(PMTask), typeof(PMAccountGroup), typeof(InventoryItem));

				for (int a = 0; a < agList.Count; a++)
				{
					for (int p = 0; p < projectList.Count; p++)
					{
						for (int t = 0; t < taskList.Count; t++)
						{
							if (taskList[t].ProjectID == projectList[p].ContractID)
							{
								for (int i = 0; i < itemList.Count; i++)
								{
									for (int f = 0; f < periodList.Count; f++)
									{
										pmhistoryInstance.AccountGroupID = agList[a].GroupID;
										pmhistoryInstance.ProjectID = projectList[p].ContractID;
										pmhistoryInstance.ProjectTaskID = taskList[t].TaskID;
										pmhistoryInstance.InventoryID = itemList[i].InventoryID;
										pmhistoryInstance.PeriodID = periodList[f];
										PMHistory hist = (PMHistory)pmhistoryCache.Locate(pmhistoryInstance);
										if (hist == null)
										{
											continue;
										}

										if (ds.Expand == ExpandType.AccountGroup && lastAccountGroup != null && lastAccountGroup.GroupID != hist.AccountGroupID)
										{
											for (int k = 0; k < agList.Count; k++)
											{
												if (agList[k] == lastAccountGroup)
												{
													dataSetCopy = new ARmDataSet(dataSetCopy);
													dataSetCopy[Keys.StartAccountGroup] = dataSetCopy[Keys.EndAccountGroup] = agList[k].GroupCD;
													splitret[k][2] = (decimal)splitret[k][2] + val;
													splitret[k][3] = dataSetCopy;
													splitret[k][4] = string.Empty;
													break;
												}
											}
											val = 0m;
										}
										if (ds.Expand == ExpandType.Project && lastProject != null && lastProject.ContractID != hist.ProjectID)
										{
											for (int k = 0; k < projectList.Count; k++)
											{
												if (projectList[k] == lastProject)
												{
													dataSetCopy = new ARmDataSet(dataSetCopy);
													dataSetCopy[Keys.StartProject] = dataSetCopy[Keys.EndProject] = projectList[k].ContractCD;
													splitret[k][2] = (decimal)splitret[k][2] + val;
													splitret[k][3] = dataSetCopy;
													splitret[k][4] = string.Empty;
													break;
												}
											}
											val = 0m;
										}
										if (ds.Expand == ExpandType.ProjectTask && lastTask != null && lastTask.TaskID != hist.ProjectTaskID)
										{
											for (int k = 0; k < taskList.Count; k++)
											{
												if (taskList[k] == lastTask)
												{
													dataSetCopy = new ARmDataSet(dataSetCopy);
													dataSetCopy[Keys.StartProjectTask] = dataSetCopy[Keys.EndProjectTask] = taskList[k].TaskCD;
													splitret[k][2] = (decimal)splitret[k][2] + val;
													splitret[k][3] = dataSetCopy;
													splitret[k][4] = string.Empty;
													break;
												}
											}
											val = 0m;
										}

										if (ds.Expand == ExpandType.Inventory && lastItem != null && lastItem.InventoryID != hist.InventoryID)
										{
											for (int k = 0; k < itemList.Count; k++)
											{
												if (itemList[k] == lastItem)
												{
													dataSetCopy = new ARmDataSet(dataSetCopy);
													dataSetCopy[Keys.StartInventory] = dataSetCopy[Keys.EndInventory] = itemList[k].InventoryCD;
													splitret[k][2] = (decimal)splitret[k][2] + val;
													splitret[k][3] = dataSetCopy;
													splitret[k][4] = string.Empty;
													break;
												}
											}
											val = 0m;
										}

										switch (ds.AmountType.Value)
										{
											case 12: //Turnover Amount
												val += hist.FinPTDAmount.Value;
												break;
											case 13: //Turnover Qty
												val += hist.FinPTDAmount.Value;
												break;
											case 6://Amount
												val += hist.FinYTDAmount.Value;
												break;
											case 7://Quantity
												val += hist.FinYTDQty.Value;
												break;
											case 8:
												val += hist.BudgetAmount.Value;
												break;
											case 9:
												val += hist.BudgetQty.Value;
												break;
											case 10:
												val += hist.RevisedAmount.Value;
												break;
											case 11:
												val += hist.RevisedQty.Value;
												break;
											case 14:
												val += hist.PTDBudgetAmount.Value;
												break;
											case 15:
												val += hist.PTDBudgetQty.Value;
												break;
											case 16:
												val += hist.PTDRevisedAmount.Value;
												break;
											case 17:
												val += hist.PTDRevisedQty.Value;
												break;
										}
										lastAccountGroup = agList[a];
										lastProject = projectList[p];
										lastTask = taskList[t];
										lastItem = itemList[i];

										if (drilldown)
										{
											resultset.Add(hist, lastProject, lastTask, lastAccountGroup, lastItem);
										}
									}
								}
							}
						}
					}
				}

				if (drilldown)
				{
					return resultset;
				}

				if (ds.Expand == ExpandType.AccountGroup)
				{
					if (lastAccountGroup != null)
					{
						for (int k = 0; k < agList.Count; k++)
						{
							if (agList[k].GroupID == lastAccountGroup.GroupID)
							{
								dataSetCopy = new ARmDataSet(dataSetCopy);
								dataSetCopy[Keys.StartAccountGroup] = dataSetCopy[Keys.EndAccountGroup] = agList[k].GroupCD;
								splitret[k][2] = val;
								splitret[k][3] = dataSetCopy;
								splitret[k][4] = string.Empty;
								break;
							}
						}
					}
					return splitret;
				}

				if (ds.Expand == ExpandType.Project)
				{
					if (lastProject != null)
					{
						for (int k = 0; k < projectList.Count; k++)
						{
							if (projectList[k].ContractID == lastProject.ContractID)
							{
								dataSetCopy = new ARmDataSet(dataSetCopy);
								dataSetCopy[Keys.StartProject] = dataSetCopy[Keys.EndProject] = projectList[k].ContractCD;
								splitret[k][2] = val;
								splitret[k][3] = dataSetCopy;
								splitret[k][4] = string.Empty;
								break;
							}
						}
					}
					return splitret;
				}

				if (ds.Expand == ExpandType.ProjectTask)
				{
					if (lastTask != null)
					{
						for (int k = 0; k < taskList.Count; k++)
						{
							if (taskList[k].TaskID == lastTask.TaskID && taskList[k].ProjectID == lastTask.ProjectID)
							{
								dataSetCopy = new ARmDataSet(dataSetCopy);
								dataSetCopy[Keys.StartProjectTask] = dataSetCopy[Keys.EndProjectTask] = taskList[k].TaskCD;
								splitret[k][2] = val;
								splitret[k][3] = dataSetCopy;
								splitret[k][4] = string.Empty;
								break;
							}
						}
					}
					return splitret;
				}

				if (ds.Expand == ExpandType.Inventory)
				{
					if (lastItem != null)
					{
						for (int k = 0; k < itemList.Count; k++)
						{
							if (itemList[k].InventoryID == lastItem.InventoryID)
							{
								dataSetCopy = new ARmDataSet(dataSetCopy);
								dataSetCopy[Keys.StartInventory] = dataSetCopy[Keys.EndInventory] = itemList[k].InventoryCD;
								splitret[k][2] = val;
								splitret[k][3] = dataSetCopy;
								splitret[k][4] = string.Empty;
								break;
							}
						}
					}
					return splitret;
				}

				return val;
			}
			else
			{
				return del(dataSet, drilldown);
			}
		}

		#endregion

		#region IARmDataSource

		public enum Keys
		{
			StartAccountGroup,
			EndAccountGroup,
			StartProject,
			EndProject,
			StartProjectTask,
			EndProjectTask,
			StartInventory,
			EndInventory,
		}

		[PXOverride]
		public bool IsParameter(ARmDataSet ds, string name, ValueBucket value, Func<ARmDataSet, string, ValueBucket, bool> del)
		{
			bool flag = del(ds, name, value);
			if (!flag)
			{
				Keys key;
				if (Enum.TryParse<Keys>(name, true, out key) && Enum.IsDefined(typeof(Keys), key))
				{
					value.Value = ds[key];
					return true;
				}
				return false;
			}
			return flag;
		}
		
		[PXOverride]
		public ARmDataSet MergeDataSet(IEnumerable<ARmDataSet> list, string expand, Func<IEnumerable<ARmDataSet>, string, ARmDataSet> del)
		{
			ARmDataSet dataSet = del(list, expand);

			bool isFirst = true;
			foreach (ARmDataSet set in list)
			{
				if (set == null) continue;

				if (expand == ExpandType.AccountGroup)
				{
					if (isFirst)
					{
						dataSet[Keys.StartAccountGroup] = set[Keys.StartAccountGroup] as string ?? "";
						dataSet[Keys.EndAccountGroup] = set[Keys.EndAccountGroup] as string ?? "";
					}
				}
				else
				{
					dataSet[Keys.StartAccountGroup] = Base.MergeMask(dataSet[Keys.StartAccountGroup] as string ?? "", set[Keys.StartAccountGroup] as string ?? "");
					dataSet[Keys.EndAccountGroup] = Base.MergeMask(dataSet[Keys.EndAccountGroup] as string ?? "", set[Keys.EndAccountGroup] as string ?? "");
				}

				if (expand == ExpandType.Project)
				{
					if (isFirst)
					{
						dataSet[Keys.StartProject] = set[Keys.StartProject] as string ?? "";
						dataSet[Keys.EndProject] = set[Keys.EndProject] as string ?? "";
					}
				}
				else
				{
					dataSet[Keys.StartProject] = Base.MergeMask(dataSet[Keys.StartProject] as string ?? "", set[Keys.StartProject] as string ?? "");
					dataSet[Keys.EndProject] = Base.MergeMask(dataSet[Keys.EndProject] as string ?? "", set[Keys.EndProject] as string ?? "");
				}

				if (expand == ExpandType.ProjectTask)
				{
					if (isFirst)
					{
						dataSet[Keys.StartProjectTask] = set[Keys.StartProjectTask] as string ?? "";
						dataSet[Keys.EndProjectTask] = set[Keys.EndProjectTask] as string ?? "";
					}
				}
				else
				{
					dataSet[Keys.StartProjectTask] = Base.MergeMask(dataSet[Keys.StartProjectTask] as string ?? "", set[Keys.StartProjectTask] as string ?? "");
					dataSet[Keys.EndProjectTask] = Base.MergeMask(dataSet[Keys.EndProjectTask] as string ?? "", set[Keys.EndProjectTask] as string ?? "");
				}

				if (expand == ExpandType.Inventory)
				{
					if (isFirst)
					{
						dataSet[Keys.StartInventory] = set[Keys.StartInventory] as string ?? "";
						dataSet[Keys.EndInventory] = set[Keys.EndInventory] as string ?? "";
					}
				}
				else
				{
					dataSet[Keys.StartInventory] = Base.MergeMask(dataSet[Keys.StartInventory] as string ?? "", set[Keys.StartInventory] as string ?? "");
					dataSet[Keys.EndInventory] = Base.MergeMask(dataSet[Keys.EndInventory] as string ?? "", set[Keys.EndInventory] as string ?? "");
				}

				isFirst = false;
			}

			string Expand = list.First().Expand;
			if ((Expand == ExpandType.AccountGroup || Expand == ExpandType.Project || Expand == ExpandType.ProjectTask || Expand == ExpandType.Inventory) && !(Expand == ExpandType.AccountGroup && (!string.IsNullOrEmpty(dataSet[Keys.StartAccountGroup] as string ?? "") && !string.IsNullOrEmpty(dataSet[Keys.EndAccountGroup] as string ?? "")) ||
				Expand == ExpandType.Project && (!string.IsNullOrEmpty(dataSet[Keys.StartAccountGroup] as string ?? "") || (!string.IsNullOrEmpty(dataSet[Keys.StartProject] as string ?? "") && !string.IsNullOrEmpty(dataSet[Keys.EndProject] as string ?? ""))) ||
				Expand == ExpandType.ProjectTask && (!string.IsNullOrEmpty(dataSet[Keys.StartProject] as string ?? "") || (!string.IsNullOrEmpty(dataSet[Keys.StartProjectTask] as string ?? "") && !string.IsNullOrEmpty(dataSet[Keys.EndProjectTask] as string ?? ""))) ||
				Expand == ExpandType.Inventory && (!string.IsNullOrEmpty(dataSet[Keys.StartProject] as string ?? "") || !string.IsNullOrEmpty(dataSet[Keys.StartProjectTask] as string ?? "") || (!string.IsNullOrEmpty(dataSet[Keys.StartInventory] as string ?? "") && !string.IsNullOrEmpty(dataSet[Keys.EndInventory] as string ?? "")))
				))
				dataSet.Expand = ExpandType.Nothing;
			else
				dataSet.Expand = Expand;

			return dataSet;
		}

		[PXOverride]
		public ARmReport GetReport(Func<ARmReport> del)
		{
			ARmReport ar = del();
			List<ARmReport.ARmReportParameter> aRp = ar.ARmParams;
			PXFieldState state;
			RMReportPM rPM = Base.Report.Cache.GetExtension<RMReportPM>(Base.Report.Current);

			if (ar.Type == ARmReport.PM)
			{
				string sViewName = string.Empty;
				string sInputMask = string.Empty;

				// StartAccountGroup, EndAccountGroup
				bool RequestEndAccountGroup = rPM.RequestEndAccountGroup ?? false;
				//int colSpan = RequestEndAccountGroup ? 1 : 2;
				int colSpan = 2;
				sViewName = sInputMask = string.Empty;
				state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourcePM.startAccountGroup>(null) as PXFieldState;
				if (state != null && !String.IsNullOrEmpty(state.ViewName))
				{
					sViewName = state.ViewName;
					if (state is PXStringState)
					{
						sInputMask = ((PXStringState)state).InputMask;
					}
				}
				Base.CreateParameter(Keys.StartAccountGroup, "StartAccountGroup", Messages.GetLocal(Messages.StartAccTitle), ar.DataSet[Keys.StartAccountGroup] as string, rPM.RequestStartAccountGroup ?? false, colSpan, sViewName, sInputMask, aRp);

				sViewName = sInputMask = string.Empty;
				state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourcePM.endAccountGroup>(null) as PXFieldState;
				if (state != null && !String.IsNullOrEmpty(state.ViewName))
				{
					sViewName = state.ViewName;
					if (state is PXStringState)
					{
						sInputMask = ((PXStringState)state).InputMask;
					}
				}
				Base.CreateParameter(Keys.EndAccountGroup, "EndAccountGroup", Messages.GetLocal(Messages.EndAccTitle), ar.DataSet[Keys.EndAccountGroup] as string, RequestEndAccountGroup, colSpan, sViewName, sInputMask, aRp);

				// StartProject, EndProject
				bool bRequestEndProject = rPM.RequestEndProject ?? false;
				//colSpan = bRequestEndProject ? 1 : 2;
				colSpan = 2;
				sViewName = sInputMask = string.Empty;
				state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourcePM.startProject>(null) as PXFieldState;
				if (state != null && !String.IsNullOrEmpty(state.ViewName))
				{
					sViewName = state.ViewName;
					if (state is PXStringState)
					{
						sInputMask = ((PXStringState)state).InputMask;
					}
				}
				Base.CreateParameter(Keys.StartProject, "StartProject", Messages.GetLocal(Messages.StartProjectTitle), ar.DataSet[Keys.StartProject] as string, rPM.RequestStartProject ?? false, colSpan, sViewName, sInputMask, aRp);

				sViewName = sInputMask = string.Empty;
				state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourcePM.endProject>(null) as PXFieldState;
				if (state != null && !String.IsNullOrEmpty(state.ViewName))
				{
					sViewName = state.ViewName;
					if (state is PXStringState)
					{
						sInputMask = ((PXStringState)state).InputMask;
					}
				}
				Base.CreateParameter(Keys.EndProject, "EndProject", Messages.GetLocal(Messages.EndProjectTitle), ar.DataSet[Keys.EndProject] as string, bRequestEndProject, colSpan, sViewName, sInputMask, aRp);

				// StartTask, EndTask
				bool RequestEndProjectTask = rPM.RequestEndProjectTask ?? false;

				//colSpan = RequestEndProjectTask ? 1 : 2;
				colSpan = 2;
				sViewName = sInputMask = string.Empty;
				state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourcePM.startProjectTask>(null) as PXFieldState;
				if (state != null && !String.IsNullOrEmpty(state.ViewName))
				{
					sViewName = state.ViewName;
					if (state is PXStringState)
					{
						sInputMask = ((PXStringState)state).InputMask;
					}
				}
				Base.CreateParameter(Keys.StartProjectTask, "StartTask", Messages.GetLocal(Messages.StartTaskTitle), ar.DataSet[Keys.StartProjectTask] as string, rPM.RequestStartProjectTask ?? false, colSpan, sViewName, sInputMask, aRp);

				sViewName = sInputMask = string.Empty;
				state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourcePM.endProjectTask>(null) as PXFieldState;
				if (state != null && !String.IsNullOrEmpty(state.ViewName))
				{
					sViewName = state.ViewName;
					if (state is PXStringState)
					{
						sInputMask = ((PXStringState)state).InputMask;
					}
				}
				Base.CreateParameter(Keys.EndProjectTask, "EndTask", Messages.GetLocal(Messages.EndTaskTitle), ar.DataSet[Keys.EndProjectTask] as string, RequestEndProjectTask, colSpan, sViewName, sInputMask, aRp);

				// StartInventory, EndInventory
				bool bRequestEndInventory = rPM.RequestEndInventory ?? false;
				//colSpan = bRequestEndInventory ? 1 : 2;
				colSpan = 2;

				sViewName = sInputMask = string.Empty;
				state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourcePM.startInventory>(null) as PXFieldState;
				if (state != null && !String.IsNullOrEmpty(state.ViewName))
				{
					sViewName = state.ViewName;
					if (state is PXStringState)
					{
						sInputMask = ((PXStringState)state).InputMask;
					}
				}
				Base.CreateParameter(Keys.StartInventory, "StartInventory", Messages.GetLocal(Messages.StartInventoryTitle), ar.DataSet[Keys.StartInventory] as string, rPM.RequestStartInventory ?? false, colSpan, sViewName, sInputMask, aRp);

				sViewName = sInputMask = string.Empty;
				state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourcePM.endInventory>(null) as PXFieldState;
				if (state != null && !String.IsNullOrEmpty(state.ViewName))
				{
					sViewName = state.ViewName;
					if (state is PXStringState)
					{
						sInputMask = ((PXStringState)state).InputMask;
					}
				}
				Base.CreateParameter(Keys.EndInventory, "EndInventory", Messages.GetLocal(Messages.EndInventoryTitle), ar.DataSet[Keys.EndInventory] as string, bRequestEndInventory, colSpan, sViewName, sInputMask, aRp);
			}

			return ar;
		}

		[PXOverride]
		public string GetUrl(Func<string> del)
		{
			string rmType = Base.Report.Current.Type;
			if (rmType == ARmReport.PM)
			{
				PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNodeByScreenID("CS600010");
				if (node != null)
				{
					return PX.Common.PXUrl.TrimUrl(node.Url);
				}
				throw new PXException(string.Format(ErrorMessages.GetLocal(ErrorMessages.NotEnoughRightsToAccessObject), "CS600010"));
			}
			else
			{
				return del();
			}
		}

		[PXOverride]
		public void FillDataSource(RMDataSource ds, ARmDataSet dst, string rmType, Action<RMDataSource, ARmDataSet, string> del)
		{
			del(ds, dst, rmType);
			if (ds != null && ds.DataSourceID != null)
			{
				if (rmType == ARmReport.PM)
				{
					RMDataSourcePM dsPM = Base.Caches[typeof(RMDataSource)].GetExtension<RMDataSourcePM>(ds);
					dst[Keys.StartAccountGroup] = dsPM.StartAccountGroup;
					dst[Keys.EndAccountGroup] = dsPM.EndAccountGroup;
					dst[Keys.StartProject] = dsPM.StartProject;
					dst[Keys.EndProject] = dsPM.EndProject;
					dst[Keys.StartProjectTask] = dsPM.StartProjectTask;
					dst[Keys.EndProjectTask] = dsPM.StartProjectTask;
					dst[Keys.StartInventory] = dsPM.StartInventory;
					dst[Keys.EndInventory] = dsPM.EndInventory;
				}
			}
		}

		private ArmDATA _Data;
		[PXOverride]
		public object GetExprContext()
		{
			if (_Data == null)
			{
				_Data = new ArmDATA();
			}
			return _Data;
		}

		#endregion
	}
}
