using System;
using System.Collections.Generic;
using System.Text;

using Sharpnado.Shades;

using Xamarin.Forms;

namespace ShadowsSample.Views
{
    public class ShadowsElement : Shadows
    {
        public static readonly BindableProperty IsCompactProperty = BindableProperty.Create(
            nameof(IsCompact),
            typeof(bool),
            typeof(ShadowsElement),
            false,
            propertyChanged: IsCompactPropertyChanged);

        private static void IsCompactPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            ((ShadowsElement)bindable).OnIsCompactChanged();
        }

        public bool IsCompact
        {
            get => (bool)GetValue(IsCompactProperty);
            set => SetValue(IsCompactProperty, value);
        }

        public virtual void OnIsCompactChanged()
        {
        }
    }
}
