using Punches.Models;
using Punches.Repository;
using Punches.Repository.Services;
using Punches.Services.GoogleSheet;

namespace Punches.Services.Fake;

public class FakeDailySheetService : IDailySheetService
{
    private readonly DataContext dataContext;
    private readonly GoogleSheetConfig config;

    public FakeDailySheetService(DataContext dataContext,
        GoogleSheetConfig config)
    {
        this.dataContext = dataContext;
        this.config = config;
    }
    public Task WriteDaily(DateTime date, string department, string name, double hour, string dailyType, string message)
    {
        return Task.CompletedTask;
    }

    public DailyType[] DailyTypes => dataContext.DailyType.Query().ToArray();

    public Task<DailyModel[]> GetDaily(DateTime date) =>
        Task.FromResult(new DailyModel[0]);

    public async Task<string> GetSheetUrl(string month)
    {
        string spreadsheetId = config.DailySpreadsheetId;
        return $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/";
    }
}