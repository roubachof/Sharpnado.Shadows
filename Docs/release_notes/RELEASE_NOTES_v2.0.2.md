# Release Notes v2.0.2

## 🐛 Windows Content Margin Fix

**Type:** Bugfix

This release fixes a critical layout issue on Windows (WinUI) where MAUI margin properties were not being applied to content views within the Shadows handler.

## 🐛 Issue

In the Windows platform implementation, content views (such as `Border` or `Frame` with `Margin` set) were added directly to the native Grid without their MAUI layout properties being transferred to the platform view. This caused all content to render without spacing, ignoring margin values.

```xaml
<!-- This margin was being ignored on Windows -->
<sho:Shadows CornerRadius="10">
    <Border Margin="10" Background="White">
        <Label Text="Content" />
    </Border>
</sho:Shadows>
```

## ✅ Fix

The Windows `ShadowsHandler` now explicitly transfers the MAUI content's `Margin` property to the native WinUI `FrameworkElement`:

```csharp
// Manually apply MAUI margin to the platform view
var contentMargin = shadowsView.Content.Margin;
_contentPlatformView.Margin = new Thickness(
    contentMargin.Left,
    contentMargin.Top,
    contentMargin.Right,
    contentMargin.Bottom);
```

This ensures proper spacing and layout behavior consistent with other platforms.

## 📝 Technical Details

- Modified: `Maui.Shadows/Platforms/Windows/ShadowsHandler.cs`
- Added explicit margin transfer from MAUI view to WinUI platform view
- No changes to other platforms (Android, iOS, MacCatalyst remain unchanged)

## 📦 Installation

```bash
dotnet add package Sharpnado.Maui.Shadows --version 2.0.2
```

## 🔄 Migration from 2.0.1

Simply update the package version. No code changes required. Windows layouts with content margins will now render correctly.

## 📚 Documentation

- Main README: https://github.com/roubachof/Sharpnado.Shadows
- Repository: https://github.com/roubachof/Sharpnado.Shadows

## 🙏 Credits

Thanks to the community for reporting this Windows layout issue.
