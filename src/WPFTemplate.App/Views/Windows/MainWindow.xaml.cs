using WPFTemplate.App.ViewModels.Windows;

namespace WPFTemplate.App.Views.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    internal MainWindow(MainWindowViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }

    // ── Title-bar chrome buttons ──────────────────────────────────────────────

    private void OnMinimizeClick(object sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;

    private void OnMaximizeClick(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    private void OnStateChanged(object sender, EventArgs e) =>
        // Swap the icon: restore (E923) when maximized, maximise (E922) when normal
        MaximizeButton.Content = WindowState == WindowState.Maximized ? "\u25A3" : "\u25A1";
}
