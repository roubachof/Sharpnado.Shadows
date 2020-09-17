using System;

namespace Sharpnado.Shades
{
    public static class Initializer
    {
        /// <summary>
        /// Your logger implementation.
        /// </summary>
        /// <param name="logLevel">Could be: DBUG, INFO, WARN, ERRO.</param>
        /// <param name="message">The log message.</param>
        /// <param name="tag">A special tag for this message (for example the method name).</param>
        public delegate void LoggerDelegate(string logLevel, string message, string tag = null);

        /// <summary>
        /// User configuration for the internal logger.
        /// </summary>
        /// <param name="loggerEnable">If false, nothing will be logged.</param>
        /// <param name="debugLogEnable">If false, only debug level will not be logged.</param>
        /// <param name="loggerDelegate">You can add your own implementation of the logger (else the default one will be used).</param>
        /// <param name="filter"></param>
        public static void Initialize(bool loggerEnable, bool debugLogEnable = false, LoggerDelegate loggerDelegate = null, string filter = null)
        {
            InternalLogger.EnableDebug = debugLogEnable;
            InternalLogger.EnableLogging = loggerEnable;
            InternalLogger.LoggerDelegate = loggerDelegate;
            InternalLogger.SetFilter(filter);
        }
    }
}
