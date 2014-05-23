using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.SM
{
	public class MyProfileMaint : SMAccessPersonalMaint
	{
		#region Selects

		public PXSelect<SMLanguageSearchSettings> LanguageSearchSettings;
		public PXSelect<Contact, Where<Contact.userID, Equal<Optional<Users.pKID>>>> Contact;

		#endregion

		#region Data Handlers

		public virtual IEnumerable languageSearchSettings()
		{
			var cache = LanguageSearchSettings.Cache;
			var oldDirtyValue = cache.IsDirty;
			foreach (PXResult<SMLanguageSearchSettings, WikiPageLanguage> record in
				PXSelectJoinGroupBy<SMLanguageSearchSettings,
					RightJoin<WikiPageLanguage, On<WikiPageLanguage.language, Equal<SMLanguageSearchSettings.name>>>,
					Where<SMLanguageSearchSettings.userID, IsNull,
						Or<SMLanguageSearchSettings.userID, Equal<Current<AccessInfo.userID>>>>,
					Aggregate<GroupBy<WikiPageLanguage.language, 
						GroupBy<SMLanguageSearchSettings.userID, 
						GroupBy<SMLanguageSearchSettings.active>>>>>.
					Select(this))
			{
				var langSettings = (SMLanguageSearchSettings)record;
				var pageLanguage = (WikiPageLanguage)record;
				if (langSettings.UserID == null)
				{
					var fieldValues = new OrderedDictionary
					                  	{
					                  		{cache.GetField(typeof(SMLanguageSearchSettings.name)), pageLanguage.Language},
					                  		{cache.GetField(typeof(SMLanguageSearchSettings.userID)), PXAccess.GetUserID()},
					                  		{cache.GetField(typeof(SMLanguageSearchSettings.active)), false}
					                  	};
					cache.Insert(fieldValues);
					langSettings = (SMLanguageSearchSettings)cache.Current;
				}
				yield return langSettings;
			}
			cache.IsDirty = oldDirtyValue;
		}

		#endregion

		#region Event Handlers
		[PXDBString(50)]
		[PXUIField(DisplayName = "Phone", Visibility = PXUIVisibility.SelectorVisible)]
		[PX.Objects.CR.PhoneValidation]
		protected virtual void Users_Phone_CacheAttached(PXCache sender)
		{
			
		}

		protected void Users_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Users user = (Users)e.Row;
			if (user == null) return;

			bool isNotFromAD = user.Source != PXUsersSourceListAttribute.ActiveDirectory;
			PXDefaultAttribute.SetPersistingCheck<Users.password>(sender, user, isNotFromAD ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<Users.email>(sender, user, isNotFromAD ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
		}

		protected override void UserPreferences_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.UserPreferences_RowSelected(sender, e);

			ResetTimeZone.SetVisible(true);
		}

		#endregion

		#region Public Methods

		public override string GetUserTimeZoneId(string username)
		{
			var result = base.GetUserTimeZoneId(username);
			if (string.IsNullOrEmpty(result))
			{
				var set = PXSelectJoin<CSCalendar,
					InnerJoin<EPEmployee, On<EPEmployee.calendarID, Equal<CSCalendar.calendarID>>,
						InnerJoin<Users, On<Users.pKID, Equal<EPEmployee.userID>>>>,
					Where<Users.username, Equal<Required<Users.username>>>>.
					Select(this, username);
				if (set != null && set.Count > 0) result = ((CSCalendar)set[0][typeof(CSCalendar)]).TimeZone;
			}
			return result;
		}

		[PXUIField(DisplayName = "Change Email", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = false)]
		[PXButton]
		public override void changeEmail()
		{
			base.changeEmail();
			foreach (Contact copy in Contact.Select(UserProfile.Current.PKID)
				       .RowCast<Contact>()
				       .Select(contact => (Contact) Contact.Cache.CreateCopy(contact)))
			{
				copy.EMail = UserProfile.Current.Email;
				Contact.Update(copy);
			}
			Actions.PressSave();
		}

		#endregion
	}
}
