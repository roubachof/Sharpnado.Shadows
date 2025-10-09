using System.ComponentModel;


using Microsoft.Maui.Controls;
using ShadowsSample.Maui.Views;

namespace ShadowsSample.Maui
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPageLandscape : ContentPage
    {
        public MainPageLandscape()
        {
            SetValue(NavigationPage.HasNavigationBarProperty, false);
            InitializeComponent();

            ResourcesHelper.SetNeumorphismMode();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            BeCreative.OnAppearing();
        }
    }
}
