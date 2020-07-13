using System.ComponentModel;
using System.Numerics;

using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;

using Sharpnado.Shades;
using Sharpnado.Shades.UWP;

using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(Shadows), typeof(UWPShadowsRenderer))]

namespace Sharpnado.Shades.UWP
{
    /// <summary>
    ///     Renderer to update all frames with better shadows matching material design standards.
    /// </summary>
    [Preserve]
    public class UWPShadowsRenderer : ViewRenderer<Shadows, Grid>
    {
        private const string LogTag = nameof(UWPShadowsRenderer);

        private Canvas _shadowsCanvas;

        private UWPShadowsController _shadowsController;

        public UWPShadowsRenderer()
        {
            AutoPackage = false;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            // We need an automation peer so we can interact with this in automated tests
            if (Control == null)
            {
                return new FrameworkElementAutomationPeer(this);
            }

            return new FrameworkElementAutomationPeer(Control);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _shadowsController?.Dispose();
                _shadowsController = null;
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Shadows> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null)
            {
                return;
            }

            if (Control == null)
            {
                SetNativeControl(new Grid());
            }

            PackChild();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Element.CornerRadius):
                    _shadowsController?.UpdateCornerRadius(Element.CornerRadius);
                    break;

                case nameof(Element.Shades):
                    _shadowsController?.UpdateShades(Element.Shades);
                    break;
            }
        }

        private void PackChild()
        {
            if (Element.Content == null)
            {
                return;
            }

            IVisualElementRenderer renderer = Element.Content.GetOrCreateRenderer();
            FrameworkElement frameworkElement = renderer.ContainerElement;

            _shadowsCanvas = new Canvas();

            Control.Children.Add(_shadowsCanvas);
            Control.Children.Add(frameworkElement);

            _shadowsController = new UWPShadowsController(_shadowsCanvas, frameworkElement, Element.CornerRadius);
            _shadowsController.UpdateShades(Element.Shades);
        }
    }
}