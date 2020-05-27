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
        private const string LogTag = "iOSShadowsController";

        [Weak]
        private readonly UIView _shadowSource;

        [Weak]
        private readonly CALayer _shadowsLayer;

        private float _cornerRadius;
        private IEnumerable<Shade> _shadesSource;

        public iOSShadowsController(UIView shadowSource, CALayer shadowLayer, float cornerRadius)
        {
            _shadowSource = shadowSource;
            _shadowsLayer = shadowLayer;
            _cornerRadius = cornerRadius;
        }

        public void DestroyShadow(int shadowIndex)
        {
            InternalLogger.Debug(LogTag, $"DestroyShadow( shadowIndex: {shadowIndex} )");
            var shadowSubLayer = _shadowsLayer.Sublayers[shadowIndex];
            shadowSubLayer.RemoveFromSuperLayer();
            shadowSubLayer.Dispose();
        }

        public void DestroyShadows()
        {
            if (_shadowsLayer?.Sublayers == null)
            {
                return;
            }

            InternalLogger.Debug(LogTag, "DestroyShadows()");
            foreach (var subLayer in _shadowsLayer.Sublayers.ToArray())
            {
                subLayer.RemoveFromSuperLayer();
                subLayer.Dispose();
            }
        }

        public void OnLayoutSubLayers()
        {
            if (_shadowsLayer == null || _shadowSource == null || _shadowSource.Frame == CGRect.Empty)
            {
                return;
            }

            _shadowsLayer.Frame = _shadowSource.Frame;

            foreach (var subLayer in _shadowsLayer.Sublayers)
            {
                SetLayerFrame(subLayer);
                InternalLogger.Debug(LogTag, () => subLayer.ToInfo());
            }
        }

        public void UpdateCornerRadius(float cornerRadius)
        {
            if (_shadowsLayer == null && _shadowSource == null)
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
            if (shadesSource == null)
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

        private void SetLayerFrame(CALayer shadeLayer)
        {
            if (_shadowSource == null)
            {
                return;
            }

            var sourceFrame = _shadowSource.Bounds;
            if (sourceFrame.Width < 1 && sourceFrame.Height < 1)
            {
                return;
            }

            shadeLayer.Frame = sourceFrame;
            shadeLayer.ShadowPath = UIBezierPath.FromRoundedRect(sourceFrame, _cornerRadius).CGPath;
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

        private void ShadePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
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
                case nameof(Shade.Offset):
                    UpdateShadeLayer(index, shade);
                    break;
            }
        }

        private void UpdateShadeLayer(int index, Shade shade)
        {
            var layer = _shadowsLayer.Sublayers[index];
            layer.ShadowColor = shade.Color.ToCGColor();
            layer.ShadowRadius = (nfloat)shade.BlurRadius / 2;
            layer.ShadowOffset = new CGSize(shade.Offset.X, shade.Offset.Y);
            layer.ShadowOpacity = (float)shade.Opacity;
            layer.CornerRadius = _cornerRadius;

            layer.SetNeedsDisplay();
        }
    }
}