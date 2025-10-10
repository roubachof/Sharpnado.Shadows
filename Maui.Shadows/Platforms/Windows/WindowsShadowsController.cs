using System.Collections.Specialized;
using System.Numerics;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Shapes;

namespace Sharpnado.Shades.Platforms.Windows;

/// <summary>
/// Controller that manages shadow visuals for Windows views using Composition API.
/// </summary>
internal partial class WindowsShadowsController : IDisposable
{
    private const string LogTag = nameof(WindowsShadowsController);
    private const float SafeMargin = 1;

    private readonly Canvas _shadowsCanvas;
    private readonly FrameworkElement _shadowSource;
    private readonly List<SpriteVisual> _shadowVisuals = new();

    private float _cornerRadius;
    private IEnumerable<Shade>? _shadesSource;
    private bool _isDisposed;
    private Compositor? _compositor;

    public WindowsShadowsController(Canvas shadowCanvas, FrameworkElement shadowSource, float cornerRadius)
    {
        _shadowsCanvas = shadowCanvas;
        _shadowSource = shadowSource;
        _cornerRadius = cornerRadius;

        _shadowSource.SizeChanged += ShadowSourceSizeChanged;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            InternalLogger.Debug(LogTag, "Dispose()");

            // Unsubscribe from size changed event to prevent memory leak
            _shadowSource.SizeChanged -= ShadowSourceSizeChanged;

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
        try
        {
            InternalLogger.Debug(LogTag, $"DestroyShadow( shadowIndex: {shadowIndex} )");

            var shadowHost = _shadowsCanvas.Children[shadowIndex];
            var visual = _shadowVisuals[shadowIndex];
            ElementCompositionPreview.SetElementChildVisual(shadowHost, null);
            _shadowsCanvas.Children.RemoveAt(shadowIndex);
            _shadowVisuals.RemoveAt(shadowIndex);
            visual.Dispose();
        }
        catch (Exception e)
        {
            InternalLogger.Error(LogTag, $"An exception occurred while disposing Windows Shadows", e);
        }
    }

    private void DestroyShadows()
    {
        InternalLogger.Debug(LogTag, "DestroyShadows()");
        for (int i = _shadowsCanvas.Children.Count - 1; i >= 0; i--)
        {
            DestroyShadow(i);
        }
    }

    private void InsertShade(int insertIndex, Shade shade)
    {
        shade.WeakPropertyChanged -= ShadePropertyChanged;
        InternalLogger.Debug(LogTag, () => $"InsertShade( insertIndex: {insertIndex}, shade: {shade} )");

        // https://docs.microsoft.com/windows/apps/desktop/composition/using-the-visual-layer-with-xaml

        var ttv = _shadowSource.TransformToVisual(_shadowsCanvas);
        global::Windows.Foundation.Point offset = ttv.TransformPoint(new global::Windows.Foundation.Point(0, 0));

        double width = _shadowSource.ActualWidth;
        double height = _shadowSource.ActualHeight;

        var shadowHost = new Rectangle()
        {
            Fill = Colors.White.ToBrush(),
            Width = width,
            Height = height,
            RadiusX = _cornerRadius,
            RadiusY = _cornerRadius,
        };

        Canvas.SetLeft(shadowHost, offset.X);
        Canvas.SetTop(shadowHost, offset.Y);

        _shadowsCanvas.Children.Insert(insertIndex, shadowHost);

        if (_compositor == null)
        {
            Visual hostVisual = ElementCompositionPreview.GetElementVisual(_shadowsCanvas);
            _compositor = hostVisual.Compositor;
        }

        var dropShadow = _compositor.CreateDropShadow();
        dropShadow.BlurRadius = (float)shade.BlurRadius * 2;
        dropShadow.Opacity = (float)shade.Opacity;
        dropShadow.Color = shade.Color.ToWindowsColor();
        dropShadow.Offset = new Vector3((float)shade.Offset.X - SafeMargin, (float)shade.Offset.Y - SafeMargin, 0);
        dropShadow.Mask = shadowHost.GetAlphaMask();

        var shadowVisual = _compositor.CreateSpriteVisual();
        shadowVisual.Size = new Vector2((float)width, (float)height);
        shadowVisual.Shadow = dropShadow;

        _shadowVisuals.Insert(insertIndex, shadowVisual);

        ElementCompositionPreview.SetElementChildVisual(shadowHost, shadowVisual);
        shade.WeakPropertyChanged += ShadePropertyChanged;
    }

    private void ShadowSourceSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var ttv = _shadowSource.TransformToVisual(_shadowsCanvas);
        global::Windows.Foundation.Point offset = ttv.TransformPoint(new global::Windows.Foundation.Point(0, 0));
        double width = _shadowSource.ActualWidth;
        double height = _shadowSource.ActualHeight;

        if (width < 1 || height < 1)
        {
            return;
        }

        InternalLogger.Debug(
            LogTag,
            () => $"shadowSource: {{ Offset: {offset}, Size: {width}x{height}, Margin: {_shadowSource.Margin} }}");

        if (_shadesSource == null)
        {
            return;
        }

        int count = Math.Min(_shadowsCanvas.Children.Count, _shadowVisuals.Count);
        for (int i = 0; i < count; i++)
        {
            var shadowHost = (Rectangle)_shadowsCanvas.Children[i];
            var shadowVisual = _shadowVisuals[i];

            InternalLogger.Debug(
                LogTag,
                () => $"shadowHost: {{ Size: {shadowHost.ActualWidth}x{shadowHost.ActualHeight}, Margin: {shadowHost.Margin} }}");

            Canvas.SetLeft(shadowHost, offset.X + SafeMargin);
            Canvas.SetTop(shadowHost, offset.Y + SafeMargin);

            double newWidth = width - 2 * SafeMargin;
            double newHeight = height - 2 * SafeMargin;

            shadowHost.Width = newWidth;
            shadowHost.Height = newHeight;

            shadowVisual.Size = new Vector2((float)width, (float)height);
        }
    }

    private void UpdateShadeVisual(int index, Shade shade)
    {
        if (index < 0 || index >= _shadowVisuals.Count)
        {
            return;
        }

        var dropShadow = (DropShadow?)_shadowVisuals[index].Shadow;
        if (dropShadow != null)
        {
            dropShadow.BlurRadius = (float)shade.BlurRadius;
            dropShadow.Opacity = (float)shade.Opacity;
            dropShadow.Color = shade.Color.ToWindowsColor();
            dropShadow.Offset = new Vector3((float)shade.Offset.X, (float)shade.Offset.Y, 0);
        }
    }
}
