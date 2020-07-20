using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SkiaSharp;
using SkiaSharp.Views.Tizen;
using ElmSharp;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Tizen;
using Xamarin.Forms.Platform.Tizen.Native;
using Sharpnado.Shades.Tizen;
using Sharpnado.Shades;
using Shadows.Tizen;
using Layout = Xamarin.Forms.Layout;

[assembly: ExportRenderer(typeof(Sharpnado.Shades.Shadows), typeof(TizenShadowsRenderer))]

namespace Sharpnado.Shades.Tizen
{
    public class TizenShadowsRenderer : LayoutRenderer
    {
        const string LogTag = "TizenShadowsController";

        SKCanvasView _canvasView;
        IEnumerable<Shade> _shadesSource;
        EvasObject ContentNativeView => Platform.GetOrCreateRenderer(ShadowsElement.Content)?.NativeView;
        Shadows ShadowsElement => Element as Shadows;

        public static void Initialize()
        {
            _ = typeof(TizenShadowsRenderer);
        }


        public TizenShadowsRenderer() : base()
        {
            RegisterPropertyHandler(Shadows.ShadesProperty, UpdateShades);
            RegisterPropertyHandler(Shadows.CornerRadiusProperty, UpdateCornerRadius);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Layout> e)
        {
            if (Control == null)
            {
                SetNativeControl(new Canvas(Forms.NativeParent));
                _canvasView = new SKCanvasView(Forms.NativeParent);
                _canvasView.PaintSurface += OnPaintSurface;
                _canvasView.Show();
                Control.Children.Add(_canvasView);
                Interop.evas_object_clip_unset(_canvasView);
                Control.LayoutUpdated += OnLayoutUpdated;

                if (ShadowsElement.Shades is INotifyCollectionChanged items)
                {
                    items.CollectionChanged += OnShadesCollectionChanged;
                }
                _shadesSource = ShadowsElement.Shades;
            }
            base.OnElementChanged(e);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (_shadesSource is INotifyCollectionChanged items)
                {
                    items.CollectionChanged -= OnShadesCollectionChanged;
                    for (int i = 0; i < _shadesSource.Count(); i++)
                    {
                        ((Shade)_shadesSource.ElementAt(i)).PropertyChanged -= ShadePropertyChanged;
                    }
                }
                if (Control != null)
                {
                    Control.LayoutUpdated -= OnLayoutUpdated;
                }
                if (_canvasView != null)
                {
                    _canvasView.PaintSurface -= OnPaintSurface;
                    _canvasView.Unrealize();
                    _canvasView = null;
                }
            }
        }

        void OnShadesCollectionChanged(object sender, EventArgs eventArgs)
        {
            UpdateGeometry();
        }

        void OnLayoutUpdated(object sender, LayoutEventArgs e)
        {
            UpdateGeometry();
        }

        void UpdateGeometry()
        {
            var geometry = ContentNativeView == null ? NativeView.Geometry : ContentNativeView.Geometry;

            double left = 0;
            double top = 0;
            double right = 0;
            double bottom = 0;

            foreach (var shadow in _shadesSource)
            {
                var scaledOffsetX = Forms.ConvertToScaledPixel(shadow.Offset.X);
                var scaledOffsetY = Forms.ConvertToScaledPixel(shadow.Offset.Y);
                var scaledBlurRadius = Forms.ConvertToScaledPixel(shadow.BlurRadius);
                var spreadSize = scaledBlurRadius * 2 + scaledBlurRadius;
                var sl = scaledOffsetX - spreadSize;
                var sr = scaledOffsetX + spreadSize;
                var st = scaledOffsetY - spreadSize;
                var sb = scaledOffsetY + spreadSize;
                if (left > sl) left = sl;
                if (top > st) top = st;
                if (right < sr) right = sr;
                if (bottom < sb) bottom = sb;
            }

            var canvasGeometry = new Rect(
                geometry.X + (int)left,
                geometry.Y + (int)top,
                geometry.Width + (int)right - (int)left,
                geometry.Height + (int)bottom - (int)top);
            if (_canvasView != null)
            {
                _canvasView.Geometry = canvasGeometry;
                _canvasView.Invalidate();
            }
        }

        void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var geometry = ContentNativeView == null ? NativeView.Geometry : ContentNativeView.Geometry;
            var canvas = e.Surface.Canvas;
            canvas.Clear();

            var path = new SKPath();
            var left = geometry.Left - _canvasView.Geometry.Left;
            var top = geometry.Top - _canvasView.Geometry.Top;
            var rect = new SKRect(left, top, left + geometry.Width, top + geometry.Height);
            var scaledRadius = Forms.ConvertToScaledPixel(ShadowsElement.CornerRadius) * 2;
            var topLeft = new SKRect(rect.Left, rect.Top, rect.Left + scaledRadius, rect.Top + scaledRadius);
            var topRight = new SKRect(rect.Right - scaledRadius, rect.Top, rect.Right, rect.Top + scaledRadius);
            var bottomLeft = new SKRect(rect.Left, rect.Bottom - scaledRadius, rect.Left + scaledRadius, rect.Bottom);
            var bottomRight = new SKRect(rect.Right - scaledRadius, rect.Bottom - scaledRadius, rect.Right, rect.Bottom);
            path.ArcTo(topLeft, 180, 90, false);
            path.ArcTo(topRight, 270, 90, false);
            path.ArcTo(bottomRight, 0, 90, false);
            path.ArcTo(bottomLeft, 90, 90, false);
            path.Close();

            using (var paint = new SKPaint())
            {
                paint.IsAntialias = true;
                paint.Style = SKPaintStyle.StrokeAndFill;
                foreach (var shade in _shadesSource)
                {
                    var scaledOffsetX = Forms.ConvertToScaledPixel(shade.Offset.X);
                    var scaledOffsetY = Forms.ConvertToScaledPixel(shade.Offset.Y);
                    var scaledBlurRadius = Forms.ConvertToScaledPixel(shade.BlurRadius);

                    canvas.Save();
                    canvas.ClipPath(path, SKClipOperation.Difference, true);
                    paint.ImageFilter = SKImageFilter.CreateDropShadow(
                        scaledOffsetX,
                        scaledOffsetY,
                        scaledBlurRadius,
                        scaledBlurRadius,
                        shade.Color.MultiplyAlpha(shade.Opacity).ToSK(),
                        SKDropShadowImageFilterShadowMode.DrawShadowOnly);
                    canvas.DrawPath(path, paint);
                    canvas.Restore();

                    canvas.Save();
                    canvas.ClipPath(path, SKClipOperation.Intersect, true);
                    canvas.DrawPath(path, paint);
                    canvas.Restore();
                }
            }
        }

        void UpdateShades(bool initialize)
        {
            if (initialize && ShadowsElement.Shades.ToList().Count == 0)
                return;

            for (int i = 0; i < _shadesSource.Count(); i++)
            {
                ((Shade)_shadesSource.ElementAt(i)).PropertyChanged -= ShadePropertyChanged;
            }

            _shadesSource = ShadowsElement.Shades;

            for (int i = 0; i < _shadesSource.Count(); i++)
            {
                ((Shade)_shadesSource.ElementAt(i)).PropertyChanged += ShadePropertyChanged;
            }

            UpdateGeometry();
        }

        void UpdateCornerRadius(bool initialize)
        {
            if (initialize && ShadowsElement.CornerRadius == 0)
                return;
            UpdateGeometry();
        }

        void ShadePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!Shade.IsShadeProperty(e.PropertyName))
            {
                return;
            }

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
                case nameof(Shade.Opacity):
                case nameof(Shade.Offset):
                    UpdateGeometry();
                    break;
            }
        }
    }
}
