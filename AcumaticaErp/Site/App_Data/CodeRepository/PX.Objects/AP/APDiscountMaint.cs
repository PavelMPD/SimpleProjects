using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.SO;
using System.Linq;
using PX.Objects.CS;


namespace PX.Objects.AP
{
	public class APDiscountMaint : PXGraph<APDiscountMaint,Vendor>
	{
        public PXSelect<Vendor> Filter;
        public PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Current<Vendor.bAccountID>>>> CurrentVendor;
		public PXSelect<APDiscount, Where<APDiscount.bAccountID, Equal<Current<Vendor.bAccountID>>>> CurrentDiscounts;

		public APDiscountMaint()
		{
			this.Filter.Cache.AllowInsert = false;
			this.Filter.Cache.AllowDelete = false;
		}

        public PXAction<Vendor> viewAPDiscountSequence;
        [PXUIField(DisplayName = Messages.ViewAPDiscountSequence, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXLookupButton]
        public virtual IEnumerable ViewAPDiscountSequence(PXAdapter adapter)
        {
            if (this.Filter.Current != null && this.CurrentDiscounts.Current != null)
            {
                if (this.Filter.Current.BAccountID != null && this.CurrentDiscounts.Current.DiscountID != null)
                {
                    APDiscountSequenceMaint apDiscountSequence = PXGraph.CreateInstance<APDiscountSequenceMaint>();
                    apDiscountSequence.Sequence.Current = new VendorDiscountSequence();
                    apDiscountSequence.Sequence.Current.VendorID = this.Filter.Current.BAccountID;
                    apDiscountSequence.Sequence.Current.DiscountID = this.CurrentDiscounts.Current.DiscountID;
                    throw new PXRedirectRequiredException(apDiscountSequence, Messages.ViewAPDiscountSequence);
                }
            }
            return adapter.Get();
        }

        protected virtual void Vendor_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
        {
            if (e.Row != null)
            {
                if (((Vendor)e.Row).LineDiscountTarget == null)
                    Filter.Cache.SetDefaultExt<Vendor.lineDiscountTarget>(e.Row);
                if (((Vendor)e.Row).IgnoreConfiguredDiscounts == null) 
                    Filter.Cache.SetDefaultExt<Vendor.ignoreConfiguredDiscounts>(e.Row);
            }
        }

		protected virtual void APDiscount_ApplicableTo_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			APDiscount row = e.Row as APDiscount;
			if (row != null)
			{
				if (row.Type == SO.DiscountType.Document)
				{
					e.ReturnState = PXStringState.CreateInstance(e.ReturnValue, 1, false, "ApplicableTo", false, 1, null,
												new string[] { DiscountTarget.Vendor },
												new string[] { Messages.VendorUnconditional }.Select(l => PXMessages.LocalizeNoPrefix(l)).ToArray(),
												true, DiscountTarget.Unconditional);
					return;
				}
			}
			if (!PXAccess.FeatureInstalled<FeaturesSet.accountLocations>())
			{
				e.ReturnState = PXStringState.CreateInstance(e.ReturnValue, 1, false, "ApplicableTo", false, 1, null,
										new string[] { DiscountTarget.VendorAndInventory, DiscountTarget.VendorAndInventoryPrice, DiscountTarget.Vendor },
										new string[] { Messages.VendorAndInventory, Messages.VendorInventoryPrice, Messages.VendorUnconditional }.Select(l => PXMessages.LocalizeNoPrefix(l)).ToArray(),
										true, DiscountTarget.Inventory);
				return;
			}

			e.ReturnState = PXStringState.CreateInstance(e.ReturnValue, 1, false, "ApplicableTo", false, 1, null,
										 new string[] { DiscountTarget.VendorAndInventory, DiscountTarget.VendorAndInventoryPrice, DiscountTarget.VendorLocation, DiscountTarget.VendorLocationAndInventory, DiscountTarget.Vendor },
										 new string[] { Messages.VendorAndInventory, Messages.VendorInventoryPrice, Messages.Vendor_Location, Messages.VendorLocationaAndInventory, Messages.VendorUnconditional }.Select(l => PXMessages.LocalizeNoPrefix(l)).ToArray(),
										 true, DiscountTarget.Inventory);
		}
			

        protected virtual void APDiscount_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            APDiscount row = e.Row as APDiscount;
            if (row != null)
            {
                if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
                {
                    if (PXSelectReadonly<ARDiscount, Where<ARDiscount.discountID, Equal<Required<ARDiscount.discountID>>>>.Select(this, row.DiscountID).Count != 0)
                    {
                        sender.RaiseExceptionHandling<APDiscount.discountID>(row, row.DiscountID, new PXSetPropertyException(Messages.DiscountCodeAlreadyExist, PXErrorLevel.Error));
                    }
                }
                if (e.Operation == PXDBOperation.Insert)
                {
                    if (PXSelectReadonly<APDiscount, Where<APDiscount.discountID, Equal<Required<APDiscount.discountID>>>>.Select(this, row.DiscountID).Count != 0)
                    {
                        sender.RaiseExceptionHandling<APDiscount.discountID>(row, row.DiscountID, new PXSetPropertyException(Messages.DiscountCodeAlreadyExist, PXErrorLevel.Error));
                    }
                }
            }
        }

        protected virtual void APDiscount_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            DiscountEngine.GetDiscountTypes(true);
        }

        protected virtual void APDiscount_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            APDiscount row = e.Row as APDiscount;
            if (row != null)
            {
                PXUIFieldAttribute.SetEnabled<APDiscount.excludeFromDiscountableAmt>(sender, row, row.Type == DiscountType.Line);
                PXUIFieldAttribute.SetEnabled<APDiscount.skipDocumentDiscounts>(sender, row, row.Type == DiscountType.Group);
            }
        }
        protected virtual void APDiscount_Type_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            APDiscount row = e.Row as APDiscount;
            if (row != null)
            {
                DiscountSequence sequence = PXSelect<DiscountSequence, Where<DiscountSequence.discountID, Equal<Required<DiscountSequence.discountID>>>>.SelectWindowed(this, 0, 1, row.DiscountID);
                if (sequence != null)
                {
                    row.Type = (string)e.OldValue;
                    sender.RaiseExceptionHandling<APDiscount.type>(row, e.OldValue, new PXSetPropertyException(SO.Messages.DiscountTypeCannotChanged));
                }
                if (row.Type == SO.DiscountType.Flat)
                {
                    row.IsAutoNumber = true;
                    if (string.IsNullOrEmpty(row.LastNumber))
                    {
                        row.LastNumber = "000000000";
                    }
                }
            }
        }
       
	}
}
