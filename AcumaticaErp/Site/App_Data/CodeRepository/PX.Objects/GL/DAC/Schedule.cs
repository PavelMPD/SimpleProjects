using PX.Data.EP;

namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	using PX.Objects.CS;

	[PXCacheName(Messages.Schedule)]
	[System.SerializableAttribute()]
	[PXPrimaryGraph(new Type[] { 
		typeof(GL.ScheduleMaint), 
		typeof(AP.APScheduleMaint), 
		typeof(AR.ARScheduleMaint) 
	},
		new Type[] { 
			typeof(Where<Schedule.module, Equal<BatchModule.moduleGL>>), 
			typeof(Where<Schedule.module, Equal<BatchModule.moduleAP>>),
			typeof(Where<Schedule.module, Equal<BatchModule.moduleAR>>)
	})]
	public partial class Schedule : PX.Data.IBqlTable
	{
        #region Selected
        public abstract class selected : IBqlField
        {
        }
        protected bool? _Selected = false;
        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Selected")]
        public bool? Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
            }
        }
        #endregion	
		#region ScheduleID
		public abstract class scheduleID : PX.Data.IBqlField
		{
		}
		protected string _ScheduleID;
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(GLSetup.scheduleNumberingID), typeof(AccessInfo.businessDate))]
		[PXSelector(typeof(Search<Schedule.scheduleID, Where<Schedule.module, Equal<Current<Schedule.module>>>>))]
		[PXFieldDescription]
		[PXDefault]
		public virtual string ScheduleID
		{
			get
			{
				return this._ScheduleID;
			}
			set
			{
				this._ScheduleID = value;
			}
		}
		#endregion
		#region ScheduleName
		public abstract class scheduleName : PX.Data.IBqlField
		{
		}
		protected String _ScheduleName;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String ScheduleName
		{
			get
			{
				return this._ScheduleName;
			}
			set
			{
				this._ScheduleName = value;
			}
		}
		#endregion
		#region Active
		public abstract class active : PX.Data.IBqlField
		{
		}
		protected Boolean? _Active;
		[PXDBBool()]
		[PXUIField(DisplayName = "Active", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault((Boolean)true)]
		public virtual Boolean? Active
		{
			get
			{
				return this._Active;
			}
			set
			{
				this._Active = value;
			}
		}
		#endregion
		#region ScheduleType
		public abstract class scheduleType : PX.Data.IBqlField
		{
		}
		protected String _ScheduleType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(GLScheduleType.Periodically)]
		[PXUIField(DisplayName = "Schedule Type", Visibility = PXUIVisibility.SelectorVisible, Enabled=false)]
		[GLScheduleType.FullList()]
		public virtual String ScheduleType
		{
			get
			{
				return this._ScheduleType;
			}
			set
			{
				this._ScheduleType = value;
			}
		}
		#endregion
		#region FormScheduleType
		public abstract class formScheduleType : PX.Data.IBqlField
		{
		}
		[PXString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Schedule Type", Visibility = PXUIVisibility.Visible)]
		[GLScheduleType.FullList()]
		public virtual String FormScheduleType
		{
			get
			{
				return this._ScheduleType;
			}
			set
			{
				this._ScheduleType = value;
			}
		}
		#endregion
		#region DailyFrequency
		public abstract class dailyFrequency : PX.Data.IBqlField
		{
		}
		protected Int16? _DailyFrequency;
		[PXDBShort()]
		[PXUIField(DisplayName = "Every", Visibility = PXUIVisibility.Visible)]
		[PXDefault((short)1)]
		public virtual Int16? DailyFrequency
		{
			get
			{
				return this._DailyFrequency;
			}
			set
			{
				this._DailyFrequency = value;
			}
		}
		#endregion
		#region WeeklyFrequency
		public abstract class weeklyFrequency : PX.Data.IBqlField
		{
		}
		protected Int16? _WeeklyFrequency;
		[PXDBShort()]
		[PXUIField(DisplayName = "Every", Visibility = PXUIVisibility.Visible)]
		[PXDefault((short)1)]
		public virtual Int16? WeeklyFrequency
		{
			get
			{
				return this._WeeklyFrequency;
			}
			set
			{
				this._WeeklyFrequency = value;
			}
		}
		#endregion
		#region WeeklyOnDay1
		public abstract class weeklyOnDay1 : PX.Data.IBqlField
		{
		}
		protected Boolean? _WeeklyOnDay1;
		[PXDBBool()]
		[PXUIField(DisplayName = "Sunday", Visibility = PXUIVisibility.Visible)]
		[PXDefault((Boolean)true)]
		public virtual Boolean? WeeklyOnDay1
		{
			get
			{
				return this._WeeklyOnDay1;
			}
			set
			{
				this._WeeklyOnDay1 = value;
			}
		}
		#endregion
		#region WeeklyOnDay2
		public abstract class weeklyOnDay2 : PX.Data.IBqlField
		{
		}
		protected Boolean? _WeeklyOnDay2;
		[PXDBBool()]
		[PXUIField(DisplayName = "Monday", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? WeeklyOnDay2
		{
			get
			{
				return this._WeeklyOnDay2;
			}
			set
			{
				this._WeeklyOnDay2 = value;
			}
		}
		#endregion
		#region WeeklyOnDay3
		public abstract class weeklyOnDay3 : PX.Data.IBqlField
		{
		}
		protected Boolean? _WeeklyOnDay3;
		[PXDBBool()]
		[PXUIField(DisplayName = "Tuesday", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? WeeklyOnDay3
		{
			get
			{
				return this._WeeklyOnDay3;
			}
			set
			{
				this._WeeklyOnDay3 = value;
			}
		}
		#endregion
		#region WeeklyOnDay4
		public abstract class weeklyOnDay4 : PX.Data.IBqlField
		{
		}
		protected Boolean? _WeeklyOnDay4;
		[PXDBBool()]
		[PXUIField(DisplayName = "Wednesday", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? WeeklyOnDay4
		{
			get
			{
				return this._WeeklyOnDay4;
			}
			set
			{
				this._WeeklyOnDay4 = value;
			}
		}
		#endregion
		#region WeeklyOnDay5
		public abstract class weeklyOnDay5 : PX.Data.IBqlField
		{
		}
		protected Boolean? _WeeklyOnDay5;
		[PXDBBool()]
		[PXUIField(DisplayName = "Thursday", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? WeeklyOnDay5
		{
			get
			{
				return this._WeeklyOnDay5;
			}
			set
			{
				this._WeeklyOnDay5 = value;
			}
		}
		#endregion
		#region WeeklyOnDay6
		public abstract class weeklyOnDay6 : PX.Data.IBqlField
		{
		}
		protected Boolean? _WeeklyOnDay6;
		[PXDBBool()]
		[PXUIField(DisplayName = "Friday", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? WeeklyOnDay6
		{
			get
			{
				return this._WeeklyOnDay6;
			}
			set
			{
				this._WeeklyOnDay6 = value;
			}
		}
		#endregion
		#region WeeklyOnDay7
		public abstract class weeklyOnDay7 : PX.Data.IBqlField
		{
		}
		protected Boolean? _WeeklyOnDay7;
		[PXDBBool()]
		[PXUIField(DisplayName = "Saturday", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? WeeklyOnDay7
		{
			get
			{
				return this._WeeklyOnDay7;
			}
			set
			{
				this._WeeklyOnDay7 = value;
			}
		}
		#endregion
		#region MonthlyFrequency
		public abstract class monthlyFrequency : PX.Data.IBqlField
		{
		}
		protected Int16? _MonthlyFrequency;
		[PXDBShort()]
		[PXUIField(DisplayName = "Every", Visibility = PXUIVisibility.Visible)]
		[PXDefault((short)1)]
		[PXIntList(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" })]
		public virtual Int16? MonthlyFrequency
		{
			get
			{
				return this._MonthlyFrequency;
			}
			set
			{
				this._MonthlyFrequency = value;
			}
		}
		#endregion
		#region MonthlyDaySel
		public abstract class monthlyDaySel : PX.Data.IBqlField
		{
		}
		protected String _MonthlyDaySel;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Day Selection", Visibility = PXUIVisibility.Visible)]
		[PXStringList(new string[] { "D", "W" }, new string[] { "On Day", "On the" })]
		[PXDefault("D")]
		public virtual String MonthlyDaySel
		{
			get
			{
				return this._MonthlyDaySel;
			}
			set
			{
				this._MonthlyDaySel = value;
			}
		}
		#endregion
		#region MonthlyOnDay
		public abstract class monthlyOnDay : PX.Data.IBqlField
		{
		}
		protected Int16? _MonthlyOnDay;
		[PXDBShort()]
		[PXUIField(DisplayName = "On Day", Visibility = PXUIVisibility.Visible)]
		[PXIntList(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 }, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" })]
		[PXDefault((short)1)]
		public virtual Int16? MonthlyOnDay
		{
			get
			{
				return this._MonthlyOnDay;
			}
			set
			{
				this._MonthlyOnDay = value;
			}
		}
		#endregion
		#region MonthlyOnWeek
		public abstract class monthlyOnWeek : PX.Data.IBqlField
		{
		}
		protected Int16? _MonthlyOnWeek;
		[PXDBShort()]
		[PXUIField(DisplayName = "On the", Visibility = PXUIVisibility.Visible)]
		[PXIntList(new int[] { 1, 2, 3, 4, 5 }, new string[] { Messages.OnFirstWeekOfMonth,Messages.OnSecondWeekOfMonth, Messages.OnThirdWeekOfMonth,Messages.OnFourthWeekOfMonth,Messages.OnLastWeekOfMonth })]
		[PXDefault((short)1)]
		public virtual Int16? MonthlyOnWeek
		{
			get
			{
				return this._MonthlyOnWeek;
			}
			set
			{
				this._MonthlyOnWeek = value;
			}
		}
		#endregion
		#region MonthlyOnDayOfWeek
		public abstract class monthlyOnDayOfWeek : PX.Data.IBqlField
		{
		}
		protected Int16? _MonthlyOnDayOfWeek;
		[PXDBShort()]
		[PXDefault((short)1)]
		[PXUIField(DisplayName = "Day of Week", Visibility = PXUIVisibility.Visible)]
		[PXIntList(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new string[] {Messages.MonthlyOnSunday, Messages.MonthlyOnMonday, Messages.MonthlyOnTuesday, Messages.MonthlyOnWednesday, Messages.MonthlyOnThursday, Messages.MonthlyOnFriday, Messages.MonthlyOnSaturday, Messages.MonthlyOnWeekday, Messages.MonthlyOnWeekend })]
		public virtual Int16? MonthlyOnDayOfWeek
		{
			get
			{
				return this._MonthlyOnDayOfWeek;
			}
			set
			{
				this._MonthlyOnDayOfWeek = value;
			}
		}
		#endregion
		#region PeriodFrequency
		public abstract class periodFrequency : PX.Data.IBqlField
		{
		}
		protected Int16? _PeriodFrequency;
		[PXDBShort()]
		[PXUIField(DisplayName = "Every", Visibility = PXUIVisibility.Visible)]
		[PXDefault((short)1)]
		public virtual Int16? PeriodFrequency
		{
			get
			{
				return this._PeriodFrequency;
			}
			set
			{
				this._PeriodFrequency = value;
			}
		}
		#endregion
		#region PeriodDateSel
		public abstract class periodDateSel : PX.Data.IBqlField
		{
		}
		protected String _PeriodDateSel;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Date Based On", Visibility = PXUIVisibility.Visible)]
		[PXDefault(PeriodDateSelOption.PeriodStart)]
		[PeriodDateSelOption.List()]
		public virtual String PeriodDateSel
		{
			get
			{
				return this._PeriodDateSel;
			}
			set
			{
				this._PeriodDateSel = value;
			}
		}
		#endregion
		#region PeriodFixedDay
		public abstract class periodFixedDay : PX.Data.IBqlField
		{
		}
		protected Int16? _PeriodFixedDay;
		[PXDBShort()]
		[PXUIField(DisplayName = "Fixed Day of the Period", Visibility = PXUIVisibility.Visible)]
		[PXDefault((short)1)]
		public virtual Int16? PeriodFixedDay
		{
			get
			{
				return this._PeriodFixedDay;
			}
			set
			{
				this._PeriodFixedDay = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _StartDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible)]
		[PXDefault(typeof(AccessInfo.businessDate))]

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
		#region NoEndDate
		public abstract class noEndDate : PX.Data.IBqlField
		{
		}
		protected Boolean? _NoEndDate;
		[PXDBBool()]
		[PXUIField(DisplayName = "Never Expires", Visibility = PXUIVisibility.Visible)]
		[PXDefault((Boolean)true)]

		public virtual Boolean? NoEndDate
		{
			get
			{
				return this._NoEndDate;
			}
			set
			{
				this._NoEndDate = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _EndDate;
		[PXDBDate()]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expiration Date", Visibility = PXUIVisibility.Visible)]
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
		#region NoRunLimit
		public abstract class noRunLimit : PX.Data.IBqlField
		{
		}
		protected Boolean? _NoRunLimit;
		[PXDBBool()]
		[PXUIField(DisplayName = "No Limit", Visibility = PXUIVisibility.Visible)]
		[PXDefault((Boolean)false)]
		public virtual Boolean? NoRunLimit
		{
			get
			{
				return this._NoRunLimit;
			}
			set
			{
				this._NoRunLimit = value;
			}
		}
		#endregion
		#region RunLimit
		public abstract class runLimit : PX.Data.IBqlField
		{
		}
		protected Int16? _RunLimit;
		[PXDBShort()]
		[PXUIField(DisplayName = "Execution Limit (times)", Visibility = PXUIVisibility.Visible)]
		[PXDefault((short)1)]
		public virtual Int16? RunLimit
		{
			get
			{
				return this._RunLimit;
			}
			set
			{
				this._RunLimit = value;
			}
		}
		#endregion
		#region RunCntr
		public abstract class runCntr : PX.Data.IBqlField
		{
		}
		protected Int16? _RunCntr;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Executed (times)", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Int16? RunCntr
		{
			get
			{
				return this._RunCntr;
			}
			set
			{
				this._RunCntr = value;
			}
		}
		#endregion
		#region NextRunDate
		public abstract class nextRunDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _NextRunDate;
		[PXDBDate()]
        [PXRequiredExpr(typeof(Where<Schedule.active, Equal<True>>))]
        [PXDefault()]
        [PXUIField(DisplayName = "Next Execution", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual DateTime? NextRunDate
		{
			get
			{
				return this._NextRunDate;
			}
			set
			{
				this._NextRunDate = value;
			}
		}
		#endregion
		#region LastRunDate
		public abstract class lastRunDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _LastRunDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Last Executed", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual DateTime? LastRunDate
		{
			get
			{
				return this._LastRunDate;
			}
			set
			{
				this._LastRunDate = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.IBqlField
		{
		}
		protected Int64? _NoteID;
		[PXNote(DescriptionField = typeof(Schedule.scheduleID))]
		public virtual Int64? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.IBqlField
		{
		}
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.IBqlField
		{
		}
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.IBqlField
		{
		}
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.IBqlField
		{
		}
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.IBqlField
		{
		}
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#region Module
		public abstract class module : PX.Data.IBqlField
		{
		}
		protected String _Module;
		[PXDBString(2, IsFixed = true)]
		[PXDefault(BatchModule.GL)]
		public virtual String Module
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
        #region Days
        public abstract class days : PX.Data.IBqlField
        {
        }
        protected String _Days;
        [PXString]
        [PXUIField]
        [PXDefault("Day(s)", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string Days
        {
            get
            {
                return this._Days;
            }
            set
            {
                this._Days = value;
            }
        }
        #endregion
        #region Weeks
        public abstract class weeks : PX.Data.IBqlField
        {
        }
        protected String _Weeks;
        [PXString]
        [PXUIField]
        [PXDefault("Week(s)", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string Weeks
        {
            get
            {
                return this._Weeks;
            }
            set
            {
                this._Weeks = value;
            }
        }
        #endregion
        #region Months
        public abstract class months : PX.Data.IBqlField
        {
        }
        protected String _Months;
        [PXString]
        [PXUIField]
        [PXDefault("Month(s)", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string Months
        {
            get
            {
                return this._Months;
            }
            set
            {
                this._Months = value;
            }
        }
        #endregion
        #region Periods
        public abstract class periods : PX.Data.IBqlField
        {
        }
        protected String _Periods;
        [PXString]
        [PXUIField]
        [PXDefault("Period(s)", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string Periods
        {
            get
            {
                return this._Periods;
            }
            set
            {
                this._Periods = value;
            }
        }
        #endregion
	}

	public class GLScheduleType
	{
		public class CustomListAttribute : PXStringListAttribute
		{
			public string[] AllowedValues
			{
				get
				{
					return _AllowedValues;
				}
			}

			public string[] AllowedLabels
			{
				get
				{
					return _AllowedLabels;
				}
			}

			public CustomListAttribute(string[] AllowedValues, string[] AllowedLabels)
				:base(AllowedValues, AllowedLabels)
			{ 
			}
		}

		public class ListAttribute : CustomListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Daily, Weekly, Monthly, Periodically },
				new string[] { Messages.Daily, Messages.Weekly, Messages.Monthly, Messages.Periodically }) { }

		}

		/// <summary>
		/// List of available periods. Daily, Weekly, Monthly or Periodically.
		/// </summary>
		public class FullListAttribute : CustomListAttribute
		{
			public FullListAttribute()
				: base(
				new string[] { Daily, Weekly, Monthly, Periodically },
				new string[] { Messages.Daily, Messages.Weekly, Messages.Monthly, Messages.Periodically }) { }
		}

		public const string Daily = "D";
		public const string Weekly = "W";
		public const string Monthly = "M";
		public const string Periodically = "P";
		public const string DeferredRevenue = "R";
		public const string DeferredExpense = "E";

		public class deferredRevenue : Constant<string>
		{
			public deferredRevenue() : base(DeferredRevenue) { ;}
		}

		public class deferredExpense : Constant<string>
		{
			public deferredExpense() : base(DeferredExpense) { ;}
		}
	}

	public static class PeriodDateSelOption 
	{

		public const string PeriodStart = "S";
		public const string PeriodEnd = "E";
		public const string PeriodFixedDate = "D";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { PeriodStart, PeriodEnd, PeriodFixedDate},
				new string[] { Messages.PeriodStartDate, Messages.PeriodEndDate, Messages.PeriodFixedDate}) { }

		}
	} 

}
