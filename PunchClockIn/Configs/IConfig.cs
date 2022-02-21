using System.ComponentModel;
using Punches.Repository.GoogleSheet;

namespace PunchClockIn.Configs;

public interface IConfig : INotifyPropertyChanged
{
    string Name { get; set; }
    bool EnableNotify { get; set; }
    string Title { get; set; }
    bool DebugMode { get; }
    bool AutoWorkOff { get; set; }

    #region Google Sheet

    string ClientSecretFilePath { get; }
    string PunchSpreadsheetId { get; set; }
    string DailySpreadsheetId { get; }

    #endregion
}

public static class ConfigExtensions
{
    public static GoogleSheetConfig ToSheetConfig(this IConfig config) =>
        new GoogleSheetConfig(config.ClientSecretFilePath)
        {
            DailySpreadsheetId = config.DailySpreadsheetId,
            ClockInSpreadsheetId = config.PunchSpreadsheetId,
        };
}