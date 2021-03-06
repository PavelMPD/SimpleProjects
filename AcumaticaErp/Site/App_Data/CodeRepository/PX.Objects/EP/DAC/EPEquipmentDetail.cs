using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.PM;

namespace PX.Objects.EP
{
    [Serializable]
    public class EPEquipmentDetail : PX.Data.IBqlTable
    {
        #region TimeCardCD

        public abstract class timeCardCD : IBqlField { }

        [PXDBDefault(typeof(EPEquipmentTimeCard.timeCardCD))]
        [PXDBString(10, IsKey = true)]
        [PXUIField(Visible = false)]
        [PXParent(typeof(Select<EPEquipmentTimeCard, Where<EPEquipmentTimeCard.timeCardCD, Equal<Current<EPEquipmentDetail.timeCardCD>>>>))]
        public virtual String TimeCardCD { get; set; }

        #endregion
        #region LineNbr
        public abstract class lineNbr : IBqlField { }

        [PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(EPEquipmentTimeCard.detailLineCntr))]
        [PXUIField(Visible = false)]
        public virtual Int32? LineNbr { get; set; }
        #endregion

        #region SetupSummaryLineNbr
        public abstract class setupSummarylineNbr : IBqlField { }
        [PXDBInt()]
        public virtual Int32? SetupSummaryLineNbr { get; set; }
        #endregion

        #region RunSummaryLineNbr
        public abstract class runSummarylineNbr : IBqlField { }
        [PXDBInt()]
        public virtual Int32? RunSummaryLineNbr { get; set; }
        #endregion

        #region SuspendSummaryLineNbr
        public abstract class suspendSummarylineNbr : IBqlField { }
        [PXDBInt()]
        public virtual Int32? SuspendSummaryLineNbr { get; set; }
        #endregion

        #region OrigLineNbr
        public abstract class origLineNbr : IBqlField { }
        [PXDBInt()]
        public virtual Int32? OrigLineNbr { get; set; }
        #endregion
        #region Date
        public abstract class date : PX.Data.IBqlField
        {
        }
        protected DateTime? _Date;
        [PXDBDate()]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Date")]
        public virtual DateTime? Date
        {
            get
            {
                return this._Date;
            }
            set
            {
                this._Date = value;
            }
        }
        #endregion
        #region Description
        public abstract class description : PX.Data.IBqlField
        {
        }
        protected String _Description;
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
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
        #region ProjectID
        public abstract class projectID : PX.Data.IBqlField
        {
        }
        protected Int32? _ProjectID;
        [PXDefault(typeof(PMProject.contractID))]
        [EPEquipmentActiveProject]
        public virtual Int32? ProjectID
        {
            get
            {
                return this._ProjectID;
            }
            set
            {
                this._ProjectID = value;
            }
        }
        #endregion
        #region ProjectTaskID
        public abstract class projectTaskID : IBqlField { }
        [ActiveProjectTask(typeof(EPEquipmentDetail.projectID), GL.BatchModule.EP, AllowNullIfContract = true, DisplayName = "Project Task")]
        public virtual Int32? ProjectTaskID { get; set; }
        #endregion
        #region RunTime
        public abstract class runTime : PX.Data.IBqlField
        {
        }
        protected Int32? _RunTime;
        [PXDBInt]
        [PXUIField(DisplayName = "Run Time")]
        public virtual Int32? RunTime
        {
            get
            {
                return this._RunTime;
            }
            set
            {
                this._RunTime = value;
            }
        }
        #endregion
        #region SetupTime
        public abstract class setupTime : PX.Data.IBqlField
        {
        }
        protected Int32? _SetupTime;
        [PXDBInt]
        [PXUIField(DisplayName = "Setup Time")]
        public virtual Int32? SetupTime
        {
            get
            {
                return this._SetupTime;
            }
            set
            {
                this._SetupTime = value;
            }
        }
        #endregion
        #region SuspendTime
        public abstract class suspendTime : PX.Data.IBqlField
        {
        }
        protected Int32? _SuspendTime;
        [PXDBInt]
        [PXUIField(DisplayName = "Suspend Time")]
        public virtual Int32? SuspendTime
        {
            get
            {
                return this._SuspendTime;
            }
            set
            {
                this._SuspendTime = value;
            }
        }
        #endregion
        #region IsBillable
        public abstract class isBillable : PX.Data.IBqlField
        {
        }
        protected Boolean? _IsBillable;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Billable")]
        public virtual Boolean? IsBillable
        {
            get
            {
                return this._IsBillable;
            }
            set
            {
                this._IsBillable = value;
            }
        }
        #endregion

        #region System Columns
        #region NoteID
        public abstract class noteID : PX.Data.IBqlField
        {
        }
        protected Int64? _NoteID;
        [PXNote()]
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


        #region Unbound Fields (Calculated in the TimecardMaint graph)

       
       

        

        #endregion
    }
}
