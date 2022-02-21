using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using PunchClockIn.Configs;
using Punches.Models;
using Punches.Repository;
using Punches.Repository.Services;
using Quartz;

namespace PunchClockIn.Scheduler.Jobs;

[DisallowConcurrentExecution]
public class ClockInUpdateJob : IJob
{
    private readonly IPunchSheetService punchSheetService;
    private readonly ILogger logger;
    private readonly IConfig config;
    private readonly IClockInRepository clockInRepository;

    public ClockInUpdateJob(
        IConfig config,
        IClockInRepository clockInRepository,
        IPunchSheetService punchSheetService,
        ILogger logger)
    {
        this.punchSheetService = punchSheetService;
        this.logger = logger;
        this.config = config;
        this.clockInRepository = clockInRepository;
    }
    public async Task Execute(IJobExecutionContext context)
    {
        logger.Info($"{nameof(ClockInUpdateJob)} Start");
        QueryParameter parameter = new QueryParameter() { Month = DateTime.Today.ToClockInMonth() };
        var clockIns = await punchSheetService.QueryMonthAsync(parameter.Month, CancellationToken.None);
        clockInRepository.Set(clockIns);
        logger.Info($"{nameof(ClockInUpdateJob)} End");
    }
}