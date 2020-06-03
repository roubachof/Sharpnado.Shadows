using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace Sharpnado.Shades
{
    public class Shadows : ContentView
    {
        public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
            nameof(CornerRadius),
            typeof(int),
            typeof(Shadows),
            DefaultCornerRadius);

        public static readonly BindableProperty ShadesProperty = BindableProperty.Create(
            nameof(Shades),
            typeof(IEnumerable<Shade>),
            typeof(Shadows),
            defaultValueCreator: (bo) => new ObservableCollection<Shade> { new Shade() },
            validateValue: (bo, v) =>
                {
                    var shades = (IEnumerable<Shade>)v;
                    return shades != null;
                });

        private const int DefaultCornerRadius = 0;

        public int CornerRadius
        {
            get => (int)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public IEnumerable<Shade> Shades
        {
            get => (IEnumerable<Shade>)GetValue(ShadesProperty);
            set => SetValue(ShadesProperty, value);
        }
    }
}
