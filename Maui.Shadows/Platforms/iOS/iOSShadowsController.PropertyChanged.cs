using System.Collections.Specialized;
using System.ComponentModel;

namespace Sharpnado.Shades.Platforms.iOS;

/// <summary>
/// iOS shadows controller partial class handling property changes.
/// </summary>
internal partial class iOSShadowsController
{
    public void UpdateCornerRadius(float cornerRadius)
    {
        if (_isDisposed 
            || (!_weakShadowsLayer.TryGetTarget(out var shadowsLayer) && !_weakShadowSource.TryGetTarget(out _)))
        {
            return;
        }

        InternalLogger.Debug(LogTag, () => $"UpdateCornerRadius( cornerRadius: {cornerRadius} )");
        bool hasChanged = _cornerRadius != cornerRadius;
        _cornerRadius = cornerRadius;

        if (hasChanged && _shadesSource != null && _shadesSource.Any())
        {
            if (shadowsLayer?.Sublayers != null)
            {
                foreach (var subLayer in shadowsLayer.Sublayers)
                {
                    subLayer.CornerRadius = cornerRadius;
                }
            }
        }
    }

    public void UpdateShades(IEnumerable<Shade>? shadesSource)
    {
        if (_isDisposed || shadesSource == null)
        {
            return;
        }

        InternalLogger.Debug(LogTag, () => $"UpdateShades( shadesSource: {shadesSource} )");
        if (!_weakShadowsLayer.TryGetTarget(out _) && !_weakShadowSource.TryGetTarget(out _))
        {
            return;
        }

        if (_shadesSource is INotifyCollectionChanged previousNotifyCollectionChanged)
        {
            previousNotifyCollectionChanged.CollectionChanged -= ShadesSourceCollectionChanged;
        }

        _shadesSource = shadesSource;
        if (_shadesSource is INotifyCollectionChanged notifyCollectionChanged)
        {
            notifyCollectionChanged.CollectionChanged += ShadesSourceCollectionChanged;
        }

        DestroyShadows();
        int i = 0;
        foreach (var shade in _shadesSource)
        {
            InsertShade(i, shade);
            i++;
        }
    }

    internal void ShadesSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isDisposed)
        {
            return;
        }

        if (!_weakShadowsLayer.TryGetTarget(out var shadowsLayer))
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

                    shadowsLayer.SetNeedsDisplay();
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    for (int i = 0, removedIndex = e.OldStartingIndex; i < e.OldItems.Count; i++)
                    {
                        RemoveShade(removedIndex, (Shade)e.OldItems[i]!);
                    }

                    shadowsLayer.SetNeedsDisplay();
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                DestroyShadows();
                shadowsLayer.SetNeedsDisplay();
                break;
        }
    }

    private void InsertShade(int insertIndex, Shade shade)
    {
        if (!_weakShadowsLayer.TryGetTarget(out var shadowsLayer))
        {
            return;
        }

        InternalLogger.Debug(LogTag, () => $"InsertShade( insertIndex: {insertIndex}, shade: {shade} )");
        var shadeSubLayer = shade.ToCALayer();
        shadeSubLayer.CornerRadius = _cornerRadius;
        SetLayerFrame(shadeSubLayer);

        shadowsLayer.InsertSublayer(shadeSubLayer, insertIndex);

        shadeSubLayer.SetNeedsDisplay();
        shade.WeakPropertyChanged += ShadePropertyChanged;
    }

    private void RemoveShade(int removedIndex, Shade shade)
    {
        if (!_weakShadowsLayer.TryGetTarget(out var shadowsLayer))
        {
            return;
        }

        InternalLogger.Debug(LogTag, () => $"RemoveShade( insertIndex: {removedIndex} )");
        shade.WeakPropertyChanged -= ShadePropertyChanged;
        DestroyShadow(removedIndex);
        shadowsLayer.SetNeedsDisplay();
    }

    private void UnsubscribeAllShades()
    {
        if (_shadesSource == null)
        {
            return;
        }

        InternalLogger.Debug(LogTag, () => $"UnsubscribeAllShades() with count: {_shadesSource.Count()}");
        foreach (var shade in _shadesSource)
        {
            shade.WeakPropertyChanged -= ShadePropertyChanged;
        }
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
            InternalLogger.Warn(LogTag, $"ShadePropertyChanged => shade property {e.PropertyName} changed but we can't find the shade in the source");
            return;
        }

        InternalLogger.Debug(LogTag, () => $"ShadePropertyChanged( shadeIndex: {index}, propertyName: {e.PropertyName} )");
        switch (e.PropertyName)
        {
            case nameof(Shade.BlurRadius):
            case nameof(Shade.Color):
            case nameof(Shade.Opacity):
            case nameof(Shade.Offset):
                UpdateShadeLayer(index, shade);
                break;
        }
    }
}
