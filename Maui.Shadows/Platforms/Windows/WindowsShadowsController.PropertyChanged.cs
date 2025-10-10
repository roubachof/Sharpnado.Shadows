using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Shapes;

namespace Sharpnado.Shades.Platforms.Windows;

/// <summary>
/// Windows shadows controller partial class handling property changes.
/// </summary>
internal partial class WindowsShadowsController
{
    public void UpdateCornerRadius(float cornerRadius)
    {
        if (_isDisposed)
        {
            return;
        }

        if (_shadowsCanvas == null && _shadowSource == null)
        {
            return;
        }

        InternalLogger.Debug(LogTag, () => $"UpdateCornerRadius( cornerRadius: {cornerRadius} )");
        bool hasChanged = _cornerRadius != cornerRadius;
        _cornerRadius = cornerRadius;

        if (hasChanged && _shadesSource != null && _shadesSource.Any())
        {
            for (int i = 0; i < _shadowsCanvas.Children.Count; i++)
            {
                var shadowHost = (Rectangle)_shadowsCanvas.Children[i];
                shadowHost.RadiusX = cornerRadius;
                shadowHost.RadiusY = cornerRadius;

                if (i < _shadowVisuals.Count)
                {
                    var shadowVisual = _shadowVisuals[i];
                    if (shadowVisual.Shadow is DropShadow dropShadow)
                    {
                        dropShadow.Mask = shadowHost.GetAlphaMask();
                    }
                }
            }
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

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    for (int i = 0, insertIndex = e.NewStartingIndex; i < e.NewItems.Count; i++)
                    {
                        InsertShade(insertIndex, (Shade)e.NewItems[i]!);
                    }
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    for (int i = 0, removedIndex = e.OldStartingIndex; i < e.OldItems.Count; i++)
                    {
                        RemoveShade(removedIndex, (Shade)e.OldItems[i]!);
                    }
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                DestroyShadows();
                break;
        }
    }

    private void RemoveShade(int removedIndex, Shade shade)
    {
        InternalLogger.Debug(LogTag, () => $"RemoveShade( insertIndex: {removedIndex} )");
        shade.WeakPropertyChanged -= ShadePropertyChanged;
        DestroyShadow(removedIndex);
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
                UpdateShadeVisual(index, shade);
                break;
        }
    }
}
