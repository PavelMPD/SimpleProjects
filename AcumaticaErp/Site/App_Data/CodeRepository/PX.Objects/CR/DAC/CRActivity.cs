using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.SM;
using PX.TM;
using PX.Objects.CT;
using PX.Web.UI;

namespace PX.Objects.CR
{
	[Serializable]
	[EPActivityPrimaryGraph]
	[PXCacheName(Messages.CRActivity)]
	public partial class EPActivity : IBqlTable
	{
		#region Selected
		public abstract class selected : IBqlField { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Service)]
		public virtual bool? Selected { get; set; }
		#endregion

		#region TaskID
		public abstract class taskID : IBqlField
		{
		}
		protected int? _TaskID;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXSelector(typeof(EPActivity.taskID),
			new[] { typeof(EPActivity.taskID), typeof(EPActivity.extID) })]
		[PXUnboundFormula(typeof(Switch<Case<Where<EPActivity.classID, Equal<CRActivityClass.email>, And<EPActivity.isIncome, Equal<True>, Or<EPActivity.incoming, Equal<True>>>>, True>, False>), typeof(LastActivity<CRActivityStatistics.lastIncomingActivityID, EPActivity.taskID>))]
		[PXUnboundFormula(typeof(Switch<Case<Where<EPActivity.classID, Equal<CRActivityClass.email>, And<EPActivity.isIncome, NotEqual<True>, Or<EPActivity.outgoing, Equal<True>>>>, True>, False>), typeof(LastActivity<CRActivityStatistics.lastOutgoingActivityID, EPActivity.taskID>))]
		[PXUnboundFormula(typeof(Switch<Case<Where<EPActivity.classID, Equal<CRActivityClass.email>, And<EPActivity.isIncome, Equal<True>, Or<EPActivity.incoming, Equal<True>>>>, True>, False>), typeof(LastActivity<CRActivityStatistics.lastIncomingActivityDate, EPActivity.createdDateTime>))]
		[PXUnboundFormula(typeof(Switch<Case<Where<EPActivity.classID, Equal<CRActivityClass.email>, And<EPActivity.isIncome, NotEqual<True>, Or<EPActivity.outgoing, Equal<True>>>>, True>, False>), typeof(LastActivity<CRActivityStatistics.lastOutgoingActivityDate, EPActivity.createdDateTime>))]
		public virtual int? TaskID
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

		#region ExtID
		public abstract class extID : IBqlField { }
		protected String _ExtID;
		[PXDBString(255)]
		[PXUIField(DisplayName = "External Ref ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String ExtID { set; get; }
		#endregion

		#region ClassID
		public abstract class classID : IBqlField {}

		protected int _ClassID;
		[PXDBInt]
		[CRActivityClass]
		[PXDefault(typeof(CRActivityClass.activity))]
		public virtual int? ClassID { get; set; }
		#endregion

		#region Type
		public abstract class type : IBqlField {}
		protected string _Type;
		[PXDBString(5, IsFixed = true, IsUnicode = false)]
		[PXUIField(DisplayName = "Type", Required = true)]
		[PXSelector(typeof(EPActivityType.type), DescriptionField = typeof(EPActivityType.description))]
		[PXRestrictor(typeof(Where<EPActivityType.active, Equal<True>>), Messages.InactiveActivityType, typeof(EPActivityType.type))]
		[PXRestrictor(typeof(Where<EPActivityType.isInternal, Equal<True>>), Messages.ExternalActivityType, typeof(EPActivityType.type))]
		[PXDefault(typeof(Search<EPActivityType.type, 
			Where<EPActivityType.isInternal, Equal<True>, 
			And<EPActivityType.isDefault, Equal<True>, 
			And<Current<EPActivity.classID>, Equal<CRActivityClass.activity>>>>>), 
			PersistingCheck = PXPersistingCheck.Nothing)] 
		public virtual String Type { get; set; }
		#endregion

		#region Incoming
		public abstract class incoming : IBqlField {}
		[PXDBBool]
		[PXFormula(typeof(Switch<Case<Where<EPActivity.type, IsNotNull>, Selector<EPActivity.type, EPActivityType.incoming>>, False>))]
		[PXUIField(DisplayName = "Incoming")]
		public virtual bool? Incoming { get; set; }
		#endregion
		#region Outgoing
		public abstract class outgoing : IBqlField { }
		[PXDBBool]
		[PXFormula(typeof(Switch<Case<Where<EPActivity.type, IsNotNull>, Selector<EPActivity.type, EPActivityType.outgoing>>, False>))]
		[PXUIField(DisplayName = "Outgoing")]
		public virtual bool? Outgoing { get; set; }
		#endregion


		#region ClassInfo
		public abstract class classInfo : IBqlField
		{
			public class emailResponse : Constant<string>
		{
				public emailResponse() : base(Messages.EmailResponseClassInfo) { }
			}
			}
		[PXString]
		[PXUIField(DisplayName = "Type", Enabled = false)]
		[PXFormula(typeof(Switch<Case<Where<EPActivity.classID, Equal<CRActivityClass.activity>, And<EPActivity.type, IsNotNull>>, Selector<EPActivity.type, EPActivityType.description>,
			Case<Where<EPActivity.classID, Equal<CRActivityClass.email>, And<EPActivity.isIncome, Equal<True>>>, classInfo.emailResponse>>,
			String<EPActivity.classID>>))]
		public virtual String ClassInfo { get; set; }
		#endregion		

		#region TrackTime
		public abstract class trackTime : IBqlField { }
		protected bool? _trackTime;
		[PXDBBool]
		[PXUIField(DisplayName = "Track Time")]
		[PXFormula(typeof(IIf<FeatureInstalled<FeaturesSet.timeReportingModule>, IsNull<Selector<EPActivity.type, EPActivityType.requireTimeByDefault>, False>, False>))]
		public virtual bool? TrackTime
		{
			get { return _trackTime; }
			set { _trackTime = value; }
		}
		#endregion

		#region EarningTypeID
		public abstract class earningTypeID : IBqlField { }
		[PXDBString(2, IsFixed = true, IsUnicode = false, InputMask = ">LL")]
		[PXDefault(typeof(Search<EPSetup.regularHoursType>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIRequired(typeof(trackTime))]
		[PXSelector(typeof(EPEarningType.typeCD), DescriptionField = typeof(EPEarningType.description))]
		[PXUIField(DisplayName = "Earning Type")]
		public virtual string EarningTypeID { get; set; }
		#endregion

		#region Body
		public abstract class body : PX.Data.IBqlField
		{
		}
		protected String _Body;
		[PXDBText(IsUnicode = true)]
		[PXUIField(DisplayName = "Activity Details")]
		public virtual String Body
		{
			get
			{
				return this._Body;
			}
			set
			{
				this._Body = value;
			}
		}
		#endregion

		#region ShowAsID
		public abstract class showAsID : IBqlField { }

		[PXDBInt]
		[PXUIField(DisplayName = "Show As")]
		[PXSelector(typeof(EPEventShowAs.showAsID), SubstituteKey = typeof(EPEventShowAs.description))]
		public virtual int? ShowAsID { get; set; }
		#endregion

		#region Priority
		public abstract class priority : PX.Data.IBqlField
		{
		}
		[PXDBInt]
		[PXUIField]
		[PXDefault(1, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXIntList(new int[] { 0, 1, 2 },
			new string[] { "Low", "Normal", "High" })]
		public virtual int? Priority { get; set; }
		#endregion

		#region GroupID
		public abstract class groupID : PX.Data.IBqlField
		{
		}
		protected Int32? _GroupID;
		[PXDBInt]
		[PXChildUpdatable(UpdateRequest = true)]
		[PXUIField(DisplayName = "Workgroup")]
		[PXSubordinateGroupSelector]
		public virtual Int32? GroupID
		{
			get
			{
				return this._GroupID;
			}
			set
			{
				this._GroupID = value;
			}
		}
		#endregion

		#region Owner
		public abstract class owner : PX.Data.IBqlField
		{
		}
		protected Guid? _Owner;
		[PXDBGuid()]
		[PXChildUpdatable(AutoRefresh = true)]
		[PXOwnerSelector(typeof(groupID))]
		[PXUIField(DisplayName = "Owner")]
		[PXDefault(typeof(Coalesce<
			Search<EPCompanyTreeMember.userID, 
				Where<EPCompanyTreeMember.workGroupID, Equal<Current<EPActivity.groupID>>,
				And<EPCompanyTreeMember.userID, Equal<Current<AccessInfo.userID>>>>>,
			Search2<Users.pKID,
				InnerJoin<EPEmployee, On<EPEmployee.userID, Equal<Users.pKID>>>, 
				Where<Users.pKID, Equal<Current<AccessInfo.userID>>,
				And<Current<EPActivity.groupID>, IsNull>>>>), 
				PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<EPActivity.groupID>))]
		public virtual Guid? Owner
		{
			get
			{
				return this._Owner;
			}
			set
			{
				this._Owner = value;
			}
		}
		#endregion

		#region StartDate
		public abstract class startDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _StartDate;
		[EPStartDate(DisplayName = "Start Date", DisplayNameDate = "Date", DisplayNameTime = "Start Time")]
		[PXFormula(typeof(TimeZoneNow))]
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

		#region CategoryID
		public abstract class categoryID : PX.Data.IBqlField
		{
		}
		protected int? _CategoryID;
		[PXDBInt]
		[PXSelector(typeof(EPEventCategory.categoryID), SubstituteKey = typeof(EPEventCategory.description))]
		[PXUIField(DisplayName = "Category")]
		public virtual int? CategoryID
		{
			get
			{
				return this._CategoryID;
			}
			set
			{
				this._CategoryID = value;
			}
		}
		#endregion


		#region MPStatus
		public abstract class mpstatus : PX.Data.IBqlField { }
		[PXDBString(2, IsFixed = true, IsUnicode = false)]
		[MailStatusList]
		[PXDefault("  ")]
		[PXUIField(DisplayName = "Mail Status", Enabled = false)]
		public virtual string MPStatus
		{
			get { return _mpStatus; }
			set { _mpStatus = value; }
		}
		private string _mpStatus;
		#endregion

		#region PercentCompletion
		public abstract class percentCompletion : PX.Data.IBqlField
		{
		}
		protected Int32? _PercentCompletion;
		[PXDBInt(MinValue = 0, MaxValue = 100)]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Completion (%)")]
		public virtual Int32? PercentCompletion
		{
			get
			{
				return this._PercentCompletion;
			}
			set
			{
				this._PercentCompletion = value;
			}
		}
		#endregion

		#region IsReminderOn
		public abstract class isReminderOn : PX.Data.IBqlField {}
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Reminder")]
		[PXFormula(typeof(Switch<Case<Where<EPActivity.classID, Equal<CRActivityClass.task>, 
												And<EPActivity.owner, IsNotNull, 
												And<EPActivity.createdByID, NotEqual<EPActivity.owner>>>>, True>, 
											False>))]
		public virtual bool? IsReminderOn { get; set; }
		#endregion

		#region ReminderDate
		public abstract class reminderDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _ReminderDate;
		[PXRemindDate(typeof(isReminderOn), typeof(startDate), InputMask = "g", PreserveTime = true)]
		[PXUIField(DisplayName = "Remind at")]
		[PXFormula(typeof(Switch<
			Case<Where<EPActivity.classID, NotEqual<CRActivityClass.task>, Or<EPActivity.isReminderOn, Equal<False>>>, Null,
			Case<Where<EPActivity.classID, Equal<CRActivityClass.task>, And<EPActivity.isReminderOn, Equal<True>, And<Current2<EPActivity.reminderDate>, IsNull>>>, Sub<Current2<EPActivity.startDate>, Minutes<int15>>>>,
			EPActivity.reminderDate>))]
		public virtual DateTime? ReminderDate
		{
			get
			{
				return this._ReminderDate;
			}
			set
			{
				this._ReminderDate = value;
			}
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.IBqlField
		{
		}
		protected Int64? _NoteID;
		[PXNote(new Type[0])]
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

		#region ParentTaskID
		public abstract class parentTaskID : PX.Data.IBqlField
		{
		}
		protected Int32? _ParentTaskID;

		[PXUIField(DisplayName = "Parent")]
		[PXDBInt]
		[PXSelector(typeof(Search<EPActivity.taskID>))]
		[PXRestrictor(typeof(Where<EPActivity.classID, Equal<CRActivityClass.task>, Or<EPActivity.classID, Equal<CRActivityClass.events>>>), null)]
		//[PXParentActivityInfo(typeof(EPActivity.refNoteID), DescriptionDisplayName = "Parent Description")]
		public virtual int? ParentTaskID
		{
			get
			{
				return this._ParentTaskID;
			}
			set
			{
				this._ParentTaskID = value;
			}
		}
		#endregion

        #region OrigTaskID
        public abstract class origTaskID : PX.Data.IBqlField
        {
        }
        protected Int32? _OrigTaskID;
        /// <summary>
        /// Use for correction. Stores the reference to the original activity.
        /// </summary>
        [PXDBInt]
        public virtual int? OrigTaskID
        {
            get
            {
                return this._OrigTaskID;
            }
            set
            {
                this._OrigTaskID = value;
            }
        }
        #endregion

		#region RefNoteID
		public abstract class refNoteID : PX.Data.IBqlField
		{
		}
		protected Int64? _RefNoteID;
		[PXDBLong]
		[PXDBDefault(null, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXParent(typeof(Select<CRActivityStatistics, Where<CRActivityStatistics.noteID, Equal<Current<EPActivity.refNoteID>>>>), LeaveChildren = true, ParentCreate = true)]
		public virtual Int64? RefNoteID
		{
			get
			{
				return this._RefNoteID;
			}
			set
			{
				this._RefNoteID = value;
			}
		}
		#endregion

		#region ParentRefNoteID
		public abstract class parentRefNoteID : PX.Data.IBqlField
		{
		}
		protected Int64? _ParentRefNoteID;
		[PXDBLong]
		[PXFormula(typeof(Selector<EPActivity.bAccountID, BAccount.noteID>))]
		public virtual Int64? ParentRefNoteID
		{
			get
			{
				return this._ParentRefNoteID;
			}
			set
			{
				this._ParentRefNoteID = value;
			}
		}
		#endregion

		#region BAccountID
		public abstract class bAccountID : IBqlField {}
		protected Int32? _BAccountID;
		[PXInt]
		[PXDBScalar(typeof(Search<BAccount.bAccountID, Where<BAccount.noteID, Equal<EPActivity.parentRefNoteID>>>))]
		[PXUIField(DisplayName = "Business Account ID", Enabled = false)]
		[PXSelector(typeof(Search<BAccount.bAccountID>), SubstituteKey = typeof(BAccount.acctCD))]
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

		#region Source
		public abstract class source : PX.Data.IBqlField
		{
		}
		protected String _Source;
		[PXString]
		[PXUIField(DisplayName = "Related Entity", Enabled = false)]
		[PXEntityInfo(typeof(EPActivity.refNoteID), DescriptionDisplayName = "Related Entity Description", DescriptionDisplayOnEmpty=true)]
		public virtual String Source
		{
			get
			{
				return _Source;
			}
			set
			{
				_Source = value;
			}
		}
		#endregion

		#region ClassIcon

		public abstract class classIcon : IBqlField
		{
			public class task : Constant<string>
			{
				public task() : base(Sprite.Main.GetFullUrl(Sprite.Main.Task)) { }
			}
			public class events : Constant<string>
			{
				public events() : base(Sprite.Main.GetFullUrl(Sprite.Main.Event)) { }
			}
			public class email : Constant<string>
			{
				public email() : base(Sprite.Control.GetFullUrl(Sprite.Control.Mail)) { }
			}
			public class emailResponse : Constant<string>
			{
				public emailResponse() : base(Sprite.Control.GetFullUrl(Sprite.Control.MailReply)) { }
			}
			public class history : Constant<string>
			{
				public history() : base(Sprite.Main.GetFullUrl(Sprite.Main.History)) { }
			}
		}

		[PXUIField(DisplayName = "", IsReadOnly = true)]
		[PXImage(HeaderImage = "")]
		[PXFormula(typeof(Switch<Case<Where<EPActivity.classID, Equal<CRActivityClass.task>>, EPActivity.classIcon.task,
			Case<Where<EPActivity.classID, Equal<CRActivityClass.events>>, EPActivity.classIcon.events,
			Case<Where<EPActivity.classID, Equal<CRActivityClass.email>, And<EPActivity.isIncome, NotEqual<True>>>, EPActivity.classIcon.email,
			Case<Where<EPActivity.classID, Equal<CRActivityClass.email>, And<EPActivity.isIncome, Equal<True>>>, EPActivity.classIcon.emailResponse,
			Case<Where<EPActivity.classID, Equal<CRActivityClass.history>>, EPActivity.classIcon.history
			>>>>>,
			Selector<Current2<EPActivity.type>, EPActivityType.imageUrl>>))]

		public virtual string ClassIcon { get; set; }

		#endregion

		#region EmailIcon

		public abstract class emailIcon : IBqlField { }

		[PXUIField(DisplayName = "", IsReadOnly = true)]
		[PXImage(HeaderImage = "")]
		[PXFormula(typeof(Switch<Case<Where<EPActivity.classID, Equal<CRActivityClass.email>, And<EPActivity.isIncome, NotEqual<True>>>, EPActivity.classIcon.email,
			Case<Where<EPActivity.classID, Equal<CRActivityClass.email>, And<EPActivity.isIncome, Equal<True>>>, EPActivity.classIcon.emailResponse>>>))]
		public virtual string EmailIcon { get; set; }

		#endregion

		#region ReminderIcon

		public abstract class reminderIcon : IBqlField
			{
			public class reminder : Constant<string>
				{
				public reminder() : base(Sprite.Control.GetFullUrl(Sprite.Control.Reminder)) { }
			}
		}

		[PXUIField(DisplayName = "", IsReadOnly = true)]
		[PXImage(HeaderImage = (Sprite.AliasControl + "@" + Sprite.Control.ReminderHead))]
		[PXFormula(typeof(Switch<Case<Where<EPActivity.isReminderOn, Equal<True>>, EPActivity.reminderIcon.reminder>>))]
		public virtual String ReminderIcon { get; set; }

		#endregion

		#region PriorityIcon

		public abstract class priorityIcon : IBqlField
				{
			public class low : Constant<string>
					{
				public low() : base(Sprite.Control.GetFullUrl(Sprite.Control.PriorityLow)) { }
				}
			public class high : Constant<string>
			{
				public high() : base(Sprite.Control.GetFullUrl(Sprite.Control.PriorityHigh)) { }
			}
		}

		[PXUIField(DisplayName = "", IsReadOnly = true)]
		[PXImage(HeaderImage = (Sprite.AliasControl + "@" + Sprite.Control.PriorityHead))]
		[PXFormula(typeof(Switch<Case<Where<EPActivity.priority, Equal<int0>>, EPActivity.priorityIcon.low,
			Case<Where<EPActivity.priority, Equal<int2>>, EPActivity.priorityIcon.high>>>))]
		public virtual String PriorityIcon { get; set; }
		#endregion

		#region IsCompleteIcon

		public abstract class isCompleteIcon : IBqlField
				{
			public class completed : Constant<string>
					{
				public completed() : base(Sprite.Control.GetFullUrl(Sprite.Control.Complete)) { }
			}
		}

		[PXUIField(DisplayName = "", IsReadOnly = true)]
		[PXImage(HeaderImage = (Sprite.AliasControl + "@" + Sprite.Control.CompleteHead))]
		[PXFormula(typeof(Switch<Case<Where<EPActivity.uistatus, Equal<ActivityStatusListAttribute.completed>>, EPActivity.isCompleteIcon.completed>>))]
		public virtual String IsCompleteIcon { get; set; }
		#endregion

		#region EndDate
		public abstract class endDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _EndDate;
		[EPEndDate(typeof(classID), typeof(startDate), typeof(timeSpent))]
		[PXUIField(DisplayName = "End Time")]
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

		#region ProcessDate

		public abstract class processDate : IBqlField { }

		[PXDate(DisplayMask = "g")]
		[PXUIField(DisplayName = "Date", Enabled = false)]
		[PXDBCalced(typeof(EPActivity.lastModifiedDateTime), typeof(DateTime))]
		public virtual DateTime? ProcessDate { get; set; }

		#endregion

		#region ImcUID

		public abstract class imcUID : IBqlField { }

		protected Guid? _ImcUID;

		[PXDBGuid(true)]
		[PXUIField(Visible = false, Enabled = false)]
		public virtual Guid? ImcUID
		{
			get { return _ImcUID ?? (_ImcUID = Guid.NewGuid()); }
			set { _ImcUID = value; }
		}

		#endregion

		#region Pop3UID

		public abstract class pop3UID : IBqlField { }

		[PXDBString(70)]
		[PXUIField(Visible = false, Enabled = false)]
		public virtual string Pop3UID { get; set; }

		#endregion

		#region ImapUID

		public abstract class imapUID : IBqlField { }

		[PXDBInt]
		[PXUIField(Visible = false, Enabled = false)]
		public virtual int? ImapUID { get; set; }

		#endregion

		#region CompletedDateTime
		public abstract class completedDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _CompletedDateTime;
		[PXDBDate(InputMask = "g", PreserveTime = true)]
		[PXUIField(DisplayName = "Completed At", Enabled = false)]
		public virtual DateTime? CompletedDateTime
		{
			get
			{
				return this._CompletedDateTime;
			}
			set
			{
				this._CompletedDateTime = value;
			}
		}
		#endregion

		#region isOverdue
		public abstract class isOverdue : PX.Data.IBqlField { }

		[PXBool]
		public virtual bool? IsOverdue
		{
			[PXDependsOnFields(typeof(uistatus),typeof(endDate))]
			get
			{
				return
					UIStatus != ActivityStatusAttribute.Completed &&
					UIStatus != ActivityStatusAttribute.Canceled &&
					Released != true &&
					_EndDate != null &&
					_EndDate < PX.Common.PXTimeZoneInfo.Now;
			}
		}
		#endregion

		#region AllDay
		public abstract class allDay : PX.Data.IBqlField
		{
		}
		protected Boolean? _AllDay;
		[PXUIField(DisplayName = "All Day")]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[EPAllDay(typeof(EPActivity.startDate), typeof(EPActivity.endDate))]
		public virtual Boolean? AllDay
		{
			get
			{
				return this._AllDay;
			}
			set
			{
				this._AllDay = value;
			}
		}
		#endregion

		#region Location
		public abstract class location : PX.Data.IBqlField
		{
		}
		protected String _Location;
		[PXDBString(255, InputMask = "")]
		[PXUIField(DisplayName = "Location")]
		public virtual String Location
		{
			get
			{
				return this._Location;
			}
			set
			{
				this._Location = value;
			}
		}
		#endregion



		#region UIStatus
		public abstract class uistatus : PX.Data.IBqlField { }

		[PXDBString(2, IsFixed = true)]
		[ActivityStatus]
		[PXUIField(DisplayName = "Status")]
		[PXDefault(ActivityStatusAttribute.Open, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Switch<
			Case<Where<EPActivity.trackTime, Equal<False>, And<EPActivity.mpstatus, Equal<StringEmpty>>>, ActivityStatusAttribute.completed,
			Case<Where<EPActivity.mpstatus, Equal<MailStatusListAttribute.processed>>, ActivityStatusAttribute.completed,
			Case<Where<EPActivity.mpstatus, Equal<MailStatusListAttribute.deleted>, 
							Or<EPActivity.mpstatus, Equal<MailStatusListAttribute.failed>,
							Or<EPActivity.mpstatus, Equal<MailStatusListAttribute.canceled>>>>, ActivityStatusAttribute.canceled>>>, 			
			ActivityStatusAttribute.open>))]
		public virtual string UIStatus
		{
			get { return _uiStatus; }
			set { _uiStatus = value; }
		}
		private string _uiStatus;

		#endregion

		#region ApprovalStatus
		public abstract class approvalStatus : IBqlField { }

		[PXDBString(2, IsFixed = true)]
		[ActivityStatusListAttribute]
		[PXUIField(DisplayName = "Record Status", Enabled = false)]
		[PXFormula(typeof(Switch<
			Case<Where<uistatus, Equal<ActivityStatusAttribute.canceled>>, ActivityStatusAttribute.canceled
				, Case<Where<released, Equal<True>>, ActivityStatusAttribute.released
					, Case<Where<uistatus, Equal<ActivityStatusAttribute.rejected>>, ActivityStatusAttribute.rejected
						, Case<Where<approverID, IsNotNull, And<uistatus, NotEqual<ActivityStatusAttribute.approved>>>, ActivityStatusAttribute.pendingApproval>
						>
					>
				>
			, ActivityStatusAttribute.completed
			>
			))]
		public virtual string ApprovalStatus { get; set; }

		#endregion

		#region IsPrivate

		public abstract class isPrivate : IBqlField { }
		protected bool? _isPrivate;

		[PXDBBool]
		[PXUIField(DisplayName = "Internal")]
		[PXFormula(typeof(IsNull<Selector<EPActivity.type, EPActivityType.privateByDefault>,False>))]
		public virtual Boolean? IsPrivate
		{
			get { return _isPrivate; }
			set { _isPrivate = value; }
		}

		#endregion

		#region IsBillable
		public abstract class isBillable : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsBillable;
		[PXDBBool()]
		[PXUIField(DisplayName = "Billable")]
		[PXFormula(typeof(Switch<Case<Where2<FeatureInstalled<FeaturesSet.timeReportingModule>, And<EPActivity.trackTime, Equal<True>>>, IsNull<Selector<EPActivity.earningTypeID, EPEarningType.isbillable>,False>>,False>))]
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

		#region ProjectID
		public abstract class projectID : IBqlField { }
		[EPActivityProjectDefault]
		[EPActiveProject(FieldClass = ProjectAttribute.DimensionName)]
		[PXFormula(typeof(Switch<
			Case<Where<Not<FeatureInstalled<FeaturesSet.projectModule>>>, DefaultValue<EPActivity.projectID>,
			Case<Where<EPActivity.parentTaskID, IsNotNull, And<Selector<EPActivity.parentTaskID, Selector<EPActivity.projectID, PMProject.contractCD>>, IsNotNull>>, Selector<EPActivity.parentTaskID, Selector<EPActivity.projectID, PMProject.contractCD>>,
			Case<Where<EPActivity.isBillable, Equal<True>, And<Selector<Current2<EPActivity.projectID>, PMProject.nonProject>, Equal<True>>>, Null,
			Case<Where<EPActivity.isBillable, Equal<False>, And<Current2<EPActivity.projectID>, IsNull>>, DefaultValue<EPActivity.projectID>>>>>,
			EPActivity.projectID>))]
		public virtual Int32? ProjectID{ get; set; }

		#endregion

		#region ProjectTaskID
		public abstract class projectTaskID : IBqlField { }
		[ActiveProjectTask(typeof(EPActivity.projectID), BatchModule.EP, AllowNullIfContract = true, DisplayName = "Project Task")]
		[PXFormula(typeof(Switch<Case<Where<EPActivity.parentTaskID, IsNotNull>, Selector<EPActivity.parentTaskID, Selector<EPActivity.projectTaskID, PMTask.taskCD>>>, EPActivity.projectTaskID>))]
		public virtual Int32? ProjectTaskID { get; set; }
		#endregion

		#region Subject
		public abstract class subject : PX.Data.IBqlField
		{
		}
		protected String _Subject;

		[PXDBString(255, InputMask = "", IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Summary", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXNavigateSelector(typeof(EPActivity.subject))]
		public virtual String Subject
		{
			[PXDependsOnFields(typeof(classID), typeof(taskID))]
			get
			{
				return _ClassID == CRActivityClass.History ? "Changeset " + _TaskID : this._Subject;
			}
			set
			{
				this._Subject = value;
			}
		}
		#endregion

		#region TimeSpent
		public abstract class timeSpent : IBqlField
		{
		}
		protected Int32? _TimeSpent;
		[PXTimeList]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBInt]
		[PXUIField(DisplayName = "Time")]
		[PXFormula(typeof(Switch<Case<Where<EPActivity.trackTime, NotEqual<True>>, int0>, EPActivity.timeSpent>))]
		public virtual Int32? TimeSpent
		{
			get
			{
				return this._TimeSpent;
			}
			set
			{
				this._TimeSpent = value;
			}
		}
		#endregion

		#region OvertimeSpent
		public abstract class overtimeSpent : PX.Data.IBqlField
		{
		}
		protected Int32? _OvertimeSpent;
		[PXTimeList]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBInt]
		[PXFormula(typeof(Switch<Case<Where<Selector<EPActivity.earningTypeID, EPEarningType.isOvertime>, Equal<True>>, EPActivity.timeSpent>, int0>))]
		[PXUIField(DisplayName = "Overtime", Enabled = false)]
		public virtual Int32? OvertimeSpent
		{
			get
			{
				return this._OvertimeSpent;
			}
			set
			{
				this._OvertimeSpent = value;
			}
		}
		#endregion

		#region TimeBillable
		public abstract class timeBillable : PX.Data.IBqlField
		{
		}
		protected Int32? _TimeBillable;
		[PXTimeList]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBInt]
		[PXFormula(typeof(
			Switch<Case<Where<EPActivity.isBillable, Equal<True>>, EPActivity.timeSpent,
						 Case<Where<EPActivity.isBillable, Equal<False>>, int0>>,
						 EPActivity.timeBillable>))]
		[PXUIField(DisplayName = "Billable Time")]
		public virtual Int32? TimeBillable
		{
			get
			{
				return this._TimeBillable;
			}
			set
			{
				this._TimeBillable = value;
			}
		}
		#endregion

		#region OvertimeBillable
		public abstract class overtimeBillable : PX.Data.IBqlField
		{
		}
		protected Int32? _OvertimeBillable;
		[PXTimeList]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBInt]
		[PXFormula(typeof(
			Switch<Case<Where<EPActivity.isBillable, Equal<True>, And<EPActivity.overtimeSpent, GreaterEqual<int0>>>, EPActivity.overtimeSpent,
						 Case<Where<EPActivity.isBillable, Equal<False>>, int0>>,
						 EPActivity.overtimeBillable>))]
		[PXUIField(DisplayName = "Billable Overtime")]
		public virtual Int32? OvertimeBillable
		{
			get
			{
				return this._OvertimeBillable;
			}
			set
			{
				this._OvertimeBillable = value;
			}
		}
		#endregion

		#region Released
		public abstract class released : PX.Data.IBqlField
		{
		}
		protected Boolean? _Released;
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Released", Enabled =  false, Visible = false)]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
			}
		}
		#endregion

		#region Billed
		public abstract class billed : PX.Data.IBqlField
		{
		}
		protected Boolean? _Billed;
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Billed")]
		public virtual Boolean? Billed
		{
			get
			{
				return this._Billed;
			}
			set
			{
				this._Billed = value;
			}
		}
		#endregion

		#region IsIncome

		public abstract class isIncome : IBqlField { }

		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Incoming")]
		public virtual Boolean? IsIncome { get; set; }

		#endregion

        #region IsCorrected
        public abstract class isCorrected : PX.Data.IBqlField
        {
        }
        protected Boolean? _IsCorrected;

        /// <summary>
        /// If true this Activity has been corrected in the Timecard and is no longer valid. Please hide this activity in all lists displayed in the UI since there is another valid activity.
        /// The valid activity has a refence back to the corrected activity via OrigTaskID field. 
        /// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? IsCorrected
        {
            get
            {
                return this._IsCorrected;
            }
            set
            {
                this._IsCorrected = value;
            }
        }
        #endregion

		#region MailAccountID

		public abstract class mailAccountID : IBqlField { }
		[PXDBInt]
		[PXUIField(DisplayName = "From")]
		public Int32? MailAccountID { get; set; }

		#endregion

		#region MailFrom
		public abstract class mailFrom : IBqlField { }

		[PXDBString(500, IsUnicode = true)]
		[PXUIField(DisplayName = "From")]
		public virtual string MailFrom { get; set; }
		#endregion

		#region MailReply
		public abstract class mailReply : IBqlField { }

		[PXDBString(500, IsUnicode = true)]
		[PXUIField(DisplayName = "Reply")]
		public virtual string MailReply { get; set; }
		#endregion

		#region MailTo
		public abstract class mailTo : PX.Data.IBqlField
		{
		}
		protected string _MailTo;
		[PXDBString(1000, IsUnicode = true)]
		[PXUIField(DisplayName = "To")]
		public virtual string MailTo
		{
			get
			{
				return this._MailTo;
			}
			set
			{
				this._MailTo = value;
			}
		}
		#endregion

		#region MailCc
		public abstract class mailCc : PX.Data.IBqlField
		{
		}
		protected string _MailCc;
		[PXDBString(1000, IsUnicode = true)]
		[PXUIField(DisplayName = "CC")]
		public virtual string MailCc
		{
			get
			{
				return this._MailCc;
			}
			set
			{
				this._MailCc = value;
			}
		}
		#endregion

		#region MailBcc
		public abstract class mailBcc : PX.Data.IBqlField
		{
		}
		protected string _MailBcc;
		[PXDBString(1000, IsUnicode = true)]
		[PXUIField(DisplayName = "BCC")]
		public virtual string MailBcc
		{
			get
			{
				return this._MailBcc;
			}
			set
			{
				this._MailBcc = value;
			}
		}
		#endregion

		#region Exception
		public abstract class exception : PX.Data.IBqlField
		{
		}
		protected string _Exception;
		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "Error Message")]
		public virtual string Exception
		{
			get
			{
				return this._Exception;
			}
			set
			{
				this._Exception = value;
			}
		}
		#endregion

		#region RetryCount
		public abstract class retryCount : PX.Data.IBqlField
		{
		}
		[PXDBInt]
		[PXUIField(Visible = false)]
		[PXDefault(0)]
		public virtual Int32? RetryCount { get; set; }
		#endregion

		#region Format
		public abstract class format : PX.Data.IBqlField
		{
		}
		protected string _Format;
		[PXDefault(EmailFormatListAttribute.Html, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(255)]
		[PXUIField(DisplayName = "Format")]
		[EmailFormatList]
		public virtual string Format
		{
			get
			{
				return this._Format;
			}
			set
			{
				this._Format = value;
			}
		}
		#endregion

		#region Ticket
		public abstract class ticket : PX.Data.IBqlField
		{
		}
		[PXDBInt]
		[PXUIField(Visible = false)]
		public virtual Int32? Ticket { get; set; }
		#endregion

		#region MessageId
		public abstract class messageId : PX.Data.IBqlField
		{
		}
		protected string _MessageId;
		[PXDBString(255)] //TODO: need review length
		[PXUIField(Visible = false)]
		public virtual string MessageId
		{
			get
			{
				return this._MessageId;
			}
			set
			{
				this._MessageId = value;
			}
		}
		#endregion

		#region ReportFormat

		public abstract class reportFormat : IBqlField { }

		[PXDBString(10)]
		[PXUIField(DisplayName = "Format")]
		[PXStringList(new[] { "PDF", "HTML", "Excel" }, new[] { "Pdf", "Html", "Excel" })]
		public virtual String ReportFormat { get; set; }

		#endregion

		#region TimeCardCD

		public abstract class timeCardCD : IBqlField { }

		[PXDBString(10)]
		[PXUIField(Visible = false)]
		public virtual String TimeCardCD { get; set; }

		#endregion

		#region SummaryLineNbr
		public abstract class summaryLineNbr : IBqlField { }

	    protected int? _SummaryLineNbr;

		/// <summary>
		/// This is a adjusting activity for the summary line in the Timecard.
		/// </summary>
		[PXDBInt()]
		public virtual Int32? SummaryLineNbr
        {
            get
            {
                return _SummaryLineNbr;
            }
             set
             {
                 _SummaryLineNbr = value;
             }
        }
		#endregion

		#region TimeSheetCD

		public abstract class timeSheetCD : IBqlField { }

		[PXDBString(15)]
		[PXUIField(Visible = false)]
		public virtual String TimeSheetCD { get; set; }

		#endregion

		#region TranID
		public abstract class tranID : IBqlField { }

		[PXDBLong]
		public virtual Int64? TranID { get; set; }
		#endregion

		#region WeekID

		public abstract class weekID : IBqlField { }

	    protected Int32? _WeekID;
	    [PXDBInt]
	    [PXUIField(DisplayName = "Time Card Week")]
		[PXWeekSelector2()]
		[PXFormula(typeof(Default<EPActivity.startDate>))]
		[PXDefaultWeek(typeof(EPActivity.startDate))]
		public virtual Int32? WeekID
	    {
            get { return this._WeekID; }
            set { this._WeekID = value; }
	    }

		#endregion

		#region LabourItemID
		public abstract class labourItemID : IBqlField { }

		[PXDBInt]
		[PXUIField(Visible = false)]
		public virtual Int32? LabourItemID { get; set; }
		#endregion

		#region OvertimeItemID
		public abstract class overtimeItemID : IBqlField { }

		[PXDBInt]
		[PXUIField(Visible = false)]
		public virtual Int32? OvertimeItemID { get; set; }
		#endregion


		#region IsExternal

		public abstract class isExternal : IBqlField { }

		[PXDBBool]
		[PXUIField(Visible = false)]
		public virtual bool? IsExternal { get; set; }

		#endregion

		#region IsSystem

		public abstract class isSystem : IBqlField { }

		[PXDBBool]
		[PXUIField(Visible = false)]
		public virtual bool? IsSystem { get; set; }

		#endregion


		#region CreatedByID
		public abstract class createdByID : PX.Data.IBqlField
		{
		}
		protected Guid? _CreatedByID;
		[PXDBCreatedByID(DontOverrideValue = true)]
		[PXUIField(DisplayName = "Created By", Enabled = false)]
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
		[PXDBCreatedByScreenID]
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
		[PXUIField(DisplayName = "Created At", Enabled = false)]
		[PXDBCreatedDateTimeUtc]
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
		[PXDBLastModifiedByID]
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
		[PXDBLastModifiedByScreenID]
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
		[PXDBLastModifiedDateTimeUtc]
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

		#region EntityDescription
		public abstract class entityDescription : IBqlField
		{
		}
		protected string _EntityDescription;
		[PXString(InputMask = "")]
		[PXUIField(DisplayName = "Entity", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, IsReadOnly = true)]
		public virtual string EntityDescription
		{
			get
			{
				return this._EntityDescription;
			}
			set
			{
				this._EntityDescription = value;
			}
		}
		#endregion

		#region IsViewed

		public abstract class isViewed : IBqlField { }

		[PXBool]
		[PXUIField(Visible = false)]
		public virtual Boolean? IsViewed { get; set; }

		#endregion

		#region DayOfWeek

		public abstract class dayOfWeek : IBqlField { }

		[PXInt]
		[PXUIField(DisplayName = PX.Objects.EP.Messages.DayOfWeek)]
		[DayOfWeek]
		public Int32? DayOfWeek
		{
			[PXDependsOnFields(typeof(startDate))]
			get
			{
				var date = StartDate;
				return date != null ? (int?)((int)date.Value.DayOfWeek) : null;
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
		[PXFormula(typeof(
			Switch<
				Case<Where<Selector<EPActivity.projectTaskID, PMTask.approverID>, IsNotNull>, Selector<EPActivity.projectTaskID, PMTask.approverID>>
				, Case<Where<Selector<EPActivity.projectID, Contract.approverID>, IsNotNull>, Selector<EPActivity.projectID, Contract.approverID>>
				>
			))]
		[PXUIField(DisplayName = "Approver", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? ApproverID { get; set; }
		#endregion

		#region StartDate
		public abstract class approvedDate : PX.Data.IBqlField
		{
		}
		[PXDBDate(DisplayMask = "d", PreserveTime = true)]
		[PXUIField(DisplayName = "Approved Date")]
		public virtual DateTime? ApprovedDate { get; set; }
		#endregion

		#region EmployeeRate
		public abstract class employeeRate : IBqlField
		{
		}
		protected decimal? _EmployeeRate;

        /// <summary>
        /// Stores Employee's Hourly rate at the time the activity was released to PM
        /// </summary>
        [IN.PXDBPriceCost]
		[PXUIField(Visible = false)]
		public virtual decimal? EmployeeRate
		{
			get
			{
				return this._EmployeeRate;
			}
			set
			{
				this._EmployeeRate = value;
			}
		}
		#endregion

		#region Obsolete
		//TODO: need remove
		#region CustomerID
		public abstract class customerID : IBqlField { }

		[PXDBInt]
		public virtual Int32? CustomerID { get; set; }
		#endregion

		//TODO: need remove
		#region LocationID
		public abstract class locationID : IBqlField { }

		[PXDBInt]
		public virtual Int32? LocationID { get; set; }
		#endregion
		//TODO: need remove
		#region Tooltip
		public abstract class tooltip : PX.Data.IBqlField
		{
		}
		[PXString]
		[PXUIField(Visible = false)]
		public virtual String Tooltip
		{
			[PXDependsOnFields(typeof(format),typeof(subject),typeof(startDate),typeof(endDate))]
			get
			{
				return string.Format("{0} ({1:hh:mm} - {2:hh:mm})", this.Subject, this.StartDate, this.EndDate);
			}
		}
		#endregion
		//TODO: need remove
		#region Hold

		public abstract class hold : IBqlField { }

		[PXBool]
		[PXUIField(Visible = false)]
		public virtual Boolean? Hold { get; set; }

		#endregion
		//TODO: need remove
		#region EntityType
		public abstract class entityType : IBqlField
		{
		}
		protected string _EntityType;
		[PXString]
		public virtual string EntityType
		{
			get
			{
				return this._EntityType;
			}
			set
			{
				this._EntityType = value;
			}
		}
		#endregion
		//TODO: need remove
		#region GraphType
		public abstract class graphType : IBqlField
		{
		}
		protected string _GraphType;

		[PXString]
		public virtual string GraphType
		{
			get
			{
				return this._GraphType;
			}
			set
			{
				this._GraphType = value;
			}
		}
		#endregion
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
	}
}
