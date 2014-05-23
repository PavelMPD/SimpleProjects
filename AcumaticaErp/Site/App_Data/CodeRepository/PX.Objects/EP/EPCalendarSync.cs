using System;
using System.Collections.Generic;
using System.Security.Permissions;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.EP
{
	public class EPCalendarSync : PXGraph
	{
		//[SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
		public IEnumerable<EPActivity> GetCalendarEvents(Guid settingsId)
		{
			var result = new List<EPActivity>();
			Load();
			if (IsPublished(settingsId)) result.AddRange(GetEvents(settingsId)); //read items before scope desposing
			return result;
		}

		public bool IsPublished(Guid id)
		{
			PXResultset<SMCalendarSettings> set = PXSelect<SMCalendarSettings,
				Where<SMCalendarSettings.urlGuid,
					Equal<Required<SMCalendarSettings.urlGuid>>>>.Select(this, id);
			if (set != null && set.Count > 0) return ((SMCalendarSettings)set[0]).IsPublic.Value;
			return false;
		}

		public virtual IEnumerable<EPActivity> GetEvents(Guid id)
		{
			foreach (var item in PXSelectJoin<EPActivity,
				LeftJoin<EPAttendee, On<EPAttendee.eventID, Equal<EPActivity.taskID>>,
				InnerJoin<SMCalendarSettings, On<SMCalendarSettings.userID, Equal<EPActivity.createdByID>,
					Or<SMCalendarSettings.userID, Equal<EPAttendee.userID>>>>>,
				Where2<
					Where<EPActivity.classID, Equal<CRActivityClass.events>>,
					And<SMCalendarSettings.urlGuid, Equal<Required<SMCalendarSettings.urlGuid>>>>,
				OrderBy<Desc<EPActivity.priority, Asc<EPActivity.startDate, Asc<EPActivity.endDate>>>>>.
					Select(this, id))
			{
				yield return item;
			}
		}
	}
}
