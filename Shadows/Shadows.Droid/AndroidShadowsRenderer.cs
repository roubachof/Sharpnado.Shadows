using System.ComponentModel;

using Android.Content;
using Android.Widget;
using Sharpnado.Shades.Droid;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Sharpnado.Shades.Shadows), typeof(AndroidShadowsRenderer))]

namespace Sharpnado.Shades.Droid
{
    public class AndroidShadowsRenderer : ViewRenderer<Shadows, FrameLayout>
    {
        private ShadowView _shadowView;

        public AndroidShadowsRenderer(Context context)
            : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Shadows> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null)
            {
                _shadowView.Dispose();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            InternalLogger.Debug("Renderer", $"Disposed( disposing: {disposing} )");
            if (disposing)
            {
                _shadowView?.Dispose();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            switch (e.PropertyName)
            {
                case "Renderer":
                    var content = GetChildAt(0);
                    if (content == null)
                    {
                        return;
                    }

                    if (_shadowView == null)
                    {
                        _shadowView = new ShadowView(Context, content, Context.ToPixels(Element.CornerRadius));
                        _shadowView.UpdateShades(Element.Shades);

                        AddView(_shadowView, 0);
                    }

                    break;

                case nameof(Element.CornerRadius):
                    _shadowView.UpdateCornerRadius(Context.ToPixels(Element.CornerRadius));
                    break;

                case nameof(Element.Shades):
                    _shadowView.UpdateShades(Element.Shades);
                    break;
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            InternalLogger.Debug("Renderer", $"OnLayout( {l}l, {t}t, {r}r, {b}b )");

            var children = GetChildAt(1);
            if (children == null)
            {
                return;
            }

            InternalLogger.Debug("Renderer", $"this: {GetX()}x, {GetY()}y, {MeasuredWidth}w, {MeasuredHeight}h");

            InternalLogger.Debug("Renderer", $"child: {children.GetX()}x, {children.GetY()}y, {children.MeasuredWidth}w, {children.MeasuredHeight}h");

            _shadowView?.Layout(MeasuredWidth, MeasuredHeight);
        }
    }
}
