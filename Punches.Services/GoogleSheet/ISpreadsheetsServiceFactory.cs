namespace Punches.Services.GoogleSheet;

public interface ISpreadsheetsServiceFactory
{
    Task<ISpreadsheetsApi> GetSpreadsheetsRepository(CancellationToken cancellationToken);
}