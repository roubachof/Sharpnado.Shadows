using System;
using System.Diagnostics;
using Xamarin.Forms.Xaml;

namespace ShadowsSample.Views
{
    public class NeuViewModel
    {
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Neumorphism : ShadowsElement
    {
        public Neumorphism()
        {
            InitializeComponent();
            BindingContext = new NeuViewModel();

            Debug.WriteLine($"Neumorphism view BindingContext: {BindingContext}");
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
            Debug.WriteLine($"ButtonPlusNeuShadows BindingContext: {ButtonPlusNeuShadows.BindingContext}");
            foreach (var shade in ButtonPlusNeuShadows.Shades)
            {
                Debug.WriteLine($"ButtonPlusShadows shade BindingContext: {shade.BindingContext}");
            }
        }
    }
}