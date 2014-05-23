namespace SW.Objects.CR
{
	using System;
	using SW.Data;
	
	[System.SerializableAttribute()]
	public class CRQuestionaty : SW.Data.IBqlTable
	{
		#region ParameterID
		public abstract class parameterID : SW.Data.IBqlField
		{
		}
		protected String _ParameterID;
		[SWDBString(10, IsKey = true)]
		[SWDefault()]
		[SWUIField(DisplayName = "Parameter ID")]
		public virtual String ParameterID
		{
			get
			{
				return this._ParameterID;
			}
			set
			{
				this._ParameterID = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : SW.Data.IBqlField
		{
		}
		protected String _Description;
		[SWDBString(60, IsUnicode = true)]
		[SWDefault("")]
		[SWUIField(DisplayName = "Description")]
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
		#region Required
		public abstract class required : SW.Data.IBqlField
		{
		}
		protected Boolean? _Required;
		[SWDBBool()]
		[SWDefault(false)]
		[SWUIField(DisplayName = "Required")]
		public virtual Boolean? Required
		{
			get
			{
				return this._Required;
			}
			set
			{
				this._Required = value;
			}
		}
		#endregion
		#region SortOrder
		public abstract class sortOrder : SW.Data.IBqlField
		{
		}
		protected Int16? _SortOrder;
		[SWDBShort()]
		[SWUIField(DisplayName = "Sort Order")]
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
		#region ControlType
		public abstract class controlType : SW.Data.IBqlField
		{
		}
		protected Int32? _ControlType;
		[SWDBInt()]
		[SWDefault(0)]
		[SWUIField(DisplayName = "Control Type")]
		public virtual Int32? ControlType
		{
			get
			{
				return this._ControlType;
			}
			set
			{
				this._ControlType = value;
			}
		}
		#endregion
		#region EntryMask
		public abstract class entryMask : SW.Data.IBqlField
		{
		}
		protected String _EntryMask;
		[SWDBString(60)]
		[SWUIField(DisplayName = "Entry Mask")]
		public virtual String EntryMask
		{
			get
			{
				return this._EntryMask;
			}
			set
			{
				this._EntryMask = value;
			}
		}
		#endregion
		#region RegExp
		public abstract class regExp : SW.Data.IBqlField
		{
		}
		protected String _RegExp;
		[SWDBString(60, IsUnicode = true)]
		[SWUIField(DisplayName = "Reg Exp")]
		public virtual String RegExp
		{
			get
			{
				return this._RegExp;
			}
			set
			{
				this._RegExp = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : SW.Data.IBqlField
		{
		}
		protected Byte[] _tstamp;
		[SWDBTimestamp()]
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
		#region CreatedDateTime
		public abstract class createdDateTime : SW.Data.IBqlField
		{
		}
		protected DateTime? _CreatedDateTime;
		[SWDBCreatedDateTime()]
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
		#region CreatedByScreenID
		public abstract class createdByScreenID : SW.Data.IBqlField
		{
		}
		protected String _CreatedByScreenID;
		[SWDBCreatedByScreenID()]
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
		#region CreatedByID
		public abstract class createdByID : SW.Data.IBqlField
		{
		}
		protected Guid? _CreatedByID;
		[SWDBCreatedByID()]
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
		#region LastModifiedByID
		public abstract class lastModifiedByID : SW.Data.IBqlField
		{
		}
		protected Guid? _LastModifiedByID;
		[SWDBLastModifiedByID()]
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
		public abstract class lastModifiedByScreenID : SW.Data.IBqlField
		{
		}
		protected String _LastModifiedByScreenID;
		[SWDBLastModifiedByScreenID()]
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
		public abstract class lastModifiedDateTime : SW.Data.IBqlField
		{
		}
		protected DateTime? _LastModifiedDateTime;
		[SWDBLastModifiedDateTime()]
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
