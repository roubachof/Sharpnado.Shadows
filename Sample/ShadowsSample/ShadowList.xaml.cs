using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}