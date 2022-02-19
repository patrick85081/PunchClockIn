using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Punches.Models;

public class QueryParameter : INotifyPropertyChanged//: ReactiveObject
{
    private string name = null;
    public string Name
    {
        get => name;
        set => this.RaiseAndSetIfChanged(ref name, value);
    }
    private string month = null;
    public string Month
    {
        get => month;
        set => this.RaiseAndSetIfChanged(ref month, value);
    }
    
    private void RaiseAndSetIfChanged<T>(ref T field,T value, [CallerMemberName] string methodName = null)
    {
        if(object.Equals(field, value)) return;

        field = value;
        OnPropertyChanged(methodName);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}