using System;
using System.Collections.ObjectModel;
using System.Globalization;

using Sharpnado.Shades;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace ShadowsSample.Maui.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DynamicShadows : ShadowsElement
    {
        public DynamicShadows()
        {
            InitializeComponent();

            InitNewShade();
            ColorEntry.TextChanged += ColorEntryTextChanged;
            BindableLayout.SetItemsSource(StackLayout, ShadeInfos);
            ColorEntryTextChanged(ColorEntry, new TextChangedEventArgs("", ColorEntry.Text));
        }

        public ObservableCollection<ShadeInfo> ShadeInfos = new ObservableCollection<ShadeInfo>();

        public override void OnIsCompactChanged()
        {
            if (IsCompact)
            {
                Description.Height = 0;
            }
        }

        private void InitNewShade()
        {
            OffsetEntry.Text = "10,-10";
            ColorEntry.Text = "#FE99FE";
            OpacityEntry.Text = "0.5";
            BlurEntry.Text = "10";
        }

        private void ColorEntryTextChanged(object? sender, TextChangedEventArgs e)
        {
            try
            {
                var color = Color.FromArgb(e.NewTextValue);
                ColorEntry.TextColor = color;
            }
            catch
            {
                // Invalid color format, keep current color
            }
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

            Color color;
            try
            {
                color = Color.FromArgb(colorText).MultiplyAlpha((float)opacity);
            }
            catch
            {
                return;
            }

            if (!double.TryParse(blurText, NumberStyles.Number, CultureInfo.InvariantCulture, out var blur))
            {
                return;
            }

            ((ObservableCollection<Shade>)CatShadows.Shades).Add(
                new Shade { Offset = new Point(xOffset, yOffset), Color = color, Opacity = opacity, BlurRadius = blur });

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
            ((ObservableCollection<Shade>)CatShadows.Shades).RemoveAt(lastIndex);
        }

        public readonly struct ShadeInfo
        {
            public ShadeInfo(string offset, Color color, string blur)
            {
                Offset = offset;
                ColorHex = color.ToHex();
                Color = new Color(color.Red, color.Green, color.Blue);
                Blur = blur;
            }

            public string Offset { get; }

            public string ColorHex { get; }

            public Color Color { get; }

            public string Blur { get; }
        }
    }
}