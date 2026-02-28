using System.Timers;
using WPFTemplate.App.Services;
using WPFTemplate.App.ViewModels;

namespace WPFTemplate.App.ViewModels.WindowViewModels;

internal class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel(MenuViewModel menu, NavigationService navigation)
    {
        Menu = menu;
        Navigation = navigation;
        StatusText = string.Empty;

        var timer = new System.Timers.Timer()
        {
            AutoReset = true,
            Interval = 1_000,
            Enabled = true
        };
        timer.Elapsed += (_, e) => StatusText = e.SignalTime.ToLocalTime().ToString();
    }

    public MenuViewModel Menu { get; }
    public NavigationService Navigation { get; }

    public string StatusText
    {
        get => field;
        set => SetProperty(ref field, value);
    }
}
