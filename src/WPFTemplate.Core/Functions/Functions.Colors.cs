namespace WPFTemplate.Core.Functions;

public partial class Functions
{
    /// <summary>
    /// Extract the RGB values from an <see cref="int"/>
    /// </summary>
    /// <param name="value">The integer value.</param>
    /// <returns>The RGB values in a tuple of <see cref="int"/>s</returns>
    public static (byte r, byte g, byte b) IntToRgb(int value) =>
            ((byte)(value >>> 16), (byte)((value >>> 8) & 255), (byte)(value & 255));

    /// <summary>
    /// Convert RGB values to their integer representation.
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <returns>An <see cref="int"/> of the combined values in the order RGB.</returns>
    public static int RgbToInt(byte r, byte g, byte b) =>
        (r << 16) | (g << 8) | b;
}
