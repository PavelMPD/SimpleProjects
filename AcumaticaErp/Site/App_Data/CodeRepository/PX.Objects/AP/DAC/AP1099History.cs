namespace PX.Objects.AP
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	public partial class AP1099History : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.IBqlField
		{
		}
		protected Int32? _BranchID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
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
		#region FinYear
		public abstract class finYear : PX.Data.IBqlField
		{
		}
		protected String _FinYear;
		[PXDBString(4, IsKey = true, IsFixed = true)]
		[PXDefault()]
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
		#region BoxNbr
		public abstract class boxNbr : PX.Data.IBqlField
		{
		}
		protected Int16? _BoxNbr;
		[PXDBShort(IsKey = true)]
		[PXDefault()]
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
		#region HistAmt
		public abstract class histAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _HistAmt;
		[CM.PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName="Amount", Visibility=PXUIVisibility.Visible, Enabled=false)]
		public virtual Decimal? HistAmt
		{
			get
			{
				return this._HistAmt;
			}
			set
			{
				this._HistAmt = value;
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
