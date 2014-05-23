using System;
using System.Diagnostics;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.TM;

namespace PX.Objects.EP
{
	[Serializable]
	[DebuggerDisplay("EventID = {EventID}, Email = {Email}")]
	public partial class EPOtherAttendee : IBqlTable
	{
		#region EventID
		public abstract class eventID : IBqlField { }

		protected Int32? _EventID;
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(EPActivity.taskID))]
		public virtual Int32? EventID
		{
			get { return _EventID; }
			set { _EventID = value; }
		}
		#endregion

		#region AttendeeID

		public abstract class attendeeID : IBqlField { }

		protected Int32? _AttendeeID;

		[PXDBIdentity(IsKey = true)]
		[PXUIField(Visible = false)]
		public virtual Int32? AttendeeID
		{
			get { return _AttendeeID; }
			set { _AttendeeID = value; }
		}

		#endregion

		#region Email
		public abstract class email : IBqlField { }

		protected String _Email;
		[PXDBString]
		public virtual String Email
		{
			get { return _Email; }
			set { _Email = value; }
		}
		#endregion

		#region Name

		public abstract class name : IBqlField { }

		protected String _Name;

		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Name")]
		public virtual String Name
		{
			get { return _Name; }
			set { _Name = value; }
		}

		#endregion

		#region Comment

		public abstract class comment : IBqlField { }

		protected String _Comment;

		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Comment")]
		public virtual String Comment
		{
			get { return _Comment; }
			set { _Comment = value; }
		}

		#endregion

		#region Invitation

		public abstract class invitation : IBqlField { }

		protected Int32? _Invitation;

		[PXDBInt]
		[PXUIField(DisplayName = "Invitation", Enabled = false)]
		[PXDefault(PXInvitationStatusAttribute.NOTINVITED)]
		[PXInvitationStatus]
		public virtual Int32? Invitation
		{
			get { return _Invitation; }
			set { _Invitation = value; }
		}

		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.IBqlField
		{
		}
		protected Guid? _CreatedByID;
		[PXDBCreatedByID]
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
		[PXDBCreatedDateTime]
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
		[PXDBLastModifiedDateTime]
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
}
