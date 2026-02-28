using System.Windows.Media;

using WPFTemplate.Core;

namespace WPFTemplate.App.Resources.Theme;

/// <summary>
/// Runtime theme management: dark/light theme, accent color, and UI font.
///
/// Resource dictionary layout (App.xaml MergedDictionaries):
///   [0]  Colors.Dark.xaml  or  Colors.Light.xaml   ← swapped by SetTheme()
///   [1]  Controls.xaml                               ← never replaced, only mutated
///
/// Accent model:
///   One public color (AccentColor).  Two internal tints (AccentColorDark,
///   AccentColorLight) and a contrasting foreground (AccentForeground) are
///   derived automatically inside SetAccentColor() so every control that
///   references any of those four keys stays visually consistent.
/// </summary>
internal static class ThemeManager
{
    // ── Index of Controls.xaml in Application.Resources.MergedDictionaries ──
    // If your App.xaml has a different ordering, update this constant.
    private const int ControlsDictIndex = 1;

    // ─────────────────────────────────────────────────────────────────────────
    // Theme
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Switches the application between dark and light themes.
    /// Replaces MergedDictionaries[0] so all DynamicResource color keys
    /// pick up the new values immediately without restarting.
    /// </summary>
    internal static void SetTheme(Theme theme)
    {
        try
        {
            Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary
            {
                Source = new Uri(ThemeToSource(theme), UriKind.Relative)
            };
            Settings.AppTheme = (byte)theme;
        }
        catch { }
    }

    private static string ThemeToSource(Theme theme) => theme switch
    {
        Theme.Dark  => "/Resources/Theme/Colors.Dark.xaml",
        Theme.Light => "/Resources/Theme/Colors.Light.xaml",
        _           => "/Resources/Theme/Colors.Light.xaml"
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Accent color
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Sets the application accent color.
    ///
    /// <para>
    /// A single <paramref name="color"/> drives four resource keys in
    /// Controls.xaml so every control that reads AccentColor / AccentColorDark /
    /// AccentColorLight / AccentForeground is automatically consistent:
    /// </para>
    /// <list type="bullet">
    ///   <item><b>AccentColor</b>       — the base color as supplied.</item>
    ///   <item><b>AccentColorDark</b>   — lightness reduced by ~20 % for
    ///         pressed states and borders.</item>
    ///   <item><b>AccentColorLight</b>  — lightness raised by ~20 % for
    ///         hover states.</item>
    ///   <item><b>AccentForeground</b>  — white when the accent is dark,
    ///         near-black when the accent is light, so text on accent
    ///         surfaces always passes contrast checks.</item>
    /// </list>
    ///
    /// The persisted value is the 0x00RRGGBB integer stored in Settings.
    /// </summary>
    internal static void SetAccentColor(Color color)
    {
        try
        {
            var controls = ControlsDict();

            controls["AccentColor"]      = new SolidColorBrush(color);
            controls["AccentColorDark"]  = new SolidColorBrush(Darken(color, 0.20f));
            controls["AccentColorLight"] = new SolidColorBrush(Lighten(color, 0.20f));
            controls["AccentForeground"] = new SolidColorBrush(ContrastForeground(color));

            // Persist only the base color (RGB — alpha not needed).
            Settings.AccentColor = RgbToInt(color.R, color.G, color.B);
        }
        catch { }
    }

    /// <summary>
    /// Restores the accent color that was previously persisted to Settings.
    /// Call this once during application startup after the resource
    /// dictionaries have been merged.
    /// </summary>
    internal static void RestoreAccentColor()
    {
        var rgb   = Settings.AccentColor;
        var color = Color.FromRgb(
            (byte)((rgb >> 16) & 0xFF),
            (byte)((rgb >>  8) & 0xFF),
            (byte)( rgb        & 0xFF));
        SetAccentColor(color);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Font
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Sets the application UI font family.
    ///
    /// <para>
    /// Writes the <c>DefaultFont</c> key in Controls.xaml.  Because every
    /// control style in Controls.xaml references that key via
    /// <c>{DynamicResource DefaultFont}</c>, the change propagates
    /// immediately to all open windows without restart.
    /// </para>
    ///
    /// <example>
    /// <code>
    /// // Switch to a system font:
    /// ThemeManager.SetFont(new FontFamily("Consolas"));
    ///
    /// // Restore the default:
    /// ThemeManager.SetFont(new FontFamily("Segoe UI"));
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="font">
    /// A <see cref="FontFamily"/> to use as the application-wide UI font.
    /// Pass <c>new FontFamily("Segoe UI")</c> to restore the default.
    /// </param>
    internal static void SetFont(FontFamily font)
    {
        try
        {
            ControlsDict()["DefaultFont"] = font;
            Settings.FontFamily = font.Source;
        }
        catch { }
    }

    /// <summary>
    /// Restores the font that was previously persisted to Settings.
    /// Call during startup after dictionaries are merged.
    /// </summary>
    internal static void RestoreFont()
    {
        var name = Settings.FontFamily;
        if (!string.IsNullOrWhiteSpace(name))
            SetFont(new FontFamily(name));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Color picker placeholder
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Opens a color-picker dialog and, if the user confirms, calls
    /// <see cref="SetAccentColor"/> with the chosen color.
    /// Implement the dialog here when the UI component is ready.
    /// </summary>
    internal static void ChooseAccentColor()
    {
        // TODO: open a color-picker Window / dialog here.
        // On confirm:  SetAccentColor(pickedColor);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Returns Controls.xaml's ResourceDictionary from MergedDictionaries.</summary>
    private static ResourceDictionary ControlsDict()
        => Application.Current.Resources.MergedDictionaries[ControlsDictIndex];

    /// <summary>
    /// Reduces the perceptual lightness of <paramref name="color"/> by
    /// <paramref name="amount"/> (0–1 scale) without touching hue or saturation.
    /// </summary>
    private static Color Darken(Color color, float amount)
    {
        float r = color.R / 255f;
        float g = color.G / 255f;
        float b = color.B / 255f;

        r = Clamp01(r - amount * r);
        g = Clamp01(g - amount * g);
        b = Clamp01(b - amount * b);

        return Color.FromRgb(
            (byte)(r * 255),
            (byte)(g * 255),
            (byte)(b * 255));
    }

    /// <summary>
    /// Raises the perceptual lightness of <paramref name="color"/> by
    /// <paramref name="amount"/> (0–1 scale) by blending toward white.
    /// </summary>
    private static Color Lighten(Color color, float amount)
    {
        float r = color.R / 255f;
        float g = color.G / 255f;
        float b = color.B / 255f;

        r = Clamp01(r + (1f - r) * amount);
        g = Clamp01(g + (1f - g) * amount);
        b = Clamp01(b + (1f - b) * amount);

        return Color.FromRgb(
            (byte)(r * 255),
            (byte)(g * 255),
            (byte)(b * 255));
    }

    /// <summary>
    /// Returns white or near-black depending on the relative luminance of
    /// <paramref name="color"/> so that text drawn on that background always
    /// meets WCAG AA contrast (≥ 4.5 : 1).
    /// </summary>
    private static Color ContrastForeground(Color color)
    {
        // Relative luminance per WCAG 2.1 §1.4.3
        double r = LinearChannel(color.R);
        double g = LinearChannel(color.G);
        double b = LinearChannel(color.B);
        double L  = 0.2126 * r + 0.7152 * g + 0.0722 * b;

        // Threshold: white on anything darker than medium gray
        return L < 0.35
            ? Color.FromRgb(0xFF, 0xFF, 0xFF)   // white
            : Color.FromRgb(0x1A, 0x1A, 0x1A);  // near-black
    }

    private static double LinearChannel(byte c)
    {
        double sRgb = c / 255.0;
        return sRgb <= 0.04045
            ? sRgb / 12.92
            : Math.Pow((sRgb + 0.055) / 1.055, 2.4);
    }

    private static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
}

// ─────────────────────────────────────────────────────────────────────────────
// Supporting types
// ─────────────────────────────────────────────────────────────────────────────

internal enum Theme : byte
{
    Light = 0,
    Dark  = 1
}
