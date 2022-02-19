using System;
using System.Reactive;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;

namespace PunchClockIn.ViewModels;

public class WorkInOutViewModel : ReactiveObject
{
    public ReactiveCommand<bool, Unit> CloseCommand { get; }
    private DateTime dateTime;
    public DateTime DateTime
    {
        get => dateTime;
        set => this.RaiseAndSetIfChanged(ref dateTime, value);
    }

    private MessageDialogResult result = MessageDialogResult.Canceled;
    public MessageDialogResult Result
    {
        get => result;
        set => this.RaiseAndSetIfChanged(ref result, value);
    }

    public WorkInOutViewModel(DateTime dateTime, Func<Task> func)
    {
        CloseCommand = ReactiveCommand.CreateFromTask<bool>((b) =>
        // CloseCommand = ReactiveCommand.CreateFromTask(() =>
        {
            Result = b == true ? MessageDialogResult.Affirmative : MessageDialogResult.Canceled;
            return func();
        });
        DateTime = dateTime;

        // MessageDialogResult.Affirmative
    }
}