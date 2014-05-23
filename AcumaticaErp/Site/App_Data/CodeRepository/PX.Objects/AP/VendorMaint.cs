using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.CA;
using PX.Objects.CR;
using PX.Objects.CM;
using PX.Objects.PO;
using PX.SM;


namespace PX.Objects.AP
{
	#region Specialized DAC Classes
	[PXSubstitute(GraphType = typeof(VendorMaint))]
	[System.SerializableAttribute()]
	public partial class VendorR : Vendor
	{
		public new abstract class bAccountID : IBqlField { }

		#region AcctCD
		public new abstract class acctCD : PX.Data.IBqlField
		{
		}
		[VendorRaw(typeof(Where<Vendor.type, Equal<BAccountType.vendorType>,
							 Or<Vendor.type, Equal<BAccountType.combinedType>>>), DescriptionField = typeof(Vendor.acctName), IsKey = true, DisplayName = "Vendor ID")]
		[PXDefault()]
		public override String AcctCD
		{
			get
			{
				return this._AcctCD;
			}
			set
			{
				this._AcctCD = value;
			}
		}
		#endregion
	}
	#endregion

	public class VendorMaint : BusinessAccountGraphBase<VendorR, VendorR, Where<BAccount.type, Equal<BAccountType.vendorType>,
							Or<BAccount.type, Equal<BAccountType.combinedType>>>>
	{

		#region InternalTypes
		[Serializable]
		[PXCacheName(Messages.VendorBalanceSummary)]
		public partial class VendorBalanceSummary : IBqlTable
		{
			#region VendorID
			public abstract class vendorID : PX.Data.IBqlField
			{
			}
			protected Int32? _VendorID;
			[PXDBInt()]
			[PXDefault()]
			public virtual Int32? VendorID
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
			#endregion
			#region Balance
			public abstract class balance : PX.Data.IBqlField
			{
			}
			protected Decimal? _Balance;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Balance", Visible = true, Enabled = false)]
			public virtual Decimal? Balance
			{
				get
				{
					return this._Balance;
				}
				set
				{
					this._Balance = value;
				}
			}
			#endregion
			#region DepositsBalance
			public abstract class depositsBalance : PX.Data.IBqlField
			{
			}
			protected Decimal? _DepositsBalance;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Prepayment Balance", Enabled = false)]
			public virtual Decimal? DepositsBalance
			{
				get
				{
					return this._DepositsBalance;
				}
				set
				{
					this._DepositsBalance = value;
				}
			}
			#endregion
			public virtual void Init()
			{
				if (!this.Balance.HasValue) this.Balance = Decimal.Zero;
				if (!this.DepositsBalance.HasValue) this.DepositsBalance = Decimal.Zero;
			}

		}
		#endregion

		#region Cache Attached
		#region NotificationSource
		[PXDBGuid(IsKey = true)]
		[PXSelector(typeof(Search<NotificationSetup.setupID,
			Where<NotificationSetup.sourceCD, Equal<APNotificationSource.vendor>>>),
			 SubstituteKey = typeof(NotificationSetup.notificationCD))]
		[PXUIField(DisplayName = "Mailing ID")]
		protected virtual void NotificationSource_SetupID_CacheAttached(PXCache sender)
		{
		}
		#region NBranchID
		[GL.Branch(null, IsDetail = false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCheckUnique(typeof(NotificationSource.setupID), IgnoreNulls = false,
			Where = typeof(Where<NotificationSource.refNoteID, Equal<Current<NotificationSource.refNoteID>>>))]
		protected virtual void NotificationSource_NBranchID_CacheAttached(PXCache sender)
		{
			
		}
		#endregion
		[PXDBString(10, IsUnicode = true)]
		protected virtual void NotificationSource_ClassID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(8, InputMask = "CC.CC.CC.CC")]
		[PXUIField(DisplayName = "Report")]
		[PXDefault(typeof(Search<NotificationSetup.reportID,
			Where<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<SiteMap.screenID,
		Where<SiteMap.url, Like<urlReports>,
			And<Where<SiteMap.screenID, Like<PXModule.ap_>,
						 Or<SiteMap.screenID, Like<PXModule.po_>,
						 Or<SiteMap.screenID, Like<PXModule.rq_>>>>>>,
		OrderBy<Asc<SiteMap.screenID>>>), typeof(SiteMap.screenID), typeof(SiteMap.title),
		Headers = new string[] { CA.Messages.ReportID, CA.Messages.ReportName },
		DescriptionField = typeof(SiteMap.title))]
		[PXFormula(typeof(Default<NotificationSource.setupID>))]
		protected virtual void NotificationSource_ReportID_CacheAttached(PXCache sender)
		{
		}
		
		#endregion

		#region NotificationRecipient		
		[PXDBInt]
		[PXDBLiteDefault(typeof(NotificationSource.sourceID))]
		protected virtual void NotificationRecipient_SourceID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(10)]
		[PXDefault]
		[VendorContactType.List]
		[PXUIField(DisplayName = "Contact Type")]
		[PXCheckUnique(typeof(NotificationRecipient.contactID),
			Where = typeof(Where<NotificationRecipient.sourceID, Equal<Current<NotificationRecipient.sourceID>>,			
			And<NotificationRecipient.refNoteID, Equal<Current<Vendor.noteID>>>>))]
		protected virtual void NotificationRecipient_ContactType_CacheAttached(PXCache sender)
		{
		}
		[PXDBInt]
		[PXUIField(DisplayName = "Contact ID")]
		[PXNotificationContactSelector(typeof(NotificationRecipient.contactType), DirtyRead = true)]
		protected virtual void NotificationRecipient_ContactID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(10, IsUnicode = true)]
		protected virtual void NotificationRecipient_ClassID_CacheAttached(PXCache sender)
		{
		}
		[PXString()]
		[PXUIField(DisplayName = "Email", Enabled = false)]
		protected virtual void NotificationRecipient_Email_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion

		#region Public Selects

		public PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Current<Vendor.bAccountID>>>> CurrentVendor;
		public PXSelect<Address, Where<Address.bAccountID, Equal<Current<Location.bAccountID>>,
					And<Address.addressID, Equal<Current<Location.vRemitAddressID>>>>> RemitAddress;
		public PXSelect<Contact, Where<Contact.bAccountID, Equal<Current<Location.bAccountID>>,
					And<Contact.contactID, Equal<Current<Location.vRemitContactID>>>>> RemitContact;
		public PXSelectJoin<VendorPaymentMethodDetail,
							InnerJoin<PaymentMethod, On<VendorPaymentMethodDetail.paymentMethodID, Equal<PaymentMethod.paymentMethodID>>,	
							InnerJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<VendorPaymentMethodDetail.paymentMethodID>,
							    And<PaymentMethodDetail.detailID, Equal<VendorPaymentMethodDetail.detailID>,
                                    And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
                                                Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>>>,
							Where<VendorPaymentMethodDetail.bAccountID, Equal<Optional<LocationExtAddress.bAccountID>>,
									And<VendorPaymentMethodDetail.locationID, Equal<Optional<LocationExtAddress.locationID>>,
									And<VendorPaymentMethodDetail.paymentMethodID, Equal<Optional<LocationExtAddress.vPaymentMethodID>>>>>, OrderBy<Asc<PaymentMethodDetail.orderIndex>>> PaymentDetails;
		public PXSelect<PaymentMethodDetail, 
                    Where<PaymentMethodDetail.paymentMethodID, Equal<Optional<LocationExtAddress.vPaymentMethodID>>,
                        And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
                                                Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>> PaymentTypeDetails;
		public PXSetup<VendorClass, Where<VendorClass.vendorClassID, Equal<Optional<Vendor.vendorClassID>>>> VendorClass;
		public PXSetup<APSetup> APSetup;
		[PXCopyPasteHiddenView]
		public PXSelect<VendorBalanceSummary> VendorBalance;

		public CRNotificationSourceList<Vendor, Vendor.vendorClassID, APNotificationSource.vendor> NotificationSources;

		public CRNotificationRecipientList<Vendor, Vendor.vendorClassID> NotificationRecipients;

		public PXSelect<POVendorInventory, Where<POVendorInventory.vendorID, Equal<Current<Vendor.bAccountID>>>> VendorItems;

		private void PopulateBoxList()
		{
			List<int> AllowedValues = new List<int>();
			List<string> AllowedLabels = new List<string>();

			foreach (AP1099Box box in PXSelectReadonly<AP1099Box>.Select(this, null))
			{
				AllowedValues.Add((int)box.BoxNbr);
				StringBuilder bld = new StringBuilder(box.BoxNbr.ToString());
				bld.Append("-");
				bld.Append(box.Descr);
				AllowedLabels.Add(bld.ToString());
			}

			if (AllowedValues.Count > 0)
			{
				PXIntListAttribute.SetList<Vendor.box1099>(CurrentVendor.Cache, null, AllowedValues.ToArray(), AllowedLabels.ToArray());
			}
		}

		public VendorMaint()
		{
			APSetup setup = APSetup.Current;
			Views.Caches.Remove(typeof(Vendor));

			PopulateBoxList();

			PXUIFieldAttribute.SetEnabled<Contact.fullName>(Caches[typeof(Contact)], null);

			action.AddMenuAction(ChangeID);
		}

		#endregion

		#region Buttons

		public PXAction<VendorR> viewRestrictionGroups;
		[PXUIField(DisplayName = GL.Messages.ViewRestrictionGroups, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewRestrictionGroups(PXAdapter adapter)
		{
			if (CurrentVendor.Current != null)
			{
				APAccessDetail graph = CreateInstance<APAccessDetail>();
				graph.Vendor.Current = graph.Vendor.Search<Vendor.acctCD>(CurrentVendor.Current.AcctCD);
				throw new PXRedirectRequiredException(graph, false, "Restricted Groups");
			}
			return adapter.Get();
		}

		public PXAction<VendorR> viewCustomer;
		[PXUIField(DisplayName = Messages.ViewCustomer, Enabled = false, Visible = true, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ViewCustomer(PXAdapter adapter)
		{
			BAccount bacct = this.BAccount.Current;
			if (bacct != null && (bacct.Type == BAccountType.CustomerType || bacct.Type == BAccountType.CombinedType))
			{
				Save.Press();
				AR.CustomerMaint editingBO = PXGraph.CreateInstance<PX.Objects.AR.CustomerMaint>();
				editingBO.BAccount.Current = editingBO.BAccount.Search<AR.Customer.acctCD>(bacct.AcctCD);
				throw new PXRedirectRequiredException(editingBO, "Edit Customer");
			}
			return adapter.Get();
		}

		public PXAction<VendorR> viewBusnessAccount;
		[PXUIField(DisplayName = Messages.ViewBusnessAccount, Enabled = false, Visible = true, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ViewBusnessAccount(PXAdapter adapter)
		{
			BAccount bacct = this.BAccount.Current;
			if (bacct != null)
			{
				Save.Press();
				CR.BusinessAccountMaint editingBO = PXGraph.CreateInstance<PX.Objects.CR.BusinessAccountMaint>();
				editingBO.BAccount.Current = editingBO.BAccount.Search<BAccount.acctCD>(bacct.AcctCD);
				throw new PXRedirectRequiredException(editingBO, "Edit Business Account");
			}
			return adapter.Get();
		}
		
		public PXAction<VendorR> extendToCustomer;
		[PXUIField(DisplayName = Messages.ExtendToCustomer, Visible = true, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ExtendToCustomer(PXAdapter adapter)
		{
			BAccount bacct = this.BAccount.Current;
			if (bacct != null && (bacct.Type == BAccountType.VendorType || bacct.Type == BAccountType.CombinedType))
			{
			    Save.Press();
			    AR.CustomerMaint editingBO = PXGraph.CreateInstance<PX.Objects.AR.CustomerMaint>();
			    AR.Customer customer = (AR.Customer)editingBO.BAccount.Cache.Extend<BAccount>(bacct);
			    editingBO.BAccount.Current = customer;
			    customer.Type = BAccountType.CombinedType;
			    LocationExtAddress defLocation = editingBO.DefLocation.Select();
			    editingBO.DefLocation.Cache.RaiseRowSelected(defLocation);
			    string locationType = LocTypeList.CombinedLoc;
			    editingBO.InitCustomerLocation(defLocation, locationType);
			    defLocation = editingBO.DefLocation.Update(defLocation);
			    foreach (Location iLoc in editingBO.IntLocations.Select())
			    {
			        if (iLoc.LocationID != defLocation.LocationID)
			        {
			            editingBO.InitCustomerLocation(iLoc, locationType);
			            editingBO.IntLocations.Update(iLoc);
			        }
			    }
			    throw new PXRedirectRequiredException(editingBO, "Edit Customer");
			}
			return adapter.Get();
		}


		public PXAction<VendorR> viewBalanceDetails;
		[PXUIField(DisplayName = "View Balance Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewBalanceDetails(PXAdapter adapter)
		{
			Vendor vendor = this.BAccount.Current;
			if (vendor != null && vendor.BAccountID > 0L)
			{
				APVendorBalanceEnq graph = PXGraph.CreateInstance<APVendorBalanceEnq>();
				graph.Clear();
				graph.Filter.Current.VendorID = vendor.BAccountID;
				throw new PXRedirectRequiredException(graph, "ViewBalanceDetails");
			}
			return adapter.Get();
		}


		public PXAction<VendorR> viewRemitOnMap;

		[PXUIField(DisplayName = CR.Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		public virtual IEnumerable ViewRemitOnMap(PXAdapter adapter)
		{

			BAccountUtility.ViewOnMap(this.RemitAddress.Current);
			return adapter.Get();
		}



		public PXAction<VendorR> newBillAdjustment;
		[PXUIField(DisplayName = AP.Messages.APInvoiceEntry, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable NewBillAdjustment(PXAdapter adapter)
		{
			Vendor vendor = this.BAccountAccessor.Current;
			if (vendor != null && vendor.BAccountID > 0L)
			{
				APInvoiceEntry invEntry = PXGraph.CreateInstance<APInvoiceEntry>();
				invEntry.Clear();

				APInvoice newDoc = invEntry.Document.Insert(new APInvoice());
				newDoc.VendorID = vendor.BAccountID;
				invEntry.Document.Cache.RaiseFieldUpdated<APInvoice.vendorID>(newDoc, null);
				throw new PXRedirectRequiredException(invEntry, AP.Messages.APInvoiceEntry);
			}
			return adapter.Get();
		}

		public PXAction<VendorR> newManualCheck;
		[PXUIField(DisplayName = AP.Messages.APPaymentEntry, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable NewManualCheck(PXAdapter adapter)
		{
			Vendor vendor = this.BAccountAccessor.Current;
			if (vendor != null && vendor.BAccountID > 0L)
			{
				APPaymentEntry payEntry = PXGraph.CreateInstance<APPaymentEntry>();
				payEntry.Clear();
				APPayment newDoc = payEntry.Document.Insert(new APPayment());
				newDoc.VendorID = vendor.BAccountID;
				payEntry.Document.Cache.RaiseFieldUpdated<APPayment.vendorID>(newDoc, null);
				throw new PXRedirectRequiredException(payEntry, AP.Messages.APPaymentEntry);
			}
			return adapter.Get();
		}

		public PXAction<VendorR> vendorDetails;
		[PXUIField(DisplayName = AP.Messages.APDocumentEnq, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable VendorDetails(PXAdapter adapter)
		{
			Vendor vendor = this.BAccountAccessor.Current;
			if (vendor != null && vendor.BAccountID > 0L)
			{
				APDocumentEnq graph = PXGraph.CreateInstance<APDocumentEnq>();
				graph.Clear();
				graph.Filter.Current.VendorID = vendor.BAccountID;
				throw new PXRedirectRequiredException(graph, AP.Messages.APDocumentEnq);
			}
			return adapter.Get();
		}

		public PXAction<VendorR> approveBillsForPayments;
		[PXUIField(DisplayName = AP.Messages.APApproveBills, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ApproveBillsForPayments(PXAdapter adapter)
		{
			Vendor vendor = this.BAccountAccessor.Current;
			if (vendor != null && vendor.BAccountID > 0L)
			{
				APApproveBills graph = PXGraph.CreateInstance<APApproveBills>();
				graph.Clear();
				graph.Filter.Current.VendorID = vendor.BAccountID;
				throw new PXRedirectRequiredException(graph, AP.Messages.APApproveBills);
			}
			return adapter.Get();
		}

		public PXAction<VendorR> payBills;
		[PXUIField(DisplayName = AP.Messages.APPayBills, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable PayBills(PXAdapter adapter)
		{
			Vendor vendor = this.BAccountAccessor.Current;
			if (vendor != null && vendor.BAccountID > 0L)
			{
				APPayBills graph = PXGraph.CreateInstance<APPayBills>();
				graph.Clear();
				graph.Filter.Current.VendorID = vendor.BAccountID;
				throw new PXRedirectRequiredException(graph, AP.Messages.APPayBills);
			}
			return adapter.Get();
		}

        //+ MMK 2011/10/03
        public PXAction<VendorR> balanceByVendor;
        [PXUIField(DisplayName = AP.Messages.BalanceByVendor, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
        public virtual IEnumerable BalanceByVendor(PXAdapter adapter)
        {
            Vendor vendor = this.BAccountAccessor.Current;
            if (vendor != null && vendor.BAccountID > 0L)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["VendorID"] = vendor.AcctCD;
                throw new PXReportRequiredException(parameters, "AP632500", AP.Messages.BalanceByVendor); //?????

            }
            return adapter.Get();
        }

        public PXAction<VendorR> vendorHistory;
        [PXUIField(DisplayName = AP.Messages.VendorHistory, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
        public virtual IEnumerable VendorHistory(PXAdapter adapter)
        {
            Vendor vendor = this.BAccountAccessor.Current;
            if (vendor != null && vendor.BAccountID > 0L)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["VendorID"] = vendor.AcctCD;
                throw new PXReportRequiredException(parameters, "AP652000", AP.Messages.VendorHistory); //?????
            }
            return adapter.Get();
        }

        //AP Aged Past Due
        public PXAction<VendorR> aPAgedPastDue;
        [PXUIField(DisplayName = AP.Messages.APAgedPastDue, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
        public virtual IEnumerable APAgedPastDue(PXAdapter adapter)
        {
            Vendor vendor = this.BAccountAccessor.Current;
            if (vendor != null && vendor.BAccountID > 0L)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["VendorID"] = vendor.AcctCD;
                throw new PXReportRequiredException(parameters, "AP631000", AP.Messages.APAgedPastDue); //?????
            }
            return adapter.Get();
        }

        public PXAction<VendorR> aPAgedOutstanding;
        [PXUIField(DisplayName = AP.Messages.APAgedOutstanding, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
        public virtual IEnumerable APAgedOutstanding(PXAdapter adapter)
        {
            Vendor vendor = this.BAccountAccessor.Current;
            if (vendor != null && vendor.BAccountID > 0L)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["VendorID"] = vendor.AcctCD;
                throw new PXReportRequiredException(parameters, "AP631500", AP.Messages.APAgedOutstanding); //?????
            }
            return adapter.Get();
        }

        public PXAction<VendorR> aPDocumentRegister;
        [PXUIField(DisplayName = AP.Messages.APDocumentRegister, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
        public virtual IEnumerable APDocumentRegister(PXAdapter adapter)
        {
            Vendor vendor = this.BAccountAccessor.Current;
            if (vendor != null && vendor.BAccountID > 0L)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["VendorID"] = vendor.AcctCD;
                throw new PXReportRequiredException(parameters, "AP621500", AP.Messages.APDocumentRegister); //?????
            }
            return adapter.Get();
        }

        public PXAction<VendorR> repVendorDetails;
        [PXUIField(DisplayName = AP.Messages.RepVendorDetails, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
        public virtual IEnumerable RepVendorDetails(PXAdapter adapter)
        {
            Vendor vendor = this.BAccountAccessor.Current;
            if (vendor != null && vendor.BAccountID > 0L)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["VendorID"] = vendor.AcctCD;
                throw new PXReportRequiredException(parameters, "AP655500", AP.Messages.RepVendorDetails); //?????
            }
            return adapter.Get();
        }
        //- MMK 2011/10/03

		//public PXDBAction<Base> newLocation;
		[PXUIField(DisplayName = Messages.VendorNewLocation, MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public new IEnumerable NewLocation(PXAdapter adapter)
		{
			if (this.BAccountAccessor.Cache.GetStatus(this.BAccountAccessor.Current) != PXEntryStatus.Inserted)
			{
				VendorLocationMaint graph = PXGraph.CreateInstance<VendorLocationMaint>();
				SelectedVendorLocation loc = new SelectedVendorLocation();
				loc.BAccountID = this.BAccountAccessor.Current.BAccountID;
				graph.Location.Insert(loc);
				throw new PXRedirectRequiredException(graph, Messages.VendorLocationMaint);
			}
			return adapter.Get();
		}

		//public PXAction<Base> viewLocation;
		[PXUIField(DisplayName = Messages.VendorViewLocation,
			MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select,
			Visible = false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public new IEnumerable ViewLocation(PXAdapter adapter)
		{
			if (this.Locations.Current != null && this.BAccountAccessor.Cache.GetStatus(this.BAccountAccessor.Current) != PXEntryStatus.Inserted)
			{
				LocationExtAddress current = this.Locations.Current;
				VendorLocationMaint graph = PXGraph.CreateInstance<VendorLocationMaint>();
				graph.Location.Current = graph.Location.Search<Location.locationID>(current.LocationID, this.BAccountAccessor.Current.AcctCD);
				throw new PXRedirectRequiredException(graph, Messages.VendorLocationMaint);
			}
			return adapter.Get();
		}


		#region Buttons
		public new PXAction<Vendor> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public override IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			BAccount bacct = this.BAccount.Current;
			if (bacct != null)
			{
				bool needSave = false;
				Save.Press();
				Address address = this.DefAddress.Current;
				if (address != null && address.IsValidated == false)
				{
					if(PXAddressValidator.Validate<Address>(this, address, true))
						needSave = true;
				}

				Address remitAddress = this.RemitAddress.Current;
				if (remitAddress != null && remitAddress.IsValidated == false && (remitAddress.AddressID!= address.AddressID))
				{
					if(PXAddressValidator.Validate<Address>(this, remitAddress, true))
						needSave = true;
				}
				LocationExtAddress locAddress = this.DefLocation.Current;
				//Needs to compare defAddress - AddressID  would be null
				if (locAddress != null && locAddress.IsValidated == false && locAddress.DefAddressID != address.AddressID) 
				{
					if(PXAddressValidator.Validate<LocationExtAddress>(this, locAddress, true))
						needSave = true;
				}
				if (needSave == true)
					this.Save.Press();

			}
			return adapter.Get();
		}
		#endregion
        #region MyButtons (MMK)
        public PXAction<VendorR> action;
        [PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable Action(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<VendorR> inquiry;
        [PXUIField(DisplayName = "Inquiries", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable Inquiry(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<VendorR> report;
        [PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable Report(PXAdapter adapter)
        {
            return adapter.Get();
        }
        #endregion

		public PXChangeID<Vendor, Vendor.acctCD> ChangeID;
		#endregion

		#region Select Delegates

		protected virtual IEnumerable remitContact()
		{
			Contact cnt = null;
			Vendor vendor = this.BAccount.Current;
			LocationExtAddress defLocation = DefLocation.Current ?? DefLocation.Select();
			if (defLocation != null && defLocation.VRemitContactID != null)
			{
				cnt = FindContact(defLocation.VRemitContactID);
				if (cnt != null)
				{
					if (defLocation.VRemitContactID == vendor.DefContactID)
					{
						cnt = PXCache<Contact>.CreateCopy(cnt);
						PXUIFieldAttribute.SetEnabled(this.RemitContact.Cache, cnt, false);
					}
					else
					{
						PXUIFieldAttribute.SetEnabled(this.RemitContact.Cache, cnt, true);
					}
				}
			}
			return new Contact[] { cnt };
		}

		protected Address formRemitAddress;
		protected virtual IEnumerable remitAddress()
		{
			Address addr = null;
			Vendor vendor = this.BAccount.Current;
			LocationExtAddress defLocation = DefLocation.Current ?? DefLocation.Select();
			if (defLocation != null && defLocation.VRemitAddressID != null)
			{
				addr = FindAddress(defLocation.VRemitAddressID);
				if (addr != null)
				{
					if (defLocation.VRemitAddressID == vendor.DefAddressID)
					{
						addr = PXCache<Address>.CreateCopy(addr);
						addr.AddressID = null;
						PXUIFieldAttribute.SetEnabled(this.RemitAddress.Cache, addr, false);
						formRemitAddress = addr;
					}
					else
					{
						PXUIFieldAttribute.SetEnabled(this.RemitAddress.Cache, addr, true);
					}
				}
			}
			return new Address[] { addr };
		}


		protected virtual IEnumerable vendorBalance()
		{

			Vendor vendor = (Vendor)this.BAccountAccessor.Current;
			List<VendorBalanceSummary> list = new List<VendorBalanceSummary>(1);
			bool isInserted = (this.BAccountAccessor.Cache.GetStatus(vendor) == PXEntryStatus.Inserted);
			if (!isInserted)
			{
				PXSelectBase<APVendorBalanceEnq.APLatestHistory> sel = new PXSelectJoinGroupBy<APVendorBalanceEnq.APLatestHistory, LeftJoin<CuryAPHistory, On<APVendorBalanceEnq.APLatestHistory.accountID, Equal<CuryAPHistory.accountID>,
										And<APVendorBalanceEnq.APLatestHistory.vendorID, Equal<CuryAPHistory.vendorID>,
										And<APVendorBalanceEnq.APLatestHistory.subID, Equal<CuryAPHistory.subID>,
										And<APVendorBalanceEnq.APLatestHistory.curyID, Equal<CuryAPHistory.curyID>,
										And<APVendorBalanceEnq.APLatestHistory.lastActivityPeriod, Equal<CuryAPHistory.finPeriodID>>>>>>>,
										Where<APVendorBalanceEnq.APLatestHistory.vendorID, Equal<Current<Vendor.bAccountID>>>,
										Aggregate<
											Sum<CuryAPHistory.finBegBalance,
											Sum<CuryAPHistory.curyFinBegBalance,
											Sum<CuryAPHistory.finYtdBalance,
											Sum<CuryAPHistory.curyFinYtdBalance,
											Sum<CuryAPHistory.tranBegBalance,
											Sum<CuryAPHistory.curyTranBegBalance,
											Sum<CuryAPHistory.tranYtdBalance,
											Sum<CuryAPHistory.curyTranYtdBalance,

											Sum<CuryAPHistory.finPtdPayments,
											Sum<CuryAPHistory.finPtdPurchases,
											Sum<CuryAPHistory.finPtdDiscTaken,
											Sum<CuryAPHistory.finPtdWhTax,
											Sum<CuryAPHistory.finPtdCrAdjustments,
											Sum<CuryAPHistory.finPtdDrAdjustments,
											Sum<CuryAPHistory.finPtdRGOL,
											Sum<CuryAPHistory.finPtdDeposits,
											Sum<CuryAPHistory.finYtdDeposits,


											Sum<CuryAPHistory.tranPtdPayments,
											Sum<CuryAPHistory.tranPtdPurchases,
											Sum<CuryAPHistory.tranPtdDiscTaken,
											Sum<CuryAPHistory.tranPtdWhTax,
											Sum<CuryAPHistory.tranPtdCrAdjustments,
											Sum<CuryAPHistory.tranPtdDrAdjustments,
											Sum<CuryAPHistory.tranPtdRGOL,
											Sum<CuryAPHistory.tranPtdDeposits,
											Sum<CuryAPHistory.tranYtdDeposits,

											Sum<CuryAPHistory.curyFinPtdPayments,
											Sum<CuryAPHistory.curyFinPtdPurchases,
											Sum<CuryAPHistory.curyFinPtdDiscTaken,
											Sum<CuryAPHistory.curyFinPtdWhTax,
											Sum<CuryAPHistory.curyFinPtdCrAdjustments,
											Sum<CuryAPHistory.curyFinPtdDrAdjustments,
											Sum<CuryAPHistory.curyFinPtdDeposits,
											Sum<CuryAPHistory.curyFinYtdDeposits,

											Sum<CuryAPHistory.curyTranPtdPayments,
											Sum<CuryAPHistory.curyTranPtdPurchases,
											Sum<CuryAPHistory.curyTranPtdDiscTaken,
											Sum<CuryAPHistory.curyTranPtdWhTax,
											Sum<CuryAPHistory.curyTranPtdCrAdjustments,
											Sum<CuryAPHistory.curyTranPtdDrAdjustments,
											Sum<CuryAPHistory.curyTranPtdDeposits,
											Sum<CuryAPHistory.curyTranYtdDeposits
											>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>(this);
				VendorBalanceSummary res = new VendorBalanceSummary();
				foreach (PXResult<APVendorBalanceEnq.APLatestHistory, CuryAPHistory> it in sel.Select())
				{
					CuryAPHistory iHst = it;
					Aggregate(res, iHst);
				}
				list.Add(res);
			}
			//SWUIFieldAttribute.SetVisible<VendorBalanceSummary.depositsBalance>(this.VendorBalance.Cache, null, !isInserted);
			//SWUIFieldAttribute.SetVisible<VendorBalanceSummary.balance>(this.VendorBalance.Cache, null, !isInserted);
			return list;
		}

		protected virtual void Address_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			Address addr = e.Row as Address;
			if (addr != null &&
				addr.AddressID == null)
			{
				e.Cancel = true;
			}
		}

		protected override void Address_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
            base.Address_RowUpdated(cache, e);

			Address addr = e.Row as Address;
			if (formRemitAddress != null &&
				addr != null)
			{
				int? id = formRemitAddress.AddressID;
				PXCache<Address>.RestoreCopy(formRemitAddress, addr);
				formRemitAddress.AddressID = id;
			}
		}
		#endregion

		#region Overrides
		public override void Persist()
		{
			using (PXConnectionScope cs = new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					try
					{
						base.Persist(typeof(Vendor), PXDBOperation.Update);
					}
					catch
					{
						Caches[typeof(Vendor)].Persisted(true);
						throw;
					}
					base.Persist();
					ts.Complete();
				}
			}
		}
		#endregion

		#region Vendor events

		[PXDBInt()]
		[PXDBChildIdentity(typeof(LocationExtAddress.locationID))]
		[PXUIField(DisplayName = "Default Location", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(Search<LocationExtAddress.locationID,
			Where<LocationExtAddress.bAccountID,
			Equal<Current<VendorR.bAccountID>>>>),
			DescriptionField = typeof(LocationExtAddress.locationCD),
			DirtyRead = true)]
		protected virtual void VendorR_DefLocationID_CacheAttached(PXCache sender)
		{

		}

		protected virtual void Vendor_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			PXRowInserting inserting = delegate(PXCache sender, PXRowInsertingEventArgs args)
			{
				Branch branch = PXSelect<Branch, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>.Select(this);
				if (branch != null)
				{
					object cd = branch.BranchCD;
					this.Locations.Cache.RaiseFieldUpdating<LocationExtAddress.locationCD>(args.Row, ref cd);
					((LocationExtAddress)args.Row).LocationCD = (string)cd;
				}
			};

			if (VendorClass.Current != null && VendorClass.Current.DefaultLocationCDFromBranch == true)
			{
				this.RowInserting.AddHandler<LocationExtAddress>(inserting);
			}

			// Executing Base Business Account Event
			base.OnBAccountRowInserted(cache, e);

			if (VendorClass.Current != null && VendorClass.Current.DefaultLocationCDFromBranch == true)
			{
				this.RowInserting.RemoveHandler<LocationExtAddress>(inserting);
			}
		}

		protected virtual void Vendor_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			Vendor row = (Vendor)e.Row;
			viewRestrictionGroups.SetEnabled(row != null && row.AcctCD != null);

			if (row == null)
				return;

			PXEntryStatus status = cache.GetStatus(row);
			viewCustomer.SetEnabled((row.Type == BAccountType.CustomerType || row.Type == BAccountType.CombinedType) && status != PXEntryStatus.Inserted);
			viewBusnessAccount.SetEnabled(status != PXEntryStatus.Inserted);
			
			bool mCActivated = (CMSetup.Current.MCActivated == true);
			PXUIFieldAttribute.SetVisible<Vendor.curyID>(cache, null, mCActivated);
			PXUIFieldAttribute.SetVisible<Vendor.curyRateTypeID>(cache, null, mCActivated);
			PXUIFieldAttribute.SetVisible<Vendor.allowOverrideCury>(cache, null, mCActivated);
			PXUIFieldAttribute.SetVisible<Vendor.allowOverrideRate>(cache, null, mCActivated);

			bool isNotInserted = !(cache.GetStatus(row) == PXEntryStatus.Inserted);
			PXUIFieldAttribute.SetVisible<VendorBalanceSummary.depositsBalance>(this.VendorBalance.Cache, null, isNotInserted);
			PXUIFieldAttribute.SetVisible<VendorBalanceSummary.balance>(this.VendorBalance.Cache, null, isNotInserted);
			PXUIFieldAttribute.SetEnabled<Vendor.taxReportFinPeriod>(cache, null, row.TaxPeriodType != PX.Objects.TX.VendorTaxPeriodType.FiscalPeriod);
			PXUIFieldAttribute.SetEnabled<Vendor.taxReportPrecision>(cache, null, row.TaxUseVendorCurPrecision != true);

			Delete.SetEnabled(CanDelete(row));
		}

		protected virtual void Vendor_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			Vendor row = e.Row as Vendor;
			if (row != null)
			{
				PX.Objects.TX.Tax tax = PXSelect<PX.Objects.TX.Tax, Where<PX.Objects.TX.Tax.taxVendorID, Equal<Current<Vendor.bAccountID>>>>.Select(this);
				if (tax != null)
				{
					e.Cancel = true;
					throw new PXException(Messages.TaxVendorDeleteErr);
				}
				if (row.Type == BAccountType.CombinedType)
				{
					// We shouldn't delete BAccount entity when it is in use by Customer entity
					PXTableAttribute tableAttr = cache.Interceptor as PXTableAttribute;
					tableAttr.BypassOnDelete(typeof(BAccount));
				}
			}
		}

		protected virtual void Vendor_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			Vendor row = e.Row as Vendor;
			if (row != null && row.Type == BAccountType.CombinedType)
				ChangeBAccountType(row, BAccountType.CustomerType);
		}

		protected virtual void Vendor_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
		}

		protected virtual void Vendor_AcctName_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			base.OnBAccountAcctNameFieldUpdated(cache, e);
		}

		protected virtual void Vendor_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (CMSetup.Current.MCActivated != true)
			{
				if (cmpany.Current == null || string.IsNullOrEmpty(cmpany.Current.BaseCuryID))
				{
					throw new PXException();
				}
				e.NewValue = cmpany.Current.BaseCuryID;
				e.Cancel = true;
			}
		}
		protected virtual void Vendor_TaxPeriodType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Vendor row = e.Row as Vendor;
			if (row != null && row.TaxPeriodType == PX.Objects.TX.VendorTaxPeriodType.FiscalPeriod)
				row.TaxReportFinPeriod = true;
		}

		protected virtual void Vendor_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (CMSetup.Current.MCActivated != true)
			{
				e.NewValue = string.Empty;
				e.Cancel = true;
			}
		}

		protected virtual void Vendor_CuryRateTypeID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (CMSetup.Current.MCActivated != true)
			{
				e.Cancel = true;
			}
		}

		public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			if (viewName == "PaymentDetails")
			{
				if (DefLocation.Current == null)
				{
					DefLocation.Current = DefLocation.Select();
				}
			}

			return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
		}

		protected virtual void Vendor_VendorClassID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Vendor row = (Vendor)e.Row;
			VendorClass vc = (VendorClass)PXSelectorAttribute.Select<Vendor.vendorClassID>(cache, row, e.NewValue);
			this.doCopyClassSettings = false;
			if (vc != null)
			{
				doCopyClassSettings = true;
				if (cache.GetStatus(row) != PXEntryStatus.Inserted)
				{
					if (BAccount.Ask(Messages.Warning, Messages.VendorClassChangeWarning, MessageButtons.YesNo, false) == WebDialogResult.No)
					{
						doCopyClassSettings = false;
						BAccount.ClearDialog();
					}
				}
			}
		}

		protected virtual void Vendor_VendorClassID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			if (this.DefLocation.Current == null)
			{
				this.DefLocation.Current = this.DefLocation.Select();
			}

			VendorClass.RaiseFieldUpdated(cache, e.Row);

			if (VendorClass.Current != null && VendorClass.Current.DefaultLocationCDFromBranch == true)
			{
				Branch branch = PXSelect<Branch, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>.Select(this);
				if (branch != null && DefLocation.Current != null && DefLocation.Cache.GetStatus(DefLocation.Current) == PXEntryStatus.Inserted)
				{
					object cd = branch.BranchCD;
					this.Locations.Cache.RaiseFieldUpdating<LocationExtAddress.locationCD>(DefLocation.Current, ref cd);
					DefLocation.Current.LocationCD = (string)cd;
					DefLocation.Cache.Normalize();
				}
			}

			DefAddress.Current = DefAddress.Select();
			if (DefAddress.Current != null && DefAddress.Current.AddressID != null)
            {
                InitDefAddress(DefAddress.Current);
                if (DefAddress.Cache.GetStatus(DefAddress.Current) == PXEntryStatus.Notchanged)
                    DefAddress.Cache.SetStatus(DefAddress.Current, PXEntryStatus.Updated);
            }

			Vendor row = (Vendor)e.Row;
			LocationExtAddress loc = DefLocation.Current;
			if (VendorClass.Current != null && doCopyClassSettings)
			{
				VendorClass.RaiseFieldUpdated(cache, e.Row);

				CopyAccounts(cache, row);
				InitVendorLocation(loc, loc.LocType);
			}
		}
				
		#endregion

		#region LocationExtAddress Events

		[PXDBString(BqlField = typeof(CR.Standalone.Location.vTaxZoneID))]
		[PXUIField(DisplayName = "Tax Zone", Required = false)]
		[PXDefault(typeof(Search<VendorClass.taxZoneID, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<TX.TaxZone.taxZoneID>), DescriptionField = typeof(TX.TaxZone.descr), CacheGlobal = true)]
		public virtual void LocationExtAddress_VTaxZoneID_CacheAttached(PXCache sender)
		{
		}

		protected override void LocationExtAddress_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			LocationExtAddress record = (LocationExtAddress)e.Row;

			record.IsRemitAddressSameAsMain = (record.VRemitAddressID == record.VDefAddressID);
			record.IsRemitContactSameAsMain = (record.VRemitContactID == record.VDefContactID);

			FillPaymentDetails(record);

			base.LocationExtAddress_RowInserted(sender, e);
		}
	

		protected override void LocationExtAddress_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			
			if (e.Row != null)
			{
				LocationExtAddress row = (LocationExtAddress)e.Row;

				row.IsRemitAddressSameAsMain = (row.VRemitAddressID == row.VDefAddressID);
				row.IsRemitContactSameAsMain = (row.VRemitContactID == row.VDefContactID);	
				FillPaymentDetails(row);
				PXUIFieldAttribute.SetEnabled<LocationExtAddress.vCashAccountID>(sender, e.Row, String.IsNullOrEmpty(row.VPaymentMethodID)==false);

                if (this.VendorClass.Current != null)
                {
                    bool isRequired = (this.VendorClass.Current.RequireTaxZone ?? false) && row.IsDefault == true;
                    PXDefaultAttribute.SetPersistingCheck<LocationExtAddress.vTaxZoneID>(this.DefLocation.Cache, null, PXPersistingCheck.Nothing);
                    PXDefaultAttribute.SetPersistingCheck<LocationExtAddress.vTaxZoneID>(this.DefLocation.Cache, e.Row, isRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
                    PXUIFieldAttribute.SetRequired<LocationExtAddress.vTaxZoneID>(this.DefLocation.Cache, isRequired);
                }
			}

			base.LocationExtAddress_RowSelected(sender, e);
		}

		protected virtual void LocationExtAddress_VDefAddressID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = BAccount.Cache.GetValue<Vendor.defAddressID>(BAccount.Current);
			e.Cancel = (BAccount.Current != null);
		}

		protected virtual void LocationExtAddress_VDefContactID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = BAccount.Cache.GetValue<Vendor.defContactID>(BAccount.Current);
			e.Cancel = (BAccount.Current != null);
		}

		protected virtual void LocationExtAddress_VRemitAddressID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = BAccount.Cache.GetValue<Vendor.defAddressID>(BAccount.Current);
			e.Cancel = (BAccount.Current != null);
		}

		protected virtual void LocationExtAddress_VRemitContactID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = BAccount.Cache.GetValue<Vendor.defContactID>(BAccount.Current);
			e.Cancel = (BAccount.Current != null); 
		}

		protected virtual void LocationExtAddress_IsRemitContactSameAsMain_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			LocationExtAddress owner = (LocationExtAddress)e.Row;
			if (owner != null)
			{
				if (owner.IsRemitContactSameAsMain == true)
				{
					if (owner.VRemitContactID != owner.VDefContactID)
					{
						Contact contact = this.FindContact(owner.VRemitContactID);
						if (contact != null && contact.ContactID == owner.VRemitContactID)
						{
							this.RemitContact.Delete(contact);
						}
						owner.VRemitContactID = owner.VDefContactID;
						//if (cache.Locate(owner) != null)
						//  cache.Update(owner);
					}
				}

				if (owner.IsRemitContactSameAsMain == false)
				{
					if (owner.VRemitContactID != null)
					{
						if (owner.VRemitContactID == owner.VDefContactID)
						{
							Contact defContact = this.FindContact(owner.VDefContactID);
							Contact cont = PXCache<Contact>.CreateCopy(defContact);
							cont.ContactID = null;
							cont.BAccountID = owner.BAccountID;
							cont.ContactType = ContactTypesAttribute.BAccountProperty;
							cont = (Contact)this.RemitContact.Cache.Insert(cont);
							owner.VRemitContactID = cont.ContactID;
							//if (cache.Locate(owner) != null)
							//  cache.Update(owner);
						}
					}
				}
			}
		}

		protected virtual void LocationExtAddress_IsRemitAddressSameAsMain_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			LocationExtAddress owner = (LocationExtAddress)e.Row;
			if (owner != null)
			{
				if (owner.IsRemitAddressSameAsMain == true)
				{
					if (owner.VRemitAddressID != owner.VDefAddressID)
					{
						Address extAddr = this.FindAddress(owner.VRemitAddressID);
						if (extAddr != null && extAddr.AddressID == owner.VRemitAddressID)
						{
							this.RemitAddress.Delete(extAddr);
						}
						owner.VRemitAddressID = owner.VDefAddressID;
						//if (cache.Locate(owner) != null)
						//  cache.Update(owner);
					}
				}
				else if (owner.VRemitAddressID != null)
				{
					if (owner.VRemitAddressID == owner.VDefAddressID)
					{
						Address defAddress = this.FindAddress(owner.VDefAddressID);
						Address addr = PXCache<Address>.CreateCopy(defAddress);
						addr.AddressID = null;
						addr.BAccountID = owner.BAccountID;
						addr = this.RemitAddress.Insert(addr);
						owner.VRemitAddressID = addr.AddressID;
						//if (cache.Locate(owner) != null)
						//  cache.Update(owner);
						formRemitAddress = addr;
					}
				}
			}
		}

		protected virtual void LocationExtAddress_CBranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}
		protected virtual void LocationExtAddress_VPaymentMethodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			LocationExtAddress row = (LocationExtAddress)e.Row;
			string oldValue = (string)e.OldValue;
			if (!String.IsNullOrEmpty(oldValue))
			{
				this.ClearPaymentDetails(row, oldValue, true);
			}
			cache.SetDefaultExt<LocationExtAddress.vCashAccountID>(e.Row);
			this.FillPaymentDetails(row);
			this.PaymentDetails.View.RequestRefresh();
		}
		protected virtual void LocationExtAddress_VPaymentMethodID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress row = (LocationExtAddress)e.Row;
			if (row == null)
			{
				e.Cancel = true;
			}
			else
			{
				if (this.VendorClass.Current != null && String.IsNullOrEmpty(this.VendorClass.Current.PaymentMethodID) == false)
				{
					e.NewValue = VendorClass.Current.PaymentMethodID;
					e.Cancel = true;
				}
			}
		}
		protected virtual void LocationExtAddress_VCashAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress row = (LocationExtAddress)e.Row;
			if (row != null)
			{
				if (this.VendorClass.Current != null && this.VendorClass.Current.CashAcctID.HasValue 
					&& row.VPaymentMethodID == this.VendorClass.Current.PaymentMethodID)
				{
					e.NewValue = this.VendorClass.Current.CashAcctID;
					e.Cancel = true;
				}
				else
				{
					e.NewValue = null;
					e.Cancel = true;
				}
			}
		}

		protected virtual void LocationExtAddress_VPaymentByType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.paymentByType>(VendorClass.Current);
				e.Cancel = true;
			}
		}
		protected virtual void LocationExtAddress_VTaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.taxZoneID>(VendorClass.Current);
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_VAPAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.aPAcctID>(VendorClass.Current);
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_VAPSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.aPSubID>(VendorClass.Current);
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_VExpenseAcctID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress loc = e.Row as LocationExtAddress;
			if (loc == null) return;
			if (this.BAccountAccessor.Current != null)
			{
				BAccount acct = this.BAccountAccessor.Current;
				if (acct != null && (acct.Type == BAccountType.VendorType || acct.Type == BAccountType.CombinedType))
				{
					if (loc.IsDefault == true)
					{
						VendorClass vClass = VendorClass.Current;
						if (vClass != null && vClass.ExpenseAcctID != null)
						{
							e.NewValue = vClass.ExpenseAcctID;
							e.Cancel = true;
						}
					}
				}
			}
		}
		protected virtual void LocationExtAddress_VExpenseSubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress loc = e.Row as LocationExtAddress;
			if (loc == null) return;
			if (this.BAccountAccessor.Current != null)
			{
				BAccount acct = this.BAccountAccessor.Current;
				if (acct != null && (acct.Type == BAccountType.VendorType || acct.Type == BAccountType.CombinedType))
				{
					if (loc.IsDefault == true)
					{
						VendorClass vClass = VendorClass.Current;
						if (vClass != null && vClass.ExpenseSubID != null)
						{
							e.NewValue = vClass.ExpenseSubID;
							e.Cancel = true;
						}
					}
				}
			}
		}

        protected virtual void LocationExtAddress_VDiscountAcctID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (VendorClass.Current != null)
            {
                e.NewValue = VendorClass.Cache.GetValue<VendorClass.discountAcctID>(VendorClass.Current);
                e.Cancel = true;
            }
        }

        protected virtual void LocationExtAddress_VDiscountSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (VendorClass.Current != null)
            {
                e.NewValue = VendorClass.Cache.GetValue<VendorClass.discountSubID>(VendorClass.Current);
                e.Cancel = true;
            }
        }

		protected virtual void LocationExtAddress_VFreightAcctID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.freightAcctID>(VendorClass.Current);
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_VFreightSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.freightSubID>(VendorClass.Current);
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_VShipTermsID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.shipTermsID>(VendorClass.Current);
				e.Cancel = true;
			}
		}	

		protected virtual void LocationExtAddress_VRcptQtyAction_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.rcptQtyAction>(VendorClass.Current) ?? POReceiptQtyAction.AcceptButWarn;
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_VPrintOrder_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.printPO>(VendorClass.Current) ?? false;
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_VEmailOrder_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.emailPO>(VendorClass.Current) ?? false;
				e.Cancel = true;
			}
		}

		protected virtual void Location_VPaymentTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (VendorClass.Current != null)
			{
				e.NewValue = VendorClass.Cache.GetValue<VendorClass.paymentMethodID>(VendorClass.Current);
				e.Cancel = true;
			}
		}

		protected override void LocationExtAddress_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			base.LocationExtAddress_RowPersisting(cache, e);
			if (e.Cancel) return;
			LocationExtAddress loc = (LocationExtAddress)e.Row;
			if (loc == null) return;

			VendorR acct = this.BAccountAccessor.Current;
			if (acct != null && (acct.Type == BAccountType.VendorType || acct.Type == BAccountType.CombinedType))
			{
				if (loc.LocationID == acct.DefLocationID && loc.IsActive != true)
				{
					cache.RaiseExceptionHandling<Location.isActive>(loc, null, new PXSetPropertyException(CR.Messages.DefaultLocationCanNotBeNotActive, typeof(Location.isActive).Name));
				}
				if (acct.TaxAgency == true && loc.VExpenseAcctID == null)
				{
					cache.RaiseExceptionHandling<Location.vExpenseAcctID>(loc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Location.vExpenseAcctID).Name));
				}
				if (acct.TaxAgency == true && loc.VExpenseSubID == null)
				{
					cache.RaiseExceptionHandling<Location.vExpenseSubID>(loc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Location.vExpenseSubID).Name));
				}
				if (VendorClass.Current != null && VendorClass.Current.RequireTaxZone == true && loc.IsDefault == true && loc.VTaxZoneID == null)
				{
					cache.RaiseExceptionHandling<Location.vTaxZoneID>(loc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Location.vTaxZoneID).Name));
				}

				PXSelectBase<CashAccount> select = new PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<Location.vCashAccountID>>>>(this);
				if (!String.IsNullOrEmpty(acct.CuryID) && acct.AllowOverrideCury != true)
				{
					CashAccount cashacct = select.Select(loc.VCashAccountID);
					if (cashacct != null)
					{
						if (acct.CuryID != cashacct.CuryID)
						{
                            cache.RaiseExceptionHandling<Location.vCashAccountID>(loc, cashacct.CashAccountCD, new PXSetPropertyException(Messages.VendorCuryDifferentDefPayCury, typeof(Location.vCashAccountID).Name));
						}
					}
				}


				foreach (VendorPaymentMethodDetail it in this.PaymentDetails.Select())
				{
					if (!ValidateDetail(it))
					{
						e.Cancel = true;
					}
				}	
			}
		}

		protected override void LocationExtAddress_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
		{
			base.LocationExtAddress_RowPersisted(cache, e);
			LocationExtAddress row = e.Row as LocationExtAddress;
			if(row != null && e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Completed)
			{
				row.VDefAddressID = row.DefAddressID;
				row.VDefContactID = row.DefContactID;
			}
		}

		#endregion

		#region NotificationRecipient Events
		protected virtual void NotificationRecipient_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			NotificationRecipient row = (NotificationRecipient)e.Row;
			if (row == null) return;
			Contact contact = PXSelectorAttribute.Select<NotificationRecipient.contactID>(cache, row) as Contact;
			if (contact == null)
			{
				switch (row.ContactType)
				{
					case VendorContactType.Primary:
						contact = DefContact.SelectWindowed(0, 1);
						break;
					case VendorContactType.Remittance:
						contact = RemitContact.SelectWindowed(0, 1);
						break;
					case VendorContactType.Shipping:
						contact = DefLocationContact.SelectWindowed(0, 1);
						break;
				}
			}
			if (contact != null)
				row.Email = contact.EMail;
		}
		#endregion

		#region Other Events

		protected virtual void VendorPaymentMethodDetail_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if(e.Row!= null)
			{
				VendorPaymentMethodDetail row = (VendorPaymentMethodDetail)e.Row;
				PaymentMethodDetail iTempl = this.FindTemplate(row);
				bool isRequired = (iTempl != null) && (iTempl.IsRequired ?? false);
				PXDefaultAttribute.SetPersistingCheck<VendorPaymentMethodDetail.detailValue>(cache, row, (isRequired) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			}
		}
		#endregion

		#region Auxillary Functions

		protected virtual void FillPaymentDetails(LocationExtAddress account)
		{
			if (account != null)
			{
				if (!string.IsNullOrEmpty(account.VPaymentMethodID))
				{
					PaymentMethod paymentTypeDef = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, account.VPaymentMethodID);
					if (paymentTypeDef != null)
                    {
                        List<PaymentMethodDetail> toAdd = new List<PaymentMethodDetail>();
                        foreach (PaymentMethodDetail it in this.PaymentTypeDetails.Select(account.VPaymentMethodID))
                        {
                            VendorPaymentMethodDetail detail = null;
                            foreach (VendorPaymentMethodDetail iPDet in this.PaymentDetails.Select(account.BAccountID, account.LocationID, account.VPaymentMethodID))
                            {
                                if (iPDet.DetailID == it.DetailID)
                                {
                                    detail = iPDet;
                                    break;
                                }
                            }
                            if (detail == null)
                            {
                                toAdd.Add(it);
                            }
                        }
                        using (ReadOnlyScope rs = new ReadOnlyScope(this.PaymentDetails.Cache))
                        {
                            foreach (PaymentMethodDetail it in toAdd)
                            {
                                VendorPaymentMethodDetail detail = new VendorPaymentMethodDetail();
                                detail.BAccountID = account.BAccountID;
                                detail.LocationID = account.LocationID;
                                detail.PaymentMethodID = account.VPaymentMethodID;
                                detail.DetailID = it.DetailID;
                                detail = this.PaymentDetails.Insert(detail);
                            }
                            if (toAdd.Count > 0)
                            {
                                this.PaymentDetails.View.RequestRefresh();
                            }
                        }
                    }
				}
			}

		}

		protected virtual void ClearPaymentDetails(LocationExtAddress account, string paymentTypeID, bool clearNewOnly)
		{
			foreach (VendorPaymentMethodDetail it in this.PaymentDetails.Select(account.BAccountID, account.LocationID, paymentTypeID))
			{
				bool doDelete = true;
				if (clearNewOnly)
				{
					PXEntryStatus status = this.PaymentDetails.Cache.GetStatus(it);
					doDelete = (status == PXEntryStatus.Inserted);
				}
				if (doDelete)
					this.PaymentDetails.Delete(it);
			}
		}

		protected override void SetAsDefault(ContactExtAddress row)
		{
			LocationExtAddress account = this.DefLocation.Current ?? DefLocation.Select();
			if (account != null)
			{
				if (account.VRemitContactID != null && (account.VRemitContactID == account.VDefContactID))
				{
					int? defContactID = (row != null) ? row.ContactID : null;
					account.VRemitContactID = defContactID;

					if (this.DefLocation.Cache.GetStatus(account) == PXEntryStatus.Notchanged)
					{
						this.DefLocation.Cache.SetStatus(account, PXEntryStatus.Updated);
					}
				}
			}
			base.SetAsDefault(row);
		}

		protected override void InitDefAddress(Address aAddress)
		{
			base.InitDefAddress(aAddress);
			VendorClass vendorClass = this.VendorClass.Current;
			if (vendorClass != null && vendorClass.CountryID != null)
			{
				aAddress.CountryID = vendorClass.CountryID;
			}
		}

		public virtual void CopyAccounts(PXCache sender, Vendor row)
		{
			sender.SetDefaultExt<Vendor.discTakenAcctID>(row);
			sender.SetDefaultExt<Vendor.discTakenSubID>(row);
			sender.SetDefaultExt<Vendor.prepaymentAcctID>(row);
			sender.SetDefaultExt<Vendor.prepaymentSubID>(row);
			sender.SetDefaultExt<Vendor.prebookAcctID>(row);
			sender.SetDefaultExt<Vendor.prebookSubID>(row);
			sender.SetDefaultExt<Vendor.pOAccrualAcctID>(row);
			sender.SetDefaultExt<Vendor.pOAccrualSubID>(row);
			sender.SetDefaultExt<Vendor.curyID>(row);
			sender.SetDefaultExt<Vendor.curyRateTypeID>(row);
			sender.SetDefaultExt<Vendor.priceListCuryID>(row);

			if (this.DefLocation.Current != null &&
				 this.DefLocation.Cache.GetStatus(this.DefLocation.Current) == PXEntryStatus.Notchanged)
			{
				this.DefLocation.Cache.SetStatus(this.DefLocation.Current, PXEntryStatus.Updated);
			}

			sender.SetDefaultExt<Vendor.allowOverrideCury>(row);
			sender.SetDefaultExt<Vendor.allowOverrideRate>(row);
			sender.SetDefaultExt<Vendor.termsID>(row);
			sender.SetDefaultExt<Vendor.taxZoneID>(row);


			if (VendorClass.Current == null)
			{
				VendorClass.Current = VendorClass.Select(row.VendorClassID);
			}

			if (VendorClass.Current != null)
			{
				row.GroupMask = VendorClass.Current.GroupMask;
			}
		}

		protected virtual void Aggregate(VendorBalanceSummary aRes, CuryAPHistory aSrc)
		{
			aRes.Init();
			aRes.VendorID = aSrc.VendorID;
			aRes.Balance += aSrc.FinYtdBalance ?? Decimal.Zero;
			aRes.DepositsBalance += aSrc.FinYtdDeposits ?? Decimal.Zero;
		}

		public virtual void InitVendorLocation(Location aLoc, string aLocationType)
		{
			this.IntLocations.Cache.SetDefaultExt<Location.vCarrierID>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.vFOBPointID>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.vLeadTime>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.vShipTermsID>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.vBranchID>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.vTaxZoneID>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.vExpenseAcctID>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.vExpenseSubID>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.vFreightAcctID>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.vFreightSubID>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.vRcptQtyAction>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.vRcptQtyMin>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.vRcptQtyMax>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.vAPAccountID>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.vAPSubID>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.vCashAccountID>(aLoc);
            this.IntLocations.Cache.SetDefaultExt<Location.vPaymentMethodID>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.vPaymentByType>(aLoc);
			
			

			aLoc.LocType = aLocationType;
		}

		public virtual void InitVendorLocation(LocationExtAddress aLoc, string aLocationType)
		{
			this.DefLocation.Cache.SetDefaultExt<Location.vCarrierID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vFOBPointID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vLeadTime>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vShipTermsID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vBranchID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vTaxZoneID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vExpenseAcctID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vExpenseSubID>(aLoc);
            this.DefLocation.Cache.SetDefaultExt<Location.vDiscountAcctID>(aLoc);
            this.DefLocation.Cache.SetDefaultExt<Location.vDiscountSubID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vFreightAcctID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vFreightSubID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vRcptQtyAction>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vRcptQtyMin>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vRcptQtyMax>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vAPAccountID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vAPSubID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vCashAccountID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vPaymentMethodID>(aLoc);			
			this.DefLocation.Cache.SetDefaultExt<Location.vPaymentByType>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vShipTermsID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vRcptQtyAction>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vPrintOrder>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vEmailOrder>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vRemitAddressID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.vRemitContactID>(aLoc);

			aLoc.LocType = aLocationType;
		}

		protected virtual PaymentMethodDetail FindTemplate(VendorPaymentMethodDetail aDet)
		{			
			 PaymentMethodDetail res = PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
			        	                And<PaymentMethodDetail.detailID, Equal<Required<PaymentMethodDetail.detailID>>,
                                        And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
                                            Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>>.Select(this, aDet.PaymentMethodID, aDet.DetailID);
			return res;
		}

		protected virtual bool ValidateDetail(VendorPaymentMethodDetail aRow)
		{
			PaymentMethodDetail iTempl = this.FindTemplate(aRow);
			if (iTempl != null && (iTempl.IsRequired ?? false) && String.IsNullOrEmpty(aRow.DetailValue))
			{
				this.PaymentDetails.Cache.RaiseExceptionHandling<VendorPaymentMethodDetail.detailValue>(aRow, aRow.DetailValue, new PXSetPropertyException(CA.Messages.ERR_RequiredValueNotEnterd));
				return false;
			}
			return true;
		}

		private bool CanDelete(Vendor row)
		{
			if (row != null)
			{
				PX.Objects.TX.Tax tax = PXSelect<PX.Objects.TX.Tax, Where<PX.Objects.TX.Tax.taxVendorID, Equal<Current<Vendor.bAccountID>>>>.Select(this);
				if (tax != null)
				{
					return false;
				}
			}

			return true;
		}
		#endregion

		#region Private members
		private bool doCopyClassSettings;
		#endregion

		#region Soap-related handlers
		protected bool addressAdded;
		protected override void Address_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			if (e.ExternalCall)
			{
				if (!addressAdded)
				{
					base.Address_RowInserted(cache, e);
					addressAdded = true;
				}
				else if (DefLocation.Cache.GetStatus(DefLocation.Current) == PXEntryStatus.Inserted)
				{
					DefLocation.Current.VRemitAddressID = DetailInserted<Address>(cache, (Address)e.Row, null);
					DefLocation.Current.IsRemitAddressSameAsMain = false;
				}
			}
		}
		protected bool contactAdded;
		protected override void Contact_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			if (e.ExternalCall)
			{
				if (!contactAdded)
				{
					base.Contact_RowInserted(cache, e);
					contactAdded = true;
				}
				else if (DefLocation.Cache.GetStatus(DefLocation.Current) == PXEntryStatus.Inserted)
				{
					DefLocation.Current.VRemitContactID = DetailInserted<Contact>(cache, (Contact)e.Row, null);
					DefLocation.Current.IsRemitContactSameAsMain = false;
				}
			}
		}
		#endregion
	}
}

