using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.Access;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.SM.Descriptor;

namespace PX.SM
{
	public class PXSelectAllowedRoles:PXSelectBase<EPLoginTypeAllowsRole>
	{
		bool isLoginTypeUpdated;

		public PXSelectAllowedRoles(PXGraph graph)
		{
			_Graph = graph;
			View = new PXView(_Graph, false, new Select<EPLoginTypeAllowsRole, Where<EPLoginTypeAllowsRole.loginTypeID, Equal<Current<Users.loginTypeID>>>>(), new PXSelectDelegate(ViewDelegate));

			_Graph.FieldUpdated.AddHandler(typeof(EPLoginTypeAllowsRole), typeof(EPLoginTypeAllowsRole.selected).Name, SelectedFieldUpdated);
			_Graph.FieldUpdated.AddHandler(typeof(Users), typeof(Users.loginTypeID).Name, UsersLoginTypeIDFieldUpdated);
			_Graph.RowPersisting.AddHandler(typeof(EPLoginTypeAllowsRole), RowPersisting);
			_Graph.RowPersisted.AddHandler(typeof(Users), UsersRowPersisted);
			_Graph.RowSelected.AddHandler(typeof(Users), UsersRowSelected);
		}

		protected virtual IEnumerable ViewDelegate()
		{
			PXCache userCache = _Graph.Caches[typeof (Users)];
			Users user = (Users) userCache.Current;
			if (user == null || user.Username == null) yield break;

			if (user.Source == PXUsersSourceListAttribute.Application || user.OverrideADRoles == true) // editable roles for native users
			{
				bool IsUserInserted = userCache.GetStatus(user) == PXEntryStatus.Inserted;

				Dictionary<string, UsersInRoles> assigned = PXSelect<UsersInRoles, Where<UsersInRoles.username, Equal<Current<Users.username>>>>.Select(_Graph).RowCast<UsersInRoles>().ToDictionary(ur => ur.Rolename);
				Dictionary<string, EPLoginTypeAllowsRole> allowed = new Dictionary<string, EPLoginTypeAllowsRole>();
				if (user.LoginTypeID == null) // all roles
				{
					foreach (EPLoginTypeAllowsRole arole in from Roles r in PXSelect<Roles, Where<Roles.guest, Equal<Current<Users.guest>>, Or<Current<Users.guest>, Equal<False>>>>.Select(_Graph).RowCast<Roles>() select new EPLoginTypeAllowsRole { Rolename = r.Rolename, IsDefault = false })
					{
						allowed.Add(arole.Rolename, arole);
						Insert(arole);
						Cache.IsDirty = false;
					}
				}
				else // from appropriate EPLoginTypeAllowsRole
				{
					allowed = PXSelect<EPLoginTypeAllowsRole, Where<EPLoginTypeAllowsRole.loginTypeID, Equal<Current<Users.loginTypeID>>>>.Select(_Graph).RowCast<EPLoginTypeAllowsRole>().ToDictionary(ar => ar.Rolename);
				}

				HashSet<string> all = new HashSet<string>(assigned.Keys);
				all.UnionWith(allowed.Keys);

				foreach (string rolename in all.Where(PXAccess.IsRoleEnabled))
				{
					EPLoginTypeAllowsRole role;
					allowed.TryGetValue(rolename, out role);

					UsersInRoles urole;
					assigned.TryGetValue(rolename, out urole);

					if (urole != null && role != null)
					{
						role.Selected = true;
						yield return role;
					}
					else if (urole == null && role != null && role.IsDefault == true && (isLoginTypeUpdated || IsUserInserted)) // add user role
					{
						_Graph.Caches[typeof(UsersInRoles)].Insert(new UsersInRoles { Rolename = role.Rolename });
						role.Selected = true;
						yield return role;
					}
					else if (urole != null) // delete user role
					{
						_Graph.Caches[typeof(UsersInRoles)].Delete(urole);
					}
					else if (role != null)
					{
						role.Selected = false;
						yield return role;
					}
				}
			}
			else // readonly mapped roles for AD users
			{
				foreach (string role in (PXUsersSelectorAttribute.GetADMappedRolesBySID(user.ExtRef) ?? new string[0]))
				{
					yield return new EPLoginTypeAllowsRole {Rolename = role, Selected = true};
				}
			}
		}

		protected virtual void SelectedFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPLoginTypeAllowsRole role = (EPLoginTypeAllowsRole) e.Row;
			UsersInRoles urole = PXSelect<UsersInRoles, 
				Where<UsersInRoles.rolename, Equal<Required<UsersInRoles.rolename>>,
					And<UsersInRoles.username, Equal<Required<UsersInRoles.username>>>>>.Select(sender.Graph, role.Rolename, ((Users)_Graph.Caches[typeof(Users)].Current).Username);
			if (role.Selected == true && urole == null) // add user role
			{
				sender.Graph.Caches[typeof(UsersInRoles)].Insert(new UsersInRoles { Rolename = role.Rolename });
			}
			if (role.Selected != true && urole != null) // delete user role
			{
				sender.Graph.Caches[typeof(UsersInRoles)].Delete(urole);
			}
		}

		protected virtual void UsersLoginTypeIDFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			isLoginTypeUpdated = true;
		}

		protected virtual void UsersRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Users user = (Users)e.Row;
			if (user == null) return;

			PXUIFieldAttribute.SetEnabled<EPLoginTypeAllowsRole.selected>(_Graph.Caches[typeof(EPLoginTypeAllowsRole)], null, user.Source == PXUsersSourceListAttribute.Application || user.OverrideADRoles == true);
			PXUIFieldAttribute.SetEnabled<EPLoginTypeAllowsRole.rolename>(_Graph.Caches[typeof(EPLoginTypeAllowsRole)], null, false);
		}

		protected virtual void UsersRowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			isLoginTypeUpdated = false;
		}

		protected virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}
	}

	[Serializable]
	public partial class ADUserFilter: IBqlTable
	{
		#region Username
		public abstract class username : IBqlField { }
		[PXDBString(64, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Active Directory User")]
		[PXADUsersSelector]
		[PXDefault]
		public virtual String Username { get; set; }
		#endregion

	}

	[Serializable]
	public partial class PXUsers : PXCacheExtension<Users>
	{
		#region DataSourceID
		public abstract class dataSourceID : IBqlField
		{
		}
		protected Int32? _ContactID;
		[PXInt]
		[PXUIField(DisplayName = "Contact")]
		[PXSelector(typeof(Search2<Contact.contactID,
				LeftJoin<Users, On<Contact.userID, Equal<Users.pKID>>,
				LeftJoin<BAccount, On<BAccount.defContactID, Equal<Contact.contactID>>>>,
			Where<Current<Users.guest>, Equal<True>, And<Contact.contactType, Equal<ContactTypesAttribute.person>,
				 Or<Current<Users.guest>, NotEqual<True>, And<Contact.contactType, Equal<ContactTypesAttribute.employee>, And<BAccount.bAccountID, IsNotNull>>>>>>),
			typeof(Contact.displayName),
			typeof(Contact.salutation),
			typeof(Contact.fullName),
			typeof(Contact.eMail),
			typeof(Users.username),
			DescriptionField = typeof(Contact.displayName))]
		[PXRestrictor(typeof(Where<Contact.eMail, IsNotNull, Or<Current<Users.source>, Equal<PXUsersSourceListAttribute.activeDirectory>>>), PX.Objects.CR.Messages.ContactWithoutEmail, typeof(Contact.displayName))]
		[PXRestrictor(typeof(Where<Contact.userID, IsNull, Or<Contact.userID, Equal<Current<Users.pKID>>>>), PX.Objects.CR.Messages.ContactWithUser, typeof(Contact.displayName))]
		[PXDBScalar(typeof(Search<Contact.contactID, Where<Contact.userID, Equal<Users.pKID>>>))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? ContactID
		{
			get
			{
				return this._ContactID;
			}
			set
			{
				this._ContactID = value;
			}
		}
		#endregion
	}

	[PXPrimaryGraph(typeof(Users))]
	public class AccessUsers : Access
	{
		public PXSelect<Contact, Where<Contact.contactID, Equal<Current<Users.contactID>>>> contact;
		public PXSelectAllowedRoles AllowedRoles;
		public PXFilter<ADUserFilter> ADUser;
		
		[PXDBInt]
		[PXUIField(DisplayName = "User Type")]
		[PXSelector(typeof(Search<EPLoginType.loginTypeID>), SubstituteKey = typeof(EPLoginType.loginTypeName))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void Users_LoginTypeID_CacheAttached(PXCache sender) { }

		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "First Name")]
		[PXFormula(typeof(Selector<Users.contactID, Contact.firstName>))]
		protected virtual void Users_FirstName_CacheAttached(PXCache sender) { }

		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Last Name")]
		[PXFormula(typeof(Selector<Users.contactID, Contact.lastName>))]
		protected virtual void Users_LastName_CacheAttached(PXCache sender) { }

		[PXDBString(128)]
		[PXUIField]
		[PXFormula(typeof(Selector<Users.contactID, Contact.eMail>))]
		[PXDefault]
		protected virtual void Users_Email_CacheAttached(PXCache sender) { }

		[PXDBBool]
		[PXFormula(typeof(Selector<Users.loginTypeID, EPLoginType.requireLoginActivation>))]
		protected virtual void Users_IsPendingActivation_CacheAttached(PXCache sender) { }

		[PXDBBool]
		[PXUIField(DisplayName = "Guest Account")]
		[PXFormula(typeof(IsNull<Selector<Users.loginTypeID, EPLoginType.isExternal>, False>))]
		protected virtual void Users_Guest_CacheAttached(PXCache sender) { }

		[PXDBBool]
		[PXUIField(DisplayName = "Force User to Change Password on Next Login")]
		[PXFormula(typeof(Switch<Case<Where<Selector<Users.loginTypeID, EPLoginType.resetPasswordOnLogin>, Equal<True>>, True>, False>))] // guarantee not null
		protected virtual void Users_PasswordChangeOnNextLogin_CacheAttached(PXCache sender) { }

		[PXBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Generate Password")]
		protected virtual void Users_GeneratePassword_CacheAttached(PXCache sender) { }

		protected virtual IEnumerable roleList()
		{
			yield break;
		}

		public PXAction<Users> AddADUser;
		[PXUIField(DisplayName = "Add Active Directory User", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public IEnumerable addADUser(PXAdapter adapter)
		{
			ADUser.AskExt();
			return adapter.Get();
		}

		public PXAction<Users> AddADUserOK;
		[PXUIField(DisplayName = "OK", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public IEnumerable addADUserOK(PXAdapter adapter)
		{
			ADUser.VerifyRequired();
			ADUserFilter filter = ADUser.Current;
			Users aduser = PXUsersSelectorAttribute.GetADUserByName(UserList.Cache, filter.Username);
			if (aduser != null)
			{
                PXActiveDirectorySyncMembershipProvider.CheckAndRenameDeletedADUser(aduser.Username, aduser.ExtRef);
				if (adapter.ImportFlag)
				{
					UserList.Insert(aduser);
				}
				else
				{
					AccessUsers graph = CreateInstance<AccessUsers>();
					graph.UserList.Insert(aduser);
					throw new PXRedirectRequiredException(graph, "New AD User");
				}
			}
			return adapter.Get();
		}

		protected virtual void Users_OverrideADRoles_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			Users user = (Users) e.Row;
			bool oldval = user.OverrideADRoles == true;
			bool newval = (bool?)e.NewValue == true;

			if (oldval != newval && !newval
				&& ((UsersInRoles)PXSelect<UsersInRoles, Where<UsersInRoles.applicationName, Equal<Current<Users.applicationName>>, And<UsersInRoles.username, Equal<Current<Users.username>>>>>.SelectSingleBound(this, new object[0])) != null
				&& UserList.Ask(PX.Objects.CR.Messages.Confirmation, string.Format(PX.Objects.CR.Messages.DeleteLocalRoles, user.Username), MessageButtons.YesNo, MessageIcon.Warning) != WebDialogResult.Yes)
			{
				e.NewValue = true;
				e.Cancel = true;
			}
		}

		protected override void Users_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Users user = (Users)e.Row;
			if (user == null) return;

			base.Users_RowSelected(sender, e);

			AllowedRoles.Cache.AllowInsert = false;
			PXDefaultAttribute.SetPersistingCheck<Users.contactID>(sender, user, user.Guest == true ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);

			PXUIFieldAttribute.SetWarning<Users.overrideADRoles>(sender, user, user.OverrideADRoles == true ? Messages.IgnoredADRoles : null);
		}

        protected virtual void Users_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
        {
            Users oldRow = e.Row as Users;
            Users row = e.NewRow as Users;

            if (row == null || oldRow == null || oldRow.ContactID == null) return;

            if (row.Guest == true && oldRow.Guest != true)
            {
                if (contact.View.Ask(
                    MyMessages.EmployeeContactWouldBeCleared,
                    MessageButtons.YesNo) != WebDialogResult.Yes)
                    e.Cancel = true;
            }
            else if (row.Guest != true && oldRow.Guest == true)
            {
                if (contact.View.Ask(
                    MyMessages.ExternalUserContactWouldBeCleared,
                    MessageButtons.YesNo) != WebDialogResult.Yes)
                    e.Cancel = true;
            }
        }

		protected virtual void Users_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			Users oldRow = e.OldRow as Users;
			Users row = e.Row as Users;
			Contact c = PXSelect<Contact, Where<Contact.userID, Equal<Current<Users.pKID>>>>.SelectSingleBound(this, new object[] { row });

			if (row == null || oldRow == null || row.Guest == oldRow.Guest || c == null) return;

			c.UserID = null;
			contact.Update(c);
		}

		protected virtual void Users_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			Contact c = (Contact)contact.View.SelectSingleBound(new object[] {e.Row});
			if (c == null) return;

			c.UserID = null;
			contact.Update(c);
		}

		protected virtual void Users_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			Users user = (Users)e.Row;
			if (user == null) return;

			PXResultset<EPAssignmentRoute> res = PXSelectJoin<EPAssignmentRoute, InnerJoin<EPAssignmentMap, On<EPAssignmentRoute.assignmentMapID, Equal<EPAssignmentMap.assignmentMapID>>>, Where<EPAssignmentRoute.ownerID, Equal<Current<Users.pKID>>>>.Select(this, new object[] { user });
			foreach (PXResult<EPAssignmentRoute, EPAssignmentMap> result in res)
			{
				throw new PXSetPropertyException(PX.Objects.EP.Messages.UserParticipateInAssignmentMap, user.Username, ((EPAssignmentMap)result).Name);
			}
		}

		protected virtual void Users_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			Users user = (Users) e.Row;
			Contact c = (Contact)contact.View.SelectSingleBound(new object[] { user });
			if (user == null || c == null || user.PKID == c.UserID) return;

			c = (Contact)contact.Cache.CreateCopy(c);
			c.UserID = user.PKID;
			contact.Update(c);
		}

		public override void Persist()
		{
			Contact cnt = contact.SelectSingle();
			cnt = (Contact)contact.Cache.CreateCopy(cnt);
			if (UserList.Current != null)
			{
				UserList.Current.IsAssigned = cnt != null;

				foreach (Contact existing in PXSelect<Contact, Where<Contact.userID, Equal<Current<Users.pKID>>>>.Select(this))
				{
					existing.UserID = null;
					contact.Update(existing);
				}

				if (cnt != null)
				{
					cnt.UserID = UserList.Current.PKID;
					contact.Update(cnt);
				}
			}

			foreach (Users user in UserList.Cache.Deleted)
			{
				cnt = (Contact)contact.View.SelectSingleBound(new object[] {user});
				if (cnt != null)
				{
					cnt.UserID = null;
					contact.Update(cnt);
				}
			}

			if (UserList.Current != null && UserList.Current.OverrideADRoles != true && UserList.Current.Source == PXUsersSourceListAttribute.ActiveDirectory)
			{
				foreach (UsersInRoles userrole in RoleList.Select())
				{
					RoleList.Delete(userrole);
				}
			}

			base.Persist();
		}
	}
}