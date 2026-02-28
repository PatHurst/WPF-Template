using System.ComponentModel;

using Microsoft.Extensions.DependencyInjection;

namespace WPFTemplate.App.Services;

/// <summary>
/// Manages page navigation by swapping the active ViewModel.
/// Bind <see cref="CurrentView"/> to a <c>ContentControl</c> and add a
/// <c>DataTemplate</c> per page ViewModel in App.xaml to wire up the views.
/// </summary>
internal class NavigationService : INotifyPropertyChanged
{
    private readonly IServiceProvider _services;
    private object? _currentView;

    internal NavigationService(IServiceProvider services)
    {
        _services = services;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// The currently active page ViewModel. Bind a <c>ContentControl.Content</c> to this.
    /// </summary>
    public object? CurrentView
    {
        get => _currentView;
        private set
        {
            if (_currentView == value) return;
            _currentView = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentView)));
        }
    }

    /// <summary>
    /// Resolve <typeparamref name="TViewModel"/> from DI and display it.
    /// </summary>
    internal void NavigateTo<TViewModel>() where TViewModel : notnull
    {
        CurrentView = _services.GetRequiredService<TViewModel>();
    }
}
