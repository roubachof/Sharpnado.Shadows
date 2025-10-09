using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Android.Content;
using Android.Graphics;
using Android.Renderscripts;
using Android.Runtime;
using Android.Util;
using AndroidView = Android.Views.View;
using AndroidOS = Android.OS;

namespace Sharpnado.Shades.Platforms.Android;

/// <summary>
/// Custom Android view that renders shadows for MAUI views.
/// Supports multiple blur algorithms: RenderEffect (GPU), RenderScript, and StackBlur (CPU).
/// </summary>
public partial class ShadowView : AndroidView
{
    internal const int MinimumSize = 5;
    internal const int MaxRadius = 100;

    private static int instanceCount = 0;

    private readonly JniWeakReference<AndroidView> _weakSource;
    private readonly Dictionary<Shade, ShadeInfo> _shadeInfos;
    private readonly BitmapCache _cache;
    private readonly HashSet<string> _pendingBitmaps = new();
    private readonly bool _tryUseRenderEffect;
    private readonly object _pendingLock = new();
    private readonly RenderScript? _renderScript;

    private bool _isDisposed;
    private bool? _isHardwareAccelerated;
    private AndroidBlurType _androidBlurType;

    public ShadowView(Context context, AndroidView shadowSource, float cornerRadius, AndroidBlurType androidBlurType = AndroidBlurType.Gpu, string? tag = null)
        : base(context)
    {
        _androidBlurType = androidBlurType;
        
        // Use RenderEffect for API 31+ (Android 12+), fallback to StackBlur for older versions
        // However, if BlurType is explicitly set to StackBlur, we'll force that instead
        _tryUseRenderEffect = androidBlurType == AndroidBlurType.Gpu && AndroidOS.Build.VERSION.SdkInt >= AndroidOS.BuildVersionCodes.S;
        if (!_tryUseRenderEffect && androidBlurType == AndroidBlurType.Gpu)
        {
            // Initialize RenderScript for older Android versions (only when Gpu mode is requested)
            _renderScript = RenderScript.Create(context);
        }

        _weakSource = new JniWeakReference<AndroidView>(shadowSource);
        _cache = BitmapCache.Instance;
        _shadeInfos = new Dictionary<Shade, ShadeInfo>();
        _cornerRadius = cornerRadius;

        shadowSource.LayoutChange += OnSourceLayoutChanged;

        LogTag = !string.IsNullOrEmpty(tag) ? $"{nameof(ShadowView)}@{tag}" : nameof(ShadowView);
        InternalLogger.Debug(LogTag, () => $"ShadowView(): {++instanceCount} instances, BlurType: {_androidBlurType}");
    }

    public ShadowView(Context context, IAttributeSet attrs)
        : base(context, attrs)
    {
        LogTag = nameof(ShadowView);
    }

    public ShadowView(Context context, IAttributeSet attrs, int defStyleAttr)
        : base(context, attrs, defStyleAttr)
    {
        LogTag = nameof(ShadowView);
    }

    public ShadowView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes)
        : base(context, attrs, defStyleAttr, defStyleRes)
    {
        LogTag = nameof(ShadowView);
    }

    protected ShadowView(IntPtr javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
        LogTag = nameof(ShadowView);
    }

    public static Predicate<AndroidView> HasMinimumSize =>
        view => view.MeasuredWidth >= MinimumSize && view.MeasuredHeight >= MinimumSize;

    public string LogTag { get; }

    public void Layout(int width, int height)
    {
        if (width <= MinimumSize || height <= MinimumSize)
        {
            return;
        }

        InternalLogger.Debug(LogTag, () => $"Layout( width: {width}, height: {height} )");
        Measure(width, height);
        Layout(0, 0, width, height);
    }

    private void OnSourceLayoutChanged(object? sender, LayoutChangeEventArgs e)
    {
        int width = e.Right - e.Left;
        int height = e.Bottom - e.Top;

        int oldWidth = e.OldRight - e.OldLeft;
        int oldHeight = e.OldBottom - e.OldTop;

        if (width <= MinimumSize || height <= MinimumSize || _isDisposed)
        {
            return;
        }

        if (_weakSource.TryGetTarget(out var source) && (width != oldWidth || height != oldHeight))
        {
            InternalLogger.Debug(LogTag, () => $"OnSourceLayoutChanged( {source.Width}w, {source.Height}h )");
            RefreshBitmaps();
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        InternalLogger.Debug(LogTag, $"Dispose( disposing: {disposing} )");

        if (_shadesSource is INotifyCollectionChanged shadeNotifyCollection)
        {
            shadeNotifyCollection.CollectionChanged -= ShadesSourceCollectionChanged;
        }

        if (_weakSource.TryGetTarget(out var source))
        {
            source.LayoutChange -= OnSourceLayoutChanged;
        }

        _renderScript?.Dispose();

        DisposeBitmaps();
        _isDisposed = true;
    }

    protected override void OnDraw(Canvas? canvas)
    {
        if (canvas == null)
        {
            return;
        }

#if DEBUG
        var stopWatch = new Stopwatch();
        stopWatch.Start();
#endif
        if (!_weakSource.TryGetTarget(out var source))
        {
            return;
        }

        foreach (var shadeInfo in _shadeInfos.Values)
        {
            // Try to get the bitmap from cache, skip if not ready yet
            if (_cache.TryGet(shadeInfo.Hash, out var shadow))
            {
                float x = source.GetX() + shadeInfo.OffsetX - MaxRadius;
                float y = source.GetY() + shadeInfo.OffsetY - MaxRadius;

                InternalLogger.Debug(LogTag, () => $"OnDraw( {x}x, {y}y )");

                canvas.DrawBitmap(shadow, x, y, null);
            }
            else
            {
                InternalLogger.Debug(LogTag, () => $"OnDraw: bitmap not ready yet for {shadeInfo.Hash}");
            }
        }

        base.OnDraw(canvas);
#if DEBUG
        LogPerf(LogTag, stopWatch);
#endif
    }

    private static void LogPerf(string tag, Stopwatch stopwatch, [CallerMemberName] string methodName = "caller")
    {
        InternalLogger.Debug(tag, () => $"{methodName}: ran in {stopwatch.ElapsedMilliseconds:0000} ms");
    }
}
