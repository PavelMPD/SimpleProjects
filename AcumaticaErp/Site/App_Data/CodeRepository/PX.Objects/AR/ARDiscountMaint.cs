using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using System.Linq;

namespace PX.Objects.SO
{
	public class ARDiscountMaint : PXGraph<SOSetupMaint>
	{
		#region Selects/Views

		public PXSelect<ARDiscount> Document;
        public PXSavePerRow<ARDiscount> Save;
		public PXCancel<ARDiscount> Cancel;
		#endregion

        public PXAction<ARDiscount> viewARDiscountSequence;
        [PXUIField(DisplayName = AR.Messages.ViewARDiscountSequence, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXLookupButton]
        public virtual IEnumerable ViewARDiscountSequence(PXAdapter adapter)
        {
            if (this.Document.Current != null)
            {
                if (this.Document.Current.DiscountID != null)
                {
                    ARDiscountSequenceMaint arDiscountSequence = PXGraph.CreateInstance<ARDiscountSequenceMaint>();
                    arDiscountSequence.Sequence.Current = new DiscountSequence();
                    arDiscountSequence.Sequence.Current.DiscountID = this.Document.Current.DiscountID;
                    throw new PXRedirectRequiredException(arDiscountSequence, AR.Messages.ViewARDiscountSequence);
                }
            }
            return adapter.Get();
        }

		protected virtual void ARDiscount_ApplicableTo_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARDiscount row = e.Row as ARDiscount;
			if (row != null )
			{
				DiscountSequence firstSeq = PXSelect<DiscountSequence, Where<DiscountSequence.discountID, Equal<Required<DiscountSequence.discountID>>>>.SelectWindowed(this, 0, 1, row.DiscountID);
				if (firstSeq != null)
				{
					e.Cancel = true;
					throw new PXSetPropertyException(Messages.SequenceExistsApplicableTo);
				}
			}
		}

		protected virtual void ARDiscount_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ARDiscount row = e.Row as ARDiscount;
			if (row != null && row.Type == DiscountType.Flat)
			{
				if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
				{
					ARDiscount duplicate = PXSelect<ARDiscount, Where<ARDiscount.type, Equal<DiscountType.FlatDiscount>,
						And<ARDiscount.applicableTo, Equal<Required<ARDiscount.applicableTo>>,
						And<ARDiscount.discountID, NotEqual<Required<ARDiscount.discountID>>>>>>.Select(this, row.ApplicableTo, row.DiscountID);

					if (duplicate != null)
					{
						if (sender.RaiseExceptionHandling<ARDiscount.applicableTo>(e.Row, row.ApplicableTo, new PXSetPropertyException(Messages.OnlyOneFlatAllowed,	typeof(ARDiscount.applicableTo).Name)))
						{
							throw new PXRowPersistingException(typeof(ARDiscount.applicableTo).Name, row.ApplicableTo, Messages.OnlyOneFlatAllowed,	typeof(ARDiscount.applicableTo).Name);
						}
					}
				}
			}
		}

        protected virtual void ARDiscount_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            ARDiscount row = e.Row as ARDiscount;
            if (row != null)
            {
                DiscountSequence firstSeq = PXSelect<DiscountSequence, Where<DiscountSequence.discountID, Equal<Required<DiscountSequence.discountID>>>>.SelectWindowed(this, 0, 1, row.DiscountID);
                if (firstSeq != null)
                {
                    e.Cancel = true;
                    throw new PXSetPropertyException(Messages.SequenceExists);
                }
            }
        }

        protected virtual void ARDiscount_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            DiscountEngine.GetDiscountTypes(true);
        }

		protected virtual void ARDiscount_Type_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARDiscount row = e.Row as ARDiscount;
			if (row != null)
			{
				DiscountSequence sequence = PXSelect<DiscountSequence, Where<DiscountSequence.discountID, Equal<Required<DiscountSequence.discountID>>>>.SelectWindowed(this, 0, 1, row.DiscountID);
				if (sequence != null)
				{
					row.Type = (string)e.OldValue;
					sender.RaiseExceptionHandling<ARDiscount.type>(row, e.OldValue, new PXSetPropertyException(Messages.DiscountTypeCannotChanged));
				}
				if (row.Type == DiscountType.Flat)
				{
					row.IsAutoNumber = true;
					if (string.IsNullOrEmpty(row.LastNumber))
					{
						row.LastNumber = "000000000";
					}
				}
			}
		}


		protected virtual void ARDiscount_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARDiscount row = e.Row as ARDiscount;
			if (row != null)
			{
                PXUIFieldAttribute.SetEnabled<ARDiscount.excludeFromDiscountableAmt>(sender, row, row.Type == DiscountType.Line);
                PXUIFieldAttribute.SetEnabled<ARDiscount.skipDocumentDiscounts>(sender, row, row.Type == DiscountType.Group);
			}
		}

		protected virtual void ARDiscount_ApplicableTo_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			ARDiscount row = e.Row as ARDiscount;
			if (row != null)
			{
                if (row.Type == DiscountType.Document)
                {
                    e.ReturnState = PXStringState.CreateInstance(e.ReturnValue, 1, false, "ApplicableTo", false, 1, null,
                                                new string[] {DiscountTarget.Customer, DiscountTarget.CustomerAndBranch, DiscountTarget.CustomerPrice, DiscountTarget.CustomerPriceAndBranch, DiscountTarget.Unconditional },
                                                new string[] {Messages.Customer, Messages.CustomerAndBranch, Messages.CustomerPrice, Messages.CustomerPriceAndBranch, Messages.Unconditional }.Select(l => PXMessages.LocalizeNoPrefix(l)).ToArray(),
                                                true, DiscountTarget.Customer);
                    return;
                }
			}

			List<string> values = new List<string>();
			List<string> labels = new List<string>();
			values.AddRange(new string[] { DiscountTarget.Customer, DiscountTarget.Inventory, DiscountTarget.InventoryPrice, DiscountTarget.CustomerAndInventory, DiscountTarget.CustomerAndInventoryPrice, DiscountTarget.CustomerPrice, DiscountTarget.CustomerPriceAndInventory, DiscountTarget.CustomerPriceAndInventoryPrice});
			labels.AddRange(new string[] { Messages.Customer, Messages.Discount_Inventory, Messages.InventoryPrice, Messages.CustomerAndInventory, Messages.CustomerAndInventoryPrice, Messages.CustomerPrice, Messages.CustomerPriceAndInventory, Messages.CustomerPriceAndInventoryPrice});
			if (PXAccess.FeatureInstalled<FeaturesSet.warehouse>())
			{
				values.AddRange(new string[] { DiscountTarget.Warehouse, DiscountTarget.WarehouseAndInventory, DiscountTarget.WarehouseAndCustomer, DiscountTarget.WarehouseAndInventoryPrice, DiscountTarget.WarehouseAndCustomerPrice });
				labels.AddRange(new string[] { Messages.Warehouse, Messages.WarehouseAndInventory, Messages.WarehouseAndCustomer, Messages.WarehouseAndInventoryPrice, Messages.WarehouseAndCustomerPrice });
			}
			values.AddRange(new string[] { DiscountTarget.Branch, DiscountTarget.Unconditional });
			labels.AddRange(new string[] { Messages.Branch, Messages.Unconditional });

			e.ReturnState = PXStringState.CreateInstance(e.ReturnValue, 1, false, "ApplicableTo", false, 1, null, values.ToArray(), labels.ToArray().Select(l => PXMessages.LocalizeNoPrefix(l)).ToArray(), true, DiscountTarget.Inventory);
		}
	
	}
}
