
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Punches.Models
{
    public class ClockMonth
    {
        [Key]
        public string Id { get; set; }

        public DateTime Month { get; set; }
        public int SheetId { get; set; }

        public ClockMonth()
        {}

        public ClockMonth(string title, int sheetId)
        {
            Id = title;
            Month = ClockInExtensions.FromClockMonth(title);
            SheetId = sheetId;
        }

        private static DateTime GetMonthDays(string Month)
        {

            var year = int.Parse(Month.Substring(0, 3));
            var month = int.Parse(Month.Substring(3, 2));
            return new DateTime(year, month, 1, new TaiwanCalendar());
        }
    }
}
