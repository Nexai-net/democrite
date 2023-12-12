// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Loggers
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Simple log container
    /// </summary>
    /// <remarks>
    ///     Attention the message template and values are merged used only this container for simple or in memory logs
    /// </remarks>
    public sealed class SimpleLog
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLog"/> class.
        /// </summary>
        public SimpleLog(LogLevel logLevel, string message)
        {
            this.LogLevel = logLevel;
            this.Message = message;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the log level.
        /// </summary>
        public LogLevel LogLevel { get; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public string Message { get; }

        #endregion
    }
}
