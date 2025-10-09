using Android.Graphics;
using Android.Renderscripts;

namespace Sharpnado.Shades.Platforms.Android;

/// <summary>
/// ShadowView partial class containing blur algorithm implementations.
/// </summary>
public partial class ShadowView
{
    /// <summary>
    /// Applies RenderEffect GPU-accelerated blur.
    /// Requires Android 12+ (API 31) and hardware acceleration.
    /// Modifies the source bitmap in-place for optimal memory usage.
    /// </summary>
    private Bitmap ApplyBlurWithRenderEffect(Bitmap source, float blurRadius)
    {
        // Clamp blur radius to reasonable values
        float effectiveBlurRadius = Math.Min(blurRadius, MaxRadius);

        // Create temporary output bitmap
        var output = Bitmap.CreateBitmap(
            source.Width,
            source.Height,
            Bitmap.Config.Argb8888)!;

        // Create a RenderNode to apply the blur effect
        using var renderNode = new RenderNode("BlurNode");
        renderNode.SetPosition(0, 0, source.Width, source.Height);

        // Create the blur effect
        using var blurEffect = RenderEffect.CreateBlurEffect(
            effectiveBlurRadius,
            effectiveBlurRadius,
            Shader.TileMode.Clamp!);

        // Apply the effect to the RenderNode
        renderNode.SetRenderEffect(blurEffect);

        // Begin recording to draw into the RenderNode
        using var recordingCanvas = renderNode.BeginRecording();
        recordingCanvas.DrawBitmap(source, 0, 0, null);
        renderNode.EndRecording();

        // Create a hardware canvas to render the RenderNode
        using var outputCanvas = new Canvas(output);
        outputCanvas.DrawRenderNode(renderNode);

        // Copy blurred result back to source bitmap (like RenderScript does)
        using var canvas = new Canvas(source);
        canvas.DrawColor(global::Android.Graphics.Color.Transparent, PorterDuff.Mode.Clear);
        canvas.DrawBitmap(output, 0, 0, null);
        output.Recycle();
        output.Dispose();

        return source;
    }

    /// <summary>
    /// Applies RenderScript blur (for older devices).
    /// Deprecated API but still works on devices older than Android 12.
    /// </summary>
    private Bitmap ApplyBlurWithRenderScript(Bitmap source, float blurRadius)
    {
        if (_renderScript == null)
        {
            return source;
        }

        // RenderScript blur radius is half of the visual blur radius
        blurRadius *= 2;
        const int MaxBlur = 25;
        float blurAmount = blurRadius > MaxRadius ? MaxRadius : blurRadius;

        while (blurAmount > 0)
        {
            Allocation input = Allocation.CreateFromBitmap(
                _renderScript,
                source,
                Allocation.MipmapControl.MipmapNone,
                AllocationUsage.Script)!;
            Allocation output = Allocation.CreateTyped(_renderScript, input.Type)!;
            ScriptIntrinsicBlur script = ScriptIntrinsicBlur.Create(_renderScript, global::Android.Renderscripts.Element.U8_4(_renderScript))!;

            float effectiveBlurRadius;
            if (blurAmount > MaxBlur)
            {
                effectiveBlurRadius = MaxBlur;
                blurAmount -= MaxBlur;
            }
            else
            {
                effectiveBlurRadius = blurAmount;
                blurAmount = 0;
            }

            script.SetRadius(effectiveBlurRadius);
            script.SetInput(input);
            script.ForEach(output);
            output.CopyTo(source);
        }

        return source;
    }

    /// <summary>
    /// Applies StackBlur algorithm to the bitmap.
    /// This is a CPU-based blur implementation that works on all Android versions.
    /// Used as fallback for devices older than Android 12 (API 31) or when forced via BlurType.
    /// Modifies the source bitmap in-place for optimal memory usage.
    /// </summary>
    private Bitmap ApplyBlurWithStackBlur(Bitmap source, float blurRadius)
    {
        // Clamp blur radius to reasonable values
        int effectiveBlurRadius = (int)Math.Round(Math.Min(blurRadius, MaxRadius));
        
        if (effectiveBlurRadius < 1)
        {
            return source;
        }

        // Store original premultiplied state
        bool wasPremultiplied = source.IsPremultiplied;
        
        // Disable premultiplied alpha to get true RGB colors for blurring
        // This prevents color bleeding and preserves color vibrancy
        source.SetPremultiplied(false);

        // Apply the optimized StackBlur algorithm in-place
        ApplyStackBlur(source, source, effectiveBlurRadius);
        
        // Restore premultiplied state
        source.SetPremultiplied(wasPremultiplied);
        
        return source;
    }
}
