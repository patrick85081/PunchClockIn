using System;
using System.Globalization;
using System.Windows.Data;
using Punches.Models;
using Punches.Repository;

namespace PunchClockIn.Converters
{
    public class DayOfWeekConverter : IValueConverter, IDisposable
    {
        private DataContext db = new DataContext();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var date = (DateTime)value;
            
            var week = date.DayOfWeek.ToWeekString();
            var day = new HolidayRepository(db).GetHoliday(date);
            return $"{week}  {day?.Name}";
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
}
