using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Common;

namespace PX.Objects.DR
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		#region Validation and Processing Messages
		public const string Prefix = "DR Error";
		public const string TranAlreadyExist = "Transactions for the given schedule already exist in the system.";
		public const string NoFinPeriod = "Failed to create deferred transaction since FinPeriod is not configured. Please configure FinPeriods for the range of time your revenue recognition schedules will cover. The problem was encountered for the following DeferredCode:{0}";
		public const string DeferredAmountSumError = "Sum of all open deferred transactions must be equal to Deferred Amount.";
		public const string InventoryIDRequired = "InventoryID for the Tran must be set for Deferred Revenue/Expense";
		public const string AutoPostFailed = "Auto-Posting failed for one or more Batch.";
		public const string DR_SYS_InvalidAccountType = "Invalid Deferred Account Type. Only {0} and {1} is supported but {2} was supplied.";
		public const string WrongDefCodeType = "Deferred Code Type doesnot match the Document Type";
		public const string RegenerateTran = "Transactions already exist. Do you want to recreate them?";
        public const string FixedAmtSumOverload = "Total Sum of all Fixed Amount Components is greater then the Tran Amount. Please correct this and try again.";
        public const string OnlyFixed = "All Deferred Components are marked as Fixed Amount. And the Sum of all components is not equal to the Tran Amount.";
		public const string SumOfComponentsError = "Sum of Total Amount of all Components must match Original Line Amount.";
		public const string ValidationFailed = "Can not save changes. Validation failed.";
		public const string GenerateTransactions = "Generate Transactions";
		public const string DuplicateSchedule = "There already exists a schedule for the given line. See schedule {0}";
		#endregion

		#region Translatable Strings used in the code
		public const string DocumentDateSelection = "Document Date Selection";
		#endregion

		#region Graph/Cache Names
		public const string SchedulesInq = "Deferred Schedules Inquiry";
		public const string ScheduleTransInq = "Deferred Transactions Inquiry";
		public const string ScheduleMaint = "Deferred Shedule Maintenance";
		public const string ScheduleMaint2 = "Deferred Shedule Maintenance 2.0";
		public const string SchedulesRecognition = "Deferred Schedule Recognition";
		public const string DeferredCodeMaint = "Deferred Code Maintenance";
		public const string SchedulesProjection = "Update Deferred Schedule Projection";
        public const string DRDraftScheduleProc = "Draft Schedules Release";
        public const string DRBalanceValidation = "Deferred Balance Validation";
		public const string Schedule = "Deferral Schedule";
		public const string DeferredCode = "Deferral Code";


		public const string ViewDocument = "View Document";
		public const string ViewSchedule = "View Schedule";
		public const string ViewGLBatch = "View GL Batch";
		public const string Release = "Release";
		public const string CreateTransactions = "Create Transactions";
        public const string DRScheduleEx = "Associated Schedule";
		#endregion

		#region Combo Values

		public const string Open = "Open";
		public const string Closed = "Closed";
		public const string Posted = "Posted";
		public const string Projected = "Projected";
		public const string Draft = "Draft";
		#endregion

		#region Deferred Code Type
		public const string Income = "Revenue";
		public const string Expense = "Expense";

		public const string Shedule = "Schedule";
		public const string CashReceipt = "On Payment";
		#endregion

		#region Deferred Method Type
        public const string EvenPeriods = "Evenly by Periods";
        public const string ProrateDays = "Evenly by Periods, Prorate by days";
        public const string ExactDays = "Evenly by Days in Period";
		#endregion

	}
}
