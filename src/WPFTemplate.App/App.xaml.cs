using WPFTemplate.App.Resources.Theme;
using WPFTemplate.App.Services;
using WPFTemplate.App.ViewModels;
using WPFTemplate.App.ViewModels.WindowViewModels;
using WPFTemplate.App.Views.Windows;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WPFTemplate.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    public App()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging(c => c.AddFile(o => o.RootPath = Environment.ProcessPath));
        services.AddSingleton<NavigationService>();
        services.AddTransient<MenuViewModel>();
        services.AddTransient<HomePageViewModel>();
        services.AddTransient<LogPageViewModel>();
        services.AddTransient<MainWindowViewModel>();
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

        ServiceProvider.GetRequiredService<NavigationService>().NavigateTo<HomePageViewModel>();

        var mainWindow = new MainWindow(ServiceProvider.GetRequiredService<MainWindowViewModel>());
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
