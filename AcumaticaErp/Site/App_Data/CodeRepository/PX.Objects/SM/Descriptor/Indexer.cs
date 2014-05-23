using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.SM;

namespace PX.Objects.SM.Descriptor
{
	public class PXSelectPureUsers<TPrimary, WhereUsers> : PXSelectPureUsers<TPrimary>
		where TPrimary : IBqlTable
		where WhereUsers : IBqlWhere, new()
	{
		public PXSelectPureUsers(PXGraph graph)
			: base(graph)
		{
			View.WhereAnd<WhereUsers>();
		}
	}
	
	public class PXSelectPureUsers<TPrimary> : PXSelectUsers<TPrimary>
		where TPrimary : IBqlTable
	{
        public PXSelectPureUsers(PXGraph graph)
            : base(graph)
        {
        }
        
        public PXSelectPureUsers(PXGraph graph, Delegate handler) : base(graph, handler)
		{
		}

		// override for sendind notification email
        protected override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			Users row = (Users)e.Row;
			if (e.TranStatus != PXTranStatus.Open) return;

			//If entry with that username already exists - restore its Guid
			Guid? restoredGuid = Access.GetGuidFromDeletedUser(((Users)e.Row).Username);
			if (restoredGuid != null)
				((Users)e.Row).PKID = restoredGuid;

			//TODO: Need move to ResetPassword action
			if (row.Source == PXUsersSourceListAttribute.Application)
			{
				bool resetPassword = (e.Operation & PXDBOperation.Command) == PXDBOperation.Insert;
				if (!resetPassword && row.Password != null && (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
				{
					Users savedUses = PXSelectReadonly<Users, Where<Users.pKID, Equal<Required<Users.pKID>>>>.SelectWindowed(_Graph, 0, 1, row.PKID);
					resetPassword = savedUses.Password == null;
				}

				if (resetPassword)
				{
					row.NewPassword = row.Password;
					row.ConfirmPassword = row.Password;
					Access.SetPassword(false, row);
                    SendLoginInfo(row);
				}
			}
			PXUIFieldAttribute.SetVisible<Users.password>(sender, e.Row, false);
			PXUIFieldAttribute.SetVisible<Users.newPassword>(sender, e.Row, true);
		}

        protected void SendLoginInfo(Users row)
        {
            TemplateNotificationGenerator templateSender = null;
            var emailPref = (PreferencesEmail)PXSelect<PreferencesEmail>.
                SelectWindowed(new PXGraph(), 0, 1);
            if (emailPref != null && emailPref.UserWelcomeNotificationId != null)
            {
                try
                {
                    templateSender = TemplateNotificationGenerator.Create(row, (int)emailPref.UserWelcomeNotificationId);
                    templateSender.LinkToEntity = false;
                }
                catch (StackOverflowException) { throw; }
                catch (OutOfMemoryException) { throw; }
                catch { }
            }

            if (templateSender != null)
            {
                using (new PXConnectionScope())
                {
                    templateSender.Body = templateSender.Body.Replace("********", row.Password);
                    templateSender.Send();
                }
            }
        }
	}
}
