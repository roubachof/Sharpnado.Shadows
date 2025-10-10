# Sharpnado.Maui.Shadows

✅ **Complete .NET 9 MAUI port of Sharpnado.Shadows - Production Ready!**

[![Nuget](https://img.shields.io/nuget/v/Sharpnado.Maui.Shadows.svg)](https://www.nuget.org/packages/Sharpnado.Maui.Shadows)

## Overview

Add customizable shadows to any .NET MAUI view across all platforms with full hardware acceleration.

### ✨ Features

- 🎨 **Multiple shadows per view** - Add as many shadows as you want
- 🎯 **Full property control** - Color, Opacity, BlurRadius, Offset, CornerRadius
- ⚡ **Hardware accelerated** - GPU rendering on all platforms
- 🎭 **Neumorphism support** - Built-in neumorphism design patterns
- 🔧 **Android BlurType** - Choose between GPU or StackBlur rendering
- 💾 **Memory efficient** - Bitmap caching, weak events, no memory leaks
- 🚀 **Modern architecture** - Full MAUI handlers for all platforms

## Supported Platforms

| Platform      | Status | Implementation           |
|---------------|--------|-------------------------|
| Android 21+   | ✅     | GPU/StackBlur + Caching |
| iOS 12.2+     | ✅     | CALayer                 |
| MacCatalyst   | ✅     | CALayer                 |
| Windows 10+   | ✅     | WinUI 3 Composition API |

## Architecture

### Core Components

#### Cross-Platform
- **Shade.cs** - Individual shadow configuration with bindable properties
- **Shadows.cs** - Main ContentView container using WeakEvent pattern
- **XAML Extensions** - ImmutableShades, ShadeStack, SingleShade, NeumorphismShades
- **MauiAppBuilderExtensions.cs** - Modern MAUI initialization

#### Platform Handlers

**Android** (`Platforms/Android/`)
- **ShadowsHandler.cs** - MAUI ViewHandler implementation
- **AndroidShadowsController.cs** - Shadow lifecycle management
- **BlurType** - GPU (RenderScript/RenderEffect) or StackBlur
- **BitmapCache** - Global bitmap caching system
- **GpuBlurHelper.cs** - Hardware-accelerated blur rendering
- **StackBlurHelper.cs** - CPU-based blur fallback

**iOS/MacCatalyst** (`Platforms/iOS/`)
- **ShadowsHandler.cs** - MAUI ViewHandler with UIView container
- **iOSShadowsController.cs** - CALayer-based shadow management
- Full hardware acceleration via Core Animation

**Windows** (`Platforms/Windows/`)
- **ShadowsHandler.cs** - MAUI ViewHandler with Grid container
- **WindowsShadowsController.cs** - SpriteVisual shadow management
- WinUI 3 Composition API with drop shadows

## Installation

```bash
dotnet add package Sharpnado.Maui.Shadows
```

## Setup

In your `MauiProgram.cs`:

```csharp
using Sharpnado.Shades;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            })
            .UseSharpnadoShadows(loggerEnable: false);

        return builder.Build();
    }
}
```

## Usage Examples

### Basic Shadow

```xml
xmlns:sh="clr-namespace:Sharpnado.Shades;assembly=Sharpnado.Maui.Shadows"

<sh:Shadows CornerRadius="10"
            Shades="{sh:SingleShade Offset='0,10', 
                                    Opacity=0.7, 
                                    BlurRadius=10,
                                    Color=Black}">
    <Button Text="Click Me" 
            BackgroundColor="White"
            CornerRadius="10" />
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
        <!-- Your content -->
    </Frame>
</sh:Shadows>
```

### Neumorphism

```xml
<sh:Shadows CornerRadius="20"
            Shades="{sh:NeumorphismShades}">
    <Button Text="Neumorphism" 
            BackgroundColor="#F0F0F3"
            CornerRadius="20" />
</sh:Shadows>
```

### Android Blur Type Selection

```xml
<sh:Shadows CornerRadius="10"
            BlurType="Gpu"
            Shades="{sh:SingleShade BlurRadius=15, Opacity=0.6, Offset='0,5', Color=Purple}">
    <Image Source="photo.jpg" />
</sh:Shadows>
```

## Key Changes from Xamarin.Forms

### Breaking Changes

1. **Package Name**: `Sharpnado.Shadows` → `Sharpnado.Maui.Shadows`
2. **Assembly Name**: `Sharpnado.Shadows` → `Sharpnado.Maui.Shadows`
3. **Initialization**: `Initializer.Initialize()` → `builder.UseSharpnadoShadows()`
4. **Target Framework**: Xamarin.Forms → .NET 9 MAUI
5. **Platform Support**: UWP/Tizen removed, MacCatalyst/Windows added

### New Features in 2.0

1. **Android BlurType Property** - Choose GPU or StackBlur rendering
2. **Improved Memory Management** - Fixed memory leaks, better cleanup
3. **Modern Handlers** - Full MAUI ViewHandler architecture
4. **Nullable Reference Types** - Enhanced null safety
5. **Enhanced Logging** - Better debugging capabilities

### Migration Guide

1. Update package reference:
   ```xml
   <!-- Old -->
   <PackageReference Include="Sharpnado.Shadows" Version="1.x" />
   
   <!-- New -->
   <PackageReference Include="Sharpnado.Maui.Shadows" Version="2.0.0" />
   ```

2. Update assembly reference in XAML:
   ```xml
   <!-- Old -->
   xmlns:sh="clr-namespace:Sharpnado.Shades;assembly=Sharpnado.Shadows"
   
   <!-- New -->
   xmlns:sh="clr-namespace:Sharpnado.Shades;assembly=Sharpnado.Maui.Shadows"
   ```

3. Update initialization:
   ```csharp
   // Old (Xamarin.Forms)
   Sharpnado.Shades.Initializer.Initialize(loggerEnable: false);
   
   // New (MAUI)
   builder.UseSharpnadoShadows(loggerEnable: false);
   ```

4. Remove platform-specific initializers (no longer needed):
   ```csharp
   // Remove these
   Sharpnado.Shades.iOS.iOSShadowsRenderer.Initialize();
   Sharpnado.Shades.Tizen.TizenShadowsRenderer.Initialize();
   // etc.
   ```

## Performance Tips

### Android
- Use `BlurType="Gpu"` (default) for best performance
- Switch to `BlurType="StackBlur"` if GPU issues occur on specific devices
- Shadows are bitmap-cached globally to minimize memory usage
- Avoid animating BlurRadius, Color, or Opacity on Android (creates new bitmaps)

### iOS/MacCatalyst
- Hardware-accelerated via CALayer
- All shadow properties are animatable without performance penalty

### Windows
- Hardware-accelerated via WinUI 3 Composition API
- SpriteVisual drop shadows are lightweight and efficient

## Building from Source

```bash
# Clone the repository
git clone https://github.com/roubachof/Sharpnado.Shadows.git
cd Sharpnado.Shadows/Maui.Shadows

# Build all platforms
dotnet build

# Build specific platform
dotnet build -f net9.0-android
dotnet build -f net9.0-ios
dotnet build -f net9.0-maccatalyst
dotnet build -f net9.0-windows10.0.19041.0

# Clean build
dotnet clean && dotnet build
```

## Documentation

For complete documentation, examples, and advanced usage, see the [main README](../README.md).

## License

MIT License - see [LICENSE](../LICENSE) for details.

## Credits

Created by [Jean-Marie Alfonsi](https://github.com/roubachof)

Migrated to .NET MAUI with contributions from the community.
