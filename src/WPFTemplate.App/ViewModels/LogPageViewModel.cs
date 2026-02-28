using Microsoft.Extensions.Logging;
using WPFTemplate.App.Command;
using WPFTemplate.App.Services;

namespace WPFTemplate.App.ViewModels;

internal class LogPageViewModel : ObservableObject
{
    private readonly NavigationService _navigation;
    private readonly ILogger<LogPageViewModel> _logger;

    public LogPageViewModel(NavigationService navigation, ILogger<LogPageViewModel> logger)
    {
        _navigation = navigation;
        _logger = logger;

        LogInfoCommand  = new RelayCommand(_ => Log(LogLevel.Information));
        LogWarnCommand  = new RelayCommand(_ => Log(LogLevel.Warning));
        LogErrorCommand = new RelayCommand(_ => Log(LogLevel.Error));
        GoHomeCommand   = new RelayCommand(_ => _navigation.NavigateTo<HomePageViewModel>());
    }

    public ICommand LogInfoCommand  { get; }
    public ICommand LogWarnCommand  { get; }
    public ICommand LogErrorCommand { get; }
    public ICommand GoHomeCommand   { get; }

    public string Output
    {
        get => field;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    private void Log(LogLevel level)
    {
        var message = $"Sample {level} message from LogPageViewModel";
        _logger.Log(level, "{Message}", message);
        Output += $"[{DateTime.Now:HH:mm:ss}] {level,-12}  {message}{Environment.NewLine}";
    }
}
