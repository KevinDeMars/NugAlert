using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NugAlert
{
    public static class DateTimeExtensions
    {
        public static DateTime LastSunday(this DateTime date)
        {
            int[] daysToAdd = { 0, -1, -2, -3, -4, -5, -6 };
            return date.AddDays(daysToAdd[(int)date.DayOfWeek]);
        }

        public static int WeekDifference(DateTime lhs, DateTime rhs)
        {
            return (lhs.LastSunday().DayOfYear - rhs.LastSunday().DayOfYear) / 7;
        }
    }
}
