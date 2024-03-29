using PX.Objects.CR;

namespace PX.Objects.CS
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(SegmentMaint))]
	public partial class Segment : PX.Data.IBqlTable
	{
		#region DimensionID
		public abstract class dimensionID : PX.Data.IBqlField
		{
		}
		protected String _DimensionID;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(Dimension.dimensionID))]
		[PXUIField(DisplayName = "Segmented Key ID", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<Dimension.dimensionID, Where<Dimension.dimensionID, InFieldClassActivated>>))]
		public virtual String DimensionID
		{
			get
			{
				return this._DimensionID;
			}
			set
			{
				this._DimensionID = value;
			}
		}
		#endregion
		#region SegmentID
		public abstract class segmentID : PX.Data.IBqlField
		{
		}
		protected Int16? _SegmentID;
		[PXDBShort(IsKey = true)]
//		[PXDefault((short) 0)]
		[PXUIField(DisplayName= "Segment ID", Visibility =PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<Segment.segmentID, Where<Segment.dimensionID,Equal<Current<Segment.dimensionID >>>>))]
		[PXParent(typeof(Select<Dimension,Where<Dimension.dimensionID, Equal<Current<Segment.dimensionID>>>>))]
		public virtual Int16? SegmentID
		{
			get
			{
				return this._SegmentID;
			}
			set
			{
				this._SegmentID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected String _Descr;
		[PXDBString(60, IsUnicode = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "Description", Visibility=PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region Length
		public abstract class length : PX.Data.IBqlField
		{
		}
		protected Int16? _Length;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Length", Visibility=PXUIVisibility.Visible)]
		public virtual Int16? Length
		{
			get
			{
				return this._Length;
			}
			set
			{
				this._Length = value;
			}
		}
		#endregion
		#region Align
		public abstract class align : PX.Data.IBqlField
		{
		}
		protected Int16? _Align;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Align", Visibility = PXUIVisibility.Visible)]
		[PXIntList(new int[] { 0, 1 }, new string[] { "Left", "Right" })]
		public virtual Int16? Align
		{
			get
			{
				return this._Align;
			}
			set
			{
				this._Align = value;
			}
		}
		#endregion
		#region FillCharacter
		public abstract class fillCharacter : PX.Data.IBqlField
		{
		}
		protected String _FillCharacter;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(" ")]
		[PXUIField(DisplayName = "Fill Character", Visibility = PXUIVisibility.Visible)]
		[PXStringList(" ;Blanks,0;Zero")]
		public virtual String FillCharacter
		{
			get
			{
				return this._FillCharacter;
			}
			set
			{
				this._FillCharacter = value;
			}
		}
		#endregion
		#region EditMask
		public abstract class editMask : PX.Data.IBqlField
		{
		}
		protected String _EditMask;
		[PXDBString(1, IsFixed = true)]
		[PXDefault("a")]
		[PXUIField(DisplayName = "Edit Mask", Visibility = PXUIVisibility.Visible)]
		[SegmentEditMask]
		public virtual String EditMask
		{
			get
			{
				return this._EditMask;
			}
			set
			{
				this._EditMask = value;
			}
		}
		#endregion
		#region CaseConvert
		public abstract class caseConvert : PX.Data.IBqlField
		{
		}
		protected Int16? _CaseConvert;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Case Conversion", Visibility = PXUIVisibility.Visible)]
		[PXIntList(new int[] { 0, 1, 2 }, new string[] { "No Change", "Uppercase", "Lowercase" })]
		public virtual Int16? CaseConvert
		{
			get
			{
				return this._CaseConvert;
			}
			set
			{
				this._CaseConvert = value;
			}
		}
		#endregion
		#region Validate
		public abstract class validate : PX.Data.IBqlField
		{
		}
		protected Boolean? _Validate;
		[PXDBBool()]
		[PXDefault((bool) false)]
		[PXUIField(DisplayName = "Validate", Visibility=PXUIVisibility.Visible)]
		public virtual Boolean? Validate
		{
			get
			{
				return this._Validate;
			}
			set
			{
				this._Validate = value;
			}
		}
		#endregion
		#region AutoNumber
		public abstract class autoNumber : PX.Data.IBqlField
		{
		}
		protected Boolean? _AutoNumber;
		[PXDBBool()]
		[PXDefault((bool) false)]
		[PXUIField(DisplayName = "Auto Number", Visibility=PXUIVisibility.Visible)]
		public virtual Boolean? AutoNumber
		{
			get
			{
				return this._AutoNumber;
			}
			set
			{
				this._AutoNumber = value;
			}
		}
		#endregion
		#region Separator
		public abstract class separator : PX.Data.IBqlField
		{
		}
		protected String _Separator;
		[PXDBString(1, IsFixed = true)]
		[PXDefault("-")]
		[PXUIField(DisplayName = "Separator", Visibility = PXUIVisibility.Visible)]
		public virtual String Separator
		{
			get
			{
				return this._Separator;
			}
			set
			{
				this._Separator = value;
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
		#region ConsolOrder
		public abstract class consolOrder : PX.Data.IBqlField
		{
		}
		protected Int16? _ConsolOrder;
		[PXDefault((short)0)]
		[PXDBShort()]
		[PXUIField(DisplayName = "Consol. Order", Visible=false)]
		public virtual Int16? ConsolOrder
		{
			get
			{
				return this._ConsolOrder;
			}
			set
			{
				this._ConsolOrder = value;
			}
		}
		#endregion
		#region ConsolNumChar
		public abstract class consolNumChar : PX.Data.IBqlField
		{
		}
		protected Int16? _ConsolNumChar;
		[PXDefault((short)0)]
		[PXDBShort()]
		[PXUIField(DisplayName = "Number of characters", Visible=false)]
		public virtual Int16? ConsolNumChar
		{
			get
			{
				return this._ConsolNumChar;
			}
			set
			{
				this._ConsolNumChar = value;
			}
		}
		#endregion
		#region IsCosted
		public abstract class isCosted : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsCosted;
		[PXDBBool()]
		[PXDefault((bool)false)]
		[PXUIField(DisplayName = "Include in Cost", Visible=false)]
		public virtual Boolean? IsCosted
		{
			get
			{
				return this._IsCosted;
			}
			set
			{
				this._IsCosted = value;
			}
		}
		#endregion

		#region ParentDimensionID
		public abstract class parentDimensionID : IBqlField{}
        [PXDBString(15, IsUnicode = true)]
        public virtual string ParentDimensionID { get; set; }
		#endregion


		#region Inherited
		public abstract class inherited : IBqlField{}
		[PXBool]
		public virtual Boolean? Inherited { get; set; }
		#endregion

		#region IsOverrideForUI

		public abstract class isOverrideForUI : IBqlField
		{
		}

		[PXBool]
		[PXUIField(DisplayName = "Override", Visible = false, IsReadOnly = true)]
		public virtual Boolean? IsOverrideForUI
		{
			[PXDependsOnFields(typeof(inherited))]
			get { return Inherited != true; }
		}

		#endregion
	}
}
