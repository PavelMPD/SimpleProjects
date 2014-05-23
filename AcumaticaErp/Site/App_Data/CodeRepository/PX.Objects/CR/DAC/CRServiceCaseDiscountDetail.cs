using System;
using PX.Data;

namespace PX.Objects.CR
{
	[Serializable]
	[PXCacheName(Messages.DiscountDetail)]
	public partial class CRServiceCaseDiscountDetail : IBqlTable, SO.IDiscountDetail
	{
        #region ServiceCaseID
		public abstract class serviceCaseID : IBqlField { }

		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(CRServiceCase.serviceCaseID))]
		[PXParent(typeof(Select<CRServiceCase,
			Where<CRServiceCase.serviceCaseID, Equal<Current<CRServiceCaseDiscountDetail.serviceCaseID>>>>))]
		[PXUIField(Visible = true)]
		public virtual Int32? ServiceCaseID { get; set; }
		#endregion

		#region DiscountID
		public abstract class discountID : IBqlField { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Discount ID")]
		public virtual String DiscountID { get; set; }
		#endregion

		#region DiscountSequenceID
		public abstract class discountSequenceID : IBqlField { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Sequence ID")]
		public virtual String DiscountSequenceID { get; set; }
		#endregion

		#region Type
		public abstract class type : IBqlField { }

		[PXDBString(1, IsKey = true, IsFixed = true)]
		[PXDefault]
		[SO.DiscountType.List]
		[PXUIField(DisplayName = "Type")]
		public virtual String Type { get; set; }
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : IBqlField { }

		[PXDBLong]
		[PXDBDefault(typeof(CRServiceCase.curyInfoID))]
		public virtual Int64? CuryInfoID { get; set; }
		#endregion

		#region DiscountableAmt
		public abstract class discountableAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _DiscountableAmt;
		[PXDBDecimal(4)]
		public virtual Decimal? DiscountableAmt
		{
			get
			{
				return this._DiscountableAmt;
			}
			set
			{
				this._DiscountableAmt = value;
			}
		}
		#endregion
		#region CuryDiscountableAmt
		public abstract class curyDiscountableAmt : IBqlField { }

		[CM.PXDBCurrency(typeof(Search<IN.INSetup.decPlPrcCst>), typeof(CRServiceCaseDiscountDetail.curyInfoID), 
			typeof(CRServiceCaseDiscountDetail.discountableAmt))]
		[PXUIField(DisplayName = "Discountable Amt.")]
		public virtual Decimal? CuryDiscountableAmt { get; set; }
		#endregion

		#region DiscountableQty
		public abstract class discountableQty : IBqlField { }

		[IN.PXDBQuantity(MinValue = 0)]
		[PXUIField(DisplayName = "Discountable Qty.")]
		public virtual Decimal? DiscountableQty { get; set; }
		#endregion

		#region DiscountAmt
		public abstract class discountAmt : IBqlField { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DiscountAmt { get; set; }
		#endregion

		#region CuryDiscountAmt
		public abstract class curyDiscountAmt : IBqlField { }

		[CM.PXDBCurrency(typeof(Search<IN.INSetup.decPlPrcCst>), typeof(CRServiceCaseDiscountDetail.curyInfoID), 
			typeof(CRServiceCaseDiscountDetail.discountAmt))]
		[PXUIField(DisplayName = "Discount Amt.")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryDiscountAmt { get; set; }
		#endregion

		#region DiscountPct
		public abstract class discountPct : IBqlField { }

		[PXDBDecimal(2)]
		[PXUIField(DisplayName = "Discount")]
		public virtual Decimal? DiscountPct { get; set; }
		#endregion

		#region FreeItemID
		public abstract class freeItemID : IBqlField { }

		[IN.Inventory(DisplayName = "Free Item")]
		public virtual Int32? FreeItemID { get; set; }
		#endregion

		#region FreeItemQty
		public abstract class freeItemQty : PX.Data.IBqlField { }

		[IN.PXDBQuantity(MinValue = 0)]
		[PXUIField(DisplayName = "Free Item Qty.")]
		public virtual Decimal? FreeItemQty { get; set; }
		#endregion
        #region IsManual
        public abstract class isManual : PX.Data.IBqlField
        {
        }
        protected Boolean? _IsManual;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual Discount")]
        public virtual Boolean? IsManual
        {
            get
            {
                return this._IsManual;
            }
            set
            {
                this._IsManual = value;
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
