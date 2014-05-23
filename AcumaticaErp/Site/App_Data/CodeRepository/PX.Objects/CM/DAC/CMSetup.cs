using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.CM
{
    [PXPrimaryGraph(typeof(CMSetupMaint))]
	[System.SerializableAttribute()]
	public partial class CMSetup : PX.Data.IBqlTable
	{
		#region BatchNumberingID
		public abstract class batchNumberingID : IBqlField
		{
		}
		protected String _BatchNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("BATCH")]
		[PXSelector(typeof(Numbering.numberingID))]
		[PXUIField(DisplayName = "Batch Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String BatchNumberingID
		{
			get
			{
				return _BatchNumberingID;
			}
			set
			{
				_BatchNumberingID = value;
			}
		}
		#endregion
        #region ExtRefNbrNumberingID
        public abstract class extRefNbrNumberingID : IBqlField
        {
        }
        protected String _ExtRefNbrNumberingID;
        [PXDBString(10, IsUnicode = true)]      
        [PXSelector(typeof(Numbering.numberingID))]
        [PXUIField(DisplayName = "Batch Ref. Number Numbering Sequence", Visibility = PXUIVisibility.Visible)]
        public virtual String ExtRefNbrNumberingID
        {
            get
            {
                return _ExtRefNbrNumberingID;
            }
            set
            {
                _ExtRefNbrNumberingID = value;
            }
        }
        #endregion
		#region APCuryOverride
		public abstract class aPCuryOverride : PX.Data.IBqlField
		{
		}
		protected Boolean? _APCuryOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Vendor CurrencyID Override")]
		public virtual Boolean? APCuryOverride
		{
			get
			{
				return this._APCuryOverride;
			}
			set
			{
				this._APCuryOverride = value;
			}
		}
		#endregion
		#region APRateTypeDflt
		public abstract class aPRateTypeDflt : PX.Data.IBqlField
		{
		}
		protected String _APRateTypeDflt;
		[PXDBString(6, IsUnicode = true)]
		[PXDefault(typeof(Search<CurrencyRateType.curyRateTypeID>))]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "AP Rate Type")]
		public virtual String APRateTypeDflt
		{
			get
			{
				return this._APRateTypeDflt;
			}
			set
			{
				this._APRateTypeDflt = value;
			}
		}
		#endregion
		#region APRateTypeOverride
		public abstract class aPRateTypeOverride : PX.Data.IBqlField
		{
		}
		protected Boolean? _APRateTypeOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Vendor Rate Type Override")]
		public virtual Boolean? APRateTypeOverride
		{
			get
			{
				return this._APRateTypeOverride;
			}
			set
			{
				this._APRateTypeOverride = value;
			}
		}
		#endregion
		#region APRateTypeReval
		public abstract class aPRateTypeReval : PX.Data.IBqlField
		{
		}
		protected String _APRateTypeReval;
		[PXDBString(6, IsUnicode = true)]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "AP Revaluation Rate Type")]
		public virtual String APRateTypeReval
		{
			get
			{
				return this._APRateTypeReval;
			}
			set
			{
				this._APRateTypeReval = value;
			}
		}
		#endregion
		#region ARCuryOverride
		public abstract class aRCuryOverride : PX.Data.IBqlField
		{
		}
		protected Boolean? _ARCuryOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Customer Currency ID Override")]
		public virtual Boolean? ARCuryOverride
		{
			get
			{
				return this._ARCuryOverride;
			}
			set
			{
				this._ARCuryOverride = value;
			}
		}
		#endregion
		#region ARRateTypeDflt
		public abstract class aRRateTypeDflt : PX.Data.IBqlField
		{
		}
		protected String _ARRateTypeDflt;
		[PXDBString(6, IsUnicode = true)]
		[PXDefault(typeof(Search<CurrencyRateType.curyRateTypeID>))]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "AR Rate Type")]
		public virtual String ARRateTypeDflt
		{
			get
			{
				return this._ARRateTypeDflt;
			}
			set
			{
				this._ARRateTypeDflt = value;
			}
		}
		#endregion
		#region ARRateTypePrc
		public abstract class aRRateTypePrc : PX.Data.IBqlField
		{
		}
		protected String _ARRateTypePrc;
		[PXDBString(6, IsUnicode = true)]
		[PXDefault(typeof(Search<CurrencyRateType.curyRateTypeID>))]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "Sales Price Rate Type ")]
		public virtual String ARRateTypePrc
		{
			get
			{
				return this._ARRateTypePrc;
			}
			set
			{
				this._ARRateTypePrc = value;
			}
		}
		#endregion
		#region ARRateTypeOverride
		public abstract class aRRateTypeOverride : PX.Data.IBqlField
		{
		}
		protected Boolean? _ARRateTypeOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Customer Rate Type Override")]
		public virtual Boolean? ARRateTypeOverride
		{
			get
			{
				return this._ARRateTypeOverride;
			}
			set
			{
				this._ARRateTypeOverride = value;
			}
		}
		#endregion
		#region ARRateTypeReval
		public abstract class aRRateTypeReval : PX.Data.IBqlField
		{
		}
		protected String _ARRateTypeReval;
		[PXDBString(6, IsUnicode = true)]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "AR Revaluation Rate Type")]
		public virtual String ARRateTypeReval
		{
			get
			{
				return this._ARRateTypeReval;
			}
			set
			{
				this._ARRateTypeReval = value;
			}
		}
		#endregion
		
		#region CARateTypeDflt
		public abstract class cARateTypeDflt : PX.Data.IBqlField
		{
		}
		protected String _CARateTypeDflt;
		[PXDBString(6, IsUnicode = true)]
		[PXDefault(typeof(Search<CurrencyRateType.curyRateTypeID>))]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "CA Rate Type")]
		public virtual String CARateTypeDflt
		{
			get
			{
				return this._CARateTypeDflt;
			}
			set
			{
				this._CARateTypeDflt = value;
			}
		}
		#endregion
		
		#region GLRateTypeDflt
		public abstract class gLRateTypeDflt : PX.Data.IBqlField
		{
		}
		protected String _GLRateTypeDflt;
		[PXDBString(6, IsUnicode = true)]
		[PXDefault(typeof(Search<CurrencyRateType.curyRateTypeID>))]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "GL Rate Type")]
		public virtual String GLRateTypeDflt
		{
			get
			{
				return this._GLRateTypeDflt;
			}
			set
			{
				this._GLRateTypeDflt = value;
			}
		}
		#endregion
		#region GLRateTypeReval
		public abstract class gLRateTypeReval : PX.Data.IBqlField
		{
		}
		protected String _GLRateTypeReval;
		[PXDBString(6, IsUnicode = true)]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "GL Revaluation Rate Type")]
		public virtual String GLRateTypeReval
		{
			get
			{
				return this._GLRateTypeReval;
			}
			set
			{
				this._GLRateTypeReval = value;
			}
		}
		#endregion

		#region MCActivated
		public abstract class mCActivated : PX.Data.IBqlField
		{
		}
		protected Boolean? _MCActivated;
		[PXBool()]
		[PXUIField(Visible = false)]
		public virtual Boolean? MCActivated
		{
			get
			{
				return PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();
			}			
		}
		#endregion
		#region RateVariance
		public abstract class rateVariance : PX.Data.IBqlField
		{
		}
		protected Decimal? _RateVariance;
		[PXDBDecimal(0)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Rate Variance Allowed, %")]
		public virtual Decimal? RateVariance
		{
			get
			{
				return this._RateVariance;
			}
			set
			{
				this._RateVariance = value;
			}
		}
		#endregion
		#region RateVarianceWarn
		public abstract class rateVarianceWarn : PX.Data.IBqlField
		{
		}
		protected Boolean? _RateVarianceWarn;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Warn About Rate Variance")]
		public virtual Boolean? RateVarianceWarn
		{
			get
			{
				return this._RateVarianceWarn;
			}
			set
			{
				this._RateVarianceWarn = value;
			}
		}
		#endregion
		#region AutoPostOption
		public abstract class autoPostOption : PX.Data.IBqlField
		{
		}
		protected Boolean? _AutoPostOption;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Automatically Post to GL on Release", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? AutoPostOption
		{
			get
			{
				return this._AutoPostOption;
			}
			set
			{
				this._AutoPostOption = value;
			}
		}
		#endregion

		#region Translation Setup
        #region TranslDefId
        public abstract class translDefId : PX.Data.IBqlField
        {
        }
        protected String _TranslDefId;
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Default Translation ID")]
        [PXSelector(typeof(TranslDef.translDefId),
           DescriptionField = typeof(TranslDef.description))]
        public virtual String TranslDefId
        {
            get
            {
                return this._TranslDefId;
            }
            set
            {
                this._TranslDefId = value;
            }
        }
        #endregion
        #region RetainPeriodsNumber
        public abstract class retainPeriodsNumber : PX.Data.IBqlField
        {
        }
        protected Int16? _RetainPeriodsNumber;
        [PXDBShort()]
		[PXDefault((short)99)]
        [PXUIField(DisplayName = "Keep History For Periods")]
        public virtual Int16? RetainPeriodsNumber
        {
            get
            {
                return this._RetainPeriodsNumber;
            }
            set
            {
                this._RetainPeriodsNumber = value;
            }
        }
        #endregion
        #region TranslNumberingID
        public abstract class translNumberingID : PX.Data.IBqlField
        {
        }
        protected String _TranslNumberingID;
        [PXDBString(10, IsUnicode = true)]
    	[PXDefault("TRANSLAT")]
        [PXUIField(DisplayName = "Translation Numbering Sequence")]
        [PXSelector(typeof(Numbering.numberingID))]
        public virtual String TranslNumberingID
        {
            get
            {
                return this._TranslNumberingID;
            }
            set
            {
                this._TranslNumberingID = value;
            }
        }
        #endregion
		#endregion
        #region GainLossSubMask
        public abstract class gainLossSubMask : PX.Data.IBqlField
        {
        }
        protected String _GainLossSubMask;
        [PXDefault()]
        [GainLossSubAccountMask(DisplayName = "Combine Gain/Loss Sub. from")]
        public virtual String GainLossSubMask
        {
            get
            {
                return this._GainLossSubMask;
            }
            set
            {
                this._GainLossSubMask = value;
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
