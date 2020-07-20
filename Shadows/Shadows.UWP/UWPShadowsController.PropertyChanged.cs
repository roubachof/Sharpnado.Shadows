using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

using Windows.UI.Composition;
using Windows.UI.Xaml.Shapes;

using Xamarin.Forms.Internals;

namespace Sharpnado.Shades.UWP
{
    public partial class UWPShadowsController
    {
        public void UpdateCornerRadius(float cornerRadius)
        {
            if (_isDisposed)
            {
                return;
            }

            if (_shadowsCanvas == null && _shadowSource == null)
            {
                return;
            }

            InternalLogger.Debug(LogTag, () => $"UpdateCornerRadius( cornerRadius: {cornerRadius} )");
            bool hasChanged = _cornerRadius != cornerRadius;
            _cornerRadius = cornerRadius;

            if (hasChanged && _shadesSource.Any())
            {
                for (int i = 0; i < _shadowsCanvas.Children.Count; i++)
                {
                    var shadowHost = (Rectangle)_shadowsCanvas.Children[i];
                    shadowHost.RadiusX = cornerRadius;
                    shadowHost.RadiusY = cornerRadius;

                    var shadowVisual = _shadowVisuals[i];
                    ((DropShadow)shadowVisual.Shadow).Mask = shadowHost.GetAlphaMask();
                }
            }
        }

        public void UpdateShades(IEnumerable<Shade> shadesSource)
        {
            if (_isDisposed)
            {
                return;
            }

            if (shadesSource == null)
            {
                return;
            }

            InternalLogger.Debug(LogTag, () => $"UpdateShades( shadesSource: {shadesSource} )");

            if (_shadesSource is INotifyCollectionChanged previousNotifyCollectionChanged)
            {
                previousNotifyCollectionChanged.CollectionChanged -= ShadesSourceCollectionChanged;
            }

            _shadesSource = shadesSource;
            if (_shadesSource is INotifyCollectionChanged notifyCollectionChanged)
            {
                notifyCollectionChanged.CollectionChanged += ShadesSourceCollectionChanged;
            }

            DestroyShadows();
            for (int i = 0; i < _shadesSource.Count(); i++)
            {
                InsertShade(i, _shadesSource.ElementAt(i));
            }
        }

        private void ShadesSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_isDisposed)
            {
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0, insertIndex = e.NewStartingIndex; i < e.NewItems.Count; i++)
                    {
                        InsertShade(insertIndex, (Shade)e.NewItems[i]);
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0, removedIndex = e.OldStartingIndex; i < e.OldItems.Count; i++)
                    {
                        RemoveShade(removedIndex, (Shade)e.OldItems[i]);
                    }

                    break;

                case NotifyCollectionChangedAction.Reset:
                    DestroyShadows();
                    break;
            }
        }

        private void RemoveShade(int removedIndex, Shade shade)
        {
            InternalLogger.Debug(LogTag, () => $"RemoveShade( insertIndex: {removedIndex} )");
            shade.PropertyChanged -= ShadePropertyChanged;
            DestroyShadow(removedIndex);
        }

        private void UnsubscribeAllShades()
        {
            InternalLogger.Debug(LogTag, () => $"UnsubscribeAllShades() with count: {_shadesSource?.Count()}");
            foreach (var shade in _shadesSource)
            {
                shade.PropertyChanged -= ShadePropertyChanged;
            }
        }

        private void ShadePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_isDisposed)
            {
                return;
            }

            if (!Shade.IsShadeProperty(e.PropertyName))
            {
                return;
            }

            var shade = (Shade)sender;
            var index = _shadesSource.IndexOf(shade);
            if (index < 0)
            {
                InternalLogger.Warn(LogTag, $"ShadePropertyChanged => shade property {e.PropertyName} changed but we can't find the shade in the source");
                return;
            }

            InternalLogger.Debug(LogTag, () => $"ShadePropertyChanged( shadeIndex: {index}, propertyName: {e.PropertyName} )");
            switch (e.PropertyName)
            {
                case nameof(Shade.BlurRadius):
                case nameof(Shade.Color):
                case nameof(Shade.Opacity):
                case nameof(Shade.Offset):
                    UpdateShadeVisual(index, shade);
                    break;
            }
        }
    }
}
