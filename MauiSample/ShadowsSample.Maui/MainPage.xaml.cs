using System;
using System.ComponentModel;
using System.Threading.Tasks;


using Microsoft.Maui.Controls;

namespace ShadowsSample.Maui
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
            // BeCreative.OnAppearing();

            if (Navigation.NavigationStack.Count == 1)
            {
                BackButtonShadows.IsVisible = false;
            }

            MainThread.BeginInvokeOnMainThread(async () =>
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    await Task.Delay(500);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                });
        }

        private void OnNavigateToShadowsListClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ShadowList());
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
