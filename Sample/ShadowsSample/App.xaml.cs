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
                MainPage = new MainPage();
            }

            Sharpnado.Shades.Initializer.Initialize(true, false);
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

