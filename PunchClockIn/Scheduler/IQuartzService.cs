using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quartz;

namespace PunchClockIn.Scheduler;

public interface IQuartzService
{
    IScheduler Scheduler { get; set; }
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// 啟動排程器
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 停止排程器
    /// </summary>
    /// <returns></returns>
    Task StopAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 取得所有作業的最新狀態
    /// </summary>
    Task<IEnumerable<JobSchedule>> GetJobSchedules();

    /// <summary>
    /// 手動觸發作業
    /// </summary>
    Task TriggerJobAsync(string jobName);

    /// <summary>
    /// 手動中斷作業
    /// </summary>
    Task InterruptJobAsync(string jobName);
}