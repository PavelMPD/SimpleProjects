using System;
using PX.Data;
using PX.Data.EP;

namespace PX.Objects.EP
{
	[PXCacheName(Messages.EPPayGroup)]
	[Serializable]
	[PXPrimaryGraph(typeof(EPPayGroupMaint))]
	public partial class EPPayGroup : IBqlTable
	{
		#region PayGroupID
		public abstract class payGroupID : IBqlField
		{
		}
		protected string _PayGroupID;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Pay Group ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string PayGroupID
		{
			get
			{
				return _PayGroupID;
			}
			set
			{
				_PayGroupID = value;
			}
		}
		#endregion
		#region PayPeriod
		public abstract class payPeriod : IBqlField
		{
			#region List
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new[] { Biweekly, Weekly, Semimonthly, Monthly },
					new[] { Messages.Biweekly, Messages.Weekly, Messages.Semimonthly, Messages.Monthly }) { }
			}

			public const string Biweekly = "BW";
			public const string Weekly = "WE";
			public const string Semimonthly = "SM";
			public const string Monthly = "MO";

			public class biweekly : Constant<string>
			{
				public biweekly() : base(Biweekly) { }
			}
			public class weekly : Constant<string>
			{
				public weekly() : base(Weekly) { }
			}
			public class semimonthly : Constant<string>
			{
				public semimonthly() : base(Semimonthly) { }
			}
			public class monthly : Constant<string>
			{
				public monthly() : base(Monthly) { }
			}
			#endregion
		}
		protected string _PayPeriod;
		[PXDBString(2, IsFixed = true)]
		[PXDefault(payPeriod.Weekly)]
		[PXUIField(DisplayName = "Pay Frequency", Visibility = PXUIVisibility.SelectorVisible)]
		[payPeriod.List]
		public virtual string PayPeriod
		{
			get
			{
				return _PayPeriod;
			}
			set
			{
				_PayPeriod = value;
			}
		}
		#endregion
		#region HoursPerPayPeriod
		public abstract class hoursPerPayPeriod : IBqlField
		{
		}
		protected decimal? _HoursPerPayPeriod;
		[PXDBDecimal(2, MinValue = 0.01)]
		[PXDefault]
		[PXUIField(DisplayName = "Hours per Pay Period", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? HoursPerPayPeriod
		{
			get
			{
				return _HoursPerPayPeriod;
			}
			set
			{
				_HoursPerPayPeriod = value;
			}
		}
		#endregion
		#region WorkHoursPerDay

		public abstract class workHoursPerDay : IBqlField { }

		[PXDBInt]
		[PXUIField(DisplayName = "Work Hours per Day")]
		public virtual Int32? WorkHoursPerDay { get; set; }

		#endregion
		#region Description
		public abstract class description : IBqlField
		{
		}
		protected string _Description;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String Description
		{
			get
			{
				return _Description;
			}
			set
			{
				_Description = value;
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
	}
}
