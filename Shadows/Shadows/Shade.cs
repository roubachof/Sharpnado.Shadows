using System.Text;
using Xamarin.Forms;

namespace Sharpnado.Shades
{
    public class Shade : Element
    {
        public static readonly BindableProperty OffsetProperty = BindableProperty.Create(
            nameof(Offset),
            typeof(Point),
            typeof(Shade),
            defaultValueCreator: _ => DefaultOffset);

        public static readonly BindableProperty ColorProperty = BindableProperty.Create(
            nameof(Color),
            typeof(Color),
            typeof(Shade),
            defaultValueCreator: _ => DefaultColor,
            coerceValue: (bo, v) =>
                {
                    var shade = (Shade)bo;
                    var color = (Color)v;
                    if (shade.Opacity < 1 && color.A == 1f)
                    {
                        // Color is plain and a Opacity is set, add Alpha
                        return color.MultiplyAlpha(shade.Opacity);
                    }

                    return color;
                },
            propertyChanged: (bo, oldValue, newValue) =>
                {
                    var shade = (Shade)bo;
                    var color = (Color)newValue;
                    if (color.A < 1 && shade.Opacity == 1f)
                    {
                        // Color has alpha and a Opacity is not set, update Opacity
                        shade.Opacity = color.A;
                    }
                });

        public static readonly BindableProperty OpacityProperty = BindableProperty.Create(
            nameof(Opacity),
            typeof(double),
            typeof(Shade),
            defaultValue: DefaultOpacity,
            propertyChanged: (bo, oldValue, newValue) =>
                {
                    var shade = (Shade)bo;
                    var previousColor = shade.Color;
                    var newOpacity = (double)newValue;
                    shade.Color = new Color(previousColor.R, previousColor.G, previousColor.B, newOpacity);
                });

        public static readonly BindableProperty BlurRadiusProperty = BindableProperty.Create(
            nameof(BlurRadius),
            typeof(double),
            typeof(Shade),
            DefaultBlurRadius);

        private const double DefaultBlurRadius = 12f;

        private const double DefaultOpacity = 0.24f;

        private static readonly Color DefaultColor = new Color(0, 0, 0, DefaultOpacity);

        private static readonly Point DefaultOffset = new Point(0, 8);

        public Point Offset
        {
            get => (Point)GetValue(OffsetProperty);
            set => SetValue(OffsetProperty, value);
        }

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public double Opacity
        {
            get => (double)GetValue(OpacityProperty);
            set => SetValue(OpacityProperty, value);
        }

        public double BlurRadius
        {
            get => (double)GetValue(BlurRadiusProperty);
            set => SetValue(BlurRadiusProperty, value);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Shade {{ Offset: {Offset},");
            builder.Append($"Color: {Color}, Opacity: {Opacity}, BlurRadius: {BlurRadius} }}");

            return builder.ToString();
        }
    }
}
