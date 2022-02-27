using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.NLog;
using Dapplo.Microsoft.Extensions.Hosting.Wpf;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PunchClockIn.Configs;
using PunchClockIn.Scheduler;
using PunchClockIn.Scheduler.Jobs;
using PunchClockIn.Services;
using PunchClockIn.ViewModels;
using Punches.Repository;
using Punches.Repository.Services;
using Punches.Services;
using Punches.Services.Fake;
using Punches.Services.GoogleSheet;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace PunchClockIn;

public static class Program
{

    [STAThread]
    public static void Main(string[] args)
    {
        var appBuilder = WebApplication.CreateBuilder(args);
        appBuilder.Host
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureContainer<ContainerBuilder>(ConfigureAutofac)
            .ConfigureServices((context, service) =>
            {
                service.UseMicrosoftDependencyResolver();

                var resolver = Locator.CurrentMutable;
                resolver.InitializeSplat();
                resolver.InitializeReactiveUI();
            })
            .ConfigureWpf(con =>
                con.UseApplication<App>()
                    .UseWindow<MainWindow>())
            .UseWpfLifetime()
            .UseConsoleLifetime();
        appBuilder.WebHost
            .ConfigureKestrel(option =>
            {
                var socketDirPath = Path.Combine(Path.GetTempPath(), "PunchClockIn");
                var socketPath = Path.Combine(socketDirPath, "Socket.sock");
                Directory.CreateDirectory(socketDirPath);
                File.Delete(socketPath);
                option.ListenUnixSocket(socketPath);
            });
        
        var app = appBuilder.Build();
        app.MapPut("/api/WorkOn", WriteWorkOn);
        app.MapGet("/", () => Results.Ok("Hello World"));

        app.Services.UseMicrosoftDependencyResolver();

        app.Run();
    }

    private static async Task<IResult> WriteWorkOn(
        Microsoft.Extensions.Logging.ILogger<App> logger,
        IPunchSheetService punchSheetService, 
        IEmployeeRepository employeeRepository, 
        IConfig config)
    {
        logger.LogInformation("Get Work On API Request");

        var employee = employeeRepository.GetAll()
            .FirstOrDefault(e => e.Id == config.Name);
        if (employee == null)
        {
            logger.LogInformation($"API Work On Request : Employee Name is not found.");
            return Results.NotFound("Employee Name is not found.");
        }

        var nowTime = DateTime.Now.TimeOfDay;
        if (punchSheetService.IsInWorkTime(nowTime))
        {
            logger.LogInformation($"API Work On Request : Current is not work on time.");
            return Results.BadRequest("Current is not work on time.");
        }

        try
        {
            await punchSheetService.WriteWorkOnTime(
                DateTime.Today,
                employee.Department, config.Name,
                nowTime);
            logger.LogInformation($"API Work On Request : Success");
            return Results.Ok("Work On Success");
        }
        catch (Exception e)
        {
            logger.LogError(e, $"API Work On Request : {e?.Message}");
            return Results.BadRequest(e?.Message);
        }
    }

    private static void ConfigureAutofac(HostBuilderContext context, ContainerBuilder builder)
    {
        IConfig config = new Config();
        builder.RegisterInstance(config)
            .As<IConfig>();

        #region View Model

        builder.RegisterType<MainViewModel>()
            .AsSelf()
            .SingleInstance();
        builder.RegisterType<NotifyIconViewModel>()
            .AsSelf()
            .SingleInstance();
        builder.RegisterType<SettingsViewModel>()
            .AsSelf()
            .SingleInstance();
        builder.RegisterType<FancyBalloonViewModel>()
            .AsSelf()
            .SingleInstance();
        builder.RegisterType<PunchQueryViewModel>()
            .AsSelf()
            .SingleInstance();
        builder.RegisterType<DailyQueryViewModel>()
            .AsSelf()
            .SingleInstance();

        #endregion

        #region Repository

        var designMode = (bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(
            typeof(DependencyObject)).DefaultValue);
        // var designMode = Application.Current is not App;
        builder.Register(x => 
                x.Resolve<IConfig>().ToSheetConfig())
            .AsSelf()
            .SingleInstance();
        if (config.DebugMode || designMode)
        {
            builder.RegisterType<DbPunchSheetService>()
                .As<IPunchSheetService>()
                .SingleInstance();
            builder.RegisterType<FakeDailySheetService>()
                .As<IDailySheetService>();
        }
        else
        {
            builder.RegisterType<PunchSheetService>()
                .As<IPunchSheetService>()
                .SingleInstance();
            builder.RegisterType<DailySheetService>()
                .As<IDailySheetService>();
            builder.Register(x => new SpreadsheetsServiceFactory(x.Resolve<GoogleSheetConfig>()))
                .As<ISpreadsheetsServiceFactory>()
                .SingleInstance();
        }

        builder.Register<DataContext>(x => new DataContext())
            .AsSelf();
        builder.RegisterType<ClockInRepository>()
            .As<IClockInRepository>();
        builder.RegisterType<EmployeeRepository>()
            .As<IEmployeeRepository>();
        builder.RegisterType<ClockMonthRepository>()
            .As<IClockMonthRepository>();
        builder.RegisterType<HolidayRepository>()
            .As<IHolidayRepository>();
        builder.RegisterType<KeyValueRepository>()
            .As<IKeyValueRepository>();

        #endregion

        #region Factory

        builder.RegisterType<ClockInJobFactory>()
            .As<IJobFactory>()
            .SingleInstance();
        builder.RegisterType<StdSchedulerFactory>()
            .As<ISchedulerFactory>()
            .SingleInstance();

        #endregion

        #region Service

        builder.RegisterType<QuartzService>()
            .As<IQuartzService>()
            .SingleInstance();
        builder.RegisterType<DialogService>()
            .As<IDialogService>()
            .SingleInstance();

        #endregion

        #region Job

        builder.RegisterType<ClockInUpdateJob>()
            .AsSelf()
            .SingleInstance();
        builder.RegisterType<ClockOutNotifyJob>()
            .AsSelf()
            .SingleInstance();
        builder.RegisterType<AutoClockOutJob>()
            .AsSelf()
            .SingleInstance();
        builder.RegisterInstance(
            new JobSchedule(
                jobName: "ClockInUpdate",
                jobType: typeof(ClockInUpdateJob),
                cronExpression: "0 0/30 8-20 ? * MON,TUE,WED,THU,FRI *")
        );
        builder.RegisterInstance(
            new JobSchedule(
                jobName: "ClockOutNotify",
                jobType: typeof(ClockOutNotifyJob),
                cronExpression: "0 0/1 * * * ?")
        );
        builder.RegisterInstance(
            new JobSchedule(
                jobName: "AutoClockOut",
                jobType: typeof(AutoClockOutJob),
                cronExpression: "0 0/1 17-20 * * ?")
        );

        #endregion

        builder.RegisterModule<NLogModule>();

        /*
        #region Init

        var resolver = builder.UseAutofacDependencyResolver();
        builder.RegisterInstance(resolver);

        // resolver.InitializeSplat();
        resolver.InitializeReactiveUI();

        var container = builder.Build();
        resolver.SetLifetimeScope(container);

        #endregion
    */
    }

}