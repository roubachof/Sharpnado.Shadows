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
        private static int instanceCount;

        private ShadowView _shadowView;

        private string _tag = nameof(AndroidShadowsRenderer);

        public AndroidShadowsRenderer(Context context)
            : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Shadows> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null)
            {
                if (!_shadowView.IsNullOrDisposed())
                {
                    _shadowView.RemoveFromParent();
                    _shadowView.Dispose();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!_shadowView.IsNullOrDisposed())
            {
                _shadowView.RemoveFromParent();
                _shadowView.Dispose();
            }

            instanceCount--;

            InternalLogger.Debug(_tag, () => $"Disposed( disposing: {disposing} ) => {instanceCount} instances");
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

                    if (!string.IsNullOrWhiteSpace(Element.StyleId))
                    {
                        _tag += $" | {Element.StyleId}@{Element.InstanceNumber}";
                    }

                    if (_shadowView == null)
                    {
                        _shadowView = new ShadowView(Context, content, Context.ToPixels(Element.CornerRadius));
                        _shadowView.UpdateShades(Element.Shades);

                        Element.WeakCollectionChanged += _shadowView.ShadesSourceCollectionChanged;

                        AddView(_shadowView, 0);

                        instanceCount++;
                        InternalLogger.Debug(_tag, () => $"Create ShadowView => {instanceCount} instances");
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

            InternalLogger.Debug(_tag, () => $"OnLayout( {l}l, {t}t, {r}r, {b}b )");

            var children = GetChildAt(1);
            if (children == null)
            {
                return;
            }

            _shadowView?.Layout(children.MeasuredWidth, children.MeasuredHeight);
        }
    }
}
