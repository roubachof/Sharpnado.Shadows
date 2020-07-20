using System;

using CoreAnimation;

using CoreGraphics;

using UIKit;

using Xamarin.Forms.Platform.iOS;

namespace Sharpnado.Shades.iOS
{
    public static class ShadeExtensions
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
}