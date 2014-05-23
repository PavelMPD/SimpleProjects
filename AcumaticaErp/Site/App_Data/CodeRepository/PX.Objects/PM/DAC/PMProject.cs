using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CT;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.CM;

namespace PX.Objects.PM
{
	[PXPrimaryGraph(typeof(ProjectEntry))]
	[PXCacheName(Messages.Project)]
    [Serializable]
	[PXEMailSource]
	public partial class PMProject : Contract , IAssign
	{
		public class ProjectBaseType : Constant<string>
		{
			public const string Project = "P";
			public ProjectBaseType() : base(Project) { ;}
		}
		
		#region ContractID
		public new abstract class contractID : PX.Data.IBqlField
		{
		}
		[PXDBIdentity()]
		[PXSelector(typeof(Search<PMProject.contractID, Where<PMProject.baseType, Equal<PMProject.ProjectBaseType>, And<PMProject.isTemplate, NotEqual<True>>>>))]
		[PXUIField(DisplayName = "Project ID")]
		public override Int32? ContractID
		{
			get
			{
				return this._ContractID;
			}
			set
			{
				this._ContractID = value;
			}
		}
		#endregion
		#region ContractCD
		public new abstract class contractCD : PX.Data.IBqlField
		{
		}
		[PXDimensionSelector(ProjectAttribute.DimensionName,
			typeof(Search<PMProject.contractCD, Where<PMProject.baseType, Equal<PMProject.ProjectBaseType>, And<PMProject.nonProject, Equal<False>, And<PMProject.isTemplate, NotEqual<True>>>>>),
			typeof(PMProject.contractCD),
			typeof(PMProject.contractCD), typeof(PMProject.customerID), typeof(PMProject.description), typeof(PMProject.status), DescriptionField = typeof(PMProject.description), Filterable = true)]
		[PXDBString(IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Project ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public override String ContractCD
		{
			get
			{
				return this._ContractCD;
			}
			set
			{
				this._ContractCD = value;
			}
		}
		#endregion
		#region Description
		public new abstract class description : PX.Data.IBqlField
		{
		}
		[PXDBString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public override String Description
		{
			get
			{
				return _Description;
			}
			set
			{
				_Description = value;
			}
		}
		#endregion
		#region OriginalContractID
		public new abstract class originalContractID : PX.Data.IBqlField
		{
		}

		[PXDBInt()]
		public override Int32? OriginalContractID
		{
			get
			{
				return this._OriginalContractID;
			}
			set
			{
				this._OriginalContractID = value;
			}
		}
		#endregion
		#region MasterContractID
		public new abstract class masterContractID : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
		public override Int32? MasterContractID
		{
			get
			{
				return this._MasterContractID;
			}
			set
			{
				this._MasterContractID = value;
			}
		}
		#endregion
		#region CaseItemID
		public new abstract class caseItemID : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
		public override Int32? CaseItemID
		{
			get
			{
				return this._CaseItemID;
			}
			set
			{
				this._CaseItemID = value;
			}
		}
		#endregion
		#region BaseType
		public new abstract class baseType : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault(ProjectBaseType.Project)]
		public override String BaseType
		{
			get
			{
				return this._BaseType;
			}
			set
			{
				this._BaseType = value;
			}
		}
		#endregion
		#region Type
		public new abstract class type : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[ContractType.List()]
		[PXDefault(ContractType.Renewable)]
		public override String Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}
		#endregion
		#region CustomerID
		public new abstract class customerID : PX.Data.IBqlField
		{
		}
		//[PXDefault]
		[Customer(DescriptionField = typeof(Customer.acctName))]
		public override Int32? CustomerID
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
		public new abstract class locationID : PX.Data.IBqlField
		{
		}
        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<Contract.customerID>>>), DisplayName = "Customer Location", DescriptionField = typeof(Location.descr))]
		[PXDefault(typeof(Search<BAccount.defLocationID, Where<BAccount.bAccountID, Equal<Current<Contract.customerID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public override Int32? LocationID
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
		#region DefaultAccountID
		public new abstract class defaultAccountID : PX.Data.IBqlField
		{
		}
		#endregion
		#region DefaultSubID
		public new abstract class defaultSubID : PX.Data.IBqlField
		{
		}
		[PXDefault(typeof(Search<Location.cSalesSubID, Where<Location.locationID, Equal<Current<PMProject.locationID>>>>))]
		[SubAccount(DisplayName = "Default Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public override Int32? DefaultSubID
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
        public new abstract class defaultAccrualAccountID : PX.Data.IBqlField
        {
        }
        #endregion
        #region DefaultAccrualSubID
        public new abstract class defaultAccrualSubID : PX.Data.IBqlField
        {
        }
        [SubAccount(DisplayName = "Default Accrual Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
        public override Int32? DefaultAccrualSubID
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
        #region BillingID
		public new abstract class billingID : PX.Data.IBqlField
		{
		}
        [PXSelector(typeof(Search<PMBilling.billingID, Where<PMBilling.isActive, Equal<True>>>))]
		[PXUIField(DisplayName="Billing Rule")]
		[PXDBString(PMBilling.billingID.Length, IsUnicode = true)]
		public override String BillingID
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
		public new abstract class allocationID : PX.Data.IBqlField
		{
		}
		[PXSelector(typeof(Search<PMAllocation.allocationID, Where<PMAllocation.isActive, Equal<True>>>))]
		[PXUIField(DisplayName = "Allocation Rule")]
		[PXDBString(PMAllocation.allocationID.Length, IsUnicode = true)]
		public override String AllocationID
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
		#region ApproverID
		public new abstract class approverID : PX.Data.IBqlField
		{
		}
		[PXDBInt]
		[PXEPEmployeeSelector]
		[PXUIField(DisplayName = "Project Manager", Visibility = PXUIVisibility.SelectorVisible)]
		public override Int32? ApproverID
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
		#region WorkgroupID
		public new abstract class workgroupID : PX.Data.IBqlField
		{
		}
		#endregion
		#region OwnerID
		public new abstract class ownerID : IBqlField
		{
		}
		#endregion
        #region RateTableID
        public new abstract class rateTableID : PX.Data.IBqlField
        {}
        [PXDBString(PMRateTable.rateTableID.Length, IsUnicode = true)]
        [PXUIField(DisplayName = "Rate Table")]
        [PXSelector(typeof(PMRateTable.rateTableID))]
        public override String RateTableID
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
		#region TemplateID
		public new abstract class templateID : PX.Data.IBqlField
		{
		}
        [Project(typeof(Where<PMProject.isTemplate, Equal<True>, And<PMProject.isActive, Equal<True>>>), DisplayName = "Template ID")]
        public override Int32? TemplateID
        {
			get
			{
				return this._TemplateID;
			}
			set
			{
				this._TemplateID = value;
			}
		}
		#endregion
		#region Status
		public new abstract class status : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[ProjectStatus.List()]
		[PXDefault(ProjectStatus.Planned)]
		[PXUIField(DisplayName = "Status", Required = true, Visibility = PXUIVisibility.SelectorVisible)]
		public override String Status
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
		#region Duration
		public new abstract class duration : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
		public override Int32? Duration
		{
			get
			{
				return this._Duration;
			}
			set
			{
				this._Duration = value;
			}
		}
		#endregion
		#region DurationType
		public new abstract class durationType : PX.Data.IBqlField
		{
		}
		
		[PXDBString(1, IsFixed = true)]
		public override string DurationType
		{
			get
			{
				return this._DurationType;
			}
			set
			{
				this._DurationType = value;
			}
		}
		#endregion
		#region StartDate
		public new abstract class startDate : PX.Data.IBqlField
		{
		}
		[PXDefault]
		[PXDBDate()]
		[PXUIField(DisplayName = "Start Date")]
		public override DateTime? StartDate
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
		#region ExpireDate
		public new abstract class expireDate : PX.Data.IBqlField
		{
		}
		[PXDBDate()]
		[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.SelectorVisible)]
		public override DateTime? ExpireDate
		{
			get
			{
				return this._ExpireDate;
			}
			set
			{
				this._ExpireDate = value;
			}
		}
		#endregion
		#region GracePeriod
		public new abstract class gracePeriod : PX.Data.IBqlField
		{
		}
		[PXDBInt(MinValue = 0, MaxValue = 365)]
		[PXDefault(0)]
		public override Int32? GracePeriod
		{
			get
			{
				return this._GracePeriod;
			}
			set
			{
				this._GracePeriod = value;
			}
		}
		#endregion
		#region AutoRenew
		public new abstract class autoRenew : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(false)]
		public override Boolean? AutoRenew
		{
			get
			{
				return this._AutoRenew;
			}
			set
			{
				this._AutoRenew = value;
			}
		}
		#endregion
		#region AutoRenewDays
		public new abstract class autoRenewDays : PX.Data.IBqlField
		{
		}
		[PXDBInt(MinValue = 0, MaxValue = 365)]
		[PXDefault(0)]
		public override Int32? AutoRenewDays
		{
			get
			{
				return this._AutoRenewDays;
			}
			set
			{
				this._AutoRenewDays = value;
			}
		}
		#endregion
		#region IsTemplate
		public new abstract class isTemplate : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(false)]
		public override Boolean? IsTemplate
		{
			get
			{
				return this._IsTemplate;
			}
			set
			{
				this._IsTemplate = value;
			}
		}
		#endregion

		#region RestrictToEmployeeList
		public new abstract class restrictToEmployeeList : PX.Data.IBqlField
		{
		}
		#endregion
		#region RestrictToResourceList
		public new abstract class restrictToResourceList : PX.Data.IBqlField
		{
		}
		#endregion

		#region DetailedBilling
		public new abstract class detailedBilling : PX.Data.IBqlField
		{
		}
		
		[PXDBInt()]
		[PXDefault(Contract.detailedBilling.Summary)]
		public override Int32? DetailedBilling
		{
			get
			{
				return this._DetailedBilling;
			}
			set
			{
				this._DetailedBilling = value;
			}
		}
		#endregion
		#region AllowOverride
		public new abstract class allowOverride : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(false)]
		public override Boolean? AllowOverride
		{
			get
			{
				return this._AllowOverride;
			}
			set
			{
				this._AllowOverride = value;
			}
		}
		#endregion
		#region RefreshOnRenewal
		public new abstract class refreshOnRenewal : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(false)]
		public override Boolean? RefreshOnRenewal
		{
			get
			{
				return this._RefreshOnRenewal;
			}
			set
			{
				this._RefreshOnRenewal = value;
			}
		}
		#endregion
		#region IsContinuous
		public new abstract class isContinuous : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(false)]
		public override Boolean? IsContinuous
		{
			get
			{
				return this._IsContinuous;
			}
			set
			{
				this._IsContinuous = value;
			}
		}
		#endregion
        
        #region Asset
        public abstract class asset : PX.Data.IBqlField
        {
        }
        protected Decimal? _Asset;
        [PXBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck=PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Assets", Enabled = false)]
        public virtual Decimal? Asset
        {
            get
            {
                return this._Asset;
            }
            set
            {
                this._Asset = value;
            }
        }
        #endregion
        #region Liability
        public abstract class liability : PX.Data.IBqlField
        {
        }
        protected Decimal? _Liability;
        [PXBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Liabilities", Enabled = false)]
        public virtual Decimal? Liability
        {
            get
            {
                return this._Liability;
            }
            set
            {
                this._Liability = value;
            }
        }
        #endregion
        #region Income
        public abstract class income : PX.Data.IBqlField
        {
        }
        protected Decimal? _Income;
        [PXBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Income", Enabled = false)]
        public virtual Decimal? Income
        {
            get
            {
                return this._Income;
            }
            set
            {
                this._Income = value;
            }
        }
        #endregion
        #region Expense
        public abstract class expense : PX.Data.IBqlField
        {
        }
        protected Decimal? _Expense;
        [PXBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Expenses", Enabled = false)]
        public virtual Decimal? Expense
        {
            get
            {
                return this._Expense;
            }
            set
            {
                this._Expense = value;
            }
        }
        #endregion

		#region IsActive
		public new abstract class isActive : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Active", Enabled=false, Visible=false, Visibility=PXUIVisibility.Visible)]
		public override Boolean? IsActive
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
		public new abstract class isCompleted : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Completed", Enabled = false, Visible = false, Visibility = PXUIVisibility.Visible)]
		public override Boolean? IsCompleted
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
        #region AutoAllocate
        public new abstract class autoAllocate : PX.Data.IBqlField
        {
        }
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Run Allocation on PM Transaction Release")]
        public override Boolean? AutoAllocate
        {
            get
            {
                return this._AutoAllocate;
            }
            set
            {
                this._AutoAllocate = value;
            }
        }
        #endregion

		#region VisibleInGL
		public new abstract class visibleInGL : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInGL>))]
		[PXUIField(DisplayName = "GL")]
		public override Boolean? VisibleInGL
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
		public new abstract class visibleInAP : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInAP>))]
		[PXUIField(DisplayName = "AP")]
		public override Boolean? VisibleInAP
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
		public new abstract class visibleInAR : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInAR>))]
		[PXUIField(DisplayName = "AR")]
		public override Boolean? VisibleInAR
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
		public new abstract class visibleInSO : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInSO>))]
		[PXUIField(DisplayName = "SO")]
		public override Boolean? VisibleInSO
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
		public new abstract class visibleInPO : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInPO>))]
		[PXUIField(DisplayName = "PO")]
		public override Boolean? VisibleInPO
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
		public new abstract class visibleInEP : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInEP>))]
		[PXUIField(DisplayName = "TE")]
		public override Boolean? VisibleInEP
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
		public new abstract class visibleInIN : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInIN>))]
		[PXUIField(DisplayName = "IN")]
		public override Boolean? VisibleInIN
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
		public new abstract class visibleInCA : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInCA>))]
		[PXUIField(DisplayName = "CA")]
		public override Boolean? VisibleInCA
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
		public new abstract class visibleInCR : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInCR>))]
		[PXUIField(DisplayName = "CR")]
		public override Boolean? VisibleInCR
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
		#region NonProject
		public new abstract class nonProject : PX.Data.IBqlField
		{
		}
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Is Global", Visibility = PXUIVisibility.Visible, Visible = false)]
        public override Boolean? NonProject
        {
            get
            {
                return this._NonProject;
            }
            set
            {
                this._NonProject = value;
            }
        }
        #endregion
		#region NoteID
		public new abstract class noteID : IBqlField{}
		[PXNote(DescriptionField = typeof(PMProject.contractCD))]
		public override Int64? NoteID
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


		#region ServiceActivate
		public new abstract class serviceActivate : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public override Boolean? ServiceActivate
		{
			get
			{
				return this._ServiceActivate;
			}
			set
			{
				this._ServiceActivate = value;
			}
		}
		#endregion

		#region Attributes
		public new abstract class attributes : IBqlField
		{
		}
		[CRAttributesField(typeof(CSAnswerType.projectAnswerType),
			typeof(Contract.contractID),
			typeof(Contract.classID))]
		public override string[] Attributes { get; set; }

		#region ClassID
		public new abstract class classID : IBqlField
		{
		}
		[PXString(20)]
		public override string ClassID
		{
			get { return GroupTypes.Project; }
		}

		#endregion
		#region EntityType
		public override string EntityType
		{
			get { return CSAnswerType.Project; }
		}
		#endregion

	
		#endregion
		
	}


	public static class ProjectStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Planned, Active, Completed, Cancelled, OnHold, PendingApproval },
				new string[] { Messages.Planned, Messages.Active, Messages.Completed, Messages.Canceled, Messages.Suspend, Messages.PendingApproval }) { ; }
		}

        public class TemplStatusListAttribute : PXStringListAttribute
        {
            public TemplStatusListAttribute()
                : base(
				new string[] { Active, OnHold },
                new string[] { Messages.Active, Messages.OnHold }) { ; }
        }

		public const string Planned = ContractStatus.Draft;
		public const string Active = ContractStatus.Activated;
		public const string Completed = ContractStatus.Completed;
        public const string OnHold = ContractStatus.Expired;
		public const string Cancelled = ContractStatus.Canceled;
		public const string PendingApproval = ContractStatus.InApproval;

	}
}
