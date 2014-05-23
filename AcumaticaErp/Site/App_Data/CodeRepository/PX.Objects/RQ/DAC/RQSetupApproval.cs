using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;

namespace PX.Objects.RQ
{
    [Serializable]
	public partial class RQSetupApproval : PX.Data.IBqlTable
	{
		#region ApprovalID
		public abstract class approvalID : PX.Data.IBqlField
		{
		}
		protected int? _ApprovalID;
		[PXDBIdentity(IsKey = true)]
		public virtual int? ApprovalID
		{
			get
			{
				return this._ApprovalID;
			}
			set
			{
				this._ApprovalID = value;
			}
		}
		#endregion
		#region Type
		public abstract class type : PX.Data.IBqlField
		{
		}
		protected String _Type;
		[PXDefault]
		[PXDBString(1, IsFixed = true)]		
		[RQType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true)]		
		public virtual String Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}
		#endregion
		#region AssignmentMapID
		public abstract class assignmentMapID : PX.Data.IBqlField
		{
		}
		protected int? _AssignmentMapID;
		[PXDefault]
		[PXDBInt()]
		[PXSelector(typeof(Search<EPAssignmentMap.assignmentMapID, 
			Where2<
				Where<Current<RQSetupApproval.type>, Equal<RQType.requestItem>,
				 And<EPAssignmentMap.entityType, Equal<AssignmentMapType.AssignmentMapTypePurchaseRequestItem>>>,
			Or<
				Where<Current<RQSetupApproval.type>, Equal<RQType.requisition>,
			 And<EPAssignmentMap.entityType, Equal<AssignmentMapType.AssignmentMapTypePurchaseRequisition>>>
			>>>), DescriptionField = typeof(EPAssignmentMap.name))]
		[PXUIField(DisplayName = "Assignment Map")]
		[PXCheckUnique(typeof(RQSetupApproval.type))]
		public virtual int? AssignmentMapID
		{
			get
			{
				return this._AssignmentMapID;
			}
			set
			{
				this._AssignmentMapID = value;
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
	}

	public class RQType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { RequestItem, Requisition},
				new string[] { Messages.RQRequest, Messages.RQRequisition }) { }
		}
	
		public const string RequestItem = "I";
		public const string Requisition = "R";

		public class requestItem : Constant<string>
		{
			public requestItem() : base(RequestItem) { ;}
		}

		public class requisition : Constant<string>
		{
			public requisition() : base(Requisition) { ;}
		}		
	}
}
