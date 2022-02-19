using System;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using PunchClockIn.ViewModels;
using Punches.Models;

namespace PunchClockIn.Services;

public interface IDialogService
{
    Task<(bool DialogResult, DateTime DateTime)> ShowWorkInOutDialog(string title, DateTime workTime);

    Task<MessageDialogResult> ShowMessageBox(
        string title, string message,
        MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings setting = null);

    Task<string> ShowInputDialog(
        string title, string message,
        MetroDialogSettings setting = null);

    Task<(bool DialogResult, DailyViewModel Daily)> ShowDailyDialog(string title, DailyType[] dailyTypes,
        Employee? employee);
}