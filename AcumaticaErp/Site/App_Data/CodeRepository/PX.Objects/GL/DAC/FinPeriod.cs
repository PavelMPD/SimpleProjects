namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	[PXPrimaryGraph(
		new Type[] { typeof(FiscalPeriodMaint)},
		new Type[] { typeof(Select<FinYear, 
			Where<FinYear.year, Equal<Current<FinPeriod.finYear>>>>)
		})]
	public partial class FinPeriod : PX.Data.IBqlTable, IPeriod
	{
        #region Selected
        public abstract class selected : PX.Data.IBqlField
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
		#region FinPeriodID
		 public abstract class finPeriodID : PX.Data.IBqlField
		 {
		 }
		 protected String _FinPeriodID;
		 [FinPeriodID(IsKey = true)]
		 [PXDefault()]
		 [PXUIField(Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = false, DisplayName = "Financial Period ID")]
		 public virtual String FinPeriodID
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
		#region StartDate
		public abstract class startDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _StartDate;
		[PXDBDate()]
		[PXDefault()]
		[PXUIField(DisplayName = "Start Date", Enabled = false)]
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
		[PXUIField(DisplayName = "EndDate", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
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
		#region FinDate
		public abstract class finDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _FinDate;
		[PXDate()]
		[PXUIField(DisplayName = "FinDate", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		[PXDBCalced(typeof(Sub<FinPeriod.endDate, PX.Objects.CS.int1>), typeof(DateTime))]
		public virtual DateTime? FinDate
		{
			get
			{
				return this._FinDate;
			}
			set
			{
				this._FinDate = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected String _Descr;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description",Visibility=PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region Closed
		public abstract class closed : PX.Data.IBqlField
		{
		}
		protected Boolean? _Closed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Closed in GL", Enabled = false)]
		public virtual Boolean? Closed
		{
			get
			{
				return this._Closed;
			}
			set
			{
				this._Closed = value;
			}
		}
		#endregion
		#region APClosed
		public abstract class aPClosed : PX.Data.IBqlField
		{
		}
		protected Boolean? _APClosed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Closed in AP", Enabled = false)]
		public virtual Boolean? APClosed
		{
			get
			{
				return this._APClosed;
			}
			set
			{
				this._APClosed = value;
			}
		}
		#endregion
		#region ARClosed
		public abstract class aRClosed : PX.Data.IBqlField
		{
		}
		protected Boolean? _ARClosed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Closed in AR", Enabled = false)]
		public virtual Boolean? ARClosed
		{
			get
			{
				return this._ARClosed;
			}
			set
			{
				this._ARClosed = value;
			}
		}
		#endregion
		#region INClosed
		public abstract class iNClosed : PX.Data.IBqlField
		{
		}
		protected Boolean? _INClosed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Closed in IN", Enabled = false)]
		public virtual Boolean? INClosed
		{
			get
			{
				return this._INClosed;
			}
			set
			{
				this._INClosed = value;
			}
		}
		#endregion
		#region CAClosed
		public abstract class cAClosed : PX.Data.IBqlField
		{
		}
		protected Boolean? _CAClosed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Closed in CA", Enabled = false)]
		public virtual Boolean? CAClosed
		{
			get
			{
				return this._CAClosed;
			}
			set
			{
				this._CAClosed = value;
			}
		}
		#endregion
        #region FAClosed
        public abstract class fAClosed : PX.Data.IBqlField
        {
        }
        protected Boolean? _FAClosed;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Closed in FA", Enabled = false)]
        public virtual Boolean? FAClosed
        {
            get
            {
                return this._FAClosed;
            }
            set
            {
                this._FAClosed = value;
            }
        }
        #endregion
        #region DateLocked
		public abstract class dateLocked : PX.Data.IBqlField
		{
		}
		protected Boolean? _DateLocked;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Date Locked", Enabled = false, Visible = false)]
		public virtual Boolean? DateLocked
		{
			get
			{
				return this._DateLocked;
			}
			set
			{
				this._DateLocked = value;
			}
		}
		#endregion
		#region Active
		public abstract class active : PX.Data.IBqlField
		{
		}
		protected Boolean? _Active;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Active")]
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
		#region PeriodNbr
		public abstract class periodNbr : PX.Data.IBqlField
		{
		}
		protected String _PeriodNbr;
		[PXDBString(2, IsFixed = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "Period Nbr.", Enabled = false)]
		public virtual String PeriodNbr
		{
			get
			{
				return this._PeriodNbr;
			}
			set
			{
				this._PeriodNbr = value;
			}
		}
		#endregion
		#region FinYear
		public abstract class finYear : PX.Data.IBqlField
		{
		}
		protected String _FinYear;
		[PXDBString(4, IsFixed = true)]
		[PXDefault(typeof(FinYear.year))]
		[PXUIField(DisplayName = "FinYear", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		[PXParent(typeof(Select<FinYear, Where<FinYear.year, Equal<Current<FinPeriod.finYear>>>>))]
	  	public virtual String FinYear
		{
			get
			{
				return this._FinYear;
			}
			set
			{
				this._FinYear = value;
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
		#region Methods
		public static implicit operator TX.TaxPeriod(FinPeriod item)
		{
			TX.TaxPeriod ret = new TX.TaxPeriod();
			ret.TaxYear = item.FinYear;
			ret.TaxPeriodID = item.FinPeriodID;
			ret.StartDate = item.StartDate;
			ret.EndDate = item.EndDate;
			return ret;
		}
		#endregion

		#region EndDateUI
		public abstract class endDateUI : PX.Data.IBqlField
		{
		}
		
		[PXDate()]
		[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
		public virtual DateTime? EndDateUI
		{
			[PXDependsOnFields(typeof(endDate),typeof(startDate))]
			get
			{
				bool isEmpty = (this.EndDate.HasValue && this.EndDate.HasValue && this.StartDate == this.EndDate);
				return ((this._EndDate.HasValue && !isEmpty) ? this._EndDate.Value.AddDays(-1): this._EndDate);
			}
			set
			{
				bool isEmpty = (value.HasValue && this.StartDate.HasValue && this.StartDate == value);
				this._EndDate = (value.HasValue && !isEmpty) ? value.Value.AddDays(1): value;
			}
		}
		#endregion
		#region Custom
		public abstract class custom : PX.Data.IBqlField
		{
		}
		protected Boolean? _Custom;
		[PXDBBool]
		[PXDefault(false)]
		public virtual Boolean? Custom
		{
			get
			{
				return this._Custom;
			}
			set
			{
				this._Custom = value;
			}
		}
		#endregion
		#region Length
		public abstract class length : PX.Data.IBqlField
		{
		}
		protected Int32? _Length;
		[PXInt]
		[PXDefault(0)]
		[PXUIField(DisplayName="Length (Days)", Visible=true, Enabled=false)]
		public virtual Int32? Length
		{
			get
			{
				if (_StartDate != null && _EndDate != null && _StartDate != _EndDate)
				{
					return ((TimeSpan)(_EndDate - _StartDate)).Days;
				}
				else
					return 0;
			}
		}
		#endregion
		#region IsAdjustment
		public abstract class isAdjustment : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsAdjustment;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Adjustment Period", Visible = false, Enabled = false)]
		public virtual Boolean? IsAdjustment
		{
			get
			{
				if (_StartDate != null && _EndDate != null && _StartDate == _EndDate)
					return true;
				return false;
			}
			set
			{
				this._IsAdjustment = value;
			}
		}
		#endregion
	}
}
