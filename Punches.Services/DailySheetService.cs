using System.Globalization;
using Punches.Models;
using Punches.Services;
using Punches.Services.GoogleSheet;

namespace Punches.Repository.Services
{
    public class DailySheetService : IDailySheetService
    {
        private readonly string spreadsheetId;
        private readonly SpreadsheetsServiceFactory serviceFactory;
        private readonly DataContext dataContext;
        private readonly IHolidayRepository holidayRepository;

        public DailySheetService(
            SpreadsheetsServiceFactory serviceFactory, DataContext dataContext, 
            IHolidayRepository holidayRepository, GoogleSheetConfig googleSheetConfig)
        {
            this.serviceFactory = serviceFactory;
            this.dataContext = dataContext;
            this.holidayRepository = holidayRepository;
            this.spreadsheetId = googleSheetConfig.DailySpreadsheetId;

            _ = GetSpreadsheetsRepository()
                .ContinueWith(t => GetDailyTypes(t.Result, DateTime.Today.ToString("yyyy/M")))
                .Unwrap();
        }

        private async Task<ISpreadsheetsApi> GetSpreadsheetsRepository() => await serviceFactory.GetSpreadsheetsRepository(CancellationToken.None);

        public async Task WriteDaily(DateTime date, string department, string name, double hour, string dailyType,
            string message)
        {
            var spreadsheetsRepository = await GetSpreadsheetsRepository();


            var title = date.ToString("yyyy/M");


            var rowIndex = await GetRowIndex(spreadsheetsRepository, date);

            await spreadsheetsRepository.AppendRequest(spreadsheetId, $"{title}!A{rowIndex}:F{rowIndex}",
                date.ToString("yyyy/M/d"), dailyType, department, name, hour, message);
        }

        public async Task<DailyModel[]> GetDaily(DateTime date)
        {
            var spreadsheetsRepository = await GetSpreadsheetsRepository();

            var firstRow = await GetRowIndex(spreadsheetsRepository, date);

            var values = await spreadsheetsRepository.GetSheetValue(
                spreadsheetId, $"{date:yyyy/M}!A{firstRow + 1}:F{(firstRow + 60)}", CancellationToken.None);
            
            if (values == null)
                return new DailyModel[0];

            return values
                .Where(x => x.Count >= 6)
                .Select(x => new DailyModel
                {
                    Date = DateTime.Parse(x[0]?.ToString()),
                    DailyType = x[1]?.ToString(),
                    Department = x[2]?.ToString(),
                    Name = x[3]?.ToString(),
                    Hour = x[4]?.ToString(),
                    Message = x[5]?.ToString(),
                })
                .ToArray();
        }

        public async Task<string> GetSheetUrl(string month)
        {
            var sheetProperty = (await (await GetSpreadsheetsRepository())
                .GetSpreadsheetsInfo(spreadsheetId))
                .SheetProperties
                .FirstOrDefault(x => x.Title == month);

            if (sheetProperty == null)
                return $"https://docs.google.com/spreadsheets/d/{spreadsheetId}";
            else
            return $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/edit#gid={sheetProperty.SheetId}";
        }

        private async Task<int> GetRowIndex(ISpreadsheetsApi spreadsheetsApi, DateTime date)
        {
            var spreadsheetsInfo = await spreadsheetsApi.GetSpreadsheetsInfo(spreadsheetId);
            var sheetProperty = spreadsheetsInfo.SheetProperties
                .FirstOrDefault(x => x.Title == date.ToString("yyyy/M"));

            if (sheetProperty == null) throw new Exception($"Sheet {date:yyyy/M} Not Found");

            string title = date.ToString("yyyy/M");
            var rowCount = sheetProperty.RowCount ?? 1000;

            var values = await spreadsheetsApi.GetSheetValue(spreadsheetId, $"{title}!A1:A{rowCount}", CancellationToken.None);
            var weekStartRowIndex = values
                .Select((x, index) => new { Row = index + 1, Value = x.ElementAtOrDefault(0)?.ToString() })
                .Where(x => x.Value == "日期")
                .ToArray();
            GregorianCalendar gc = new GregorianCalendar();
            var lookup = Enumerable.Range(1, DateTime.DaysInMonth(date.Year, date.Month))
                .Select(x => new DateTime(date.Year, date.Month, x))
                .ToDictionary(x => x, x => gc.GetWeekOfYear(x, CalendarWeekRule.FirstDay, DayOfWeek.Monday));
            var baseNumber = lookup.FirstOrDefault(x => 
                !holidayRepository.IsHoliday(x.Key)
            ).Value;

            if (!lookup.ContainsKey(date.Date)) throw new Exception($"{date} is not Working date.");
            var index = lookup[date] - baseNumber;

            var rowIndex = weekStartRowIndex[index].Row;
            return rowIndex;
        }

        /// <summary>
        /// 取得日報種類
        /// </summary>
        /// <param name="service"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private async Task<DailyType[]> GetDailyTypes(ISpreadsheetsApi service, string title)
        {
            if (dataContext.DailyType.Query().Count() <= 0)
            {
                var ranges = await service.GetSheetValue(spreadsheetId, $"{title}!I2:I80", CancellationToken.None);
                var dailyTypes = ranges.Select(x => x.FirstOrDefault()?.ToString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select((x, index) => new DailyType() { Id = index, Name = x })
                    .ToArray();
                dataContext.DailyType.Upsert(dailyTypes);
            }

            return dataContext.DailyType.Query().ToArray();
        }
        public DailyType[] DailyTypes => dataContext.DailyType.Query().ToArray();
    }
}
