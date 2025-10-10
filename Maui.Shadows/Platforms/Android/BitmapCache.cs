using System.Text;
using AndroidGraphics = Android.Graphics;

namespace Sharpnado.Shades.Platforms.Android;

public class BitmapCache : IDisposable
{
    private const string LogTag = nameof(BitmapCache);

    private static BitmapCache? instance;

    private readonly Dictionary<string, Bucket> _cache = new();
    private readonly object _cacheLock = new();

    public static BitmapCache Instance => instance ?? Initialize();

    public void Dispose()
    {
        lock (_cacheLock)
        {
            foreach (var bucket in _cache.Values)
            {
                DisposeBitmap(bucket.Value);
            }

            _cache?.Clear();
        }
    }

    public AndroidGraphics.Bitmap GetOrCreate(string hash, Func<AndroidGraphics.Bitmap> create)
    {
        InternalLogger.Debug(LogTag, () => $"GetOrCreate( hash: {hash} )");
        lock (_cacheLock)
        {
            if (_cache.TryGetValue(hash, out var bucket))
            {
                return bucket.Value;
            }

            return Create(hash, create);
        }
    }

    public bool TryGet(string hash, out AndroidGraphics.Bitmap? bitmap)
    {
        lock (_cacheLock)
        {
            if (_cache.TryGetValue(hash, out var bucket))
            {
                bitmap = bucket.Value;
                return true;
            }

            bitmap = null;
            return false;
        }
    }

    public bool Contains(string hash)
    {
        lock (_cacheLock)
        {
            return _cache.ContainsKey(hash);
        }
    }

    public AndroidGraphics.Bitmap Add(string hash, Func<AndroidGraphics.Bitmap> create)
    {
        InternalLogger.Debug(LogTag, () => $"Add( hash: {hash} )");
        lock (_cacheLock)
        {
            if (_cache.TryGetValue(hash, out var bucket))
            {
                bucket.ReferenceCount++;
                InternalLogger.Debug(LogTag, () => $"Reference count: {bucket.ReferenceCount}");
                return bucket.Value;
            }

            return Create(hash, create);
        }
    }

    public void IncrementReferenceCount(string hash)
    {
        InternalLogger.Debug(LogTag, () => $"IncrementReference( hash: {hash} )");
        lock (_cacheLock)
        {
            if (_cache.TryGetValue(hash, out var bucket))
            {
                bucket.ReferenceCount++;
                InternalLogger.Debug(LogTag, () => $"Reference count: {bucket.ReferenceCount}");
            }
        }
    }

    public bool Remove(string hash)
    {
        InternalLogger.Debug(LogTag, () => $"Remove( hash: {hash} )");
        lock (_cacheLock)
        {
            if (_cache.TryGetValue(hash, out var bucket))
            {
                bucket.ReferenceCount--;
                InternalLogger.Debug(LogTag, () => $"Reference count: {bucket.ReferenceCount}");

                if (bucket.ReferenceCount <= 0)
                {
                    _cache.Remove(hash);
                    InternalLogger.Debug(LogTag, () => $"Removing bitmap, bitmap count is {_cache.Count}");

                    DisposeBitmap(bucket.Value);
                }

                return true;
            }

            return false;
        }
    }

    public void Log()
    {
        lock (_cacheLock)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"BitmapCache: {_cache.Count} bitmaps");
            foreach (var keyValue in _cache)
            {
                stringBuilder.AppendLine($"    {keyValue.Key}:{keyValue.Value.ReferenceCount:00}");
            }

            InternalLogger.Debug(LogTag, () => stringBuilder.ToString());
        }
    }

    private static BitmapCache Initialize()
    {
        instance?.Dispose();
        return instance = new BitmapCache();
    }

    private static void DisposeBitmap(AndroidGraphics.Bitmap? bitmap)
    {
        if (bitmap.IsNullOrDisposed() || bitmap!.IsRecycled)
        {
            return;
        }

        bitmap.Recycle();
        bitmap.Dispose();
    }

    private AndroidGraphics.Bitmap Create(string hash, Func<AndroidGraphics.Bitmap> create)
    {
        var newBitmap = create();
        _cache.Add(hash, new Bucket(1, newBitmap));
        InternalLogger.Debug(LogTag, () => $"New bitmap created, bitmap count is {_cache.Count}");
        return newBitmap;
    }

    public class Bucket
    {
        public Bucket(int referenceCount, AndroidGraphics.Bitmap value)
        {
            ReferenceCount = referenceCount;
            Value = value;
        }

        public int ReferenceCount { get; set; }

        public AndroidGraphics.Bitmap Value { get; }
    }
}
