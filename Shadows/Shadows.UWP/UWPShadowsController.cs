using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;

using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

using Xamarin.Forms.Internals;

using Rectangle = Windows.UI.Xaml.Shapes.Rectangle;

namespace Sharpnado.Shades.UWP
{
 public class UWPShadowsController
    {
        private const string LogTag = nameof(UWPShadowsController);

        private const float SafeMargin = 1;

        private readonly Canvas _shadowsCanvas;

        private readonly FrameworkElement _shadowSource;

        private readonly List<SpriteVisual> _shadowVisuals = new List<SpriteVisual>();

        private float _cornerRadius;
        private IEnumerable<Shade> _shadesSource;

        private Compositor _compositor;


        public UWPShadowsController(Canvas shadowCanvas, FrameworkElement shadowSource, float cornerRadius)
        {

            _shadowsCanvas = shadowCanvas;

            _shadowSource = shadowSource;

            _cornerRadius = cornerRadius;

            _shadowSource.SizeChanged += ShadowSourceSizeChanged;
        }

        public void DestroyShadow(int shadowIndex)
        {
            try
            {
                InternalLogger.Debug(LogTag, $"DestroyShadow( shadowIndex: {shadowIndex} )");

                var shadowHost = _shadowsCanvas.Children[shadowIndex];
                var visual = _shadowVisuals[shadowIndex];
                ElementCompositionPreview.SetElementChildVisual(shadowHost, null);
                _shadowsCanvas.Children.RemoveAt(shadowIndex);
                _shadowVisuals.RemoveAt(shadowIndex);
                visual.Dispose();
            }
            catch { }
        }

        public void DestroyShadows()
        {
            InternalLogger.Debug(LogTag, "DestroyShadows()");
            for (int i = _shadowsCanvas.Children.Count - 1; i >= 0; i++)
            {
                DestroyShadow(i);
            }
        }

        public void UpdateCornerRadius(float cornerRadius)
        {
            if (_shadowsCanvas == null && _shadowSource == null)
            {
                return;
            }

            InternalLogger.Debug(LogTag, () => $"UpdateCornerRadius( cornerRadius: {cornerRadius} )");
            bool hasChanged = _cornerRadius != cornerRadius;
            _cornerRadius = cornerRadius;

            if (hasChanged && _shadesSource.Any())
            {
                for (int i = 0; i < _shadowsCanvas.Children.Count; i++)
                {
                    var shadowHost = (Rectangle)_shadowsCanvas.Children[i];
                    shadowHost.RadiusX = cornerRadius;
                    shadowHost.RadiusY = cornerRadius;

                    var shadowVisual = _shadowVisuals[i];
                    ((DropShadow)shadowVisual.Shadow).Mask = shadowHost.GetAlphaMask();
                }
            }
        }

        public void UpdateShades(IEnumerable<Shade> shadesSource)
        {
            if (shadesSource == null)
            {
                return;
            }

            InternalLogger.Debug(LogTag, () => $"UpdateShades( shadesSource: {shadesSource} )");

            if (_shadesSource is INotifyCollectionChanged previousNotifyCollectionChanged)
            {
                previousNotifyCollectionChanged.CollectionChanged -= ShadesSourceCollectionChanged;
            }

            _shadesSource = shadesSource;
            if (_shadesSource is INotifyCollectionChanged notifyCollectionChanged)
            {
                notifyCollectionChanged.CollectionChanged += ShadesSourceCollectionChanged;
            }

            DestroyShadows();
            for (int i = 0; i < _shadesSource.Count(); i++)
            {
                InsertShade(i, _shadesSource.ElementAt(i));
            }
        }

        private void ShadesSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0, insertIndex = e.NewStartingIndex; i < e.NewItems.Count; i++)
                    {
                        InsertShade(insertIndex, (Shade)e.NewItems[i]);
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0, removedIndex = e.OldStartingIndex; i < e.OldItems.Count; i++)
                    {
                        RemoveShade(removedIndex, (Shade)e.OldItems[i]);
                    }

                    break;

                case NotifyCollectionChangedAction.Reset:
                    DestroyShadows();
                    break;
            }
        }

        private void InsertShade(int insertIndex, Shade shade)
        {
            shade.PropertyChanged -= ShadePropertyChanged;
            InternalLogger.Debug(LogTag, () => $"InsertShade( insertIndex: {insertIndex}, shade: {shade} )");

            // https://docs.microsoft.com/en-US/windows/uwp/composition/using-the-visual-layer-with-xaml

            var ttv = _shadowSource.TransformToVisual(_shadowsCanvas);
            Windows.Foundation.Point offset = ttv.TransformPoint(new Windows.Foundation.Point(0, 0));

            double width = _shadowSource.ActualWidth;
            double height = _shadowSource.ActualHeight;

            var shadowHost = new Rectangle()
            {
                Fill = Xamarin.Forms.Color.White.ToBrush(),
                Width = width,
                Height = height,
                RadiusX = _cornerRadius,
                RadiusY = _cornerRadius,
            };

                Canvas.SetLeft(shadowHost, offset.X);
                Canvas.SetTop(shadowHost, offset.Y);

            _shadowsCanvas.Children.Insert(insertIndex, shadowHost);

            if (_compositor == null)
            {
                Visual hostVisual = ElementCompositionPreview.GetElementVisual(_shadowsCanvas);
                _compositor = hostVisual.Compositor;
            }

            var dropShadow = _compositor.CreateDropShadow();
            dropShadow.BlurRadius = (float)shade.BlurRadius * 2;
            dropShadow.Opacity = 1;
            dropShadow.Color = shade.Color.ToWindowsColor();
            dropShadow.Offset = new Vector3((float)shade.Offset.X - SafeMargin, (float)shade.Offset.Y - SafeMargin, 0);
            dropShadow.Mask = shadowHost.GetAlphaMask();

            var shadowVisual = _compositor.CreateSpriteVisual();
            shadowVisual.Size = new Vector2((float)width, (float)height);
            shadowVisual.Shadow = dropShadow;

            _shadowVisuals.Insert(insertIndex, shadowVisual);

            ElementCompositionPreview.SetElementChildVisual(shadowHost, shadowVisual);
            shade.PropertyChanged += ShadePropertyChanged;
        }

        private void ShadowSourceSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var ttv = _shadowSource.TransformToVisual(_shadowsCanvas);
            Windows.Foundation.Point offset = ttv.TransformPoint(new Windows.Foundation.Point(0, 0));
            double width = _shadowSource.ActualWidth;
            double height = _shadowSource.ActualHeight;


            if (width < 1 || height < 1)
            {
                return;
            }

            //InternalLogger.Debug(
            //    LogTag,
            //    $"shadowSource: {{ ActualOffset: {_shadowSource.ActualOffset}, ActualSize: {_shadowSource.ActualSize}, Margin: {_shadowSource.Margin} }}");

            for (int i = 0; i < _shadesSource.Count(); i++)
            {
                var shadowHost = (Rectangle)_shadowsCanvas.Children[i];
                var shadowVisual = _shadowVisuals[i];

                //InternalLogger.Debug(
                //    LogTag,
                //    $"shadowHost: {{ ActualOffset: {shadowHost.ActualOffset}, ActualSize: {shadowHost.ActualSize}, Margin: {shadowHost.Margin} }}");
                Canvas.SetLeft(shadowHost, offset.X + SafeMargin);
                Canvas.SetTop(shadowHost, offset.Y + SafeMargin);


                double newWidth = width - 2 * SafeMargin;
                double newHeight = height - 2 * SafeMargin;

                shadowHost.Width = newWidth;
                shadowHost.Height = newHeight;

                shadowVisual.Size = new Vector2((float)width, (float)height);
            }
        }


        private void RemoveShade(int removedIndex, Shade shade)
        {
            InternalLogger.Debug(LogTag, () => $"RemoveShade( insertIndex: {removedIndex} )");
            shade.PropertyChanged -= ShadePropertyChanged;
            DestroyShadow(removedIndex);
        }

        private void ShadePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var shade = (Shade)sender;
            var index = _shadesSource.IndexOf(shade);
            if (index < 0)
            {
                InternalLogger.Warn(LogTag, $"ShadePropertyChanged => shade property {e.PropertyName} changed but we can't find the shade in the source");
                return;
            }

            InternalLogger.Debug(LogTag, () => $"ShadePropertyChanged( shadeIndex: {index}, propertyName: {e.PropertyName} )");
            switch (e.PropertyName)
            {
                case nameof(Shade.BlurRadius):
                case nameof(Shade.Color):
                case nameof(Shade.Offset):
                    UpdateShadeVisual(index, shade);
                    break;
            }
        }

        private void UpdateShadeVisual(int index, Shade shade)
        {
            var dropShadow = (DropShadow)_shadowVisuals[index].Shadow;
            dropShadow.BlurRadius = (float)shade.BlurRadius;
            dropShadow.Opacity = 1;
            dropShadow.Color = shade.Color.ToWindowsColor();
            dropShadow.Offset = new Vector3((float)shade.Offset.X, (float)shade.Offset.Y, 0);
        }
    }

    internal static class ColorExtensions
    {
        public static Brush ToBrush(this Xamarin.Forms.Color color)
        {
            return new SolidColorBrush(color.ToWindowsColor());
        }

        public static Windows.UI.Color ToWindowsColor(this Xamarin.Forms.Color color)
        {
            return Windows.UI.Color.FromArgb((byte)(color.A * 255), (byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255));
        }
    }

}
