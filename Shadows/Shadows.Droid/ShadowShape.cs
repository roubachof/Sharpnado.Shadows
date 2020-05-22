using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Renderscripts;

namespace Sharpnado.MaterialFrame.Droid.Shadows
{
    /// <summary>
    /// https://stackoverflow.com/questions/42002508/how-to-add-shadow-to-vector-drawable.
    /// </summary>
    public class ShadowShape : Shape
    {
        private readonly Context ctx;

        // private readonly Drawable source;

        // private readonly Drawable mask;

        private readonly Drawable shadow;

        private Matrix matrix = new Matrix();

        private Bitmap bitmap;

        private float shadowRadius;

        public ShadowShape(Context ctx, Drawable source, Drawable mask, Drawable shadow, float shadowRadius)
        {
            this.ctx = ctx;
            // this.source = source;
            //this.mask = mask;
            this.shadow = shadow;
            this.shadowRadius = shadowRadius;
        }

        public override void Draw(Canvas canvas, Paint paint)
        {
            // canvas.DrawColor(0xffdddddd);
            canvas.DrawBitmap(bitmap, matrix, null);
        }

        private Bitmap BlurRenderScript(Bitmap input)
        {
            Bitmap output = Bitmap.CreateBitmap(input.Width, input.Height, input.GetConfig());
            RenderScript rs = RenderScript.Create(ctx);
            ScriptIntrinsicBlur script = ScriptIntrinsicBlur.Create(rs, Element.U8_4(rs));
            Allocation inAlloc = Allocation.CreateFromBitmap(
                rs,
                input,
                Allocation.MipmapControl.MipmapNone,
                AllocationUsage.GraphicsTexture);
            Allocation outAlloc = Allocation.CreateFromBitmap(rs, output);
            script.SetRadius(shadowRadius);
            script.SetInput(inAlloc);
            script.ForEach(outAlloc);
            outAlloc.CopyTo(output);
            rs.Destroy();
            return output;
        }
    }
}