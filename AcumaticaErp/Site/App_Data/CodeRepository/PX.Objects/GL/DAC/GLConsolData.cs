namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	public partial class GLConsolData : PX.Data.IBqlTable
	{
		#region AccountCD
		public abstract class accountCD : PX.Data.IBqlField
		{
		}
		protected String _AccountCD;
		[PXDBString(30, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		public virtual String AccountCD
		{
			get
			{
				return this._AccountCD;
			}
			set
			{
				this._AccountCD = value;
			}
		}
		#endregion
		#region MappedValue
		public abstract class mappedValue : PX.Data.IBqlField
		{
		}
		protected String _MappedValue;
		[PXDBString(30, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		public virtual String MappedValue
		{
			get
			{
				return this._MappedValue;
			}
			set
			{
				this._MappedValue = value;
			}
		}
		#endregion
		#region FinPeriod
		public abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		protected String _FinPeriodID;
		[GL.FinPeriodID(IsKey = true)]
		[PXDefault()]
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
		#region ConsolAmtCredit
		public abstract class consolAmtCredit : PX.Data.IBqlField
		{
		}
		protected Decimal? _ConsolAmtCredit;
		[PXDBDecimal(6)]
		public virtual Decimal? ConsolAmtCredit
		{
			get
			{
				return this._ConsolAmtCredit;
			}
			set
			{
				this._ConsolAmtCredit = value;
			}
		}
		#endregion
		#region ConsolAmtDebit
		public abstract class consolAmtDebit : PX.Data.IBqlField
		{
		}
		protected Decimal? _ConsolAmtDebit;
		[PXDBDecimal(6)]
		public virtual Decimal? ConsolAmtDebit
		{
			get
			{
				return this._ConsolAmtDebit;
			}
			set
			{
				this._ConsolAmtDebit = value;
			}
		}
		#endregion
	}
}
