using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

using WeakEvent;

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
            defaultValueCreator: (bo) => new ObservableCollection<Shade> { new Shade { Parent = (Shadows)bo } },
            validateValue: (bo, v) => v is IEnumerable<Shade>,
            propertyChanged: ShadesPropertyChanged,
            coerceValue: CoerceShades);

        private const int DefaultCornerRadius = 0;

        private static int instanceCount = 0;

        private readonly WeakEventSource<NotifyCollectionChangedEventArgs> _weakCollectionChangedSource = new WeakEventSource<NotifyCollectionChangedEventArgs>();

        public Shadows()
        {
            InstanceNumber = ++instanceCount;
        }

        public event EventHandler<NotifyCollectionChangedEventArgs> WeakCollectionChanged
        {
            add => _weakCollectionChangedSource.Subscribe(value);
            remove => _weakCollectionChangedSource.Unsubscribe(value);
        }

        public int InstanceNumber { get; }

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

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            foreach (var shade in Shades)
            {
                SetInheritedBindingContext(shade, BindingContext);
            }
        }

        private static object CoerceShades(BindableObject bindable, object value)
        {
            if (!(value is ReadOnlyCollection<Shade> readonlyCollection))
            {
                return value;
            }

            return new ReadOnlyCollection<Shade>(
                readonlyCollection.Select(s => s.Clone())
                    .ToList());
        }

        private static void ShadesPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var shadows = (Shadows)bindable;
            var enumerableShades = (IEnumerable<Shade>)newvalue;

            if (oldvalue != null)
            {
                if (oldvalue is INotifyCollectionChanged oldCollection)
                {
                    oldCollection.CollectionChanged -= shadows.OnShadeCollectionChanged;
                }

                foreach (var shade in enumerableShades)
                {
                    shade.Parent = null;
                    shade.BindingContext = null;
                }
            }

            foreach (var shade in enumerableShades)
            {
                shade.Parent = shadows;
                SetInheritedBindingContext(shade, shadows.BindingContext);
            }

            if (newvalue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += shadows.OnShadeCollectionChanged;
            }
        }

        private void OnShadeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Shade newShade in e.NewItems)
                    {
                        newShade.Parent = this;
                        SetInheritedBindingContext(newShade, BindingContext);
                        _weakCollectionChangedSource.Raise(this, e);
                    }

                    break;

                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Remove:
                    foreach (Shade oldShade in e.OldItems ?? new Shade[0])
                    {
                        oldShade.Parent = null;
                        oldShade.BindingContext = null;
                        _weakCollectionChangedSource.Raise(this, e);
                    }

                    break;
            }
        }
    }
}
