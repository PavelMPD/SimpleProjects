using System;
using System.Security.Permissions;
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
using UploadFile = PX.SM.UploadFile;

namespace PX.Objects.CS
{
    [Serializable]
	public class BranchMaint : BusinessAccountGraphBase<BranchMaint.BranchBAccount, BranchMaint.BranchBAccount, Where<True, Equal<True>>> 
	{
		#region InternalTypes
		[PXProjection(typeof(Select2<BAccount,
			InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccount.bAccountID>>>, Where<True, Equal<True>>>), Persistent = true)]
		[PXCacheName(Messages.Branch)]
        [Serializable]
		public partial class BranchBAccount : BAccount
		{
			public new abstract class bAccountID : IBqlField { }

			#region BranchBranchCD
			public abstract class branchBranchCD : PX.Data.IBqlField
			{
			}
			[PXDBString(30, IsUnicode = true, BqlField = typeof(Branch.branchCD))]
			[PXDimension("BIZACCT")]
			[PXUIField(DisplayName = "Branch", Visibility = PXUIVisibility.Invisible)]
			[PXExtraKey()]
			public virtual String BranchBranchCD
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
			#region BranchRoleName
			public abstract class branchRoleName : IBqlField { }
			private string _BranchRoleName;
			[PXDBString(64, IsUnicode = true, InputMask = "", BqlField = typeof(Branch.roleName))]
			[PXSelector(typeof(Search<PX.SM.Roles.rolename, Where<PX.SM.Roles.guest, Equal<False>>>), DescriptionField = typeof(PX.SM.Roles.descr))]
			[PXUIField(DisplayName = "Access Role")]
			public string BranchRoleName
			{
				get
				{
					return _BranchRoleName;
				}
				set
				{
					_BranchRoleName = value;
				}
			}
			#endregion
			#region BranchLogoName
			public abstract class branchLogoName : IBqlField { }

			[PXDBString(IsUnicode = true, InputMask = "", BqlField = typeof(Branch.logoName))]
			[PXUIField(DisplayName = "Logo")]
			public string BranchLogoName { get; set; }
			#endregion
			#region BranchLedgerID
			public abstract class ledgerID : PX.Data.IBqlField
			{
			}
			protected Int32? _LedgerID;
			[PXDBInt(BqlField = typeof(Branch.ledgerID))]
			[PXUIField(DisplayName = "Posting Ledger")]
			[PXSelector(typeof(Search<Ledger.ledgerID, Where<Ledger.balanceType, Equal<ActualLedger>>>), DescriptionField = typeof(Ledger.descr), SubstituteKey = typeof(Ledger.ledgerCD))]
			public virtual Int32? LedgerID
			{
				get
				{
					return this._LedgerID;
				}
				set
				{
					this._LedgerID = value;
				}
			}
			#endregion
			#region BranchBAccountID
			public abstract class branchBAccountID : PX.Data.IBqlField
			{
			}
			[PXDBInt(BqlField = typeof(Branch.bAccountID))]
			[PXUIField(Visible = true, Enabled = false)]
			[PXExtraKey()]
			public virtual Int32? BranchBAccountID
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
			#region BranchActive
			public abstract class active : IBqlField { }
			[PXDBBool(BqlField = typeof(Branch.active))]
			[PXUIField(DisplayName = "Active", FieldClass="BRANCH")]
			[PXDefault(true)]
			public bool? Active
			{
				get;
				set;
			}
			#endregion
			#region AcctCD
			public new abstract class acctCD : PX.Data.IBqlField
			{
			}
			[PXDimensionSelector("BIZACCT", typeof(Search2<BAccount.acctCD, InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccount.bAccountID>>>>), typeof(BAccount.acctCD))]
			[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
			[PXDefault()]
			[PXUIField(DisplayName = "Branch ID", Visibility = PXUIVisibility.SelectorVisible)]
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
			[PXDefault()]
			[PXUIField(DisplayName = "Branch Name", Visibility = PXUIVisibility.SelectorVisible)]
			public override String AcctName
			{
				get
				{
					return this._AcctName;
				}
				set
				{
					this._AcctName = value;
				}
			}
			#endregion
			#region Type
			public new abstract class type : PX.Data.IBqlField
			{
			}
			[PXDBString(2, IsFixed = true, BqlField=typeof(BAccount.type))]
			public override String Type
			{
				get
				{
					return this._Type;
				}
				set
				{
					this._Type = value;
				}
			}
			#endregion
			#region AcctReferenceNbr
			public new abstract class acctReferenceNbr : PX.Data.IBqlField
			{
			}
			[PXDBString(50, IsUnicode = true, BqlField = typeof(BAccount.acctReferenceNbr))]
			public override String AcctReferenceNbr
			{
				get
				{
					return this._AcctReferenceNbr;
				}
				set
				{
					this._AcctReferenceNbr = value;
				}
			}
			#endregion
			#region ParentBAccountID
			public new abstract class parentBAccountID : PX.Data.IBqlField
			{
			}
			[PXDBInt(BqlField = typeof(BAccount.parentBAccountID))]
			public override Int32? ParentBAccountID
			{
				get
				{
					return this._ParentBAccountID;
				}
				set
				{
					this._ParentBAccountID = value;
				}
			}
			#endregion
			#region OwnerID
			public new abstract class ownerID : IBqlField { }
			[PXDBGuid(BqlField = typeof(BAccount.ownerID))]
			public override Guid? OwnerID
			{
				get
				{
					return this._OwnerID;
				}
				set
				{
					this._OwnerID = value;
				}
			}
			#endregion
			#region BranchPhoneMask
			public abstract class branchPhoneMask : PX.Data.IBqlField
			{
			}
			protected String _BranchPhoneMask;
			[PXDBString(50, BqlField = typeof(Branch.phoneMask))]
			[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Phone Mask")]
			public virtual String BranchPhoneMask
			{
				get
				{
					return this._BranchPhoneMask;
				}
				set
				{
					this._BranchPhoneMask = value;
				}
			}
			#endregion
			#region BranchCountryID
			public abstract class branchCountryID : PX.Data.IBqlField
			{
			}
			protected String _BranchCountryID;
			[PXDBString(2, IsFixed = true, BqlField = typeof(Branch.countryID))]
			[PXUIField(DisplayName = "Default Country")]
			[PXSelector(typeof(Country.countryID), DescriptionField = typeof(Country.description))]
			public virtual String BranchCountryID
			{
				get
				{
					return this._BranchCountryID;
				}
				set
				{
					this._BranchCountryID = value;
				}
			}
			#endregion
			#region GroupMask
			public abstract class groupMask : IBqlField { }
			protected Byte[] _GroupMask;
			[SingleGroup(BqlTable = typeof(Branch))]
			public virtual Byte[] GroupMask
			{
				get
				{
					return this._GroupMask;
				}
				set
				{
					this._GroupMask = value;
				}
			}
			#endregion
			#region AcctMapNbr
			public abstract class acctMapNbr : PX.Data.IBqlField
			{
			}
			protected int? _AcctMapNbr;
			[PXDBInt(BqlTable = typeof(Branch))]
			[PXDefault(0)]
			public virtual int? AcctMapNbr
			{
				get
				{
					return this._AcctMapNbr;
				}
				set
				{
					this._AcctMapNbr = value;
				}
			}
            #endregion

            #region AllowsRUTROT
            public abstract class allowsRUTROT : IBqlField { }

            [PXDBBool(BqlField = typeof(Branch.allowsRUTROT))]
            [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(DisplayName = "Uses ROT & RUT deduction", FieldClass = AR.RUTROTMessages.FieldClass)]
            public virtual Boolean? AllowsRUTROT { get; set; }
            #endregion
            #region RUTROTDeductionPct
            public abstract class rUTROTDeductionPct : IBqlField { }

            [PXDBDecimal(BqlField = typeof(Branch.rUTROTDeductionPct), MinValue = 0, MaxValue = 100)]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Deduction,%", FieldClass = AR.RUTROTMessages.FieldClass)]
            public Decimal? RUTROTDeductionPct { get; set; }
            #endregion
            #region RUTROTPersonalAllowanceLimit
            public abstract class rUTROTPersonalAllowanceLimit : IBqlField { }

            [PXDBDecimal(BqlField = typeof(Branch.rUTROTPersonalAllowanceLimit), MinValue = 0, MaxValue = 100000000)]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Personal allowance limit", FieldClass = AR.RUTROTMessages.FieldClass)]
            public virtual Decimal? RUTROTPersonalAllowanceLimit { get; set; }
            #endregion
            #region RUTROTCuryID
            public abstract class rUTROTCuryID : PX.Data.IBqlField
            {
            }

            [PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(Branch.rUTROTCuryID))]
            [PXUIField(DisplayName = "Currency", FieldClass = AR.RUTROTMessages.FieldClass)]
            [PXSelector(typeof(Currency.curyID))]
            [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual String RUTROTCuryID { get; set; }
            #endregion
            #region RUTROTClaimNextRefNbr
            public abstract class rUTROTClaimNextRefNbr : PX.Data.IBqlField { }

            [PXDBInt(MinValue = 0, MaxValue = 100000000, BqlField = typeof(Branch.rUTROTClaimNextRefNbr))]
            [PXDefault(0)]
            [PXUIField(DisplayName = "Next Export File Ref Nbr", FieldClass = AR.RUTROTMessages.FieldClass)]
            public virtual int? RUTROTClaimNextRefNbr { get; set; }
            #endregion
            #region RUTROTOrgNbrValidRegEx
            public abstract class rUTROTOrgNbrValidRegEx : PX.Data.IBqlField
            {
            }

            [PXDBString(255, BqlField = typeof(Branch.rUTROTOrgNbrValidRegEx))]
            [PXUIField(DisplayName = "Org. Nbr. Validation Reg. Exp.", FieldClass = AR.RUTROTMessages.FieldClass)]
            public virtual String RUTROTOrgNbrValidRegEx { get; set; }
            #endregion
		}
		#endregion

		#region Buttons
		public new PXAction<BranchBAccount> validateAddresses;
		[PXUIField(DisplayName = Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton]
		public new virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			BAccount bacct = this.BAccount.Current;			
			if (bacct != null )
			{
				bool needSave = false;
				Save.Press();
				Address address = this.DefAddress.Current;
				if (address != null && address.IsValidated == false) 
				{					
					PXAddressValidator.Validate<Address>(this, address, true);
					needSave = true;					
				}
				LocationExtAddress locAddress = this.DefLocation.Current;
				if (locAddress != null && locAddress.IsValidated == false && locAddress.AddressID != address.AddressID)
				{
					PXAddressValidator.Validate<LocationExtAddress>(this, locAddress, true);
					needSave = true;
				}
				if(needSave == true)
					this.Save.Press();
				
			}
			return adapter.Get();
		}
		#endregion

		#region CTor + Public members
		public BranchMaint()
		{
			this.Employees.Cache.AllowInsert = false;
			this.Employees.Cache.AllowDelete = false;
			this.Employees.Cache.AllowUpdate = false;
			PXUIFieldAttribute.SetDisplayName(Caches[typeof(Contact)], typeof(Contact.salutation).Name, CR.Messages.Attention);

			PXUIFieldAttribute.SetEnabled<Contact.fullName>(Caches[typeof(Contact)], null);
			if (!PXAccess.FeatureInstalled<FeaturesSet.branch>())
			{
				BranchBAccount br = PXSelectReadonly<BranchBAccount>.SelectWindowed(this, 0, 1);
				this.CurrentBAccount.Cache.AllowInsert = (br == null);
				this.CurrentBAccount.Cache.AllowDelete = (br == null);
				Next.SetVisible(false);
				Prev.SetVisible(false);
				Last.SetVisible(false);
				First.SetVisible(false);
				Insert.SetVisible(false);
				Delete.SetVisible(false);
			}
		}

		public PXSelect<BranchBAccount, Where<BranchBAccount.bAccountID, Equal<Current<BranchBAccount.bAccountID>>>> CurrentBAccount;
		public PXSelectJoin<EPEmployee, InnerJoin<Contact, On<Contact.contactID, Equal<EPEmployee.defContactID>, And<Contact.bAccountID, Equal<EPEmployee.parentBAccountID>>>,
							LeftJoin<Address, On<Address.addressID, Equal<EPEmployee.defAddressID>,
							And<Address.bAccountID, Equal<EPEmployee.parentBAccountID>>>>>,
							Where<EPEmployee.parentBAccountID, Equal<Current<BranchBAccount.bAccountID>>>> Employees;

		public PXSelect<NoteDoc, Where<NoteDoc.noteID, Equal<Current<BranchBAccount.noteID>>>> Notedocs;
		public PXSelect<PX.SM.UploadFile> Files;

		public PXSelect<GLSetup> glsetup;
		public PXSelect<Company> Company;
		public PXSelect<Currency, Where<Currency.curyID, Equal<Current<Company.baseCuryID>>>> CompanyCurrency;


		#region Buttons

		public new PXAction<BranchBAccount> viewContact;
		public new PXAction<BranchBAccount> newContact;
		public new PXAction<BranchBAccount> viewLocation;
		public new PXAction<BranchBAccount> newLocation;
		public new PXAction<BranchBAccount> setDefault;
		

		public new PXAction<BranchBAccount> viewMainOnMap;
		public new PXAction<BranchBAccount> viewDefLocationOnMap;
		

		#region Button Delegates

		[PXUIField(DisplayName = CS.Messages.ViewEmployee, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		public new IEnumerable ViewContact(PXAdapter adapter)
		{
			if (this.Employees.Current != null && this.BAccountAccessor.Cache.GetStatus(this.BAccountAccessor.Current) != PXEntryStatus.Inserted)
			{
				EPEmployee current = this.Employees.Current;
				EmployeeMaint graph = PXGraph.CreateInstance<EmployeeMaint>();
				if ((graph.Employee.Current = graph.Employee.Search<EPEmployee.bAccountID>(current.BAccountID)) == null)
				{
					throw new PXSetPropertyException(Messages.BranchNotEnoughRights, this.BAccountAccessor.Current.AcctCD);
				}
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
				try
				{
					cont = (EPEmployee)graph.Employee.Insert(cont);
				}
				catch (PXFieldProcessingException ex)
				{
					if (graph.Employee.Cache.GetBqlField(ex.FieldName) == typeof(EPEmployee.parentBAccountID))
					{ 
						throw new PXSetPropertyException(Messages.BranchNotEnoughRights, this.BAccountAccessor.Current.AcctCD); 
					}
					throw;
				}

				throw new PXRedirectRequiredException(graph, CR.Messages.ContactMaint);
			}
			return adapter.Get();
		}


		/*
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
		*/

		[PXUIField(DisplayName = CR.Messages.SetDefault, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton()]
		public new IEnumerable SetDefault(PXAdapter adapter)
		{
			BranchBAccount acct = this.BAccount.Current;
			if (Locations.Current != null && acct != null && Locations.Current.LocationID != acct.DefLocationID)
			{
				acct.DefLocationID = Locations.Current.LocationID;
				this.BAccount.Update(acct);
			}
			return adapter.Get();
		}
		
		[PXCancelButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select)]
		protected override IEnumerable Cancel(PXAdapter a)
		{
			foreach (BranchBAccount r in (new PXCancel<BranchBAccount>(this, "Cancel")).Press(a))
			{
				if (r != null && BAccount.Cache.GetStatus(r) == PXEntryStatus.Inserted)
				{
					BAccountItself acct = PXSelectReadonly<BAccountItself, Where<BAccountItself.acctCD, Equal<Required<BAccountItself.acctCD>>>>.Select(this, r.AcctCD);
					if (acct != null && acct.BAccountID != r.BAccountID)
					{
						BAccount.Cache.RaiseExceptionHandling<BAccount.acctCD>(r, r.AcctCD,
							new PXSetPropertyException(Messages.ItemExistsReenter));
					}
					
				}
				yield return r;					
			}
		}
	   
		#endregion

		#endregion
		#endregion

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

		#region Events Handlers

		[PXDBInt()]
		[PXDBChildIdentity(typeof(LocationExtAddress.locationID))]
		[PXUIField(DisplayName = "Default Location", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(Search<LocationExtAddress.locationID,
			Where<LocationExtAddress.bAccountID,
			Equal<Current<BranchBAccount.bAccountID>>>>),
			DescriptionField = typeof(LocationExtAddress.locationCD),
			DirtyRead = true)]
		protected virtual void BranchBAccount_DefLocationID_CacheAttached(PXCache sender)
		{

		}

		[PXUIField(DisplayName = "Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[ContactDisplayName(typeof(Contact.lastName), typeof(Contact.firstName), typeof(Contact.midName), typeof(Contact.title), true)]
		protected virtual void Contact_DisplayName_CacheAttached(PXCache sender)
		{

		}

		protected virtual void BranchBAccount_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			this.OnBAccountRowInserted(sender, e);

            Company rec = Company.Select();
            Company.Cache.SetStatus(rec, PXEntryStatus.Updated);
		}

        protected virtual void BranchBAccount_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        { 
            BranchBAccount item = e.Row as BranchBAccount;
            if (item != null)
            {
                Ledger ledger = PXSelectJoin<Ledger,
                    InnerJoin<Branch, On<Branch.branchID, Equal<Ledger.defBranchID>>>,
                    Where<Branch.branchCD, Equal<Required<Branch.branchCD>>>>.Select(this, item.BranchBranchCD);

                if (ledger != null)
                {
                    GLHistory hist = PXSelect<GLHistory, Where<GLHistory.ledgerID, Equal<Required<GLHistory.ledgerID>>>>.Select(this, ledger.LedgerID);
                    if (hist != null)
                    {
                        throw new PXSetPropertyException(ErrorMessages.CantDeleteRecord);
                    }
                }
            }
        }

        public virtual void BranchBAccount_AllowsRUTROT_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var branch = (BranchBAccount)e.Row;
            if (branch == null)
                return;

            if (branch.AllowsRUTROT == true)
            {
                branch.RUTROTCuryID = this.Company.Current.BaseCuryID;
                branch.RUTROTPersonalAllowanceLimit = 50000.0m;
                branch.RUTROTDeductionPct = 50.0m;
                branch.RUTROTOrgNbrValidRegEx = "^(\\d{10})$";

                BAccount.Cache.RaiseFieldUpdated<BranchBAccount.rUTROTPersonalAllowanceLimit>(branch, 0.0m);
                BAccount.Cache.RaiseFieldUpdated<BranchBAccount.rUTROTDeductionPct>(branch, 0.0m);
            }
            else
            {
                branch.RUTROTDeductionPct = 0.0m;
                branch.RUTROTPersonalAllowanceLimit = 0.0m;
                branch.RUTROTCuryID = null;
            }
        }

        public virtual void BranchBAccount_RUTROTClaimNextRefNbr_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            var branch = (BranchBAccount)e.Row;

            if (branch == null || PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>() == false || branch.AllowsRUTROT != true)
                return;

            int newValue = (int?)e.NewValue ?? 0;
            int oldValue = branch.RUTROTClaimNextRefNbr ?? 0;

            if (newValue < oldValue)
            {
                PXUIFieldAttribute.SetWarning<BranchBAccount.rUTROTClaimNextRefNbr>(this.BAccount.Cache, branch, AR.RUTROTMessages.ClaimNextRefDecreased);
            }
        }

        public virtual void BranchBAccount_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            if(PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>())
                UpdateRUTROTControlsState();
        }

        private void UpdateRUTROTControlsState()
        {
            var branchBAcct = CurrentBAccount.Current;
            var branchBAcctCache = CurrentBAccount.Cache;

            var showRUTROTFields = branchBAcct.AllowsRUTROT == true;

            PXUIFieldAttribute.SetEnabled<BranchBAccount.rUTROTCuryID>(branchBAcctCache, branchBAcct, showRUTROTFields);
            PXUIFieldAttribute.SetEnabled<BranchBAccount.rUTROTPersonalAllowanceLimit>(branchBAcctCache, branchBAcct, showRUTROTFields);
            PXUIFieldAttribute.SetEnabled<BranchBAccount.rUTROTDeductionPct>(branchBAcctCache, branchBAcct, showRUTROTFields);
            PXUIFieldAttribute.SetEnabled<BranchBAccount.rUTROTClaimNextRefNbr>(branchBAcctCache, branchBAcct, showRUTROTFields);

            var persistingCheck = showRUTROTFields ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing;

            PXDefaultAttribute.SetPersistingCheck<BranchBAccount.rUTROTCuryID>(branchBAcctCache, branchBAcct, persistingCheck);
            PXDefaultAttribute.SetPersistingCheck<BranchBAccount.rUTROTPersonalAllowanceLimit>(branchBAcctCache, branchBAcct, persistingCheck);
            PXDefaultAttribute.SetPersistingCheck<BranchBAccount.rUTROTDeductionPct>(branchBAcctCache, branchBAcct, persistingCheck);
            PXDefaultAttribute.SetPersistingCheck<BranchBAccount.rUTROTCuryID>(branchBAcctCache, branchBAcct, persistingCheck);
        }

		protected virtual void BranchBAccount_Active_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
            BranchBAccount item = e.Row as BranchBAccount;
			if (item != null && (bool?)e.NewValue == false)
			{
				Ledger ledger;
				if ((ledger = PXSelectJoin<Ledger, InnerJoin<Branch, On<Branch.branchID, Equal<Ledger.defBranchID>>>, Where<Branch.branchCD, Equal<Required<Branch.branchCD>>>>.SelectWindowed(this, 0, 1, item.BranchBranchCD)) != null)
				{
					throw new PXSetPropertyException(GL.Messages.BranchUsedWithLedger, ledger.LedgerCD);
				}
				IN.INSite site;
				if ((site = PXSelectJoin<IN.INSite, InnerJoin<Branch, On<Branch.branchID, Equal<IN.INSite.branchID>>>, Where<Branch.branchCD, Equal<Required<Branch.branchCD>>>>.SelectWindowed(this, 0, 1, item.BranchBranchCD)) != null)
				{
					throw new PXSetPropertyException(GL.Messages.BranchUsedWithSite, site.SiteCD);
				}

				FA.FixedAsset fa;
				if ((fa = PXSelectJoin<FA.FixedAsset, InnerJoin<Branch, On<Branch.branchID, Equal<FA.FixedAsset.branchID>>>, Where<Branch.branchCD, Equal<Required<Branch.branchCD>>>>.SelectWindowed(this, 0, 1, item.BranchBranchCD)) != null)
				{
					throw new PXSetPropertyException(GL.Messages.BranchUsedWithFixedAsset, fa.AssetCD);
				}
			}
		}

		protected virtual void BranchBAccount_AcctName_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			this.OnBAccountAcctNameFieldUpdated(sender, e);
		}

        public override void Persist()
        {
            bool requestRelogin = Accessinfo.BranchID == null;

            using (PXTransactionScope ts = new PXTransactionScope())
            {
                foreach (BranchBAccount item in BAccount.Cache.Deleted)
                {
                    Branch branch = PXSelect<Branch, Where<Branch.branchCD, Equal<Required<Branch.branchCD>>>>.Select(this, item.BranchBranchCD);
                    if (branch != null)
                    {
                        if (Accessinfo.BranchID == branch.BranchID)
                            requestRelogin = true;
                        PXDatabase.Update<Ledger>(
                                new PXDataFieldAssign<Ledger.defBranchID>(null),
                                new PXDataFieldRestrict<Ledger.defBranchID>(branch.BranchID));
                    }
                }
                base.Persist();
                ts.Complete();
            }
            using (PXTransactionScope tran = new PXTransactionScope())
            {
                PXResultset<Branch> activeBranches = PXSelect<Branch>.Select(this);
                bool clearRoleNames = true;
                foreach (Branch branch in PXSelect<Branch>.Select(this))
                {
                    if (branch.RoleName != null) clearRoleNames = false;
                }
                using (PXReadDeletedScope rds = new PXReadDeletedScope())
                {
                    if (clearRoleNames)
                    {
                        PXDatabase.Update<Branch>(new PXDataFieldAssign<Branch.roleName>(null));
                    }
                }
                tran.Complete();
            }
            if (requestRelogin)
            {
                PXAccess.ResetBranchSlot();
                PXLogin.SetBranchID(CreateInstance<PX.SM.SMAccessPersonalMaint>().GetDefaultBranchId());
            }
        }

		public override int Persist(Type cacheType, PXDBOperation operation)
		{
			int res = base.Persist(cacheType, operation);
			if (cacheType == typeof(BranchBAccount) && operation == PXDBOperation.Update)
			{
				foreach (PXResult<NoteDoc, UploadFile> rec in PXSelectJoin<NoteDoc, InnerJoin<UploadFile, On<NoteDoc.fileID, Equal<UploadFile.fileID>>>,
														Where<NoteDoc.noteID, Equal<Current<BranchBAccount.noteID>>>>.Select(this))
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

		protected virtual void BranchBAccount_Type_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = BAccountType.CompanyType;
		}

		protected virtual void BranchBAccount_BranchBranchCD_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			//PXDBChildIdentity() hack
			if (e.Table != null && e.Operation == PXDBOperation.Update)
			{
				e.IsRestriction = false;
				e.Cancel = true;
			}
		}

		protected virtual void BranchBAccount_LedgerID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.branch>())
			{
				Ledger ledger = PXSelect<Ledger, Where<Ledger.balanceType, Equal<LedgerBalanceType.actual>>>.Select(this);

				if (ledger != null)
				{
					e.NewValue = ledger.LedgerID;
					e.Cancel = true;
				}
			}
		}
		#endregion	

		#region Events Company
		protected virtual void Company_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Company row = e.Row as Company;
			if (row != null)
			{
				if (row.BaseCuryID == null)
				{
					Currency b = new Currency();
					b.CuryID = String.Empty;
					b = this.CompanyCurrency.Insert(b);
					if (b != null)
					{
						row.BaseCuryID = b.CuryID;
					}
					this.CompanyCurrency.Cache.IsDirty = false;
				}
				PXUIFieldAttribute.SetEnabled<Company.baseCuryID>(sender, row, (glsetup.Select().Count == 0));
			}
		}

		protected virtual void Company_BaseCuryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!String.IsNullOrEmpty((string)e.NewValue) && PXSelectorAttribute.Select<Company.baseCuryID>(sender, e.Row, e.NewValue) == null)
			{
				foreach (Currency bc in CompanyCurrency.Cache.Inserted)
				{
					bc.CuryID = (string)e.NewValue;
					CompanyCurrency.Cache.Normalize();
					e.Cancel = true;
					break;
				}
			}
		}

		protected virtual void Company_BaseCuryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CompanyCurrency.View.RequestRefresh();
		}

		#endregion
	}

  [PXProjection(typeof(Select2<Branch, 
	  InnerJoin<Ledger, On<Ledger.ledgerID, Equal<Branch.ledgerID>>, 
	  InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>, 
	  InnerJoin<Address, On<Address.bAccountID, Equal<BAccountR.bAccountID>, And<Address.addressID, Equal<BAccountR.defAddressID>>>, 
	  InnerJoin<Contact, On<Contact.bAccountID, Equal<BAccountR.bAccountID>, And<Contact.contactID, Equal<BAccountR.defContactID>>>>>>>,
	Where2<Where2<Not<FeatureInstalled<FeaturesSet.branch>>, Or2<IsSingleBranchCompany, Or<Branch.branchID, Equal<Ledger.defBranchID>, And<Ledger.postInterCompany, Equal<False>, Or<Branch.branchID, NotEqual<Ledger.defBranchID>, And<Ledger.postInterCompany, Equal<True>, Or<Ledger.defBranchID, IsNull, And<Ledger.postInterCompany, Equal<True>>>>>>>>>, And<Branch.active, Equal<True>, And<MatchWithBranch<Branch.branchID, Current<AccessInfo.branchID>>>>>>))]
  [Serializable]
  public partial class CompanyBAccount : IBqlTable
  {
	  #region AcctName
	  public abstract class acctName : PX.Data.IBqlField { }

	  [PXDBString(60, IsUnicode = true, BqlField = typeof(BAccountR.acctName))]
	  [PXUIField(DisplayName = "Company")]
	  [PXDefault()]
	  public virtual String AcctName
	  {
		  get;
		  set;
	  }
	  #endregion
	  #region MasterBranchID
	  public abstract class masterBranchID : PX.Data.IBqlField { }

	  [PXDefault()]
	  [MasterBranch(BqlField = typeof(Branch.branchID))]
	  public virtual Int32? MasterBranchID
	  {
		  get;
		  set;
	  }
	  #endregion
	  #region BAccountID
	  public abstract class bAccountID : PX.Data.IBqlField { }
	  [PXDefault()]
	  [PXDBInt(BqlField = typeof(BAccountR.bAccountID))]
	  public virtual Int32? BAccountID
	  {
		  get;
		  set;
	  }
	  #endregion
	  #region LogoName
	  public abstract class logoName : PX.Data.IBqlField { }

	  [PXDBString(IsUnicode = true, BqlField = typeof(Branch.logoName))]
	  [PXDefault()]
	  public virtual String LogoName
	  {
		  get;
		  set;
	  }
	  #endregion
	  #region AddressLine1
	  public abstract class addressLine1 : PX.Data.IBqlField
	  {
	  }
	  protected String _AddressLine1;
	  [PXDBString(50, IsUnicode = true, BqlField = typeof(Address.addressLine1))]
	  [PXUIField(DisplayName = "Address Line 1")]
	  public virtual String AddressLine1
	  {
		  get
		  {
			  return this._AddressLine1;
		  }
		  set
		  {
			  this._AddressLine1 = value;
		  }
	  }
	  #endregion
	  #region AddressLine2
	  public abstract class addressLine2 : PX.Data.IBqlField
	  {
	  }
	  protected String _AddressLine2;
	  [PXDBString(50, IsUnicode = true, BqlField = typeof(Address.addressLine2))]
	  [PXUIField(DisplayName = "Address Line 2")]
	  public virtual String AddressLine2
	  {
		  get
		  {
			  return this._AddressLine2;
		  }
		  set
		  {
			  this._AddressLine2 = value;
		  }
	  }
	  #endregion
	  #region AddressLine3
	  public abstract class addressLine3 : PX.Data.IBqlField
	  {
	  }
	  protected String _AddressLine3;
	  [PXDBString(50, IsUnicode = true, BqlField = typeof(Address.addressLine3))]
	  [PXUIField(DisplayName = "Address Line 3")]
	  public virtual String AddressLine3
	  {
		  get
		  {
			  return this._AddressLine3;
		  }
		  set
		  {
			  this._AddressLine3 = value;
		  }
	  }
	  #endregion
	  #region City
	  public abstract class city : PX.Data.IBqlField
	  {
	  }
	  protected String _City;
	  [PXDBString(50, IsUnicode = true, BqlField = typeof(Address.city))]
	  [PXUIField(DisplayName = "City")]
	  public virtual String City
	  {
		  get
		  {
			  return this._City;
		  }
		  set
		  {
			  this._City = value;
		  }
	  }
	  #endregion
	  #region CountryID
	  public abstract class countryID : PX.Data.IBqlField
	  {
	  }
	  protected String _CountryID;
	  [PXDBString(2, IsFixed = true, BqlField = typeof(Address.countryID))]
	  [PXUIField(DisplayName = "Country")]
	  public virtual String CountryID
	  {
		  get
		  {
			  return this._CountryID;
		  }
		  set
		  {
			  this._CountryID = value;
		  }
	  }
	  #endregion
	  #region State
	  public abstract class state : PX.Data.IBqlField
	  {
	  }
	  protected String _State;
	  [PXDBString(50, IsUnicode = true, BqlField = typeof(Address.state))]
	  [PXUIField(DisplayName = "State")]
	  public virtual String State
	  {
		  get
		  {
			  return this._State;
		  }
		  set
		  {
			  this._State = value;
		  }
	  }
	  #endregion
	  #region PostalCode
	  public abstract class postalCode : PX.Data.IBqlField
	  {
	  }
	  protected String _PostalCode;
	  [PXDBString(20, BqlField = typeof(Address.postalCode))]
	  [PXUIField(DisplayName = "Postal Code")]
	  public virtual String PostalCode
	  {
		  get
		  {
			  return this._PostalCode;
		  }
		  set
		  {
			  this._PostalCode = value;
		  }
	  }
	  #endregion
	  #region Title
	  public abstract class title : PX.Data.IBqlField
	  {
	  }
	  protected String _Title;
	  [PXDBString(50, IsUnicode = true, BqlField = typeof(Contact.title))]
	  [Titles]
	  [PXUIField(DisplayName = "Title")]
	  public virtual String Title
	  {
		  get
		  {
			  return this._Title;
		  }
		  set
		  {
			  this._Title = value;
		  }
	  }
	  #endregion
	  #region Salutation
	  public abstract class salutation : PX.Data.IBqlField
	  {
	  }
	  protected String _Salutation;
	  [PXDBString(255, IsUnicode = true, BqlField = typeof(Contact.salutation))]
	  [PXUIField(DisplayName = "Position")]
	  public virtual String Salutation
	  {
		  get
		  {
			  return this._Salutation;
		  }
		  set
		  {
			  this._Salutation = value;
		  }
	  }
	  #endregion
	  #region FullName
	  public abstract class fullName : PX.Data.IBqlField
	  {
	  }
	  protected String _FullName;
	  [PXDBString(255, IsUnicode = true, BqlField = typeof(Contact.fullName))]
	  [PXUIField(DisplayName = "Business Name")]
	  public virtual String FullName
	  {
		  get
		  {
			  return this._FullName;
		  }
		  set
		  {
			  this._FullName = value;
		  }
	  }
	  #endregion
	  #region EMail
	  public abstract class eMail : PX.Data.IBqlField
	  {
	  }
	  protected String _EMail;
	  [PXDBEmail(BqlField = typeof(Contact.eMail))]
	  [PXUIField(DisplayName = "Email")]
	  public virtual String EMail
	  {
		  get
		  {
			  return this._EMail == null ? null : _EMail.Trim();
		  }
		  set
		  {
			  this._EMail = value;
		  }
	  }
	  #endregion
	  #region Phone1
	  public abstract class phone1 : PX.Data.IBqlField
	  {
	  }
	  protected String _Phone1;
	  [PhoneValidation]
	  [PXDBString(50, BqlField = typeof(Contact.phone1))]
	  [PXUIField(DisplayName = "Phone 1")]
	  public virtual String Phone1
	  {
		  get
		  {
			  return this._Phone1;
		  }
		  set
		  {
			  this._Phone1 = value;
		  }
	  }
	  #endregion
  }
}
