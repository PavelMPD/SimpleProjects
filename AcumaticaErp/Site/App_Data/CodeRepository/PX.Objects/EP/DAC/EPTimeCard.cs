using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.TM;

namespace PX.Objects.EP
{
	[PXPrimaryGraph(typeof(TimeCardMaint))]
	[PXCacheName(Messages.TimeCard)]
	[Serializable]
	[PXEMailSource]
	public partial class EPTimeCard : IBqlTable, IAssign
	{

		#region TimeCardCD
		public abstract class timeCardCD : IBqlField { }

		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(EPSetup.timeCardNumberingID), typeof(AccessInfo.businessDate))]
		[PXSelector(typeof(Search2<EPTimeCard.timeCardCD,
			InnerJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<EPTimeCard.employeeID>>>,
			Where<EPTimeCard.createdByID, Equal<Current<AccessInfo.userID>>,
						 Or<EPEmployee.userID, Equal<Current<AccessInfo.userID>>,
						 Or<EPEmployee.userID, OwnedUser<Current<AccessInfo.userID>>,
						 Or<EPTimeCard.noteID, Approver<Current<AccessInfo.userID>>>>>>>),
			typeof(EPTimeCard.timeCardCD),
            typeof(EPTimeCard.employeeID),
			typeof(EPTimeCard.weekDescription),
			typeof(EPTimeCard.status))]
		[PXFieldDescription]
		public virtual String TimeCardCD { get; set; }
		#endregion

        #region EmployeeID
		public abstract class employeeID : IBqlField { }

		[PXDBInt]
        [PXDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>))]
		[PXUIField(DisplayName = "Employee")]
        [PXSubordinateSelector]
        [PXFieldDescription]
        public virtual Int32? EmployeeID { get; set; }
		#endregion
		#region Status

		public abstract class status : IBqlField { }

        public const string ApprovedStatus = "A";
        public const string HoldStatus = "H";
        public const string ReleasedStatus = "R";
        public const string OpenStatus = "O";
        public const string RejectedStatus = "C";

        [PXDBString(1)]
        [PXDefault("H")]
        [PXStringList(new[] { HoldStatus, OpenStatus, ApprovedStatus, RejectedStatus, ReleasedStatus }, new[] { "On Hold", "Open", "Approved", "Rejected", "Released" })]
        [PXUIField(DisplayName = "Status")]
        public virtual String Status { get; set; }

		#endregion
		#region WeekID

		public abstract class weekId : IBqlField { }

	    protected Int32? _WeekID;
	    [PXDBInt]
	    [PXUIField(DisplayName = "Week")]
		[PXWeekSelector2(DescriptionField = typeof(EPWeekRaw.shortDescription))]
        public virtual Int32? WeekID
	    {
            get
            {
                return this._WeekID;
            }
            set
            {
                this._WeekID = value;
            }
	    }

		#endregion
		#region OrigTimeCardCD
		public abstract class origTimeCardCD : IBqlField { }
        [PXUIField(DisplayName = "Orig. Ref. Nbr.", Enabled = false)]
		[PXDBString(10, IsUnicode = true)]
		public virtual String OrigTimeCardCD { get; set; }
		#endregion
		#region IsApproved

		public abstract class isApproved : IBqlField { }

		[PXDBBool]
        [PXDefault(false)]
		[PXUIField(Visible = false)]
		public virtual Boolean? IsApproved { get; set; }

		#endregion
		#region IsRejected

		public abstract class isRejected : IBqlField { }

		[PXDBBool]
        [PXDefault(false)]
		[PXUIField(Visible = false)]
		public virtual Boolean? IsRejected { get; set; }

		#endregion
		#region IsHold

		public abstract class isHold : IBqlField { }

		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Visible = false)]
		public virtual Boolean? IsHold { get; set; }

		#endregion
		#region IsReleased

		public abstract class isReleased : IBqlField { }

		[PXDBBool]
        [PXDefault(false)]
		[PXUIField(Visible = false)]
		public virtual Boolean? IsReleased { get; set; }

		#endregion
		#region WorkgroupID
		public abstract class workgroupID : IBqlField { }

		[PXInt]		
		[PXUIField(DisplayName = "Workgroup ID", Visible = false)]
		[PXSelector(typeof(EPCompanyTreeOwner.workGroupID), SubstituteKey = typeof(EPCompanyTreeOwner.description))]
		public virtual int? WorkgroupID { get; set; }
		#endregion
		#region OwnerID
		public abstract class ownerID : IBqlField { }

		[PXGuid]
		[PXUIField(Visible = false)]
		public virtual Guid? OwnerID { get; set; }
		#endregion
		#region SummaryLineCntr
		public abstract class summaryLineCntr : PX.Data.IBqlField
		{
		}
		protected int? _SummaryLineCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual int? SummaryLineCntr
		{
			get
			{
				return this._SummaryLineCntr;
			}
			set
			{
				this._SummaryLineCntr = value;
			}
		}
		#endregion	

        #region System Columns
        #region NoteID
        public abstract class noteID : PX.Data.IBqlField
        {
        }
        protected Int64? _NoteID;
        [PXNote(typeof(EPTimeCard),
            DescriptionField = typeof(EPTimeCard.timeCardCD),
            Selector = typeof(EPTimeCard.timeCardCD),
            ShowInReferenceSelector = true)]
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
        #endregion


        #region Unbound Fields (Calculated in the TimecardMaint graph)

        #region Selected
        public abstract class selected : IBqlField { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }
        #endregion

        #region WeekStartDate (Used in Report)

		public abstract class weekStartDate : IBqlField { }

		protected DateTime? _WeekStartDate;
		[PXDate]
		[PXUIField(DisplayName = "Week Start Date")]
		[PXFormula(typeof(Selector<EPTimeCard.weekId, EPWeekRaw.startDate>))]
		public virtual DateTime? WeekStartDate {
			get { return _WeekStartDate; }
			set { _WeekStartDate = value; }
		}

		#endregion
        
        public abstract class weekDescription : IBqlField { }
		[PXString]
		[PXUIField(DisplayName = "Week")]
		[PXFormula(typeof(Selector<EPTimeCard.weekId, EPWeekRaw.description>))]
		public virtual String WeekDescription { get; set; }

        public abstract class weekShortDescription : IBqlField { }
		[PXString]
		[PXUIField(DisplayName = "Week")]
		[PXFieldDescription]
		[PXFormula(typeof(Selector<EPTimeCard.weekId, EPWeekRaw.shortDescription>))]
		public virtual String WeekShortDescription { get; set; }

        public abstract class timeSpentCalc : IBqlField { }
        [PXInt]
        [PXUIField(DisplayName = "Regular", Enabled = false)]
        public virtual Int32? TimeSpentCalc { get; set; }
        
        public abstract class overtimeSpentCalc : IBqlField { }
        [PXInt]
        [PXUIField(DisplayName = "Overtime Spent", Enabled = false)]
        public virtual Int32? OvertimeSpentCalc { get; set; }
        
        public abstract class totalSpentCalc : IBqlField { }
        [PXInt]
        [PXUIField(DisplayName = "Total Time Spent", Enabled = false)]
        public virtual Int32? TotalSpentCalc { get; set; }
        
        public abstract class timeBillableCalc : IBqlField { }
        [PXInt]
        [PXUIField(DisplayName = "Billable", Enabled = false)]
        public virtual Int32? TimeBillableCalc { get; set; }
        
        public abstract class overtimeBillableCalc : IBqlField { }
        [PXInt]
        [PXUIField(DisplayName = "Billable Overtime", Enabled = false)]
        public virtual Int32? OvertimeBillableCalc { get; set; }
        
        public abstract class totalBillableCalc : IBqlField { }
        [PXInt]
        [PXUIField(DisplayName = "Total Billable", Enabled = false)]
        public virtual Int32? TotalBillableCalc { get; set; }

        public abstract class timecardType : IBqlField { }
	    [PXString]
        [PXStringList(new string[] { "N", "C", "D" }, new string[] { "Normal", "Correction", "Normal-Corrected" })]
	    [PXUIField(DisplayName = "Type", Enabled = false)]
	    public virtual string TimecardType
	    {
	        get;set;// { return string.IsNullOrEmpty(OrigTimeCardCD) ? "N" : "C"; }

	    }

        public abstract class billingRateCalc : IBqlField { }
        [PXInt]
        [PXUIField(DisplayName = "Billing Ratio", Enabled = false)]
        public virtual Int32? BillingRateCalc
        {
            get
            {
                if (TotalSpentCalc != 0)
                {
                    return TotalBillableCalc * 100 / TotalSpentCalc;
                }
                else
                {
                    return 0;
                }
            }
        }

        #endregion
    }
}
