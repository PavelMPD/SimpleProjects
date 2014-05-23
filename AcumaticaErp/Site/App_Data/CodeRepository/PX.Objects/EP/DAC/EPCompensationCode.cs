using System;
using PX.Data;
using PX.Data.EP;

namespace PX.Objects.EP
{
	[PXCacheName(Messages.EPCompensationCode)]
	[Serializable]
	[PXPrimaryGraph(typeof(EPCompCodeMaint))]
	public partial class EPCompensationCode : IBqlTable
	{
		#region CompensationCode
		public abstract class compensationCode : IBqlField
		{
		}
		protected string _CompensationCode;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Compensation Code", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string CompensationCode
		{
			get
			{
				return _CompensationCode;
			}
			set
			{
				_CompensationCode = value;
			}
		}
		#endregion
		#region Type
		public abstract class type : IBqlField
		{
			#region List
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new [] { RegularPortionOfAllPay, GrossPayBasis, RegularHoursBasis, RegularPayBasis, TotalHoursBasis },
					new [] { Messages.RegularPortionOfAllPay, Messages.GrossPayBasis, Messages.RegularHoursBasis, Messages.RegularPayBasis, Messages.TotalHoursBasis }) { }
			}

			public const string RegularPortionOfAllPay = "RA";
			public const string GrossPayBasis = "GP";
			public const string RegularHoursBasis = "RH";
			public const string RegularPayBasis = "RP";
			public const string TotalHoursBasis = "TH";

			public class regularPortionOfAllPay : Constant<string>
			{
				public regularPortionOfAllPay() : base(RegularPortionOfAllPay) {}
			}
			public class grossPayBasis : Constant<string>
			{
				public grossPayBasis() : base(GrossPayBasis) {}
			}
			public class regularHoursBasis : Constant<string>
			{
				public regularHoursBasis() : base(RegularHoursBasis) {}
			}
			public class regularPayBasis : Constant<string>
			{
				public regularPayBasis() : base(RegularPayBasis) {}
			}
			public class totalHoursBasis : Constant<string>
			{
				public totalHoursBasis() : base(TotalHoursBasis) { }
			}
			#endregion
		}
		protected string _Type;
		[PXDBString(2, IsFixed = true)]
		[PXDefault(type.RegularPortionOfAllPay)]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible)]
		[type.List]
		public virtual string Type
		{
			get
			{
				return _Type;
			}
			set
			{
				_Type = value;
			}
		}
		#endregion
		#region Rate
		public abstract class rate : IBqlField
		{
		}
		protected decimal? _Rate;
		[PXDBDecimal(2)]
		[PXUIField(DisplayName = "Rate", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? Rate
		{
			get
			{
				return _Rate;
			}
			set
			{
				_Rate = value;
			}
		}
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
