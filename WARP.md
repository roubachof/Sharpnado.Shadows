# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

Sharpnado.Shadows is a Xamarin.Forms library that adds customizable shadows to any view across Android, iOS, UWP, and Tizen platforms. The library enables multiple shadows per view with configurable properties (Color, Opacity, BlurRadius, Offset) and supports Neumorphism design patterns.

## Repository Structure

```
Sharpnado.Shadows/
├── Shadows/                      # Xamarin.Forms library (legacy)
│   ├── Shadows/                  # .NET Standard 2.0 core library
│   ├── Shadows.Droid/            # Android platform implementation
│   ├── Shadows.iOS/              # iOS platform implementation
│   ├── Shadows.UWP/              # Universal Windows Platform implementation
│   └── Shadows.Tizen/            # Tizen platform implementation
├── Maui.Shadows/                 # .NET 9 MAUI library (NEW)
│   ├── Shade.cs                  # Core shadow configuration
│   ├── Shadows.cs                # Main ContentView
│   ├── *Extension.cs             # XAML markup extensions
│   └── MauiAppBuilderExtensions.cs  # MAUI initialization
├── Sample/                       # Sample application demonstrating library usage
│   ├── ShadowsSample/            # Shared sample code
│   ├── ShadowsSample.Android/    # Android sample app
│   ├── ShadowsSample.iOS/        # iOS sample app
│   ├── ShadowsSample.UWP/        # UWP sample app
│   └── ShadowsSample.Tizen/      # Tizen sample app
├── Docs/                         # Documentation assets (images, etc.)
├── Sharpnado.Shadows.nuspec      # NuGet package specification
└── make-package.ps1              # PowerShell script for building NuGet package
```

## Build Commands

### Building the Xamarin.Forms library (legacy)
```bash
# Clean the solution
msbuild ./Shadows/Shadows.sln /t:Clean

# Restore packages
msbuild ./Shadows/Shadows.sln /t:Restore

# Build in Release mode
msbuild ./Shadows/Shadows.sln /t:Build /p:Configuration=Release

# Build in Debug mode
msbuild ./Shadows/Shadows.sln /t:Build /p:Configuration=Debug
```

### Building the sample application
```bash
# Build the sample solution
msbuild ./Sample/ShadowsSample.sln /t:Build /p:Configuration=Debug
```

### Building the MAUI library (.NET 9)

```bash
# From Maui.Shadows directory
cd Maui.Shadows
dotnet build

# Build for specific platform
dotnet build -f net9.0-android
dotnet build -f net9.0-ios
dotnet build -f net9.0-maccatalyst

# Clean build
dotnet clean && dotnet build
```

### Creating NuGet package
```powershell
# Legacy Xamarin.Forms package
pwsh ./make-package.ps1
```

Note: The `make-package.ps1` script will:
1. Set Xamarin.Forms version in all projects
2. Clean and rebuild the solution
3. Package the NuGet with version from assembly

**MAUI NuGet packaging** is not yet configured - will be added in future updates.

## Architecture

### Core Concepts

**Shadows Component**: A Xamarin.Forms `ContentView` that wraps any view and applies one or more shadow layers (called "Shades").

**Shade**: Represents an individual shadow with four properties:
- `Point Offset`: Shadow offset from the source
- `Color Color`: Shadow color
- `double Opacity`: Shadow opacity (0.0 to 1.0)
- `double BlurRadius`: Blur amount

### Platform-Specific Implementations

Each platform has its own rendering strategy optimized for performance:

**Android** (`Shadows.Droid/`):
- Uses `RenderScript` for blur effects
- Implements `ShadowView` as a custom Android `View`
- `BitmapCache` singleton provides bitmap caching to avoid recreating shadows
- Shadow bitmaps are created on-demand and cached by hash (color + size + blur)
- Key classes: `AndroidShadowsRenderer`, `ShadowView`, `BitmapCache`

**iOS** (`Shadows.iOS/`):
- Uses `CALayer` with native shadow properties
- `iOSShadowsController` manages sublayers for each shade
- Each shade becomes a separate `CALayer` with shadow properties
- Key classes: `iOSShadowsRenderer`, `iOSShadowsController`, `ShadeExtensions`

**UWP** (`Shadows.UWP/`):
- Uses `SpriteVisual` drop shadows from the Windows Composition API
- Key classes: `UWPShadowsRenderer`, `UWPShadowsController`

**Tizen** (`Shadows.Tizen/`):
- Uses `SkiaSharp` for rendering
- Key classes: `TizenShadowsRenderer`

### Core Library Structure

The core `.NET Standard 2.0` library (`Shadows/Shadows/`) contains:

- **Shadows.cs**: Main component that extends `ContentView`
- **Shade.cs**: Bindable properties for individual shadow configuration
- **Initializer.cs**: Library initialization with logging configuration
- **Markup Extensions**: XAML helpers
  - `ImmutableShadesExtension`: Creates readonly shade collections
  - `ShadeStackExtension`: Creates mutable observable collections
  - `SingleShadeExtension`: Convenience for single shadows
  - `NeumorphismShadesExtension`: Pre-configured Neumorphism shadows
- **InternalLogger.cs**: Internal logging system

### Key Design Patterns

**Shade Collections**:
- `ReadOnlyCollection<Shade>`: Immutable, shades are cloned to prevent leaks. Use for static ResourceDictionary definitions.
- `ObservableCollection<Shade>`: For dynamic addition/removal of shades during view lifetime
- `IEnumerable<Shade>`: For modifying shade properties during view lifetime

**Important**: Non-ReadOnly shades MUST be declared as transient instances, never as static resources.

**Weak Events**: The library uses `WeakEvent` pattern to prevent memory leaks when shades are added/removed or properties change.

**Platform Renderers**: Each platform implements a custom renderer that:
1. Creates platform-specific shadow views/layers
2. Subscribes to shade collection changes
3. Subscribes to individual shade property changes
4. Updates shadows when properties change

## Initialization Requirements

**iOS**: Must call after `Xamarin.Forms.Forms.Init()`:
```csharp
Sharpnado.Shades.iOS.iOSShadowsRenderer.Initialize();
```

**UWP**: Must register renderers before `Xamarin.Forms.Forms.Init()`:
```csharp
var rendererAssemblies = new[] { typeof(UWPShadowsRenderer).GetTypeInfo().Assembly };
Xamarin.Forms.Forms.Init(e, rendererAssemblies);
```

**Tizen**: Must call after `Xamarin.Forms.Forms.Init()`:
```csharp
Sharpnado.Shades.Tizen.TizenShadowsRenderer.Initialize();
```

**Core**: In App.xaml.cs (optional, for logging):
```csharp
Sharpnado.Shades.Initializer.Initialize(loggerEnable: false);
```

## Performance Considerations

- **Android**: Shadow rendering is CPU-intensive. Bitmap caching significantly improves performance. Avoid animating shade properties (Color, Blur, Opacity, Size) as it creates multiple bitmaps. Animating the view itself (rotation, scale) is fine.
- **All Platforms**: When animating view size, temporarily disable shadows by setting `Shades` to empty collection, then restore after animation completes.

## Dependencies

- **ThomasLevesque.WeakEvent** (4.1.0): Weak event pattern implementation
- **Xamarin.Forms** (3.6.0.220655+): Cross-platform UI framework
- **SkiaSharp.Views** (Tizen only): 2D graphics rendering

## Code Style

- Project uses **StyleCop.Analyzers** for code style enforcement
- StyleCop rules defined in `Shadows/StyleCopRules.ruleset`
- Assembly info is manually managed (GenerateAssemblyInfo is false)
- Language version set to `latest`

## Namespace Convention

Note: The assembly is named `Sharpnado.Shadows` but the root namespace is `Sharpnado.Shades`. This is intentional for XAML schema registration.
