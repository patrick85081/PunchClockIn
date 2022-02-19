using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace Punches.Repository.Extensions;

public static class SheetsServiceEx
{

    public static async Task AppendRequest(this SheetsService service, string spreadsheetId, string sheetRange, params object[] data)
    {
        var dataRange = BuildValueRange(data);

        var request = service.Spreadsheets.Values
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
    public static async Task WriteRequest(this SheetsService service, string spreadsheetId, string sheetRange, params object[] data)
    {
        var dataRange = BuildValueRange(data);

        var request = service.Spreadsheets.Values
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