using PX.Data.EP;

namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	using PX.Objects.CS;
	using PX.Objects.CM;

	public class BatchStatus
	{ 
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
			new string[] { Hold, Balanced, Unposted, Posted, Completed, Voided, Released, PartiallyReleased, Scheduled },
			new string[] { Messages.Hold, Messages.Balanced, Messages.Unposted, Messages.Posted, Messages.Completed, Messages.Voided, Messages.Released, Messages.PartiallyReleased, Messages.Scheduled }) { }
		}

		public const string Hold = "H";
		public const string Balanced = "B";
		public const string Unposted = "U";
		public const string Posted = "P";
		public const string Completed = "C";
		public const string Voided = "V";
		public const string Released = "R";
		public const string PartiallyReleased = "Q";
		public const string Scheduled = "S";

		public class hold : Constant<string>
		{
			public hold() : base(Hold) { ;}
		}

		public class balanced : Constant<string>
		{
			public balanced() : base(Balanced) { ;}
		}

		public class unposted : Constant<string>
		{
			public unposted() : base(Unposted) { ;}
		}

		public class posted : Constant<string>
		{
			public posted() : base(Posted) { ;}
		}

		public class completed : Constant<string>
		{
			public completed() : base(Completed) { ;}
		}

		public class voided : Constant<string>
		{
			public voided() : base(Voided) { ;}
		}

		public class scheduled : Constant<string>
		{
			public scheduled() : base(Scheduled) { ;}
		}
	}

	public static class BatchModule
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { GL, AP, AR, CM, CA, IN, DR, FA, PM},
				new string[] { Messages.ModuleGL, Messages.ModuleAP, Messages.ModuleAR, Messages.ModuleCM, Messages.ModuleCA, Messages.ModuleIN, Messages.ModuleDR, Messages.ModeleFA, Messages.ModulePM }) { }
		}

		public class FullListAttribute : PXStringListAttribute
		{
			public FullListAttribute()
				: base(
				new string[] { GL, AP, AR, CM, CA, IN, DR, FA, PM, TX, SO, PO },
				new string[] { Messages.ModuleGL, Messages.ModuleAP, Messages.ModuleAR, Messages.ModuleCM, Messages.ModuleCA, Messages.ModuleIN, Messages.ModuleDR, Messages.ModeleFA, Messages.ModulePM, Messages.ModuleTX, Messages.ModuleSO, Messages.ModulePO }) { }
		}

		/// <summary>
		/// Specilaized for GL.BatchModule version of the <see cref="AutoNumberAttribute"/><br/>
		/// It defines how the new numbers are generated for the GL Batch. <br/>
		/// References Batch.module and Batch.dateEntered fields of the document,<br/>
		/// and also define a link between  numbering ID's defined in GLSetup (namely GLSetup.batchNumberingID)<br/>
		/// and CADeposit: <br/>
		/// </summary>	
		public class NumberingAttribute : AutoNumberAttribute
		{
			public NumberingAttribute()
				:base(typeof(Batch.module), typeof(Batch.dateEntered), 
				new string[] { GL, AP, AR, CM, CA, IN, DR, FA, PM }, 
				new Type[] 
				{	
					typeof(GLSetup.batchNumberingID), 
					typeof(Search<AP.APSetup.batchNumberingID>), 
					typeof(Search<AR.ARSetup.batchNumberingID>), 
					typeof(Search<CM.CMSetup.batchNumberingID>),
					typeof(Search<CA.CASetup.batchNumberingID>), 
					typeof(Search<IN.INSetup.batchNumberingID>), 
					typeof(GLSetup.batchNumberingID),
					typeof(Search<FA.FASetup.batchNumberingID>),
					typeof(Search<PM.PMSetup.batchNumberingID>) 
				})
			{
			}
		}

		public class CashManagerListAttribute : PXStringListAttribute
		{
			public CashManagerListAttribute()
				: base(
				new string[] { AP, AR, CA },
				new string[] { Messages.ModuleAP, Messages.ModuleAR, Messages.ModuleCA }) { }
		}

		public class GLOnlyListAttribute : PXStringListAttribute
		{
			public GLOnlyListAttribute()
				: base(
					new string[] { GL },
					new string[] { Messages.ModuleGL }) { }
		}
				
		/// <summary>
		/// List of Modules supported in Project Management
		/// </summary>
		public class PMListAttribute : PXStringListAttribute
		{
			public PMListAttribute()
				: base(
				new string[] { GL, AP, AR, IN, PM, CA },
				new string[] { Messages.ModuleGL, Messages.ModuleAP, Messages.ModuleAR, Messages.ModuleIN, Messages.ModulePM, Messages.ModuleCA }) { }
		}


		public const string GL = "GL";
		public const string AP = "AP";
		public const string AR = "AR";
		public const string CA = "CA";
		public const string CM = "CM";
		public const string IN = "IN";
		public const string SO = "SO";
		public const string PO = "PO";
		public const string DR = "DR";
		public const string FA = "FA";
		public const string EP = "EP";
		public const string PM = "PM";
		public const string TX = "TX";
		public const string CR = "CR";

		public class moduleGL : Constant<string>
		{
			public moduleGL() : base(GL) { ;}
		}

		public class moduleAP : Constant<string>
		{
			public moduleAP() : base(AP) { ;}
		}

		public class moduleTX : Constant<string>
		{
			public moduleTX() : base(TX) { ;}
		}

		public class moduleAR : Constant<string>
		{
			public moduleAR() : base(AR) { ;}
		}

		public class moduleCM : Constant<string>
		{
			public moduleCM() : base(CM) { ;}
		}

		public class moduleCA : Constant<string>
		{
			public moduleCA() : base(CA) { ;}
		}

		public class moduleIN : Constant<string>
		{
			public moduleIN() : base(IN) { ;}
		}

		public class moduleSO : Constant<string>
		{
			public moduleSO() : base(SO) { ;}
		}

		public class modulePO : Constant<string>
		{
			public modulePO() : base(PO) { ;}
		}

		public class moduleDR : Constant<string>
		{
			public moduleDR() : base(DR) { ;}
		}

		public class moduleFA : Constant<string>
		{
			public moduleFA() : base(FA) { ;}
		}

		public class moduleEP : Constant<string>
		{
			public moduleEP() : base(EP) { ;}
		}

		public class modulePM : Constant<string>
		{
			public modulePM() : base(PM) { ;}
		}
	}

	public static class BatchTypeCode
	{
		public const string Normal = "H";
		public const string Recurring = "R";
		public const string Consolidation = "C";
		public const string TrialBalance = "T";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
					new string[] { BatchTypeCode.Normal, BatchTypeCode.Recurring, BatchTypeCode.Consolidation, BatchTypeCode.TrialBalance },
					new string[] { Messages.BTNormal, Messages.BTRecurring, Messages.BTConsolidation, Messages.BTTrialBalance }) { }
		}
	}

	[System.SerializableAttribute()]
	[PXCacheName(Messages.Batch)]
	[PXPrimaryGraph(typeof(JournalEntry))]
	public partial class Batch : PX.Data.IBqlTable
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
		[PXDefault()]
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
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<Current<Batch.module>>,And<Batch.draft,Equal<False>>>, OrderBy<Desc<Batch.batchNbr>>>), Filterable = true)]
		[PXUIField(DisplayName="Batch Number", Visibility=PXUIVisibility.SelectorVisible)]
		[BatchModule.Numbering()]
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
		[PXDefault(typeof(Search<Branch.ledgerID, Where<Branch.branchID, Equal<Current<Batch.branchID>>>>))]
		[PXUIField(DisplayName = "Ledger", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search5<Ledger.ledgerID, LeftJoin<Branch, On<Ledger.ledgerID, Equal<Branch.ledgerID>>>, Where<Ledger.balanceType, NotEqual<BudgetLedger>, And<Where<Ledger.balanceType, NotEqual<LedgerBalanceType.actual>, Or<Optional2<Batch.branchID>, IsNull, Or<Branch.branchID, Equal<Optional2<Batch.branchID>>>>>>>,Aggregate<GroupBy<Ledger.ledgerID, GroupBy<Ledger.postInterCompany>>>>),
			//						new Type[] { typeof(Ledger.ledgerCD), typeof(Ledger.descr) },
			//						new string[] { "Ledger ID", "Description" }, 
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
		[PXDBDate(MaxValue="06/06/2079", MinValue="01/01/1900")]
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
        [OpenPeriod(typeof(Batch.dateEntered))]
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
		#region NumberCode
		public abstract class numberCode : PX.Data.IBqlField
		{
		}
		protected String _NumberCode;
		[PXDBString(5, IsUnicode = true, InputMask = ">aaaaa")]
		[PXUIField(DisplayName = "Number. Code")]
		public virtual String NumberCode
		{
			get
			{
				return this._NumberCode;
			}
			set
			{
				this._NumberCode = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.IBqlField
		{
		}
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(Visible = false)]
		[AutoNumber(typeof(Search<GLNumberCode.numberingID, Where<GLNumberCode.numberCode,Equal<Current<Batch.numberCode>>>>), typeof(Batch.dateEntered))]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.IBqlField
		{
		}
		protected String _Status;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(BatchStatus.Hold)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[BatchStatus.List()]
		public virtual String Status
		{
			[PXDependsOnFields(typeof(posted), typeof(voided), typeof(scheduled), typeof(released), typeof(hold))]
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
		[PXDBCurrency(typeof(Batch.curyInfoID), typeof(Batch.debitTotal))]
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
		[PXDBCurrency(typeof(Batch.curyInfoID), typeof(Batch.creditTotal))]
		[PXDefault(TypeCode.Decimal,"0.0")]
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
		[PXDBCurrency(typeof(Batch.curyInfoID), typeof(Batch.controlTotal))]
		[PXDefault(TypeCode.Decimal,"0.0")]
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
		[PXDBBaseCury(typeof(Batch.ledgerID))]
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
		[PXDBBaseCury(typeof(Batch.ledgerID))]
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
		[PXDBBaseCury(typeof(Batch.ledgerID))]
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
		#region AutoReverse
		public abstract class autoReverse : PX.Data.IBqlField
		{
		}
		protected Boolean? _AutoReverse;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName="Auto Reversing", Visibility=PXUIVisibility.Visible)]
		public virtual Boolean? AutoReverse
		{
			get
			{
				return this._AutoReverse;
			}
			set
			{
				this._AutoReverse = value;
			}
		}
		#endregion
		#region AutoReverseCopy
		public abstract class autoReverseCopy : PX.Data.IBqlField
		{
		}
		protected Boolean? _AutoReverseCopy;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Reversing Entry", Enabled = false, Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? AutoReverseCopy
		{
			get
			{
				return this._AutoReverseCopy;
			}
			set
			{
				this._AutoReverseCopy = value;
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
		[PXUIField(DisplayName="Orig. Batch Number", Visibility=PXUIVisibility.Visible, Enabled =false)]
        [PXSelector(typeof(Batch.batchNbr))]
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
		#region Draft
		public abstract class draft : PX.Data.IBqlField
		{
		}
		protected Boolean? _Draft;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Draft
		{
			get
			{
				return this._Draft;
			}
			set
			{
				this._Draft = value;
				this.SetStatus();
			}
		}
		#endregion
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.IBqlField
		{
		}
		protected String _TranPeriodID;
		[FinPeriodID(typeof(Batch.dateEntered))]
		[PXDefault(MapErrorTo = typeof(Batch.dateEntered))]
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
		[PXUIField(DisplayName = "Currency", Visibility=PXUIVisibility.SelectorVisible)]
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
		#region ScheduleID
		public abstract class scheduleID : PX.Data.IBqlField
		{
		}
		protected string _ScheduleID;
		[PXDBString(10, IsUnicode = true)]
		public virtual string ScheduleID
		{
			get
			{
				return this._ScheduleID;
			}
			set
			{
				this._ScheduleID = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.IBqlField
		{
		}
		protected Int64? _NoteID;
		[PXNote(DescriptionField = typeof(Batch.batchNbr))]
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
		[PXUIField(DisplayName = "Last Modified", Visibility = PXUIVisibility.Visible, Enabled=false)]
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
		[PXDefault(true, typeof(Search<GLSetup.holdEntry>))]
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
		#region Scheduled
		public abstract class scheduled : PX.Data.IBqlField
		{
		}
		protected Boolean? _Scheduled;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Scheduled
		{
			get
			{
				return this._Scheduled;
			}
			set
			{
				this._Scheduled = value;
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
        #region CreateTaxTran
        public abstract class createTaxTrans : PX.Data.IBqlField
        {
        }
        protected Boolean? _CreateTaxTrans;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName ="Create Tax Trans.")]
        public virtual Boolean? CreateTaxTrans
        {
            get
            {
                return this._CreateTaxTrans;
            }
            set
            {
                this._CreateTaxTrans = value;        
            }
        }
        #endregion
		#region SkipTaxValidation
		public abstract class skipTaxValidation : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Skip Tax Amounts Validation")]
		public virtual Boolean? SkipTaxValidation{ get; set;}
		#endregion

		#region Methods
		protected virtual void SetStatus()
		{
			if (this._Voided != null && (bool)this._Voided)
			{
				this._Status = "V";
			}
			else if (this._Hold != null && (bool)this._Hold)
			{
				this._Status = "H";
			}
			else if (this._Scheduled != null && (bool)this._Scheduled)
			{
				this._Status = "S";
			}
			else if (this._Released != null && (bool)this._Released == false)
			{
				this._Status = "B";
			}
			else if (this._Posted != null && (bool)this._Posted == false)
			{
				this._Status = "U";
			}
			else if (this._Posted != null && (bool)this._Posted)
			{
				this._Status = "P";
			}
		}
		#endregion
	}

    [Serializable]
	public partial class BatchReport : Batch
	{
		#region BatchNbr
		public new abstract class batchNbr : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
        [PXSelector(typeof(Search<Batch.batchNbr,
            Where<Batch.finPeriodID, Equal<Optional<Batch.finPeriodID>>,
            And<Batch.ledgerID, Equal<Optional<Batch.ledgerID>>,
            And<Batch.released, Equal<True>,
            And<Batch.posted, Equal<True>,
            And<Where<Batch.module, Equal<BatchModule.moduleGL>,
            Or<Batch.module, Equal<BatchModule.moduleCM>>>>>>>>>))]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.SelectorVisible)]
		public override String BatchNbr
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
    }
}
