using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.IN
{
	public class INKitSpecMaint : PXGraph<INKitSpecMaint, INKitSpecHdr>
	{ 

		#region Public Members


		public PXSelect<INKitSpecHdr, Where<INKitSpecHdr.kitInventoryID, Equal<Optional<INKitSpecHdr.kitInventoryID>>>> Hdr;

		public PXSelect<INKitSpecStkDet,
			Where<INKitSpecStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
				And<INKitSpecStkDet.revisionID, Equal<Current<INKitSpecHdr.revisionID>>>>,
			OrderBy<Asc<INKitSpecStkDet.kitInventoryID, Asc<INKitSpecStkDet.revisionID, Asc<INKitSpecStkDet.lineNbr>>>>> StockDet;

		public PXSelect<INKitSpecNonStkDet,
			Where<INKitSpecNonStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
				And<INKitSpecNonStkDet.revisionID, Equal<Current<INKitSpecHdr.revisionID>>>>,
			OrderBy<Asc<INKitSpecNonStkDet.kitInventoryID, Asc<INKitSpecNonStkDet.revisionID, Asc<INKitSpecNonStkDet.lineNbr>>>>> NonStockDet;

		public PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>> KitItem;
		#endregion Public Members        
        protected virtual void INKitSpecHdr_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
           INKitSpecHdr row = e.Row as INKitSpecHdr;
           if (row != null)
           {
               InventoryItem item = KitItem.Select();
               if (item != null)
               {
                   PXUIFieldAttribute.SetEnabled<INKitSpecHdr.kitSubItemID>(sender, row, item.StkItem == true);
                   row.IsNonStock = item.StkItem != true;
                   if (row.LotSerTrack == null) 
                   {
                       INLotSerClass kitLotSerClass = PXSelect<INLotSerClass, Where<INLotSerClass.lotSerClassID, Equal<Required<INLotSerClass.lotSerClassID>>>>.Select(this, item.LotSerClassID);
                       if (kitLotSerClass != null)
                       {
                           row.LotSerTrack = kitLotSerClass.LotSerTrack;
                       }
                   }
               }          
           }
        }

        protected virtual void INKitSpecHdr_KitInventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            INKitSpecHdr row = e.Row as INKitSpecHdr;
            if (row != null)
            {
                INLotSerClass kitLotSerClass = PXSelectJoin<INLotSerClass,
                                                            InnerJoin<InventoryItem, On<InventoryItem.lotSerClassID, Equal<INLotSerClass.lotSerClassID>>>,
                                                            Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, row.KitInventoryID);
                if (kitLotSerClass != null)
                {
                    row.LotSerTrack = kitLotSerClass.LotSerTrack;
                }
            }
        }     

		protected virtual void INKitSpecHdr_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			INKitSpecHdr row = e.Row as INKitSpecHdr;
			if (row != null && row.IsNonStock == true)
			{
				INKitSpecHdr existing = PXSelectReadonly<INKitSpecHdr, Where<INKitSpecHdr.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>>.Select(this);
				if (existing != null)
				{
					sender.RaiseExceptionHandling<INKitSpecHdr.revisionID>(e.Row, row.RevisionID, new PXSetPropertyException(Messages.SingleRevisionForNS));
				}
			}
		}

		protected virtual void INKitSpecHdr_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			INKitSpecHdr row = e.Row as INKitSpecHdr;
            if (row != null && row.IsNonStock == true && e.Operation == PXDBOperation.Insert)
			{
				INKitSpecHdr existing = PXSelectReadonly<INKitSpecHdr, Where<INKitSpecHdr.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>>.Select(this);
				if (existing != null)
				{
                    if (sender.RaiseExceptionHandling<INKitSpecHdr.revisionID>(e.Row, row.RevisionID, new PXSetPropertyException(Messages.SingleRevisionForNS)))
                    {
                        throw new PXRowPersistingException(typeof(INKitSpecHdr.revisionID).Name, null, Messages.SingleRevisionForNS);
                    }
                }
			}
		}

		protected virtual void INKitSpecStkDet_CompInventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INKitSpecStkDet row = e.Row as INKitSpecStkDet;
			if (row != null)
			{
				PXSelectBase<INKitSpecStkDet> select = new PXSelect<INKitSpecStkDet,
				Where<INKitSpecStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
				And<INKitSpecStkDet.revisionID, Equal<Current<INKitSpecHdr.revisionID>>,
				And<INKitSpecStkDet.compInventoryID, Equal<Required<INKitSpecStkDet.compInventoryID>>,
				And<INKitSpecStkDet.compSubItemID, Equal<Required<INKitSpecStkDet.compSubItemID>>>>>>>(this);

				PXResultset<INKitSpecStkDet> res = select.Select(e.NewValue, row.CompSubItemID);

				if (res.Count > 0)
				{
					e.Cancel = true;
					sender.RaiseExceptionHandling<INKitSpecStkDet.compInventoryID>(e.Row, e.NewValue, new PXSetPropertyException(Messages.KitItemMustBeUniqueAccrosSubItems));
				}
			}
		}

		protected virtual void INKitSpecStkDet_CompSubItemID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INKitSpecStkDet row = e.Row as INKitSpecStkDet;
			if (row != null)
			{
				PXSelectBase<INKitSpecStkDet> select = new PXSelect<INKitSpecStkDet,
				Where<INKitSpecStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
				And<INKitSpecStkDet.revisionID, Equal<Current<INKitSpecHdr.revisionID>>,
				And<INKitSpecStkDet.compInventoryID, Equal<Required<INKitSpecStkDet.compInventoryID>>,
				And<INKitSpecStkDet.compSubItemID, Equal<Required<INKitSpecStkDet.compSubItemID>>>>>>>(this);

				PXResultset<INKitSpecStkDet> res = select.Select(row.CompInventoryID, e.NewValue);

				if (res.Count > 0)
				{
					e.Cancel = true;
					sender.RaiseExceptionHandling<INKitSpecStkDet.compSubItemID>(e.Row, e.NewValue, new PXSetPropertyException(Messages.KitItemMustBeUniqueAccrosSubItems));
				}
			}
		}
		
		protected virtual void INKitSpecStkDet_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			INKitSpecStkDet row = e.NewRow as INKitSpecStkDet;
			if (row != null)
			{
				if (row.AllowQtyVariation == true)
				{
					if(	((row.MinCompQty != null) && (row.DfltCompQty < row.MinCompQty))
						|| ((row.MaxCompQty != null) && (row.DfltCompQty > row.MaxCompQty))	)
					{
                        throw new PXSetPropertyException(Messages.DfltQtyShouldBeBetweenMinAndMaxQty);
					}
				}

				//if (row.KitInventoryID == row.CompInventoryID)
				//{
				//   throw new PXSetPropertyException(Messages.KitMayNotIncludeItselfAsComponentPart);
				//}
			}
		}

		protected virtual void INKitSpecStkDet_CompInventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INKitSpecStkDet row = e.Row as INKitSpecStkDet;
			if (row != null)
			{
				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, row.CompInventoryID);
				if (item != null)
				{
					row.UOM = item.BaseUnit;
				}
			}
		}

        
		protected virtual void INKitSpecStkDet_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			INKitSpecStkDet row = e.Row as INKitSpecStkDet;
			if (row != null)
			{
				PXSelectBase<INKitSpecStkDet> select = new PXSelect<INKitSpecStkDet,
				Where<INKitSpecStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
				And<INKitSpecStkDet.revisionID, Equal<Current<INKitSpecHdr.revisionID>>,
				And<INKitSpecStkDet.compInventoryID, Equal<Required<INKitSpecStkDet.compInventoryID>>,
				And<INKitSpecStkDet.compSubItemID, Equal<Required<INKitSpecStkDet.compSubItemID>>>>>>>(this);

				PXResultset<INKitSpecStkDet> res = select.Select(row.CompInventoryID, row.CompSubItemID);

				if (res.Count > 1)
				{
					InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, row.CompInventoryID);
					if (sender.RaiseExceptionHandling<INKitSpecStkDet.compInventoryID>(e.Row, item.InventoryCD, new PXException(Messages.KitItemMustBeUniqueAccrosSubItems)))
					{
						throw new PXRowPersistingException(typeof(INKitSpecStkDet.compInventoryID).Name, item.InventoryCD, Messages.KitItemMustBeUniqueAccrosSubItems);
					}
				}
			}
		}


		protected virtual void INKitSpecNonStkDet_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			INKitSpecNonStkDet row = e.NewRow as INKitSpecNonStkDet;
			if (row != null)
			{
				if (row.AllowQtyVariation == true)
				{
					if (((row.MinCompQty != null) && (row.DfltCompQty < row.MinCompQty))
						|| ((row.MaxCompQty != null) && (row.DfltCompQty > row.MaxCompQty)))
					{
						throw new PXSetPropertyException(typeof(INKitSpecNonStkDet.dfltCompQty).Name, null, Messages.DfltQtyShouldBeBetweenMinAndMaxQty);
					}
				}

				if (row.KitInventoryID == row.CompInventoryID)
				{
                    throw new PXSetPropertyException(typeof(INKitSpecNonStkDet.compInventoryID).Name, null, Messages.KitMayNotIncludeItselfAsComponentPart);
				}
			}
		}       

		protected virtual void INKitSpecNonStkDet_CompInventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
            INKitSpecNonStkDet row = e.Row as INKitSpecNonStkDet;
            if (row != null)
            {
                InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, row.CompInventoryID);
                if (item != null)
                {
                    row.UOM = item.BaseUnit;
                }
            }
		}

		protected virtual void INKitSpecNonStkDet_CompInventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INKitSpecNonStkDet row = e.Row as INKitSpecNonStkDet;
			if (row != null)
			{
				PXSelectBase<INKitSpecNonStkDet> select = new PXSelect<INKitSpecNonStkDet,
				Where<INKitSpecNonStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
				And<INKitSpecNonStkDet.revisionID, Equal<Current<INKitSpecHdr.revisionID>>,
				And<INKitSpecNonStkDet.compInventoryID, Equal<Required<INKitSpecStkDet.compInventoryID>>>>>>(this);

				PXResultset<INKitSpecNonStkDet> res = select.Select(e.NewValue);

				if (res.Count > 0)
				{
                    InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, e.NewValue);
                    e.NewValue = item.InventoryCD;
                    throw new PXSetPropertyException(Messages.KitItemMustBeUnique);
				}
			}
		}

		protected virtual void INKitSpecNonStkDet_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			INKitSpecNonStkDet row = e.Row as INKitSpecNonStkDet;
			if (row != null)
			{
				PXSelectBase<INKitSpecNonStkDet> select = new PXSelect<INKitSpecNonStkDet,
				Where<INKitSpecNonStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
				And<INKitSpecNonStkDet.revisionID, Equal<Current<INKitSpecHdr.revisionID>>,
				And<INKitSpecNonStkDet.compInventoryID, Equal<Required<INKitSpecStkDet.compInventoryID>>>>>>(this);

				PXResultset<INKitSpecNonStkDet> res = select.Select(row.CompInventoryID);

				if (res.Count > 1)
				{
					InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, row.CompInventoryID);
					if (sender.RaiseExceptionHandling<INKitSpecNonStkDet.compInventoryID>(e.Row, item.InventoryCD, new PXException(Messages.KitItemMustBeUnique)))
					{
						throw new PXRowPersistingException(typeof(INKitSpecNonStkDet.compInventoryID).Name, item.InventoryCD, Messages.KitItemMustBeUnique);
					}
				}
			}
		}

        [NonStockItem(DisplayName = "Component ID")]
        [PXDefault()]
        [PXRestrictor(typeof(Where<InventoryItem.inventoryID, NotEqual<Current<INKitSpecHdr.kitInventoryID>>>), Messages.UsingKitAsItsComponent)]
        protected virtual void INKitSpecNonStkDet_CompInventoryID_CacheAttached(PXCache sender)
        { }

        [Inventory(typeof(InnerJoin<INLotSerClass, On<INLotSerClass.lotSerClassID, Equal<InventoryItem.lotSerClassID>>>), typeof(Where<InventoryItem.stkItem, Equal<boolTrue>>))]
        [PXDefault()]
        [PXRestrictor(typeof(Where<INLotSerTrack.serialNumbered, Equal<Current<INKitSpecHdr.lotSerTrack>>,
                                  Or<INLotSerClass.lotSerTrack, Equal<INLotSerTrack.lotNumbered>,
                                  Or<INLotSerClass.lotSerTrack, Equal<INLotSerTrack.notNumbered>>>
                          >), Messages.SNComponentInSNKit)]
        protected virtual void INKitSpecStkDet_CompInventoryID_CacheAttached(PXCache sender)
        { }
	}
}
