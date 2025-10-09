using System.Collections.Specialized;
using System.ComponentModel;

namespace Sharpnado.Shades.Platforms.Android;

public partial class ShadowView
{
    private float _cornerRadius;
    private IEnumerable<Shade>? _shadesSource;

    public void UpdateCornerRadius(float cornerRadius)
    {
        if (_isDisposed)
        {
            return;
        }

        InternalLogger.Debug(LogTag, () => $"UpdateCornerRadius( cornerRadius: {cornerRadius} )");
        bool hasChanged = _cornerRadius != cornerRadius;
        _cornerRadius = cornerRadius;

        if (hasChanged && _shadesSource != null && _shadesSource.Any())
        {
            RefreshBitmaps();
            Invalidate();
        }
    }

    public void UpdateShades(IEnumerable<Shade>? shadesSource)
    {
        if (_isDisposed)
        {
            return;
        }

        if (shadesSource == null)
        {
            return;
        }

        InternalLogger.Debug(LogTag, () => $"UpdateShades( shadesSource: {shadesSource} )");
        _shadesSource = shadesSource;

        DisposeBitmaps();
        int i = 0;
        foreach (var shade in _shadesSource)
        {
            InsertShade(i, shade);
            i++;
        }

        Invalidate();
    }

    public void UpdateBlurType(AndroidBlurType androidBlurType)
    {
        if (_isDisposed)
        {
            return;
        }

        if (_androidBlurType == androidBlurType)
        {
            return;
        }

        InternalLogger.Debug(LogTag, () => $"UpdateBlurType( blurType: {androidBlurType} )");
        _androidBlurType = androidBlurType;

        // Refresh all bitmaps with the new blur type
        if (_shadesSource != null && _shadesSource.Any())
        {
            RefreshBitmaps();
            Invalidate();
        }
    }

    public void ShadesSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isDisposed)
        {
            return;
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    for (int i = 0, insertIndex = e.NewStartingIndex; i < e.NewItems.Count; i++)
                    {
                        InsertShade(insertIndex, (Shade)e.NewItems[i]!);
                    }

                    Invalidate();
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    for (int i = 0, removedIndex = e.OldStartingIndex; i < e.OldItems.Count; i++)
                    {
                        RemoveShade(removedIndex, (Shade)e.OldItems[i]!);
                    }

                    Invalidate();
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                DisposeBitmaps();
                Invalidate();
                break;
        }
    }

    private void InsertShade(int insertIndex, Shade shade)
    {
        InternalLogger.Debug(LogTag, () => $"InsertShade( insertIndex: {insertIndex}, shade: {shade} )");
        InsertBitmap(shade);
        shade.WeakPropertyChanged += ShadePropertyChanged;
    }

    private void RemoveShade(int removedIndex, Shade shade)
    {
        InternalLogger.Debug(LogTag, () => $"RemoveShade( removedIndex: {removedIndex} )");
        shade.WeakPropertyChanged -= ShadePropertyChanged;
        DisposeBitmap(shade);
    }

    private void ShadePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isDisposed)
        {
            return;
        }

        if (e.PropertyName == null || !Shade.IsShadeProperty(e.PropertyName))
        {
            return;
        }

        var shade = (Shade)sender!;
        var index = _shadesSource?.ToList().IndexOf(shade) ?? -1;
        if (index < 0)
        {
            InternalLogger.Warn(
                LogTag, $"ShadePropertyChanged => shade property {e.PropertyName} changed but we can't find the shade in the source");
            return;
        }

        InternalLogger.Debug(LogTag, () => $"ShadePropertyChanged( shadeIndex: {index}, propertyName: {e.PropertyName} )");
        switch (e.PropertyName)
        {
            case nameof(Shade.BlurRadius):
            case nameof(Shade.Color):
            case nameof(Shade.Opacity):
                RefreshBitmap(shade);
                Invalidate();
                break;

            case nameof(Shade.Offset):
                UpdateShadeInfo(shade);
                Invalidate();
                break;
        }
    }
}
