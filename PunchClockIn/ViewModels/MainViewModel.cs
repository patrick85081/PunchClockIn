using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using PunchClockIn.Configs;
using PunchClockIn.Scheduler;
using PunchClockIn.Services;
using Punches.Repository;
using Punches.Repository.Extensions;
using Punches.Repository.Services;
using ReactiveUI;

namespace PunchClockIn.ViewModels;

public class MainViewModel : ReactiveObject
{
    private readonly IClockInSheetService clockInSheetService;
    private readonly IEmployeeRepository employeeRepository;
    private readonly IQuartzService quartzService;
    private readonly IDialogService dialogService;
    private readonly IDailySheetService dailySheetService;
    private readonly IKeyValueRepository keyValueRepository;
    private readonly IConfig config;
    private readonly PunchQueryViewModel punchQuery;

    #region Property

    public string Version => Assembly.GetEntryAssembly().GetName().Version.ToString();
    public string Title => titleProperty.Value;

    private readonly ObservableAsPropertyHelper<string> titleProperty;

    #endregion

    #region Command

    public ReactiveCommand<Unit, Unit> InitCommand { get; }

    public ReactiveCommand<Unit, Unit> OpenPunchUrlCommand => ReactiveCommand.Create(() =>
    {
        var url = clockInSheetService.GetSheetUrl(punchQuery.SelectSheets);
        var info = new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true };
        Process.Start(info);
    });
    public ReactiveCommand<Unit, Unit> OpenDailyUrlCommand => ReactiveCommand.Create(() =>
    {
        dailySheetService.GetSheetUrl(DateTime.Today.ToString("yyyy/M"))
            .ContinueWith(t =>
            {
                var url = t.Result;
                var info = new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true };
                Process.Start(info);
            });
        // var url = await dailySheetService.GetSheetUrl(DateTime.Today.ToString("yyyy/M"));
        // var info = new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true };
        // Process.Start(info);
    });

    public ReactiveCommand<Unit, Unit> WorkInCommand => ReactiveCommand.CreateFromTask(async () =>
    {
        try
        {
            if (string.IsNullOrWhiteSpace(config.Name)) throw new Exception($"Please Set Employee.");
            var defaultWorkTime = DateTime.Today.AddHours(8).AddMinutes(30);
            var (dialogResult, dateTime) =
                await dialogService.ShowWorkInOutDialog($"{config.Name} 上班打卡", defaultWorkTime);
            if (!dialogResult) return;

            var employee = employeeRepository.GetAll().FirstOrDefault(e => e.Id == config.Name);
            if (employee == null)
            {
                await dialogService.ShowMessageBox("Message", $"{config.Name} Not Found");
                return;
            }

            await clockInSheetService.WriteWorkOnTime(dateTime.Date, employee.Department, employee.Id, dateTime.TimeOfDay);
            await punchQuery.QueryCommand.Execute(punchQuery.QueryParameter);
            await dialogService.ShowMessageBox("Message", "Success");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await dialogService.ShowMessageBox("Message", $"Fail {e.Message}");
        }
    });

    public ReactiveCommand<Unit, Unit> WorkOutCommand => ReactiveCommand.CreateFromTask(async () =>
    {
        try
        {
            if (string.IsNullOrWhiteSpace(config.Name)) throw new Exception($"Please Set Employee.");
            var defaultWorkTime = DateTime.Today.AddHours(17).AddMinutes(30);
            var (dialogResult, dateTime) =
                await dialogService.ShowWorkInOutDialog($"{config.Name} 下班打卡", defaultWorkTime);
            if (!dialogResult) return;

            await clockInSheetService.WriteWorkOffTime(dateTime.Date, config.Name, dateTime.TimeOfDay);
            await punchQuery.QueryCommand.Execute(punchQuery.QueryParameter);
            await dialogService.ShowMessageBox("Message", "Success");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await dialogService.ShowMessageBox("Message", $"Fail {e.Message}");
        }
    });

    public ReactiveCommand<Unit, Unit> DailyCommand => ReactiveCommand.CreateFromTask(async () =>
    {
        try
        {
            if (string.IsNullOrWhiteSpace(config.Name)) throw new Exception($"Please Set Employee.");
            var employee = employeeRepository.GetAll().FirstOrDefault(e => e.Id == config.Name);
            if (employee == null) throw new Exception($"Employee {config.Name} Not Found");

            var (dialogResult, daily) = await dialogService.ShowDailyDialog("日報填寫", dailySheetService.DailyTypes, employee);
            if (!dialogResult) return;

            await dailySheetService.WriteDaily(
                daily.DateTime,
                employee.Department,
                $"{employee.ChineseName} {employee.EnglishName}",
                daily.Hour,
                daily.SelectDailyType,
                daily.Daily);
            await dialogService.ShowMessageBox("Message", "Success");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await dialogService.ShowMessageBox("Message", $"Fail {e.Message}");
        }
    });

    #endregion


    public MainViewModel(
        IClockInSheetService clockInSheetService,
        IEmployeeRepository employeeRepository,
        IQuartzService quartzService,
        IDialogService dialogService,
        IDailySheetService dailySheetService,
        IKeyValueRepository keyValueRepository,
        IConfig config,
        PunchQueryViewModel punchQuery)
    {
        this.clockInSheetService = clockInSheetService;
        this.employeeRepository = employeeRepository;
        this.quartzService = quartzService;
        this.dialogService = dialogService;
        this.dailySheetService = dailySheetService;
        this.keyValueRepository = keyValueRepository;
        this.config = config;
        this.punchQuery = punchQuery;


        titleProperty = config.ObservableForProperty(x => x.Title)
            .Select(x => x.Value)
            .StartWith(keyValueRepository.GetTitle())
            .Select(x => string.IsNullOrWhiteSpace(x) switch { true => "線上打卡", _ => x })
            .Do(x => keyValueRepository.SetTitle(x))
            .ToProperty(this, vm => vm.Title);

        InitCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var jobSchedules = await this.quartzService.GetJobSchedules();
            foreach (var jobSchedule in jobSchedules)
            {
                await quartzService.TriggerJobAsync(jobSchedule.JobName);
            }
        });

        this.quartzService.StartAsync(CancellationToken.None);
    }
    
    public ReactiveCommand<Unit, Unit> TestCommand => ReactiveCommand.Create(() =>
    {
        var notify = new ViewModelLocator().FancyBalloon;
        notify.Show = !notify.Show;
    });
}