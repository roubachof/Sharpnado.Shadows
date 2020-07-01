using SkiaSharp;

namespace Shadows.Tizen
{
    public static class ColorExtensions
    {
        public static SKColor ToSK(this Xamarin.Forms.Color color)
        {
            return new SKColor(
                        (byte)(color.R * 255),
                        (byte)(color.G * 255),
                        (byte)(color.B * 255),
                        (byte)(color.A * 255));
        }
    }
}
