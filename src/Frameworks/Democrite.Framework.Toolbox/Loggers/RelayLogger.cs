// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Loggers
{
    using Microsoft.Extensions.Logging;

    using System;

    /// <summary>
    /// Simple logger used to relay to another logger
    /// </summary>
    /// <seealso cref="ILogger" />
    public sealed class RelayLogger : ILogger
    {
        #region Fields

        private readonly string _categoryName;
        private readonly ILogger _target;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayLogger"/> class.
        /// </summary>
        public RelayLogger(ILogger target, string categoryName)
        {
            this._target = target;
            this._categoryName = categoryName;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return this._target.BeginScope(state);
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return this._target.IsEnabled(logLevel);
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            this._target.Log<TState>(logLevel, eventId, state, exception, (s, e) => "[" + this._categoryName + "] " + formatter(s, e));
        }

        #endregion
    }
}
