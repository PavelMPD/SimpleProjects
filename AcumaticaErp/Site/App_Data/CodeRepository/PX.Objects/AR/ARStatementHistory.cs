using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using System.Collections;
using System.Globalization;

namespace PX.Objects.AR
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class ARStatementHistory : PXGraph<ARStatementHistory>
    {
		[System.SerializableAttribute()]
		public partial class ARStatementHistoryParameters : IBqlTable
		{
			#region StatementCycleId
			public abstract class statementCycleId : PX.Data.IBqlField
			{
			}
			protected String _StatementCycleId;
			[PXDBString(10, IsUnicode = true)]
			[PXUIField(DisplayName = "Statement Cycle ID", Visibility = PXUIVisibility.Visible)]
			[PXSelector(typeof(ARStatementCycle.statementCycleId))]
			public virtual String StatementCycleId
			{
				get
				{
					return this._StatementCycleId;
				}
				set
				{
					this._StatementCycleId = value;
				}
			}
			#endregion
			#region StartDate
			public abstract class startDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _StartDate;
			[PXDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? StartDate
			{
				get
				{
					return this._StartDate;
				}
				set
				{
					this._StartDate = value;
				}
			}
			#endregion
			#region EndDate
			public abstract class endDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _EndDate;
			[PXDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? EndDate
			{
				get
				{
					return this._EndDate;
				}
				set
				{
					this._EndDate = value;
				}
			}
			#endregion
		}

		[Serializable]
		public partial class HistoryResult : IBqlTable
		{
			#region StatementCycleId
			public abstract class statementCycleId : PX.Data.IBqlField
			{
			}
			protected String _StatementCycleId;
			[PXDBString(10, IsUnicode = true, IsKey = true)]
			[PXUIField(DisplayName = "Statement Cycle ID", Visibility = PXUIVisibility.Visible)]
			public virtual String StatementCycleId
			{
				get
				{
					return this._StatementCycleId;
				}
				set
				{
					this._StatementCycleId = value;
				}
			}
			#endregion			
			#region StatementDate
			public abstract class statementDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _StatementDate;
			[PXDBDate(IsKey = true)]
			[PXDefault()]
			[PXUIField(DisplayName = "Statement Date")]
			public virtual DateTime? StatementDate
			{
				get
				{
					return this._StatementDate;
				}
				set
				{
					this._StatementDate = value;
				}
			}
			#endregion
			#region Descr
			public abstract class descr : PX.Data.IBqlField
			{
			}
			protected String _Descr;
			[PXDBString(60, IsUnicode = true)]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String Descr
			{
				get
				{
					return this._Descr;
				}
				set
				{
					this._Descr = value;
				}
			}
			#endregion
			#region NumberOfDocuments
			public abstract class numberOfDocuments : PX.Data.IBqlField
			{
			}
			protected int? _NumberOfDocuments;
			[PXInt()]
			[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Number Of Documents")]
			public virtual int? NumberOfDocuments
			{
				get
				{
					return this._NumberOfDocuments;
				}
				set
				{
					this._NumberOfDocuments = value;
				}
			}
			#endregion
			#region DontPrintCount
			public abstract class dontPrintCount : PX.Data.IBqlField
			{
			}
			protected int? _DontPrintCount;
			[PXInt()]
			[PXDefault(0,PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Don't Print",Enabled=false)]
			public virtual int? DontPrintCount
			{
				get
				{
					return this._DontPrintCount;
				}
				set
				{
					this._DontPrintCount = value;
				}
			}
			#endregion
			#region PrintedCount
			public abstract class printedCount : PX.Data.IBqlField
			{
			}
			protected int? _PrintedCount;
			[PXInt()]
			[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Printed", Enabled = false)]
			public virtual int? PrintedCount
			{
				get
				{
					return this._PrintedCount;
				}
				set
				{
					this._PrintedCount = value;
				}
			}
			#endregion
			#region DontEmailCount
			public abstract class dontEmailCount : PX.Data.IBqlField
			{
			}
			protected int? _DontEmailCount;
			[PXInt()]
			[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Don't Email",Enabled=false)]
			public virtual int? DontEmailCount
			{
				get
				{
					return this._DontEmailCount;
				}
				set
				{
					this._DontEmailCount = value;
				}
			}
			#endregion
			#region EmailedCount
			public abstract class emailedCount : PX.Data.IBqlField
			{
			}
			protected int? _EmailedCount;
			[PXInt()]
			[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Emailed",Enabled=false)]
			public virtual int? EmailedCount
			{
				get
				{
					return this._EmailedCount;
				}
				set
				{
					this._EmailedCount = value;
				}
			}
			#endregion			
			#region NoActionCount
			public abstract class noActionCount : PX.Data.IBqlField
			{
			}
			protected int? _NoActionCount;
			[PXInt()]
			[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "No Action", Enabled = false)]
			public virtual int? NoActionCount
			{
				get
				{
					return this._NoActionCount;
				}
				set
				{
					this._NoActionCount = value;
				}
			}
			#endregion
			#region ToPrintCount
			public abstract class toPrintCount : PX.Data.IBqlField
			{
			}
			
			[PXInt()]			
			[PXUIField(DisplayName = "To Print",Enabled=false)]
			public virtual int? ToPrintCount
			{
				[PXDependsOnFields(typeof(numberOfDocuments),typeof(dontPrintCount))]
				get
				{
					return this.NumberOfDocuments - this.DontPrintCount;
				}
				set
				{
					this._PrintedCount = value;
				}
			}
			#endregion
			#region ToEmailCount
			public abstract class toEmailCount : PX.Data.IBqlField
			{
			}
			
			[PXInt()]			
			[PXUIField(DisplayName = "To Email",Enabled=false)]
			public virtual int? ToEmailCount
			{
				[PXDependsOnFields(typeof(numberOfDocuments),typeof(dontEmailCount))]
				get
				{
					return this._NumberOfDocuments - this._DontEmailCount;
				}
				set
				{
					
				}
			}
			#endregion
			#region PrintCompletion
			public abstract class printCompletion : PX.Data.IBqlField
			{
			}
			
			[PXDecimal(2)]			
			[PXUIField(DisplayName = "Print Completion %",Enabled=false)]
			public virtual Decimal? PrintCompletion
			{
				[PXDependsOnFields(typeof(toPrintCount),typeof(printedCount))]
				get
				{
					return (this.ToPrintCount.HasValue && this.ToPrintCount != 0) ? (((decimal)this.PrintedCount / (decimal)this.ToPrintCount) * 100.0m) : 100.0m;
				}
				set
				{
					
				}
			}
			#endregion
			#region EmailCompletion
			public abstract class emailCompletion : PX.Data.IBqlField
			{
			}

			[PXDecimal(2)]
            [PXUIField(DisplayName = "Email Completion %", Enabled = false)]
			public virtual Decimal? EmailCompletion
			{
				[PXDependsOnFields(typeof(toEmailCount),typeof(emailedCount))]
				get
				{
					return (this.ToEmailCount.HasValue && this.ToEmailCount != 0) ? (((decimal)this.EmailedCount / (decimal)this.ToEmailCount) * 100.0m) : 100.0m;
				}
				set
				{

				}
			}
			#endregion
		}

		public PXFilter<ARStatementHistoryParameters> Filter;
		public PXCancel<ARStatementHistoryParameters> Cancel;
		[PXFilterable]
		public PXSelect<HistoryResult> History;

		public ARStatementHistory()
		{
			ARSetup setup = ARSetup.Current;
			History.Cache.AllowDelete = false;
			History.Cache.AllowInsert = false;
			History.Cache.AllowUpdate = false;
		}

		public PXSetup<ARSetup> ARSetup;
	
		protected virtual IEnumerable history()
		{
			this.History.Cache.Clear();

			ARStatementHistoryParameters header = Filter.Current;
			if (header == null)
			{
				yield break;
			}

			Dictionary<string, Dictionary<DateTime, int>> result = new Dictionary<string, Dictionary<DateTime, int>>();
			Dictionary<string, string> dictARStatementCycle = new Dictionary<string, string>();

			int? prevCustomerID = null;
			string prevCuryID = null;
			HistoryResult prevRec = null;

			foreach (PXResult<ARStatement,ARStatementCycle, Customer> item in 
					PXSelectJoin<ARStatement,
						LeftJoin<ARStatementCycle, On<ARStatementCycle.statementCycleId, Equal<ARStatement.statementCycleId>>,
						LeftJoin<Customer, On<Customer.bAccountID, Equal<ARStatement.customerID>>>>,
					  Where<ARStatement.statementDate, GreaterEqual<Required<ARStatement.statementDate>>,
					    And<ARStatement.statementDate, LessEqual<Required<ARStatement.statementDate>>,
					    And<Where<ARStatement.statementCycleId, Equal<Required<ARStatement.statementCycleId>>,
					           Or<Required<ARStatement.statementCycleId>,IsNull>>>>>,
				    OrderBy<Asc<ARStatement.statementCycleId, 
						        Asc<ARStatement.statementDate, 
										Asc<ARStatement.customerID,
										Asc<ARStatement.curyID>>>>>>
					.Select(this, header.StartDate, header.EndDate, header.StatementCycleId, header.StatementCycleId))
			{
				ARStatement st = item; 
				Customer cust  = item;
				HistoryResult rec = new HistoryResult();
				rec.StatementCycleId  = st.StatementCycleId;
				rec.StatementDate     = st.StatementDate;
				rec.Descr = ((ARStatementCycle)item).Descr;
				rec.DontPrintCount =0;
				rec.DontEmailCount =0;
				rec.PrintedCount = 0;
				rec.EmailedCount = 0;
				rec.NoActionCount = 0;
				rec = (HistoryResult)this.History.Cache.Locate(rec) ?? this.History.Insert(rec);
				if (rec != null)
				{
					if (prevRec != rec) { prevCustomerID = null; prevCuryID = null; }

					if (prevCustomerID != st.CustomerID || 
						 (cust.PrintCuryStatements == true && prevCuryID != st.CuryID))
					{
						rec.NumberOfDocuments += 1;
						rec.DontPrintCount += ((st.DontPrint??false) == true) ? 1 : 0;
						rec.DontEmailCount += ((st.DontEmail??false) == true) ? 1 : 0;
						rec.NoActionCount += ((st.DontEmail ?? false) == true && (st.DontPrint??false)==true) ? 1 : 0;
						rec.PrintedCount += ((st.Printed??false) == true) ? 1 : 0;
						rec.EmailedCount += ((st.Emailed ?? false) == true) ? 1 : 0;
					}
					prevRec        = rec;
					prevCustomerID = st.CustomerID;
					prevCuryID     = st.CuryID;										
				}
				/*
				if (result.ContainsKey(((ARStatement)st).StatementCycleId))
				{
					if (result[((ARStatement)st).StatementCycleId].ContainsKey(((ARStatement)st).StatementDate ?? DateTime.MinValue))
					{
						result[((ARStatement)st).StatementCycleId][((ARStatement)st).StatementDate ?? DateTime.MinValue] = result[((ARStatement)st).StatementCycleId][((ARStatement)st).StatementDate ?? DateTime.MinValue] + 1;
					}
					else
					{
						result[((ARStatement)st).StatementCycleId].Add(((ARStatement)st).StatementDate ?? DateTime.MinValue, 1);
					}
				}
				else
				{
					Dictionary<DateTime, int> dictValue = new Dictionary<DateTime, int>();
					dictValue.Add(((ARStatement)st).StatementDate ?? DateTime.MinValue, 1);
					result.Add(((ARStatement)st).StatementCycleId, dictValue);
				}
				if(dictARStatementCycle.ContainsKey (((ARStatement)st).StatementCycleId) == false)
					dictARStatementCycle.Add(((ARStatement)st).StatementCycleId, ((ARStatementCycle)st).Descr);
				*/
			}
			this.History.Cache.IsDirty = false;
			foreach(HistoryResult ret in this.History.Cache.Inserted)
				yield return ret;

			/*
			foreach (string statementCycleId in result.Keys)
			{
				foreach (DateTime statementDate in result[statementCycleId].Keys)
				{
					HistoryResult res = new HistoryResult();
					res.StatementCycleId = statementCycleId;
					res.StatementDate = statementDate;
					res.NumberOfDocuments = result[statementCycleId][statementDate];
					res.Descr = dictARStatementCycle[statementCycleId];
					yield return res;
				}
			}
			 */
		}

		protected virtual void ARStatementHistoryParameters_StartDate_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = new DateTime(Accessinfo.BusinessDate.Value.Year, 1, 1);
		}
		#region Sub-screen Navigation Button
		public PXAction<ARStatementHistoryParameters> viewDetails;
		[PXUIField(DisplayName = "Customer Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewDetails(PXAdapter adapter)
		{
			if (this.History.Current != null && this.Filter.Current != null)
			{
				HistoryResult res = this.History.Current;
				//ARStatementHistoryParameters currentFilter = this.Filter.Current;
				ARStatementDetails graph = PXGraph.CreateInstance<ARStatementDetails>();

				ARStatementDetails.ARStatementDetailsParameters filter = graph.Filter.Current;
				filter.StatementCycleId = res.StatementCycleId;
				filter.StatementDate = res.StatementDate;
				graph.Filter.Update(filter);
				filter = graph.Filter.Select();
				throw new PXRedirectRequiredException(graph, "Statement Details");

			}
			return Filter.Select();
		}

		public PXAction<ARStatementHistoryParameters> printReport;
		[PXUIField(DisplayName = "Print Statement", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable PrintReport(PXAdapter adapter)
		{
			ARStatementPrint graph = PXGraph.CreateInstance<ARStatementPrint>();
			graph.Filter.Current.StatementCycleId = this.History.Current.StatementCycleId;
			graph.Filter.Current.StatementDate = this.History.Current.StatementDate;
			throw new PXRedirectRequiredException(graph, "Report Process");
			/*
			if (this.History.Current != null && this.Filter.Current != null)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				Copy(parameters, this.History.Current);
				throw new PXReportRequiredException(parameters,ARStatementReportParams.CS_StatementReportID, "AR Statement Report");
			}
			return Filter.Select();
			*/
		}
		/*
		public PXAction<ARStatementHistoryParameters> printCuryReport;
		[PXUIField(DisplayName = "Print Cury. Statement", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable PrintCuryReport(PXAdapter adapter)
		{
			if (this.History.Current != null && this.Filter.Current != null)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				Copy(parameters,this.History.Current);
				throw new PXReportRequiredException(parameters, ARStatementReportParams.CS_CuryStatementReportID, "AR Statement Report");
			}
			return Filter.Select();
		}
		*/
		#endregion

		protected static void Copy(Dictionary<string, string> aDest,HistoryResult aSrc)
		{
			aDest[ARStatementReportParams.Parameters.StatementCycleID] = aSrc.StatementCycleId;
			aDest[ARStatementReportParams.Parameters.StatementDate] = aSrc.StatementDate.Value.ToString("d", CultureInfo.InvariantCulture); 			
		}
	}
}
