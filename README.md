# Sharpnado.Shadows for .NET MAUI

<p align="left"><img src="Docs/shadows.png" height="180"/>

Get it from NuGet:

[![Nuget](https://img.shields.io/nuget/v/Sharpnado.Maui.Shadows.svg)](https://www.nuget.org/packages/Sharpnado.Maui.Shadows)

| Supported platforms              |
|----------------------------------|
| :heavy_check_mark: Android       |
| :heavy_check_mark: iOS           |
| :heavy_check_mark: Windows       |
| :heavy_check_mark: MacCatalyst   |

**Requirements:** .NET 9+

![Presentation](Docs/github_banner.png)

## Installation

1. Install the NuGet package in your .NET MAUI project:

```bash
dotnet add package Sharpnado.Maui.Shadows
```

2. In your `MauiProgram.cs`, register Sharpnado.Shadows handlers:

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

That's it! No platform-specific initialization required. The handlers are automatically registered for all platforms.

## Features

Add as **many** **custom** shadows as you like to any `.NET MAUI` view (`Android`, `iOS`, `Windows`, `MacCatalyst`).

* You can specify each shadow `Color`, `Opacity`, `BlurRadius`, and `Offset`
* **Android**: Choose between `Gpu` or `StackBlur` rendering with the new `BlurType` property
* Simply implement `Neumorphism` design patterns
* You can add one shadow, 3 shadows, 99 shadows, to any `.NET MAUI` element
* Animate any of these properties and make the shadows dance around your elements
* Pure `.NET MAUI` implementation with no external dependencies (except ThomasLevesque.WeakEvent)

## Animating shadows

Rendering `Shadows` is GPU/CPU intensive (especially on Android with `StackBlur`).

**Using .NET MAUI animation API with shadows is totally fine**: it won't recreate the `Shadows` bitmaps.

However, animating the color, blur, opacity or size of a `Shade`, will result in creating **multiple bitmaps on Android**.

Therefore if you want to animate the size of a view which is using `Shadows`, you should "disable" the shadows during the animation.

**Tip**: On Android, prefer `BlurType="Gpu"` for better performance when animating views.

```xml
<sh:Shadows x:Name="MyViewShadows"
            CornerRadius="30"
            Shades="{sh:SingleShade Offset='0, 10',
                                    Opacity=0.7,
                                    Color=Violet}">
    <mine:SuperCustomView x:Name="MyView" />
</sh:Shadows>
```

```csharp
private async Task MyViewTotallyFineAnimations()
{
    // Nothing to disable here
    await MyView.RotateXTo(90);
    await MyView.ScaleTo(2);
    await MyView.RotateXTo(0);
    await MyView.ScaleTo(1);
}

private async Task MyViewNeedsDisableShadowsAnimation()
{
    var shades = MyViewShadows.Shades;
    
    // Disabling shadows
    MyViewShadows.Shades = new List<Shades>();

    await MyView.AnimateWidthRequestAsync();
    
    // Restoring shadows
    MyViewShadows.Shades = shades;
}
```

## Shadows for .NET MAUI component creators

`Shadows` has been developed with modularity in mind, making it really easy to integrate into your own .NET MAUI components.

## Using Shadows

`Shadows` is a container for any `.NET MAUI` view. 
Just wrap your view in it and start adding shadows:

**XAML**

```xml
<sh:Shadows x:Name="CatShadows"
            HorizontalOptions="Center"
            VerticalOptions="Center"
            CornerRadius="10">
    <sh:Shadows.Shades>
        <sh:ImmutableShades>
            <sh:Shade BlurRadius="10"
                      Opacity="0.5"
                      Offset="-10,-10"
                      Color="#FE99FE" />
            <sh:Shade BlurRadius="10"
                      Opacity="0.5"
                      Offset="10,10"
                      Color="#00B0FB" />
        </sh:ImmutableShades>
    </sh:Shadows.Shades>
    <Frame WidthRequest="80"
           Padding="10"
           BackgroundColor="White"
           CornerRadius="10">
        <Image Source="{images:ImageResource nyan_cat.png}" />
    </Frame>
</sh:Shadows>
```

**OUTPUT**

<img src="Docs/cat_shadows.png" height="180"/>

Thanks to the `CornerRadius` property you can match your target corner to achieve a perfect shadow.
For example, you can add a shadow to a rounded button:

**XAML**

```xml
<sh:Shadows CornerRadius="30"
            HorizontalOptions="Center"
            VerticalOptions="Center"
            Shades="{sh:SingleShade Offset='0, 10',
                                    Opacity=0.7,
                                    Color=Violet}">
    <ImageButton WidthRequest="60"
                 HeightRequest="60"
                 Padding="20"
                 BackgroundColor="Violet"
                 CornerRadius="30"
                 Source="{StaticResource IconPlusWhite}" />
</sh:Shadows>
```

**OUTPUT**

<img src="Docs/rounded_button.png" height="180"/>

### Choose your Shade Collection

You can use several type of `IEnumerable<Shade>`:

#### 1. ReadOnlyCollection<Shade>

This is what you want to use most of the time.
All the different `IMarkupExtension` like `ImmutableShades, NeumorphismShades, SingleShade`, return a `ReadOnlyCollection<Shade>`.
If you use a `ReadOnlyCollection<Shade>`, all shades will be cloned to be sure the immutability is respected.
It means, you can specify shades as static objects in your `ResourceDictionary`, it won't create any leak or view hierarchy issues.

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                    xmlns:sh="clr-namespace:Sharpnado.Shades;assembly=Sharpnado.Maui.Shadows">
    <sh:SingleShade x:Key="ShadowTop"
                    BlurRadius="6"
                    Opacity="0.15"
                    Offset="0,-8"
                    Color="{StaticResource ShadowsColor}" />

    <sh:SingleShade x:Key="ShadowBottom"
                    BlurRadius="6"
                    Opacity="0.1"
                    Offset="0,5"
                    Color="{StaticResource ShadowsColor}" />

    <sh:SingleShade x:Key="ShadowAccentBottom"
                    BlurRadius="6"
                    Opacity="0.4"
                    Offset="0,4"
                    Color="{StaticResource AccentColor}" />

    <sh:ImmutableShades x:Key="ShadowNone" />

    <sh:NeumorphismShades x:Key="ShadowNeumorphism" />

    <sh:NeumorphismShades x:Key="ShadowThinNeumorphism"
                          LowerOffset="8, 6"
                          UpperOffset="-8,-6" />
</ResourceDictionary>
```

#### 2. ObservableCollection<Shade>

Only if you want to dynamically add or remove shade during the view lifetime.

#### 3. All other IEnumerable<Shade>

If you want to modify a shade property during the view lifetime.

**IMPORTANT**: if you don't use a `ReadOnlyCollection<Shade>` please be sure to declare your `Shade` as transient. 
It means you should declare a new instance of `Shade` for each `Shadows` views. For example, in code-behind with `new Shade()`, or in xaml with `Shades` property. 
Just don't reference static instances of shade from `ResourceDictionary` with `StaticResource` references, or even in a C# class.

### Shades

The `Shadows` component has only 2 properties:

1. `int CornerRadius` which should be equal to the component's child view `CornerRadius` to achieve a good shadow effect,
2. `IEnumerable<Shade> Shades` which is the enumeration of all the shades this shadow is made of.

A shade is what you could call a "sub-shadow". 

Each shade has 4 properties:

1. `Point Offset`: the offset of the shade
2. `Color Color`: the color of the shade
3. `double Opacity`: the opacity of the shade
4. `double BlurRadius`: the amount of blur your want for this shade

**Logo.xaml**

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<sh:Shadows x:Class="ShadowsSample.Views.Logo"
            xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
            xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
            xmlns:sh="clr-namespace:Sharpnado.Shades;assembly=Sharpnado.Maui.Shadows"
            VerticalOptions="Center"
            CornerRadius="3">
    <sh:Shadows.Shades>
        <sh:ImmutableShades>
            <sh:Shade BlurRadius="10"
                      Opacity="1"
                      Offset="-15,-15"
                      Color="Yellow" />
            <sh:Shade BlurRadius="10"
                      Opacity="1"
                      Offset="15,15"
                      Color="Yellow" />
            <sh:Shade BlurRadius="10"
                      Opacity="1"
                      Offset="-15,15"
                      Color="Violet" />
            <sh:Shade BlurRadius="10"
                      Opacity="1"
                      Offset="15,-15"
                      Color="Violet" />
            <sh:Shade BlurRadius="5"
                      Opacity="1"
                      Offset="-5,-5"
                      Color="DeepSkyBlue" />
            <sh:Shade BlurRadius="5"
                      Opacity="1"
                      Offset="5,5"
                      Color="DeepSkyBlue" />
            <sh:Shade BlurRadius="5"
                      Opacity="1"
                      Offset="0,0"
                      Color="White" />
        </sh:ImmutableShades>
    </sh:Shadows.Shades>
    <Label Style="{StaticResource TextHuge}"
           FontFamily="{StaticResource FontKarmatic}"
           Text="Shadows" />
</sh:Shadows>
```

**OUTPUT**

<img src="Docs/logo.png" height="180"/>

### Neumorphism

To have a nice `Neumorphism` effect we need to choose a background color.
I found that `#F0F0F3` was quite good, so I will stick to it for our content **and** our page background color.

Since `Neumorphism` implementation is made of 2 shadows, one bright at the top left, one dark at the bottom right, achieving a `Neumorphism` style with `Shadows` for all the views is really easy:

```xml
<Style ApplyToDerivedTypes="True" TargetType="sh:Shadows">
    <Setter Property="CornerRadius" Value="10" />
    <Setter Property="Shades">
        <sh:ImmutableShades>
            <sh:Shade BlurRadius="10"
                      Opacity="1"
                      Offset="-10,-10"
                      Color="White" />
            <sh:Shade BlurRadius="10"
                      Opacity="1"
                      Offset="6, 6"
                      Color="#19000000" />
        </sh:ImmutableShades>
    </Setter>
</Style>
```

If you want to add `Neumorphism` to specific elements a `NeumorphismShades` markup extension will help you with that:

**XAML**

```xml
<sh:Shadows Grid.Row="1"
            Grid.Column="0"
            HorizontalOptions="Center"
            VerticalOptions="Center"
            CornerRadius="40"
            Shades="{sh:NeumorphismShades}">
    <ImageButton WidthRequest="60"
                 HeightRequest="60"
                 Padding="20"
                 BackgroundColor="#F0F0F3"
                 CornerRadius="30"
                 Source="{StaticResource IconPlusGray}" />
</sh:Shadows>

<sh:Shadows Grid.Row="1"
            Grid.Column="1"
            HorizontalOptions="Center"
            VerticalOptions="Center"
            CornerRadius="10"
            Shades="{sh:NeumorphismShades}">
    <Button Style="{StaticResource TextHeadline}"
            WidthRequest="120"
            HeightRequest="60"
            BackgroundColor="#F0F0F3"
            CornerRadius="10"
            Text="Reset"
            TextColor="Gray" />
</sh:Shadows>
```

**OUTPUT**

<img src="Docs/neumorphism.png" height="370"/>

### Be creative!

One last thing: all properties of a `Shade` are animatable.

You can achieve nice effects thinking outside the box!
Have a look at the `BeCreative.xaml` file and its code-behind.

![BeCreative](Docs/nyan_cat.gif)


### Immutable and mutable collections of shades

To have a better control of your shades, `Shadows` provides 2 kinds of `MarkupExtension`:

1. One immutable collection of shades: `ImmutableShades` (readonly type)
2. One mutable collection: `ShadeStack` (observable collection type)

Use the first one if the shade collection will not change and the second one if you want to dynamically add or remove shades.

*Dynamic shades starting with 0 shade.*
```xml
<sh:Shadows HorizontalOptions="Center" 
            VerticalOptions="Center"
            CornerRadius="10">
    <sh:Shadows.Shades>
        <sh:ShadeStack />
    </sh:Shadows.Shades>
    <Frame WidthRequest="80"
           Padding="10"
           BackgroundColor="White"
           CornerRadius="10">
        <Image Source="{images:ImageResource nyan_cat.png}" />
    </Frame>
</sh:Shadows>
```

#### SingleShade

You can also use the `SingleShade` markup extension if you just have one shadow.
It will remove some xaml elements:

```xml
<sh:Shadows CornerRadius="30"
            HorizontalOptions="Center"
            VerticalOptions="Center"
            Shades="{sh:SingleShade Offset='0, 10',
                                    Opacity=0.7,
                                    Color=Violet}">
    <ImageButton WidthRequest="60"
                 HeightRequest="60"
                 Padding="20"
                 BackgroundColor="Violet"
                 CornerRadius="30"
                 Source="{StaticResource IconPlusWhite}" />
</sh:Shadows>
```

## Android Blur Types

Starting with version 2.0.0, you can choose between two blur rendering algorithms on Android:

```xml
<sh:Shadows CornerRadius="10"
            AndroidBlurType="Gpu"
            Shades="{sh:SingleShade BlurRadius=10, Opacity=0.5, Offset='5,5', Color=Black}">
    <!-- Your content -->
</sh:Shadows>
```

### BlurType Options:

* **`Gpu`** (Default): Uses GPU-accelerated rendering with RenderScript/RenderEffect. Best performance for most cases.
* **`StackBlur`**: Pure CPU-based StackBlur algorithm. Use when GPU rendering has issues on specific devices.

## Performance

* On **Android**, shadows use GPU acceleration by default (`RenderScript`/`RenderEffect`). Bitmaps are cached in a global `BitmapCache`. For a particular color, size and blur, you will only have one instance alive. The new `BlurType` property lets you choose between GPU and CPU rendering.
* On **iOS** and **MacCatalyst**, a `Shade` is implemented with native `CALayer` shadows with hardware acceleration.
* On **Windows**, `Shade` is implemented with WinUI 3 `SpriteVisual` drop shadows using the Composition API.

## Legacy Xamarin.Forms Support

If you need Xamarin.Forms support, please use version 1.x of the package (`Sharpnado.Shadows`).

Version 2.0+ is exclusively for .NET MAUI.
