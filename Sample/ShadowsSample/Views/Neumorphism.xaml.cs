using Xamarin.Forms.Xaml;

namespace ShadowsSample.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Neumorphism : ShadowsElement
    {
        public Neumorphism()
        {
            InitializeComponent();
        }

        public override void OnIsCompactChanged()
        {
            if (IsCompact)
            {
                Description.Height = 0;
            }
        }
    }
}