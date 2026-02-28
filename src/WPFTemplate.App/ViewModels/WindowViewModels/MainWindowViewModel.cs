using System.Timers;

namespace InnoJob.App.ViewModels.WindowViewModels;

internal class MainWindowViewModel : ObservableObject
{
    MainWindowViewModel()
    {
        StatusText = string.Empty;

        var timer = new System.Timers.Timer()
        {
            AutoReset = true,
            Interval = 1_000,
            Enabled = true
        };
        timer.Elapsed += (_, e) => StatusText = e.SignalTime.ToLocalTime().ToString();
    }

    public string StatusText
    {
        get => field;
        set => SetProperty(ref field, value);
    }
}
