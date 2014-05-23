using System;
using PX.Data;
using PX.Common;

namespace PX.Objects.CR
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		#region Validation and Processing Messages
		public const string BAccountIsType = "Business Account is {0}.";
		public const string Prefix = "CR Error";
		public const string SelectRecord = "You must select the record first.";
		public const string CustomerRequired = "Customer is required for this case. Please convert {0} to customer.";
		public const string CustomerIDRequired = "Customer ID is required.";
		public const string ContractIDRequired = "Contract ID is required.";
		public const string ContactIDRequired = "Contact ID is required.";
		public const string AccountNameIsRequired = "Account Name is required.";
		public const string OpportunityIsRequired = "Opportunity ID is required.";
		public const string CampaignIsReferenced = "This Campaign is referenced and cannot be deleted.";
		public const string AddNewCampaignMembers = "Add new Campaign members.";
		public const string CampaignDeleteMembersQuestion = "Do you want to delete selected Campaign members?";
		public const string ContractExpired = "Contract has expired.";
		public const string ContractIsNotActive = "Contract is not active.";
		public const string ContractInGracePeriod = "Selected Contract is on the grace period. {0} day(s) left before the expiration.";
		public const string CaseClassDetailsIsNotSet = "Cases of the given class cannot be billed. The 'Labor Item' and 'Overtime Class ID' must be specified in the Case Class if you want to bill for the hours. If you want to bill for the 'number of cases' set the Case Count Item in the Contract Template.";
		public const string ProspectNotCustomer = "Prospect Account must be converted to Customer Account prior the creating of Invoice or Sales Order.";
        public const string InvoiceAlreadyCreated = "One or more existing invoices have been found for this opportunity.";
		public const string InvoiceAlreadyCreatedDeleted = "Invoice document was created for this Opportunity but cannot be found with the system. Do you want to recreate it?";
		public const string OrderAlreadyCreated = "Sales Order documents already exists for this Opportunity.";
		public const string OrderAlreadyCreatedDeleted = "Sales Order document was created for this Opportunity but cannot be found with the system. Do you want to recreate it?";
		public const string OrderView = "Sales Order document was created for this Opportunity. Do you want to view it?";
		public const string CannotCompleteEmail = "Draft email cannot be completed.";
		public const string CannotDeleteTasksAndEvents = "There are some events or tasks, that cannot be deleted. Are you sure you whant to delete?";
		public const string ConfirmDeleteActivities = "There are some activities, that cannot be deleted. Are you sure you whant to delete?";
		public const string CannotDeleteActivity = "Billable, Time Card and currently email processing Activity can not be deleted.";
		public const string CannotDeleteDefaultLoc = "Default Business Account Location cannot be deleted.";
		public const string CannotDeleteDefaultContact = "Default Business Account Contact cannot be deleted.";
        public const string CannotReassignDefaultContact = "You cannot assign this contact to a different business account since it is used as a default contact.";
		public const string CannotCloseClosedCase = "Case already closed.";
		public const string BAccountChangeWarning = "Do you want to assign this contact to the different Business Account?";
		public const string ArgumentIsNullOrEmpty = "Argument is null or an empty string.";
		public const string SelectContactWithBAccount = "You cannot select the Contact that is not assigned to the Business Account.";
		public const string SelectContactForCreatedCase = "Please, select Contact for the created Case.";
		public const string BillableTimeCannotBeGreaterThanTimeSpent = "Time Billable cannot be greater than Time Spent.";
		public const string OvertimeBillableCannotBeGreaterThanOvertimeSpent = "Overtime Billable cannot be greater than the Overtime Spent.";
		public const string DurationActivityExceed24Hours = "Activity Duration cannot exceed 24 hours. Please split.";
		public const string FailedToSelectCalenderId = "Calendar with specified Calendar ID cannot be found.";
		public const string CaseCannotBeDeletedUseStatus = "This Case has one or more Task/Event/Activity record and cannot be deleted. Cancel or Close the Case instead.";
		public const string ContactCannotBeDeleted = "This Contact has one or more Task/Event/Activity record and cannot be deleted.";
		public const string LeadCannotBeDeleted = "This Lead has one or more Task/Event/Activity record and cannot be deleted.";
		public const string BAccountCannotBeDeleted = "This Customer has one or more Task/Event/Activity record and cannot be deleted.";
		public const string ServiceCaseCannotBeDeleted = "This service call has one or more Task/Event/Activity record and cannot be deleted.";
		public const string TaskCannotBeDeleted = "Task cannot be deleted.";
		public const string EventCannotBeDeleted = "Event cannot be deleted.";
		public const string MassMailSend = "There are {0} Email messages will be generated. Please confirm to proceed.";
		public const string MassMailReSend = "Try to resend selected message?";
		public const string MassMailReSendAll = "There are {0} Email messages failed to send. Please confirm to resend.";
		public const string RecipientsNotFound = "At least one recipient must be specified for the Email activity.";
		public const string BadFilterDataField = "Incorrect Data Field value.";
		public const string BadFilterCondition = "Incorrect Condition value";
		public const string TemplateDublicateError = "Template with this name already exists.";
		public const string AssignNotSetup = "Default Lead Assignment Map is not entered in CR setup.";
		public const string AssignNotSetupBAccount = "Default Customer Assignment Map is not entered in CR setup";
		public const string AssignNotSetupOpportunity = "Default Opportunity Assignment Map is not entered in CR setup";
		public const string AssignNotSetupCase = "Default Case Assignment Map is not entered in CR setup";
		public const string ValidationFailed = "Validation is failed for one or more fields.";
		public const string ReferenceCycleDetected = "Cyclic reference detected.";
		public const string NumberingIDIsNull = "Numbering ID is not configured in the CR setup.";
		public const string AttributeIsReferenced = "One or more attributes is referenced and cannot be deleted.";
		public const string IncorrectInputData = "Incorrect Input Data.";
		public const string SelectBAccountCustomerForCreatedCase = "This Case Class requires the Business Account to be specified.";
		public const string EntityParameterIsNotValid = "Entitiy parameter is not valid";
		public const string InheritanceType = "Inheritance type.";
		public const string TypeMustBeInheritedFrom = "Type '{0}' must be inherited from '{1}' type.";
		public const string TypeDoesnotImplementDefaultConstructor = "Type '{0}' doesn't implement default constructor.";
		public const string TypeDoesnotImplementConstructorWithParameters = "Type '{0}' doesn't implement constructor with parameters: ";
		public const string IncorrectParametersForCreatingGenericType = "Incorrect parameters for creating generic type.";
		public const string OpportunityCannotBeDeletedUseStatus = "This Opportunity has one or more Task/Event/Activity record and cannot be deleted. Cancel the Opportunity instead.";
		public const string CaseCannotBeFound = "Case cannot be Found";
		public const string ContractCannotBeFound = "Contract cannot be found";
		public const string LaborNotConfigured = "Labor Item cannot be found";
		public const string DueDateLessThanStartDate = "Due Date cannot be less then Start Date";
        public const string EndDateLessThanStartDate = "End Date cannot be earlier then Start Date";
		public const string ReminderDateRequired = "'Reminder at' is required";
		public const string CloseCaseWithHoldActivities = "Case has On-Hold billable activities and cannot be closed.";
		public const string OpportunityIsNotCreated = "Opportunity is not created";
		public const string FillAccountAttributes = "Please, Fill the Account Attributes";
		public const string FillOpportunityAttributes = "Please, Fill the Opportunity Attributes";
		public const string ContractActivationDateInFuture = "Contract activation date is in future. This contract can only be used starting from {0}";
		public const string CaseAlreadyReleased = "Given Case is already released.";
		public const string AttributeNotValid = "One or more Attributes are not valid.";
		public const string LeadNoMail = "This Contact is requested no email activities. Continue create email?";
		public const string RecordIsReferenced = "This record is referenced and cannot be deleted.";
		public const string ErrorHeader = "Error";
		public const string NoCorrectContacts = "There are no correct contacts for this operation.";
        public const string ErrorFormat = "Specified format is not supported for notifications of this type.";
		public const string DeleteClassNotification = "Unable to delete notification entered for class";
		public const string SiteNotDefined = "The warehouse isn't specified for some stock items in the product list.";
		public const string DefaultLocationCanNotBeNotActive = "Default location can not be inactive.";
		public const string DefaultContactCanNotBeNotActive = "Default contact can not be inactive.";
		public const string EitherAccountOrContactMustBeSpecified = "Either Business Account or Contact must be specified.";
		public const string OnlyBillByActivity = "Case is configured to bill by Activity. You can release only activities.";
		public const string OnlyBillByCase = "Activity assigned to Case. Case is configured to bill by Case. You can release only case";
		public const string CaseCannotDependUponItself = "Case cannot depend upon itself.";
		public const string AssignmentMapIdEmpty = "Assignment Map ID is not specified.";
		public const string AssignmentError = "Unable to find route for assignment process.";
		public const string CantGenerateInvoice = "Failed to Generated ARInvoice during the instant billing of a case.";
		public const string UserInAnotherContact = "User associated with another Contact '{0}'.";
		public const string ContactWithoutEmail = "Contact '{0}' do not have an email address.";
		public const string ContactWithUser = "Contact {0} already associated with another user.";
		public const string DeleteLocalRoles = "All Local Roles of User '{0}' will be deleted.";
		public const string EmptyEmail = "Recipient '{0}': Email is empty.";
		public const string InvalidRecipients = "{0} of {1} recipients are invalid in notification'{2}', module {3}.";
		public const string CantDeleteEmployeeContact = "Can not delete Contact of the Employee.";
        public const string OneOrMoreActivitiesAreNotApproved = "One or more activities that require Project Manager's approval is not approved. Case can be released only when all activities are approved.";
		public const string NonContactLoginType = "Incorrect User Type '{0}', linked entity must be 'Contact'";
		public const string OpportunityWithIdAlreadyExist = "Opportunity with this ID already exist in the system.";
		public const string InactiveActivityType = "Activity Type '{0}' is not active.";
		public const string InternalActivityType = "Activity Type '{0}' is internal.";
		public const string ExternalActivityType = "Activity Type '{0}' is external. Only portal should create activities of this type.";
		public const string InactiveContactClass = "Contact Class '{0}' is not active.";
		public const string ActivityTypeUsage = "This Activity Type can't be deleted because it's used.";
		public const string ActivityTypeUsageChanged = "This Activity Type can't be changed because it's used.";
		public const string UnableToFindGraph = "Unable to find primary graph for record.";
		public const string ContactNotFound = "Contact not found.";
		public const string DuplicateViewNotFound = "Unable to find view to process duplicate record.";
		public const string NoDuplicatesWereFound = "No Duplicates were found.";
		public const string ContactClassChangeWarning = "Please confirm if you want to update current Contact settings with the Contact Class defaults. Original settings will be preserved otherwise.";
		public const string PurgeLimitCannotBeNegative = "Purge month limit cannot be negative";
		public const string PurgeLimitCannotBeLessThanInCRSetup = "Purge month limit cannot be less than in CR Setup";
		public const string AttachToAccountNotFound = "Please select business account to make attachment.";
		public const string LeadHavePossibleDuplicates = "This lead probably has duplicates";
		public const string NoPossibleDuplicates = "No possible duplicates found";

		public const string ContactHavePossibleDuplicates = "This contact probably has duplicates";
		public const string BAccountHavePossibleDuplicates = "This baccount probably has duplicates";
		public const string MergeNonProspect = "Only prospect business accounts possible to merge.";
		public const string CannotChangeBAccount = "You can not change Business Account. Contact '{0}' have cases or opportunities.";
		public const string BAccountAlreadyExists = "Business Account '{0}' already exists.";
		public const string OpportunityAlreadyExists = "Opportunity '{0}' already exists.";
		public const string DuplicatesNotSelected = "Please select duplicates for merge operation.";
		public const string DefAddressNotExists = "Default Address does not exists for '{0}'";		
		public const string ContactInactive = "Contact '{0}' is inactive or closed.";
        public const string BAccountRequiredToCreateCase = "Business Account is require to create add Case.";

		#endregion

		#region Translatable Strings used in the code
		public const string Warning = AP.Messages.Warning;
		public const string Confirmation = GL.Messages.Confirmation;
		public const string AskConfirmation = GL.Messages.Confirmation;
		public const string ConfirmRemoving = "Are you sure you want to remove {0} members?";
		public const string New = "<NEW>";
		public const string ActivityClassInfo = "Activity";
		public const string DefaultLocationCD = "MAIN";
		public const string DefaultLocationDescription = "Primary Location";
		public const string LeadAddress = "Lead Address";
		public const string CaseClass = "Case Class";
		public const string DisplayType = "Type"; //TODO: need remove
		public const string ContactType = "Contact";
		public const string LeadType = "Lead";
		public const string CampaignID = "Campaign ID";
		public const string ClassSeverity = "Severity Class";
		public const string ContractSLAMapping = "SLA Mapping";
		
		public const string EmailClassInfo = "Email";
		public const string EmailResponseClassInfo = "Email Response";
		public const string HistoryClassInfo = "Changeset";
		public const string EditVendor = "Edit Vendor";
		public const string Company = "Company";
		public const string NoNameRouter = "Router";
		public const string ViewInvoice = CT.Messages.ViewInvoice;
		public const string CreateSalesOrder = "Create Sales Order";
		public const string ViewSalesOrder = "View Sales Order";
		public const string ViewCase = "View Case";
		public const string Employee = "Employee";
		public const string Workgroup = "Workgroup";
		public const string Router = "Router";
        public const string InventoryItem = IN.Messages.InventoryItem;
		public const string OpportunityDocDisc = "Opportunity Document Discount";
		public const string Position = "Position";
		public const string SendArticlesByMail = "Send";
		public const string SendArticlesByMailToolTip = "Sends all articles by e-mail";
		public const string AddArticle = "New";
		public const string AddArticleToolTip = "Initializes a new record";
		public const string ViewArticle = "Details";
		public const string ViewArticleToolTip = "Shows current article";
		public const string NoEmailParameter = "There is no '((email))' parameter in content of the template";
		public const string NoEmailTemplate = "Given template is no published for current and default languages";
		public const string EmptyValueErrorFormat = "'{0}' may not be empty.";
		public const string EmailFromFormat = "Email from {0}";
		public const string AccessDenied = "Access denied";
		public const string NoAccessRightsToScreen = "You do not have enough rights to access the screen {0} ({1})";
		public const string NoAccessRightsTo = "You do not have enough rights to access {0}";
		public const string BAccount = "Customer";
		public const string LocationID = "Location ID";
		public const string LeadTimeDays = "Lead Time (Days)";
		public const string PrimaryAccountID = "Primary Account ID";
		public const string PrimaryContact = "Primary Contact";
		public const string PrimaryContract = "Primary Contract";
		public const string CustomerID = "Customer ID";

	    public const string EmailNotificationTemplate = "Notification Template";
	    public const string EmailActivityTemplate = "Activity";
	    public const string KnowledgeBaseArticle = "KB Article";

		public const string CompleteTaskTooltip = "Complete (Ctrl + K)";
		public const string CompleteTaskAndFollowUp = "Complete & Follow-Up";
		public const string CompleteTaskAndFollowUpTooltip = "Complete & Follow-Up (Ctrl + Shift + K)";

		public const string Shortcuts = "Shortcuts";

		public const string CantDeleteNotHoldActivity = ErrorMessages.CantDeleteRecord + " Activity is not draft.";
		public const string CreateAccount = "Create Account";

		public const string CreateOpportunity = "Create Opportunity";

		public const string InvalidIBqlField = "Type '{0}' must inherit 'PX.Data.IBqlField' interface.";

		public const string IBqlFieldMustBeNested = "Field '{0}' must be nested in 'PX.Data.IBqlTable' type.";
		public const string IBqlTableMustBeInherited = "Type '{0}' must inherit 'PX.Data.IBqlTable' interface.";

		public const string NeedGraphType = "Type '{0}' must inherit 'PX.Data.PXGraph' type";
		public const string NeedBqlCommandType = "Type '{0}' must inherit 'PX.Data.BqlCommand' type";

		public const string GraphTypesAndConditionsLengthException = "The length of 'graphTypes' must be equal to the length of 'conditions'";

		public const string Attention = "Attention";

		public const string Unit = "Unit";

		public const string ThereAreManualSubscribers = "There are manual subscribers";

		public const string EmailAccountDoesnotExist = "Account doesn't exist";

		public const string Subject = "Subject";
		public const string Description = "Description";

		public const string CaseEmailDefaultSubject = "[Case #{0}] {1}";
		public const string CaseEmailSubjectValidator = "[Case #{0}]";

		public const string ActivityStatusListException = "Attribute '{0}' only can be used with '{1}' DAC.";

		public const string DueDate = "Due Date";

		public const string CaseNotFound = "The case cannot be found or you doesn't have enough access rights";
		public const string AccountNotFound = "The account cannot be found or you doesn't have enough access rights";

		public const string LeadIsNotSelected = "The Lead is not selected";
		public const string LeadNotFound = "The Lead cannot be found or you doesn't have enough access rights";
		public const string AccountAssignmentMapIsNotSet = "Customer Assignment Map is not configured in CRSetup";

		public const string EmailNotificationError = "Email send failed. Please, check notification configuration";
		public const string EmailNotificationSetupNotFound = "Email send failed. Notification Settings '{0}' not found.";

		public const string IncorrectMatching = "Incorrect value";

		public const string NeedOrderInsteadOfInvoice = "Invoice cannot be created from the opportunity with stock items. Sales Order document should be created instead.";

		public const string RequiredAttributesAreEmpty = "There are empty required attributes: {0}";
		public const string NothingToMerge = "No records to merge. Please select at least two records to proceed operation.";
		public const string MergeNoConflict = "There are no conflicted values. Do you want to continue and delete {0} records?";

		public const string CRSetupFieldsAreEmpty = "Some fields are empty or have not correct value: {1} in CRSetup";
		public const string DuplicateValidationRulesAreEmpty = "Duplicate Validation rules are not set in CRSetup";

		#endregion

		#region Graph Names
		public const string CRCustomerClassMaint = "Business Account Class Maintenance";
		public const string BusinessAccountMaint = "Business Account Maintenance";
		public const string CRBusinessAccuntsEnq = "Business Account Inquiry";
		public const string CRMergeBAccountsMaint = "Business Account Mass Update";
		public const string LocationMaint = "Business Account Location Maintenance";
		public const string ContactMaint = "Contact Maintenance";
		public const string ContractMaint = "Contract Maintenance";
		public const string SOOrderMaint = "Sales Order Maintenance";
		public const string CRMergeContactsMaint = "Contact Merge Process";
		public const string CRLeadClassMaint = "Lead Class Maintenance";
		public const string CRLeadMaint = "Lead Maintenance";
		public const string CRAssignmentProcess = "Lead Assignment Process";
		public const string CRLeadConvertMaint = "Lead Conversion Process";
		public const string CRLeadMergeMaint = "Lead Merge Process";
		public const string CRImportLeadsMaint = "Lead Import Process";
		public const string CRLeadsEnq = "Lead Inquiry";
		public const string CRContactsEnq = "Contact Inquiry";
		public const string CRCampaignMaint = "Campaign Maintenance";
		public const string CRAddCampaignMembersMaint = "Campaign Add Members Process";
		public const string CRAssignmentMaint = "Assignment Map Maintenance";
		public const string CRCaseClassMaint = "Case Class Maintenance";
		public const string CRCaseMaint = "Case Maintenance";
		public const string CRCaseAssignmentProcess = "Case Assignment Process";
		public const string CRCasesEnq = "Case Inquiry";
		public const string CRMailListMaint = "Mailing List Maintenance";
		public const string CRMassMailMaint = "Mass Mail Entry";
		public const string CROpportunityClassMaint = "Opportunity Class Maintenance";
		public const string CROpportunityMaint = "Opportunity Maintenance";
		public const string CROpportunitiesEnq = "Opportunity Inquiry";
		public const string CRCampaignMembersEnq = "Campaign Member Inquiry";
		public const string CRActivityReleaseProcess = "Activity Release Process";
		public const string CRCaseReleaseProcess = "Case Release Process";
		public const string CRAttributeMaint = "Attribute Maintenance";
		public const string CREventMaint = "Event Maintenance";
		public const string CRCaseEventMaint = "Case Event Maintenance";
		public const string CRTaskMaint = "Task Maintenance";
		public const string CRCaseTaskMaint = "Case Task Maintenance";
		public const string CRActivityMaint = "Activity Maintenance";
		public const string CREmailActivityMaint = "Email Activity Maintenance";
		public const string CRReportEmailMaint = "Report Email Maintenance";
		public const string CRCaseActivityMaint = "Case Activity Maintenance";
		public const string CREmailCaseActivityMaint = "Email Case Activity Maintenance";
		public const string CRTaskActivityMaint = "Task Activity Maintenance";
		public const string CRSetup = "CR Setup";
        public const string CRSetupNotEntered = "Required configuration data is not entered in Company Setup.";
		public const string LeadMassProcess = "Lead Mass Process";
		public const string CRCaseBillingProcess = "Case Billing Process";
		public const string CampaignMemberMassProcess = "Campaign Member Mass Process";
		public const string KBArticle = "Knowledge Base Article";
		public const string KBResponseMaint = "Article Response Maintenance";
        public const string BuildingMaint = "Branch Buildings";
		public const string ServiceCallMaint = "Service Call Maintenance";
		public const string MergeMaint = "Merge Maintenance";
		public const string MergePrepareProcess = "Merge Preparing Process";
		public const string MergeProcess = "Merge Process";
		public const string MergeEntry = "Merge";
		#endregion

		#region Cache Names
		public const string BusinessAccount = "Prospect";
		public const string ParentBusinessAccount = "Parent Prospect";
		public const string BAccountCD = "Business Account";
		public const string BAccountName = "Business Account Name";
		public const string BAccountClass = "Business Account Class";
		public const string ParentAccount = "Parent Business Account";
		public const string ParentAccountID = "Parent Business Account";
		public const string ParentAccountName = "Parent Business Account Name";
		public const string Class = "Class ID";
		public const string LeadContact = "Lead/Contact";
		public const string Contact = "Contact";
		public const string Primary = "Primary";
		public const string TasksEvents = "Tasks and Events";
		public const string Activities = "Activities";
		public const string Contracts = "Contracts";
		public const string Orders = "Orders";
		public const string Opportunities = "Opportunities";
		public const string Cases = "Cases";
		public const string Contract = "Contract";
		public const string ContractDescription = "Contract Description";
		public const string Answers = "Answers";
		public const string Articles = "Articles";
		public const string OpportunityProducts = "Opportunity Products";
		public const string OpportunityTax = "Opportunity Tax";
		public const string Address = "Address";
		public const string CampaignMember = "Campaign Members";
		public const string Subscriptions = "Subscriptions";
		public const string MailRecipients = "Mailing List Subscribers";
		public const string FreeItems = "Free Items";
		public const string DiscountDetails = "Discount Details";
		public const string CaseActivities = "Case Activities";
		public const string Subscribtion = "Subscription";
		public const string Unsubscribtion = "Unsubscription";
		public const string NewLead = "New Lead";
		public const string Match = "Match";
		public const string MatchAll = "Match All";
		public const string MatchingRecords = "Matching Records";
		public const string Responses = "Responses";
		public const string Relations = "Relations";
		public const string MailList = "Marketing List";
		public const string PreviewSettings = "Preview Settings";
		public const string LocationExtAddress = "Location with Address";
		public const string ContactExtAddress = "Contact with Address";
		public const string ContactNotification = "Contact Notification";
		public const string Location = "Location";
		public const string ServiceCall = "Service Call";
		public const string ServiceCallItem = "Item";
		public const string ServiceCallLabor = "Labor";
		public const string ServiceCallMaterial = "Material";
		public const string SelectionCriteria = "Selection Criteria";
		public const string SelectionPreview = "Selection Preview";
		public const string Customer = "Business Account";
		public const string CustomerName = "Customer Name";
		public const string DiscountDetail = "Discount Detail";
		public const string BillingAddress = "Billing Address";
		public const string DestinationAddress = "Destination Address";
		public const string OpportunityClass = "Opportunity Class";
		public const string CustomerClass = "Customer Class";
		public const string LeadClass = "Contact Class";
		public const string MassMail = "Mass Emails";
		public const string CaseForNotification = "Case for notification";
		public const string CRActivity = "Activity";
		public const string Criterion = "Criterion";
		public const string MergeMethod = "Method";
		public const string RecordForMerge = "Item For Merge";
		public const string RecordMerged = "The record was merged successfully.";
		public const string MergeDocument = "Merge Document";
		public const string CommunacationClass = "Communacation Summary";
		public const string GetMore = "more..";
		public const string ReceiveAll = "Receive All";
		public const string Probability = "Opportunity Probability";
        public const string LocationARAccountSub = "Location GL Accounts";
		public const string CampaignStatus = "Campaign Status";
		public const string ContactBAccountID = "Customer ID";
		public const string ContactStatus = "Lead Status";
		public const string EMailAccountDescription = "To";
		public const string ContactFullName = "Company Name";
		public const string BAccountAcctCD = "Parent Account ID";
		public const string BAccountType = "Business Account Type";
		#endregion

		#region View Names

		public const string Selection = "Selection";
		public const string LeadsAndContacts = "Leads and Contacts";
		public const string Leads = "Leads";
		public const string Contacts = "Contacts";
		public const string MainContact = "Main Contact";
		public const string Locations = "Locations";
		public const string DeliverySettings = "Delivery Settings";
		public const string DeliveryContact = "Shipping Contact";
		public const string DeliveryAddress = "Shipping Address";
		public const string Campaign = "Campaign";
		public const string CampaignMembers = "Campaign Members";
		public const string BusinessAccounts = "Business Accounts";
		public const string Filter = "Filter";
		public const string MergeProperties = "Merge Properties";
		public const string UpdateProperties = "Update Properties";
		public const string MailLists = "Mail Lists";
		public const string MassMailSummary = "Summary";
		public const string Campaigns = "Campaigns";
		public const string Preview = "Preview";
		public const string EntityFields = "Entity Fields";
		public const string FailedMessages = "Failed Messages";
		public const string SendedMessages = "Sended Messages";
		public const string History = "History";
		public const string MarketingList = "Marketing List Info";
		public const string EquipmentSummary = "Equipment Summary";
		public const string CallDetails = "Call Details";
		public const string CallDetailsSplit = "Call Details Split";
		public const string Scheduling = "Scheduling";
		public const string Labor = "Labor";
		public const string Materials = "Materials";
		public const string Taxes = "Taxes";
		public const string ConversionSummary = "Conversion Summary";
		public const string TaskSettings = "Task Information";
		public const string EventSettings = "Event Information";
		public const string AccountAnswers = "Account Attributes";
		public const string OpportunityAnswers = "Opportunity Attributes";
		public const string Document = "Document";
		public const string Criteria = "Criteria";
		public const string Methods = "Methods";
		public const string MergeItems = "Items";
		public const string Groups = "Groups";
		public const string Items = "Items";
		public const string Notifications = "Notifications";
		public const string CaseReferences = "Related Cases";
		public const string Attributes = "Attributes";
		public const string CaseClassReaction = "Reaction";
		public const string ImportSettings = "Settings";

		#endregion

		#region Combo Values
		// PhoneType
		public const string Business1 = "Business 1";
		public const string Business2 = "Business 2";
		public const string Business3 = "Business 3";
		public const string BusinessAssistant1 = "Business Assistant 1";
		public const string BusinessFax = "Business Fax";
		public const string Home = "Home";
		public const string HomeFax = "Home Fax";
		public const string Cell = "Cell";

		//Title
		public const string Doctor = "Dr.";
		public const string Miss = "Miss";
		public const string Mr = "Mr.";
		public const string Mrs = "Mrs.";
		public const string Ms = "Ms.";
		public const string Prof = "Prof.";

		//LocTypeList
		public const string CompanyLoc = "Company";
		public const string VendorLoc = "Vendor";
		public const string CustomerLoc = "Customer";
		public const string CombinedLoc = "Customer & Vendor";
		public const string EmployeeLoc = "Employee";

		//BAccountType
		public const string VendorType = "Vendor";
		public const string CustomerType = "Customer";
		public const string CombinedType = "Customer & Vendor";
		public const string EmployeeType = "Employee";
		public const string EmpCombinedType = "Customer & Employee";
		public const string ProspectType = "Prospect";
		public const string CompanyType = "Company";

		//BAccount.status
		public const string Active = "Active";
		public const string Hold = "On Hold";
		public const string HoldPayments = "Hold Payments";
		public const string Inactive = "Inactive";
		public const string OneTime = "One-Time";
		public const string CreditHold = "Credit Hold";

		//AddressTypes
		public const string BusinessAddress = "Business";
		public const string HomeAddress = "Home";
		public const string OtherAddress = "Other";

		//ContactTypes
		public const string Person = "Contact";
		public const string SalesPerson = "Sales Person";
		public const string BAccountProperty = "Business Account";

		//LeadRating
		public const string Hot = "Hot";
		public const string Warm = "Warm";
		public const string Cold = "Cold";

		//CSAnswerType
		public const string Lead = "Lead";
		public const string Account = "Account";
		public const string Case = "Case";		
		public const string Opportunity = "Opportunity";

		// CRMassMailStatus
		public const string Hold_MassMailStatus = "On Hold";
		public const string Prepared_MassMailStatus = "Prepared";
		public const string Approved_MassMailStatus = "Approved";
		public const string Sent_MassMailStatus = "Sent";

		// CRMassMailSource
		public const string MailList_MassMailSource = "Marketing Lists";
		public const string Campaign_MassMailSource = "Campaigns";
		public const string LeadContacts_MassMailSource = "Leads/Contacts/Employees";

		//AssignmentMapType
		public const string Lead_AssignmentMapType = Lead;
		public const string BAccount_AssignmentMapType = BAccount;
		public const string Opportunity_AssignmentMapType = Opportunity;
		public const string Case_AssignmentMapType = Case;
		public const string ExpenseClaim_AssignmentMapType = "Expense Claim";
				
		//RuleType
		public const string AllTrue = "All conditions are true.";
		public const string AtleastOneIsTrue = "At least one condition is true.";
		public const string AtleastOneIsFalse = "At least one condition is false.";

		//LeadSource
		public const string Web = "Web";
		public const string PhoneInq = "Phone Inquiry";
		public const string Referral = "Referral";
		public const string PurchasedList = "Purchased List";
		public const string Other = "Other";

		//Campaign Status
		public const string Prepared = "Prepared";
		public const string Started = "Started";
		public const string Finished = "Finished";
		public const string Canceled = "Canceled";

		//LeadMajorStatus
		public const string RecordFlag = "Just a Record";
		public const string JustCreatedFlag = "Just Created";
		public const string HoldFlag = "On Hold";
		public const string OpenFlag = "Open";
		public const string ClosedFlag = "Closed";
		public const string SuspendedFlag = "Suspended";
		public const string ConvertedFlag = "Converted";

		//LeadStatus
		public const string LeadNew = "New";
		public const string LeadOpen = "Open";
		public const string LeadSuspend = "Suspended";
		public const string Closed = "Closed";
		public const string LeadConverted = "Converted";

		//LeadResolution
		public const string LeadResolutionAssigned = "Assigned";
		public const string LeadResolutionContacted = "Contacted";
		public const string LeadResolutionConverted = "Converted";
		public const string LeadResolutionNotConverted = "Not Converted";
		public const string LeadResolutionNotContacted = "Not Contacted";
		public const string LeadResolutionDuplicate = "Duplicate";

		//ActivityMajorStatus
		public const string PreprocessFlag = "Preprocess";
		public const string ProcessingFlag = "Processing";
		public const string ProcessedFlag = "Processed";
		public const string FailedFlag = "Failed";
		public const string CanceledFlag = "Canceled";
		public const string CompletedFlag = "Completed";
		public const string DeletedFlag = "Deleted";
		public const string ReleasedFlag = "Released";

		//CRActivityTypes
		public const string Appointment = "Appointment";
		public const string Email = "Email";
		public const string EmailResponse = "Email Response";
		public const string PhoneCall = "Phone Call";
		public const string Note = "Note";
		public const string Chat = "Chat";
		public const string Message = "Message";
		public const string WorkItem = "Work Item";	

		//CRContactMethods
		public const string MethodAny = "Any";
		public const string MethodEmail = "Email";
		public const string MethodMail = "Mail";
		public const string MethodFax = "Fax";
		public const string MethodPhone = "Phone";

		//CRMaritalStatuses
		public const string Single = "Single";
		public const string Married = "Married";
		public const string Divorced = "Divorced";
		public const string Widowed = "Widowed";

		//CRGenders
		public const string Male = "Male";
		public const string Female = "Female";

		//Case Sources
		public const string CaseSourceEmail = "Email";
		public const string CaseSourcePhone = "Phone";
		public const string CaseSourceWeb = "Web";
		public const string CaseSourceChat = "Chat";

		//CRValidationRules
		public const string LeadToContactValidation = "Lead to Contact";
		public const string LeadToAccountValidation = "Lead to Account";
		public const string AccountValidation = "Account";

		//Transformation Rules
		public const string DomainName = "Domain Name";
		public const string None = "None";
		public const string SplitWords = "Split Words";

		//Duplicate Statuses
		public const string Validated = "Validated";
		public const string PossibleDuplicated= "Possible Duplicate";
		public const string NotValidated = "Not Validated";
		public const string Duplicated = "Duplicated";

		//BillingTypeList
		public const string PerCase = "Per Case";
		public const string PerActivity = "Per Activity";
		//Application
		public const string Portal = "Portal";
		public const string Backend = "Back-end";
		#endregion

		#region Custom Actions
		public const string ViewContact = "Contact Details";
		public const string ViewLocation = "Location Details";
		public const string ViewContract = "Contract Details";
		public const string ViewOrder = "Order Details";

		public const string AddNewLocation = "Add Location";
		public const string AddNewFakeLocation = " ";
		public const string SetDefault = "Set as Default";
		public const string ViewOnMap = "View on Map";
		public const string RunTemplate = "Run Template";
		public const string SaveTemplate = "Save Template";
		public const string SaveAsTemplate = "Save as Template";
		public const string CancelTemplate = "Clear";
		public const string RemoveTemplate = "Remove Template";
		public const string MergeLead = "Merge";
		public const string AssignLeads = "Assign";
		public const string AssignMap = "Assignment Map";
		public const string Search = "Search";
		public const string AddNewSubscribers = "Add New Subscribers";
		public const string AddCampaign = "Add Campaigns";
		public const string AddNewMembers = "Add New Members";
		public const string TrashCurrent = "Move to Trash";		
		public const string TrashCurrentConfirm = "Current record will be moved to trash.";
		public const string DeleteSelected = "Delete Selected";
		public const string MultiDeleteTooltip = "Removes selected records";
		public const string ViewVendor = "View Vendor";
		public const string ViewCustomer = "View Customer";
		public const string ConvertToCustomer = "Convert To Customer";
		public const string ConvertToVendor = "Convert To Vendor";
		public const string Merge = "Merge";
		public const string MergeAll = "Merge All";
		public const string Update = "Update";
		public const string Add = "Add Members";
		public const string Remove = "Remove";
		public const string PropertyDisplayName = "Name";
		public const string PropertyValue = "Value";
		public const string Import = "Import";
		public const string ReSend = "Resend Message";
		public const string ReSendAllFailed = "Resend All Failed";

		public const string OpportunityDetails = "Opportunity Details";
		public const string BusinessAccountDetails = "Business Account Details";
		public const string Details = "Details";
		public const string ViewAllActivities = "View Activities";
		public const string ViewDetails = "View Details";
		public const string ViewDetailsTooltip = "Shows details of current record";
		public const string AddNew = "Add New";
		public const string AddFakeNew = " ";
		public const string AddNew2 = "New";
		public const string AddContact = "Add Contact";
		public const string AddNewContact = "New Contact";
		public const string AttachToAccount = "Attach to Account";
		public const string AddNewFakeContact = " ";
		public const string AddNewLead = "New Lead";
		public const string AddNewFakeLead = " ";
		public const string AddNewOpportunity = "Add Opportunity";
		public const string AddNewFakeOpportunity = " ";
		public const string MergeContact = "Merge Contacts";
		public const string MergeBAccount = "Merge Business Account";
		public const string AddAll = "Add All";
		public const string Load = "Load";
		public const string Assign = "Apply Assignment Rules";
		public const string ShowCustomer = "Show Customers";
		public const string ShowProspect = "Show Prospects";
		public const string Release = "Release";
		public const string IncludeInactive = "Include Inactive";
		public const string SearchText = "Search";
		public const string ShowLeads = "Show Leads";
		public const string ShowContacts = "Show Contacts";
		public const string Convert = "Convert";
		public const string ConvertToContact = "Convert to Contact";
		public const string ConvertToOpportunity = "Convert to Opportunity";
		public const string ConvertToBAccount = "Convert to Business Account";
		public const string MarkAsValidated = "Mark as Validated";
		public const string CheckForDuplicates = "Check for Duplicates";		
		public const string InsertArticle = "Insert Article";
		public const string InsertArticleToolTip = "Inserts article into the mail body";


		public const string AddNewCase = "Add Case";
		public const string AddNewFakeCase = " ";
		public const string CaseDetails = "Case Details";
		public const string AddTask = "Add Task";
		public const string AddEvent = "Add Event";
		public const string AddActivity = "Add Activity";
		public const string AddEmail = "Add Email";
		public const string RegisterActivity = "Register Activity";
		public const string AddTypedActivityFormat = "Add {0}";
		public const string SendMail = "Send Email";

		public const string CompleteActivity = "Complete Activity";
		public const string CancelActivity = "Cancel Activity";
		public const string ViewActivity = "View Activity";
		public const string ViewCompletedActivity = "View Completed Activity";
		public const string CopyFromCompany = "Copy from Company";
		//public const string AssignmentUp = "Up";
		//public const string AssignmentDown = "Down";
        public const string AssignmentUp = " ";
        public const string AssignmentDown = " ";
        public const string ShowContact = "Show Contact";
		public const string Delete = "Delete";
		public const string Send = "Send";
		public const string SendAll = "Send All";
		public const string Reply = "Reply";
		public const string ReplyAll = "Reply All";
		public const string Forward = "Forward";
		public const string PreviewMessage = "Test Message";
		public const string LoadTemplate = "Load Template";
		public const string CreateInvoice = "Create Invoice";
		public const string SetAsPrimary = "Set as Primary";
		public const string Complete = "Complete";
		public const string Cancel = "Cancel";
		public const string Prev = "Prev";
		public const string Next = "Next";
		public const string EndTimeLTStartTime = "End Time cannot be less than Start Time";

		public const string Actions = "Actions";

		public const string Prepare = "Prepare";
		public const string PrepareAll = "Prepare All";
		public const string Process = "Process";
		public const string ProcessAll = "Process All";

		public const string LastActivity = "Last Activity";

		public const string MassMergeActionName = "Merge";

		public const string OpenActivityOwner = "OpenActivityOwner";
		#endregion

		public static string GetLocal(string msg)
		{
			return PXLocalizer.Localize(msg, typeof(Messages).ToString());
		}

		public static string FormNoAccessRightsMessage(Type graphType)
		{
			PXSiteMapNode sitemap = PXSiteMap.Provider.FindSiteMapNodeUnsecure(graphType);
			if (sitemap != null && sitemap.ScreenID != null)
			{
				return string.Format(GetLocal(Messages.NoAccessRightsToScreen), sitemap.Title, sitemap.ScreenID);
			}
			else
			{
				return string.Format(GetLocal(Messages.NoAccessRightsTo), graphType.ToString());
			}
		}
	}
}
