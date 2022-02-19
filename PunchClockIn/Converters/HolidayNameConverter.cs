using System;
using System.Globalization;
using System.Windows.Data;
using Punches.Repository;

namespace PunchClockIn.Converters;

public class HolidayNameConverter : IValueConverter, IDisposable
{
    private DataContext dataContext = new DataContext();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var date = (DateTime)value;
        var holidayRepository = new HolidayRepository(dataContext);
        return holidayRepository.GetHoliday(date)?.Name ?? "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        dataContext?.Dispose();
    }
}