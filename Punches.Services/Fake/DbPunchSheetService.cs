using Punches.Models;
using Punches.Repository;
using Punches.Repository.Extensions;
using Punches.Repository.Services;
using Punches.Services.GoogleSheet;

namespace Punches.Services.Fake;

public class DbPunchSheetService : IPunchSheetService
{
    private readonly DataContext dataContext;
    private readonly IClockInRepository clockInRepository;
    private readonly IKeyValueRepository keyValueRepository;
    private readonly GoogleSheetConfig config;

    public DbPunchSheetService(
        DataContext dataContext, 
        IClockInRepository clockInRepository, 
        IKeyValueRepository keyValueRepository,
        GoogleSheetConfig config)
    {
        this.dataContext = dataContext;
        this.clockInRepository = clockInRepository;
        this.keyValueRepository = keyValueRepository;
        this.config = config;
    }

    public string GetSheetUrl(ClockMonth month)
    {
        string spreadsheetId = config.ClockInSpreadsheetId;
        return $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/edit#gid={month.SheetId}";
    }
    
    public Task<(string title, ClockMonth[] month)> GetTitleAndMonth(CancellationToken cancellationToken)
    {
        var month = dataContext.Month
            .FindAll()
            .ToArray();
        return Task.FromResult((title: keyValueRepository.GetTitle(), month: month));
    }

    public async Task<string[]> GetNames(CancellationToken cancellationToken)
    {
        var em = await GetEmployee(cancellationToken);
        return em.Select(e => e.Id).ToArray();
    }

    public async Task<ClockIn[]> QueryEmployeeAsync(
        QueryParameter parameter,
        CancellationToken cancellationToken)
    {
        return (await QueryMonthAsync(parameter.Month, cancellationToken))
            .Where(x => x.Name == parameter.Name)
            .ToArray();
    }
    public Task<ClockIn[]> QueryMonthAsync(string month, CancellationToken cancellationToken)
    {
        var clockIns = clockInRepository.GetMonth(month)
            .Where(x => x.RowNumber > 0)
            .ToArray();
        return Task.FromResult(clockIns);
    }

    public Task<Employee[]> GetEmployee(CancellationToken cancellationToken)
    {
        var employees = dataContext.Employee
            .FindAll()
            .OrderBy(e => e.Id)
            .ToArray();

        return Task.FromResult(employees);
    }

    public async Task WriteWorkOffTime(DateTime date, string name, TimeSpan workOff)
    {
        // Not Implement
    }

    public async Task WriteWorkOnTime(DateTime date, string department, string name, TimeSpan workOn)
    {
        // Not Implement
    }
    
    private readonly TimeSpan StartTime = TimeSpan.Parse("08:00");
    private readonly TimeSpan EndTime = TimeSpan.Parse("09:30");
    public bool IsInWorkTime(TimeSpan nowTime)
    {
        return nowTime < StartTime || EndTime < nowTime;
    }
}