using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.PM
{
	[PXPrimaryGraph(typeof(AccountGroupMaint))]
    [Serializable]
	public partial class PMAccountGroup : IBqlTable, IAttributeSupport
	{
		#region GroupID
		public abstract class groupID : PX.Data.IBqlField
		{
		}
		protected Int32? _GroupID;
		[PXDBIdentity()]
		[PXSelector(typeof(PMAccountGroup.groupID))]
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
		#region GroupCD
		public abstract class groupCD : PX.Data.IBqlField
		{
		}
		protected String _GroupCD;
		[PXDimensionSelector(AccountGroupAttribute.DimensionName,
			typeof(Search<PMAccountGroup.groupCD>),
			typeof(PMAccountGroup.groupCD),
			typeof(PMAccountGroup.groupCD), typeof(PMAccountGroup.description), typeof(PMAccountGroup.type), typeof(PMAccountGroup.isActive), DescriptionField = typeof(PMTask.description))]
		[PXDBString(IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Account Group ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String GroupCD
		{
			get
			{
				return this._GroupCD;
			}
			set
			{
				this._GroupCD = value;
			}
		}
		#endregion
		
		#region Description
		public abstract class description : PX.Data.IBqlField
		{
		}
		protected String _Description;
		[PXDBString(250, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
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
		#region IsActive
		public abstract class isActive : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsActive;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				this._IsActive = value;
			}
		}
		#endregion		
		#region Type
		public abstract class type : PX.Data.IBqlField
		{
		}
		protected string _Type;
		[PXDBString(1)]
		[PXDefault(GL.AccountType.Asset)]
		[PMAccountType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string Type
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
		#region SortOrder
		public abstract class sortOrder : PX.Data.IBqlField
		{
		}
		protected Int16? _SortOrder;
		[PXDBShort()]
		[PXUIField(DisplayName = "Sort Order")]
		public virtual Int16? SortOrder
		{
			get
			{
				return this._SortOrder;
			}
			set
			{
				this._SortOrder = value;
			}
		}
		#endregion
		#region Attributes
		[CRAttributesField(typeof(CSAnswerType.accountGroupAnswerType), typeof(PMAccountGroup.groupID), typeof(PMAccountGroup.classID))]
		public virtual string[] Attributes { get; set; }

		public abstract class classID : IBqlField
		{
		}
		[PXString(20)]
		public virtual string ClassID
		{
			get { return GroupTypes.AccountGroup; }
		}

		public virtual string EntityType
		{
			get { return CSAnswerType.AccountGroup; }
		}

		public virtual int? ID
		{
			get { return GroupID; }
		}
		#endregion
		
		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.IBqlField
		{
		}
		protected Int64? _NoteID;
        [PXNote]
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

	public static class PMAccountType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { GL.AccountType.Asset, GL.AccountType.Liability, GL.AccountType.Income, GL.AccountType.Expense, OffBalance },
				new string[] { GL.Messages.Asset, GL.Messages.Liability, GL.Messages.Income, GL.Messages.Expense, Messages.OffBalance }) { }

			
		}

		public const string OffBalance = "O"; 
		public class offBalance : Constant<string>
		{
			public offBalance() : base(OffBalance) { ;}
		}
				
	}

	public static class AccountGroupStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Active },
				new string[] { Messages.Active }) { ; }
		}
		public const string Active = "A";
	}
}
