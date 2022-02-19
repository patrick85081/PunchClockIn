using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Quartz;
using Quartz.Spi;

namespace PunchClockIn.Scheduler;

public class QuartzService : IQuartzService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IJobFactory _jobFactory;
    private readonly ILogger _logger;
    private readonly IEnumerable<JobSchedule> _injectJobSchedules;
    private List<JobSchedule> _allJobSchedules; 
    public IScheduler Scheduler { get; set; }

    public CancellationToken CancellationToken { get; private set; }
    public QuartzService(
        ILogger logger,
        ISchedulerFactory schedulerFactory,
        IJobFactory jobFactory, 
        IEnumerable<JobSchedule> jobSchedules)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        // _logger = loggerFactory.CreateLogger<QuartzService>();
        _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
        _jobFactory = jobFactory ?? throw new ArgumentNullException(nameof(jobFactory));
        _injectJobSchedules = jobSchedules ?? throw new ArgumentNullException(nameof(jobSchedules));
    }

    /// <summary>
    /// 啟動排程器
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (Scheduler == null || Scheduler.IsShutdown)
        {
            // 存下 cancellation token 
            CancellationToken = cancellationToken;

            // 先加入在 startup 註冊注入的 Job 工作
            _allJobSchedules = new List<JobSchedule>();
            _allJobSchedules.AddRange(_injectJobSchedules);

            // 再模擬動態加入新 Job 項目 (e.g. 從 DB 來的，針對不同報表能動態決定產出時機)
            // _allJobSchedules.Add(new JobSchedule(jobName: "333", jobType: typeof(ReportJob), cronExpression: "0/13 * * * * ?"));
            // _allJobSchedules.Add(new JobSchedule(jobName: "444", jobType: typeof(ReportJob), cronExpression: "0/20 * * * * ?"));
            // _allJobSchedules.Add(
            //     new JobSchedule(
            //         jobName: "ClockIn",
            //         jobType: typeof(ClockInUpdateJob),
            //         cronExpression: "* 0/30 8-18 ? * MON,TUE,WED,THU,FRI *"));

            // 初始排程器 Scheduler
            Scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            Scheduler.JobFactory = _jobFactory;


            // 逐一將工作項目加入排程器中 
            foreach (var jobSchedule in _allJobSchedules)
            {
                var jobDetail = CreateJobDetail(jobSchedule);
                var trigger = CreateTrigger(jobSchedule);
                await Scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);
                jobSchedule.JobStatus = JobStatus.Scheduled;
            }

            // 啟動排程
            await Scheduler.Start(cancellationToken);
            _logger.Info($"Scheduler StartAsync");
        }
    }

    /// <summary>
    /// 停止排程器
    /// </summary>
    /// <returns></returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (Scheduler != null && !Scheduler.IsShutdown)
        {
            _logger.Info($"Scheduler StopAsync");
            await Scheduler.Shutdown(cancellationToken);
        }
    }

    /// <summary>
    /// 取得所有作業的最新狀態
    /// </summary>
    public async Task<IEnumerable<JobSchedule>> GetJobSchedules()
    {
        if (Scheduler.IsShutdown)
        {
            // 排程器停止時更新各工作狀態為停止
            foreach (var jobSchedule in _allJobSchedules)
            {
                jobSchedule.JobStatus = JobStatus.Stopped;
            }
        }
        else
        {
            // 取得目前正在執行的 Job 來更新各 Job 狀態
            var executingJobs = await Scheduler.GetCurrentlyExecutingJobs();
            foreach (var jobSchedule in _allJobSchedules)
            {
                var isRunning = executingJobs.FirstOrDefault(j => j.JobDetail.Key.Name == jobSchedule.JobName) != null;
                jobSchedule.JobStatus = isRunning ? JobStatus.Running : JobStatus.Scheduled;
            }

        }

        return _allJobSchedules;
    }

    /// <summary>
    /// 手動觸發作業
    /// </summary>
    public async Task TriggerJobAsync(string jobName)
    {
        if (Scheduler != null && !Scheduler.IsShutdown)
        {
            _logger.Info($"job:{jobName} - TriggerJobAsync");
            await Scheduler.TriggerJob(new JobKey(jobName), CancellationToken);
        }
    }

    /// <summary>
    /// 手動中斷作業
    /// </summary>
    public async Task InterruptJobAsync(string jobName)
    {
        if (Scheduler != null && !Scheduler.IsShutdown)
        {
            var targetExecutingJob = await GetExecutingJob(jobName);
            if (targetExecutingJob != null)
            {
                _logger.Info($"job:{jobName} - InterruptJobAsync");
                await Scheduler.Interrupt(new JobKey(jobName));
            }

        }
    }

    /// <summary>
    /// 取得特定執行中的作業
    /// </summary>
    private async Task<IJobExecutionContext> GetExecutingJob(string jobName)
    {
        if (Scheduler != null)
        {
            var executingJobs = await Scheduler.GetCurrentlyExecutingJobs();
            return executingJobs.FirstOrDefault(j => j.JobDetail.Key.Name == jobName);
        }

        return null;
    }

    /// <summary>
    /// 建立作業細節 (後續會透過 JobFactory 依此資訊從 DI 容器取出 Job 實體)
    /// </summary>
    private IJobDetail CreateJobDetail(JobSchedule jobSchedule)
    {
        var jobType = jobSchedule.JobType;
        var jobDetail = JobBuilder
            .Create(jobType)
            .WithIdentity(jobSchedule.JobName)  
            .WithDescription(jobType.Name)
            .Build();

        // 可以在建立 job 時傳入資料給 job 使用
        jobDetail.JobDataMap.Put("Payload", jobSchedule);

        return jobDetail;
    }

    /// <summary>
    /// 產生觸發器
    /// </summary>
    /// <param name="schedule"></param>
    /// <returns></returns>
    private ITrigger CreateTrigger(JobSchedule schedule)
    {
        return TriggerBuilder
            .Create()
            .WithIdentity($"{schedule.JobName}.trigger") 
            .WithCronSchedule(schedule.CronExpression)
            .WithDescription(schedule.CronExpression)
            .Build();
    }
}