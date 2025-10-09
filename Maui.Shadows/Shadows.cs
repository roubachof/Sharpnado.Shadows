using System.Collections.ObjectModel;
using System.Collections.Specialized;
using WeakEvent;

namespace Sharpnado.Shades;

/// <summary>
/// Specifies the blur algorithm to use for shadow rendering on Android.
/// </summary>
public enum AndroidBlurType
{
    /// <summary>
    /// GPU-accelerated blur using RenderEffect (Android 12+) or RenderScript (older versions).
    /// This is the default and provides best performance with hardware acceleration.
    /// </summary>
    Gpu = 0,

    /// <summary>
    /// CPU-based StackBlur algorithm.
    /// Forces the use of StackBlur regardless of Android version or hardware acceleration.
    /// Use this for consistency across all devices or when GPU blur produces unwanted artifacts.
    /// </summary>
    StackBlur = 1,
}


public class Shadows : ContentView
{
    public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
        nameof(CornerRadius),
        typeof(int),
        typeof(Shadows),
        DefaultCornerRadius);

    public static readonly BindableProperty ShadesProperty = BindableProperty.Create(
        nameof(Shades),
        typeof(IEnumerable<Shade>),
        typeof(Shadows),
        defaultValueCreator: (bo) => new ObservableCollection<Shade> { new Shade { Parent = (Shadows)bo } },
        validateValue: (bo, v) => v is IEnumerable<Shade>,
        propertyChanged: ShadesPropertyChanged,
        coerceValue: CoerceShades);

    public static readonly BindableProperty AndroidBlurTypeProperty = BindableProperty.Create(
        nameof(AndroidBlurType),
        typeof(AndroidBlurType),
        typeof(Shadows),
        AndroidBlurType.Gpu);

    private const int DefaultCornerRadius = 0;
    private static int instanceCount = 0;

    private readonly WeakEventSource<NotifyCollectionChangedEventArgs> _weakCollectionChangedSource = new();

    public Shadows()
    {
        InstanceNumber = ++instanceCount;
    }

    public event EventHandler<NotifyCollectionChangedEventArgs>? WeakCollectionChanged
    {
        add => _weakCollectionChangedSource.Subscribe(value);
        remove => _weakCollectionChangedSource.Unsubscribe(value);
    }

    public int InstanceNumber { get; }

    public int CornerRadius
    {
        get => (int)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public IEnumerable<Shade> Shades
    {
        get => (IEnumerable<Shade>)GetValue(ShadesProperty);
        set => SetValue(ShadesProperty, value);
    }

    /// <summary>
    /// Gets or sets the blur algorithm to use for shadow rendering on Android.
    /// Default is Gpu (RenderEffect/RenderScript).
    /// Set to StackBlur to force CPU-based blur for consistency or compatibility.
    /// </summary>
    public AndroidBlurType AndroidBlurType
    {
        get => (AndroidBlurType)GetValue(AndroidBlurTypeProperty);
        set => SetValue(AndroidBlurTypeProperty, value);
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        foreach (var shade in Shades)
        {
            shade.BindingContext = BindingContext;
        }
    }

    private static object CoerceShades(BindableObject bindable, object value)
    {
        if (value is not ReadOnlyCollection<Shade> readonlyCollection)
        {
            return value;
        }

        return new ReadOnlyCollection<Shade>(
            readonlyCollection.Select(s => s.Clone()).ToList());
    }

    private static void ShadesPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        var shadows = (Shadows)bindable;
        var enumerableShades = (IEnumerable<Shade>)newvalue;

        if (oldvalue != null)
        {
            if (oldvalue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= shadows.OnShadeCollectionChanged;
            }

            foreach (var shade in enumerableShades)
            {
                shade.Parent = null;
                shade.BindingContext = null;
            }
        }

        foreach (var shade in enumerableShades)
        {
            shade.Parent = shadows;
            shade.BindingContext = shadows.BindingContext;
        }

        if (newvalue is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += shadows.OnShadeCollectionChanged;
        }
    }

    private void OnShadeCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    foreach (Shade newShade in e.NewItems)
                    {
                        newShade.Parent = this;
                        newShade.BindingContext = BindingContext;
                        _weakCollectionChangedSource.Raise(this, e);
                    }
                }
                break;

            case NotifyCollectionChangedAction.Reset:
            case NotifyCollectionChangedAction.Remove:
                foreach (Shade oldShade in e.OldItems ?? Array.Empty<Shade>())
                {
                    oldShade.Parent = null;
                    oldShade.BindingContext = null;
                    _weakCollectionChangedSource.Raise(this, e);
                }
                break;
        }
    }
}
