using Microsoft.Extensions.Logging;
using Sharpnado.Shades;
using Sharpnado.Tabs;
using Sharpnado.MaterialFrame;

namespace ShadowsSample.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseSharpnadoShadows(
				enableLogging: true,
				enableDebugLogging: true,
				logFilter: "ShadowsHandler|ShadowView|BitmapCache")
			.UseSharpnadoTabs(loggerEnable: false)
			.UseSharpnadoMaterialFrame(loggerEnable: false)
			.ConfigureFonts(fonts =>
			{
				// OpenSans fonts
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Bold.ttf", "OpenSansBold");
				fonts.AddFont("OpenSans-SemiBold.ttf", "OpenSansSemibold");
				fonts.AddFont("OpenSans-ExtraBold.ttf", "OpenSansExtraBold");
				fonts.AddFont("OpenSans-Light.ttf", "OpenSansLight");
				fonts.AddFont("OpenSans-Italic.ttf", "OpenSansItalic");
				fonts.AddFont("OpenSans-BoldItalic.ttf", "OpenSansBoldItalic");
				
				// Karmatic Arcade font
				fonts.AddFont("ka1.ttf", "KarmaticArcade");
				
				// Font Awesome fonts (for icons)
				fonts.AddFont("Font-Awesome-5-Free-Solid-900.ttf", "FontAwesomeSolid");
				fonts.AddFont("Font-Awesome-5-Free-Regular-400.ttf", "FontAwesomeRegular");
				fonts.AddFont("Font-Awesome-5-Brands-Regular-400.otf", "FontAwesomeBrands");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
