using WPFTemplate.App.Command;
using WPFTemplate.App.Services;

namespace WPFTemplate.App.ViewModels;

internal class HomePageViewModel : ObservableObject
{
    private readonly NavigationService _navigation;

    public HomePageViewModel(NavigationService navigation)
    {
        _navigation = navigation;
        GoToLogPageCommand = new RelayCommand(_ => _navigation.NavigateTo<LogPageViewModel>());
    }

    public ICommand GoToLogPageCommand { get; }
}
