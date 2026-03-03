using System.Windows.Shapes;

using WPFTemplate.App.Command;
using WPFTemplate.App.Services;
using WPFTemplate.App.ViewModels.Pages;
using WPFTemplate.App.Views.Windows;

namespace WPFTemplate.App.ViewModels.Windows;

internal class MenuViewModel
{
    private readonly NavigationService _navigation;

    public MenuViewModel(NavigationService navigation) => _navigation = navigation;

    public ICommand ExitCommand =>
        field ??= new RelayCommand(_ => Application.Current.Shutdown());

    public ICommand NavigateHomeCommand =>
        field ??= new RelayCommand(_ => _navigation.NavigateTo<HomePageViewModel>());

    public ICommand NavigateLogCommand =>
        field ??= new RelayCommand(_ => _navigation.NavigateTo<LogPageViewModel>());

    public ICommand ShowAboutCommand =>
        field ??= new RelayCommand(_ =>
            new AboutWindow { Owner = Application.Current.MainWindow }.ShowDialog());

    public IEnumerable<CommandViewModel> SetThemeCommands =>
        field ??= BuildThemeCommands();

    private static List<CommandViewModel> BuildThemeCommands()
    {
        List<CommandViewModel> cmds = [];

        foreach (var (name, theme) in new (string, Theme)[]
        {
            ("Light",    Theme.Light),
            ("Dark",     Theme.Dark),
            ("Classic",  Theme.Classic),
            ("Fluent",   Theme.Fluent),
            ("Eridanus", Theme.Eridanus),
        })
        {
            var t = theme;
            CommandViewModel cmd = null!;
            cmd = new CommandViewModel(name, _ =>
            {
                ThemeManager.SetTheme(t);
                foreach (var c in cmds)
                    c.IsActive = false;
                cmd.IsActive = true;
            });
            cmd.IsActive = (Theme)Settings.AppTheme == t;
            cmds.Add(cmd);
        }

        return cmds;
    }


    private List<CommandViewModel>? _setAccentColorCommands;
    public IEnumerable<CommandViewModel> SetAccentColorCommands => _setAccentColorCommands ??= BuildAccentColorCommands();

    private static List<CommandViewModel> BuildAccentColorCommands()
    {
        List<CommandViewModel> cmds = [];

        foreach (var (name, color) in new (string, Color)[]
        {
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
        })
        {
            var c = color;
            var icon = new Rectangle
            {
                Fill = new SolidColorBrush(c),
                RadiusX = 2,
                RadiusY = 2,
                Width = 12,
                Height = 12
            };

            CommandViewModel cmd = null!;
            cmd = new CommandViewModel(name, icon, _ =>
            {
                ThemeManager.SetAccentColor(c);
                foreach (var x in cmds)
                    x.IsActive = false;
                cmd.IsActive = true;
            });
            cmd.IsActive = RgbToInt(c.R, c.G, c.B) == Settings.AccentColor;
            cmds.Add(cmd);
        }

        return cmds;
    }

    // ── Settings / Font ───────────────────────────────────────────────────────

    private List<CommandViewModel>? _setFontCommands;
    public IEnumerable<CommandViewModel> SetFontCommands => _setFontCommands ??= BuildFontCommands();

    private List<CommandViewModel> BuildFontCommands()
    {
        List<CommandViewModel> cmds = [];

        var currentFont = string.IsNullOrEmpty(Settings.FontFamily) ? "Segoe UI" : Settings.FontFamily;

        foreach (var name in new[]
        {
            "Segoe UI",
            "Segoe UI Variable",
            "Calibri",
            "Arial",
            "Verdana",
            "Consolas",
            "Cascadia Code",
            "Courier New",
        })
        {
            var n = name;
            CommandViewModel cmd = null!;
            cmd = new CommandViewModel(n, _ =>
            {
                ThemeManager.SetFont(new FontFamily(n));
                foreach (var c in cmds)
                    c.IsActive = false;
                cmd.IsActive = true;
            });
            cmd.IsActive = n.Equals(currentFont, StringComparison.OrdinalIgnoreCase);
            cmds.Add(cmd);
        }

        return cmds;
    }
}
