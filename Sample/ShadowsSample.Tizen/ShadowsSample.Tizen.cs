using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;

namespace ShadowsSample.Tizen
{
    class Program : FormsApplication
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            ElmSharp.Utility.AppendGlobalFontPath(this.DirectoryInfo.Resource);
            LoadApplication(new App());
        }

        static void Main(string[] args)
        {
            var app = new Program();
            Forms.Init(app, true);
            Sharpnado.Shades.Tizen.TizenShadowsRenderer.Initialize();
            app.Run(args);
        }
    }
}
