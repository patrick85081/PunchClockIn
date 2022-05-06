using Punches.Models;

namespace Punches.Services;

public interface IDailySheetService
{
    Task WriteDaily(DateTime date, string department, string name, double hour, string dailyType, string message, string note);
    DailyType[] DailyTypes { get; }
    Task<DailyModel[]> GetDaily(DateTime date);
    Task<string> GetSheetUrl(string month);
}