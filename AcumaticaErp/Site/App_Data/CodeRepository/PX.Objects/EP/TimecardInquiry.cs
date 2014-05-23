using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data.EP;
using PX.Objects.CR;

namespace PX.Objects.EP
{
    public class TimecardInquiry : PXGraph<TimecardInquiry>
    {
        public PXCancel<TimecardFilter> Cancel;
        public PXFilter<TimecardFilter> Filter;
        public PXSelect<EPTimeCard, Where2<Where<EPTimeCard.weekId, GreaterEqual<Current<TimecardFilter.fromWeekId>>, Or<Current<TimecardFilter.fromWeekId>, IsNull>>,
            And2<Where<EPTimeCard.weekId, LessEqual<Current<TimecardFilter.toWeekId>>, Or<Current<TimecardFilter.toWeekId>, IsNull>>,
            And<Where<Current<TimecardFilter.employeeID>, IsNull, Or<EPTimeCard.employeeID, Equal<Current<TimecardFilter.employeeID>>>>>>>> Items;


        protected virtual void EPTimeCard_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            EPTimeCard row = e.Row as EPTimeCard;
            if (row == null) return;

            RecalculateTotals(row);
        }

        protected virtual void TimecardFilter_FromWeekID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
			e.NewValue = PXWeekSelector2Attribute.GetNextWeekID(this, Accessinfo.BusinessDate.Value);
        }

        protected virtual void TimecardFilter_ToWeekID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
			e.NewValue = PXWeekSelector2Attribute.GetNextWeekID(this, Accessinfo.BusinessDate.Value);
        }



        protected virtual void RecalculateTotals(EPTimeCard timecard)
        {
            if (timecard == null)
                throw new ArgumentNullException();

            int timeSpent = 0;
            int regularTime = 0;
            int overtimeSpent = 0;
            int timeBillable = 0;
            int overtimeBillable = 0;

            PXResultset<TimeCardMaint.EPTimecardDetail> resultset;

            if (timecard.IsHold == true)
            {
                resultset = PXSelectJoin<TimeCardMaint.EPTimecardDetail,
                    InnerJoin<EPEarningType, On<TimeCardMaint.EPTimecardDetail.earningTypeID, Equal<EPEarningType.typeCD>>,
                    InnerJoin<EPEmployee, On<EPEmployee.userID, Equal<TimeCardMaint.EPTimecardDetail.owner>>>>,
                    Where<TimeCardMaint.EPTimecardDetail.weekID, Equal<Required<EPActivity.weekID>>,
                    And<TimeCardMaint.EPTimecardDetail.timeCardCD, IsNull,
                    And<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>>>.Select(this, timecard.WeekID, timecard.EmployeeID);
            }
            else
            {
                resultset = PXSelectJoin<TimeCardMaint.EPTimecardDetail,
                    InnerJoin<EPEarningType, On<TimeCardMaint.EPTimecardDetail.earningTypeID, Equal<EPEarningType.typeCD>>,
                    InnerJoin<EPEmployee, On<EPEmployee.userID, Equal<TimeCardMaint.EPTimecardDetail.owner>>>>,
                    Where<TimeCardMaint.EPTimecardDetail.timeCardCD, Equal<Required<TimeCardMaint.EPTimecardDetail.timeCardCD>>,
                    And<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>>.Select(this, timecard.TimeCardCD,  timecard.EmployeeID);
            }

            foreach (PXResult<TimeCardMaint.EPTimecardDetail, EPEarningType> res in resultset)
            {
                TimeCardMaint.EPTimecardDetail activity = (TimeCardMaint.EPTimecardDetail)res;
                EPEarningType et = (EPEarningType)res;
                activity.IsOvertimeCalc = et.IsOvertime;
                RecalculateFields(activity);

                timeSpent += activity.TimeSpent.GetValueOrDefault();
                regularTime += activity.RegularTimeCalc.GetValueOrDefault();
                timeBillable += activity.BillableTimeCalc.GetValueOrDefault();
                overtimeSpent += activity.OverTimeCalc.GetValueOrDefault();
                overtimeBillable += activity.BillableOvertimeCalc.GetValueOrDefault();
            }
            
            timecard.TimeSpentCalc = regularTime;
            timecard.OvertimeSpentCalc = overtimeSpent;
            timecard.TotalSpentCalc = timeSpent;

            timecard.TimeBillableCalc = timeBillable;
            timecard.OvertimeBillableCalc = overtimeBillable;
            timecard.TotalBillableCalc = timeBillable + overtimeBillable;
        }

        protected virtual void RecalculateFields(TimeCardMaint.EPTimecardDetail row)
        {
            if (row == null)
                throw new ArgumentNullException();

            if (row.IsOvertimeCalc == true)
            {
                row.OverTimeCalc = row.TimeSpent;
                row.RegularTimeCalc = null;
                if (row.IsBillable == true)
                {
                    row.BillableOvertimeCalc = row.TimeBillable;
                }
                else
                {
                    row.BillableOvertimeCalc = null;
                }
            }
            else
            {
                row.OverTimeCalc = null;
                row.RegularTimeCalc = row.TimeSpent;
                if (row.IsBillable == true)
                {
                    row.BillableTimeCalc = row.TimeBillable;
                }
                else
                {
                    row.BillableTimeCalc = null;
                }
            }
        }

        [Serializable]
        public partial class TimecardFilter : IBqlTable
        {
            #region EmployeeID
            public abstract class employeeID : IBqlField { }

            [PXDBInt]
            [PXDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>))]
            [PXUIField(DisplayName = "Employee")]
            [PXSubordinateSelector(typeof(Where<EPEmployee.timeCardRequired, Equal<True>>))]
            [PXFieldDescription]
            public virtual Int32? EmployeeID { get; set; }
            #endregion
            #region FromWeekID

            public abstract class fromWeekId : IBqlField { }

            protected Int32? _FromWeekID;
            [PXDBInt]
            [PXUIField(DisplayName = "From Week")]
            [PXWeekSelector2(DescriptionField = typeof(EPWeekRaw.shortDescription))]
            public virtual Int32? FromWeekID
            {
                get
                {
                    return this._FromWeekID;
                }
                set
                {
                    this._FromWeekID = value;
                }
            }

            #endregion
            #region ToWeekID

            public abstract class toWeekId : IBqlField { }

            protected Int32? _ToWeekID;
            [PXDBInt]
            [PXUIField(DisplayName = "To Week")]
            [PXWeekSelector2(DescriptionField = typeof(EPWeekRaw.shortDescription))]
            public virtual Int32? ToWeekID
            {
                get
                {
                    return this._ToWeekID;
                }
                set
                {
                    this._ToWeekID = value;
                }
            }

            #endregion
        }
    }
}
