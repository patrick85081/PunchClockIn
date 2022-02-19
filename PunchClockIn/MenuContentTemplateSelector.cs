using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using PunchClockIn.ViewModels;

namespace PunchClockIn;

public class MenuContentTemplateSelector : DataTemplateSelector
{
    public Dictionary<object, DataTemplate> Templates { get; } = new Dictionary<object, DataTemplate>();
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item == null) 
            return null;

        var result = item switch
        {
            MenuType type => type,
            HamburgerMenuItem h => (MenuType)h.Tag,
            _ => MenuType.ClockIn,
        };

        if (Templates.TryGetValue(result, out var template))
            return template;
        else 
            return Templates?.Values?.FirstOrDefault();
    }
}