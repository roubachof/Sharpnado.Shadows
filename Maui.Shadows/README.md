# Sharpnado.Maui.Shadows

.NET 9 MAUI port of Sharpnado.Shadows

## Migration Status

### ✅ Completed

#### Core Library
- **Maui.Shadows.csproj** - Multi-targeted .NET 9 MAUI project
  - `net9.0` (base)
  - `net9.0-android`
  - `net9.0-ios`
  - `net9.0-maccatalyst`
  - `net9.0-windows10.0.19041.0` (Windows only)

#### Core Classes
- **Shade.cs** - Individual shadow configuration with bindable properties
  - Migrated from Xamarin.Forms to MAUI
  - Updated to use C# 11 features (nullable reference types, target-typed new)
  - Color API updated from Xamarin.Forms to MAUI (e.g., `Color.A` → `Color.Alpha`)

- **Shadows.cs** - Main ContentView container for shadow effects
  - Migrated from Xamarin.Forms to MAUI
  - Maintains weak event pattern for memory leak prevention
  - Updated to use nullable reference types

#### XAML Markup Extensions
- **ImmutableShadesExtension.cs** - ReadOnly shade collections for static resources
- **ShadeStackExtension.cs** - Observable shade collections for dynamic changes
- **SingleShadeExtension.cs** - Convenience extension for single shadow
- **NeumorphismShadesExtension.cs** - Pre-configured neumorphism shadow pairs

#### Infrastructure
- **InternalLogger.cs** - Internal logging system
  - Updated for .NET 9 (Action<> instead of custom delegate)
  - Maintains backward compatibility with filtering

- **MauiAppBuilderExtensions.cs** - MAUI initialization pattern
  - Replaces old Xamarin.Forms Initializer class
  - Follows MAUI builder pattern: `builder.UseSharpnadoShadows()`
  - Placeholder for future platform handler registration

### 🚧 Pending

#### Platform Handlers (To be implemented)
- **Android Handler** - RenderScript-based shadow rendering with bitmap caching
- **iOS/MacCatalyst Handler** - CALayer-based shadow implementation
- **Windows Handler** - SpriteVisual drop shadow implementation
- **Tizen Handler** - SkiaSharp-based rendering (optional)

## Usage

### Basic Setup in MauiProgram.cs

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSharpnadoShadows(
                enableLogging: false,
                enableDebugLogging: false
            );

        return builder.Build();
    }
}
```

### XAML Usage (same as Xamarin.Forms)

```xml
xmlns:sh="clr-namespace:Sharpnado.Shades;assembly=Sharpnado.Maui.Shadows"

<sh:Shadows CornerRadius="10"
            Shades="{sh:SingleShade Offset='0,10', 
                                    Opacity=0.7, 
                                    Color=Purple}">
    <Button Text="Click Me" />
</sh:Shadows>
```

## Key Changes from Xamarin.Forms

1. **Namespace**: Still uses `Sharpnado.Shades` namespace (assembly name is `Sharpnado.Maui.Shadows`)
2. **Initialization**: Changed from `Initializer.Initialize()` to `builder.UseSharpnadoShadows()`
3. **Color API**: Updated to MAUI Color API (Alpha, Red, Green, Blue instead of A, R, G, B)
4. **Target Framework**: .NET 9 with multi-targeting for all MAUI platforms
5. **Minimum OS Versions**:
   - Android: 21.0 (Android 5.0)
   - iOS: 12.2
   - MacCatalyst: 15.0
   - Windows: 10.0.17763.0

## Building

```bash
# From Maui.Shadows directory
dotnet build

# For specific platform
dotnet build -f net9.0-android
dotnet build -f net9.0-ios
```

## Next Steps

1. Implement Android Handler with RenderScript
2. Implement iOS/MacCatalyst Handler with CALayer
3. Implement Windows Handler with SpriteVisual
4. Create sample MAUI application
5. Port unit tests (if any)
6. Update NuGet packaging script
