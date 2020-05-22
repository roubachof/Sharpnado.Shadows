using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sharpnado.Shades
{
    [ContentProperty(nameof(Items))]
    [AcceptEmptyServiceProvider]
    public class ShadeStackExtension : IMarkupExtension<ObservableCollection<Shade>>
    {
        public ShadeStackExtension()
        {
            Items = new List<Shade>();
        }

        public List<Shade> Items { get; }

        public ObservableCollection<Shade> ProvideValue(IServiceProvider serviceProvider)
        {
            if (Items == null)
            {
                return new ObservableCollection<Shade>();
            }

            return new ObservableCollection<Shade>(Items);
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return (this as IMarkupExtension<IReadOnlyCollection<Shade>>).ProvideValue(serviceProvider);
        }
    }
}