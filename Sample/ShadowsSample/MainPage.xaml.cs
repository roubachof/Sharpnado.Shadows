using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Sharpnado.Shades;

using Xamarin.Forms;

namespace ShadowsSample
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            SetValue(NavigationPage.HasNavigationBarProperty, false);
            InitializeComponent();

            ResourcesHelper.SetNeumorphismMode();
            InitNewShade();
            ColorEntry.TextChanged += ColorEntryTextChanged;
            BindableLayout.SetItemsSource(StackLayout, ShadeInfos);
            ColorEntryTextChanged(ColorEntry, new TextChangedEventArgs("", ColorEntry.Text));
        }

        public ObservableCollection<ShadeInfo> ShadeInfos = new ObservableCollection<ShadeInfo>();


        private void InitNewShade()
        {
            OffsetEntry.Text = "10,-10";
            ColorEntry.Text = "#99FF99";
            OpacityEntry.Text = "0.5";
            BlurEntry.Text = "10";
        }

        private void ColorEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            var color = Color.FromHex(e.NewTextValue);
            if (color == Color.Default)
            {
                return;
            }

            ColorEntry.TextColor = color;
        }

        private void AddShade(object sender, EventArgs e)
        {
            string offsetText = OffsetEntry.Text;
            string colorText = ColorEntry.Text;
            string opacityText = OpacityEntry.Text;
            string blurText = BlurEntry.Text;

            if (offsetText is null || colorText is null || opacityText is null || blurText is null)
            {
                return;
            }

            var splitOffset = offsetText.Split(',');
            if (splitOffset.Length != 2)
            {
                return;
            }

            if (!int.TryParse(
                    splitOffset[0],
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var xOffset) 
                || !int.TryParse(
                    splitOffset[1],
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var yOffset))
            {
                return;
            }

            if (!double.TryParse(opacityText, NumberStyles.Number, CultureInfo.InvariantCulture, out var opacity))
            {
                return;
            }

            var color = Color.FromHex(colorText).MultiplyAlpha(opacity);
            if (color == Color.Default)
            {
                return;
            }

            if (!double.TryParse(blurText, NumberStyles.Number, CultureInfo.InvariantCulture, out var blur))
            {
                return;
            }

            ((ObservableCollection<Shade>)DynamicShadows.Shades).Add(
                new Shade { Offset = new Point(xOffset, yOffset), Color = color, BlurRadius = blur });

            ShadeInfos.Add(new ShadeInfo($"{xOffset},{yOffset}", color, blur.ToString()));
        }

        private void RemoveShade(object sender, EventArgs e)
        {
            if (ShadeInfos.Count == 0)
            {
                return;
            }

            int lastIndex = ShadeInfos.Count - 1;
            ShadeInfos.RemoveAt(lastIndex);
            ((ObservableCollection<Shade>)DynamicShadows.Shades).RemoveAt(lastIndex);
        }

        public readonly struct ShadeInfo
        {
            public ShadeInfo(string offset, Color color, string blur)
            {
                Offset = offset;
                ColorHex = color.ToHex();
                Color = new Color(color.R, color.G, color.B);
                Blur = blur;
            }

            public string Offset { get; }

            public string ColorHex { get; }

            public Color Color { get; }

            public string Blur { get; }
        }
    }
}
