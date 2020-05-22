using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Renderscripts;
using Android.Runtime;
using Android.Util;
using Android.Views;

using Xamarin.Forms.Platform.Android;

namespace Sharpnado.Shades.Droid
{
    public partial class ShadowView : View
    {
        private const int MaxRadius = 25;

        private readonly JniWeakReference<View> _weakSource;
        private readonly RenderScript _renderScript;
        private readonly List<Bitmap> _shadesBitmaps;

        public ShadowView(Context context, View shadowSource, float cornerRadius)
            : base(context)
        {
            _renderScript = RenderScript.Create(context);
            _weakSource = new JniWeakReference<View>(shadowSource);

            _shadesBitmaps = new List<Bitmap>();
            _cornerRadius = cornerRadius;
        }

        public ShadowView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        public ShadowView(Context context, IAttributeSet attrs, int defStyleAttr)
            : base(context, attrs, defStyleAttr)
        {
        }

        public ShadowView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes)
            : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

        protected ShadowView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public void Layout(int width, int height)
        {
            if (width < 1 || height < 1)
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine($"ShadowView::Layout( width: {width}, height: {height} )");

            Measure(width, height);
            Layout(0, 0, width, height);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _renderScript.Destroy();

                DisposeBitmaps();
            }
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            if ((w != oldw || h != oldh) && _weakSource.TryGetTarget(out var source))
            {
                System.Diagnostics.Debug.WriteLine($"ShadowView::OnSizeChanged( {source.MeasuredWidth}w, {source.MeasuredHeight}h )");

                CreateAndDrawBitmaps();
            }
        }

        protected override void OnDraw(Canvas canvas)
        {
            if (!_weakSource.TryGetTarget(out var source))
            {
                return;
            }

            var immutableSource = _shadesSource.ToArray();
            if (immutableSource.Length != _shadesBitmaps.Count)
            {
                return;
            }

            for (int i = 0; i < immutableSource.Length; i++)
            {
                var info = ShadeInfo.FromShade(Context, immutableSource[i]);
                var shadow = _shadesBitmaps[i];

                float x = source.GetX() + info.OffsetX - MaxRadius;
                float y = source.GetY() + info.OffsetY - MaxRadius;

                System.Diagnostics.Debug.WriteLine($"ShadowView::OnDraw( {x}x, {y}y )");

                canvas.DrawBitmap(shadow, x, y, null);
            }

            base.OnDraw(canvas);
        }

        private void CreateAndDrawBitmaps()
        {
            DisposeBitmaps();

            if (!_weakSource.TryGetTarget(out var source) || source.MeasuredHeight < 1 || source.MeasuredWidth < 1)
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine($"ShadowView::CreateBitmaps()");
            var immutableSource = _shadesSource.ToArray();
            for (int i = 0; i < immutableSource.Length; i++)
            {
                CreateBitmap(i);
            }

            DrawBitmaps(immutableSource);
        }

        private void CreateBitmap(int shadeInfoIndex)
        {
            if (!_weakSource.TryGetTarget(out var source) || source.MeasuredHeight < 1 || source.MeasuredWidth < 1)
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine($"ShadowView::CreateBitmaps( shadeInfoIndex: {shadeInfoIndex}, sourceWidth: {source.MeasuredWidth}, sourceHeight: {source.MeasuredHeight})");

            _shadesBitmaps.Insert(
                shadeInfoIndex,
                Bitmap.CreateBitmap(
                    source.MeasuredWidth + 2 * MaxRadius,
                    source.MeasuredHeight + 2 * MaxRadius,
                    Bitmap.Config.Argb8888));
        }

        private void DrawBitmap(int shadeInfoIndex, Shade shade)
        {
            System.Diagnostics.Debug.WriteLine($"ShadowView::DrawBitmap( shadeInfoIndex: {shadeInfoIndex})");
            if (!_weakSource.TryGetTarget(out var source))
            {
                return;
            }

            if (shadeInfoIndex >= _shadesBitmaps.Count)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"ShadowView::DrawBitmap => Couldn't find a bitmap at index {shadeInfoIndex}, possibly CreateBitmap wasn't run because the measurement has not have been made yet");
                return;
            }

            var info = ShadeInfo.FromShade(Context, shade);
            var shadow = _shadesBitmaps[shadeInfoIndex];

            shadow.EraseColor(Color.Transparent);

            System.Diagnostics.Debug.WriteLine($"ShadowView::DrawBitmap( shadeInfoIndex: {shadeInfoIndex}, sourceWidth: {source.MeasuredWidth}, sourceHeight: {source.MeasuredHeight}, bitmapWidth: {shadow.Width}, bitmapHeight: {shadow.Height})");

            using var bitmapCanvas = new Canvas(shadow);
            using var paint = new Paint { Color = info.Color };
            bitmapCanvas.DrawRoundRect(
                MaxRadius,
                MaxRadius,
                source.MeasuredWidth + MaxRadius,
                source.MeasuredHeight + MaxRadius,
                _cornerRadius,
                _cornerRadius,
                paint);

            if (info.BlurRadius < 1)
            {
                return;
            }

            Allocation input = Allocation.CreateFromBitmap(
                _renderScript,
                shadow,
                Allocation.MipmapControl.MipmapNone,
                AllocationUsage.Script);
            Allocation output = Allocation.CreateTyped(_renderScript, input.Type);
            ScriptIntrinsicBlur script = ScriptIntrinsicBlur.Create(_renderScript, Element.U8_4(_renderScript));

            script.SetRadius(info.BlurRadius > 25 ? 25 : info.BlurRadius);
            script.SetInput(input);
            script.ForEach(output);
            output.CopyTo(shadow);
        }

        private void DrawBitmaps(Shade[] immutableSource)
        {
            System.Diagnostics.Debug.WriteLine($"ShadowView::DrawBitmaps()");
            for (int i = 0; i < immutableSource.Length; i++)
            {
                DrawBitmap(i, immutableSource[i]);
            }
        }

        private void DisposeBitmap(int index)
        {
            var bitmap = _shadesBitmaps[index];
            _shadesBitmaps.RemoveAt(index);
            bitmap.Recycle();
            bitmap.Dispose();
        }

        private void DisposeBitmaps()
        {
            if (_shadesBitmaps.Count == 0)
            {
                return;
            }

            foreach (var bitmap in _shadesBitmaps)
            {
                bitmap.Recycle();
                bitmap.Dispose();
            }

            _shadesBitmaps.Clear();
        }

        private struct ShadeInfo
        {
            private ShadeInfo(Color color, float blurRadius, float offsetX, float offsetY)
            {
                Color = color;
                BlurRadius = blurRadius;
                OffsetX = offsetX;
                OffsetY = offsetY;
            }

            public Color Color { get; }

            public float BlurRadius { get; }

            public float OffsetX { get; }

            public float OffsetY { get; }

            public static ShadeInfo FromShade(Context context, Shade shade)
            {
                return new ShadeInfo(
                    shade.Color.ToAndroid(),
                    (float)shade.BlurRadius,
                    context.ToPixels(shade.Offset.X),
                    context.ToPixels(shade.Offset.Y));
            }
        }
    }
}