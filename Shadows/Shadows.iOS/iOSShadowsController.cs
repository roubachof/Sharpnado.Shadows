using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

using CoreAnimation;
using CoreGraphics;
using UIKit;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;

namespace Sharpnado.Shades.iOS
{
    public class iOSShadowsController
    {
        [Weak]
        private readonly CALayer _shadowsLayer;

        private float _cornerRadius;
        private IEnumerable<Shade> _shadesSource;

        public iOSShadowsController(CALayer shadowsLayer)
        {
            _shadowsLayer = shadowsLayer;
            _shadowsLayer.MasksToBounds = false;
        }

        public void DestroyShadow(int shadowIndex)
        {
            var shadowSubLayer = _shadowsLayer.Sublayers[shadowIndex];
            shadowSubLayer.RemoveFromSuperLayer();
            shadowSubLayer.Dispose();
        }

        public void DestroyShadows()
        {
            foreach (var subLayer in _shadowsLayer.Sublayers.ToArray())
            {
                subLayer.RemoveFromSuperLayer();
                subLayer.Dispose();
            }
        }

        public void OnLayoutSubLayers()
        {
            if (_shadowsLayer.Bounds.Width > 0)
            {
                foreach (var subLayer in _shadowsLayer.Sublayers)
                {
                    subLayer.ShadowPath = UIBezierPath.FromRoundedRect(_shadowsLayer.Bounds, _cornerRadius).CGPath;
                }
            }
        }

        public void UpdateCornerRadius(float cornerRadius)
        {
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
            if (shadesSource == null)
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
            var shadeSubLayer = shade.ToCALayer();
            _shadowsLayer.InsertSublayer(shadeSubLayer, insertIndex);

            shadeSubLayer.SetNeedsDisplay();
            shade.PropertyChanged += ShadePropertyChanged;
        }

        private void RemoveShade(int removedIndex, Shade shade)
        {
            shade.PropertyChanged -= ShadePropertyChanged;
            DestroyShadow(removedIndex);
            _shadowsLayer.SetNeedsDisplay();
        }

        private void ShadePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var shade = (Shade)sender;
            var index = _shadesSource.IndexOf(shade);
            if (index < 0)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"ShadowView::ShadePropertyChanged => shade property {e.PropertyName} changed but we can't find the shade in the source");
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(Shade.BlurRadius):
                case nameof(Shade.Color):
                case nameof(Shade.Offset):
                    UpdateShadeLayer(index, shade);
                    break;
            }
        }

        private void UpdateShadeLayer(int index, Shade shade)
        {
            var layer = _shadowsLayer.Sublayers[index];
            layer.ShadowColor = shade.Color.ToCGColor();
            layer.ShadowRadius = (nfloat)shade.BlurRadius;
            layer.ShadowOffset = new CGSize(shade.Offset.X, shade.Offset.Y);
            layer.ShadowOpacity = (float)shade.Opacity;

            layer.SetNeedsDisplay();
        }
    }
}