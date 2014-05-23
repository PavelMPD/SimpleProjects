using System.Collections;
using System.Collections.Generic;
using PX.Data;
using System;
using PX.Objects.CR;
using PX.Objects.PM;

namespace PX.Objects.EP
{
    [GL.TableDashboardType]
    public class TimeCardRelease : PXGraph<TimeCardRelease>
    {
        #region Selects

        [PXViewName(Messages.Timecards)]
        [PXFilterable]
        public PXProcessing<EPTimeCardRow> FilteredItems;

        public PXSetup<EPSetup> Setup;

        public IEnumerable filteredItems()
        {
            bool found = false;
            foreach (EPTimeCardRow item in FilteredItems.Cache.Inserted)
            {
                found = true;
                yield return item;
            }
            if (found)
                yield break;

            PXSelectBase<EPTimeCardRow> select = new PXSelectJoin<EPTimeCardRow,
            LeftJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<EPTimeCard.employeeID>>,
            LeftJoin<EPApproval, On<EPApproval.refNoteID, Equal<EPTimeCardRow.noteID>>,
            LeftJoin<EPEmployeeEx, On<EPEmployeeEx.userID, Equal<EPApproval.approvedByID>>>>>,
            Where<EPTimeCardRow.isApproved, Equal<True>,
                And2<Where<EPTimeCardRow.isReleased, NotEqual<True>, Or<EPTimeCardRow.isReleased, IsNull>>,
                And2<Where<EPTimeCardRow.isHold, NotEqual<True>, Or<EPTimeCardRow.isHold, IsNull>>,
                And<Where<EPTimeCardRow.isRejected, NotEqual<True>, Or<EPTimeCardRow.isRejected, IsNull>>>>>>,
            OrderBy<Asc<EPTimeCardRow.timeCardCD,
                    Desc<EPTimeCardRow.approveDate>>>>(this);

            List<string> timeCards = new List<string>();
            foreach (PXResult<EPTimeCardRow, EPEmployee, EPApproval, EPEmployeeEx> res in select.Select())
            {
                EPTimeCardRow rec = (EPTimeCardRow)res;
                if (!timeCards.Contains(rec.TimeCardCD))
                {
                    timeCards.Add(rec.TimeCardCD);
                    EPEmployee employee = (EPEmployee)res[1];
                    EPApproval approval = (EPApproval)res;
                    EPEmployeeEx approver = (EPEmployeeEx)res[3];


                    rec.EmployeeCD = employee.AcctCD;
                    rec.EmployeeName = employee.AcctName;
                    rec.ApproveDate = approval.ApproveDate;
                    rec.ApprovedBy = approver.AcctCD;

                    RecalculateTotals(rec);

                    FilteredItems.Cache.SetStatus(rec, PXEntryStatus.Inserted);
                    yield return rec;
                }
            }

            FilteredItems.Cache.IsDirty = false;
        }

        #endregion

        #region Ctors

        public TimeCardRelease()
        {
            FilteredItems.SetProcessCaption(Messages.Release);
            FilteredItems.SetProcessAllCaption(Messages.ReleaseAll);
            FilteredItems.SetSelected<EPTimeCard.selected>();

            FilteredItems.SetProcessDelegate(TimeCardRelease.Release);

            Actions.Move("Process", "Cancel");
        }

        #endregion
        #region Actions

        public PXCancel<EPTimeCardRow> Cancel;

        public PXAction<EPTimeCardRow> viewDetails;

        [PXUIField(DisplayName = Messages.ViewDetails)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
        public virtual IEnumerable ViewDetails(PXAdapter adapter)
        {
            var row = FilteredItems.Current;
            if (row != null)
            {

                TimeCardMaint graph = PXGraph.CreateInstance<TimeCardMaint>();
                graph.Document.Current = graph.Document.Search<EPTimeCard.timeCardCD>(row.TimeCardCD);
                throw new PXRedirectRequiredException(graph, true, Messages.ViewDetails)
                {
                    Mode = PXBaseRedirectException.WindowMode.NewWindow
                };


            }
            return adapter.Get();
        }

        #endregion

        
        protected virtual void RecalculateTotals(EPTimeCard timecard)
        {
            if (timecard == null)
                throw new ArgumentNullException();

            int timeSpent = 0;
            int regularTime = 0;
            int overtimeSpent = 0;
            int timeBillable = 0;
            int overtimeBillable = 0;

            PXResultset<TimeCardMaint.EPTimecardDetail> resultset = PXSelectJoin<TimeCardMaint.EPTimecardDetail,
                    InnerJoin<EPEarningType, On<TimeCardMaint.EPTimecardDetail.earningTypeID, Equal<EPEarningType.typeCD>>>,
                    Where<TimeCardMaint.EPTimecardDetail.timeCardCD, Equal<Required<EPActivity.timeCardCD>>>>.Select(this, timecard.TimeCardCD);

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

        public static void Release(List<EPTimeCardRow> timeCards)
        {
            TimeCardMaint timeCardMaint = PXGraph.CreateInstance<TimeCardMaint>();
            foreach (EPTimeCardRow item in timeCards)
            {
                timeCardMaint.Clear();
                timeCardMaint.Document.Current = timeCardMaint.Document.Search<EPTimeCard.timeCardCD>(item.TimeCardCD);
                timeCardMaint.release.Press();
            }
        }

      

        [PXHidden()]
        [Serializable]
        public partial class EPTimeCardRow : EPTimeCard
        {
            #region TimeCardCD
            public new abstract class timeCardCD : IBqlField { }

            [PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCC")]
            [PXDefault]
            [PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
            public override String TimeCardCD { get; set; }
            #endregion
            #region EmployeeID
            public new abstract class employeeID : IBqlField { }

            [PXDBInt]
            [PXUIField(DisplayName = "Employee")]
            [PXEPEmployeeSelector]
            public override Int32? EmployeeID { get; set; }
            #endregion

            #region EmployeeCD
            public abstract class employeeCD : IBqlField
            {
            }
            protected string _EmployeeCD;
            [PXString(30, IsUnicode = true, InputMask = "")]
            [PXUIField(DisplayName = "Employee ID", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual string EmployeeCD
            {
                get
                {
                    return this._EmployeeCD;
                }
                set
                {
                    this._EmployeeCD = value;
                }
            }
            #endregion
            #region EmployeeName
            public abstract class employeeName : IBqlField
            {
            }
            protected string _EmployeeName;
            [PXString(60, IsUnicode = true, InputMask = "")]
            [PXUIField(DisplayName = "Employee Name")]
            public virtual string EmployeeName
            {
                get
                {
                    return this._EmployeeName;
                }
                set
                {
                    this._EmployeeName = value;
                }
            }
            #endregion
            #region ApprovedBy
            public abstract class approvedBy : IBqlField
            {
            }
            protected string _ApprovedBy;
            [PXString(30, IsUnicode = true, InputMask = "")]
            [PXUIField(DisplayName = "Approved by", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual string ApprovedBy
            {
                get
                {
                    return this._ApprovedBy;
                }
                set
                {
                    this._ApprovedBy = value;
                }
            }
            #endregion
            #region ApproveDate
            public abstract class approveDate : PX.Data.IBqlField
            {
            }
            protected DateTime? _ApproveDate;
            [PXDate()]
            [PXUIField(DisplayName = "Approve Date", Enabled = false)]
            public virtual DateTime? ApproveDate
            {
                get
                {
                    return this._ApproveDate;
                }
                set
                {
                    this._ApproveDate = value;
                }
            }
            #endregion

            #region IsApproved

            public new abstract class isApproved : IBqlField { }

            #endregion
            #region IsRejected

            public new abstract class isRejected : IBqlField { }

            #endregion
            #region IsHold

            public new abstract class isHold : IBqlField { }

            #endregion
            #region IsReleased

            public new abstract class isReleased : IBqlField { }

            #endregion
            #region CreatedByID

            public new abstract class createdByID : PX.Data.IBqlField { }

            #endregion
            #region LastModifiedByID

            public new abstract class lastModifiedByID : PX.Data.IBqlField { }

            #endregion

        }

        [PXHidden()]
        public partial class EPEmployeeEx : EPEmployee
        {
            #region BAccountID
            public new abstract class bAccountID : IBqlField { }
            #endregion

            #region UserID
            public new abstract class userID : IBqlField { }
            #endregion
        }
    }
}
