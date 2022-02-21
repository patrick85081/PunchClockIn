using Microsoft.Extensions.Logging;
using Punches.Models;
using Punches.Repository.Extensions;
using Punches.Repository.GoogleSheet;

namespace Punches.Repository.Services;

public class PunchSheetService : IPunchSheetService
{
    private readonly string spreadsheetId;
    private readonly SpreadsheetsServiceFactory factory;
    private readonly ILogger logger;
    const int startRowIndex = 2;

    private Task<ISpreadsheetsApi> GetSpreadsheetsRepository(CancellationToken cancellationToken) =>
        factory.GetSpreadsheetsRepository(cancellationToken);

    public PunchSheetService(SpreadsheetsServiceFactory factory, ILogger<PunchSheetService> logger, GoogleSheetConfig googleSheetConfig)
    {
        this.factory = factory;
        this.logger = logger;
        this.spreadsheetId = googleSheetConfig.ClockInSpreadsheetId;
    }

    public string GetSheetUrl(ClockMonth month)
    {
        return $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/edit#gid={month.SheetId}";
    }

    public async Task<(string title, ClockMonth[] month)> GetTitleAndMonth(CancellationToken cancellationToken)
    {
        var spreadsheetsRepository = await GetSpreadsheetsRepository(cancellationToken);
        var spreadsheetsInfo = await spreadsheetsRepository.GetSpreadsheetsInfo(spreadsheetId);

        var title = spreadsheetsInfo.Title;
        var months = spreadsheetsInfo.SheetProperties
            .Where(sheetProperty => int.TryParse(sheetProperty.Title, out var _))
            .Select(sheetProperty => new ClockMonth(sheetProperty.Title, sheetProperty.SheetId))
            .Reverse()
            .ToArray();

        return (title, months);
    }

    public async Task<string[]> GetNames(CancellationToken cancellationToken)
    {
        return (await GetEmployee(cancellationToken))
            .Select(e => e.Id)
            .ToArray();
    }

    /// <summary>
    /// 讀取通訊錄
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Employee[]> GetEmployee(CancellationToken cancellationToken)
    {
        var spreadsheetsRepository = await GetSpreadsheetsRepository(cancellationToken);
        var sheetValue = await spreadsheetsRepository.GetSheetValue(spreadsheetId, "通訊錄!A3:F40", cancellationToken);

        var rawData = sheetValue
            .Where(r => r.Count >= 1)
            .Select((row, index) =>
                new Employee()
                {
                    ChineseName = row.ElementAtOrDefault(0)?.ToString(),
                    EnglishName = row.ElementAtOrDefault(1)?.ToString(),
                    Ext = row.ElementAtOrDefault(2)?.ToString(),
                    Phone = row.ElementAtOrDefault(3)?.ToString(),
                    Email = row.ElementAtOrDefault(4)?.ToString(),
                    SkypeId = row.ElementAtOrDefault(5)?.ToString(),
                    Index = index + 1,

                })
            .ToArray();

        // 讀取部門
        IEnumerable<Employee> GetDepartment(Employee[] employees)
        {
            var enumerator = employees.GetEnumerator();
            string department = null;
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current as Employee;

                if (current.ChineseName == null || current.ChineseName.Length > 6)
                    continue;
                else if (current.ChineseName.EndsWith("部"))
                    department = current.ChineseName;
                else
                {
                    current.Department = department;
                    yield return current;
                }
            }
        }

        var employees = GetDepartment(rawData).ToArray();
        return employees;
    }

    public async Task<ClockIn[]> QueryEmployeeAsync(
        QueryParameter parameter,
        CancellationToken cancellationToken)
    {
        return (await QueryMonthAsync(parameter.Month, cancellationToken))
            .Where(x => x.Name == parameter.Name)
            .ToArray();
    }
    public async Task<ClockIn[]> QueryMonthAsync(string month, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(month)) return Array.Empty<ClockIn>();
        var range = $"{month}!A{startRowIndex}:I440";

        var spreadsheetsRepository = await GetSpreadsheetsRepository(cancellationToken);
        var sheetValue = await spreadsheetsRepository.GetSheetValue(spreadsheetId, range, cancellationToken);

        var clockIns = (
                from x in sheetValue.WithIndex()
                let row = x.Item
                let index = x.Index
                // where r.Any() && r.ElementAtOrDefault(2)?.ToString() == parameter.Name
                where row.Any() && !string.IsNullOrEmpty(row.ElementAtOrDefault(2)?.ToString())
                let date = DateTime.TryParse(row.ElementAtOrDefault(0)?.ToString(), out var date) ? (DateTime?)date : null
                where date.HasValue
                let name = row.ElementAtOrDefault(2)?.ToString()
                select new ClockIn
                {
                    Id = ClockIn.GetId(date.Value, name),
                    Date = date.Value,
                    Department = row[1]?.ToString(),
                    Name = name,
                    WorkOn = TimeSpan.TryParse(DateAndTimeStringFix(row.ElementAtOrDefault(3)), out var workOn) ? workOn : null,
                    WorkOff = TimeSpan.TryParse(DateAndTimeStringFix(row.ElementAtOrDefault(4)), out var workOff) ? workOff : null,
                    Location = row.ElementAtOrDefault(6)?.ToString(),
                    Status = row.ElementAtOrDefault(7)?.ToString(),
                    Reason = row.ElementAtOrDefault(8)?.ToString(),
                    RowNumber = startRowIndex + index,
                }
            )
            .ToArray();
        return clockIns;

        string DateAndTimeStringFix(object obj)
        {
            return (obj?.ToString() ?? "")
                .Replace("：", ":")
                .Trim();
        }
    }

    /// <summary>
    /// 下班打卡
    /// </summary>
    /// <param name="date"></param>
    /// <param name="name"></param>
    /// <param name="workOff"></param>
    /// <exception cref="Exception"></exception>
    public async Task WriteWorkOffTime(DateTime date, string name, TimeSpan workOff)
    {
        var spreadsheetsRepository = await GetSpreadsheetsRepository(CancellationToken.None);

        logger.LogInformation($"{date:yyyy/MM/dd} {name} 下班打卡 {workOff:hh\\:mm}");
        var month = date.ToClockInMonth();
        QueryParameter parameter = new QueryParameter() { Name = name, Month = month };
        var queryData = await QueryMonthAsync(parameter.Month, CancellationToken.None);
        var clockIn = queryData.FirstOrDefault(x => x.Name == name && x.Date.Date == date.Date);
        if (clockIn == null)
            throw new Exception($"{date:yyyy/MM/dd} {name} Clock In is not found");
        
        var clockInRowNumber = clockIn.RowNumber;

        await spreadsheetsRepository.WriteRequest(spreadsheetId, $"{month}!E{clockInRowNumber}:E{clockInRowNumber}",
            workOff.ToString(@"hh\:mm"));
    }

    /// <summary>
    /// 上班打卡
    /// </summary>
    /// <param name="date"></param>
    /// <param name="department"></param>
    /// <param name="name"></param>
    /// <param name="workOn"></param>
    public async Task WriteWorkOnTime(DateTime date, string department, string name, TimeSpan workOn)
    {
        var spreadsheetsRepository = await GetSpreadsheetsRepository(CancellationToken.None);

        logger.LogInformation($"{date:yyyy/MM/dd} {name} 上班打卡 {workOn:hh\\:mm}");
        var month = date.ToClockInMonth();
        QueryParameter parameter = new QueryParameter() { Name = name, Month = month };
        var queryData = (await QueryMonthAsync(parameter.Month, CancellationToken.None))
            .OrderBy(x => x.RowNumber)
            .ToArray();
        var clockIn = queryData.FirstOrDefault(x => x.Name == name && x.Date.Date == date.Date);

        var clockInRowNumber = clockIn switch
        {
            // { } => clockIn.RowNumber,
            {} => throw new Exception($"{date:yyyy/MM/dd} {name} already Work On"),
            _ => queryData.Select(x => x.RowNumber).LastOrDefault(startRowIndex) + 1,
        };
        
        if (clockIn == null)
            await spreadsheetsRepository.AppendRequest(spreadsheetId, $"{month}!A{clockInRowNumber}:D{clockInRowNumber}",
                date.ToString("yyyy/M/d"), department, name, workOn.ToString(@"hh\:mm"));
        else
            await spreadsheetsRepository.WriteRequest(spreadsheetId, $"{month}!A{clockInRowNumber}:D{clockInRowNumber}",
                date.ToString("yyyy/M/d"), department, name, workOn.ToString(@"hh\:mm"));
        await spreadsheetsRepository.WriteRequest(spreadsheetId, $"{month}!G{clockInRowNumber}:G{clockInRowNumber}", "公司");
    }

    private readonly TimeSpan StartTime = TimeSpan.Parse("08:00");
    private readonly TimeSpan EndTime = TimeSpan.Parse("09:30");
    public bool IsInWorkTime(TimeSpan nowTime)
    {
        return nowTime < StartTime || EndTime < nowTime;
    }
}