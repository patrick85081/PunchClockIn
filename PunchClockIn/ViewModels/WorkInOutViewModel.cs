using System;
using System.Reactive;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;

namespace PunchClockIn.ViewModels;

public class WorkInOutViewModel : ReactiveObject
{
    public ReactiveCommand<bool, Unit> CloseCommand { get; }
    private WorkType workType;

    public WorkType WorkType
    {
        get => workType;
        set => this.RaiseAndSetIfChanged(ref workType, value);
    }
    private DateTime dateTime;
    public DateTime DateTime
    {
        get => dateTime;
        set => this.RaiseAndSetIfChanged(ref dateTime, value);
    }

    private string location = "公司";
    public string Location
    {
        get => location;
        set => this.RaiseAndSetIfChanged(ref location, value);
    }

    private string remark = "";
    public string Remark
    {
        get => remark;
        set => this.RaiseAndSetIfChanged(ref remark, value);
    }

    private MessageDialogResult result = MessageDialogResult.Canceled;
    public MessageDialogResult Result
    {
        get => result;
        set => this.RaiseAndSetIfChanged(ref result, value);
    }
    
    public WorkInOutViewModel(DateTime dateTime, WorkType workType, Func<Task> func)
    {
        CloseCommand = ReactiveCommand.CreateFromTask<bool>((b) =>
        // CloseCommand = ReactiveCommand.CreateFromTask(() =>
        {
            Result = b == true ? MessageDialogResult.Affirmative : MessageDialogResult.Canceled;
            return func();
        });
        DateTime = dateTime;
        WorkType = workType;

        // MessageDialogResult.Affirmative
    }
}