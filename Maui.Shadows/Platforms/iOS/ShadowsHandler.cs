using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;

namespace Sharpnado.Shades.Platforms.iOS;

/// <summary>
/// MAUI handler for Shadows component on iOS.
/// Uses CALayer for native iOS shadow rendering.
/// </summary>
public class ShadowsHandler() : ContentViewHandler(ShadowsMapper)
{
    public static PropertyMapper<Shadows, ShadowsHandler> ShadowsMapper = new(Mapper)
    {
        [nameof(Shadows.CornerRadius)] = MapCornerRadius,
        [nameof(Shadows.Shades)] = MapShades,
    };

    private static int _instanceCount;
    private string _tag = nameof(ShadowsHandler);

    private iOSShadowsController? _shadowsController;
    private CALayer? _shadowsLayer;

    private Shadows Shadows => (Shadows)VirtualView;

    protected override Microsoft.Maui.Platform.ContentView CreatePlatformView()
    {
        if (VirtualView is not Shadows shadowsView)
        {
            throw new InvalidOperationException($"VirtualView must be of type {nameof(Shadows)}");
        }

        var platformView = base.CreatePlatformView();

        if (!string.IsNullOrWhiteSpace(shadowsView.StyleId))
        {
            _tag += $" | {shadowsView.StyleId}@{shadowsView.InstanceNumber}";
        }

        InternalLogger.Debug(_tag, () => $"CreatePlatformView() => {++_instanceCount} instances");

        return platformView;
    }

    public override void PlatformArrange(Rect rect)
    {
        base.PlatformArrange(rect);

        // Wait for content to be available
        if (_shadowsController == null && PlatformView.Subviews.Length > 0)
        {
            CreateShadowController(PlatformView, PlatformView.Subviews[0], Shadows);
        }

        // Trigger layout for shadow controller
        _shadowsController?.OnLayoutSubLayers();
    }

    protected override void DisconnectHandler(Microsoft.Maui.Platform.ContentView platformView)
    {
        if (_shadowsController != null)
        {
            _shadowsController.Dispose();
            _shadowsController = null;
        }

        if (_shadowsLayer != null)
        {
            _shadowsLayer.Dispose();
            _shadowsLayer = null;
        }

        _instanceCount--;
        InternalLogger.Debug(_tag, () => $"Disposed => {_instanceCount} instances");

        base.DisconnectHandler(platformView);
    }

    public static void MapCornerRadius(ShadowsHandler handler, Shadows shadowsView)
    {
        handler._shadowsController?.UpdateCornerRadius(shadowsView.CornerRadius);
    }

    public static void MapShades(ShadowsHandler handler, Shadows shadowsView)
    {
        handler._shadowsController?.UpdateShades(shadowsView.Shades);
    }

    private void CreateShadowController(Microsoft.Maui.Platform.ContentView platformView, UIView shadowSource, Shadows formsElement)
    {
        if (_shadowsController != null)
        {
            return; // Already created
        }

        platformView.Layer.BackgroundColor = new CGColor(0, 0, 0, 0);
        platformView.Layer.MasksToBounds = false;

        _shadowsLayer = new CALayer { MasksToBounds = false };
        platformView.Layer.InsertSublayer(_shadowsLayer, 0);

        _shadowsController = new iOSShadowsController(shadowSource, _shadowsLayer, formsElement.CornerRadius);
        _shadowsController.UpdateShades(formsElement.Shades);

        // Subscribe to collection changes
        if (VirtualView is Shadows shadowsView)
        {
            shadowsView.WeakCollectionChanged += _shadowsController.ShadesSourceCollectionChanged;
        }

        InternalLogger.Debug(_tag, () => "ShadowController created");
    }
}
