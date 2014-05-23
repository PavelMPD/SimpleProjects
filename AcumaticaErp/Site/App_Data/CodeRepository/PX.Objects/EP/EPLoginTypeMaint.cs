using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace PX.Objects.EP
{
    public class EPLoginTypeMaint : PXGraph<EPLoginTypeMaint, EPLoginType>
    {
        #region Selects

        public PXSelect<EPLoginType> LoginType;
        public PXSelect<EPLoginType, Where<EPLoginType.loginTypeID, Equal<Current<EPLoginType.loginTypeID>>>> CurrentLoginType;

        public PXSelectJoin<EPLoginTypeAllowsRole, InnerJoin<Roles, On<Roles.rolename, Equal<EPLoginTypeAllowsRole.rolename>>>,
            Where<EPLoginTypeAllowsRole.loginTypeID, Equal<Current<EPLoginType.loginTypeID>>>> AllowedRoles;

        public PXSelectJoin<EPManagedLoginType,
                        InnerJoin<EPLoginType, On<EPLoginType.loginTypeID, Equal<EPManagedLoginType.loginTypeID>>>,
                        Where<EPManagedLoginType.parentLoginTypeID, Equal<Current<EPLoginType.loginTypeID>>>> ManagedLoginTypes;

	    public PXSelectUsersInRoles UserRoles; 
        #endregion

		protected virtual void EPLoginType_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPLoginType ut = (EPLoginType) e.Row;
			ManagedLoginTypes.Cache.AllowInsert = ut != null && ut.Entity == EPLoginType.entity.Contact;
			ManagedLoginTypes.Cache.AllowUpdate = ut != null && ut.Entity == EPLoginType.entity.Contact;
		}

		protected virtual void EPLoginType_Entity_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			string newVal = (string) e.NewValue;
			EPLoginTypeAllowsRole allowed = PXSelectJoin<EPLoginTypeAllowsRole, LeftJoin<Roles, On<EPLoginTypeAllowsRole.rolename, Equal<Roles.rolename>>>, Where<Roles.guest, NotEqual<True>, And<EPLoginTypeAllowsRole.loginTypeID, Equal<Current<EPLoginType.loginTypeID>>>>>.Select(this);
			if (newVal == EPLoginType.entity.Contact && allowed != null)
			{
				throw new PXSetPropertyException(Messages.CantLoginTypeEntityChange );
			}
		}

	    public override void Persist()
	    {
			List<EPLoginTypeAllowsRole> allowed = new List<EPLoginTypeAllowsRole>(AllowedRoles.Select().RowCast<EPLoginTypeAllowsRole>());
			List<UsersInRoles> assigned = new List<UsersInRoles>(PXSelectJoin<UsersInRoles, LeftJoin<Users, On<UsersInRoles.username, Equal<Users.username>>>, Where<Users.loginTypeID, Equal<Current<EPLoginType.loginTypeID>>>>.Select(this).RowCast<UsersInRoles>());

		    assigned.RemoveAll(ur => allowed.Exists(ar => ar.Rolename == ur.Rolename));
			if (assigned.Count > 0 && AllowedRoles.View.Ask(null, null, string.Empty, Messages.ConfirmDeleteNotAllowedRoles, MessageButtons.YesNo, MessageIcon.Warning) != WebDialogResult.Yes)
			{
				return;
			}

		    foreach (UsersInRoles role in assigned)
		    {
			    UserRoles.Delete(role);
		    }

		    base.Persist();
	    }
    }
}
