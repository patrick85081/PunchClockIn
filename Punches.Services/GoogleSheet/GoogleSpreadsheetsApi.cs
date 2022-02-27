using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace Punches.Services.GoogleSheet;

public class GoogleSpreadsheetsApi : ISpreadsheetsApi
{
    private SheetsService sheetsService;

    public GoogleSpreadsheetsApi(SheetsService sheetsService)
    {
        this.sheetsService = sheetsService;
    }

    public async Task<SpreadsheetsInfo> GetSpreadsheetsInfo(
        string spreadsheetId, CancellationToken cancellationToken = default)
    {
        var sheetInfo = await sheetsService.Spreadsheets
            .Get(spreadsheetId)
            .ExecuteAsync(cancellationToken);

        var title = sheetInfo
            .Properties
            .Title
            .Trim();
        var sheetProperties = sheetInfo
            .Sheets
            .Where(x => x.Properties.SheetId.HasValue)
            .Select(x => new SheetProperty()
            {
                Title = x.Properties.Title, 
                SheetId = x.Properties.SheetId.Value,
                RowCount = x.Properties.GridProperties.RowCount,
            })
            .ToArray();

        return new SpreadsheetsInfo
        {
            Title = title,
            SheetProperties = sheetProperties,
        };
    }

    public async Task<IList<IList<object>>> GetSheetValue(string spreadsheetId, string range, CancellationToken cancellationToken = default)
    {
        var sheetPages = await sheetsService.Spreadsheets
            .Values
            .Get(spreadsheetId, range)
            .ExecuteAsync(cancellationToken);

        var rawData = sheetPages
            .Values;

        return rawData;
    }
    
    public async Task AppendRequest(string spreadsheetId, string sheetRange, params object[] data)
    {
        var dataRange = BuildValueRange(data);

        var request = sheetsService.Spreadsheets.Values
            .Append(dataRange, spreadsheetId, sheetRange);
        request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;

        var response = await request.ExecuteAsync();

        static ValueRange BuildValueRange(params object[] data)
        {
            var dataRange = new ValueRange()
            {
                // MajorDimension = "COLUMNS",
                Values = new List<IList<object>>()
                {
                    data.ToList()
                },
            };
            return dataRange;
        }
    }
    public async Task WriteRequest(string spreadsheetId, string sheetRange, params object[] data)
    {
        var dataRange = BuildValueRange(data);

        var request = sheetsService.Spreadsheets.Values
            .Update(dataRange, spreadsheetId, sheetRange);
        request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

        var response = await request.ExecuteAsync();

        static ValueRange BuildValueRange(params object[] data)
        {
            var dataRange = new ValueRange()
            {
                // MajorDimension = "COLUMNS",
                Values = new List<IList<object>>()
                {
                    data.ToList()
                },
            };
            return dataRange;
        }
    }
}