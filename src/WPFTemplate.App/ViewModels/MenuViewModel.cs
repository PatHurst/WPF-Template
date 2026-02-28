using WPFTemplate.App.Command;
using WPFTemplate.App.Resources.Theme;
using System.Windows.Shapes;

namespace WPFTemplate.App.ViewModels;

internal class MenuViewModel
{
    /// <summary>
    /// List of commands to set the theme's accent color.
    /// </summary>
    public IEnumerable<CommandViewModel> SetAccentColorCommands =>
        field ??= new List<(string name,Color value)>([
            ("Red",    Color.FromRgb(255, 59,  48)),
            ("Orange", Color.FromRgb(255, 149, 0)),
            ("Yellow", Color.FromRgb(255, 204, 0)),
            ("Green",  Color.FromRgb(52,  199, 89)),
            ("Mint",   Color.FromRgb(0,   199, 190)),
            ("Teal",   Color.FromRgb(48,  176, 199)),
            ("Blue",   Color.FromRgb(0,   122, 255)),
            ("Indigo", Color.FromRgb(88,  86,  214)),
            ("Purple", Color.FromRgb(175, 82,  222)),
            ("Pink",   Color.FromRgb(255, 45,  85)),
        ])
        .Select(color =>
        {
            var icon = new Rectangle()
            {
                Fill = new SolidColorBrush(color.value),
                RadiusX = 2,
                RadiusY = 2,
                Width = 12,
                Height = 12
            };
            return new CommandViewModel(color.name, icon, _ => ThemeManager.SetAccentColor(color.value));
        })
        .ToList();

    /// <summary>
    /// Command to set the application light theme.
    /// </summary>
    public ICommand SetLightThemeCommand => field ??= new RelayCommand(_ => ThemeManager.SetTheme(Theme.Light));

    /// <summary>
    /// Command to set the application light theme.
    /// </summary>
    public ICommand SetDarkThemeCommand => field ??= new RelayCommand(_ => ThemeManager.SetTheme(Theme.Dark));
}
