using System;
using System.Linq;
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
public class AutoClockOutJob : IJob
{
    private readonly IConfig config;
    private readonly IClockInRepository clockInRepository;
    private readonly IClockInSheetService clockInSheetService;
    private readonly ILogger logger;

    public AutoClockOutJob(
        IConfig config,
        IClockInRepository clockInRepository,
        IClockInSheetService clockInSheetService,
        ILogger logger)
    {
        this.config = config;
        this.clockInRepository = clockInRepository;
        this.clockInSheetService = clockInSheetService;
        this.logger = logger;
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        if (string.IsNullOrEmpty(config.Name) || !config.AutoWorkOff) return;
        var now = DateTime.Now;
        var today = now.Date;
        var clockIn = clockInRepository.Get(today.ToClockInMonth(), config.Name)
            .FirstOrDefault(c => c.Date.Date == today);
        
        if (clockIn == null || !clockIn.WorkOn.HasValue || clockIn.WorkOff.HasValue) return;


        var shouldWorkOffTime = clockIn.WorkOn.Value.Add(TimeSpan.FromHours(9));
        if (now.TimeOfDay > shouldWorkOffTime) logger.Info($"Current Time: {now:hh\\:mm}");
        if (now - (today.Add(shouldWorkOffTime)) <= TimeSpan.FromMinutes(5)) return;
        
        var workOffTime = shouldWorkOffTime.Add(TimeSpan.FromMinutes(new Random().Next(3, 10)));
        logger.Info($"{config.Name} {clockIn.WorkOn} 目標打卡時間 {shouldWorkOffTime}，開始打下班卡 {workOffTime}");
        await clockInSheetService.WriteWorkOffTime(today, config.Name, workOffTime);
        logger.Info($"{config.Name} 打卡成功");

        QueryParameter parameter = new QueryParameter() { Month = today.ToClockInMonth(), Name = config.Name };
        var rowData = await clockInSheetService.QueryMonthAsync(parameter.Month, CancellationToken.None);
        var checkData = rowData.FirstOrDefault(x => x.Name == config.Name && x.Date == today);
        if (checkData == null)
        {
            logger.Error($"{config.Name} 下班打卡失敗");
            return;
        }

        logger.Info($"確認 {config.Name} 實際下班打卡時間 {checkData.WorkOff}");
        clockInRepository.Set(rowData);
    }
}