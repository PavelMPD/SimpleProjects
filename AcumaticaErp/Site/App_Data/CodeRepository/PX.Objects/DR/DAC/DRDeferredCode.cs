namespace PX.Objects.DR
{
	using System;
	using PX.Data;
	using PX.Objects.GL;
	
	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(DeferredCodeMaint))]
	[PXCacheName(Messages.DeferredCode)]
	public partial class DRDeferredCode : PX.Data.IBqlTable
	{
		public const string ScheduleOptionFixedDate = "D";
		public const string ScheduleOptionStart = "S";
		public const string ScheduleOptionEnd = "E";

		#region DeferredCodeID
		public abstract class deferredCodeID : PX.Data.IBqlField
		{
		}
		protected String _DeferredCodeID;
		[PXDefault()]
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Deferral Code", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<DRDeferredCode.deferredCodeID>))]
		[PX.Data.EP.PXFieldDescription]
		public virtual String DeferredCodeID
		{
			get
			{
				return this._DeferredCodeID;
			}
			set
			{
				this._DeferredCodeID = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.IBqlField
		{
		}
		protected String _Description;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PX.Data.EP.PXFieldDescription]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region AccountType
		public abstract class accountType : PX.Data.IBqlField
		{
		}
		protected string _AccountType;
		[PXDBString(1)]
		[PXDefault(DeferredAccountType.Income)]
		[DeferredAccountType.List()]
		[PXUIField(DisplayName = "Code Type", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string AccountType
		{
			get
			{
				return this._AccountType;
			}
			set
			{
				this._AccountType = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.IBqlField
		{
		}
		protected Int32? _AccountID;
		[Account(DescriptionField = typeof(Account.description), Visibility = PXUIVisibility.SelectorVisible, DisplayName="Deferral Account")]
		[PXDefault()]
		public virtual Int32? AccountID
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
		public abstract class subID : PX.Data.IBqlField
		{
		}
		protected Int32? _SubID;
		[SubAccount(typeof(DRDeferredCode.accountID), DescriptionField = typeof(Sub.description), Visibility = PXUIVisibility.SelectorVisible, DisplayName="Deferral Sub.")]
		[PXDefault()]
		public virtual Int32? SubID
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
		#region ReconNowPct
		public abstract class reconNowPct : PX.Data.IBqlField
		{
		}
		protected Decimal? _ReconNowPct;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
		[PXUIField(DisplayName = "Recognize Now %", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? ReconNowPct
		{
			get
			{
				return this._ReconNowPct;
			}
			set
			{
				this._ReconNowPct = value;
			}
		}
		#endregion
		#region StartOffset
		public abstract class startOffset : PX.Data.IBqlField
		{
		}
		protected Int16? _StartOffset;
		[PXDefault((short)0)]
		[PXDBShort]
		[PXUIField(DisplayName = "Start Offset")]
		public virtual Int16? StartOffset
		{
			get
			{
				return this._StartOffset;
			}
			set
			{
				this._StartOffset = value;
			}
		}
		#endregion
		#region Occurrences
		public abstract class occurrences : PX.Data.IBqlField
		{
		}
		protected Int16? _Occurrences;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Occurrences")]
		public virtual Int16? Occurrences
		{
			get
			{
				return this._Occurrences;
			}
			set
			{
				this._Occurrences = value;
			}
		}
		#endregion
		#region Frequency
		public abstract class frequency : PX.Data.IBqlField
		{
		}
		protected Int16? _Frequency;
		[PXDBShort()]
		[PXUIField(DisplayName = "Every", Visibility = PXUIVisibility.Visible)]
		[PXDefault((short)1)]
		public virtual Int16? Frequency
		{
			get
			{
				return this._Frequency;
			}
			set
			{
				this._Frequency = value;
			}
		}
		#endregion
		#region ScheduleOption
		public abstract class scheduleOption : PX.Data.IBqlField
		{
		}
		protected String _ScheduleOption;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Schedule Options", Visibility = PXUIVisibility.Visible)]
		[PXDefault("S")]
		[PXStringList(new string[] { "S", "E", "D" }, new string[] { "Start of Financial Period", "End of Financial Period", "Fixed Day of the Period" })]
		public virtual String ScheduleOption
		{
			get
			{
				return this._ScheduleOption;
			}
			set
			{
				this._ScheduleOption = value;
			}
		}
		#endregion
		#region FixedDay
		public abstract class fixedDay : PX.Data.IBqlField
		{
		}
		protected Int16? _FixedDay;
		[PXDBShort(MinValue = 1)]
		[PXUIField(DisplayName = "Fixed Day of the Period", Visibility = PXUIVisibility.Visible)]
		[PXDefault((short)1)]
		public virtual Int16? FixedDay
		{
			get
			{
				return this._FixedDay;
			}
			set
			{
				this._FixedDay = value;
			}
		}
		#endregion
		#region Method
		public abstract class method : PX.Data.IBqlField
		{
		}
		protected String _Method;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Recognition Method", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(DeferredMethodType.EvenPeriods)]
		[DeferredMethodType.List]
		public virtual String Method
		{
			get
			{
				return this._Method;
			}
			set
			{
				this._Method = value;
			}
		}
		#endregion

		#region NoteID

		public abstract class noteID : IBqlField { }

		[PXNote(DescriptionField = typeof(DRDeferredCode.deferredCodeID))]
		public virtual Int64? NoteID { get; set; }

		#endregion
		

		#region System Columns
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
		#endregion
	}

	public static class DeferredAccountType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Income, Expense },
				new string[] { Messages.Income, Messages.Expense }) { }
		}

		public const string Income = GL.AccountType.Income;
		public const string Expense = GL.AccountType.Expense;

		public class income : Constant<string>
		{
			public income() : base(Income) { ;}
		}

		public class expense : Constant<string>
		{
			public expense() : base(Expense) { ;}
		}

	}

	public static class DeferredMethodType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { EvenPeriods, ProrateDays, ExactDays, CashReceipt },
				new string[] { Messages.EvenPeriods, Messages.ProrateDays, Messages.ExactDays, Messages.CashReceipt }) { ; }
		}
		public const string EvenPeriods = "E";
		public const string ProrateDays = "P";
		public const string ExactDays = "D";
		public const string CashReceipt = "C";


		public class EvenPeriodMethod : Constant<string>
		{
			public EvenPeriodMethod() : base(DeferredMethodType.EvenPeriods) { ;}
		}
		public class ProrateDaysMethod : Constant<string>
		{
			public ProrateDaysMethod() : base(DeferredMethodType.ProrateDays) { ;}
		}
		public class ExactDaysMethod : Constant<string>
		{
			public ExactDaysMethod() : base(DeferredMethodType.ExactDays) { ;}
		}

		 public class cashReceipt : Constant<string>
		 {
			 public cashReceipt() : base(CashReceipt) { ;}
		 }

	}
}
