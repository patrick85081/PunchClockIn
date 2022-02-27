using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using Punches.Models;
using Punches.Services;
using ReactiveUI;

namespace PunchClockIn.ViewModels;

public class DailyQueryViewModel : ReactiveObject
{
    private readonly IDailySheetService dailySheetService;
    private readonly ObservableAsPropertyHelper<DailyModel[]> showDailyProperty;
    public ObservableCollection<DailyModel> DailyModels { get; } = new ObservableCollection<DailyModel>();

    public DailyModel[] ShowDaily => showDailyProperty.Value;
    public ReactiveCommand<DateTime, Unit> Query { get; }
    public ObservableCollection<string> Names { get; } = new ObservableCollection<string>();
    private string selectName = "All";

    public string SelectName
    {
        get => selectName;
        set => this.RaiseAndSetIfChanged(ref selectName, value);
    }

    public DailyQueryViewModel(IDailySheetService dailySheetService)
    {
        this.dailySheetService = dailySheetService;
        Names.Add("All");
        SelectName = "All";
        this.ObservableForProperty(vm => vm.SelectName)
            .Select(x => x.Value)
            .ToProperty(this, x => x.SelectName);
            // .Subscribe(Console.WriteLine);
        this.showDailyProperty = Observable.CombineLatest(
            DailyModels.ObserveCollectionChanges().Select(x => x.Sender as IEnumerable<DailyModel>),
            this.ObservableForProperty(vm => vm.SelectName).Select(x => x.Value),
            (models, name) =>
                models.Where(x => x.Name == name || name == "All")
                    .ToArray())
            .ToProperty(this, vm => vm.ShowDaily);
        // this.showDailyProperty = this.WhenAny(
        //         vm => vm.DailyModels,
        //         vm => vm.SelectName,
        //         (models, name) =>
        //             models.Value
        //                 .Where(x => x.Name == name.Value || name.Value == "All")
        //                 .ToArray())
            // .ToProperty(this, vm => ShowDaily);
        Query = ReactiveCommand.CreateFromTask<DateTime, Unit>(async date =>
        {
            var dailyModels = await dailySheetService.GetDaily(date);
            DailyModels.Clear();
            DailyModels.AddRange(dailyModels);

            var select = SelectName;
            var names = DailyModels.GroupBy(x => x.Name)
                .Select(x => x.Key)
                .ToArray();
            if (names.Any())
            {
                Names.Clear();
                Names.Add("All");
                Names.AddRange(names);
                SelectName = select;
            }
            
            return Unit.Default;
        });
    }
}