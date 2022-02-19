using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using PunchClockIn.ViewModels;
using PunchClockIn.Views;
using Punches.Models;

namespace PunchClockIn.Services
{
    public class DialogService : IDialogService
    {
        public async Task<(bool DialogResult, DailyViewModel Daily)> ShowDailyDialog(
            string title, DailyType[] dailyTypes, Employee employee)
        {
            var metroWindow = Application.Current.MainWindow as MetroWindow;
            var dialog = new CustomDialog(metroWindow)
            {
                Title = title,
            };
            var viewModel = new DailyViewModel(
                () => metroWindow.HideMetroDialogAsync(dialog))
            {
                DailyTypes = dailyTypes.Select(x => x.Name).ToArray(),
                SelectDailyType = "優化",
                Hour = 6.5,
                Employee = employee,
            };
            dialog.Content = new DailyDialog()
            {
                DataContext = viewModel
            };
            await metroWindow.ShowMetroDialogAsync(dialog);
            await dialog.WaitUntilUnloadedAsync();
            return (viewModel.Result == MessageDialogResult.Affirmative, viewModel);
        }

        public async Task<(bool DialogResult, DateTime DateTime)> ShowWorkInOutDialog(string title, DateTime workTime)
        {
            var metroWindow = Application.Current.MainWindow as MetroWindow;
            var dialog = new CustomDialog(metroWindow)
            {
                Title = title,
            };
            var viewModel = new WorkInOutViewModel(workTime,
                () => metroWindow.HideMetroDialogAsync(dialog));
            dialog.Content = new WorkInOutDialog()
            {
                DataContext = viewModel
            };
            await metroWindow.ShowMetroDialogAsync(dialog);
            await dialog.WaitUntilUnloadedAsync();
            return (viewModel.Result == MessageDialogResult.Affirmative, viewModel.DateTime);
        }

        public async Task<MessageDialogResult> ShowMessageBox(
            string title, string message,
            MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings setting = null)
        {
            var metroWindow = Application.Current.MainWindow as MetroWindow;
            return await metroWindow.ShowMessageAsync(title, message, style, setting);
        }
        public async Task<string> ShowInputDialog(
            string title, string message,
            MetroDialogSettings setting = null)
        {
            var metroWindow = Application.Current.MainWindow as MetroWindow;
            return await metroWindow.ShowInputAsync(title, message, setting);
        }
    }
}
