using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace ShadowsSample.Maui
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

        private void OnBackButtonClicked(object? sender, EventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}
