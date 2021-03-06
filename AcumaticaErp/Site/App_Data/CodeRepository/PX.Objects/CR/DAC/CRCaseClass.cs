using PX.Objects.IN;
using PX.SM;

namespace PX.Objects.CR
{
	using System;
	using PX.Data;
	using PX.Objects.CS;
	using PX.Objects.CT;

	[PXCacheName(Messages.CaseClass)]
	[PXPrimaryGraph(typeof(CRCaseClassMaint))]
	[System.SerializableAttribute()]
	public partial class CRCaseClass : PX.Data.IBqlTable
	{
		#region CaseClassID
		public abstract class caseClassID : PX.Data.IBqlField
		{
		}
		protected String _CaseClassID;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Case Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(CRCaseClass.caseClassID), DescriptionField = typeof(CRCaseClass.description))]
		public virtual String CaseClassID
		{
			get
			{
				return this._CaseClassID;
			}
			set
			{
				this._CaseClassID = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.IBqlField
		{
		}
		protected String _Description;
		[PXDBString(255, IsUnicode = true)]
		[PXDefault()]
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
		#region Status
		public abstract class status : PX.Data.IBqlField
		{
		}
		protected String _Status;
		[PXDBString(1, IsFixed = true)]		
		[PXDefault(CRCaseStatusesAttribute._NEW)]
		[PXUIField(DisplayName = "Default Status")]
		[CRCaseStatuses(BqlField = typeof(CRCase.status))]
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
		#region IsBillable
		public abstract class isBillable : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsBillable;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Billable")]
		[PXUIEnabled(typeof(Switch<Case<Where<perItemBilling, Equal<BillingTypeListAttribute.perCase>>, True>, False>))]
		[PXFormula(typeof(Switch<Case<Where<perItemBilling, Equal<BillingTypeListAttribute.perActivity>>, False>>))]
		public virtual Boolean? IsBillable
		{
			get
			{
				return this._IsBillable;
			}
			set
			{
				this._IsBillable = value;
			}
		}
		#endregion
		#region AllowOverrideBillable
		public abstract class allowOverrideBillable : PX.Data.IBqlField
		{
		}
		protected Boolean? _AllowOverrideBillable;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Enable Billable Option Override")]
		[PXUIEnabled(typeof(Switch<Case<Where<perItemBilling, Equal<BillingTypeListAttribute.perCase>>, True>, False>))]
		[PXFormula(typeof(Switch<Case<Where<perItemBilling, Equal<BillingTypeListAttribute.perActivity>>, False>>))]
		public virtual Boolean? AllowOverrideBillable
		{
			get
			{
				return this._AllowOverrideBillable;
			}
			set
			{
				this._AllowOverrideBillable = value;
			}
		}
		#endregion
		#region RequireCustomer
		public abstract class requireCustomer : PX.Data.IBqlField
		{
		}
		protected Boolean? _RequireCustomer;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Customer")]
		[PXUIEnabled(typeof(Switch<Case<Where<isBillable, Equal<True>, Or<allowOverrideBillable, Equal<True>>>, False>, True>))]
		[PXFormula(typeof(Switch<Case<Where<isBillable, Equal<True>, Or<allowOverrideBillable, Equal<True>>>, True>, Current<requireCustomer>>))]
		public virtual Boolean? RequireCustomer
		{
			get
			{
				return this._RequireCustomer;
			}
			set
			{
				this._RequireCustomer = value;
			}
		}
		#endregion
		#region RequireContact
		public abstract class requireContact : PX.Data.IBqlField
		{
		}
		protected Boolean? _RequireContact;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Contact")]
		public virtual Boolean? RequireContact
		{
			get
			{
				return this._RequireContact;
			}
			set
			{
				this._RequireContact = value;
			}
		}
		#endregion
		#region RequireContract
		public abstract class requireContract : PX.Data.IBqlField
		{
		}
		protected Boolean? _RequireContract;
		[PXDBBool()]
		[PXDefault(false)]
		[PXFormula(typeof(Switch<Case<Where<perItemBilling, Equal<BillingTypeListAttribute.perActivity>>, True>, Current<requireContract>>))]
		[PXUIEnabled(typeof(Switch<Case<Where<perItemBilling, Equal<BillingTypeListAttribute.perActivity>>, False>, True>))]
		[PXUIField(DisplayName = "Require Contract")]
		public virtual Boolean? RequireContract
		{
			get
			{
				return this._RequireContract;
			}
			set
			{
				this._RequireContract = value;
			}
		}
		#endregion
        #region PerItemBilling
        public abstract class perItemBilling : PX.Data.IBqlField
        {
        }
		protected Int32? _PerItemBilling;
		[PXDBInt()]
		[BillingTypeList()]
        [PXDefault(0)]
		[PXUIField(DisplayName="Billing Mode")]
		public virtual Int32? PerItemBilling
        {
            get
            {
                return this._PerItemBilling;
            }
            set
            {
                this._PerItemBilling = value;
            }
        }
        #endregion
        #region LabourItemID
        public abstract class labourItemID : PX.Data.IBqlField
        {
        }
        protected Int32? _LabourItemID;
        [PXDBInt()]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Labor Item", Required = false)]
        [PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.itemType, Equal<INItemTypes.laborItem>, And<Match<Current<AccessInfo.userName>>>>>), typeof(InventoryItem.inventoryCD))]
		[PXUIRequired(typeof(Switch<Case<Where<perItemBilling, Equal<BillingTypeListAttribute.perCase>>, True>, False>))]
		[PXUIEnabled(typeof(Switch<Case<Where<perItemBilling, Equal<BillingTypeListAttribute.perCase>>, True>, False>))]
		[PXFormula(typeof(Switch<Case<Where<perItemBilling, Equal<BillingTypeListAttribute.perActivity>>, Null>, Current<labourItemID>>))]
		public virtual Int32? LabourItemID
        {
            get
            {
                return this._LabourItemID;
            }
            set
            {
                this._LabourItemID = value;
            }
        }
        #endregion
        #region OvertimeItemID
        public abstract class overtimeItemID : PX.Data.IBqlField
        {
        }
        protected Int32? _OvertimeItemID;
        [PXDBInt()]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Overtime Labor Item", Required = false)]
        [PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.itemType, Equal<INItemTypes.laborItem>, And<Match<Current<AccessInfo.userName>>>>>), typeof(InventoryItem.inventoryCD))]
		[PXUIRequired(typeof(Switch<Case<Where<perItemBilling, Equal<BillingTypeListAttribute.perCase>>, True>, False>))]
		[PXUIEnabled(typeof(Switch<Case<Where<perItemBilling, Equal<BillingTypeListAttribute.perCase>>, True>, False>))]
		[PXFormula(typeof(Switch<Case<Where<perItemBilling, Equal<BillingTypeListAttribute.perActivity>>, Null>, Current<overtimeItemID>>))]
		public virtual Int32? OvertimeItemID
        {
            get
            {
                return this._OvertimeItemID;
            }
            set
            {
                this._OvertimeItemID = value;
            }
        }
        #endregion



		#region DefaultEMailAccount
		public abstract class defaultEMailAccountID : PX.Data.IBqlField { }

		[PXSelector(typeof(EMailAccount.emailAccountID), typeof(EMailAccount.description), DescriptionField = typeof(EMailAccount.description))]
		[PXUIField(DisplayName = "Default Email Account")]
		[PXDBInt]
		public virtual int? DefaultEMailAccountID { get; set; }
		#endregion
		#region RoundingInMinutes
		public abstract class roundingInMinutes : PX.Data.IBqlField
		{
		}
		protected Int32? _RoundingInMinutes;
		[PXDBInt()]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Round Time by")]
		[EP.PXTimeList(5, 6)]
		public virtual Int32? RoundingInMinutes
		{
			get
			{
				return this._RoundingInMinutes;
			}
			set
			{
				this._RoundingInMinutes = value;
			}
		}
		#endregion

		#region MinBillTimeInMinutes
		public abstract class minBillTimeInMinutes : PX.Data.IBqlField
		{
		}
		protected Int32? _MinBillTimeInMinutes;
		[PXDBInt()]
		[PXDefault(0)]
		[EP.PXTimeList(5, 12)]
		[PXUIField(DisplayName = "Min Billable Time", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? MinBillTimeInMinutes
		{
			get
			{
				return this._MinBillTimeInMinutes;
			}
			set
			{
				this._MinBillTimeInMinutes = value;
			}
		}
		#endregion

		#region ReopenCaseTimeInDays
		public abstract class reopenCaseTimeInDays : PX.Data.IBqlField
		{
		}
		protected Int32? _ReopenCaseTimeInDays;
		[PXDBInt(MinValue = 0, MaxValue = 1440)]
		[PXUIField(DisplayName = "Allowed Period to Reopen Case (in Days)")]
		public virtual Int32? ReopenCaseTimeInDays
		{
			get
			{
				return this._ReopenCaseTimeInDays;
			}
			set
			{
				this._ReopenCaseTimeInDays = value;
			}
		}
		#endregion

		#region IsInternal

		public abstract class isInternal : PX.Data.IBqlField { }
		protected Boolean? _IsInternal;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Internal", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Boolean? IsInternal
		{
			get
			{
				return this._IsInternal;
			}
			set
			{
				this._IsInternal = value;
			}
		}

		#endregion

		#region NoteID
		public abstract class noteID : IBqlField { }

		[PXNote]
		public virtual Int64? NoteID { get; set; }
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

	public class BillingTypeListAttribute : PXIntListAttribute
	{
		public const int PerCase = 0;
		public const int PerActivity = 1;

		public BillingTypeListAttribute()
			: base(new[] { PerCase, PerActivity }, new[] { Messages.PerCase, Messages.PerActivity })
		{
		}

		public class perCase : Constant<int>
		{
			public perCase() : base(PerCase) { }
		}

		public class perActivity : Constant<int>
		{
			public perActivity() : base(PerActivity) { }
		}

	}
}
