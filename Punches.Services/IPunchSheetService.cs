using Punches.Models;

namespace Punches.Services;

public interface IPunchSheetService
{
    Task<(string title, ClockMonth[] month)> GetTitleAndMonth(CancellationToken cancellationToken);
    Task<string[]> GetNames(CancellationToken cancellationToken);
    Task<ClockIn[]> QueryMonthAsync(string month, CancellationToken cancellationToken);
    Task<Employee[]> GetEmployee(CancellationToken cancellationToken);
    /// <summary>
    /// 打下班卡
    /// </summary>
    /// <param name="date"></param>
    /// <param name="name"></param>
    /// <param name="workOff"></param>
    /// <returns></returns>
    Task WriteWorkOffTime(DateTime date, string name, TimeSpan workOff);

    /// <summary>
    /// 打上班卡
    /// </summary>
    /// <param name="date"></param>
    /// <param name="department"></param>
    /// <param name="name"></param>
    /// <param name="workOn"></param>
    /// <param name="s"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    Task WriteWorkOnTime(DateTime date, string department, string name, TimeSpan workOn, 
        string location, string remark);
    string GetSheetUrl(ClockMonth month);

    Task<ClockIn[]> QueryEmployeeAsync(
        QueryParameter parameter,
        CancellationToken cancellationToken);

    bool IsInWorkTime(TimeSpan nowTime);
}