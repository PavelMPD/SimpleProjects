using PX.Data.EP;

namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	using PX.Objects.CS;
	using PX.Objects.CM;

	public class GLDocBatchStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
			new string[] { Hold, Balanced, Completed, Voided, Released  },
			new string[] { Messages.Hold, Messages.Balanced, Messages.Completed, Messages.Voided, Messages.Released }) { }
		}

		public const string Hold = "H";
		public const string Balanced = "B";
		public const string Completed = "C";
		public const string Voided = "V";
		public const string Released = "R";
		
		

		public class hold : Constant<string>
		{
			public hold() : base(Hold) { ;}
		}

		public class balanced : Constant<string>
		{
			public balanced() : base(Balanced) { ;}
		}
		
		public class completed : Constant<string>
		{
			public completed() : base(Completed) { ;}
		}

		public class voided : Constant<string>
		{
			public voided() : base(Voided) { ;}
		}
		
	}
	
	[System.SerializableAttribute()]
	[PXCacheName(Messages.GLDocBatch)]
	[PXPrimaryGraph(typeof(JournalWithSubEntry))]
	public partial class GLDocBatch : PX.Data.IBqlTable
	{
		#region Selected
		public abstract class selected : IBqlField
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
		#region BranchID
		public abstract class branchID : PX.Data.IBqlField
		{
		}
		protected Int32? _BranchID;
		[Branch()]
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
		#region Module
		public abstract class module : PX.Data.IBqlField
		{
		}
		protected String _Module;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault(GL.BatchModule.GL)]
		[PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.SelectorVisible)]
		[BatchModule.List()]
		[PXFieldDescription]
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
		#region BatchNbr
		public abstract class batchNbr : PX.Data.IBqlField
		{
		}
		protected String _BatchNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXSelector(typeof(Search<GLDocBatch.batchNbr, Where<GLDocBatch.module, Equal<Current<GLDocBatch.module>>>, OrderBy<Desc<GLDocBatch.batchNbr>>>), Filterable = true)]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(GLSetup.docBatchNumberingID),typeof(GLDocBatch.dateEntered))]
		[PXFieldDescription]
		public virtual String BatchNbr
		{
			get
			{
				return this._BatchNbr;
			}
			set
			{
				this._BatchNbr = value;
			}
		}
		#endregion
		#region LedgerID
		public abstract class ledgerID : PX.Data.IBqlField
		{
		}
		protected Int32? _LedgerID;
		[PXDBInt()]
		[PXDefault(typeof(Search<Branch.ledgerID, Where<Branch.branchID, Equal<Current<GLDocBatch.branchID>>>>))]
		[PXUIField(DisplayName = "Ledger", Visibility = PXUIVisibility.SelectorVisible, Enabled=false)]
		[PXSelector(typeof(Search2<Ledger.ledgerID, LeftJoin<Branch, On<Branch.ledgerID, Equal<Ledger.ledgerID>>>, Where<Ledger.balanceType, NotEqual<BudgetLedger>, And<Where<Ledger.balanceType, NotEqual<LedgerBalanceType.actual>, Or<Branch.branchID, Equal<Current<GLDocBatch.branchID>>>>>>>),
						SubstituteKey = typeof(Ledger.ledgerCD))]
		public virtual Int32? LedgerID
		{
			get
			{
				return this._LedgerID;
			}
			set
			{
				this._LedgerID = value;
			}
		}
		#endregion
		#region DateEntered
		public abstract class dateEntered : PX.Data.IBqlField
		{
		}
		protected DateTime? _DateEntered;
		[PXDBDate(MaxValue = "06/06/2079", MinValue = "01/01/1900")]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Transaction Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? DateEntered
		{
			get
			{
				return this._DateEntered;
			}
			set
			{
				this._DateEntered = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		protected String _FinPeriodID;
		[OpenPeriod(typeof(GLDocBatch.dateEntered))]
        [PXDefault()]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
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
		#region BatchType
		public abstract class batchType : PX.Data.IBqlField
		{
		}
		protected String _BatchType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(BatchTypeCode.Normal)]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.Invisible)]
		[BatchTypeCode.List()]
		public virtual String BatchType
		{
			get
			{
				return this._BatchType;
			}
			set
			{
				this._BatchType = value;
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.IBqlField
		{
		}
		protected String _Status;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(GLDocBatchStatus.Hold)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[GLDocBatchStatus.List()]
		public virtual String Status
		{
			[PXDependsOnFields(typeof(posted), typeof(voided), typeof(released), typeof(hold))]
			get
			{
				return this._Status;
			}
			set
			{
				//this._Status = value;
			}
		}
		#endregion
		#region CuryDebitTotal
		public abstract class curyDebitTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryDebitTotal;
		[PXDBCurrency(typeof(GLDocBatch.curyInfoID), typeof(GLDocBatch.debitTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Debit Total", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? CuryDebitTotal
		{
			get
			{
				return this._CuryDebitTotal;
			}
			set
			{
				this._CuryDebitTotal = value;
			}
		}
		#endregion
		#region CuryCreditTotal
		public abstract class curyCreditTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryCreditTotal;
		[PXDBCurrency(typeof(GLDocBatch.curyInfoID), typeof(GLDocBatch.creditTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Credit Total", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? CuryCreditTotal
		{
			get
			{
				return this._CuryCreditTotal;
			}
			set
			{
				this._CuryCreditTotal = value;
			}
		}
		#endregion
		#region CuryControlTotal
		public abstract class curyControlTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryControlTotal;
		[PXDBCurrency(typeof(GLDocBatch.curyInfoID), typeof(GLDocBatch.controlTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Control Total")]
		public virtual Decimal? CuryControlTotal
		{
			get
			{
				return this._CuryControlTotal;
			}
			set
			{
				this._CuryControlTotal = value;
			}
		}
		#endregion
		#region DebitTotal
		public abstract class debitTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _DebitTotal;
		[PXDBBaseCury(typeof(GLDocBatch.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DebitTotal
		{
			get
			{
				return this._DebitTotal;
			}
			set
			{
				this._DebitTotal = value;
			}
		}
		#endregion
		#region CreditTotal
		public abstract class creditTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _CreditTotal;
		[PXDBBaseCury(typeof(GLDocBatch.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CreditTotal
		{
			get
			{
				return this._CreditTotal;
			}
			set
			{
				this._CreditTotal = value;
			}
		}
		#endregion
		#region ControlTotal
		public abstract class controlTotal : PX.Data.IBqlField
		{
		}
		protected Decimal? _ControlTotal;
		[PXDBBaseCury(typeof(GLDocBatch.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ControlTotal
		{
			get
			{
				return this._ControlTotal;
			}
			set
			{
				this._ControlTotal = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[PX.Objects.CM.CurrencyInfo()]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		
		#region OrigModule
		public abstract class origModule : PX.Data.IBqlField
		{
		}
		protected String _OrigModule;
		[PXDBString(2, IsFixed = true)]
		public virtual String OrigModule
		{
			get
			{
				return this._OrigModule;
			}
			set
			{
				this._OrigModule = value;
			}
		}
		#endregion
		#region OrigBatchNbr
		public abstract class origBatchNbr : PX.Data.IBqlField
		{
		}
		protected String _OrigBatchNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Orig. Batch Number", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual String OrigBatchNbr
		{
			get
			{
				return this._OrigBatchNbr;
			}
			set
			{
				this._OrigBatchNbr = value;
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
		#region Posted
		public abstract class posted : PX.Data.IBqlField
		{
		}
		protected Boolean? _Posted;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Posted
		{
			get
			{
				return this._Posted;
			}
			set
			{
				this._Posted = value;
				this.SetStatus();
			}
		}
		#endregion
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.IBqlField
		{
		}
		protected String _TranPeriodID;
		[FinPeriodID(typeof(GLDocBatch.dateEntered))]
		[PXDefault(MapErrorTo = typeof(GLDocBatch.dateEntered))]
		public virtual String TranPeriodID
		{
			get
			{
				return this._TranPeriodID;
			}
			set
			{
				this._TranPeriodID = value;
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
		#region CuryID
		public abstract class curyID : PX.Data.IBqlField
		{
		}
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Search<Company.baseCuryID>))]
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
		#region NoteID
		public abstract class noteID : PX.Data.IBqlField
		{
		}
		protected Int64? _NoteID;
		[PXNote(DescriptionField = typeof(GLDocBatch.batchNbr))]
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
		[PXUIField(DisplayName = "Last Modified", Visibility = PXUIVisibility.Visible, Enabled = false)]
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
		#region Hold
		public abstract class hold : PX.Data.IBqlField
		{
		}
		protected Boolean? _Hold;
		[PXDBBool()]
		[PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.Visible)]
        [PXDefault(true,typeof(Search<GLSetup.vouchersHoldEntry>))]
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
		#region Voided
		public abstract class voided : PX.Data.IBqlField
		{
		}
		protected Boolean? _Voided;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Voided
		{
			get
			{
				return this._Voided;
			}
			set
			{
				this._Voided = value;
				this.SetStatus();
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.IBqlField
		{
		}
		protected String _Description;
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible)]
		[PXDBString(255, IsUnicode = true)]
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
		#region Methods
		protected virtual void SetStatus()
		{
			if (this._Voided != null && (bool)this._Voided)
			{
				this._Status = GLDocBatchStatus.Voided;
			}
			else if (this._Hold != null && (bool)this._Hold)
			{
				this._Status = GLDocBatchStatus.Hold;
			}			
			else if (this._Released != null && (bool)this._Released == false)
			{
				this._Status = GLDocBatchStatus.Balanced;
			}
			else if (this._Released != null && (bool)this._Released ==true)
			{
				this._Status = GLDocBatchStatus.Released;
			}			
		}
		#endregion
	}
}
