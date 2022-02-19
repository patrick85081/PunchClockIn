using System.Globalization;

namespace Punches.Models;

public static class ClockInExtensions
{
    public static string ToClockInMonth(this DateTime date)
    {
        var taiwanCalendar = new TaiwanCalendar();
        return $"{taiwanCalendar.GetYear(date)}{taiwanCalendar.GetMonth(date):00}";
    }

    public static string ToWeekString(this DayOfWeek week) =>
        week switch
        {
            DayOfWeek.Sunday => "星期日",
            DayOfWeek.Monday => "星期一",
            DayOfWeek.Tuesday => "星期二",
            DayOfWeek.Wednesday => "星期三",
            DayOfWeek.Thursday => "星期四",
            DayOfWeek.Friday => "星期五",
            DayOfWeek.Saturday => "星期六",
            _ => "",
        };

    public static bool IsNoClockIn(this ClockIn clockIn)
    {
        return System.DateTime.Today > clockIn.Date &&
               (clockIn.WorkOn == null || clockIn.WorkOff == null)
               && string.IsNullOrEmpty(clockIn.Status);
    }
    public static IEnumerable<DateTime> GetMonthDays(string Month)
    {
        var taiwanCalendar = new TaiwanCalendar();
        var year = int.Parse(Month.Substring(0, 3));// + 1911;
        var month = int.Parse(Month.Substring(3, 2));
        var dayInMonth = taiwanCalendar.GetDaysInMonth(year, month);
        // var dayInMonth = DateTime.DaysInMonth(year, month);
        var days = (
            from day in Enumerable.Range(1, dayInMonth)
            // select  new DateTime(year, month, day)
            select  new DateTime(year, month, day, taiwanCalendar)
        );
        return days;
    }

    public static DateTime FromClockMonth(this string Month)
    {
        var year = int.Parse(Month.Substring(0, 3));
        var month = int.Parse(Month.Substring(3, 2));
        return new DateTime(year, month, 1, new TaiwanCalendar());
    }

    public static IEnumerable<ClockIn> JoinToEmptyDate(this IEnumerable<ClockIn> clockIns, QueryParameter parameter) =>
        clockIns.JoinToEmptyDate(parameter.Month, parameter.Name);
    public static IEnumerable<ClockIn> JoinToEmptyDate(this IEnumerable<ClockIn> clockIns, string month, string name)
    {

        var newClockIn = (
                from date in GetMonthDays(month)
                join clockIn in clockIns on date equals clockIn.Date into x
                from xx in x.DefaultIfEmpty()
                select xx ?? new ClockIn()
                {
                    Id = ClockIn.GetId(date, name),
                    Date = date,
                }
            )
            .ToArray();

        return newClockIn!;
    }
}