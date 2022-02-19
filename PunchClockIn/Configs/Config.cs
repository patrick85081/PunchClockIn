using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using IniParser.Model;
using IniParser.Model.Configuration;
using IniParser.Parser;
using ReactiveUI;

namespace PunchClockIn.Configs
{
    public class Config : ReactiveObject, IConfig
    {
        private readonly IniData reader;

        private readonly string sectionName = "App";

        private readonly string configIni;
        protected string ConfigIni => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configIni);

        Dictionary<Type, Func<object, object>> mapping = new()
        {
            {typeof(bool), x => bool.TryParse(x?.ToString(), out var value) ? value : default(bool)},
            {typeof(string), x => x?.ToString()},
        };

        public Config()
        {
            configIni = "Config.ini";
            var parser = new IniParser.FileIniDataParser(new IniDataParser(new IniParserConfiguration()
            {
                CommentRegex = new Regex(@"^[;#\-]"),
            }));
            if (File.Exists(ConfigIni))
                reader = parser.ReadFile(ConfigIni, Encoding.UTF8);
            else
                reader = new IniData();

            var x = this.GetType()
                .GetProperties()
                .ToObservable()
                .Where(p => mapping.ContainsKey(p.PropertyType))
                .Do(p =>
                {
                    var value = mapping[p.PropertyType](reader[sectionName][p.Name]);
                    p.SetValue(this, value);
                })
                .ToList()
                .GetAwaiter()
                .GetResult();

            this.Changed
                .GroupBy(g => g.PropertyName)
                .SelectMany(WriteData)
                .Throttle(TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                {
                    parser.WriteFile(ConfigIni, reader);
                });

            IObservable<PropertyInfo> WriteData(IGroupedObservable<string, IReactivePropertyChangedEventArgs<IReactiveObject>> source) =>
                source.Select(eventArgs => eventArgs.Sender.GetType()
                        .GetProperty(eventArgs.PropertyName))
                    .Do(property =>
                    {
                        var value = property.GetValue(this);
                        reader[sectionName][property.Name] = value?.ToString();
                    });
        }

        private string name = null;
        private bool enableNotify = false;
        private string title = null;
        private bool debugMode;
        private bool autoWorkOff;
        private string certFilePath;
        private string punchSpreadsheetId;
        private string dailySpreadsheetId;

        public string Title
        {
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }

        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        public bool EnableNotify
        {
            get => enableNotify;
            set => this.RaiseAndSetIfChanged(ref enableNotify, value);
        }

        public bool AutoWorkOff
        {
            get => autoWorkOff;
            set => this.RaiseAndSetIfChanged(ref autoWorkOff, value);
        }

        public bool DebugMode
        {
            get => debugMode;
            private set => this.RaiseAndSetIfChanged(ref debugMode, value);
        }

        public string ClientSecretFilePath
        {
            get => certFilePath;
            private set => this.RaiseAndSetIfChanged(ref certFilePath, value);
        }

        public string PunchSpreadsheetId
        {
            get => punchSpreadsheetId;
            set => this.RaiseAndSetIfChanged(ref punchSpreadsheetId, value);
        }

        public string DailySpreadsheetId
        {
            get => dailySpreadsheetId;
            private set => this.RaiseAndSetIfChanged(ref dailySpreadsheetId, value);
        }
    }
}
