using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using PunchClockIn.Configs;
using Punches.Repository;
using ReactiveUI;

namespace PunchClockIn.ViewModels;

public class SettingsViewModel : ReactiveObject
{
    private readonly IEmployeeRepository employeeRepository;
    private readonly IConfig config;
    public ObservableCollection<string> Employees { get; } = new ObservableCollection<string>();
    private string selectEmployee;
    private bool enableNotify;
    private bool enableAutoClockOff;

    public string SelectEmployee
    {
        get => selectEmployee;
        set => this.RaiseAndSetIfChanged(ref selectEmployee, value);
    }
    public bool EnableNotify
    {
        get => enableNotify;
        set => this.RaiseAndSetIfChanged(ref enableNotify, value);
    }

    public bool EnableAutoClockOff
    {
        get => enableAutoClockOff;
        set => this.RaiseAndSetIfChanged(ref enableAutoClockOff, value);
    }

    public SettingsViewModel(IEmployeeRepository employeeRepository, IConfig config)
    {
        this.employeeRepository = employeeRepository;
        this.config = config;

        Employees.AddRange(employeeRepository.GetAll().Select(x => x.Id));
        SelectEmployee = config.Name;
        EnableNotify = config.EnableNotify;
        EnableAutoClockOff = config.AutoWorkOff;

        this.ObservableForProperty(x => x.SelectEmployee)
            .Select(x => x.Value)
            .Subscribe(x => config.Name = x);
        this.ObservableForProperty(x => x.EnableNotify)
            .Select(x => x.Value)
            .Subscribe(x => config.EnableNotify = x);
        this.ObservableForProperty(x => x.EnableAutoClockOff)
            .Select(x => x.Value)
            .Subscribe(x => config.AutoWorkOff = x);
    }    
    
}