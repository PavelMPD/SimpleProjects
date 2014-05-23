using System;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;

namespace PX.Objects.EP
{
    [Serializable]
	public partial class EPTimeCardRecord : IBqlTable
	{
		#region TimeCardCD

		public abstract class timeCardCD : IBqlField { }

		[PXDBDefault(typeof(EPTimeCard.timeCardCD) )]
		[PXDBString(10, IsKey = true)]
		[PXUIField(Visible = false)]
		[PXParent(typeof(Select<EPTimeCard, Where<EPTimeCard.timeCardCD, Equal<Current<EPTimeCardRecord.timeCardCD>>>>))]
		public virtual String TimeCardCD { get; set; }

		#endregion
		#region LineNbr
		public abstract class lineNbr : IBqlField { }

		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(EPTimeCard))]
		[PXUIField(Visible = false)]
		public virtual Int32? LineNbr { get; set; }
		#endregion
		#region ProjectID
		public abstract class projectID : IBqlField { }
		[PXDefault]
		[EPTimeCardActiveProjectAttribute]
		public virtual Int32? ProjectID { get; set; }
		#endregion
		#region TaskID
		public abstract class taskID : IBqlField { }
		[PXDefault]
		[ActiveProjectTask(typeof(EPTimeCardRecord.projectID), BatchModule.EP, AllowNullIfContract = true, DisplayName = "Project Task")]
		public virtual Int32? TaskID { get; set; }
		#endregion
		#region TimeSpent
		public abstract class timeSpent : IBqlField { }

		[CRTimeSpan]
		[PXUIField(DisplayName = "Time Spent", Enabled = false)]
		public virtual Int32? TimeSpent { 
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
		[PXDefault("00:00")]
		[CRDBTimeSpan]
		[PXUIField(DisplayName = "Sun")]
		public virtual Int32? Sun { get; set; }
		#endregion
		#region Mon
		public abstract class mon : IBqlField { }

		[PXDefault("00:00")]
		[CRDBTimeSpan]
		[PXUIField(DisplayName = "Mon")]
		public virtual Int32? Mon { get; set; }
		#endregion
		#region Tue
		public abstract class tue : IBqlField { }
		[PXDefault("00:00")]
		[CRDBTimeSpan]
		[PXUIField(DisplayName = "Tue")]
		public virtual Int32? Tue { get; set; }
		#endregion
		#region Wed
		public abstract class wed : IBqlField { }
		[PXDefault("00:00")]
		[CRDBTimeSpan]
		[PXUIField(DisplayName = "Wed")]
		public virtual Int32? Wed { get; set; }
		#endregion
		#region Thu
		public abstract class thu : IBqlField { }
		[PXDefault("00:00")]
		[CRDBTimeSpan]
		[PXUIField(DisplayName = "Thu")]
		public virtual Int32? Thu { get; set; }
		#endregion
		#region Fri
		public abstract class fri : IBqlField { }
		[PXDefault("00:00")]
		[CRDBTimeSpan]
		[PXUIField(DisplayName = "Fri")]
		public virtual Int32? Fri { get; set; }
		#endregion
		#region Sat
		public abstract class sat : IBqlField { }
		[PXDefault("00:00")]
		[CRDBTimeSpan]
		[PXUIField(DisplayName = "Sat")]
		public virtual Int32? Sat { get; set; }
		#endregion
		#region OrigLineNbr
		public abstract class origLineNbr : IBqlField { }

		[PXDBInt]
		public virtual Int32? OrigLineNbr { get; set; }
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

	}

	  
}
