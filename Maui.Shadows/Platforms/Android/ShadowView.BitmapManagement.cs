using System.Diagnostics;
using Android.Graphics;
using AndroidRectF = Android.Graphics.RectF;
using AndroidPaint = Android.Graphics.Paint;

namespace Sharpnado.Shades.Platforms.Android;

/// <summary>
/// ShadowView partial class containing bitmap lifecycle and cache management.
/// </summary>
public partial class ShadowView
{
    private void RefreshBitmaps()
    {
#if DEBUG
        var stopWatch = new Stopwatch();
        stopWatch.Start();
#endif
        DisposeBitmaps();

        if (!_weakSource.TryGetTarget(out var source) || !HasMinimumSize(source))
        {
            return;
        }

        InternalLogger.Debug(LogTag, "RefreshBitmaps()");
        if (_shadesSource != null)
        {
            foreach (var shade in _shadesSource)
            {
                InsertBitmap(shade);
            }
        }
#if DEBUG
        LogPerf(LogTag, stopWatch);
#endif
    }

    private void RefreshBitmap(Shade shade)
    {
#if DEBUG
        var stopWatch = new Stopwatch();
        stopWatch.Start();
#endif
        DisposeBitmaps();

        if (!_weakSource.TryGetTarget(out var source) || !HasMinimumSize(source))
        {
            return;
        }

        InternalLogger.Debug(LogTag, $"RefreshBitmap( shade: {shade} )");
        if (_shadeInfos.TryGetValue(shade, out var shadeInfo))
        {
            _shadeInfos.Remove(shade);
            _cache.Remove(shadeInfo.Hash);
        }

        InsertBitmap(shade);
#if DEBUG
        LogPerf(LogTag, stopWatch);
#endif
    }

    private void InsertBitmap(Shade shade)
    {
        if (!_weakSource.TryGetTarget(out var source)
            || !HasMinimumSize(source)
            || Context == null)
        {
            return;
        }
#if DEBUG
        var stopWatch = new Stopwatch();
        stopWatch.Start();
#endif
        InternalLogger.Debug(LogTag, () => $"InsertBitmap( shade: {shade}, sourceWidth: {source.MeasuredWidth}, sourceHeight: {source.MeasuredHeight})");

        var shadeInfo = ShadeInfo.FromShade(Context, shade, _cornerRadius, source);
        _shadeInfos.Add(shade, shadeInfo);

        // Check if bitmap is already in cache or being created
        if (!_cache.Contains(shadeInfo.Hash))
        {
            lock (_pendingLock)
            {
                if (_pendingBitmaps.Add(shadeInfo.Hash))
                {
                    // Start async bitmap creation
                    _ = CreateBitmapAsync(shadeInfo);
                }
            }
        }
        else
        {
            _cache.IncrementReferenceCount(shadeInfo.Hash);
        }
#if DEBUG
        LogPerf(LogTag, stopWatch);
#endif
    }

    private async Task CreateBitmapAsync(ShadeInfo shadeInfo)
    {
        try
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            InternalLogger.Debug(LogTag, () => $"CreateBitmapAsync( shadeInfo: {shadeInfo} ) - starting");

            var shadow = Bitmap.CreateBitmap(
                shadeInfo.Width,
                shadeInfo.Height,
                Bitmap.Config.Argb8888);

            // Check hardware acceleration on first draw
            if (!_isHardwareAccelerated.HasValue)
            {
                using var testCanvas = new Canvas(shadow);
                _isHardwareAccelerated = testCanvas.IsHardwareAccelerated;
                InternalLogger.Debug(LogTag, () => $"Hardware acceleration detected: {_isHardwareAccelerated}");
            }

            // RenderEffect requires hardware acceleration and must run on UI thread
            // StackBlur is CPU-intensive and can run on background thread
            if (_tryUseRenderEffect && _isHardwareAccelerated == true)
            {
                // RenderEffect needs UI thread - run synchronously
                shadow = CreateBitmapSync(shadeInfo, shadow);
            }
            else
            {
                // StackBlur & Renderscript can run on background thread
                shadow = await Task.Run(() => CreateBitmapSync(shadeInfo, shadow));
            }

            // Add to cache on completion
            _cache.Add(shadeInfo.Hash, () => shadow);

            // Mark as no longer pending
            lock (_pendingLock)
            {
                _pendingBitmaps.Remove(shadeInfo.Hash);
            }

            // Invalidate on UI thread to trigger redraw
            PostInvalidate();

#if DEBUG
            LogPerf(LogTag, stopWatch);
#endif
            InternalLogger.Debug(LogTag, () => $"CreateBitmapAsync( shadeInfo: {shadeInfo} ) - completed");
        }
        catch (Exception ex)
        {
            InternalLogger.Debug(LogTag, () => $"CreateBitmapAsync failed: {ex.Message}");
            lock (_pendingLock)
            {
                _pendingBitmaps.Remove(shadeInfo.Hash);
            }
        }
    }

    private Bitmap CreateBitmapSync(ShadeInfo shadeInfo, Bitmap shadow)
    {
        InternalLogger.Debug(LogTag, () => $"CreateBitmapSync( shadeInfo: {shadeInfo} )");
        AndroidRectF rect = new AndroidRectF(
            ShadeInfo.Padding,
            ShadeInfo.Padding,
            shadeInfo.Width - ShadeInfo.Padding,
            shadeInfo.Height - ShadeInfo.Padding);

        using var bitmapCanvas = new Canvas(shadow);
        using var paint = new AndroidPaint { Color = shadeInfo.Color };
        bitmapCanvas.DrawRoundRect(
            rect,
            _cornerRadius,
            _cornerRadius,
            paint);

        if (shadeInfo.BlurRadius < 1)
        {
            return shadow;
        }

        // If BlurType is explicitly set to StackBlur, force that algorithm
        if (_androidBlurType == AndroidBlurType.StackBlur)
        {
            InternalLogger.Debug(LogTag, () => $"Blurring with StackBlur (CPU) - forced by BlurType setting");
            return ApplyBlurWithStackBlur(shadow, shadeInfo.BlurRadius);
        }

        // Otherwise, use the automatic GPU/CPU selection based on device capabilities
        InternalLogger.Debug(LogTag, () => $"_useRenderEffect: {_tryUseRenderEffect}, _isHardwareAccelerated: {_isHardwareAccelerated}");
        if (_tryUseRenderEffect && _isHardwareAccelerated == true)
        {
            InternalLogger.Debug(LogTag, () => $"Blurring with RenderEffect (GPU)");
            // Use RenderEffect for API 31+ (Android 12+) with hardware acceleration
            return ApplyBlurWithRenderEffect(shadow, shadeInfo.BlurRadius);
        }

        if (_renderScript != null)
        {
            InternalLogger.Debug(LogTag, () => $"Blurring with RenderScript (deprecated, CPU)");
            // Use RenderScript for older Android versions (deprecated but still works)
            return ApplyBlurWithRenderScript(shadow, shadeInfo.BlurRadius);
        }

        InternalLogger.Debug(LogTag, () => $"Blurring with StackBlur (CPU) - fallback");
        // Use StackBlur for older Android versions or when hardware acceleration is unavailable
        return ApplyBlurWithStackBlur(shadow, shadeInfo.BlurRadius);
    }

    private void DisposeBitmap(Shade shade)
    {
#if DEBUG
        var stopWatch = new Stopwatch();
        stopWatch.Start();
#endif
        InternalLogger.Debug(LogTag, () => $"DisposeBitmap( shade: {shade} )");
        var shadeInfo = _shadeInfos[shade];
        _shadeInfos.Remove(shade);

        _cache.Remove(shadeInfo.Hash);
#if DEBUG
        LogPerf(LogTag, stopWatch);
#endif
    }

    private void DisposeBitmaps()
    {
        if (_shadeInfos.Count == 0)
        {
            return;
        }
#if DEBUG
        var stopWatch = new Stopwatch();
        stopWatch.Start();
#endif
        InternalLogger.Debug(LogTag, () => $"DisposeBitmaps()");
        foreach (var shadeInfo in _shadeInfos.Values)
        {
            _cache.Remove(shadeInfo.Hash);
        }

        _shadeInfos.Clear();
#if DEBUG
        LogPerf(LogTag, stopWatch);
#endif
    }

    private void UpdateShadeInfo(Shade shade)
    {
        if (!_weakSource.TryGetTarget(out var source) || Context == null)
        {
            return;
        }

        InternalLogger.Debug(LogTag, () => $"UpdateShadeInfo( shade: {shade} )");
        _shadeInfos[shade] = ShadeInfo.FromShade(Context, shade, _cornerRadius, source);
    }
}
