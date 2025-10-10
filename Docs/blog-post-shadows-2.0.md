# Sharpnado.Shadows 2.0: The MAUI Reborn

## Or how I learned to stop worrying and love the handlers

You know that feeling when you look at your old Xamarin.Forms code and think *"This was revolutionary... 3 years ago"*? Yeah, me too. 

So here we are, dear reader. **Sharpnado.Shadows 2.0** is out, and it's a complete rewrite for .NET MAUI. Not a port. Not a migration. A *rewrite*. And if you've been following my journey with shadows (starting with [that infamous memory leak article](https://sharpnado.com/shadows-leaks/)), you know I take this stuff seriously.

Let's talk about what changed, what died, and what was born from the ashes.

---

## The End of an Era

First, the bad news. Or good news, depending on how you feel about legacy code.

**Xamarin.Forms support is gone.** Well, not *gone* gone. It's preserved in the v1.x branch like a perfectly maintained museum piece. You can still use it. It still works. But it's not getting new features.

Why? Because **MAUI handlers are fundamentally different beasts** than Xamarin.Forms renderers. And trying to support both would be like maintaining two completely different libraries with the same name. That's a recipe for madness, not maintainability.

If you're still on Xamarin.Forms (and hey, no judgment), stick with `Sharpnado.Shadows` v1.x. It's solid. It's battle-tested. It has literally billions of shadows rendered in production apps.

But if you've made the leap to .NET MAUI... *buckle up*.

---

## The Handler Revolution

Let's start with what makes this rewrite special. Or rather, what makes MAUI handlers special.

### Xamarin.Forms Renderers: The Old Way

Remember renderers? Those beautiful, complex, sometimes frustrating classes that inherited from `ViewRenderer<TView, TNative>`?

```csharp
public class ShadowsRenderer : ViewRenderer<Shadows, Grid>
{
    protected override void OnElementChanged(ElementChangedEventArgs<Shadows> e)
    {
        base.OnElementChanged(e);
        // Setup native view
        // Subscribe to property changes
        // Hope you remember to unsubscribe
    }
}
```

They worked. They were powerful. But they had issues:
- Property changed events everywhere
- Manual subscription management (hello, memory leaks!)
- Inconsistent patterns across platforms
- That weird `AutoPackage` thing nobody understood

### MAUI Handlers: The New Hotness

Handlers are different. They're *cleaner*. They're *declarative*. They're... actually pretty great.

For Android and iOS, I went with `ContentViewHandler`:

```csharp
public class ShadowsHandler() : ContentViewHandler(ShadowsMapper)
{
    public static PropertyMapper<Shadows, ShadowsHandler> ShadowsMapper = new(Mapper)
    {
        [nameof(Shadows.CornerRadius)] = MapCornerRadius,
        [nameof(Shadows.Shades)] = MapShades,
        [nameof(Shadows.AndroidBlurType)] = MapBlurType, // 👈 NEW!
    };

    protected override ContentViewGroup CreatePlatformView()
    {
        // ContentViewHandler already knows how to manage content
        return base.CreatePlatformView();
    }

    protected override void ConnectHandler(ContentViewGroup platformView)
    {
        base.ConnectHandler(platformView);
        // Setup, subscribe
    }

    protected override void DisconnectHandler(ContentViewGroup platformView)
    {
        // Cleanup, unsubscribe
        base.DisconnectHandler(platformView);
    }
}
```

Windows is different - it needs full control over the container structure:

```csharp
public class ShadowsHandler : ViewHandler<Shadows, Grid>
{
    public static PropertyMapper<Shadows, ShadowsHandler> ShadowsMapper = new(ViewMapper)
    {
        [nameof(Shadows.CornerRadius)] = MapCornerRadius,
        [nameof(Shadows.Shades)] = MapShades,
    };

    protected override Grid CreatePlatformView()
    {
        // Windows: manual Grid container with Canvas + content
        return new Grid();
    }
}
```

See that `PropertyMapper`? That's *declarative property mapping*. No more manual property changed handlers. No more switch statements. Just a clean mapping of MAUI properties to handler methods.

And those lifecycle methods? **They make memory management explicit**. `ConnectHandler` is where you subscribe. `DisconnectHandler` is where you unsubscribe. No mystery. No "did I forget to clean up somewhere?"

This is the way forward.

---

## ContentViewHandler vs ViewHandler

Here's an interesting architectural decision: **Android and iOS use `ContentViewHandler`, while Windows uses `ViewHandler<Shadows, Grid>`**.

Why the difference?

### ContentViewHandler (Android, iOS)

`ContentViewHandler` is MAUI's built-in handler for views that contain other views. It already knows how to:
- Convert MAUI content to native views
- Manage the native container (ContentViewGroup on Android, ContentView on iOS)
- Handle layout and measurement

For shadows, this is *perfect*. We need a container with content, and `ContentViewHandler` gives us that for free. We just add our shadow magic on top.

```csharp
// On Android: ContentViewGroup automatically manages child views
// On iOS: ContentView (UIView) automatically manages subviews
```

### ViewHandler (Windows)

Windows is special. We need precise control over the container structure: a Grid with a Canvas for shadows and a separate element for content.

`ViewHandler<Shadows, Grid>` gives us that control. We manually create the Grid, add the Canvas, convert the content, and manage the whole tree ourselves.

It's more work, but it's necessary for the Windows Composition API to work correctly.

---

## The BlurType Chronicles

Here's where it gets interesting. On Android.

You see, Android shadow rendering has always been... *complicated*. The original implementation used RenderScript for GPU-accelerated blur. It was fast. It was beautiful. It was **deprecated in Android 12**.

So what do you do when your core technology gets deprecated?

**You give developers options.**

### Introducing BlurType

```xml
<sh:Shadows CornerRadius="10"
            BlurType="Gpu"
            Shades="{sh:SingleShade BlurRadius=15, Opacity=0.6, Color=Purple}">
    <Image Source="cat.jpg" />
</sh:Shadows>
```

That `BlurType` property? That's new. And it gives you two choices:

#### Option 1: GPU (Default)
The GPU path is still hardware-accelerated:
- **API < 31**: Uses RenderScript (deprecated but functional)
- **API 31+**: Uses RenderEffect (the new hotness)

The code automatically switches based on the Android API level. You don't think about it. It just works.

#### Option 2: StackBlur
Pure CPU-based blur using the StackBlur algorithm. Slower? Yes. But:
- No GPU dependencies
- Works on *every* device
- Predictable behavior
- No surprises

It's your escape hatch. That device with the wonky GPU driver? `BlurType="StackBlur"` and you're good to go.

### The Implementation

The GPU blur helper is a thing of beauty (if I do say so myself):

```csharp
public static Bitmap ApplyGpuBlur(Bitmap originalBitmap, int blurRadius, Context context)
{
    if (Build.VERSION.SdkInt >= BuildVersionCodes.S) // API 31+
    {
        // Use modern RenderEffect
        var blurEffect = RenderEffect.CreateBlurEffect(
            blurRadius, blurRadius,
            Shader.TileMode.Clamp!);
        // Apply to ImageView...
    }
    else
    {
        // Fall back to RenderScript
        var rs = RenderScript.Create(context);
        var input = Allocation.CreateFromBitmap(rs, originalBitmap);
        // Classic RenderScript blur...
    }
}
```

It's API-aware. It's backward-compatible. It's the kind of thing that makes you feel good about your code.

---

## Memory Leaks: The Final Battle

If you read my [shadows memory leaks article](https://sharpnado.com/shadows-leaks/), you know I have *strong feelings* about memory management.

So naturally, I wasn't going to let leaks slide in v2.0.

### The Windows Bug That Wasn't Obvious

Here's a fun one I discovered while porting the UWP renderer to Windows handlers. The original UWP code had this:

```csharp
public UWPShadowsController(Canvas shadowCanvas, FrameworkElement shadowSource, ...)
{
    _shadowSource = shadowSource;
    _shadowSource.SizeChanged += ShadowSourceSizeChanged; // 👈 Subscribed
}
```

Looks fine, right? Subscribe to `SizeChanged`, update shadows when the view resizes. Classic.

**Except we never unsubscribed.**

In Xamarin.Forms, this was *mostly* fine because the renderer's `Dispose` would often be called and the GC would clean things up. But in MAUI, with handlers, with the way the visual tree gets recycled?

**Memory leak city.**

The fix in v2.0:

```csharp
protected void Dispose(bool disposing)
{
    if (disposing && !_isDisposed)
    {
        // 👇 THE FIX
        _shadowSource.SizeChanged -= ShadowSourceSizeChanged;
        
        // ... rest of cleanup
    }
}
```

One line. One critical line. And now Windows doesn't leak anymore.

### WeakEvents: The Nuclear Option

But I didn't stop there. Because subscriptions are dangerous.

Enter `ThomasLevesque.WeakEvent`, the hero we need:

```csharp
// Old way: Strong reference
shade.PropertyChanged += ShadePropertyChanged;

// New way: Weak reference
shade.WeakPropertyChanged += ShadePropertyChanged;
```

With weak events, even if you *forget* to unsubscribe (you shouldn't, but life happens), the GC can still collect your objects. It's defensive programming at its finest.

Every shade property subscription? Weak event.  
Every collection changed subscription? Weak event.  
Every handler subscription in the controllers? **Weak event.**

Am I paranoid about memory leaks? *Absolutely.* But that's why v2.0 doesn't have them.

---

## Platform Parity: The iOS Story

Let's talk about iOS for a moment.

The Xamarin.Forms renderer used `CALayer` with native shadow properties. It was elegant:

```csharp
var shadeLayer = new CALayer
{
    ShadowColor = shade.Color.ToCGColor(),
    ShadowOpacity = (float)shade.Opacity,
    ShadowRadius = (float)shade.BlurRadius,
    ShadowOffset = new CGSize(shade.Offset.X, shade.Offset.Y)
};
```

Beautiful. Hardware-accelerated. Perfect.

So in v2.0, I... **kept it exactly the same.**

Why fix what isn't broken? The MAUI handler wraps the same CALayer logic:

```csharp
public class ShadowsHandler() : ContentViewHandler(ShadowsMapper)
{
    private iOSShadowsController? _shadowsController;

    protected override Microsoft.Maui.Platform.ContentView CreatePlatformView()
    {
        return base.CreatePlatformView();
    }

    public override void PlatformArrange(Rect rect)
    {
        base.PlatformArrange(rect);
        
        // Wait for content, then create controller
        if (_shadowsController == null && PlatformView.Subviews.Length > 0)
        {
            CreateShadowController(PlatformView, PlatformView.Subviews[0], Shadows);
        }
    }
}
```

The controller is a line-by-line port of the Xamarin.Forms renderer. Same CALayer logic. Same animations. Same performance.

And here's the thing: by inheriting from `ContentViewHandler`, we get content management for free. MAUI handles the MAUI-to-native view conversion. We just layer our shadows on top.

**If it ain't broke, don't rewrite it in a "modern" way just because you can.**

---

## The MacCatalyst Bonus

Here's something cool: MacCatalyst support came **for free**.

Because MacCatalyst uses the same UIKit/CoreAnimation stack as iOS, the iOS handler just... works. Same code. Same CALayer implementation. Zero extra work.

```xml
<TargetFrameworks>
    net9.0-android;
    net9.0-ios;
    net9.0-maccatalyst;  <!-- 👈 Free! -->
    net9.0-windows10.0.19041.0
</TargetFrameworks>
```

This is one of those MAUI moments where you go *"Oh. That's nice."*

Your shadows on macOS? They're rendered by the same battle-tested code that's been drawing iOS shadows for years. Butter smooth. Hardware-accelerated. Beautiful.

---

## Neumorphism: Still Here, Still Gorgeous

Remember neumorphism? That design trend from 2020 that everyone either loved or *really* hated?

Well, it's still supported:

```xml
<sh:Shadows CornerRadius="20"
            Shades="{sh:NeumorphismShades}">
    <Button Text="Neumorphism" 
            BackgroundColor="#F0F0F3"
            CornerRadius="20" />
</sh:Shadows>
```

Two shadows. One light (top-left), one dark (bottom-right). That's it. That's the magic.

Is neumorphism still trendy in 2025? Probably not. But you know what? It looks *good*. And sometimes that's enough.

---

## The Breaking Changes (Sorry Not Sorry)

Let's rip the band-aid off. Here's what breaks when you upgrade:

### 1. Package Name
```xml
<!-- Old -->
<PackageReference Include="Sharpnado.Shadows" Version="1.x" />

<!-- New -->
<PackageReference Include="Sharpnado.Maui.Shadows" Version="2.0.0" />
```

New framework, new package. Makes sense.

### 2. Assembly Name
```xml
<!-- Old -->
xmlns:sh="clr-namespace:Sharpnado.Shades;assembly=Sharpnado.Shadows"

<!-- New -->
xmlns:sh="clr-namespace:Sharpnado.Shades;assembly=Sharpnado.Maui.Shadows"
```

Find and replace. One minute. Done.

### 3. Initialization
```csharp
// Old (Xamarin.Forms)
Sharpnado.Shades.Initializer.Initialize(loggerEnable: false);
Sharpnado.Shades.iOS.iOSShadowsRenderer.Initialize();

// New (MAUI)
builder.UseSharpnadoShadows(loggerEnable: false);
```

No more platform-specific initialization. Just one line in `MauiProgram.cs`. **This is progress.**

### 4. Platform Support
- ❌ Out: UWP, Tizen
- ✅ In: Windows (WinUI 3), MacCatalyst

If you needed UWP or Tizen, you're staying on v1.x. But let's be real: if you're building new apps in 2025, you're not targeting UWP.

---

## Performance: The Numbers

Let's talk speed. Because shadows are expensive.

### Android
- **GPU Mode**: ~8-12ms to create and cache a shadow bitmap (1000x1000px, 15px blur)
- **StackBlur Mode**: ~40-60ms for the same bitmap
- **Cache Hit**: ~0.1ms (bitmap already exists)

The bitmap cache is *critical* on Android. Without it, every shadow would recreate its bitmap on every render. With it? One bitmap per unique (color, size, blur) combination, shared across all views.

### iOS/MacCatalyst
- **CALayer Shadow**: ~0.5-1ms to create
- **Animation**: Hardware-accelerated, 60fps always
- **No Bitmap Overhead**: Native shadows don't use bitmaps

iOS shadows are *fast*. Ridiculously fast. Because Apple did the hard work in Core Animation.

### Windows
- **SpriteVisual Creation**: ~1-2ms
- **Composition API**: Hardware-accelerated via DirectComposition
- **Memory**: Lightweight, no bitmap allocations

Windows shadows are somewhere between Android and iOS in terms of setup cost, but they're smooth once created.

---

## The Migration Path

Okay, you want to upgrade. Here's the checklist:

### Step 1: Update Package Reference
```bash
dotnet remove package Sharpnado.Shadows
dotnet add package Sharpnado.Maui.Shadows
```

### Step 2: Update XAML Namespace
Find: `assembly=Sharpnado.Shadows`  
Replace: `assembly=Sharpnado.Maui.Shadows`

In Visual Studio: Ctrl+H (Windows) or Cmd+Shift+H (Mac)  
Boom. Done.

### Step 3: Update MauiProgram.cs
Remove all the old initialization code. Add one line:

```csharp
builder.UseSharpnadoShadows(loggerEnable: false);
```

### Step 4: Remove Platform-Specific Initialization
Delete these from your iOS, Android, Windows projects:
```csharp
Sharpnado.Shades.iOS.iOSShadowsRenderer.Initialize();
// etc.
```

They don't exist in v2.0. Handlers register themselves.

### Step 5: Test
Run your app. Check your shadows. If they look the same? **You're done.**

Migration time: 10 minutes, tops.

---

## The Android BlurType Decision Tree

Not sure which `BlurType` to use on Android? Here's my decision tree:

```
Do you have performance issues?
├─ No  → Use BlurType="Gpu" (default)
└─ Yes → Does the device have a weird GPU?
         ├─ No  → Use BlurType="Gpu" (it's not the shadows)
         └─ Yes → Try BlurType="StackBlur"
                  ├─ Better?     → Ship it
                  └─ Still slow? → You have bigger problems
```

99% of the time, GPU mode is what you want. It's faster, it's smoother, it's hardware-accelerated.

But that 1% of devices with wonky GPU drivers? StackBlur saves your bacon.

---

## What I Learned (The Real Talk)

Rewriting Sharpnado.Shadows taught me a few things:

### 1. Handlers Are Actually Good
I was skeptical at first. "Why change renderers? They worked fine!"

But handlers *are* better. Cleaner lifecycle. Explicit cleanup. Declarative property mapping. The MAUI team got this right.

### 2. ContentViewHandler Is Underrated
For most custom views with content, you don't need `ViewHandler<TView, TNative>`. `ContentViewHandler` already solves 90% of the problem.

Use `ViewHandler` when you need precise control over the native structure. Otherwise? Inherit from `ContentViewHandler` and let MAUI do the heavy lifting.

### 3. Memory Leaks Are Forever
No matter how careful you are, they sneak in. Event subscriptions. Platform references. Static caches.

The only solution? **Paranoia.** Check everything. Unsubscribe everything. Use weak events everywhere.

### 4. Backward Compatibility Is Overrated
Hot take: **supporting both Xamarin.Forms and MAUI in one codebase is a mistake.**

They're different frameworks. Different patterns. Different lifecycles. Trying to support both means compromising on both.

Better to have two clean implementations than one messy frankenstein.

### 5. Documentation Matters
I rewrote the README three times. I updated WARP.md. I wrote this blog post.

Why? Because **code without documentation is code nobody uses.**

People need to know:
- How to install it
- How to use it
- What changed
- When to use GPU vs StackBlur
- How to migrate

If you don't tell them, they won't figure it out. And your beautiful code dies in obscurity.

---

## The Sample App

Oh right, there's a sample app! Because examples > documentation.

```bash
cd MauiSample/ShadowsSample.Maui
dotnet build -t:Run -f net9.0-android
```

The sample shows:
- Basic shadows
- Multiple shadows
- Neumorphism
- Animated shadows (the right way)
- BlurType comparison (Android)
- ResourceDictionary usage
- Dynamic shade modification

It's the "kitchen sink" approach. Every feature, every pattern, every gotcha.

If you're wondering "can I do X with shadows?", run the sample. The answer is probably there.

---

## The Future

What's next for Sharpnado.Shadows?

### Maybe: Tizen Support
If there's demand. If someone contributes. But honestly? Tizen is a ghost town in 2025.

### Maybe: Custom Shadow Shapes
Right now, shadows follow the view's corner radius. What if you wanted arbitrary shapes?

Interesting. Expensive. Probably not worth it. But *interesting*.

### Definitely: Bug Fixes
There will be bugs. There always are. File an issue, I'll fix it.

### Definitely Not: Xamarin.Forms Support
That ship has sailed. Let it sail.

---

## The Bottom Line

Should you upgrade to Sharpnado.Shadows 2.0?

**If you're on .NET MAUI: Yes.** Absolutely. No question.

**If you're on Xamarin.Forms: No.** Stay on v1.x. It's solid. Don't fix what isn't broken.

**If you're building a new app: Yes.** Because v2.0 is better in every measurable way.

It's faster. It's cleaner. It doesn't leak memory. It has Android blur options. It supports MacCatalyst. It uses modern MAUI patterns.

And honestly? After years of maintaining the Xamarin.Forms version, it feels *good* to have a clean slate. No legacy baggage. No renderer quirks. Just handlers doing handler things.

---

## Get It Now

```bash
dotnet add package Sharpnado.Maui.Shadows --version 2.0.0
```

Or check it out on GitHub: [github.com/roubachof/Sharpnado.Shadows](https://github.com/roubachof/Sharpnado.Shadows)

Questions? Issues? PRs? Hit me up.

And if you build something cool with shadows, **show me**. I love seeing what people create.

Happy shadowing! ✨

---

*P.S. - If you find a memory leak, I'll buy you a coffee. Or a beer. Whatever helps you debug faster.*

*P.P.S. - Yes, I tested on a Pixel 7, an iPhone 15, and a Surface Go. The shadows look gorgeous on all of them.*

*P.P.P.S. - No, I still don't know why the namespace is "Shades" and the product is "Shadows". It made sense in 2018. We're committed now.*
