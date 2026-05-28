using System.ComponentModel;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;

using WPFTemplate.App.ViewModels.Base;

namespace WPFTemplate.App.Services;

/// <summary>
/// Manages page navigation by swapping the active ViewModel.
/// Bind <see cref="CurrentView"/> to a <c>ContentControl</c> and add a
/// <c>DataTemplate</c> per page ViewModel in App.xaml to wire up the views.
/// </summary>
internal class NavigationService : ObservableObject
{
    private readonly IServiceProvider _services;

    public NavigationService(IServiceProvider services)
    {
        _services = services;
    }

    /// <summary>
    /// The currently active page ViewModel. Bind a <c>ContentControl.Content</c> to this.
    /// </summary>
    public object? CurrentView
    {
        get => field;
        private set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Resolve <typeparamref name="TViewModel"/> from DI and display it.
    /// </summary>
    internal void NavigateTo<TViewModel>() where TViewModel : notnull
    {
        CurrentView = _services.GetRequiredService<TViewModel>();
    }
}
