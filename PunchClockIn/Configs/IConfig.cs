using System.ComponentModel;

namespace PunchClockIn.Configs;

public interface IConfig : INotifyPropertyChanged
{
    string Name { get; set; }
    bool EnableNotify { get; set; }
    string Title { get; set; }
    bool DebugMode { get; }
    string ClientSecretFilePath { get; }
    bool AutoWorkOff { get; set; }
}