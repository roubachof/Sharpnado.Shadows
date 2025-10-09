# BlurType Property Usage (Android Only)

The `BlurType` property allows you to control which blur algorithm is used for shadow rendering on Android.

## Available Options

- **`BlurType.Gpu`** (Default): Uses GPU-accelerated blur
  - RenderEffect on Android 12+ (API 31+)
  - RenderScript on older versions
  - Best performance with hardware acceleration
  
- **`BlurType.StackBlur`**: Forces CPU-based StackBlur algorithm
  - Consistent results across all Android versions
  - Useful when GPU blur produces artifacts or inconsistencies
  - Better color fidelity in some cases

## XAML Examples

### Using Default GPU Blur
```xml
<shades:Shadows CornerRadius="10">
    <shades:Shadows.Shades>
        <shades:Shade BlurRadius="10" 
                      Color="Black" 
                      Opacity="0.5" 
                      Offset="0,4"/>
    </shades:Shadows.Shades>
    <shades:Shadows.Content>
        <Label Text="Hello World" />
    </shades:Shadows.Content>
</shades:Shadows>
```

### Forcing StackBlur Algorithm
```xml
<shades:Shadows CornerRadius="10"
                BlurType="StackBlur">
    <shades:Shadows.Shades>
        <shades:Shade BlurRadius="10" 
                      Color="Black" 
                      Opacity="0.5" 
                      Offset="0,4"/>
    </shades:Shadows.Shades>
    <shades:Shadows.Content>
        <Label Text="Hello World" />
    </shades:Shadows.Content>
</shades:Shadows>
```

## C# Code Example

```csharp
var shadows = new Shadows
{
    CornerRadius = 10,
#if ANDROID
    BlurType = BlurType.StackBlur, // Force StackBlur on Android
#endif
    Shades = new[]
    {
        new Shade
        {
            BlurRadius = 10,
            Color = Colors.Black,
            Opacity = 0.5,
            Offset = new Point(0, 4)
        }
    },
    Content = new Label { Text = "Hello World" }
};
```

## When to Use StackBlur

Consider using `BlurType.StackBlur` when:

1. **Consistency is critical**: You need identical shadow rendering across all Android versions
2. **GPU artifacts**: RenderEffect/RenderScript produces visual artifacts on some devices
3. **Color accuracy**: StackBlur may provide better color fidelity for certain shadow colors
4. **Testing/Debugging**: To isolate blur algorithm behavior

## Performance Considerations

- **GPU (RenderEffect/RenderScript)**: 
  - Faster on most modern devices
  - Runs asynchronously with hardware acceleration
  - May vary slightly between devices
  
- **StackBlur**: 
  - CPU-intensive, runs on background thread
  - Predictable, consistent results
  - May be slower on large shadows with high blur radius

## Platform Availability

**Note**: The `BlurType` property is only available on Android. On other platforms (iOS, Windows, etc.), it will not be accessible as it's wrapped in `#if ANDROID` conditional compilation.
