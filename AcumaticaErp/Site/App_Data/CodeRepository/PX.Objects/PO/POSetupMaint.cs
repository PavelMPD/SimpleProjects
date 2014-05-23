using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.AP;
using PX.Objects.EP;
using PX.Objects.CS;

namespace PX.Objects.PO
{
	public class POSetupMaint : PXGraph<POSetupMaint>
	{
        #region Cache Attached Events
        #region NotificationSetupRecipient
        #region ContactType
        
        [PXDBString(10)]
        [PXDefault]
        [VendorContactType.ClassList]
        [PXUIField(DisplayName = "Contact Type")]
        [PXCheckUnique(typeof(NotificationSetupRecipient.contactID),
            Where = typeof(Where<NotificationSetupRecipient.setupID, Equal<Current<NotificationSetupRecipient.setupID>>>))]
        protected virtual void NotificationSetupRecipient_ContactType_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region ContactID

        [PXDBInt]
        [PXUIField(DisplayName = "Contact ID")]
        [PXNotificationContactSelector(typeof(NotificationSetupRecipient.contactType),
            typeof(Search2<Contact.contactID,
                LeftJoin<EPEmployee,
                            On<EPEmployee.parentBAccountID, Equal<Contact.bAccountID>,
                            And<EPEmployee.defContactID, Equal<Contact.contactID>>>>,
                Where<Current<NotificationSetupRecipient.contactType>, Equal<NotificationContactType.employee>,
                            And<EPEmployee.acctCD, IsNotNull>>>))]
        protected virtual void NotificationSetupRecipient_ContactID_CacheAttached(PXCache sender)
        {
        }
        #endregion        
        #endregion
        #endregion

		public PXSave<POSetup> Save;
		public PXCancel<POSetup> Cancel;
		public PXSelect<POSetup> Setup;
		public PXSelect<POSetupApproval> SetupApproval;

		public CRNotificationSetupList<PONotification> Notifications;
		public PXSelect<NotificationSetupRecipient,
			Where<NotificationSetupRecipient.setupID, Equal<Current<PONotification.setupID>>>> Recipients;

		#region Setups
		public PXSetup<GL.Company> Company;
		public CM.CMSetupSelect CMSetup;
		
	
		#endregion

		public POSetupMaint()
		{
			
		}

		protected virtual void POSetup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			POSetup row = (POSetup)e.Row;
			if (row != null)
			{
				PXUIFieldAttribute.SetEnabled<POSetup.autoReleaseLCIN>(sender, row, (row.AutoReleaseIN == true));
				PXUIFieldAttribute.SetEnabled<POSetup.copyLineNoteSO>(sender, row, (row.CopyLineDescrSO == true));
			}
		}		

		protected virtual void POSetup_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			POSetup row = (POSetup)e.Row;
			if (row != null)
			{
				if (row.AutoReleaseIN == false)
					row.AutoReleaseLCIN = false;
				if (row.CopyLineDescrSO == false)
					row.CopyLineNoteSO = false;
			}
		}

		protected virtual void POSetup_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			POSetup row = (POSetup)e.Row;
			if (row != null)
			{
				if (row.AutoReleaseIN == false)
					row.AutoReleaseLCIN = false;
				if (row.CopyLineDescrSO == false)
					row.CopyLineNoteSO = false;
			}
		}

		protected virtual void POSetupApproval_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			POSetupApproval row = (POSetupApproval)e.Row;			
			if (row != null)			
				e.Cancel = !ValidateDuplicates(sender, row, null);							
		}

		protected virtual void POSetupApproval_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			POSetupApproval row = (POSetupApproval)e.Row;
			POSetupApproval newRow = (POSetupApproval)e.NewRow;
			if (row != null && newRow != null && row != newRow && 
				 (row.OrderType != newRow.OrderType || row.AssignmentMapID != newRow.AssignmentMapID))				
					e.Cancel = !ValidateDuplicates(sender, newRow, row);												
		}

		private bool ValidateDuplicates(PXCache sender, POSetupApproval row, POSetupApproval oldRow)
		{
			foreach (POSetupApproval sibling in SetupApproval.Select())
			{
				if (String.Equals(sibling.OrderType, row.OrderType, StringComparison.OrdinalIgnoreCase) &&
						sibling.AssignmentMapID == row.AssignmentMapID &&
						row.ApprovalID != sibling.ApprovalID)
				{
					if (oldRow == null || oldRow.OrderType != row.OrderType)
						sender.RaiseExceptionHandling<POSetupApproval.orderType>(
							row, row.OrderType, new PXSetPropertyException(ErrorMessages.DuplicateEntryAdded)
							);
					if (oldRow == null || oldRow.AssignmentMapID != row.AssignmentMapID)
						sender.RaiseExceptionHandling<POSetupApproval.assignmentMapID>(
							row, row.AssignmentMapID, new PXSetPropertyException(ErrorMessages.DuplicateEntryAdded)
							);
					return false;
				}
			}
			PXUIFieldAttribute.SetError<POSetupApproval.orderType>(sender, row, null);
			PXUIFieldAttribute.SetError<POSetupApproval.assignmentMapID>(sender, row, null);
			return true;
		}
	}
}
