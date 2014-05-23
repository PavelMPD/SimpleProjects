using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.SO;
using PX.Objects.GL;
using System.Collections;
using PX.TM;
using PX.Objects.CS;
using PX.Objects.AR;

namespace PX.Objects.IN
{
	[PX.Objects.GL.TableAndChartDashboardType]    
    [Serializable]
	public class INUpdateBasePrice : PXGraph<INUpdateBasePrice>
	{
		public PXCancel<ItemFilter> Cancel;
		public PXFilter<ItemFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessing<UpdateBasePriceRecord, ItemFilter>
			Items;
		
		public INUpdateBasePrice()
		{
			Items.SetSelected<UpdateBasePriceRecord.selected>();

			Items.SetProcessDelegate<INUpdateBasePriceProcess>(UpdateBasePrice);
			Items.SetProcessCaption(Messages.Process);
			Items.SetProcessAllCaption(Messages.ProcessAll);
		}

		public virtual IEnumerable items()
		{
			if (Filter.Current == null)
			{
				yield break;
			}
			Items.Cache.Clear();

			PXSelectBase<InventoryItem> select = new PXSelect<InventoryItem,
					Where2<Match<Current<AccessInfo.userName>>,
					And<InventoryItem.pendingBasePriceDate, LessEqual<Current<ItemFilter.pendingBasePriceDate>>>>>(this);

			if (this.Filter.Current != null && !string.IsNullOrEmpty(this.Filter.Current.PriceClassID))
			{
				select.WhereAnd<Where<InventoryItem.priceClassID, Equal<Current<ItemFilter.priceClassID>>>>();
			}

            if (this.Filter.Current != null && this.Filter.Current.PriceManagerID != null )
            {
				select.WhereAnd<Where<InventoryItem.priceManagerID, Equal<Current<ItemFilter.priceManagerID>>>>();
            }

            if (this.Filter.Current != null && this.Filter.Current.WorkGroupID != null)
            {
				select.WhereAnd<Where<InventoryItem.priceWorkgroupID, Equal<Current<ItemFilter.workGroupID>>>>();
            }

			foreach (InventoryItem item in select.Select())
			{
				UpdateBasePriceRecord record = new UpdateBasePriceRecord();
				record.InventoryID = item.InventoryID;
				record.Descr = item.Descr;
				record.PendingBasePrice = item.PendingBasePrice;
				record.PendingBasePriceDate = item.PendingBasePriceDate;
				record.BasePrice = item.BasePrice;
				record.BasePriceDate = item.BasePriceDate;
				record.PriceClassID = item.PriceClassID;
				record.BaseUnit = item.BaseUnit;
				record.SalesUnit = item.SalesUnit;

				yield return Items.Insert(record);
			}

			Items.Cache.IsDirty = false;
		}

		public static void UpdateBasePrice(INUpdateBasePriceProcess graph, UpdateBasePriceRecord item)
		{
			graph.UpdateBasePrice(item);
		}


		#region Local Types

        [Serializable]
		public partial class ItemFilter : IBqlTable
		{
            #region CurrentUserID
            public abstract class currentUserID : PX.Data.IBqlField
            {
            }

			[PXDBGuid]
			[CR.CRCurrentOwnerID]
			public virtual Guid? CurrentUserID { get; set; }
            #endregion
            #region PriceManagerID
            public abstract class priceManagerID : PX.Data.IBqlField
            {
            }
            protected Guid? _PriceManagerID;
            [PXDBGuid]
            [PXUIField(DisplayName = "Price Manager")]
            [PX.TM.PXSubordinateOwnerSelector]
            public virtual Guid? PriceManagerID
            {
                get
                {
                    return (_MyUser == true) ? CurrentUserID : _PriceManagerID;
                }
                set
                {
                    _PriceManagerID = value;
                }
            }
            #endregion
            #region MyUser
            public abstract class myUser : PX.Data.IBqlField
            {
            }
            protected Boolean? _MyUser;
            [PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Me")]
            public virtual Boolean? MyUser
            {
                get
                {
                    return _MyUser;
                }
                set
                {
                    _MyUser = value;
                }
            }
            #endregion
            #region WorkGroupID
            public abstract class workGroupID : PX.Data.IBqlField
            {
            }
            protected Int32? _WorkGroupID;
            [PXDBInt]
            [PXUIField(DisplayName = "Price Workgroup")]
            [PXSelector(typeof(Search<EPCompanyTree.workGroupID,
                Where<EPCompanyTree.workGroupID, Owned<Current<AccessInfo.userID>>>>),
             SubstituteKey = typeof(EPCompanyTree.description))]
            public virtual Int32? WorkGroupID
            {
                get
                {
                    return (_MyWorkGroup == true) ? null : _WorkGroupID;
                }
                set
                {
                    _WorkGroupID = value;
                }
            }
            #endregion
            #region MyWorkGroup
            public abstract class myWorkGroup : PX.Data.IBqlField
            {
            }
            protected Boolean? _MyWorkGroup;
            [PXDefault(false)]
            [PXDBBool]
            [PXUIField(DisplayName = "My", Visibility = PXUIVisibility.Visible)]
            public virtual Boolean? MyWorkGroup
            {
                get
                {
                    return _MyWorkGroup;
                }
                set
                {
                    _MyWorkGroup = value;
                }
            }
            #endregion
            
			#region PendingBasePriceDate
			public abstract class pendingBasePriceDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _PendingBasePriceDate;
			[PXDBDate()]
			[PXUIField(DisplayName = "Max. Pending Base Price Date")]
			[PXDefault(typeof(AccessInfo.businessDate))]
			public virtual DateTime? PendingBasePriceDate
			{
				get
				{
					return this._PendingBasePriceDate;
				}
				set
				{
					this._PendingBasePriceDate = value;
				}
			}
			#endregion
			#region PriceClassID
			public abstract class priceClassID : PX.Data.IBqlField
			{
			}
			protected String _PriceClassID;
			[PXDBString(10, IsUnicode = true)]
			[PXSelector(typeof(INPriceClass.priceClassID))]
			[PXUIField(DisplayName = "Price Class", Visibility = PXUIVisibility.Visible)]
			public virtual String PriceClassID
			{
				get
				{
					return this._PriceClassID;
				}
				set
				{
					this._PriceClassID = value;
				}
			}
			#endregion

			
		}
				
		[PXPrimaryGraph(new Type[] {
		            typeof(NonStockItemMaint),
		            typeof(InventoryItemMaint)},
				new Type[] {
		            typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<UpdateBasePriceRecord.inventoryID>>, And<InventoryItem.stkItem, Equal<False>>>>),
		            typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<UpdateBasePriceRecord.inventoryID>>, And<InventoryItem.stkItem, Equal<True>>>>) 
		            })]
        [Serializable]
		public partial class UpdateBasePriceRecord : IBqlTable
		{
			#region Selected
			public abstract class selected : PX.Data.IBqlField
			{
			}
			protected Boolean? _Selected = false;
			[PXBool()]
			[PXUIField(DisplayName = "Selected")]
			public virtual Boolean? Selected
			{
				get
				{
					return this._Selected;
				}
				set
				{
					this._Selected = value;
				}
			}
			#endregion
			#region InventoryID
			public abstract class inventoryID : PX.Data.IBqlField
			{
			}
			protected Int32? _InventoryID;
			[Inventory(IsKey = true, DirtyRead = true, DisplayName = "Inventory ID")]
			public virtual Int32? InventoryID
			{
				get
				{
					return this._InventoryID;
				}
				set
				{
					this._InventoryID = value;
				}
			}
			#endregion
			#region Descr
			public abstract class descr : PX.Data.IBqlField
			{
			}
			protected String _Descr;
			[PXDBString(255, IsUnicode = true)]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String Descr
			{
				get
				{
					return this._Descr;
				}
				set
				{
					this._Descr = value;
				}
			}
			#endregion
			#region PendingBasePrice
			public abstract class pendingBasePrice : PX.Data.IBqlField
			{
			}
			protected Decimal? _PendingBasePrice;
			[PXDBPriceCost()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Pending Price")]
			public virtual Decimal? PendingBasePrice
			{
				get
				{
					return this._PendingBasePrice;
				}
				set
				{
					this._PendingBasePrice = value;
				}
			}
			#endregion
			#region PendingBasePriceDate
			public abstract class pendingBasePriceDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _PendingBasePriceDate;
			[PXDBDate()]
			[PXUIField(DisplayName = "Pending Price Date", Enabled = true)]
			public virtual DateTime? PendingBasePriceDate
			{
				get
				{
					return this._PendingBasePriceDate;
				}
				set
				{
					this._PendingBasePriceDate = value;
				}
			}
			#endregion
			#region BasePrice
			public abstract class basePrice : PX.Data.IBqlField
			{
			}
			protected Decimal? _BasePrice;
			[PXDBPriceCost()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Current Price", Enabled = false)]
			public virtual Decimal? BasePrice
			{
				get
				{
					return this._BasePrice;
				}
				set
				{
					this._BasePrice = value;
				}
			}
			#endregion
			#region BasePriceDate
			public abstract class basePriceDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _BasePriceDate;
			[PXDBDate()]
			[PXUIField(DisplayName = "Effective Date", Enabled = false)]
			public virtual DateTime? BasePriceDate
			{
				get
				{
					return this._BasePriceDate;
				}
				set
				{
					this._BasePriceDate = value;
				}
			}
			#endregion
			#region PriceClassID
			public abstract class priceClassID : PX.Data.IBqlField
			{
			}
			protected String _PriceClassID;
			[PXDBString(10, IsUnicode = true)]
			[PXSelector(typeof(INPriceClass.priceClassID), DescriptionField = typeof(INPriceClass.description))]
			[PXUIField(DisplayName = "Price Class", Visibility = PXUIVisibility.Visible)]
			public virtual String PriceClassID
			{
				get
				{
					return this._PriceClassID;
				}
				set
				{
					this._PriceClassID = value;
				}
			}
			#endregion
			#region BaseUnit
			public abstract class baseUnit : PX.Data.IBqlField
			{
			}
			protected String _BaseUnit;
			[INUnit(DisplayName = "Base Unit", Visibility = PXUIVisibility.Visible)]
			public virtual String BaseUnit
			{
				get
				{
					return this._BaseUnit;
				}
				set
				{
					this._BaseUnit = value;
				}
			}
			#endregion
			#region SalesUnit
			public abstract class salesUnit : PX.Data.IBqlField
			{
			}
			protected String _SalesUnit;
			[INUnit(typeof(UpdateBasePriceRecord.inventoryID), DisplayName = "Sales Unit", Visibility = PXUIVisibility.Visible)]
			public virtual String SalesUnit
			{
				get
				{
					return this._SalesUnit;
				}
				set
				{
					this._SalesUnit = value;
				}
			}
			#endregion
		}

		#endregion
	}

	public class INUpdateBasePriceProcess : PXGraph<INUpdateBasePriceProcess>
	{
		public PXSetup<Company> Company;

		public virtual void UpdateBasePrice(PX.Objects.IN.INUpdateBasePrice.UpdateBasePriceRecord item)
		{
			using (PXConnectionScope cs = new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					DateTime updateTime = DateTime.Now;
					string uom = item.BaseUnit;
					decimal? salesPrice = item.PendingBasePrice;
					decimal? lastSalesPrice = item.BasePrice;

                    //So that UI reflects the change:
					item.BasePrice = item.PendingBasePrice;
					item.BasePriceDate = item.PendingBasePriceDate;
					item.PendingBasePrice = 0;
					item.PendingBasePriceDate = null;

					PXDatabase.Update<InventoryItem>(
									new PXDataFieldAssign("BasePrice", PXDbType.DirectExpression, "PendingBasePrice"),
									new PXDataFieldAssign("BasePriceDate", PXDbType.DirectExpression, "PendingBasePriceDate"),
									new PXDataFieldAssign("PendingBasePrice", PXDbType.Decimal, 0m),
									new PXDataFieldAssign("PendingBasePriceDate", PXDbType.DateTime, null),
									new PXDataFieldAssign("LastBasePrice", PXDbType.DirectExpression, "BasePrice"),
									new PXDataFieldAssign("LastModifiedDateTime", PXDbType.DateTime, updateTime),
									new PXDataFieldRestrict("InventoryID", PXDbType.Int, item.InventoryID)
									);
					
					if (SalesPriceUpdateUnit == SalesPriceUpdateUnitType.SalesUnit)
					{
						uom = item.SalesUnit;
						salesPrice = INUnitAttribute.ConvertFromBase(this.Caches[typeof(PX.Objects.IN.INUpdateBasePrice.UpdateBasePriceRecord)], item.InventoryID, item.SalesUnit, item.PendingBasePrice.Value, INPrecision.UNITCOST);
						lastSalesPrice = INUnitAttribute.ConvertFromBase(this.Caches[typeof(PX.Objects.IN.INUpdateBasePrice.UpdateBasePriceRecord)], item.InventoryID, item.SalesUnit, item.BasePrice.Value, INPrecision.UNITCOST);
					}

					PXDatabase.Update<ARSalesPrice>(
									new PXDataFieldAssign("SalesPrice", PXDbType.Decimal, salesPrice),
									new PXDataFieldAssign("LastDate", PXDbType.DirectExpression, "EffectiveDate"),
									new PXDataFieldAssign("PendingPrice", PXDbType.Decimal, 0m),
									new PXDataFieldAssign("EffectiveDate", PXDbType.DateTime, null),
									new PXDataFieldAssign("LastPrice", PXDbType.Decimal, lastSalesPrice),
									new PXDataFieldAssign("LastModifiedDateTime", PXDbType.DateTime, updateTime),
									new PXDataFieldRestrict("InventoryID", PXDbType.Int, item.InventoryID),
                                    new PXDataFieldRestrict("CustPriceClassID", PXDbType.VarChar, AR.ARPriceClass.EmptyPriceClass),
                                    new PXDataFieldRestrict("CuryID", PXDbType.VarChar, Company.Current.BaseCuryID),
									new PXDataFieldRestrict("UOM", PXDbType.VarChar, uom)
									);


					ts.Complete();
				}
			}
			
		}

		private string SalesPriceUpdateUnit
		{
			get
			{
				SOSetup sosetup = PXSelect<SOSetup>.Select(this);
				if (sosetup != null && !string.IsNullOrEmpty(sosetup.SalesPriceUpdateUnit))
				{
					return sosetup.SalesPriceUpdateUnit;
				}

				return SalesPriceUpdateUnitType.BaseUnit;
			}
		}
	}
}
