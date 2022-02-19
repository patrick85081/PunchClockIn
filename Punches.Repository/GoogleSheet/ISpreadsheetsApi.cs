namespace Punches.Repository.GoogleSheet;

public interface ISpreadsheetsApi
{
    Task<SpreadsheetsInfo> GetSpreadsheetsInfo(
        string spreadsheetId, CancellationToken cancellationToken = default);

    Task<IList<IList<object>>> GetSheetValue(string spreadsheetId, string range, CancellationToken cancellationToken);
    Task AppendRequest(string spreadsheetId, string sheetRange, params object[] data);
    Task WriteRequest(string spreadsheetId, string sheetRange, params object[] data);
}