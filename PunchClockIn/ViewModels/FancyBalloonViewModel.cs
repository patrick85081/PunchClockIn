using System;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;

namespace PunchClockIn.ViewModels;

public class FancyBalloonViewModel : ReactiveObject
{
    private bool show = false;
    public bool Show
    {
        get => show;
        set => this.RaiseAndSetIfChanged(ref show, value);
    }

    public ReactiveCommand<Unit, bool> CloseCommand => ReactiveCommand.Create(() => Show = false);

    private DateTime targetTime;
    private readonly ObservableAsPropertyHelper<TimeSpan> countDownBackwardProperty;
    private ObservableAsPropertyHelper<string> negativeProperty;

    private string title = "溫馨提醒";
    public string Title
    {
        get => title;
        set => this.RaiseAndSetIfChanged(ref title, value);
    }
    public DateTime TargetTime
    {
        get => targetTime;
        set => this.RaiseAndSetIfChanged(ref targetTime, value);
    }
    public TimeSpan CountDownBackward => countDownBackwardProperty.Value;
    public string Negative => negativeProperty.Value;


    public FancyBalloonViewModel()
    {
        countDownBackwardProperty = Observable.Interval(TimeSpan.FromSeconds(1))
            .Select(_ => TargetTime - DateTime.Now)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.CountDownBackward);
        negativeProperty = this.ObservableForProperty(
                x => x.CountDownBackward,
                time => time < TimeSpan.Zero ? "-" : string.Empty)
            .ToProperty(this, x => x.Negative);
    }
}