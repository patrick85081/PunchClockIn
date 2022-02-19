using Punches.Models;
using Punches.Repository.Extensions;
using Punches.Repository.Services;
using Punches.Repository.GoogleSheet;

namespace Punches.Repository.Fake;

public class DbClockInSheetService : IClockInSheetService
{
    private readonly DataContext dataContext;
    private readonly IClockInRepository clockInRepository;
    private readonly IKeyValueRepository keyValueRepository;
    private readonly GoogleSheetConfig config;

    public DbClockInSheetService(
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
        
    }

    public async Task WriteWorkOnTime(DateTime date, string department, string name, TimeSpan workOn)
    {
        
    }
}