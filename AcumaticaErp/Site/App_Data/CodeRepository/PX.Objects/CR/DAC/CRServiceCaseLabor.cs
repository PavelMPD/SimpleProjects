using System;
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.CR
{
	[PXCacheName(Messages.ServiceCallLabor)]
	[Serializable]
	public partial class CRServiceCaseLabor : IBqlTable
	{
		#region ServiceCaseID
		public abstract class serviceCaseID : IBqlField { }

		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(CRServiceCase.serviceCaseID))]
		[PXNavigateSelector(typeof(CRServiceCase.serviceCaseID))]
		public virtual Int32? ServiceCaseID { get; set; }
		#endregion

		#region EmployeeID

		public abstract class employeeID : IBqlField { }

		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Employee ID")]
		[PXSelector(typeof(Search<EPEmployee.bAccountID>), DescriptionField = typeof(EPEmployee.acctCD))]
		public virtual Int32? EmployeeID { get; set; }

		#endregion

		#region Date

		public abstract class startDate : IBqlField { }

		[PXDBDate(PreserveTime = true, UseTimeZone = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Start Date")]
		public virtual DateTime? StartDate { get; set; }

		#endregion

		#region TimeSpent
		public abstract class timeSpent : IBqlField { }

		[PXDBTimeSpan]
		[PXDefault]
		[PXUIField(DisplayName = "Time Worked")]
		public virtual Int32? TimeSpent { get; set; }
		#endregion

		#region OvertimeSpent
		public abstract class overtimeSpent : IBqlField { }

		[PXDBTimeSpan]
		[PXDefault]
		[PXUIField(DisplayName = "Overtime Worked", Enabled = false)]
		public virtual Int32? OvertimeSpent { get; set; }
		#endregion

		#region TimeBillable
		public abstract class timeBillable : IBqlField { }

		[PXDBTimeSpan]
		[PXDefault]
		[PXUIField(DisplayName = "Billable Time")]
		public virtual Int32? TimeBillable { get; set; }
		#endregion

		#region OvertimeBillable
		public abstract class overtimeBillable : IBqlField { }

		[PXDBTimeSpan]
		[PXDefault]
		[PXUIField(DisplayName = "Billable Overtime")]
		public virtual Int32? OvertimeBillable { get; set; }
		#endregion

		#region Description

		public abstract class description : IBqlField { }

		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual String Description { get; set; }

		#endregion

		#region NoteID
		public abstract class noteID : IBqlField { }

		[PXNote]
		public virtual Int64? NoteID { get; set; }
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
		[PXDBCreatedDateTimeUtc]
		[PXUIField(DisplayName = "Service Call Date", Enabled = false)]
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
		[PXDBLastModifiedDateTimeUtc]
		[PXUIField(DisplayName = "Last Activity", Enabled = false)]
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
