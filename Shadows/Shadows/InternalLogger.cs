using System;
using System.Runtime.CompilerServices;
using System.Text;

[assembly:InternalsVisibleTo("Sharpnado.Shadows.Android")]
[assembly:InternalsVisibleTo("Sharpnado.Shadows.iOS")]
[assembly:InternalsVisibleTo("Sharpnado.Shadows.UWP")]
[assembly:InternalsVisibleTo("Sharpnado.Shadows.Tizen")]
[assembly:InternalsVisibleTo("Sharpnado.Presentation.Forms")]

namespace Sharpnado.Shades
{
    internal static class InternalLogger
    {
        public const string DebugLevel = "DBUG";
        public const string InfoLevel = "INFO";
        public const string WarningLevel = "WARN";
        public const string ErrorLevel = "ERRO";

        public static Initializer.LoggerDelegate LoggerDelegate { get; set; }

        public static bool EnableLogging { get; set; } = false;

        public static bool EnableDebug { get; set; } = false;

        public static string[] Filters { get; private set; } = new string[0];

        /// <summary>
        /// Separate tags you want to filter with pipe operator.
        /// </summary>
        /// <example>SetFilter("ShadowView|BitmapCache")</example>
        /// <param name="filter"></param>
        public static void SetFilter(string filter)
        {
            Filters = filter == null ? new string[0] : filter.Split('|');
        }

        public static void Debug(string tag, Func<string> format)
        {
            if (!EnableDebug)
            {
                return;
            }

            DiagnosticLog(DebugLevel, format(), tag);
        }

        public static void Debug(string tag, string format)
        {
            if (!EnableDebug)
            {
                return;
            }

            DiagnosticLog(DebugLevel, format, tag);
        }

        public static void Debug(string format)
        {
            if (!EnableDebug)
            {
                return;
            }

            DiagnosticLog(DebugLevel, format);
        }

        public static void Info(string tag, string format)
        {
            DiagnosticLog(InfoLevel, format, tag);
        }

        public static void Info(string format)
        {
            DiagnosticLog(InfoLevel, format);
        }

        public static void Warn(string tag, string format)
        {
            DiagnosticLog(WarningLevel, format, tag);
        }

        public static void Warn(string format)
        {
            DiagnosticLog(WarningLevel, format);
        }

        public static void Error(string tag, string format)
        {
            DiagnosticLog(ErrorLevel, format, tag);
        }

        public static void Error(string format)
        {
            DiagnosticLog(ErrorLevel, format);
        }

        public static void Error(string tag, Exception exception)
        {
            Error($"{exception.Message}{Environment.NewLine}{exception}", tag);
        }

        public static void Error(string tag, string message, Exception exception)
        {
            Error($"{message}{Environment.NewLine}{exception}", tag);
        }

        public static void Error(Exception exception)
        {
            Error(null, exception);
        }

        private static void DiagnosticLog(string logLevel, string format, string tag = null)
        {
            if (!EnableLogging)
            {
                return;
            }

            if (tag != null && Filters.Length > 0)
            {
                bool found = false;
                foreach (var filter in Filters)
                {
                    if (found = tag.Contains(filter))
                    {
                        break;
                    }
                }

                if (!found)
                {
                    return;
                }
            }

            if (LoggerDelegate != null)
            {
                LoggerDelegate(logLevel, format, tag);
                return;
            }

            const string DateFormat = "MM-dd H:mm:ss.fff";
            const string Separator = " | ";
            const string SharpnadoInternals = nameof(SharpnadoInternals);

            var builder = new StringBuilder(DateTime.Now.ToString(DateFormat));
            builder.Append(Separator);
            builder.Append(SharpnadoInternals);
            builder.Append(Separator);
            builder.Append(logLevel);
            builder.Append(Separator);
            if (!string.IsNullOrWhiteSpace(tag))
            {
                builder.Append(tag);
                builder.Append(Separator);
            }

            builder.Append(format);

#if DEBUG
            System.Diagnostics.Debug.WriteLine(builder);
#else
            Console.WriteLine(builder);
#endif
        }
    }
}