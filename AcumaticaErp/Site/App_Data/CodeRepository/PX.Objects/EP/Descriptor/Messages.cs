using System;
using PX.Data;
using PX.Common;

namespace PX.Objects.EP
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		// Add your messages here as follows (see line below):
		// public const string YourMessage = "Your message here.";
		#region Validation and Processing Messages
		public const string Prefix = "EP Error";
		public const string RecordExists = "Record already exists.";
		public const string BAccountExists = "This ID is already used for another Vendor or Customer record.";
		public const string VendorClassExists = "This ID is already used for the Vendor Class.";
		public const string EmployeeLoginExists = "This Login ID is assigned to Employee {0}: {1}. It cannot be associated with another Employee.";
		public const string DocumentOutOfBalance = "Document is out of balance.";
		public const string CustomerRequired = "Customer ID must be specified for billable items.";
		public const string CustomerLocationRequired = "Customer Location ID must be specified.";
		public const string Document_Status_Invalid = "Document Status is invalid for processing.";
		public const string EmployeeTermsCannotHaveCashDiscounts = "You cannot use Terms with configured Cash Discount for Employees.";
		public const string EmployeeTermsCannotHaveMultiplyInstallments = "You cannot use Terms with configured Multiple Installments for Employees.";
		public const string BillableTimeDetailsTotalDiffersFromHeaderTotal = "Billable Time has been calculated incorrectly. It will be recalculated.";
		public const string WorkedTimeMustNotExceedDateBreak = "Time interval should be within the single day. Please split the record.";
		public const string BillableTimeCanNotBeGreaterThenWorkedTime = "Billable Time and Billable Overtime should not exceed the Time Worked.";
		public const string BillableOverTimeCanNotBeGreaterThenWorkedOverTime = "Billable Overtime  should not exceed the Over Time.";
        public const string TimePeriodEnteredCanNotBeGreaterThen24Hours = "You cannot specify a time interval that is larger than 24 hours. Please split the record.";
		public const string TaskProcessUsage = "This task is ivolved into one or more processes and cannot be deleted. Originating document must be updated instead.";
	    public const string CompleteAndCancelNotAvailableForTask = "Task in draft status could not be canceled or completed";
		public const string WorkOverTimeCannotBeGreaterThenWorkedTime = "Overtime Worked cannot be greater then Time Worked";
		public const string ClaimAssignmentRulesNotSetup = "Claim Approval Map is not specified on the Time & Expenses Preferences. Please complete Time & Expenses Preferences and repeat the operation.";
		public const string AssignmentRulesNoResult = "Assignment Rules failed to assign Approver for the document. Please contact your system administrator";
		public const string MessageWithActivity = "This message has activity and cannot be deleted.";
		public const string ConfirmDeleteAttendeeHeader = "Delete Attendee";
		public const string ConfirmDeleteAttendeeText = "Are you sure you want to delete";
		public const string ConfirmRescheduleNotificationHeader = "Event date was changed";
		public const string ConfirmRescheduleNotificationText = "Would you like to notify invited attendees about new event date?";
		public const string EMailWasChanged = "Email was changed";
		public const string SendInvitationToNewEMail = "Send the invitation to new e-mail";
		public const string InvalidEmail = "Incorrect e-mail";
		public const string MailToUndefined = "Create message failed. Email recipient should be defined.";
		public const string EventIsNotSaved = "Event must be saved";
		public const string EventIsThePast = "Incorrect Event Date";
		public const string EventIsNotEditable = "Incorrect Event Status";
		public const string OpenActivity = "Open Selected Activity";
		public const string PreviouslyEnterCustomerOrContract = "Please enter customer or contract.";
		public const string DontHaveAppoveRights = "You don't have access rights to approve document.";
		public const string DontHaveRejectRights = "You don't have access rights to reject document.";
		public const string ReleaseClaimWithoutFinPeriod = "Fin Period should be specified to release claim.";
		public const string RatePeriodOverlapped = "Rate period is overlaps with the other.";
		public const string RateCustomerExist = "Default rate for customer already exist, please define period of rate.";
		public const string RateLaborEmpty = "Labor Item or Override Item should be definded.";
		public const string EmptyEmailAccountAddress = "Email account address is empty";
		public const string MailAccountNotSpecified = "Email account is not specified";
		public const string InvalidEmailForOperation = "Invalid email message for this operation";
	    public const string ValueMustBeGreaterThanZero = "The value must be greater than zero";
		public const string ActivityTimeUnitIsEmpty = "Activity Time Unit is not entered in Time & Expenses Preferences";
		public const string EmployeeRateUnitIsEmpty = "Employee Hour Rate Unit is not entered in Time & Expenses Preferences";
		public const string InventoryItemIsEmpty = "Inventory Item is not entered";
		public const string InventoryAccountIsEmpty = "Account Group of Inventory Item is not entered";
		public const string WorkGreaterThanInPayRole = "Must be less or equal to {0}";
		public const string DateNotInWeek = "Doesn't correspond to week number";
		public const string AccountGroupIsNotAssignedForItem = "Expense Account of '{0}' is not included in any Account Group. Please assign an Account Group to the following Account '{1}' and try again.";
		public const string AccountGroupIsNotAssignedForAccount = "Expense Account '{0}' is not included in any Account Group. Please assign an Account Group given Account and try again.";
		public const string InvalidMailProviderType = "Type '{0}' must contain constructor with following parameters: {1}, {2}, {3}, {4}";
		public const string ReportCannotBeFound = "Report '{0}' cannot be found";
		public const string CustomerDoesNotMatchProject = "Customer specified doesnot match the Customer on the Project/Contract.";
		public const string AssigmentMapEntityType = "Only types can be used as entity type for assignment map.";
		public const string ProjectIsNotAvailableForEmployee = "Current Employee can not post transactions to the given Project.";
		public const string ProcessRouteSequence = "Sequence {0} process successfully.";
		public const string CannotDeleteCorrectionActivity = "In the correction Time Card if you want to delete/eliminate previosly released Activity just set the Time to zero.";
        public const string CannotDeleteCorrectionRecord = "In the correction Time Card if you want to delete/eliminate previosly released time record just set the Time to zero.";
		public const string LaborClassNotSpecified = "Labor Item is not specified for the Employee.";
		public const string OvertimeLaborClassNotSpecified = "Overtime Labor Item is not specified for the Employee.";
		public const string FailedToConvertUnits = "Failed to convert from '{0}' to '{1}'. Please configure unit conversions for '{2}'.";
        public const string DepartmentInUse = "This Department is assigned to the Employee and cannot be changed.";
        public const string PositionInUse = "This Position is assigned to the Employee and cannot be changed.";
        public const string MailProcessingIsTurnedOff = "Mail processing is turned off.";

		public const string NoDefualtAccountOnProject = "PMSetup is configured to get its Expense Account from the Project but Default Account is not configured for Project '{0}'.";
        public const string NoDefualtAccrualAccountOnProject = "PMSetup is configured to get its Expense Accrual Account from the Project but Default Accrual Account is not configured for Project '{0}'.";
		public const string NoAccountGroupOnProject = "PMSetup is configured to get its Expense Account from the Project but Default Account '{0}' for Project '{1}' is not mapped to any AccountGroup.";
        public const string NoDefualtAccountOnTask = "PMSetup is configured to get its Expense Account from the Task but Default Account is not configured for Project '{0}' Task '{1}'.";
        public const string NoDefualtAccrualAccountOnTask = "PMSetup is configured to get its Expense Accrual Account from the Task but Default Accrual Account is not configured for Project '{0}' Task '{1}'.";
		public const string NoAccountGroupOnTask = "PMSetup is configured to get its Expense Account from the Task but Default Account '{0}' for Project '{1}' Task '{2}' is not mapped to any AccountGroup.";
		public const string NoExpenseAccountOnEmployee = "PMSetup is configured to get its Expense Account from the Employee but Expense Account is not configured for Employee '{0}'";
		public const string NoDefaultAccountOnEquipment = "PMSetup is configured to get its Expense Account from the Equipment but Default Account is not configured for Equipment '{0}'";
		public const string NoAccountGroupOnEmployee = "PMSetup is configured to get its Expense Account from the Resource but Expense Account '{0}' for Employee '{1}' is not mapped to any AccountGroup.";
		public const string NoAccountGroupOnEquipment = "PMSetup is configured to get its Expense Account from the Resource but Expense Account '{0}' for Equipment '{1}' is not mapped to any AccountGroup.";
		public const string NoExpenseAccountOnInventory = "PMSetup is configured to get its Expense Account from the Inventory Item but Expense Account is not configured for Inventory Item '{0}'";
        public const string NoAccrualExpenseAccountOnInventory = "PMSetup is configured to get its Expense Accrual Account from the Inventory Item but Expense Accrual Account is not configured for Inventory Item '{0}'";
		public const string NoAccountGroupOnInventory = "PMSetup is configured to get its Expense Account from the Inventory Item but Expense Account '{0}' for Inventory Item '{1}' is not mapped to any AccountGroup.";
		public const string NoExpenseSubOnInventory = "PMSetup is configured to combine Expense Subaccount from Inventory Item but Expense subaccount for '{0}' is empty.";
        public const string NoExpenseAccrualSubOnInventory = "PMSetup is configured to combine Expense Accrual Subaccount from Inventory Item but Expense Accrual subaccount for '{0}' is empty.";
		public const string NoExpenseSubOnProject = "PMSetup is configured to combine Expense Subaccount from Project but Expense subaccount for '{0}' is empty.";
        public const string NoExpenseAccrualSubOnProject = "PMSetup is configured to combine Expense Accrual Subaccount from Project but Expense Accrual subaccount for '{0}' is empty.";
		public const string NoExpenseSubOnTask = "PMSetup is configured to combine Expense Subaccount from Task but Expense subaccount for '{0}'.'{1}' is empty.";
        public const string NoExpenseAccrualSubOnTask = "PMSetup is configured to combine Expense Accrual Subaccount from Task but Expense Accrual subaccount for '{0}'.'{1}' is empty.";
		public const string NoExpenseSubOnEmployee = "PMSetup is configured to combine Expense Subaccount from Resource but Expense subaccount for Employee '{0}' is empty.";
		public const string NoDefaultSubOnEquipment = "PMSetup is configured to combine Expense Subaccount from Resource but Default subaccount for Equipment '{0}' is empty.";
		public const string ExpenseAccrualIsRequired = "Expense Accrual Account is Required but is not configured for Non-Stock Item '{0}'. Please setup the account and try again.";
		public const string ExpenseAccrualSubIsRequired = "Expense Accrual Subaccount is Required but is not configured for Non-Stock Item '{0}'. Please setup the subaccount and try again.";
		public const string TimeCardInFutureExists = "Since there exists a Time Card for the future week you cannot change the Employee in the given week.";
        public const string EquipmentTimeCardInFutureExists = "Since there exists a Time Card for the future week you cannot change the Equipment in the given week.";
		public const string AlreadyReleased = "This document is already released.";
		public const string DateOutOfRange = "Date is out of range. It can only be within the given week of the Time Card.";
		public const string ApprovalRefNoteIDNull = "Record for approving not found, RefNoteId is undefined.";
		public const string ApprovalRecordNotFound = "Record for approving not found.";
        public const string TimeCardNoDelete = "Since there exists a timecard for the future week you cannot delete this Time Card.";
	    public const string ProjectIsNotAvailableForEquipment = "This project is not available for current equipment";
		public const string NotificationTemplateNotFound = "Template cannot be not found";
		public const string NotificationTemplateCDNotFound = "Notification Template '{0}' cannot be not found";
		public const string EffectiveDateIsLower = "New record cannot have date lower than last activity date end. Set date bigger than {0}";
	    public const string InvalidProjectTaskInActivity = "Activity is referencing Project Task that was not found in the system. Please correct the activity.";
	    public const string OneOrMoreActivitiesAreNotApproved = "One or more activities that require Project Manager's approval is not approved. Time Card can be submited for approval only when all activities are approveed.";
        public const string ActivityIsNotApproved = "Activity is not approved.";
		public const string CantLoginTypeEntityChange = "Cannot change entity to Contact. Delete all non-guest Allowed Roles.";
		public const string ActivityAssignedToTimeCard = "This Activity assigned to the Time Card. You may do changes only in a Time Card screen.";
		public const string UserWithoutEmployee = "The user '{0}' is not associated with an employee.";
        public const string EquipmentSetupRateIsNotDefined = "The Setup Rate Class is not defined for the given Equipment. Please set the Setup Rate Class on the Equipment screen and try agaain.";
        public const string EquipmentRunRateIsNotDefined = "The Run Rate Item is not defined for the given Equipment. Please set the Run Rate Class on the Equipment screen and try agaain.";
        public const string EquipmentSuspendRateIsNotDefined = "The Suspend Rate Item is not defined for the given Equipment. Please set the Suspend Rate Class on the Equipment screen and try agaain.";
		public const string defaultActivityTypeNoTracTime = "Default Activity Type must have track time is enabled.";
		public const string EmailIsEmpty = "Please note that an email address will be required when assigning a user account to an employee.";
        public const string OvertimeNotAllowed = "Overtime cannot be specified untill all the available regular time is utilised. Regular time for week = {0} hrs";
        public const string TimecradIsNotNormalized = "Time Card must be filled out completly. Regular time for week = {0} hrs. You can use 'Normalize Timecard' to automatically fill up the remaining hours.";
		public const string DisableEmployeeBeforeDeleting = "Make this employee inactive before deleting";
		public const string UserParticipateInAssignmentMap = "User '{0}' participate in the Assignment and Approval Map '{1}'";
		public const string UserCannotBeFound = "User cannot be found";
		public const string CustomWeekNotFound = "Custom Week cannot be found";
		public const string CustomWeekNotFoundByDate = "Custom Week cannot be found. Custom Week's must be generated with date greater than {0:d}";
        public const string NoSalesAccountOnInventory = "Sales Account is not setup for the Inventory Item but is required. Inventory Item '{0}'.";
	    public const string RGIsNotDefinedForEmployee = "Regular Hours per week is not defined for the given employee.";
	    public const string TotalTimeForWeekCannotExceedHours = "Regular Time for the week cannot exceed {0} hrs.";
	    public const string TimecardIsNotValid = "Time Card is not valid. Please correct and try again.";
		public const string InactiveEpmloyee = "The status of employee '{0}' is '{1}'.";
		public const string InactiveUser = "The user '{0}' was not activated.";
		public const string ExistsActivitiesLessThanDate = "The {0} cannot be earlier than '{1:d}' because some activities or time cards were created after the specified period.";
		public const string ExistsActivitiesGreateThanDate = "The {0} cannot be later than '{1:d}' because some activities or time cards were created before the specified period.";
		public const string ActivityIsNotCompleted = "Activity is not Completed.";
		public const string CustomWeekNotCreated = "You must configure the weeks in the Custom Week Settings tab for the period spanning between {0:d} and {1:d} because there are existing Activities in this period";
		public const string TimecardMustBeSaved = "Time Card must be saved before you can view the Activities.";
		public const string UnableConvertToTime = "Cannot convert \"{0}\" to Time";
		public const string CostInUse = "You cannot delete this record of \"Employee Cost\" because it is already in use.";
		public const string CannotDeleteInUse = "Cannot delete because it is already in use.";
		public const string TransactionNotExists = "Transaction does not exist";
		public const string MustBeEmployee = "User must be an Employee to use current screen.";
		public const string TimeInTimezoneOfEmployee = "Time is displayed in the timezone of Employee. TimeZone=";
		
		#endregion //Validation and Processing Messages

		#region Translatable Strings used in the code
		public const string ARInvoiceDesc = "Reimbursable Personal Expenses";
		public const string Release = "Release";
		public const string ReleaseAll = "Release All";
		public const string Approve = "Approve";
		public const string ApproveAll = "Approve All";
		public const string ClaimDetails = "Claim Details";
		public const string Sent = CR.Messages.Sent_MassMailStatus;
		public const string Send = "Send";
		public const string SendMessage = "Send Message";
		public const string Forward = "Forward";
		public const string ForwardMessage = "Forward Message";
		public const string Reply = "Reply";
		public const string ReplyMessage = "Reply Message";
		public const string View = "View";
		public const string ViewEMail = "View Email";
		public const string MarkAllAsRead = "Mark All as Read";
		public const string MarkAsRead = "Mark as Read";
		public const string ttipMarkAllAsRead = "Mark all messages in folder as read";
		public const string ttipMarkAsRead = "Mark selected messages as read";
		public const string ViewARDocument = "View AR Document";
		public const string AddAttendee = "Add Attandee";
		public const string RemoveAttendee = "Delete Attandee";
		public const string Inbox = "Inbox";		
		public const string Received = "Received";
		public const string Draft = CT.Messages.Draft;
		public const string Open = "Open";
		public const string Deferred = "Deferred";
		public const string Outbox = "Outbox";
		public const string Failed = "Failed";
		public const string Completed = GL.Messages.Completed;
		public const string Canceled = "Canceled";
		public const string Cancel = "Cancel";
		public const string CancelTooltipS = "Marks current record as canceled";
		public const string Receive = "Receive";
		public const string ttipReceive = "Receive all mail for seleced account";
		public const string ProcessAll = IN.Messages.ProcessAll;
		public const string Process = IN.Messages.Process;
		public const string NavigateToBox = "Open email box";
		public const string ttipProcess = "Process selected messages";
		public const string AcceptInvitation = "Accept";
		public const string AcceptInvitationTooltip = "Accept invitation to event";
		public const string RejectInvitation = "Reject";
		public const string RejectInvitationTooltip = "Reject invitation to event";
		public const string SendCard = "Send Card";
		public const string SendCardTooltip = "Send i-Calendar format information about event by e-mail";
		public const string SendInvitations = "Invite All";
		public const string SendInvitationsTooltip = "Send e-mail invitations for all";
		public const string SendPersonalInvitation = "Invite";
		public const string SendPersonalInvitationTooltip = "Send e-mail invitation for current record";
		public const string ResendPersonalInvitation = "Invitation was already sent. Are you sure you want send repeat invitation?";
		public const string SendInvitationSuccessful = "Invitation was successful sent.";
		public const string Invitation = "Invitation";
		public const string CancelInvitation = "Cancel Invitation";
		public const string EmailIncorrect = "Incorrect e-mail address";
		public const string ConfirmIncorrectEmailInvitations = "There are incorrect e-mail addresses. Would you like send invitations anyway?";
		public const string ConfirmOneIncorrectEmailInvitation = "There are incorrect e-mail addresses. Would you like send invitation anyway?";
		public const string NotifyNotInvitedAttendees = "Would you like send the event invitation to only not invited attendees?";
		public const string NotifyAttendees = "Would you like to send the event invitation to the selected attendees?";
		public const string ConfirmCancelAttendeeInvitations = "Would you like send cancel invitations?";
		public const string ViewSource = "View Entity";
		public const string SourceDescription = "Source Description";
		public const string TaskID = "Task ID";
		public const string TaskType = "Type";
		public const string OtherTask = "All";
		public const string Assigned = "Assigned";
		public const string AssignedTo = "Assigned To";
		public const string WorkGroup = "Workgroup";
		public const string Owner = "Owner";
		public const string Escalated = "Escalated";
		public const string FollowUp = "Follow Up";
		public const string DayOfWeek = "Day Of Week";
		public const string MarkAsCompleted = "Mark As Completed";
		public const string MarkAsCompletedTooltip = "Marks current record as completed (Ctrl + K)";
		public const string Dismiss = "Dismiss";
		public const string DismissAll = "Dismiss All";
		public const string Snooze = "Snooze";
		public const string Complete = "Complete";
		public const string CompleteTooltip = "Marks current record as completed (Ctrl + K)";
		public const string CompleteTooltipS = "Marks current record as completed";
		public const string CompleteAndFollowUp = "Complete & Follow-Up";
		public const string CompleteAndFollowUpTooltip = "Marks current record as completed and creates new its copy (Ctrl + Shift + K)";
		public const string CancelSending = "Cancel Sending";
		public const string CancelSendingTooltip = "Cancels sending of current record";
		public const string AssignmentMap = "Assignment Map";
		public const string ViewDetails = "View Details";
		public const string ttipViewDetails = "View Task Details";
		public const string ViewEntity = "View Entity";
		public const string ViewOwner = "View Owner";
		public const string ViewAccount = "View Account";
		public const string ViewParentAccount = "View Parent Account";
		public const string ttipViewEntity = "View Reference Entity";
		public const string ttipViewOwner = "Shows current owner";
        public const string ttipViewAccount = "Shows current customer";
        public const string ttipViewParentAccount = "Shows current parent customer";
		public const string Periods = "Periods";
		public const string CompleteEventTooltip = "Complete Event (Ctrl + K)";
		public const string CompleteAndFollowUpEvent = "Complete & Follow-Up";
		public const string CompleteAndFollowUpEventTooltip = "Complete & Follow-Up (Ctrl + Shift + K)";
		public const string DueDateFormat = "Date: {0}";
		public const string Min5 = "5 minutes";
		public const string Min10 = "10 minutes";
		public const string Min15 = "15 minutes";
		public const string Min20 = "20 minutes";
		public const string Min25 = "25 minutes";
		public const string Min30 = "30 minutes";
		public const string Min35 = "35 minutes";
		public const string Min40 = "40 minutes";
		public const string Min45 = "45 minutes";
		public const string Min50 = "50 minutes";
		public const string Min55 = "55 minutes";
		public const string Min60 = "1 hour";
		public const string Min120 = "2 hours";
		public const string Min240 = "4 hours";
		public const string Min720 = "0.5 days";
		public const string Min1440 = "1 day";
		public const string Date = "Date";
		public const string Delete = "Delete";
		public const string ttipDelete = "Delete selected messages";
		public const string CreatedBy = "Created By";
		public const string Company = "Company";
		public const string ttipViewEventDetails = "View Event Details.";
		public const string AddEvent = "Add Event";
		public const string ExportCalendar = "Export Calendar";
		public const string ExportCalendarTooltip = "Export iCalendar-format information about calendar";
		public const string ImportCalendar = "Import Calendar";
		public const string ImportCalendarTooltip = "Import iCalendar-format information about calendar";
		public const string ExportCard = "Export Card";
		public const string ExportCardTooltip = "Export iCalendar-format information about event";
		public const string ImportCard = "Import Card";
		public const string ImportCardTooltip = "Import iCalendar-format information about event";
		public const string CompleteEvent = "Complete";
		public const string CancelEvent = "Cancel";
		public const string OneOfRecordsIsNull = "One of records is null";
		public const string NullTaskID = "TaskID cannot be null";
		public const string NullStartDate = "Start Date cannot be null";
		public const string NoteIdFieldIsAbsent = "NoteId field is absent in '{0}'";
		public const string EntityIsNotViewed = "Is not viewed";
		public const string EntityIsViewed = "Is viewed";
		public const string NoRightForNotificatinTemplate = "There are no rights for current notificatin template.";
		public const string Number = "Number";
		public const string Name = "Name";
		public const string OldValue = "Old Value";
		public const string NewValue = "New Value";
		public const string CompletedAt = "Completed At";
		public const string EmptyTimecardAssignMap = "Time Card Approval Map is not entered in Time & Expenses Preferences";
		public const string EmptyEquipmentTimecardAssignMap = "Equipment Time Card Approval Map is not entered in Time & Expenses Preferences";
		public const string ParentBAccount = "Parent Account Name";
		public const string CannotApproveRejectedItem = "Cannot approve rejected document.";
		public const string AddTask = "Add Task";
		public const string AddActivity = "Add Activity";
		public const string AddActivityTooltip = "Adds new activity";
		public const string Actions = "Actions";
		public const string Reject = "Reject";
		public const string Assign = "Assign";
		public const string ApprovalDate = "Approval Date";
		public const string EmptyEmployeeID = "Employee must be set";
		public const string EmployeeID = "Employee ID";
		public const string Active = "Active";
        public const string Inactive = "Inactive";
		public const string Copy = "Copy";
		public const string Correct = "Correct";
		public const string EmptyLabourOrOvertimeItem = "Labour or Overtime Labour Class is not configured";
		public const string CreateNew = "New";
		public const string Accounts = "Mail Boxes";
		public const string InboxBox = "Inbox";
		public const string DraftBox = "Draft";
		public const string SentBox = "Sent";
		public const string OutboxBox = "Outbox";
		public const string RemovedBox = "Trash";
		public const string Group = "Group";
		public const string Workgroup = "Workgroup";
		public const string Router = "Router";
		public const string Jump = "Jump";
		public const string DownloadEmlFile = "Download .eml file";
		public const string DownloadEmlFileTooltip = "Export as .eml file";
		public const string Automatically = "Automatically";
		public const string ttipAutomatically = "";
		public const string ConvertToLead = "Convert to Lead";
		public const string ttipConvertToLead = "";
		public const string ConvertToCase = "Convert to Case";
		public const string ttipConvertToCase = "";
		public const string Resend = "Resend";
		public const string ttipsend = "";
		public const string ActivityIsBilled = "Activity cannot be deleted because it has already been billed";
		public const string ActivityIsReleased = "Activity cannot be deleted because it has already been released";
		public const string ActivityIs = "Activity in status \"{0}\" cannot be deleted.";
		public const string EmailTemplateIsNotConfigured = "Email message template is not configured";
		public const string RejectAll = "Reject All";
	    public const string PreloadFromTasks = "Preload From Tasks";
        public const string PreloadFromPreviousTimecard = "Preload From Previous Time Card";
        public const string PreloadHolidays = "Preload Holidays";
        public const string NormalizeTimecard = "Normalize Time Card";
		public const string ConfirmDeleteNotAllowedRoles = "Any not allowed roles for all corresponding users will be deleted.";
		public const string SetupNotEntered = "Required configuration data is not entered.\nAdd or change EP Default Activity Type and set \"Track Time\" using {0}";
		public const string StartDateOutOfRange = "Date is out of range. It can only be within \"From Date\" and \"Till Date\".";
		public const string DoNotRound = "None";
		public const string FixedDayOfMonth = "Fixed Day of Month";
		public const string EndOfMonth = "End of Month";
		public const string EndOfYear = "End Of Year";
		public const string WeekNoEndDate = "Previous week have no end date";
		public const string WeekInUse = "Week in use, cannot delete record";
		public const string WeekNotLast = "Week is not last, cannot delete record";
		public const string StartDateGreaterThanEndDate = "{0} cannot be later than '{1:d}'";
		public const string IncrorrectPrevWeek = "Incorrect \"End Date\" of previous week";
		public const string EmployeeContact = "Employee Contact";
		public const string PrintExpenseClaims = "Print Expense Claims";
		public const string HasOpenActivity = "There is one or more open time activities. Please Complete them to proceed with approval.";

		#endregion

		#region Not Traslatable Strings used in the code

		public const string EPActivityIsExpected = "Invalid cache type. EPActivity is expected.";

		#endregion

		#region Graph Names
		public const string EPDocumentRelease = "Expense Claims Release Process";
		public const string EPCustomerBilling = "Customer Billing Process";
		public const string EPSetupMaint = "Employee Management Setup";
		public const string EmployeeClassMaint = "Employee Class Maintenance";
		public const string PositionMaint = "Position Maintenance";
		public const string DepartmentMaint = "Department Maintenance";
		public const string ExpenseClaim = "Expense Claim";
		public const string ExpenseClaimEntry = "Expense Claim Entry";
		public const string EmployeeClaimsEnq = "Expense Claim Inquiry";
		public const string EmployeeClaimsApproval = "Expense Claim Approval";
		public const string EmployeeMaint = "Employee Maintenance";
		public const string EMailMessages = "Email Messages Maintenance";
		public const string EMailMessagesInq = "Email Messages Inquiry";
		public const string EPTaskInquiry = "Task Inquiry";
		public const string EPEventInquiry = "Event Inquiry";
		public const string EPActivityInq = "Activity Inquiry";
		public const string Employee = "Employee";
		public const string CalendarSynchronization = "Calendar Synchronization";
		public const string EmailRoutingMaint = "Route Email Maintenance";
		public const string EmailResponseMaint = "Email Response Maintenance";
		public const string EventTaskCategories = "Event and Task Categories Maintenance";
		public const string EventStatusTypes = "Event Status Types Maintenance";
		public const string EPPayGroupMaint = "Pay Groups Maintenance";
		public const string EPCompCodeMaint = "Compensation Codes Maintenance";
		public const string ChangesetMaint = "Changeset Maintenance";
		public const string TimeCardMaint = "Time Card Maintenance";
		public const string TimeCardSimpleMaint = "Time Card (Simple) Maintenance";
		public const string TimeCardDocument = "Document";
		public const string TimeCardDetails = "Details";
		public const string TimeCardSummary = "Summary";
		public const string TimeCardActivities = "Activities";
		public const string ActivitiesEnq = "Activities Inquiry";
		public const string EntityActivitiesMaint = "Entity Activities Maintenance";
		public const string TimeCardApprovalProcessing = "Approve Time Cards";
		public const string TimeCardReleaseProcessing = "Release Time Cards";
		public const string HourlyRateIsNotSet = "Employee rate is not set for a given date '{0}'. Please check Employee maintenance screen. Employee:'{1}'";
		public const string EmailProcessing = "Process Emails";
		public const string EmailSendReceiveMaint = "Send/Receive Emails";
		public const string EquipmentMaint = "Equipment Maintenance";
		public const string EmailsEnq = "Emails Inquiry";
		public const string Branch = "Branch";
        public const string EPSetup = "Time & Expenses Preferences";

		#endregion

		#region DAC Names
		public const string EPContractRate = "Contract Rates";
		public const string Task = "Task";
		public const string Event = "Event";
		public const string EmployeeClass = "Employee Class";
		public const string CustomerVendorClass = "Customer/Vendor Class";
		public const string EmailRouting = "Route Email";
		public const string EPCompensationCode = "Compensation Code";
		public const string EPPayGroup = "Pay Group";
		public const string EPEmployeeRate = "Employee Rate";
		public const string EPEmployeeRateByProject = "Employee Rate By Projects";
		public const string TimeCard = "Time Card";
        public const string EquipmentTimeCard = "Equipment Time Card";
		public const string TimeCardSimple = "Simple";
		public const string TimeCardDefault = "Default";
		public const string TimeCardDetail = "Time Card Summary";
        public const string EquipmentSummary = "Equipment Time Card Summary";
        public const string EquipmentDetail = "Equipment Time Card Detail";
		public const string MailBox = "Mail Box";
		public const string DoNotSplit = "Do Not Split";
		public const string WeekEmployee = Week + ", " + Employee;
		public const string ProjectEmployee = PM.Messages.Project + ", " + Employee;
		public const string WeekProject = Week + ", " + Employee;
		public const string WeekProjectEmployee = Week + ", " + PM.Messages.Project + ", " + Employee;
		public const string ContractProject = CR.Messages.Contract + " / " + PM.Messages.Project;

		#endregion

		#region View Names

		public const string Events = "Events";
		public const string Folders = "Email Accounts";
		public const string Filter = "Filter";
		public const string EmailStatistics = "Email Accounts Summary";
		public const string EmailMessages = "Email Messages";
		public const string Changeset = "Changeset";
		public const string ChangesetDetails = "Fields";
		public const string Activities = "Activities";
		public const string Timecards = "Time Cards";
		public const string ActivityType = "Activity Type";
		public const string ActivityTypes = "Activity Types";
		public const string TimeCardApproval = "Approval";
		public const string Emails = "Emails";
		public const string EquipmentAnswers = "Equipment Answers";
		public const string Selection = "Selection";
		public const string MailBoxes = "Mail Boxes";
		public const string EarningType = "Earning Type";

		#endregion

		#region Statuses

		#region EP claim statuses
		public const string Hold = "On Hold";
		public const string Balanced = "Pending Approval";
		public const string Voided = "Rejected";
		public const string Pending = "Pending";
		public const string Approved = "Approved";
		public const string Rejected = "Rejected";
		public const string Released = "Released";
		public const string Closed = "Closed";
		public const string NotRequired = "Not Required";
		public const string PartiallyApprove = "Partially";
		public const string PendingApproval = "Pending";
		#endregion

		#region Invitations

		public const string InvitationInvited = "Invited";
		public const string InvitationAccepted = "Accepted";
		public const string InvitationRejected = "Rejected";
		public const string InvitationRescheduled = "Rescheduled";
		public const string InvitationCanceled = "Canceled";

		#endregion

		#endregion //Statuses

		#region EP Mask Codes
		public const string MaskItem = "Non-Stock Item";
		public const string MaskEmployee = "Employee";
        //public const string MaskCompany = "Company Location";
        public const string MaskCompany = "Branch Location";
		#endregion

		#region Combo Values

		public const string Day = "Day";
		public const string Week = "Week";
		public const string Month = "Month";
		public const string Year = "Year";

		public const string Sunday = "Sunday";
		public const string Monday = "Monday";
		public const string Tuesday = "Tuesday";
		public const string Wednesday = "Wednesday";
		public const string Thursday = "Thursday";
		public const string Friday = "Friday";
		public const string Saturday = "Saturday";

		public const string CompleteTask = "Complete";
		public const string CancelTask = "Cancel";

		public const string PreSend = "Wait Sending";
		public const string EmailProcessed = "Processed";
		public const string PreProcess = "Pre Process";
		public const string InProcess = "In Process";
		public const string Processed = "Processed";
		public const string Waiting = "Waiting";
		public const string EmailSent = "Sent";
		public const string EmailReceived = "Received";
		public const string EmailDeleted = "Deleted";

		public const string Success = "Success";
		public const string Error = "Error";
		public const string Deleted = "Deleted";

		public const string RegularPortionOfAllPay = "Regular Portion of All Pay";
		public const string GrossPayBasis = "Gross Pay Basis";
		public const string RegularHoursBasis = "Regular Hours Basis";
		public const string RegularPayBasis = "Regular Pay Basis";
		public const string TotalHoursBasis = "Total Hours Basis";

		public const string Biweekly = "Semiweekly";
		public const string Weekly = "Weekly";
		public const string Semimonthly = "Semimonthly";
		public const string Monthly = "Monthly";

		public const string Hourly = "Hourly";
		public const string Salary = "Salaried Non-Exempt";
		public const string SalaryWithExemption = "Salaried Exempt";

		public const string UseTimecards = "Use Timecards";

        //EPLoginTypes
        public const string ContactType = "Contact";
        public const string EmployeeType = "Employee";

        public const string Validate = "Validate";
        public const string WarningOnly = "Warning Only";
        public const string None = "None";

		#endregion
	}
}
