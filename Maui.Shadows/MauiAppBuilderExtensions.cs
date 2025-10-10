namespace Sharpnado.Shades;

/// <summary>
/// Extension methods for configuring Sharpnado.Shadows in a MAUI application.
/// </summary>
public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Configures Sharpnado.Shadows for the MAUI application.
    /// </summary>
    /// <param name="builder">The MauiAppBuilder instance.</param>
    /// <param name="enableLogging">If false, nothing will be logged.</param>
    /// <param name="enableDebugLogging">If false, only debug level will not be logged.</param>
    /// <param name="loggerDelegate">You can add your own implementation of the logger (else the default one will be used).</param>
    /// <param name="logFilter">Separate tags you want to filter with pipe operator (e.g., "ShadowView|BitmapCache").</param>
    /// <returns>The MauiAppBuilder for chaining.</returns>
    public static MauiAppBuilder UseSharpnadoShadows(
        this MauiAppBuilder builder,
        bool enableLogging = false,
        bool enableDebugLogging = false,
        Action<string, string, string?>? loggerDelegate = null,
        string? logFilter = null)
    {
        InternalLogger.EnableLogging = enableLogging;
        InternalLogger.EnableDebug = enableDebugLogging;
        InternalLogger.LoggerDelegate = loggerDelegate;
        InternalLogger.SetFilter(logFilter);

        // Register platform-specific handlers
        builder.ConfigureMauiHandlers(handlers =>
        {
#if ANDROID
            handlers.AddHandler<Shadows, Platforms.Android.ShadowsHandler>();
#elif IOS || MACCATALYST
            handlers.AddHandler<Shadows, Platforms.iOS.ShadowsHandler>();
#elif WINDOWS
            handlers.AddHandler<Shadows, Platforms.Windows.ShadowsHandler>();
#endif
        });

        return builder;
    }
}
