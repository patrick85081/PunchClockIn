using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using static System.Environment;

namespace Punches.Services.GoogleSheet;

public class SpreadsheetsServiceFactory
{
    private readonly GoogleSheetConfig config;

    private readonly string? certFilePath = null;

    public SpreadsheetsServiceFactory(GoogleSheetConfig config)
    {
        this.config = config;
    }

    private SheetsService service = null;
    private readonly SemaphoreSlim _lock = new(1, 1);
    public bool IsCreate => service != null;

    public async Task<ISpreadsheetsApi> GetSpreadsheetsRepository(CancellationToken cancellationToken) =>
        new GoogleSpreadsheetsApi(await GetService(cancellationToken));

    private async Task<SheetsService> GetService(CancellationToken cancellationToken)
    {
        if (service == null)
        {
            try
            {
                await _lock.WaitAsync();
                if (service == null)
                {
                    var scopes = new string[] { SheetsService.Scope.Spreadsheets };

                    var credPath = GetFolderPath(SpecialFolder.Personal);
                            
                    service = await CreateService(config.ClientSecretStream, cancellationToken, scopes, credPath);
                }
            }
            finally
            {
                _lock.Release();
            }
        }
        return service;
    }
    private static async Task<SheetsService> CreateService(
        Stream stream, CancellationToken cancellationToken, string[] scopes, string credPath)
    {
        var googleClientSecrets = await GoogleClientSecrets.FromStreamAsync(stream, cancellationToken);
        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            googleClientSecrets.Secrets,
            scopes,
            "user",
            cancellationToken,
            new FileDataStore(credPath, true));
        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "Demo Sheet",
        });

        return service;
    }
}