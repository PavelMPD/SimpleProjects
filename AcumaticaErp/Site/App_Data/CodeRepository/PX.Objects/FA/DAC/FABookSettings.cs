using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.FA
{
	[Serializable]
	public partial class FABookSettings : IBqlTable
	{
		#region BookID
		public abstract class bookID : PX.Data.IBqlField
		{
		}
		protected Int32? _BookID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXSelector(typeof(FABook.bookID), SubstituteKey = typeof(FABook.bookCode), DescriptionField = typeof (FABook.description))]
		[PXUIField(DisplayName = "Book")]
		public virtual Int32? BookID
		{
			get
			{
				return this._BookID;
			}
			set
			{
				this._BookID = value;
			}
		}
		#endregion
		#region AssetID
		public abstract class assetID : PX.Data.IBqlField
		{
		}
		protected Int32? _AssetID;
		[PXDBInt(IsKey = true)]
		[PXDBLiteDefault(typeof(FixedAsset.assetID))]
		[PXParent(typeof(Select<FixedAsset, Where<FixedAsset.recordType, Equal<FARecordType.classType>, And<FixedAsset.assetID, Equal<Current<FABookSettings.assetID>>>>>), UseCurrent = true, LeaveChildren = false)]
		[PXUIField(DisplayName = "Asset Class", Visibility = PXUIVisibility.Invisible, Visible = false)]
		public virtual Int32? AssetID
		{
			get
			{
				return this._AssetID;
			}
			set
			{
				this._AssetID = value;
			}
		}
		#endregion
		#region Depreciate
		public abstract class depreciate : PX.Data.IBqlField
		{
		}
		protected Boolean? _Depreciate;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Depreciate", Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual Boolean? Depreciate
		{
			get
			{
				return this._Depreciate;
			}
			set
			{
				this._Depreciate = value;
			}
		}
		#endregion
		#region UpdateGL
		public abstract class updateGL : PX.Data.IBqlField
		{
		}
		protected Boolean? _UpdateGL;
        [PXBool]
		[PXDBScalar(typeof(Search<FABook.updateGL, Where<FABook.bookID, Equal<FABookSettings.bookID>>>))]
		[PXDefault(false, typeof(Search<FABook.updateGL, Where<FABook.bookID, Equal<Current<FABookSettings.bookID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Update GL", Enabled = false)]
		public virtual Boolean? UpdateGL
		{
			get
			{
				return this._UpdateGL;
			}
			set
			{
				this._UpdateGL = value;
			}
		}
		#endregion
		#region DepreciationMethodID
		public abstract class depreciationMethodID : PX.Data.IBqlField
		{
		}
		protected Int32? _DepreciationMethodID;
		[PXDBInt]
		[PXSelector(typeof(Search<FADepreciationMethod.methodID,
			Where2<Where<FADepreciationMethod.usefulLife, Equal<Current<usefulLife>>,
				Or<FADepreciationMethod.usefulLife, IsNull,
                Or<FADepreciationMethod.usefulLife, Equal<decimal0>>>>,
			And<Where<FADepreciationMethod.recordType, NotEqual<FARecordType.assetType>>>>>),
					typeof(FADepreciationMethod.methodCD), 
					typeof(FADepreciationMethod.depreciationMethod),
					typeof(FADepreciationMethod.usefulLife),
					typeof(FADepreciationMethod.recoveryPeriod),
					typeof(FADepreciationMethod.averagingConvention),
					typeof(FADepreciationMethod.recordType),
					typeof(FADepreciationMethod.description),
					SubstituteKey = typeof(FADepreciationMethod.methodCD),
					DescriptionField = typeof(FADepreciationMethod.description)
			)]
		[PXUIField(DisplayName = "Class Method", Visibility = PXUIVisibility.Visible)]
		public virtual Int32? DepreciationMethodID
		{
			get
			{
				return this._DepreciationMethodID;
			}
			set
			{
				this._DepreciationMethodID = value;
			}
		}
		#endregion
		#region MidMonthType
		public abstract class midMonthType : IBqlField
		{
		}
		protected string _MidMonthType;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Mid-Period Type")]
		[PXFormula(typeof(Selector<FABookSettings.bookID, FABook.midMonthType>))]
		[FABook.midMonthType.List]
		public virtual string MidMonthType
		{
			get
			{
				return _MidMonthType;
			}
			set
			{
				_MidMonthType = value;
			}
		}
		#endregion
		#region MidMonthDay
		public abstract class midMonthDay : IBqlField
		{
		}
		protected short? _MidMonthDay;
		[PXDBShort]
		[PXUIField(DisplayName = "Mid-Period Day")]
		[PXFormula(typeof(Selector<FABookSettings.bookID, FABook.midMonthDay>))]
		public virtual short? MidMonthDay
		{
			get
			{
				return _MidMonthDay;
			}
			set
			{
				_MidMonthDay = value;
			}
		}
		#endregion
		#region Bonus
		public abstract class bonus : PX.Data.IBqlField
		{
		}
		protected Boolean? _Bonus;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Bonus")]
		public virtual Boolean? Bonus
		{
			get
			{
				return this._Bonus;
			}
			set
			{
				this._Bonus = value;
			}
		}
		#endregion
		#region Sect179
		public abstract class sect179 : PX.Data.IBqlField
		{
		}
		protected Boolean? _Sect179;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Sect. 179")]
		public virtual Boolean? Sect179
		{
			get
			{
				return this._Sect179;
			}
			set
			{
				this._Sect179 = value;
			}
		}
		#endregion
		#region AveragingConvention
		public abstract class averagingConvention : IBqlField
		{
		}
		protected string _AveragingConvention;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Averaging Convention")]
		[PXDefault(typeof(Search<FADepreciationMethod.averagingConvention, Where<FADepreciationMethod.methodID, Equal<Current<depreciationMethodID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[FAAveragingConvention.List]
		public virtual string AveragingConvention
		{
			get
			{
				return _AveragingConvention;
			}
			set
			{
				_AveragingConvention = value;
			}
		}
		#endregion
		#region UsefulLife
		public abstract class usefulLife : PX.Data.IBqlField
		{
		}
		protected Decimal? _UsefulLife;
		[PXDBDecimal(4)]
		[PXDefault(typeof(FixedAsset.usefulLife))]
		[PXUIField(DisplayName = "Useful Life, Years")]
		public virtual Decimal? UsefulLife
		{
			get
			{
				return this._UsefulLife;
			}
			set
			{
				this._UsefulLife = value;
			}
		}
		#endregion
		#region RecoveryPeriod
		public abstract class recoveryPeriod : IBqlField
		{
		}
		protected Int32? _RecoveryPeriod;
		[PXDBInt]
		[PXUIField(DisplayName = "Recovery Period", Visible = false)]
		[PXFormula(typeof(CalcRecoveryPeriods<bookID, usefulLife>))]
		public virtual Int32? RecoveryPeriod
		{
			get
			{
				return _RecoveryPeriod;
			}
			set
			{
				_RecoveryPeriod = value;
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
}
