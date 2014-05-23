using System;

namespace DebtCollection.Common
{
    public class DatesHelper
    {
        public static int GetDaysTillNextFriday(DateTime fromDate)
        {
            var delta = ((int)DayOfWeek.Friday) - (int)fromDate.DayOfWeek;
            if (delta < 0)
            {
                // if today it is sunday or saturday
                delta = Math.Abs(delta) + (int)DayOfWeek.Friday;
            }
            return delta;
        }

        public static int GetDaysFromLastFriday(DateTime currentDate)
        {
            var delta = (int)currentDate.DayOfWeek - (int)DayOfWeek.Friday;
            if (delta < 0)
            {
                // if today it is sunday or saturday
                delta =
                    ((int)DayOfWeek.Friday - Math.Abs(delta))
                    + 2; // Sun, Sat, Fri
            }
            return delta;
        }
    }
}
