using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.EP
{

    public class EmployeeCostEngine
	{
		protected PXGraph graph;
		protected string defaultUOM = EPSetup.Hour;
		
		public EmployeeCostEngine(PXGraph graph)
		{
			if ( graph == null )
				throw new ArgumentNullException();

			this.graph = graph;

			EPSetup setup = PXSelect<EPSetup>.Select(graph);
            if (setup != null && !String.IsNullOrEmpty(setup.EmployeeRateUnit))
			    defaultUOM = setup.EmployeeRateUnit;
		}


		public class Rate
		{
			public int? EmployeeID { get; private set; }
			public decimal HourlyRate { get; private set; }
			public string Type { get; private set; }
			public string UOM { get; private set; }

            /// <summary>
            /// Total Hours in week
            /// </summary>
			public int? RegularHours { get; private set; }

            /// <summary>
            /// Employee Rate for a Week
            /// </summary>
			public decimal? RateByType { get; private set; }
			
			public Rate(int? employeeID, string type, decimal? hourlyRate, string uom, int? regularHours, decimal? rateByType)
			{
				this.EmployeeID = employeeID;
				this.UOM = uom;
				this.HourlyRate = hourlyRate ?? 0m;
				this.Type = string.IsNullOrEmpty(type) ? RateTypesAttribute.Hourly : type;
				this.RegularHours = regularHours ?? 0;
				this.RateByType = rateByType;
			}
		}
		

		public virtual Rate GetEmployeeRate(int? projectId, int? projectTaskId, int? employeeId, DateTime? date)
		{
			decimal? hourlyRate = null;
			decimal? rate = null;
			PXResult<EPEmployeeRate, EPEmployeeRateByProject> result = ((PXResult<EPEmployeeRate, EPEmployeeRateByProject>)
				PXSelectJoin<
					EPEmployeeRate
					, LeftJoin<
						EPEmployeeRateByProject
						, On<
							EPEmployeeRate.rateID, Equal<EPEmployeeRateByProject.rateID>
							, And<EPEmployeeRateByProject.projectID, Equal<Required<EPEmployeeRateByProject.projectID>>
								, And<Where<
									EPEmployeeRateByProject.taskID, Equal<Required<EPEmployeeRateByProject.taskID>>
									, Or<EPEmployeeRateByProject.taskID, IsNull>
									>>
								>
							>
						>
					, Where<
						EPEmployeeRate.employeeID, Equal<Required<EPEmployeeRate.employeeID>>
						, And<EPEmployeeRate.effectiveDate, LessEqual<Required<EPEmployeeRate.effectiveDate>>>
						>
						, OrderBy<Desc<EPEmployeeRate.effectiveDate, Desc<EPEmployeeRateByProject.taskID>>>
					>.SelectWindowed(graph, 0, 1, projectId, projectTaskId, employeeId, date)).
				With(_ => (_));

			EPEmployeeRateByProject employeeRateByProject = (EPEmployeeRateByProject) result;
			EPEmployeeRate employeeRate = (EPEmployeeRate) result;

			if (employeeRateByProject != null && employeeRateByProject.HourlyRate != null)
				hourlyRate = employeeRateByProject.HourlyRate;
            else if (employeeRate != null)
                hourlyRate = employeeRate.HourlyRate;
            else
            {
                CR.BAccountR baccount = PXSelect<CR.BAccountR, Where<CR.BAccountR.bAccountID, Equal<Required<CR.BAccountR.bAccountID>>>>.Select(graph, employeeId);
                throw new PXException(string.Format(Messages.HourlyRateIsNotSet, date, baccount.AcctCD));
            }
		    if (employeeRate.RateType == RateTypesAttribute.Hourly)
				rate = hourlyRate;
			else
				rate = hourlyRate * employeeRate.RegularHours;

			return new Rate(employeeId, employeeRate.RateType, hourlyRate, defaultUOM, employeeRate.RegularHours, rate);
		}

        public virtual EPEmployeeRate GetEmployeeRate(int? employeeID, DateTime? date)
        {
            PXSelectBase<EPEmployeeRate> select = new PXSelect<EPEmployeeRate, Where<EPEmployeeRate.employeeID, Equal<Required<EPEmployeeRate.employeeID>>,
                    And<EPEmployeeRate.effectiveDate, LessEqual<Required<EPEmployeeRate.effectiveDate>>>>,
                    OrderBy<Desc<EPEmployeeRate.effectiveDate>>>(graph);

            return select.SelectWindowed(0, 1, employeeID, date);
        }

		public virtual decimal? CalculateEmployeeCost(CR.EPActivity activity, int? employeeID, DateTime date)
		{
			decimal? cost;
			Rate employeeRate = GetEmployeeRate(activity.ProjectID, activity.ProjectTaskID, employeeID, date);

			if (employeeRate.Type == RateTypesAttribute.SalaryWithExemption && activity.TimeCardCD != null)
			{
				//Overtime is not applicable. Rate is prorated based on actual hours worked on the given week

				EPTimeCard timecard = PXSelect<EPTimeCard, Where<EPTimeCard.timeCardCD, Equal<Required<EPTimeCard.timeCardCD>>>>.Select(graph, activity.TimeCardCD);
				if (timecard.TotalSpentCalc <= employeeRate.RegularHours * 60)
                {
                    cost = employeeRate.RateByType / employeeRate.RegularHours;
                }
                else
                {
					cost = employeeRate.RateByType / (timecard.TotalSpentCalc / 60);
                }
			}
			else
			{
				cost = employeeRate.HourlyRate * GetOvertimeMultiplier(activity.EarningTypeID, (int)employeeID, date);
			}

			return cost;
		}

		public virtual decimal GetOvertimeMultiplier(string earningTypeID, int employeeID, DateTime effectiveDate)
		{
			EPEmployeeRate employeeRate = GetEmployeeRate(employeeID, effectiveDate);
			if (employeeRate != null && employeeRate.RateType == RateTypesAttribute.SalaryWithExemption)
				return 1;
			EPEarningType earningType = PXSelect<EPEarningType>.Search<EPEarningType.typeCD>(graph, earningTypeID);
			return earningType != null && earningType.IsOvertime == true ? (decimal)earningType.OvertimeMultiplier : 1;
	    }

	    public virtual int? GetLaborClass(CR.EPActivity activity)
		{
			int? laborClassID = null;

			CR.CRCase refCase = PXSelect<CR.CRCase, Where<CR.CRCase.noteID, Equal<Required<CR.EPActivity.refNoteID>>>>.Select(graph, activity.RefNoteID);

			if (refCase != null)
			{
				CR.CRCaseClass caseClass = (CR.CRCaseClass)PXSelectorAttribute.Select<CR.CRCase.caseClassID>(graph.Caches[typeof(CR.CRCase)], refCase);
				laborClassID = CR.CRCaseClassLaborMatrix.GetLaborClassID(graph, caseClass.CaseClassID, activity.EarningTypeID);
			}

			EPEmployee employee = PXSelect<EPEmployee>.Search<EPEmployee.userID>(graph, activity.Owner);
			if (employee == null)
				throw new Exception(Messages.EmptyEmployeeID);

			if (laborClassID == null && activity.ProjectID != null && employee.BAccountID != null)
				laborClassID = EPContractRate.GetProjectLaborClassID(graph, (int)activity.ProjectID, (int)employee.BAccountID, activity.EarningTypeID);

			if (laborClassID == null)
				laborClassID = EPEmployeeClassLaborMatrix.GetLaborClassID(graph, employee.BAccountID, activity.EarningTypeID);

			if (laborClassID == null)
				laborClassID = employee.LabourItemID;

			return laborClassID;
		}



	}
}
