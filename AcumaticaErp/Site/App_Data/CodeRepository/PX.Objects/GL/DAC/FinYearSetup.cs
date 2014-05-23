namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
    [PXPrimaryGraph(typeof(FiscalYearSetupMaint))]
    [PXCacheName(Messages.FiscalYearSetupMaint)]
	public partial class FinYearSetup : PX.Data.IBqlTable, IYearSetup
	{
		#region FirstFinYear
		public abstract class firstFinYear : PX.Data.IBqlField
		{
		}
		protected String _FirstFinYear;
		[PXDBString(4, IsFixed = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "First Financial Year",Enabled = false)]
		public virtual String FirstFinYear
		{
			get
			{
				return this._FirstFinYear;
			}
			set
			{
				this._FirstFinYear = value;
			}
		}
		#endregion
		#region BegFinYear
		public abstract class begFinYear : PX.Data.IBqlField
		{
		}
		protected DateTime? _BegFinYear;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Financial Year Starts On")]
		public virtual DateTime? BegFinYear
		{
			get
			{
				return this._BegFinYear;
			}
			set
			{
				this._BegFinYear = value;
			}
		}
		#endregion
		#region FinPeriods
		public abstract class finPeriods : PX.Data.IBqlField
		{
		}
		protected Int16? _FinPeriods;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Number of Financial Periods ")]
		public virtual Int16? FinPeriods
		{
			get
			{
				return this._FinPeriods;
			}
			set
			{
				this._FinPeriods = value;
			}
		}
		#endregion
		#region PeriodLength
		public abstract class periodLength : PX.Data.IBqlField
		{
		}
		protected Int16? _PeriodLength;
		[PXDBShort(MinValue=5, MaxValue=366)]
		[PXDefault(PersistingCheck =PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Length of Financial Period (days)",Visible= true, Enabled = false)]
		public virtual Int16? PeriodLength
		{
			get
			{
				return this._PeriodLength;
			}
			set
			{
				this._PeriodLength = value;
			}
		}
		#endregion
		#region PeriodType
		public abstract class periodType0 : PX.Data.IBqlField
		{
		}
		protected string _PeriodType;
		[PXDBString(2,IsFixed = true)]
		[PXDefault(FinPeriodType.Month)]
		[PXUIField(DisplayName = "Period Type")]
		[FinPeriodType.List()]
		public virtual string PeriodType
		{
			get
			{
				return this._PeriodType;
			}
			set
			{
				this._PeriodType = value;
			}
		}
		#endregion
		#region UserDefined
		public abstract class userDefined : PX.Data.IBqlField
		{
		}
		protected Boolean? _UserDefined;
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "User-Defined Periods")]
		public virtual Boolean? UserDefined
		{
			get
			{
				return (this.PeriodType == FinPeriodType.CustomPeriodsNumber);
			}
			set
			{
				//this._UserDefined = value;
			}
		}
		#endregion
		#region PeriodsStartDate
		public abstract class periodsStartDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _PeriodsStartDate;
		[PXDBDate()]
		[PXDefault(typeof(FinYearSetup.begFinYear))]
		[PXUIField(DisplayName = "First Period Start Date", Visible = true)]
		public virtual DateTime? PeriodsStartDate
		{
			get
			{
				return this._PeriodsStartDate;
			}
			set
			{
				this._PeriodsStartDate = value;
			}
		}
		#endregion
		#region HasAdjustmentPeriod
		public abstract class hasAdjustmentPeriod : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Has Adjustment Period")]
		public bool? HasAdjustmentPeriod
		{
			get { return this._HasAdjustmentPeriod; }
			set { if (value.HasValue) this._HasAdjustmentPeriod = value.Value; }
		}
		protected bool _HasAdjustmentPeriod = false;
		#endregion 
		#region EndYearCalcMethod
		public abstract class endYearCalcMethod : PX.Data.IBqlField
		{
		}
		protected string _EndYearCalcMethod;
		[PXDBString(2, IsFixed = true)]
		[PXDefault(EndYearMethod.Calendar)]
		[PXUIField(DisplayName = "Year End Calculation Method")]
		[EndYearMethod.List()]
		public virtual string EndYearCalcMethod
		{
			get
			{
				return this._EndYearCalcMethod;
			}
			set
			{
				this._EndYearCalcMethod = value;
			}
		}
		#endregion
		#region EndYearDayOfWeek
		public abstract class endYearDayOfWeek : PX.Data.IBqlField
		{
		}
		protected int? _EndYearDayOfWeek;
		[PXDBInt()]
		[PXDefault(7)]
		[PXUIField(DisplayName = "Periods Start Day of Week", Enabled = true)]
		[PXIntList(new int[] { 1, 2, 3, 4, 5, 6, 7 }, new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" })]
		public virtual int? EndYearDayOfWeek
		{
			get
			{
				return this._EndYearDayOfWeek;
			}
			set
			{
				this._EndYearDayOfWeek = value;
			}
		}
		#endregion
		#region YearLastDayOfWeek
		public abstract class yearLastDayOfWeek : PX.Data.IBqlField
		{
		}
		protected int? _YearLastDayOfWeek;
		[PXInt()]
		[PXUIField(DisplayName = "Day of Week", Enabled = false)]
		[PXIntList(new int[] { 1, 2, 3, 4, 5, 6, 7 }, new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" })]
		public virtual int? YearLastDayOfWeek
		{
			get
			{
				return _EndYearDayOfWeek - 1 > 0 ? _EndYearDayOfWeek - 1 : _EndYearDayOfWeek - 1 + 7;
			}
			set
			{
				this._YearLastDayOfWeek = value;
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
		//attention - during regeneration they are erased
		#region AdditionalFields 
		
		#region AdjustToPeriodStart
		public abstract class adjustToPeriodStart : IBqlField { };
		[PXUIField(DisplayName ="Adjust To Period Start")]
		[PXBool]
		public bool? AdjustToPeriodStart
		{
			get { return _AdjustToPeriodStart; }
			set { if (value.HasValue) this._AdjustToPeriodStart = value.Value; }
		}
		protected bool _AdjustToPeriodStart = false;
		#endregion

		#region BelongsToNextYear
		public abstract class belongsToNextYear:IBqlField { };
		[PXUIField(DisplayName ="Belongs To Next Year")]
		[PXBool()]
		public bool? BelongsToNextYear
		{
			get { return _BelongsToNextYear; }
			set { if (value.HasValue) this._BelongsToNextYear = value.Value; }
		}
		protected bool? _BelongsToNextYear=null;
		#endregion

		#endregion

		#region Methods
		public bool IsFixedLengthPeriod
		{
			get
			{
				return FiscalPeriodSetupCreator.IsFixedLengthPeriod(this.FPType);
			}
		}
		public FiscalPeriodSetupCreator.FPType FPType
		{
			get
			{
				return FinPeriodType.GetFPType(this.PeriodType);
			}
		} 
		#endregion
	}

	public class FinPeriodType
	{
		public const string Month = "MO";
		public const string BiMonth = "BM";
		public const string Quarter = "QR";
		public const string Week = "WK";
		public const string BiWeek = "BW";
		public const string FourWeek = "FW";
		public const string Decade = "DC";
		public const string FourFourFive = "FF";
		public const string FourFiveFour = "FI";
		public const string FiveFourFour = "IF";
		public const string CustomPeriodLength = "CL";
		public const string CustomPeriodsNumber = "CN";
		
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Month , BiMonth, Quarter, Week, BiWeek, FourWeek, FourFourFive, FourFiveFour, FiveFourFour, CustomPeriodsNumber},
				new string[] { Messages.PT_Month, Messages.PT_BiMonth, Messages.PT_Quarter, Messages.PT_Week, Messages.PT_BiWeek, Messages.PT_FourWeeks, Messages.PT_FourFourFive, Messages.PT_FourFiveFour, Messages.PT_FiveFourFour, Messages.PT_CustomPeriodsNumber}
				) { }
		}

		public static FiscalPeriodSetupCreator.FPType GetFPType(string aFinPeriodType) 
		{			
			switch (aFinPeriodType) 
			{
				case Month : return FiscalPeriodSetupCreator.FPType.Month; 
				case BiMonth: return FiscalPeriodSetupCreator.FPType.BeMonth;
				case Quarter: return FiscalPeriodSetupCreator.FPType.Quarter;
				case Week: return FiscalPeriodSetupCreator.FPType.Week;
				case BiWeek: return FiscalPeriodSetupCreator.FPType.BeWeek;
				case FourWeek: return FiscalPeriodSetupCreator.FPType.FourWeek;
				case Decade: return FiscalPeriodSetupCreator.FPType.Decade;
				case FourFourFive: return FiscalPeriodSetupCreator.FPType.FourFourFive;
				case FourFiveFour: return FiscalPeriodSetupCreator.FPType.FourFiveFour;
				case FiveFourFour: return FiscalPeriodSetupCreator.FPType.FiveFourFour;
				case CustomPeriodsNumber: return FiscalPeriodSetupCreator.FPType.Custom;					
				default: 
					throw new PXException("Unknown Fin Period type");
			}
		}
	}
	public class EndYearMethod
	{
		public const string Calendar = "CA";
		public const string LastDay = "LD";
		public const string NearestDay = "ND";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Calendar, LastDay, NearestDay },
				new string[] { Messages.EndYearCalculation_Default, Messages.EndYearCalculation_LastDay, Messages.EndYearCalculation_ClosestDay }
				) { }
		}
	}
}
