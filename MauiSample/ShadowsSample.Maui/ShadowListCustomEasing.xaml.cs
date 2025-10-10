using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace ShadowsSample.Maui
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