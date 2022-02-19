using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using PunchClockIn.ViewModels;

namespace PunchClockIn
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon notifyIcon;
        public TaskbarIcon NotifyIcon => notifyIcon;

        public App()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            if (Debugger.IsAttached)
            {
                var cur =Process.GetCurrentProcess();
                Process.GetProcessesByName("PunchClockIn")
                    .Where(p => p!=cur)
                    .ToList()
                    .ForEach(p => p.CloseMainWindow());
            }
                
            if (!SingletonApp.IsRunning("ClockIn"))
            {
                if (!Environment.GetCommandLineArgs().Contains("-hide"))
                    SingletonApp.SendShowCommand();
                Shutdown(0);
            }
            
            InitializeComponent();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            notifyIcon?.Dispose();
            // RxApp.MainThreadScheduler
            //     .Schedule(Unit.Default,
            //         ((scheduler, unit) =>
            //         {
            //             SingletonApp.Exit();
            //             return new Disposable();
            //         }));

            SingletonApp.Exit();

            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
        }
    }
}
