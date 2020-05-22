using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sharpnado.Shades
{
    [AcceptEmptyServiceProvider]
    public class SingleShadeExtension : IMarkupExtension<ReadOnlyCollection<Shade>>
    {
        public Point Offset { get; set; } = (Point)Shade.OffsetProperty.DefaultValue;

        public Color Color { get; set; } = (Color)Shade.ColorProperty.DefaultValue;

        public double Opacity { get; set; } = (double)Shade.OpacityProperty.DefaultValue;

        public double BlurRadius { get; set; } = (double)Shade.BlurRadiusProperty.DefaultValue;

        public ReadOnlyCollection<Shade> ProvideValue(IServiceProvider serviceProvider)
        {
            return new ReadOnlyCollection<Shade>(
                new[] { new Shade { Offset = Offset, Color = Color, Opacity = Opacity, BlurRadius = BlurRadius } });
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return (this as IMarkupExtension<IReadOnlyCollection<Shade>>).ProvideValue(serviceProvider);
        }
    }
}