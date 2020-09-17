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
    public partial class ShadowListCustomEasing : ContentPage
    {
        public ShadowListCustomEasing()
        {
            InitializeComponent();

            ResourcesHelper.SetNeumorphismMode();
        }
    }
}