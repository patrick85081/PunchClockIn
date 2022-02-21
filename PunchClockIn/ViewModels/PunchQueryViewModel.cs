using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using PunchClockIn.Configs;
using Punches.Models;
using Punches.Repository;
using Punches.Repository.Extensions;
using Punches.Repository.Services;
using ReactiveUI;

namespace PunchClockIn.ViewModels;

public class PunchQueryViewModel : ReactiveObject
{
    private readonly IEmployeeRepository employeeRepository;
    private readonly IClockInRepository clockInRepository;
    private readonly IClockMonthRepository clockMonthRepository;
    private readonly IPunchSheetService punchSheetService;
    private readonly IKeyValueRepository keyValueRepository;
    private readonly IConfig config;

    #region Property

    private string[] nameList = Array.Empty<string>();

    public string[] NameList
    {
        get => nameList;
        set => this.RaiseAndSetIfChanged(ref nameList, value);
    }

    private string title = "線上打卡";

    public string Title
    {
        get => title;
        set => this.RaiseAndSetIfChanged(ref title, value);
    }

    public ObservableCollection<ClockIn> ClockIns { get; } = new ObservableCollection<ClockIn>();
    private ClockMonth[] sheets = Array.Empty<ClockMonth>();
    private ClockMonth selectSheets;

    /// <summary>
    /// 月份
    /// </summary>
    public ClockMonth[] Sheets
    {
        get => sheets;
        set => this.RaiseAndSetIfChanged(ref sheets, value);
    }

    public ClockMonth SelectSheets
    {
        get => selectSheets;
        set
        {
            this.RaiseAndSetIfChanged(ref selectSheets, value);
            QueryParameter.Month = value?.Id;
        }
    }

    public QueryParameter QueryParameter { get; } = new QueryParameter();

    #endregion

    public ReactiveCommand<QueryParameter, QueryParameter> QueryCommand { get; }

    private readonly object _locker = new();
    public PunchQueryViewModel(
        IEmployeeRepository employeeRepository,
        IClockInRepository clockInRepository,
        IClockMonthRepository clockMonthRepository,
        IPunchSheetService punchSheetService,
        IKeyValueRepository keyValueRepository,
        IConfig config
    )
    {
        this.employeeRepository = employeeRepository;
        this.clockInRepository = clockInRepository;
        this.clockMonthRepository = clockMonthRepository;
        this.punchSheetService = punchSheetService;
        this.keyValueRepository = keyValueRepository;
        this.config = config;


        NameObservable(this.employeeRepository, this.punchSheetService)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(names =>
            {
                NameList = names;
                if (QueryParameter.Name == null)
                    QueryParameter.Name = names.FirstOrDefault(x => x == config.Name) ?? names.LastOrDefault();
            });

        TitleAndMonthObservable()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(item =>
            {
                var (title, month) = item;
                var today = DateTime.Today;
                //Title = title;
                config.Title = title;
                // keyValueRepository.SetTitle(title);
                Sheets = month;
                SelectSheets =
                    month.FirstOrDefault(x => x.Month.Year == today.Year && x.Month.Month == today.Month) ??
                    month.FirstOrDefault();
            });


        // var canQuery = this.QueryParameter.Changed
        //     .Select(x => !string.IsNullOrEmpty(QueryParameter.Month) && !string.IsNullOrEmpty(QueryParameter.Month));
        var canQuery = this.WhenAnyValue(
                vm => vm.QueryParameter.Month,
                vm => vm.QueryParameter.Name)
            .Select(((string month, string name) x) => x is { month: { Length: > 0 }, name: { Length: > 0 } });
        QueryCommand = ReactiveCommand.Create<QueryParameter, QueryParameter>((q) => q, canQuery);

        var queryEvent = this.WhenAnyValue(
                vm => vm.QueryParameter.Month,
                vm => vm.QueryParameter.Name)
            .Select(_ => QueryParameter)
            .Merge(QueryCommand)
            .Where(parameter => !string.IsNullOrEmpty(parameter.Month) && !string.IsNullOrEmpty(parameter.Name))
            .Throttle(TimeSpan.FromMilliseconds(100));

        QueryObservable(queryEvent, this.clockInRepository)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(newClockIn =>
            {
                lock (_locker)
                {
                    ClockIns.Clear();
                    ClockIns.AddRange(newClockIn);
                }
            });

        this.config.ObservableForProperty(c => c.Name)
            .Select(x => x.Value)
            .Subscribe(x => QueryParameter.Name = x);
            
    }

    private IObservable<string[]> NameObservable(IEmployeeRepository employeeRepository, IPunchSheetService punchSheetService)
    {
        var fromDB = Observable.Return(employeeRepository.GetAll());
        var fromNetwork = Observable.FromAsync(punchSheetService.GetEmployee)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Do(data => this.employeeRepository.Set(data));

        var nameObservable = fromDB.Merge(fromNetwork)
            .Select(e => e.Where(em => em.Department != null)
                .OrderBy(em => em.Index)
                .Select(em => em.Id)
                .ToArray());
        return nameObservable;
    }

    private IObservable<(string title, ClockMonth[])> TitleAndMonthObservable()
    {
        var fromDb = Observable.Return(
            (title: this.keyValueRepository.GetTitle(), month: this.clockMonthRepository.GetAll().ToArray()));

        var fromNetwork = Observable
            .FromAsync(cancellationToken => this.punchSheetService.GetTitleAndMonth(cancellationToken))
            .Do(item => this.config.Title = item.title)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Do(item => this.clockMonthRepository.Set(item.month));

        var titleAndMonthObservable = fromDb.Merge(fromNetwork)
            .Select(item => 
                (item.title, item.month.OrderByDescending(m => m.Id).ToArray()));
        return titleAndMonthObservable;
    }

    private IObservable<ClockIn[]> QueryObservable(
        IObservable<QueryParameter> queryEvent,
        IClockInRepository clockInRepository)
    {
        var fromDB = queryEvent
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select(query => clockInRepository.Get(query).ToArray());

        var fromNetwork = queryEvent
            .Select(qp =>
                Observable.FromAsync(cancellationToken => this.punchSheetService.QueryMonthAsync(qp.Month, cancellationToken)))
            .Switch()
            .Select(data => data.GroupBy(d => d.Name)
                .Select(x => x.JoinToEmptyDate(QueryParameter.Month, x.Key).ToArray())
                .SelectMany(x => x))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Do(data => clockInRepository.Set(data))
            .Select(data => data.Where(d => d.Name == QueryParameter.Name)
                .ToArray());

        var queryObservable = fromDB.Merge(fromNetwork)
            .Select(data => data.JoinToEmptyDate(QueryParameter).ToArray());
        return queryObservable;
    }
}