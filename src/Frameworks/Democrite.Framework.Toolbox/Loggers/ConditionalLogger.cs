// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Loggers
{
    using Microsoft.Extensions.Logging;

    using System;

    /// <summary>
    /// Define a proxy class used to enable/disabled logger base on an external condition like option
    /// </summary>
    /// <seealso cref="ILogger" />
    public class ConditionalLogger<TCondHolder> : ILogger
    {
        #region Fields

        private readonly Func<TCondHolder, bool> _conditionProvider;
        private readonly TCondHolder _condHolder;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalLogger"/> class.
        /// </summary>
        public ConditionalLogger(ILogger logger,
                                 TCondHolder condHolder,
                                 Func<TCondHolder, bool> conditionProvider)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(condHolder);
            ArgumentNullException.ThrowIfNull(conditionProvider);

            this._logger = logger;
            this._condHolder = condHolder;
            this._conditionProvider = conditionProvider;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return this._logger.BeginScope(state);
        }

        /// <inheritdoc />
        public virtual bool IsEnabled(LogLevel logLevel)
        {
            // Used to condition logging from an external condition
            if (this._conditionProvider(this._condHolder))
                return this._logger.IsEnabled(logLevel);
            return false;
        }

        /// <inheritdoc />
        public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            this._logger.Log<TState>(logLevel, eventId, state, exception, formatter);
        }

        #endregion
    }

    /// <inheritdoc cref="ConditionalLogger{TCategoryName}"/>
    public class ConditionalLogger<TCategoryName, TCondHolder> : ConditionalLogger<TCondHolder>, ILogger<TCategoryName>
    {

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalLogger{TCategoryName, TCondHolder}"/> class.
        /// </summary>
        public ConditionalLogger(ILogger<TCategoryName> logger,
                                 TCondHolder condHolder,
                                 Func<TCondHolder, bool> conditionProvider)
            : base(logger, condHolder, conditionProvider)
        {
        }

        #endregion
    }
}
