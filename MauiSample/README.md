# Sharpnado.Shadows MAUI Sample

.NET 9 MAUI sample application demonstrating Sharpnado.Shadows functionality.

## Project Structure

```
MauiSample/
└── ShadowsSample.Maui/        # .NET 9 MAUI application
    ├── MainPage.xaml           # Main page with shadow examples
    ├── MainPage.xaml.cs        # Code-behind
    ├── MauiProgram.cs          # MAUI initialization with Shadows setup
    └── ShadowsSample.Maui.csproj  # Project file with reference to Maui.Shadows
```

## Features Demonstrated

### 1. Single Shadow
Simple shadow with customizable offset and opacity.

```xml
<sh:Shadows CornerRadius="10"
            Shades="{sh:SingleShade Offset='0,10', Opacity=0.3, Color=Black}">
    <Frame>
        <!-- Your content -->
    </Frame>
</sh:Shadows>
```

### 2. Colored Shadows
Multiple colored shadows for creative effects.

```xml
<sh:Shadows CornerRadius="15">
    <sh:Shadows.Shades>
        <sh:ImmutableShades>
            <sh:Shade BlurRadius="10" Opacity="0.6" Offset="-10,-10" Color="#FE99FE" />
            <sh:Shade BlurRadius="10" Opacity="0.6" Offset="10,10" Color="#00B0FB" />
        </sh:ImmutableShades>
    </sh:Shadows.Shades>
    <!-- Your content -->
</sh:Shadows>
```

### 3. Neumorphism
Pre-configured neumorphism effect.

```xml
<sh:Shadows CornerRadius="20"
            Shades="{sh:NeumorphismShades}">
    <Button Text="Neumorphism Button" />
</sh:Shadows>
```

### 4. Rounded Button with Shadow
Purple gradient-like shadow effect.

```xml
<sh:Shadows CornerRadius="30"
            Shades="{sh:SingleShade Offset='0,10', Opacity=0.7, Color=Purple}">
    <Button Text="Click me" />
</sh:Shadows>
```

## Setup

The sample demonstrates proper MAUI initialization in `MauiProgram.cs`:

```csharp
builder
    .UseMauiApp<App>()
    .UseSharpnadoShadows(
        enableLogging: true,
        enableDebugLogging: true,
        logFilter: "ShadowsHandler")
```

## Building

### Build for Android
```bash
cd MauiSample/ShadowsSample.Maui
dotnet build -f net9.0-android
```

### Build for iOS
```bash
dotnet build -f net9.0-ios
```

### Build for all platforms
```bash
dotnet build
```

## Running

### Android
```bash
dotnet build -t:Run -f net9.0-android
```

### iOS (macOS only)
```bash
dotnet build -t:Run -f net9.0-ios
```

## Requirements

- .NET 9 SDK
- MAUI workload installed (`dotnet workload install maui`)
- Platform-specific tools:
  - Android: Android SDK
  - iOS: Xcode (macOS only)
  - Windows: Windows SDK

## Notes

- The sample uses a light neumorphism background (#F0F0F3) to showcase shadows effectively
- Shadow rendering on Android uses RenderScript (deprecated on Android 31+, but still functional)
- Shadows are cached for performance - animating the view itself is fine, but animating shadow properties creates new bitmaps
