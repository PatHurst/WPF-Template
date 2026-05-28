using System.Windows.Threading;

using WPFTemplate.App.Services;
using WPFTemplate.App.ViewModels.Base;

namespace WPFTemplate.App.ViewModels.Windows;

internal class MainWindowViewModel : ObservableObject, IDisposable
{
    private readonly DispatcherTimer _timer;

    public MainWindowViewModel(MenuViewModel menu, NavigationService navigation)
    {
        Menu = menu;
        Navigation = navigation;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) => StatusText = DateTime.Now.ToString();
        _timer.Start();
    }

    public void Dispose() => _timer.Stop();

    public MenuViewModel Menu { get; }
    public NavigationService Navigation { get; }

    public string StatusText
    {
        get => field ??= string.Empty;
        set => SetProperty(ref field, value);
    }

    public string NotificationText
    {
        get => field ??= string.Empty;
        set => SetProperty(ref field, value);
    }
}
