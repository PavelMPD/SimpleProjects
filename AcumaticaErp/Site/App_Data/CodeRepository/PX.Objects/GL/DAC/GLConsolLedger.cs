namespace PX.Objects.GL
{
	using System;
	using PX.Data;

	[System.SerializableAttribute()]
	public partial class GLConsolLedger : PX.Data.IBqlTable
	{
		#region SetupID
		public abstract class setupID : PX.Data.IBqlField
		{
		}
		protected Int32? _SetupID;
		[PXDBInt(IsKey = true)]
		public virtual Int32? SetupID
		{
			get
			{
				return this._SetupID;
			}
			set
			{
				this._SetupID = value;
			}
		}
		#endregion
		#region LedgerCD
		public abstract class ledgerCD : PX.Data.IBqlField
		{
		}
		protected String _LedgerCD;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Ledger", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		public virtual String LedgerCD
		{
			get
			{
				return this._LedgerCD;
			}
			set
			{
				this._LedgerCD = value;
			}
		}
		#endregion
		#region PostInterCompany
		public abstract class postInterCompany : PX.Data.IBqlField
		{
		}
		protected Boolean? _PostInterCompany;
		[PXDBBool()]
		[PXDefault()]
		[PXUIField(DisplayName = "Generates Inter-Branch Transactions", Enabled = false)]
		public virtual Boolean? PostInterCompany
		{
			get
			{
				return this._PostInterCompany;
			}
			set
			{
				this._PostInterCompany = value;
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
	}
}
