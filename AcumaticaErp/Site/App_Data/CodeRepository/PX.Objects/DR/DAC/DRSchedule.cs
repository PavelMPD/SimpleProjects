namespace PX.Objects.DR
{
	using System;
	using PX.Data;
	using PX.Objects.GL;
	using PX.Objects.CR;
	using PX.Objects.CS;
	using PX.Objects.AR;
	using PX.Objects.CT;
	using System.Diagnostics;
	using PX.Objects.AP;
	using PX.Objects.PM;

	[PXPrimaryGraph(typeof(DraftScheduleMaint))]
	[System.SerializableAttribute()]
	[DebuggerDisplay("SheduleID={ScheduleID} DocType={DocType} RefNbr={RefNbr}")]	
	[PXCacheName(Messages.Schedule)]
	public partial class DRSchedule : PX.Data.IBqlTable
	{
		#region ScheduleID
		public abstract class scheduleID : PX.Data.IBqlField
		{
		}
		protected Int32? _ScheduleID;
		[PXParent(typeof(Select<ARTran, Where<ARTran.tranType, Equal<Current<DRSchedule.docType>>, And<ARTran.refNbr, Equal<Current<DRSchedule.refNbr>>, And<ARTran.lineNbr, Equal<Current<DRSchedule.lineNbr>>>>>>))]
		[PXParent(typeof(Select<APTran, Where<APTran.tranType, Equal<Current<DRSchedule.docType>>, And<APTran.refNbr, Equal<Current<DRSchedule.refNbr>>, And<APTran.lineNbr, Equal<Current<DRSchedule.lineNbr>>>>>>))]
		[PXSelector(typeof(Search<DRSchedule.scheduleID>), Filterable = true)]
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PX.Data.EP.PXFieldDescription]
		public virtual Int32? ScheduleID
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

		#region ProxyScheduleID
		public abstract class proxyScheduleID : PX.Data.IBqlField
		{
		}
		[PXInt]
		public virtual Int32? ProxyScheduleID
		{
			get
			{
				return this._ScheduleID < 0 ? null : this._ScheduleID;
			}
		}
		#endregion

		#region Module
		public abstract class module : PX.Data.IBqlField
		{
		}
		protected String _Module;
		[PXDBString(2, IsFixed = true)]
		[PXDefault(BatchModule.AR)]
		[PXUIField(DisplayName = "Module")]
		[PX.Data.EP.PXFieldDescription]
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
		#region DocType
		public abstract class docType : PX.Data.IBqlField
		{
		}
		protected String _DocType;
		[PXDefault(ARDocType.Invoice)]
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Doc. Type", Enabled = true)]
		[PX.Data.EP.PXFieldDescription]
		public virtual String DocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
				this._DocType = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.IBqlField
		{
		}
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[DRDocumentSelector(typeof(DRSchedule.module), typeof(DRSchedule.docType))]
		[PX.Data.EP.PXFieldDescription]
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
		#region LineNbr
		public abstract class lineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _LineNbr;
		[PXDBInt()]
		[PXUIField(DisplayName = "LineNbr")]
		[DRLineSelector(typeof(DRSchedule.module), typeof(DRSchedule.docType), typeof(DRSchedule.refNbr))]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region DocDate
		public abstract class docDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _DocDate;
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = "Doc. Date", Enabled = false)]
		public virtual DateTime? DocDate
		{
			get
			{
				return this._DocDate;
			}
			set
			{
				this._DocDate = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		protected String _FinPeriodID;
		[PXDefault]
		[AROpenPeriod(typeof(DRSchedule.docDate))]
		[PXUIField(DisplayName = "Fin. Period", Enabled = false)]
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
		#region BAccountID
		public abstract class bAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _BAccountID;
		[PXDBInt()]
		[PXSelector(typeof(Search<BAccountR.bAccountID, Where<BAccountR.type, Equal<Current<DRSchedule.bAccountType>>>>), new Type[] { typeof(BAccountR.acctCD), typeof(BAccountR.acctName), typeof(BAccountR.type) }, SubstituteKey = typeof(BAccountR.acctCD), DescriptionField=typeof(BAccountR.acctName))]
		[PXUIField(DisplayName="Customer/Vendor", Visibility=PXUIVisibility.SelectorVisible)]
		public virtual Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		#region BAccountLocID
		public abstract class bAccountLocID : PX.Data.IBqlField
		{
		}
		protected Int32? _BAccountLocID;
		[PXDefault(typeof(Search<BAccountR.defLocationID, Where<BAccountR.bAccountID, Equal<Current<DRSchedule.bAccountID>>>>))]
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<DRSchedule.bAccountID>>>))]
		public virtual Int32? BAccountLocID
		{
			get
			{
				return this._BAccountLocID;
			}
			set
			{
				this._BAccountLocID = value;
			}
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.IBqlField
		{
		}
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Transaction Descr.", Visibility = PXUIVisibility.Visible)]
		public virtual String TranDesc
		{
			get
			{
				return this._TranDesc;
			}
			set
			{
				this._TranDesc = value;
			}
		}
		#endregion
		#region IsCustom
		public abstract class isCustom : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsCustom;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is Custom", Enabled = false)]
		public virtual Boolean? IsCustom
		{
			get
			{
				return this._IsCustom;
			}
			set
			{
				this._IsCustom = value;
			}
		}
		#endregion
		#region IsDraft
		public abstract class isDraft : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsDraft;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is Draft", Visible = false)]
		public virtual Boolean? IsDraft
		{
			get
			{
				return this._IsDraft;
			}
			set
			{
				this._IsDraft = value;
			}
		}
		#endregion
		#region ProjectID
		public abstract class projectID : IBqlField { }
		[ProjectDefault]
		[PXDimensionSelector(ProjectAttribute.DimensionName,
			typeof(Search<PMProject.contractID, Where<PMProject.isTemplate, NotEqual<True>>>),
			typeof(PMProject.contractCD),
			typeof(PMProject.contractCD), typeof(PMProject.customerID), typeof(PMProject.description), typeof(PMProject.status), DescriptionField = typeof(PMProject.description))]
		[PXDBInt]
		[PXUIField(DisplayName = "Project", Visibility = PXUIVisibility.Visible)]
		public virtual Int32? ProjectID { get; set; }
		#endregion
		#region TaskID
		public abstract class taskID : IBqlField { }

		[ProjectTask(typeof(DRSchedule.projectID), DisplayName = "Project Task", AllowNullIfContract=true)]
		public virtual Int32? TaskID { get; set; }
		#endregion

		#region DocumentType
		public abstract class documentType : PX.Data.IBqlField
		{
		}
		protected String _DocumentType;
		[PXString(3, IsFixed = true)]
		[DRScheduleDocumentType.List]
		[PXUIField(DisplayName = "Doc. Type", Enabled = false, Required = true)]
		public virtual String DocumentType
		{
			get
			{
				return this._DocumentType;
			}
			set
			{
				this._DocumentType = value;
			}
		}
		#endregion
		#region BAccountType
		public abstract class bAccountType : PX.Data.IBqlField
		{
		}
		protected String _BAccountType;
		[PXDefault(CR.BAccountType.CustomerType, PersistingCheck=PXPersistingCheck.Nothing)]
		[PXString(2, IsFixed = true)]
		[PXStringList(new string[] { CR.BAccountType.VendorType, CR.BAccountType.CustomerType },
				new string[] { CR.Messages.VendorType, CR.Messages.CustomerType })]
		public virtual String BAccountType
		{
			get
			{
				return this._BAccountType;
			}
			set
			{
				this._BAccountType = value;
			}
		}
		#endregion
		#region OrigLineAmt
		public abstract class origLineAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _OrigLineAmt;
		[PXUIField(DisplayName="Original Line Amt.", Enabled=false)]
		[PXDecimal(4)]
		public virtual Decimal? OrigLineAmt
		{
			get
			{
				return this._OrigLineAmt;
			}
			set
			{
				this._OrigLineAmt = value;
			}
		}
		#endregion
		#region DocumentTypeEx
		public abstract class documentTypeEx : PX.Data.IBqlField
		{
		}
		[PXString(3, IsFixed = true)]
		[DRScheduleDocumentType.List]
		[PXUIField(DisplayName = "Doc. Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String DocumentTypeEx
		{
			[PXDependsOnFields(typeof(module), typeof(docType))]
			get
			{
				return DRScheduleDocumentType.BuildDocumentType(Module, DocType);
			}
		}
		#endregion

		#region NoteID

		public abstract class noteID : IBqlField { }

		[PXNote(DescriptionField = typeof(DRSchedule.scheduleID))]
		public virtual Int64? NoteID { get; set; }

		#endregion

		#region System Columns
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

}
