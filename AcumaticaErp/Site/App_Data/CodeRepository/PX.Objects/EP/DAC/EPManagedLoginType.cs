using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.EP
{
    [Serializable]
    public partial class EPManagedLoginType : IBqlTable
    {
        #region LoginTypeID
        public abstract class loginTypeID : IBqlField { }

        [PXDBInt(IsKey = true)]
        [PXParent(typeof(Select<EPLoginType, Where<EPLoginType.loginTypeID, Equal<Current<EPManagedLoginType.loginTypeID>>>>))]
        [PXSelector(typeof(Search<EPLoginType.loginTypeID, Where<EPLoginType.entity, Equal<EPLoginType.entity.contact>>>),
            DescriptionField = typeof(EPLoginType.loginTypeName), SubstituteKey = typeof(EPLoginType.loginTypeName))]
        [PXUIField(DisplayName = "User Type")]
        public virtual int? LoginTypeID { get; set; }
        #endregion

        #region ParentLoginTypeID
        public abstract class parentLoginTypeID : IBqlField { }

        [PXDBInt(IsKey = true)]
        [PXDBChildIdentity(typeof(EPLoginType.loginTypeID))]
        [PXParent(typeof(Select<EPLoginType, Where<EPLoginType.loginTypeID, Equal<Current<EPManagedLoginType.parentLoginTypeID>>>>))]
        [PXDefault(typeof(EPLoginType.loginTypeID))]
        [PXUIField(DisplayName = "Parent User Type")]
        public virtual int? ParentLoginTypeID { get; set; }
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

        [PXDBLastModifiedDateTimeUtc]
        [PXUIField(DisplayName = "Last Modified Date", Enabled = false)]
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
