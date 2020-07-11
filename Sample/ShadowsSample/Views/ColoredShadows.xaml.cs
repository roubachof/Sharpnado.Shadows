using System;
using System.Diagnostics;
using Sharpnado.Presentation.Forms;
using Xamarin.Forms.Xaml;

namespace ShadowsSample.Views
{
    public class ColoredViewModel
    {
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ColoredShadows : ShadowsElement
    {
        public ColoredShadows()
        {
            InitializeComponent();
            BindingContext = new ColoredViewModel();

            Debug.WriteLine($"ColoredShadows view BindingContext: {BindingContext}");
        }

        public override void OnIsCompactChanged()
        {
            if (IsCompact)
            {
                Description.Height = 0;
            }
        }

        private void ImageButtonOnClicked(object sender, EventArgs e)
        {
            Debug.WriteLine($"ButtonPlusColoredShadows BindingContext: {ButtonPlusColoredShadows.BindingContext}");
            foreach (var shade in ButtonPlusColoredShadows.Shades)
            {
                Debug.WriteLine($"ButtonPlusColoredShadows shade BindingContext: {shade.BindingContext}");
            }
        }
    }
}