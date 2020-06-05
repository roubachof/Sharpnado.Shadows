using Xamarin.Forms.Xaml;

namespace ShadowsSample.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ColoredShadows : ShadowsElement
    {
        public ColoredShadows()
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