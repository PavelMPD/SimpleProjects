using System;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;

namespace PX.Objects.EP
{
	[PXVirtual]
	[PXCacheName(Messages.TimeCardDetail)]
	[Serializable]
	public partial class EPTimeCardSummary : IBqlTable
	{
		#region TimeCardCD

		public abstract class timeCardCD : IBqlField { }

        [PXDBDefault(typeof(EPTimeCard.timeCardCD))]
		[PXDBString(10, IsKey = true)]
		[PXUIField(Visible = false)]
        [PXParent(typeof(Select<EPTimeCard, Where<EPTimeCard.timeCardCD, Equal<Current<EPTimeCardSummary.timeCardCD>>>>))]
		public virtual String TimeCardCD { get; set; }

		#endregion
		#region LineNbr
		public abstract class lineNbr : IBqlField { }

		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(EPTimeCard.summaryLineCntr))]
		[PXUIField(Visible = false)]
		public virtual Int32? LineNbr { get; set; }
		#endregion
		#region EarningType
		public abstract class earningType : IBqlField { }
		[PXDBString(2, IsFixed = true, IsUnicode = false, InputMask=">LL")]
		[PXDefault(typeof(Search<EPSetup.regularHoursType>))]
		[PXSelector(typeof(EPEarningType.typeCD))]
		[PXUIField(DisplayName = "Earning Type")]
		public virtual string EarningType { get; set; }
		#endregion

		#region ParentTaskID
		public abstract class parentTaskID : PX.Data.IBqlField
		{
		}
		protected Int32? _ParentTaskID;

		[PXUIField(DisplayName = "Task ID")]
		[PXDBInt]
		[PXSelector(typeof(Search<EPActivity.taskID>))]
		[PXRestrictor(typeof(Where<EPActivity.classID, Equal<CRActivityClass.task>, Or<EPActivity.classID, Equal<CRActivityClass.events>>>), null)]
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

		#region ProjectID
		public abstract class projectID : IBqlField { }
		[ProjectDefault(BatchModule.EP, ForceProjectExplicitly = true)]
		[EPTimeCardActiveProjectAttribute]
		public virtual Int32? ProjectID { get; set; }
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : IBqlField { }
		[ActiveProjectTask(typeof(EPTimeCardSummary.projectID), BatchModule.EP, AllowNullIfContract = true, DisplayName = "Project Task")]
		public virtual Int32? ProjectTaskID { get; set; }
		#endregion
		#region TimeSpent
		public abstract class timeSpent : IBqlField { }

        [PXInt]
		[PXUIField(DisplayName = "Time Spent", Enabled = false)]
		public virtual Int32? TimeSpent
		{
			get
			{
				return Mon.GetValueOrDefault() +
					   Tue.GetValueOrDefault() +
					   Wed.GetValueOrDefault() +
					   Thu.GetValueOrDefault() +
					   Fri.GetValueOrDefault() +
					   Sat.GetValueOrDefault() +
					   Sun.GetValueOrDefault();
			}
		}
		#endregion
		#region Sun
		public abstract class sun : IBqlField { }
        [PXTimeList]
        [PXDBInt]
		[PXUIField(DisplayName = "Sun")]
		public virtual Int32? Sun { get; set; }
		#endregion
		#region Mon
		public abstract class mon : IBqlField { }

	    protected int? _Mon;
        [PXTimeList]
        [PXDBInt]
		[PXUIField(DisplayName = "Mon")]
		public virtual Int32? Mon {
            get { return _Mon;  } 
            set { _Mon = value; }
        }
		#endregion
		#region Tue
		public abstract class tue : IBqlField { }
        [PXTimeList]
        [PXDBInt]
		[PXUIField(DisplayName = "Tue")]
		public virtual Int32? Tue { get; set; }
		#endregion
		#region Wed
		public abstract class wed : IBqlField { }
        [PXTimeList]
        [PXDBInt]
		[PXUIField(DisplayName = "Wed")]
		public virtual Int32? Wed { get; set; }
		#endregion
		#region Thu
		public abstract class thu : IBqlField { }
        [PXTimeList]
        [PXDBInt]
		[PXUIField(DisplayName = "Thu")]
		public virtual Int32? Thu { get; set; }
		#endregion
		#region Fri
		public abstract class fri : IBqlField { }
        [PXTimeList]
        [PXDBInt]
		[PXUIField(DisplayName = "Fri")]
		public virtual Int32? Fri { get; set; }
		#endregion
		#region Sat
		public abstract class sat : IBqlField { }
        [PXTimeList]
        [PXDBInt]
		[PXUIField(DisplayName = "Sat")]
		public virtual Int32? Sat { get; set; }
		#endregion
		#region IsBillable
		public abstract class isBillable : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsBillable;
		[PXDBBool()]
		[PXDefault()]
		[PXUIField(DisplayName = "Billable")]
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
		#region Description
		public abstract class description : PX.Data.IBqlField
		{
		}
		protected String _Description;
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
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

		#region ApprovalStatus
		public abstract class approvalStatus : IBqlField { }

		[PXString(2, IsFixed = true)]
		[ApprovalStatusListAttribute]
		[PXUIField(DisplayName = "Approval Status", Enabled = false)]
		public virtual string ApprovalStatus { get; set; }
		#endregion

		#region ApproverID
		public abstract class approverID : PX.Data.IBqlField
		{
		}
		[PXInt]
		[PXEPEmployeeSelector]
		[PXFormula(typeof(
			Switch<
				Case<Where<Selector<EPTimeCardSummary.projectTaskID, PMTask.approverID>, IsNotNull>, Selector<EPTimeCardSummary.projectTaskID, PMTask.approverID>>
				, Case<Where<Selector<EPTimeCardSummary.projectID, CT.Contract.approverID>, IsNotNull>, Selector<EPTimeCardSummary.projectID, CT.Contract.approverID>>
				>
			))]
		[PXUIField(DisplayName = "Approver", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? ApproverID { get; set; }
		#endregion

		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.IBqlField
		{
		}
		protected Int64? _NoteID;
		[PXNote()]
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

        public int? GetTimeTotal(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday: return Mon;
                case DayOfWeek.Tuesday: return Tue;
                case DayOfWeek.Wednesday: return Wed;
                case DayOfWeek.Thursday: return Thu;
                case DayOfWeek.Friday: return Fri;
                case DayOfWeek.Saturday: return Sat;
                case DayOfWeek.Sunday: return Sun;

                default:
                    return null;

            }
        }

        public int? GetTimeTotal()
        {
            return Mon.GetValueOrDefault() + Tue.GetValueOrDefault() + Wed.GetValueOrDefault() + Thu.GetValueOrDefault() +
                   Fri.GetValueOrDefault() + Sat.GetValueOrDefault() + Sun.GetValueOrDefault();

        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", EarningType, Mon, Tue, Wed, Thu, Fri, Sat, Sun);
        }

	}
}
