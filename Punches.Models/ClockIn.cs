using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Punches.Models;

public class ClockIn
{
    [Key]
    public string Id { get; set; }

    public DateTime Date { get; set; }
    public string Department { get; set; }
    public string Name { get; set; }
    public TimeSpan? WorkOn { get; set; }
    public TimeSpan? WorkOff { get; set; }
    public string Status { get; set; }
    public string Reason { get; set; }
    public string Location { get; set; }
    public int RowNumber { get; set; }

    public static string GetId(DateTime Date, string Name)
    {
        var tw = new TaiwanCalendar();
        return $"{tw.GetYear(Date)}{tw.GetMonth(Date):00}{tw.GetDayOfMonth(Date):00}{Name}";
    }
}