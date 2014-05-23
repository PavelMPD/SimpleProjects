namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	using PX.Objects.CS;
	using PX.Objects.CM;
	
	[System.SerializableAttribute()]
    [PXPrimaryGraph(typeof(GLSetupMaint))]
    [PXCacheName(Messages.GLSetupMaint)]
    public partial class GLSetup : PX.Data.IBqlTable
	{
		#region YtdNetIncAccountID
		public abstract class ytdNetIncAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _YtdNetIncAccountID;
		[PXDefault()]
		[Account(null, typeof(Search<Account.accountID, Where<Match<Current<AccessInfo.userName>>>>), DisplayName = "YTD Net Income Acct", DescriptionField = typeof(Account.description), Visibility = PXUIVisibility.SelectorVisible)]
        [PXRestrictor(typeof(Where<Account.type, Equal<AccountType.liability>>), Messages.YTDNetIncomeMayBeLiability)]
        public virtual Int32? YtdNetIncAccountID
		{
			get
			{
				return this._YtdNetIncAccountID;
			}
			set
			{
				this._YtdNetIncAccountID = value;
			}
		}
		#endregion
		#region RetEarnAccountID
		public abstract class retEarnAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _RetEarnAccountID;
		[PXDefault()]
		[Account(null, typeof(Search<Account.accountID, Where<Match<Current<AccessInfo.userName>>>>), DisplayName = "Retained Earnings Acct", DescriptionField = typeof(Account.description), Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? RetEarnAccountID
		{
			get
			{
				return this._RetEarnAccountID;
			}
			set
			{
				this._RetEarnAccountID = value;
			}
		}
		#endregion
		#region AutoRevOption
		public abstract class autoRevOption : PX.Data.IBqlField
		{
		}
		protected String _AutoRevOption;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(AutoRevOptions.OnPost)]
		[PXUIField(DisplayName = "Generate Reversing Entries", Visibility = PXUIVisibility.Visible)]
		[AutoRevOptions.List()]
		//[PXStringList("P;On Post,C;On Period Closing")]
		public virtual String AutoRevOption
		{
			get
			{
				return this._AutoRevOption;
			}
			set
			{
				this._AutoRevOption = value;
			}
		}
		#endregion
		#region AutoRevEntry
		public abstract class autoRevEntry : PX.Data.IBqlField
		{
		}
		protected Boolean? _AutoRevEntry;
		[PXDBBool()]
		[PXDefault(false)]
        [PXUIField(DisplayName = "Create Negative Entries on Reversal")]
		public virtual Boolean? AutoRevEntry
		{
			get
			{
				return this._AutoRevEntry;
			}
			set
			{
				this._AutoRevEntry = value;
			}
		}
		#endregion
		#region AutoPostOption
		public abstract class autoPostOption : PX.Data.IBqlField
		{
		}
		protected Boolean? _AutoPostOption;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Automatically Post on Release", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? AutoPostOption
		{
			get
			{
				return this._AutoPostOption;
			}
			set
			{
				this._AutoPostOption = value;
			}
		}
		#endregion
		#region COAOrder
		public abstract class cOAOrder : PX.Data.IBqlField
		{
		}
		protected Int16? _COAOrder;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Chart of Accounts Order")]
		public virtual Int16? COAOrder
		{
			get
			{
				return this._COAOrder;
			}
			set
			{
				this._COAOrder = value;
			}
		}
		#endregion
		#region RequireControlTotal
		public abstract class requireControlTotal : PX.Data.IBqlField
		{
		}
		protected Boolean? _RequireControlTotal;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Validate Batch Control Totals on Entry")]
		public virtual Boolean? RequireControlTotal
		{
			get
			{
				return this._RequireControlTotal;
			}
			set
			{
				this._RequireControlTotal = value;
			}
		}
		#endregion
		#region PostClosedPeriods
		public abstract class postClosedPeriods : PX.Data.IBqlField
		{
		}
		protected Boolean? _PostClosedPeriods;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Posting to Closed Periods")]
		public virtual Boolean? PostClosedPeriods
		{
			get
			{
				return this._PostClosedPeriods;
			}
			set
			{
				this._PostClosedPeriods = value;
			}
		}
		#endregion
		#region BatchNumberingID
		public abstract class batchNumberingID : PX.Data.IBqlField
		{
		}
		protected String _BatchNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("BATCH")]
		[PXUIField(DisplayName = "Batch Numbering Sequence")]
		[PXSelector(typeof(Numbering.numberingID),
			typeof(Numbering.numberingID), typeof(Numbering.descr),
			DescriptionField = typeof(Numbering.descr))]
		public virtual String BatchNumberingID
		{
			get
			{
				return this._BatchNumberingID;
			}
			set
			{
				this._BatchNumberingID = value;
			}
		}
		#endregion
		#region DocBatchNumberingID
		public abstract class docBatchNumberingID : PX.Data.IBqlField
		{
		}
		protected String _DocBatchNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("BATCH")]
		[PXUIField(DisplayName = "Document Batch Numbering Sequence")]
		[PXSelector(typeof(Numbering.numberingID),
			typeof(Numbering.numberingID), typeof(Numbering.descr),
			DescriptionField = typeof(Numbering.descr))]
		public virtual String DocBatchNumberingID
		{
			get
			{
				return this._DocBatchNumberingID;
			}
			set
			{
				this._DocBatchNumberingID = value;
			}
		}
		#endregion
		#region TBImportNumberingID
		public abstract class tBImportNumberingID : PX.Data.IBqlField
		{
		}
		protected String _TBImportNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("TBIMPORT")]
		[PXUIField(DisplayName = "Import Numbering Sequence")]
		[PXSelector(typeof(Numbering.numberingID),
			typeof(Numbering.numberingID), typeof(Numbering.descr),
			DescriptionField = typeof(Numbering.descr))]
		public virtual String TBImportNumberingID
		{
			get
			{
				return this._TBImportNumberingID;
			}
			set
			{
				this._TBImportNumberingID = value;
			}
		}
		#endregion
		#region AllocationNumberingID
		public abstract class allocationNumberingID : PX.Data.IBqlField
		{
		}
		protected String _AllocationNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("ALLOCATION")]
		[PXUIField(DisplayName = "Allocation Numbering Sequence")]
		[PXSelector(typeof(Numbering.numberingID),
			typeof(Numbering.numberingID), typeof(Numbering.descr),
			DescriptionField = typeof(Numbering.descr))]
		public virtual String AllocationNumberingID
		{
			get
			{
				return this._AllocationNumberingID;
			}
			set
			{
				this._AllocationNumberingID = value;
			}
		}
		#endregion
		#region ScheduleNumberingID
		public abstract class scheduleNumberingID : PX.Data.IBqlField
		{
		}
		protected String _ScheduleNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("SCHEDULE")]
		[PXUIField(DisplayName = "Schedule Numbering Sequence")]
		[PXSelector(typeof(Numbering.numberingID),
			typeof(Numbering.numberingID), typeof(Numbering.descr),
			DescriptionField = typeof(Numbering.descr))]
		public virtual String ScheduleNumberingID
		{
			get
			{
				return this._ScheduleNumberingID;
			}
			set
			{
				this._ScheduleNumberingID = value;
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
		#region HoldEntry
		public abstract class holdEntry : PX.Data.IBqlField
		{
		}
		protected Boolean? _HoldEntry;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Hold Batches on Entry")]
		public virtual Boolean? HoldEntry
		{
			get
			{
				return this._HoldEntry;
			}
			set
			{
				this._HoldEntry = value;
			}
		}
		#endregion
        #region VouchersHoldEntry
        public abstract class vouchersHoldEntry : PX.Data.IBqlField
        {
        }
        protected Boolean? _VouchersHoldEntry;
        [PXDBBool()]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Hold Vouchers on Entry")]
        public virtual Boolean? VouchersHoldEntry
        {
            get
            {
                return this._VouchersHoldEntry;
            }
            set
            {
                this._VouchersHoldEntry = value;
            }
        }
        #endregion
		#region ConsolSegmentId
		public abstract class consolSegmentId : PX.Data.IBqlField
		{
			public class PXConsolSegmentSelectorAttribute : PXSelectorAttribute
			{
				public PXConsolSegmentSelectorAttribute()
					: base(typeof(Search<Segment.segmentID, Where<Segment.dimensionID, Equal<SubAccountAttribute.dimensionName>>>),
							typeof(Segment.segmentID), typeof(Segment.descr))
				{
					DescriptionField = typeof(Segment.descr);
					_UnconditionalSelect = new Search<Segment.segmentID, Where<Segment.dimensionID, Equal<SubAccountAttribute.dimensionName>,
												And<Segment.segmentID, Equal<Required<Segment.segmentID>>>>>();
				}
			}
		}
		protected Int16? _ConsolSegmentId;
		[PXDBShort()]
		[PXUIField(DisplayName = "Consolidation Segment Number")]
		[consolSegmentId.PXConsolSegmentSelector]
		public virtual Int16? ConsolSegmentId
		{
			get
			{
				return this._ConsolSegmentId;
			}
			set
			{
				this._ConsolSegmentId = value;
			}
		}
		#endregion
		#region PerRetainTran
		public abstract class perRetainTran : PX.Data.IBqlField
		{
		}
		protected Int16? _PerRetainTran;
		[PXDBShort()]
		[PXDefault((short)99)]
		[PXUIField(DisplayName = "Keep Transactions for", Visibility = PXUIVisibility.Visible)]
		public virtual Int16? PerRetainTran
		{
			get
			{
				return this._PerRetainTran;
			}
			set
			{
				this._PerRetainTran = value;
			}
		}
		#endregion
		#region DefaultSubID
		public abstract class defaultSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _DefaultSubID;
		[SubAccount(DisplayName="Default Subaccount")]
		public virtual Int32? DefaultSubID
		{
			get
			{
				return this._DefaultSubID;
			}
			set
			{
				this._DefaultSubID = value;
			}
		}
		#endregion
		#region TrialBalanceSign
		public abstract class trialBalanceSign : IBqlField
		{
            public const string Normal = "N";
            public const string Reversed = "R";

            public class ListAttribute : PXStringListAttribute
            {
                public ListAttribute()
                    : base(
                        new string[] { Normal, Reversed },
                        new string[] { Messages.Normal, Messages.Reversed }) { }
            }

            public class normal : Constant<string> { public normal() : base(Normal) {} }
            public class reversed : Constant<string> { public reversed() : base(Reversed) {} }

        }
		protected String _TrialBalanceSign;
		[PXDBString]
		[trialBalanceSign.List]
		[PXDefault(trialBalanceSign.Normal)]
		[PXUIField(DisplayName = "Sign of the Trial Balance")]
		public virtual String TrialBalanceSign
		{
			get
			{
				return this._TrialBalanceSign;
			}
			set
			{
				this._TrialBalanceSign = value;
			}
		}
		#endregion
	}

	public static class AutoRevOptions
	{
		public const string OnPost ="P";
		public const string OnPeriodClosing ="C";

		public class ListAttribute : PXStringListAttribute
		{
				public ListAttribute()
					: base(
						new string[] { AutoRevOptions.OnPost, AutoRevOptions.OnPeriodClosing},
						new string[] { Messages.AutoRevOnPost, Messages.AutoRevOnPeriodClosing}) { }
		}	
	}

}
