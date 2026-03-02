using System.Windows.Threading;

using WPFTemplate.App.Services;
using WPFTemplate.App.ViewModels.Pages;
using WPFTemplate.App.ViewModels.Windows;
using WPFTemplate.App.Views.Windows;
using WPFTemplate.Services.Database;

using Karambolo.Extensions.Logging.File;

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

        ServiceProvider = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddLogging(c => c.AddFile(o =>
            {
                o.RootPath = AppContext.BaseDirectory;
                o.Files = [new LogFileOptions { Path = "app.log" }];
            }))
            .AddSingleton<NavigationService>()
            .AddTransient<MenuViewModel>()
            .AddTransient<HomePageViewModel>()
            .AddTransient<LogPageViewModel>()
            .AddTransient<MainWindowViewModel>()
            .BuildServiceProvider();

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        var connStr = ServiceProvider.GetRequiredService<IConfiguration>()["Database:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(connStr))
            Db.Configure(connStr);

        InitializeTheme();
        await InitializeWindows();
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
        (ServiceProvider as IDisposable)?.Dispose();
    }

    private void InitializeTheme()
    {
        ThemeManager.SetTheme((Theme)Settings.AppTheme);

        var (r, g, b) = IntToRgb(Settings.AccentColor);
        ThemeManager.SetAccentColor(Color.FromRgb(r, g, b));
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogAndShow(e.Exception);
        e.Handled = true;
        Shutdown(1);
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            LogAndShow(ex);
    }

    private void LogAndShow(Exception ex)
    {
        try { ServiceProvider.GetService<ILogger<App>>()?.LogCritical(ex, "Unhandled exception"); }
        catch { }

        MessageBox.Show(
            $"An unexpected error occurred:\n\n{ex.Message}",
            "Unexpected Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}
