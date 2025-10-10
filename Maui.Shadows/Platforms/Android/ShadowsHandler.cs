using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Sharpnado.Shades.Platforms.Android;

public class ShadowsHandler() : ContentViewHandler(ShadowsMapper)
{
    public static PropertyMapper<Shadows, ShadowsHandler> ShadowsMapper = new(Mapper)
    {
        [nameof(Shadows.CornerRadius)] = MapCornerRadius,
        [nameof(Shadows.Shades)] = MapShades,
        [nameof(Shadows.AndroidBlurType)] = MapBlurType,
    };

    private static int _instanceCount;
    private ShadowView? _shadowView;
    private string _tag = nameof(ShadowsHandler);

    protected override ContentViewGroup CreatePlatformView()
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

    protected override void ConnectHandler(ContentViewGroup platformView)
    {
        base.ConnectHandler(platformView);

        if (VirtualView is not Shadows shadowsView)
        {
            return;
        }

        // Wait for the content to be laid out
        platformView.ViewAttachedToWindow += OnPlatformViewAttachedToWindow;
        platformView.LayoutChange += OnPlatformViewLayoutChange;
    }

    protected override void DisconnectHandler(ContentViewGroup platformView)
    {
        platformView.ViewAttachedToWindow -= OnPlatformViewAttachedToWindow;
        platformView.LayoutChange -= OnPlatformViewLayoutChange;

        if (VirtualView is Shadows shadowsView && _shadowView != null)
        {
            shadowsView.WeakCollectionChanged -= _shadowView.ShadesSourceCollectionChanged;

            if (!_shadowView.IsNullOrDisposed())
            {
                platformView.RemoveView(_shadowView);
                _shadowView.Dispose();
                _shadowView = null;
            }

            _instanceCount--;
            InternalLogger.Debug(_tag, () => $"Disposed => {_instanceCount} instances");
        }

        base.DisconnectHandler(platformView);
    }

    public static void MapCornerRadius(ShadowsHandler handler, Shadows shadowsView)
    {
        if (handler._shadowView != null)
        {
            handler._shadowView.UpdateCornerRadius(handler.Context.ToPixels(shadowsView.CornerRadius));
        }
    }

    public static void MapShades(ShadowsHandler handler, Shadows shadowsView)
    {
        handler._shadowView?.UpdateShades(shadowsView.Shades);
    }

    public static void MapBlurType(ShadowsHandler handler, Shadows shadowsView)
    {
        handler._shadowView?.UpdateBlurType(shadowsView.AndroidBlurType);
    }

    private void OnPlatformViewAttachedToWindow(object? sender, global::Android.Views.View.ViewAttachedToWindowEventArgs e)
    {
        if (VirtualView is not Shadows shadowsView)
        {
            return;
        }

        var content = PlatformView.GetChildAt(0);
        if (content == null)
        {
            // no content, no shadows
            return;
        }

        if (_shadowView == null)
        {
            _shadowView = new ShadowView(Context, content, Context.ToPixels(shadowsView.CornerRadius), shadowsView.AndroidBlurType);
            _shadowView.UpdateShades(shadowsView.Shades);

            shadowsView.WeakCollectionChanged += _shadowView.ShadesSourceCollectionChanged;

            PlatformView.AddView(_shadowView, 0);

            InternalLogger.Debug(_tag, () => "ShadowView created and added");
        }
    }

    private void OnPlatformViewLayoutChange(object? sender, global::Android.Views.View.LayoutChangeEventArgs e)
    {
        var platformView = PlatformView;

        InternalLogger.Debug(_tag, () => $"OnLayoutChange( {e.Left}, {e.Top}, {e.Right}, {e.Bottom} )");

        var children = platformView.GetChildAt(1);
        if (children == null)
        {
            return;
        }

        // _shadowView?.Layout(children.MeasuredWidth, children.MeasuredHeight);

        _shadowView?.Layout(e.Right - e.Left, e.Bottom - e.Top);
    }
}
