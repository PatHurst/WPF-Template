namespace WPFTemplate.App.Services;

internal interface IWindowService
{
    bool? ShowDialog<TWindow>() where TWindow : Window;
}
