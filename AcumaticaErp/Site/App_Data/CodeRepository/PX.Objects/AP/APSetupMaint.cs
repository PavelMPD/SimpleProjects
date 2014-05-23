using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.BQLConstants;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.AP
{
	public class APSetupMaint : PXGraph<APSetupMaint>
	{
		public PXSave<APSetup> Save;
		public PXCancel<APSetup> Cancel;
		public PXSelect<APSetup> Setup;
		//public PXSelect<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<APSetup.dfltVendorClassID>>>> DefaultVendorClass;
		public PXSelect<AP1099Box> Boxes1099;
		public PXSelect<Account, Where<Account.accountID,Equal<Required<Account.accountID>>>> Account_AccountID;
		public PXSelect<Account, Where<Account.box1099, Equal<Required<Account.box1099>>>> Account_Box1099;

		public CRNotificationSetupList<APNotification> Notifications;
		public PXSelect<NotificationSetupRecipient,
			Where<NotificationSetupRecipient.setupID, Equal<Current<APNotification.setupID>>>> Recipients;

		#region CacheAttached
		[PXDBString(10)]
		[PXDefault]
		[VendorContactType.ClassList]
		[PXUIField(DisplayName = "Contact Type")]
		[PXCheckUnique(typeof(NotificationSetupRecipient.contactID),
			Where = typeof(Where<NotificationSetupRecipient.setupID, Equal<Current<NotificationSetupRecipient.setupID>>>))]
		public virtual void NotificationSetupRecipient_ContactType_CacheAttached(PXCache sender)
		{
		}
		[PXDBInt]
		[PXUIField(DisplayName = "Contact ID")]
		[PXNotificationContactSelector(typeof(NotificationSetupRecipient.contactType),
			typeof(Search2<Contact.contactID,
				LeftJoin<EPEmployee,
							On<EPEmployee.parentBAccountID, Equal<Contact.bAccountID>,
							And<EPEmployee.defContactID, Equal<Contact.contactID>>>>,
				Where<Current<NotificationSetupRecipient.contactType>, Equal<NotificationContactType.employee>,
							And<EPEmployee.acctCD, IsNotNull>>>))]
		public virtual void NotificationSetupRecipient_ContactID_CacheAttached(PXCache sender)
		{
		}

		#endregion				

		#region Setups
		public CM.CMSetupSelect CMSetup;
		public PXSetup<GL.Company> Company;
		public PXSetup<GLSetup> GLSetup;
		#endregion

		public APSetupMaint()
		{
			GLSetup setup = GLSetup.Current;

            Boxes1099.Cache.AllowDelete = false;
            Boxes1099.Cache.AllowInsert = false;
            Boxes1099.Cache.AllowUpdate = true;
		}

		protected virtual void AP1099Box_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			AP1099Box row = (AP1099Box)e.Row;
            if (row == null || row.OldAccountID != null) return;
            
            Account acct = (Account)Account_Box1099.Select(row.BoxNbr);

			if (acct != null)
			{
                row.AccountID    = acct.AccountID;
                row.OldAccountID = acct.AccountID;
			}
		}

		protected virtual void AP1099Box_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Update)
			{
				e.Cancel = true;
				return;
			}

			foreach (AP1099Box box in Boxes1099.Cache.Updated)
			{
				if (box.OldAccountID != null && (box.AccountID == null || box.OldAccountID != box.AccountID))
				{
					Account acct = (Account)Account_AccountID.Select(box.OldAccountID);
					if (acct != null)
					{
						acct.Box1099 = null;
						Account_AccountID.Cache.Update(acct);
					}
				}

				if (box.AccountID != null && (box.OldAccountID == null || box.OldAccountID != box.AccountID))
				{
					Account acct = (Account)Account_AccountID.Select(box.AccountID);
					if (acct != null)
					{
						acct.Box1099 = box.BoxNbr;
						Account_AccountID.Cache.Update(acct);
					}
				}
			}
		}
        protected virtual void APSetup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            if (e.Row == null) return;

            PXUIFieldAttribute.SetEnabled<APSetup.invoicePrecision>(sender, e.Row, ((e.Row as APSetup).InvoiceRounding != RoundingType.Currency));
        }
        /*
        protected virtual void APSetup_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            if (DefaultVendorClass.Select().Count == 0)
            {
                DefaultVendorClass.Cache.Insert(new VendorClass());
            }
        }
		
        protected virtual void APSetup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            PXUIFieldAttribute.SetEnabled<APSetup.dfltVendorClassID>(sender, e.Row, (Setup.Cache.GetStatus(e.Row) != PXEntryStatus.Inserted));
        }
		
        protected virtual void VendorClass_VendorClassID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = ((APSetup)Setup.Select()).DfltVendorClassID;
        }
		
        protected virtual void VendorClass_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            bool mCActivated = (CMSetup.Current.MCActivated == true);

            PXUIFieldAttribute.SetVisible<VendorClass.vendorClassID>    (sender, e.Row, false);
            PXUIFieldAttribute.SetVisible<VendorClass.curyID>           (sender, null, mCActivated);
            PXUIFieldAttribute.SetVisible<VendorClass.curyRateTypeID>   (sender, null, mCActivated);
            PXUIFieldAttribute.SetVisible<VendorClass.allowOverrideCury>(sender, null, mCActivated);
            PXUIFieldAttribute.SetVisible<VendorClass.allowOverrideRate>(sender, null, mCActivated);
        }

		
        protected virtual void VendorClass_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (CMSetup.Current.MCActivated != true)
            {
                e.NewValue = null;
                e.Cancel = true;
            }
        }

        protected virtual void VendorClass_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (CMSetup.Current.MCActivated != true)
            {
                e.NewValue = string.Empty;
                e.Cancel = true;
            }
        }

        protected virtual void VendorClass_CuryRateTypeID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (CMSetup.Current.MCActivated != true)
            {
                e.Cancel = true;
            }
        }
        */
	}
}
