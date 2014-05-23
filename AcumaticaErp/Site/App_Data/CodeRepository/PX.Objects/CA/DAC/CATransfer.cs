using PX.Data.EP;

namespace PX.Objects.CA
{
	using System;
	using PX.Data;
	using PX.Objects.CM;
	using PX.Objects.CS;
	using PX.Objects.AR;
	using PX.Objects.GL;

	[PXCacheName(Messages.CATransfer)]
	[System.SerializableAttribute()]
	public partial class CATransfer : PX.Data.IBqlTable, ICADocument
	{
		public string DocType
		{
			get
			{
				return CAAPARTranType.CATransfer;
			}
		}
		public string RefNbr
		{
			get
			{
				return this._TransferNbr;
			}
		}
		#region TransferNbr
		public abstract class transferNbr : PX.Data.IBqlField
		{
		}
		protected String _TransferNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Transfer Number", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(CATransfer.transferNbr))]
        [AutoNumber(typeof(CASetup.transferNumberingID), typeof(CATransfer.inDate))]
		public virtual String TransferNbr
		{
			get
			{
				return this._TransferNbr;
			}
			set
			{
				this._TransferNbr = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected String _Descr;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
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
		#region OutAccountID
		public abstract class outAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _OutAccountID;
		[PXDefault()]
		[GL.CashAccount(DisplayName = "Account", DescriptionField = typeof(CashAccount.descr))]
		public virtual Int32? OutAccountID
		{
			get
			{
				return this._OutAccountID;
			}
			set
			{
				this._OutAccountID = value;
			}
		}
		#endregion
		#region InAccountID
		public abstract class inAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _InAccountID;
		[PXDefault()]
		[GL.CashAccount(DisplayName = "Account", DescriptionField = typeof(CashAccount.descr))]
		public virtual Int32? InAccountID
		{
			get
			{
				return this._InAccountID;
			}
			set
			{
				this._InAccountID = value;
			}
		}
		#endregion
		#region OutCuryInfoID
		public abstract class outCuryInfoID : PX.Data.IBqlField
		{
		}
		protected Int64? _OutCuryInfoID;
		[PXDBLong()]
        [CurrencyInfo(ModuleCode = "CA", CuryIDField = "OutCuryID", CuryRateField = "OutCuryRate")]
		public virtual Int64? OutCuryInfoID
		{
			get
			{
				return this._OutCuryInfoID;
			}
			set
			{
				this._OutCuryInfoID = value;
			}
		}
		#endregion
		#region InCuryInfoID
		public abstract class inCuryInfoID : PX.Data.IBqlField
		{
		}
		protected Int64? _InCuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(ModuleCode = "CA", CuryIDField = "InCuryID", CuryRateField="InCuryRate")]
		public virtual Int64? InCuryInfoID
		{
			get
			{
				return this._InCuryInfoID;
			}
			set
			{
				this._InCuryInfoID = value;
			}
		}
		#endregion
		#region InCuryID
		public abstract class inCuryID : PX.Data.IBqlField
		{
		}
		protected String _InCuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Enabled = false)]
		[PXDefault(typeof(Search<CashAccount.curyID, Where<CashAccount.cashAccountID, Equal<Current<CATransfer.inAccountID>>>>))]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String InCuryID
		{
			get
			{
				return this._InCuryID;
			}
			set
			{
				this._InCuryID = value;
			}
		}
		#endregion
		#region OutCuryID
		public abstract class outCuryID : PX.Data.IBqlField
		{
		}
		protected String _OutCuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Enabled = false)]
		[PXDefault(typeof(Search<CashAccount.curyID, Where<CashAccount.cashAccountID, Equal<Current<CATransfer.outAccountID>>>>))]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String OutCuryID
		{
			get
			{
				return this._OutCuryID;
			}
			set
			{
				this._OutCuryID = value;
			}
		}
		#endregion
		#region CuryTranOut
		public abstract class curyTranOut : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTranOut;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount")]
		[PXDBCurrency(typeof(CATransfer.outCuryInfoID), typeof(CATransfer.tranOut))]
		public virtual Decimal? CuryTranOut
		{
			get
			{
				return this._CuryTranOut;
			}
			set
			{
				this._CuryTranOut = value;
			}
		}
		#endregion
		#region CuryTranIn
		public abstract class curyTranIn : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTranIn;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount")]
		[PXDBCurrency(typeof(CATransfer.inCuryInfoID), typeof(CATransfer.tranIn))]
		public virtual Decimal? CuryTranIn
		{
			get
			{
				return this._CuryTranIn;
			}
			set
			{
				this._CuryTranIn = value;
			}
		}
		#endregion
		#region TranOut
		public abstract class tranOut : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranOut;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Base Currency Amount", Enabled = false)]
		public virtual Decimal? TranOut
		{
			get
			{
				return this._TranOut;
			}
			set
			{
				this._TranOut = value;
			}
		}
		#endregion
		#region TranIn
		public abstract class tranIn : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranIn;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Base Currency Amount", Enabled = false)]
		public virtual Decimal? TranIn
		{
			get
			{
				return this._TranIn;
			}
			set
			{
				this._TranIn = value;
			}
		}
		#endregion
		#region InDate
		public abstract class inDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _InDate;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Receipt Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? InDate
		{
			get
			{
				return this._InDate;
			}
			set
			{
				this._InDate = value;
			}
		}
		#endregion
		#region OutDate
		public abstract class outDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _OutDate;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Transfer Date")]
		public virtual DateTime? OutDate
		{
			get
			{
				return this._OutDate;
			}
			set
			{
				this._OutDate = value;
			}
		}
		#endregion
		#region OutExtRefNbr
		public abstract class outExtRefNbr : PX.Data.IBqlField
		{
		}
		protected String _OutExtRefNbr;
		[PXDBString(40, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Document Ref.")]
		public virtual String OutExtRefNbr
		{
			get
			{
				return this._OutExtRefNbr;
			}
			set
			{
				this._OutExtRefNbr = value;
			}
		}
		#endregion
		#region InExtRefNbr
		public abstract class inExtRefNbr : PX.Data.IBqlField
		{
		}
		protected String _InExtRefNbr;
		[PXDBString(40, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Document Ref.")]
		public virtual String InExtRefNbr
		{
			get
			{
				return this._InExtRefNbr;
			}
			set
			{
				this._InExtRefNbr = value;
			}
		}
		#endregion
		#region TranIDOut
		public abstract class tranIDOut : PX.Data.IBqlField
		{
		}
		protected Int64? _TranIDOut;
		[PXDBLong()]
		[TransferCashTranID()]
		[PXSelector(typeof(Search<CATran.tranID>), DescriptionField = typeof(CATran.batchNbr))]
		public virtual Int64? TranIDOut
		{
			get
			{
				return this._TranIDOut;
			}
			set
			{
				this._TranIDOut = value;
			}
		}
		#endregion
		#region TranIDIn
		public abstract class tranIDIn : PX.Data.IBqlField
		{
		}
		protected Int64? _TranIDIn;
		[PXDBLong()]
		[TransferCashTranID]
		[PXSelector(typeof(Search<CATran.tranID>), DescriptionField = typeof(CATran.batchNbr))]
		public virtual Int64? TranIDIn
		{
			get
			{
				return this._TranIDIn;
			}
			set
			{
				this._TranIDIn = value;
			}
		}
		#endregion
		#region LineCntr
		public abstract class lineCntr : PX.Data.IBqlField
		{
		}
		protected Int32? _LineCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? LineCntr
		{
			get
			{
				return this._LineCntr;
			}
			set
			{
				this._LineCntr = value;
			}
		}
		#endregion
		#region RGOLAmt
		public abstract class rGOLAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _RGOLAmt;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "RGOL", Enabled = false)]
		public virtual Decimal? RGOLAmt
		{
			get
			{
				return this._RGOLAmt;
			}
			set
			{
				this._RGOLAmt = value;
			}
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.IBqlField
		{
		}
		protected Boolean? _Hold;
		[PXDBBool()]
		[PXDefault(typeof(Search<CASetup.holdEntry>))]
		[PXUIField(DisplayName = "Hold")]
		public virtual Boolean? Hold
		{
			get
			{
				return this._Hold;
			}
			set
			{
				this._Hold = value;
				this.SetStatus();
			}
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.IBqlField
		{
		}
		protected Boolean? _Released;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
				this.SetStatus();
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.IBqlField
		{
		}
		protected Int64? _NoteID;
		[PXNote(DescriptionField = typeof(CATransfer.transferNbr))]
		public virtual Int64? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
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
		#region Status
		protected String _Status;
		[PXString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[CATransferStatus.List()]
		public virtual String Status
		{
			[PXDependsOnFields(typeof(released), typeof(hold))]
			get
			{
				return this._Status;
			}
			set
			{
			}
		}
		#endregion
		#region ClearedOut
		public abstract class clearedOut : PX.Data.IBqlField
		{
		}
		protected Boolean? _ClearedOut;
		[PXDBBool]
		[PXUIField(DisplayName = "Cleared")]
		[PXDefault(false)]
		public virtual Boolean? ClearedOut
		{
			get
			{
				return this._ClearedOut;
			}
			set
			{
				this._ClearedOut = value;
			}
		}
		#endregion
        #region ClearDateOut
        public abstract class clearDateOut : PX.Data.IBqlField
        {
        }
        protected DateTime? _ClearDateOut;
        [PXDBDate]
        [PXUIField(DisplayName = "Clear Date", Required = false)]
        public virtual DateTime? ClearDateOut
        {
            get
            {
                return this._ClearDateOut;
            }
            set
            {
                this._ClearDateOut = value;
            }
        }
        #endregion
		#region ClearedIn
		public abstract class clearedIn : PX.Data.IBqlField
		{
		}
		protected Boolean? _ClearedIn;
		[PXDBBool]
		[PXUIField(DisplayName = "Cleared")]
		[PXDefault(false)]
		public virtual Boolean? ClearedIn
		{
			get
			{
				return this._ClearedIn;
			}
			set
			{
				this._ClearedIn = value;
			}
		}
		#endregion
        #region ClearDateIn
        public abstract class clearDateIn : PX.Data.IBqlField
        {
        }
        protected DateTime? _ClearDateIn;
        [PXDBDate]
        [PXUIField(DisplayName = "Clear Date", Required = false)]
        public virtual DateTime? ClearDateIn
        {
            get
            {
                return this._ClearDateIn;
            }
            set
            {
                this._ClearDateIn = value;
            }
        }
        #endregion
		#region CashBalanceIn
		public abstract class cashBalanceIn : PX.Data.IBqlField
		{
		}
		protected Decimal? _CashBalanceIn;
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCury(typeof(CATransfer.inCuryID))]
		[PXUIField(DisplayName = "Available Balance", Enabled = false)]
		[CashBalance(typeof(CATransfer.inAccountID))]
		public virtual Decimal? CashBalanceIn
		{
			get
			{
				return this._CashBalanceIn;
			}
			set
			{
				this._CashBalanceIn = value;
			}
		}
		#endregion
		#region CashBalanceOut
		public abstract class cashBalanceOut : PX.Data.IBqlField
		{
		}
		protected Decimal? _CashBalanceOut;
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCury(typeof(CATransfer.outCuryID))]
        [PXUIField(DisplayName = "Available Balance", Enabled = false)]
		[CashBalance(typeof(CATransfer.outAccountID))]
		public virtual Decimal? CashBalanceOut
		{
			get
			{
				return this._CashBalanceOut;
			}
			set
			{
				this._CashBalanceOut = value;
			}
		}
		#endregion
        #region InGLBalance
        public abstract class inGLBalance : PX.Data.IBqlField
        {
        }
        protected Decimal? _InGLBalance;

        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXCury(typeof(CATransfer.inCuryID))]
        [PXUIField(DisplayName = "GL Balance", Enabled = false)]
        [GLBalance(typeof(CATransfer.inAccountID), null, typeof(CATransfer.inDate))]
        public virtual Decimal? InGLBalance
        {
            get
            {
                return this._InGLBalance;
            }
            set
            {
                this._InGLBalance = value;
            }
        }
        #endregion
        #region OutGLBalance
        public abstract class outGLBalance : PX.Data.IBqlField
        {
        }
        protected Decimal? _OutGLBalance;

        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXCury(typeof(CATransfer.outCuryID))]
        [PXUIField(DisplayName = "GL Balance", Enabled = false)]
        [GLBalance(typeof(CATransfer.outAccountID), null, typeof(CATransfer.outDate))]
        public virtual Decimal? OutGLBalance
        {
            get
            {
                return this._OutGLBalance;
            }
            set
            {
                this._OutGLBalance = value;
            }
        }
        #endregion
		#region TranIDOut_CATran_batchNbr
		public abstract class tranIDOut_CATran_batchNbr : PX.Data.IBqlField
		{
		}
		#endregion
		#region TranIDIn_CATran_batchNbr
		public abstract class tranIDIn_CATran_batchNbr : PX.Data.IBqlField
		{
		}
		#endregion
		#region Methods
		public virtual void SetStatus()
		{
			if (this._Released != null && (bool)this._Released)
			{
				this._Status = CATransferStatus.Released;
			}
			else if (this._Hold != null && (bool)this._Hold)
			{
				this._Status = CATransferStatus.Hold;
			}
			else
			{
				this._Status = CATransferStatus.Balanced;
			}
		}
		#endregion
	}

	/*
	public partial class AddExpenseFilter : PX.Data.IBqlTable
	{
		#region TranDate
		public abstract class tranDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _TranDate;
		[PXDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Doc. Date")]
		public virtual DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _CashAccountID;
		[PXDefault()]
		[GL.CashAccount(DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(CashAccount.descr))]
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
		#region CuryID
		public abstract class curyID : PX.Data.IBqlField
		{
		}
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.IBqlField
		{
		}
		protected String _ExtRefNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Document Ref.")]
		public virtual String ExtRefNbr
		{
			get
			{
				return this._ExtRefNbr;
			}
			set
			{
				this._ExtRefNbr = value;
			}
		}
		#endregion
		#region EntryTypeID
		public abstract class entryTypeID : PX.Data.IBqlField
		{
		}
		protected String _EntryTypeID;
		[PXString(10, IsUnicode = true)]
		[PXSelector(typeof(Search2<CAEntryType.entryTypeId,
						InnerJoin<CashAccountETDetail, On<CashAccountETDetail.entryTypeID, Equal<CAEntryType.entryTypeId>>>,
        				 Where<CashAccountETDetail.accountID, Equal<Current<AddExpenseFilter.cashAccountID>>,
			            And<CAEntryType.module, Equal<GL.BatchModule.moduleCA>, 
			            And<CAEntryType.drCr, Equal<CACredit>>>>>))]
		[PXUIField(DisplayName = "Entry Type")]
		public virtual String EntryTypeID
		{
			get
			{
				return this._EntryTypeID;
			}
			set
			{
				this._EntryTypeID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected String _Descr;
		[PXString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
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
		#region AccountID
		public abstract class accountID : PX.Data.IBqlField
		{
		}
		protected Int32? _AccountID;
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Offset Account")]
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
		[SubAccount(typeof(AddExpenseFilter.accountID), DisplayName = "Offset Subaccount")]
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
		#region CuryTranAmt
		public abstract class curyTranAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTranAmt;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount")]
		[PXDecimal(2)]
		public virtual Decimal? CuryTranAmt
		{
			get
			{
				return this._CuryTranAmt;
			}
			set
			{
				this._CuryTranAmt = value;
			}
		}
		#endregion
	}
	*/
	public class CACredit : Constant<string>
	{
		public CACredit() : base("C") { ;}
	}

	public class CATransferStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Balanced, Hold, Released, Pending, Rejected },
				new string[] { Messages.Balanced, Messages.Hold, Messages.Released, Messages.Pending, Messages.Rejected }) { ; }
		}

		public const string Balanced = "B";
		public const string Hold = "H";
		public const string Released = "R";
		public const string Rejected = "J";
		public const string Pending = "P";

		public class balanced : Constant<string>
		{
			public balanced() : base(Balanced) { ;}
		}

		public class hold : Constant<string>
		{
			public hold() : base(Hold) { ;}
		}

		public class released : Constant<string>
		{
			public released() : base(Released) { ;}
		}
		public class rejected : Constant<string>
		{
			public rejected() : base(Rejected) { ;}
		}
		public class pending : Constant<string>
		{
			public pending() : base(Pending) { ;}
		}

	}
}
