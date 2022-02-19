using System;
using System.Globalization;
using System.Windows.Data;
using Punches.Models;
using Punches.Repository;

namespace PunchClockIn.Converters;

public class NoWorkTimeConverter : IValueConverter, IDisposable
{
    private DataContext db = new DataContext();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {

        var clockIn = value as ClockIn;
        if (clockIn == null) return false;

        // 假日
        if (clockIn.Date.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Saturday) return false;

        // 還沒到
        if (clockIn.Date > DateTime.Today) return false;

        // 請假
        if (!string.IsNullOrWhiteSpace(clockIn.Status)) return false;

        var day = new HolidayRepository(db).GetHoliday(clockIn.Date);
        if (day == null || day.IsHoliday) return false;

        // 上班未打卡
        if (!clockIn.WorkOn.HasValue && DateTime.Now > DateTime.Today.AddHours(9).AddMinutes(30)) return true;
        
        // 下班未打卡
        if (!clockIn.WorkOff.HasValue && DateTime.Now > DateTime.Today.AddHours(18).AddMinutes(30)) return true;

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        db?.Dispose();
    }
}