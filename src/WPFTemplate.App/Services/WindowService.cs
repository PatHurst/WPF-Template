using Microsoft.Extensions.DependencyInjection;

namespace WPFTemplate.App.Services;

internal class WindowService(IServiceProvider services) : IWindowService
{
    public bool? ShowDialog<TWindow>() where TWindow : Window
    {
        var window = services.GetRequiredService<TWindow>();
        window.Owner = Application.Current.MainWindow;
        return window.ShowDialog();
    }
}
