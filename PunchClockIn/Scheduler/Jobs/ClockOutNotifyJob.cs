using System;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using NLog;
using PunchClockIn.Configs;
using PunchClockIn.ViewModels;
using Punches.Repository;
using Punches.Repository.Services;
using Quartz;
using ReactiveUI;

namespace PunchClockIn.Scheduler.Jobs;

[DisallowConcurrentExecution]
public class ClockOutNotifyJob : IJob
{
    private readonly IConfig config;
    private readonly IClockInRepository clockInRepository;
    private readonly IPunchSheetService punchSheetService;
    private readonly ILogger logger;
    private readonly FancyBalloonViewModel fancyBalloon;

    public ClockOutNotifyJob(
        IConfig config,
        IClockInRepository clockInRepository,
        IPunchSheetService punchSheetService,
        ILogger logger,
        FancyBalloonViewModel fancyBalloon)
    {
        this.config = config;
        this.clockInRepository = clockInRepository;
        this.punchSheetService = punchSheetService;
        this.logger = logger;
        this.fancyBalloon = fancyBalloon;
    }
    public async Task Execute(IJobExecutionContext context)
    {
        if (!config.EnableNotify || string.IsNullOrWhiteSpace(config.Name)) return;

        var clockIn = clockInRepository.GetByDate(DateTime.Today, config.Name);
        if (clockIn == null || !clockIn.WorkOn.HasValue || clockIn.WorkOff.HasValue) return;

        var workOff = clockIn.WorkOn.Value.Add(TimeSpan.FromHours(9));
        var shouldWorkOffTime = DateTime.Today.Add(workOff);

        CheckNotify(shouldWorkOffTime, -30);
        CheckNotify(shouldWorkOffTime, -5);
    }

    private void CheckNotify(DateTime shouldWorkOffTime, int minute)
    {
        var now = DateTime.Now;
        var notifyTime = shouldWorkOffTime.AddMinutes(minute);
        if (notifyTime.Date == now.Date && notifyTime.Hour == now.Hour && notifyTime.Minute == now.Minute)
        {
            logger.Info($"{config.Name} 下班時間 {shouldWorkOffTime:HH:mm}，提前{Math.Abs(minute)}分鐘通知");
            RxApp.MainThreadScheduler
                .Schedule(() =>
                {
                    fancyBalloon.Title = $"前 {Math.Abs(minute)} 分鐘通知";
                    fancyBalloon.TargetTime = shouldWorkOffTime;
                    fancyBalloon.Show = false;
                    fancyBalloon.Show = true;
                });
        }
    }
}