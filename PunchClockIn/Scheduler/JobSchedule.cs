using System;

namespace PunchClockIn.Scheduler;

public class JobSchedule
{
    public JobSchedule(string jobName, Type jobType, string cronExpression)
    {
        JobName = jobName;
        JobType = jobType;
        CronExpression = cronExpression;
    }

    public string JobName { get; private set; }
    public Type JobType { get; private set; }
    public string CronExpression { get; private set; }
    public JobStatus JobStatus { get; internal set; } = JobStatus.Init;
}