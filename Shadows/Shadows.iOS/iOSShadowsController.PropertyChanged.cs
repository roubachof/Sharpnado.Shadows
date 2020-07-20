using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

using Xamarin.Forms.Internals;

namespace Sharpnado.Shades.iOS
{
    public partial class iOSShadowsController
    {
        public void UpdateCornerRadius(float cornerRadius)
        {
            if (_isDisposed || _shadowsLayer == null && _shadowSource == null)
            {
                return;
            }

            InternalLogger.Debug(LogTag, () => $"UpdateCornerRadius( cornerRadius: {cornerRadius} )");
            bool hasChanged = _cornerRadius != cornerRadius;
            _cornerRadius = cornerRadius;

            if (hasChanged && _shadesSource.Any())
            {
                foreach (var subLayer in _shadowsLayer.Sublayers)
                {
                    subLayer.CornerRadius = cornerRadius;
                }
            }
        }

        public void UpdateShades(IEnumerable<Shade> shadesSource)
        {
            if (_isDisposed || shadesSource == null)
            {
                return;
            }

            InternalLogger.Debug(LogTag, () => $"UpdateShades( shadesSource: {shadesSource} )");
            if (_shadowsLayer == null && _shadowSource == null)
            {
                return;
            }

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

                    _shadowsLayer.SetNeedsDisplay();

                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0, removedIndex = e.OldStartingIndex; i < e.OldItems.Count; i++)
                    {
                        RemoveShade(removedIndex, (Shade)e.OldItems[i]);
                    }

                    _shadowsLayer.SetNeedsDisplay();

                    break;

                case NotifyCollectionChangedAction.Reset:
                    DestroyShadows();
                    _shadowsLayer.SetNeedsDisplay();
                    break;
            }
        }

        private void InsertShade(int insertIndex, Shade shade)
        {
            InternalLogger.Debug(LogTag, () => $"InsertShade( insertIndex: {insertIndex}, shade: {shade} )");
            var shadeSubLayer = shade.ToCALayer();
            shadeSubLayer.CornerRadius = _cornerRadius;
            SetLayerFrame(shadeSubLayer);

            _shadowsLayer.InsertSublayer(shadeSubLayer, insertIndex);

            shadeSubLayer.SetNeedsDisplay();
            shade.PropertyChanged += ShadePropertyChanged;
        }

        private void RemoveShade(int removedIndex, Shade shade)
        {
            InternalLogger.Debug(LogTag, () => $"RemoveShade( insertIndex: {removedIndex} )");
            shade.PropertyChanged -= ShadePropertyChanged;
            DestroyShadow(removedIndex);
            _shadowsLayer.SetNeedsDisplay();
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
                    UpdateShadeLayer(index, shade);
                    break;
            }
        }
    }
}