using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Objects.AR;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Data;
using PX.SM;

namespace PX.Objects.CR
{
	public class LeadMaint : PXGraph<LeadMaint>
	{

		#region AccountsFilter
		[Serializable]
		public partial class AccountsFilter : IBqlTable
		{
			#region BAccountID
			public abstract class bAccountID : IBqlField { }

			[PXDefault]
			[PXDBString(30, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
			[PXUIField(DisplayName = "Business Account ID", Required = true)]
			public virtual string BAccountID { get; set; }
			#endregion

			#region AccountName			
			public abstract class accountName : IBqlField { }

			[PXDefault(typeof(Contact.fullName))]
			[PXDBString(60, IsUnicode = true)]
			[PXUIField(DisplayName = "Company Name", Required = true)]
			public virtual string AccountName { get; set; }
			#endregion

			#region AccountClass
			public abstract class accountClass : IBqlField { }

			[PXDefault(typeof(CRSetup.defaultCustomerClassID))]
			[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
			[PXUIField(DisplayName = "Businnes Account Class", Required = true)]
			[PXSelector(typeof(CRCustomerClass.cRCustomerClassID))]
			public virtual string AccountClass { get; set; }
			#endregion
		}

		#endregion

		#region OppotunityFilter
		[Serializable]
		public class OpportunityFilter : IBqlTable
		{

			#region OpportunityID
			public abstract class opportunityID : PX.Data.IBqlField { }

			[PXDBString(CROpportunity.OpportunityIDLength, IsUnicode = true, InputMask = ">CCCCCCCCCC")]
			[PXUIField(DisplayName = "Opportunity ID", Required = true)]
			[PXDefault]
			public virtual String OpportunityID { get; set; }
			#endregion

		}
		#endregion

		#region Selects

		//TODO: need review
		[PXHidden]
		public PXSelect<BAccount>
			bAccountBasic;

		[PXHidden]
		[PXCheckCurrent]
		public PXSetup<Company>
			company;

		[PXHidden]
		[PXCheckCurrent]
		public PXSetup<CRSetup>
			Setup;
			
		[PXViewName(Messages.Lead)]
		public PXSelect<Contact,
			Where<Contact.contactType, Equal<ContactTypesAttribute.lead>>>
			Lead;

		[PXHidden]
		public PXSelect<Contact,
			Where<Contact.contactID, Equal<Current<Contact.contactID>>>>
			LeadCurrent;

		[PXHidden]
		public PXSelect<Contact,
			Where<Contact.contactID, Equal<Current<Contact.contactID>>>>
			LeadCurrent2;

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
			Where<Current<Contact.contactID>, Greater<Zero>,
 			And<Where<CROpportunity.bAccountID, Equal<Current<Contact.bAccountID>>,
					   Or<CROpportunity.contactID, Equal<Current<Contact.contactID>>>>>>>
			Opportunities;

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
		public PXFilter<AccountsFilter> AccountInfo;

		protected virtual void AccountsFilter_BAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.IsDimensionAutonumbered(CustomerAttribute.DimensionName))
			{
				e.NewValue = this.GetDimensionAutonumberingNewValue(CustomerAttribute.DimensionName);
			}
		}

		protected virtual void AccountsFilter_AccountName_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
				if (LeadCurrent.Current != null && LeadCurrent.Current.FullName != null)
				{
					e.NewValue = LeadCurrent.Current.FullName;
				}
		}

		protected virtual void AccountsFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled<AccountsFilter.bAccountID>(sender, e.Row, !this.IsDimensionAutonumbered(CustomerAttribute.DimensionName));
		}
		#endregion

		#region Create Opportunity

		[PXViewName(Messages.CreateOpportunity)]
		public PXFilter<OpportunityFilter> OpportunityInfo;

		#endregion

		#region Ctors

		public LeadMaint()
		{
			PXUIFieldAttribute.SetRequired<Contact.status>(Lead.Cache, true);
			PXUIFieldAttribute.SetRequired<Contact.classID>(Lead.Cache, true);
			PXUIFieldAttribute.SetRequired<Contact.lastName>(Lead.Cache, true);

			PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(bAccountBasic.Cache, Messages.BAccountCD);
			PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(bAccountBasic.Cache, Messages.BAccountName);

			Activities.GetNewEmailAddress =
				() =>
				{
					var contact = Lead.Current;
					return contact != null && !string.IsNullOrWhiteSpace(contact.EMail) ? new Email(contact.DisplayName, contact.EMail) : Email.Empty;
				};
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

		public PXAction<Contact> convertToContact;
		[PXUIField(DisplayName = Messages.ConvertToContact)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ConvertToContact(PXAdapter adapter)
		{
			List<Contact> contacts = new List<Contact>(adapter.Get().Cast<Contact>());
			foreach (Contact lead in contacts)
			{
				PXLongOperation.StartOperation(this, () =>
				{
					ContactMaint graph = CreateInstance<ContactMaint>();
					lead.DuplicateFound = false;
					lead.DuplicateStatus = DuplicateStatusAttribute.NotValidated;
					graph.Contact.Current = lead;

					graph.Contact.Cache.SetStatus(lead, PXEntryStatus.Updated);
					graph.Save.Press();
					throw new PXRedirectRequiredException(graph, "Contact");
				});
			}
			return contacts;
		}

		public PXAction<Contact> convertToOpportunity;
		[PXUIField(DisplayName = Messages.ConvertToOpportunity)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ConvertToOpportunity(PXAdapter adapter)
		{
			bool isOpportunityAutoNumberOn = this.IsNumberingAutonumbered(Setup.Current.OpportunityNumberingID);
			List<Contact> contacts = new List<Contact>(adapter.Get().Cast<Contact>());
			foreach (Contact lead in contacts)
			{

				OpportunityMaint opportunityMaint = PXGraph.CreateInstance<OpportunityMaint>();
				CROpportunity opportunity = (CROpportunity)opportunityMaint.Opportunity.Cache.CreateInstance();

				if (!isOpportunityAutoNumberOn)
				{
					if (OpportunityInfo.AskExt() != WebDialogResult.OK || !OpportunityInfo.VerifyRequired()) return contacts;
					CROpportunity existing = PXSelect<CROpportunity, Where<CROpportunity.opportunityID, Equal<Required<CROpportunity.opportunityID>>>>.SelectSingleBound(this, null, OpportunityInfo.Current.OpportunityID);
					if (existing != null)
					{
						OpportunityInfo.Cache.RaiseExceptionHandling<OpportunityFilter.opportunityID>(OpportunityInfo.Current, OpportunityInfo.Current.OpportunityID, new PXSetPropertyException(Messages.OpportunityAlreadyExists, OpportunityInfo.Current.OpportunityID));
						return contacts;
					}

					object cd = OpportunityInfo.Current.OpportunityID;
					opportunityMaint.Opportunity.Cache.RaiseFieldUpdating<CROpportunity.opportunityID>(null, ref cd);
					opportunity.OpportunityID = (string) cd;
				}
				PXLongOperation.StartOperation(this, delegate()
				{
					CRContactClass cls = PXSelect<CRContactClass, Where<CRContactClass.classID, Equal<Current<Contact.classID>>>>.SelectSingleBound(this, new object[] { lead });
					if (cls != null && cls.OwnerToOpportunity == true)
					{
						opportunity.WorkgroupID = lead.WorkgroupID;
						opportunity.OwnerID = lead.OwnerID;
					}

					if (lead.BAccountID != null)
						opportunity.BAccountID = lead.BAccountID;
					opportunity.ContactID = lead.ContactID;
					opportunity = (CROpportunity)opportunityMaint.Opportunity.Cache.Insert(opportunity);

					ContactMaint contactGraph = PXGraph.CreateInstance<ContactMaint>();
					lead.ContactType = ContactTypesAttribute.Person;
					contactGraph.Contact.Update(lead);
					contactGraph.Save.Press();

					opportunityMaint.Opportunity.Search<CROpportunity.opportunityID>(opportunity.OpportunityID);

					throw new PXRedirectRequiredException(opportunityMaint, "Opportunity", true);
				});
			}
			return contacts;
		}

		public PXAction<Contact> convertToBAccount;
		[PXUIField(DisplayName = Messages.ConvertToBAccount, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ConvertToBAccount(PXAdapter adapter)
		{
			List<Contact> contacts = new List<Contact>(adapter.Get().Cast<Contact>());
			foreach (Contact lead in contacts)
			{
				if (AccountInfo.AskExt((graph, view) => graph.Views[view].Cache.Clear(), true) != WebDialogResult.OK) return contacts;
				bool empty_required = !AccountInfo.VerifyRequired();
				BAccount existing = PXSelect<BAccount, Where<BAccount.acctCD, Equal<Required<BAccount.acctCD>>>>.SelectSingleBound(this, null, AccountInfo.Current.BAccountID);
				if (existing != null)
				{
					AccountInfo.Cache.RaiseExceptionHandling<AccountsFilter.bAccountID>(AccountInfo.Current, AccountInfo.Current.BAccountID, new PXSetPropertyException(Messages.BAccountAlreadyExists, AccountInfo.Current.BAccountID));
					return contacts; 
				}
				if (empty_required) return contacts;

				Save.Press();
				PXLongOperation.StartOperation(this, () => ConvertToAccount(lead, AccountInfo.Current));
			}
			return contacts;
		}

		public static void ConvertToAccount(Contact contact, AccountsFilter param)
		{
			BusinessAccountMaint accountMaint = CreateInstance<BusinessAccountMaint>();
			object cd = param.BAccountID;
			accountMaint.BAccount.Cache.RaiseFieldUpdating<BAccount.acctCD>(null, ref cd);
			BAccount account = new BAccount
			{
				AcctCD = (string) cd,
				AcctName = param.AccountName,
				Type = BAccountType.ProspectType,
				ClassID = param.AccountClass,
			};

			#region Set Contact and Address fields
			CRContactClass cls = PXSelect<CRContactClass, Where<CRContactClass.classID, Equal<Current<Contact.classID>>>>.SelectSingleBound(accountMaint, new object[] { contact });
			if (cls != null && cls.OwnerToBAccount == true)
			{
				account.WorkgroupID = contact.WorkgroupID;
				account.OwnerID = contact.OwnerID;
			}

			try
			{
				object newValue = account.OwnerID;
				accountMaint.BAccount.Cache.RaiseFieldVerifying<BAccount.ownerID>(account, ref newValue);
			}
			catch (PXSetPropertyException)
			{
				account.OwnerID = null;
			}
	
			account = accountMaint.BAccount.Insert(account);

			accountMaint.Answers.CopyAllAttributes(account, contact);

			Contact defContact = PXCache<Contact>.CreateCopy(PXSelect<Contact, Where<Contact.contactID, Equal<Current<BAccount.defContactID>>>>.SelectSingleBound(accountMaint, new object[] { account }));
			PXCache<Contact>.RestoreCopy(defContact, contact);
			defContact.ContactType = ContactTypesAttribute.BAccountProperty;
			defContact.Title = null;
			defContact.FirstName = null;
			defContact.LastName = null;
			defContact.MidName = null;
			defContact.FullName = account.AcctName;
			defContact.ContactID = account.DefContactID;			
			defContact.BAccountID = account.BAccountID;
			defContact.DuplicateStatus = DuplicateStatusAttribute.NotValidated;
			defContact.DuplicateFound = false;
			defContact = accountMaint.DefContact.Update(defContact);

			Address contactAddress = PXSelect<Address, Where<Address.addressID, Equal<Required<Contact.defAddressID>>>>.Select(accountMaint, contact.DefAddressID );
			if (contactAddress == null)
				throw new PXException(Messages.DefAddressNotExists, contact.DisplayName);
			contactAddress.BAccountID = account.BAccountID;
			accountMaint.AddressCurrent.Cache.Clear();

			defContact.DefAddressID = contactAddress.AddressID;
			defContact = accountMaint.DefContact.Update(defContact);
			
			contactAddress = accountMaint.AddressCurrent.Update(contactAddress);

			account.DefAddressID = contactAddress.AddressID;
			accountMaint.BAccount.Update(account);

			contact.BAccountID = account.BAccountID;
			contact.DuplicateStatus = DuplicateStatusAttribute.NotValidated;
			contact.DuplicateFound = false;
			accountMaint.Contacts.Cache.SetStatus(contact, PXEntryStatus.Updated);

			account.NoteID = PXNoteAttribute.GetNoteID<EPActivity.noteID>(accountMaint.CurrentBAccount.Cache, account);
			foreach (EPActivity a in PXSelect<EPActivity, Where<EPActivity.refNoteID, Equal<Required<Contact.noteID>>>>.Select(accountMaint, contact.NoteID))
			{
				a.ParentRefNoteID = account.NoteID;
				accountMaint.Activities.Cache.Update(a);
			}
			#endregion

			//accountMaint.BAccount.Cache.RaiseFieldUpdated<BAccount.defContactID>(account, account.DefContactID);
			throw new PXRedirectRequiredException(accountMaint, "Business Account");
		}


		public PXAction<Contact> checkForDuplicates;
		[PXUIField(DisplayName = Messages.CheckForDuplicates)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable CheckForDuplicates(PXAdapter adapter)
		{
			foreach (Contact rec in adapter.Get())
			{
				Lead.Current = rec;
				Contact orig = rec;
				Contact lead = rec;

				if (lead.MajorStatus == LeadMajorStatusesAttribute._CLOSED)
					yield return lead;				
					yield return lead;				

				if ( adapter.ExternalCall || rec.DuplicateStatus == DuplicateStatusAttribute.NotValidated)
				{
					Lead.Current.DuplicateFound = true;
					Duplicates.View.Clear();
					var result = Duplicates.Select();
					Lead.Current.DuplicateFound = (result != null && result.Count > 0);

					lead = Lead.Cache.CreateCopy(Lead.Current) as Contact;
					lead.DuplicateStatus = DuplicateStatusAttribute.Validated;

					Decimal? score = 0;
					foreach (PXResult<CRDuplicateRecord, Contact, CRLeadContactValidationProcess.Contact2> r in result)
					{
						CRLeadContactValidationProcess.Contact2 duplicate = r;
						CRDuplicateRecord contactScore = r;

						int duplicateWeight = ContactMaint.GetContactWeight(duplicate);
						int currentWeight = ContactMaint.GetContactWeight(Lead.Current);
						if (duplicateWeight > currentWeight ||
							(duplicateWeight == currentWeight &&
						     duplicate.ContactID < Lead.Current.ContactID))
						{
							lead.DuplicateStatus = DuplicateStatusAttribute.PossibleDuplicated;
							if (contactScore.Score > score)
								score = contactScore.Score;
						}
					}
					if(orig.DuplicateStatus != lead.DuplicateStatus)
						lead = Lead.Update(lead);

					if (Lead.Current.DuplicateFound == false && adapter.ExternalCall)
						Lead.Cache.RaiseExceptionHandling<Contact.duplicateStatus>(lead, null,
						                                                           new PXSetPropertyException(
																					   Messages.NoPossibleDuplicates, 
																					   PXErrorLevel.Warning));
				}				
				yield return lead;
			}			

			if(Lead.Cache.IsDirty)			
				Save.Press();			
		}	
		
		#endregion

		#region Event Handlers

		#region Contact

		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Lead ID", Visibility = PXUIVisibility.Invisible)]
		[ContactSelector(typeof(ContactTypesAttribute.lead))]
		public virtual void Contact_ContactID_CacheAttached(PXCache sender) {}

		[PXDBString(2, IsFixed = true)]
		[PXDefault(ContactTypesAttribute.Lead)]
		[ContactTypes]
		public virtual void Contact_ContactType_CacheAttached(PXCache sender) {}

		[CustomerAndProspect(DisplayName = "Business Account", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual void Contact_BAccountID_CacheAttached(PXCache sender) {}

		[PXDBInt]
		[LeadMajorStatuses]
		[PXDefault(LeadMajorStatusesAttribute._JUST_CREATED, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Visible = true)]
		public virtual void Contact_MajorStatus_CacheAttached(PXCache sender) {}

		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
		[LeadStatuses]
		[PXDefault(LeadStatusesAttribute.New)]
		[PXUIRequired(typeof(Switch<Case<Where<Contact.contactType, Equal<ContactTypesAttribute.lead>>, True>, False>))]
		public virtual void Contact_Status_CacheAttached(PXCache sender) {}

		[PXDBString(2, IsFixed = true)]
		[LeadResolutions]
		[PXUIField(DisplayName = "Reason", Visibility = PXUIVisibility.SelectorVisible)]
		[CRDropDownAutoValue(typeof(Contact.status))]
		public virtual void Contact_Resolution_CacheAttached(PXCache sender) {}

		[PXUIField(DisplayName = "Company Name", Visibility = PXUIVisibility.SelectorVisible)]
		[CRLeadFullName(typeof(Contact.bAccountID))]
		public virtual void Contact_FullName_CacheAttached(PXCache sender) {}

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Lead Class")]
		[PXSelector(typeof(CRContactClass.classID), DescriptionField = typeof(CRContactClass.description), CacheGlobal = true)]
		[PXRestrictor(typeof(Where<CRContactClass.active, Equal<True>>), Messages.InactiveContactClass, typeof(CRContactClass.classID))]
		[PXMassMergableField]
		[PXDefault(typeof(Search<CRSetup.defaultLeadClassID>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual void Contact_ClassID_CacheAttached(PXCache sender) { }

		protected virtual void Contact_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Contact row = e.Row as Contact;
			if (row == null) return;

			var isNotInserted = sender.GetStatus(row) != PXEntryStatus.Inserted;
			Relations.Cache.AllowInsert = isNotInserted;
			Opportunities.Cache.AllowInsert = isNotInserted;
			Members.Cache.AllowInsert = isNotInserted;
			Subscriptions.Cache.AllowInsert = isNotInserted;

			PXUIFieldAttribute.SetEnabled<Contact.contactID>(sender, row, true);

			CRContactClass leadClass = row.ClassID.
				With(_ => (CRContactClass)PXSelectReadonly<CRContactClass,
					Where<CRContactClass.classID, Equal<Required<CRContactClass.classID>>>>.
					Select(this, _));
			if (leadClass != null)
			{
				Activities.DefaultEMailAccountId = leadClass.DefaultEMailAccountID;
			}

			if (row.DuplicateStatus == DuplicateStatusAttribute.PossibleDuplicated || row.DuplicateFound == true)
			{
				sender.RaiseExceptionHandling<Contact.duplicateStatus>(row, 
					null, new PXSetPropertyException(Messages.LeadHavePossibleDuplicates, PXErrorLevel.Warning, row.ContactID));
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

		protected virtual void Contact_ClassID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			Contact row = (Contact) e.Row;
			CRContactClass cc = (CRContactClass) PXSelectorAttribute.Select<Contact.classID>(sender, row, e.NewValue);
			this.doCopyClassSettings = false;
			if (cc != null)
			{
				this.doCopyClassSettings = true;
				if (sender.GetStatus(row) != PXEntryStatus.Inserted)
				{
					if (e.ExternalCall && Lead.Ask(Messages.Warning, Messages.ContactClassChangeWarning, MessageButtons.YesNo) == WebDialogResult.No)
					{
						this.doCopyClassSettings = false;
					}
				}
			}
		}

		protected virtual void Contact_ClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Contact row = (Contact) e.Row;
			if (row != null)
			{
				if (this.doCopyClassSettings)
				{
					sender.SetDefaultExt<Contact.workgroupID>(row);
					sender.SetDefaultExt<Contact.ownerID>(row);					
					sender.SetDefaultExt<Contact.source>(row);
				}
			}
		}

		protected virtual void Contact_FullName_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Contact row = (Contact) e.Row;
			if (row != null)
			{
				AccountInfo.Cache.SetDefaultExt<AccountsFilter.accountName>(AccountInfo.Current);
			}
		}

		public override void Persist()
		{
			base.Persist();
			if (Setup.Current.ValidateContactDuplicatesOnEntry == true)
			{
					checkForDuplicates.Press();				
			}
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

		#region CRDuplicateRecord

		protected virtual void CRDuplicateRecord_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.IsDirty = false;
		}

		#endregion

		#endregion

		#region Protected Methods

		protected bool CanBeMerged(Contact row)
		{
			return row.QualificationDate == null;
		}

		#endregion

		#region Private Methods
		
		#endregion

		#region Private members

		private bool doCopyClassSettings;

		#endregion
	}

	public static class PXAutonumberingInfo
	{
		public static bool IsDimensionAutonumbered(this PXGraph graph, string dimension)
		{
			return PXSelect<Segment, Where<Segment.dimensionID, Equal<Required<Segment.dimensionID>>>>.Select(graph, dimension)
				.RowCast<Segment>()
				.Any(segment => segment.AutoNumber == true);
		}

		public static bool IsNumberingAutonumbered(this PXGraph graph, string numbering)
		{
			return PXSelect<Numbering, Where<Numbering.numberingID, Equal<Required<Numbering.numberingID>>>>.Select(graph, numbering)
				.RowCast<Numbering>()
				.Any(n => n.UserNumbering != true);
		}

		public static string GetDimensionAutonumberingNewValue(this PXGraph graph, string dimension)
		{
			Numbering n = (PXResult<Dimension, Numbering>)PXSelectJoin<Dimension,
				LeftJoin<Numbering, On<Dimension.numberingID, Equal<Numbering.numberingID>>>,
				Where<Dimension.dimensionID, Equal<Required<Dimension.dimensionID>>,
					And<Numbering.userNumbering, NotEqual<True>>>>
				.SelectSingleBound(graph, null, dimension);

			return n.With(_ => _.NewSymbol) ?? Messages.New;
		}
	}
}
