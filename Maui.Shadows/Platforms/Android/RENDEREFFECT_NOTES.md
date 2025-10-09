# Android Shadow Rendering Implementation

## Overview

The Android implementation of `ShadowView` now supports two rendering approaches for blur effects:

1. **RenderEffect** (API 31+, Android 12+) - Modern approach
2. **RenderScript** (API < 31) - Legacy approach for older devices

## Implementation Details

### API Level Detection

The implementation automatically detects the Android API level and chooses the appropriate rendering method:

```csharp
_useRenderEffect = AndroidOS.Build.VERSION.SdkInt >= AndroidOS.BuildVersionCodes.S;
```

- **API 31+ (Android 12+)**: Uses `RenderEffect` with `BlurMaskFilter`
- **API < 31**: Uses deprecated `RenderScript` for backwards compatibility

### RenderEffect Implementation (API 31+)

For Android 12 and later, shadows are rendered using `RenderNode` with `RenderEffect`:

```csharp
using var renderNode = new global::Android.Graphics.RenderNode("BlurNode");
renderNode.SetPosition(0, 0, source.Width, source.Height);

using var blurEffect = global::Android.Graphics.RenderEffect.CreateBlurEffect(
    effectiveBlurRadius,
    effectiveBlurRadius,
    global::Android.Graphics.Shader.TileMode.Clamp);

renderNode.SetRenderEffect(blurEffect);
using var recordingCanvas = renderNode.BeginRecording();
recordingCanvas.DrawBitmap(source, 0, 0, null);
renderNode.EndRecording();

using var outputCanvas = new Canvas(output);
outputCanvas.DrawRenderNode(renderNode);
```

**Benefits:**
- Modern Android API that's actively maintained
- Hardware-accelerated blur on supported devices
- Better performance on newer devices with GPU acceleration
- Avoids deprecated RenderScript warnings

**Fallback:** When hardware acceleration is not available or RenderNode fails, the implementation automatically falls back to the CPU-based StackBlur algorithm.

### RenderScript Implementation (Legacy)

For devices running Android 11 and earlier, the implementation continues to use `RenderScript`:

```csharp
ScriptIntrinsicBlur script = ScriptIntrinsicBlur.Create(_renderScript, AndroidElement.U8_4(_renderScript));
script.SetRadius(effectiveBlurRadius);
script.SetInput(input);
script.ForEach(output);
```

**Characteristics:**
- Uses iterative blur passes for large blur radii (max 25 per pass)
- Deprecated but still functional on older Android versions
- Automatically cleaned up on disposal

### StackBlur Algorithm

When RenderEffect is unavailable (non-hardware accelerated canvas) or on API < 31, the implementation uses the StackBlur algorithm:

```csharp
ApplyStackBlur(source, output, (int)Math.Round(effectiveBlurRadius));
```

**Characteristics:**
- Fast CPU-based blur implementation by Mario Klingemann
- Works on all Android versions
- Good performance without GPU acceleration
- Produces high-quality blur results
- Can optionally run on a background thread for better responsiveness

**Benefits:**
- No GPU requirements
- Consistent results across all devices
- Better quality than BlurMaskFilter
- Same algorithm used in Sharpnado.MaterialFrame

## Performance Considerations

- **RenderEffect (API 31+)**: Fastest on modern devices with hardware acceleration
- **StackBlur**: Excellent CPU-based fallback, good performance on all devices
- **RenderScript (API < 31)**: Mature implementation optimized over many Android versions
- All implementations use bitmap caching to avoid re-rendering unchanged shadows

## Logging

The implementation logs which rendering method is being used:

```
ShadowView(): X instances, using RenderEffect
```
or
```
ShadowView(): X instances, using RenderScript
```

Enable debug logging in `MauiProgram.cs`:

```csharp
.UseSharpnadoShadows(
    enableLogging: true,
    enableDebugLogging: true,
    logFilter: "ShadowsHandler")
```

## Migration Path

This implementation provides a smooth migration path from RenderScript to modern Android APIs:

1. **Now**: Supports both RenderScript (all devices) and RenderEffect (API 31+)
2. **Future**: When minimum API level is raised to 31+, RenderScript code can be removed
3. **Long-term**: Can adopt newer blur APIs as they become available (e.g., API 33+ features)

## Testing Recommendations

Test shadow rendering on:
- ✅ Android 12+ devices (API 31+) - RenderEffect path
- ✅ Android 11 devices (API 30) - RenderScript path
- ✅ Various shadow configurations (colors, blur radii, offsets)
- ✅ Dynamic shadow changes (adding/removing shades)
- ✅ Performance with multiple shadows

## Known Limitations

1. **RenderEffect bitmap blur** requires hardware acceleration (automatically falls back to StackBlur if not available)
2. **StackBlur** is CPU-intensive for large blur radii - consider running on background thread for large shadows
3. **Maximum blur radius** is clamped to 100 pixels to maintain performance across all implementations

## References

- [Android RenderEffect Documentation](https://developer.android.com/reference/android/graphics/RenderEffect)
- [Android RenderNode Documentation](https://developer.android.com/reference/android/graphics/RenderNode)
- [RenderScript Deprecation](https://developer.android.com/guide/topics/renderscript/migrate)
- [Stack Blur Algorithm by Mario Klingemann](http://www.quasimondo.com/StackBlurForCanvas/StackBlurDemo.html)
