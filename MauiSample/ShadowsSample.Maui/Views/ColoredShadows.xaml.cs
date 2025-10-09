using System;
using System.Diagnostics;
using Microsoft.Maui.Controls.Xaml;

namespace ShadowsSample.Maui.Views
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