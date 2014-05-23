using System;
using PX.Data;

namespace PX.Objects.CR
{
	[Serializable]
	public class CRActivityStatistics : IBqlTable
	{
		#region NoteID
		public abstract class noteID : IBqlField {}
		[PXDBLong(IsKey = true)]
		public virtual Int64? NoteID { get; set; }
		#endregion

		#region LastIncomingActivityID
		public abstract class lastIncomingActivityID : IBqlField { }
		[PXDBInt]
		[PXDBDefault(typeof(EPActivity.taskID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? LastIncomingActivityID { get; set; }
		#endregion

		#region LastOutgoingActivityID
		public abstract class lastOutgoingActivityID : IBqlField { }
		[PXDBInt]
		[PXDBDefault(typeof(EPActivity.taskID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? LastOutgoingActivityID { get; set; }
		#endregion

		#region LastIncomingActivityDate
		public abstract class lastIncomingActivityDate : IBqlField { }
		[PXDBDate(UseSmallDateTime = false)]
		[PXUIField(DisplayName = "Last Incoming Activity Date", Enabled = false)]
		public virtual DateTime? LastIncomingActivityDate { get; set; }
		#endregion

		#region LastOutgoingActivityDate
		public abstract class lastOutgoingActivityDate : IBqlField { }
		[PXDBDate(UseSmallDateTime = false)]
		[PXUIField(DisplayName = "Last Outgoing Activity Date", Enabled = false)]
		public virtual DateTime? LastOutgoingActivityDate { get; set; }
		#endregion

		#region LastActivityDate
		public abstract class lastActivityDate : IBqlField { }
		[PXDBCalced(typeof(Switch<Case<Where<lastIncomingActivityDate, IsNotNull, And<lastOutgoingActivityDate, IsNull>>, lastIncomingActivityDate,
			Case<Where<lastOutgoingActivityDate, IsNotNull, And<lastIncomingActivityDate, IsNull>>, lastOutgoingActivityDate,
			Case<Where<lastIncomingActivityDate, Greater<lastOutgoingActivityDate>>, lastIncomingActivityDate>>>, lastOutgoingActivityDate>), typeof(DateTime))]
		[PXUIField(DisplayName = "Last Activity Date", Enabled = false)]
		public virtual DateTime? LastActivityDate { get; set; }
		#endregion
	}
}
