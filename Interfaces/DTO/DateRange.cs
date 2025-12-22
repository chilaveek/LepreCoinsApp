using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces.DTO
{
    public record DateRange(
        DateTime StartDate,
        DateTime EndDate)
    {
        public static DateRange CurrentMonth()
        {
            var now = DateTime.Now;
            return new DateRange(
                new DateTime(now.Year, now.Month, 1),
                new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)));
        }

        public static DateRange CurrentYear()
        {
            var now = DateTime.Now;
            return new DateRange(
                new DateTime(now.Year, 1, 1),
                new DateTime(now.Year, 12, 31));
        }

        public static DateRange Last30Days()
        {
            var now = DateTime.Now;
            return new DateRange(
                now.AddDays(-30),
                now);
        }
    }

}
