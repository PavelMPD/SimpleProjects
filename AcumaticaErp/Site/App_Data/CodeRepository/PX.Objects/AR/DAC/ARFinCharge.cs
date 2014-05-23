using PX.Objects.CM;

namespace PX.Objects.AR
{
	using System;
	using PX.Data;
	using PX.Objects.CS;
	using PX.Objects.GL;
	using PX.Objects.TX;

	
	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(ARFinChargesMaint))]
	public partial class ARFinCharge : PX.Data.IBqlTable
	{
		#region FinChargeID
		public abstract class finChargeID : PX.Data.IBqlField
		{
		}
		protected String _FinChargeID;
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault()]
		[PXUIField(DisplayName = "Overdue Charge ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String FinChargeID
		{
			get
			{
				return this._FinChargeID;
			}
			set
			{
				this._FinChargeID = value;
			}
		}
		#endregion
		#region FinChargeDesc
		public abstract class finChargeDesc : PX.Data.IBqlField
		{
		}
		protected String _FinChargeDesc;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Overdue Charge Description")]
		public virtual String FinChargeDesc
		{
			get
			{
				return this._FinChargeDesc;
			}
			set
			{
				this._FinChargeDesc = value;
			}
		}
		#endregion
		#region TermsID
		public abstract class termsID : PX.Data.IBqlField
		{
		}
		protected String _TermsID;
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault()]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, Or<Terms.visibleTo, Equal<TermsVisibleTo.customer>>>>), DescriptionField = typeof(Terms.descr), Filterable = true)]
		public virtual String TermsID
		{
			get
			{
				return this._TermsID;
			}
			set
			{
				this._TermsID = value;
			}
		}
		#endregion
		#region BaseCurFlag
		public abstract class baseCurFlag : PX.Data.IBqlField
		{
		}
		protected Boolean? _BaseCurFlag;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Base Currency")]
		public virtual Boolean? BaseCurFlag
		{
			get
			{
				return this._BaseCurFlag;
			}
			set
			{
				this._BaseCurFlag = value;
			}
		}
		#endregion
		#region MinFinChargeFlag
		public abstract class minFinChargeFlag : PX.Data.IBqlField
		{
		}
		protected Boolean? _MinFinChargeFlag;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Min. Amount")]
		public virtual Boolean? MinFinChargeFlag
		{
			get
			{
				return this._MinFinChargeFlag;
			}
			set
			{
				this._MinFinChargeFlag = value;
			}
		}
		#endregion
		#region MinFinChargeAmount
		public abstract class minFinChargeAmount : PX.Data.IBqlField
		{
		}
		protected Decimal? _MinFinChargeAmount;
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Min. Charge Amount")]
		public virtual Decimal? MinFinChargeAmount
		{
			get
			{
				return this._MinFinChargeAmount;
			}
			set
			{
				this._MinFinChargeAmount = value;
			}
		}
		#endregion
		#region PercentFlag
		public abstract class percentFlag : PX.Data.IBqlField
		{
		}
		protected Boolean? _PercentFlag;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "By Percent")]
		public virtual Boolean? PercentFlag
		{
			get
			{
				return this._PercentFlag;
			}
			set
			{
				this._PercentFlag = value;
			}
		}
		#endregion
		#region FinChargePercent
		public abstract class finChargePercent : PX.Data.IBqlField
		{
		}
		protected Decimal? _FinChargePercent;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Charge Percent")]
		public virtual Decimal? FinChargePercent
		{
			get
			{
				return this._FinChargePercent;
			}
			set
			{
				this._FinChargePercent = value;
			}
		}
		#endregion
		#region FinChargeAcctID
		public abstract class finChargeAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _FinChargeAccountID;

		[PXDefault()]
		[Account(DisplayName = "Overdue Charges Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description),Required=true)]
		public virtual Int32? FinChargeAccountID
		{
			get
			{
				return this._FinChargeAccountID;
			}
			set
			{
				this._FinChargeAccountID = value;
			}
		}
		#endregion
		#region FinChargeSubID
		public abstract class finChargeSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _FinChargeSubID;

		[PXDefault()]
		[SubAccount(typeof(ARFinCharge.finChargeAccountID), DisplayName = "Overdue Charges Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description),Required=true)]
		public virtual Int32? FinChargeSubID
		{
			get
			{
				return this._FinChargeSubID;
			}
			set
			{
				this._FinChargeSubID = value;
			}
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.IBqlField
		{
		}
		protected String _TaxCategoryID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String TaxCategoryID
		{
			get
			{
				return this._TaxCategoryID;
			}
			set
			{
				this._TaxCategoryID = value;
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
