using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using Branch = PX.Objects.GL.Branch;

namespace PX.Objects.IN
{
	public class INSiteMaint : PXGraph<INSiteMaint, INSite>
	{
		public PXSelect<BAccount> _bAccount;		
		public PXSelect<INSite, Where<Match<Current<AccessInfo.userName>>>> site;
		public PXSelect<INSite, Where<INSite.siteID,Equal<Current<INSite.siteID>>>> siteaccounts;
		[PXFilterable]
		public PXSelect<INLocation, Where<INLocation.siteID, Equal<Current<INSite.siteID>>>> location;
		public PXSetup<Branch, Where<Branch.branchID, Equal<Optional<INSite.branchID>>>> branch;
		public PXSelect<Address, Where<Address.bAccountID, Equal<Current<Branch.bAccountID>>,
					And<Address.addressID, Equal<Current<INSite.addressID>>>>> Address;
		public PXSelect<Contact, Where<Contact.bAccountID, Equal<Current<Branch.bAccountID>>,
					And<Contact.contactID, Equal<Current<INSite.contactID>>>>> Contact;
		public PXSelect<INItemSite, Where<INItemSite.siteID, Equal<Current<INSite.siteID>>>> itemsiterecords;
		public PXSetup<INSetup> insetup;

		public PXAction<INSite> viewRestrictionGroups;
		[PXUIField(DisplayName = GL.Messages.ViewRestrictionGroups, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewRestrictionGroups(PXAdapter adapter)
		{
			if (site.Current != null)
			{
				INAccessDetail graph = CreateInstance<INAccessDetail>();
				graph.Site.Current = graph.Site.Search<INSite.siteCD>(site.Current.SiteCD);
				throw new PXRedirectRequiredException(graph, false, "Restricted Groups");
			}
			return adapter.Get();
		}


		public PXAction<INSite> iNLocationLabels;
		[PXUIField(DisplayName = Messages.INLocationLabels, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable INLocationLabels(PXAdapter adapter)
		{
			if (site.Current != null)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["WarehouseID"] = site.Current.SiteCD;
				throw new PXReportRequiredException(parameters, "IN619000", Messages.INLocationLabels);
			}
			return adapter.Get();
		}

		#region Buttons
		public PXAction<INSite> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton]
		public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			INSite inSite = this.site.Current;
			if (inSite != null)
			{
				bool needSave = false;
				Save.Press();
				Address address = this.Address.Current;
				if (address != null && address.IsValidated == false)
				{
					PXAddressValidator.Validate<Address>(this, address, true);
					needSave = true;
				}				
				if (needSave == true)
					this.Save.Press();
			}
			return adapter.Get();
		}
		#endregion
		
		#region MyButtons (MMK)
		public PXAction<INSite> action;
        [PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable Action(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<INSite> report;
        [PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable Report(PXAdapter adapter)
        {
            return adapter.Get();
        }
        #endregion

		public INSiteMaint()
		{
			INSetup record = insetup.Current;
			PXDBDefaultAttribute.SetDefaultForInsert<INLocation.siteID>(location.Cache, null, true);
			PXDBDefaultAttribute.SetDefaultForUpdate<INLocation.siteID>(location.Cache, null, true);

			PXUIFieldAttribute.SetVisible<INSite.pPVAcctID>(siteaccounts.Cache, null, true);
			PXUIFieldAttribute.SetVisible<INSite.pPVSubID>(siteaccounts.Cache, null, true);

			PXUIFieldAttribute.SetVisible<INSite.discAcctID>(siteaccounts.Cache, null, false);
			PXUIFieldAttribute.SetVisible<INSite.discSubID>(siteaccounts.Cache, null, false);

			PXUIFieldAttribute.SetVisible<INSite.freightAcctID>(siteaccounts.Cache, null, false);
			PXUIFieldAttribute.SetVisible<INSite.freightSubID>(siteaccounts.Cache, null, false);

			PXUIFieldAttribute.SetVisible<INSite.miscAcctID>(siteaccounts.Cache, null, false);
			PXUIFieldAttribute.SetVisible<INSite.miscSubID>(siteaccounts.Cache, null, false);

			PXUIFieldAttribute.SetDisplayName(Caches[typeof(Contact)], typeof(Contact.salutation).Name, CR.Messages.Attention);

			PXUIFieldAttribute.SetEnabled<Contact.fullName>(Caches[typeof(Contact)], null);
		}

		[PXDefault(typeof(Search<Branch.countryID, Where<Branch.branchID, Equal<Current<INSite.branchID>>>>))]
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Country")]
		[PXSelector(typeof(Search<Country.countryID>), DescriptionField = typeof(Country.description), CacheGlobal = true)]
		protected virtual void Address_CountryID_CacheAttached(PXCache sender)
		{ 
		}

		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(INSite.siteID), DefaultForInsert = false, DefaultForUpdate = false)]
		[PXParent(typeof(Select<INSite, Where<INSite.siteID, Equal<Current<INLocation.siteID>>>>))]
		protected virtual void INLocation_SiteID_CacheAttached(PXCache sender)
		{ 
		}

		protected virtual void INSite_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INAcctSubDefault.Required(sender, e);
			viewRestrictionGroups.SetEnabled(e.Row != null && ((INSite)e.Row).SiteCD != null);
		}		

		protected virtual void INSite_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			INSite row = (INSite)e.Row;
			if(row != null)
			{
				PXResult<INSiteStatus, InventoryItem> status =
				(PXResult<INSiteStatus, InventoryItem>)
				PXSelectJoin<INSiteStatus,
					InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INSiteStatus.inventoryID>>>,
					Where<INSiteStatus.siteID, Equal<Required<INSiteStatus.siteID>>>>
				.SelectWindowed(this, 0, 1, row.SiteID, 0m);

				if (status != null && ((InventoryItem)status).InventoryCD != null)
				{
					e.Cancel = true;
					throw new PXRowPersistingException(typeof(INSite).Name, row, Messages.SiteUsageDeleted, ((InventoryItem)status).InventoryCD);
				}
			}
		}
		protected virtual void INSite_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			INAcctSubDefault.Required(sender, e);
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || 
					(e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				INSite site = ((INSite)e.Row);
				if(site.OverrideInvtAccSub != true)
				{
					PXDefaultAttribute.SetPersistingCheck<INSite.invtAcctID>(sender, e.Row, PXPersistingCheck.Nothing);
					PXDefaultAttribute.SetPersistingCheck<INSite.invtSubID>(sender, e.Row, PXPersistingCheck.Nothing);
				}
				if(site.ReceiptLocationIDOverride == true || site.ShipLocationIDOverride == true)
				{
					List<PXDataFieldParam> prm = new List<PXDataFieldParam>();
					if(site.ReceiptLocationIDOverride == true)
						prm.Add( new PXDataFieldAssign(typeof(INItemSite.dfltReceiptLocationID).Name, PXDbType.Int, site.ReceiptLocationID));
					if(site.ShipLocationIDOverride == true)
						prm.Add( new PXDataFieldAssign(typeof(INItemSite.dfltShipLocationID).Name, PXDbType.Int, site.ShipLocationID));
					prm.Add( new PXDataFieldRestrict(typeof(INItemSite.siteID).Name,PXDbType.Int, site.SiteID));
					PXDatabase.Update<INItemSite>(prm.ToArray());
				}

				if (site.Active != true) 
				{
					if ((INRegister)PXSelect<INRegister
						, Where<
							INRegister.released, NotEqual<True>
							, And<Where<INRegister.siteID, Equal<Current<INSite.siteID>>, Or<INRegister.toSiteID, Equal<Current<INSite.siteID>>>>>
						>>.SelectSingleBound(this, new object[] { e.Row }) != null
						|| (INTran)PXSelect<INTran
						, Where<
							INTran.released, NotEqual<True>
							, And<Where<INTran.siteID, Equal<Current<INSite.siteID>>, Or<INTran.toSiteID, Equal<Current<INSite.siteID>>>>>
						>>.SelectSingleBound(this, new object[] { e.Row }) != null)
					{
						sender.RaiseExceptionHandling<INSite.active>(e.Row, null, new PXSetPropertyException(Messages.CantDeactivateSite));
					}
				}

			}

		}

		protected string[] _WrongLocations = new string[] { null, null, null, null };

		protected virtual void INSite_ReceiptLocationID_ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e)
		{
			if (e.Cancel = IsImport)
			{
				_WrongLocations[0] = e.NewValue as string;
			}
		}

		protected virtual void INSite_ReturnLocationID_ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e)
		{
			if (e.Cancel = IsImport)
			{
				_WrongLocations[1] = e.NewValue as string;
			}
		}

		protected virtual void INSite_DropShipLocationID_ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e)
		{
			if (e.Cancel = IsImport)
			{
				_WrongLocations[2] = e.NewValue as string;
			}
		}

		protected virtual void INSite_ShipLocationID_ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e)
		{
			if (e.Cancel = IsImport)
			{
				_WrongLocations[3] = e.NewValue as string;
			}
		}

		protected virtual void INLocation_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			string cd;
			if (site.Current != null && IsImport && (cd = ((INLocation)e.Row).LocationCD) != null)
			{
				if (_WrongLocations[0] == cd)
				{
					site.Current.ReceiptLocationID = ((INLocation)e.Row).LocationID;
					_WrongLocations[0] = null;
				}
				if (_WrongLocations[1] == cd)
				{
					site.Current.ReturnLocationID = ((INLocation)e.Row).LocationID;
					_WrongLocations[1] = null;
				}
				if (_WrongLocations[2] == cd)
				{
					site.Current.DropShipLocationID = ((INLocation)e.Row).LocationID;
					_WrongLocations[2] = null;
				}
				if (_WrongLocations[3] == cd)
				{
					site.Current.ShipLocationID = ((INLocation)e.Row).LocationID;
					_WrongLocations[3] = null;
				}
			}
		}

		public override void Clear()
		{
			base.Clear();
			_WrongLocations[0] = null;
			_WrongLocations[1] = null;
			_WrongLocations[2] = null;
			_WrongLocations[3] = null;
		}

		protected virtual void INLocation_IsCosted_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null) return;

			bool enable;
			if ((bool?)e.NewValue == true)
			{
				INLocationStatus status =
				PXSelect<INLocationStatus,
					Where<INLocationStatus.siteID, Equal<Current<INLocation.siteID>>,
						And<INLocationStatus.locationID, Equal<Current<INLocation.locationID>>,
							And<INLocationStatus.qtyOnHand, Greater<decimal0>>>>>.SelectSingleBound(this, new object[] { e.Row });
							enable = status == null;

			}
			else
			{
				INCostStatus status =
				PXSelect<INCostStatus,
					Where<INCostStatus.costSiteID, Equal<Current<INLocation.locationID>>,
						And<INCostStatus.qtyOnHand, Greater<decimal0>>>>.SelectSingleBound(this, new object[] { e.Row });
							enable = status == null;
			}

			if (!enable)
				throw new PXSetPropertyException(Messages.LocationCostedWarning, PXErrorLevel.Error);

			if ((bool?)e.NewValue == true)
			{
				sender.RaiseExceptionHandling<INLocation.isCosted>(e.Row, true,
						new PXSetPropertyException(Messages.LocationCostedSetWarning, PXErrorLevel.RowWarning));
			}
		}
		
		protected virtual void INLocation_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			INLocation l = (INLocation)e.Row;
			if (this.site.Current == null || l == null) return;
			if(site.Cache.GetStatus(this.site.Current) == PXEntryStatus.Deleted) return;
			INSite s = PXCache<INSite>.CreateCopy(this.site.Current);
			if (s.DropShipLocationID == l.LocationID)
				s.DropShipLocationID = null;
			if (s.ReceiptLocationID == l.LocationID)
				s.ReceiptLocationID = null;
			if (s.ShipLocationID == l.LocationID)
				s.ShipLocationID = null;
			if (s.ReturnLocationID == l.LocationID)
				s.ReturnLocationID = null;
			this.site.Update(s);
		}
		
		protected virtual void Address_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Address addr = e.Row as Address;
			if (addr != null)
			{
				site.Current.AddressID = addr.AddressID;
			}
		}
		protected virtual void Contact_ContactType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = ContactTypesAttribute.BAccountProperty;
		}
		protected virtual void Contact_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Contact cont = e.Row as Contact;
			if (cont != null)
			{
				site.Current.ContactID = cont.ContactID;
			}
		}

		protected virtual void Address_BAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (branch.Current != null)
			{
				e.NewValue = (int?)branch.Cache.GetValue<Branch.bAccountID>(branch.Current);
				e.Cancel = true;
			}
		}

		protected virtual void Address_CountryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Address address = (Address)e.Row;
			address.State = null;
			address.PostalCode = null;
		}
		
		protected virtual void Contact_BAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (branch.Current != null)
			{
				e.NewValue = (int?)branch.Cache.GetValue<Branch.bAccountID>(branch.Current);
				e.Cancel = true;
			}
		}

		protected virtual void Contact_DefAddressID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		//protected virtual void INSite_Active_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		//{
		//    if (((e.NewValue as bool?) ?? false) != true)
		//    {
		//        if ((INRegister)PXSelect<INRegister, Where<INRegister.siteID, Equal<Current<INSite.siteID>>, And<INRegister.released, NotEqual<True>>>>.SelectSingleBound(this, new object[] { e.Row }) != null 
		//            || (INTran)PXSelect<INTran, Where<INTran.siteID, Equal<Current<INSite.siteID>>, And<INTran.released, NotEqual<True>>>>.SelectSingleBound(this, new object[] { e.Row }) != null)
		//        {
		//            throw new PXSetPropertyException(Messages.CantDeactivateSite);
		//        }
		//    }
		//}

	    protected virtual void INSite_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{			
			try
			{
				Contact cont = new Contact();
				Address addr = new Address();

				addr.BAccountID = (int?)branch.Cache.GetValue<Branch.bAccountID>(branch.Current);
				addr = (Address)Address.Cache.Insert(addr);

				cont.BAccountID = (int?)branch.Cache.GetValue<Branch.bAccountID>(branch.Current);
				cont.DefAddressID = addr.AddressID;
				cont = (Contact)Contact.Cache.Insert(cont);				
			}
			finally 
			{
				Address.Cache.IsDirty = false;
				Contact.Cache.IsDirty = false;			
			}				
		}

		protected virtual void INSite_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<INSite.branchID>(e.Row, e.OldRow))
			{
				bool found = false;
				foreach (Address record in Address.Cache.Inserted)
				{
					record.BAccountID = (int?)branch.Cache.GetValue<Branch.bAccountID>(branch.Current);
					record.CountryID = (string)branch.Cache.GetValue<Branch.countryID>(branch.Current);
					found = true;
				}

				if (!found)
				{
					object old_branch = branch.View.SelectSingleBound(new object[] { e.OldRow });
					Address addr = (Address)Address.View.SelectSingleBound(new object[] { old_branch, e.OldRow }) ?? new Address();

					addr.BAccountID = (int?)branch.Cache.GetValue<Branch.bAccountID>(branch.Current);
					addr.CountryID = (string)branch.Cache.GetValue<Branch.countryID>(branch.Current);
					addr.AddressID = null;
					Address.Cache.Insert(addr);

				}
				else
				{
					Address.Cache.Normalize();
				}

				found = false;
				foreach (Contact cont in Contact.Cache.Inserted)
				{
					cont.BAccountID = (int?)branch.Cache.GetValue<Branch.bAccountID>(branch.Current);
					cont.DefAddressID = null;
					foreach (Address record in Address.Cache.Inserted)
					{
						cont.DefAddressID = record.AddressID;
					} 
					found = true;
				}

				if (!found)
				{
					object old_branch = branch.View.SelectSingleBound(new object[] { e.OldRow });
					Contact cont = (Contact)Contact.View.SelectSingleBound(new object[] { old_branch, e.OldRow }) ?? new Contact() ;

					cont.BAccountID = (int?)branch.Cache.GetValue<Branch.bAccountID>(branch.Current);
					cont.DefAddressID = null;
					foreach (Address record in Address.Cache.Inserted)
					{
						cont.DefAddressID = record.AddressID;
					} 
					cont.ContactID = null;
					Contact.Cache.Insert(cont);
				}
				else
				{
					Contact.Cache.Normalize();
				}
			}
		}
		
		protected virtual void INSite_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
		{
			UpateSiteLocation<INSite.receiptLocationID, INSite.receiptLocationIDOverride>(cache, e);
			UpateSiteLocation<INSite.shipLocationID, INSite.shipLocationIDOverride>(cache, e);
		}

		protected void UpateSiteLocation<Field, FieldResult>(PXCache cache, PXRowUpdatingEventArgs e)
			where Field : IBqlField
			where FieldResult : IBqlField
		{
			int? newValue = (int?)cache.GetValue<Field>(e.NewRow);
			int? value = (int?)cache.GetValue<Field>(e.Row);
			if (value != newValue && e.ExternalCall == true)
			{
				INItemSite itemsite =
					PXSelect<INItemSite,
						Where<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>.SelectWindowed(this, 0, 1,
						                                                                             cache.GetValue<INSite.siteID>(e.Row));
				if (itemsite != null &&
				    site.Ask(Messages.Warning, Messages.SiteLocationOverride, MessageButtons.YesNo) == WebDialogResult.Yes)
					cache.SetValue<FieldResult>(e.NewRow, true);
				else
					cache.SetValue<FieldResult>(e.NewRow, false);
			}
		}

		protected virtual void INSite_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			INSite row = (INSite)e.Row;
			Address.Cache.Delete(Address.Current);
			Contact.Cache.Delete(Contact.Current);
		}


		public PXAction<INSite> viewOnMap;		

		[PXUIField(DisplayName = CR.Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewOnMap(PXAdapter adapter)
		{
			BAccountUtility.ViewOnMap(this.Address.Current);
			return adapter.Get();
		}

	}
}
