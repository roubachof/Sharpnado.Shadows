using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using CoreAnimation;
using CoreGraphics;
using UIKit;
using Xamarin.Forms.Platform.iOS;

namespace Sharpnado.Shades.iOS
{
    public partial class iOSShadowsController : IDisposable
    {
        private const string LogTag = "iOSShadowsController";

        [Weak]
        private readonly UIView _shadowSource;

        [Weak]
        private readonly CALayer _shadowsLayer;

        private bool _isDisposed;

        private float _cornerRadius;
        private IEnumerable<Shade> _shadesSource;

        public iOSShadowsController(UIView shadowSource, CALayer shadowLayer, float cornerRadius)
        {
            _shadowSource = shadowSource;
            _shadowsLayer = shadowLayer;
            _cornerRadius = cornerRadius;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void OnLayoutSubLayers()
        {
            if (_shadowsLayer == null || _shadowSource == null || _shadowSource.Frame == CGRect.Empty)
            {
                return;
            }

            _shadowsLayer.Frame = _shadowSource.Frame;

            if (_shadowsLayer.Sublayers == null)
            {
                return;
            }

            foreach (var subLayer in _shadowsLayer.Sublayers)
            {
                SetLayerFrame(subLayer);
                InternalLogger.Debug(LogTag, () => subLayer.ToInfo());
            }
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                InternalLogger.Debug(LogTag, "Dispose()");

                if (_shadesSource is INotifyCollectionChanged shadeNotifyCollection)
                {
                    shadeNotifyCollection.CollectionChanged -= ShadesSourceCollectionChanged;
                }

                UnsubscribeAllShades();
                DestroyShadows();

                _isDisposed = true;
            }
        }

        private void DestroyShadow(int shadowIndex)
        {
            InternalLogger.Debug(LogTag, $"DestroyShadow( shadowIndex: {shadowIndex} )");
            var shadowSubLayer = _shadowsLayer.Sublayers[shadowIndex];
            shadowSubLayer.RemoveFromSuperLayer();
            shadowSubLayer.Dispose();
        }

        private void DestroyShadows()
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