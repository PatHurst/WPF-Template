using Microsoft.Win32;

public static class Settings
{
    private static readonly string _keyName = @"HKEY_CURRENT_USER\SOFTWARE\InnoJob";

    public static int AppTheme
    {
        get => GetRegistryValue(nameof(AppTheme), 0);
        set => SetRegistryValue(nameof(AppTheme), value);
    }

    public static int AccentColor
    {
        get => GetRegistryValue(nameof(AccentColor), 0);
        set => SetRegistryValue(nameof(AccentColor), value);
    }

    public static string FontFamily
    {
        get => GetRegistryValue(nameof(FontFamily), string.Empty);
        set => SetRegistryValue<string>(nameof(FontFamily), value);
    }

    private static T GetRegistryValue<T>(string valueName, T defaultValue)
    {
        try
        {
            var read = Registry.GetValue(_keyName, valueName, null);

            if (read is T result)
                return result;

            // Value missing — persist the default and return it
            SetRegistryValue(valueName, defaultValue);
            return defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    private static void SetRegistryValue<T>(string valueName, T value)
    {
        try
        {
            switch (value)
            {
                case int:
                case byte:
                case short:
                    Registry.SetValue(_keyName, valueName, value!, RegistryValueKind.DWord);
                    break;
                default:
                    Registry.SetValue(_keyName, valueName, value!, RegistryValueKind.String);
                    break;
            }
        }
        catch { }
    }
}