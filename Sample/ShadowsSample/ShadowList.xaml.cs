using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShadowsSample
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShadowList : ContentPage
    {
        public ShadowList()
        {
            InitializeComponent();

            ResourcesHelper.SetNeumorphismMode();
        }

        private void OnNavigateToMainPageClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MainPage());
        }

        private void LogoOnTapped(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}