using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using System.Collections;
using PX.Objects.CS;
using PX.Objects.SO;

namespace PX.Objects.AP
{
    [PX.Objects.GL.TableAndChartDashboardType]
    public class APUpdateDiscounts : PXGraph<APUpdateDiscounts>
    {
        public PXCancel<ItemFilter> Cancel;
        public PXFilter<ItemFilter> Filter;
        [PXFilterable]
        public PXFilteredProcessing<SO.ARUpdateDiscounts.SelectedItem, ItemFilter> Items;

        public virtual IEnumerable items()
        {
            ItemFilter filter = Filter.Current;
            if (filter == null)
            {
                yield break;
            }
            bool found = false;
            foreach (SO.ARUpdateDiscounts.SelectedItem item in Items.Cache.Inserted)
            {
                found = true;
                yield return item;
            }
            if (found)
                yield break;


            List<string> added = new List<string>();
            PXSelect<VendorDiscountSequence,
                Where<VendorDiscountSequence.startDate, LessEqual<Current<ItemFilter.pendingDiscountDate>>,
                And<VendorDiscountSequence.vendorID, Equal<Current<ItemFilter.vendorID>>,
                And<VendorDiscountSequence.isPromotion, Equal<boolFalse>>>>> s1 = new PXSelect<VendorDiscountSequence, Where<VendorDiscountSequence.startDate, LessEqual<Current<ItemFilter.pendingDiscountDate>>, And<VendorDiscountSequence.vendorID, Equal<Current<ItemFilter.vendorID>>, And<VendorDiscountSequence.isPromotion, Equal<boolFalse>>>>>(this);

            foreach (VendorDiscountSequence sequence in s1.Select())
            {
                string key = string.Format("{0}.{1}", sequence.DiscountID, sequence.DiscountSequenceID);
                added.Add(key);

                SO.ARUpdateDiscounts.SelectedItem item = new SO.ARUpdateDiscounts.SelectedItem();
                item.DiscountID = sequence.DiscountID;
                item.DiscountSequenceID = sequence.DiscountSequenceID;
                item.Description = sequence.Description;
                item.DiscountedFor = sequence.DiscountedFor;
                item.BreakBy = sequence.BreakBy;
                item.IsPromotion = sequence.IsPromotion;
                item.IsActive = sequence.IsActive;
                item.StartDate = sequence.StartDate;
                item.EndDate = sequence.EndDate;

                yield return Items.Insert(item);
            }

            foreach (DiscountDetail detail in PXSelectGroupBy<DiscountDetail, Where<DiscountDetail.startDate, LessEqual<Current<ItemFilter.pendingDiscountDate>>>,
                Aggregate<GroupBy<DiscountDetail.discountID, GroupBy<DiscountDetail.discountSequenceID>>>>.Select(this))
            {
                string key = string.Format("{0}.{1}", detail.DiscountID, detail.DiscountSequenceID);

                if (!added.Contains(key))
                {
                    VendorDiscountSequence sequence = PXSelect<VendorDiscountSequence,
                        Where<VendorDiscountSequence.discountID, Equal<Required<VendorDiscountSequence.discountID>>,
                        And<VendorDiscountSequence.discountSequenceID, Equal<Required<VendorDiscountSequence.discountSequenceID>>>>>.Select(this, detail.DiscountID, detail.DiscountSequenceID);

                    if (sequence != null && sequence.IsPromotion == false)
                    {
                        SO.ARUpdateDiscounts.SelectedItem item = new SO.ARUpdateDiscounts.SelectedItem();
                        item.DiscountID = sequence.DiscountID;
                        item.DiscountSequenceID = sequence.DiscountSequenceID;
                        item.Description = sequence.Description;
                        item.DiscountedFor = sequence.DiscountedFor;
                        item.BreakBy = sequence.BreakBy;
                        item.IsPromotion = sequence.IsPromotion;
                        item.IsActive = sequence.IsActive;
                        item.StartDate = sequence.StartDate;
                        item.EndDate = sequence.EndDate;

                        yield return Items.Insert(item);
                    }
                }
            }

            Items.Cache.IsDirty = false;
        }

        public APUpdateDiscounts()
        {
            Items.SetSelected<SO.ARUpdateDiscounts.SelectedItem.selected>();
            Items.SetProcessCaption(IN.Messages.Process);
            Items.SetProcessAllCaption(IN.Messages.ProcessAll);
        }

        #region EventHandlers
        protected virtual void ItemFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            Items.Cache.Clear();
        }

        protected virtual void ItemFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            ItemFilter filter = Filter.Current;
            DateTime? date = Filter.Current.PendingDiscountDate;
            Items.SetProcessDelegate<UpdateDiscountProcess>(
                    delegate(UpdateDiscountProcess graph, SO.ARUpdateDiscounts.SelectedItem item)
                    {
                        UpdateDiscount(graph, item, date);
                    });
        }

        #endregion

        public static void UpdateDiscount(UpdateDiscountProcess graph, SO.ARUpdateDiscounts.SelectedItem item, DateTime? filterDate)
        {
            graph.UpdateDiscount(item, filterDate);
        }

        public static void UpdateDiscount(string discountID, string discountSequenceID, DateTime? filterDate)
        {
            UpdateDiscountProcess graph = PXGraph.CreateInstance<UpdateDiscountProcess>();

            graph.UpdateDiscount(discountID, discountSequenceID, filterDate);
        }



        #region Local Types

        [Serializable]
        public partial class ItemFilter : IBqlTable
        {
            public abstract class vendorID : PX.Data.IBqlField
            {
            }
            protected int? _VendorID;
            [Vendor()]
            public virtual int? VendorID
            {
                get
                {
                    return this._VendorID;
                }
                set
                {
                    this._VendorID = value;
                }
            }
            #region PendingDiscountDate
            public abstract class pendingDiscountDate : PX.Data.IBqlField
            {
            }
            protected DateTime? _PendingDiscountDate;
            [PXDBDate()]
            [PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "Max. Pending Discount Date")]
            public virtual DateTime? PendingDiscountDate
            {
                get
                {
                    return this._PendingDiscountDate;
                }
                set
                {
                    this._PendingDiscountDate = value;
                }
            }
            #endregion
        }
        #endregion
    }
}
