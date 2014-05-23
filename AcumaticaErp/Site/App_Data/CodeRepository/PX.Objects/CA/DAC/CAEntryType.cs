namespace PX.Objects.CA
{
	using System;
	using PX.Data;
	using PX.Objects.GL;
	using PX.Objects.CM;
	using PX.Objects.CS;
    using PX.Objects.CR;
	using PX.Objects.AP;
	using PX.Objects.AR;

	public class CADrCr
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { CADebit, CACredit },
				new string[] { Messages.CADebit, Messages.CACredit }) { }
		}

		public const string CADebit = "D";
		public const string CACredit = "C";

		public class cADebit : Constant<string>
		{
			public cADebit() : base(CADebit) { ;}
		}

		public class cACredit : Constant<string>
		{
			public cACredit() : base(CACredit) { ;}
		}

		public static decimal DebitAmt(string DrCr, decimal CuryTranAmt)
		{
			switch (DrCr)
			{
				case "C":
					return (decimal)0.0;
				case "D":
					return CuryTranAmt;
				default:
					return (decimal)0.0;

			}
		}

		public static decimal CreditAmt(string DrCr, decimal CuryTranAmt)
		{
			switch (DrCr)
			{
				case "C":
					return CuryTranAmt;
				case "D":
					return (decimal)0.0;
				default:
					return (decimal)0.0;
			}
		}
	}

	public class CATranType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { CATransferOut, CATransferIn, CATransferExp, CAAdjustment },
				new string[] { Messages.CATransferOut, Messages.CATransferIn, Messages.CATransferExp, Messages.CAAdjustment }) { }
		}

        /// <summary>
        /// Selector. Defines a list of possible CADeposit types - namely <br/>
        /// CADeposit and CAVoidDeposit <br/>
        /// <example>
        /// [CATranType.DepositList()]
        /// </example>
        /// </summary>
		public class DepositListAttribute: PXStringListAttribute
		{
			public DepositListAttribute()
				:base(
					new string[]{CADeposit,CAVoidDeposit},
					new string[] { Messages.CADeposit, Messages.CAVoidDeposit }					
				){}
		}

		public const string CATransferOut = "CTO";
		public const string CATransferIn = "CTI";
		public const string CATransferExp = "CTE";
		public const string CATransferRGOL = "CTG";
		public const string CAAdjustment = "CAE";
		public const string CAAdjustmentRGOL = "CAG";
		public const string CADeposit = "CDT";
		public const string CAVoidDeposit = "CVD";

		

		public class cATransferOut : Constant<string>
		{
			public cATransferOut() : base(CATransferOut) { ;}
		}

		public class cATransferIn : Constant<string>
		{
			public cATransferIn() : base(CATransferIn) { ;}
		}

		public class cATransferExp : Constant<string>
		{
			public cATransferExp() : base(CATransferExp) { ;}
		}

		public class cAAdjustment : Constant<string>
		{
			public cAAdjustment() : base(CAAdjustment) { ;}
		}

		public class cADeposit : Constant<string>
		{
			public cADeposit() : base(CADeposit) { ;}
		}

		public class cAVoidDeposit : Constant<string>
		{
			public cAVoidDeposit() : base(CAVoidDeposit) { ;}
		}
	}

    public class CashAccountScalarAttribute : CashAccountAttribute
    {
        public CashAccountScalarAttribute(): base(){}
        public CashAccountScalarAttribute(Type branchID, Type SearchType): base(branchID,SearchType){}
        public override void GetSubscriber<ISubscriber>(System.Collections.Generic.List<ISubscriber> subscribers)
        {
            if (typeof(ISubscriber) != typeof(IPXCommandPreparingSubscriber) && typeof(ISubscriber) != typeof(IPXRowSelectingSubscriber))
            {
                base.GetSubscriber<ISubscriber>(subscribers);
            }
        }
    }
	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(EntryTypeMaint))]
	public partial class CAEntryType : PX.Data.IBqlTable
	{
        
		#region EntryTypeId
		public abstract class entryTypeId : PX.Data.IBqlField
		{
		}
		protected String _EntryTypeId;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Entry Type ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String EntryTypeId
		{
			get
			{
				return this._EntryTypeId;
			}
			set
			{
				this._EntryTypeId = value;
			}
		}
		#endregion
		#region OrigModule
		public abstract class module : PX.Data.IBqlField
		{
		}
		protected String _Module;
		[PXDBString(2, IsFixed = true)]
		[PXDefault(BatchModule.CA)]
		[BatchModule.CashManagerList()]
		[PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;
			}
		}
		#endregion
        #region CashAccountID
        public abstract class cashAccountID : PX.Data.IBqlField
        {
        }
        protected Int32? _CashAccountID;
        
        [CashAccountScalar(DisplayName = "Reclassification Account", Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(CashAccount.descr))]
        [PXDBScalar(typeof(Search<CashAccount.cashAccountID, Where<CashAccount.accountID, Equal<CAEntryType.accountID>,
                                   And<CashAccount.subID, Equal<CAEntryType.subID>, And<CashAccount.branchID, Equal<CAEntryType.branchID>>>>>))]
        public virtual Int32? CashAccountID
        {
            get
            {
                return this._CashAccountID;
            }
            set
            {
                this._CashAccountID = value;
            }
        }
        #endregion
		#region AccountID
		public abstract class accountID : PX.Data.IBqlField
		{
		}
		protected Int32? _AccountID;
		[Account(DescriptionField = typeof(Account.description), DisplayName = "Default Offset Account")]
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
		#region SubID
		public abstract class subID : PX.Data.IBqlField
		{
		}
		protected Int32? _SubID;
		[SubAccount(typeof(CAEntryType.accountID), DisplayName = "Default Offset Subaccount")]
		public virtual Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
        #region BranchID
        public abstract class branchID : PX.Data.IBqlField
        {
        }
        protected Int32? _BranchID;
        [PXDBInt()]
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
		#region ReferenceID
			public abstract class referenceID : PX.Data.IBqlField
			{
			}
			protected Int32? _ReferenceID;
			[PXDBInt()]
			[PXUIField(DisplayName = "Business Account")]
			[PXVendorCustomerSelector(typeof(CAEntryType.module))]
			public virtual Int32? ReferenceID
			{
				get
				{
					return this._ReferenceID;
				}
				set
				{
					this._ReferenceID = value;
				}
			}
		#endregion
		#region DrCr
			public abstract class drCr : PX.Data.IBqlField
			{
			}
			protected String _DrCr;
			[PXDefault("D")]
			[PXDBString(1, IsFixed = true)]
			[CADrCr.List()]
			[PXUIField(DisplayName = "Disb./Receipt", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String DrCr
			{
				get
				{
					return this._DrCr;
				}
				set
				{
					this._DrCr = value;
				}
			}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected String _Descr;
		[PXDBString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Entry Type Description", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region UseToReclassifyPayments
		public abstract class useToReclassifyPayments : PX.Data.IBqlField
		{
		}
		protected Boolean? _UseToReclassifyPayments;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use for Payments Reclassification", Visibility=PXUIVisibility.SelectorVisible)]
		public virtual Boolean? UseToReclassifyPayments
		{
			get
			{
				return this._UseToReclassifyPayments;
			}
			set
			{
				this._UseToReclassifyPayments = value;
			}
		}
		#endregion		
        #region Consolidate
        public abstract class consolidate : PX.Data.IBqlField
        {
        }
        protected Boolean? _Consolidate;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Deduct from Payment")]
        public Boolean? Consolidate
        {
            get
            {
                return _Consolidate;
            }
            set
            {
                _Consolidate = value;
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
