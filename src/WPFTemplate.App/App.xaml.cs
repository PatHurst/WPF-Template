using InnoJob.App.Resources.Theme;
using InnoJob.App.Views.Windows;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InnoJob.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public App()
    {
        var services = new ServiceCollection();
        services.AddLogging(c => c.AddFile(o => o.RootPath = Environment.ProcessPath));

        ServiceProvider = services.BuildServiceProvider();
    }

    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        InitializeTheme();
        _ = InitializeWindows();
    }

    private async Task InitializeWindows()
    {
        var splash = new Views.Windows.SplashScreen();
        splash.Show();

        await Task.Delay(2_000);

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();
        splash.Close();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {

    }

    private void InitializeTheme()
    {
        ThemeManager.SetTheme((Theme)Settings.AppTheme);

        var (r, g, b) = IntToRgb(Settings.AccentColor);
        ThemeManager.SetAccentColor(Color.FromRgb(r, g, b));
    }
}
