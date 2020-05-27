using System.Text;
using CoreAnimation;

namespace Sharpnado.Shades.iOS
{
    public static class CALayerExtensions
    {
        public static void LogInfo(this CALayer layer)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"CALayer( bounds: {layer.Bounds}");
            builder.AppendLine($"    Offset: {layer.ShadowOffset},");
            builder.AppendLine(
                $"    Color: {{ R:{layer.ShadowColor.Components[0]}, G:{layer.ShadowColor.Components[1]}, B:{layer.ShadowColor.Components[1]}, Alpha:{layer.ShadowColor.Alpha} }}, Opacity: {layer.ShadowOpacity},");
            builder.Append($"    ShadowRadius: {layer.ShadowRadius} )");

            var result = builder.ToString();

            InternalLogger.Debug(result);
        }
    }
}