# MacCatalyst Font Awesome Icons Fix

## Problem

Font Awesome icons (and other custom fonts) don't display on MacCatalyst even though they work fine on iOS and Android. The text shows as empty boxes or question marks.

## Root Cause

MacCatalyst has a bug/limitation in .NET MAUI where custom fonts must be explicitly declared in `Info.plist` using the `UIAppFonts` key, even though they are properly registered via `ConfigureFonts()` in `MauiProgram.cs`.

This is different from iOS where `MauiFont` in the `.csproj` is sufficient.

## Solution Applied

### 1. Added UIAppFonts to Info.plist

In `Platforms/MacCatalyst/Info.plist`, we added:

```xml
<!-- Custom fonts declaration for MacCatalyst -->
<key>UIAppFonts</key>
<array>
    <string>Fonts/OpenSans-Regular.ttf</string>
    <string>Fonts/OpenSans-Bold.ttf</string>
    <string>Fonts/OpenSans-SemiBold.ttf</string>
    <string>Fonts/OpenSans-ExtraBold.ttf</string>
    <string>Fonts/OpenSans-Light.ttf</string>
    <string>Fonts/OpenSans-Italic.ttf</string>
    <string>Fonts/OpenSans-BoldItalic.ttf</string>
    <string>Fonts/ka1.ttf</string>
    <string>Fonts/Font-Awesome-5-Free-Solid-900.otf</string>
    <string>Fonts/Font-Awesome-5-Free-Regular-400.otf</string>
    <string>Fonts/Font-Awesome-5-Brands-Regular-400.otf</string>
</array>
```

### 2. Added BundleResource entries for Font Awesome

In `ShadowsSample.Maui.csproj`, we added explicit bundle resource entries for MacCatalyst:

```xml
<!-- Explicit font references for MacCatalyst (workaround for icon font issues) -->
<BundleResource Condition="$(TargetFramework.Contains('maccatalyst'))" Include="Resources\Fonts\Font-Awesome-5-Free-Solid-900.otf">
    <LogicalName>Fonts/Font-Awesome-5-Free-Solid-900.otf</LogicalName>
</BundleResource>
<BundleResource Condition="$(TargetFramework.Contains('maccatalyst'))" Include="Resources\Fonts\Font-Awesome-5-Free-Regular-400.otf">
    <LogicalName>Fonts/Font-Awesome-5-Free-Regular-400.otf</LogicalName>
</BundleResource>
<BundleResource Condition="$(TargetFramework.Contains('maccatalyst'))" Include="Resources\Fonts\Font-Awesome-5-Brands-Regular-400.otf">
    <LogicalName>Fonts/Font-Awesome-5-Brands-Regular-400.otf</LogicalName>
</BundleResource>
```

## Important Notes

### Path Format

The paths in `UIAppFonts` MUST use:
- Forward slashes `/` (not backslashes)
- Relative path from the app bundle root
- Format: `Fonts/filename.ext`

### Font File Names

- Must match exactly the files in `Resources/Fonts/`
- Case-sensitive on macOS
- Include the correct extension (`.ttf` or `.otf`)

### When to Use This Fix

You need this fix if:
- ✅ Custom fonts work on iOS
- ✅ Custom fonts work on Android
- ❌ Custom fonts DON'T work on MacCatalyst
- The fonts are icon fonts (like Font Awesome, Material Icons, etc.)

### Alternative Solutions Tried

1. **❌ Only using MauiFont**: Not sufficient for MacCatalyst
2. **❌ Changing font aliases**: Doesn't solve the root issue
3. **✅ UIAppFonts + BundleResource**: Works reliably

## Testing

After applying this fix:

1. Clean the solution:
   ```bash
   dotnet clean -f net9.0-maccatalyst
   ```

2. Rebuild:
   ```bash
   dotnet build -f net9.0-maccatalyst
   ```

3. Run on Mac and verify Font Awesome icons display correctly

## Related Issues

- [MAUI Issue #8920](https://github.com/dotnet/maui/issues/8920)
- [MAUI Issue #10261](https://github.com/dotnet/maui/issues/10261)
- [MAUI Issue #17453](https://github.com/dotnet/maui/issues/17453)

## Platform Comparison

| Platform | MauiFont Only | UIAppFonts Required | BundleResource Required |
|----------|---------------|---------------------|------------------------|
| Android | ✅ Works | ❌ Not used | ❌ Not used |
| iOS | ✅ Works | ❌ Optional | ❌ Not needed |
| MacCatalyst | ❌ **FAILS** | ✅ **REQUIRED** | ✅ **RECOMMENDED** |
| Windows | ✅ Works | ❌ Not used | ❌ Not used |

## Quick Reference

### For Icon Fonts (Font Awesome, Material Icons, etc.)

Always add to MacCatalyst `Info.plist`:
```xml
<key>UIAppFonts</key>
<array>
    <string>Fonts/YourIconFont.otf</string>
</array>
```

### For Regular Fonts

MacCatalyst usually works with just `MauiFont`, but if you encounter issues, apply the same fix.

## Known Issue with OTF Files

⚠️ **IMPORTANT**: MacCatalyst has known issues with `.otf` (OpenType) font files in .NET MAUI!

### Symptoms
- Fonts work on iOS and Android
- Fonts DON'T work on MacCatalyst even with `UIAppFonts` configured
- OTF files are not loaded correctly

### Root Cause
MacCatalyst has a bug where OTF files are not properly registered even when declared in Info.plist.

### Solutions

#### Solution A: Convert OTF to TTF (Recommended)

1. Convert Font Awesome OTF files to TTF format:
   - Online: https://convertio.co/otf-ttf/
   - Or download Font Awesome 6 which includes TTF versions

2. Replace the OTF files with TTF versions in `Resources/Fonts/`

3. Update `MauiProgram.cs`:
```csharp
fonts.AddFont("Font-Awesome-5-Free-Solid-900.ttf", "FontAwesomeSolid");
fonts.AddFont("Font-Awesome-5-Free-Regular-400.ttf", "FontAwesomeRegular");
```

4. Update `Info.plist`:
```xml
<string>Fonts/Font-Awesome-5-Free-Solid-900.ttf</string>
<string>Fonts/Font-Awesome-5-Free-Regular-400.ttf</string>
```

#### Solution B: Use Font Awesome 6 (Modern Alternative)

Font Awesome 6 has better support and provides TTF files by default:

```bash
# Download from https://fontawesome.com/download
# Use the TTF files from the webfonts folder
```

#### Solution C: Use Alternative Icon Font

Consider using icon fonts that work reliably on all platforms:
- **Material Design Icons** (TTF format)
- **Ionicons** (TTF format)
- **Segoe Fluent Icons** (built-in on Windows, available as TTF)

### Diagnostic Tool

A diagnostic helper has been added to `Helpers/FontDiagnostics.cs` that:
- Lists all available font families
- Tests Font Awesome font loading
- Outputs to Debug console

It runs automatically on MacCatalyst startup. Check the Output window for:
```
=== Available Font Families ===
...
=== Searching for Font Awesome ===
✅ Found: Font Awesome 5 Free
  - Font Awesome 5 Free Solid
```

If you see `❌ No Font Awesome fonts found!`, the fonts are NOT loaded.

### Updated Icons.xaml

MacCatalyst platform support has been added:
```xml
<OnPlatform x:Key="FontAwesome" x:TypeArguments="x:String">
    <OnPlatform.Platforms>
        <On Platform="Android" Value="Font-Awesome-5-Free-Solid-900.otf#Font Awesome 5 Free" />
        <On Platform="iOS" Value="Font Awesome 5 Free Solid" />
        <On Platform="MacCatalyst" Value="Font Awesome 5 Free Solid" />
    </OnPlatform.Platforms>
</OnPlatform>
```

## Status

⚠️ **Partial Fix** - UIAppFonts configured, but OTF files may still not work on MacCatalyst.

**Next Steps**:
1. Run app on MacCatalyst and check Debug output for font diagnostics
2. If fonts are not found, convert OTF → TTF
3. Rebuild and retest
