using System.ComponentModel;
using System.Runtime.Remoting.Contexts;

using Sharpnado.Shades.iOS;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Sharpnado.Shades.Shadows), typeof(iOSShadowsRenderer))]

namespace Sharpnado.Shades.iOS
{
    public class iOSShadowsRenderer : VisualElementRenderer<Shadows>
    {
        public static new void Init()
        {
            var preserveRenderer = typeof(iOSShadowsRenderer);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Shadows> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null)
            {
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            switch (e.PropertyName)
            {
                case "Renderer":
                    break;

                case nameof(Element.CornerRadius):
                    break;

                case nameof(Element.Shades):
                    break;
            }
        }
    }
}
