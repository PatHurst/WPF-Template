using System.Windows.Threading;

using WPFTemplate.App.Services;
using WPFTemplate.App.ViewModels.Base;

namespace WPFTemplate.App.ViewModels.Windows;

internal class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel(MenuViewModel menu, NavigationService navigation)
    {
        Menu = menu;
        Navigation = navigation;
        StatusText = string.Empty;

        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) => StatusText = DateTime.Now.ToString();
        timer.Start();
    }

    public MenuViewModel Menu { get; }
    public NavigationService Navigation { get; }

    public string StatusText
    {
        get => field;
        set => SetProperty(ref field, value);
    }
}
