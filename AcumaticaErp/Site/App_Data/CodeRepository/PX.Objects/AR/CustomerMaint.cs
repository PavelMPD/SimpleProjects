using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.CA;
using PX.Objects.CR;
using PX.Objects.CM;
using PX.Objects.EP;
using PX.Objects.SO;
using PX.SM;
using PX.CCProcessing;

namespace PX.Objects.AR
{
	public class CustomerMaint : BusinessAccountGraphBase<Customer, Customer, Where<BAccount.type, Equal<BAccountType.customerType>,
				Or<BAccount.type, Equal<BAccountType.combinedType>>>>
	{
		#region InternalTypes

		[Serializable]
		[PXCacheName(Messages.CustomerBalanceSummary)]
		public partial class CustomerBalanceSummary : IBqlTable
		{
			#region CustomerID
			public abstract class customerID : PX.Data.IBqlField
			{
			}
			protected Int32? _CustomerID;
			[PXDBInt()]
			[PXDefault()]
			public virtual Int32? CustomerID
			{
				get
				{
					return this._CustomerID;
				}
				set
				{
					this._CustomerID = value;
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
            #region UnreleasedBalance
            public abstract class unreleasedBalance : PX.Data.IBqlField
            {
            }
            protected Decimal? _UnreleasedBalance;
            [PXBaseCury()]
            [PXUIField(DisplayName = "Unreleased Balance", Visible = true, Enabled = false)]
            public virtual Decimal? UnreleasedBalance
            {
                get
                {
                    return this._UnreleasedBalance;
                }
                set
                {
                    this._UnreleasedBalance = value;
                }
            }
            #endregion
            #region DepositsBalance
			public abstract class depositsBalance : PX.Data.IBqlField
			{
			}
			protected Decimal? _DepositsBalance;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Prepayments Balance",Visible=true,Enabled=false)]
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
			#region SignedDepositsBalance
			public abstract class signedDepositsBalance : PX.Data.IBqlField
			{
			}
			
			[PXBaseCury()]
			[PXUIField(DisplayName = "Prepayments Balance",Visible=true,Enabled=false)]
			public virtual Decimal? SignedDepositsBalance
			{
				get
				{
					return (this._DepositsBalance* Decimal.MinusOne);
				}
				set
				{
					
				}
			}
			#endregion
            #region OpenOrdersBalance
            public abstract class openOrdersBalance : PX.Data.IBqlField
            {
            }
            protected Decimal? _OpenOrdersBalance;
            [PXBaseCury()]
            [PXUIField(DisplayName = "Open Orders Balance", Visible = true, Enabled = false)]
            public virtual Decimal? OpenOrdersBalance
            {
                get
                {
                    return this._OpenOrdersBalance;
                }
                set
                {
                    this._OpenOrdersBalance = value;
                }
            }
            #endregion
            #region RemainingCreditLimit
            public abstract class remainingCreditLimit : PX.Data.IBqlField
            {
            }
            protected Decimal? _RemainingCreditLimit;
            [PXBaseCury()]
            [PXUIField(DisplayName = "Remaining Credit Limit", Visible = true, Enabled = false)]
            public virtual Decimal? RemainingCreditLimit
            {
                get
                {
                    return this._RemainingCreditLimit;
                }
                set
                {
                    this._RemainingCreditLimit = value;
                }
            }
            #endregion
            #region OldInvoiceDate
            public abstract class oldInvoiceDate : IBqlField {}
            [PXDate]
            [PXUIField(DisplayName = "First Invoice Date", Visible = true, Enabled = false)]
            public virtual DateTime? OldInvoiceDate { get; set; }
            #endregion

            public virtual void Init()
			{
				if (!this.Balance.HasValue) this.Balance = Decimal.Zero;
                if (!this.UnreleasedBalance.HasValue) this.Balance = Decimal.Zero;
                if (!this.RemainingCreditLimit.HasValue) this.Balance = Decimal.Zero;
                if (!this.DepositsBalance.HasValue) this.DepositsBalance = Decimal.Zero;
                if (!this.OpenOrdersBalance.HasValue) this.OpenOrdersBalance = Decimal.Zero;
            }

		}
		
		#endregion

		#region Cache Attached
		#region NotificationSource
		[PXDBGuid(IsKey = true)]
		[PXSelector(typeof(Search<NotificationSetup.setupID,
			Where<NotificationSetup.sourceCD, Equal<ARNotificationSource.customer>>>),
			 SubstituteKey = typeof(NotificationSetup.notificationCD))]
		[PXUIField(DisplayName = "Mailing ID")]		
		protected virtual void NotificationSource_SetupID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(10, IsUnicode = true)]
		protected virtual void NotificationSource_ClassID_CacheAttached(PXCache sender)
		{
		}
		[GL.Branch(null, IsDetail = false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCheckUnique(typeof(NotificationSource.setupID), IgnoreNulls = false,
			Where = typeof(Where<NotificationSource.refNoteID, Equal<Current<NotificationSource.refNoteID>>>))]
		protected virtual void NotificationSource_NBranchID_CacheAttached(PXCache sender)
		{

		}		
		[PXDBString(8, InputMask = "CC.CC.CC.CC")]
		[PXUIField(DisplayName = "Report")]
		[PXDefault(typeof(Search<NotificationSetup.reportID,
			Where<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<SiteMap.screenID,
			Where<SiteMap.url, Like<urlReports>,
				And<Where<SiteMap.screenID, Like<PXModule.ar_>,
							 Or<SiteMap.screenID, Like<PXModule.so_>,
							 Or<SiteMap.screenID, Like<PXModule.cr_>>>>>>,
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
		[CustomerContactType.List]
		[PXUIField(DisplayName = "Contact Type")]
		[PXCheckUnique(typeof(NotificationRecipient.contactID),
			Where = typeof(Where<NotificationRecipient.sourceID, Equal<Current<NotificationRecipient.sourceID>>,
			And<NotificationRecipient.refNoteID, Equal<Current<Customer.noteID>>>>))]
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

		#region CustSalesPeople
		[SalesPerson(IsKey = true, DescriptionField = typeof(SalesPerson.descr))]
		[PXParent(typeof(Select<SalesPerson, Where<SalesPerson.salesPersonID, Equal<Current<CustSalesPeople.salesPersonID>>>>))]
		public virtual void CustSalesPeople_SalesPersonID_CacheAttached(PXCache sender)
		{	
		}
		[PXDBInt]
		[PXDBLiteDefault(typeof(BAccount.bAccountID))]
		[PXParent(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<CustSalesPeople.bAccountID>>>>))]
		public virtual void CustSalesPeople_BAccountID_CacheAttached(PXCache sender)
		{	
		}
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Location ID", Visibility = PXUIVisibility.Visible)]
		[PXDimensionSelector(LocationIDAttribute.DimensionName, typeof(Search<LocationExtAddress.locationID, Where<LocationExtAddress.bAccountID,
			Equal<Current<CustSalesPeople.bAccountID>>>>), typeof(LocationExtAddress.locationCD),
			typeof(Location.locationCD), typeof(Location.descr),
			DirtyRead = true, DescriptionField = typeof(LocationExtAddress.descr))]
		[PXDefault(typeof(Search<Customer.defLocationID, Where<Customer.bAccountID, Equal<Current<CustSalesPeople.bAccountID>>>>))]
		public virtual void CustSalesPeople_LocationID_CacheAttached(PXCache sender)
		{	
		}		
		[PXDBDecimal(6)]
		[PXDefault(typeof(Search<SalesPerson.commnPct, Where<SalesPerson.salesPersonID, Equal<Current<CustSalesPeople.salesPersonID>>>>))]
		[PXUIField(DisplayName = "Commission %")]
		public virtual void CustSalesPeople_CommisionPct_CacheAttached(PXCache sender)
		{
		}				
		#endregion

		#region CustomerPaymentMethod
		[PXDBInt()]
		[PXDBLiteDefault(typeof(BAccount.bAccountID))]
		//[Customer(DescriptionField = typeof(Customer.acctName), Visible = false, DirtyRead = true)]
		[PXParent(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<CustomerPaymentMethod.bAccountID>>>>))]
		public virtual void CustomerPaymentMethod_BAccountID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(10, IsUnicode = true,IsKey=true)]
		[PXUIField(DisplayName = "Payment Method", Enabled = false)]
		[PXDefault(typeof(Customer.defPaymentMethodID))]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID, Where<PaymentMethod.isActive, Equal<boolTrue>,
                        And<PaymentMethod.useForAR,Equal<True>>>>), DescriptionField = typeof(PaymentMethod.descr))]
		public virtual void CustomerPaymentMethod_PaymentMethodID_CacheAttached(PXCache sender)
		{
		}
		[GL.CashAccount(null, typeof(Search2<CashAccount.cashAccountID, InnerJoin<PaymentMethodAccount,
			On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
            And<PaymentMethodAccount.useForAR,Equal<True>,
			And<PaymentMethodAccount.paymentMethodID, Equal<Current<CustomerPaymentMethod.paymentMethodID>>>>>>,
			Where<Match<Current<AccessInfo.userName>>>>), DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(typeof(Search<CA.PaymentMethod.defaultCashAccountID, Where<CA.PaymentMethod.paymentMethodID, Equal<Current<CustomerPaymentMethod.paymentMethodID>>>>))]		
		public virtual void CustomerPaymentMethod_CashAccountID_CacheAttached(PXCache sender)
		{
		}	
		#endregion

		#region CarrierPluginCustomer
		[Customer(DescriptionField = typeof(Customer.acctName), Filterable = true)]
		[PXUIField(DisplayName = "Customer ID")]
		[PXDBDefault(typeof(Customer.bAccountID))]
		[PXParent(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<CarrierPluginCustomer.customerID>>>>))]
		public virtual void CarrierPluginCustomer_CustomerID_CacheAttached(PXCache sender)
		{
		}	
		#endregion
		#endregion

		#region Selects Declarations

		public PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<Customer.bAccountID>>>> CurrentCustomer;

		public PXSelect<Address, Where<Address.bAccountID, Equal<Current<Customer.bAccountID>>,
			  And<Address.addressID, Equal<Current<Customer.defBillAddressID>>>>> BillAddress;
		public PXSelect<Contact, Where<Contact.bAccountID, Equal<Current<Customer.bAccountID>>,
			  And<Contact.contactID, Equal<Current<Customer.defBillContactID>>>>> BillContact;
		public PXSelect<CustSalesPeople, Where<CustSalesPeople.bAccountID, Equal<Current<Customer.bAccountID>>>, OrderBy<Asc<CustSalesPeople.salesPersonID, Asc<CustSalesPeople.locationID>>>> SalesPersons;
		public PXSetup<CustomerClass, Where<CustomerClass.customerClassID, Equal<Optional<Customer.customerClassID>>>> CustomerClass;
		public PXSetup<ARSetup> ARSetup;
		public PXFilter<CustomerBalanceSummary> CustomerBalance;
		public PXSelect<CarrierPluginCustomer, Where<CarrierPluginCustomer.customerID, Equal<Current<Customer.bAccountID>>>> Carriers;

		[PXCopyPasteHiddenView()]
		public PXSelect<CustomerPaymentMethod,
			Where<CustomerPaymentMethod.pMInstanceID, Equal<Optional<Customer.defPMInstanceID>>>> DefPaymentMethodInstance;
        public PXSelect<CustomerPaymentMethodInfo,
			Where<CustomerPaymentMethodInfo.pMInstanceID, Equal<Current<Customer.defPMInstanceID>>>> DefPaymentMethodInstanceInfo;
		[PXCopyPasteHiddenView()] 
		public PXSelect<CustomerPaymentMethodInfo, Where<CustomerPaymentMethodInfo.pMInstanceID, Equal<Optional<Customer.defPMInstanceID>>>> DefPaymentMethod;
		
		[PXCopyPasteHiddenView()]
		public PXSelect<CustomerPaymentMethodInfo, Where2<Where<CustomerPaymentMethodInfo.bAccountID, Equal<Current<Customer.bAccountID>>, Or<CustomerPaymentMethodInfo.bAccountID, IsNull>>,
			And<CustomerPaymentMethodInfo.isActive, IsNotNull>>> PaymentMethods;

		public IEnumerable paymentMethods()
		{
			PXResultset<CustomerPaymentMethodInfo> cpmInfo = PXSelect<CustomerPaymentMethodInfo, Where2<Where<CustomerPaymentMethodInfo.bAccountID, Equal<Current<Customer.bAccountID>>, Or<CustomerPaymentMethodInfo.bAccountID, IsNull>>,
				And<CustomerPaymentMethodInfo.isActive, IsNotNull>>>.Select(this);
			Dictionary<string, List<CustomerPaymentMethodInfo>> overrides = new Dictionary<string, List<CustomerPaymentMethodInfo>>();
			foreach (CustomerPaymentMethodInfo paymentMethodInfo in cpmInfo)
			{
				List<CustomerPaymentMethodInfo> infoList;
				if (!overrides.TryGetValue(paymentMethodInfo.PaymentMethodID, out infoList))
				{
					infoList = new List<CustomerPaymentMethodInfo>();
					overrides[paymentMethodInfo.PaymentMethodID] = infoList;
				}
				infoList.Add(paymentMethodInfo);
			}

			foreach (KeyValuePair<string, List<CustomerPaymentMethodInfo>> kvpInfo in overrides)
			{
				if (kvpInfo.Value.Count > 1)
				{
					CustomerPaymentMethodInfo sharedPM = kvpInfo.Value.FindLast((CustomerPaymentMethodInfo info) => info.ARIsOnePerCustomer == true);
					if (sharedPM != null)
					{
						yield return kvpInfo.Value.FindLast((CustomerPaymentMethodInfo info) => info.IsCustomerPaymentMethod == true);
					}
					else
					{
						foreach (CustomerPaymentMethodInfo pmInfo in kvpInfo.Value)
						{
							yield return pmInfo;
						}
					}
				}
				else
				{
					yield return kvpInfo.Value[0];
				}
			}
		}

		[PXCopyPasteHiddenView()]
		public PXSelectJoin<CustomerPaymentMethodDetail,
			  LeftJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<CustomerPaymentMethodDetail.paymentMethodID>,
			  And<PaymentMethodDetail.detailID, Equal<CustomerPaymentMethodDetail.detailID>,
              And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>,
			Where<CustomerPaymentMethodDetail.pMInstanceID, Equal<Current<Customer.defPMInstanceID>>>, OrderBy<Asc<PaymentMethodDetail.orderIndex>>> DefPaymentMethodInstanceDetails;

		public PXSelectJoin<CustomerPaymentMethodDetail,
			  LeftJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<CustomerPaymentMethodDetail.paymentMethodID>,
			  And<PaymentMethodDetail.detailID, Equal<CustomerPaymentMethodDetail.detailID>,
			  And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>,
			Where<CustomerPaymentMethodDetail.pMInstanceID, Equal<Current<Customer.defPMInstanceID>>>, OrderBy<Asc<PaymentMethodDetail.orderIndex>>> DefPaymentMethodInstanceDetailsAll;

		public PXSelect<PaymentMethodDetail,
			Where<PaymentMethodDetail.paymentMethodID, Equal<Optional<CustomerPaymentMethod.paymentMethodID>>,
            And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>> PMDetails;


		public IEnumerable defPaymentMethodInstanceDetails()
		{
			bool isTokenized = isTokenizedPaymentMethod(CurrentCustomer.Current.DefPMInstanceID);
			foreach (PXResult<CustomerPaymentMethodDetail, PaymentMethodDetail> res in PXSelectJoin<CustomerPaymentMethodDetail, LeftJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<CustomerPaymentMethodDetail.paymentMethodID>,
				And<PaymentMethodDetail.detailID, Equal<CustomerPaymentMethodDetail.detailID>,
					And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>,
				Where<CustomerPaymentMethodDetail.pMInstanceID, Equal<Current<Customer.defPMInstanceID>>>, OrderBy<Asc<PaymentMethodDetail.orderIndex>>>.Select(this))
			{
				PaymentMethodDetail pmd = res;
				if (isTokenized && pmd.IsCCProcessingID == true || !isTokenized && pmd.IsCCProcessingID != true)
				{
					yield return res;
				}
			}
		}

		public PXSelect<PaymentMethod,
		  Where<PaymentMethod.paymentMethodID, Equal<Optional<CustomerPaymentMethod.paymentMethodID>>>> PaymentMethodDef;

		public CRNotificationSourceList<Customer, Customer.customerClassID, ARNotificationSource.customer> NotificationSources;

		public CRNotificationRecipientList<Customer, Customer.customerClassID> NotificationRecipients;

		public PXSelectJoin<CCProcessingCenter, InnerJoin<CustomerPaymentMethod, 
			On<CCProcessingCenter.processingCenterID, Equal<CustomerPaymentMethod.cCProcessingCenterID>>>, 
				Where<CustomerPaymentMethod.pMInstanceID, Equal<Optional<Customer.defPMInstanceID>>>> PMProcessingCenter;

		public PXSelectJoin<CustomerPaymentMethodDetail, InnerJoin<PaymentMethodDetail,
			On<CustomerPaymentMethodDetail.paymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>,
				And<CustomerPaymentMethodDetail.detailID, Equal<PaymentMethodDetail.detailID>,
					And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>,
			Where<PaymentMethodDetail.isCCProcessingID, Equal<True>, And<CustomerPaymentMethodDetail.pMInstanceID, Equal<Optional<Customer.defPMInstanceID>>>>> ccpIdDet;

		public CustomerMaint()
		{
			this.PaymentMethods.Cache.AllowInsert = false;
			this.PaymentMethods.Cache.AllowDelete = false;
			//      this.PaymentMethods.Cache.AllowUpdate = false;
			//PXUIFieldAttribute.SetEnabled(this.PaymentMethods.Cache, null, false);
			ARSetup setup = ARSetup.Current;
			this.DefPaymentMethodInstanceDetails.Cache.AllowInsert = false;
			this.DefPaymentMethodInstanceDetails.Cache.AllowDelete = false;
            this.DefPaymentMethodInstance.Cache.AllowUpdate = false;
			PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.cashAccountID>(this.DefPaymentMethodInstance.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<CustomerPaymentMethodDetail.detailID>(this.DefPaymentMethodInstanceDetails.Cache, null, false);

			PXUIFieldAttribute.SetVisible<Customer.cOGSAcctID>(BAccount.Cache, null, false);
			PXUIFieldAttribute.SetVisible<Customer.cOGSSubID>(BAccount.Cache, null, false);

			PXUIFieldAttribute.SetEnabled<Contact.fullName>(Caches[typeof(Contact)], null);

			action.AddMenuAction(ChangeID);
		}


		#endregion

		#region Buttons

		public PXAction<Customer> viewRestrictionGroups;
		[PXUIField(DisplayName = GL.Messages.ViewRestrictionGroups, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewRestrictionGroups(PXAdapter adapter)
		{
			if (CurrentCustomer.Current != null)
			{
				ARAccessDetail graph = CreateInstance<ARAccessDetail>();
				graph.Customer.Current = graph.Customer.Search<Customer.acctCD>(CurrentCustomer.Current.AcctCD);
				throw new PXRedirectRequiredException(graph, false, "Restricted Groups");
			}
			return adapter.Get();
		}

		public PXAction<Customer> viewVendor;
		[PXUIField(DisplayName = Messages.ViewVendor, Enabled = false, Visible = true, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewVendor(PXAdapter adapter)
		{
			BAccount bacct = this.BAccount.Current;
			if (bacct != null && (bacct.Type == BAccountType.VendorType || bacct.Type == BAccountType.CombinedType))
			{
				Save.Press();
				AP.VendorMaint editingBO = PXGraph.CreateInstance<AP.VendorMaint>();
				editingBO.BAccount.Current = editingBO.BAccount.Search<AP.VendorR.acctCD>(bacct.AcctCD);
				throw new PXRedirectRequiredException(editingBO, CR.Messages.EditVendor);
			}
			return adapter.Get();
		}

		public PXAction<Customer> viewBusnessAccount;
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

		public PXAction<Customer> extendToVendor;
		[PXUIField(DisplayName = Messages.ExtendToVendor, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ExtendToVendor(PXAdapter adapter)
		{
			BAccount bacct = this.BAccount.Current;
			if (bacct != null && (bacct.Type == BAccountType.CustomerType))
			{
				Save.Press();
				AP.VendorMaint editingBO = PXGraph.CreateInstance<AP.VendorMaint>();
				AP.VendorR vendor = (AP.VendorR)editingBO.BAccount.Cache.Extend<BAccount>(bacct);
				editingBO.BAccount.Current = vendor;
				vendor.Type = BAccountType.CombinedType;
				LocationExtAddress defLocation = editingBO.DefLocation.Select();
				editingBO.DefLocation.Cache.RaiseRowSelected(defLocation);
				string locationType = LocTypeList.CombinedLoc;
				editingBO.InitVendorLocation(defLocation, locationType);
				defLocation = editingBO.DefLocation.Update(defLocation);
				foreach (Location iLoc in editingBO.IntLocations.Select())
				{
					if (iLoc.LocationID != defLocation.LocationID)
					{
						editingBO.InitVendorLocation(iLoc, locationType);
						editingBO.IntLocations.Update(iLoc);
					}
				}
				throw new PXRedirectRequiredException(editingBO, CR.Messages.EditVendor);
			}
			return adapter.Get();
		}

		public PXAction<Customer> customerDocuments;
		[PXUIField(DisplayName = "Customer Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable CustomerDocuments(PXAdapter adapter)
		{
			if (BAccount.Current != null && this.BAccount.Current.BAccountID > 0L)
			{
				ARDocumentEnq graph = PXGraph.CreateInstance<ARDocumentEnq>();
				graph.Filter.Current.CustomerID = BAccount.Current.BAccountID;
				graph.Filter.Select();
				throw new PXRedirectRequiredException(graph, "Customer Details");				
			}
			return adapter.Get();
		}

		public PXAction<Customer> statementForCustomer;
		[PXUIField(DisplayName = "Statement For Customer", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable StatementForCustomer(PXAdapter adapter)
		{
			if (BAccount.Current != null && this.BAccount.Current.BAccountID > 0L)
			{
				ARStatementForCustomer graph = PXGraph.CreateInstance<ARStatementForCustomer>();
				graph.Filter.Current.CustomerID = BAccount.Current.BAccountID;
				graph.Filter.Select();
				throw new PXRedirectRequiredException(graph, "Statement For Customer");
			}
			return adapter.Get();
		}

		public PXDBAction<Customer> viewPaymentMethod;
		[PXUIField(DisplayName = "View Payment Method", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewPaymentMethod(PXAdapter adapter)
		{
			if (this.PaymentMethods.Current != null)
			{
				CustomerPaymentMethodInfo current = this.PaymentMethods.Current as CustomerPaymentMethodInfo;
				Customer customer = this.BAccount.Current;
				if (customer != null && current != null && this.BAccount.Current.BAccountID > 0L)
				{
					if (current.ARIsOnePerCustomer != true)
					{
						CustomerPaymentMethodMaint graph = PXGraph.CreateInstance<CustomerPaymentMethodMaint>();
						graph.CustomerPaymentMethod.Current =
							graph.CustomerPaymentMethod.Search<CustomerPaymentMethod.pMInstanceID>(current.PMInstanceID, customer.AcctCD);
						PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
					}
					else
					{
						throw new PXSetPropertyException(Messages.NoPaymentInstance, PXErrorLevel.RowInfo);
					}
				}
			}
			return adapter.Get();
		}

		public PXDBAction<Customer> addPaymentMethod;
		[PXUIField(DisplayName = "Add Payment Method", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable AddPaymentMethod(PXAdapter adapter)
		{
			if (this.BAccount.Current != null && this.BAccount.Current.BAccountID > 0L)
			{
				Customer customer = this.BAccount.Current;
				CustomerPaymentMethodMaint graph = PXGraph.CreateInstance<CustomerPaymentMethodMaint>();
				CustomerPaymentMethod row = new CustomerPaymentMethod();
				row.BAccountID = customer.BAccountID;
                row = (CustomerPaymentMethod)graph.CustomerPaymentMethod.Insert(row);
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}
		
		/*public PXAction<Customer> autoApplyPayments;
		[PXUIField(DisplayName = "Auto Apply Payments", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable AutoApplyPayments(PXAdapter adapter)
		{
		  if (this.BAccount.Current != null)
		  {
			//Customer customer = this.BAccount.Current;
			ARAutoApplyPayments graph = PXGraph.CreateInstance<ARAutoApplyPayments>();
			throw new PXRedirectRequiredException(graph, "AutoApplyPayments");
		  }
		  return adapter.Get();
		}*/

		public PXAction<Customer> newInvoiceMemo;
		[PXUIField(DisplayName = "New Invoice/Memo", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable NewInvoiceMemo(PXAdapter adapter)
		{
			Customer customer = this.BAccount.Current;
			if (customer != null && customer.BAccountID > 0L)
			{
				ARInvoiceEntry invEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
				invEntry.Clear();
				ARInvoice newDoc = invEntry.Document.Insert(new ARInvoice());
				newDoc.CustomerID = customer.BAccountID;
				invEntry.Document.Cache.RaiseFieldUpdated<ARInvoice.customerID>(newDoc, null);
				throw new PXRedirectRequiredException(invEntry, "ARInvoiceEntry");
			}
			return adapter.Get();
		}

		public PXAction<Customer> newSalesOrder;
		[PXUIField(DisplayName = "New Sales Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable NewSalesOrder(PXAdapter adapter)
		{
			Customer customer = this.BAccount.Current;
			if (customer != null && customer.BAccountID > 0L)
			{
				SOOrderEntry soEntry = PXGraph.CreateInstance<SOOrderEntry>();
				soEntry.Clear();
				SOOrder newDoc = (SOOrder) soEntry.Document.Cache.Insert();
				newDoc.CustomerID = customer.BAccountID;
				soEntry.Document.Cache.RaiseFieldUpdated<ARInvoice.customerID>(newDoc, null);
				throw new PXRedirectRequiredException(soEntry, "SOOrderEntry");
			}
			return adapter.Get();
		}

		public PXAction<Customer> newPayment;
		[PXUIField(DisplayName = "New Payment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable NewPayment(PXAdapter adapter)
		{
			Customer customer = this.BAccount.Current;
			if (customer != null && customer.BAccountID > 0L)
			{
				ARPaymentEntry payEntry = PXGraph.CreateInstance<ARPaymentEntry>();
				payEntry.Clear();
				ARPayment newDoc = payEntry.Document.Insert(new ARPayment());
				newDoc.CustomerID = customer.BAccountID;
				payEntry.Document.Cache.RaiseFieldUpdated<ARInvoice.customerID>(newDoc, null);
				throw new PXRedirectRequiredException(payEntry, "ARPaymentEntry");
			}
			return adapter.Get();
		}

		public PXAction<Customer> writeOffBalance;
		[PXUIField(DisplayName = "Write Off Balance", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable WriteOffBalance(PXAdapter adapter)
		{
			Customer customer = this.BAccount.Current;
			if (customer != null && customer.BAccountID > 0L)
			{
				ARCreateWriteOff graph = PXGraph.CreateInstance<ARCreateWriteOff>();
				graph.Clear();
				graph.Filter.Current.CustomerID = customer.BAccountID;
				throw new PXRedirectRequiredException(graph, "WriteOffBalance");
			}
			return adapter.Get();
		}

		public PXAction<Customer> viewBillAddressOnMap;

		[PXUIField(DisplayName = CR.Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton()]
		public virtual IEnumerable ViewBillAddressOnMap(PXAdapter adapter)
		{

			BAccountUtility.ViewOnMap(this.BillAddress.Current);
			return adapter.Get();
		}


        public PXAction<Customer> regenerateLastStatement;
        [PXUIField(DisplayName = Messages.RegenerateLastStatement, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = true)]
        [PXButton]
        public virtual IEnumerable RegenerateLastStatement(PXAdapter adapter)
        {
            var customer = CurrentCustomer.Current;

            if (customer == null)
                return adapter.Get();

            var cycle = new PXSelect<ARStatementCycle,
                            Where<ARStatementCycle.statementCycleId,
                            Equal<Required<Customer.statementCycleId>>>>(this).SelectSingle(CurrentCustomer.Current.StatementCycleId);

            if (cycle == null)
                throw new PXException(Messages.StatementCycleNotSpecified);

            var statements = PXSelect<ARStatement, Where<ARStatement.customerID, Equal<Required<Customer.bAccountID>>,
                                                    And<ARStatement.statementCycleId, Equal<Required<ARStatementCycle.statementCycleId>>>>>.Select(this, customer.BAccountID, cycle.StatementCycleId);

            if (statements == null || statements.Count == 0)
                throw new PXException(Messages.NoStatementToRegenerate);

            PXLongOperation.StartOperation(this, () =>
                {
                    var process = PXGraph.CreateInstance<StatementCycleProcessBO>();
                    StatementCycleProcessBO.RegenerateStatements(process, cycle, new Customer[] { customer });
                });

            return adapter.Get();
        }

        #region MyButtons (MMK)
        public PXAction<Customer> action;
        [PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable Action(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<Customer> inquiry;
        [PXUIField(DisplayName = "Inquiries", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable Inquiry(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<Customer> report;
        [PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable Report(PXAdapter adapter)
        {
            return adapter.Get();
        }
        #endregion

        //+ MMK 2011/10/04
        public PXAction<Customer> aRBalanceByCustomer;
        [PXUIField(DisplayName = AR.Messages.ARBalanceByCustomer, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
        public virtual IEnumerable ARBalanceByCustomer(PXAdapter adapter)
        {
            Customer customer = this.BAccountAccessor.Current;
            if (customer != null && customer.BAccountID > 0L)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["CustomerID"] = customer.AcctCD;
                throw new PXReportRequiredException(parameters, "AR632500", AR.Messages.ARBalanceByCustomer); //?????
            }
            return adapter.Get();
        }

        public PXAction<Customer> customerHistory;
        [PXUIField(DisplayName = AR.Messages.CustomerHistory, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
        public virtual IEnumerable CustomerHistory(PXAdapter adapter)
        {
            Customer customer = this.BAccountAccessor.Current;
            if (customer != null && customer.BAccountID > 0L)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["CustomerID"] = customer.AcctCD;
                throw new PXReportRequiredException(parameters, "AR652000", AR.Messages.CustomerHistory); //?????
            }
            return adapter.Get();
        }

        public PXAction<Customer> aRAgedPastDue;
        [PXUIField(DisplayName = AR.Messages.ARAgedPastDue, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
        public virtual IEnumerable ARAgedPastDue(PXAdapter adapter)
        {
            Customer customer = this.BAccountAccessor.Current;
            if (customer != null && customer.BAccountID > 0L)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["CustomerID"] = customer.AcctCD;
                throw new PXReportRequiredException(parameters, "AR631000", AR.Messages.ARAgedPastDue); //?????
            }
            return adapter.Get();
        }

        public PXAction<Customer> aRAgedOutstanding;
        [PXUIField(DisplayName = AR.Messages.ARAgedOutstanding, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
        public virtual IEnumerable ARAgedOutstanding(PXAdapter adapter)
        {
            Customer customer = this.BAccountAccessor.Current;
            if (customer != null && customer.BAccountID > 0L)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["CustomerID"] = customer.AcctCD;
                throw new PXReportRequiredException(parameters, "AR631500", AR.Messages.ARAgedOutstanding); //?????
            }
            return adapter.Get();
        }

        public PXAction<Customer> aRRegister;
        [PXUIField(DisplayName = AR.Messages.ARRegister, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
        public virtual IEnumerable ARRegister(PXAdapter adapter)
        {
            Customer customer = this.BAccountAccessor.Current;
            if (customer != null && customer.BAccountID > 0L)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["CustomerID"] = customer.AcctCD;
                throw new PXReportRequiredException(parameters, "AR621500", AR.Messages.ARRegister); //?????
            }
            return adapter.Get();
        }

        public PXAction<Customer> customerDetails;
        [PXUIField(DisplayName = AR.Messages.CustomerDetails, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
        public virtual IEnumerable CustomerDetails(PXAdapter adapter)
        {
            Customer customer = this.BAccountAccessor.Current;
            if (customer != null && customer.BAccountID > 0L)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["CustomerID"] = customer.AcctCD;
                throw new PXReportRequiredException(parameters, "AR651000", AR.Messages.CustomerDetails); //?????
            }
            return adapter.Get();
        }

        //- MMK 2011/10/04

        public PXAction<Customer> customerStatement;
        [PXUIField(DisplayName = AR.Messages.CustomerStatement, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
        public virtual IEnumerable CustomerStatement(PXAdapter adapter)
        {
            var parameters = GetStatementReportParams();
            if (parameters == null)
                return adapter.Get();

            throw new PXReportRequiredException(GetStatementReportParams(), "AR641500", Messages.CustomerStatement);
        }

        public PXAction<Customer> customerStatementMC;
        [PXUIField(DisplayName = AR.Messages.CustomerStatementMC, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Report)]
        public virtual IEnumerable CustomerStatementMC(PXAdapter adapter)
        {
            var parameters = GetStatementReportParams();
            if (parameters == null)
                return adapter.Get();

            throw new PXReportRequiredException(GetStatementReportParams(), "AR642000", Messages.CustomerStatementMC);
        }

        private Dictionary<string, string> GetStatementReportParams()
        {
            var customer = CurrentCustomer.Current;

            if (customer == null)
                return null;

            var cycle = new PXSelect<ARStatementCycle,
                            Where<ARStatementCycle.statementCycleId,
                            Equal<Required<Customer.statementCycleId>>>>(this).SelectSingle(customer.StatementCycleId);

            if (cycle == null)
                throw new PXException(Messages.StatementCycleNotSpecified);

            if (cycle.LastStmtDate == null)
                throw new PXException(Messages.NoStatementsForCustomer);

            var parameters = new Dictionary<string, string>();
            parameters["CustomerId"] = customer.AcctCD;
            parameters["StatementCycleId"] = customer.StatementCycleId;
            parameters["StatementDate"] = cycle.LastStmtDate.Value.ToShortDateString();

            return parameters;
        }

		#region Validate Address Button
		public new PXAction<Customer> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton]
		public override IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			Customer bacct = this.BAccount.Current;
			if (bacct != null)
			{
				bool needSave = false;
				Save.Press();
				Address address = FindAddress(bacct.DefAddressID); 
				if (address != null && address.IsValidated == false)
				{
					if (PXAddressValidator.Validate<Address>(this, address, true))
						needSave = true;
				}
				Address billAddress = FindAddress(bacct.DefBillAddressID); 
				if (billAddress != null && billAddress.IsValidated == false && (billAddress.AddressID != address.AddressID))
				{
					if (PXAddressValidator.Validate<Address>(this, billAddress, true))
						needSave = true;
				}

				LocationExtAddress locAddress = this.DefLocation.Current;
				//Needs to compare defAddress - AddressID  would be null
				if (locAddress != null && locAddress.IsValidated == false && locAddress.IsAddressSameAsMain == false)
				{
					if (PXAddressValidator.Validate<LocationExtAddress>(this, locAddress, true))
						needSave = true;
				}
				if (needSave == true)
					this.Save.Press();

			}
			return adapter.Get();
		}
		#endregion

		public PXChangeID<Customer, Customer.acctCD> ChangeID;

		#endregion

		#region Select Delegates and Sefault Accessors

		protected virtual IEnumerable billContact()
		{
			Contact cnt = null;
			Customer customer = this.BAccount.Current;
			if (customer != null && customer.DefBillContactID != null)
			{
				cnt = FindContact(customer.DefBillContactID);
				if (cnt != null)
				{
					if (customer.IsBillContSameAsMain == true)
					{
						cnt = PXCache<Contact>.CreateCopy(cnt);
						PXUIFieldAttribute.SetEnabled(this.BillContact.Cache, cnt, false);
					}
					else
					{
						PXUIFieldAttribute.SetEnabled(this.BillContact.Cache, cnt, true);
					}
				}
			}
			return new Contact[] { cnt };
		}

		protected virtual IEnumerable billAddress()
		{
			Address addr = null;
			Customer customer = this.BAccount.Current;
			if (customer != null && customer.DefBillAddressID != null)
			{
				addr = FindAddress(customer.DefBillAddressID);
				if (addr != null)
				{
					if (customer.IsBillSameAsMain == true)
					{
						addr = PXCache<Address>.CreateCopy(addr);
						addr.AddressID = null;
						PXUIFieldAttribute.SetEnabled(this.BillAddress.Cache, addr, false);
						formBillAddress = addr;
					}
					else
					{
						PXUIFieldAttribute.SetEnabled(this.BillAddress.Cache, addr, true);
					}
				}
			}
			return new Address[] { addr };
		}

		protected virtual IEnumerable customerBalance()
		{

			Customer customer = (Customer)this.BAccountAccessor.Current;
			List<CustomerBalanceSummary> list = new List<CustomerBalanceSummary>(1);
			if (customer != null && customer.BAccountID > 0L)
			{
				bool isInserted = (this.BAccountAccessor.Cache.GetStatus(customer) == PXEntryStatus.Inserted);
				if (!isInserted)
				{
                    CustomerBalanceSummary res = new CustomerBalanceSummary();
                    CuryARHistory prepaymentbal = PXSelectJoinGroupBy<CuryARHistory, InnerJoin<ARCustomerBalanceEnq.ARLatestHistory, On<ARCustomerBalanceEnq.ARLatestHistory.accountID, Equal<CuryARHistory.accountID>,
								And<ARCustomerBalanceEnq.ARLatestHistory.branchID, Equal<CuryARHistory.branchID>,
								And<ARCustomerBalanceEnq.ARLatestHistory.customerID, Equal<CuryARHistory.customerID>,
								And<ARCustomerBalanceEnq.ARLatestHistory.subID, Equal<CuryARHistory.subID>,
								And<ARCustomerBalanceEnq.ARLatestHistory.curyID, Equal<CuryARHistory.curyID>,
								And<ARCustomerBalanceEnq.ARLatestHistory.lastActivityPeriod, Equal<CuryARHistory.finPeriodID>>>>>>>>,
								Where<ARCustomerBalanceEnq.ARLatestHistory.customerID, Equal<Current<Customer.bAccountID>>>,
								Aggregate<Sum<CuryARHistory.finYtdDeposits>>>.Select(this);

                    ARBalances bal = PXSelectGroupBy<ARBalances,
                                        Where<ARBalances.customerID, Equal<Current<Customer.bAccountID>>>,
                                            Aggregate<
                                            Sum<ARBalances.currentBal,
                                            Sum<ARBalances.totalOpenOrders,
                                            Sum<ARBalances.totalPrepayments,
                                            Sum<ARBalances.totalShipped,
                                            Sum<ARBalances.unreleasedBal,
                                            Min<ARBalances.oldInvoiceDate>>>>>>>>.Select(this);

				    res.Balance = bal.CurrentBal;
				    res.UnreleasedBalance = bal.UnreleasedBal;
				    res.OpenOrdersBalance = bal.TotalOpenOrders;
                    res.DepositsBalance = prepaymentbal.FinYtdDeposits;
                    TimeSpan overdue = (DateTime)Accessinfo.BusinessDate - (DateTime)(bal.OldInvoiceDate ?? Accessinfo.BusinessDate);
                    
                    if (customer.CreditRule == CreditRuleTypes.CS_DAYS_PAST_DUE || customer.CreditRule == CreditRuleTypes.CS_BOTH)
                    {
                        res.OldInvoiceDate = bal.OldInvoiceDate;
                        if ((customer.CreditDaysPastDue ?? 0) < overdue.Days)
                        {
                            CustomerBalance.Cache.RaiseExceptionHandling<CustomerBalanceSummary.oldInvoiceDate>(res, res.OldInvoiceDate, new PXSetPropertyException(Messages.CreditDaysPastDueWereExceeded, PXErrorLevel.Warning));
                        }
                    }
                    else
                    {
                        res.OldInvoiceDate = null;
                    }

                    if (customer.CreditRule == CreditRuleTypes.CS_CREDIT_LIMIT || customer.CreditRule == CreditRuleTypes.CS_BOTH)
                    {
                        res.RemainingCreditLimit = customer.CreditLimit - ((bal.CurrentBal ?? 0) + (bal.UnreleasedBal ?? 0) + (bal.TotalOpenOrders ?? 0) + (bal.TotalShipped ?? 0) - (bal.TotalPrepayments ?? 0));
                        if(res.RemainingCreditLimit <= decimal.Zero)
                        {
                            CustomerBalance.Cache.RaiseExceptionHandling<CustomerBalanceSummary.remainingCreditLimit>(res, res.RemainingCreditLimit, new PXSetPropertyException(Messages.CreditLimitWasExceeded, PXErrorLevel.Warning));
                        }
                    }
                    else
                    {
                        res.RemainingCreditLimit = decimal.Zero;
                    }
					list.Add(res);
				}
				//PXUIFieldAttribute.SetVisible<CustomerBalanceSummary.balance>(this.CustomerBalance.Cache, null, !isInserted);
			}
			return list;
		}
		
		#endregion

		public override void Persist()
		{
			using (PXConnectionScope cs = new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					try
					{
						base.Persist(typeof(Customer), PXDBOperation.Update);
					}
					catch
					{
						Caches[typeof(Customer)].Persisted(true);
						throw;
					}
					base.Persist();
					ts.Complete();
				}
			}
		}

		#region Customer events

		[PXDBInt()]
		[PXDBChildIdentity(typeof(LocationExtAddress.locationID))]
		[PXUIField(DisplayName = "Default Location", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(Search<LocationExtAddress.locationID,
			Where<LocationExtAddress.bAccountID,
			Equal<Current<Customer.bAccountID>>>>),
			DescriptionField = typeof(LocationExtAddress.locationCD),
			DirtyRead = true)]
		protected virtual void Customer_DefLocationID_CacheAttached(PXCache sender)
		{
			
		}

		protected virtual void Customer_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
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

			if (CustomerClass.Current != null && CustomerClass.Current.DefaultLocationCDFromBranch == true)
			{
				this.RowInserting.AddHandler<LocationExtAddress>(inserting);
			}

			base.OnBAccountRowInserted(cache, e);

			if (CustomerClass.Current != null && CustomerClass.Current.DefaultLocationCDFromBranch == true)
			{
				this.RowInserting.RemoveHandler<LocationExtAddress>(inserting);
			}

			bool needUpdate = false;
			Customer customer = (Customer)e.Row;
			if (!customer.DefBillAddressID.HasValue)
			{
				customer.DefBillAddressID = customer.DefAddressID;
				needUpdate = true;
			}
			if (!customer.DefBillContactID.HasValue)
			{
				customer.DefBillContactID = customer.DefContactID;
				needUpdate = true;
			}
			if (customer.CustomerClassID != null)
			{
				cache.SetDefaultExt<Customer.defPaymentMethodID>(customer);
			}
			if (needUpdate)
				this.BAccountAccessor.Cache.Update(customer);
		}

		protected virtual void Customer_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			Customer row = (Customer)e.Row;
			viewRestrictionGroups.SetEnabled(row != null && row.AcctCD != null);
			if (row == null) return;

			if (row.DefBillAddressID == row.DefAddressID)
			{
				row.IsBillSameAsMain = true;
			}

			if (row.DefBillContactID == row.DefContactID)
			{
				row.IsBillContSameAsMain = true;
			}
			//base.BAccount_RowSelected(cache, e); //BaseContact will be inserted here
			if (!row.DefBillContactID.HasValue)
			{
				row.DefBillContactID = row.DefContactID;
				row.IsBillContSameAsMain = true;
			}

			PXEntryStatus status = cache.GetStatus(row);
			viewVendor.SetEnabled((row.Type == BAccountType.VendorType || row.Type == BAccountType.CombinedType) && status != PXEntryStatus.Inserted);
			viewBusnessAccount.SetEnabled(status != PXEntryStatus.Inserted);
			
			bool creditRuleBoth = (row.CreditRule == CreditRuleTypes.CS_BOTH);
			PXUIFieldAttribute.SetEnabled<Customer.creditLimit>(cache, row, (row.CreditRule == CreditRuleTypes.CS_CREDIT_LIMIT || creditRuleBoth));
			PXUIFieldAttribute.SetEnabled<Customer.creditDaysPastDue>(cache, row, (row.CreditRule == CreditRuleTypes.CS_DAYS_PAST_DUE || creditRuleBoth));			

			bool smallBalanceAllow = (row.SmallBalanceAllow ?? false);
			PXUIFieldAttribute.SetEnabled<Customer.smallBalanceLimit>(cache, row, smallBalanceAllow);
            if(!smallBalanceAllow)row.SmallBalanceLimit = 0;

			bool mCActivated = (CMSetup.Current.MCActivated == true);
			PXUIFieldAttribute.SetVisible<Customer.curyID>(cache, null, mCActivated);
			PXUIFieldAttribute.SetVisible<Customer.curyRateTypeID>(cache, null, mCActivated);
			PXUIFieldAttribute.SetVisible<Customer.printCuryStatements>(cache, null, mCActivated);
			PXUIFieldAttribute.SetVisible<Customer.allowOverrideCury>(cache, null, mCActivated);
			PXUIFieldAttribute.SetVisible<Customer.allowOverrideRate>(cache, null, mCActivated);

            writeOffBalance.SetEnabled(row.SmallBalanceAllow ?? false);			

			if (this.DefLocation.Current != null && this.CustomerClass.Current != null)
			{
				bool isRequired = (this.CustomerClass.Current.RequireTaxZone ?? false);
				PXDefaultAttribute.SetPersistingCheck<LocationExtAddress.cTaxZoneID>(this.DefLocation.Cache, this.DefLocation.Current, isRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
				PXUIFieldAttribute.SetRequired<LocationExtAddress.cTaxZoneID>(this.DefLocation.Cache, isRequired);
				if (isRequired && string.IsNullOrEmpty(this.DefLocation.Current.CTaxZoneID))
				{
					if (this.DefLocation.Cache.GetStatus(this.DefLocation.Current) == PXEntryStatus.Notchanged)
					{
						this.DefLocation.Cache.SetStatus(this.DefLocation.Current, PXEntryStatus.Updated);
					}
				}
			}
			CustomerPaymentMethod cpm = DefPaymentMethodInstance.Select(CurrentCustomer.Current.DefPMInstanceID);
			bool enablePMEdit = cpm != null;
			this.PaymentMethodDef.Cache.RaiseRowSelected(PaymentMethodDef.Current);
			this.DefPaymentMethodInstance.Cache.AllowUpdate = enablePMEdit;
			this.DefPaymentMethodInstanceDetails.Cache.AllowUpdate = enablePMEdit && !isTokenizedPaymentMethod(row.DefPMInstanceID);
		}

		protected virtual void Customer_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			Customer row = e.Row as Customer;
			if (row != null && row.Type == BAccountType.CombinedType)
			{
				// We shouldn't delete BAccount entity when it is in use by Vendor entity
				PXTableAttribute tableAttr = cache.Interceptor as PXTableAttribute;
				tableAttr.BypassOnDelete(typeof(BAccount));
			}
		}

		protected virtual void Customer_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			Customer row = e.Row as Customer;
			if (row != null && row.Type == BAccountType.CombinedType)
				ChangeBAccountType(row, BAccountType.VendorType);
		}

		protected virtual void Customer_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			Customer row = (Customer)e.Row;
			if (row != null)
			{
				if (e.Operation == PXDBOperation.Delete)
				{
					
					
				}
				else
				{
					if ((row.SendStatementByEmail ?? true) || (row.MailInvoices ?? true))
					{
						bool defBillContactEquals = (row.DefBillContactID == row.DefContactID);
						if (this.DefContact.Current == null)
						{
							this.DefContact.Current = this.DefContact.Select();
						}
						if (this.BillContact.Current == null)
						{
							this.BillContact.Current = this.BillContact.Select();
						}
						Contact contact = defBillContactEquals ? this.DefContact.Current : this.BillContact.Current;
						PXCache contactCache = defBillContactEquals ? this.DefContact.Cache : this.BillContact.Cache;
						if (contact != null && String.IsNullOrEmpty(contact.EMail))
						{
							if (contactCache.GetStatus(contact) == PXEntryStatus.Notchanged)
							{
								contactCache.SetStatus(contact, PXEntryStatus.Updated);
							}

							RaiseEmailErrors(cache, contactCache, contact, row);
						}
					}
				}
			}
		}

        private void RaiseEmailErrors(PXCache persistingCache, PXCache contactCache, Contact contact, Customer customer)
        {
            string statementsFieldName = PXUIFieldAttribute.GetDisplayName<Customer.sendStatementByEmail>(persistingCache);
            string invoicesFieldName = PXUIFieldAttribute.GetDisplayName<Customer.mailInvoices>(persistingCache);
            string bothFields = String.Join(", ", statementsFieldName, invoicesFieldName);

            contactCache.RaiseExceptionHandling<Contact.eMail>(contact, contact.EMail, new PXSetPropertyException(Messages.ERR_EmailIsRequiredForSendByEmailOptions, PXErrorLevel.RowError, bothFields));

            if (customer.MailInvoices ?? true)
            {
                persistingCache.RaiseExceptionHandling<Customer.mailInvoices>(customer, customer.SendStatementByEmail, new PXSetPropertyException(Messages.ERR_EmailIsRequiredForOption, PXErrorLevel.RowError, invoicesFieldName));
            }
            if (customer.SendStatementByEmail ?? true)
            {
                persistingCache.RaiseExceptionHandling<Customer.sendStatementByEmail>(customer, customer.SendStatementByEmail, new PXSetPropertyException(Messages.ERR_EmailIsRequiredForOption, PXErrorLevel.RowError, statementsFieldName));
            }

            throw new PXSetPropertyException(Messages.ERR_EmailIsRequiredForSendByEmailOptions, PXErrorLevel.RowError, bothFields);
        }

		protected virtual void Customer_AcctName_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			base.OnBAccountAcctNameFieldUpdated(cache, e);
		}

		protected virtual void Customer_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
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

		protected virtual void Customer_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (CMSetup.Current.MCActivated != true)
			{
				e.NewValue = string.Empty;
				e.Cancel = true;
			}
		}

		protected virtual void Customer_CuryRateTypeID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (CMSetup.Current.MCActivated != true)
			{
				e.Cancel = true;
			}
		}

		protected virtual void Customer_IsBillSameAsMain_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Customer owner = (Customer)e.Row;
			if (owner != null)
			{
				if (owner.IsBillSameAsMain == true)
				{
					if (owner.DefBillAddressID != owner.DefAddressID)
					{
						Address extAddr = this.FindAddress(owner.DefBillAddressID);
						if (extAddr != null && extAddr.AddressID == owner.DefBillAddressID)
						{
							Address defAddr = this.FindAddress(owner.DefAddressID);
							defAddr.RevisionID = extAddr.RevisionID + 1;
							DefAddress.Cache.Update(defAddr);

							this.BillAddress.Delete(extAddr);
						}
						owner.DefBillAddressID = owner.DefAddressID;
					}
				}
				else if (owner.DefBillAddressID != null)
				{
					if (owner.DefBillAddressID == owner.DefAddressID)
					{
						Address source = this.FindAddress(owner.DefAddressID);
						Address addr = PXCache<Address>.CreateCopy(source);

						addr.AddressID = null;
						addr.RevisionID++;
						addr = this.BillAddress.Insert(addr);
						owner.DefBillAddressID = addr.AddressID;
						formBillAddress = addr;
					}
				}

			}

		}

		protected virtual void Customer_IsBillContSameAsMain_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Customer row = (Customer)e.Row;
			if (row != null)
			{
				if (row.IsBillContSameAsMain == true)
				{
					if (row.DefBillContactID != row.DefContactID)
					{
						Contact contact = this.FindContact(row.DefBillContactID);
						if (contact != null && contact.ContactID == row.DefBillContactID)
						{
							this.BillContact.Delete(contact);
						}
						row.DefBillContactID = row.DefContactID;
					}
				}
				else if (row.DefBillContactID != null)
				{
					if (row.DefBillContactID == row.DefContactID)
					{
						Contact cont = new Contact();
						Contact defContact = this.FindContact(row.DefContactID);
						PXCache<Contact>.RestoreCopy(cont, defContact);
						cont.ContactID = null;
						cont.BAccountID = row.BAccountID;
						cont.ContactType = ContactTypesAttribute.BAccountProperty;
						cont = this.BillContact.Insert(cont);
						row.DefBillContactID = cont.ContactID;

					}
				}
			}
		}

		protected virtual void Customer_CustomerClassID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Customer row = (Customer)e.Row;
			CustomerClass cc = (CustomerClass)PXSelectorAttribute.Select<Customer.customerClassID>(cache, row, e.NewValue);
			this.doCopyClassSettings = false;
			if (cc != null)
			{
				this.doCopyClassSettings = true;
				if (cache.GetStatus(row) != PXEntryStatus.Inserted)
				{
					if (BAccount.Ask(Messages.Warning, Messages.CustomerClassChangeWarning, MessageButtons.YesNo) == WebDialogResult.No)
					{
						this.doCopyClassSettings = false;
					}
				}
			}
		}

		protected virtual void Customer_CustomerClassID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Customer row = (Customer)e.Row;
			if (row != null)
			{
				if (this.DefLocation.Current == null)
				{
					this.DefLocation.Current = this.DefLocation.Select();
				}

				CustomerClass.RaiseFieldUpdated(cache, e.Row);

				if (CustomerClass.Current != null && CustomerClass.Current.DefaultLocationCDFromBranch == true)
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

			    if (this.doCopyClassSettings)
				{
					CopyAccounts(cache, row);

					InitCustomerLocation(this.DefLocation.Current, this.DefLocation.Current.LocType);

					if(CustomerClass.Current.SalesPersonID != null)
					{
						CustSalesPeople sperson = new CustSalesPeople { SalesPersonID = CustomerClass.Current.SalesPersonID, IsDefault = true };
						SalesPersons.Insert(sperson);
					}
				}
				bool isInserted = (cache.GetStatus(row) == PXEntryStatus.Inserted);
				if (isInserted || String.IsNullOrEmpty(row.DefPaymentMethodID))
				{
                    cache.SetDefaultExt<Customer.defPaymentMethodID>(row);
				}
			}
		}

        protected virtual void Customer_DefPaymentMethodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
			//assuming that this field assigned directly only when defaulting from Customer Class
			Customer row = (Customer) e.Row;
	        if (row == null) return;

			if (row.DefPMInstanceID.HasValue)
			{
				if (this.DefPaymentMethodInstance.Current == null ||
					this.DefPaymentMethodInstance.Current.PMInstanceID != row.DefPMInstanceID)
				{
					this.DefPaymentMethodInstance.Current = this.DefPaymentMethodInstance.Select();
				}
				CustomerPaymentMethod current = this.DefPaymentMethodInstance.Current;
				if (current != null && current.PaymentMethodID != row.DefPaymentMethodID
					&& this.DefPaymentMethodInstance.Cache.GetStatus(current) == PXEntryStatus.Inserted)
				{
					this.DefPaymentMethodInstance.Delete(current);
					cache.SetValue<Customer.defPMInstanceID>(row, null);
				}
				else if (current == null)
				{
					cache.SetValue<Customer.defPMInstanceID>(row, null);
				}
			}

	        PaymentMethod paymentMethod = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, row.DefPaymentMethodID);
			if (paymentMethod != null)
			{
				if (paymentMethod.ARIsOnePerCustomer == true)
				{
					cache.SetValueExt<Customer.defPMInstanceID>(row, paymentMethod.PMInstanceID);
				}
				else
				{
					CreateDefPaymentMethod(row);
				}
			}
			else
			{
				cache.SetValueExt<Customer.defPMInstanceID>(row, null);
			}
        }

		protected virtual void Customer_DefPMInstanceID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Customer row = (Customer) e.Row;
			if (row == null) return;
			CustomerPaymentMethodInfo pmInfo = DefPaymentMethod.Select(row.DefPMInstanceID);
			if (pmInfo != null)
			{
				cache.SetValue<Customer.defPaymentMethodID>(row, pmInfo.PaymentMethodID);
			}
		}

		protected virtual void Customer_CreditRule_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Customer row = (Customer)e.Row;
			bool creditRuleNoCheking = (row.CreditRule == CreditRuleTypes.CS_NO_CHECKING);
			if (row.CreditRule == CreditRuleTypes.CS_CREDIT_LIMIT || creditRuleNoCheking)
			{
				row.CreditDaysPastDue = 0;
			}
			if (row.CreditRule == CreditRuleTypes.CS_DAYS_PAST_DUE || creditRuleNoCheking)
			{
				row.CreditLimit = 0m;
			}
		}

		protected virtual void Customer_SmallBalanceAllow_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Customer row = (Customer)e.Row;
			row.SmallBalanceLimit = 0m;
		}
		#endregion

		#region LocationExtAddress Events

		[PXDBString(BqlField = typeof(CR.Standalone.Location.cTaxZoneID))]
		[PXUIField(DisplayName = "Tax Zone", Required = false)]
		[PXDefault(typeof(Search<AR.CustomerClass.taxZoneID, Where<AR.CustomerClass.customerClassID, Equal<Current<AR.Customer.customerClassID>>>>), PersistingCheck=PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<TX.TaxZone.taxZoneID>), DescriptionField = typeof(TX.TaxZone.descr), CacheGlobal = true)]
		public virtual void LocationExtAddress_CTaxZoneID_CacheAttached(PXCache sender)
		{ 
		}

		protected virtual void LocationExtAddress_VBranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		protected virtual void LocationExtAddress_CARAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (CustomerClass.Current != null)
			{
				e.NewValue = CustomerClass.Cache.GetValue<CustomerClass.aRAcctID>(CustomerClass.Current);
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_CARSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (CustomerClass.Current != null)
			{
				e.NewValue = CustomerClass.Cache.GetValue<CustomerClass.aRSubID>(CustomerClass.Current);
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_CSalesAcctID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress loc = e.Row as LocationExtAddress;
			if (loc == null) return;
			BAccount acct = this.BAccountAccessor.Current;
			if (acct != null &&
				(acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType) &&
				loc.IsDefault == true)
			{
				CustomerClass cClass = CustomerClass.Current;
				if (cClass != null && cClass.SalesAcctID != null)
				{
					e.NewValue = cClass.SalesAcctID;
					e.Cancel = true;
				}
			}
		}
		protected virtual void LocationExtAddress_CPriceClassID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress loc = e.Row as LocationExtAddress;
			if (loc == null) return;
			BAccount acct = this.BAccountAccessor.Current;
			if (acct != null &&
				(acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType) &&
				loc.IsDefault == true)
			{
				CustomerClass cClass = CustomerClass.Current;
				if (cClass != null && cClass.PriceClassID != null)
				{
					e.NewValue = cClass.PriceClassID;
					e.Cancel = true;
				}
			}
		}
		protected virtual void LocationExtAddress_CSalesSubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			LocationExtAddress loc = e.Row as LocationExtAddress;
			if (loc == null) return;
			BAccount acct = this.BAccountAccessor.Current;
			if (acct != null &&
				(acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType) &&
				loc.IsDefault == true)
			{
				CustomerClass cClass = CustomerClass.Current;
				if (cClass != null && cClass.SalesSubID != null)
				{
					e.NewValue = cClass.SalesSubID;
					e.Cancel = true;
				}
			}
		}

        protected virtual void LocationExtAddress_CDiscountAcctID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            LocationExtAddress loc = e.Row as LocationExtAddress;
            if (loc == null) return;
            BAccount acct = this.BAccountAccessor.Current;
            if (acct != null &&
                (acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType) &&
                loc.IsDefault == true)
            {
                CustomerClass cClass = CustomerClass.Current;
                if (cClass != null && cClass.DiscountAcctID != null)
                {
                    e.NewValue = cClass.DiscountAcctID;
                    e.Cancel = true;
                }
            }
        }

        protected virtual void LocationExtAddress_CDiscountSubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            LocationExtAddress loc = e.Row as LocationExtAddress;
            if (loc == null) return;
            BAccount acct = this.BAccountAccessor.Current;
            if (acct != null &&
                (acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType) &&
                loc.IsDefault == true)
            {
                CustomerClass cClass = CustomerClass.Current;
                if (cClass != null && cClass.DiscountSubID != null)
                {
                    e.NewValue = cClass.DiscountSubID;
                    e.Cancel = true;
                }
            }
        }

        protected virtual void LocationExtAddress_CFreightAcctID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            LocationExtAddress loc = e.Row as LocationExtAddress;
            if (loc == null) return;
            BAccount acct = this.BAccountAccessor.Current;
            if (acct != null &&
                (acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType) &&
                loc.IsDefault == true)
            {
                CustomerClass cClass = CustomerClass.Current;
                if (cClass != null && cClass.FreightAcctID != null)
                {
                    e.NewValue = cClass.FreightAcctID;
                    e.Cancel = true;
                }
            }
        }

        protected virtual void LocationExtAddress_CFreightSubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            LocationExtAddress loc = e.Row as LocationExtAddress;
            if (loc == null) return;
            BAccount acct = this.BAccountAccessor.Current;
            if (acct != null &&
                (acct.Type == BAccountType.CustomerType || acct.Type == BAccountType.CombinedType) &&
                loc.IsDefault == true)
            {
                CustomerClass cClass = CustomerClass.Current;
                if (cClass != null && cClass.FreightSubID != null)
                {
                    e.NewValue = cClass.FreightSubID;
                    e.Cancel = true;
                }
            }
        }

		protected virtual void LocationExtAddress_CCarrierID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (CustomerClass.Current != null)
			{
				e.NewValue = CustomerClass.Cache.GetValue<CustomerClass.shipVia>(CustomerClass.Current);
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_CShipTermsID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (CustomerClass.Current != null)
			{
				e.NewValue = CustomerClass.Cache.GetValue<CustomerClass.shipTermsID>(CustomerClass.Current);
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_CShipComplete_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (CustomerClass.Current != null)
			{
				e.NewValue = CustomerClass.Cache.GetValue<CustomerClass.shipComplete>(CustomerClass.Current) ?? SOShipComplete.CancelRemainder;
				e.Cancel = true;
			}
		}

		protected override void LocationExtAddress_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			base.LocationExtAddress_RowPersisting(cache, e);
			if (e.Cancel) return;
			LocationExtAddress loc = (LocationExtAddress)e.Row;
			if (loc == null) return;
			BAccount acct = this.BAccountAccessor.Current;
			if (acct != null &&
					(acct.Type == BAccountType.CustomerType ||
					 acct.Type == BAccountType.CombinedType))
			{
				if (loc.LocationID == acct.DefLocationID && loc.IsActive != true)
				{
					cache.RaiseExceptionHandling<Location.isActive>(loc, null, new PXSetPropertyException(CR.Messages.DefaultLocationCanNotBeNotActive, typeof(Location.isActive).Name));
				}
				if (loc.CSalesAcctID == null)
				{
					cache.RaiseExceptionHandling<Location.cSalesAcctID>(loc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Location.cSalesAcctID).Name));
				}
				if (loc.CSalesSubID == null)
				{
					cache.RaiseExceptionHandling<Location.cSalesSubID>(loc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Location.cSalesSubID).Name));
				}
				if (CustomerClass.Current != null && CustomerClass.Current.RequireTaxZone == true && loc.CTaxZoneID == null)
				{
					cache.RaiseExceptionHandling<Location.cTaxZoneID>(loc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Location.cTaxZoneID).Name));
				}
			}
		}
		#endregion

		#region Address Events
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
			addr.RevisionID++;
			if (formBillAddress != null &&
			  addr != null)
			{
				int? id = formBillAddress.AddressID;
				PXCache<Address>.RestoreCopy(formBillAddress, addr);
				formBillAddress.AddressID = id;

			}
		}



		#endregion

		#region Contact Address
		protected virtual void Contact_EMail_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Contact row = (Contact)e.Row;
			Customer owner = this.BAccount.Current;
			if (owner != null && row != null)
			{
				if (row.ContactID == owner.DefBillContactID)
				{
					if (this.BAccount.Cache.GetStatus(owner) == PXEntryStatus.Notchanged)
					{
						this.BAccount.Cache.SetStatus(owner, PXEntryStatus.Updated);
					}
				}
			}
		}
		protected virtual void NotificationRecipient_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			NotificationRecipient row = (NotificationRecipient)e.Row;
			if (row == null) return;
			Contact contact = PXSelectorAttribute.Select<NotificationRecipient.contactID>(cache, row) as Contact;
			if (contact == null)
			{
				switch (row.ContactType)
				{
					case CustomerContactType.Primary:
						contact = DefContact.SelectWindowed(0, 1);
						break;
					case CustomerContactType.Billing:
						contact = BillContact.SelectWindowed(0, 1);
						break;
					case CustomerContactType.Shipping:
						contact = DefLocationContact.SelectWindowed(0, 1);
						break;
				}
			}
			if (contact != null)
				row.Email = contact.EMail;
		}
		#endregion

		#region CustSalesPersons Events
		public virtual void CustSalesPeople_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			CustSalesPeople row = (CustSalesPeople)e.Row;
			if (row != null)
			{
				List<CustSalesPeople> current = new List<CustSalesPeople>();
				bool duplicated = false;
				foreach (CustSalesPeople iSP in this.SalesPersons.Select())
				{
					if (row.SalesPersonID == iSP.SalesPersonID)
					{
						current.Add(iSP);
						if (row.LocationID == iSP.LocationID)
							duplicated = true;
					}
				}
				if (duplicated)
				{
					LocationExtAddress freeLocation = null;
					foreach (LocationExtAddress iLoc in this.Locations.Select())
					{
						bool found = current.Exists(new Predicate<CustSalesPeople>(delegate(CustSalesPeople op) { return (op.LocationID == iLoc.LocationID); }));
						if (!found)
						{
							freeLocation = iLoc;
							break;
						}
					}
					if (freeLocation != null)
					{
						row.LocationID = freeLocation.LocationID;
					}
					else
					{
						throw new PXException(Messages.SalesPersonAddedForAllLocations);
					}
				}

				Dictionary<Int32?, short> locationspcount = new Dictionary<int?, short>();
				locationspcount[row.LocationID] = 0;

				foreach (CustSalesPeople iSP in this.SalesPersons.Select())
				{
					if (iSP.IsDefault == true)
					{
						short counter;
						if (locationspcount.TryGetValue(iSP.LocationID, out counter))
						{
							locationspcount[iSP.LocationID] = ++counter;
						}
						else
						{
							locationspcount[iSP.LocationID] = 1;
						}
					}
				}

				//InitNewRow is false for salespersons grid
				if (locationspcount[row.LocationID] == 0)
				{
					row.IsDefault = true;
				}
				else
				{
					CheckDoubleDefault(cache, row);
				}
			}
		}

		protected virtual void CheckDoubleDefault(PXCache sender, CustSalesPeople row)
		{
			if (row != null)
			{
				bool refresh = false;
				foreach (CustSalesPeople iSP in this.SalesPersons.Select())
				{
					if (row.LocationID == iSP.LocationID && row.SalesPersonID != iSP.SalesPersonID && row.IsDefault == true)
					{
						iSP.IsDefault = false;
						SalesPersons.Cache.Update(iSP);
						refresh = true;
					}
				}

				if (refresh)
				{
					this.SalesPersons.View.RequestRefresh();
				}
			}
		}

		protected virtual void CustSalesPeople_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			CheckDoubleDefault(sender, (CustSalesPeople)e.NewRow);
		}

		public virtual void CustSalesPeople_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CustSalesPeople row = (CustSalesPeople)e.Row;
			if (row != null)
			{
				bool multipleLocations = false;
				int count = 0;
				foreach (LocationExtAddress iLoc in this.Locations.Select())
				{
					if (count > 0)
					{
						multipleLocations = true;
						break;
					}
					count++;
				}

				PXUIFieldAttribute.SetEnabled<CustSalesPeople.locationID>(this.SalesPersons.Cache, row, ((!row.LocationID.HasValue) || multipleLocations));
			}
		}

		#endregion

		#region Default Payment Method Events
		
		protected virtual void CustomerPaymentMethod_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{			
			CustomerPaymentMethod row = (CustomerPaymentMethod)e.Row;
            PXUIFieldAttribute.SetEnabled(cache, null, false);
			if (row != null)
			{
				Customer acct = this.BAccount.Current;
                //row.IsDefault = (acct != null) && 
                //    acct.DefPaymentMethodID == row.PaymentMethodID &&
                //        (acct.DefPMInstanceID.HasValue ==false || acct.DefPMInstanceID == row.PMInstanceID);
    			if (acct != null && acct.DefPMInstanceID == row.PMInstanceID)
				{
					PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.descr>(cache, row, false);
					PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.cashAccountID>(cache, row, true);
					bool isInserted = (cache.GetStatus(e.Row) == PXEntryStatus.Inserted);
					//PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.paymentMethodID>(this.DefPaymentMethodInstance.Cache, e.Row, (isInserted || String.IsNullOrEmpty(row.PaymentMethodID)));
					if (!String.IsNullOrEmpty(row.PaymentMethodID))
					{

						PaymentMethod pmDef = (PaymentMethod)this.PaymentMethodDef.Select();
						bool singleInstance = pmDef.ARIsOnePerCustomer ?? false;
						bool isIDMaskExists = false;
						if (!singleInstance)
						{

							foreach (PaymentMethodDetail iDef in this.PMDetails.Select(row.PaymentMethodID))
							{
								if ((iDef.IsIdentifier ?? false) && (!string.IsNullOrEmpty(iDef.DisplayMask)))
								{
									isIDMaskExists = true;
									break;
								}
							}
						}
						if (!(isIDMaskExists || singleInstance))
						{
							PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.descr>(cache, row, true);
						}
					}
					if (!isInserted && (!String.IsNullOrEmpty(row.PaymentMethodID)))
					{
						this.MergeDetailsWithDefinition(row);

						CCProcTran ccTran = PXSelect<CCProcTran, Where<CCProcTran.pMInstanceID, Equal<Required<CCProcTran.pMInstanceID>>,
									And<CCProcTran.procStatus, Equal<CCProcStatus.finalized>,
									And<CCProcTran.tranStatus, Equal<CCTranStatusCode.approved>>>>>.Select(this, row.PMInstanceID);
						bool hasTransactions = (ccTran != null);
						this.DefPaymentMethodInstanceDetails.Cache.AllowDelete = !hasTransactions;
						PXUIFieldAttribute.SetEnabled(this.DefPaymentMethodInstanceDetails.Cache, null, !hasTransactions);
					}

					if (row.CashAccountID.HasValue)
					{
						PaymentMethodAccount pmAcct = PXSelect<PaymentMethodAccount, Where<PaymentMethodAccount.cashAccountID, Equal<Required<PaymentMethodAccount.cashAccountID>>,
															And<PaymentMethodAccount.paymentMethodID, Equal<Required<PaymentMethodAccount.paymentMethodID>>,
															And<PaymentMethodAccount.useForAR, Equal<True>>>>>.Select(this, row.CashAccountID, row.PaymentMethodID);
						PXUIFieldAttribute.SetWarning<CustomerPaymentMethod.cashAccountID>(cache, e.Row, pmAcct == null ? string.Format(Messages.CashAccountIsNotConfiguredForPaymentMethodInAR, row.PaymentMethodID) : null);
					}
				}
			}
		}
		
		protected virtual void CustomerPaymentMethod_Descr_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			CustomerPaymentMethod row = (CustomerPaymentMethod)e.Row;
			PaymentMethod pmDef = this.PaymentMethodDef.Select(row.PaymentMethodID);
			if (pmDef != null && (pmDef.ARIsOnePerCustomer ?? false))
			{
				row.Descr = pmDef.Descr;
			}
		}

		protected virtual void CustomerPaymentMethod_Descr_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CustomerPaymentMethod row = (CustomerPaymentMethod)e.Row;
			PaymentMethod def = this.PaymentMethodDef.Select(row.PaymentMethodID);
			if (!(def.ARIsOnePerCustomer ?? false))
			{
				CustomerPaymentMethod existing = PXSelect<CustomerPaymentMethod,
				Where<CustomerPaymentMethod.bAccountID, Equal<Required<CustomerPaymentMethod.bAccountID>>,
				And<CustomerPaymentMethod.paymentMethodID, Equal<Required<CustomerPaymentMethod.paymentMethodID>>,
				And<CustomerPaymentMethod.pMInstanceID, NotEqual<Required<CustomerPaymentMethod.pMInstanceID>>,
				And<CustomerPaymentMethod.descr, Equal<Required<CustomerPaymentMethod.descr>>>>>>>.Select(this, row.BAccountID, row.PaymentMethodID, row.PMInstanceID, row.Descr);
				if (existing != null)
				{
					cache.RaiseExceptionHandling<CustomerPaymentMethod.descr>(row, row.Descr, new PXSetPropertyException(Messages.CustomerPMInstanceHasDuplicatedDescription, PXErrorLevel.Warning));
				}
			}
		}
		#region PM Details Events

		protected virtual void CustomerPaymentMethodDetail_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CustomerPaymentMethodDetail row = (CustomerPaymentMethodDetail)e.Row;
			if (row != null)
			{
				PaymentMethodDetail iTempl = this.FindTemplate(row);
				bool isRequired = (iTempl != null) && (iTempl.IsRequired ?? false);
				PXDefaultAttribute.SetPersistingCheck<CustomerPaymentMethodDetail.value>(cache, row, (isRequired) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				bool isDecrypted = (iTempl != null) && ((iTempl.IsEncrypted ?? false) == false);
				PXRSACryptStringAttribute.SetDecrypted<CustomerPaymentMethodDetail.value>(cache, row, isDecrypted);
				if (iTempl != null && iTempl.IsExpirationDate == true)
				{
					PXUIFieldAttribute.SetEnabled<CustomerPaymentMethodDetail.value>(cache, row, true);
				}
			}
		}

		protected virtual void CustomerPaymentMethodDetail_Value_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CustomerPaymentMethodDetail row = e.Row as CustomerPaymentMethodDetail;
			PaymentMethodDetail def = this.FindTemplate(row);
			if (def != null)
			{
				if (def.IsIdentifier ?? false)
				{
					string id = CustomerPaymentMethodMaint.IDObfuscator.MaskID(row.Value, def.EntryMask, def.DisplayMask);
					if (this.DefPaymentMethodInstance.Current.Descr != id)
					{
						CustomerPaymentMethod parent = this.DefPaymentMethodInstance.Current;
						parent.Descr = String.Format("{0}:{1}", parent.PaymentMethodID, id);
						this.DefPaymentMethodInstance.Update(parent);
					}
				}
				if (def.IsExpirationDate ?? false)
				{
					CustomerPaymentMethod parent = this.DefPaymentMethodInstance.Current;
					try
					{
						parent.ExpirationDate = CustomerPaymentMethodMaint.ParseExpiryDate(row.Value);
					}
					catch (FormatException)
					{
						parent.ExpirationDate = null;
					}
					this.DefPaymentMethodInstance.Update(parent);
				}
			}
		}

		protected virtual void CustomerPaymentMethodDetail_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			CustomerPaymentMethodDetail row = (CustomerPaymentMethodDetail)e.Row;
			if (this.DefPaymentMethodInstance.Current != null)
			{
				PaymentMethodDetail def = this.FindTemplate(row);
				if (def != null && (def.IsIdentifier ?? false))
				{
					this.DefPaymentMethodInstance.Current.Descr = null;
				}
			}
		}
		#endregion
		#endregion

		#region CustomerPaymentMethodInfo
        
		protected virtual void CustomerPaymentMethodInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CustomerPaymentMethodInfo row = e.Row as CustomerPaymentMethodInfo;
			if (row == null) return;

			Customer cust = CurrentCustomer.Current as Customer;
			if (cust == null) return;

			row.IsDefault = row.PMInstanceID == cust.DefPMInstanceID;
		}



		protected virtual void CustomerPaymentMethodInfo_IsDefault_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			CustomerPaymentMethodInfo row = e.Row as CustomerPaymentMethodInfo;
			if (row == null) return;
			Customer cust = BAccount.Current as Customer;
			cust.DefPMInstanceID = (bool?) e.NewValue == false ? null : row.PMInstanceID;
			BAccount.Update(cust);
			this.CurrentCustomer.Cache.RaiseRowSelected(cust);
			PaymentMethods.View.RequestRefresh();
		}

		protected virtual void CustomerPaymentMethodInfo_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		#endregion
        #region Internal Fuctions
        protected virtual bool AllowChangeAccounts()
		{
			return true; //Add actual condition here 
		}
		protected override void InitDefAddress(Address aAddress)
		{
			base.InitDefAddress(aAddress);
			CustomerClass customerClass = this.CustomerClass.Current;
			if (customerClass != null && customerClass.CountryID != null)
			{
				aAddress.CountryID = customerClass.CountryID;
			}
		}
		protected override void SetAsDefault(ContactExtAddress row)
		{
			Customer account = this.BAccount.Current;
			if (account != null)
			{
				if (account.DefBillContactID.HasValue && (account.DefBillContactID == account.DefContactID))
				{
					int? defContactID = (row != null) ? row.ContactID : null;
					account.DefBillContactID = defContactID;
				}
			}
			base.SetAsDefault(row);
		}

		protected virtual void CreateDefPaymentMethod(Customer account)
		{			
            PaymentMethod defaultPM = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, account.DefPaymentMethodID);
            if (account.DefPMInstanceID == null && defaultPM != null && defaultPM.IsAccountNumberRequired == true)
            {
                CustomerPaymentMethod pmInstance = new CustomerPaymentMethod();
				pmInstance = this.DefPaymentMethodInstance.Insert(pmInstance);
                if (pmInstance.BAccountID == null)
                {
                    pmInstance.BAccountID = account.BAccountID;
                }
                account.DefPMInstanceID = pmInstance.PMInstanceID;
                if (this.AddPMDetails() && (!defaultPM.ARIsOnePerCustomer ?? false))
                {
                    this.DefPaymentMethodInstance.Current.Descr = account.DefPaymentMethodID;
                }
            }
		}

		protected virtual bool AddPMDetails()
		{
			string pmID = this.DefPaymentMethodInstance.Current.PaymentMethodID;
            bool setAccountNo = true;
            if (!String.IsNullOrEmpty(pmID))
			{
				foreach (PaymentMethodDetail it in this.PMDetails.Select())
				{
					CustomerPaymentMethodDetail det = new CustomerPaymentMethodDetail();
                    if (it.IsIdentifier ?? false) setAccountNo = false;
                    det.DetailID = it.DetailID;
					det = this.DefPaymentMethodInstanceDetails.Insert(det);
				}
			}
            return setAccountNo;
		}
		protected virtual void ClearPMDetails()
		{
			foreach (CustomerPaymentMethodDetail iDet in this.DefPaymentMethodInstanceDetailsAll.Select())
			{
				this.DefPaymentMethodInstanceDetails.Delete(iDet);
			}
		}
		protected virtual PaymentMethodDetail FindTemplate(CustomerPaymentMethodDetail aDet)
		{
			PaymentMethodDetail res = PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
				And<PaymentMethodDetail.detailID, Equal<Required<PaymentMethodDetail.detailID>>,
                And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>.Select(this, aDet.PaymentMethodID, aDet.DetailID);
			return res;
		}
		protected virtual void MergeDetailsWithDefinition(CustomerPaymentMethod aRow)
		{
			string paymentMethodID = aRow.PaymentMethodID;
			if (aRow.PMInstanceID != this.mergedPMInstance)
			{
				List<PaymentMethodDetail> toAdd = new List<PaymentMethodDetail>();
				List<CustomerPaymentMethodDetail> toDelete = new List<CustomerPaymentMethodDetail>();
				foreach (PaymentMethodDetail it in this.PMDetails.Select(paymentMethodID))
				{
					CustomerPaymentMethodDetail detail = null;
					foreach (CustomerPaymentMethodDetail iPDet in this.DefPaymentMethodInstanceDetailsAll.Select())
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
				//foreach (PXResult<CustomerPaymentMethodDetail, PaymentMethodDetail> iCPM in this.DefPaymentMethodInstanceDetails.Select()) 
				//{
				//    if (string.IsNullOrEmpty(((PaymentMethodDetail)iCPM).PaymentMethodID))
				//        toDelete.Add((CustomerPaymentMethodDetail)iCPM);
				//}
				using (ReadOnlyScope rs = new ReadOnlyScope(this.DefPaymentMethodInstanceDetails.Cache))
				{
					foreach (PaymentMethodDetail it in toAdd)
					{
						CustomerPaymentMethodDetail detail = new CustomerPaymentMethodDetail();
						detail.DetailID = it.DetailID;
						detail = this.DefPaymentMethodInstanceDetails.Insert(detail);
					}

					if (toAdd.Count > 0 || toDelete.Count > 0)
					{
						this.DefPaymentMethodInstanceDetails.View.RequestRefresh();
					}
				}
				//foreach (CustomerPaymentMethodDetail iDel in toDelete)
				//{
				//    this.DefPaymentMethodInstanceDetails.Delete(iDel);
				//}				
				this.mergedPMInstance = aRow.PMInstanceID;
			}
		}

		#endregion

		#region Utility Functions
		public virtual void InitCustomerLocation(Location aLoc, string aLocationType)
		{
			this.IntLocations.Cache.SetDefaultExt<Location.cCarrierID>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.cFOBPointID>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.cResedential>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.cSaturdayDelivery>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.cLeadTime>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.cShipComplete>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.cShipTermsID>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.cBranchID>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.cTaxZoneID>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.cSalesSubID>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.cSalesAcctID>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.cSalesSubID>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.cARAccountID>(aLoc);
			this.IntLocations.Cache.SetDefaultExt<Location.cARSubID>(aLoc);

			aLoc.LocType = aLocationType;
		}

		public virtual void InitCustomerLocation(LocationExtAddress aLoc, string aLocationType)
		{
			this.DefLocation.Cache.SetDefaultExt<Location.cCarrierID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.cFOBPointID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.cResedential>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.cSaturdayDelivery>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.cLeadTime>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.cShipComplete>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.cShipTermsID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.cBranchID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.cTaxZoneID>(aLoc);
            this.DefLocation.Cache.SetDefaultExt<Location.cDiscountAcctID>(aLoc);
            this.DefLocation.Cache.SetDefaultExt<Location.cDiscountSubID>(aLoc);
            this.DefLocation.Cache.SetDefaultExt<Location.cFreightAcctID>(aLoc);
            this.DefLocation.Cache.SetDefaultExt<Location.cFreightSubID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.cSalesAcctID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.cSalesSubID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.cARAccountID>(aLoc);
			this.DefLocation.Cache.SetDefaultExt<Location.cARSubID>(aLoc);

			aLoc.LocType = aLocationType;
		}


		public virtual void CopyAccounts(PXCache cache, Customer row)
		{
			cache.SetDefaultExt<Customer.termsID>(row);
			cache.SetDefaultExt<Customer.curyID>(row);
			cache.SetDefaultExt<Customer.curyRateTypeID>(row);
			cache.SetDefaultExt<Customer.taxZoneID>(row);
			cache.SetDefaultExt<Customer.allowOverrideCury>(row);
			cache.SetDefaultExt<Customer.allowOverrideRate>(row);

			cache.SetDefaultExt<Customer.discTakenAcctID>(row);
			cache.SetDefaultExt<Customer.discTakenSubID>(row);

			if (this.DefLocation.Current != null &&
				 this.DefLocation.Cache.GetStatus(this.DefLocation.Current) == PXEntryStatus.Notchanged)
			{
				this.DefLocation.Cache.SetStatus(this.DefLocation.Current, PXEntryStatus.Updated);
			}

			if (CustomerClass.Current == null)
			{ 
				CustomerClass.Current = CustomerClass.Select(row.CustomerClassID);
			}

			if (CustomerClass.Current != null)
			{			
				row.GroupMask = CustomerClass.Current.GroupMask;
			}

			cache.SetDefaultExt<Customer.cOGSAcctID>(row);
			cache.SetDefaultExt<Customer.cOGSSubID>(row);

			cache.SetDefaultExt<Customer.smallBalanceAllow>(row);
			cache.SetDefaultExt<Customer.smallBalanceLimit>(row);
			cache.SetDefaultExt<Customer.autoApplyPayments>(row);
			cache.SetDefaultExt<Customer.printStatements>(row);
			cache.SetDefaultExt<Customer.printCuryStatements>(row);
			cache.SetDefaultExt<Customer.sendStatementByEmail>(row);

			cache.SetDefaultExt<Customer.creditLimit>(row);
			cache.SetDefaultExt<Customer.creditRule>(row);
			cache.SetDefaultExt<Customer.creditDaysPastDue>(row);
			cache.SetDefaultExt<Customer.statementCycleId>(row);
			cache.SetDefaultExt<Customer.statementType>(row);

			cache.SetDefaultExt<Customer.finChargeApply>(row);
			cache.SetDefaultExt<Customer.printInvoices>(row);
			cache.SetDefaultExt<Customer.mailInvoices>(row);

			cache.SetDefaultExt<Customer.prepaymentAcctID>(row);
			cache.SetDefaultExt<Customer.prepaymentSubID>(row);
		}
		#endregion
	
		#region Private members
		protected Address formBillAddress;
		private bool doCopyClassSettings;
		private int? mergedPMInstance = null;
        
		private bool isTokenizedPaymentMethod(int? PMInstanceID)
		{
			CustomerPaymentMethod cpm = PXSelect<CustomerPaymentMethod, Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>.Select(this, PMInstanceID);
			if (cpm != null )
			{
				CCProcessingCenter processingCenter = PXSelect<CCProcessingCenter,
					Where<CCProcessingCenter.processingCenterID, Equal<Required<CCProcessingCenter.processingCenterID>>>>.Select(this, cpm.CCProcessingCenterID);
				return CCPaymentProcessing.IsFeatureSupported(processingCenter, CCProcessingFeature.Tokenization);
			}
			return false;
		}

		private bool isHFPaymentMethod
		{
			get
			{
				CCProcessingCenter processingCenter = PMProcessingCenter.SelectSingle();
				return CCPaymentProcessing.IsFeatureSupported(processingCenter, CCProcessingFeature.HostedForm);
			}
		}

		private bool isCCPIDFilled
		{
			get
			{
				CustomerPaymentMethodDetail id = ccpIdDet.Select();
				return id != null && !string.IsNullOrEmpty(id.Value);
			}
		}
        
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
				else if (CurrentCustomer.Cache.GetStatus(CurrentCustomer.Current) == PXEntryStatus.Inserted)
				{
					CurrentCustomer.Current.DefBillAddressID = DetailInserted<Address>(cache, (Address)e.Row, null);
					CurrentCustomer.Current.IsBillSameAsMain = false;
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
				else if (CurrentCustomer.Cache.GetStatus(CurrentCustomer.Current) == PXEntryStatus.Inserted)
				{
					CurrentCustomer.Current.DefBillContactID = DetailInserted<Contact>(cache, (Contact)e.Row, null);
					CurrentCustomer.Current.IsBillContSameAsMain = false;
				}
			}
		}

		#endregion
	}
}



