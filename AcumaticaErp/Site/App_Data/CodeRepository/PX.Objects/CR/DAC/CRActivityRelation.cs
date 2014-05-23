using System;
using PX.Data;

namespace PX.Objects.CR
{
    [Serializable]
	public partial class CRActivityRelation : IBqlTable
	{
		#region RecordID

		public abstract class recordID : IBqlField { }

		[PXDBIdentity(IsKey = true)]
		[PXUIField(Visible = false)]
		public virtual Int32? RecordID { get; set; }

		#endregion

		#region TaskID

		public abstract class taskID : IBqlField { }

		[PXDBInt]
		[PXUIField(Visible = false)]
		[PXDBDefault(typeof(EPActivity.taskID))]
			public virtual Int32? TaskID { get; set; }

		#endregion

		#region RefTaskID

		public abstract class refTaskID : IBqlField { }

		[PXDBInt]
		[PXUIField(DisplayName = "Task ID")]
		[RefTaskSelectorAttribute(typeof(EPActivity.taskID))]
		public virtual Int32? RefTaskID { get; set; }

		#endregion

		#region Subject

		public abstract class subject : IBqlField { }

		[PXString]
		[PXUIField(DisplayName = "Subject", Enabled = false)]
		public virtual String Subject { get; set; }

		#endregion

		#region StartDate
		public abstract class startDate : IBqlField { }

		[PXDate(InputMask = "g", DisplayMask = "g")]
		[PXUIField(DisplayName = "Start Date", Enabled = false)]
		public virtual DateTime? StartDate { get; set; }
		#endregion

		#region EndDate
		public abstract class endDate : IBqlField { }

		[PXDate(InputMask = "g", DisplayMask = "g")]
		[PXUIField(DisplayName = "Due Date", Enabled = false)]
		public virtual DateTime? EndDate { get; set; }
		#endregion

		#region CompletedDateTime
		public abstract class completedDateTime : IBqlField { }

		[PXDate(InputMask = "g", DisplayMask = "g")]
		[PXUIField(DisplayName = "Completed At", Enabled = false)]
		public virtual DateTime? CompletedDateTime { get; set; }
		#endregion

		#region Status
		public abstract class status : IBqlField { }

		[PXString]
		[ActivityStatusList]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		public virtual String Status { get; set; }
		#endregion


		#region CreatedByID
		public abstract class createdByID : PX.Data.IBqlField
		{
		}
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		[PXUIField(DisplayName = "Created By")]
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
		[PXUIField(DisplayName = "Created Date")]
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
		[PXUIField(DisplayName = "Last Modified By")]
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
		[PXUIField(DisplayName = "Last Modified Date")]
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
