# Release Notes v2.0.1

## 📦 Windows Platform Inclusion

**Type:** Packaging Fix

This is a re-release of v2.0.0 to include Windows platform binaries.

## 🐛 Issue

Version 2.0.0 was packaged on macOS, which resulted in the Windows target framework (`net9.0-windows10.0.19041.0`) being excluded from the NuGet package.

## ✅ Fix

Version 2.0.1 includes all platform binaries:
- ✅ `net9.0` - .NET 9 base
- ✅ `net9.0-android` - Android
- ✅ `net9.0-ios` - iOS
- ✅ `net9.0-maccatalyst` - MacCatalyst
- ✅ `net9.0-windows10.0.19041.0` - Windows (now included)

## 📝 Code Changes

**No code changes** - This release is functionally identical to v2.0.0.

All features from v2.0.0 remain unchanged:
- Full .NET 9 MAUI support with modern handlers architecture
- Android BlurType property (Gpu/StackBlur)
- Android improved GPU-based blur rendering
- iOS/MacCatalyst complete rewrite using ViewHandler
- Windows full WinUI 3 support with Composition API
- MacCatalyst full platform support
- Complete handler refactoring for all platforms
- Fixed memory leaks in Windows handler
- Enhanced logging and debugging capabilities

## 📦 Installation

```bash
dotnet add package Sharpnado.Maui.Shadows --version 2.0.1
```

## 🔄 Migration from 2.0.0

Simply update the package version. No code changes required.

Windows developers who were unable to use v2.0.0 can now use v2.0.1.

## 📚 Documentation

For full v2.0.0 feature documentation, see:
- Main README: https://github.com/roubachof/Sharpnado.Shadows
- Blog post: [Docs/blog-post-shadows-2.0.md](../blog-post-shadows-2.0.md)

Repository: https://github.com/roubachof/Sharpnado.Shadows
