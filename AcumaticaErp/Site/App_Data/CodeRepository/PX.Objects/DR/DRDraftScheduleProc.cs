using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using System.Collections;
using PX.Objects.CR;
using PX.Objects.IN;
using PX.Objects.GL;
using System.Diagnostics;

namespace PX.Objects.DR
{
    public class DRDraftScheduleProc : PXGraph<DRRecognition>
    {
        public PXCancel<SchedulesFilter> Cancel;
        public PXFilter<SchedulesFilter> Filter;
		[PXFilterable]
        public PXFilteredProcessing<DRScheduleDetail, SchedulesFilter> Items;

        public virtual IEnumerable items()
        {
            SchedulesFilter filter = this.Filter.Current;
            if (filter != null)
            {
                PXSelectBase<DRScheduleDetail> select = new PXSelectJoin<DRScheduleDetail,
                    InnerJoin<DRSchedule, On<DRSchedule.scheduleID, Equal<DRScheduleDetail.scheduleID>>,
                    InnerJoin<DRDeferredCode, On<DRDeferredCode.deferredCodeID, Equal<DRScheduleDetail.defCode>>>>,
                    Where<DRDeferredCode.accountType, Equal<Current<SchedulesFilter.accountType>>,
                    And<DRScheduleDetail.status, Equal<DRScheduleStatus.DraftStatus>>>>(this);

                if (!string.IsNullOrEmpty(filter.DeferredCode))
                {
                    select.WhereAnd<Where<DRScheduleDetail.defCode, Equal<Current<SchedulesFilter.deferredCode>>>>();
                }

                if (filter.AccountID != null)
                {
                    select.WhereAnd<Where<DRScheduleDetail.defAcctID, Equal<Current<SchedulesFilter.accountID>>>>();
                }

                if (filter.SubID != null)
                {
                    select.WhereAnd<Where<DRScheduleDetail.defSubID, Equal<Current<SchedulesFilter.subID>>>>();
                }

                if (filter.BAccountID != null)
                {
                    select.WhereAnd<Where<DRScheduleDetail.bAccountID, Equal<Current<SchedulesFilter.bAccountID>>>>();
                }

                if (filter.ComponentID != null)
                {
                    select.WhereAnd<Where<DRScheduleDetail.componentID, Equal<Current<SchedulesFilter.componentID>>>>();
                }
                
                return select.Select();

            }
            else
                return null;
        }
        
        public DRDraftScheduleProc()
		{
            Items.SetSelected<DRScheduleDetail.selected>();
		}
        
        public PXAction<SchedulesFilter> viewSchedule;
        [PXUIField(DisplayName = "View Schedule")]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
        public virtual IEnumerable ViewSchedule(PXAdapter adapter)
        {
            if (Items.Current != null)
            {
				DraftScheduleMaint target = PXGraph.CreateInstance<DraftScheduleMaint>();
                target.Schedule.Current = PXSelect<DRSchedule, Where<DRSchedule.scheduleID, Equal<Required<DRSchedule.scheduleID>>>>.Select(this, Items.Current.ScheduleID);
				throw new PXRedirectRequiredException(target, true, "ViewSchedule") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            return adapter.Get();
        }
        
        #region EventHandlers

        protected virtual void SchedulesFilter_AccountType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            SchedulesFilter row = e.Row as SchedulesFilter;
            if (row != null)
            {
                row.BAccountID = null;
                row.DeferredCode = null;
                row.AccountID = null;
                row.SubID = null;
            }
        }
                
        protected virtual void SchedulesFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            Items.Cache.Clear();
        }

        protected virtual void SchedulesFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            Items.SetProcessDelegate(ReleaseCustomSchedule);
        }

        protected virtual void DRScheduleDetail_ComponentID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void DRScheduleDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            DR.DRScheduleDetail row = e.Row as DR.DRScheduleDetail;

            if (row != null)
            {
                row.DocumentType = DRScheduleDocumentType.BuildDocumentType(row.Module, row.DocType);
            }
        }
        #endregion

        public static void ReleaseCustomSchedule(DRScheduleDetail item)
        {
            ScheduleMaint maint = PXGraph.CreateInstance<ScheduleMaint>();
            maint.Clear();
            maint.Document.Current = PXSelect<DR.DRScheduleDetail,
                Where<DR.DRScheduleDetail.scheduleID, Equal<Required<DR.DRScheduleDetail.scheduleID>>,
                And<DR.DRScheduleDetail.componentID, Equal<Required<DR.DRScheduleDetail.componentID>>>>>.Select(maint, item.ScheduleID, item.ComponentID);

            maint.ReleaseCustomSchedule();
        }

        #region Local Types
        [Serializable]
        public partial class SchedulesFilter : IBqlTable
        {
            #region AccountType
            public abstract class accountType : PX.Data.IBqlField
            {
            }
            protected string _AccountType;
            [PXDBString(1)]
            [PXDefault(DeferredAccountType.Income)]
            [DeferredAccountType.List()]
            [PXUIField(DisplayName = "Code Type", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual string AccountType
            {
                get
                {
                    return this._AccountType;
                }
                set
                {
                    this._AccountType = value;

                    switch (value)
                    {
                        case DeferredAccountType.Expense:
                            _BAccountType = CR.BAccountType.VendorType;
                            break;
                        default:
                            _BAccountType = CR.BAccountType.CustomerType;
                            break;
                    }
                }
            }
            #endregion
            #region AccountID
            public abstract class accountID : PX.Data.IBqlField
            {
            }
            protected Int32? _AccountID;
            [Account(DisplayName = "Deferral Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
            public virtual Int32? AccountID
            {
                get
                {
                    return this._AccountID;
                }
                set
                {
                    this._AccountID = value;
                }
            }
            #endregion
            #region SubID
            public abstract class subID : PX.Data.IBqlField
            {
            }
            protected Int32? _SubID;
            [SubAccount(typeof(SchedulesFilter.accountID), DisplayName = "Deferral Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
            public virtual Int32? SubID
            {
                get
                {
                    return this._SubID;
                }
                set
                {
                    this._SubID = value;
                }
            }
            #endregion
            #region DeferredCode
            public abstract class deferredCode : PX.Data.IBqlField
            {
            }
            protected String _DeferredCode;
            [PXDBString(10, InputMask = ">aaaaaaaaaa")]
            [PXUIField(DisplayName = "Deferral Code")]
            [PXSelector(typeof(Search<DRDeferredCode.deferredCodeID, Where<DRDeferredCode.accountType, Equal<Current<SchedulesFilter.accountType>>>>))]
            public virtual String DeferredCode
            {
                get
                {
                    return this._DeferredCode;
                }
                set
                {
                    this._DeferredCode = value;
                }
            }
            #endregion

            #region BAccountType
            public abstract class bAccountType : PX.Data.IBqlField
            {
            }
            protected String _BAccountType;
            [PXDefault(CR.BAccountType.CustomerType)]
            [PXString(2, IsFixed = true)]
            [PXStringList(new string[] { CR.BAccountType.VendorType, CR.BAccountType.CustomerType },
                    new string[] { CR.Messages.VendorType, CR.Messages.CustomerType })]
            public virtual String BAccountType
            {
                get
                {
                    return this._BAccountType;
                }
                set
                {
                    this._BAccountType = value;
                }
            }
            #endregion
            #region BAccountID
            public abstract class bAccountID : PX.Data.IBqlField
            {
            }
            protected Int32? _BAccountID;
            [PXDBInt()]
            [PXUIField(DisplayName = "Customer/Vendor")]
            [PXSelector(typeof(Search<BAccountR.bAccountID, Where<BAccountR.type, Equal<Current<SchedulesFilter.bAccountType>>>>), new Type[] { typeof(BAccountR.acctCD), typeof(BAccountR.acctName), typeof(BAccountR.type) }, SubstituteKey = typeof(BAccountR.acctCD))]
            public virtual Int32? BAccountID
            {
                get
                {
                    return this._BAccountID;
                }
                set
                {
                    this._BAccountID = value;
                }
            }
            #endregion
            #region ComponentID
            public abstract class componentID : PX.Data.IBqlField
            {
            }
            protected Int32? _ComponentID;

            [Inventory(DisplayName = "Component")]
            public virtual Int32? ComponentID
            {
                get
                {
                    return this._ComponentID;
                }
                set
                {
                    this._ComponentID = value;
                }
            }

            #endregion
        }

        #endregion
    }
}
