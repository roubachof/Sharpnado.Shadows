# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

Sharpnado.Shadows is a .NET MAUI library (version 2.0+) that adds customizable shadows to any view across Android, iOS, Windows, and MacCatalyst platforms. The library enables multiple shadows per view with configurable properties (Color, Opacity, BlurRadius, Offset, CornerRadius) and supports Neumorphism design patterns.

**Version 2.0.0** is a complete rewrite for .NET 9 MAUI with modern handler architecture. Legacy Xamarin.Forms support (v1.x) has been moved to a separate branch.

## Repository Structure

```
Sharpnado.Shadows/
├── Maui.Shadows/                        # .NET 9 MAUI library (main project)
│   ├── Shade.cs                         # Core shadow configuration (BindableObject)
│   ├── Shadows.cs                       # Main ContentView with weak events
│   ├── *Extension.cs                    # XAML markup extensions
│   ├── MauiAppBuilderExtensions.cs      # MAUI initialization (UseSharpnadoShadows)
│   ├── InternalLogger.cs                # Logging infrastructure
│   ├── Maui.Shadows.csproj              # Multi-targeted .NET 9 project
│   └── Platforms/                       # Platform-specific implementations
│       ├── Android/                     # Android handlers and controllers
│       │   ├── ShadowsHandler.cs        # MAUI ViewHandler
│       │   ├── AndroidShadowsController.cs  # Shadow lifecycle management
│       │   ├── BitmapCache.cs           # Global bitmap caching
│       │   ├── GpuBlurHelper.cs         # GPU-based blur (RenderScript/RenderEffect)
│       │   └── StackBlurHelper.cs       # CPU-based StackBlur fallback
│       ├── iOS/                         # iOS/MacCatalyst handlers
│       │   ├── ShadowsHandler.cs        # MAUI ViewHandler with UIView
│       │   └── iOSShadowsController.cs  # CALayer-based shadows
│       └── Windows/                     # Windows handlers
│           ├── ShadowsHandler.cs        # MAUI ViewHandler with Grid
│           └── WindowsShadowsController.cs  # WinUI 3 Composition API
├── MauiSample/                          # .NET MAUI sample application
│   └── ShadowsSample.Maui/              # Sample app demonstrating library features
├── Docs/                                # Documentation assets (images, diagrams)
└── README.md                            # Main documentation
```

## Build Commands

### Building the MAUI library (.NET 9)

```bash
# From repository root
cd Maui.Shadows

# Build all target frameworks
dotnet build

# Build for specific platform
dotnet build -f net9.0-android
dotnet build -f net9.0-ios
dotnet build -f net9.0-maccatalyst
dotnet build -f net9.0-windows10.0.19041.0

# Build in Release mode
dotnet build -c Release

# Clean build
dotnet clean && dotnet build
```

### Building the sample application

```bash
# From repository root
cd MauiSample/ShadowsSample.Maui

# Build for all platforms
dotnet build

# Run on specific platform
dotnet build -t:Run -f net9.0-android
dotnet build -t:Run -f net9.0-ios
```

### Creating NuGet package

```bash
# From Maui.Shadows directory
dotnet pack -c Release

# Package will be created in bin/Release/
# Sharpnado.Maui.Shadows.2.0.0.nupkg
```

### Running tests (if available)

```bash
# From repository root
dotnet test
```

## Architecture

### Core Concepts

**Shadows Component**: A .NET MAUI `ContentView` that wraps any view and applies one or more shadow layers (called "Shades").

**Shade**: Represents an individual shadow with five main properties:
- `Point Offset`: Shadow offset from the source (X, Y)
- `Color Color`: Shadow color (MAUI Color)
- `double Opacity`: Shadow opacity (0.0 to 1.0)
- `double BlurRadius`: Blur amount in pixels
- `float CornerRadius`: Corner radius matching (set on Shadows, not individual Shade)

**BlurType** (Android only): Choose rendering algorithm:
- `Gpu` (default): Hardware-accelerated blur using RenderScript/RenderEffect
- `StackBlur`: CPU-based StackBlur algorithm

### Platform-Specific Implementations (.NET MAUI Handlers)

Each platform uses modern MAUI handlers with optimized rendering:

**Android** (`Maui.Shadows/Platforms/Android/`):
- **Handler**: `ShadowsHandler` (ViewHandler<Shadows, FrameLayout>)
- **Controller**: `AndroidShadowsController` manages shadow lifecycle
- **Blur Rendering**:
  - `GpuBlurHelper`: RenderScript (API < 31) or RenderEffect (API 31+)
  - `StackBlurHelper`: CPU fallback for compatibility
- **Caching**: `BitmapCache` singleton with hash-based bitmap reuse
- **Memory Management**: WeakEvents, proper disposal, cached bitmaps
- **Key Classes**: `ShadowsHandler`, `AndroidShadowsController`, `BitmapCache`, `GpuBlurHelper`, `StackBlurHelper`

**iOS/MacCatalyst** (`Maui.Shadows/Platforms/iOS/`):
- **Handler**: `ShadowsHandler` (ViewHandler<Shadows, UIView>)
- **Controller**: `iOSShadowsController` manages CALayer sublayers
- **Rendering**: Native `CALayer` with shadowColor, shadowOpacity, shadowRadius, shadowOffset
- **Hardware Acceleration**: Full Core Animation GPU acceleration
- **Memory**: UIView container with proper event unsubscription
- **Key Classes**: `ShadowsHandler`, `iOSShadowsController`, `ShadeExtensions`

**Windows** (`Maui.Shadows/Platforms/Windows/`):
- **Handler**: `ShadowsHandler` (ViewHandler<Shadows, Grid>)
- **Controller**: `WindowsShadowsController` manages SpriteVisual shadows
- **Rendering**: WinUI 3 Composition API with DropShadow
- **Structure**: Grid container with Canvas for shadows + content view
- **Hardware Acceleration**: GPU-accelerated via Windows Composition
- **Fixed**: Memory leak (SizeChanged event) from original UWP renderer
- **Key Classes**: `ShadowsHandler`, `WindowsShadowsController`

### Core Library Structure

The .NET 9 MAUI library (`Maui.Shadows/`) contains:

**Core Components**:
- **Shadows.cs**: Main `ContentView` with weak event pattern for memory safety
- **Shade.cs**: `BindableObject` with properties for shadow configuration
- **MauiAppBuilderExtensions.cs**: Modern MAUI initialization with `UseSharpnadoShadows()`
- **InternalLogger.cs**: Logging infrastructure with Action-based delegates

**XAML Markup Extensions** (all implement `IMarkupExtension`):
- **ImmutableShadesExtension**: Creates `ReadOnlyCollection<Shade>` for static resources
- **ShadeStackExtension**: Creates `ObservableCollection<Shade>` for dynamic changes
- **SingleShadeExtension**: Convenience for single shadow (returns `ReadOnlyCollection<Shade>`)
- **NeumorphismShadesExtension**: Pre-configured two-shadow neumorphism effect

**Platform Handlers** (see Platform-Specific Implementations section above)

### Key Design Patterns

**Shade Collections**:
- `ReadOnlyCollection<Shade>`: Immutable, shades are cloned to prevent leaks. **Use for static ResourceDictionary definitions.**
- `ObservableCollection<Shade>`: For dynamic addition/removal of shades during view lifetime
- `IEnumerable<Shade>`: For modifying shade properties during view lifetime

**Important**: Non-ReadOnly shades MUST be declared as transient instances, never as static resources.

**Weak Events**: The library uses `ThomasLevesque.WeakEvent` pattern to prevent memory leaks:
- `WeakCollectionChanged`: For shade collection changes
- `WeakPropertyChanged`: For individual shade property changes
- Prevents handlers from keeping views alive after disposal

**MAUI Handler Pattern**: Each platform implements a `ViewHandler<Shadows, TNative>` that:
1. **CreatePlatformView()**: Creates native container (FrameLayout/UIView/Grid)
2. **ConnectHandler()**: Sets up shadow controller, subscribes to events
3. **DisconnectHandler()**: Unsubscribes events, disposes controller, cleans up resources
4. **PropertyMapper**: Maps `CornerRadius` and `Shades` property changes
5. **Controller**: Separate class manages shadow lifecycle (create, update, destroy)

**Memory Management**:
- All handlers properly unsubscribe from events in `DisconnectHandler()`
- Controllers dispose native resources (bitmaps, layers, visuals)
- Android: Global bitmap cache with weak references
- iOS: CALayer sublayers removed from superlayer
- Windows: SpriteVisual and DropShadow properly disposed

## Initialization Requirements (.NET MAUI)

In `MauiProgram.cs`:

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
            .UseSharpnadoShadows(loggerEnable: false); // Add this line

        return builder.Build();
    }
}
```

**That's it!** No platform-specific initialization required. Handlers are automatically registered via `MauiAppBuilderExtensions.UseSharpnadoShadows()`.

**Logging**: Set `loggerEnable: true` to enable internal logging for debugging.

## Performance Considerations

### Android
- **BlurType="Gpu"** (default): Best performance with hardware acceleration
  - API < 31: Uses RenderScript (deprecated but functional)
  - API 31+: Uses RenderEffect (modern API)
- **BlurType="StackBlur"**: CPU fallback, use if GPU has issues on specific devices
- **Bitmap Caching**: Shadows are cached globally by hash (color, size, blur)
- **Avoid Animating**: Don't animate BlurRadius, Color, or Opacity (creates new bitmaps)
- **View Animations OK**: Rotation, scale, translation don't recreate bitmaps
- **Size Animations**: Temporarily set `Shades` to empty collection, animate, then restore

### iOS/MacCatalyst
- **CALayer Hardware Acceleration**: All shadow rendering is GPU-accelerated
- **Animate Freely**: All properties (color, blur, opacity, offset) can be animated efficiently
- **No Bitmap Overhead**: Native CALayer shadows don't use bitmaps

### Windows
- **Composition API**: Hardware-accelerated via WinUI 3 Composition
- **SpriteVisual**: Lightweight drop shadows with GPU rendering
- **Animate Freely**: All properties can be animated efficiently

### General
- **Weak Events**: Prevent memory leaks, no performance overhead
- **Shade Collections**: Use `ReadOnlyCollection<Shade>` in ResourceDictionary for best performance
- **Multiple Shadows**: Each shade adds rendering cost, but caching mitigates on Android

## Dependencies

- **Microsoft.Maui.Controls** (9.0.110): .NET MAUI framework
- **ThomasLevesque.WeakEvent** (4.1.0): Weak event pattern implementation for memory leak prevention

## Code Style

- **Language Version**: `latest` (C# 12 features enabled)
- **Nullable Reference Types**: Enabled throughout project
- **ImplicitUsings**: Enabled for common namespaces
- **Target Framework**: .NET 9 with multi-targeting
- **Coding Patterns**: Modern C# (pattern matching, target-typed new, null-coalescing)
- **Async/Await**: Used where appropriate (not in this project)
- **Disposal**: Proper `IDisposable` implementation in all controllers

## Namespace Convention

**Assembly Name**: `Sharpnado.Maui.Shadows`  
**Root Namespace**: `Sharpnado.Shades`  
**XAML Namespace**: `xmlns:sh="clr-namespace:Sharpnado.Shades;assembly=Sharpnado.Maui.Shadows"`

This is intentional - "Shades" is the conceptual name (a shadow is made of multiple shades), while "Shadows" is the product name.

## Version History

### Version 2.0.0 (Current - .NET MAUI)
- Complete rewrite for .NET 9 MAUI
- Modern ViewHandler architecture for all platforms
- Android: New `BlurType` property (Gpu/StackBlur)
- Fixed memory leaks (Windows SizeChanged event, proper event unsubscription)
- Enhanced null safety with nullable reference types
- Improved logging and debugging
- **Breaking Changes**: See README.md for migration guide

### Version 1.x (Legacy - Xamarin.Forms)
- Original Xamarin.Forms implementation
- Supported: Android, iOS, UWP, Tizen
- Deprecated: No longer maintained
- Code moved to separate branch

## NuGet Package

- **Package ID**: `Sharpnado.Maui.Shadows`
- **Version**: 2.0.0
- **License**: MIT
- **Target Frameworks**: net9.0, net9.0-android, net9.0-ios, net9.0-maccatalyst, net9.0-windows10.0.19041.0
- **Minimum OS Versions**:
  - Android: API 21 (Android 5.0)
  - iOS: 12.2
  - MacCatalyst: 15.0
  - Windows: 10.0.17763.0
