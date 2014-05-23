using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.TM;
using PX.Objects.TX;
using PX.Objects.IN;
using PX.Objects.GL;
using PX.Objects.CM;

namespace PX.Objects.PM
{
	[PXCacheName(Messages.ProjectTask)]
	[PXPrimaryGraph(typeof(ProjectTaskEntry))]
    [Serializable]
	public partial class PMTask : PX.Data.IBqlTable, IAttributeSupport
	{
        #region Selected
        public abstract class selected : IBqlField
        {
        }
        protected bool? _Selected = false;
        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected
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

		#region ProjectID
		public abstract class projectID : PX.Data.IBqlField
		{
		}
		protected Int32? _ProjectID;
        [Project(typeof(Where<PMProject.nonProject, NotEqual<True>, And<PMProject.isTemplate, NotEqual<True>>>), DisplayName = "Project ID", IsKey = true, DirtyRead=true)]
		[PXParent(typeof(Select<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
        [PXDBLiteDefault(typeof(PMProject.contractID))]
        public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.IBqlField
		{
		}
		protected Int32? _TaskID;
		[PXDBIdentity()]
		[PXSelector(typeof(PMTask.taskID))]
		public virtual Int32? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion
		#region TaskCD
		public abstract class taskCD : PX.Data.IBqlField
		{
		}
		protected String _TaskCD;
		[PXDimension(ProjectTaskAttribute.DimensionName)]
		[PXDBString(IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Task ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String TaskCD
		{
			get
			{
				return this._TaskCD;
			}
			set
			{
				this._TaskCD = value;
			}
		}
		#endregion
		

		#region Description
		public abstract class description : PX.Data.IBqlField
		{
		}
		protected String _Description;
		[PXDBString(250, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
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
		#region CustomerID
		public abstract class customerID : PX.Data.IBqlField
		{
		}
		protected Int32? _CustomerID;

		[PXDefault(typeof(Search<PMProject.customerID, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Customer(DescriptionField = typeof(Customer.acctName), Visibility = PXUIVisibility.SelectorVisible, Enabled=false)]
		public virtual Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.IBqlField
		{
		}
		protected Int32? _LocationID;
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<PMTask.customerID>>>), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location", DescriptionField = typeof(Location.descr))]
		[PXDefault(typeof(Search<PMProject.locationID, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region RateTableID
		public abstract class rateTableID : PX.Data.IBqlField
		{
		}
		protected String _RateTableID;
		[PXDBString(PMRateTable.rateTableID.Length, IsUnicode = true)]
		[PXDefault(typeof(Search<PMProject.rateTableID, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXUIField(DisplayName = "Rate Table")]
        [PXSelector(typeof(PMRateTable.rateTableID))]
        public virtual String RateTableID
		{
			get
			{
                return this._RateTableID;
			}
			set
			{
                this._RateTableID = value;
			}
		}
		#endregion
        #region BillingID
        public abstract class billingID : PX.Data.IBqlField
        {
        }
        protected String _BillingID;
        [PXDefault(typeof(PMProject.billingID), PersistingCheck=PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search<PMBilling.billingID, Where<PMBilling.isActive, Equal<True>>>))]
        [PXUIField(DisplayName = "Billing Rule")]
		[PXDBString(PMBilling.billingID.Length, IsUnicode = true)]
        public virtual String BillingID
        {
            get
            {
                return this._BillingID;
            }
            set
            {
                this._BillingID = value;
            }
        }
        #endregion
		#region AllocationID
		public abstract class allocationID : PX.Data.IBqlField
		{
		}
		protected String _AllocationID;
		[PXDefault(typeof(PMProject.allocationID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<PMAllocation.allocationID, Where<PMAllocation.isActive, Equal<True>>>))]
		[PXUIField(DisplayName = "Allocation Rule")]
		[PXDBString(PMAllocation.allocationID.Length, IsUnicode = true)]
		public virtual String AllocationID
		{
			get
			{
				return this._AllocationID;
			}
			set
			{
				this._AllocationID = value;
			}
		}
		#endregion
        #region BillingOption
        public abstract class billingOption : PX.Data.IBqlField
        {
        }
        protected String _BillingOption;
        [PXDBString(1, IsFixed = true)]
        [PMBillingOption.List()]
        [PXDefault(PMBillingOption.OnBilling)]
        [PXUIField(DisplayName = "Billing Option", Required = true, Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String BillingOption
        {
            get
            {
                return this._BillingOption;
            }
            set
            {
                this._BillingOption = value;
            }
        }
        #endregion
		#region Status
		public abstract class status : PX.Data.IBqlField
		{
		}
		protected String _Status;
		[PXDBString(1, IsFixed = true)]
		[ProjectTaskStatus.List()]
		[PXDefault(ProjectTaskStatus.Planned)]
		[PXUIField(DisplayName = "Status", Required = true, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		#endregion
		#region PlannedStartDate
		public abstract class plannedStartDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _PlannedStartDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Planned Start")]
		public virtual DateTime? PlannedStartDate
		{
			get
			{
				return this._PlannedStartDate;
			}
			set
			{
				this._PlannedStartDate = value;
			}
		}
		#endregion
		#region PlannedEndDate
		public abstract class plannedEndDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _PlannedEndDate;
		[PXDBDate()]
		[PXVerifyEndDate(typeof(plannedStartDate), AutoChangeWarning = true)]
		[PXUIField(DisplayName = "Planned End")]
		public virtual DateTime? PlannedEndDate
		{
			get
			{
				return this._PlannedEndDate;
			}
			set
			{
				this._PlannedEndDate = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _StartDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Start Date")]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _EndDate;
		[PXDBDate()]
		[PXVerifyEndDate(typeof(startDate), AutoChangeWarning = true)]
		[PXUIField(DisplayName = "End Date")]
		public virtual DateTime? EndDate
		{
			get
			{
				return this._EndDate;
			}
			set
			{
				this._EndDate = value;
			}
		}
		#endregion
		#region ApproverID
		public abstract class approverID : PX.Data.IBqlField
		{
		}
		protected Int32? _ApproverID;
		[PXDBInt]
		[PXEPEmployeeSelector]
		[PXUIField(DisplayName = "Approver", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? ApproverID
		{
			get
			{
				return this._ApproverID;
			}
			set
			{
				this._ApproverID = value;
			}
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.IBqlField
		{
		}
		protected String _TaxCategoryID;
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category", Visible=false)]//Not used anywere
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
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
		#region DefaultAccountID
		public abstract class defaultAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _DefaultAccountID;
		[PXDefault(typeof(PMProject.defaultAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Default Account")]
		public virtual Int32? DefaultAccountID
		{
			get
			{
				return this._DefaultAccountID;
			}
			set
			{
				this._DefaultAccountID = value;
			}
		}
		#endregion
		#region DefaultSubID
		public abstract class defaultSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _DefaultSubID;
		[PXDefault(typeof(PMProject.defaultSubID), PersistingCheck=PXPersistingCheck.Nothing )]
		[SubAccount(DisplayName = "Default Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
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
        #region DefaultAccrualAccountID
        public abstract class defaultAccrualAccountID : PX.Data.IBqlField
        {
        }
        protected Int32? _DefaultAccrualAccountID;
        [PXDefault(typeof(PMProject.defaultAccrualAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
        [Account(DisplayName = "Default Accrual Account")]
        public virtual Int32? DefaultAccrualAccountID
        {
            get
            {
                return this._DefaultAccrualAccountID;
            }
            set
            {
                this._DefaultAccrualAccountID = value;
            }
        }
        #endregion
        #region DefaultAccrualSubID
        public abstract class defaultAccrualSubID : PX.Data.IBqlField
        {
        }
        protected Int32? _DefaultAccrualSubID;
        [PXDefault(typeof(PMProject.defaultAccrualSubID), PersistingCheck = PXPersistingCheck.Nothing)]
        [SubAccount(DisplayName = "Default Accrual Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
        public virtual Int32? DefaultAccrualSubID
        {
            get
            {
                return this._DefaultAccrualSubID;
            }
            set
            {
                this._DefaultAccrualSubID = value;
            }
        }
        #endregion
        #region CompletedPct
		public abstract class completedPct : PX.Data.IBqlField
		{
		}
		protected Int32? _CompletedPct;
		[PXDBInt(MinValue = 0)]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Completed (%)")]
		public virtual Int32? CompletedPct
		{
			get
			{
				return this._CompletedPct;
			}
			set
			{
				this._CompletedPct = value;
			}
		}
		#endregion
       	#region IsActive
		public abstract class isActive : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsActive;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Active", Enabled=false, Visibility=PXUIVisibility.Visible, Visible=false)]
		public virtual Boolean? IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				this._IsActive = value;
			}
		}
		#endregion
		#region IsCompleted
		public abstract class isCompleted : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsCompleted;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Completed", Enabled = false, Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Boolean? IsCompleted
		{
			get
			{
				return this._IsCompleted;
			}
			set
			{
				this._IsCompleted = value;
			}
		}
		#endregion
		#region IsCancelled
		public abstract class isCancelled : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsCancelled;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Cancelled", Enabled = false, Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Boolean? IsCancelled
		{
			get
			{
				return this._IsCancelled;
			}
			set
			{
				this._IsCancelled = value;
			}
		}
		#endregion
		
		#region VisibleInGL
		public abstract class visibleInGL : PX.Data.IBqlField
		{
		}
		protected Boolean? _VisibleInGL;
		[PXDBBool()]
		[PXDefault(typeof(Search<PMProject.visibleInGL, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXUIField(DisplayName = "GL")]
		public virtual Boolean? VisibleInGL
		{
			get
			{
				return this._VisibleInGL;
			}
			set
			{
				this._VisibleInGL = value;
			}
		}
		#endregion
		#region VisibleInAP
		public abstract class visibleInAP : PX.Data.IBqlField
		{
		}
		protected Boolean? _VisibleInAP;
		[PXDBBool()]
		[PXDefault(typeof(Search<PMProject.visibleInAP, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXUIField(DisplayName = "AP")]
		public virtual Boolean? VisibleInAP
		{
			get
			{
				return this._VisibleInAP;
			}
			set
			{
				this._VisibleInAP = value;
			}
		}
		#endregion
		#region VisibleInAR
		public abstract class visibleInAR : PX.Data.IBqlField
		{
		}
		protected Boolean? _VisibleInAR;
		[PXDBBool()]
		[PXDefault(typeof(Search<PMProject.visibleInAR, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXUIField(DisplayName = "AR")]
		public virtual Boolean? VisibleInAR
		{
			get
			{
				return this._VisibleInAR;
			}
			set
			{
				this._VisibleInAR = value;
			}
		}
		#endregion
		#region VisibleInSO
		public abstract class visibleInSO : PX.Data.IBqlField
		{
		}
		protected Boolean? _VisibleInSO;
		[PXDBBool()]
		[PXDefault(typeof(Search<PMProject.visibleInSO, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXUIField(DisplayName = "SO")]
		public virtual Boolean? VisibleInSO
		{
			get
			{
				return this._VisibleInSO;
			}
			set
			{
				this._VisibleInSO = value;
			}
		}
		#endregion
		#region VisibleInPO
		public abstract class visibleInPO : PX.Data.IBqlField
		{
		}
		protected Boolean? _VisibleInPO;
		[PXDBBool()]
		[PXDefault(typeof(Search<PMProject.visibleInPO, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXUIField(DisplayName = "PO")]
		public virtual Boolean? VisibleInPO
		{
			get
			{
				return this._VisibleInPO;
			}
			set
			{
				this._VisibleInPO = value;
			}
		}
		#endregion
		#region VisibleInEP
		public abstract class visibleInEP : PX.Data.IBqlField
		{
		}
		protected Boolean? _VisibleInEP;
		[PXDBBool()]
		[PXDefault(typeof(Search<PMProject.visibleInEP, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXUIField(DisplayName = "TE")]
		public virtual Boolean? VisibleInEP
		{
			get
			{
				return this._VisibleInEP;
			}
			set
			{
				this._VisibleInEP = value;
			}
		}
		#endregion
		#region VisibleInIN
		public abstract class visibleInIN : PX.Data.IBqlField
		{
		}
		protected Boolean? _VisibleInIN;
		[PXDBBool()]
		[PXDefault(typeof(Search<PMProject.visibleInIN, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXUIField(DisplayName = "IN")]
		public virtual Boolean? VisibleInIN
		{
			get
			{
				return this._VisibleInIN;
			}
			set
			{
				this._VisibleInIN = value;
			}
		}
		#endregion
		#region VisibleInCA
		public abstract class visibleInCA : PX.Data.IBqlField
		{
		}
		protected Boolean? _VisibleInCA;
		[PXDBBool()]
		[PXDefault(typeof(Search<PMProject.visibleInCA, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXUIField(DisplayName = "CA")]
		public virtual Boolean? VisibleInCA
		{
			get
			{
				return this._VisibleInCA;
			}
			set
			{
				this._VisibleInCA = value;
			}
		}
		#endregion
		#region VisibleInCR
		public abstract class visibleInCR : PX.Data.IBqlField
		{
		}
		protected Boolean? _VisibleInCR;
		[PXDBBool()]
		[PXDefault(typeof(Search<PMProject.visibleInCR, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXUIField(DisplayName = "CR")]
		public virtual Boolean? VisibleInCR
		{
			get
			{
				return this._VisibleInCR;
			}
			set
			{
				this._VisibleInCR = value;
			}
		}
		#endregion
        #region AutoIncludeInPrj
        public abstract class autoIncludeInPrj : IBqlField
        {
        }
        protected bool? _AutoIncludeInPrj;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Automatically Include in Project")]
        public virtual bool? AutoIncludeInPrj
        {
            get
            {
                return _AutoIncludeInPrj;
            }
            set
            {
                _AutoIncludeInPrj = value;
            }
        }
        #endregion
		#region LineCtr
		public abstract class lineCtr : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
		[PXDefault(0)]
		public virtual int? LineCtr { get; set; }
		#endregion
        
		

		#region Attributes
		public abstract class attributes : IBqlField
		{
		}
		[CRAttributesField(typeof(CSAnswerType.projectTaskAnswerType),
			typeof(PMTask.taskID),
			typeof(PMTask.classID))]
		public virtual string[] Attributes { get; set; }

		public virtual Int32? ID 
		{
			get { return TaskID; } 
		}
		#region ClassID

		public abstract class classID : IBqlField
		{
		}
		[PXString(20)]
		public virtual string ClassID
		{
			get { return GroupTypes.Task; }
		}
		#endregion
		#region EntityType
		public virtual string EntityType
		{
			get { return CSAnswerType.ProjectTask; }
		}
		#endregion

		#endregion

		#region templateID
		public abstract class templateID : IBqlField
		{
		}
		[PXInt]
		public virtual int? TemplateID { get; set; }
		#endregion

        #region System Columns
        #region NoteID
        public abstract class noteID : PX.Data.IBqlField
		{
		}
		protected Int64? _NoteID;
		[PXNote(DescriptionField = typeof(PMTask.taskCD))]
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

	}

    public static class ProjectTaskStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Planned, Active, Canceled, Completed },
				new string[] { Messages.Planned, Messages.Active, Messages.Canceled, Messages.Completed }) { ; }
		}
		public const string Planned = "D";
		public const string Active = "A";
		public const string Canceled = "C";
		public const string Completed = "F";
		
	}

    public static class PMBillingOption
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { OnBilling, OnTaskCompletion, OnProjectCompetion },
                new string[] { Messages.OnBilling, Messages.OnTaskCompletion, Messages.OnProjectCompetion }) { ; }
        }
        public const string OnBilling = "B";
        public const string OnTaskCompletion = "T";
        public const string OnProjectCompetion = "P";
    }
}
