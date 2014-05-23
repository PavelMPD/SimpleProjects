using System;
using PX.Common;
using PX.Data;

namespace PX.Objects.CS
{
	public class CustomTimeRegionProvider : ITimeRegionProvider
	{
		private sealed class TimeRegion : ITimeRegion
		{
			private readonly CustomTimeRegionProvider _provider;
			private readonly string _id;

			public TimeRegion(CustomTimeRegionProvider provider, string id)
			{
				if (provider == null) throw new ArgumentNullException("provider");
				if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");

				_provider = provider;
				_id = id;
			}

			public TimeZoneInfo.AdjustmentRule GetAdjustmentRule(int year)
			{
				return _provider.GetAdjustmentRule(year, _id);
			}

			public bool SupportsDaylightSavingTime
			{
				get { return true; }
			}
		}

		private sealed class DefinitionParameters
		{
			private readonly string _id;
			private readonly int _year;

			public DefinitionParameters(string id, int year)
			{
				_id = id;
				_year = year;
			}

			public string Id
			{
				get { return _id; }
			}

			public int Year
			{
				get { return _year; }
			}
		}

		private sealed class Definition : IPrefetchable<DefinitionParameters>
		{
			private const string _SLOT_KEY = "_CUSTOM_TIME_REGION_PROVIDER_SLOT_KEY_";

			private static readonly ITimeRegionProvider _systemProvider;
			private static readonly Type[] _tables;

			private TimeZoneInfo.AdjustmentRule _rule;

			static Definition()
			{
				_systemProvider = new SystemTimeRegionProvider();
				_tables = new Type[] { typeof(DaylightShift) };
			}

			public TimeZoneInfo.AdjustmentRule Rule
			{
				get { return _rule; }
			}

			public void Prefetch(DefinitionParameters parameter)
			{
				_rule = Initialize(parameter);
			}

			private TimeZoneInfo.AdjustmentRule Initialize(DefinitionParameters parameter)
			{
				DaylightShift row = null;
				var id = parameter.Id;
				var year = parameter.Year;
				using (new PXConnectionScope())
				using (PXDataRecord rec = PXDatabase.SelectSingle<DaylightShift>
					(
						new PXDataField(typeof(DaylightShift.fromDate).Name),
						new PXDataField(typeof(DaylightShift.toDate).Name),
						new PXDataField(typeof(DaylightShift.shift).Name),
						new PXDataFieldValue(typeof(DaylightShift.isActive).Name, PXDbType.Bit, 1),
						new PXDataFieldValue(typeof(DaylightShift.year).Name, PXDbType.Int, year),
						new PXDataFieldValue(typeof(DaylightShift.timeZone).Name, PXDbType.VarChar, id)
					))
				{
					if (rec != null)
					{
						row = new DaylightShift();
						row.IsActive = true;
						row.Year = year;
						row.TimeZone = id;
						row.FromDate = rec.GetDateTime(0);
						row.ToDate = rec.GetDateTime(1);
						row.Shift = rec.GetInt32(2);
					}
				}

				if (row == null || string.IsNullOrEmpty(row.TimeZone))
				{
					return _systemProvider.FindTimeRegionByTimeZone(id).With(_ => _.GetAdjustmentRule(year));
				}

				var start = new DateTime(year, 1, 1);
				var end = new DateTime(year, 12, 31);
				var delta = TimeSpan.FromMinutes(row.Shift ?? 0);
				var tranStart = GetTransitionTime((DateTime)row.FromDate);
				var tranEnd = GetTransitionTime((DateTime)row.ToDate);
				return TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(start, end, delta, tranStart, tranEnd);
			}

			public static TimeZoneInfo.AdjustmentRule GetRule(int year, string id)
			{
				var parameter = new DefinitionParameters(id, year);
				var key = _SLOT_KEY + year + id;
				using (new PXConnectionScope())
				{
					return PXDatabase.GetSlot<Definition, DefinitionParameters>(key, parameter, _tables).
						With(_ => _.Rule);
				}
			}
		}

		public ITimeRegion FindTimeRegionByTimeZone(string id)
		{
			if (string.IsNullOrEmpty(id)) return null;

			return new TimeRegion(this, id);
		}

		private TimeZoneInfo.AdjustmentRule GetAdjustmentRule(int year, string id)
		{
			return Definition.GetRule(year, id);
		}

		private static TimeZoneInfo.TransitionTime GetTransitionTime(DateTime source)
		{
			var timeOfDay = new DateTime(new TimeSpan(source.Hour, source.Minute, source.Second).Ticks);
			var month = source.Month;
			var day = source.Day;
			return TimeZoneInfo.TransitionTime.CreateFixedDateRule(timeOfDay, month, day);
		}
	}
}
