using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace LogonService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly HttpClient client;

    public Worker(ILogger<Worker> logger, IHttpClientFactory clientFactory)
    {
        _logger = logger;
        this.client = clientFactory.CreateClient("WorkOn");
        logger.LogInformation("Worker construct");
    }

    private Subject<EntryWrittenEventArgs> eventArgsSubject = new Subject<EntryWrittenEventArgs>();
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("程式啟動");

        try
        {
            var test = await client.GetStringAsync("/");
            _logger.LogInformation($"Api Test Result : {test}");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, $"API Test Fail : {ex.Message}");
        }
        
        var eventLog = new EventLog("Security") { EnableRaisingEvents = true };
        eventLog.EntryWritten += OnEntryWritten;

        var logonEvent = eventArgsSubject
            .Throttle(TimeSpan.FromSeconds(5))
            .Do(x => _logger.LogInformation("使用者登入，發送打卡命令"))
            .SelectMany(x =>
                Observable.FromAsync(() => client.PutAsync("/api/WorkOn", null))
                    .RetryWhen(x => x.Do(err => _logger.LogError(err, $"API Fail {err.Message}"))
                        .SelectMany(_ => Observable.Never<EntryWrittenEventArgs>()))
            )
            .Do(x => _logger.LogInformation(
                $"Result: {x.StatusCode}, Content: {x.Content.ReadAsStringAsync().GetAwaiter().GetResult()}"))
            .Subscribe(
                onNext: x => { },
                onError: err =>
            {
                _logger.LogError(err, $"Fail {err.Message}");
            });

        using(logonEvent)
        using (eventLog)
        using (eventArgsSubject)
            stoppingToken.WaitHandle.WaitOne();
        
        _logger.LogInformation("程式結束");
    }

    private async void OnEntryWritten(object sender, EntryWrittenEventArgs e)
    {
        if (e.Entry is not { InstanceId: 4624 /*or 4625*/ })
            return;
        
        var logInSource = e.Entry.ReplacementStrings.ElementAtOrDefault(18);
        if (logInSource.Trim() == "-")
            return;
        
        _logger.LogDebug($"On Logon Event {e.Entry.InstanceId} : {string.Join(", ", e.Entry.ReplacementStrings)}");
        _logger.LogDebug(e.Entry.Message);
        _logger.LogInformation($"On Logon Event {e.Entry.InstanceId} : {logInSource}");

        if (logInSource != "127.0.0.1")
        {
            _logger.LogInformation($"Log in Network Source is not in local. 不處理");
            return;
        }
        
        var nowTime = DateTime.Now.TimeOfDay;
        if (nowTime < TimeSpan.Parse("08:00") || TimeSpan.Parse("09:30") < nowTime)
        {
            _logger.LogInformation("不是在 上班打卡時間，不處理");
            return;
        }

        eventArgsSubject.OnNext(e);
    }
}