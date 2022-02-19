using System;
using Autofac;
using Quartz;
using Quartz.Spi;

namespace PunchClockIn.Scheduler;

public class ClockInJobFactory : IJobFactory
{
    private ILifetimeScope lifetimeScope;

    public ClockInJobFactory(ILifetimeScope lifetimeScope)
    {
        this.lifetimeScope = lifetimeScope;
    }
    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        var jobType = bundle.JobDetail.JobType;

        return lifetimeScope.Resolve(jobType) as IJob;
    }

    public void ReturnJob(IJob job)
    {
        var disposable = job as IDisposable;
        disposable?.Dispose();
    }
}