using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reactive;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using Punches.Models;
using ReactiveUI;

namespace PunchClockIn.ViewModels
{
    public class DailyViewModel : ReactiveObject, IDataErrorInfo
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

        private string daily = "";

        [Required]
        public string Daily
        {
            get => daily;
            set => this.RaiseAndSetIfChanged(ref daily, value);
        }

        public string[] DailyTypes { get; set; }
        private string selectDailyType = "";

        public string SelectDailyType
        {
            get => selectDailyType;
            set => this.RaiseAndSetIfChanged(ref selectDailyType, value);
        }

        private double hour = 0;

        public double Hour
        {
            get => hour;
            set => this.RaiseAndSetIfChanged(ref hour, value);
        }

        private Employee employee = null;

        public Employee Employee
        {
            get => employee;
            set => this.RaiseAndSetIfChanged(ref employee, value);
        }

        private string note = "";

        public string Note
        {
            get => note;
            set => this.RaiseAndSetIfChanged(ref note, value);
        }

        public DailyViewModel(Func<Task> closeAction)
        {
            DateTime = DateTime.Today;
            CloseCommand = ReactiveCommand.CreateFromTask<bool>(b =>
            {
                Result = b ? MessageDialogResult.Affirmative : MessageDialogResult.Canceled;

                if (b == false) return closeAction();
                if (!string.IsNullOrWhiteSpace(Error)) return Task.CompletedTask;

                return closeAction();
            });
        }


        /*
        public IEnumerable GetErrors(string? propertyName)
        {
            throw new NotImplementedException();
        }
        
        public bool HasErrors { get; } 
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        */
        public string Error => this[nameof(Daily)];

        public string this[string columnName]
        {
            get
            {
                return columnName switch
                {
                    nameof(Daily) when Daily is not { Length: > 0 } => "請輸入 日報內容",
                    _ => null,
                };
            }
        }
    }
}
