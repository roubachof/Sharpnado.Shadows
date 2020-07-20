using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

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
        private const int MinimumSize = 5;
        private const int MaxRadius = 100;

        private const string LogTag = nameof(ShadowView);

        private readonly JniWeakReference<View> _weakSource;
        private readonly RenderScript _renderScript;
        private readonly List<Bitmap> _shadesBitmaps;

        private bool _isDisposed;

        private int _lastSourceWidth = 0;
        private int _lastSourceHeight = 0;

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

        private static Predicate<View> HasMinimumSize =>
            view => view.MeasuredWidth >= MinimumSize && view.MeasuredHeight >= 5;

        private Predicate<View> SourceSizeChanged =>
            view => view.MeasuredWidth != _lastSourceWidth || view.MeasuredHeight != _lastSourceHeight;

        private bool ShouldDrawBitmaps =>
            _shadesBitmaps.Count != _shadesSource.Count()
            || (_weakSource.TryGetTarget(out var source) && SourceSizeChanged(source));

        public void Layout(int width, int height)
        {
            if (width <= MinimumSize || height <= MinimumSize)
            {
                return;
            }

            InternalLogger.Debug(LogTag, () => $"Layout( width: {width}, height: {height} )");

            Measure(width, height);
            Layout(0, 0, width, height);

            if (ShouldDrawBitmaps)
            {
                CreateAndDrawBitmaps();
                Invalidate();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                InternalLogger.Debug(LogTag, "Dispose()");

                if (_shadesSource is INotifyCollectionChanged shadeNotifyCollection)
                {
                    shadeNotifyCollection.CollectionChanged -= ShadesSourceCollectionChanged;
                }

                _renderScript.Destroy();

                UnsubscribeAllShades();
                DisposeBitmaps();

                _isDisposed = true;
            }
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            if (_weakSource.TryGetTarget(out var source) && (ShouldDrawBitmaps || (w != oldw || h != oldh)))
            {
                InternalLogger.Debug(LogTag, () => $"OnSizeChanged( {source.MeasuredWidth}w, {source.MeasuredHeight}h )");

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

                InternalLogger.Debug(LogTag, () => $"OnDraw( {x}x, {y}y )");

                canvas.DrawBitmap(shadow, x, y, null);
            }

            base.OnDraw(canvas);
        }

        private void CreateAndDrawBitmaps()
        {
            DisposeBitmaps();

            if (!_weakSource.TryGetTarget(out var source) || !HasMinimumSize(source))
            {
                return;
            }

            InternalLogger.Debug(LogTag, "CreateAndDrawBitmaps()");
            var immutableSource = _shadesSource.ToArray();
            for (int i = 0; i < immutableSource.Length; i++)
            {
                CreateBitmap(i);
            }

            DrawBitmaps(immutableSource);
        }

        private void CreateBitmap(int shadeInfoIndex)
        {
            if (!_weakSource.TryGetTarget(out var source) || !HasMinimumSize(source))
            {
                return;
            }

            _lastSourceWidth = source.MeasuredWidth;
            _lastSourceHeight = source.MeasuredHeight;

            InternalLogger.Debug(LogTag, () => $"CreateBitmaps( shadeInfoIndex: {shadeInfoIndex}, sourceWidth: {_lastSourceWidth}, sourceHeight: {_lastSourceHeight})");

            _shadesBitmaps.Insert(
                shadeInfoIndex,
                Bitmap.CreateBitmap(
                    _lastSourceWidth + 2 * MaxRadius,
                    _lastSourceHeight + 2 * MaxRadius,
                    Bitmap.Config.Argb8888));
        }

        private void DrawBitmap(int shadeInfoIndex, Shade shade)
        {
            InternalLogger.Debug(LogTag, () => $"DrawBitmap( shadeInfoIndex: {shadeInfoIndex})");
            if (!_weakSource.TryGetTarget(out var source))
            {
                return;
            }

            if (shadeInfoIndex >= _shadesBitmaps.Count)
            {
                InternalLogger.Warn(
                    LogTag, $"DrawBitmap => Couldn't find a bitmap at index {shadeInfoIndex}, possibly CreateBitmap wasn't run because the measurement has not have been made yet");
                return;
            }

            var info = ShadeInfo.FromShade(Context, shade);
            var shadow = _shadesBitmaps[shadeInfoIndex];

            shadow.EraseColor(Color.Transparent);

            InternalLogger.Debug(
                LogTag,
                () =>
                    $"DrawBitmap( shadeInfoIndex: {shadeInfoIndex}, sourceWidth: {source.MeasuredWidth}, sourceHeight: {source.MeasuredHeight}, bitmapWidth: {shadow.Width}, bitmapHeight: {shadow.Height})");
            InternalLogger.Debug(LogTag, () => info.ToString());
            RectF rect = new RectF(
                MaxRadius,
                MaxRadius,
                source.MeasuredWidth + MaxRadius,
                source.MeasuredHeight + MaxRadius);

            using var bitmapCanvas = new Canvas(shadow);
            using var paint = new Paint { Color = info.Color };
            bitmapCanvas.DrawRoundRect(
                rect,
                _cornerRadius,
                _cornerRadius,
                paint);

            if (info.BlurRadius < 1)
            {
                return;
            }

            const int MaxBlur = 25;
            float blurAmount = info.BlurRadius > MaxRadius ? MaxRadius : info.BlurRadius;
            while (blurAmount > 0)
            {
                Allocation input = Allocation.CreateFromBitmap(
                    _renderScript,
                    shadow,
                    Allocation.MipmapControl.MipmapNone,
                    AllocationUsage.Script);
                Allocation output = Allocation.CreateTyped(_renderScript, input.Type);
                ScriptIntrinsicBlur script = ScriptIntrinsicBlur.Create(_renderScript, Element.U8_4(_renderScript));

                float blurRadius;
                if (blurAmount > MaxBlur)
                {
                    blurRadius = MaxBlur;
                    blurAmount -= MaxBlur;
                }
                else
                {
                    blurRadius = blurAmount;
                    blurAmount = 0;
                }

                script.SetRadius(blurRadius);
                script.SetInput(input);
                script.ForEach(output);
                output.CopyTo(shadow);
            }
        }

        private void DrawBitmaps(Shade[] immutableSource)
        {
            InternalLogger.Debug(LogTag, "DrawBitmaps()");
            for (int i = 0; i < immutableSource.Length; i++)
            {
                DrawBitmap(i, immutableSource[i]);
            }
        }

        private void DisposeBitmap(int index)
        {
            InternalLogger.Debug(LogTag, () => $"DisposeBitmap( index: {index} )");
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

            InternalLogger.Debug(LogTag, $"DisposeBitmaps()");
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
                // float blurCoeff = 1f + (float)shade.BlurRadius / 20f;

                return new ShadeInfo(
                    shade.Color.MultiplyAlpha(shade.Opacity).ToAndroid(),
                    context.ToPixels(shade.BlurRadius) * 2,
                    context.ToPixels(shade.Offset.X),
                    context.ToPixels(shade.Offset.Y));
            }

            public override string ToString()
            {
                var builder = new StringBuilder();
                builder.AppendLine($"ShadeInfo( Offset: {{ X: {OffsetX}, Y: {OffsetY} }}, ");
                builder.AppendLine(
                    $"    Color=> R:{Color.R}, G:{Color.G}, B:{Color.B}, Alpha:{Color.A}, BlurRadius: {BlurRadius} )");
                builder.AppendLine(
                    $"    BlurRadius: {BlurRadius} )");

                return builder.ToString();
            }
        }
    }
}
