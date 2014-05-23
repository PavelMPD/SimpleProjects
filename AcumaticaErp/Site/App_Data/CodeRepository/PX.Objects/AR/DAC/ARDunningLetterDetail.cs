namespace PX.Objects.AR
{
	using System;
	using PX.Data;
    using PX.Objects.CM;
    using PX.Objects.CA;

	/// <summary>
    /// List of invoises in Dunning Letter
	/// </summary>
	[System.SerializableAttribute()]
	public partial class ARDunningLetterDetail : PX.Data.IBqlTable
	{
		#region DunningLetterID
		public abstract class dunningLetterID : PX.Data.IBqlField
		{
		}
		protected Int32? _DunningLetterID;
		[PXDBInt(IsKey=true)]
        [PXDBLiteDefault(typeof(ARDunningLetter.dunningLetterID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(Enabled = false)]
        [PXParent(typeof(Select<ARDunningLetter, Where<ARDunningLetter.dunningLetterID, Equal<Current<ARDunningLetterDetail.dunningLetterID>>>>))]
        public virtual Int32? DunningLetterID
		{
			get
			{
				return this._DunningLetterID;
			}
			set
			{
				this._DunningLetterID = value;
			}
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.IBqlField
		{
		}
		protected String _DocType;
		[PXDBString(3, IsFixed = true, IsKey=true)]
		[PXDefault()]
		public virtual String DocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
				this._DocType = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.IBqlField
		{
		}
		protected String _RefNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region DocDate
		public abstract class docDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _DocDate;
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		public virtual DateTime? DocDate
		{
			get
			{
				return this._DocDate;
			}
			set
			{
				this._DocDate = value;
			}
		}
		#endregion
		#region DueDate
		public abstract class dueDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _DueDate;
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		public virtual DateTime? DueDate
		{
			get
			{
				return this._DueDate;
			}
			set
			{
				this._DueDate = value;
			}
		}
		#endregion
        #region CuryOrigDocAmt
        public abstract class curyOrigDocAmt : PX.Data.IBqlField
		{
		}
        protected Decimal? _CuryOrigDocAmt;
        [PXDBCury(typeof(ARDunningLetterDetail.curyID))]
        public virtual Decimal? CuryOrigDocAmt
		{
			get
			{
                return this._CuryOrigDocAmt;
			}
			set
			{
                this._CuryOrigDocAmt = value;
			}
		}
		#endregion
        #region CuryDocBal
        public abstract class curyDocBal : PX.Data.IBqlField
		{
		}
        protected Decimal? _CuryDocBal;
        [PXDBCury(typeof(ARDunningLetterDetail.curyID))]
        public virtual Decimal? CuryDocBal
		{
			get
			{
                return this._CuryDocBal;
			}
			set
			{
                this._CuryDocBal = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.IBqlField
		{
		}
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true)]
		[PXDefault("")]
		[PXSelector(typeof(PX.Objects.CM.Currency.curyID), CacheGlobal = true)]
		[PXUIField(DisplayName = "Currency ID")]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
        #region OrigDocAmt
        public abstract class origDocAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _OrigDocAmt;
        [PXDBBaseCury()]
        public virtual Decimal? OrigDocAmt
        {
            get
            {
                return this._OrigDocAmt;
            }
            set
            {
                this._OrigDocAmt = value;
            }
        }
        #endregion
        #region DocBal
        public abstract class docBal : PX.Data.IBqlField
        {
        }
        protected Decimal? _DocBal;
        [PXDBBaseCury()]
        public virtual Decimal? DocBal
        {
            get
            {
                return this._DocBal;
            }
            set
            {
                this._DocBal = value;
            }
        }
        #endregion
        #region Overdue
        public abstract class overdue : PX.Data.IBqlField
        {
        }
        protected Boolean? _Overdue;
        [PXDBBool()]
        [PXDefault(true)]
        public virtual Boolean? Overdue
        {
            get
            {
                return this._Overdue;
            }
            set
            {
                this._Overdue = value;
            }
        }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.IBqlField
        {
        }
        protected Byte[] _tstamp;
        [PXDBTimestamp(RecordComesFirst = true)]
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
