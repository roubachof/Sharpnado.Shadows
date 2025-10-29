using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Grid = Microsoft.UI.Xaml.Controls.Grid;

namespace Sharpnado.Shades.Platforms.Windows;

/// <summary>
/// MAUI handler for Shadows component on Windows.
/// Uses WinUI 3 Composition API for shadow rendering.
/// Custom ViewHandler that manages a Grid container with shadows canvas and content.
/// </summary>
public class ShadowsHandler : ViewHandler<Shadows, Grid>
{
    private static int _instanceCount;
    private string _tag = nameof(ShadowsHandler);

    private WindowsShadowsController? _shadowsController;
    private Canvas? _shadowsCanvas;
    private FrameworkElement? _contentPlatformView;

    public ShadowsHandler() : base(ShadowsMapper)
    {
    }

    public static PropertyMapper<Shadows, ShadowsHandler> ShadowsMapper = new(ViewMapper)
    {
        [nameof(Shadows.CornerRadius)] = MapCornerRadius,
        [nameof(Shadows.Shades)] = MapShades,
    };

    protected override Grid CreatePlatformView()
    {
        if (VirtualView is not Shadows shadowsView)
        {
            throw new InvalidOperationException($"VirtualView must be of type {nameof(Shadows)}");
        }

        // Create a Grid container that will hold shadows canvas and content
        var grid = new Grid();

        if (!string.IsNullOrWhiteSpace(shadowsView.StyleId))
        {
            _tag += $" | {shadowsView.StyleId}@{shadowsView.InstanceNumber}";
        }

        InternalLogger.Debug(_tag, () => $"CreatePlatformView() => {++_instanceCount} instances");

        return grid;
    }

    protected override void ConnectHandler(Grid platformView)
    {
        base.ConnectHandler(platformView);

        if (VirtualView is not Shadows shadowsView)
        {
            return;
        }

        // Create shadow canvas
        _shadowsCanvas = new Canvas();
        platformView.Children.Add(_shadowsCanvas);

        // Create and add content view
        if (shadowsView.Content != null)
        {
            _contentPlatformView = shadowsView.Content.ToPlatform(MauiContext!);
            
            // Manually apply MAUI margin to the platform view
            var contentMargin = shadowsView.Content.Margin;
            _contentPlatformView.Margin = new Microsoft.UI.Xaml.Thickness(
                contentMargin.Left,
                contentMargin.Top,
                contentMargin.Right,
                contentMargin.Bottom);
            
            platformView.Children.Add(_contentPlatformView);

            // Create shadow controller
            _shadowsController = new WindowsShadowsController(_shadowsCanvas, _contentPlatformView, shadowsView.CornerRadius);
            _shadowsController.UpdateShades(shadowsView.Shades);

            // Subscribe to collection changes
            shadowsView.WeakCollectionChanged += _shadowsController.ShadesSourceCollectionChanged;

            InternalLogger.Debug(_tag, () => "ShadowController created");
        }
    }

    protected override void DisconnectHandler(Grid platformView)
    {
        if (VirtualView is Shadows shadowsView)
        {
            if (_shadowsController != null)
            {
                shadowsView.WeakCollectionChanged -= _shadowsController.ShadesSourceCollectionChanged;
            }
        }

        if (_shadowsController != null)
        {
            _shadowsController.Dispose();
            _shadowsController = null;
        }

        // Clear platform view children
        platformView.Children.Clear();
        _shadowsCanvas = null;
        _contentPlatformView = null;

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
}
