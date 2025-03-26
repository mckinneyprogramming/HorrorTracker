﻿using HorrorTracker.Utilities.Logging.Interfaces;
using Serilog;
using Serilog.Core;
using System.Diagnostics.CodeAnalysis;

namespace HorrorTracker.Utilities.Logging
{
    /// <summary>
    /// The <see cref="LoggerService"/> class.
    /// </summary>
    /// <seealso cref="ILoggerService"/>
    [ExcludeFromCodeCoverage]
    public class LoggerService : ILoggerService
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly Logger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerService"/> class.
        /// </summary>
        public LoggerService()
        {
            _logger = new LoggerConfiguration().WriteTo.Seq(LoggerUrl).CreateLogger();
        }

        /// <inheritdoc/>
        public void CloseAndFlush()
        {
            Log.CloseAndFlush();
        }

        /// <inheritdoc/>
        public void LogError(string message, Exception exception)
        {
            _logger.Error(message, exception);
        }

        /// <inheritdoc/>
        public void LogInformation(string message)
        {
            _logger.Information(message);
        }

        /// <inheritdoc/>
        public void LogWarning(string message)
        {
            _logger.Warning(message);
        }

        /// <summary>
        /// Retrieves the logger url from the app settings.
        /// </summary>
        private static string? LoggerUrl => Environment.GetEnvironmentVariable("LoggerUrl");
    }
}