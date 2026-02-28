# WPF Template

A batteries-included starting point for WPF desktop applications targeting .NET 10.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Windows (WPF is Windows-only)
- Visual Studio 2022 17.10+ or JetBrains Rider 2024.1+ (required for the `.slnx` solution format)

---

## Using This Template

1. Copy or clone this repo into a new folder with your app name.
2. Rename the three `.csproj` files and the solution file to match your app name.
3. Find-and-replace `WPFTemplate` with your app name across all files (namespaces, csproj metadata).
4. Update the registry key in `src/WPFTemplate.Core/Settings.cs`:
   ```csharp
   private static readonly string _keyName = @"HKEY_CURRENT_USER\SOFTWARE\YourAppName";
   ```
5. Update `Title` in `MainWindow.xaml`.
6. If you need the database layer, add a project reference from `WPFTemplate.App` to `WPFTemplate.Services` and call `Db.Configure(connectionString)` in `App.xaml.cs`.

---

## Solution Structure

```
WPF Template.slnx
└── src/
    ├── WPFTemplate.App          # WPF application (WinExe)
    │   ├── Command/             # RelayCommand, CommandViewModel
    │   ├── Controls/            # Placeholder for custom controls
    │   ├── Resources/Theme/     # ThemeManager, Colors.Light/Dark.xaml, Controls.xaml
    │   ├── Services/            # NavigationService
    │   ├── ViewModels/          # ObservableObject base, page VMs
    │   │   └── WindowViewModels/
    │   └── Views/
    │       ├── Pages/           # Page UserControls (HomePage, LogPage)
    │       └── Windows/         # MainWindow, SplashScreen
    │
    ├── WPFTemplate.Core         # Shared library: Settings, utility functions
    │   └── Functions/
    │
    └── WPFTemplate.Services     # Data access layer
        └── Database/            # Db, DbQuery, DbReader (SQL Server)
```

---

## Architecture

### MVVM

All pages follow the ViewModel-first MVVM pattern:

- **`ObservableObject`** — base class with `SetProperty<T>` and `RaisePropertyChanged`.
- **`RelayCommand`** — `ICommand` implementation that wraps any `Action<object?>`.
- Page ViewModels inherit from `ObservableObject`. They do not reference their views.

### Dependency Injection

Services and ViewModels are registered in `App.xaml.cs` using `Microsoft.Extensions.DependencyInjection`:

```csharp
services.AddSingleton<NavigationService>();
services.AddTransient<MenuViewModel>();
services.AddTransient<HomePageViewModel>();
services.AddTransient<LogPageViewModel>();
services.AddTransient<MainWindowViewModel>();
```

Resolve from anywhere via `App.ServiceProvider.GetRequiredService<T>()`.

> **Note:** `MainWindow` is not registered in DI. It is instantiated manually in `App.xaml.cs`
> to avoid a C# accessibility mismatch between a `public` constructor and `internal` VM types.

### Navigation

Navigation is handled by `NavigationService` (singleton). The main window's content area is a
`ContentControl` whose `Content` property is bound to `NavigationService.CurrentView`.
WPF automatically resolves the correct view via typed `DataTemplate` entries in `App.xaml`.

```
NavigationService.CurrentView  →  DataTemplate lookup  →  View rendered in ContentControl
```

---

## Adding a Page

**1. Create the ViewModel** in `ViewModels/`:

```csharp
// ViewModels/FooPageViewModel.cs
using WPFTemplate.App.Command;
using WPFTemplate.App.Services;

namespace WPFTemplate.App.ViewModels;

internal class FooPageViewModel : ObservableObject
{
    private readonly NavigationService _navigation;

    public FooPageViewModel(NavigationService navigation)
    {
        _navigation = navigation;
        GoHomeCommand = new RelayCommand(_ => _navigation.NavigateTo<HomePageViewModel>());
    }

    public ICommand GoHomeCommand { get; }
}
```

**2. Create the View** in `Views/Pages/`:

```xml
<!-- Views/Pages/FooPage.xaml -->
<UserControl x:Class="WPFTemplate.App.Views.Pages.FooPage" ...>
    <Grid>
        <Button Command="{Binding GoHomeCommand}" Content="← Back" />
    </Grid>
</UserControl>
```

```csharp
// Views/Pages/FooPage.xaml.cs
public partial class FooPage : UserControl
{
    public FooPage() => InitializeComponent();
}
```

**3. Register the DataTemplate** in `App.xaml`:

```xml
<DataTemplate DataType="{x:Type vm:FooPageViewModel}">
    <pages:FooPage />
</DataTemplate>
```

**4. Register in DI** in `App.xaml.cs`:

```csharp
services.AddTransient<FooPageViewModel>();
```

**5. Navigate to it** from any other ViewModel:

```csharp
_navigation.NavigateTo<FooPageViewModel>();
```

---

## Logging

Logging uses `Microsoft.Extensions.Logging` with a Karambolo file sink configured at startup.
Log files are written alongside the executable.

Inject `ILogger<T>` into any ViewModel or service:

```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public void DoWork()
    {
        _logger.LogInformation("Starting work");
        _logger.LogWarning("Something looks off");
        _logger.LogError("Something went wrong");
    }
}
```

See `LogPageViewModel.cs` for a working example inside the navigation system.

---

## Theming

The theme system supports runtime light/dark switching and custom accent colors. All control
styles use `DynamicResource` bindings so changes take effect instantly without restarting.

```csharp
// Switch theme
ThemeManager.SetTheme(Theme.Dark);
ThemeManager.SetTheme(Theme.Light);

// Set accent color
ThemeManager.SetAccentColor(Color.FromRgb(0, 122, 255));

// Change font
ThemeManager.SetFont(new FontFamily("Segoe UI Variable"));
```

The accent system derives four keys automatically from a single color:

| Resource key        | Usage                              |
|---------------------|------------------------------------|
| `AccentColor`       | Base accent (buttons, highlights)  |
| `AccentColorDark`   | Pressed/border states (–20%)       |
| `AccentColorLight`  | Hover states (+20%)                |
| `AccentForeground`  | White or near-black for WCAG AA    |

The current theme and accent color are persisted to the user settings file and restored on next launch.

---

## Settings

There are two distinct layers of configuration.

### User preferences — `Settings` (read/write at runtime)

Stored in `%LocalAppData%\WPFTemplate\appsettings.json`, created automatically on first write.
This is the right place for anything that the user can change at runtime (theme, accent color, font).

```csharp
// Read
int theme   = Settings.AppTheme;
int accent  = Settings.AccentColor;
string font = Settings.FontFamily;

// Write — file is updated immediately
Settings.AppTheme    = (int)Theme.Dark;
Settings.AccentColor = RgbToInt(0, 122, 255);
Settings.FontFamily  = "Consolas";
```

To add a new preference, add a property to `SettingsData` (private nested class in `Settings.cs`)
and a matching public property on `Settings` that reads from and writes to `_data`.

### App configuration — `IConfiguration` (read-only at runtime)

`appsettings.json` in the application directory is loaded at startup via `Microsoft.Extensions.Configuration`
and registered as `IConfiguration` in the DI container. Use this for deployment-time settings that
don't change at runtime, such as the database connection string.

```json
{
  "Database": {
    "ConnectionString": "Server=.\\SQLEXPRESS;Database=MyDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Inject `IConfiguration` into any service to read from it:

```csharp
public class MyService
{
    public MyService(IConfiguration config)
    {
        var connStr = config["Database:ConnectionString"];
    }
}
```

---

## Database (Optional)

`WPFTemplate.Services` provides a SQL Server data access layer built on ADO.NET with a
functional `Either<Error, T>` error model (via LanguageExt) so no exceptions escape the layer.

To activate it:

1. Add a project reference from `WPFTemplate.App` to `WPFTemplate.Services`.
2. Call `Db.Configure(connectionString)` once in `App.xaml.cs` before any queries run:
   ```csharp
   Db.Configure(@"Server=.\SQLEXPRESS;Database=MyDb;Trusted_Connection=True;TrustServerCertificate=True;");
   ```

**Querying:**

```csharp
// Multiple rows
Either<Error, IEnumerable<User>> result = await DbQuery.QueryMany(
    "SELECT Id, Name FROM Users",
    reader => new User(reader.Int("Id"), reader.Str("Name"))
);

// Single row (returns None if not found)
Either<Error, Option<User>> result = await DbQuery.QuerySingle(
    "SELECT Id, Name FROM Users WHERE Id = @id",
    reader => new User(reader.Int("Id"), reader.Str("Name")),
    new SqlParameter("@id", userId)
);

// Execute (INSERT / UPDATE / DELETE)
Either<Error, int> rowsAffected = await DbQuery.Execute(
    "UPDATE Users SET Name = @name WHERE Id = @id",
    new SqlParameter("@name", name),
    new SqlParameter("@id", id)
);
```

`DbReader` provides null-safe typed column accessors: `Str`, `Int`, `Long`, `Bool`, `DateTime`,
`Guid`, `Decimal`, `Double`, `Bytes` — and `Opt*` variants that return `Option<T>` for nullable columns.

---

## NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| `Karambolo.Extensions.Logging.File` | 4.1.0 | File-based logging |
| `LanguageExt.Core` | 5.0.0-beta-77 | Functional types (`Either`, `Option`) |
| `Microsoft.Extensions.Configuration.Json` | 10.0.0 | JSON app config (`appsettings.json`) |
| `Microsoft.Extensions.DependencyInjection` | 10.0.3 | DI container |
| `Microsoft.Data.SqlClient` | 6.1.4 | SQL Server (Services project) |
