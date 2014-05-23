using System;
using PX.Data;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System.Web;
using System.IO;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.SM;

//OBSOLETE
#if false
namespace PX.Objects.CS
{
	#region DAC Class Override 

	[SerializableAttribute]
	[PXSubstitute(GraphType = typeof(CompanySetup))]
	public class MaintCompany : Company
	{
		#region BAccountID
		public new abstract class bAccountID : PX.Data.IBqlField
		{
		}
		[PXDBInt()]
		[PXUIField(Visible = true, Enabled = false)]
		[PXDBLiteDefault(typeof(BAccount.bAccountID))]
		public override Int32? BAccountID
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
	}

	#endregion

    public class CompanySetup : BusinessAccountGraphBase<BAccount, Company, Where<BAccount.type, Equal<BAccountType.companyType>>> 
	{
		#region InternalTypes
		[PXProjection(typeof(Select2<Company,
			LeftJoin<BAccount, On<Company.bAccountID, Equal<BAccount.bAccountID>>>, Where<boolTrue, Equal<boolTrue>>>))]
        [Serializable]
		public partial class CompanyBAccount : BAccount
		{
			#region CompanyCompanyCD
			public abstract class companyCompanyCD : PX.Data.IBqlField
			{
			}
			protected string _CompanyCompanyCD;
			[PXDBString(30, IsUnicode = true, BqlField = typeof(Company.companyCD))]
			[PXDefault()]
			[PXDimension("BIZACCT")]
			[PXUIField(DisplayName = "Company", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String CompanyCompanyCD
			{
				get
				{
					return this._CompanyCompanyCD;
				}
				set
				{
					this._CompanyCompanyCD = value;
				}
			}
			#endregion
			#region CompanyBaseCuryID
			public abstract class companyBaseCuryID : PX.Data.IBqlField
			{
			}
			protected String _CompanyBaseCuryID;
			[PXDBString(5, IsUnicode = true, BqlField = typeof(Company.baseCuryID), InputMask = ">LLLLL")]
			[PXDefault()]
			[PXUIField(DisplayName = "Base Currency ID")]
			[PXSelector(typeof(Search<Currency.curyID>))]
			public virtual String CompanyBaseCuryID
			{
				get
				{
					return this._CompanyBaseCuryID;
				}
				set
				{
					this._CompanyBaseCuryID = value;
				}
			}
			#endregion
			#region CompanyBAccountID
			public abstract class companyBAccountID : PX.Data.IBqlField
			{
			}
			protected Int32? _CompanyBAccountID;
			[PXDBInt(BqlField = typeof(Company.bAccountID))]
			[PXUIField(Visible = true, Enabled = false)]
			public virtual Int32? CompanyBAccountID
			{
				get
				{
					return this._CompanyBAccountID;
				}
				set
				{
					this._CompanyBAccountID = value;
				}
			}
			#endregion
			#region AcctCD
			public new abstract class acctCD : PX.Data.IBqlField
			{
			}
			[PXDimension("BIZACCT", ValidComboRequired = false)]
			[PXDBString(30, IsUnicode = true)]
			[PXDefault()]
			[PXUIField(DisplayName = "Account ID", Visibility = PXUIVisibility.SelectorVisible)]
			public override String AcctCD
			{
				get
				{
					return base._AcctCD;
				}
				set
				{
					base._AcctCD = value;
				}
			}
			#endregion
			#region AcctName
			public new abstract class acctName : PX.Data.IBqlField
			{
			}
			
			[PXDBString(60, IsUnicode = true)]
			[PXUIField(DisplayName = "Company Name", Visibility = PXUIVisibility.Invisible)]
			public override String AcctName
			{
				get
				{
					return "Company Name";
				}
				set
				{
				}
			}
			#endregion
			#region CompanyPhoneMask
			public abstract class companyPhoneMask : PX.Data.IBqlField
			{
			}
			protected String _CompanyPhoneMask;
			[PXDBString(50, BqlField = typeof(Company.phoneMask))]
			[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Phone Mask")]
			public virtual String CompanyPhoneMask
			{
				get
				{
					return this._CompanyPhoneMask;
				}
				set
				{
					this._CompanyPhoneMask = value;
				}
			}
			#endregion
			#region CompanyCountryID
			public abstract class companyCountryID : PX.Data.IBqlField
			{
			}
			protected String _CompanyCountryID;
			[PXDBString(2, IsFixed = true, BqlField = typeof(Company.countryID))]
			[PXUIField(DisplayName = "Default Country")]
			[PXSelector(typeof(Country.countryID), DescriptionField = typeof(Country.description))]
			public virtual String CompanyCountryID
			{
				get
				{
					return this._CompanyCountryID;
				}
				set
				{
					this._CompanyCountryID = value;
				}
			}
			#endregion
		}

        #region Cache Attached Events

        #region Currency
        #region CuryID

        [PXDBString(5, IsUnicode = true, IsKey = true, InputMask = ">LLLLL")]
        [PXDefault()]
        [PXUIField(DisplayName = "Currency ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<CM.Currency.curyID>), CacheGlobal = true)]
        protected virtual void Currency_CuryID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region RealGainAcctID

        [PXDBInt()]
        [PXUIField(Required = false)]
        protected virtual void Currency_RealGainAcctID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region RealGainSubID

        [PXDBInt()]
        [PXUIField(Required = false)]
        protected virtual void Currency_RealGainSubID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region RealLossAcctID

        [PXDBInt()]
        [PXUIField(Required = false)]
        protected virtual void Currency_RealLossAcctID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region RealLossSubID

        [PXDBInt()]
        [PXUIField(Required = false)]
        protected virtual void Currency_RealLossSubID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region RevalGainAcctID

        [PXDBInt()]
        [PXUIField(Required = false)]
        protected virtual void Currency_RevalGainAcctID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region RevalGainSubID

        [PXDBInt()]
        [PXUIField(Required = false)]
        protected virtual void Currency_RevalGainSubID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region RevalLossAcctID

        [PXDBInt()]
        [PXUIField(Required = false)]
        protected virtual void Currency_RevalLossAcctID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region RevalLossSubID

        [PXDBInt()]
        [PXUIField(Required = false)]
        protected virtual void Currency_RevalLossSubID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region TranslationGainAcctID

        [PXDBInt()]
        [PXUIField(Required = false)]
        protected virtual void Currency_TranslationGainAcctID_CacheAttached(PXCache sender)
        {
        }

        #endregion
        #region TranslationGainSubID

        [PXDBInt()]
        [PXUIField(Required = false)]
        protected virtual void Currency_TranslationGainSubID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region TranslationLossAcctID

        [PXDBInt()]
        [PXUIField(Required = false)]
        protected virtual void Currency_TranslationLossAcctID_CacheAttached(PXCache sender)
        {
        }

        #endregion
        #region TranslationLossSubID

        [PXDBInt()]
        [PXUIField(Required = false)]
        protected virtual void Currency_TranslationLossSubID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region UnrealizedGainAcctID

        [PXDBInt()]
        [PXUIField(Required = false)]
        protected virtual void Currency_UnrealizedGainAcctID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region UnrealizedGainSubID

        [PXDBInt()]
        [PXUIField(Required = false)]
        protected virtual void Currency_UnrealizedGainSubID_CacheAttached(PXCache sender)
        {
        }

        #endregion
        #region UnrealizedLossAcctID

        [PXUIField(Required = false)]
        [PXDBInt()]
        protected virtual void Currency_UnrealizedLossAcctID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region UnrealizedLossSubID

        [PXDBInt()]
        [PXUIField(Required = false)]
        protected virtual void Currency_UnrealizedLossSubID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region RoundingGainAcctID

        [PXDBInt()]
        [PXUIField(Required = false)]
        protected virtual void Currency_RoundingGainAcctID_CacheAttached(PXCache sender)
        {
        }

        #endregion
        #region RoundingGainSubID

        [PXDBInt()]
        [PXUIField(Required = false)]
        protected virtual void Currency_RoundingGainSubID_CacheAttached(PXCache sender)
        {
        }

        #endregion
        #region RoundingLossAcctID

        [PXDBInt()]
        [PXUIField(Required = false)]
        protected virtual void Currency_RoundingLossAcctID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region RoundingLossSubID

        [PXDBInt()]
        [PXUIField(Required = false)]
        protected virtual void Currency_RoundingLossSubID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion
        #endregion
		#endregion

		#region CTor + Public members
		public CompanySetup()
		{
			this.CompanyHeader.Cache.AllowInsert = false;
			this.CompanyHeader.Cache.AllowDelete = false;
			this.Views.Caches.Remove(typeof(BAccount));
			this.Views.Caches.Insert(0, typeof(BAccount));
			this.Views.Caches.Remove(typeof(CompanyBAccount));
			this.Views.Caches.Insert(0, typeof(CompanyBAccount));
			this.Employees.Cache.AllowInsert = false;
			this.Employees.Cache.AllowDelete = false;
			this.Employees.Cache.AllowUpdate = false;
		}

		[PXHidden]
		public PXSelect<Contact>
			BaseContacts;

		public new PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Current<CompanyBAccount.bAccountID>>>> BAccount;
		public PXSelect<CompanyBAccount> CompanyHeader;

		public PXSelect<Company> Company;
		public PXSelect<Currency, Where<Currency.curyID, Equal<Current<CompanyBAccount.companyBaseCuryID>>>> CompanyCurrency;
		public PXSelectJoin<EPEmployee, InnerJoin<Contact, On<Contact.contactID, Equal<EPEmployee.defContactID>>,
							LeftJoin<Address, On<Address.addressID, Equal<EPEmployee.defAddressID>,
							And<Address.bAccountID, Equal<EPEmployee.parentBAccountID>>>>>,
							Where<EPEmployee.parentBAccountID, Equal<Current<Company.bAccountID>>>> Employees;
		public PXSelect<NoteDoc, Where<NoteDoc.noteID, Equal<Current<CompanyBAccount.noteID>>>> Notedocs;
		public PXSelect<PX.SM.UploadFile> Files;


		#region Buttons
		public new PXSaveCancel<CompanyBAccount> Save;
		public PXCancel<CompanyBAccount> Cancel1;
		public new PXAction<CompanyBAccount> viewContact;
		public new PXAction<CompanyBAccount> newContact;
		public new PXAction<CompanyBAccount> viewLocation;
		public new PXAction<CompanyBAccount> newLocation;
		public new PXAction<CompanyBAccount> setDefault;
		

		public new PXAction<CompanyBAccount> viewMainOnMap;
		public new PXAction<CompanyBAccount> viewDefLocationOnMap;
		
		

		#region Button Delegates

		[PXUIField(DisplayName = CS.Messages.ViewEmployee, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		public new IEnumerable ViewContact(PXAdapter adapter)
		{
			if (this.Employees.Current != null && this.BAccountAccessor.Cache.GetStatus(this.BAccountAccessor.Current) != PXEntryStatus.Inserted)
			{
				EPEmployee current = this.Employees.Current;
				EmployeeMaint graph = PXGraph.CreateInstance<EmployeeMaint>();
				graph.Employee.Current = graph.Employee.Search<EPEmployee.bAccountID>(current.BAccountID);
				throw new PXRedirectRequiredException(graph, CR.Messages.ContactMaint);
			}
			return adapter.Get();
		}


		[PXUIField(DisplayName = CS.Messages.NewEmployee)] 
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public new IEnumerable NewContact(PXAdapter adapter)
		{
			if (this.BAccountAccessor.Cache.GetStatus(this.BAccountAccessor.Current) != PXEntryStatus.Inserted)
			{
				EmployeeMaint graph = PXGraph.CreateInstance<EmployeeMaint>();
				EPEmployee cont = new EPEmployee();
				cont.RouteEmails = true;
				cont.ParentBAccountID= this.BAccountAccessor.Current.BAccountID;
				cont = (EPEmployee)graph.Employee.Insert(cont);
				throw new PXRedirectRequiredException(graph, CR.Messages.ContactMaint);
			}
			return adapter.Get();
		}


		[PXUIField(DisplayName = CR.Messages.ViewLocation, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		public new IEnumerable ViewLocation(PXAdapter adapter)
		{
			if (this.Locations.Current != null && this.BAccountAccessor.Cache.GetStatus(this.BAccountAccessor.Current) != PXEntryStatus.Inserted)
			{
				LocationExtAddress current = this.Locations.Current;
				CompanyLocationMaint graph = PXGraph.CreateInstance<CompanyLocationMaint>();
				graph.Location.Current = graph.Location.Search<Location.locationID>(current.LocationID, this.BAccountAccessor.Current.AcctCD);
				throw new PXRedirectRequiredException(graph, CR.Messages.LocationMaint);
			}
			return adapter.Get();
		}


		[PXUIField(DisplayName = CR.Messages.NewLocation)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public new IEnumerable NewLocation(PXAdapter adapter)
		{
			if (this.BAccountAccessor.Cache.GetStatus(this.BAccountAccessor.Current) != PXEntryStatus.Inserted)
			{
				CompanyLocationMaint graph = PXGraph.CreateInstance<CompanyLocationMaint>();
				SelectedLocation loc = new SelectedLocation();
				loc.BAccountID = this.BAccountAccessor.Current.BAccountID;
				loc = (SelectedLocation)graph.Location.Insert(loc);
				throw new PXRedirectRequiredException(graph, Messages.CompanyLocationMaint);
			}
			return adapter.Get();
		}


		[PXUIField(DisplayName = CR.Messages.SetDefault, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton()]
		public new IEnumerable SetDefault(PXAdapter adapter)
		{
			CompanyBAccount acct = this.CompanyHeader.Current;
			if (Locations.Current != null && acct != null && Locations.Current.LocationID != acct.DefLocationID)
			{
				acct.DefLocationID = Locations.Current.LocationID;
				this.CompanyHeader.Update(acct);
			}
			return adapter.Get();
		}
		
		#endregion

		#endregion
		#endregion

        #region Events Handlers
		
		[PXUIField(DisplayName = "Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[ContactDisplayName(typeof(Contact.lastName), typeof(Contact.firstName), typeof(Contact.midName), typeof(Contact.title), true)]
		protected virtual void Contact_DisplayName_CacheAttached(PXCache sender)
		{
			
		}

		protected virtual void Company_BaseCuryID_ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e)
		{
			e.Cancel = CompanyHeader.Cache.RaiseExceptionHandling<CompanyBAccount.companyBaseCuryID>(CompanyHeader.Current, e.NewValue, e.Exception);
		}

        protected virtual void CompanyBAccount_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CompanyBAccount row = e.Row as CompanyBAccount;
			if (row != null)
			{
				if (row.BAccountID == null)
				{
					BAccount acct = BAccount.Insert(row);
					row.CompanyBAccountID = acct.BAccountID;
					PXCache<BAccount>.RestoreCopy(row, acct);
					CompanyHeader.Update(row);
				}
				else if (row.BAccountID > 0)
				{
					BAccount.Current = PXCache<BAccount>.CreateCopy(row);
					PXUIFieldAttribute.SetEnabled<CompanyBAccount.acctCD>(sender, row, false);
				}
				bool allowCurrencyChange = false;
				if (row.CompanyBaseCuryID == null)
				{
					Currency b = new Currency();
					b.CuryID = String.Empty;
					b = this.CompanyCurrency.Insert(b);
					if (b != null)
						row.CompanyBaseCuryID = b.CuryID;
					allowCurrencyChange = true;
				}
				else
				{
					allowCurrencyChange = AllowChangeCurrency();
				}
				PXUIFieldAttribute.SetEnabled<CompanyBAccount.companyBaseCuryID>(sender, row, allowCurrencyChange);

			}
		}
		protected virtual void CompanyBAccount_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			CompanyBAccount header = e.Row as CompanyBAccount;
			BAccount acct = BAccount.Select();
			if (acct != null && header != null)
			{
				PXCache<BAccount>.RestoreCopy(acct, header);
				BAccount.Cache.Normalize();
				if (BAccount.Cache.GetStatus(acct) == PXEntryStatus.Notchanged)
				{
					BAccount.Cache.SetStatus(acct, PXEntryStatus.Updated);
				}
			}
		}

		protected virtual void CompanyBAccount_AcctCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CompanyBAccount row = e.Row as CompanyBAccount;
			if (row != null)
			{
				if (row.BAccountID != null && row.BAccountID > 0)
				{
					throw new PXSetPropertyException(Messages.FieldReadOnly);
				}
				else if (e.NewValue != null)
				{
					BAccount acct = PXSelectReadonly<BAccount, Where<BAccount.acctCD, Equal<Required<BAccount.acctCD>>>>.Select(this, e.NewValue);
					if (acct != null && acct.BAccountID != row.BAccountID)
					{
						throw new PXSetPropertyException(Messages.ItemExistsReenter);
					}
				}
			}
		}

        protected virtual void CompanyBAccount_CompanyBaseCuryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!String.IsNullOrEmpty((string)e.NewValue) && PXSelectorAttribute.Select<CompanyBAccount.companyBaseCuryID>(sender, e.Row, e.NewValue) == null)
			{
				foreach (Currency bc in this.CompanyCurrency.Cache.Inserted)
				{
					bc.CuryID = (string)e.NewValue;
					CompanyCurrency.Cache.Normalize();
					e.Cancel = true;
					break;
				}
			}
		}

		protected virtual void CompanyBAccount_CompanyBaseCuryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CompanyCurrency.View.RequestRefresh();
		}

		public override int Persist(Type cacheType, PXDBOperation operation)
		{
			int res = base.Persist(cacheType, operation);
			if (cacheType == typeof(CompanyBAccount) && operation == PXDBOperation.Update)
			{
				foreach (PXResult<NoteDoc, UploadFile> rec in PXSelectJoin<NoteDoc, InnerJoin<UploadFile, On<NoteDoc.fileID, Equal<UploadFile.fileID>>>,
														Where<NoteDoc.noteID, Equal<Current<CompanyBAccount.noteID>>>>.Select(this))
				{
					UploadFile file = (UploadFile)rec;
					if (file.IsPublic != true)
					{
						this.SelectTimeStamp();
						file.IsPublic = true;
						file = (UploadFile)this.Caches[typeof(UploadFile)].Update(file);
						this.Caches[typeof(UploadFile)].PersistUpdated(file);
					}
				}
			}
			return res;
		}
	
		protected virtual void BAccount_AcctCD_ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e)
		{
			e.Cancel = CompanyHeader.Cache.RaiseExceptionHandling<CompanyBAccount.acctCD>(CompanyHeader.Current, e.NewValue, e.Exception);
		}
		protected virtual void BAccount_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			BAccount acct = e.Row as BAccount;
			CompanyBAccount header = CompanyHeader.Current;
			if(acct != null && header != null)
			{
				PXCache<BAccount>.RestoreCopy(header, acct);
			}
		}
		protected virtual void BAccount_Type_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = BAccountType.CompanyType;
		}
	
		#endregion	

		#region Overrides

		public override PXSelectBase<BAccount> BAccountAccessor
		{
			get
			{
				return this.BAccount;
			}
		}

		public override void Persist()
		{
			CompanyBAccount header = CompanyHeader.Current;
			if (header != null && CompanyHeader.Cache.GetStatus(header) == PXEntryStatus.Updated)
			{
				Company company = Company.Select();
				if (company != null)
				{
					company.BAccountID = header.CompanyBAccountID;
					company.CompanyCD = header.AcctCD;
					company.BaseCuryID = header.CompanyBaseCuryID;
					company.CountryID = header.CompanyCountryID;
					company.PhoneMask = header.CompanyPhoneMask;
					Company.Update(company);
				}

			}
			base.Persist();
		}
		
		#endregion

		#region Auxillary Functions
		private bool AllowChangeCurrency()
		{
			GLSetup setup = PXSelect<GLSetup>.Select(this);
			return setup == null;
		} 
		#endregion
	}


}
#endif
