using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WPFTemplate.App;

internal static class Settings
{
    private static readonly string _settingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WPFTemplate");

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private static readonly string _settingsFile = Path.Combine(_settingsDir, "appsettings.json");

    private static SettingsData _data = Load();

    public static int AppTheme
    {
        get => _data.AppTheme;
        set
        {
            _data.AppTheme = value;
            Save();
        }
    }

    public static int AccentColor
    {
        get => _data.AccentColor;
        set
        {
            _data.AccentColor = value;
            Save();
        }
    }

    public static string FontFamily
    {
        get => _data.FontFamily;
        set
        {
            _data.FontFamily = value;
            Save();
        }
    }

    private static SettingsData Load()
    {
        try
        {
            if (File.Exists(_settingsFile))
            {
                var json = File.ReadAllText(_settingsFile);
                return JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();
            }
        }
        catch (Exception ex)
        {
            App.ServiceProvider.GetService<ILogger<App>>()?.LogError(ex, "Settings could not be loaded");
        }

        return new SettingsData();
    }

    private static void Save()
    {
        try
        {
            Directory.CreateDirectory(_settingsDir);

            var json = JsonSerializer.Serialize(_data, _jsonSerializerOptions);

            File.WriteAllText(_settingsFile, json);
        }
        catch (Exception ex)
        {
            App.ServiceProvider.GetService<ILogger<App>>()?.LogError(ex, "Settings could not be saved");
        }
    }

    private sealed class SettingsData
    {
        [JsonPropertyName("appTheme")]
        public int AppTheme { get; set; }

        [JsonPropertyName("accentColor")]
        public int AccentColor { get; set; }

        [JsonPropertyName("fontFamily")]
        public string FontFamily { get; set; } = string.Empty;
    }
}
