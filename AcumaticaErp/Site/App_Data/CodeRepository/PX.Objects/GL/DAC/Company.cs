using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.GL
{
	[System.SerializableAttribute()]
    [PXPrimaryGraph(typeof(BranchMaint))]
	[ForceSaveInDash(false)]
	public partial class Company : PX.Data.IBqlTable
	{
		#region CompanyCD
		public abstract class companyCD : PX.Data.IBqlField
		{
		}
		protected String _CompanyCD;
		[PXDBString(30, IsUnicode = true)]
		[PXDBLiteDefault(typeof(BAccount.acctCD))]
		[PXDimension("BIZACCT")]
		[PXUIField(DisplayName = "Company", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String CompanyCD
		{
			get
			{
				return this._CompanyCD;
			}
			set
			{
				this._CompanyCD = value;
			}
		}
		#endregion
		#region BaseCuryID
		public abstract class baseCuryID : PX.Data.IBqlField
		{
		}
		protected String _BaseCuryID;
		[PXDBString(5, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Base Currency ID")]
		[PXSelector(typeof(Search<Currency.curyID>))]
		public virtual String BaseCuryID
		{
			get
			{
				return this._BaseCuryID;
			}
			set
			{
				this._BaseCuryID = value;
			}
		}
		#endregion
		/*
		#region BAccountID
		public abstract class bAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _BAccountID;
		[PXDBInt()]
		[PXUIField(Visible=true,Enabled=false)]
		public virtual Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		*/
		#region PhoneMask
		public abstract class phoneMask : PX.Data.IBqlField
		{
		}
		protected String _PhoneMask;
		[PXDBString(50)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Phone Mask")]
		public virtual String PhoneMask
		{
			get
			{
				return this._PhoneMask;
			}
			set
			{
				this._PhoneMask = value;
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
