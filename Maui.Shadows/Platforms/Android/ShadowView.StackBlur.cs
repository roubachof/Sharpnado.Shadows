using Android.Graphics;

namespace Sharpnado.Shades.Platforms.Android;

/// <summary>
/// ShadowView partial class containing the StackBlur algorithm implementation.
/// </summary>
public partial class ShadowView
{
    /// <summary>
    /// Stack Blur Algorithm by Mario Klingemann.
    /// Fast CPU-based blur implementation that works on all Android versions.
    /// Optimized with double buffering to reduce memory allocations.
    /// 
    /// This algorithm provides a good approximation of Gaussian blur with O(n) complexity,
    /// making it much faster than true Gaussian blur for large radii.
    /// </summary>
    /// <param name="input">The source bitmap to blur.</param>
    /// <param name="output">The destination bitmap for the blurred result.</param>
    /// <param name="radius">The blur radius (higher values = more blur).</param>
    private static void ApplyStackBlur(Bitmap input, Bitmap output, int radius)
    {
        if (radius < 1)
        {
            // Copy input to output without blur
            using var canvas = new Canvas(output);
            canvas.DrawBitmap(input, 0, 0, null);
            return;
        }

        int width = input.Width;
        int height = input.Height;
        int wh = width * height;

        // Double buffering: use two pixel buffers and swap between them
        // This reduces memory usage by 67% compared to separate RGBA channel arrays
        int[] pixels1 = new int[wh];
        int[] pixels2 = new int[wh];

        input.GetPixels(pixels1, 0, width, 0, 0, width, height);

        int wm = width - 1;
        int hm = height - 1;
        int div = radius + radius + 1;

        int[] vmin = new int[Math.Max(width, height)];

        // Pre-calculate division lookup table for performance
        int divsum = (div + 1) >> 1;
        divsum *= divsum;
        int[] dv = new int[256 * divsum];
        for (int i = 0; i < 256 * divsum; i++)
        {
            dv[i] = (i / divsum);
        }

        // Stack for maintaining running blur window
        int[][] stack = new int[div][];
        for (int k = 0; k < div; k++)
        {
            stack[k] = new int[4]; // [R, G, B, A]
        }

        int r1 = radius + 1;

        // Horizontal blur pass: read from pixels1, write to pixels2
        int yw = 0;
        int yi = 0;
        for (int y = 0; y < height; y++)
        {
            int rinsum = 0, ginsum = 0, binsum = 0, ainsum = 0;
            int routsum = 0, goutsum = 0, boutsum = 0, aoutsum = 0;
            int rsum = 0, gsum = 0, bsum = 0, asum = 0;

            // Initialize blur kernel for this row
            for (int i = -radius; i <= radius; i++)
            {
                int p = pixels1[yi + Math.Min(wm, Math.Max(i, 0))];
                int[] sir = stack[i + radius];
                sir[0] = (p >> 16) & 0xff; // Red
                sir[1] = (p >> 8) & 0xff;  // Green
                sir[2] = p & 0xff;          // Blue
                sir[3] = (p >> 24) & 0xff; // Alpha
                int rbs = r1 - Math.Abs(i);
                rsum += sir[0] * rbs;
                gsum += sir[1] * rbs;
                bsum += sir[2] * rbs;
                asum += sir[3] * rbs;
                if (i > 0)
                {
                    rinsum += sir[0];
                    ginsum += sir[1];
                    binsum += sir[2];
                    ainsum += sir[3];
                }
                else
                {
                    routsum += sir[0];
                    goutsum += sir[1];
                    boutsum += sir[2];
                    aoutsum += sir[3];
                }
            }
            int stackpointer = radius;

            // Process each pixel in the row
            for (int x = 0; x < width; x++)
            {
                // Store intermediate blurred channels in pixels2
                pixels2[yi] = (dv[asum] << 24) | (dv[rsum] << 16) | (dv[gsum] << 8) | dv[bsum];

                rsum -= routsum;
                gsum -= goutsum;
                bsum -= boutsum;
                asum -= aoutsum;

                int stackstart = stackpointer - radius + div;
                int[] sir = stack[stackstart % div];

                routsum -= sir[0];
                goutsum -= sir[1];
                boutsum -= sir[2];
                aoutsum -= sir[3];

                if (y == 0)
                {
                    vmin[x] = Math.Min(x + radius + 1, wm);
                }
                int p = pixels1[yw + vmin[x]];

                sir[0] = (p >> 16) & 0xff;
                sir[1] = (p >> 8) & 0xff;
                sir[2] = p & 0xff;
                sir[3] = (p >> 24) & 0xff;

                rinsum += sir[0];
                ginsum += sir[1];
                binsum += sir[2];
                ainsum += sir[3];

                rsum += rinsum;
                gsum += ginsum;
                bsum += binsum;
                asum += ainsum;

                stackpointer = (stackpointer + 1) % div;
                sir = stack[stackpointer % div];

                routsum += sir[0];
                goutsum += sir[1];
                boutsum += sir[2];
                aoutsum += sir[3];

                rinsum -= sir[0];
                ginsum -= sir[1];
                binsum -= sir[2];
                ainsum -= sir[3];

                yi++;
            }
            yw += width;
        }

        // Vertical blur pass: read from pixels2, write back to pixels1
        for (int x = 0; x < width; x++)
        {
            int rinsum = 0, ginsum = 0, binsum = 0, ainsum = 0;
            int routsum = 0, goutsum = 0, boutsum = 0, aoutsum = 0;
            int rsum = 0, gsum = 0, bsum = 0, asum = 0;
            int yp = -radius * width;

            // Initialize blur kernel for this column
            for (int i = -radius; i <= radius; i++)
            {
                yi = Math.Max(0, yp) + x;

                int[] sir = stack[i + radius];
                int p = pixels2[yi];

                sir[0] = (p >> 16) & 0xff;
                sir[1] = (p >> 8) & 0xff;
                sir[2] = p & 0xff;
                sir[3] = (p >> 24) & 0xff;

                int rbs = r1 - Math.Abs(i);

                rsum += sir[0] * rbs;
                gsum += sir[1] * rbs;
                bsum += sir[2] * rbs;
                asum += sir[3] * rbs;

                if (i > 0)
                {
                    rinsum += sir[0];
                    ginsum += sir[1];
                    binsum += sir[2];
                    ainsum += sir[3];
                }
                else
                {
                    routsum += sir[0];
                    goutsum += sir[1];
                    boutsum += sir[2];
                    aoutsum += sir[3];
                }

                if (i < hm)
                {
                    yp += width;
                }
            }
            yi = x;
            int stackpointer = radius;

            // Process each pixel in the column
            for (int y = 0; y < height; y++)
            {
                pixels1[yi] = (dv[asum] << 24) | (dv[rsum] << 16) | (dv[gsum] << 8) | dv[bsum];

                rsum -= routsum;
                gsum -= goutsum;
                bsum -= boutsum;
                asum -= aoutsum;

                int stackstart = stackpointer - radius + div;
                int[] sir = stack[stackstart % div];

                routsum -= sir[0];
                goutsum -= sir[1];
                boutsum -= sir[2];
                aoutsum -= sir[3];

                if (x == 0)
                {
                    vmin[y] = Math.Min(y + r1, hm) * width;
                }
                int p = pixels2[x + vmin[y]];

                sir[0] = (p >> 16) & 0xff;
                sir[1] = (p >> 8) & 0xff;
                sir[2] = p & 0xff;
                sir[3] = (p >> 24) & 0xff;

                rinsum += sir[0];
                ginsum += sir[1];
                binsum += sir[2];
                ainsum += sir[3];

                rsum += rinsum;
                gsum += ginsum;
                bsum += binsum;
                asum += ainsum;

                stackpointer = (stackpointer + 1) % div;
                sir = stack[stackpointer];

                routsum += sir[0];
                goutsum += sir[1];
                boutsum += sir[2];
                aoutsum += sir[3];

                rinsum -= sir[0];
                ginsum -= sir[1];
                binsum -= sir[2];
                ainsum -= sir[3];

                yi += width;
            }
        }

        output.SetPixels(pixels1, 0, width, 0, 0, width, height);
    }
}
