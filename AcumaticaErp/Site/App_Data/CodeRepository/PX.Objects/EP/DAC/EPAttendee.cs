using System;
using System.Diagnostics;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.TM;

namespace PX.Objects.EP
{
	[Serializable]
	[DebuggerDisplay("EventID = {EventID}, UserID = {UserID}")]
	public partial class EPAttendee : IBqlTable
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

		#region UserID
		public abstract class userID : IBqlField { }

		protected Guid? _UserID;
		[PXDBGuid(IsKey = true)]
		public virtual Guid? UserID
		{
			get { return _UserID; }
			set { _UserID = value; }
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
