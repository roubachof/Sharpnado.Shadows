using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Platform;
using UIKit;

namespace Sharpnado.Shades.Platforms.iOS;

/// <summary>
/// Extension methods to convert Shade objects to iOS CALayer.
/// </summary>
internal static class ShadeExtensions
{
    public static CALayer ToCALayer(this Shade shade)
    {
        return new CALayer
        {
            ShadowColor = shade.Color.ToCGColor(),
            ShadowRadius = (nfloat)shade.BlurRadius / UIScreen.MainScreen.Scale,
            ShadowOffset = new CGSize(shade.Offset.X, shade.Offset.Y),
            ShadowOpacity = (float)shade.Opacity,
            MasksToBounds = false,
            RasterizationScale = UIScreen.MainScreen.Scale,
            ShouldRasterize = true,
        };
    }
}
