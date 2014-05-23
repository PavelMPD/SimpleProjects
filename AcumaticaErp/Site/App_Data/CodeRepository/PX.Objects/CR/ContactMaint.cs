using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR.MassProcess;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.SM.Descriptor;
using PX.SM;

namespace PX.Objects.CR
{
	public class ContactMaint : PXGraph<ContactMaint>
	{
		#region Inner Types
		[Serializable]
		public class CurrentUser : Users
		{
			public new abstract class pKID : IBqlField {}
			public new abstract class guest : IBqlField { }
		}
		#endregion

		#region Selects

		//TODO: need review
		[PXHidden]
		public PXSelect<BAccount>
			bAccountBasic;

		[PXHidden]
		public PXSetup<Company>
			company;

		[PXHidden]
		public PXSetup<CRSetup>
			Setup;

		[PXViewName(Messages.Contact)]
		public SelectContactEmailSync<Where<Contact.contactType, Equal<ContactTypesAttribute.person>, Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>
			Contact;

		public PXSelect<Contact,
			Where<Contact.contactID, Equal<Current<Contact.contactID>>>>
			ContactCurrent;

		public PXSelect<Contact,
			Where<Contact.contactID, Equal<Current<Contact.contactID>>>>
			ContactCurrent2;

		[PXViewName(Messages.Address)]
		public AddressSelect<Contact.defAddressID, Contact.isAddressSameAsMain, Contact.bAccountID>
			AddressCurrent;

		[PXViewName(Messages.Answers)]
		public CRAttributeList<Contact>
			Answers;

		[PXViewName(Messages.Activities)]
		[PXFilterable]
		[CheckActivitiesOnDelete]
		[CRDefaultMailTo]
		[CRBAccountReference(typeof(Select<BAccountR, Where<BAccountR.bAccountID, Equal<Current<Contact.bAccountID>>>>))]
		public CRActivityList<Contact>
			Activities;

		[PXViewName(Messages.Relations)]
		[PXFilterable]
		public CRRelationsList<Contact.noteID>
			Relations;

		[PXViewName(Messages.Opportunities)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(Contact))]
		[PXViewDetailsButton(typeof(Contact),
			typeof(Select<Contact,
				Where<Contact.contactID, Equal<Current<CROpportunity.contactID>>>>))]
		public PXSelectReadonly2<CROpportunity,
			LeftJoin<Contact, On<Contact.contactID, Equal<CROpportunity.contactID>>, 
			LeftJoin<CROpportunityProbability, On<CROpportunityProbability.stageCode, Equal<CROpportunity.stageID>>>>,
			Where<CROpportunity.contactID, Equal<Current<Contact.contactID>>>>
			Opportunities;

		[PXFilterable]
		[PXViewDetailsButton(typeof(Contact))]
		public PXSelectReadonly<CRCase,
			Where<CRCase.contactID, Equal<Current<Contact.contactID>>>>
			Cases;

		[PXViewName(Messages.CampaignMember)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(Contact), 
			typeof(Select<CRCampaign, 
				Where<CRCampaign.campaignID, Equal<Current<CRCampaignMembers.campaignID>>>>))]
		public PXSelectJoin<CRCampaignMembers,
			InnerJoin<CRCampaign, On<CRCampaignMembers.campaignID, Equal<CRCampaign.campaignID>>>,
			Where<CRCampaignMembers.contactID, Equal<Current<Contact.contactID>>>>
			Members;

		[PXViewName(Messages.Subscriptions)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(Contact),
			typeof(Select<CRMarketingList,
				Where<CRMarketingList.marketingListID, Equal<Current<CRMarketingListMember.marketingListID>>>>))]
		public PXSelectJoin<CRMarketingListMember,
			InnerJoin<CRMarketingList,
				On<CRMarketingList.marketingListID, Equal<CRMarketingListMember.marketingListID>>>,
			Where<CRMarketingListMember.contactID, Equal<Current<Contact.contactID>>>>
			Subscriptions;

		[PXViewName(Messages.Notifications)]
		public PXSelectJoin<ContactNotification,
			InnerJoin<NotificationSetup,
				On<NotificationSetup.setupID, Equal<ContactNotification.setupID>>>,
			Where<ContactNotification.contactID, Equal<Optional<Contact.contactID>>>>
			NWatchers;

		public PXSelectPureUsers<Contact, Where<Users.pKID, Equal<Current<Contact.userID>>>> User;
		public PXSelectUsersInRoles UserRoles;

		public PXSelectAllowedRoles Roles;

		[PXViewDetailsButton(typeof(Contact),
			typeof(Select<Contact,
			Where<Contact.contactID, Equal<Current<CRDuplicateRecord.duplicateContactID>>>>))]
		[PXViewDetailsButton(typeof(Contact),
			typeof(Select2<BAccountCRM,
				InnerJoin<Contact, On<Contact.bAccountID, Equal<BAccountCRM.bAccountID>>>,
				Where<Contact.contactID, Equal<Current<CRDuplicateRecord.duplicateContactID>>>>),
				ActionName = "Duplicates_BAccount_ViewDetails")]
		public CRDuplicateContactList Duplicates;

		#endregion
		#region Create Account

		[PXViewName(Messages.CreateAccount)]
		public PXFilter<LeadMaint.AccountsFilter> AccountInfo;

		protected virtual void AccountsFilter_BAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.IsDimensionAutonumbered(CustomerAttribute.DimensionName))
			{
				e.NewValue = this.GetDimensionAutonumberingNewValue(CustomerAttribute.DimensionName);
			}
		}

		protected virtual void AccountsFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled<LeadMaint.AccountsFilter.bAccountID>(sender, e.Row, !this.IsDimensionAutonumbered(CustomerAttribute.DimensionName));
		}
		#endregion

		#region Ctors

		public ContactMaint()
		{
			PXUIFieldAttribute.SetRequired<Contact.lastName>(Contact.Cache, true);

			PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(bAccountBasic.Cache, Messages.BAccountCD);
			PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(bAccountBasic.Cache, Messages.BAccountName);

			Activities.GetNewEmailAddress =
				() =>
				{
					var contact = Contact.Current;
					return contact != null && !string.IsNullOrWhiteSpace(contact.EMail) ? new Email(contact.DisplayName, contact.EMail) : Email.Empty;
				};

			PXUIFieldAttribute.SetEnabled<EPLoginTypeAllowsRole.rolename>(Roles.Cache, null, false);
			Roles.Cache.AllowInsert = false;
			Roles.Cache.AllowDelete = false;

		}

		#endregion

		#region Actions

		public PXSave<Contact> Save;
		public PXSaveClose<Contact> SaveClose;
		public PXCancel<Contact> Cancel;
		public PXInsert<Contact> Insert;
		public PXCopyPasteAction<Contact> CopyPaste;
		public PXDelete<Contact> Delete;
		public PXFirst<Contact, Contact.displayName> First;
		public PXPrevious<Contact, Contact.displayName> Previous;
		public PXNext<Contact, Contact.displayName> Next;
		public PXLast<Contact, Contact.displayName> Last;

		public PXMenuAction<Contact> Action;

		public PXDBAction<Contact> addOpportunity;
        [PXUIField(DisplayName = Messages.AddNewOpportunity, FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
		public virtual void AddOpportunity()
		{
			var row = ContactCurrent.Current;
			if (row == null || row.ContactID == null) return;

			var graph = PXGraph.CreateInstance<OpportunityMaint>();
			var newOpportunity = (CROpportunity)graph.Opportunity.Cache.CreateInstance();
			newOpportunity.BAccountID = row.BAccountID;
			newOpportunity.ContactID = row.ContactID;
			graph.Opportunity.Insert(newOpportunity);
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXDBAction<Contact> addCase;
        [PXUIField(DisplayName = Messages.AddNewCase, FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
		public virtual void AddCase()
		{
		    var row = ContactCurrent.Current;
		    if (row == null || row.ContactID == null) return;

            Contact contact = (Contact)PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.Select(this, row.ContactID);
            if (contact.BAccountID == null)
            {
                Contact.Cache.RaiseExceptionHandling<Contact.bAccountID>(row, null, new PXSetPropertyException<Contact.bAccountID>(Messages.BAccountRequiredToCreateCase));
                return;
            }
			var graph = PXGraph.CreateInstance<CRCaseMaint>();
			var newCase = (CRCase)graph.Case.Cache.CreateInstance();
			newCase.CustomerID = row.BAccountID;
			newCase.ContactID = row.ContactID;
			graph.Case.Insert(newCase);
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXAction<Contact> copyBAccountContactInfo;
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowDown, Tooltip = Messages.CopyFromCompany)]
		[PXUIField(DisplayName = Messages.CopyFromCompany)]
		public virtual void CopyBAccountContactInfo()
		{
			var row = ContactCurrent.Current as Contact;
			if (row == null || row.BAccountID == null) return;

			var acct = (BAccount)PXSelect<BAccount,
				Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Select(this, row.BAccountID);
			if (acct != null && acct.DefContactID != null)
			{
				var defContact = (Contact)PXSelect<Contact,
					Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
					Select(this, acct.DefContactID);
				if (defContact != null)
					CopyContactInfo(row, defContact);
				ContactCurrent.Update(row);
			}
		}

		public PXAction<Contact> checkForDuplicates;
		[PXUIField(DisplayName = Messages.CheckForDuplicates)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable CheckForDuplicates(PXAdapter adapter)
		{
			foreach (Contact rec in adapter.Get())
			{
				Contact.Current = rec;
				Contact contact = rec;

				if (adapter.ExternalCall || rec.DuplicateStatus == DuplicateStatusAttribute.NotValidated)
				{
					Contact.Current.DuplicateFound = true;
					Duplicates.View.Clear();
					var result = Duplicates.Select();
					Contact.Current.DuplicateFound = (result != null && result.Count > 0);

					contact = Contact.Cache.CreateCopy(Contact.Current) as Contact;
					contact.DuplicateStatus = DuplicateStatusAttribute.Validated;

					Decimal? score = 0;
					foreach (PXResult<CRDuplicateRecord, Contact, CRLeadContactValidationProcess.Contact2> r in result)
					{
						CRLeadContactValidationProcess.Contact2 duplicate = r;
						CRDuplicateRecord contactScore = r;
						int duplicateWeight = GetContactWeight(duplicate);
						int currentWeight = GetContactWeight(Contact.Current);
						if (duplicateWeight > currentWeight ||
								(duplicateWeight == currentWeight &&
								 duplicate.ContactID < Contact.Current.ContactID))
						{
							contact.DuplicateStatus = DuplicateStatusAttribute.PossibleDuplicated;
							if (contactScore.Score > score)
								score = contactScore.Score;
						}
					}
					contact = Contact.Update(contact);

					if (Contact.Current.DuplicateFound == false && adapter.ExternalCall)
						Contact.Cache.RaiseExceptionHandling<Contact.duplicateStatus>(contact, null,
																				   new PXSetPropertyException(
																					   Messages.NoPossibleDuplicates,
																					   PXErrorLevel.Warning));
				}
				yield return contact;
			}

			if (Contact.Cache.IsDirty)
				Save.Press();
		}	

		public PXAction<Contact> convertToBAccount;
		[PXUIField(DisplayName = Messages.ConvertToBAccount, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ConvertToBAccount(PXAdapter adapter)
		{
			List<Contact> contacts = new List<Contact>(adapter.Get().Cast<Contact>());
			foreach (Contact contact in contacts.Where(contact => contact != null))
			{
				if (AccountInfo.AskExt((graph, view) => graph.Views[view].Cache.Clear(), true) != WebDialogResult.OK) return contacts;
				bool empty_required = !AccountInfo.VerifyRequired();
				BAccount existing = PXSelect<BAccount, Where<BAccount.acctCD, Equal<Required<BAccount.acctCD>>>>.SelectSingleBound(this, null, AccountInfo.Current.BAccountID);
				if (existing != null)
				{
					AccountInfo.Cache.RaiseExceptionHandling<LeadMaint.AccountsFilter.bAccountID>(AccountInfo.Current, AccountInfo.Current.BAccountID, new PXSetPropertyException(Messages.BAccountAlreadyExists, AccountInfo.Current.BAccountID));
					return contacts;
				}
				if (empty_required) return contacts;

				Save.Press();
				PXLongOperation.StartOperation(this, () => LeadMaint.ConvertToAccount(contact, AccountInfo.Current));
			}
			return contacts;
		}	
		#endregion

		#region Event Handlers

		#region Contact

		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Contact ID")]
		[PXSelector(typeof(Search<Contact.contactID, 
			Where<Contact.contactType, Equal<ContactTypesAttribute.person>, 
				Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>), 
				DescriptionField = typeof(Contact.displayName))]
		public virtual void Contact_ContactID_CacheAttached(PXCache sender) { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Contact Class")]
		[PXSelector(typeof(CRContactClass.classID), DescriptionField = typeof(CRContactClass.description), CacheGlobal = true)]
		[PXRestrictor(typeof(Where<CRContactClass.active, Equal<True>>), Messages.InactiveContactClass, typeof(CRContactClass.classID))]
		[PXMassMergableField]
		[PXDefault(typeof(Search<CRSetup.defaultContactClassID>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual void Contact_ClassID_CacheAttached(PXCache sender) { }

		[BAccount(Visibility = PXUIVisibility.SelectorVisible)]
		[PXRestrictor(typeof(Where<Current<Contact.contactType>, NotEqual<ContactTypesAttribute.employee>, 
			Or<BAccount.type, Equal<BAccountType.companyType>>>), Messages.BAccountIsType, typeof(BAccount.type))]
		[PXRestrictor(typeof(Where<Current<Contact.contactType>, NotEqual<ContactTypesAttribute.person>, 
			Or<BAccount.type, Equal<BAccountType.prospectType>,
			Or<BAccount.type, Equal<BAccountType.customerType>,
			Or<BAccount.type, Equal<BAccountType.vendorType>,
			Or<BAccount.type, Equal<BAccountType.combinedType>>>>>>), Messages.BAccountIsType, typeof(BAccount.type))]
		public virtual void Contact_BAccountID_CacheAttached(PXCache sender) { }

		[PXUIField(DisplayName = "Company Name", Visibility = PXUIVisibility.SelectorVisible)]
		[CRLeadFullName(typeof(Contact.bAccountID))]
		public virtual void Contact_FullName_CacheAttached(PXCache sender){}

		[PXDBGuid(IsKey = true)]
		[PXDefault]
		[PXUIField(Visibility = PXUIVisibility.Invisible)]
		public virtual void Users_PKID_CacheAttached(PXCache sender){}

		[PXDBString(64, IsUnicode = true, InputMask = ""/*"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA||.'@-_"*/)]
		[PXDefault]
		[PXUIField(DisplayName = "Username")]
		public virtual void Users_Username_CacheAttached(PXCache sender){}

		//DONE: need to duplicate in User Maint
		[PXDBInt]
		[PXUIField(DisplayName = "User Type")]
		[PXRestrictor(typeof(Where<EPLoginType.entity, Equal<EPLoginType.entity.contact>>), Messages.NonContactLoginType, typeof(EPLoginType.loginTypeName))]
		[PXSelector(typeof(Search5<EPLoginType.loginTypeID, LeftJoin<EPManagedLoginType, On<EPLoginType.loginTypeID, Equal<EPManagedLoginType.loginTypeID>>,
								LeftJoin<Users, On<EPManagedLoginType.parentLoginTypeID, Equal<Users.loginTypeID>>,
								LeftJoin<CurrentUser, On<CurrentUser.pKID, Equal<Current<AccessInfo.userID>>>>>>,
								Where<Users.pKID, Equal<CurrentUser.pKID>, And<CurrentUser.guest, Equal<True>,
									Or<CurrentUser.guest, NotEqual<True>>>>, 
								Aggregate<GroupBy<EPLoginType.loginTypeID, GroupBy<EPLoginType.loginTypeName>>>>), 
			SubstituteKey = typeof(EPLoginType.loginTypeName))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void Users_LoginTypeID_CacheAttached(PXCache sender) { }

		[PXDBBool]
		[PXUIField(DisplayName = "Guest Account")]
		[PXFormula(typeof(Switch<Case<Where<Selector<Users.loginTypeID, EPLoginType.entity>, Equal<EPLoginType.entity.contact>>, True>, False>))]
		protected virtual void Users_Guest_CacheAttached(PXCache sender) { }

		[PXDBBool]
		[PXFormula(typeof(Selector<Users.loginTypeID, EPLoginType.requireLoginActivation>))]
		protected virtual void Users_IsPendingActivation_CacheAttached(PXCache sender) { }

		[PXDBBool]
		[PXUIField(DisplayName = "Force User to Change Password on Next Login")]
		[PXFormula(typeof(Switch<Case<Where<Selector<Users.loginTypeID, EPLoginType.resetPasswordOnLogin>, Equal<True>>, True>, False>))]
		protected virtual void Users_PasswordChangeOnNextLogin_CacheAttached(PXCache sender) { }

		[PXBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Generate Password")]
		protected virtual void Users_GeneratePassword_CacheAttached(PXCache sender) { }
		// end

		[PXDBString(64, IsKey = true, IsUnicode = true, InputMask = "")]
		[PXDefault(typeof(Users.username))]
		[PXParent(typeof(Select<Users, Where<Users.username, Equal<Current<UsersInRoles.username>>>>))]
		protected virtual void UsersInRoles_Username_CacheAttached(PXCache sender) { }

		public override void Persist()
		{
			if (User.Current != null && User.Cache.GetStatus(User.Current) == PXEntryStatus.Inserted)
			{
				User.Current.OldPassword = User.Current.Password;
				User.Current.NewPassword = User.Current.Password;
				User.Current.ConfirmPassword = User.Current.Password;

				User.Current.FirstName = Contact.Current.FirstName;
				User.Current.LastName = Contact.Current.LastName;
				User.Current.Email = Contact.Current.EMail;

				User.Current.IsAssigned = true;
			}
			base.Persist();
			if (Setup.Current.ValidateContactDuplicatesOnEntry == true)
			{
				checkForDuplicates.Press();
			}
		}

		protected virtual void Contact_BAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			Contact contact = (Contact) e.Row;
			if (contact.BAccountID != (int?) e.NewValue && contact.ContactID > 0)
			{
				CRCase crCase = Cases.SelectSingle();
				CROpportunity op = Opportunities.SelectSingle();
				if (crCase != null || op != null)
				{
					throw new PXSetPropertyException(Messages.CannotChangeBAccount);
				}
			}
		}

		protected virtual void Users_State_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.ReturnValue == null && (e.Row == null || sender.GetStatus(e.Row) == PXEntryStatus.Inserted))
			{
				e.ReturnValue = Users.state.NotCreated;
			}
		}

		protected virtual void Users_LoginTypeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			UserRoles.Cache.Clear();
			if (((Users) e.Row).LoginTypeID == null)
			{
				User.Cache.Clear();
				Contact.Current.UserID = null;
			}
		}

		protected virtual void Users_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Users user = (Users) e.Row;
			if (Contact.Current.UserID == null)
			{
				Contact.Current.UserID = user.PKID;
			}
			else
			{
				User.Cache.Clear();
				UserRoles.Cache.Clear();
			}

			EPLoginType ltype = PXSelect<EPLoginType, Where<EPLoginType.loginTypeID, Equal<Current<Users.loginTypeID>>>>.SelectSingleBound(this, new object[]{user});
			user.Username = ltype != null && ltype.EmailAsLogin == true ? Contact.Current.EMail : null;
		}

		protected virtual void Contact_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Contact row = e.Row as Contact;
			if (row == null) return;

			var isNotInserted = sender.GetStatus(row) != PXEntryStatus.Inserted;
			Contact.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			Contact.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;

			AddressCurrent.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person;
			AddressCurrent.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			AddressCurrent.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;

			Answers.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person;
			Answers.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			Answers.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;

			Activities.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person;
			Activities.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			Activities.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;

			Relations.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person && isNotInserted;
			Relations.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			Relations.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;

			Opportunities.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person && isNotInserted;
			Opportunities.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			Opportunities.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;

			Cases.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person && isNotInserted;
			Cases.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			Cases.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;

			Members.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person && isNotInserted;
			Members.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			Members.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;

			Subscriptions.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person && isNotInserted;
			Subscriptions.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			Subscriptions.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;

			NWatchers.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person && isNotInserted;
			NWatchers.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			NWatchers.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;

			User.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person;
			User.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			User.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;
			User.Cache.AllowSelect = row.ContactType == ContactTypesAttribute.Person;
			User.Cache.ClearQueryCache();

			Roles.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person;
			Roles.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			Roles.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;
			Roles.Cache.AllowSelect = row.ContactType == ContactTypesAttribute.Person;
			Roles.Cache.ClearQueryCache();

			UserRoles.Cache.AllowInsert = row.ContactType == ContactTypesAttribute.Person;
			UserRoles.Cache.AllowUpdate = row.ContactType == ContactTypesAttribute.Person;
			UserRoles.Cache.AllowDelete = row.ContactType == ContactTypesAttribute.Person;
			UserRoles.Cache.AllowSelect = row.ContactType == ContactTypesAttribute.Person;
			UserRoles.Cache.ClearQueryCache();

			var bAccount = row.BAccountID.
				With(_ => (BAccount)PXSelect<BAccount,
					Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Select(this, _.Value));
			var isCustomerOrProspect = bAccount == null || 
				bAccount.Type == BAccountType.CustomerType ||
				bAccount.Type == BAccountType.ProspectType || 
				bAccount.Type == BAccountType.CombinedType;
			addOpportunity.SetEnabled(isNotInserted && isCustomerOrProspect);
			addCase.SetEnabled(isNotInserted && isCustomerOrProspect);

			PXUIFieldAttribute.SetEnabled<Contact.contactID>(sender, row, true);
			PXUIFieldAttribute.SetEnabled<Contact.bAccountID>(sender, row, row.ContactType == ContactTypesAttribute.Person);

			CRContactClass contactClass = row.ClassID.
				With(_ => (CRContactClass)PXSelectReadonly<CRContactClass,
					Where<CRContactClass.classID, Equal<Required<CRContactClass.classID>>>>.
					Select(this, _));
			if (contactClass != null)
			{
				Activities.DefaultEMailAccountId = contactClass.DefaultEMailAccountID;
			}

			bool isUserInserted = row.UserID == null || User.Cache.GetStatus(User.Current) == PXEntryStatus.Inserted;
			bool hasLoginType = isUserInserted && User.Current != null && User.Current.LoginTypeID != null;
			PXUIFieldAttribute.SetEnabled<Users.loginTypeID>(User.Cache, User.Current, isUserInserted && row.IsActive == true);
			PXUIFieldAttribute.SetEnabled<Users.username>(User.Cache, User.Current, hasLoginType);
			PXUIFieldAttribute.SetEnabled<Users.generatePassword>(User.Cache, User.Current, hasLoginType);
			PXUIFieldAttribute.SetEnabled<Users.password>(User.Cache, User.Current, hasLoginType && User.Current.GeneratePassword != true);

			PXDefaultAttribute.SetPersistingCheck<Users.username>(User.Cache, User.Current, hasLoginType ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXUIFieldAttribute.SetRequired<Users.username>(User.Cache, hasLoginType);
	
			PXDefaultAttribute.SetPersistingCheck<Users.password>(User.Cache, User.Current, hasLoginType ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXUIFieldAttribute.SetRequired<Users.password>(User.Cache, hasLoginType);

			PXDefaultAttribute.SetPersistingCheck<Contact.eMail>(sender, row, hasLoginType ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXUIFieldAttribute.SetRequired<Contact.eMail>(sender, hasLoginType);

			User.Current = (Users)User.View.SelectSingleBound(new[] { e.Row });

			if (row.DuplicateStatus == DuplicateStatusAttribute.PossibleDuplicated || row.DuplicateFound == true)
			{
				sender.RaiseExceptionHandling<Contact.duplicateStatus>(row,
					null, new PXSetPropertyException(Messages.ContactHavePossibleDuplicates, PXErrorLevel.Warning, row.ContactID));
			}
		}

		/*protected virtual void Contact_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			sender.IsDirty = false;
		}*/

		protected virtual void Contact_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			Contact contact = (Contact) e.Row;
			if(contact != null && contact.ContactType == ContactTypesAttribute.Employee)
				throw new PXSetPropertyException(Messages.CantDeleteEmployeeContact);
		}

		protected virtual void Contact_IsActive_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Contact c = (Contact) e.Row;
			if (c.IsActive == true && c.IsActive != (bool?)e.OldValue)
			{
				c.DuplicateStatus = DuplicateStatusAttribute.NotValidated;
			}

			Users user = PXSelect<Users, Where<Users.pKID, Equal<Current<Contact.userID>>>>.SelectSingleBound(this, new object[] { c });
			if (user != null)
			{
				user.IsApproved = c.IsActive == true;
				User.Update(user);
			}
		}

		protected virtual void Contact_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			Contact row = e.Row as Contact;
			if (row == null || e.TranStatus != PXTranStatus.Open) return;
			if (CRGrammProcess.PersistGrams(this, row) &&
					Setup.Current.ValidateContactDuplicatesOnEntry == true &&
					Object.Equals(sender.GetValue<Contact.duplicateStatus>(e.Row), sender.GetValueOriginal<Contact.duplicateStatus>(e.Row)))
				row.DuplicateStatus = DuplicateStatusAttribute.NotValidated;			
		}

		#endregion

		#region CRCampaignMembers

		[PXDBInt(IsKey = true)]
		[PXDBLiteDefault(typeof(Contact.contactID))]
		protected virtual void CRCampaignMembers_ContactID_CacheAttached(PXCache sender)
		{

		}

		#endregion

		#region CRMarketingListMember

		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Marketing List ID")]
		[PXSelector(typeof(Search<CRMarketingList.marketingListID,
			Where<CRMarketingList.isDynamic, IsNull, Or<CRMarketingList.isDynamic, NotEqual<True>>>>),
			DescriptionField = typeof(CRMarketingList.mailListCode))]
		protected virtual void CRMarketingListMember_MarketingListID_CacheAttached(PXCache sender)
		{

		}

		[PXDBInt(IsKey = true)]
		[PXDBLiteDefault(typeof(Contact.contactID))]
		protected virtual void CRMarketingListMember_ContactID_CacheAttached(PXCache sender)
		{

		}

		#endregion

		#endregion

		#region Private Methods

		protected void CopyContactInfo(Contact dest, Contact src)
		{
			if (!string.IsNullOrEmpty(src.FaxType)) dest.FaxType = src.FaxType;
			if (!string.IsNullOrEmpty(src.Phone1Type)) dest.Phone1Type = src.Phone1Type;
			if (!string.IsNullOrEmpty(src.Phone2Type)) dest.Phone2Type = src.Phone2Type;
			if (!string.IsNullOrEmpty(src.Phone3Type)) dest.Phone3Type = src.Phone3Type;

			dest.Fax = src.Fax;
			dest.Phone1 = src.Phone1;
			dest.Phone2 = src.Phone2;
			dest.Phone3 = src.Phone3;
			dest.WebSite = src.WebSite;
			dest.EMail = src.EMail;
		}

		public static int GetContactWeight(Contact contact)
		{
			if (contact.ContactType == ContactTypesAttribute.BAccountProperty)
				return 2;
			if (contact.ContactType == ContactTypesAttribute.Person)
				return 1;
			return 0;
		}
		#endregion
	}
}
