using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;

namespace Sharpnado.Shades.Platforms.Windows;

/// <summary>
/// MAUI handler for Shadows component on Windows.
/// Uses WinUI 3 Composition API for shadow rendering.
/// </summary>
public class ShadowsHandler : ContentViewHandler
{
    private static int _instanceCount;
    private string _tag = nameof(ShadowsHandler);

    private WindowsShadowsController? _shadowsController;
    private Canvas? _shadowsCanvas;

    public ShadowsHandler() : base(ShadowsMapper)
    {
    }

    public static PropertyMapper<Shadows, ShadowsHandler> ShadowsMapper = new(Mapper)
    {
        [nameof(Shadows.CornerRadius)] = MapCornerRadius,
        [nameof(Shadows.Shades)] = MapShades,
    };

    protected override ContentPanel CreatePlatformView()
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

    protected override void ConnectHandler(ContentPanel platformView)
    {
        base.ConnectHandler(platformView);

        if (VirtualView is not Shadows shadowsView)
        {
            return;
        }

        // Wait for content to be available
        if (platformView.Children.Count > 0 && platformView.Children[0] is Panel contentPanel)
        {
            PackChild(platformView, contentPanel, shadowsView);
        }
    }

    protected override void DisconnectHandler(ContentPanel platformView)
    {
        if (_shadowsController != null)
        {
            _shadowsController.Dispose();
            _shadowsController = null;
        }

        _shadowsCanvas = null;

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

    private void PackChild(ContentPanel platformView, Panel contentPanel, Shadows shadowsView)
    {
        if (_shadowsController != null)
        {
            return; // Already created
        }

        _shadowsCanvas = new Canvas();

        // Insert shadows canvas before content
        platformView.Children.Insert(0, _shadowsCanvas);

        _shadowsController = new WindowsShadowsController(_shadowsCanvas, contentPanel, shadowsView.CornerRadius);
        _shadowsController.UpdateShades(shadowsView.Shades);

        // Subscribe to collection changes
        if (VirtualView is Shadows shadows)
        {
            shadows.WeakCollectionChanged += _shadowsController.ShadesSourceCollectionChanged;
        }

        InternalLogger.Debug(_tag, () => "ShadowController created");
    }
}
