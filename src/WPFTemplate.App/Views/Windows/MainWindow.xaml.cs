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
}
