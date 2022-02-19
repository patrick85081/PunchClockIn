using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using MahApps.Metro.Controls;
using PunchClockIn.Commands;
using PunchClockIn.ViewModels;

namespace PunchClockIn
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow //Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;
            this.StateChanged += OnStateChanged;
            if (Environment.GetCommandLineArgs().Contains("-hide"))
                this.Activated += (s, e) => this.Hide();
            SingletonApp.HandleShow(this);

            /*
            var showCommandBinding = new CommandBinding(MainWindowCommands.ShowCommand, (s, e) =>
            {
                e.Handled = true;
                Show();
            });
            showCommandBinding.CanExecute += (sender, args) => args.CanExecute = this.Visibility != Visibility.Visible;
            CommandBindings.Add(
                showCommandBinding
            );
            var hideCommandBinding = new CommandBinding(MainWindowCommands.HideCommand,
                (s, e) =>
                    Hide(),
                (s, e) =>
                    e.CanExecute = true);
            CommandBindings.Add(hideCommandBinding);
        */
        }

        private void OnStateChanged(object? sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            /*
            if (MessageBox.Show(
                    this, "縮到工具列嗎?", "Message", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
                e.Cancel = true;
                this.Hide();
                return;
            }
            */
            if (!Debugger.IsAttached)
            {
                e.Cancel = true;
                this.Hide();
                return;
            }
        }

        private void MainWindow_OnStateChanged(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                MainWindowCommands.HideCommand.Execute().Subscribe();
            }
        }
    }
}
