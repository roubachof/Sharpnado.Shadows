using System.Threading.Tasks;

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;

using Sharpnado.Shades.Droid;
using Sharpnado.Tasks;

using Xamarin.Forms;

namespace ShadowsSample.Droid
{
    [Activity(Label = "ShadowsSample", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (Device.Idiom == TargetIdiom.Tablet)
            {
                RequestedOrientation = ScreenOrientation.Landscape;
            }

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            Sharpnado.Presentation.Forms.Droid.SharpnadoInitializer.Initialize();

            LoadApplication(new App());

            TaskMonitor.Create(
                async () =>
                    {
                        while (true)
                        {
                            BitmapCache.Instance?.Log();
                            await Task.Delay(10000);
                        }
                    });
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}