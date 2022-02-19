using System.Reactive;
using System.Windows;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;
using ReactiveUI;

namespace PunchClockIn.Commands
{
    // public static class NotifyIconCommands
    // {
        // public static RoutedUICommand CloseNotifyCommand { get; } = new();
        // public static RoutedUICommand ShowNotifyCommand { get; } = new();

    //     static NotifyIconCommands()
    //     {
    //     }
    // }

    public static class MainWindowCommands
    {
        public static ReactiveCommand<Unit, Unit> ShowCommand { get; }
        public static ReactiveCommand<Unit, Unit> HideCommand { get; }

        static MainWindowCommands()
        {
            var input = new InputGestureCollection();

            ShowCommand = ReactiveCommand.Create(() =>
            {
                if (Application.Current.MainWindow == null)
                    Application.Current.MainWindow = new MainWindow();
                Application.Current.MainWindow?.Activate();
                Application.Current.MainWindow?.Show();
            });
            HideCommand = ReactiveCommand.Create(() =>
            {
                Application.Current.MainWindow?.Hide();
                (Application.Current as App)
                    ?.NotifyIcon
                    ?.ShowBalloonTip(Application.Current.MainWindow?.Title ?? "打卡程式", "縮小在工具列 繼續執行", BalloonIcon.Info);
            });
        }

    }
}