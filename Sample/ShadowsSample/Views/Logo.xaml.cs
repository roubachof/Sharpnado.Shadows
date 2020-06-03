using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sharpnado.Shades;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShadowsSample.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Logo : Shadows
    {
        public Logo()
        {
            InitializeComponent();
        }
    }
}