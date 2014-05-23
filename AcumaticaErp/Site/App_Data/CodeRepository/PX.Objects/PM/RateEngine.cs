using System;
using System.Text;
using PX.Data;
using System.CodeDom.Compiler;

namespace PX.Objects.PM
{
	public class RateEngine
	{
		protected PXGraph graph;
		protected string rateTypeID;
		protected PMTran tran;
		protected StringBuilder trace;
		
		public RateEngine(PXGraph graph, string rateTypeID, PMTran tran)
		{
			if (graph == null)
				throw new ArgumentNullException("graph");

			if (string.IsNullOrEmpty(rateTypeID))
				throw new ArgumentNullException("rateTypeID", "Argument is null or an empty string");
						
			if (tran == null)
				throw new ArgumentNullException("tran");

			this.graph = graph;
			this.rateTypeID = rateTypeID;
			this.tran = tran;
		}

		/// <summary>
		/// Searches for Rate based on RateCode, PriceClass and Transaction properties.
		/// Rate is first searched for the given PriceClass; if not found it is then searched in BasePriceClass.
		/// </summary>
		public decimal? GetRate(string rateTableID)
		{
			if (string.IsNullOrEmpty(rateTableID))
                throw new ArgumentNullException("rateTableID", "Argument is null or an empty string");

		    trace = new StringBuilder();

            trace.AppendFormat("Calculating Rate. RateTable:{0}", rateTableID);

            PXSelectBase<PMRateDefinition> select = new PXSelect<PMRateDefinition,
                Where<PMRateDefinition.rateTableID, Equal<Required<PMRateDefinition.rateTableID>>,
                And<PMRateDefinition.rateTypeID, Equal<Required<PMRateDefinition.rateTypeID>>>>,
                OrderBy<Asc<PMRateDefinition.sequence>>>(graph);

            PXResultset<PMRateDefinition> rateDefinitions = select.Select(rateTableID, rateTypeID);

            foreach (PMRateDefinition rd in rateDefinitions)
            {
                trace.AppendFormat("Start Processing Sequence:{0}", rd.Description);
                decimal? rate = GetRate(rd);
                if (rate != null)
                {
                    trace.AppendFormat("End Processing Sequence. Rate Defined:{0}", rate);
                    return rate;
                }
                else
                {
                    trace.AppendFormat("End Processing Sequence. Rate Not Defined");
                }

            }
            return null;
		}

		public string GetTrace()
		{
			//Add transaction properties:
			PMAccountGroup ag = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(graph, tran.AccountGroupID);
			if (ag != null)
			{
				trace.AppendFormat(" PMTran.AccountGroup={0} ", ag.GroupCD);
			}

			IN.InventoryItem inventoryItem = PXSelect<IN.InventoryItem, Where<IN.InventoryItem.inventoryID, Equal<Required<IN.InventoryItem.inventoryID>>>>.Select(graph, tran.InventoryID);
			if (inventoryItem != null)
			{
				trace.AppendFormat(" PMTran.InventoryID={0} ", inventoryItem.InventoryCD);
			}
            
			return trace.ToString();
		}
        
		protected virtual decimal? GetRate(PMRateDefinition rd)
		{
			bool isApplicable = true;

			if (rd.Project == true)
			{
				if (!IsProjectFit(rd.RateDefinitionID, tran.ProjectID))
				{
					isApplicable = false;
				}
			}

			if (rd.Task == true)
			{
				if (!IsTaskFit(rd.RateDefinitionID, tran.ProjectID, tran.TaskID))
				{
					isApplicable = false;
				}
			}

			if (rd.AccountGroup == true)
			{
				if (!IsAccountGroupFit(rd.RateDefinitionID, tran.AccountGroupID))
				{
					isApplicable = false;
				}
			}

			if (rd.RateItem == true)
			{
				if (!IsItemFit(rd.RateDefinitionID, tran.InventoryID))
				{
					isApplicable = false;
				}
			}

			if (rd.Employee == true)
			{
				if (!IsEmployeeFit(rd.RateDefinitionID, tran.ResourceID))
				{
					isApplicable = false;
				}
			}

			if (isApplicable)
			{
				PXSelectBase<PMRate> select = new PXSelect<PMRate,
					Where<PMRate.rateDefinitionID, Equal<Required<PMRate.rateDefinitionID>>,
					And<Where<PMRate.startDate, LessEqual<Required<PMRate.startDate>>,
					And2<Where<PMRate.endDate, GreaterEqual<Required<PMRate.endDate>>>, Or<PMRate.endDate, IsNull>>>>>>(graph);

				PMRate rate = select.Select(rd.RateDefinitionID, tran.Date, tran.Date);
				trace.AppendFormat("	Searching Rate for Date:{0}", tran.Date);
                
				if (rate != null)
				{
					return rate.Rate;
				}
				else

					return null;
			}
			else
			{
				return null;
			}

		}

		protected virtual bool IsProjectFit(int? rateDefinitionID, int? projectID)
		{
			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(graph, projectID);

			if (project == null)
				throw new PXException(Messages.ProjectNotFound, projectID);

			string cd = project.ContractCD;
			
			PXSelectBase<PMProjectRate> select = new PXSelect<PMProjectRate,
				Where<PMProjectRate.rateDefinitionID, Equal<Required<PMProjectRate.rateDefinitionID>>>>(graph);

			foreach (PMProjectRate item in select.Select(rateDefinitionID))
			{
				if (IsFit(item.ProjectCD.Trim(), cd.Trim()))
				{
					trace.AppendFormat("	Checking Project {0}..Match found.", cd.Trim());
					return true;
				}
			}

			trace.AppendFormat("	Checking Project {0}..Match not found.", cd.Trim());
			return false;
		}

		protected virtual bool IsTaskFit(int? rateDefinitionID, int? projectID, int? taskID)
		{
			PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(graph, projectID, taskID);
			if (task == null)
				throw new PXException(Messages.TaskNotFound, projectID, taskID);

			string cd = task.TaskCD;

			PXSelectBase<PMTaskRate> select = new PXSelect<PMTaskRate,
				Where<PMTaskRate.rateDefinitionID, Equal<Required<PMTaskRate.rateDefinitionID>>>>(graph);

			foreach (PMTaskRate item in select.Select(rateDefinitionID))
			{
				if (IsFit(item.TaskCD.Trim(), cd.Trim()))
				{
					trace.AppendFormat("	Checking Task {0}..Match found.", cd.Trim());
					return true;
				}
			}

			trace.AppendFormat("	Checking Task {0}..Match not found.", cd.Trim());
			return false;
		}

		protected virtual bool IsAccountGroupFit(int? rateDefinitionID, int? accountGroupID)
		{
			PMAccountGroupRate item = PXSelect<PMAccountGroupRate,
				Where<PMAccountGroupRate.rateDefinitionID, Equal<Required<PMAccountGroupRate.rateDefinitionID>>,
				And<PMAccountGroupRate.accountGroupID, Equal<Required<PMAccountGroupRate.accountGroupID>>>>>.Select(graph, rateDefinitionID, accountGroupID);

			if (item != null)
			{
				trace.AppendFormat("	Checking Account Group..Match found.");
				return true;
			}
			else
			{
				trace.AppendFormat("	Checking Account Group..Match not found.");
				return false;
			}
		}

		protected virtual bool IsItemFit(int? rateDefinitionID, int? inventoryID)
		{
			PMItemRate item = PXSelect<PMItemRate,
				Where<PMItemRate.rateDefinitionID, Equal<Required<PMItemRate.rateDefinitionID>>,
				And<PMItemRate.inventoryID, Equal<Required<PMItemRate.inventoryID>>>>>.Select(graph, rateDefinitionID, inventoryID);

			if (item != null)
			{
				trace.AppendFormat("	Checking Inventory Item..Match found.");
				return true;
			}
			else
			{
				trace.AppendFormat("	Checking Inventory Item..Match not found.");
				return false;
			}
		}

		protected virtual bool IsEmployeeFit(int? rateDefinitionID, int? employeeID)
		{
			PMEmployeeRate item = PXSelect<PMEmployeeRate,
				Where<PMEmployeeRate.rateDefinitionID, Equal<Required<PMEmployeeRate.rateDefinitionID>>,
				And<PMEmployeeRate.employeeID, Equal<Required<PMEmployeeRate.employeeID>>>>>.Select(graph, rateDefinitionID, employeeID);

			if (item != null)
			{
				trace.AppendFormat("	Checking Employee..Match found.");
				return true;
			}
			else
			{
				trace.AppendFormat("	Checking Employee..Match not found.");
				return false;
			}
		}

		protected virtual bool IsFit(string wildcard, string value)
		{
			if (value.Length < wildcard.Length)
			{
				value += new string(' ', wildcard.Length - value.Length);
			}
			else if (value.Length > wildcard.Length)
			{
				return false;
			}

			for (int i = 0; i < wildcard.Length; i++)
			{
				if (wildcard[i] == '?')
					continue;

				if (wildcard[i] != value[i])
					return false;
			}

			return true;
		}

	}
}
