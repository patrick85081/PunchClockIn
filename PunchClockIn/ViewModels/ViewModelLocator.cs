using Splat;

namespace PunchClockIn.ViewModels
{
    public class ViewModelLocator
    {
        public NotifyIconViewModel Notify => Locator.Current.GetService<NotifyIconViewModel>();
        public MainViewModel Main => Locator.Current.GetService<MainViewModel>();
        public SettingsViewModel Settings => Locator.Current.GetService<SettingsViewModel>();
        public FancyBalloonViewModel FancyBalloon => Locator.Current.GetService<FancyBalloonViewModel>();
        public PunchQueryViewModel PunchQuery => Locator.Current.GetService<PunchQueryViewModel>();
        public DailyQueryViewModel DailyQuery => Locator.Current.GetService<DailyQueryViewModel>();
    }
}
