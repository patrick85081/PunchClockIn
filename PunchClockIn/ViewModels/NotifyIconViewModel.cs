using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using DynamicData.Binding;
using Hardcodet.Wpf.TaskbarNotification;
using ReactiveUI;

namespace PunchClockIn.ViewModels;

public class NotifyIconViewModel : ReactiveObject
{
    public bool IsShow => isShowProperty.Value;
    public bool IsHide => isHideProperty.Value;
    private readonly ObservableAsPropertyHelper<bool> isHideProperty;
    private readonly ObservableAsPropertyHelper<bool> isShowProperty;


    private bool isShowBalloon = false;

    public bool IsShowBalloon
    {
        get => isShowBalloon;
        set => this.RaiseAndSetIfChanged(ref isShowBalloon, value);
    }
    public NotifyIconViewModel()
    {
        InitCommand = ReactiveCommand.Create(Init);
        var isShow = InitCommand.Select(v => Application.Current.MainWindow)
            .SelectMany(v => v.ObservableForProperty(w => w.Visibility)
                .Select(v => v.Value)
                .StartWith(v.Visibility)
                .Select(v => v == Visibility.Visible))
            .DistinctUntilChanged();
        var isHide = isShow.Select(b => !b);

        isShowProperty = isShow.ToProperty(this, vm => vm.IsShow);
        isHideProperty = isHide.ToProperty(this, vm => vm.IsHide);
        ShowCommand = ReactiveCommand.Create(() =>
        {
            if (Application.Current.MainWindow == null)
                Application.Current.MainWindow = new MainWindow();
            Application.Current.MainWindow?.Activate();
            Application.Current.MainWindow?.Show();
        }, this.WhenPropertyChanged(vm => vm.IsHide).Select(x => x.Value));
        HideCommand = ReactiveCommand.Create(() =>
            {
                Application.Current.MainWindow?.Hide();
                (Application.Current as App)
                    ?.NotifyIcon
                    ?.ShowBalloonTip(Application.Current.MainWindow?.Title ?? "打卡程式", "縮小在工具列 繼續執行", BalloonIcon.Info);
            },
            this.WhenPropertyChanged(vm => vm.IsShow).Select(x => x.Value));
    }

    void Init()
    {
    }

    public ReactiveCommand<Unit, Unit> InitCommand { get; }

    public ReactiveCommand<Unit, Unit> ShowCommand { get; }

    public ReactiveCommand<Unit, Unit> HideCommand { get; }

    public ReactiveCommand<Unit, Unit> ExitCommand =>
        ReactiveCommand.Create(() => { Application.Current.Shutdown(0); });
}