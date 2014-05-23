namespace PX.Objects.TX
{
	using System;
	using PX.Data;

	public class TaxPeriodStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Prepared, Open, Closed },
				new string[] { Messages.Prepared, Messages.Open, Messages.Closed }) { ; }
		}

		public const string Prepared = "P";
		public const string Open = "N";
		public const string Closed = "C";
		public const string Dummy = "D";

		public class prepared : Constant<string>
		{
			public prepared() : base(Prepared) { ;}
		}

		public class open : Constant<string>
		{
			public open() : base(Open) { ;}
		}

		public class closed : Constant<string>
		{
			public closed() : base(Closed) { ;}
		}
	}

	[System.SerializableAttribute()]
	public partial class TaxPeriod : PX.Data.IBqlTable
	{
		#region VendorID
		public abstract class vendorID : PX.Data.IBqlField
		{
		}
		protected Int32? _VendorID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region TaxPeriodID
		public abstract class taxPeriodID : PX.Data.IBqlField
		{
		}
		protected String _TaxPeriodID;
		[GL.FinPeriodID(IsKey=true)]
		[PXDefault()]
		[PXUIField(DisplayName="Tax Period", Visibility=PXUIVisibility.SelectorVisible)]
		public virtual String TaxPeriodID
		{
			get
			{
				return this._TaxPeriodID;
			}
			set
			{
				this._TaxPeriodID = value;
			}
		}
		#endregion
		#region TaxYear
		public abstract class taxYear : PX.Data.IBqlField
		{
		}
		protected String _TaxYear;
		[PXDBString(4, IsFixed = true)]
		[PXDefault()]
		public virtual String TaxYear
		{
			get
			{
				return this._TaxYear;
			}
			set
			{
				this._TaxYear = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _StartDate;
		[PXDBDate()]
		[PXDefault()]
		[PXUIField(DisplayName = "From", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region EndDate
		public abstract class endDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _EndDate;
		[PXDBDate()]
		[PXDefault()]
		[PXUIField(DisplayName = "To", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region Status
		public abstract class status : PX.Data.IBqlField
		{
		}
		protected String _Status;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TaxPeriodStatus.Open)]
		[TaxPeriodStatus.List()]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		#endregion		
		#region Filed
		public abstract class filed : PX.Data.IBqlField
		{
		}
		protected Boolean? _Filed;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Filed
		{
			get
			{
				return this._Filed;
			}
			set
			{
				this._Filed = value;
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
	}
}
