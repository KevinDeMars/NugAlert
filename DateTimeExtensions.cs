using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NugAlert
{
    public static class DateTimeExtensions
    {
        public static DateTime GetLastSunday(this DateTime date)
        {
            while (date.DayOfWeek != DayOfWeek.Sunday)
                date = date.AddDays(-1);
            return date;
        }
    }
}
