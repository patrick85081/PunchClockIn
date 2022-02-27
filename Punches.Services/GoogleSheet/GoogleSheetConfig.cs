namespace Punches.Services.GoogleSheet;

public class GoogleSheetConfig
{
    private string clientSecretFilePath;

    public GoogleSheetConfig()
    {
    }
    
    public GoogleSheetConfig(string clientSecretFilePath)
    {
        this.clientSecretFilePath = clientSecretFilePath;
    }

    public Stream ClientSecretStream => File.OpenRead(clientSecretFilePath);

    public string ClockInSpreadsheetId { get; init; } 

    public string DailySpreadsheetId { get; init; } 
}