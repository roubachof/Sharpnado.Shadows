using Microsoft.Maui.Controls;
using ShadowsSample.Maui.Helpers;

namespace ShadowsSample.Maui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                MainPage = new MainPageLandscape();
            }
            else
            {
                MainPage = new NavigationPage(new MainPage());
            }
        }

        protected override void OnStart()
        {
#if MACCATALYST
            // Diagnostic: List all available fonts on MacCatalyst
            FontDiagnostics.ListAllFonts();
            FontDiagnostics.TestFontAwesome();
#endif
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}

