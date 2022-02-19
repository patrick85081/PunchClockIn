using System.ComponentModel;

namespace PunchClockIn.Scheduler;

public enum JobStatus : byte
{
    [Description("初始化")]
    Init = 0,
    [Description("已排程")]
    Scheduled = 1,
    [Description("執行中")]
    Running = 2,
    [Description("已停止")]
    Stopped = 3, 
}