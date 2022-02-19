using Punches.Models;

namespace Punches.Repository.Services;

public interface IClockInSheetService
{
    Task<(string title, ClockMonth[] month)> GetTitleAndMonth(CancellationToken cancellationToken);
    Task<string[]> GetNames(CancellationToken cancellationToken);
    Task<ClockIn[]> QueryMonthAsync(string month, CancellationToken cancellationToken);
    Task<Employee[]> GetEmployee(CancellationToken cancellationToken);
    Task WriteWorkOffTime(DateTime date, string name, TimeSpan workOff);
    Task WriteWorkOnTime(DateTime date, string department, string name, TimeSpan workOn);
    string GetSheetUrl(ClockMonth month);

    Task<ClockIn[]> QueryEmployeeAsync(
        QueryParameter parameter,
        CancellationToken cancellationToken);
}