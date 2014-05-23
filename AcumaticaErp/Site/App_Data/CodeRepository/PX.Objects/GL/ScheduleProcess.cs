using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.BQLConstants;
using PX.Objects.GL.Overrides.ScheduleProcess;
using PX.Objects.CS;
using PX.Objects.CM;

namespace PX.Objects.GL
{
	public class ScheduleDet
	{
		public DateTime? ScheduledDate;
		public string ScheduledPeriod;
	}

	[PX.Objects.GL.TableAndChartDashboardType]
	public class ScheduleRun : PXGraph<ScheduleRun>
	{
        #region Cache Attached Events
        #region Schedule
        #region ScheduleID

        [PXDBString(10, IsUnicode = true, IsKey = true)]
        [PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.Visible, Enabled = false)]        
        protected virtual void Schedule_ScheduleID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion
        #endregion
        [Serializable]
		public partial class Parameters : IBqlTable
		{
			#region StartDate
			public abstract class startDate : IBqlField {}

            [PXDBDate]
            [PXUIField(DisplayName = "Start on", Visibility = PXUIVisibility.Visible)]
            public virtual DateTime? StartDate { get; set; }
			#endregion
            #region LimitTypeSel
            public abstract class limitTypeSel : IBqlField
			{
                public const string RunTillDate = "D";
                public const string RunMultipleTimes = "M";

                public class ListAttribute : PXStringListAttribute
			{
                    public ListAttribute()
                        : base(
                        new string[] { RunTillDate, RunMultipleTimes},
                        new string[] { Messages.RunTillDate, Messages.RunMultipleTimes }) { }
				}
				}
            [PXDBString(1, IsFixed = true)]
            [PXUIField(DisplayName = "Stop", Visibility = PXUIVisibility.Visible, Required = true)]
            [PXDefault(limitTypeSel.RunMultipleTimes)]
            [limitTypeSel.List]
            public virtual string LimitTypeSel { get; set; }
			#endregion
			#region EndDate
			public abstract class endDate : IBqlField {}
            [PXDBDate]
			[PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "On this date", Visibility = PXUIVisibility.Visible)]
            public virtual DateTime? EndDate { get; set; }
			#endregion
            #region RunLimit
            public abstract class runLimit : IBqlField {}
            [PXDBShort]
            [PXUIField(DisplayName = "After running this number of schedules", Visibility = PXUIVisibility.Visible)]
            [PXDefault((short)1, PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual short? RunLimit { get; set; }
            #endregion
		}

		public PXFilter<Parameters> Filter;
		public PXCancel<Parameters> Cancel;
		[PXFilterable]
		public PXFilteredProcessing<Schedule, Parameters,
			Where<Schedule.active, Equal<boolTrue>,
			And<Schedule.module, Equal<BatchModule.moduleGL>,
			And<Schedule.nextRunDate, LessEqual<Current<Parameters.endDate>>,
			And<Where<Current<Parameters.startDate>, IsNull, Or<Schedule.nextRunDate, GreaterEqual<Current<Parameters.startDate>>>>>>>>>
			Schedule_List;

		public PXSetup<GLSetup> GLSetup;

		public ScheduleRun()
		{
			GLSetup setup = GLSetup.Current;
			Schedule_List.SetProcessCaption(Messages.ProcRunSelected);
            Schedule_List.SetProcessAllCaption(Messages.ProcRunAll);
		}

		public PXAction<Parameters> NewSchedule;
		[PXUIField(DisplayName = "New Schedule", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable newSchedule(PXAdapter adapter)
		{
			ScheduleMaint graph = CreateInstance<ScheduleMaint>();
			graph.Schedule_Header.Insert(new Schedule());
			graph.Schedule_Header.Cache.IsDirty = false;
			throw new PXRedirectRequiredException(graph, true, "New Schedule"){Mode =PXBaseRedirectException.WindowMode.NewWindow};
		}

		public PXAction<Parameters> ViewSchedule;
		[PXUIField(DisplayName = "View Schedule", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable viewSchedule(PXAdapter adapter)
		{
			if (Schedule_List.Current != null)
			{
				PXRedirectHelper.TryRedirect(Schedule_List.Cache, Schedule_List.Current, "Schedule", PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

	    public static void SetProcessDelegate<ProcessGraph>(PXGraph graph, Parameters filter, PXProcessing<Schedule> view)
            where ProcessGraph : PXGraph<ProcessGraph>, IScheduleProcessing, new()
        {
            if (filter == null) return;

            short Times = Parameters.limitTypeSel.RunMultipleTimes == filter.LimitTypeSel ? (filter.RunLimit ?? 1) : short.MaxValue;
            DateTime Date = Parameters.limitTypeSel.RunTillDate == filter.LimitTypeSel ? (filter.EndDate ?? ((AccessInfo)graph.Caches[typeof(AccessInfo)].Current).BusinessDate.Value) : DateTime.MaxValue;
            view.SetProcessDelegate(
                delegate(ProcessGraph sp, Schedule schedule)
                {
                    sp.Clear();
                    sp.GenerateProc(schedule, Times, Date);
                }
            );
        }

        protected virtual void Parameters_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            SetProcessDelegate<ScheduleProcess>(this, (Parameters)e.Row, Schedule_List);
        }

		protected virtual void Parameters_StartDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Parameters filterData = (Parameters)e.Row;
			if (filterData.EndDate.HasValue && filterData.StartDate > filterData.EndDate)
				filterData.EndDate = filterData.StartDate;
		}

		protected virtual void Schedule_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			Schedule task = (Schedule)e.Row;
			if (task == null || PXLongOperation.Exists(UID)) return;

			if ((task.NoRunLimit ?? false) == false && task.RunCntr == task.RunLimit)
			{
				cache.RaiseExceptionHandling<Schedule.scheduleID>(e.Row, task.ScheduleID, new PXSetPropertyException(Messages.SheduleExecutionLimitExceeded, PXErrorLevel.RowError));
			}
			else
			{
				if ((task.NoEndDate ?? false) == false && task.EndDate < Accessinfo.BusinessDate)
				{
					cache.RaiseExceptionHandling<Schedule.scheduleID>(e.Row, task.ScheduleID, new PXSetPropertyException(Messages.SheduleHasExpired, PXErrorLevel.RowError));
				}
				else
				{
					if (task.NextRunDate > Accessinfo.BusinessDate)
					{
						cache.RaiseExceptionHandling<Schedule.scheduleID>(e.Row, task.ScheduleID, new PXSetPropertyException(Messages.SheduleNextExecutionDateExceeded, PXErrorLevel.RowWarning));
					}
				}
			}
		}

		}

    public interface IScheduleProcessing
    {
        void GenerateProc(Schedule s);
        void GenerateProc(Schedule s, short Times, DateTime runDate);
	}

	public class ScheduleProcess : PXGraph<ScheduleProcess>, IScheduleProcessing
	{
		public PXSelect<Schedule> Running_Schedule;
		public PXSelect<BatchNew> Batch_Created;
		public PXSelect<GLTranNew> Tran_Created;
		public PXSelect<CurrencyInfo> CuryInfo_Created;

		public GLSetup GLSetup
		{
			get
			{
				return PXSelect<GLSetup>.Select(this);
			}
		}

		public ScheduleProcess()
		{
			//AutoNumberAttribute.SetAllowOverride<BatchNew.batchNbr>(Batch_Created.Cache, null, true);
		}

		protected virtual void BatchNew_BatchNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

        private List<ScheduleDet> MakeSchedule(Schedule s, short Times, DateTime runDate)
		{
			return MakeSchedule(this, s, Times, runDate);
		}

        public static List<ScheduleDet> MakeSchedule(PXGraph graph, Schedule s, short Times)
		{
			return MakeSchedule(graph, s, Times, graph.Accessinfo.BusinessDate.Value);
		}

		public static List<ScheduleDet> MakeSchedule(PXGraph graph, Schedule s, short Times, DateTime runDate)
		{
			List<ScheduleDet> ret = new List<ScheduleDet>();

            if (s.NextRunDate == null && s.LastRunDate == null)
            {
                s.NextRunDate = s.StartDate;
            }
            else if (s.NextRunDate == null)
            {
                s.NextRunDate = s.LastRunDate;
            }

		    int i = 0;
            short oldRunCntr = s.RunCntr ?? 0;
			do
			{
				ScheduleDet d = new ScheduleDet();
                do
                {
                    switch (s.ScheduleType)
                    {
                        case "D":
                            d.ScheduledDate = s.NextRunDate;
                            s.NextRunDate = ((DateTime)s.NextRunDate).AddDays((short)s.DailyFrequency);
                            break;
                        case "W":
                            CalcRunDatesWeekly(s, d);
                            break;
                        case "P":
                            try
                            {
                                switch (s.PeriodDateSel)
                                {
                                    case "S":
                                        d.ScheduledDate = FinPeriodIDAttribute.PeriodStartDate(graph, FinPeriodIDAttribute.PeriodFromDate(graph, s.NextRunDate));
                                        s.NextRunDate = FinPeriodIDAttribute.PeriodStartDate(graph, FinPeriodIDAttribute.PeriodPlusPeriod(graph, FinPeriodIDAttribute.PeriodFromDate(graph, s.NextRunDate), (short)s.PeriodFrequency));
                                        break;
                                    case "E":
                                        d.ScheduledDate = FinPeriodIDAttribute.PeriodEndDate(graph, FinPeriodIDAttribute.PeriodFromDate(graph, s.NextRunDate));
                                        s.NextRunDate = FinPeriodIDAttribute.PeriodEndDate(graph, FinPeriodIDAttribute.PeriodPlusPeriod(graph, FinPeriodIDAttribute.PeriodFromDate(graph, s.NextRunDate), (short)s.PeriodFrequency));
                                        break;
                                    case "D":
                                        d.ScheduledDate = FinPeriodIDAttribute.PeriodStartDate(graph, FinPeriodIDAttribute.PeriodFromDate(graph, s.NextRunDate)).AddDays((short)s.PeriodFixedDay - 1);
                                        if (((DateTime)d.ScheduledDate).CompareTo(FinPeriodIDAttribute.PeriodEndDate(graph, FinPeriodIDAttribute.PeriodFromDate(graph, s.NextRunDate))) > 0)
                                        {
                                            d.ScheduledDate = FinPeriodIDAttribute.PeriodEndDate(graph, FinPeriodIDAttribute.PeriodFromDate(graph, s.NextRunDate));
                                        }

                                        DateTime? OldNextRunDate = s.NextRunDate;
                                        s.NextRunDate = FinPeriodIDAttribute.PeriodStartDate(graph, FinPeriodIDAttribute.PeriodPlusPeriod(graph, FinPeriodIDAttribute.PeriodFromDate(graph, s.NextRunDate), (short)s.PeriodFrequency)).AddDays((short)s.PeriodFixedDay - 1);
                                        if (((DateTime)s.NextRunDate).CompareTo(FinPeriodIDAttribute.PeriodEndDate(graph, FinPeriodIDAttribute.PeriodPlusPeriod(graph, FinPeriodIDAttribute.PeriodFromDate(graph, OldNextRunDate), (short)s.PeriodFrequency))) > 0)
                                        {
                                            s.NextRunDate = FinPeriodIDAttribute.PeriodEndDate(graph, FinPeriodIDAttribute.PeriodPlusPeriod(graph, FinPeriodIDAttribute.PeriodFromDate(graph, OldNextRunDate), (short)s.PeriodFrequency));
                                        }
                                        break;
                                }
                            }
                            catch (PXFinPeriodException)
                            {
                                if (d.ScheduledDate != null && (bool)s.NoRunLimit == false && s.RunCntr + 1 == s.RunLimit)
                                {
                                    s.NextRunDate = null;
                                }
                                else
                                {
                                    s.RunCntr = oldRunCntr;
                                    throw;
                                }
                            }
                            break;
                        case "M":
                            switch (s.MonthlyDaySel)
                            {
                                case "D":
                                    d.ScheduledDate = new PXDateTime((short)((DateTime)s.NextRunDate).Year, (short)((DateTime)s.NextRunDate).Month, (short)s.MonthlyOnDay);
                                    s.NextRunDate = PXDateTime.DatePlusMonthSetDay((DateTime)d.ScheduledDate, (short)s.MonthlyFrequency, (short)s.MonthlyOnDay);
                                    break;
                                case "W":
                                    d.ScheduledDate = PXDateTime.MakeDayOfWeek((short)((DateTime)s.NextRunDate).Year, (short)((DateTime)s.NextRunDate).Month, (short)s.MonthlyOnWeek, (short)s.MonthlyOnDayOfWeek);

                                    //s.NextRunDate = ((DateTime)d.ScheduledDate).AddMonths((short)s.MonthlyFrequency);
                                    s.NextRunDate = new PXDateTime(((DateTime)d.ScheduledDate).Year, ((DateTime)d.ScheduledDate).Month, 1).AddMonths((short)s.MonthlyFrequency);
                                    s.NextRunDate = PXDateTime.MakeDayOfWeek((short)((DateTime)s.NextRunDate).Year, (short)((DateTime)s.NextRunDate).Month, (short)s.MonthlyOnWeek, (short)s.MonthlyOnDayOfWeek);
                                    break;
                            }
                            break;
                        default:
                            throw new PXException();
                    }
                } while (d.ScheduledDate == s.LastRunDate);

                if (d.ScheduledDate != null
                    && ((DateTime)d.ScheduledDate).CompareTo(s.StartDate) >= 0
                    && ((bool)s.NoEndDate || ((DateTime)d.ScheduledDate).CompareTo(s.EndDate) <= 0)
                    && (((DateTime)d.ScheduledDate).CompareTo(runDate) <= 0)
                    && ((bool)s.NoRunLimit || s.RunCntr < s.RunLimit))
                {
                    try
                    {
                        d.ScheduledPeriod = FinPeriodIDAttribute.PeriodFromDate(graph, d.ScheduledDate);
                    }
                    catch
                    {
                        s.RunCntr = oldRunCntr;
                        throw;
                    }
                    ret.Add(d);
                    s.RunCntr++;

                    if ((bool)s.NoRunLimit == false && s.RunCntr >= s.RunLimit
                        || (s.NextRunDate != null) && (bool)s.NoEndDate == false && ((DateTime)s.NextRunDate).CompareTo(s.EndDate) > 0)
                    {
                        s.Active = false;
                        s.NextRunDate = null;
                    }

                    if (s.NextRunDate == null)
                    {
                        break;
                    }

                    if (++i >= Times)
                    {
                        return ret;
                    }
                }
                else
                {
                    if (d.ScheduledDate != null)
                    {
                        s.NextRunDate = d.ScheduledDate;
                    }
                    break;
                }
			} while (true);

			return ret;
		}

        private static void CalcRunDatesWeekly(Schedule s, ScheduleDet d)
        {
            DateTime? nextRunDateL = s.NextRunDate, iteratorDate = s.NextRunDate;
            DateTime? scheduledDateL = null;
            do
            {
                DayOfWeek dow = ((DateTime)iteratorDate).DayOfWeek;
                if ((bool)s.WeeklyOnDay1 && dow == DayOfWeek.Sunday
                    || (bool)s.WeeklyOnDay2 && dow == DayOfWeek.Monday
                    || (bool)s.WeeklyOnDay3 && dow == DayOfWeek.Tuesday
                    || (bool)s.WeeklyOnDay4 && dow == DayOfWeek.Wednesday
                    || (bool)s.WeeklyOnDay5 && dow == DayOfWeek.Thursday
                    || (bool)s.WeeklyOnDay6 && dow == DayOfWeek.Friday)
                {
                    if (scheduledDateL == null)
                    {
                        scheduledDateL = iteratorDate;
                    }
                    else
                    {
                        nextRunDateL = iteratorDate;
                    }
                }
                else if (dow == DayOfWeek.Saturday)
                {
                    if ((bool)s.WeeklyOnDay7)
                    {
                        if (scheduledDateL == null)
                        {
                            scheduledDateL = iteratorDate;
                        }
                        else
                        {
                            nextRunDateL = iteratorDate;
                        }
                    }
                    if (nextRunDateL == s.NextRunDate)
                    {
                        iteratorDate = ((DateTime)iteratorDate).AddDays(7 * ((short)s.WeeklyFrequency - 1) + 1);
                        continue;
                    }
                }
                iteratorDate = ((DateTime)iteratorDate).AddDays(1);
            } while (nextRunDateL == s.NextRunDate);
            s.NextRunDate = nextRunDateL;
            d.ScheduledDate = scheduledDateL;
        }


		public static DateTime? GetNextRunDate(PXGraph graph, Schedule s)
		{
            if (s.Active == false)
            {
                return null;
            }
            
            Schedule copy = PXCache<Schedule>.CreateCopy(s);

            if (copy.LastRunDate != null && !(copy.Active == true && copy.NextRunDate == null))
            {
                return copy.NextRunDate;
            }

			copy.NextRunDate = null;

			try
			{
				DateTime? BusinessDate = graph.Accessinfo.BusinessDate;
				graph.Accessinfo.BusinessDate = copy.NoEndDate == true ? DateTime.MaxValue : copy.EndDate;

				try
				{
					List<ScheduleDet> det = MakeSchedule(graph, copy, 1);

					if (det.Count > 0)
					{
						return det[0].ScheduledDate;
					}
				}
				finally
				{
					graph.Accessinfo.BusinessDate = BusinessDate;
				}
			}
			catch (PXFinPeriodException)
			{
                if (s.NextRunDate != null)
                {
                    return s.NextRunDate;
                }
                else
                {
                    throw;
                }
			}
			return copy.NextRunDate;
		}

		public virtual void GenerateProc(Schedule s)
		{
            GenerateProc(s, 1, Accessinfo.BusinessDate.Value);
        }

		public virtual void GenerateProc(Schedule s, short Times, DateTime runDate)
		{
			string LastBatchNbr = "0000000000";
			Int64 LastInfoID = -1;
			List<ScheduleDet> sd = MakeSchedule(s, Times, runDate);
			
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					foreach (ScheduleDet d in sd)
					{
						foreach (BatchNew b in PXSelect<BatchNew, Where<BatchNew.scheduleID, Equal<Optional<Schedule.scheduleID>>, And<BatchNew.scheduled,Equal<boolTrue>>>>.Select(this, s.ScheduleID))
						{
							BatchNew copy = PXCache<BatchNew>.CreateCopy(b);
							copy.OrigBatchNbr = copy.BatchNbr;
							copy.OrigModule = copy.Module;
							copy.CuryInfoID = null;
							copy.NumberCode = "GLREC";
							copy.NoteID = null;

							CurrencyInfo info = (CurrencyInfo) PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(this, b.CuryInfoID);

							if (info != null)
							{
								CurrencyInfo infocopy = PXCache<CurrencyInfo>.CreateCopy(info);
								infocopy.CuryInfoID = LastInfoID;
								copy.CuryInfoID = LastInfoID;
								CuryInfo_Created.Cache.Insert(infocopy);
							}

							copy.Posted = false;
							copy.Released = false;
							copy.Status = "B";
							copy.Scheduled = false;

							copy.DateEntered = d.ScheduledDate;
							copy.FinPeriodID = d.ScheduledPeriod;
							copy.TranPeriodID = null;

							copy.BatchNbr = LastBatchNbr;
							copy.RefBatchNbr = LastBatchNbr;
							LastBatchNbr = AutoNumberAttribute.NextNumber(LastBatchNbr);
							LastInfoID--;

							copy = (BatchNew) Batch_Created.Cache.Insert(copy);

							CurrencyInfoAttribute.SetEffectiveDate<Batch.dateEntered>(Batch_Created.Cache, new PXFieldUpdatedEventArgs(copy, null, false));
							PXNoteAttribute.SetNote(Batch_Created.Cache, copy, PXNoteAttribute.GetNote(Caches[typeof(BatchNew)], b));
							PXNoteAttribute.SetFileNotes(Batch_Created.Cache, copy, PXNoteAttribute.GetFileNotes(Caches[typeof(BatchNew)], b));

							foreach (GLTranNew tran in PXSelect<GLTranNew, Where<GLTranNew.module, Equal<Required<GLTranNew.module>>, And<GLTranNew.batchNbr, Equal<Required<GLTranNew.batchNbr>>>>>.Select(this, b.Module, b.BatchNbr))
							{
								GLTranNew trancopy = PXCache<GLTranNew>.CreateCopy(tran);
								trancopy.OrigBatchNbr = trancopy.BatchNbr;
								trancopy.OrigModule = trancopy.Module;
								trancopy.BatchNbr = copy.BatchNbr;
								trancopy.RefBatchNbr = copy.RefBatchNbr;
								trancopy.CuryInfoID = copy.CuryInfoID;
								trancopy.CATranID = null;

								trancopy.TranDate = d.ScheduledDate;
								trancopy.FinPeriodID = d.ScheduledPeriod;
								trancopy.TranPeriodID = d.ScheduledPeriod;
								Tran_Created.Cache.Insert(trancopy);
							}
						}

						s.LastRunDate = d.ScheduledDate;
						Running_Schedule.Cache.Update(s);
					}
					Running_Schedule.Cache.Persist(PXDBOperation.Update);

					Batch_Created.Cache.Persist(PXDBOperation.Insert);
					Batch_Created.Cache.Persist(PXDBOperation.Update);

					foreach (GLTranNew tran in Tran_Created.Cache.Inserted)
					{
						foreach (BatchNew batch in Batch_Created.Cache.Cached)
						{ 
							if (object.Equals(batch.RefBatchNbr, tran.RefBatchNbr))
							{
								tran.BatchNbr = batch.BatchNbr;
								tran.CuryInfoID = batch.CuryInfoID;
								if (!string.IsNullOrEmpty(batch.RefNbr))
								{
									tran.RefNbr = batch.RefNbr;
								}
								break;
							}
						} 
					}
					Tran_Created.Cache.Normalize();

					Tran_Created.Cache.Persist(PXDBOperation.Insert);
					Tran_Created.Cache.Persist(PXDBOperation.Update);
					Caches[typeof(CA.CADailySummary)].Persist(PXDBOperation.Insert);

					ts.Complete(this);
				}
				Running_Schedule.Cache.Persisted(false);
				Batch_Created.Cache.Persisted(false);
				Tran_Created.Cache.Persisted(false);
				Caches[typeof(CA.CADailySummary)].Persisted(false);
			}
		}
	}

namespace PX.Objects.GL.Overrides.ScheduleProcess
{

	[System.SerializableAttribute()]
	public partial class BatchNew : Batch
	{
		#region Module
		public new abstract class module : PX.Data.IBqlField
		{
		}
		[PXDBString(2, IsKey = true, IsFixed = true)]
		public override String Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;
			}
		}
		#endregion
		#region BatchNbr
		public new abstract class batchNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[AutoNumber(typeof(GLSetup.batchNumberingID), typeof(BatchNew.dateEntered))]
		public override String BatchNbr
		{
			get
			{
				return this._BatchNbr;
			}
			set
			{
				this._BatchNbr = value;
			}
		}
		#endregion
		#region RefBatchNbr
		public abstract class refBatchNbr : PX.Data.IBqlField
		{
		}
		protected string _RefBatchNbr;
		[PXString(15, IsUnicode = true)]
		public virtual string RefBatchNbr
		{
			get
			{
				return this._RefBatchNbr;
			}
			set
			{
				this._RefBatchNbr = value;
			}
		}
		#endregion
		#region DateEntered
		public new abstract class dateEntered : PX.Data.IBqlField
		{
		}
		[PXDBDate()]
		public override DateTime? DateEntered
		{
			get
			{
				return this._DateEntered;
			}
			set
			{
				this._DateEntered = value;
			}
		}
		#endregion
	}
	[System.SerializableAttribute()]
	public partial class GLTranNew : GLTran
	{
		#region Module
		public new abstract class module : PX.Data.IBqlField
		{
		}
		[PXDBString(2, IsKey = true, IsFixed = true)]
		public override String Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;
			}
		}
		#endregion
		#region BatchNbr
		public new abstract class batchNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		public override string BatchNbr
		{
			get
			{
				return this._BatchNbr;
			}
			set
			{
				this._BatchNbr = value;
			}
		}
		#endregion
		#region RefBatchNbr
		public abstract class refBatchNbr : PX.Data.IBqlField
		{
		}
		protected string _RefBatchNbr;
		[PXString(15, IsUnicode = true)]
		public virtual string RefBatchNbr
		{
			get
			{
				return this._RefBatchNbr;
			}
			set
			{
				this._RefBatchNbr = value;
			}
		}
		#endregion
		#region LedgerID
		[PXDBInt()]
		public override Int32? LedgerID
		{
			get
			{
				return this._LedgerID;
			}
			set
			{
				this._LedgerID = value;
			}
		}
		#endregion
		#region AccountID
		[PXDBInt()]
		public override Int32? AccountID
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
		[PXDBInt()]
		public override Int32? SubID
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
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		[PXDBLong()]
		public override Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region ReferenceNbr
		[PXDBString(15, IsUnicode=true)]
		public override String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region TranDesc
		[PXDBString(60, IsUnicode = true)]
		public override String TranDesc
		{
			get
			{
				return this._TranDesc;
			}
			set
			{
				this._TranDesc = value;
			}
		}
		#endregion
		#region TranDate
		public new abstract class tranDate : PX.Data.IBqlField
		{
		}
		[PXDBDate()]
		public override DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		[PXDBString(6, IsFixed = true)]
		public override String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region TranPeriodID
		public new abstract class tranPeriodID : PX.Data.IBqlField
		{
		}
		[PXDBString(6, IsFixed = true)]
		public override String TranPeriodID
		{
			get
			{
				return this._TranPeriodID;
			}
			set
			{
				this._TranPeriodID = value;
			}
		}
		#endregion
	}
}

