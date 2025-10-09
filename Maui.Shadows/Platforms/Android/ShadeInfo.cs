using Android.Content;
using Microsoft.Maui.Platform;
using AndroidColor = Android.Graphics.Color;
using AndroidView = Android.Views.View;

namespace Sharpnado.Shades.Platforms.Android;

/// <summary>
/// Represents the computed information needed to render a shadow bitmap.
/// Contains platform-specific values in pixels.
/// </summary>
internal readonly record struct ShadeInfo
{
    public const int Padding = ShadowView.MaxRadius;

    private ShadeInfo(AndroidColor color, float blurRadius, float offsetX, float offsetY, float cornerRadius, int width, int height)
    {
        Color = color;
        BlurRadius = blurRadius;
        OffsetX = offsetX;
        OffsetY = offsetY;
        CornerRadius = cornerRadius;
        Width = width;
        Height = height;
        Hash = $"{Width}:{Height},{Color},{BlurRadius},{CornerRadius}";
    }

    public AndroidColor Color { get; }
    public float BlurRadius { get; }
    public float OffsetX { get; }
    public float OffsetY { get; }
    public float CornerRadius { get; }
    public int Width { get; }
    public int Height { get; }
    public string Hash { get; }

    public static ShadeInfo FromShade(Context context, Shade shade, float cornerRadius, AndroidView shadowsSource)
    {
        return new ShadeInfo(
            shade.Color.WithAlpha((float)(shade.Color.Alpha * shade.Opacity)).ToPlatform(),
            context.ToPixels(shade.BlurRadius),
            context.ToPixels(shade.Offset.X),
            context.ToPixels(shade.Offset.Y),
            cornerRadius,
            shadowsSource.Width + 2 * Padding,
            shadowsSource.Height + 2 * Padding);
    }

    public override string ToString() =>
        $"ShadeInfo( Offset: {OffsetX};{OffsetY}, Size: {Width}x{Height}, Color: {Color}, BlurRadius: {BlurRadius} )";
}
