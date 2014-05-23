namespace PX.Objects.AP
{
	using System;
	using PX.Data;
	using PX.Objects.GL;
	
	[System.SerializableAttribute()]
	public partial class AP1099Box : PX.Data.IBqlTable
	{
		#region BoxNbr
		public abstract class boxNbr : PX.Data.IBqlField
		{
		}
		protected Int16? _BoxNbr;
		[PXDBShort(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName="Box", Visibility=PXUIVisibility.Visible, Enabled=false)]
		public virtual Int16? BoxNbr
		{
			get
			{
				return this._BoxNbr;
			}
			set
			{
				this._BoxNbr = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected String _Descr;
		[PXDBString(60, IsUnicode=true)]
		[PXUIField(DisplayName="Description", Visibility=PXUIVisibility.Visible, Enabled=false)]
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
		#region MinReportAmt
		public abstract class minReportAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _MinReportAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName="Minimum Report Amount", Visibility=PXUIVisibility.Visible)]
		public virtual Decimal? MinReportAmt
		{
			get
			{
				return this._MinReportAmt;
			}
			set
			{
				this._MinReportAmt = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.IBqlField
		{
		}
		protected Int32? _AccountID;
		[UnboundAccount(DisplayName = "Account", Visibility = PXUIVisibility.Visible)]
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
		#region OldAccountID
		public abstract class oldAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _OldAccountID;
		[PXInt()]
		public virtual Int32? OldAccountID
		{
			get
			{
				return this._OldAccountID;
			}
			set
			{
				this._OldAccountID = value;
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
