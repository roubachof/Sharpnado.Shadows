using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

namespace Sharpnado.Shades.Platforms.Windows;

/// <summary>
/// Extension methods for converting MAUI colors to Windows colors.
/// </summary>
internal static class ColorExtensions
{
    public static Microsoft.UI.Xaml.Media.Brush ToBrush(this Color color)
    {
        return new Microsoft.UI.Xaml.Media.SolidColorBrush(color.ToWindowsColor());
    }

    public static global::Windows.UI.Color ToWindowsColor(this Color color)
    {
        color.ToRgba(out byte r, out byte g, out byte b, out byte a);
        return global::Windows.UI.Color.FromArgb(a, r, g, b);
    }
}
