using System;
using System.ComponentModel;

using Sharpnado.Tasks;

using Xamarin.Forms;

namespace ShadowsSample
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
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

        private void OnNavigateToShadowsListClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ShadowList());
        }
    }
}
