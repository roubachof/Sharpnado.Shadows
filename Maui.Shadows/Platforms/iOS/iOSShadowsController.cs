using System.Collections.Specialized;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;

namespace Sharpnado.Shades.Platforms.iOS;

/// <summary>
/// Controller that manages shadow CALayers for iOS views.
/// Uses iOS native shadow rendering with CALayer.
/// </summary>
internal partial class iOSShadowsController : IDisposable
{
    private const string LogTag = "iOSShadowsController";

    private readonly WeakReference<UIView> _weakShadowSource;
    private readonly WeakReference<CALayer> _weakShadowsLayer;

    private bool _isDisposed;
    private float _cornerRadius;
    private IEnumerable<Shade>? _shadesSource;

    public iOSShadowsController(UIView shadowSource, CALayer shadowLayer, float cornerRadius)
    {
        _weakShadowSource = new WeakReference<UIView>(shadowSource);
        _weakShadowsLayer = new WeakReference<CALayer>(shadowLayer);
        _cornerRadius = cornerRadius;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void OnLayoutSubLayers()
    {
        if (_isDisposed 
            || !_weakShadowsLayer.TryGetTarget(out var shadowsLayer) 
            || !_weakShadowSource.TryGetTarget(out var shadowSource)
            || shadowSource.Frame == CGRect.Empty)
        {
            return;
        }

        shadowsLayer.Frame = shadowSource.Frame;

        if (shadowsLayer.Sublayers == null)
        {
            return;
        }

        foreach (var subLayer in shadowsLayer.Sublayers)
        {
            SetLayerFrame(subLayer);
            InternalLogger.Debug(LogTag, () => subLayer.ToInfo());
        }
    }

    protected void Dispose(bool disposing)
    {
        if (disposing && !_isDisposed)
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
        if (!_weakShadowsLayer.TryGetTarget(out var shadowsLayer) || shadowsLayer.Sublayers == null)
        {
            return;
        }

        InternalLogger.Debug(LogTag, $"DestroyShadow( shadowIndex: {shadowIndex} )");
        var shadowSubLayer = shadowsLayer.Sublayers[shadowIndex];
        shadowSubLayer.RemoveFromSuperLayer();
        shadowSubLayer.Dispose();
    }

    private void DestroyShadows()
    {
        if (!_weakShadowsLayer.TryGetTarget(out var shadowsLayer) || shadowsLayer.Sublayers == null)
        {
            return;
        }

        InternalLogger.Debug(LogTag, "DestroyShadows()");
        foreach (var subLayer in shadowsLayer.Sublayers.ToArray())
        {
            subLayer.RemoveFromSuperLayer();
            subLayer.Dispose();
        }
    }

    private void SetLayerFrame(CALayer shadeLayer)
    {
        if (!_weakShadowSource.TryGetTarget(out var shadowSource))
        {
            return;
        }

        var sourceFrame = shadowSource.Bounds;
        if (sourceFrame.Width < 1 && sourceFrame.Height < 1)
        {
            return;
        }

        shadeLayer.Frame = sourceFrame;
        shadeLayer.ShadowPath = UIBezierPath.FromRoundedRect(sourceFrame, _cornerRadius).CGPath;
    }

    private void UpdateShadeLayer(int index, Shade shade)
    {
        if (!_weakShadowsLayer.TryGetTarget(out var shadowsLayer) || shadowsLayer.Sublayers == null)
        {
            return;
        }

        var layer = shadowsLayer.Sublayers[index];
        layer.ShadowColor = shade.Color.ToCGColor();
        layer.ShadowRadius = (nfloat)shade.BlurRadius / 2;
        layer.ShadowOffset = new CGSize(shade.Offset.X, shade.Offset.Y);
        layer.ShadowOpacity = (float)shade.Opacity;
        layer.CornerRadius = _cornerRadius;

        layer.SetNeedsDisplay();
    }
}
