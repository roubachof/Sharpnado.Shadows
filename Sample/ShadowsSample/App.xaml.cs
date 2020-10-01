using Xamarin.Forms;

namespace ShadowsSample
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            if (Device.Idiom == TargetIdiom.Tablet)
            {
                MainPage = new MainPageLandscape();
            }
            else
            {
                MainPage = new NavigationPage(new MainPage());
            }

            Sharpnado.Shades.Initializer.Initialize(true, true, filter: "ShadowsRenderer");

        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}

