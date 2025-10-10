# Sharpnado.Maui.Shadows

Add beautiful, customizable shadows to any .NET MAUI view across all platforms.

[![NuGet](https://img.shields.io/nuget/v/Sharpnado.Maui.Shadows.svg)](https://www.nuget.org/packages/Sharpnado.Maui.Shadows)

## Quick Start

### 1. Install

```bash
dotnet add package Sharpnado.Maui.Shadows
```

### 2. Initialize in MauiProgram.cs

```csharp
using Sharpnado.Shades;

builder.UseSharpnadoShadows(loggerEnable: false);
```

### 3. Use in XAML

```xml
xmlns:sh="clr-namespace:Sharpnado.Shades;assembly=Sharpnado.Maui.Shadows"

<sh:Shadows CornerRadius="10"
            Shades="{sh:SingleShade BlurRadius=10, Opacity=0.5, Color=Black, Offset='0,5'}">
    <Button Text="Shadow Button" />
</sh:Shadows>
```

## Features

✨ **Multiple Shadows** - Add as many shadows as needed per view  
🎯 **Full Control** - Color, Opacity, BlurRadius, Offset, CornerRadius  
⚡ **Hardware Accelerated** - GPU rendering on all platforms  
🔧 **Android BlurType** - Choose GPU or StackBlur rendering  
🎨 **Neumorphism** - Built-in neumorphism support  
💾 **Memory Safe** - Weak events, no memory leaks  
📦 **XAML Extensions** - SingleShade, ImmutableShades, ShadeStack, NeumorphismShades

## Platforms

| Platform | Min Version | Implementation |
|----------|-------------|----------------|
| Android | API 21 | RenderScript/RenderEffect + Caching |
| iOS | 12.2 | CALayer |
| MacCatalyst | 15.0 | CALayer |
| Windows | 10.0.17763.0 | WinUI 3 Composition API |

## Examples

### Single Shadow
```xml
<sh:Shadows CornerRadius="20"
            Shades="{sh:SingleShade BlurRadius=15, Opacity=0.6, Color=Purple, Offset='0,10'}">
    <Image Source="photo.jpg" />
</sh:Shadows>
```

### Multiple Shadows
```xml
<sh:Shadows CornerRadius="10">
    <sh:Shadows.Shades>
        <sh:ImmutableShades>
            <sh:Shade BlurRadius="10" Opacity="0.5" Offset="-10,-10" Color="White" />
            <sh:Shade BlurRadius="10" Opacity="0.5" Offset="10,10" Color="#19000000" />
        </sh:ImmutableShades>
    </sh:Shadows.Shades>
    <Frame BackgroundColor="#F0F0F3" CornerRadius="10">
        <Label Text="Neumorphism" />
    </Frame>
</sh:Shadows>
```

### Neumorphism (Quick)
```xml
<sh:Shadows CornerRadius="20" Shades="{sh:NeumorphismShades}">
    <Button Text="Neumorphism" BackgroundColor="#F0F0F3" />
</sh:Shadows>
```

### Android Blur Type
```xml
<sh:Shadows CornerRadius="10" 
            BlurType="Gpu"
            Shades="{sh:SingleShade BlurRadius=20, Opacity=0.7, Color=Black, Offset='0,8'}">
    <Image Source="background.jpg" />
</sh:Shadows>
```

**BlurType options:**
- `Gpu` (default): Hardware-accelerated (RenderScript API<31, RenderEffect API≥31)
- `StackBlur`: CPU-based fallback for compatibility

## Performance Tips

### Android
- Use `BlurType="Gpu"` for best performance (default)
- Shadows are globally cached by (color, size, blur)
- Avoid animating BlurRadius, Color, or Opacity (creates new bitmaps)

### iOS/MacCatalyst
- Hardware-accelerated via CALayer
- All properties are safely animatable

### Windows
- Hardware-accelerated via WinUI 3 Composition API
- Lightweight SpriteVisual shadows

## Migration from Xamarin.Forms

1. Update package: `Sharpnado.Shadows` → `Sharpnado.Maui.Shadows`
2. Update XAML assembly: `assembly=Sharpnado.Shadows` → `assembly=Sharpnado.Maui.Shadows`
3. Update initialization: `builder.UseSharpnadoShadows()`
4. Remove platform-specific initialization (no longer needed)

**Migration time: ~10 minutes**

## Documentation

- [GitHub Repository](https://github.com/roubachof/Sharpnado.Shadows)
- [Full Documentation](https://github.com/roubachof/Sharpnado.Shadows/blob/main/README.md)
- [Sample Application](https://github.com/roubachof/Sharpnado.Shadows/tree/main/MauiSample)
- [Release Notes](https://github.com/roubachof/Sharpnado.Shadows/releases)

## License

MIT License - © Jean-Marie Alfonsi

## Support

- [GitHub Issues](https://github.com/roubachof/Sharpnado.Shadows/issues)
- [GitHub Discussions](https://github.com/roubachof/Sharpnado.Shadows/discussions)
