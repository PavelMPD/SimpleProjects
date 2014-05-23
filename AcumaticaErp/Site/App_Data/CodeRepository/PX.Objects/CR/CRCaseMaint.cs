using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using PX.Common;
using PX.Data;
using System.Collections;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CT;
using PX.Objects.GL;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.SM;
using PX.TM;

namespace PX.Objects.CR
{
	public class CRCaseMaint : PXGraph<CRCaseMaint, CRCase>
	{
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

		[PXHidden]
		public PXSelect<Contact>
			Contacts;

		[PXViewName(Messages.Case)]
		public PXSelect<CRCase>
			Case;

		[PXHidden]
		public PXSelect<CRCase,
			Where<CRCase.caseID, Equal<Current<CRCase.caseID>>>>
			CaseCurrent;

		[PXHidden]
		public PXSetup<CRCaseClass, Where<CRCaseClass.caseClassID, Equal<Optional<CRCase.caseClassID>>>> Class;

		[PXViewName(Messages.Answers)]
		public CRAttributeList<CRCase>
			Answers;

		[PXViewName(Messages.Activities)]
		[PXFilterable]
		[CRBAccountReference(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<CRCase.customerID>>>>))]
		public CRActivityList<CRCase>
			Activities;

		[PXViewName(Messages.CaseReferences)]
		[PXViewDetailsButton(typeof(CRCase), 
			typeof(Select<CRCase, Where<CRCase.caseID, Equal<Current<CRCaseReference.childCaseID>>>>))]
		public PXSelectJoin<CRCaseReference, 
			LeftJoin<CRCase, On<CRCase.caseID, Equal<CRCaseReference.childCaseID>>>>
			CaseRefs;

		[PXViewName(Messages.Relations)]
		[PXFilterable]
		public CRRelationsList<CRCase.noteID>
			Relations;

		#endregion

		#region Ctors

		public CRCaseMaint()
		{
			if (string.IsNullOrEmpty(Setup.Current.CaseNumberingID))
			{
				throw new PXSetPropertyException(Messages.NumberingIDIsNull, Messages.CRSetup);
			}

			PXUIFieldAttribute.SetRequired<CRCase.caseClassID>(Case.Cache, true);

			Activities.GetNewEmailAddress =
				() =>
				{
					var current = Case.Current;
					if (current != null)
					{
						var contact = current.ContactID.
							With(_ => (Contact)PXSelect<Contact, 
								Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
							Select(this, _.Value));
						if (contact != null && !string.IsNullOrWhiteSpace(contact.EMail))
							return new Email(contact.DisplayName, contact.EMail);

						var customerContact = current.CustomerID.
							With(_ => (PXResult<Contact, BAccount>)PXSelectJoin<Contact,
								InnerJoin<BAccount, On<BAccount.defContactID, Equal<Contact.contactID>>>,
								Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
							Select(this, _.Value)).
							With(_ => (Contact)_);
						if (customerContact != null && !string.IsNullOrWhiteSpace(customerContact.EMail))
							return new Email(customerContact.DisplayName, customerContact.EMail);
					}
					return Email.Empty;
				};

			if (!this.Views.Caches.Contains(typeof(EPActivity)))
				this.Views.Caches.Add(typeof(EPActivity));
			var bAccountCache = Caches[typeof(BAccount)];
			PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(bAccountCache, Messages.BAccountCD);
			PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(bAccountCache, Messages.BAccountName);
		}

		#endregion

		#region Data Handlers

		protected virtual IEnumerable caseRefs()
		{
			var currentCaseId = Case.Current.With(_ => _.CaseID);
			if (currentCaseId == null) yield break;

			var ht = new HybridDictionary();
			foreach (CRCaseReference item in
				PXSelect<CRCaseReference,
					Where<CRCaseReference.parentCaseID, Equal<Required<CRCaseReference.parentCaseID>>>>.
				Select(this, currentCaseId))
			{
				var childCaseId = item.ChildCaseID ?? 0;
				if (ht.Contains(childCaseId)) continue;

				ht.Add(childCaseId, item);
				var relCase = SelectCase(childCaseId);
				yield return new PXResult<CRCaseReference, CRCase>(item, relCase);
			}

			var cache = CaseRefs.Cache;
			var oldIsDirty = cache.IsDirty;
			foreach (CRCaseReference item in 
				PXSelect<CRCaseReference,
					Where<CRCaseReference.childCaseID, Equal<Required<CRCaseReference.childCaseID>>>>.
				Select(this, currentCaseId))
			{
				var parentCaseId = item.ParentCaseID ?? 0;
				if (ht.Contains(parentCaseId)) continue;

				ht.Add(parentCaseId, item);
				cache.Delete(item);
				var newItem = (CRCaseReference)cache.CreateInstance();
				newItem.ParentCaseID = currentCaseId;
				newItem.ChildCaseID = parentCaseId;

			    switch (item.RelationType)
			    {
                    case CaseRelationTypeAttribute._DEPENDS_ON_VALUE:
			            newItem.RelationType = CaseRelationTypeAttribute._BLOCKS_VALUE;
                        break;
                    case CaseRelationTypeAttribute._DUBLICATE_OF_VALUE:
			            newItem.RelationType = CaseRelationTypeAttribute._DUBLICATE_OF_VALUE;
                        break;
                    case CaseRelationTypeAttribute._RELATED_VALUE:
			            newItem.RelationType = CaseRelationTypeAttribute._RELATED_VALUE;
                        break;
                    default:
			            newItem.RelationType = CaseRelationTypeAttribute._DEPENDS_ON_VALUE;
			            break;
			    }
				
				newItem = (CRCaseReference)cache.Insert(newItem);
				cache.IsDirty = oldIsDirty;
				var relCase = SelectCase(parentCaseId);
				yield return new PXResult<CRCaseReference, CRCase>(newItem, relCase);
			}
		}

		/*public virtual IEnumerable casereferencesdependson()
		{
			var idsHashtable = new Hashtable();
			foreach (RelatedCase item in CaseReferencesDependsOn.Cache.Cached)
			{
				idsHashtable.Add(item.CaseID, item);
				var status = CaseReferencesDependsOn.Cache.GetStatus(item);
				if (status == PXEntryStatus.Inserted || status == PXEntryStatus.Updated || status == PXEntryStatus.Notchanged)
				{
					var @case = (CRCase)PXSelect<CRCase, Where<CRCase.caseID, Equal<Required<CRCase.caseID>>>>.Select(this, item.CaseID);
					yield return new PXResult<RelatedCase, CRCase>(item, @case);
				}
			}
			foreach (PXResult<CRCaseReference, CRCase> item in
				PXSelectJoin<CRCaseReference,
				InnerJoin<CRCase, On<CRCaseReference.childCaseID, Equal<CRCase.caseID>>>,
				Where<CRCaseReference.parentCaseID, Equal<Current<CRCase.caseID>>>>.
				Select(this))
			{
				var @case = (CRCase)item;
				if (idsHashtable.ContainsKey(@case.CaseID)) continue;

				yield return new PXResult<RelatedCase, CRCase>(
					new RelatedCase
						{
							CaseID = @case.CaseID,
							RelationType = CaseRelationTypeAttribute._DEPENDS_ON_VALUE
						},
					@case);
			}

			foreach (PXResult<CRCaseReference, CRCase> item in
				PXSelectJoin<CRCaseReference,
				InnerJoin<CRCase, On<CRCaseReference.parentCaseID, Equal<CRCase.caseID>>>,
				Where<CRCaseReference.childCaseID, Equal<Current<CRCase.caseID>>>>.
				Select(this))
			{
				var @case = (CRCase)item;
				if (idsHashtable.ContainsKey(@case.CaseID)) continue;

				yield return new PXResult<RelatedCase, CRCase>(
					new RelatedCase
						{
							CaseID = @case.CaseID,
							RelationType = CaseRelationTypeAttribute._BLOCKS_VALUE
						},
					@case);
			}
		}*/

		/*public override void Persist()
		{
			CorrectRelatedCaseRecords();

			base.Persist();
		}*/

		#endregion

		#region Actions

		public new PXSave<CRCase> Save;
		public new PXCancel<CRCase> Cancel;
		public new PXInsert<CRCase> Insert;
		public new PXCopyPasteAction<CRCase> CopyPaste;
		public new PXDelete<CRCase> Delete;
		public new PXFirst<CRCase> First;
		public new PXPrevious<CRCase> Previous;
		public new PXNext<CRCase> Next;
		public new PXLast<CRCase> Last;

		public PXAction<CRCase> release;
		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			List<CRCase> list = new List<CRCase>(adapter.Get().Cast<CRCase>());
			Save.Press();

			PXLongOperation.StartOperation(this, delegate
				{
					foreach (CRCase @case in list)
					{
						CRCaseMaint graph = PXGraph.CreateInstance<CRCaseMaint>();
						if (@case == null || @case.Released == true) continue;
						graph.CheckBillingSettings(@case);
						graph.ReleaseCase(@case);
					}
				});

			return adapter.Get();
		}

		public PXMenuAction<CRCase> Action;
		public PXMenuInquiry<CRCase> Inquiry;

		public PXAction<CRCase> takeCase;
		[PXUIField(DisplayName = "Take Case", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual void TakeCase()
		{
			var caseCur = Case.Cache.CreateCopy(Case.Current) as CRCase;
			if (caseCur == null) return;

			caseCur.OwnerID = EmployeeMaint.GetCurrentEmployeeID(this);
			if (caseCur.WorkgroupID != null)
			{
				EPCompanyTreeMember member = PXSelect<EPCompanyTreeMember, 
												Where<EPCompanyTreeMember.userID, Equal<Current<AccessInfo.userID>>, 
												  And<EPCompanyTreeMember.workGroupID, Equal<Required<CRCase.workgroupID>>>>>.
				Select(this, caseCur.WorkgroupID);

				if (member == null)
				{
					caseCur.WorkgroupID = null;
				}
			}
			if (caseCur.OwnerID != Case.Current.OwnerID || caseCur.WorkgroupID != Case.Current.WorkgroupID)
			{
				Case.Update(caseCur);
				Save.Press();
			}
		}

		public PXAction<CRCase> assign;
		[PXUIField(DisplayName = Messages.Assign, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable Assign(PXAdapter adapter)
		{
			if (!Setup.Current.DefaultCaseAssignmentMapID.HasValue)
			{
				throw new PXSetPropertyException(Messages.AssignNotSetupCase, "CR Setup");
			}

			EPAssignmentProcessHelper<CRCase> aph = new EPAssignmentProcessHelper<CRCase>(this);
			aph.Assign(CaseCurrent.Current, Setup.Current.DefaultCaseAssignmentMapID);

			CaseCurrent.Update(CaseCurrent.Current);

			return adapter.Get();
		}

		public PXAction<CRCase> viewInvoice;
		[PXUIField(DisplayName = Messages.ViewInvoice, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Inquiry)]
		public virtual IEnumerable ViewInvoice(PXAdapter adapter)
		{
			if (CaseCurrent.Current != null && !string.IsNullOrEmpty(CaseCurrent.Current.ARRefNbr))
			{
				ARInvoiceEntry target = PXGraph.CreateInstance<ARInvoiceEntry>();
				target.Clear();
				target.Document.Current = target.Document.Search<ARInvoice.refNbr>(CaseCurrent.Current.ARRefNbr);
				throw new PXRedirectRequiredException(target, "ViewInvoice");
			}

			return adapter.Get();
		}

		#endregion

		#region Event Handlers

		#region Contacts

		[CustomerProspectVendor(DisplayName = "Business Account", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual void Contact_BAccountID_CacheAttached(PXCache sender)
		{

		}

		#endregion

		#region CRCase

		[PXDBString(2, IsFixed = true)]
		[CRCaseResolutions]
		[PXUIField(DisplayName = "Reason")]
		[CRDropDownAutoValue(typeof(CRCase.status))]
		public virtual void CRCase_Resolution_CacheAttached(PXCache sender) { }

		[CRCaseBillableTime]
		[PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
		[PXUIField(DisplayName = "Billable Time", Enabled = false)]
		public virtual void CRCase_TimeBillable_CacheAttached(PXCache sender) { }

        [PXDBInt]
        [PXUIField(DisplayName = "Contract")]
        [PXSelector(typeof(Search2<Contract.contractID,
                LeftJoin<ContractBillingSchedule, On<Contract.contractID, Equal<ContractBillingSchedule.contractID>>>,
            Where<Contract.isTemplate, NotEqual<True>,
                And<Contract.baseType, Equal<Contract.ContractBaseType>,
                And<Where<Current<CRCase.customerID>, IsNull,
                        Or2<Where<Contract.customerID, Equal<Current<CRCase.customerID>>,
                            And<Current<CRCase.locationID>, IsNull>>,
                        Or2<Where<ContractBillingSchedule.accountID, Equal<Current<CRCase.customerID>>,
                            And<Current<CRCase.locationID>, IsNull>>,
                        Or2<Where<Contract.customerID, Equal<Current<CRCase.customerID>>,
                            And<Contract.locationID, Equal<Current<CRCase.locationID>>>>,
                        Or<Where<ContractBillingSchedule.accountID, Equal<Current<CRCase.customerID>>,
                            And<ContractBillingSchedule.locationID, Equal<Current<CRCase.locationID>>>>>>>>>>>>,
            OrderBy<Desc<Contract.contractCD>>>),
            DescriptionField = typeof(Contract.description),
            SubstituteKey = typeof(Contract.contractCD), Filterable = true)]
        [PXRestrictor(typeof(Where<Contract.status, Equal<ContractStatus.ContractStatusActivated>, Or<Contract.status, Equal<ContractStatus.ContractStatusUpgrade>>>), Messages.ContractIsNotActive)]
        [PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, LessEqual<Contract.graceDate>, Or<Contract.expireDate, IsNull>>), Messages.ContractExpired)]
        [PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, GreaterEqual<Contract.startDate>>), Messages.ContractActivationDateInFuture, typeof(Contract.startDate))]
        [PXFormula(typeof(Default<CRCase.customerID>))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual void CRCase_ContractID_CacheAttached(PXCache sender) { }

		protected virtual void CRCase_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var caseRow = e.Row as CRCase;
			if (caseRow == null) return;

			Activities.DefaultSubject = string.Format(Messages.CaseEmailDefaultSubject, caseRow.CaseCD, caseRow.Subject);

			var caseClass = PXSelectorAttribute.Select<CRCase.caseClassID>(cache, e.Row) as CRCaseClass;
			if (caseClass != null)
			{
				Activities.DefaultEMailAccountId = caseClass.DefaultEMailAccountID;
			}

			var perItemBilling = false;
			var denyOverrideBillable = false;
			if (caseClass != null)
			{
				denyOverrideBillable = caseClass.AllowOverrideBillable != true;
				perItemBilling = caseClass.PerItemBilling == BillingTypeListAttribute.PerActivity;
			}

			if (caseRow.IsBillable != true)
				caseRow.ManualBillableTimes = false;

			var isNotReleased = caseRow.Released != true;
			if (isNotReleased)
			{
				PXUIFieldAttribute.SetEnabled<CRCase.manualBillableTimes>(cache, caseRow, caseRow.IsBillable == true);
				PXUIFieldAttribute.SetEnabled<CRCase.isBillable>(cache, caseRow, !perItemBilling && !denyOverrideBillable);
				var canModifyBillableTimes = caseRow.IsBillable == true && caseRow.ManualBillableTimes == true;
				PXUIFieldAttribute.SetEnabled<CRCase.timeBillable>(cache, caseRow, canModifyBillableTimes);
				PXUIFieldAttribute.SetEnabled<CRCase.overtimeBillable>(cache, caseRow, canModifyBillableTimes);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(cache, caseRow, false);
			}
			PXUIFieldAttribute.SetEnabled<CRCase.caseCD>(cache, caseRow, true);

			viewInvoice.SetEnabled(!String.IsNullOrEmpty(caseRow.ARRefNbr));
			release.SetEnabled(isNotReleased && caseRow.IsBillable == true && !perItemBilling);
			Activities.Cache.AllowInsert = isNotReleased;

			PXDefaultAttribute.SetPersistingCheck<CRCase.customerID>(cache, caseRow, (caseRow.IsBillable == true || (caseClass != null && caseClass.RequireCustomer == true)) ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<CRCase.contractID>(cache, caseRow, (caseClass != null && PXAccess.FeatureInstalled<CS.FeaturesSet.contractManagement>() && caseClass.RequireContract == true) ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<CRCase.contactID>(cache, caseRow, (caseClass != null && caseClass.RequireContact == true) ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
		}

		protected virtual void CRCase_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			var oldRow = e.Row as CRCase;
			var row = e.NewRow as CRCase;
			if (oldRow == null || row == null) return;

			if (oldRow.Status != row.Status && oldRow.Resolution == row.Resolution)
				row.Resolution = null;
		}

		protected virtual void CRCase_SLAETA_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CRCase row = e.Row as CRCase;
			if (row == null || row.CreatedDateTime == null) return;

			if (row.ClassID != null && row.Severity != null)
			{
				var severity = (CRClassSeverityTime)PXSelect<CRClassSeverityTime,
														Where<CRClassSeverityTime.caseClassID, Equal<Required<CRClassSeverityTime.caseClassID>>,
														And<CRClassSeverityTime.severity, Equal<Required<CRClassSeverityTime.severity>>>>>.
														Select(this, row.ClassID, row.Severity);
				if (severity != null && severity.TimeReaction != null)
				{
					e.NewValue = ((DateTime)row.CreatedDateTime).AddMinutes((int)severity.TimeReaction);
					e.Cancel = true;
				}
			}

			if (row.Severity != null && row.ContractID != null)
			{
				var template = (Contract)PXSelect<Contract, Where<Contract.contractID, Equal<Required<CRCase.contractID>>>>.Select(this, row.ContractID);
				if (template == null) return;
				
				var sla = (ContractSLAMapping)PXSelect<ContractSLAMapping,
												  Where<ContractSLAMapping.severity, Equal<Required<CRCase.severity>>,
												  And<ContractSLAMapping.contractID, Equal<Required<CRCase.contractID>>>>>.
												  Select(this, row.Severity, template.TemplateID);
				if (sla != null && sla.Period != null)
				{
					e.NewValue = ((DateTime)row.CreatedDateTime).AddMinutes((int)sla.Period);
					e.Cancel = true;
				}
			}
		}

		protected virtual void CRCase_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as CRCase;
			var oldRow = e.OldRow as CRCase;
			if (row == null || oldRow == null) return;

			if (row.OwnerID == null)
			{
				row.AssignDate = null;
			}
			else if (oldRow.OwnerID == null)
			{
				row.AssignDate = PXTimeZoneInfo.Now;
			}

			if (row.MajorStatus == CRCaseMajorStatusesAttribute._CLOSED && oldRow.MajorStatus != CRCaseMajorStatusesAttribute._CLOSED)
			{
				row.ResolutionDate = PXTimeZoneInfo.Now;
			}
			else if (row.MajorStatus < CRCaseMajorStatusesAttribute._CLOSED && oldRow.MajorStatus == CRCaseMajorStatusesAttribute._CLOSED)
			{
				row.ResolutionDate = null;
			}
		}

		protected virtual void CRCase_ContactID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var row = e.Row as CRCase;			
			if (row == null || row.CustomerID == null) return;
			
			var contactsSet = PXSelectJoin<Contact,
				InnerJoin<BAccount,
					On<BAccount.bAccountID, Equal<Contact.bAccountID>,
						And<BAccount.defContactID, NotEqual<Contact.contactID>>>>,
				Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>,
				And<Contact.contactType, NotEqual<ContactTypesAttribute.bAccountProperty>>>>.
				SelectWindowed(this, 0, 2, row.CustomerID);
									
			if (contactsSet != null && contactsSet.Count == 1)
			{
				e.NewValue = ((Contact)contactsSet[0]).ContactID;
				e.Cancel = true;
			}				
			else if (contactsSet != null && row.ContactID != null && contactsSet.Any(contact => PXResult.Unwrap<Contact>(contact).ContactID == row.ContactID))
			{
				//Keep previous contact.
				throw new PXSetPropertyException<CRCase.contactID>(string.Empty);
			}
		}

		protected virtual void CRCase_ContractID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CRCase row = e.Row as CRCase;
			if (row == null || row.CustomerID == null) return;

			List<object> contracts = PXSelectorAttribute.SelectAll<CRCase.contractID>(sender, e.Row);
			if (contracts.Exists(contract => PXResult.Unwrap<Contract>(contract).ContractID == row.ContractID))
			{
				e.NewValue = row.ContractID;
			}
			else if (contracts.Count == 1)
			{
				e.NewValue = PXResult.Unwrap<Contract>(contracts[0]).ContractID;
			}
			e.Cancel = true;
		}

		protected virtual void CRCase_ContractID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CRCase crcase = (CRCase)e.Row;
			Contract contract = PXResult.Unwrap<Contract>(PXSelectorAttribute.Select<CRCase.contractID>(sender, e.Row, e.NewValue));
			if (crcase == null || contract == null) return;

			int daysLeft;
			if (Accessinfo.BusinessDate != null
				&& ContractMaint.IsInGracePeriod(contract, (DateTime)Accessinfo.BusinessDate, out daysLeft))
			{
				sender.RaiseExceptionHandling<CRCase.contractID>(crcase, e.NewValue, new PXSetPropertyException(Messages.ContractInGracePeriod, PXErrorLevel.Warning, daysLeft));
			}
		}
		#endregion

		#region CRCaseReference

		[PXDBInt(IsKey = true)]
		[PXDBLiteDefault(typeof(CRCase.caseID))]
		[PXUIField(Visible = false)]
		public virtual void CRCaseReference_ParentCaseID_CacheAttached(PXCache sender)
		{
			
		}

		protected virtual void CRCaseReference_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			var row = e.Row as CRCaseReference;
			if (row == null || row.ChildCaseID == null || row.ParentCaseID == null) return;

			var alternativeRecord = (CRCaseReference)PXSelect<CRCaseReference,
				Where<CRCaseReference.parentCaseID, Equal<Required<CRCaseReference.parentCaseID>>,
					And<CRCaseReference.childCaseID, Equal<Required<CRCaseReference.childCaseID>>>>>.
				Select(this, row.ChildCaseID, row.ParentCaseID);
			if (alternativeRecord != null)
				sender.Delete(alternativeRecord);
		}

        protected virtual void CRCaseReference_RelationType_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            
        }

		protected virtual void CRCaseReference_ChildCaseID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var row = e.Row as CRCaseReference;
			if (row == null || e.NewValue == null) return;

			if (object.Equals(row.ParentCaseID, e.NewValue))
			{
				e.Cancel = true;
				throw new PXSetPropertyException(Messages.CaseCannotDependUponItself);
			}
		}

		#endregion
		#endregion

		#region CacheAttached

		[PXBool]
		[PXDefault(false)]
		[PXDBCalced(typeof(True), typeof(Boolean))]
		protected virtual void BAccount_ViewInCrm_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region Private Methods

		private CRCase SelectCase(object caseId)
		{
			if (caseId == null) return null;

			return (CRCase)PXSelect<CRCase,
				Where<CRCase.caseID, Equal<Required<CRCase.caseID>>>>.
				Select(this, caseId);
		}
        
		protected virtual void ReleaseCase(CRCase item)
		{
            RegisterEntry registerEntry = (RegisterEntry)PXGraph.CreateInstance(typeof(RegisterEntry));

            PXSelectBase<EPActivityApprove> select = new PXSelect<EPActivityApprove,
                Where<EPActivityApprove.refNoteID, Equal<Required<EPActivityApprove.refNoteID>>>>(this);

            List<EPActivityApprove> list = new List<EPActivityApprove>();
            foreach (EPActivityApprove activity in select.Select(item.NoteID))
            {
                list.Add(activity);

				if ((activity.TimeSpent.GetValueOrDefault() != 0 || activity.TimeBillable.GetValueOrDefault() != 0) && activity.ApproverID != null && activity.ApprovalStatus != ActivityStatusListAttribute.Completed && activity.UIStatus != ActivityStatusListAttribute.Canceled)
                {
                    throw new PXException(Messages.OneOrMoreActivitiesAreNotApproved);
                }
            }

            if (item.ContractID != null)
            {
                //Contract Billing:

                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    RecordContractUsage(item);

                    if (!EmployeeActivitiesRelease.RecordCostTrans(registerEntry, list))
                    {
                        throw new PXException("Failed to Record cost transactions to PM");
                    }
										item.Released = true;
										Case.Update(item);
										this.Save.Press();
                    ts.Complete();
                }
            }
            else
            {
                //Direct Billing:
                Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, item.CustomerID);
                if (customer == null)
                {
                    throw new PXException("Customer not found. Customer is required for the case to be billed.");
                }

                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    ARInvoiceEntry invoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
                    invoiceEntry.FieldVerifying.AddHandler<ARInvoice.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
                    invoiceEntry.FieldVerifying.AddHandler<ARTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

                    ARInvoice invoice = (ARInvoice)invoiceEntry.Caches[typeof(ARInvoice)].CreateInstance();
                    invoice.DocType = ARDocType.Invoice;
                    invoice = (ARInvoice)invoiceEntry.Caches[typeof(ARInvoice)].Insert(invoice);
                    invoice.CustomerID = item.CustomerID;
                    invoice.CustomerLocationID = item.LocationID;
                    invoice.DocDate = Accessinfo.BusinessDate;
					invoice.DocDesc = item.Subject;
					invoice = invoiceEntry.Document.Update(invoice);

					invoiceEntry.Caches[typeof(ARInvoice)].SetValueExt<ARInvoice.hold>(invoice, false);                    
					invoiceEntry.customer.Current.CreditRule = customer.CreditRule;
					invoice = invoiceEntry.Document.Update(invoice);
                    foreach (ARTran tran in GenerateARTrans(item))
                    {
                        invoiceEntry.Transactions.Insert(tran);
                    }

                    invoiceEntry.Actions.PressSave();

                    item.Released = true;
                    item.Released = true;
                    item.ARRefNbr = invoiceEntry.Document.Current.RefNbr;
                    Case.Update(item);
                    this.Save.Press();

                    if (!EmployeeActivitiesRelease.RecordCostTrans(registerEntry, list))
                    {
                        throw new PXException("Failed to Record cost transactions to PM");
                    }


                    ts.Complete();
                }
            }

            if (registerEntry.Transactions.Select().Count > 0)//there can be no cost transactions at all - they were created when a timecard was released.
            {
                EPSetup setup = PXSelect<EPSetup>.Select(registerEntry);

                if (setup != null && setup.AutomaticReleasePM == true)
                {
                    PX.Objects.PM.RegisterRelease.Release(registerEntry.Document.Current);
                }
            }
		}
        
        protected virtual void RecordContractUsage(CRCase item)
        {
            RegisterEntry registerEntry = PXGraph.CreateInstance<RegisterEntry>();
            registerEntry.FieldVerifying.AddHandler<PMTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
            registerEntry.FieldVerifying.AddHandler<PMTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });//restriction should be applicable only for budgeting.
            registerEntry.Document.Cache.Insert();
            registerEntry.Document.Current.Description = item.Subject;
            registerEntry.Document.Current.Released = true;
            registerEntry.Views.Caches.Add(typeof(EPActivity));

           
            foreach (PMTran tran in GeneratePMTrans(item))
            {
                registerEntry.Transactions.Insert(tran);
            }

            item.Released = true;
            Case.Update(item);

            registerEntry.Save.Press();
        }
        
		public override object GetValueExt(string viewName, object data, string fieldName)
		{
			object ret = base.GetValueExt(viewName, data, fieldName);
			if (String.Equals(viewName, "CaseCurrent", StringComparison.OrdinalIgnoreCase) && String.Equals(fieldName, "CustomerID", StringComparison.OrdinalIgnoreCase) && ret is PXFieldState && !String.IsNullOrEmpty(((PXFieldState)ret).Error))
			{
				((PXFieldState)ret).Error = null;
			}
			return ret;
		}
        
        protected virtual List<PMTran> GeneratePMTrans(CRCase c)
		{
            Contract contract = PXSelect<Contract,
				Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.
				Select(this, c.ContractID);

            Contract template = PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(this, contract.TemplateID);

            CRCaseClass caseClass = PXSelect<CRCaseClass, Where<CRCaseClass.caseClassID, Equal<Required<CRCaseClass.caseClassID>>>>.Select(this, c.CaseClassID);

			List<PMTran> result = new List<PMTran>();

			DateTime startDate = (DateTime)c.CreatedDateTime;
			DateTime endDate = startDate.Add(new TimeSpan(0, (c.TimeBillable ?? 0), 0));

			PXResultset<EPActivity> list = PXSelect<EPActivity,
				Where<EPActivity.refNoteID, Equal<Required<EPActivity.refNoteID>>>,
				OrderBy<Desc<EPActivity.createdDateTime>>>.
				Select(this, c.NoteID);

			#region For Case without activities
			if (list.Count > 0)
			{
				startDate = (DateTime)((EPActivity)list[0]).StartDate;
				endDate = startDate;
			}
			#endregion

			PXCache cache = null;
			foreach (EPActivity activity in list)
			{
				if (cache == null) cache = Caches[activity.GetType()];
				if (activity.ClassID == CRActivityClass.Activity && activity.IsBillable == true)
				{
					if (activity.StartDate != null && (DateTime)activity.StartDate < startDate)
					{
						startDate = (DateTime)activity.StartDate;
					}

					if (activity.EndDate != null && (DateTime)activity.EndDate > endDate)
					{
						endDate = (DateTime)activity.EndDate;
					}
					activity.Billed = true;
				}
				activity.Released = true;				
				cache.Update(activity);
			}

            if (template.CaseItemID != null)
			{
				InventoryItem item = PXSelect<InventoryItem,
					Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.
                    Select(this, template.CaseItemID);

				PMTran newTran = new PMTran();
				newTran.ProjectID = contract.ContractID;
                newTran.InventoryID = template.CaseItemID;
				newTran.AccountGroupID = contract.ContractAccountGroup;
				newTran.OrigRefID = c.NoteID;
				newTran.BAccountID = c.CustomerID;
				newTran.LocationID = c.LocationID;
				newTran.Description = c.Subject;
				newTran.StartDate = startDate;
				newTran.EndDate = endDate;
			    newTran.Date = endDate;
				newTran.Qty = 1;
				newTran.BillableQty = 1;
				newTran.UOM = item.SalesUnit;
				newTran.Released = true;
				newTran.Allocated = true;
				newTran.BillingID = contract.BillingID;
				newTran.IsQtyOnly = true;
				result.Add(newTran);
			}

			#region Record Labor Usage
            if (caseClass.LabourItemID != null)
			{
				int totalBillableMinutes = (c.TimeBillable ?? 0);

                if (caseClass.OvertimeItemID == null)
				{
					//append overtime to billable
					totalBillableMinutes += (c.OvertimeBillable ?? 0);
				}

				if (totalBillableMinutes > 0)
				{
					if (caseClass.PerItemBilling == BillingTypeListAttribute.PerCase && caseClass.RoundingInMinutes > 1)
					{
						decimal fraction = Convert.ToDecimal(totalBillableMinutes) / Convert.ToDecimal(caseClass.RoundingInMinutes);
						int points = Convert.ToInt32(Math.Ceiling(fraction));
						totalBillableMinutes = points * (caseClass.RoundingInMinutes ?? 0);
					}

					if (caseClass.PerItemBilling == BillingTypeListAttribute.PerCase && caseClass.MinBillTimeInMinutes > 0)
					{
						totalBillableMinutes = Math.Max(totalBillableMinutes, (int)caseClass.MinBillTimeInMinutes);
					}

					InventoryItem item = PXSelect<InventoryItem,
						Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.
                        Select(this, caseClass.LabourItemID);
					PMTran newLabourTran = new PMTran();
					newLabourTran.ProjectID = contract.ContractID;
                    newLabourTran.InventoryID = caseClass.LabourItemID;
					newLabourTran.AccountGroupID = contract.ContractAccountGroup;
					newLabourTran.OrigRefID = c.NoteID;
					newLabourTran.BAccountID = c.CustomerID;
					newLabourTran.LocationID = c.LocationID;
					newLabourTran.Description = c.Subject;
					newLabourTran.StartDate = startDate;
					newLabourTran.EndDate = endDate;
                    newLabourTran.Date = endDate;
					newLabourTran.UOM = item.SalesUnit;
					newLabourTran.Qty = Convert.ToDecimal(TimeSpan.FromMinutes(totalBillableMinutes).TotalHours);
					newLabourTran.BillableQty = newLabourTran.Qty;
					newLabourTran.Released = true;
					newLabourTran.Allocated = true;
					newLabourTran.BillingID = contract.BillingID;
					newLabourTran.IsQtyOnly = true;
					result.Add(newLabourTran);
				}
			}
			#endregion

			#region Record Overtime Usage

            if (caseClass.OvertimeItemID.HasValue)
			{
				InventoryItem item = PXSelect<InventoryItem,
					Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.
                    Select(this, caseClass.OvertimeItemID);
				int totalOvertimeBillableMinutes = (c.OvertimeBillable ?? 0);

				if (totalOvertimeBillableMinutes > 0)
				{
					if (caseClass.PerItemBilling == BillingTypeListAttribute.PerCase && caseClass.RoundingInMinutes > 1)
					{
						decimal fraction = Convert.ToDecimal(totalOvertimeBillableMinutes) / Convert.ToDecimal(caseClass.RoundingInMinutes);
						int points = Convert.ToInt32(Math.Ceiling(fraction));
						totalOvertimeBillableMinutes = points * (caseClass.RoundingInMinutes ?? 0);
					}

					PMTran newOvertimeTran = new PMTran();
					newOvertimeTran.ProjectID = contract.ContractID;
                    newOvertimeTran.InventoryID = caseClass.OvertimeItemID;
					newOvertimeTran.AccountGroupID = contract.ContractAccountGroup;
					newOvertimeTran.OrigRefID = c.NoteID;
					newOvertimeTran.BAccountID = c.CustomerID;
					newOvertimeTran.LocationID = c.LocationID;
					newOvertimeTran.Description = c.Subject;
					newOvertimeTran.StartDate = startDate;
					newOvertimeTran.EndDate = endDate;
                    newOvertimeTran.Date = endDate;
					newOvertimeTran.Qty = Convert.ToDecimal(TimeSpan.FromMinutes(totalOvertimeBillableMinutes).TotalHours);
					newOvertimeTran.BillableQty = newOvertimeTran.Qty;
					newOvertimeTran.UOM = item.SalesUnit;
					newOvertimeTran.Released = true;
					newOvertimeTran.Allocated = true;
					newOvertimeTran.BillingID = contract.BillingID;
					newOvertimeTran.IsQtyOnly = true;
					result.Add(newOvertimeTran);
				}
			}

			#endregion


			return result;
		}

        protected virtual List<ARTran> GenerateARTrans(CRCase c)
        {
            CRCaseClass caseClass = PXSelect<CRCaseClass, Where<CRCaseClass.caseClassID, Equal<Required<CRCaseClass.caseClassID>>>>.Select(this, c.CaseClassID);

            List<ARTran> result = new List<ARTran>();

            DateTime startDate = (DateTime)c.CreatedDateTime;
            DateTime endDate = startDate.Add(new TimeSpan(0, (c.TimeBillable ?? 0), 0));

            PXResultset<EPActivity> list = PXSelect<EPActivity,
                Where<EPActivity.refNoteID, Equal<Required<EPActivity.refNoteID>>>,
                OrderBy<Desc<EPActivity.createdDateTime>>>.
                Select(this, c.NoteID);

            #region For Case without activities
            if (list.Count > 0)
            {
                startDate = (DateTime)((EPActivity)list[0]).StartDate;
                endDate = startDate;
            }
            #endregion

            PXCache cache = null;
            foreach (EPActivity activity in list)
            {
                if (cache == null) cache = Caches[activity.GetType()];
                if (activity.ClassID == CRActivityClass.Activity && activity.IsBillable == true)
                {
                    if (activity.StartDate != null && (DateTime)activity.StartDate < startDate)
                    {
                        startDate = (DateTime)activity.StartDate;
                    }

                    if (activity.EndDate != null && (DateTime)activity.EndDate > endDate)
                    {
                        endDate = (DateTime)activity.EndDate;
                    }
                    activity.Billed = true;
                }
                activity.Released = true;
                cache.Update(activity);
            }
            
            #region Record Labor Usage
            if (caseClass.LabourItemID != null)
            {
                int totalBillableMinutes = (c.TimeBillable ?? 0);

                if (caseClass.OvertimeItemID == null)
                {
                    //append overtime to billable
                    totalBillableMinutes += (c.OvertimeBillable ?? 0);
                }

                if (totalBillableMinutes > 0)
                {
                    if (caseClass.RoundingInMinutes > 1)
                    {
                        decimal fraction = Convert.ToDecimal(totalBillableMinutes) / Convert.ToDecimal(caseClass.RoundingInMinutes);
                        int points = Convert.ToInt32(Math.Ceiling(fraction));
                        totalBillableMinutes = points * (caseClass.RoundingInMinutes ?? 0);
                    }

                    if (caseClass.MinBillTimeInMinutes > 0)
                    {
                        totalBillableMinutes = Math.Max(totalBillableMinutes, (int)caseClass.MinBillTimeInMinutes);
                    }

                    InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, caseClass.LabourItemID);
                    ARTran newLabourTran = new ARTran();
                    newLabourTran.InventoryID = caseClass.LabourItemID;
                    newLabourTran.TranDesc = c.Subject;
                    newLabourTran.UOM = item.SalesUnit;
                    newLabourTran.Qty = Convert.ToDecimal(TimeSpan.FromMinutes(totalBillableMinutes).TotalHours);
                    
                    result.Add(newLabourTran);
                }
            }
            #endregion

            #region Record Overtime Usage

            if (caseClass.OvertimeItemID.HasValue)
            {
                InventoryItem item = PXSelect<InventoryItem,
                    Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.
                    Select(this, caseClass.OvertimeItemID);
                int totalOvertimeBillableMinutes = (c.OvertimeBillable ?? 0);

                if (totalOvertimeBillableMinutes > 0)
                {
                    if (caseClass.RoundingInMinutes > 1)
                    {
                        decimal fraction = Convert.ToDecimal(totalOvertimeBillableMinutes) / Convert.ToDecimal(caseClass.RoundingInMinutes);
                        int points = Convert.ToInt32(Math.Ceiling(fraction));
                        totalOvertimeBillableMinutes = points * (caseClass.RoundingInMinutes ?? 0);
                    }

                    ARTran newOvertimeTran = new ARTran();
                    newOvertimeTran.InventoryID = caseClass.OvertimeItemID;
                    newOvertimeTran.TranDesc = c.Subject;
                    newOvertimeTran.UOM = item.SalesUnit;
                    newOvertimeTran.Qty = Convert.ToDecimal(TimeSpan.FromMinutes(totalOvertimeBillableMinutes).TotalHours);
                    
                    result.Add(newOvertimeTran);
                }
            }

            #endregion


            return result;
        }

		private bool VerifyField<TField>(object row, object newValue)
			where TField : IBqlField
		{
			if (row == null) return true;

			var result = false;
			var cache = Caches[row.GetType()];
			try
			{
				result = cache.RaiseFieldVerifying<TField>(row, ref newValue);
			}
			catch (StackOverflowException) { throw; }
			catch (OutOfMemoryException) { throw; }
			catch (Exception) { }

			return result;
		}

		private void CheckBillingSettings(CRCase @case)
		{

			EPActivity activity = PXSelectReadonly<EPActivity,
				Where<EPActivity.isBillable, Equal<True>,
					And2<Where<EPActivity.uistatus, IsNull,
						Or<EPActivity.uistatus, Equal<ActivityStatusAttribute.open>>>,
					And<Where<EPActivity.refNoteID, Equal<Current<CRCase.noteID>>>>>>>.SelectSingleBound(this, new object[] { @case });
			if (activity != null)
			{
				throw new PXException(Messages.CloseCaseWithHoldActivities);
			}

			CRCaseClass caseClass = PXSelect<CRCaseClass, Where<CRCaseClass.caseClassID, Equal<Required<CRCaseClass.caseClassID>>>>.Select(this, @case.CaseClassID);


			if (caseClass.PerItemBilling == BillingTypeListAttribute.PerActivity)
			{
				throw new PXException(Messages.OnlyBillByActivity);
			}

		    if (@case.IsBillable == true)
		    {
		        if (@case.ContractID == null)
		        {
		            if (caseClass.LabourItemID == null)
		                throw new PXException(Messages.CaseClassDetailsIsNotSet);
		        }
		        else
		        {
		            Contract contract = PXSelect<Contract, Where<Contract.contractID, Equal<Current<CRCase.contractID>>>>.SelectSingleBound(this, new object[] {@case});
		            Contract template = PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(this, contract.TemplateID);

		            if (caseClass.LabourItemID == null && template.CaseItemID == null)
		            {
		                throw new PXException(Messages.CaseClassDetailsIsNotSet);
		            }
		        }
		    }

		}
		#endregion

		#region MailProcessor.IEntityHandler

		public virtual void Process(object row, EPActivity message)
		{
			var @case = row as CRCase;
			if (@case == null) return;

			if (@case.MajorStatus != CRCaseMajorStatusesAttribute._CLOSED &&
				@case.MajorStatus != CRCaseMajorStatusesAttribute._RELEASED &&
				@case.Released != true && @case.Status == CRCaseStatusesAttribute._PENDING_CUSTOMER)
			{
				Clear();
				SelectTimeStamp();

				@case.Status = CRCaseStatusesAttribute._OPEN;
				@case.Resolution = CRCaseResolutionsAttribute._CUSTOMER_REPLIED;
				Case.Cache.Update(@case);
				this.EnshureCachePersistance(typeof(CRCase));
				Save.Press();
				SelectTimeStamp();
			}
		}

		#endregion
	}
}
